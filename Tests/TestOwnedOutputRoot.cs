// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.ComponentModel;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace LlmFromScratch.Tests;

internal sealed class TestOwnedOutputRoot
{
    internal const string CleanupFailureDataKey = "TestOwnedOutputRoot.CleanupFailure";
    private const string RootPrefix = "llm-from-scratch-test-output-";
    private readonly Action? _postCleanupFailure;
    private readonly Action<string>? _traversalLeaseAcquired;

    private TestOwnedOutputRoot(
        Action? postCleanupFailure,
        Action<string>? traversalLeaseAcquired)
    {
        RepositoryRootPath = FindRepositoryRoot();
        var tempRoot = CanonicalDirectory(Path.GetTempPath());
        RootPath = Path.Combine(tempRoot, RootPrefix + Guid.NewGuid().ToString("N"));
        ValidateOwnedRootIdentity(RootPath, tempRoot, RepositoryRootPath, requireExists: false);
        Directory.CreateDirectory(RootPath);
        ValidateOwnedRootIdentity(RootPath, tempRoot, RepositoryRootPath, requireExists: true);
        // Русский комментарий для сопровождающего: сразу после создания проверяем
        // каталог тем же lease-механизмом, которым потом чистим дерево. На Unix это
        // нативный open без следования финальной ссылке; на Windows это прежний
        // handle без delete-share.
        using var rootLease = DirectoryTraversalLease.Acquire(RootPath);
        _postCleanupFailure = postCleanupFailure;
        _traversalLeaseAcquired = traversalLeaseAcquired;
    }

    public string RootPath { get; }

    internal string RepositoryRootPath { get; }

    public static void Run(
        Action<TestOwnedOutputRoot> body,
        Action? postCleanupFailure = null,
        Action<string>? traversalLeaseAcquired = null)
    {
        ArgumentNullException.ThrowIfNull(body);
        var owner = new TestOwnedOutputRoot(postCleanupFailure, traversalLeaseAcquired);
        ExceptionDispatchInfo? primary = null;

        try
        {
            body(owner);
        }
        catch (Exception exception)
        {
            primary = ExceptionDispatchInfo.Capture(exception);
        }

        var cleanupFailure = owner.TryCleanup();
        Rethrow(primary, cleanupFailure);
    }

    public static T Run<T>(
        Func<TestOwnedOutputRoot, T> body,
        Action? postCleanupFailure = null,
        Action<string>? traversalLeaseAcquired = null)
    {
        ArgumentNullException.ThrowIfNull(body);
        T result = default!;
        Run(owner =>
        {
            result = body(owner);
        }, postCleanupFailure, traversalLeaseAcquired);
        return result;
    }

    public static async Task RunAsync(
        Func<TestOwnedOutputRoot, Task> body,
        Action? postCleanupFailure = null,
        Action<string>? traversalLeaseAcquired = null)
    {
        ArgumentNullException.ThrowIfNull(body);
        var owner = new TestOwnedOutputRoot(postCleanupFailure, traversalLeaseAcquired);
        ExceptionDispatchInfo? primary = null;

        try
        {
            await body(owner).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            primary = ExceptionDispatchInfo.Capture(exception);
        }

        var cleanupFailure = owner.TryCleanup();
        Rethrow(primary, cleanupFailure);
    }

    public string RequireOwnedPath(string path)
    {
        var fullPath = ValidateFullyQualifiedPath(path);
        ValidateOwnedRootIdentity(
            RootPath,
            CanonicalDirectory(Path.GetTempPath()),
            RepositoryRootPath,
            requireExists: true);

        if (!IsSameOrDescendant(fullPath, RootPath))
            throw new InvalidOperationException($"Path is outside the test-owned output root: {path}");
        if (IsSameOrDescendant(fullPath, RepositoryRootPath))
            throw new InvalidOperationException($"Repository paths are not test-owned output paths: {path}");

        RejectExistingReparseSegments(RootPath, fullPath);
        return fullPath;
    }

