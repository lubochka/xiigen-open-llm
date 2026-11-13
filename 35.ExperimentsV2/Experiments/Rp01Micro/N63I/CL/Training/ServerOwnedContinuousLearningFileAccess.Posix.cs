// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

internal sealed partial class ServerOwnedContinuousLearningFileAccess
{
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
    private const uint PosixPermissionBitsMask = 0x1FF;
    private const uint PosixGroupOrOtherWriteMask = 0x12;
    private const int PosixEntryNotFound = 2;
    private const int PosixIsDirectory = 21;
    private const int PosixTooManySymbolicLinks = 40;
    private const int PosixWouldBlock = 11;
    private const int PosixFlockExclusive = 0x02;
    private const int PosixFlockNonBlocking = 0x04;
    private const int PosixNameMaxUtf8Bytes = 255;

    private static ServerOwnedContinuousLearningFileAccess AcquirePosix(
        string baseCheckpoint,
        string tokenizer,
        string adapterRoot,
        string cacheRoot,
        string registryRoot,
        string dpoPairRoot,
        ContinuousLearningRetainedObjectRole dpoObjectRole, // Preserve the producer's distinction between a mutable store and sealed input.
        ServerOwnedContinuousLearningFileAccessOptions options,
        Action onRetainedObjectTrustLost)
    {
        EnsurePosixLinuxX64StatLayout();
        var entries = new List<RetainedObjectEntry>();
        var retained = new Dictionary<string, AcquiredPath>(StringComparer.Ordinal);
        var terminalIdentities = new List<RetainedFileIdentity>(6);
        try
        {
            terminalIdentities.Add(AcquireTerminalPosix(
                baseCheckpoint,
                ContinuousLearningRetainedObjectRole.BaseCheckpoint,
                requireDirectory: false,
                maximumFileBytes: ContinuousLearningModelServerConfigurationLoader
                    .MaximumBaseCheckpointBytes,
                entries,
                retained,
                options));
            terminalIdentities.Add(AcquireTerminalPosix(
                tokenizer,
                ContinuousLearningRetainedObjectRole.Tokenizer,
                requireDirectory: false,
                maximumFileBytes: ContinuousLearningModelServerConfigurationLoader
                    .MaximumTokenizerBytes,
                entries,
                retained,
                options));
            terminalIdentities.Add(AcquireTerminalPosix(
                adapterRoot,
                ContinuousLearningRetainedObjectRole.AdapterRoot,
                requireDirectory: true,
                maximumFileBytes: null,
                entries,
                retained,
                options));
            terminalIdentities.Add(AcquireTerminalPosix(
                cacheRoot,
                ContinuousLearningRetainedObjectRole.CacheRoot,
                requireDirectory: true,
                maximumFileBytes: null,
                entries,
                retained,
                options));
            terminalIdentities.Add(AcquireTerminalPosix(
                registryRoot,
                ContinuousLearningRetainedObjectRole.RegistryRoot,
                requireDirectory: true,
                maximumFileBytes: null,
                entries,
                retained,
                options));
            terminalIdentities.Add(AcquireTerminalPosix(
                dpoPairRoot,
                dpoObjectRole, // Sealed input retains no-follow identity without taking a mutable-store flock.
                requireDirectory: true,
                maximumFileBytes: null,
                entries,
                retained,
                options));

            if (terminalIdentities.Distinct().Count() != terminalIdentities.Count)
                throw new ContinuousLearningPathIdentityException();

            return new ServerOwnedContinuousLearningFileAccess(
                entries.ToArray(),
                onRetainedObjectTrustLost);
        }
        catch
        {
            for (var index = entries.Count - 1; index >= 0; index--)
                entries[index].Handle.Dispose();
            throw;
        }
    }

