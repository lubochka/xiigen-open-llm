// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using DPO;
using LoRA;
using TransformerModel;
using TransformerModel.Config;

public sealed class DesignConversionMachineTests
{
    [Fact]
    public async Task Machine_CreatesFlowAndOrchestratorWithoutClaimingLearning()
    {
        var machine = new DesignConversionMachine();

        Assert.NotNull(machine.Flow);
        var orchestratorResult = await machine.GetOrchestratorAsync();

        Assert.True(orchestratorResult.IsSuccess, orchestratorResult.ErrorMessage);
        Assert.NotNull(orchestratorResult.Value);
    }

    [Fact]
    public async Task Orchestrator_RunsDesignConversionRoutingFlowFromMetadataInput()
    {
        var machine = new DesignConversionMachine();
        var orchestratorResult = await machine.GetOrchestratorAsync();
        Assert.True(orchestratorResult.IsSuccess, orchestratorResult.ErrorMessage);

        var result = await orchestratorResult.Value!.RunAsync(new LogicalOrchestratorRequest(
            RunId: "machine-orchestrator-run",
            ScenarioId: "machine-orchestrator-scenario",
            UserRequest: "convert this design",
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["runtime_flow_input"] = DesignConversionTestFixtures.CreateInput()
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionRoutingFlow.FlowId, result.Value!.SelectedFlowId);
        Assert.Equal(bool.FalseString, result.Value.FlowResult.Metrics["deterministic_logic_claimed_as_learning"]);
        Assert.Contains("DESIGN_CONVERSION_MACHINE_RUNTIME_SCAFFOLD_NOT_LEARNED", result.Value.Warnings);
    }

    [Fact]
    public async Task TrainBatch_IsMachineLifecycleOnlyAndDoesNotPromoteUnits()
    {
        var machine = new DesignConversionMachine();
        var result = await DesignConversionLegalTeacherCapabilityTestFactory.TrainMachineStrictAsync(
            machine,
            new LearningBatch<DesignConversionDomain.DesignConversionDomain>(
            BatchId: "design-conversion-batch-001",
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: "item-001",
                    Prompt: "source design",
                    ChosenResponse: "{\"html\":\"<main><h1>Converted design</h1></main>\",\"css\":\"main{display:grid}\"}",
                    RejectedResponse: "{\"html\":\"<main></main>\",\"css\":\"main{display:block}\"}")
            ],
            SourceIds: ["slice-3-test"]));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, result.Value!.RowsTrained);
        Assert.Equal(result.Value.AccuracyBefore, result.Value.AccuracyAfter);
        Assert.Empty(result.Value.PromotedUnitIds);

        var checkpoint = machine.ExportCheckpoint();
        Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);
        Assert.True(checkpoint.Value!.HasTrainingScaffoldHistory);
        Assert.False(checkpoint.Value.DeterministicLogicClaimedAsLearning);
        Assert.Contains(DesignConversionMachine.HeldOutAccuracyNotMeasuredEvidence, checkpoint.Value.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.LocalPreferenceFitSignalOnlyEvidence, checkpoint.Value.EvidenceRefs);
        Assert.False(checkpoint.Value.CriticUnit.LearnedWeightsPresent);
        Assert.False(checkpoint.Value.ImproverUnit.LearnedWeightsPresent);
    }

    [Fact]
    public async Task TrainBatch_IgnoresPromptUsefulnessScoreWhenPersistingLearnedWeights()
    {
        var machine = new DesignConversionMachine();

        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, BuildPass150Batch("gc2-usefulness-score-not-learned"));

        Assert.True(train.IsSuccess, train.ErrorMessage);
        var checkpoint = machine.ExportCheckpoint();
        Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);
        var criticWeights = checkpoint.Value!.CriticUnit.LearnedWeights ?? [];
        Assert.DoesNotContain(criticWeights, weight => string.Equals("source_manifest:hash_diversity", weight.FeatureKey, StringComparison.Ordinal));
        Assert.DoesNotContain(
            criticWeights,
            weight => weight.FeatureKey.Contains("usefulness", StringComparison.OrdinalIgnoreCase) ||
                      weight.FeatureKey.Contains("review_priority", StringComparison.OrdinalIgnoreCase) ||
                      weight.FeatureKey.Contains("accepted_delta", StringComparison.OrdinalIgnoreCase) ||
                      weight.FeatureKey.Contains("score_margin", StringComparison.OrdinalIgnoreCase) ||
                      string.Equals("visual:chosen_score", weight.FeatureKey, StringComparison.Ordinal) ||
                      string.Equals("visual:rejected_score_inverse", weight.FeatureKey, StringComparison.Ordinal) ||
                      weight.Derivation.Contains("usefulness", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task EvaluateAsync_HeldOutFeatureFitPassesGateWhenTrainedRowsMatch()
    {
        var machine = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, BuildPass150Batch("heldout-fit-positive-training"));
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var evaluation = await machine.EvaluateAsync(new LearningEvaluationSet<DesignConversionDomain.DesignConversionDomain>(
            EvaluationId: "heldout-fit-positive-eval",
            DomainTag: DesignConversionMachine.DomainTag,
            EvaluationItems:
            [
                new LearningEvaluationItem(
                    ItemId: "heldout-fit-positive-row",
                    Prompt: BuildHeldOutEvaluationPrompt(includeInternalVector: true),
                    ExpectedResponse: "{\"visual_score\":0.95,\"passed\":true}")
            ]));

        Assert.True(evaluation.IsSuccess, evaluation.ErrorMessage);
        Assert.Equal(1, evaluation.Value!.RowsEvaluated);
        Assert.Equal(1.0, evaluation.Value.Accuracy);
        Assert.True(evaluation.Value.PassesGate, evaluation.Value.GateNote);
        Assert.Contains("accuracy=1.0000", evaluation.Value.GateNote, StringComparison.Ordinal);
        Assert.Contains("threshold=0.7500", evaluation.Value.GateNote, StringComparison.Ordinal);
        Assert.Contains("learned_weights_present=True", evaluation.Value.GateNote, StringComparison.Ordinal);
        Assert.Contains("not trained visual QA authority", evaluation.Value.GateNote, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "DesignConversionMachine lifecycle scaffold does not perform learned visual evaluation yet.",
            evaluation.Value.GateNote,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task EvaluateAsync_HeldOutFeatureFitFailsGateWhenHeldOutRowsMismatch()
    {
        var machine = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, BuildPass150Batch("heldout-fit-mismatch-training"));
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var evaluation = await machine.EvaluateAsync(new LearningEvaluationSet<DesignConversionDomain.DesignConversionDomain>(
            EvaluationId: "heldout-fit-mismatch-eval",
            DomainTag: DesignConversionMachine.DomainTag,
            EvaluationItems:
            [
                new LearningEvaluationItem(
                    ItemId: "heldout-fit-mismatch-row",
                    Prompt: BuildHeldOutEvaluationPrompt(includeInternalVector: false),
                    ExpectedResponse: "{\"visual_score\":0.95,\"passed\":true}")
            ]));

        Assert.True(evaluation.IsSuccess, evaluation.ErrorMessage);
        Assert.Equal(1, evaluation.Value!.RowsEvaluated);
        Assert.Equal(0.0, evaluation.Value.Accuracy);
        Assert.False(evaluation.Value.PassesGate);
        Assert.Contains("accuracy=0.0000", evaluation.Value.GateNote, StringComparison.Ordinal);
        Assert.Contains("threshold=0.7500", evaluation.Value.GateNote, StringComparison.Ordinal);
        Assert.Contains("learned_weights_present=True", evaluation.Value.GateNote, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "DesignConversionMachine lifecycle scaffold does not perform learned visual evaluation yet.",
            evaluation.Value.GateNote,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task EvaluateAsync_ConfiguredHeldOutThresholdRejectsAccuracyAcceptedByDefault()
    {
        var defaultMachine = new DesignConversionMachine();
        var defaultTrain = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(
                defaultMachine,
                BuildPass150Batch("heldout-threshold-default-training"));
        Assert.True(defaultTrain.IsSuccess, defaultTrain.ErrorMessage);

        var defaultEvaluation = await defaultMachine.EvaluateAsync(BuildThreeOfFourHeldOutEvaluationSet(
            "heldout-threshold-default-eval"));

        Assert.True(defaultEvaluation.IsSuccess, defaultEvaluation.ErrorMessage);
        Assert.Equal(4, defaultEvaluation.Value!.RowsEvaluated);
        Assert.Equal(0.75, defaultEvaluation.Value.Accuracy);
        Assert.True(defaultEvaluation.Value.PassesGate, defaultEvaluation.Value.GateNote);
        Assert.Contains("accuracy=0.7500", defaultEvaluation.Value.GateNote, StringComparison.Ordinal);
        Assert.Contains("threshold=0.7500", defaultEvaluation.Value.GateNote, StringComparison.Ordinal);

        var strictMachine = new DesignConversionMachine(
            new DesignConversionRoutingFlow(),
            DesignConversionMachineEvaluationOptions.FromMeasurementGateThresholds(
                new DesignConversionMeasurementGateThresholds(
                    RequiredHeldOutQualityScore: 0.90,
                    MaxRuntimeMs: 1_000,
                    MaxCostUsd: 0.01m,
                    MaxPeakRamMb: 512)));
        var strictTrain = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(
                strictMachine,
                BuildPass150Batch("heldout-threshold-strict-training"));
        Assert.True(strictTrain.IsSuccess, strictTrain.ErrorMessage);

        var strictEvaluation = await strictMachine.EvaluateAsync(BuildThreeOfFourHeldOutEvaluationSet(
            "heldout-threshold-strict-eval"));

        Assert.True(strictEvaluation.IsSuccess, strictEvaluation.ErrorMessage);
        Assert.Equal(4, strictEvaluation.Value!.RowsEvaluated);
        Assert.Equal(0.75, strictEvaluation.Value.Accuracy);
        Assert.False(strictEvaluation.Value.PassesGate);
        Assert.Contains("accuracy=0.7500", strictEvaluation.Value.GateNote, StringComparison.Ordinal);
        Assert.Contains("threshold=0.9000", strictEvaluation.Value.GateNote, StringComparison.Ordinal);
        Assert.Contains("< threshold=0.9000;", strictEvaluation.Value.GateNote, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoadCheckpointAsync_UsesOneByteSnapshotForHashAndImport()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"checkpoint-snapshot-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var firstMachine = new DesignConversionMachine();
            var firstTrain = await DesignConversionLegalTeacherCapabilityTestFactory
                .TrainMachineStrictAsync(firstMachine, BuildPass150Batch("snapshot-first"));
            Assert.True(firstTrain.IsSuccess, firstTrain.ErrorMessage);
            var firstCheckpoint = firstMachine.ExportCheckpoint();
            Assert.True(firstCheckpoint.IsSuccess, firstCheckpoint.ErrorMessage);
            var firstBytes = JsonSerializer.SerializeToUtf8Bytes(
                firstCheckpoint.Value!,
                DesignConversionCheckpointJsonOptions.Standard);

            var secondMachine = new DesignConversionMachine();
            Assert.True((await DesignConversionLegalTeacherCapabilityTestFactory
                .TrainMachineStrictAsync(secondMachine, BuildPass150Batch("snapshot-second-a"))).IsSuccess);
            Assert.True((await DesignConversionLegalTeacherCapabilityTestFactory
                .TrainMachineStrictAsync(secondMachine, BuildPass150Batch("snapshot-second-b"))).IsSuccess);
            var secondCheckpoint = secondMachine.ExportCheckpoint();
            Assert.True(secondCheckpoint.IsSuccess, secondCheckpoint.ErrorMessage);
            var secondBytes = JsonSerializer.SerializeToUtf8Bytes(
                secondCheckpoint.Value!,
                DesignConversionCheckpointJsonOptions.Standard);

            var path = Path.Combine(root, "checkpoint.json");
            await File.WriteAllBytesAsync(path, firstBytes);
            var readCount = 0;

            var loaded = await DesignConversionMachine.LoadCheckpointAsync(
                path,
                async (requestedPath, cancellationToken) =>
                {
                    readCount++;
                    Assert.Equal(Path.GetFullPath(path), Path.GetFullPath(requestedPath));
                    await File.WriteAllBytesAsync(requestedPath, secondBytes, cancellationToken);
                    return firstBytes;
                });

            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            Assert.Equal(1, readCount);
            Assert.Equal(Sha256Bytes(firstBytes), loaded.Value!.CheckpointSha256);
            Assert.NotEqual(Sha256Bytes(secondBytes), loaded.Value.CheckpointSha256);
            Assert.Equal(firstCheckpoint.Value!.Version, loaded.Value.Checkpoint.Version);
            Assert.Equal(
                firstCheckpoint.Value.TrainingHistory.Select(history => history.BatchId),
                loaded.Value.Checkpoint.TrainingHistory.Select(history => history.BatchId));
            Assert.NotEqual(secondCheckpoint.Value!.Version, loaded.Value.Checkpoint.Version);

            var reExported = loaded.Value.Machine.ExportCheckpoint();
            Assert.True(reExported.IsSuccess, reExported.ErrorMessage);
            Assert.Equal(firstCheckpoint.Value.Version, reExported.Value!.Version);
            Assert.Equal(
                firstCheckpoint.Value.TrainingHistory.Select(history => history.BatchId),
                reExported.Value.TrainingHistory.Select(history => history.BatchId));
            Assert.Equal(Sha256Bytes(secondBytes), await ReadFileSha256Async(path));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ImportAsync_RejectsInvalidExportAccuracyAndLeavesTargetUnbound()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"import-invalid-accuracy-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var source = new DesignConversionMachine();
            var checkpointPath = Path.Combine(root, "checkpoint.json");
            var save = await source.SaveCheckpointAsync(checkpointPath);
            Assert.True(save.IsSuccess, save.ErrorMessage);

            var forgedExport = save.Value! with { AccuracyAtExport = double.NaN };
            var target = new DesignConversionMachine();
            var import = await target.ImportAsync(forgedExport);

            Assert.False(import.IsSuccess);
            Assert.Equal("IMPORT_ACCURACY_INVALID", import.ErrorCode);
            Assert.Equal("Export accuracy is invalid.", import.ErrorMessage);
            await AssertMachineRemainsUnboundAsync(target);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ImportAsync_RejectsForgedInRangeExportAccuracyAndLeavesTargetUnbound()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"import-forged-accuracy-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var source = new DesignConversionMachine();
            var checkpointPath = Path.Combine(root, "checkpoint.json");
            var save = await source.SaveCheckpointAsync(checkpointPath);
            Assert.True(save.IsSuccess, save.ErrorMessage);
            Assert.Equal(0.0, save.Value!.AccuracyAtExport);

            var forgedExport = save.Value with { AccuracyAtExport = 1.0 };
            var target = new DesignConversionMachine();
            var import = await target.ImportAsync(forgedExport);

            Assert.False(import.IsSuccess);
            Assert.Equal(DesignConversionMachine.ImportCheckpointMetadataMismatchCode, import.ErrorCode);
            Assert.Equal("Export metadata does not match the loaded checkpoint snapshot.", import.ErrorMessage);
            await AssertMachineRemainsUnboundAsync(target);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TrainBatch_CanonicalContentMutatesCheckpointWithoutLabelDerivedWeights()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"pass150-real-training-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var runtimeInput = DesignConversionTestFixtures.CreateInput();
            var baselineMachine = new DesignConversionMachine();
            var baselineProbe = await RunMachineScoreAsync(baselineMachine, runtimeInput, "pass150-baseline-runtime");
            var baselinePath = Path.Combine(root, "baseline-checkpoint.json");
            var baselineSave = await baselineMachine.SaveCheckpointAsync(baselinePath);
            Assert.True(baselineSave.IsSuccess, baselineSave.ErrorMessage);
            var baselineSha = await ReadFileSha256Async(baselinePath);

            var trainedMachine = new DesignConversionMachine();
            var train = await DesignConversionLegalTeacherCapabilityTestFactory
                .TrainMachineStrictAsync(trainedMachine, BuildPass150Batch("pass150-positive-margin"));
            Assert.True(train.IsSuccess, train.ErrorMessage);
            Assert.True(train.Value!.RewardMargin > 0.0);
            Assert.Equal(train.Value.AccuracyBefore, train.Value.AccuracyAfter);

            var trainedPath = Path.Combine(root, "trained-checkpoint.json");
            var trainedSave = await trainedMachine.SaveCheckpointAsync(trainedPath);
            Assert.True(trainedSave.IsSuccess, trainedSave.ErrorMessage);
            var trainedSha = await ReadFileSha256Async(trainedPath);
            Assert.NotEqual(baselineSha, trainedSha);
            Assert.NotEqual(baselineSave.Value!.Version, trainedSave.Value!.Version);

            var loaded = await DesignConversionMachine.LoadCheckpointAsync(trainedPath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            Assert.Contains(DesignConversionMachine.HeldOutAccuracyNotMeasuredEvidence, loaded.Value!.Checkpoint.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.LocalPreferenceFitSignalOnlyEvidence, loaded.Value.Checkpoint.EvidenceRefs);
            Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(loaded.Value!.Checkpoint));
            Assert.True(DesignConversionMachine.CountRealLearnedWeightObservations(loaded.Value.Checkpoint) > 0);
            var criticWeights = loaded.Value.Checkpoint.CriticUnit.LearnedWeights ?? [];
            Assert.DoesNotContain(criticWeights, weight => string.Equals(
                "source_manifest:hash_diversity",
                weight.FeatureKey,
                StringComparison.Ordinal));
            Assert.DoesNotContain(criticWeights, weight =>
                weight.FeatureKey.Contains("usefulness", StringComparison.OrdinalIgnoreCase) ||
                weight.FeatureKey.Contains("review_priority", StringComparison.OrdinalIgnoreCase) ||
                weight.FeatureKey.Contains("accepted_delta", StringComparison.OrdinalIgnoreCase) ||
                weight.FeatureKey.Contains("score_margin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals("visual:chosen_score", weight.FeatureKey, StringComparison.Ordinal) ||
                string.Equals("visual:rejected_score_inverse", weight.FeatureKey, StringComparison.Ordinal));
            Assert.Contains(criticWeights, weight =>
                string.Equals(weight.FeatureKey, "source_kind:figma", StringComparison.Ordinal));

            var freshLoadedProbe = await RunMachineScoreAsync(
                loaded.Value.Machine,
                runtimeInput,
                "pass150-fresh-loaded-runtime");
            Assert.DoesNotContain(DesignConversionMachine.LearnedInferenceAppliedEvidence, baselineProbe.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.LearnedInferenceAppliedEvidence, freshLoadedProbe.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.LocalCheckpointFeatureAdjustmentEvidence, freshLoadedProbe.EvidenceRefs);
            Assert.DoesNotContain(
                freshLoadedProbe.EvidenceRefs,
                reference => reference.Contains("LEARNED_INFERENCE_AUTHORITY", StringComparison.Ordinal));
            Assert.DoesNotContain(
                freshLoadedProbe.EvidenceRefs,
                reference => reference.Contains("TRAINED_MODEL_AUTHORITY", StringComparison.Ordinal) ||
                             reference.Contains("trained-model-authority", StringComparison.OrdinalIgnoreCase) ||
                             reference.Contains("learned-inference-authority", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(
                freshLoadedProbe.EvidenceRefs,
                reference => reference.Contains("LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT", StringComparison.Ordinal) &&
                             (reference.Contains("NOT_TRAINED", StringComparison.Ordinal) ||
                              reference.Contains("NOT_LEARNED", StringComparison.Ordinal)));
            Assert.Contains("DESIGN_CONVERSION_RUNTIME_SCAFFOLD_NOT_LEARNED_OUTPUT", freshLoadedProbe.FlowWarnings);
            Assert.Contains(DesignConversionMachine.RuntimeScaffoldNotTrainedModelWarning, freshLoadedProbe.FlowWarnings);
            Assert.Contains("DESIGN_CONVERSION_MACHINE_RUNTIME_SCAFFOLD_NOT_LEARNED", freshLoadedProbe.OrchestratorWarnings);
            Assert.Contains(DesignConversionMachine.RuntimeScaffoldNotTrainedModelWarning, freshLoadedProbe.OrchestratorWarnings);
            Assert.DoesNotContain(
                freshLoadedProbe.OrchestratorWarnings,
                warning => warning.Contains("LEARNED_INFERENCE", StringComparison.Ordinal));
            Assert.Equal("critic_local_checkpoint_feature_adjusted_score", freshLoadedProbe.PrimaryMetricName);
            Assert.DoesNotContain(
                freshLoadedProbe.Metrics.Keys,
                key => key.Contains("LearnedInference", StringComparison.OrdinalIgnoreCase) ||
                       key.Contains("learned_inference", StringComparison.OrdinalIgnoreCase));
            Assert.Equal(bool.TrueString, freshLoadedProbe.Metrics["LocalCheckpointFeatureAdjustmentApplied"]);
            Assert.Equal(bool.FalseString, freshLoadedProbe.Metrics["trained_model_authority"]);
            Assert.Equal(bool.TrueString, freshLoadedProbe.Metrics["not_trained_model_authority"]);
            Assert.Contains("not trained model authority", freshLoadedProbe.ScalingSignalNote, StringComparison.Ordinal);
            Assert.Contains("ProductReady/model training remains false", freshLoadedProbe.Notes, StringComparison.Ordinal);
            Assert.True(freshLoadedProbe.Score > baselineProbe.Score);

            var easyNegative = await TrainLoadAndProbeAsync(
                root,
                "easy-negative",
                TrainingContentVariant.SparseRejected,
                runtimeInput);
            var hardNegative = await TrainLoadAndProbeAsync(
                root,
                "hard-negative",
                TrainingContentVariant.StructuredRejected,
                runtimeInput);
            Assert.Equal(easyNegative.SemanticFeatureWeight, hardNegative.SemanticFeatureWeight);
            Assert.Equal(easyNegative.Score, hardNegative.Score);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task FreshLoadedLearnedWeights_RaisesScoreButKeepsBaselineAcceptanceAuthority()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"score-ceiling-recovery-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineMachine = new DesignConversionMachine();
            var baselineProbe = await RunMachineScoreAsync(
                baselineMachine,
                runtimeInput,
                "score-ceiling-baseline-runtime");
            Assert.Equal(0.40, baselineProbe.Score, precision: 2);
            Assert.False(baselineProbe.Passed);
            var oldEchoCap = baselineProbe.Score;
            var oldEchoPlusPointTwoCap = baselineProbe.Score + 0.20;

            var trainedMachine = new DesignConversionMachine();
            var train = await DesignConversionLegalTeacherCapabilityTestFactory
                .TrainMachineStrictAsync(trainedMachine, BuildPass150Batch("score-ceiling-positive-margin"));
            Assert.True(train.IsSuccess, train.ErrorMessage);

            var trainedPath = Path.Combine(root, "trained-checkpoint.json");
            var trainedSave = await trainedMachine.SaveCheckpointAsync(trainedPath);
            Assert.True(trainedSave.IsSuccess, trainedSave.ErrorMessage);

            var loaded = await DesignConversionMachine.LoadCheckpointAsync(trainedPath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(loaded.Value!.Checkpoint));
            Assert.True(DesignConversionMachine.CountRealLearnedWeightObservations(loaded.Value.Checkpoint) > 0);

            var freshLoadedProbe = await RunMachineScoreAsync(
                loaded.Value.Machine,
                runtimeInput,
                "score-ceiling-fresh-loaded-runtime");

            Assert.Contains(DesignConversionMachine.LearnedInferenceAppliedEvidence, freshLoadedProbe.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.LocalCheckpointFeatureAdjustmentEvidence, freshLoadedProbe.EvidenceRefs);
            Assert.True(
                freshLoadedProbe.Score > oldEchoCap,
                $"fresh loaded score {freshLoadedProbe.Score:0.######} did not exceed old echo cap {oldEchoCap:0.######}");
            Assert.True(
                freshLoadedProbe.Score > oldEchoPlusPointTwoCap,
                $"fresh loaded score {freshLoadedProbe.Score:0.######} stayed within old echo+0.20 cap {oldEchoPlusPointTwoCap:0.######}");
            Assert.True(freshLoadedProbe.Score > 0.60, $"fresh loaded score stayed capped at {freshLoadedProbe.Score:0.######}");
            Assert.False(freshLoadedProbe.Passed);
            Assert.Equal("DESIGN_CONVERSION_INPUT_EVIDENCE_NEEDS_IMPROVEMENT", freshLoadedProbe.OverallVerdict);
            Assert.Equal("route_candidate_to_trained_improver_or_render_qa_when_available", freshLoadedProbe.NextAction);
            Assert.Equal(bool.FalseString, freshLoadedProbe.Metrics["critic_passed"]);
            Assert.Equal(bool.FalseString, freshLoadedProbe.Metrics["trained_model_authority"]);
            Assert.Equal(bool.TrueString, freshLoadedProbe.Metrics["not_trained_model_authority"]);
            Assert.Contains("ProductReady/model training remains false", freshLoadedProbe.Notes, StringComparison.Ordinal);
            Assert.Contains(DesignConversionMachine.RuntimeScaffoldNotTrainedModelWarning, freshLoadedProbe.FlowWarnings);
            Assert.Contains(
                freshLoadedProbe.EvidenceRefs,
                reference => reference.StartsWith("LocalCheckpointFeatureAdjustmentSummary", StringComparison.Ordinal) &&
                             reference.Contains("score_before=0.4", StringComparison.Ordinal) &&
                             reference.Contains("adjustment_scale=tanh_bounded_0.5", StringComparison.Ordinal) &&
                             reference.Contains("acceptance_authority=baseline_visual_verdict", StringComparison.Ordinal) &&
                             !reference.Contains("acceptance_threshold=", StringComparison.Ordinal));
            Assert.DoesNotContain(
                freshLoadedProbe.EvidenceRefs,
                reference => reference.Contains("acceptance_threshold=", StringComparison.Ordinal));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_DirectDpoPolicy_DifferentPayloadWeightsRemainConfiguredWhenRuntimePairUnavailable()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var config = BuildDirectPolicyFixtureConfig();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "direct-dpo-policy-baseline");

            var first = await SaveDirectPolicyRuntimeCheckpointAsync(
                Path.Combine(root, "first"),
                config,
                "direct-dpo-policy-first-runtime.json",
                weightSeed: 7,
                directRuntimeKind: true);
            var second = await SaveDirectPolicyRuntimeCheckpointAsync(
                Path.Combine(root, "second"),
                config,
                "direct-dpo-policy-second-runtime.json",
                weightSeed: 701,
                directRuntimeKind: true);

            var firstLoad = await DesignConversionMachine.LoadCheckpointAsync(first.RuntimeCheckpointPath);
            var secondLoad = await DesignConversionMachine.LoadCheckpointAsync(second.RuntimeCheckpointPath);

            Assert.True(firstLoad.IsSuccess, firstLoad.ErrorMessage);
            Assert.True(secondLoad.IsSuccess, secondLoad.ErrorMessage);
            Assert.Equal(DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind, firstLoad.Value!.Checkpoint.DpoPolicyCheckpointRef!.RuntimeBridgeKind);
            Assert.Equal(DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind, secondLoad.Value!.Checkpoint.DpoPolicyCheckpointRef!.RuntimeBridgeKind);
            AssertCurrentDirectPolicyRandomContext(first.PolicyCheckpointRef);
            AssertCurrentDirectPolicyRandomContext(
                firstLoad.Value.Checkpoint.DpoPolicyCheckpointRef);
            AssertCurrentDirectPolicyRandomContext(second.PolicyCheckpointRef);
            AssertCurrentDirectPolicyRandomContext(
                secondLoad.Value.Checkpoint.DpoPolicyCheckpointRef);

            var firstProbe = await RunMachineScoreAsync(
                firstLoad.Value.Machine,
                runtimeInput,
                "direct-dpo-policy-first-probe");
            var secondProbe = await RunMachineScoreAsync(
                secondLoad.Value.Machine,
                runtimeInput,
                "direct-dpo-policy-second-probe");

            AssertDirectPolicyConfiguredPairUnavailableProbe(firstProbe, baselineProbe, root);
            AssertDirectPolicyConfiguredPairUnavailableProbe(secondProbe, baselineProbe, root);
            Assert.Equal(firstProbe.Score, secondProbe.Score);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("stale_schema_3")]
    [InlineData("missing_random_context")]
    [InlineData("invalid_random_context")]
    [InlineData("wrong_random_root")]
    [InlineData("wrong_random_fingerprint")]
    [InlineData("missing_required_random_evidence")]
    [InlineData("tampered_required_random_evidence")]
    [InlineData("conflicting_root_evidence")]
    [InlineData("conflicting_fingerprint_evidence")]
    [InlineData("conflicting_derivation_evidence")]
    public async Task LoadCheckpointAsync_CurrentDirectDpoPolicyRandomAuthorityMutationsFailClosed(
        string mutation)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            $"direct-dpo-policy-random-authority-{mutation}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                BuildDirectPolicyFixtureConfig(),
                "direct-dpo-policy-random-authority-runtime.json",
                weightSeed: 41,
                directRuntimeKind: true);
            var runtimeCheckpoint = JsonSerializer.Deserialize<DesignConversionMachineCheckpoint>(
                await File.ReadAllBytesAsync(runtime.RuntimeCheckpointPath),
                DesignConversionCheckpointJsonOptions.Standard);
            Assert.NotNull(runtimeCheckpoint?.DpoPolicyCheckpointRef);
            var policyRef = runtimeCheckpoint!.DpoPolicyCheckpointRef!;
            var mutatedPolicyRef = mutation switch
            {
                "stale_schema_3" => policyRef with
                {
                    TrainingContext = policyRef.TrainingContext with { SchemaVersion = 3 }
                },
                "missing_random_context" => policyRef with
                {
                    TrainingContext = policyRef.TrainingContext with { RandomContext = null }
                },
                "invalid_random_context" => policyRef with
                {
                    TrainingContext = policyRef.TrainingContext with
                    {
                        RandomContext = policyRef.TrainingContext.RandomContext! with
                        {
                            PolicyDropoutStreamDomain = "invalid"
                        }
                    }
                },
                "wrong_random_root" => policyRef with
                {
                    TrainingContext = policyRef.TrainingContext with
                    {
                        RandomContext = policyRef.TrainingContext.RandomContext! with
                        {
                            RootInitializationSeed = 1UL
                        }
                    }
                },
                "wrong_random_fingerprint" => policyRef with
                {
                    TrainingContext = policyRef.TrainingContext with
                    {
                        RandomContext = policyRef.TrainingContext.RandomContext! with
                        {
                            TrainingConfigurationFingerprint = new string('f', 64)
                        }
                    }
                },
                "missing_required_random_evidence" => WithCommittedPolicyEvidence(
                    policyRef,
                    policyRef.EvidenceRefs.Where(evidence => !string.Equals(
                        evidence,
                        DesignConversionDpoPolicyRandomContextContract.TypedEvidence,
                        StringComparison.Ordinal))),
                "tampered_required_random_evidence" => WithCommittedPolicyEvidence(
                    policyRef,
                    policyRef.EvidenceRefs.Select(evidence => string.Equals(
                            evidence,
                            DesignConversionDpoPolicyRandomContextContract
                                .InitializationReproducibilityOnlyEvidence,
                            StringComparison.Ordinal)
                        ? $"{evidence}:tampered"
                        : evidence)),
                "conflicting_root_evidence" => WithCommittedPolicyEvidence(
                    policyRef,
                    policyRef.EvidenceRefs.Append(
                        $"{DesignConversionDpoPolicyRandomContextContract.RootInitializationSeedEvidencePrefix}" +
                        checked(policyRef.TrainingContext.RandomContext!
                            .RootInitializationSeed + 1UL)
                            .ToString(CultureInfo.InvariantCulture))),
                "conflicting_fingerprint_evidence" => WithCommittedPolicyEvidence(
                    policyRef,
                    policyRef.EvidenceRefs.Append(
                        $"{DesignConversionDpoPolicyRandomContextContract.TrainingConfigurationFingerprintEvidencePrefix}" +
                        new string('f', 64))),
                "conflicting_derivation_evidence" => WithCommittedPolicyEvidence(
                    policyRef,
                    policyRef.EvidenceRefs.Append(
                        $"{DesignConversionDpoPolicyRandomContextContract.RandomDerivationSchemeEvidencePrefix}" +
                        "conflicting-derivation/v1")),
                _ => throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null)
            };
            if (mutation.StartsWith("conflicting_", StringComparison.Ordinal))
            {
                Assert.True(
                    DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Validate(
                        mutatedPolicyRef.TrainingContext
                            .LegalTeacherAdmissionCommitment));
                Assert.True(
                    DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
                        .MatchesCheckpointEvidence(
                            mutatedPolicyRef.TrainingContext
                                .LegalTeacherAdmissionCommitment,
                            mutatedPolicyRef.EvidenceRefs));
                Assert.False(
                    DesignConversionDpoPolicyRandomContextContract
                        .HasRequiredEvidenceRefs(
                            mutatedPolicyRef.TrainingContext.RandomContext,
                            mutatedPolicyRef.EvidenceRefs));
            }
            await File.WriteAllBytesAsync(
                runtime.RuntimeCheckpointPath,
                JsonSerializer.SerializeToUtf8Bytes(
                    runtimeCheckpoint with { DpoPolicyCheckpointRef = mutatedPolicyRef },
                    DesignConversionCheckpointJsonOptions.Standard));

            var load = await DesignConversionMachine.LoadCheckpointAsync(
                runtime.RuntimeCheckpointPath);

            Assert.False(load.IsSuccess);
            Assert.Equal(DesignConversionMachine.CheckpointDpoPolicyRefInvalidCode, load.ErrorCode);
            Assert.Null(load.Value);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_ReadCompatibleSchema3DirectPolicyFailsExplicitRuntimeSchemaGate()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            $"direct-dpo-policy-schema3-runtime-gate-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                BuildDirectPolicyFixtureConfig(),
                "direct-dpo-policy-schema3-runtime-gate.json",
                weightSeed: 43,
                directRuntimeKind: true);
            var runtimeCheckpoint = JsonSerializer.Deserialize<DesignConversionMachineCheckpoint>(
                await File.ReadAllBytesAsync(runtime.RuntimeCheckpointPath),
                DesignConversionCheckpointJsonOptions.Standard);
            var currentPolicyRef = Assert.IsType<DesignConversionDpoPolicyCheckpointRef>(
                runtimeCheckpoint?.DpoPolicyCheckpointRef);
            var randomContext = Assert.IsType<DesignConversionDpoPolicyRandomContext>(
                currentPolicyRef.TrainingContext.RandomContext);
            var randomEvidenceRefs = DesignConversionDpoPolicyRandomContextContract
                .BuildRequiredEvidenceRefs(randomContext)
                .ToHashSet(StringComparer.Ordinal);
            var schema3PolicyRef = currentPolicyRef with
            {
                TrainingContext = currentPolicyRef.TrainingContext with
                {
                    SchemaVersion = 3,
                    RandomContext = null
                }
            };
            schema3PolicyRef = WithCommittedPolicyEvidence(
                schema3PolicyRef,
                schema3PolicyRef.EvidenceRefs.Where(
                    evidence => !randomEvidenceRefs.Contains(evidence)));
            await File.WriteAllBytesAsync(
                runtime.RuntimeCheckpointPath,
                JsonSerializer.SerializeToUtf8Bytes(
                    runtimeCheckpoint! with
                    {
                        DpoPolicyCheckpointRef = schema3PolicyRef
                    },
                    DesignConversionCheckpointJsonOptions.Standard));

            var load = await DesignConversionMachine.LoadCheckpointAsync(
                runtime.RuntimeCheckpointPath);

            Assert.False(load.IsSuccess);
            Assert.Equal(
                DesignConversionMachine.DpoPolicyRuntimeSchemaUnsupportedCode,
                load.ErrorCode);
            Assert.Equal(
                DesignConversionMachine.DpoPolicyRuntimeSchemaUnsupportedMessage,
                load.ErrorMessage);
            Assert.Null(load.Value);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ImportCheckpoint_Schema3DirectPolicyFailsExplicitlyWithoutMutatingExistingState()
    {
        var machine = new DesignConversionMachine();
        var trained = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(
                machine,
                BuildPass150Batch("schema3-direct-public-import-existing-state"));
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        var beforeCheckpoint = machine.ExportCheckpoint();
        Assert.True(beforeCheckpoint.IsSuccess, beforeCheckpoint.ErrorMessage);
        var existingCheckpoint = Assert.IsType<DesignConversionMachineCheckpoint>(
            beforeCheckpoint.Value);
        Assert.True(existingCheckpoint.HasTrainingScaffoldHistory);
        Assert.NotEmpty(existingCheckpoint.TrainingHistory);
        Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(
            existingCheckpoint));
        var runtimeInput = DesignConversionTestFixtures.CreateInput(
            comparisonScore: 0.41,
            comparisonPassed: false);
        var beforeRouting = await RunMachineScoreAsync(
            machine,
            runtimeInput,
            "schema3-direct-public-import-routing");
        var beforeGeneration = CaptureMachineStateGeneration(machine);

        var currentPolicyRef = BuildDirectPolicyCheckpointRef(
            "dpo_policy_step_000001.bin",
            new string('c', 64),
            policyByteLength: 1,
            config: BuildDirectPolicyFixtureConfig(),
            runtimeBridgeKind:
                DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind);
        var randomContext = Assert.IsType<DesignConversionDpoPolicyRandomContext>(
            currentPolicyRef.TrainingContext.RandomContext);
        var randomEvidenceRefs = DesignConversionDpoPolicyRandomContextContract
            .BuildRequiredEvidenceRefs(randomContext)
            .ToHashSet(StringComparer.Ordinal);
        var schema3PolicyRef = currentPolicyRef with
        {
            TrainingContext = currentPolicyRef.TrainingContext with
            {
                SchemaVersion = 3,
                RandomContext = null
            }
        };
        schema3PolicyRef = WithCommittedPolicyEvidence(
            schema3PolicyRef,
            schema3PolicyRef.EvidenceRefs.Where(
                evidence => !randomEvidenceRefs.Contains(evidence)));
        var incoming = existingCheckpoint with
        {
            DpoPolicyCheckpointRef = schema3PolicyRef
        };

        var import = machine.ImportCheckpoint(incoming);

        Assert.False(import.IsSuccess);
        Assert.Equal(
            DesignConversionMachine.DpoPolicyRuntimeSchemaUnsupportedCode,
            import.ErrorCode);
        Assert.Equal(
            DesignConversionMachine.DpoPolicyRuntimeSchemaUnsupportedMessage,
            import.ErrorMessage);
        Assert.False(import.Value);
        Assert.Equal(beforeGeneration, CaptureMachineStateGeneration(machine));
        var schema4WithoutScorer = machine.ImportCheckpoint(
            existingCheckpoint with
            {
                DpoPolicyCheckpointRef = currentPolicyRef
            });
        Assert.False(schema4WithoutScorer.IsSuccess);
        Assert.Equal(
            DesignConversionMachine.DpoPolicyCheckpointPayloadModelLoadFailedCode,
            schema4WithoutScorer.ErrorCode);
        Assert.Equal(
            "Direct DPO policy inference checkpoint requires a loaded policy scorer snapshot.",
            schema4WithoutScorer.ErrorMessage);
        Assert.False(schema4WithoutScorer.Value);
        Assert.Equal(beforeGeneration, CaptureMachineStateGeneration(machine));
        var afterCheckpoint = machine.ExportCheckpoint();
        Assert.True(afterCheckpoint.IsSuccess, afterCheckpoint.ErrorMessage);
        Assert.Equal(
            SerializeCheckpointForStateComparison(existingCheckpoint),
            SerializeCheckpointForStateComparison(afterCheckpoint.Value!));
        var afterRouting = await RunMachineScoreAsync(
            machine,
            runtimeInput,
            "schema3-direct-public-import-routing");
        Assert.Equal(
            JsonSerializer.SerializeToUtf8Bytes(
                beforeRouting,
                DesignConversionCheckpointJsonOptions.Standard),
            JsonSerializer.SerializeToUtf8Bytes(
                afterRouting,
                DesignConversionCheckpointJsonOptions.Standard));
        Assert.Equal(beforeGeneration, CaptureMachineStateGeneration(machine));
    }

    [Fact]
    public async Task LoadCheckpointAsync_DirectDpoPolicyRuntimeEncodingBindingRemainsBoundWhenRuntimePairUnavailable()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-runtime-binding-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var config = BuildDirectPolicyFixtureConfig();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "direct-dpo-policy-runtime-binding-baseline");
            var bindingEvidenceRefs =
                BuildValidContinuationRuntimeEncodingEvidence(
                    config,
                    evaluated: true);

            var legacy = await SaveDirectPolicyRuntimeCheckpointAsync(
                Path.Combine(root, "legacy"),
                config,
                "direct-dpo-policy-legacy-runtime.json",
                weightSeed: 53,
                directRuntimeKind: true,
                policyRefEvidenceRefs: [DesignConversionMachine.DpoPolicyContinuationRuntimeEncodingAppliedEvidence]);
            var bound = await SaveDirectPolicyRuntimeCheckpointAsync(
                Path.Combine(root, "bound"),
                config,
                "direct-dpo-policy-bound-runtime.json",
                weightSeed: 53,
                directRuntimeKind: true,
                policyRefEvidenceRefs: bindingEvidenceRefs,
                evaluatedHeldOutAcceptanceAuthorityFactory: policyRef =>
                    BuildTypedDirectPolicyHeldOutAcceptanceAuthority(
                        policyRef,
                        accuracy: 1.0));

            var legacyLoad = await DesignConversionMachine.LoadCheckpointAsync(legacy.RuntimeCheckpointPath);
            var boundLoad = await DesignConversionMachine.LoadCheckpointAsync(bound.RuntimeCheckpointPath);
            Assert.True(legacyLoad.IsSuccess, legacyLoad.ErrorMessage);
            Assert.True(boundLoad.IsSuccess, boundLoad.ErrorMessage);

            var legacyProbe = await RunMachineScoreAsync(
                legacyLoad.Value!.Machine,
                runtimeInput,
                "direct-dpo-policy-runtime-binding-legacy-probe");
            var boundProbe = await RunMachineScoreAsync(
                boundLoad.Value!.Machine,
                runtimeInput,
                "direct-dpo-policy-runtime-binding-bound-probe");

            AssertDirectPolicyConfiguredPairUnavailableProbe(legacyProbe, baselineProbe, root);
            AssertDirectPolicyConfiguredPairUnavailableProbe(boundProbe, baselineProbe, root);
            Assert.Equal(legacyProbe.Score, boundProbe.Score);
            Assert.DoesNotContain(
                DesignConversionMachine.DpoPolicyContinuationRuntimeEncodingAppliedEvidence,
                legacyProbe.EvidenceRefs);
            Assert.DoesNotContain(
                DesignConversionMachine.DpoPolicyContinuationRuntimeEncodingAppliedEvidence,
                boundProbe.EvidenceRefs);
            foreach (var bindingEvidenceRef in bindingEvidenceRefs)
                Assert.Contains(bindingEvidenceRef, boundProbe.EvidenceRefs);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_CoordinatorOwnedExactPairRanksGeneratedMinusIncumbentWithoutReceiptLabelAuthority()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core",
            $"direct-dpo-policy-exact-pair-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var config = BuildDirectPolicyFixtureConfig();
            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-exact-pair-runtime.json",
                weightSeed: 67,
                directRuntimeKind: true,
                evaluatedHeldOutAcceptanceAuthorityFactory: policyRef =>
                    BuildTypedDirectPolicyHeldOutAcceptanceAuthority(
                        policyRef,
                        accuracy: 1.0,
                        continuedTraining: true));
            var loaded = await DesignConversionMachine.LoadCheckpointAsync(
                runtime.RuntimeCheckpointPath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            var fixture = BuildExactRuntimePairFixture(
                preferredDirection:
                    DesignConversionIndependentPreferenceEvidenceContract
                        .IncumbentSide);

            var probe = await RunMachineScoreAsync(
                loaded.Value!.Machine,
                fixture.RuntimeInput,
                "direct-dpo-policy-exact-pair-probe");

            var expectedModel = new LlmModel(config);
            using (var stream = File.OpenRead(runtime.PolicyCheckpointPath))
                expectedModel.LoadCheckpoint(stream);
            var binding = BuildValidContinuationRuntimeInferenceBinding(
                config,
                evaluated: true);
            var batch = DesignConversionDpoPolicyCheckpointBridge
                .BuildRuntimePolicyPairBatchForInference(
                    fixture.Pair.BuildPolicyPrompt(),
                    DesignConversionDpoPolicyCheckpointBridge
                        .SerializePolicySideText(
                            fixture.Pair.Generated.Html,
                            fixture.Pair.Generated.Css),
                    DesignConversionDpoPolicyCheckpointBridge
                        .SerializePolicySideText(
                            fixture.Pair.Incumbent.Html,
                            fixture.Pair.Incumbent.Css),
                    binding,
                    config);
            Assert.True(batch.IsSuccess, batch.ErrorMessage);
            var generatedLogProbability =
                DpoTrainer.EvaluateAverageNextTokenLogProbability(
                    expectedModel,
                    batch.Value!.ChosenIds,
                    batch.Value.ChosenMask);
            var incumbentLogProbability =
                DpoTrainer.EvaluateAverageNextTokenLogProbability(
                    expectedModel,
                    batch.Value.RejectedIds,
                    batch.Value.RejectedMask);
            var expectedMargin = generatedLogProbability -
                incumbentLogProbability;
            var expectedScore = Math.Round(
                1.0 / (1.0 + Math.Exp(-Math.Clamp(
                    expectedMargin,
                    -60.0,
                    60.0))),
                6);

            Assert.Equal(expectedScore, probe.Score, 6);
            Assert.Equal(
                fixture.Pair.SourcePair.GeneratedEvaluation.SubmittedRequest.Html,
                fixture.Pair.Generated.Html);
            Assert.Equal(
                fixture.Pair.SourcePair.GeneratedEvaluation.SubmittedRequest.Css,
                fixture.Pair.Generated.Css);
            Assert.Equal(
                fixture.Pair.SourcePair.IncumbentEvaluation.SubmittedRequest.Html,
                fixture.Pair.Incumbent.Html);
            Assert.Equal(
                fixture.Pair.SourcePair.IncumbentEvaluation.SubmittedRequest.Css,
                fixture.Pair.Incumbent.Css);
            Assert.Equal(
                DesignConversionDpoGenerator.Sha256Ref(
                    DesignConversionDpoPolicyCheckpointBridge
                        .SerializePolicySideText(
                            fixture.Pair.Generated.Html,
                            fixture.Pair.Generated.Css)),
                fixture.Pair.Generated.ContentSha256);
            Assert.Equal(
                DesignConversionDpoGenerator.Sha256Ref(
                    DesignConversionDpoPolicyCheckpointBridge
                        .SerializePolicySideText(
                            fixture.Pair.Incumbent.Html,
                            fixture.Pair.Incumbent.Css)),
                fixture.Pair.Incumbent.ContentSha256);
            Assert.Equal(
                DesignConversionDpoGenerator
                    .BuildIndependentPreferenceEvaluationIdentity(
                        fixture.Pair.SourcePair.GeneratedEvaluation),
                fixture.Pair.Generated.EvaluationIdentity);
            Assert.Equal(
                DesignConversionDpoGenerator
                    .BuildIndependentPreferenceEvaluationIdentity(
                        fixture.Pair.SourcePair.IncumbentEvaluation),
                fixture.Pair.Incumbent.EvaluationIdentity);
            Assert.Equal(
                DesignConversionIndependentPreferenceEvidenceContract
                    .IncumbentSide,
                fixture.Pair.IndependentEvaluation.PreferredDirection);
            Assert.Contains(
                "dpo_policy_pair_score_direction:generated_minus_incumbent",
                probe.EvidenceRefs);
            Assert.Contains(
                $"dpo_policy_pair_binding_sha256:{fixture.Pair.PairBindingSha256}",
                probe.EvidenceRefs);
            Assert.Contains(
                $"dpo_policy_pair_generated_content_sha256:{fixture.Pair.Generated.ContentSha256}",
                probe.EvidenceRefs);
            Assert.Contains(
                $"dpo_policy_pair_incumbent_content_sha256:{fixture.Pair.Incumbent.ContentSha256}",
                probe.EvidenceRefs);
            Assert.Contains(
                $"dpo_policy_pair_independent_receipt_sha256:{fixture.Pair.IndependentEvaluation.ReceiptSha256}",
                probe.EvidenceRefs);
            Assert.Contains(
                DesignConversionMachine.DpoPolicyDirectInferenceScoreEvidence,
                probe.EvidenceRefs);
            Assert.DoesNotContain(
                DesignConversionMachine.DpoPolicyRuntimePairContentUnavailableEvidence,
                probe.EvidenceRefs);
            Assert.Equal(bool.TrueString,
                probe.Metrics["direct_dpo_policy_runtime_pair_content_available"]);
            Assert.Equal(bool.TrueString,
                probe.Metrics["direct_dpo_policy_inference_applied"]);
            Assert.Equal(bool.FalseString,
                probe.Metrics["direct_dpo_policy_acceptance_authority"]);
            Assert.Equal(bool.TrueString,
                probe.Metrics["baseline_visual_verdict_acceptance_authority"]);
            AssertPublicSurfaceDoesNotContain(
                probe,
                [fixture.Pair.Generated.Html, fixture.Pair.Generated.Css,
                    fixture.Pair.Incumbent.Html, fixture.Pair.Incumbent.Css,
                    root]);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_ExactPairTamperMatrixAndReplayFailClosedToPairUnavailableBaseline()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core",
            $"direct-dpo-policy-pair-tamper-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var config = BuildDirectPolicyFixtureConfig();
            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-pair-tamper-runtime.json",
                weightSeed: 71,
                directRuntimeKind: true,
                evaluatedHeldOutAcceptanceAuthorityFactory: policyRef =>
                    BuildTypedDirectPolicyHeldOutAcceptanceAuthority(
                        policyRef,
                        accuracy: 1.0,
                        continuedTraining: true));
            var loaded = await DesignConversionMachine.LoadCheckpointAsync(
                runtime.RuntimeCheckpointPath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            var fixture = BuildExactRuntimePairFixture();
            var baseline = await RunMachineScoreAsync(
                loaded.Value!.Machine,
                fixture.RuntimeInput with { DpoPolicyRuntimePair = null },
                "direct-dpo-policy-pair-tamper-baseline");

            DesignConversionDpoPolicyRuntimePair Rebind(
                DesignConversionDpoPolicyRuntimePair value)
                => value with
                {
                    PairBindingSha256 =
                        BuildRuntimePairBindingSha256ForTest(value)
                };

            var rootTamperJobId = fixture.Pair.RootJobId + ":tampered";
            var rootTamper = fixture.Pair with
            {
                RootJobId = rootTamperJobId,
                RootOwnershipBinding = DesignConversionDpoLeakageGuard.HashRef(
                    "dcm-runtime-rejected-root-ownership",
                    rootTamperJobId + "|" + fixture.Pair.RootRequestId)
            };
            var directionTamper = fixture.Pair with
            {
                IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                {
                    PreferredDirection =
                        DesignConversionIndependentPreferenceEvidenceContract
                            .IncumbentSide
                }
            };
            var swapped = fixture.Pair with
            {
                Generated = fixture.Pair.Incumbent with
                {
                    Side = DesignConversionDpoPolicyRuntimePairContract
                        .GeneratedSide
                },
                Incumbent = fixture.Pair.Generated with
                {
                    Side = DesignConversionDpoPolicyRuntimePairContract
                        .IncumbentSide
                }
            };
            var tamperedPairs = new Dictionary<string,
                DesignConversionDpoPolicyRuntimePair>
            {
                ["pair-binding"] = fixture.Pair with
                {
                    PairBindingSha256 = DesignConversionDpoGenerator.Sha256Ref(
                        "tampered-pair-binding")
                },
                ["content"] = Rebind(fixture.Pair with
                {
                    Generated = fixture.Pair.Generated with
                    {
                        Html = fixture.Pair.Generated.Html + "<!--tampered-->"
                    }
                }),
                ["content-hash-bypass"] = Rebind(fixture.Pair with
                {
                    Generated = fixture.Pair.Generated with
                    {
                        ContentSha256 = DesignConversionDpoGenerator.Sha256Ref(
                            "tampered-content-hash")
                    }
                }),
                ["generated-custody-ref"] = Rebind(fixture.Pair with
                {
                    Generated = fixture.Pair.Generated with
                    {
                        CustodyRef = fixture.Pair.Generated.CustodyRef +
                            ":tampered"
                    }
                }),
                ["incumbent-custody-ref"] = Rebind(fixture.Pair with
                {
                    Incumbent = fixture.Pair.Incumbent with
                    {
                        CustodyRef = fixture.Pair.Incumbent.CustodyRef +
                            ":tampered"
                    }
                }),
                ["generated-custody-binding"] = Rebind(fixture.Pair with
                {
                    Generated = fixture.Pair.Generated with
                    {
                        CustodyBinding = fixture.Pair.Generated.CustodyBinding +
                            ":tampered"
                    }
                }),
                ["incumbent-custody-binding"] = Rebind(fixture.Pair with
                {
                    Incumbent = fixture.Pair.Incumbent with
                    {
                        CustodyBinding = fixture.Pair.Incumbent.CustodyBinding +
                            ":tampered"
                    }
                }),
                ["generated-output-id"] = Rebind(fixture.Pair with
                {
                    Generated = fixture.Pair.Generated with
                    {
                        OutputId = fixture.Pair.Generated.OutputId + ":tampered"
                    }
                }),
                ["generated-candidate-id"] = Rebind(fixture.Pair with
                {
                    Generated = fixture.Pair.Generated with
                    {
                        CandidateId = fixture.Pair.Generated.CandidateId +
                            ":tampered"
                    }
                }),
                ["generated-verdict-id"] = Rebind(fixture.Pair with
                {
                    Generated = fixture.Pair.Generated with
                    {
                        VerdictId = fixture.Pair.Generated.VerdictId +
                            ":tampered"
                    }
                }),
                ["blank-output-id"] = Rebind(fixture.Pair with
                {
                    Generated = fixture.Pair.Generated with { OutputId = " " }
                }),
                ["blank-candidate-id"] = Rebind(fixture.Pair with
                {
                    Generated = fixture.Pair.Generated with { CandidateId = "" }
                }),
                ["blank-verdict-id"] = Rebind(fixture.Pair with
                {
                    Generated = fixture.Pair.Generated with { VerdictId = "\t" }
                }),
                ["one-sided"] = Rebind(fixture.Pair with
                {
                    Incumbent = fixture.Pair.Incumbent with
                    {
                        Html = string.Empty,
                        Css = string.Empty,
                        ContentSha256 = DesignConversionDpoGenerator.Sha256Ref(
                            DesignConversionDpoPolicyCheckpointBridge
                                .SerializePolicySideText(string.Empty,
                                    string.Empty))
                    }
                }),
                ["duplicate-side"] = Rebind(fixture.Pair with
                {
                    Incumbent = fixture.Pair.Generated with
                    {
                        Side = DesignConversionDpoPolicyRuntimePairContract
                            .IncumbentSide
                    }
                }),
                ["swapped"] = Rebind(swapped),
                ["root"] = Rebind(rootTamper),
                ["root-request"] = Rebind(fixture.Pair with
                {
                    RootRequestId = fixture.Pair.RootRequestId + ":tampered"
                }),
                ["attempt"] = Rebind(fixture.Pair with
                {
                    AttemptNumber = fixture.Pair.AttemptNumber + 1
                }),
                ["generated-evaluation-job"] = Rebind(fixture.Pair with
                {
                    Generated = fixture.Pair.Generated with
                    {
                        EvaluationJobId =
                            fixture.Pair.Generated.EvaluationJobId + ":tampered"
                    }
                }),
                ["generated-evaluation-request"] = Rebind(fixture.Pair with
                {
                    Generated = fixture.Pair.Generated with
                    {
                        EvaluationRequestId = fixture.Pair.Generated
                            .EvaluationRequestId + ":tampered"
                    }
                }),
                ["artifact-root"] = Rebind(fixture.Pair with
                {
                    ArtifactRootBinding = DesignConversionDpoGenerator.Sha256Ref(
                        "tampered-artifact-root")
                }),
                ["stale"] = Rebind(fixture.Pair with
                {
                    ObservedAtUtc = fixture.Pair.IndependentEvaluation
                        .ExpiresAtUtc.AddSeconds(1)
                }),
                ["direction"] = Rebind(directionTamper),
                ["receipt-sha"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        ReceiptSha256 = DesignConversionDpoGenerator.Sha256Ref(
                            "tampered-receipt")
                    }
                }),
                ["profile-id"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        EvaluationProfileId = "tampered-profile"
                    }
                }),
                ["profile-version"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        EvaluationProfileVersion = "tampered-version"
                    }
                }),
                ["profile-config-sha"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        EvaluationProfileConfigurationSha256 =
                            DesignConversionDpoGenerator.Sha256Ref(
                                "tampered-profile-config")
                    }
                }),
                ["model-sha"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        EvaluationModelSha256 =
                            DesignConversionDpoGenerator.Sha256Ref(
                                "tampered-model")
                    }
                }),
                ["independent-schema"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        Schema = "tampered-independent-schema"
                    }
                }),
                ["independent-attempt"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        AttemptNumber = fixture.Pair.IndependentEvaluation
                            .AttemptNumber + 1
                    }
                }),
                ["independent-generated-ref"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        GeneratedRef = fixture.Pair.IndependentEvaluation
                            .GeneratedRef + ":tampered"
                    }
                }),
                ["independent-incumbent-ref"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        IncumbentRef = fixture.Pair.IndependentEvaluation
                            .IncumbentRef + ":tampered"
                    }
                }),
                ["independent-generated-binding"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        GeneratedBinding = fixture.Pair.IndependentEvaluation
                            .GeneratedBinding + ":tampered"
                    }
                }),
                ["independent-incumbent-binding"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        IncumbentBinding = fixture.Pair.IndependentEvaluation
                            .IncumbentBinding + ":tampered"
                    }
                }),
                ["scorer-request-sha"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        ScorerRequestSha256 =
                            DesignConversionDpoGenerator.Sha256Ref(
                                "tampered-scorer-request")
                    }
                }),
                ["scorer-output-sha"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        ScorerOutputSha256 =
                            DesignConversionDpoGenerator.Sha256Ref(
                                "tampered-scorer-output")
                    }
                }),
                ["scorer-provenance-kind"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        ScorerProvenance = fixture.Pair.IndependentEvaluation
                            .ScorerProvenance with { Kind = "tampered-kind" }
                    }
                }),
                ["scorer-provenance-id"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        ScorerProvenance = fixture.Pair.IndependentEvaluation
                            .ScorerProvenance with { ScorerId = "tampered-id" }
                    }
                }),
                ["scorer-provenance-version"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        ScorerProvenance = fixture.Pair.IndependentEvaluation
                            .ScorerProvenance with
                            {
                                ScorerVersion = "tampered-version"
                            }
                    }
                }),
                ["scorer-provenance-request"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        ScorerProvenance = fixture.Pair.IndependentEvaluation
                            .ScorerProvenance with
                            {
                                RequestId = "tampered-request"
                            }
                    }
                }),
                ["scorer-provenance-output"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        ScorerProvenance = fixture.Pair.IndependentEvaluation
                            .ScorerProvenance with
                            {
                                OutputId = "tampered-output"
                            }
                    }
                }),
                ["scorer-provenance-runtime"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        ScorerProvenance = fixture.Pair.IndependentEvaluation
                            .ScorerProvenance with
                            {
                                RuntimeInstanceRef = "tampered-runtime"
                            }
                    }
                }),
                ["scorer-provenance-inference-request"] = Rebind(
                    fixture.Pair with
                    {
                        IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                        {
                            ScorerProvenance = fixture.Pair.IndependentEvaluation
                                .ScorerProvenance with
                                {
                                    InferenceRequestId =
                                        "tampered-inference-request"
                            }
                        }
                    }),
                ["scorer-provenance-profile-id"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        ScorerProvenance = fixture.Pair.IndependentEvaluation
                            .ScorerProvenance with
                            {
                                EvaluationProfileId =
                                    "tampered-provenance-profile"
                            }
                    }
                }),
                ["scorer-provenance-profile-version"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        ScorerProvenance = fixture.Pair.IndependentEvaluation
                            .ScorerProvenance with
                            {
                                EvaluationProfileVersion =
                                    "tampered-provenance-version"
                            }
                    }
                }),
                ["scorer-provenance-profile-config"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        ScorerProvenance = fixture.Pair.IndependentEvaluation
                            .ScorerProvenance with
                            {
                                EvaluationProfileConfigurationSha256 =
                                    DesignConversionDpoGenerator.Sha256Ref(
                                        "tampered-provenance-config")
                            }
                    }
                }),
                ["scorer-provenance-model"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        ScorerProvenance = fixture.Pair.IndependentEvaluation
                            .ScorerProvenance with
                            {
                                EvaluationModelSha256 =
                                    DesignConversionDpoGenerator.Sha256Ref(
                                        "tampered-provenance-model")
                            }
                    }
                }),
                ["independent-artifact-root"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        ArtifactRootBinding = DesignConversionDpoGenerator.Sha256Ref(
                            "tampered-independent-artifact-root")
                    }
                }),
                ["independent-generated-identity"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        GeneratedEvaluationIdentity =
                            DesignConversionDpoGenerator.Sha256Ref(
                                "tampered-generated-identity")
                    }
                }),
                ["independent-incumbent-identity"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        IncumbentEvaluationIdentity =
                            DesignConversionDpoGenerator.Sha256Ref(
                                "tampered-incumbent-identity")
                    }
                }),
                ["source-pair-generated-evaluation"] = Rebind(fixture.Pair with
                {
                    SourcePair = fixture.Pair.SourcePair with
                    {
                        GeneratedEvaluation = fixture.Pair.SourcePair
                            .GeneratedEvaluation with
                            {
                                JobId = fixture.Pair.SourcePair
                                    .GeneratedEvaluation.JobId + ":tampered"
                            }
                    }
                }),
                ["source-pair-incumbent-evaluation"] = Rebind(fixture.Pair with
                {
                    SourcePair = fixture.Pair.SourcePair with
                    {
                        IncumbentEvaluation = fixture.Pair.SourcePair
                            .IncumbentEvaluation with
                            {
                                JobId = fixture.Pair.SourcePair
                                    .IncumbentEvaluation.JobId + ":tampered"
                            }
                    }
                }),
                ["issued-at"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        IssuedAtUtc = fixture.Pair.IndependentEvaluation.IssuedAtUtc
                            .AddSeconds(1)
                    }
                }),
                ["expires-at"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        ExpiresAtUtc = fixture.Pair.IndependentEvaluation.ExpiresAtUtc
                            .AddSeconds(1)
                    }
                }),
                ["replay-nonce"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = fixture.Pair.IndependentEvaluation with
                    {
                        ReplayNonce = fixture.Pair.IndependentEvaluation.ReplayNonce +
                            ":tampered"
                    }
                }),
                ["default-issuer"] = fixture.Pair with
                {
                    RuntimeAuthorityCapability = null
                },
                ["missing-receipt"] = Rebind(fixture.Pair with
                {
                    IndependentEvaluation = null!
                })
            };

            foreach (var tamper in tamperedPairs)
            {
                var probe = await RunMachineScoreAsync(
                    loaded.Value.Machine,
                    fixture.RuntimeInput with
                    {
                        DpoPolicyRuntimePair = tamper.Value
                    },
                    $"direct-dpo-policy-pair-tamper-{tamper.Key}");
                AssertDirectPolicyConfiguredPairUnavailableProbe(
                    probe,
                    baseline,
                    root);
            }

            var requestContentBindingTamper = await RunMachineScoreAsync(
                loaded.Value.Machine,
                fixture.RuntimeInput with
                {
                    Request = fixture.RuntimeInput.Request with
                    {
                        Html = fixture.RuntimeInput.Request.Html +
                            "<!--request-binding-tampered-->"
                    }
                },
                "direct-dpo-policy-pair-tamper-request-content-binding");
            AssertDirectPolicyConfiguredPairUnavailableProbe(
                requestContentBindingTamper,
                baseline,
                root);

            var reconstructedJson = JsonSerializer.Serialize(fixture.Pair);
            var reconstructed = JsonSerializer.Deserialize<
                DesignConversionDpoPolicyRuntimePair>(reconstructedJson);
            Assert.NotNull(reconstructed);
            Assert.Null(reconstructed!.RuntimeAuthorityCapability);
            Assert.Equal(reconstructedJson, JsonSerializer.Serialize(reconstructed));
            Assert.Equal(fixture.Pair.PairBindingSha256,
                reconstructed.PairBindingSha256);
            Assert.Equal(fixture.Pair.IndependentEvaluation.ReceiptSha256,
                reconstructed.IndependentEvaluation.ReceiptSha256);
            var reconstructionProbe = await RunMachineScoreAsync(
                loaded.Value.Machine,
                fixture.RuntimeInput with
                {
                    DpoPolicyRuntimePair = reconstructed
                },
                "direct-dpo-policy-pair-json-reconstruction");
            AssertDirectPolicyConfiguredPairUnavailableProbe(
                reconstructionProbe,
                baseline,
                root);

            var valid = await RunMachineScoreAsync(
                loaded.Value.Machine,
                fixture.RuntimeInput,
                "direct-dpo-policy-pair-tamper-valid");
            Assert.Contains(
                DesignConversionMachine.DpoPolicyDirectInferenceScoreEvidence,
                valid.EvidenceRefs);
            var replay = await RunMachineScoreAsync(
                loaded.Value.Machine,
                fixture.RuntimeInput,
                "direct-dpo-policy-pair-tamper-replay");
            AssertDirectPolicyConfiguredPairUnavailableProbe(
                replay,
                baseline,
                root);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("output-id")]
    [InlineData("candidate-id")]
    [InlineData("verdict-id")]
    public void RuntimePairFactory_EachGeneratedIncumbentIdentityCollisionFailsBeforeCapabilityMint(
        string collisionField)
    {
        var fixture = BuildExactRuntimePairFixture();
        var generatedOutput = fixture.Pair.SourcePair.GeneratedEvaluation
            .RuntimeFlowResult.Output;
        var mutatedIncumbent = MutateRuntimePairEvaluationCollision(
            fixture.Pair.SourcePair.IncumbentEvaluation,
            collisionField,
            generatedOutput);
        var sourcePair = fixture.Pair.SourcePair with
        {
            IncumbentEvaluation = mutatedIncumbent
        };
        var receipt = DesignConversionIndependentPreferenceEvidenceTestIssuer
            .Issue(
                sourcePair,
                fixture.Pair.ArtifactRootBinding,
                fixture.Pair.ObservedAtUtc);

        var created = DesignConversionDpoPolicyRuntimePairFactory.Create(
            fixture.RootRequest,
            sourcePair,
            receipt,
            fixture.Pair.ArtifactRootBinding,
            fixture.Pair.ObservedAtUtc);

        Assert.False(created.IsSuccess);
        Assert.Equal(
            DesignConversionDpoPolicyRuntimePairContract.PairContentInvalidReason,
            created.ErrorCode);
        Assert.Null(created.Value);
    }

    [Fact]
    public void DirectDpoPolicyEvaluatedInferencePairPackingRetainsDivergenceAfterLongCommonPrefix()
    {
        var config = BuildDirectPolicyFixtureConfig() with { ContextLen = 12 };
        var binding = BuildValidContinuationRuntimeInferenceBinding(
            config,
            evaluated: true);
        var commonPrefix = new string('a', 64);
        var chosen = $"{commonPrefix}X";
        var rejected = $"{commonPrefix}Y";

        var independentlyTruncatedChosen =
            DesignConversionDpoPolicyCheckpointBridge
                .EncodeRuntimePolicyTextForInference(
                    chosen,
                    binding,
                    config,
                    maxTokenCount: config.ContextLen / 2,
                    padToMinimum: false);
        var independentlyTruncatedRejected =
            DesignConversionDpoPolicyCheckpointBridge
                .EncodeRuntimePolicyTextForInference(
                    rejected,
                    binding,
                    config,
                    maxTokenCount: config.ContextLen / 2,
                    padToMinimum: false);
        Assert.True(
            independentlyTruncatedChosen.IsSuccess,
            independentlyTruncatedChosen.ErrorMessage);
        Assert.True(
            independentlyTruncatedRejected.IsSuccess,
            independentlyTruncatedRejected.ErrorMessage);
        Assert.Equal(
            independentlyTruncatedChosen.Value!.ToArray(),
            independentlyTruncatedRejected.Value!.ToArray());

        var pair = DesignConversionDpoPolicyCheckpointBridge
            .BuildRuntimePolicyPairBatchForInference(
                "prompt",
                chosen,
                rejected,
                binding,
                config);

        Assert.True(pair.IsSuccess, pair.ErrorMessage);
        Assert.Equal(config.ContextLen, pair.Value!.ChosenIds.Data.Length);
        Assert.Equal(config.ContextLen, pair.Value.RejectedIds.Data.Length);
        Assert.False(
            pair.Value.ChosenIds.Data.SequenceEqual(
                pair.Value.RejectedIds.Data));
        Assert.Contains((float)'X', pair.Value.ChosenIds.Data);
        Assert.Contains((float)'Y', pair.Value.RejectedIds.Data);
        Assert.Equal(
            pair.Value.ChosenMask.Data,
            pair.Value.RejectedMask.Data);
    }

    [Fact]
    public async Task LoadCheckpointAsync_DirectDpoPolicyPartialContinuationRuntimeEncodingBindingFailsClosedBeforeDirectScoreAuthority()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-runtime-binding-partial-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var config = BuildDirectPolicyFixtureConfig();
            var partialBindingEvidenceRefs = BuildValidContinuationRuntimeEncodingEvidence(config)
                .Where(evidence => !evidence.StartsWith(
                    "dpo_runtime_encoding_context_length:",
                    StringComparison.Ordinal))
                .ToArray();
            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-runtime-binding-partial-runtime.json",
                weightSeed: 59,
                directRuntimeKind: true,
                policyRefEvidenceRefs: partialBindingEvidenceRefs,
                evaluatedHeldOutAcceptanceAuthorityFactory: policyRef =>
                    BuildTypedDirectPolicyHeldOutAcceptanceAuthority(
                        policyRef,
                        accuracy: 1.0));

            var load = await DesignConversionMachine.LoadCheckpointAsync(runtime.RuntimeCheckpointPath);

            Assert.False(load.IsSuccess);
            Assert.Equal(
                DesignConversionDpoPolicyCheckpointBridge.ContinuationRuntimeEncodingInvalidErrorCode,
                load.ErrorCode);
            Assert.Null(load.Value);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_DirectDpoPolicyNonCanonicalContinuationRuntimeEncodingShaFailsClosedBeforeDirectScoreAuthority()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-runtime-binding-sha-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            const string tokenizerSha256Prefix = "dpo_runtime_encoding_tokenizer_sha256:";
            var config = BuildDirectPolicyFixtureConfig();
            var malformedBindingEvidenceRefs = BuildValidContinuationRuntimeEncodingEvidence(config)
                .Select(evidence => evidence.StartsWith(
                        tokenizerSha256Prefix,
                        StringComparison.Ordinal)
                    ? $"{tokenizerSha256Prefix}{new string('A', 64)}"
                    : evidence)
                .ToArray();
            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-runtime-binding-sha-runtime.json",
                weightSeed: 61,
                directRuntimeKind: true,
                policyRefEvidenceRefs: malformedBindingEvidenceRefs,
                evaluatedHeldOutAcceptanceAuthorityFactory: policyRef =>
                    BuildTypedDirectPolicyHeldOutAcceptanceAuthority(
                        policyRef,
                        accuracy: 1.0));

            var load = await DesignConversionMachine.LoadCheckpointAsync(runtime.RuntimeCheckpointPath);

            Assert.False(load.IsSuccess);
            Assert.Equal(
                DesignConversionDpoPolicyCheckpointBridge.ContinuationRuntimeEncodingInvalidErrorCode,
                load.ErrorCode);
            Assert.Null(load.Value);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("tokenRange")]
    [InlineData("tokenizerArtifactIdentity")]
    [InlineData("tokenizerVocabularyIdentity")]
    public async Task LoadCheckpointAsync_DirectDpoPolicyCanonicalButNonExactContinuationRuntimeEncodingBindingFailsClosed(
        string caseName)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            $"direct-dpo-policy-runtime-binding-nonexact-{caseName}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var config = BuildDirectPolicyFixtureConfig();
            var valid = BuildValidContinuationRuntimeInferenceBinding(
                config,
                evaluated: true);
            var invalid = caseName switch
            {
                "tokenRange" => valid with
                {
                    TokenIdMaximum = valid.TokenIdMaximum - 1
                },
                "tokenizerArtifactIdentity" => valid with
                {
                    TokenizerArtifactSha256 = MutateCanonicalSha256(
                        valid.TokenizerArtifactSha256)
                },
                "tokenizerVocabularyIdentity" => valid with
                {
                    TokenizerVocabularyIdentitySha256 = MutateCanonicalSha256(
                        valid.TokenizerVocabularyIdentitySha256)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(caseName))
            };
            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                $"direct-dpo-policy-runtime-binding-nonexact-{caseName}.json",
                weightSeed: 67,
                directRuntimeKind: true,
                policyRefEvidenceRefs:
                    BuildContinuationRuntimeEncodingEvidence(invalid),
                evaluatedHeldOutAcceptanceAuthorityFactory: policyRef =>
                    BuildTypedDirectPolicyHeldOutAcceptanceAuthority(
                        policyRef,
                        accuracy: 1.0));

            var load = await DesignConversionMachine.LoadCheckpointAsync(
                runtime.RuntimeCheckpointPath);

            Assert.False(load.IsSuccess);
            Assert.Equal(
                DesignConversionDpoPolicyCheckpointBridge
                    .ContinuationRuntimeEncodingInvalidErrorCode,
                load.ErrorCode);
            Assert.Null(load.Value);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("canonicalLegacyDowngrade")]
    [InlineData("bindingEvidenceStripped")]
    public async Task LoadCheckpointAsync_CurrentEvaluatedDirectPolicyRequiresEvaluatedPairAwareRuntimeBinding(
        string caseName)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            $"direct-dpo-policy-evaluated-binding-downgrade-{caseName}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var config = BuildDirectPolicyFixtureConfig();
            var policyRefEvidenceRefs = caseName switch
            {
                "canonicalLegacyDowngrade" =>
                    BuildValidContinuationRuntimeEncodingEvidence(
                        config,
                        evaluated: false),
                "bindingEvidenceStripped" => [],
                _ => throw new ArgumentOutOfRangeException(nameof(caseName))
            };
            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                $"direct-dpo-policy-evaluated-binding-downgrade-{caseName}.json",
                weightSeed: 71,
                directRuntimeKind: true,
                policyRefEvidenceRefs: policyRefEvidenceRefs,
                evaluatedHeldOutAcceptanceAuthorityFactory: policyRef =>
                    BuildTypedDirectPolicyHeldOutAcceptanceAuthority(
                        policyRef,
                        accuracy: 1.0),
                emitEvaluatedRuntimeEncodingBinding: false);

            var load = await DesignConversionMachine.LoadCheckpointAsync(
                runtime.RuntimeCheckpointPath);

            Assert.False(load.IsSuccess);
            Assert.Equal(
                DesignConversionDpoPolicyCheckpointBridge
                    .ContinuationRuntimeEncodingInvalidErrorCode,
                load.ErrorCode);
            Assert.Null(load.Value);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_DirectDpoPolicyCanonicalTopLevelHeldOutEvidenceWithoutTypedEvaluation_BaselineOwns()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-top-level-no-typed-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            const double checkpointAccuracy = 1.0;
            const double configuredThreshold = 0.0;
            var config = BuildDirectPolicyFixtureConfig();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "direct-dpo-policy-top-level-no-typed-baseline");
            Assert.False(baselineProbe.Passed);

            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-top-level-no-typed-runtime.json",
                weightSeed: 13,
                directRuntimeKind: true,
                topLevelEvidenceRefs: BuildEvaluatedRuntimeCheckpointEvidence(checkpointAccuracy),
                accuracyAfterLastBatch: checkpointAccuracy);

            var load = await DesignConversionMachine.LoadCheckpointWithEvaluationOptionsAsync(
                runtime.RuntimeCheckpointPath,
                new DesignConversionMachineEvaluationOptions(configuredThreshold));
            Assert.True(load.IsSuccess, load.ErrorMessage);

            var probe = await RunMachineScoreAsync(
                load.Value!.Machine,
                runtimeInput,
                "direct-dpo-policy-top-level-no-typed-probe");

            AssertDirectPolicyConfiguredPairUnavailableProbe(probe, baselineProbe, root);
            Assert.DoesNotContain(DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityEvidence, probe.EvidenceRefs);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_DirectDpoPolicyTypedEvaluationPolicyBindingTampered_BaselineOwns()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-typed-tamper-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            const double checkpointAccuracy = 1.0;
            const double configuredThreshold = 0.0;
            var config = BuildDirectPolicyFixtureConfig();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "direct-dpo-policy-typed-tamper-baseline");
            Assert.False(baselineProbe.Passed);

            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-typed-tamper-runtime.json",
                weightSeed: 19,
                directRuntimeKind: true,
                topLevelEvidenceRefs: BuildEvaluatedRuntimeCheckpointEvidence(checkpointAccuracy),
                accuracyAfterLastBatch: checkpointAccuracy,
                evaluatedHeldOutAcceptanceAuthorityFactory: policyRef =>
                    BuildTypedDirectPolicyHeldOutAcceptanceAuthority(policyRef, checkpointAccuracy) with
                    {
                        ChildPolicyCheckpointSha256 = new string('0', 64)
                    });

            var load = await DesignConversionMachine.LoadCheckpointWithEvaluationOptionsAsync(
                runtime.RuntimeCheckpointPath,
                new DesignConversionMachineEvaluationOptions(configuredThreshold));
            Assert.True(load.IsSuccess, load.ErrorMessage);

            var probe = await RunMachineScoreAsync(
                load.Value!.Machine,
                runtimeInput,
                "direct-dpo-policy-typed-tamper-probe");

            AssertDirectPolicyConfiguredPairUnavailableProbe(probe, baselineProbe, root);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_DirectDpoPolicyTypedEvaluationTrainCountBindingTampered_BaselineOwns()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-typed-count-tamper-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            const double checkpointAccuracy = 1.0;
            const double configuredThreshold = 0.0;
            var config = BuildDirectPolicyFixtureConfig();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "direct-dpo-policy-typed-count-tamper-baseline");
            Assert.False(baselineProbe.Passed);

            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-typed-count-tamper-runtime.json",
                weightSeed: 23,
                directRuntimeKind: true,
                topLevelEvidenceRefs: BuildEvaluatedRuntimeCheckpointEvidence(checkpointAccuracy),
                accuracyAfterLastBatch: checkpointAccuracy,
                evaluatedHeldOutAcceptanceAuthorityFactory: policyRef =>
                {
                    var validAuthority =
                        BuildTypedDirectPolicyHeldOutAcceptanceAuthority(policyRef, checkpointAccuracy);
                    var tamperedTrainRecordCount = policyRef.BridgePreferenceCount + 1;
                    var tamperedHeldOutRecordCount =
                        policyRef.SourceRecordCount - tamperedTrainRecordCount;
                    Assert.NotEqual(policyRef.BridgePreferenceCount, tamperedTrainRecordCount);
                    Assert.True(tamperedHeldOutRecordCount > 0);
                    Assert.Equal(
                        policyRef.SourceRecordCount,
                        tamperedTrainRecordCount + tamperedHeldOutRecordCount);
                    return validAuthority with
                    {
                        MaxBridgePreferenceCount = tamperedTrainRecordCount,
                        TrainRecordCount = tamperedTrainRecordCount,
                        HeldOutRecordCount = tamperedHeldOutRecordCount,
                        EvaluatedHeldOutRecordCount = tamperedHeldOutRecordCount,
                        CorrectCount = tamperedHeldOutRecordCount,
                        Accuracy = checkpointAccuracy
                    };
                });

            var load = await DesignConversionMachine.LoadCheckpointWithEvaluationOptionsAsync(
                runtime.RuntimeCheckpointPath,
                new DesignConversionMachineEvaluationOptions(configuredThreshold));
            Assert.True(load.IsSuccess, load.ErrorMessage);

            var probe = await RunMachineScoreAsync(
                load.Value!.Machine,
                runtimeInput,
                "direct-dpo-policy-typed-count-tamper-probe");

            AssertDirectPolicyConfiguredPairUnavailableProbe(probe, baselineProbe, root);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_EvaluatedDirectDpoPolicyThresholdLow_RuntimePairUnavailablePreservesBaselineFailure()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-authority-low-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            const double checkpointAccuracy = 1.0;
            const double configuredThreshold = 0.0;
            var config = BuildDirectPolicyFixtureConfig();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "direct-dpo-policy-authority-low-baseline");
            Assert.False(baselineProbe.Passed);

            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-authority-low-runtime.json",
                weightSeed: 17,
                directRuntimeKind: true,
                topLevelEvidenceRefs: BuildEvaluatedRuntimeCheckpointEvidence(checkpointAccuracy),
                accuracyAfterLastBatch: checkpointAccuracy,
                evaluatedHeldOutAcceptanceAuthorityFactory: policyRef =>
                    BuildTypedDirectPolicyHeldOutAcceptanceAuthority(policyRef, checkpointAccuracy));

            var load = await DesignConversionMachine.LoadCheckpointWithEvaluationOptionsAsync(
                runtime.RuntimeCheckpointPath,
                new DesignConversionMachineEvaluationOptions(configuredThreshold));
            Assert.True(load.IsSuccess, load.ErrorMessage);

            var probe = await RunMachineScoreAsync(
                load.Value!.Machine,
                runtimeInput,
                "direct-dpo-policy-authority-low-probe");

            AssertDirectPolicyConfiguredPairUnavailableProbe(probe, baselineProbe, root);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_EvaluatedDirectDpoPolicyThresholdHigh_RuntimePairUnavailablePreservesBaselinePass()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-authority-high-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            const double checkpointAccuracy = 1.0;
            const double configuredThreshold = 1.0;
            var config = BuildDirectPolicyFixtureConfig();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.91, comparisonPassed: true);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "direct-dpo-policy-authority-high-baseline");
            Assert.True(baselineProbe.Passed);

            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-authority-high-runtime.json",
                weightSeed: 29,
                directRuntimeKind: true,
                topLevelEvidenceRefs: BuildEvaluatedRuntimeCheckpointEvidence(checkpointAccuracy),
                accuracyAfterLastBatch: checkpointAccuracy,
                evaluatedHeldOutAcceptanceAuthorityFactory: policyRef =>
                    BuildTypedDirectPolicyHeldOutAcceptanceAuthority(policyRef, checkpointAccuracy));

            var load = await DesignConversionMachine.LoadCheckpointWithEvaluationOptionsAsync(
                runtime.RuntimeCheckpointPath,
                new DesignConversionMachineEvaluationOptions(configuredThreshold));
            Assert.True(load.IsSuccess, load.ErrorMessage);

            var probe = await RunMachineScoreAsync(
                load.Value!.Machine,
                runtimeInput,
                "direct-dpo-policy-authority-high-probe");

            AssertDirectPolicyConfiguredPairUnavailableProbe(probe, baselineProbe, root);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_DirectDpoPolicyWithoutEvaluatedRuntimeEvidence_RuntimePairUnavailablePreservesBaseline()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-no-eval-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            const double configuredThreshold = 0.0;
            var config = BuildDirectPolicyFixtureConfig();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "direct-dpo-policy-no-eval-baseline");
            Assert.False(baselineProbe.Passed);

            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-no-eval-runtime.json",
                weightSeed: 37,
                directRuntimeKind: true);

            var load = await DesignConversionMachine.LoadCheckpointWithEvaluationOptionsAsync(
                runtime.RuntimeCheckpointPath,
                new DesignConversionMachineEvaluationOptions(configuredThreshold));
            Assert.True(load.IsSuccess, load.ErrorMessage);

            var probe = await RunMachineScoreAsync(
                load.Value!.Machine,
                runtimeInput,
                "direct-dpo-policy-no-eval-probe");

            AssertDirectPolicyConfiguredPairUnavailableProbe(probe, baselineProbe, root);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_EvaluatedDirectDpoPolicyBelowHeldOutThreshold_BaselineOwns()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-low-heldout-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            const double checkpointAccuracy = 0.50;
            const double configuredThreshold = 1.0;
            var config = BuildDirectPolicyFixtureConfig();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.91, comparisonPassed: true);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "direct-dpo-policy-low-heldout-baseline");
            Assert.True(baselineProbe.Passed);

            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-low-heldout-runtime.json",
                weightSeed: 41,
                directRuntimeKind: true,
                topLevelEvidenceRefs: BuildEvaluatedRuntimeCheckpointEvidence(checkpointAccuracy),
                accuracyAfterLastBatch: checkpointAccuracy,
                evaluatedHeldOutAcceptanceAuthorityFactory: policyRef =>
                    BuildTypedDirectPolicyHeldOutAcceptanceAuthority(policyRef, checkpointAccuracy));

            var load = await DesignConversionMachine.LoadCheckpointWithEvaluationOptionsAsync(
                runtime.RuntimeCheckpointPath,
                new DesignConversionMachineEvaluationOptions(configuredThreshold));
            Assert.True(load.IsSuccess, load.ErrorMessage);

            var probe = await RunMachineScoreAsync(
                load.Value!.Machine,
                runtimeInput,
                "direct-dpo-policy-low-heldout-probe");

            AssertDirectPolicyConfiguredPairUnavailableProbe(probe, baselineProbe, root);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_EvaluatedDirectDpoPolicy_TrainBatchKeepsConfiguredPairUnavailableFallback()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-train-revoke-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            const double checkpointAccuracy = 1.0;
            const double configuredThreshold = 0.0;
            var config = BuildDirectPolicyFixtureConfig();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "direct-dpo-policy-train-revoke-baseline");
            Assert.False(baselineProbe.Passed);

            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-train-revoke-runtime.json",
                weightSeed: 47,
                directRuntimeKind: true,
                topLevelEvidenceRefs: BuildEvaluatedRuntimeCheckpointEvidence(checkpointAccuracy),
                accuracyAfterLastBatch: checkpointAccuracy,
                evaluatedHeldOutAcceptanceAuthorityFactory: policyRef =>
                    BuildTypedDirectPolicyHeldOutAcceptanceAuthority(policyRef, checkpointAccuracy));

            var load = await DesignConversionMachine.LoadCheckpointWithEvaluationOptionsAsync(
                runtime.RuntimeCheckpointPath,
                new DesignConversionMachineEvaluationOptions(configuredThreshold));
            Assert.True(load.IsSuccess, load.ErrorMessage);

            var activeProbe = await RunMachineScoreAsync(
                load.Value!.Machine,
                runtimeInput,
                "direct-dpo-policy-train-revoke-active-probe");
            AssertDirectPolicyConfiguredPairUnavailableProbe(activeProbe, baselineProbe, root);

            var train = await DesignConversionLegalTeacherCapabilityTestFactory
                .TrainMachineStrictAsync(
                    load.Value.Machine,
                    BuildPass150Batch("direct-dpo-policy-train-revoke-batch"));
            Assert.True(train.IsSuccess, train.ErrorMessage);
            Assert.Equal(checkpointAccuracy, train.Value!.AccuracyBefore);
            Assert.Equal(checkpointAccuracy, train.Value.AccuracyAfter);

            var revokedProbe = await RunMachineScoreAsync(
                load.Value.Machine,
                runtimeInput,
                "direct-dpo-policy-train-revoke-new-probe");

            AssertDirectPolicyConfiguredPairUnavailableProbe(revokedProbe, baselineProbe, root);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_EvaluatedDirectDpoPolicy_EvaluateAsyncKeepsConfiguredPairUnavailableFallback()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-eval-revoke-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            const double checkpointAccuracy = 1.0;
            const double configuredThreshold = 0.0;
            var config = BuildDirectPolicyFixtureConfig();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "direct-dpo-policy-eval-revoke-baseline");
            Assert.False(baselineProbe.Passed);

            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-eval-revoke-runtime.json",
                weightSeed: 53,
                directRuntimeKind: true,
                topLevelEvidenceRefs: BuildEvaluatedRuntimeCheckpointEvidence(checkpointAccuracy),
                accuracyAfterLastBatch: checkpointAccuracy,
                evaluatedHeldOutAcceptanceAuthorityFactory: policyRef =>
                    BuildTypedDirectPolicyHeldOutAcceptanceAuthority(policyRef, checkpointAccuracy));

            var load = await DesignConversionMachine.LoadCheckpointWithEvaluationOptionsAsync(
                runtime.RuntimeCheckpointPath,
                new DesignConversionMachineEvaluationOptions(configuredThreshold));
            Assert.True(load.IsSuccess, load.ErrorMessage);

            var activeProbe = await RunMachineScoreAsync(
                load.Value!.Machine,
                runtimeInput,
                "direct-dpo-policy-eval-revoke-active-probe");
            AssertDirectPolicyConfiguredPairUnavailableProbe(activeProbe, baselineProbe, root);

            var evaluation = await load.Value.Machine.EvaluateAsync(BuildThreeOfFourHeldOutEvaluationSet(
                "direct-dpo-policy-eval-revoke-generic-eval"));
            Assert.True(evaluation.IsSuccess, evaluation.ErrorMessage);
            Assert.Equal(4, evaluation.Value!.RowsEvaluated);
            var checkpointAfterEvaluation = load.Value.Machine.ExportCheckpoint();
            Assert.True(checkpointAfterEvaluation.IsSuccess, checkpointAfterEvaluation.ErrorMessage);
            Assert.Contains(
                DesignConversionMachine.EvaluateAsyncHeldOutAccuracyMeasuredEvidence,
                checkpointAfterEvaluation.Value!.EvidenceRefs);
            Assert.DoesNotContain(
                DesignConversionMachine.DpoRuntimeCheckpointAccuracyFromHeldOutEvaluationEvidence,
                checkpointAfterEvaluation.Value.EvidenceRefs);

            var revokedProbe = await RunMachineScoreAsync(
                load.Value.Machine,
                runtimeInput,
                "direct-dpo-policy-eval-revoke-new-probe");

            AssertDirectPolicyConfiguredPairUnavailableProbe(revokedProbe, baselineProbe, root);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [MemberData(nameof(MalformedDirectDpoPolicyAuthorityEvidenceCases))]
    public async Task LoadCheckpointAsync_DirectDpoPolicyMalformedAuthorityEvidence_RuntimePairUnavailablePreservesBaseline(
        string caseId,
        string[] topLevelEvidenceRefs)
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-authority-malformed-{caseId}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            const double checkpointAccuracy = 1.0;
            const double configuredThreshold = 0.0;
            var config = BuildDirectPolicyFixtureConfig();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                $"direct-dpo-policy-authority-malformed-{caseId}-baseline");
            Assert.False(baselineProbe.Passed);

            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                $"direct-dpo-policy-authority-malformed-{caseId}-runtime.json",
                weightSeed: 61,
                directRuntimeKind: true,
                topLevelEvidenceRefs: topLevelEvidenceRefs,
                accuracyAfterLastBatch: checkpointAccuracy);

            var load = await DesignConversionMachine.LoadCheckpointWithEvaluationOptionsAsync(
                runtime.RuntimeCheckpointPath,
                new DesignConversionMachineEvaluationOptions(configuredThreshold));
            Assert.True(load.IsSuccess, load.ErrorMessage);
            Assert.Equal(DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind, load.Value!.Checkpoint.DpoPolicyCheckpointRef!.RuntimeBridgeKind);

            var probe = await RunMachineScoreAsync(
                load.Value.Machine,
                runtimeInput,
                $"direct-dpo-policy-authority-malformed-{caseId}-probe");

            AssertDirectPolicyConfiguredPairUnavailableProbe(probe, baselineProbe, root);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_DirectDpoPolicyPolicyRefSpoofedAuthorityEvidence_BaselineOwnsWithoutInjectedAuthorityLeak()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-authority-spoof-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            const double configuredThreshold = 0.0;
            var config = BuildDirectPolicyFixtureConfig();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "direct-dpo-policy-authority-spoof-baseline");
            Assert.False(baselineProbe.Passed);

            var spoofedAuthorityEvidenceRefs = BuildSpoofedDirectPolicyAuthorityEvidence();
            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-authority-spoof-runtime.json",
                weightSeed: 67,
                directRuntimeKind: true,
                topLevelEvidenceRefs: spoofedAuthorityEvidenceRefs,
                accuracyAfterLastBatch: 1.0,
                policyRefEvidenceRefs: spoofedAuthorityEvidenceRefs);

            var load = await DesignConversionMachine.LoadCheckpointWithEvaluationOptionsAsync(
                runtime.RuntimeCheckpointPath,
                new DesignConversionMachineEvaluationOptions(configuredThreshold));
            Assert.True(load.IsSuccess, load.ErrorMessage);
            Assert.Equal(DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind, load.Value!.Checkpoint.DpoPolicyCheckpointRef!.RuntimeBridgeKind);

            var probe = await RunMachineScoreAsync(
                load.Value.Machine,
                runtimeInput,
                "direct-dpo-policy-authority-spoof-probe");

            AssertDirectPolicyConfiguredPairUnavailableProbe(probe, baselineProbe, root);
            Assert.DoesNotContain(DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityEvidence, probe.EvidenceRefs);
            Assert.Equal(bool.FalseString, probe.Metrics["direct_dpo_policy_acceptance_authority"]);
            Assert.Equal(bool.TrueString, probe.Metrics["baseline_visual_verdict_acceptance_authority"]);
            AssertPublicSurfaceDoesNotContain(probe, spoofedAuthorityEvidenceRefs);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_DirectDpoPolicyHostilePolicyRefAndTopLevelEvidence_LoadsAndDoesNotLeakPublicSurface()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-hostile-evidence-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            const double checkpointAccuracy = 1.0;
            const double configuredThreshold = 0.0;
            var config = BuildDirectPolicyFixtureConfig();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var hostileEvidenceRefs = BuildHostileDirectPolicyRuntimeEvidenceRefs();
            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-hostile-evidence-runtime.json",
                weightSeed: 71,
                directRuntimeKind: true,
                topLevelEvidenceRefs: BuildEvaluatedRuntimeCheckpointEvidence(checkpointAccuracy)
                    .Concat(hostileEvidenceRefs)
                    .ToArray(),
                accuracyAfterLastBatch: checkpointAccuracy,
                policyRefEvidenceRefs: hostileEvidenceRefs,
                evaluatedHeldOutAcceptanceAuthorityFactory: policyRef =>
                    BuildTypedDirectPolicyHeldOutAcceptanceAuthority(policyRef, checkpointAccuracy));

            var load = await DesignConversionMachine.LoadCheckpointWithEvaluationOptionsAsync(
                runtime.RuntimeCheckpointPath,
                new DesignConversionMachineEvaluationOptions(configuredThreshold));
            Assert.True(load.IsSuccess, load.ErrorMessage);

            var probe = await RunMachineScoreAsync(
                load.Value!.Machine,
                runtimeInput,
                "direct-dpo-policy-hostile-evidence-probe");

            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "direct-dpo-policy-hostile-evidence-baseline");
            AssertDirectPolicyConfiguredPairUnavailableProbe(probe, baselineProbe, root);
            AssertPublicSurfaceDoesNotContain(probe, hostileEvidenceRefs);
            Assert.DoesNotContain(probe.EvidenceRefs, evidence => evidence.Contains('\\'));
            Assert.DoesNotContain(probe.EvidenceRefs, evidence => evidence.StartsWith("/", StringComparison.Ordinal));
            Assert.DoesNotContain(probe.EvidenceRefs, evidence => evidence.Contains('<'));
            Assert.DoesNotContain(probe.EvidenceRefs, evidence => evidence.Contains('{'));
            Assert.DoesNotContain(probe.EvidenceRefs, evidence => evidence.Contains('"'));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_DirectDpoPolicySchemaRequiredSlashAndAtProvenance_RemainsPublic()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-schema-provenance-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            const string providerKind = "direct-policy-provider@unit";
            const string configKind = "direct-policy-config@unit";
            const string baseModelId = "fixture-org/direct-policy@unit";
            const string baseRevision = "unit-test@v1";
            var config = BuildDirectPolicyFixtureConfig();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "direct-dpo-policy-schema-provenance-baseline");

            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-schema-provenance-runtime.json",
                weightSeed: 73,
                directRuntimeKind: true,
                providerKind: providerKind,
                configKind: configKind,
                baseModelId: baseModelId,
                baseRevision: baseRevision);

            var load = await DesignConversionMachine.LoadCheckpointAsync(runtime.RuntimeCheckpointPath);
            Assert.True(load.IsSuccess, load.ErrorMessage);

            var probe = await RunMachineScoreAsync(
                load.Value!.Machine,
                runtimeInput,
                "direct-dpo-policy-schema-provenance-probe");

            AssertDirectPolicyConfiguredPairUnavailableProbe(probe, baselineProbe, root);
            AssertRequiredDirectPolicyProvenanceEvidence(probe, runtime.PolicyCheckpointRef);
            Assert.Contains($"dpo_policy_base_provider_kind:{providerKind}", probe.EvidenceRefs);
            Assert.Contains($"dpo_policy_base_configuration_kind:{configKind}", probe.EvidenceRefs);
            Assert.Contains($"dpo_policy_base_model_id:{baseModelId}", probe.EvidenceRefs);
            Assert.Contains($"dpo_policy_base_revision:{baseRevision}", probe.EvidenceRefs);
            Assert.Contains($"dpo_policy_base_model_id:{baseModelId}", BuildRuntimePublicSurface(probe), StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_DirectDpoPolicyLoraRequiredEvidence_RemainsPublicAndTopologyIsRuntimeGenerated()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-lora-provenance-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var config = BuildDirectPolicyFixtureConfig();
            var loraContext = BuildDirectPolicyFixtureLoraContext(config);
            var hostileEvidenceRefs = BuildHostileDirectPolicyRuntimeEvidenceRefs();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "direct-dpo-policy-lora-provenance-baseline");

            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-lora-provenance-runtime.json",
                weightSeed: 79,
                directRuntimeKind: true,
                policyRefEvidenceRefs: hostileEvidenceRefs,
                loraContext: loraContext);

            Assert.NotNull(runtime.PolicyCheckpointRef.TrainingContext.Lora);
            Assert.DoesNotContain(
                DesignConversionMachine.DpoPolicyRuntimeLoraTopologyEvidence,
                runtime.PolicyCheckpointRef.EvidenceRefs);

            var load = await DesignConversionMachine.LoadCheckpointAsync(runtime.RuntimeCheckpointPath);
            Assert.True(load.IsSuccess, load.ErrorMessage);
            Assert.NotNull(load.Value!.Checkpoint.DpoPolicyCheckpointRef!.TrainingContext.Lora);

            var probe = await RunMachineScoreAsync(
                load.Value.Machine,
                runtimeInput,
                "direct-dpo-policy-lora-provenance-probe");

            AssertDirectPolicyConfiguredPairUnavailableProbe(probe, baselineProbe, root);
            AssertRequiredDirectPolicyProvenanceEvidence(probe, runtime.PolicyCheckpointRef);
            AssertRequiredDirectPolicyLoraEvidence(probe, loraContext);
            Assert.Contains(DesignConversionMachine.DpoPolicyRuntimeLoraTopologyEvidence, probe.EvidenceRefs);
            AssertPublicSurfaceDoesNotContain(probe, hostileEvidenceRefs);
            Assert.DoesNotContain(probe.EvidenceRefs, evidence => evidence.Contains(root, StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(probe.EvidenceRefs, evidence => evidence.Contains(".card{", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(probe.EvidenceRefs, evidence => evidence.Contains("\"type\":\"FRAME\"", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_DirectDpoPolicyAdjacentRoundTripAccuracyMismatch_BaselineOwns()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-adjacent-accuracy-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            const double checkpointAccuracy = 0.5;
            const double configuredThreshold = 0.0;
            var adjacentAccuracy = BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(checkpointAccuracy) + 1);
            var roundTrippedAdjacentAccuracy = double.Parse(
                adjacentAccuracy.ToString("R", CultureInfo.InvariantCulture),
                CultureInfo.InvariantCulture);
            Assert.NotEqual(checkpointAccuracy, adjacentAccuracy);
            Assert.Equal(adjacentAccuracy, roundTrippedAdjacentAccuracy);

            var config = BuildDirectPolicyFixtureConfig();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "direct-dpo-policy-adjacent-accuracy-baseline");
            Assert.False(baselineProbe.Passed);

            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-adjacent-accuracy-runtime.json",
                weightSeed: 73,
                directRuntimeKind: true,
                topLevelEvidenceRefs: BuildEvaluatedRuntimeCheckpointEvidence(adjacentAccuracy),
                accuracyAfterLastBatch: checkpointAccuracy);

            var load = await DesignConversionMachine.LoadCheckpointWithEvaluationOptionsAsync(
                runtime.RuntimeCheckpointPath,
                new DesignConversionMachineEvaluationOptions(configuredThreshold));
            Assert.True(load.IsSuccess, load.ErrorMessage);

            var probe = await RunMachineScoreAsync(
                load.Value!.Machine,
                runtimeInput,
                "direct-dpo-policy-adjacent-accuracy-probe");

            AssertDirectPolicyConfiguredPairUnavailableProbe(probe, baselineProbe, root);
            Assert.DoesNotContain(DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityEvidence, probe.EvidenceRefs);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_UntypedDefaultLiteralBindsCancellationTokenOverload()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"load-default-literal-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var path = Path.Combine(root, "default-literal-checkpoint.json");
            var save = await new DesignConversionMachine().SaveCheckpointAsync(path);
            Assert.True(save.IsSuccess, save.ErrorMessage);

            var load = await DesignConversionMachine.LoadCheckpointAsync(path, default);

            Assert.True(load.IsSuccess, load.ErrorMessage);
            Assert.Equal(DesignConversionMachine.MachineId, load.Value!.Checkpoint.MachineId);
            Assert.Equal(DesignConversionMachine.DomainTag, load.Value.Checkpoint.DomainTag);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointWithEvaluationOptionsAsync_NullEvaluationOptionsReturnsStableCode()
    {
        var result = await DesignConversionMachine.LoadCheckpointWithEvaluationOptionsAsync(
            "null-options-should-not-read.json",
            (DesignConversionMachineEvaluationOptions)null!);

        Assert.False(result.IsSuccess);
        Assert.Equal("EVALUATION_OPTIONS_REQUIRED", result.ErrorCode);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LoadCheckpointAsync_CanceledTokenReturnsCancelledWithoutCheckpointReaderIo(bool explicitOptions)
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var readerCalls = 0;
        using (DesignConversionMachine.UseCheckpointByteReaderForCurrentAsyncFlow((_, _) =>
        {
            readerCalls++;
            return Task.FromResult(Array.Empty<byte>());
        }))
        {
            var result = explicitOptions
                ? await DesignConversionMachine.LoadCheckpointWithEvaluationOptionsAsync(
                    "cancel-before-io-explicit-options.json",
                    DesignConversionMachineEvaluationOptions.Default,
                    cts.Token)
                : await DesignConversionMachine.LoadCheckpointAsync(
                    "cancel-before-io-default-options.json",
                    cts.Token);

            Assert.False(result.IsSuccess);
            Assert.Equal("CANCELLED", result.ErrorCode);
            Assert.Equal(0, readerCalls);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_DirectDpoPolicy_MalformedHashValidPayloadFailsStableModelLoadCode()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"direct-dpo-policy-malformed-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var malformedPayload = new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55 };
            var runtime = await SaveRuntimeCheckpointWithPolicyPayloadAsync(
                root,
                "malformed-direct-runtime.json",
                malformedPayload,
                BuildDirectPolicyFixtureConfig(),
                DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind);

            var load = await DesignConversionMachine.LoadCheckpointAsync(runtime.RuntimeCheckpointPath);

            Assert.False(load.IsSuccess);
            Assert.Equal(DesignConversionMachine.DpoPolicyCheckpointPayloadModelLoadFailedCode, load.ErrorCode);
            Assert.Null(load.Value);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_DirectDpoPolicy_LegacyNotDirectSiblingKindLoadsWithoutPolicyAuthority()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"legacy-dpo-policy-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var legacyPayload = new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55 };
            var runtime = await SaveRuntimeCheckpointWithPolicyPayloadAsync(
                root,
                "legacy-runtime.json",
                legacyPayload,
                BuildDirectPolicyFixtureConfig(),
                DesignConversionMachine.DpoPolicyLegacySiblingPayloadRuntimeBridgeKind);

            var load = await DesignConversionMachine.LoadCheckpointAsync(runtime.RuntimeCheckpointPath);

            Assert.True(load.IsSuccess, load.ErrorMessage);
            Assert.Equal(DesignConversionMachine.DpoPolicyLegacySiblingPayloadRuntimeBridgeKind, load.Value!.Checkpoint.DpoPolicyCheckpointRef!.RuntimeBridgeKind);

            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "legacy-dpo-policy-baseline");
            var legacyProbe = await RunMachineScoreAsync(
                load.Value.Machine,
                runtimeInput,
                "legacy-dpo-policy-probe");

            Assert.Equal(baselineProbe.Score, legacyProbe.Score);
            Assert.Equal(baselineProbe.Passed, legacyProbe.Passed);
            Assert.DoesNotContain(DesignConversionMachine.DpoPolicyDirectInferenceScoreEvidence, legacyProbe.EvidenceRefs);
            Assert.Equal(bool.FalseString, legacyProbe.Metrics.GetValueOrDefault("direct_dpo_policy_inference_applied", bool.FalseString));
            Assert.DoesNotContain("direct_dpo_policy_score_authority", legacyProbe.Metrics.Keys);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<RuntimeScoreProbe> RunMachineScoreAsync(
        DesignConversionMachine machine,
        DesignConversionRuntimeFlowInput input,
        string runId)
    {
        var orchestrator = await machine.GetOrchestratorAsync();
        Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);

        var result = await orchestrator.Value!.RunAsync(new LogicalOrchestratorRequest(
            RunId: runId,
            ScenarioId: $"{runId}-scenario",
            UserRequest: "convert this design",
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["runtime_flow_input"] = input
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        return new RuntimeScoreProbe(
            Score: result.Value!.FlowResult.Summary.EnhancedPrimary,
            Passed: result.Value.FlowResult.Summary.Passed,
            OverallVerdict: result.Value.OverallVerdict,
            NextAction: result.Value.FlowResult.Summary.NextAction,
            EvidenceRefs: result.Value.FlowResult.EvidenceIds,
            FlowWarnings: result.Value.FlowResult.Warnings,
            OrchestratorWarnings: result.Value.Warnings,
            Metrics: result.Value.FlowResult.Metrics,
            PrimaryMetricName: result.Value.FlowResult.Summary.PrimaryMetricName,
            ScalingSignalNote: result.Value.FlowResult.Summary.ScalingSignalNote,
            Notes: result.Value.FlowResult.Summary.Notes);
    }

    private static async Task<NegativePairProbe> TrainLoadAndProbeAsync(
        string root,
        string caseId,
        TrainingContentVariant contentVariant,
        DesignConversionRuntimeFlowInput runtimeInput)
    {
        var machine = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(
                machine,
                BuildPass150Batch($"pass150-{caseId}", contentVariant));
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var path = Path.Combine(root, $"{caseId}-checkpoint.json");
        var save = await machine.SaveCheckpointAsync(path);
        Assert.True(save.IsSuccess, save.ErrorMessage);
        var loaded = await DesignConversionMachine.LoadCheckpointAsync(path);
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        var semanticFeature = Assert.Single(
            loaded.Value!.Checkpoint.CriticUnit.LearnedWeights ?? [],
            weight => string.Equals("source_kind:figma", weight.FeatureKey, StringComparison.Ordinal));
        var probe = await RunMachineScoreAsync(loaded.Value.Machine, runtimeInput, $"pass150-{caseId}-runtime");

        return new NegativePairProbe(probe.Score, semanticFeature.Weight);
    }

    private static LearningBatch<DesignConversionDomain.DesignConversionDomain> BuildPass150Batch(
        string batchId,
        double chosenScore,
        double rejectedScore)
        => new(
            BatchId: batchId,
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: $"{batchId}:critic",
                    Prompt: $"{{\"unit_id\":\"{DesignConversionDpoUnits.CriticUnit}\",\"usefulness_score\":0.5,\"internal_vector\":[0.2,0.4]}}",
                    ChosenResponse: $"{{\"visual_score\":{FormatScore(chosenScore)}}}",
                    RejectedResponse: $"{{\"visual_score\":{FormatScore(rejectedScore)}}}")
            ],
            SourceIds: ["pass150-shared-source-no-source-kind-feature"]);

    private static LearningBatch<DesignConversionDomain.DesignConversionDomain> BuildPass150Batch(
        string batchId,
        TrainingContentVariant contentVariant = TrainingContentVariant.Primary)
    {
        var content = contentVariant switch
        {
            TrainingContentVariant.SparseRejected => (
                Chosen: "{\"html\":\"<main><section><h1>Design</h1><p>Complete layout</p></section></main>\",\"css\":\"main{display:grid;gap:1rem}section{padding:1rem}\"}",
                Rejected: "{\"html\":\"<main></main>\",\"css\":\"main{display:block}\"}"),
            TrainingContentVariant.StructuredRejected => (
                Chosen: "{\"html\":\"<main><section><h1>Design</h1><p>Complete layout</p></section></main>\",\"css\":\"main{display:grid;gap:1rem}section{padding:1rem}\"}",
                Rejected: "{\"html\":\"<main><section><p>Incomplete layout</p></section></main>\",\"css\":\"main{display:flex}section{padding:0}\"}"),
            _ => (
                Chosen: "{\"html\":\"<main><article><h1>Converted design</h1></article></main>\",\"css\":\"main{display:grid}article{max-width:60rem}\"}",
                Rejected: "{\"html\":\"<main><article></article></main>\",\"css\":\"main{display:block}article{max-width:none}\"}")
        };
        return new(
            BatchId: batchId,
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: $"{batchId}:critic",
                    Prompt: $"{{\"unit_id\":\"{DesignConversionDpoUnits.CriticUnit}\",\"source_kind\":\"figma\",\"usefulness_score\":0.5,\"internal_vector\":[0.2,0.4]}}",
                    ChosenResponse: content.Chosen,
                    RejectedResponse: content.Rejected)
            ],
            SourceIds: ["pass150-shared-source-no-source-kind-feature"]);
    }

    private static string FormatScore(double value)
        => value.ToString("0.######", CultureInfo.InvariantCulture);

    private enum TrainingContentVariant
    {
        Primary,
        SparseRejected,
        StructuredRejected
    }

    private static string BuildHeldOutEvaluationPrompt(bool includeInternalVector)
    {
        const string heldOutSourceHash = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var vectorJson = includeInternalVector
            ? ",\"internal_vector\":[0.2,0.4]"
            : string.Empty;

        return $"{{\"unit_id\":\"{DesignConversionDpoUnits.CriticUnit}\",\"source_manifest_hash\":\"sha256:{heldOutSourceHash}\"{vectorJson}}}";
    }

    private static LearningEvaluationSet<DesignConversionDomain.DesignConversionDomain>
        BuildThreeOfFourHeldOutEvaluationSet(string evaluationId)
        => new(
            EvaluationId: evaluationId,
            DomainTag: DesignConversionMachine.DomainTag,
            EvaluationItems:
            [
                BuildHeldOutEvaluationItem($"{evaluationId}:match-1", includeInternalVector: true),
                BuildHeldOutEvaluationItem($"{evaluationId}:match-2", includeInternalVector: true),
                BuildHeldOutEvaluationItem($"{evaluationId}:match-3", includeInternalVector: true),
                BuildHeldOutEvaluationItem($"{evaluationId}:miss-1", includeInternalVector: false)
            ]);

    private static LearningEvaluationItem BuildHeldOutEvaluationItem(
        string itemId,
        bool includeInternalVector)
        => new(
            ItemId: itemId,
            Prompt: BuildHeldOutEvaluationPrompt(includeInternalVector),
            ExpectedResponse: "{\"visual_score\":0.95,\"passed\":true}");

    private static async Task<string> ReadFileSha256Async(string path)
        => Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(path))).ToLowerInvariant();

    private static string Sha256Bytes(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static async Task<DirectPolicyRuntimeFixture> SaveDirectPolicyRuntimeCheckpointAsync(
        string root,
        ModelConfig config,
        string runtimeCheckpointFileName,
        int weightSeed,
        bool directRuntimeKind,
        IReadOnlyList<string>? topLevelEvidenceRefs = null,
        double? accuracyAfterLastBatch = null,
        IReadOnlyList<string>? policyRefEvidenceRefs = null,
        string? providerKind = null,
        string? configKind = null,
        string? baseModelId = null,
        string? baseRevision = null,
        DesignConversionDpoPolicyCheckpointLoraContext? loraContext = null,
        Func<DesignConversionDpoPolicyCheckpointRef, DesignConversionDpoHeldOutEvaluation>?
            evaluatedHeldOutAcceptanceAuthorityFactory = null,
        bool emitEvaluatedRuntimeEncodingBinding = true)
    {
        Directory.CreateDirectory(root);
        var policyPath = Path.Combine(root, "dpo_policy_step_000001.bin");
        var model = new LlmModel(config);
        if (loraContext is not null)
            LoraApplicator.Apply(model, BuildLoraConfig(loraContext));
        FillDirectPolicyModelWeights(model, weightSeed);
        model.SaveCheckpoint(policyPath);

        return await SaveRuntimeCheckpointWithPolicyPayloadAsync(
            root,
            runtimeCheckpointFileName,
            await File.ReadAllBytesAsync(policyPath),
            config,
            directRuntimeKind
                ? DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind
                : DesignConversionMachine.DpoPolicyLegacySiblingPayloadRuntimeBridgeKind,
            topLevelEvidenceRefs,
            accuracyAfterLastBatch,
            policyRefEvidenceRefs,
            providerKind,
            configKind,
            baseModelId,
            baseRevision,
            loraContext,
            evaluatedHeldOutAcceptanceAuthorityFactory,
            emitEvaluatedRuntimeEncodingBinding);
    }

    private static async Task<DirectPolicyRuntimeFixture> SaveRuntimeCheckpointWithPolicyPayloadAsync(
        string root,
        string runtimeCheckpointFileName,
        byte[] policyPayload,
        ModelConfig config,
        string runtimeBridgeKind,
        IReadOnlyList<string>? topLevelEvidenceRefs = null,
        double? accuracyAfterLastBatch = null,
        IReadOnlyList<string>? policyRefEvidenceRefs = null,
        string? providerKind = null,
        string? configKind = null,
        string? baseModelId = null,
        string? baseRevision = null,
        DesignConversionDpoPolicyCheckpointLoraContext? loraContext = null,
        Func<DesignConversionDpoPolicyCheckpointRef, DesignConversionDpoHeldOutEvaluation>?
            evaluatedHeldOutAcceptanceAuthorityFactory = null,
        bool emitEvaluatedRuntimeEncodingBinding = true)
    {
        Directory.CreateDirectory(root);
        var policyFileName = "dpo_policy_step_000001.bin";
        var policyPath = Path.Combine(root, policyFileName);
        await File.WriteAllBytesAsync(policyPath, policyPayload);
        var policySha256 = Sha256Bytes(policyPayload);

        var checkpoint = new DesignConversionMachine().ExportCheckpoint();
        Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);

        var effectivePolicyRefEvidenceRefs = policyRefEvidenceRefs ?? [];
        if (evaluatedHeldOutAcceptanceAuthorityFactory is not null &&
            emitEvaluatedRuntimeEncodingBinding &&
            !effectivePolicyRefEvidenceRefs.Any(evidence => string.Equals(
                evidence,
                "DESIGN_CONVERSION_DPO_CONTINUATION_RUNTIME_ENCODING_BOUND",
                StringComparison.Ordinal)))
        {
            effectivePolicyRefEvidenceRefs = effectivePolicyRefEvidenceRefs
                .Concat(BuildValidContinuationRuntimeEncodingEvidence(
                    config,
                    evaluated: true))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }

        var policyRef = BuildDirectPolicyCheckpointRef(
            policyFileName,
            policySha256,
            policyPayload.Length,
            config,
            runtimeBridgeKind,
            effectivePolicyRefEvidenceRefs,
            providerKind,
            configKind,
            baseModelId,
            baseRevision,
            loraContext);
        if (evaluatedHeldOutAcceptanceAuthorityFactory is not null)
        {
            policyRef = policyRef with
            {
                EvaluatedHeldOutAcceptanceAuthority =
                    evaluatedHeldOutAcceptanceAuthorityFactory(policyRef)
            };
        }
        var checkpointEvidenceRefs = topLevelEvidenceRefs is null
            ? checkpoint.Value!.EvidenceRefs
            : checkpoint.Value!.EvidenceRefs
                .Where(evidence =>
                    !string.Equals(
                        evidence,
                        DesignConversionMachine.HeldOutAccuracyNotMeasuredEvidence,
                        StringComparison.Ordinal) &&
                    !string.Equals(
                        evidence,
                        DesignConversionMachine.LocalPreferenceFitSignalOnlyEvidence,
                        StringComparison.Ordinal))
                .Concat(topLevelEvidenceRefs)
                .ToArray();
        var runtimeCheckpoint = checkpoint.Value! with
        {
            DpoPolicyCheckpointRef = policyRef,
            AccuracyAfterLastBatch = accuracyAfterLastBatch ?? checkpoint.Value.AccuracyAfterLastBatch,
            EvidenceRefs = checkpointEvidenceRefs
        };
        var runtimeCheckpointPath = Path.Combine(root, runtimeCheckpointFileName);
        await File.WriteAllBytesAsync(
            runtimeCheckpointPath,
            JsonSerializer.SerializeToUtf8Bytes(
                runtimeCheckpoint,
                DesignConversionCheckpointJsonOptions.Standard));

        return new DirectPolicyRuntimeFixture(runtimeCheckpointPath, policyPath, policySha256, policyRef);
    }

    private static DesignConversionDpoPolicyCheckpointRef BuildDirectPolicyCheckpointRef(
        string policyFileName,
        string policySha256,
        int policyByteLength,
        ModelConfig config,
        string runtimeBridgeKind,
        IReadOnlyList<string>? additionalEvidenceRefs = null,
        string? providerKind = null,
        string? configKind = null,
        string? baseModelId = null,
        string? baseRevision = null,
        DesignConversionDpoPolicyCheckpointLoraContext? loraContext = null)
    {
        const int sourceRecordCount = 51;
        const int bridgePreferenceCount = 1;
        var resolvedProviderKind = providerKind ?? "direct-policy-fixture-provider";
        var resolvedConfigKind = configKind ?? "direct-policy-fixture-config";
        var resolvedBaseModelId = baseModelId ?? "direct-policy-fixture-model";
        var resolvedBaseRevision = baseRevision ?? "unit-test-v1";
        var isDirectRuntime = string.Equals(
            runtimeBridgeKind,
            DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind,
            StringComparison.Ordinal);
        var randomContext = isDirectRuntime
            ? DesignConversionDpoPolicyBaseTestProvider.CreateCurrentRandomContext()
            : null;
        var configSha256 = DesignConversionDpoPolicyBaseMetadata.ComputeConfigSha256(config);
        var initialWeightSha256 = DesignConversionDpoPolicyBaseMetadata.ComputeInitialWeightSha256(new LlmModel(config));
        var baseCheckpointSha256 = Sha256Bytes(JsonSerializer.SerializeToUtf8Bytes(new
        {
            provider = resolvedProviderKind,
            model = resolvedBaseModelId,
            revision = resolvedBaseRevision,
            configSha256
        }));
        if (string.Equals(baseCheckpointSha256, initialWeightSha256, StringComparison.Ordinal))
            baseCheckpointSha256 = Sha256Bytes(JsonSerializer.SerializeToUtf8Bytes(new { alternate = baseCheckpointSha256 }));

        var evidenceRefs = new[]
            {
                DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence,
                DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence,
                "DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_SOURCE_11_DPO",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PRODUCT_READY_FALSE",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PAYLOAD_FILE_PERSISTED",
                "11.DPO:DpoTrainer",
                "11.DPO:DpoTrainer.StepVector",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_INJECTED",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_VALIDATED",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_MODELS_DISTINCT",
                "DESIGN_CONVERSION_DPO_POLICY_INITIAL_POLICY_REFERENCE_BITWISE_EQUAL",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_AND_INITIAL_WEIGHT_HASHES_DISTINCT",
                $"dpo_policy_base_provider_kind:{resolvedProviderKind}",
                $"dpo_policy_base_configuration_kind:{resolvedConfigKind}",
                $"dpo_policy_base_model_id:{resolvedBaseModelId}",
                $"dpo_policy_base_revision:{resolvedBaseRevision}",
                $"dpo_policy_base_checkpoint_sha256:{baseCheckpointSha256}",
                $"dpo_policy_config_sha256:{configSha256}",
                $"dpo_policy_initial_weight_sha256:{initialWeightSha256}",
                $"dpo_policy_checkpoint_sha256:{policySha256}",
                $"dpo_policy_checkpoint_file:{policyFileName}",
                $"dpo_policy_bridge_source_record_count:{sourceRecordCount.ToString(CultureInfo.InvariantCulture)}",
                $"dpo_policy_bridge_preference_count:{bridgePreferenceCount.ToString(CultureInfo.InvariantCulture)}",
                "dpo_policy_step_count:1",
                "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_TYPED",
                "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_NO_ABSOLUTE_PATH",
                "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_PAYLOAD_FILE_SIBLING"
            }
            .Concat(isDirectRuntime
                ? [DesignConversionMachine.DpoPolicyDirectInferenceConfiguredEvidence]
                : [])
            .Concat(isDirectRuntime
                ? DesignConversionDpoPolicyRandomContextContract
                    .BuildRequiredEvidenceRefs(randomContext!)
                : [])
            .Concat(loraContext is null
                ? []
                : [
                    DesignConversionMachine.DpoPolicyLoraAppliedEvidence,
                    "10.LoRA:LoraApplicator",
                    $"dpo_policy_lora_rank:{loraContext.Rank.ToString(CultureInfo.InvariantCulture)}",
                    $"dpo_policy_lora_alpha:{loraContext.Alpha.ToString("0.######", CultureInfo.InvariantCulture)}",
                    $"dpo_policy_lora_dropout:{loraContext.Dropout.ToString("0.######", CultureInfo.InvariantCulture)}",
                    $"dpo_policy_lora_target_modules:{string.Join(",", loraContext.TargetModules)}",
                    $"dpo_policy_lora_adapter_count:{loraContext.AdapterCount.ToString(CultureInfo.InvariantCulture)}"
                ])
            .Concat(additionalEvidenceRefs ?? [])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new DesignConversionDpoPolicyCheckpointRef(
            ArtifactKind: "11.DPO:DpoTrainer.policy_checkpoint",
            PolicyCheckpointFileName: policyFileName,
            Sha256: policySha256,
            SourceSystem: "11.DPO:DpoTrainer",
            SourceMechanism: "11.DPO:DpoTrainer.StepVector:fresh_loaded",
            RuntimeBridgeKind: runtimeBridgeKind,
            FreshLoadVerified: true,
            ConvertedToRuntimeWeights: true,
            SourceRecordCount: sourceRecordCount,
            BridgePreferenceCount: bridgePreferenceCount,
            DpoStepCount: 1,
            TrainingContext: new DesignConversionDpoPolicyCheckpointTrainingContext(
                ConfigKind: resolvedConfigKind,
                VocabSize: config.VocabSize,
                ContextLength: config.ContextLen,
                HiddenDim: config.HiddenDim,
                NumLayers: config.NumLayers,
                NumHeads: config.NumHeads,
                NumKvHeads: config.NumKvHeads,
                FfnHiddenDim: config.FfnHiddenDim,
                RawPayloadPersisted: true,
                AbsolutePathPersisted: false,
                SchemaVersion: isDirectRuntime ? 4 : 2,
                RopeTheta: config.RopeTheta,
                RmsNormEps: config.RmsNormEps,
                ProviderKind: resolvedProviderKind,
                BaseModelId: resolvedBaseModelId,
                BaseRevision: resolvedBaseRevision,
                BaseCheckpointSha256: baseCheckpointSha256,
                ConfigSha256: configSha256,
                InitialWeightSha256: initialWeightSha256,
                Lora: loraContext) with
            {
                RandomContext = randomContext,
                LegalTeacherAdmissionCommitment = isDirectRuntime
                    ? DesignConversionDpoLegalTeacherAdmissionCommitmentTestFactory.Create(evidenceRefs)
                    : null
            },
            EvidenceRefs: evidenceRefs,
            PolicyCheckpointByteLength: policyByteLength);
    }

    [Fact]
    public async Task LoadCheckpointAsync_PreviousV2EvaluationRemainsLoadableEvidenceOnly()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            $"direct-dpo-policy-v2-evidence-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            const double checkpointAccuracy = 1d;
            var config = BuildDirectPolicyFixtureConfig();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(
                comparisonScore: 0.40,
                comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "direct-dpo-policy-v2-evidence-baseline");
            var runtime = await SaveDirectPolicyRuntimeCheckpointAsync(
                root,
                config,
                "direct-dpo-policy-v2-evidence-runtime.json",
                weightSeed: 47,
                directRuntimeKind: true,
                topLevelEvidenceRefs: BuildEvaluatedRuntimeCheckpointEvidence(checkpointAccuracy),
                accuracyAfterLastBatch: checkpointAccuracy,
                evaluatedHeldOutAcceptanceAuthorityFactory: policyRef =>
                    BuildTypedDirectPolicyHeldOutAcceptanceAuthority(policyRef, checkpointAccuracy) with
                    {
                        Schema = DesignConversionDpoPolicyCheckpointBridge.PreviousEvaluatedSchema,
                        G3Metrics = null
                    });

            var load = await DesignConversionMachine.LoadCheckpointWithEvaluationOptionsAsync(
                runtime.RuntimeCheckpointPath,
                new DesignConversionMachineEvaluationOptions(0d));

            Assert.True(load.IsSuccess, load.ErrorMessage);
            var probe = await RunMachineScoreAsync(
                load.Value!.Machine,
                runtimeInput,
                "direct-dpo-policy-v2-evidence-probe");
            AssertDirectPolicyConfiguredPairUnavailableProbe(probe, baselineProbe, root);
            Assert.DoesNotContain(
                DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityEvidence,
                probe.EvidenceRefs);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void DirectDpoG3AuthorityPreview_CycleOneCannotActivateEvenWithPerfectMetrics()
    {
        var policyRef = BuildDirectPolicyCheckpointRef(
            "dpo_policy_step_000001.bin",
            new string('9', 64),
            policyByteLength: 1,
            config: BuildDirectPolicyFixtureConfig(),
            runtimeBridgeKind: DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind);
        policyRef = policyRef with
        {
            EvaluatedHeldOutAcceptanceAuthority =
                BuildTypedDirectPolicyHeldOutAcceptanceAuthority(policyRef, 1d)
        };

        var actual = DesignConversionMachine
            .BuildDirectDpoG3AcceptanceAuthorityPreviewForTests(policyRef, 1d);

        Assert.False(actual.Active);
        Assert.Equal(
            DesignConversionG3AcceptancePolicy.ContinuedTrainingMissingReason,
            actual.Reason);
    }

    [Fact]
    public void DirectDpoG3AuthorityPreview_ValidImmediateParentContinuationActivates()
    {
        var policyRef = BuildDirectPolicyCheckpointRef(
            "dpo_policy_step_000001.bin",
            new string('8', 64),
            policyByteLength: 1,
            config: BuildDirectPolicyFixtureConfig(),
            runtimeBridgeKind: DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind);
        policyRef = policyRef with
        {
            EvaluatedHeldOutAcceptanceAuthority =
                BuildTypedDirectPolicyHeldOutAcceptanceAuthority(
                    policyRef,
                    accuracy: 1d,
                    continuedTraining: true)
        };

        var permissive = DesignConversionMachine
            .BuildDirectDpoG3AcceptanceAuthorityPreviewForTests(
                policyRef,
                checkpointAccuracyAfterLastBatch: 1d,
                configuredRuntimeScoreThreshold: 0d);
        var strict = DesignConversionMachine
            .BuildDirectDpoG3AcceptanceAuthorityPreviewForTests(
                policyRef,
                checkpointAccuracyAfterLastBatch: 1d,
                configuredRuntimeScoreThreshold: 1d);

        Assert.True(permissive.Active);
        Assert.Equal(DesignConversionG3AcceptancePolicy.EligibleReason, permissive.Reason);
        Assert.Equal(permissive, strict);
    }

    [Fact]
    public void DirectDpoG3AuthorityPreview_PreviousSchemaLoadsAsEvidenceOnly()
    {
        var policyRef = BuildDirectPolicyCheckpointRef(
            "dpo_policy_step_000001.bin",
            new string('7', 64),
            policyByteLength: 1,
            config: BuildDirectPolicyFixtureConfig(),
            runtimeBridgeKind: DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind);
        var previous = BuildTypedDirectPolicyHeldOutAcceptanceAuthority(
            policyRef,
            accuracy: 1d,
            continuedTraining: true) with
        {
            Schema = DesignConversionDpoPolicyCheckpointBridge.PreviousEvaluatedSchema,
            G3Metrics = null
        };
        policyRef = policyRef with { EvaluatedHeldOutAcceptanceAuthority = previous };

        var actual = DesignConversionMachine
            .BuildDirectDpoG3AcceptanceAuthorityPreviewForTests(policyRef, 1d);

        Assert.False(actual.Active);
        Assert.Equal("typed_evaluated_heldout_authority_snapshot_invalid", actual.Reason);
    }

    [Fact]
    public void DirectDpoG3AuthorityPreview_MissingFreshLoadCannotActivate()
    {
        var policyRef = BuildDirectPolicyCheckpointRef(
            "dpo_policy_step_000001.bin",
            new string('6', 64),
            policyByteLength: 1,
            config: BuildDirectPolicyFixtureConfig(),
            runtimeBridgeKind: DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind);
        policyRef = policyRef with
        {
            EvaluatedHeldOutAcceptanceAuthority =
                BuildTypedDirectPolicyHeldOutAcceptanceAuthority(
                    policyRef,
                    accuracy: 1d,
                    continuedTraining: true) with
                {
                    FreshLoadVerified = false
                }
        };

        var actual = DesignConversionMachine
            .BuildDirectDpoG3AcceptanceAuthorityPreviewForTests(policyRef, 1d);

        Assert.False(actual.Active);
        Assert.Equal(DesignConversionG3AcceptancePolicy.FreshLoadMissingReason, actual.Reason);
    }

    [Theory]
    [InlineData("input_hash")]
    [InlineData("child_binding")]
    [InlineData("parent_binding")]
    [InlineData("heldout_count")]
    [InlineData("baseline_count")]
    [InlineData("baseline_rate")]
    [InlineData("absolute_count")]
    [InlineData("absolute_rate")]
    [InlineData("passport_scorer_id")]
    [InlineData("passport_scorer_version")]
    [InlineData("passport_normalization_profile")]
    [InlineData("passport_metric_profile")]
    [InlineData("lineage_parent")]
    public void DirectDpoG3AuthorityPreview_TamperedMetricsOrLineageCannotActivate(
        string mutation)
    {
        var policyRef = BuildDirectPolicyCheckpointRef(
            "dpo_policy_step_000001.bin",
            new string('5', 64),
            policyByteLength: 1,
            config: BuildDirectPolicyFixtureConfig(),
            runtimeBridgeKind: DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind);
        var evaluation = BuildTypedDirectPolicyHeldOutAcceptanceAuthority(
            policyRef,
            accuracy: 1d,
            continuedTraining: true);
        var metrics = evaluation.G3Metrics!;
        evaluation = mutation switch
        {
            "input_hash" => evaluation with
            {
                HeldOutEvaluationInputSha256 = evaluation.SelectedTrainInputSha256
            },
            "child_binding" => evaluation with
            {
                ChildPolicyCheckpointSha256 = new string('4', 64)
            },
            "parent_binding" => evaluation with
            {
                ReferencePolicyCheckpointSha256 = new string('3', 64)
            },
            "heldout_count" => evaluation with
            {
                G3Metrics = metrics with { HeldOutCount = metrics.HeldOutCount + 1 }
            },
            "baseline_count" => evaluation with
            {
                G3Metrics = metrics with { BaselineWinCount = metrics.BaselineWinCount - 1 }
            },
            "baseline_rate" => evaluation with
            {
                G3Metrics = metrics with { BaselineWinRate = 0.99d }
            },
            "absolute_count" => evaluation with
            {
                G3Metrics = metrics with
                {
                    AbsoluteChildPreferenceCorrectCount =
                        metrics.AbsoluteChildPreferenceCorrectCount - 1
                }
            },
            "absolute_rate" => evaluation with
            {
                G3Metrics = metrics with { AbsoluteChildPreferenceAccuracy = 0.99d }
            },
            "passport_scorer_id" => evaluation with
            {
                MeasurementNoisePassport = BuildDpoMeasurementNoisePassport(
                    evaluation,
                    DesignConversionDpoPolicyCheckpointBridge
                        .EvaluatedMeasurementNoiseScorerIdentity with
                    {
                        ScorerId = "foreign-direct-g3-scorer"
                    }),
                MeasurementNoiseStatus = MeasurementNoisePolicy.ImprovedStatus
            },
            "passport_scorer_version" => evaluation with
            {
                MeasurementNoisePassport = BuildDpoMeasurementNoisePassport(
                    evaluation,
                    DesignConversionDpoPolicyCheckpointBridge
                        .EvaluatedMeasurementNoiseScorerIdentity with
                    {
                        ScorerVersion =
                            DesignConversionDpoPolicyCheckpointBridge
                                .PreviousEvaluatedSchema
                    }),
                MeasurementNoiseStatus = MeasurementNoisePolicy.ImprovedStatus
            },
            "passport_normalization_profile" => evaluation with
            {
                MeasurementNoisePassport = BuildDpoMeasurementNoisePassport(
                    evaluation,
                    DesignConversionDpoPolicyCheckpointBridge
                        .EvaluatedMeasurementNoiseScorerIdentity with
                    {
                        NormalizationProfileId = "foreign-packing-v2"
                    }),
                MeasurementNoiseStatus = MeasurementNoisePolicy.ImprovedStatus
            },
            "passport_metric_profile" => evaluation with
            {
                MeasurementNoisePassport = BuildDpoMeasurementNoisePassport(
                    evaluation,
                    DesignConversionDpoPolicyCheckpointBridge
                        .EvaluatedMeasurementNoiseScorerIdentity with
                    {
                        MetricProfileId = "foreign-baseline-win-rate"
                    }),
                MeasurementNoiseStatus = MeasurementNoisePolicy.ImprovedStatus
            },
            "lineage_parent" => evaluation with
            {
                CycleAccuracyLineage = evaluation.CycleAccuracyLineage! with
                {
                    ParentPolicyCheckpointSha256 = new string('4', 64)
                }
            },
            _ => throw new InvalidOperationException(mutation)
        };
        policyRef = policyRef with { EvaluatedHeldOutAcceptanceAuthority = evaluation };

        var actual = DesignConversionMachine
            .BuildDirectDpoG3AcceptanceAuthorityPreviewForTests(policyRef, 1d);

        Assert.False(actual.Active);
    }

    private static MeasurementNoisePassport BuildDpoMeasurementNoisePassport(
        DesignConversionDpoHeldOutEvaluation evaluation,
        HeldOutScorerIdentity scorerIdentity)
        => MeasurementNoisePolicy.BuildPassport(
            scorerIdentity,
            evaluation.DatasetId,
            evaluation.EvaluatedHeldOutRecordCount,
            Enumerable.Repeat(
                evaluation.Accuracy,
                MeasurementNoisePolicyConfig.DefaultRepeatCount).ToArray(),
            MeasurementNoisePolicyConfig.Canonical,
            DateTimeOffset.UnixEpoch);

    private static DesignConversionDpoHeldOutEvaluation BuildTypedDirectPolicyHeldOutAcceptanceAuthority(
        DesignConversionDpoPolicyCheckpointRef policyRef,
        double accuracy,
        bool continuedTraining = false,
        int? absoluteCorrectCount = null)
    {
        const int heldOutRecordCount = 50;
        var correctCount = checked((int)Math.Round(accuracy * heldOutRecordCount));
        Assert.Equal(accuracy, (double)correctCount / heldOutRecordCount);
        var resolvedAbsoluteCorrectCount = absoluteCorrectCount ?? heldOutRecordCount;
        Assert.InRange(resolvedAbsoluteCorrectCount, 0, heldOutRecordCount);
        var context = policyRef.TrainingContext;
        return new DesignConversionDpoHeldOutEvaluation(
            Schema: DesignConversionDpoPolicyCheckpointBridge.EvaluatedSchema,
            PackingId: DesignConversionDpoPolicyCheckpointBridge.EvaluatedPackingId,
            VocabSize: context.VocabSize,
            ContextLength: context.ContextLength,
            DpoStepCount: policyRef.DpoStepCount,
            MaxBridgePreferenceCount: policyRef.BridgePreferenceCount,
            ChildPolicyCheckpointSha256: policyRef.Sha256,
            ReferencePolicyCheckpointSha256: context.BaseCheckpointSha256!,
            ReferenceMode: continuedTraining ? "immediate_parent" : "provider_fresh_step_zero",
            DatasetId: "direct-policy-fixture-heldout",
            PartitionSnapshotSha256: new string('b', 64),
            TrainRecordSetSha256: new string('c', 64),
            HeldOutRecordSetSha256: new string('d', 64),
            TrainSourceSetSha256: new string('e', 64),
            HeldOutSourceSetSha256: new string('f', 64),
            SelectedTrainInputSha256: new string('1', 64),
            HeldOutEvaluationInputSha256: new string('a', 64),
            TrainRecordCount: policyRef.SourceRecordCount - heldOutRecordCount,
            HeldOutRecordCount: heldOutRecordCount,
            EvaluatedHeldOutRecordCount: heldOutRecordCount,
            CorrectCount: correctCount,
            Accuracy: accuracy,
            MeanRelativeMargin: 0.5,
            MeanChildMargin: 0.75,
            MeanReferenceMargin: 0.25,
            FreshLoadVerified: true,
            ReferenceFrozen: true,
            SourceDisjoint: true,
            HeldOutNotTrained: true)
        {
            G3Metrics = new DesignConversionG3AcceptanceMetrics(
                HeldOutCount: heldOutRecordCount,
                BaselineWinCount: correctCount,
                BaselineWinRate: accuracy,
                AbsoluteChildPreferenceCorrectCount: resolvedAbsoluteCorrectCount,
                AbsoluteChildPreferenceAccuracy:
                    (double)resolvedAbsoluteCorrectCount / heldOutRecordCount)
            {
                MeasurementNoiseScorerIdentity =
                    DesignConversionDpoPolicyCheckpointBridge
                        .EvaluatedMeasurementNoiseScorerIdentity,
                MeasurementNoiseEvaluationSetId = "direct-policy-fixture-heldout"
            },
            CycleAccuracyLineage = new DesignConversionDpoHeldOutCycleAccuracyLineage(
                CycleOrdinal: continuedTraining ? 2 : 1,
                CurrentMeasuredAccuracy: accuracy,
                HasParentMeasurement: continuedTraining,
                ParentCycleOrdinal: continuedTraining ? 1 : null,
                ParentMeasuredAccuracy: continuedTraining ? accuracy : null,
                ParentPolicyCheckpointSha256:
                    continuedTraining ? context.BaseCheckpointSha256 : null,
                ParentDatasetId: continuedTraining ? "direct-policy-fixture-heldout" : null,
                ParentHeldOutRecordSetSha256: continuedTraining ? new string('d', 64) : null,
                ParentHeldOutSourceSetSha256: continuedTraining ? new string('f', 64) : null,
                ParentHeldOutEvaluationInputSha256: continuedTraining ? new string('a', 64) : null,
                ComparableHeldOut: continuedTraining,
                AccuracyDelta: continuedTraining ? 0d : null)
        };
    }

    private static string[] BuildEvaluatedRuntimeCheckpointEvidence(double accuracy)
        =>
        [
            DesignConversionMachine.DpoRuntimeCheckpointAccuracyFromHeldOutEvaluationEvidence,
            DesignConversionMachine.DpoRuntimeCheckpointAccuracyCurrentCycleHeldOutEvaluationScopeEvidence,
            $"dpo_runtime_checkpoint_accuracy_value:{accuracy.ToString("R", CultureInfo.InvariantCulture)}",
            "dpo_runtime_checkpoint_accuracy_evaluated_heldout_count:50",
            $"dpo_runtime_checkpoint_accuracy_input_sha256:{new string('a', 64)}"
        ];

    private static DesignConversionDpoRuntimeInferenceEncodingBinding
        BuildValidContinuationRuntimeInferenceBinding(
            ModelConfig config,
            bool evaluated = true)
    {
        var binding = DesignConversionDpoPolicyCheckpointBridge
            .BuildRuntimeInferenceEncodingBindingPreviewForTests(
                config,
                evaluated);
        Assert.True(binding.IsSuccess, binding.ErrorMessage);
        return binding.Value!;
    }

    private static string[] BuildValidContinuationRuntimeEncodingEvidence(
        ModelConfig config,
        bool evaluated = true)
        => BuildContinuationRuntimeEncodingEvidence(
            BuildValidContinuationRuntimeInferenceBinding(
                config,
                evaluated));

    private static string[] BuildContinuationRuntimeEncodingEvidence(
        DesignConversionDpoRuntimeInferenceEncodingBinding binding)
        =>
        [
            "DESIGN_CONVERSION_DPO_CONTINUATION_RUNTIME_ENCODING_BOUND",
            $"dpo_runtime_encoding_schema:{binding.BindingSchema}",
            $"dpo_runtime_encoding_source_descriptor_schema:{binding.SourceDescriptorSchema}",
            $"dpo_runtime_encoding_token_mapping:{binding.PolicyTokenMapping}",
            $"dpo_runtime_encoding_token_id_min:{binding.TokenIdMinimum.ToString(CultureInfo.InvariantCulture)}",
            $"dpo_runtime_encoding_token_id_max:{binding.TokenIdMaximum.ToString(CultureInfo.InvariantCulture)}",
            $"dpo_runtime_encoding_context_length:{binding.ContextLength.ToString(CultureInfo.InvariantCulture)}",
            $"dpo_runtime_encoding_tokenizer_sha256:{binding.TokenizerArtifactSha256}",
            $"dpo_runtime_encoding_vocabulary_sha256:{binding.TokenizerVocabularyIdentitySha256}"
        ];

    private static string MutateCanonicalSha256(string value)
        => $"{(value[0] == '0' ? '1' : '0')}{value[1..]}";

    private static string[] BuildSpoofedDirectPolicyAuthorityEvidence()
        =>
        [
            DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityEvidence,
            DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityScoreEvidence,
            DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityConfiguredEvidence,
            "direct-dpo-policy-acceptance-authority:spoofed_direct_policy_score_configured_threshold",
            "direct-dpo-policy-acceptance-authority-reason:spoofed_active_checkpoint",
            "DESIGN_CONVERSION_RUNTIME_DPO_POLICY_DIRECT_INFERENCE_SCORE_APPLIED_SPOOFED_BASELINE_ACCEPTANCE",
            "DESIGN_CONVERSION_DPO_POLICY_DIRECT_RUNTIME_INFERENCE_CONFIGURED_SPOOFED_NOT_G3_ACCEPTANCE",
            "baseline_visual_verdict_until_G3_spoofed_checkpoint_authority"
        ];

    private static string[] BuildHostileDirectPolicyRuntimeEvidenceRefs()
        =>
        [
            "color:red",
            "display:none",
            "type:FRAME",
            "<main class=\"shell\">raw html</main>",
            ".card{color:red}",
            "{\"type\":\"FRAME\",\"children\":[]}",
            @"C:\Projects\llm_mvp_core\secret.html",
            "/tmp/llm_mvp_core/secret.css",
            @"\\server\share\secret.json"
        ];

    public static IEnumerable<object[]> MalformedDirectDpoPolicyAuthorityEvidenceCases()
    {
        const double accuracy = 1.0;
        var valid = BuildEvaluatedRuntimeCheckpointEvidence(accuracy);
        const string accuracyPrefix = "dpo_runtime_checkpoint_accuracy_value:";
        const string countPrefix = "dpo_runtime_checkpoint_accuracy_evaluated_heldout_count:";
        const string hashPrefix = "dpo_runtime_checkpoint_accuracy_input_sha256:";
        var accuracyMarker = $"{accuracyPrefix}{accuracy.ToString("R", CultureInfo.InvariantCulture)}";
        const string countMarker = $"{countPrefix}2";
        var hashMarker = $"{hashPrefix}{new string('a', 64)}";

        yield return MalformedEvidenceCase(
            "missing-heldout-source-scope",
            valid.Where(evidence => !string.Equals(
                evidence,
                DesignConversionMachine.DpoRuntimeCheckpointAccuracyFromHeldOutEvaluationEvidence,
                StringComparison.Ordinal)).ToArray());
        yield return MalformedEvidenceCase(
            "duplicate-heldout-source-scope",
            valid.Concat([DesignConversionMachine.DpoRuntimeCheckpointAccuracyFromHeldOutEvaluationEvidence]).ToArray());
        yield return MalformedEvidenceCase(
            "malformed-heldout-source-scope",
            valid.Select(evidence => string.Equals(
                    evidence,
                    DesignConversionMachine.DpoRuntimeCheckpointAccuracyFromHeldOutEvaluationEvidence,
                    StringComparison.Ordinal)
                ? evidence.ToLowerInvariant()
                : evidence).ToArray());
        yield return MalformedEvidenceCase(
            "missing-current-cycle-scope",
            valid.Where(evidence => !string.Equals(
                evidence,
                DesignConversionMachine.DpoRuntimeCheckpointAccuracyCurrentCycleHeldOutEvaluationScopeEvidence,
                StringComparison.Ordinal)).ToArray());
        yield return MalformedEvidenceCase(
            "duplicate-current-cycle-scope",
            valid.Concat([DesignConversionMachine.DpoRuntimeCheckpointAccuracyCurrentCycleHeldOutEvaluationScopeEvidence]).ToArray());
        yield return MalformedEvidenceCase(
            "generic-evaluate-scope",
            valid
                .Where(evidence =>
                    !string.Equals(evidence, DesignConversionMachine.DpoRuntimeCheckpointAccuracyFromHeldOutEvaluationEvidence, StringComparison.Ordinal) &&
                    !string.Equals(evidence, DesignConversionMachine.DpoRuntimeCheckpointAccuracyCurrentCycleHeldOutEvaluationScopeEvidence, StringComparison.Ordinal))
                .Concat([
                    DesignConversionMachine.EvaluateAsyncHeldOutAccuracyMeasuredEvidence,
                    DesignConversionMachine.EvaluateAsyncSanitizedLocalCheckpointFeatureFitEvidence
                ])
                .ToArray());
        yield return MalformedEvidenceCase(
            "missing-accuracy",
            valid.Where(evidence => !evidence.StartsWith(accuracyPrefix, StringComparison.Ordinal)).ToArray());
        yield return MalformedEvidenceCase(
            "duplicate-accuracy",
            valid.Concat([accuracyMarker]).ToArray());
        yield return MalformedEvidenceCase(
            "malformed-accuracy",
            ReplacePrefix(valid, accuracyPrefix, $"{accuracyPrefix}not-a-number"));
        yield return MalformedEvidenceCase(
            "mismatched-accuracy",
            ReplacePrefix(valid, accuracyPrefix, $"{accuracyPrefix}0.5"));
        yield return MalformedEvidenceCase(
            "missing-count",
            valid.Where(evidence => !evidence.StartsWith(countPrefix, StringComparison.Ordinal)).ToArray());
        yield return MalformedEvidenceCase(
            "duplicate-count",
            valid.Concat([countMarker]).ToArray());
        yield return MalformedEvidenceCase(
            "malformed-count",
            ReplacePrefix(valid, countPrefix, $"{countPrefix}two"));
        yield return MalformedEvidenceCase(
            "zero-count",
            ReplacePrefix(valid, countPrefix, $"{countPrefix}0"));
        yield return MalformedEvidenceCase(
            "negative-count",
            ReplacePrefix(valid, countPrefix, $"{countPrefix}-1"));
        yield return MalformedEvidenceCase(
            "missing-hash",
            valid.Where(evidence => !evidence.StartsWith(hashPrefix, StringComparison.Ordinal)).ToArray());
        yield return MalformedEvidenceCase(
            "duplicate-hash",
            valid.Concat([hashMarker]).ToArray());
        yield return MalformedEvidenceCase(
            "uppercase-hash",
            ReplacePrefix(valid, hashPrefix, $"{hashPrefix}{new string('A', 64)}"));
        yield return MalformedEvidenceCase(
            "short-hash",
            ReplacePrefix(valid, hashPrefix, $"{hashPrefix}{new string('a', 63)}"));
    }

    private static object[] MalformedEvidenceCase(string caseId, string[] evidenceRefs)
        => [caseId, evidenceRefs];

    private static string[] ReplacePrefix(string[] evidenceRefs, string prefix, string replacement)
        => evidenceRefs
            .Select(evidence => evidence.StartsWith(prefix, StringComparison.Ordinal)
                ? replacement
                : evidence)
            .ToArray();

    private static string BuildRuntimePublicSurface(RuntimeScoreProbe probe)
        => string.Join(
            "\n",
            probe.EvidenceRefs.Concat([
                JsonSerializer.Serialize(probe.Metrics),
                string.Join("|", probe.FlowWarnings),
                string.Join("|", probe.OrchestratorWarnings),
                probe.PrimaryMetricName,
                probe.ScalingSignalNote,
                probe.Notes,
                probe.NextAction,
                probe.OverallVerdict
            ]));

    private static void AssertPublicSurfaceDoesNotContain(
        RuntimeScoreProbe probe,
        IReadOnlyList<string> forbiddenValues)
    {
        var publicSurface = BuildRuntimePublicSurface(probe);
        foreach (var forbiddenValue in forbiddenValues)
            Assert.DoesNotContain(forbiddenValue, publicSurface, StringComparison.Ordinal);
    }

    private static void AssertRequiredDirectPolicyProvenanceEvidence(
        RuntimeScoreProbe probe,
        DesignConversionDpoPolicyCheckpointRef policyRef)
    {
        AssertCurrentDirectPolicyRandomContext(policyRef);
        var context = policyRef.TrainingContext;
        var expectedEvidenceRefs = new[]
        {
            DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence,
            DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence,
            "DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_SOURCE_11_DPO",
            "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PRODUCT_READY_FALSE",
            "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PAYLOAD_FILE_PERSISTED",
            "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_PAYLOAD_FILE_SIBLING",
            DesignConversionMachine.DpoPolicyDirectInferenceConfiguredEvidence,
            "11.DPO:DpoTrainer",
            "11.DPO:DpoTrainer.StepVector",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_INJECTED",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_VALIDATED",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_MODELS_DISTINCT",
            "DESIGN_CONVERSION_DPO_POLICY_INITIAL_POLICY_REFERENCE_BITWISE_EQUAL",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_AND_INITIAL_WEIGHT_HASHES_DISTINCT",
            $"dpo_policy_base_provider_kind:{context.ProviderKind}",
            $"dpo_policy_base_configuration_kind:{context.ConfigKind}",
            $"dpo_policy_base_model_id:{context.BaseModelId}",
            $"dpo_policy_base_revision:{context.BaseRevision}",
            $"dpo_policy_base_checkpoint_sha256:{context.BaseCheckpointSha256}",
            $"dpo_policy_config_sha256:{context.ConfigSha256}",
            $"dpo_policy_initial_weight_sha256:{context.InitialWeightSha256}",
            $"dpo_policy_checkpoint_sha256:{policyRef.Sha256}",
            $"dpo_policy_checkpoint_file:{policyRef.PolicyCheckpointFileName}",
            $"dpo_policy_bridge_source_record_count:{policyRef.SourceRecordCount.ToString(CultureInfo.InvariantCulture)}",
            $"dpo_policy_bridge_preference_count:{policyRef.BridgePreferenceCount.ToString(CultureInfo.InvariantCulture)}",
            $"dpo_policy_step_count:{policyRef.DpoStepCount.ToString(CultureInfo.InvariantCulture)}",
            "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_TYPED",
            "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_NO_ABSOLUTE_PATH"
        };

        foreach (var expectedEvidenceRef in expectedEvidenceRefs)
            Assert.Contains(expectedEvidenceRef, probe.EvidenceRefs);
        foreach (var expectedEvidenceRef in
                 DesignConversionDpoPolicyRandomContextContract.BuildRequiredEvidenceRefs(
                     context.RandomContext!))
        {
            Assert.Contains(expectedEvidenceRef, probe.EvidenceRefs);
        }
    }

    private static void AssertCurrentDirectPolicyRandomContext(
        DesignConversionDpoPolicyCheckpointRef? policyRef)
    {
        var value = Assert.IsType<DesignConversionDpoPolicyCheckpointRef>(policyRef);
        var expectedRandomContext = DesignConversionDpoPolicyBaseTestProvider
            .CreateCurrentRandomContext();
        Assert.Equal(4, value.TrainingContext.SchemaVersion);
        Assert.Equal(expectedRandomContext, value.TrainingContext.RandomContext);
        Assert.True(DesignConversionDpoPolicyRandomContextContract.HasRequiredEvidenceRefs(
            expectedRandomContext,
            value.EvidenceRefs));
        Assert.Contains(
            $"{DesignConversionDpoPolicyRandomContextContract.RootInitializationSeedEvidencePrefix}" +
            expectedRandomContext.RootInitializationSeed.ToString(CultureInfo.InvariantCulture),
            value.EvidenceRefs);
        Assert.All(
            DesignConversionDpoPolicyRandomContextContract.BuildRequiredEvidenceRefs(
                expectedRandomContext),
            required => Assert.Contains(required, value.EvidenceRefs));
    }

    private static DesignConversionDpoPolicyCheckpointRef WithCommittedPolicyEvidence(
        DesignConversionDpoPolicyCheckpointRef policyRef,
        IEnumerable<string> evidenceRefs)
    {
        var snapshot = evidenceRefs.ToArray();
        return policyRef with
        {
            TrainingContext = policyRef.TrainingContext with
            {
                LegalTeacherAdmissionCommitment =
                    DesignConversionDpoLegalTeacherAdmissionCommitmentTestFactory.Create(snapshot)
            },
            EvidenceRefs = snapshot
        };
    }

    private static void AssertRequiredDirectPolicyLoraEvidence(
        RuntimeScoreProbe probe,
        DesignConversionDpoPolicyCheckpointLoraContext lora)
    {
        var expectedEvidenceRefs = new[]
        {
            DesignConversionMachine.DpoPolicyLoraAppliedEvidence,
            "10.LoRA:LoraApplicator",
            $"dpo_policy_lora_rank:{lora.Rank.ToString(CultureInfo.InvariantCulture)}",
            $"dpo_policy_lora_alpha:{lora.Alpha.ToString("0.######", CultureInfo.InvariantCulture)}",
            $"dpo_policy_lora_dropout:{lora.Dropout.ToString("0.######", CultureInfo.InvariantCulture)}",
            $"dpo_policy_lora_target_modules:{string.Join(",", lora.TargetModules)}",
            $"dpo_policy_lora_adapter_count:{lora.AdapterCount.ToString(CultureInfo.InvariantCulture)}"
        };

        foreach (var expectedEvidenceRef in expectedEvidenceRefs)
            Assert.Contains(expectedEvidenceRef, probe.EvidenceRefs);
    }

    private static void AssertDirectPolicyConfiguredPairUnavailableProbe(
        RuntimeScoreProbe probe,
        RuntimeScoreProbe baseline,
        string forbiddenPathFragment)
    {
        Assert.Equal(baseline.Score, probe.Score);
        Assert.Equal(baseline.Passed, probe.Passed);
        Assert.Equal(baseline.OverallVerdict, probe.OverallVerdict);
        Assert.Equal(baseline.NextAction, probe.NextAction);
        Assert.Equal(baseline.PrimaryMetricName, probe.PrimaryMetricName);
        Assert.Equal(baseline.ScalingSignalNote, probe.ScalingSignalNote);
        Assert.Equal(baseline.Notes, probe.Notes);

        Assert.Contains(DesignConversionMachine.DpoPolicyDirectInferenceConfiguredEvidence, probe.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoPolicyRuntimePairContentUnavailableEvidence, probe.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoPolicyDirectInferenceNoRawPayloadEvidence, probe.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, probe.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, probe.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.DpoPolicyDirectInferenceScoreEvidence, probe.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityScoreEvidence, probe.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityConfiguredEvidence, probe.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityEvidence, probe.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.DpoTrainerPolicyCheckpointRuntimeScoringEvidence, probe.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.LearnedInferenceAppliedEvidence, probe.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.DpoPolicyContinuationRuntimeEncodingAppliedEvidence, probe.EvidenceRefs);
        Assert.DoesNotContain(probe.EvidenceRefs, evidence => evidence.Contains("direct-dpo-policy-score-source", StringComparison.Ordinal));
        Assert.DoesNotContain(probe.EvidenceRefs, evidence => evidence.Contains("direct-dpo-policy-acceptance-authority", StringComparison.Ordinal));

        Assert.Equal(bool.TrueString, probe.Metrics["direct_dpo_policy_runtime_scorer_configured"]);
        Assert.Equal(bool.FalseString, probe.Metrics["direct_dpo_policy_runtime_pair_content_available"]);
        Assert.Equal(bool.FalseString, probe.Metrics["direct_dpo_policy_inference_applied"]);
        Assert.Equal(bool.FalseString, probe.Metrics["direct_dpo_policy_score_authority"]);
        Assert.Equal(bool.FalseString, probe.Metrics["direct_dpo_policy_acceptance_authority"]);
        Assert.Equal(bool.TrueString, probe.Metrics["baseline_visual_verdict_acceptance_authority"]);
        Assert.Equal(bool.FalseString, probe.Metrics[DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey]);
        Assert.Equal(bool.TrueString, probe.Metrics[DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey]);
        Assert.Contains(DesignConversionMachine.DpoPolicyRuntimePairContentUnavailableWarning, probe.FlowWarnings);
        Assert.Contains(DesignConversionMachine.DpoPolicyRuntimePairContentUnavailableWarning, probe.OrchestratorWarnings);

        var publicSurface = BuildRuntimePublicSurface(probe);
        Assert.DoesNotContain("LlmModel.Forward", publicSurface, StringComparison.Ordinal);
        Assert.DoesNotContain("log-prob margin", publicSurface, StringComparison.Ordinal);
        Assert.DoesNotContain("DirectDpoPolicyInferenceSummary", publicSurface, StringComparison.Ordinal);
        Assert.DoesNotContain(forbiddenPathFragment, publicSurface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<main class=", publicSurface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".card{", publicSurface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"type\":\"FRAME\"", publicSurface, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildRuntimePairBindingSha256ForTest(
        DesignConversionDpoPolicyRuntimePair pair)
    {
        var independent = pair.IndependentEvaluation;
        var sourcePair = pair.SourcePair;
        var snapshot = new
        {
            schema = pair.Schema ?? string.Empty,
            attempt_number = pair.AttemptNumber,
            root_job_id = pair.RootJobId ?? string.Empty,
            root_request_id = pair.RootRequestId ?? string.Empty,
            root_ownership_binding = pair.RootOwnershipBinding ?? string.Empty,
            generated_candidate_id = pair.GeneratedCandidateId ?? string.Empty,
            generated = RuntimePairSideSnapshotForTest(pair.Generated),
            incumbent = RuntimePairSideSnapshotForTest(pair.Incumbent),
            artifact_root_binding = pair.ArtifactRootBinding ?? string.Empty,
            independent_schema = independent?.Schema ?? string.Empty,
            independent_attempt_number = independent?.AttemptNumber ?? 0,
            independent_generated_ref = independent?.GeneratedRef ?? string.Empty,
            independent_incumbent_ref = independent?.IncumbentRef ?? string.Empty,
            independent_generated_binding = independent?.GeneratedBinding ?? string.Empty,
            independent_incumbent_binding = independent?.IncumbentBinding ?? string.Empty,
            independent_generated_evaluation_identity =
                independent?.GeneratedEvaluationIdentity ?? string.Empty,
            independent_incumbent_evaluation_identity =
                independent?.IncumbentEvaluationIdentity ?? string.Empty,
            independent_preferred_direction =
                independent?.PreferredDirection ?? string.Empty,
            independent_receipt_sha256 =
                independent?.ReceiptSha256 ?? string.Empty,
            scorer_request_sha256 =
                independent?.ScorerRequestSha256 ?? string.Empty,
            scorer_output_sha256 =
                independent?.ScorerOutputSha256 ?? string.Empty,
            evaluation_profile_id =
                independent?.EvaluationProfileId ?? string.Empty,
            evaluation_profile_version =
                independent?.EvaluationProfileVersion ?? string.Empty,
            evaluation_profile_configuration_sha256 =
                independent?.EvaluationProfileConfigurationSha256 ?? string.Empty,
            evaluation_model_sha256 =
                independent?.EvaluationModelSha256 ?? string.Empty,
            independent_artifact_root_binding =
                independent?.ArtifactRootBinding ?? string.Empty,
            scorer_provenance = RuntimePairScorerSnapshotForTest(
                independent?.ScorerProvenance),
            independent_issued_at_utc = independent?.IssuedAtUtc
                .ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ??
                string.Empty,
            independent_expires_at_utc = independent?.ExpiresAtUtc
                .ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ??
                string.Empty,
            independent_replay_nonce = independent?.ReplayNonce ?? string.Empty,
            source_pair_attempt_number = sourcePair?.AttemptNumber ?? 0,
            source_pair_generated_candidate_id =
                sourcePair?.GeneratedCandidateId ?? string.Empty,
            source_pair_generated_evaluation_identity = sourcePair is null
                ? string.Empty
                : DesignConversionDpoGenerator
                    .BuildIndependentPreferenceEvaluationIdentity(
                        sourcePair.GeneratedEvaluation),
            source_pair_incumbent_evaluation_identity = sourcePair is null
                ? string.Empty
                : DesignConversionDpoGenerator
                    .BuildIndependentPreferenceEvaluationIdentity(
                        sourcePair.IncumbentEvaluation),
            observed_at_utc = pair.ObservedAtUtc.ToUniversalTime()
                .ToString("O", CultureInfo.InvariantCulture)
        };
        return Sha256RefForTest(JsonSerializer.Serialize(snapshot));
    }

    private static object RuntimePairSideSnapshotForTest(
        DesignConversionDpoPolicyRuntimeSide? side)
        => side is null
            ? new { missing = true }
            : new
            {
                side = side.Side ?? string.Empty,
                html_sha256 = Sha256RefForTest(side.Html ?? string.Empty),
                css_sha256 = Sha256RefForTest(side.Css ?? string.Empty),
                content_sha256 = side.ContentSha256 ?? string.Empty,
                custody_ref = side.CustodyRef ?? string.Empty,
                custody_binding = side.CustodyBinding ?? string.Empty,
                evaluation_identity = side.EvaluationIdentity ?? string.Empty,
                evaluation_job_id = side.EvaluationJobId ?? string.Empty,
                evaluation_request_id = side.EvaluationRequestId ?? string.Empty,
                output_id = side.OutputId ?? string.Empty,
                candidate_id = side.CandidateId ?? string.Empty,
                verdict_id = side.VerdictId ?? string.Empty
            };

    private static object RuntimePairScorerSnapshotForTest(
        DesignConversionIndependentPreferenceScorerProvenance? provenance)
        => provenance is null
            ? new { missing = true }
            : new
            {
                kind = provenance.Kind ?? string.Empty,
                scorer_id = provenance.ScorerId ?? string.Empty,
                scorer_version = provenance.ScorerVersion ?? string.Empty,
                request_id = provenance.RequestId ?? string.Empty,
                output_id = provenance.OutputId ?? string.Empty,
                runtime_instance_ref = provenance.RuntimeInstanceRef ?? string.Empty,
                evaluation_profile_id = provenance.EvaluationProfileId ?? string.Empty,
                evaluation_profile_version = provenance.EvaluationProfileVersion ?? string.Empty,
                evaluation_profile_configuration_sha256 =
                    provenance.EvaluationProfileConfigurationSha256 ?? string.Empty,
                evaluation_model_sha256 = provenance.EvaluationModelSha256 ?? string.Empty,
                inference_request_id = provenance.InferenceRequestId ?? string.Empty
            };

    private static string Sha256RefForTest(string value)
        => "sha256:" + Convert.ToHexString(
                SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(
                    value ?? string.Empty)))
            .ToLowerInvariant();

    private static DesignConversionInternalEvaluationResult
        MutateRuntimePairEvaluationCollision(
            DesignConversionInternalEvaluationResult evaluation,
            string collisionField,
            DesignConversionImprovementOutput generatedOutput)
    {
        var output = collisionField switch
        {
            "output-id" => evaluation.RuntimeFlowResult.Output with
            {
                OutputId = generatedOutput.OutputId
            },
            "candidate-id" => evaluation.RuntimeFlowResult.Output with
            {
                CandidateId = generatedOutput.CandidateId
            },
            "verdict-id" => evaluation.RuntimeFlowResult.Output with
            {
                Verdict = evaluation.RuntimeFlowResult.Output.Verdict with
                {
                    VerdictId = generatedOutput.Verdict.VerdictId
                }
            },
            _ => throw new ArgumentOutOfRangeException(nameof(collisionField))
        };
        var candidate = collisionField == "candidate-id"
            ? evaluation.RuntimeFlowResult.Candidate with
            {
                CandidateId = generatedOutput.CandidateId
            }
            : evaluation.RuntimeFlowResult.Candidate;
        var runtimeFlow = evaluation.RuntimeFlowResult with
        {
            Candidate = candidate,
            Output = output,
            ImproverOutput = evaluation.RuntimeFlowResult.ImproverOutput with
            {
                Candidate = candidate,
                Output = output
            }
        };
        return evaluation with
        {
            RuntimeFlowResult = runtimeFlow,
            Verdict = output.Verdict
        };
    }

    private static RuntimePairFixture BuildExactRuntimePairFixture(
        string preferredDirection =
            DesignConversionIndependentPreferenceEvidenceContract
                .GeneratedSide)
    {
        const string rootJobId = "dpo-runtime-pair-root-job";
        const string rootRequestId = "dpo-runtime-pair-root-request";
        const string generatedIdentity =
            "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
        const string generatedCandidateId =
            "teacher-authored:" + generatedIdentity;
        const string generatedRequestId =
            "teacher-authored-request:" + generatedIdentity;
        const string incumbentHtml =
            "<main class=\"incumbent\"><p>Measured incumbent</p></main>";
        const string incumbentCss =
            ".incumbent{display:block;gap:2px;}";
        const string generatedHtml =
            "<main class=\"generated\"><h1>Measured generated</h1></main>";
        const string generatedCss =
            ".generated{display:grid;gap:16px;}";
        var rootBinding = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-runtime-rejected-root-ownership",
            rootJobId + "|" + rootRequestId);

        var incumbent = RebindRuntimePairEvaluation(
            DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
                jobId: rootJobId,
                requestId: rootRequestId,
                outputId: "dpo-runtime-pair-incumbent-output",
                candidateId: "dpo-runtime-pair-incumbent-candidate",
                verdictId: "dpo-runtime-pair-incumbent-verdict",
                score: 0.72,
                passed: false,
                htmlSuffix: "incumbent"),
            rootJobId,
            rootRequestId,
            "dpo-runtime-pair-incumbent-candidate",
            incumbentHtml,
            incumbentCss,
            rootBinding);
        var generatedJobId = rootJobId + ":loop-attempt:2";
        var generated = RebindRuntimePairEvaluation(
            DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
                jobId: generatedJobId,
                requestId: generatedRequestId,
                outputId: "dpo-runtime-pair-generated-output",
                candidateId: generatedCandidateId,
                verdictId: "dpo-runtime-pair-generated-verdict",
                score: 0.86,
                passed: true,
                htmlSuffix: "generated"),
            generatedJobId,
            generatedRequestId,
            generatedCandidateId,
            generatedHtml,
            generatedCss,
            rootBinding);
        var incumbentBinding =
            DesignConversionDpoTeacherAttestationRuntime
                .BuildIncumbentBinding(incumbent.SubmittedRequest);
        var generatedBinding =
            DesignConversionDpoTeacherAttestationRuntime
                .BuildGeneratedBinding(generatedHtml, generatedCss);
        var authoring = new DesignConversionDpoTeacherCandidateAuthoringEvidence(
            DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
            DesignConversionDpoLeakageGuard.HashRef(
                "dcm-dpo-teacher-incumbent",
                incumbentBinding),
            DesignConversionDpoTeacherAttestationRuntime
                .BuildGeneratedRefFromCandidateId(generatedCandidateId),
            incumbentBinding,
            generatedBinding,
            generatedCandidateId,
            generatedRequestId,
            DesignConversionDpoGenerator.Sha256Ref(
                "runtime-pair-authoring-output"),
            TrainingTeacherProvenance.Deterministic(
                "runtime-pair-authoring-request"),
            ["runtime-pair-authoring:evidence"],
            new Dictionary<string, string>(),
            AttemptNumber: 2)
        {
            InferenceProfileId =
                LocalQwenOllamaInferenceProfileAuthority.Authoring.ProfileId,
            InferenceProfileConfigurationSha256 =
                LocalQwenOllamaInferenceProfileAuthority.Authoring
                    .ConfigurationSha256
        };
        var sourcePair = new DesignConversionTeacherAuthoredEvaluationPair(
            2,
            generated,
            incumbent,
            authoring,
            generatedCandidateId);
        var artifactRootBinding = DesignConversionDpoGenerator.Sha256Ref(
            "runtime-pair-artifact-root");
        var observedAtUtc = DateTimeOffset.UtcNow;
        var receipt = DesignConversionIndependentPreferenceEvidenceTestIssuer
            .Issue(
                sourcePair,
                artifactRootBinding,
                observedAtUtc,
                preferredDirection);
        var submitted = incumbent.SubmittedRequest;
        var rootRequest = new DesignConversionInternalEvaluateRequest(
            rootJobId,
            rootRequestId,
            submitted.SourceKind,
            submitted.Html,
            submitted.Css,
            submitted.DesignTreePayload,
            submitted.OriginalTreePayload,
            submitted.Screenshots,
            submitted.DesignRenders,
            submitted.Budget,
            [new RenderQaResolution("desktop", 1440, 900, 1.0)],
            [],
            new Dictionary<string, string>(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DesignConversionRuntimeRejectedEvaluationHandoff
                    .RootOwnershipBindingProvenanceKey] = rootBinding
            });
        var created = DesignConversionDpoPolicyRuntimePairFactory.Create(
            rootRequest,
            sourcePair,
            receipt,
            artifactRootBinding,
            observedAtUtc);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        var runtimeInput = DesignConversionTestFixtures.CreateInput(
            comparisonScore: 0.40,
            comparisonPassed: false) with
        {
            Request = generated.SubmittedRequest,
            DpoPolicyRuntimePair = created.Value
        };
        return new RuntimePairFixture(rootRequest, created.Value!, runtimeInput);
    }

    private static DesignConversionInternalEvaluationResult
        RebindRuntimePairEvaluation(
            DesignConversionInternalEvaluationResult evaluation,
            string jobId,
            string requestId,
            string candidateId,
            string html,
            string css,
            string rootBinding)
    {
        var submittedRequest = evaluation.SubmittedRequest with
        {
            RequestId = requestId,
            Html = html,
            Css = css,
            Provenance = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DesignConversionRuntimeRejectedEvaluationHandoff
                    .RootOwnershipBindingProvenanceKey] = rootBinding,
                ["internal_service_job_id"] = jobId
            }
        };
        var output = evaluation.RuntimeFlowResult.Output with
        {
            CandidateId = candidateId,
            Html = html,
            Css = css
        };
        var candidate = evaluation.RuntimeFlowResult.Candidate with
        {
            CandidateId = candidateId,
            RequestId = requestId,
            Html = html,
            Css = css
        };
        var runtimeFlow = evaluation.RuntimeFlowResult with
        {
            Request = submittedRequest,
            Candidate = candidate,
            Output = output,
            ImproverOutput = evaluation.RuntimeFlowResult.ImproverOutput with
            {
                Candidate = candidate,
                Output = output
            }
        };
        return evaluation with
        {
            JobId = jobId,
            SubmittedRequest = submittedRequest,
            RuntimeFlowResult = runtimeFlow,
            Verdict = output.Verdict,
            ImprovedHtml = html,
            ImprovedCss = css
        };
    }

    private static ModelConfig BuildDirectPolicyFixtureConfig()
        => new()
        {
            VocabSize = 256,
            ContextLen = 128,
            HiddenDim = 16,
            NumLayers = 1,
            NumHeads = 4,
            NumKvHeads = 1,
            FfnHiddenDim = 32,
            RopeTheta = 10_000f,
            RmsNormEps = 1e-5f
        };

    private static DesignConversionDpoPolicyCheckpointLoraContext BuildDirectPolicyFixtureLoraContext(
        ModelConfig config)
    {
        var lora = LoraApplicator.ValidateAndCanonicalize(new LoraConfig
        {
            Rank = 2,
            Alpha = 4f,
            Dropout = 0f,
            TargetModules = ["q_proj", "v_proj"]
        });

        return new DesignConversionDpoPolicyCheckpointLoraContext(
            lora.Rank,
            lora.Alpha,
            lora.Dropout,
            lora.TargetModules,
            config.NumLayers * lora.TargetModules.Length);
    }

    private static LoraConfig BuildLoraConfig(
        DesignConversionDpoPolicyCheckpointLoraContext lora)
        => new()
        {
            Rank = lora.Rank,
            Alpha = lora.Alpha,
            Dropout = lora.Dropout,
            TargetModules = lora.TargetModules.ToArray()
        };

    private static void FillDirectPolicyModelWeights(LlmModel model, int seed)
    {
        var entries = model.GetCheckpointParameterEntries();
        for (var entryIndex = 0; entryIndex < entries.Count; entryIndex++)
        {
            var data = entries[entryIndex].Parameter.Value.Data;
            for (var valueIndex = 0; valueIndex < data.Length; valueIndex++)
            {
                var angle = (seed + (entryIndex * 17) + valueIndex + 1) * 0.137f;
                data[valueIndex] = MathF.Sin(angle) * 0.045f + MathF.Cos(angle * 0.37f) * 0.015f;
            }
        }
    }

    private static byte[] SerializeCheckpointForStateComparison(
        DesignConversionMachineCheckpoint checkpoint)
        => JsonSerializer.SerializeToUtf8Bytes(
            checkpoint with { ExportedAtUtc = DateTimeOffset.UnixEpoch },
            DesignConversionCheckpointJsonOptions.Standard);

    private static long CaptureMachineStateGeneration(
        DesignConversionMachine machine)
    {
        var field = typeof(DesignConversionMachine).GetField(
            "_stateGeneration",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return Assert.IsType<long>(field!.GetValue(machine));
    }

    private static async Task AssertMachineRemainsUnboundAsync(DesignConversionMachine machine)
    {
        var export = await machine.ExportAsync();

        Assert.False(export.IsSuccess);
        Assert.Equal(DesignConversionMachine.ExportCheckpointNotBoundCode, export.ErrorCode);
    }

    private sealed record RuntimeScoreProbe(
        double Score,
        bool Passed,
        string OverallVerdict,
        string NextAction,
        IReadOnlyList<string> EvidenceRefs,
        IReadOnlyList<string> FlowWarnings,
        IReadOnlyList<string> OrchestratorWarnings,
        IReadOnlyDictionary<string, string> Metrics,
        string PrimaryMetricName,
        string ScalingSignalNote,
        string Notes);

    private sealed record NegativePairProbe(double Score, double SemanticFeatureWeight);

    private sealed record DirectPolicyRuntimeFixture(
        string RuntimeCheckpointPath,
        string PolicyCheckpointPath,
        string PolicyCheckpointSha256,
        DesignConversionDpoPolicyCheckpointRef PolicyCheckpointRef);

    private sealed record RuntimePairFixture(
        DesignConversionInternalEvaluateRequest RootRequest,
        DesignConversionDpoPolicyRuntimePair Pair,
        DesignConversionRuntimeFlowInput RuntimeInput);
}

