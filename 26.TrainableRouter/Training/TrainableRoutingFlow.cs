// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using TrainableRouter.Contracts;

public sealed class TrainableRoutingFlow : ITrainableRoutingFlow
{
    public const string FlowId = "trainable-routing-flow";

    private readonly IUnitScorer _scorer;
    private readonly IRoutingDecisionLog _decisionLog;
    private readonly RoutingFlowThresholds _thresholds;

    public TrainableRoutingFlow(
        IUnitScorer scorer,
        IRoutingDecisionLog decisionLog,
        RoutingFlowThresholds? thresholds = null)
    {
        _scorer = scorer;
        _decisionLog = decisionLog;
        _thresholds = thresholds ?? RoutingFlowThresholds.Default;
    }

    public async Task<OperationResult<UnitScoringResult[]>> ScoreAsync(
        RoutingRequest request,
        CancellationToken ct = default)
    {
        var scored = await _scorer.ScoreAsync(request, ct);
        if (!scored.IsSuccess)
            return scored;

        var ranked = scored.Value!;
        if (ranked.Length > 0 && ranked.Any(candidate =>
                string.IsNullOrWhiteSpace(candidate.AuthorityRole) ||
                !StringComparer.Ordinal.Equals(
                    candidate.AuthorityRole,
                    RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole)))
        {
            return OperationResult<UnitScoringResult[]>.Fail(
                "Routing scorer candidate authority role is invalid.",
                "ROUTING_SCORER_AUTHORITY_ROLE_INVALID");
        }

        return scored;
    }

    public async Task<OperationResult<RoutingDecision>> RouteAsync(
        RoutingRequest request,
        CancellationToken ct = default)
    {
        var scored = await ScoreAsync(request, ct);
        if (!scored.IsSuccess)
            return scored.MapError<RoutingDecision>();

        var ranked = scored.Value!;
        if (ranked.Length == 0)
            return OperationResult<RoutingDecision>.Fail("No scored unit candidates were returned.", "NO_SCORED_UNITS");

        var scoreGap = ComputeScoreGap(ranked, _thresholds);
        if (!TryChooseRoutableCandidate(ranked, _thresholds, out var chosen, out scoreGap))
            return OperationResult<RoutingDecision>.Ok(BuildKnowledgeEdgeDecision(request, ranked, chosen, scoreGap, _thresholds));

        var reason = $"Selected {chosen.UnitId} for domain {chosen.DomainTag} with score {chosen.Score:0.###}, confidence {chosen.Confidence:0.###}, and score gap {scoreGap.ScoreGap:0.###}.";
        var decision = new RoutingDecision(
            request.RequestId,
            chosen.UnitId,
            ranked,
            chosen.Confidence,
            reason)
        {
            AuthorityRole = chosen.AuthorityRole,
            ScoreGap = scoreGap,
            ThresholdsRef = _thresholds
        };

        return OperationResult<RoutingDecision>.Ok(decision);
    }

    public Task<OperationResult<bool>> RecordDecisionAsync(
        RoutingDecisionRecord decision,
        CancellationToken ct = default)
        => _decisionLog.AppendAsync(decision, ct);

    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<RoutingRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return OperationResult<LogicalFlowResult>.Fail("Logical flow request is required.", "FLOW_REQUEST_REQUIRED");

        var routed = await RouteAsync(request.Input, cancellationToken);
        if (!routed.IsSuccess)
            return routed.MapError<LogicalFlowResult>();

        var decision = routed.Value!;
        var chosenUnitId = decision.KnowledgeEdge ? string.Empty : decision.ChosenUnitId ?? string.Empty;
        var verdict = decision.KnowledgeEdge ? "ROUTING_KNOWLEDGE_EDGE" : "ROUTED";
        var nextAction = decision.KnowledgeEdge ? "ROUTING_KNOWLEDGE_EDGE" : "ROUTE_TO_UNIT";
        var summary = new LogicalFlowSummary(
            $"summary-{request.RunId}",
            1,
            0,
            decision.Confidence,
            "routing_confidence",
            false,
            !decision.KnowledgeEdge,
            false,
            nextAction,
            decision.Confidence,
            chosenUnitId,
            decision.Reason,
            0,
            0);