    private static RetainedFileIdentity AcquireTerminalPosix(
        string terminalPath,
        ContinuousLearningRetainedObjectRole terminalRole,
        bool requireDirectory,
        long? maximumFileBytes,
        List<RetainedObjectEntry> entries,
        Dictionary<string, AcquiredPath> retained,
        ServerOwnedContinuousLearningFileAccessOptions options)
    {
        var normalizedTerminal = NormalizeExpectedPosixPath(terminalPath);
        var segments = normalizedTerminal
            .TrimStart('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            // POSIX root не может быть принятым артефактом или mutation root:
            // у него нет parent dirfd и имени leaf, которое потом можно
            // независимо переоткрыть с запретом follow.
            throw new ContinuousLearningPathIdentityException();
        }

        if (!retained.TryGetValue("/", out var parent))
        {
            var rootHandle = OpenPosixRoot();
            try
            {
                var rootIdentity = ValidatePosixHandle(
                    rootHandle,
                    requireDirectory: true,
                    requireOwnerWriteBoundary: false);
                var rootEntry = new RetainedObjectEntry(
                    ContinuousLearningRetainedObjectRole.VolumeOrShareAnchor,
                    rootHandle,
                    rootIdentity,
                    ContinuousLearningRetainedObjectKind.Directory,
                    MinimumBytes: 0,
                    MaximumBytes: 0,
                    RequiredLinkCount: 0,
                    ParentHandleForIndependentOpen: null,
                    LeafNameUtf8ForIndependentOpen: null);
                parent = new AcquiredPath("/", rootEntry);
                retained.Add("/", parent);
                entries.Add(rootEntry);
            }
            catch
            {
                rootHandle.Dispose();
                throw;
            }
        }

        for (var index = 0; index < segments.Length; index++)
        {
            var segment = segments[index];
            EnsurePosixSingleName(segment);
            var isTerminal = index == segments.Length - 1;
            var childRequiresDirectory = !isTerminal || requireDirectory;
            var childPath = CombinePosixPath(parent.ExpectedPath, segment);
            if (retained.TryGetValue(childPath, out var existing))
            {
                if ((existing.Entry.Kind == ContinuousLearningRetainedObjectKind.Directory)
                    != childRequiresDirectory)
                    throw new ContinuousLearningPathIdentityException();
                parent = existing;
                continue;
            }

            if (!TryValidateRetainedEntryPosix(parent.Entry))
                throw new ContinuousLearningPathIdentityException();
            options.AfterParentRetainedBeforeChildOpenForTesting?.Invoke(
                parent.ExpectedPath,
                childPath);
            if (!TryValidateRetainedEntryPosix(parent.Entry))
                throw new ContinuousLearningPathIdentityException();

            var childHandle = OpenPosixChild(
                parent.Entry.Handle,
                segment,
                childRequiresDirectory,
                out _);
            if (childHandle.IsInvalid)
            {
                childHandle.Dispose();
                throw new ContinuousLearningPathIdentityException();
            }

            try
            {
                var childIdentity = ValidatePosixHandle(
                    childHandle,
                    childRequiresDirectory,
                    requireOwnerWriteBoundary: isTerminal);
                if (isTerminal &&
                    childRequiresDirectory &&
                    IsMutableMutationRoot(terminalRole))
                {
                    // Windows удерживает mutation root обязательными share
                    // modes; Linux не имеет этой mandatory-модели, поэтому
                    // POSIX backend держит реальный advisory flock на самом
                    // directory fd и всё равно сверяет identity при каждой
                    // операции. Это не строковый путь и не silent allow.
                    AcquirePosixDirectoryMutationLease(childHandle);
                }

                if (isTerminal && !childRequiresDirectory)
                {
                    if (!TryValidatePosixFileConstraints(
                            GetPosixStat(childHandle),
                            minimumBytes: 1,
                            maximumBytes: maximumFileBytes!.Value,
                            requiredLinkCount: 1,
                            requireOwnerWriteBoundary: true))
                    {
                        throw new ContinuousLearningPathIdentityException();
                    }
                }

                var childEntry = new RetainedObjectEntry(
                    isTerminal
                        ? terminalRole
                        : ContinuousLearningRetainedObjectRole.LexicalAncestor,
                    childHandle,
                    childIdentity,
                    childRequiresDirectory
                        ? ContinuousLearningRetainedObjectKind.Directory
                        : ContinuousLearningRetainedObjectKind.RegularFile,
                    MinimumBytes: childRequiresDirectory ? 0 : 1,
                    MaximumBytes: childRequiresDirectory
                        ? 0
                        : maximumFileBytes!.Value,
                    RequiredLinkCount: childRequiresDirectory ? 0U : 1U,
                    ParentHandleForIndependentOpen: parent.Entry.Handle,
                    LeafNameUtf8ForIndependentOpen: Encoding.UTF8.GetBytes(
                        segment));
                var child = new AcquiredPath(childPath, childEntry);
                retained.Add(childPath, child);
                entries.Add(childEntry);
                parent = child;
            }
            catch
            {
                childHandle.Dispose();
                throw;
            }
        }

        if ((parent.Entry.Kind == ContinuousLearningRetainedObjectKind.Directory)
            != requireDirectory)
            throw new ContinuousLearningPathIdentityException();
        return parent.Entry.Identity;
    }

