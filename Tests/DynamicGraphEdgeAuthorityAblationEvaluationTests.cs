// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Collections.ObjectModel;
using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

public sealed class DynamicGraphEdgeAuthorityAblationEvaluationTests
{
    private const string EdgeId = "edge-evaluation";
    private const string AuthorityUnitId = "edge-evaluation-authority";
    private const string DomainTag = "edge-evaluation-domain";
    private const string RequestInvalidCode =
        "EDGE_AUTHORITY_ABLATION_EVALUATION_REQUEST_INVALID";
    private const string ProbeResultInvalidCode =
        "EDGE_AUTHORITY_ABLATION_EVALUATION_PROBE_RESULT_INVALID";
    private const string ResultInvalidCode =
        "EDGE_AUTHORITY_ABLATION_EVALUATION_RESULT_INVALID";
    private const string ResultInvalidMessage =
        "Dynamic graph edge authority ablation evaluation result is invalid.";
    private static readonly string[] Targets = ["target-a", "target-b", "target-c"];
    private static readonly string[] LeakageSignals =
        DynamicGraphEdgeAuthorityLeakageSignalVocabulary.CreateSnapshot();
    private static readonly string[] IdentityProbeIds =
        ["source-id-control-01", "source-path-control-02", "correlation-id-control-03"];
    private static readonly string[] IdentitySignals =
        ["source_id", "source_path", "correlation_id"];
    private static readonly string[] IdentityValues =
        [
            "edge-authority-source-id-control",
            "identity-control://edge-authority-source-path",
            "edge-authority-correlation-id-control"
        ];

