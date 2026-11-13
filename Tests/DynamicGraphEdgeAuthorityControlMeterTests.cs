// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

public sealed class DynamicGraphEdgeAuthorityControlMeterTests
{
    private const string InvalidCode = "INVALID_EDGE_AUTHORITY_CONTROL_MEASUREMENT";
    private const string InvalidMessage = "Dynamic graph edge authority control measurement is invalid.";
    private const string EdgeId = "edge-authority-control-distinctive";
    private const string UnitId = "edge-authority-control-unit-distinctive";

    private static readonly string[] LeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];
    private static readonly string[] OrderProbeIds =
        Enumerable.Range(1, 5).Select(index => $"candidate-order-permutation-{index:D2}").ToArray();
    private static readonly string[] IdentityProbeIds =
        ["source-id-control-01", "source-path-control-02", "correlation-id-control-03"];
    private static readonly string[] IdentityControlledSignals =
        ["source_id", "source_path", "correlation_id"];
    private static readonly string[] IdentityControlValues =
        [
            "edge-authority-source-id-control",
            "identity-control://edge-authority-source-path",
            "edge-authority-correlation-id-control"
        ];

    [Fact]
    public void PublicSurface_ExposesOnlyOwnedStructuralOrderAndIdentityEvidence()
    {
        AssertOwnedRecordShape<DynamicGraphEdgeAuthorityControlMeasurement>(
            ("AuthorityEvidence", typeof(DynamicGraphEdgeAuthorityEvidence)),
            ("EdgeOrderQuality", typeof(DynamicGraphEdgeOrderQualityMeasurement)),
            ("EdgeIdentityQuality", typeof(DynamicGraphEdgeIdentityQualityMeasurement)));
        AssertOwnedRecordShape<DynamicGraphTargetEdgeAuthorityControlMeasurement>(
            ("Target", typeof(DynamicGraphMeasurementTarget)),
            ("EdgeAuthorityControl", typeof(DynamicGraphEdgeAuthorityControlMeasurement)));

        var method = Assert.Single(typeof(DynamicGraphEdgeAuthorityControlMeter)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Equal(nameof(DynamicGraphEdgeAuthorityControlMeter.Measure), method.Name);
        Assert.Equal(
            typeof(OperationResult<DynamicGraphEdgeAuthorityControlMeasurement>),
            method.ReturnType);
        Assert.Equal(
            typeof(EdgeAuthorityExecutionResult),
            Assert.Single(method.GetParameters()).ParameterType);

        var publicSurface = string.Join(
            " ",
            typeof(DynamicGraphEdgeAuthorityControlMeasurement)
                .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(member => member.Name));
        Assert.DoesNotContain("GraphQuality", publicSurface, StringComparison.Ordinal);
        Assert.DoesNotContain("Novelty", publicSurface, StringComparison.Ordinal);
        Assert.DoesNotContain("Accuracy", publicSurface, StringComparison.Ordinal);
        Assert.DoesNotContain("Training", publicSurface, StringComparison.Ordinal);
        Assert.DoesNotContain("Promotion", publicSurface, StringComparison.Ordinal);
    }

    [Fact]
    public void Measure_CanonicalResultReturnsExactOwnedStructuralOrderAndIdentityEvidence()
    {
        var result = DynamicGraphEdgeAuthorityControlMeter.Measure(CreateResult());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var measurement = result.Value!;
        AssertAuthorityEvidence(measurement.AuthorityEvidence);
        AssertOrderQuality(measurement.EdgeOrderQuality);
        AssertIdentityQuality(measurement.EdgeIdentityQuality);

        Assert.NotSame(measurement.AuthorityEvidence, measurement.AuthorityEvidence);
        Assert.NotSame(measurement.EdgeOrderQuality, measurement.EdgeOrderQuality);
        Assert.NotSame(measurement.EdgeIdentityQuality, measurement.EdgeIdentityQuality);
        AssertOwnedReadOnlySnapshot(
            () => measurement.AuthorityEvidence.ExcludedLeakageSignals,
            "forged-authority-exclusion",
            LeakageSignals[0]);
        AssertOwnedReadOnlySnapshot(
            () => measurement.EdgeOrderQuality.ProbeIds,
            "forged-order-probe",
            OrderProbeIds[0]);
        AssertOwnedReadOnlySnapshot(
            () => measurement.EdgeIdentityQuality.ControlledSignals,
            "forged-identity-signal",
            IdentityControlledSignals[0]);
    }

    [Fact]
    public void Measure_NullResultFailsClosed()
        => AssertInvalid(DynamicGraphEdgeAuthorityControlMeter.Measure(null));

    [Fact]
    public void Measure_UsesExactlyOneSharedClassifierExceptionFilter()
    {
        var measure = Assert.IsAssignableFrom<MethodInfo>(
            typeof(DynamicGraphEdgeAuthorityControlMeter).GetMethod(
                nameof(DynamicGraphEdgeAuthorityControlMeter.Measure),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        var body = Assert.IsAssignableFrom<MethodBody>(measure.GetMethodBody());
        var clause = Assert.Single(body.ExceptionHandlingClauses);
        Assert.Equal(ExceptionHandlingClauseOptions.Filter, clause.Flags);
        Assert.True(clause.FilterOffset >= 0);
        Assert.True(clause.FilterOffset < clause.HandlerOffset);

        var classifier = Assert.IsAssignableFrom<MethodInfo>(
            typeof(SnapshotExceptionGraphClassifier).GetMethod(
                nameof(SnapshotExceptionGraphClassifier.IsNonFatal),
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: [typeof(Exception)],
                modifiers: null));
        var il = Assert.IsType<byte[]>(body.GetILAsByteArray());
        var classifierCalls = ReadIlInstructions(il, 0, il.Length)
            .Where(instruction => instruction.OpCode == OpCodes.Call
                || instruction.OpCode == OpCodes.Callvirt)
            .Where(instruction => ResolveMethod(measure, instruction.MetadataToken) == classifier)
            .ToArray();

        var classifierCall = Assert.Single(classifierCalls);
        Assert.InRange(classifierCall.Offset, clause.FilterOffset, clause.HandlerOffset - 1);
        Assert.Equal(typeof(bool), classifier.ReturnType);
    }

    [Theory]
    [InlineData(nameof(DynamicGraphEdgeAuthorityEvidence.EdgeId), null)]
    [InlineData(nameof(DynamicGraphEdgeAuthorityEvidence.AuthorityUnitId), "")]
    [InlineData(nameof(DynamicGraphEdgeAuthorityEvidence.AuthorityUnitLevel), " L2")]
    [InlineData(nameof(DynamicGraphEdgeAuthorityEvidence.BaselineUnitId), "baseline\nunit")]
    [InlineData(nameof(DynamicGraphEdgeAuthorityEvidence.BaselineRole), "candidate\u0001generator")]
    [InlineData(nameof(DynamicGraphEdgeAuthorityEvidence.SourceProof), " ")]
    [InlineData(nameof(DynamicGraphEdgeAuthorityEvidence.CutoverProof), "cutover\u2028proof")]
    [InlineData(nameof(DynamicGraphEdgeAuthorityEvidence.AblationProofRef), "ablation\rproof")]
    public void Measure_EachMalformedStructuralStringRepresentativeFailsClosed(
        string propertyName,
        string? invalidValue)
    {
        var result = CreateResult();
        SetAuthorityEvidenceProperty(result, propertyName, invalidValue);

        AssertInvalid(DynamicGraphEdgeAuthorityControlMeter.Measure(result));
    }

    [Fact]
    public void Measure_AuthorityLevelMustBeExactlyL2()
    {
        var result = CreateResult();
        SetAuthorityEvidenceProperty(
            result,
            nameof(DynamicGraphEdgeAuthorityEvidence.AuthorityUnitLevel),
            "L3");

        AssertInvalid(DynamicGraphEdgeAuthorityControlMeter.Measure(result));
    }

    [Fact]
    public void Measure_DeterministicFinalAuthorityMustBeFalse()
    {
        var result = CreateResult();
        SetAuthorityEvidenceProperty(
            result,
            nameof(DynamicGraphEdgeAuthorityEvidence.DeterministicFinalAuthority),
            true);

        AssertInvalid(DynamicGraphEdgeAuthorityControlMeter.Measure(result));
    }

    [Fact]
    public void Measure_AuthorityExclusionsMustBeTheExactCanonicalBoundary()
    {
        var result = CreateResult();
        GetPrivateStringArray(GetStoredAuthorityEvidence(result), "_excludedLeakageSignals")[0] =
            "source_id";

        AssertInvalid(DynamicGraphEdgeAuthorityControlMeter.Measure(result));
    }

    [Fact]
    public void Measure_AuthorityEdgeIdMustAgreeOrdinallyWithOrderAndIdentityEvidence()
    {
        var result = CreateResult();
        const string differentEdgeId = "different-edge-authority-control";
        SetRecordProperty(GetStoredNormalizedInput(result), nameof(EdgeAuthorityInput.EdgeId), differentEdgeId);
        SetRecordProperty(
            GetStoredOrderReport(result),
            nameof(DynamicGraphEdgeCandidateOrderLeakageReport.EdgeId),
            differentEdgeId);
        SetRecordProperty(
            GetStoredIdentityReport(result),
            nameof(DynamicGraphEdgeIdentityLeakageReport.EdgeId),
            differentEdgeId);

        Assert.True(DynamicGraphEdgeOrderQualityMeter.Measure(result).IsSuccess);
        Assert.True(DynamicGraphEdgeIdentityQualityMeter.Measure(result).IsSuccess);
        AssertInvalid(DynamicGraphEdgeAuthorityControlMeter.Measure(result));
    }

    [Fact]
    public void Measure_AuthorityUnitIdMustAgreeOrdinallyWithOrderResolvedUnit()
    {
        var result = CreateResult();
        SetRecordProperty(
            GetStoredOrderReport(result),
            nameof(DynamicGraphEdgeCandidateOrderLeakageReport.ResolvedAuthorityUnitId),
            "different-order-authority-unit");

        Assert.True(DynamicGraphEdgeOrderQualityMeter.Measure(result).IsSuccess);
        Assert.True(DynamicGraphEdgeIdentityQualityMeter.Measure(result).IsSuccess);
        AssertInvalid(DynamicGraphEdgeAuthorityControlMeter.Measure(result));
    }

    [Fact]
    public void Measure_AuthorityUnitIdMustAgreeOrdinallyWithIdentityResolvedUnit()
    {
        var result = CreateResult();
        SetRecordProperty(
            GetStoredIdentityReport(result),
            nameof(DynamicGraphEdgeIdentityLeakageReport.ResolvedAuthorityUnitId),
            "different-identity-authority-unit");

        Assert.True(DynamicGraphEdgeOrderQualityMeter.Measure(result).IsSuccess);
        Assert.True(DynamicGraphEdgeIdentityQualityMeter.Measure(result).IsSuccess);
        AssertInvalid(DynamicGraphEdgeAuthorityControlMeter.Measure(result));
    }

    [Fact]
    public void Measure_CorruptedOrderEvidencePreservesOrderMeterErrorExactly()
    {
        var result = CreateResult();
        SetResultCarrier(result, "_candidateOrderLeakageReport", null);

        AssertFailure(
            DynamicGraphEdgeAuthorityControlMeter.Measure(result),
            "INVALID_EDGE_ORDER_QUALITY_MEASUREMENT",
            "Dynamic graph edge order quality measurement is invalid.");
    }

    [Fact]
    public void Measure_CorruptedIdentityEvidencePreservesIdentityMeterErrorExactly()
    {
        var result = CreateResult();
        SetResultCarrier(result, "_identityLeakageReport", null);

        Assert.True(DynamicGraphEdgeOrderQualityMeter.Measure(result).IsSuccess);
        AssertFailure(
            DynamicGraphEdgeAuthorityControlMeter.Measure(result),
            "INVALID_EDGE_IDENTITY_QUALITY_MEASUREMENT",
            "Dynamic graph edge identity quality measurement is invalid.");
    }

    [Fact]
    public void Measure_StructuralCorruptionWinsBeforeBothNestedMeterFailures()
    {
        var result = CreateResult();
        SetAuthorityEvidenceProperty(
            result,
            nameof(DynamicGraphEdgeAuthorityEvidence.AuthorityUnitLevel),
            "L3");
        SetResultCarrier(result, "_candidateOrderLeakageReport", null);
        SetResultCarrier(result, "_identityLeakageReport", null);

        AssertInvalid(DynamicGraphEdgeAuthorityControlMeter.Measure(result));
    }

    [Fact]
    public void MeasurementConstructor_OwnsCallerArraysAndNestedCarrierStateAcrossEveryRead()
    {
        var authorityExclusions = LeakageSignals.ToArray();
        var orderProbeIds = OrderProbeIds.ToArray();
        var orderExclusions = LeakageSignals.ToArray();
        var identityProbeIds = IdentityProbeIds.ToArray();
        var identitySignals = IdentityControlledSignals.ToArray();
        var identityValues = IdentityControlValues.ToArray();
        var identityExclusions = LeakageSignals.ToArray();
        var authority = CreateAuthorityEvidence(authorityExclusions);
        var order = new DynamicGraphEdgeOrderQualityMeasurement(
            EdgeId,
            UnitId,
            5,
            true,
            orderProbeIds,
            orderExclusions);
        var identity = new DynamicGraphEdgeIdentityQualityMeasurement(
            EdgeId,
            UnitId,
            3,
            true,
            identityProbeIds,
            identitySignals,
            identityValues,
            identityExclusions);
        var measurement = new DynamicGraphEdgeAuthorityControlMeasurement(authority, order, identity);

        authorityExclusions[0] = "caller-authority-mutation";
        orderProbeIds[0] = "caller-order-probe-mutation";
        orderExclusions[0] = "caller-order-exclusion-mutation";
        identityProbeIds[0] = "caller-identity-probe-mutation";
        identitySignals[0] = "caller-identity-signal-mutation";
        identityValues[0] = "caller-identity-value-mutation";
        identityExclusions[0] = "caller-identity-exclusion-mutation";
        GetPrivateStringArray(authority, "_excludedLeakageSignals")[0] = "source-authority-mutation";
        GetPrivateStringArray(order, "_probeIds")[0] = "source-order-probe-mutation";
        GetPrivateStringArray(order, "_excludedLeakageSignals")[0] = "source-order-exclusion-mutation";
        GetPrivateStringArray(identity, "_probeIds")[0] = "source-identity-probe-mutation";
        GetPrivateStringArray(identity, "_controlledSignals")[0] = "source-identity-signal-mutation";
        GetPrivateStringArray(identity, "_controlValues")[0] = "source-identity-value-mutation";
        GetPrivateStringArray(identity, "_excludedLeakageSignals")[0] =
            "source-identity-exclusion-mutation";

        var returnedAuthority = measurement.AuthorityEvidence;
        var returnedOrder = measurement.EdgeOrderQuality;
        var returnedIdentity = measurement.EdgeIdentityQuality;
        GetPrivateStringArray(returnedAuthority, "_excludedLeakageSignals")[0] =
            "returned-authority-mutation";
        GetPrivateStringArray(returnedOrder, "_probeIds")[0] = "returned-order-probe-mutation";
        GetPrivateStringArray(returnedIdentity, "_controlledSignals")[0] =
            "returned-identity-signal-mutation";

        AssertAuthorityEvidence(measurement.AuthorityEvidence);
        AssertOrderQuality(measurement.EdgeOrderQuality);
        AssertIdentityQuality(measurement.EdgeIdentityQuality);
    }

    [Fact]
    public void TargetMeasurement_OwnsTargetAndCombinedMeasurementAcrossEveryRead()
    {
        var combined = Assert.IsType<DynamicGraphEdgeAuthorityControlMeasurement>(
            DynamicGraphEdgeAuthorityControlMeter.Measure(CreateResult()).Value);
        var target = new DynamicGraphMeasurementTarget(
            EdgeId,
            DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
            DynamicGraphMeasurementMechanismKind.LearningSubject,
            false);
        var association = new DynamicGraphTargetEdgeAuthorityControlMeasurement(target, combined);

        SetRecordProperty(target, nameof(DynamicGraphMeasurementTarget.TargetId), "source-target-mutation");
        GetPrivateStringArray(
            GetPrivateCarrier<DynamicGraphEdgeOrderQualityMeasurement>(combined, "_edgeOrderQuality"),
            "_probeIds")[0] = "source-combined-probe-mutation";

        var firstTarget = association.Target;
        var secondTarget = association.Target;
        var firstMeasurement = association.EdgeAuthorityControl;
        var secondMeasurement = association.EdgeAuthorityControl;
        Assert.NotSame(firstTarget, secondTarget);
        Assert.NotSame(firstMeasurement, secondMeasurement);
        SetRecordProperty(firstTarget, nameof(DynamicGraphMeasurementTarget.TargetId), "mutated-target");
        GetPrivateStringArray(
            GetPrivateCarrier<DynamicGraphEdgeOrderQualityMeasurement>(
                firstMeasurement,
                "_edgeOrderQuality"),
            "_probeIds")[0] = "mutated-probe";

        Assert.Equal(EdgeId, association.Target.TargetId, StringComparer.Ordinal);
        AssertOrderQuality(association.EdgeAuthorityControl.EdgeOrderQuality);
    }

    private static EdgeAuthorityExecutionResult CreateResult()
    {
        var candidates = CreateCandidates();
        var decision = CreateDecision(candidates);
        var normalizedInput = new EdgeAuthorityInput(
            EdgeId,
            ["edge", "authority", "control"],
            candidates.SelectMany(candidate => candidate.EvidencePredicates).ToArray(),
            new ContextSignals(
                [Domain.CODE_Q],
                ["completed-authority-control-sub-phase"],
                ["active-authority-control-edge"],
                new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.84d }),
            candidates,
            LeakageSignals);
        var orderReport = new DynamicGraphEdgeCandidateOrderLeakageReport(
            EdgeId,
            UnitId,
            true,
            Enumerable.Range(0, 5)
                .Select(index => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                    OrderProbeIds[index],
                    CreatePermutation(candidates, index),
                    CopyDecision(decision)))
                .ToArray(),
            LeakageSignals);
        var identityReport = new DynamicGraphEdgeIdentityLeakageReport(
            EdgeId,
            UnitId,
            true,
            Enumerable.Range(0, 3)
                .Select(index => new DynamicGraphEdgeIdentityControlProbe(
                    IdentityProbeIds[index],
                    IdentityControlledSignals[index],
                    IdentityControlValues[index],
                    CopyDecision(decision)))
                .ToArray(),
            LeakageSignals);

        return new EdgeAuthorityExecutionResult(
            normalizedInput,
            candidates.Select(CopyCandidate).ToArray(),
            CopyDecision(decision),
            CreateAuthorityEvidence(LeakageSignals),
            orderReport,
            identityReport,
            0.94d,
            ["authority-control-evidence"],
            []);
    }

    private static DynamicGraphEdgeAuthorityEvidence CreateAuthorityEvidence(
        IReadOnlyList<string> exclusions)
        => new(
            EdgeId,
            UnitId,
            "L2",
            false,
            "edge-authority-control-baseline-unit",
            "candidate_generator",
            exclusions,
            "edge-authority-control-source-proof",
            "edge-authority-control-cutover-proof",
            "edge-authority-control-ablation-proof-ref");

    private static EdgeCandidate[] CreateCandidates()
        => Enumerable.Range(0, 3)
            .Select(index => new EdgeCandidate(
                $"target-{(char)('a' + index)}",
                [$"AUTHORITY_CONTROL_PREDICATE_{(char)('A' + index)}"],
                0.22d + (index * 0.2d),
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole))
            .ToArray();

    private static EdgeAuthorityDecision CreateDecision(IReadOnlyList<EdgeCandidate> candidates)
        => new(
            [("target-c", 0.94d)],
            1,
            false,
            RoutingAuthorityVocabulary.TrainedEdgeUnitRole,
            candidates
                .OrderByDescending(candidate => candidate.BaselineScore)
                .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
                .Select(candidate => candidate.TargetSubPhaseId)
                .ToArray());

    private static EdgeCandidate[] CreatePermutation(IReadOnlyList<EdgeCandidate> candidates, int index)
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

    private static DynamicGraphEdgeAuthorityEvidence GetStoredAuthorityEvidence(
        EdgeAuthorityExecutionResult result)
        => GetResultCarrier<DynamicGraphEdgeAuthorityEvidence>(result, "_authorityEvidence");

    private static EdgeAuthorityInput GetStoredNormalizedInput(EdgeAuthorityExecutionResult result)
        => GetResultCarrier<EdgeAuthorityInput>(result, "_normalizedInput");

    private static DynamicGraphEdgeCandidateOrderLeakageReport GetStoredOrderReport(
        EdgeAuthorityExecutionResult result)
        => GetResultCarrier<DynamicGraphEdgeCandidateOrderLeakageReport>(
            result,
            "_candidateOrderLeakageReport");

    private static DynamicGraphEdgeIdentityLeakageReport GetStoredIdentityReport(
        EdgeAuthorityExecutionResult result)
        => GetResultCarrier<DynamicGraphEdgeIdentityLeakageReport>(result, "_identityLeakageReport");

    private static T GetResultCarrier<T>(EdgeAuthorityExecutionResult result, string fieldName)
    {
        var field = Assert.IsAssignableFrom<FieldInfo>(
            typeof(EdgeAuthorityExecutionResult).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        Assert.True(field.IsInitOnly);
        return Assert.IsType<T>(field.GetValue(result));
    }

    private static void SetResultCarrier(
        EdgeAuthorityExecutionResult result,
        string fieldName,
        object? value)
    {
        var field = Assert.IsAssignableFrom<FieldInfo>(
            typeof(EdgeAuthorityExecutionResult).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        Assert.True(field.IsInitOnly);
        field.SetValue(result, value);
    }

    private static void SetAuthorityEvidenceProperty(
        EdgeAuthorityExecutionResult result,
        string propertyName,
        object? value)
        => SetRecordProperty(GetStoredAuthorityEvidence(result), propertyName, value);

    private static void SetRecordProperty(object owner, string propertyName, object? value)
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

    private static void AssertAuthorityEvidence(DynamicGraphEdgeAuthorityEvidence evidence)
    {
        Assert.Equal(EdgeId, evidence.EdgeId, StringComparer.Ordinal);
        Assert.Equal(UnitId, evidence.AuthorityUnitId, StringComparer.Ordinal);
        Assert.Equal("L2", evidence.AuthorityUnitLevel, StringComparer.Ordinal);
        Assert.False(evidence.DeterministicFinalAuthority);
        Assert.Equal("edge-authority-control-baseline-unit", evidence.BaselineUnitId, StringComparer.Ordinal);
        Assert.Equal("candidate_generator", evidence.BaselineRole, StringComparer.Ordinal);
        Assert.Equal(LeakageSignals, evidence.ExcludedLeakageSignals);
        Assert.Equal("edge-authority-control-source-proof", evidence.SourceProof, StringComparer.Ordinal);
        Assert.Equal("edge-authority-control-cutover-proof", evidence.CutoverProof, StringComparer.Ordinal);
        Assert.Equal(
            "edge-authority-control-ablation-proof-ref",
            evidence.AblationProofRef,
            StringComparer.Ordinal);
    }

    private static void AssertOrderQuality(DynamicGraphEdgeOrderQualityMeasurement measurement)
    {
        Assert.Equal(EdgeId, measurement.EdgeId, StringComparer.Ordinal);
        Assert.Equal(UnitId, measurement.ResolvedAuthorityUnitId, StringComparer.Ordinal);
        Assert.Equal(5, measurement.PermutationProbeCount);
        Assert.True(measurement.PermutationInvariant);
        Assert.Equal(OrderProbeIds, measurement.ProbeIds);
        Assert.Equal(LeakageSignals, measurement.ExcludedLeakageSignals);
    }

    private static void AssertIdentityQuality(DynamicGraphEdgeIdentityQualityMeasurement measurement)
    {
        Assert.Equal(EdgeId, measurement.EdgeId, StringComparer.Ordinal);
        Assert.Equal(UnitId, measurement.ResolvedAuthorityUnitId, StringComparer.Ordinal);
        Assert.Equal(3, measurement.IdentityControlProbeCount);
        Assert.True(measurement.IdentityInvariant);
        Assert.Equal(IdentityProbeIds, measurement.ProbeIds);
        Assert.Equal(IdentityControlledSignals, measurement.ControlledSignals);
        Assert.Equal(IdentityControlValues, measurement.ControlValues);
        Assert.Equal(LeakageSignals, measurement.ExcludedLeakageSignals);
    }

    private static void AssertOwnedReadOnlySnapshot(
        Func<IReadOnlyList<string>> getter,
        string mutation,
        string expectedFirstValue)
    {
        var first = getter();
        var second = getter();
        Assert.NotSame(first, second);
        Assert.IsType<ReadOnlyCollection<string>>(first);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)first)[0] = mutation);
        Assert.Equal(expectedFirstValue, second[0], StringComparer.Ordinal);
    }

    private static void AssertInvalid(
        OperationResult<DynamicGraphEdgeAuthorityControlMeasurement> result)
        => AssertFailure(result, InvalidCode, InvalidMessage);

    private static void AssertFailure(
        OperationResult<DynamicGraphEdgeAuthorityControlMeasurement> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
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
        Assert.Empty(typeof(T).GetFields(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));
    }

    private static MethodBase ResolveMethod(MethodInfo owner, int? metadataToken)
    {
        Assert.True(metadataToken.HasValue);
        return owner.Module.ResolveMethod(
            metadataToken.Value,
            owner.DeclaringType?.GetGenericArguments(),
            owner.GetGenericArguments())!;
    }

    private static IReadOnlyList<IlInstruction> ReadIlInstructions(
        byte[] il,
        int startOffset,
        int endOffset)
    {
        ArgumentNullException.ThrowIfNull(il);
        if (startOffset < 0 || endOffset < startOffset || endOffset > il.Length)
            throw new ArgumentOutOfRangeException(nameof(startOffset));

        var instructions = new List<IlInstruction>();
        var offset = startOffset;
        while (offset < endOffset)
        {
            var instructionOffset = offset;
            var opCode = ReadOpCode(il, ref offset, endOffset);
            var operandOffset = offset;
            var operandSize = GetOperandSize(opCode, il, operandOffset, endOffset);
            int? metadataToken = null;
            if (opCode.OperandType == OperandType.InlineMethod)
                metadataToken = ReadInt32(il, operandOffset, endOffset);

            offset = checked(offset + operandSize);
            if (offset > endOffset)
                throw new InvalidOperationException("IL operand extends beyond the requested boundary.");

            instructions.Add(new IlInstruction(instructionOffset, opCode, metadataToken));
        }

        if (offset != endOffset)
            throw new InvalidOperationException("IL decoder did not finish on the requested boundary.");

        return instructions;
    }

    private static OpCode ReadOpCode(byte[] il, ref int offset, int endOffset)
    {
        EnsureAvailable(offset, 1, endOffset);
        var first = il[offset++];
        ushort value = first;
        if (first == 0xfe)
        {
            EnsureAvailable(offset, 1, endOffset);
            value = (ushort)(0xfe00 | il[offset++]);
        }

        if (!IlOpCodes.TryGetValue(value, out var opCode))
            throw new InvalidOperationException($"Unknown IL opcode 0x{value:X4}.");

        return opCode;
    }

    private static int GetOperandSize(OpCode opCode, byte[] il, int operandOffset, int endOffset)
        => opCode.OperandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineBrTarget
                or OperandType.InlineField
                or OperandType.InlineI
                or OperandType.InlineMethod
                or OperandType.InlineSig
                or OperandType.InlineString
                or OperandType.InlineTok
                or OperandType.InlineType
                or OperandType.ShortInlineR => 4,
            OperandType.InlineI8 or OperandType.InlineR => 8,
            OperandType.InlineSwitch => GetInlineSwitchOperandSize(il, operandOffset, endOffset),
            _ => throw new InvalidOperationException($"Unsupported IL operand type {opCode.OperandType}.")
        };

    private static int GetInlineSwitchOperandSize(byte[] il, int operandOffset, int endOffset)
    {
        var count = ReadInt32(il, operandOffset, endOffset);
        if (count < 0)
            throw new InvalidOperationException("IL switch has a negative target count.");

        var size = checked(4 + (count * 4));
        EnsureAvailable(operandOffset, size, endOffset);
        return size;
    }

    private static int ReadInt32(byte[] il, int offset, int endOffset)
    {
        EnsureAvailable(offset, sizeof(int), endOffset);
        return BitConverter.ToInt32(il, offset);
    }

    private static void EnsureAvailable(int offset, int size, int endOffset)
    {
        if (offset < 0 || size < 0 || offset > endOffset - size)
            throw new InvalidOperationException("IL instruction extends beyond the requested boundary.");
    }

    private static readonly IReadOnlyDictionary<ushort, OpCode> IlOpCodes = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.FieldType == typeof(OpCode))
        .Select(field => (OpCode)field.GetValue(null)!)
        .ToDictionary(opCode => unchecked((ushort)opCode.Value));

    private readonly record struct IlInstruction(int Offset, OpCode OpCode, int? MetadataToken);
}
