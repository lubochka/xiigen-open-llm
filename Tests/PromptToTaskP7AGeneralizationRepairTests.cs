// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using SmartSkillsDomain;
using Xunit;

namespace LlmFromScratch.Tests;

public sealed class PromptToTaskP7AGeneralizationRepairTests
{
    private const string PhaseStatus = "FAIL_NEEDS_TRAINING_COVERAGE";
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private static readonly JsonSerializerOptions JsonLineOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task PromptToTaskP7AParentRepair_RecordsTrainingCoverageGapAfterRemovingEvalInformedCode()
    {
        string repoRoot = FindRepositoryRoot();
        string artifactRoot = Path.Combine(repoRoot, "artifacts", "smart-skills-semantic-quality-completion");
        string p6Dir = Path.Combine(artifactRoot, "p6-prompt-to-task-training-checkpoint-candidate");
        string p7aDir = Path.Combine(artifactRoot, "p7a-prompt-to-task-generalization-repair");
        Directory.CreateDirectory(p7aDir);

        string checkpointPath = Path.Combine(p6Dir, "prompt-to-task-p6-candidate-checkpoint.json");
        string manifestPath = Path.Combine(p6Dir, "prompt-to-task-p6-candidate-manifest.json");
        string reservedPath = Path.Combine(p6Dir, "prompt-to-task-p6-reserved-eval-holdout.jsonl");

        var loadResult = await PromptToTaskMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        Assert.True(loadResult.IsSuccess, loadResult.ErrorMessage);

        List<P6ReservedRow> reservedRows = ReadJsonl<P6ReservedRow>(reservedPath);
        Assert.Equal(4, reservedRows.Count);
        Assert.All(reservedRows, row => Assert.False(row.UsedForTrainingInP6));
        Assert.All(reservedRows, row => Assert.False(row.TrainingReady));

        var positiveResults = reservedRows
            .Select(row => EvaluateReservedRow(loadResult.Value!.Machine, row))
            .ToArray();
        var controlResults = reservedRows
            .SelectMany(row => ControlIdsFor(row).Select(control => EvaluateControl(loadResult.Value!.Machine, row, control)))
            .ToArray();

        int positiveCount = positiveResults.Count(result => result.PositiveReservedBehavior);
        int shortcutLeaks = controlResults.Count(result => result.ShortcutProducedLearnedOutput);

        Assert.Equal(0, positiveCount);
        Assert.Equal(0, shortcutLeaks);

        WriteRepairReport(p7aDir, positiveResults, controlResults);
        WriteReviewCycles(p7aDir);
        AssertNoMojibakeInDirectory(p7aDir);
    }

    private static P7AProbeResult EvaluateReservedRow(PromptToTaskMachine machine, P6ReservedRow row)
    {
        string[] signals = SignalsFor(row.RowId);
        var request = new PromptToTaskRequest(
            PromptId: $"p7a-eval-{row.RowId}",
            RawPrompt: $"Редактированный проверочный запрос P7A для {row.RowId}. Частный исходный текст не используется.",
            RawPromptSha256: $"sha256:{Sha256Hex(row.RowId + "|" + string.Join("|", signals))}",
            PromptProvenance: "p7a_reserved_redacted_surrogate",
            UserLanguage: "ru",
            ExplicitConstraints: signals,
            ContextFields: new Dictionary<string, string>
            {
                ["p7a_split_role"] = row.SplitRole,
                ["private_text_used"] = "false"
            });

        OperationResult<PromptToTaskResponse> result = machine.Translate(request);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        string? matchedPatternId = result.Value!.StructuredTask.SourceProvenance.TryGetValue("learned_pattern_id", out string? patternId)
            ? patternId
            : null;
        int matchedSignalCount = result.Value.StructuredTask.SourceProvenance.TryGetValue("matched_signal_count", out string? matched) && int.TryParse(matched, out int count)
            ? count
            : 0;

        return new P7AProbeResult(
            RowId: row.RowId,
            SplitRole: row.SplitRole,
            ProbeKind: "reserved_positive",
            ObservedStatus: result.Value.ContractStatus,
            ObservedTaskType: result.Value.StructuredTask.TaskType,
            MatchedPatternId: matchedPatternId,
            MatchedSignalCount: matchedSignalCount,
            PositiveReservedBehavior: result.Value.ContractStatus == PromptToTaskMachine.LearnedPatternStatus,
            ShortcutProducedLearnedOutput: false,
            SummaryRu: "Зарезервированная строка не получила обученный baseline-ответ после удаления eval-informed маппинга. Это честный сигнал нехватки train-only покрытия, а не runtime PASS.");
    }

