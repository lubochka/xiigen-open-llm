// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.Contracts;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

public interface IModelRepository
{
    /// <summary>
    /// Returns a stable canonical SHA-256 identity for the repository's
    /// non-secret configuration and logical destination. Fresh instances that
    /// address the same repository must return the same identity.
    /// </summary>
    OperationResult<string> PreflightStableConfigurationIdentitySha256();

    Task<OperationResult<Stream>> LoadWeightsAsync(
        string unitId,
        string? serverBaseUrl,
        string localFallbackPath,
        CancellationToken ct = default);

    Task<OperationResult<RemoteUnitManifest>> UploadAsync(
        GeneratedCapabilityCheckpointManifest manifest,
        string serverBaseUrl,
        CancellationToken ct = default);
}