    [Fact]
    public void PublicSurface_UsesOnlyProbeInterfaceAndFixedResultMetadata()
    {
        Assert.Contains(
            typeof(IDynamicGraphEdgeAuthorityAblationProbe),
            typeof(DynamicGraphEdgeAuthorityAblationProbe).GetInterfaces());
        var interfaceMethod = Assert.Single(
            typeof(IDynamicGraphEdgeAuthorityAblationProbe).GetMethods());
        var concreteMethod = Assert.Single(typeof(DynamicGraphEdgeAuthorityAblationProbe)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Equal(nameof(IDynamicGraphEdgeAuthorityAblationProbe.RunAsync), interfaceMethod.Name);
        Assert.Equal(concreteMethod.ReturnType, interfaceMethod.ReturnType);
        Assert.Equal(
            concreteMethod.GetParameters().Select(parameter => parameter.ParameterType),
            interfaceMethod.GetParameters().Select(parameter => parameter.ParameterType));

        var runnerConstructor = Assert.Single(
            typeof(DynamicGraphEdgeAuthorityAblationEvaluationRunner).GetConstructors());
        Assert.Equal(
            [typeof(IDynamicGraphEdgeAuthorityAblationProbe)],
            runnerConstructor.GetParameters().Select(parameter => parameter.ParameterType));
        var fields = typeof(DynamicGraphEdgeAuthorityAblationEvaluationRunner)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Equal(
            [typeof(IDynamicGraphEdgeAuthorityAblationProbe)],
            fields.Select(field => field.FieldType));
        Assert.DoesNotContain(fields, field =>
            field.FieldType.Name.Contains("Scorer", StringComparison.Ordinal)
            || field.FieldType.Name.Contains("Resolver", StringComparison.Ordinal)
            || field.FieldType.Name.Contains("LogicalUnit", StringComparison.Ordinal));

        Assert.Empty(
            typeof(DynamicGraphEdgeAuthorityAblationEvaluationResult).GetConstructors());
        Assert.Equal(
            "edge-authority-paired-top1-exact-match",
            DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedScorerId);
        Assert.Equal(
            "1.0.0",
            DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedScorerVersion);
        Assert.Equal(
            "dynamic-graph-q11-selection-drop-v1",
            DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedThresholdSetId);
        Assert.Equal(
            "round-12-away-from-zero",
            DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedNormalizationId);
    }

    [Fact]
    public async Task ProducerSuccess_PassesPublicOwnedCarrierImporter()
    {
        var produced = await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(
            ProbeFromPredictions(["target-a", "target-b"], ["target-b", "target-a"]))
            .RunAsync(CreateRequest(2, ["target-a", "target-b"]));

        Assert.True(produced.IsSuccess, $"{produced.ErrorCode}: {produced.ErrorMessage}");
        var imported = DynamicGraphEdgeAuthorityAblationEvaluationRunner.ImportValidated(
            ImportFrom(produced.Value!));
        Assert.True(imported.IsSuccess, $"{imported.ErrorCode}: {imported.ErrorMessage}");
        Assert.NotSame(produced.Value, imported.Value);
        Assert.NotSame(produced.Value!.Rows, imported.Value!.Rows);
        Assert.NotSame(produced.Value.SplitProof, imported.Value.SplitProof);
        Assert.Equal(produced.Value.LearnedCorrectCount, imported.Value.LearnedCorrectCount);
        Assert.Equal(produced.Value.BaselineCorrectCount, imported.Value.BaselineCorrectCount);
    }

    [Fact]
    public async Task RunAsync_MissingAdmittedEvalRootCoverageFailsBeforeProbe()
    {
        var traversal = CreateTraversal(0);
        var request = CreateRequest(
            CreateEdge(),
            CreateRows([traversal], ["target-a"]),
            SplitForRows(2),
            CreateSourceBindings(2));
        var probe = ProbeFromPredictions(["target-a"], ["target-b"]);

        var result = await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(probe)
            .RunAsync(request);

        AssertFailure(
            result,
            RequestInvalidCode,
            "Edge authority ablation evaluation request is invalid.");
        Assert.Equal(0, probe.CallCount);
    }

    [Fact]
    public async Task RunAsync_MultipleRowsForSameRootAndExactFullBindingTupleSucceed()
    {
        var rows = CreateRepeatedRootRows();
        var request = CreateRequest(
            CreateEdge(),
            rows,
            Split(eval: ["eval-root-000"], hashes: ["eval-hash-000"]),
            [CreateSourceBindings(1)[0]]);
        var probe = ProbeFromPredictions(
            ["target-a", "target-b"],
            ["target-b", "target-a"]);

        var result = await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(probe)
            .RunAsync(request);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(2, probe.CallCount);
        Assert.Equal(["eval-root-000", "eval-root-000"],
            result.Value!.Rows.Select(row => row.EvalSourceRoot));
        Assert.Equal(["eval-hash-000", "eval-hash-000"],
            result.Value.Rows.Select(row => row.CallerAttestedSourceHash));
    }

    [Theory]
    [InlineData("hash")]
    [InlineData("document")]
    [InlineData("path")]
    public async Task RunAsync_RepeatedRootWithChangedBindingTupleFailsBeforeProbe(string field)
    {
        var rows = CreateRepeatedRootRows();
        rows[1] = field switch
        {
            "hash" => CopyRow(rows[1], hash: "eval-hash-001"),
            "document" => CopyRow(
                rows[1],
                traversal: CreateTraversal(
                    1,
                    documentId: "document-001",
                    sourcePath: "repo://eval/000")),
            _ => CopyRow(
                rows[1],
                traversal: CreateTraversal(
                    1,
                    documentId: "document-000",
                    sourcePath: "repo://eval/001"))
        };
        var request = CreateRequest(
            CreateEdge(),
            rows,
            Split(
                eval: ["eval-root-000"],
                hashes: ["eval-hash-000", "eval-hash-001"]),
            [CreateSourceBindings(1)[0]]);
        var probe = ProbeFromPredictions(["target-a", "target-b"], ["target-b", "target-a"]);

        var result = await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(probe)
            .RunAsync(request);

        AssertFailure(
            result,
            RequestInvalidCode,
            "Edge authority ablation evaluation request is invalid.");
        Assert.Equal(0, probe.CallCount);
    }

    [Fact]
    public async Task RunAsync_ProcessesEveryRowSequentiallyInSuppliedOrdinalOrderAndScoresPairedTops()
    {
        var request = CreateRequest(3, ["target-a", "target-b", "target-c"]);
        var learned = new[] { "target-a", "target-b", "target-a" };
        var baseline = new[] { "target-b", "target-a", "target-c" };
        var probe = new DelegateProbe
        {
            Handler = (edge, input, _, flags, _, index) =>
            {
                Assert.Equal(EdgeId, edge!.EdgeId, StringComparer.Ordinal);
                Assert.True(flags!.UseLearnedUnitWhenAvailable);
                return Task.FromResult(OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>.Ok(
                    CreateProbeResult(input!, learned[index], baseline[index])));
            }
        };

        var result = await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(probe)
            .RunAsync(request);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var evidence = result.Value!;
        Assert.Equal(3, probe.CallCount);
        Assert.Equal(1, probe.MaximumConcurrentCalls);
        Assert.Equal(["run-000", "run-001", "run-002"], probe.RunIds);
        Assert.Equal(2, evidence.LearnedCorrectCount);
        Assert.Equal(2d / 3d, evidence.LearnedSelectionAccuracy);
        Assert.Equal(1, evidence.BaselineCorrectCount);
        Assert.Equal(1d / 3d, evidence.BaselineSelectionAccuracy);
        Assert.Equal(1d / 3d, evidence.AccuracyDrop);
        Assert.True(evidence.MeetsQ11NumericalGate);
        Assert.Equal(
            [("row-000", "target-a", "target-a", "target-b", true, false),
             ("row-001", "target-b", "target-b", "target-a", true, false),
             ("row-002", "target-c", "target-a", "target-c", false, true)],
            evidence.Rows.Select(row => (
                row.RowId,
                row.ExpectedTopTargetId,
                row.LearnedTopTargetId,
                row.BaselineTopTargetId,
                row.LearnedTopMatchesExpected,
                row.BaselineTopMatchesExpected)));
        Assert.All(evidence.Rows, row =>
        {
            Assert.StartsWith("eval-root-", row.EvalSourceRoot, StringComparison.Ordinal);
            Assert.StartsWith(
                "eval-hash-",
                row.CallerAttestedSourceHash,
                StringComparison.Ordinal);
            Assert.StartsWith(
                "label-proof-",
                row.CallerAttestedLabelProvenanceId,
                StringComparison.Ordinal);
            Assert.StartsWith("document-", row.TraversalDocumentId, StringComparison.Ordinal);
            Assert.StartsWith("repo://eval/", row.TraversalSourcePath, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task RunAsync_LearnedAndBaselineAccuracyAreIndependentAndNeverSwapped()
    {
        var request = CreateRequest(3, ["target-a", "target-b", "target-c"]);
        var probe = ProbeFromPredictions(
            ["target-a", "target-b", "target-c"],
            ["target-b", "target-c", "target-a"]);

        var result = await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(probe)
            .RunAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.LearnedCorrectCount);
        Assert.Equal(0, result.Value.BaselineCorrectCount);
        Assert.Equal(1d, result.Value.LearnedSelectionAccuracy);
        Assert.Equal(0d, result.Value.BaselineSelectionAccuracy);
        Assert.All(result.Value.Rows, row =>
        {
            Assert.True(row.LearnedTopMatchesExpected);
            Assert.False(row.BaselineTopMatchesExpected);
            Assert.NotEqual(row.LearnedTopTargetId, row.BaselineTopTargetId);
        });
    }

    [Theory]
    [InlineData(GraphEdgeKind.FanOut)]
    [InlineData(GraphEdgeKind.ManyToMany)]
    [InlineData(GraphEdgeKind.MultiOutput)]
    public async Task RunAsync_AcceptsEveryEdgeKindAdmittedByCanonicalProbe(GraphEdgeKind kind)
    {
        var edge = CreateEdge();
        edge.Kind = kind;
        var traversal = CreateTraversal(0);
        var result = await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(
            ProbeFromPredictions(["target-a"], ["target-b"]))
            .RunAsync(CreateRequest(
                edge,
                CreateRows([traversal], ["target-a"]),
                SplitForRows(1)));

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
    }

    [Fact]
    public async Task RunAsync_MissingLearnedTopIsIncorrectWithoutFabricatingPrediction()
    {
        var result = await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(
            ProbeFromPredictions([null], ["target-a"]))
            .RunAsync(CreateRequest(1, ["target-a"]));

        Assert.True(result.IsSuccess);
        var row = Assert.Single(result.Value!.Rows);
        Assert.Null(row.LearnedTopTargetId);
        Assert.False(row.LearnedTopMatchesExpected);
        Assert.True(row.BaselineTopMatchesExpected);
        Assert.Equal(0, result.Value.LearnedCorrectCount);
        Assert.Equal(1, result.Value.BaselineCorrectCount);
        Assert.Equal(-1d, result.Value.AccuracyDrop);
    }

    [Theory]
    [InlineData(100, false, 0.01d)]
    [InlineData(50, true, 0.02d)]
    public async Task RunAsync_UsesLockedTwelveDecimalAwayFromZeroThreshold(
        int rowCount,
        bool expectedGate,
        double expectedDrop)
    {
        var expected = Enumerable.Repeat("target-a", rowCount).ToArray();
        var learned = Enumerable.Repeat("target-a", rowCount).ToArray();
        var baseline = Enumerable.Repeat("target-a", rowCount).ToArray();
        baseline[^1] = "target-b";

        var result = await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(
            ProbeFromPredictions(learned, baseline))
            .RunAsync(CreateRequest(rowCount, expected));

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedDrop, result.Value!.AccuracyDrop, 12);
        Assert.Equal(expectedGate, result.Value.MeetsQ11NumericalGate);
        Assert.Equal(
            expectedGate,
            Math.Round(result.Value.AccuracyDrop, 12, MidpointRounding.AwayFromZero)
                >= DynamicGraphAblationMeter.MinimumAccuracyDrop);
    }

    [Fact]
    public async Task RunAsync_NormalizesBinaryDriftBeforeApplyingExactQ11Threshold()
    {
        const int rowCount = 100;
        var expected = Enumerable.Repeat("target-a", rowCount).ToArray();
        var learned = Enumerable.Range(0, rowCount)
            .Select(index => index < 95 ? "target-a" : "target-b")
            .ToArray();
        var baseline = Enumerable.Range(0, rowCount)
            .Select(index => index < 93 ? "target-a" : "target-b")
            .ToArray();

        var result = await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(
            ProbeFromPredictions(learned, baseline))
            .RunAsync(CreateRequest(rowCount, expected));

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var evidence = result.Value!;
        Assert.Equal(95, evidence.LearnedCorrectCount);
        Assert.Equal(93, evidence.BaselineCorrectCount);
        Assert.Equal(0.95d, evidence.LearnedSelectionAccuracy);
        Assert.Equal(0.93d, evidence.BaselineSelectionAccuracy);
        var rawDrop = evidence.LearnedSelectionAccuracy - evidence.BaselineSelectionAccuracy;
        Assert.True(rawDrop < 0.02d);
        Assert.Equal(rawDrop, evidence.AccuracyDrop);
        Assert.Equal(0.02d, evidence.NormalizedAccuracyDrop);
        Assert.True(evidence.MeetsQ11NumericalGate);
        Assert.Equal("edge-authority-paired-top1-exact-match", evidence.ScorerId);
        Assert.Equal("1.0.0", evidence.ScorerVersion);
        Assert.Equal("dynamic-graph-q11-selection-drop-v1", evidence.ThresholdSetId);
        Assert.Equal("round-12-away-from-zero", evidence.NormalizationId);
    }

    [Theory]
    [InlineData(InvalidRequestCase.DuplicateRowId)]
    [InlineData(InvalidRequestCase.MissingRowId)]
    [InlineData(InvalidRequestCase.SubstitutedRowId)]
    [InlineData(InvalidRequestCase.UnadmittedRoot)]
    [InlineData(InvalidRequestCase.MissingRoot)]
    [InlineData(InvalidRequestCase.UnadmittedHash)]
    [InlineData(InvalidRequestCase.MissingHash)]
    [InlineData(InvalidRequestCase.SwappedRowHash)]
    [InlineData(InvalidRequestCase.BindingDocumentMismatch)]
    [InlineData(InvalidRequestCase.BindingPathMismatch)]
    [InlineData(InvalidRequestCase.TraversalDocumentMismatch)]
    [InlineData(InvalidRequestCase.TraversalPathMismatch)]
    [InlineData(InvalidRequestCase.TrainingSourceTraversal)]
    [InlineData(InvalidRequestCase.BindingRootSetMismatch)]
    [InlineData(InvalidRequestCase.DuplicateBindingRoot)]
    [InlineData(InvalidRequestCase.DuplicateBindingHash)]
    [InlineData(InvalidRequestCase.MissingBindings)]
    [InlineData(InvalidRequestCase.EmptyBindings)]
    [InlineData(InvalidRequestCase.NullBinding)]
    [InlineData(InvalidRequestCase.MissingLabel)]
    [InlineData(InvalidRequestCase.MissingProvenance)]
    [InlineData(InvalidRequestCase.DuplicateProvenance)]
    [InlineData(InvalidRequestCase.InvalidExpectedTarget)]
    [InlineData(InvalidRequestCase.EdgeMismatch)]
    [InlineData(InvalidRequestCase.TraversalEdgeMismatch)]
    [InlineData(InvalidRequestCase.TraversalDomainMismatch)]
    [InlineData(InvalidRequestCase.NullRow)]
    public async Task RunAsync_InvalidRowOrCorrelationFailsBeforeProbe(InvalidRequestCase invalid)
    {
        var request = CreateInvalidRequest(invalid);
        var probe = ProbeFromPredictions(["target-a", "target-b"], ["target-b", "target-a"]);

        var result = await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(probe)
            .RunAsync(request);

        AssertFailure(
            result,
            RequestInvalidCode,
            "Edge authority ablation evaluation request is invalid.");
        Assert.Equal(0, probe.CallCount);
    }

    public static IEnumerable<object?[]> SplitFailures()
    {
        yield return [null, "INVALID_SPLIT_PROOF"];
        yield return [new DynamicGraphMeasurementSplitProof(
            null!, ["eval-root-000"], ["eval-hash-000"], 0, "simhash", 0.90d), "INVALID_SPLIT_PROOF"];
        yield return [new DynamicGraphMeasurementSplitProof(
            ["train-root"], null!, ["eval-hash-000"], 0, "simhash", 0.90d), "INVALID_SPLIT_PROOF"];
        yield return [new DynamicGraphMeasurementSplitProof(
            ["train-root"], ["eval-root-000"], null!, 0, "simhash", 0.90d), "INVALID_SPLIT_PROOF"];
        yield return [Split(train: [], eval: ["eval-root-000"], hashes: ["eval-hash-000"]), "INVALID_SPLIT_PROOF"];
        yield return [Split(eval: []), "INVALID_SPLIT_PROOF"];
        yield return [Split(hashes: []), "INVALID_SPLIT_PROOF"];
        yield return [Split(train: ["train-root", "train-root"]), "INVALID_SPLIT_PROOF"];
        yield return [Split(train: ["shared"], eval: ["shared"], hashes: ["eval-hash-000"]), "TRAIN_HELDOUT_OVERLAP"];
        yield return [Split(nearDuplicates: 1), "TRAIN_HELDOUT_NEAR_DUPLICATE"];
        yield return [Split(nearDuplicates: -1), "INVALID_SPLIT_PROOF"];
        yield return [Split(method: "cosine"), "INVALID_SPLIT_PROOF"];
        yield return [Split(threshold: double.NaN), "INVALID_SPLIT_PROOF"];
        yield return [Split(threshold: -0.01d), "INVALID_SPLIT_PROOF"];
        yield return [Split(eval: ["eval-root-000", "eval-root-000"]), "INVALID_SPLIT_PROOF"];
        yield return [Split(hashes: ["eval-hash-000", "eval-hash-000"]), "INVALID_SPLIT_PROOF"];
    }

    [Theory]
    [MemberData(nameof(SplitFailures))]
    public async Task RunAsync_PreservesEverySplitGuardFailureBeforeProbe(
        DynamicGraphMeasurementSplitProof? split,
        string expectedCode)
    {
        var traversal = CreateTraversal(0);
        var request = CreateRequest(
            CreateEdge(),
            CreateRows([traversal], ["target-a"]),
            split);
        var probe = ProbeFromPredictions(["target-a"], ["target-b"]);

        var result = await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(probe)
            .RunAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.ErrorCode, StringComparer.Ordinal);
        Assert.Equal(0, probe.CallCount);
    }

    [Theory]
    [InlineData(MalformedProbeResult.NullOperation)]
    [InlineData(MalformedProbeResult.NullSuccess)]
    [InlineData(MalformedProbeResult.EdgeMismatch)]
    [InlineData(MalformedProbeResult.AuthorityMismatch)]
    [InlineData(MalformedProbeResult.InputMismatch)]
    [InlineData(MalformedProbeResult.BaselineEmpty)]
    [InlineData(MalformedProbeResult.BaselineOrderMismatch)]
    [InlineData(MalformedProbeResult.RowInputMismatch)]
    [InlineData(MalformedProbeResult.ProposedCandidateMismatch)]
    [InlineData(MalformedProbeResult.LearnedTargetOutsideCandidates)]
    [InlineData(MalformedProbeResult.AuthorityEvidenceLeakageMismatch)]
    [InlineData(MalformedProbeResult.OrderProbeCountMismatch)]
    [InlineData(MalformedProbeResult.OrderProbePermutationMismatch)]
    [InlineData(MalformedProbeResult.OrderProbeDecisionMismatch)]
    [InlineData(MalformedProbeResult.IdentityProbeCountMismatch)]
    [InlineData(MalformedProbeResult.IdentityProbeSignalMismatch)]
    [InlineData(MalformedProbeResult.IdentityProbeControlValueMismatch)]
    [InlineData(MalformedProbeResult.IdentityProbeDecisionMismatch)]
    public async Task RunAsync_MalformedSuccessfulProbeResultFailsAtExactSecondRow(
        MalformedProbeResult malformed)
    {
        var request = CreateRequest(3, ["target-a", "target-b", "target-c"]);
        var probe = new DelegateProbe
        {
            Handler = (_, input, _, _, _, index) =>
            {
                if (index == 0)
                {
                    return Task.FromResult(OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>.Ok(
                        CreateProbeResult(input!, "target-a", "target-b")));
                }

                return malformed switch
                {
                    MalformedProbeResult.NullOperation => Task.FromResult<OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>>(null!),
                    MalformedProbeResult.NullSuccess => Task.FromResult(
                        OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>.Ok(null!)),
                    _ => Task.FromResult(
                        OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>.Ok(
                            CreateMalformedProbeResult(input!, malformed)))
                };
            }
        };

        var result = await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(probe)
            .RunAsync(request);

        AssertFailure(
            result,
            ProbeResultInvalidCode,
            "Edge authority ablation probe result is invalid.");
        Assert.Equal(2, probe.CallCount);
        Assert.Equal(["run-000", "run-001"], probe.RunIds);
    }

    [Fact]
    public async Task RunAsync_ProbeFailureIsPreservedAndStopsLaterRows()
    {
        var probe = new DelegateProbe
        {
            Handler = (_, input, _, _, _, index) => index == 0
                ? Task.FromResult(OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>.Ok(
                    CreateProbeResult(input!, "target-a", "target-b")))
                : Task.FromResult(OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>.Fail(
                    "authoritative probe failure",
                    "AUTHORITATIVE_PROBE_FAILURE"))
        };

        var result = await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(probe)
            .RunAsync(CreateRequest(3, ["target-a", "target-b", "target-c"]));

        AssertFailure(result, "AUTHORITATIVE_PROBE_FAILURE", "authoritative probe failure");
        Assert.Equal(2, probe.CallCount);
    }

    [Theory]
    [InlineData(ExceptionShape.Ordinary)]
    [InlineData(ExceptionShape.Fatal)]
    [InlineData(ExceptionShape.NestedFatal)]
    [InlineData(ExceptionShape.UnrequestedCancellation)]
    public async Task RunAsync_MapsOnlyOrdinaryProbeExceptions(ExceptionShape shape)
    {
        Exception exception = shape switch
        {
            ExceptionShape.Ordinary => new InvalidOperationException("ordinary"),
            ExceptionShape.Fatal => new OutOfMemoryException("fatal"),
            ExceptionShape.NestedFatal => new InvalidOperationException(
                "outer",
                new OutOfMemoryException("fatal")),
            _ => new OperationCanceledException("unrequested")
        };
        var probe = new DelegateProbe { Handler = (_, _, _, _, _, _) => throw exception };
        var run = () => new DynamicGraphEdgeAuthorityAblationEvaluationRunner(probe)
            .RunAsync(CreateRequest(1, ["target-a"]));

        if (shape == ExceptionShape.Ordinary)
        {
            AssertFailure(
                await run(),
                "EDGE_AUTHORITY_ABLATION_EVALUATION_PROBE_EXECUTION_FAILED",
                "Edge authority ablation probe execution failed.");
        }
        else
        {
            var thrown = await Assert.ThrowsAnyAsync<Exception>(run);
            Assert.Same(exception, thrown);
        }
    }

    [Fact]
    public async Task RunAsync_CancellationPrecedenceAndPostFirstRowStopAreExact()
    {
        using var enteredCancelled = new CancellationTokenSource();
        enteredCancelled.Cancel();
        var noDependency = new DynamicGraphEdgeAuthorityAblationEvaluationRunner(null);
        AssertFailure(
            await noDependency.RunAsync(null, enteredCancelled.Token),
            "CANCELLED",
            "Cancelled.");
        AssertFailure(
            await noDependency.RunAsync(null),
            "EDGE_AUTHORITY_ABLATION_EVALUATION_PROBE_REQUIRED",
            "Edge authority ablation probe is required.");
        var unusedProbe = ProbeFromPredictions(["target-a"], ["target-b"]);
        AssertFailure(
            await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(unusedProbe)
                .RunAsync(null),
            "EDGE_AUTHORITY_ABLATION_EVALUATION_REQUEST_REQUIRED",
            "Edge authority ablation evaluation request is required.");
        Assert.Equal(0, unusedProbe.CallCount);

        using var cts = new CancellationTokenSource();
        var probe = new DelegateProbe
        {
            Handler = (_, input, _, _, _, _) =>
            {
                cts.Cancel();
                return Task.FromResult(OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>.Ok(
                    CreateProbeResult(input!, "target-a", "target-b")));
            }
        };
        var result = await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(probe)
            .RunAsync(CreateRequest(3, ["target-a", "target-b", "target-c"]), cts.Token);
        AssertFailure(result, "CANCELLED", "Cancelled.");
        Assert.Equal(1, probe.CallCount);
    }

    [Fact]
    public async Task RunAsync_CallerMutationAcrossAwaitCannotChangeFrozenLaterRows()
    {
        var edge = CreateEdge();
        var traversals = Enumerable.Range(0, 2)
            .Select(index => CreateTraversal(index))
            .ToArray();
        var rows = CreateRows(traversals, ["target-a", "target-b"]);
        var bindings = CreateSourceBindings(2);
        var request = CreateRequest(edge, rows, SplitForRows(2), bindings);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var probe = new DelegateProbe
        {
            Handler = async (seenEdge, input, _, _, token, index) =>
            {
                if (index == 0)
                {
                    entered.SetResult();
                    await release.Task.WaitAsync(token);
                }

                return OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>.Ok(
                    CreateProbeResult(input!, index == 0 ? "target-a" : "target-b", "target-c"));
            }
        };

        var run = new DynamicGraphEdgeAuthorityAblationEvaluationRunner(probe).RunAsync(request);
        await entered.Task;
        edge.EdgeId = "mutated-edge";
        traversals[1].Source.CompiledPredicateIds[0] = "MUTATED";
        traversals[1].Context.CompletedSubPhases[0] = "mutated-completed";
        ((List<string>)traversals[1].UpstreamNodeOutput.UpstreamCandidateRefs)[0] = "mutated-ref";
        bindings[1] = new DynamicGraphEdgeAuthorityAblationSourceBinding(
            "mutated-root",
            "mutated-hash",
            "mutated-document",
            "mutated-path");
        release.SetResult();

        var result = await run;
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(2, probe.CallCount);
        Assert.Equal(["run-000", "run-001"], probe.RunIds);
        Assert.Equal(["PREDICATE_A", "PREDICATE_A"], probe.FirstPredicates);
    }

    [Fact]
    public async Task RunAsync_ReturnedEvidenceAndSplitAreDeepOwnedAcrossReads()
    {
        var result = Assert.IsType<DynamicGraphEdgeAuthorityAblationEvaluationResult>(
            (await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(
                ProbeFromPredictions(["target-a", "target-b"], ["target-b", "target-a"]))
                .RunAsync(CreateRequest(2, ["target-a", "target-b"]))).Value);

        var firstRows = result.Rows;
        var firstSplit = result.SplitProof;
        Assert.NotSame(firstRows, result.Rows);
        Assert.NotSame(firstSplit, result.SplitProof);
        Assert.IsType<ReadOnlyCollection<DynamicGraphEdgeAuthorityAblationRowEvidence>>(firstRows);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<DynamicGraphEdgeAuthorityAblationRowEvidence>)firstRows)[0] = firstRows[1]);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<string>)firstSplit.EvalSourceRoots)[0] = "mutated");
        Assert.Equal("row-000", result.Rows[0].RowId, StringComparer.Ordinal);
        Assert.Equal("eval-root-000", result.SplitProof.EvalSourceRoots[0], StringComparer.Ordinal);
    }

    [Fact]
    public void Request_SourceBindingsAndCallerAttestedRowsAreDeepOwnedAcrossReads()
    {
        var bindings = CreateSourceBindings(2);
        var rows = CreateRows([CreateTraversal(0), CreateTraversal(1)], ["target-a", "target-b"]);
        var request = CreateRequest(CreateEdge(), rows, SplitForRows(2), bindings);

        bindings[0] = new DynamicGraphEdgeAuthorityAblationSourceBinding(
            "mutated-root",
            "mutated-hash",
            "mutated-document",
            "mutated-path");
        rows[0] = CopyRow(rows[0], provenance: "mutated-label-attestation");

        var firstBindings = request.SourceBindings!;
        var firstRows = request.Rows!;
        Assert.NotSame(firstBindings, request.SourceBindings);
        Assert.NotSame(firstRows, request.Rows);
        Assert.IsType<ReadOnlyCollection<DynamicGraphEdgeAuthorityAblationSourceBinding>>(
            firstBindings);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<DynamicGraphEdgeAuthorityAblationSourceBinding>)firstBindings)[0] =
                firstBindings[1]);
        Assert.Equal("eval-root-000", request.SourceBindings![0].EvalSourceRoot);
        Assert.Equal(
            "eval-hash-000",
            request.SourceBindings[0].CallerAttestedSourceHash);
        Assert.Equal(
            "label-proof-000",
            request.Rows![0].CallerAttestedLabelProvenanceId);
    }

    [Theory]
    [InlineData("scorer-id", "wrong-scorer")]
    [InlineData("scorer-version", "9.9.9")]
    [InlineData("threshold-set-id", "wrong-threshold")]
    [InlineData("normalization-id", "wrong-normalization")]
    public void PublicImporter_RejectsEveryInconsistentFixedMetadataField(
        string dimension,
        string value)
    {
        var valid = CreateValidImport();
        var invalid = dimension switch
        {
            "scorer-id" => CopyImport(valid, scorerId: value),
            "scorer-version" => CopyImport(valid, scorerVersion: value),
            "threshold-set-id" => CopyImport(valid, thresholdSetId: value),
            _ => CopyImport(valid, normalizationId: value)
        };

        AssertFailure(
            DynamicGraphEdgeAuthorityAblationEvaluationRunner.ImportValidated(invalid),
            ResultInvalidCode,
            ResultInvalidMessage);
    }

    [Theory]
    [MemberData(nameof(CarrierTamperCases))]
    public void PublicImporter_RejectsEveryStaleOrMalformedCarrierField(string dimension)
    {
        var tampered = TamperImport(dimension);

        AssertFailure(
            DynamicGraphEdgeAuthorityAblationEvaluationRunner.ImportValidated(tampered),
            ResultInvalidCode,
            ResultInvalidMessage);
    }

    [Fact]
    public void PublicImporter_AcceptsRepeatedRootOnlyWithOneExactFullTuple()
    {
        var rows = CreateRepeatedRootEvidence();
        var import = CreateImport(
            rows,
            Split(eval: ["eval-root-000"], hashes: ["eval-hash-000"]),
            learnedCorrectCount: 2,
            learnedAccuracy: 1d,
            baselineCorrectCount: 0,
            baselineAccuracy: 0d,
            accuracyDrop: 1d,
            normalizedAccuracyDrop: 1d,
            meetsGate: true);

        var result = DynamicGraphEdgeAuthorityAblationEvaluationRunner.ImportValidated(import);
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var owned = result.Value!;
        Assert.Equal(2, owned.RowsEvaluated);
        Assert.Equal(["eval-root-000", "eval-root-000"],
            owned.Rows.Select(row => row.EvalSourceRoot));
    }

    [Fact]
    public void PublicImporter_DeepOwnsCallerCollectionsBeforeValidationAndAcrossReads()
    {
        var trainRoots = new[] { "train-a" };
        var evalRoots = new[] { "eval-root-000", "eval-root-001" };
        var hashes = new[] { "eval-hash-000", "eval-hash-001" };
        var rows = CreateValidCarrierRows();
        var import = CreateImport(
            rows,
            new DynamicGraphMeasurementSplitProof(
                trainRoots,
                evalRoots,
                hashes,
                0,
                "simhash",
                0.85d),
            learnedCorrectCount: 1,
            learnedAccuracy: 0.5d,
            baselineCorrectCount: 1,
            baselineAccuracy: 0.5d,
            accuracyDrop: 0d,
            normalizedAccuracyDrop: 0d,
            meetsGate: false);

        trainRoots[0] = "mutated-train";
        evalRoots[0] = "mutated-eval";
        hashes[0] = "mutated-hash";
        rows[0] = rows[0] with { RowId = "mutated-row" };

        var result = DynamicGraphEdgeAuthorityAblationEvaluationRunner.ImportValidated(import);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal("row-000", result.Value!.Rows[0].RowId);
        Assert.Equal("eval-root-000", result.Value.SplitProof.EvalSourceRoots[0]);
        Assert.NotSame(import.Rows, result.Value.Rows);
        Assert.NotSame(import.SplitProof, result.Value.SplitProof);
        Assert.NotSame(result.Value.Rows, result.Value.Rows);
        Assert.NotSame(result.Value.SplitProof, result.Value.SplitProof);
    }

    public static TheoryData<string> CarrierTamperCases()
    {
        var cases = new TheoryData<string>();
        foreach (var dimension in new[]
                 {
                     "evaluation-id", "edge-id", "authority-id", "rows-empty", "split-overlap",
                     "row-id-empty", "row-id-duplicate", "label-id-empty", "label-id-duplicate",
                     "root-unadmitted", "hash-unadmitted", "document-empty", "path-empty",
                     "expected-empty", "learned-empty", "baseline-empty", "root-coverage",
                     "same-root-hash-change", "same-root-document-change", "same-root-path-change",
                     "same-hash-different-root", "learned-boolean", "baseline-boolean",
                     "learned-count", "baseline-count", "learned-accuracy", "baseline-accuracy",
                     "raw-drop", "normalized-drop", "gate"
                 })
        {
            cases.Add(dimension);
        }

        return cases;
    }

    private static DelegateProbe ProbeFromPredictions(
        IReadOnlyList<string?> learned,
        IReadOnlyList<string> baseline)
        => new()
        {
            Handler = (_, input, _, _, _, index) => Task.FromResult(
                OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>.Ok(
                    CreateProbeResult(input!, learned[index], baseline[index])))
        };

    private static DynamicGraphEdgeAuthorityAblationEvaluationRequest CreateRequest(
        int rowCount,
        IReadOnlyList<string> expected,
        DynamicGraphMeasurementSplitProof? split = null)
    {
        var traversals = Enumerable.Range(0, rowCount)
            .Select(index => CreateTraversal(index))
            .ToArray();
        return CreateRequest(
            CreateEdge(),
            CreateRows(traversals, expected),
            split ?? SplitForRows(rowCount));
    }

    private static DynamicGraphEdgeAuthorityAblationEvaluationRequest CreateRequest(
        MutableEdge edge,
        IReadOnlyList<DynamicGraphEdgeAuthorityAblationEvaluationRow> rows,
        DynamicGraphMeasurementSplitProof? split)
        => CreateRequest(edge, rows, split, CreateSourceBindings(rows.Count));

    private static DynamicGraphEdgeAuthorityAblationEvaluationRequest CreateRequest(
        MutableEdge edge,
        IReadOnlyList<DynamicGraphEdgeAuthorityAblationEvaluationRow> rows,
        DynamicGraphMeasurementSplitProof? split,
        IReadOnlyList<DynamicGraphEdgeAuthorityAblationSourceBinding>? bindings)
        => new(
            "edge-selection-evaluation",
            edge,
            DomainTag,
            new LogicalExecutionFlags(),
            split,
            bindings,
            rows);

    private static DynamicGraphEdgeAuthorityAblationEvaluationRow[] CreateRows(
        IReadOnlyList<EdgeTraversalInput> traversals,
        IReadOnlyList<string> expected)
        => Enumerable.Range(0, traversals.Count)
            .Select(index => new DynamicGraphEdgeAuthorityAblationEvaluationRow(
                $"row-{index:D3}",
                $"eval-root-{index:D3}",
                $"eval-hash-{index:D3}",
                traversals[index],
                expected[index],
                $"label-proof-{index:D3}"))
            .ToArray();

    private static DynamicGraphEdgeAuthorityAblationSourceBinding[] CreateSourceBindings(
        int rowCount)
        => Enumerable.Range(0, rowCount)
            .Select(index => new DynamicGraphEdgeAuthorityAblationSourceBinding(
                $"eval-root-{index:D3}",
                $"eval-hash-{index:D3}",
                $"document-{index:D3}",
                $"repo://eval/{index:D3}"))
            .ToArray();

    private static DynamicGraphEdgeAuthorityAblationEvaluationRow[] CreateRepeatedRootRows()
        =>
        [
            new DynamicGraphEdgeAuthorityAblationEvaluationRow(
                "row-000",
                "eval-root-000",
                "eval-hash-000",
                CreateTraversal(0),
                "target-a",
                "label-proof-000"),
            new DynamicGraphEdgeAuthorityAblationEvaluationRow(
                "row-001",
                "eval-root-000",
                "eval-hash-000",
                CreateTraversal(
                    1,
                    documentId: "document-000",
                    sourcePath: "repo://eval/000"),
                "target-b",
                "label-proof-001")
        ];

    private static DynamicGraphEdgeAuthorityAblationEvaluationImport CreateValidImport()
        => CreateImport(
            CreateValidCarrierRows(),
            SplitForRows(2),
            learnedCorrectCount: 1,
            learnedAccuracy: 0.5d,
            baselineCorrectCount: 1,
            baselineAccuracy: 0.5d,
            accuracyDrop: 0d,
            normalizedAccuracyDrop: 0d,
            meetsGate: false);

    private static DynamicGraphEdgeAuthorityAblationRowEvidence[] CreateValidCarrierRows()
        =>
        [
            new DynamicGraphEdgeAuthorityAblationRowEvidence(
                "row-000",
                "eval-root-000",
                "eval-hash-000",
                "document-000",
                "repo://eval/000",
                "label-proof-000",
                "target-a",
                "target-a",
                "target-b",
                true,
                false),
            new DynamicGraphEdgeAuthorityAblationRowEvidence(
                "row-001",
                "eval-root-001",
                "eval-hash-001",
                "document-001",
                "repo://eval/001",
                "label-proof-001",
                "target-b",
                "target-a",
                "target-b",
                false,
                true)
        ];

    private static DynamicGraphEdgeAuthorityAblationRowEvidence[] CreateRepeatedRootEvidence()
        =>
        [
            new DynamicGraphEdgeAuthorityAblationRowEvidence(
                "row-000",
                "eval-root-000",
                "eval-hash-000",
                "document-000",
                "repo://eval/000",
                "label-proof-000",
                "target-a",
                "target-a",
                "target-b",
                true,
                false),
            new DynamicGraphEdgeAuthorityAblationRowEvidence(
                "row-001",
                "eval-root-000",
                "eval-hash-000",
                "document-000",
                "repo://eval/000",
                "label-proof-001",
                "target-b",
                "target-b",
                "target-a",
                true,
                false)
        ];

    private static DynamicGraphEdgeAuthorityAblationEvaluationImport CreateImport(
        IReadOnlyList<DynamicGraphEdgeAuthorityAblationRowEvidence> rows,
        DynamicGraphMeasurementSplitProof split,
        int learnedCorrectCount,
        double learnedAccuracy,
        int baselineCorrectCount,
        double baselineAccuracy,
        double accuracyDrop,
        double normalizedAccuracyDrop,
        bool meetsGate,
        string evaluationId = "edge-selection-evaluation",
        string edgeId = EdgeId,
        string authorityUnitId = AuthorityUnitId,
        string scorerId =
            DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedScorerId,
        string scorerVersion =
            DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedScorerVersion,
        string thresholdSetId =
            DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedThresholdSetId,
        string normalizationId =
            DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedNormalizationId)
        => new(
            evaluationId,
            edgeId,
            authorityUnitId,
            scorerId,
            scorerVersion,
            thresholdSetId,
            normalizationId,
            split,
            rows,
            learnedCorrectCount,
            learnedAccuracy,
            baselineCorrectCount,
            baselineAccuracy,
            accuracyDrop,
            normalizedAccuracyDrop,
            meetsGate);

    private static DynamicGraphEdgeAuthorityAblationEvaluationImport ImportFrom(
        DynamicGraphEdgeAuthorityAblationEvaluationResult source)
        => new(
            source.EvaluationId,
            source.EdgeId,
            source.AuthorityUnitId,
            source.ScorerId,
            source.ScorerVersion,
            source.ThresholdSetId,
            source.NormalizationId,
            source.SplitProof,
            source.Rows,
            source.LearnedCorrectCount,
            source.LearnedSelectionAccuracy,
            source.BaselineCorrectCount,
            source.BaselineSelectionAccuracy,
            source.AccuracyDrop,
            source.NormalizedAccuracyDrop,
            source.MeetsQ11NumericalGate);

    private static DynamicGraphEdgeAuthorityAblationEvaluationImport CopyImport(
        DynamicGraphEdgeAuthorityAblationEvaluationImport source,
        IReadOnlyList<DynamicGraphEdgeAuthorityAblationRowEvidence>? rows = null,
        DynamicGraphMeasurementSplitProof? split = null,
        int? learnedCorrectCount = null,
        double? learnedAccuracy = null,
        int? baselineCorrectCount = null,
        double? baselineAccuracy = null,
        double? accuracyDrop = null,
        double? normalizedAccuracyDrop = null,
        bool? meetsGate = null,
        string? evaluationId = null,
        string? edgeId = null,
        string? authorityUnitId = null,
        string? scorerId = null,
        string? scorerVersion = null,
        string? thresholdSetId = null,
        string? normalizationId = null)
        => CreateImport(
            rows ?? source.Rows!,
            split ?? source.SplitProof!,
            learnedCorrectCount ?? source.LearnedCorrectCount,
            learnedAccuracy ?? source.LearnedSelectionAccuracy,
            baselineCorrectCount ?? source.BaselineCorrectCount,
            baselineAccuracy ?? source.BaselineSelectionAccuracy,
            accuracyDrop ?? source.AccuracyDrop,
            normalizedAccuracyDrop ?? source.NormalizedAccuracyDrop,
            meetsGate ?? source.MeetsQ11NumericalGate,
            evaluationId ?? source.EvaluationId!,
            edgeId ?? source.EdgeId!,
            authorityUnitId ?? source.AuthorityUnitId!,
            scorerId ?? source.ScorerId!,
            scorerVersion ?? source.ScorerVersion!,
            thresholdSetId ?? source.ThresholdSetId!,
            normalizationId ?? source.NormalizationId!);

    private static DynamicGraphEdgeAuthorityAblationEvaluationImport TamperImport(
        string dimension)
    {
        var valid = CreateValidImport();
        var rows = valid.Rows!.ToArray();
        switch (dimension)
        {
            case "evaluation-id":
                return CopyImport(valid, evaluationId: " evaluation");
            case "edge-id":
                return CopyImport(valid, edgeId: "edge\ninvalid");
            case "authority-id":
                return CopyImport(valid, authorityUnitId: " ");
            case "rows-empty":
                return CopyImport(valid, rows: []);
            case "split-overlap":
                return CopyImport(
                    valid,
                    split: Split(
                        train: ["shared-root"],
                        eval: ["shared-root"],
                        hashes: ["eval-hash-000"]));
            case "row-id-empty":
                rows[0] = rows[0] with { RowId = "" };
                break;
            case "row-id-duplicate":
                rows[1] = rows[1] with { RowId = rows[0].RowId };
                break;
            case "label-id-empty":
                rows[0] = rows[0] with { CallerAttestedLabelProvenanceId = " " };
                break;
            case "label-id-duplicate":
                rows[1] = rows[1] with
                {
                    CallerAttestedLabelProvenanceId =
                        rows[0].CallerAttestedLabelProvenanceId
                };
                break;
            case "root-unadmitted":
                rows[0] = rows[0] with { EvalSourceRoot = "eval-root-unadmitted" };
                break;
            case "hash-unadmitted":
                rows[0] = rows[0] with
                {
                    CallerAttestedSourceHash = "eval-hash-unadmitted"
                };
                break;
            case "document-empty":
                rows[0] = rows[0] with { TraversalDocumentId = "" };
                break;
            case "path-empty":
                rows[0] = rows[0] with { TraversalSourcePath = " " };
                break;
            case "expected-empty":
                rows[0] = rows[0] with { ExpectedTopTargetId = "" };
                break;
            case "learned-empty":
                rows[0] = rows[0] with { LearnedTopTargetId = " " };
                break;
            case "baseline-empty":
                rows[0] = rows[0] with { BaselineTopTargetId = "" };
                break;
            case "root-coverage":
                return CopyImport(valid, rows: [rows[0]]);
            case "same-root-hash-change":
                return CreateRepeatedTupleTamper("hash");
            case "same-root-document-change":
                return CreateRepeatedTupleTamper("document");
            case "same-root-path-change":
                return CreateRepeatedTupleTamper("path");
            case "same-hash-different-root":
                rows[1] = rows[1] with
                {
                    CallerAttestedSourceHash = rows[0].CallerAttestedSourceHash
                };
                break;
            case "learned-boolean":
                rows[0] = rows[0] with { LearnedTopMatchesExpected = false };
                break;
            case "baseline-boolean":
                rows[0] = rows[0] with { BaselineTopMatchesExpected = true };
                break;
            case "learned-count":
                return CopyImport(valid, learnedCorrectCount: 0);
            case "baseline-count":
                return CopyImport(valid, baselineCorrectCount: 0);
            case "learned-accuracy":
                return CopyImport(valid, learnedAccuracy: 0.25d);
            case "baseline-accuracy":
                return CopyImport(valid, baselineAccuracy: 0.25d);
            case "raw-drop":
                return CopyImport(valid, accuracyDrop: 0.25d);
            case "normalized-drop":
                return CopyImport(valid, normalizedAccuracyDrop: 0.25d);
            case "gate":
                return CopyImport(valid, meetsGate: true);
            default:
                throw new ArgumentOutOfRangeException(nameof(dimension));
        }

        return CopyImport(valid, rows: rows);
    }

    private static DynamicGraphEdgeAuthorityAblationEvaluationImport CreateRepeatedTupleTamper(
        string field)
    {
        var rows = CreateRepeatedRootEvidence();
        rows[1] = field switch
        {
            "hash" => rows[1] with { CallerAttestedSourceHash = "eval-hash-001" },
            "document" => rows[1] with { TraversalDocumentId = "document-001" },
            _ => rows[1] with { TraversalSourcePath = "repo://eval/001" }
        };
        return CreateImport(
            rows,
            Split(
                eval: ["eval-root-000"],
                hashes: ["eval-hash-000", "eval-hash-001"]),
            learnedCorrectCount: 2,
            learnedAccuracy: 1d,
            baselineCorrectCount: 0,
            baselineAccuracy: 0d,
            accuracyDrop: 1d,
            normalizedAccuracyDrop: 1d,
            meetsGate: true);
    }

    private static DynamicGraphEdgeAuthorityAblationEvaluationRequest CreateInvalidRequest(
        InvalidRequestCase invalid)
    {
        var edge = CreateEdge();
        var traversals = new[] { CreateTraversal(0), CreateTraversal(1) };
        var rows = CreateRows(traversals, ["target-a", "target-b"]);
        var bindings = CreateSourceBindings(2);
        IReadOnlyList<DynamicGraphEdgeAuthorityAblationSourceBinding>? requestBindings = bindings;
        var split = SplitForRows(2);
        switch (invalid)
        {
            case InvalidRequestCase.DuplicateRowId:
                rows[1] = CopyRow(rows[1], rowId: rows[0].RowId);
                break;
            case InvalidRequestCase.MissingRowId:
                rows[1] = CopyRow(rows[1], rowId: " ");
                break;
            case InvalidRequestCase.SubstitutedRowId:
                rows[1] = CopyRow(rows[1], rowId: "row-001\nsubstituted");
                break;
            case InvalidRequestCase.UnadmittedRoot:
                rows[1] = CopyRow(rows[1], root: "eval-root-unadmitted");
                break;
            case InvalidRequestCase.MissingRoot:
                rows[1] = CopyRow(rows[1], root: " ");
                break;
            case InvalidRequestCase.UnadmittedHash:
                rows[1] = CopyRow(rows[1], hash: "eval-hash-unadmitted");
                break;
            case InvalidRequestCase.MissingHash:
                rows[1] = CopyRow(rows[1], hash: " ");
                break;
            case InvalidRequestCase.SwappedRowHash:
                rows[0] = CopyRow(
                    rows[0],
                    hash: rows[1].CallerAttestedSourceHash);
                break;
            case InvalidRequestCase.BindingDocumentMismatch:
                bindings[1] = CopyBinding(bindings[1], documentId: "different-document");
                break;
            case InvalidRequestCase.BindingPathMismatch:
                bindings[1] = CopyBinding(bindings[1], sourcePath: "repo://eval/different");
                break;
            case InvalidRequestCase.TraversalDocumentMismatch:
                rows[1] = CopyRow(
                    rows[1],
                    traversal: CreateTraversal(1, documentId: "different-document"));
                break;
            case InvalidRequestCase.TraversalPathMismatch:
                rows[1] = CopyRow(
                    rows[1],
                    traversal: CreateTraversal(1, sourcePath: "repo://eval/different"));
                break;
            case InvalidRequestCase.TrainingSourceTraversal:
                bindings[1] = CopyBinding(bindings[1], sourcePath: "train-root");
                rows[1] = CopyRow(
                    rows[1],
                    traversal: CreateTraversal(1, sourcePath: "train-root"));
                break;
            case InvalidRequestCase.BindingRootSetMismatch:
                bindings[1] = CopyBinding(bindings[1], root: "eval-root-unadmitted");
                break;
            case InvalidRequestCase.DuplicateBindingRoot:
                bindings[1] = CopyBinding(bindings[1], root: bindings[0].EvalSourceRoot);
                break;
            case InvalidRequestCase.DuplicateBindingHash:
                bindings[1] = CopyBinding(
                    bindings[1],
                    hash: bindings[0].CallerAttestedSourceHash);
                break;
            case InvalidRequestCase.MissingBindings:
                requestBindings = null;
                break;
            case InvalidRequestCase.EmptyBindings:
                requestBindings = [];
                break;
            case InvalidRequestCase.NullBinding:
                bindings[1] = null!;
                break;
            case InvalidRequestCase.MissingLabel:
                rows[1] = CopyRow(rows[1], expected: " ");
                break;
            case InvalidRequestCase.MissingProvenance:
                rows[1] = CopyRow(rows[1], provenance: " ");
                break;
            case InvalidRequestCase.DuplicateProvenance:
                rows[1] = CopyRow(
                    rows[1],
                    provenance: rows[0].CallerAttestedLabelProvenanceId);
                break;
            case InvalidRequestCase.InvalidExpectedTarget:
                rows[1] = CopyRow(rows[1], expected: "target-unadmitted");
                break;
            case InvalidRequestCase.EdgeMismatch:
                edge.EdgeId = "different-edge";
                break;
            case InvalidRequestCase.TraversalEdgeMismatch:
                rows[1] = CopyRow(rows[1], traversal: CreateTraversal(1, edgeId: "different-edge"));
                break;
            case InvalidRequestCase.TraversalDomainMismatch:
                rows[1] = CopyRow(rows[1], traversal: CreateTraversal(1, domain: "different-domain"));
                break;
            case InvalidRequestCase.NullRow:
                rows[1] = null!;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(invalid));
        }

        return CreateRequest(edge, rows, split, requestBindings);
    }

    private static DynamicGraphEdgeAuthorityAblationEvaluationRow CopyRow(
        DynamicGraphEdgeAuthorityAblationEvaluationRow source,
        string? rowId = null,
        string? root = null,
        string? hash = null,
        EdgeTraversalInput? traversal = null,
        string? expected = null,
        string? provenance = null)
        => new(
            rowId ?? source.RowId,
            root ?? source.EvalSourceRoot,
            hash ?? source.CallerAttestedSourceHash,
            traversal ?? source.TraversalInput,
            expected ?? source.ExpectedTopTargetId,
            provenance ?? source.CallerAttestedLabelProvenanceId);

    private static DynamicGraphEdgeAuthorityAblationSourceBinding CopyBinding(
        DynamicGraphEdgeAuthorityAblationSourceBinding source,
        string? root = null,
        string? hash = null,
        string? documentId = null,
        string? sourcePath = null)
        => new(
            root ?? source.EvalSourceRoot,
            hash ?? source.CallerAttestedSourceHash,
            documentId ?? source.TraversalDocumentId,
            sourcePath ?? source.TraversalSourcePath);

    private static DynamicGraphMeasurementSplitProof SplitForRows(int rowCount)
        => Split(
            eval: Enumerable.Range(0, rowCount).Select(index => $"eval-root-{index:D3}").ToArray(),
            hashes: Enumerable.Range(0, rowCount).Select(index => $"eval-hash-{index:D3}").ToArray());

    private static DynamicGraphMeasurementSplitProof Split(
        IReadOnlyList<string>? train = null,
        IReadOnlyList<string>? eval = null,
        IReadOnlyList<string>? hashes = null,
        int nearDuplicates = 0,
        string method = "simhash",
        double threshold = 0.90d)
        => new(
            train ?? ["train-root"],
            eval ?? ["eval-root-000"],
            hashes ?? ["eval-hash-000"],
            nearDuplicates,
            method,
            threshold);

    private static MutableEdge CreateEdge()
        => new()
        {
            EdgeId = EdgeId,
            FromNodeId = "source-node",
            ToNodeIds = Array.AsReadOnly(Targets.ToArray()),
            Kind = GraphEdgeKind.ManyToMany,
            ParallelEligible = true,
            AuthorityUnitId = AuthorityUnitId,
            AuthorityUnitLevel = "L2",
            DeterministicFinalAuthority = false,
            BaselineUnitId = "baseline-unit",
            BaselineRole = RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole,
            ExcludedLeakageSignals = Array.AsReadOnly(LeakageSignals.ToArray()),
            SourceProof = "source-proof",
            CutoverProof = "cutover-proof",
            AblationProofRef = "ablation-proof"
        };

    private static EdgeTraversalInput CreateTraversal(
        int index,
        string edgeId = EdgeId,
        string domain = DomainTag,
        string? documentId = null,
        string? sourcePath = null)
        => new(
            $"run-{index:D3}",
            edgeId,
            new SubPhaseRouterSourceDocument(
                documentId ?? $"document-{index:D3}",
                sourcePath ?? $"repo://eval/{index:D3}",
                "internal",
                "AGPL-3.0-only",
                "design",
                $"Evaluation {index:D3}",
                $"evaluation source text {index:D3}",
                ["PREDICATE_A", "PREDICATE_B", "PREDICATE_C"],
                "compiled",
                "none",
                domain),
            new ContextSignals(
                [Domain.CODE_Q],
                [$"completed-{index:D3}"],
                [$"active-{index:D3}"],
                new ReadOnlyDictionary<Domain, double>(
                    new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.75d })),
            new NodeOutputToEdgeInputAdapter(
                "source-node",
                new List<string> { $"candidate-{index:D3}-a", $"candidate-{index:D3}-b" },
                $"upstream-hash-{index:D3}",
                "GraphNodeCandidate.Id -> UpstreamCandidateRefs (order-free set)"));

    private static DynamicGraphEdgeAuthorityAblationProbeResult CreateProbeResult(
        EdgeTraversalInput traversal,
        string? learnedTop,
        string baselineTop,
        string edgeId = EdgeId,
        string authorityUnitId = AuthorityUnitId,
        bool inputMismatch = false,
        bool emptyBaseline = false,
        bool wrongBaselineOrder = false,
        bool rowInputMismatch = false)
    {
        var candidates = CreateCandidates(baselineTop);
        var subjectInput = CreateInput(
            traversal,
            candidates,
            edgeId,
            rowInputMismatch ? "DIFFERENT_PREDICATE" : null);
        var baselineInput = inputMismatch
            ? subjectInput with { SemanticTokens = ["different-semantic"] }
            : CreateInput(traversal, candidates, edgeId);
        var baselineOrder = emptyBaseline
            ? Array.Empty<string>()
            : candidates
                .OrderByDescending(candidate => candidate.BaselineScore)
                .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
                .Select(candidate => candidate.TargetSubPhaseId)
                .ToArray();
        if (wrongBaselineOrder)
            Array.Reverse(baselineOrder);
        var decision = learnedTop is null
            ? new EdgeAuthorityDecision(
                [],
                0,
                true,
                RoutingAuthorityVocabulary.TrainedEdgeUnitRole,
                baselineOrder)
            : new EdgeAuthorityDecision(
                [(learnedTop, 0.90d)],
                1,
                false,
                RoutingAuthorityVocabulary.TrainedEdgeUnitRole,
                baselineOrder);
        var report = new DynamicGraphEdgeCandidateOrderLeakageReport(
            edgeId,
            authorityUnitId,
            true,
            Enumerable.Range(0, 5)
                .Select(index => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                    $"candidate-order-permutation-{index + 1:D2}",
                    CreatePermutation(subjectInput.Candidates, index),
                    CopyDecision(decision)))
                .ToArray(),
            LeakageSignals);
        var identity = new DynamicGraphEdgeIdentityLeakageReport(
            edgeId,
            authorityUnitId,
            true,
            Enumerable.Range(0, 3)
                .Select(index => new DynamicGraphEdgeIdentityControlProbe(
                    IdentityProbeIds[index],
                    IdentitySignals[index],
                    IdentityValues[index],
                    CopyDecision(decision)))
                .ToArray(),
            LeakageSignals);
        var subject = new EdgeAuthorityExecutionResult(
            subjectInput,
            candidates,
            decision,
            new DynamicGraphEdgeAuthorityEvidence(
                edgeId,
                authorityUnitId,
                "L2",
                false,
                "baseline-unit",
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole,
                LeakageSignals,
                "source-proof",
                "cutover-proof",
                "ablation-proof"),
            report,
            identity,
            0.90d,
            ["subject-evidence"],
            []);
        return new DynamicGraphEdgeAuthorityAblationProbeResult(
            subject,
            baselineInput,
            baselineOrder,
            authorityUnitId);
    }

    private static DynamicGraphEdgeAuthorityAblationProbeResult CreateMalformedProbeResult(
        EdgeTraversalInput traversal,
        MalformedProbeResult malformed)
    {
        var basic = malformed switch
        {
            MalformedProbeResult.EdgeMismatch => CreateProbeResult(
                traversal,
                "target-b",
                "target-a",
                edgeId: "different-edge"),
            MalformedProbeResult.AuthorityMismatch => CreateProbeResult(
                traversal,
                "target-b",
                "target-a",
                authorityUnitId: "different-authority"),
            MalformedProbeResult.InputMismatch => CreateProbeResult(
                traversal,
                "target-b",
                "target-a",
                inputMismatch: true),
            MalformedProbeResult.BaselineEmpty => CreateProbeResult(
                traversal,
                "target-b",
                "target-a",
                emptyBaseline: true),
            MalformedProbeResult.BaselineOrderMismatch => CreateProbeResult(
                traversal,
                "target-b",
                "target-a",
                wrongBaselineOrder: true),
            MalformedProbeResult.RowInputMismatch => CreateProbeResult(
                traversal,
                "target-b",
                "target-a",
                rowInputMismatch: true),
            _ => null
        };
        if (basic is not null)
            return basic;

        var valid = CreateProbeResult(traversal, "target-b", "target-a");
        var subject = valid.Subject;
        var corruptedSubject = malformed switch
        {
            MalformedProbeResult.ProposedCandidateMismatch => CopySubject(
                subject,
                proposedCandidates: subject.ProposedCandidates.Take(2).ToArray()),
            MalformedProbeResult.LearnedTargetOutsideCandidates => CopySubject(
                subject,
                decision: new EdgeAuthorityDecision(
                    [("target-outside-candidates", 0.90d)],
                    1,
                    false,
                    subject.Decision.AuthoritySource,
                    subject.Decision.AblationBaselineRank)),
            MalformedProbeResult.AuthorityEvidenceLeakageMismatch => CopySubject(
                subject,
                authorityEvidence: CopyAuthorityEvidence(
                    subject.AuthorityEvidence,
                    excludedLeakageSignals: LeakageSignals.Take(2).ToArray())),
            MalformedProbeResult.OrderProbeCountMismatch => CopySubject(
                subject,
                orderReport: CopyOrderReport(
                    subject.CandidateOrderLeakageReport,
                    probes: subject.CandidateOrderLeakageReport.Probes.Take(4).ToArray())),
            MalformedProbeResult.OrderProbePermutationMismatch => CopySubject(
                subject,
                orderReport: CorruptOrderProbe(
                    subject.CandidateOrderLeakageReport,
                    subject.Decision,
                    candidateOrder: CreatePermutation(subject.NormalizedInput.Candidates, 1))),
            MalformedProbeResult.OrderProbeDecisionMismatch => CopySubject(
                subject,
                orderReport: CorruptOrderProbe(
                    subject.CandidateOrderLeakageReport,
                    DecisionWithFirstConfidence(subject.Decision, 0.89d))),
            MalformedProbeResult.IdentityProbeCountMismatch => CopySubject(
                subject,
                identityReport: CopyIdentityReport(
                    subject.IdentityLeakageReport,
                    probes: subject.IdentityLeakageReport.Probes.Take(2).ToArray())),
            MalformedProbeResult.IdentityProbeSignalMismatch => CopySubject(
                subject,
                identityReport: CorruptIdentityProbe(
                    subject.IdentityLeakageReport,
                    subject.Decision,
                    controlledSignal: "source_path")),
            MalformedProbeResult.IdentityProbeControlValueMismatch => CopySubject(
                subject,
                identityReport: CorruptIdentityProbe(
                    subject.IdentityLeakageReport,
                    subject.Decision,
                    controlValue: "wrong-control-value")),
            MalformedProbeResult.IdentityProbeDecisionMismatch => CopySubject(
                subject,
                identityReport: CorruptIdentityProbe(
                    subject.IdentityLeakageReport,
                    DecisionWithFirstConfidence(subject.Decision, 0.89d))),
            _ => throw new ArgumentOutOfRangeException(nameof(malformed))
        };

        return CopyProbeResult(valid, subject: corruptedSubject);
    }

    private static DynamicGraphEdgeAuthorityAblationProbeResult CopyProbeResult(
        DynamicGraphEdgeAuthorityAblationProbeResult source,
        EdgeAuthorityExecutionResult? subject = null)
        => new(
            subject ?? source.Subject,
            source.BaselineInput,
            source.OrderedBaselineTargetIds,
            source.AuthorityUnitExcludedFromDirectBaselineBranchId);

    private static EdgeAuthorityExecutionResult CopySubject(
        EdgeAuthorityExecutionResult source,
        IReadOnlyList<EdgeCandidate>? proposedCandidates = null,
        EdgeAuthorityDecision? decision = null,
        DynamicGraphEdgeAuthorityEvidence? authorityEvidence = null,
        DynamicGraphEdgeCandidateOrderLeakageReport? orderReport = null,
        DynamicGraphEdgeIdentityLeakageReport? identityReport = null)
        => new(
            source.NormalizedInput,
            proposedCandidates ?? source.ProposedCandidates,
            decision ?? source.Decision,
            authorityEvidence ?? source.AuthorityEvidence,
            orderReport ?? source.CandidateOrderLeakageReport,
            identityReport ?? source.IdentityLeakageReport,
            source.UnitConfidence,
            source.EvidenceIds,
            source.Warnings);

    private static DynamicGraphEdgeAuthorityEvidence CopyAuthorityEvidence(
        DynamicGraphEdgeAuthorityEvidence source,
        IReadOnlyList<string>? excludedLeakageSignals = null)
        => new(
            source.EdgeId,
            source.AuthorityUnitId,
            source.AuthorityUnitLevel,
            source.DeterministicFinalAuthority,
            source.BaselineUnitId,
            source.BaselineRole,
            excludedLeakageSignals ?? source.ExcludedLeakageSignals,
            source.SourceProof,
            source.CutoverProof,
            source.AblationProofRef);

    private static DynamicGraphEdgeCandidateOrderLeakageReport CopyOrderReport(
        DynamicGraphEdgeCandidateOrderLeakageReport source,
        IReadOnlyList<DynamicGraphEdgeCandidateOrderPermutationProbe>? probes = null)
        => new(
            source.EdgeId,
            source.ResolvedAuthorityUnitId,
            source.IsOrderInvariant,
            probes ?? source.Probes,
            source.ExcludedLeakageSignals);

    private static DynamicGraphEdgeIdentityLeakageReport CopyIdentityReport(
        DynamicGraphEdgeIdentityLeakageReport source,
        IReadOnlyList<DynamicGraphEdgeIdentityControlProbe>? probes = null)
        => new(
            source.EdgeId,
            source.ResolvedAuthorityUnitId,
            source.IsIdentityInvariant,
            probes ?? source.Probes,
            source.ExcludedLeakageSignals);

    private static DynamicGraphEdgeCandidateOrderLeakageReport CorruptOrderProbe(
        DynamicGraphEdgeCandidateOrderLeakageReport source,
        EdgeAuthorityDecision decision,
        IReadOnlyList<EdgeCandidate>? candidateOrder = null)
    {
        var probes = source.Probes.ToArray();
        probes[0] = new DynamicGraphEdgeCandidateOrderPermutationProbe(
            probes[0].ProbeId,
            candidateOrder ?? probes[0].CandidateOrder,
            decision);
        return CopyOrderReport(source, probes);
    }

    private static DynamicGraphEdgeIdentityLeakageReport CorruptIdentityProbe(
        DynamicGraphEdgeIdentityLeakageReport source,
        EdgeAuthorityDecision decision,
        string? controlledSignal = null,
        string? controlValue = null)
    {
        var probes = source.Probes.ToArray();
        probes[0] = new DynamicGraphEdgeIdentityControlProbe(
            probes[0].ProbeId,
            controlledSignal ?? probes[0].ControlledSignal,
            controlValue ?? probes[0].ControlValue,
            decision);
        return CopyIdentityReport(source, probes);
    }

    private static EdgeAuthorityDecision DecisionWithFirstConfidence(
        EdgeAuthorityDecision source,
        double confidence)
        => new(
            source.OrderedTargets.Select((target, index) =>
                index == 0
                    ? (target.TargetSubPhaseId, confidence)
                    : target).ToArray(),
            source.FanOutCount,
            source.KnowledgeEdge,
            source.AuthoritySource,
            source.AblationBaselineRank.ToArray());

    private static EdgeAuthorityDecision CopyDecision(EdgeAuthorityDecision source)
        => new(
            source.OrderedTargets.ToArray(),
            source.FanOutCount,
            source.KnowledgeEdge,
            source.AuthoritySource,
            source.AblationBaselineRank.ToArray());

    private static EdgeCandidate[] CreatePermutation(
        IReadOnlyList<EdgeCandidate> candidates,
        int index)
    {
        var source = candidates.Select(CopyCandidate).ToArray();
        var offset = index switch
        {
            0 => 0,
            1 => 1,
            2 => 2,
            3 => -1,
            4 => -2,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
        if (offset < 0)
        {
            var reversed = source.Reverse().ToArray();
            return RotateLeft(reversed, -offset - 1);
        }

        return RotateLeft(source, offset);
    }

    private static EdgeCandidate[] RotateLeft(IReadOnlyList<EdgeCandidate> source, int offset)
        => Enumerable.Range(0, source.Count)
            .Select(index => CopyCandidate(source[(index + offset) % source.Count]))
            .ToArray();

    private static EdgeAuthorityInput CreateInput(
        EdgeTraversalInput traversal,
        IReadOnlyList<EdgeCandidate> candidates,
        string edgeId,
        string? firstPredicate = null)
    {
        var predicates = traversal.Source.CompiledPredicateIds.ToArray();
        if (firstPredicate is not null)
            predicates[0] = firstPredicate;
        return new EdgeAuthorityInput(
            edgeId,
            ["semantic-a", "semantic-b"],
            predicates,
            CopyContext(traversal.Context),
            Array.AsReadOnly(candidates.Select(CopyCandidate).ToArray()),
            LeakageSignals.ToArray());
    }

    private static EdgeCandidate[] CreateCandidates(string baselineTop)
        => Targets.Select(target => new EdgeCandidate(
                target,
                [$"PREDICATE_{target[^1].ToString().ToUpperInvariant()}"],
                StringComparer.Ordinal.Equals(target, baselineTop) ? 0.90d : 0.20d,
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole))
            .ToArray();

    private static ContextSignals CopyContext(ContextSignals source)
        => new(
            source.PriorDomains.ToArray(),
            source.CompletedSubPhases.ToArray(),
            source.ActiveKnowledgeEdges.ToArray(),
            new ReadOnlyDictionary<Domain, double>(
                source.PriorConfidenceMap.ToDictionary(pair => pair.Key, pair => pair.Value)));

    private static EdgeCandidate CopyCandidate(EdgeCandidate source)
        => source with { EvidencePredicates = source.EvidencePredicates.ToArray() };

    private static void AssertFailure(
        OperationResult<DynamicGraphEdgeAuthorityAblationEvaluationResult> result,
        string code,
        string message)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(code, result.ErrorCode, StringComparer.Ordinal);
        Assert.Equal(message, result.ErrorMessage, StringComparer.Ordinal);
    }

    public enum InvalidRequestCase
    {
        DuplicateRowId,
        MissingRowId,
        SubstitutedRowId,
        UnadmittedRoot,
        MissingRoot,
        UnadmittedHash,
        MissingHash,
        SwappedRowHash,
        BindingDocumentMismatch,
        BindingPathMismatch,
        TraversalDocumentMismatch,
        TraversalPathMismatch,
        TrainingSourceTraversal,
        BindingRootSetMismatch,
        DuplicateBindingRoot,
        DuplicateBindingHash,
        MissingBindings,
        EmptyBindings,
        NullBinding,
        MissingLabel,
        MissingProvenance,
        DuplicateProvenance,
        InvalidExpectedTarget,
        EdgeMismatch,
        TraversalEdgeMismatch,
        TraversalDomainMismatch,
        NullRow
    }

    public enum MalformedProbeResult
    {
        NullOperation,
        NullSuccess,
        EdgeMismatch,
        AuthorityMismatch,
        InputMismatch,
        BaselineEmpty,
        BaselineOrderMismatch,
        RowInputMismatch,
        ProposedCandidateMismatch,
        LearnedTargetOutsideCandidates,
        AuthorityEvidenceLeakageMismatch,
        OrderProbeCountMismatch,
        OrderProbePermutationMismatch,
        OrderProbeDecisionMismatch,
        IdentityProbeCountMismatch,
        IdentityProbeSignalMismatch,
        IdentityProbeControlValueMismatch,
        IdentityProbeDecisionMismatch
    }

    public enum ExceptionShape
    {
        Ordinary,
        Fatal,
        NestedFatal,
        UnrequestedCancellation
    }

    private sealed class DelegateProbe : IDynamicGraphEdgeAuthorityAblationProbe
    {
        private int _activeCalls;

        internal Func<
            IDynamicGraphEdge?,
            EdgeTraversalInput?,
            string?,
            LogicalExecutionFlags?,
            CancellationToken,
            int,
            Task<OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>>> Handler
        { get; set; }
            = (_, _, _, _, _, _) => throw new InvalidOperationException("Handler is required.");

        internal int CallCount { get; private set; }

        internal int MaximumConcurrentCalls { get; private set; }

        internal List<string> RunIds { get; } = [];

        internal List<string> FirstPredicates { get; } = [];

        public async Task<OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>> RunAsync(
            IDynamicGraphEdge? edge,
            EdgeTraversalInput? input,
            string? domainTag,
            LogicalExecutionFlags? flags,
            CancellationToken cancellationToken = default)
        {
            var index = CallCount++;
            RunIds.Add(input!.RunId);
            FirstPredicates.Add(input.Source.CompiledPredicateIds[0]);
            var active = Interlocked.Increment(ref _activeCalls);
            MaximumConcurrentCalls = Math.Max(MaximumConcurrentCalls, active);
            try
            {
                return await Handler(edge, input, domainTag, flags, cancellationToken, index);
            }
            finally
            {
                Interlocked.Decrement(ref _activeCalls);
            }
        }
    }

    private sealed class MutableEdge : IDynamicGraphEdge
    {
        public string EdgeId { get; set; } = "";

        public string FromNodeId { get; set; } = "";

        public IReadOnlyList<string> ToNodeIds { get; set; } = [];

        public GraphEdgeKind Kind { get; set; }

        public bool ParallelEligible { get; set; }

        public string AuthorityUnitId { get; set; } = "";

        public string AuthorityUnitLevel { get; set; } = "";

        public bool DeterministicFinalAuthority { get; set; }

        public string? BaselineUnitId { get; set; }

        public string BaselineRole { get; set; } = "";

        public IReadOnlyList<string> ExcludedLeakageSignals { get; set; } = [];

        public string SourceProof { get; set; } = "";

        public string CutoverProof { get; set; } = "";

        public string AblationProofRef { get; set; } = "";
    }
}