    private static P7AProbeResult EvaluateControl(PromptToTaskMachine machine, P6ReservedRow row, string controlId)
    {
        string[] signals = ShortcutSignalsFor(row, controlId);
        var request = new PromptToTaskRequest(
            PromptId: $"p7a-control-{row.RowId}-{controlId}",
            RawPrompt: $"Редактированный отрицательный контроль P7A: {controlId}. Частный исходный текст не используется.",
            RawPromptSha256: $"sha256:{Sha256Hex(row.RowId + "|" + controlId + "|" + string.Join("|", signals))}",
            PromptProvenance: "p7a_negative_control_redacted",
            UserLanguage: "ru",
            ExplicitConstraints: signals,
            ContextFields: new Dictionary<string, string>
            {
                ["control_id"] = controlId,
                ["private_text_used"] = "false"
            });

        OperationResult<PromptToTaskResponse> result = machine.Translate(request);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        bool shortcutProducedLearnedOutput = result.Value!.ContractStatus == PromptToTaskMachine.LearnedPatternStatus;

        return new P7AProbeResult(
            RowId: row.RowId,
            SplitRole: row.SplitRole,
            ProbeKind: controlId,
            ObservedStatus: result.Value.ContractStatus,
            ObservedTaskType: result.Value.StructuredTask.TaskType,
            MatchedPatternId: null,
            MatchedSignalCount: 0,
            PositiveReservedBehavior: false,
            ShortcutProducedLearnedOutput: shortcutProducedLearnedOutput,
            SummaryRu: shortcutProducedLearnedOutput
                ? "Контроль провален: короткий маркер дал обученный ответ."
                : "Контроль не прошел по короткому маркеру.");
    }

    private static void WriteRepairReport(
        string p7aDir,
        IReadOnlyCollection<P7AProbeResult> positiveResults,
        IReadOnlyCollection<P7AProbeResult> controlResults)
    {
        int positiveCount = positiveResults.Count(result => result.PositiveReservedBehavior);
        int shortcutLeaks = controlResults.Count(result => result.ShortcutProducedLearnedOutput);
        var report = new
        {
            status = PhaseStatus,
            verdict = PhaseStatus,
            parentReviewFinding = "Parent review rejected the previous P7A repair because it added production mappings for exact P7 reserved/eval/holdout signal combinations.",
            fixApplied = "Removed the derived production mappings from PromptToTaskTaskPatternUnit and changed P7A evidence to an honest train-only coverage gap.",
            diagnosis = "P7 still fails because the P6 checkpoint has only two training-approved rows and literal learned signal coverage is too narrow for source-disjoint reserved rows.",
            safeAlternative = "Future repair must add train-only source-disjoint PromptToTask rows and rewrite a candidate checkpoint, or introduce a common taxonomy that predates P7 and is not derived from reserved rows.",
            codeRepairRetained = false,
            trainingCoverageRequired = true,
            checkpointCreated = false,
            checkpointOverwritten = false,
            finalModelOverwritten = false,
            trainingRun = false,
            trainedOnEvalOrHoldoutRows = false,
            semanticQualityClaimed = false,
            p7PassClaimed = false,
            reservedRowsEvaluated = positiveResults.Count,
            positiveReservedBehavior = positiveCount,
            ablationControlsRun = controlResults.Count,
            shortcutLeaks,
            remainingBlockedRu = "Нужна следующая ограниченная фаза для добавления train-only покрытия или заранее задокументированной общей таксономии. P7 PASS и semantic quality не заявлены.",
            positiveRows = positiveResults,
            controls = controlResults
        };
        WriteJson(Path.Combine(p7aDir, "p7a-repair-report.json"), report);

        var markdown = new StringBuilder();
        markdown.AppendLine("# P7A: ремонт после parent review");
        markdown.AppendLine();
        markdown.AppendLine("## Вердикт");
        markdown.AppendLine();
        markdown.AppendLine($"`{PhaseStatus}`. Предыдущий P7A PASS-кандидат отклонен: он делал reserved/eval/holdout строки успешными через кодовый маппинг, подсмотренный из P7 сигналов. Этот маппинг удален.");
        markdown.AppendLine();
        markdown.AppendLine("## Что нашел parent review");
        markdown.AppendLine();
        markdown.AppendLine("Ремонт добавлял в production code точные сочетания сигналов, которые встречались в P7 reserved/eval/holdout строках. Это не было прямым обучением на eval rows, но было eval-informed code repair: deterministic fixture fitting вместо честного обобщения.");
        markdown.AppendLine();
        markdown.AppendLine("## Что исправлено");
        markdown.AppendLine();
        markdown.AppendLine("Производственный маппинг удален из `PromptToTaskTaskPatternUnit`. Runtime снова не использует P7 row id, source family, путь/имя файла, exact prompt, либо точные reserved signal combinations как authority. P7A больше не заявляет PASS.");
        markdown.AppendLine();
        markdown.AppendLine("## Текущий результат");
        markdown.AppendLine();
        markdown.AppendLine($"- Зарезервированные строки с положительным runtime-поведением: {positiveCount}/{positiveResults.Count}");
        markdown.AppendLine($"- Отрицательные контроли, прошедшие через короткий маркер: {shortcutLeaks}/{controlResults.Count}");
        markdown.AppendLine("- Новый checkpoint не создавался; P6 checkpoint только загружался.");
        markdown.AppendLine("- Eval/holdout строки не использовались для обучения.");
        markdown.AppendLine("- Semantic quality и P7 PASS не заявлены.");
        markdown.AppendLine();
        markdown.AppendLine("## Почему это честный FAIL");
        markdown.AppendLine();
        markdown.AppendLine("P6 candidate checkpoint содержит только две training-approved строки. Этого покрытия недостаточно, чтобы source-disjoint reserved строки с другими безопасными формулировками дали обученный результат без подсказок из P7. Без train-only расширения или заранее утвержденной общей таксономии runtime должен оставаться осторожным.");
        markdown.AppendLine();
        markdown.AppendLine("## Следующее действие");
        markdown.AppendLine();
        markdown.AppendLine("Нужна отдельная bounded phase: добавить новые PromptToTask training-only source-disjoint строки или подготовить общую таксономию, источник которой документирован до P7, затем создать новый checkpoint candidate и заново пройти P7 eval/ablation gate.");
        WriteText(Path.Combine(p7aDir, "p7a-repair-report.md"), markdown.ToString());
    }

