// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;
using SemanticCompilerAdapter;

// This unit makes deterministic routing decisions on the existing product path.
// Its output reports deterministic baseline provenance and never trained authority.
public sealed class MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
{
    // This id identifies the first multi-sub-phase router.
    public const string Id = "unit-multi-sub-phase-router-v1";

    // This role names the fan-out routing responsibility.
    public const string Role = "multi-sub-phase-router";

    // This domain tag scopes the unit to whole-development-area orchestration.
    public const string DomainTag = "whole_development_area";

    // The topology registry provides the current adaptive sub-phase catalog.
    private readonly ILogicalTopologyRegistry? _registry;

    // The single router provides a fallback when no multi-route evidence is strong enough.
    private readonly ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>? _singleRouter;

    // Create the multi-router from a topology registry and optional single-route fallback.
    public MultiSubPhaseRouterUnit(
        ILogicalTopologyRegistry? registry,
        ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>? singleRouter = null)
    {
        // Store the topology registry for candidate lookup.
        _registry = registry;

        // Store or create the single-route fallback.
        _singleRouter = singleRouter ?? (registry is null ? null : new SubPhaseRouterUnit(registry));
    }

    // Execute one multi-sub-phase routing pass.
    public async Task<OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>> ExecuteAsync(
        LogicalUnitRequest<SubPhaseRouterSourceDocument> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail("Cancelled.", "CANCELLED");

        // Reject invalid construction before reading topology.
        if (_registry is null)
            return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail("Topology registry is required.", "TOPOLOGY_REGISTRY_REQUIRED");

        // Reject missing input before scoring.
        if (request.Input is null)
            return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail("Source document is required.", "SOURCE_DOCUMENT_REQUIRED");

        // Read all candidate sub-phases.
        var subPhases = _registry.ListSubPhases();
        if (!subPhases.IsSuccess)
            return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail(subPhases.ErrorMessage, subPhases.ErrorCode);

        // Reject empty topology because there is no target to emit.
        if (subPhases.Value!.Count == 0)
            return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail("No sub-phases are registered.", "NO_SUB_PHASES_REGISTERED");

        // E21 prompt segments are compiled by the canonical semantic compiler. Route them independently.
        if (request.Input.SegmentDocuments is { Length: > 1 } segmentDocuments)
        {
            var segmentDecisions = new List<SubPhaseRoutingDecision>();
            foreach (var segmentDocument in segmentDocuments)
            {
                var segmentResult = await ExecuteAsync(new LogicalUnitRequest<SubPhaseRouterSourceDocument>(
                    request.RunId,
                    request.UnitId,
                    request.UnitRole,
                    request.DomainTag,
                    segmentDocument,
                    request.Flags,
                    request.Metadata), cancellationToken);
                if (segmentResult.IsSuccess)
                    segmentDecisions.AddRange(segmentResult.Value!.Output);
            }

            if (segmentDecisions.Count > 0)
            {
                var merged = segmentDecisions
                    .GroupBy(decision => decision.PredictedSubPhaseId, StringComparer.Ordinal)
                    .Select(group => group.OrderByDescending(decision => decision.Confidence).First())
                    .OrderByDescending(decision => decision.Confidence)
                    .ThenBy(decision => decision.PredictedSubPhaseId, StringComparer.Ordinal)
                    .ToArray();
                var evidence = merged.SelectMany(decision => decision.EvidencePredicates).Distinct(StringComparer.Ordinal).OrderBy(predicate => predicate, StringComparer.Ordinal).ToArray();
                var segmentedLogicalResult = new LogicalUnitResult<SubPhaseRoutingDecision[]>(
                    request.UnitId,
                    request.UnitRole,
                    request.DomainTag,
                    merged,
                    merged.Min(decision => decision.Confidence),
                    evidence,
                    merged.Select(decision => decision.PredictedSubPhaseId).ToArray(),
                    segmentDocuments.Select(segment => segment.DocumentId).ToArray(),
                    ["semantic_prompt_segments_routed_independently"]);
                return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Ok(segmentedLogicalResult);
            }
        }

        // Reuse the original raw baseline calculation and immediately restore the router-local carrier shape.
        var scored = DeterministicSubPhaseBaselineScorer
            .ScoreRelevant(request.Input, subPhases.Value, ContextPacketReader.Read(request.Metadata))
            .Select(candidate => new CandidateScore(
                candidate.Entry,
                candidate.Score,
                candidate.EvidencePredicates.ToArray(),
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole))
            .ToArray();

        // If no multi-route evidence exists, delegate to the single router.
        if (scored.Length == 0)
        {
            // The fallback must exist when registry construction was valid.
            if (_singleRouter is null)
                return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail("Single sub-phase router is required.", "SINGLE_ROUTER_REQUIRED");

            // Execute the single router.
            var single = await _singleRouter.ExecuteAsync(new LogicalUnitRequest<SubPhaseRouterSourceDocument>(
                request.RunId,
                SubPhaseRouterUnit.Id,
                SubPhaseRouterUnit.Role,
                SubPhaseRouterUnit.DomainTag,
                request.Input,
                request.Flags,
                request.Metadata), cancellationToken);

            // Preserve single-router failures.
            if (!single.IsSuccess)
                return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail(single.ErrorMessage, single.ErrorCode);

            // Validate and own the successful dependency result before topology lookup or conversion.
            var singleSnapshot = SnapshotSingleRouterResult(request.Input, single.Value);
            if (!singleSnapshot.IsSuccess)
            {
                return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail(
                    singleSnapshot.ErrorMessage,
                    singleSnapshot.ErrorCode);
            }

            var ownedSingle = singleSnapshot.Value!;

            // Resolve the single fallback id against the topology before converting it into a routed candidate.
            var fallbackEntry = FindSubPhase(subPhases.Value, ownedSingle.Output.PredictedSubPhaseId);

            // Return an explicit failure when the fallback id is not registered in the active topology.
            if (fallbackEntry is null)
                return BuildMissingFallbackSubPhaseFailure(ownedSingle);

            // The registered target must also preserve the phase identity asserted by the single router.
            if (!StringComparer.Ordinal.Equals(
                    fallbackEntry.SubPhase.PhaseId,
                    ownedSingle.Output.PredictedPhaseId))
            {
                return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail(
                    "Single sub-phase router result identity does not match the request.",
                    "SINGLE_ROUTER_RESULT_IDENTITY_MISMATCH");
            }

            // Convert the registered single decision into the multi-result shape.
            scored =
            [
                new CandidateScore(
                    fallbackEntry,
                    ownedSingle.Output.Confidence,
                    ownedSingle.Output.EvidencePredicates.ToArray(),
                    ownedSingle.Output.AuthorityRole)
            ];
        }

        // Build the output decisions.
        var decisions = scored.Select((candidate, index) =>
        {
            // Convert score to confidence, preserving the first fallback confidence if it was already confidence-shaped.
            var confidence = candidate.Score > 1.0 ? Math.Min(0.97, candidate.Score) : Math.Round(Math.Min(0.97, 0.70 + (candidate.Score * 0.27)), 2);

            // Build one routing decision.
            return new SubPhaseRoutingDecision(
                request.Input.DocumentId.Trim(),
                request.Input.SourceStatus.Trim(),
                candidate.Entry.SubPhase.PhaseId,
                candidate.Entry.SubPhase.SubPhaseId,
                confidence,
                index == 0 && scored.Length > 1 ? Math.Round(Math.Min(0.49, scored[1].Score * 0.49), 2) : 0.0,
                candidate.EvidencePredicates,
                confidence < 0.70,
                confidence < 0.70 ? "knowledge_edge: sub-phase evidence below confidence floor" : "source_classified_by_multi_sub_phase_router")
            {
                AuthorityRole = candidate.AuthorityRole
            };
        }).ToArray();

        // Aggregate predicate evidence.
        var evidencePredicates = decisions.SelectMany(decision => decision.EvidencePredicates).Distinct(StringComparer.Ordinal).OrderBy(predicate => predicate, StringComparer.Ordinal).ToArray();

        // Build warnings that keep source path visible as provenance but not as scoring evidence.
        var warnings = BuildSourcePathWarnings(request.Input, subPhases.Value, decisions)
            // Add source-status warnings after the path-scoring firewall evidence.
            .Concat(string.Equals(request.Input.SourceStatus, "ready", StringComparison.Ordinal) ? [] : new[] { "source_status_not_ready" })
            // Keep warning order deterministic for tests and reports.
            .ToArray();

        // Build provenance ids without feeding the source path into scoring.
        var evidenceIds = BuildEvidenceIds(request.Input);

        // Wrap the decisions in the standard logical-unit result.
        var result = new LogicalUnitResult<SubPhaseRoutingDecision[]>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            decisions,
            decisions.Min(decision => decision.Confidence),
            evidencePredicates,
            decisions.Select(decision => decision.PredictedSubPhaseId).ToArray(),
            evidenceIds,
            warnings);

