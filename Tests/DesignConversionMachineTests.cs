// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using AgentInfrastructure.Logical;
using DesignConversionDomain;

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
        var result = await machine.TrainBatchAsync(new LearningBatch<DesignConversionDomain.DesignConversionDomain>(
            BatchId: "design-conversion-batch-001",
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: "item-001",
                    Prompt: "source design",
                    ChosenResponse: "chosen candidate",
                    RejectedResponse: "rejected candidate")
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

        var train = await machine.TrainBatchAsync(BuildPass150Batch("gc2-usefulness-score-not-learned", 0.95, 0.15));

        Assert.True(train.IsSuccess, train.ErrorMessage);
        var checkpoint = machine.ExportCheckpoint();
        Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);
        var criticWeights = checkpoint.Value!.CriticUnit.LearnedWeights ?? [];
        Assert.Contains(criticWeights, weight => string.Equals("source_manifest:hash_diversity", weight.FeatureKey, StringComparison.Ordinal));
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
    public async Task LoadCheckpointAsync_UsesOneByteSnapshotForHashAndImport()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"checkpoint-snapshot-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var firstMachine = new DesignConversionMachine();
            var firstTrain = await firstMachine.TrainBatchAsync(BuildPass150Batch("snapshot-first", 0.95, 0.15));
            Assert.True(firstTrain.IsSuccess, firstTrain.ErrorMessage);
            var firstCheckpoint = firstMachine.ExportCheckpoint();
            Assert.True(firstCheckpoint.IsSuccess, firstCheckpoint.ErrorMessage);
            var firstBytes = JsonSerializer.SerializeToUtf8Bytes(
                firstCheckpoint.Value!,
                DesignConversionCheckpointJsonOptions.Standard);

            var secondMachine = new DesignConversionMachine();
            Assert.True((await secondMachine.TrainBatchAsync(BuildPass150Batch("snapshot-second-a", 0.95, 0.15))).IsSuccess);
            Assert.True((await secondMachine.TrainBatchAsync(BuildPass150Batch("snapshot-second-b", 0.95, 0.15))).IsSuccess);
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
    public async Task TrainBatch_DpoRowsMutateCheckpointAndFreshLoadUsesNegativePairWeights()
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
            var train = await trainedMachine.TrainBatchAsync(BuildPass150Batch("pass150-positive-margin", 0.95, 0.15));
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
            Assert.Contains(criticWeights, weight => string.Equals(
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
            Assert.DoesNotContain(criticWeights, weight =>
                weight.FeatureKey.StartsWith("source_kind:", StringComparison.Ordinal));

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
                chosenScore: 0.20,
                rejectedScore: 0.00,
                runtimeInput);
            var hardNegative = await TrainLoadAndProbeAsync(
                root,
                "hard-negative",
                chosenScore: 0.20,
                rejectedScore: 0.90,
                runtimeInput);
            Assert.Equal(easyNegative.HashDiversityWeight, hardNegative.HashDiversityWeight);
            Assert.Equal(easyNegative.Score, hardNegative.Score);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task FreshLoadedLearnedWeights_CanRaiseLowRuntimeScoreBeyondOldEchoCap()
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
            var train = await trainedMachine.TrainBatchAsync(BuildPass150Batch("score-ceiling-positive-margin", 0.95, 0.15));
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
            Assert.True(freshLoadedProbe.Score >= 0.85, $"fresh loaded score did not reach local runtime threshold: {freshLoadedProbe.Score:0.######}");
            Assert.True(freshLoadedProbe.Passed);
            Assert.Equal("DESIGN_CONVERSION_INPUT_EVIDENCE_PASSED", freshLoadedProbe.OverallVerdict);
            Assert.Equal(bool.TrueString, freshLoadedProbe.Metrics["critic_passed"]);
            Assert.Equal(bool.FalseString, freshLoadedProbe.Metrics["trained_model_authority"]);
            Assert.Equal(bool.TrueString, freshLoadedProbe.Metrics["not_trained_model_authority"]);
            Assert.Contains("ProductReady/model training remains false", freshLoadedProbe.Notes, StringComparison.Ordinal);
            Assert.Contains(DesignConversionMachine.RuntimeScaffoldNotTrainedModelWarning, freshLoadedProbe.FlowWarnings);
            Assert.Contains(
                freshLoadedProbe.EvidenceRefs,
                reference => reference.StartsWith("LocalCheckpointFeatureAdjustmentSummary", StringComparison.Ordinal) &&
                             reference.Contains("score_before=0.4", StringComparison.Ordinal) &&
                             reference.Contains("adjustment_scale=tanh_bounded_0.5", StringComparison.Ordinal) &&
                             reference.Contains("acceptance_threshold=0.85", StringComparison.Ordinal));
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
        double chosenScore,
        double rejectedScore,
        DesignConversionRuntimeFlowInput runtimeInput)
    {
        var machine = new DesignConversionMachine();
        var train = await machine.TrainBatchAsync(BuildPass150Batch(
            $"pass150-{caseId}",
            chosenScore,
            rejectedScore));
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var path = Path.Combine(root, $"{caseId}-checkpoint.json");
        var save = await machine.SaveCheckpointAsync(path);
        Assert.True(save.IsSuccess, save.ErrorMessage);
        var loaded = await DesignConversionMachine.LoadCheckpointAsync(path);
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        var hashDiversity = Assert.Single(
            loaded.Value!.Checkpoint.CriticUnit.LearnedWeights ?? [],
            weight => string.Equals("source_manifest:hash_diversity", weight.FeatureKey, StringComparison.Ordinal));
        var probe = await RunMachineScoreAsync(loaded.Value.Machine, runtimeInput, $"pass150-{caseId}-runtime");

        return new NegativePairProbe(probe.Score, hashDiversity.Weight);
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

    private static string FormatScore(double value)
        => value.ToString("0.######", CultureInfo.InvariantCulture);

    private static async Task<string> ReadFileSha256Async(string path)
        => Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(path))).ToLowerInvariant();

    private static string Sha256Bytes(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private sealed record RuntimeScoreProbe(
        double Score,
        bool Passed,
        string OverallVerdict,
        IReadOnlyList<string> EvidenceRefs,
        IReadOnlyList<string> FlowWarnings,
        IReadOnlyList<string> OrchestratorWarnings,
        IReadOnlyDictionary<string, string> Metrics,
        string PrimaryMetricName,
        string ScalingSignalNote,
        string Notes);

    private sealed record NegativePairProbe(double Score, double HashDiversityWeight);
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