internal static class DesignConversionTestFixtures
{
    public static DesignConversionRuntimeFlowInput CreateInput(
        double comparisonScore = 0.64,
        bool comparisonPassed = false)
    {
        var request = CreateRequest();
        var expected = CreateImage("expected", 1440, 900);
        var actual = CreateImage("actual", 1440, 900);
        var qaVerdict = CreateVerdict("qa-verdict", score: 0.82, passed: true);
        var comparisonVerdict = CreateVerdict("comparison-verdict", score: comparisonScore, passed: comparisonPassed);
        var qaResult = new RenderQaResultRecord(
            ResultId: "qa-result-001",
            RequestId: request.RequestId,
            Expected: expected,
            Actual: actual,
            Verdict: qaVerdict,
            Metrics: new Dictionary<string, double> { ["visual_distance"] = 0.18 },
            Warnings: []);
        var comparison = new DesignConversionQaComparisonArtifact(
            ArtifactId: "comparison-001",
            ComparisonRequestId: "comparison-request-001",
            Expected: expected,
            Actual: actual,
            Verdict: comparisonVerdict,
            DiffImageUri: "artifact://diff.png",
            DiffImageSha256: "sha256:diff",
            Metrics: new Dictionary<string, double> { ["pixel_delta"] = 0.36 },
            EvidenceRefs: ["comparison-source:001"]);

        return new DesignConversionRuntimeFlowInput(
            Request: request,
            QaResults: [qaResult],
            ComparisonArtifacts: [comparison],
            AiTeacherEvidenceRefs: ["teacher-review:redacted"],
            DeterministicEvidenceRefs: new Dictionary<string, string>
            {
                ["fixture"] = "render-qa-record"
            });
    }

