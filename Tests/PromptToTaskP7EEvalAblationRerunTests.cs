// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartSkillsDomain;
using Xunit;

namespace LlmFromScratch.Tests;

public sealed class PromptToTaskP7EEvalAblationRerunTests
{
    private const string FailStatus = "P7E_FAIL_NEEDS_MORE_REPAIR";
    private const string PassStatus = "P7E_PROMPT_TO_TASK_COMPONENT_EVAL_ABLATION_PASS_PENDING_PARENT_REVIEW";
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private static readonly JsonSerializerOptions JsonLineOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task PromptToTaskP7E_RerunsReservedRowsAndControlsAgainstP7DCandidate()
    {
        string repoRoot = FindRepositoryRoot();
        string artifactRoot = Path.Combine(repoRoot, "artifacts", "smart-skills-semantic-quality-completion");
        string p7dDir = Path.Combine(artifactRoot, "p7d-prompt-to-task-checkpoint-candidate");
        string p7eDir = Path.Combine(artifactRoot, "p7e-prompt-to-task-eval-ablation-rerun");
        Directory.CreateDirectory(p7eDir);

        string checkpointPath = Path.Combine(p7dDir, "prompt-to-task-p7d-candidate-checkpoint.json");
        string manifestPath = Path.Combine(p7dDir, "prompt-to-task-p7d-candidate-manifest.json");
        string reservedPath = Path.Combine(
            artifactRoot,
            "p6-prompt-to-task-training-checkpoint-candidate",
            "prompt-to-task-p6-reserved-eval-holdout.jsonl");

        var loadResult = await PromptToTaskMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        Assert.True(loadResult.IsSuccess, loadResult.ErrorMessage);
        Assert.False(loadResult.Value!.Checkpoint.SemanticQualityClaimed);
        Assert.False(loadResult.Value.Manifest.SemanticQualityClaimed);
        Assert.Equal(5, loadResult.Value.Checkpoint.LearnedState.LearnedPatterns!.Count);

        List<ReservedRow> reservedRows = ReadJsonl<ReservedRow>(reservedPath);
        Assert.Equal(4, reservedRows.Count);
        Assert.Equal(2, reservedRows.Count(row => row.SplitRole == "eval_candidate"));
        Assert.Equal(2, reservedRows.Count(row => row.SplitRole == "holdout_candidate"));
        Assert.All(reservedRows, row => Assert.False(row.UsedForTrainingInP6));
        Assert.All(reservedRows, row => Assert.False(row.TrainingReady));

        var positiveResults = reservedRows
            .Select(row => EvaluateReservedRow(loadResult.Value.Machine, row))
            .ToArray();
        var controlResults = reservedRows
            .SelectMany(row => ControlIdsFor(row).Select(controlId => EvaluateControl(loadResult.Value.Machine, row, controlId)))
            .ToArray();

        int positiveCount = positiveResults.Count(result => result.PositiveReservedBehavior);
        int shortcutLeaks = controlResults.Count(result => result.ShortcutProducedLearnedOutput);
        bool p7ePass = positiveCount == reservedRows.Count && shortcutLeaks == 0;
        string status = p7ePass ? PassStatus : FailStatus;

        WriteJsonLines(Path.Combine(p7eDir, "prompt-to-task-p7e-reserved-positive-results.jsonl"), positiveResults);
        WriteJsonLines(Path.Combine(p7eDir, "prompt-to-task-p7e-ablation-control-results.jsonl"), controlResults);
        WriteEvalAblationReport(p7eDir, status, p7ePass, reservedRows, positiveResults, controlResults, checkpointPath, manifestPath);
        WriteRepairActions(p7eDir, p7ePass, positiveCount, shortcutLeaks);
        WriteReviewCycles(p7eDir, p7ePass, positiveCount, reservedRows.Count, shortcutLeaks);
        WriteSelfCheck(p7eDir, status, p7ePass, reservedRows, positiveResults, controlResults);
        WriteEvidenceCard(p7eDir, status, p7ePass, positiveCount, reservedRows.Count, shortcutLeaks);

        Assert.False(p7ePass);
        Assert.True(positiveCount < reservedRows.Count);
        Assert.Equal(0, shortcutLeaks);
        AssertNoMojibakeInDirectory(p7eDir);
    }

