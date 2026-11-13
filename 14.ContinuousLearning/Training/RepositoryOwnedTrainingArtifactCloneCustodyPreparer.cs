// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Microsoft.Win32.SafeHandles;

/// <summary>
/// Describes what was done before strict repository-owned readers touched a
/// Git-cloned artifact tree.
/// </summary>
public sealed record RepositoryOwnedTrainingArtifactCloneCustodyPreparation(
    string RepositoryRoot,
    RepositoryOwnedTrainingArtifactCloneCustodyPreparationSource Source,
    int DirectoriesPrepared,
    int FilesPrepared,
    IReadOnlyList<string> PreparedRepositoryRelativeRoots);

/// <summary>
/// Names the platform branch used by the clone-custody preparer.
/// </summary>
public enum RepositoryOwnedTrainingArtifactCloneCustodyPreparationSource
{
    MissingRepositoryOwnedRoots,
    LinuxCloneLikeModesNarrowed,
    LinuxAlreadyOwnerOnly,
    WindowsAclBoundaryPreservedNoOp,
}

internal sealed record
    RepositoryOwnedTrainingArtifactCloneCustodyRootPreparationObservation(
        string RepositoryRoot,
        string RepositoryRelativeRoot);

/// <summary>
/// Re-establishes local custody bits that Git and Git LFS intentionally do not
/// transport for repository-owned training artifacts.
/// </summary>
public static class RepositoryOwnedTrainingArtifactCloneCustodyPreparer
{
    public const string InvalidErrorCode =
        "REPOSITORY_OWNED_TRAINING_ARTIFACT_CLONE_CUSTODY_INVALID";
    public const string UnsafeErrorCode =
        "REPOSITORY_OWNED_TRAINING_ARTIFACT_CLONE_CUSTODY_UNSAFE";
    public const string UnavailableErrorCode =
        "REPOSITORY_OWNED_TRAINING_ARTIFACT_CLONE_CUSTODY_UNAVAILABLE";

    private const int LinuxOpenReadOnly = 0;
    private const int LinuxOpenDirectory = 0x10000;
    private const int LinuxOpenNoFollow = 0x20000;
    private const int LinuxOpenCloseOnExec = 0x80000;
    private const int LinuxErrnoNoEntry = 2;
    private const int LinuxErrnoNotDirectory = 20;
    private const int LinuxErrnoLoop = 40;
    private static readonly UnixFileMode OwnerDirectoryMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
    private static readonly UnixFileMode OwnerFileMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite;
    private static readonly UnixFileMode GroupOrOtherWriteMask =
        UnixFileMode.GroupWrite | UnixFileMode.OtherWrite;
    private static readonly UnixFileMode ExecuteMask =
        UnixFileMode.UserExecute |
        UnixFileMode.GroupExecute |
        UnixFileMode.OtherExecute;
    private static readonly UnixFileMode SpecialUnixModeBits =
        UnixFileMode.SetUser | UnixFileMode.SetGroup | UnixFileMode.StickyBit;
    private static readonly AsyncLocal<Action<
            RepositoryOwnedTrainingArtifactCloneCustodyRootPreparationObservation>?>
        RootPreparationObserver = new();

    internal static Action<
        RepositoryOwnedTrainingArtifactCloneCustodyRootPreparationObservation>?
        RootPreparationObservedForTesting
    {
        get => RootPreparationObserver.Value;
        set => RootPreparationObserver.Value = value;
    }

    public static OperationResult<
            RepositoryOwnedTrainingArtifactCloneCustodyPreparation>
        PrepareForStrictReaders(string repositoryRoot)
    {
        var context = PreparePublicContextForStrictReaders(repositoryRoot);
        return context.IsSuccess && context.Value is not null
            ? OperationResult<
                RepositoryOwnedTrainingArtifactCloneCustodyPreparation>.Ok(
                context.Value.Preparation)
            : context.MapError<
                RepositoryOwnedTrainingArtifactCloneCustodyPreparation>();
    }

    internal static OperationResult<PreparedContext>
        PreparePublicContextForStrictReaders(string repositoryRoot)
        => PrepareContextForStrictReaders(
            repositoryRoot,
            includeDpoRoot: true,
            includeModelRoot: true,
            includeReferenceCorpusRoot: true);

