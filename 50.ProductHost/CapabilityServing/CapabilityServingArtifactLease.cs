// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProductHost.CapabilityServing;

using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32.SafeHandles;

internal sealed class CapabilityServingArtifactLease : IDisposable
{
    private const UnixFileMode PrivateDirectoryMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
    private const UnixFileMode PrivateFileMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite;

    private readonly string _allowedRoot;
    private readonly long _maximumBytes;
    private readonly long _length;
    private string? _sha256;
    private bool _disposed;

    private CapabilityServingArtifactLease(
        string requestedPath,
        string allowedRoot,
        long maximumBytes,
        FileStream stream,
        long length,
        string? sha256)
    {
        RequestedPath = requestedPath;
        _allowedRoot = allowedRoot;
        _maximumBytes = maximumBytes;
        Stream = stream;
        _length = length;
        _sha256 = sha256;
    }

    internal string RequestedPath { get; }
    internal FileStream Stream { get; }
    internal long MaximumBytes => _maximumBytes;
    internal long Length => _length;

    internal static CapabilityServingArtifactLease Open(
        string path,
        string allowedRoot,
        long maximumBytes)
    {
        var lease = OpenDeferred(path, allowedRoot, maximumBytes);
        try
        {
            lease.CompleteVerification();
            return lease;
        }
        catch
        {
            lease.Dispose();
            throw;
        }
    }

    // Open and validate the exact filesystem object without hashing it yet.
    // ProductHost uses this two-stage form so all unique artifact lengths can be
    // admitted under one cumulative budget before any expensive artifact read.
    internal static CapabilityServingArtifactLease OpenDeferred(
        string path,
        string allowedRoot,
        long maximumBytes)
    {
        if (maximumBytes <= 0)
            throw new InvalidDataException("Artifact size boundary is invalid.");

        var requestedPath = Path.GetFullPath(path);
        var canonicalRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(allowedRoot));
        ValidatePathBeforeOpen(requestedPath, canonicalRoot);