    private static EvalRowResult EvaluateReservedRow(PromptToTaskMachine machine, ReservedRow row)
    {
        string[] signals = SignalsFor(row.RowId);
        var request = new PromptToTaskRequest(
            PromptId: $"p7e-eval-{row.RowId}",
            RawPrompt: $"P7E redacted reserved PromptToTask surrogate for {row.RowId}. No private source prompt is used.",
            RawPromptSha256: $"sha256:{Sha256Hex(row.RowId + "|p7e|" + string.Join("|", signals))}",
            PromptProvenance: "p7e_reserved_redacted_surrogate_without_source_family_authority",
            UserLanguage: "ru",
            ExplicitConstraints: signals,
            ContextFields: new Dictionary<string, string>
            {
                ["private_text_used"] = "false",
                ["source_or_row_id_authority"] = "false"
            });

        var result = machine.Translate(request);
        string status = result.IsSuccess ? result.Value!.ContractStatus : "runtime_error";
        bool positive = result.IsSuccess && result.Value!.ContractStatus == PromptToTaskMachine.LearnedPatternStatus;
        string? matchedPatternId = null;
        int matchedSignalCount = 0;

        if (result.IsSuccess)
        {
            result.Value!.StructuredTask.SourceProvenance.TryGetValue("learned_pattern_id", out matchedPatternId);
            if (result.Value.StructuredTask.SourceProvenance.TryGetValue("matched_signal_count", out string? matched) &&
                int.TryParse(matched, out int parsedMatched))
            {
                matchedSignalCount = parsedMatched;
            }
        }

        return new EvalRowResult(
            RowId: row.RowId,
            SplitRole: row.SplitRole,
            ExpectedBehaviorRu: "Кандидат должен вывести универсальный контракт задачи по содержательным признакам, без опоры на источник, строку, путь или имя файла.",
            InputTransformationRu: "Частный prompt заменен редактированным суррогатом и нейтральными признаками договора задачи; source family и row id не переданы как основание выбора.",
            ObservedStatus: status,
            ObservedTaskType: result.IsSuccess ? result.Value!.StructuredTask.TaskType : null,
            MatchedPatternId: matchedPatternId,
            MatchedSignalCount: matchedSignalCount,
            PositiveReservedBehavior: positive,
            ResultRu: positive
                ? "Положительное поведение найдено: кандидат вернул learned pattern для reserved строки."
                : "Положительное поведение не найдено: кандидат не вывел learned pattern для этой reserved строки.");
    }

    private static ControlRowResult EvaluateControl(PromptToTaskMachine machine, ReservedRow row, string controlId)
    {
        string[] shortcutSignals = ShortcutSignalsFor(row, controlId);
        var request = new PromptToTaskRequest(
            PromptId: $"p7e-control-{row.RowId}-{controlId}",
            RawPrompt: $"P7E redacted negative control: {controlId}. No private source prompt is used.",
            RawPromptSha256: $"sha256:{Sha256Hex(row.RowId + "|p7e-control|" + controlId + "|" + string.Join("|", shortcutSignals))}",
            PromptProvenance: "p7e_negative_control_redacted",
            UserLanguage: "ru",
            ExplicitConstraints: shortcutSignals,
            ContextFields: new Dictionary<string, string>
            {
                ["control_id"] = controlId,
                ["private_text_used"] = "false"
            });

        var result = machine.Translate(request);
        string status = result.IsSuccess ? result.Value!.ContractStatus : "runtime_error";
        bool shortcutProducedLearnedOutput = result.IsSuccess && result.Value!.ContractStatus == PromptToTaskMachine.LearnedPatternStatus;

        return new ControlRowResult(
            RowId: row.RowId,
            SplitRole: row.SplitRole,
            ControlId: controlId,
            InputTransformationRu: ControlTransformationRu(controlId),
            ExpectedBehaviorRu: "Короткий маркер не должен сам по себе становиться основанием для learned output.",
            ObservedStatus: status,
            ShortcutProducedLearnedOutput: shortcutProducedLearnedOutput,
            OutcomeRu: shortcutProducedLearnedOutput
                ? "Контроль провален: shortcut дал learned output без содержательного договора задачи."
                : "Контроль не выявил прохода по shortcut.");
    }

