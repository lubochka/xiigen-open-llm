// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Security.Cryptography;
using AgentInfrastructure;
using DesignConversionDomain;

public interface IDesignConversionModelServerArtifactRefResolver
{
    OperationResult<bool> Validate(
        DesignConversionModelServerArtifactRef artifact,
        string subject);
}

public sealed class DesignConversionServerOwnedLocalArtifactRefResolver : IDesignConversionModelServerArtifactRefResolver
{
    public const long DefaultMaxArtifactBytes = 64L * 1024L * 1024L;
    public const string Boundary = "server_owned_local_artifact_root_png_proof";

    private readonly string _artifactRoot;
    private readonly string _artifactRootWithSeparator;
    private readonly long _maxArtifactBytes;
    private readonly StringComparison _pathComparison;

    public DesignConversionServerOwnedLocalArtifactRefResolver(
        string artifactRoot,
        long maxArtifactBytes = DefaultMaxArtifactBytes)
    {
        if (string.IsNullOrWhiteSpace(artifactRoot))
            throw new ArgumentException("DesignConversion server-owned artifact root is required.", nameof(artifactRoot));
        if (maxArtifactBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxArtifactBytes), "DesignConversion server-owned artifact max bytes must be positive.");

        _artifactRoot = TrimTrailingSeparators(Path.GetFullPath(artifactRoot));
        _artifactRootWithSeparator = EnsureTrailingSeparator(_artifactRoot);
        _maxArtifactBytes = maxArtifactBytes;
        _pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
    }

    public OperationResult<bool> Validate(
        DesignConversionModelServerArtifactRef artifact,
        string subject)
    {
        if (artifact is null)
            return Reject($"{subject} server-owned local artifact proof is required.");
        if (artifact.Width <= 0 || artifact.Height <= 0)
            return Reject($"{subject} server-owned local artifact proof must include positive dimensions.");

        var path = ResolveLocalFilePath(artifact.Uri);
        if (!path.IsSuccess)
            return path.MapError<bool>();

        var fullPath = path.Value!;
        if (!IsUnderRoot(fullPath))
            return Reject($"{subject} server-owned local artifact proof path escaped the configured artifact root.");

        var artifactBytes = ReadBoundedRegularFileSnapshot(fullPath, subject);
        if (!artifactBytes.IsSuccess)
            return artifactBytes.MapError<bool>();

        var expectedSha = NormalizeExpectedSha256(artifact.Sha256);
        if (!expectedSha.IsSuccess)
            return expectedSha.MapError<bool>();

        var actualSha = Sha256Bytes(artifactBytes.Value!);
        if (!string.Equals(expectedSha.Value!, actualSha, StringComparison.OrdinalIgnoreCase))
            return Reject($"{subject} server-owned local artifact proof SHA-256 did not match the file bytes.");

        var png = InspectPngSnapshot(artifactBytes.Value!, subject);
        if (!png.IsSuccess)
            return png.MapError<bool>();

        var pngEvidence = png.Value!;
        var dimensions = ValidatePngDimensions(artifact, pngEvidence, subject);
        if (!dimensions.IsSuccess)
            return dimensions;

        if (!pngEvidence.HasNonBlankPixels)
            return Reject($"{subject} server-owned local artifact proof must contain nonblank PNG pixels.");

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidatePngDimensions(
        DesignConversionModelServerArtifactRef artifact,
        DesignConversionBrowserPngEvidence png,
        string subject)
    {
        var metadata = artifact.Metadata ?? new Dictionary<string, string>();
        var hasViewportWidth = HasMetadataKey(metadata, "viewport_width");
        var hasViewportHeight = HasMetadataKey(metadata, "viewport_height");
        if (hasViewportWidth != hasViewportHeight)
            return Reject("DesignConversion server-owned local artifact proof viewport dimensions must be a complete pair.");
        if (hasViewportWidth &&
            (!TryMetadataPositiveInt(metadata, "viewport_width", out var viewportWidth) ||
             !TryMetadataPositiveInt(metadata, "viewport_height", out var viewportHeight) ||
             viewportWidth != artifact.Width ||
             viewportHeight != artifact.Height))
        {
            return Reject("DesignConversion server-owned local artifact proof viewport dimensions did not match the declared logical viewport.");
        }

        var hasPngWidth = HasMetadataKey(metadata, "png_width");
        var hasPngHeight = HasMetadataKey(metadata, "png_height");
        if (hasPngWidth != hasPngHeight)
            return Reject("DesignConversion server-owned local artifact proof PNG dimensions must be a complete pair.");

        var declaredPngWidth = artifact.Width;
        var declaredPngHeight = artifact.Height;
        if (hasPngWidth)
        {
            if (!TryMetadataPositiveInt(metadata, "png_width", out declaredPngWidth) ||
                !TryMetadataPositiveInt(metadata, "png_height", out declaredPngHeight))
            {
                return Reject("DesignConversion server-owned local artifact proof PNG dimensions must be positive integers.");
            }
        }

        if (HasMetadataKey(metadata, DesignConversionRenderResolutionFactory.DeviceScaleFactorMetadataKey))
        {
            var scale = double.TryParse(
                metadata[DesignConversionRenderResolutionFactory.DeviceScaleFactorMetadataKey],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var parsedScale)
                ? parsedScale
                : double.NaN;
            var physical = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
                new RenderQaResolution("server-owned-artifact-proof", artifact.Width, artifact.Height, scale),
                DesignConversionModelServerArtifactRefGuard.ErrorCode,
                subject + " server-owned local artifact proof");
            if (!physical.IsSuccess)
                return physical.MapError<bool>();
            if (declaredPngWidth != physical.Value!.PhysicalWidth ||
                declaredPngHeight != physical.Value.PhysicalHeight)
            {
                return Reject("DesignConversion server-owned local artifact proof PNG dimensions did not match the declared logical viewport and device scale factor.");
            }
        }
        else if (declaredPngWidth != artifact.Width || declaredPngHeight != artifact.Height)
        {
            return Reject("DesignConversion server-owned local artifact proof PNG dimensions did not match the declared viewport.");
        }

        if (png.PngWidth != declaredPngWidth || png.PngHeight != declaredPngHeight)
        {
            return Reject(
                "DesignConversion server-owned local artifact proof dimensions did not match the declared PNG proof: " +
                $"{png.PngWidth.ToString(CultureInfo.InvariantCulture)}x{png.PngHeight.ToString(CultureInfo.InvariantCulture)} != " +
                $"{declaredPngWidth.ToString(CultureInfo.InvariantCulture)}x{declaredPngHeight.ToString(CultureInfo.InvariantCulture)}.");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool HasMetadataKey(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.ContainsKey(key);

    private static bool TryMetadataPositiveInt(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        out int parsed)
    {
        parsed = 0;
        return metadata.TryGetValue(key, out var value) &&
               !string.IsNullOrWhiteSpace(value) &&
               int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) &&
               parsed > 0;
    }

    private OperationResult<string> ResolveLocalFilePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return RejectString("DesignConversion server-owned local artifact proof URI is required.");

        try
        {
            var text = value.Trim();
            if (ContainsTraversalSegment(text) || ContainsTraversalSegment(Uri.UnescapeDataString(text)))
                return RejectString("DesignConversion server-owned local artifact proof must not contain traversal segments.");
            if (IsNetworkOrDevicePath(text))
                return RejectString("DesignConversion server-owned local artifact proof must not reference UNC, network, or device paths.");

            if (Path.IsPathFullyQualified(text))
                return OperationResult<string>.Ok(Path.GetFullPath(text));

            if (!Uri.TryCreate(text, UriKind.Absolute, out var uri))
                return RejectString("DesignConversion server-owned local artifact proof must be an absolute local file reference.");

            if (!uri.IsFile)
                return RejectString("DesignConversion server-owned local artifact proof only accepts file refs from the configured artifact root.");
            if (uri.IsUnc || !string.IsNullOrWhiteSpace(uri.Host))
                return RejectString("DesignConversion server-owned local artifact proof must not reference a network file path.");

            var localPath = uri.LocalPath;
            if (IsNetworkOrDevicePath(localPath))
                return RejectString("DesignConversion server-owned local artifact proof must not reference UNC, network, or device paths.");

            return OperationResult<string>.Ok(Path.GetFullPath(localPath));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or UriFormatException)
        {
            return RejectString("DesignConversion server-owned local artifact proof path is invalid.");
        }
    }

    private OperationResult<byte[]> ReadBoundedRegularFileSnapshot(string fullPath, string subject)
    {
        try
        {
            using var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                FileOptions.SequentialScan);

            var regular = ValidateRegularNonReparseFile(fullPath);
            if (!regular.IsSuccess)
                return regular.MapError<byte[]>();

            var read = ReadAtMostConfiguredBytes(stream, subject);
            if (!read.IsSuccess)
                return read;

            return read;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            return RejectBytes($"{subject} server-owned local artifact proof file was not found.");
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException)
        {
            return RejectBytes($"{subject} server-owned local artifact proof file could not be read.");
        }
        catch (Exception ex) when (ex is IOException or NotSupportedException or ArgumentException or PathTooLongException)
        {
            return RejectBytes($"{subject} server-owned local artifact proof file was not a stable readable regular file.");
        }
    }

    private OperationResult<byte[]> ReadAtMostConfiguredBytes(Stream stream, string subject)
    {
        var chunk = new byte[81920];
        using var bytes = new MemoryStream();

        while (true)
        {
            var read = stream.Read(chunk, 0, chunk.Length);
            if (read == 0)
                break;

            if (bytes.Length + read > _maxArtifactBytes)
            {
                return RejectBytes(
                    $"{subject} server-owned local artifact proof exceeds the configured size bound: " +
                    $">{_maxArtifactBytes.ToString(CultureInfo.InvariantCulture)} bytes.");
            }

            bytes.Write(chunk, 0, read);
        }

        return OperationResult<byte[]>.Ok(bytes.ToArray());
    }

    private OperationResult<bool> ValidateRegularNonReparseFile(string fullPath)
    {
        var fileInfo = new FileInfo(fullPath);
        if ((fileInfo.Attributes & FileAttributes.Directory) != 0)
            return Reject("DesignConversion server-owned local artifact proof must reference a regular file.");
        if ((fileInfo.Attributes & FileAttributes.ReparsePoint) != 0)
            return Reject("DesignConversion server-owned local artifact proof must not be a symlink or reparse-point file.");

        var parent = fileInfo.Directory;
        if (parent is null)
            return Reject("DesignConversion server-owned local artifact proof parent directory is invalid.");

        var root = new DirectoryInfo(_artifactRoot);
        if (root.Exists && (root.Attributes & FileAttributes.ReparsePoint) != 0)
            return Reject("DesignConversion server-owned artifact root must not be a symlink or reparse point.");

        var relativeParent = Path.GetRelativePath(_artifactRoot, parent.FullName);
        var current = _artifactRoot;
        foreach (var segment in relativeParent.Split(
                     [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment is ".")
                continue;
            if (segment is "..")
                return Reject("DesignConversion server-owned local artifact proof path escaped the configured artifact root.");

            current = Path.Combine(current, segment);
            var directory = new DirectoryInfo(current);
            if (!directory.Exists)
                return Reject("DesignConversion server-owned local artifact proof parent directory was not found.");
            if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
                return Reject("DesignConversion server-owned local artifact proof parent directory must not be a symlink or reparse point.");
        }

        return OperationResult<bool>.Ok(true);
    }

    private OperationResult<string> NormalizeExpectedSha256(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return RejectString("DesignConversion server-owned local artifact proof SHA-256 is required.");

        var trimmed = value.Trim();
        var sha = trimmed.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)
            ? trimmed["sha256:".Length..]
            : trimmed;
        if (sha.Length != 64 || !sha.All(Uri.IsHexDigit))
            return RejectString("DesignConversion server-owned local artifact proof SHA-256 must be a sha256-prefixed 64-hex digest.");

        return OperationResult<string>.Ok(sha.ToLowerInvariant());
    }

    private bool IsUnderRoot(string fullPath)
        => fullPath.Equals(_artifactRoot, _pathComparison) ||
           fullPath.StartsWith(_artifactRootWithSeparator, _pathComparison);

    private static string Sha256Bytes(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static OperationResult<DesignConversionBrowserPngEvidence> InspectPngSnapshot(
        byte[] bytes,
        string subject)
    {
        var snapshotRoot = Path.Combine(Path.GetTempPath(), "design-conversion-server-owned-artifact-proof-snapshots");
        var snapshotPath = Path.Combine(snapshotRoot, $"{Guid.NewGuid():N}.png");

        try
        {
            Directory.CreateDirectory(snapshotRoot);
            File.WriteAllBytes(snapshotPath, bytes);
            var png = DesignConversionBrowserPngEvidenceInspector.Inspect(new Uri(snapshotPath).AbsoluteUri);
            if (!png.IsSuccess)
            {
                return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                    $"{subject} server-owned local artifact proof failed strict PNG inspection: {png.ErrorCode ?? "PNG_INSPECTION_FAILED"}.",
                    DesignConversionModelServerArtifactRefGuard.ErrorCode);
            }

            return png;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
            return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                $"{subject} server-owned local artifact proof could not be inspected from a stable byte snapshot.",
                DesignConversionModelServerArtifactRefGuard.ErrorCode);
        }
        finally
        {
            try
            {
                if (File.Exists(snapshotPath))
                    File.Delete(snapshotPath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
            }
        }
    }

    private static bool ContainsTraversalSegment(string value)
        => value.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries)
            .Any(static segment => segment.Equals("..", StringComparison.Ordinal));

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

    private static OperationResult<bool> Reject(string message)
        => OperationResult<bool>.Fail(message, DesignConversionModelServerArtifactRefGuard.ErrorCode);

    private static OperationResult<string> RejectString(string message)
        => OperationResult<string>.Fail(message, DesignConversionModelServerArtifactRefGuard.ErrorCode);

    private static OperationResult<byte[]> RejectBytes(string message)
        => OperationResult<byte[]>.Fail(message, DesignConversionModelServerArtifactRefGuard.ErrorCode);
}
