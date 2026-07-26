// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;
using TrainableRouter.Contracts;

public sealed class InMemoryRoutingDecisionLog : IRoutingDecisionLog
{
    private readonly List<RoutingDecisionRecord> _records = [];

    public Task<OperationResult<bool>> AppendAsync(
        RoutingDecisionRecord decision,
        CancellationToken ct = default)
    {
        if (decision is null)
            return Task.FromResult(OperationResult<bool>.Fail("Routing decision is required.", "ROUTING_DECISION_REQUIRED"));

        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<bool>.Fail("Routing decision append was cancelled.", "ROUTING_DECISION_CANCELLED"));

        _records.Add(decision);
        return Task.FromResult(OperationResult<bool>.Ok(true));
    }

    public Task<OperationResult<IReadOnlyList<RoutingDecisionRecord>>> ListAsync(
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<IReadOnlyList<RoutingDecisionRecord>>.Fail("Routing decision list was cancelled.", "ROUTING_DECISION_LIST_CANCELLED"));

        return Task.FromResult(OperationResult<IReadOnlyList<RoutingDecisionRecord>>.Ok(_records.ToArray()));
    }
}
