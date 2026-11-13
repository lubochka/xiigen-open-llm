// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

public sealed class TestOwnedOutputRootTests
{
    [Fact]
    public void Run_Success_DeletesCompleteOwnedRoot()
    {
        string? root = null;
        TestOwnedOutputRoot.Run(owner =>
        {
            root = owner.RootPath;
            File.WriteAllText(owner.RequireOwnedPath(Path.Combine(root, "result.txt")), "owned");
            Assert.True(Directory.Exists(root));
        });

        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public void Run_PrimaryException_PreservesTypeMessageAndOriginalStack()
    {
        string? root = null;
        var exception = Assert.Throws<OwnedPrimaryException>(() =>
            TestOwnedOutputRoot.Run(owner =>
            {
                root = owner.RootPath;
                ThrowOwnedPrimary();
            }));

        Assert.Equal("primary", exception.Message);
        Assert.Contains(nameof(ThrowOwnedPrimary), exception.StackTrace);
        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public async Task RunAsync_Cancellation_PreservesCancellationAndCleans()
    {
        string? root = null;
        using var source = new CancellationTokenSource();
        source.Cancel();

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            TestOwnedOutputRoot.RunAsync(owner =>
            {
                root = owner.RootPath;
                return Task.FromCanceled(source.Token);
            }));

        Assert.Equal(source.Token, exception.CancellationToken);
        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public void Run_CleanupFailureAfterSuccessfulCleanup_FailsWithoutDebris()
    {
        string? root = null;
        var exception = Assert.Throws<IOException>(() =>
            TestOwnedOutputRoot.Run(
                owner => root = owner.RootPath,
                postCleanupFailure: () => throw new IOException("cleanup-only")));

        Assert.Equal("cleanup-only", exception.Message);
        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public void Run_PrimaryAndCleanupFailure_PreservesPrimaryAndAttachesDiagnostic()
    {
        string? root = null;
        var exception = Assert.Throws<OwnedPrimaryException>(() =>
            TestOwnedOutputRoot.Run(
                owner =>
                {
                    root = owner.RootPath;
                    ThrowOwnedPrimary();
                },
                postCleanupFailure: () => throw new IOException("cleanup-secondary")));

        Assert.Equal("primary", exception.Message);
        Assert.Contains(nameof(ThrowOwnedPrimary), exception.StackTrace);
        Assert.Contains("cleanup-secondary", Assert.IsType<string>(exception.Data[TestOwnedOutputRoot.CleanupFailureDataKey]));
        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public async Task RunAsync_ParallelInvocations_UseDistinctDirectTempChildren()
    {
        var tempRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath()));
        var roots = await Task.WhenAll(Enumerable.Range(0, 12).Select(_ => Task.Run(() =>
            TestOwnedOutputRoot.Run(owner =>
            {
                Assert.Equal(tempRoot, Path.GetDirectoryName(owner.RootPath));
                return owner.RootPath;
            }))));

        Assert.Equal(roots.Length, roots.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(roots, root => Assert.False(Directory.Exists(root)));
    }

    [TestOwnedOutputRootWindowsFact]
    public void Cleanup_NestedTraversalLease_BlocksRenameBeforeEnumeration()
    {
        string? ownedRoot = null;
        string? nestedDirectory = null;
        string? replacementDirectory = null;
        var nestedLeaseHookRan = false;

        TestOwnedOutputRoot.Run(
            owner =>
            {
                ownedRoot = owner.RootPath;
                nestedDirectory = owner.RequireOwnedPath(Path.Combine(owner.RootPath, "nested"));
                replacementDirectory = owner.RequireOwnedPath(Path.Combine(owner.RootPath, "nested-replacement"));
                Directory.CreateDirectory(nestedDirectory);
                File.WriteAllText(
                    owner.RequireOwnedPath(Path.Combine(nestedDirectory, "payload.txt")),
                    "owned");
            },
            traversalLeaseAcquired: leasedDirectory =>
            {
                if (!string.Equals(leasedDirectory, nestedDirectory, StringComparison.OrdinalIgnoreCase))
                    return;

                nestedLeaseHookRan = true;
                var exception = Assert.Throws<IOException>(() =>
                    Directory.Move(nestedDirectory!, replacementDirectory!));
                var win32Code = exception.HResult & 0xFFFF;
                Assert.Contains(win32Code, new[] { 5, 32 });
            });

        Assert.True(nestedLeaseHookRan);
        Assert.False(Directory.Exists(replacementDirectory));
        Assert.False(Directory.Exists(ownedRoot));
    }

    [TestOwnedOutputRootUnixLinkFact]
    public void Cleanup_UnixDirectoryLink_RemovesOnlyLinkAndLeavesOutsideDirectory()
    {
        var outsideRoot = Path.Combine(Path.GetTempPath(), "llm-from-scratch-unix-outside-sentinel-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outsideRoot);
        var sentinel = Path.Combine(outsideRoot, "sentinel.txt");
        File.WriteAllText(sentinel, "must-survive");

        try
        {
            string? ownedRoot = null;
            TestOwnedOutputRoot.Run(owner =>
            {
                ownedRoot = owner.RootPath;
                var link = Path.Combine(owner.RootPath, "outside-link");

                // Русский комментарий: это активная Unix-проверка no-follow cleanup.
                // Если helper пойдёт по ссылке рекурсивно, sentinel снаружи исчезнет.
                Directory.CreateSymbolicLink(link, outsideRoot);
                Assert.True((File.GetAttributes(link) & FileAttributes.ReparsePoint) != 0);
                Assert.Throws<InvalidOperationException>(() =>
                    owner.RequireOwnedPath(Path.Combine(link, "sentinel.txt")));
            });

            Assert.False(Directory.Exists(ownedRoot));
            Assert.True(File.Exists(sentinel));
            Assert.Equal("must-survive", File.ReadAllText(sentinel));
        }
        finally
        {
            if (Directory.Exists(outsideRoot))
                Directory.Delete(outsideRoot, recursive: true);
        }
    }

    [Fact]
    public void RequireOwnedPath_RejectsRelativeParentRootEscapeAndRepositoryPaths()
    {
        TestOwnedOutputRoot.Run(owner =>
        {
            Assert.Throws<InvalidOperationException>(() => owner.RequireOwnedPath("relative.txt"));
            Assert.Throws<InvalidOperationException>(() => owner.RequireOwnedPath(Path.Combine(owner.RootPath, "child", "..", "escape.txt")));
            Assert.Throws<InvalidOperationException>(() => owner.RequireOwnedPath(Path.Combine(Path.GetDirectoryName(owner.RootPath)!, "other.txt")));
            Assert.Throws<InvalidOperationException>(() => owner.RequireOwnedPath(Path.Combine(owner.RepositoryRootPath, "Tests", "repo-output.txt")));
        });
    }

    [Fact]
    public void ToStableReference_MapsRepositoryAndOwnedPathsAndRejectsEverythingElse()
    {
        TestOwnedOutputRoot.Run(owner =>
        {
            var repoInput = Path.Combine(owner.RepositoryRootPath, "models", "fixture.json");
            var ownedOutput = owner.RequireOwnedPath(Path.Combine(owner.RootPath, "evidence", "eval.json"));

            Assert.Equal("models/fixture.json", owner.ToStableReference(repoInput));
            Assert.Equal("test-owned-output/evidence/eval.json", owner.ToStableReference(ownedOutput));
            Assert.DoesNotContain('\\', owner.ToStableReference(ownedOutput));
            Assert.Throws<InvalidOperationException>(() => owner.ToStableReference(Path.Combine(Path.GetDirectoryName(owner.RootPath)!, "foreign.txt")));
            Assert.Throws<InvalidOperationException>(() => owner.ToStableReference(Path.Combine(owner.RootPath, "evidence", "..", "escape.json")));
        });
    }

    [TestOwnedOutputRootReparsePointFact]
    public void Cleanup_ReparseChild_RemovesOnlyLinkAndLeavesOutsideSentinel()
    {
        var outsideRoot = Path.Combine(Path.GetTempPath(), "llm-from-scratch-outside-sentinel-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outsideRoot);
        var sentinel = Path.Combine(outsideRoot, "sentinel.txt");
        File.WriteAllText(sentinel, "must-survive");

        try
        {
            string? ownedRoot = null;
            TestOwnedOutputRoot.Run(owner =>
            {
                ownedRoot = owner.RootPath;
                var link = Path.Combine(owner.RootPath, "outside-link");
                Directory.CreateSymbolicLink(link, outsideRoot);
                Assert.Throws<InvalidOperationException>(() => owner.RequireOwnedPath(link));
            });

            Assert.False(Directory.Exists(ownedRoot));
            Assert.True(File.Exists(sentinel));
            Assert.Equal("must-survive", File.ReadAllText(sentinel));
        }
        finally
        {
            if (Directory.Exists(outsideRoot))
                Directory.Delete(outsideRoot, recursive: true);
        }
    }

    private static void ThrowOwnedPrimary()
        => throw new OwnedPrimaryException("primary");

    private sealed class OwnedPrimaryException(string message) : Exception(message);
}

file sealed class TestOwnedOutputRootReparsePointFactAttribute : FactAttribute
{
    public TestOwnedOutputRootReparsePointFactAttribute()
        => Skip = TestOwnedOutputRootReparsePointCapability.GetSkipReason();
}

file sealed class TestOwnedOutputRootWindowsFactAttribute : FactAttribute
{
    public TestOwnedOutputRootWindowsFactAttribute()
        => Skip = OperatingSystem.IsWindows()
            ? null
            : "Windows-only: this assertion proves the stronger no-delete-share handle blocks rename during traversal.";
}

file sealed class TestOwnedOutputRootUnixLinkFactAttribute : FactAttribute
{
    public TestOwnedOutputRootUnixLinkFactAttribute()
        => Skip = OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()
            ? TestOwnedOutputRootReparsePointCapability.GetSkipReason()
            : "Unix-only: this assertion proves no-follow cleanup removes a directory link without touching the external target.";
}

file static class TestOwnedOutputRootReparsePointCapability
{
    public static string? GetSkipReason()
    {
        var probe = Path.Combine(Path.GetTempPath(), "llm-from-scratch-reparse-probe-" + Guid.NewGuid().ToString("N"));
        var target = Path.Combine(probe, "target");
        var link = Path.Combine(probe, "link");
        try
        {
            Directory.CreateDirectory(target);
            try
            {
                Directory.CreateSymbolicLink(link, target);
                return null;
            }
            catch (Exception exception) when (IsCapabilityLimitation(exception))
            {
                return $"REPARSE_TEST_PLATFORM_LIMITATION: {exception.GetType().Name}, HResult=0x{exception.HResult:X8}: {exception.Message}";
            }
        }
        finally
        {
            if (Directory.Exists(link))
                Directory.Delete(link, recursive: false);
            if (Directory.Exists(probe))
                Directory.Delete(probe, recursive: true);
        }
    }

    private static bool IsCapabilityLimitation(Exception exception)
        => exception is NotSupportedException
            || (OperatingSystem.IsWindows()
                && exception is IOException or UnauthorizedAccessException
                && exception.HResult is unchecked((int)0x80070522) or unchecked((int)0x80070032) or unchecked((int)0x80070001));
}
