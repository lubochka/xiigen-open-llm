// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ContinuousLearning;
using Microsoft.Win32.SafeHandles;

public sealed class ServerOwnedContinuousLearningFileAccessTests
{
    [ContinuousLearningFileAccessWindowsFact]
    public void SixDistinctPreprovisionedPaths_RetainPointInTimeLeaseAndKeepGuardIncomplete()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var startup = fixture.CreateStartup();
        var before = fixture.Snapshot();

        using var lease = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(lease);
        Assert.Throws<ContinuousLearningPathIdentityException>(
            () => startup.AcquireRetainedFileAccess());

        Assert.Equal(before, fixture.Snapshot());
        Assert.True(startup.Readiness.ConfigurationAdmitted);
        Assert.False(startup.Readiness.PathIdentityVerified);
        Assert.Equal(
            "retained_object_identity_bound_path_unverified_inactive",
            startup.Readiness.ConfigurationStatus);
        Assert.Equal(
            ContinuousLearningModelServerStartup.NamespacePathUnverifiedErrorCode,
            startup.Readiness.ConfigurationErrorCode);
        Assert.False(startup.Readiness.CompositionMaterialized);
        Assert.True(startup.Readiness.RetainedObjectIdentityBound);
        Assert.False(startup.Readiness.StableDpoExecutorConfigured);
        Assert.False(startup.Readiness.TrainingEnabled);
        Assert.False(startup.Readiness.CycleExecutionAvailable);
        Assert.False(startup.Readiness.PerformsTraining);
        Assert.False(startup.Readiness.ProductReady);
        AssertExactConfiguredPathsNotReachable(
            [startup, lease],
            fixture.ConfiguredPaths);
    }

    [ContinuousLearningFileAccessWindowsFact]
    public void RetainedArtifactReads_UseIndependentFilePointersAndSurviveSiblingDisposal()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var startup = fixture.CreateStartup();
        using var lease = startup.AcquireRetainedFileAccess();

        using var first = lease.OpenRetainedArtifactForRead(
            ContinuousLearningRetainedArtifactRole.BaseCheckpoint);
        Assert.Equal(0, first.Position);
        Assert.Equal(0x01, first.ReadByte());

        using var second = lease.OpenRetainedArtifactForRead(
            ContinuousLearningRetainedArtifactRole.BaseCheckpoint);
        Assert.Equal(0, second.Position);
        Assert.Equal(0x01, second.ReadByte());
        first.Dispose();

        Assert.Equal(0x02, second.ReadByte());
        Assert.True(second.CanRead);
    }

    [ContinuousLearningFileAccessLinuxX64Fact]
    public void PosixSixDistinctPaths_RetainIdentityAndReadArtifactsThroughIndependentOpenAt()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        var startup = fixture.CreateStartup();

        using var lease = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(lease);

        using var first = lease.OpenRetainedArtifactForRead(
            ContinuousLearningRetainedArtifactRole.BaseCheckpoint);
        Assert.Equal(0, first.Position);
        Assert.Equal(0x01, first.ReadByte());

        using var second = lease.OpenRetainedArtifactForRead(
            ContinuousLearningRetainedArtifactRole.BaseCheckpoint);
        Assert.Equal(0, second.Position);
        Assert.Equal(0x01, second.ReadByte());
        first.Dispose();

        Assert.Equal(0x02, second.ReadByte());
        Assert.True(startup.Readiness.RetainedObjectIdentityBound);
        AssertExactConfiguredPathsNotReachable(
            [startup, lease],
            fixture.ConfiguredPaths);
    }

    [ContinuousLearningFileAccessLinuxX64Fact]
    public void PosixFinalArtifactSymlink_FailsClosedWithoutReadingOutsideTarget()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        var outsideRoot = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-file-access-outside-{Guid.NewGuid():N}");
        Directory.CreateDirectory(outsideRoot);
        var outsideFile = Path.Combine(outsideRoot, "sentinel.bin");
        File.WriteAllBytes(outsideFile, [0xCA, 0xFE]);

        try
        {
            File.Delete(fixture.BaseCheckpointPath);
            // POSIX-регрессия строит настоящую final symlink-подмену. Если
            // backend уберёт O_NOFOLLOW, Acquire прочитает внешний sentinel
            // под доверенным именем base checkpoint.
            File.CreateSymbolicLink(fixture.BaseCheckpointPath, outsideFile);
            var startup = fixture.CreateStartup();

            var exception = Assert.Throws<ContinuousLearningPathIdentityException>(
                () => startup.AcquireRetainedFileAccess());

            Assert.Equal(
                ContinuousLearningPathIdentityException.StableErrorCode,
                exception.Message);
            Assert.Equal([0xCA, 0xFE], File.ReadAllBytes(outsideFile));
        }
        finally
        {
            if (Directory.Exists(outsideRoot))
                Directory.Delete(outsideRoot, recursive: true);
        }
    }

    [ContinuousLearningFileAccessLinuxX64Fact]
    public void PosixTerminalRootSymlink_FailsClosedWithoutBindingOutsideDirectory()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        var outsideRoot = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-file-access-root-outside-{Guid.NewGuid():N}");
        Directory.CreateDirectory(outsideRoot);

        try
        {
            Directory.Delete(fixture.PairRoot);
            // Terminal mutation root тоже нельзя принимать через symlink:
            // иначе последующие admission/registry шаги получили бы доверие
            // к чужому каталогу под правильной строкой конфигурации.
            File.CreateSymbolicLink(fixture.PairRoot, outsideRoot);
            var startup = fixture.CreateStartup();

            var exception = Assert.Throws<ContinuousLearningPathIdentityException>(
                () => startup.AcquireRetainedFileAccess());

            Assert.Equal(
                ContinuousLearningPathIdentityException.StableErrorCode,
                exception.Message);
            Assert.True(Directory.Exists(outsideRoot));
        }
        finally
        {
            if (Directory.Exists(outsideRoot))
                Directory.Delete(outsideRoot, recursive: true);
        }
    }

    [ContinuousLearningFileAccessLinuxX64Fact]
    public void PosixDeleteRecreateArtifact_MakesRetainedTrustStickyLost()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        var startup = fixture.CreateStartup();
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);

        File.Delete(fixture.BaseCheckpointPath);
        File.WriteAllBytes(fixture.BaseCheckpointPath, [0x10, 0x20, 0x30]);
        SetLinuxFileMode(
            fixture.BaseCheckpointPath,
            UnixFileMode.UserRead | UnixFileMode.UserWrite);

        var exception = Assert.Throws<ContinuousLearningPathIdentityException>(
            access.EnsureRetained);

        Assert.Equal(
            ContinuousLearningPathIdentityException.StableErrorCode,
            exception.Message);
        Assert.False(startup.ObserveReadiness().RetainedObjectIdentityBound);
    }

    [ContinuousLearningFileAccessLinuxX64Fact]
    public void PosixRootDeleteRecreate_MakesRetainedTrustStickyLost()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        var startup = fixture.CreateStartup();
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        var movedPairRoot = fixture.PairRoot + ".moved";

        Directory.Move(fixture.PairRoot, movedPairRoot);
        Directory.CreateDirectory(fixture.PairRoot);
        SetLinuxFileMode(
            fixture.PairRoot,
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.UserExecute);

        var exception = Assert.Throws<ContinuousLearningPathIdentityException>(
            access.EnsureRetained);

        Assert.Equal(
            ContinuousLearningPathIdentityException.StableErrorCode,
            exception.Message);
        Assert.False(startup.ObserveReadiness().RetainedObjectIdentityBound);
    }

    [ContinuousLearningFileAccessLinuxX64Fact]
    public void PosixValidateExistingRegularLeaf_RejectsFinalSymlinkWithoutFollowing()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        var outsideRoot = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-file-access-leaf-outside-{Guid.NewGuid():N}");
        Directory.CreateDirectory(outsideRoot);
        var outsideFile = Path.Combine(outsideRoot, "sentinel.bin");
        File.WriteAllBytes(outsideFile, [0x41]);

        try
        {
            var link = Path.Combine(fixture.PairRoot, "selected-pair.bin");
            File.CreateSymbolicLink(link, outsideFile);
            var startup = fixture.CreateStartup();
            using var access = startup.AcquireRetainedFileAccess();
            startup.BindRetainedFileAccess(access);

            // Admission leaf — это bool-gate, а не потеря доверия root:
            // удержанный каталог остаётся валидным, пока финальная symlink
            // компонента просто не принимается как regular leaf.
            Assert.False(access.ValidateExistingRegularLeaf(
                ContinuousLearningRetainedRootRole.DpoPairs,
                "selected-pair.bin",
                maximumBytes: 32));
            access.EnsureRetained();
            Assert.Equal([0x41], File.ReadAllBytes(outsideFile));
        }
        finally
        {
            if (Directory.Exists(outsideRoot))
                Directory.Delete(outsideRoot, recursive: true);
        }
    }

    [ContinuousLearningFileAccessLinuxX64Fact]
    public void PosixGroupWritableTerminalRoot_IsRejectedBeforeBinding()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        SetLinuxFileMode(
            fixture.PairRoot,
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.UserExecute |
            UnixFileMode.GroupWrite);
        var startup = fixture.CreateStartup();

        var exception = Assert.Throws<ContinuousLearningPathIdentityException>(
            () => startup.AcquireRetainedFileAccess());

        Assert.Equal(
            ContinuousLearningPathIdentityException.StableErrorCode,
            exception.Message);
    }

    [ContinuousLearningFileAccessWindowsFact]
    public void MissingFinalPairRoot_FailsAndRollsBackEveryEarlierHandle()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: false);
        var startup = fixture.CreateStartup();
        var before = fixture.Snapshot();

        var exception = Assert.Throws<ContinuousLearningPathIdentityException>(
            () => startup.AcquireRetainedFileAccess());

        Assert.Equal(
            ContinuousLearningPathIdentityException.StableErrorCode,
            exception.Message);
        Assert.Equal(before, fixture.Snapshot());
        Directory.CreateDirectory(fixture.PairRoot);
        Assert.Throws<ContinuousLearningPathIdentityException>(
            () => startup.AcquireRetainedFileAccess());
        AssertExactConfiguredPathsNotReachable(
            [startup],
            fixture.ConfiguredPaths);
    }

    [ContinuousLearningFileAccessWindowsFact]
    public void HardLinkedCheckpointAndTokenizerAlias_IsRejectedWithWin32SetupEvidence()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        File.Delete(fixture.TokenizerPath);
        CreateHardLinkOrFail(
            fixture.TokenizerPath,
            fixture.BaseCheckpointPath);
        var startup = fixture.CreateStartup();

        var exception = Assert.Throws<ContinuousLearningPathIdentityException>(
            () => startup.AcquireRetainedFileAccess());

        Assert.Equal(
            ContinuousLearningPathIdentityException.StableErrorCode,
            exception.Message);
        fixture.AssertDirectoryRenameable(fixture.Root);
    }

    [ContinuousLearningFileAccessWindowsFact]
    public void ArtifactWithSecondUnconfiguredHardLink_IsRejected()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        CreateHardLinkOrFail(
            Path.Combine(fixture.Root, "unconfigured-checkpoint-link.bin"),
            fixture.BaseCheckpointPath);
        var startup = fixture.CreateStartup();

        var exception = Assert.Throws<ContinuousLearningPathIdentityException>(
            () => startup.AcquireRetainedFileAccess());

        Assert.Equal(
            ContinuousLearningPathIdentityException.StableErrorCode,
            exception.Message);
        fixture.AssertDirectoryRenameable(fixture.Root);
    }

    [ContinuousLearningFileAccessWindowsFact]
    public void ParentRetainedRace_SwappedChildJunctionFailsAndOutsideSentinelIsUntouched()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var outside = Path.Combine(fixture.Root, "outside-sentinel-root");
        Directory.CreateDirectory(outside);
        var sentinel = Path.Combine(outside, "sentinel.bin");
        File.WriteAllBytes(sentinel, [0x55, 0xAA, 0x19]);
        var sentinelBefore = File.ReadAllBytes(sentinel);
        var startup = fixture.CreateStartup();
        Exception? setupFailure = null;
        var hookCompleted = false;

        try
        {
            var exception = Assert.Throws<ContinuousLearningPathIdentityException>(
                () => startup.AcquireRetainedFileAccess(
                    new ServerOwnedContinuousLearningFileAccessOptions
                    {
                        AfterParentRetainedBeforeChildOpenForTesting =
                            (_, childPath) =>
                            {
                                if (!string.Equals(
                                        childPath,
                                        fixture.PairRoot,
                                        StringComparison.OrdinalIgnoreCase))
                                {
                                    return;
                                }

                                try
                                {
                                    Directory.Delete(fixture.PairRoot);
                                    CreateWindowsJunctionOrFail(
                                        fixture.PairRoot,
                                        outside);
                                    hookCompleted = true;
                                }
                                catch (Exception setupException)
                                {
                                    setupFailure = setupException;
                                    throw;
                                }
                            },
                    }));

            Assert.Equal(
                ContinuousLearningPathIdentityException.StableErrorCode,
                exception.Message);
            Assert.True(
                setupFailure is null,
                setupFailure?.Message ?? "Junction setup unexpectedly failed.");
            Assert.True(hookCompleted);
            Assert.Equal(sentinelBefore, File.ReadAllBytes(sentinel));
        }
        finally
        {
            RemoveWindowsJunctionIfPresent(fixture.PairRoot);
        }
    }

    [ContinuousLearningFileAccessWindowsFact]
    public void SuccessfulLease_IdempotentDisposeClosesNativeHandlesAndAllowsOneWayRenames()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var startup = fixture.CreateStartup();
        using var lease = startup.AcquireRetainedFileAccess();
        var retainedHandles = GetRetainedHandles(lease);

        AssertDeleteAccessBlockedExternally(
            fixture.BaseCheckpointPath,
            requireDirectory: false);
        AssertDeleteAccessBlockedExternally(
            fixture.TokenizerPath,
            requireDirectory: false);

        lease.Dispose();
        lease.Dispose();
        Assert.All(retainedHandles, handle => Assert.True(handle.IsClosed));

        AssertDeleteAccessAvailable(
            fixture.BaseCheckpointPath,
            requireDirectory: false);
        AssertDeleteAccessAvailable(
            fixture.TokenizerPath,
            requireDirectory: false);
        AssertDeleteAccessAvailable(
            fixture.AdapterRoot,
            requireDirectory: true);
        AssertDeleteAccessAvailable(
            fixture.Root,
            requireDirectory: true);
        using (var replacement = fixture.CreateStartup().AcquireRetainedFileAccess())
        {
            replacement.EnsureRetained();
            using var adapter = replacement.AcquireAdapterRootCapability();
            using var cache = replacement.AcquireCompilerCacheRootCapability();
            using var registry = replacement.AcquireRegistryRootCapability();
            Assert.NotNull(adapter);
            Assert.NotNull(cache);
            Assert.NotNull(registry);
        }
        fixture.AssertDirectoryRenameable(fixture.PairRoot);
        fixture.AssertPostDisposeOneWayRenames();
    }

    [ContinuousLearningFileAccessWindowsFact]
    public void TerminalDirectoryLease_MetadataMutationFalsifiesStableNamespaceGuard()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var junctionProbe = new JunctionProbeTarget(fixture.PairRoot);
        var startup = fixture.CreateStartup();
        var before = fixture.Snapshot();
        using var lease = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(lease);
        var readinessBefore = startup.Readiness;
        Assert.True(readinessBefore.RetainedObjectIdentityBound);
        Assert.False(readinessBefore.PathIdentityVerified);

        AssertGenericWriteJunctionMutationBlocked(
            fixture.PairRoot,
            junctionProbe.TargetRoot);
        CreateWindowsMetadataOnlyJunctionOrFail(
            fixture.PairRoot,
            junctionProbe.TargetRoot);
        Assert.True(
            (File.GetAttributes(fixture.PairRoot) & FileAttributes.ReparsePoint) != 0);
        var lost = startup.Readiness;
        Assert.False(lost.PathIdentityVerified);
        Assert.False(lost.RetainedObjectIdentityBound);
        Assert.Equal("retained_object_identity_lost_inactive", lost.ConfigurationStatus);
        Assert.Equal(
            ContinuousLearningModelServerStartup.RetainedObjectIdentityLostErrorCode,
            lost.ConfigurationErrorCode);
        Assert.False(lost.CompositionMaterialized);
        Assert.False(lost.StableDpoExecutorConfigured);
        Assert.False(lost.TrainingEnabled);
        Assert.False(lost.CycleExecutionAvailable);
        Assert.False(lost.PerformsTraining);
        Assert.False(lost.ProductReady);
        junctionProbe.AssertSentinelUnchanged();
        DeleteWindowsJunctionTagMetadataOnlyOrFail(fixture.PairRoot);

        Assert.Equal(before, fixture.Snapshot());
        Assert.Throws<ContinuousLearningPathIdentityException>(lease.EnsureRetained);
        Assert.Equal(lost, startup.ObserveReadiness());
        junctionProbe.AssertSentinelUnchanged();

        lease.Dispose();
        CreateWindowsJunctionOrFail(
            fixture.PairRoot,
            junctionProbe.TargetRoot);
        junctionProbe.AssertSentinelUnchanged();
        Assert.True(
            (File.GetAttributes(fixture.PairRoot) & FileAttributes.ReparsePoint) != 0);
        RemoveWindowsJunctionIfPresent(fixture.PairRoot);
        Directory.CreateDirectory(fixture.PairRoot);
        fixture.AssertDirectoryRenameable(fixture.PairRoot);
        junctionProbe.AssertSentinelUnchanged();
    }

    [ContinuousLearningFileAccessWindowsFact]
    public async Task ObserveReadiness_PublishesStickyLossWhileAccessCallbackIsBlocked()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var junctionProbe = new JunctionProbeTarget(fixture.PairRoot);
        var startup = fixture.CreateStartup();
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        var callbackField = typeof(ServerOwnedContinuousLearningFileAccess).GetField(
            "_onRetainedObjectTrustLost",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var originalCallback = Assert.IsType<Action>(callbackField.GetValue(access));
        using var callbackEntered = new ManualResetEventSlim(initialState: false);
        using var releaseCallback = new ManualResetEventSlim(initialState: false);
        var callbackCount = 0;
        callbackField.SetValue(access, (Action)(() =>
        {
            Interlocked.Increment(ref callbackCount);
            callbackEntered.Set();
            releaseCallback.Wait();
            originalCallback();
        }));
        Task<Exception>? ensureTask = null;
        ContinuousLearningProductionReadinessStatus? observed = null;

        try
        {
            CreateWindowsMetadataOnlyJunctionOrFail(
                fixture.PairRoot,
                junctionProbe.TargetRoot);
            ensureTask = Task.Run(() => Record.Exception(access.EnsureRetained));
            Assert.True(
                callbackEntered.Wait(TimeSpan.FromSeconds(5)),
                "The retained-object loss callback was not entered within the bound.");

            observed = startup.Readiness;
            var explicitlyObserved = startup.ObserveReadiness();

            Assert.False(observed.RetainedObjectIdentityBound);
            Assert.Equal(
                "retained_object_identity_lost_inactive",
                observed.ConfigurationStatus);
            Assert.Equal(
                ContinuousLearningModelServerStartup.RetainedObjectIdentityLostErrorCode,
                observed.ConfigurationErrorCode);
            Assert.False(observed.PathIdentityVerified);
            Assert.False(observed.CompositionMaterialized);
            Assert.False(observed.StableDpoExecutorConfigured);
            Assert.False(observed.TrainingEnabled);
            Assert.False(observed.CycleExecutionAvailable);
            Assert.False(observed.PerformsTraining);
            Assert.False(observed.ProductReady);
            Assert.Equal(observed, explicitlyObserved);
        }
        finally
        {
            releaseCallback.Set();
        }

        var ensureException = await ensureTask!.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.IsType<ContinuousLearningPathIdentityException>(ensureException);
        Assert.Equal(1, Volatile.Read(ref callbackCount));
        Assert.Equal(observed, startup.ObserveReadiness());
        junctionProbe.AssertSentinelUnchanged();

        DeleteWindowsJunctionTagMetadataOnlyOrFail(fixture.PairRoot);

        Assert.Equal(observed, startup.ObserveReadiness());
        Assert.Equal(1, Volatile.Read(ref callbackCount));
        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
        junctionProbe.AssertSentinelUnchanged();
    }

    [ContinuousLearningFileAccessWindowsFact]
    public async Task RealModelServerProcessLease_MalformedArtifactsFailClosedBeforeMetadataMutation()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var junctionProbe = new JunctionProbeTarget(fixture.PairRoot);
        var before = fixture.Snapshot();
        var readyRoot = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-lease-host-probe-{Guid.NewGuid():N}");
        Directory.CreateDirectory(readyRoot);
        var readyFile = Path.Combine(readyRoot, "ready.json");
        var executable = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "25.ModelServer",
            "bin",
            "Debug",
            "net9.0",
            "ModelServer.exe"));
        Assert.True(File.Exists(executable), $"ModelServer executable missing: {executable}");
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = executable,
                ArgumentList =
                {
                    ModelServer.Program.ReadyFileArgumentName,
                    readyFile,
                },
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
        };
        process.StartInfo.Environment["DOTNET_ROLL_FORWARD"] = "Major";
        foreach (var (name, value) in fixture.EnvironmentValues())
            process.StartInfo.Environment[name] = value;

        try
        {
            Assert.True(process.Start(), "ModelServer lease probe process did not start.");
            var ready = SpinWait.SpinUntil(
                () => File.Exists(readyFile) || process.HasExited,
                TimeSpan.FromSeconds(20));
            Assert.True(
                ready && File.Exists(readyFile) && !process.HasExited,
                "ModelServer did not publish readiness. " +
                DescribeExitedProcess(process));
            Assert.Equal(
                executable,
                Path.GetFullPath(process.MainModule!.FileName),
                ignoreCase: true);
            using var readyDocument = JsonDocument.Parse(
                File.ReadAllText(readyFile));
            var baseUrl = new Uri(readyDocument.RootElement
                .GetProperty("baseUrl")
                .GetString()!);
            using var client = new HttpClient { BaseAddress = baseUrl };
            var readinessJson = await client.GetStringAsync(
                ModelServer.ContinuousLearningReadinessEndpoint.EndpointPath);
            using var readiness = JsonDocument.Parse(readinessJson);
            Assert.False(readiness.RootElement
                .GetProperty("path_identity_verified")
                .GetBoolean());
            Assert.Equal(
                "materialization_failed_inactive",
                readiness.RootElement
                    .GetProperty("configuration_status")
                    .GetString());
            Assert.Equal(
                ContinuousLearningModelServerStartup
                    .MaterializationFailedErrorCode,
                readiness.RootElement
                    .GetProperty("configuration_error_code")
                    .GetString());
            Assert.False(readiness.RootElement
                .GetProperty("retained_object_identity_bound")
                .GetBoolean());
            Assert.False(readiness.RootElement
                .GetProperty("composition_materialized")
                .GetBoolean());
            Assert.False(readiness.RootElement
                .GetProperty("stable_dpo_executor_configured")
                .GetBoolean());
            Assert.False(readiness.RootElement
                .GetProperty("training_enabled")
                .GetBoolean());
            Assert.False(readiness.RootElement
                .GetProperty("cycle_execution_available")
                .GetBoolean());
            Assert.False(readiness.RootElement
                .GetProperty("performs_training")
                .GetBoolean());
            Assert.False(readiness.RootElement
                .GetProperty("product_ready")
                .GetBoolean());

            Assert.False(
                process.HasExited,
                "ModelServer exited before its live lease could be probed. " +
                DescribeExitedProcess(process));

            AssertDeleteAccessBlockedExternally(
                fixture.BaseCheckpointPath,
                requireDirectory: false);
            AssertDeleteAccessBlockedExternally(
                fixture.TokenizerPath,
                requireDirectory: false);
            AssertDeleteAccessBlockedExternally(
                fixture.AdapterRoot,
                requireDirectory: true);
            AssertDeleteAccessBlockedExternally(
                fixture.Root,
                requireDirectory: true);
            AssertGenericWriteJunctionMutationBlocked(
                fixture.PairRoot,
                junctionProbe.TargetRoot);
            CreateWindowsMetadataOnlyJunctionOrFail(
                fixture.PairRoot,
                junctionProbe.TargetRoot);
            Assert.True(
                (File.GetAttributes(fixture.PairRoot) & FileAttributes.ReparsePoint) != 0);
            junctionProbe.AssertSentinelUnchanged();
            var duringMutationJson = await client.GetStringAsync(
                ModelServer.ContinuousLearningReadinessEndpoint.EndpointPath);
            using (var duringMutation = JsonDocument.Parse(duringMutationJson))
            {
                Assert.False(duringMutation.RootElement
                    .GetProperty("path_identity_verified")
                    .GetBoolean());
                Assert.False(duringMutation.RootElement
                    .GetProperty("retained_object_identity_bound")
                    .GetBoolean());
                Assert.Equal(
                    "retained_object_identity_lost_inactive",
                    duringMutation.RootElement
                        .GetProperty("configuration_status")
                        .GetString());
                Assert.Equal(
                    ContinuousLearningModelServerStartup
                        .RetainedObjectIdentityLostErrorCode,
                    duringMutation.RootElement
                        .GetProperty("configuration_error_code")
                        .GetString());
                Assert.False(duringMutation.RootElement
                    .GetProperty("composition_materialized")
                    .GetBoolean());
                Assert.False(duringMutation.RootElement
                    .GetProperty("stable_dpo_executor_configured")
                    .GetBoolean());
                Assert.False(duringMutation.RootElement
                    .GetProperty("training_enabled")
                    .GetBoolean());
                Assert.False(duringMutation.RootElement
                    .GetProperty("cycle_execution_available")
                    .GetBoolean());
                Assert.False(duringMutation.RootElement
                    .GetProperty("performs_training")
                    .GetBoolean());
                Assert.False(duringMutation.RootElement
                    .GetProperty("product_ready")
                    .GetBoolean());
            }
            DeleteWindowsJunctionTagMetadataOnlyOrFail(fixture.PairRoot);
            Assert.Equal(before, fixture.Snapshot());
            junctionProbe.AssertSentinelUnchanged();
            var afterCleanupJson = await client.GetStringAsync(
                ModelServer.ContinuousLearningReadinessEndpoint.EndpointPath);
            using var afterCleanup = JsonDocument.Parse(afterCleanupJson);
            Assert.False(afterCleanup.RootElement
                .GetProperty("retained_object_identity_bound")
                .GetBoolean());
            Assert.Equal(
                "retained_object_identity_lost_inactive",
                afterCleanup.RootElement
                    .GetProperty("configuration_status")
                    .GetString());
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                Assert.True(
                    process.WaitForExit(10_000),
                    "ModelServer lease probe did not terminate within the bound.");
            }
            if (Directory.Exists(readyRoot))
                Directory.Delete(readyRoot, recursive: true);
        }

        Assert.True(process.HasExited);
        AssertDeleteAccessAvailable(
            fixture.BaseCheckpointPath,
            requireDirectory: false);
        AssertDeleteAccessAvailable(
            fixture.TokenizerPath,
            requireDirectory: false);
        AssertDeleteAccessAvailable(
            fixture.AdapterRoot,
            requireDirectory: true);
        AssertDeleteAccessAvailable(
            fixture.Root,
            requireDirectory: true);
        CreateWindowsJunctionOrFail(
            fixture.PairRoot,
            junctionProbe.TargetRoot);
        junctionProbe.AssertSentinelUnchanged();
        Assert.True(
            (File.GetAttributes(fixture.PairRoot) & FileAttributes.ReparsePoint) != 0);
        RemoveWindowsJunctionIfPresent(fixture.PairRoot);
        Directory.CreateDirectory(fixture.PairRoot);
        fixture.AssertDirectoryRenameable(fixture.PairRoot);
        junctionProbe.AssertSentinelUnchanged();
        fixture.AssertPostDisposeOneWayRenames();
    }

    [ContinuousLearningFileAccessWindowsFact]
    public void InjectedUnsupportedPlatform_FailsBeforeNativeCallOrMutation()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var startup = fixture.CreateStartup();
        var before = fixture.Snapshot();
        var hookCalls = 0;

        var exception = Assert.Throws<ContinuousLearningPathIdentityException>(
            () => startup.AcquireRetainedFileAccess(
                new ServerOwnedContinuousLearningFileAccessOptions
                {
                    PlatformForTesting =
                        ServerOwnedContinuousLearningFileAccessPlatform.Unsupported,
                    AfterParentRetainedBeforeChildOpenForTesting =
                        (_, _) => hookCalls++,
                }));

        Assert.Equal(
            ContinuousLearningPathIdentityException.StableErrorCode,
            exception.Message);
        Assert.Equal(0, hookCalls);
        Assert.Equal(before, fixture.Snapshot());
        Assert.Throws<ContinuousLearningPathIdentityException>(
            () => startup.AcquireRetainedFileAccess(
                new ServerOwnedContinuousLearningFileAccessOptions
                {
                    PlatformForTesting =
                        ServerOwnedContinuousLearningFileAccessPlatform.Windows,
                    AfterParentRetainedBeforeChildOpenForTesting =
                        (_, _) => hookCalls++,
                }));
        Assert.Equal(0, hookCalls);
        fixture.AssertDirectoryRenameable(fixture.Root);
    }

    [ContinuousLearningFileAccessWindowsFact]
    public async Task ConcurrentOneShotAcquisition_BurnsBundleExactlyOnce()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var startup = fixture.CreateStartup();
        using var gate = new ManualResetEventSlim(initialState: false);
        var attempts = Enumerable.Range(0, 12)
            .Select(_ => Task.Run(() =>
            {
                gate.Wait();
                try
                {
                    return startup.AcquireRetainedFileAccess();
                }
                catch (ContinuousLearningPathIdentityException)
                {
                    return null;
                }
            }))
            .ToArray();

        gate.Set();
        var results = await Task.WhenAll(attempts);
        var access = Assert.Single(results, result => result is not null)!;
        try
        {
            access.EnsureRetained();
        }
        finally
        {
            access.Dispose();
        }
        Assert.Equal(11, results.Count(result => result is null));
    }

    [ContinuousLearningFileAccessWindowsFact]
    public void HandleRelativeLeafValidation_IsStrictAndDoesNotLoseTrustedRootForLeafRejection()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var validLeaf = "valid.bin";
        File.WriteAllBytes(Path.Combine(fixture.AdapterRoot, validLeaf), [1, 2, 3]);
        File.WriteAllBytes(Path.Combine(fixture.CacheRoot, validLeaf), [4]);
        File.WriteAllBytes(Path.Combine(fixture.RegistryRoot, validLeaf), [5]);
        File.WriteAllBytes(Path.Combine(fixture.PairRoot, validLeaf), [6]);
        Directory.CreateDirectory(Path.Combine(fixture.AdapterRoot, "directory.bin"));
        File.WriteAllBytes(
            Path.Combine(fixture.AdapterRoot, "oversize.bin"),
            [1, 2, 3, 4]);
        var hardLinked = Path.Combine(fixture.AdapterRoot, "hard-linked.bin");
        File.WriteAllBytes(hardLinked, [7]);
        CreateHardLinkOrFail(
            Path.Combine(fixture.AdapterRoot, "hard-linked-second.bin"),
            hardLinked);
        using var reparseTarget = new JunctionProbeTarget(
            Path.Combine(fixture.AdapterRoot, "reparse.bin"));
        CreateWindowsJunctionOrFail(reparseTarget.LinkPath, reparseTarget.TargetRoot);

        var startup = fixture.CreateStartup();
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);

        Assert.True(access.ValidateExistingRegularLeaf(
            ContinuousLearningRetainedRootRole.Adapter,
            validLeaf,
            maximumBytes: 3));
        Assert.True(access.ValidateExistingRegularLeaf(
            ContinuousLearningRetainedRootRole.Cache,
            validLeaf,
            maximumBytes: 1));
        Assert.True(access.ValidateExistingRegularLeaf(
            ContinuousLearningRetainedRootRole.Registry,
            validLeaf,
            maximumBytes: 1));
        Assert.True(access.ValidateExistingRegularLeaf(
            ContinuousLearningRetainedRootRole.DpoPairs,
            validLeaf,
            maximumBytes: 1));

        var invalidNames = new[]
        {
            string.Empty,
            " ",
            ".",
            "..",
            @"nested\leaf.bin",
            "nested/leaf.bin",
            "leaf.bin:stream",
            "leaf<.bin",
            "leaf>.bin",
            "leaf\".bin",
            "leaf|.bin",
            "leaf\0.bin",
            "leaf\u0001.bin",
            "*.bin",
            "?.bin",
            @"C:\leaf.bin",
            @"\\server\share\leaf.bin",
            @"\\?\C:\leaf.bin",
            "leaf.bin.",
            "leaf.bin ",
            new string('x', 256),
            "CON",
            "prn.txt",
            "AUX",
            "nul.bin",
            "COM1",
            "lpt9.bin",
        };
        Assert.All(invalidNames, name => Assert.False(
            access.ValidateExistingRegularLeaf(
                ContinuousLearningRetainedRootRole.Adapter,
                name,
                maximumBytes: 32)));
        Assert.False(access.ValidateExistingRegularLeaf(
            (ContinuousLearningRetainedRootRole)999,
            validLeaf,
            maximumBytes: 32));
        Assert.False(access.ValidateExistingRegularLeaf(
            ContinuousLearningRetainedRootRole.Adapter,
            validLeaf,
            maximumBytes: 0));
        Assert.False(access.ValidateExistingRegularLeaf(
            ContinuousLearningRetainedRootRole.Adapter,
            validLeaf,
            maximumBytes: -1));
        Assert.False(access.ValidateExistingRegularLeaf(
            ContinuousLearningRetainedRootRole.Adapter,
            "missing.bin",
            maximumBytes: 32));
        Assert.False(access.ValidateExistingRegularLeaf(
            ContinuousLearningRetainedRootRole.Adapter,
            "directory.bin",
            maximumBytes: 32));
        Assert.False(access.ValidateExistingRegularLeaf(
            ContinuousLearningRetainedRootRole.Adapter,
            "reparse.bin",
            maximumBytes: 32));
        Assert.False(access.ValidateExistingRegularLeaf(
            ContinuousLearningRetainedRootRole.Adapter,
            "oversize.bin",
            maximumBytes: 3));
        Assert.False(access.ValidateExistingRegularLeaf(
            ContinuousLearningRetainedRootRole.Adapter,
            "hard-linked.bin",
            maximumBytes: 32));

        access.EnsureRetained();
        Assert.True(startup.ObserveReadiness().RetainedObjectIdentityBound);
    }

    [ContinuousLearningFileAccessWindowsFact]
    public void LeafValidation_PostOpenRootRevalidationMakesRootLossSticky()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        const string leafName = "selected-pair.bin";
        File.WriteAllBytes(Path.Combine(fixture.PairRoot, leafName), [0x41]);
        var startup = fixture.CreateStartup();
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        var rootHandle = GetRetainedEntryHandle(
            access,
            ContinuousLearningRetainedObjectRole.DpoPairRoot);

        var exception = Assert.Throws<ContinuousLearningPathIdentityException>(
            () => access.ValidateExistingRegularLeaf(
                ContinuousLearningRetainedRootRole.DpoPairs,
                leafName,
                maximumBytes: 1,
                afterLeafOpenBeforeRootRevalidationForTesting: rootHandle.Dispose));

        Assert.Equal(
            ContinuousLearningPathIdentityException.StableErrorCode,
            exception.Message);
        Assert.False(startup.Readiness.RetainedObjectIdentityBound);
        Assert.Equal(
            "retained_object_identity_lost_inactive",
            startup.Readiness.ConfigurationStatus);
        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
        Assert.False(startup.ObserveReadiness().RetainedObjectIdentityBound);
    }

    [ContinuousLearningFileAccessWindowsFact]
    public void RetainedEntries_AreStringFreeRoleTaggedIdentityAndConstraintRecords()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var startup = fixture.CreateStartup();
        using var access = startup.AcquireRetainedFileAccess();
        var entries = GetRetainedEntries(access);
        var roles = entries.Select(GetRetainedEntryRole).ToArray();

        Assert.Contains(ContinuousLearningRetainedObjectRole.VolumeOrShareAnchor, roles);
        Assert.Contains(ContinuousLearningRetainedObjectRole.LexicalAncestor, roles);
        Assert.Contains(ContinuousLearningRetainedObjectRole.BaseCheckpoint, roles);
        Assert.Contains(ContinuousLearningRetainedObjectRole.Tokenizer, roles);
        Assert.Contains(ContinuousLearningRetainedObjectRole.AdapterRoot, roles);
        Assert.Contains(ContinuousLearningRetainedObjectRole.CacheRoot, roles);
        Assert.Contains(ContinuousLearningRetainedObjectRole.RegistryRoot, roles);
        Assert.Contains(ContinuousLearningRetainedObjectRole.DpoPairRoot, roles);

        foreach (var entry in entries)
        {
            var entryType = entry.GetType();
            Assert.DoesNotContain(
                entryType.GetFields(BindingFlags.Instance |
                                    BindingFlags.Public |
                                    BindingFlags.NonPublic),
                field => field.FieldType == typeof(string));
            var handle = Assert.IsType<SafeFileHandle>(ReadProperty(entry, "Handle"));
            Assert.False(handle.IsInvalid);
            Assert.False(handle.IsClosed);
            var identity = ReadProperty(entry, "Identity")!;
            Assert.NotNull(identity);
            Assert.DoesNotContain(
                identity.GetType().GetFields(BindingFlags.Instance |
                                             BindingFlags.Public |
                                             BindingFlags.NonPublic),
                field => field.FieldType == typeof(string));
            Assert.NotEqual(0UL, ReadProperty<ulong>(identity, "VolumeSerialNumber"));
            var low = ReadProperty<ulong>(identity, "FileIdLow");
            var high = ReadProperty<ulong>(identity, "FileIdHigh");
            Assert.True(low != 0 || high != 0);

            var kind = ReadProperty<ContinuousLearningRetainedObjectKind>(entry, "Kind");
            var minimum = ReadProperty<long>(entry, "MinimumBytes");
            var maximum = ReadProperty<long>(entry, "MaximumBytes");
            var linkCount = ReadProperty<uint>(entry, "RequiredLinkCount");
            if (kind == ContinuousLearningRetainedObjectKind.RegularFile)
            {
                Assert.Equal(1, minimum);
                Assert.True(maximum > 0);
                Assert.Equal(1U, linkCount);
            }
            else
            {
                Assert.Equal(0, minimum);
                Assert.Equal(0, maximum);
                Assert.Equal(0U, linkCount);
            }
        }
    }

    [ContinuousLearningFileAccessWindowsTheory]
    [InlineData((int)ContinuousLearningRetainedObjectRole.VolumeOrShareAnchor)]
    [InlineData((int)ContinuousLearningRetainedObjectRole.LexicalAncestor)]
    [InlineData((int)ContinuousLearningRetainedObjectRole.BaseCheckpoint)]
    [InlineData((int)ContinuousLearningRetainedObjectRole.Tokenizer)]
    [InlineData((int)ContinuousLearningRetainedObjectRole.AdapterRoot)]
    [InlineData((int)ContinuousLearningRetainedObjectRole.CacheRoot)]
    [InlineData((int)ContinuousLearningRetainedObjectRole.RegistryRoot)]
    [InlineData((int)ContinuousLearningRetainedObjectRole.DpoPairRoot)]
    public void EnsureRetained_RevalidatesEveryStructuralRole(int roleValue)
    {
        RequireWindows();
        var role = (ContinuousLearningRetainedObjectRole)roleValue;
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var startup = fixture.CreateStartup();
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);

        GetRetainedEntryHandle(access, role).Dispose();

        var readiness = startup.ObserveReadiness();
        Assert.False(readiness.RetainedObjectIdentityBound);
        Assert.Equal(
            ContinuousLearningModelServerStartup.RetainedObjectIdentityLostErrorCode,
            readiness.ConfigurationErrorCode);
        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
    }

    [ContinuousLearningFileAccessWindowsFact]
    public async Task ConcurrentObservationLeafValidationAndDispose_InvokeLossOnceOutsideLock()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var junctionProbe = new JunctionProbeTarget(fixture.PairRoot);
        var startup = fixture.CreateStartup();
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        var callbackField = typeof(ServerOwnedContinuousLearningFileAccess).GetField(
            "_onRetainedObjectTrustLost",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var originalCallback = Assert.IsType<Action>(callbackField.GetValue(access));
        var callbackCount = 0;
        var callbackObservedOutsideLock = 0;
        callbackField.SetValue(access, (Action)(() =>
        {
            Interlocked.Increment(ref callbackCount);
            using var disposalCompleted = new ManualResetEventSlim(initialState: false);
            var disposer = new Thread(() =>
            {
                access.Dispose();
                disposalCompleted.Set();
            })
            {
                IsBackground = true,
            };
            disposer.Start();
            if (disposalCompleted.Wait(TimeSpan.FromSeconds(3)))
                Volatile.Write(ref callbackObservedOutsideLock, 1);
            originalCallback();
        }));

        try
        {
            CreateWindowsMetadataOnlyJunctionOrFail(
                fixture.PairRoot,
                junctionProbe.TargetRoot);
            using var start = new ManualResetEventSlim(initialState: false);
            var tasks = Enumerable.Range(0, 24)
                .Select(index => Task.Run(() =>
                {
                    start.Wait();
                    try
                    {
                        switch (index % 4)
                        {
                            case 0:
                                _ = startup.ObserveReadiness();
                                break;
                            case 1:
                                access.EnsureRetained();
                                break;
                            case 2:
                                _ = access.ValidateExistingRegularLeaf(
                                    ContinuousLearningRetainedRootRole.DpoPairs,
                                    "missing.bin",
                                    maximumBytes: 16);
                                break;
                            default:
                                access.Dispose();
                                break;
                        }
                    }
                    catch (ContinuousLearningPathIdentityException)
                    {
                    }
                }))
                .ToArray();
            start.Set();
            await Task.WhenAll(tasks);
        }
        finally
        {
            DeleteWindowsJunctionTagMetadataOnlyOrFail(fixture.PairRoot);
        }

        Assert.Equal(1, Volatile.Read(ref callbackCount));
        Assert.Equal(1, Volatile.Read(ref callbackObservedOutsideLock));
        var lost = startup.ObserveReadiness();
        Assert.False(lost.RetainedObjectIdentityBound);
        Assert.Equal("retained_object_identity_lost_inactive", lost.ConfigurationStatus);
        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
    }

    [ContinuousLearningFileAccessWindowsFact]
    public void CorruptRegistryLockedAdapterAndNonsensicalArtifacts_AreUnchangedAndUnparsed()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        File.WriteAllBytes(
            fixture.BaseCheckpointPath,
            [0xFF, 0x00, 0xFE, 0x81, 0x7F]);
        File.WriteAllBytes(
            fixture.TokenizerPath,
            [0xC0, 0xAF, 0xED, 0xA0, 0x80]);
        var registry = Path.Combine(fixture.RegistryRoot, "registry.json");
        var adapter = Path.Combine(fixture.AdapterRoot, "corrupt.lora");
        File.WriteAllText(registry, "{ not-valid-registry-json }");
        File.WriteAllBytes(adapter, [0xDE, 0xAD, 0xBE, 0xEF]);
        var before = fixture.Snapshot();
        var startup = fixture.CreateStartup();

        using (var lockedRegistry = new FileStream(
                   registry,
                   FileMode.Open,
                   FileAccess.ReadWrite,
                   FileShare.None))
        using (var lockedAdapter = new FileStream(
                   adapter,
                   FileMode.Open,
                   FileAccess.ReadWrite,
                   FileShare.None))
        using (startup.AcquireRetainedFileAccess())
        {
        }

        Assert.Equal(before, fixture.Snapshot());
    }

    [Fact]
    public void LeaseAndActivationSurface_RemainsInternalAndHasNoPublicExecutionApi()
    {
        var assembly = typeof(ContinuousLearningModelServerStartup).Assembly;
        var leaseType = assembly.GetType(
            "ContinuousLearning.ServerOwnedContinuousLearningFileAccess");
        Assert.NotNull(leaseType);
        Assert.False(leaseType!.IsPublic);
        Assert.DoesNotContain(leaseType.GetMethods(
                BindingFlags.Public | BindingFlags.Static),
            method => method.Name.Contains("Acquire", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            assembly.GetExportedTypes(),
            type => type.Name.Contains("FileAccess", StringComparison.OrdinalIgnoreCase)
                    || type.Name.Contains("Handle", StringComparison.OrdinalIgnoreCase)
                    || type.Name.Contains("PathIdentity", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            typeof(ContinuousLearningModelServerStartup).GetMethods(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public),
            method => method.DeclaringType ==
                          typeof(ContinuousLearningModelServerStartup) &&
                      (method.Name.Contains("Path", StringComparison.OrdinalIgnoreCase)
                       || method.Name.Contains("Handle", StringComparison.OrdinalIgnoreCase)
                       || method.Name.Contains("Run", StringComparison.OrdinalIgnoreCase)
                       || method.Name.Contains("Train", StringComparison.OrdinalIgnoreCase)
                       || method.Name.Contains("Cycle", StringComparison.OrdinalIgnoreCase)));
        var nativeImports = leaseType.GetMethods(
                BindingFlags.Static | BindingFlags.NonPublic)
            .Select(method => method.GetCustomAttribute<DllImportAttribute>())
            .Where(attribute => attribute is not null)
            .Select(attribute => attribute!.EntryPoint ?? string.Empty)
            .ToArray();
        Assert.DoesNotContain(
            nativeImports,
            name => name.Contains("DeviceIoControl", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("oplock", StringComparison.OrdinalIgnoreCase));
    }

    private static void RequireWindows()
        => Assert.True(
            OperatingSystem.IsWindows(),
            "R1.1 critical physical-path lease tests require Windows execution.");

    private static void PrepareLinuxOwnerWriteBoundary(FileAccessFixture fixture)
    {
        if (!OperatingSystem.IsLinux())
            return;

        const UnixFileMode directoryMode =
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.UserExecute;
        const UnixFileMode fileMode =
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite;

        // Тесты не проверяют umask хоста. Они явно задают owner-write-only
        // mutation boundary, чтобы POSIX-регрессии мерили security invariant,
        // а не случайные права каталога временных файлов.
        File.SetUnixFileMode(fixture.Root, directoryMode);
        File.SetUnixFileMode(fixture.AdapterRoot, directoryMode);
        File.SetUnixFileMode(fixture.CacheRoot, directoryMode);
        File.SetUnixFileMode(fixture.RegistryRoot, directoryMode);
        if (Directory.Exists(fixture.PairRoot))
            File.SetUnixFileMode(fixture.PairRoot, directoryMode);
        File.SetUnixFileMode(fixture.BaseCheckpointPath, fileMode);
        File.SetUnixFileMode(fixture.TokenizerPath, fileMode);
    }

    private static void SetLinuxFileMode(string path, UnixFileMode mode)
    {
        if (!OperatingSystem.IsLinux())
            return;

        File.SetUnixFileMode(path, mode);
    }

    private static IReadOnlyList<SafeFileHandle> GetRetainedHandles(
        ServerOwnedContinuousLearningFileAccess access)
        => GetRetainedEntries(access)
            .Select(entry => Assert.IsType<SafeFileHandle>(
                ReadProperty(entry, "Handle")))
            .ToArray();

    private static IReadOnlyList<object> GetRetainedEntries(
        ServerOwnedContinuousLearningFileAccess access)
        => Assert.IsAssignableFrom<Array>(
            typeof(ServerOwnedContinuousLearningFileAccess)
                .GetField("_entries", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(access))
            .Cast<object>()
            .ToArray();

    private static SafeFileHandle GetRetainedEntryHandle(
        ServerOwnedContinuousLearningFileAccess access,
        ContinuousLearningRetainedObjectRole role)
        => Assert.IsType<SafeFileHandle>(ReadProperty(
            GetRetainedEntries(access)
                .First(entry => GetRetainedEntryRole(entry) == role),
            "Handle"));

    private static ContinuousLearningRetainedObjectRole GetRetainedEntryRole(
        object entry)
        => ReadProperty<ContinuousLearningRetainedObjectRole>(entry, "Role");

    private static object? ReadProperty(object instance, string name)
        => instance.GetType().GetProperty(
                name,
                BindingFlags.Instance | BindingFlags.Public)!
            .GetValue(instance);

    private static T ReadProperty<T>(object instance, string name)
        => Assert.IsType<T>(ReadProperty(instance, name));

    private static void AssertExactConfiguredPathsNotReachable(
        IEnumerable<object?> roots,
        IReadOnlyCollection<string> exactPaths)
    {
        var pending = new Stack<object?>(roots);
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var continuousLearningAssembly = typeof(ContinuousLearningModelServerStartup).Assembly;
        while (pending.TryPop(out var candidate))
        {
            if (candidate is null)
                continue;
            if (candidate is string text)
            {
                Assert.DoesNotContain(
                    exactPaths,
                    path => string.Equals(path, text, StringComparison.OrdinalIgnoreCase));
                continue;
            }

            var type = candidate.GetType();
            if (!type.IsValueType && !visited.Add(candidate))
                continue;
            Assert.True(visited.Count <= 10_000, "Reachability walk exceeded its bound.");
            if (candidate is Array array)
            {
                foreach (var element in array)
                    pending.Push(element);
                continue;
            }
            if (candidate is Delegate callback)
            {
                foreach (var invocation in callback.GetInvocationList())
                    pending.Push(invocation.Target);
                continue;
            }
            if (type.Assembly != continuousLearningAssembly)
                continue;

            foreach (var field in type.GetFields(
                         BindingFlags.Instance |
                         BindingFlags.Public |
                         BindingFlags.NonPublic))
            {
                pending.Push(field.GetValue(candidate));
            }
        }
    }

    private static void AssertDeleteAccessBlockedExternally(
        string path,
        bool requireDirectory)
    {
        using var handle = OpenDeleteAccess(path, requireDirectory);
        var error = handle.IsInvalid ? Marshal.GetLastPInvokeError() : 0;
        Assert.True(
            handle.IsInvalid,
            "DELETE access unexpectedly succeeded while the host lease was live.");
        Assert.Equal(32, error);
        Assert.True(File.Exists(path) || Directory.Exists(path));
    }

    private static string DescribeExitedProcess(System.Diagnostics.Process process)
        => process.HasExited
            ? $"exit={process.ExitCode}; stdout={process.StandardOutput.ReadToEnd()}; " +
              $"stderr={process.StandardError.ReadToEnd()}"
            : "process-still-running";

    private static void AssertDeleteAccessAvailable(
        string path,
        bool requireDirectory)
    {
        var handle = OpenDeleteAccess(path, requireDirectory);
        try
        {
            Assert.False(
                handle.IsInvalid,
                $"DELETE access failed after disposal with Win32 error {Marshal.GetLastPInvokeError()}.");
        }
        finally
        {
            handle.Dispose();
        }
        Assert.True(handle.IsClosed);
    }

    private static SafeFileHandle OpenDeleteAccess(
        string path,
        bool requireDirectory)
    {
        const uint deleteAccess = 0x00010000;
        const uint openExisting = 3;
        const uint backupSemantics = 0x02000000;
        const uint openReparsePoint = 0x00200000;
        return CreateFileForJunction(
            path,
            deleteAccess,
            (uint)(FileShare.Read | FileShare.Write | FileShare.Delete),
            IntPtr.Zero,
            openExisting,
            openReparsePoint | (requireDirectory ? backupSemantics : 0),
            IntPtr.Zero);
    }

    private static void CreateHardLinkOrFail(string linkPath, string existingPath)
    {
        if (CreateHardLinkWindows(linkPath, existingPath, IntPtr.Zero))
            return;
        var error = Marshal.GetLastPInvokeError();
        Assert.Fail(
            $"Required Windows hard-link setup failed with Win32 error {error}.");
    }

    private static void AssertGenericWriteJunctionMutationBlocked(
        string linkPath,
        string targetPath)
    {
        var succeeded = TryCreateWindowsJunction(
            linkPath,
            targetPath,
            desiredAccess: 0x40000000,
            out var failedPhase,
            out var error);

        Assert.False(
            succeeded,
            "Terminal directory unexpectedly accepted FSCTL_SET_REPARSE_POINT while leased.");
        Assert.Equal("CreateFileW", failedPhase);
        Assert.Equal(32, error);
        Assert.True(Directory.Exists(linkPath));
        Assert.True(
            (File.GetAttributes(linkPath) & FileAttributes.ReparsePoint) == 0);
    }

    private static void CreateWindowsJunctionOrFail(
        string linkPath,
        string targetPath)
    {
        if (TryCreateWindowsJunction(
                linkPath,
                targetPath,
                desiredAccess: 0x40000000,
                out var failedPhase,
                out var error))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Required Windows junction setup failed at {failedPhase} with Win32 error {error}.");
    }

    private static void CreateWindowsMetadataOnlyJunctionOrFail(
        string linkPath,
        string targetPath)
    {
        const uint fileWriteAttributes = 0x00000100;
        if (TryCreateWindowsJunction(
                linkPath,
                targetPath,
                fileWriteAttributes,
                out var failedPhase,
                out var error))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Required metadata-only Windows junction mutation failed at {failedPhase} with Win32 error {error}.");
    }

    private static void DeleteWindowsJunctionTagMetadataOnlyOrFail(string linkPath)
    {
        const uint fileWriteAttributes = 0x00000100;
        const uint openExisting = 3;
        const uint backupSemantics = 0x02000000;
        const uint openReparsePoint = 0x00200000;
        const uint deleteReparsePoint = 0x000900AC;
        const uint mountPointTag = 0xA0000003;

        using var handle = CreateFileForJunction(
            linkPath,
            fileWriteAttributes,
            (uint)(FileShare.Read | FileShare.Write | FileShare.Delete),
            IntPtr.Zero,
            openExisting,
            backupSemantics | openReparsePoint,
            IntPtr.Zero);
        if (handle.IsInvalid)
        {
            throw new InvalidOperationException(
                "Required metadata-only Windows junction cleanup failed at " +
                $"CreateFileW with Win32 error {Marshal.GetLastPInvokeError()}.");
        }

        var buffer = new byte[8];
        using (var memory = new MemoryStream(buffer, writable: true))
        using (var writer = new BinaryWriter(
                   memory,
                   Encoding.Unicode,
                   leaveOpen: true))
        {
            writer.Write(mountPointTag);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
        }
        if (!DeviceIoControlForJunction(
                handle,
                deleteReparsePoint,
                buffer,
                buffer.Length,
                IntPtr.Zero,
                0,
                out _,
                IntPtr.Zero))
        {
            throw new InvalidOperationException(
                "Required metadata-only Windows junction cleanup failed at " +
                $"FSCTL_DELETE_REPARSE_POINT with Win32 error {Marshal.GetLastPInvokeError()}.");
        }
        Assert.True(Directory.Exists(linkPath));
        Assert.True(
            (File.GetAttributes(linkPath) & FileAttributes.ReparsePoint) == 0);
    }

    private static bool TryCreateWindowsJunction(
        string linkPath,
        string targetPath,
        uint desiredAccess,
        out string failedPhase,
        out int error)
    {
        const uint openExisting = 3;
        const uint backupSemantics = 0x02000000;
        const uint openReparsePoint = 0x00200000;
        const uint setReparsePoint = 0x000900A4;
        const uint mountPointTag = 0xA0000003;

        Directory.CreateDirectory(linkPath);
        using var handle = CreateFileForJunction(
            linkPath,
            desiredAccess,
            (uint)(FileShare.Read | FileShare.Write | FileShare.Delete),
            IntPtr.Zero,
            openExisting,
            backupSemantics | openReparsePoint,
            IntPtr.Zero);
        if (handle.IsInvalid)
        {
            error = Marshal.GetLastPInvokeError();
            failedPhase = "CreateFileW";
            return false;
        }

        var printName = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(targetPath));
        var substituteName = @"\??\" + printName;
        var substituteBytes = Encoding.Unicode.GetBytes(substituteName);
        var printBytes = Encoding.Unicode.GetBytes(printName);
        var pathBytes = substituteBytes.Length + sizeof(ushort) +
                        printBytes.Length + sizeof(ushort);
        var reparseDataLength = checked((ushort)(8 + pathBytes));
        var buffer = new byte[8 + reparseDataLength];
        using (var memory = new MemoryStream(buffer, writable: true))
        using (var writer = new BinaryWriter(
                   memory,
                   Encoding.Unicode,
                   leaveOpen: true))
        {
            writer.Write(mountPointTag);
            writer.Write(reparseDataLength);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write(checked((ushort)substituteBytes.Length));
            writer.Write(checked((ushort)(substituteBytes.Length + sizeof(ushort))));
            writer.Write(checked((ushort)printBytes.Length));
            writer.Write(substituteBytes);
            writer.Write((ushort)0);
            writer.Write(printBytes);
            writer.Write((ushort)0);
        }

        if (DeviceIoControlForJunction(
                handle,
                setReparsePoint,
                buffer,
                buffer.Length,
                IntPtr.Zero,
                0,
                out _,
                IntPtr.Zero))
        {
            failedPhase = string.Empty;
            error = 0;
            return true;
        }

        error = Marshal.GetLastPInvokeError();
        failedPhase = "FSCTL_SET_REPARSE_POINT";
        return false;
    }

    private static void RemoveWindowsJunctionIfPresent(string linkPath)
    {
        if (!Directory.Exists(linkPath) && !File.Exists(linkPath))
            return;
        if (!RemoveDirectoryForJunction(linkPath))
        {
            var error = Marshal.GetLastPInvokeError();
            if (error != 3)
            {
                throw new InvalidOperationException(
                    $"Windows junction cleanup failed with Win32 error {error}.");
            }
        }
    }

    private sealed class JunctionProbeTarget : IDisposable
    {
        private readonly string _linkPath;
        private readonly byte[] _sentinelBytes = [0x91, 0x27, 0xE4, 0x5A];
        private readonly string _sentinelPath;

        internal JunctionProbeTarget(string linkPath)
        {
            _linkPath = linkPath;
            TargetRoot = Path.Combine(
                Path.GetTempPath(),
                $"continuous-learning-junction-target-{Guid.NewGuid():N}");
            Directory.CreateDirectory(TargetRoot);
            _sentinelPath = Path.Combine(TargetRoot, "outside-sentinel.bin");
            File.WriteAllBytes(_sentinelPath, _sentinelBytes);
        }

        internal string TargetRoot { get; }
        internal string LinkPath => _linkPath;

        internal void AssertSentinelUnchanged()
            => Assert.Equal(_sentinelBytes, File.ReadAllBytes(_sentinelPath));

        public void Dispose()
        {
            if ((Directory.Exists(_linkPath) || File.Exists(_linkPath))
                && (File.GetAttributes(_linkPath) & FileAttributes.ReparsePoint) != 0)
            {
                RemoveWindowsJunctionIfPresent(_linkPath);
            }
            if (Directory.Exists(TargetRoot))
                Directory.Delete(TargetRoot, recursive: true);
        }
    }

    [DllImport(
        "kernel32.dll",
        EntryPoint = "CreateHardLinkW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateHardLinkWindows(
        string fileName,
        string existingFileName,
        IntPtr securityAttributes);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "CreateFileW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern SafeFileHandle CreateFileForJunction(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeviceIoControlForJunction(
        SafeFileHandle device,
        uint ioControlCode,
        byte[] inputBuffer,
        int inputBufferSize,
        IntPtr outputBuffer,
        int outputBufferSize,
        out int bytesReturned,
        IntPtr overlapped);

    [DllImport("kernel32.dll", EntryPoint = "RemoveDirectoryW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveDirectoryForJunction(string pathName);

}

internal sealed class FileAccessFixture : IDisposable
{
    private string _cleanupRoot;

    public FileAccessFixture(bool provisionAllRoots)
    {
        Root = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-file-access-tests-{Guid.NewGuid():N}");
        _cleanupRoot = Root;
        Directory.CreateDirectory(Root);
        BaseCheckpointPath = Path.Combine(Root, "base-checkpoint.bin");
        TokenizerPath = Path.Combine(Root, "tokenizer.bin");
        AdapterRoot = Path.Combine(Root, "adapter-root");
        CacheRoot = Path.Combine(Root, "cache-root");
        RegistryRoot = Path.Combine(Root, "registry-root");
        RepositoryRoot = Path.Combine(Root, "repository-root");
        CreateRepositorySentinels(RepositoryRoot);
        PairRoot = Path.Combine(
            RepositoryRoot,
            "training-data",
            "dpo",
            "html-css",
            "learning-unit",
            "v0001",
            "pairs");
        File.WriteAllBytes(BaseCheckpointPath, [0x01, 0x02, 0x03]);
        File.WriteAllBytes(TokenizerPath, [0x04, 0x05, 0x06]);
        Directory.CreateDirectory(AdapterRoot);
        Directory.CreateDirectory(CacheRoot);
        Directory.CreateDirectory(RegistryRoot);
        if (provisionAllRoots)
            Directory.CreateDirectory(PairRoot);
    }

    public string Root { get; }
    public string BaseCheckpointPath { get; }
    public string TokenizerPath { get; }
    public string AdapterRoot { get; }
    public string CacheRoot { get; }
    public string RegistryRoot { get; }
    public string RepositoryRoot { get; }
    public string PairRoot { get; }
    public string[] ConfiguredPaths =>
    [
        BaseCheckpointPath,
        TokenizerPath,
        AdapterRoot,
        CacheRoot,
        RegistryRoot,
        PairRoot,
    ];

    public ContinuousLearningModelServerStartup CreateStartup()
    {
        var environment = ValidEnvironment();
        return ContinuousLearningModelServerStartup.Create(name =>
            environment.TryGetValue(name, out var value) ? value : null);
    }

    public IReadOnlyDictionary<string, string> EnvironmentValues()
        => ValidEnvironment();

    public string[] Snapshot()
        => Directory.EnumerateFileSystemEntries(
                Root,
                "*",
                SearchOption.AllDirectories)
            .Select(path => File.Exists(path)
                ? $"F|{Path.GetRelativePath(Root, path)}|" +
                  Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))
                : $"D|{Path.GetRelativePath(Root, path)}")
            .Order(StringComparer.Ordinal)
            .ToArray();

    public void AssertFileRenameable(string path)
    {
        var moved = path + ".rename-probe";
        MoveRoundTrip(path, moved);
    }

    public void AssertDirectoryRenameable(string path)
    {
        var moved = path + ".rename-probe";
        MoveRoundTrip(path, moved);
    }

    public void AssertPostDisposeOneWayRenames()
    {
        var movedFile = BaseCheckpointPath + ".post-dispose";
        var movedTokenizer = TokenizerPath + ".post-dispose";
        var movedDirectory = AdapterRoot + ".post-dispose";
        var movedRoot = Root + ".post-dispose";
        MoveOneWay(BaseCheckpointPath, movedFile);
        MoveOneWay(TokenizerPath, movedTokenizer);
        MoveOneWay(AdapterRoot, movedDirectory);
        MoveOneWay(Root, movedRoot);
        _cleanupRoot = movedRoot;
        Assert.True(File.Exists(Path.Combine(
            movedRoot,
            Path.GetFileName(movedFile))));
        Assert.True(File.Exists(Path.Combine(
            movedRoot,
            Path.GetFileName(movedTokenizer))));
        Assert.True(Directory.Exists(Path.Combine(
            movedRoot,
            Path.GetFileName(movedDirectory))));
    }

    public void Dispose()
    {
        if (Directory.Exists(_cleanupRoot))
            Directory.Delete(_cleanupRoot, recursive: true);
    }

    private static void MoveRoundTrip(string path, string moved)
    {
        Assert.True(
            MoveFileWindows(path, moved),
            $"Rename probe failed with Win32 error {Marshal.GetLastPInvokeError()}.");
        Assert.True(
            MoveFileWindows(moved, path),
            $"Rename restore failed with Win32 error {Marshal.GetLastPInvokeError()}.");
    }

    private static void MoveOneWay(string path, string moved)
        => Assert.True(
            MoveFileWindows(path, moved),
            $"One-way rename failed with Win32 error {Marshal.GetLastPInvokeError()}.");

    [DllImport(
        "kernel32.dll",
        EntryPoint = "MoveFileW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool MoveFileWindows(
        string existingFileName,
        string newFileName);

    private Dictionary<string, string> ValidEnvironment()
        => new(StringComparer.Ordinal)
        {
            [ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName]
                = bool.TrueString,
            [ContinuousLearningModelServerConfigurationLoader.ModelIdEnvironmentName]
                = "qwen3.5:9b",
            [ContinuousLearningModelServerConfigurationLoader.OllamaManifestDigestEnvironmentName]
                = new string('d', 64),
            [ContinuousLearningModelServerConfigurationLoader.OllamaModelLayerDigestEnvironmentName]
                = new string('c', 64),
            [ContinuousLearningModelServerConfigurationLoader.BaseModelIdEnvironmentName]
                = "registry/qwen3.5-base-9b",
            [ContinuousLearningModelServerConfigurationLoader.BaseModelRevisionEnvironmentName]
                = "base-checkpoint-r17",
            [ContinuousLearningModelServerConfigurationLoader.MaxTeacherCallsEnvironmentName]
                = "100",
            [ContinuousLearningModelServerConfigurationLoader.AuditSaltEnvironmentName]
                = Convert.ToBase64String(Enumerable.Range(1, 32)
                    .Select(static value => checked((byte)value))
                    .ToArray()),
            [ContinuousLearningModelServerConfigurationLoader.LocalRetentionPolicyRefEnvironmentName]
                = $"sha256:{new string('a', 64)}",
            [ContinuousLearningModelServerConfigurationLoader.OutputLicenseRefEnvironmentName]
                = $"sha256:{new string('b', 64)}",
            [ContinuousLearningModelServerConfigurationLoader.BaseCheckpointEnvironmentName]
                = BaseCheckpointPath,
            [ContinuousLearningModelServerConfigurationLoader.TokenizerEnvironmentName]
                = TokenizerPath,
            [ContinuousLearningModelServerConfigurationLoader.AdapterRootEnvironmentName]
                = AdapterRoot,
            [ContinuousLearningModelServerConfigurationLoader.CacheRootEnvironmentName]
                = CacheRoot,
            [ContinuousLearningModelServerConfigurationLoader.RegistryRootEnvironmentName]
                = RegistryRoot,
            [ContinuousLearningModelServerConfigurationLoader.RepositoryRootEnvironmentName]
                = RepositoryRoot,
            [ContinuousLearningModelServerConfigurationLoader.DpoPairRootEnvironmentName]
                = PairRoot,
            [ContinuousLearningModelServerConfigurationLoader.RepositoryModelIdEnvironmentName]
                = "qwen3-5-9b-dpo",
            [ContinuousLearningModelServerConfigurationLoader.RepositoryModelVersionEnvironmentName]
                = "v0001",
            [ContinuousLearningModelServerConfigurationLoader.RuntimeConfigSha256EnvironmentName]
                = new string('1', 64),
            [ContinuousLearningModelServerConfigurationLoader.RuntimeTopologySha256EnvironmentName]
                = new string('2', 64),
            [ContinuousLearningModelServerConfigurationLoader.RuntimeTokenizerSha256EnvironmentName]
                = new string('3', 64),
            [ContinuousLearningModelServerConfigurationLoader.RuntimeTrainerSha256EnvironmentName]
                = new string('4', 64),
            [ContinuousLearningModelServerConfigurationLoader.RuntimeSchemaSha256EnvironmentName]
                = new string('5', 64),
            [ContinuousLearningModelServerConfigurationLoader.RuntimeCompatibilityKeyEnvironmentName]
                = "tp1-dpo-runtime-v0001",
            [ContinuousLearningModelServerConfigurationLoader.ModelVocabSizeEnvironmentName] = "256",
            [ContinuousLearningModelServerConfigurationLoader.ModelContextLenEnvironmentName] = "8",
            [ContinuousLearningModelServerConfigurationLoader.ModelHiddenDimEnvironmentName] = "8",
            [ContinuousLearningModelServerConfigurationLoader.ModelNumLayersEnvironmentName] = "1",
            [ContinuousLearningModelServerConfigurationLoader.ModelNumHeadsEnvironmentName] = "1",
            [ContinuousLearningModelServerConfigurationLoader.ModelNumKvHeadsEnvironmentName] = "1",
            [ContinuousLearningModelServerConfigurationLoader.ModelFfnHiddenDimEnvironmentName] = "16",
            [ContinuousLearningModelServerConfigurationLoader.ModelRopeThetaEnvironmentName] = "10000",
            [ContinuousLearningModelServerConfigurationLoader.ModelRmsNormEpsEnvironmentName] = "1E-05",
            [ContinuousLearningModelServerConfigurationLoader.TrainingBatchSizeEnvironmentName] = "1",
            [ContinuousLearningModelServerConfigurationLoader.TrainingSeqLenEnvironmentName] = "8",
            [ContinuousLearningModelServerConfigurationLoader.TrainingMaxLrEnvironmentName] = "0.01",
            [ContinuousLearningModelServerConfigurationLoader.TrainingMinLrEnvironmentName] = "0.001",
            [ContinuousLearningModelServerConfigurationLoader.TrainingWarmupStepsEnvironmentName] = "0",
            [ContinuousLearningModelServerConfigurationLoader.TrainingTotalStepsEnvironmentName] = "10",
            [ContinuousLearningModelServerConfigurationLoader.TrainingGradAccumStepsEnvironmentName] = "1",
            [ContinuousLearningModelServerConfigurationLoader.TrainingGradClipNormEnvironmentName] = "1",
            [ContinuousLearningModelServerConfigurationLoader.TrainingWeightDecayEnvironmentName] = "0",
            [ContinuousLearningModelServerConfigurationLoader.TrainingCheckpointEveryEnvironmentName] = "1",
            [ContinuousLearningModelServerConfigurationLoader.TrainingLogEveryEnvironmentName] = "1",
        };

    private static void CreateRepositorySentinels(string repositoryRoot)
    {
        Directory.CreateDirectory(repositoryRoot);
        File.WriteAllText(
            Path.Combine(repositoryRoot, "LlmFromScratch.sln"),
            "fixture solution sentinel");
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "11.DPO"));
    }
}

file sealed class ContinuousLearningFileAccessLinuxX64FactAttribute : FactAttribute
{
    public ContinuousLearningFileAccessLinuxX64FactAttribute()
    {
        if (!OperatingSystem.IsLinux())
        {
            Skip = "Только Linux: регрессия доказывает POSIX openat/no-follow retained custody.";
            return;
        }

        if (RuntimeInformation.ProcessArchitecture != Architecture.X64)
        {
            Skip = "Только Linux x64: native stat layout fail-closed на других архитектурах.";
        }
    }
}

file sealed class ContinuousLearningFileAccessWindowsFactAttribute : FactAttribute
{
    public ContinuousLearningFileAccessWindowsFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            // Обычные file-access факты в этом файле проверяют Windows handle
            // leases и delete-sharing. Для Linux выше есть отдельные POSIX-
            // факты, поэтому вне Windows эти проверки должны быть NotExecuted.
            Skip = "WINDOWS_ONLY_CAPABILITY_ORACLE: R1.1 critical physical-path lease tests require Windows execution.";
        }
    }
}

file sealed class ContinuousLearningFileAccessWindowsTheoryAttribute : TheoryAttribute
{
    public ContinuousLearningFileAccessWindowsTheoryAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            // Все строки ролей зависят от Windows retained-handle lease.
            // Пропуск при обнаружении не даёт полному WSL-прогону создавать
            // ложные красные падения.
            Skip = "WINDOWS_ONLY_CAPABILITY_ORACLE: R1.1 critical physical-path lease tests require Windows execution.";
        }
    }
}