    internal static OperationResult<PreparedContext>
        PrepareDpoContextForStrictReaders(string repositoryRoot)
        => PrepareContextForStrictReaders(
            repositoryRoot,
            includeDpoRoot: true,
            includeModelRoot: false,
            includeReferenceCorpusRoot: false);

    internal static OperationResult<PreparedContext>
        PrepareModelAndDpoContextForStrictReaders(string repositoryRoot)
        => PrepareContextForStrictReaders(
            repositoryRoot,
            includeDpoRoot: true,
            includeModelRoot: true,
            includeReferenceCorpusRoot: false);

    private static OperationResult<PreparedContext> PrepareContextForStrictReaders(
        string repositoryRoot,
        bool includeDpoRoot,
        bool includeModelRoot,
        bool includeReferenceCorpusRoot)
        => PreparedContext.Prepare(
            repositoryRoot,
            includeDpoRoot,
            includeModelRoot,
            includeReferenceCorpusRoot);

    internal sealed class PreparedContext
    {
        private readonly string repositoryRoot;
        private readonly bool includesDpoRoot;
        private readonly bool includesModelRoot;
        private readonly bool includesReferenceCorpusRoot;

        internal static OperationResult<PreparedContext> Prepare(
            string repositoryRoot,
            bool includeDpoRoot,
            bool includeModelRoot,
            bool includeReferenceCorpusRoot)
        {
            if (!includeDpoRoot && !includeModelRoot
                && !includeReferenceCorpusRoot)
            {
                return Invalid<PreparedContext>();
            }

            var selectedRoots = new List<CustodyRootPlan>(capacity: 3);
            string? canonicalRepositoryRoot = null;
            if (includeDpoRoot)
            {
                var dpoRoot = RepositoryOwnedTrainingArtifactLayout
                    .ResolveDpoArtifactRoot(repositoryRoot);
                if (!dpoRoot.IsSuccess || dpoRoot.Value is null)
                    return Invalid<PreparedContext>();
                canonicalRepositoryRoot = dpoRoot.Value.RepositoryRoot;
                selectedRoots.Add(new CustodyRootPlan(
                    dpoRoot.Value.DpoArtifactRoot,
                    dpoRoot.Value.DpoArtifactRootRelativePath));
            }

            if (includeModelRoot)
            {
                var modelRoot = RepositoryOwnedTrainingArtifactLayout
                    .ResolveModelArtifactRoot(repositoryRoot);
                if (!modelRoot.IsSuccess || modelRoot.Value is null)
                    return Invalid<PreparedContext>();
                canonicalRepositoryRoot ??= modelRoot.Value.RepositoryRoot;
                if (!PathsEqual(
                        canonicalRepositoryRoot,
                        modelRoot.Value.RepositoryRoot))
                {
                    return Invalid<PreparedContext>();
                }

                selectedRoots.Add(new CustodyRootPlan(
                    modelRoot.Value.ModelArtifactRoot,
                    modelRoot.Value.ModelArtifactRootRelativePath));
            }

            if (includeReferenceCorpusRoot)
            {
                var referenceCorpusRoot = RepositoryOwnedTrainingArtifactLayout
                    .ResolveReferenceCorpusArtifactRoot(repositoryRoot);
                if (!referenceCorpusRoot.IsSuccess
                    || referenceCorpusRoot.Value is null)
                {
                    return Invalid<PreparedContext>();
                }

                canonicalRepositoryRoot ??=
                    referenceCorpusRoot.Value.RepositoryRoot;
                if (!PathsEqual(
                        canonicalRepositoryRoot,
                        referenceCorpusRoot.Value.RepositoryRoot))
                {
                    return Invalid<PreparedContext>();
                }

                selectedRoots.Add(new CustodyRootPlan(
                    referenceCorpusRoot.Value.ReferenceCorpusArtifactRoot,
                    referenceCorpusRoot.Value
                        .ReferenceCorpusArtifactRootRelativePath));
            }

            if (string.IsNullOrWhiteSpace(canonicalRepositoryRoot))
                return Invalid<PreparedContext>();

            if (OperatingSystem.IsWindows())
            {
                // WHY: Windows custody in this repository is an ACL contract,
                // not a Unix chmod claim. Clone preparation therefore validates
                // the canonical roots above and lets strict Windows readers
                // enforce their existing ACL boundary when they open the files.
                return OperationResult<PreparedContext>.Ok(
                    new PreparedContext(
                        canonicalRepositoryRoot,
                        includeDpoRoot,
                        includeModelRoot,
                        includeReferenceCorpusRoot,
                        new RepositoryOwnedTrainingArtifactCloneCustodyPreparation(
                            canonicalRepositoryRoot,
                            RepositoryOwnedTrainingArtifactCloneCustodyPreparationSource
                                .WindowsAclBoundaryPreservedNoOp,
                            0,
                            0,
                            [])));
            }

            if (!OperatingSystem.IsLinux())
                return Unavailable<PreparedContext>();
            if (RuntimeInformation.ProcessArchitecture != Architecture.X64)
                return Unavailable<PreparedContext>();

            try
            {
                var preparedRoots = new List<string>(capacity: 2);
                var directoriesPrepared = 0;
                var filesPrepared = 0;
                var rootsSeen = 0;
                foreach (var root in selectedRoots)
                {
                    RootPreparationObservedForTesting?.Invoke(
                        new RepositoryOwnedTrainingArtifactCloneCustodyRootPreparationObservation(
                            canonicalRepositoryRoot,
                            root.RelativePath));
                    if (!Directory.Exists(root.FullPath))
                        continue;

                    rootsSeen++;
                    var prepared = PrepareLinuxExistingRoot(root.FullPath);
                    if (!prepared.IsSuccess)
                        return prepared.MapError<PreparedContext>();

                    if (prepared.Value.DirectoriesPrepared > 0 ||
                        prepared.Value.FilesPrepared > 0)
                    {
                        preparedRoots.Add(root.RelativePath);
                        directoriesPrepared += prepared.Value.DirectoriesPrepared;
                        filesPrepared += prepared.Value.FilesPrepared;
                    }
                }

                var source = rootsSeen == 0
                    ? RepositoryOwnedTrainingArtifactCloneCustodyPreparationSource
                        .MissingRepositoryOwnedRoots
                    : directoriesPrepared == 0 && filesPrepared == 0
                        ? RepositoryOwnedTrainingArtifactCloneCustodyPreparationSource
                            .LinuxAlreadyOwnerOnly
                        : RepositoryOwnedTrainingArtifactCloneCustodyPreparationSource
                            .LinuxCloneLikeModesNarrowed;

                // WHY: PreparedContext is intentionally not constructible
                // through an exposed factory. The only valid instance is born
                // here, after root selection, no-follow traversal and any
                // required chmod have already succeeded for this stack call.
                return OperationResult<PreparedContext>.Ok(
                    new PreparedContext(
                        canonicalRepositoryRoot,
                        includeDpoRoot,
                        includeModelRoot,
                        includeReferenceCorpusRoot,
                        new RepositoryOwnedTrainingArtifactCloneCustodyPreparation(
                            canonicalRepositoryRoot,
                            source,
                            directoriesPrepared,
                            filesPrepared,
                            preparedRoots)));
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return Unavailable<PreparedContext>();
            }
        }

