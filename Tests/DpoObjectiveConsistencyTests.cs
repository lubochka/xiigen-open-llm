// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Text.Json;
using DPO;
using DPO.Models;
using LoRA;
using TensorCore.Core;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;
using TrainingLoop.Optimizers;

namespace LlmFromScratch.Tests;

// Numerical/recipient engineering fixtures only; no actual Student lesson or gain.
public sealed class DpoObjectiveConsistencyTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"dpo-objective-{Guid.NewGuid():N}");
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public DpoObjectiveConsistencyTests(Xunit.Abstractions.ITestOutputHelper output) => _output = output;

    [Theory]
    [InlineData(-100f)]
    [InlineData(-20f)]
    [InlineData(-1f)]
    [InlineData(0f)]
    [InlineData(1f)]
    [InlineData(20f)]
    [InlineData(100f)]
    public void ExactObjective_PreservesStableLossAndDerivativeIncludingRepresentableTails(float margin)
    {
        var (loss, derivative) = ExactObjective(margin);
        var expectedLoss = (float)IndependentSoftplus(margin);
        var expectedDerivative = (float)(-1d / (1d + Math.Exp(margin)));
        Assert.True(float.IsFinite(loss));
        Assert.True(float.IsFinite(derivative));
        Assert.True(loss > 0f);
        Assert.True(derivative < 0f);
        AssertRelative(expectedLoss, loss, 0.000002);
        AssertRelative(expectedDerivative, derivative, 0.000002);
        if (margin == -100f) Assert.Equal(100f, loss);
    }

    [Theory]
    [InlineData(-20f)]
    [InlineData(-1f)]
    [InlineData(0f)]
    [InlineData(1f)]
    [InlineData(20f)]
    public void ExactDerivative_MatchesIndependentLossFiniteDifference(float margin)
    {
        const double epsilon = 0.0001;
        var numerical = (IndependentSoftplus(margin + epsilon) - IndependentSoftplus(margin - epsilon)) / (2 * epsilon);
        AssertRelative(numerical, ExactObjective(margin).Derivative, 0.00001);
    }

    [Theory]
    [InlineData("Step", true)]
    [InlineData("StepMany", true)]
    [InlineData("StepVector", true)]
    [InlineData("EvaluateMany", true)]
    [InlineData("Step", false)]
    [InlineData("StepMany", false)]
    [InlineData("StepVector", false)]
    [InlineData("EvaluateMany", false)]
    public void ActualEntrypoints_SelectProfileObjectiveAndPersistIdentity(string entry, bool exact)
    {
        var batch = Batch("ab", "ccc", "d", exact);
        var policy = Model();
        var reference = Model();
        FillModel(policy, 17);
        FillModel(reference, 29);
        var margin = IndependentMargin(policy, reference, batch);
        var baseLoss = exact ? IndependentSoftplus(margin) : LegacyLoss(margin);
        var multiplier = entry == "StepVector" && exact ? 1.5d : 1d;
        var trainer = Trainer(policy, reference, entry + exact);
        var actual = entry switch
        {
            "Step" => trainer.Step(batch, 1),
            "StepMany" => trainer.StepMany([batch, batch], 1),
            "StepVector" => trainer.StepVector([new(batch, Enumerable.Repeat(1f, 30).ToArray())], 1),
            _ => trainer.EvaluateMany([batch, batch], 1)
        };
        Assert.Equal(margin, actual.RewardMargin, 5);
        Assert.Equal((float)(baseLoss * multiplier), actual.Loss, 5);
        Assert.Equal(exact ? DpoExactPairMaterial.ObjectiveVersion : null, actual.ObjectiveVersion);
        if (entry != "EvaluateMany")
        {
            var json = File.ReadAllText(Path.Combine(_root, entry + exact, DpoTrainer.BuildMetricsFileName(1)));
            var readback = JsonSerializer.Deserialize<DpoMetrics>(json)!;
            Assert.Equal(actual, readback);
            using var document = JsonDocument.Parse(json);
            Assert.Equal(exact, document.RootElement.TryGetProperty(nameof(DpoMetrics.ObjectiveVersion), out _));
        }
    }

    [Theory]
    [InlineData("StepMany")]
    [InlineData("StepVector")]
    [InlineData("EvaluateMany")]
    public void MixedProfiles_ReportNoHomogeneousVersionAndKeepEachProfileLoss(string entry)
    {
        var exact = Batch("ab", "ccc", "d", true);
        var legacy = Batch("ab", "ccc", "d", false);
        var policy = Model();
        var reference = Model();
        FillModel(policy, 17);
        FillModel(reference, 29);
        var exactLoss = IndependentSoftplus(IndependentMargin(policy, reference, exact));
        var legacyLoss = LegacyLoss(IndependentMargin(policy, reference, legacy));
        var trainer = Trainer(policy, reference, "mixed" + entry);
        var actual = entry switch
        {
            "StepMany" => trainer.StepMany([exact, legacy], 1),
            "StepVector" => trainer.StepVector([new(exact, Enumerable.Repeat(1f, 60).ToArray(), 1), new(legacy, [1f], 3)], 1),
            _ => trainer.EvaluateMany([exact, legacy], 1)
        };
        var expected = entry == "StepVector" ? 0.25 * 2 * exactLoss + 0.75 * legacyLoss : (exactLoss + legacyLoss) / 2;
        Assert.Equal((float)expected, actual.Loss, 5);
        Assert.Null(actual.ObjectiveVersion);
        if (entry != "EvaluateMany")
        {
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(_root, "mixed" + entry, DpoTrainer.BuildMetricsFileName(1))));
            Assert.False(document.RootElement.TryGetProperty(nameof(DpoMetrics.ObjectiveVersion), out _));
        }
    }

    [Fact]
    public void LegacyJsonAndPublicAverage_AreUnchanged()
    {
        const string oldJson = "{\"Step\":7,\"Loss\":0.25,\"ChosenReward\":0.5,\"RejectedReward\":-0.5,\"RewardMargin\":1,\"Accuracy\":1}";
        var metrics = JsonSerializer.Deserialize<DpoMetrics>(oldJson)!;
        Assert.Null(metrics.ObjectiveVersion);
        Assert.Equal(oldJson, JsonSerializer.Serialize(metrics));
        var model = Model();
        FillModel(model, 17);
        var batch = Batch("ab", "ccc", "d", true);
        var expected = IndependentSequenceScore(model, batch, chosen: true, responseOnly: false);
        Assert.Equal(expected, DpoTrainer.EvaluateAverageNextTokenLogProbability(model, batch.ChosenIds, batch.ChosenMask), 5);
    }

    [Fact]
    public void ExactCarrier_DigestCommitsTheNewObjectiveWhileMaterialAndFramingStayV2()
    {
        var batch = Batch("ab", "ccc", "d", true);
        Assert.Equal("segmented-byte-BPE-v2", batch.MaterialVersion);
        Assert.Equal("prompt-response-segments-no-separator-v2", DpoExactPairMaterial.FramingVersion);
        Assert.Equal("response-sum-softplus-signal-weighted-v3", DpoExactPairMaterial.ObjectiveVersion);
        // Independent reconstruction changes ONLY the old objective identity.
        Assert.Equal(DigestWithObjective(batch, DpoExactPairMaterial.ObjectiveVersion), batch.ExactMaterial!.DigestSha256);
        Assert.NotEqual(DigestWithObjective(batch, "response-target-logprob-sum-v2"), batch.ExactMaterial.DigestSha256);
    }

    [Fact]
    public void UnequalWeightsAndSignals_ScaleReportedLossAndActualOptimizerMomentsBySameFactor()
    {
        var (baselinePolicy, baselineReference, _) = LoraPair();
        var (weightedPolicy, weightedReference, _) = LoraPair();
        var batch = Batch("ab", "ccc", "ddd", true);
        var baselineTrainer = Trainer(baselinePolicy, baselineReference, "baseline");
        var weightedTrainer = Trainer(weightedPolicy, weightedReference, "weighted");
        var baseline = baselineTrainer.StepVector([new(batch, [])], 1);
        var weighted = weightedTrainer.StepVector([new(batch, [], 1f), new(batch, Enumerable.Repeat(1f, 60).ToArray(), 3f)], 1);
        const float multiplier = 0.25f * 1f + 0.75f * 2f;
        Assert.Equal(baseline.Loss * multiplier, weighted.Loss, 6);
        Assert.Equal(DpoExactPairMaterial.ObjectiveVersion, weighted.ObjectiveVersion);
        var first = baselineTrainer.ExportOptimizerState();
        var second = weightedTrainer.ExportOptimizerState();
        Assert.Equal(1, first.Step);
        Assert.Equal(1, second.Step);
        Assert.Equal(first.Entries.Select(x => x.Key), second.Entries.Select(x => x.Key));
        var nonzero = 0;
        foreach (var original in first.Entries)
        {
            var scaled = second.Entries.Single(x => x.Key == original.Key);
            for (var i = 0; i < original.FirstMoment.Count; i++)
            {
                if (MathF.Abs(original.FirstMoment[i]) > 1e-10f)
                {
                    nonzero++;
                    AssertRelative(original.FirstMoment[i] * multiplier, scaled.FirstMoment[i], 0.0001);
                    AssertRelative(original.SecondMoment[i] * multiplier * multiplier, scaled.SecondMoment[i], 0.0002);
                }
            }
        }
        Assert.True(nonzero > 0, "Actual AdamW first/second moments must expose a nonzero adapter gradient.");
    }

    [Fact]
    public void RealProjectLora_ImprovesChosenPreferenceAndChangesOnlyAllowedAdapterKeys()
    {
        var (policy, reference, adapters) = LoraPair();
        var batch = Batch("ab", "ccc", "ddd", true);
        var entries = policy.GetCheckpointParameterEntries();
        var allowed = adapters.SelectMany(adapter => adapter.AdapterParameters.Select(x => x.Parameter)).ToHashSet();
        var allowedKeys = entries.Where(x => allowed.Contains(x.Parameter)).Select(x => x.Key).ToHashSet(StringComparer.Ordinal);
        Assert.Equal(4, allowedKeys.Count);
        Assert.All(entries, entry => Assert.Equal(allowedKeys.Contains(entry.Key), entry.Parameter.Value.RequiresGrad));
        Assert.All(reference.Parameters(), parameter => Assert.False(parameter.Value.RequiresGrad));
        foreach (var entry in entries)
            Assert.NotSame(entry.Parameter.Value.Data, reference.GetCheckpointParameterEntries().Single(x => x.Key == entry.Key).Parameter.Value.Data);
        var before = Snapshot(policy);
        var referenceBefore = Snapshot(reference);
        var beforeMargin = IndependentMargin(policy, reference, batch);
        var trainer = Trainer(policy, reference, "real-lora");
        var metrics = trainer.StepVector([new(batch, [1f])], 1);
        var afterMargin = IndependentMargin(policy, reference, batch);
        Assert.True(afterMargin > beforeMargin, $"Chosen relative preference must increase: before={beforeMargin:R}, after={afterMargin:R}.");
        var after = Snapshot(policy);
        var changed = before.Keys.Where(key => !before[key].SequenceEqual(after[key])).OrderBy(x => x).ToArray();
        Assert.NotEmpty(changed);
        Assert.All(changed, key => Assert.Contains(key, allowedKeys));
        foreach (var key in before.Keys.Where(key => !allowedKeys.Contains(key))) Assert.Equal(before[key], after[key]);
        foreach (var (key, bits) in referenceBefore) Assert.Equal(bits, Snapshot(reference)[key]);
        Assert.All(trainer.ExportOptimizerState().Entries, entry => Assert.Contains(entry.Key, allowedKeys));
        Assert.Equal(DpoExactPairMaterial.ObjectiveVersion, metrics.ObjectiveVersion);
        // Test receipts identify exact changing keys and direction, not scientific learning.
        _output.WriteLine(JsonSerializer.Serialize(new
        { beforeMargin, afterMargin, allowedKeys = allowedKeys.OrderBy(x => x).ToArray(), changedKeys = changed, baseUnchanged = true, referenceUnchanged = true }));
    }

    private DpoTrainer Trainer(LlmModel policy, LlmModel reference, string name)
        => new(policy, reference, Path.Combine(_root, name), DpoMutationAdmissionCapability.Issue(new TestLease()));

    private static (LlmModel Policy, LlmModel Reference, IReadOnlyList<LoraLinear> Adapters) LoraPair()
    {
        var policy = Model();
        var reference = Model();
        FillModel(policy, 41);
        foreach (var entry in policy.GetCheckpointParameterEntries())
            Array.Copy(entry.Parameter.Value.Data, reference.GetCheckpointParameterEntries().Single(x => x.Key == entry.Key).Parameter.Value.Data, entry.Parameter.Value.Data.Length);
        var config = new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] };
        var pair = LoraApplicator.ApplyFreshSeededPair(policy, reference, config, 941, "objective-policy", "objective-reference");
        // Explicitly match/freeze independent reference storage as the existing logical consumer does.
        foreach (var entry in policy.GetCheckpointParameterEntries())
        {
            var target = reference.GetCheckpointParameterEntries().Single(x => x.Key == entry.Key).Parameter;
            Array.Copy(entry.Parameter.Value.Data, target.Value.Data, target.Value.Data.Length);
            target.Value.RequiresGrad = false;
        }
        return (policy, reference, pair.FirstModelAdapters);
    }

    private static LlmModel Model()
        => new(new ModelConfig { VocabSize = 256, ContextLen = 8, HiddenDim = 8, NumLayers = 1, NumHeads = 2, NumKvHeads = 1, FfnHiddenDim = 16 });
    private static void FillModel(LlmModel model, int seed)
    {
        var random = new Random(seed);
        foreach (var parameter in model.Parameters())
            for (var i = 0; i < parameter.Value.Data.Length; i++)
                parameter.Value.Data[i] = parameter.Value.Shape.Rank == 1 ? 1f : (float)(random.NextDouble() - 0.5) * 0.5f;
    }
    private static DpoBatch Batch(string prompt, string chosen, string rejected, bool exact)
    {
        var tokenizer = new BpeTokenizer(); tokenizer.Train("fixture", 256);
        var admission = DpoTrainerPairCustoms.CreateAdmitted(prompt, chosen, rejected,
            DpoPairSideAuthorKind.DeterministicGenerator, DpoPairSideAuthorKind.DeterministicGenerator, "objective-test").Admission;
        var batch = DpoExactPairMaterial.CreateBatch(tokenizer, prompt, chosen, rejected, admission);
        return exact ? batch : batch with
        { MaterialVersion = DpoExactPairMaterial.LegacyVersion, ExactMaterial = null, ChosenResponseMask = null, RejectedResponseMask = null };
    }
    private static Dictionary<string, int[]> Snapshot(LlmModel model)
        => model.GetCheckpointParameterEntries().ToDictionary(x => x.Key, x => x.Parameter.Value.Data.Select(BitConverter.SingleToInt32Bits).ToArray(), StringComparer.Ordinal);
    private static float IndependentMargin(LlmModel policy, LlmModel reference, DpoBatch batch)
    {
        var responseOnly = batch.MaterialVersion == DpoExactPairMaterial.Version;
        return 0.1f * ((IndependentSequenceScore(policy, batch, true, responseOnly) - IndependentSequenceScore(reference, batch, true, responseOnly))
            - (IndependentSequenceScore(policy, batch, false, responseOnly) - IndependentSequenceScore(reference, batch, false, responseOnly)));
    }
    private static float IndependentSequenceScore(LlmModel model, DpoBatch batch, bool chosen, bool responseOnly)
    {
        var ids = chosen ? batch.ChosenIds : batch.RejectedIds;
        var real = chosen ? batch.ChosenMask : batch.RejectedMask;
        var start = responseOnly ? batch.ExactMaterial!.PromptLength : 1;
        var end = (int)real.Data.Sum();
        var logits = model.Forward(ids, training: false).Data;
        double result = 0;
        for (var j = start; j < end; j++)
        {
            var offset = (j - 1) * model.Config.VocabSize;
            var maximum = logits.Skip(offset).Take(model.Config.VocabSize).Max();
            var denominator = logits.Skip(offset).Take(model.Config.VocabSize).Sum(x => Math.Exp(x - maximum));
            result += logits[offset + (int)ids.Data[j]] - maximum - Math.Log(denominator);
        }
        return (float)(responseOnly ? result : result / (end - start));
    }
    private static double IndependentSoftplus(double margin)
    {
        var z = Math.Exp(-Math.Abs(margin));
        // Independent alternating Taylor reference for tails that 1+z loses.
        var correction = z < 0.001 ? z - z * z / 2 + z * z * z / 3 - Math.Pow(z, 4) / 4 : Math.Log(1 + z);
        return Math.Max(0, -margin) + correction;
    }
    private static float LegacyLoss(float margin) => -MathF.Log(1f / (1f + MathF.Exp(-margin)) + 1e-8f);
    private static (float Loss, float Derivative) ExactObjective(float margin)
        => ((float, float))typeof(DpoTrainer).GetMethod("ExactObjective", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [margin])!;
    private static void AssertRelative(double expected, double actual, double tolerance)
        => Assert.True(Math.Abs(actual - expected) / Math.Max(Math.Abs(expected), (double)float.Epsilon) <= tolerance,
            $"expected={expected:R}, actual={actual:R}, tolerance={tolerance}");
    private static string DigestWithObjective(DpoBatch batch, string objective)
    {
        var m = batch.ExactMaterial!;
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, new System.Text.UTF8Encoding(false, true), true))
        {
            foreach (var text in new[] { DpoExactPairMaterial.Version, DpoExactPairMaterial.FramingVersion, objective,
                m.Prompt, m.Chosen, m.Rejected, m.AdmissionFingerprintSha256, m.TokenizerSha256 }) writer.Write(text);
            writer.Write(m.TokenizerVocabSize); writer.Write(m.PromptLength); writer.Write(m.ChosenLength); writer.Write(m.RejectedLength); writer.Write(m.SequenceLength);
            foreach (var tensor in new[] { batch.ChosenIds, batch.RejectedIds, batch.ChosenMask, batch.RejectedMask, batch.ChosenResponseMask!, batch.RejectedResponseMask! })
            { writer.Write(tensor.Data.Length); foreach (var value in tensor.Data) writer.Write(value); }
        }
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream.ToArray())).ToLowerInvariant();
    }
    private sealed class TestLease : IDpoMutationAdmissionIssuer
    {
        public IDisposable AcquireDpoTrainerMutationPermit() => new CancellationTokenSource();
        public void Dispose() { }
    }
    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true); }
}