    public string ToStableReference(string path)
    {
        var fullPath = ValidateFullyQualifiedPath(path);
        if (IsSameOrDescendant(fullPath, RepositoryRootPath))
        {
            RejectExistingReparseSegments(RepositoryRootPath, fullPath);
            return NormalizeStableRelativePath(Path.GetRelativePath(RepositoryRootPath, fullPath));
        }

        if (IsSameOrDescendant(fullPath, RootPath))
        {
            var ownedPath = RequireOwnedPath(fullPath);
            var relative = Path.GetRelativePath(RootPath, ownedPath);
            return relative == "."
                ? "test-owned-output"
                : "test-owned-output/" + NormalizeStableRelativePath(relative);
        }

        throw new InvalidOperationException($"Path is neither repository input nor test-owned output: {path}");
    }

    private Exception? TryCleanup()
    {
        try
        {
            Cleanup();
            _postCleanupFailure?.Invoke();
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    private void Cleanup()
    {
        var tempRoot = CanonicalDirectory(Path.GetTempPath());
        ValidateOwnedRootIdentity(RootPath, tempRoot, RepositoryRootPath, requireExists: true);
        DeleteChildrenWithoutFollowingReparsePoints(RootPath);
        ValidateOwnedRootIdentity(RootPath, tempRoot, RepositoryRootPath, requireExists: true);
        Directory.Delete(RootPath, recursive: false);
    }

    private void DeleteChildrenWithoutFollowingReparsePoints(string directory)
    {
        using var traversalLease = DirectoryTraversalLease.Acquire(directory);
        _traversalLeaseAcquired?.Invoke(directory);

        foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
        {
            var attributes = File.GetAttributes(entry);
            var isDirectory = (attributes & FileAttributes.Directory) != 0;
            var isReparsePoint = (attributes & FileAttributes.ReparsePoint) != 0;

            if (isReparsePoint)
            {
                if (isDirectory)
                    Directory.Delete(entry, recursive: false);
                else
                    File.Delete(entry);
                continue;
            }

            if (isDirectory)
            {
                DeleteChildrenWithoutFollowingReparsePoints(entry);
                Directory.Delete(entry, recursive: false);
            }
            else
            {
                File.Delete(entry);
            }
        }
    }

    private static void Rethrow(ExceptionDispatchInfo? primary, Exception? cleanupFailure)
    {
        if (primary is not null)
        {
            if (cleanupFailure is not null)
                primary.SourceException.Data[CleanupFailureDataKey] = cleanupFailure.ToString();
            primary.Throw();
        }

        if (cleanupFailure is not null)
            ExceptionDispatchInfo.Capture(cleanupFailure).Throw();
    }

    private static string ValidateFullyQualifiedPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!Path.IsPathFullyQualified(path))
            throw new InvalidOperationException($"Path must be fully qualified: {path}");
        if (ContainsParentTraversal(path))
            throw new InvalidOperationException($"Parent traversal is not allowed: {path}");
        return Path.GetFullPath(path);
    }