        private PreparedContext(
            string repositoryRoot,
            bool includesDpoRoot,
            bool includesModelRoot,
            bool includesReferenceCorpusRoot,
            RepositoryOwnedTrainingArtifactCloneCustodyPreparation preparation)
        {
            this.repositoryRoot = Path.GetFullPath(repositoryRoot);
            this.includesDpoRoot = includesDpoRoot;
            this.includesModelRoot = includesModelRoot;
            this.includesReferenceCorpusRoot = includesReferenceCorpusRoot;
            Preparation = preparation;
        }

        internal RepositoryOwnedTrainingArtifactCloneCustodyPreparation
            Preparation { get; }

        internal bool CoversDpoRoot(string candidateRepositoryRoot)
            => includesDpoRoot &&
               PathsEqual(repositoryRoot, candidateRepositoryRoot);

        internal bool CoversModelAndDpoRoots(string candidateRepositoryRoot)
            => includesDpoRoot &&
               includesModelRoot &&
               PathsEqual(repositoryRoot, candidateRepositoryRoot);

        internal bool CoversReferenceCorpusRoot(string candidateRepositoryRoot)
            => includesReferenceCorpusRoot &&
               PathsEqual(repositoryRoot, candidateRepositoryRoot);
    }

    [SupportedOSPlatform("linux")]
    private static OperationResult<LinuxPreparationCounts>
        PrepareLinuxExistingRoot(string root)
    {
        var handle = OpenLinuxDirectoryNoFollow(root, out var errno);
        if (handle is null)
        {
            return errno == LinuxErrnoNoEntry
                ? OperationResult<LinuxPreparationCounts>.Ok(
                    LinuxPreparationCounts.Empty)
                : Unsafe<LinuxPreparationCounts>();
        }

        using (handle)
        {
            return PrepareLinuxDirectory(handle, root);
        }
    }

