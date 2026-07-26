// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Security.Cryptography;
using AgentInfrastructure;

public sealed record DesignConversionModelServerArtifactUploadRequest(
    string ArtifactId,
    string Kind,
    string MediaType,
    string ExpectedSha256,
    int Width,
    int Height,
    IReadOnlyDictionary<string, string>? Metadata = null)
{
    public string Role => Kind;
}

public sealed record DesignConversionModelServerArtifactUploadReceipt(
    DesignConversionModelServerArtifactRef ArtifactRef,
    long ByteCount,
    string MediaType,
    string Sha256,
    string ArtifactId,
    string Kind,
    int Width,
    int Height);

public sealed class DesignConversionModelServerArtifactUploadStore
{
    public const long DefaultMaxUploadBytes = DesignConversionServerOwnedLocalArtifactRefResolver.DefaultMaxArtifactBytes;
    public const string MetadataInvalidErrorCode = "DESIGN_CONVERSION_ARTIFACT_UPLOAD_METADATA_INVALID";
    public const string OversizeErrorCode = "DESIGN_CONVERSION_ARTIFACT_UPLOAD_TOO_LARGE";
    public const string HashMismatchErrorCode = "DESIGN_CONVERSION_ARTIFACT_UPLOAD_SHA256_MISMATCH";
    public const string PngValidationErrorCode = "DESIGN_CONVERSION_ARTIFACT_UPLOAD_PNG_INVALID";
    public const string IoErrorCode = "DESIGN_CONVERSION_ARTIFACT_UPLOAD_IO_ERROR";
    public const string PublishRaceErrorCode = "DESIGN_CONVERSION_ARTIFACT_UPLOAD_PUBLISH_RACE";

    private const string PngMediaType = "image/png";
    private const string Sha256Prefix = "sha256:";
    private const int BufferSize = 81920;
    private const int MaxLogicalTokenLength = 96;
    private const int PublishGateCount = 64;
    private const int PublishedArtifactValidationAttemptCount = 6;
    private static readonly TimeSpan PublishedArtifactValidationRetryDelay = TimeSpan.FromMilliseconds(25);
    private static readonly StringComparer PublishPathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;
    private static readonly SemaphoreSlim[] PublishGates = CreatePublishGates();
    private readonly string _artifactRoot;
    private readonly string _artifactRootWithSeparator;
    private readonly string _ingressRoot;
    private readonly long _maxBytes;
    private readonly StringComparison _pathComparison;
    private readonly DesignConversionServerOwnedLocalArtifactRefResolver _resolver;

    public DesignConversionModelServerArtifactUploadStore(
        string artifactRoot,
        long maxBytes = DefaultMaxUploadBytes)
    {
        if (maxBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxBytes), "DesignConversion artifact upload max bytes must be positive.");

