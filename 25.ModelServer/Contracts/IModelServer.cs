// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.Contracts;

using AgentInfrastructure;

public interface IModelServer
{
    Task<OperationResult<RemoteUnitManifest>> PublishAsync(
        RemoteUnitManifest manifest,
        Stream weights,
        CancellationToken ct = default);

    Task<OperationResult<Stream>> OpenWeightsAsync(
        string unitId,
        CancellationToken ct = default);
}