    [SupportedOSPlatform("linux")]
    private static OperationResult<LinuxPreparationCounts> PrepareLinuxDirectory(
        SafeFileHandle directoryHandle,
        string directoryPath)
    {
        if (!LinuxDirectoryHandleMatchesPath(directoryHandle, directoryPath))
            return Unsafe<LinuxPreparationCounts>();

        var directoryPrepared =
            EnsureLinuxDirectoryHandleOwnerOnly(directoryHandle, directoryPath);
        if (!directoryPrepared.IsSuccess)
            return directoryPrepared.MapError<LinuxPreparationCounts>();

        var totals = new LinuxPreparationCounts(
            directoryPrepared.Value ? 1 : 0,
            0);
        foreach (var entry in Directory.EnumerateFileSystemEntries(
                     directoryPath,
                     "*",
                     SearchOption.TopDirectoryOnly))
        {
            var name = Path.GetFileName(entry);
            if (!IsSafeImmediateName(name))
                return Unsafe<LinuxPreparationCounts>();

            var childPath = Path.GetFullPath(Path.Combine(directoryPath, name));
            var childDirectory = OpenLinuxDirectoryAtNoFollow(
                directoryHandle,
                name,
                out var directoryErrno);
            if (childDirectory is not null)
            {
                using (childDirectory)
                {
                    if (!LinuxDirectoryHandleMatchesPath(
                            childDirectory,
                            childPath))
                    {
                        return Unsafe<LinuxPreparationCounts>();
                    }

                    var childResult = PrepareLinuxDirectory(
                        childDirectory,
                        childPath);
                    if (!childResult.IsSuccess)
                        return childResult;

                    totals += childResult.Value;
                    continue;
                }
            }

            if (directoryErrno == LinuxErrnoLoop)
                return Unsafe<LinuxPreparationCounts>();

            var fileHandle = OpenLinuxFileAtNoFollow(
                directoryHandle,
                name,
                out var fileErrno);
            if (fileHandle is null)
            {
                return fileErrno is LinuxErrnoLoop ||
                       directoryErrno is not LinuxErrnoNotDirectory
                    ? Unsafe<LinuxPreparationCounts>()
                    : Unavailable<LinuxPreparationCounts>();
            }

            using (fileHandle)
            {
                if (!LinuxFileHandleMatchesPath(fileHandle, childPath))
                    return Unsafe<LinuxPreparationCounts>();

                var filePrepared =
                    EnsureLinuxFileHandleOwnerOnly(fileHandle, childPath);
                if (!filePrepared.IsSuccess)
                    return filePrepared.MapError<LinuxPreparationCounts>();

                totals += new LinuxPreparationCounts(
                    0,
                    filePrepared.Value ? 1 : 0);
            }
        }

        return OperationResult<LinuxPreparationCounts>.Ok(totals);
    }

    [SupportedOSPlatform("linux")]
    private static OperationResult<bool> EnsureLinuxDirectoryHandleOwnerOnly(
        SafeFileHandle handle,
        string requestedPath)
    {
        if (!LinuxDirectoryHandleMatchesPath(handle, requestedPath))
            return Unsafe<bool>();

        var mode = File.GetUnixFileMode(handle);
        if (!DirectoryModeCanBeNarrowedToOwner(mode))
            return Unsafe<bool>();

        if (DirectoryModeIsOwnerOnly(mode))
            return OperationResult<bool>.Ok(false);

        // WHY: a Git clone commonly expands directory permissions to 0755.
        // Narrowing uses the already-opened O_NOFOLLOW descriptor, not a later
        // string chmod, so a namespace swap cannot redirect the permission edit
        // outside the repository-owned subtree.
        File.SetUnixFileMode(handle, OwnerDirectoryMode);
        return LinuxDirectoryHandleMatchesPath(handle, requestedPath) &&
               DirectoryModeIsOwnerOnly(File.GetUnixFileMode(handle))
            ? OperationResult<bool>.Ok(true)
            : Unsafe<bool>();
    }