    private static SubmittedDesignConversionRequest CreateRequest()
        => new(
            RequestId: "design-request-machine-001",
            SourceKind: DesignConversionSourceKind.Figma,
            Html: "<main class=\"card\">Hello</main>",
            Css: ".card{display:grid;}",
            DesignTreePayload: new DesignTreePayload(
                PayloadId: "tree-current",
                Format: "figma-json",
                Content: "{\"type\":\"FRAME\"}",
                Sha256: "sha256:tree-current",
                Metadata: new Dictionary<string, string>()),
            OriginalTreePayload: new DesignTreePayload(
                PayloadId: "tree-original",
                Format: "original-json",
                Content: "{\"type\":\"FRAME\"}",
                Sha256: "sha256:tree-original",
                Metadata: new Dictionary<string, string>()),
            Screenshots: [new DesignRenderArtifact("shot-001", "screenshot", 1440, 900, "artifact://shot.png", "sha256:shot", new Dictionary<string, string>())],
            DesignRenders: [new DesignRenderArtifact("render-001", "design-render", 1440, 900, "artifact://render.png", "sha256:render", new Dictionary<string, string>())],
            Budget: new DesignConversionBudgetMetadata(
                UserId: "user-redacted",
                Tier: "pro",
                MaxImprovementCandidates: 3,
                MaxRenderQaRuns: 4,
                BudgetUsd: 1.50m,
                Metadata: new Dictionary<string, string>()),
            Provenance: new Dictionary<string, string> { ["source"] = "machine-test" });

    private static RenderQaImageRecord CreateImage(string id, int width, int height)
        => new(
            ImageId: id,
            Label: id,
            Resolution: new RenderQaResolution($"res-{width}x{height}", width, height, 1.0),
            Uri: $"artifact://{id}.png",
            Sha256: $"sha256:{id}",
            Metadata: new Dictionary<string, string>());

    private static VisualVerdict CreateVerdict(string verdictId, double score, bool passed)
        => new(
            VerdictId: verdictId,
            Score: score,
            Defects:
            [
                new VisualDefect(
                    DefectId: $"{verdictId}:spacing",
                    Category: "spacing",
                    Severity: passed ? "info" : "major",
                    Description: "Spacing evidence supplied by render QA input.",
                    X: 24,
                    Y: 40,
                    Width: 120,
                    Height: 32,
                    EvidenceImageIds: ["expected", "actual"])
            ],
            EvidenceRefs: [$"verdict-evidence:{verdictId}"],
            Passed: passed);
}
