// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Buffers.Binary;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32.SafeHandles;

internal enum DesignConversionHandleBoundIoStage
{
    RootSegmentBeforeOpen,
    LeafBeforeOpen,
    MutationLockAcquired,
    BeforeAtomicRename,
    AfterAtomicRenameBeforeDirectoryFlush,
    AfterDurableCommitBeforeBackupCleanup,
    AfterBackupDeleteBeforeDirectoryFlush,
    BeforePublicationBackupRecoveryRename,
    BeforeDelete,
    AfterDeleteMarkBeforeDirectoryFlush,
    BeforeDirectoryMetadataFlush,
    BetweenNamespacePresenceScans,
    AfterDurableDelete
}

internal readonly record struct DesignConversionFileIdentity(
    ulong VolumeSerialNumber,
    ulong FileIdLow,
    ulong FileIdHigh);

internal readonly record struct DesignConversionHandleBoundDestinationExpectation(
    bool MustBeAbsent,
    DesignConversionFileIdentity? ExpectedIdentity,
    long ExpectedLength,
    string? ExpectedSha256)
{
    internal static DesignConversionHandleBoundDestinationExpectation Absent =>
        new(true, null, 0L, null);

    internal static DesignConversionHandleBoundDestinationExpectation Exact(
        DesignConversionFileIdentity identity,
        long length,
        string sha256) => new(false, identity, length, sha256);
}

internal sealed record DesignConversionHandleBoundEntry(
    string Name,
    long Length,
    DesignConversionFileIdentity Identity);

internal sealed record DesignConversionHandleBoundRegularFileSnapshot(
    bool Exists,
    byte[] Bytes,
    string? PhysicalIdentity)
{
    internal static DesignConversionHandleBoundRegularFileSnapshot Missing { get; } =
        new(false, [], null);
}

internal enum DesignConversionHandleBoundDeleteDisposition
{
    AlreadyAbsent,
    NamespaceEntryRemovedDirectoryFlushNotConfirmed,
    DurableDelete
}

internal sealed class DesignConversionHandleBoundSharingViolationException(string message)
    : IOException(message);

internal sealed class DesignConversionHandleBoundTrustException(string message)
    : IOException(message);

internal sealed class DesignConversionHandleBoundConcurrentCreateException(string message)
    : IOException(message);

internal sealed class DesignConversionHandleBoundNotFoundException(string message)
    : IOException(message);

internal sealed class DesignConversionHandleBoundDeleteDispositionException(
    string message,
    DesignConversionHandleBoundDeleteDisposition disposition,
    Exception innerException)
    : IOException(message, innerException)
{
    public DesignConversionHandleBoundDeleteDisposition Disposition { get; } =
        disposition;
}

internal sealed class DesignConversionHandleBoundPostCommitCleanupException(
    string message,
    Exception innerException)
    : IOException(message, innerException);

internal sealed class DesignConversionHandleBoundEnumerationLimitException(string message)
    : IOException(message);

internal sealed class DesignConversionHandleBoundNameCollisionException(string message)
    : IOException(message);

/// <summary>
/// Windows and Linux handle-relative, no-follow storage boundary. All
/// descendants are opened relative to retained directory handles. Windows keeps
/// the stronger kernel handle semantics; Linux uses pinned dirfd/openat,
/// stable device+inode identity, advisory locks, and directory fsync.
/// Unsupported platforms fail closed.
/// </summary>
internal sealed class DesignConversionHandleBoundDirectory : IDisposable
{
    internal const string PublicationBackupSuffix = ".publication-backup";

    private const uint GenericRead = 0x80000000;
    private const uint GenericWrite = 0x40000000;
    private const uint DeleteAccess = 0x00010000;
    private const uint Synchronize = 0x00100000;
    private const uint FileListDirectory = 0x00000001;
    private const uint FileTraverse = 0x00000020;
    private const uint FileReadAttributes = 0x00000080;
    private const uint FileAttributeNormal = 0x00000080;
    private const uint FileAttributeDirectory = 0x00000010;
    private const uint FileAttributeReparsePoint = 0x00000400;
    private const uint FileFlagBackupSemantics = 0x02000000;
    private const uint FileFlagOpenReparsePoint = 0x00200000;
    private const uint OpenExisting = 3;
    private const uint FileOpen = 1;
    private const uint FileCreate = 2;
    private const uint FileOpenIf = 3;
    private const uint FileDirectoryFile = 0x00000001;
    private const uint FileWriteThrough = 0x00000002;
    private const uint FileSynchronousIoNonAlert = 0x00000020;
    private const uint FileNonDirectoryFile = 0x00000040;
    private const uint FileOpenReparsePoint = 0x00200000;
    private const uint ObjectCaseInsensitive = 0x00000040;
    private const uint ObjectDontReparse = 0x00001000;
    private const int StatusNoSuchFile = unchecked((int)0xC000000F);
    private const int StatusObjectNameNotFound = unchecked((int)0xC0000034);
    private const int StatusObjectNameCollision = unchecked((int)0xC0000035);
    private const int StatusObjectPathNotFound = unchecked((int)0xC000003A);
    private const int StatusSharingViolation = unchecked((int)0xC0000043);
    private const int StatusLockNotGranted = unchecked((int)0xC0000055);
    private const int StatusNoMoreFiles = unchecked((int)0x80000006);
    private const int FileIdBothDirectoryInformation = 37;
    private const int FileRenameInformation = 10;
    internal const int DirectoryBufferBytes = 64 * 1024;
    private const int FileIdBothNameOffset = 104;
    private const int LinuxAtCurrentDirectory = -100;
    private const int LinuxOpenReadOnly = 0;
    private const int LinuxOpenReadWrite = 0x0002;
    private const int LinuxOpenCreate = 0x0040;
    private const int LinuxOpenExclusive = 0x0080;
    private const int LinuxOpenDirectory = 0x10000;
    private const int LinuxOpenNoFollow = 0x20000;
    private const int LinuxOpenCloseOnExec = 0x80000;
    private const int LinuxOpenSync = 0x101000;
    private const int LinuxDirectoryOpenFlags =
        LinuxOpenReadOnly | LinuxOpenDirectory | LinuxOpenNoFollow | LinuxOpenCloseOnExec;
    private const int LinuxFileTypeMask = 0xF000;
    private const int LinuxRegularFileType = 0x8000;
    private const int LinuxDirectoryFileType = 0x4000;
    private const int LinuxSymbolicLinkFileType = 0xA000;
    private const int LinuxEntryNotFound = 2;
    private const int LinuxEntryExists = 17;
    private const int LinuxInterrupted = 4;
    private const int LinuxInvalidArgument = 22;
    private const int LinuxFunctionNotImplemented = 38;
    private const int LinuxDirectoryNotEmpty = 39;
    private const int LinuxNotDirectory = 20;
    private const int LinuxIsDirectory = 21;
    private const int LinuxTooManySymbolicLinks = 40;
    private const int LinuxOperationNotSupported = 95;
    private const int LinuxWouldBlock = 11;
    private const int LinuxFlockExclusive = 0x02;
    private const int LinuxFlockNonBlocking = 0x04;
    private const int LinuxFlockUnlock = 0x08;
    private const int LinuxUnlinkRemoveDirectory = 0x200;
    private const int LinuxDirentNameOffset = 19;
    private const int LinuxNameMaxUtf8Bytes = 255;
    private const uint LinuxPermissionBitsMask = 0x1FF;
    private const uint LinuxGroupOrOtherAccessMask = 0x3F;
    private const uint LinuxOwnerDirectoryMode = 0x1C0; // 0700: владелец читает/пишет/заходит.
    private const uint LinuxOwnerFileMode = 0x180; // 0600: секреты читает/пишет только владелец.
    private const long LinuxV9FsMagic = 0x01021997;
    private const uint LinuxRenameNoReplace = 1;

    private readonly string _rootDirectory;
    private readonly IReadOnlyList<SafeFileHandle> _directoryHandles;
    private readonly SafeFileHandle _rootHandle;
    private readonly StorageBackend _backend;
    private readonly Action<DesignConversionHandleBoundIoStage, string>? _beforeIo;
    private int _disposed;

    private DesignConversionHandleBoundDirectory(
        string rootDirectory,
        IReadOnlyList<SafeFileHandle> directoryHandles,
        StorageBackend backend,
        Action<DesignConversionHandleBoundIoStage, string>? beforeIo)
    {
        _rootDirectory = rootDirectory;
        _directoryHandles = directoryHandles;
        _rootHandle = directoryHandles[^1];
        _backend = backend;
        _beforeIo = beforeIo;
    }

    public static DesignConversionHandleBoundDirectory AcquireOrCreate(
        string rootDirectory,
        Action<DesignConversionHandleBoundIoStage, string>? beforeIo)
        => Acquire(rootDirectory, createMissing: true, beforeIo);

    public static DesignConversionHandleBoundDirectory AcquireExisting(
        string rootDirectory,
        Action<DesignConversionHandleBoundIoStage, string>? beforeIo)
        => Acquire(rootDirectory, createMissing: false, beforeIo);

    internal static DesignConversionHandleBoundDirectory AcquireExistingAllowDeleteShare(
        string rootDirectory,
        Action<DesignConversionHandleBoundIoStage, string>? beforeIo)
        => Acquire(
            rootDirectory,
            createMissing: false,
            beforeIo,
            denyDeleteSharingForExistingRoot: false);

    private static DesignConversionHandleBoundDirectory Acquire(
        string rootDirectory,
        bool createMissing,
        Action<DesignConversionHandleBoundIoStage, string>? beforeIo,
        bool denyDeleteSharingForExistingRoot = true)
    {
        if (OperatingSystem.IsWindows())
        {
            return AcquireWindows(
                rootDirectory,
                createMissing,
                beforeIo,
                denyDeleteSharingForExistingRoot);
        }
        if (OperatingSystem.IsLinux())
            return AcquireLinux(rootDirectory, createMissing, beforeIo);

        throw new PlatformNotSupportedException(
            "DesignConversion handle-bound durable storage is supported only on Windows and Linux.");
    }