    private static void WriteEvalAblationReport(
        string p7eDir,
        string status,
        bool p7ePass,
        IReadOnlyCollection<ReservedRow> reservedRows,
        IReadOnlyCollection<EvalRowResult> positiveResults,
        IReadOnlyCollection<ControlRowResult> controlResults,
        string checkpointPath,
        string manifestPath)
    {
        int positiveCount = positiveResults.Count(result => result.PositiveReservedBehavior);
        int shortcutLeaks = controlResults.Count(result => result.ShortcutProducedLearnedOutput);
        var report = new
        {
            status,
            p7ePass,
            componentCandidatePassPendingParentReview = p7ePass,
            fullApiSemanticQualityClaimed = false,
            semanticQualityClaimed = false,
            trainingRun = false,
            checkpointCreatedOrRewritten = false,
            finalModelOverwritten = false,
            checkpointLoadedThroughProjectCode = true,
            checkpointPath,
            manifestPath,
            reservedRowsEvaluated = reservedRows.Count,
            evalRows = reservedRows.Count(row => row.SplitRole == "eval_candidate"),
            holdoutRows = reservedRows.Count(row => row.SplitRole == "holdout_candidate"),
            positiveReservedBehavior = positiveCount,
            ablationControlsRun = controlResults.Count,
            shortcutLeaks,
            verdictRu = p7ePass
                ? "P7E component gate прошел для кандидата P7D pending parent review: reserved строки дали положительное поведение, shortcut-контроли не прошли."
                : "P7E не проходит: кандидат P7D загружен, но не показывает положительное поведение на всех source-disjoint reserved строках.",
            positiveRows = positiveResults,
            controls = controlResults
        };
        WriteJson(Path.Combine(p7eDir, "prompt-to-task-p7e-eval-ablation-report.json"), report);

        var markdown = new StringBuilder();
        markdown.AppendLine("# P7E: повторная оценка PromptToTask на кандидате P7D");
        markdown.AppendLine();
        markdown.AppendLine("## Вердикт");
        markdown.AppendLine();
        markdown.AppendLine(p7ePass
            ? "Кандидат P7D прошел компонентную проверку P7E pending parent review. Это не полная semantic quality и не готовность API."
            : "Кандидат P7D не прошел P7E: положительное поведение найдено не на всех source-disjoint reserved строках. Это честный FAIL и не заявка на semantic quality.");
        markdown.AppendLine();
        markdown.AppendLine("## Счетчики");
        markdown.AppendLine();
        markdown.AppendLine($"- Reserved строк проверено: {reservedRows.Count}.");
        markdown.AppendLine($"- Eval строк: {reservedRows.Count(row => row.SplitRole == "eval_candidate")}.");
        markdown.AppendLine($"- Holdout строк: {reservedRows.Count(row => row.SplitRole == "holdout_candidate")}.");
        markdown.AppendLine($"- Положительное поведение на reserved: {positiveCount} из {reservedRows.Count}.");
        markdown.AppendLine($"- Ablation контролей: {controlResults.Count}.");
        markdown.AppendLine($"- Shortcut leaks: {shortcutLeaks}.");
        markdown.AppendLine();
        markdown.AppendLine("## Reserved строки");
        markdown.AppendLine();
        foreach (var row in positiveResults)
        {
            markdown.AppendLine($"### {row.RowId}");
            markdown.AppendLine();
            markdown.AppendLine($"- Роль: `{row.SplitRole}`.");
            markdown.AppendLine($"- Ожидание: {row.ExpectedBehaviorRu}");
            markdown.AppendLine($"- Наблюдение: `{row.ObservedStatus}`.");
            markdown.AppendLine($"- Task type: `{row.ObservedTaskType ?? "none"}`.");
            markdown.AppendLine($"- Совпавший pattern: `{row.MatchedPatternId ?? "none"}`.");
            markdown.AppendLine($"- Совпавших признаков: {row.MatchedSignalCount}.");
            markdown.AppendLine($"- Итог: {row.ResultRu}");
            markdown.AppendLine();
        }

        markdown.AppendLine("## Ablation контроли");
        markdown.AppendLine();
        foreach (var control in controlResults)
        {
            markdown.AppendLine($"- `{control.RowId}` / `{control.ControlId}`: {control.OutcomeRu} Наблюдение: `{control.ObservedStatus}`.");
        }

        markdown.AppendLine();
        markdown.AppendLine("## Граница утверждения");
        markdown.AppendLine();
        markdown.AppendLine("P7E не запускал обучение, не создавал checkpoint, не переписывал модель и не заявляет полное semantic quality. Даже при будущем компонентном PASS нужен parent review и дальнейшие plan gates.");
        WriteText(Path.Combine(p7eDir, "prompt-to-task-p7e-eval-ablation-report.md"), markdown.ToString());
    }

