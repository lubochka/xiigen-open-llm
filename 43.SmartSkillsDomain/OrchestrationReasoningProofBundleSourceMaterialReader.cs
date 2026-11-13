// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using AgentInfrastructure;
using Microsoft.Win32.SafeHandles;

/// <summary>
/// Reads checkpoint proof source materials only through the opened checkpoint
/// bundle boundary. This is intentionally not used by direct trainer
/// materialization: it protects proof rehydration from portable refs that are
/// lexically inside the bundle but escape through a symlink/junction/reparse
/// point before source admission reads content.
/// </summary>
internal static class OrchestrationReasoningProofBundleSourceMaterialReader
{
    private const long MaximumProofSourceMaterialBytes = 64L * 1024L * 1024L;
    private const int ReadBufferSize = 64 * 1024;
    private const string BoundaryMessage =
        "Admission proof source material refs must resolve to regular files inside the opened checkpoint bundle directory.";

    private const int PosixOpenReadOnly = 0;
    private const int PosixOpenDirectory = 0x10000;
    private const int PosixOpenNoFollow = 0x20000;
    private const int PosixOpenCloseOnExec = 0x80000;
    private const int PosixDirectoryOpenFlags =
        PosixOpenReadOnly |
        PosixOpenDirectory |
        PosixOpenNoFollow |
        PosixOpenCloseOnExec;
    private const int PosixRegularReadOpenFlags =
        PosixOpenReadOnly |
        PosixOpenNoFollow |
        PosixOpenCloseOnExec;
    private const uint PosixFileTypeMask = 0xF000;
    private const uint PosixRegularFileType = 0x8000;
    private const uint PosixDirectoryFileType = 0x4000;
    private const int PosixEntryNotFound = 2;
    private const int PosixPermissionDenied = 13;
    private const int PosixNotDirectory = 20;
    private const int PosixIsDirectory = 21;
    private const int PosixNameTooLong = 36;
    private const int PosixTooManySymbolicLinks = 40;
    private const int PosixNameMaxUtf8Bytes = 255;

    private const uint WindowsFileListDirectory = 0x0001;
    private const uint WindowsShareRead = 0x00000001;
    private const uint WindowsShareWrite = 0x00000002;
    private const uint WindowsShareDelete = 0x00000004;
    private const uint WindowsBundleRootShareMode = WindowsShareRead | WindowsShareWrite;
    private const uint WindowsLeafFileShareMode = WindowsShareRead;
    private const uint WindowsOpenExisting = 3;
    private const uint WindowsFileFlagBackupSemantics = 0x02000000;
    private const uint WindowsFileAttributeDirectory = 0x00000010;

    internal static async Task<OperationResult<OrchestrationReasoningVerifiedSourceMaterial>> AdmitAsync(
        string bundleDirectory,
        OrchestrationReasoningSourceMaterialReference source,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<OrchestrationReasoningVerifiedSourceMaterial>.Fail("Load cancelled.", "CANCELLED");
        if (source is null)
            return OutsideBundle<OrchestrationReasoningVerifiedSourceMaterial>();

        try
        {
            using var bundle = OpenBundle(bundleDirectory);
            var payload = await ReadBundleFileAsync(bundle, source.PayloadRef, ct);
            var manifest = await ReadBundleFileAsync(bundle, source.ManifestRef, ct);
            var ledger = await ReadBundleFileAsync(bundle, source.LedgerRef, ct);
            var evidence = await ReadBundleFileAsync(bundle, source.EvidenceRef, ct);

            return OrchestrationReasoningSourceAdmission.AdmitVerifiedSourceMaterial(
                payload.Path,
                manifest.Path,
                ledger.Path,
                evidence.Path,
                payload.Bytes,
                manifest.Bytes,
                ledger.Bytes,
                evidence.Bytes);
        }
        catch (ProofBundleReadException ex)
        {
            return OperationResult<OrchestrationReasoningVerifiedSourceMaterial>.Fail(ex.Message, ex.ErrorCode);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return OperationResult<OrchestrationReasoningVerifiedSourceMaterial>.Fail("Load cancelled.", "CANCELLED");
        }
    }

