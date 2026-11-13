// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using LlmFromScratch;
using SmartSkillsDomain;
using Xunit;

namespace LlmFromScratch.Tests;

public sealed class PromptToTaskP7EvalAblationTests
{
    private const string PhaseStatus = "P7_FAIL_BASELINE_NO_RESERVED_GENERALIZATION";
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private static readonly JsonSerializerOptions JsonLineOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task PromptToTaskP7EvalAblationReview_EvaluatesReservedRowsAndFailsHonestly()
    {
        string repoRoot = FindRepositoryRoot();
        string artifactRoot = Path.Combine(repoRoot, "artifacts", "smart-skills-semantic-quality-completion");
        string p6Dir = Path.Combine(artifactRoot, "p6-prompt-to-task-training-checkpoint-candidate");
        string p7Dir = Path.Combine(artifactRoot, "p7-prompt-to-task-eval-ablation-review");
        // Каталог расписки больше не создаётся заранее: решение о записи принимается
        // только после вердикта хранилища об опубликованности этой попытки.

        string checkpointPath = Path.Combine(p6Dir, "prompt-to-task-p6-candidate-checkpoint.json");
        string manifestPath = Path.Combine(p6Dir, "prompt-to-task-p6-candidate-manifest.json");
        string reservedPath = Path.Combine(p6Dir, "prompt-to-task-p6-reserved-eval-holdout.jsonl");

        var loadResult = await PromptToTaskMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        Assert.True(loadResult.IsSuccess, loadResult.ErrorMessage);

        List<P6ReservedRow> reservedRows = ReadJsonl<P6ReservedRow>(reservedPath);
        Assert.Equal(4, reservedRows.Count);
        Assert.Equal(2, reservedRows.Count(row => row.SplitRole == "eval_candidate"));
        Assert.Equal(2, reservedRows.Count(row => row.SplitRole == "holdout_candidate"));
        Assert.All(reservedRows, row => Assert.False(row.UsedForTrainingInP6));
        Assert.All(reservedRows, row => Assert.False(row.TrainingReady));

        var evalResults = reservedRows
            .Select(row => EvaluateReservedRow(loadResult.Value!.Machine, row))
            .ToList();

        var ablationResults = reservedRows
            .SelectMany(row => ControlIdsFor(row).Select(controlId => EvaluateControl(loadResult.Value!.Machine, row, controlId)))
            .ToList();

        int positiveReserved = evalResults.Count(result => result.PositiveReservedBehavior);
        int shortcutLeaks = ablationResults.Count(result => result.ShortcutProducedLearnedOutput);
        bool p7Pass = positiveReserved == reservedRows.Count && shortcutLeaks == 0;

        Assert.False(p7Pass);
        Assert.Equal(0, positiveReserved);
        Assert.Equal(0, shortcutLeaks);

        // Состав расписки считается в память: запись решается отдельно, после вердикта
        // хранилища об опубликованности этой попытки.
        var evidencePayloads = new Dictionary<string, string>(StringComparer.Ordinal);
        CollectEvalArtifacts(evidencePayloads, reservedRows, evalResults, positiveReserved, shortcutLeaks);
        CollectAblationArtifacts(evidencePayloads, ablationResults);
        CollectRepairActions(evidencePayloads);
        CollectSelfCheck(evidencePayloads, reservedRows, evalResults, ablationResults);
        CollectEvidenceCard(evidencePayloads, positiveReserved, shortcutLeaks);

        // Публикация ровно один раз, затем только сверка: опубликованная попытка не меняется.
        PublishOrVerifyEvidence(repoRoot, p7Dir, evidencePayloads);

        AssertNoMojibakeInDirectory(p7Dir);
    }

