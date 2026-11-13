// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using AgentInfrastructure;

// This unit diagnoses candidate-order leakage without selecting or replacing the edge-authority decision.
public sealed class DynamicGraphEdgeCandidateOrderLeakageGuardUnit
    : ILogicalUnit<DynamicGraphEdgeCandidateOrderProbeInput, DynamicGraphEdgeCandidateOrderLeakageReport>
{
    public const string Id = "dynamic-graph-edge-candidate-order-leakage-guard";
    public const string Role = "edge-authority-candidate-order-diagnostics";
    public const string LeakageCode = "EDGE_AUTHORITY_CANDIDATE_ORDER_LEAKAGE";
    public const string GuardFailedCode = "EDGE_AUTHORITY_CANDIDATE_ORDER_GUARD_FAILED";
    public const string CancelledCode = "CANCELLED";

    private const int ProbeCount = 5;
    private const string ExpectedAuthorityRole = "edge-authority";

    private readonly LogicalUnitDescriptor? _descriptor;
    private readonly ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>? _resolvedAuthority;

    public DynamicGraphEdgeCandidateOrderLeakageGuardUnit(
        LogicalUnitDescriptor? descriptor,
        ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>? resolvedAuthority)
    {
        _descriptor = descriptor;
        _resolvedAuthority = resolvedAuthority;
    }

    public async Task<OperationResult<LogicalUnitResult<DynamicGraphEdgeCandidateOrderLeakageReport>>> ExecuteAsync(
        LogicalUnitRequest<DynamicGraphEdgeCandidateOrderProbeInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        EdgeAuthorityInput normalizedInput;
        EdgeAuthorityDecision referenceDecision;
        LogicalUnitDescriptor descriptor;
        ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision> resolvedAuthority;
        try
        {
            if (request is null
                || request.Input is null
                || request.Flags is null
                || request.Metadata is null
                || !IsCanonicalValue(request.RunId)
                || !StringComparer.Ordinal.Equals(request.UnitId, Id)
                || !StringComparer.Ordinal.Equals(request.UnitRole, Role)
                || !IsCanonicalValue(request.DomainTag)
                || !TrySnapshotDescriptor(_descriptor, request.DomainTag, out descriptor)
                || _resolvedAuthority is not { } authority
                || !TrySnapshotInput(request.Input, out normalizedInput, out referenceDecision))
            {
                return GuardFailed();
            }

            resolvedAuthority = authority;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return GuardFailed();
        }

        var probes = new List<DynamicGraphEdgeCandidateOrderPermutationProbe>(ProbeCount);
        for (var probeIndex = 0; probeIndex < ProbeCount; probeIndex++)
        {
            if (cancellationToken.IsCancellationRequested)
                return Cancelled();

            try
            {
                var candidateOrder = CreateExpectedPermutation(normalizedInput.Candidates, probeIndex);
                var probeInput = CopyInputWithCandidates(normalizedInput, candidateOrder);
                var authorityRequest = new LogicalUnitRequest<EdgeAuthorityInput>(
                    request.RunId,
                    descriptor.UnitId,
                    descriptor.UnitRole,
                    descriptor.DomainTag,
                    probeInput,
                    request.Flags with { },
                    new ReadOnlyDictionary<string, object>(
                        new Dictionary<string, object>(StringComparer.Ordinal)));

                var executed = await resolvedAuthority
                    .ExecuteAsync(authorityRequest, cancellationToken)
                    .ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                    return Cancelled();

                if (!TrySnapshotProbeDecision(
                        executed,
                        descriptor,
                        candidateOrder,
                        out var probeDecision))
                {
                    return GuardFailed();
                }

                if (!DecisionsEqual(referenceDecision, probeDecision))
                    return LeakageDetected();

                probes.Add(new DynamicGraphEdgeCandidateOrderPermutationProbe(
                    CreateProbeId(probeIndex),
                    candidateOrder,
                    probeDecision));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Cancelled();
            }
            catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return GuardFailed();
            }
        }

        try
        {
            var report = new DynamicGraphEdgeCandidateOrderLeakageReport(
                normalizedInput.EdgeId,
                descriptor.UnitId,
                IsOrderInvariant: true,
                Array.AsReadOnly(probes.ToArray()),
                DynamicGraphEdgeAuthorityLeakageBoundary.CreateSnapshot());
            var result = new LogicalUnitResult<DynamicGraphEdgeCandidateOrderLeakageReport>(
                Id,
                Role,
                request.DomainTag,
                report,
                1d,
                normalizedInput.PredicateIds.ToArray(),
                [request.DomainTag],
                ["EDGE_AUTHORITY_CANDIDATE_ORDER_INVARIANT"],
                []);
            return OperationResult<LogicalUnitResult<DynamicGraphEdgeCandidateOrderLeakageReport>>.Ok(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return GuardFailed();
        }
    }

    private static bool TrySnapshotInput(
        DynamicGraphEdgeCandidateOrderProbeInput source,
        out EdgeAuthorityInput normalizedInput,
        out EdgeAuthorityDecision referenceDecision)
    {
        normalizedInput = null!;
        referenceDecision = null!;

        var input = source.NormalizedInput;
        if (!DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotNormalizedInput(
                input,
                input.EdgeId,
                input.Candidates,
                out normalizedInput))
        {
            return false;
        }

        var candidateTargets = normalizedInput.Candidates
            .Select(candidate => candidate.TargetSubPhaseId)
            .ToArray();
        return DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotDecision(
            source.ReferenceDecision,
            candidateTargets,
            normalizedInput.Candidates,
            out referenceDecision);
    }

    private static bool TrySnapshotDescriptor(
        LogicalUnitDescriptor? source,
        string expectedDomainTag,
        out LogicalUnitDescriptor descriptor)
    {
        descriptor = null!;
        if (source is null
            || !IsCanonicalValue(source.UnitId)
            || !StringComparer.Ordinal.Equals(source.UnitRole, ExpectedAuthorityRole)
            || !StringComparer.Ordinal.Equals(source.DomainTag, expectedDomainTag)
            || source.InputType != typeof(EdgeAuthorityInput)
            || source.OutputType != typeof(EdgeAuthorityDecision)
            || !source.IsLearned
            || !Enum.IsDefined(source.TrustState)
            || source.TrustState is LogicalUnitTrustState.Candidate or LogicalUnitTrustState.Rejected)
        {
            return false;
        }

        descriptor = source with { };
        return true;
    }

    private static bool TrySnapshotProbeDecision(
        OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>? executed,
        LogicalUnitDescriptor descriptor,
        IReadOnlyList<EdgeCandidate> candidateOrder,
        out EdgeAuthorityDecision decision)
    {
        decision = null!;
        if (executed is null
            || !executed.IsSuccess
            || executed.Value is null
            || !DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotUnitEnvelope(
                executed.Value,
                descriptor,
                out _,
                out _,
                out _,
                out var decisionSource))
        {
            return false;
        }

        var targetIds = candidateOrder
            .Select(candidate => candidate.TargetSubPhaseId)
            .ToArray();
        return DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotDecision(
            decisionSource,
            targetIds,
            candidateOrder,
            out decision);
    }

    // Counts one and two necessarily repeat orders; larger sets use five distinct, position-moving controls.
    internal static EdgeCandidate[] CreateExpectedPermutation(
        IReadOnlyList<EdgeCandidate> candidates,
        int permutationIndex)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        if (candidates.Count == 0 || permutationIndex is < 0 or >= ProbeCount)
            throw new ArgumentOutOfRangeException(nameof(permutationIndex));

        var source = candidates
            .Select(DynamicGraphEdgeAuthorityContractSnapshots.CopyCandidate)
            .ToArray();
        var offset = source.Length switch
        {
            1 => 0,
            2 => permutationIndex % 2,
            3 => permutationIndex switch
            {
                0 => 0,
                1 => 1,
                2 => 2,
                3 => -1,
                _ => -2
            },
            4 when permutationIndex == 4 => -1,
            _ => permutationIndex
        };

        if (offset < 0)
        {
            var reversed = source.Reverse().ToArray();
            var rotation = (-offset - 1) % reversed.Length;
            return RotateLeft(reversed, rotation);
        }

        return RotateLeft(source, offset % source.Length);
    }

    internal static string CreateProbeId(int probeIndex)
    {
        if (probeIndex is < 0 or >= ProbeCount)
            throw new ArgumentOutOfRangeException(nameof(probeIndex));

        return $"candidate-order-permutation-{probeIndex + 1:D2}";
    }

    internal static bool DecisionsEqual(EdgeAuthorityDecision expected, EdgeAuthorityDecision actual)
    {
        if (expected.FanOutCount != actual.FanOutCount
            || expected.KnowledgeEdge != actual.KnowledgeEdge
            || !StringComparer.Ordinal.Equals(expected.AuthoritySource, actual.AuthoritySource)
            || expected.OrderedTargets.Count != actual.OrderedTargets.Count
            || expected.AblationBaselineRank.Count != actual.AblationBaselineRank.Count)
        {
            return false;
        }

        for (var index = 0; index < expected.OrderedTargets.Count; index++)
        {
            if (!StringComparer.Ordinal.Equals(
                    expected.OrderedTargets[index].TargetSubPhaseId,
                    actual.OrderedTargets[index].TargetSubPhaseId)
                || !expected.OrderedTargets[index].Confidence.Equals(
                    actual.OrderedTargets[index].Confidence))
            {
                return false;
            }
        }

        return expected.AblationBaselineRank.SequenceEqual(
            actual.AblationBaselineRank,
            StringComparer.Ordinal);
    }

    private static EdgeCandidate[] RotateLeft(IReadOnlyList<EdgeCandidate> source, int offset)
        => Enumerable.Range(0, source.Count)
            .Select(index => DynamicGraphEdgeAuthorityContractSnapshots.CopyCandidate(
                source[(index + offset) % source.Count]))
            .ToArray();

    private static EdgeAuthorityInput CopyInputWithCandidates(
        EdgeAuthorityInput source,
        IReadOnlyList<EdgeCandidate> candidates)
        => new(
            source.EdgeId,
            source.SemanticTokens.ToArray(),
            source.PredicateIds.ToArray(),
            DynamicGraphEdgeAuthorityContractSnapshots.CopyInput(source).Context,
            Array.AsReadOnly(
                candidates
                    .Select(DynamicGraphEdgeAuthorityContractSnapshots.CopyCandidate)
                    .ToArray()),
            source.ExcludedLeakageSignals.ToArray());

    private static bool IsCanonicalValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && value.IndexOfAny(['\r', '\n', '\u0085', '\u2028', '\u2029']) < 0;

    private static OperationResult<LogicalUnitResult<DynamicGraphEdgeCandidateOrderLeakageReport>> Cancelled()
        => Failure("Cancelled.", CancelledCode);

    private static OperationResult<LogicalUnitResult<DynamicGraphEdgeCandidateOrderLeakageReport>> GuardFailed()
        => Failure("Edge authority candidate-order guard failed.", GuardFailedCode);

    private static OperationResult<LogicalUnitResult<DynamicGraphEdgeCandidateOrderLeakageReport>> LeakageDetected()
        => Failure("Edge authority candidate-order leakage detected.", LeakageCode);

    private static OperationResult<LogicalUnitResult<DynamicGraphEdgeCandidateOrderLeakageReport>> Failure(
        string message,
        string code)
        => OperationResult<LogicalUnitResult<DynamicGraphEdgeCandidateOrderLeakageReport>>.Fail(message, code);
}
