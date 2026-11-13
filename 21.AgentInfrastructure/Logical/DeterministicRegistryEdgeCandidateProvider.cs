// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using AgentInfrastructure;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

// This provider turns the registered sub-phase corpus into deterministic raw-score edge candidates.
public sealed class DeterministicRegistryEdgeCandidateProvider : IEdgeCandidateProvider
{
    public const string Id = "edge-candidate-provider-deterministic-registry-v1";
    public const string BaselineRole = "candidate_generator";

    private const string CandidateAuthorityRole = RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole;
    private readonly ILogicalTopologyRegistry? _registry;

    public DeterministicRegistryEdgeCandidateProvider(ILogicalTopologyRegistry? registry)
    {
        _registry = registry;
    }

    // Propose validates and snapshots every caller-owned boundary before running the shared raw scorer.
    public OperationResult<EdgeCandidate[]> Propose(IDynamicGraphEdge edge, EdgeTraversalInput input)
    {
        if (_registry is null)
            return Failure("Topology registry is required.", "EDGE_CANDIDATE_PROVIDER_DEPENDENCY_REQUIRED");

        if (edge is null)
            return Failure("Dynamic graph edge is required.", "EDGE_CANDIDATE_EDGE_REQUIRED");

        if (input is null)
            return Failure("Edge traversal input is required.", "EDGE_CANDIDATE_TRAVERSAL_REQUIRED");

        try
        {
            return ProposeCore(_registry, edge, input);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return Failure("Edge candidate proposal failed.", "EDGE_CANDIDATE_PROPOSAL_FAILED");
        }
    }

    private static OperationResult<EdgeCandidate[]> ProposeCore(
        ILogicalTopologyRegistry registry,
        IDynamicGraphEdge edge,
        EdgeTraversalInput input)
    {
        var source = input.Source;
        if (source is null)
            return Failure("Source document is required.", "EDGE_CANDIDATE_SOURCE_REQUIRED");

        var context = input.Context;
        if (context is null)
            return Failure("Context signals are required.", "EDGE_CANDIDATE_CONTEXT_REQUIRED");

        var upstream = input.UpstreamNodeOutput;
        if (upstream is null)
            return Failure("Upstream node output is required.", "EDGE_CANDIDATE_UPSTREAM_REQUIRED");

        var edgeId = edge.EdgeId;
        var traversalEdgeId = input.EdgeId;
        if (!IsCanonicalValue(edgeId) || !IsCanonicalValue(traversalEdgeId))
            return Failure("Edge identifiers must be canonical.", "EDGE_CANDIDATE_EDGE_ID_INVALID");

        if (!StringComparer.Ordinal.Equals(edgeId, traversalEdgeId))
            return Failure("Edge identifiers do not match.", "EDGE_CANDIDATE_EDGE_ID_MISMATCH");

        var sourceNodeId = edge.FromNodeId;
        var upstreamSourceNodeId = upstream.SourceNodeId;
        if (!IsCanonicalValue(sourceNodeId) || !IsCanonicalValue(upstreamSourceNodeId))
            return Failure("Source node identifiers must be canonical.", "EDGE_CANDIDATE_SOURCE_NODE_INVALID");

        if (!StringComparer.Ordinal.Equals(sourceNodeId, upstreamSourceNodeId))
            return Failure("Source node identifiers do not match.", "EDGE_CANDIDATE_SOURCE_NODE_MISMATCH");

        var baselineUnitId = edge.BaselineUnitId;
        if (!IsCanonicalValue(baselineUnitId))
            return Failure("Baseline unit id must be canonical.", "EDGE_CANDIDATE_BASELINE_ID_INVALID");

        if (!StringComparer.Ordinal.Equals(baselineUnitId, Id))
            return Failure("Baseline unit id does not identify this provider.", "EDGE_CANDIDATE_BASELINE_ID_MISMATCH");

        var baselineRole = edge.BaselineRole;
        if (!IsCanonicalValue(baselineRole) || !StringComparer.Ordinal.Equals(baselineRole, BaselineRole))
            return Failure("Baseline role is invalid.", "EDGE_CANDIDATE_BASELINE_ROLE_INVALID");

        var targets = SnapshotTargets(edge.ToNodeIds);
        if (!targets.IsSuccess)
            return targets.MapError<EdgeCandidate[]>();

        var contextSnapshot = SnapshotContext(context);
        if (!contextSnapshot.IsSuccess)
            return contextSnapshot.MapError<EdgeCandidate[]>();

        if (source.SegmentDocuments is { Length: > 1 })
        {
            return Failure(
                "Segment aggregation is not defined for edge candidate proposal.",
                "EDGE_CANDIDATE_SEGMENT_AGGREGATION_UNDEFINED");
        }

        OperationResult<IReadOnlyList<LogicalSubPhaseEntry>>? listed;
        try
        {
            listed = registry.ListSubPhases();
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return Failure("Topology registry lookup failed.", "EDGE_CANDIDATE_REGISTRY_FAILED");
        }

        if (listed is null)
            return Failure("Topology registry result is invalid.", "EDGE_CANDIDATE_REGISTRY_RESULT_INVALID");

        if (!listed.IsSuccess)
            return Failure("Topology registry lookup failed.", "EDGE_CANDIDATE_REGISTRY_FAILED");

        if (listed.Value is null)
            return Failure("Topology registry result is invalid.", "EDGE_CANDIDATE_REGISTRY_RESULT_INVALID");

        OperationResult<LogicalSubPhaseEntry[]> registrySnapshot;
        try
        {
            registrySnapshot = SnapshotRegistryEntries(listed.Value);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return Failure("Topology registry result is invalid.", "EDGE_CANDIDATE_REGISTRY_RESULT_INVALID");
        }

        if (!registrySnapshot.IsSuccess)
            return registrySnapshot.MapError<EdgeCandidate[]>();

        var entries = registrySnapshot.Value!;
        if (entries.Length == 0)
            return Failure("No sub-phases are registered.", "EDGE_CANDIDATE_NO_SUBPHASES");

        var registeredIds = entries
            .Select(entry => entry.SubPhase.SubPhaseId)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var target in targets.Value!)
        {
            if (!registeredIds.Contains(target))
                return Failure("An edge target is not registered.", "EDGE_CANDIDATE_TARGET_NOT_REGISTERED");
        }

