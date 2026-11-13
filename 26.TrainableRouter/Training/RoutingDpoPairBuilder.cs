// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;

public sealed class RoutingDpoPairBuilder
{
    public OperationResult<RoutingDpoPair[]> BuildPairs(
        IReadOnlyList<RoutingDecisionRecord> decisions)
    {
        if (decisions is null)
            return OperationResult<RoutingDpoPair[]>.Fail("Routing decisions are required.", "ROUTING_DECISIONS_REQUIRED");

        var pairs = new List<RoutingDpoPair>();
        for (var i = 0; i < decisions.Count; i++)
        {
            var decision = decisions[i];
            var rejected = decision.RejectedCandidates
                .OrderByDescending(candidate => candidate.Score)
                .FirstOrDefault();

            if (rejected is null)
                continue;

            pairs.Add(new RoutingDpoPair(
                $"{decision.RunId}/routing-dpo-{i + 1:0000}",
                $"Prompt slice: {decision.TaskSlice}\nCandidate units: {string.Join(", ", decision.RejectedCandidates.Select(candidate => candidate.UnitId).Prepend(decision.ChosenUnitId))}",
                $"Route to {decision.ChosenUnitId}. {decision.CorrectReason}",
                $"Route to {rejected.UnitId}. [WRONG: {decision.WrongRouteConsequence}]",
                decision.RunId,
                decision.ChosenUnitId,
                rejected.UnitId));
        }

        return OperationResult<RoutingDpoPair[]>.Ok(pairs.ToArray());
    }
}
