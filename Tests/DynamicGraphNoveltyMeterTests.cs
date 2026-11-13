// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Collections;
using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class DynamicGraphNoveltyMeterTests
{
    private const string InvalidCode = "INVALID_NOVELTY_MEASUREMENT";
    private const string InvalidMessage = "Dynamic graph novelty measurement is invalid.";

    [Fact]
    public void Contracts_ContainExactlyTheNoveltyFieldsInOrder()
    {
        AssertRecordShape<DynamicGraphNoveltyObservation>(
            ("EvaluationId", typeof(string)),
            ("RequestedRows", typeof(int)),
            ("RowsEvaluated", typeof(int)),
            ("SelectionShortfall", typeof(int)),
            ("NewPatternRate", typeof(double)),
            ("SourceTokenSeenRate", typeof(double)),
            ("MaxAccuracy", typeof(double)),
            ("SplitProof", typeof(DynamicGraphMeasurementSplitProof)));
        AssertRecordShape<DynamicGraphNoveltyMeasurement>(
            ("EvaluationId", typeof(string)),
            ("RequestedRows", typeof(int)),
            ("RowsEvaluated", typeof(int)),
            ("SelectionShortfall", typeof(int)),
            ("NewPatternRate", typeof(double)),
            ("SourceTokenSeenRate", typeof(double)),
            ("MaxAccuracy", typeof(double)),
            ("MinimumNewPatternRate", typeof(double)),
            ("MaximumSeenTokenRate", typeof(double)),
            ("MeetsNoveltyGate", typeof(bool)),
            ("SplitProof", typeof(DynamicGraphMeasurementSplitProof)));
    }

    [Fact]
    public void Meter_PublicSurfaceContainsOnlyThePureMeasureMethod()
    {
        var method = Assert.Single(typeof(DynamicGraphNoveltyMeter).GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

        Assert.Equal(nameof(DynamicGraphNoveltyMeter.Measure), method.Name);
        Assert.Equal(typeof(OperationResult<DynamicGraphNoveltyMeasurement>), method.ReturnType);
        var parameter = Assert.Single(method.GetParameters());
        Assert.Equal("observation", parameter.Name);
        Assert.Equal(typeof(DynamicGraphNoveltyObservation), parameter.ParameterType);
        Assert.Empty(typeof(DynamicGraphNoveltyMeter).GetFields(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(typeof(DynamicGraphNoveltyMeter).GetProperties(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.All(
            typeof(DynamicGraphNoveltyMeter).GetFields(
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly),
            field => Assert.True(field.IsLiteral, $"Meter retained non-constant field {field.Name}."));
    }

    [Fact]
    public void PublicSurface_DoesNotOverclaimSelectionTrainingPromotionOrFullQuality()
    {
        var surface = string.Join('|', new[]
        {
            typeof(DynamicGraphNoveltyMeter).FullName,
            string.Join(',', typeof(DynamicGraphNoveltyObservation).GetProperties().Select(value => value.Name)),
            string.Join(',', typeof(DynamicGraphNoveltyMeasurement).GetProperties().Select(value => value.Name))
        });

        foreach (var forbidden in new[]
                 {
                     "SelectRows", "CalculatePattern", "CalculateToken", "Train", "Dpo",
                     "Promot", "Lifecycle", "Ablation", "SourceId", "NamedControl", "FullQuality",
                     "Checkpoint", "Verdict"
                 })
        {
            Assert.DoesNotContain(forbidden, surface, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Measure_PassingObservationPreservesEvidenceAndDerivesExactThresholds()
    {
        var split = Proof();
        var result = DynamicGraphNoveltyMeter.Measure(Observation(
            evaluationId: "eval-novelty-7",
            requestedRows: 200,
            rowsEvaluated: 200,
            selectionShortfall: 0,
            newPatternRate: 0.26d,
            sourceTokenSeenRate: 0.74d,
            maxAccuracy: 0.75d,
            splitProof: split));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var measurement = result.Value!;
        Assert.Equal("eval-novelty-7", measurement.EvaluationId);
        Assert.Equal(200, measurement.RequestedRows);
        Assert.Equal(200, measurement.RowsEvaluated);
        Assert.Equal(0, measurement.SelectionShortfall);
        Assert.Equal(0.26d, measurement.NewPatternRate);
        Assert.Equal(0.74d, measurement.SourceTokenSeenRate);
        Assert.Equal(0.75d, measurement.MaxAccuracy);
        Assert.Equal(0.25d, measurement.MinimumNewPatternRate);
        Assert.Equal(0.75d, measurement.MaximumSeenTokenRate);
        Assert.True(measurement.MeetsNoveltyGate);
        Assert.NotSame(split, measurement.SplitProof);
    }

    [Fact]
    public void Measure_PreservesEvaluationIdEvidence()
        => Assert.Equal(
            "eval-field",
            Successful(Observation(evaluationId: "eval-field")).EvaluationId);

    [Fact]
    public void Measure_PreservesRequestedRowsEvidence()
        => Assert.Equal(
            137,
            Successful(Observation(
                requestedRows: 137,
                rowsEvaluated: 137)).RequestedRows);

    [Fact]
    public void Measure_PreservesRowsEvaluatedEvidence()
        => Assert.Equal(
            136,
            Successful(Observation(
                requestedRows: 137,
                rowsEvaluated: 136,
                selectionShortfall: 1)).RowsEvaluated);

    [Fact]
    public void Measure_PreservesSelectionShortfallEvidence()
        => Assert.Equal(
            1,
            Successful(Observation(
                requestedRows: 137,
                rowsEvaluated: 136,
                selectionShortfall: 1)).SelectionShortfall);

    [Fact]
    public void Measure_PreservesNewPatternRateEvidence()
        => Assert.Equal(
            0.37d,
            Successful(Observation(newPatternRate: 0.37d)).NewPatternRate);

    [Fact]
    public void Measure_PreservesSourceTokenSeenRateEvidence()
        => Assert.Equal(
            0.63d,
            Successful(Observation(sourceTokenSeenRate: 0.63d)).SourceTokenSeenRate);

    [Fact]
    public void Measure_PreservesMaxAccuracyEvidence()
        => Assert.Equal(
            0.81d,
            Successful(Observation(maxAccuracy: 0.81d)).MaxAccuracy);

    [Theory]
    [InlineData(0.25d, false)]
    [InlineData(0.25000000000000006d, true)]
    [InlineData(0.24999999999999997d, false)]
    public void Measure_NewPatternBoundaryIsStrictAndUsesRawDouble(
        double newPatternRate,
        bool expected)
    {
        var result = DynamicGraphNoveltyMeter.Measure(Observation(
            newPatternRate: newPatternRate,
            sourceTokenSeenRate: 0.74d,
            maxAccuracy: 0.75d));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(expected, result.Value!.MeetsNoveltyGate);
        Assert.Equal(newPatternRate, result.Value.NewPatternRate);
    }

    [Theory]
    [InlineData(0.75d, false)]
    [InlineData(0.7499999999999999d, true)]
    [InlineData(0.7500000000000001d, false)]
    public void Measure_SeenTokenBoundaryIsStrictAndUsesRawDouble(
        double sourceTokenSeenRate,
        bool expected)
    {
        var result = DynamicGraphNoveltyMeter.Measure(Observation(
            newPatternRate: 0.26d,
            sourceTokenSeenRate: sourceTokenSeenRate,
            maxAccuracy: 0.75d));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(expected, result.Value!.MeetsNoveltyGate);
        Assert.Equal(sourceTokenSeenRate, result.Value.SourceTokenSeenRate);
    }

    [Fact]
    public void Measure_BitIncrementAndBitDecrementProveNoRoundingOrEpsilon()
    {
        var exactNew = 1d - 0.75d;
        var exactSeen = 0.75d;
        var passes = DynamicGraphNoveltyMeter.Measure(Observation(
            newPatternRate: double.BitIncrement(exactNew),
            sourceTokenSeenRate: double.BitDecrement(exactSeen)));
        var failsNew = DynamicGraphNoveltyMeter.Measure(Observation(
            newPatternRate: exactNew,
            sourceTokenSeenRate: double.BitDecrement(exactSeen)));
        var failsSeen = DynamicGraphNoveltyMeter.Measure(Observation(
            newPatternRate: double.BitIncrement(exactNew),
            sourceTokenSeenRate: exactSeen));

        Assert.True(passes.IsSuccess, passes.ErrorMessage);
        Assert.True(passes.Value!.MeetsNoveltyGate);
        Assert.False(failsNew.Value!.MeetsNoveltyGate);
        Assert.False(failsSeen.Value!.MeetsNoveltyGate);
    }

    [Fact]
    public void Measure_CompleteSelectionIsRequiredEvenWhenRatesPass()
    {
        var result = DynamicGraphNoveltyMeter.Measure(Observation(
            requestedRows: 10,
            rowsEvaluated: 9,
            selectionShortfall: 1,
            newPatternRate: 1d,
            sourceTokenSeenRate: 0d));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.MeetsNoveltyGate);
        Assert.Equal(1, result.Value.SelectionShortfall);
        Assert.Equal(9, result.Value.RowsEvaluated);
    }

    [Fact]
    public void Measure_ZeroEvaluatedRowsIsValidEvidenceButCannotMeetGate()
    {
        var result = DynamicGraphNoveltyMeter.Measure(Observation(
            requestedRows: 10,
            rowsEvaluated: 0,
            selectionShortfall: 10,
            newPatternRate: 1d,
            sourceTokenSeenRate: 0d));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.MeetsNoveltyGate);
    }

    [Theory]
    [InlineData(0.25d, 0.74d)]
    [InlineData(0.26d, 0.75d)]
    [InlineData(0.25d, 0.75d)]
    public void Measure_EachRatePredicateCanIndependentlyMiss(
        double newPatternRate,
        double sourceTokenSeenRate)
    {
        var result = DynamicGraphNoveltyMeter.Measure(Observation(
            newPatternRate: newPatternRate,
            sourceTokenSeenRate: sourceTokenSeenRate));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.MeetsNoveltyGate);
    }

    [Theory]
    [InlineData(0d, 1d, false)]
    [InlineData(1d, 0d, true)]
    public void Measure_MaxAccuracyExtremesProduceExactUnclampedThresholds(
        double maxAccuracy,
        double expectedMinimum,
        bool expectedGate)
    {
        var result = DynamicGraphNoveltyMeter.Measure(Observation(
            newPatternRate: 0.5d,
            sourceTokenSeenRate: 0.5d,
            maxAccuracy: maxAccuracy));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(expectedMinimum, result.Value!.MinimumNewPatternRate);
        Assert.Equal(maxAccuracy, result.Value.MaximumSeenTokenRate);
        Assert.Equal(expectedGate, result.Value.MeetsNoveltyGate);
    }

    [Fact]
    public void Measure_NullObservationReturnsStableLocalError()
        => AssertInvalid(DynamicGraphNoveltyMeter.Measure(null));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" eval")]
    [InlineData("eval ")]
    [InlineData("eval\nline")]
    [InlineData("eval\u0085line")]
    [InlineData("eval\u2028line")]
    [InlineData("eval\u2029line")]
    public void Measure_NonCanonicalEvaluationIdReturnsStableLocalError(string? evaluationId)
        => AssertInvalid(DynamicGraphNoveltyMeter.Measure(Observation(evaluationId: evaluationId!)));

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(-1, 0, 0)]
    [InlineData(10, -1, 11)]
    [InlineData(10, 11, 0)]
    [InlineData(10, 9, -1)]
    [InlineData(10, 9, 0)]
    [InlineData(10, 9, 2)]
    public void Measure_InvalidRowOrShortfallShapeReturnsStableLocalError(
        int requestedRows,
        int rowsEvaluated,
        int selectionShortfall)
        => AssertInvalid(DynamicGraphNoveltyMeter.Measure(Observation(
            requestedRows: requestedRows,
            rowsEvaluated: rowsEvaluated,
            selectionShortfall: selectionShortfall)));

    public static TheoryData<string, double> InvalidRates => new()
    {
        { "new-pattern", double.NaN },
        { "new-pattern", double.NegativeInfinity },
        { "new-pattern", double.PositiveInfinity },
        { "new-pattern", -double.Epsilon },
        { "new-pattern", double.BitIncrement(1d) },
        { "seen-token", double.NaN },
        { "seen-token", double.NegativeInfinity },
        { "seen-token", double.PositiveInfinity },
        { "seen-token", -double.Epsilon },
        { "seen-token", double.BitIncrement(1d) },
        { "max-accuracy", double.NaN },
        { "max-accuracy", double.NegativeInfinity },
        { "max-accuracy", double.PositiveInfinity },
        { "max-accuracy", -double.Epsilon },
        { "max-accuracy", double.BitIncrement(1d) }
    };

    [Theory]
    [MemberData(nameof(InvalidRates))]
    public void Measure_EachRateRejectsEveryNonFiniteOrOutOfRangeValue(
        string carrier,
        double invalid)
    {
        var result = DynamicGraphNoveltyMeter.Measure(Observation(
            newPatternRate: carrier == "new-pattern" ? invalid : 0.26d,
            sourceTokenSeenRate: carrier == "seen-token" ? invalid : 0.74d,
            maxAccuracy: carrier == "max-accuracy" ? invalid : 0.75d));

        AssertInvalid(result);
    }

    [Fact]
    public void Measure_ValidRateEndpointsArePreservedWithoutClamping()
    {
        var result = DynamicGraphNoveltyMeter.Measure(Observation(
            newPatternRate: 1d,
            sourceTokenSeenRate: 0d,
            maxAccuracy: 0.75d));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1d, result.Value!.NewPatternRate);
        Assert.Equal(0d, result.Value.SourceTokenSeenRate);
        Assert.True(result.Value.MeetsNoveltyGate);
    }

    [Fact]
    public void Measure_SplitFailureHasPrecedenceOverSimultaneouslyInvalidScalars()
    {
        var split = Proof(trainRoots: ["same-root"], evalRoots: ["same-root"]);
        var result = DynamicGraphNoveltyMeter.Measure(Observation(
            evaluationId: " bad-id ",
            requestedRows: 0,
            rowsEvaluated: -1,
            selectionShortfall: -1,
            newPatternRate: double.NaN,
            sourceTokenSeenRate: double.PositiveInfinity,
            maxAccuracy: -1d,
            splitProof: split));

        AssertFailure(result, "TRAIN_HELDOUT_OVERLAP", "Training and held-out source roots overlap.");
    }

    [Fact]
    public void Measure_NullSplitMapsExactGuardErrorBeforeScalarValidation()
    {
        var result = DynamicGraphNoveltyMeter.Measure(new DynamicGraphNoveltyObservation(
            " bad-id ",
            100,
            100,
            0,
            0.26d,
            0.74d,
            0.75d,
            null!));

        AssertFailure(result, "INVALID_SPLIT_PROOF", "Dynamic graph measurement split proof is invalid.");
    }

    [Fact]
    public void Measure_OrdinarySplitSnapshotFailureMapsExactGuardError()
    {
        var exception = new InvalidOperationException("ordinary snapshot failure");
        var result = DynamicGraphNoveltyMeter.Measure(Observation(
            evaluationId: " bad-id ",
            splitProof: Proof(sourceHashes: new FaultingReadOnlyList<string>(exception))));

        AssertFailure(result, "INVALID_SPLIT_PROOF", "Dynamic graph measurement split proof is invalid.");
    }

    [Fact]
    public void Measure_CancellationSplitSnapshotFailureEscapesAsSameInstance()
    {
        var exception = new OperationCanceledException("cancel snapshot");

        var thrown = Assert.Throws<OperationCanceledException>(() =>
            DynamicGraphNoveltyMeter.Measure(Observation(
                evaluationId: " bad-id ",
                splitProof: Proof(sourceHashes: new FaultingReadOnlyList<string>(exception)))));

        Assert.Same(exception, thrown);
    }

    [Fact]
    public void Measure_FatalSplitSnapshotFailureEscapesAsSameInstance()
    {
        var exception = new OutOfMemoryException("fatal snapshot");

        var thrown = Assert.Throws<OutOfMemoryException>(() =>
            DynamicGraphNoveltyMeter.Measure(Observation(
                evaluationId: " bad-id ",
                splitProof: Proof(trainRoots: new FaultingReadOnlyList<string>(exception)))));

        Assert.Same(exception, thrown);
    }

    [Fact]
    public void Measure_ReturnsFreshGuardOwnedReadOnlySplitProofUnaffectedByCallerMutation()
    {
        var train = new List<string> { "train-a" };
        var eval = new List<string> { "eval-a" };
        var hashes = new List<string> { "hash-a" };
        var callerProof = Proof(train, eval, hashes);

        var first = DynamicGraphNoveltyMeter.Measure(Observation(splitProof: callerProof));
        var second = DynamicGraphNoveltyMeter.Measure(Observation(splitProof: callerProof));
        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);

        train[0] = "mutated-train";
        eval[0] = "mutated-eval";
        hashes[0] = "mutated-hash";
        Assert.NotSame(callerProof, first.Value!.SplitProof);
        Assert.NotSame(first.Value.SplitProof, second.Value!.SplitProof);
        Assert.Equal(["train-a"], first.Value.SplitProof.TrainSourceRoots);
        Assert.Equal(["eval-a"], first.Value.SplitProof.EvalSourceRoots);
        Assert.Equal(["hash-a"], first.Value.SplitProof.SourceHashes);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<string>)first.Value.SplitProof.TrainSourceRoots).Add("forbidden"));
    }

    private static DynamicGraphNoveltyObservation Observation(
        string evaluationId = "eval-novelty-1",
        int requestedRows = 100,
        int rowsEvaluated = 100,
        int selectionShortfall = 0,
        double newPatternRate = 0.26d,
        double sourceTokenSeenRate = 0.74d,
        double maxAccuracy = 0.75d,
        DynamicGraphMeasurementSplitProof? splitProof = null)
        => new(
            evaluationId,
            requestedRows,
            rowsEvaluated,
            selectionShortfall,
            newPatternRate,
            sourceTokenSeenRate,
            maxAccuracy,
            splitProof ?? Proof());

    private static DynamicGraphMeasurementSplitProof Proof(
        IReadOnlyList<string>? trainRoots = null,
        IReadOnlyList<string>? evalRoots = null,
        IReadOnlyList<string>? sourceHashes = null)
        => new(
            trainRoots ?? ["train-a"],
            evalRoots ?? ["eval-a"],
            sourceHashes ?? ["hash-a"],
            0,
            "simhash",
            0.85d);

    private static void AssertInvalid(OperationResult<DynamicGraphNoveltyMeasurement> result)
        => AssertFailure(result, InvalidCode, InvalidMessage);

    private static DynamicGraphNoveltyMeasurement Successful(
        DynamicGraphNoveltyObservation observation)
    {
        var result = DynamicGraphNoveltyMeter.Measure(observation);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private static void AssertFailure(
        OperationResult<DynamicGraphNoveltyMeasurement> result,
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
