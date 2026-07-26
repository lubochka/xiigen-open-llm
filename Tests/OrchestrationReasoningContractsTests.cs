// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using System.Security.Cryptography;
using SmartSkillsDomain;

public sealed class OrchestrationReasoningContractsTests
{
    [Fact]
    public void OrchestrationReasoningDomain_MarkerLivesInSmartSkillsDomain()
    {
        Assert.Equal("SmartSkillsDomain", typeof(OrchestrationReasoningDomain).Namespace);
    }

    [Fact]
    public void OrchestrationReasoningRequest_ConsumesStructuredTaskAndContextRefs()
    {
        var task = CreateStructuredTask();
        var request = new OrchestrationReasoningRequest(
            RequestId: "orch-request-001",
            Task: task,
            GuideDerivedStateRefs: ["guide-state:common:v1"],
            CorpusBundleIds: ["corpus:sonnet-local-analyzes"],
            AdaptiveContextRefs: new Dictionary<string, string>
            {
                ["project_local_state"] = "adaptive:project-001"
            },
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["external_ai"] = "approval-required",
                ["deterministic_shortcut"] = "forbidden"
            });

        Assert.Equal(task, request.Task);
        Assert.Contains("guide-state:common:v1", request.GuideDerivedStateRefs);
        Assert.Contains("corpus:sonnet-local-analyzes", request.CorpusBundleIds);
        Assert.Equal("adaptive:project-001", request.AdaptiveContextRefs["project_local_state"]);
        Assert.Equal("forbidden", request.RuntimePolicyRefs["deterministic_shortcut"]);
    }

    [Fact]
    public void OrchestrationScenarioPlan_CarriesPlanFamiliesAndRiskFlags()
    {
        var task = CreateStructuredTask();
        var plan = new OrchestrationScenarioPlan(
            ScenarioId: "scenario-001",
            Task: task,
            SelectedLearnedPatterns: ["bounded-work-order-pattern"],
            PhaseFamilies: ["contract", "validation"],
            GateFamilies: ["filtered-dotnet-test"],
            AssetFamilies: ["common", "adaptive"],
            HostCheckSets: ["claude", "cursor", "codex"],
            RiskFlags: ["no-training-implementation", "no-deterministic-replay"],
            NextGranularizationActions: ["split-handoff-test"]);

        Assert.Equal(task, plan.Task);
        Assert.Contains("bounded-work-order-pattern", plan.SelectedLearnedPatterns);
        Assert.Contains("contract", plan.PhaseFamilies);
        Assert.Contains("filtered-dotnet-test", plan.GateFamilies);
        Assert.Contains("adaptive", plan.AssetFamilies);
        Assert.Equal(["claude", "cursor", "codex"], plan.HostCheckSets);
        Assert.Contains("split-handoff-test", plan.NextGranularizationActions);
    }

    [Fact]
    public void OrchestrationReasoningResponse_SerializesContractOnlyScenarioPlan()
    {
        var task = CreateStructuredTask();
        var request = new OrchestrationReasoningRequest(
            RequestId: "orch-request-002",
            Task: task,
            GuideDerivedStateRefs: ["guide-state:common:v2"],
            CorpusBundleIds: ["corpus:orchestration-reasoning"],
            AdaptiveContextRefs: new Dictionary<string, string> { ["user"] = "adaptive:user-redacted" },
            RuntimePolicyRefs: new Dictionary<string, string> { ["host_boundary"] = "thin-client" });
        var plan = new OrchestrationScenarioPlan(
            ScenarioId: "scenario-002",
            Task: task,
            SelectedLearnedPatterns: ["prompt-to-task-handoff"],
            PhaseFamilies: ["handoff"],
            GateFamilies: ["contract-serialization"],
            AssetFamilies: ["manifest"],
            HostCheckSets: ["codex"],
            RiskFlags: ["contract-only"],
            NextGranularizationActions: ["create-handoff-fixture"]);
        var response = new OrchestrationReasoningResponse(
            ResponseId: "orch-response-002",
            Request: request,
            ScenarioPlan: plan,
            ContractEvidence: ["contract-only", "no scenario selector implementation"],
            ContractStatus: "contract_only_no_training");

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var roundTrip = JsonSerializer.Deserialize<OrchestrationReasoningResponse>(
            JsonSerializer.Serialize(response, options),
            options);

        Assert.NotNull(roundTrip);
        Assert.Equal(response.ResponseId, roundTrip!.ResponseId);
        Assert.Equal(response.Request.Task.PromptId, roundTrip.Request.Task.PromptId);
        Assert.Equal(response.ScenarioPlan.ScenarioId, roundTrip.ScenarioPlan.ScenarioId);
        Assert.Contains("no scenario selector implementation", roundTrip.ContractEvidence);
        Assert.Equal("contract_only_no_training", roundTrip.ContractStatus);
    }

    [Fact]
    public void OrchestrationReasoningPlanPatternDpoSignalBuilder_BuildsProjectLocalSignalFromExplicitPlans()
    {
        var chosen = CreateScenarioPlan("scenario-corrected", ["bounded-work-order-pattern"]);
        var rejected = CreateScenarioPlan(OrchestrationReasoningMachine.NoInferenceScenarioId, []);
        var request = new OrchestrationReasoningPlanPatternDpoRequest(
            PairId: "orch-dpo-pair-001",
            SourcePromptLedgerId: "ups-orch-001",
            ChosenPlan: chosen,
            RejectedPlan: rejected,
            Reason: "User correction selected a concrete orchestration pattern instead of the no-plan placeholder.",
            Metadata: new Dictionary<string, string>
            {
                ["correction_source"] = "user_visible_review"
            });

        var result = new OrchestrationReasoningPlanPatternDpoSignalBuilder().Build(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("orch-dpo-pair-001", result.Value!.PairId);
        Assert.Equal("sha256:prompt-orch-001", result.Value.RawPromptSha256);
        Assert.Equal("scenario-corrected", result.Value.ChosenPlan.ScenarioId);
        Assert.Equal(OrchestrationReasoningMachine.NoInferenceScenarioId, result.Value.RejectedPlan.ScenarioId);
        Assert.Equal("IOrchestrationReasoningPlanPatternDpoTraining", result.Value.TargetTrainingAdapter);
        Assert.Equal("project-local:.xiigen/orchestration-reasoning/dpo-signals.jsonl", result.Value.StorageScope);
        Assert.Equal("explicit_plan_pattern_feedback_signal", result.Value.RouteMode);
        Assert.False(result.Value.SemanticQualityClaimed);
        Assert.False(result.Value.ContainsRawPromptText);
    }

    [Fact]
    public void OrchestrationReasoningPlanPatternDpoSignalBuilder_RejectsMismatchedPromptHashes()
    {
        var chosen = CreateScenarioPlan("scenario-corrected", ["bounded-work-order-pattern"]);
        var rejected = CreateScenarioPlan(OrchestrationReasoningMachine.NoInferenceScenarioId, []) with
        {
            Task = CreateStructuredTask() with { RawPromptSha256 = "sha256:other-prompt" }
        };
        var request = new OrchestrationReasoningPlanPatternDpoRequest(
            PairId: "orch-dpo-pair-002",
            SourcePromptLedgerId: "ups-orch-002",
            ChosenPlan: chosen,
            RejectedPlan: rejected,
            Reason: "Mismatched prompt identities must not be converted into DPO.",
            Metadata: new Dictionary<string, string>());

        var result = new OrchestrationReasoningPlanPatternDpoSignalBuilder().Build(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("ORCHESTRATION_REASONING_DPO_PROMPT_HASH_MISMATCH", result.ErrorCode);
    }

    [Fact]
    public void OrchestrationReasoningAdaptiveDpoSignalWriter_WritesUserLocalRefsWithoutCommonCheckpointMutation()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"or-adaptive-{Guid.NewGuid():N}");
        var commonCheckpointPath = FindRepoFile(
            "models",
            "orchestration-reasoning",
            "orchestration-reasoning-v1",
            "checkpoint.json");
        var commonCheckpointBefore = HashFile(commonCheckpointPath);
        var rawPrompt = "Private orchestration prompt text must not be persisted.";
        var rawAnalysis = "Private chain analysis must not be persisted.";
        var request = new OrchestrationReasoningAdaptiveDpoSignalWriteRequest(
            UserLocalRoot: tempRoot,
            SignalId: "or-adaptive-signal-001",
            SourcePromptLedgerId: "ups-or-adaptive-001",
            RawPromptSha256: "sha256:prompt-or-adaptive-001",
            ChosenScenarioPlanRef: "scenario-plan-ref:chosen:001",
            RejectedScenarioPlanRef: "scenario-plan-ref:rejected:001",
            FeedbackClass: "plan_pattern_correction",
            RedactedPromptRefs: new Dictionary<string, string>
            {
                ["prompt_ref"] = "prompt-ref:sha256:prompt-or-adaptive-001"
            },
            RedactedAnalysisRefs: new Dictionary<string, string>
            {
                ["analysis_ref"] = "analysis-ref:sha256:analysis-or-adaptive-001"
            },
            ProvenanceRefs: new Dictionary<string, string>
            {
                ["source_kind"] = "user_correction_redacted",
                ["scenario_review_ref"] = "scenario-review-ref:or-adaptive-001"
            });

        try
        {
            var result = new OrchestrationReasoningAdaptiveDpoSignalWriter().Write(request);

            Assert.True(result.IsSuccess);
            Assert.EndsWith(Path.Combine(".xiigen", "orchestration-reasoning", "adaptive-dpo-signals.jsonl"), result.Value!.SignalPath);
            Assert.True(File.Exists(result.Value.SignalPath));
            Assert.Equal("user-local:.xiigen/orchestration-reasoning/adaptive-dpo-signals.jsonl", result.Value.Signal.StorageScope);
            Assert.Equal("scenario-plan-ref:chosen:001", result.Value.Signal.ChosenRef);
            Assert.Equal("scenario-plan-ref:rejected:001", result.Value.Signal.RejectedRef);
            Assert.Equal("plan_pattern_correction", result.Value.Signal.FeedbackClass);
            Assert.Equal("IOrchestrationReasoningAdaptiveDpoTraining", result.Value.Signal.TargetTrainingAdapter);
            Assert.False(result.Value.Signal.ContainsRawPromptText);
            Assert.False(result.Value.Signal.ContainsRawAnalysisText);
            Assert.False(result.Value.Signal.SemanticQualityClaimed);

            var persistedText = File.ReadAllText(result.Value.SignalPath);
            Assert.Contains("chosenRef", persistedText);
            Assert.Contains("rejectedRef", persistedText);
            Assert.Contains("prompt-ref:sha256:prompt-or-adaptive-001", persistedText);
            Assert.Contains("analysis-ref:sha256:analysis-or-adaptive-001", persistedText);
            Assert.DoesNotContain(rawPrompt, persistedText);
            Assert.DoesNotContain(rawAnalysis, persistedText);
            Assert.DoesNotContain("Private orchestration prompt text", persistedText);
            Assert.DoesNotContain("Private chain analysis", persistedText);
            Assert.Equal(commonCheckpointBefore, HashFile(commonCheckpointPath));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void OrchestrationReasoningAdaptiveTrainer_TrainsUserLocalCheckpointAndSkipsReplayWithoutCommonCheckpointMutation()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"or-adaptive-train-{Guid.NewGuid():N}");
        var commonCheckpointPath = FindRepoFile(
            "models",
            "orchestration-reasoning",
            "orchestration-reasoning-v1",
            "checkpoint.json");
        var commonCheckpointBefore = HashFile(commonCheckpointPath);
        var checkpointPath = Path.Combine(
            tempRoot,
            ".xiigen",
            "orchestration-reasoning",
            "adaptive-checkpoint.json");
        var reportPath = Path.Combine(
            tempRoot,
            ".xiigen",
            "orchestration-reasoning",
            "adaptive-training-report.json");

        try
        {
            var writeResult = new OrchestrationReasoningAdaptiveDpoSignalWriter().Write(
                new OrchestrationReasoningAdaptiveDpoSignalWriteRequest(
                    UserLocalRoot: tempRoot,
                    SignalId: "or-adaptive-signal-train-001",
                    SourcePromptLedgerId: "ups-or-train-001",
                    RawPromptSha256: "sha256:prompt-or-train-001",
                    ChosenScenarioPlanRef: "scenario-plan-ref:chosen:train-001",
                    RejectedScenarioPlanRef: "scenario-plan-ref:rejected:train-001",
                    FeedbackClass: "plan-pattern-correction",
                    RedactedPromptRefs: new Dictionary<string, string>
                    {
                        ["prompt_ref"] = "prompt-ref:sha256:prompt-or-train-001"
                    },
                    RedactedAnalysisRefs: new Dictionary<string, string>
                    {
                        ["analysis_ref"] = "analysis-ref:sha256:analysis-or-train-001"
                    },
                    ProvenanceRefs: new Dictionary<string, string>
                    {
                        ["source"] = "unit-test",
                        ["review_ref"] = "scenario-review-ref:or-train-001"
                    }));
            Assert.True(writeResult.IsSuccess);

            var trainer = new OrchestrationReasoningAdaptiveTrainer();
            var trainResult = trainer.TrainUserLocal(
                new OrchestrationReasoningAdaptiveTrainingRequest(
                    UserLocalRoot: tempRoot,
                    CheckpointPath: checkpointPath,
                    ReportPath: reportPath));

            Assert.True(trainResult.IsSuccess);
            Assert.True(File.Exists(checkpointPath));
            Assert.True(File.Exists(reportPath));
            Assert.Single(trainResult.Value!.Checkpoint.ScenarioPlanProfiles);
            Assert.Contains("or-adaptive-signal-train-001", trainResult.Value.Checkpoint.TrainedSignalIds);
            Assert.False(trainResult.Value.Checkpoint.SemanticQualityClaimed);
            Assert.Equal(0, trainResult.Value.Report.MetricBefore);
            Assert.Equal(1, trainResult.Value.Report.MetricAfter);
            Assert.True(trainResult.Value.Report.MetricImproved);
            Assert.False(trainResult.Value.Report.NoOp);
            Assert.False(trainResult.Value.Report.CommonCheckpointMutated);

            var loaded = trainer.LoadCheckpoint(checkpointPath);
            Assert.True(loaded.IsSuccess);
            Assert.Contains("or-adaptive-signal-train-001", loaded.Value!.TrainedSignalIds);

            var replayResult = trainer.TrainUserLocal(
                new OrchestrationReasoningAdaptiveTrainingRequest(
                    UserLocalRoot: tempRoot,
                    CheckpointPath: checkpointPath,
                    ReportPath: reportPath,
                    PreviousCheckpointPath: checkpointPath));

            Assert.True(replayResult.IsSuccess);
            Assert.True(replayResult.Value!.Report.NoOp);
            Assert.Equal("All eligible OrchestrationReasoning adaptive DPO signal rows were already trained; replay skipped.",
                replayResult.Value.Report.NoOpReason);
            Assert.Equal(1, replayResult.Value.Report.MetricBefore);
            Assert.Equal(1, replayResult.Value.Report.MetricAfter);
            Assert.False(replayResult.Value.Report.MetricImproved);
            Assert.Contains("or-adaptive-signal-train-001", replayResult.Value.Checkpoint.TrainedSignalIds);
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
    public void OrchestrationReasoningTrainingPairContract_CarriesRefOnlyDpoBindingFields()
    {
        var pair = new OrchestrationReasoningTrainingPairContract(
            TrainingPairId: "or-training-pair-001",
            SourceRowId: "or-source-row-001",
            ApprovedPairRef: "approved-pair:or-001",
            ChosenScenarioRef: "scenario-ref:chosen:or-001",
            RejectedScenarioRef: "scenario-ref:rejected:or-001",
            ErrorCode: "OR_NEAR_MISS_MISSING_GATE",
            ErrorTaxonomyRef: "taxonomy:orchestration-reasoning:near-miss:v1",
            SourceGuideSha256: "sha256:source-guide-001",
            SourceManifestRef: "training-data/orchestration-reasoning/manifest.json",
            SplitId: "split-or-common-001",
            SplitRole: "train",
            HeldOutSetId: "heldout-or-common-001",
            NearMissStrategyRef: "near-miss:missing-required-gate",
            ContainsRawPrivateText: false,
            ProvenanceRefs: new Dictionary<string, string>
            {
                ["guide_root_ref"] = "guide-root:orchestration-reasoning",
                ["source_analysis_ref"] = "analysis-ref:redacted:001"
            });

        Assert.Equal("or-training-pair-001", pair.TrainingPairId);
        Assert.Equal("approved-pair:or-001", pair.ApprovedPairRef);
        Assert.Equal("scenario-ref:chosen:or-001", pair.ChosenScenarioRef);
        Assert.Equal("scenario-ref:rejected:or-001", pair.RejectedScenarioRef);
        Assert.Equal("OR_NEAR_MISS_MISSING_GATE", pair.ErrorCode);
        Assert.Equal("taxonomy:orchestration-reasoning:near-miss:v1", pair.ErrorTaxonomyRef);
        Assert.Equal("sha256:source-guide-001", pair.SourceGuideSha256);
        Assert.Equal("train", pair.SplitRole);
        Assert.Equal("heldout-or-common-001", pair.HeldOutSetId);
        Assert.False(pair.ContainsRawPrivateText);
        Assert.Equal("analysis-ref:redacted:001", pair.ProvenanceRefs["source_analysis_ref"]);
    }

    [Fact]
    public void OrchestrationReasoningCheckpointStateContract_CarriesLifecycleRefsForFutureAssetGates()
    {
        var checkpoint = new OrchestrationReasoningCheckpointStateContract(
            LearnedGuideStateId: "guide-state:orchestration-reasoning:common:v1",
            CheckpointId: "checkpoint:orchestration-reasoning:v1",
            CheckpointSha256: "sha256:checkpoint-or-001",
            SchemaVersion: "orchestration-reasoning-checkpoint-state-v1",
            SourceRowCount: 136,
            DpoPairCount: 7,
            SourceManifestRef: "training-data/orchestration-reasoning/manifest.json",
            GuideStateRef: "models/orchestration-reasoning/orchestration-reasoning-v1/guide-state.json",
            ModelStateRef: "models/orchestration-reasoning/orchestration-reasoning-v1/model-state.json",
            TrainingStateRef: "models/orchestration-reasoning/orchestration-reasoning-v1/training-state.json",
            ExportManifestRef: "models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json",
            FreshLoadProbeRef: "artifacts/orchestration-reasoning-lifecycle/fresh-load.json",
            ContinueTrainingStateRef: "artifacts/orchestration-reasoning-lifecycle/continue.json",
            ProvenanceRefs: new Dictionary<string, string>
            {
                ["source_manifest_sha256"] = "sha256:source-manifest-001",
                ["accepted_pair_manifest_ref"] = "training-data/orchestration-reasoning/dpo-pairs-manifest.json"
            });

        Assert.Equal("guide-state:orchestration-reasoning:common:v1", checkpoint.LearnedGuideStateId);
        Assert.Equal("checkpoint:orchestration-reasoning:v1", checkpoint.CheckpointId);
        Assert.Equal("sha256:checkpoint-or-001", checkpoint.CheckpointSha256);
        Assert.Equal(136, checkpoint.SourceRowCount);
        Assert.Equal(7, checkpoint.DpoPairCount);
        Assert.Equal("models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json", checkpoint.ExportManifestRef);
        Assert.Equal("artifacts/orchestration-reasoning-lifecycle/fresh-load.json", checkpoint.FreshLoadProbeRef);
        Assert.Equal("training-data/orchestration-reasoning/dpo-pairs-manifest.json", checkpoint.ProvenanceRefs["accepted_pair_manifest_ref"]);
    }

    [Fact]
    public void OrchestrationReasoningScenarioUnit_EvaluatesHeldOutRowsWithoutSemanticOrHostClaim()
    {
        var state = new OrchestrationReasoningScenarioUnitState(
            SchemaVersion: 1,
            StateId: "or-scenario-unit-state:test",
            CheckpointRef: "artifacts/orchestration-reasoning/checkpoints/common/or-scenario-unit-v1.json",
            ManifestRef: "artifacts/orchestration-reasoning/manifests/or-scenario-unit-v1.manifest.json",
            Patterns:
            [
                new OrchestrationReasoningScenarioUnitPattern(
                    PatternId: "or-unit-pattern-audit-repair",
                    ScenarioRef: "scenario:architecture-audit-repair",
                    RequiredTaskTypes: ["audit_repair"],
                    RequiredConstraintSignals: ["no-code", "no-commit"],
                    RequiredRequirementSignals: ["parent review required"],
                    RequiredDeliverableSignals: ["audit report"],
                    RequiredEvidenceSignals: ["held-out eval report"],
                    PhaseFamilies: ["audit"],
                    GateFamilies: ["parent-review"],
                    AssetFamilies: ["llm_mvp_core"],
                    HostCheckSets: ["codex"],
                    RiskFlags: ["no_host_runtime_claim"],
                    NextGranularizationActions: ["return evidence packet"],
                    Confidence: 0.9,
                    AcceptedPairRef: "or-pair-accepted-001")
            ],
            AcceptedPairRefs: ["or-pair-accepted-001"],
            QuarantinedPairRefs: ["or-pair-quarantined-001"],
            MinimumConfidence: 0.2,
            MinimumTopTwoMargin: 0.05,
            ContinueTrainingRunClaimed: false,
            CheckpointRewritten: false,
            HostRuntimeClaimed: false,
            SemanticQualityClaimed: false);
        var unit = new OrchestrationReasoningScenarioUnit(state);

        var nearest = unit.FindNearestOrchestrationGuidePatterns(
            new OrchestrationReasoningNearestQuery(
                RequestId: "or-unit-request-001",
                TaskType: "audit_repair",
                ConstraintSignals: ["no-code", "no-commit"],
                RequirementSignals: ["parent review required"],
                DeliverableSignals: ["audit report"],
                EvidenceSignals: ["held-out eval report"],
                RuntimePolicyRefs: new Dictionary<string, string>()),
            k: 3);
        var report = unit.EvaluateHeldOutRows(
        [
            new OrchestrationReasoningScenarioEvaluationRow(
                RowId: "or-heldout-row-001",
                Request: CreateOrUnitRequest("or-heldout-001", "audit_repair", ["no-code", "no-commit"]),
                ExpectedPatternId: "or-unit-pattern-audit-repair",
                ExpectedScenarioRef: "scenario:architecture-audit-repair"),
            new OrchestrationReasoningScenarioEvaluationRow(
                RowId: "or-heldout-row-002",
                Request: CreateOrUnitRequest("or-heldout-002", "implementation", ["code allowed"]),
                ExpectedPatternId: "or-unit-pattern-audit-repair",
                ExpectedScenarioRef: "scenario:architecture-audit-repair")
        ]);

        Assert.Single(nearest);
        Assert.Equal("or-unit-pattern-audit-repair", nearest[0].PatternId);
        Assert.False(nearest[0].SemanticQualityClaimed);
        Assert.False(nearest[0].HostRuntimeClaimed);
        Assert.Equal(2, report.HeldOutRowCount);
        Assert.Equal(1, report.CorrectCount);
        Assert.Equal(0.5, report.Accuracy);
        Assert.False(report.PassesThresholds);
        Assert.False(report.SemanticQualityClaimed);
        Assert.False(report.HostRuntimeClaimed);
        Assert.Contains("below the OrchestrationReasoningScenarioUnit threshold", report.BlockReason);
        Assert.Contains(report.DeterministicControlNotes, note => note.Contains("transparent baseline", StringComparison.OrdinalIgnoreCase));
        Assert.False(state.ContinueTrainingRunClaimed);
        Assert.False(state.CheckpointRewritten);
        Assert.Contains("or-pair-accepted-001", state.AcceptedPairRefs);
        Assert.Contains("or-pair-quarantined-001", state.QuarantinedPairRefs);
    }

    [Fact]
    public void OrchestrationReasoningGuideStateContracts_DoNotExposeRawGuideTextFields()
    {
        var propertyNames = new[]
        {
            typeof(OrchestrationReasoningTrainingPairContract),
            typeof(OrchestrationReasoningCheckpointStateContract)
        }
        .SelectMany(type => type.GetProperties())
        .Select(property => property.Name)
        .ToArray();

        Assert.DoesNotContain(propertyNames, name => name.Contains("RawGuide", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propertyNames, name => name.Contains("GuideText", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propertyNames, name => name.Contains("RawAnalysis", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propertyNames, name => name.Contains("FullText", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("ContainsRawPrivateText", propertyNames);
    }

    [Fact]
    public void OrchestrationReasoningContracts_DoNotExposeDeterministicSelectorOrTrainingImplementation()
    {
        var contractNames = new[]
        {
            nameof(OrchestrationReasoningDomain),
            nameof(OrchestrationReasoningRequest),
            nameof(OrchestrationScenarioPlan),
            nameof(OrchestrationReasoningTrainingPairContract),
            nameof(OrchestrationReasoningCheckpointStateContract),
            nameof(OrchestrationReasoningPlanPatternDpoSignal),
            nameof(OrchestrationReasoningResponse)
        };

        Assert.DoesNotContain(contractNames, name => name.Contains("Selector", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(contractNames, name => name.Contains("Classifier", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(contractNames, name => name.Contains("Trainer", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(contractNames, name => name.Contains("Machine", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(contractNames, name => name.Contains("Replay", StringComparison.OrdinalIgnoreCase));
    }

    private static OrchestrationScenarioPlan CreateScenarioPlan(
        string scenarioId,
        IReadOnlyList<string> selectedLearnedPatterns)
        => new(
            ScenarioId: scenarioId,
            Task: CreateStructuredTask(),
            SelectedLearnedPatterns: selectedLearnedPatterns,
            PhaseFamilies: ["implementation"],
            GateFamilies: ["focused-contract-test"],
            AssetFamilies: ["common", "adaptive"],
            HostCheckSets: ["codex"],
            RiskFlags: ["explicit-plan-pattern-feedback"],
            NextGranularizationActions: ["parent-review-required"]);

    private static StructuredTaskContract CreateStructuredTask()
        => new(
            PromptId: "prompt-orch-001",
            RawPromptSha256: "sha256:prompt-orch-001",
            UserLanguage: "en",
            TaskType: "bounded_work_order",
            ExplicitUserRequirements: ["exactly one subphase", "parent review required"],
            Constraints: ["no training implementation"],
            Deliverables: ["scenario plan contract"],
            ForbiddenActions: ["external AI", "prompt replay"],
            RequiredHosts: ["claude", "cursor", "codex"],
            EvidenceNeeds: ["contract tests"],
            SourceProvenance: new Dictionary<string, string>
            {
                ["source_root_id"] = "sonnet-local-analyzes",
                ["prompt_id"] = "prompt-orch-001"
            },
            SplitId: "split-orch-contract-01");

    private static OrchestrationReasoningRequest CreateOrUnitRequest(
        string requestId,
        string taskType,
        IReadOnlyList<string> constraints)
        => new(
            RequestId: requestId,
            Task: CreateStructuredTask() with
            {
                RawPromptSha256 = $"sha256:{requestId}",
                TaskType = taskType,
                ExplicitUserRequirements = ["parent review required"],
                Constraints = constraints,
                Deliverables = ["audit report"],
                EvidenceNeeds = ["held-out eval report"]
            },
            GuideDerivedStateRefs: ["guide-state:orchestration-reasoning:common:v1"],
            CorpusBundleIds: ["corpus:orchestration-reasoning:common"],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>());

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
