// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

public sealed class DynamicGraphEdgeIdentityQualityMeterTests
{
    private const string InvalidCode = "INVALID_EDGE_IDENTITY_QUALITY_MEASUREMENT";
    private const string InvalidMessage = "Dynamic graph edge identity quality measurement is invalid.";
    private const string EdgeId = "edge-identity-quality-distinctive";
    private const string UnitId = "learned-edge-identity-authority-distinctive";

    private static readonly string[] LeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];
    private static readonly string[] ProbeIds =
        ["source-id-control-01", "source-path-control-02", "correlation-id-control-03"];
    private static readonly string[] ControlledSignals =
        ["source_id", "source_path", "correlation_id"];
    private static readonly string[] ControlValues =
        [
            "edge-authority-source-id-control",
            "identity-control://edge-authority-source-path",
            "edge-authority-correlation-id-control"
        ];

    [Fact]
    public void PublicSurface_ContainsOnlyIdentityAndExclusionEvidence()
    {
        var properties = typeof(DynamicGraphEdgeIdentityQualityMeasurement)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType))
            .ToArray();
        Assert.Equal(
            [
                ("ControlValues", typeof(IReadOnlyList<string>)),
                ("ControlledSignals", typeof(IReadOnlyList<string>)),
                ("EdgeId", typeof(string)),
                ("ExcludedLeakageSignals", typeof(IReadOnlyList<string>)),
                ("IdentityControlProbeCount", typeof(int)),
                ("IdentityInvariant", typeof(bool)),
                ("ProbeIds", typeof(IReadOnlyList<string>)),
                ("ResolvedAuthorityUnitId", typeof(string))
            ],
            properties);

        var method = Assert.Single(typeof(DynamicGraphEdgeIdentityQualityMeter)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Equal(nameof(DynamicGraphEdgeIdentityQualityMeter.Measure), method.Name);
        Assert.Equal(
            typeof(OperationResult<DynamicGraphEdgeIdentityQualityMeasurement>),
            method.ReturnType);
        Assert.Equal(
            typeof(EdgeAuthorityExecutionResult),
            Assert.Single(method.GetParameters()).ParameterType);

        var forbidden = new[]
        {
            "CandidateOrder", "Aggregate", "Trained", "Promotion", "Lifecycle", "Training"
        };
        var publicMemberNames = typeof(DynamicGraphEdgeIdentityQualityMeasurement)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member.MemberType is MemberTypes.Property or MemberTypes.Field)
            .Select(member => member.Name)
            .Concat(typeof(DynamicGraphEdgeIdentityQualityMeter)
                .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(member => member.MemberType is MemberTypes.Property or MemberTypes.Field)
                .Select(member => member.Name));
        Assert.DoesNotContain(
            publicMemberNames,
            name => forbidden.Any(term => name.Contains(term, StringComparison.Ordinal)));
    }

    [Fact]
    public void Measure_DistinctiveCanonicalResultReturnsOwnedReadOnlyIdentityEvidence()
    {
        var result = DynamicGraphEdgeIdentityQualityMeter.Measure(CreateResult());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var measurement = result.Value!;
        Assert.Equal(EdgeId, measurement.EdgeId);
        Assert.Equal(UnitId, measurement.ResolvedAuthorityUnitId);
        Assert.Equal(3, measurement.IdentityControlProbeCount);
        Assert.True(measurement.IdentityInvariant);
        Assert.Equal(ProbeIds, measurement.ProbeIds);
        Assert.Equal(ControlledSignals, measurement.ControlledSignals);
        Assert.Equal(ControlValues, measurement.ControlValues);
        Assert.Equal(LeakageSignals, measurement.ExcludedLeakageSignals);

        AssertOwnedReadOnlySnapshot(() => measurement.ProbeIds, "forged-probe-id", ProbeIds[0]);
        AssertOwnedReadOnlySnapshot(
            () => measurement.ControlledSignals,
            "forged-signal",
            ControlledSignals[0]);
        AssertOwnedReadOnlySnapshot(() => measurement.ControlValues, "forged-control", ControlValues[0]);
        AssertOwnedReadOnlySnapshot(
            () => measurement.ExcludedLeakageSignals,
            "forged-exclusion",
            LeakageSignals[0]);
    }

    [Fact]
    public void MeasurementConstructor_OwnsEveryCallerCollection()
    {
        var probeIds = ProbeIds.ToArray();
        var controlledSignals = ControlledSignals.ToArray();
        var controlValues = ControlValues.ToArray();
        var exclusions = LeakageSignals.ToArray();
        var measurement = new DynamicGraphEdgeIdentityQualityMeasurement(
            EdgeId,
            UnitId,
            3,
            true,
            probeIds,
            controlledSignals,
            controlValues,
            exclusions);

        probeIds[0] = "mutated-probe";
        controlledSignals[0] = "mutated-signal";
        controlValues[0] = "mutated-control";
        exclusions[0] = "mutated-exclusion";

        Assert.Equal(ProbeIds, measurement.ProbeIds);
        Assert.Equal(ControlledSignals, measurement.ControlledSignals);
        Assert.Equal(ControlValues, measurement.ControlValues);
        Assert.Equal(LeakageSignals, measurement.ExcludedLeakageSignals);
    }

    [Theory]
    [InlineData("ProbeIds")]
    [InlineData("ControlledSignals")]
    [InlineData("ControlValues")]
    [InlineData("ExcludedLeakageSignals")]
    public void MeasurementConstructor_NullGuardsEveryCollection(string parameterName)
    {
        IReadOnlyList<string>? probeIds = ProbeIds;
        IReadOnlyList<string>? controlledSignals = ControlledSignals;
        IReadOnlyList<string>? controlValues = ControlValues;
        IReadOnlyList<string>? exclusions = LeakageSignals;
        switch (parameterName)
        {
            case "ProbeIds":
                probeIds = null;
                break;
            case "ControlledSignals":
                controlledSignals = null;
                break;
            case "ControlValues":
                controlValues = null;
                break;
            case "ExcludedLeakageSignals":
                exclusions = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(parameterName));
        }

        var exception = Assert.Throws<ArgumentNullException>(() =>
            new DynamicGraphEdgeIdentityQualityMeasurement(
                EdgeId,
                UnitId,
                3,
                true,
                probeIds!,
                controlledSignals!,
                controlValues!,
                exclusions!));
        Assert.Equal(parameterName, exception.ParamName);
    }

    [Fact]
    public void Measure_NullReturnsExactStableFailure()
        => AssertInvalid(DynamicGraphEdgeIdentityQualityMeter.Measure(null));

    [Fact]
    public void Measure_NonFatalCorruptedOwnedCarrierReturnsExactStableFailure()
    {
        var result = CreateResult();
        CorruptIdentityReport(result);

        AssertInvalid(DynamicGraphEdgeIdentityQualityMeter.Measure(result));
    }

    [Fact]
    public void Measure_UsesOneSharedClassifierFilterAcrossExtractionAndHelperValidation()
    {
        var measure = Assert.IsAssignableFrom<MethodInfo>(
            typeof(DynamicGraphEdgeIdentityQualityMeter).GetMethod(
                nameof(DynamicGraphEdgeIdentityQualityMeter.Measure),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        var body = Assert.IsAssignableFrom<MethodBody>(measure.GetMethodBody());
        var clause = Assert.Single(body.ExceptionHandlingClauses);
        Assert.Equal(ExceptionHandlingClauseOptions.Filter, clause.Flags);
        Assert.True(clause.FilterOffset >= 0);
        Assert.True(clause.FilterOffset < clause.HandlerOffset);

        var il = Assert.IsType<byte[]>(body.GetILAsByteArray());
        var instructions = ReadIlInstructions(il, 0, il.Length);
        var filterCalls = instructions
            .Where(instruction => instruction.Offset >= clause.FilterOffset
                && instruction.Offset < clause.HandlerOffset
                && instruction.OpCode is var opCode
                && (opCode == OpCodes.Call || opCode == OpCodes.Callvirt))
            .Select(instruction => ResolveMethod(measure, instruction.MetadataToken))
            .ToArray();
        var classifier = Assert.IsAssignableFrom<MethodInfo>(
            typeof(SnapshotExceptionGraphClassifier).GetMethod(
                nameof(SnapshotExceptionGraphClassifier.IsNonFatal),
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: [typeof(Exception)],
                modifiers: null));
        Assert.Equal(classifier, Assert.Single(filterCalls));

        var protectedCalls = instructions
            .Where(instruction => instruction.Offset >= clause.TryOffset
                && instruction.Offset < clause.TryOffset + clause.TryLength
                && instruction.OpCode is var opCode
                && (opCode == OpCodes.Call || opCode == OpCodes.Callvirt))
            .Select(instruction => ResolveMethod(measure, instruction.MetadataToken))
            .OfType<MethodInfo>()
            .ToArray();
        Assert.Contains(
            protectedCalls,
            method => method.DeclaringType == typeof(DynamicGraphEdgeAuthorityExecutionPipeline)
                && method.Name == "TrySnapshotNormalizedInput");
        Assert.Contains(
            protectedCalls,
            method => method.DeclaringType == typeof(DynamicGraphEdgeAuthorityExecutionPipeline)
                && method.Name == "TrySnapshotDecision");
    }

    [Theory]
    [InlineData("report-edge-mismatch")]
    [InlineData("report-edge-case-mismatch")]
    [InlineData("invalid-unit-id")]
    [InlineData("control-unit-id")]
    [InlineData("false-invariant")]
    [InlineData("missing-report-exclusion")]
    [InlineData("reordered-report-exclusions")]
    [InlineData("extra-report-exclusion")]
    [InlineData("missing-input-exclusion")]
    [InlineData("reordered-input-exclusions")]
    [InlineData("extra-input-exclusion")]
    [InlineData("two-probes")]
    [InlineData("four-probes")]
    [InlineData("wrong-probe-id")]
    [InlineData("duplicate-probe-id")]
    [InlineData("probe-id-case-mismatch")]
    [InlineData("wrong-controlled-signal")]
    [InlineData("duplicate-controlled-signal")]
    [InlineData("reordered-controlled-signals")]
    [InlineData("controlled-signal-case-mismatch")]
    [InlineData("wrong-control-value")]
    [InlineData("control-value-case-mismatch")]
    [InlineData("probe-decision-ordered-targets")]
    [InlineData("probe-decision-fan-out")]
    [InlineData("probe-decision-knowledge-edge")]
    [InlineData("probe-decision-authority-source")]
    [InlineData("probe-decision-ablation-rank")]
    [InlineData("execution-decision")]
    [InlineData("proposed-candidate-agreement")]
    [InlineData("corrupted-owned-carrier")]
    public void Measure_RejectsEachForgedIdentityBoundaryWithSameStableFailure(string mutation)
        => AssertInvalid(DynamicGraphEdgeIdentityQualityMeter.Measure(CreateResult(mutation)));

    private static EdgeAuthorityExecutionResult CreateResult(string mutation = "none")
    {
        var normalizedCandidates = CreateCandidates();
        var proposedCandidates = normalizedCandidates.Select(CopyCandidate).ToArray();
        var inputExclusions = LeakageSignals.ToArray();
        var reportEdgeId = EdgeId;
        var reportUnitId = UnitId;
        var identityInvariant = true;
        var reportExclusions = LeakageSignals.ToArray();
        var canonicalDecision = CreateDecision(normalizedCandidates);
        var executionDecision = CopyDecision(canonicalDecision);
        var probes = Enumerable.Range(0, 3)
            .Select(index => new DynamicGraphEdgeIdentityControlProbe(
                ProbeIds[index],
                ControlledSignals[index],
                ControlValues[index],
                canonicalDecision))
            .ToList();
        var corruptOwnedCarrier = false;

        switch (mutation)
        {
            case "none":
                break;
            case "report-edge-mismatch":
                reportEdgeId = "different-edge";
                break;
            case "report-edge-case-mismatch":
                reportEdgeId = EdgeId.ToUpperInvariant();
                break;
            case "invalid-unit-id":
                reportUnitId = " ";
                break;
            case "control-unit-id":
                reportUnitId = "learned\u0001unit";
                break;
            case "false-invariant":
                identityInvariant = false;
                break;
            case "missing-report-exclusion":
                reportExclusions = reportExclusions.SkipLast(1).ToArray();
                break;
            case "reordered-report-exclusions":
                reportExclusions = reportExclusions.Reverse().ToArray();
                break;
            case "extra-report-exclusion":
                reportExclusions = [.. reportExclusions, "extra_signal"];
                break;
            case "missing-input-exclusion":
                inputExclusions = inputExclusions.SkipLast(1).ToArray();
                break;
            case "reordered-input-exclusions":
                inputExclusions = inputExclusions.Reverse().ToArray();
                break;
            case "extra-input-exclusion":
                inputExclusions = [.. inputExclusions, "extra_signal"];
                break;
            case "two-probes":
                probes.RemoveAt(probes.Count - 1);
                break;
            case "four-probes":
                probes.Add(new DynamicGraphEdgeIdentityControlProbe(
                    "extra-probe",
                    "extra_signal",
                    "extra-control",
                    canonicalDecision));
                break;
            case "wrong-probe-id":
                probes[0] = CopyProbe(probes[0], probeId: "wrong-probe");
                break;
            case "duplicate-probe-id":
                probes[1] = CopyProbe(probes[1], probeId: probes[0].ProbeId);
                break;
            case "probe-id-case-mismatch":
                probes[0] = CopyProbe(probes[0], probeId: probes[0].ProbeId.ToUpperInvariant());
                break;
            case "wrong-controlled-signal":
                probes[0] = CopyProbe(probes[0], controlledSignal: "wrong_signal");
                break;
            case "duplicate-controlled-signal":
                probes[1] = CopyProbe(probes[1], controlledSignal: probes[0].ControlledSignal);
                break;
            case "reordered-controlled-signals":
                (probes[0], probes[1]) =
                    (CopyProbe(probes[0], controlledSignal: probes[1].ControlledSignal),
                     CopyProbe(probes[1], controlledSignal: probes[0].ControlledSignal));
                break;
            case "controlled-signal-case-mismatch":
                probes[0] = CopyProbe(
                    probes[0],
                    controlledSignal: probes[0].ControlledSignal.ToUpperInvariant());
                break;
            case "wrong-control-value":
                probes[0] = CopyProbe(probes[0], controlValue: "wrong-control");
                break;
            case "control-value-case-mismatch":
                probes[0] = CopyProbe(probes[0], controlValue: probes[0].ControlValue.ToUpperInvariant());
                break;
            case "probe-decision-ordered-targets":
                probes[0] = CopyProbe(
                    probes[0],
                    decision: canonicalDecision with { OrderedTargets = [("target-b", 0.91d)] });
                break;
            case "probe-decision-fan-out":
                probes[0] = CopyProbe(
                    probes[0],
                    decision: canonicalDecision with { FanOutCount = canonicalDecision.FanOutCount + 1 });
                break;
            case "probe-decision-knowledge-edge":
                probes[0] = CopyProbe(
                    probes[0],
                    decision: canonicalDecision with { KnowledgeEdge = !canonicalDecision.KnowledgeEdge });
                break;
            case "probe-decision-authority-source":
                probes[0] = CopyProbe(
                    probes[0],
                    decision: canonicalDecision with
                    {
                        AuthoritySource = RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole
                    });
                break;
            case "probe-decision-ablation-rank":
                probes[0] = CopyProbe(
                    probes[0],
                    decision: canonicalDecision with
                    {
                        AblationBaselineRank = canonicalDecision.AblationBaselineRank.Reverse().ToArray()
                    });
                break;
            case "execution-decision":
                executionDecision = canonicalDecision with { OrderedTargets = [("target-b", 0.91d)] };
                break;
            case "proposed-candidate-agreement":
                proposedCandidates[0] = proposedCandidates[0] with
                {
                    BaselineScore = proposedCandidates[0].BaselineScore + 0.01d
                };
                break;
            case "corrupted-owned-carrier":
                corruptOwnedCarrier = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mutation));
        }

        var normalizedInput = new EdgeAuthorityInput(
            EdgeId,
            ["identity", "quality", "distinctive"],
            normalizedCandidates.SelectMany(candidate => candidate.EvidencePredicates).ToArray(),
            new ContextSignals(
                [Domain.CODE_Q],
                ["completed-distinctive-sub-phase"],
                ["active-distinctive-edge"],
                new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.83d }),
            normalizedCandidates,
            inputExclusions);
        var identityReport = new DynamicGraphEdgeIdentityLeakageReport(
            reportEdgeId,
            reportUnitId,
            identityInvariant,
            probes,
            reportExclusions);
        var deliberatelyUntrustedOrderReport = new DynamicGraphEdgeCandidateOrderLeakageReport(
            "untrusted-order-edge",
            "untrusted-order-unit",
            false,
            [],
            ["not-an-exact-exclusion-boundary"]);
        var result = new EdgeAuthorityExecutionResult(
            normalizedInput,
            proposedCandidates,
            executionDecision,
            new DynamicGraphEdgeAuthorityEvidence(
                EdgeId,
                UnitId,
                "L2",
                false,
                "identity-quality-baseline-unit",
                "candidate_generator",
                LeakageSignals,
                "identity-quality-source-proof",
                "identity-quality-cutover-proof",
                "identity-quality-ablation-proof"),
            deliberatelyUntrustedOrderReport,
            identityReport,
            0.94d,
            ["identity-evidence"],
            []);
        if (corruptOwnedCarrier)
            CorruptIdentityReport(result);

        return result;
    }

    private static EdgeCandidate[] CreateCandidates()
        => Enumerable.Range(0, 3)
            .Select(index => new EdgeCandidate(
                $"target-{(char)('a' + index)}",
                [$"IDENTITY_PREDICATE_{(char)('A' + index)}"],
                0.21d + (index * 0.2d),
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

    private static DynamicGraphEdgeIdentityControlProbe CopyProbe(
        DynamicGraphEdgeIdentityControlProbe source,
        string? probeId = null,
        string? controlledSignal = null,
        string? controlValue = null,
        EdgeAuthorityDecision? decision = null)
        => new(
            probeId ?? source.ProbeId,
            controlledSignal ?? source.ControlledSignal,
            controlValue ?? source.ControlValue,
            decision ?? source.Decision);

    private static EdgeCandidate CopyCandidate(EdgeCandidate source)
        => source with { EvidencePredicates = source.EvidencePredicates.ToArray() };

    private static EdgeAuthorityDecision CopyDecision(EdgeAuthorityDecision source)
        => new(
            source.OrderedTargets.ToArray(),
            source.FanOutCount,
            source.KnowledgeEdge,
            source.AuthoritySource,
            source.AblationBaselineRank.ToArray());

    private static void CorruptIdentityReport(EdgeAuthorityExecutionResult result)
    {
        var field = Assert.IsAssignableFrom<FieldInfo>(
            typeof(EdgeAuthorityExecutionResult).GetField(
                "_identityLeakageReport",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        Assert.True(field.IsInitOnly);
        field.SetValue(result, null);
    }

    private static void AssertOwnedReadOnlySnapshot(
        Func<IReadOnlyList<string>> getter,
        string mutation,
        string expectedFirstValue)
    {
        var firstRead = getter();
        var secondRead = getter();
        Assert.NotSame(firstRead, secondRead);
        var owner = Assert.IsType<ReadOnlyCollection<string>>(firstRead);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)owner)[0] = mutation);
        Assert.Equal(expectedFirstValue, secondRead[0]);
    }

    private static void AssertInvalid(OperationResult<DynamicGraphEdgeIdentityQualityMeasurement> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(InvalidCode, result.ErrorCode);
        Assert.Equal(InvalidMessage, result.ErrorMessage);
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