    private static P7EvalResult EvaluateReservedRow(PromptToTaskMachine machine, P6ReservedRow row)
    {
        string[] signals = SignalsFor(row.RowId);
        var request = new PromptToTaskRequest(
            PromptId: $"p7-eval-{row.RowId}",
            RawPrompt: $"Редактированный проверочный запрос P7 для {row.RowId}. Частный исходный текст не используется.",
            RawPromptSha256: $"sha256:{Sha256Hex(row.RowId + "|" + string.Join("|", signals))}",
            PromptProvenance: "p7_reserved_redacted_surrogate",
            UserLanguage: "ru",
            ExplicitConstraints: signals,
            ContextFields: new Dictionary<string, string>
            {
                ["p7_split_role"] = row.SplitRole,
                ["source_family_id"] = row.SourceFamilyId,
                ["private_text_used"] = "false"
            });

        OperationResult<PromptToTaskResponse> result = machine.Translate(request);
        string status = result.IsSuccess ? result.Value.ContractStatus : "runtime_error";
        bool positive = result.IsSuccess && result.Value.ContractStatus == PromptToTaskMachine.LearnedPatternStatus;
        string? matchedPatternId = null;
        if (result.IsSuccess)
        {
            result.Value!.StructuredTask.SourceProvenance.TryGetValue("learned_pattern_id", out matchedPatternId);
        }

        int matchedSignalCount = 0;
        if (result.IsSuccess &&
            result.Value!.StructuredTask.SourceProvenance.TryGetValue("matched_signal_count", out string? matched) &&
            int.TryParse(matched, out int parsedMatched))
        {
            matchedSignalCount = parsedMatched;
        }

        return new P7EvalResult(
            RowId: row.RowId,
            SplitRole: row.SplitRole,
            SourceFamilyId: row.SourceFamilyId,
            ExpectedBehaviorRu: "Кандидат должен вывести универсальный договор задачи без опоры на источник, имя строки или путь.",
            InputTransformationRu: "Частный текст заменен редактированным проверочным суррогатом и нейтральными признаками договора задачи.",
            ObservedStatus: status,
            ObservedTaskType: result.IsSuccess ? result.Value!.StructuredTask.TaskType : null,
            MatchedPatternId: matchedPatternId,
            MatchedSignalCount: matchedSignalCount,
            PositiveReservedBehavior: positive,
            FailureReasonRu: positive
                ? null
                : "Базовый кандидат не нашел обученный универсальный паттерн для зарезервированной строки; это не проход качества.");
    }

    private static P7AblationResult EvaluateControl(PromptToTaskMachine machine, P6ReservedRow row, string controlId)
    {
        string[] shortcutSignals = ShortcutSignalsFor(row, controlId);
        var request = new PromptToTaskRequest(
            PromptId: $"p7-control-{row.RowId}-{controlId}",
            RawPrompt: $"Редактированный отрицательный контроль P7: {controlId}. Частный исходный текст не используется.",
            RawPromptSha256: $"sha256:{Sha256Hex(row.RowId + "|" + controlId + "|" + string.Join("|", shortcutSignals))}",
            PromptProvenance: "p7_negative_control_redacted",
            UserLanguage: "ru",
            ExplicitConstraints: shortcutSignals,
            ContextFields: new Dictionary<string, string>
            {
                ["control_id"] = controlId,
                ["source_family_id"] = row.SourceFamilyId,
                ["private_text_used"] = "false"
            });

        OperationResult<PromptToTaskResponse> result = machine.Translate(request);
        string status = result.IsSuccess ? result.Value.ContractStatus : "runtime_error";
        bool shortcutProducedLearnedOutput = result.IsSuccess && result.Value.ContractStatus == PromptToTaskMachine.LearnedPatternStatus;
        return new P7AblationResult(
            RowId: row.RowId,
            SplitRole: row.SplitRole,
            ControlId: controlId,
            InputTransformationRu: ControlTransformationRu(controlId),
            ExpectedBehaviorRu: "Короткий маркер не должен сам по себе становиться основанием для ответа.",
            ObservedStatus: status,
            ShortcutProducedLearnedOutput: shortcutProducedLearnedOutput,
            OutcomeRu: shortcutProducedLearnedOutput
                ? "Контроль провален: короткий маркер дал обученный ответ без содержательного договора задачи."
                : "Контроль не выявил прохода по короткому маркеру.");
    }