    private static SafeFileHandle OpenRetainedArtifactForReadPosix(
        RetainedObjectEntry entry)
    {
        var parentHandle = entry.ParentHandleForIndependentOpen
            ?? throw new ContinuousLearningPathIdentityException();
        var leafName = DecodeRetainedPosixLeafName(entry);
        var reopened = OpenPosixChild(
            parentHandle,
            leafName,
            requireDirectory: false,
            out _);
        if (reopened.IsInvalid)
        {
            reopened.Dispose();
            throw new ContinuousLearningPathIdentityException();
        }

        try
        {
            // Повторное чтение артефакта на Linux открывает новое file
            // description через удержанный parent dirfd. В отличие от dup(),
            // это даёт независимый file pointer и не обращается к исходной
            // полной строке пути.
            var identity = ValidatePosixHandle(
                reopened,
                requireDirectory: false,
                requireOwnerWriteBoundary: true);
            if (identity != entry.Identity ||
                !TryValidatePosixFileConstraints(
                    GetPosixStat(reopened),
                    entry.MinimumBytes,
                    entry.MaximumBytes,
                    entry.RequiredLinkCount,
                    requireOwnerWriteBoundary: true))
            {
                throw new ContinuousLearningPathIdentityException();
            }

            return reopened;
        }
        catch
        {
            reopened.Dispose();
            throw;
        }
    }

    private static SafeFileHandle? TryOpenExistingRegularLeafPosix(
        SafeFileHandle rootHandle,
        string leafName)
    {
        EnsurePosixSingleName(leafName);
        var descriptor = OpenAtPosix(
            ToPosixFileDescriptor(rootHandle),
            leafName,
            PosixRegularReadOpenFlags,
            0);
        if (descriptor < 0)
        {
            // Для public leaf-admission missing/symlink/directory означает
            // "leaf не принят", а не потерю root trust. Root revalidation ниже
            // всё равно делает mutation race sticky-lost.
            var error = Marshal.GetLastPInvokeError();
            if (error is PosixEntryNotFound
                or PosixIsDirectory
                or PosixTooManySymbolicLinks)
            {
                return null;
            }
            return null;
        }

        return new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
    }

