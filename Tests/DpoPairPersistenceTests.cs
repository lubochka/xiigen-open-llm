// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;

namespace LlmFromScratch.Tests;

public sealed class DpoPairPersistenceTests : IDisposable
{
    private const string ChildControlPrefix = "XIIGEN_DPO_PAIR_CHILD_";
    private const string ChildRootVariable = "XIIGEN_DPO_PAIR_CHILD_ROOT";
    private const string ChildMaximumVariable = "XIIGEN_DPO_PAIR_CHILD_MAXIMUM";
    private const string ChildPromptVariable = "XIIGEN_DPO_PAIR_CHILD_PROMPT";
    private const string ChildResultVariable = "XIIGEN_DPO_PAIR_CHILD_RESULT";
    private const string ChildLockMarkerVariable = "XIIGEN_DPO_PAIR_CHILD_LOCK_MARKER";
    private const string ChildModeVariable = "XIIGEN_DPO_PAIR_CHILD_MODE";
    private const string ChildNonceVariable = "XIIGEN_DPO_PAIR_CHILD_NONCE";
    private const string PollutedUnknownChildVariable =
        "XIIGEN_DPO_PAIR_CHILD_STALE_UNKNOWN";
    private const string DisableFileLockingVariable =
        "DOTNET_SYSTEM_IO_DISABLEFILELOCKING";
    private const string RouteOneChildMode = "route-one";
    private const string SameProcessGateChildMode = "same-process-gate";
    private static readonly string[] ChildControlVariables =
    [
        ChildRootVariable,
        ChildMaximumVariable,
        ChildPromptVariable,
        ChildResultVariable,
        ChildLockMarkerVariable,
        ChildModeVariable,
        ChildNonceVariable,
        PollutedUnknownChildVariable,
    ];
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        $"xiigen-dpo-pair-store-{Guid.NewGuid():N}");

    private sealed record ChildProbe(
        Process Process,
        string Nonce,
        string ResultPath,
        Task<string> StandardOutput,
        Task<string> StandardError);

    [Fact]
    public void EditedFeedback_IsDurableBeforeSuccess_AndFreshRouterLoadsExactPair()
    {
        var cache = new RecordingDpoPairCache();
        var first = BuildRouter(cache);
        var feedback = EditedFeedback();

        var routed = first.Route(feedback);

        Assert.True(routed.IsSuccess, $"{routed.ErrorCode}: {routed.ErrorMessage}");
        var recordPath = Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        Assert.Matches(
            "^[0-9a-f]{64}\\.dpo-pair\\.json$",
            Path.GetFileName(recordPath));

        var restarted = BuildRouter(new RecordingDpoPairCache());
        var loaded = restarted.GetDpoPairs();

        Assert.True(loaded.IsSuccess, $"{loaded.ErrorCode}: {loaded.ErrorMessage}");
        var pair = Assert.Single(loaded.Value!);
        Assert.Equal(feedback.Prompt, pair.Prompt);
        Assert.Equal(feedback.UserCorrection, pair.Chosen);
        Assert.Equal(feedback.ModelResponse, pair.Rejected);
        Assert.Equal(feedback.Domain, pair.Domain);
    }

    [Fact]
    public void DuplicateFeedback_AcrossRestart_IsIdempotentWithStableIdentity()
    {
        var first = BuildRouter(new RecordingDpoPairCache());
        var feedback = EditedFeedback();
        var firstResult = first.Route(feedback);
        var firstPath = Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));

        var restarted = BuildRouter(new RecordingDpoPairCache());
        var secondResult = restarted.Route(feedback);
        var secondPath = Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));

        Assert.True(firstResult.IsSuccess, firstResult.ErrorMessage);
        Assert.True(secondResult.IsSuccess, secondResult.ErrorMessage);
        Assert.Equal(Path.GetFileName(firstPath), Path.GetFileName(secondPath));
        Assert.Single(restarted.GetDpoPairs().Value!);
    }

    [Fact]
    public async Task ConcurrentDuplicateFeedback_PublishesOneCanonicalRecord()
    {
        var feedback = WithTrainingAdmission(new FeedbackEvent(
            "concurrent prompt",
            "concurrent rejected",
            null,
            FeedbackSignal.Rejected,
            "BUG"));

        var results = Enumerable.Range(0, 32)
            .Select(_ => Task.Run(
                () => BuildRouter(new RecordingDpoPairCache()).Route(feedback)))
            .ToArray();
        await Task.WhenAll(results);

        Assert.All(results, task => Assert.True(
            task.Result.IsSuccess,
            $"{task.Result.ErrorCode}: {task.Result.ErrorMessage}"));
        Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        Assert.Single(BuildRouter(new RecordingDpoPairCache()).GetDpoPairs().Value!);
        Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
        Assert.Equal(
            0,
            FileDpoPairStore.GetProcessGateReferenceCountForTesting(_root));
    }

    [Fact]
    public async Task ConcurrentInstance_WhenProcessGateIsHeld_FailsBusyBeforeStorageRead()
    {
        using var acquired = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        var holder = BuildRouter(
            new RecordingDpoPairCache(),
            afterMutationLockAcquiredForTesting: () =>
            {
                acquired.Set();
                release.Wait();
            });
        var holdingTask = Task.Run(
            () => holder.Route(EditedFeedback("lock-holder")));
        OperationResult<bool>? blocked = null;
        var waiterHookCalled = false;
        try
        {
            Assert.True(acquired.Wait(TimeSpan.FromSeconds(15)));
            var waiter = new FeedbackRouter(
                 new NullDpoPairSessionStorage(),
                 new RecordingDpoPairCache(),
                _root + Path.DirectorySeparatorChar,
                FileDpoPairStore.DefaultMaximumRecordCount,
                afterMutationLockAcquiredForTesting: () =>
                    waiterHookCalled = true,
                mutationLockTimeoutForTesting: TimeSpan.FromMilliseconds(100));
            var stopwatch = Stopwatch.StartNew();
            blocked = waiter.Route(EditedFeedback("lock-waiter"));
            stopwatch.Stop();
            Assert.InRange(
                stopwatch.Elapsed,
                TimeSpan.FromMilliseconds(50),
                TimeSpan.FromSeconds(2));
            Assert.False(waiterHookCalled);
            Assert.Empty(Directory.GetFiles(_root, "*.dpo-pair.json"));
            Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
            Assert.Equal(
                1,
                FileDpoPairStore.GetProcessGateReferenceCountForTesting(_root));
        }
        finally
        {
            release.Set();
        }

        var published = await holdingTask;
        Assert.NotNull(blocked);
        Assert.False(blocked.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_BUSY", blocked.ErrorCode);
        Assert.DoesNotContain(_root, blocked.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.True(published.IsSuccess, published.ErrorMessage);
        Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        Assert.Equal(
            0,
            FileDpoPairStore.GetProcessGateReferenceCountForTesting(_root));
    }

    [Fact]
    public void RegionLockContentionClassifier_RetriesOnlyProvenNativeCodes()
    {
        Assert.True(FileDpoPairStore.IsLockContention(
            new SyntheticDpoPairIOException(unchecked((int)0x80070020)),
            isWindows: true));
        Assert.True(FileDpoPairStore.IsLockContention(
            new SyntheticDpoPairIOException(unchecked((int)0x80070021)),
            isWindows: true));
        Assert.True(FileDpoPairStore.IsLockContention(
            new SyntheticDpoPairIOException(11),
            isWindows: false));
        Assert.True(FileDpoPairStore.IsLockContention(
            new SyntheticDpoPairIOException(13),
            isWindows: false));
        Assert.False(FileDpoPairStore.IsLockContention(
            new SyntheticDpoPairIOException(95),
            isWindows: false));
        Assert.False(FileDpoPairStore.IsLockContention(
            new IOException("generic lock failure"),
            isWindows: true));
    }

    [Fact]
    public void RegionLockUnavailable_FailsBeforeHookSnapshotOrPublication()
    {
        var hookCalled = false;
        var router = BuildRouter(
            new RecordingDpoPairCache(),
            afterMutationLockAcquiredForTesting: () => hookCalled = true,
            mutationRegionLockFailureForTesting: () =>
                new SyntheticDpoPairIOException(95));

        var result = router.Route(EditedFeedback("lock-unavailable"));

        Assert.False(result.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_UNAVAILABLE", result.ErrorCode);
        Assert.False(hookCalled);
        Assert.Empty(Directory.GetFiles(_root, "*.dpo-pair.json"));
        Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
        Assert.Equal(
            0,
            FileDpoPairStore.GetProcessGateReferenceCountForTesting(_root));
    }

    [LinuxFact]
    public void LinuxAccessDeniedRegionLockContention_RetriesToBusy()
    {
        if (!OperatingSystem.IsLinux())
            return;

        var hookCalled = false;
        var stopwatch = Stopwatch.StartNew();
        var result = BuildRouter(
                new RecordingDpoPairCache(),
                afterMutationLockAcquiredForTesting: () => hookCalled = true,
                mutationLockTimeoutForTesting: TimeSpan.FromMilliseconds(100),
                mutationRegionLockFailureForTesting: () =>
                    new SyntheticDpoPairIOException(13))
            .Route(EditedFeedback("linux-eacces-contention"));
        stopwatch.Stop();

        Assert.False(result.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_BUSY", result.ErrorCode);
        Assert.InRange(
            stopwatch.Elapsed,
            TimeSpan.FromMilliseconds(50),
            TimeSpan.FromSeconds(2));
        Assert.False(hookCalled);
        Assert.Empty(Directory.GetFiles(_root, "*.dpo-pair.json"));
        Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
        Assert.Equal(
            0,
            FileDpoPairStore.GetProcessGateReferenceCountForTesting(_root));
    }

    [Fact]
    public void RegionLockLength_IsExactlyOneBeforeAndAfterAcquisition()
    {
        var routed = BuildRouter(new RecordingDpoPairCache())
            .Route(EditedFeedback("exact-lock-length"));
        var lockPath = Path.Combine(_root, ".dpo-pair-store.lock");

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.Equal(1, new FileInfo(lockPath).Length);
        Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        Assert.Equal(
            0,
            FileDpoPairStore.GetProcessGateReferenceCountForTesting(_root));
    }

    [Fact]
    public void ExistingWrongRegionLockLength_FailsBeforeHookOrPublication()
    {
        CreatePrivateTestDirectory(_root);
        var lockPath = Path.Combine(_root, ".dpo-pair-store.lock");
        using (var stream = CreatePrivateTestFile(
                   lockPath,
                   FileMode.CreateNew,
                   FileAccess.Write,
                   FileShare.None))
        {
            stream.Write([0, 0]);
            stream.Flush(flushToDisk: true);
        }
        var hookCalled = false;

        var result = BuildRouter(
                new RecordingDpoPairCache(),
                afterMutationLockAcquiredForTesting: () => hookCalled = true)
            .Route(EditedFeedback("wrong-existing-lock-length"));

        Assert.False(result.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_UNAVAILABLE", result.ErrorCode);
        Assert.False(hookCalled);
        Assert.Equal(2, new FileInfo(lockPath).Length);
        Assert.Empty(Directory.GetFiles(_root, "*.dpo-pair.json"));
        Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
        Assert.Equal(
            0,
            FileDpoPairStore.GetProcessGateReferenceCountForTesting(_root));
    }

    [Fact]
    public void RegionLockLengthChangedAfterAcquisition_FailsBeforeHookOrPublication()
    {
        var hookCalled = false;
        var result = BuildRouter(
                new RecordingDpoPairCache(),
                afterMutationLockAcquiredForTesting: () => hookCalled = true,
                corruptMutationLockLengthAfterAcquisitionForTesting: true)
            .Route(EditedFeedback("post-lock-length-change"));
        var lockPath = Path.Combine(_root, ".dpo-pair-store.lock");

        Assert.False(result.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_UNAVAILABLE", result.ErrorCode);
        Assert.False(hookCalled);
        Assert.Equal(2, new FileInfo(lockPath).Length);
        Assert.Empty(Directory.GetFiles(_root, "*.dpo-pair.json"));
        Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
        Assert.Equal(
            0,
            FileDpoPairStore.GetProcessGateReferenceCountForTesting(_root));
    }

    [Fact]
    public void GetAll_WhenPlatformIsUnsupported_FailsBeforeMissingRootShortcut()
    {
        var result = BuildRouter(
                new RecordingDpoPairCache(),
                unsupportedPlatformForTesting: true)
            .GetDpoPairs();

        Assert.False(result.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_UNAVAILABLE", result.ErrorCode);
        Assert.False(Directory.Exists(_root));
        Assert.Equal(
            0,
            FileDpoPairStore.GetProcessGateReferenceCountForTesting(_root));
    }

    [Fact]
    public async Task DifferentStorageRoots_DoNotShareMutationLockIdentity()
    {
        using var acquired = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        var holder = BuildRouter(
            new RecordingDpoPairCache(),
            afterMutationLockAcquiredForTesting: () =>
            {
                acquired.Set();
                release.Wait();
            });
        var holdingTask = Task.Run(
            () => holder.Route(EditedFeedback("first-root-holder")));
        OperationResult<bool>? independent = null;
        var secondRoot = Path.Combine(_root, "independent-root");
        try
        {
            Assert.True(acquired.Wait(TimeSpan.FromSeconds(15)));
            var secondRouter = new FeedbackRouter(
                new NullDpoPairSessionStorage(),
                new RecordingDpoPairCache(),
                secondRoot,
                FileDpoPairStore.DefaultMaximumRecordCount,
                afterMutationLockAcquiredForTesting: null,
                mutationLockTimeoutForTesting: TimeSpan.FromMilliseconds(100));
            independent = secondRouter.Route(
                EditedFeedback("second-root-independent"));
        }
        finally
        {
            release.Set();
        }

        var published = await holdingTask;
        Assert.NotNull(independent);
        Assert.True(
            independent.IsSuccess,
            $"{independent.ErrorCode}: {independent.ErrorMessage}");
        Assert.True(published.IsSuccess, published.ErrorMessage);
        Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        Assert.Single(Directory.GetFiles(secondRoot, "*.dpo-pair.json"));
        Assert.Equal(
            0,
            FileDpoPairStore.GetProcessGateReferenceCountForTesting(_root));
        Assert.Equal(
            0,
            FileDpoPairStore.GetProcessGateReferenceCountForTesting(secondRoot));
    }

    [Fact]
    public async Task WindowsCaseVariantOfSameRoot_UsesSameMutationLock()
    {
        if (!OperatingSystem.IsWindows())
            return;

        CreatePrivateTestDirectory(_root);
        var caseVariantRoot = Path.Combine(
            Path.GetDirectoryName(_root)!,
            ToggleAsciiCase(Path.GetFileName(_root)));
        Assert.False(string.Equals(
            _root,
            caseVariantRoot,
            StringComparison.Ordinal));

        using var acquired = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        var holder = BuildRouter(
            new RecordingDpoPairCache(),
            afterMutationLockAcquiredForTesting: () =>
            {
                acquired.Set();
                release.Wait();
            });
        var holdingTask = Task.Run(
            () => holder.Route(EditedFeedback("case-holder")));
        OperationResult<bool>? blocked = null;
        try
        {
            Assert.True(acquired.Wait(TimeSpan.FromSeconds(15)));
            var waiter = new FeedbackRouter(
                new NullDpoPairSessionStorage(),
                new RecordingDpoPairCache(),
                caseVariantRoot,
                FileDpoPairStore.DefaultMaximumRecordCount,
                afterMutationLockAcquiredForTesting: null,
                mutationLockTimeoutForTesting: TimeSpan.FromMilliseconds(100));
            blocked = waiter.Route(EditedFeedback("case-waiter"));
        }
        finally
        {
            release.Set();
        }

        var published = await holdingTask;
        Assert.NotNull(blocked);
        Assert.False(blocked.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_BUSY", blocked.ErrorCode);
        Assert.True(published.IsSuccess, published.ErrorMessage);
        Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
    }

    [Fact]
    public void CorruptCanonicalRecord_FailsReadAndFutureWriteWithoutSensitiveLeakage()
    {
        var router = BuildRouter(new RecordingDpoPairCache());
        var feedback = EditedFeedback();
        Assert.True(router.Route(feedback).IsSuccess);
        var recordPath = Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        File.WriteAllText(recordPath, "{not-json");

        var read = BuildRouter(new RecordingDpoPairCache()).GetDpoPairs();
        var write = BuildRouter(new RecordingDpoPairCache()).Route(
            WithTrainingAdmission(new FeedbackEvent(
            "second private prompt",
            "second private rejected",
            null,
            FeedbackSignal.Rejected,
            "BUG")));

        Assert.False(read.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CORRUPT", read.ErrorCode);
        AssertNonLeaking(read.ErrorMessage, feedback);
        Assert.False(write.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CORRUPT", write.ErrorCode);
        Assert.DoesNotContain("second private prompt", write.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(_root, write.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
    }

    [Fact]
    public void ExistingIdentityWithTamperedPayload_FailsClosedAsCorruptionBeforeCollisionResolution()
    {
        var router = BuildRouter(new RecordingDpoPairCache());
        var feedback = EditedFeedback();
        Assert.True(router.Route(feedback).IsSuccess);
        var recordPath = Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        var json = File.ReadAllText(recordPath);
        Assert.Contains("\"chosen\":\"private corrected response\"", json, StringComparison.Ordinal);
        File.WriteAllText(
            recordPath,
            json.Replace(
                "\"chosen\":\"private corrected response\"",
                "\"chosen\":\"tampered private response\"",
                StringComparison.Ordinal));

        var collision = BuildRouter(new RecordingDpoPairCache()).Route(feedback);
        var corruptRead = BuildRouter(new RecordingDpoPairCache()).GetDpoPairs();

        Assert.False(collision.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CORRUPT", collision.ErrorCode);
        AssertNonLeaking(collision.ErrorMessage, feedback);
        Assert.False(corruptRead.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CORRUPT", corruptRead.ErrorCode);
    }

    [Fact]
    public void UnknownProperty_FailsExactSchema()
    {
        var path = PersistOneCanonicalRecord();
        var json = File.ReadAllText(path);
        File.WriteAllText(path, json[..^1] + ",\"unexpected\":true}");

        AssertCorruptStore();
    }

    [Fact]
    public void DuplicateProperty_FailsExactSchema()
    {
        var path = PersistOneCanonicalRecord();
        var json = File.ReadAllText(path);
        File.WriteAllText(path, "{\"prompt\":\"duplicate\"," + json[1..]);

        AssertCorruptStore();
    }

    [Theory]
    [InlineData("\"schemaVersion\":5", "\"schemaVersion\":\"5\"")]
    [InlineData("\"schemaVersion\":5", "\"schemaVersion\":6")]
    public void WrongSchemaTypeOrVersion_FailsClosed(
        string original,
        string replacement)
    {
        var path = PersistOneCanonicalRecord();
        var json = File.ReadAllText(path);
        Assert.Contains(original, json, StringComparison.Ordinal);
        File.WriteAllText(
            path,
            json.Replace(original, replacement, StringComparison.Ordinal));

        AssertCorruptStore();
    }

    [Fact]
    public void NonCanonicalPairId_FailsClosed()
    {
        var path = PersistOneCanonicalRecord();
        var pairId = Path.GetFileName(path)[..64];
        var json = File.ReadAllText(path);
        File.WriteAllText(
            path,
            json.Replace(pairId, pairId.ToUpperInvariant(), StringComparison.Ordinal));

        AssertCorruptStore();
    }

    [Fact]
    public void CanonicalRecordRenamedToWrongHash_FailsClosed()
    {
        var path = PersistOneCanonicalRecord();
        var wrongPath = Path.Combine(
            _root,
            new string('b', 64) + ".dpo-pair.json");
        File.Move(path, wrongPath);

        AssertCorruptStore();
    }

    [Fact]
    public void SymlinkCapabilityClassifier_SkipsOnlyKnownPlatformLimitations()
    {
        Assert.True(DpoPairSymlinkTestSupport.IsRecognizedCapabilityLimitation(
            new NotSupportedException(),
            isWindows: false));
        Assert.True(DpoPairSymlinkTestSupport.IsRecognizedCapabilityLimitation(
            new SyntheticDpoPairIOException(
                DpoPairSymlinkTestSupport.HResultPrivilegeNotHeld),
            isWindows: true));
        Assert.True(DpoPairSymlinkTestSupport.IsRecognizedCapabilityLimitation(
            new SyntheticDpoPairUnauthorizedAccessException(
                DpoPairSymlinkTestSupport.HResultOperationNotSupported),
            isWindows: true));
        Assert.True(DpoPairSymlinkTestSupport.IsRecognizedCapabilityLimitation(
            new SyntheticDpoPairIOException(
                DpoPairSymlinkTestSupport.HResultInvalidFunction),
            isWindows: true));
        Assert.False(DpoPairSymlinkTestSupport.IsRecognizedCapabilityLimitation(
            new IOException("unexpected"),
            isWindows: true));
        Assert.False(DpoPairSymlinkTestSupport.IsRecognizedCapabilityLimitation(
            new UnauthorizedAccessException("unexpected"),
            isWindows: true));
        Assert.False(DpoPairSymlinkTestSupport.IsRecognizedCapabilityLimitation(
            new SyntheticDpoPairIOException(
                DpoPairSymlinkTestSupport.HResultPrivilegeNotHeld),
            isWindows: false));
    }

    [SymlinkFact]
    public void RootDirectorySymlink_IsRejected()
    {
        CreatePrivateTestDirectory(_root);
        var target = Path.Combine(_root, "target-root");
        var link = Path.Combine(_root, "linked-root");
        CreatePrivateTestDirectory(target);
        CreateDirectorySymlinkRequired(link, target);

        try
        {
            var result = new FeedbackRouter(
                new NullDpoPairSessionStorage(),
                new RecordingDpoPairCache(),
                link).Route(EditedFeedback());

            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_PAIR_STORE_UNAVAILABLE", result.ErrorCode);
        }
        finally
        {
            DeleteDirectoryLink(link);
        }
    }

    [SymlinkFact]
    public void AncestorDirectorySymlink_IsRejected()
    {
        CreatePrivateTestDirectory(_root);
        var target = Path.Combine(_root, "ancestor-target");
        var link = Path.Combine(_root, "ancestor-link");
        CreatePrivateTestDirectory(target);
        CreateDirectorySymlinkRequired(link, target);

        try
        {
            var result = new FeedbackRouter(
                new NullDpoPairSessionStorage(),
                new RecordingDpoPairCache(),
                Path.Combine(link, "pair-store")).Route(EditedFeedback());

            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_PAIR_STORE_UNAVAILABLE", result.ErrorCode);
            Assert.False(Directory.Exists(Path.Combine(target, "pair-store")));
        }
        finally
        {
            DeleteDirectoryLink(link);
        }
    }

    [SymlinkFact]
    public void CanonicalRecordSymlink_IsRejected()
    {
        var path = PersistOneCanonicalRecord();
        var target = Path.Combine(_root, "record-target.json");
        File.Move(path, target);
        CreateFileSymlinkRequired(path, target);

        try
        {
            AssertCorruptStore();
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [SymlinkFact]
    public void MutationLockFileSymlink_IsRejectedBeforePublication()
    {
        CreatePrivateTestDirectory(_root);
        var target = Path.Combine(_root, "lock-target");
        var lockPath = Path.Combine(_root, ".dpo-pair-store.lock");
        File.WriteAllText(target, "not a lock");
        CreateFileSymlinkRequired(lockPath, target);

        try
        {
            var result = BuildRouter(new RecordingDpoPairCache())
                .Route(EditedFeedback());

            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_PAIR_STORE_UNAVAILABLE", result.ErrorCode);
            Assert.Empty(Directory.GetFiles(_root, "*.dpo-pair.json"));
        }
        finally
        {
            if (File.Exists(lockPath))
                File.Delete(lockPath);
        }
    }

    [LinuxFact]
    public void LinuxCreatedRootLockAndCanonical_AreOwnerOnlyAndReadable()
    {
        if (!OperatingSystem.IsLinux())
            return;

        var routed = BuildRouter(new RecordingDpoPairCache())
            .Route(EditedFeedback("unix-private-mode"));
        var canonical = Assert.Single(
            Directory.GetFiles(_root, "*.dpo-pair.json"));
        var lockPath = Path.Combine(_root, ".dpo-pair-store.lock");
        var loaded = BuildRouter(new RecordingDpoPairCache()).GetDpoPairs();

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Single(loaded.Value!);
        Assert.Equal(
            UnixFileMode.UserRead
            | UnixFileMode.UserWrite
            | UnixFileMode.UserExecute,
            File.GetUnixFileMode(_root));
        Assert.Equal(
            UnixFileMode.UserRead | UnixFileMode.UserWrite,
            File.GetUnixFileMode(lockPath));
        Assert.Equal(
            UnixFileMode.UserRead | UnixFileMode.UserWrite,
            File.GetUnixFileMode(canonical));
    }

    [LinuxFact]
    public void LinuxUnsafeExistingRoot_FailsClosedWithoutWideningOrPublication()
    {
        if (!OperatingSystem.IsLinux())
            return;

        CreatePrivateTestDirectory(_root);
        var unsafeMode = UnixFileMode.UserRead
            | UnixFileMode.UserWrite
            | UnixFileMode.UserExecute
            | UnixFileMode.GroupRead;
        File.SetUnixFileMode(_root, unsafeMode);

        var result = BuildRouter(new RecordingDpoPairCache())
            .Route(EditedFeedback("unsafe-root"));

        Assert.False(result.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_UNAVAILABLE", result.ErrorCode);
        Assert.Equal(unsafeMode, File.GetUnixFileMode(_root));
        Assert.False(File.Exists(
            Path.Combine(_root, ".dpo-pair-store.lock")));
        Assert.Empty(Directory.GetFiles(_root, "*.dpo-pair.json"));
        Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
    }

    [LinuxFact]
    public void LinuxUnsafeExistingLock_FailsClosedWithoutModeChangeOrPublication()
    {
        if (!OperatingSystem.IsLinux())
            return;

        CreatePrivateTestDirectory(_root);
        File.SetUnixFileMode(
            _root,
            UnixFileMode.UserRead
            | UnixFileMode.UserWrite
            | UnixFileMode.UserExecute);
        var lockPath = Path.Combine(_root, ".dpo-pair-store.lock");
        File.WriteAllBytes(lockPath, [0]);
        var unsafeMode = UnixFileMode.UserRead
            | UnixFileMode.UserWrite
            | UnixFileMode.GroupWrite;
        File.SetUnixFileMode(lockPath, unsafeMode);

        var result = BuildRouter(new RecordingDpoPairCache())
            .Route(EditedFeedback("unsafe-lock"));

        Assert.False(result.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_UNAVAILABLE", result.ErrorCode);
        Assert.Equal(unsafeMode, File.GetUnixFileMode(lockPath));
        Assert.Empty(Directory.GetFiles(_root, "*.dpo-pair.json"));
        Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
    }

    [LinuxFact]
    public void LinuxUnsafeCanonicalMode_FailsClosedWithoutWidening()
    {
        if (!OperatingSystem.IsLinux())
            return;

        var canonical = PersistOneCanonicalRecord();
        var unsafeMode = UnixFileMode.UserRead
            | UnixFileMode.UserWrite
            | UnixFileMode.GroupRead;
        File.SetUnixFileMode(canonical, unsafeMode);

        var loaded = BuildRouter(new RecordingDpoPairCache()).GetDpoPairs();

        Assert.False(loaded.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CORRUPT", loaded.ErrorCode);
        Assert.Equal(unsafeMode, File.GetUnixFileMode(canonical));
    }

    [Fact]
    public void ZeroLengthCanonicalRecord_FailsClosed()
    {
        CreatePrivateTestDirectory(_root);
        using (CreatePrivateTestFile(
                   Path.Combine(
                       _root,
                       new string('a', 64) + ".dpo-pair.json"),
                   FileMode.CreateNew,
                   FileAccess.Write,
                   FileShare.None))
        {
        }

        AssertCorruptStore();
    }

    [Fact]
    public void OversizedCanonicalRecord_FailsClosedWithoutUnboundedRead()
    {
        CreatePrivateTestDirectory(_root);
        var path = Path.Combine(
            _root,
            new string('a', 64) + ".dpo-pair.json");
        using (var stream = CreatePrivateTestFile(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None))
        {
            stream.SetLength((16L * 1024L * 1024L) + 1L);
        }

        AssertCorruptStore();
    }

    [Fact]
    public void ExactCapacity_NewIdentityIsRejectedWithoutPublication_AndStoreRemainsReadable()
    {
        const int maximumRecordCount = 2;
        var router = BuildRouter(
            new RecordingDpoPairCache(),
            maximumRecordCount);
        foreach (var prompt in new[] { "first-at-cap", "second-at-cap" })
        {
            Assert.True(router.Route(EditedFeedback(prompt)).IsSuccess);
        }

        var rejected = router.Route(EditedFeedback("new-beyond-cap"));
        var loaded = router.GetDpoPairs();

        Assert.False(rejected.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CAPACITY", rejected.ErrorCode);
        Assert.DoesNotContain(_root, rejected.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Equal(maximumRecordCount, loaded.Value!.Count);
        Assert.Equal(
            maximumRecordCount,
            Directory.GetFiles(_root, "*.dpo-pair.json").Length);
        Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
    }

    [Fact]
    public void ExactCapacity_DuplicateRetryRemainsIdempotent()
    {
        const int maximumRecordCount = 2;
        var router = BuildRouter(
            new RecordingDpoPairCache(),
            maximumRecordCount);
        var duplicate = EditedFeedback("duplicate-at-cap");
        Assert.True(router.Route(duplicate).IsSuccess);
        Assert.True(router.Route(EditedFeedback("other-at-cap")).IsSuccess);

        var retried = router.Route(duplicate);

        Assert.True(retried.IsSuccess, $"{retried.ErrorCode}: {retried.ErrorMessage}");
        Assert.Equal(
            maximumRecordCount,
            Directory.GetFiles(_root, "*.dpo-pair.json").Length);
        Assert.Equal(maximumRecordCount, router.GetDpoPairs().Value!.Count);
    }

    [Theory]
    [InlineData("prompt")]
    [InlineData("chosen")]
    [InlineData("rejected")]
    [InlineData("domain")]
    public void EveryOversizedField_IsRejectedBeforePublication(string field)
    {
        var oversized = new string('x', (4 * 1024 * 1024) + 1);
        var feedback = new FeedbackEvent(
            field == "prompt" ? oversized : "prompt",
            field == "rejected" ? oversized : "rejected",
            field == "chosen" ? oversized : "chosen",
            FeedbackSignal.Edited,
            field == "domain" ? oversized : "BUG");

        var result = BuildRouter(new RecordingDpoPairCache()).Route(feedback);

        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_DPO_PAIR", result.ErrorCode);
        Assert.False(Directory.Exists(_root));
    }

    [Fact]
    public void AggregateFieldLimit_IsRejectedBeforePublication()
    {
        var large = new string('x', (4 * 1024 * 1024) - 512);
        var feedback = new FeedbackEvent(
            large,
            large,
            large,
            FeedbackSignal.Edited,
            large);

        var result = BuildRouter(new RecordingDpoPairCache()).Route(feedback);

        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_DPO_PAIR", result.ErrorCode);
        Assert.False(Directory.Exists(_root));
    }

    [Fact]
    public void ValidBoundedPairWithoutProvenance_FailsBeforePublication()
    {
        var result = BuildRouter(new RecordingDpoPairCache()).Route(
            new FeedbackEvent(
                "prompt",
                "rejected",
                "chosen",
                FeedbackSignal.Edited,
                "BUG"));

        Assert.False(result.IsSuccess);
        Assert.Equal("LEGAL_TEACHER_PROVENANCE_REQUIRED", result.ErrorCode);
        Assert.False(Directory.Exists(_root));
    }

    [Fact]
    public void ValidBoundedPairWithoutGovernance_FailsBeforePublication()
    {
        var result = BuildRouter(new RecordingDpoPairCache()).Route(
            new FeedbackEvent(
                "prompt",
                "rejected",
                "chosen",
                FeedbackSignal.Edited,
                "BUG")
            {
                TeacherProvenance = TrainingTeacherProvenance.Deterministic(
                    "dpo-pair-persistence-missing-governance"),
            });

        Assert.False(result.IsSuccess);
        Assert.Equal("TRAINING_DATA_GOVERNANCE_REQUIRED", result.ErrorCode);
        Assert.False(Directory.Exists(_root));
    }

    [Fact]
    public void JsonEscapeExpansionBeyondRecordLimit_IsRejectedAndTemporaryFileIsRemoved()
    {
        var escapeHeavy = new string('\u0001', 700_000);
        var result = BuildRouter(new RecordingDpoPairCache()).Route(
            new FeedbackEvent(
                escapeHeavy,
                escapeHeavy,
                escapeHeavy,
                FeedbackSignal.Edited,
                escapeHeavy));

        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_DPO_PAIR", result.ErrorCode);
        Assert.False(Directory.Exists(_root));
    }

    [Fact]
    public void MultiplePairs_LoadInStableCanonicalIdentityOrder()
    {
        var router = BuildRouter(new RecordingDpoPairCache());
        foreach (var prompt in new[] { "third", "first", "second" })
        {
            Assert.True(router.Route(WithTrainingAdmission(new FeedbackEvent(
                prompt,
                $"rejected-{prompt}",
                $"chosen-{prompt}",
                FeedbackSignal.Edited,
                "BUG"))).IsSuccess);
        }

        var expectedPrompts = Directory
            .GetFiles(_root, "*.dpo-pair.json")
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(ReadPrompt)
            .ToArray();
        var restarted = BuildRouter(new RecordingDpoPairCache()).GetDpoPairs();

        Assert.True(restarted.IsSuccess, restarted.ErrorMessage);
        Assert.Equal(expectedPrompts, restarted.Value!.Select(pair => pair.Prompt));
    }

    [Fact]
    public void InterruptedTemporaryFile_IsIgnored_AndCanonicalPublicationStillWorks()
    {
        CreatePrivateTestDirectory(_root);
        var temporaryPath = Path.Combine(_root, ".interrupted.tmp");
        File.WriteAllText(temporaryPath, "{partial");
        var router = BuildRouter(new RecordingDpoPairCache());

        var before = router.GetDpoPairs();
        var routed = router.Route(EditedFeedback());
        var after = router.GetDpoPairs();

        Assert.True(before.IsSuccess, before.ErrorMessage);
        Assert.Empty(before.Value!);
        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.True(after.IsSuccess, after.ErrorMessage);
        Assert.Single(after.Value!);
        Assert.True(File.Exists(temporaryPath));
        Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
    }

    [Fact]
    public async Task SeparateProcesses_ConcurrentDuplicatePublication_IsIdempotent()
    {
        var dotnetHost = FindDotnetHost();
        var repositoryRoot = FindRepositoryRoot();
        var projectPath = Path.Combine(
            repositoryRoot,
            "Tests",
            "LlmFromScratch.Tests.csproj");
        var children = Enumerable.Range(0, 3)
            .Select(_ => StartChildProbe(dotnetHost, repositoryRoot, projectPath))
            .ToArray();
        string[] outcomes;

        try
        {
            outcomes = await Task.WhenAll(children.Select(WaitForChildProbe));
        }
        finally
        {
            foreach (var child in children)
                await CleanupChildProbeAsync(child);
        }

        Assert.All(outcomes, outcome => Assert.Equal("SUCCESS", outcome));
        Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        Assert.Single(BuildRouter(new RecordingDpoPairCache()).GetDpoPairs().Value!);
        Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
    }

    [Fact]
    public async Task SeparateProcesses_AtCapacityMinusOne_OnePublishesAndOneFailsCapacity()
    {
        const int maximumRecordCount = 2;
        var seeded = BuildRouter(
            new RecordingDpoPairCache(),
            maximumRecordCount);
        Assert.True(seeded.Route(EditedFeedback("capacity-seed")).IsSuccess);

        var dotnetHost = FindDotnetHost();
        var repositoryRoot = FindRepositoryRoot();
        var projectPath = Path.Combine(
            repositoryRoot,
            "Tests",
            "LlmFromScratch.Tests.csproj");
        var first = StartChildProbe(
            dotnetHost,
            repositoryRoot,
            projectPath,
            prompt: "capacity-first",
            maximumRecordCount: maximumRecordCount);
        var second = StartChildProbe(
            dotnetHost,
            repositoryRoot,
            projectPath,
            prompt: "capacity-second",
            maximumRecordCount: maximumRecordCount);
        string[] outcomes;

        try
        {
            outcomes = await Task.WhenAll(
                WaitForChildProbe(first),
                WaitForChildProbe(second));
        }
        finally
        {
            foreach (var child in new[] { first, second })
                await CleanupChildProbeAsync(child);
        }

        Assert.Single(outcomes, outcome => outcome == "SUCCESS");
        Assert.Single(
            outcomes,
            outcome => outcome == "DPO_PAIR_STORE_CAPACITY");
        Assert.Equal(
            maximumRecordCount,
            Directory.GetFiles(_root, "*.dpo-pair.json").Length);
        Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
        var loaded = seeded.GetDpoPairs();
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Equal(maximumRecordCount, loaded.Value!.Count);
    }

    [LinuxFact]
    public async Task LinuxDisabledImplicitFileLocking_NeverExceedsCapacity()
    {
        if (!OperatingSystem.IsLinux())
            return;

        const int maximumRecordCount = 2;
        var seeded = BuildRouter(
            new RecordingDpoPairCache(),
            maximumRecordCount);
        Assert.True(seeded.Route(EditedFeedback("linux-lock-seed")).IsSuccess);
        var dotnetHost = FindDotnetHost();
        var repositoryRoot = FindRepositoryRoot();
        var projectPath = Path.Combine(
            repositoryRoot,
            "Tests",
            "LlmFromScratch.Tests.csproj");
        var first = StartChildProbe(
            dotnetHost,
            repositoryRoot,
            projectPath,
            prompt: "linux-lock-first",
            maximumRecordCount: maximumRecordCount,
            disableImplicitFileLocking: true);
        var second = StartChildProbe(
            dotnetHost,
            repositoryRoot,
            projectPath,
            prompt: "linux-lock-second",
            maximumRecordCount: maximumRecordCount,
            disableImplicitFileLocking: true);
        string[] outcomes;

        try
        {
            outcomes = await Task.WhenAll(
                WaitForChildProbe(first),
                WaitForChildProbe(second));
        }
        finally
        {
            foreach (var child in new[] { first, second })
                await CleanupChildProbeAsync(child);
        }

        Assert.All(
            outcomes,
            outcome => Assert.Contains(
                outcome,
                new[]
                {
                    "SUCCESS",
                    "DPO_PAIR_STORE_CAPACITY",
                    "DPO_PAIR_STORE_UNAVAILABLE",
                }));
        Assert.InRange(
            outcomes.Count(outcome => outcome == "SUCCESS"),
            0,
            1);
        var canonicalCount =
            Directory.GetFiles(_root, "*.dpo-pair.json").Length;
        Assert.InRange(canonicalCount, 1, maximumRecordCount);
        Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
        var loaded = seeded.GetDpoPairs();
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Equal(canonicalCount, loaded.Value!.Count);
    }

    [LinuxFact]
    public async Task LinuxDisabledImplicitFileLocking_SameProcessInstancesUseProcessGate()
    {
        if (!OperatingSystem.IsLinux())
            return;

        var dotnetHost = FindDotnetHost();
        var repositoryRoot = FindRepositoryRoot();
        var projectPath = Path.Combine(
            repositoryRoot,
            "Tests",
            "LlmFromScratch.Tests.csproj");
        var child = StartChildProbe(
            dotnetHost,
            repositoryRoot,
            projectPath,
            maximumRecordCount: 2,
            disableImplicitFileLocking: true,
            childMode: SameProcessGateChildMode);
        string outcome;

        try
        {
            outcome = await WaitForChildProbe(child);
        }
        finally
        {
            await CleanupChildProbeAsync(child);
        }

        Assert.Equal("SUCCESS", outcome);
        Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
        var loaded = BuildRouter(
            new RecordingDpoPairCache(),
            maximumRecordCount: 2).GetDpoPairs();
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Single(loaded.Value!);
    }

    [Fact]
    public async Task CrashedPublisher_ReleasesMutationLock_AndNextPublisherRecovers()
    {
        CreatePrivateTestDirectory(_root);
        var dotnetHost = FindDotnetHost();
        var repositoryRoot = FindRepositoryRoot();
        var projectPath = Path.Combine(
            repositoryRoot,
            "Tests",
            "LlmFromScratch.Tests.csproj");
        var lockMarker = Path.Combine(_root, ".child-lock-acquired");
        var child = StartChildProbe(
            dotnetHost,
            repositoryRoot,
            projectPath,
            prompt: "crashed-publisher",
            maximumRecordCount: 2,
            lockMarker: lockMarker);

        try
        {
            await WaitForPath(lockMarker, child);
            child.Process.Kill(entireProcessTree: true);
        }
        finally
        {
            await CleanupChildProbeAsync(child);
        }

        File.Delete(lockMarker);
        var router = BuildRouter(new RecordingDpoPairCache(), maximumRecordCount: 2);
        var recovered = router.Route(EditedFeedback("after-crash"));

        Assert.True(
            recovered.IsSuccess,
            $"{recovered.ErrorCode}: {recovered.ErrorMessage}");
        Assert.Single(router.GetDpoPairs().Value!);
        Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
    }

    [Fact]
    public async Task ChildProbe_ClearsPollutedInheritedControls_AndEchoesFreshNonce()
    {
        var dotnetHost = FindDotnetHost();
        var repositoryRoot = FindRepositoryRoot();
        var projectPath = Path.Combine(
            repositoryRoot,
            "Tests",
            "LlmFromScratch.Tests.csproj");
        var ambientValues = ChildControlVariables.ToDictionary(
            variable => variable,
            Environment.GetEnvironmentVariable,
            StringComparer.Ordinal);
        ChildProbe? child = null;
        try
        {
            foreach (var variable in ChildControlVariables)
                Environment.SetEnvironmentVariable(variable, $"polluted-{variable}");

            child = StartChildProbe(
                dotnetHost,
                repositoryRoot,
                projectPath,
                prompt: "isolated-child");
            var outcome = await WaitForChildProbe(child);

            Assert.Equal("SUCCESS", outcome);
            Assert.All(
                ChildControlVariables,
                variable => Assert.Equal(
                    $"polluted-{variable}",
                    Environment.GetEnvironmentVariable(variable)));
        }
        finally
        {
            if (child is not null)
                await CleanupChildProbeAsync(child);
            foreach (var entry in ambientValues)
                Environment.SetEnvironmentVariable(entry.Key, entry.Value);
        }

        Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
    }

    [Fact]
    public void SeparateProcessChildProbe_RoutesOnePair()
    {
        var childRoot = Environment.GetEnvironmentVariable(
            ChildRootVariable);
        if (string.IsNullOrWhiteSpace(childRoot))
            return;

        var mode = Environment.GetEnvironmentVariable(ChildModeVariable);
        var nonce = Environment.GetEnvironmentVariable(ChildNonceVariable);
        Assert.True(Guid.TryParseExact(nonce, "N", out _));
        Assert.Null(Environment.GetEnvironmentVariable(
            PollutedUnknownChildVariable));
        var maximumRecordCountText = Environment.GetEnvironmentVariable(
            ChildMaximumVariable);
        var maximumRecordCount = string.IsNullOrWhiteSpace(maximumRecordCountText)
            ? FileDpoPairStore.DefaultMaximumRecordCount
            : int.Parse(maximumRecordCountText);
        if (StringComparer.Ordinal.Equals(mode, SameProcessGateChildMode))
        {
            RunSameProcessGateChildScenario(
                childRoot,
                maximumRecordCount);
            WriteChildResult(nonce!, "SUCCESS");
            return;
        }

        Assert.Equal(RouteOneChildMode, mode);
        var lockMarker = Environment.GetEnvironmentVariable(
            ChildLockMarkerVariable);
        Action? afterLock = string.IsNullOrWhiteSpace(lockMarker)
            ? null
            : () =>
            {
                File.WriteAllText(lockMarker, nonce!);
                Thread.Sleep(Timeout.Infinite);
            };
        var router = new FeedbackRouter(
            new NullDpoPairSessionStorage(),
            new RecordingDpoPairCache(),
            childRoot,
            maximumRecordCount,
            afterLock);
        var prompt = Environment.GetEnvironmentVariable(
            ChildPromptVariable);
        var result = router.Route(
            EditedFeedback(
                string.IsNullOrWhiteSpace(prompt)
                    ? "private prompt"
                    : prompt));
        WriteChildResult(
            nonce!,
            result.IsSuccess ? "SUCCESS" : result.ErrorCode);
    }

    private static void RunSameProcessGateChildScenario(
        string childRoot,
        int maximumRecordCount)
    {
        using var acquired = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        var first = new FeedbackRouter(
            new NullDpoPairSessionStorage(),
            new RecordingDpoPairCache(),
            childRoot,
            maximumRecordCount,
            afterMutationLockAcquiredForTesting: () =>
            {
                acquired.Set();
                release.Wait();
            });
        var firstTask = Task.Run(
            () => first.Route(EditedFeedback("child-process-gate-holder")));
        OperationResult<bool>? secondResult = null;
        var secondHookCalled = false;
        try
        {
            Assert.True(acquired.Wait(TimeSpan.FromSeconds(15)));
            var second = new FeedbackRouter(
                new NullDpoPairSessionStorage(),
                new RecordingDpoPairCache(),
                childRoot,
                maximumRecordCount,
                afterMutationLockAcquiredForTesting: () =>
                    secondHookCalled = true,
                mutationLockTimeoutForTesting: TimeSpan.FromMilliseconds(100));
            secondResult = second.Route(
                EditedFeedback("child-process-gate-waiter"));

            Assert.False(secondResult.IsSuccess);
            Assert.Equal("DPO_PAIR_STORE_BUSY", secondResult.ErrorCode);
            Assert.False(secondHookCalled);
            Assert.Empty(Directory.GetFiles(childRoot, "*.dpo-pair.json"));
            Assert.Empty(Directory.GetFiles(childRoot, "*.tmp"));
            Assert.Equal(
                1,
                FileDpoPairStore.GetProcessGateReferenceCountForTesting(
                    childRoot));
        }
        finally
        {
            release.Set();
        }

        var firstResult = firstTask.GetAwaiter().GetResult();
        Assert.True(firstResult.IsSuccess, firstResult.ErrorMessage);
        Assert.Single(Directory.GetFiles(childRoot, "*.dpo-pair.json"));
        Assert.Empty(Directory.GetFiles(childRoot, "*.tmp"));
        var loaded = new FeedbackRouter(
            new NullDpoPairSessionStorage(),
            new RecordingDpoPairCache(),
            childRoot,
            maximumRecordCount).GetDpoPairs();
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Single(loaded.Value!);
        Assert.Equal(
            0,
            FileDpoPairStore.GetProcessGateReferenceCountForTesting(childRoot));
    }

    private static void WriteChildResult(string nonce, string outcome)
    {
        var resultPath = Environment.GetEnvironmentVariable(
            ChildResultVariable);
        Assert.False(string.IsNullOrWhiteSpace(resultPath));
        File.WriteAllText(resultPath, $"{nonce}|{outcome}");
    }

    [Fact]
    public void ObsoleteConstructor_AcceptedFeedbackStillUsesPositiveCache()
    {
        var cache = new RecordingDpoPairCache();
#pragma warning disable CS0618
        var router = new FeedbackRouter(new NullDpoPairSessionStorage(), cache);
#pragma warning restore CS0618

        var result = router.Route(new FeedbackEvent(
            "prompt",
            "accepted",
            null,
            FeedbackSignal.Accepted,
            "BUG"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void ObsoleteConstructor_PairOperationsFailClosedWithoutMemoryFallback()
    {
        var cache = new RecordingDpoPairCache();
#pragma warning disable CS0618
        var router = new FeedbackRouter(new NullDpoPairSessionStorage(), cache);
#pragma warning restore CS0618

        var edited = router.Route(EditedFeedback());
        var rejected = router.Route(WithTrainingAdmission(new FeedbackEvent(
            "prompt",
            "rejected",
            null,
            FeedbackSignal.Rejected,
            "BUG")));
        var read = router.GetDpoPairs();

        Assert.False(edited.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_UNAVAILABLE", edited.ErrorCode);
        Assert.False(rejected.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_UNAVAILABLE", rejected.ErrorCode);
        Assert.False(read.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_UNAVAILABLE", read.ErrorCode);
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void UnavailableStorage_FailsBeforeEditedPositiveExampleIsAdded()
    {
        CreatePrivateTestDirectory(_root);
        var occupiedPath = Path.Combine(_root, "occupied");
        File.WriteAllText(occupiedPath, "not a directory");
        var cache = new RecordingDpoPairCache();
        var router = new FeedbackRouter(
            new NullDpoPairSessionStorage(),
            cache,
            occupiedPath);
        var feedback = EditedFeedback();

        var result = router.Route(feedback);

        Assert.False(result.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_UNAVAILABLE", result.ErrorCode);
        Assert.Equal(0, cache.Count);
        AssertNonLeaking(result.ErrorMessage, feedback);
    }

    [Fact]
    public async Task Pipeline_PropagatesPairStoreFailure_WithoutCallingTrainer()
    {
        var trainer = new CountingDpoPairTrainer();
        var pipeline = new ContinuousLearningPipeline(
            new NullDpoPairSessionStorage(),
            new SingleDpoPairExampleIndexer(),
            new RecordingDpoPairCache(),
            new FailingDpoPairRouter(),
            trainer,
            new NullDpoPairRegistry());

        var result = await pipeline.RunCycleAsync("server-owned-project-root");

        Assert.False(result.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CORRUPT", result.ErrorCode);
        Assert.Equal(0, trainer.CallCount);
    }

    [Fact]
    public void DownstreamCancellation_DoesNotEraseAcceptedPair()
    {
        var router = BuildRouter(new RecordingDpoPairCache());
        var feedback = EditedFeedback();
        Assert.True(router.Route(feedback).IsSuccess);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(
            () => cancellation.Token.ThrowIfCancellationRequested());
        var loadedAfterCancellation = BuildRouter(new RecordingDpoPairCache()).GetDpoPairs();

        Assert.True(
            loadedAfterCancellation.IsSuccess,
            loadedAfterCancellation.ErrorMessage);
        Assert.Single(loadedAfterCancellation.Value!);
    }

    private string PersistOneCanonicalRecord()
    {
        var result = BuildRouter(new RecordingDpoPairCache()).Route(EditedFeedback());
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
    }

    private void AssertCorruptStore()
    {
        var result = BuildRouter(new RecordingDpoPairCache()).GetDpoPairs();
        Assert.False(result.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CORRUPT", result.ErrorCode);
        Assert.DoesNotContain(_root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadPrompt(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllBytes(path));
        return document.RootElement.GetProperty("prompt").GetString()!;
    }

    private static DirectoryInfo CreatePrivateTestDirectory(string path)
        => OperatingSystem.IsWindows()
            ? Directory.CreateDirectory(path)
            : Directory.CreateDirectory(
                path,
                UnixFileMode.UserRead
                | UnixFileMode.UserWrite
                | UnixFileMode.UserExecute);

    private static FileStream CreatePrivateTestFile(
        string path,
        FileMode mode,
        FileAccess access,
        FileShare share)
    {
        var options = new FileStreamOptions
        {
            Mode = mode,
            Access = access,
            Share = share,
        };
        if (!OperatingSystem.IsWindows())
        {
            options.UnixCreateMode =
                UnixFileMode.UserRead | UnixFileMode.UserWrite;
        }
        return new FileStream(path, options);
    }

    private static void CreateDirectorySymlinkRequired(string link, string target)
        => Directory.CreateSymbolicLink(link, target);

    private static void CreateFileSymlinkRequired(string link, string target)
        => File.CreateSymbolicLink(link, target);

    private static void DeleteDirectoryLink(string link)
    {
        if (Directory.Exists(link)
            && (File.GetAttributes(link) & FileAttributes.ReparsePoint) != 0)
        {
            Directory.Delete(link);
        }
    }

    private ChildProbe StartChildProbe(
        string dotnetHost,
        string repositoryRoot,
        string projectPath,
        string? prompt = null,
        int? maximumRecordCount = null,
        string? lockMarker = null,
        bool disableImplicitFileLocking = false,
        string childMode = RouteOneChildMode)
    {
        var nonce = Guid.NewGuid().ToString("N");
        var resultPath = Path.Combine(_root, $".child-{nonce}.result");
        var start = new ProcessStartInfo
        {
            FileName = dotnetHost,
            WorkingDirectory = repositoryRoot,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        start.ArgumentList.Add("test");
        start.ArgumentList.Add(projectPath);
        start.ArgumentList.Add("--no-build");
        start.ArgumentList.Add("--no-restore");
        start.ArgumentList.Add("--filter");
        start.ArgumentList.Add(
            "FullyQualifiedName=LlmFromScratch.Tests.DpoPairPersistenceTests"
            + ".SeparateProcessChildProbe_RoutesOnePair");
        start.ArgumentList.Add("--verbosity");
        start.ArgumentList.Add("quiet");
        foreach (var variable in start.Environment.Keys
                     .Where(variable => variable.StartsWith(
                         ChildControlPrefix,
                         StringComparison.OrdinalIgnoreCase))
                     .ToArray())
        {
            start.Environment.Remove(variable);
        }
        start.Environment.Remove(DisableFileLockingVariable);
        start.Environment[ChildRootVariable] = _root;
        start.Environment[ChildModeVariable] = childMode;
        start.Environment[ChildNonceVariable] = nonce;
        start.Environment[ChildResultVariable] = resultPath;
        if (!string.IsNullOrWhiteSpace(prompt))
            start.Environment[ChildPromptVariable] = prompt;
        if (maximumRecordCount is not null)
        {
            start.Environment[ChildMaximumVariable] =
                maximumRecordCount.Value.ToString();
        }
        if (!string.IsNullOrWhiteSpace(lockMarker))
            start.Environment[ChildLockMarkerVariable] = lockMarker;
        if (disableImplicitFileLocking)
            start.Environment[DisableFileLockingVariable] = "1";

        var process = Process.Start(start);
        Assert.NotNull(process);
        return new ChildProbe(
            process,
            nonce,
            resultPath,
            process.StandardOutput.ReadToEndAsync(),
            process.StandardError.ReadToEndAsync());
    }

    private static async Task WaitForPath(string path, ChildProbe child)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        while (true)
        {
            timeout.Token.ThrowIfCancellationRequested();
            if (File.Exists(path))
            {
                var marker = await File.ReadAllTextAsync(path, timeout.Token);
                if (StringComparer.Ordinal.Equals(marker, child.Nonce))
                    return;
            }
            if (child.Process.HasExited)
            {
                var output = await child.StandardOutput;
                var error = await child.StandardError;
                throw new InvalidOperationException(
                    $"Lock-holder child exited with {child.Process.ExitCode}."
                    + $"{Environment.NewLine}stdout:{Environment.NewLine}{output}"
                    + $"{Environment.NewLine}stderr:{Environment.NewLine}{error}");
            }

            await Task.Delay(25, timeout.Token);
        }
    }

    private static async Task<string> WaitForChildProbe(ChildProbe child)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        await child.Process.WaitForExitAsync(timeout.Token);
        var output = await child.StandardOutput.WaitAsync(timeout.Token);
        var error = await child.StandardError.WaitAsync(timeout.Token);
        Assert.True(
            child.Process.ExitCode == 0,
            $"Child probe exit={child.Process.ExitCode}.{Environment.NewLine}"
            + $"stdout:{Environment.NewLine}{output}{Environment.NewLine}"
            + $"stderr:{Environment.NewLine}{error}");
        Assert.True(
            File.Exists(child.ResultPath),
            $"Child probe did not publish nonce result.{Environment.NewLine}"
            + $"stdout:{Environment.NewLine}{output}{Environment.NewLine}"
            + $"stderr:{Environment.NewLine}{error}");
        var payload = await File.ReadAllTextAsync(
            child.ResultPath,
            timeout.Token);
        var prefix = child.Nonce + "|";
        Assert.StartsWith(prefix, payload, StringComparison.Ordinal);
        return payload[prefix.Length..];
    }

    private static async Task CleanupChildProbeAsync(ChildProbe child)
    {
        try
        {
            try
            {
                if (!child.Process.HasExited)
                    child.Process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException) when (child.Process.HasExited)
            {
                // The process exited between the state check and Kill.
            }

            using var timeout = new CancellationTokenSource(
                TimeSpan.FromSeconds(15));
            await child.Process.WaitForExitAsync(timeout.Token);
            await child.StandardOutput.WaitAsync(timeout.Token);
            await child.StandardError.WaitAsync(timeout.Token);
            Assert.True(child.Process.HasExited);
        }
        finally
        {
            child.Process.Dispose();
        }
    }

    private static string ToggleAsciiCase(string value)
        => new(value.Select(character => character switch
        {
            >= 'a' and <= 'z' => (char)(character - ('a' - 'A')),
            >= 'A' and <= 'Z' => (char)(character + ('a' - 'A')),
            _ => character,
        }).ToArray());

    private static string FindDotnetHost()
    {
        var configured = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");
        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
            return configured;

        var directory = new DirectoryInfo(
            Path.GetDirectoryName(typeof(object).Assembly.Location)!);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet");
            if (File.Exists(candidate))
                return candidate;
            directory = directory.Parent;
        }

        throw new InvalidOperationException("The bounded child probe could not locate dotnet.");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new InvalidOperationException("The bounded child probe could not locate the repository.");
    }

    public void Dispose()
    {
        var temporaryRoot = Path.GetFullPath(Path.GetTempPath())
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fullRoot = Path.GetFullPath(_root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        Assert.True(
            string.Equals(
                temporaryRoot,
                Path.GetDirectoryName(fullRoot),
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal));
        Assert.StartsWith(
            "xiigen-dpo-pair-store-",
            Path.GetFileName(fullRoot),
            StringComparison.Ordinal);
        if (!Directory.Exists(fullRoot))
            return;

        Assert.False(
            (File.GetAttributes(fullRoot) & FileAttributes.ReparsePoint) != 0);
        foreach (var entry in Directory.EnumerateFileSystemEntries(
                     fullRoot,
                     "*",
                     SearchOption.TopDirectoryOnly))
        {
            Assert.True(
                string.Equals(
                    fullRoot,
                    Path.GetDirectoryName(Path.GetFullPath(entry)),
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal));
            if ((File.GetAttributes(entry) & FileAttributes.ReparsePoint) == 0)
                continue;
            if (Directory.Exists(entry))
                Directory.Delete(entry);
            else
                File.Delete(entry);
        }

        Directory.Delete(fullRoot, recursive: true);
    }

    private FeedbackRouter BuildRouter(
        IDomainDatasetCache cache,
        int maximumRecordCount = FileDpoPairStore.DefaultMaximumRecordCount,
        Action? afterMutationLockAcquiredForTesting = null,
        TimeSpan? mutationLockTimeoutForTesting = null,
        Func<IOException?>? mutationRegionLockFailureForTesting = null,
        bool corruptMutationLockLengthAfterAcquisitionForTesting = false,
        bool unsupportedPlatformForTesting = false)
        => new(
            new NullDpoPairSessionStorage(),
            cache,
            _root,
            maximumRecordCount,
            afterMutationLockAcquiredForTesting,
            mutationLockTimeoutForTesting,
            mutationRegionLockFailureForTesting,
            corruptMutationLockLengthAfterAcquisitionForTesting,
            unsupportedPlatformForTesting);

    private static FeedbackEvent EditedFeedback(string prompt = "private prompt")
        => WithTrainingAdmission(new FeedbackEvent(
            prompt,
            "private rejected response",
            "private corrected response",
            FeedbackSignal.Edited,
            "CODE_Q"));

    private static FeedbackEvent WithTrainingAdmission(FeedbackEvent feedback)
        => feedback with
        {
            TeacherProvenance = TrainingTeacherProvenance.Deterministic(
                CreateTrainingRequestId(feedback)),
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                [$"sha256:{new string('a', 64)}"]),
        };

    private static string CreateTrainingRequestId(FeedbackEvent feedback)
    {
        var canonical = JsonSerializer.Serialize(new
        {
            feedback.Prompt,
            feedback.ModelResponse,
            feedback.UserCorrection,
            Signal = (int)feedback.Signal,
            feedback.Domain,
        });
        return Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
    }

    private void AssertNonLeaking(string message, FeedbackEvent feedback)
    {
        Assert.DoesNotContain(_root, message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(feedback.Prompt, message, StringComparison.Ordinal);
        Assert.DoesNotContain(feedback.ModelResponse, message, StringComparison.Ordinal);
        Assert.DoesNotContain(feedback.UserCorrection!, message, StringComparison.Ordinal);
    }
}

file sealed class RecordingDpoPairCache : IDomainDatasetCache
{
    private readonly List<TrainingExample> _examples = [];

    public int Count => _examples.Count;

    public OperationResult<bool> Add(TrainingExample example)
    {
        _examples.Add(example);
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<IReadOnlyList<TrainingExample>> GetByDomain(string domain)
        => OperationResult<IReadOnlyList<TrainingExample>>.Ok(
            _examples.Where(example => StringComparer.Ordinal.Equals(example.Domain, domain))
                .ToArray());

    public OperationResult<int> TotalCount()
        => OperationResult<int>.Ok(_examples.Count);

    public Task<OperationResult<bool>> FlushAsync()
        => Task.FromResult(OperationResult<bool>.Ok(true));
}

[AttributeUsage(AttributeTargets.Method)]
file sealed class SymlinkFactAttribute : FactAttribute
{
    private static readonly Lazy<string?> UnavailableReason = new(
        DpoPairSymlinkTestSupport.GetCapabilitySkipReason);

    public SymlinkFactAttribute()
    {
        Skip = UnavailableReason.Value;
    }
}

[AttributeUsage(AttributeTargets.Method)]
file sealed class LinuxFactAttribute : FactAttribute
{
    public LinuxFactAttribute()
    {
        if (!OperatingSystem.IsLinux())
            Skip = "LINUX_ONLY_OS_LOCK_AND_UNIX_MODE_CONTRACT";
    }
}

file static class DpoPairSymlinkTestSupport
{
    public const int HResultPrivilegeNotHeld = unchecked((int)0x80070522);
    public const int HResultOperationNotSupported = unchecked((int)0x80070032);
    public const int HResultInvalidFunction = unchecked((int)0x80070001);

    public static string? GetCapabilitySkipReason()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"xiigen-dpo-pair-symlink-probe-{Guid.NewGuid():N}");
        var directoryTarget = Path.Combine(root, "directory-target");
        var directoryLink = Path.Combine(root, "directory-link");
        var fileTarget = Path.Combine(root, "file-target");
        var fileLink = Path.Combine(root, "file-link");

        try
        {
            Directory.CreateDirectory(directoryTarget);
            File.WriteAllText(fileTarget, "probe");
            Directory.CreateSymbolicLink(directoryLink, directoryTarget);
            File.CreateSymbolicLink(fileLink, fileTarget);
            return null;
        }
        catch (Exception exception)
            when (IsRecognizedCapabilityLimitation(
                exception,
                OperatingSystem.IsWindows()))
        {
            return "REPARSE_TEST_PLATFORM_LIMITATION: "
                + $"{exception.GetType().FullName}, HResult=0x{exception.HResult:X8}: "
                + exception.Message;
        }
        finally
        {
            if (File.Exists(fileLink))
                File.Delete(fileLink);
            if (Directory.Exists(directoryLink)
                && (File.GetAttributes(directoryLink) & FileAttributes.ReparsePoint) != 0)
            {
                Directory.Delete(directoryLink);
            }
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    public static bool IsRecognizedCapabilityLimitation(
        Exception exception,
        bool isWindows)
    {
        if (exception is NotSupportedException)
            return true;
        if (!isWindows
            || exception is not (IOException or UnauthorizedAccessException))
        {
            return false;
        }

        return exception.HResult is
            HResultPrivilegeNotHeld
            or HResultOperationNotSupported
            or HResultInvalidFunction;
    }
}

file sealed class SyntheticDpoPairIOException : IOException
{
    public SyntheticDpoPairIOException(int hResult)
        : base("Synthetic DPO-pair symlink I/O failure.")
        => HResult = hResult;
}

file sealed class SyntheticDpoPairUnauthorizedAccessException
    : UnauthorizedAccessException
{
    public SyntheticDpoPairUnauthorizedAccessException(int hResult)
        : base("Synthetic DPO-pair symlink access failure.")
        => HResult = hResult;
}

file sealed class NullDpoPairSessionStorage : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        => Task.FromResult(OperationResult<bool>.Ok(true));

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        => Task.FromResult(OperationResult<AgentState>.Fail(
            "Session state is unavailable.",
            "NOT_FOUND"));

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok([]));
}

file sealed class SingleDpoPairExampleIndexer : IProjectIndexer
{
    public OperationResult<IReadOnlyList<TrainingExample>> Index(string rootPath)
        => OperationResult<IReadOnlyList<TrainingExample>>.Ok(
            [
                new TrainingExample(
                    "prompt",
                    "response",
                    "BUG",
                    1d,
                    DateTimeOffset.UnixEpoch),
            ]);
}

file sealed class FailingDpoPairRouter : IFeedbackRouter
{
    public OperationResult<bool> Route(FeedbackEvent feedback)
        => OperationResult<bool>.Fail(
            "Pair storage is unavailable.",
            "DPO_PAIR_STORE_CORRUPT");

    public OperationResult<IReadOnlyList<DpoPair>> GetDpoPairs()
        => OperationResult<IReadOnlyList<DpoPair>>.Fail(
            "Pair storage is unavailable.",
            "DPO_PAIR_STORE_CORRUPT");
}

file sealed class CountingDpoPairTrainer : IIncrementalTrainer
{
    public int CallCount { get; private set; }

    public Task<OperationResult<TrainedAdapterArtifact>> TrainAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair> dpoPairs,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.FromResult(OperationResult<TrainedAdapterArtifact>.Fail(
            "The trainer must not be called.",
            "UNEXPECTED_TRAINER_CALL"));
    }
}

file sealed class NullDpoPairRegistry : ContinuousLearning.IDomainModelRegistry
{
    public OperationResult<bool> Register(ModelAdapter adapter)
        => OperationResult<bool>.Ok(true);

    public OperationResult<ModelAdapter?> GetAdapter(string domain)
        => OperationResult<ModelAdapter?>.Ok(null);

    public OperationResult<IReadOnlyList<ModelAdapter>> ListAll()
        => OperationResult<IReadOnlyList<ModelAdapter>>.Ok([]);
}