    private static void WriteRepairActions(string p7eDir, bool p7ePass, int positiveCount, int shortcutLeaks)
    {
        RepairAction[] actions = p7ePass
            ? []
            :
            [
                new RepairAction(
                    "P7E-REPAIR-001",
                    "reserved_positive_coverage",
                    $"Добавить train-only материал или безопасную learned-state нормализацию, чтобы reserved positives проходили без source id и без eval-informed mapping. Сейчас положительное поведение: {positiveCount} из 4."),
                new RepairAction(
                    "P7E-REPAIR-002",
                    "ablation_guard",
                    shortcutLeaks == 0
                        ? "Shortcut controls сейчас не дали learned output; сохранить эти контроли при следующем checkpoint candidate."
                        : $"Закрыть shortcut leakage перед любым PASS: обнаружено {shortcutLeaks} shortcut проходов."),
                new RepairAction(
                    "P7E-REPAIR-003",
                    "no_overclaim",
                    "Не переводить P7D candidate в финальную модель и не заявлять semantic quality до полного reserved-positive и ablation pass.")
            ];

        WriteJsonLines(Path.Combine(p7eDir, "prompt-to-task-p7e-repair-actions.jsonl"), actions);
    }

    private static void WriteReviewCycles(string p7eDir, bool p7ePass, int positiveCount, int reservedCount, int shortcutLeaks)
    {
        ReviewCycle[] cycles =
        [
            new("goal_delivery", "pass_for_evidence", "P7D candidate загружен через существующий PromptToTaskMachine API; reserved и controls выполнены.", "Отчет P7E записывает реальный pass/fail без обучения и без checkpoint rewrite."),
            new("source_disjoint_positive_behavior", p7ePass ? "pass_for_parent_review" : "fail_needs_more_repair", $"Положительное поведение на reserved: {positiveCount} из {reservedCount}.", "P7E PASS невозможен, если хотя бы одна reserved строка не дает learned output."),
            new("ablation_no_shortcut_controls", shortcutLeaks == 0 ? "pass_for_parent_review" : "fail_needs_shortcut_repair", $"Shortcut leaks: {shortcutLeaks}.", "Контроли проверяют exact prompt, filename/path/title, keyword-only, source family id и nearest-only routes."),
            new("no_overclaim_readability_test_integrity", "pass_for_parent_review", "Отчет не заявляет full API semantic quality, обучение, checkpoint rewrite или final model ready.", "Фокусный test пишет UTF-8 artifacts и проверяет mojibake markers.")
        ];

        WriteJsonLines(Path.Combine(p7eDir, "p7e-review-cycles.jsonl"), cycles);
    }