        var metrics = new Dictionary<string, string>
        {
            ["chosen_unit_id"] = chosenUnitId,
            ["candidate_count"] = decision.RankedCandidates.Length.ToString(),
            ["routing_confidence"] = decision.Confidence.ToString("0.######"),
            ["knowledge_edge"] = decision.KnowledgeEdge.ToString(),
            ["minimum_routing_confidence"] = (decision.ThresholdsRef ?? _thresholds).MinimumRoutingConfidence.ToString("0.######"),
            ["minimum_score_gap"] = (decision.ThresholdsRef ?? _thresholds).MinimumScoreGap.ToString("0.######"),
            ["score_gap"] = (decision.ScoreGap?.ScoreGap ?? 0).ToString("0.######"),
            ["thresholds_ref"] = (decision.ThresholdsRef ?? _thresholds).ThresholdsRef,
            [RoutingAuthorityVocabulary.MetadataKey] = decision.AuthorityRole
        };

        var result = new LogicalFlowResult(
            request.FlowId,
            request.ScenarioId,
            verdict,
            decision.Reason,
            summary,
            metrics,
            [decision.RequestId],
            []);

        return OperationResult<LogicalFlowResult>.Ok(result);
    }

    public static bool TryChooseRoutableCandidate(
        UnitScoringResult[] ranked,
        RoutingFlowThresholds thresholds,
        out UnitScoringResult chosen,
        out RoutingScoreGapEvidence scoreGap)
    {
        chosen = ranked.FirstOrDefault()!;
        scoreGap = ComputeScoreGap(ranked, thresholds);

        if (chosen is null)
            return false;

        if (chosen.Confidence < thresholds.MinimumRoutingConfidence)
            return false;

        return scoreGap.MeetsMinimumScoreGap;
    }

    public static RoutingScoreGapEvidence ComputeScoreGap(
        UnitScoringResult[] ranked,
        RoutingFlowThresholds thresholds)
    {
        var top = ranked.FirstOrDefault();
        var second = ranked.Skip(1).FirstOrDefault();
        var gap = top is null
            ? 0
            : second is null
                ? top.Score
                : top.Score - second.Score;

        return new RoutingScoreGapEvidence(
            top?.UnitId ?? string.Empty,
            second?.UnitId,
            top?.Score ?? 0,
            second?.Score,
            gap,
            second is null || gap >= thresholds.MinimumScoreGap);
    }

    public static RoutingDecision BuildKnowledgeEdgeDecision(
        RoutingRequest request,
        UnitScoringResult[] ranked,
        UnitScoringResult topCandidate,
        RoutingScoreGapEvidence scoreGap,
        RoutingFlowThresholds thresholds)
    {
        var belowConfidence = topCandidate.Confidence < thresholds.MinimumRoutingConfidence;
        var reasonCode = belowConfidence
            ? "ROUTING_CONFIDENCE_BELOW_THRESHOLD"
            : "ROUTING_SCORE_GAP_BELOW_THRESHOLD";
        var explanation = belowConfidence
            ? $"Top candidate {topCandidate.UnitId} confidence {topCandidate.Confidence:0.###} is below minimum {thresholds.MinimumRoutingConfidence:0.###}."
            : $"Top candidate {topCandidate.UnitId} score gap {scoreGap.ScoreGap:0.###} is below minimum {thresholds.MinimumScoreGap:0.###}.";

        return new RoutingDecision(
            request.RequestId,
            null,
            ranked,
            topCandidate.Confidence,
            explanation)
        {
            AuthorityRole = topCandidate.AuthorityRole,
            KnowledgeEdge = true,
            ScoreGap = scoreGap,
            ThresholdsRef = thresholds,
            KnowledgeEdgeEvidence = new RoutingKnowledgeEdge(true, reasonCode, explanation)
        };
    }
}