    private static OpenedBundle OpenBundle(string bundleDirectory)
    {
        if (string.IsNullOrWhiteSpace(bundleDirectory))
            throw Boundary();
        var fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(bundleDirectory));
        if (!Directory.Exists(fullRoot))
            throw Rejected("Checkpoint bundle directory is missing.");

        if (OperatingSystem.IsLinux())
            return OpenPosixBundle(fullRoot);
        if (OperatingSystem.IsWindows())
            return OpenWindowsBundle(fullRoot);

        throw Rejected("Checkpoint proof source-material custody is supported only on Linux and Windows.");
    }

    private static async Task<ProofBundleFile> ReadBundleFileAsync(
        OpenedBundle bundle,
        string reference,
        CancellationToken ct)
    {
        var normalized = NormalizePortableReference(reference);
        var requestedPath = Path.GetFullPath(Path.Combine(
            bundle.RequestedRoot,
            normalized.Replace('/', Path.DirectorySeparatorChar)));
        if (!IsInsideOrEqual(requestedPath, bundle.RequestedRoot))
            throw Boundary();

        if (bundle.Platform == ProofBundlePlatform.Linux)
            return await ReadBundleFileLinuxAsync(bundle, normalized, requestedPath, ct);
        if (bundle.Platform == ProofBundlePlatform.Windows)
            return await ReadBundleFileWindowsAsync(bundle, requestedPath, ct);

        throw Rejected("Checkpoint proof source-material custody is unsupported.");
    }

    private static async Task<ProofBundleFile> ReadBundleFileLinuxAsync(
        OpenedBundle bundle,
        string normalizedReference,
        string requestedPath,
        CancellationToken ct)
    {
        var handle = OpenLinuxNoFollow(bundle.RootHandle, normalizedReference);
        try
        {
            var stat = GetPosixStat(handle);
            if ((stat.Mode & PosixFileTypeMask) != PosixRegularFileType)
                throw Rejected("Checkpoint proof source material is not a regular file.");
            var ownedHandle = handle;
            handle = null;
            var bytes = await ReadSafeHandleBytesAsync(ownedHandle, stat.Size, ct);
            return new ProofBundleFile(requestedPath, bytes);
        }
        finally
        {
            handle?.Dispose();
        }
    }

    private static async Task<ProofBundleFile> ReadBundleFileWindowsAsync(
        OpenedBundle bundle,
        string requestedPath,
        CancellationToken ct)
    {
        try
        {
            await using var stream = new FileStream(requestedPath, new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = (FileShare)WindowsLeafFileShareMode,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
                BufferSize = ReadBufferSize
            });
            var currentRoot = CaptureWindowsDirectorySnapshot(bundle.RootHandle);
            var leaf = CaptureWindowsFileSnapshot(stream.SafeFileHandle);
            if ((leaf.Information.FileAttributes & WindowsFileAttributeDirectory) != 0)
                throw Rejected("Checkpoint proof source material is not a regular file.");

            // Windows relative no-follow open is intentionally not replicated in
            // 43. The root handle is held without FILE_SHARE_DELETE, then after
            // the leaf is opened we re-sample both live handles: current root
            // identity/final path and leaf identity/final path. Parent ancestors
            // can still be renamed, so the containment comparison uses the
            // current root path captured after leaf open, and bytes are read only
            // from that already-validated leaf handle.
            if (leaf.Information.VolumeSerialNumber != currentRoot.Information.VolumeSerialNumber)
                throw Boundary();
            if (!IsInsideOrEqual(leaf.FinalPath, currentRoot.FinalPath))
                throw Boundary();

            if (stream.Length > MaximumProofSourceMaterialBytes)
                throw Rejected("Checkpoint proof source material exceeds the size boundary.");
            var bytes = await ReadStreamBytesAsync(stream, stream.Length, ct);
            return new ProofBundleFile(requestedPath, bytes);
        }
        catch (FileNotFoundException ex)
        {
            throw Rejected(ex.Message);
        }
        catch (DirectoryNotFoundException ex)
        {
            throw Rejected(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw Rejected(ex.Message);
        }
        catch (IOException ex)
        {
            throw Rejected(ex.Message);
        }
    }

    private static OpenedBundle OpenPosixBundle(string fullRoot)
    {
        EnsurePosixLinuxX64StatLayout();
        var descriptor = OpenPosix(fullRoot, PosixDirectoryOpenFlags, 0);
        if (descriptor < 0)
            throw MapPosixOpenError(Marshal.GetLastPInvokeError());
        var handle = new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
        try
        {
            var stat = GetPosixStat(handle);
            if ((stat.Mode & PosixFileTypeMask) != PosixDirectoryFileType)
                throw Boundary();
            return new OpenedBundle(
                ProofBundlePlatform.Linux,
                fullRoot,
                handle);
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    private static SafeFileHandle OpenLinuxNoFollow(SafeFileHandle root, string normalizedReference)
    {
        var segments = normalizedReference.Split('/');
        SafeFileHandle? current = null;
        var parent = root;
        try
        {
            for (var index = 0; index < segments.Length - 1; index++)
            {
                var next = OpenAtPosixHandle(parent, segments[index], requireDirectory: true);
                current?.Dispose();
                current = next;
                parent = current;
            }

            var leaf = OpenAtPosixHandle(parent, segments[^1], requireDirectory: false);
            current?.Dispose();
            current = null;
            return leaf;
        }
        catch
        {
            current?.Dispose();
            throw;
        }
    }

    private static SafeFileHandle OpenAtPosixHandle(
        SafeFileHandle parent,
        string name,
        bool requireDirectory)
    {
        EnsureSingleName(name);
        var descriptor = OpenAtPosix(
            ToPosixFileDescriptor(parent),
            name,
            requireDirectory ? PosixDirectoryOpenFlags : PosixRegularReadOpenFlags,
            0);
        if (descriptor < 0)
            throw MapPosixOpenError(Marshal.GetLastPInvokeError());
        return new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
    }

    private static OpenedBundle OpenWindowsBundle(string fullRoot)
    {
        var handle = CreateFileW(
            fullRoot,
            WindowsFileListDirectory,
            WindowsBundleRootShareMode,
            IntPtr.Zero,
            WindowsOpenExisting,
            WindowsFileFlagBackupSemantics,
            IntPtr.Zero);
        if (handle.IsInvalid)
        {
            handle.Dispose();
            throw Rejected("Checkpoint bundle directory could not be opened.");
        }

        try
        {
            var information = GetWindowsFileInformation(handle);
            if ((information.FileAttributes & WindowsFileAttributeDirectory) == 0)
                throw Boundary();
            return new OpenedBundle(
                ProofBundlePlatform.Windows,
                fullRoot,
                handle);
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    private static async Task<byte[]> ReadSafeHandleBytesAsync(
        SafeFileHandle handle,
        long expectedLength,
        CancellationToken ct)
    {
        FileStream? stream = null;
        try
        {
            stream = new FileStream(handle, FileAccess.Read, ReadBufferSize, isAsync: false);
            return await ReadStreamBytesAsync(stream, expectedLength, ct);
        }
        finally
        {
            if (stream is not null)
                await stream.DisposeAsync();
            else
                handle.Dispose();
        }
    }

    private static async Task<byte[]> ReadStreamBytesAsync(
        FileStream stream,
        long expectedLength,
        CancellationToken ct)
    {
        if (expectedLength < 0 || expectedLength > MaximumProofSourceMaterialBytes)
            throw Rejected("Checkpoint proof source material exceeds the size boundary.");
        if (expectedLength > int.MaxValue)
            throw Rejected("Checkpoint proof source material exceeds the in-memory read boundary.");

        var bytes = new byte[(int)expectedLength];
        var offset = 0;
        while (offset < bytes.Length)
        {
            ct.ThrowIfCancellationRequested();
            var read = await stream.ReadAsync(bytes.AsMemory(offset), ct);
            if (read == 0)
                break;
            offset += read;
        }

        if (offset != bytes.Length || stream.Length != expectedLength)
            throw Rejected("Checkpoint proof source material changed while it was being read.");
        return bytes;
    }

    private static string NormalizePortableReference(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            throw Boundary();

        var normalized = reference.Trim().Replace('\\', '/');
        if (normalized.StartsWith("/", StringComparison.Ordinal)
            || normalized.StartsWith("//", StringComparison.Ordinal)
            || IsWindowsDriveQualifiedReference(normalized))
        {
            throw Boundary();
        }

        var segments = new List<string>();
        foreach (var segment in normalized.Split('/'))
        {
            if (segment.Length == 0 || string.Equals(segment, ".", StringComparison.Ordinal))
                continue;
            if (string.Equals(segment, "..", StringComparison.Ordinal)
                || segment.Contains('\0', StringComparison.Ordinal))
            {
                throw Boundary();
            }
            if (OperatingSystem.IsLinux() && Encoding.UTF8.GetByteCount(segment) > PosixNameMaxUtf8Bytes)
                throw Boundary();
            segments.Add(segment);
        }

        if (segments.Count == 0)
            throw Boundary();
        return string.Join("/", segments);
    }

    private static bool IsWindowsDriveQualifiedReference(string reference)
        => reference.Length >= 2
           && reference[1] == ':'
           && ((reference[0] >= 'A' && reference[0] <= 'Z')
               || (reference[0] >= 'a' && reference[0] <= 'z'));

    private static bool IsInsideOrEqual(string path, string root)
    {
        var fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        var fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        if (string.Equals(
                fullPath,
                fullRoot,
                OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        {
            return true;
        }

        var prefix = fullRoot + Path.DirectorySeparatorChar;
        return fullPath.StartsWith(
            prefix,
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    private static ProofBundleReadException MapPosixOpenError(int error)
        => error switch
        {
            PosixTooManySymbolicLinks => Boundary(),
            PosixNameTooLong => Boundary(),
            PosixEntryNotFound => Rejected("Checkpoint proof source material is missing."),
            PosixNotDirectory => Rejected("Checkpoint proof source material parent is not a directory."),
            PosixIsDirectory => Rejected("Checkpoint proof source material is not a regular file."),
            PosixPermissionDenied => Rejected("Checkpoint proof source material could not be opened."),
            _ => Rejected("Checkpoint proof source material could not be opened.")
        };

    private static ProofBundleReadException Boundary()
        => new(BoundaryMessage, OrchestrationReasoningSourceAdmission.ProofSourceMaterialOutsideBundleCode);

    private static OperationResult<T> OutsideBundle<T>()
        => OperationResult<T>.Fail(
            BoundaryMessage,
            OrchestrationReasoningSourceAdmission.ProofSourceMaterialOutsideBundleCode);

    private static ProofBundleReadException Rejected(string message)
        => new(message, OrchestrationReasoningSourceAdmission.AdmissionRejectedCode);

    private static PosixStatView GetPosixStat(SafeFileHandle handle)
    {
        EnsurePosixLinuxX64StatLayout();
        if (handle.IsInvalid || handle.IsClosed)
            throw Boundary();
        if (FStatPosix(ToPosixFileDescriptor(handle), out var stat) != 0)
            throw Rejected("Checkpoint proof source material identity is unavailable.");
        return new PosixStatView(stat.Device, stat.Inode, stat.Mode, stat.Size);
    }

    private static WindowsByHandleFileInformation GetWindowsFileInformation(SafeFileHandle handle)
    {
        if (handle.IsInvalid || handle.IsClosed)
            throw Boundary();
        if (!GetFileInformationByHandle(handle, out var information))
            throw Rejected("Checkpoint proof source material identity is unavailable.");
        return information;
    }

    private static WindowsOpenedPathSnapshot CaptureWindowsDirectorySnapshot(SafeFileHandle handle)
    {
        var information = GetWindowsFileInformation(handle);
        if ((information.FileAttributes & WindowsFileAttributeDirectory) == 0)
            throw Boundary();
        return new WindowsOpenedPathSnapshot(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(ResolveOpenedFinalPathWindows(handle))),
            information);
    }

    private static WindowsOpenedPathSnapshot CaptureWindowsFileSnapshot(SafeFileHandle handle)
    {
        var information = GetWindowsFileInformation(handle);
        return new WindowsOpenedPathSnapshot(
            Path.GetFullPath(ResolveOpenedFinalPathWindows(handle)),
            information);
    }

    private static string ResolveOpenedFinalPathWindows(SafeFileHandle handle)
    {
        if (handle.IsClosed || handle.IsInvalid)
            throw Boundary();
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
                throw Rejected("Checkpoint proof source material final path is unavailable.");
            if (length < builder.Capacity)
            {
                const string dosPrefix = @"\\?\";
                const string uncPrefix = @"\\?\UNC\";
                var path = builder.ToString();
                if (path.StartsWith(uncPrefix, StringComparison.OrdinalIgnoreCase))
                    return @"\\" + path[uncPrefix.Length..];
                if (path.StartsWith(dosPrefix, StringComparison.OrdinalIgnoreCase))
                    return path[dosPrefix.Length..];
                throw Boundary();
            }
            capacity = checked((int)length + 1);
        }
        throw Boundary();
    }

    private static void EnsureSingleName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Contains('/', StringComparison.Ordinal)
            || value.Contains('\\', StringComparison.Ordinal)
            || value.Contains('\0', StringComparison.Ordinal)
            || string.Equals(value, ".", StringComparison.Ordinal)
            || string.Equals(value, "..", StringComparison.Ordinal))
        {
            throw Boundary();
        }
    }

    private static int ToPosixFileDescriptor(SafeFileHandle handle)
    {
        if (handle.IsInvalid || handle.IsClosed)
            throw Boundary();
        return checked((int)handle.DangerousGetHandle().ToInt64());
    }

    private static void EnsurePosixLinuxX64StatLayout()
    {
        if (!OperatingSystem.IsLinux() ||
            RuntimeInformation.ProcessArchitecture != Architecture.X64)
        {
            throw Rejected("Checkpoint proof source-material custody requires the verified Linux x64 stat layout.");
        }
    }

    private sealed record OpenedBundle(
        ProofBundlePlatform Platform,
        string RequestedRoot,
        SafeFileHandle RootHandle)
        : IDisposable
    {
        public void Dispose()
            => RootHandle.Dispose();
    }

    private sealed record ProofBundleFile(string Path, byte[] Bytes);

    private sealed class ProofBundleReadException(string message, string errorCode)
        : Exception(message)
    {
        internal string ErrorCode { get; } = errorCode;
    }

    private readonly record struct PosixStatView(
        ulong Device,
        ulong Inode,
        uint Mode,
        long Size);

    private readonly record struct WindowsOpenedPathSnapshot(
        string FinalPath,
        WindowsByHandleFileInformation Information);

    private enum ProofBundlePlatform
    {
        Linux,
        Windows
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PosixStat
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
    private struct WindowsByHandleFileInformation
    {
        public uint FileAttributes;
        public WindowsFileTime CreationTime;
        public WindowsFileTime LastAccessTime;
        public WindowsFileTime LastWriteTime;
        public uint VolumeSerialNumber;
        public uint FileSizeHigh;
        public uint FileSizeLow;
        public uint NumberOfLinks;
        public uint FileIndexHigh;
        public uint FileIndexLow;
    }

    private struct WindowsFileTime
    {
        public uint LowDateTime;
        public uint HighDateTime;
    }

    [DllImport("libc", EntryPoint = "open", SetLastError = true)]
    private static extern int OpenPosix(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags,
        uint mode);

    [DllImport("libc", EntryPoint = "openat", SetLastError = true)]
    private static extern int OpenAtPosix(
        int directoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags,
        uint mode);

    [DllImport("libc", EntryPoint = "fstat", SetLastError = true)]
    private static extern int FStatPosix(
        int fileDescriptor,
        out PosixStat stat);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern SafeFileHandle CreateFileW(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetFileInformationByHandle(
        SafeFileHandle file,
        out WindowsByHandleFileInformation fileInformation);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern uint GetFinalPathNameByHandleW(
        SafeFileHandle file,
        [Out] StringBuilder path,
        uint pathCapacity,
        uint flags);
}