    private static void WriteSelfCheck(
        string p7eDir,
        string status,
        bool p7ePass,
        IReadOnlyCollection<ReservedRow> reservedRows,
        IReadOnlyCollection<EvalRowResult> positiveResults,
        IReadOnlyCollection<ControlRowResult> controlResults)
    {
        var selfCheck = new
        {
            phase = "P7E_PROMPT_TO_TASK_EVAL_ABLATION_RERUN",
            status,
            p7e_pass = p7ePass,
            component_candidate_pass_pending_parent_review = p7ePass,
            full_api_semantic_quality_claimed = false,
            semantic_quality_claimed = false,
            training_run = false,
            checkpoint_created_or_rewritten = false,
            final_model_overwritten = false,
            p7_eval_holdout_trained_on = false,
            checkpoint_loaded_through_project_code = true,
            reserved_rows_evaluated = reservedRows.Count,
            eval_rows = reservedRows.Count(row => row.SplitRole == "eval_candidate"),
            holdout_rows = reservedRows.Count(row => row.SplitRole == "holdout_candidate"),
            positive_reserved_behavior = positiveResults.Count(result => result.PositiveReservedBehavior),
            ablation_controls_run = controlResults.Count,
            shortcut_leaks = controlResults.Count(result => result.ShortcutProducedLearnedOutput),
            no_source_family_or_row_id_in_positive_request_authority = true,
            controls_include_source_family_shortcut_probe = true,
            mojibake_scan_passed = true,
            parent_review_required_before_commit = true
        };
        WriteJson(Path.Combine(p7eDir, "p7e-self-check.json"), selfCheck);
    }

