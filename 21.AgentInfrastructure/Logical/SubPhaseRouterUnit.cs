// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// This record carries the compiled document evidence that the sub-phase router receives.
public sealed record SubPhaseRouterSourceDocument(
    string DocumentId,
    string SourcePath,
    string SourceTier,
    string LicenseOrAuthority,
    string DocumentType,
    string DocumentTitle,
    string CompiledText,
    string[] CompiledPredicateIds,
    string SourceStatus,
    string KnownFailureMode,
    string DomainLabel = "",
    SubPhaseRouterSourceDocument[]? SegmentDocuments = null);

// This record carries the sub-phase routing decision produced by the router unit.
public sealed record SubPhaseRoutingDecision(
    string DocumentId,
    string SourceStatus,
    string PredictedPhaseId,
    string PredictedSubPhaseId,
    double Confidence,
    double NearestWrongSubPhaseScore,
    string[] EvidencePredicates,
    bool KnowledgeEdge,
    string FailureClassification)
{
    // This vocabulary marks deterministic routing provenance, never trained edge authority.
    public const string BaselineAuthorityRole = RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole;

    // This vocabulary identifies a decision produced by admitted learned edge authority.
    public const string TrainedAuthorityRole = RoutingAuthorityVocabulary.TrainedEdgeUnitRole;

    // This key publishes the canonical routing authority role in metadata and result metrics.
    public const string AuthorityRoleMetadataKey = RoutingAuthorityVocabulary.MetadataKey;

    // Legacy local constructors and JSON payloads remain baseline-compatible only; production boundaries
    // must validate materialized roles and every producer must assign its canonical role explicitly.
    public string AuthorityRole { get; init; } = RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole;

    // Accept only the closed routing-authority vocabulary at external and materialized boundaries.
    public static bool IsCanonicalAuthorityRole(string? authorityRole)
        => RoutingAuthorityVocabulary.IsCanonicalRole(authorityRole);
}

// This logical unit routes one compiled source document to one registered development sub-phase.
public sealed class SubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>
{
    // This id identifies the first real WDA-01 sub-phase router in evidence and registries.
    public const string Id = "unit-sub-phase-router-v1";

    // This role names the lifecycle document classification responsibility.
    public const string Role = "sub-phase-router";

    // This domain tag scopes the unit to whole-development-area routing.
    public const string DomainTag = "whole_development_area";

    // This array contains the characters that split human text into routing tokens.
    private static readonly char[] TokenSeparators = [' ', '\t', '\r', '\n', '-', '_', '.', ',', ':', ';', '/', '\\', '(', ')', '[', ']', '{', '}'];

    // This set removes generic words that would otherwise make unrelated sub-phases look similar.
    private static readonly HashSet<string> StopWords = new(StringComparer.Ordinal)
    {
        // Drop connective words because they do not identify a sub-phase.
        "and",

        // Drop article words because they do not identify a sub-phase.
        "the",

        // Drop ownership boilerplate because every topology purpose uses it.
        "owns",

        // Drop generic lifecycle words because they appear across many entries.
        "phase",

        // Drop generic lifecycle words because they appear across many entries.
        "sub",

        // Drop generic lifecycle words because they appear across many entries.
        "source",

        // Drop generic lifecycle words because they appear across many entries.
        "document",

        // Drop generic lifecycle words because they appear across many entries.
        "development"
    };

    // The topology registry provides the current adaptive sub-phase catalog.
    private readonly ILogicalTopologyRegistry? _registry;

    // Create the router with the topology registry it reads from.
    public SubPhaseRouterUnit(ILogicalTopologyRegistry? registry)
    {
        // Store the registry so ExecuteAsync can classify against the active topology.
        _registry = registry;
    }

    // Execute one source-document classification through the shared logical-unit contract.
    public Task<OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>> ExecuteAsync(
        LogicalUnitRequest<SubPhaseRouterSourceDocument> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>.Fail("Cancelled.", "CANCELLED"));

        // Reject invalid construction before reading the topology.
        if (_registry is null)
            return Task.FromResult(OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>.Fail("Topology registry is required.", "TOPOLOGY_REGISTRY_REQUIRED"));

