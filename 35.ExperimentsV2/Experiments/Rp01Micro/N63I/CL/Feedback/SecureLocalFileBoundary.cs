// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DesignConversionDomain")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("44.DesignConversionDomain")]

namespace ContinuousLearning;

using System.Globalization;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using AgentInfrastructure.Logical;
using Microsoft.Win32.SafeHandles;

/// <summary>
/// Shared local-file boundary used by secret-bearing durable stores.  Only
/// Windows and Linux have opened-handle final-path verification here; every
/// other platform fails closed.
/// </summary>
internal static class SecureLocalFileBoundary
{
    internal static readonly UnixFileMode OwnerDirectoryMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
    internal static readonly UnixFileMode OwnerFileMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite;
    private static readonly UnixFileMode GroupOrOtherAccessMask =
        UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
        UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute;

    internal static bool IsSupportedPlatform()
        => OperatingSystem.IsWindows() || OperatingSystem.IsLinux();

    internal static FileStream OpenOwnerOnlyFile(
        string path,
        FileMode mode,
        FileAccess access,
        FileShare share,
        FileOptions options,
        int bufferSize = 1)
    {
        if (!IsSupportedPlatform())
            throw new PlatformNotSupportedException();
        var streamOptions = new FileStreamOptions
        {
            Mode = mode,
            Access = access,
            Share = share,
            Options = options,
            BufferSize = bufferSize
        };
        // A creation mode is the only place where this owner-only permission can
        // be applied: the runtime rejects UnixCreateMode for a mode that cannot
        // create a new file, so requesting it while merely reopening an existing
        // file would turn every read into an ArgumentException. Reopened files
        // keep the owner-only mode they were created with, and callers still
        // verify it separately through HasOwnerOnlyFileMode.
        if (OperatingSystem.IsLinux() &&
            mode is FileMode.CreateNew or FileMode.Create
                or FileMode.OpenOrCreate or FileMode.Append)
        {
            streamOptions.UnixCreateMode = OwnerFileMode;
        }

        return new FileStream(path, streamOptions);
    }

    internal static void CreateOwnerOnlyDirectory(string path)
    {
        if (!IsSupportedPlatform())
            throw new PlatformNotSupportedException();
        if (OperatingSystem.IsWindows())
            Directory.CreateDirectory(path);
        else
            Directory.CreateDirectory(path, OwnerDirectoryMode);
    }

    internal static void CreateCustodyOwnerOnlyDirectory(string path)
    {
        if (OperatingSystem.IsWindows())
            CreateWindowsCustodyDirectory(path);
        else
            CreateOwnerOnlyDirectory(path);
        if (!HasCustodyOwnerOnlyDirectoryMode(path))
            throw new UnauthorizedAccessException(
                "Custody directory owner boundary could not be established.");
    }