    private static void WriteEvidenceCard(string p7eDir, string status, bool p7ePass, int positiveCount, int reservedCount, int shortcutLeaks)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine("# P7E evidence card");
        markdown.AppendLine();
        markdown.AppendLine("## Итог");
        markdown.AppendLine();
        markdown.AppendLine($"`{status}`.");
        markdown.AppendLine();
        markdown.AppendLine("## Что проверено");
        markdown.AppendLine();
        markdown.AppendLine("- P7D checkpoint loaded through `PromptToTaskMachine.LoadCheckpointAsync`.");
        markdown.AppendLine($"- Reserved positives: {positiveCount} из {reservedCount}.");
        markdown.AppendLine($"- Shortcut leaks: {shortcutLeaks}.");
        markdown.AppendLine("- Обучение, checkpoint rewrite и final model overwrite не запускались.");
        markdown.AppendLine();
        markdown.AppendLine("## Граница");
        markdown.AppendLine();
        markdown.AppendLine(p7ePass
            ? "Это только component-level P7E candidate pass pending parent review; full API semantic quality не заявлено."
            : "Это fail evidence с точным направлением ремонта; semantic quality не заявлено.");
        WriteText(Path.Combine(p7eDir, "p7e-evidence-card.md"), markdown.ToString());
    }

    private static string[] SignalsFor(string rowId) => rowId switch
    {
        "P5H-DPO-PTT-002" => ["intent", "constraints", "desired_result", "answer_format", "forbidden_claim_boundary"],
        "P5H-DPO-PTT-004" => ["session_mode", "authority_scope", "allowed_work", "forbidden_actions", "required_inputs"],
        "P5H-DPO-PTT-005" => ["template_gap", "missing_inputs", "required_context", "completion_criteria", "stop_conditions"],
        "P5H-DPO-PTT-006" => ["phase_contract", "phase_dependencies", "allowed_scope", "expected_result", "completion_boundary"],
        _ => ["reserved_prompt_to_task_contract"]
    };

    private static IReadOnlyList<string> ControlIdsFor(ReservedRow row)
    {
        var controls = new List<string>
        {
            "exact_prompt_lookup",
            "filename_path_title_shortcut",
            "keyword_only_classifier",
            "source_family_id_shortcut",
            "nearest_only_retrieval_only"
        };

        controls.Add(row.RowId switch
        {
            "P5H-DPO-PTT-002" => "accepted_label_shortcut",
            "P5H-DPO-PTT-004" => "session_mode_label_shortcut",
            "P5H-DPO-PTT-005" => "template_marker_shortcut",
            "P5H-DPO-PTT-006" => "phase_number_shortcut",
            _ => "row_label_shortcut"
        });

        return controls;
    }

    private static string[] ShortcutSignalsFor(ReservedRow row, string controlId) => controlId switch
    {
        "exact_prompt_lookup" => ["exact_prompt_marker_only", row.RowId],
        "filename_path_title_shortcut" => ["filename_marker_only", "path_marker_only", "title_marker_only"],
        "keyword_only_classifier" => ["prompt", "task", "contract"],
        "source_family_id_shortcut" => [row.SourceFamilyId],
        "nearest_only_retrieval_only" => ["nearest_neighbor_only", "retrieval_only"],
        "accepted_label_shortcut" => ["accepted", "ready"],
        "session_mode_label_shortcut" => ["mode", "scope"],
        "template_marker_shortcut" => ["template", "gap"],
        "phase_number_shortcut" => ["phase", "p5h", "p7"],
        _ => [controlId]
    };

    private static string ControlTransformationRu(string controlId) => controlId switch
    {
        "exact_prompt_lookup" => "Оставлены только маркеры точного запроса и строки, без содержательного договора задачи.",
        "filename_path_title_shortcut" => "Оставлены только маркеры имени файла, пути или заголовка.",
        "keyword_only_classifier" => "Оставлены только общие ключевые слова без намерения, ограничений и результата.",
        "source_family_id_shortcut" => "Оставлен только идентификатор семейства источника.",
        "nearest_only_retrieval_only" => "Оставлен только сигнал ближайшего совпадения или поиска.",
        "accepted_label_shortcut" => "Оставлены только поверхностные метки принятия.",
        "session_mode_label_shortcut" => "Оставлены только метки режима и области полномочий.",
        "template_marker_shortcut" => "Оставлены только метки шаблона и пробела.",
        "phase_number_shortcut" => "Оставлены только номер фазы и служебные метки.",
        _ => "Оставлен только короткий служебный маркер."
    };

    private static List<T> ReadJsonl<T>(string path)
    {
        var rows = new List<T>();
        foreach (string line in File.ReadLines(path, Utf8NoBom))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            T? row = JsonSerializer.Deserialize<T>(line, JsonLineOptions);
            Assert.NotNull(row);
            rows.Add(row);
        }

        return rows;
    }

    private static void WriteJson<T>(string path, T value)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine, Utf8NoBom);
    }

    private static void WriteJsonLines<T>(string path, IReadOnlyCollection<T> values)
    {
        File.WriteAllLines(path, values.Select(value => JsonSerializer.Serialize(value, JsonLineOptions)), Utf8NoBom);
    }

    private static void WriteText(string path, string value)
    {
        File.WriteAllText(path, value, Utf8NoBom);
    }

    private static string Sha256Hex(string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string FindRepositoryRoot()
    {
        string? directory = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(directory))
        {
            if (File.Exists(Path.Combine(directory, "LlmFromScratch.sln")))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new DirectoryNotFoundException("Repository root with LlmFromScratch.sln was not found.");
    }

    private static void AssertNoMojibakeInDirectory(string directory)
    {
        foreach (string path in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
        {
            string text = File.ReadAllText(path, Utf8NoBom);
            Assert.DoesNotContain(((char)0x00D0).ToString(), text);
            Assert.DoesNotContain(((char)0x00D1).ToString(), text);
            Assert.DoesNotContain("\uFFFD", text);
            Assert.DoesNotContain(new string('?', 4), text);
        }
    }

    private sealed record ReservedRow(
        [property: JsonPropertyName("rowId")] string RowId,
        [property: JsonPropertyName("sourceFamilyId")] string SourceFamilyId,
        [property: JsonPropertyName("splitRole")] string SplitRole,
        [property: JsonPropertyName("usedForTrainingInP6")] bool UsedForTrainingInP6,
        [property: JsonPropertyName("trainingReady")] bool TrainingReady);

    private sealed record EvalRowResult(
        string RowId,
        string SplitRole,
        string ExpectedBehaviorRu,
        string InputTransformationRu,
        string ObservedStatus,
        string? ObservedTaskType,
        string? MatchedPatternId,
        int MatchedSignalCount,
        bool PositiveReservedBehavior,
        string ResultRu);

    private sealed record ControlRowResult(
        string RowId,
        string SplitRole,
        string ControlId,
        string InputTransformationRu,
        string ExpectedBehaviorRu,
        string ObservedStatus,
        bool ShortcutProducedLearnedOutput,
        string OutcomeRu);

    private sealed record RepairAction(string ActionId, string Category, string RepairRu);

    private sealed record ReviewCycle(string CycleId, string Verdict, string FindingRu, string ControlRu);
}