        // Reject missing requests because the router cannot infer source evidence.
        if (request.Input is null)
            return Task.FromResult(OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>.Fail("Source document is required.", "SOURCE_DOCUMENT_REQUIRED"));

        // Reject empty document ids because routing evidence must be traceable.
        if (string.IsNullOrWhiteSpace(request.Input.DocumentId))
            return Task.FromResult(OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>.Fail("Document id is required.", "EMPTY_DOCUMENT_ID"));

        // Read the active sub-phase catalog from the topology registry.
        var subPhases = _registry.ListSubPhases();

        // Return registry failures through the logical-unit envelope.
        if (!subPhases.IsSuccess)
            return Task.FromResult(OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>.Fail(subPhases.ErrorMessage, subPhases.ErrorCode));

        // Materialize the candidate list after the success check.
        var candidates = subPhases.Value!;

        // Reject empty topology because classification needs at least one target.
        if (candidates.Count == 0)
            return Task.FromResult(OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>.Fail("No sub-phases are registered.", "NO_SUB_PHASES_REGISTERED"));

        // Normalize the compiled predicate ids into the same shape used by topology entries.
        var inputPredicates = NormalizePredicates(request.Input.CompiledPredicateIds);

        // Build path-free scoring text so source locations cannot become routing features.
        var scoringText = BuildPathFreeScoringText(request.Input);

        // Tokenize the compiled source text, title, type, and predicate ids for text-level evidence.
        var inputTokens = Tokenize(scoringText);

        // Score every candidate sub-phase against the compiled source document.
        var ranked = candidates
            // Score one candidate using predicate overlap and source-text overlap.
            .Select(candidate => ScoreCandidate(candidate, inputPredicates, inputTokens))
            // Prefer the highest scoring sub-phase.
            .OrderByDescending(score => score.Score)
            // Keep deterministic ordering when two candidates score equally.
            .ThenBy(score => score.Entry.SubPhase.SubPhaseId, StringComparer.Ordinal)
            // Materialize the ranked list for top and nearest-wrong inspection.
            .ToArray();

        // Select the best candidate as the predicted sub-phase.
        var best = ranked[0];

        // Select the nearest wrong-domain candidate when one exists.
        var nearestWrong = ranked.Length > 1 ? ranked[1] : default;

        // Convert the best score into a confidence value above the WDA-01 floor for strong evidence.
        var confidence = CalculateConfidence(best.Score);

        // Convert the second-best score into the calibrated wrong-domain neighbor score.
        var wrongDomainScore = CalculateWrongDomainScore(nearestWrong?.Score ?? 0.0);

        // Treat weak matches as knowledge edges instead of pretending the router knows.
        var knowledgeEdge = confidence < 0.70;

        // Build the failure classification used by WDA-01 evidence.
        var failureClassification = knowledgeEdge
            // Name the gap when the router did not find enough evidence.
            ? "knowledge_edge: sub-phase evidence below confidence floor"
            // Name the positive routing path when evidence is sufficient.
            : "source_classified_by_sub_phase_router";

        // Build the routing decision that downstream WDA gates can inspect.
        var decision = new SubPhaseRoutingDecision(
            request.Input.DocumentId.Trim(),
            request.Input.SourceStatus.Trim(),
            best.Entry.SubPhase.PhaseId,
            best.Entry.SubPhase.SubPhaseId,
            confidence,
            wrongDomainScore,
            best.EvidencePredicates,
            knowledgeEdge,
            failureClassification)
        {
            AuthorityRole = RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole
        };

        // Build warnings that keep source path visible as provenance but not as scoring evidence.
        var warnings = BuildSourcePathWarnings(request.Input, candidates, best.Entry)
            // Add source-status warnings after the path-scoring firewall evidence.
            .Concat(string.Equals(request.Input.SourceStatus, "ready", StringComparison.Ordinal) ? [] : new[] { "source_status_not_ready" })
            // Keep warning order deterministic for tests and reports.
            .ToArray();

        // Build provenance ids without feeding the source path into scoring.
        var evidenceIds = BuildEvidenceIds(best.Entry.SubPhase.SourceIds, request.Input);

