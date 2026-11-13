// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Runtime.InteropServices;
using System.Security.Cryptography;
using AgentInfrastructure;
using DPO;
using Microsoft.Win32.SafeHandles;

// Единая точка доверия к файловой личности для обучения способностей.
// Верхний код больше не гадает по имени ОС: он спрашивает, умеет ли текущая
// платформа удержать именно тот физический файл, который был создан и проверен.
internal static partial class LogicalCapabilityFileCustody
{
    internal const uint GenericReadAccess = 0x80000000;
    internal const uint GenericWriteAccess = 0x40000000;
    internal const uint DeleteAccess = 0x00010000;
    internal const uint CreateNewDisposition = 1;
    internal const uint OpenExistingDisposition = 3;

    internal static bool IsCurrentPlatformSupported =>
        OperatingSystem.IsWindows() ||
        (OperatingSystem.IsLinux() &&
         RuntimeInformation.ProcessArchitecture == Architecture.X64);

    internal static FileStream? OpenIdentityStream(
        string path,
        uint desiredAccess,
        FileShare share,
        uint creationDisposition = OpenExistingDisposition)
    {
        if (OperatingSystem.IsWindows())
            return OpenWindowsIdentityStream(path, desiredAccess, share, creationDisposition);
        if (OperatingSystem.IsLinux() &&
            RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            return OpenLinuxIdentityStream(path, desiredAccess, creationDisposition);
        }

        return null;
    }

    internal static DpoTrainerArtifactOwnershipProof CaptureIdentityProof(
        FileStream stream,
        string path,
        int step,
        DpoTrainerArtifactKind kind)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var fullPath = Path.GetFullPath(path);
        var shape = CaptureFileShape(stream.SafeFileHandle, fullPath);
        var originalPosition = stream.Position;
        stream.Position = 0;
        var sha256 = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        stream.Position = originalPosition;
        if (shape.Length != stream.Length)
            throw new IOException("LOGICAL_CAPABILITY_FILE_IDENTITY_LENGTH_CHANGED");

