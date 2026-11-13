// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AutonomousLearning;
using AgentInfrastructure;
using ContinuousLearning;
using LoRA;
using TensorCore.Core;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

public sealed class HeldOutLoraAccuracyEvaluatorTests
{
    [Fact]
    public async Task TrainingSourceSha256Async_RestoresPosition_AndHonorsCancellation()
    {
        await using var stream = new MemoryStream([1, 2, 3, 4, 5]);
        stream.Position = 3;

        var expected = TrainingSourceProvenance.Sha256(stream);
        var actual = await TrainingSourceProvenance.Sha256Async(stream);

        Assert.Equal(expected, actual);
        Assert.Equal(3, stream.Position);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await TrainingSourceProvenance.Sha256Async(
                stream,
                cancellation.Token));
        Assert.Equal(3, stream.Position);
    }

    [Fact]
    public void Evaluate_ActualTrainingSourceOverlap_FailsBeforeArtifactAccess()
    {
        var shared = Source("shared", "same bytes");
        var manifest = Manifest(shared);
        var evaluator = new HeldOutLoraAccuracyEvaluator(
            TinyConfig(),
            Path.Combine(Path.GetTempPath(), $"missing-base-{Guid.NewGuid():N}"),
            Path.Combine(Path.GetTempPath(), $"missing-tokenizer-{Guid.NewGuid():N}"),
            Provider(new HeldOutEvaluationCase("case-1", shared, "prompt", "expected")));

        var result = evaluator.Evaluate(BaselineRequest(manifest));

        Assert.False(result.IsSuccess);
        Assert.Equal("HELD_OUT_SOURCE_OVERLAP", result.ErrorCode);
    }

    [Fact]
    public void Evaluate_BaseAndCompatibleNonZeroAdapter_UseExactArtifactIdentities()
    {
        using var fixture = new HeldOutEvaluatorFixture();
        var manifest = Manifest(Source("train", "training bytes"));
        var adapter = fixture.WriteAdapter(
            manifest,
            IncrementalTrainer.DefaultConfig,
            fillAdapterParameters: true);
        var evaluator = fixture.CreateEvaluator(
            Provider(new HeldOutEvaluationCase(
                "case-1",
                Source("held", "held bytes"),
                "p",
                "r")));

        var baseline = evaluator.Evaluate(BaselineRequest(manifest));
        Assert.True(baseline.IsSuccess, baseline.ErrorMessage);
        var candidate = evaluator.Evaluate(CandidateRequest(
            manifest,
            adapter,
            baseline.Value!.Artifacts));

        Assert.True(candidate.IsSuccess, candidate.ErrorMessage);
        Assert.InRange(baseline.Value.Accuracy, 0.0, 1.0);
        Assert.InRange(candidate.Value!.Accuracy, 0.0, 1.0);
        Assert.Equal(adapter.Provenance!.AdapterSha256, candidate.Value.AdapterSha256);
        Assert.Equal(manifest.Sha256, candidate.Value.TrainingSourceManifestSha256);
        Assert.Equal(baseline.Value.Artifacts, candidate.Value.Artifacts);
    }

    [Fact]
    public void Evaluate_LoadsNonZeroAdapterIntoScoredForwardPath()
    {
        using var fixture = new HeldOutEvaluatorFixture();
        var manifest = Manifest(Source("train", "training bytes"));
        ModelAdapter? separatingAdapter = null;
        HeldOutEvaluationCase? separatingCase = null;
        foreach (var (initial, increment) in new[]
                 {
                     (0.125f, 0.0005f),
                     (-0.25f, 0.001f),
                     (0.75f, -0.0015f),
                     (-1.25f, 0.002f),
                 })
        {
            var candidate = fixture.WriteAdapter(
                manifest,
                IncrementalTrainer.DefaultConfig,
                fillAdapterParameters: true,
                initialAdapterValue: initial,
                adapterValueIncrement: increment);
            var evaluationCase = fixture.FindCaseWhereAdapterChangesPrediction(candidate);
            if (evaluationCase is null)
                continue;
            separatingAdapter = candidate;
            separatingCase = evaluationCase;
            break;
        }

        Assert.NotNull(separatingAdapter);
        Assert.NotNull(separatingCase);
        var evaluator = fixture.CreateEvaluator(Provider(separatingCase!));
        var baseline = evaluator.Evaluate(BaselineRequest(manifest));
        Assert.True(baseline.IsSuccess, baseline.ErrorMessage);
        var candidateResult = evaluator.Evaluate(CandidateRequest(
            manifest,
            separatingAdapter!,
            baseline.Value!.Artifacts));

        Assert.True(candidateResult.IsSuccess, candidateResult.ErrorMessage);
        Assert.Equal(0.0, baseline.Value.Accuracy);
        Assert.Equal(1.0, candidateResult.Value!.Accuracy);
    }

    [Fact]
    public void Evaluate_AdapterBytesDoNotMatchTrainerProvenance_FailsClosed()
    {
        using var fixture = new HeldOutEvaluatorFixture();
        var manifest = Manifest(Source("train", "training bytes"));
        var adapter = fixture.WriteAdapter(
            manifest,
            IncrementalTrainer.DefaultConfig,
            fillAdapterParameters: true);
        File.AppendAllText(adapter.AdapterPath, "tampered");
        var evaluator = fixture.CreateEvaluator(Provider(ValidHeldCase()));

        var baseline = evaluator.Evaluate(BaselineRequest(manifest));
        Assert.True(baseline.IsSuccess, baseline.ErrorMessage);
        var result = evaluator.Evaluate(CandidateRequest(
            manifest,
            adapter,
            baseline.Value!.Artifacts));

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_PROVENANCE_MISMATCH", result.ErrorCode);
    }

    [Fact]
    public void Evaluate_StructurallyValidButIncompatibleAdapter_FailsBeforeInference()
    {
        using var fixture = new HeldOutEvaluatorFixture();
        var manifest = Manifest(Source("train", "training bytes"));
        var adapter = fixture.WriteAdapter(
            manifest,
            new LoraConfig
            {
                Rank = 2,
                Alpha = 4,
                Dropout = 0,
                TargetModules = ["q_proj", "v_proj"],
            },
            fillAdapterParameters: true);
        var evaluator = fixture.CreateEvaluator(Provider(ValidHeldCase()));

        var baseline = evaluator.Evaluate(BaselineRequest(manifest));
        Assert.True(baseline.IsSuccess, baseline.ErrorMessage);
        var result = evaluator.Evaluate(CandidateRequest(
            manifest,
            adapter,
            baseline.Value!.Artifacts));

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_CONFIGURATION_MISMATCH", result.ErrorCode);
    }

    [Fact]
    public void Evaluate_CandidateMustUseExactActualBatchManifest()
    {
        using var fixture = new HeldOutEvaluatorFixture();
        var trainedManifest = Manifest(Source("train", "training bytes"));
        var declaredManifest = Manifest(Source("other", "other bytes"));
        var adapter = fixture.WriteAdapter(
            trainedManifest,
            IncrementalTrainer.DefaultConfig,
            fillAdapterParameters: true);
        var evaluator = fixture.CreateEvaluator(Provider(ValidHeldCase()));

        var baseline = evaluator.Evaluate(BaselineRequest(declaredManifest));
        Assert.True(baseline.IsSuccess, baseline.ErrorMessage);
        var result = evaluator.Evaluate(CandidateRequest(
            declaredManifest,
            adapter,
            baseline.Value!.Artifacts));

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_TRAINING_SOURCE_UNDECLARED", result.ErrorCode);
    }

    [Theory]
    [InlineData("not-canonical", "TRAINING_SOURCE_ID_NONCANONICAL")]
    [InlineData("", "TRAINING_SOURCE_ID_NONCANONICAL")]
    public void Evaluate_InvalidActualTrainingSourceManifest_FailsBeforeArtifactAccess(
        string sourceId,
        string expectedCode)
    {
        var evaluator = new HeldOutLoraAccuracyEvaluator(
            TinyConfig(),
            Path.Combine(Path.GetTempPath(), $"missing-base-{Guid.NewGuid():N}"),
            Path.Combine(Path.GetTempPath(), $"missing-tokenizer-{Guid.NewGuid():N}"),
            Provider(ValidHeldCase()));
        var request = BaselineRequest(
            new TrainingSourceManifest([sourceId], new string('0', 64)));

        var result = evaluator.Evaluate(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.ErrorCode);
    }

    [Fact]
    public void Evaluate_ManifestHashMismatch_FailsBeforeArtifactAccess()
    {
        var manifest = Manifest(Source("train", "training bytes")) with
        {
            Sha256 = new string('0', 64),
        };
        var evaluator = new HeldOutLoraAccuracyEvaluator(
            TinyConfig(),
            Path.Combine(Path.GetTempPath(), $"missing-base-{Guid.NewGuid():N}"),
            Path.Combine(Path.GetTempPath(), $"missing-tokenizer-{Guid.NewGuid():N}"),
            Provider(ValidHeldCase()));

        var result = evaluator.Evaluate(BaselineRequest(manifest));

        Assert.False(result.IsSuccess);
        Assert.Equal("TRAINING_SOURCE_MANIFEST_HASH_MISMATCH", result.ErrorCode);
    }

    [Fact]
    public void TrainingSourceManifest_IsCanonicalSortedAndRejectsDuplicates()
    {
        var first = Source("one", "first bytes");
        var second = Source("two", "second bytes");

        var forward = TrainingSourceProvenance.BuildManifest([first, second]);
        var reverse = TrainingSourceProvenance.BuildManifest([second, first]);
        var duplicate = TrainingSourceProvenance.BuildManifest([first, first]);

        Assert.True(forward.IsSuccess, forward.ErrorMessage);
        Assert.True(reverse.IsSuccess, reverse.ErrorMessage);
        Assert.Equal(forward.Value!.SourceIds, reverse.Value!.SourceIds);
        Assert.Equal(forward.Value.Sha256, reverse.Value.Sha256);
        Assert.False(duplicate.IsSuccess);
        Assert.Equal("TRAINING_SOURCE_ID_DUPLICATE", duplicate.ErrorCode);
        Assert.NotEqual(
            Source("CaseSensitive", "same bytes"),
            Source("casesensitive", "same bytes"));

        var inherited = Manifest(first);
        var current = Manifest(first, second);
        var combined = TrainingSourceProvenance.Combine(inherited, current);
        Assert.True(combined.IsSuccess, combined.ErrorMessage);
        Assert.Equal(2, combined.Value!.SourceIds.Count);
    }

    [Fact]
    public void TrainingSourceIdentity_NormalizesWindowsAndUrlAliases_ButPreservesUnixCase()
    {
        Assert.Equal(
            Source(@"C:\Data\Project\File.cs", "first"),
            Source("c:/data/project/file.cs", "first"));
        Assert.Equal(
            Source("HTTP://Example.COM:80/a", "first"),
            Source("http://example.com/a", "first"));
        Assert.NotEqual(
            Source("/Data/File.cs", "first"),
            Source("/data/File.cs", "first"));
        Assert.Throws<ArgumentException>(() => Source("http://", "first"));
        Assert.False(TrainingSourceProvenance.TryParseSourceId("source:not-valid", out _));
    }

    [Fact]
    public void Evaluate_ContentAliasAcrossDifferentOrigins_FailsBeforeArtifactAccess()
    {
        var manifest = Manifest(Source("train-a", "identical bytes"));
        var evaluator = new HeldOutLoraAccuracyEvaluator(
            TinyConfig(),
            Path.Combine(Path.GetTempPath(), $"missing-base-{Guid.NewGuid():N}"),
            Path.Combine(Path.GetTempPath(), $"missing-tokenizer-{Guid.NewGuid():N}"),
            Provider(new HeldOutEvaluationCase(
                "case-1",
                Source("held-b", "identical bytes"),
                "prompt",
                "expected")));

        var result = evaluator.Evaluate(BaselineRequest(manifest));

        Assert.False(result.IsSuccess);
        Assert.Equal("HELD_OUT_SOURCE_OVERLAP", result.ErrorCode);
    }

    [Theory]
    [InlineData(@"C:\Data\Project\File.cs", "c:/data/project/file.cs")]
    [InlineData("HTTP://Example.COM:80/a", "http://example.com/a")]
    public void Evaluate_NormalizedOriginAliasWithDifferentContent_FailsBeforeArtifactAccess(
        string trainingOrigin,
        string heldOrigin)
    {
        var manifest = Manifest(Source(trainingOrigin, "training bytes"));
        var evaluator = new HeldOutLoraAccuracyEvaluator(
            TinyConfig(),
            Path.Combine(Path.GetTempPath(), $"missing-base-{Guid.NewGuid():N}"),
            Path.Combine(Path.GetTempPath(), $"missing-tokenizer-{Guid.NewGuid():N}"),
            Provider(new HeldOutEvaluationCase(
                "case-1",
                Source(heldOrigin, "different held bytes"),
                "prompt",
                "expected")));

        var result = evaluator.Evaluate(BaselineRequest(manifest));

        Assert.False(result.IsSuccess);
        Assert.Equal("HELD_OUT_SOURCE_OVERLAP", result.ErrorCode);
    }

    [Fact]
    public void Evaluate_ReplacedBaseBetweenPhases_IsRejected()
    {
        using var fixture = new HeldOutEvaluatorFixture();
        var manifest = Manifest(Source("train", "training bytes"));
        var adapter = fixture.WriteAdapter(
            manifest,
            IncrementalTrainer.DefaultConfig,
            fillAdapterParameters: true);
        var evaluator = fixture.CreateEvaluator(Provider(ValidHeldCase()));
        var baseline = evaluator.Evaluate(BaselineRequest(manifest));
        Assert.True(baseline.IsSuccess, baseline.ErrorMessage);

        new LlmModel(fixture.ModelConfig).SaveCheckpoint(fixture.BaseCheckpointPath);
        var result = evaluator.Evaluate(CandidateRequest(
            manifest,
            adapter,
            baseline.Value!.Artifacts));

        Assert.False(result.IsSuccess);
        Assert.Equal("EVALUATION_ARTIFACT_IDENTITY_CHANGED", result.ErrorCode);
    }

    [Fact]
    public void Evaluate_ArtifactSizeLimit_FailsClosed()
    {
        using var fixture = new HeldOutEvaluatorFixture();
        var manifest = Manifest(Source("train", "training bytes"));
        var limits = new HeldOutEvaluationLimits(MaxBaseCheckpointBytes: 1);
        var evaluator = fixture.CreateEvaluator(Provider(ValidHeldCase()), limits: limits);

        var result = evaluator.Evaluate(BaselineRequest(manifest));

        Assert.False(result.IsSuccess);
        Assert.Equal("BASE_CHECKPOINT_SIZE_INVALID", result.ErrorCode);
    }

    [Fact]
    public void Evaluate_MaterializesProviderCasesExactlyOnce_AndHoldsArtifactsWhileScoring()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var fixture = new HeldOutEvaluatorFixture();
        var manifest = Manifest(Source("train", "training bytes"));
        var adapter = fixture.WriteAdapter(
            manifest,
            IncrementalTrainer.DefaultConfig,
            fillAdapterParameters: true);
        var baselineEvaluator = fixture.CreateEvaluator(Provider(ValidHeldCase()));
        var baseline = baselineEvaluator.Evaluate(BaselineRequest(manifest));
        Assert.True(baseline.IsSuccess, baseline.ErrorMessage);

        var cases = new SingleEnumerationCases(ValidHeldCase());
        var probeRan = false;
        var evaluator = fixture.CreateEvaluator(
            new DirectHeldOutProvider(
                new SourceDisjointHeldOutEvaluationSet("CODE_Q", cases)),
            phaseProbe: phase =>
            {
                if (phase != HeldOutEvaluationProbePhase.BeforeForward)
                    return;
                AssertHeldAgainstReplacement(fixture.BaseCheckpointPath);
                AssertHeldAgainstReplacement(fixture.TokenizerPath);
                AssertHeldAgainstReplacement(adapter.AdapterPath);
                probeRan = true;
            });

        var result = evaluator.Evaluate(CandidateRequest(
            manifest,
            adapter,
            baseline.Value!.Artifacts));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(probeRan);
        Assert.Equal(1, cases.EnumerationCount);
    }

    [Theory]
    [InlineData((int)HeldOutEvaluationProbePhase.BeforeForward)]
    [InlineData((int)HeldOutEvaluationProbePhase.BeforeArgMax)]
    public void Evaluate_CancellationDuringCompute_Propagates(
        int cancellationPhaseValue)
    {
        var cancellationPhase = (HeldOutEvaluationProbePhase)cancellationPhaseValue;
        using var fixture = new HeldOutEvaluatorFixture();
        using var cancellation = new CancellationTokenSource();
        var evaluator = fixture.CreateEvaluator(
            Provider(ValidHeldCase()),
            phaseProbe: phase =>
            {
                if (phase == cancellationPhase)
                    cancellation.Cancel();
            });

        Assert.Throws<OperationCanceledException>(() => evaluator.Evaluate(
            BaselineRequest(Manifest(Source("train", "training bytes"))),
            cancellation.Token));
    }

    [Fact]
    public void LlmModel_CancellationTriggeredInsideForward_PropagatesAtStageBoundary()
    {
        var model = new LlmModel(TinyConfig());
        using var cancellation = new CancellationTokenSource();
        var tokenIds = new Tensor([1f, 2f], new TensorShape(1, 2));

        Assert.Throws<OperationCanceledException>(() => model.Forward(
            tokenIds,
            training: false,
            cancellation.Token,
            phase =>
            {
                if (phase == LlmForwardPhase.AfterEmbedding)
                    cancellation.Cancel();
            }));
    }

    [Fact]
    public void TensorOps_MatMulCancellationInterruptsTheActiveKernel()
    {
        const int size = 128;
        var left = new Tensor(new float[size * size], new TensorShape(size, size));
        var right = new Tensor(new float[size * size], new TensorShape(size, size));
        var expectedLeft = left.Data.ToArray();
        var expectedRight = right.Data.ToArray();
        using var cancellation = new CancellationTokenSource();
        using var kernelProgress = new ManualResetEventSlim();
        using var cancellationIssued = new ManualResetEventSlim();
        var matchingEventCount = 0;
        using var observer = TensorKernelProgress.Observe(progress =>
        {
            if (!StringComparer.Ordinal.Equals(progress.KernelName, "SimdOps.MatVecRow"))
            {
                return;
            }

            if (Interlocked.Increment(ref matchingEventCount) != 1)
            {
                return;
            }

            Assert.Equal(TensorKernelProgressStage.Progress, progress.Stage);
            Assert.True(progress.CompletedWork > 0);
            kernelProgress.Set();
            if (!cancellationIssued.Wait(TimeSpan.FromSeconds(5)))
                throw new TimeoutException("Cancellation thread did not acknowledge MatMul progress.");
        });
        var canceller = new Thread(() =>
        {
            kernelProgress.Wait();
            cancellation.Cancel();
            cancellationIssued.Set();
        });
        canceller.Start();

        Exception? failure = null;
        try
        {
            _ = TensorOps.MatMul(left, right, cancellation.Token);
        }
        catch (Exception exception)
        {
            failure = exception;
        }
        finally
        {
            kernelProgress.Set();
            Assert.True(canceller.Join(TimeSpan.FromSeconds(5)));
        }

        Assert.Equal(1, Volatile.Read(ref matchingEventCount));
        Assert.IsType<OperationCanceledException>(failure);
        Assert.Equal(expectedLeft, left.Data);
        Assert.Equal(expectedRight, right.Data);
    }

    [Fact]
    public void TensorOps_CancellationOverloadPreservesUncancelledMatMulResult()
    {
        var left = new Tensor([1f, 2f, 3f, 4f], new TensorShape(2, 2));
        var right = new Tensor([5f, 6f, 7f, 8f], new TensorShape(2, 2));

        var legacy = TensorOps.MatMul(left, right);
        var cancellable = TensorOps.MatMul(left, right, CancellationToken.None);

        Assert.Equal(legacy.Shape.Dimensions, cancellable.Shape.Dimensions);
        Assert.Equal(legacy.Data, cancellable.Data);
    }

    [Fact]
    public void Evaluate_LogitsBudget_FailsBeforeTensorOrForward()
    {
        using var fixture = new HeldOutEvaluatorFixture();
        var evaluator = fixture.CreateEvaluator(
            Provider(ValidHeldCase()),
            limits: new HeldOutEvaluationLimits(MaxLogitsElements: 1));

        var result = evaluator.Evaluate(
            BaselineRequest(Manifest(Source("train", "training bytes"))));

        Assert.False(result.IsSuccess);
        Assert.Equal("HELD_OUT_COMPUTE_LIMIT_EXCEEDED", result.ErrorCode);
    }

    [Fact]
    public void Evaluate_TightBaseBudgetRejectsCandidateSolelyForLoraCompute()
    {
        using var fixture = new HeldOutEvaluatorFixture();
        var manifest = Manifest(Source("train", "training bytes"));
        var lora = IncrementalTrainer.DefaultConfig;
        var adapter = fixture.WriteAdapter(manifest, lora, fillAdapterParameters: true);
        var evaluationCase = ValidHeldCase();
        var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
        var tokens = tokenizer.Encode(string.Concat(
            evaluationCase.Prompt,
            "\n",
            evaluationCase.ExpectedResponse)).Count - 1L;
        var config = fixture.ModelConfig;
        var hidden = (long)config.HiddenDim;
        var kvWidth = (long)config.NumKvHeads * config.HeadDim;
        var projectionOps = tokens
            * (2L * hidden * hidden
               + 2L * hidden * kvWidth
               + 3L * hidden * config.FfnHiddenDim);
        var attentionOps = 2L
            * config.NumHeads
            * tokens
            * tokens
            * config.HeadDim;
        var baseBudget = (projectionOps + attentionOps) * config.NumLayers
                         + tokens * hidden * config.VocabSize;
        var evaluator = fixture.CreateEvaluator(
            Provider(evaluationCase),
            loraConfig: lora,
            limits: new HeldOutEvaluationLimits(MaxOperationBudget: baseBudget));

        var baseline = evaluator.Evaluate(BaselineRequest(manifest));
        Assert.True(baseline.IsSuccess, baseline.ErrorMessage);
        var candidate = evaluator.Evaluate(CandidateRequest(
            manifest,
            adapter,
            baseline.Value!.Artifacts));

        Assert.False(candidate.IsSuccess);
        Assert.Equal("HELD_OUT_COMPUTE_LIMIT_EXCEEDED", candidate.ErrorCode);
    }

    [Fact]
    public void EstimateCaseCompute_AccountsExactLoraInferenceAllocationsAndWorkspaces()
    {
        var config = TinyConfig();
        var lora = LoraApplicator.ValidateAndCanonicalize(
            IncrementalTrainer.DefaultConfig);
        const int tokens = 7;

        var baseline = HeldOutLoraAccuracyEvaluator.EstimateCaseCompute(
            config,
            tokens,
            candidateLora: null);
        var candidate = HeldOutLoraAccuracyEvaluator.EstimateCaseCompute(
            config,
            tokens,
            lora);

        var hidden = (long)config.HiddenDim;
        var kvWidth = (long)config.NumKvHeads * config.HeadDim;
        long expectedElementsPerLayer = 0;
        long expectedOperationsPerLayer = 0;
        foreach (var target in lora.TargetModules)
        {
            var output = target is "k_proj" or "v_proj" ? kvWidth : hidden;
            var rank = (long)lora.Rank;
            var explicitAndMatMulTransposes = 2L * rank * (hidden + output);
            expectedElementsPerLayer += explicitAndMatMulTransposes
                                        + tokens * rank
                                        + 3L * tokens * output;
            expectedOperationsPerLayer += explicitAndMatMulTransposes
                                          + tokens * hidden * rank
                                          + tokens * rank * output
                                          + 2L * tokens * output;
        }

        var expectedElements = expectedElementsPerLayer * config.NumLayers;
        var expectedOperations = expectedOperationsPerLayer * config.NumLayers;
        Assert.Equal(baseline.LogitsElements, candidate.LogitsElements);
        Assert.Equal(baseline.LogitsBytes, candidate.LogitsBytes);
        Assert.Equal(expectedElements, candidate.ActivationElements - baseline.ActivationElements);
        Assert.Equal(expectedElements * sizeof(float), candidate.ActivationBytes - baseline.ActivationBytes);
        Assert.Equal(expectedOperations, candidate.Operations - baseline.Operations);
    }

    [Fact]
    public void Evaluate_CumulativeBudgetValidatesAllCasesBeforeModelConstruction()
    {
        using var fixture = new HeldOutEvaluatorFixture();
        var first = ValidHeldCase();
        var second = new HeldOutEvaluationCase(
            "case-2",
            Source("held-2", "held bytes 2"),
            first.Prompt,
            first.ExpectedResponse);
        var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
        var inputCount = tokenizer.Encode(string.Concat(
            first.Prompt,
            "\n",
            first.ExpectedResponse)).Count - 1;
        var perCase = HeldOutLoraAccuracyEvaluator.EstimateCaseCompute(
            fixture.ModelConfig,
            inputCount,
            candidateLora: null);
        var phases = new List<HeldOutEvaluationProbePhase>();
        var evaluator = fixture.CreateEvaluator(
            Provider(first, second),
            limits: new HeldOutEvaluationLimits(
                MaxCumulativeOperationBudget: perCase.Operations),
            phaseProbe: phases.Add);

        var result = evaluator.Evaluate(
            BaselineRequest(Manifest(Source("train", "training bytes"))));

        Assert.False(result.IsSuccess);
        Assert.Equal("HELD_OUT_COMPUTE_LIMIT_EXCEEDED", result.ErrorCode);
        Assert.Empty(phases);
    }

    [Fact]
    public void Evaluate_CandidateComputeFailureDoesNotConstructApplyLoadOrForward()
    {
        using var fixture = new HeldOutEvaluatorFixture();
        var manifest = Manifest(Source("train", "training bytes"));
        var evaluationCase = ValidHeldCase();
        var lora = LoraApplicator.ValidateAndCanonicalize(
            IncrementalTrainer.DefaultConfig);
        var adapter = fixture.WriteAdapter(manifest, lora, fillAdapterParameters: true);
        var baselineEvaluator = fixture.CreateEvaluator(Provider(evaluationCase));
        var baseline = baselineEvaluator.Evaluate(BaselineRequest(manifest));
        Assert.True(baseline.IsSuccess, baseline.ErrorMessage);

        var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
        var inputCount = tokenizer.Encode(string.Concat(
            evaluationCase.Prompt,
            "\n",
            evaluationCase.ExpectedResponse)).Count - 1;
        var baseOnly = HeldOutLoraAccuracyEvaluator.EstimateCaseCompute(
            fixture.ModelConfig,
            inputCount,
            candidateLora: null);
        var phases = new List<HeldOutEvaluationProbePhase>();
        var evaluator = fixture.CreateEvaluator(
            Provider(evaluationCase),
            loraConfig: lora,
            limits: new HeldOutEvaluationLimits(
                MaxOperationBudget: baseOnly.Operations),
            phaseProbe: phases.Add);

        var result = evaluator.Evaluate(CandidateRequest(
            manifest,
            adapter,
            baseline.Value!.Artifacts));

        Assert.False(result.IsSuccess);
        Assert.Equal("HELD_OUT_COMPUTE_LIMIT_EXCEEDED", result.ErrorCode);
        Assert.Empty(phases);
    }

    [Fact]
    public void Evaluate_NonZeroCandidateFreezesEveryBaseAndLoraParameterBeforeForward()
    {
        using var fixture = new HeldOutEvaluatorFixture();
        var manifest = Manifest(Source("train", "training bytes"));
        var lora = LoraApplicator.ValidateAndCanonicalize(
            IncrementalTrainer.DefaultConfig);
        var adapter = fixture.WriteAdapter(manifest, lora, fillAdapterParameters: true);
        var baselineEvaluator = fixture.CreateEvaluator(Provider(ValidHeldCase()));
        var baseline = baselineEvaluator.Evaluate(BaselineRequest(manifest));
        Assert.True(baseline.IsSuccess, baseline.ErrorMessage);
        var observationCount = 0;
        var evaluator = fixture.CreateEvaluator(
            Provider(ValidHeldCase()),
            loraConfig: lora,
            beforeForwardModelObserver: model =>
            {
                observationCount++;
                var entries = model.GetCheckpointParameterEntries();
                Assert.Contains(entries, entry =>
                    entry.Key.Contains("lora_a", StringComparison.Ordinal));
                Assert.Contains(entries, entry =>
                    entry.Key.Contains("lora_b", StringComparison.Ordinal));
                Assert.All(entries, entry => Assert.False(entry.Parameter.Value.RequiresGrad));
            });

        var result = evaluator.Evaluate(CandidateRequest(
            manifest,
            adapter,
            baseline.Value!.Artifacts));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, observationCount);
    }

    [Fact]
    public void FreezingLoadedBaseAndLoraParametersPreservesExactLogits()
    {
        using var fixture = new HeldOutEvaluatorFixture();
        var manifest = Manifest(Source("train", "training bytes"));
        var lora = LoraApplicator.ValidateAndCanonicalize(
            IncrementalTrainer.DefaultConfig);
        var adapter = fixture.WriteAdapter(manifest, lora, fillAdapterParameters: true);
        var model = new LlmModel(fixture.ModelConfig);
        model.LoadCheckpoint(fixture.BaseCheckpointPath);
        LoraApplicator.Apply(model, lora);
        model.LoadAdapterCheckpoint(adapter.AdapterPath);
        var input = new Tensor([1f, 2f, 3f], new TensorShape(1, 3));

        var before = model.Forward(input, training: false).Data.ToArray();
        Assert.Contains(model.Parameters(), parameter => parameter.Value.RequiresGrad);
        foreach (var parameter in model.Parameters())
            parameter.Value.RequiresGrad = false;
        var after = model.Forward(input, training: false).Data;

        Assert.Equal(before, after);
    }

    [Fact]
    public void Evaluate_ExcessiveLoraRank_FailsBeforeApply()
    {
        using var fixture = new HeldOutEvaluatorFixture();
        var manifest = Manifest(Source("train", "training bytes"));
        var config = new LoraConfig
        {
            Rank = 4_097,
            Alpha = 8_194,
            Dropout = 0,
            TargetModules = ["q_proj"],
        };
        var adapter = fixture.WriteAdapter(manifest, config, fillAdapterParameters: false);
        var evaluator = fixture.CreateEvaluator(Provider(ValidHeldCase()), loraConfig: config);
        var baseline = evaluator.Evaluate(BaselineRequest(manifest));
        Assert.True(baseline.IsSuccess, baseline.ErrorMessage);

        var candidate = evaluator.Evaluate(CandidateRequest(
            manifest,
            adapter,
            baseline.Value!.Artifacts));

        Assert.False(candidate.IsSuccess);
        Assert.Equal("HELD_OUT_LORA_LIMIT_EXCEEDED", candidate.ErrorCode);
    }

    [Fact]
    public void Evaluate_ModelArithmeticOverflow_FailsBeforeModelAllocation()
    {
        using var fixture = new HeldOutEvaluatorFixture();
        var huge = new ModelConfig
        {
            VocabSize = int.MaxValue,
            ContextLen = 2,
            HiddenDim = int.MaxValue,
            NumLayers = int.MaxValue,
            NumHeads = 1,
            NumKvHeads = 1,
            FfnHiddenDim = int.MaxValue,
        };
        var evaluator = new HeldOutLoraAccuracyEvaluator(
            huge,
            fixture.BaseCheckpointPath,
            fixture.TokenizerPath,
            Provider(ValidHeldCase()),
            limits: new HeldOutEvaluationLimits(
                MaxVocabSize: int.MaxValue,
                MaxContextLength: int.MaxValue,
                MaxHiddenDimension: int.MaxValue,
                MaxLayers: int.MaxValue,
                MaxEstimatedParameters: long.MaxValue));

        var result = evaluator.Evaluate(
            BaselineRequest(Manifest(Source("train", "training bytes"))));

        Assert.False(result.IsSuccess);
        Assert.Equal("HELD_OUT_MODEL_LIMIT_EXCEEDED", result.ErrorCode);
    }

    [Fact]
    public void PathIdentityComparison_DetectsReplacementIdentity()
    {
        Assert.False(HeldOutLoraAccuracyEvaluator.PathIdentitiesEqualForTest(
            Path.GetFullPath(Path.Combine(Path.GetTempPath(), "original.bin")),
            Path.GetFullPath(Path.Combine(Path.GetTempPath(), "replacement.bin"))));
    }

    [Fact]
    public void Evaluate_UnsupportedOpenedIdentityFailsWithStableCode()
    {
        using var fixture = new HeldOutEvaluatorFixture();
        var evaluator = fixture.CreateEvaluator(
            Provider(ValidHeldCase()),
            identityResolver: _ => throw new PlatformNotSupportedException());

        var result = evaluator.Evaluate(
            BaselineRequest(Manifest(Source("train", "training bytes"))));

        Assert.False(result.IsSuccess);
        Assert.Equal("BASE_CHECKPOINT_IDENTITY_UNSUPPORTED", result.ErrorCode);
    }

    [Fact]
    public void Evaluate_InjectedOpenedIdentityReplacement_IsRejected()
    {
        using var fixture = new HeldOutEvaluatorFixture();
        var baseIdentityReads = 0;
        var evaluator = fixture.CreateEvaluator(
            Provider(ValidHeldCase()),
            identityResolver: stream =>
            {
                var canonical = Path.GetFullPath(stream.Name);
                if (!StringComparer.OrdinalIgnoreCase.Equals(
                        canonical,
                        fixture.BaseCheckpointPath))
                    return canonical;
                return Interlocked.Increment(ref baseIdentityReads) == 1
                    ? canonical
                    : string.Concat(canonical, ".replacement-identity");
            });

        var result = evaluator.Evaluate(
            BaselineRequest(Manifest(Source("train", "training bytes"))));

        Assert.False(result.IsSuccess);
        Assert.Equal("HELD_ARTIFACT_CHANGED", result.ErrorCode);
    }

    [Fact]
    public void Evaluate_LinuxRenameAndSameByteReplacement_IsRejected()
    {
        if (!OperatingSystem.IsLinux())
            return;

        using var fixture = new HeldOutEvaluatorFixture();
        var originalBytes = File.ReadAllBytes(fixture.BaseCheckpointPath);
        var renamedPath = Path.Combine(fixture.RootDirectory, "renamed-base.bin");
        var replaced = false;
        var evaluator = fixture.CreateEvaluator(
            Provider(ValidHeldCase()),
            phaseProbe: phase =>
            {
                if (phase != HeldOutEvaluationProbePhase.BeforeForward || replaced)
                    return;
                File.Move(fixture.BaseCheckpointPath, renamedPath);
                File.WriteAllBytes(fixture.BaseCheckpointPath, originalBytes);
                replaced = true;
            });

        var result = evaluator.Evaluate(
            BaselineRequest(Manifest(Source("train", "training bytes"))));

        Assert.False(result.IsSuccess);
        Assert.Equal("HELD_ARTIFACT_CHANGED", result.ErrorCode);
    }

    [Fact]
    public void Evaluate_PreCancelledRequest_PropagatesCancellation()
    {
        var evaluator = new HeldOutLoraAccuracyEvaluator(
            TinyConfig(),
            Path.Combine(Path.GetTempPath(), $"missing-base-{Guid.NewGuid():N}"),
            Path.Combine(Path.GetTempPath(), $"missing-tokenizer-{Guid.NewGuid():N}"),
            Provider(ValidHeldCase()));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            evaluator.Evaluate(
                BaselineRequest(Manifest(Source("train", "training bytes"))),
                cancellation.Token));
    }

    [Fact]
    public void ConfiguredProvider_SnapshotsCallerOwnedCases()
    {
        var cases = new List<HeldOutEvaluationCase> { ValidHeldCase() };
        var provider = new ConfiguredHeldOutEvaluationSetProvider(
        [
            new SourceDisjointHeldOutEvaluationSet("CODE_Q", cases),
        ]);

        cases.Clear();
        var result = provider.Get("CODE_Q");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var evaluationCase = Assert.Single(result.Value!.Cases);
        Assert.Equal("case-1", evaluationCase.CaseId);
    }

    private static AccuracyEvaluationRequest BaselineRequest(
        TrainingSourceManifest manifest)
        => new("CODE_Q", null, manifest, AccuracyEvaluationPhase.Baseline, null);

    private static AccuracyEvaluationRequest CandidateRequest(
        TrainingSourceManifest manifest,
        ModelAdapter adapter,
        AccuracyEvaluationArtifactIdentity baselineArtifacts)
        => new(
            "CODE_Q",
            adapter,
            manifest,
            AccuracyEvaluationPhase.Candidate,
            baselineArtifacts);

    private static HeldOutEvaluationCase ValidHeldCase()
        => new("case-1", Source("held", "held bytes"), "p", "r");

    private static ConfiguredHeldOutEvaluationSetProvider Provider(
        params HeldOutEvaluationCase[] cases)
        => new([new SourceDisjointHeldOutEvaluationSet("CODE_Q", cases)]);

    private static string Source(string origin, string content)
        => TrainingSourceProvenance.CreateSourceId(origin, content);

    private static TrainingSourceManifest Manifest(params string[] sources)
        => TrainingSourceProvenance.BuildManifest(sources).Value!;

    private static void AssertHeldAgainstReplacement(string path)
        => Assert.Throws<IOException>(() =>
        {
            using var _ = new FileStream(
                path,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None);
        });

    private static ModelConfig TinyConfig()
        => new()
        {
            VocabSize = 256,
            ContextLen = 32,
            HiddenDim = 8,
            NumLayers = 1,
            NumHeads = 2,
            NumKvHeads = 1,
            FfnHiddenDim = 16,
        };

    private sealed class HeldOutEvaluatorFixture : IDisposable
    {
        public string RootDirectory { get; }
        public string BaseCheckpointPath { get; }
        public string TokenizerPath { get; }
        public ModelConfig ModelConfig { get; } = TinyConfig();

        public HeldOutEvaluatorFixture()
        {
            RootDirectory = Path.Combine(
                Path.GetTempPath(),
                $"held-out-lora-evaluator-{Guid.NewGuid():N}");
            Directory.CreateDirectory(RootDirectory);
            BaseCheckpointPath = Path.GetFullPath(Path.Combine(RootDirectory, "base.bin"));
            TokenizerPath = Path.GetFullPath(Path.Combine(RootDirectory, "tokenizer.json"));

            new LlmModel(ModelConfig).SaveCheckpoint(BaseCheckpointPath);
            var tokenizer = new BpeTokenizer();
            tokenizer.Train("held out tokenizer corpus", 256);
            tokenizer.Save(TokenizerPath);
        }

        public HeldOutLoraAccuracyEvaluator CreateEvaluator(
            IHeldOutEvaluationSetProvider provider,
            LoraConfig? loraConfig = null,
            HeldOutEvaluationLimits? limits = null,
            Action<HeldOutEvaluationProbePhase>? phaseProbe = null,
            Func<FileStream, string>? identityResolver = null,
            Action<LlmModel>? beforeForwardModelObserver = null)
            => new(
                ModelConfig,
                BaseCheckpointPath,
                TokenizerPath,
                provider,
                loraConfig,
                limits,
                phaseProbe,
                identityResolver,
                beforeForwardModelObserver);

        public ModelAdapter WriteAdapter(
            TrainingSourceManifest manifest,
            LoraConfig config,
            bool fillAdapterParameters,
            float initialAdapterValue = 0.125f,
            float adapterValueIncrement = 0.0005f)
        {
            var model = new LlmModel(ModelConfig);
            model.LoadCheckpoint(BaseCheckpointPath);
            var canonical = LoraApplicator.ValidateAndCanonicalize(config);
            var adapters = LoraApplicator.Apply(model, canonical);
            if (fillAdapterParameters)
            {
                var value = initialAdapterValue;
                foreach (var (_, parameter) in adapters.SelectMany(
                             static adapter => adapter.AdapterParameters))
                {
                    for (var index = 0; index < parameter.Value.Data.Length; index++)
                    {
                        parameter.Value.Data[index] = value;
                        value += adapterValueIncrement;
                    }
                }
            }

            var path = Path.GetFullPath(Path.Combine(
                RootDirectory,
                $"adapter-{Guid.NewGuid():N}.lora"));
            model.SaveAdapterCheckpoint(path);
            return new ModelAdapter(
                "CODE_Q",
                path,
                manifest.SourceIds.Count,
                DateTimeOffset.UtcNow,
                new ModelAdapterProvenance(
                    manifest,
                    Hash(path),
                    Hash(BaseCheckpointPath),
                    Hash(TokenizerPath),
                    TrainingSourceProvenance.Identify(ModelConfig),
                    TrainingSourceProvenance.Identify(canonical)));
        }

        public HeldOutEvaluationCase? FindCaseWhereAdapterChangesPrediction(
            ModelAdapter adapter)
        {
            var tokenizer = BpeTokenizer.Load(TokenizerPath);
            var baseline = new LlmModel(ModelConfig);
            baseline.LoadCheckpoint(BaseCheckpointPath);
            var candidate = new LlmModel(ModelConfig);
            candidate.LoadCheckpoint(BaseCheckpointPath);
            LoraApplicator.Apply(candidate, IncrementalTrainer.DefaultConfig);
            candidate.LoadAdapterCheckpoint(adapter.AdapterPath);

            for (var character = 33; character <= 126; character++)
            {
                var prompt = ((char)character).ToString();
                var prefix = tokenizer.Encode(string.Concat(prompt, "\n"));
                var baselinePrediction = PredictNext(baseline, prefix);
                var candidatePrediction = PredictNext(candidate, prefix);
                if (candidatePrediction is < 33 or > 126
                    || candidatePrediction == baselinePrediction)
                    continue;

                return new HeldOutEvaluationCase(
                    "adapter-forward-case",
                    Source("held-forward", prompt),
                    prompt,
                    ((char)candidatePrediction).ToString());
            }

            return null;
        }

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
                Directory.Delete(RootDirectory, recursive: true);
        }

        private static string Hash(string path)
        {
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
            return TrainingSourceProvenance.Sha256(stream);
        }

        private static int PredictNext(LlmModel model, IReadOnlyList<int> prefix)
        {
            var input = new Tensor(
                prefix.Select(static token => (float)token).ToArray(),
                new TensorShape(1, prefix.Count));
            var logits = model.Forward(input, training: false);
            var offset = (prefix.Count - 1) * model.Config.VocabSize;
            var best = 0;
            for (var token = 1; token < model.Config.VocabSize; token++)
            {
                if (logits.Data[offset + token] > logits.Data[offset + best])
                    best = token;
            }
            return best;
        }
    }

    private sealed class DirectHeldOutProvider(SourceDisjointHeldOutEvaluationSet set)
        : IHeldOutEvaluationSetProvider
    {
        public OperationResult<SourceDisjointHeldOutEvaluationSet> Get(string domain)
            => OperationResult<SourceDisjointHeldOutEvaluationSet>.Ok(set);
    }

    private sealed class SingleEnumerationCases(HeldOutEvaluationCase evaluationCase)
        : IReadOnlyList<HeldOutEvaluationCase>
    {
        public int EnumerationCount { get; private set; }
        public int Count => 1;
        public HeldOutEvaluationCase this[int index]
            => index == 0
                ? evaluationCase
                : throw new ArgumentOutOfRangeException(nameof(index));

        public IEnumerator<HeldOutEvaluationCase> GetEnumerator()
        {
            EnumerationCount++;
            if (EnumerationCount > 1)
                throw new InvalidOperationException("CASES_ENUMERATED_MORE_THAN_ONCE");

            yield return evaluationCase;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