    [SupportedOSPlatform("linux")]
    private static OperationResult<bool> EnsureLinuxFileHandleOwnerOnly(
        SafeFileHandle handle,
        string requestedPath)
    {
        if (!LinuxFileHandleMatchesPath(handle, requestedPath))
            return Unsafe<bool>();

        // WHY: O_NOFOLLOW closes symlink substitution, but a regular file can
        // still be a hardlink to an inode with another name outside the allowed
        // repository-owned roots. Before chmod changes local custody, fstat the
        // already-opened descriptor and require a single link.
        if (!LinuxRegularFileHandleHasSingleName(handle))
            return Unsafe<bool>();

        var mode = File.GetUnixFileMode(handle);
        if (!FileModeCanBeNarrowedToOwner(mode))
            return Unsafe<bool>();

        if (FileModeIsOwnerOnly(mode))
            return OperationResult<bool>.Ok(false);

        // WHY: Git LFS materializes regular files as ordinary clone files
        // such as 0644. The strict readers still require 0600 custody before
        // they trust manifests or weights, so the chmod is bound to this
        // no-follow file handle and immediately rechecked.
        File.SetUnixFileMode(handle, OwnerFileMode);
        return LinuxFileHandleMatchesPath(handle, requestedPath) &&
               FileModeIsOwnerOnly(File.GetUnixFileMode(handle))
            ? OperationResult<bool>.Ok(true)
            : Unsafe<bool>();
    }

    [SupportedOSPlatform("linux")]
    private static bool LinuxDirectoryHandleMatchesPath(
        SafeFileHandle handle,
        string requestedPath)
    {
        try
        {
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    handle,
                    requestedPath))
            {
                return false;
            }

            var attributes = File.GetAttributes(handle);
            return (attributes & FileAttributes.Directory) != 0 &&
                   (attributes & FileAttributes.ReparsePoint) == 0;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    [SupportedOSPlatform("linux")]
    private static bool LinuxFileHandleMatchesPath(
        SafeFileHandle handle,
        string requestedPath)
    {
        try
        {
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    handle,
                    requestedPath))
            {
                return false;
            }

            var attributes = File.GetAttributes(handle);
            return (attributes & FileAttributes.Directory) == 0 &&
                   (attributes & FileAttributes.ReparsePoint) == 0;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool DirectoryModeCanBeNarrowedToOwner(UnixFileMode mode)
        => (mode & OwnerDirectoryMode) == OwnerDirectoryMode &&
           (mode & GroupOrOtherWriteMask) == 0 &&
           (mode & SpecialUnixModeBits) == 0;

    private static bool DirectoryModeIsOwnerOnly(UnixFileMode mode)
        => DirectoryModeCanBeNarrowedToOwner(mode) &&
           (mode & (UnixFileMode.GroupRead |
                    UnixFileMode.GroupExecute |
                    UnixFileMode.OtherRead |
                    UnixFileMode.OtherExecute)) == 0;

    private static bool FileModeCanBeNarrowedToOwner(UnixFileMode mode)
        => (mode & OwnerFileMode) == OwnerFileMode &&
           (mode & GroupOrOtherWriteMask) == 0 &&
           (mode & ExecuteMask) == 0 &&
           (mode & SpecialUnixModeBits) == 0;

    private static bool FileModeIsOwnerOnly(UnixFileMode mode)
        => FileModeCanBeNarrowedToOwner(mode) &&
           (mode & (UnixFileMode.GroupRead | UnixFileMode.OtherRead)) == 0;

