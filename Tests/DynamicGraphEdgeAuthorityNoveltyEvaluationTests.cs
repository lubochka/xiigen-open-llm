// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Collections;
using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class DynamicGraphEdgeAuthorityNoveltyEvaluationTests
{
    private const string A = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string B = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string C = "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";
    private const string D = "dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd";
    private const string UpperA = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
    private const string NonHex = "gaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string EvidenceInvalidCode = "EDGE_AUTHORITY_NOVELTY_EVIDENCE_INVALID";
    private const string EvidenceInvalidMessage =
        "Dynamic graph edge authority novelty evidence is invalid.";
    private const string PairedInvalidCode =
        "EDGE_AUTHORITY_ABLATION_EVALUATION_RESULT_INVALID";
    private const string PairedInvalidMessage =
        "Dynamic graph edge authority ablation evaluation result is invalid.";

    [Fact]
    public void PublicSurface_IsSynchronousPureAndRequestCarriesNoAggregatesOrRowSourceIdentity()
    {
        var method = Assert.Single(
            typeof(DynamicGraphEdgeAuthorityNoveltyEvaluationProducer)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Equal("Produce", method.Name);
        Assert.Equal(
            typeof(OperationResult<DynamicGraphEdgeAuthorityNoveltyEvaluationResult>),
            method.ReturnType);
        Assert.Equal(
            [typeof(DynamicGraphEdgeAuthorityNoveltyEvaluationRequest)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.False(typeof(Task).IsAssignableFrom(method.ReturnType));
        Assert.DoesNotContain(
            method.GetParameters(),
            parameter => parameter.ParameterType == typeof(CancellationToken));
        Assert.Empty(
            typeof(DynamicGraphEdgeAuthorityNoveltyEvaluationProducer)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

        Assert.Equal(
            "sha256-lowerhex-v1",
            DynamicGraphEdgeAuthorityNoveltyEvaluationResult.FingerprintAlgorithmId);
        Assert.Equal(
            "paired-heldout-top1-max-v1",
            DynamicGraphEdgeAuthorityNoveltyEvaluationResult.ProducedMaxAccuracyRuleId);

        var requestProperties = typeof(DynamicGraphEdgeAuthorityNoveltyEvaluationRequest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Equal(
            [
                "PairedSelection", "TrainingBindings", "RequestedRowIds", "NoveltyRows",
                "CallerAttestedFingerprintNormalizationId", "CallerAttestedTokenizationId"
            ],
            requestProperties.Select(property => property.Name));
        Assert.DoesNotContain(
            requestProperties,
            property => new[] { "Rate", "Count", "Gate", "Accuracy" }
                .Any(term => property.Name.Contains(term, StringComparison.OrdinalIgnoreCase)));

        Assert.Equal(
            ["RowId", "CallerAttestedPatternFingerprint", "CallerAttestedTokenFingerprints"],
            typeof(DynamicGraphEdgeAuthorityNoveltyRowInput)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name));
    }

    [Fact]
    public void Produce_NullRequestReturnsLockedRequiredFailure()
        => AssertFailure(
            DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(null),
            "EDGE_AUTHORITY_NOVELTY_REQUEST_REQUIRED",
            "Dynamic graph edge authority novelty evaluation request is required.");

    [Fact]
    public void Produce_HappyPathDeepOwnsEveryCarrierAndMatchesDirectMeterProjection()
    {
        var requested = new List<string> { "row-000", "row-001" };
        var trainPatterns = new List<string> { A };
        var trainTokens = new List<string> { A };
        var firstTokens = new List<string> { B };
        var training = new List<DynamicGraphEdgeAuthorityNoveltyTrainingBinding>
        {
            new("train-a", "train-hash", trainPatterns, trainTokens)
        };
        var rows = new List<DynamicGraphEdgeAuthorityNoveltyRowInput>
        {
            new("row-000", B, firstTokens),
            new("row-001", C, [C])
        };

        var result = DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(
            Request(Paired(), training, requested, rows));

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var value = result.Value!;
        requested[0] = "mutated-request";
        trainPatterns[0] = D;
        trainTokens[0] = D;
        firstTokens[0] = D;
        training.Clear();
        rows.Clear();

        Assert.Equal("fingerprint-normalization-v1", value.CallerAttestedFingerprintNormalizationId);
        Assert.Equal("tokenization-v1", value.CallerAttestedTokenizationId);
        Assert.Equal("paired-heldout-top1-max-v1", value.MaxAccuracyRuleId);
        Assert.Equal(["row-000", "row-001"], value.RequestedRowIds);
        Assert.Empty(value.MissingRowIds);
        Assert.Equal([A], Assert.Single(value.TrainingEvidence).CallerAttestedPatternFingerprints);
        Assert.Equal([A], Assert.Single(value.TrainingEvidence).CallerAttestedTokenFingerprints);
        Assert.Equal([B], value.Rows[0].CallerAttestedTokenFingerprints);
        Assert.True(value.Rows[0].IsNewPattern);
        Assert.Empty(value.Rows[0].SeenTrainingTokenFingerprints);
        Assert.Equal("eval-a", value.Rows[0].EvalSourceRoot);
        Assert.Equal("eval-hash", value.Rows[0].CallerAttestedSourceHash);
        Assert.Equal("document-000", value.Rows[0].TraversalDocumentId);
        Assert.Equal("repo://eval/a", value.Rows[0].TraversalSourcePath);
        Assert.Equal("label-000", value.Rows[0].CallerAttestedLabelProvenanceId);
        Assert.NotSame(value.RequestedRowIds, value.RequestedRowIds);
        Assert.NotSame(value.TrainingEvidence, value.TrainingEvidence);
        Assert.NotSame(value.Rows, value.Rows);
        Assert.NotSame(value.PairedSelection, value.PairedSelection);
        Assert.NotSame(value.Observation, value.Observation);
        Assert.NotSame(value.Measurement, value.Measurement);
        AssertReadOnly(value.RequestedRowIds);
        AssertReadOnly(value.TrainingEvidence);
        AssertReadOnly(value.Rows);
        AssertReadOnly(value.Rows[0].CallerAttestedTokenFingerprints);

        Assert.Equal(1d, value.Observation.NewPatternRate);
        Assert.Equal(0d, value.Observation.SourceTokenSeenRate);
        Assert.Equal(1d, value.Observation.MaxAccuracy);
        Assert.True(value.Measurement.MeetsNoveltyGate);
        var direct = DynamicGraphNoveltyMeter.Measure(value.Observation);
        Assert.True(direct.IsSuccess, direct.ErrorMessage);
        AssertMeasurementsEqual(direct.Value!, value.Measurement);
    }

    [Fact]
    public void Produce_NewPatternRateIsRowWeightedForRepeatedEvalPattern()
    {
        var paired = Paired(rowCount: 3, learnedCorrect: 2, baselineCorrect: 1);
        var rows = new[]
        {
            new DynamicGraphEdgeAuthorityNoveltyRowInput("row-000", B, [B]),
            new DynamicGraphEdgeAuthorityNoveltyRowInput("row-001", B, [C]),
            new DynamicGraphEdgeAuthorityNoveltyRowInput("row-002", A, [D])
        };

        var result = Successful(Request(paired, noveltyRows: rows));

        Assert.Equal(2d / 3d, result.Observation.NewPatternRate);
        Assert.NotEqual(1d / 2d, result.Observation.NewPatternRate);
        Assert.Equal([true, true, false], result.Rows.Select(row => row.IsNewPattern));
    }

    [Fact]
    public void Produce_SourceTokenSeenRateUsesUniqueGlobalTokenSets()
    {
        var rows = new[]
        {
            new DynamicGraphEdgeAuthorityNoveltyRowInput("row-000", B, [A, B]),
            new DynamicGraphEdgeAuthorityNoveltyRowInput("row-001", C, [B, C])
        };

        var result = Successful(Request(Paired(), noveltyRows: rows));

        Assert.Equal(1d / 3d, result.Observation.SourceTokenSeenRate);
        Assert.NotEqual(1d / 2d, result.Observation.SourceTokenSeenRate);
        Assert.Equal([A], result.Rows[0].SeenTrainingTokenFingerprints);
        Assert.Empty(result.Rows[1].SeenTrainingTokenFingerprints);
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(1, 2)]
    public void Produce_MaxAccuracyUsesExactLearnedOrBaselineMaximum(
        int learnedCorrect,
        int baselineCorrect)
    {
        var result = Successful(Request(Paired(
            rowCount: 3,
            learnedCorrect: learnedCorrect,
            baselineCorrect: baselineCorrect)));

        Assert.Equal(2d / 3d, result.Observation.MaxAccuracy);
        Assert.Equal(2d / 3d, result.Measurement.MaxAccuracy);
        Assert.Equal("paired-heldout-top1-max-v1", result.MaxAccuracyRuleId);
    }

    [Fact]
    public void Produce_OrderedRequestedCohortDerivesMissingIdsAndFalseGateAsSuccess()
    {
        var requested = new[]
        {
            "missing-before", "row-000", "missing-between", "row-001", "missing-after"
        };

        var result = DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(
            Request(Paired(), requestedRowIds: requested));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(requested, result.Value!.RequestedRowIds);
        Assert.Equal(
            ["missing-before", "missing-between", "missing-after"],
            result.Value.MissingRowIds);
        Assert.Equal(5, result.Value.Observation.RequestedRows);
        Assert.Equal(2, result.Value.Observation.RowsEvaluated);
        Assert.Equal(3, result.Value.Observation.SelectionShortfall);
        Assert.False(result.Value.Measurement.MeetsNoveltyGate);
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("duplicate")]
    [InlineData("noncanonical")]
    [InlineData("reordered")]
    [InlineData("nonsubset")]
    public void Produce_InvalidRequestedCohortFailsClosed(string condition)
    {
        IReadOnlyList<string> requested = condition switch
        {
            "empty" => [],
            "duplicate" => ["row-000", "row-000", "row-001"],
            "noncanonical" => [" row-000", "row-001"],
            "reordered" => ["row-001", "row-000"],
            _ => ["row-000"]
        };

        AssertEvidenceInvalid(DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(
            Request(Paired(), requestedRowIds: requested)));
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("extra")]
    [InlineData("reordered")]
    [InlineData("duplicate")]
    public void Produce_TrainingBindingsMustExactlyFollowSplitTrainRootOrder(string condition)
    {
        var split = Proof(
            trainRoots: ["train-a", "train-b"],
            hashes: ["train-hash-a", "train-hash-b", "eval-hash"]);
        var paired = Paired(split: split);
        var first = Training("train-a", "train-hash-a", [A], [A]);
        var second = Training("train-b", "train-hash-b", [B], [B]);
        IReadOnlyList<DynamicGraphEdgeAuthorityNoveltyTrainingBinding> bindings = condition switch
        {
            "missing" => [first],
            "extra" => [first, second, Training("train-c", "train-hash-c", [C], [C])],
            "reordered" => [second, first],
            _ => [first, Training("train-a", "train-hash-b", [B], [B])]
        };

        AssertEvidenceInvalid(DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(
            Request(paired, trainingBindings: bindings)));
    }

    [Theory]
    [InlineData("train-train")]
    [InlineData("train-eval")]
    public void Produce_GlobalSourceHashMappingRejectsPartitionCollisions(string condition)
    {
        DynamicGraphMeasurementSplitProof split;
        IReadOnlyList<DynamicGraphEdgeAuthorityNoveltyTrainingBinding> bindings;
        if (condition == "train-train")
        {
            split = Proof(
                trainRoots: ["train-a", "train-b"],
                hashes: ["collision", "eval-hash"]);
            bindings =
            [
                Training("train-a", "collision", [A], [A]),
                Training("train-b", "collision", [B], [B])
            ];
        }
        else
        {
            split = Proof(hashes: ["eval-hash"]);
            bindings = [Training("train-a", "eval-hash", [A], [A])];
        }

        AssertEvidenceInvalid(DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(
            Request(Paired(split: split), trainingBindings: bindings)));
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("extra")]
    public void Produce_FlatSourceHashSetMustExactlyEqualDerivedAtomicMappings(string condition)
    {
        var hashes = condition == "missing"
            ? new[] { "eval-hash" }
            : new[] { "train-hash", "eval-hash", "orphan-hash" };
        var paired = Paired(split: Proof(hashes: hashes));

        AssertEvidenceInvalid(DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(
            Request(paired)));
    }

    [Fact]
    public void Produce_OpaqueNonShaSourceHashesAreAcceptedWithoutFingerprintValidation()
    {
        var split = Proof(hashes: ["opaque-training-source", "opaque-eval-source"]);
        var paired = Paired(split: split, evalHash: "opaque-eval-source");
        var training = new[]
        {
            Training("train-a", "opaque-training-source", [A], [A])
        };

        var result = DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(
            Request(paired, trainingBindings: training));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            "opaque-training-source",
            Assert.Single(result.Value!.TrainingEvidence).CallerAttestedSourceHash);
        Assert.Equal("opaque-eval-source", result.Value.Rows[0].CallerAttestedSourceHash);
    }

    [Theory]
    [InlineData("uppercase")]
    [InlineData("short")]
    [InlineData("nonhex")]
    public void Produce_RejectsEveryNonLowerHex64Fingerprint(string condition)
    {
        var invalid = condition switch
        {
            "uppercase" => UpperA,
            "short" => "abc",
            _ => NonHex
        };
        var training = new[] { Training("train-a", "train-hash", [invalid], [A]) };
        var row = new[]
        {
            new DynamicGraphEdgeAuthorityNoveltyRowInput("row-000", B, [B]),
            new DynamicGraphEdgeAuthorityNoveltyRowInput("row-001", C, [C])
        };

        AssertEvidenceInvalid(DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(
            Request(Paired(), trainingBindings: training)));
        AssertEvidenceInvalid(DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(
            Request(
                Paired(),
                noveltyRows:
                [
                    row[0] with { CallerAttestedPatternFingerprint = invalid },
                    row[1]
                ])));
        AssertEvidenceInvalid(DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(
            Request(
                Paired(),
                noveltyRows:
                [
                    row[0] with { CallerAttestedTokenFingerprints = [invalid] },
                    row[1]
                ])));
    }

    [Theory]
    [InlineData("training-pattern-empty")]
    [InlineData("training-token-empty")]
    [InlineData("novelty-token-empty")]
    [InlineData("training-duplicate")]
    [InlineData("training-reordered")]
    [InlineData("novelty-duplicate")]
    [InlineData("novelty-reordered")]
    public void Produce_RejectsEmptyDuplicateOrNonOrdinalFingerprintLists(string condition)
    {
        var trainingPatterns = condition switch
        {
            "training-pattern-empty" => Array.Empty<string>(),
            "training-duplicate" => [A, A],
            "training-reordered" => [B, A],
            _ => new[] { A }
        };
        var trainingTokens = condition == "training-token-empty"
            ? Array.Empty<string>()
            : new[] { A };
        IReadOnlyList<string> firstNoveltyTokens = condition switch
        {
            "novelty-token-empty" => [],
            "novelty-duplicate" => [B, B],
            "novelty-reordered" => [C, B],
            _ => [B]
        };

        AssertEvidenceInvalid(DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(
            Request(
                Paired(),
                trainingBindings:
                [Training("train-a", "train-hash", trainingPatterns, trainingTokens)],
                noveltyRows:
                [
                    new("row-000", B, firstNoveltyTokens),
                    new("row-001", C, [C])
                ])));
    }

    [Theory]
    [InlineData("wrong-id")]
    [InlineData("reordered")]
    [InlineData("missing")]
    [InlineData("extra")]
    public void Produce_NoveltyRowsMustExactlyMatchPairedRowsOneForOneInOrder(string condition)
    {
        var first = new DynamicGraphEdgeAuthorityNoveltyRowInput("row-000", B, [B]);
        var second = new DynamicGraphEdgeAuthorityNoveltyRowInput("row-001", C, [C]);
        IReadOnlyList<DynamicGraphEdgeAuthorityNoveltyRowInput> rows = condition switch
        {
            "wrong-id" => [first with { RowId = "other-row" }, second],
            "reordered" => [second, first],
            "missing" => [first],
            _ => [first, second, new("row-002", D, [D])]
        };

        AssertEvidenceInvalid(DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(
            Request(Paired(), noveltyRows: rows)));
    }

    [Theory]
    [InlineData("invalid", "INVALID_SPLIT_PROOF", "Dynamic graph measurement split proof is invalid.")]
    [InlineData("overlap", "TRAIN_HELDOUT_OVERLAP", "Training and held-out source roots overlap.")]
    [InlineData("near", "TRAIN_HELDOUT_NEAR_DUPLICATE", "Training and held-out sources contain near-duplicates.")]
    public void Produce_SplitFailurePrecedesEveryHostileNoveltyCollection(
        string condition,
        string expectedCode,
        string expectedMessage)
    {
        var split = condition switch
        {
            "overlap" => Proof(
                trainRoots: ["shared"],
                evalRoots: ["shared"],
                hashes: ["train-hash", "eval-hash"]),
            "near" => Proof(nearDuplicateCount: 1),
            _ => Proof(method: "SimHash")
        };
        var exception = new InvalidOperationException("must not enumerate novelty evidence");
        var request = new DynamicGraphEdgeAuthorityNoveltyEvaluationRequest(
            Paired(split: split, evalRoot: condition == "overlap" ? "shared" : "eval-a"),
            new FaultingReadOnlyList<DynamicGraphEdgeAuthorityNoveltyTrainingBinding>(exception),
            new FaultingReadOnlyList<string>(exception),
            new FaultingReadOnlyList<DynamicGraphEdgeAuthorityNoveltyRowInput>(exception),
            " bad-normalization ",
            " bad-tokenization ");

        AssertFailure(
            DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(request),
            expectedCode,
            expectedMessage);
    }

    [Fact]
    public void Produce_InvalidPairedCarrierReturnsExactReusedFailureBeforeEvidenceTraversal()
    {
        var paired = Paired();
        var scorerField = typeof(DynamicGraphEdgeAuthorityAblationEvaluationResult)
            .GetField("_scorerId", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(scorerField);
        scorerField!.SetValue(paired, "wrong-scorer");
        var exception = new InvalidOperationException("must not enumerate novelty evidence");
        var request = new DynamicGraphEdgeAuthorityNoveltyEvaluationRequest(
            paired,
            new FaultingReadOnlyList<DynamicGraphEdgeAuthorityNoveltyTrainingBinding>(exception),
            new FaultingReadOnlyList<string>(exception),
            new FaultingReadOnlyList<DynamicGraphEdgeAuthorityNoveltyRowInput>(exception),
            "fingerprint-normalization-v1",
            "tokenization-v1");

        AssertFailure(
            DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(request),
            PairedInvalidCode,
            PairedInvalidMessage);
    }

    [Fact]
    public void Produce_OrdinaryAndNestedOrdinaryEvidenceFailuresNormalizeLocally()
    {
        foreach (var exception in new Exception[]
                 {
                     new InvalidOperationException("ordinary"),
                     new AggregateException(
                         "nested ordinary",
                         new InvalidOperationException("inner"))
                 })
        {
            var request = Request(
                Paired(),
                trainingBindings:
                    new FaultingReadOnlyList<DynamicGraphEdgeAuthorityNoveltyTrainingBinding>(
                        exception));

            AssertEvidenceInvalid(
                DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(request));
        }
    }

    [Theory]
    [InlineData("cancel")]
    [InlineData("fatal")]
    [InlineData("nested-cancel")]
    [InlineData("nested-fatal")]
    public void Produce_CancellationFatalAndNestedFatalEvidenceFailuresEscapeUnchanged(
        string condition)
    {
        Exception exception = condition switch
        {
            "cancel" => new OperationCanceledException("cancel"),
            "fatal" => new OutOfMemoryException("fatal"),
            "nested-cancel" => new AggregateException(
                "nested cancel",
                new OperationCanceledException("inner cancel")),
            _ => new AggregateException(
                "nested fatal",
                new InvalidOperationException("ordinary"),
                new OutOfMemoryException("inner fatal"))
        };
        var request = Request(
            Paired(),
            noveltyRows: new FaultingReadOnlyList<DynamicGraphEdgeAuthorityNoveltyRowInput>(
                exception));

        var thrown = Assert.ThrowsAny<Exception>(() =>
            DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(request));

        Assert.Same(exception, thrown);
    }

    [Theory]
    [InlineData(null, "tokenization-v1")]
    [InlineData(" normalization ", "tokenization-v1")]
    [InlineData("fingerprint-normalization-v1", "")]
    [InlineData("fingerprint-normalization-v1", "tokenization\ninvalid")]
    public void Produce_RequestWideSchemeIdsMustBeCanonical(
        string? normalization,
        string? tokenization)
        => AssertEvidenceInvalid(DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(
            new DynamicGraphEdgeAuthorityNoveltyEvaluationRequest(
                Paired(),
                DefaultTraining(),
                ["row-000", "row-001"],
                DefaultNoveltyRows(),
                normalization,
                tokenization)));

    private static DynamicGraphEdgeAuthorityNoveltyEvaluationResult Successful(
        DynamicGraphEdgeAuthorityNoveltyEvaluationRequest request)
    {
        var result = DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(request);
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return result.Value!;
    }

    private static DynamicGraphEdgeAuthorityNoveltyEvaluationRequest Request(
        DynamicGraphEdgeAuthorityAblationEvaluationResult paired,
        IReadOnlyList<DynamicGraphEdgeAuthorityNoveltyTrainingBinding>? trainingBindings = null,
        IReadOnlyList<string>? requestedRowIds = null,
        IReadOnlyList<DynamicGraphEdgeAuthorityNoveltyRowInput>? noveltyRows = null)
        => new(
            paired,
            trainingBindings ?? DefaultTraining(),
            requestedRowIds ?? paired.Rows.Select(row => row.RowId).ToArray(),
            noveltyRows ?? DefaultNoveltyRows(paired.Rows.Count),
            "fingerprint-normalization-v1",
            "tokenization-v1");

    private static DynamicGraphEdgeAuthorityNoveltyTrainingBinding[] DefaultTraining()
        => [Training("train-a", "train-hash", [A], [A])];

    private static DynamicGraphEdgeAuthorityNoveltyRowInput[] DefaultNoveltyRows(int count = 2)
        => Enumerable.Range(0, count)
            .Select(index => new DynamicGraphEdgeAuthorityNoveltyRowInput(
                $"row-{index:D3}",
                index % 2 == 0 ? B : C,
                [index % 2 == 0 ? B : C]))
            .ToArray();

    private static DynamicGraphEdgeAuthorityNoveltyTrainingBinding Training(
        string root,
        string hash,
        IReadOnlyList<string> patterns,
        IReadOnlyList<string> tokens)
        => new(root, hash, patterns, tokens);

    private static DynamicGraphEdgeAuthorityAblationEvaluationResult Paired(
        int rowCount = 2,
        int learnedCorrect = 2,
        int baselineCorrect = 1,
        DynamicGraphMeasurementSplitProof? split = null,
        string evalRoot = "eval-a",
        string evalHash = "eval-hash")
    {
        var proof = split ?? Proof();
        var rows = Enumerable.Range(0, rowCount)
            .Select(index =>
            {
                var learnedMatches = index < learnedCorrect;
                var baselineMatches = index < baselineCorrect;
                return new DynamicGraphEdgeAuthorityAblationRowEvidence(
                    $"row-{index:D3}",
                    evalRoot,
                    evalHash,
                    "document-000",
                    "repo://eval/a",
                    $"label-{index:D3}",
                    "target-a",
                    learnedMatches ? "target-a" : "target-b",
                    baselineMatches ? "target-a" : "target-b",
                    learnedMatches,
                    baselineMatches);
            })
            .ToArray();
        var learnedAccuracy = (double)learnedCorrect / rowCount;
        var baselineAccuracy = (double)baselineCorrect / rowCount;
        var rawDrop = learnedAccuracy - baselineAccuracy;
        var normalizedDrop = Math.Round(rawDrop, 12, MidpointRounding.AwayFromZero);
        var constructor = Assert.Single(
            typeof(DynamicGraphEdgeAuthorityAblationEvaluationResult)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance),
            candidate => candidate.GetParameters().Length == 12);
        return Assert.IsType<DynamicGraphEdgeAuthorityAblationEvaluationResult>(
            constructor.Invoke(
            [
                "edge-novelty-evaluation",
                "edge-a",
                "authority-a",
                proof,
                rows,
                learnedCorrect,
                learnedAccuracy,
                baselineCorrect,
                baselineAccuracy,
                rawDrop,
                normalizedDrop,
                normalizedDrop >= DynamicGraphAblationMeter.MinimumAccuracyDrop
            ]));
    }

    private static DynamicGraphMeasurementSplitProof Proof(
        IReadOnlyList<string>? trainRoots = null,
        IReadOnlyList<string>? evalRoots = null,
        IReadOnlyList<string>? hashes = null,
        int nearDuplicateCount = 0,
        string method = "simhash")
        => new(
            trainRoots ?? ["train-a"],
            evalRoots ?? ["eval-a"],
            hashes ?? ["train-hash", "eval-hash"],
            nearDuplicateCount,
            method,
            0.85d);

    private static void AssertEvidenceInvalid(
        OperationResult<DynamicGraphEdgeAuthorityNoveltyEvaluationResult> result)
        => AssertFailure(result, EvidenceInvalidCode, EvidenceInvalidMessage);

    private static void AssertFailure(
        OperationResult<DynamicGraphEdgeAuthorityNoveltyEvaluationResult> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    private static void AssertMeasurementsEqual(
        DynamicGraphNoveltyMeasurement expected,
        DynamicGraphNoveltyMeasurement actual)
    {
        Assert.Equal(expected.EvaluationId, actual.EvaluationId);
        Assert.Equal(expected.RequestedRows, actual.RequestedRows);
        Assert.Equal(expected.RowsEvaluated, actual.RowsEvaluated);
        Assert.Equal(expected.SelectionShortfall, actual.SelectionShortfall);
        Assert.Equal(expected.NewPatternRate, actual.NewPatternRate);
        Assert.Equal(expected.SourceTokenSeenRate, actual.SourceTokenSeenRate);
        Assert.Equal(expected.MaxAccuracy, actual.MaxAccuracy);
        Assert.Equal(expected.MinimumNewPatternRate, actual.MinimumNewPatternRate);
        Assert.Equal(expected.MaximumSeenTokenRate, actual.MaximumSeenTokenRate);
        Assert.Equal(expected.MeetsNoveltyGate, actual.MeetsNoveltyGate);
        Assert.Equal(expected.SplitProof.TrainSourceRoots, actual.SplitProof.TrainSourceRoots);
        Assert.Equal(expected.SplitProof.EvalSourceRoots, actual.SplitProof.EvalSourceRoots);
        Assert.Equal(expected.SplitProof.SourceHashes, actual.SplitProof.SourceHashes);
    }

    private static void AssertReadOnly<T>(IReadOnlyList<T> values)
        => Assert.Throws<NotSupportedException>(() => ((IList<T>)values).Add(default!));

    private sealed class FaultingReadOnlyList<T>(Exception exception) : IReadOnlyList<T>
    {
        public int Count => 1;

        public T this[int index] => throw exception;

        public IEnumerator<T> GetEnumerator() => throw exception;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