        SubPhaseBaselineScore[] scored;
        try
        {
            // The shared scorer receives the complete registry corpus before target filtering.
            scored = DeterministicSubPhaseBaselineScorer.ScoreRelevant(source, entries, contextSnapshot.Value!);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return Failure("Edge candidate proposal failed.", "EDGE_CANDIDATE_PROPOSAL_FAILED");
        }

        var allowedTargets = targets.Value!.ToHashSet(StringComparer.Ordinal);
        var candidates = scored
            .Where(candidate => allowedTargets.Contains(candidate.Entry.SubPhase.SubPhaseId))
            .Select(candidate => new EdgeCandidate(
                candidate.Entry.SubPhase.SubPhaseId,
                candidate.EvidencePredicates.ToArray(),
                candidate.Score,
                CandidateAuthorityRole))
            .OrderBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
            .ToArray();

        if (candidates.Length == 0)
            return Failure("No relevant edge candidates were found.", "EDGE_CANDIDATE_NO_MATCH");

        return OperationResult<EdgeCandidate[]>.Ok(candidates);
    }

    private static OperationResult<string[]> SnapshotTargets(IReadOnlyList<string>? targets)
    {
        if (targets is null)
            return OperationResult<string[]>.Fail("Edge targets are invalid.", "EDGE_CANDIDATE_TARGETS_INVALID");

        string[] snapshot;
        try
        {
            var expectedCount = targets.Count;
            if (expectedCount == 0)
                return OperationResult<string[]>.Fail("Edge targets are invalid.", "EDGE_CANDIDATE_TARGETS_INVALID");

            snapshot = targets.ToArray();
            if (snapshot.Length != expectedCount)
                return OperationResult<string[]>.Fail("Edge targets are invalid.", "EDGE_CANDIDATE_TARGETS_INVALID");
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return OperationResult<string[]>.Fail("Edge targets are invalid.", "EDGE_CANDIDATE_TARGETS_INVALID");
        }

        if (snapshot.Any(target => !IsCanonicalValue(target))
            || snapshot.Distinct(StringComparer.Ordinal).Count() != snapshot.Length)
        {
            return OperationResult<string[]>.Fail("Edge targets are invalid.", "EDGE_CANDIDATE_TARGETS_INVALID");
        }

        return OperationResult<string[]>.Ok(snapshot);
    }

    private static OperationResult<ContextSignals> SnapshotContext(ContextSignals context)
    {
        try
        {
            if (context.PriorDomains is null
                || context.CompletedSubPhases is null
                || context.ActiveKnowledgeEdges is null
                || context.PriorConfidenceMap is null)
            {
                return ContextInvalid();
            }

            var priorDomains = context.PriorDomains.ToArray();
            if (priorDomains.Any(domain => !Enum.IsDefined(typeof(Domain), domain)))
                return ContextInvalid();

            priorDomains = priorDomains
                .Distinct()
                .OrderBy(domain => (int)domain)
                .ToArray();

            var completed = SnapshotCanonicalContextValues(context.CompletedSubPhases);
            if (completed is null)
                return ContextInvalid();

            var activeEdges = SnapshotCanonicalContextValues(context.ActiveKnowledgeEdges);
            if (activeEdges is null)
                return ContextInvalid();

            var expectedConfidenceCount = context.PriorConfidenceMap.Count;
            var confidence = new SortedDictionary<Domain, double>();
            var enumeratedConfidenceCount = 0;
            foreach (var pair in context.PriorConfidenceMap)
            {
                enumeratedConfidenceCount++;
                if (!Enum.IsDefined(typeof(Domain), pair.Key)
                    || !double.IsFinite(pair.Value)
                    || !confidence.TryAdd(pair.Key, pair.Value))
                {
                    return ContextInvalid();
                }
            }

            if (enumeratedConfidenceCount != expectedConfidenceCount || confidence.Count != expectedConfidenceCount)
                return ContextInvalid();

            var readOnlyConfidence = new ReadOnlyDictionary<Domain, double>(confidence);
            return OperationResult<ContextSignals>.Ok(new ContextSignals(
                priorDomains,
                completed,
                activeEdges,
                readOnlyConfidence));
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return ContextInvalid();
        }
    }

    private static string[]? SnapshotCanonicalContextValues(string[] values)
    {
        var snapshot = values.ToArray();
        if (snapshot.Any(value => !IsCanonicalValue(value)))
            return null;

        return snapshot
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
    }

    private static OperationResult<LogicalSubPhaseEntry[]> SnapshotRegistryEntries(
        IReadOnlyList<LogicalSubPhaseEntry> entries)
    {
        var expectedCount = entries.Count;
        var snapshot = new List<LogicalSubPhaseEntry>(expectedCount);
        var identifiers = new HashSet<string>(StringComparer.Ordinal);

        foreach (var entry in entries)
        {
            if (entry is null || entry.SubPhase is null)
                return RegistrySubPhaseInvalid();

            var subPhase = entry.SubPhase;
            var subPhaseId = subPhase.SubPhaseId;
            var phaseId = subPhase.PhaseId;
            var name = subPhase.Name;
            var purpose = subPhase.Purpose;
            var domainIds = subPhase.DomainIds?.ToArray();
            var predicateIds = subPhase.PredicateIds?.ToArray();
            var sourceIds = (subPhase.SourceIds ?? []).ToArray();
            var revision = entry.Revision;
            var updatedAtUtc = entry.UpdatedAtUtc;

            if (!IsCanonicalValue(subPhaseId)
                || name is null
                || purpose is null
                || domainIds is null
                || predicateIds is null
                || domainIds.Any(value => value is null)
                || predicateIds.Any(value => value is null))
            {
                return RegistrySubPhaseInvalid();
            }

            if (!identifiers.Add(subPhaseId))
            {
                return OperationResult<LogicalSubPhaseEntry[]>.Fail(
                    "Topology registry contains duplicate sub-phase identifiers.",
                    "EDGE_CANDIDATE_REGISTRY_SUBPHASE_DUPLICATED");
            }

            var clonedSubPhase = new LogicalSubPhase(
                subPhaseId,
                phaseId,
                name,
                purpose,
                domainIds,
                predicateIds,
                sourceIds);
            snapshot.Add(new LogicalSubPhaseEntry(clonedSubPhase, revision, updatedAtUtc));
        }

        if (snapshot.Count != expectedCount)
        {
            return OperationResult<LogicalSubPhaseEntry[]>.Fail(
                "Topology registry result is invalid.",
                "EDGE_CANDIDATE_REGISTRY_RESULT_INVALID");
        }

        return OperationResult<LogicalSubPhaseEntry[]>.Ok(snapshot.ToArray());
    }

    private static bool IsCanonicalValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim());

    private static bool IsNonFatalException(Exception exception)
        => exception is not OperationCanceledException
           && exception is not OutOfMemoryException
           && exception is not StackOverflowException
           && exception is not AccessViolationException
           && exception is not AppDomainUnloadedException
           && !StringComparer.Ordinal.Equals(exception.GetType().FullName, "System.Threading.ThreadAbortException");

    private static OperationResult<ContextSignals> ContextInvalid()
        => OperationResult<ContextSignals>.Fail("Context signals are invalid.", "EDGE_CANDIDATE_CONTEXT_INVALID");

    private static OperationResult<LogicalSubPhaseEntry[]> RegistrySubPhaseInvalid()
        => OperationResult<LogicalSubPhaseEntry[]>.Fail(
            "Topology registry contains an invalid sub-phase.",
            "EDGE_CANDIDATE_REGISTRY_SUBPHASE_INVALID");

    private static OperationResult<EdgeCandidate[]> Failure(string message, string code)
        => OperationResult<EdgeCandidate[]>.Fail(message, code);
}