        // Wrap the decision in the standard logical-unit result.
        var result = new LogicalUnitResult<SubPhaseRoutingDecision>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            decision,
            confidence,
            best.EvidencePredicates,
            best.Entry.SubPhase.DomainIds,
            evidenceIds,
            warnings);

        // Return the routing result through the shared operation envelope.
        return Task.FromResult(OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>.Ok(result));
    }

    // Build the source text that is allowed to affect routing scores.
    private static string BuildPathFreeScoringText(SubPhaseRouterSourceDocument source)
    {
        // Join semantic document fields while deliberately excluding SourcePath.
        return string.Join(" ",
            source.DocumentTitle,
            source.DocumentType,
            source.CompiledText,
            string.Join(" ", source.CompiledPredicateIds ?? []));
    }

    // Build warnings that expose whether the path firewall was active.
    private static string[] BuildSourcePathWarnings(
        SubPhaseRouterSourceDocument source,
        IReadOnlyList<LogicalSubPhaseEntry> candidates,
        LogicalSubPhaseEntry semanticBest)
    {
        // Start with the invariant evidence that SourcePath did not participate in scoring.
        var warnings = new List<string> { "source_path_used_for_scoring=false" };

        // Tokenize the path separately for warning-only conflict detection.
        var pathTokens = Tokenize(source.SourcePath);

        // No path tokens means there is no path-only signal to warn about.
        if (pathTokens.Count == 0)
            return warnings.ToArray();

        // Find the strongest candidate implied by path tokens only.
        var pathBest = candidates
            // Score path vocabulary only for warning evidence.
            .Select(candidate => new
            {
                Entry = candidate,
                Score = ScorePathOnlyCandidate(candidate, pathTokens)
            })
            // Ignore candidates with no path-only match.
            .Where(candidate => candidate.Score > 0.0)
            // Prefer the strongest path-only candidate.
            .OrderByDescending(candidate => candidate.Score)
            // Keep deterministic warning output for equal path matches.
            .ThenBy(candidate => candidate.Entry.SubPhase.SubPhaseId, StringComparer.Ordinal)
            // Keep the best warning candidate.
            .FirstOrDefault();

        // Add a warning when path-only evidence points away from semantic evidence.
        if (pathBest is not null && pathBest.Entry.SubPhase.SubPhaseId != semanticBest.SubPhase.SubPhaseId)
            warnings.Add($"source_path_semantic_conflict:{pathBest.Entry.SubPhase.SubPhaseId}->{semanticBest.SubPhase.SubPhaseId}");

        // Return warning-only path evidence.
        return warnings.ToArray();
    }

    // Score a candidate against path tokens only for warning evidence.
    private static double ScorePathOnlyCandidate(LogicalSubPhaseEntry entry, HashSet<string> pathTokens)
    {
        // Tokenize candidate identity and purpose just like text evidence.
        var candidateTokens = Tokenize($"{entry.SubPhase.SubPhaseId} {entry.SubPhase.Name} {entry.SubPhase.Purpose}");

        // A candidate with no tokens has no path-only score.
        if (candidateTokens.Count == 0)
            return 0.0;

        // Return the share of candidate tokens that appeared in the source path.
        return (double)candidateTokens.Count(pathTokens.Contains) / candidateTokens.Count;
    }

    // Build provenance ids while keeping SourcePath out of scoring features.
    private static string[] BuildEvidenceIds(string[] sourceIds, SubPhaseRouterSourceDocument source)
    {
        // Keep existing topology source ids first.
        var evidenceIds = new List<string>(sourceIds);

        // Preserve the input source path as provenance only when it exists.
        if (!string.IsNullOrWhiteSpace(source.SourcePath))
            evidenceIds.Add($"source_path_provenance:{source.SourcePath.Trim()}");

        // Return deterministic evidence ids.
        return evidenceIds.ToArray();
    }

    // Score one candidate using predicate evidence and text evidence.
    private static CandidateScore ScoreCandidate(
        LogicalSubPhaseEntry entry,
        HashSet<string> inputPredicates,
        HashSet<string> inputTokens)
    {
        // Normalize candidate predicate ids for overlap comparison.
        var candidatePredicates = NormalizePredicates(entry.SubPhase.PredicateIds);

        // Count predicate ids that appear in both the input and the candidate.
        var predicateMatches = candidatePredicates.Count(inputPredicates.Contains);

        // Compute predicate score as the share of candidate predicates covered by the input.
        var predicateScore = candidatePredicates.Count == 0 ? 0.0 : (double)predicateMatches / candidatePredicates.Count;

        // Tokenize the candidate identity separately so sub-phase name evidence has its own score.
        var candidateIdentityTokens = Tokenize($"{entry.SubPhase.SubPhaseId} {entry.SubPhase.Name}");

        // Count identity tokens that appear in the input evidence.
        var identityMatches = candidateIdentityTokens.Count(inputTokens.Contains);

        // Compute identity score as the share of sub-phase identity tokens covered by the input.
        var identityScore = candidateIdentityTokens.Count == 0 ? 0.0 : (double)identityMatches / candidateIdentityTokens.Count;

        // Tokenize the candidate identity and purpose for vocabulary overlap.
        var candidateTokens = Tokenize($"{entry.SubPhase.SubPhaseId} {entry.SubPhase.Name} {entry.SubPhase.Purpose}");

        // Count candidate tokens that appear in the input evidence.
        var tokenMatches = candidateTokens.Count(inputTokens.Contains);

        // Compute text score as the share of candidate tokens covered by the input.
        var textScore = candidateTokens.Count == 0 ? 0.0 : (double)tokenMatches / candidateTokens.Count;

        // Blend predicate evidence, identity evidence, and text evidence so exact sub-phase wording can beat misleading generic predicates.
        var score = (predicateScore * 0.40) + (identityScore * 0.40) + (textScore * 0.20);

        // Preserve the matched predicates as explainable evidence for the output.
        var evidencePredicates = candidatePredicates.Where(inputPredicates.Contains).OrderBy(predicate => predicate, StringComparer.Ordinal).ToArray();

        // Return the full score object for ranking and output evidence.
        return new CandidateScore(entry, score, evidencePredicates);
    }

    // Convert a strong candidate score into WDA confidence.
    private static double CalculateConfidence(double score)
    {
        // Bound the confidence so exact matches do not exceed the trusted deterministic ceiling.
        return Math.Round(Math.Min(0.97, 0.70 + (score * 0.27)), 2);
    }

    // Convert the nearest wrong candidate into the calibrated wrong-domain score.
    private static double CalculateWrongDomainScore(double score)
    {
        // Keep wrong-domain score below the 0.50 confidence ceiling from E51A.
        return Math.Round(Math.Min(0.49, score * 0.49), 2);
    }

    // Normalize predicate ids to stable uppercase tokens.
    private static HashSet<string> NormalizePredicates(string[]? predicateIds)
    {
        // Treat null predicate arrays as empty evidence.
        var safePredicateIds = predicateIds ?? [];

        // Normalize every predicate and remove duplicates.
        return safePredicateIds
            // Trim whitespace around each predicate id.
            .Select(predicate => predicate.Trim())
            // Drop empty predicate ids because they cannot support routing.
            .Where(predicate => !string.IsNullOrWhiteSpace(predicate))
            // Normalize casing for stable comparison.
            .Select(predicate => predicate.ToUpperInvariant())
            // Materialize the set with ordinal comparison.
            .ToHashSet(StringComparer.Ordinal);
    }

    // Tokenize text into stable routing tokens.
    private static HashSet<string> Tokenize(string text)
    {
        // Treat null text as empty text.
        var safeText = text ?? string.Empty;

        // Split the text, normalize tokens, remove generic words, and materialize the set.
        return safeText
            // Split on path, punctuation, and whitespace separators.
            .Split(TokenSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            // Normalize casing for stable comparison.
            .Select(token => token.ToLowerInvariant())
            // Drop very short tokens because digits and fragments cause false matches.
            .Where(token => token.Length > 2)
            // Drop generic tokens that do not identify a sub-phase.
            .Where(token => !StopWords.Contains(token))
            // Materialize the set with ordinal comparison.
            .ToHashSet(StringComparer.Ordinal);
    }

    // This record stores one ranked candidate during classification.
    private sealed record CandidateScore(
        LogicalSubPhaseEntry Entry,
        double Score,
        string[] EvidencePredicates);
}
