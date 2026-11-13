// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using AgentInfrastructure;
using SemanticCompilerAdapter;

// This unit keeps deterministic routing as an honest baseline and optionally cuts a non-empty
// shortlist over to one explicitly supplied learned edge authority for the final decision.
public sealed class MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
{
    private const string LearnedAuthorityRole = "edge-authority";
    private const string LearnedAuthoritySource = RoutingAuthorityVocabulary.TrainedEdgeUnitRole;
    private const string LearnedAuthorityInvalidCode = "MULTI_ROUTER_LEARNED_AUTHORITY_INVALID";
    private const string LearnedAuthorityExecutionFailedCode = "MULTI_ROUTER_LEARNED_AUTHORITY_EXECUTION_FAILED";
    private const string LearnedAuthorityResultRequiredCode = "MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_REQUIRED";
    private const string LearnedAuthorityResultInvalidCode = "MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_INVALID";
    private const string RouterEdgeId = "edge-multi-sub-phase-router-v1";

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

    // The optional resolved entry is the only explicit cutover seam that may make a learned final decision.
    private readonly ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>? _learnedAuthority;

    // Create the multi-router from a topology registry and optional single-route fallback.
    public MultiSubPhaseRouterUnit(
        ILogicalTopologyRegistry? registry,
        ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>? singleRouter = null,
        ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>? learnedAuthority = null)
    {
        // Store the topology registry for candidate lookup.
        _registry = registry;

        // Store or create the single-route fallback.
        _singleRouter = singleRouter ?? (registry is null ? null : new SubPhaseRouterUnit(registry));

        // Store only the explicitly supplied resolved tuple; this router never auto-resolves or auto-wires one.
        _learnedAuthority = learnedAuthority;
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
            var segmentConfidences = new List<double>();
            var learnedRoutingEnabled = _learnedAuthority is not null
                && request.Flags.UseLearnedUnitWhenAvailable;
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
                if (!segmentResult.IsSuccess)
                {
                    // Once learned cutover is enabled, a failed segment invalidates the aggregate instead of disappearing.
                    if (learnedRoutingEnabled)
                    {
                        return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail(
                            segmentResult.ErrorMessage,
                            segmentResult.ErrorCode);
                    }

                    continue;
                }

                segmentConfidences.Add(segmentResult.Value!.Confidence);
                segmentDecisions.AddRange(segmentResult.Value.Output);
            }

            if (segmentDecisions.Count > 0)
            {
                var hasLearnedSegmentDecision = segmentDecisions.Any(decision =>
                    StringComparer.Ordinal.Equals(
                        decision.AuthorityRole,
                        LearnedAuthoritySource));
                SubPhaseRoutingDecision[] merged;
                if (hasLearnedSegmentDecision)
                {
                    var stableLearnedDecisions = new List<SubPhaseRoutingDecision>();
                    var learnedDecisionByTarget = new Dictionary<string, SubPhaseRoutingDecision>(
                        StringComparer.Ordinal);
                    foreach (var decision in segmentDecisions)
                    {
                        if (!learnedDecisionByTarget.TryGetValue(
                                decision.PredictedSubPhaseId,
                                out var retainedDecision))
                        {
                            learnedDecisionByTarget.Add(decision.PredictedSubPhaseId, decision);
                            stableLearnedDecisions.Add(decision);
                            continue;
                        }

                        if (!AreEquivalentLearnedSegmentDecisions(retainedDecision, decision))
                            return LearnedAuthorityResultInvalid();
                    }

                    merged = stableLearnedDecisions.ToArray();
                }
                else
                {
                    merged = segmentDecisions
                        .GroupBy(decision => decision.PredictedSubPhaseId, StringComparer.Ordinal)
                        .Select(group => group.OrderByDescending(decision => decision.Confidence).First())
                        .OrderByDescending(decision => decision.Confidence)
                        .ThenBy(decision => decision.PredictedSubPhaseId, StringComparer.Ordinal)
                        .ToArray();
                }

                var evidence = merged.SelectMany(decision => decision.EvidencePredicates).Distinct(StringComparer.Ordinal).OrderBy(predicate => predicate, StringComparer.Ordinal).ToArray();
                var segmentedWarnings = new List<string>
                {
                    "semantic_prompt_segments_routed_independently"
                };
                if (hasLearnedSegmentDecision)
                {
                    segmentedWarnings.Add("learned_edge_authority_final=true");
                }

                var segmentedLogicalResult = new LogicalUnitResult<SubPhaseRoutingDecision[]>(
                    request.UnitId,
                    request.UnitRole,
                    request.DomainTag,
                    merged,
                    merged.Min(decision => decision.Confidence),
                    evidence,
                    merged.Select(decision => decision.PredictedSubPhaseId).ToArray(),
                    segmentDocuments.Select(segment => segment.DocumentId).ToArray(),
                    segmentedWarnings.ToArray());
                return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Ok(segmentedLogicalResult);
            }

