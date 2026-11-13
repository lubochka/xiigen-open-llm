// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SmartSkillsDomain;

namespace LlmFromScratch.Tests;

public sealed class PromptToTaskP7GFormalEvalAblationConfirmationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly JsonSerializerOptions JsonLineOptions = new() { WriteIndented = false };

    [Fact]
    public async Task P7GFormalConfirmationFreshLoadsP7FAndBlocksShortcutControls()
    {
        var root = FindRepositoryRoot();
        var artifactRoot = Path.Combine(root, "artifacts", "smart-skills-semantic-quality-completion");
        var p7fDir = Path.Combine(artifactRoot, "p7f-prompt-to-task-missing-coverage-repair");
        var p7gDir = Path.Combine(artifactRoot, "p7g-prompt-to-task-formal-eval-ablation-confirmation");
        // Каталог расписки больше не создаётся заранее: решение о записи принимается
        // только после вердикта хранилища об опубликованности этой попытки.

        var checkpointPath = Path.Combine(p7fDir, "prompt-to-task-p7f-candidate-checkpoint.json");
        var manifestPath = Path.Combine(p7fDir, "prompt-to-task-p7f-candidate-manifest.json");
        var checkpointJson = await File.ReadAllTextAsync(checkpointPath, Encoding.UTF8);
        using var checkpointDocument = JsonDocument.Parse(checkpointJson);

        Assert.False(checkpointDocument.RootElement.GetProperty("semanticQualityClaimed").GetBoolean());
        Assert.Equal(7, checkpointDocument.RootElement.GetProperty("learnedState").GetProperty("learnedPatterns").GetArrayLength());
        AssertNoEvalHoldoutTrainingRefs(checkpointDocument);
        AssertNoExactFailedP7ESignalCombination(checkpointDocument);

        var loadResult = await PromptToTaskMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        Assert.True(loadResult.IsSuccess, loadResult.ErrorMessage);
        var machine = loadResult.Value!.Machine;
        Assert.False(loadResult.Value.Checkpoint.SemanticQualityClaimed);
        Assert.False(loadResult.Value.Manifest.SemanticQualityClaimed);

        var positiveResults = ReservedRows()
            .Select(row => EvaluatePositive(machine, row))
            .ToArray();
        var controlResults = ReservedRows()
            .SelectMany(row => ControlProbes(row).Select(control => EvaluateControl(machine, row, control)))
            .ToArray();

        var positivePasses = positiveResults.Count(result => result.PositiveReservedBehavior);
        var shortcutLeaks = controlResults.Count(result => result.ShortcutProducedLearnedOutput);
        var candidatePassed = positivePasses == positiveResults.Length && shortcutLeaks == 0;

        // Содержимое расписки считается в память: запись решается отдельно, после вердикта
        // хранилища. Состав полностью определён входом, поэтому опубликованные байты можно
        // сверить отпечатком вместо их перезаписи.
        var evidencePayloads = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["prompt-to-task-p7g-reserved-positive-results.jsonl"] = BuildJsonl(positiveResults),
            ["prompt-to-task-p7g-ablation-control-results.jsonl"] = BuildJsonl(controlResults),
            ["prompt-to-task-p7g-eval-ablation-report.json"] = BuildReportJson(
                Relative(root, checkpointPath),
                Relative(root, manifestPath),
                positiveResults,
                controlResults,
                candidatePassed)
        };

        // Публикация ровно один раз, затем только сверка: опубликованная попытка не меняется.
        PublishOrVerifyEvidence(root, p7gDir, evidencePayloads);

        Assert.True(candidatePassed);
        Assert.Equal(4, positivePasses);
        Assert.Equal(0, shortcutLeaks);
        AssertNoMojibakeInDirectory(p7gDir);
    }

    private static PositiveResult EvaluatePositive(PromptToTaskMachine machine, ReservedRow row)
    {
        var request = BuildRequest($"p7g-positive-{row.RowId}", row.Signals, "p7g_reserved_positive");
        var result = machine.Translate(request);
        var status = result.IsSuccess ? result.Value!.ContractStatus : "runtime_error";
        var positive = status == PromptToTaskMachine.LearnedPatternStatus;
        var provenance = result.IsSuccess ? result.Value!.StructuredTask.SourceProvenance : new Dictionary<string, string>();

        return new PositiveResult(
            ProbeId: $"p7g-positive-{row.RowId}",
            SourceRowId: row.RowId,
            ProposedRole: row.ProposedRole,
            InputTransformationSummary: "Redacted reserved-row surrogate plus neutral content signals; no raw private prompt text.",
            ExpectedStableBehavior: "PromptToTask should return a learned task contract from content signals only.",
            ObservedStatus: status,
            ObservedTaskType: result.IsSuccess ? result.Value!.StructuredTask.TaskType : null,
            MatchedPatternId: provenance.TryGetValue("learned_pattern_id", out var patternId) ? patternId : null,
            MatchedSignalCount: provenance.TryGetValue("matched_signal_count", out var matched) && int.TryParse(matched, out var count) ? count : 0,
            PositiveReservedBehavior: positive,
            EvidenceBoundary: "fresh_loaded_p7f_checkpoint_component_only",
            CommandOrMethodUsed: "PromptToTaskMachine.LoadCheckpointAsync + PromptToTaskMachine.Translate",
            NoTrainingRun: true,
            NoCheckpointRewrite: true,
            SemanticQualityClaimed: false);
    }

    private static ControlResult EvaluateControl(PromptToTaskMachine machine, ReservedRow row, ControlProbe control)
    {
        var request = BuildRequest($"p7g-control-{row.RowId}-{control.ControlId}", control.Signals, $"p7g_control_{control.ControlId}");
        var result = machine.Translate(request);
        var status = result.IsSuccess ? result.Value!.ContractStatus : "runtime_error";
        var leaked = status == PromptToTaskMachine.LearnedPatternStatus;
        var provenance = result.IsSuccess ? result.Value!.StructuredTask.SourceProvenance : new Dictionary<string, string>();

        return new ControlResult(
            ProbeId: $"p7g-control-{row.RowId}-{control.ControlId}",
            SourceRowId: row.RowId,
            ProposedRole: row.ProposedRole,
            ControlId: control.ControlId,
            InputTransformationSummary: control.InputTransformationSummary,
            ExpectedStableBehavior: "Shortcut-only control must not produce a learned PromptToTask output.",
            ObservedStatus: status,
            ObservedTaskType: result.IsSuccess ? result.Value!.StructuredTask.TaskType : null,
            MatchedPatternId: provenance.TryGetValue("learned_pattern_id", out var patternId) ? patternId : null,
            ShortcutProducedLearnedOutput: leaked,
            EvidenceBoundary: "fresh_loaded_p7f_checkpoint_component_only",
            CommandOrMethodUsed: "PromptToTaskMachine.LoadCheckpointAsync + PromptToTaskMachine.Translate",
            NoTrainingRun: true,
            NoCheckpointRewrite: true,
            SemanticQualityClaimed: false);
    }

    private static PromptToTaskRequest BuildRequest(string promptId, IReadOnlyList<string> signals, string scope)
    {
        var surrogate = $"P7G redacted PromptToTask formal confirmation probe {promptId}.";
        return new PromptToTaskRequest(
            PromptId: promptId,
            RawPrompt: surrogate,
            RawPromptSha256: $"sha256:{Sha256Hex(surrogate + "|" + string.Join("|", signals))}",
            PromptProvenance: "p7g_redacted_surrogate_without_source_authority",
            UserLanguage: "ru",
            ExplicitConstraints: signals.ToArray(),
            ContextFields: new Dictionary<string, string>
            {
                ["probe_scope"] = scope,
                ["raw_private_prompt_text_used"] = "false",
                ["source_or_row_id_authority"] = "false"
            });
    }

    private static IReadOnlyList<ReservedRow> ReservedRows() =>
    [
        new("P5H-DPO-PTT-002", "eval_candidate", ["intent", "constraints", "desired_result", "answer_format", "forbidden_claim_boundary"]),
        new("P5H-DPO-PTT-004", "eval_candidate", ["session_mode", "authority_scope", "allowed_work", "forbidden_actions", "required_inputs"]),
        new("P5H-DPO-PTT-005", "holdout_candidate", ["template_gap", "missing_inputs", "required_context", "completion_criteria", "stop_conditions"]),
        new("P5H-DPO-PTT-006", "holdout_candidate", ["phase_contract", "phase_dependencies", "allowed_scope", "expected_result", "completion_boundary"])
    ];

    private static IReadOnlyList<ControlProbe> ControlProbes(ReservedRow row) =>
    [
        new("exact_prompt_lookup", [$"exact_prompt:{row.RowId}"], "Only an exact prompt lookup marker is provided."),
        new("filename_path_title", [$"filename:{row.RowId}", $"path:{row.RowId}", $"title:{row.RowId}"], "Only filename/path/title-like markers are provided."),
        new("source_family_id", [$"source_family:{row.RowId}", $"source_family_id:{row.RowId}"], "Only source-family markers are provided."),
        new("nearest_only_retrieval_only", ["nearest_only", "retrieval_only"], "Only retrieval/nearest-neighbor markers are provided."),
        new("row_id_marker", [row.RowId.ToLowerInvariant()], "Only the row id marker is provided."),
        new("source_provenance_signal", ["source_ref", "source_manifest", "checkpoint_ref", "accepted_ref"], "Only provenance field names are provided."),
        new("pattern_id_authority", ["p7f-pattern-task-contract-planning", "p7f-pattern-missing-context-template"], "Only candidate learned pattern ids are provided."),
        new("keyword_only", ["prompt", "task"], "Only broad keywords are provided.")
    ];

    private static void AssertNoEvalHoldoutTrainingRefs(JsonDocument checkpointDocument)
    {
        var refs = checkpointDocument.RootElement.GetProperty("acceptedRefIds").EnumerateArray()
            .Select(value => value.GetString() ?? string.Empty)
            .ToArray();

        foreach (var forbidden in new[] { "P5H-DPO-PTT-002", "P5H-DPO-PTT-004", "P5H-DPO-PTT-005", "P5H-DPO-PTT-006", "eval", "holdout" })
        {
            Assert.DoesNotContain(refs, value => value.Contains(forbidden, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static void AssertNoExactFailedP7ESignalCombination(JsonDocument checkpointDocument)
    {
        var forbiddenCombos = new[]
        {
            new[] { "intent", "constraints", "desired_result", "answer_format", "forbidden_claim_boundary" },
            new[] { "template_gap", "missing_inputs", "required_context", "completion_criteria", "stop_conditions" }
        };

        foreach (var pattern in checkpointDocument.RootElement.GetProperty("learnedState").GetProperty("learnedPatterns").EnumerateArray())
        {
            var signals = pattern.GetProperty("learnedSignals").EnumerateArray()
                .Select(value => value.GetString() ?? string.Empty)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            Assert.DoesNotContain(forbiddenCombos, combo => combo.All(signals.Contains));
        }
    }

    // Текст строкового набора расписки считается без записи на диск.
    private static string BuildJsonl<T>(IEnumerable<T> rows)
    {
        var lines = rows.Select(row => JsonSerializer.Serialize(row, JsonLineOptions)); // Одна строка на запись.
        return string.Join("\n", lines) + "\n"; // Разделитель LF и ровно один перевод строки в конце.
    }

    // Текст отчёта расписки считается без записи на диск.
    private static string BuildReportJson(
        string checkpointRef,
        string manifestRef,
        IReadOnlyCollection<PositiveResult> positiveResults,
        IReadOnlyCollection<ControlResult> controlResults,
        bool candidatePassed)
    {
        var report = new
        {
            phase = "P7G_PROMPT_TO_TASK_FORMAL_EVAL_ABLATION_CONFIRMATION",
            verdict = candidatePassed ? "PASS_CANDIDATE_READY_FOR_PARENT_REVIEW" : "FAIL_EVIDENCE_READY_FOR_PARENT_REVIEW",
            status = candidatePassed ? "ready_for_parent_review" : "fail_ready_for_parent_review",
            checkpointPath = checkpointRef,
            manifestPath = manifestRef,
            freshLoadThroughProjectCode = true,
            reservedPositiveRows = positiveResults.Count,
            reservedPositivePasses = positiveResults.Count(result => result.PositiveReservedBehavior),
            ablationControls = controlResults.Count,
            shortcutLeaks = controlResults.Count(result => result.ShortcutProducedLearnedOutput),
            evalHoldoutTrainingContamination = false,
            exactFailedP7ESignalCombinationCopied = false,
            componentEvalAblationCandidatePassed = candidatePassed,
            fullApiSemanticQualityClaimed = false,
            semanticQualityClaimed = false,
            p7PassClaimed = false,
            finalModelReadyClaimed = false,
            noTrainingRun = true,
            noCheckpointRewrite = true
        };

        return NormalizeLf(JsonSerializer.Serialize(report, JsonOptions)) + "\n"; // Переводы строк LF, один в конце.
    }

    // Публикация расписки опыта ровно один раз, затем только сверка её отпечатком.
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
        var published = ExperimentFixes.ExperimentEvidenceSink.IsPublishedEvidenceDirectory(
            repositoryRoot, declaredDirectory, out var verdict);

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
                sink.WriteTextExactly(payload.Key, payload.Value); // Содержимое пишется как есть, без правок.
        }

        // Независимо от того, писал ли этот прогон, назначенная расписка обязана быть на
        // месте и непустой. Побитовое равенство здесь намеренно НЕ утверждается: состав
        // отчёта зависит от наблюдений живой сборки, и расхождение означало бы отдельную
        // находку о расхождении опубликованной попытки с текущим кодом, а не дефект этой
        // защиты. Доказательство неизменности байт даёт снимок отпечатков вокруг прогона.
        foreach (var payload in payloads)
        {
            var savedPath = Path.Combine(declaredDirectory, payload.Key); // Путь опубликованного файла.
            Assert.True(
                File.Exists(savedPath),
                $"P7G_EVIDENCE_MISSING:{payload.Key}; каталог расписки {declaredDirectory};"
                + $" вердикт хранилища: {verdict}");
            Assert.True(
                new FileInfo(savedPath).Length > 0,
                $"P7G_EVIDENCE_EMPTY:{payload.Key}; каталог расписки {declaredDirectory};"
                + $" вердикт хранилища: {verdict}");
            Assert.False(
                string.IsNullOrWhiteSpace(payload.Value),
                $"P7G_EVIDENCE_PAYLOAD_EMPTY:{payload.Key}"); // Пересчёт состава обязан был что-то дать.
        }
    }

    private static void AssertNoMojibakeInDirectory(string artifactDir)
    {
        foreach (var path in Directory.EnumerateFiles(artifactDir, "*", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(path, Encoding.UTF8);
            Assert.DoesNotContain(((char)0x00D0).ToString(), text);
            Assert.DoesNotContain(((char)0x00D1).ToString(), text);
            Assert.DoesNotContain("\uFFFD", text);
            Assert.DoesNotContain(new string('?', 4), text);
        }
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    private static string Sha256Hex(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string NormalizeLf(string value) =>
        value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);

    private static string Relative(string root, string fullPath)
        => Path.GetRelativePath(root, fullPath).Replace('\\', '/');

    private sealed record ReservedRow(string RowId, string ProposedRole, IReadOnlyList<string> Signals);

    private sealed record ControlProbe(string ControlId, IReadOnlyList<string> Signals, string InputTransformationSummary);

    private sealed record PositiveResult(
        string ProbeId,
        string SourceRowId,
        string ProposedRole,
        string InputTransformationSummary,
        string ExpectedStableBehavior,
        string ObservedStatus,
        string? ObservedTaskType,
        string? MatchedPatternId,
        int MatchedSignalCount,
        bool PositiveReservedBehavior,
        string EvidenceBoundary,
        string CommandOrMethodUsed,
        bool NoTrainingRun,
        bool NoCheckpointRewrite,
        bool SemanticQualityClaimed);

    private sealed record ControlResult(
        string ProbeId,
        string SourceRowId,
        string ProposedRole,
        string ControlId,
        string InputTransformationSummary,
        string ExpectedStableBehavior,
        string ObservedStatus,
        string? ObservedTaskType,
        string? MatchedPatternId,
        bool ShortcutProducedLearnedOutput,
        string EvidenceBoundary,
        string CommandOrMethodUsed,
        bool NoTrainingRun,
        bool NoCheckpointRewrite,
        bool SemanticQualityClaimed);
}
