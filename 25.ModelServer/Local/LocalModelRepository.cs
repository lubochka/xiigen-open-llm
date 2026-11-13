// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.Local;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ModelServer.Contracts;
using System.Security.Cryptography;
using System.Text;

public sealed class LocalModelRepository : IModelRepository
{
    private static readonly string DefaultStableConfigurationIdentitySha256 =
        BuildStableConfigurationIdentitySha256(
            "model-server-local-model-repository-default-v1");

    private readonly string _stableConfigurationIdentitySha256;

    public LocalModelRepository()
        : this(DefaultStableConfigurationIdentitySha256)
    {
    }

    public LocalModelRepository(string stableConfigurationIdentitySha256)
    {
        if (!IsCanonicalSha256Ref(stableConfigurationIdentitySha256))
        {
            throw new ArgumentException(
                "Stable repository configuration identity must be a canonical non-secret SHA-256 reference.",
                nameof(stableConfigurationIdentitySha256));
        }
        _stableConfigurationIdentitySha256 = stableConfigurationIdentitySha256;
    }

    public OperationResult<string> PreflightStableConfigurationIdentitySha256()
        => OperationResult<string>.Ok(_stableConfigurationIdentitySha256);

    public Task<OperationResult<Stream>> LoadWeightsAsync(
        string unitId,
        string? serverBaseUrl,
        string localFallbackPath,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(unitId))
            return Task.FromResult(OperationResult<Stream>.Fail("Unit id is required.", "UNIT_ID_REQUIRED"));

        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<Stream>.Fail("Model load was cancelled.", "MODEL_LOAD_CANCELLED"));

        if (string.IsNullOrWhiteSpace(localFallbackPath))
            return Task.FromResult(OperationResult<Stream>.Fail("Local fallback path is required.", "LOCAL_PATH_REQUIRED"));

        if (!File.Exists(localFallbackPath))
            return Task.FromResult(OperationResult<Stream>.Fail($"Model weights not found: {localFallbackPath}", "MODEL_WEIGHTS_NOT_FOUND"));

        Stream stream = File.OpenRead(localFallbackPath);
        return Task.FromResult(OperationResult<Stream>.Ok(stream));
    }

    public Task<OperationResult<RemoteUnitManifest>> UploadAsync(
        GeneratedCapabilityCheckpointManifest manifest,
        string serverBaseUrl,
        CancellationToken ct = default)
    {
        if (manifest is null)
            return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail("Manifest is required.", "MANIFEST_REQUIRED"));

        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail("Model upload was cancelled.", "MODEL_UPLOAD_CANCELLED"));

        if (string.IsNullOrWhiteSpace(serverBaseUrl))
            return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail("Server base URL is required.", "SERVER_URL_REQUIRED"));

        if (string.IsNullOrWhiteSpace(manifest.PolicyCheckpointPath) || !File.Exists(manifest.PolicyCheckpointPath))
            return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail("Policy checkpoint must exist before upload.", "POLICY_CHECKPOINT_NOT_FOUND"));

        var endpoint = $"/models/{Uri.EscapeDataString(manifest.UnitId)}";
        var remote = new RemoteUnitManifest(
            manifest.UnitId,
            manifest.UnitRole,
            manifest.DomainTag,
            manifest.Version,
            serverBaseUrl.TrimEnd('/'),
            endpoint,
            ComputeSha256(manifest.PolicyCheckpointPath),
            manifest.TrainingBranch ?? "",
            manifest.TrainingRunId ?? "",
            manifest.SourceDatasetSha256 ?? "",
            new RemoteModelConfig(
                manifest.VocabSize,
                manifest.ContextLen,
                manifest.HiddenDim,
                manifest.NumLayers,
                manifest.NumHeads,
                manifest.NumKvHeads,
                manifest.FfnHiddenDim,
                manifest.RopeTheta,
                manifest.RmsNormEps),
            new RemoteLoraConfig(
                manifest.LoraRank,
                manifest.LoraAlpha,
                manifest.LoraDropout,
                manifest.LoraTargetModules),
            manifest.TokenizerPath,
            DateTimeOffset.UtcNow);

        return Task.FromResult(OperationResult<RemoteUnitManifest>.Ok(remote));
    }

    private static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash);
    }

    private static string BuildStableConfigurationIdentitySha256(string material)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(material)))
            .ToLowerInvariant();

    private static bool IsCanonicalSha256Ref(string value)
        => value is { Length: 71 } &&
           value.StartsWith("sha256:", StringComparison.Ordinal) &&
           value.AsSpan(7).ToString().All(character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');
}
