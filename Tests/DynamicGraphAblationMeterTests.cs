// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Collections;
using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class DynamicGraphAblationMeterTests
{
    private const string InvalidCode = "INVALID_ABLATION_MEASUREMENT";
    private const string InvalidMessage = "Dynamic graph ablation measurement is invalid.";
    private const string MismatchCode = "ABLATION_EVALUATION_MISMATCH";
    private const string MismatchMessage = "Dynamic graph ablation measurements do not describe the same held-out evaluation.";
    private const string InvalidSelectionCode =
        "EDGE_AUTHORITY_ABLATION_EVALUATION_RESULT_INVALID";
    private const string InvalidSelectionMessage =
        "Dynamic graph edge authority ablation evaluation result is invalid.";

    [Fact]
    public void Contract_ContainsExactlyTheSevenNumericEvidenceFields()
    {
        AssertRecordShape<DynamicGraphAblationMeasurement>(
            ("EvaluationId", typeof(string)),
            ("RowsEvaluated", typeof(int)),
            ("SubjectHeldOutAccuracy", typeof(double)),
            ("AblatedHeldOutAccuracy", typeof(double)),
            ("AccuracyDrop", typeof(double)),
            ("MeetsQ11NumericalGate", typeof(bool)),
            ("SplitProof", typeof(DynamicGraphMeasurementSplitProof)));

        var forbidden = new[]
        {
            "Novelty", "SourceId", "Trained", "Promotion", "Lifecycle", "QualityGate"
        };
        Assert.DoesNotContain(
            typeof(DynamicGraphAblationMeasurement)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(property => property.Name),
            name => forbidden.Any(term => name.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void Meter_PublicSurfacePreservesOneMeasureAndAddsExactDistinctSelectionProjection()
    {
        var methods = typeof(DynamicGraphAblationMeter)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        Assert.Equal(2, methods.Length);
        var method = Assert.Single(methods, candidate => candidate.Name == "Measure");
        var selection = Assert.Single(
            methods,
            candidate => candidate.Name == "MeasureEdgeAuthoritySelection");
        Assert.Equal(
            [typeof(DynamicGraphAccuracyMeasurement), typeof(DynamicGraphAccuracyMeasurement)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [typeof(DynamicGraphEdgeAuthorityAblationEvaluationResult)],
            selection.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(method.ReturnType, selection.ReturnType);
        Assert.Equal(0.02d, DynamicGraphAblationMeter.MinimumAccuracyDrop);
        Assert.Empty(method.GetMethodBody()!.ExceptionHandlingClauses);
    }

    [Fact]
    public void Measure_UsesSubjectMinusAblatedHeldOutAccuracyAndRetainsRawDrop()
    {
        var result = DynamicGraphAblationMeter.Measure(
            Measurement(heldOutAccuracy: 0.96d),
            Measurement(heldOutAccuracy: 0.93d));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("eval-1", result.Value!.EvaluationId);
        Assert.Equal(50, result.Value.RowsEvaluated);
        Assert.Equal(0.96d, result.Value.SubjectHeldOutAccuracy);
        Assert.Equal(0.93d, result.Value.AblatedHeldOutAccuracy);
        Assert.Equal(0.96d - 0.93d, result.Value.AccuracyDrop);
        Assert.True(result.Value.MeetsQ11NumericalGate);
    }

    [Fact]
    public void Measure_DriftProneExactDecimalBoundaryPassesAfterLockedNormalization()
    {
        var result = DynamicGraphAblationMeter.Measure(
            Measurement(heldOutAccuracy: 0.95d),
            Measurement(heldOutAccuracy: 0.93d));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value!.AccuracyDrop < DynamicGraphAblationMeter.MinimumAccuracyDrop);
        Assert.True(result.Value.MeetsQ11NumericalGate);
    }

    [Theory]
    [InlineData(0.949999999998d, 0.93d)]
    [InlineData(0.93d, 0.93d)]
    [InlineData(0.92d, 0.93d)]
    [InlineData(0.90d, 0.93d)]
    public void Measure_BelowZeroAndNegativeDropsSucceedWithFalseGate(double subject, double ablated)
    {
        var result = DynamicGraphAblationMeter.Measure(
            Measurement(heldOutAccuracy: subject),
            Measurement(heldOutAccuracy: ablated));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(subject - ablated, result.Value!.AccuracyDrop);
        Assert.False(result.Value.MeetsQ11NumericalGate);
    }

    [Fact]
    public void Measure_OnlyHeldOutAccuracyAffectsAblationResult()
    {
        var baseline = DynamicGraphAblationMeter.Measure(
            Measurement(
                liveMarginAccuracy: 0d,
                heldOutAccuracy: 0.96d,
                accuracyDelta: -1d,
                rewardMargin: -100d,
                meetsQ30: false),
            Measurement(
                liveMarginAccuracy: 1d,
                heldOutAccuracy: 0.93d,
                accuracyDelta: 1d,
                rewardMargin: 100d,
                meetsQ30: true));
        var varied = DynamicGraphAblationMeter.Measure(
            Measurement(
                liveMarginAccuracy: 1d,
                heldOutAccuracy: 0.96d,
                accuracyDelta: 0.42d,
                rewardMargin: double.MaxValue,
                meetsQ30: true),
            Measurement(
                liveMarginAccuracy: 0d,
                heldOutAccuracy: 0.93d,
                accuracyDelta: -0.42d,
                rewardMargin: -double.MaxValue,
                meetsQ30: false));

        Assert.True(baseline.IsSuccess, baseline.ErrorMessage);
        Assert.True(varied.IsSuccess, varied.ErrorMessage);
        Assert.Equal(baseline.Value!.EvaluationId, varied.Value!.EvaluationId);
        Assert.Equal(baseline.Value.RowsEvaluated, varied.Value.RowsEvaluated);
        Assert.Equal(baseline.Value.SubjectHeldOutAccuracy, varied.Value.SubjectHeldOutAccuracy);
        Assert.Equal(baseline.Value.AblatedHeldOutAccuracy, varied.Value.AblatedHeldOutAccuracy);
        Assert.Equal(baseline.Value.AccuracyDrop, varied.Value.AccuracyDrop);
        Assert.Equal(baseline.Value.MeetsQ11NumericalGate, varied.Value.MeetsQ11NumericalGate);
        Assert.Equal(baseline.Value.SplitProof.TrainSourceRoots, varied.Value.SplitProof.TrainSourceRoots);
        Assert.Equal(baseline.Value.SplitProof.EvalSourceRoots, varied.Value.SplitProof.EvalSourceRoots);
        Assert.Equal(baseline.Value.SplitProof.SourceHashes, varied.Value.SplitProof.SourceHashes);
    }

    [Fact]
    public void Measure_NullSubjectDoesNotTraverseHostileAblatedSplit()
    {
        var result = DynamicGraphAblationMeter.Measure(
            null,
            Measurement(splitProof: HostileProof(new BadImageFormatException("must not escape"))));

        AssertInvalid(result);
    }

    [Fact]
    public void Measure_NullAblatedDoesNotTraverseHostileSubjectSplit()
    {
        var result = DynamicGraphAblationMeter.Measure(
            Measurement(splitProof: HostileProof(new BadImageFormatException("must not escape"))),
            null);

        AssertInvalid(result);
    }

    [Fact]
    public void Measure_SubjectSplitFailurePrecedesHostileAblatedTraversal()
    {
        var result = DynamicGraphAblationMeter.Measure(
            Measurement(splitProof: Proof(trainRoots: ["shared"], evalRoots: ["shared"])),
            Measurement(splitProof: HostileProof(new BadImageFormatException("must not escape"))));

        AssertFailure(result, "TRAIN_HELDOUT_OVERLAP", "Training and held-out source roots overlap.");
    }

    [Fact]
    public void Measure_InvalidSubjectShapeDoesNotPrecedeInvalidSubjectSplitAdmission()
    {
        var invalidSubject = Measurement(
            heldOutAccuracy: double.NaN,
            splitProof: Proof(trainRoots: ["shared"], evalRoots: ["shared"]));

        var result = DynamicGraphAblationMeter.Measure(invalidSubject, Measurement());

        AssertFailure(result, "TRAIN_HELDOUT_OVERLAP", "Training and held-out source roots overlap.");
    }

    [Fact]
    public void Measure_InvalidSubjectShapeDoesNotPrecedeOrdinaryInvalidAblatedSplitAdmission()
    {
        var invalidSubject = Measurement(heldOutAccuracy: double.NaN);
        var invalidAblatedSplit = Measurement(splitProof: Proof(method: "SimHash"));

        var result = DynamicGraphAblationMeter.Measure(invalidSubject, invalidAblatedSplit);

        AssertFailure(result, "INVALID_SPLIT_PROOF", "Dynamic graph measurement split proof is invalid.");
    }

    [Fact]
    public void Measure_InvalidSubjectShapeDoesNotSuppressFatalAblatedSplitTraversal()
    {
        var fatal = new BadImageFormatException("fatal ablated split");
        var invalidSubject = Measurement(heldOutAccuracy: double.NaN);
        var hostileAblated = Measurement(splitProof: HostileProof(fatal));

        var thrown = Assert.Throws<BadImageFormatException>(() =>
            DynamicGraphAblationMeter.Measure(invalidSubject, hostileAblated));

        Assert.Same(fatal, thrown);
    }

    [Fact]
    public void Measure_InvalidAblatedShapeDoesNotPrecedeInvalidAblatedSplitAdmission()
    {
        var invalidAblated = Measurement(
            heldOutAccuracy: double.NaN,
            splitProof: Proof(method: "SimHash"));

        var result = DynamicGraphAblationMeter.Measure(Measurement(), invalidAblated);

        AssertFailure(result, "INVALID_SPLIT_PROOF", "Dynamic graph measurement split proof is invalid.");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("overlap")]
    [InlineData("near-duplicate")]
    public void Measure_SubjectSplitGuardErrorIsMappedExactlyBeforeAblated(string condition)
    {
        var proof = condition switch
        {
            "overlap" => Proof(trainRoots: ["shared"], evalRoots: ["shared"], nearDuplicateCount: 1),
            "near-duplicate" => Proof(nearDuplicateCount: 1),
            _ => Proof(method: "SimHash")
        };

        var result = DynamicGraphAblationMeter.Measure(
            Measurement(splitProof: proof),
            Measurement(splitProof: HostileProof(new BadImageFormatException("must not escape"))));

        AssertSplitFailure(result, condition);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("overlap")]
    [InlineData("near-duplicate")]
    public void Measure_AblatedSplitGuardErrorIsMappedExactlyAfterValidSubject(string condition)
    {
        var proof = condition switch
        {
            "overlap" => Proof(trainRoots: ["shared"], evalRoots: ["shared"], nearDuplicateCount: 1),
            "near-duplicate" => Proof(nearDuplicateCount: 1),
            _ => Proof(method: "SimHash")
        };

        var result = DynamicGraphAblationMeter.Measure(
            Measurement(),
            Measurement(splitProof: proof));

        AssertSplitFailure(result, condition);
    }

    [Theory]
    [InlineData("subject", "ordinary")]
    [InlineData("subject", "nested-ordinary")]
    [InlineData("ablated", "ordinary")]
    [InlineData("ablated", "nested-ordinary")]
    public void Measure_OrdinaryFaultingSplitSnapshotsFailClosed(string position, string graph)
    {
        Exception exception = graph == "ordinary"
            ? new InvalidOperationException("ordinary")
            : new AggregateException(new InvalidOperationException("nested ordinary"));
        var hostile = Measurement(splitProof: HostileProof(exception));

        var result = DynamicGraphAblationMeter.Measure(
            position == "subject" ? hostile : Measurement(),
            position == "ablated" ? hostile : Measurement());

        AssertFailure(result, "INVALID_SPLIT_PROOF", "Dynamic graph measurement split proof is invalid.");
    }

    [Theory]
    [InlineData("subject", "cancellation")]
    [InlineData("subject", "wrapped-cancellation")]
    [InlineData("subject", "fatal")]
    [InlineData("subject", "wrapped-fatal")]
    [InlineData("ablated", "cancellation")]
    [InlineData("ablated", "wrapped-cancellation")]
    [InlineData("ablated", "fatal")]
    [InlineData("ablated", "wrapped-fatal")]
    public void Measure_CancellationAndFatalSplitSnapshotGraphsEscapeUnchanged(
        string position,
        string graph)
    {
        Exception exception = graph switch
        {
            "cancellation" => new OperationCanceledException("cancel"),
            "wrapped-cancellation" => new WrapperException(new OperationCanceledException("cancel")),
            "fatal" => new BadImageFormatException("fatal"),
            _ => new AggregateException(
                new InvalidOperationException("ordinary"),
                new WrapperException(new BadImageFormatException("fatal")))
        };
        var hostile = Measurement(splitProof: HostileProof(exception));

        var thrown = Assert.Throws(exception.GetType(), () => DynamicGraphAblationMeter.Measure(
            position == "subject" ? hostile : Measurement(),
            position == "ablated" ? hostile : Measurement()));

        Assert.Same(exception, thrown);
    }

    [Theory]
    [MemberData(nameof(InvalidShapeCases))]
    public void Measure_InvalidFullShapeFailsClosedInEitherPosition(string position, string invalidPart)
    {
        var invalid = InvalidMeasurementShape(invalidPart);
        var result = DynamicGraphAblationMeter.Measure(
            position == "subject" ? invalid : Measurement(),
            position == "ablated" ? invalid : Measurement());

        AssertInvalid(result);
    }

    [Theory]
    [InlineData("subject")]
    [InlineData("ablated")]
    public void Measure_InvalidShapePrecedesEvaluationIdentityComparison(string position)
    {
        var invalid = Measurement(evaluationId: "eval-invalid-side") with
        {
            Accuracy = new DynamicGraphAccuracyAxis(0d, double.NaN, 0d)
        };
        var other = Measurement(evaluationId: "eval-other-side");

        var result = DynamicGraphAblationMeter.Measure(
            position == "subject" ? invalid : other,
            position == "ablated" ? invalid : other);

        AssertInvalid(result);
    }

    [Theory]
    [MemberData(nameof(MismatchCases))]
    public void Measure_EveryEvaluationAndSplitIdentityDifferenceReturnsStableMismatch(string dimension)
    {
        var subject = Measurement();
        var ablated = dimension switch
        {
            "evaluation-id-case" => Measurement(evaluationId: "Eval-1"),
            "rows" => Measurement(rowsEvaluated: 51),
            "train-case" => Measurement(splitProof: Proof(trainRoots: ["Train-a", "train-b"])),
            "train-order" => Measurement(splitProof: Proof(trainRoots: ["train-b", "train-a"])),
            "train-cardinality" => Measurement(splitProof: Proof(trainRoots: ["train-a"])),
            "train-value" => Measurement(splitProof: Proof(trainRoots: ["train-a", "train-c"])),
            "eval-case" => Measurement(splitProof: Proof(evalRoots: ["Eval-a", "eval-b"])),
            "eval-order" => Measurement(splitProof: Proof(evalRoots: ["eval-b", "eval-a"])),
            "eval-cardinality" => Measurement(splitProof: Proof(evalRoots: ["eval-a"])),
            "eval-value" => Measurement(splitProof: Proof(evalRoots: ["eval-a", "eval-c"])),
            "hash-case" => Measurement(splitProof: Proof(hashes: ["Hash-a", "hash-b"])),
            "hash-order" => Measurement(splitProof: Proof(hashes: ["hash-b", "hash-a"])),
            "hash-cardinality" => Measurement(splitProof: Proof(hashes: ["hash-a"])),
            "hash-value" => Measurement(splitProof: Proof(hashes: ["hash-a", "hash-c"])),
            "method" => Measurement(splitProof: Proof(method: "minhash")),
            "threshold" => Measurement(splitProof: Proof(threshold: 0.86d)),
            _ => throw new ArgumentOutOfRangeException(nameof(dimension))
        };

        var result = DynamicGraphAblationMeter.Measure(subject, ablated);

        AssertMismatch(result);
    }

    [Theory]
    [InlineData("subject")]
    [InlineData("ablated")]
    public void Measure_NonZeroNearDuplicateCountInEitherPositionIsRejectedByPublicGuardAdmission(
        string position)
    {
        var contaminated = Measurement(splitProof: Proof(nearDuplicateCount: 1));
        var result = DynamicGraphAblationMeter.Measure(
            position == "subject" ? contaminated : Measurement(),
            position == "ablated" ? contaminated : Measurement());

        AssertFailure(
            result,
            "TRAIN_HELDOUT_NEAR_DUPLICATE",
            "Training and held-out sources contain near-duplicates.");
    }

    [Fact]
    public void Measure_PostAdmissionNearDuplicateCountEqualityIsGuaranteedByBothPublicGuards()
    {
        var subjectProof = Proof();
        var ablatedProof = Proof();
        var admittedSubject = DynamicGraphHeldOutSplitGuard.Validate(subjectProof);
        var admittedAblated = DynamicGraphHeldOutSplitGuard.Validate(ablatedProof);

        Assert.True(admittedSubject.IsSuccess, admittedSubject.ErrorMessage);
        Assert.True(admittedAblated.IsSuccess, admittedAblated.ErrorMessage);
        Assert.NotSame(subjectProof, admittedSubject.Value);
        Assert.NotSame(ablatedProof, admittedAblated.Value);
        Assert.Equal(0, admittedSubject.Value!.CrossSplitNearDuplicateCount);
        Assert.Equal(0, admittedAblated.Value!.CrossSplitNearDuplicateCount);
        Assert.Equal(
            admittedSubject.Value.CrossSplitNearDuplicateCount,
            admittedAblated.Value.CrossSplitNearDuplicateCount);

        var result = DynamicGraphAblationMeter.Measure(
            Measurement(heldOutAccuracy: 0.96d, splitProof: subjectProof),
            Measurement(heldOutAccuracy: 0.93d, splitProof: ablatedProof));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, result.Value!.SplitProof.CrossSplitNearDuplicateCount);
    }

    [Fact]
    public void Measure_ReturnsSubjectGuardOwnedReadOnlySplitSnapshotAndEveryExpectedScalar()
    {
        var train = new List<string> { "train-a", "train-b" };
        var eval = new List<string> { "eval-a", "eval-b" };
        var hashes = new List<string> { "hash-a", "hash-b" };
        var callerProof = Proof(train, eval, hashes, threshold: 0.85d);
        var result = DynamicGraphAblationMeter.Measure(
            Measurement(heldOutAccuracy: 0.96d, splitProof: callerProof),
            Measurement(heldOutAccuracy: 0.93d, splitProof: Proof()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        train[0] = "mutated-train";
        eval[0] = "mutated-eval";
        hashes[0] = "mutated-hash";

        var value = result.Value!;
        Assert.Equal("eval-1", value.EvaluationId);
        Assert.Equal(50, value.RowsEvaluated);
        Assert.Equal(0.96d, value.SubjectHeldOutAccuracy);
        Assert.Equal(0.93d, value.AblatedHeldOutAccuracy);
        Assert.Equal(0.96d - 0.93d, value.AccuracyDrop);
        Assert.True(value.MeetsQ11NumericalGate);
        Assert.NotSame(callerProof, value.SplitProof);
        Assert.Equal(["train-a", "train-b"], value.SplitProof.TrainSourceRoots);
        Assert.Equal(["eval-a", "eval-b"], value.SplitProof.EvalSourceRoots);
        Assert.Equal(["hash-a", "hash-b"], value.SplitProof.SourceHashes);
        Assert.Equal(0, value.SplitProof.CrossSplitNearDuplicateCount);
        Assert.Equal("simhash", value.SplitProof.SimhashOrMinhash);
        Assert.Equal(0.85d, value.SplitProof.NearDuplicateThreshold);
        AssertReadOnly(value.SplitProof.TrainSourceRoots);
        AssertReadOnly(value.SplitProof.EvalSourceRoots);
        AssertReadOnly(value.SplitProof.SourceHashes);
    }

    [Theory]
    [InlineData(95, 93, 0.95d, 0.93d, true)]
    [InlineData(94, 93, 0.94d, 0.93d, false)]
    public void MeasureEdgeAuthoritySelection_ProjectsOnlyValidatedPairedSelection(
        int learnedCorrect,
        int baselineCorrect,
        double expectedLearnedAccuracy,
        double expectedBaselineAccuracy,
        bool expectedGate)
    {
        var carrier = SelectionCarrier(100, learnedCorrect, baselineCorrect);

        var result = DynamicGraphAblationMeter.MeasureEdgeAuthoritySelection(carrier);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("edge-selection-eval", result.Value!.EvaluationId);
        Assert.Equal(100, result.Value.RowsEvaluated);
        Assert.Equal(expectedLearnedAccuracy, result.Value.SubjectHeldOutAccuracy);
        Assert.Equal(expectedBaselineAccuracy, result.Value.AblatedHeldOutAccuracy);
        Assert.Equal(expectedLearnedAccuracy - expectedBaselineAccuracy, result.Value.AccuracyDrop);
        Assert.Equal(expectedGate, result.Value.MeetsQ11NumericalGate);
        Assert.Equal(["eval-a"], result.Value.SplitProof.EvalSourceRoots);
        Assert.Equal(["hash-a"], result.Value.SplitProof.SourceHashes);
        AssertReadOnly(result.Value.SplitProof.EvalSourceRoots);
        AssertReadOnly(result.Value.SplitProof.SourceHashes);
    }

    [Fact]
    public void MeasureEdgeAuthoritySelection_DriftProneExactThresholdUsesLockedNormalization()
    {
        var result = DynamicGraphAblationMeter.MeasureEdgeAuthoritySelection(
            SelectionCarrier(100, learnedCorrect: 95, baselineCorrect: 93));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value!.AccuracyDrop < 0.02d);
        Assert.Equal(0.95d - 0.93d, result.Value.AccuracyDrop);
        Assert.True(result.Value.MeetsQ11NumericalGate);
    }

    [Fact]
    public void MeasureEdgeAuthoritySelection_NullFailsWithLockedResultError()
    {
        var result = DynamicGraphAblationMeter.MeasureEdgeAuthoritySelection(null);

        AssertSelectionInvalid(result);
    }

    [Theory]
    [InlineData("learned-count")]
    [InlineData("baseline-count")]
    [InlineData("learned-accuracy")]
    [InlineData("baseline-accuracy")]
    [InlineData("raw-drop")]
    [InlineData("normalized-drop")]
    [InlineData("gate")]
    [InlineData("row-boolean")]
    public void MeasureEdgeAuthoritySelection_StaleCarrierFailsWithLockedResultError(
        string dimension)
    {
        var valid = SelectionCarrier(10, learnedCorrect: 8, baselineCorrect: 5);
        var stale = StaleSelectionCarrier(valid, dimension);

        var result = DynamicGraphAblationMeter.MeasureEdgeAuthoritySelection(stale);

        AssertSelectionInvalid(result);
    }

    [Theory]
    [InlineData("invalid", "INVALID_SPLIT_PROOF", "Dynamic graph measurement split proof is invalid.")]
    [InlineData("overlap", "TRAIN_HELDOUT_OVERLAP", "Training and held-out source roots overlap.")]
    [InlineData("near", "TRAIN_HELDOUT_NEAR_DUPLICATE", "Training and held-out sources contain near-duplicates.")]
    public void MeasureEdgeAuthoritySelection_SplitFailurePrecedesStaleNumericCarrier(
        string splitFailure,
        string expectedCode,
        string expectedMessage)
    {
        var split = splitFailure switch
        {
            "overlap" => SelectionProof(train: ["shared"], eval: ["shared"]),
            "near" => SelectionProof(nearDuplicateCount: 1),
            _ => SelectionProof(method: "SimHash")
        };
        var stale = SelectionCarrier(
            10,
            learnedCorrect: 8,
            baselineCorrect: 5,
            split: split,
            storedRawDrop: 0.99d);

        var result = DynamicGraphAblationMeter.MeasureEdgeAuthoritySelection(stale);

        AssertFailure(result, expectedCode, expectedMessage);
    }

    [Fact]
    public void MeasureEdgeAuthoritySelection_ReturnsOwnedAdmittedSplit()
    {
        var train = new List<string> { "train-a" };
        var eval = new List<string> { "eval-a" };
        var hashes = new List<string> { "hash-a" };
        var split = SelectionProof(train, eval, hashes);
        var result = DynamicGraphAblationMeter.MeasureEdgeAuthoritySelection(
            SelectionCarrier(10, 8, 5, split));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        train[0] = "mutated-train";
        eval[0] = "mutated-eval";
        hashes[0] = "mutated-hash";
        Assert.Equal(["train-a"], result.Value!.SplitProof.TrainSourceRoots);
        Assert.Equal(["eval-a"], result.Value.SplitProof.EvalSourceRoots);
        Assert.Equal(["hash-a"], result.Value.SplitProof.SourceHashes);
    }

    public static TheoryData<string, string> InvalidShapeCases()
    {
        var cases = new TheoryData<string, string>();
        var parts = new[]
        {
            "empty-id", "whitespace-id", "leading-id", "trailing-id", "case-whitespace-id",
            "control-id", "rows-zero", "rows-negative", "null-axis",
            "live-nan", "live-positive-infinity", "live-negative-infinity", "live-below", "live-above",
            "heldout-nan", "heldout-positive-infinity", "heldout-negative-infinity", "heldout-below", "heldout-above",
            "delta-nan", "delta-positive-infinity", "delta-negative-infinity", "delta-below", "delta-above",
            "reward-nan", "reward-positive-infinity", "reward-negative-infinity"
        };
        foreach (var position in new[] { "subject", "ablated" })
        {
            foreach (var part in parts)
                cases.Add(position, part);
        }

        return cases;
    }

    public static TheoryData<string> MismatchCases()
    {
        var cases = new TheoryData<string>();
        foreach (var dimension in new[]
                 {
                     "evaluation-id-case", "rows",
                     "train-case", "train-order", "train-cardinality", "train-value",
                     "eval-case", "eval-order", "eval-cardinality", "eval-value",
                     "hash-case", "hash-order", "hash-cardinality", "hash-value",
                     "method", "threshold"
                 })
        {
            cases.Add(dimension);
        }

        return cases;
    }

    private static DynamicGraphAccuracyMeasurement InvalidMeasurementShape(string part)
    {
        var measurement = Measurement();
        return part switch
        {
            "empty-id" => measurement with { EvaluationId = "" },
            "whitespace-id" => measurement with { EvaluationId = " " },
            "leading-id" => measurement with { EvaluationId = " eval-1" },
            "trailing-id" => measurement with { EvaluationId = "eval-1 " },
            "case-whitespace-id" => measurement with { EvaluationId = "\teval-1" },
            "control-id" => measurement with { EvaluationId = "eval\u2028one" },
            "rows-zero" => measurement with { RowsEvaluated = 0 },
            "rows-negative" => measurement with { RowsEvaluated = -1 },
            "null-axis" => measurement with { Accuracy = null! },
            "live-nan" => WithAxis(measurement, live: double.NaN),
            "live-positive-infinity" => WithAxis(measurement, live: double.PositiveInfinity),
            "live-negative-infinity" => WithAxis(measurement, live: double.NegativeInfinity),
            "live-below" => WithAxis(measurement, live: -double.Epsilon),
            "live-above" => WithAxis(measurement, live: Math.BitIncrement(1d)),
            "heldout-nan" => WithAxis(measurement, heldOut: double.NaN),
            "heldout-positive-infinity" => WithAxis(measurement, heldOut: double.PositiveInfinity),
            "heldout-negative-infinity" => WithAxis(measurement, heldOut: double.NegativeInfinity),
            "heldout-below" => WithAxis(measurement, heldOut: -double.Epsilon),
            "heldout-above" => WithAxis(measurement, heldOut: Math.BitIncrement(1d)),
            "delta-nan" => WithAxis(measurement, delta: double.NaN),
            "delta-positive-infinity" => WithAxis(measurement, delta: double.PositiveInfinity),
            "delta-negative-infinity" => WithAxis(measurement, delta: double.NegativeInfinity),
            "delta-below" => WithAxis(measurement, delta: Math.BitDecrement(-1d)),
            "delta-above" => WithAxis(measurement, delta: Math.BitIncrement(1d)),
            "reward-nan" => measurement with { HeldOutRewardMargin = double.NaN },
            "reward-positive-infinity" => measurement with { HeldOutRewardMargin = double.PositiveInfinity },
            "reward-negative-infinity" => measurement with { HeldOutRewardMargin = double.NegativeInfinity },
            _ => throw new ArgumentOutOfRangeException(nameof(part))
        };
    }

    private static DynamicGraphAccuracyMeasurement WithAxis(
        DynamicGraphAccuracyMeasurement measurement,
        double? live = null,
        double? heldOut = null,
        double? delta = null)
        => measurement with
        {
            Accuracy = measurement.Accuracy with
            {
                LiveMarginAccuracy = live ?? measurement.Accuracy.LiveMarginAccuracy,
                HeldOutAccuracy = heldOut ?? measurement.Accuracy.HeldOutAccuracy,
                AccuracyDelta = delta ?? measurement.Accuracy.AccuracyDelta
            }
        };

    private static DynamicGraphEdgeAuthorityAblationEvaluationResult SelectionCarrier(
        int rowCount,
        int learnedCorrect,
        int baselineCorrect,
        DynamicGraphMeasurementSplitProof? split = null,
        double? storedRawDrop = null)
    {
        var rows = Enumerable.Range(0, rowCount)
            .Select(index =>
            {
                var learnedMatches = index < learnedCorrect;
                var baselineMatches = index < baselineCorrect;
                return new DynamicGraphEdgeAuthorityAblationRowEvidence(
                    $"row-{index:D3}",
                    "eval-a",
                    "hash-a",
                    "document-a",
                    "repo://eval/a",
                    $"label-attestation-{index:D3}",
                    "target-a",
                    learnedMatches ? "target-a" : "target-b",
                    baselineMatches ? "target-a" : "target-b",
                    learnedMatches,
                    baselineMatches);
            })
            .ToArray();
        var learnedAccuracy = (double)learnedCorrect / rowCount;
        var baselineAccuracy = (double)baselineCorrect / rowCount;
        var rawDrop = storedRawDrop ?? learnedAccuracy - baselineAccuracy;
        var normalizedDrop = Math.Round(rawDrop, 12, MidpointRounding.AwayFromZero);
        return ConstructSelectionCarrier(
            "edge-selection-eval",
            "edge-selection-edge",
            "edge-selection-authority",
            split ?? SelectionProof(),
            rows,
            learnedCorrect,
            learnedAccuracy,
            baselineCorrect,
            baselineAccuracy,
            rawDrop,
            normalizedDrop,
            normalizedDrop >= 0.02d);
    }

    private static DynamicGraphEdgeAuthorityAblationEvaluationResult StaleSelectionCarrier(
        DynamicGraphEdgeAuthorityAblationEvaluationResult source,
        string dimension)
    {
        var rows = source.Rows.ToArray();
        if (dimension == "row-boolean")
        {
            rows[0] = rows[0] with
            {
                LearnedTopMatchesExpected = !rows[0].LearnedTopMatchesExpected
            };
        }

        return ConstructSelectionCarrier(
            source.EvaluationId,
            source.EdgeId,
            source.AuthorityUnitId,
            source.SplitProof,
            rows,
            dimension == "learned-count"
                ? source.LearnedCorrectCount - 1
                : source.LearnedCorrectCount,
            dimension == "learned-accuracy"
                ? source.LearnedSelectionAccuracy - 0.01d
                : source.LearnedSelectionAccuracy,
            dimension == "baseline-count"
                ? source.BaselineCorrectCount - 1
                : source.BaselineCorrectCount,
            dimension == "baseline-accuracy"
                ? source.BaselineSelectionAccuracy - 0.01d
                : source.BaselineSelectionAccuracy,
            dimension == "raw-drop" ? source.AccuracyDrop + 0.01d : source.AccuracyDrop,
            dimension == "normalized-drop"
                ? source.NormalizedAccuracyDrop + 0.01d
                : source.NormalizedAccuracyDrop,
            dimension == "gate" ? !source.MeetsQ11NumericalGate : source.MeetsQ11NumericalGate);
    }

    private static DynamicGraphEdgeAuthorityAblationEvaluationResult ConstructSelectionCarrier(
        string evaluationId,
        string edgeId,
        string authorityUnitId,
        DynamicGraphMeasurementSplitProof split,
        IReadOnlyList<DynamicGraphEdgeAuthorityAblationRowEvidence> rows,
        int learnedCorrectCount,
        double learnedAccuracy,
        int baselineCorrectCount,
        double baselineAccuracy,
        double accuracyDrop,
        double normalizedAccuracyDrop,
        bool meetsGate)
    {
        var constructor = Assert.Single(
            typeof(DynamicGraphEdgeAuthorityAblationEvaluationResult)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance),
            candidate => candidate.GetParameters().Length == 12);
        return Assert.IsType<DynamicGraphEdgeAuthorityAblationEvaluationResult>(
            constructor.Invoke(
            [
                evaluationId,
                edgeId,
                authorityUnitId,
                split,
                rows,
                learnedCorrectCount,
                learnedAccuracy,
                baselineCorrectCount,
                baselineAccuracy,
                accuracyDrop,
                normalizedAccuracyDrop,
                meetsGate
            ]));
    }

    private static DynamicGraphMeasurementSplitProof SelectionProof(
        IReadOnlyList<string>? train = null,
        IReadOnlyList<string>? eval = null,
        IReadOnlyList<string>? hashes = null,
        int nearDuplicateCount = 0,
        string method = "simhash")
        => new(
            train ?? ["train-a"],
            eval ?? ["eval-a"],
            hashes ?? ["hash-a"],
            nearDuplicateCount,
            method,
            0.85d);

    private static DynamicGraphAccuracyMeasurement Measurement(
        string evaluationId = "eval-1",
        int rowsEvaluated = 50,
        double liveMarginAccuracy = 0.5d,
        double heldOutAccuracy = 0.95d,
        double accuracyDelta = 0.02d,
        double rewardMargin = 0.025d,
        bool meetsQ30 = true,
        DynamicGraphMeasurementSplitProof? splitProof = null)
        => new(
            evaluationId,
            rowsEvaluated,
            new DynamicGraphAccuracyAxis(liveMarginAccuracy, heldOutAccuracy, accuracyDelta),
            rewardMargin,
            meetsQ30,
            splitProof ?? Proof());

    private static DynamicGraphMeasurementSplitProof Proof(
        IReadOnlyList<string>? trainRoots = null,
        IReadOnlyList<string>? evalRoots = null,
        IReadOnlyList<string>? hashes = null,
        int nearDuplicateCount = 0,
        string method = "simhash",
        double threshold = 0.85d)
        => new(
            trainRoots ?? ["train-a", "train-b"],
            evalRoots ?? ["eval-a", "eval-b"],
            hashes ?? ["hash-a", "hash-b"],
            nearDuplicateCount,
            method,
            threshold);

    private static DynamicGraphMeasurementSplitProof HostileProof(Exception exception)
        => Proof(trainRoots: new FaultingReadOnlyList<string>(exception));

    private static void AssertSplitFailure(
        OperationResult<DynamicGraphAblationMeasurement> result,
        string condition)
    {
        var expected = condition switch
        {
            "overlap" => ("TRAIN_HELDOUT_OVERLAP", "Training and held-out source roots overlap."),
            "near-duplicate" => ("TRAIN_HELDOUT_NEAR_DUPLICATE", "Training and held-out sources contain near-duplicates."),
            _ => ("INVALID_SPLIT_PROOF", "Dynamic graph measurement split proof is invalid.")
        };
        AssertFailure(result, expected.Item1, expected.Item2);
    }

    private static void AssertInvalid(OperationResult<DynamicGraphAblationMeasurement> result)
        => AssertFailure(result, InvalidCode, InvalidMessage);

    private static void AssertSelectionInvalid(
        OperationResult<DynamicGraphAblationMeasurement> result)
        => AssertFailure(result, InvalidSelectionCode, InvalidSelectionMessage);

    private static void AssertMismatch(OperationResult<DynamicGraphAblationMeasurement> result)
        => AssertFailure(result, MismatchCode, MismatchMessage);

    private static void AssertFailure(
        OperationResult<DynamicGraphAblationMeasurement> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    private static void AssertReadOnly(IReadOnlyList<string> values)
        => Assert.Throws<NotSupportedException>(() => ((IList<string>)values).Add("forbidden"));

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

    private sealed class WrapperException(Exception innerException)
        : Exception("wrapper", innerException);
}
