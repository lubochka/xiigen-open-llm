// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.Remote;

using AgentInfrastructure;
using ModelServer.Contracts;
using System.Security.Cryptography;

// This in-memory locator is a shared bootstrap fixture for tests and local
// routing flows. It deliberately does not publish weights; production
// publication remains behind the ModelServer-owned training and checkpoint
// authority.
public sealed class InMemoryRemoteUnitLocator : IRemoteUnitLocator
{
    private readonly InMemoryRegistry _registry = new();

    public OperationResult<string> PreflightStableConfigurationIdentitySha256()
        => OperationResult<string>.Ok(
            _registry.StableConfigurationIdentitySha256);

    public Task<OperationResult<RemoteUnitManifest>> LocateAsync(
        string unitId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(unitId))
            return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail("Unit id is required.", "UNIT_ID_REQUIRED"));

        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail("Unit lookup was cancelled.", "UNIT_LOOKUP_CANCELLED"));

        return _registry.Manifests.TryGetValue(unitId, out var manifest)
            ? Task.FromResult(OperationResult<RemoteUnitManifest>.Ok(manifest))
            : Task.FromResult(OperationResult<RemoteUnitManifest>.Fail($"Remote unit not registered: {unitId}", "REMOTE_UNIT_NOT_FOUND"));
    }

    public Task<OperationResult<bool>> RegisterAsync(
        RemoteUnitManifest manifest,
        CancellationToken ct = default)
    {
        if (manifest is null)
            return Task.FromResult(OperationResult<bool>.Fail("Remote unit manifest is required.", "MANIFEST_REQUIRED"));

        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<bool>.Fail("Unit registration was cancelled.", "UNIT_REGISTER_CANCELLED"));

        if (string.IsNullOrWhiteSpace(manifest.UnitId))
            return Task.FromResult(OperationResult<bool>.Fail("Unit id is required.", "UNIT_ID_REQUIRED"));

        if (string.IsNullOrWhiteSpace(manifest.ServerBaseUrl) || string.IsNullOrWhiteSpace(manifest.UnitEndpoint))
            return Task.FromResult(OperationResult<bool>.Fail("Server URL and endpoint are required.", "REMOTE_ENDPOINT_REQUIRED"));

        _registry.Manifests[manifest.UnitId] = manifest;
        return Task.FromResult(OperationResult<bool>.Ok(true));
    }

    private static string BuildRegistryIdentitySha256()
        => "sha256:" + Convert.ToHexString(SHA256.HashData(
                RandomNumberGenerator.GetBytes(32)))
            .ToLowerInvariant();

    private sealed class InMemoryRegistry
    {
        public string StableConfigurationIdentitySha256 { get; } =
            BuildRegistryIdentitySha256();

        public Dictionary<string, RemoteUnitManifest> Manifests { get; } =
            new(StringComparer.Ordinal);
    }
}