    private static void WriteReviewCycles(string p7aDir)
    {
        var cycles = new[]
        {
            new ReviewCycle(
                "parent_review_cycle_1_eval_informed_code_leakage",
                "Parent review указал, что P7A PASS был получен через production-маппинг точных reserved/eval/holdout signal combinations.",
                "Маппинг удален, verdict изменен на FAIL_NEEDS_TRAINING_COVERAGE, P7 PASS больше не заявлен.",
                true),
            new ReviewCycle(
                "cycle_2_leakage_no_shortcut",
                "Проверены короткие маркеры: row id, source family, filename/path/title, exact prompt, keyword-only и retrieval-only.",
                "24 отрицательных контроля не дали обученный output; shortcut authority не добавлен.",
                true),
            new ReviewCycle(
                "cycle_3_trainable_architecture",
                "Проверена trainable boundary после удаления unsafe code repair.",
                "Оставлен честный data coverage gap: будущий ремонт должен идти через training-approved rows или заранее документированную common taxonomy.",
                true),
            new ReviewCycle(
                "cycle_4_verification_docs_no_claim",
                "Проверены артефакты, документация и claim boundary.",
                "JSON/JSONL parse, focused test, mojibake scan, overclaim scan и git diff --check должны пройти без semantic-quality или P7 PASS claim.",
                true)
        };
        string path = Path.Combine(p7aDir, "p7a-review-cycles.jsonl");
        File.WriteAllLines(path, cycles.Select(cycle => JsonSerializer.Serialize(cycle, JsonLineOptions)), Utf8NoBom);
    }

    private static string[] SignalsFor(string rowId) => rowId switch
    {
        "P5H-DPO-PTT-002" => new[] { "intent", "constraints", "desired_result", "answer_format", "forbidden_claim_boundary" },
        "P5H-DPO-PTT-004" => new[] { "session_mode", "authority_scope", "allowed_work", "forbidden_actions", "required_inputs" },
        "P5H-DPO-PTT-005" => new[] { "template_gap", "missing_inputs", "required_context", "completion_criteria", "stop_conditions" },
        "P5H-DPO-PTT-006" => new[] { "phase_contract", "phase_dependencies", "allowed_scope", "expected_result", "completion_boundary" },
        _ => new[] { "reserved_prompt_to_task_contract" }
    };

    private static IReadOnlyList<string> ControlIdsFor(P6ReservedRow row)
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

    private static string[] ShortcutSignalsFor(P6ReservedRow row, string controlId) => controlId switch
    {
        "exact_prompt_lookup" => new[] { "exact_prompt_marker_only", row.RowId },
        "filename_path_title_shortcut" => new[] { "filename_marker_only", "path_marker_only", "title_marker_only" },
        "keyword_only_classifier" => new[] { "prompt", "task", "contract" },
        "source_family_id_shortcut" => new[] { row.SourceFamilyId },
        "nearest_only_retrieval_only" => new[] { "nearest_neighbor_only", "retrieval_only" },
        "accepted_label_shortcut" => new[] { "accepted", "ready" },
        "session_mode_label_shortcut" => new[] { "mode", "scope" },
        "template_marker_shortcut" => new[] { "template", "gap" },
        "phase_number_shortcut" => new[] { "phase", "p5h", "p7" },
        _ => new[] { controlId }
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

    private sealed record P6ReservedRow(
        [property: JsonPropertyName("rowId")] string RowId,
        [property: JsonPropertyName("sourceFamilyId")] string SourceFamilyId,
        [property: JsonPropertyName("splitRole")] string SplitRole,
        [property: JsonPropertyName("usedForTrainingInP6")] bool UsedForTrainingInP6,
        [property: JsonPropertyName("trainingReady")] bool TrainingReady);

    private sealed record P7AProbeResult(
        string RowId,
        string SplitRole,
        string ProbeKind,
        string ObservedStatus,
        string ObservedTaskType,
        string? MatchedPatternId,
        int MatchedSignalCount,
        bool PositiveReservedBehavior,
        bool ShortcutProducedLearnedOutput,
        string SummaryRu);

    private sealed record ReviewCycle(
        string CycleId,
        string FocusRu,
        string ResultRu,
        bool Passed);
}
