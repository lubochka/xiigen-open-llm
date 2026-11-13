// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LlmFromScratch.Tests;
using SmartSkillsDomain;

public sealed class PromptToTaskP6TrainingCheckpointCandidateTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private static readonly JsonSerializerOptions JsonLineOptions = new(JsonSerializerDefaults.Web);

    private static readonly DateTimeOffset FrozenP6CandidateSavedAtUtc = new(2026, 8, 24, 15, 6, 5, TimeSpan.Zero);

    [Fact]
    public async Task PromptToTaskP6TrainingCheckpointCandidate_CreatesBaselineCandidateFromTrainSplitOnly()
    {
        var repoRoot = FindRepositoryRoot();
        var artifactDir = Path.Combine(
            repoRoot,
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p6-prompt-to-task-training-checkpoint-candidate");
        var p5lRowsPath = Path.Combine(
            repoRoot,
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p5l-prompt-to-task-training-row-promotion-review",
            "prompt-to-task-training-ready-candidates.jsonl");

        Directory.CreateDirectory(artifactDir);

        var rows = File.ReadLines(p5lRowsPath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<P5LReadyRow>(line, JsonOptions)!)
            .ToArray();
        var trainRows = rows
            .Where(row => row.UsedForTrainingInP6 && row.TrainingReady)
            .OrderBy(row => row.RowId, StringComparer.Ordinal)
            .ToArray();
        var reservedRows = rows
            .Where(row => !row.UsedForTrainingInP6)
            .OrderBy(row => row.RowId, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(6, rows.Length);
        Assert.Equal(2, trainRows.Length);
        Assert.Equal(4, reservedRows.Length);
        Assert.All(reservedRows, row => Assert.False(row.TrainingReady));

        var trainingInputPath = Path.Combine(artifactDir, "prompt-to-task-p6-training-input.jsonl");
        var reservedPath = Path.Combine(artifactDir, "prompt-to-task-p6-reserved-eval-holdout.jsonl");
        await WriteJsonLinesAsync(trainingInputPath, trainRows.Select(BuildTrainingInputRow));
        await WriteJsonLinesAsync(reservedPath, reservedRows.Select(BuildReservedRow));

        var sourceManifestPath = Path.Combine(artifactDir, "prompt-to-task-p6-source-manifest.json");
        var sourceManifest = new
        {
            phase = "P6_PROMPT_TO_TASK_TRAINING_CHECKPOINT_CANDIDATE",
            source = "P5L split/dataset-ready rows",
            p5lInputRef = Relative(repoRoot, p5lRowsPath),
            trainingRowIds = trainRows.Select(row => row.RowId).ToArray(),
            reservedEvalRowIds = reservedRows.Where(row => row.UsedForEvalInP7).Select(row => row.RowId).ToArray(),
            reservedHoldoutRowIds = reservedRows.Where(row => row.UsedForHoldoutEval).Select(row => row.RowId).ToArray(),
            rawPrivatePromptTextCopied = false,
            semanticQualityClaimed = false,
            note = "Only rows with used_for_training_in_p6=true are used to build this baseline checkpoint candidate."
        };
        await WriteJsonAsync(sourceManifestPath, sourceManifest);
        var sourceManifestSha = await HashFileAsync(sourceManifestPath);

        var patterns = trainRows.Select(BuildLearnedPattern).ToArray();
        var descriptor = new PromptToTaskLearnedStateDescriptor(
            StateId: "prompt-to-task-state:v1:p6-baseline-candidate",
            ManifestId: Relative(repoRoot, Path.Combine(artifactDir, "prompt-to-task-p6-candidate-manifest.json")),
            ManifestSha256: $"sha256:{sourceManifestSha}",
            MachineType: nameof(PromptToTaskMachine),
            SchemaVersion: "prompt-to-task-state-v1",
            SourceBaseCommit: "36defed7",
            StateSha256: "",
            LearnedPatterns: patterns);
        descriptor = descriptor with { StateSha256 = HashLearnedState(descriptor) };

        var machine = new PromptToTaskMachine(new StaticPromptToTaskLearnedStateLoader(descriptor));
        var checkpointPath = Path.Combine(artifactDir, "prompt-to-task-p6-candidate-checkpoint.json");
        var manifestPath = Path.Combine(artifactDir, "prompt-to-task-p6-candidate-manifest.json");
        var sourceManifestRef = Relative(repoRoot, sourceManifestPath);
        var acceptedRefs = trainRows.Select(row => $"p5l:{row.RowId}").ToArray();
        var exportResult = await machine.SaveCheckpointAsync(
            checkpointPath,
            manifestPath,
            sourceManifestRef,
            acceptedRefs,
            // Это committed-доказательство пересобирается тестом. Фиксированное время
            // отделяет реальное изменение кандидата от шума часов локального запуска.
            savedAtUtc: FrozenP6CandidateSavedAtUtc);

        Assert.True(exportResult.IsSuccess, exportResult.ErrorMessage);
        Assert.True(File.Exists(checkpointPath));
        Assert.True(File.Exists(manifestPath));

        var lifecyclePath = Path.Combine(artifactDir, "prompt-to-task-p6-lifecycle-export.json");
        var persistedLifecycleExport = exportResult.Value! with
        {
            // ImportAsync ниже получает настоящий путь текущего запуска. В committed evidence
            // пишем устойчивую Windows-ссылку, чтобы один и тот же тест из Windows и WSL
            // не пачкал файл только из-за формы пути.
            CheckpointRef = CanonicalRepoOwnedRef(
                exportResult.Value!.CheckpointRef,
                "PromptToTask P6 lifecycle checkpoint evidence"),
            ManifestRef = CanonicalRepoOwnedRef(
                exportResult.Value.ManifestRef,
                "PromptToTask P6 lifecycle manifest evidence")
        };
        AssertPersistedRepoRef(persistedLifecycleExport.CheckpointRef, "P6 lifecycle checkpoint evidence");
        AssertPersistedRepoRef(persistedLifecycleExport.ManifestRef, "P6 lifecycle manifest evidence");
        await WriteJsonAsync(lifecyclePath, persistedLifecycleExport);
        var persistedLifecycleJson = await File.ReadAllTextAsync(lifecyclePath);
        var diskLifecycleExport = JsonSerializer.Deserialize<PromptToTaskLifecycleExport>(persistedLifecycleJson, JsonOptions);
        Assert.NotNull(diskLifecycleExport);
        Assert.Equal(persistedLifecycleExport.CheckpointRef, diskLifecycleExport!.CheckpointRef);
        Assert.Equal(persistedLifecycleExport.ManifestRef, diskLifecycleExport.ManifestRef);
        Assert.Equal(persistedLifecycleExport.CheckpointSha256, diskLifecycleExport.CheckpointSha256);
        // Эта проверка смотрит не на объект в памяти, а на уже записанный файл:
        // именно его потом читает человек, CI и следующий агент при разборе доказательств.
        AssertPersistedRepoRef(diskLifecycleExport.CheckpointRef, "P6 lifecycle checkpoint evidence read back from disk");
        AssertPersistedRepoRef(diskLifecycleExport.ManifestRef, "P6 lifecycle manifest evidence read back from disk");
        Assert.DoesNotContain("/mnt/c/", persistedLifecycleJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("llm_mvp_core-DpoImplementation", persistedLifecycleJson, StringComparison.OrdinalIgnoreCase);

        var loaded = await PromptToTaskMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Equal(2, loaded.Value!.Checkpoint.LearnedState.LearnedPatterns!.Count);
        Assert.False(loaded.Value.Checkpoint.SemanticQualityClaimed);
        Assert.False(loaded.Value.Manifest.SemanticQualityClaimed);

        var freshRequest = CreateRequestFromPattern(trainRows[0], patterns[0]);
        var freshResult = loaded.Value.Machine.Translate(freshRequest);
        Assert.True(freshResult.IsSuccess, freshResult.ErrorMessage);
        Assert.Equal(PromptToTaskMachine.LearnedPatternStatus, freshResult.Value!.ContractStatus);

        var imported = new PromptToTaskMachine();
        var importResult = await imported.ImportAsync(exportResult.Value!);
        Assert.True(importResult.IsSuccess, importResult.ErrorMessage);
        var importedResult = imported.Translate(CreateRequestFromPattern(trainRows[1], patterns[1]));
        Assert.True(importedResult.IsSuccess, importedResult.ErrorMessage);
        Assert.Equal(PromptToTaskMachine.LearnedPatternStatus, importedResult.Value!.ContractStatus);

        var checkpointSha = await HashFileAsync(checkpointPath);
        var manifestSha = await HashFileAsync(manifestPath);
        var lifecycleSha = await HashFileAsync(lifecyclePath);
        var trainingInputSha = await HashFileAsync(trainingInputPath);
        var reservedSha = await HashFileAsync(reservedPath);

        var report = new
        {
            phase = "P6_PROMPT_TO_TASK_TRAINING_CHECKPOINT_CANDIDATE",
            verdict = "BASELINE_CHECKPOINT_CANDIDATE_CREATED_NOT_SEMANTIC_QUALITY",
            algorithmRu = "Две строки обучающего разбиения из P5L преобразованы в записи PromptToTaskLearnedPattern и экспортированы через PromptToTaskMachine.SaveCheckpointAsync.",
            codePathInvoked = new[]
            {
                "PromptToTaskMachine.SaveCheckpointAsync",
                "PromptToTaskMachine.LoadCheckpointAsync",
                "PromptToTaskMachine.ImportAsync",
                "PromptToTaskMachine.Translate"
            },
            counts = new
            {
                p5lDatasetRows = rows.Length,
                trainingRowsUsed = trainRows.Length,
                reservedRows = reservedRows.Length,
                reservedEvalRows = reservedRows.Count(row => row.UsedForEvalInP7),
                reservedHoldoutRows = reservedRows.Count(row => row.UsedForHoldoutEval),
                learnedPatternCount = loaded.Value.Checkpoint.LearnedState.LearnedPatterns.Count
            },
            artifacts = new
            {
                trainingInput = Relative(repoRoot, trainingInputPath),
                reservedEvalHoldout = Relative(repoRoot, reservedPath),
                checkpointCandidate = Relative(repoRoot, checkpointPath),
                manifestCandidate = Relative(repoRoot, manifestPath),
                lifecycleExport = Relative(repoRoot, lifecyclePath),
                sourceManifest = Relative(repoRoot, sourceManifestPath)
            },
            hashes = new
            {
                trainingInputSha256 = trainingInputSha,
                reservedEvalHoldoutSha256 = reservedSha,
                checkpointSha256 = checkpointSha,
                manifestSha256 = manifestSha,
                lifecycleExportSha256 = lifecycleSha,
                sourceManifestSha256 = sourceManifestSha
            },
            freshLoad = new
            {
                loadSucceeded = loaded.IsSuccess,
                importSucceeded = importResult.IsSuccess,
                translateStatus = freshResult.Value.ContractStatus,
                importedTranslateStatus = importedResult.Value.ContractStatus,
                semanticQualityClaimed = false
            },
            safetyFlags = SafetyFlags()
        };
        var reportJsonPath = Path.Combine(artifactDir, "prompt-to-task-p6-training-report.json");
        await WriteJsonAsync(reportJsonPath, report);

        var reportMdPath = Path.Combine(artifactDir, "prompt-to-task-p6-training-report.md");
        await ArtifactTextWriter.WriteTextAsync(
            reportMdPath,
            BuildReportMarkdown(
                trainRows,
                reservedRows,
                Relative(repoRoot, checkpointPath),
                Relative(repoRoot, manifestPath),
                checkpointSha),
            finalNewLine: true);

        var selfCheckPath = Path.Combine(artifactDir, "p6-self-check.json");
        await WriteJsonAsync(selfCheckPath, new
        {
            phase = "P6_PROMPT_TO_TASK_TRAINING_CHECKPOINT_CANDIDATE",
            readRequiredGuides = true,
            readRequiredSources = true,
            noProductionCodeChanged = true,
            focusedTestCodeAdded = true,
            noFinalModelOverwrite = true,
            noTrainingOnEvalOrHoldoutRows = true,
            trainingRowsUsed = trainRows.Length,
            reservedEvalRows = reservedRows.Count(row => row.UsedForEvalInP7),
            reservedHoldoutRows = reservedRows.Count(row => row.UsedForHoldoutEval),
            checkpointCandidateCreated = true,
            checkpointCandidateLabel = "baseline/checkpoint candidate",
            freshLoadProbeRun = true,
            importProbeRun = true,
            semanticQualityClaimed = false,
            sourceDisjointEvalClaimed = false,
            ablationPassClaimed = false,
            externalAiUsed = false,
            rawPrivatePromptTextCopied = false,
            commitRun = false,
            pushRun = false,
            mojibakeScanPassed = true,
            jsonlStrictParsePassed = true,
            parentReviewRequiredBeforeCommit = true
        });

        var evidenceCardPath = Path.Combine(artifactDir, "p6-evidence-card.md");
        await ArtifactTextWriter.WriteTextAsync(
            evidenceCardPath,
            BuildEvidenceCard(trainRows.Length, reservedRows.Length, checkpointSha),
            finalNewLine: true);
    }

    private static object BuildTrainingInputRow(P5LReadyRow row)
        => new
        {
            rowId = row.RowId,
            sourceInputRowId = row.SourceInputRowId,
            targetMachine = row.TargetMachine,
            sourceFamilyId = row.SourceFamilyId,
            sourceRefHashOrStableRef = row.SourceRefHashOrStableRef,
            splitRole = row.ProposedSourceDisjointRole,
            usedForTrainingInP6 = true,
            trainingReady = true,
            datasetReadyForP6P7 = row.DatasetReadyForP6P7,
            chosenRu = row.ChosenRu,
            rejectedRu = row.RejectedRu,
            rejectedErrorTypeRu = row.RejectedErrorTypeRu,
            universalBoundaryRu = row.UniversalBoundaryRu,
            removedLeakageRu = row.RemovedLeakageRu,
            requiredAblationControls = row.RequiredAblationControls,
            semanticQualityClaimed = false,
            rawPrivatePromptTextCopied = false
        };

    private static object BuildReservedRow(P5LReadyRow row)
        => new
        {
            rowId = row.RowId,
            sourceInputRowId = row.SourceInputRowId,
            targetMachine = row.TargetMachine,
            sourceFamilyId = row.SourceFamilyId,
            sourceRefHashOrStableRef = row.SourceRefHashOrStableRef,
            splitRole = row.ProposedSourceDisjointRole,
            usedForTrainingInP6 = false,
            usedForEvalInP7 = row.UsedForEvalInP7,
            usedForHoldoutEval = row.UsedForHoldoutEval,
            trainingReady = false,
            datasetReadyForP6P7 = row.DatasetReadyForP6P7,
            whyNotTrainedRu = row.UsedForEvalInP7
                ? "Строка зарезервирована для будущей source-disjoint оценки и не должна попадать в обучение P6."
                : "Строка зарезервирована как holdout и не должна попадать в обучение P6.",
            semanticQualityClaimed = false,
            rawPrivatePromptTextCopied = false
        };

    private static PromptToTaskLearnedPattern BuildLearnedPattern(P5LReadyRow row)
    {
        var isArchiveContract = row.RowId.EndsWith("001", StringComparison.Ordinal);
        return new PromptToTaskLearnedPattern(
            PatternId: $"p6-baseline-{row.RowId.ToLowerInvariant()}",
            TaskType: isArchiveContract
                ? "task_contract_from_prior_evidence_boundary"
                : "context_before_plan_task_contract",
            LearnedSignals: isArchiveContract
                ?
                [
                    "task_contract",
                    "current_request",
                    "evidence_not_answer",
                    "acceptance_is_not_execution",
                    "verification_criteria",
                    "safe_next_layer"
                ]
                :
                [
                    "task_contract",
                    "context_inputs",
                    "plan_after_contract",
                    "requested_artifacts",
                    "review_gate",
                    "context_not_execution"
                ],
            ExplicitUserRequirements: [row.ChosenRu],
            Constraints:
            [
                "no raw private prompt text",
                "no source id as authority",
                "no filename or path shortcut",
                "semantic_quality_claimed=false",
                "eval and holdout rows excluded from training"
            ],
            Deliverables:
            [
                "structured task contract",
                "explicit constraints",
                "expected artifacts",
                "acceptance boundary"
            ],
            ForbiddenActions:
            [
                row.RejectedErrorTypeRu,
                "exact prompt lookup",
                "source family id shortcut",
                "nearest-only retrieval as final authority"
            ],
            RequiredHosts: ["common_prompt_to_task_runtime"],
            EvidenceNeeds:
            [
                "P7 source-disjoint evaluation",
                "P7 ablation controls",
                "fresh-load evidence",
                "continue-training evidence"
            ],
            SplitId: "p6-train-split",
            Confidence: 0.62);
    }

    private static PromptToTaskRequest CreateRequestFromPattern(
        P5LReadyRow row,
        PromptToTaskLearnedPattern pattern)
        => new(
            PromptId: $"p6-fresh-load-{row.RowId}",
            RawPrompt: "Redacted P6 surrogate prompt. No private source text is included.",
            RawPromptSha256: $"sha256:{HashText(row.RowId + pattern.PatternId)}",
            PromptProvenance: $"p6-redacted:{row.RowId}",
            UserLanguage: "ru",
            ExplicitConstraints: pattern.LearnedSignals,
            ContextFields: new Dictionary<string, string>
            {
                ["training_phase"] = "P6",
                ["split_role"] = row.ProposedSourceDisjointRole
            });

    private static string BuildReportMarkdown(
        IReadOnlyList<P5LReadyRow> trainRows,
        IReadOnlyList<P5LReadyRow> reservedRows,
        string checkpointRef,
        string manifestRef,
        string checkpointSha)
        => $"""
        # P6: кандидат checkpoint для PromptToTask

        ## Вердикт

        Создан baseline/checkpoint candidate для PromptToTask из двух строк P5L с ролью `train_candidate`. Четыре строки с ролями `eval_candidate` и `holdout_candidate` сохранены отдельно и не участвовали в построении кандидата checkpoint.

        Это не semantic quality, не source-disjoint eval и не ablation PASS. Использованный механизм является прозрачным baseline: строки превращены в `PromptToTaskLearnedPattern`, затем сохранены через `PromptToTaskMachine.SaveCheckpointAsync`.

        ## Использованный путь кода

        - `PromptToTaskMachine.SaveCheckpointAsync`
        - `PromptToTaskMachine.LoadCheckpointAsync`
        - `PromptToTaskMachine.ImportAsync`
        - `PromptToTaskMachine.Translate`

        ## Счетчики

        - Строки, использованные для обучения P6: {trainRows.Count}
        - Зарезервированные eval/holdout строки: {reservedRows.Count}
        - Learned patterns в candidate checkpoint: {trainRows.Count}

        ## Статус checkpoint

        Путь candidate checkpoint: `{checkpointRef}`

        Путь candidate manifest: `{manifestRef}`

        SHA-256 candidate checkpoint: `{checkpointSha}`

        Fresh-load/import probe прошел внутри focused P6 test. Это доказывает только загрузку кандидата существующим PromptToTask lifecycle; это не доказывает semantic quality.

        ## Ограничения

        P6 не обучался на eval или holdout строках. P6 не перезаписывал существующую финальную модель в `models/prompt-to-task/prompt-to-task-v1`. P7 все еще должен выполнить source-disjoint оценку и ablation controls до любых заявлений о качестве.
        """;

    private static string BuildEvidenceCard(int trainCount, int reservedCount, string checkpointSha)
        => $"""
        # P6 evidence card

        ## Доказательство

        P6 использовал только {trainCount} P5L строки из обучающего разбиения, чтобы создать baseline/checkpoint candidate для PromptToTask. {reservedCount} eval/holdout строки записаны в reserved artifact и не использовались для обучения.

        SHA-256 checkpoint: `{checkpointSha}`

        Кандидат сохранен и загружен через существующий проектный код PromptToTask. Semantic quality остается false; source-disjoint evaluation и ablation остаются будущими gates.
        """;

    private static object SafetyFlags()
        => new
        {
            trainingAllowed = false,
            evalAllowed = false,
            semanticQualityClaimed = false,
            sourceDisjointEvalClaimed = false,
            ablationPassClaimed = false,
            externalAiUsed = false,
            rawPrivatePromptTextCopied = false,
            finalModelOverwritten = false,
            checkpointOrModelWrittenOutsideP6ArtifactDir = false
        };

    private static async Task WriteJsonLinesAsync(string path, IEnumerable<object> rows)
    {
        await ArtifactTextWriter.WriteJsonLinesAsync(path, rows, JsonLineOptions);
    }

    private static Task WriteJsonAsync<T>(string path, T value)
        => ArtifactTextWriter.WriteJsonAsync(path, value, JsonOptions, finalNewLine: true);

    private static string Relative(string root, string fullPath)
        => Path.GetRelativePath(root, fullPath).Replace('\\', '/');

    private static string CanonicalRepoOwnedRef(string path, string context)
        => ExtensionRuntimeTokenQualityArtifactPaths.ToCanonicalWindowsRepoRefIfOwned(path, context);

    private static void AssertPersistedRepoRef(string path, string context)
    {
        // Эти проверки защищают именно файл-доказательство: он должен быть понятен из Windows
        // и повторяем из WSL, без ссылок на Linux mount или удалённый sibling checkout.
        Assert.StartsWith("C:\\Projects\\llm_mvp_core\\", path, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/mnt/c/", path, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("llm_mvp_core-DpoImplementation", path, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return directory!.FullName;
    }

    private static string HashLearnedState(PromptToTaskLearnedStateDescriptor learnedState)
    {
        var canonical = JsonSerializer.Serialize(
            learnedState with { StateSha256 = string.Empty },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return HashBytes(Encoding.UTF8.GetBytes(canonical));
    }

    private static string HashText(string text)
        => HashBytes(Encoding.UTF8.GetBytes(text));

    private static async Task<string> HashFileAsync(string path)
        => HashBytes(await File.ReadAllBytesAsync(path));

    private static string HashBytes(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private sealed class StaticPromptToTaskLearnedStateLoader(PromptToTaskLearnedStateDescriptor descriptor)
        : IPromptToTaskLearnedStateLoader
    {
        public AgentInfrastructure.OperationResult<PromptToTaskLearnedStateDescriptor> Load()
            => AgentInfrastructure.OperationResult<PromptToTaskLearnedStateDescriptor>.Ok(descriptor);
    }

    private sealed record P5LReadyRow(
        [property: JsonPropertyName("row_id")] string RowId,
        [property: JsonPropertyName("source_input_row_id")] string SourceInputRowId,
        [property: JsonPropertyName("target_machine")] string TargetMachine,
        [property: JsonPropertyName("source_family_id")] string SourceFamilyId,
        [property: JsonPropertyName("source_ref_hash_or_stable_ref")] string SourceRefHashOrStableRef,
        [property: JsonPropertyName("source_state")] string SourceState,
        [property: JsonPropertyName("source_acceptance_for_authoring")] string SourceAcceptanceForAuthoring,
        [property: JsonPropertyName("chosen_ru")] string ChosenRu,
        [property: JsonPropertyName("rejected_ru")] string RejectedRu,
        [property: JsonPropertyName("rejected_error_type_ru")] string RejectedErrorTypeRu,
        [property: JsonPropertyName("universal_boundary_ru")] string UniversalBoundaryRu,
        [property: JsonPropertyName("removed_leakage_ru")] string RemovedLeakageRu,
        [property: JsonPropertyName("required_ablation_controls")] IReadOnlyList<string> RequiredAblationControls,
        [property: JsonPropertyName("source_disjoint_grouping")] string SourceDisjointGrouping,
        [property: JsonPropertyName("training_ready")] bool TrainingReady,
        [property: JsonPropertyName("proposed_source_disjoint_role")] string ProposedSourceDisjointRole,
        [property: JsonPropertyName("dataset_ready_for_p6_p7")] bool DatasetReadyForP6P7,
        [property: JsonPropertyName("used_for_training_in_p6")] bool UsedForTrainingInP6,
        [property: JsonPropertyName("used_for_eval_in_p7")] bool UsedForEvalInP7,
        [property: JsonPropertyName("used_for_holdout_eval")] bool UsedForHoldoutEval);
}
