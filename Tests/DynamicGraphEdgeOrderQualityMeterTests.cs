// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using System.Reflection.Emit;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

public sealed class DynamicGraphEdgeOrderQualityMeterTests
{
    private const string InvalidCode = "INVALID_EDGE_ORDER_QUALITY_MEASUREMENT";
    private const string InvalidMessage = "Dynamic graph edge order quality measurement is invalid.";
    private const string EdgeId = "edge-order-quality";
    private const string UnitId = "learned-edge-authority";

    private static readonly string[] LeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];
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
    public void PublicSurface_ContainsOnlyOrderAndExclusionEvidence()
    {
        var properties = typeof(DynamicGraphEdgeOrderQualityMeasurement)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType))
            .ToArray();
        Assert.Equal(
            [
                ("EdgeId", typeof(string)),
                ("ExcludedLeakageSignals", typeof(IReadOnlyList<string>)),
                ("PermutationInvariant", typeof(bool)),
                ("PermutationProbeCount", typeof(int)),
                ("ProbeIds", typeof(IReadOnlyList<string>)),
                ("ResolvedAuthorityUnitId", typeof(string))
            ],
            properties);

        var methods = typeof(DynamicGraphEdgeOrderQualityMeter)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        var method = Assert.Single(methods);
        Assert.Equal("Measure", method.Name);
        Assert.Equal(
            typeof(OperationResult<DynamicGraphEdgeOrderQualityMeasurement>),
            method.ReturnType);
        var parameter = Assert.Single(method.GetParameters());
        Assert.Equal(typeof(EdgeAuthorityExecutionResult), parameter.ParameterType);

        var forbidden = new[] { "Novelty", "Ablation", "SourceId", "Trained", "Promotion", "Lifecycle", "QualityGate" };
        var publicMemberNames = typeof(DynamicGraphEdgeOrderQualityMeasurement)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member.MemberType is MemberTypes.Property or MemberTypes.Field)
            .Select(member => member.Name)
            .Concat(typeof(DynamicGraphEdgeOrderQualityMeter)
                .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(member => member.MemberType is MemberTypes.Property or MemberTypes.Field)
                .Select(member => member.Name));
        Assert.DoesNotContain(
            publicMemberNames,
            name => forbidden.Any(term => name.Contains(term, StringComparison.Ordinal)));
    }

    [Fact]
    public void Measure_CanonicalThreeCandidateResultReturnsOwnedReadOnlyEvidence()
    {
        var result = DynamicGraphEdgeOrderQualityMeter.Measure(CreateResult());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var measurement = result.Value!;
        Assert.Equal(EdgeId, measurement.EdgeId);
        Assert.Equal(UnitId, measurement.ResolvedAuthorityUnitId);
        Assert.Equal(5, measurement.PermutationProbeCount);
        Assert.True(measurement.PermutationInvariant);
        Assert.Equal(
            Enumerable.Range(1, 5).Select(index => $"candidate-order-permutation-{index:D2}"),
            measurement.ProbeIds);
        Assert.Equal(LeakageSignals, measurement.ExcludedLeakageSignals);

        var firstProbeIds = measurement.ProbeIds;
        var secondProbeIds = measurement.ProbeIds;
        var firstExclusions = measurement.ExcludedLeakageSignals;
        var secondExclusions = measurement.ExcludedLeakageSignals;
        Assert.NotSame(firstProbeIds, secondProbeIds);
        Assert.NotSame(firstExclusions, secondExclusions);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)firstProbeIds)[0] = "forged-probe");
        Assert.Throws<NotSupportedException>(() => ((IList<string>)firstExclusions)[0] = "forged-exclusion");
        Assert.Equal("candidate-order-permutation-01", secondProbeIds[0]);
        Assert.Equal("candidate_index", secondExclusions[0]);
    }

    [Fact]
    public void Measure_NullReturnsExactStableFailure()
        => AssertInvalid(DynamicGraphEdgeOrderQualityMeter.Measure(null));

    [Fact]
    public void Measure_NonFatalCorruptedOwnedCarrierReturnsExactStableFailure()
    {
        var result = CreateResult();
        var normalizedInputField = Assert.IsAssignableFrom<FieldInfo>(
            typeof(EdgeAuthorityExecutionResult).GetField(
                "_normalizedInput",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        Assert.True(normalizedInputField.IsInitOnly);
        normalizedInputField.SetValue(result, null);

        AssertInvalid(DynamicGraphEdgeOrderQualityMeter.Measure(result));
    }

    [Fact]
    public void Measure_UsesSingleSharedClassifierExceptionFilterRatherThanBroadCatch()
    {
        var measure = Assert.IsAssignableFrom<MethodInfo>(
            typeof(DynamicGraphEdgeOrderQualityMeter).GetMethod(
                nameof(DynamicGraphEdgeOrderQualityMeter.Measure),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        var body = Assert.IsAssignableFrom<MethodBody>(measure.GetMethodBody());
        var clause = Assert.Single(body.ExceptionHandlingClauses);
        Assert.Equal(ExceptionHandlingClauseOptions.Filter, clause.Flags);
        Assert.True(clause.FilterOffset >= 0);
        Assert.True(clause.FilterOffset < clause.HandlerOffset);

        var il = Assert.IsType<byte[]>(body.GetILAsByteArray());
        var calls = ReadIlInstructions(il, clause.FilterOffset, clause.HandlerOffset)
            .Where(instruction => instruction.OpCode == OpCodes.Call
                || instruction.OpCode == OpCodes.Callvirt)
            .Select(instruction => measure.Module.ResolveMethod(
                instruction.MetadataToken!.Value,
                measure.DeclaringType?.GetGenericArguments(),
                measure.GetGenericArguments()))
            .ToArray();
        var classifier = Assert.IsAssignableFrom<MethodInfo>(
            typeof(SnapshotExceptionGraphClassifier).GetMethod(
                nameof(SnapshotExceptionGraphClassifier.IsNonFatal),
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: [typeof(Exception)],
                modifiers: null));

        var filterCall = Assert.Single(calls);
        Assert.Equal(classifier, filterCall);
        Assert.Equal(typeof(bool), classifier.ReturnType);
    }

    [Theory]
    [InlineData("input-report-edge-mismatch")]
    [InlineData("report-edge-case-mismatch")]
    [InlineData("blank-authority")]
    [InlineData("control-authority")]
    [InlineData("false-invariant")]
    [InlineData("missing-exclusion")]
    [InlineData("reordered-exclusions")]
    [InlineData("extra-exclusion")]
    [InlineData("normalized-input-boundary-mismatch")]
    [InlineData("four-probes")]
    [InlineData("six-probes")]
    [InlineData("wrong-probe-id")]
    [InlineData("duplicate-probe-id")]
    [InlineData("probe-id-case-mismatch")]
    [InlineData("swapped-candidate")]
    [InlineData("omitted-candidate")]
    [InlineData("mutated-candidate-target")]
    [InlineData("candidate-target-case-mismatch")]
    [InlineData("mutated-candidate-evidence")]
    [InlineData("candidate-evidence-case-mismatch")]
    [InlineData("mutated-candidate-score")]
    [InlineData("mutated-candidate-role")]
    [InlineData("candidate-role-case-mismatch")]
    [InlineData("probe-decision-mismatch")]
    [InlineData("probe-decision-target-id-mismatch")]
    [InlineData("probe-decision-fan-out-mismatch")]
    [InlineData("probe-decision-knowledge-edge-mismatch")]
    [InlineData("probe-decision-authority-source-mismatch")]
    [InlineData("probe-decision-ablation-rank-mismatch")]
    [InlineData("execution-decision-mismatch")]
    [InlineData("normalized-proposed-mismatch")]
    public void Measure_RejectsEachForgedBoundaryWithSameStableFailure(string mutation)
        => AssertInvalid(DynamicGraphEdgeOrderQualityMeter.Measure(CreateResult(mutation)));

    private static EdgeAuthorityExecutionResult CreateResult(string mutation = "none")
    {
        var normalizedCandidates = CreateCandidates();
        var proposedCandidates = normalizedCandidates.Select(CopyCandidate).ToArray();
        var inputEdgeId = EdgeId;
        var reportEdgeId = EdgeId;
        var resolvedAuthorityUnitId = UnitId;
        var isOrderInvariant = true;
        var inputExclusions = LeakageSignals.ToArray();
        var reportExclusions = LeakageSignals.ToArray();
        var canonicalDecision = CreateDecision(normalizedCandidates);
        var executionDecision = CopyDecision(canonicalDecision);
        var probes = Enumerable.Range(0, 5)
            .Select(index => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                CreateProbeId(index),
                CreatePermutation(normalizedCandidates, index),
                canonicalDecision))
            .ToList();

        switch (mutation)
        {
            case "none":
                break;
            case "input-report-edge-mismatch":
                reportEdgeId = "different-edge";
                break;
            case "report-edge-case-mismatch":
                reportEdgeId = EdgeId.ToUpperInvariant();
                break;
            case "blank-authority":
                resolvedAuthorityUnitId = " ";
                break;
            case "control-authority":
                resolvedAuthorityUnitId = "learned\u0001authority";
                break;
            case "false-invariant":
                isOrderInvariant = false;
                break;
            case "missing-exclusion":
                reportExclusions = reportExclusions.SkipLast(1).ToArray();
                break;
            case "reordered-exclusions":
                reportExclusions = reportExclusions.Reverse().ToArray();
                break;
            case "extra-exclusion":
                reportExclusions = [.. reportExclusions, "extra_signal"];
                break;
            case "normalized-input-boundary-mismatch":
                inputExclusions = inputExclusions.SkipLast(1).ToArray();
                break;
            case "four-probes":
                probes.RemoveAt(probes.Count - 1);
                break;
            case "six-probes":
                probes.Add(new DynamicGraphEdgeCandidateOrderPermutationProbe(
                    "candidate-order-permutation-06",
                    CreatePermutation(normalizedCandidates, 0),
                    canonicalDecision));
                break;
            case "wrong-probe-id":
                probes[0] = CopyProbe(probes[0], probeId: "wrong-probe-id");
                break;
            case "duplicate-probe-id":
                probes[1] = CopyProbe(probes[1], probeId: probes[0].ProbeId);
                break;
            case "probe-id-case-mismatch":
                probes[0] = CopyProbe(probes[0], probeId: probes[0].ProbeId.ToUpperInvariant());
                break;
            case "swapped-candidate":
                probes[0] = CopyProbe(
                    probes[0],
                    candidateOrder: [probes[0].CandidateOrder[1], probes[0].CandidateOrder[0], probes[0].CandidateOrder[2]]);
                break;
            case "omitted-candidate":
                probes[0] = CopyProbe(probes[0], candidateOrder: probes[0].CandidateOrder.SkipLast(1).ToArray());
                break;
            case "mutated-candidate-target":
                probes[0] = MutateFirstCandidate(
                    probes[0],
                    candidate => candidate with { TargetSubPhaseId = "forged-target" });
                break;
            case "candidate-target-case-mismatch":
                probes[0] = MutateFirstCandidate(
                    probes[0],
                    candidate => candidate with
                    {
                        TargetSubPhaseId = candidate.TargetSubPhaseId.ToUpperInvariant()
                    });
                break;
            case "mutated-candidate-evidence":
                probes[0] = MutateFirstCandidate(
                    probes[0],
                    candidate => candidate with { EvidencePredicates = ["FORGED_PREDICATE"] });
                break;
            case "candidate-evidence-case-mismatch":
                probes[0] = MutateFirstCandidate(
                    probes[0],
                    candidate => candidate with
                    {
                        EvidencePredicates = candidate.EvidencePredicates
                            .Select(predicate => predicate.ToLowerInvariant())
                            .ToArray()
                    });
                break;
            case "mutated-candidate-score":
                probes[0] = MutateFirstCandidate(
                    probes[0],
                    candidate => candidate with { BaselineScore = candidate.BaselineScore + 0.01d });
                break;
            case "mutated-candidate-role":
                probes[0] = MutateFirstCandidate(
                    probes[0],
                    candidate => candidate with { AuthorityRole = "forged-role" });
                break;
            case "candidate-role-case-mismatch":
                probes[0] = MutateFirstCandidate(
                    probes[0],
                    candidate => candidate with
                    {
                        AuthorityRole = candidate.AuthorityRole.ToUpperInvariant()
                    });
                break;
            case "probe-decision-mismatch":
                probes[0] = CopyProbe(
                    probes[0],
                    decision: canonicalDecision with
                    {
                        OrderedTargets = [("target-c", 0.92d)]
                    });
                break;
            case "probe-decision-target-id-mismatch":
                probes[0] = CopyProbe(
                    probes[0],
                    decision: canonicalDecision with
                    {
                        OrderedTargets = [("target-a", 0.93d)]
                    });
                break;
            case "probe-decision-fan-out-mismatch":
                probes[0] = CopyProbe(
                    probes[0],
                    decision: canonicalDecision with
                    {
                        FanOutCount = canonicalDecision.FanOutCount + 1
                    });
                break;
            case "probe-decision-knowledge-edge-mismatch":
                probes[0] = CopyProbe(
                    probes[0],
                    decision: canonicalDecision with
                    {
                        KnowledgeEdge = !canonicalDecision.KnowledgeEdge
                    });
                break;
            case "probe-decision-authority-source-mismatch":
                probes[0] = CopyProbe(
                    probes[0],
                    decision: canonicalDecision with
                    {
                        AuthoritySource = RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole
                    });
                break;
            case "probe-decision-ablation-rank-mismatch":
                probes[0] = CopyProbe(
                    probes[0],
                    decision: canonicalDecision with
                    {
                        AblationBaselineRank = canonicalDecision.AblationBaselineRank.Reverse().ToArray()
                    });
                break;
            case "execution-decision-mismatch":
                executionDecision = canonicalDecision with
                {
                    OrderedTargets = [("target-c", 0.92d)]
                };
                break;
            case "normalized-proposed-mismatch":
                proposedCandidates[0] = proposedCandidates[0] with
                {
                    BaselineScore = proposedCandidates[0].BaselineScore + 0.01d
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mutation));
        }

        var normalizedInput = new EdgeAuthorityInput(
            inputEdgeId,
            ["edge", "order", "quality"],
            normalizedCandidates.SelectMany(candidate => candidate.EvidencePredicates).ToArray(),
            new ContextSignals(
                [Domain.CODE_Q],
                ["completed-sub-phase"],
                ["active-edge"],
                new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.8d }),
            normalizedCandidates,
            inputExclusions);
        var report = new DynamicGraphEdgeCandidateOrderLeakageReport(
            reportEdgeId,
            resolvedAuthorityUnitId,
            isOrderInvariant,
            probes,
            reportExclusions);
        var identityReport = new DynamicGraphEdgeIdentityLeakageReport(
            inputEdgeId,
            UnitId,
            true,
            Enumerable.Range(0, 3)
                .Select(index => new DynamicGraphEdgeIdentityControlProbe(
                    IdentityProbeIds[index],
                    IdentityControlledSignals[index],
                    IdentityControlValues[index],
                    canonicalDecision))
                .ToArray(),
            LeakageSignals);
        return new EdgeAuthorityExecutionResult(
            normalizedInput,
            proposedCandidates,
            executionDecision,
            new DynamicGraphEdgeAuthorityEvidence(
                inputEdgeId,
                UnitId,
                "L2",
                false,
                "order-quality-baseline-unit",
                "candidate_generator",
                LeakageSignals,
                "order-quality-source-proof",
                "order-quality-cutover-proof",
                "order-quality-ablation-proof"),
            report,
            identityReport,
            0.93d,
            ["evidence"],
            []);
    }

    private static EdgeCandidate[] CreateCandidates()
        => Enumerable.Range(0, 3)
            .Select(index => new EdgeCandidate(
                $"target-{(char)('a' + index)}",
                [$"PREDICATE_{(char)('A' + index)}"],
                0.2d + (index * 0.2d),
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole))
            .ToArray();

    private static EdgeAuthorityDecision CreateDecision(IReadOnlyList<EdgeCandidate> candidates)
        => new(
            [("target-c", 0.93d)],
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
            var rotation = -offset - 1;
            return RotateLeft(reversed, rotation);
        }

        return RotateLeft(source, offset);
    }

    private static EdgeCandidate[] RotateLeft(IReadOnlyList<EdgeCandidate> source, int offset)
        => Enumerable.Range(0, source.Count)
            .Select(index => CopyCandidate(source[(index + offset) % source.Count]))
            .ToArray();

    private static string CreateProbeId(int index)
        => $"candidate-order-permutation-{index + 1:D2}";

    private static DynamicGraphEdgeCandidateOrderPermutationProbe MutateFirstCandidate(
        DynamicGraphEdgeCandidateOrderPermutationProbe probe,
        Func<EdgeCandidate, EdgeCandidate> mutation)
    {
        var candidates = probe.CandidateOrder.Select(CopyCandidate).ToArray();
        candidates[0] = mutation(candidates[0]);
        return CopyProbe(probe, candidateOrder: candidates);
    }

    private static DynamicGraphEdgeCandidateOrderPermutationProbe CopyProbe(
        DynamicGraphEdgeCandidateOrderPermutationProbe source,
        string? probeId = null,
        IReadOnlyList<EdgeCandidate>? candidateOrder = null,
        EdgeAuthorityDecision? decision = null)
        => new(
            probeId ?? source.ProbeId,
            candidateOrder ?? source.CandidateOrder,
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

    private static void AssertInvalid(OperationResult<DynamicGraphEdgeOrderQualityMeasurement> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(InvalidCode, result.ErrorCode);
        Assert.Equal(InvalidMessage, result.ErrorMessage);
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
            var opCode = ReadOpCode(il, ref offset, endOffset);
            var operandOffset = offset;
            var operandSize = GetOperandSize(opCode, il, operandOffset, endOffset);
            int? metadataToken = null;
            if (opCode.OperandType == OperandType.InlineMethod)
                metadataToken = ReadInt32(il, operandOffset, endOffset);

            offset = checked(offset + operandSize);
            if (offset > endOffset)
                throw new InvalidOperationException("IL operand extends beyond the exception-filter boundary.");

            instructions.Add(new IlInstruction(opCode, metadataToken));
        }

        if (offset != endOffset)
            throw new InvalidOperationException("IL decoder did not finish on the exception-filter boundary.");

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

    private readonly record struct IlInstruction(OpCode OpCode, int? MetadataToken);
}