    internal static FileStream OpenCustodyOwnerOnlyFile(
        string path,
        FileMode mode,
        FileAccess access,
        FileShare share,
        FileOptions options,
        int bufferSize = 1)
    {
        var stream = OperatingSystem.IsWindows()
            ? OpenWindowsCustodyFile(path, mode, access, share, options, bufferSize)
            : OpenOwnerOnlyFile(path, mode, access, share, options, bufferSize);
        try
        {
            if (!HasCustodyOwnerOnlyFileMode(path))
                throw new UnauthorizedAccessException(
                    "Custody file owner boundary could not be established.");
            return stream;
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    internal static bool HasOwnerOnlyDirectoryMode(string path)
        => OperatingSystem.IsWindows() ||
           (OperatingSystem.IsLinux() &&
            (File.GetUnixFileMode(path) & GroupOrOtherAccessMask) == 0 &&
            (File.GetUnixFileMode(path) & OwnerDirectoryMode) == OwnerDirectoryMode);

    internal static bool HasOwnerOnlyFileMode(string path)
        => OperatingSystem.IsWindows() ||
           (OperatingSystem.IsLinux() &&
            (File.GetUnixFileMode(path) & GroupOrOtherAccessMask) == 0 &&
            (File.GetUnixFileMode(path) & OwnerFileMode) == OwnerFileMode);

    internal static bool HasCustodyOwnerOnlyDirectoryMode(string path)
        => OperatingSystem.IsWindows()
            ? VerifyWindowsCustodyPath(path, requireDirectory: true)
            : HasOwnerOnlyDirectoryMode(path);

    internal static bool HasCustodyOwnerOnlyFileMode(string path)
        => OperatingSystem.IsWindows()
            ? VerifyWindowsCustodyPath(path, requireDirectory: false)
            : HasOwnerOnlyFileMode(path);

    internal static bool OpenedHandleMatchesRequestedPath(
        SafeFileHandle handle,
        string requestedPath)
    {
        var resolved = ResolveOpenedFinalPath(handle);
        if (resolved is null)
            return false;
        try
        {
            return PathsEqual(Path.GetFullPath(resolved), requestedPath);
        }
        catch
        {
            return false;
        }
    }

    internal static bool AnyExistingPathSegmentIsReparsePoint(string fullPath)
    {
        string? current = Path.GetFullPath(fullPath);
        while (current is not null)
        {
            try
            {
                if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    return true;
            }
            catch (Exception exception)
                when (exception is FileNotFoundException or DirectoryNotFoundException)
            {
            }

            var parent = Path.GetDirectoryName(
                Path.TrimEndingDirectorySeparator(current));
            if (string.IsNullOrEmpty(parent) || PathsEqual(parent, current))
                break;
            current = parent;
        }
        return false;
    }

    internal static byte[]? ReadBoundedStableBytes(
        FileStream stream,
        long maximumBytes)
    {
        var declaredLength = stream.Length;
        if (declaredLength <= 0 || declaredLength > maximumBytes ||
            declaredLength > int.MaxValue)
        {
            return null;
        }
        var length = checked((int)declaredLength);
        var first = new byte[length];
        stream.Position = 0;
        if (!ReadExactly(stream, first) || stream.ReadByte() >= 0 ||
            stream.Length != declaredLength)
        {
            return null;
        }
        var second = new byte[length];
        stream.Position = 0;
        if (!ReadExactly(stream, second) || stream.ReadByte() >= 0 ||
            stream.Length != declaredLength ||
            !CryptographicOperations.FixedTimeEquals(first, second))
        {
            return null;
        }
        return first;
    }

    internal static bool FlushParentDirectory(string path)
    {
        // Windows callers must use PublishAtomicDurable for a durable rename.
        // Deletion is intentionally only recoverable/idempotent there and does
        // not pretend that a no-op directory flush is durable.
        if (OperatingSystem.IsWindows())
            return false;
        if (!OperatingSystem.IsLinux())
            return false;
        var parent = Path.GetDirectoryName(Path.GetFullPath(path));
        if (string.IsNullOrWhiteSpace(parent))
            return false;
        const int readOnly = 0;
        const int directory = 0x10000;
        var descriptor = Open(parent, readOnly | directory);
        if (descriptor < 0)
            return false;
        try
        {
            return Fsync(descriptor) == 0;
        }
        finally
        {
            _ = Close(descriptor);
        }
    }

    internal static bool PublishAtomicDurable(string sourcePath, string destinationPath)
    {
        // Validate both ordinary names before GetFullPath or the first mutation.
        // Extended names are an implementation detail of the native calls only.
        string nativeSource = string.Empty, nativeDestination = string.Empty;
        if (OperatingSystem.IsWindows() &&
            (!TryGetWindowsNativePath(sourcePath, out nativeSource) ||
             !TryGetWindowsNativePath(destinationPath, out nativeDestination)))
            return false;
        if (!IsSupportedPlatform() ||
            File.Exists(destinationPath) ||
            !string.Equals(
                Path.GetPathRoot(Path.GetFullPath(sourcePath)),
                Path.GetPathRoot(Path.GetFullPath(destinationPath)),
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal))
        {
            return false;
        }
        if (OperatingSystem.IsWindows())
        {
            const uint moveFileWriteThrough = 0x00000008;
            if (!MoveFileExW(nativeSource, nativeDestination, moveFileWriteThrough))
                return false;
            const uint genericRead = 0x80000000;
            const uint genericWrite = 0x40000000;
            const uint readControl = 0x00020000;
            const uint fileReadAttributes = 0x00000080;
            const uint openExisting = 3;
            const uint fileAttributeNormal = 0x00000080;
            const uint openReparsePoint = 0x00200000;
            var attributes = new SecurityAttributes
            {
                Length = Marshal.SizeOf<SecurityAttributes>()
            };
            using var published = CreateFileW(
                nativeDestination,
                genericRead | genericWrite | readControl | fileReadAttributes,
                (uint)FileShare.Read,
                ref attributes,
                openExisting,
                fileAttributeNormal | openReparsePoint,
                IntPtr.Zero);
            return !published.IsInvalid &&
                   VerifyWindowsCustodyHandle(
                       published,
                       destinationPath,
                       requireDirectory: false) &&
                   FlushFileBuffers(published);
        }
        try
        {
            File.Move(sourcePath, destinationPath);
            return FlushParentDirectory(sourcePath) &&
                   FlushParentDirectory(destinationPath);
        }
        catch (IOException)
        {
            return false;
        }
    }

    private static bool ReadExactly(Stream stream, byte[] destination)
    {
        var offset = 0;
        while (offset < destination.Length)
        {
            var read = stream.Read(destination, offset, destination.Length - offset);
            if (read <= 0)
                return false;
            offset += read;
        }
        return true;
    }

    private static string? ResolveOpenedFinalPath(SafeFileHandle handle)
    {
        if (handle.IsInvalid || handle.IsClosed)
            return null;
        if (OperatingSystem.IsWindows())
            return ResolveOpenedFinalPathWindows(handle);
        if (OperatingSystem.IsLinux())
            return ResolveOpenedFinalPathLinux(handle);
        return null;
    }

    private static string? ResolveOpenedFinalPathWindows(SafeFileHandle handle)
    {
        const int maximumPathCharacters = 32768;
        var capacity = 512;
        while (capacity <= maximumPathCharacters)
        {
            var builder = new StringBuilder(capacity);
            var length = GetFinalPathNameByHandleW(
                handle, builder, (uint)builder.Capacity, 0);
            if (length == 0)
                return null;
            if (length < builder.Capacity)
                return NormalizeWindowsFinalPath(builder.ToString());
            if (length >= maximumPathCharacters)
                break;
            capacity = checked((int)length + 1);
        }
        return null;
    }

    private static string? NormalizeWindowsFinalPath(string finalPath)
    {
        const string dosPrefix = @"\\?\";
        const string uncPrefix = @"\\?\UNC\";
        string normalized;
        if (finalPath.StartsWith(uncPrefix, StringComparison.OrdinalIgnoreCase))
            normalized = @"\\" + finalPath[uncPrefix.Length..];
        else if (finalPath.StartsWith(dosPrefix, StringComparison.OrdinalIgnoreCase))
            normalized = finalPath[dosPrefix.Length..];
        else
            return null;
        return Path.IsPathFullyQualified(normalized) ? normalized : null;
    }

    private static string? ResolveOpenedFinalPathLinux(SafeFileHandle handle)
    {
        const int maximumLinkBytes = 64 * 1024;
        var descriptor = handle.DangerousGetHandle().ToInt64();
        if (descriptor < 0)
            return null;
        var descriptorPath = "/proc/self/fd/" +
                             descriptor.ToString(CultureInfo.InvariantCulture);
        var bufferLength = 256;
        while (bufferLength <= maximumLinkBytes)
        {
            var buffer = new byte[bufferLength];
            var read = ReadLink(descriptorPath, buffer, (nuint)buffer.Length);
            if (read < 0)
                return null;
            if (read < buffer.Length)
            {
                var finalPath = Encoding.UTF8.GetString(buffer, 0, (int)read);
                return Path.IsPathFullyQualified(finalPath) &&
                       !finalPath.EndsWith(" (deleted)", StringComparison.Ordinal)
                    ? finalPath
                    : null;
            }
            bufferLength *= 2;
        }
        return null;
    }

    private static bool PathsEqual(string first, string second)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(first)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(second)),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    [SupportedOSPlatform("windows")]
    private static bool TryGetWindowsNativePath(string path, out string nativePath)
    {
        nativePath = string.Empty;
        // Inspect the original components first: normalization must not hide an
        // ambiguous name before the extended namespace disables Win32 parsing.
        if (!IsOrdinaryWindowsPath(path))
            return false;
        try
        {
            var full = Path.GetFullPath(path);
            if (!IsOrdinaryWindowsPath(full))
                return false;
            nativePath = full.StartsWith(@"\\", StringComparison.Ordinal)
                ? @"\\?\UNC\" + full[2..]
                : @"\\?\" + full;
            return true;
        }
        catch (Exception exception) when ((exception is ArgumentException or
                   NotSupportedException or PathTooLongException) &&
                   SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool IsOrdinaryWindowsPath(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return false;
        var ordinary = path.Replace('/', '\\');
        string[] components;
        if (ordinary.Length >= 3 &&
            (ordinary[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z') &&
            ordinary[1] == ':' && ordinary[2] == '\\')
        {
            components = ordinary[3..].Split('\\');
        }
        else if (ordinary.StartsWith(@"\\", StringComparison.Ordinal))
        {
            components = ordinary[2..].Split('\\');
            // UNC needs an ordinary server and share, not a device/extended
            // prefix, empty root component or dot-segment root alias.
            if (components.Length < 2 ||
                components[0] is "" or "." or ".." ||
                components[1] is "" or "." or "..")
                return false;
        }
        else
        {
            return false;
        }

        foreach (var component in components)
        {
            if (component is "" or "." or "..")
                continue;
            if (component[^1] is '.' or ' ' ||
                component.Any(c => char.IsControl(c) || c is '<' or '>' or ':' or '"' or '|' or '?' or '*'))
                return false;
            var dot = component.IndexOf('.');
            var basename = (dot < 0 ? component : component[..dot]).TrimEnd(' ');
            if (basename.Equals("CON", StringComparison.OrdinalIgnoreCase) ||
                basename.Equals("PRN", StringComparison.OrdinalIgnoreCase) ||
                basename.Equals("AUX", StringComparison.OrdinalIgnoreCase) ||
                basename.Equals("NUL", StringComparison.OrdinalIgnoreCase) ||
                basename.Equals("CONIN$", StringComparison.OrdinalIgnoreCase) ||
                basename.Equals("CONOUT$", StringComparison.OrdinalIgnoreCase) ||
                (basename.Length == 4 &&
                 (basename.StartsWith("COM", StringComparison.OrdinalIgnoreCase) ||
                  basename.StartsWith("LPT", StringComparison.OrdinalIgnoreCase)) &&
                 (basename[3] is >= '1' and <= '9' or '¹' or '²' or '³')))
                return false;
        }
        return true;
    }

    private static IOException WindowsIoFailure(string message, int capturedError)
        => capturedError == 0
            ? new IOException(message)
            : new IOException(message, capturedError <= 0 ? capturedError :
                unchecked((int)0x80070000) | (capturedError & 0xffff));

    [SupportedOSPlatform("windows")]
    private static void CreateWindowsCustodyDirectory(string path)
    {
        if (!TryGetWindowsNativePath(path, out var nativePath))
            throw new IOException("Windows custody path representation is invalid.");
        var descriptor = CreateWindowsSecurityDescriptor(requireDirectory: true);
        try
        {
            var attributes = new SecurityAttributes
            {
                Length = Marshal.SizeOf<SecurityAttributes>(),
                SecurityDescriptor = descriptor,
                InheritHandle = false
            };
            if (!CreateDirectoryW(nativePath, ref attributes))
            {
                var error = Marshal.GetLastPInvokeError();
                const int alreadyExists = 183;
                if (error != alreadyExists ||
                    !Directory.Exists(path))
                {
                    throw WindowsIoFailure(
                        "Windows custody directory could not be created.", error);
                }
            }
        }
        finally
        {
            _ = LocalFree(descriptor);
        }
        if (!VerifyWindowsCustodyPath(path, requireDirectory: true))
            throw new UnauthorizedAccessException(
                "Windows custody directory ACL could not be verified.");
    }

    [SupportedOSPlatform("windows")]
    private static FileStream OpenWindowsCustodyFile(
        string path,
        FileMode mode,
        FileAccess access,
        FileShare share,
        FileOptions options,
        int bufferSize)
    {
        if (mode is not FileMode.CreateNew and not FileMode.Open)
            throw new ArgumentOutOfRangeException(nameof(mode));
        if (!TryGetWindowsNativePath(path, out var nativePath))
            throw new IOException("Windows custody path representation is invalid.");
        var descriptor = mode == FileMode.CreateNew
            ? CreateWindowsSecurityDescriptor(requireDirectory: false)
            : IntPtr.Zero;
        try
        {
            var attributes = new SecurityAttributes
            {
                Length = Marshal.SizeOf<SecurityAttributes>(),
                SecurityDescriptor = descriptor,
                InheritHandle = false
            };
            const uint genericRead = 0x80000000;
            const uint genericWrite = 0x40000000;
            const uint readControl = 0x00020000;
            const uint fileReadAttributes = 0x00000080;
            const uint createNew = 1;
            const uint openExisting = 3;
            const uint fileAttributeNormal = 0x00000080;
            const uint openReparsePoint = 0x00200000;
            const uint writeThrough = 0x80000000;
            const uint sequentialScan = 0x08000000;
            const uint randomAccess = 0x10000000;
            var desiredAccess = readControl | fileReadAttributes |
                                (access.HasFlag(FileAccess.Read) ? genericRead : 0) |
                                (access.HasFlag(FileAccess.Write) ? genericWrite : 0);
            var flags = fileAttributeNormal | openReparsePoint |
                        (options.HasFlag(FileOptions.WriteThrough) ? writeThrough : 0) |
                        (options.HasFlag(FileOptions.SequentialScan) ? sequentialScan : 0) |
                        (options.HasFlag(FileOptions.RandomAccess) ? randomAccess : 0);
            var handle = CreateFileW(
                nativePath,
                desiredAccess,
                (uint)share,
                ref attributes,
                mode == FileMode.CreateNew ? createNew : openExisting,
                flags,
                IntPtr.Zero);
            if (handle.IsInvalid)
            {
                var error = Marshal.GetLastPInvokeError();
                handle.Dispose();
                throw WindowsIoFailure("Windows custody file could not be opened.", error);
            }
            try
            {
                if (!VerifyWindowsCustodyHandle(handle, path, requireDirectory: false))
                    throw new UnauthorizedAccessException(
                        "Windows custody file identity or ACL is invalid.");
                return new FileStream(handle, access, bufferSize, isAsync: false);
            }
            catch
            {
                handle.Dispose();
                throw;
            }
        }
        finally
        {
            if (descriptor != IntPtr.Zero)
                _ = LocalFree(descriptor);
        }
    }

    [SupportedOSPlatform("windows")]
    private static bool VerifyWindowsCustodyPath(
        string path,
        bool requireDirectory)
    {
        if (!TryGetWindowsNativePath(path, out var nativePath))
            return false;
        const uint readControl = 0x00020000;
        const uint fileReadAttributes = 0x00000080;
        const uint openExisting = 3;
        const uint fileAttributeNormal = 0x00000080;
        const uint backupSemantics = 0x02000000;
        const uint openReparsePoint = 0x00200000;
        var attributes = new SecurityAttributes
        {
            Length = Marshal.SizeOf<SecurityAttributes>()
        };
        using var handle = CreateFileW(
            nativePath,
            readControl | fileReadAttributes,
            (uint)(FileShare.Read | FileShare.Write | FileShare.Delete),
            ref attributes,
            openExisting,
            fileAttributeNormal | openReparsePoint |
                (requireDirectory ? backupSemantics : 0),
            IntPtr.Zero);
        return !handle.IsInvalid &&
               VerifyWindowsCustodyHandle(handle, path, requireDirectory);
    }

    [SupportedOSPlatform("windows")]
    private static bool VerifyWindowsCustodyHandle(
        SafeFileHandle handle,
        string path,
        bool requireDirectory)
    {
        try
        {
            if (!OpenedHandleMatchesRequestedPath(handle, path) ||
                !GetFileInformationByHandle(handle, out var information))
            {
                return false;
            }
            const uint directoryAttribute = 0x00000010;
            const uint reparseAttribute = 0x00000400;
            if ((information.FileAttributes & reparseAttribute) != 0 ||
                requireDirectory !=
                    ((information.FileAttributes & directoryAttribute) != 0))
            {
                return false;
            }
            return VerifyWindowsHandleSecurity(handle, requireDirectory);
        }
        catch (SystemException)
        {
            return false;
        }
    }

    [SupportedOSPlatform("windows")]
    private static bool VerifyWindowsHandleSecurity(
        SafeFileHandle handle,
        bool requireDirectory)
    {
        const uint ownerSecurityInformation = 0x00000001;
        const uint daclSecurityInformation = 0x00000004;
        const uint seFileObject = 1;
        var status = GetSecurityInfo(
            handle,
            seFileObject,
            ownerSecurityInformation | daclSecurityInformation,
            out _,
            out _,
            out _,
            out _,
            out var descriptor);
        if (status != 0 || descriptor == IntPtr.Zero)
            return false;
        try
        {
            var length = GetSecurityDescriptorLength(descriptor);
            if (length == 0 || length > 64 * 1024)
                return false;
            var bytes = new byte[length];
            Marshal.Copy(descriptor, bytes, 0, checked((int)length));
            var raw = new RawSecurityDescriptor(bytes, 0);
            if (!raw.ControlFlags.HasFlag(ControlFlags.DiscretionaryAclPresent) ||
                !raw.ControlFlags.HasFlag(ControlFlags.DiscretionaryAclProtected) ||
                raw.ControlFlags.HasFlag(ControlFlags.DiscretionaryAclDefaulted) ||
                raw.Owner is not SecurityIdentifier owner ||
                raw.DiscretionaryAcl is null)
            {
                return false;
            }
            using var identity = WindowsIdentity.GetCurrent();
            var current = identity.User;
            if (current is null)
                return false;
            var trusted = RequiredWindowsSids(current)
                .ToDictionary(static sid => sid.Value, StringComparer.OrdinalIgnoreCase);
            if (!trusted.ContainsKey(owner.Value) ||
                raw.DiscretionaryAcl.Count != trusted.Count)
            {
                return false;
            }
            var observed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (GenericAce ace in raw.DiscretionaryAcl)
            {
                if (ace is not CommonAce common ||
                    common.AceQualifier != AceQualifier.AccessAllowed ||
                    common.SecurityIdentifier is null ||
                    common.IsInherited ||
                    common.AccessMask != (int)FileSystemRights.FullControl ||
                    !trusted.ContainsKey(common.SecurityIdentifier.Value) ||
                    !observed.Add(common.SecurityIdentifier.Value))
                {
                    return false;
                }
                var expectedFlags = requireDirectory
                    ? AceFlags.ContainerInherit | AceFlags.ObjectInherit
                    : AceFlags.None;
                if (common.AceFlags != expectedFlags)
                    return false;
            }
            return observed.SetEquals(trusted.Keys);
        }
        finally
        {
            _ = LocalFree(descriptor);
        }
    }

    [SupportedOSPlatform("windows")]
    private static IntPtr CreateWindowsSecurityDescriptor(bool requireDirectory)
    {
        using var identity = WindowsIdentity.GetCurrent();
        var current = identity.User ?? throw new UnauthorizedAccessException(
            "Current Windows custody identity is unavailable.");
        var sddl = BuildWindowsCustodySddl(current, requireDirectory);
        if (!ConvertStringSecurityDescriptorToSecurityDescriptorW(
                sddl,
                1,
                out var descriptor,
                out _) || descriptor == IntPtr.Zero)
        {
            throw new UnauthorizedAccessException(
                "Windows custody security descriptor could not be built.");
        }
        return descriptor;
    }

    [SupportedOSPlatform("windows")]
    internal static string BuildWindowsCustodySddl(
        SecurityIdentifier current,
        bool requireDirectory)
    {
        ArgumentNullException.ThrowIfNull(current);
        var inheritance = requireDirectory ? "OICI" : string.Empty;
        return RequiredWindowsSids(current).Aggregate(
            "D:P",
            (sddl, sid) =>
                sddl + $"(A;{inheritance};FA;;;{sid.Value})");
    }

    [SupportedOSPlatform("windows")]
    private static SecurityIdentifier[] RequiredWindowsSids(SecurityIdentifier current)
        => new[]
            {
                current,
                new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null)
            }
            .DistinctBy(static sid => sid.Value, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    [StructLayout(LayoutKind.Sequential)]
    private struct SecurityAttributes
    {
        internal int Length;
        internal IntPtr SecurityDescriptor;
        [MarshalAs(UnmanagedType.Bool)]
        internal bool InheritHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ByHandleFileInformation
    {
        internal uint FileAttributes;
        internal System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
        internal System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
        internal System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
        internal uint VolumeSerialNumber;
        internal uint FileSizeHigh;
        internal uint FileSizeLow;
        internal uint NumberOfLinks;
        internal uint FileIndexHigh;
        internal uint FileIndexLow;
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateDirectoryW(
        string path,
        ref SecurityAttributes securityAttributes);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern SafeFileHandle CreateFileW(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        ref SecurityAttributes securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandle(
        SafeFileHandle file,
        out ByHandleFileInformation information);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FlushFileBuffers(SafeFileHandle file);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern uint GetSecurityInfo(
        SafeFileHandle handle,
        uint objectType,
        uint securityInformation,
        out IntPtr owner,
        out IntPtr group,
        out IntPtr dacl,
        out IntPtr sacl,
        out IntPtr securityDescriptor);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ConvertStringSecurityDescriptorToSecurityDescriptorW(
        string stringSecurityDescriptor,
        uint stringSdRevision,
        out IntPtr securityDescriptor,
        out uint securityDescriptorSize);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern uint GetSecurityDescriptorLength(IntPtr securityDescriptor);

    [DllImport("kernel32.dll")]
    private static extern IntPtr LocalFree(IntPtr memory);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern uint GetFinalPathNameByHandleW(
        SafeFileHandle file,
        StringBuilder path,
        uint pathLength,
        uint flags);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool MoveFileExW(
        string existingFileName,
        string newFileName,
        uint flags);

    [DllImport("libc", EntryPoint = "readlink", SetLastError = true)]
    private static extern nint ReadLink(
        string path,
        byte[] buffer,
        nuint bufferSize);

    [DllImport("libc", EntryPoint = "open", SetLastError = true)]
    private static extern int Open(string path, int flags);

    [DllImport("libc", EntryPoint = "fsync", SetLastError = true)]
    private static extern int Fsync(int descriptor);

    [DllImport("libc", EntryPoint = "close", SetLastError = true)]
    private static extern int Close(int descriptor);
}
