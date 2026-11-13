// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;

public sealed class RoutingTrainingRunner
{
    private readonly RoutingProfileTrainer _profileTrainer;

    public RoutingTrainingRunner(RoutingProfileTrainer? profileTrainer = null)
    {
        _profileTrainer = profileTrainer ?? new RoutingProfileTrainer();
    }

    public async Task<OperationResult<RoutingTrainingRunResult>> TrainAndEvaluateAsync(
        IReadOnlyList<RoutingTrainingExample> trainingExamples,
        IReadOnlyList<RoutingTrainingExample> evaluationExamples,
        CancellationToken ct = default)
    {
        if (trainingExamples is null)
            return OperationResult<RoutingTrainingRunResult>.Fail("Training examples are required.", "TRAINING_EXAMPLES_REQUIRED");

        if (evaluationExamples is null)
            return OperationResult<RoutingTrainingRunResult>.Fail("Evaluation examples are required.", "EVALUATION_EXAMPLES_REQUIRED");

        var profiles = _profileTrainer.Train(trainingExamples);
        if (!profiles.IsSuccess)
            return profiles.MapError<RoutingTrainingRunResult>();

        var log = new InMemoryRoutingDecisionLog();
        var flow = new TrainableRoutingFlow(new BaselineKeywordCandidateGenerator(profiles.Value!), log);
        var cases = new List<RoutingCaseResult>();
        var decisions = new List<RoutingDecisionRecord>();

        foreach (var example in evaluationExamples)
        {
            if (ct.IsCancellationRequested)
                return OperationResult<RoutingTrainingRunResult>.Fail("Routing training was cancelled.", "ROUTING_TRAINING_CANCELLED");

            var request = ToRoutingRequest(example);
            var routed = await flow.RouteAsync(request, ct);
            if (!routed.IsSuccess)
                return routed.MapError<RoutingTrainingRunResult>();

            var decision = routed.Value!;
            var actualUnitId = decision.KnowledgeEdge ? string.Empty : decision.ChosenUnitId ?? string.Empty;
            var routeAccepted = !decision.KnowledgeEdge;
            var routeCorrect = routeAccepted && string.Equals(example.ChosenUnitId, actualUnitId, StringComparison.Ordinal);
            cases.Add(new RoutingCaseResult(
                example.ExampleId,
                example.ChosenUnitId,
                actualUnitId,
                routeCorrect,
                decision.Confidence,
                decision.KnowledgeEdge,
                decision.ScoreGap?.ScoreGap ?? 0,
                decision.Reason));

            var decisionRecord = new RoutingDecisionRecord(
                $"routing-eval-{example.ExampleId}",
                example.ExampleId,
                example.Prompt,
                example.TaskSlice,
                routeAccepted ? example.ChosenUnitId : string.Empty,
                decision.RankedCandidates.FirstOrDefault(candidate => candidate.UnitId == example.ChosenUnitId)?.Score ?? 0,
                decision.RankedCandidates.Where(candidate => candidate.UnitId != example.ChosenUnitId).ToArray(),
                example.CorrectReason,
                example.WrongRouteConsequence,
                DateTimeOffset.UtcNow)
            {
                KnowledgeEdge = decision.KnowledgeEdge,
                ScoreGap = decision.ScoreGap,
                ThresholdsRef = decision.ThresholdsRef
            };

            decisions.Add(decisionRecord);
        }

        var acceptedDecisions = decisions.Where(decision => !decision.KnowledgeEdge).ToArray();
        var pairs = new RoutingDpoPairBuilder().BuildPairs(acceptedDecisions);
        if (!pairs.IsSuccess)
            return pairs.MapError<RoutingTrainingRunResult>();

        var correct = cases.Count(item => item.Correct);
        var result = new RoutingTrainingRunResult(
            trainingExamples.Count,
            evaluationExamples.Count,
            correct,
            evaluationExamples.Count == 0 ? 0 : correct / (double)evaluationExamples.Count,
            profiles.Value!.Values.OrderBy(profile => profile.UnitId, StringComparer.Ordinal).ToArray(),
            pairs.Value!,
            cases.ToArray())
        {
            KnowledgeEdgeCount = cases.Count(item => item.KnowledgeEdge),
            DecisionRecords = decisions.ToArray()
        };

        return OperationResult<RoutingTrainingRunResult>.Ok(result);
    }

    private static RoutingRequest ToRoutingRequest(RoutingTrainingExample example)
        => new(
            example.ExampleId,
            example.Prompt,
            example.TaskSlice,
            example.RequiredDirection,
            example.DomainHint,
            example.CandidateUnitIds,
            new Dictionary<string, string>());
}
