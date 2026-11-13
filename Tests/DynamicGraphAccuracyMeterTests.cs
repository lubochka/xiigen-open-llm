// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Collections;
using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class DynamicGraphAccuracyMeterTests
{
    private const string InvalidCode = "INVALID_ACCURACY_MEASUREMENT";
    private const string InvalidMessage = "Dynamic graph accuracy measurement is invalid.";
    private const string MismatchCode = "HELDOUT_EVALUATION_MISMATCH";
    private const string MismatchMessage = "Held-out evaluation results do not describe the same evaluation set.";

    [Fact]
    public void Contracts_ContainExactlyThePlannedEvidenceFields()
    {
        AssertRecordShape<DynamicGraphAccuracyAxis>(
            ("LiveMarginAccuracy", typeof(double)),
            ("HeldOutAccuracy", typeof(double)),
            ("AccuracyDelta", typeof(double)));
        AssertRecordShape<DynamicGraphAccuracyMeasurement>(
            ("EvaluationId", typeof(string)),
            ("RowsEvaluated", typeof(int)),
            ("Accuracy", typeof(DynamicGraphAccuracyAxis)),
            ("HeldOutRewardMargin", typeof(double)),
            ("MeetsQ30NumericalGate", typeof(bool)),
            ("SplitProof", typeof(DynamicGraphMeasurementSplitProof)));
    }

    [Fact]
    public void Thresholds_AreTheLockedPublicQ30Values()
    {
        Assert.Equal(0.95d, DynamicGraphAccuracyMeter.MinimumHeldOutAccuracy);
        Assert.Equal(0.02d, DynamicGraphAccuracyMeter.MinimumAccuracyDelta);
        Assert.Equal(0.025d, DynamicGraphAccuracyMeter.MinimumHeldOutRewardMargin);
    }

    [Fact]
    public void Measure_ProjectsHeldOutAfterMinusBeforeAndKeepsLiveSeparate()
    {
        var result = DynamicGraphAccuracyMeter.Measure(
            0.44d,
            Evaluation(accuracy: 0.60d, rewardMargin: -0.3d, passesGate: true),
            Evaluation(accuracy: 0.96d, rewardMargin: 0.026d, passesGate: false),
            Proof());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("eval-1", result.Value!.EvaluationId);
        Assert.Equal(50, result.Value.RowsEvaluated);
        Assert.Equal(0.44d, result.Value.Accuracy.LiveMarginAccuracy);
        Assert.Equal(0.96d, result.Value.Accuracy.HeldOutAccuracy);
        Assert.Equal(0.36d, result.Value.Accuracy.AccuracyDelta, 12);
        Assert.Equal(0.026d, result.Value.HeldOutRewardMargin);
        Assert.True(result.Value.MeetsQ30NumericalGate);
    }

    [Fact]
    public void Measure_CoincidentLiveAndHeldOutValuesAreAccepted()
    {
        var result = DynamicGraphAccuracyMeter.Measure(
            0.95d,
            Evaluation(accuracy: 0.93d),
            Evaluation(accuracy: 0.95d),
            Proof());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(result.Value!.Accuracy.LiveMarginAccuracy, result.Value.Accuracy.HeldOutAccuracy);
        Assert.True(result.Value.MeetsQ30NumericalGate);
    }

    [Fact]
    public void Measure_IgnoresContradictorySourcePassesGateFlags()
    {
        var result = DynamicGraphAccuracyMeter.Measure(
            0d,
            Evaluation(accuracy: 0.93d, passesGate: true),
            Evaluation(accuracy: 0.95d, rewardMargin: 0.025d, passesGate: false),
            Proof());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value!.MeetsQ30NumericalGate);
    }

    [Fact]
    public void Measure_UsesAfterMinusBeforeRatherThanReverseDelta()
    {
        var increasing = DynamicGraphAccuracyMeter.Measure(
            0d,
            Evaluation(accuracy: 0.93d),
            Evaluation(accuracy: 0.95d),
            Proof());
        var decreasing = DynamicGraphAccuracyMeter.Measure(
            1d,
            Evaluation(accuracy: 0.97d),
            Evaluation(accuracy: 0.95d),
            Proof());

        Assert.True(increasing.Value!.MeetsQ30NumericalGate);
        Assert.Equal(0.02d, increasing.Value.Accuracy.AccuracyDelta, 12);
        Assert.False(decreasing.Value!.MeetsQ30NumericalGate);
        Assert.Equal(-0.02d, decreasing.Value.Accuracy.AccuracyDelta, 12);
    }

    [Fact]
    public void Measure_LiveAccuracyCannotSubstituteForHeldOutAccuracy()
    {
        var result = DynamicGraphAccuracyMeter.Measure(
            1d,
            Evaluation(accuracy: 0.90d),
            Evaluation(accuracy: 0.94d, rewardMargin: 1d),
            Proof());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.MeetsQ30NumericalGate);
        Assert.Equal(0.94d, result.Value.Accuracy.HeldOutAccuracy);
    }

    [Fact]
    public void Measure_ExactDriftProneDecimalBoundariesAreInclusive()
    {
        var result = DynamicGraphAccuracyMeter.Measure(
            0d,
            Evaluation(accuracy: 0.93d),
            Evaluation(accuracy: 0.95d, rewardMargin: 0.1d - 0.075d),
            Proof());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value!.MeetsQ30NumericalGate);
        Assert.True(result.Value.Accuracy.AccuracyDelta < 0.02d);
    }

    [Fact]
    public void Measure_ValuesAboveEveryBoundaryPass()
    {
        var result = DynamicGraphAccuracyMeter.Measure(
            0d,
            Evaluation(accuracy: 0.93d),
            Evaluation(accuracy: 0.9501d, rewardMargin: 0.0251d),
            Proof());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value!.MeetsQ30NumericalGate);
    }

    [Fact]
    public void Measure_ImmediateBelowRawHeldOutAccuracyDoesNotRoundUpToGate()
    {
        var result = DynamicGraphAccuracyMeter.Measure(
            0d,
            Evaluation(accuracy: 0.92d),
            Evaluation(accuracy: 0.9499999999996d, rewardMargin: 0.03d),
            Proof());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.MeetsQ30NumericalGate);
    }

    [Fact]
    public void Measure_ImmediateBelowRawRewardMarginDoesNotRoundUpToGate()
    {
        var result = DynamicGraphAccuracyMeter.Measure(
            0d,
            Evaluation(accuracy: 0.93d),
            Evaluation(accuracy: 0.96d, rewardMargin: 0.0249999999996d),
            Proof());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.MeetsQ30NumericalGate);
    }

    [Theory]
    [InlineData("heldout")]
    [InlineData("delta")]
    [InlineData("reward")]
    public void Measure_OneBelowThresholdPredicateMakesTheConjunctionFalse(string predicate)
    {
        var beforeAccuracy = predicate == "delta" ? 0.9300006d : 0.93d;
        var afterAccuracy = predicate == "heldout" ? 0.9499994d : 0.95d;
        var rewardMargin = predicate == "reward" ? 0.0249994d : 0.025d;

        var result = DynamicGraphAccuracyMeter.Measure(
            0d,
            Evaluation(accuracy: beforeAccuracy),
            Evaluation(accuracy: afterAccuracy, rewardMargin: rewardMargin),
            Proof());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.MeetsQ30NumericalGate);
    }

    [Fact]
    public void Measure_SplitOverlapPrecedesNullEvaluationsAndPreservesGuardError()
    {
        var result = DynamicGraphAccuracyMeter.Measure(
            double.NaN,
            null,
            null,
            Proof(trainRoots: ["shared"], evalRoots: ["shared"], nearDuplicateCount: 2));

        AssertFailure(
            result,
            "TRAIN_HELDOUT_OVERLAP",
            "Training and held-out source roots overlap.");
    }

    [Fact]
    public void Measure_NearDuplicateFailurePrecedesInvalidMeasurementsAndPreservesGuardError()
    {
        var result = DynamicGraphAccuracyMeter.Measure(
            double.NaN,
            null,
            null,
            Proof(nearDuplicateCount: 1));

        AssertFailure(
            result,
            "TRAIN_HELDOUT_NEAR_DUPLICATE",
            "Training and held-out sources contain near-duplicates.");
    }

    [Fact]
    public void Measure_MalformedSplitPrecedesNullEvaluationsAndPreservesGuardError()
    {
        var result = DynamicGraphAccuracyMeter.Measure(
            double.NaN,
            null,
            null,
            Proof(method: "SimHash"));

        AssertFailure(result, "INVALID_SPLIT_PROOF", "Dynamic graph measurement split proof is invalid.");
    }

    [Fact]
    public void Measure_NullSplitPrecedesNullEvaluationsAndPreservesGuardError()
    {
        var result = DynamicGraphAccuracyMeter.Measure(double.NaN, null, null, null);

        AssertFailure(result, "INVALID_SPLIT_PROOF", "Dynamic graph measurement split proof is invalid.");
    }

    [Fact]
    public void Measure_OrdinaryThrowingSplitSnapshotPrecedesInvalidMeasurements()
    {
        var result = DynamicGraphAccuracyMeter.Measure(
            double.NaN,
            null,
            null,
            Proof(trainRoots: new FaultingReadOnlyList<string>(new InvalidOperationException("race"))));

        AssertFailure(result, "INVALID_SPLIT_PROOF", "Dynamic graph measurement split proof is invalid.");
    }

    [Fact]
    public void Measure_CancellationFromSplitSnapshotEscapes()
    {
        var cancellation = new OperationCanceledException("cancel split snapshot");

        var thrown = Assert.Throws<OperationCanceledException>(() => DynamicGraphAccuracyMeter.Measure(
            0d,
            null,
            null,
            Proof(evalRoots: new FaultingReadOnlyList<string>(cancellation))));

        Assert.Same(cancellation, thrown);
    }

    [Fact]
    public void Measure_NestedFatalFromSplitSnapshotEscapes()
    {
        var fatal = new OutOfMemoryException("fatal split snapshot");
        var wrapper = new AggregateException(new InvalidOperationException("ordinary"), fatal);

        var thrown = Assert.Throws<AggregateException>(() => DynamicGraphAccuracyMeter.Measure(
            0d,
            null,
            null,
            Proof(hashes: new FaultingReadOnlyList<string>(wrapper))));

        Assert.Same(wrapper, thrown);
    }

    [Theory]
    [InlineData("before")]
    [InlineData("after")]
    public void Measure_NullEvaluationFailsClosed(string carrier)
    {
        var result = DynamicGraphAccuracyMeter.Measure(
            0d,
            carrier == "before" ? null : Evaluation(accuracy: 0.93d),
            carrier == "after" ? null : Evaluation(accuracy: 0.95d),
            Proof());

        AssertInvalid(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" leading")]
    [InlineData("trailing ")]
    [InlineData("line\nfeed")]
    [InlineData("control\u0001value")]
    [InlineData("separator\u2028value")]
    public void Measure_NonCanonicalEvaluationIdFailsClosed(string evaluationId)
    {
        AssertInvalid(DynamicGraphAccuracyMeter.Measure(
            0d,
            Evaluation(evaluationId: evaluationId, accuracy: 0.93d),
            Evaluation(evaluationId: evaluationId, accuracy: 0.95d),
            Proof()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Measure_NonPositiveRowsEvaluatedFailsClosed(int rowsEvaluated)
    {
        AssertInvalid(DynamicGraphAccuracyMeter.Measure(
            0d,
            Evaluation(rowsEvaluated: rowsEvaluated, accuracy: 0.93d),
            Evaluation(rowsEvaluated: rowsEvaluated, accuracy: 0.95d),
            Proof()));
    }

    [Theory]
    [InlineData("live", double.NaN)]
    [InlineData("live", double.PositiveInfinity)]
    [InlineData("live", -0.001d)]
    [InlineData("live", 1.001d)]
    [InlineData("before-accuracy", double.NegativeInfinity)]
    [InlineData("before-accuracy", -0.001d)]
    [InlineData("after-accuracy", double.NaN)]
    [InlineData("after-accuracy", 1.001d)]
    [InlineData("before-reward", double.PositiveInfinity)]
    [InlineData("after-reward", double.NegativeInfinity)]
    public void Measure_NonFiniteOrOutOfRangeNumbersFailClosed(string carrier, double invalid)
    {
        var live = carrier == "live" ? invalid : 0d;
        var before = Evaluation(
            accuracy: carrier == "before-accuracy" ? invalid : 0.93d,
            rewardMargin: carrier == "before-reward" ? invalid : 0d);
        var after = Evaluation(
            accuracy: carrier == "after-accuracy" ? invalid : 0.95d,
            rewardMargin: carrier == "after-reward" ? invalid : 0.025d);

        AssertInvalid(DynamicGraphAccuracyMeter.Measure(live, before, after, Proof()));
    }

    [Fact]
    public void Measure_DifferentEvaluationIdsReturnStableMismatch()
    {
        var result = DynamicGraphAccuracyMeter.Measure(
            0d,
            Evaluation(evaluationId: "eval-before", accuracy: 0.93d),
            Evaluation(evaluationId: "eval-after", accuracy: 0.95d),
            Proof());

        AssertMismatch(result);
    }

    [Fact]
    public void Measure_DifferentRowCountsReturnStableMismatch()
    {
        var result = DynamicGraphAccuracyMeter.Measure(
            0d,
            Evaluation(rowsEvaluated: 49, accuracy: 0.93d),
            Evaluation(rowsEvaluated: 50, accuracy: 0.95d),
            Proof());

        AssertMismatch(result);
    }

    [Fact]
    public void Measure_ReturnsGuardOwnedReadOnlySplitProof()
    {
        var train = new List<string> { "train-a" };
        var eval = new List<string> { "eval-a" };
        var hashes = new List<string> { "hash-a" };
        var callerProof = Proof(train, eval, hashes);

        var result = DynamicGraphAccuracyMeter.Measure(
            0d,
            Evaluation(accuracy: 0.93d),
            Evaluation(accuracy: 0.95d),
            callerProof);
        Assert.True(result.IsSuccess, result.ErrorMessage);

        train[0] = "mutated-train";
        eval[0] = "mutated-eval";
        hashes[0] = "mutated-hash";
        Assert.NotSame(callerProof, result.Value!.SplitProof);
        Assert.Equal(["train-a"], result.Value.SplitProof.TrainSourceRoots);
        Assert.Equal(["eval-a"], result.Value.SplitProof.EvalSourceRoots);
        Assert.Equal(["hash-a"], result.Value.SplitProof.SourceHashes);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<string>)result.Value.SplitProof.TrainSourceRoots).Add("forbidden"));
    }

    private static LearningEvaluationResult Evaluation(
        string evaluationId = "eval-1",
        int rowsEvaluated = 50,
        double accuracy = 0.95d,
        double rewardMargin = 0.025d,
        bool passesGate = true)
        => new(evaluationId, rowsEvaluated, accuracy, rewardMargin, passesGate, "source note ignored");

    private static DynamicGraphMeasurementSplitProof Proof(
        IReadOnlyList<string>? trainRoots = null,
        IReadOnlyList<string>? evalRoots = null,
        IReadOnlyList<string>? hashes = null,
        int nearDuplicateCount = 0,
        string method = "simhash")
        => new(
            trainRoots ?? ["train-a"],
            evalRoots ?? ["eval-a"],
            hashes ?? ["hash-a"],
            nearDuplicateCount,
            method,
            0.85d);

    private static void AssertInvalid(OperationResult<DynamicGraphAccuracyMeasurement> result)
        => AssertFailure(result, InvalidCode, InvalidMessage);

    private static void AssertMismatch(OperationResult<DynamicGraphAccuracyMeasurement> result)
        => AssertFailure(result, MismatchCode, MismatchMessage);

    private static void AssertFailure(
        OperationResult<DynamicGraphAccuracyMeasurement> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    private static void AssertRecordShape<T>(params (string Name, Type Type)[] expected)
    {
        var constructor = Assert.Single(typeof(T).GetConstructors());
        Assert.Equal(
            expected,
            constructor.GetParameters()
                .Select(parameter => (parameter.Name!, parameter.ParameterType))
                .ToArray());
        Assert.Equal(
            expected.Select(field => field.Name),
            typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name));
    }

    private sealed class FaultingReadOnlyList<T>(Exception exception) : IReadOnlyList<T>
    {
        public int Count => 1;
        public T this[int index] => throw exception;
        public IEnumerator<T> GetEnumerator() => throw exception;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