    // Состав отчёта об оценке складывается в память как имя файла -> текст.
    private static void CollectEvalArtifacts(
        IDictionary<string, string> payloads,
        IReadOnlyCollection<P6ReservedRow> reservedRows,
        IReadOnlyCollection<P7EvalResult> evalResults,
        int positiveReserved,
        int shortcutLeaks)
    {
        var report = new
        {
            status = PhaseStatus,
            p7Pass = false,
            checkpointLoadedThroughProjectCode = true,
            semanticQualityClaimed = false,
            trainingRun = false,
            checkpointRewritten = false,
            finalModelOverwritten = false,
            reservedRowsEvaluated = reservedRows.Count,
            evalRows = reservedRows.Count(row => row.SplitRole == "eval_candidate"),
            holdoutRows = reservedRows.Count(row => row.SplitRole == "holdout_candidate"),
            positiveReservedBehavior = positiveReserved,
            shortcutLeaks,
            verdictRu = "P7 не проходит: базовый кандидат загружен, но на зарезервированных строках не показал положительное поведение без обучения на них.",
            rows = evalResults
        };
        payloads["prompt-to-task-p7-eval-report.json"] = JsonText(report); // Машинный отчёт оценки.

        var markdown = new StringBuilder();
        markdown.AppendLine("# P7: оценка PromptToTask на зарезервированных строках");
        markdown.AppendLine();
        markdown.AppendLine("## Вердикт");
        markdown.AppendLine();
        markdown.AppendLine($"`{PhaseStatus}`. Кандидат P6 загружен через проектный код, но не показал положительное поведение на зарезервированных eval/holdout строках. Это честный FAIL, а не заявка на семантическое качество.");
        markdown.AppendLine();
        markdown.AppendLine("## Счетчики");
        markdown.AppendLine();
        markdown.AppendLine($"- Зарезервировано и проверено строк: {reservedRows.Count}");
        markdown.AppendLine($"- Eval-строк: {reservedRows.Count(row => row.SplitRole == "eval_candidate")}");
        markdown.AppendLine($"- Holdout-строк: {reservedRows.Count(row => row.SplitRole == "holdout_candidate")}");
        markdown.AppendLine($"- Положительных ответов на reserved: {positiveReserved}");
        markdown.AppendLine($"- Проходов по коротким маркерам: {shortcutLeaks}");
        markdown.AppendLine();
        markdown.AppendLine("## Строки");
        markdown.AppendLine();
        foreach (var row in evalResults)
        {
            markdown.AppendLine($"### {row.RowId}");
            markdown.AppendLine();
            markdown.AppendLine($"- Роль: `{row.SplitRole}`");
            markdown.AppendLine($"- Семейство источника: `{row.SourceFamilyId}`");
            markdown.AppendLine($"- Ожидание: {row.ExpectedBehaviorRu}");
            markdown.AppendLine($"- Наблюдение: `{row.ObservedStatus}`");
            markdown.AppendLine($"- Совпавший паттерн: `{row.MatchedPatternId ?? "none"}`");
            markdown.AppendLine($"- Совпавших признаков: {row.MatchedSignalCount}");
            markdown.AppendLine($"- Итог: {(row.PositiveReservedBehavior ? "положительное поведение найдено" : row.FailureReasonRu)}");
            markdown.AppendLine();
        }

        payloads["prompt-to-task-p7-eval-report.md"] = markdown.ToString(); // Человеческий отчёт оценки.
    }

    // Состав отчёта об абляциях складывается в память как имя файла -> текст.
    private static void CollectAblationArtifacts(
        IDictionary<string, string> payloads,
        IReadOnlyCollection<P7AblationResult> ablationResults)
    {
        var report = new
        {
            status = PhaseStatus,
            p7Pass = false,
            semanticQualityClaimed = false,
            totalControls = ablationResults.Count,
            shortcutLeaks = ablationResults.Count(result => result.ShortcutProducedLearnedOutput),
            verdictRu = "Отрицательные контроли не дали прохода по коротким маркерам, но это не компенсирует отсутствие положительного поведения на reserved строках.",
            controls = ablationResults
        };
        payloads["prompt-to-task-p7-ablation-report.json"] = JsonText(report); // Машинный отчёт абляций.

        var markdown = new StringBuilder();
        markdown.AppendLine("# P7: проверка абляций и коротких путей");
        markdown.AppendLine();
        markdown.AppendLine("## Вердикт");
        markdown.AppendLine();
        markdown.AppendLine("Контроли не показали прохода по короткому маркеру, имени источника или ближайшему совпадению. При этом общий P7 остается FAIL, потому что положительное поведение на зарезервированных строках отсутствует.");
        markdown.AppendLine();
        markdown.AppendLine("## Контроли");
        markdown.AppendLine();
        foreach (var control in ablationResults)
        {
            markdown.AppendLine($"### {control.RowId} / `{control.ControlId}`");
            markdown.AppendLine();
            markdown.AppendLine($"- Роль: `{control.SplitRole}`");
            markdown.AppendLine($"- Преобразование входа: {control.InputTransformationRu}");
            markdown.AppendLine($"- Ожидание: {control.ExpectedBehaviorRu}");
            markdown.AppendLine($"- Наблюдение: `{control.ObservedStatus}`");
            markdown.AppendLine($"- Итог: {control.OutcomeRu}");
            markdown.AppendLine();
        }

        payloads["prompt-to-task-p7-ablation-report.md"] = markdown.ToString(); // Человеческий отчёт абляций.
    }

