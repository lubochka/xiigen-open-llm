// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.Http;

using AgentInfrastructure;
using ModelServer.Contracts;

public sealed class ModelEndpointRegistry
{
    private readonly Dictionary<string, RemoteUnitManifest> _endpoints = new(StringComparer.Ordinal);

    public OperationResult<bool> Register(RemoteUnitManifest manifest)
    {
        if (manifest is null)
            return OperationResult<bool>.Fail("Remote unit manifest is required.", "MANIFEST_REQUIRED");

        if (string.IsNullOrWhiteSpace(manifest.UnitId))
            return OperationResult<bool>.Fail("Unit id is required.", "UNIT_ID_REQUIRED");

        if (string.IsNullOrWhiteSpace(manifest.UnitEndpoint))
            return OperationResult<bool>.Fail("Unit endpoint is required.", "UNIT_ENDPOINT_REQUIRED");

        _endpoints[manifest.UnitId] = manifest;
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<RemoteUnitManifest> Resolve(string unitId)
    {
        if (string.IsNullOrWhiteSpace(unitId))
            return OperationResult<RemoteUnitManifest>.Fail("Unit id is required.", "UNIT_ID_REQUIRED");

        return _endpoints.TryGetValue(unitId, out var manifest)
            ? OperationResult<RemoteUnitManifest>.Ok(manifest)
            : OperationResult<RemoteUnitManifest>.Fail($"Endpoint not registered: {unitId}", "ENDPOINT_NOT_FOUND");
    }
}
