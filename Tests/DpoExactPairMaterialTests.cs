// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Security.Cryptography;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO;
using DPO.Models;
using LoRA;
using TensorCore.Core;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

// Isolated engineering invariants; these synthetic pairs do not measure Student learning.
public sealed class DpoExactPairMaterialTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"dpo-exact-{Guid.NewGuid():N}");

    [Fact]
    public void Preparation_PreservesWhitespaceUnicodeCrlfAndContrastBeyond480()
    {
        var tokenizer = Tokenizer();
        var prompt = "  literal\t\r\n\u4e2d\u6587 e\u0301 \U0001f642  ";
        var common = new string('q', 501);
        var chosen = common + " \"a  b\"\r\n";
        var rejected = common + " \"a b\"\r\n";
        var prepared = Prepare(tokenizer, prompt, chosen, rejected);
        var batch = Assert.Single(prepared.OptimizerInputs).Batch.Batch;
        var material = Assert.IsType<DpoExactPairMaterial>(batch.ExactMaterial);
        Assert.Equal(prompt, material.Prompt);
        Assert.Equal(chosen, material.Chosen);
        Assert.Equal(rejected, material.Rejected);
        Assert.Equal(tokenizer.Encode(prompt).Count, material.PromptLength);
        Assert.Equal(prompt + chosen, DecodeReal(tokenizer, batch.ChosenIds, material.ChosenLength));
        Assert.Equal(prompt + rejected, DecodeReal(tokenizer, batch.RejectedIds, material.RejectedLength));
        Assert.NotEqual(batch.ChosenIds.Data, batch.RejectedIds.Data);
        Assert.Equal(tokenizer.ComputeStateSha256(), material.TokenizerSha256);
        Assert.Equal(64, material.DigestSha256.Length);
    }

    [Fact]
    public void Segmentation_DoesNotMergeAcrossAssessedBoundaryOrAddSeparator()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("abab abab abab abab", 260);
        var batch = Batch(tokenizer, "a", "b", "c");
        var segmented = tokenizer.Encode("a").Concat(tokenizer.Encode("b")).ToArray();
        Assert.NotEqual(tokenizer.Encode("ab").ToArray(), segmented);
        Assert.Equal(segmented.Select(x => (float)x), batch.ChosenIds.Data);
        Assert.Equal("ab", DecodeReal(tokenizer, batch.ChosenIds, batch.ExactMaterial!.ChosenLength));
    }

    [Theory]
    [InlineData("a", "b", "cd")]
    [InlineData("ab", "cde", "f")]
    [InlineData("x", " ", "  ")]
    public void Masks_IncludeFirstAndLastResponseExcludePromptAndRightPadding(string prompt, string chosen, string rejected)
    {
        var batch = Batch(Tokenizer(), prompt, chosen, rejected);
        var material = batch.ExactMaterial!;
        AssertMasks(batch.ChosenMask, batch.ChosenResponseMask!, material.PromptLength, material.ChosenLength);
        AssertMasks(batch.RejectedMask, batch.RejectedResponseMask!, material.PromptLength, material.RejectedLength);
        Assert.Equal(1f, batch.ChosenResponseMask!.Data[material.PromptLength]);
        Assert.Equal(1f, batch.ChosenResponseMask.Data[material.ChosenLength - 1]);
        Assert.Equal(1f, batch.RejectedResponseMask!.Data[material.RejectedLength - 1]);
    }

    [Fact]
    public void Commitment_IsStableAndBindsRawSegmentsAdmissionAndTokenizer()
    {
        var tokenizer = Tokenizer();
        var batch = Batch(tokenizer, "a", "b", "c");
        Assert.Equal(batch.ExactMaterial!.DigestSha256, Batch(tokenizer, "a", "b", "c").ExactMaterial!.DigestSha256);
        Assert.NotEqual(batch.ExactMaterial.DigestSha256, Batch(tokenizer, "a ", "b", "c").ExactMaterial!.DigestSha256);
        var differentTokenizer = new BpeTokenizer();
        differentTokenizer.Train("xyxy xyxy", 258);
        Assert.NotEqual(batch.ExactMaterial.DigestSha256, Batch(differentTokenizer, "a", "b", "c").ExactMaterial!.DigestSha256);
        var foreign = DpoTrainerPairCustoms.CreateAdmitted("foreign", "b", "c",
            DpoPairSideAuthorKind.DeterministicGenerator, DpoPairSideAuthorKind.DeterministicGenerator, "test");
        AssertInvalid(() => DpoExactPairMaterial.CaptureAndValidate(batch with { Admission = foreign.Admission }, 512, 8));
        AssertInvalid(() => DpoExactPairMaterial.CaptureAndValidate(batch, 512, 8, differentTokenizer.ComputeStateSha256()));
    }

    public static IEnumerable<object[]> InvalidRawPairs()
    {
        yield return ["", "b", "c"];
        yield return ["a", "", "c"];
        yield return ["a", "b", ""];
        yield return ["a", "same", "same"];
        yield return ["\ud800", "b", "c"];
        yield return ["a", "\udc00", "c"];
        yield return ["a", "b", "\ud800"];
    }

    [Theory]
    [MemberData(nameof(InvalidRawPairs))]
    public void Preparation_RejectsEmptyMalformedUtf16OrCollapsedContrast(string prompt, string chosen, string rejected)
    {
        var result = LogicalDpoTrainingUnit.PrepareExecution([Preference(prompt, chosen, rejected)], Tokenizer(), DpoExactPairMaterial.Version);
        Assert.False(result.IsSuccess);
        Assert.Equal(DpoExactPairMaterial.InvalidErrorCode, result.ErrorCode);
        Assert.False(Directory.Exists(_root));
    }

    [Fact]
    public void Preparation_RejectsTokenizerWithoutSegmentRoundtrip()
    {
        AssertInvalid(() => Batch(new BpeTokenizer(), "a", "b", "c"));
    }

    [Fact]
    public void Capacity_ExactBoundaryAcceptedAndBothModelLimitsEnforced()
    {
        var batch = Batch(Tokenizer(), "ab", "cde", "f");
        Assert.Equal(5, DpoExactPairMaterial.CaptureAndValidate(batch, 256, 5).ExactMaterial!.SequenceLength);
        var capacity = Assert.Throws<DpoMutationAdmissionException>(() => DpoExactPairMaterial.CaptureAndValidate(batch, 256, 4));
        Assert.Equal(DpoExactPairMaterial.CapacityErrorCode, capacity.ErrorCode);
        AssertInvalid(() => DpoExactPairMaterial.CaptureAndValidate(batch, (int)'f', 5));
        // Reference capacity is lower than policy capacity: rejection must precede any forward.
        var trainer = new DpoTrainer(Model(context: 8), Model(context: 4), _root);
        Assert.Equal(DpoExactPairMaterial.CapacityErrorCode,
            Assert.Throws<DpoMutationAdmissionException>(() => trainer.EvaluateMany([batch], 1)).ErrorCode);
    }

    [Theory]
    [InlineData("Step")]
    [InlineData("StepMany")]
    [InlineData("StepVector")]
    [InlineData("EvaluateMany")]
    public void InvalidMaterial_FailsBeforePermitWeightsAndFilesOnEveryEntry(string entry)
    {
        var policy = Model();
        var reference = Model();
        var lease = new TestLease();
        var trainer = DpoTrainer.CreateForPreparedContinuation(policy, reference, _root,
            DpoMutationAdmissionCapability.Issue(lease), 1e-5f, null, null, null);
        lease.Requests = 0;
        var beforePolicy = ModelHash(policy);
        var beforeReference = ModelHash(reference);
        foreach (var invalid in InvalidBatches())
        {
            var exception = Assert.Throws<DpoMutationAdmissionException>(() => Invoke(trainer, entry, invalid));
            Assert.Contains(exception.ErrorCode, new[] { DpoExactPairMaterial.InvalidErrorCode, DpoExactPairMaterial.CapacityErrorCode });
            Assert.Equal(0, lease.Requests);
            Assert.Equal(beforePolicy, ModelHash(policy));
            Assert.Equal(beforeReference, ModelHash(reference));
            Assert.False(Directory.Exists(_root));
        }
    }

    [Fact]
    public void StepVector_NullElementRetainsArgumentNullContractBeforePermit()
    {
        var lease = new TestLease();
        var trainer = DpoTrainer.CreateForPreparedContinuation(Model(), Model(), _root,
            DpoMutationAdmissionCapability.Issue(lease), 1e-5f, null, null, null);
        lease.Requests = 0;
        Assert.Throws<ArgumentNullException>(() => trainer.StepVector([null!], 1));
        Assert.Equal(0, lease.Requests);
        Assert.False(Directory.Exists(_root));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void ResponseSumAndBackward_MatchIndependentFiniteDifferencesWithPromptAndPaddingExcluded(int responseCount)
    {
        var logits = new Tensor(new float[] { 4, 1, -2, 0.4f, -0.3f, 0.1f, -0.2f, 0.8f, 0.3f, 9, 1, -1, 0, 0, 0 }, new TensorShape(1, 5, 3), true);
        var ids = new Tensor(new float[] { 0, 0, 1, 2, 0 }, new TensorShape(1, 5));
        var mask = new Tensor(new float[] { 0, 0, 1, responseCount == 2 ? 1 : 0, 0 }, new TensorShape(1, 5));
        var targets = new (int Row, int Token)[] { (1, 1), (2, 2) }.Take(responseCount).ToArray();
        var expected = IndependentScore(logits.Data, 3, targets);
        var score = Score(logits, ids, mask, sum: true);
        Assert.Equal(expected, score, 5);
        Assert.Equal(expected / responseCount, Score(logits, ids, mask, sum: false), 5);
        const float upstream = -0.37f;
        Backward(logits, ids, mask, score, sum: true, upstream);
        for (var index = 0; index < logits.Data.Length; index++)
        {
            var original = logits.Data[index];
            logits.Data[index] = original + 0.001f;
            var plus = IndependentScore(logits.Data, 3, targets);
            logits.Data[index] = original - 0.001f;
            var minus = IndependentScore(logits.Data, 3, targets);
            logits.Data[index] = original;
            Assert.InRange(Math.Abs(logits.Grad!.Data[index] - upstream * (plus - minus) / 0.002f), 0, 0.0002f);
        }
        var sumGradient = logits.Grad!.Data.ToArray();
        logits.Grad = null;
        Backward(logits, ids, mask, score / responseCount, sum: false, upstream);
        for (var index = 0; index < sumGradient.Length; index++)
            Assert.Equal(sumGradient[index] / responseCount, logits.Grad!.Data[index], 6);
        Assert.Contains(sumGradient.Skip(3).Take(3), value => value != 0f); // first response
        Assert.Contains(sumGradient.Skip(responseCount * 3).Take(3), value => value != 0f); // last response
        Assert.All(sumGradient.Take(3).Concat(sumGradient.Skip((responseCount + 1) * 3)), value => Assert.Equal(0f, value));
    }

    [Fact]
    public void EvaluateMany_ConsumesResponseSumWhilePublicAverageRemainsLegacyMean()
    {
        var batch = Batch(Tokenizer(), "ab", "cde", "f");
        var policy = Model();
        var reference = Model();
        FillModel(policy, 17);
        FillModel(reference, 29);
        var expectedChosen = 0.1f * (ModelScore(policy, batch, true) - ModelScore(reference, batch, true));
        var expectedRejected = 0.1f * (ModelScore(policy, batch, false) - ModelScore(reference, batch, false));
        var trainer = new DpoTrainer(policy, reference, _root);
        var actual = trainer.EvaluateMany([batch], 3);
        Assert.Equal(expectedChosen, actual.ChosenReward, 5);
        Assert.Equal(expectedRejected, actual.RejectedReward, 5);
        var fullLogits = policy.Forward(batch.ChosenIds, training: false);
        var expectedAverage = IndependentScore(fullLogits.Data, policy.Config.VocabSize,
            Enumerable.Range(1, batch.ExactMaterial!.ChosenLength - 1).Select(j => (j - 1, (int)batch.ChosenIds.Data[j])).ToArray())
            / (batch.ExactMaterial.ChosenLength - 1);
        Assert.Equal(expectedAverage, DpoTrainer.EvaluateAverageNextTokenLogProbability(policy, batch.ChosenIds, batch.ChosenMask), 5);
        Assert.False(Directory.Exists(_root));
    }

    [Theory]
    [InlineData("Step")]
    [InlineData("StepMany")]
    [InlineData("StepVector")]
    public void Trainer_CapturesTensorMasksVectorAndListBeforePermitCallback(string entry)
    {
        var source = Batch(Tokenizer(), "ab", "cde", "f");
        var policy = Model();
        var reference = Model();
        FillModel(policy, 17);
        FillModel(reference, 29);
        var referenceBefore = ModelHash(reference);
        var baseline = new DpoTrainer(policy, reference, _root).EvaluateMany([source], 1);
        var list = new List<DpoBatch> { source, source };
        var vector = new float[] { 1, 0 };
        var vectors = new List<DpoVectorBatch> { new(source, vector) };
        var lease = new TestLease();
        var trainer = DpoTrainer.CreateForPreparedContinuation(policy, reference, _root,
            DpoMutationAdmissionCapability.Issue(lease), 1e-5f, null, null, null);
        lease.Callback = () =>
        {
            source.ChosenIds.Data[0] = float.NaN;
            Array.Fill(source.ChosenResponseMask!.Data, 0f);
            Array.Fill(source.RejectedMask.Data, 0f);
            vector[0] = float.NaN;
            list.Clear();
            vectors.Clear();
        };
        var actual = entry switch
        {
            "Step" => trainer.Step(source, 1),
            "StepMany" => trainer.StepMany(list, 1),
            _ => trainer.StepVector(vectors, 1)
        };
        Assert.Equal(baseline.ChosenReward, actual.ChosenReward, 5);
        Assert.Equal(baseline.RejectedReward, actual.RejectedReward, 5);
        // The one active slot in the ORIGINAL captured vector scales both
        // exact loss and gradient. Callback mutation to NaN must not erase it.
        var expectedLoss = entry == "StepVector" ? baseline.Loss * (1f + 1f / 60f) : baseline.Loss;
        Assert.Equal(expectedLoss, actual.Loss, 5);
        if (entry == "StepVector") Assert.NotEqual(baseline.Loss, actual.Loss);
        Assert.Equal(referenceBefore, ModelHash(reference));
        Assert.True(File.Exists(Path.Combine(_root, DpoTrainer.BuildPolicyCheckpointFileName(1))));
    }

    [Theory]
    [InlineData("tokenizer")]
    [InlineData("mask")]
    [InlineData("request-version")]
    [InlineData("missing-version")]
    public async Task LogicalUnit_RejectsMismatchBeforePreparationPermit(string mismatch)
    {
        var tokenizer = Tokenizer();
        var prepared = Prepare(tokenizer, "ab", "c", "d");
        var lease = new TestLease();
        var policy = Model();
        var reference = Model();
        var request = LogicalRequest(policy, reference, tokenizer, prepared, lease);
        if (mismatch == "tokenizer") tokenizer.Train("abcd abcd", 258);
        if (mismatch == "mask") prepared.OptimizerInputs[0].Batch.Batch.ChosenResponseMask!.Data[2] = 0;
        if (mismatch == "request-version") request = request with { MaterialVersion = DpoExactPairMaterial.LegacyVersion };
        if (mismatch == "missing-version") request = request with { MaterialVersion = null! };
        var before = ModelHash(policy);
        var result = await new LogicalDpoTrainingUnit().ExecuteAsync(Envelope(request));
        Assert.False(result.IsSuccess);
        Assert.Equal(DpoExactPairMaterial.InvalidErrorCode, result.ErrorCode);
        Assert.Equal(0, lease.Requests);
        Assert.Equal(before, ModelHash(policy));
        Assert.False(Directory.Exists(_root));
    }

    [Fact]
    public async Task LogicalUnit_CapturesPreparedInputsBeforeFirstPreparationPermit()
    {
        var tokenizer = Tokenizer();
        var prepared = Prepare(tokenizer, "ab", "cd", "e");
        var original = prepared.OptimizerInputs[0].Batch;
        var expectedIds = original.Batch.ChosenIds.Data.ToArray();
        var expectedMask = original.Batch.ChosenResponseMask!.Data.ToArray();
        var expectedVector = original.TeachingVector.ToArray();
        var lease = new TestLease();
        var request = LogicalRequest(Model(), Model(), tokenizer, prepared, lease);
        lease.Callback = () =>
        {
            original.Batch.ChosenIds.Data[0] = float.NaN;
            Array.Fill(original.Batch.ChosenResponseMask.Data, 0f);
            original.TeachingVector[0] = float.NaN;
            request.Preferences[0] = Preference("changed", "changed", "changed");
        };
        var observed = false;
        LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting = (_, batches) =>
        {
            var consumed = Assert.Single(batches);
            Assert.NotSame(original, consumed);
            Assert.Equal(expectedIds, consumed.Batch.ChosenIds.Data);
            Assert.Equal(expectedMask, consumed.Batch.ChosenResponseMask!.Data);
            Assert.Equal(expectedVector, consumed.TeachingVector);
            observed = true;
        };
        try
        {
            var result = await new LogicalDpoTrainingUnit().ExecuteAsync(Envelope(request));
            Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
            Assert.True(observed);
            Assert.True(lease.Requests > 0);
        }
        finally { LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting = null; }
    }

    [Fact]
    public void OrdinaryPreparation_KeepsLegacyVersionAndCompactBehavior()
    {
        var result = LogicalDpoTrainingUnit.PrepareExecution([Preference("  a\t\r\n", "b  c", "d")], Tokenizer());
        Assert.True(result.IsSuccess, result.ErrorMessage);
        var batch = Assert.Single(result.Value!.OptimizerInputs).Batch.Batch;
        Assert.Equal(DpoExactPairMaterial.LegacyVersion, result.Value.MaterialVersion);
        Assert.Equal(DpoExactPairMaterial.LegacyVersion, batch.MaterialVersion);
        Assert.Null(batch.ExactMaterial);
        Assert.Null(batch.ChosenResponseMask);
        Assert.Equal("a b c", DecodeReal(Tokenizer(), batch.ChosenIds, (int)batch.ChosenMask.Data.Sum()));
    }

    private IEnumerable<DpoBatch> InvalidBatches()
    {
        var tokenizer = Tokenizer();
        var batch = Batch(tokenizer, "ab", "cd", "e");
        yield return batch with { MaterialVersion = "unknown" };
        yield return batch with { MaterialVersion = null! };
        yield return batch with { MaterialVersion = DpoExactPairMaterial.LegacyVersion };
        yield return batch with { ExactMaterial = null };
        yield return batch with { ChosenResponseMask = null };
        yield return batch with { Admission = Batch(tokenizer, "different", "cd", "e").Admission };
        foreach (var value in new[] { 12.5f, -1f, 512f, float.NaN, float.PositiveInfinity })
        {
            var tampered = Batch(tokenizer, "ab", "cd", "e");
            tampered.ChosenIds.Data[0] = value;
            yield return tampered;
        }
        var badMask = Batch(tokenizer, "ab", "cd", "e");
        badMask.ChosenResponseMask!.Data[0] = 1;
        yield return badMask;
        yield return Batch(tokenizer, "abcdefg", "hi", "j");
    }

    private LogicalDpoTrainingRequest LogicalRequest(LlmModel policy, LlmModel reference, BpeTokenizer tokenizer,
        LogicalDpoPreparedExecution prepared, TestLease lease)
    {
        var mutation = LogicalDpoMutationAdmissionCapability.Issue(lease).Value!;
        var attestation = LogicalCapabilityExecutableManifestAttestationCapability.Issue(lease).Value!;
        return new LogicalDpoTrainingRequest(policy, reference, tokenizer, _root, [Preference("ab", "cd", "e")],
            new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
            AdmissionCapabilities: new LogicalCapabilityTrainingAdmissionCapabilities(mutation, DpoMutationAdmissionCapability.Issue(lease), attestation),
            MutationAdmission: mutation)
        { PreparedExecution = prepared, MaterialVersion = DpoExactPairMaterial.Version };
    }

    private static LogicalUnitRequest<LogicalDpoTrainingRequest> Envelope(LogicalDpoTrainingRequest request)
        => new("dpo-exact-test", LogicalDpoTrainingUnit.Id, LogicalDpoTrainingUnit.Role, LogicalDpoTrainingUnit.DomainTag,
            request, new LogicalExecutionFlags(), new Dictionary<string, object>());
    private static LogicalDpoPreferenceExample Preference(string prompt, string chosen, string rejected)
        => new(prompt, chosen, rejected, [1f, 0f])
        { ChosenAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator, RejectedAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator };
    private static LogicalDpoPreparedExecution Prepare(BpeTokenizer tokenizer, string prompt, string chosen, string rejected)
    {
        var result = LogicalDpoTrainingUnit.PrepareExecution([Preference(prompt, chosen, rejected)], tokenizer, DpoExactPairMaterial.Version);
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return result.Value!;
    }
    private static DpoBatch Batch(BpeTokenizer tokenizer, string prompt, string chosen, string rejected)
        => DpoExactPairMaterial.CreateBatch(tokenizer, prompt, chosen, rejected,
            DpoTrainerPairCustoms.CreateAdmitted(prompt, chosen, rejected,
                DpoPairSideAuthorKind.DeterministicGenerator, DpoPairSideAuthorKind.DeterministicGenerator, "test").Admission);
    private static BpeTokenizer Tokenizer() { var tokenizer = new BpeTokenizer(); tokenizer.Train("fixture", 256); return tokenizer; }
    private static LlmModel Model(int context = 8)
        => new(new ModelConfig { VocabSize = 512, ContextLen = context, HiddenDim = 8, NumLayers = 1, NumHeads = 2, NumKvHeads = 1, FfnHiddenDim = 16 });
    private static string DecodeReal(BpeTokenizer tokenizer, Tensor ids, int count)
        => tokenizer.Decode(ids.Data.Take(count).Select(x => (int)x).ToList());
    private static void AssertMasks(Tensor real, Tensor response, int promptLength, int realLength)
    {
        for (var j = 0; j < real.Data.Length; j++)
        {
            Assert.Equal(j < realLength ? 1f : 0f, real.Data[j]);
            Assert.Equal(j >= promptLength && j < realLength ? 1f : 0f, response.Data[j]);
        }
    }
    private static void AssertInvalid(Action action)
        => Assert.Equal(DpoExactPairMaterial.InvalidErrorCode, Assert.Throws<DpoMutationAdmissionException>(action).ErrorCode);
    private static void Invoke(DpoTrainer trainer, string entry, DpoBatch batch)
    {
        switch (entry)
        {
            case "Step": trainer.Step(batch, 1); break;
            case "StepMany": trainer.StepMany([Batch(Tokenizer(), "a", "b", "c"), batch], 1); break;
            case "StepVector": trainer.StepVector([new(Batch(Tokenizer(), "a", "b", "c"), [1]), new(batch, [1])], 1); break;
            default: trainer.EvaluateMany([Batch(Tokenizer(), "a", "b", "c"), batch], 1); break;
        }
    }
    private static string ModelHash(LlmModel model)
    {
        using var stream = new MemoryStream();
        model.SaveCheckpointToStream(stream);
        return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
    }
    private static void FillModel(LlmModel model, int seed)
    {
        var random = new Random(seed);
        foreach (var parameter in model.Parameters())
            for (var i = 0; i < parameter.Value.Data.Length; i++)
                parameter.Value.Data[i] = parameter.Value.Shape.Rank == 1
                    ? 1 : (float)(random.NextDouble() - 0.5) * 0.15f;
    }
    private static float IndependentScore(float[] logits, int vocab, (int Row, int Token)[] targets)
    {
        double sum = 0;
        foreach (var (row, token) in targets)
        {
            var maximum = logits.Skip(row * vocab).Take(vocab).Max();
            var denominator = logits.Skip(row * vocab).Take(vocab).Sum(x => Math.Exp(x - maximum));
            sum += logits[row * vocab + token] - maximum - Math.Log(denominator);
        }
        return (float)sum;
    }
    private static float ModelScore(LlmModel model, DpoBatch batch, bool chosen)
    {
        var ids = chosen ? batch.ChosenIds : batch.RejectedIds;
        var end = chosen ? batch.ExactMaterial!.ChosenLength : batch.ExactMaterial!.RejectedLength;
        return IndependentScore(model.Forward(ids, training: false).Data, model.Config.VocabSize,
            Enumerable.Range(batch.ExactMaterial!.PromptLength, end - batch.ExactMaterial.PromptLength).Select(j => (j - 1, (int)ids.Data[j])).ToArray());
    }
    private static float Score(Tensor logits, Tensor ids, Tensor mask, bool sum)
        => (float)typeof(DpoTrainer).GetMethod("ComputeTargetLogProbs", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [logits, ids, mask, sum])!;
    private static void Backward(Tensor logits, Tensor ids, Tensor mask, float score, bool sum, float upstream)
    {
        var graphType = typeof(DpoTrainer).GetNestedType("PolicyLogProbGraph", BindingFlags.NonPublic)!;
        var graph = Activator.CreateInstance(graphType, [logits, ids, mask, score, sum]);
        typeof(DpoTrainer).GetMethod("BackwardLogProbs", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [graph, upstream]);
    }
    private sealed class TestLease : ILogicalCapabilityTrainingAdmissionLease, ILogicalDpoMutationAdmissionIssuer,
        IDpoMutationAdmissionIssuer, ILogicalCapabilityExecutableManifestAttestationIssuer
    {
        public int Requests;
        public Action? Callback;
        public OperationResult<IDisposable> AcquireMutationPermit() { Requests++; Callback?.Invoke(); return OperationResult<IDisposable>.Ok(new CancellationTokenSource()); }
        OperationResult<IDisposable> ILogicalDpoMutationAdmissionIssuer.AcquireDpoMutationPermit() => AcquireMutationPermit();
        IDisposable IDpoMutationAdmissionIssuer.AcquireDpoTrainerMutationPermit() => AcquireMutationPermit().Value!;
        OperationResult<LogicalCapabilityExecutableManifestAttestation> ILogicalCapabilityExecutableManifestAttestationIssuer.SealExecutableManifest(
            string executableManifestSha256, LogicalCapabilityTrainingReplayAttestation replayAttestation)
            => OperationResult<LogicalCapabilityExecutableManifestAttestation>.Fail("Unused synthetic fixture.", "UNUSED");
        public void Dispose() { }
    }
    public void Dispose()
    { if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true); }
}
