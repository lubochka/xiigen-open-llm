// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using DPO;
using DPO.Models;
using TransformerModel;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

// Unit tests for DpoTrainer — step output, metric correctness, atomic checkpoint.
// Behavioral assertion gate: DpoMetrics fields are checked for concrete properties,
// not just "no exception thrown".
public class DpoTrainerTests : IDisposable
{
    // Minimal model config: tiny vocab, 1 layer, 16-dim — creates fast in tests
    private static readonly ModelConfig SmallCfg = new()
    {
        VocabSize    = 64,
        ContextLen   = 8,
        HiddenDim    = 16,
        NumLayers    = 1,
        NumHeads     = 2,
        NumKvHeads   = 1,
        FfnHiddenDim = 32
    };

    private readonly string _checkpointDir = Path.Combine(Path.GetTempPath(), $"dpo-test-{Guid.NewGuid()}");

    public void Dispose()
    {
        if (Directory.Exists(_checkpointDir))
            Directory.Delete(_checkpointDir, recursive: true);
    }

    // Build a minimal DpoBatch with the given token IDs (all 1 batch × seqLen)
    private static DpoBatch MakeBatch(int[] chosenIds, int[] rejectedIds)
    {
        float[] cIds  = chosenIds.Select(i => (float)i).ToArray();
        float[] rIds  = rejectedIds.Select(i => (float)i).ToArray();
        float[] cMask = new float[chosenIds.Length];
        float[] rMask = new float[rejectedIds.Length];
        Array.Fill(cMask,  1f);
        Array.Fill(rMask,  1f);

        return new DpoBatch
        {
            ChosenIds    = new Tensor(cIds,  new TensorShape(1, chosenIds.Length)),
            RejectedIds  = new Tensor(rIds,  new TensorShape(1, rejectedIds.Length)),
            ChosenMask   = new Tensor(cMask, new TensorShape(1, chosenIds.Length)),
            RejectedMask = new Tensor(rMask, new TensorShape(1, rejectedIds.Length)),
        };
    }

