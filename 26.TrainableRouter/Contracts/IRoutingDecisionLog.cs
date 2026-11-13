// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Contracts;

using AgentInfrastructure;

public interface IRoutingDecisionLog
{
    Task<OperationResult<bool>> AppendAsync(
        RoutingDecisionRecord decision,
        CancellationToken ct = default);

    Task<OperationResult<IReadOnlyList<RoutingDecisionRecord>>> ListAsync(
        CancellationToken ct = default);
}