    private static bool PathsEqual(string first, string second)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(first)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(second)),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    [SupportedOSPlatform("linux")]
    private static bool LinuxRegularFileHandleHasSingleName(
        SafeFileHandle handle)
    {
        if (handle.IsInvalid || handle.IsClosed)
            return false;

        if (FStatLinux(ToLinuxFileDescriptor(handle), out var stat) != 0)
            return false;

        const uint fileTypeMask = 0xF000;
        const uint regularFileType = 0x8000;
        return (stat.Mode & fileTypeMask) == regularFileType &&
               stat.LinkCount == 1 &&
               stat.Device != 0 &&
               stat.Inode != 0;
    }

    private static int ToLinuxFileDescriptor(SafeFileHandle handle)
    {
        if (handle.IsInvalid || handle.IsClosed)
            throw new InvalidOperationException(
                "The Linux file handle is not open.");
        return checked((int)handle.DangerousGetHandle().ToInt64());
    }

    private static bool IsSafeImmediateName(string? name)
        => !string.IsNullOrWhiteSpace(name) &&
           !StringComparer.Ordinal.Equals(name, ".") &&
           !StringComparer.Ordinal.Equals(name, "..") &&
           name.IndexOfAny(
               [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar])
           < 0 &&
           !name.Any(char.IsControl);

    [SupportedOSPlatform("linux")]
    private static SafeFileHandle? OpenLinuxDirectoryNoFollow(
        string path,
        out int errno)
    {
        var descriptor = LinuxOpen(
            path,
            LinuxOpenReadOnly |
            LinuxOpenDirectory |
            LinuxOpenNoFollow |
            LinuxOpenCloseOnExec);
        return WrapLinuxDescriptor(descriptor, out errno);
    }

    [SupportedOSPlatform("linux")]
    private static SafeFileHandle? OpenLinuxDirectoryAtNoFollow(
        SafeFileHandle parent,
        string segment,
        out int errno)
    {
        var descriptor = LinuxOpenAt(
            parent,
            segment,
            LinuxOpenReadOnly |
            LinuxOpenDirectory |
            LinuxOpenNoFollow |
            LinuxOpenCloseOnExec,
            0);
        return WrapLinuxDescriptor(descriptor, out errno);
    }

    [SupportedOSPlatform("linux")]
    private static SafeFileHandle? OpenLinuxFileAtNoFollow(
        SafeFileHandle parent,
        string fileName,
        out int errno)
    {
        var descriptor = LinuxOpenAt(
            parent,
            fileName,
            LinuxOpenReadOnly |
            LinuxOpenNoFollow |
            LinuxOpenCloseOnExec,
            0);
        return WrapLinuxDescriptor(descriptor, out errno);
    }

    private static SafeFileHandle? WrapLinuxDescriptor(
        int descriptor,
        out int errno)
    {
        if (descriptor < 0)
        {
            errno = Marshal.GetLastPInvokeError();
            return null;
        }

        errno = 0;
        var handle = new SafeFileHandle(
            new IntPtr(descriptor),
            ownsHandle: true);
        if (!handle.IsInvalid)
            return handle;

        handle.Dispose();
        return null;
    }

    private static OperationResult<T> Invalid<T>()
        => OperationResult<T>.Fail(
            "Repository-owned clone custody preparation request is invalid.",
            InvalidErrorCode);

    private static OperationResult<T> Unsafe<T>()
        => OperationResult<T>.Fail(
            "Repository-owned clone custody preparation found an unsafe artifact shape and failed closed.",
            UnsafeErrorCode);

    private static OperationResult<T> Unavailable<T>()
        => OperationResult<T>.Fail(
            "Repository-owned clone custody preparation is unavailable.",
            UnavailableErrorCode);

    private readonly record struct LinuxPreparationCounts(
        int DirectoriesPrepared,
        int FilesPrepared)
    {
        internal static LinuxPreparationCounts Empty { get; } = new(0, 0);

        public static LinuxPreparationCounts operator +(
            LinuxPreparationCounts left,
            LinuxPreparationCounts right)
            => new(
                left.DirectoriesPrepared + right.DirectoriesPrepared,
                left.FilesPrepared + right.FilesPrepared);
    }

    private readonly record struct CustodyRootPlan(
        string FullPath,
        string RelativePath);

    [DllImport("libc", EntryPoint = "open", SetLastError = true)]
    private static extern int LinuxOpen(string path, int flags);

    [DllImport("libc", EntryPoint = "openat", SetLastError = true)]
    private static extern int LinuxOpenAt(
        SafeFileHandle directory,
        string path,
        int flags,
        uint mode);

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

    [DllImport("libc", EntryPoint = "fstat", SetLastError = true)]
    private static extern int FStatLinux(
        int fileDescriptor,
        out LinuxStat stat);
}
