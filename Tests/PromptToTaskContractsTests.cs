// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using System.Security.Cryptography;
using SmartSkillsDomain;

public sealed class PromptToTaskContractsTests
{
    [Fact]
    public void PromptToTaskDomain_MarkerLivesInSmartSkillsDomain()
    {
        Assert.Equal("SmartSkillsDomain", typeof(PromptToTaskDomain).Namespace);
    }

    [Fact]
    public void PromptToTaskRequest_CarriesRawPromptIdentityAndContext()
    {
        var request = new PromptToTaskRequest(
            PromptId: "prompt-001",
            RawPrompt: "Create a bounded work order and forbid broad tests.",
            RawPromptSha256: "sha256:prompt-001",
            PromptProvenance: "conversation:current-visible",
            UserLanguage: "en",
            ExplicitConstraints: ["<=15 minutes", "no broad tests"],
            ContextFields: new Dictionary<string, string>
            {
                ["source_root_id"] = "claude-user-prompts",
                ["host"] = "codex"
            });

        Assert.Equal("prompt-001", request.PromptId);
        Assert.Equal("sha256:prompt-001", request.RawPromptSha256);
        Assert.Contains("no broad tests", request.ExplicitConstraints);
        Assert.Equal("claude-user-prompts", request.ContextFields["source_root_id"]);
    }

    [Fact]
    public void StructuredTaskContract_CarriesHandoffFieldsForOrchestrationReasoning()
    {
        var task = new StructuredTaskContract(
            PromptId: "prompt-001",
            RawPromptSha256: "sha256:prompt-001",
            UserLanguage: "en",
            TaskType: "bounded_work_order",
            ExplicitUserRequirements: ["exactly one subphase", "state update required"],
            Constraints: ["no training implementation"],
            Deliverables: ["contract diff", "focused tests"],
            ForbiddenActions: ["external AI", "host repos"],
            RequiredHosts: ["codex"],
            EvidenceNeeds: ["contract tests", "state evidence"],
            SourceProvenance: new Dictionary<string, string>
            {
                ["source_root_id"] = "claude-user-prompts",
                ["prompt_id"] = "prompt-001"
            },
            SplitId: "split-ptt-contract-01");

        Assert.Equal("bounded_work_order", task.TaskType);
        Assert.Contains("state update required", task.ExplicitUserRequirements);
        Assert.Contains("no training implementation", task.Constraints);
        Assert.Contains("host repos", task.ForbiddenActions);
        Assert.Equal("split-ptt-contract-01", task.SplitId);
    }

    [Fact]
    public void PromptToTaskResponse_SerializesContractOnlyHandoff()
    {
        var request = new PromptToTaskRequest(
            PromptId: "prompt-002",
            RawPrompt: "Translate raw prompt into structured task.",
            RawPromptSha256: "sha256:prompt-002",
            PromptProvenance: "corpus:prompt-to-task",
            UserLanguage: "en",
            ExplicitConstraints: ["preserve forbidden actions"],
            ContextFields: new Dictionary<string, string> { ["source_kind"] = "raw_prompt" });
        var task = new StructuredTaskContract(
            PromptId: request.PromptId,
            RawPromptSha256: request.RawPromptSha256,
            UserLanguage: request.UserLanguage,
            TaskType: "contract_probe",
            ExplicitUserRequirements: ["preserve prompt identity"],
            Constraints: request.ExplicitConstraints,
            Deliverables: ["structured task"],
            ForbiddenActions: ["deterministic classifier"],
            RequiredHosts: ["claude", "cursor", "codex"],
            EvidenceNeeds: ["round-trip serialization"],
            SourceProvenance: new Dictionary<string, string>
            {
                ["prompt_provenance"] = request.PromptProvenance
            },
            SplitId: "split-serialization");
        var response = new PromptToTaskResponse(
            ResponseId: "response-002",
            Request: request,
            StructuredTask: task,
            ContractEvidence: ["contract-only", "no classifier implementation"],
            ContractStatus: "contract_only_no_training");

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var roundTrip = JsonSerializer.Deserialize<PromptToTaskResponse>(
            JsonSerializer.Serialize(response, options),
            options);

        Assert.NotNull(roundTrip);
        Assert.Equal(response.ResponseId, roundTrip!.ResponseId);
        Assert.Equal(response.Request.RawPrompt, roundTrip.Request.RawPrompt);
        Assert.Equal(response.StructuredTask.TaskType, roundTrip.StructuredTask.TaskType);
        Assert.Contains("no classifier implementation", roundTrip.ContractEvidence);
        Assert.Equal("contract_only_no_training", roundTrip.ContractStatus);
    }

