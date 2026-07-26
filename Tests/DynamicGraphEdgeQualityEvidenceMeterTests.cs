// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Collections;
using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

public sealed class DynamicGraphEdgeQualityEvidenceMeterTests
{
    private const string EdgeId = "edge-quality-evidence";
    private const string AuthorityUnitId = "edge-quality-authority-unit";
    private const string EvaluationId = "held-out-edge-quality-evaluation";

    private static readonly string[] LeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];

    [Fact]
    public void Contract_ExposesExactOwnedShapesWithoutDuplicateTopLevelIds()
    {
        AssertOwnedRecordShape<DynamicGraphEdgeQualityEvidenceMeasurement>(
            ("EdgeAuthorityControl", typeof(DynamicGraphEdgeAuthorityControlMeasurement)),
            ("Novelty", typeof(DynamicGraphNoveltyMeasurement)),
            ("Ablation", typeof(DynamicGraphAblationMeasurement)));
        AssertOwnedRecordShape<DynamicGraphTargetEdgeQualityEvidenceMeasurement>(
            ("Target", typeof(DynamicGraphMeasurementTarget)),
            ("EdgeQualityEvidence", typeof(DynamicGraphEdgeQualityEvidenceMeasurement)));

        Assert.Null(typeof(DynamicGraphEdgeQualityEvidenceMeasurement).GetProperty("EdgeId"));
        Assert.Null(typeof(DynamicGraphEdgeQualityEvidenceMeasurement).GetProperty("EvaluationId"));
        Assert.Null(typeof(DynamicGraphTargetEdgeQualityEvidenceMeasurement).GetProperty("EdgeId"));
        Assert.Null(typeof(DynamicGraphTargetEdgeQualityEvidenceMeasurement).GetProperty("EvaluationId"));
    }

    [Fact]
    public void Meter_ExposesOnlyTheExactMeasureMethodWithoutExceptionHandling()
    {
        var method = Assert.Single(typeof(DynamicGraphEdgeQualityEvidenceMeter).GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

        Assert.Equal("Measure", method.Name);
        Assert.Equal(typeof(OperationResult<DynamicGraphEdgeQualityEvidenceMeasurement>), method.ReturnType);
        Assert.Equal(
            new[]
            {
                ("authorityResult", typeof(EdgeAuthorityExecutionResult)),
                ("noveltyObservation", typeof(DynamicGraphNoveltyObservation)),
                ("subject", typeof(DynamicGraphAccuracyMeasurement)),
                ("ablated", typeof(DynamicGraphAccuracyMeasurement))
            },
            method.GetParameters()
                .Select(parameter => (parameter.Name!, parameter.ParameterType))
                .ToArray());
        Assert.Empty(Assert.IsAssignableFrom<MethodBody>(method.GetMethodBody()).ExceptionHandlingClauses);
    }

    [Fact]
    public void Measure_ComposesExactValuesWhenEdgeAndEvaluationIdsDiffer()
    {
        var authorityResult = AuthorityResult();
        var noveltyObservation = NoveltyObservation();
        var subject = Accuracy(heldOutAccuracy: 0.96d);
        var ablated = Accuracy(heldOutAccuracy: 0.93d);

        var result = DynamicGraphEdgeQualityEvidenceMeter.Measure(
            authorityResult,
            noveltyObservation,
            subject,
            ablated);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(EdgeId, result.Value!.EdgeAuthorityControl.AuthorityEvidence.EdgeId, StringComparer.Ordinal);
        Assert.Equal(EvaluationId, result.Value.Novelty.EvaluationId, StringComparer.Ordinal);
        Assert.Equal(EvaluationId, result.Value.Ablation.EvaluationId, StringComparer.Ordinal);
        Assert.Equal(73, result.Value.Novelty.RowsEvaluated);
        Assert.Equal(73, result.Value.Ablation.RowsEvaluated);
        Assert.True(result.Value.Novelty.MeetsNoveltyGate);
        Assert.True(result.Value.Ablation.MeetsQ11NumericalGate);
        Assert.Equal(0.03d, result.Value.Ablation.AccuracyDrop, 12);
    }

    [Fact]
    public void Measure_CarriesSuccessfulFalseNoveltyAndAblationGates()
    {
        var result = DynamicGraphEdgeQualityEvidenceMeter.Measure(
            AuthorityResult(),
            NoveltyObservation(newPatternRate: 0.25d, sourceTokenSeenRate: 0.75d),
            Accuracy(heldOutAccuracy: 0.94d),
            Accuracy(heldOutAccuracy: 0.93d));

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.False(result.Value!.Novelty.MeetsNoveltyGate);
        Assert.False(result.Value.Ablation.MeetsQ11NumericalGate);
    }

    [Fact]
    public void Measure_AuthorityFailurePrecedesHostileNoveltyAndAblation()
    {
        var result = DynamicGraphEdgeQualityEvidenceMeter.Measure(
            null,
            HostileNovelty(new BadImageFormatException("must not traverse novelty")),
            HostileAccuracy(new OperationCanceledException("must not traverse ablation")),
            Accuracy());

        AssertFailure(
            result,
            "INVALID_EDGE_AUTHORITY_CONTROL_MEASUREMENT",
            "Dynamic graph edge authority control measurement is invalid.");
    }

    [Fact]
    public void Measure_NoveltyFailurePrecedesHostileAblation()
    {
        var result = DynamicGraphEdgeQualityEvidenceMeter.Measure(
            AuthorityResult(),
            NoveltyObservation(splitProof: Proof(method: "SimHash")),
            HostileAccuracy(new BadImageFormatException("must not traverse ablation")),
            Accuracy());

        AssertFailure(
            result,
            "INVALID_SPLIT_PROOF",
            "Dynamic graph measurement split proof is invalid.");
    }

    [Fact]
    public void Measure_AblationSubjectFailurePrecedesAblatedTraversal()
    {
        var result = DynamicGraphEdgeQualityEvidenceMeter.Measure(
            AuthorityResult(),
            NoveltyObservation(),
            Accuracy(splitProof: Proof(trainRoots: ["shared"], evalRoots: ["shared"])),
            HostileAccuracy(new BadImageFormatException("must not traverse ablated")));

        AssertFailure(
            result,
            "TRAIN_HELDOUT_OVERLAP",
            "Training and held-out source roots overlap.");
    }

    [Fact]
    public void Measure_PreservesAblationEvaluationMismatch()
    {
        var result = DynamicGraphEdgeQualityEvidenceMeter.Measure(
            AuthorityResult(),
            NoveltyObservation(),
            Accuracy(evaluationId: "subject-evaluation"),
            Accuracy(evaluationId: "ablated-evaluation"));

        AssertFailure(
            result,
            "ABLATION_EVALUATION_MISMATCH",
            "Dynamic graph ablation measurements do not describe the same held-out evaluation.");
    }

    [Fact]
    public void Measure_PreservesNearDuplicateSplitFailureBeforeCorrelation()
    {
        var result = DynamicGraphEdgeQualityEvidenceMeter.Measure(
            AuthorityResult(),
            NoveltyObservation(splitProof: Proof(nearDuplicateCount: 1)),
            Accuracy(),
            Accuracy());

        AssertFailure(
            result,
            "TRAIN_HELDOUT_NEAR_DUPLICATE",
            "Training and held-out sources contain near-duplicates.");
    }

    [Theory]
    [InlineData("evaluation-id")]
    [InlineData("rows")]
    [InlineData("train-roots")]
    [InlineData("eval-roots")]
    [InlineData("source-hashes")]
    [InlineData("method")]
    [InlineData("threshold")]
    public void Measure_RejectsEachIndependentCrossEvidenceMismatch(string mismatch)
    {
        var noveltyEvaluationId = mismatch == "evaluation-id" ? "novelty-evaluation" : EvaluationId;
        var noveltyRows = mismatch == "rows" ? 71 : 73;
        var noveltyProof = mismatch switch
        {
            "train-roots" => Proof(trainRoots: ["train-other"]),
            "eval-roots" => Proof(evalRoots: ["eval-other"]),
            "source-hashes" => Proof(hashes: ["hash-other"]),
            "method" => Proof(method: "minhash"),
            "threshold" => Proof(threshold: 0.9d),
            _ => Proof()
        };

        var result = DynamicGraphEdgeQualityEvidenceMeter.Measure(
            AuthorityResult(),
            NoveltyObservation(
                evaluationId: noveltyEvaluationId,
                requestedRows: noveltyRows,
                rowsEvaluated: noveltyRows,
                splitProof: noveltyProof),
            Accuracy(),
            Accuracy(heldOutAccuracy: 0.93d));

        AssertFailure(
            result,
            "EDGE_QUALITY_EVALUATION_MISMATCH",
            "Dynamic graph edge quality evidence does not describe the same held-out evaluation.");
    }

    [Theory]
    [InlineData("evaluation-id-case")]
    [InlineData("train-roots-case")]
    [InlineData("eval-roots-case")]
    [InlineData("source-hashes-case")]
    [InlineData("train-roots-order")]
    [InlineData("eval-roots-order")]
    [InlineData("source-hashes-order")]
    public void Measure_RequiresOrdinalIdentityAndSequenceOrderAcrossValidEvidence(
        string mismatch)
    {
        var ablationProof = Proof(
            trainRoots: ["train-a", "train-b"],
            evalRoots: ["eval-a", "eval-b"],
            hashes: ["hash-a", "hash-b"]);
        var noveltyProof = mismatch switch
        {
            "train-roots-case" => Proof(
                trainRoots: ["TRAIN-A", "train-b"],
                evalRoots: ["eval-a", "eval-b"],
                hashes: ["hash-a", "hash-b"]),
            "eval-roots-case" => Proof(
                trainRoots: ["train-a", "train-b"],
                evalRoots: ["EVAL-A", "eval-b"],
                hashes: ["hash-a", "hash-b"]),
            "source-hashes-case" => Proof(
                trainRoots: ["train-a", "train-b"],
                evalRoots: ["eval-a", "eval-b"],
                hashes: ["HASH-A", "hash-b"]),
            "train-roots-order" => Proof(
                trainRoots: ["train-b", "train-a"],
                evalRoots: ["eval-a", "eval-b"],
                hashes: ["hash-a", "hash-b"]),
            "eval-roots-order" => Proof(
                trainRoots: ["train-a", "train-b"],
                evalRoots: ["eval-b", "eval-a"],
                hashes: ["hash-a", "hash-b"]),
            "source-hashes-order" => Proof(
                trainRoots: ["train-a", "train-b"],
                evalRoots: ["eval-a", "eval-b"],
                hashes: ["hash-b", "hash-a"]),
            _ => ablationProof
        };
        var noveltyEvaluationId = mismatch == "evaluation-id-case"
            ? "HELD-OUT-EDGE-QUALITY-EVALUATION"
            : EvaluationId;
        var noveltyObservation = NoveltyObservation(
            evaluationId: noveltyEvaluationId,
            splitProof: noveltyProof);
        var subject = Accuracy(splitProof: ablationProof);
        var ablated = Accuracy(heldOutAccuracy: 0.93d, splitProof: ablationProof);

        var novelty = DynamicGraphNoveltyMeter.Measure(noveltyObservation);
        var ablation = DynamicGraphAblationMeter.Measure(subject, ablated);
        Assert.True(novelty.IsSuccess, $"{novelty.ErrorCode}: {novelty.ErrorMessage}");
        Assert.True(ablation.IsSuccess, $"{ablation.ErrorCode}: {ablation.ErrorMessage}");

        var result = DynamicGraphEdgeQualityEvidenceMeter.Measure(
            AuthorityResult(),
            noveltyObservation,
            subject,
            ablated);

        AssertFailure(
            result,
            "EDGE_QUALITY_EVALUATION_MISMATCH",
            "Dynamic graph edge quality evidence does not describe the same held-out evaluation.");
    }

    [Fact]
    public void Measure_OrdinaryNestedSnapshotFailureMapsAtResponsibleMeter()
    {
        var result = DynamicGraphEdgeQualityEvidenceMeter.Measure(
            AuthorityResult(),
            HostileNovelty(new InvalidOperationException("ordinary snapshot failure")),
            Accuracy(),
            Accuracy());

        AssertFailure(
            result,
            "INVALID_SPLIT_PROOF",
            "Dynamic graph measurement split proof is invalid.");
    }

    [Fact]
    public void Measure_FatalNestedSnapshotFailureEscapesAsSameInstance()
    {
        var fatal = new AggregateException(
            new InvalidOperationException("ordinary sibling"),
            new OutOfMemoryException("fatal nested snapshot"));

        var thrown = Assert.Throws<AggregateException>(() =>
            DynamicGraphEdgeQualityEvidenceMeter.Measure(
                AuthorityResult(),
                HostileNovelty(fatal),
                Accuracy(),
                Accuracy()));

        Assert.Same(fatal, thrown);
    }

    [Fact]
    public void Measure_OwnsCallerSplitCarriersAndReturnsFreshDeepSnapshots()
    {
        var trainRoots = new List<string> { "train-a" };
        var evalRoots = new List<string> { "eval-a" };
        var sourceHashes = new List<string> { "hash-a" };
        var proof = Proof(trainRoots, evalRoots, sourceHashes);

        var result = DynamicGraphEdgeQualityEvidenceMeter.Measure(
            AuthorityResult(),
            NoveltyObservation(splitProof: proof),
            Accuracy(splitProof: proof),
            Accuracy(heldOutAccuracy: 0.93d, splitProof: proof));
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");

        trainRoots[0] = "caller-mutated-train";
        evalRoots[0] = "caller-mutated-eval";
        sourceHashes[0] = "caller-mutated-hash";

        var firstNovelty = result.Value!.Novelty;
        var secondNovelty = result.Value.Novelty;
        var firstAblation = result.Value.Ablation;
        var secondAblation = result.Value.Ablation;
        Assert.NotSame(firstNovelty, secondNovelty);
        Assert.NotSame(firstNovelty.SplitProof, secondNovelty.SplitProof);
        Assert.NotSame(firstAblation, secondAblation);
        Assert.NotSame(firstAblation.SplitProof, secondAblation.SplitProof);
        Assert.Equal("train-a", secondNovelty.SplitProof.TrainSourceRoots[0]);
        Assert.Equal("eval-a", secondAblation.SplitProof.EvalSourceRoots[0]);
        Assert.Equal("hash-a", secondAblation.SplitProof.SourceHashes[0]);
        AssertReadOnly(firstNovelty.SplitProof.TrainSourceRoots);
        AssertReadOnly(firstAblation.SplitProof.SourceHashes);

        SetRecordProperty(
            firstNovelty.SplitProof,
            nameof(DynamicGraphMeasurementSplitProof.TrainSourceRoots),
            new[] { "returned-copy-mutated" });
        SetRecordProperty(
            firstAblation.SplitProof,
            nameof(DynamicGraphMeasurementSplitProof.SourceHashes),
            new[] { "returned-copy-mutated" });
        Assert.Equal("train-a", result.Value.Novelty.SplitProof.TrainSourceRoots[0]);
        Assert.Equal("hash-a", result.Value.Ablation.SplitProof.SourceHashes[0]);
    }

    [Fact]
    public void Measure_ReturnsFreshAuthorityControlSnapshots()
    {
        var result = DynamicGraphEdgeQualityEvidenceMeter.Measure(
            AuthorityResult(),
            NoveltyObservation(),
            Accuracy(),
            Accuracy(heldOutAccuracy: 0.93d));
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");

        var first = result.Value!.EdgeAuthorityControl;
        var second = result.Value.EdgeAuthorityControl;
        Assert.NotSame(first, second);
        Assert.NotSame(first.AuthorityEvidence, second.AuthorityEvidence);
        Assert.NotSame(first.EdgeOrderQuality, second.EdgeOrderQuality);
        Assert.NotSame(first.EdgeIdentityQuality, second.EdgeIdentityQuality);

        GetPrivateStringArray(
            GetPrivateCarrier<DynamicGraphEdgeOrderQualityMeasurement>(first, "_edgeOrderQuality"),
            "_probeIds")[0] = "returned-copy-mutated";
        Assert.Equal(
            "candidate-order-permutation-01",
            result.Value.EdgeAuthorityControl.EdgeOrderQuality.ProbeIds[0],
            StringComparer.Ordinal);
    }

    private static EdgeAuthorityExecutionResult AuthorityResult()
    {
        var candidates = Enumerable.Range(0, 3)
            .Select(index => new EdgeCandidate(
                $"target-{(char)('a' + index)}",
                [$"PREDICATE_{(char)('A' + index)}"],
                0.2d + (index * 0.2d),
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole))
            .ToArray();
        var decision = new EdgeAuthorityDecision(
            [("target-c", 0.93d)],
            1,
            false,
            RoutingAuthorityVocabulary.TrainedEdgeUnitRole,
            candidates
                .OrderByDescending(candidate => candidate.BaselineScore)
                .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
                .Select(candidate => candidate.TargetSubPhaseId)
                .ToArray());
        var normalizedInput = new EdgeAuthorityInput(
            EdgeId,
            ["edge", "quality", "evidence"],
            candidates.SelectMany(candidate => candidate.EvidencePredicates).ToArray(),
            new ContextSignals(
                [Domain.CODE_Q],
                ["completed-sub-phase"],
                ["active-edge"],
                new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.8d }),
            candidates,
            LeakageSignals);
        var orderProbes = Enumerable.Range(0, 5)
            .Select(index => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                $"candidate-order-permutation-{index + 1:D2}",
                CreatePermutation(candidates, index),
                CopyDecision(decision)))
            .ToArray();
        var identityProbeIds = new[]
        {
            "source-id-control-01", "source-path-control-02", "correlation-id-control-03"
        };
        var controlledSignals = new[] { "source_id", "source_path", "correlation_id" };
        var controlValues = new[]
        {
            "edge-authority-source-id-control",
            "identity-control://edge-authority-source-path",
            "edge-authority-correlation-id-control"
        };
        var identityProbes = Enumerable.Range(0, 3)
            .Select(index => new DynamicGraphEdgeIdentityControlProbe(
                identityProbeIds[index],
                controlledSignals[index],
                controlValues[index],
                CopyDecision(decision)))
            .ToArray();

        return new EdgeAuthorityExecutionResult(
            normalizedInput,
            candidates,
            CopyDecision(decision),
            new DynamicGraphEdgeAuthorityEvidence(
                EdgeId,
                AuthorityUnitId,
                "L2",
                false,
                "edge-quality-baseline-unit",
                "candidate_generator",
                LeakageSignals,
                "edge-quality-source-proof",
                "edge-quality-cutover-proof",
                "edge-quality-ablation-proof"),
            new DynamicGraphEdgeCandidateOrderLeakageReport(
                EdgeId,
                AuthorityUnitId,
                true,
                orderProbes,
                LeakageSignals),
            new DynamicGraphEdgeIdentityLeakageReport(
                EdgeId,
                AuthorityUnitId,
                true,
                identityProbes,
                LeakageSignals),
            0.93d,
            ["evidence"],
            []);
    }

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
            return Rotate(reversed, -offset - 1);
        }

        return Rotate(source, offset);
    }

    private static EdgeCandidate[] Rotate(IReadOnlyList<EdgeCandidate> source, int offset)
        => Enumerable.Range(0, source.Count)
            .Select(index => CopyCandidate(source[(index + offset) % source.Count]))
            .ToArray();

    private static EdgeCandidate CopyCandidate(EdgeCandidate source)
        => source with { EvidencePredicates = source.EvidencePredicates.ToArray() };

    private static EdgeAuthorityDecision CopyDecision(EdgeAuthorityDecision source)
        => new(
            source.OrderedTargets.ToArray(),
            source.FanOutCount,
            source.KnowledgeEdge,
            source.AuthoritySource,
            source.AblationBaselineRank.ToArray());

    private static DynamicGraphNoveltyObservation NoveltyObservation(
        string evaluationId = EvaluationId,
        int requestedRows = 73,
        int rowsEvaluated = 73,
        double newPatternRate = 0.26d,
        double sourceTokenSeenRate = 0.74d,
        DynamicGraphMeasurementSplitProof? splitProof = null)
        => new(
            evaluationId,
            requestedRows,
            rowsEvaluated,
            requestedRows - rowsEvaluated,
            newPatternRate,
            sourceTokenSeenRate,
            0.75d,
            splitProof ?? Proof());

    private static DynamicGraphAccuracyMeasurement Accuracy(
        string evaluationId = EvaluationId,
        int rowsEvaluated = 73,
        double heldOutAccuracy = 0.96d,
        DynamicGraphMeasurementSplitProof? splitProof = null)
        => new(
            evaluationId,
            rowsEvaluated,
            new DynamicGraphAccuracyAxis(0.5d, heldOutAccuracy, 0.02d),
            0.025d,
            true,
            splitProof ?? Proof());

    private static DynamicGraphMeasurementSplitProof Proof(
        IReadOnlyList<string>? trainRoots = null,
        IReadOnlyList<string>? evalRoots = null,
        IReadOnlyList<string>? hashes = null,
        int nearDuplicateCount = 0,
        string method = "simhash",
        double threshold = 0.85d)
        => new(
            trainRoots ?? ["train-a"],
            evalRoots ?? ["eval-a"],
            hashes ?? ["hash-a"],
            nearDuplicateCount,
            method,
            threshold);

    private static DynamicGraphNoveltyObservation HostileNovelty(Exception exception)
        => NoveltyObservation(splitProof: Proof(
            trainRoots: new FaultingReadOnlyList<string>(exception)));

    private static DynamicGraphAccuracyMeasurement HostileAccuracy(Exception exception)
        => Accuracy(splitProof: Proof(
            trainRoots: new FaultingReadOnlyList<string>(exception)));

    private static void AssertFailure(
        OperationResult<DynamicGraphEdgeQualityEvidenceMeasurement> result,
        string code,
        string message)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(code, result.ErrorCode);
        Assert.Equal(message, result.ErrorMessage);
    }

    private static void AssertOwnedRecordShape<T>(params (string Name, Type Type)[] expected)
    {
        var constructor = Assert.Single(typeof(T).GetConstructors());
        Assert.Equal(
            expected,
            constructor.GetParameters()
                .Select(parameter => (parameter.Name!, parameter.ParameterType))
                .ToArray());
        Assert.Equal(
            expected,
            typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => (property.Name, property.PropertyType))
                .ToArray());
        Assert.All(
            typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            property => Assert.Null(property.SetMethod));
        Assert.Empty(typeof(T).GetFields(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.All(
            typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            field => Assert.True(field.IsInitOnly));
    }

    private static void AssertReadOnly(IReadOnlyList<string> values)
        => Assert.Throws<NotSupportedException>(() => ((IList<string>)values)[0] = "forbidden");

    private static void SetRecordProperty(object owner, string propertyName, object value)
    {
        var field = Assert.IsAssignableFrom<FieldInfo>(owner.GetType().GetField(
            $"<{propertyName}>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        Assert.True(field.IsInitOnly);
        field.SetValue(owner, value);
    }

    private static string[] GetPrivateStringArray(object owner, string fieldName)
    {
        var field = Assert.IsAssignableFrom<FieldInfo>(owner.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        Assert.True(field.IsInitOnly);
        return Assert.IsType<string[]>(field.GetValue(owner));
    }

    private static T GetPrivateCarrier<T>(object owner, string fieldName)
    {
        var field = Assert.IsAssignableFrom<FieldInfo>(owner.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        Assert.True(field.IsInitOnly);
        return Assert.IsType<T>(field.GetValue(owner));
    }

    private sealed class FaultingReadOnlyList<T>(Exception exception) : IReadOnlyList<T>
    {
        public int Count => 1;
        public T this[int index] => throw exception;
        public IEnumerator<T> GetEnumerator() => throw exception;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
