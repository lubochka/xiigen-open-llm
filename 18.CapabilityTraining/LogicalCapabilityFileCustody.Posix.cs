// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AgentInfrastructure;
using DPO;
using Microsoft.Win32.SafeHandles;

internal static partial class LogicalCapabilityFileCustody
{
    private static FileStream? OpenLinuxIdentityStream(
        string path,
        uint desiredAccess,
        uint creationDisposition)
    {
        try
        {
            EnsureLinuxX64StatLayout();
            var fullPath = Path.GetFullPath(path);
            var parentPath = Path.GetDirectoryName(fullPath);
            var leafName = Path.GetFileName(fullPath);
            if (string.IsNullOrEmpty(parentPath) ||
                !IsSafeLeafName(leafName))
            {
                return null;
            }

            using var parentChain = OpenLinuxDirectoryChain(
                parentPath,
                createMissing: false,
                finalDirectoryMustBeOwned: true);
            var leafHandle = OpenLinuxLeaf(
                parentChain.DirectoryHandle,
                leafName,
                fullPath,
                desiredAccess,
                creationDisposition);
            try
            {
                ValidateLinuxRegularFileHandle(leafHandle, fullPath);
                return new FileStream(leafHandle, FileAccessFor(desiredAccess));
            }
            catch
            {
                leafHandle.Dispose();
                throw;
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return null;
        }
    }

    private static LogicalCapabilityFileShape CaptureLinuxFileShape(
        SafeFileHandle handle,
        string fullPath,
        bool requireRegularFile)
    {
        var stat = GetLinuxStat(handle, fullPath);
        if (requireRegularFile)
        {
            ValidateLinuxRegularStat(stat, fullPath);
            if (stat.LinkCount > uint.MaxValue || stat.LinkCount != 1)
                throw new IOException("LOGICAL_CAPABILITY_FILE_IDENTITY_LINK_COUNT_UNSAFE");
            if (stat.Size < 0)
                throw new IOException("LOGICAL_CAPABILITY_FILE_IDENTITY_LENGTH_INVALID");
        }

        // На Linux физическая личность файла — это пара device/inode. Мы не
        // смешиваем её с Windows FileId, чтобы одинаковые числа на разных ОС
        // не стали ложным совпадением.
        return new LogicalCapabilityFileShape(
            checked((long)stat.Size),
            stat.LinkCount > uint.MaxValue ? uint.MaxValue : (uint)stat.LinkCount,
            new DpoTrainerArtifactIdentity(
                DpoTrainerArtifactIdentityPlatform.Linux,
                stat.Device,
                stat.Inode,
                0UL));
    }

    private static bool LinuxHandleMatchesPath(
        SafeFileHandle handle,
        string canonicalPath)
    {
        SafeFileHandle? reopened = null;
        try
        {
            var fullPath = Path.GetFullPath(canonicalPath);
            var parentPath = Path.GetDirectoryName(fullPath);
            var leafName = Path.GetFileName(fullPath);
            if (string.IsNullOrEmpty(parentPath) ||
                !IsSafeLeafName(leafName))
            {
                return false;
            }

            using var parentChain = OpenLinuxDirectoryChain(
                parentPath,
                createMissing: false,
                finalDirectoryMustBeOwned: true);
            reopened = OpenLinuxLeaf(
                parentChain.DirectoryHandle,
                leafName,
                fullPath,
                GenericReadAccess,
                OpenExistingDisposition);
            ValidateLinuxRegularFileHandle(reopened, fullPath);
            return LinuxHandlesHaveSameIdentity(handle, reopened);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
        finally
        {
            reopened?.Dispose();
        }
    }

    private static bool LinuxHandlesHaveSameIdentity(
        SafeFileHandle left,
        SafeFileHandle right)
    {
        try
        {
            var leftShape = CaptureLinuxFileShape(left, string.Empty, requireRegularFile: false);
            var rightShape = CaptureLinuxFileShape(right, string.Empty, requireRegularFile: false);
            return leftShape.Identity == rightShape.Identity;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static void MarkLinuxIdentityForDeletion(
        FileStream stream,
        string path,
        Action? beforeUnlinkForTesting)
    {
        var fullPath = Path.GetFullPath(path);
        var parentPath = Path.GetDirectoryName(fullPath);
        var leafName = Path.GetFileName(fullPath);
        if (string.IsNullOrEmpty(parentPath) ||
            !IsSafeLeafName(leafName))
        {
            throw new IOException("LOGICAL_CAPABILITY_FILE_DELETE_PATH_UNSAFE");
        }

        using var parentChain = OpenLinuxDirectoryChain(
            parentPath,
            createMissing: false,
            finalDirectoryMustBeOwned: true);
        MarkLinuxIdentityForDeletionAt(
            stream,
            parentChain.DirectoryHandle,
            leafName,
            fullPath,
            parentPath,
            beforeUnlinkForTesting);
    }

    private static void MarkLinuxIdentityForDeletionAt(
        FileStream stream,
        SafeFileHandle parentHandle,
        string leafName,
        string fullPath,
        string parentPath,
        Action? beforeUnlinkForTesting)
    {
        var originalShape = CaptureLinuxFileShape(
            stream.SafeFileHandle,
            fullPath,
            requireRegularFile: true);
        using var reopenedHandle = OpenLinuxLeaf(
            parentHandle,
            leafName,
            fullPath,
            GenericReadAccess,
            OpenExistingDisposition);
        var reopenedShape = CaptureLinuxFileShape(
            reopenedHandle,
            fullPath,
            requireRegularFile: true);
        if (reopenedShape.Identity != originalShape.Identity ||
            reopenedShape.Length != originalShape.Length ||
            reopenedShape.LinkCount != originalShape.LinkCount)
        {
            throw new IOException("LOGICAL_CAPABILITY_FILE_DELETE_IDENTITY_CHANGED");
        }

        // Удаляем только то имя, которое прямо сейчас указывает на удержанный
        // inode. Если кто-то успел подменить путь чужим файлом, сравнение выше
        // остановит удаление и сохранит чужие байты.
        beforeUnlinkForTesting?.Invoke();
        if (UnlinkAtLinux(ToFileDescriptor(parentHandle), leafName, 0) != 0)
        {
            throw LinuxIOException(
                "unlink logical-capability owned file",
                fullPath,
                Marshal.GetLastPInvokeError());
        }

        var retainedAfterUnlink = CaptureLinuxFileShape(
            stream.SafeFileHandle,
            fullPath,
            requireRegularFile: false);
        if (retainedAfterUnlink.Identity != originalShape.Identity ||
            retainedAfterUnlink.LinkCount != 0)
        {
            throw new IOException("LOGICAL_CAPABILITY_FILE_DELETE_HANDLE_CHANGED_OR_LINKED");
        }

        FlushLinuxDirectory(parentHandle, parentPath);
    }

    private static bool TryRenameLinuxIdentityNoReplace(
        FileStream stream,
        DpoTrainerArtifactOwnershipProof proof,
        string sourcePath,
        string destinationPath,
        FileShare share)
    {
        var sourceFullPath = Path.GetFullPath(sourcePath);
        var destinationFullPath = Path.GetFullPath(destinationPath);
        var sourceParentPath = Path.GetDirectoryName(sourceFullPath);
        var destinationParentPath = Path.GetDirectoryName(destinationFullPath);
        var sourceLeafName = Path.GetFileName(sourceFullPath);
        var destinationLeafName = Path.GetFileName(destinationFullPath);
        if (string.IsNullOrEmpty(sourceParentPath) ||
            string.IsNullOrEmpty(destinationParentPath) ||
            !IsSafeLeafName(sourceLeafName) ||
            !IsSafeLeafName(destinationLeafName))
        {
            return false;
        }

        LinuxDirectoryHandleChain? sourceParentChain = null;
        LinuxDirectoryHandleChain? destinationParentChain = null;
        try
        {
            if (!ProofMatchesStream(stream, proof) ||
                !PathMatchesProof(sourceFullPath, proof, share))
            {
                return false;
            }

            sourceParentChain = OpenLinuxDirectoryChain(
                sourceParentPath,
                createMissing: false,
                finalDirectoryMustBeOwned: true);
            destinationParentChain = StringComparer.Ordinal.Equals(
                    sourceParentPath,
                    destinationParentPath)
                ? sourceParentChain
                : OpenLinuxDirectoryChain(
                    destinationParentPath,
                    createMissing: false,
                    finalDirectoryMustBeOwned: true);
            var sourceParentHandle = sourceParentChain.DirectoryHandle;
            var destinationParentHandle = destinationParentChain.DirectoryHandle;
            using (var sourceReopened = OpenLinuxLeaf(
                       sourceParentHandle,
                       sourceLeafName,
                       sourceFullPath,
                       GenericReadAccess,
                       OpenExistingDisposition))
            {
                var retainedShape = CaptureLinuxFileShape(
                    stream.SafeFileHandle,
                    sourceFullPath,
                    requireRegularFile: true);
                var sourceShape = CaptureLinuxFileShape(
                    sourceReopened,
                    sourceFullPath,
                    requireRegularFile: true);
                if (sourceShape.Identity != retainedShape.Identity ||
                    sourceShape.Length != retainedShape.Length ||
                    sourceShape.LinkCount != retainedShape.LinkCount ||
                    sourceShape.LinkCount != 1)
                {
                    return false;
                }
            }

            // RENAME_NOREPLACE закрывает окно между проверкой destination и
            // rename: если кто-то успел положить туда чужой файл или symlink, мы
            // получим отказ и не перезапишем его.
            if (RenameAt2Linux(
                    ToFileDescriptor(sourceParentHandle),
                    sourceLeafName,
                    ToFileDescriptor(destinationParentHandle),
                    destinationLeafName,
                    LinuxRenameNoReplace) != 0)
            {
                return false;
            }

            FlushLinuxDirectory(destinationParentHandle, destinationParentPath);
            if (!ReferenceEquals(sourceParentHandle, destinationParentHandle))
                FlushLinuxDirectory(sourceParentHandle, sourceParentPath);
            return ProofMatchesStream(stream, proof) &&
                   PathMatchesProof(destinationFullPath, proof, share);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
        finally
        {
            if (!ReferenceEquals(sourceParentChain, destinationParentChain))
                destinationParentChain?.Dispose();
            sourceParentChain?.Dispose();
        }
    }

    private static OperationResult<string> SaveLinuxPayloadNoOverwrite(
        string canonicalPath,
        string temporaryFileName,
        string finalFileName,
        byte[] payload,
        string alreadyExistsErrorCode,
        string writeFailedErrorCode,
        Action<string, string, string>? beforeDirectorySegmentOpenForTesting,
        Action<string, string, string>? afterDirectorySegmentOpenForTesting)
    {
        var directory = Path.GetDirectoryName(canonicalPath);
        if (string.IsNullOrWhiteSpace(directory) ||
            !IsSafeLeafName(temporaryFileName) ||
            !IsSafeLeafName(finalFileName))
        {
            return OperationResult<string>.Fail(
                "Generated capability manifest path is invalid.",
                writeFailedErrorCode);
        }

        FileStream? temporaryStream = null;
        var canonicalDirectory = Path.GetFullPath(directory);
        var temporaryPath = Path.Combine(canonicalDirectory, temporaryFileName);
        var retainedLeafName = temporaryFileName;
        var committed = false;
        var lockTaken = false;
        LinuxDirectoryHandleChain? directoryChain = null;
        try
        {
            directoryChain = OpenLinuxDirectoryChain(
                canonicalDirectory,
                createMissing: true,
                finalDirectoryMustBeOwned: true,
                beforeDirectorySegmentOpenForTesting,
                afterDirectorySegmentOpenForTesting);
            var directoryHandle = directoryChain.DirectoryHandle;
            lockTaken = TryAcquireLinuxAdvisoryLock(directoryHandle, canonicalDirectory);
            if (!lockTaken)
            {
                return OperationResult<string>.Fail(
                    "Generated capability manifest publication lock is unavailable.",
                    writeFailedErrorCode);
            }

            temporaryStream = OpenLinuxIdentityStreamAt(
                directoryHandle,
                temporaryFileName,
                temporaryPath,
                GenericReadAccess | GenericWriteAccess | DeleteAccess,
                CreateNewDisposition);
            temporaryStream.Write(payload, 0, payload.Length);
            temporaryStream.Flush(flushToDisk: true);
            var temporaryProof = CaptureIdentityProof(
                temporaryStream,
                temporaryPath,
                step: 0,
                DpoTrainerArtifactKind.PolicySerialization);
            if (temporaryProof.Length != payload.LongLength ||
                temporaryProof.LinkCount != 1 ||
                !PathMatchesProof(temporaryPath, temporaryProof, FileShare.Read))
            {
                return OperationResult<string>.Fail(
                    "Generated capability manifest temporary identity is invalid.",
                    writeFailedErrorCode);
            }

            var renameResult = RenameAt2Linux(
                ToFileDescriptor(directoryHandle),
                temporaryFileName,
                ToFileDescriptor(directoryHandle),
                finalFileName,
                LinuxRenameNoReplace);
            if (renameResult != 0)
            {
                var error = Marshal.GetLastPInvokeError();
                return error is LinuxErrorFileExists
                    ? OperationResult<string>.Fail(
                        "Generated capability manifest already exists.",
                        alreadyExistsErrorCode)
                    : OperationResult<string>.Fail(
                        "Generated capability manifest publication failed.",
                        writeFailedErrorCode);
            }

            retainedLeafName = finalFileName;
            FlushLinuxDirectory(directoryHandle, canonicalDirectory);
            if (!PathMatchesProof(
                    canonicalPath,
                    temporaryProof with { Path = canonicalPath },
                    FileShare.Read))
            {
                return OperationResult<string>.Fail(
                    "Generated capability manifest publication identity changed.",
                    writeFailedErrorCode);
            }

            committed = true;
            return OperationResult<string>.Ok(canonicalPath);
        }
        catch (EntryPointNotFoundException)
        {
            return OperationResult<string>.Fail(
                "Generated capability manifest publication primitive is unavailable.",
                writeFailedErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<string>.Fail(
                "Generated capability manifest publication failed.",
                writeFailedErrorCode);
        }
        finally
        {
            if (!committed && temporaryStream is not null)
            {
                try
                {
                    var retainedPath = Path.Combine(
                        canonicalDirectory,
                        retainedLeafName);
                    if (directoryChain is not null)
                    {
                        MarkLinuxIdentityForDeletionAt(
                            temporaryStream,
                            directoryChain.DirectoryHandle,
                            retainedLeafName,
                            retainedPath,
                            canonicalDirectory,
                            beforeUnlinkForTesting: null);
                    }
                    else
                    {
                        MarkLinuxIdentityForDeletion(
                            temporaryStream,
                            retainedPath,
                            beforeUnlinkForTesting: null);
                    }
                }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    // Отказ очистки не маскирует fail-closed результат: главное,
                    // что мы не удалили чужой файл по имени без сверки inode.
                }
            }

            temporaryStream?.Dispose();
            if (directoryChain is not null)
            {
                if (lockTaken)
                    _ = FlockLinux(
                        ToFileDescriptor(directoryChain.DirectoryHandle),
                        LinuxFlockUnlock);
                directoryChain.Dispose();
            }
        }
    }

    private static FileStream OpenLinuxIdentityStreamAt(
        SafeFileHandle parentHandle,
        string leafName,
        string fullPath,
        uint desiredAccess,
        uint creationDisposition)
    {
        var handle = OpenLinuxLeaf(
            parentHandle,
            leafName,
            fullPath,
            desiredAccess,
            creationDisposition);
        try
        {
            ValidateLinuxRegularFileHandle(handle, fullPath);
            return new FileStream(handle, FileAccessFor(desiredAccess));
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    private static LinuxDirectoryHandleChain OpenLinuxDirectoryChain(
        string path,
        bool createMissing,
        bool finalDirectoryMustBeOwned,
        Action<string, string, string>? beforeDirectorySegmentOpenForTesting = null,
        Action<string, string, string>? afterDirectorySegmentOpenForTesting = null)
    {
        EnsureLinuxX64StatLayout();
        var fullPath = Path.GetFullPath(path);
        if (!Path.IsPathRooted(fullPath))
            throw new IOException("LOGICAL_CAPABILITY_DIRECTORY_PATH_UNSAFE");
        var root = Path.GetPathRoot(fullPath);
        if (!StringComparer.Ordinal.Equals(root, "/"))
            throw new IOException("LOGICAL_CAPABILITY_DIRECTORY_ROOT_UNSUPPORTED");

        var handles = new List<SafeFileHandle>();
        try
        {
            // Идём от "/" только через удержанные directory fd. Строка пути
            // нужна для выбора следующего segment и сообщения об ошибке, но не
            // является доказательством владения: каждый следующий шаг делает
            // openat/mkdirat относительно уже проверенного parent fd.
            var rootHandle = OpenLinuxDirectoryRoot(root);
            handles.Add(rootHandle);
            var currentPath = root;
            var relative = Path.GetRelativePath(root, fullPath);
            if (!string.Equals(relative, ".", StringComparison.Ordinal))
            {
                var segments = relative.Split(
                    [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                    StringSplitOptions.RemoveEmptyEntries);
                for (var index = 0; index < segments.Length; index++)
                {
                    var segment = segments[index];
                    if (!IsSafeLeafName(segment))
                        throw new IOException("LOGICAL_CAPABILITY_DIRECTORY_SEGMENT_UNSAFE");
                    var nextPath = string.Equals(
                            currentPath,
                            "/",
                            StringComparison.Ordinal)
                        ? "/" + segment
                        : Path.Combine(currentPath, segment);

                    beforeDirectorySegmentOpenForTesting?.Invoke(
                        currentPath,
                        segment,
                        nextPath);
                    var child = OpenLinuxDirectorySegment(
                        handles[^1],
                        currentPath,
                        segment,
                        nextPath,
                        createMissing,
                        out var created);
                    var childTransferred = false;
                    try
                    {
                        var isFinal = index == segments.Length - 1;
                        ValidateLinuxDirectoryHandle(
                            child,
                            nextPath,
                            requireOwnedCustodyDirectory:
                            created || (finalDirectoryMustBeOwned && isFinal));

                        // SafeFileHandle становится собственностью chain только
                        // после успешной проверки. До этого точечного момента
                        // local finally обязан закрыть child, иначе отказ trust
                        // оставит открытый fd в текущем процессе.
                        handles.Add(child);
                        childTransferred = true;
                    }
                    finally
                    {
                        if (!childTransferred)
                            child.Dispose();
                    }

                    afterDirectorySegmentOpenForTesting?.Invoke(
                        currentPath,
                        segment,
                        nextPath);
                    currentPath = nextPath;
                }
            }

            return new LinuxDirectoryHandleChain(fullPath, handles);
        }
        catch
        {
            for (var index = handles.Count - 1; index >= 0; index--)
                handles[index].Dispose();
            throw;
        }
    }

    private static SafeFileHandle OpenLinuxDirectoryRoot(string root)
    {
        var descriptor = OpenLinux(
            root,
            LinuxOpenReadOnly | LinuxOpenDirectory | LinuxOpenNoFollow |
            LinuxOpenCloseOnExec,
            0);
        if (descriptor < 0)
        {
            throw LinuxIOException(
                "open logical-capability trusted root directory",
                root,
                Marshal.GetLastPInvokeError());
        }

        var handle = new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
        try
        {
            ValidateLinuxDirectoryHandle(
                handle,
                root,
                requireOwnedCustodyDirectory: false);
            return handle;
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    private static SafeFileHandle OpenLinuxDirectorySegment(
        SafeFileHandle parentHandle,
        string parentPath,
        string segment,
        string fullPath,
        bool createMissing,
        out bool created)
    {
        created = false;
        var descriptor = OpenAtLinux(
            ToFileDescriptor(parentHandle),
            segment,
            LinuxOpenReadOnly | LinuxOpenDirectory | LinuxOpenNoFollow |
            LinuxOpenCloseOnExec,
            0);
        if (descriptor < 0)
        {
            var error = Marshal.GetLastPInvokeError();
            if (!createMissing || error is not LinuxErrorNoEntry)
            {
                throw LinuxIOException(
                    "open logical-capability directory segment without following links",
                    fullPath,
                    error);
            }

            if (MkDirAtLinux(
                    ToFileDescriptor(parentHandle),
                    segment,
                    LinuxPrivateDirectoryMode) != 0)
            {
                var mkdirError = Marshal.GetLastPInvokeError();
                if (mkdirError is not LinuxErrorFileExists)
                {
                    throw LinuxIOException(
                        "create logical-capability directory segment",
                        fullPath,
                        mkdirError);
                }
            }
            else
            {
                created = true;
                FlushLinuxDirectory(parentHandle, parentPath);
            }

            descriptor = OpenAtLinux(
                ToFileDescriptor(parentHandle),
                segment,
                LinuxOpenReadOnly | LinuxOpenDirectory | LinuxOpenNoFollow |
                LinuxOpenCloseOnExec,
                0);
            if (descriptor < 0)
            {
                throw LinuxIOException(
                    "open created logical-capability directory segment",
                    fullPath,
                    Marshal.GetLastPInvokeError());
            }
        }

        return new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
    }

    private static SafeFileHandle OpenLinuxLeaf(
        SafeFileHandle parentHandle,
        string leafName,
        string fullPath,
        uint desiredAccess,
        uint creationDisposition)
    {
        if (!IsSafeLeafName(leafName))
            throw new IOException("LOGICAL_CAPABILITY_FILE_LEAF_UNSAFE");
        var flags = ((desiredAccess & GenericWriteAccess) != 0
                ? LinuxOpenReadWrite
                : LinuxOpenReadOnly) |
            LinuxOpenNoFollow |
            LinuxOpenCloseOnExec;
        if (creationDisposition == CreateNewDisposition)
            flags |= LinuxOpenCreate | LinuxOpenExclusive;
        else if (creationDisposition != OpenExistingDisposition)
            throw new IOException("LOGICAL_CAPABILITY_FILE_OPEN_MODE_UNSUPPORTED");

        var descriptor = OpenAtLinux(
            ToFileDescriptor(parentHandle),
            leafName,
            flags,
            LinuxPrivateFileMode);
        if (descriptor < 0)
        {
            throw LinuxIOException(
                "open logical-capability leaf without following links",
                fullPath,
                Marshal.GetLastPInvokeError());
        }

        return new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
    }

    private static bool TryAcquireLinuxAdvisoryLock(
        SafeFileHandle directoryHandle,
        string directory)
    {
        var descriptor = ToFileDescriptor(directoryHandle);
        var timeout = TimeSpan.FromSeconds(30);
        var wait = Stopwatch.StartNew();
        while (true)
        {
            if (FlockLinux(
                    descriptor,
                    LinuxFlockExclusive | LinuxFlockNonBlocking) == 0)
            {
                // Это advisory-lock, а не Windows mandatory custody. Чужой
                // процесс всё равно ловится identity/readback проверками ниже.
                return true;
            }

            var error = Marshal.GetLastPInvokeError();
            if (error is not (LinuxErrorWouldBlock or LinuxErrorInterrupted))
            {
                throw LinuxIOException(
                    "acquire logical-capability publication lock",
                    directory,
                    error);
            }

            if (wait.Elapsed >= timeout)
                return false;
            Thread.Sleep(TimeSpan.FromMilliseconds(25));
        }
    }

    private static void FlushLinuxDirectory(SafeFileHandle directoryHandle, string directory)
    {
        if (FsyncLinux(ToFileDescriptor(directoryHandle)) != 0)
        {
            var error = Marshal.GetLastPInvokeError();
            if (error is not (LinuxErrorInvalidArgument or LinuxErrorOperationNotSupported))
            {
                throw LinuxIOException(
                    "flush logical-capability directory",
                    directory,
                    error);
            }
        }
    }

    private static void ValidateLinuxRegularFileHandle(
        SafeFileHandle handle,
        string path)
        => ValidateLinuxRegularStat(GetLinuxStat(handle, path), path);

    private static void ValidateLinuxRegularStat(LinuxStat stat, string path)
    {
        if ((stat.Mode & LinuxFileTypeMask) != LinuxRegularFileType)
            throw new IOException("LOGICAL_CAPABILITY_FILE_IDENTITY_NON_REGULAR_FILE");
        ValidateLinuxOwnerAndWriteTrust(stat, path);
    }

    private static void ValidateLinuxDirectoryHandle(
        SafeFileHandle handle,
        string path,
        bool requireOwnedCustodyDirectory)
    {
        var stat = GetLinuxStat(handle, path);
        if ((stat.Mode & LinuxFileTypeMask) != LinuxDirectoryFileType)
            throw new IOException("LOGICAL_CAPABILITY_DIRECTORY_KIND_UNSAFE");
        ValidateLinuxDirectoryOwnerAndWriteTrust(
            stat,
            path,
            requireOwnedCustodyDirectory);
    }

    private static void ValidateLinuxDirectoryOwnerAndWriteTrust(
        LinuxStat stat,
        string path,
        bool requireOwnedCustodyDirectory)
    {
        var permissionBits = stat.Mode & LinuxPermissionBitsMask;
        var groupOrOtherWritable =
            (permissionBits & LinuxGroupOrOtherWriteMask) != 0;
        var sticky = (stat.Mode & LinuxStickyBit) != 0;
        if (requireOwnedCustodyDirectory)
        {
            if (stat.Uid != GetEffectiveUserIdLinux() ||
                groupOrOtherWritable)
            {
                throw new IOException(
                    $"LOGICAL_CAPABILITY_DIRECTORY_OWNER_TRUST_UNPROVEN:{path}");
            }

            return;
        }

        // Предки вроде "/" или "/mnt" могут быть root-owned и неизменяемыми
        // для текущего пользователя, а "/tmp" обычно world-writable со sticky
        // bit. Мы не считаем их owned-артефактами: доверяем только удержанному
        // descriptor-walk и запрещаем writable ancestor без sticky-защиты.
        if (groupOrOtherWritable && !sticky)
        {
            throw new IOException(
                $"LOGICAL_CAPABILITY_DIRECTORY_ANCESTOR_WRITE_TRUST_UNPROVEN:{path}");
        }
    }

    private static void ValidateLinuxOwnerAndWriteTrust(
        LinuxStat stat,
        string path)
    {
        var permissionBits = stat.Mode & LinuxPermissionBitsMask;
        if (stat.Uid != GetEffectiveUserIdLinux() ||
            (permissionBits & LinuxGroupOrOtherWriteMask) != 0)
        {
            throw new IOException(
                $"LOGICAL_CAPABILITY_FILE_OWNER_TRUST_UNPROVEN:{path}");
        }
    }

    private static LinuxStat GetLinuxStat(SafeFileHandle handle, string path)
    {
        EnsureLinuxX64StatLayout();
        var descriptor = ToFileDescriptor(handle);
        if (FStatLinux(descriptor, out var stat) != 0)
        {
            throw LinuxIOException(
                "inspect logical-capability file handle",
                path,
                Marshal.GetLastPInvokeError());
        }

        return stat;
    }

    private static bool TryCreateLinuxHardLinkForTesting(
        string existingPath,
        string newPath)
        => LinkLinux(
            Path.GetFullPath(existingPath),
            Path.GetFullPath(newPath)) == 0;

    private static bool IsSafeLeafName(string leafName)
        => !string.IsNullOrWhiteSpace(leafName) &&
           leafName.IndexOf('\0') < 0 &&
           string.Equals(Path.GetFileName(leafName), leafName, StringComparison.Ordinal) &&
           leafName is not "." and not "..";

    private static int ToFileDescriptor(SafeFileHandle handle)
    {
        if (handle.IsInvalid || handle.IsClosed)
            throw new IOException("LOGICAL_CAPABILITY_FILE_HANDLE_UNAVAILABLE");
        return checked((int)handle.DangerousGetHandle().ToInt64());
    }

    private static IOException LinuxIOException(string operation, string path, int error)
        => new($"Logical capability file custody failed to {operation} '{path}' (errno {error}).");

    private static void EnsureLinuxX64StatLayout()
    {
        if (!OperatingSystem.IsLinux() ||
            RuntimeInformation.ProcessArchitecture != Architecture.X64)
        {
            throw new PlatformNotSupportedException(
                "Logical capability Linux file identity is verified only for the x64 stat layout.");
        }
    }

    private sealed class LinuxDirectoryHandleChain(
        string fullPath,
        List<SafeFileHandle> handles) : IDisposable
    {
        internal string FullPath { get; } = fullPath;

        internal SafeFileHandle DirectoryHandle =>
            handles.Count == 0
                ? throw new IOException("LOGICAL_CAPABILITY_DIRECTORY_HANDLE_UNAVAILABLE")
                : handles[^1];

        public void Dispose()
        {
            for (var index = handles.Count - 1; index >= 0; index--)
                handles[index].Dispose();
            handles.Clear();
        }
    }

    private const int LinuxOpenReadOnly = 0;
    private const int LinuxOpenReadWrite = 2;
    private const int LinuxOpenCreate = 0x40;
    private const int LinuxOpenExclusive = 0x80;
    private const int LinuxOpenDirectory = 0x10000;
    private const int LinuxOpenNoFollow = 0x20000;
    private const int LinuxOpenCloseOnExec = 0x80000;
    private const int LinuxStickyBit = 0x200;
    private const int LinuxFileTypeMask = 0xF000;
    private const int LinuxRegularFileType = 0x8000;
    private const int LinuxDirectoryFileType = 0x4000;
    private const int LinuxSymbolicLinkFileType = 0xA000;
    private const int LinuxErrorNoEntry = 2;
    private const int LinuxErrorInterrupted = 4;
    private const int LinuxErrorWouldBlock = 11;
    private const int LinuxErrorFileExists = 17;
    private const int LinuxErrorNotDirectory = 20;
    private const int LinuxErrorInvalidArgument = 22;
    private const int LinuxErrorFunctionNotImplemented = 38;
    private const int LinuxErrorOperationNotSupported = 95;
    private const int LinuxFlockExclusive = 0x02;
    private const int LinuxFlockNonBlocking = 0x04;
    private const int LinuxFlockUnlock = 0x08;
    private const uint LinuxPermissionBitsMask = 0x1FF;
    private const uint LinuxGroupOrOtherWriteMask = 0x12;
    private const uint LinuxPrivateFileMode = 0x180; // 0600: читать/писать может только владелец процесса.
    private const uint LinuxPrivateDirectoryMode = 0x1C0; // 0700: каталог публикации принадлежит только текущей сделке.
    private const uint LinuxRenameNoReplace = 1;

    [StructLayout(LayoutKind.Sequential)]
    private struct LinuxStat
    {
        internal ulong Device;
        internal ulong Inode;
        internal ulong LinkCount;
        internal uint Mode;
        internal uint Uid;
        internal uint Gid;
        internal int Padding0;
        internal ulong DeviceId;
        internal long Size;
        internal long BlockSize;
        internal long Blocks;
        internal long AccessTimeSeconds;
        internal long AccessTimeNanoseconds;
        internal long ModificationTimeSeconds;
        internal long ModificationTimeNanoseconds;
        internal long ChangeTimeSeconds;
        internal long ChangeTimeNanoseconds;
        internal long Unused0;
        internal long Unused1;
        internal long Unused2;
    }

    [DllImport("libc", EntryPoint = "open", SetLastError = true)]
    private static extern int OpenLinux(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags,
        uint mode);

    [DllImport("libc", EntryPoint = "openat", SetLastError = true)]
    private static extern int OpenAtLinux(
        int directoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags,
        uint mode);

    [DllImport("libc", EntryPoint = "mkdirat", SetLastError = true)]
    private static extern int MkDirAtLinux(
        int directoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        uint mode);

    [DllImport("libc", EntryPoint = "unlinkat", SetLastError = true)]
    private static extern int UnlinkAtLinux(
        int directoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags);

    [DllImport("libc", EntryPoint = "renameat2", SetLastError = true)]
    private static extern int RenameAt2Linux(
        int oldDirectoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string oldPath,
        int newDirectoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string newPath,
        uint flags);

    [DllImport("libc", EntryPoint = "link", SetLastError = true)]
    private static extern int LinkLinux(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string existingPath,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string newPath);

    [DllImport("libc", EntryPoint = "fstat", SetLastError = true)]
    private static extern int FStatLinux(
        int fileDescriptor,
        out LinuxStat status);

    [DllImport("libc", EntryPoint = "fsync", SetLastError = true)]
    private static extern int FsyncLinux(int fileDescriptor);

    [DllImport("libc", EntryPoint = "flock", SetLastError = true)]
    private static extern int FlockLinux(int fileDescriptor, int operation);

    [DllImport("libc", EntryPoint = "geteuid")]
    private static extern uint GetEffectiveUserIdLinux();
}