    [Fact]
    public void PromptToTaskTaskPatternDpoSignalBuilder_BuildsProjectLocalSignalFromExplicitTasks()
    {
        var chosen = CreateStructuredTask("bounded_work_order");
        var rejected = CreateStructuredTask(PromptToTaskMachine.NotReadyTaskType);
        var request = new PromptToTaskTaskPatternDpoRequest(
            PairId: "ptt-dpo-pair-001",
            SourcePromptLedgerId: "ups-ptt-001",
            ChosenTask: chosen,
            RejectedTask: rejected,
            Reason: "User correction selected a concrete task pattern instead of the not-ready placeholder.",
            Metadata: new Dictionary<string, string>
            {
                ["correction_source"] = "user_visible_review"
            });

        var result = new PromptToTaskTaskPatternDpoSignalBuilder().Build(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("ptt-dpo-pair-001", result.Value!.PairId);
        Assert.Equal("sha256:prompt-dpo-001", result.Value.RawPromptSha256);
        Assert.Equal("bounded_work_order", result.Value.ChosenTask.TaskType);
        Assert.Equal(PromptToTaskMachine.NotReadyTaskType, result.Value.RejectedTask.TaskType);
        Assert.Equal("IPromptToTaskTaskPatternDpoTraining", result.Value.TargetTrainingAdapter);
        Assert.Equal("project-local:.xiigen/prompt-to-task/dpo-signals.jsonl", result.Value.StorageScope);
        Assert.Equal("explicit_task_pattern_feedback_signal", result.Value.RouteMode);
        Assert.False(result.Value.SemanticQualityClaimed);
        Assert.False(result.Value.ContainsRawPromptText);
    }

    [Fact]
    public void PromptToTaskTaskPatternDpoSignalBuilder_RejectsMismatchedPromptHashes()
    {
        var chosen = CreateStructuredTask("bounded_work_order");
        var rejected = CreateStructuredTask(PromptToTaskMachine.NotReadyTaskType) with
        {
            RawPromptSha256 = "sha256:other-prompt"
        };
        var request = new PromptToTaskTaskPatternDpoRequest(
            PairId: "ptt-dpo-pair-002",
            SourcePromptLedgerId: "ups-ptt-002",
            ChosenTask: chosen,
            RejectedTask: rejected,
            Reason: "Mismatched prompt identities must not be converted into DPO.",
            Metadata: new Dictionary<string, string>());

        var result = new PromptToTaskTaskPatternDpoSignalBuilder().Build(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("PROMPT_TO_TASK_DPO_PROMPT_HASH_MISMATCH", result.ErrorCode);
    }

    [Fact]
    public void PromptToTaskAdaptiveDpoSignalWriter_WritesUserLocalRefsWithoutCommonCheckpointMutation()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"ptt-adaptive-{Guid.NewGuid():N}");
        var commonCheckpointPath = FindRepoFile(
            "models",
            "prompt-to-task",
            "prompt-to-task-v1",
            "prompt-to-task-v1-checkpoint.json");
        var commonCheckpointBefore = HashFile(commonCheckpointPath);
        var rawPrompt = "Private prompt text must not be persisted in the adaptive signal.";
        var request = new PromptToTaskAdaptiveDpoSignalWriteRequest(
            UserLocalRoot: tempRoot,
            SignalId: "ptt-adaptive-signal-001",
            SourcePromptLedgerId: "ups-ptt-adaptive-001",
            RawPromptSha256: "sha256:prompt-adaptive-001",
            ChosenStructuredTaskRef: "structured-task-ref:chosen:001",
            RejectedStructuredTaskRef: "structured-task-ref:rejected:001",
            FeedbackClass: "task_pattern_correction",
            ProvenanceRefs: new Dictionary<string, string>
            {
                ["prompt_ref"] = "prompt-ref:sha256:prompt-adaptive-001",
                ["source_kind"] = "user_correction_redacted"
            });

        try
        {
            var result = new PromptToTaskAdaptiveDpoSignalWriter().Write(request);

            Assert.True(result.IsSuccess);
            Assert.EndsWith(Path.Combine(".xiigen", "prompt-to-task", "adaptive-dpo-signals.jsonl"), result.Value!.SignalPath);
            Assert.True(File.Exists(result.Value.SignalPath));
            Assert.Equal("project-local:.xiigen/prompt-to-task/adaptive-dpo-signals.jsonl", result.Value.Signal.StorageScope);
            Assert.Equal("structured-task-ref:chosen:001", result.Value.Signal.ChosenStructuredTaskRef);
            Assert.Equal("structured-task-ref:rejected:001", result.Value.Signal.RejectedStructuredTaskRef);
            Assert.Equal("task_pattern_correction", result.Value.Signal.FeedbackClass);
            Assert.Equal("IPromptToTaskAdaptiveDpoTraining", result.Value.Signal.TargetTrainingAdapter);
            Assert.False(result.Value.Signal.ContainsRawPromptText);
            Assert.False(result.Value.Signal.SemanticQualityClaimed);

            var persistedText = File.ReadAllText(result.Value.SignalPath);
            Assert.Contains("chosenStructuredTaskRef", persistedText);
            Assert.Contains("rejectedStructuredTaskRef", persistedText);
            Assert.Contains("prompt-ref:sha256:prompt-adaptive-001", persistedText);
            Assert.DoesNotContain(rawPrompt, persistedText);
            Assert.DoesNotContain("Private prompt text", persistedText);
            Assert.Equal(commonCheckpointBefore, HashFile(commonCheckpointPath));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void PromptToTaskAdaptiveTrainer_TrainsUserLocalCheckpointAndSkipsReplayWithoutCommonCheckpointMutation()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"ptt-adaptive-train-{Guid.NewGuid():N}");
        var commonCheckpointPath = FindRepoFile(
            "models",
            "prompt-to-task",
            "prompt-to-task-v1",
            "prompt-to-task-v1-checkpoint.json");
        var commonCheckpointBefore = HashFile(commonCheckpointPath);
        var checkpointPath = Path.Combine(
            tempRoot,
            ".xiigen",
            "prompt-to-task",
            "adaptive-checkpoint.json");
        var reportPath = Path.Combine(
            tempRoot,
            ".xiigen",
            "prompt-to-task",
            "adaptive-training-report.json");

        try
        {
            var writeResult = new PromptToTaskAdaptiveDpoSignalWriter().Write(
                new PromptToTaskAdaptiveDpoSignalWriteRequest(
                    UserLocalRoot: tempRoot,
                    SignalId: "ptt-adaptive-signal-train-001",
                    SourcePromptLedgerId: "ups-ptt-train-001",
                    RawPromptSha256: "sha256:prompt-train-001",
                    ChosenStructuredTaskRef: "structured-task-ref:chosen:train-001",
                    RejectedStructuredTaskRef: "structured-task-ref:rejected:train-001",
                    FeedbackClass: "task-pattern-correction",
                    ProvenanceRefs: new Dictionary<string, string>
                    {
                        ["prompt_ref"] = "prompt-ref:sha256:prompt-train-001",
                        ["source"] = "unit-test"
                    }));
            Assert.True(writeResult.IsSuccess);

            var trainer = new PromptToTaskAdaptiveTrainer();
            var trainResult = trainer.TrainUserLocal(
                new PromptToTaskAdaptiveTrainingRequest(
                    UserLocalRoot: tempRoot,
                    CheckpointPath: checkpointPath,
                    ReportPath: reportPath));

            Assert.True(trainResult.IsSuccess);
            Assert.True(File.Exists(checkpointPath));
            Assert.True(File.Exists(reportPath));
            Assert.Single(trainResult.Value!.Checkpoint.TaskProfiles);
            Assert.Contains("ptt-adaptive-signal-train-001", trainResult.Value.Checkpoint.TrainedSignalIds);
            Assert.False(trainResult.Value.Checkpoint.SemanticQualityClaimed);
            Assert.Equal(0, trainResult.Value.Report.MetricBefore);
            Assert.Equal(1, trainResult.Value.Report.MetricAfter);
            Assert.True(trainResult.Value.Report.MetricImproved);
            Assert.False(trainResult.Value.Report.NoOp);
            Assert.False(trainResult.Value.Report.CommonCheckpointMutated);

            var loaded = trainer.LoadCheckpoint(checkpointPath);
            Assert.True(loaded.IsSuccess);
            Assert.Contains("ptt-adaptive-signal-train-001", loaded.Value!.TrainedSignalIds);

            var replayResult = trainer.TrainUserLocal(
                new PromptToTaskAdaptiveTrainingRequest(
                    UserLocalRoot: tempRoot,
                    CheckpointPath: checkpointPath,
                    ReportPath: reportPath,
                    PreviousCheckpointPath: checkpointPath));

            Assert.True(replayResult.IsSuccess);
            Assert.True(replayResult.Value!.Report.NoOp);
            Assert.Equal("All eligible PromptToTask adaptive DPO signal rows were already trained; replay skipped.",
                replayResult.Value.Report.NoOpReason);
            Assert.Equal(1, replayResult.Value.Report.MetricBefore);
            Assert.Equal(1, replayResult.Value.Report.MetricAfter);
            Assert.False(replayResult.Value.Report.MetricImproved);
            Assert.Contains("ptt-adaptive-signal-train-001", replayResult.Value.Checkpoint.TrainedSignalIds);
            Assert.False(replayResult.Value.Report.CommonCheckpointMutated);
            Assert.Equal(commonCheckpointBefore, HashFile(commonCheckpointPath));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void PromptToTaskTrainingPairContract_UsesRefsHashesTaxonomyAndHeldoutSplit()
    {
        var pair = new PromptToTaskTrainingPairContract(
            TrainingPairId: "ptt-pair-001",
            SourceRowId: "source-row:ptt:001",
            ApprovedPairRef: "approved-pairs/ptt/001",
            ChosenRef: "dpo/ptt/chosen/001",
            RejectedRef: "dpo/ptt/rejected/001",
            ErrorCode: "PromptError-04",
            ErrorTaxonomyRef: "docs/strategies/prompt-to-task-dpo-negative-taxonomy",
            SourcePromptSha256: "sha256:prompt-001",
            SourceManifestRef: "training-data/prompt-to-task/manifest.json",
            SplitId: "split-ptt-v1",
            SplitRole: "heldout",
            HeldOutSetId: "heldout-ptt-v1",
            NearMissStrategyRef: "PROMPT-TO-TASK-DPO-NEGATIVE-STRATEGY-v1",
            ContainsRawPrivateText: false,
            ProvenanceRefs: new Dictionary<string, string>
            {
                ["source_root_ref"] = "claude-sessions/user-prompts:redacted",
                ["license_scope"] = "project-local-redacted"
            });

        Assert.Equal("source-row:ptt:001", pair.SourceRowId);
        Assert.Equal("approved-pairs/ptt/001", pair.ApprovedPairRef);
        Assert.Equal("dpo/ptt/chosen/001", pair.ChosenRef);
        Assert.Equal("dpo/ptt/rejected/001", pair.RejectedRef);
        Assert.Equal("PromptError-04", pair.ErrorCode);
        Assert.Equal("heldout", pair.SplitRole);
        Assert.False(pair.ContainsRawPrivateText);
        Assert.DoesNotContain(
            typeof(PromptToTaskTrainingPairContract).GetProperties(),
            property => string.Equals(property.Name, "RawPrompt", StringComparison.Ordinal)
                || property.Name.Contains("PromptText", StringComparison.Ordinal));
    }

    [Fact]
    public void PromptToTaskCheckpointStateContract_CarriesLifecycleRefsAndCounts()
    {
        var state = new PromptToTaskCheckpointStateContract(
            LearnedStateId: "prompt-to-task-state:v1:001",
            CheckpointId: "checkpoint:prompt-to-task:v1:001",
            CheckpointSha256: "sha256:checkpoint-001",
            SchemaVersion: "prompt-to-task-state-v1",
            SourceRowCount: 120,
            DpoPairCount: 64,
            SourceManifestRef: "training-data/prompt-to-task/manifest.json",
            ModelStateRef: "models/prompt-to-task/model-state.json",
            TrainingStateRef: "models/prompt-to-task/training-state.json",
            ExportManifestRef: "models/prompt-to-task/export-manifest.json",
            FreshLoadProbeRef: "reports/prompt-to-task/fresh-load.json",
            ContinueTrainingStateRef: "reports/prompt-to-task/continue-training.json",
            ProvenanceRefs: new Dictionary<string, string>
            {
                ["base_commit"] = "commit:example",
                ["source_manifest_sha256"] = "sha256:manifest-001"
            });

        Assert.Equal("checkpoint:prompt-to-task:v1:001", state.CheckpointId);
        Assert.Equal("sha256:checkpoint-001", state.CheckpointSha256);
        Assert.Equal("prompt-to-task-state-v1", state.SchemaVersion);
        Assert.Equal(120, state.SourceRowCount);
        Assert.Equal(64, state.DpoPairCount);
        Assert.Equal("models/prompt-to-task/training-state.json", state.TrainingStateRef);
        Assert.Equal("reports/prompt-to-task/fresh-load.json", state.FreshLoadProbeRef);
        Assert.Equal("reports/prompt-to-task/continue-training.json", state.ContinueTrainingStateRef);
    }

    [Fact]
    public void PromptToTaskTaskPatternUnit_EvaluatesHeldOutRowsWithoutSemanticQualityClaim()
    {
        var state = new PromptToTaskTaskPatternUnitState(
            SchemaVersion: 1,
            StateId: "ptt-task-pattern-unit-state:test",
            CheckpointRef: "artifacts/prompt-to-task/checkpoints/common/ptt-task-pattern-unit-v1.json",
            ManifestRef: "artifacts/prompt-to-task/manifests/ptt-task-pattern-unit-v1.manifest.json",
            Patterns:
            [
                new PromptToTaskTaskPatternUnitPattern(
                    PatternId: "ptt-unit-pattern-implementation",
                    TaskType: "bounded_implementation_slice",
                    LearnedSignals: ["bounded_subphase", "work_order:PTT-UNIT"],
                    ExplicitUserRequirements: ["implement scoped production behavior"],
                    Constraints: ["no host repos"],
                    Deliverables: ["focused test evidence"],
                    ForbiddenActions: ["training runs"],
                    RequiredHosts: ["llm_mvp_core"],
                    EvidenceNeeds: ["held-out eval report"],
                    SplitId: "heldout-ptt-unit",
                    Confidence: 0.91,
                    AcceptedPairId: "ptt-pair-accepted-001")
            ],
            AcceptedPairIds: ["ptt-pair-accepted-001"],
            QuarantinedPairIds: ["ptt-pair-quarantined-001"],
            MinimumConfidence: 0.2,
            MinimumTopTwoMargin: 0.05,
            SemanticQualityClaimed: false);
        var unit = new PromptToTaskTaskPatternUnit(state);

        var nearest = unit.FindNearestPromptToTaskPatterns(
            new PromptToTaskNearestQuery(
                PromptId: "prompt-unit-001",
                RawPromptSha256: "sha256:prompt-unit-001",
                PromptSignals: ["bounded_subphase"],
                ContextFields: new Dictionary<string, string> { ["work_order"] = "PTT-UNIT" }),
            k: 3);
        var report = unit.EvaluateHeldOutRows(
        [
            new PromptToTaskTaskPatternEvaluationRow(
                RowId: "ptt-heldout-row-001",
                Request: CreateUnitRequest("ptt-heldout-001", ["bounded_subphase"], "PTT-UNIT"),
                ExpectedPatternId: "ptt-unit-pattern-implementation",
                ExpectedTaskType: "bounded_implementation_slice"),
            new PromptToTaskTaskPatternEvaluationRow(
                RowId: "ptt-heldout-row-002",
                Request: CreateUnitRequest("ptt-heldout-002", ["unmatched"], "OTHER"),
                ExpectedPatternId: "ptt-unit-pattern-implementation",
                ExpectedTaskType: "bounded_implementation_slice")
        ]);

        Assert.Single(nearest);
        Assert.Equal("ptt-unit-pattern-implementation", nearest[0].PatternId);
        Assert.False(nearest[0].SemanticQualityClaimed);
        Assert.Equal(2, report.HeldOutRowCount);
        Assert.Equal(1, report.CorrectCount);
        Assert.Equal(0.5, report.Accuracy);
        Assert.False(report.PassesThresholds);
        Assert.False(report.SemanticQualityClaimed);
        Assert.Contains("below the PromptToTaskTaskPatternUnit threshold", report.BlockReason);
        Assert.Contains(report.DeterministicControlNotes, note => note.Contains("transparent baseline", StringComparison.OrdinalIgnoreCase));
        Assert.False(state.SemanticQualityClaimed);
        Assert.Contains("ptt-pair-accepted-001", state.AcceptedPairIds);
        Assert.Contains("ptt-pair-quarantined-001", state.QuarantinedPairIds);
    }

    [Fact]
    public void PromptToTaskContracts_DoNotExposeDeterministicClassifierOrLookupImplementation()
    {
        var promptToTaskTypes = typeof(PromptToTaskDomain).Assembly.GetTypes()
            .Where(type => type.Namespace == "SmartSkillsDomain"
                && type.Name.Contains("PromptToTask", StringComparison.Ordinal))
            .Select(type => type.Name)
            .ToArray();

        Assert.Contains(nameof(PromptToTaskDomain), promptToTaskTypes);
        Assert.Contains(nameof(PromptToTaskRequest), promptToTaskTypes);
        Assert.Contains(nameof(PromptToTaskResponse), promptToTaskTypes);
        Assert.Contains(nameof(PromptToTaskTaskPatternDpoSignal), promptToTaskTypes);
        Assert.Contains(nameof(PromptToTaskTrainingPairContract), promptToTaskTypes);
        Assert.Contains(nameof(PromptToTaskCheckpointStateContract), promptToTaskTypes);
        Assert.Contains(nameof(PromptToTaskAdaptiveTrainer), promptToTaskTypes);
        Assert.DoesNotContain(promptToTaskTypes, name => name.Contains("Classifier", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(promptToTaskTypes, name => name.Contains("Lookup", StringComparison.OrdinalIgnoreCase));
    }

    private static StructuredTaskContract CreateStructuredTask(string taskType)
        => new(
            PromptId: "prompt-dpo-001",
            RawPromptSha256: "sha256:prompt-dpo-001",
            UserLanguage: "en",
            TaskType: taskType,
            ExplicitUserRequirements: ["exactly one bounded work order"],
            Constraints: ["no broad tests"],
            Deliverables: ["DPO task-pattern signal"],
            ForbiddenActions: ["deterministic classifier", "external AI"],
            RequiredHosts: ["codex"],
            EvidenceNeeds: ["focused contract test"],
            SourceProvenance: new Dictionary<string, string>
            {
                ["prompt_provenance"] = "conversation:visible-current",
                ["prompt_id"] = "prompt-dpo-001"
            },
            SplitId: $"split-{taskType}");

    private static PromptToTaskRequest CreateUnitRequest(
        string promptId,
        IReadOnlyList<string> constraints,
        string workOrder)
        => new(
            PromptId: promptId,
            RawPrompt: $"redacted prompt {promptId}",
            RawPromptSha256: $"sha256:{promptId}",
            PromptProvenance: "contract-test:redacted",
            UserLanguage: "en",
            ExplicitConstraints: constraints,
            ContextFields: new Dictionary<string, string>
            {
                ["work_order"] = workOrder
            });

    private static string HashFile(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static string FindRepoFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(segments).ToArray());
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate repository file.", Path.Combine(segments));
    }
}
