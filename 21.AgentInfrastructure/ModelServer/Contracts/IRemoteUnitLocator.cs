// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.Contracts;

using AgentInfrastructure;

// This contract lives in AgentInfrastructure even though the namespace stays
// ModelServer.Contracts: the locator is shared routing transport, not the
// ModelServer training authority. Keeping the namespace stable avoids a
// breaking rename while letting TrainableRouter depend on the shared surface
// without depending on the server executable.
public interface IRemoteUnitLocator
{
    /// <summary>
    /// Returns a stable canonical SHA-256 identity for the locator's
    /// non-secret configuration and logical registry destination. Fresh
    /// instances that address the same registry must return the same identity.
    /// </summary>
    OperationResult<string> PreflightStableConfigurationIdentitySha256();

    Task<OperationResult<RemoteUnitManifest>> LocateAsync(
        string unitId,
        CancellationToken ct = default);

    Task<OperationResult<bool>> RegisterAsync(
        RemoteUnitManifest manifest,
        CancellationToken ct = default);
}