        // Return the routing result.
        return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Ok(result);
    }

    // Build warnings that expose whether the path firewall was active.
    private static string[] BuildSourcePathWarnings(
        SubPhaseRouterSourceDocument source,
        IReadOnlyList<LogicalSubPhaseEntry> candidates,
        IReadOnlyList<SubPhaseRoutingDecision> semanticDecisions)
    {
        // Start with the invariant evidence that SourcePath did not participate in scoring.
        var warnings = new List<string> { "source_path_used_for_scoring=false" };

        // Tokenize the path separately for warning-only conflict detection.
        var pathTokens = DeterministicSubPhaseBaselineScorer.Tokenize(source.SourcePath);

        // No path tokens means there is no path-only signal to warn about.
        if (pathTokens.Count == 0 || semanticDecisions.Count == 0)
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

        // Read the semantic winner from the already computed route decisions.
        var semanticBestId = semanticDecisions[0].PredictedSubPhaseId;

        // Add a warning when path-only evidence points away from semantic evidence.
        if (pathBest is not null && pathBest.Entry.SubPhase.SubPhaseId != semanticBestId)
            warnings.Add($"source_path_semantic_conflict:{pathBest.Entry.SubPhase.SubPhaseId}->{semanticBestId}");

        // Return warning-only path evidence.
        return warnings.ToArray();
    }

    // Score a candidate against path tokens only for warning evidence.
    private static double ScorePathOnlyCandidate(LogicalSubPhaseEntry entry, HashSet<string> pathTokens)
    {
        // Tokenize candidate identity and purpose just like text evidence.
        var candidateTokens = DeterministicSubPhaseBaselineScorer.Tokenize($"{entry.SubPhase.SubPhaseId} {entry.SubPhase.Name} {entry.SubPhase.Purpose}");

        // A candidate with no tokens has no path-only score.
        if (candidateTokens.Count == 0)
            return 0.0;

        // Return the share of candidate tokens that appeared in the source path.
        return (double)candidateTokens.Count(pathTokens.Contains) / candidateTokens.Count;
    }

    // Build provenance ids while keeping SourcePath out of scoring features.
    private static string[] BuildEvidenceIds(SubPhaseRouterSourceDocument source)
    {
        // Keep the source document id as the primary provenance marker.
        var evidenceIds = new List<string> { source.DocumentId };

        // Preserve the input source path as provenance only when it exists.
        if (!string.IsNullOrWhiteSpace(source.SourcePath))
            evidenceIds.Add($"source_path_provenance:{source.SourcePath.Trim()}");

        // Return deterministic evidence ids.
        return evidenceIds.ToArray();
    }

    // Build a failure result when the single fallback points to an unknown topology id.
    private static OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>> BuildMissingFallbackSubPhaseFailure(
        LogicalUnitResult<SubPhaseRoutingDecision> single)
    {
        // Normalize the missing id for safe diagnostic text.
        var missingSubPhaseId = SafeDiagnosticText(single.Output.PredictedSubPhaseId);

        // Build a single-line failure message that preserves the unknown id.
        var message = $"Single-router fallback id '{missingSubPhaseId}' is not registered in active topology.";

        // Return the explicit topology-consistency failure through the normal operation envelope.
        return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail(message, "FALLBACK_SUBPHASE_ID_NOT_REGISTERED");
    }

    // Validate and deep-snapshot an injected successful single-router result before any downstream use.
    private static OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>> SnapshotSingleRouterResult(
        SubPhaseRouterSourceDocument source,
        LogicalUnitResult<SubPhaseRoutingDecision>? result)
    {
        // A successful operation with no carrier is malformed.
        if (result is null)
            return BuildInvalidSingleRouterResultFailure();

        LogicalUnitResult<SubPhaseRoutingDecision> ownedResult;
        try
        {
            // Snapshot every mutable dependency carrier before validating or using any of its contents.
            var decision = result.Output;
            var decisionEvidence = decision?.EvidencePredicates?.ToArray();
            var predicateIds = result.PredicateIds?.ToArray();
            var domainTags = result.DomainTags?.ToArray();
            var evidenceIds = result.EvidenceIds?.ToArray();
            var warnings = result.Warnings?.ToArray();
            var transitionSignal = result.TransitionSignal;
            var transitionEvidence = transitionSignal?.EvidenceIds?.ToArray();

            if (decision is null
                || decisionEvidence is null
                || predicateIds is null
                || domainTags is null
                || evidenceIds is null
                || warnings is null
                || transitionSignal is not null && transitionEvidence is null)
            {
                return BuildInvalidSingleRouterResultFailure();
            }

            var snapshotDecision = new SubPhaseRoutingDecision(
                decision.DocumentId,
                decision.SourceStatus,
                decision.PredictedPhaseId,
                decision.PredictedSubPhaseId,
                decision.Confidence,
                decision.NearestWrongSubPhaseScore,
                decisionEvidence,
                decision.KnowledgeEdge,
                decision.FailureClassification)
            {
                AuthorityRole = decision.AuthorityRole
            };
            var snapshotTransitionSignal = transitionSignal is null
                ? null
                : new PhaseTransitionSignal(
                    transitionSignal.SuggestedDomain,
                    transitionSignal.TransitionPrompt,
                    transitionSignal.SourceNodeId,
                    transitionSignal.SourceSubPhaseId,
                    transitionEvidence!);
            ownedResult = new LogicalUnitResult<SubPhaseRoutingDecision>(
                result.UnitId,
                result.UnitRole,
                result.DomainTag,
                snapshotDecision,
                result.Confidence,
                predicateIds,
                domainTags,
                evidenceIds,
                warnings)
            {
                TransitionSignal = snapshotTransitionSignal
            };
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return BuildInvalidSingleRouterResultFailure();
        }

        // Validation below observes only the independently owned snapshot.
        var ownedDecision = ownedResult.Output;

        // Never normalize or launder learned, blank, or unknown authority through the baseline fallback.
        if (!StringComparer.Ordinal.Equals(
                ownedDecision.AuthorityRole,
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole))
        {
            return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>.Fail(
                $"Single sub-phase router authority role must be {RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole}.",
                "SINGLE_ROUTER_AUTHORITY_ROLE_INVALID");
        }

        // The dependency carrier and decision must belong to the exact request boundary.
        if (!StringComparer.Ordinal.Equals(ownedResult.UnitId, SubPhaseRouterUnit.Id)
            || !StringComparer.Ordinal.Equals(ownedResult.UnitRole, SubPhaseRouterUnit.Role)
            || !StringComparer.Ordinal.Equals(ownedResult.DomainTag, SubPhaseRouterUnit.DomainTag)
            || !StringComparer.Ordinal.Equals(ownedDecision.DocumentId, source.DocumentId?.Trim())
            || !StringComparer.Ordinal.Equals(ownedDecision.SourceStatus, source.SourceStatus?.Trim()))
        {
            return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>.Fail(
                "Single sub-phase router result identity does not match the request.",
                "SINGLE_ROUTER_RESULT_IDENTITY_MISMATCH");
        }

        // Reject malformed scalar and collection state instead of silently trimming or dropping it.
        if (!IsCanonicalSingleRouterValue(ownedDecision.DocumentId)
            || !IsCanonicalSingleRouterValue(ownedDecision.SourceStatus)
            || !IsCanonicalSingleRouterValue(ownedDecision.PredictedPhaseId)
            || !IsCanonicalSingleRouterValue(ownedDecision.PredictedSubPhaseId)
            || !IsCanonicalSingleRouterValue(ownedDecision.FailureClassification)
            || !double.IsFinite(ownedDecision.Confidence)
            || ownedDecision.Confidence is < 0d or > 1d
            || !double.IsFinite(ownedDecision.NearestWrongSubPhaseScore)
            || ownedDecision.NearestWrongSubPhaseScore is < 0d or > 1d
            || !double.IsFinite(ownedResult.Confidence)
            || ownedResult.Confidence is < 0d or > 1d
            || !ownedResult.Confidence.Equals(ownedDecision.Confidence)
            || !ownedDecision.EvidencePredicates.All(IsCanonicalSingleRouterValue)
            || !ownedResult.PredicateIds.All(IsCanonicalSingleRouterValue)
            || !ownedResult.DomainTags.All(IsCanonicalSingleRouterValue)
            || !ownedResult.EvidenceIds.All(IsCanonicalSingleRouterValue)
            || !ownedResult.Warnings.All(IsSafeSingleRouterText)
            || !ownedResult.PredicateIds.SequenceEqual(ownedDecision.EvidencePredicates, StringComparer.Ordinal))
        {
            return BuildInvalidSingleRouterResultFailure();
        }

        if (ownedResult.TransitionSignal is { } ownedTransitionSignal)
        {
            if (!Enum.IsDefined(typeof(SemanticCompiler29.Domain), ownedTransitionSignal.SuggestedDomain)
                || !IsCanonicalSingleRouterValue(ownedTransitionSignal.TransitionPrompt)
                || !IsCanonicalSingleRouterValue(ownedTransitionSignal.SourceNodeId)
                || !IsCanonicalSingleRouterValue(ownedTransitionSignal.SourceSubPhaseId)
                || !ownedTransitionSignal.EvidenceIds.All(IsCanonicalSingleRouterValue))
            {
                return BuildInvalidSingleRouterResultFailure();
            }
        }

        return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>.Ok(ownedResult);
    }

    // Return the stable malformed-success failure used by every non-identity validation branch.
    private static OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>> BuildInvalidSingleRouterResultFailure()
        => OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>.Fail(
            "Single sub-phase router result is invalid.",
            "SINGLE_ROUTER_RESULT_INVALID");

    // Require an already-canonical non-empty boundary value; callers are not normalized in place.
    private static bool IsCanonicalSingleRouterValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && value.All(character => !char.IsControl(character));

    // Warnings may be empty but must stay single-line and free of control characters.
    private static bool IsSafeSingleRouterText(string? value)
        => value is not null
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && value.All(character => !char.IsControl(character));

    // Find a sub-phase entry by id for fallback conversion.
    private static LogicalSubPhaseEntry? FindSubPhase(IReadOnlyList<LogicalSubPhaseEntry> entries, string subPhaseId)
    {
        // Return only an exact registered entry; missing ids stay unknown.
        return entries.FirstOrDefault(entry => entry.SubPhase.SubPhaseId == subPhaseId);
    }

    // Normalize diagnostic values so ids stay single-line and safe for reports.
    private static string SafeDiagnosticText(string value)
    {
        // Collapse missing or whitespace-only values to an explicit placeholder.
        if (string.IsNullOrWhiteSpace(value))
            return "<empty>";

        // Keep diagnostics single-line so they cannot masquerade as another evidence row.
        return value.Trim().Replace('\r', ' ').Replace('\n', ' ');
    }

    // This record stores one scored route candidate.
    private sealed record CandidateScore(
        LogicalSubPhaseEntry Entry,
        double Score,
        string[] EvidencePredicates,
        string AuthorityRole);
}
