// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.Contracts;

public interface IDesignConversionActivatedModelCheckpointLoader
{
    Task<OperationResult<DesignConversionMachineLoadResult>> LoadAsync(
        DesignConversionActivatedModelVersion activeVersion,
        CancellationToken ct = default);
}

internal static class DesignConversionActivatedModelCheckpointLoaderValidation
{
    public static OperationResult<bool> RequireFreshLoadVerifiedRealWeights(
        DesignConversionActivatedModelVersion activeVersion)
    {
        if (!activeVersion.CheckpointFreshLoadVerified ||
            !activeVersion.CheckpointContainsRealTrainedWeights)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion active model checkpoint must be fresh-load verified and contain real trained weights.",
                DesignConversionActivatedModelRuntimeEvaluator.ActiveModelCheckpointRealWeightsRequiredErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }
}

public sealed class DesignConversionLocalActivatedModelCheckpointLoader : IDesignConversionActivatedModelCheckpointLoader
{
    public Task<OperationResult<DesignConversionMachineLoadResult>> LoadAsync(
        DesignConversionActivatedModelVersion activeVersion,
        CancellationToken ct = default)
    {
        if (activeVersion is null)
        {
            return Task.FromResult(OperationResult<DesignConversionMachineLoadResult>.Fail(
                "DesignConversion active model version is required.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_VERSION_REQUIRED"));
        }

        var checkpointProof = DesignConversionActivatedModelCheckpointLoaderValidation.RequireFreshLoadVerifiedRealWeights(activeVersion);
        if (!checkpointProof.IsSuccess)
            return Task.FromResult(checkpointProof.MapError<DesignConversionMachineLoadResult>());

        return DesignConversionMachine.LoadCheckpointAsync(activeVersion.CheckpointPath, ct);
    }
}

public sealed class DesignConversionRepositoryActivatedModelCheckpointLoader : IDesignConversionActivatedModelCheckpointLoader
{
    public const string RepositoryFreshLoadEvidence =
        "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOCATOR_FRESH_LOAD_VERIFIED";

    private const string CacheSubdirectoryName = "design-conversion-active-checkpoints";
    private const string CacheFileName = "design-conversion-machine-checkpoint.json";
    private const int MaximumCheckpointByteCount = 16 * 1024 * 1024;
    private const int RepositoryReadBufferSize = 64 * 1024;

    private static readonly RuntimeUnitSpec[] RuntimeUnits =
    [
        new(
            DesignConversionRuntimeScaffoldCriticUnit.UnitId,
            DesignConversionRuntimeScaffoldCriticUnit.Role),
        new(
            DesignConversionRuntimeScaffoldImproverUnit.UnitId,
            DesignConversionRuntimeScaffoldImproverUnit.Role)
    ];

    private readonly IModelRepository _repository;
    private readonly IRemoteUnitLocator _locator;
    private readonly string _configuredCacheRoot;
    private readonly StringComparison _pathComparison;

    public DesignConversionRepositoryActivatedModelCheckpointLoader(
        IModelRepository repository,
        IRemoteUnitLocator locator,
        string serverOwnedCacheRoot)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _locator = locator ?? throw new ArgumentNullException(nameof(locator));
        if (string.IsNullOrWhiteSpace(serverOwnedCacheRoot))
            throw new ArgumentException("DesignConversion active checkpoint cache root is required.", nameof(serverOwnedCacheRoot));

