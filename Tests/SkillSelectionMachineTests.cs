// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using System.Security.Cryptography;
using AgentInfrastructure.Logical;
using SmartSkillsDomain;

public sealed class SkillSelectionMachineTests
{
    [Fact]
    public void IMachineTraining_AdapterExtendsILearningMachine()
    {
        Assert.True(typeof(ILearningMachine<SkillSelectionDomain>)
            .IsAssignableFrom(typeof(IMachineTraining<SkillSelectionDomain>)));
    }

    [Fact]
    public void SkillSelectionMachine_ImplementsILearningMachine()
    {
        var machine = new SkillSelectionMachine();

        Assert.IsAssignableFrom<ILearningMachine<SkillSelectionDomain>>(machine);
    }

    [Fact]
    public void SkillSelectionMachine_ExposesIMachineTrainingAdapter()
    {
        var machine = new SkillSelectionMachine();

        Assert.Same(machine, machine.TrainingAdapter);
        Assert.IsAssignableFrom<IMachineTraining<SkillSelectionDomain>>(machine.TrainingAdapter);
    }

    [Fact]
    public async Task SkillSelectionMachine_ExportAsyncImportAsync_RoundTripsStoredCheckpointFiles()
    {
        var persistenceDirectory = Path.Combine(
            Path.GetTempPath(),
            $"skill-selection-persistence-{Guid.NewGuid():N}");
        try
        {
            var request = new SkillSelectionRequest(
                Prompt: "Load planning and plan-review skills for a bounded review.",
                ProjectRoot: "C:/Projects/llm_mvp_core",
                AvailableSkillIds: ["planning-session-startup", "plan-review"],
                HostFacts: new Dictionary<string, string> { ["host"] = "codex" },
                UserPromptSourceEntryId: "UPS-SKILL-PERSIST-001");
            var chosen = new SkillSelectionDecision(
                SelectedSkillIds: ["plan-review"],
                RejectedSkillIds: ["planning-session-startup"],
                EvidenceFeatures: ["TRAINING_PAIR_SELECTED_PLAN_REVIEW"],
                Confidence: 1.0,
                RequiresSecondPass: false,
                DecisionText: "Human-corrected pair prefers plan-review.");
            var rejected = new SkillSelectionDecision(
                SelectedSkillIds: ["planning-session-startup"],
                RejectedSkillIds: ["plan-review"],
                EvidenceFeatures: ["REJECTED_MISSING_REVIEW"],
                Confidence: 0.1,
                RequiresSecondPass: true,
                DecisionText: "Rejected because review skill was missing.");
            var machine = new SkillSelectionMachine(persistenceDirectory);
            var training = await machine.TrainMachineBatchAsync(
                [
                    new SkillSelectionMachinePair(
                        PairId: "skill-persist-pair-001",
                        Request: request,
                        Chosen: chosen,
                        Rejected: rejected,
                        RejectedType: "missing_review_skill",
                        SourcePromptEntryId: "UPS-SKILL-PERSIST-001",
                        TargetTrainingAdapter: SkillSelectionMachine.PromptUnderstandingTrainingAdapter)
                ]);

            Assert.True(training.IsSuccess, training.ErrorMessage);

            var exportResult = await machine.ExportAsync();

            Assert.True(exportResult.IsSuccess, exportResult.ErrorMessage);
            var export = exportResult.Value!;
            Assert.Equal(SkillSelectionMachine.MachineId, export.MachineId);
            Assert.Equal(SkillSelectionMachine.DomainTag, export.DomainTag);
            Assert.True(File.Exists(export.CheckpointRef));
            Assert.True(File.Exists(export.ManifestRef));
            Assert.NotEqual(export.CheckpointRef, export.ManifestRef);
            Assert.DoesNotContain("{", export.CheckpointRef, StringComparison.Ordinal);
            Assert.DoesNotContain("candidatePositionWeights", export.CheckpointRef, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("{", export.ManifestRef, StringComparison.Ordinal);
            var checkpointJson = await File.ReadAllTextAsync(export.CheckpointRef);
            Assert.DoesNotContain(request.Prompt, checkpointJson, StringComparison.Ordinal);
            Assert.Contains("trainedPromptKeys", checkpointJson, StringComparison.Ordinal);
            Assert.Contains("promptUnderstanding", checkpointJson, StringComparison.Ordinal);
            Assert.Contains("taskUnderstanding", checkpointJson, StringComparison.Ordinal);
            Assert.Contains("taskType", checkpointJson, StringComparison.Ordinal);
            Assert.Contains("routing", checkpointJson, StringComparison.Ordinal);
            Assert.Contains("skillCandidates", checkpointJson, StringComparison.Ordinal);
            Assert.Contains("skillSelection", checkpointJson, StringComparison.Ordinal);
            Assert.Contains("promptContext", checkpointJson, StringComparison.Ordinal);
            Assert.Contains("resultGap", checkpointJson, StringComparison.Ordinal);
            Assert.Contains("outputValidation", checkpointJson, StringComparison.Ordinal);
            Assert.Contains("taskCompletion", checkpointJson, StringComparison.Ordinal);
            Assert.Contains("intermediateChecks", checkpointJson, StringComparison.Ordinal);

            var manifest = JsonSerializer.Deserialize<SkillSelectionCheckpointManifest>(
                await File.ReadAllTextAsync(export.ManifestRef),
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
            Assert.NotNull(manifest);
            var manifestDirectory = Path.GetDirectoryName(export.ManifestRef)!;
            Assert.Equal("skill-selection-checkpoint.json", manifest!.CheckpointRef);
            Assert.Equal("skill-selection-manifest.json", manifest.ManifestRef);
            Assert.Equal(export.CheckpointRef, Path.GetFullPath(Path.Combine(manifestDirectory, manifest.CheckpointRef)));
            Assert.Equal(export.ManifestRef, Path.GetFullPath(Path.Combine(manifestDirectory, manifest.ManifestRef)));
            Assert.Equal(Sha256(export.CheckpointRef), manifest.CheckpointSha256);
            Assert.False(manifest.SemanticQualityClaimed);

            var fresh = new SkillSelectionMachine();
            var importResult = await fresh.ImportAsync(export);
            var decision = fresh.DecideFromTrainedState(request);
            var loadResult = await SkillSelectionMachine.LoadCheckpointAsync(export.CheckpointRef);

            Assert.True(importResult.IsSuccess, importResult.ErrorMessage);
            Assert.True(decision.IsSuccess, decision.ErrorMessage);
            Assert.Equal(["plan-review"], decision.Value!.SelectedSkillIds);
            Assert.Equal(["planning-session-startup"], decision.Value.RejectedSkillIds);
            Assert.False(decision.Value.RequiresSecondPass);
            Assert.True(decision.Value.Confidence > 0.0);
            Assert.Contains(SkillSelectionMachine.SemanticPipelineBoundaryEvidence, decision.Value.EvidenceFeatures);
            Assert.Contains(SkillSelectionMachine.PromptUnderstandingCheckpointEvidence, decision.Value.EvidenceFeatures);
            Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, decision.Value.EvidenceFeatures);
            Assert.True(loadResult.IsSuccess, loadResult.ErrorMessage);
            Assert.Equal(export.CheckpointRef, loadResult.Value!.CheckpointRef);
        }
        finally
        {
            if (Directory.Exists(persistenceDirectory))
                Directory.Delete(persistenceDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task SkillSelectionMachine_ImportAsyncRejectsCheckpointWhenManifestHashIsStale()
    {
        var persistenceDirectory = Path.Combine(
            Path.GetTempPath(),
            $"skill-selection-stale-manifest-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(persistenceDirectory);
            var machine = new SkillSelectionMachine(persistenceDirectory);
            var exportResult = await machine.ExportAsync();

            Assert.True(exportResult.IsSuccess, exportResult.ErrorMessage);
            var export = exportResult.Value!;
            var manifest = JsonSerializer.Deserialize<SkillSelectionCheckpointManifest>(
                await File.ReadAllTextAsync(export.ManifestRef),
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
            Assert.NotNull(manifest);
            Assert.Equal(Sha256(export.CheckpointRef), manifest!.CheckpointSha256);

            await File.AppendAllTextAsync(export.CheckpointRef, "\n ");
            Assert.NotEqual(manifest.CheckpointSha256, Sha256(export.CheckpointRef));

            var fresh = new SkillSelectionMachine();
            var importResult = await fresh.ImportAsync(export);

            Assert.False(importResult.IsSuccess);
            Assert.Equal("MANIFEST_CHECKPOINT_HASH_MISMATCH", importResult.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(persistenceDirectory))
                Directory.Delete(persistenceDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task SkillSelectionMachine_CandidatePositionWeightsAreScaffoldOnly()
    {
        var trainingRequest = new SkillSelectionRequest(
            Prompt: "Review planning requirements for a bounded work order.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["planning-session-startup", "plan-review"],
            HostFacts: new Dictionary<string, string> { ["host"] = "codex" },
            UserPromptSourceEntryId: "UPS-SKILL-BOUNDARY-001");
        var request = new SkillSelectionRequest(
            Prompt: "Draw interface colors for a visual asset.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["planning-session-startup", "plan-review"],
            HostFacts: new Dictionary<string, string> { ["host"] = "codex" },
            UserPromptSourceEntryId: "UPS-SKILL-BOUNDARY-HELDOUT-001");
        var machine = new SkillSelectionMachine();
        var training = await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "skill-boundary-pair-001",
                    Request: trainingRequest,
                    Chosen: new SkillSelectionDecision(
                        SelectedSkillIds: ["plan-review"],
                        RejectedSkillIds: ["planning-session-startup"],
                        EvidenceFeatures: ["TRAINING_PAIR_SELECTED_PLAN_REVIEW"],
                        Confidence: 1.0,
                        RequiresSecondPass: false,
                        DecisionText: "Human-corrected pair prefers plan-review."),
                    Rejected: new SkillSelectionDecision(
                        SelectedSkillIds: ["planning-session-startup"],
                        RejectedSkillIds: ["plan-review"],
                        EvidenceFeatures: ["REJECTED_MISSING_REVIEW"],
                        Confidence: 0.1,
                        RequiresSecondPass: true,
                        DecisionText: "Rejected because review skill was missing."),
                    RejectedType: "missing_review_skill",
                    SourcePromptEntryId: "UPS-SKILL-BOUNDARY-001",
                    TargetTrainingAdapter: SkillSelectionMachine.PromptUnderstandingTrainingAdapter)
            ]);

        var decision = machine.DecideFromTrainedState(request);

        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(decision.IsSuccess, decision.ErrorMessage);
        Assert.Empty(decision.Value!.SelectedSkillIds);
        Assert.Equal(request.AvailableSkillIds, decision.Value.RejectedSkillIds);
        Assert.True(decision.Value.RequiresSecondPass);
        Assert.Equal(0.0, decision.Value.Confidence);
        Assert.Contains(SkillSelectionMachine.SemanticPipelineBoundaryEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(CandidateOrderLeakageGuardUnit.DiagnosticsEvidence, decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.StartsWith("candidate_position_suggestion=", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.StartsWith("candidate_index=", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.StartsWith("candidate_weight=", StringComparison.Ordinal));
        Assert.Contains("candidate_position_role=diagnostics_provenance_only", decision.Value.EvidenceFeatures);
        Assert.Contains("candidate_order_permutation_selection_authority=false", decision.Value.EvidenceFeatures);
        Assert.Contains("candidate_order_permutation_count=2", decision.Value.EvidenceFeatures);
        Assert.Contains("candidate_order_sensitivity_detected=False", decision.Value.EvidenceFeatures);
        Assert.Contains("SS_02_TO_SS_12_REQUIRED_FOR_FINAL_AUTHORITY", decision.Value.EvidenceFeatures);
        Assert.Contains("diagnostics and leakage-guard evidence only", decision.Value.DecisionText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SkillSelectionMachine_CandidateOrderDiagnosticsArePermutationInvariant()
    {
        var trainingRequest = new SkillSelectionRequest(
            Prompt: "Review planning requirements for a bounded work order.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["planning-session-startup", "plan-review"],
            HostFacts: new Dictionary<string, string> { ["host"] = "codex" },
            UserPromptSourceEntryId: "UPS-SKILL-ORDER-TRAIN-001");
        var firstOrderRequest = new SkillSelectionRequest(
            Prompt: "Draw interface colors for a visual asset.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["alpha-skill", "beta-skill"],
            HostFacts: new Dictionary<string, string> { ["host"] = "codex" },
            UserPromptSourceEntryId: "UPS-SKILL-ORDER-HELDOUT-001");
        var secondOrderRequest = firstOrderRequest with
        {
            AvailableSkillIds = ["beta-skill", "alpha-skill"],
            UserPromptSourceEntryId = "UPS-SKILL-ORDER-HELDOUT-002"
        };
        var machine = new SkillSelectionMachine();
        var training = await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "skill-order-pair-001",
                    Request: trainingRequest,
                    Chosen: new SkillSelectionDecision(
                        SelectedSkillIds: ["plan-review"],
                        RejectedSkillIds: ["planning-session-startup"],
                        EvidenceFeatures: ["TRAINING_PAIR_SELECTED_PLAN_REVIEW"],
                        Confidence: 1.0,
                        RequiresSecondPass: false,
                        DecisionText: "Human-corrected pair prefers plan-review."),
                    Rejected: new SkillSelectionDecision(
                        SelectedSkillIds: ["planning-session-startup"],
                        RejectedSkillIds: ["plan-review"],
                        EvidenceFeatures: ["REJECTED_MISSING_REVIEW"],
                        Confidence: 0.1,
                        RequiresSecondPass: true,
                        DecisionText: "Rejected because review skill was missing."),
                    RejectedType: "missing_review_skill",
                    SourcePromptEntryId: "UPS-SKILL-ORDER-TRAIN-001",
                    TargetTrainingAdapter: SkillSelectionMachine.PromptUnderstandingTrainingAdapter)
            ]);

        var firstDecision = machine.DecideFromTrainedState(firstOrderRequest);
        var secondDecision = machine.DecideFromTrainedState(secondOrderRequest);
        var report = machine.EvaluateCandidateOrderLeakage(firstOrderRequest);
        var unit = new CandidateOrderLeakageGuardUnit(
            machine.ExportCandidateOrderLeakageGuardState(),
            machine.ProbeWithoutCandidatePositionAuthority);
        var unitResult = await unit.ExecuteAsync(new LogicalUnitRequest<SkillSelectionRequest>(
            RunId: "candidate-order-unit-run",
            UnitId: CandidateOrderLeakageGuardUnit.UnitId,
            UnitRole: CandidateOrderLeakageGuardUnit.Role,
            DomainTag: SkillSelectionMachine.DomainTag,
            Input: firstOrderRequest,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>()));
        var flow = new SkillSelectionLogicalFlow(machine);
        var flowResult = await flow.ExecuteAsync(new LogicalFlowRequest<SkillSelectionRequest>(
            RunId: "candidate-order-flow-run",
            FlowId: SkillSelectionLogicalFlow.FlowId,
            ScenarioId: "candidate-order-permutation",
            Input: firstOrderRequest,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>()));

        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(firstDecision.IsSuccess, firstDecision.ErrorMessage);
        Assert.True(secondDecision.IsSuccess, secondDecision.ErrorMessage);
        Assert.True(unitResult.IsSuccess, unitResult.ErrorMessage);
        Assert.True(flowResult.IsSuccess, flowResult.ErrorMessage);
        Assert.Empty(firstDecision.Value!.SelectedSkillIds);
        Assert.Empty(secondDecision.Value!.SelectedSkillIds);
        Assert.Equal(firstOrderRequest.AvailableSkillIds, firstDecision.Value.RejectedSkillIds);
        Assert.Equal(secondOrderRequest.AvailableSkillIds, secondDecision.Value.RejectedSkillIds);
        Assert.True(firstDecision.Value.RequiresSecondPass);
        Assert.True(secondDecision.Value.RequiresSecondPass);
        Assert.Equal(0.0, firstDecision.Value.Confidence);
        Assert.Equal(0.0, secondDecision.Value.Confidence);
        Assert.DoesNotContain(firstDecision.Value.EvidenceFeatures, evidence => evidence.StartsWith("candidate_position_suggestion=", StringComparison.Ordinal));
        Assert.DoesNotContain(secondDecision.Value.EvidenceFeatures, evidence => evidence.StartsWith("candidate_position_suggestion=", StringComparison.Ordinal));
        Assert.Contains(CandidateOrderLeakageGuardUnit.DiagnosticsEvidence, firstDecision.Value.EvidenceFeatures);
        Assert.Contains(CandidateOrderLeakageGuardUnit.DiagnosticsEvidence, secondDecision.Value.EvidenceFeatures);
        Assert.Contains("candidate_order_probe_source=semantic_units_without_candidate_position", firstDecision.Value.EvidenceFeatures);
        Assert.Equal(
            firstDecision.Value.EvidenceFeatures.Order(StringComparer.Ordinal),
            secondDecision.Value.EvidenceFeatures.Order(StringComparer.Ordinal));
        Assert.False(report.CanSelectSkill);
        Assert.False(report.SemanticQualityClaimed);
        Assert.False(report.OrderSensitivityDetected);
        Assert.True(report.PermutationSelectionInvariant);
        Assert.Equal(2, report.PermutationCount);
        Assert.Contains(report.BlockedFeatureIds, feature => feature.Contains("candidate_position", StringComparison.Ordinal));
        Assert.All(report.PermutationProbes, probe =>
        {
            Assert.Empty(probe.SelectedSkillIds);
            Assert.True(probe.RequiresSecondPass);
            Assert.Equal(0.0, probe.Confidence);
        });
        Assert.Equal(["current", "reversed"], report.PermutationProbes.Select(probe => probe.OrderLabel).ToArray());
        Assert.Equal([0, 1], report.PermutationProbes[0].SourceCandidatePositions);
        Assert.Equal([1, 0], report.PermutationProbes[1].SourceCandidatePositions);
        Assert.False(unitResult.Value!.Output.CanSelectSkill);
        Assert.Equal(2, unitResult.Value.Output.PermutationCount);
        Assert.Contains(CandidateOrderLeakageGuardUnit.DiagnosticsEvidence, unitResult.Value.EvidenceIds);
        Assert.Contains(CandidateOrderLeakageGuardUnit.DiagnosticsEvidence, flowResult.Value!.EvidenceIds);
        Assert.Equal("1", flowResult.Value.Metrics["candidate_order_guard_unit_count"]);
        Assert.Equal("2", flowResult.Value.Metrics["candidate_order_permutation_count"]);
        Assert.Equal(bool.FalseString, flowResult.Value.Metrics["candidate_order_sensitivity_detected"]);
    }

    [Fact]
    public async Task SkillSelectionMachine_CandidateOrderProbesUseSemanticDecisionPathWhenAvailable()
    {
        var trainingRequest = new SkillSelectionRequest(
            Prompt: "Review planning coverage before accepting an execution plan.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["planning-session-startup", "plan-review"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-CANDIDATE-ORDER-SEMANTIC-TRAIN-001");
        var heldOutRequest = new SkillSelectionRequest(
            Prompt: "Please review the plan coverage before the next executor starts.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["planning-session-startup", "plan-review"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-CANDIDATE-ORDER-SEMANTIC-HELDOUT-001");
        var machine = new SkillSelectionMachine();
        var training = await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "candidate-order-semantic-pair-001",
                    Request: trainingRequest,
                    Chosen: new SkillSelectionDecision(
                        SelectedSkillIds: ["plan-review"],
                        RejectedSkillIds: ["planning-session-startup"],
                        EvidenceFeatures: ["human_review_intent"],
                        Confidence: 1.0,
                        RequiresSecondPass: false,
                        DecisionText: "The request asks for review coverage."),
                    Rejected: new SkillSelectionDecision(
                        SelectedSkillIds: ["planning-session-startup"],
                        RejectedSkillIds: ["plan-review"],
                        EvidenceFeatures: ["near_miss_startup"],
                        Confidence: 0.1,
                        RequiresSecondPass: true,
                        DecisionText: "Startup is adjacent but not the primary intent."),
                    RejectedType: "adjacent_wrong_skill",
                    SourcePromptEntryId: "UPS-CANDIDATE-ORDER-SEMANTIC-TRAIN-001",
                    TargetTrainingAdapter: SkillSelectionMachine.PromptUnderstandingTrainingAdapter)
            ]);

        var directProbe = machine.ProbeWithoutCandidatePositionAuthority(heldOutRequest);
        var reversedProbe = machine.ProbeWithoutCandidatePositionAuthority(heldOutRequest with
        {
            AvailableSkillIds = ["plan-review", "planning-session-startup"]
        });
        var report = machine.EvaluateCandidateOrderLeakage(heldOutRequest);

        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.Equal(["plan-review"], directProbe.SelectedSkillIds);
        Assert.Equal(["plan-review"], reversedProbe.SelectedSkillIds);
        Assert.False(directProbe.RequiresSecondPass);
        Assert.False(reversedProbe.RequiresSecondPass);
        Assert.True(directProbe.Confidence > 0.0);
        Assert.True(reversedProbe.Confidence > 0.0);
        Assert.False(report.CanSelectSkill);
        Assert.False(report.SemanticQualityClaimed);
        Assert.False(report.OrderSensitivityDetected);
        Assert.True(report.PermutationSelectionInvariant);
        Assert.Equal(2, report.PermutationCount);
        Assert.Contains("candidate_order_probe_source=semantic_units_without_candidate_position", report.EvidenceFeatures);
        Assert.All(report.PermutationProbes, probe =>
        {
            Assert.Equal(["plan-review"], probe.SelectedSkillIds);
            Assert.False(probe.RequiresSecondPass);
            Assert.True(probe.Confidence > 0.0);
        });
        Assert.Equal(["current", "reversed"], report.PermutationProbes.Select(probe => probe.OrderLabel).ToArray());
        Assert.Equal([0, 1], report.PermutationProbes[0].SourceCandidatePositions);
        Assert.Equal([1, 0], report.PermutationProbes[1].SourceCandidatePositions);
    }

    [Fact]
    public async Task SkillSelectionMachine_PromptUnderstandingUsesCheckpointSectionForHeldOutPrompt()
    {
        var trainingRequest = new SkillSelectionRequest(
            Prompt: "Review planning coverage before accepting an execution plan.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["planning-session-startup", "plan-review"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS02-TRAIN-001");
        var heldOutRequest = new SkillSelectionRequest(
            Prompt: "Please review the plan coverage before the next executor starts.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["planning-session-startup", "plan-review"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS02-HELDOUT-001");
        var machine = new SkillSelectionMachine();

        var training = await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss02-prompt-understanding-pair-001",
                    Request: trainingRequest,
                    Chosen: new SkillSelectionDecision(
                        SelectedSkillIds: ["plan-review"],
                        RejectedSkillIds: ["planning-session-startup"],
                        EvidenceFeatures: ["human_review_intent"],
                        Confidence: 1.0,
                        RequiresSecondPass: false,
                        DecisionText: "The request asks for review coverage."),
                    Rejected: new SkillSelectionDecision(
                        SelectedSkillIds: ["planning-session-startup"],
                        RejectedSkillIds: ["plan-review"],
                        EvidenceFeatures: ["near_miss_startup"],
                        Confidence: 0.1,
                        RequiresSecondPass: true,
                        DecisionText: "Startup is adjacent but not the primary intent."),
                    RejectedType: "adjacent_wrong_skill",
                    SourcePromptEntryId: "UPS-SS02-TRAIN-001",
                    TargetTrainingAdapter: SkillSelectionMachine.PromptUnderstandingTrainingAdapter)
            ]);

        var checkpoint = machine.ExportCheckpoint();
        var fresh = new SkillSelectionMachine();
        fresh.LoadCheckpoint(checkpoint);
        var unitState = fresh.ExportPromptUnderstandingSkillUnitState();
        var unitResult = await new PromptUnderstandingSkillUnit().ExecuteAsync(new LogicalUnitRequest<PromptUnderstandingSkillInput>(
            RunId: "prompt-understanding-unit-run",
            UnitId: PromptUnderstandingSkillUnit.UnitId,
            UnitRole: PromptUnderstandingSkillUnit.Role,
            DomainTag: SkillSelectionMachine.DomainTag,
            Input: new PromptUnderstandingSkillInput(heldOutRequest, unitState),
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>()));
        var decision = fresh.DecideFromTrainedState(heldOutRequest);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.Equal(SkillSelectionMachine.PromptUnderstandingUnitId, checkpoint.PromptUnderstanding.UnitId);
        Assert.Single(checkpoint.PromptUnderstanding.SkillProfiles);
        Assert.Equal(PromptUnderstandingSkillUnit.UnitId, unitState.UnitId);
        Assert.False(unitState.SemanticQualityClaimed);
        Assert.Equal(["plan-review"], unitState.SkillVocabulary);
        var unitProfile = Assert.Single(unitState.SkillProfiles);
        Assert.Equal("plan-review", unitProfile.SkillId);
        Assert.Contains("review", unitProfile.PromptFeatureWeights.Keys);
        Assert.Contains("ss02-prompt-understanding-pair-001", unitProfile.SourcePairIds);
        Assert.DoesNotContain(unitState.PromptFeatureVocabulary, token => token.Contains("UPS", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(unitState.PromptFeatureVocabulary, token => token.Contains("ss02-prompt-understanding-pair", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(unitState.PromptFeatureVocabulary, token => token.Contains("Projects", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(SkillSelectionMachine.TaskUnderstandingUnitId, checkpoint.TaskUnderstanding.UnitId);
        Assert.Empty(checkpoint.TaskUnderstanding.SkillProfiles);
        Assert.Equal(SkillSelectionMachine.TaskTypeUnitId, checkpoint.TaskType.UnitId);
        Assert.Empty(checkpoint.TaskType.SkillProfiles);
        Assert.Equal(SkillSelectionMachine.RoutingUnitId, checkpoint.Routing.UnitId);
        Assert.Empty(checkpoint.Routing.SkillProfiles);
        Assert.Equal(SkillSelectionMachine.SkillCandidatesUnitId, checkpoint.SkillCandidates.UnitId);
        Assert.Empty(checkpoint.SkillCandidates.SkillProfiles);
        Assert.False(checkpoint.SkillCandidates.SemanticQualityClaimed);
        Assert.False(checkpoint.Routing.SemanticQualityClaimed);
        Assert.False(checkpoint.TaskType.SemanticQualityClaimed);
        Assert.False(checkpoint.TaskUnderstanding.SemanticQualityClaimed);
        Assert.False(checkpoint.PromptUnderstanding.SemanticQualityClaimed);
        Assert.True(unitResult.IsSuccess, unitResult.ErrorMessage);
        Assert.True(unitResult.Value!.Output.HasDecision);
        Assert.Equal(["plan-review"], unitResult.Value.Output.SelectedSkillIds);
        Assert.False(unitResult.Value.Output.SemanticQualityClaimed);
        Assert.True(unitResult.Value.Output.BaselineCounterOnly);
        Assert.Contains(PromptUnderstandingSkillUnit.UnitExecutedEvidence, unitResult.Value.EvidenceIds);
        Assert.Contains(PromptUnderstandingSkillUnit.BaselineEvidence, unitResult.Value.EvidenceIds);
        Assert.True(decision.IsSuccess, decision.ErrorMessage);
        Assert.Equal(["plan-review"], decision.Value!.SelectedSkillIds);
        Assert.Contains(SkillSelectionMachine.PromptUnderstandingCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(PromptUnderstandingSkillUnit.UnitExecutedEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(PromptUnderstandingSkillUnit.BaselineEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("prompt_understanding:semantic_quality_claimed=false", decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("UPS-SS02", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("ss02-prompt-understanding-pair", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains(trainingRequest.Prompt, StringComparison.Ordinal));
        Assert.Contains("typed baseline prompt-feature state", decision.Value.DecisionText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SkillSelectionMachine_TaskUnderstandingUsesCheckpointSectionForHeldOutTaskPrompt()
    {
        var trainingRequest = new SkillSelectionRequest(
            Prompt: "Validate plan deliverables and coverage constraints before marking the task done.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["task-completion", "claim-verification"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS03-TRAIN-001");
        var heldOutRequest = new SkillSelectionRequest(
            Prompt: "Check completion constraints and review deliverables before saying the task is done.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["task-completion", "claim-verification"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS03-HELDOUT-001");
        var machine = new SkillSelectionMachine();

        var training = await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss03-task-understanding-pair-001",
                    Request: trainingRequest,
                    Chosen: new SkillSelectionDecision(
                        SelectedSkillIds: ["task-completion"],
                        RejectedSkillIds: ["claim-verification"],
                        EvidenceFeatures: ["task_completion_intent"],
                        Confidence: 1.0,
                        RequiresSecondPass: false,
                        DecisionText: "The request asks whether completion constraints are satisfied."),
                    Rejected: new SkillSelectionDecision(
                        SelectedSkillIds: ["claim-verification"],
                        RejectedSkillIds: ["task-completion"],
                        EvidenceFeatures: ["near_miss_claim_check"],
                        Confidence: 0.1,
                        RequiresSecondPass: true,
                        DecisionText: "Claim verification is adjacent but not the task completion intent."),
                    RejectedType: "adjacent_wrong_skill",
                    SourcePromptEntryId: "UPS-SS03-TRAIN-001",
                    TargetTrainingAdapter: SkillSelectionMachine.TaskUnderstandingTrainingAdapter)
            ]);

        var checkpoint = machine.ExportCheckpoint();
        var fresh = new SkillSelectionMachine();
        fresh.LoadCheckpoint(checkpoint);
        var unitState = fresh.ExportTaskUnderstandingSkillUnitState();
        var unitResult = await new TaskUnderstandingSkillUnit().ExecuteAsync(new LogicalUnitRequest<TaskUnderstandingSkillInput>(
            RunId: "task-understanding-unit-run",
            UnitId: TaskUnderstandingSkillUnit.UnitId,
            UnitRole: TaskUnderstandingSkillUnit.Role,
            DomainTag: SkillSelectionMachine.DomainTag,
            Input: new TaskUnderstandingSkillInput(heldOutRequest, unitState),
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>()));
        var decision = fresh.DecideFromTrainedState(heldOutRequest);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.Empty(checkpoint.PromptUnderstanding.SkillProfiles);
        Assert.Equal(SkillSelectionMachine.TaskUnderstandingUnitId, checkpoint.TaskUnderstanding.UnitId);
        Assert.Single(checkpoint.TaskUnderstanding.SkillProfiles);
        Assert.Equal(TaskUnderstandingSkillUnit.UnitId, unitState.UnitId);
        Assert.False(unitState.SemanticQualityClaimed);
        Assert.Equal(["task-completion"], unitState.SkillVocabulary);
        var unitProfile = Assert.Single(unitState.SkillProfiles);
        Assert.Equal("task-completion", unitProfile.SkillId);
        Assert.Contains("deliverables", unitProfile.TaskFeatureWeights.Keys);
        Assert.Contains("ss03-task-understanding-pair-001", unitProfile.SourcePairIds);
        Assert.DoesNotContain(unitState.TaskFeatureVocabulary, token => token.Contains("UPS", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(unitState.TaskFeatureVocabulary, token => token.Contains("ss03-task-understanding-pair", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(unitState.TaskFeatureVocabulary, token => token.Contains("Projects", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(SkillSelectionMachine.TaskTypeUnitId, checkpoint.TaskType.UnitId);
        Assert.Empty(checkpoint.TaskType.SkillProfiles);
        Assert.Equal(SkillSelectionMachine.RoutingUnitId, checkpoint.Routing.UnitId);
        Assert.Empty(checkpoint.Routing.SkillProfiles);
        Assert.Equal(SkillSelectionMachine.SkillCandidatesUnitId, checkpoint.SkillCandidates.UnitId);
        Assert.Empty(checkpoint.SkillCandidates.SkillProfiles);
        Assert.False(checkpoint.SkillCandidates.SemanticQualityClaimed);
        Assert.False(checkpoint.Routing.SemanticQualityClaimed);
        Assert.False(checkpoint.TaskType.SemanticQualityClaimed);
        Assert.False(checkpoint.TaskUnderstanding.SemanticQualityClaimed);
        Assert.True(unitResult.IsSuccess, unitResult.ErrorMessage);
        Assert.True(unitResult.Value!.Output.HasDecision);
        Assert.Equal(["task-completion"], unitResult.Value.Output.SelectedSkillIds);
        Assert.False(unitResult.Value.Output.SemanticQualityClaimed);
        Assert.True(unitResult.Value.Output.BaselineCounterOnly);
        Assert.Contains(TaskUnderstandingSkillUnit.UnitExecutedEvidence, unitResult.Value.EvidenceIds);
        Assert.Contains(TaskUnderstandingSkillUnit.BaselineEvidence, unitResult.Value.EvidenceIds);
        Assert.True(decision.IsSuccess, decision.ErrorMessage);
        Assert.Equal(["task-completion"], decision.Value!.SelectedSkillIds);
        Assert.Contains(SkillSelectionMachine.TaskUnderstandingCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(TaskUnderstandingSkillUnit.UnitExecutedEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(TaskUnderstandingSkillUnit.BaselineEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("task_understanding:semantic_quality_claimed=false", decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("UPS-SS03", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("ss03-task-understanding-pair", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains(trainingRequest.Prompt, StringComparison.Ordinal));
        Assert.Contains("typed baseline task-boundary state", decision.Value.DecisionText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SkillSelectionMachine_TaskUnderstandingAblationLowersDecisionWithoutUnitSection()
    {
        var request = new SkillSelectionRequest(
            Prompt: "Check completion constraints and review deliverables before saying the task is done.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["task-completion", "claim-verification"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS03-ABLATION-001");
        var trained = new SkillSelectionMachine();
        await trained.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss03-task-understanding-pair-002",
                    Request: request with { Prompt = "Validate plan deliverables and coverage constraints before marking the task done." },
                    Chosen: new SkillSelectionDecision(["task-completion"], ["claim-verification"], ["task_completion_intent"], 1.0, false, "Completion intent."),
                    Rejected: new SkillSelectionDecision(["claim-verification"], ["task-completion"], ["near_miss_claim_check"], 0.1, true, "Claim check near miss."),
                    RejectedType: "adjacent_wrong_skill",
                    SourcePromptEntryId: "UPS-SS03-ABLATION-TRAIN",
                    TargetTrainingAdapter: SkillSelectionMachine.TaskUnderstandingTrainingAdapter)
            ]);
        var empty = new SkillSelectionMachine();

        var trainedDecision = trained.DecideFromTrainedState(request);
        var emptyDecision = empty.DecideFromTrainedState(request);

        Assert.True(trainedDecision.IsSuccess, trainedDecision.ErrorMessage);
        Assert.True(emptyDecision.IsSuccess, emptyDecision.ErrorMessage);
        Assert.Equal(["task-completion"], trainedDecision.Value!.SelectedSkillIds);
        Assert.Empty(emptyDecision.Value!.SelectedSkillIds);
        Assert.True(trainedDecision.Value.Confidence > emptyDecision.Value.Confidence);
        Assert.Contains(SkillSelectionMachine.TaskUnderstandingCheckpointEvidence, trainedDecision.Value.EvidenceFeatures);
        Assert.Contains("NO_TRAINED_STATE_AVAILABLE", emptyDecision.Value.EvidenceFeatures);
    }

    [Fact]
    public async Task SkillSelectionMachine_TaskTypeUsesCheckpointSectionForHeldOutTypePrompt()
    {
        var trainingRequest = new SkillSelectionRequest(
            Prompt: "Classify this work as review evidence triage rather than implementation.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["review-work", "implementation-work"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS04-TRAIN-001");
        var heldOutRequest = new SkillSelectionRequest(
            Prompt: "Classify this request as review evidence work before changing code.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["implementation-work", "review-work"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS04-HELDOUT-001");
        var machine = new SkillSelectionMachine();
        var training = await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss04-task-type-pair-001",
                    Request: trainingRequest,
                    Chosen: new SkillSelectionDecision(["review-work"], ["implementation-work"], ["review_type"], 1.0, false, "Review work type."),
                    Rejected: new SkillSelectionDecision(["implementation-work"], ["review-work"], ["implementation_type_near_miss"], 0.1, true, "Implementation near miss."),
                    RejectedType: "adjacent_wrong_task_type",
                    SourcePromptEntryId: "UPS-SS04-TRAIN-001",
                    TargetTrainingAdapter: SkillSelectionMachine.TaskTypeTrainingAdapter)
            ]);

        var checkpoint = machine.ExportCheckpoint();
        var fresh = new SkillSelectionMachine();
        fresh.LoadCheckpoint(checkpoint);
        var taskTypeUnitState = fresh.ExportTaskTypeSkillUnitState();
        var taskTypeUnit = new TaskTypeSkillUnit();
        var taskTypeFeatures = SkillSelectionMachine.ExtractPromptUnderstandingSignals(heldOutRequest.Prompt);
        var nearestExamples = taskTypeUnit.FindNearestTaskTypeExamples(taskTypeFeatures, taskTypeUnitState, 3);
        var reviewSupport = taskTypeUnit.ComputeTaskTypeSupport(taskTypeFeatures, "review-work", taskTypeUnitState, 3);
        var implementationSupport = taskTypeUnit.ComputeTaskTypeSupport(taskTypeFeatures, "implementation-work", taskTypeUnitState, 3);
        var taskTypeUnitEvidence = taskTypeUnit.Evaluate(new TaskTypeSkillInput(
            Request: heldOutRequest,
            State: taskTypeUnitState));
        var decision = fresh.DecideFromTrainedState(heldOutRequest);
        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.Empty(checkpoint.PromptUnderstanding.SkillProfiles);
        Assert.Empty(checkpoint.TaskUnderstanding.SkillProfiles);
        Assert.Equal(SkillSelectionMachine.TaskTypeUnitId, checkpoint.TaskType.UnitId);
        Assert.Equal(
            ["implementation-work", "review-work"],
            checkpoint.TaskType.SkillProfiles.Select(profile => profile.SkillId).Order(StringComparer.Ordinal).ToArray());
        Assert.Equal(SkillSelectionMachine.RoutingUnitId, checkpoint.Routing.UnitId);
        Assert.Empty(checkpoint.Routing.SkillProfiles);
        Assert.Equal(SkillSelectionMachine.SkillCandidatesUnitId, checkpoint.SkillCandidates.UnitId);
        Assert.Empty(checkpoint.SkillCandidates.SkillProfiles);
        Assert.False(checkpoint.SkillCandidates.SemanticQualityClaimed);
        Assert.False(checkpoint.Routing.SemanticQualityClaimed);
        Assert.False(checkpoint.TaskType.SemanticQualityClaimed);
        Assert.Equal(TaskTypeSkillUnit.UnitId, taskTypeUnitState.UnitId);
        Assert.False(taskTypeUnitState.SemanticQualityClaimed);
        var reviewProfile = Assert.Single(
            taskTypeUnitState.SkillProfiles,
            profile => string.Equals(profile.SkillId, "review-work", StringComparison.Ordinal));
        var implementationProfile = Assert.Single(
            taskTypeUnitState.SkillProfiles,
            profile => string.Equals(profile.SkillId, "implementation-work", StringComparison.Ordinal));
        Assert.NotEmpty(reviewProfile.TaskTypeFeatureWeights);
        Assert.All(reviewProfile.TaskTypeFeatureWeights.Values, value => Assert.True(value > 0));
        Assert.NotEmpty(implementationProfile.TaskTypeFeatureWeights);
        Assert.All(implementationProfile.TaskTypeFeatureWeights.Values, value => Assert.True(value < 0));
        Assert.Contains("ss04-task-type-pair-001", reviewProfile.SourcePairIds);
        Assert.Contains("ss04-task-type-pair-001", implementationProfile.SourcePairIds);
        Assert.Contains(taskTypeUnitState.TaskTypeFeatureVocabulary, signal => string.Equals(signal, "review", StringComparison.Ordinal));
        Assert.NotEmpty(taskTypeUnitState.KnnMemory);
        Assert.NotEmpty(nearestExamples);
        Assert.All(nearestExamples, example =>
        {
            Assert.Equal("review-work", example.ChosenSkillId);
            Assert.DoesNotContain("UPS-SS04", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain("ss04-task-type-pair", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain("C:/Projects", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain(example.TaskTypeFeatures, feature => feature.Contains("UPS-SS04", StringComparison.Ordinal));
            Assert.DoesNotContain(example.TaskTypeFeatures, feature => feature.Contains("C:/Projects", StringComparison.Ordinal));
        });
        Assert.True(reviewSupport.SupportScore > 0.0);
        Assert.True(reviewSupport.MatchedFeatureCount > 0);
        Assert.Equal(0.0, implementationSupport.SupportScore);
        Assert.Equal(0, implementationSupport.MatchedFeatureCount);
        Assert.True(taskTypeUnitEvidence.HasDecision);
        Assert.Equal(["review-work"], taskTypeUnitEvidence.SelectedSkillIds);
        Assert.False(taskTypeUnitEvidence.SemanticQualityClaimed);
        Assert.True(taskTypeUnitEvidence.BaselineCounterOnly);
        Assert.True(taskTypeUnitEvidence.LearnedTaskTypeSignalScore > 0);
        Assert.Contains(TaskTypeSkillUnit.UnitExecutedEvidence, taskTypeUnitEvidence.EvidenceFeatures);
        Assert.Contains(TaskTypeSkillUnit.BaselineEvidence, taskTypeUnitEvidence.EvidenceFeatures);
        Assert.True(decision.IsSuccess, decision.ErrorMessage);
        Assert.Equal(["review-work"], decision.Value!.SelectedSkillIds);
        Assert.Contains(SkillSelectionMachine.TaskTypeCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(TaskTypeSkillUnit.UnitExecutedEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(TaskTypeSkillUnit.BaselineEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("task_type:semantic_quality_claimed=false", decision.Value.EvidenceFeatures);
        Assert.Contains(decision.Value.EvidenceFeatures, evidence => evidence.StartsWith("task_type:learned_task_type_signal_score=", StringComparison.Ordinal));
        Assert.Contains(decision.Value.EvidenceFeatures, evidence => evidence.StartsWith("task_type:nearest_example_count=", StringComparison.Ordinal));
        Assert.Contains(decision.Value.EvidenceFeatures, evidence => evidence.StartsWith("task_type:support_score=", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("source_pair_count", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("UPS-SS04", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("ss04-task-type-pair", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("C:/Projects", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains(trainingRequest.Prompt, StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("candidate_position", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("candidate_order", StringComparison.Ordinal));
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("task_type selected", decision.Value.DecisionText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SkillSelectionMachine_TaskTypeAblationLowersDecisionWithoutUnitSection()
    {
        var request = new SkillSelectionRequest(
            Prompt: "Classify this request as review evidence work before changing code.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["implementation-work", "review-work"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS04-ABLATION-001");
        var trained = new SkillSelectionMachine();
        await trained.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss04-task-type-pair-002",
                    Request: request with { Prompt = "Classify this work as review evidence triage rather than implementation." },
                    Chosen: new SkillSelectionDecision(["review-work"], ["implementation-work"], ["review_type"], 1.0, false, "Review work type."),
                    Rejected: new SkillSelectionDecision(["implementation-work"], ["review-work"], ["implementation_type_near_miss"], 0.1, true, "Implementation near miss."),
                    RejectedType: "adjacent_wrong_task_type",
                    SourcePromptEntryId: "UPS-SS04-ABLATION-TRAIN",
                    TargetTrainingAdapter: SkillSelectionMachine.TaskTypeTrainingAdapter)
            ]);
        var empty = new SkillSelectionMachine();

        var trainedDecision = trained.DecideFromTrainedState(request);
        var emptyDecision = empty.DecideFromTrainedState(request);

        Assert.True(trainedDecision.IsSuccess, trainedDecision.ErrorMessage);
        Assert.True(emptyDecision.IsSuccess, emptyDecision.ErrorMessage);
        Assert.Equal(["review-work"], trainedDecision.Value!.SelectedSkillIds);
        Assert.Empty(emptyDecision.Value!.SelectedSkillIds);
        Assert.True(trainedDecision.Value.Confidence > emptyDecision.Value.Confidence);
        Assert.Contains(SkillSelectionMachine.TaskTypeCheckpointEvidence, trainedDecision.Value.EvidenceFeatures);
        Assert.Contains("NO_TRAINED_STATE_AVAILABLE", emptyDecision.Value.EvidenceFeatures);
    }

    [Fact]
    public async Task SkillSelectionMachine_RoutingUsesCheckpointSectionForHeldOutRoutePrompt()
    {
        var trainingRequest = new SkillSelectionRequest(
            Prompt: "Route this request through prompt-to-task before reasoning because the task contract is missing.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["prompt-to-task-route", "result-gap-route"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS05-TRAIN-001");
        var heldOutRequest = new SkillSelectionRequest(
            Prompt: "Route this request to prompt-to-task first because structured task details are absent.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["result-gap-route", "prompt-to-task-route"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS05-HELDOUT-001");
        var machine = new SkillSelectionMachine();
        var training = await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss05-routing-pair-001",
                    Request: trainingRequest,
                    Chosen: new SkillSelectionDecision(["prompt-to-task-route"], ["result-gap-route"], ["route_to_prompt_to_task"], 1.0, false, "Route to prompt-to-task."),
                    Rejected: new SkillSelectionDecision(["result-gap-route"], ["prompt-to-task-route"], ["result_gap_near_miss"], 0.1, true, "Result-gap route is adjacent but premature."),
                    RejectedType: "adjacent_wrong_route",
                    SourcePromptEntryId: "UPS-SS05-TRAIN-001",
                    TargetTrainingAdapter: SkillSelectionMachine.RoutingTrainingAdapter)
            ]);

        var checkpoint = machine.ExportCheckpoint();
        var fresh = new SkillSelectionMachine();
        fresh.LoadCheckpoint(checkpoint);
        var routingUnitState = fresh.ExportRoutingSkillUnitState();
        var routingUnit = new RoutingSkillUnit();
        var routingFeatures = SkillSelectionMachine.ExtractPromptUnderstandingSignals(heldOutRequest.Prompt);
        var nearestExamples = routingUnit.FindNearestRoutingExamples(routingFeatures, routingUnitState, 3);
        var promptToTaskSupport = routingUnit.ComputeRoutingSupport(routingFeatures, "prompt-to-task-route", routingUnitState, 3);
        var resultGapSupport = routingUnit.ComputeRoutingSupport(routingFeatures, "result-gap-route", routingUnitState, 3);
        var routingUnitEvidence = routingUnit.Evaluate(new RoutingSkillInput(
            Request: heldOutRequest,
            State: routingUnitState));
        var decision = fresh.DecideFromTrainedState(heldOutRequest);

        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.Empty(checkpoint.PromptUnderstanding.SkillProfiles);
        Assert.Empty(checkpoint.TaskUnderstanding.SkillProfiles);
        Assert.Empty(checkpoint.TaskType.SkillProfiles);
        Assert.Equal(SkillSelectionMachine.RoutingUnitId, checkpoint.Routing.UnitId);
        Assert.Equal(
            ["prompt-to-task-route", "result-gap-route"],
            checkpoint.Routing.SkillProfiles.Select(profile => profile.SkillId).Order(StringComparer.Ordinal).ToArray());
        Assert.Equal(SkillSelectionMachine.SkillCandidatesUnitId, checkpoint.SkillCandidates.UnitId);
        Assert.Empty(checkpoint.SkillCandidates.SkillProfiles);
        Assert.False(checkpoint.SkillCandidates.SemanticQualityClaimed);
        Assert.False(checkpoint.Routing.SemanticQualityClaimed);
        Assert.Equal(RoutingSkillUnit.UnitId, routingUnitState.UnitId);
        Assert.False(routingUnitState.SemanticQualityClaimed);
        var promptToTaskProfile = Assert.Single(
            routingUnitState.SkillProfiles,
            profile => string.Equals(profile.SkillId, "prompt-to-task-route", StringComparison.Ordinal));
        var resultGapProfile = Assert.Single(
            routingUnitState.SkillProfiles,
            profile => string.Equals(profile.SkillId, "result-gap-route", StringComparison.Ordinal));
        Assert.NotEmpty(promptToTaskProfile.RoutingFeatureWeights);
        Assert.All(promptToTaskProfile.RoutingFeatureWeights.Values, value => Assert.True(value > 0));
        Assert.NotEmpty(resultGapProfile.RoutingFeatureWeights);
        Assert.All(resultGapProfile.RoutingFeatureWeights.Values, value => Assert.True(value < 0));
        Assert.Contains("ss05-routing-pair-001", promptToTaskProfile.SourcePairIds);
        Assert.Contains("ss05-routing-pair-001", resultGapProfile.SourcePairIds);
        Assert.Contains(routingUnitState.RoutingFeatureVocabulary, signal => string.Equals(signal, "route", StringComparison.Ordinal));
        Assert.NotEmpty(routingUnitState.KnnMemory);
        Assert.NotEmpty(nearestExamples);
        Assert.All(nearestExamples, example =>
        {
            Assert.Equal("prompt-to-task-route", example.ChosenSkillId);
            Assert.DoesNotContain("UPS-SS05", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain("ss05-routing-pair", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain("C:/Projects", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain(example.RoutingFeatures, feature => feature.Contains("UPS-SS05", StringComparison.Ordinal));
            Assert.DoesNotContain(example.RoutingFeatures, feature => feature.Contains("C:/Projects", StringComparison.Ordinal));
        });
        Assert.True(promptToTaskSupport.SupportScore > 0.0);
        Assert.True(promptToTaskSupport.MatchedFeatureCount > 0);
        Assert.Equal(0.0, resultGapSupport.SupportScore);
        Assert.Equal(0, resultGapSupport.MatchedFeatureCount);
        Assert.True(routingUnitEvidence.HasDecision);
        Assert.Equal(["prompt-to-task-route"], routingUnitEvidence.SelectedSkillIds);
        Assert.False(routingUnitEvidence.SemanticQualityClaimed);
        Assert.True(routingUnitEvidence.BaselineCounterOnly);
        Assert.True(routingUnitEvidence.LearnedRoutingSignalScore > 0);
        Assert.Contains(RoutingSkillUnit.UnitExecutedEvidence, routingUnitEvidence.EvidenceFeatures);
        Assert.Contains(RoutingSkillUnit.BaselineEvidence, routingUnitEvidence.EvidenceFeatures);
        Assert.True(decision.IsSuccess, decision.ErrorMessage);
        Assert.Equal(["prompt-to-task-route"], decision.Value!.SelectedSkillIds);
        Assert.Contains(SkillSelectionMachine.RoutingCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(RoutingSkillUnit.UnitExecutedEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(RoutingSkillUnit.BaselineEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("routing:semantic_quality_claimed=false", decision.Value.EvidenceFeatures);
        Assert.Contains(decision.Value.EvidenceFeatures, evidence => evidence.StartsWith("routing:learned_routing_signal_score=", StringComparison.Ordinal));
        Assert.Contains(decision.Value.EvidenceFeatures, evidence => evidence.StartsWith("routing:nearest_example_count=", StringComparison.Ordinal));
        Assert.Contains(decision.Value.EvidenceFeatures, evidence => evidence.StartsWith("routing:support_score=", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("source_pair_count", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("UPS-SS05", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("ss05-routing-pair", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("C:/Projects", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains(trainingRequest.Prompt, StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("candidate_position", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("candidate_order", StringComparison.Ordinal));
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("routing selected", decision.Value.DecisionText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SkillSelectionMachine_RoutingAblationLowersDecisionWithoutUnitSection()
    {
        var request = new SkillSelectionRequest(
            Prompt: "Route this request to prompt-to-task first because structured task details are absent.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["result-gap-route", "prompt-to-task-route"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS05-ABLATION-001");
        var trained = new SkillSelectionMachine();
        await trained.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss05-routing-pair-002",
                    Request: request with { Prompt = "Route this request through prompt-to-task before reasoning because the task contract is missing." },
                    Chosen: new SkillSelectionDecision(["prompt-to-task-route"], ["result-gap-route"], ["route_to_prompt_to_task"], 1.0, false, "Route to prompt-to-task."),
                    Rejected: new SkillSelectionDecision(["result-gap-route"], ["prompt-to-task-route"], ["result_gap_near_miss"], 0.1, true, "Result-gap route is adjacent but premature."),
                    RejectedType: "adjacent_wrong_route",
                    SourcePromptEntryId: "UPS-SS05-ABLATION-TRAIN",
                    TargetTrainingAdapter: SkillSelectionMachine.RoutingTrainingAdapter)
            ]);
        var empty = new SkillSelectionMachine();

        var trainedDecision = trained.DecideFromTrainedState(request);
        var emptyDecision = empty.DecideFromTrainedState(request);

        Assert.True(trainedDecision.IsSuccess, trainedDecision.ErrorMessage);
        Assert.True(emptyDecision.IsSuccess, emptyDecision.ErrorMessage);
        Assert.Equal(["prompt-to-task-route"], trainedDecision.Value!.SelectedSkillIds);
        Assert.Empty(emptyDecision.Value!.SelectedSkillIds);
        Assert.True(trainedDecision.Value.Confidence > emptyDecision.Value.Confidence);
        Assert.Contains(SkillSelectionMachine.RoutingCheckpointEvidence, trainedDecision.Value.EvidenceFeatures);
        Assert.Contains("NO_TRAINED_STATE_AVAILABLE", emptyDecision.Value.EvidenceFeatures);
    }

    [Fact]
    public async Task SkillSelectionMachine_SkillCandidatesUsesCheckpointSectionForHeldOutCandidatePrompt()
    {
        var trainingRequest = new SkillSelectionRequest(
            Prompt: "Find candidate skills for coverage review before accepting the plan.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["coverage-completeness-gate", "image-generation"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS06-TRAIN-001");
        var heldOutRequest = new SkillSelectionRequest(
            Prompt: "Find candidate skills for coverage validation before plan acceptance.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "coverage-completeness-gate"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS06-HELDOUT-001");
        var machine = new SkillSelectionMachine();
        var training = await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss06-skill-candidates-pair-001",
                    Request: trainingRequest,
                    Chosen: new SkillSelectionDecision(["coverage-completeness-gate"], ["image-generation"], ["candidate_skill_for_coverage_review"], 1.0, false, "Coverage review candidate."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["coverage-completeness-gate"], ["visual_asset_near_miss"], 0.1, true, "Image generation is adjacent only for visual asset requests."),
                    RejectedType: "adjacent_wrong_candidate",
                    SourcePromptEntryId: "UPS-SS06-TRAIN-001",
                    TargetTrainingAdapter: SkillSelectionMachine.SkillCandidatesTrainingAdapter)
            ]);

        var checkpoint = machine.ExportCheckpoint();
        var fresh = new SkillSelectionMachine();
        fresh.LoadCheckpoint(checkpoint);
        var decision = fresh.DecideFromTrainedState(heldOutRequest);

        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.Empty(checkpoint.PromptUnderstanding.SkillProfiles);
        Assert.Empty(checkpoint.TaskUnderstanding.SkillProfiles);
        Assert.Empty(checkpoint.TaskType.SkillProfiles);
        Assert.Empty(checkpoint.Routing.SkillProfiles);
        Assert.Equal(SkillSelectionMachine.SkillCandidatesUnitId, checkpoint.SkillCandidates.UnitId);
        Assert.False(checkpoint.SkillCandidates.SemanticQualityClaimed);
        var checkpointProfiles = checkpoint.SkillCandidates.SkillProfiles
            .ToDictionary(profile => profile.SkillId, StringComparer.Ordinal);
        Assert.Equal(2, checkpointProfiles.Count);
        Assert.Contains("ss06-skill-candidates-pair-001", checkpointProfiles["coverage-completeness-gate"].SourcePairIds);
        Assert.All(checkpointProfiles["coverage-completeness-gate"].LearnedSignals.Values, value => Assert.True(value > 0));
        Assert.Contains("ss06-skill-candidates-pair-001", checkpointProfiles["image-generation"].SourcePairIds);
        Assert.All(checkpointProfiles["image-generation"].LearnedSignals.Values, value => Assert.True(value < 0));

        var skillCandidatesState = fresh.ExportSkillCandidatesSkillUnitState();
        Assert.Equal(SkillCandidatesSkillUnit.UnitId, skillCandidatesState.UnitId);
        Assert.False(skillCandidatesState.SemanticQualityClaimed);
        Assert.Contains("coverage", skillCandidatesState.CandidateFeatureVocabulary);
        Assert.Equal(2, skillCandidatesState.SkillProfiles.Count);
        Assert.Contains(skillCandidatesState.SkillProfiles, profile =>
            profile.SkillId == "coverage-completeness-gate"
            && profile.CandidateFeatureWeights.Values.All(value => value > 0));
        Assert.Contains(skillCandidatesState.SkillProfiles, profile =>
            profile.SkillId == "image-generation"
            && profile.CandidateFeatureWeights.Values.All(value => value < 0));

        var skillCandidatesUnit = new SkillCandidatesSkillUnit();
        var heldOutFeatures = SkillSelectionMachine.ExtractPromptUnderstandingSignals(heldOutRequest.Prompt);
        var nearest = skillCandidatesUnit.FindNearestSkillCandidateExamples(heldOutFeatures, skillCandidatesState, 3);
        var coverageSupport = skillCandidatesUnit.ComputeSkillCandidateSupport(
            heldOutFeatures,
            "coverage-completeness-gate",
            skillCandidatesState,
            3);
        var wrongSupport = skillCandidatesUnit.ComputeSkillCandidateSupport(
            heldOutFeatures,
            "image-generation",
            skillCandidatesState,
            3);
        Assert.NotEmpty(nearest);
        Assert.All(nearest, example =>
        {
            Assert.DoesNotContain("ss06-skill-candidates-pair-001", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain("UPS-SS06", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain("C:/Projects", example.ExampleId, StringComparison.Ordinal);
        });
        Assert.True(coverageSupport.SupportScore > 0.0);
        Assert.True(coverageSupport.MatchedFeatureCount > 0);
        Assert.Equal(0.0, wrongSupport.SupportScore);
        Assert.Equal(0, wrongSupport.MatchedFeatureCount);

        Assert.True(decision.IsSuccess, decision.ErrorMessage);
        Assert.Equal(["coverage-completeness-gate"], decision.Value!.SelectedSkillIds);
        Assert.Contains(SkillSelectionMachine.SkillCandidatesCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(SkillCandidatesSkillUnit.UnitExecutedEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(SkillCandidatesSkillUnit.BaselineEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("skill_candidates:semantic_quality_claimed=false", decision.Value.EvidenceFeatures);
        Assert.Contains("skill_candidates:knn_role=support_evidence_only", decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, decision.Value.EvidenceFeatures);
        var runtimeEvidence = string.Join("\n", decision.Value.EvidenceFeatures);
        Assert.DoesNotContain("source_pair_count", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("ss06-skill-candidates-pair-001", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("UPS-SS06", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain(trainingRequest.Prompt, runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Projects", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_position", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_order", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_index", runtimeEvidence, StringComparison.Ordinal);
        Assert.Contains("skill_candidates selected", decision.Value.DecisionText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SkillSelectionMachine_SkillCandidatesAblationLowersDecisionWithoutUnitSection()
    {
        var request = new SkillSelectionRequest(
            Prompt: "Find candidate skills for coverage validation before plan acceptance.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "coverage-completeness-gate"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS06-ABLATION-001");
        var trained = new SkillSelectionMachine();
        await trained.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss06-skill-candidates-pair-002",
                    Request: request with { Prompt = "Find candidate skills for coverage review before accepting the plan." },
                    Chosen: new SkillSelectionDecision(["coverage-completeness-gate"], ["image-generation"], ["candidate_skill_for_coverage_review"], 1.0, false, "Coverage review candidate."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["coverage-completeness-gate"], ["visual_asset_near_miss"], 0.1, true, "Visual asset near miss."),
                    RejectedType: "adjacent_wrong_candidate",
                    SourcePromptEntryId: "UPS-SS06-ABLATION-TRAIN",
                    TargetTrainingAdapter: SkillSelectionMachine.SkillCandidatesTrainingAdapter)
            ]);
        var empty = new SkillSelectionMachine();

        var trainedDecision = trained.DecideFromTrainedState(request);
        var emptyDecision = empty.DecideFromTrainedState(request);

        Assert.True(trainedDecision.IsSuccess, trainedDecision.ErrorMessage);
        Assert.True(emptyDecision.IsSuccess, emptyDecision.ErrorMessage);
        Assert.Equal(["coverage-completeness-gate"], trainedDecision.Value!.SelectedSkillIds);
        Assert.Empty(emptyDecision.Value!.SelectedSkillIds);
        Assert.True(trainedDecision.Value.Confidence > emptyDecision.Value.Confidence);
        Assert.Contains(SkillSelectionMachine.SkillCandidatesCheckpointEvidence, trainedDecision.Value.EvidenceFeatures);
        Assert.Contains("NO_TRAINED_STATE_AVAILABLE", emptyDecision.Value.EvidenceFeatures);
    }

    [Fact]
    public async Task SkillSelectionMachine_SkillSelectionUsesCheckpointSectionForHeldOutFinalSelectionPrompt()
    {
        var trainingRequest = new SkillSelectionRequest(
            Prompt: "Choose the final skill for claim verification after reviewing candidate coverage evidence.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["claim-verification", "image-generation"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS07-TRAIN-001");
        var heldOutRequest = new SkillSelectionRequest(
            Prompt: "Select the final skill for verifying claims after checking coverage evidence.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "claim-verification"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS07-HELDOUT-001");
        var machine = new SkillSelectionMachine();
        var training = await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss07-skill-selection-pair-001",
                    Request: trainingRequest,
                    Chosen: new SkillSelectionDecision(["claim-verification"], ["image-generation"], ["final_skill_for_claim_verification"], 1.0, false, "Claim verification is the final semantic selection."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["claim-verification"], ["visual_asset_near_miss"], 0.1, true, "Image generation is adjacent only for visual asset requests."),
                    RejectedType: "adjacent_wrong_final_skill",
                    SourcePromptEntryId: "UPS-SS07-TRAIN-001",
                    TargetTrainingAdapter: SkillSelectionMachine.SkillSelectionTrainingAdapter)
            ]);

        var checkpoint = machine.ExportCheckpoint();
        var fresh = new SkillSelectionMachine();
        fresh.LoadCheckpoint(checkpoint);
        var decision = fresh.DecideFromTrainedState(heldOutRequest);

        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.Empty(checkpoint.PromptUnderstanding.SkillProfiles);
        Assert.Empty(checkpoint.TaskUnderstanding.SkillProfiles);
        Assert.Empty(checkpoint.TaskType.SkillProfiles);
        Assert.Empty(checkpoint.Routing.SkillProfiles);
        Assert.Empty(checkpoint.SkillCandidates.SkillProfiles);
        Assert.Equal(SkillSelectionMachine.SkillSelectionUnitId, checkpoint.SkillSelection.UnitId);
        Assert.False(checkpoint.SkillSelection.SemanticQualityClaimed);
        var checkpointProfiles = checkpoint.SkillSelection.SkillProfiles
            .ToDictionary(profile => profile.SkillId, StringComparer.Ordinal);
        Assert.Equal(2, checkpointProfiles.Count);
        Assert.Contains("ss07-skill-selection-pair-001", checkpointProfiles["claim-verification"].SourcePairIds);
        Assert.All(checkpointProfiles["claim-verification"].LearnedSignals.Values, value => Assert.True(value > 0));
        Assert.Contains("ss07-skill-selection-pair-001", checkpointProfiles["image-generation"].SourcePairIds);
        Assert.All(checkpointProfiles["image-generation"].LearnedSignals.Values, value => Assert.True(value < 0));

        var skillSelectionState = fresh.ExportSkillSelectionSkillUnitState();
        Assert.Equal(SkillSelectionSkillUnit.UnitId, skillSelectionState.UnitId);
        Assert.False(skillSelectionState.SemanticQualityClaimed);
        Assert.Contains("claim", skillSelectionState.FinalSelectionFeatureVocabulary);
        Assert.Equal(2, skillSelectionState.SkillProfiles.Count);
        Assert.Contains(skillSelectionState.SkillProfiles, profile =>
            profile.SkillId == "claim-verification"
            && profile.FinalSelectionFeatureWeights.Values.All(value => value > 0));
        Assert.Contains(skillSelectionState.SkillProfiles, profile =>
            profile.SkillId == "image-generation"
            && profile.FinalSelectionFeatureWeights.Values.All(value => value < 0));

        var skillSelectionUnit = new SkillSelectionSkillUnit();
        var heldOutFeatures = SkillSelectionMachine.ExtractPromptUnderstandingSignals(heldOutRequest.Prompt);
        var nearest = skillSelectionUnit.FindNearestSkillSelectionExamples(heldOutFeatures, skillSelectionState, 3);
        var claimSupport = skillSelectionUnit.ComputeSkillSelectionSupport(
            heldOutFeatures,
            "claim-verification",
            skillSelectionState,
            3);
        var wrongSupport = skillSelectionUnit.ComputeSkillSelectionSupport(
            heldOutFeatures,
            "image-generation",
            skillSelectionState,
            3);
        Assert.NotEmpty(nearest);
        Assert.All(nearest, example =>
        {
            Assert.DoesNotContain("ss07-skill-selection-pair-001", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain("UPS-SS07", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain("C:/Projects", example.ExampleId, StringComparison.Ordinal);
        });
        Assert.True(claimSupport.SupportScore > 0.0);
        Assert.True(claimSupport.MatchedFeatureCount > 0);
        Assert.Equal(0.0, wrongSupport.SupportScore);
        Assert.Equal(0, wrongSupport.MatchedFeatureCount);

        Assert.True(decision.IsSuccess, decision.ErrorMessage);
        Assert.Equal(["claim-verification"], decision.Value!.SelectedSkillIds);
        Assert.Contains(SkillSelectionMachine.SkillSelectionCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(SkillSelectionSkillUnit.UnitExecutedEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(SkillSelectionSkillUnit.BaselineEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("skill_selection:semantic_quality_claimed=false", decision.Value.EvidenceFeatures);
        Assert.Contains("skill_selection:knn_role=support_evidence_only", decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.SkillCandidatesCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, decision.Value.EvidenceFeatures);
        var runtimeEvidence = string.Join("\n", decision.Value.EvidenceFeatures);
        Assert.DoesNotContain("source_pair_count", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("ss07-skill-selection-pair-001", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("UPS-SS07", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain(trainingRequest.Prompt, runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Projects", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_position", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_order", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_index", runtimeEvidence, StringComparison.Ordinal);
        Assert.Contains("skill_selection selected final skills", decision.Value.DecisionText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SkillSelectionMachine_SkillSelectionAblationLowersDecisionWithoutUnitSection()
    {
        var request = new SkillSelectionRequest(
            Prompt: "Select the final skill for verifying claims after checking coverage evidence.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "claim-verification"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS07-ABLATION-001");
        var trained = new SkillSelectionMachine();
        await trained.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss07-skill-selection-pair-002",
                    Request: request with { Prompt = "Choose the final skill for claim verification after reviewing candidate coverage evidence." },
                    Chosen: new SkillSelectionDecision(["claim-verification"], ["image-generation"], ["final_skill_for_claim_verification"], 1.0, false, "Claim verification final selection."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["claim-verification"], ["visual_asset_near_miss"], 0.1, true, "Visual asset near miss."),
                    RejectedType: "adjacent_wrong_final_skill",
                    SourcePromptEntryId: "UPS-SS07-ABLATION-TRAIN",
                    TargetTrainingAdapter: SkillSelectionMachine.SkillSelectionTrainingAdapter)
            ]);
        var empty = new SkillSelectionMachine();

        var trainedDecision = trained.DecideFromTrainedState(request);
        var emptyDecision = empty.DecideFromTrainedState(request);

        Assert.True(trainedDecision.IsSuccess, trainedDecision.ErrorMessage);
        Assert.True(emptyDecision.IsSuccess, emptyDecision.ErrorMessage);
        Assert.Equal(["claim-verification"], trainedDecision.Value!.SelectedSkillIds);
        Assert.Empty(emptyDecision.Value!.SelectedSkillIds);
        Assert.True(trainedDecision.Value.Confidence > emptyDecision.Value.Confidence);
        Assert.Contains(SkillSelectionMachine.SkillSelectionCheckpointEvidence, trainedDecision.Value.EvidenceFeatures);
        Assert.Contains("NO_TRAINED_STATE_AVAILABLE", emptyDecision.Value.EvidenceFeatures);
    }

    [Fact]
    public async Task SkillSelectionMachine_FinalSelectionStateIsNotShadowedByPromptContextState()
    {
        var request = new SkillSelectionRequest(
            Prompt: "Choose the final adaptive repair skill using redacted host facts and provenance details.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["context-audit", "adaptive-repair"],
            HostFacts: new Dictionary<string, string>
            {
                ["host_type"] = "code_agent",
                ["privacy_scope"] = "redacted"
            },
            UserPromptSourceEntryId: "UPS-SS07-QSG26-HELDOUT");
        var machine = new SkillSelectionMachine();

        var training = await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss07-qsg26-prompt-context-shadow-pair",
                    Request: request,
                    Chosen: new SkillSelectionDecision(["context-audit"], ["adaptive-repair"], ["redacted_context_audit"], 1.0, false, "Prompt context alone would prefer context audit."),
                    Rejected: new SkillSelectionDecision(["adaptive-repair"], ["context-audit"], ["wrong_without_final_selection"], 0.1, true, "Adaptive repair is wrong for prompt context only."),
                    RejectedType: "prompt_context_shadowing_final_selection",
                    SourcePromptEntryId: "UPS-SS07-QSG26-CONTEXT",
                    TargetTrainingAdapter: SkillSelectionMachine.PromptContextTrainingAdapter),
                new SkillSelectionMachinePair(
                    PairId: "ss07-qsg26-final-selection-pair",
                    Request: request,
                    Chosen: new SkillSelectionDecision(["adaptive-repair"], ["context-audit"], ["final_selection_adaptive_repair"], 1.0, false, "Final selection correction prefers adaptive repair."),
                    Rejected: new SkillSelectionDecision(["context-audit"], ["adaptive-repair"], ["context_only_near_miss"], 0.1, true, "Context audit is only the prompt-context near miss."),
                    RejectedType: "final_selection_overrides_context_near_miss",
                    SourcePromptEntryId: "UPS-SS07-QSG26-FINAL",
                    TargetTrainingAdapter: SkillSelectionMachine.SkillSelectionTrainingAdapter)
            ]);
        var checkpoint = machine.ExportCheckpoint();
        var fresh = new SkillSelectionMachine();
        fresh.LoadCheckpoint(checkpoint);

        var decision = fresh.DecideFromTrainedState(request);

        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.NotEmpty(checkpoint.PromptContext.SkillProfiles);
        Assert.NotEmpty(checkpoint.SkillSelection.SkillProfiles);
        Assert.True(decision.IsSuccess, decision.ErrorMessage);
        Assert.Equal(["adaptive-repair"], decision.Value!.SelectedSkillIds);
        Assert.Contains(SkillSelectionMachine.SkillSelectionCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(SkillSelectionSkillUnit.UnitExecutedEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(SkillSelectionSkillUnit.BaselineEvidence, decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.PromptContextCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("skill_selection selected final skills", decision.Value.DecisionText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SkillSelectionMachine_FinalSelectionReturnsEveryPositiveLearnedSkill()
    {
        var request = new SkillSelectionRequest(
            Prompt: "Select final plan review and claim verification skills after evidence coverage review.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "coverage-audit", "plan-review", "claim-verification"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS07-QSG26-MULTI-HELDOUT");
        var machine = new SkillSelectionMachine();

        var training = await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss07-qsg26-final-selection-multilabel-pair",
                    Request: request,
                    Chosen: new SkillSelectionDecision(["plan-review", "coverage-audit", "claim-verification"], ["image-generation"], ["final_selection_review_claim_and_coverage"], 1.0, false, "Plan review, coverage audit, and claim verification are required."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["plan-review", "coverage-audit", "claim-verification"], ["visual_asset_near_miss"], 0.1, true, "Image generation is adjacent only."),
                    RejectedType: "missing_multilabel_final_selection",
                    SourcePromptEntryId: "UPS-SS07-QSG26-MULTI",
                    TargetTrainingAdapter: SkillSelectionMachine.SkillSelectionTrainingAdapter),
                new SkillSelectionMachinePair(
                    PairId: "ss07-qsg26-final-selection-score-order-pair",
                    Request: request,
                    Chosen: new SkillSelectionDecision(["plan-review"], ["image-generation"], ["final_selection_plan_review_extra_support"], 1.0, false, "Plan review has stronger final selection support."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["plan-review"], ["visual_asset_still_wrong"], 0.1, true, "Image generation remains adjacent only."),
                    RejectedType: "score_order_final_selection",
                    SourcePromptEntryId: "UPS-SS07-QSG26-MULTI-ORDER",
                    TargetTrainingAdapter: SkillSelectionMachine.SkillSelectionTrainingAdapter)
            ]);

        var decision = machine.DecideFromTrainedState(request);

        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(decision.IsSuccess, decision.ErrorMessage);
        Assert.Equal(
            ["plan-review", "claim-verification", "coverage-audit"],
            decision.Value!.SelectedSkillIds);
        Assert.Equal(["image-generation"], decision.Value.RejectedSkillIds);
        Assert.Contains(SkillSelectionMachine.SkillSelectionCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("skill_selection:positive_selected_skill_count=3", decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, decision.Value.EvidenceFeatures);
    }

    [Fact]
    public async Task SkillSelectionMachine_FinalSelectionStateIsNotShadowedByLaterSemanticStates()
    {
        var shadowCases = new[]
        {
            new
            {
                UnitName = "result-gap",
                ShadowSkillId = "gap-repair",
                Adapter = SkillSelectionMachine.ResultGapTrainingAdapter,
                CheckpointEvidence = SkillSelectionMachine.ResultGapCheckpointEvidence
            },
            new
            {
                UnitName = "output-validation",
                ShadowSkillId = "output-validation",
                Adapter = SkillSelectionMachine.OutputValidationTrainingAdapter,
                CheckpointEvidence = SkillSelectionMachine.OutputValidationCheckpointEvidence
            },
            new
            {
                UnitName = "task-completion",
                ShadowSkillId = "task-completion",
                Adapter = SkillSelectionMachine.TaskCompletionTrainingAdapter,
                CheckpointEvidence = SkillSelectionMachine.TaskCompletionCheckpointEvidence
            },
            new
            {
                UnitName = "intermediate-checks",
                ShadowSkillId = "intermediate-checks",
                Adapter = SkillSelectionMachine.IntermediateChecksTrainingAdapter,
                CheckpointEvidence = SkillSelectionMachine.IntermediateChecksCheckpointEvidence
            }
        };

        foreach (var shadowCase in shadowCases)
        {
            var request = new SkillSelectionRequest(
                Prompt: $"Choose the final adaptive repair skill after reviewing {shadowCase.UnitName} evidence and remaining quality gates.",
                ProjectRoot: "C:/Projects/llm_mvp_core",
                AvailableSkillIds: [shadowCase.ShadowSkillId, "adaptive-repair"],
                HostFacts: new Dictionary<string, string>(),
                UserPromptSourceEntryId: $"UPS-SS07-QSG26-{shadowCase.UnitName.ToUpperInvariant()}-HELDOUT");
            var shadowPair = new SkillSelectionMachinePair(
                PairId: $"ss07-qsg26-{shadowCase.UnitName}-shadow-pair",
                Request: request,
                Chosen: new SkillSelectionDecision([shadowCase.ShadowSkillId], ["adaptive-repair"], [$"{shadowCase.UnitName}_shadow_signal"], 1.0, false, $"{shadowCase.UnitName} alone would select its shadow skill."),
                Rejected: new SkillSelectionDecision(["adaptive-repair"], [shadowCase.ShadowSkillId], ["wrong_without_final_selection"], 0.1, true, "Adaptive repair is wrong without final-selection authority."),
                RejectedType: $"{shadowCase.UnitName}_shadowing_final_selection",
                SourcePromptEntryId: $"UPS-SS07-QSG26-{shadowCase.UnitName.ToUpperInvariant()}-SHADOW",
                TargetTrainingAdapter: shadowCase.Adapter);
            var finalSelectionPair = new SkillSelectionMachinePair(
                PairId: $"ss07-qsg26-{shadowCase.UnitName}-final-selection-pair",
                Request: request,
                Chosen: new SkillSelectionDecision(["adaptive-repair"], [shadowCase.ShadowSkillId], ["final_selection_adaptive_repair"], 1.0, false, "Final selection correction prefers adaptive repair."),
                Rejected: new SkillSelectionDecision([shadowCase.ShadowSkillId], ["adaptive-repair"], [$"{shadowCase.UnitName}_near_miss"], 0.1, true, $"{shadowCase.UnitName} is only the later-unit near miss."),
                RejectedType: $"final_selection_overrides_{shadowCase.UnitName}_near_miss",
                SourcePromptEntryId: $"UPS-SS07-QSG26-{shadowCase.UnitName.ToUpperInvariant()}-FINAL",
                TargetTrainingAdapter: SkillSelectionMachine.SkillSelectionTrainingAdapter);
            var shadowOnlyMachine = new SkillSelectionMachine();
            var shadowOnlyTraining = await shadowOnlyMachine.TrainMachineBatchAsync([shadowPair]);
            var shadowOnlyDecision = shadowOnlyMachine.DecideFromTrainedState(request);
            var combinedMachine = new SkillSelectionMachine();
            var combinedTraining = await combinedMachine.TrainMachineBatchAsync([shadowPair, finalSelectionPair]);

            var combinedDecision = combinedMachine.DecideFromTrainedState(request);

            Assert.True(shadowOnlyTraining.IsSuccess, shadowOnlyTraining.ErrorMessage);
            Assert.True(shadowOnlyDecision.IsSuccess, shadowOnlyDecision.ErrorMessage);
            Assert.Equal([shadowCase.ShadowSkillId], shadowOnlyDecision.Value!.SelectedSkillIds);
            Assert.Contains(shadowCase.CheckpointEvidence, shadowOnlyDecision.Value.EvidenceFeatures);
            Assert.True(combinedTraining.IsSuccess, combinedTraining.ErrorMessage);
            Assert.True(combinedDecision.IsSuccess, combinedDecision.ErrorMessage);
            Assert.Equal(["adaptive-repair"], combinedDecision.Value!.SelectedSkillIds);
            Assert.Contains(SkillSelectionMachine.SkillSelectionCheckpointEvidence, combinedDecision.Value.EvidenceFeatures);
            Assert.Contains(SkillSelectionSkillUnit.UnitExecutedEvidence, combinedDecision.Value.EvidenceFeatures);
            Assert.Contains(SkillSelectionSkillUnit.BaselineEvidence, combinedDecision.Value.EvidenceFeatures);
            Assert.DoesNotContain(shadowCase.CheckpointEvidence, combinedDecision.Value.EvidenceFeatures);
            Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, combinedDecision.Value.EvidenceFeatures);
            Assert.Contains("skill_selection selected final skills", combinedDecision.Value.DecisionText, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task SkillSelectionMachine_PromptContextUsesCheckpointSectionForHeldOutContextPrompt()
    {
        var trainingRequest = new SkillSelectionRequest(
            Prompt: "Use redacted host facts and provenance context to choose the context audit skill for a safe session.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["context-audit", "image-generation"],
            HostFacts: new Dictionary<string, string>
            {
                ["host_type"] = "code_agent",
                ["privacy_scope"] = "redacted"
            },
            UserPromptSourceEntryId: "UPS-SS08-TRAIN-001");
        var heldOutRequest = new SkillSelectionRequest(
            Prompt: "Choose the context audit skill using redacted host facts and provenance details for this session.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "context-audit"],
            HostFacts: new Dictionary<string, string>
            {
                ["host_type"] = "code_agent",
                ["privacy_scope"] = "redacted"
            },
            UserPromptSourceEntryId: "UPS-SS08-HELDOUT-001");
        var machine = new SkillSelectionMachine();
        var training = await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss08-prompt-context-pair-001",
                    Request: trainingRequest,
                    Chosen: new SkillSelectionDecision(["context-audit"], ["image-generation"], ["redacted_host_provenance_context"], 1.0, false, "Context audit matches redacted host and provenance context."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["context-audit"], ["visual_asset_near_miss"], 0.1, true, "Image generation is adjacent only for visual asset requests."),
                    RejectedType: "adjacent_wrong_context_skill",
                    SourcePromptEntryId: "UPS-SS08-TRAIN-001",
                    TargetTrainingAdapter: SkillSelectionMachine.PromptContextTrainingAdapter)
            ]);

        var checkpoint = machine.ExportCheckpoint();
        var fresh = new SkillSelectionMachine();
        fresh.LoadCheckpoint(checkpoint);
        var promptContextUnitState = fresh.ExportPromptContextSkillSelectionUnitState();
        var decision = fresh.DecideFromTrainedState(heldOutRequest);
        var promptContextUnitEvidence = new PromptContextSkillSelectionUnit().Evaluate(new PromptContextSkillInput(
            Request: heldOutRequest,
            State: promptContextUnitState));
        var contextStrippedDecision = fresh.DecideFromTrainedState(heldOutRequest with
        {
            ProjectRoot = "",
            HostFacts = new Dictionary<string, string>(),
            UserPromptSourceEntryId = ""
        });

        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.Empty(checkpoint.PromptUnderstanding.SkillProfiles);
        Assert.Empty(checkpoint.TaskUnderstanding.SkillProfiles);
        Assert.Empty(checkpoint.TaskType.SkillProfiles);
        Assert.Empty(checkpoint.Routing.SkillProfiles);
        Assert.Empty(checkpoint.SkillCandidates.SkillProfiles);
        Assert.Empty(checkpoint.SkillSelection.SkillProfiles);
        Assert.Equal(SkillSelectionMachine.PromptContextUnitId, checkpoint.PromptContext.UnitId);
        Assert.Equal(
            ["context-audit", "image-generation"],
            checkpoint.PromptContext.SkillProfiles.Select(profile => profile.SkillId).Order(StringComparer.Ordinal).ToArray());
        var contextAuditProfile = Assert.Single(
            checkpoint.PromptContext.SkillProfiles,
            profile => string.Equals(profile.SkillId, "context-audit", StringComparison.Ordinal));
        var imageGenerationProfile = Assert.Single(
            checkpoint.PromptContext.SkillProfiles,
            profile => string.Equals(profile.SkillId, "image-generation", StringComparison.Ordinal));
        Assert.Contains("ss08-prompt-context-pair-001", contextAuditProfile.SourcePairIds);
        Assert.Contains("ss08-prompt-context-pair-001", imageGenerationProfile.SourcePairIds);
        Assert.All(contextAuditProfile.LearnedSignals.Values, value => Assert.True(value > 0));
        Assert.Contains(contextAuditProfile.LearnedSignals, signal => signal.Key.StartsWith("host_key:", StringComparison.Ordinal));
        Assert.Contains(contextAuditProfile.LearnedSignals, signal => signal.Key.StartsWith("host_value:", StringComparison.Ordinal));
        Assert.Contains("project_root_present", contextAuditProfile.LearnedSignals.Keys);
        Assert.NotEmpty(imageGenerationProfile.LearnedSignals);
        Assert.All(imageGenerationProfile.LearnedSignals, signal =>
        {
            Assert.StartsWith("prompt:", signal.Key, StringComparison.Ordinal);
            Assert.True(signal.Value < 0);
        });
        Assert.Equal(PromptContextSkillSelectionUnit.UnitId, promptContextUnitState.UnitId);
        Assert.False(promptContextUnitState.SemanticQualityClaimed);
        Assert.Equal(
            ["context-audit", "image-generation"],
            promptContextUnitState.SkillProfiles.Select(profile => profile.SkillId).Order(StringComparer.Ordinal).ToArray());
        var typedContextAuditProfile = Assert.Single(
            promptContextUnitState.SkillProfiles,
            profile => string.Equals(profile.SkillId, "context-audit", StringComparison.Ordinal));
        var typedImageGenerationProfile = Assert.Single(
            promptContextUnitState.SkillProfiles,
            profile => string.Equals(profile.SkillId, "image-generation", StringComparison.Ordinal));
        Assert.NotEmpty(typedContextAuditProfile.PromptFeatureWeights);
        Assert.NotEmpty(typedContextAuditProfile.ContextFeatureWeights);
        Assert.All(typedContextAuditProfile.PromptFeatureWeights.Keys, signal => Assert.StartsWith("prompt:", signal, StringComparison.Ordinal));
        Assert.Contains(typedContextAuditProfile.ContextFeatureWeights.Keys, signal => signal.StartsWith("host_key:", StringComparison.Ordinal));
        Assert.Contains(typedContextAuditProfile.ContextFeatureWeights.Keys, signal => signal.StartsWith("host_value:", StringComparison.Ordinal));
        Assert.Contains("project_root_present", typedContextAuditProfile.ContextFeatureWeights.Keys);
        Assert.NotEmpty(typedImageGenerationProfile.PromptFeatureWeights);
        Assert.Empty(typedImageGenerationProfile.ContextFeatureWeights);
        Assert.All(typedImageGenerationProfile.PromptFeatureWeights, signal =>
        {
            Assert.StartsWith("prompt:", signal.Key, StringComparison.Ordinal);
            Assert.True(signal.Value < 0);
        });
        Assert.Contains(promptContextUnitState.PromptFeatureVocabulary, signal => signal.StartsWith("prompt:", StringComparison.Ordinal));
        Assert.Contains(promptContextUnitState.ContextFeatureVocabulary, signal => signal.StartsWith("host_key:", StringComparison.Ordinal));
        Assert.True(promptContextUnitEvidence.HasDecision);
        Assert.Equal(["context-audit"], promptContextUnitEvidence.SelectedSkillIds);
        Assert.False(promptContextUnitEvidence.SemanticQualityClaimed);
        Assert.True(promptContextUnitEvidence.BaselineCounterOnly);
        Assert.True(promptContextUnitEvidence.PromptScore > 0);
        Assert.True(promptContextUnitEvidence.ContextScore > 0);
        Assert.Contains(PromptContextSkillSelectionUnit.UnitExecutedEvidence, promptContextUnitEvidence.EvidenceFeatures);
        Assert.Contains(PromptContextSkillSelectionUnit.BaselineEvidence, promptContextUnitEvidence.EvidenceFeatures);
        Assert.False(checkpoint.PromptContext.SemanticQualityClaimed);
        Assert.True(decision.IsSuccess, decision.ErrorMessage);
        Assert.Equal(["context-audit"], decision.Value!.SelectedSkillIds);
        Assert.Contains(SkillSelectionMachine.PromptContextCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(PromptContextSkillSelectionUnit.UnitExecutedEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(PromptContextSkillSelectionUnit.BaselineEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("prompt_context:semantic_quality_claimed=false", decision.Value.EvidenceFeatures);
        Assert.Contains(decision.Value.EvidenceFeatures, evidence => evidence.StartsWith("prompt_context:prompt_score=", StringComparison.Ordinal));
        Assert.Contains(decision.Value.EvidenceFeatures, evidence => evidence.StartsWith("prompt_context:context_score=", StringComparison.Ordinal));
        Assert.Contains(decision.Value.EvidenceFeatures, evidence => evidence.StartsWith("prompt_context:source_profile_count=", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("UPS-SS08", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("ss08-prompt-context-pair", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("C:/Projects", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains(trainingRequest.Prompt, StringComparison.Ordinal));
        Assert.DoesNotContain(SkillSelectionMachine.SkillSelectionCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("prompt_context selected", decision.Value.DecisionText, StringComparison.Ordinal);
        Assert.True(contextStrippedDecision.IsSuccess, contextStrippedDecision.ErrorMessage);
        Assert.True(decision.Value.Confidence > contextStrippedDecision.Value!.Confidence);
    }

    [Fact]
    public async Task SkillSelectionMachine_PromptContextAblationLowersDecisionWithoutUnitSection()
    {
        var request = new SkillSelectionRequest(
            Prompt: "Choose the context audit skill using redacted host facts and provenance details for this session.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "context-audit"],
            HostFacts: new Dictionary<string, string>
            {
                ["host_type"] = "code_agent",
                ["privacy_scope"] = "redacted"
            },
            UserPromptSourceEntryId: "UPS-SS08-ABLATION-001");
        var trained = new SkillSelectionMachine();
        await trained.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss08-prompt-context-pair-002",
                    Request: request with { Prompt = "Use redacted host facts and provenance context to choose the context audit skill for a safe session." },
                    Chosen: new SkillSelectionDecision(["context-audit"], ["image-generation"], ["redacted_host_provenance_context"], 1.0, false, "Context audit matches redacted host and provenance context."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["context-audit"], ["visual_asset_near_miss"], 0.1, true, "Visual asset near miss."),
                    RejectedType: "adjacent_wrong_context_skill",
                    SourcePromptEntryId: "UPS-SS08-ABLATION-TRAIN",
                    TargetTrainingAdapter: SkillSelectionMachine.PromptContextTrainingAdapter)
            ]);
        var empty = new SkillSelectionMachine();

        var trainedDecision = trained.DecideFromTrainedState(request);
        var contextStrippedDecision = trained.DecideFromTrainedState(request with
        {
            ProjectRoot = "",
            HostFacts = new Dictionary<string, string>(),
            UserPromptSourceEntryId = ""
        });
        var emptyDecision = empty.DecideFromTrainedState(request);

        Assert.True(trainedDecision.IsSuccess, trainedDecision.ErrorMessage);
        Assert.True(contextStrippedDecision.IsSuccess, contextStrippedDecision.ErrorMessage);
        Assert.True(emptyDecision.IsSuccess, emptyDecision.ErrorMessage);
        Assert.Equal(["context-audit"], trainedDecision.Value!.SelectedSkillIds);
        Assert.Empty(emptyDecision.Value!.SelectedSkillIds);
        Assert.True(trainedDecision.Value.Confidence > emptyDecision.Value.Confidence);
        Assert.True(trainedDecision.Value.Confidence > contextStrippedDecision.Value!.Confidence);
        Assert.Contains(SkillSelectionMachine.PromptContextCheckpointEvidence, trainedDecision.Value.EvidenceFeatures);
        Assert.Contains("NO_TRAINED_STATE_AVAILABLE", emptyDecision.Value.EvidenceFeatures);
    }

    [Fact]
    public async Task SkillSelectionMachine_ResultGapUsesCheckpointSectionForHeldOutGapPrompt()
    {
        var trainingRequest = new SkillSelectionRequest(
            Prompt: "After the answer, detect the missing review skill and require a second pass before completion.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["plan-review", "image-generation"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS09-TRAIN-001");
        var heldOutRequest = new SkillSelectionRequest(
            Prompt: "Inspect the output for a missing review skill before marking the work complete.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "plan-review"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS09-HELDOUT-001");
        var machine = new SkillSelectionMachine();
        var training = await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss09-result-gap-pair-001",
                    Request: trainingRequest,
                    Chosen: new SkillSelectionDecision(["plan-review"], ["image-generation"], ["missing_review_skill_after_output"], 1.0, false, "Plan review repairs the missing post-output skill gap."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["plan-review"], ["visual_asset_near_miss"], 0.1, true, "Image generation is adjacent only for visual asset requests."),
                    RejectedType: "adjacent_wrong_gap_skill",
                    SourcePromptEntryId: "UPS-SS09-TRAIN-001",
                    TargetTrainingAdapter: SkillSelectionMachine.ResultGapTrainingAdapter)
            ]);

        var checkpoint = machine.ExportCheckpoint();
        var fresh = new SkillSelectionMachine();
        fresh.LoadCheckpoint(checkpoint);
        var decision = fresh.DecideFromTrainedState(heldOutRequest);

        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.Empty(checkpoint.PromptUnderstanding.SkillProfiles);
        Assert.Empty(checkpoint.TaskUnderstanding.SkillProfiles);
        Assert.Empty(checkpoint.TaskType.SkillProfiles);
        Assert.Empty(checkpoint.Routing.SkillProfiles);
        Assert.Empty(checkpoint.SkillCandidates.SkillProfiles);
        Assert.Empty(checkpoint.SkillSelection.SkillProfiles);
        Assert.Empty(checkpoint.PromptContext.SkillProfiles);
        Assert.Equal(SkillSelectionMachine.ResultGapUnitId, checkpoint.ResultGap.UnitId);
        Assert.False(checkpoint.ResultGap.SemanticQualityClaimed);
        var checkpointProfiles = checkpoint.ResultGap.SkillProfiles
            .ToDictionary(profile => profile.SkillId, StringComparer.Ordinal);
        Assert.Equal(2, checkpointProfiles.Count);
        Assert.Contains("ss09-result-gap-pair-001", checkpointProfiles["plan-review"].SourcePairIds);
        Assert.All(checkpointProfiles["plan-review"].LearnedSignals.Values, value => Assert.True(value > 0));
        Assert.Contains("ss09-result-gap-pair-001", checkpointProfiles["image-generation"].SourcePairIds);
        Assert.All(checkpointProfiles["image-generation"].LearnedSignals.Values, value => Assert.True(value < 0));

        var resultGapState = fresh.ExportResultGapSkillUnitState();
        Assert.Equal(ResultGapSkillUnit.UnitId, resultGapState.UnitId);
        Assert.False(resultGapState.SemanticQualityClaimed);
        Assert.Contains("missing", resultGapState.ResultGapFeatureVocabulary);
        Assert.Equal(2, resultGapState.SkillProfiles.Count);
        Assert.Contains(resultGapState.SkillProfiles, profile =>
            profile.SkillId == "plan-review"
            && profile.ResultGapFeatureWeights.Values.All(value => value > 0));
        Assert.Contains(resultGapState.SkillProfiles, profile =>
            profile.SkillId == "image-generation"
            && profile.ResultGapFeatureWeights.Values.All(value => value < 0));

        var resultGapUnit = new ResultGapSkillUnit();
        var heldOutFeatures = SkillSelectionMachine.ExtractPromptUnderstandingSignals(heldOutRequest.Prompt);
        var nearest = resultGapUnit.FindNearestResultGapExamples(heldOutFeatures, resultGapState, 3);
        var planReviewSupport = resultGapUnit.ComputeResultGapSupport(
            heldOutFeatures,
            "plan-review",
            resultGapState,
            3);
        var wrongSupport = resultGapUnit.ComputeResultGapSupport(
            heldOutFeatures,
            "image-generation",
            resultGapState,
            3);
        Assert.NotEmpty(nearest);
        Assert.All(nearest, example =>
        {
            Assert.DoesNotContain("ss09-result-gap-pair-001", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain("UPS-SS09", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain("C:/Projects", example.ExampleId, StringComparison.Ordinal);
        });
        Assert.True(planReviewSupport.SupportScore > 0.0);
        Assert.True(planReviewSupport.MatchedFeatureCount > 0);
        Assert.Equal(0.0, wrongSupport.SupportScore);
        Assert.Equal(0, wrongSupport.MatchedFeatureCount);

        Assert.True(decision.IsSuccess, decision.ErrorMessage);
        Assert.Equal(["plan-review"], decision.Value!.SelectedSkillIds);
        Assert.Contains(SkillSelectionMachine.ResultGapCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(ResultGapSkillUnit.UnitExecutedEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(ResultGapSkillUnit.BaselineEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("result_gap:semantic_quality_claimed=false", decision.Value.EvidenceFeatures);
        Assert.Contains("result_gap:knn_role=support_evidence_only", decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.PromptContextCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, decision.Value.EvidenceFeatures);
        var runtimeEvidence = string.Join("\n", decision.Value.EvidenceFeatures);
        Assert.DoesNotContain("source_pair_count", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("ss09-result-gap-pair-001", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("UPS-SS09", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain(trainingRequest.Prompt, runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Projects", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_position", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_order", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_index", runtimeEvidence, StringComparison.Ordinal);
        Assert.Contains("result_gap selected", decision.Value.DecisionText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SkillSelectionMachine_ResultGapAblationLowersDecisionWithoutUnitSection()
    {
        var request = new SkillSelectionRequest(
            Prompt: "Inspect the output for a missing review skill before marking the work complete.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "plan-review"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS09-ABLATION-001");
        var trained = new SkillSelectionMachine();
        await trained.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss09-result-gap-pair-002",
                    Request: request with { Prompt = "After the answer, detect the missing review skill and require a second pass before completion." },
                    Chosen: new SkillSelectionDecision(["plan-review"], ["image-generation"], ["missing_review_skill_after_output"], 1.0, false, "Result gap repair skill."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["plan-review"], ["visual_asset_near_miss"], 0.1, true, "Visual asset near miss."),
                    RejectedType: "adjacent_wrong_gap_skill",
                    SourcePromptEntryId: "UPS-SS09-ABLATION-TRAIN",
                    TargetTrainingAdapter: SkillSelectionMachine.ResultGapTrainingAdapter)
            ]);
        var empty = new SkillSelectionMachine();

        var trainedDecision = trained.DecideFromTrainedState(request);
        var emptyDecision = empty.DecideFromTrainedState(request);

        Assert.True(trainedDecision.IsSuccess, trainedDecision.ErrorMessage);
        Assert.True(emptyDecision.IsSuccess, emptyDecision.ErrorMessage);
        Assert.Equal(["plan-review"], trainedDecision.Value!.SelectedSkillIds);
        Assert.Empty(emptyDecision.Value!.SelectedSkillIds);
        Assert.True(trainedDecision.Value.Confidence > emptyDecision.Value.Confidence);
        Assert.Contains(SkillSelectionMachine.ResultGapCheckpointEvidence, trainedDecision.Value.EvidenceFeatures);
        Assert.Contains("NO_TRAINED_STATE_AVAILABLE", emptyDecision.Value.EvidenceFeatures);
    }

    [Fact]
    public async Task SkillSelectionMachine_OutputValidationUsesCheckpointSectionForHeldOutValidationPrompt()
    {
        var trainingRequest = new SkillSelectionRequest(
            Prompt: "Validate whether the answer satisfies task constraints and required evidence before acceptance.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["output-validation", "image-generation"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS10-TRAIN-001");
        var heldOutRequest = new SkillSelectionRequest(
            Prompt: "Check the response against the requested constraints and evidence before accepting it.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "output-validation"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS10-HELDOUT-001");
        var machine = new SkillSelectionMachine();
        var training = await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss10-output-validation-pair-001",
                    Request: trainingRequest,
                    Chosen: new SkillSelectionDecision(["output-validation"], ["image-generation"], ["answer_constraint_evidence_validation"], 1.0, false, "Output validation checks answer constraints and evidence."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["output-validation"], ["visual_asset_near_miss"], 0.1, true, "Image generation is adjacent only for visual asset requests."),
                    RejectedType: "adjacent_wrong_validation_skill",
                    SourcePromptEntryId: "UPS-SS10-TRAIN-001",
                    TargetTrainingAdapter: SkillSelectionMachine.OutputValidationTrainingAdapter)
            ]);

        var checkpoint = machine.ExportCheckpoint();
        var fresh = new SkillSelectionMachine();
        fresh.LoadCheckpoint(checkpoint);
        var decision = fresh.DecideFromTrainedState(heldOutRequest);

        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.Empty(checkpoint.PromptUnderstanding.SkillProfiles);
        Assert.Empty(checkpoint.TaskUnderstanding.SkillProfiles);
        Assert.Empty(checkpoint.TaskType.SkillProfiles);
        Assert.Empty(checkpoint.Routing.SkillProfiles);
        Assert.Empty(checkpoint.SkillCandidates.SkillProfiles);
        Assert.Empty(checkpoint.SkillSelection.SkillProfiles);
        Assert.Empty(checkpoint.PromptContext.SkillProfiles);
        Assert.Empty(checkpoint.ResultGap.SkillProfiles);
        Assert.Equal(SkillSelectionMachine.OutputValidationUnitId, checkpoint.OutputValidation.UnitId);
        Assert.False(checkpoint.OutputValidation.SemanticQualityClaimed);
        var checkpointProfiles = checkpoint.OutputValidation.SkillProfiles
            .ToDictionary(profile => profile.SkillId, StringComparer.Ordinal);
        Assert.Equal(2, checkpointProfiles.Count);
        Assert.Contains("ss10-output-validation-pair-001", checkpointProfiles["output-validation"].SourcePairIds);
        Assert.All(checkpointProfiles["output-validation"].LearnedSignals.Values, value => Assert.True(value > 0));
        Assert.Contains("ss10-output-validation-pair-001", checkpointProfiles["image-generation"].SourcePairIds);
        Assert.All(checkpointProfiles["image-generation"].LearnedSignals.Values, value => Assert.True(value < 0));

        var outputValidationState = fresh.ExportOutputValidationSkillUnitState();
        Assert.Equal(OutputValidationSkillUnit.UnitId, outputValidationState.UnitId);
        Assert.False(outputValidationState.SemanticQualityClaimed);
        Assert.Contains("evidence", outputValidationState.ValidationFeatureVocabulary);
        Assert.Equal(2, outputValidationState.SkillProfiles.Count);
        Assert.Contains(outputValidationState.SkillProfiles, profile =>
            profile.SkillId == "output-validation"
            && profile.ValidationFeatureWeights.Values.All(value => value > 0));
        Assert.Contains(outputValidationState.SkillProfiles, profile =>
            profile.SkillId == "image-generation"
            && profile.ValidationFeatureWeights.Values.All(value => value < 0));

        var outputValidationUnit = new OutputValidationSkillUnit();
        var heldOutFeatures = SkillSelectionMachine.ExtractPromptUnderstandingSignals(heldOutRequest.Prompt);
        var nearest = outputValidationUnit.FindNearestOutputValidationExamples(heldOutFeatures, outputValidationState, 3);
        var validationSupport = outputValidationUnit.ComputeOutputValidationSupport(
            heldOutFeatures,
            "output-validation",
            outputValidationState,
            3);
        var wrongSupport = outputValidationUnit.ComputeOutputValidationSupport(
            heldOutFeatures,
            "image-generation",
            outputValidationState,
            3);
        Assert.NotEmpty(nearest);
        Assert.All(nearest, example =>
        {
            Assert.DoesNotContain("ss10-output-validation-pair-001", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain("UPS-SS10", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain("C:/Projects", example.ExampleId, StringComparison.Ordinal);
        });
        Assert.True(validationSupport.SupportScore > 0.0);
        Assert.True(validationSupport.MatchedFeatureCount > 0);
        Assert.Equal(0.0, wrongSupport.SupportScore);
        Assert.Equal(0, wrongSupport.MatchedFeatureCount);

        Assert.True(decision.IsSuccess, decision.ErrorMessage);
        Assert.Equal(["output-validation"], decision.Value!.SelectedSkillIds);
        Assert.Contains(SkillSelectionMachine.OutputValidationCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(OutputValidationSkillUnit.UnitExecutedEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(OutputValidationSkillUnit.BaselineEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("output_validation:semantic_quality_claimed=false", decision.Value.EvidenceFeatures);
        Assert.Contains("output_validation:knn_role=support_evidence_only", decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.ResultGapCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, decision.Value.EvidenceFeatures);
        var runtimeEvidence = string.Join("\n", decision.Value.EvidenceFeatures);
        Assert.DoesNotContain("source_pair_count", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("ss10-output-validation-pair-001", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("UPS-SS10", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain(trainingRequest.Prompt, runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Projects", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_position", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_order", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_index", runtimeEvidence, StringComparison.Ordinal);
        Assert.Contains("output_validation selected", decision.Value.DecisionText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SkillSelectionMachine_OutputValidationAblationLowersDecisionWithoutUnitSection()
    {
        var request = new SkillSelectionRequest(
            Prompt: "Check the response against the requested constraints and evidence before accepting it.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "output-validation"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS10-ABLATION-001");
        var trained = new SkillSelectionMachine();
        await trained.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss10-output-validation-pair-002",
                    Request: request with { Prompt = "Validate whether the answer satisfies task constraints and required evidence before acceptance." },
                    Chosen: new SkillSelectionDecision(["output-validation"], ["image-generation"], ["answer_constraint_evidence_validation"], 1.0, false, "Output validation skill."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["output-validation"], ["visual_asset_near_miss"], 0.1, true, "Visual asset near miss."),
                    RejectedType: "adjacent_wrong_validation_skill",
                    SourcePromptEntryId: "UPS-SS10-ABLATION-TRAIN",
                    TargetTrainingAdapter: SkillSelectionMachine.OutputValidationTrainingAdapter)
            ]);
        var empty = new SkillSelectionMachine();

        var trainedDecision = trained.DecideFromTrainedState(request);
        var emptyDecision = empty.DecideFromTrainedState(request);

        Assert.True(trainedDecision.IsSuccess, trainedDecision.ErrorMessage);
        Assert.True(emptyDecision.IsSuccess, emptyDecision.ErrorMessage);
        Assert.Equal(["output-validation"], trainedDecision.Value!.SelectedSkillIds);
        Assert.Empty(emptyDecision.Value!.SelectedSkillIds);
        Assert.True(trainedDecision.Value.Confidence > emptyDecision.Value.Confidence);
        Assert.Contains(SkillSelectionMachine.OutputValidationCheckpointEvidence, trainedDecision.Value.EvidenceFeatures);
        Assert.Contains("NO_TRAINED_STATE_AVAILABLE", emptyDecision.Value.EvidenceFeatures);
    }

    [Fact]
    public async Task SkillSelectionMachine_TaskCompletionUsesCheckpointSectionForHeldOutCompletionPrompt()
    {
        var trainingRequest = new SkillSelectionRequest(
            Prompt: "Decide whether the task is truly complete after checking gaps and remaining work.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["task-completion", "image-generation"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS11-TRAIN-001");
        var heldOutRequest = new SkillSelectionRequest(
            Prompt: "Determine if the work can be called complete after reviewing remaining gaps.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "task-completion"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS11-HELDOUT-001");
        var machine = new SkillSelectionMachine();
        var training = await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss11-task-completion-pair-001",
                    Request: trainingRequest,
                    Chosen: new SkillSelectionDecision(["task-completion"], ["image-generation"], ["completion_gap_review"], 1.0, false, "Task completion checks whether gaps remain before calling work done."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["task-completion"], ["visual_asset_near_miss"], 0.1, true, "Image generation is adjacent only for visual asset requests."),
                    RejectedType: "adjacent_wrong_completion_skill",
                    SourcePromptEntryId: "UPS-SS11-TRAIN-001",
                    TargetTrainingAdapter: SkillSelectionMachine.TaskCompletionTrainingAdapter)
            ]);

        var checkpoint = machine.ExportCheckpoint();
        var fresh = new SkillSelectionMachine();
        fresh.LoadCheckpoint(checkpoint);
        var decision = fresh.DecideFromTrainedState(heldOutRequest);

        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.Empty(checkpoint.PromptUnderstanding.SkillProfiles);
        Assert.Empty(checkpoint.TaskUnderstanding.SkillProfiles);
        Assert.Empty(checkpoint.TaskType.SkillProfiles);
        Assert.Empty(checkpoint.Routing.SkillProfiles);
        Assert.Empty(checkpoint.SkillCandidates.SkillProfiles);
        Assert.Empty(checkpoint.SkillSelection.SkillProfiles);
        Assert.Empty(checkpoint.PromptContext.SkillProfiles);
        Assert.Empty(checkpoint.ResultGap.SkillProfiles);
        Assert.Empty(checkpoint.OutputValidation.SkillProfiles);
        Assert.Equal(SkillSelectionMachine.TaskCompletionUnitId, checkpoint.TaskCompletion.UnitId);
        Assert.False(checkpoint.TaskCompletion.SemanticQualityClaimed);
        var checkpointProfiles = checkpoint.TaskCompletion.SkillProfiles
            .ToDictionary(profile => profile.SkillId, StringComparer.Ordinal);
        Assert.Equal(2, checkpointProfiles.Count);
        Assert.Contains("ss11-task-completion-pair-001", checkpointProfiles["task-completion"].SourcePairIds);
        Assert.All(checkpointProfiles["task-completion"].LearnedSignals.Values, value => Assert.True(value > 0));
        Assert.Contains("ss11-task-completion-pair-001", checkpointProfiles["image-generation"].SourcePairIds);
        Assert.All(checkpointProfiles["image-generation"].LearnedSignals.Values, value => Assert.True(value < 0));

        var taskCompletionState = fresh.ExportTaskCompletionSkillUnitState();
        Assert.Equal(TaskCompletionSkillUnit.UnitId, taskCompletionState.UnitId);
        Assert.False(taskCompletionState.SemanticQualityClaimed);
        Assert.Contains("complete", taskCompletionState.CompletionFeatureVocabulary);
        Assert.Equal(2, taskCompletionState.SkillProfiles.Count);
        Assert.Contains(taskCompletionState.SkillProfiles, profile =>
            profile.SkillId == "task-completion"
            && profile.CompletionFeatureWeights.Values.All(value => value > 0));
        Assert.Contains(taskCompletionState.SkillProfiles, profile =>
            profile.SkillId == "image-generation"
            && profile.CompletionFeatureWeights.Values.All(value => value < 0));

        var taskCompletionUnit = new TaskCompletionSkillUnit();
        var heldOutFeatures = SkillSelectionMachine.ExtractPromptUnderstandingSignals(heldOutRequest.Prompt);
        var nearest = taskCompletionUnit.FindNearestTaskCompletionExamples(heldOutFeatures, taskCompletionState, 3);
        var completionSupport = taskCompletionUnit.ComputeTaskCompletionSupport(
            heldOutFeatures,
            "task-completion",
            taskCompletionState,
            3);
        var wrongSupport = taskCompletionUnit.ComputeTaskCompletionSupport(
            heldOutFeatures,
            "image-generation",
            taskCompletionState,
            3);
        Assert.NotEmpty(nearest);
        Assert.All(nearest, example =>
        {
            Assert.DoesNotContain("ss11-task-completion-pair-001", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain("UPS-SS11", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain("C:/Projects", example.ExampleId, StringComparison.Ordinal);
        });
        Assert.True(completionSupport.SupportScore > 0.0);
        Assert.True(completionSupport.MatchedFeatureCount > 0);
        Assert.Equal(0.0, wrongSupport.SupportScore);
        Assert.Equal(0, wrongSupport.MatchedFeatureCount);

        Assert.True(decision.IsSuccess, decision.ErrorMessage);
        Assert.Equal(["task-completion"], decision.Value!.SelectedSkillIds);
        Assert.Contains(SkillSelectionMachine.TaskCompletionCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(TaskCompletionSkillUnit.UnitExecutedEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(TaskCompletionSkillUnit.BaselineEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("task_completion:semantic_quality_claimed=false", decision.Value.EvidenceFeatures);
        Assert.Contains("task_completion:knn_role=support_evidence_only", decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.OutputValidationCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, decision.Value.EvidenceFeatures);
        var runtimeEvidence = string.Join("\n", decision.Value.EvidenceFeatures);
        Assert.DoesNotContain("source_pair_count", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("ss11-task-completion-pair-001", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("UPS-SS11", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain(trainingRequest.Prompt, runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Projects", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_position", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_order", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_index", runtimeEvidence, StringComparison.Ordinal);
        Assert.Contains("task_completion selected", decision.Value.DecisionText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SkillSelectionMachine_TaskCompletionAblationLowersDecisionWithoutUnitSection()
    {
        var request = new SkillSelectionRequest(
            Prompt: "Determine if the work can be called complete after reviewing remaining gaps.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "task-completion"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS11-ABLATION-001");
        var trained = new SkillSelectionMachine();
        await trained.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss11-task-completion-pair-002",
                    Request: request with { Prompt = "Decide whether the task is truly complete after checking gaps and remaining work." },
                    Chosen: new SkillSelectionDecision(["task-completion"], ["image-generation"], ["completion_gap_review"], 1.0, false, "Task completion skill."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["task-completion"], ["visual_asset_near_miss"], 0.1, true, "Visual asset near miss."),
                    RejectedType: "adjacent_wrong_completion_skill",
                    SourcePromptEntryId: "UPS-SS11-ABLATION-TRAIN",
                    TargetTrainingAdapter: SkillSelectionMachine.TaskCompletionTrainingAdapter)
            ]);
        var empty = new SkillSelectionMachine();

        var trainedDecision = trained.DecideFromTrainedState(request);
        var emptyDecision = empty.DecideFromTrainedState(request);

        Assert.True(trainedDecision.IsSuccess, trainedDecision.ErrorMessage);
        Assert.True(emptyDecision.IsSuccess, emptyDecision.ErrorMessage);
        Assert.Equal(["task-completion"], trainedDecision.Value!.SelectedSkillIds);
        Assert.Empty(emptyDecision.Value!.SelectedSkillIds);
        Assert.True(trainedDecision.Value.Confidence > emptyDecision.Value.Confidence);
        Assert.Contains(SkillSelectionMachine.TaskCompletionCheckpointEvidence, trainedDecision.Value.EvidenceFeatures);
        Assert.Contains("NO_TRAINED_STATE_AVAILABLE", emptyDecision.Value.EvidenceFeatures);
    }

    [Fact]
    public async Task SkillSelectionMachine_IntermediateChecksUsesCheckpointSectionForHeldOutCheckPrompt()
    {
        var trainingRequest = new SkillSelectionRequest(
            Prompt: "Run an intermediate check before continuing so the next step stays aligned with evidence.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["intermediate-checks", "image-generation"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS12-TRAIN-001");
        var heldOutRequest = new SkillSelectionRequest(
            Prompt: "Pause for an evidence checkpoint before continuing to the next step.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "intermediate-checks"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS12-HELDOUT-001");
        var machine = new SkillSelectionMachine();
        var training = await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss12-intermediate-checks-pair-001",
                    Request: trainingRequest,
                    Chosen: new SkillSelectionDecision(["intermediate-checks"], ["image-generation"], ["mid_step_evidence_check"], 1.0, false, "Intermediate checks keep step execution aligned with evidence before continuing."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["intermediate-checks"], ["visual_asset_near_miss"], 0.1, true, "Image generation is adjacent only for visual asset requests."),
                    RejectedType: "adjacent_wrong_intermediate_check_skill",
                    SourcePromptEntryId: "UPS-SS12-TRAIN-001",
                    TargetTrainingAdapter: SkillSelectionMachine.IntermediateChecksTrainingAdapter)
            ]);

        var checkpoint = machine.ExportCheckpoint();
        var fresh = new SkillSelectionMachine();
        fresh.LoadCheckpoint(checkpoint);
        var decision = fresh.DecideFromTrainedState(heldOutRequest);

        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.Empty(checkpoint.PromptUnderstanding.SkillProfiles);
        Assert.Empty(checkpoint.TaskUnderstanding.SkillProfiles);
        Assert.Empty(checkpoint.TaskType.SkillProfiles);
        Assert.Empty(checkpoint.Routing.SkillProfiles);
        Assert.Empty(checkpoint.SkillCandidates.SkillProfiles);
        Assert.Empty(checkpoint.SkillSelection.SkillProfiles);
        Assert.Empty(checkpoint.PromptContext.SkillProfiles);
        Assert.Empty(checkpoint.ResultGap.SkillProfiles);
        Assert.Empty(checkpoint.OutputValidation.SkillProfiles);
        Assert.Empty(checkpoint.TaskCompletion.SkillProfiles);
        Assert.Equal(SkillSelectionMachine.IntermediateChecksUnitId, checkpoint.IntermediateChecks.UnitId);
        Assert.Equal(
            ["image-generation", "intermediate-checks"],
            checkpoint.IntermediateChecks.SkillProfiles.Select(profile => profile.SkillId).Order(StringComparer.Ordinal).ToArray());
        Assert.False(checkpoint.IntermediateChecks.SemanticQualityClaimed);
        var intermediateChecksUnitState = fresh.ExportIntermediateChecksSkillUnitState();
        var intermediateChecksUnit = new IntermediateChecksSkillUnit();
        var checkFeatures = SkillSelectionMachine.ExtractPromptUnderstandingSignals(heldOutRequest.Prompt);
        var nearestExamples = intermediateChecksUnit.FindNearestCheckExamples(checkFeatures, intermediateChecksUnitState, 3);
        var intermediateSupport = intermediateChecksUnit.ComputeCheckSupport(checkFeatures, "intermediate-checks", intermediateChecksUnitState, 3);
        var imageSupport = intermediateChecksUnit.ComputeCheckSupport(checkFeatures, "image-generation", intermediateChecksUnitState, 3);
        var intermediateChecksUnitEvidence = intermediateChecksUnit.Evaluate(new IntermediateChecksSkillInput(
            Request: heldOutRequest,
            State: intermediateChecksUnitState));
        Assert.Equal(IntermediateChecksSkillUnit.UnitId, intermediateChecksUnitState.UnitId);
        Assert.False(intermediateChecksUnitState.SemanticQualityClaimed);
        var typedProfile = Assert.Single(
            intermediateChecksUnitState.SkillProfiles,
            profile => string.Equals(profile.SkillId, "intermediate-checks", StringComparison.Ordinal));
        var rejectedProfile = Assert.Single(
            intermediateChecksUnitState.SkillProfiles,
            profile => string.Equals(profile.SkillId, "image-generation", StringComparison.Ordinal));
        Assert.NotEmpty(typedProfile.CheckFeatureWeights);
        Assert.All(typedProfile.CheckFeatureWeights.Values, value => Assert.True(value > 0));
        Assert.NotEmpty(rejectedProfile.CheckFeatureWeights);
        Assert.All(rejectedProfile.CheckFeatureWeights.Values, value => Assert.True(value < 0));
        Assert.Contains("ss12-intermediate-checks-pair-001", typedProfile.SourcePairIds);
        Assert.Contains("ss12-intermediate-checks-pair-001", rejectedProfile.SourcePairIds);
        Assert.Contains(intermediateChecksUnitState.CheckFeatureVocabulary, signal => string.Equals(signal, "intermediate", StringComparison.Ordinal));
        Assert.NotEmpty(intermediateChecksUnitState.KnnMemory);
        Assert.NotEmpty(nearestExamples);
        Assert.All(nearestExamples, example =>
        {
            Assert.Equal("intermediate-checks", example.ChosenSkillId);
            Assert.DoesNotContain("UPS-SS12", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain("ss12-intermediate-checks-pair", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain("C:/Projects", example.ExampleId, StringComparison.Ordinal);
            Assert.DoesNotContain(example.CheckFeatures, feature => feature.Contains("UPS-SS12", StringComparison.Ordinal));
            Assert.DoesNotContain(example.CheckFeatures, feature => feature.Contains("C:/Projects", StringComparison.Ordinal));
        });
        Assert.True(intermediateSupport.SupportScore > 0.0);
        Assert.True(intermediateSupport.MatchedFeatureCount > 0);
        Assert.Equal(0.0, imageSupport.SupportScore);
        Assert.True(imageSupport.MatchedFeatureCount == 0);
        Assert.True(intermediateChecksUnitEvidence.HasDecision);
        Assert.Equal(["intermediate-checks"], intermediateChecksUnitEvidence.SelectedSkillIds);
        Assert.False(intermediateChecksUnitEvidence.SemanticQualityClaimed);
        Assert.True(intermediateChecksUnitEvidence.BaselineCounterOnly);
        Assert.True(intermediateChecksUnitEvidence.LearnedCheckSignalScore > 0);
        Assert.Contains(IntermediateChecksSkillUnit.UnitExecutedEvidence, intermediateChecksUnitEvidence.EvidenceFeatures);
        Assert.Contains(IntermediateChecksSkillUnit.BaselineEvidence, intermediateChecksUnitEvidence.EvidenceFeatures);
        Assert.True(decision.IsSuccess, decision.ErrorMessage);
        Assert.Equal(["intermediate-checks"], decision.Value!.SelectedSkillIds);
        Assert.Contains(SkillSelectionMachine.IntermediateChecksCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(IntermediateChecksSkillUnit.UnitExecutedEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(IntermediateChecksSkillUnit.BaselineEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("intermediate_checks:semantic_quality_claimed=false", decision.Value.EvidenceFeatures);
        Assert.Contains(decision.Value.EvidenceFeatures, evidence => evidence.StartsWith("intermediate_checks:learned_check_signal_score=", StringComparison.Ordinal));
        Assert.Contains(decision.Value.EvidenceFeatures, evidence => evidence.StartsWith("intermediate_checks:vocabulary_size=", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("source_pair_count", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("UPS-SS12", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("ss12-intermediate-checks-pair", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("C:/Projects", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains(trainingRequest.Prompt, StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("candidate_position", StringComparison.Ordinal));
        Assert.DoesNotContain(decision.Value.EvidenceFeatures, evidence => evidence.Contains("candidate_order", StringComparison.Ordinal));
        Assert.DoesNotContain(SkillSelectionMachine.TaskCompletionCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("intermediate_checks selected", decision.Value.DecisionText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SkillSelectionMachine_IntermediateChecksAblationLowersDecisionWithoutUnitSection()
    {
        var request = new SkillSelectionRequest(
            Prompt: "Pause for an evidence checkpoint before continuing to the next step.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "intermediate-checks"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS12-ABLATION-001");
        var trained = new SkillSelectionMachine();
        await trained.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss12-intermediate-checks-pair-002",
                    Request: request with { Prompt = "Run an intermediate check before continuing so the next step stays aligned with evidence." },
                    Chosen: new SkillSelectionDecision(["intermediate-checks"], ["image-generation"], ["mid_step_evidence_check"], 1.0, false, "Intermediate checks skill."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["intermediate-checks"], ["visual_asset_near_miss"], 0.1, true, "Visual asset near miss."),
                    RejectedType: "adjacent_wrong_intermediate_check_skill",
                    SourcePromptEntryId: "UPS-SS12-ABLATION-TRAIN",
                    TargetTrainingAdapter: SkillSelectionMachine.IntermediateChecksTrainingAdapter)
            ]);
        var empty = new SkillSelectionMachine();

        var trainedDecision = trained.DecideFromTrainedState(request);
        var emptyDecision = empty.DecideFromTrainedState(request);

        Assert.True(trainedDecision.IsSuccess, trainedDecision.ErrorMessage);
        Assert.True(emptyDecision.IsSuccess, emptyDecision.ErrorMessage);
        Assert.Equal(["intermediate-checks"], trainedDecision.Value!.SelectedSkillIds);
        Assert.Empty(emptyDecision.Value!.SelectedSkillIds);
        Assert.True(trainedDecision.Value.Confidence > emptyDecision.Value.Confidence);
        Assert.Contains(SkillSelectionMachine.IntermediateChecksCheckpointEvidence, trainedDecision.Value.EvidenceFeatures);
        Assert.Contains("NO_TRAINED_STATE_AVAILABLE", emptyDecision.Value.EvidenceFeatures);
    }

    [Fact]
    public async Task SkillSelectionMachine_PromptUnderstandingAblationLowersDecisionWithoutUnitSection()
    {
        var request = new SkillSelectionRequest(
            Prompt: "Please review the plan coverage before the next executor starts.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["planning-session-startup", "plan-review"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-SS02-ABLATION-001");
        var trained = new SkillSelectionMachine();
        await trained.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "ss02-prompt-understanding-pair-002",
                    Request: request with { Prompt = "Review planning coverage before accepting an execution plan." },
                    Chosen: new SkillSelectionDecision(["plan-review"], ["planning-session-startup"], ["human_review_intent"], 1.0, false, "Review intent."),
                    Rejected: new SkillSelectionDecision(["planning-session-startup"], ["plan-review"], ["near_miss_startup"], 0.1, true, "Startup near miss."),
                    RejectedType: "adjacent_wrong_skill",
                    SourcePromptEntryId: "UPS-SS02-ABLATION-TRAIN",
                    TargetTrainingAdapter: SkillSelectionMachine.PromptUnderstandingTrainingAdapter)
            ]);
        var empty = new SkillSelectionMachine();

        var trainedDecision = trained.DecideFromTrainedState(request);
        var emptyDecision = empty.DecideFromTrainedState(request);

        Assert.True(trainedDecision.IsSuccess, trainedDecision.ErrorMessage);
        Assert.True(emptyDecision.IsSuccess, emptyDecision.ErrorMessage);
        Assert.Equal(["plan-review"], trainedDecision.Value!.SelectedSkillIds);
        Assert.Empty(emptyDecision.Value!.SelectedSkillIds);
        Assert.True(trainedDecision.Value.Confidence > emptyDecision.Value.Confidence);
        Assert.Contains(SkillSelectionMachine.PromptUnderstandingCheckpointEvidence, trainedDecision.Value.EvidenceFeatures);
        Assert.Contains("NO_TRAINED_STATE_AVAILABLE", emptyDecision.Value.EvidenceFeatures);
    }

    [Fact]
    public async Task SkillSelectionOrchestrator_AcceptsRequestAndReturnsDecision()
    {
        var orchestrator = new SkillSelectionOrchestrator();
        var request = new SkillSelectionRequest(
            Prompt: "Load planning and plan-review skills for a bounded work order.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["planning-session-startup", "plan-review"],
            HostFacts: new Dictionary<string, string> { ["host"] = "codex" },
            UserPromptSourceEntryId: "UPS-S1-001");

        var result = await orchestrator.DecideAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.SelectedSkillIds);
        Assert.Equal(request.AvailableSkillIds, result.Value.RejectedSkillIds);
        Assert.True(result.Value.RequiresSecondPass);
        Assert.Equal(0.0, result.Value.Confidence);
        Assert.Contains("NO_TRAINED_SEMANTIC_SELECTION", result.Value.EvidenceFeatures);
    }

    [Fact]
    public void SkillSelectionRequestDecision_SerializeRoundTrip()
    {
        var request = new SkillSelectionRequest(
            Prompt: "Choose skills for OP-45 anti-masquerade review.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["planning-session-startup", "plan-review"],
            HostFacts: new Dictionary<string, string> { ["host"] = "codex", ["phase"] = "39" },
            UserPromptSourceEntryId: "UPS-S1-002");
        var decision = new SkillSelectionDecision(
            SelectedSkillIds: [],
            RejectedSkillIds: request.AvailableSkillIds,
            EvidenceFeatures: ["S1_CONTRACT_ONLY", "NO_TRAINED_SEMANTIC_SELECTION"],
            Confidence: 0.0,
            RequiresSecondPass: true,
            DecisionText: "Contract scaffold only.");

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var requestRoundTrip = JsonSerializer.Deserialize<SkillSelectionRequest>(
            JsonSerializer.Serialize(request, options),
            options);
        var decisionRoundTrip = JsonSerializer.Deserialize<SkillSelectionDecision>(
            JsonSerializer.Serialize(decision, options),
            options);

        Assert.NotNull(requestRoundTrip);
        Assert.Equal(request.Prompt, requestRoundTrip!.Prompt);
        Assert.Equal(request.ProjectRoot, requestRoundTrip.ProjectRoot);
        Assert.Equal(request.AvailableSkillIds, requestRoundTrip.AvailableSkillIds);
        Assert.Equal(request.HostFacts, requestRoundTrip.HostFacts);
        Assert.Equal(request.UserPromptSourceEntryId, requestRoundTrip.UserPromptSourceEntryId);

        Assert.NotNull(decisionRoundTrip);
        Assert.Equal(decision.SelectedSkillIds, decisionRoundTrip!.SelectedSkillIds);
        Assert.Equal(decision.RejectedSkillIds, decisionRoundTrip.RejectedSkillIds);
        Assert.Equal(decision.EvidenceFeatures, decisionRoundTrip.EvidenceFeatures);
        Assert.Equal(decision.Confidence, decisionRoundTrip.Confidence);
        Assert.Equal(decision.RequiresSecondPass, decisionRoundTrip.RequiresSecondPass);
        Assert.Equal(decision.DecisionText, decisionRoundTrip.DecisionText);
    }

    [Fact]
    public void SkillSelection_ProjectLocalState_PreservesUserPreferences()
    {
        var localState = new ProjectLocalSkillState(
            WikiNotes: new Dictionary<string, string>
            {
                ["planning-session-startup"] = "Use Luba's bounded sub-agent work order rule first."
            },
            Preferences: new Dictionary<string, string>
            {
                ["work_order_limit_minutes"] = "15",
                ["review_protocol"] = "OP-45"
            },
            Feedback: ["Do not turn missing tests into a pass."],
            CustomSkillWeights: new Dictionary<string, double>
            {
                ["planning-session-startup"] = 2.5,
                ["plan-review"] = 2.0
            },
            ServerLibraryEntries:
            [
                new ServerSkillLibraryEntry("planning-session-startup", "1.0.0", "Old server copy.", 1.0)
            ]);
        var refreshed = ProjectLocalSkillStateRefresh.ApplyServerLibraryRefresh(
            localState,
            [
                new ServerSkillLibraryEntry("planning-session-startup", "2.0.0", "New server copy.", 0.1),
                new ServerSkillLibraryEntry("plan-review", "2.0.0", "New review skill.", 0.1)
            ]);

        Assert.Equal(localState.WikiNotes, refreshed.WikiNotes);
        Assert.Equal(localState.Preferences, refreshed.Preferences);
        Assert.Equal(localState.Feedback, refreshed.Feedback);
        Assert.Equal(localState.CustomSkillWeights, refreshed.CustomSkillWeights);
        Assert.Equal(["plan-review", "planning-session-startup"], refreshed.ServerLibraryEntries.Select(entry => entry.SkillId));
        Assert.Equal("2.0.0", refreshed.ServerLibraryEntries.Single(entry => entry.SkillId == "planning-session-startup").Version);
    }

    [Fact]
    public void SkillSelection_ServerLibrary_DoesNotOverwriteProjectLocalAdaptation()
    {
        var localState = new ProjectLocalSkillState(
            WikiNotes: new Dictionary<string, string>
            {
                ["custom-phase-splitting"] = "Local project prefers micro-phases under 15 minutes."
            },
            Preferences: new Dictionary<string, string>
            {
                ["phase_split_strategy"] = "micro"
            },
            Feedback: ["Server upgrade must propose changes, not rewrite local adaptation."],
            CustomSkillWeights: new Dictionary<string, double>
            {
                ["custom-phase-splitting"] = 9.0
            },
            ServerLibraryEntries: []);
        var refreshed = ProjectLocalSkillStateRefresh.ApplyServerLibraryRefresh(
            localState,
            [
                new ServerSkillLibraryEntry("custom-phase-splitting", "9.9.9", "Conflicting server default.", 0.0),
                new ServerSkillLibraryEntry("new-server-skill", "1.0.0", "Fresh shared skill.", 1.0)
            ]);

        Assert.Equal("Local project prefers micro-phases under 15 minutes.", refreshed.WikiNotes["custom-phase-splitting"]);
        Assert.Equal("micro", refreshed.Preferences["phase_split_strategy"]);
        Assert.Contains("Server upgrade must propose changes, not rewrite local adaptation.", refreshed.Feedback);
        Assert.Equal(9.0, refreshed.CustomSkillWeights["custom-phase-splitting"]);
        Assert.Contains(refreshed.ServerLibraryEntries, entry => entry.SkillId == "custom-phase-splitting" && entry.Version == "9.9.9");
        Assert.Contains(refreshed.ServerLibraryEntries, entry => entry.SkillId == "new-server-skill");
    }

    [Fact]
    public void ServerSkillLibraryRegistry_LoadsUniversalSkills()
    {
        var registry = new ServerSkillLibraryRegistry();

        var skills = registry.LoadUniversalSkills();

        Assert.NotEmpty(skills);
        Assert.Contains(skills, skill => skill.SkillId == "planning-session-startup");
        Assert.Contains(skills, skill => skill.SkillId == "plan-review");
        Assert.All(skills, skill =>
        {
            Assert.False(string.IsNullOrWhiteSpace(skill.SkillId));
            Assert.False(string.IsNullOrWhiteSpace(skill.Version));
            Assert.False(string.IsNullOrWhiteSpace(skill.Summary));
            Assert.Equal("universal", skill.PortabilityLabel);
            Assert.True(skill.DefaultWeight > 0);
        });
    }

    [Fact]
    public void ProjectLocalSkillState_StoresWikiPreferencesFeedback()
    {
        var serverLibrary = new ServerSkillLibraryRegistry().LoadUniversalSkills();
        var localState = new ProjectLocalSkillState(
            WikiNotes: new Dictionary<string, string>
            {
                ["plan-review"] = "Local project requires explicit seven-check rows."
            },
            Preferences: new Dictionary<string, string>
            {
                ["work_order_minutes"] = "15",
                ["anti_masquerade_protocol"] = "OP-45"
            },
            Feedback: ["Prefer plan-review when the user asks to validate coverage."],
            CustomSkillWeights: new Dictionary<string, double>
            {
                ["plan-review"] = 3.5
            },
            ServerLibraryEntries: serverLibrary);

        Assert.Equal("Local project requires explicit seven-check rows.", localState.WikiNotes["plan-review"]);
        Assert.Equal("15", localState.Preferences["work_order_minutes"]);
        Assert.Equal("OP-45", localState.Preferences["anti_masquerade_protocol"]);
        Assert.Contains("Prefer plan-review when the user asks to validate coverage.", localState.Feedback);
        Assert.Equal(3.5, localState.CustomSkillWeights["plan-review"]);
        Assert.All(localState.ServerLibraryEntries, entry => Assert.Equal("universal", entry.PortabilityLabel));
    }

    [Fact]
    public void SkillSelectionFeedback_WritesProjectLocalDpoSignal()
    {
        var universalLibrary = new ServerSkillLibraryRegistry().LoadUniversalSkills();
        var localState = new ProjectLocalSkillState(
            WikiNotes: new Dictionary<string, string>
            {
                ["coverage-review"] = "Feedback should train local selection, not mutate shared library."
            },
            Preferences: new Dictionary<string, string>
            {
                ["preferred_review"] = "plan-review"
            },
            Feedback: [],
            CustomSkillWeights: new Dictionary<string, double>(),
            ServerLibraryEntries: universalLibrary);
        var writer = new SkillSelectionFeedbackWriter();
        var feedback = new SkillSelectionFeedbackEvent(
            FeedbackId: "fb-001",
            Prompt: "Validate that the plan covers all seven checks.",
            OriginallySelectedSkillIds: ["planning-session-startup"],
            CorrectedSkillIds: ["planning-session-startup", "plan-review"],
            FeedbackText: "The answer missed plan-review; add it next time.",
            SourcePromptLedgerId: "UPS-S-011");

        var result = writer.WriteProjectLocalSignal(localState, feedback);

        Assert.False(result.UniversalLibraryMutated);
        Assert.Equal(universalLibrary, localState.ServerLibraryEntries);
        Assert.Equal(universalLibrary, result.UpdatedProjectLocalState.ServerLibraryEntries);
        Assert.Contains(feedback.FeedbackText, result.UpdatedProjectLocalState.Feedback);
        Assert.Equal("project-local:.xiigen/smart-skills/dpo-signals.jsonl", result.Signal.StorageScope);
        Assert.Equal("IProjectLocalSkillSelectionFeedbackTraining", result.Signal.TargetTrainingAdapter);
        Assert.Equal("UPS-S-011", result.Signal.SourcePromptLedgerId);
        Assert.Equal(["planning-session-startup", "plan-review"], result.Signal.ChosenSkillIds);
        Assert.Empty(result.Signal.RejectedSkillIds);
        Assert.False(result.Signal.ContainsPrivateAbsolutePath);
        Assert.False(result.Signal.SemanticQualityClaimed);
        Assert.Equal("project_local_feedback_signal", result.Signal.RouteMode);
    }

    [Fact]
    public void SkillLibraryUpgrade_DoesNotOverwriteProjectLocalState()
    {
        var oldLibrary = new[]
        {
            new ServerSkillLibraryEntry(
                SkillId: "plan-review",
                Version: "1.0.0",
                Summary: "Reviews plans against OP-45 and seven-check coverage.",
                DefaultWeight: 1.0)
        };
        var newLibrary = new[]
        {
            new ServerSkillLibraryEntry(
                SkillId: "plan-review",
                Version: "2.0.0",
                Summary: "Updated shared plan-review skill.",
                DefaultWeight: 0.3),
            new ServerSkillLibraryEntry(
                SkillId: "upgrade-review",
                Version: "1.0.0",
                Summary: "Reviews shared library upgrade boundaries.",
                DefaultWeight: 0.9)
        };
        var localState = new ProjectLocalSkillState(
            WikiNotes: new Dictionary<string, string>
            {
                ["plan-review"] = "Local wiki says every review must include OP-45 and OP-46."
            },
            Preferences: new Dictionary<string, string>
            {
                ["preferred_review_skill"] = "plan-review",
                ["work_order_minutes"] = "15"
            },
            Feedback: ["Never let a server refresh rewrite project-local preferences."],
            CustomSkillWeights: new Dictionary<string, double>
            {
                ["plan-review"] = 4.5
            },
            ServerLibraryEntries: oldLibrary);
        var planner = new SkillLibraryUpgradePlanner();

        var result = planner.PlanUpgrade(new SkillLibraryUpgradeRequest(
            OldUniversalLibrary: oldLibrary,
            NewUniversalLibrary: newLibrary,
            ProjectLocalState: localState));

        Assert.True(result.ProjectLocalStatePreserved);
        Assert.False(result.UniversalLibraryMutatedProjectLocalState);
        Assert.False(result.SemanticQualityClaimed);
        Assert.Equal("schema_upgrade_boundary_not_semantic_authority", result.EvidenceMode);
        Assert.Equal("project-local:.xiigen/smart-skills/preference-patches.json", result.StorageScope);
        Assert.Equal(localState.WikiNotes, result.UpgradedProjectLocalState.WikiNotes);
        Assert.Equal(localState.Preferences, result.UpgradedProjectLocalState.Preferences);
        Assert.Equal(localState.Feedback, result.UpgradedProjectLocalState.Feedback);
        Assert.Equal(localState.CustomSkillWeights, result.UpgradedProjectLocalState.CustomSkillWeights);
        Assert.Equal(["plan-review", "upgrade-review"], result.AppliedUniversalLibrary.Select(entry => entry.SkillId));
        Assert.Equal("2.0.0", result.AppliedUniversalLibrary.Single(entry => entry.SkillId == "plan-review").Version);
    }

    [Fact]
    public void SkillLibraryUpgrade_ProducesPreferencePatchForAmbiguousMigration()
    {
        var oldLibrary = new[]
        {
            new ServerSkillLibraryEntry(
                SkillId: "plan-review",
                Version: "1.0.0",
                Summary: "Reviews plans against OP-45 and coverage gates.",
                DefaultWeight: 1.0)
        };
        var newLibrary = new[]
        {
            new ServerSkillLibraryEntry(
                SkillId: "review-governance",
                Version: "2.0.0",
                Summary: "Reviews governance and OP-45 coverage.",
                DefaultWeight: 1.0),
            new ServerSkillLibraryEntry(
                SkillId: "review-coverage",
                Version: "2.0.0",
                Summary: "Reviews coverage gates and completion matrix rows.",
                DefaultWeight: 1.0)
        };
        var localState = new ProjectLocalSkillState(
            WikiNotes: new Dictionary<string, string>
            {
                ["plan-review"] = "Local wiki binds plan-review to Luba's seven-check review."
            },
            Preferences: new Dictionary<string, string>
            {
                ["plan-review"] = "always include for plan validation"
            },
            Feedback: ["Plan-review was preferred before the shared library renamed it."],
            CustomSkillWeights: new Dictionary<string, double>
            {
                ["plan-review"] = 3.25
            },
            ServerLibraryEntries: oldLibrary);
        var planner = new SkillLibraryUpgradePlanner();

        var result = planner.PlanUpgrade(new SkillLibraryUpgradeRequest(
            OldUniversalLibrary: oldLibrary,
            NewUniversalLibrary: newLibrary,
            ProjectLocalState: localState));

        Assert.True(result.ProjectLocalStatePreserved);
        Assert.Equal("always include for plan validation", result.UpgradedProjectLocalState.Preferences["plan-review"]);
        Assert.DoesNotContain(result.UpgradedProjectLocalState.Preferences.Keys, key => key == "review-governance");
        Assert.DoesNotContain(result.UpgradedProjectLocalState.CustomSkillWeights.Keys, key => key == "review-coverage");
        Assert.NotEmpty(result.PreferencePatchProposals);
        Assert.Contains(result.PreferencePatchProposals, patch =>
            patch.ExistingProjectLocalKey == "plan-review"
            && patch.CandidateUniversalSkillId == "review-governance");
        Assert.Contains(result.PreferencePatchProposals, patch =>
            patch.ExistingProjectLocalKey == "plan-review"
            && patch.CandidateUniversalSkillId == "review-coverage");
        Assert.All(result.PreferencePatchProposals, patch =>
        {
            Assert.Equal("project-local:.xiigen/smart-skills/preference-patches.json", patch.StorageScope);
            Assert.True(patch.RequiresUserDecision);
            Assert.False(patch.SemanticQualityClaimed);
            Assert.StartsWith("project-local-patch-plan-review-to-", patch.PatchId, StringComparison.Ordinal);
        });
        Assert.False(result.SemanticQualityClaimed);
    }

    [Fact]
    public void ResultGapMachine_DetectsMissingSkillAfterOutput()
    {
        var machine = new ResultGapMachine();
        var request = new ResultGapRequest(
            Prompt: "Review output and recover if plan-review skill was missed.",
            SelectedSkillIds: ["planning-session-startup"],
            AiOutput: "I loaded only the planning startup skill and then reported done.",
            ExpectedMissingSkillId: "plan-review",
            ObservedMissingSkillId: "plan-review",
            UserPromptSourceEntryId: "UPS-S5-001");

        var result = machine.Evaluate(request);

        Assert.True(result.SkillGapDetected);
        Assert.True(result.RequiresSecondPass);
        Assert.Equal("plan-review", result.MissingSkillId);
        Assert.Equal("bootstrap_test_harness_not_semantic_authority", result.EvidenceMode);
        Assert.Contains("SECOND_PASS_CONTEXT_UPDATE", result.EvidenceFeatures);
        Assert.Contains("RESULT_GAP_DPO_ROW", result.EvidenceFeatures);
    }

    [Fact]
    public void SkillSelection_SecondPassAddsMissingSkillAndWritesDpoRow()
    {
        var machine = new ResultGapMachine();
        var result = machine.Evaluate(new ResultGapRequest(
            Prompt: "Use planning and plan-review skills, then catch missing skills after output.",
            SelectedSkillIds: ["planning-session-startup"],
            AiOutput: "The answer contains planning only and forgot review.",
            ExpectedMissingSkillId: "plan-review",
            ObservedMissingSkillId: "plan-review",
            UserPromptSourceEntryId: "UPS-S5-002"));

        Assert.NotNull(result.ContextUpdate);
        Assert.NotNull(result.DpoRow);
        Assert.Equal("plan-review", result.ContextUpdate!.AddedSkillId);
        Assert.Contains("planning-session-startup", result.ContextUpdate.RequiredSkillIds);
        Assert.Contains("plan-review", result.ContextUpdate.RequiredSkillIds);
        Assert.Equal("IResultGapTraining", result.DpoRow!.TargetTrainingAdapter);
        Assert.Equal("UPS-S5-002", result.DpoRow.SourcePromptLedgerId);
        Assert.Equal(["planning-session-startup", "plan-review"], result.DpoRow.ChosenSkillIds);
        Assert.Equal(["planning-session-startup"], result.DpoRow.RejectedSkillIds);
    }

    [Fact]
    public void ResultGapAdaptiveSignalWriter_WritesUserLocalSignalWithoutCommonCheckpointMutation()
    {
        var commonCheckpoint = Path.Combine(
            FindRepositoryRoot(),
            "models",
            "skill-selection",
            "skill-selection-v1",
            "skill-selection-v1-checkpoint.json");
        Assert.True(File.Exists(commonCheckpoint), $"Common checkpoint not found: {commonCheckpoint}");
        var beforeHash = Sha256(commonCheckpoint);
        var userLocalRoot = Path.Combine(
            Path.GetTempPath(),
            $"ad-01-result-gap-signal-{Guid.NewGuid():N}");
        var rawPrompt = "Use planning and plan-review skills, then catch missing skills after output.";
        var rawOutput = "The answer contains planning only and forgot review.";
        var resultGap = new ResultGapMachine().Evaluate(new ResultGapRequest(
            Prompt: rawPrompt,
            SelectedSkillIds: ["planning-session-startup"],
            AiOutput: rawOutput,
            ExpectedMissingSkillId: "plan-review",
            ObservedMissingSkillId: "plan-review",
            UserPromptSourceEntryId: "UPS-AD01-001"));
        var writer = new ResultGapAdaptiveSignalWriter();

        try
        {
            var write = writer.WriteUserLocalSignal(new ResultGapAdaptiveSignalWriteRequest(
                UserLocalRoot: userLocalRoot,
                ResultGap: resultGap,
                PromptRef: "prompt-ref:UPS-AD01-001:redacted",
                OutputRef: "output-ref:UPS-AD01-001:redacted",
                SourcePromptLedgerId: "UPS-AD01-001"));

            Assert.True(write.IsSuccess, write.ErrorMessage);
            Assert.True(write.Value!.Written);
            Assert.False(write.Value.CommonCheckpointMutated);
            Assert.Contains(Path.Combine(".xiigen", "smart-skills", "result-gap"), write.Value.SignalPath, StringComparison.Ordinal);
            Assert.True(File.Exists(write.Value.SignalPath));
            Assert.Equal(beforeHash, Sha256(commonCheckpoint));
            Assert.Equal("project-local:.xiigen/smart-skills/result-gap/adaptive-dpo-signals.jsonl", write.Value.Signal.StorageScope);
            Assert.Equal("missing_pattern_result_gap", write.Value.Signal.ErrorClass);
            Assert.Equal("IResultGapTraining", write.Value.Signal.TargetTrainingAdapter);
            Assert.Equal("result-gap:chosen:UPS-AD01-001:plan-review", write.Value.Signal.ChosenRef);
            Assert.Equal("result-gap:rejected:UPS-AD01-001:plan-review", write.Value.Signal.RejectedRef);
            Assert.Equal(["planning-session-startup", "plan-review"], write.Value.Signal.ChosenSkillIds);
            Assert.Equal(["planning-session-startup"], write.Value.Signal.RejectedSkillIds);
            Assert.False(write.Value.Signal.ContainsRawPrivateText);
            Assert.False(write.Value.Signal.SemanticQualityClaimed);
            Assert.False(write.Value.Signal.CommonCheckpointMutationClaimed);

            var line = File.ReadAllLines(write.Value.SignalPath).Single();
            var persisted = JsonSerializer.Deserialize<ResultGapAdaptiveDpoSignal>(
                line,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
            Assert.NotNull(persisted);
            Assert.Equal(write.Value.Signal.ChosenRef, persisted!.ChosenRef);
            Assert.Equal(write.Value.Signal.RejectedRef, persisted.RejectedRef);
            Assert.DoesNotContain(rawPrompt, line, StringComparison.Ordinal);
            Assert.DoesNotContain(rawOutput, line, StringComparison.Ordinal);
            Assert.DoesNotContain("models/skill-selection", line, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(userLocalRoot))
                Directory.Delete(userLocalRoot, recursive: true);
        }
    }

    [Fact]
    public void ResultGapAdaptiveTrainer_TrainsUserLocalCheckpointWithoutCommonCheckpointMutation()
    {
        var commonCheckpoint = Path.Combine(
            FindRepositoryRoot(),
            "models",
            "skill-selection",
            "skill-selection-v1",
            "skill-selection-v1-checkpoint.json");
        Assert.True(File.Exists(commonCheckpoint), $"Common checkpoint not found: {commonCheckpoint}");
        var beforeHash = Sha256(commonCheckpoint);
        var userLocalRoot = Path.Combine(
            Path.GetTempPath(),
            $"ad-02-result-gap-adaptive-{Guid.NewGuid():N}");
        var resultGap = new ResultGapMachine().Evaluate(new ResultGapRequest(
            Prompt: "Use planning and plan-review skills, then catch missing skills after output.",
            SelectedSkillIds: ["planning-session-startup"],
            AiOutput: "The answer contains planning only and forgot review.",
            ExpectedMissingSkillId: "plan-review",
            ObservedMissingSkillId: "plan-review",
            UserPromptSourceEntryId: "UPS-AD02-001"));
        var writer = new ResultGapAdaptiveSignalWriter();
        var trainer = new ResultGapAdaptiveTrainer();

        try
        {
            var write = writer.WriteUserLocalSignal(new ResultGapAdaptiveSignalWriteRequest(
                UserLocalRoot: userLocalRoot,
                ResultGap: resultGap,
                PromptRef: "prompt-ref:UPS-AD02-001:redacted",
                OutputRef: "output-ref:UPS-AD02-001:redacted",
                SourcePromptLedgerId: "UPS-AD02-001"));
            Assert.True(write.IsSuccess, write.ErrorMessage);

            var train = trainer.TrainUserLocalAsync(new ResultGapAdaptiveTrainingRequest(
                UserLocalRoot: userLocalRoot,
                SignalPath: write.Value!.SignalPath));

            Assert.True(train.IsSuccess, train.ErrorMessage);
            Assert.Equal(beforeHash, Sha256(commonCheckpoint));
            Assert.False(train.Value!.Checkpoint.SemanticQualityClaimed);
            Assert.False(train.Value.Checkpoint.CommonCheckpointMutationClaimed);
            Assert.Single(train.Value.Checkpoint.MissingPatternProfiles);
            Assert.Equal("plan-review", train.Value.Checkpoint.MissingPatternProfiles[0].MissingSkillId);
            Assert.Contains("result-gap-adaptive-UPS-AD02-001-plan-review", train.Value.Checkpoint.TrainedSignalIds);
            Assert.True(train.Value.Report.Passed);
            Assert.True(train.Value.Report.MetricImproved);
            Assert.Equal(0, train.Value.Report.MetricBefore);
            Assert.Equal(1, train.Value.Report.MetricAfter);
            Assert.False(train.Value.Report.CommonCheckpointMutated);
            Assert.True(train.Value.Report.FreshLoadPassed);
            Assert.Contains(Path.Combine(".xiigen", "smart-skills", "result-gap"), train.Value.Report.CheckpointPath, StringComparison.Ordinal);
            Assert.Contains(Path.Combine(".xiigen", "smart-skills", "result-gap"), train.Value.Report.ReportPath, StringComparison.Ordinal);
            Assert.True(File.Exists(train.Value.Report.CheckpointPath));
            Assert.True(File.Exists(train.Value.Report.ReportPath));

            var loaded = trainer.LoadCheckpoint(train.Value.Report.CheckpointPath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            Assert.Equal(train.Value.Checkpoint.TrainedSignalIds, loaded.Value!.TrainedSignalIds);
        }
        finally
        {
            if (Directory.Exists(userLocalRoot))
                Directory.Delete(userLocalRoot, recursive: true);
        }
    }

    [Fact]
    public void SkillSelectionCorrectionDpoSignalWriter_WritesRedactedUserLocalCorrectionSignalWithoutCommonCheckpointMutation()
    {
        var commonCheckpoint = Path.Combine(
            FindRepositoryRoot(),
            "models",
            "skill-selection",
            "skill-selection-v1",
            "skill-selection-v1-checkpoint.json");
        Assert.True(File.Exists(commonCheckpoint), $"Common checkpoint not found: {commonCheckpoint}");
        var beforeHash = Sha256(commonCheckpoint);
        var userLocalRoot = Path.Combine(
            Path.GetTempPath(),
            $"ad-03-correction-signal-{Guid.NewGuid():N}");
        var rawPrompt = "Please replace the selected planning skill with plan-review for this correction.";
        var rawOutput = "The AI response picked planning-session-startup and missed review.";
        var rawAiResponse = "I selected planning-session-startup.";
        var writer = new SkillSelectionCorrectionDpoSignalWriter();

        try
        {
            var write = writer.WriteUserLocalSignal(new SkillSelectionCorrectionDpoSignalWriteRequest(
                UserLocalRoot: userLocalRoot,
                CorrectionId: "AD03-CORRECTION-001",
                ChosenSkillIds: ["plan-review"],
                RejectedSkillIds: ["planning-session-startup"],
                CorrectionClass: "user_changed_selected_skill",
                PromptRef: "prompt-ref:UPS-AD03-001:redacted",
                OutputRef: "output-ref:UPS-AD03-001:redacted",
                AiResponseRef: "ai-response-ref:UPS-AD03-001:redacted",
                SourcePromptLedgerId: "UPS-AD03-001",
                ProvenanceRefs: new Dictionary<string, string>
                {
                    ["correction_event_ref"] = "correction-ref:UPS-AD03-001:redacted"
                }));

            Assert.True(write.IsSuccess, write.ErrorMessage);
            Assert.True(write.Value!.Written);
            Assert.False(write.Value.CommonCheckpointMutated);
            Assert.Contains(Path.Combine(".xiigen", "smart-skills", "corrections"), write.Value.SignalPath, StringComparison.Ordinal);
            Assert.True(File.Exists(write.Value.SignalPath));
            Assert.Equal(beforeHash, Sha256(commonCheckpoint));
            Assert.Equal("project-local:.xiigen/smart-skills/corrections/adaptive-dpo-signals.jsonl", write.Value.Signal.StorageScope);
            Assert.Equal("user_changed_selected_skill", write.Value.Signal.CorrectionClass);
            Assert.Equal("IProjectLocalSkillSelectionFeedbackTraining", write.Value.Signal.TargetTrainingAdapter);
            Assert.Equal("skill-correction:chosen:UPS-AD03-001:AD03-CORRECTION-001", write.Value.Signal.ChosenRef);
            Assert.Equal("skill-correction:rejected:UPS-AD03-001:AD03-CORRECTION-001", write.Value.Signal.RejectedRef);
            Assert.Equal(["plan-review"], write.Value.Signal.ChosenSkillIds);
            Assert.Equal(["planning-session-startup"], write.Value.Signal.RejectedSkillIds);
            Assert.Equal("prompt-ref:UPS-AD03-001:redacted", write.Value.Signal.PromptRef);
            Assert.Equal("output-ref:UPS-AD03-001:redacted", write.Value.Signal.OutputRef);
            Assert.Equal("ai-response-ref:UPS-AD03-001:redacted", write.Value.Signal.AiResponseRef);
            Assert.False(write.Value.Signal.ContainsRawPrivateText);
            Assert.False(write.Value.Signal.SemanticQualityClaimed);
            Assert.False(write.Value.Signal.CommonCheckpointMutationClaimed);

            var line = File.ReadAllLines(write.Value.SignalPath).Single();
            var persisted = JsonSerializer.Deserialize<SkillSelectionCorrectionAdaptiveDpoSignal>(
                line,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
            Assert.NotNull(persisted);
            Assert.Equal(write.Value.Signal.ChosenRef, persisted!.ChosenRef);
            Assert.Equal(write.Value.Signal.RejectedRef, persisted.RejectedRef);
            Assert.DoesNotContain(rawPrompt, line, StringComparison.Ordinal);
            Assert.DoesNotContain(rawOutput, line, StringComparison.Ordinal);
            Assert.DoesNotContain(rawAiResponse, line, StringComparison.Ordinal);
            Assert.DoesNotContain("models/skill-selection", line, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(userLocalRoot))
                Directory.Delete(userLocalRoot, recursive: true);
        }
    }

    [Fact]
    public void SkillSelectionCorrectionAdaptiveTrainer_ContinuesUserLocalCheckpointFromCorrectionSignals()
    {
        var commonCheckpoint = Path.Combine(
            FindRepositoryRoot(),
            "models",
            "skill-selection",
            "skill-selection-v1",
            "skill-selection-v1-checkpoint.json");
        Assert.True(File.Exists(commonCheckpoint), $"Common checkpoint not found: {commonCheckpoint}");
        var beforeHash = Sha256(commonCheckpoint);
        var userLocalRoot = Path.Combine(
            Path.GetTempPath(),
            $"ad-04-correction-adaptive-{Guid.NewGuid():N}");
        var writer = new SkillSelectionCorrectionDpoSignalWriter();
        var trainer = new SkillSelectionCorrectionAdaptiveTrainer();

        try
        {
            var firstWrite = writer.WriteUserLocalSignal(new SkillSelectionCorrectionDpoSignalWriteRequest(
                UserLocalRoot: userLocalRoot,
                CorrectionId: "AD04-CORRECTION-001",
                ChosenSkillIds: ["plan-review"],
                RejectedSkillIds: ["planning-session-startup"],
                CorrectionClass: "user_changed_selected_skill",
                PromptRef: "prompt-ref:UPS-AD04-001:redacted",
                OutputRef: "output-ref:UPS-AD04-001:redacted",
                AiResponseRef: "ai-response-ref:UPS-AD04-001:redacted",
                SourcePromptLedgerId: "UPS-AD04-001",
                ProvenanceRefs: new Dictionary<string, string>
                {
                    ["correction_event_ref"] = "correction-ref:UPS-AD04-001:redacted"
                }));
            Assert.True(firstWrite.IsSuccess, firstWrite.ErrorMessage);
            var firstTrain = trainer.TrainUserLocalAsync(new SkillSelectionCorrectionAdaptiveTrainingRequest(
                UserLocalRoot: userLocalRoot,
                SignalPath: firstWrite.Value!.SignalPath));
            Assert.True(firstTrain.IsSuccess, firstTrain.ErrorMessage);
            Assert.Equal(0, firstTrain.Value!.Report.MetricBefore);
            Assert.Equal(1, firstTrain.Value.Report.MetricAfter);
            Assert.True(firstTrain.Value.Report.FreshLoadPassed);

            var secondWrite = writer.WriteUserLocalSignal(new SkillSelectionCorrectionDpoSignalWriteRequest(
                UserLocalRoot: userLocalRoot,
                CorrectionId: "AD04-CORRECTION-002",
                ChosenSkillIds: ["claim-verification"],
                RejectedSkillIds: ["planning-session-startup"],
                CorrectionClass: "user_changed_selected_skill",
                PromptRef: "prompt-ref:UPS-AD04-002:redacted",
                OutputRef: "output-ref:UPS-AD04-002:redacted",
                AiResponseRef: "ai-response-ref:UPS-AD04-002:redacted",
                SourcePromptLedgerId: "UPS-AD04-002",
                ProvenanceRefs: new Dictionary<string, string>
                {
                    ["correction_event_ref"] = "correction-ref:UPS-AD04-002:redacted"
                }));
            Assert.True(secondWrite.IsSuccess, secondWrite.ErrorMessage);

            var continued = trainer.TrainUserLocalAsync(new SkillSelectionCorrectionAdaptiveTrainingRequest(
                UserLocalRoot: userLocalRoot,
                SignalPath: secondWrite.Value!.SignalPath));

            Assert.True(continued.IsSuccess, continued.ErrorMessage);
            Assert.Equal(beforeHash, Sha256(commonCheckpoint));
            Assert.False(continued.Value!.Checkpoint.SemanticQualityClaimed);
            Assert.False(continued.Value.Checkpoint.CommonCheckpointMutationClaimed);
            Assert.Single(continued.Value.Checkpoint.CorrectionProfiles);
            Assert.Equal("user_changed_selected_skill", continued.Value.Checkpoint.CorrectionProfiles[0].CorrectionClass);
            Assert.Equal(2, continued.Value.Checkpoint.CorrectionProfiles[0].SignalCount);
            Assert.Contains("skill-correction-adaptive-UPS-AD04-001-AD04-CORRECTION-001", continued.Value.Checkpoint.TrainedSignalIds);
            Assert.Contains("skill-correction-adaptive-UPS-AD04-002-AD04-CORRECTION-002", continued.Value.Checkpoint.TrainedSignalIds);
            Assert.True(continued.Value.Report.Passed);
            Assert.True(continued.Value.Report.MetricImproved);
            Assert.Equal(1, continued.Value.Report.MetricBefore);
            Assert.Equal(2, continued.Value.Report.MetricAfter);
            Assert.False(continued.Value.Report.CommonCheckpointMutated);
            Assert.True(continued.Value.Report.FreshLoadPassed);
            Assert.True(continued.Value.Report.ContinueProbePassed);
            Assert.Equal(continued.Value.Report.CheckpointSha256, continued.Value.Report.FreshLoadCheckpointSha256);
            Assert.Equal(continued.Value.Report.CheckpointSha256, continued.Value.Report.ContinueCheckpointSha256);
            Assert.Contains(Path.Combine(".xiigen", "smart-skills", "corrections"), continued.Value.Report.CheckpointPath, StringComparison.Ordinal);
            Assert.Contains(Path.Combine(".xiigen", "smart-skills", "corrections"), continued.Value.Report.ReportPath, StringComparison.Ordinal);
            Assert.True(File.Exists(continued.Value.Report.CheckpointPath));
            Assert.True(File.Exists(continued.Value.Report.ReportPath));

            var loaded = trainer.LoadCheckpoint(continued.Value.Report.CheckpointPath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            Assert.Equal(continued.Value.Checkpoint.TrainedSignalIds, loaded.Value!.TrainedSignalIds);
        }
        finally
        {
            if (Directory.Exists(userLocalRoot))
                Directory.Delete(userLocalRoot, recursive: true);
        }
    }

    [Fact]
    public void TaskCompletionMachine_BlocksFalseDoneWhenSkillGapRemains()
    {
        var gapMachine = new ResultGapMachine();
        var completionMachine = new TaskCompletionMachine();
        var gap = gapMachine.Evaluate(new ResultGapRequest(
            Prompt: "Do not call the task done while plan-review is missing.",
            SelectedSkillIds: ["planning-session-startup"],
            AiOutput: "Done.",
            ExpectedMissingSkillId: "plan-review",
            ObservedMissingSkillId: "plan-review",
            UserPromptSourceEntryId: "UPS-S5-003"));

        var decision = completionMachine.Decide(new TaskCompletionRequest(
            Prompt: "Do not call the task done while plan-review is missing.",
            AiOutput: "Done.",
            ResultGap: gap));

        Assert.False(decision.IsComplete);
        Assert.True(decision.BlocksFalseDone);
        Assert.Equal("BLOCK_FALSE_DONE_SKILL_GAP_REMAINS", decision.Verdict);
        Assert.Contains("SECOND_PASS_REQUIRED", decision.EvidenceFeatures);
        Assert.Contains("missing_skill=plan-review", decision.EvidenceFeatures);
    }

    [Fact]
    public void SmartSkillsTrainableSemanticUnits_AllElevenDomainsHaveAdapters()
    {
        var expectedDomains = new[]
        {
            "prompt_understanding",
            "task_understanding",
            "task_type",
            "routing",
            "skill_candidates",
            "skill_selection",
            "prompt_context",
            "result_gap",
            "output_validation",
            "task_completion",
            "intermediate_checks"
        };
        var rows = SmartSkillsSemanticUnitCatalog.All;

        Assert.Equal(11, rows.Count);
        Assert.Equal(
            expectedDomains.OrderBy(domain => domain, StringComparer.Ordinal),
            rows.Select(row => row.DomainId).OrderBy(domain => domain, StringComparer.Ordinal));
        Assert.Equal(
            SmartSkillsSemanticUnitCatalog.ExpectedDomainIds.OrderBy(domain => domain, StringComparer.Ordinal),
            rows.Select(row => row.DomainId).OrderBy(domain => domain, StringComparer.Ordinal));
        Assert.Equal(11, rows.Select(row => row.DomainId).Distinct(StringComparer.Ordinal).Count());
        Assert.All(rows, row =>
        {
            Assert.False(string.IsNullOrWhiteSpace(row.DomainId));
            Assert.False(string.IsNullOrWhiteSpace(row.MachineName));
            Assert.False(string.IsNullOrWhiteSpace(row.TrainingAdapterName));
            Assert.False(string.IsNullOrWhiteSpace(row.DpoRowShapeName));
            Assert.False(string.IsNullOrWhiteSpace(row.CheckpointSectionName));
            Assert.False(string.IsNullOrWhiteSpace(row.HeldOutFixtureId));
            Assert.StartsWith("semantic_units.", row.CheckpointSectionName, StringComparison.Ordinal);
            Assert.StartsWith("heldout.", row.HeldOutFixtureId, StringComparison.Ordinal);
            Assert.True(row.Trainable);
            Assert.False(row.DeterministicFinalAuthority);
        });
    }

    [Fact]
    public void SmartSkillsTrainableSemanticUnits_NoDeterministicFinalAuthority()
    {
        var forbiddenFinalAuthorityMarkers = new[]
        {
            "CandidateSkillRetriever",
            "TaskIntentBoundaryClassifier",
            "ResultGapInspector",
            "OutputValidator",
            "TaskCompletionGuard",
            "BM25",
            "regex",
            "static router"
        };

        foreach (var row in SmartSkillsSemanticUnitCatalog.All)
        {
            var searchable = string.Join(
                "|",
                row.MachineName,
                row.TrainingAdapterName,
                row.DpoRowShapeName,
                row.CheckpointSectionName,
                row.HeldOutFixtureId);
            foreach (var marker in forbiddenFinalAuthorityMarkers)
                Assert.False(
                    searchable.Contains(marker, StringComparison.OrdinalIgnoreCase),
                    $"{row.DomainId} must not use {marker} as a final semantic authority.");

            Assert.True(row.Trainable);
            Assert.False(row.DeterministicFinalAuthority);
            Assert.False(row.RuntimeSemanticQualityClaimed);
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "models", "skill-selection", "skill-selection-v1", "skill-selection-v1-checkpoint.json")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root with skill-selection checkpoint.");
    }

    private static string Sha256(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
}
