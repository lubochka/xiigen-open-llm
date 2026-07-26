// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Contracts;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

public interface ITrainableRoutingFlow : ILogicalFlow<RoutingRequest>
{
    Task<OperationResult<UnitScoringResult[]>> ScoreAsync(
        RoutingRequest request,
        CancellationToken ct = default);

    Task<OperationResult<RoutingDecision>> RouteAsync(
        RoutingRequest request,
        CancellationToken ct = default);

    Task<OperationResult<bool>> RecordDecisionAsync(
        RoutingDecisionRecord decision,
        CancellationToken ct = default);
}
