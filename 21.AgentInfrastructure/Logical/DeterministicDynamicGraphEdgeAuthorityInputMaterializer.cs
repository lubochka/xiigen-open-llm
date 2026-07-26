// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using AgentInfrastructure;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

// Creates one canonical edge-authority input without executing, resolving, or mutating graph state.
public sealed class DeterministicDynamicGraphEdgeAuthorityInputMaterializer
    : IDynamicGraphEdgeAuthorityInputMaterializer
{
    private const string ExpectedProjectionRule =
        "GraphNodeCandidate.Id -> UpstreamCandidateRefs (order-free set)";
    private const string ExpectedCandidateRole = RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole;

    private static readonly char[] TokenSeparators =
        [' ', '\t', '\r', '\n', '-', '_', '.', ',', ':', ';', '/', '\\', '(', ')', '[', ']', '{', '}'];

    public OperationResult<EdgeAuthorityInput> Materialize(
        IDynamicGraphEdge edge,
        EdgeTraversalInput input,
        IReadOnlyList<EdgeCandidate> candidates)
    {
        if (edge is null)
            return Failure("Dynamic graph edge is required.", "EDGE_AUTHORITY_EDGE_REQUIRED");

        if (input is null)
            return Failure("Edge traversal input is required.", "EDGE_AUTHORITY_TRAVERSAL_REQUIRED");

        if (input.Source is null)
            return Failure("Edge authority source is required.", "EDGE_AUTHORITY_SOURCE_REQUIRED");

        if (input.Context is null)
            return Failure("Edge authority context is required.", "EDGE_AUTHORITY_CONTEXT_REQUIRED");

        if (input.UpstreamNodeOutput is null)
            return Failure("Upstream node output is required.", "EDGE_AUTHORITY_UPSTREAM_REQUIRED");

        if (candidates is null)
            return Failure("Edge authority candidates are required.", "EDGE_AUTHORITY_CANDIDATES_REQUIRED");

        if (!IsCanonicalValue(edge.EdgeId) || !IsCanonicalValue(input.EdgeId))
            return Failure("Edge authority edge id is invalid.", "EDGE_AUTHORITY_EDGE_ID_INVALID");

        if (!StringComparer.Ordinal.Equals(edge.EdgeId, input.EdgeId))
            return Failure("Edge authority edge ids do not match.", "EDGE_AUTHORITY_EDGE_ID_MISMATCH");

        if (!IsCanonicalValue(edge.FromNodeId)
            || !IsCanonicalValue(input.UpstreamNodeOutput.SourceNodeId))
        {
            return Failure(
                "Edge authority source node identity is invalid.",
                "EDGE_AUTHORITY_SOURCE_NODE_INVALID");
        }

        if (!StringComparer.Ordinal.Equals(
                edge.FromNodeId,
                input.UpstreamNodeOutput.SourceNodeId))
        {
            return Failure(
                "Edge authority source node identities do not match.",
                "EDGE_AUTHORITY_SOURCE_NODE_MISMATCH");
        }

        var upstreamValidation = ValidateUpstream(input.UpstreamNodeOutput);
        if (!upstreamValidation.IsSuccess)
            return upstreamValidation.MapError<EdgeAuthorityInput>();

        var targetsValidation = SnapshotTargets(edge.ToNodeIds);
        if (!targetsValidation.IsSuccess)
            return targetsValidation.MapError<EdgeAuthorityInput>();

        var allowedTargets = targetsValidation.Value!;
        var candidateSnapshot = candidates.ToArray();
        if (candidateSnapshot.Length == 0)
            return Failure("Edge authority candidates are required.", "EDGE_AUTHORITY_CANDIDATES_REQUIRED");

        if (candidateSnapshot.Length > allowedTargets.Count)
        {
            return Failure(
                "Edge authority candidate count exceeds the target count.",
                "EDGE_AUTHORITY_CANDIDATE_INVALID");
        }

        if (!DynamicGraphEdgeAuthorityLeakageBoundary.IsExact(edge.ExcludedLeakageSignals))
        {
            return Failure(
                "Edge authority leakage boundary is invalid.",
                "EDGE_AUTHORITY_LEAKAGE_BOUNDARY_INVALID");
        }

        var compiledPredicateSnapshot = input.Source.CompiledPredicateIds?.ToArray() ?? [];
        var semanticTokens = BuildSemanticTokens(
            input.Source.DocumentTitle,
            input.Source.DocumentType,
            input.Source.CompiledText,
            compiledPredicateSnapshot);
        var predicateIds = NormalizePredicateIds(compiledPredicateSnapshot);

        if (semanticTokens.Length == 0 && predicateIds.Length == 0)
        {
            return Failure(
                "Edge authority semantic evidence is required.",
                "EDGE_AUTHORITY_SEMANTIC_EVIDENCE_REQUIRED");
        }

        var contextSnapshot = SnapshotContext(input.Context);
        if (!contextSnapshot.IsSuccess)
            return contextSnapshot.MapError<EdgeAuthorityInput>();

        var candidateValidation = SnapshotCandidates(
            candidateSnapshot,
            allowedTargets,
            predicateIds);
        if (!candidateValidation.IsSuccess)
            return candidateValidation.MapError<EdgeAuthorityInput>();

        return OperationResult<EdgeAuthorityInput>.Ok(
            new EdgeAuthorityInput(
                edge.EdgeId,
                semanticTokens,
                predicateIds,
                contextSnapshot.Value!,
                Array.AsReadOnly(candidateValidation.Value!),
                DynamicGraphEdgeAuthorityLeakageBoundary.CreateSnapshot()));
    }

    private static OperationResult<bool> ValidateUpstream(NodeOutputToEdgeInputAdapter upstream)
    {
        if (upstream.UpstreamCandidateRefs is null
            || upstream.UpstreamCandidateRefs.Count == 0)
        {
            return OperationResult<bool>.Fail(
                "Upstream candidate references are invalid.",
                "EDGE_AUTHORITY_UPSTREAM_REFS_INVALID");
        }

        var refs = upstream.UpstreamCandidateRefs.ToArray();
        if (refs.Any(string.IsNullOrWhiteSpace))
        {
            return OperationResult<bool>.Fail(
                "Upstream candidate references are invalid.",
                "EDGE_AUTHORITY_UPSTREAM_REFS_INVALID");
        }

        for (var index = 0; index < refs.Length; index++)
        {
            if (!StringComparer.Ordinal.Equals(refs[index], refs[index].Trim()))
            {
                return OperationResult<bool>.Fail(
                    "Upstream candidate references are not canonical.",
                    "EDGE_AUTHORITY_UPSTREAM_REFS_NOT_CANONICAL");
            }

            if (index > 0
                && StringComparer.Ordinal.Compare(refs[index - 1], refs[index]) >= 0)
            {
                return OperationResult<bool>.Fail(
                    "Upstream candidate references are not canonical.",
                    "EDGE_AUTHORITY_UPSTREAM_REFS_NOT_CANONICAL");
            }
        }

        if (!IsCanonicalValue(upstream.UpstreamOutputHash))
        {
            return OperationResult<bool>.Fail(
                "Upstream output hash is invalid.",
                "EDGE_AUTHORITY_UPSTREAM_HASH_INVALID");
        }

        if (!StringComparer.Ordinal.Equals(upstream.ProjectionRule, ExpectedProjectionRule))
        {
            return OperationResult<bool>.Fail(
                "Upstream projection rule is invalid.",
                "EDGE_AUTHORITY_UPSTREAM_PROJECTION_INVALID");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<HashSet<string>> SnapshotTargets(
        IReadOnlyList<string> targetIds)
    {
        if (targetIds is null || targetIds.Count == 0)
        {
            return OperationResult<HashSet<string>>.Fail(
                "Edge authority targets are invalid.",
                "EDGE_AUTHORITY_TARGETS_INVALID");
        }

        var targetSnapshot = targetIds.ToArray();
        var allowedTargets = new HashSet<string>(StringComparer.Ordinal);
        foreach (var targetId in targetSnapshot)
        {
            if (!IsCanonicalValue(targetId) || !allowedTargets.Add(targetId))
            {
                return OperationResult<HashSet<string>>.Fail(
                    "Edge authority targets are invalid.",
                    "EDGE_AUTHORITY_TARGETS_INVALID");
            }
        }

        return OperationResult<HashSet<string>>.Ok(allowedTargets);
    }

    private static string[] BuildSemanticTokens(
        string documentTitle,
        string documentType,
        string compiledText,
        IReadOnlyList<string> compiledPredicateIds)
    {
        var tokens = new HashSet<string>(StringComparer.Ordinal);
        AddSemanticTokens(documentTitle, tokens);
        AddSemanticTokens(documentType, tokens);
        AddSemanticTokens(compiledText, tokens);

        foreach (var predicateId in compiledPredicateIds)
            AddSemanticTokens(predicateId, tokens);

        return tokens.OrderBy(static token => token, StringComparer.Ordinal).ToArray();
    }

    private static void AddSemanticTokens(string value, ISet<string> tokens)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        foreach (var token in value.Split(
                     TokenSeparators,
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var normalized = token.ToLowerInvariant();
            if (normalized.Length > 2)
                tokens.Add(normalized);
        }
    }

    private static string[] NormalizePredicateIds(IReadOnlyList<string> compiledPredicateIds)
        => compiledPredicateIds
            .Where(static predicateId => !string.IsNullOrWhiteSpace(predicateId))
            .Select(static predicateId => predicateId.Trim().ToUpperInvariant())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static predicateId => predicateId, StringComparer.Ordinal)
            .ToArray();

    private static OperationResult<ContextSignals> SnapshotContext(ContextSignals context)
    {
        if (context.PriorDomains is null
            || context.CompletedSubPhases is null
            || context.ActiveKnowledgeEdges is null
            || context.PriorConfidenceMap is null)
        {
            return ContextFailure();
        }

        var priorDomainSource = context.PriorDomains.ToArray();
        if (priorDomainSource.Any(static domain => !Enum.IsDefined(typeof(Domain), domain)))
            return ContextFailure();

        var priorDomains = priorDomainSource
            .Distinct()
            .OrderBy(static domain => (int)domain)
            .ToArray();

        var completedSubPhases = SnapshotCanonicalValues(context.CompletedSubPhases);
        if (completedSubPhases is null)
            return ContextFailure();

        var activeKnowledgeEdges = SnapshotCanonicalValues(context.ActiveKnowledgeEdges);
        if (activeKnowledgeEdges is null)
            return ContextFailure();

        var orderedConfidenceMap = new SortedDictionary<Domain, double>();
        foreach (var pair in context.PriorConfidenceMap.ToArray())
        {
            if (!Enum.IsDefined(typeof(Domain), pair.Key) || !double.IsFinite(pair.Value))
                return ContextFailure();

            orderedConfidenceMap.Add(pair.Key, pair.Value);
        }

        return OperationResult<ContextSignals>.Ok(
            new ContextSignals(
                priorDomains,
                completedSubPhases,
                activeKnowledgeEdges,
                new ReadOnlyDictionary<Domain, double>(orderedConfidenceMap)));
    }

    private static string[]? SnapshotCanonicalValues(IReadOnlyList<string> values)
    {
        var source = values.ToArray();
        if (source.Any(static value => !IsCanonicalValue(value)))
            return null;

        return source
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
    }

    private static OperationResult<EdgeCandidate[]> SnapshotCandidates(
        IReadOnlyList<EdgeCandidate> candidates,
        IReadOnlySet<string> allowedTargets,
        IReadOnlyList<string> predicateIds)
    {
        var normalizedPredicateIds = new HashSet<string>(predicateIds, StringComparer.Ordinal);
        var seenTargets = new HashSet<string>(StringComparer.Ordinal);
        var snapshots = new List<EdgeCandidate>(candidates.Count);

        foreach (var candidate in candidates)
        {
            if (candidate is null || !IsCanonicalValue(candidate.TargetSubPhaseId))
            {
                return OperationResult<EdgeCandidate[]>.Fail(
                    "Edge authority candidate is invalid.",
                    "EDGE_AUTHORITY_CANDIDATE_INVALID");
            }

            if (!allowedTargets.Contains(candidate.TargetSubPhaseId))
            {
                return OperationResult<EdgeCandidate[]>.Fail(
                    "Edge authority candidate target is not allowed.",
                    "EDGE_AUTHORITY_CANDIDATE_TARGET_NOT_ALLOWED");
            }

            if (!seenTargets.Add(candidate.TargetSubPhaseId))
            {
                return OperationResult<EdgeCandidate[]>.Fail(
                    "Edge authority candidate target is duplicated.",
                    "EDGE_AUTHORITY_CANDIDATE_TARGET_DUPLICATED");
            }

            if (!double.IsFinite(candidate.BaselineScore) || candidate.BaselineScore < 0d)
            {
                return OperationResult<EdgeCandidate[]>.Fail(
                    "Edge authority candidate baseline score is invalid.",
                    "EDGE_AUTHORITY_CANDIDATE_SCORE_INVALID");
            }

            if (!StringComparer.Ordinal.Equals(candidate.AuthorityRole, ExpectedCandidateRole))
            {
                return OperationResult<EdgeCandidate[]>.Fail(
                    "Edge authority candidate role is invalid.",
                    "EDGE_AUTHORITY_CANDIDATE_ROLE_INVALID");
            }

            if (candidate.EvidencePredicates is null)
            {
                return OperationResult<EdgeCandidate[]>.Fail(
                    "Edge authority candidate evidence is invalid.",
                    "EDGE_AUTHORITY_CANDIDATE_EVIDENCE_INVALID");
            }

            var normalizedEvidence = new HashSet<string>(StringComparer.Ordinal);
            foreach (var evidencePredicate in candidate.EvidencePredicates.ToArray())
            {
                if (string.IsNullOrWhiteSpace(evidencePredicate))
                {
                    return OperationResult<EdgeCandidate[]>.Fail(
                        "Edge authority candidate evidence is invalid.",
                        "EDGE_AUTHORITY_CANDIDATE_EVIDENCE_INVALID");
                }

                var normalized = evidencePredicate.Trim().ToUpperInvariant();
                if (!normalizedPredicateIds.Contains(normalized))
                {
                    return OperationResult<EdgeCandidate[]>.Fail(
                        "Edge authority candidate evidence is invalid.",
                        "EDGE_AUTHORITY_CANDIDATE_EVIDENCE_INVALID");
                }

                normalizedEvidence.Add(normalized);
            }

            snapshots.Add(
                new EdgeCandidate(
                    candidate.TargetSubPhaseId,
                    normalizedEvidence
                        .OrderBy(static predicate => predicate, StringComparer.Ordinal)
                        .ToArray(),
                    candidate.BaselineScore,
                    ExpectedCandidateRole));
        }

        return OperationResult<EdgeCandidate[]>.Ok(
            snapshots
                .OrderBy(static candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
                .ToArray());
    }

    private static bool IsCanonicalValue(string value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim());

    private static OperationResult<ContextSignals> ContextFailure()
        => OperationResult<ContextSignals>.Fail(
            "Edge authority context is invalid.",
            "EDGE_AUTHORITY_CONTEXT_INVALID");

    private static OperationResult<EdgeAuthorityInput> Failure(string message, string code)
        => OperationResult<EdgeAuthorityInput>.Fail(message, code);
}