        _configuredCacheRoot = serverOwnedCacheRoot;
        _pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
    }

    public async Task<OperationResult<DesignConversionMachineLoadResult>> LoadAsync(
        DesignConversionActivatedModelVersion activeVersion,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionMachineLoadResult>();

        var validatedActive = ValidateActiveVersion(activeVersion);
        if (!validatedActive.IsSuccess)
            return validatedActive.MapError<DesignConversionMachineLoadResult>();

        var expected = validatedActive.Value!;
        var loadedUnits = new List<LoadedRepositoryUnit>(RuntimeUnits.Length);
        foreach (var unit in RuntimeUnits)
        {
            if (ct.IsCancellationRequested)
                return Cancelled<DesignConversionMachineLoadResult>();

            var located = await LocateAndValidateAsync(unit, expected, ct).ConfigureAwait(false);
            if (!located.IsSuccess)
                return located.MapError<DesignConversionMachineLoadResult>();

            var loaded = await LoadUnitBytesAsync(unit, located.Value!, expected, ct).ConfigureAwait(false);
            if (!loaded.IsSuccess)
                return loaded.MapError<DesignConversionMachineLoadResult>();

            loadedUnits.Add(loaded.Value!);
        }

        var unitBytes = ValidateLoadedUnits(expected, loadedUnits);
        if (!unitBytes.IsSuccess)
            return unitBytes.MapError<DesignConversionMachineLoadResult>();

        var cachedPath = await WriteVerifiedCacheAsync(expected, unitBytes.Value!, ct).ConfigureAwait(false);
        if (!cachedPath.IsSuccess)
            return cachedPath.MapError<DesignConversionMachineLoadResult>();

        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionMachineLoadResult>();

        var loadedCheckpoint = await DesignConversionMachine.LoadCheckpointAsync(cachedPath.Value!, ct).ConfigureAwait(false);
        if (!loadedCheckpoint.IsSuccess)
        {
            if (ct.IsCancellationRequested || string.Equals(loadedCheckpoint.ErrorCode, "CANCELLED", StringComparison.Ordinal))
                return Cancelled<DesignConversionMachineLoadResult>();

            return FailLoad(
                "DesignConversion repository active checkpoint cached copy could not be loaded.",
                PreserveSafeErrorCode(
                    loadedCheckpoint.ErrorCode,
                    "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_CACHED_LOAD_FAILED"));
        }

        if (loadedCheckpoint.Value is null)
        {
            return FailLoad(
                "DesignConversion repository active checkpoint cached load returned no checkpoint.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_CACHED_LOAD_EMPTY");
        }

        var identity = ValidateLoadedCheckpointIdentity(expected, loadedCheckpoint.Value);
        if (!identity.IsSuccess)
            return identity.MapError<DesignConversionMachineLoadResult>();

        return loadedCheckpoint;
    }

    private async Task<OperationResult<LocatedRepositoryUnit>> LocateAndValidateAsync(
        RuntimeUnitSpec unit,
        ValidatedActiveVersion expected,
        CancellationToken ct)
    {
        OperationResult<RemoteUnitManifest> located;
        try
        {
            var locateTask = _locator.LocateAsync(unit.UnitId, ct);
            if (locateTask is null)
            {
                return FailLocated(
                    "DesignConversion repository active checkpoint locator lookup failed.",
                    "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOCATE_FAILED");
            }

            located = await locateTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<LocatedRepositoryUnit>();
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return FailLocated(
                "DesignConversion repository active checkpoint locator lookup failed.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOCATE_FAILED");
        }

        if (located is null)
        {
            return FailLocated(
                "DesignConversion repository active checkpoint locator lookup failed.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOCATE_FAILED");
        }

        if (!located.IsSuccess)
        {
            if (ct.IsCancellationRequested || string.Equals(located.ErrorCode, "CANCELLED", StringComparison.Ordinal))
                return Cancelled<LocatedRepositoryUnit>();

            return FailLocated(
                "DesignConversion repository active checkpoint locator lookup failed.",
                PreserveSafeErrorCode(
                    located.ErrorCode,
                    "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOCATE_FAILED"));
        }

        if (located.Value is null)
        {
            return FailLocated(
                "DesignConversion repository active checkpoint locator returned no manifest.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOCATED_MANIFEST_REQUIRED");
        }

        var manifest = located.Value;
        if (!string.Equals(manifest.UnitId, unit.UnitId, StringComparison.Ordinal) ||
            !string.Equals(manifest.UnitRole, unit.UnitRole, StringComparison.Ordinal) ||
            !string.Equals(manifest.DomainTag, DesignConversionMachine.DomainTag, StringComparison.Ordinal) ||
            !string.Equals(manifest.DomainTag, expected.DomainTag, StringComparison.Ordinal) ||
            !string.Equals(manifest.Version, expected.Version, StringComparison.Ordinal) ||
            !IsSameSha256(manifest.ManifestSha256, expected.CheckpointSha256))
        {
            return FailLocated(
                "DesignConversion repository active checkpoint located manifest identity did not match the active version.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_MANIFEST_IDENTITY_MISMATCH");
        }

        var serverBaseUrl = ValidateServerBaseUrl(manifest.ServerBaseUrl);
        if (!serverBaseUrl.IsSuccess)
            return serverBaseUrl.MapError<LocatedRepositoryUnit>();

        var endpointValidation = ValidateUnitEndpoint(
            manifest.UnitEndpoint,
            serverBaseUrl.Value!,
            unit.UnitId);
        if (!endpointValidation.IsSuccess)
            return endpointValidation.MapError<LocatedRepositoryUnit>();

        return OperationResult<LocatedRepositoryUnit>.Ok(
            new LocatedRepositoryUnit(unit, manifest, serverBaseUrl.Value!));
    }

    private async Task<OperationResult<LoadedRepositoryUnit>> LoadUnitBytesAsync(
        RuntimeUnitSpec unit,
        LocatedRepositoryUnit located,
        ValidatedActiveVersion expected,
        CancellationToken ct)
    {
        OperationResult<Stream> loaded;
        try
        {
            var loadTask = _repository.LoadWeightsAsync(
                unit.UnitId,
                located.ServerBaseUrl,
                expected.LocalFallbackPath,
                ct);
            if (loadTask is null)
            {
                return FailLoaded(
                    "DesignConversion repository active checkpoint unit load failed.",
                    "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOAD_FAILED");
            }

            loaded = await loadTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<LoadedRepositoryUnit>();
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return FailLoaded(
                "DesignConversion repository active checkpoint unit load failed.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOAD_FAILED");
        }

        if (loaded is null)
        {
            return FailLoaded(
                "DesignConversion repository active checkpoint unit load failed.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOAD_FAILED");
        }

        if (!loaded.IsSuccess)
        {
            if (ct.IsCancellationRequested || string.Equals(loaded.ErrorCode, "CANCELLED", StringComparison.Ordinal))
                return Cancelled<LoadedRepositoryUnit>();

            return FailLoaded(
                "DesignConversion repository active checkpoint unit load failed.",
                PreserveSafeErrorCode(
                    loaded.ErrorCode,
                    "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOAD_FAILED"));
        }

        if (loaded.Value is null)
        {
            return FailLoaded(
                "DesignConversion repository active checkpoint unit load returned no stream.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOAD_STREAM_REQUIRED");
        }

        OperationResult<RepositoryStreamBytes> read;
        try
        {
            await using var repositoryStream = loaded.Value;
            read = await ReadAllBytesAndHashAsync(repositoryStream, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<LoadedRepositoryUnit>();
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return FailLoaded(
                "DesignConversion repository active checkpoint stream could not be read.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_STREAM_READ_FAILED");
        }

        if (!read.IsSuccess)
            return read.MapError<LoadedRepositoryUnit>();

        var bytes = read.Value!.Bytes;
        var sha256 = read.Value.Sha256;

        if (!IsSameSha256(sha256, located.Manifest.ManifestSha256) ||
            !IsSameSha256(sha256, expected.CheckpointSha256))
        {
            return FailLoaded(
                "DesignConversion repository active checkpoint unit hash did not match the located manifest and active version.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOAD_HASH_MISMATCH");
        }

        return OperationResult<LoadedRepositoryUnit>.Ok(new LoadedRepositoryUnit(
            unit,
            bytes,
            sha256));
    }

    private static OperationResult<VerifiedRepositoryBytes> ValidateLoadedUnits(
        ValidatedActiveVersion expected,
        IReadOnlyList<LoadedRepositoryUnit> loadedUnits)
    {
        if (loadedUnits.Count != RuntimeUnits.Length)
        {
            return OperationResult<VerifiedRepositoryBytes>.Fail(
                "DesignConversion repository active checkpoint requires both runtime units.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_BOTH_UNITS_REQUIRED");
        }

        foreach (var unit in RuntimeUnits)
        {
            if (!loadedUnits.Any(loaded => string.Equals(loaded.Unit.UnitId, unit.UnitId, StringComparison.Ordinal)))
            {
                return OperationResult<VerifiedRepositoryBytes>.Fail(
                    "DesignConversion repository active checkpoint requires both runtime units.",
                    "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_BOTH_UNITS_REQUIRED");
            }
        }

        var first = loadedUnits[0];
        var second = loadedUnits[1];
        if (!IsSameSha256(first.Sha256, expected.CheckpointSha256) ||
            !IsSameSha256(second.Sha256, expected.CheckpointSha256) ||
            !first.Bytes.AsSpan().SequenceEqual(second.Bytes))
        {
            return OperationResult<VerifiedRepositoryBytes>.Fail(
                "DesignConversion repository active checkpoint units did not resolve to the same verified checkpoint bytes.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_UNIT_BYTES_MISMATCH");
        }

        return OperationResult<VerifiedRepositoryBytes>.Ok(new VerifiedRepositoryBytes(
            first.Bytes,
            NormalizeSha256(expected.CheckpointSha256)!));
    }

    private async Task<OperationResult<string>> WriteVerifiedCacheAsync(
        ValidatedActiveVersion expected,
        VerifiedRepositoryBytes verified,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<string>();

        var finalPath = BuildCachePath(verified.Sha256);
        if (!finalPath.IsSuccess)
            return finalPath;

        string? tempPath = null;
        var moved = false;
        try
        {
            var existing = await ValidateExistingCacheAsync(finalPath.Value!, verified.Sha256, ct)
                .ConfigureAwait(false);
            if (!existing.IsSuccess)
                return existing.MapError<string>();
            if (existing.Value)
                return finalPath;

            if (ct.IsCancellationRequested)
                return Cancelled<string>();

            var directory = Path.GetDirectoryName(finalPath.Value!);
            if (string.IsNullOrWhiteSpace(directory))
            {
                return OperationResult<string>.Fail(
                    "DesignConversion repository active checkpoint cache directory is invalid.",
                    "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_CACHE_PATH_INVALID");
            }

            Directory.CreateDirectory(directory);
            tempPath = Path.Combine(
                directory,
                $".{CacheFileName}.{expected.ActivationRefHash}.{Guid.NewGuid():N}.tmp");
            await File.WriteAllBytesAsync(tempPath, verified.Bytes, ct).ConfigureAwait(false);
            if (ct.IsCancellationRequested)
                return Cancelled<string>();

            try
            {
                File.Move(tempPath, finalPath.Value!, overwrite: false);
                moved = true;
            }
            catch (IOException) when (File.Exists(finalPath.Value!))
            {
                var raced = await ValidateExistingCacheAsync(finalPath.Value!, verified.Sha256, ct)
                    .ConfigureAwait(false);
                if (!raced.IsSuccess)
                    return raced.MapError<string>();
                if (!raced.Value)
                    return OperationResult<string>.Fail(
                        "DesignConversion repository active checkpoint cache already contains different bytes.",
                        "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_CACHE_MISMATCH");

                return finalPath;
            }

            return finalPath;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<string>();
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return OperationResult<string>.Fail(
                "DesignConversion repository active checkpoint cache write failed.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_CACHE_WRITE_FAILED");
        }
        finally
        {
            if (!moved && tempPath is not null)
                TryDeleteFile(tempPath);
        }
    }

    private async Task<OperationResult<bool>> ValidateExistingCacheAsync(
        string finalPath,
        string expectedSha256,
        CancellationToken ct)
    {
        try
        {
            if (!File.Exists(finalPath))
                return OperationResult<bool>.Ok(false);

            var hash = await HashFileAsync(finalPath, ct).ConfigureAwait(false);
            if (!hash.IsSuccess)
                return hash.MapError<bool>();

            if (!IsSameSha256(hash.Value!, expectedSha256))
            {
                return OperationResult<bool>.Fail(
                    "DesignConversion repository active checkpoint cache already contains different bytes.",
                    "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_CACHE_MISMATCH");
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<bool>();
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion repository active checkpoint cache could not be read.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_CACHE_READ_FAILED");
        }
    }

    private OperationResult<string> BuildCachePath(string sha256)
    {
        var normalizedSha = NormalizeSha256(sha256);
        if (normalizedSha is null)
        {
            return OperationResult<string>.Fail(
                "DesignConversion repository active checkpoint SHA is invalid.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_SHA_INVALID");
        }

        try
        {
            var cacheRoot = Path.GetFullPath(_configuredCacheRoot);
            var cacheRootWithSeparator = EnsureTrailingDirectorySeparator(cacheRoot);
            var fullPath = Path.GetFullPath(Path.Combine(
                cacheRoot,
                CacheSubdirectoryName,
                normalizedSha[..2],
                normalizedSha,
                CacheFileName));
            if (!fullPath.StartsWith(cacheRootWithSeparator, _pathComparison))
            {
                return OperationResult<string>.Fail(
                    "DesignConversion repository active checkpoint cache path escaped its root.",
                    "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_CACHE_PATH_INVALID");
            }

            return OperationResult<string>.Ok(fullPath);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return OperationResult<string>.Fail(
                "DesignConversion repository active checkpoint cache path is invalid.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_CACHE_PATH_INVALID");
        }
    }

    private static OperationResult<ValidatedActiveVersion> ValidateActiveVersion(
        DesignConversionActivatedModelVersion? activeVersion)
    {
        if (activeVersion is null)
        {
            return OperationResult<ValidatedActiveVersion>.Fail(
                "DesignConversion active model version is required.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_VERSION_REQUIRED");
        }

        var checkpointProof = DesignConversionActivatedModelCheckpointLoaderValidation.RequireFreshLoadVerifiedRealWeights(activeVersion);
        if (!checkpointProof.IsSuccess)
            return checkpointProof.MapError<ValidatedActiveVersion>();

        var checkpointSha256 = NormalizeSha256(activeVersion.CheckpointSha256);
        if (checkpointSha256 is null)
        {
            return OperationResult<ValidatedActiveVersion>.Fail(
                "DesignConversion active model checkpoint SHA is invalid.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_CHECKPOINT_SHA_INVALID");
        }

        if (string.IsNullOrWhiteSpace(activeVersion.ActivationRef) ||
            string.IsNullOrWhiteSpace(activeVersion.ModelId) ||
            string.IsNullOrWhiteSpace(activeVersion.ManifestId) ||
            string.IsNullOrWhiteSpace(activeVersion.ManifestRef) ||
            string.IsNullOrWhiteSpace(activeVersion.Version) ||
            string.IsNullOrWhiteSpace(activeVersion.MachineId) ||
            string.IsNullOrWhiteSpace(activeVersion.DomainTag) ||
            !string.Equals(activeVersion.MachineId, DesignConversionMachine.MachineId, StringComparison.Ordinal) ||
            !string.Equals(activeVersion.DomainTag, DesignConversionMachine.DomainTag, StringComparison.Ordinal))
        {
            return OperationResult<ValidatedActiveVersion>.Fail(
                "DesignConversion active model identity is invalid for repository checkpoint loading.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_IDENTITY_INVALID");
        }

        return OperationResult<ValidatedActiveVersion>.Ok(new ValidatedActiveVersion(
            activeVersion.ActivationRef,
            activeVersion.Version,
            activeVersion.MachineId,
            activeVersion.DomainTag,
            checkpointSha256,
            activeVersion.CheckpointContainsRealTrainedWeights,
            activeVersion.CheckpointPath ?? string.Empty));
    }

    private static OperationResult<bool> ValidateLoadedCheckpointIdentity(
        ValidatedActiveVersion expected,
        DesignConversionMachineLoadResult loaded)
    {
        if (!IsSameSha256(loaded.CheckpointSha256, expected.CheckpointSha256) ||
            !string.Equals(loaded.Checkpoint.MachineId, expected.MachineId, StringComparison.Ordinal) ||
            !string.Equals(loaded.Checkpoint.DomainTag, expected.DomainTag, StringComparison.Ordinal) ||
            !string.Equals(loaded.Checkpoint.Version, expected.Version, StringComparison.Ordinal))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion repository active checkpoint cached load identity did not match the active version.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_CACHED_LOAD_IDENTITY_MISMATCH");
        }

        if (expected.CheckpointContainsRealTrainedWeights &&
            !DesignConversionMachine.CheckpointHasRealLearnedWeights(loaded.Checkpoint))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion repository active checkpoint cached load did not contain expected learned weights.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_CACHED_LOAD_REAL_WEIGHTS_MISMATCH");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static async Task<OperationResult<RepositoryStreamBytes>> ReadAllBytesAndHashAsync(
        Stream stream,
        CancellationToken ct)
    {
        byte[]? buffer = null;
        try
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
                if (stream.Length > MaximumCheckpointByteCount)
                    return CheckpointTooLarge();
            }

            var initialCapacity = stream.CanSeek
                ? checked((int)stream.Length)
                : 0;
            using var memory = new MemoryStream(initialCapacity);
            using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            buffer = ArrayPool<byte>.Shared.Rent(RepositoryReadBufferSize);
            long totalBytes = 0;
            while (true)
            {
                var read = await stream.ReadAsync(
                    buffer.AsMemory(0, RepositoryReadBufferSize),
                    ct).ConfigureAwait(false);
                if (read == 0)
                    break;
                if (read < 0 || read > RepositoryReadBufferSize)
                {
                    return OperationResult<RepositoryStreamBytes>.Fail(
                        "DesignConversion repository active checkpoint stream could not be read.",
                        "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_STREAM_READ_FAILED");
                }
                if (totalBytes + read > MaximumCheckpointByteCount)
                    return CheckpointTooLarge();

                hasher.AppendData(buffer, 0, read);
                memory.Write(buffer, 0, read);
                totalBytes += read;
            }

            var bytes = memory.ToArray();
            var sha256 = Convert.ToHexString(hasher.GetHashAndReset()).ToLowerInvariant();
            return OperationResult<RepositoryStreamBytes>.Ok(new RepositoryStreamBytes(bytes, sha256));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<RepositoryStreamBytes>();
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return OperationResult<RepositoryStreamBytes>.Fail(
                "DesignConversion repository active checkpoint stream could not be read.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_STREAM_READ_FAILED");
        }
        finally
        {
            if (buffer is not null)
                ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }

    private static OperationResult<RepositoryStreamBytes> CheckpointTooLarge()
        => OperationResult<RepositoryStreamBytes>.Fail(
            "DesignConversion repository active checkpoint exceeds the allowed size.",
            "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_CHECKPOINT_TOO_LARGE");

    private static async Task<OperationResult<string>> HashFileAsync(string path, CancellationToken ct)
    {
        try
        {
            await using var stream = File.OpenRead(path);
            var hash = await SHA256.HashDataAsync(stream, ct).ConfigureAwait(false);
            return OperationResult<string>.Ok(Convert.ToHexString(hash).ToLowerInvariant());
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<string>();
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return OperationResult<string>.Fail(
                "DesignConversion repository active checkpoint cache could not be read.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_CACHE_READ_FAILED");
        }
    }

    private OperationResult<string> ValidateServerBaseUrl(string serverBaseUrl)
    {
        var trimmed = serverBaseUrl?.Trim() ?? string.Empty;
        if (trimmed.Length == 0 ||
            trimmed.Any(ch => char.IsControl(ch) || char.IsWhiteSpace(ch) || ch == '\\') ||
            !TryGetRawAbsoluteUrlPath(trimmed, out var rawPath) ||
            !HasSafePath(rawPath) ||
            !Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !HasSafeHost(uri) ||
            !HasSafePath(uri.AbsolutePath) ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment))
        {
            return OperationResult<string>.Fail(
                "DesignConversion repository active checkpoint manifest server base URL is invalid.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_SERVER_BASE_URL_INVALID");
        }

        return OperationResult<string>.Ok(uri.AbsoluteUri.TrimEnd('/'));
    }

    private static bool TryGetRawAbsoluteUrlPath(string rawUrl, out string rawPath)
    {
        rawPath = string.Empty;
        var schemeSeparator = rawUrl.IndexOf("://", StringComparison.Ordinal);
        if (schemeSeparator <= 0)
            return false;

        var authorityStart = schemeSeparator + 3;
        if (authorityStart >= rawUrl.Length)
            return false;

        var suffixStart = rawUrl.IndexOfAny(['/', '?', '#'], authorityStart);
        if (suffixStart < 0)
        {
            rawPath = "/";
            return true;
        }

        if (rawUrl[suffixStart] is '?' or '#')
        {
            rawPath = "/";
            return true;
        }

        var suffixEnd = rawUrl.IndexOfAny(['?', '#'], suffixStart);
        rawPath = suffixEnd < 0
            ? rawUrl[suffixStart..]
            : rawUrl[suffixStart..suffixEnd];
        return true;
    }

    private OperationResult<bool> ValidateUnitEndpoint(
        string endpoint,
        string serverBaseUrl,
        string expectedUnitId)
    {
        var trimmed = endpoint?.Trim() ?? string.Empty;
        if (trimmed.Length == 0 ||
            !string.Equals(trimmed, endpoint, StringComparison.Ordinal) ||
            !HasCanonicalUnitEndpoint(trimmed, expectedUnitId))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion repository active checkpoint manifest endpoint is invalid.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_UNIT_ENDPOINT_INVALID");
        }

        var relativeEndpoint = trimmed[1..];
        if (!Uri.TryCreate(serverBaseUrl.TrimEnd('/') + "/", UriKind.Absolute, out var baseUri) ||
            !Uri.TryCreate(baseUri, relativeEndpoint, out var remoteUri) ||
            !string.Equals(remoteUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !HasSafeHost(remoteUri) ||
            !string.IsNullOrEmpty(remoteUri.UserInfo) ||
            !string.IsNullOrEmpty(remoteUri.Query) ||
            !string.IsNullOrEmpty(remoteUri.Fragment))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion repository active checkpoint manifest endpoint is invalid.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_UNIT_ENDPOINT_INVALID");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool HasCanonicalUnitEndpoint(string endpoint, string expectedUnitId)
    {
        const string unitEndpointPrefix = "/models/";
        if (string.IsNullOrEmpty(expectedUnitId) ||
            !endpoint.StartsWith(unitEndpointPrefix, StringComparison.Ordinal) ||
            endpoint.StartsWith("//", StringComparison.Ordinal) ||
            endpoint.IndexOfAny(['?', '#']) >= 0 ||
            endpoint.Any(ch => char.IsControl(ch) || char.IsWhiteSpace(ch) || ch == '\\'))
        {
            return false;
        }

        var encodedIdentity = endpoint[unitEndpointPrefix.Length..];
        if (encodedIdentity.Length == 0 ||
            encodedIdentity.Contains('/') ||
            !HasStrictPercentTriplets(encodedIdentity))
        {
            return false;
        }

        string decodedIdentity;
        try
        {
            decodedIdentity = Uri.UnescapeDataString(encodedIdentity);
        }
        catch (UriFormatException)
        {
            return false;
        }

        if (!string.Equals(decodedIdentity, expectedUnitId, StringComparison.Ordinal) ||
            HasDecodedTraversalComponent(decodedIdentity))
        {
            return false;
        }

        // Закодированная identity не является файловым путём: slash/backslash
        // допустимы только внутри одного percent-encoded сегмента, а не как raw route.
        var canonicalEndpoint = unitEndpointPrefix + Uri.EscapeDataString(expectedUnitId);
        return string.Equals(endpoint, canonicalEndpoint, StringComparison.Ordinal);
    }

    private static bool HasStrictPercentTriplets(string value)
    {
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] != '%')
                continue;

            if (index > value.Length - 3 ||
                HexDigitValue(value[index + 1]) < 0 ||
                HexDigitValue(value[index + 2]) < 0)
            {
                return false;
            }

            index += 2;
        }

        return true;
    }

    private static bool HasDecodedTraversalComponent(string decodedIdentity)
        => decodedIdentity
            .Split(['/', '\\'], StringSplitOptions.None)
            .Any(segment => segment is "." or "..");

    private static bool HasSafeHost(Uri uri)
    {
        var host = uri.IdnHost;
        if (string.IsNullOrWhiteSpace(host) || host.Length > 253)
            return false;

        return uri.HostNameType switch
        {
            UriHostNameType.Dns => host
                .Split('.', StringSplitOptions.None)
                .All(label => label.Length is > 0 and <= 63 &&
                              char.IsAsciiLetterOrDigit(label[0]) &&
                              char.IsAsciiLetterOrDigit(label[^1]) &&
                              label.All(ch => char.IsAsciiLetterOrDigit(ch) || ch == '-')),
            UriHostNameType.IPv4 => host.All(ch => char.IsAsciiDigit(ch) || ch == '.'),
            UriHostNameType.IPv6 => host.All(ch => char.IsAsciiHexDigit(ch) || ch is ':' or '.'),
            _ => false
        };
    }

    private static bool HasSafePath(string path)
    {
        const int maxDecodeLayers = 8;
        var currentLayer = path;
        for (var decodeCount = 0; ; decodeCount++)
        {
            if (!TryValidatePathLayer(currentLayer, out var hasPercentEscapes))
                return false;
            if (!hasPercentEscapes)
                return true;
            if (decodeCount >= maxDecodeLayers)
                return false;

            string decodedLayer;
            try
            {
                decodedLayer = Uri.UnescapeDataString(currentLayer);
            }
            catch (UriFormatException)
            {
                return false;
            }

            if (string.Equals(decodedLayer, currentLayer, StringComparison.Ordinal))
                return false;

            currentLayer = decodedLayer;
        }
    }

    private static bool TryValidatePathLayer(string path, out bool hasPercentEscapes)
    {
        hasPercentEscapes = false;
        if (path.Any(ch => char.IsControl(ch) || char.IsWhiteSpace(ch) || ch == '\\') ||
            path.Split('/', StringSplitOptions.None).Any(segment => segment is "." or ".."))
        {
            return false;
        }

        for (var index = 0; index < path.Length; index++)
        {
            if (path[index] != '%')
                continue;

            hasPercentEscapes = true;
            if (index > path.Length - 3)
                return false;

            var high = HexDigitValue(path[index + 1]);
            var low = HexDigitValue(path[index + 2]);
            if (high < 0 || low < 0)
                return false;

            var decodedByte = (high << 4) | low;
            if (decodedByte <= 0x20 ||
                decodedByte == 0x7f ||
                decodedByte is 0x2f or 0x5c)
            {
                return false;
            }

            index += 2;
        }

        return true;
    }

    private static int HexDigitValue(char value)
        => value switch
        {
            >= '0' and <= '9' => value - '0',
            >= 'a' and <= 'f' => value - 'a' + 10,
            >= 'A' and <= 'F' => value - 'A' + 10,
            _ => -1
        };

    private static string? NormalizeSha256(string? value)
    {
        if (value is null)
            return null;

        var trimmed = value.Trim();
        if (trimmed.Length != 64 || !trimmed.All(char.IsAsciiHexDigit))
            return null;

        return trimmed.ToLowerInvariant();
    }

    private static bool IsSameSha256(string? left, string? right)
    {
        var normalizedLeft = NormalizeSha256(left);
        var normalizedRight = NormalizeSha256(right);
        return normalizedLeft is not null &&
               normalizedRight is not null &&
               string.Equals(normalizedLeft, normalizedRight, StringComparison.Ordinal);
    }

    private static string EnsureTrailingDirectorySeparator(string value)
        => value.EndsWith(Path.DirectorySeparatorChar) || value.EndsWith(Path.AltDirectorySeparatorChar)
            ? value
            : value + Path.DirectorySeparatorChar;

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                File.Delete(path);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            // Best-effort cleanup only for this call's unique temp file.
        }
    }

    private static bool IsNonFatalException(Exception exception)
    {
        var pending = new Stack<Exception>();
        var visited = new HashSet<Exception>(ReferenceEqualityComparer.Instance);
        pending.Push(exception);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current))
                continue;

            if (current is OutOfMemoryException
                or StackOverflowException
                or AccessViolationException
                or AppDomainUnloadedException
                or BadImageFormatException
                || string.Equals(
                    current.GetType().FullName,
                    "System.Threading.ThreadAbortException",
                    StringComparison.Ordinal))
            {
                return false;
            }

            if (current is AggregateException aggregate)
            {
                foreach (var inner in aggregate.InnerExceptions)
                    pending.Push(inner);
            }
            else if (current.InnerException is not null)
            {
                pending.Push(current.InnerException);
            }
        }

        return true;
    }

    private static OperationResult<T> Cancelled<T>()
        => OperationResult<T>.Fail(
            "DesignConversion repository active checkpoint load was cancelled.",
            "CANCELLED");

    private static OperationResult<DesignConversionMachineLoadResult> FailLoad(string message, string code)
        => OperationResult<DesignConversionMachineLoadResult>.Fail(message, code);

    private static OperationResult<LocatedRepositoryUnit> FailLocated(string message, string code)
        => OperationResult<LocatedRepositoryUnit>.Fail(message, code);

    private static OperationResult<LoadedRepositoryUnit> FailLoaded(string message, string code)
        => OperationResult<LoadedRepositoryUnit>.Fail(message, code);

    private static string PreserveSafeErrorCode(string? errorCode, string fallback)
    {
        if (string.IsNullOrWhiteSpace(errorCode) ||
            errorCode.Length > 128 ||
            errorCode.Any(ch => !char.IsAsciiLetterOrDigit(ch) && ch is not '_' and not '-') ||
            ContainsForbiddenDiagnosticLeakage(errorCode))
        {
            return fallback;
        }

        return errorCode;
    }

    private static bool ContainsForbiddenDiagnosticLeakage(string value)
        => value.Contains("exception", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("credential", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("password", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("api_key", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("apikey", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("token", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("private", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("payload", StringComparison.OrdinalIgnoreCase);

    private static string HashRef(string prefix, string value)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();
        return $"{prefix}-{hash[..16]}";
    }

    private sealed record RuntimeUnitSpec(string UnitId, string UnitRole);

    private sealed record ValidatedActiveVersion(
        string ActivationRef,
        string Version,
        string MachineId,
        string DomainTag,
        string CheckpointSha256,
        bool CheckpointContainsRealTrainedWeights,
        string LocalFallbackPath)
    {
        public string ActivationRefHash
            => HashRef("activation-ref", ActivationRef)[^16..];
    }

    private sealed record LocatedRepositoryUnit(
        RuntimeUnitSpec Unit,
        RemoteUnitManifest Manifest,
        string ServerBaseUrl);

    private sealed record LoadedRepositoryUnit(
        RuntimeUnitSpec Unit,
        byte[] Bytes,
        string Sha256);

    private sealed record RepositoryStreamBytes(byte[] Bytes, string Sha256);

    private sealed record VerifiedRepositoryBytes(byte[] Bytes, string Sha256);
}