        _pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        _artifactRoot = NormalizeAndPrepareArtifactRoot(artifactRoot);
        _artifactRootWithSeparator = EnsureTrailingSeparator(_artifactRoot);
        _ingressRoot = Path.Combine(_artifactRoot, "ingress");
        _maxBytes = maxBytes;
        _resolver = new DesignConversionServerOwnedLocalArtifactRefResolver(_artifactRoot, maxBytes);
    }

    public long MaxUploadBytes => _maxBytes;

    public async Task<OperationResult<DesignConversionModelServerArtifactUploadReceipt>> StoreAsync(
        Stream upload,
        DesignConversionModelServerArtifactUploadRequest metadata,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<DesignConversionModelServerArtifactUploadReceipt>.Fail(
                "DesignConversion artifact upload was cancelled.",
                "CANCELLED");
        }

        var validated = ValidateRequest(upload, metadata);
        if (!validated.IsSuccess)
            return validated.MapError<DesignConversionModelServerArtifactUploadReceipt>();

        var request = validated.Value!;
        var finalDirectory = BuildFinalDirectory(request);
        var prepared = PreparePublishDirectory(finalDirectory);
        if (!prepared.IsSuccess)
            return prepared.MapError<DesignConversionModelServerArtifactUploadReceipt>();

        var finalPath = BuildFinalPath(finalDirectory, request);
        var tempPath = Path.Combine(finalDirectory, $"{Guid.NewGuid():N}.tmp");
        var tempCreated = false;

        try
        {
            tempCreated = true;
            var streamed = await StreamToTempFileAsync(upload, tempPath, cancellationToken).ConfigureAwait(false);
            if (!streamed.IsSuccess)
                return streamed.MapError<DesignConversionModelServerArtifactUploadReceipt>();

            var streamedFile = streamed.Value!;
            if (!string.Equals(streamedFile.Sha256, request.ExpectedSha256, StringComparison.Ordinal))
            {
                return OperationResult<DesignConversionModelServerArtifactUploadReceipt>.Fail(
                    "DesignConversion artifact upload SHA-256 did not match the expected digest.",
                    HashMismatchErrorCode);
            }

            var tempArtifact = CreateArtifactRef(tempPath, request, streamedFile.ByteCount, streamedFile.Sha256);
            var tempValidation = _resolver.Validate(tempArtifact, "DesignConversion artifact upload");
            if (!tempValidation.IsSuccess)
            {
                return OperationResult<DesignConversionModelServerArtifactUploadReceipt>.Fail(
                    "DesignConversion artifact upload failed strict server-owned PNG validation.",
                    PngValidationErrorCode);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var published = await PublishTempFileAsync(
                    tempPath,
                    finalPath,
                    request,
                    streamedFile,
                    cancellationToken)
                .ConfigureAwait(false);
            if (published.IsSuccess && !File.Exists(tempPath))
                tempCreated = false;

            return published;
        }
        catch (OperationCanceledException)
        {
            return OperationResult<DesignConversionModelServerArtifactUploadReceipt>.Fail(
                "DesignConversion artifact upload was cancelled.",
                "CANCELLED");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
            return OperationResult<DesignConversionModelServerArtifactUploadReceipt>.Fail(
                "DesignConversion artifact upload could not be stored safely.",
                IoErrorCode);
        }
        finally
        {
            if (tempCreated)
                TryDeleteFile(tempPath);
        }
    }

    private async Task<OperationResult<StreamedUpload>> StreamToTempFileAsync(
        Stream upload,
        string tempPath,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[BufferSize];
        long total = 0;

        using var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        await using (var output = new FileStream(
                         tempPath,
                         FileMode.CreateNew,
                         FileAccess.Write,
                         FileShare.None,
                         BufferSize,
                         FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var remaining = _maxBytes - total;
                var readLength = remaining >= buffer.Length
                    ? buffer.Length
                    : (int)remaining + 1;
                var read = await upload.ReadAsync(buffer.AsMemory(0, readLength), cancellationToken)
                    .ConfigureAwait(false);
                if (read == 0)
                    break;

                if (read > remaining)
                {
                    return OperationResult<StreamedUpload>.Fail(
                        "DesignConversion artifact upload exceeded the configured size limit.",
                        OversizeErrorCode);
                }

                sha256.AppendData(buffer.AsSpan(0, read));
                await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                total += read;
            }

            await output.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        var digest = Convert.ToHexString(sha256.GetHashAndReset()).ToLowerInvariant();
        return OperationResult<StreamedUpload>.Ok(new StreamedUpload(total, $"sha256:{digest}"));
    }

    private async Task<OperationResult<DesignConversionModelServerArtifactUploadReceipt>> PublishTempFileAsync(
        string tempPath,
        string finalPath,
        ValidatedUploadRequest request,
        StreamedUpload streamed,
        CancellationToken cancellationToken)
    {
        var gate = ResolvePublishGate(finalPath);
        var gateAcquired = false;
        try
        {
            await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            gateAcquired = true;

            if (File.Exists(finalPath))
                return await ReconcileExistingPublishedArtifactAsync(finalPath, request, streamed, cancellationToken)
                    .ConfigureAwait(false);

            var movedToFinalPath = false;
            try
            {
                File.Move(tempPath, finalPath, overwrite: false);
                movedToFinalPath = true;
                var validated = await ValidatePublishedArtifactWithRetryAsync(
                        finalPath,
                        request,
                        streamed,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (validated.IsSuccess)
                    return validated;

                TryDeleteFile(finalPath);
                movedToFinalPath = false;
                return validated;
            }
            catch (IOException) when (File.Exists(finalPath))
            {
                return await ReconcileExistingPublishedArtifactAsync(finalPath, request, streamed, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                if (movedToFinalPath)
                    TryDeleteFile(finalPath);
                throw;
            }
        }
        finally
        {
            if (gateAcquired)
                gate.Release();
        }
    }

    private async Task<OperationResult<DesignConversionModelServerArtifactUploadReceipt>> ReconcileExistingPublishedArtifactAsync(
        string finalPath,
        ValidatedUploadRequest request,
        StreamedUpload streamed,
        CancellationToken cancellationToken)
    {
        var existing = await ValidatePublishedArtifactWithRetryAsync(
                finalPath,
                request,
                streamed,
                cancellationToken)
            .ConfigureAwait(false);
        if (existing.IsSuccess)
            return existing;

        return OperationResult<DesignConversionModelServerArtifactUploadReceipt>.Fail(
            "DesignConversion artifact upload publish race could not be reconciled safely.",
            PublishRaceErrorCode);
    }

    private async Task<OperationResult<DesignConversionModelServerArtifactUploadReceipt>> ValidatePublishedArtifactWithRetryAsync(
        string finalPath,
        ValidatedUploadRequest request,
        StreamedUpload streamed,
        CancellationToken cancellationToken)
    {
        OperationResult<DesignConversionModelServerArtifactUploadReceipt>? last = null;
        for (var attempt = 0; attempt < PublishedArtifactValidationAttemptCount; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            last = ValidatePublishedArtifact(finalPath, request, streamed);
            if (last.IsSuccess)
                return last;

            if (attempt + 1 < PublishedArtifactValidationAttemptCount)
            {
                await Task.Delay(PublishedArtifactValidationRetryDelay, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return last!;
    }

    private OperationResult<DesignConversionModelServerArtifactUploadReceipt> ValidatePublishedArtifact(
        string finalPath,
        ValidatedUploadRequest request,
        StreamedUpload streamed)
    {
        try
        {
            var info = new FileInfo(finalPath);
            if (!info.Exists || info.Length != streamed.ByteCount)
            {
                return OperationResult<DesignConversionModelServerArtifactUploadReceipt>.Fail(
                    "DesignConversion artifact upload publish race produced a mismatched artifact.",
                    PublishRaceErrorCode);
            }

            var artifactRef = CreateArtifactRef(finalPath, request, streamed.ByteCount, streamed.Sha256);
            var validated = _resolver.Validate(artifactRef, "DesignConversion artifact upload receipt");
            if (!validated.IsSuccess)
            {
                return OperationResult<DesignConversionModelServerArtifactUploadReceipt>.Fail(
                    "DesignConversion artifact upload receipt failed strict server-owned PNG validation.",
                    PngValidationErrorCode);
            }

            return OperationResult<DesignConversionModelServerArtifactUploadReceipt>.Ok(
                new DesignConversionModelServerArtifactUploadReceipt(
                    ArtifactRef: artifactRef,
                    ByteCount: streamed.ByteCount,
                    MediaType: PngMediaType,
                    Sha256: streamed.Sha256,
                    ArtifactId: request.ArtifactId,
                    Kind: request.Kind,
                    Width: request.Width,
                    Height: request.Height));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
            return OperationResult<DesignConversionModelServerArtifactUploadReceipt>.Fail(
                "DesignConversion artifact upload published artifact could not be validated safely.",
                IoErrorCode);
        }
    }

    private OperationResult<ValidatedUploadRequest> ValidateRequest(
        Stream upload,
        DesignConversionModelServerArtifactUploadRequest? metadata)
    {
        if (upload is null)
        {
            return OperationResult<ValidatedUploadRequest>.Fail(
                "DesignConversion artifact upload stream is required.",
                MetadataInvalidErrorCode);
        }

        if (metadata is null)
        {
            return OperationResult<ValidatedUploadRequest>.Fail(
                "DesignConversion artifact upload metadata is required.",
                MetadataInvalidErrorCode);
        }

        if (!string.Equals(metadata.MediaType?.Trim(), PngMediaType, StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult<ValidatedUploadRequest>.Fail(
                "DesignConversion artifact upload only accepts image/png media.",
                MetadataInvalidErrorCode);
        }

        var artifactId = NormalizeLogicalToken(metadata.ArtifactId, "artifact id");
        if (!artifactId.IsSuccess)
            return artifactId.MapError<ValidatedUploadRequest>();

        var kind = NormalizeLogicalToken(metadata.Kind, "kind");
        if (!kind.IsSuccess)
            return kind.MapError<ValidatedUploadRequest>();

        if (metadata.Width <= 0 || metadata.Height <= 0)
        {
            return OperationResult<ValidatedUploadRequest>.Fail(
                "DesignConversion artifact upload dimensions must be positive.",
                MetadataInvalidErrorCode);
        }

        var sha256 = NormalizeExpectedSha256(metadata.ExpectedSha256);
        if (!sha256.IsSuccess)
            return sha256.MapError<ValidatedUploadRequest>();

        return OperationResult<ValidatedUploadRequest>.Ok(
            new ValidatedUploadRequest(
                artifactId.Value!,
                kind.Value!,
                sha256.Value!,
                metadata.Width,
                metadata.Height));
    }

    private OperationResult<bool> PreparePublishDirectory(string finalDirectory)
    {
        try
        {
            Directory.CreateDirectory(finalDirectory);
            return ValidateDirectoryChain(finalDirectory);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion artifact upload storage directory is not safe.",
                IoErrorCode);
        }
    }

    private OperationResult<bool> ValidateDirectoryChain(string directory)
    {
        var fullDirectory = Path.GetFullPath(directory);
        if (!IsUnderRoot(fullDirectory))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion artifact upload storage directory escaped the configured root.",
                IoErrorCode);
        }

        var root = new DirectoryInfo(_artifactRoot);
        if (!root.Exists || (root.Attributes & FileAttributes.ReparsePoint) != 0)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion artifact upload root is not a safe local directory.",
                IoErrorCode);
        }

        var relative = Path.GetRelativePath(_artifactRoot, fullDirectory);
        var current = _artifactRoot;
        foreach (var segment in relative.Split(
                     [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment is ".")
                continue;
            if (segment is "..")
            {
                return OperationResult<bool>.Fail(
                    "DesignConversion artifact upload storage directory escaped the configured root.",
                    IoErrorCode);
            }

            current = Path.Combine(current, segment);
            var info = new DirectoryInfo(current);
            if (!info.Exists || (info.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                return OperationResult<bool>.Fail(
                    "DesignConversion artifact upload storage directory is not safe.",
                    IoErrorCode);
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    private DesignConversionModelServerArtifactRef CreateArtifactRef(
        string path,
        ValidatedUploadRequest request,
        long byteCount,
        string sha256)
        => new(
            ArtifactId: request.ArtifactId,
            Kind: request.Kind,
            Width: request.Width,
            Height: request.Height,
            Uri: new Uri(path).AbsoluteUri,
            Sha256: sha256,
            Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["media_type"] = PngMediaType,
                ["byte_count"] = byteCount.ToString(CultureInfo.InvariantCulture),
                ["ingress_boundary"] = DesignConversionServerOwnedLocalArtifactRefResolver.Boundary
            });

    private string BuildFinalDirectory(ValidatedUploadRequest request)
        => Path.Combine(_ingressRoot, request.Kind, request.ArtifactId);

    private static string BuildFinalPath(
        string finalDirectory,
        ValidatedUploadRequest request)
        => Path.Combine(
            finalDirectory,
            $"{request.Width.ToString(CultureInfo.InvariantCulture)}x" +
            $"{request.Height.ToString(CultureInfo.InvariantCulture)}-" +
            $"{request.ExpectedSha256[Sha256Prefix.Length..]}.png");

    private static SemaphoreSlim ResolvePublishGate(string finalPath)
    {
        var hash = unchecked((uint)PublishPathComparer.GetHashCode(finalPath));
        return PublishGates[hash % PublishGateCount];
    }

    private static SemaphoreSlim[] CreatePublishGates()
    {
        var gates = new SemaphoreSlim[PublishGateCount];
        for (var index = 0; index < gates.Length; index++)
            gates[index] = new SemaphoreSlim(1, 1);
        return gates;
    }

    private bool IsUnderRoot(string fullPath)
        => fullPath.Equals(_artifactRoot, _pathComparison) ||
           fullPath.StartsWith(_artifactRootWithSeparator, _pathComparison);

    private static OperationResult<string> NormalizeLogicalToken(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return OperationResult<string>.Fail(
                $"DesignConversion artifact upload {fieldName} is required.",
                MetadataInvalidErrorCode);
        }

        var token = value.Trim();
        if (token.Length > MaxLogicalTokenLength ||
            !token.All(static ch => char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_'))
        {
            return OperationResult<string>.Fail(
                $"DesignConversion artifact upload {fieldName} must be a bounded safe logical token.",
                MetadataInvalidErrorCode);
        }

        return OperationResult<string>.Ok(token);
    }

    private static OperationResult<string> NormalizeExpectedSha256(string? value)
    {
        if (value is null ||
            value.Length != Sha256Prefix.Length + 64 ||
            !value.StartsWith(Sha256Prefix, StringComparison.Ordinal))
        {
            return OperationResult<string>.Fail(
                "DesignConversion artifact upload expected SHA-256 must use canonical lowercase sha256 form.",
                MetadataInvalidErrorCode);
        }

        for (var index = Sha256Prefix.Length; index < value.Length; index++)
        {
            var character = value[index];
            if (character is not (>= '0' and <= '9') and not (>= 'a' and <= 'f'))
            {
                return OperationResult<string>.Fail(
                    "DesignConversion artifact upload expected SHA-256 must use canonical lowercase sha256 form.",
                    MetadataInvalidErrorCode);
            }
        }

        return OperationResult<string>.Ok(value);
    }

    private static string NormalizeAndPrepareArtifactRoot(string artifactRoot)
    {
        if (string.IsNullOrWhiteSpace(artifactRoot))
            throw new ArgumentException("DesignConversion artifact upload root is required.", nameof(artifactRoot));

        var trimmed = artifactRoot.Trim();
        if (trimmed.Contains("://", StringComparison.Ordinal) ||
            !Path.IsPathFullyQualified(trimmed) ||
            IsNetworkOrDevicePath(trimmed))
        {
            throw new ArgumentException("DesignConversion artifact upload root must be a local fully qualified directory path.", nameof(artifactRoot));
        }

        try
        {
            var fullPath = TrimTrailingSeparators(Path.GetFullPath(trimmed));
            if (IsNetworkOrDevicePath(fullPath))
                throw new ArgumentException("DesignConversion artifact upload root must be a local directory path.", nameof(artifactRoot));

            Directory.CreateDirectory(fullPath);
            var root = new DirectoryInfo(fullPath);
            if (!root.Exists || (root.Attributes & FileAttributes.ReparsePoint) != 0)
                throw new ArgumentException("DesignConversion artifact upload root must not be a symlink or reparse point.", nameof(artifactRoot));

            return fullPath;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or PathTooLongException)
        {
            throw new ArgumentException("DesignConversion artifact upload root must be a safe local directory path.", nameof(artifactRoot));
        }
    }

    private static bool IsNetworkOrDevicePath(string value)
        => value.StartsWith(@"\\", StringComparison.Ordinal) ||
           value.StartsWith("//", StringComparison.Ordinal) ||
           value.StartsWith(@"\\?\", StringComparison.Ordinal) ||
           value.StartsWith(@"\\.\", StringComparison.Ordinal);

    private static string EnsureTrailingSeparator(string value)
        => value.EndsWith(Path.DirectorySeparatorChar) || value.EndsWith(Path.AltDirectorySeparatorChar)
            ? value
            : value + Path.DirectorySeparatorChar;

    private static string TrimTrailingSeparators(string value)
    {
        var root = Path.GetPathRoot(value) ?? string.Empty;
        var result = value;
        while (result.Length > root.Length &&
               (result.EndsWith(Path.DirectorySeparatorChar) || result.EndsWith(Path.AltDirectorySeparatorChar)))
        {
            result = result[..^1];
        }

        return result;
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }

    private sealed record ValidatedUploadRequest(
        string ArtifactId,
        string Kind,
        string ExpectedSha256,
        int Width,
        int Height);

    private sealed record StreamedUpload(long ByteCount, string Sha256);
}