    private static bool ContainsParentTraversal(string path)
        => path.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries)
            .Any(segment => segment == "..");

    private static void ValidateOwnedRootIdentity(
        string rootPath,
        string tempRoot,
        string repositoryRoot,
        bool requireExists)
    {
        var canonicalRoot = Path.GetFullPath(rootPath);
        var canonicalTemp = CanonicalDirectory(tempRoot);
        if (!string.Equals(Path.GetDirectoryName(canonicalRoot), canonicalTemp, PathComparison))
            throw new InvalidOperationException("The test-owned root must be a direct child of the canonical temp directory.");

        var leaf = Path.GetFileName(canonicalRoot);
        if (!leaf.StartsWith(RootPrefix, StringComparison.Ordinal)
            || !Guid.TryParseExact(leaf[RootPrefix.Length..], "N", out _))
        {
            throw new InvalidOperationException("The test-owned root does not have the required GUID ownership identity.");
        }

        if (IsSameOrDescendant(canonicalRoot, repositoryRoot))
            throw new InvalidOperationException("The test-owned root must be outside the repository.");

        if (!requireExists)
            return;
        if (!Directory.Exists(canonicalRoot))
            throw new DirectoryNotFoundException($"The test-owned root no longer exists: {canonicalRoot}");
        if ((File.GetAttributes(canonicalRoot) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidOperationException("The test-owned root must not be a reparse point.");
    }

    private static void RejectExistingReparseSegments(string trustedRoot, string candidate)
    {
        var relative = Path.GetRelativePath(trustedRoot, candidate);
        if (relative == ".")
        {
            RejectReparsePointIfPresent(trustedRoot);
            return;
        }

        var current = trustedRoot;
        RejectReparsePointIfPresent(current);
        foreach (var segment in relative.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, segment);
            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(current);
            }
            catch (FileNotFoundException)
            {
                break;
            }
            catch (DirectoryNotFoundException)
            {
                break;
            }

            if ((attributes & FileAttributes.ReparsePoint) != 0)
                throw new InvalidOperationException($"Reparse-point traversal is not allowed: {current}");
        }
    }

    private static void RejectReparsePointIfPresent(string path)
    {
        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidOperationException($"Reparse-point traversal is not allowed: {path}");
    }

    private static string NormalizeStableRelativePath(string relative)
    {
        if (Path.IsPathFullyQualified(relative) || ContainsParentTraversal(relative) || relative == "..")
            throw new InvalidOperationException($"Unsafe stable reference: {relative}");
        var normalized = relative.Replace('\\', '/');
        if (normalized.StartsWith("../", StringComparison.Ordinal) || normalized.Contains("/../", StringComparison.Ordinal))
            throw new InvalidOperationException($"Unsafe stable reference: {relative}");
        return normalized;
    }

    private static bool IsSameOrDescendant(string candidate, string root)
    {
        var canonicalCandidate = Path.TrimEndingDirectorySeparator(Path.GetFullPath(candidate));
        var canonicalRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        return string.Equals(canonicalCandidate, canonicalRoot, PathComparison)
            || canonicalCandidate.StartsWith(canonicalRoot + Path.DirectorySeparatorChar, PathComparison);
    }

    private static string CanonicalDirectory(string path)
        => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

    private static StringComparison PathComparison
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return CanonicalDirectory(current.FullName);
            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the canonical repository root.");
    }

    private sealed class DirectoryTraversalLease : IDisposable
    {
        private const uint FileListDirectory = 0x00000001;
        private const uint FileReadAttributes = 0x00000080;
        private const uint FileShareRead = 0x00000001;
        private const uint FileShareWrite = 0x00000002;
        private const uint OpenExisting = 3;
        private const uint FileFlagBackupSemantics = 0x02000000;
        private const uint FileFlagOpenReparsePoint = 0x00200000;
        private const uint FileAttributeDirectory = 0x00000010;
        private const uint FileAttributeReparsePoint = 0x00000400;
        private const int UnixWouldBlockLinux = 11;
        private const int UnixWouldBlockMacOS = 35;

        private readonly SafeFileHandle _handle;

        private DirectoryTraversalLease(SafeFileHandle handle)
            => _handle = handle;

        public static DirectoryTraversalLease Acquire(string directory)
        {
            if (!OperatingSystem.IsWindows())
                return AcquireUnix(directory);

            var handle = NativeMethods.CreateFileW(
                directory,
                FileListDirectory | FileReadAttributes,
                FileShareRead | FileShareWrite,
                IntPtr.Zero,
                OpenExisting,
                FileFlagBackupSemantics | FileFlagOpenReparsePoint,
                IntPtr.Zero);
            if (handle.IsInvalid)
            {
                var error = Marshal.GetLastWin32Error();
                handle.Dispose();
                throw Win32CleanupFailure("open traversal lease", directory, error);
            }

            if (!NativeMethods.GetFileInformationByHandle(handle, out var information))
            {
                var error = Marshal.GetLastWin32Error();
                handle.Dispose();
                throw Win32CleanupFailure("query traversal lease", directory, error);
            }

            if ((information.FileAttributes & FileAttributeDirectory) == 0)
            {
                handle.Dispose();
                throw new IOException($"Traversal lease target is not a directory: {directory}");
            }

            if ((information.FileAttributes & FileAttributeReparsePoint) != 0)
            {
                handle.Dispose();
                throw new IOException($"Traversal lease target is a reparse point: {directory}");
            }

            return new DirectoryTraversalLease(handle);
        }

        public void Dispose()
            => _handle.Dispose();

        private static DirectoryTraversalLease AcquireUnix(string directory)
        {
            var flags = UnixNativeFlagsForCurrentPlatform();

            // Русский комментарий: O_NOFOLLOW защищает именно последний компонент
            // пути. Если кто-то подменил test-owned каталог на ссылку наружу,
            // cleanup падает закрыто и не идёт в чужое дерево.
            var descriptor = NativeMethods.OpenUnix(directory, flags.DirectoryOpenFlags, 0);
            if (descriptor < 0)
                throw UnixCleanupFailure("open traversal lease without following links", directory, Marshal.GetLastPInvokeError());

            var handle = new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
            if (NativeMethods.Flock(descriptor, flags.ExclusiveNonBlockingLock) == 0)
                return new DirectoryTraversalLease(handle);

            var error = Marshal.GetLastPInvokeError();
            handle.Dispose();
            if (error is UnixWouldBlockLinux or UnixWouldBlockMacOS)
                throw new IOException($"Test-owned output directory is already leased by another process: {directory}");
            throw UnixCleanupFailure("lock traversal lease", directory, error);
        }

        private static UnixNativeFlags UnixNativeFlagsForCurrentPlatform()
        {
            if (OperatingSystem.IsLinux())
            {
                return new UnixNativeFlags(
                    OpenReadOnly: 0,
                    OpenDirectory: 0x10000,
                    OpenNoFollow: 0x20000,
                    OpenCloseOnExec: 0x80000,
                    FlockExclusive: 0x02,
                    FlockNonBlocking: 0x04);
            }

            if (OperatingSystem.IsMacOS())
            {
                return new UnixNativeFlags(
                    OpenReadOnly: 0,
                    OpenDirectory: 0x100000,
                    OpenNoFollow: 0x0100,
                    OpenCloseOnExec: 0x01000000,
                    FlockExclusive: 0x02,
                    FlockNonBlocking: 0x04);
            }

            throw new PlatformNotSupportedException(
                "Test-owned output cleanup supports no-follow traversal leases only on Windows, Linux, and macOS.");
        }

        private static IOException Win32CleanupFailure(string operation, string path, int error)
            => new(
                $"Could not {operation} for '{path}': {new Win32Exception(error).Message} (Win32={error}).",
                new Win32Exception(error));

        private static IOException UnixCleanupFailure(string operation, string path, int error)
            => new($"Could not {operation} for '{path}': errno {error}.");

        private readonly record struct UnixNativeFlags(
            int OpenReadOnly,
            int OpenDirectory,
            int OpenNoFollow,
            int OpenCloseOnExec,
            int FlockExclusive,
            int FlockNonBlocking)
        {
            public int DirectoryOpenFlags => OpenReadOnly | OpenDirectory | OpenNoFollow | OpenCloseOnExec;

            public int ExclusiveNonBlockingLock => FlockExclusive | FlockNonBlocking;
        }

        private static class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern SafeFileHandle CreateFileW(
                string fileName,
                uint desiredAccess,
                uint shareMode,
                IntPtr securityAttributes,
                uint creationDisposition,
                uint flagsAndAttributes,
                IntPtr templateFile);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetFileInformationByHandle(
                SafeFileHandle file,
                out ByHandleFileInformation information);

            [DllImport("libc", EntryPoint = "open", SetLastError = true)]
            public static extern int OpenUnix(string path, int flags, uint mode);

            [DllImport("libc", EntryPoint = "flock", SetLastError = true)]
            public static extern int Flock(int fileDescriptor, int operation);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ByHandleFileInformation
        {
            public uint FileAttributes;
            public FileTime CreationTime;
            public FileTime LastAccessTime;
            public FileTime LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FileTime
        {
            public uint LowDateTime;
            public uint HighDateTime;
        }
    }
}