        return new DpoTrainerArtifactOwnershipProof(
            step,
            kind,
            fullPath,
            stream.Length,
            sha256,
            shape.LinkCount,
            shape.Identity);
    }

    internal static bool PathMatchesProof(
        string path,
        DpoTrainerArtifactOwnershipProof proof,
        FileShare share)
    {
        try
        {
            using var stream = OpenIdentityStream(
                path,
                GenericReadAccess,
                share);
            return stream is not null && ProofMatchesStream(stream, proof);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    internal static bool ProofMatchesStream(
        FileStream stream,
        DpoTrainerArtifactOwnershipProof proof)
    {
        try
        {
            var observed = CaptureIdentityProof(
                stream,
                proof.Path,
                proof.Step,
                proof.Kind);
            return observed.Identity == proof.Identity &&
                   observed.LinkCount == proof.LinkCount &&
                   observed.LinkCount == 1 &&
                   observed.Length == proof.Length &&
                   StringComparer.Ordinal.Equals(observed.Sha256, proof.Sha256);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    internal static bool OpenedHandleMatchesCanonicalPath(
        SafeFileHandle handle,
        string canonicalPath)
    {
        if (OperatingSystem.IsWindows())
            return WindowsHandleMatchesPath(handle, canonicalPath);
        if (OperatingSystem.IsLinux() &&
            RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            return LinuxHandleMatchesPath(handle, canonicalPath);
        }

        return false;
    }

    internal static bool StreamIdentitiesMatch(
        SafeFileHandle left,
        SafeFileHandle right)
    {
        if (OperatingSystem.IsWindows())
            return WindowsHandlesHaveSameIdentity(left, right);
        if (OperatingSystem.IsLinux() &&
            RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            return LinuxHandlesHaveSameIdentity(left, right);
        }

        return false;
    }

    internal static bool TryAcquireIdentityLock(FileStream stream, string path)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (OperatingSystem.IsWindows())
        {
            // На Windows сам retained handle с delete/share policy уже является
            // удержанием имени; дополнительный advisory-lock там не нужен.
            return true;
        }

        if (OperatingSystem.IsLinux() &&
            RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            // Linux lock-файл остаётся обычным файлом в каталоге DPO. Мы берём
            // flock на тот же открытый descriptor, чтобы принять только свободный
            // служебный sidecar и держать его до commit/rollback текущей сделки.
            return TryAcquireLinuxAdvisoryLock(stream.SafeFileHandle, path);
        }

        return false;
    }

    internal static void MarkIdentityForDeletion(FileStream stream, string path)
        => MarkIdentityForDeletionCore(
            stream,
            path,
            beforeLinuxUnlinkForTesting: null);

    internal static void MarkIdentityForDeletionForTesting(
        FileStream stream,
        string path,
        Action? beforeLinuxUnlinkForTesting)
        => MarkIdentityForDeletionCore(
            stream,
            path,
            beforeLinuxUnlinkForTesting);

    private static void MarkIdentityForDeletionCore(
        FileStream stream,
        string path,
        Action? beforeLinuxUnlinkForTesting)
    {
        if (OperatingSystem.IsWindows())
        {
            MarkWindowsIdentityForDeletion(stream);
            return;
        }

        if (OperatingSystem.IsLinux() &&
            RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            MarkLinuxIdentityForDeletion(
                stream,
                path,
                beforeLinuxUnlinkForTesting);
            return;
        }

        throw new IOException("LOGICAL_CAPABILITY_FILE_DELETE_UNAVAILABLE");
    }

    internal static bool TryRenameIdentityNoReplace(
        FileStream stream,
        DpoTrainerArtifactOwnershipProof proof,
        string sourcePath,
        string destinationPath,
        FileShare share)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(proof);
        if (OperatingSystem.IsLinux() &&
            RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            // Переименование через общий custody helper оставляет в верхнем
            // коде только бизнес-смысл: какую identity переносим. Все POSIX
            // детали no-follow, directory fd и RENAME_NOREPLACE живут здесь.
            return TryRenameLinuxIdentityNoReplace(
                stream,
                proof,
                sourcePath,
                destinationPath,
                share);
        }

        return false;
    }

    internal static OperationResult<string> SavePayloadNoOverwrite(
        string canonicalPath,
        string temporaryFileName,
        string finalFileName,
        byte[] payload,
        string custodyUnavailableErrorCode,
        string alreadyExistsErrorCode,
        string writeFailedErrorCode)
        => SavePayloadNoOverwriteCore(
            canonicalPath,
            temporaryFileName,
            finalFileName,
            payload,
            custodyUnavailableErrorCode,
            alreadyExistsErrorCode,
            writeFailedErrorCode,
            beforeLinuxDirectorySegmentOpenForTesting: null,
            afterLinuxDirectorySegmentOpenForTesting: null);

    internal static OperationResult<string> SavePayloadNoOverwriteForTesting(
        string canonicalPath,
        string temporaryFileName,
        string finalFileName,
        byte[] payload,
        string custodyUnavailableErrorCode,
        string alreadyExistsErrorCode,
        string writeFailedErrorCode,
        Action<string, string, string>? beforeLinuxDirectorySegmentOpenForTesting,
        Action<string, string, string>? afterLinuxDirectorySegmentOpenForTesting)
        => SavePayloadNoOverwriteCore(
            canonicalPath,
            temporaryFileName,
            finalFileName,
            payload,
            custodyUnavailableErrorCode,
            alreadyExistsErrorCode,
            writeFailedErrorCode,
            beforeLinuxDirectorySegmentOpenForTesting,
            afterLinuxDirectorySegmentOpenForTesting);

    private static OperationResult<string> SavePayloadNoOverwriteCore(
        string canonicalPath,
        string temporaryFileName,
        string finalFileName,
        byte[] payload,
        string custodyUnavailableErrorCode,
        string alreadyExistsErrorCode,
        string writeFailedErrorCode,
        Action<string, string, string>? beforeLinuxDirectorySegmentOpenForTesting,
        Action<string, string, string>? afterLinuxDirectorySegmentOpenForTesting)
    {
        ArgumentNullException.ThrowIfNull(payload);
        if (OperatingSystem.IsLinux() &&
            RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            return SaveLinuxPayloadNoOverwrite(
                canonicalPath,
                temporaryFileName,
                finalFileName,
                payload,
                alreadyExistsErrorCode,
                writeFailedErrorCode,
                beforeLinuxDirectorySegmentOpenForTesting,
                afterLinuxDirectorySegmentOpenForTesting);
        }

        return OperationResult<string>.Fail(
            "Generated capability file identity custody is unavailable.",
            custodyUnavailableErrorCode);
    }

    internal static bool TryCreateHardLinkForTesting(
        string existingPath,
        string newPath)
    {
        if (OperatingSystem.IsWindows())
            return TryCreateWindowsHardLinkForTesting(existingPath, newPath);
        if (OperatingSystem.IsLinux() &&
            RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            return TryCreateLinuxHardLinkForTesting(existingPath, newPath);
        }

        return false;
    }

    private static LogicalCapabilityFileShape CaptureFileShape(
        SafeFileHandle handle,
        string fullPath)
    {
        if (OperatingSystem.IsWindows())
            return CaptureWindowsFileShape(handle);
        if (OperatingSystem.IsLinux() &&
            RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            return CaptureLinuxFileShape(handle, fullPath, requireRegularFile: true);
        }

        throw new IOException("LOGICAL_CAPABILITY_FILE_IDENTITY_UNAVAILABLE");
    }

    private static FileAccess FileAccessFor(uint desiredAccess)
        => (desiredAccess & GenericWriteAccess) != 0
            ? FileAccess.ReadWrite
            : FileAccess.Read;

    private readonly record struct LogicalCapabilityFileShape(
        long Length,
        uint LinkCount,
        DpoTrainerArtifactIdentity Identity);
}