        var stream = new FileStream(requestedPath, new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.Read,
            Options = FileOptions.SequentialScan,
            BufferSize = 4096
        });
        try
        {
            ValidateOpenedHandle(stream.SafeFileHandle, requestedPath, canonicalRoot);
            var length = stream.Length;
            if (length <= 0 || length > maximumBytes)
                throw new InvalidDataException("Artifact size boundary was exceeded.");
            ValidatePathBeforeOpen(requestedPath, canonicalRoot);
            ValidateOpenedHandle(stream.SafeFileHandle, requestedPath, canonicalRoot);
            return new CapabilityServingArtifactLease(
                requestedPath,
                canonicalRoot,
                maximumBytes,
                stream,
                length,
                sha256: null);
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    internal void CompleteVerification()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_sha256 is not null)
            return;

        ValidatePathBeforeOpen(RequestedPath, _allowedRoot);
        ValidateOpenedHandle(Stream.SafeFileHandle, RequestedPath, _allowedRoot);
        _sha256 = ComputeBoundedSha256(Stream, _maximumBytes, _length);
        ValidatePathBeforeOpen(RequestedPath, _allowedRoot);
        ValidateOpenedHandle(Stream.SafeFileHandle, RequestedPath, _allowedRoot);
    }

    internal void VerifyUnchanged()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var sha256 = _sha256
            ?? throw new InvalidOperationException(
                "Artifact verification has not been completed.");
        ValidatePathBeforeOpen(RequestedPath, _allowedRoot);
        ValidateOpenedHandle(Stream.SafeFileHandle, RequestedPath, _allowedRoot);
        if (Stream.Length != _length ||
            !CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(sha256),
                Convert.FromHexString(ComputeBoundedSha256(Stream, _maximumBytes, _length))))
        {
            throw new InvalidDataException("Artifact changed while it was being restored.");
        }

        using var reopened = new FileStream(RequestedPath, new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.Read,
            Options = FileOptions.SequentialScan,
            BufferSize = 4096
        });
        ValidateOpenedHandle(reopened.SafeFileHandle, RequestedPath, _allowedRoot);
        if (reopened.Length != _length ||
            !CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(sha256),
                Convert.FromHexString(ComputeBoundedSha256(reopened, _maximumBytes, _length))))
        {
            throw new InvalidDataException("Artifact path identity changed while it was being restored.");
        }
    }

    internal static void ValidatePrivateDirectory(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (OperatingSystem.IsWindows())
        {
            ValidateWindowsOwnerAcl(new DirectoryInfo(fullPath));
            return;
        }

        if (File.GetUnixFileMode(fullPath) != PrivateDirectoryMode)
            throw new UnauthorizedAccessException("Capability-serving directory mode must be 0700.");
    }

    internal static void SetPrivateFileMode(string path)
    {
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(path, PrivateFileMode);
    }

    internal static void ValidatePrivateFile(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (OperatingSystem.IsWindows())
        {
            ValidateWindowsOwnerAcl(new FileInfo(fullPath));
            return;
        }

        if (File.GetUnixFileMode(fullPath) != PrivateFileMode)
            throw new UnauthorizedAccessException("Capability-serving file mode must be 0600.");
    }

    private static void ValidatePathBeforeOpen(string path, string allowedRoot)
    {
        if (!IsInside(path, allowedRoot) ||
            !File.Exists(path) ||
            Directory.Exists(path) ||
            AnyPathSegmentIsReparsePoint(path))
        {
            throw new InvalidDataException("Artifact path is outside its trusted root.");
        }

        ValidateTrustedDirectoryChain(path, allowedRoot);
        ValidatePrivateFile(path);
    }

    private static void ValidateTrustedDirectoryChain(string filePath, string allowedRoot)
    {
        var current = Path.GetDirectoryName(filePath)
            ?? throw new InvalidDataException("Artifact parent directory is unavailable.");
        while (true)
        {
            if (!IsInside(current + Path.DirectorySeparatorChar, allowedRoot) &&
                !PathsEqual(current, allowedRoot))
            {
                throw new InvalidDataException(
                    "Artifact directory escaped its trusted root.");
            }

            ValidatePrivateDirectory(current);
            if (PathsEqual(current, allowedRoot))
                break;
            current = Path.GetDirectoryName(current)
                ?? throw new InvalidDataException(
                    "Artifact directory chain is incomplete.");
        }
    }

    private static void ValidateOpenedHandle(
        SafeFileHandle handle,
        string requestedPath,
        string allowedRoot)
    {
        var finalPath = Path.GetFullPath(ResolveOpenedFinalPath(handle));
        if (!PathsEqual(finalPath, requestedPath) || !IsInside(finalPath, allowedRoot))
            throw new InvalidDataException("Opened artifact identity does not match its trusted path.");
    }

    private static string ComputeBoundedSha256(
        FileStream stream,
        long maximumBytes,
        long expectedLength)
    {
        stream.Position = 0;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[64 * 1024];
        long total = 0;
        while (true)
        {
            var read = stream.Read(buffer, 0, buffer.Length);
            if (read == 0)
                break;
            total = checked(total + read);
            if (total > maximumBytes)
                throw new InvalidDataException("Artifact grew beyond its size boundary.");
            hash.AppendData(buffer, 0, read);
        }

        if (total != expectedLength || stream.Length != expectedLength)
            throw new InvalidDataException("Artifact changed while it was being read.");
        stream.Position = 0;
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    [SupportedOSPlatform("windows")]
    private static void ValidateWindowsOwnerAcl(FileSystemInfo info)
    {
        using var identity = WindowsIdentity.GetCurrent();
        var currentSid = identity.User
            ?? throw new UnauthorizedAccessException("Current Windows owner identity is unavailable.");
        FileSystemSecurity security = info switch
        {
            DirectoryInfo directory => FileSystemAclExtensions.GetAccessControl(directory),
            FileInfo file => FileSystemAclExtensions.GetAccessControl(file),
            _ => throw new InvalidOperationException("Unsupported filesystem object.")
        };
        var owner = security.GetOwner(typeof(SecurityIdentifier)) as SecurityIdentifier
            ?? throw new UnauthorizedAccessException(
                "Capability-serving Windows owner identity is unavailable.");
        if (!owner.Equals(currentSid))
            throw new UnauthorizedAccessException("Capability-serving path is not owned by the current identity.");

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            currentSid.Value,
            new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null).Value,
            new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null).Value
        };
        foreach (FileSystemAccessRule rule in security.GetAccessRules(
                     includeExplicit: true,
                     includeInherited: true,
                     typeof(SecurityIdentifier)))
        {
            if (rule.AccessControlType == AccessControlType.Allow &&
                rule.IdentityReference is SecurityIdentifier sid &&
                !allowed.Contains(sid.Value))
            {
                throw new UnauthorizedAccessException(
                    "Capability-serving Windows ACL grants access outside the owner boundary.");
            }
        }
    }

    private static bool AnyPathSegmentIsReparsePoint(string fullPath)
    {
        string? current = Path.GetFullPath(fullPath);
        while (current is not null)
        {
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                return true;
            var parent = Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(current));
            if (string.IsNullOrEmpty(parent) || PathsEqual(parent, current))
                break;
            current = parent;
        }
        return false;
    }

    private static bool IsInside(string path, string root)
    {
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        var prefix = Path.TrimEndingDirectorySeparator(root) + Path.DirectorySeparatorChar;
        return path.StartsWith(prefix, comparison);
    }

    private static bool PathsEqual(string left, string right)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(left)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(right)),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    private static string ResolveOpenedFinalPath(SafeFileHandle handle)
    {
        if (handle.IsClosed || handle.IsInvalid)
            throw new InvalidDataException("Artifact handle is invalid.");
        if (OperatingSystem.IsWindows())
        {
            var capacity = 512;
            while (capacity <= 32_768)
            {
                var builder = new StringBuilder(capacity);
                var length = GetFinalPathNameByHandleW(
                    handle,
                    builder,
                    (uint)builder.Capacity,
                    0);
                if (length == 0)
                    throw new InvalidDataException("Artifact final path is unavailable.");
                if (length < builder.Capacity)
                {
                    const string dosPrefix = @"\\?\";
                    const string uncPrefix = @"\\?\UNC\";
                    var path = builder.ToString();
                    if (path.StartsWith(uncPrefix, StringComparison.OrdinalIgnoreCase))
                        return @"\\" + path[uncPrefix.Length..];
                    if (path.StartsWith(dosPrefix, StringComparison.OrdinalIgnoreCase))
                        return path[dosPrefix.Length..];
                    throw new InvalidDataException("Artifact final path is invalid.");
                }
                capacity = checked((int)length + 1);
            }
            throw new InvalidDataException("Artifact final path is too long.");
        }

        if (OperatingSystem.IsLinux())
        {
            var descriptor = handle.DangerousGetHandle().ToInt64();
            var descriptorPath = "/proc/self/fd/" +
                                 descriptor.ToString(CultureInfo.InvariantCulture);
            var buffer = new byte[64 * 1024];
            var read = ReadLink(descriptorPath, buffer, (nuint)buffer.Length);
            if (read <= 0 || read >= buffer.Length)
                throw new InvalidDataException("Artifact final path is unavailable.");
            var path = Encoding.UTF8.GetString(buffer, 0, (int)read);
            if (!Path.IsPathFullyQualified(path) ||
                path.EndsWith(" (deleted)", StringComparison.Ordinal))
            {
                throw new InvalidDataException("Artifact final path is invalid.");
            }
            return path;
        }

        throw new PlatformNotSupportedException(
            "Capability serving supports trusted artifact handles only on Windows and Linux.");
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        Stream.Dispose();
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern uint GetFinalPathNameByHandleW(
        SafeFileHandle file,
        [Out] StringBuilder path,
        uint pathCapacity,
        uint flags);

    [DllImport("libc", EntryPoint = "readlink", SetLastError = true)]
    private static extern nint ReadLink(
        string path,
        byte[] buffer,
        nuint bufferSize);
}