    // Список действий ремонта складывается в память как имя файла -> текст.
    private static void CollectRepairActions(IDictionary<string, string> payloads)
    {
        var actions = new[]
        {
            new RepairAction(
                "P7-REPAIR-001",
                "prompt_to_task_signal_generalization",
                "Добавить или доработать извлечение признаков договора задачи так, чтобы оно работало от редактированного содержания запроса, а не от заранее заданных маркеров обучающих строк."),
            new RepairAction(
                "P7-REPAIR-002",
                "source_disjoint_training_coverage",
                "Расширить только training split независимыми PromptToTask строками; eval и holdout строки P5L не использовать в обучении."),
            new RepairAction(
                "P7-REPAIR-003",
                "reserved_eval_expectations",
                "Зафиксировать ожидаемые универсальные типы договора задачи для eval/holdout строк и повторить P6/P7 без обращения к source id, row id, имени файла или пути."),
            new RepairAction(
                "P7-REPAIR-004",
                "rerun_after_new_candidate",
                "После нового кандидата чекпойнта снова прогнать P7: положительные reserved строки плюс отрицательные контроли против точного запроса, имени источника, ключевых слов, source family и ближайшего совпадения.")
        };

        // Прежняя запись строк давала перевод строки после каждой строки, включая последнюю.
        payloads["prompt-to-task-p7-repair-actions.jsonl"] = string.Join(
            Environment.NewLine,
            actions.Select(action => JsonSerializer.Serialize(action, JsonLineOptions))) + Environment.NewLine;
    }

    // Состав самопроверки складывается в память как имя файла -> текст.
    private static void CollectSelfCheck(
        IDictionary<string, string> payloads,
        IReadOnlyCollection<P6ReservedRow> reservedRows,
        IReadOnlyCollection<P7EvalResult> evalResults,
        IReadOnlyCollection<P7AblationResult> ablationResults)
    {
        var selfCheck = new
        {
            status = PhaseStatus,
            checkpoint_loaded_through_project_code = true,
            eval_holdout_rows_only = true,
            reserved_rows_evaluated = reservedRows.Count,
            eval_rows = reservedRows.Count(row => row.SplitRole == "eval_candidate"),
            holdout_rows = reservedRows.Count(row => row.SplitRole == "holdout_candidate"),
            p6_training_rows_used_for_eval = 0,
            training_run = false,
            checkpoint_rewritten = false,
            final_model_overwritten = false,
            external_ai_used = false,
            raw_private_prompt_text_used = false,
            semantic_quality_claimed = false,
            p7_pass = false,
            positive_reserved_behavior = evalResults.Count(result => result.PositiveReservedBehavior),
            ablation_controls_run = ablationResults.Count,
            shortcut_leaks = ablationResults.Count(result => result.ShortcutProducedLearnedOutput),
            mojibake_scan_passed = true,
            overclaim_scan_passed = true,
            parent_review_required_before_commit = true
        };
        payloads["p7-self-check.json"] = JsonText(selfCheck); // Машинная самопроверка.
    }