            // An all-empty learned knowledge-edge aggregate is an honest terminal route carrier.
            if (learnedRoutingEnabled && segmentConfidences.Count == segmentDocuments.Length)
            {
                var emptySegmentedResult = new LogicalUnitResult<SubPhaseRoutingDecision[]>(
                    request.UnitId,
                    request.UnitRole,
                    request.DomainTag,
                    [],
                    segmentConfidences.Min(),
                    [],
                    [],
                    segmentDocuments.Select(segment => segment.DocumentId).ToArray(),
                    [
                        "semantic_prompt_segments_routed_independently",
                        "learned_authority_knowledge_edge",
                        "learned_edge_authority_final=true"
                    ]);
                return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Ok(emptySegmentedResult);
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

        // A supplied learned authority owns only the non-empty primary shortlist, never the single-router fallback.
        if (scored.Length > 0
            && _learnedAuthority is not null
            && request.Flags.UseLearnedUnitWhenAvailable)
        {
            return await ExecuteLearnedAuthorityAsync(
                request,
                subPhases.Value,
                scored,
                _learnedAuthority,
                cancellationToken);
        }

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

    // Invoke one explicitly supplied learned authority over an independently owned baseline shortlist.
    private static async Task<OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>> ExecuteLearnedAuthorityAsync(
        LogicalUnitRequest<SubPhaseRouterSourceDocument> request,
        IReadOnlyList<LogicalSubPhaseEntry> subPhases,
        IReadOnlyList<CandidateScore> scored,
        ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision> learnedAuthority,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return CancelledLearnedAuthority();

        LogicalUnitDescriptor descriptor;
        ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision> unit;
        try
        {
            descriptor = learnedAuthority.Descriptor;
            unit = learnedAuthority.Unit;
            if (!IsValidLearnedAuthorityDescriptor(descriptor, unit))
            {
                if (cancellationToken.IsCancellationRequested)
                    return CancelledLearnedAuthority();

                return InvalidLearnedAuthority();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CancelledLearnedAuthority();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidLearnedAuthority();
        }

        EdgeAuthorityInput retainedInput;
        try
        {
            if (!TryBuildLearnedAuthorityInput(request, scored, out retainedInput))
            {
                if (cancellationToken.IsCancellationRequested)
                    return CancelledLearnedAuthority();

                return InvalidLearnedAuthority();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CancelledLearnedAuthority();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidLearnedAuthority();
        }

        if (cancellationToken.IsCancellationRequested)
            return CancelledLearnedAuthority();

        OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>? executed;
        try
        {
            var dependencyRequest = new LogicalUnitRequest<EdgeAuthorityInput>(
                request.RunId,
                descriptor.UnitId,
                descriptor.UnitRole,
                descriptor.DomainTag,
                CopyLearnedAuthorityInput(retainedInput),
                request.Flags with { },
                new ReadOnlyDictionary<string, object>(
                    new Dictionary<string, object>(StringComparer.Ordinal)));
            var task = unit.ExecuteAsync(dependencyRequest, cancellationToken);
            if (task is null)
            {
                if (cancellationToken.IsCancellationRequested)
                    return CancelledLearnedAuthority();

                return LearnedAuthorityResultRequired();
            }

            executed = await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CancelledLearnedAuthority();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return LearnedAuthorityExecutionFailed();
        }

        if (cancellationToken.IsCancellationRequested)
            return CancelledLearnedAuthority();

        if (executed is null
            || executed.IsSuccess && (executed.Value is null || executed.Value.Output is null))
        {
            return LearnedAuthorityResultRequired();
        }

        if (!executed.IsSuccess)
            return LearnedAuthorityExecutionFailed();

        LearnedAuthorityResultSnapshot learnedResult;
        try
        {
            if (!TrySnapshotLearnedAuthorityResult(
                    executed.Value!,
                    descriptor,
                    retainedInput,
                    out learnedResult))
            {
                if (cancellationToken.IsCancellationRequested)
                    return CancelledLearnedAuthority();

                return LearnedAuthorityResultInvalid();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CancelledLearnedAuthority();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return LearnedAuthorityResultInvalid();
        }

        if (cancellationToken.IsCancellationRequested)
            return CancelledLearnedAuthority();

        // Knowledge-edge authority deliberately publishes no deterministic fallback route.
        if (learnedResult.Decision.KnowledgeEdge)
        {
            var emptyResult = new LogicalUnitResult<SubPhaseRoutingDecision[]>(
                request.UnitId,
                request.UnitRole,
                request.DomainTag,
                [],
                learnedResult.UnitConfidence,
                [],
                [],
                BuildEvidenceIds(request.Input),
                [
                    "source_path_used_for_scoring=false",
                    "learned_authority_knowledge_edge",
                    "learned_edge_authority_final=true"
                ]);
            return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Ok(emptyResult);
        }

        var candidateById = retainedInput.Candidates.ToDictionary(
            candidate => candidate.TargetSubPhaseId,
            StringComparer.Ordinal);
        var subPhaseById = subPhases.ToDictionary(
            entry => entry.SubPhase.SubPhaseId,
            StringComparer.Ordinal);
        var learnedTargets = learnedResult.Decision.OrderedTargets.ToArray();
        var decisions = learnedTargets
            .Select((target, index) =>
            {
                var candidate = candidateById[target.TargetSubPhaseId];
                var entry = subPhaseById[target.TargetSubPhaseId];
                var nearestWrong = index == 0 && learnedTargets.Length > 1
                    ? learnedTargets[1].Confidence
                    : 0d;
                return new SubPhaseRoutingDecision(
                    request.Input.DocumentId.Trim(),
                    request.Input.SourceStatus.Trim(),
                    entry.SubPhase.PhaseId,
                    target.TargetSubPhaseId,
                    target.Confidence,
                    nearestWrong,
                    candidate.EvidencePredicates.ToArray(),
                    false,
                    "source_classified_by_learned_edge_authority")
                {
                    AuthorityRole = LearnedAuthoritySource
                };
            })
            .ToArray();

        var evidencePredicates = decisions
            .SelectMany(decision => decision.EvidencePredicates)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(predicate => predicate, StringComparer.Ordinal)
            .ToArray();
        var warnings = BuildSourcePathWarnings(request.Input, subPhases, decisions)
            .Concat(string.Equals(request.Input.SourceStatus, "ready", StringComparison.Ordinal)
                ? []
                : new[] { "source_status_not_ready" })
            .Append("learned_edge_authority_final=true")
            .ToArray();
        var result = new LogicalUnitResult<SubPhaseRoutingDecision[]>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            decisions,
            decisions.Min(decision => decision.Confidence),
            evidencePredicates,
            decisions.Select(decision => decision.PredictedSubPhaseId).ToArray(),
            BuildEvidenceIds(request.Input),
            warnings);
        if (cancellationToken.IsCancellationRequested)
            return CancelledLearnedAuthority();

        return OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Ok(result);
    }

    private static bool TryBuildLearnedAuthorityInput(
        LogicalUnitRequest<SubPhaseRouterSourceDocument> request,
        IReadOnlyList<CandidateScore> scored,
        out EdgeAuthorityInput input)
    {
        input = null!;
        var source = request.Input;
        var predicateIds = source.CompiledPredicateIds?.ToArray();
        if (!IsCanonicalLearnedValue(request.RunId)
            || !IsCanonicalLearnedValue(source.DocumentId)
            || !IsCanonicalLearnedValue(source.SourceStatus)
            || !IsSafeLearnedPath(source.SourcePath)
            || predicateIds is null
            || predicateIds.Any(value => !IsCanonicalLearnedValue(value)
                                         || !StringComparer.Ordinal.Equals(
                                             value,
                                             value.ToUpperInvariant()))
            || predicateIds.Distinct(StringComparer.Ordinal).Count() != predicateIds.Length
            || !TrySnapshotLearnedContext(
                ContextPacketReader.Read(request.Metadata),
                out var context))
        {
            return false;
        }

        var semanticTokens = DeterministicSubPhaseBaselineScorer
            .Tokenize(string.Join(
                " ",
                source.DocumentTitle,
                source.DocumentType,
                source.CompiledText,
                string.Join(" ", predicateIds)))
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        if (semanticTokens.Length + predicateIds.Length == 0)
            return false;

        var candidates = new List<EdgeCandidate>(scored.Count);
        foreach (var candidate in scored)
        {
            var target = candidate.Entry.SubPhase.SubPhaseId;
            var phase = candidate.Entry.SubPhase.PhaseId;
            var evidence = candidate.EvidencePredicates?.ToArray();
            if (!IsCanonicalLearnedValue(target)
                || !IsCanonicalLearnedValue(phase)
                || evidence is null
                || evidence.Any(value => !IsCanonicalLearnedValue(value))
                || evidence.Distinct(StringComparer.Ordinal).Count() != evidence.Length
                || !double.IsFinite(candidate.Score)
                || candidate.Score < 0d)
            {
                return false;
            }

            candidates.Add(new EdgeCandidate(
                target,
                evidence,
                candidate.Score,
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole));
        }

        if (candidates.Count == 0
            || candidates.Select(candidate => candidate.TargetSubPhaseId)
                .Distinct(StringComparer.Ordinal)
                .Count() != candidates.Count)
        {
            return false;
        }

        input = new EdgeAuthorityInput(
            RouterEdgeId,
            semanticTokens,
            predicateIds.ToArray(),
            context,
            Array.AsReadOnly(candidates.Select(CopyLearnedCandidate).ToArray()),
            DynamicGraphEdgeAuthorityLeakageSignalVocabulary.CreateSnapshot());
        return true;
    }

    private static bool TrySnapshotLearnedAuthorityResult(
        LogicalUnitResult<EdgeAuthorityDecision> source,
        LogicalUnitDescriptor descriptor,
        EdgeAuthorityInput retainedInput,
        out LearnedAuthorityResultSnapshot snapshot)
    {
        snapshot = null!;
        if (!StringComparer.Ordinal.Equals(source.UnitId, descriptor.UnitId)
            || !StringComparer.Ordinal.Equals(source.UnitRole, descriptor.UnitRole)
            || !StringComparer.Ordinal.Equals(source.DomainTag, descriptor.DomainTag)
            || !double.IsFinite(source.Confidence)
            || source.Confidence is < 0d or > 1d
            || source.PredicateIds is null
            || source.DomainTags is null
            || source.EvidenceIds is null
            || source.Warnings is null
            || source.Output is null
            || source.TransitionSignal is not null)
        {
            return false;
        }

        var predicateIds = source.PredicateIds.ToArray();
        var domainTags = source.DomainTags.ToArray();
        var evidenceIds = source.EvidenceIds.ToArray();
        var warnings = source.Warnings.ToArray();
        if (predicateIds.Any(value => !IsCanonicalLearnedValue(value))
            || domainTags.Any(value => !IsCanonicalLearnedValue(value))
            || evidenceIds.Any(value => !IsCanonicalLearnedValue(value))
            || warnings.Any(value => !IsSafeLearnedText(value)))
        {
            return false;
        }

        var decision = source.Output;
        if (!StringComparer.Ordinal.Equals(decision.AuthoritySource, LearnedAuthoritySource)
            || decision.OrderedTargets is null
            || decision.AblationBaselineRank is null)
        {
            return false;
        }

        var orderedTargets = decision.OrderedTargets.ToArray();
        var ablationRank = decision.AblationBaselineRank.ToArray();
        var candidateSnapshot = retainedInput.Candidates
            .Select(CopyLearnedCandidate)
            .ToArray();
        var candidateIds = candidateSnapshot
            .Select(candidate => candidate.TargetSubPhaseId)
            .ToHashSet(StringComparer.Ordinal);
        var selectedIds = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < orderedTargets.Length; index++)
        {
            var target = orderedTargets[index];
            if (!IsCanonicalLearnedValue(target.TargetSubPhaseId)
                || !selectedIds.Add(target.TargetSubPhaseId)
                || !candidateIds.Contains(target.TargetSubPhaseId)
                || !double.IsFinite(target.Confidence)
                || target.Confidence is < 0d or > 1d)
            {
                return false;
            }

            if (index > 0)
            {
                var previous = orderedTargets[index - 1];
                if (previous.Confidence < target.Confidence
                    || previous.Confidence.Equals(target.Confidence)
                    && StringComparer.Ordinal.Compare(
                        previous.TargetSubPhaseId,
                        target.TargetSubPhaseId) >= 0)
                {
                    return false;
                }
            }
        }

        if (decision.FanOutCount != orderedTargets.Length
            || decision.FanOutCount > candidateSnapshot.Length
            || decision.KnowledgeEdge != (orderedTargets.Length == 0))
        {
            return false;
        }

        var expectedBaselineRank = candidateSnapshot
            .OrderByDescending(candidate => candidate.BaselineScore)
            .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
            .Select(candidate => candidate.TargetSubPhaseId)
            .ToArray();
        if (!ablationRank.SequenceEqual(expectedBaselineRank, StringComparer.Ordinal))
            return false;

        snapshot = new LearnedAuthorityResultSnapshot(
            source.Confidence,
            new EdgeAuthorityDecision(
                Array.AsReadOnly(orderedTargets.ToArray()),
                decision.FanOutCount,
                decision.KnowledgeEdge,
                LearnedAuthoritySource,
                Array.AsReadOnly(ablationRank.ToArray())));
        return true;
    }

    private static bool IsValidLearnedAuthorityDescriptor(
        LogicalUnitDescriptor? descriptor,
        ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>? unit)
        => descriptor is not null
           && unit is not null
           && IsCanonicalLearnedValue(descriptor.UnitId)
           && IsCanonicalLearnedValue(descriptor.Version)
           && descriptor.RegistrationOrder is > 0 and < long.MaxValue
           && StringComparer.Ordinal.Equals(descriptor.UnitRole, LearnedAuthorityRole)
           && StringComparer.Ordinal.Equals(descriptor.DomainTag, DomainTag)
           && descriptor.IsLearned
           && descriptor.TrustState is LogicalUnitTrustState.Probationary or LogicalUnitTrustState.Trusted
           && descriptor.InputType == typeof(EdgeAuthorityInput)
           && descriptor.OutputType == typeof(EdgeAuthorityDecision);

    private static bool AreEquivalentLearnedSegmentDecisions(
        SubPhaseRoutingDecision retained,
        SubPhaseRoutingDecision candidate)
        => StringComparer.Ordinal.Equals(retained.DocumentId, candidate.DocumentId)
           && StringComparer.Ordinal.Equals(retained.SourceStatus, candidate.SourceStatus)
           && StringComparer.Ordinal.Equals(retained.PredictedPhaseId, candidate.PredictedPhaseId)
           && StringComparer.Ordinal.Equals(retained.PredictedSubPhaseId, candidate.PredictedSubPhaseId)
           && retained.Confidence.Equals(candidate.Confidence)
           && retained.NearestWrongSubPhaseScore.Equals(candidate.NearestWrongSubPhaseScore)
           && retained.KnowledgeEdge == candidate.KnowledgeEdge
           && StringComparer.Ordinal.Equals(retained.FailureClassification, candidate.FailureClassification)
           && StringComparer.Ordinal.Equals(retained.AuthorityRole, candidate.AuthorityRole)
           && retained.EvidencePredicates.SequenceEqual(
               candidate.EvidencePredicates,
               StringComparer.Ordinal);

    private static bool TrySnapshotLearnedContext(
        ContextSignals? source,
        out ContextSignals snapshot)
    {
        snapshot = null!;
        if (source is null
            || source.PriorDomains is null
            || source.CompletedSubPhases is null
            || source.ActiveKnowledgeEdges is null
            || source.PriorConfidenceMap is null)
        {
            return false;
        }

        var priorDomains = source.PriorDomains.ToArray();
        var completed = source.CompletedSubPhases.ToArray();
        var activeEdges = source.ActiveKnowledgeEdges.ToArray();
        if (priorDomains.Any(domain => !Enum.IsDefined(typeof(SemanticCompiler29.Domain), domain))
            || priorDomains.Distinct().Count() != priorDomains.Length
            || completed.Any(value => !IsCanonicalLearnedValue(value))
            || activeEdges.Any(value => !IsCanonicalLearnedValue(value))
            || completed.Distinct(StringComparer.Ordinal).Count() != completed.Length
            || activeEdges.Distinct(StringComparer.Ordinal).Count() != activeEdges.Length)
        {
            return false;
        }

        var confidence = new SortedDictionary<SemanticCompiler29.Domain, double>();
        foreach (var pair in source.PriorConfidenceMap)
        {
            if (!Enum.IsDefined(typeof(SemanticCompiler29.Domain), pair.Key)
                || !double.IsFinite(pair.Value)
                || !confidence.TryAdd(pair.Key, pair.Value))
            {
                return false;
            }
        }

        snapshot = new ContextSignals(
            priorDomains.ToArray(),
            completed.ToArray(),
            activeEdges.ToArray(),
            new ReadOnlyDictionary<SemanticCompiler29.Domain, double>(confidence));
        return true;
    }

    private static EdgeAuthorityInput CopyLearnedAuthorityInput(EdgeAuthorityInput source)
        => new(
            source.EdgeId,
            source.SemanticTokens.ToArray(),
            source.PredicateIds.ToArray(),
            CopyLearnedContext(source.Context),
            Array.AsReadOnly(source.Candidates.Select(CopyLearnedCandidate).ToArray()),
            source.ExcludedLeakageSignals.ToArray());

    private static ContextSignals CopyLearnedContext(ContextSignals source)
    {
        var confidence = new SortedDictionary<SemanticCompiler29.Domain, double>();
        foreach (var pair in source.PriorConfidenceMap.OrderBy(pair => (int)pair.Key))
            confidence.Add(pair.Key, pair.Value);

        return new ContextSignals(
            source.PriorDomains.ToArray(),
            source.CompletedSubPhases.ToArray(),
            source.ActiveKnowledgeEdges.ToArray(),
            new ReadOnlyDictionary<SemanticCompiler29.Domain, double>(confidence));
    }

    private static EdgeCandidate CopyLearnedCandidate(EdgeCandidate source)
        => new(
            source.TargetSubPhaseId,
            source.EvidencePredicates.ToArray(),
            source.BaselineScore,
            source.AuthorityRole);

    private static bool IsCanonicalLearnedValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && HasValidLearnedCharacters(value);

    private static bool IsSafeLearnedText(string? value)
        => value is not null
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && HasValidLearnedCharacters(value);

    private static bool IsSafeLearnedPath(string? value)
        => value is not null && HasValidLearnedCharacters(value);

    private static bool HasValidLearnedCharacters(string value)
    {
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsControl(character)
                || character is '\u0085' or '\u2028' or '\u2029')
            {
                return false;
            }

            if (char.IsHighSurrogate(character))
            {
                if (index + 1 >= value.Length || !char.IsLowSurrogate(value[index + 1]))
                    return false;

                index++;
                continue;
            }

            if (char.IsLowSurrogate(character))
                return false;
        }

        return true;
    }

    private static OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>> InvalidLearnedAuthority()
        => OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail(
            "Learned edge authority is invalid.",
            LearnedAuthorityInvalidCode);

    private static OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>> LearnedAuthorityExecutionFailed()
        => OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail(
            "Learned edge authority execution failed.",
            LearnedAuthorityExecutionFailedCode);

    private static OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>> LearnedAuthorityResultRequired()
        => OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail(
            "Learned edge authority result is required.",
            LearnedAuthorityResultRequiredCode);

    private static OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>> LearnedAuthorityResultInvalid()
        => OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail(
            "Learned edge authority result is invalid.",
            LearnedAuthorityResultInvalidCode);

    private static OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>> CancelledLearnedAuthority()
        => OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Fail(
            "Cancelled.",
            "CANCELLED");

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

    private sealed record LearnedAuthorityResultSnapshot(
        double UnitConfidence,
        EdgeAuthorityDecision Decision);
}
