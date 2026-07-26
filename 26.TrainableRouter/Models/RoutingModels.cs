// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter;

public sealed record RoutingRequest(
    string RequestId,
    string OriginalPrompt,
    string TaskSlice,
    string? RequiredDirection,
    string? DomainHint,
    string[] CandidateUnitIds,
    IReadOnlyDictionary<string, string> Context);

public sealed record UnitScoringResult(
    string UnitId,
    string UnitRole,
    string DomainTag,
    double Score,
    double Confidence);

public sealed record RoutingFlowThresholds(
    double MinimumRoutingConfidence,
    double MinimumScoreGap,
    string ThresholdsRef)
{
    public static RoutingFlowThresholds Default { get; } = new(0.40, 0.05, "trainable-routing-flow-thresholds-v1");
}

public sealed record RoutingScoreGapEvidence(
    string TopUnitId,
    string? SecondUnitId,
    double TopScore,
    double? SecondScore,
    double ScoreGap,
    bool MeetsMinimumScoreGap);

public sealed record RoutingKnowledgeEdge(
    bool KnowledgeEdge,
    string ReasonCode,
    string Explanation);

public sealed record RoutingDecision(
    string RequestId,
    string? ChosenUnitId,
    UnitScoringResult[] RankedCandidates,
    double Confidence,
    string Reason)
{
    public bool KnowledgeEdge { get; init; }

    public RoutingScoreGapEvidence? ScoreGap { get; init; }

    public RoutingFlowThresholds? ThresholdsRef { get; init; }

    public RoutingKnowledgeEdge? KnowledgeEdgeEvidence { get; init; }
}

public sealed record RoutingDecisionRecord(
    string RunId,
    string RequestId,
    string Prompt,
    string TaskSlice,
    string ChosenUnitId,
    double ChosenScore,
    UnitScoringResult[] RejectedCandidates,
    string CorrectReason,
    string WrongRouteConsequence,
    DateTimeOffset DecidedAtUtc)
{
    public bool KnowledgeEdge { get; init; }

    public RoutingScoreGapEvidence? ScoreGap { get; init; }

    public RoutingFlowThresholds? ThresholdsRef { get; init; }
}

public sealed record RoutingDpoPair(
    string PairId,
    string ASide,
    string Chosen,
    string Rejected,
    string SourceDecisionRunId,
    string ChosenUnitId,
    string RejectedUnitId);

public sealed record RoutingTrainingResult(
    int PairsBuilt,
    int PairsTrainedOn,
    double RewardMargin,
    double DpoAccuracy,
    bool Promoted);