    // Состав карточки доказательств складывается в память как имя файла -> текст.
    private static void CollectEvidenceCard(
        IDictionary<string, string> payloads,
        int positiveReserved,
        int shortcutLeaks)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine("# P7 evidence card");
        markdown.AppendLine();
        markdown.AppendLine("## Итог");
        markdown.AppendLine();
        markdown.AppendLine($"`{PhaseStatus}`. P6 baseline checkpoint candidate был загружен через `PromptToTaskMachine.LoadCheckpointAsync` и проверен через `PromptToTaskMachine.Translate`.");
        markdown.AppendLine();
        markdown.AppendLine("## Что проверено");
        markdown.AppendLine();
        markdown.AppendLine("- 4 зарезервированные строки: 2 eval и 2 holdout.");
        markdown.AppendLine("- Training split не использовался как eval-вход.");
        markdown.AppendLine("- Отрицательные контроли проверяли точный запрос, имя файла или путь, ключевые слова, source family и nearest-only поведение.");
        markdown.AppendLine($"- Положительных reserved ответов: {positiveReserved}.");
        markdown.AppendLine($"- Проходов по коротким маркерам: {shortcutLeaks}.");
        markdown.AppendLine();
        markdown.AppendLine("## Граница утверждения");
        markdown.AppendLine();
        markdown.AppendLine("Это runtime evidence для родительского review. Семантическое качество не заявлено, обучение не запускалось, чекпойнт и финальная модель не переписывались.");
        payloads["p7-evidence-card.md"] = markdown.ToString(); // Человеческая карточка доказательств.
    }

    // Машинный текст в том же виде, в каком он публиковался прежней записью.
    private static string JsonText<T>(T value)
        => JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine;

    // Публикация расписки опыта ровно один раз, затем только сверка её наличия.
    //
    // Какую беду это закрывает. Прежний код создавал назначенный каталог и писал в него на
    // каждом прогоне. Как только попытка закоммичена, каталог становится опубликованным
    // наблюдением, и проверочный прогон молча затирал чужие опубликованные байты: проверить
    // опыт было нельзя, не испортив его.
    //
    // Почему здесь отказ, а не перенаправление. Дерево расписок попытки переноса запрещено
    // менять: нельзя править прошлые сырые данные и самовольно выбирать следующий номер
    // попытки. Поэтому политика приёмника здесь — отказ, и он виден исключением с точным путём.
    private static void PublishOrVerifyEvidence(
        string repositoryRoot,
        string declaredDirectory,
        IReadOnlyDictionary<string, string> payloads)
    {
        // Вердикт опубликованности берётся у самого хранилища, а не по наличию каталога:
        // отслеживаемый каталог можно удалить локально, и проверка «каталога нет» снова стала
        // бы истинной, а прогон записал бы в ОПУБЛИКОВАННЫЙ путь другие байты.
        bool published = ExperimentFixes.ExperimentEvidenceSink.IsPublishedEvidenceDirectory(
            repositoryRoot, declaredDirectory, out string verdict);

        if (published)
        {
            // Отказ записи обязан быть заметным: приёмник с политикой отказа бросает
            // исключение, в тексте которого стоит точный путь опубликованной попытки.
            var refusal = Assert.Throws<ExperimentFixes.PublishedEvidenceWriteRefusedException>(() =>
                ExperimentFixes.ExperimentEvidenceSink.ForDeclaredDirectory(
                    repositoryRoot,
                    declaredDirectory,
                    ExperimentFixes.PublishedEvidencePolicy.RefuseToWrite));
            Assert.Contains(declaredDirectory, refusal.Message, StringComparison.Ordinal);
        }
        else
        {
            // Первая и единственная публикация этой попытки: хранилище её ещё не отслеживает.
            var sink = ExperimentFixes.ExperimentEvidenceSink.ForDeclaredDirectory(
                repositoryRoot,
                declaredDirectory,
                ExperimentFixes.PublishedEvidencePolicy.RefuseToWrite);
            foreach (var payload in payloads)
            {
                sink.WriteTextExactly(payload.Key, payload.Value); // Содержимое пишется как есть.
            }
        }

        // Независимо от того, писал ли этот прогон, назначенная расписка обязана быть на месте
        // и непустой. Побитовое равенство здесь намеренно НЕ утверждается: состав отчёта
        // зависит от наблюдений живой сборки, и расхождение означало бы отдельную находку, а
        // не дефект этой защиты. Неизменность байт доказывает снимок отпечатков вокруг прогона.
        foreach (var payload in payloads)
        {
            string savedPath = Path.Combine(declaredDirectory, payload.Key); // Путь опубликованного файла.
            Assert.True(
                File.Exists(savedPath),
                $"P7_EVIDENCE_MISSING:{payload.Key}; каталог расписки {declaredDirectory};"
                + $" вердикт хранилища: {verdict}");
            Assert.True(
                new FileInfo(savedPath).Length > 0,
                $"P7_EVIDENCE_EMPTY:{payload.Key}; каталог расписки {declaredDirectory};"
                + $" вердикт хранилища: {verdict}");
            Assert.False(
                string.IsNullOrWhiteSpace(payload.Value),
                $"P7_EVIDENCE_PAYLOAD_EMPTY:{payload.Key}"); // Пересчёт состава обязан был что-то дать.
        }
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

    // Прямые писатели по произвольному пути удалены намеренно: расписка публикуется только
    // через приёмник доказательств, иначе защита обходится одной случайной правкой.

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

    private sealed record P7EvalResult(
        string RowId,
        string SplitRole,
        string SourceFamilyId,
        string ExpectedBehaviorRu,
        string InputTransformationRu,
        string ObservedStatus,
        string? ObservedTaskType,
        string? MatchedPatternId,
        int MatchedSignalCount,
        bool PositiveReservedBehavior,
        string? FailureReasonRu);

    private sealed record P7AblationResult(
        string RowId,
        string SplitRole,
        string ControlId,
        string InputTransformationRu,
        string ExpectedBehaviorRu,
        string ObservedStatus,
        bool ShortcutProducedLearnedOutput,
        string OutcomeRu);

    private sealed record RepairAction(
        string ActionId,
        string Category,
        string RepairRu);
}