    // ──────────────────────────────────────────────────────────────────────────
    // DpoMetrics field contracts
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Step_ReturnsMetricsWithCorrectStep()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = new DpoTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 1, 2, 3, 4 }, new[] { 1, 5, 6, 4 });

        var metrics = trainer.Step(batch, step: 7);

        Assert.Equal(7, metrics.Step);
    }

    [Fact]
    public void Step_LossIsPositive()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = new DpoTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 1, 2, 3 }, new[] { 1, 5, 3 });

        var metrics = trainer.Step(batch, step: 0);

        // DPO loss = -log σ(margin) which is always in (0, log2] for any real margin
        Assert.True(metrics.Loss > 0f, $"Loss should be positive, got {metrics.Loss}");
        Assert.False(float.IsNaN(metrics.Loss), "Loss must not be NaN");
    }

    [Fact]
    public void Step_AccuracyIsBinary()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = new DpoTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 1, 2, 3 }, new[] { 1, 5, 3 });

        var metrics = trainer.Step(batch, step: 0);

        // Accuracy is 1.0 if margin>0 else 0.0 — never anything else
        Assert.True(metrics.Accuracy is 0f or 1f,
            $"Accuracy must be 0 or 1, got {metrics.Accuracy}");
    }

    [Fact]
    public void Step_RewardMargin_MatchesChosenMinusRejected()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = new DpoTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 2, 3 }, new[] { 5, 6 });

        var metrics = trainer.Step(batch, step: 0);

        float expected = metrics.ChosenReward - metrics.RejectedReward;
        Assert.Equal(expected, metrics.RewardMargin, precision: 5);
    }

    [Fact]
    public void Step_AccuracyConsistentWithMargin()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = new DpoTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 1, 2, 3 }, new[] { 4, 5, 6 });

        var metrics = trainer.Step(batch, step: 0);

        // Accuracy = 1 iff margin > 0 (by definition in DpoTrainer)
        if (metrics.RewardMargin > 0f)
            Assert.Equal(1f, metrics.Accuracy);
        else
            Assert.Equal(0f, metrics.Accuracy);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Atomic checkpoint (DNA rule 8 + DNA rule 7)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Step_WritesMetricsFileAtomically()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = new DpoTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 1, 2 }, new[] { 3, 4 });

        trainer.Step(batch, step: 42);

        // Metrics file must exist, .tmp must be gone
        string metricsPath = Path.Combine(_checkpointDir, "dpo_step_000042.json");
        Assert.True(File.Exists(metricsPath), "Metrics file must be written");
        Assert.False(File.Exists(metricsPath + ".tmp"), ".tmp file must not be left behind");
    }

    [Fact]
    public void Step_MetricsFileContainsValidJson()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = new DpoTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 1, 2 }, new[] { 3, 4 });

        var metrics = trainer.Step(batch, step: 1);
        string json = File.ReadAllText(Path.Combine(_checkpointDir, "dpo_step_000001.json"));

        // Must deserialize without error and match the returned metrics
        var loaded = System.Text.Json.JsonSerializer.Deserialize<DpoMetrics>(json)!;
        Assert.Equal(metrics.Step, loaded.Step);
        Assert.Equal(metrics.Loss, loaded.Loss, precision: 5);
    }

    [Fact]
    public void Step_WritesLoadablePolicyCheckpointAtomically()
    {
        var model = new LlmModel(SmallCfg);
        var refMdl = new LlmModel(SmallCfg);
        SetDeterministicWeights(model);
        CopyWeights(model, refMdl);
        var trainer = new DpoTrainer(model, refMdl, _checkpointDir);
        var batch = MakeBatch(new[] { 1, 2, 3, 4 }, new[] { 1, 5, 6, 4 });

        trainer.Step(batch, step: 42);

        var checkpointPath = Path.Combine(_checkpointDir, DpoTrainer.BuildPolicyCheckpointFileName(42));
        Assert.True(File.Exists(checkpointPath), "Policy checkpoint must be written by the real DPO trainer.");
        Assert.False(File.Exists(checkpointPath + ".tmp"), ".tmp policy checkpoint must not be left behind.");

        var loaded = new LlmModel(SmallCfg);
        loaded.LoadCheckpoint(checkpointPath);

        AssertModelParametersEqual(model, loaded);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Multiple steps — model updates
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void MultipleSteps_DoNotThrow()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = new DpoTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 1, 2, 3 }, new[] { 4, 5, 6 });

        var ex = Record.Exception(() =>
        {
            for (int i = 0; i < 3; i++)
                trainer.Step(batch, step: i);
        });

        Assert.Null(ex);
    }

    [Fact]
    public void MultipleSteps_EachWritesOwnMetricsFile()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = new DpoTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 1, 2 }, new[] { 3, 4 });

        for (int i = 0; i < 3; i++)
            trainer.Step(batch, step: i);

        // One metrics file per step
        var files = Directory.GetFiles(_checkpointDir, "*.json");
        Assert.Equal(3, files.Length);
    }

    [Fact]
    public void StepMany_WithCopiedReferenceImprovesRewardMarginOnRepeatedPreference()
    {
        var model = new LlmModel(SmallCfg);
        var refMdl = new LlmModel(SmallCfg);
        SetDeterministicWeights(model);
        CopyWeights(model, refMdl);
        var trainer = new DpoTrainer(model, refMdl, _checkpointDir, learningRate: 5e-2f);
        var batch = MakeBatch(
            new[] { 1, 2, 3, 4, 5, 6, 7, 8 },
            new[] { 1, 9, 10, 11, 12, 13, 14, 8 });

        var initial = trainer.EvaluateMany([batch], step: 0);
        for (var step = 1; step <= 40; step++)
            trainer.StepMany([batch], step);
        var trained = trainer.EvaluateMany([batch], step: 41);

        Assert.Equal(0f, initial.RewardMargin, precision: 5);
        Assert.True(
            trained.RewardMargin > initial.RewardMargin + 1e-5f,
            $"Expected reward margin to improve from {initial.RewardMargin}, got {trained.RewardMargin}.");
    }

    private static void AssertModelParametersEqual(LlmModel expected, LlmModel actual)
    {
        var expectedParameters = expected.Parameters().ToArray();
        var actualParameters = actual.Parameters().ToArray();
        Assert.Equal(expectedParameters.Length, actualParameters.Length);

        for (var parameterIndex = 0; parameterIndex < expectedParameters.Length; parameterIndex++)
        {
            var expectedData = expectedParameters[parameterIndex].Value.Data;
            var actualData = actualParameters[parameterIndex].Value.Data;
            Assert.Equal(expectedData.Length, actualData.Length);

            for (var valueIndex = 0; valueIndex < expectedData.Length; valueIndex++)
                Assert.Equal(expectedData[valueIndex], actualData[valueIndex]);
        }
    }

    private static void SetDeterministicWeights(LlmModel model)
    {
        var rng = new Random(42);
        var parameters = model.Parameters().ToArray();
        FillNormal(parameters[0].Value.Data, rng, std: 0.02f);
        FillOnes(parameters[1].Value.Data);
        FillKaiming(parameters[2].Value.Data, rng, parameters[2].Value.Shape.Dimensions[1]);
        FillKaiming(parameters[3].Value.Data, rng, parameters[3].Value.Shape.Dimensions[1]);
        FillKaiming(parameters[4].Value.Data, rng, parameters[4].Value.Shape.Dimensions[1]);
        FillKaiming(parameters[5].Value.Data, rng, parameters[5].Value.Shape.Dimensions[1]);
        FillOnes(parameters[6].Value.Data);
        FillKaiming(parameters[7].Value.Data, rng, parameters[7].Value.Shape.Dimensions[1]);
        FillKaiming(parameters[8].Value.Data, rng, parameters[8].Value.Shape.Dimensions[1]);
        FillKaiming(parameters[9].Value.Data, rng, parameters[9].Value.Shape.Dimensions[1]);
        FillOnes(parameters[10].Value.Data);
        FillKaiming(parameters[11].Value.Data, rng, parameters[11].Value.Shape.Dimensions[1]);
    }

    private static void FillKaiming(float[] data, Random rng, int fanIn)
    {
        var bound = MathF.Sqrt(6.0f / fanIn);
        for (var i = 0; i < data.Length; i++)
            data[i] = (float)(rng.NextDouble() * 2 * bound - bound);
    }

    private static void FillNormal(float[] data, Random rng, float std)
    {
        for (var i = 0; i < data.Length; i += 2)
        {
            var u1 = 1.0 - rng.NextDouble();
            var u2 = 1.0 - rng.NextDouble();
            var r = Math.Sqrt(-2.0 * Math.Log(u1));
            var theta = 2.0 * Math.PI * u2;

            data[i] = (float)(r * Math.Cos(theta) * std);
            if (i + 1 < data.Length)
                data[i + 1] = (float)(r * Math.Sin(theta) * std);
        }
    }

    private static void FillOnes(float[] data)
    {
        Array.Fill(data, 1.0f);
    }

    private static void CopyWeights(LlmModel source, LlmModel target)
    {
        var sourceParameters = source.Parameters().ToArray();
        var targetParameters = target.Parameters().ToArray();
        for (var parameterIndex = 0; parameterIndex < Math.Min(sourceParameters.Length, targetParameters.Length); parameterIndex++)
        {
            var sourceData = sourceParameters[parameterIndex].Value.Data;
            var targetData = targetParameters[parameterIndex].Value.Data;
            Array.Copy(sourceData, targetData, Math.Min(sourceData.Length, targetData.Length));
        }
    }
}