    internal static SafeFileHandle AcquireLinuxPublicationSerializationLock(
        string logicalLockName,
        CancellationToken cancellationToken)
    {
        EnsureLinuxX64StatLayout();
        var lockRoot = Path.Combine(
            Path.GetTempPath(),
            "xiigen-dpo-runtime-publication-locks");
        EnsureLinuxSymbolicLinkFreePath(lockRoot, includeLeaf: false);
        Directory.CreateDirectory(lockRoot);
        EnsureLinuxSymbolicLinkFreePath(lockRoot, includeLeaf: true);

        var lockDigest = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(logicalLockName)))
            .ToLowerInvariant();
        var lockPath = Path.Combine(lockRoot, $"{lockDigest}.lock");
        var descriptor = OpenLinux(
            lockPath,
            LinuxOpenReadWrite |
            LinuxOpenCreate |
            LinuxOpenNoFollow |
            LinuxOpenCloseOnExec,
            LinuxOwnerFileMode);
        if (descriptor < 0)
        {
            var error = Marshal.GetLastPInvokeError();
            if (error is LinuxTooManySymbolicLinks or LinuxIsDirectory)
            {
                throw new DesignConversionHandleBoundTrustException(
                    "Linux publication lock leaf is not a regular file.");
            }

            throw LinuxIOException("open Linux publication lock", lockPath, error);
        }

        var handle = new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
        try
        {
            ValidateLinuxHandleKind(handle, lockPath, requireDirectory: false);
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (FlockLinux(
                        descriptor,
                        LinuxFlockExclusive | LinuxFlockNonBlocking) == 0)
                {
                    // Это advisory-lock, а не Windows mandatory mutex: он
                    // сериализует наших publisher'ов, а чужие процессы всё равно
                    // ловятся проверкой identity/readback перед публикацией.
                    return handle;
                }

                var error = Marshal.GetLastPInvokeError();
                if (error is not (LinuxWouldBlock or LinuxInterrupted))
                    throw LinuxIOException("acquire Linux publication lock", lockPath, error);
                if (cancellationToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(25)))
                    throw new OperationCanceledException(cancellationToken);
            }
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    internal static bool ReleaseLinuxPublicationSerializationLock(SafeHandle handle)
        => handle is SafeFileHandle fileHandle &&
           FlockLinux(ToFileDescriptor(fileHandle), LinuxFlockUnlock) == 0;

    internal static string ReadLinuxPhysicalDirectoryIdentity(string directory)
    {
        EnsureLinuxX64StatLayout();
        var full = Path.GetFullPath(directory);
        EnsureLinuxSymbolicLinkFreePath(full, includeLeaf: true);
        using var handle = OpenLinuxDirectoryAbsolute(full, out var error);
        if (handle.IsInvalid)
        {
            handle.Dispose();
            if (error is LinuxTooManySymbolicLinks or LinuxNotDirectory)
            {
                throw new DesignConversionHandleBoundTrustException(
                    "Linux physical destination directory is not publishable.");
            }

            throw LinuxIOException("open Linux physical directory identity", full, error);
        }

        var stat = GetLinuxStat(handle, full);
        if ((stat.Mode & LinuxFileTypeMask) != LinuxDirectoryFileType)
        {
            throw new DesignConversionHandleBoundTrustException(
                "Linux physical destination identity is not a directory.");
        }

        // Строка пути не является authority: lock namespace и recheck
        // привязаны к device+inode, чтобы alias/bind-mount не дал второй замок.
        return $"linux:{stat.Device:x16}:{stat.Inode:x16}";
    }

    internal static void EnsureLinuxSymbolicLinkFreePath(
        string path,
        bool includeLeaf)
    {
        EnsureLinuxX64StatLayout();
        var full = Path.GetFullPath(path);
        var current = includeLeaf ? full : Path.GetDirectoryName(full);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (LStatLinux(current, out var stat) == 0)
            {
                if ((stat.Mode & LinuxFileTypeMask) == LinuxSymbolicLinkFileType)
                {
                    throw new DesignConversionHandleBoundTrustException(
                        "Linux publication path contains a symbolic-link authority hop.");
                }
            }
            else
            {
                var error = Marshal.GetLastPInvokeError();
                if (error != LinuxEntryNotFound && error != LinuxNotDirectory)
                    throw LinuxIOException(
                        "inspect Linux path for symbolic links",
                        current,
                        error);
            }

            var parent = Path.GetDirectoryName(
                Path.TrimEndingDirectorySeparator(current));
            if (string.Equals(parent, current, StringComparison.Ordinal))
                break;
            current = parent;
        }
    }

    internal static DesignConversionHandleBoundRegularFileSnapshot
        ReadLinuxRegularFileSnapshotNoFollow(
            string path,
            int maximumBytes,
            CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureLinuxX64StatLayout();
        EnsureLinuxSymbolicLinkFreePath(path, includeLeaf: false);
        var full = Path.GetFullPath(path);
        var descriptor = OpenLinux(
            full,
            LinuxOpenReadOnly | LinuxOpenNoFollow | LinuxOpenCloseOnExec,
            0);
        if (descriptor < 0)
        {
            var error = Marshal.GetLastPInvokeError();
            if (error == LinuxEntryNotFound)
                return DesignConversionHandleBoundRegularFileSnapshot.Missing;
            if (error is LinuxTooManySymbolicLinks or LinuxIsDirectory)
            {
                throw new DesignConversionHandleBoundTrustException(
                    "Linux no-follow snapshot leaf is not a regular file.");
            }

            throw LinuxIOException(
                "open Linux regular file snapshot without following links",
                full,
                error);
        }

        var handle = new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
        try
        {
            var stat = GetLinuxStat(handle, full);
            if ((stat.Mode & LinuxFileTypeMask) != LinuxRegularFileType)
            {
                throw new DesignConversionHandleBoundTrustException(
                    "Linux no-follow snapshot leaf is not a regular file.");
            }

            using var stream = new FileStream(handle, FileAccess.Read);
            if (stream.Length < 0 || stream.Length > maximumBytes)
                throw new IOException("Linux no-follow snapshot exceeds publication bound.");
            var bytes = new byte[checked((int)stream.Length)];
            stream.ReadExactly(bytes);
            cancellationToken.ThrowIfCancellationRequested();
            var afterRead = GetLinuxStat(stream.SafeFileHandle, full);
            if (stream.Length != bytes.Length ||
                afterRead.Device != stat.Device ||
                afterRead.Inode != stat.Inode)
            {
                // Snapshot доверен только пока тот же fd сохраняет device+inode:
                // rollback и сравнение не должны защищать уже подменённый файл.
                throw new DesignConversionHandleBoundTrustException(
                    "Linux no-follow snapshot identity changed during read.");
            }

            return new DesignConversionHandleBoundRegularFileSnapshot(
                true,
                bytes,
                $"linux:{stat.Device:x16}:{stat.Inode:x16}");
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    private static DesignConversionHandleBoundDirectory AcquireWindows(
        string rootDirectory,
        bool createMissing,
        Action<DesignConversionHandleBoundIoStage, string>? beforeIo,
        bool denyDeleteSharingForExistingRoot)
    {
        var fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootDirectory));
        var pathRoot = Path.GetPathRoot(fullRoot)
            ?? throw new InvalidDataException("Handle-bound storage root is not fully qualified.");
        var segments = fullRoot[pathRoot.Length..]
            .Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries);
        var handles = new List<SafeFileHandle>(segments.Length + 1);
        try
        {
            var anchor = OpenWindowsDirectory(pathRoot, out var anchorError);
            if (anchor.IsInvalid)
            {
                anchor.Dispose();
                throw WindowsIOException("open filesystem root anchor", pathRoot, anchorError);
            }

            ValidateWindowsHandle(anchor, pathRoot, requireDirectory: true);
            handles.Add(anchor);
            var parentPath = pathRoot;
            for (var segmentIndex = 0; segmentIndex < segments.Length; segmentIndex++)
            {
                var segment = segments[segmentIndex];
                var isDurableStoreRoot = segmentIndex == segments.Length - 1;
                EnsureLeafName(segment);
                var childPath = Path.Combine(parentPath, segment);
                var parentHandle = handles[^1];
                ValidateWindowsHandle(parentHandle, parentPath, requireDirectory: true);
                DesignConversionFileIdentity? observedIdentity = null;
                var observedHandle = NtOpenWindowsDirectory(
                    parentHandle,
                    segment,
                    FileOpen,
                    requireDurableWriteAccess: createMissing && isDurableStoreRoot,
                    denyDeleteSharing: false,
                    out var observedStatus);
                if (observedStatus >= 0)
                {
                    try
                    {
                        ValidateWindowsHandle(
                            observedHandle,
                            childPath,
                            requireDirectory: true);
                        observedIdentity = GetWindowsIdentity(
                            observedHandle,
                            childPath);
                    }
                    finally
                    {
                        observedHandle.Dispose();
                    }
                }
                else
                {
                    observedHandle.Dispose();
                    if (!createMissing &&
                        observedStatus is StatusNoSuchFile or
                            StatusObjectNameNotFound or
                            StatusObjectPathNotFound)
                    {
                        throw new DesignConversionHandleBoundNotFoundException(
                            "Handle-bound existing directory was not found.");
                    }
                    if (observedStatus is not StatusNoSuchFile and
                        not StatusObjectNameNotFound and
                        not StatusObjectPathNotFound)
                    {
                        throw WindowsNtIOException(
                            "observe root-relative directory before safe open",
                            childPath,
                            observedStatus);
                    }
                }

                beforeIo?.Invoke(
                    DesignConversionHandleBoundIoStage.RootSegmentBeforeOpen,
                    childPath);
                ValidateWindowsHandle(parentHandle, parentPath, requireDirectory: true);

                var childHandle = NtOpenWindowsDirectory(
                    parentHandle,
                    segment,
                    observedIdentity is null && createMissing ? FileCreate : FileOpen,
                    requireDurableWriteAccess: createMissing && isDurableStoreRoot,
                    denyDeleteSharing:
                        !createMissing &&
                        isDurableStoreRoot &&
                        denyDeleteSharingForExistingRoot,
                    out var status);
                if (status < 0)
                {
                    childHandle.Dispose();
                    if (observedIdentity is null && status == StatusObjectNameCollision)
                    {
                        throw new DesignConversionHandleBoundTrustException(
                            "Handle-bound root segment appeared during safe creation.");
                    }

                    if (observedIdentity is not null &&
                        status is StatusNoSuchFile or
                            StatusObjectNameNotFound or
                            StatusObjectPathNotFound)
                    {
                        throw new DesignConversionHandleBoundTrustException(
                            "Handle-bound root segment disappeared during safe open.");
                    }

                    if (!createMissing &&
                        status is StatusNoSuchFile or
                            StatusObjectNameNotFound or
                            StatusObjectPathNotFound)
                    {
                        throw new DesignConversionHandleBoundNotFoundException(
                            "Handle-bound existing directory was not found.");
                    }

                    throw WindowsNtIOException(
                        observedIdentity is null
                            ? "create root-relative directory without following links"
                            : "open root-relative directory without following links",
                        childPath,
                        status);
                }

                ValidateWindowsHandle(childHandle, childPath, requireDirectory: true);
                if (observedIdentity is not null &&
                    GetWindowsIdentity(childHandle, childPath) != observedIdentity.Value)
                {
                    childHandle.Dispose();
                    throw new DesignConversionHandleBoundTrustException(
                        "Handle-bound root segment identity changed during safe open.");
                }
                handles.Add(childHandle);
                parentPath = childPath;
            }

            return new DesignConversionHandleBoundDirectory(
                fullRoot,
                handles,
                StorageBackend.Windows,
                beforeIo);
        }
        catch
        {
            for (var index = handles.Count - 1; index >= 0; index--)
                handles[index].Dispose();
            throw;
        }
    }

    private static DesignConversionHandleBoundDirectory AcquireLinux(
        string rootDirectory,
        bool createMissing,
        Action<DesignConversionHandleBoundIoStage, string>? beforeIo)
    {
        EnsureLinuxX64StatLayout();
        var fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootDirectory));
        if (!Path.IsPathFullyQualified(fullRoot) ||
            !fullRoot.StartsWith(Path.DirectorySeparatorChar))
        {
            throw new InvalidDataException("Handle-bound Linux storage root is not fully qualified.");
        }

        var segments = fullRoot
            .TrimStart(Path.DirectorySeparatorChar)
            .Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        var handles = new List<SafeFileHandle>(segments.Length + 1);
        try
        {
            var anchor = OpenLinuxDirectoryAbsolute("/", out var anchorError);
            if (anchor.IsInvalid)
            {
                anchor.Dispose();
                throw LinuxIOException("open filesystem root anchor", "/", anchorError);
            }

            ValidateLinuxHandleKind(anchor, "/", requireDirectory: true);
            handles.Add(anchor);
            var parentPath = "/";
            for (var segmentIndex = 0; segmentIndex < segments.Length; segmentIndex++)
            {
                var segment = segments[segmentIndex];
                EnsureLinuxLeafName(segment);
                var childPath = Path.Combine(parentPath, segment);
                var parentHandle = handles[^1];
                ValidateLinuxHandleKind(parentHandle, parentPath, requireDirectory: true);

                DesignConversionFileIdentity? observedIdentity = null;
                var observedHandle = OpenLinuxDirectoryAt(
                    parentHandle,
                    segment,
                    out var observedError);
                if (!observedHandle.IsInvalid)
                {
                    try
                    {
                        ValidateLinuxHandleKind(
                            observedHandle,
                            childPath,
                            requireDirectory: true);
                        observedIdentity = GetLinuxIdentity(observedHandle, childPath);
                    }
                    finally
                    {
                        observedHandle.Dispose();
                    }
                }
                else
                {
                    observedHandle.Dispose();
                    if (!createMissing && observedError == LinuxEntryNotFound)
                    {
                        throw new DesignConversionHandleBoundNotFoundException(
                            "Handle-bound existing directory was not found.");
                    }
                    if (observedError != LinuxEntryNotFound)
                    {
                        throw LinuxIOException(
                            "observe root-relative directory before safe open",
                            childPath,
                            observedError);
                    }
                }

                beforeIo?.Invoke(
                    DesignConversionHandleBoundIoStage.RootSegmentBeforeOpen,
                    childPath);
                ValidateLinuxHandleKind(parentHandle, parentPath, requireDirectory: true);

                if (observedIdentity is null)
                {
                    if (!createMissing)
                    {
                        throw new DesignConversionHandleBoundNotFoundException(
                            "Handle-bound existing directory was not found.");
                    }

                    // Создаём каталог только через уже закреплённый fd родителя:
                    // строковый путь больше не может увести нас через подменённый ancestor.
                    var created = MakeDirectoryAtLinux(
                        ToFileDescriptor(parentHandle),
                        segment,
                        LinuxOwnerDirectoryMode);
                    if (created != 0)
                    {
                        var createError = Marshal.GetLastPInvokeError();
                        if (createError == LinuxEntryExists)
                        {
                            throw new DesignConversionHandleBoundConcurrentCreateException(
                                "Handle-bound root segment appeared during safe creation.");
                        }

                        throw LinuxIOException(
                            "create root-relative directory without following links",
                            childPath,
                            createError);
                    }

                    // fsync родителя делает факт появления нового сегмента переживаемым
                    // при crash/restart, а не только видимым в кэше процесса.
                    FsyncLinuxDirectory(parentHandle, parentPath);
                }

                var childHandle = OpenLinuxDirectoryAt(
                    parentHandle,
                    segment,
                    out var status);
                if (childHandle.IsInvalid)
                {
                    childHandle.Dispose();
                    if (observedIdentity is not null && status == LinuxEntryNotFound)
                    {
                        throw new DesignConversionHandleBoundTrustException(
                            "Handle-bound root segment disappeared during safe open.");
                    }

                    throw LinuxIOException(
                        observedIdentity is null
                            ? "open created root-relative directory without following links"
                            : "open root-relative directory without following links",
                        childPath,
                        status);
                }

                ValidateLinuxHandleKind(childHandle, childPath, requireDirectory: true);
                if (observedIdentity is not null &&
                    GetLinuxIdentity(childHandle, childPath) != observedIdentity.Value)
                {
                    childHandle.Dispose();
                    throw new DesignConversionHandleBoundTrustException(
                        "Handle-bound root segment identity changed during safe open.");
                }
                handles.Add(childHandle);
                parentPath = childPath;
            }

            return new DesignConversionHandleBoundDirectory(
                fullRoot,
                handles,
                StorageBackend.Linux,
                beforeIo);
        }
        catch
        {
            for (var index = handles.Count - 1; index >= 0; index--)
                handles[index].Dispose();
            throw;
        }
    }

    public FileStream? TryOpenExisting(
        string leafName,
        FileShare share,
        bool writeThrough,
        DesignConversionFileIdentity? expectedIdentity = null)
        => OpenLeaf(
            leafName,
            share,
            writeThrough,
            FileOpen,
            returnMissing: true,
            expectedIdentity);

    public FileStream? TryOpenExistingReadOnly(string leafName)
        => OpenLeaf(
            leafName,
            FileShare.Read,
            writeThrough: false,
            FileOpen,
            returnMissing: true,
            expectedIdentity: null,
            desiredAccess: GenericRead | Synchronize,
            fileAccess: FileAccess.Read);

    public FileStream? TryOpenExistingReadOnly(string leafName, FileShare share)
        => OpenLeaf(
            leafName,
            share,
            writeThrough: false,
            FileOpen,
            returnMissing: true,
            expectedIdentity: null,
            desiredAccess: GenericRead | Synchronize,
            fileAccess: FileAccess.Read);

    public FileStream CreateNew(
        string leafName,
        FileShare share,
        bool writeThrough)
        => OpenLeaf(
               leafName,
               share,
               writeThrough,
               FileCreate,
               returnMissing: false,
               expectedIdentity: null)
           ?? throw new IOException("Handle-bound file creation returned no handle.");

    public FileStream OpenOrCreate(
        string leafName,
        FileShare share,
        bool writeThrough)
        => OpenLeaf(
               leafName,
               share,
               writeThrough,
               FileOpenIf,
               returnMissing: false,
               expectedIdentity: null)
           ?? throw new IOException("Handle-bound file open-or-create returned no handle.");

    public IReadOnlyList<DesignConversionHandleBoundEntry> EnumerateEntries(
        int maximumEntryCount,
        string heldLeafName,
        FileStream heldStream,
        string? secondHeldLeafName = null,
        FileStream? secondHeldStream = null)
    {
        if (maximumEntryCount <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(maximumEntryCount),
                "Handle-bound enumeration ceiling must be positive.");
        EnsureLeafNameForBackend(heldLeafName);
        ValidateRootHandle();
        var entries = new List<DesignConversionHandleBoundEntry>(
            Math.Min(maximumEntryCount, 256));
        EnumerateNamesBounded(maximumEntryCount, name =>
        {
            if (string.Equals(name, heldLeafName, StringComparison.Ordinal))
            {
                entries.Add(BuildHeldEntry(heldLeafName, heldStream));
                return;
            }
            if (secondHeldLeafName is not null &&
                secondHeldStream is not null &&
                string.Equals(name, secondHeldLeafName, StringComparison.Ordinal))
            {
                entries.Add(BuildHeldEntry(secondHeldLeafName, secondHeldStream));
                return;
            }

            using var stream = TryOpenExisting(
                name,
                FileShare.Read | FileShare.Write | FileShare.Delete,
                writeThrough: false)
                ?? throw new IOException("Directory entry disappeared during handle-bound inventory.");
            entries.Add(new DesignConversionHandleBoundEntry(
                name,
                stream.Length,
                GetIdentity(stream, name)));
        });

        ValidateRootHandle();
        return entries;
    }

    internal IReadOnlyList<string> EnumerateNames(
        int maximumEntryCount,
        string heldLeafName,
        FileStream heldStream)
    {
        if (maximumEntryCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumEntryCount),
                "Handle-bound enumeration ceiling must be positive.");
        }
        ArgumentNullException.ThrowIfNull(heldStream);
        EnsureLeafNameForBackend(heldLeafName);
        ValidateRootHandle();
        _ = BuildHeldEntry(heldLeafName, heldStream);
        var names = new List<string>(Math.Min(maximumEntryCount, 256));
        EnumerateNamesBounded(maximumEntryCount, names.Add);
        _ = BuildHeldEntry(heldLeafName, heldStream);
        ValidateRootHandle();
        return names;
    }

    internal IReadOnlyList<string> EnumerateNames(int maximumEntryCount)
    {
        if (maximumEntryCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumEntryCount),
                "Handle-bound enumeration ceiling must be positive.");
        }

        ValidateRootHandle();
        var names = new List<string>(Math.Min(maximumEntryCount, 256));
        EnumerateNamesBounded(maximumEntryCount, names.Add);
        ValidateRootHandle();
        return names;
    }

    internal bool ContainsNameNoFollow(
        int maximumEntryCount,
        string leafName)
    {
        if (maximumEntryCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumEntryCount),
                "Handle-bound enumeration ceiling must be positive.");
        }
        EnsureLeafNameForBackend(leafName);

        ValidateRootHandle();
        var firstObserved = false;
        EnumerateNamesBounded(maximumEntryCount, name =>
        {
            if (string.Equals(name, leafName, EntryNameComparison))
                firstObserved = true;
        });

        _beforeIo?.Invoke(
            DesignConversionHandleBoundIoStage.BetweenNamespacePresenceScans,
            Path.Combine(_rootDirectory, leafName));
        ValidateRootHandle();

        var secondObserved = false;
        EnumerateNamesBounded(maximumEntryCount, name =>
        {
            if (string.Equals(name, leafName, EntryNameComparison))
                secondObserved = true;
        });
        ValidateRootHandle();

        return firstObserved || secondObserved;
    }

    internal void ValidateHeldLeaf(
        string heldLeafName,
        FileStream heldStream)
    {
        ArgumentNullException.ThrowIfNull(heldStream);
        EnsureLeafNameForBackend(heldLeafName);
        ValidateRootHandle();
        _ = BuildHeldEntry(heldLeafName, heldStream);
        ValidateRootHandle();
    }

    private DesignConversionHandleBoundEntry BuildHeldEntry(
        string leafName,
        FileStream heldStream)
    {
        // Удержанный stream не доверяется по имени: мы сверяем живой handle с
        // тем же leaf в закреплённом root. На Windows ядро ещё запрещает
        // rename/delete через FileShare.None; на Linux это даёт fail-closed
        // проверку identity поверх advisory-lock модели.
        EnsureLeafNameForBackend(leafName);
        ValidateLeafHandle(heldStream.SafeFileHandle, leafName);
        return new DesignConversionHandleBoundEntry(
            leafName,
            heldStream.Length,
            GetIdentityFromHandle(heldStream.SafeFileHandle, leafName));
    }

    public DesignConversionFileIdentity GetIdentity(FileStream stream, string leafName)
    {
        EnsureLeafNameForBackend(leafName);
        ValidateLeafHandle(stream.SafeFileHandle, leafName);
        return GetIdentityFromHandle(stream.SafeFileHandle, leafName);
    }

    public uint GetLinkCount(FileStream stream, string leafName)
    {
        EnsureLeafNameForBackend(leafName);
        ValidateLeafHandle(stream.SafeFileHandle, leafName);
        if (_backend == StorageBackend.Linux)
        {
            var stat = GetLinuxStat(stream.SafeFileHandle, leafName);
            if (stat.LinkCount > uint.MaxValue)
                throw new InvalidDataException("Handle-bound Linux link count exceeds the safe bound.");
            return (uint)stat.LinkCount;
        }

        var path = Path.Combine(_rootDirectory, leafName);
        if (!GetFileInformationByHandleExWindows(
                stream.SafeFileHandle,
                FileInfoByHandleClass.FileStandardInfo,
                out FileStandardInformation info,
                (uint)Marshal.SizeOf<FileStandardInformation>()))
        {
            throw WindowsIOException(
                "read handle link count",
                path,
                Marshal.GetLastPInvokeError());
        }

        return info.NumberOfLinks;
    }

    public void AtomicReplaceAndFlush(
        FileStream tempStream,
        string tempLeafName,
        string destinationLeafName,
        int maximumRollbackBytes,
        DesignConversionHandleBoundDestinationExpectation destinationExpectation = default,
        Action? beforeDestinationSnapshot = null)
    {
        EnsureLeafNameForBackend(tempLeafName);
        EnsureLeafNameForBackend(destinationLeafName);
        ValidateRootHandle();
        ValidateLeafHandle(tempStream.SafeFileHandle, tempLeafName);

        beforeDestinationSnapshot?.Invoke();
        using var prior = OpenDestinationSnapshot(
            destinationLeafName,
            maximumRollbackBytes,
            destinationExpectation);
        var backupLeafName = destinationLeafName + PublicationBackupSuffix;
        var priorMovedToBackup = false;
        var tempPublished = false;
        var durableCommitReached = false;
        try
        {
            if (prior is not null && _backend == StorageBackend.Linux)
            {
                // Linux cannot stop an uncooperative rename with the open file
                // handle alone. Preserve the verified old destination first, so
                // a later collision leaves recoverable bytes in our backup.
                MovePriorDestinationToBackup(
                    prior,
                    destinationLeafName,
                    backupLeafName);
                priorMovedToBackup = true;
            }

            _beforeIo?.Invoke(
                DesignConversionHandleBoundIoStage.BeforeAtomicRename,
                Path.Combine(_rootDirectory, destinationLeafName));
            ValidateRootHandle();
            ValidateLeafHandle(tempStream.SafeFileHandle, tempLeafName);
            if (prior is not null && !priorMovedToBackup)
            {
                // Держим уже проверенный destination открытым до барьера и
                // переносим его в backup без overwrite; так чужой leaf,
                // появившийся в гонке, не будет молча затёрт.
                MovePriorDestinationToBackup(
                    prior,
                    destinationLeafName,
                    backupLeafName);
                priorMovedToBackup = true;
            }

            // Публикация никогда не является overwrite: если destination
            // появился после снимка, rename обязан столкнуться и оставить
            // чужие байты на месте.
            RenameHandleNoReplace(
                tempStream.SafeFileHandle,
                tempLeafName,
                destinationLeafName,
                "atomically publish to an unoccupied root-relative state");
            tempPublished = true;
            ValidateLeafHandle(tempStream.SafeFileHandle, destinationLeafName);

            _beforeIo?.Invoke(
                DesignConversionHandleBoundIoStage.AfterAtomicRenameBeforeDirectoryFlush,
                Path.Combine(_rootDirectory, destinationLeafName));
            FlushDirectoryMetadata();
            durableCommitReached = true;

            if (priorMovedToBackup)
            {
                _beforeIo?.Invoke(
                    DesignConversionHandleBoundIoStage.AfterDurableCommitBeforeBackupCleanup,
                    Path.Combine(_rootDirectory, backupLeafName));
                DeleteLeafByHandle(
                    prior!.Stream.SafeFileHandle,
                    backupLeafName);
                prior.Dispose();
                _beforeIo?.Invoke(
                    DesignConversionHandleBoundIoStage.AfterBackupDeleteBeforeDirectoryFlush,
                    Path.Combine(_rootDirectory, backupLeafName));
                FlushDirectoryMetadata();
            }
            ValidateRootHandle();
        }
        catch (Exception publicationException)
        {
            if (durableCommitReached)
            {
                throw publicationException is
                    DesignConversionHandleBoundPostCommitCleanupException
                    ? publicationException
                    : new DesignConversionHandleBoundPostCommitCleanupException(
                        "The new state publication is durable, but backup cleanup durability is unknown.",
                        publicationException);
            }

            try
            {
                RollBackConditionalPublication(
                    tempStream,
                    tempLeafName,
                    destinationLeafName,
                    prior,
                    backupLeafName,
                    priorMovedToBackup,
                    tempPublished);
            }
            catch (DesignConversionHandleBoundTrustException)
                when (_backend == StorageBackend.Linux &&
                      priorMovedToBackup &&
                      !tempPublished)
            {
                throw;
            }
            catch (Exception rollbackException)
            {
                throw new IOException(
                    "Handle-bound publication failed and rollback could not restore the prior durable state.",
                    new AggregateException(
                        publicationException,
                        rollbackException));
            }

            throw;
        }
    }

    public void RenameNoReplaceAndFlush(
        FileStream sourceStream,
        string sourceLeafName,
        string destinationLeafName)
    {
        EnsureLeafNameForBackend(sourceLeafName);
        EnsureLeafNameForBackend(destinationLeafName);
        ValidateRootHandle();
        ValidateLeafHandle(sourceStream.SafeFileHandle, sourceLeafName);
        _beforeIo?.Invoke(
            DesignConversionHandleBoundIoStage.BeforePublicationBackupRecoveryRename,
            Path.Combine(_rootDirectory, sourceLeafName));
        ValidateRootHandle();
        ValidateLeafHandle(sourceStream.SafeFileHandle, sourceLeafName);
        RenameHandleNoReplace(
            sourceStream.SafeFileHandle,
            sourceLeafName,
            destinationLeafName,
            "recover publication backup without overwriting destination");
        ValidateLeafHandle(sourceStream.SafeFileHandle, destinationLeafName);
        FlushDirectoryMetadata();
        ValidateRootHandle();
    }

    internal void RenameToDirectoryNoReplaceAndFlush(
        FileStream sourceStream,
        string sourceLeafName,
        DesignConversionHandleBoundDirectory destinationDirectory,
        string destinationLeafName)
    {
        ArgumentNullException.ThrowIfNull(sourceStream);
        ArgumentNullException.ThrowIfNull(destinationDirectory);
        EnsureLeafNameForBackend(sourceLeafName);
        destinationDirectory.EnsureLeafNameForBackend(destinationLeafName);
        if (_backend != StorageBackend.Linux ||
            destinationDirectory._backend != StorageBackend.Linux)
        {
            throw new PlatformNotSupportedException(
                "Cross-directory no-replace publication is currently defined for Linux handle-bound custody.");
        }

        ValidateRootHandle();
        destinationDirectory.ValidateRootHandle();
        ValidateLeafHandle(sourceStream.SafeFileHandle, sourceLeafName);
        _beforeIo?.Invoke(
            DesignConversionHandleBoundIoStage.BeforePublicationBackupRecoveryRename,
            Path.Combine(_rootDirectory, sourceLeafName));
        RenameLinuxHandleNoReplace(
            sourceStream.SafeFileHandle,
            sourceLeafName,
            destinationDirectory,
            destinationLeafName,
            "publish root-relative file into another retained directory without overwriting");
        FlushDirectoryMetadata();
        destinationDirectory.FlushDirectoryMetadata();
        ValidateRootHandle();
        destinationDirectory.ValidateRootHandle();
    }

    internal void ValidateLinuxCustodyRootTrustContract()
    {
        if (_backend != StorageBackend.Linux)
            throw new PlatformNotSupportedException(
                "Linux custody root trust proof is available only on Linux.");
        ValidateLinuxHandleKind(_rootHandle, _rootDirectory, requireDirectory: true);
        ValidateLinuxCustodyStatTrustContract(
            GetLinuxStat(_rootHandle, _rootDirectory),
            requireDirectory: true,
            _rootDirectory);
    }

    internal void ValidateLinuxCustodyFileTrustContract(
        FileStream stream,
        string leafName,
        uint expectedLinkCount = 1)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (_backend != StorageBackend.Linux)
            throw new PlatformNotSupportedException(
                "Linux custody file trust proof is available only on Linux.");
        EnsureLeafNameForBackend(leafName);
        ValidateLeafHandle(stream.SafeFileHandle, leafName);
        var path = Path.Combine(_rootDirectory, leafName);
        var stat = GetLinuxStat(stream.SafeFileHandle, path);
        ValidateLinuxCustodyStatTrustContract(stat, requireDirectory: false, path);
        if (stat.LinkCount != expectedLinkCount)
        {
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound Linux custody rejected a secret file with an unexpected link count.");
        }
    }

    public DesignConversionHandleBoundDeleteDisposition DeleteIfRegular(
        string leafName,
        DesignConversionFileIdentity? expectedIdentity = null)
    {
        EnsureLeafNameForBackend(leafName);
        using var stream = TryOpenExisting(
            leafName,
            FileShare.Read | FileShare.Write | FileShare.Delete,
            writeThrough: true,
            expectedIdentity);
        if (stream is null)
        {
            if (expectedIdentity is not null)
            {
                throw new DesignConversionHandleBoundTrustException(
                    "Handle-bound delete target disappeared before deletion.");
            }

            return DesignConversionHandleBoundDeleteDisposition.AlreadyAbsent;
        }
        _beforeIo?.Invoke(
            DesignConversionHandleBoundIoStage.BeforeDelete,
            Path.Combine(_rootDirectory, leafName));
        ValidateRootHandle();
        if (expectedIdentity is not null &&
            GetIdentity(stream, leafName) != expectedIdentity.Value)
        {
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound delete target identity changed before deletion.");
        }
        DeleteLeafByHandle(stream.SafeFileHandle, leafName);
        stream.Dispose();
        try
        {
            _beforeIo?.Invoke(
                DesignConversionHandleBoundIoStage.AfterDeleteMarkBeforeDirectoryFlush,
                Path.Combine(_rootDirectory, leafName));
            FlushDirectoryMetadata();
        }
        catch (Exception ex) when (
            ex is not DesignConversionHandleBoundTrustException)
        {
            throw new DesignConversionHandleBoundDeleteDispositionException(
                "Handle-bound deletion removed the namespace entry, but directory durability was not confirmed.",
                DesignConversionHandleBoundDeleteDisposition
                    .NamespaceEntryRemovedDirectoryFlushNotConfirmed,
                ex);
        }

        try
        {
            _beforeIo?.Invoke(
                DesignConversionHandleBoundIoStage.AfterDurableDelete,
                Path.Combine(_rootDirectory, leafName));
        }
        catch (Exception ex)
        {
            throw new DesignConversionHandleBoundPostCommitCleanupException(
                "Handle-bound deletion is durable, but post-commit observation failed.",
                ex);
        }

        return DesignConversionHandleBoundDeleteDisposition.DurableDelete;
    }

    public DesignConversionHandleBoundDeleteDisposition DeleteHeldRegular(
        FileStream retainedStream,
        string leafName,
        DesignConversionFileIdentity expectedIdentity)
        => DeleteHeldRegular(
            retainedStream,
            leafName,
            expectedIdentity,
            expectedLinkCount: 1);

    internal DesignConversionHandleBoundDeleteDisposition DeleteHeldRegular(
        FileStream retainedStream,
        string leafName,
        DesignConversionFileIdentity expectedIdentity,
        uint expectedLinkCount)
    {
        ArgumentNullException.ThrowIfNull(retainedStream);
        EnsureLeafNameForBackend(leafName);
        ValidateRootHandle();
        ValidateLeafHandle(retainedStream.SafeFileHandle, leafName);
        if (GetIdentity(retainedStream, leafName) != expectedIdentity ||
            GetLinkCount(retainedStream, leafName) != expectedLinkCount)
        {
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound retained delete proof no longer matches its original file.");
        }

        _beforeIo?.Invoke(
            DesignConversionHandleBoundIoStage.BeforeDelete,
            Path.Combine(_rootDirectory, leafName));
        ValidateRootHandle();
        DeleteLeafByHandle(
            retainedStream.SafeFileHandle,
            leafName,
            expectedLinkCount);
        try
        {
            _beforeIo?.Invoke(
                DesignConversionHandleBoundIoStage.AfterDeleteMarkBeforeDirectoryFlush,
                Path.Combine(_rootDirectory, leafName));
            FlushDirectoryMetadata();
        }
        catch (Exception ex) when (
            ex is not DesignConversionHandleBoundTrustException)
        {
            throw new DesignConversionHandleBoundDeleteDispositionException(
                "Handle-bound retained deletion removed the namespace entry, but directory durability was not confirmed.",
                DesignConversionHandleBoundDeleteDisposition
                    .NamespaceEntryRemovedDirectoryFlushNotConfirmed,
                ex);
        }

        _beforeIo?.Invoke(
            DesignConversionHandleBoundIoStage.AfterDurableDelete,
            Path.Combine(_rootDirectory, leafName));
        return DesignConversionHandleBoundDeleteDisposition.DurableDelete;
    }

    public bool TryDeleteEmptyRootDirectoryNoGap()
    {
        ValidateRootHandle();
        if (_backend == StorageBackend.Linux)
            return TryDeleteLinuxEmptyRootDirectoryNoGap();

        throw new PlatformNotSupportedException(
            "Handle-bound root directory deletion is routed through the caller on Windows.");
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        for (var index = _directoryHandles.Count - 1; index >= 0; index--)
            _directoryHandles[index].Dispose();
    }

    private FileStream? OpenLeaf(
        string leafName,
        FileShare share,
        bool writeThrough,
        uint createDisposition,
        bool returnMissing,
        DesignConversionFileIdentity? expectedIdentity,
        uint desiredAccess = GenericRead | GenericWrite | DeleteAccess | Synchronize,
        FileAccess fileAccess = FileAccess.ReadWrite)
    {
        if (_backend == StorageBackend.Linux)
        {
            return OpenLinuxLeaf(
                leafName,
                share,
                writeThrough,
                createDisposition,
                returnMissing,
                expectedIdentity,
                fileAccess);
        }

        EnsureLeafName(leafName);
        var path = Path.Combine(_rootDirectory, leafName);
        ValidateRootHandle();
        _beforeIo?.Invoke(DesignConversionHandleBoundIoStage.LeafBeforeOpen, path);
        ValidateRootHandle();

        var handle = NtOpenWindowsRelative(
            _rootHandle,
            leafName,
            desiredAccess,
            share,
            createDisposition,
            FileAttributeNormal,
            FileSynchronousIoNonAlert |
            FileNonDirectoryFile |
            FileOpenReparsePoint |
            (writeThrough ? FileWriteThrough : 0),
            out var status);
        if (status < 0)
        {
            handle.Dispose();
            if (returnMissing &&
                status is StatusNoSuchFile or StatusObjectNameNotFound or StatusObjectPathNotFound)
            {
                return null;
            }

            if (status == StatusObjectNameCollision && createDisposition == FileCreate)
            {
                throw new DesignConversionHandleBoundNameCollisionException(
                    "Handle-bound file already exists.");
            }
            if (status == StatusObjectNameCollision)
                throw new IOException("Handle-bound file already exists.");
            if (status is StatusSharingViolation or StatusLockNotGranted)
            {
                throw new DesignConversionHandleBoundSharingViolationException(
                    $"Handle-bound storage could not acquire an exclusive handle for '{path}'.");
            }

            throw WindowsNtIOException("open root-relative file without following links", path, status);
        }

        try
        {
            ValidateWindowsHandle(handle, path, requireDirectory: false);
            if (expectedIdentity is not null &&
                GetWindowsIdentity(handle, path) != expectedIdentity.Value)
            {
                throw new DesignConversionHandleBoundTrustException(
                    "Handle-bound file identity changed after trusted inventory.");
            }

            return new FileStream(handle, fileAccess, 16 * 1024, isAsync: false);
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    private DestinationSnapshot? OpenDestinationSnapshot(
        string leafName,
        int maximumBytes,
        DesignConversionHandleBoundDestinationExpectation expectation)
    {
        var stream = TryOpenExisting(
            leafName,
            FileShare.Read,
            writeThrough: false);
        if (stream is null)
        {
            if (expectation.ExpectedIdentity is not null)
            {
                throw new DesignConversionHandleBoundTrustException(
                    "The expected publication destination disappeared before replacement.");
            }
            return null;
        }
        try
        {
            if (expectation.MustBeAbsent)
            {
                throw new DesignConversionHandleBoundTrustException(
                    "A publication destination appeared after the absent state was observed.");
            }
            if (expectation.ExpectedIdentity is { } expectedIdentity &&
                GetIdentity(stream, leafName) != expectedIdentity)
            {
                throw new DesignConversionHandleBoundTrustException(
                    "The publication destination identity changed before replacement.");
            }
            if (expectation.ExpectedIdentity is not null &&
                stream.Length != expectation.ExpectedLength)
            {
                throw new DesignConversionHandleBoundTrustException(
                    "The publication destination length changed before replacement.");
            }
            if (stream.Length < 0L || stream.Length > maximumBytes)
                throw new InvalidDataException("Existing state exceeds the rollback bound.");
            var bytes = new byte[checked((int)stream.Length)];
            stream.Position = 0L;
            stream.ReadExactly(bytes);
            if (expectation.ExpectedSha256 is not null)
            {
                var actualSha256 =
                    $"sha256:{Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()}";
                if (!string.Equals(
                        actualSha256,
                        expectation.ExpectedSha256,
                        StringComparison.Ordinal))
                {
                    throw new DesignConversionHandleBoundTrustException(
                        "The publication destination content changed before replacement.");
                }
            }
            return new DestinationSnapshot(stream);
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    private void RollBackConditionalPublication(
        FileStream tempStream,
        string tempLeafName,
        string destinationLeafName,
        DestinationSnapshot? prior,
        string backupLeafName,
        bool priorMovedToBackup,
        bool tempPublished)
    {
        if (tempPublished)
        {
            DeleteLeafByHandle(
                tempStream.SafeFileHandle,
                destinationLeafName);
            tempStream.Dispose();
            FlushDirectoryMetadata();
        }
        else
        {
            DeleteLeafByHandle(
                tempStream.SafeFileHandle,
                tempLeafName);
            tempStream.Dispose();
            FlushDirectoryMetadata();
        }

        if (!tempPublished && priorMovedToBackup && _backend == StorageBackend.Linux)
        {
            // A Linux collision means another process placed content at the
            // destination after our verified old state was moved aside. We
            // leave both the foreign destination and the recoverable old backup
            // untouched.
            throw new DesignConversionHandleBoundTrustException(
                "The publication destination was occupied after the verified old state was preserved in backup; refusing to overwrite foreign content.");
        }

        if (priorMovedToBackup)
        {
            RenameHandleNoReplace(
                prior!.Stream.SafeFileHandle,
                backupLeafName,
                destinationLeafName,
                "restore publication backup during rollback");
            ValidateLeafHandle(prior.Stream.SafeFileHandle, destinationLeafName);
            FlushDirectoryMetadata();
        }
    }

    private void MovePriorDestinationToBackup(
        DestinationSnapshot prior,
        string destinationLeafName,
        string backupLeafName)
    {
        RenameHandleNoReplace(
            prior.Stream.SafeFileHandle,
            destinationLeafName,
            backupLeafName,
            "move observed destination to publication backup");
        ValidateLeafHandle(prior.Stream.SafeFileHandle, backupLeafName);
        FlushDirectoryMetadata();
    }

    private void RenameHandleNoReplace(
        SafeFileHandle sourceHandle,
        string sourceLeafName,
        string destinationLeafName,
        string operation)
    {
        EnsureLeafNameForBackend(sourceLeafName);
        EnsureLeafNameForBackend(destinationLeafName);
        if (_backend == StorageBackend.Linux)
        {
            RenameLinuxHandleNoReplace(
                sourceHandle,
                sourceLeafName,
                destinationLeafName,
                operation);
            return;
        }

        RenameWindowsHandle(
            sourceHandle,
            destinationLeafName,
            replaceIfExists: false);
    }

    private void RenameWindowsHandle(
        SafeFileHandle sourceHandle,
        string destinationLeafName,
        bool replaceIfExists)
    {
        EnsureLeafName(destinationLeafName);
        var rootAddRef = false;
        var fileNameBytes = checked(destinationLeafName.Length * sizeof(char));
        var rootOffset = IntPtr.Size == 8 ? 8 : 4;
        var lengthOffset = rootOffset + IntPtr.Size;
        var nameOffset = lengthOffset + sizeof(uint);
        var buffer = Marshal.AllocHGlobal(checked(nameOffset + fileNameBytes));
        try
        {
            for (var index = 0; index < nameOffset + fileNameBytes; index++)
                Marshal.WriteByte(buffer, index, 0);
            Marshal.WriteByte(buffer, 0, replaceIfExists ? (byte)1 : (byte)0);
            _rootHandle.DangerousAddRef(ref rootAddRef);
            Marshal.WriteIntPtr(buffer, rootOffset, _rootHandle.DangerousGetHandle());
            Marshal.WriteInt32(buffer, lengthOffset, fileNameBytes);
            Marshal.Copy(
                destinationLeafName.ToCharArray(),
                0,
                IntPtr.Add(buffer, nameOffset),
                destinationLeafName.Length);

            var status = NtSetInformationFileWindows(
                sourceHandle,
                out _,
                buffer,
                checked((uint)(nameOffset + fileNameBytes)),
                FileRenameInformation);
            if (status < 0)
            {
                if (!replaceIfExists && status == StatusObjectNameCollision)
                {
                    throw new DesignConversionHandleBoundNameCollisionException(
                        "Handle-bound collision-denying rename rejected an occupied destination.");
                }

                throw WindowsNtIOException(
                    replaceIfExists
                        ? "atomically replace root-relative state"
                        : "atomically publish to an unoccupied root-relative state",
                    destinationLeafName,
                    status);
            }
        }
        finally
        {
            if (rootAddRef)
                _rootHandle.DangerousRelease();
            Marshal.FreeHGlobal(buffer);
        }
    }

    private void MarkDeleteOnClose(SafeFileHandle handle, string leafName)
    {
        var buffer = Marshal.AllocHGlobal(sizeof(int));
        try
        {
            Marshal.WriteInt32(buffer, 1);
            if (!SetFileInformationByHandleWindows(
                    handle,
                    FileInfoByHandleClass.FileDispositionInfo,
                    buffer,
                    sizeof(int)))
            {
                throw WindowsIOException(
                    "delete root-relative file",
                    leafName,
                    Marshal.GetLastPInvokeError());
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private void DeleteLeafByHandle(
        SafeFileHandle handle,
        string leafName,
        uint expectedLinkCount = 1)
    {
        EnsureLeafNameForBackend(leafName);
        if (_backend == StorageBackend.Linux)
        {
            DeleteLinuxLeafByHandle(handle, leafName, expectedLinkCount);
            return;
        }

        MarkDeleteOnClose(handle, leafName);
    }

    public void FlushDirectoryMetadata()
    {
        ValidateRootHandle();
        _beforeIo?.Invoke(
            DesignConversionHandleBoundIoStage.BeforeDirectoryMetadataFlush,
            _rootDirectory);
        ValidateRootHandle();
        if (_backend == StorageBackend.Linux)
        {
            FsyncLinuxDirectory(_rootHandle, _rootDirectory);
            return;
        }

        var status = NtFlushBuffersFileWindows(_rootHandle, out _);
        if (status < 0)
            throw WindowsNtIOException("durably flush root directory metadata", _rootDirectory, status);
    }

    private void EnumerateWindowsNamesBounded(
        int maximumEntryCount,
        Action<string> onName)
    {
        ArgumentNullException.ThrowIfNull(onName);
        var buffer = Marshal.AllocHGlobal(DirectoryBufferBytes);
        try
        {
            var observedEntryCount = 0;
            var restart = true;
            while (true)
            {
                var status = NtQueryDirectoryFileWindows(
                    _rootHandle,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    out var ioStatus,
                    buffer,
                    DirectoryBufferBytes,
                    FileIdBothDirectoryInformation,
                    returnSingleEntry: false,
                    IntPtr.Zero,
                    restart);
                restart = false;
                if (status == StatusNoMoreFiles)
                    break;
                if (status < 0)
                    throw WindowsNtIOException("enumerate root directory by handle", _rootDirectory, status);

                var bytesReturned = ValidateDirectoryQueryByteCount(
                    ioStatus.Information.ToInt64());
                var managedBuffer = new byte[bytesReturned];
                Marshal.Copy(buffer, managedBuffer, 0, bytesReturned);
                observedEntryCount = ParseFileIdBothDirectoryInformationBuffer(
                    managedBuffer,
                    bytesReturned,
                    maximumEntryCount,
                    observedEntryCount,
                    static rawName =>
                        new string(MemoryMarshal.Cast<byte, char>(rawName.Span)),
                    onName);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    internal static int ValidateDirectoryQueryByteCount(long information)
    {
        if (information < 1L || information > DirectoryBufferBytes)
        {
            throw new InvalidDataException(
                "Handle-bound directory enumeration returned an invalid byte count.");
        }

        return checked((int)information);
    }

    internal static int ParseFileIdBothDirectoryInformationBuffer(
        ReadOnlyMemory<byte> buffer,
        int bytesReturned,
        int maximumEntryCount,
        int observedEntryCount,
        Func<ReadOnlyMemory<byte>, string> materializeName,
        Action<string> onName)
    {
        ArgumentNullException.ThrowIfNull(materializeName);
        ArgumentNullException.ThrowIfNull(onName);
        if (bytesReturned <= 0 ||
            bytesReturned > DirectoryBufferBytes ||
            bytesReturned > buffer.Length ||
            maximumEntryCount <= 0 ||
            observedEntryCount < 0 ||
            observedEntryCount > maximumEntryCount)
        {
            throw new InvalidDataException(
                "Handle-bound directory parser received invalid bounds.");
        }

        var offset = 0;
        while (offset < bytesReturned)
        {
            var remainingBytes = bytesReturned - offset;
            if (remainingBytes < FileIdBothNameOffset)
            {
                throw new InvalidDataException(
                    "Handle-bound directory record is shorter than its fixed header.");
            }

            var remaining = buffer.Span.Slice(offset, remainingBytes);
            var nextOffset = BinaryPrimitives.ReadInt32LittleEndian(remaining[..sizeof(int)]);
            var nameLength = BinaryPrimitives.ReadInt32LittleEndian(
                remaining.Slice(60, sizeof(int)));
            if (nextOffset < 0)
            {
                throw new InvalidDataException(
                    "Handle-bound directory record has a negative next offset.");
            }

            int recordLength;
            if (nextOffset == 0)
            {
                recordLength = remainingBytes;
            }
            else
            {
                if (nextOffset < FileIdBothNameOffset ||
                    (nextOffset & 7) != 0)
                {
                    throw new InvalidDataException(
                        "Handle-bound directory record has an invalid aligned next offset.");
                }
                int nextAbsoluteOffset;
                try
                {
                    nextAbsoluteOffset = checked(offset + nextOffset);
                }
                catch (OverflowException ex)
                {
                    throw new InvalidDataException(
                        "Handle-bound directory record offset overflowed.",
                        ex);
                }
                if (nextAbsoluteOffset <= offset ||
                    nextAbsoluteOffset > bytesReturned)
                {
                    throw new InvalidDataException(
                        "Handle-bound directory record does not make bounded forward progress.");
                }
                recordLength = nextOffset;
            }

            if (nameLength < 0 || (nameLength & 1) != 0)
            {
                throw new InvalidDataException(
                    "Handle-bound directory record has an invalid UTF-16 name length.");
            }
            int nameEnd;
            try
            {
                nameEnd = checked(FileIdBothNameOffset + nameLength);
            }
            catch (OverflowException ex)
            {
                throw new InvalidDataException(
                    "Handle-bound directory record name length overflowed.",
                    ex);
            }
            if (nameEnd > recordLength)
            {
                throw new InvalidDataException(
                    "Handle-bound directory name overlaps the next record.");
            }
            if (nextOffset == 0)
            {
                var maximumTerminalLength = checked((nameEnd + 7) & ~7);
                if (recordLength > maximumTerminalLength)
                {
                    throw new InvalidDataException(
                        "Handle-bound terminal directory record contains trailing record data.");
                }
            }

            var rawName = buffer.Slice(offset + FileIdBothNameOffset, nameLength);
            if (!IsRawDotOrDotDot(rawName.Span))
            {
                observedEntryCount = checked(observedEntryCount + 1);
                if (observedEntryCount > maximumEntryCount)
                {
                    throw new DesignConversionHandleBoundEnumerationLimitException(
                        "Handle-bound directory entry ceiling was exceeded.");
                }

                var name = materializeName(rawName)
                    ?? throw new InvalidDataException(
                        "Handle-bound directory materializer returned no name.");
                EnsureLeafName(name);
                onName(name);
            }

            if (nextOffset == 0)
            {
                offset = bytesReturned;
                break;
            }
            offset = checked(offset + nextOffset);
        }

        if (offset != bytesReturned)
        {
            throw new InvalidDataException(
                "Handle-bound directory parser did not consume the complete buffer.");
        }
        return observedEntryCount;
    }

    private static bool IsRawDotOrDotDot(ReadOnlySpan<byte> rawName)
        => rawName.Length == sizeof(char) &&
           BinaryPrimitives.ReadUInt16LittleEndian(rawName) == '.' ||
           rawName.Length == 2 * sizeof(char) &&
           BinaryPrimitives.ReadUInt16LittleEndian(rawName[..sizeof(char)]) == '.' &&
           BinaryPrimitives.ReadUInt16LittleEndian(rawName[sizeof(char)..]) == '.';

    private StringComparison EntryNameComparison =>
        _backend == StorageBackend.Windows
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private void EnumerateNamesBounded(int maximumEntryCount, Action<string> onName)
    {
        if (_backend == StorageBackend.Linux)
        {
            EnumerateLinuxNamesBounded(maximumEntryCount, onName);
            return;
        }

        EnumerateWindowsNamesBounded(maximumEntryCount, onName);
    }

    private void ValidateRootHandle()
    {
        if (_backend == StorageBackend.Linux)
        {
            ValidateLinuxHandleKind(_rootHandle, _rootDirectory, requireDirectory: true);
            return;
        }

        ValidateWindowsHandle(_rootHandle, _rootDirectory, requireDirectory: true);
    }

    private void ValidateLeafHandle(SafeFileHandle handle, string leafName)
    {
        if (_backend == StorageBackend.Linux)
        {
            ValidateLinuxLeafHandle(handle, leafName);
            return;
        }

        ValidateWindowsHandle(
            handle,
            Path.Combine(_rootDirectory, leafName),
            requireDirectory: false);
    }

    private DesignConversionFileIdentity GetIdentityFromHandle(
        SafeFileHandle handle,
        string leafName)
        => _backend == StorageBackend.Linux
            ? GetLinuxIdentity(handle, leafName)
            : GetWindowsIdentity(
                handle,
                Path.Combine(_rootDirectory, leafName));

    private static SafeFileHandle OpenLinuxDirectoryAbsolute(string path, out int error)
    {
        var descriptor = OpenLinux(path, LinuxDirectoryOpenFlags, 0);
        error = descriptor < 0 ? Marshal.GetLastPInvokeError() : 0;
        return new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
    }

    private static SafeFileHandle OpenLinuxDirectoryAt(
        SafeFileHandle parentHandle,
        string segment,
        out int error)
    {
        var descriptor = OpenAtLinux(
            ToFileDescriptor(parentHandle),
            segment,
            LinuxDirectoryOpenFlags,
            0);
        error = descriptor < 0 ? Marshal.GetLastPInvokeError() : 0;
        return new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
    }

    private FileStream? OpenLinuxLeaf(
        string leafName,
        FileShare share,
        bool writeThrough,
        uint createDisposition,
        bool returnMissing,
        DesignConversionFileIdentity? expectedIdentity,
        FileAccess fileAccess)
    {
        EnsureLinuxLeafName(leafName);
        var path = Path.Combine(_rootDirectory, leafName);
        ValidateRootHandle();
        _beforeIo?.Invoke(DesignConversionHandleBoundIoStage.LeafBeforeOpen, path);
        ValidateRootHandle();

        var flags = LinuxFileFlags(fileAccess, writeThrough);
        var descriptor = createDisposition switch
        {
            FileCreate => OpenAtLinux(
                ToFileDescriptor(_rootHandle),
                leafName,
                flags | LinuxOpenCreate | LinuxOpenExclusive,
                LinuxOwnerFileMode),
            FileOpenIf => OpenLinuxOpenOrCreateLeaf(leafName, flags),
            FileOpen => OpenAtLinux(
                ToFileDescriptor(_rootHandle),
                leafName,
                flags,
                0),
            _ => throw new ArgumentOutOfRangeException(
                nameof(createDisposition),
                "Unsupported handle-bound Linux create disposition.")
        };

        if (descriptor < 0)
        {
            var error = Marshal.GetLastPInvokeError();
            if (returnMissing && error == LinuxEntryNotFound)
                return null;
            if (error == LinuxEntryExists && createDisposition == FileCreate)
            {
                throw new DesignConversionHandleBoundNameCollisionException(
                    "Handle-bound file already exists.");
            }
            if (error == LinuxEntryExists)
                throw new IOException("Handle-bound file already exists.");
            if (error is LinuxTooManySymbolicLinks or LinuxIsDirectory)
            {
                throw new DesignConversionHandleBoundTrustException(
                    "Handle-bound safe open rejected a non-regular or linked leaf.");
            }

            throw LinuxIOException("open root-relative file without following links", path, error);
        }

        var handle = new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
        try
        {
            ValidateLinuxLeafHandle(handle, leafName);
            if (expectedIdentity is not null &&
                GetLinuxIdentity(handle, path) != expectedIdentity.Value)
            {
                throw new DesignConversionHandleBoundTrustException(
                    "Handle-bound file identity changed after trusted inventory.");
            }

            if (share == FileShare.None)
            {
                // FileShare.None не переносится из .NET в POSIX-ядро, поэтому
                // Linux-путь берёт явный advisory flock на тот же fd. Все наши
                // writer'ы соблюдают этот lock; чужой несоблюдающий процесс
                // всё равно ловится identity/readback-проверками.
                if (FlockLinux(descriptor, LinuxFlockExclusive | LinuxFlockNonBlocking) != 0)
                {
                    var lockError = Marshal.GetLastPInvokeError();
                    if (lockError == LinuxWouldBlock)
                    {
                        throw new DesignConversionHandleBoundSharingViolationException(
                            $"Handle-bound storage could not acquire an exclusive handle for '{path}'.");
                    }

                    throw LinuxIOException("acquire advisory file lease", path, lockError);
                }

                _beforeIo?.Invoke(
                    DesignConversionHandleBoundIoStage.MutationLockAcquired,
                    path);
            }

            return new FileStream(handle, fileAccess, 16 * 1024, isAsync: false);
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    private int OpenLinuxOpenOrCreateLeaf(string leafName, int flags)
    {
        var descriptor = OpenAtLinux(
            ToFileDescriptor(_rootHandle),
            leafName,
            flags,
            0);
        if (descriptor >= 0)
            return descriptor;

        var error = Marshal.GetLastPInvokeError();
        if (error != LinuxEntryNotFound)
            return -1;

        // Open-or-create выполняется в два шага, чтобы существующий symlink или
        // directory не прятались за O_CREAT: сначала безопасный open, затем
        // единственная exclusive creation.
        descriptor = OpenAtLinux(
            ToFileDescriptor(_rootHandle),
            leafName,
            flags | LinuxOpenCreate | LinuxOpenExclusive,
            LinuxOwnerFileMode);
        if (descriptor < 0 && Marshal.GetLastPInvokeError() == LinuxEntryExists)
        {
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound file appeared during safe creation.");
        }
        if (descriptor >= 0)
        {
            try
            {
                FsyncLinuxDirectory(_rootHandle, _rootDirectory);
            }
            catch
            {
                _ = CloseLinux(descriptor);
                throw;
            }
        }
        return descriptor;
    }

    private static int LinuxFileFlags(FileAccess fileAccess, bool writeThrough)
    {
        var accessFlags = fileAccess == FileAccess.Read
            ? LinuxOpenReadOnly
            : LinuxOpenReadWrite;
        return accessFlags |
               LinuxOpenNoFollow |
               LinuxOpenCloseOnExec |
               (writeThrough ? LinuxOpenSync : 0);
    }

    private void RenameLinuxHandleNoReplace(
        SafeFileHandle sourceHandle,
        string sourceLeafName,
        string destinationLeafName,
        string operation)
    {
        ValidateLinuxLeafHandle(sourceHandle, sourceLeafName);
        var sourceIdentity = GetLinuxIdentity(sourceHandle, sourceLeafName);
        int error;
        try
        {
            // renameat2 с RENAME_NOREPLACE даёт атомарный "publish only if
            // destination is still empty" внутри закреплённого root fd.
            if (RenameAt2Linux(
                    ToFileDescriptor(_rootHandle),
                    sourceLeafName,
                    ToFileDescriptor(_rootHandle),
                    destinationLeafName,
                    LinuxRenameNoReplace) != 0)
            {
                error = Marshal.GetLastPInvokeError();
                if (error == LinuxEntryExists)
                {
                    throw new DesignConversionHandleBoundNameCollisionException(
                        "Handle-bound collision-denying rename rejected an occupied destination.");
                }

                if (!IsLinuxNoReplaceRenameUnsupported(error))
                    throw LinuxIOException(operation, destinationLeafName, error);
            }
            else
            {
                ValidateLinuxLeafHandle(sourceHandle, destinationLeafName);
                if (GetLinuxIdentity(sourceHandle, destinationLeafName) != sourceIdentity)
                {
                    throw new DesignConversionHandleBoundTrustException(
                        "Handle-bound Linux rename published a different leaf identity.");
                }

                return;
            }
        }
        catch (EntryPointNotFoundException ex)
        {
            LinkAtThenUnlinkLinuxNoReplace(
                sourceHandle,
                sourceLeafName,
                this,
                destinationLeafName,
                sourceIdentity,
                operation,
                ex);
            return;
        }

        // `/mnt/c` может вернуть EINVAL на `RENAME_NOREPLACE`; тогда тот же
        // no-overwrite барьер строится через identity-bound `linkat` + `unlinkat`.
        LinkAtThenUnlinkLinuxNoReplace(
            sourceHandle,
            sourceLeafName,
            this,
            destinationLeafName,
            sourceIdentity,
            operation,
            renameFailure: null);
    }

    private void RenameLinuxHandleNoReplace(
        SafeFileHandle sourceHandle,
        string sourceLeafName,
        DesignConversionHandleBoundDirectory destinationDirectory,
        string destinationLeafName,
        string operation)
    {
        ValidateLinuxLeafHandle(sourceHandle, sourceLeafName);
        destinationDirectory.EnsureLeafNameForBackend(destinationLeafName);
        var sourceIdentity = GetLinuxIdentity(sourceHandle, sourceLeafName);
        int error;
        try
        {
            // Перенос из `stage` в `committed` идёт между двумя удержанными
            // каталогами: оба файловых дескриптора уже проверены, а
            // `RENAME_NOREPLACE` сохраняет чужое конечное имя.
            if (RenameAt2Linux(
                    ToFileDescriptor(_rootHandle),
                    sourceLeafName,
                    ToFileDescriptor(destinationDirectory._rootHandle),
                    destinationLeafName,
                    LinuxRenameNoReplace) != 0)
            {
                error = Marshal.GetLastPInvokeError();
                if (error == LinuxEntryExists)
                {
                    throw new DesignConversionHandleBoundNameCollisionException(
                        "Handle-bound collision-denying rename rejected an occupied destination.");
                }

                if (!IsLinuxNoReplaceRenameUnsupported(error))
                    throw LinuxIOException(operation, destinationLeafName, error);
            }
            else
            {
                destinationDirectory.ValidateLinuxLeafHandle(sourceHandle, destinationLeafName);
                if (GetLinuxIdentity(
                        sourceHandle,
                        Path.Combine(destinationDirectory._rootDirectory, destinationLeafName)) !=
                    sourceIdentity)
                {
                    throw new DesignConversionHandleBoundTrustException(
                        "Handle-bound Linux rename published a different leaf identity.");
                }
                return;
            }
        }
        catch (EntryPointNotFoundException ex)
        {
            LinkAtThenUnlinkLinuxNoReplace(
                sourceHandle,
                sourceLeafName,
                destinationDirectory,
                destinationLeafName,
                sourceIdentity,
                operation,
                ex);
            return;
        }

        LinkAtThenUnlinkLinuxNoReplace(
            sourceHandle,
            sourceLeafName,
            destinationDirectory,
            destinationLeafName,
            sourceIdentity,
            operation,
            renameFailure: null);
    }

    private void LinkAtThenUnlinkLinuxNoReplace(
        SafeFileHandle sourceHandle,
        string sourceLeafName,
        DesignConversionHandleBoundDirectory destinationDirectory,
        string destinationLeafName,
        DesignConversionFileIdentity sourceIdentity,
        string operation,
        Exception? renameFailure)
    {
        ValidateLinuxLeafHandle(sourceHandle, sourceLeafName);
        // Fallback допустим только для ещё-не-опубликованного source:
        // один link и та же identity доказывают, что чужой файл не подсунут.
        if (GetLinuxIdentity(sourceHandle, sourceLeafName) != sourceIdentity ||
            GetLinuxStat(sourceHandle, Path.Combine(_rootDirectory, sourceLeafName)).LinkCount != 1)
        {
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound Linux link fallback rejected an unowned source state.");
        }

        // `linkat` сам откажет при EEXIST, но ранняя проверка даёт тот же
        // no-overwrite contract и одинаковую классификацию ошибки для callers.
        using (var existingDestination =
               destinationDirectory.TryOpenExistingReadOnly(destinationLeafName))
        {
            if (existingDestination is not null)
            {
                throw new DesignConversionHandleBoundNameCollisionException(
                    "Handle-bound collision-denying link rejected an occupied destination.");
            }
        }

        try
        {
            if (LinkAtLinux(
                    ToFileDescriptor(_rootHandle),
                    sourceLeafName,
                    ToFileDescriptor(destinationDirectory._rootHandle),
                    destinationLeafName,
                    0) != 0)
            {
                var error = Marshal.GetLastPInvokeError();
                if (error == LinuxEntryExists)
                {
                    throw new DesignConversionHandleBoundNameCollisionException(
                        "Handle-bound collision-denying link rejected an occupied destination.");
                }

                throw new PlatformNotSupportedException(
                    "DesignConversion Linux handle-bound storage needs renameat2 or linkat for handle-bound no-overwrite publication.",
                    renameFailure ?? LinuxIOException(operation, destinationLeafName, error));
            }
        }
        catch (EntryPointNotFoundException ex)
        {
            throw new PlatformNotSupportedException(
                "DesignConversion Linux handle-bound storage needs renameat2 or linkat for handle-bound no-overwrite publication.",
                ex);
        }

        // После `linkat` оба имени должны указывать на тот же inode; link count
        // 2 — ожидаемое короткое crash window до удаления исходного имени.
        destinationDirectory.ValidateLinuxLeafHandle(sourceHandle, destinationLeafName);
        if (GetLinuxIdentity(
                sourceHandle,
                Path.Combine(destinationDirectory._rootDirectory, destinationLeafName)) !=
            sourceIdentity ||
            GetLinuxStat(
                sourceHandle,
                Path.Combine(destinationDirectory._rootDirectory, destinationLeafName)).LinkCount != 2)
        {
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound Linux link fallback published a different file identity.");
        }

        // Сначала fsync destination directory: если процесс упадёт до unlink,
        // recovery увидит две ссылки на один inode и сможет безопасно сверить их.
        destinationDirectory.FlushDirectoryMetadata();
        ValidateLinuxLeafHandle(sourceHandle, sourceLeafName);
        if (GetLinuxIdentity(sourceHandle, sourceLeafName) != sourceIdentity ||
            GetLinuxStat(sourceHandle, Path.Combine(_rootDirectory, sourceLeafName)).LinkCount != 2)
        {
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound Linux link fallback preserved the source after an unsafe replacement.");
        }

        if (UnlinkAtLinux(ToFileDescriptor(_rootHandle), sourceLeafName, 0) != 0)
        {
            throw LinuxIOException(
                "remove source name after linked publication",
                Path.Combine(_rootDirectory, sourceLeafName),
                Marshal.GetLastPInvokeError());
        }

        // Финальный fsync source directory фиксирует исчезновение temp-имени;
        // после него остаётся ровно одно canonical имя опубликованного inode.
        FlushDirectoryMetadata();
        destinationDirectory.ValidateLinuxLeafHandle(sourceHandle, destinationLeafName);
        if (GetLinuxIdentity(
                sourceHandle,
                Path.Combine(destinationDirectory._rootDirectory, destinationLeafName)) !=
            sourceIdentity ||
            GetLinuxStat(
                sourceHandle,
                Path.Combine(destinationDirectory._rootDirectory, destinationLeafName)).LinkCount != 1)
        {
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound Linux link fallback did not finish with one committed name.");
        }
    }

    private void DeleteLinuxLeafByHandle(
        SafeFileHandle handle,
        string leafName,
        uint expectedLinkCount)
    {
        ValidateLinuxLeafHandle(handle, leafName);
        var identity = GetLinuxIdentity(handle, leafName);
        if (GetLinuxStat(handle, Path.Combine(_rootDirectory, leafName)).LinkCount !=
            expectedLinkCount)
        {
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound Linux delete rejected a multiply linked retained file.");
        }

        var descriptor = OpenAtLinux(
            ToFileDescriptor(_rootHandle),
            leafName,
            LinuxOpenReadOnly | LinuxOpenNoFollow | LinuxOpenCloseOnExec,
            0);
        if (descriptor < 0)
        {
            throw LinuxIOException(
                "re-open root-relative file before deletion",
                Path.Combine(_rootDirectory, leafName),
                Marshal.GetLastPInvokeError());
        }
        using var reopened = new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
        ValidateLinuxLeafHandle(reopened, leafName);
        var reopenedStat = GetLinuxStat(
            reopened,
            Path.Combine(_rootDirectory, leafName));
        if (GetLinuxIdentity(reopened, leafName) != identity ||
            reopenedStat.LinkCount != expectedLinkCount)
        {
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound Linux delete preserved a replacement with a different identity.");
        }

        // Имя повторно открыто через удержанный дескриптор корневого каталога и
        // совпало с давно открытым файловым объектом. Только после этого можно
        // удалить именно эту запись каталога; чужая подмена остаётся нетронутой.
        if (UnlinkAtLinux(ToFileDescriptor(_rootHandle), leafName, 0) != 0)
            throw LinuxIOException(
                "delete root-relative file",
                Path.Combine(_rootDirectory, leafName),
                Marshal.GetLastPInvokeError());

        if (GetLinuxIdentity(handle, leafName) != identity)
        {
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound Linux delete target identity changed during deletion.");
        }
    }

    private bool TryDeleteLinuxEmptyRootDirectoryNoGap()
    {
        if (_directoryHandles.Count < 2)
            return false;

        var leafName = Path.GetFileName(_rootDirectory);
        EnsureLinuxLeafName(leafName);
        var parentHandle = _directoryHandles[^2];
        var parentPath = Path.GetDirectoryName(_rootDirectory);
        if (string.IsNullOrEmpty(parentPath))
            parentPath = "/";

        ValidateLinuxHandleKind(parentHandle, parentPath, requireDirectory: true);
        ValidateLinuxHandleKind(_rootHandle, _rootDirectory, requireDirectory: true);
        using var reopened = OpenLinuxDirectoryAt(parentHandle, leafName, out var openError);
        if (reopened.IsInvalid)
        {
            reopened.Dispose();
            return openError == LinuxEntryNotFound
                ? false
                : throw LinuxIOException(
                    "re-open root directory before removal",
                    _rootDirectory,
                    openError);
        }

        ValidateLinuxHandleKind(reopened, _rootDirectory, requireDirectory: true);
        if (GetLinuxIdentity(reopened, _rootDirectory) !=
            GetLinuxIdentity(_rootHandle, _rootDirectory))
        {
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound Linux root directory identity changed before deletion.");
        }

        // Linux удаляет имя каталога через дескриптор его родителя. Перед `rmdir`
        // мы снова доказываем, что имя указывает на удержанный корневой каталог,
        // поэтому пустой чужой каталог по тому же пути удалить невозможно.
        if (UnlinkAtLinux(
                ToFileDescriptor(parentHandle),
                leafName,
                LinuxUnlinkRemoveDirectory) != 0)
        {
            var error = Marshal.GetLastPInvokeError();
            if (error is LinuxEntryNotFound or LinuxDirectoryNotEmpty)
                return false;

            throw LinuxIOException(
                "remove empty root directory through retained parent handle",
                _rootDirectory,
                error);
        }

        FsyncLinuxDirectory(parentHandle, parentPath);
        return true;
    }

    private void ValidateLinuxLeafHandle(SafeFileHandle handle, string leafName)
    {
        ValidateLinuxHandleKind(
            handle,
            Path.Combine(_rootDirectory, leafName),
            requireDirectory: false);
        using var reopened = TryOpenLinuxExistingRegularLeaf(leafName);
        if (reopened is null)
        {
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound Linux leaf disappeared before identity validation.");
        }

        var handleIdentity = GetLinuxIdentity(handle, leafName);
        var reopenedIdentity = GetLinuxIdentity(
            reopened,
            Path.Combine(_rootDirectory, leafName));
        if (handleIdentity != reopenedIdentity)
        {
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound Linux leaf identity changed before safe operation.");
        }
    }

    private SafeFileHandle? TryOpenLinuxExistingRegularLeaf(string leafName)
    {
        var descriptor = OpenAtLinux(
            ToFileDescriptor(_rootHandle),
            leafName,
            LinuxOpenReadOnly | LinuxOpenNoFollow | LinuxOpenCloseOnExec,
            0);
        if (descriptor < 0)
        {
            var error = Marshal.GetLastPInvokeError();
            if (error == LinuxEntryNotFound)
                return null;
            if (error is LinuxTooManySymbolicLinks or LinuxIsDirectory)
            {
                throw new DesignConversionHandleBoundTrustException(
                    "Handle-bound Linux safe open rejected a linked or non-regular leaf.");
            }

            throw LinuxIOException(
                "re-open root-relative file for identity validation",
                Path.Combine(_rootDirectory, leafName),
                error);
        }

        var handle = new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
        try
        {
            ValidateLinuxHandleKind(
                handle,
                Path.Combine(_rootDirectory, leafName),
                requireDirectory: false);
            return handle;
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    private void EnumerateLinuxNamesBounded(
        int maximumEntryCount,
        Action<string> onName)
    {
        ArgumentNullException.ThrowIfNull(onName);
        // fdopendir читает и двигает позицию самого file description. Поэтому
        // для каждой инвентаризации открываем новый fd на уже закреплённый root:
        // повторный обход каталога не наследует EOF от предыдущего обхода.
        var enumerationDescriptor = OpenAtLinux(
            ToFileDescriptor(_rootHandle),
            ".",
            LinuxDirectoryOpenFlags,
            0);
        if (enumerationDescriptor < 0)
            throw LinuxIOException(
                "open independent root directory stream for enumeration",
                _rootDirectory,
                Marshal.GetLastPInvokeError());

        var directory = IntPtr.Zero;
        try
        {
            directory = FdOpenDirLinux(enumerationDescriptor);
            if (directory == IntPtr.Zero)
            {
                var error = Marshal.GetLastPInvokeError();
                _ = CloseLinux(enumerationDescriptor);
                enumerationDescriptor = -1;
                throw LinuxIOException("open DIR stream from root handle", _rootDirectory, error);
            }

            var observedEntryCount = 0;
            while (true)
            {
                // readdir сообщает ошибку только через errno: NULL с errno=0
                // означает обычный конец каталога, а ненулевой errno — сбой.
                Marshal.SetLastPInvokeError(0);
                var entry = ReadDirLinux(directory);
                if (entry == IntPtr.Zero)
                {
                    var error = Marshal.GetLastPInvokeError();
                    if (error != 0)
                    {
                        throw LinuxIOException(
                            "enumerate root directory stream",
                            _rootDirectory,
                            error);
                    }

                    break;
                }

                var namePointer = IntPtr.Add(entry, LinuxDirentNameOffset);
                var name = Marshal.PtrToStringUTF8(namePointer)
                    ?? throw new InvalidDataException(
                        "Handle-bound Linux directory entry name is invalid UTF-8.");
                if (name is "." or "..")
                    continue;

                EnsureLinuxLeafName(name);
                observedEntryCount = checked(observedEntryCount + 1);
                if (observedEntryCount > maximumEntryCount)
                {
                    throw new DesignConversionHandleBoundEnumerationLimitException(
                        "Handle-bound directory entry ceiling was exceeded.");
                }

                onName(name);
            }
        }
        finally
        {
            if (directory != IntPtr.Zero)
                _ = CloseDirLinux(directory);
            else if (enumerationDescriptor >= 0)
                _ = CloseLinux(enumerationDescriptor);
        }
    }

    private static void ValidateLinuxHandleKind(
        SafeFileHandle handle,
        string expectedPath,
        bool requireDirectory)
    {
        var stat = GetLinuxStat(handle, expectedPath);
        var fileType = stat.Mode & LinuxFileTypeMask;
        var isDirectory = fileType == LinuxDirectoryFileType;
        var isRegular = fileType == LinuxRegularFileType;
        if (requireDirectory != isDirectory ||
            (!requireDirectory && !isRegular))
        {
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound Linux safe open resolved an unexpected handle kind.");
        }
    }

    private static DesignConversionFileIdentity GetLinuxIdentity(
        SafeFileHandle handle,
        string expectedPath)
    {
        var stat = GetLinuxStat(handle, expectedPath);
        return new DesignConversionFileIdentity(
            stat.Device,
            stat.Inode,
            0UL);
    }

    private static void ValidateLinuxOwnerOnly(
        LinuxStatView stat,
        bool requireDirectory,
        string path)
    {
        var requiredMode = requireDirectory
            ? LinuxOwnerDirectoryMode
            : LinuxOwnerFileMode;
        var permissionBits = stat.Mode & LinuxPermissionBitsMask;
        if (stat.Uid != GetEffectiveUserIdLinux() ||
            (permissionBits & LinuxGroupOrOtherAccessMask) != 0 ||
            (permissionBits & requiredMode) != requiredMode)
        {
            throw new DesignConversionHandleBoundTrustException(
                $"Handle-bound Linux custody owner-only mode was not proven for '{path}'.");
        }
    }

    private static void ValidateLinuxCustodyStatTrustContract(
        LinuxStatView stat,
        bool requireDirectory,
        string path)
    {
        if (stat.FileSystemType != LinuxV9FsMagic ||
            IsLinuxOwnerOnlyStat(stat, requireDirectory))
        {
            ValidateLinuxOwnerOnly(stat, requireDirectory, path);
            return;
        }

        var permissionBits = stat.Mode & LinuxPermissionBitsMask;
        // На v9fs это не называется «только владелец»: WSL отдаёт ожидаемую
        // проекцию Windows-диска, а приватность POSIX здесь не доказана.
        if (stat.Uid != 0 || permissionBits != LinuxPermissionBitsMask)
        {
            throw new DesignConversionHandleBoundTrustException(
                $"Handle-bound Linux custody could not prove a supported v9fs host-user boundary for '{path}'.");
        }
    }

    private static bool IsLinuxOwnerOnlyStat(
        LinuxStatView stat,
        bool requireDirectory)
    {
        var requiredMode = requireDirectory
            ? LinuxOwnerDirectoryMode
            : LinuxOwnerFileMode;
        var permissionBits = stat.Mode & LinuxPermissionBitsMask;
        return stat.Uid == GetEffectiveUserIdLinux() &&
               (permissionBits & LinuxGroupOrOtherAccessMask) == 0 &&
               (permissionBits & requiredMode) == requiredMode;
    }

    private static bool IsLinuxNoReplaceRenameUnsupported(int error)
        => error is LinuxInvalidArgument or
            LinuxFunctionNotImplemented or
            LinuxOperationNotSupported;

    private static LinuxStatView GetLinuxStat(
        SafeFileHandle handle,
        string expectedPath)
    {
        EnsureLinuxX64StatLayout();
        if (handle.IsInvalid || handle.IsClosed)
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound storage received an invalid handle.");
        if (FStatLinux(ToFileDescriptor(handle), out var stat) != 0)
        {
            throw LinuxIOException(
                "inspect handle",
                expectedPath,
                Marshal.GetLastPInvokeError());
        }
        if (FStatFsLinux(ToFileDescriptor(handle), out var statFs) != 0)
        {
            throw LinuxIOException(
                "inspect handle filesystem",
                expectedPath,
                Marshal.GetLastPInvokeError());
        }

        return new LinuxStatView(
            stat.Device,
            stat.Inode,
            stat.LinkCount,
            stat.Mode,
            stat.Uid,
            statFs.Type);
    }

    private static void EnsureLinuxX64StatLayout()
    {
        if (!OperatingSystem.IsLinux() ||
            RuntimeInformation.ProcessArchitecture != Architecture.X64)
        {
            throw new PlatformNotSupportedException(
                "Handle-bound Linux identity is verified only for the x64 stat layout.");
        }
    }

    private static void FsyncLinuxDirectory(SafeFileHandle directoryHandle, string path)
    {
        if (FsyncLinux(ToFileDescriptor(directoryHandle)) != 0)
            throw LinuxIOException(
                "durably flush root directory metadata",
                path,
                Marshal.GetLastPInvokeError());
    }

    private static int ToFileDescriptor(SafeFileHandle handle)
    {
        if (handle.IsInvalid || handle.IsClosed)
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound storage received an invalid handle.");
        return checked((int)handle.DangerousGetHandle().ToInt64());
    }

    private static SafeFileHandle OpenWindowsDirectory(string path, out int error)
    {
        var handle = CreateFileWindows(
            path,
            FileListDirectory | FileReadAttributes | FileTraverse | Synchronize,
            (uint)(FileShare.Read | FileShare.Write | FileShare.Delete),
            IntPtr.Zero,
            OpenExisting,
            FileFlagBackupSemantics | FileFlagOpenReparsePoint,
            IntPtr.Zero);
        error = handle.IsInvalid ? Marshal.GetLastPInvokeError() : 0;
        return handle;
    }

    private static SafeFileHandle NtOpenWindowsDirectory(
        SafeFileHandle parentHandle,
        string segment,
        uint createDisposition,
        bool requireDurableWriteAccess,
        bool denyDeleteSharing,
        out int status)
        => NtOpenWindowsRelative(
            parentHandle,
            segment,
            FileListDirectory |
            FileReadAttributes |
            FileTraverse |
            Synchronize |
            (requireDurableWriteAccess ? GenericWrite : 0),
            denyDeleteSharing
                ? FileShare.Read | FileShare.Write
                : FileShare.Read | FileShare.Write | FileShare.Delete,
            createDisposition,
            FileAttributeDirectory,
            FileSynchronousIoNonAlert | FileDirectoryFile | FileOpenReparsePoint,
            out status);

    private static SafeFileHandle NtOpenWindowsRelative(
        SafeFileHandle rootHandle,
        string relativeName,
        uint desiredAccess,
        FileShare share,
        uint createDisposition,
        uint fileAttributes,
        uint createOptions,
        out int status)
    {
        var nameBuffer = Marshal.StringToHGlobalUni(relativeName);
        var unicodeStringPointer = Marshal.AllocHGlobal(Marshal.SizeOf<UnicodeString>());
        var rootHandleAddRef = false;
        try
        {
            var nameLength = checked((ushort)(relativeName.Length * sizeof(char)));
            var unicodeString = new UnicodeString
            {
                Length = nameLength,
                MaximumLength = checked((ushort)(nameLength + sizeof(char))),
                Buffer = nameBuffer
            };
            Marshal.StructureToPtr(unicodeString, unicodeStringPointer, fDeleteOld: false);

            rootHandle.DangerousAddRef(ref rootHandleAddRef);
            var objectAttributes = new ObjectAttributes
            {
                Length = Marshal.SizeOf<ObjectAttributes>(),
                RootDirectory = rootHandle.DangerousGetHandle(),
                ObjectName = unicodeStringPointer,
                Attributes = ObjectCaseInsensitive | ObjectDontReparse,
                SecurityDescriptor = IntPtr.Zero,
                SecurityQualityOfService = IntPtr.Zero
            };
            status = NtCreateFileWindows(
                out var handle,
                desiredAccess,
                ref objectAttributes,
                out _,
                IntPtr.Zero,
                fileAttributes,
                (uint)share,
                createDisposition,
                createOptions,
                IntPtr.Zero,
                0);
            return handle;
        }
        finally
        {
            if (rootHandleAddRef)
                rootHandle.DangerousRelease();
            Marshal.FreeHGlobal(unicodeStringPointer);
            Marshal.FreeHGlobal(nameBuffer);
        }
    }

    private static void ValidateWindowsHandle(
        SafeFileHandle handle,
        string expectedPath,
        bool requireDirectory)
    {
        if (handle.IsInvalid || handle.IsClosed)
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound storage received an invalid handle.");
        if (!GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileAttributeTagInfo,
                out FileAttributeTagInformation info,
                (uint)Marshal.SizeOf<FileAttributeTagInformation>()))
        {
            throw WindowsIOException("inspect handle", expectedPath, Marshal.GetLastPInvokeError());
        }

        if ((info.FileAttributes & FileAttributeReparsePoint) != 0)
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound safe open rejected a reparse-point handle.");
        var isDirectory = (info.FileAttributes & FileAttributeDirectory) != 0;
        if (isDirectory != requireDirectory)
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound safe open resolved an unexpected handle kind.");

        var finalPath = GetWindowsFinalPath(handle, expectedPath);
        if (!string.Equals(
                NormalizeWindowsPath(finalPath),
                NormalizeWindowsPath(expectedPath),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound safe open resolved outside the expected path.");
        }
    }

    private static DesignConversionFileIdentity GetWindowsIdentity(
        SafeFileHandle handle,
        string expectedPath)
    {
        if (!GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileIdInfo,
                out FileIdInformation info,
                (uint)Marshal.SizeOf<FileIdInformation>()))
        {
            throw WindowsIOException("read handle identity", expectedPath, Marshal.GetLastPInvokeError());
        }

        return new DesignConversionFileIdentity(
            info.VolumeSerialNumber,
            info.FileId.Low,
            info.FileId.High);
    }

    private static string GetWindowsFinalPath(SafeFileHandle handle, string expectedPath)
    {
        var capacity = 512;
        while (capacity <= 32 * 1024)
        {
            var buffer = new StringBuilder(capacity);
            var length = GetFinalPathNameByHandleWindows(
                handle,
                buffer,
                (uint)capacity,
                0);
            if (length == 0)
            {
                throw WindowsIOException(
                    "resolve final handle path",
                    expectedPath,
                    Marshal.GetLastPInvokeError());
            }

            if (length < capacity)
                return buffer.ToString();
            capacity = checked((int)length + 1);
        }

        throw new InvalidDataException("Handle-bound final path exceeds the safe limit.");
    }

    private static string NormalizeWindowsPath(string path)
    {
        if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
            path = @"\\" + path[8..];
        else if (path.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase))
            path = path[4..];

        var fullPath = Path.GetFullPath(path);
        var root = Path.GetPathRoot(fullPath);
        return string.Equals(fullPath, root, StringComparison.OrdinalIgnoreCase)
            ? fullPath
            : Path.TrimEndingDirectorySeparator(fullPath);
    }

    private static void EnsureLeafName(string leafName)
    {
        if (string.IsNullOrWhiteSpace(leafName) ||
            leafName.IndexOf('\0') >= 0 ||
            !string.Equals(Path.GetFileName(leafName), leafName, StringComparison.Ordinal) ||
            leafName is "." or "..")
        {
            throw new InvalidDataException("Handle-bound leaf name is invalid.");
        }
    }

    private void EnsureLeafNameForBackend(string leafName)
    {
        if (_backend == StorageBackend.Linux)
        {
            EnsureLinuxLeafName(leafName);
            return;
        }

        EnsureLeafName(leafName);
    }

    private static void EnsureLinuxLeafName(string leafName)
    {
        EnsureLeafName(leafName);
        // Ошибка размера имени здесь является входной validation до syscall,
        // а не признаком повреждения сохранённого состояния: Linux считает
        // компонент пути байтами, Windows — UTF-16 символами.
        if (Encoding.UTF8.GetByteCount(leafName) > LinuxNameMaxUtf8Bytes)
        {
            throw new ArgumentException(
                "Handle-bound Linux leaf name exceeds the 255-byte UTF-8 limit.",
                nameof(leafName));
        }
    }

    private static IOException WindowsIOException(string operation, string path, int error)
        => new(
            $"Handle-bound storage failed to {operation} '{path}' " +
            $"(Win32 error {error}: {new Win32Exception(error).Message}).");

    private static IOException WindowsNtIOException(string operation, string path, int status)
    {
        var error = RtlNtStatusToDosErrorWindows(status);
        return new IOException(
            $"Handle-bound storage failed to {operation} '{path}' " +
            $"(NTSTATUS 0x{unchecked((uint)status):X8}, Win32 error {error}).");
    }

    private static IOException LinuxIOException(string operation, string path, int error)
        => new(
            $"Handle-bound storage failed to {operation} '{path}' " +
            $"(errno {error}: {new Win32Exception(error).Message}).");

    private sealed record DestinationSnapshot(
        FileStream Stream) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
                Stream.Dispose();
        }
    }

    private enum StorageBackend
    {
        Windows,
        Linux
    }

    private readonly record struct LinuxStatView(
        ulong Device,
        ulong Inode,
        ulong LinkCount,
        uint Mode,
        uint Uid,
        long FileSystemType);

    private enum FileInfoByHandleClass
    {
        FileStandardInfo = 1,
        FileRenameInfo = 3,
        FileDispositionInfo = 4,
        FileAttributeTagInfo = 9,
        FileIdInfo = 18
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileAttributeTagInformation
    {
        public uint FileAttributes;
        public uint ReparseTag;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileStandardInformation
    {
        public long AllocationSize;
        public long EndOfFile;
        public uint NumberOfLinks;
        public byte DeletePending;
        public byte Directory;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileId128
    {
        public ulong Low;
        public ulong High;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileIdInformation
    {
        public ulong VolumeSerialNumber;
        public FileId128 FileId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct UnicodeString
    {
        public ushort Length;
        public ushort MaximumLength;
        public IntPtr Buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ObjectAttributes
    {
        public int Length;
        public IntPtr RootDirectory;
        public IntPtr ObjectName;
        public uint Attributes;
        public IntPtr SecurityDescriptor;
        public IntPtr SecurityQualityOfService;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IoStatusBlock
    {
        public IntPtr Status;
        public IntPtr Information;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LinuxStat
    {
        public ulong Device;
        public ulong Inode;
        public ulong LinkCount;
        public uint Mode;
        public uint Uid;
        public uint Gid;
        public int Padding0;
        public ulong DeviceId;
        public long Size;
        public long BlockSize;
        public long Blocks;
        public long AccessTimeSeconds;
        public long AccessTimeNanoseconds;
        public long ModificationTimeSeconds;
        public long ModificationTimeNanoseconds;
        public long ChangeTimeSeconds;
        public long ChangeTimeNanoseconds;
        public long Unused0;
        public long Unused1;
        public long Unused2;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LinuxStatFs
    {
        public long Type;
        public long BlockSize;
        public ulong Blocks;
        public ulong BlocksFree;
        public ulong BlocksAvailable;
        public ulong Files;
        public ulong FilesFree;
        public int FileSystemId0;
        public int FileSystemId1;
        public long NameLength;
        public long FragmentSize;
        public long Flags;
        public long Spare0;
        public long Spare1;
        public long Spare2;
        public long Spare3;
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFileWindows(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleEx", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleExWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        out FileAttributeTagInformation fileInformation,
        uint bufferSize);

    [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleEx", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleExWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        out FileIdInformation fileInformation,
        uint bufferSize);

    [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleEx", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleExWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        out FileStandardInformation fileInformation,
        uint bufferSize);

    [DllImport("kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetFinalPathNameByHandleWindows(
        SafeFileHandle file,
        StringBuilder filePath,
        uint filePathLength,
        uint flags);

    [DllImport("kernel32.dll", EntryPoint = "SetFileInformationByHandle", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetFileInformationByHandleWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        IntPtr fileInformation,
        uint bufferSize);

    [DllImport("ntdll.dll", EntryPoint = "NtCreateFile")]
    private static extern int NtCreateFileWindows(
        out SafeFileHandle fileHandle,
        uint desiredAccess,
        ref ObjectAttributes objectAttributes,
        out IoStatusBlock ioStatusBlock,
        IntPtr allocationSize,
        uint fileAttributes,
        uint shareAccess,
        uint createDisposition,
        uint createOptions,
        IntPtr extendedAttributes,
        uint extendedAttributesLength);

    [DllImport("ntdll.dll", EntryPoint = "NtQueryDirectoryFile")]
    private static extern int NtQueryDirectoryFileWindows(
        SafeFileHandle fileHandle,
        IntPtr @event,
        IntPtr apcRoutine,
        IntPtr apcContext,
        out IoStatusBlock ioStatusBlock,
        IntPtr fileInformation,
        int length,
        int fileInformationClass,
        [MarshalAs(UnmanagedType.Bool)] bool returnSingleEntry,
        IntPtr fileName,
        [MarshalAs(UnmanagedType.Bool)] bool restartScan);

    [DllImport("ntdll.dll", EntryPoint = "NtFlushBuffersFile")]
    private static extern int NtFlushBuffersFileWindows(
        SafeFileHandle fileHandle,
        out IoStatusBlock ioStatusBlock);

    [DllImport("ntdll.dll", EntryPoint = "NtSetInformationFile")]
    private static extern int NtSetInformationFileWindows(
        SafeFileHandle fileHandle,
        out IoStatusBlock ioStatusBlock,
        IntPtr fileInformation,
        uint length,
        int fileInformationClass);

    [DllImport("ntdll.dll", EntryPoint = "RtlNtStatusToDosError")]
    private static extern uint RtlNtStatusToDosErrorWindows(int status);

    [DllImport("libc", EntryPoint = "open", SetLastError = true)]
    private static extern int OpenLinux(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags,
        uint mode);

    [DllImport("libc", EntryPoint = "lstat", SetLastError = true)]
    private static extern int LStatLinux(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        out LinuxStat stat);

    [DllImport("libc", EntryPoint = "openat", SetLastError = true)]
    private static extern int OpenAtLinux(
        int directoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags,
        uint mode);

    [DllImport("libc", EntryPoint = "mkdirat", SetLastError = true)]
    private static extern int MakeDirectoryAtLinux(
        int directoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        uint mode);

    [DllImport("libc", EntryPoint = "renameat2", SetLastError = true)]
    private static extern int RenameAt2Linux(
        int oldDirectoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string oldPath,
        int newDirectoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string newPath,
        uint flags);

    [DllImport("libc", EntryPoint = "linkat", SetLastError = true)]
    private static extern int LinkAtLinux(
        int oldDirectoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string oldPath,
        int newDirectoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string newPath,
        int flags);

    [DllImport("libc", EntryPoint = "unlinkat", SetLastError = true)]
    private static extern int UnlinkAtLinux(
        int directoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags);

    [DllImport("libc", EntryPoint = "fsync", SetLastError = true)]
    private static extern int FsyncLinux(int fileDescriptor);

    [DllImport("libc", EntryPoint = "fstat", SetLastError = true)]
    private static extern int FStatLinux(
        int fileDescriptor,
        out LinuxStat stat);

    [DllImport("libc", EntryPoint = "fstatfs", SetLastError = true)]
    private static extern int FStatFsLinux(
        int fileDescriptor,
        out LinuxStatFs stat);

    [DllImport("libc", EntryPoint = "flock", SetLastError = true)]
    private static extern int FlockLinux(
        int fileDescriptor,
        int operation);

    [DllImport("libc", EntryPoint = "geteuid")]
    private static extern uint GetEffectiveUserIdLinux();

    [DllImport("libc", EntryPoint = "fdopendir", SetLastError = true)]
    private static extern IntPtr FdOpenDirLinux(int fileDescriptor);

    [DllImport("libc", EntryPoint = "readdir", SetLastError = true)]
    private static extern IntPtr ReadDirLinux(IntPtr directory);

    [DllImport("libc", EntryPoint = "closedir", SetLastError = true)]
    private static extern int CloseDirLinux(IntPtr directory);

    [DllImport("libc", EntryPoint = "close", SetLastError = true)]
    private static extern int CloseLinux(int fileDescriptor);
}