    private static bool TryValidateRegularLeafPosix(
        SafeFileHandle handle,
        long maximumBytes)
    {
        try
        {
            var stat = GetPosixStat(handle);
            if ((stat.Mode & PosixFileTypeMask) != PosixRegularFileType)
                return false;
            var identity = GetPosixIdentity(stat);
            return identity.VolumeSerialNumber != 0 &&
                   identity.FileIdLow != 0 &&
                   TryValidatePosixFileConstraints(
                       stat,
                       minimumBytes: 1,
                       maximumBytes,
                       requiredLinkCount: 1,
                       requireOwnerWriteBoundary: true);
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool TryValidateRetainedEntryPosix(RetainedObjectEntry entry)
    {
        try
        {
            var stat = GetPosixStat(entry.Handle);
            if (!TryValidatePosixEntryShape(entry, stat))
                return false;
            if (GetPosixIdentity(stat) != entry.Identity)
                return false;

            if (entry.ParentHandleForIndependentOpen is null ||
                entry.LeafNameUtf8ForIndependentOpen is null)
            {
                return entry.Role ==
                       ContinuousLearningRetainedObjectRole.VolumeOrShareAnchor;
            }

            using var reopened = OpenPosixChild(
                entry.ParentHandleForIndependentOpen,
                DecodeRetainedPosixLeafName(entry),
                entry.Kind == ContinuousLearningRetainedObjectKind.Directory,
                out var error);
            if (reopened.IsInvalid)
            {
                _ = error;
                return false;
            }

            var reopenedStat = GetPosixStat(reopened);
            return TryValidatePosixEntryShape(entry, reopenedStat) &&
                   GetPosixIdentity(reopenedStat) == entry.Identity;
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool TryValidatePosixEntryShape(
        RetainedObjectEntry entry,
        PosixStatView stat)
    {
        var isDirectory = (stat.Mode & PosixFileTypeMask) ==
                          PosixDirectoryFileType;
        var isRegular = (stat.Mode & PosixFileTypeMask) ==
                        PosixRegularFileType;
        if ((entry.Kind == ContinuousLearningRetainedObjectKind.Directory) !=
            isDirectory ||
            (entry.Kind == ContinuousLearningRetainedObjectKind.RegularFile) !=
            isRegular)
        {
            return false;
        }

        if (entry.Kind == ContinuousLearningRetainedObjectKind.RegularFile)
        {
            return TryValidatePosixFileConstraints(
                stat,
                entry.MinimumBytes,
                entry.MaximumBytes,
                entry.RequiredLinkCount,
                requireOwnerWriteBoundary: true);
        }

        var requiresTerminalRootTrust =
            entry.Role is ContinuousLearningRetainedObjectRole.AdapterRoot
                or ContinuousLearningRetainedObjectRole.CacheRoot
                or ContinuousLearningRetainedObjectRole.RegistryRoot
                or ContinuousLearningRetainedObjectRole.DpoPairRoot // Keep the mutable store's owner boundary unchanged.
                or ContinuousLearningRetainedObjectRole.SealedRepositoryDpoSourceRoot; // Revalidate the sealed input's owner boundary on every retained check as at admission.
        return entry.MinimumBytes == 0 &&
               entry.MaximumBytes == 0 &&
               entry.RequiredLinkCount == 0 &&
               (!requiresTerminalRootTrust ||
                IsPosixOwnerWriteBoundaryTrusted(stat));
    }

    private static bool TryValidatePosixFileConstraints(
        PosixStatView stat,
        long minimumBytes,
        long maximumBytes,
        uint requiredLinkCount,
        bool requireOwnerWriteBoundary)
    {
        if ((stat.Mode & PosixFileTypeMask) != PosixRegularFileType ||
            stat.Size < minimumBytes ||
            stat.Size > maximumBytes ||
            stat.LinkCount != requiredLinkCount)
        {
            return false;
        }

        return !requireOwnerWriteBoundary ||
               IsPosixOwnerWriteBoundaryTrusted(stat);
    }

    private static RetainedFileIdentity ValidatePosixHandle(
        SafeFileHandle handle,
        bool requireDirectory,
        bool requireOwnerWriteBoundary)
    {
        var stat = GetPosixStat(handle);
        var fileType = stat.Mode & PosixFileTypeMask;
        if (requireDirectory
                ? fileType != PosixDirectoryFileType
                : fileType != PosixRegularFileType)
        {
            throw new ContinuousLearningPathIdentityException();
        }

        if (requireOwnerWriteBoundary &&
            !IsPosixOwnerWriteBoundaryTrusted(stat))
        {
            throw new ContinuousLearningPathIdentityException();
        }

        return GetPosixIdentity(stat);
    }

    private static bool IsPosixOwnerWriteBoundaryTrusted(PosixStatView stat)
    {
        // Linux custody здесь доказывает не "путь выглядит нормально", а что
        // удержанный inode принадлежит текущему uid и не допускает запись от
        // group/other. Read/execute bits для моделей не запрещаются: они не
        // дают третьей стороне право заменить bytes под доверенным inode.
        var permissionBits = stat.Mode & PosixPermissionBitsMask;
        return stat.Uid == GetEffectiveUserIdPosix() &&
               (permissionBits & PosixGroupOrOtherWriteMask) == 0;
    }

    private static RetainedFileIdentity GetPosixIdentity(PosixStatView stat)
    {
        if (stat.Device == 0 || stat.Inode == 0)
            throw new ContinuousLearningPathIdentityException();
        return new RetainedFileIdentity(
            ServerOwnedContinuousLearningFileAccessPlatform.Posix,
            stat.Device,
            stat.Inode,
            0UL);
    }

    private static PosixStatView GetPosixStat(SafeFileHandle handle)
    {
        EnsurePosixLinuxX64StatLayout();
        if (handle.IsInvalid || handle.IsClosed)
            throw new ContinuousLearningPathIdentityException();
        if (FStatPosix(ToPosixFileDescriptor(handle), out var stat) != 0)
            throw new ContinuousLearningPathIdentityException();
        return new PosixStatView(
            stat.Device,
            stat.Inode,
            stat.LinkCount,
            stat.Mode,
            stat.Uid,
            stat.Size);
    }

    private static SafeFileHandle OpenPosixRoot()
    {
        var descriptor = OpenPosix(
            "/",
            PosixDirectoryOpenFlags,
            0);
        if (descriptor < 0)
            throw new ContinuousLearningPathIdentityException();
        return new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
    }

    private static SafeFileHandle OpenPosixChild(
        SafeFileHandle parentHandle,
        string leafName,
        bool requireDirectory,
        out int error)
    {
        EnsurePosixSingleName(leafName);
        var flags = requireDirectory
            ? PosixDirectoryOpenFlags
            : PosixRegularReadOpenFlags;
        var descriptor = OpenAtPosix(
            ToPosixFileDescriptor(parentHandle),
            leafName,
            flags,
            0);
        error = descriptor < 0 ? Marshal.GetLastPInvokeError() : 0;
        return new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
    }

    private static void AcquirePosixDirectoryMutationLease(SafeFileHandle handle)
    {
        if (FlockPosix(
                ToPosixFileDescriptor(handle),
                PosixFlockExclusive | PosixFlockNonBlocking) == 0)
        {
            return;
        }

        var error = Marshal.GetLastPInvokeError();
        if (error == PosixWouldBlock)
            throw new ContinuousLearningMutationAdmissionDomainAlreadyOwnedException();
        throw new ContinuousLearningPathIdentityException();
    }

    private static string DecodeRetainedPosixLeafName(RetainedObjectEntry entry)
    {
        if (entry.LeafNameUtf8ForIndependentOpen is null)
            throw new ContinuousLearningPathIdentityException();
        var leafName = Encoding.UTF8.GetString(
            entry.LeafNameUtf8ForIndependentOpen);
        EnsurePosixSingleName(leafName);
        return leafName;
    }

    private static string NormalizeExpectedPosixPath(string path)
    {
        var fullPath = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(path));
        if (!Path.IsPathFullyQualified(fullPath) ||
            !fullPath.StartsWith("/", StringComparison.Ordinal))
        {
            throw new ContinuousLearningPathIdentityException();
        }

        return fullPath.Length == 0 ? "/" : fullPath;
    }

    private static string CombinePosixPath(string parent, string child)
        => parent == "/"
            ? "/" + child
            : parent + "/" + child;

    private static void EnsurePosixSingleName(string value)
    {
        EnsureSingleName(value);
        // Linux ограничивает имя байтами UTF-8. Проверяем это до syscall,
        // чтобы слишком длинное имя не превращалось в платформенно-разный
        // native errno.
        if (Encoding.UTF8.GetByteCount(value) > PosixNameMaxUtf8Bytes)
            throw new ContinuousLearningPathIdentityException();
    }

    private static int ToPosixFileDescriptor(SafeFileHandle handle)
    {
        if (handle.IsInvalid || handle.IsClosed)
            throw new ContinuousLearningPathIdentityException();
        return checked((int)handle.DangerousGetHandle().ToInt64());
    }

    private static void EnsurePosixLinuxX64StatLayout()
    {
        if (!OperatingSystem.IsLinux() ||
            RuntimeInformation.ProcessArchitecture != Architecture.X64)
        {
            // Непроверенный native layout не получает диагностическую поблажку:
            // для caller это тот же stable identity failure без раскрытия
            // локальных platform details.
            throw new ContinuousLearningPathIdentityException();
        }
    }

    private readonly record struct PosixStatView(
        ulong Device,
        ulong Inode,
        ulong LinkCount,
        uint Mode,
        uint Uid,
        long Size);

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

    [DllImport("libc", EntryPoint = "flock", SetLastError = true)]
    private static extern int FlockPosix(
        int fileDescriptor,
        int operation);

    [DllImport("libc", EntryPoint = "geteuid")]
    private static extern uint GetEffectiveUserIdPosix();
}
