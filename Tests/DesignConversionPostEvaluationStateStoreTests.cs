// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using DesignConversionDomain;

public sealed class DesignConversionPostEvaluationStateStoreTests
{
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);

    [Fact]
    public async Task WriteReadAndEligibility_DurableCommittedStateBindsExactArtifactRecord()
    {
        var artifactRoot = NewRoot("committed-positive");
        var store = NewArtifactStore(artifactRoot);
        var initial = InitialReservedState("committed-positive");

        var created = await store.WriteAsync(initial);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        Assert.Equal(1, created.Value!.Version);

        var attempted = await store.WriteAsync(created.Value with
        {
            SettlementAttempted = true,
            UpdatedAtUtc = created.Value.UpdatedAtUtc.AddTicks(1)
        });
        Assert.True(attempted.IsSuccess, attempted.ErrorMessage);
        Assert.Equal(2, attempted.Value!.Version);

        var settled = await store.WriteAsync(attempted.Value with
        {
            AccountingStatus = JsonDesignConversionPostEvaluationStateStore.AccountingSettled,
            AccountingDesiredAction = JsonDesignConversionPostEvaluationStateStore.DesiredNone,
            UpdatedAtUtc = attempted.Value.UpdatedAtUtc.AddTicks(1)
        });
        Assert.True(settled.IsSuccess, settled.ErrorMessage);

        var committed = await store.WriteAsync(settled.Value! with
        {
            PersistenceStatus = JsonDesignConversionPostEvaluationStateStore.PersistenceCommitted,
            LedgerRecordId = "ledger-record-001",
            ArtifactRecordId = "artifact-record-001",
            CompletionBindingKind =
                JsonDesignConversionPostEvaluationStateStore
                    .CompletionBindingLocalArtifactDbRecord,
            CompletionBindingRef = "artifact-record-001",
            UpdatedAtUtc = settled.Value!.UpdatedAtUtc.AddTicks(1)
        });
        Assert.True(committed.IsSuccess, committed.ErrorMessage);
        Assert.Equal(4, committed.Value!.Version);

        var read = await new JsonDesignConversionPostEvaluationStateStore(store.RootDirectory)
            .ReadAsync(initial.StateRef);
        Assert.True(read.IsSuccess, read.ErrorMessage);
        Assert.Equal(committed.Value, read.Value);
        Assert.True(await JsonDesignConversionPostEvaluationStateStore.IsCommittedForTrainingAsync(
            artifactRoot,
            initial.StateRef,
            "artifact-record-001"));
        Assert.False(await JsonDesignConversionPostEvaluationStateStore.IsCommittedForTrainingAsync(
            artifactRoot,
            initial.StateRef,
            "artifact-record-substitution"));
    }

    [Fact]
    public async Task WriteAsync_RepoRootV9fsPublishKeepsStateReadableAndTempFree()
    {
        var root = NewRepoRootStateRoot("post-eval-v9fs-publish");
        var state = InitialNoReservationState("post-eval-v9fs-publish");
        var stageTrace = new List<string>();
        DesignConversionHandleBoundIoStage? lastPublicationStage = null;
        var options = DesignConversionPostEvaluationStateStoreOptions.Default with
        {
            BeforeHandleIoForTesting = (stage, path) =>
            {
                // Диагностика хранит последнюю удержанную handle-операцию:
                // на `/mnt/c` это отличает renameat2-сбой от trust/inventory.
                stageTrace.Add($"{stage}:{path}");
                if (stage is
                    DesignConversionHandleBoundIoStage.BeforeAtomicRename or
                    DesignConversionHandleBoundIoStage
                        .AfterAtomicRenameBeforeDirectoryFlush)
                {
                    lastPublicationStage = stage;
                }
            }
        };
        var store = new JsonDesignConversionPostEvaluationStateStore(root, options);

        var written = await store.WriteAsync(state);

        var lastStage = lastPublicationStage?.ToString() ??
                        (stageTrace.Count == 0 ? "<none>" : stageTrace[^1]);
        Assert.True(
            written.IsSuccess,
            $"root={root}; errorCode={written.ErrorCode}; " +
            $"errorMessage={written.ErrorMessage}; lastStage={lastStage}; " +
            $"stageTrace={string.Join(" | ", stageTrace)}");
        var read = await new JsonDesignConversionPostEvaluationStateStore(root)
            .ReadAsync(state.StateRef);
        Assert.True(read.IsSuccess, read.ErrorMessage);
        Assert.Equal(written.Value, read.Value);
        var listed = await new JsonDesignConversionPostEvaluationStateStore(root)
            .ListAsync();
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        Assert.Single(listed.Value!);
        Assert.Contains(
            stageTrace,
            item => item.StartsWith(
                DesignConversionHandleBoundIoStage.BeforeDirectoryMetadataFlush +
                ":",
                StringComparison.Ordinal));
        Assert.Empty(Directory.EnumerateFiles(
            root,
            "*" + JsonDesignConversionPostEvaluationStateStore.WriteTempSuffix));
    }

    [Fact]
    public async Task WriteAsync_RepoRootV9fsFallbackCrashWindowReconcilesSameIdentityTemp()
    {
        if (!OperatingSystem.IsLinux())
            return;

        var root = NewRepoRootStateRoot("post-eval-v9fs-link-crash");
        var state = InitialNoReservationState("post-eval-v9fs-link-crash");
        var sawBeforeAtomicRename = 0;
        var sawAfterAtomicRename = 0;
        var injected = 0;
        var options = DesignConversionPostEvaluationStateStoreOptions.Default with
        {
            BeforeHandleIoForTesting = (stage, _) =>
            {
                if (stage == DesignConversionHandleBoundIoStage.BeforeAtomicRename)
                    Interlocked.Exchange(ref sawBeforeAtomicRename, 1);
                if (stage ==
                    DesignConversionHandleBoundIoStage
                        .AfterAtomicRenameBeforeDirectoryFlush)
                {
                    Interlocked.Exchange(ref sawAfterAtomicRename, 1);
                }
                if (stage ==
                        DesignConversionHandleBoundIoStage
                            .BeforeDirectoryMetadataFlush &&
                    Volatile.Read(ref sawBeforeAtomicRename) == 1 &&
                    Volatile.Read(ref sawAfterAtomicRename) == 0 &&
                    Interlocked.Exchange(ref injected, 1) == 0)
                {
                    // На v9fs это моделирует падение после `linkat`, но до
                    // `unlinkat`: оба имени должны остаться одним inode.
                    throw new IOException(
                        "injected-v9fs-link-before-unlink-crash-window");
                }
            }
        };

        var failed = await new JsonDesignConversionPostEvaluationStateStore(root, options)
            .WriteAsync(state);

        Assert.False(failed.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.WriteFailedErrorCode,
            failed.ErrorCode);
        Assert.Equal(1, Volatile.Read(ref injected));
        Assert.Equal(0, Volatile.Read(ref sawAfterAtomicRename));
        var destinationPath = StatePath(root, state.StateRef);
        var tempPath = Assert.Single(Directory.EnumerateFiles(
            root,
            "*" + JsonDesignConversionPostEvaluationStateStore.WriteTempSuffix));
        var destinationLeaf = Path.GetFileName(destinationPath);
        var tempLeaf = Path.GetFileName(tempPath);
        Assert.Equal(
            CaptureEntry(root, destinationLeaf).Identity,
            CaptureEntry(root, tempLeaf).Identity);
        Assert.Equal(2U, CaptureLinkCount(root, destinationLeaf));
        Assert.Equal(2U, CaptureLinkCount(root, tempLeaf));

        var restarted = await new JsonDesignConversionPostEvaluationStateStore(root)
            .ReadAsync(state.StateRef);

        Assert.True(restarted.IsSuccess, restarted.ErrorMessage);
        Assert.Equal(state.StateRef, restarted.Value!.StateRef);
        Assert.Empty(Directory.EnumerateFiles(
            root,
            "*" + JsonDesignConversionPostEvaluationStateStore.WriteTempSuffix));
        Assert.Equal(1U, CaptureLinkCount(root, destinationLeaf));
    }

    [Fact]
    public async Task WriteAsync_RepoRootV9fsFallbackCollisionPreservesForeignDestination()
    {
        var root = NewRepoRootStateRoot("post-eval-v9fs-collision");
        var state = InitialNoReservationState("post-eval-v9fs-collision");
        var destinationPath = StatePath(root, state.StateRef);
        var foreignBytes = StrictUtf8.GetBytes("foreign-v9fs-destination");
        var injected = 0;
        var options = DesignConversionPostEvaluationStateStoreOptions.Default with
        {
            BeforeHandleIoForTesting = (stage, path) =>
            {
                if (stage != DesignConversionHandleBoundIoStage.BeforeAtomicRename ||
                    !string.Equals(
                        Path.GetFullPath(path),
                        Path.GetFullPath(destinationPath),
                        StringComparison.OrdinalIgnoreCase) ||
                    Interlocked.Exchange(ref injected, 1) != 0)
                {
                    return;
                }

                // Чужой destination появляется прямо перед публикацией:
                // fallback обязан сохранить его байты и не оставлять backup.
                File.WriteAllBytes(destinationPath, foreignBytes);
            }
        };

        var failed = await new JsonDesignConversionPostEvaluationStateStore(root, options)
            .WriteAsync(state);

        Assert.False(failed.IsSuccess);
        Assert.Equal(1, Volatile.Read(ref injected));
        Assert.Equal(foreignBytes, await File.ReadAllBytesAsync(destinationPath));
        Assert.Empty(Directory.EnumerateFiles(
            root,
            "*" + JsonDesignConversionPostEvaluationStateStore.WriteTempSuffix));
        Assert.False(File.Exists(
            destinationPath + DesignConversionHandleBoundDirectory.PublicationBackupSuffix));
    }

    [Fact]
    public async Task WriteAsync_RepoRootV9fsFallbackRejectsHardLinkedOwnedTemp()
    {
        if (!OperatingSystem.IsLinux())
            return;

        var root = NewRepoRootStateRoot("post-eval-v9fs-temp-hardlink");
        var state = InitialNoReservationState("post-eval-v9fs-temp-hardlink");
        string? tempPath = null;
        string? hardLinkPath = null;
        var hardLinked = 0;
        var sawAfterAtomicRename = 0;
        var options = DesignConversionPostEvaluationStateStoreOptions.Default with
        {
            BeforeHandleIoForTesting = (stage, path) =>
            {
                if (stage == DesignConversionHandleBoundIoStage.LeafBeforeOpen &&
                    path.EndsWith(
                        JsonDesignConversionPostEvaluationStateStore
                            .WriteTempSuffix,
                        StringComparison.Ordinal))
                {
                    tempPath = path;
                }
                if (stage == DesignConversionHandleBoundIoStage.BeforeAtomicRename &&
                    tempPath is not null &&
                    Interlocked.Exchange(ref hardLinked, 1) == 0)
                {
                    hardLinkPath = tempPath + ".hardlink";
                    // Дополнительная hardlink делает temp не owned-by-writer;
                    // publish должен остановиться до canonical destination.
                    Assert.True(
                        TryCreateHardLink(tempPath, hardLinkPath),
                        "test setup must create the extra hardlink");
                }
                if (stage ==
                    DesignConversionHandleBoundIoStage
                        .AfterAtomicRenameBeforeDirectoryFlush)
                {
                    Interlocked.Exchange(ref sawAfterAtomicRename, 1);
                }
            }
        };

        var failed = await new JsonDesignConversionPostEvaluationStateStore(root, options)
            .WriteAsync(state);

        Assert.False(failed.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.WriteFailedErrorCode,
            failed.ErrorCode);
        Assert.Equal(1, Volatile.Read(ref hardLinked));
        Assert.Equal(0, Volatile.Read(ref sawAfterAtomicRename));
        Assert.NotNull(tempPath);
        Assert.NotNull(hardLinkPath);
        Assert.True(File.Exists(tempPath));
        Assert.True(File.Exists(hardLinkPath));
        Assert.False(File.Exists(StatePath(root, state.StateRef)));
        Assert.Equal(
            CaptureEntry(root, Path.GetFileName(tempPath!)).Identity,
            CaptureEntry(root, Path.GetFileName(hardLinkPath!)).Identity);
    }

    [Theory]
    [InlineData("root-symlink")]
    [InlineData("lock-symlink")]
    [InlineData("state-symlink")]
    [InlineData("temp-symlink")]
    [InlineData("state-directory")]
    [InlineData("temp-directory")]
    public async Task WriteAsync_RepoRootV9fsUnsafeNamespaceEntryFailsClosed(
        string mutation)
    {
        if (!OperatingSystem.IsLinux())
            return;

        var root = NewRepoRootStateRoot("post-eval-v9fs-" + mutation);
        var state = InitialNoReservationState("post-eval-v9fs-" + mutation);
        var outsideRoot = NewRepoRootStateRoot("post-eval-v9fs-outside-" + mutation);
        Directory.CreateDirectory(outsideRoot);
        var outsidePath = Path.Combine(outsideRoot, "outside");
        var outsideBytes = StrictUtf8.GetBytes("outside-" + mutation);
        string? directoryPath = null;
        // Каждая mutation имитирует namespace entry, который нельзя считать
        // обычным state/tmp файлом внутри удержанного root.
        switch (mutation)
        {
            case "root-symlink":
            {
                var parent =
                    DesignConversionPostEvaluationEligibilityTestFixture
                        .CreateHandleBoundTempRoot("post-eval-v9fs-root-link");
                Directory.CreateDirectory(parent);
                var target = Path.Combine(parent, "target");
                Directory.CreateDirectory(target);
                root = Path.Combine(parent, "linked-state-root");
                Directory.CreateSymbolicLink(root, target);
                directoryPath = target;
                break;
            }
            case "lock-symlink":
                Directory.CreateDirectory(root);
                await File.WriteAllBytesAsync(outsidePath, outsideBytes);
                File.CreateSymbolicLink(
                    Path.Combine(
                        root,
                        JsonDesignConversionPostEvaluationStateStore
                            .MutationLockFileName),
                    outsidePath);
                break;
            case "state-symlink":
                Directory.CreateDirectory(root);
                await File.WriteAllBytesAsync(outsidePath, outsideBytes);
                File.CreateSymbolicLink(StatePath(root, state.StateRef), outsidePath);
                break;
            case "temp-symlink":
                Directory.CreateDirectory(root);
                await File.WriteAllBytesAsync(outsidePath, outsideBytes);
                File.CreateSymbolicLink(
                    Path.Combine(root, BuildTestTempLeaf(state.StateRef)),
                    outsidePath);
                break;
            case "state-directory":
                Directory.CreateDirectory(root);
                directoryPath = StatePath(root, state.StateRef);
                Directory.CreateDirectory(directoryPath);
                break;
            case "temp-directory":
                Directory.CreateDirectory(root);
                directoryPath = Path.Combine(root, BuildTestTempLeaf(state.StateRef));
                Directory.CreateDirectory(directoryPath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null);
        }

        var result = await new JsonDesignConversionPostEvaluationStateStore(root)
            .WriteAsync(state);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
            result.ErrorCode);
        if (File.Exists(outsidePath))
            Assert.Equal(outsideBytes, await File.ReadAllBytesAsync(outsidePath));
        if (directoryPath is not null)
            Assert.True(Directory.Exists(directoryPath));
    }

    [Fact]
    public void HandleBoundDirectory_RepoRootLinuxRejectsLeafNameAboveUtf8NameMax()
    {
        if (!OperatingSystem.IsLinux())
            return;

        var root = NewRepoRootStateRoot("post-eval-v9fs-name-max");
        using var directory =
            DesignConversionHandleBoundDirectory.AcquireOrCreate(
                root,
                beforeIo: null);
        var oversizedLeaf = new string('é', 128);

        // Linux NAME_MAX считается в UTF-8 байтах; это input validation до
        // syscall, поэтому ожидаем ArgumentException, а не storage corruption.
        var ex = Assert.Throws<ArgumentException>(() =>
            directory.CreateNew(
                oversizedLeaf,
                FileShare.Read,
                writeThrough: true));

        Assert.Contains("255-byte UTF-8", ex.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("before-delete")]
    [InlineData("after-delete")]
    public async Task DurableTrainingDenial_ReleaseFailureRestoresFailClosedAuthorityAcrossRestart(
        string failureStage)
    {
        var artifactRoot = NewRoot("training-denial-" + failureStage);
        var normal = NewArtifactStore(artifactRoot);
        var initial = await normal.WriteAsync(
            InitialNoReservationState(
                "training-denial-" + failureStage));
        Assert.True(initial.IsSuccess, initial.ErrorMessage);
        var denied = await normal.DenyTrainingEligibilityAsync(
            initial.Value!.StateRef,
            "artifact-training-denial",
            "TEST_COMMIT_PENDING");
        Assert.True(denied.IsSuccess, denied.ErrorMessage);
        var committed = await normal.WriteAsync(
            initial.Value with
            {
                PersistenceStatus =
                    JsonDesignConversionPostEvaluationStateStore
                        .PersistenceCommitted,
                ArtifactRecordId = "artifact-training-denial",
                CompletionBindingKind =
                    JsonDesignConversionPostEvaluationStateStore
                        .CompletionBindingLocalArtifactDbRecord,
                CompletionBindingRef = "artifact-training-denial",
                UpdatedAtUtc = initial.Value.UpdatedAtUtc.AddTicks(1),
                Version = initial.Value.Version
            });
        Assert.True(committed.IsSuccess, committed.ErrorMessage);

        var options =
            DesignConversionPostEvaluationStateStoreOptions.Default with
            {
                BeforeTrainingEligibilityDenialDeleteForTesting =
                    failureStage == "before-delete"
                        ? _ => throw new IOException(
                            "injected-denial-release-before-delete")
                        : null,
                AfterTrainingEligibilityDenialDeleteForTesting =
                    failureStage == "after-delete"
                        ? _ => throw new IOException(
                            "injected-denial-release-after-delete")
                        : null
            };
        var failingRelease =
            new JsonDesignConversionPostEvaluationStateStore(
                normal.RootDirectory,
                options);

        var release =
            await failingRelease
                .ReleaseTrainingEligibilityDenialAsync(
                    initial.Value.StateRef,
                    "artifact-training-denial");
        var restarted =
            new JsonDesignConversionPostEvaluationStateStore(
                normal.RootDirectory);
        var inventory = await restarted.ListAsync();

        Assert.False(release.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore
                .TrainingEligibilityDenyReleaseFailedErrorCode,
            release.ErrorCode);
        Assert.True(inventory.IsSuccess, inventory.ErrorMessage);
        Assert.True(Assert.Single(inventory.Value!).TrainingEligibilityDenied);
        Assert.False(
            await JsonDesignConversionPostEvaluationStateStore
                .IsCommittedForTrainingAsync(
                    artifactRoot,
                    initial.Value.StateRef,
                    "artifact-training-denial"));
    }

    [Fact]
    public async Task CorruptTrainingDenialMarker_FailsEligibilityInventoryClosed()
    {
        var artifactRoot = NewRoot("training-denial-corrupt");
        var store = NewArtifactStore(artifactRoot);
        var initial = await store.WriteAsync(
            InitialNoReservationState("training-denial-corrupt"));
        Assert.True(initial.IsSuccess, initial.ErrorMessage);
        Assert.True(
            (await store.DenyTrainingEligibilityAsync(
                initial.Value!.StateRef,
                "artifact-training-denial-corrupt",
                "TEST_COMMIT_PENDING")).IsSuccess);
        var denialDirectory =
            store.RootDirectory +
            JsonDesignConversionPostEvaluationStateStore
                .TrainingEligibilityDenialDirectorySuffix;
        var denialPath = Assert.Single(
            Directory.EnumerateFiles(
                denialDirectory,
                "*.deny.json",
                SearchOption.TopDirectoryOnly));
        await File.WriteAllTextAsync(
            denialPath,
            "{\"schemaVersion\":\"corrupt\"}");

        var inventory =
            await new JsonDesignConversionPostEvaluationStateStore(
                    store.RootDirectory)
                .ListAsync();

        Assert.False(inventory.IsSuccess);
        Assert.False(
            await JsonDesignConversionPostEvaluationStateStore
                .IsCommittedForTrainingAsync(
                    artifactRoot,
                    initial.Value.StateRef,
                    "artifact-training-denial-corrupt"));
    }

    [Theory]
    [InlineData("unknown-property")]
    [InlineData("duplicate-property")]
    public async Task TrainingDenialMarker_StrictSchemaMutationFailsListAndCentralEligibilityClosed(
        string mutation)
    {
        var fixture = await SeedCommittedTrainingDenialAsync(
            "training-denial-strict-" + mutation);
        var json = await File.ReadAllTextAsync(
            fixture.DenialPath,
            StrictUtf8);
        var mutated = mutation switch
        {
            "unknown-property" =>
                json[..^1] + ",\"unexpected\":\"must-fail\"}",
            "duplicate-property" =>
                json[..^1] +
                ",\"schemaVersion\":\"" +
                JsonDesignConversionPostEvaluationStateStore
                    .TrainingEligibilityDenialSchemaVersion +
                "\"}",
            _ => throw new ArgumentOutOfRangeException(
                nameof(mutation),
                mutation,
                null)
        };
        await File.WriteAllTextAsync(
            fixture.DenialPath,
            mutated,
            StrictUtf8);

        var restarted = NewArtifactStore(fixture.ArtifactRoot);
        var listed = await restarted.ListAsync();

        Assert.False(listed.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.ReadFailedErrorCode,
            listed.ErrorCode);
        Assert.False(
            await JsonDesignConversionPostEvaluationStateStore
                .IsCommittedForTrainingAsync(
                    fixture.ArtifactRoot,
                    fixture.State.StateRef,
                    fixture.ArtifactRecordId));
    }

    [Theory]
    [InlineData(
        nameof(DesignConversionHandleBoundIoStage.BeforeDelete),
        false,
        true)]
    [InlineData(
        nameof(DesignConversionHandleBoundIoStage
            .AfterDeleteMarkBeforeDirectoryFlush),
        false,
        true)]
    [InlineData(
        nameof(DesignConversionHandleBoundIoStage
            .BeforeDirectoryMetadataFlush),
        false,
        true)]
    [InlineData(
        nameof(DesignConversionHandleBoundIoStage.AfterDurableDelete),
        true,
        false)]
    public async Task TrainingDenialRelease_FaultBoundaryHasExactOuterAndRestartSemantics(
        string injectedStageName,
        bool expectedReleaseSuccess,
        bool expectedRestartDenied)
    {
        var injectedStage =
            Enum.Parse<DesignConversionHandleBoundIoStage>(
                injectedStageName);
        var fixture = await SeedCommittedTrainingDenialAsync(
            "training-denial-release-fault-" + injectedStage);
        var primaryDeleteStarted = false;
        var options =
            DesignConversionPostEvaluationStateStoreOptions.Default with
            {
                BeforeTrainingEligibilityDenialHandleIoForTesting =
                    (stage, path) =>
                    {
                        var isPrimary =
                            string.Equals(
                                path,
                                fixture.DenialPath,
                                StringComparison.OrdinalIgnoreCase);
                        if (stage ==
                                DesignConversionHandleBoundIoStage
                                    .BeforeDelete &&
                            isPrimary)
                        {
                            primaryDeleteStarted = true;
                        }

                        var shouldFault =
                            stage == injectedStage &&
                            (injectedStage ==
                                DesignConversionHandleBoundIoStage
                                    .BeforeDirectoryMetadataFlush
                                ? primaryDeleteStarted
                                : isPrimary);
                        if (!shouldFault)
                        {
                            return;
                        }

                        throw new IOException(
                            "injected-training-denial-release-" +
                            injectedStage);
                    }
            };
        var faultingStore =
            new JsonDesignConversionPostEvaluationStateStore(
                fixture.Store.RootDirectory,
                options);

        var release =
            await faultingStore.ReleaseTrainingEligibilityDenialAsync(
                fixture.State.StateRef,
                fixture.ArtifactRecordId);
        var restarted = NewArtifactStore(fixture.ArtifactRoot);
        var listed = await restarted.ListAsync();

        Assert.Equal(expectedReleaseSuccess, release.IsSuccess);
        if (!expectedReleaseSuccess)
        {
            Assert.Equal(
                JsonDesignConversionPostEvaluationStateStore
                    .TrainingEligibilityDenyReleaseFailedErrorCode,
                release.ErrorCode);
        }

        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        Assert.Equal(
            expectedRestartDenied,
            Assert.Single(listed.Value!).TrainingEligibilityDenied);
        Assert.Equal(
            !expectedRestartDenied,
            await JsonDesignConversionPostEvaluationStateStore
                .IsCommittedForTrainingAsync(
                    fixture.ArtifactRoot,
                    fixture.State.StateRef,
                    fixture.ArtifactRecordId));
    }

    [Fact]
    public async Task TrainingDenialRelease_WitnessCleanupFailureFailsClosedAndRetryReconciles()
    {
        var fixture = await SeedCommittedTrainingDenialAsync(
            "training-denial-witness-cleanup");
        var primaryDurablyDeleted = false;
        var options =
            DesignConversionPostEvaluationStateStoreOptions.Default with
            {
                BeforeTrainingEligibilityDenialHandleIoForTesting =
                    (stage, path) =>
                    {
                        if (stage ==
                                DesignConversionHandleBoundIoStage
                                    .AfterDurableDelete &&
                            string.Equals(
                                path,
                                fixture.DenialPath,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            primaryDurablyDeleted = true;
                            return;
                        }

                        if (primaryDurablyDeleted &&
                            stage ==
                                DesignConversionHandleBoundIoStage
                                    .BeforeDelete &&
                            path.EndsWith(
                                JsonDesignConversionPostEvaluationStateStore
                                    .TrainingEligibilityDenialReleaseWitnessSuffix,
                                StringComparison.Ordinal))
                        {
                            throw new IOException(
                                "injected-release-witness-cleanup-failure");
                        }
                    }
            };
        var faultingStore =
            new JsonDesignConversionPostEvaluationStateStore(
                fixture.Store.RootDirectory,
                options);

        var release =
            await faultingStore.ReleaseTrainingEligibilityDenialAsync(
                fixture.State.StateRef,
                fixture.ArtifactRecordId);
        var restarted = NewArtifactStore(fixture.ArtifactRoot);
        var listed = await restarted.ListAsync();
        var retry =
            await restarted.ReleaseTrainingEligibilityDenialAsync(
                fixture.State.StateRef,
                fixture.ArtifactRecordId);

        Assert.False(release.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore
                .TrainingEligibilityDenyReleaseFailedErrorCode,
            release.ErrorCode);
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        Assert.True(Assert.Single(listed.Value!).TrainingEligibilityDenied);
        Assert.True(retry.IsSuccess, retry.ErrorMessage);
        Assert.True(
            await JsonDesignConversionPostEvaluationStateStore
                .IsCommittedForTrainingAsync(
                    fixture.ArtifactRoot,
                    fixture.State.StateRef,
                    fixture.ArtifactRecordId));
    }

    [Theory]
    [InlineData(
        nameof(DesignConversionHandleBoundIoStage
            .BeforeDirectoryMetadataFlush))]
    [InlineData(
        nameof(DesignConversionHandleBoundIoStage.AfterDurableDelete))]
    public async Task TrainingDenialRelease_FinalWitnessPostNamespaceFailureReturnsSuccessAndFreshEligibility(
        string injectedStageName)
    {
        var injectedStage =
            Enum.Parse<DesignConversionHandleBoundIoStage>(
                injectedStageName);
        var fixture = await SeedCommittedTrainingDenialAsync(
            "training-denial-final-witness-" + injectedStage);
        var witnessDeleteStarted = false;
        var options =
            DesignConversionPostEvaluationStateStoreOptions.Default with
            {
                BeforeTrainingEligibilityDenialHandleIoForTesting =
                    (stage, path) =>
                    {
                        var isWitness =
                            path.EndsWith(
                                JsonDesignConversionPostEvaluationStateStore
                                    .TrainingEligibilityDenialReleaseWitnessSuffix,
                                StringComparison.Ordinal);
                        if (stage ==
                                DesignConversionHandleBoundIoStage
                                    .BeforeDelete &&
                            isWitness)
                        {
                            witnessDeleteStarted = true;
                        }

                        if (!witnessDeleteStarted ||
                            stage != injectedStage ||
                            (injectedStage ==
                                 DesignConversionHandleBoundIoStage
                                     .AfterDurableDelete &&
                             !isWitness))
                        {
                            return;
                        }

                        throw new IOException(
                            "injected-final-witness-" + injectedStage);
                    }
            };
        var faultingStore =
            new JsonDesignConversionPostEvaluationStateStore(
                fixture.Store.RootDirectory,
                options);

        var release =
            await faultingStore.ReleaseTrainingEligibilityDenialAsync(
                fixture.State.StateRef,
                fixture.ArtifactRecordId);
        var restarted = NewArtifactStore(fixture.ArtifactRoot);
        var listed = await restarted.ListAsync();

        Assert.True(release.IsSuccess, release.ErrorMessage);
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        Assert.False(
            Assert.Single(listed.Value!).TrainingEligibilityDenied);
        Assert.True(
            await JsonDesignConversionPostEvaluationStateStore
                .IsCommittedForTrainingAsync(
                    fixture.ArtifactRoot,
                    fixture.State.StateRef,
                    fixture.ArtifactRecordId));
    }

    [Fact]
    public async Task TrainingDenialInventory_UnexpectedLeafFailsListAndCentralEligibilityClosed()
    {
        var fixture = await SeedCommittedTrainingDenialAsync(
            "training-denial-unexpected-leaf");
        await File.WriteAllTextAsync(
            Path.Combine(
                fixture.DenialDirectory,
                "unexpected-leaf.txt"),
            "must-not-be-ignored",
            StrictUtf8);

        var restarted = NewArtifactStore(fixture.ArtifactRoot);
        var listed = await restarted.ListAsync();

        Assert.False(listed.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore
                .TrustBoundaryErrorCode,
            listed.ErrorCode);
        Assert.False(
            await JsonDesignConversionPostEvaluationStateStore
                .IsCommittedForTrainingAsync(
                    fixture.ArtifactRoot,
                    fixture.State.StateRef,
                    fixture.ArtifactRecordId));
    }

    [Fact]
    public async Task TrainingDenialInventory_EnforcesMarkerCountQuota()
    {
        var fixture = await SeedCommittedTrainingDenialAsync(
            "training-denial-count-quota");
        var second =
            await fixture.Store.DenyTrainingEligibilityAsync(
                fixture.State.StateRef,
                fixture.ArtifactRecordId + "-second",
                "SECOND_DENIAL");
        Assert.True(second.IsSuccess, second.ErrorMessage);
        var quotaStore =
            new JsonDesignConversionPostEvaluationStateStore(
                fixture.Store.RootDirectory,
                Options(
                    maxSingle: 4096,
                    maxCount: 1,
                    maxTotal: 8192));

        var listed = await quotaStore.ListAsync();

        Assert.False(listed.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore
                .QuotaExceededErrorCode,
            listed.ErrorCode);
        Assert.False(
            await JsonDesignConversionPostEvaluationStateStore
                .IsCommittedForTrainingAsync(
                    fixture.ArtifactRoot,
                    fixture.State.StateRef,
                    fixture.ArtifactRecordId));
    }

    [Fact]
    public async Task TrainingDenialInventory_EnforcesSingleMarkerByteQuota()
    {
        var artifactRoot =
            NewRoot("training-denial-single-byte-quota");
        var store = NewArtifactStore(artifactRoot);
        var state = await store.WriteAsync(
            InitialNoReservationState(
                "training-denial-single-byte-quota"));
        Assert.True(state.IsSuccess, state.ErrorMessage);
        var artifactRecordId =
            "artifact-training-denial-single-byte-quota";
        var denied = await store.DenyTrainingEligibilityAsync(
            state.Value!.StateRef,
            artifactRecordId,
            new string('x', 5000));
        Assert.True(denied.IsSuccess, denied.ErrorMessage);
        var quotaStore =
            new JsonDesignConversionPostEvaluationStateStore(
                store.RootDirectory,
                Options(
                    maxSingle: 4096,
                    maxCount: 8,
                    maxTotal: 16384));

        var listed = await quotaStore.ListAsync();

        Assert.False(listed.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore
                .QuotaExceededErrorCode,
            listed.ErrorCode);
        Assert.False(
            await JsonDesignConversionPostEvaluationStateStore
                .IsCommittedForTrainingAsync(
                    artifactRoot,
                    state.Value.StateRef,
                    artifactRecordId));
    }

    [Fact]
    public async Task TrainingDenialInventory_EnforcesTotalMarkerByteQuota()
    {
        var artifactRoot =
            NewRoot("training-denial-total-byte-quota");
        var store = NewArtifactStore(artifactRoot);
        var state = await store.WriteAsync(
            InitialNoReservationState(
                "training-denial-total-byte-quota"));
        Assert.True(state.IsSuccess, state.ErrorMessage);
        var firstArtifact =
            "artifact-training-denial-total-byte-quota-first";
        Assert.True(
            (await store.DenyTrainingEligibilityAsync(
                state.Value!.StateRef,
                firstArtifact,
                new string('x', 3000))).IsSuccess);
        Assert.True(
            (await store.DenyTrainingEligibilityAsync(
                state.Value.StateRef,
                firstArtifact + "-second",
                new string('y', 3000))).IsSuccess);
        var quotaStore =
            new JsonDesignConversionPostEvaluationStateStore(
                store.RootDirectory,
                Options(
                    maxSingle: 4096,
                    maxCount: 8,
                    maxTotal: 5000));

        var listed = await quotaStore.ListAsync();

        Assert.False(listed.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore
                .QuotaExceededErrorCode,
            listed.ErrorCode);
        Assert.False(
            await JsonDesignConversionPostEvaluationStateStore
                .IsCommittedForTrainingAsync(
                    artifactRoot,
                    state.Value.StateRef,
                    firstArtifact));
    }

    [PostEvaluationStateStoreReparseFact]
    public async Task TrainingDenialDirectory_ReparsePointFailsClosedWithoutFollowingTarget()
    {
        var artifactRoot = NewRoot("training-denial-directory-reparse");
        var store = NewArtifactStore(artifactRoot);
        var state = await store.WriteAsync(
            InitialNoReservationState(
                "training-denial-directory-reparse"));
        Assert.True(state.IsSuccess, state.ErrorMessage);
        var denialDirectory =
            store.RootDirectory +
            JsonDesignConversionPostEvaluationStateStore
                .TrainingEligibilityDenialDirectorySuffix;
        var outside = Path.Combine(
            NewRoot("training-denial-directory-target"),
            "outside");
        Directory.CreateDirectory(outside);
        Directory.CreateSymbolicLink(denialDirectory, outside);

        var denied = await store.DenyTrainingEligibilityAsync(
            state.Value!.StateRef,
            "artifact-training-denial-directory-reparse",
            "MUST_FAIL_CLOSED");
        var listed = await store.ListAsync();

        Assert.False(denied.IsSuccess);
        Assert.False(listed.IsSuccess);
        Assert.Empty(Directory.EnumerateFileSystemEntries(outside));
        Assert.False(
            await JsonDesignConversionPostEvaluationStateStore
                .IsCommittedForTrainingAsync(
                    artifactRoot,
                    state.Value.StateRef,
                    "artifact-training-denial-directory-reparse"));
    }

    [PostEvaluationStateStoreReparseFact]
    public async Task TrainingDenialMarker_ReparsePointFailsClosedWithoutFollowingTarget()
    {
        var fixture = await SeedCommittedTrainingDenialAsync(
            "training-denial-marker-reparse");
        var outside = Path.Combine(
            NewRoot("training-denial-marker-target"),
            "outside-denial.json");
        Directory.CreateDirectory(Path.GetDirectoryName(outside)!);
        var originalBytes = await File.ReadAllBytesAsync(
            fixture.DenialPath);
        await File.WriteAllBytesAsync(outside, originalBytes);
        File.Delete(fixture.DenialPath);
        File.CreateSymbolicLink(fixture.DenialPath, outside);

        var restarted = NewArtifactStore(fixture.ArtifactRoot);
        var listed = await restarted.ListAsync();

        Assert.False(listed.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore
                .TrustBoundaryErrorCode,
            listed.ErrorCode);
        Assert.Equal(
            originalBytes,
            await File.ReadAllBytesAsync(outside));
        Assert.False(
            await JsonDesignConversionPostEvaluationStateStore
                .IsCommittedForTrainingAsync(
                    fixture.ArtifactRoot,
                    fixture.State.StateRef,
                    fixture.ArtifactRecordId));
    }

    [Fact]
    public async Task TrainingDenialMarker_IdentitySwapBeforeOpenFailsTrustBoundaryClosed()
    {
        var fixture = await SeedCommittedTrainingDenialAsync(
            "training-denial-identity-swap");
        var originalBytes = await File.ReadAllBytesAsync(
            fixture.DenialPath);
        var movedPath = fixture.DenialPath + ".moved";
        var swapped = false;
        var primaryOpenCount = 0;
        var options =
            DesignConversionPostEvaluationStateStoreOptions.Default with
            {
                BeforeTrainingEligibilityDenialHandleIoForTesting =
                    (stage, path) =>
                    {
                        if (swapped ||
                            stage !=
                                DesignConversionHandleBoundIoStage
                                    .LeafBeforeOpen ||
                            !string.Equals(
                                path,
                                fixture.DenialPath,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            return;
                        }

                        primaryOpenCount++;
                        if (primaryOpenCount != 2)
                            return;

                        File.Move(
                            fixture.DenialPath,
                            movedPath,
                            overwrite: false);
                        File.WriteAllBytes(
                            fixture.DenialPath,
                            originalBytes);
                        swapped = true;
                    }
            };
        var swappingStore =
            new JsonDesignConversionPostEvaluationStateStore(
                fixture.Store.RootDirectory,
                options);

        var listed = await swappingStore.ListAsync();

        Assert.True(swapped);
        Assert.False(listed.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore
                .TrustBoundaryErrorCode,
            listed.ErrorCode);
        Assert.False(
            await JsonDesignConversionPostEvaluationStateStore
                .IsCommittedForTrainingAsync(
                    fixture.ArtifactRoot,
                    fixture.State.StateRef,
                    fixture.ArtifactRecordId));
    }

    [Fact]
    public async Task ReadAndListAsync_LegacyCommittedArtifactIdWithoutV2AuthorityIsQuarantined()
    {
        var artifactRoot = NewRoot("legacy-committed-artifact");
        var store = NewArtifactStore(artifactRoot);
        Directory.CreateDirectory(store.RootDirectory);
        var legacy = InitialNoReservationState("legacy-committed-artifact") with
        {
            PersistenceStatus =
                JsonDesignConversionPostEvaluationStateStore.PersistenceCommitted,
            LedgerRecordId = "improvement-loop-completion:legacy-non-authoritative",
            ArtifactRecordId = "legacy-artifact-record",
            UpdatedAtUtc = DateTimeOffset.Parse("2026-07-30T00:00:01Z"),
            Version = 2L
        };
        await File.WriteAllBytesAsync(
            StatePath(store.RootDirectory, legacy.StateRef),
            SerializeLegacyStateEnvelope(legacy));

        var read = await store.ReadAsync(legacy.StateRef);
        var listed = await store.ListAsync();

        Assert.True(read.IsSuccess, read.ErrorMessage);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.PersistenceQuarantined,
            read.Value!.PersistenceStatus);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore
                .LegacyCompletionBindingUnverifiableErrorCode,
            read.Value.LastErrorCode);
        Assert.Empty(read.Value.CompletionBindingKind);
        Assert.Empty(read.Value.CompletionBindingRef);
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        Assert.Equal(read.Value, Assert.Single(listed.Value!));
        Assert.False(await JsonDesignConversionPostEvaluationStateStore.IsCommittedForTrainingAsync(
            artifactRoot,
            legacy.StateRef,
            legacy.ArtifactRecordId));
    }

    [Fact]
    public async Task ReadAndListAsync_LegacyCommittedFakeLedgerIsQuarantinedAndNeverTrainingEligible()
    {
        var artifactRoot = NewRoot("legacy-fake-ledger");
        var store = NewArtifactStore(artifactRoot);
        Directory.CreateDirectory(store.RootDirectory);
        var legacy = InitialNoReservationState("legacy-fake-ledger") with
        {
            PersistenceStatus =
                JsonDesignConversionPostEvaluationStateStore.PersistenceCommitted,
            LedgerRecordId = "improvement-loop-completion:legacy-fake",
            ArtifactRecordId = string.Empty,
            UpdatedAtUtc = DateTimeOffset.Parse("2026-07-30T00:00:01Z"),
            Version = 2L
        };
        await File.WriteAllBytesAsync(
            StatePath(store.RootDirectory, legacy.StateRef),
            SerializeLegacyStateEnvelope(legacy));

        var read = await store.ReadAsync(legacy.StateRef);
        var listed = await store.ListAsync();

        Assert.True(read.IsSuccess, read.ErrorMessage);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.PersistenceQuarantined,
            read.Value!.PersistenceStatus);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore
                .LegacyCompletionBindingUnverifiableErrorCode,
            read.Value.LastErrorCode);
        Assert.Empty(read.Value.CompletionBindingKind);
        Assert.Empty(read.Value.CompletionBindingRef);
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        Assert.Equal(read.Value, Assert.Single(listed.Value!));
        Assert.False(await JsonDesignConversionPostEvaluationStateStore.IsCommittedForTrainingAsync(
            artifactRoot,
            legacy.StateRef,
            "any-artifact-record"));
    }

    [Theory]
    [InlineData("canonical-ledger")]
    [InlineData("missing-target")]
    public async Task ReadAndListAsync_LegacyCommittedWithoutExplicitCompletionAuthorityIsQuarantined(
        string targetShape)
    {
        var artifactRoot = NewRoot("legacy-unverified-" + targetShape);
        var store = NewArtifactStore(artifactRoot);
        Directory.CreateDirectory(store.RootDirectory);
        var ledgerRecordId = targetShape == "canonical-ledger"
            ? JsonlDesignConversionVisualCostLedger.RecordIdPrefix + "-" +
              new string('a', 64)
            : string.Empty;
        var legacy = InitialNoReservationState("legacy-unverified-" + targetShape) with
        {
            PersistenceStatus =
                JsonDesignConversionPostEvaluationStateStore.PersistenceCommitted,
            LedgerRecordId = ledgerRecordId,
            ArtifactRecordId = string.Empty,
            UpdatedAtUtc = DateTimeOffset.Parse("2026-07-30T00:00:01Z"),
            Version = 2L
        };
        await File.WriteAllBytesAsync(
            StatePath(store.RootDirectory, legacy.StateRef),
            SerializeLegacyStateEnvelope(legacy));

        var read = await store.ReadAsync(legacy.StateRef);
        var listed = await store.ListAsync();

        Assert.True(read.IsSuccess, read.ErrorMessage);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.PersistenceQuarantined,
            read.Value!.PersistenceStatus);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore
                .LegacyCompletionBindingUnverifiableErrorCode,
            read.Value.LastErrorCode);
        Assert.Empty(read.Value.CompletionBindingKind);
        Assert.Empty(read.Value.CompletionBindingRef);
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        Assert.Equal(read.Value, Assert.Single(listed.Value!));
        Assert.False(await JsonDesignConversionPostEvaluationStateStore.IsCommittedForTrainingAsync(
            artifactRoot,
            legacy.StateRef,
            "unverified-artifact-record"));
    }

    [Fact]
    public async Task ReadAsync_LegacyUpgradeIsReadOnlyAndRepeatedReadsAreIdempotent()
    {
        var root = NewRoot("legacy-repeat-read");
        var store = new JsonDesignConversionPostEvaluationStateStore(root);
        Directory.CreateDirectory(root);
        var legacy = InitialNoReservationState("legacy-repeat-read") with
        {
            PersistenceStatus =
                JsonDesignConversionPostEvaluationStateStore.PersistenceCommitted,
            LedgerRecordId = JsonlDesignConversionVisualCostLedger.RecordIdPrefix +
                             "-" +
                             new string('b', 64),
            UpdatedAtUtc = DateTimeOffset.Parse("2026-07-30T00:00:01Z"),
            Version = 2L
        };
        var path = StatePath(root, legacy.StateRef);
        var originalBytes = SerializeLegacyStateEnvelope(legacy);
        await File.WriteAllBytesAsync(path, originalBytes);

        var first = await store.ReadAsync(legacy.StateRef);
        var second = await store.ReadAsync(legacy.StateRef);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(first.Value, second.Value);
        Assert.Equal(originalBytes, await File.ReadAllBytesAsync(path));
    }

    [Theory]
    [InlineData("duplicate-envelope")]
    [InlineData("unknown-state-property")]
    [InlineData("missing-required-property")]
    [InlineData("unknown-schema")]
    public async Task ReadAsync_LegacySchemaRejectsAmbiguousMalformedOrUnknownJson(
        string mutation)
    {
        var root = NewRoot("legacy-strict-" + mutation);
        var store = new JsonDesignConversionPostEvaluationStateStore(root);
        Directory.CreateDirectory(root);
        var legacy = InitialNoReservationState("legacy-strict-" + mutation);
        var json = StrictUtf8.GetString(SerializeLegacyStateEnvelope(legacy));
        var mutated = mutation switch
        {
            "duplicate-envelope" => json.Replace(
                "{\"schemaVersion\":",
                "{\"schemaVersion\":\"" +
                JsonDesignConversionPostEvaluationStateStore.LegacySchemaVersion +
                "\",\"schemaVersion\":",
                StringComparison.Ordinal),
            "unknown-state-property" => json.Replace(
                "\"version\":1}",
                "\"version\":1,\"unknownState\":true}",
                StringComparison.Ordinal),
            "missing-required-property" => json.Replace(
                $"\"userRef\":\"{legacy.UserRef}\",",
                string.Empty,
                StringComparison.Ordinal),
            "unknown-schema" => json.Replace(
                JsonDesignConversionPostEvaluationStateStore.LegacySchemaVersion,
                "design-conversion-post-evaluation-state-v0",
                StringComparison.Ordinal),
            _ => throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null)
        };
        await File.WriteAllTextAsync(
            StatePath(root, legacy.StateRef),
            mutated,
            StrictUtf8);

        var read = await store.ReadAsync(legacy.StateRef);
        var listed = await store.ListAsync();

        Assert.False(read.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.ReadFailedErrorCode,
            read.ErrorCode);
        Assert.False(listed.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.ReadFailedErrorCode,
            listed.ErrorCode);
    }

    [Fact]
    public async Task ReadAsync_RenamedEnvelopeWhoseStateRefDoesNotMatchFilenameFailsClosed()
    {
        var root = NewRoot("renamed-envelope");
        var store = new JsonDesignConversionPostEvaluationStateStore(root);
        var source = InitialReservedState("renamed-source");
        var written = await store.WriteAsync(source);
        Assert.True(written.IsSuccess, written.ErrorMessage);
        var substitutedRef = JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
            "renamed-substitution");
        File.Move(StatePath(root, source.StateRef), StatePath(root, substitutedRef));

        var read = await store.ReadAsync(substitutedRef);

        Assert.False(read.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.ReadFailedErrorCode,
            read.ErrorCode);
    }

    [Theory]
    [InlineData("duplicate-envelope")]
    [InlineData("duplicate-state")]
    [InlineData("unknown-envelope-property")]
    [InlineData("unknown-state-property")]
    [InlineData("missing-required-property")]
    [InlineData("unknown-accounting-status")]
    [InlineData("unknown-desired-action")]
    [InlineData("unknown-persistence-status")]
    [InlineData("non-utc-timestamp")]
    [InlineData("bad-version")]
    public async Task ReadAsync_StrictSchemaRejectsAmbiguousOrInvalidJson(string mutation)
    {
        var root = NewRoot("strict-" + mutation);
        var store = new JsonDesignConversionPostEvaluationStateStore(root);
        var state = InitialReservedState("strict-" + mutation);
        var written = await store.WriteAsync(state);
        Assert.True(written.IsSuccess, written.ErrorMessage);
        var path = StatePath(root, state.StateRef);
        var json = await File.ReadAllTextAsync(path, StrictUtf8);
        var mutated = mutation switch
        {
            "duplicate-envelope" => json.Replace(
                "{\"schemaVersion\":",
                "{\"schemaVersion\":\"design-conversion-post-evaluation-state-v2\",\"schemaVersion\":",
                StringComparison.Ordinal),
            "duplicate-state" => json.Replace(
                $"\"stateRef\":\"{state.StateRef}\"",
                $"\"stateRef\":\"{state.StateRef}\",\"stateRef\":\"{state.StateRef}\"",
                StringComparison.Ordinal),
            "unknown-envelope-property" => json.Replace(
                "\"state\":{",
                "\"unknownEnvelope\":true,\"state\":{",
                StringComparison.Ordinal),
            "unknown-state-property" => json.Replace(
                "\"version\":1}",
                "\"version\":1,\"unknownState\":true}",
                StringComparison.Ordinal),
            "missing-required-property" => json.Replace(
                $"\"userRef\":\"{state.UserRef}\",",
                string.Empty,
                StringComparison.Ordinal),
            "unknown-accounting-status" => json.Replace(
                "\"accountingStatus\":\"pending\"",
                "\"accountingStatus\":\"learned\"",
                StringComparison.Ordinal),
            "unknown-desired-action" => json.Replace(
                "\"accountingDesiredAction\":\"settle\"",
                "\"accountingDesiredAction\":\"guess\"",
                StringComparison.Ordinal),
            "unknown-persistence-status" => json.Replace(
                "\"persistenceStatus\":\"provisional\"",
                "\"persistenceStatus\":\"maybe\"",
                StringComparison.Ordinal),
            "non-utc-timestamp" => json.Replace(
                "\"updatedAtUtc\":\"2026-07-30T00:00:00+00:00\"",
                "\"updatedAtUtc\":\"2026-07-30T02:00:00+02:00\"",
                StringComparison.Ordinal),
            "bad-version" => json.Replace(
                "\"version\":1",
                "\"version\":0",
                StringComparison.Ordinal),
            _ => throw new InvalidOperationException(mutation)
        };
        Assert.NotEqual(json, mutated);
        await File.WriteAllTextAsync(path, mutated, StrictUtf8);

        var read = await store.ReadAsync(state.StateRef);

        Assert.False(read.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.ReadFailedErrorCode,
            read.ErrorCode);
    }

    [Fact]
    public async Task WriteAsync_RejectsIllegalAccountingRollbackAndStaleVersion()
    {
        var store = new JsonDesignConversionPostEvaluationStateStore(
            NewRoot("accounting-fsm"));
        var created = await store.WriteAsync(InitialReservedState("accounting-fsm"));
        Assert.True(created.IsSuccess, created.ErrorMessage);
        var reconciling = await store.WriteAsync(created.Value! with
        {
            AccountingStatus =
                JsonDesignConversionPostEvaluationStateStore.AccountingReconciliationRequired,
            AccountingDesiredAction =
                JsonDesignConversionPostEvaluationStateStore.DesiredRelease,
            SettlementDefinitivelyNotApplied = true,
            ReleaseAttempted = true,
            UpdatedAtUtc = created.Value!.UpdatedAtUtc.AddTicks(1)
        });
        Assert.True(reconciling.IsSuccess, reconciling.ErrorMessage);

        var rollback = await store.WriteAsync(reconciling.Value! with
        {
            AccountingStatus = JsonDesignConversionPostEvaluationStateStore.AccountingPending,
            AccountingDesiredAction = JsonDesignConversionPostEvaluationStateStore.DesiredRelease,
            UpdatedAtUtc = reconciling.Value!.UpdatedAtUtc.AddTicks(1)
        });
        var stale = await store.WriteAsync(created.Value! with
        {
            SettlementAttempted = true,
            UpdatedAtUtc = reconciling.Value.UpdatedAtUtc.AddTicks(2)
        });

        AssertConflict(rollback);
        AssertConflict(stale);
    }

    [Theory]
    [InlineData("reservation")]
    [InlineData("user")]
    [InlineData("period-start")]
    [InlineData("period-end")]
    [InlineData("charged-cost-after-attempt")]
    [InlineData("updated-at-rollback")]
    public async Task WriteAsync_RejectsImmutableIdentityAndMonotonicityViolations(
        string mutation)
    {
        var store = new JsonDesignConversionPostEvaluationStateStore(
            NewRoot("immutable-" + mutation));
        var created = await store.WriteAsync(
            InitialReservedState("immutable-" + mutation));
        Assert.True(created.IsSuccess, created.ErrorMessage);
        var attempted = await store.WriteAsync(created.Value! with
        {
            SettlementAttempted = true,
            UpdatedAtUtc = created.Value!.UpdatedAtUtc.AddTicks(1)
        });
        Assert.True(attempted.IsSuccess, attempted.ErrorMessage);

        var candidate = mutation switch
        {
            "reservation" => attempted.Value! with
            {
                ReservationRef = "different-reservation",
                UpdatedAtUtc = attempted.Value!.UpdatedAtUtc.AddTicks(1)
            },
            "user" => attempted.Value! with
            {
                UserRef = "different-user",
                UpdatedAtUtc = attempted.Value!.UpdatedAtUtc.AddTicks(1)
            },
            "period-start" => attempted.Value! with
            {
                PeriodStartUtc = attempted.Value!.PeriodStartUtc.AddDays(1),
                UpdatedAtUtc = attempted.Value.UpdatedAtUtc.AddTicks(1)
            },
            "period-end" => attempted.Value! with
            {
                PeriodEndExclusiveUtc = attempted.Value!.PeriodEndExclusiveUtc.AddDays(1),
                UpdatedAtUtc = attempted.Value.UpdatedAtUtc.AddTicks(1)
            },
            "charged-cost-after-attempt" => attempted.Value! with
            {
                ChargedCostUsd = attempted.Value!.ChargedCostUsd + 0.01m,
                UpdatedAtUtc = attempted.Value.UpdatedAtUtc.AddTicks(1)
            },
            "updated-at-rollback" => attempted.Value! with
            {
                UpdatedAtUtc = attempted.Value!.UpdatedAtUtc.AddTicks(-1)
            },
            _ => throw new InvalidOperationException(mutation)
        };

        var result = await store.WriteAsync(candidate);

        AssertConflict(result);
    }

    [Fact]
    public async Task WriteAsync_RejectsResultIdentityReplacementAndTerminalPersistenceSwitches()
    {
        var committedStore = new JsonDesignConversionPostEvaluationStateStore(
            NewRoot("committed-terminal"));
        var committedInitial = await committedStore.WriteAsync(
            InitialNoReservationState("committed-terminal"));
        Assert.True(committedInitial.IsSuccess, committedInitial.ErrorMessage);
        var committed = await committedStore.WriteAsync(committedInitial.Value! with
        {
            PersistenceStatus = JsonDesignConversionPostEvaluationStateStore.PersistenceCommitted,
            LedgerRecordId = "ledger-original",
            ArtifactRecordId = "artifact-original",
            CompletionBindingKind =
                JsonDesignConversionPostEvaluationStateStore
                    .CompletionBindingLocalArtifactDbRecord,
            CompletionBindingRef = "artifact-original",
            UpdatedAtUtc = committedInitial.Value!.UpdatedAtUtc.AddTicks(1)
        });
        Assert.True(committed.IsSuccess, committed.ErrorMessage);

        var replaceIds = await committedStore.WriteAsync(committed.Value! with
        {
            LedgerRecordId = "ledger-substituted",
            ArtifactRecordId = "artifact-substituted",
            UpdatedAtUtc = committed.Value!.UpdatedAtUtc.AddTicks(1)
        });
        var switchCommittedToQuarantine = await committedStore.WriteAsync(committed.Value with
        {
            PersistenceStatus = JsonDesignConversionPostEvaluationStateStore.PersistenceQuarantined,
            LastErrorCode = "late-quarantine",
            UpdatedAtUtc = committed.Value.UpdatedAtUtc.AddTicks(2)
        });
        var downgradeCommitted = await committedStore.WriteAsync(committed.Value with
        {
            PersistenceStatus = JsonDesignConversionPostEvaluationStateStore.PersistenceProvisional,
            LedgerRecordId = string.Empty,
            ArtifactRecordId = string.Empty,
            UpdatedAtUtc = committed.Value.UpdatedAtUtc.AddTicks(3)
        });

        AssertConflict(replaceIds);
        AssertConflict(switchCommittedToQuarantine);
        AssertConflict(downgradeCommitted);

        var quarantinedStore = new JsonDesignConversionPostEvaluationStateStore(
            NewRoot("quarantined-terminal"));
        var quarantineInitial = await quarantinedStore.WriteAsync(
            InitialNoReservationState("quarantined-terminal"));
        var quarantined = await quarantinedStore.WriteAsync(quarantineInitial.Value! with
        {
            PersistenceStatus = JsonDesignConversionPostEvaluationStateStore.PersistenceQuarantined,
            LedgerRecordId = "ledger-partial",
            LastErrorCode = "durable-quarantine",
            UpdatedAtUtc = quarantineInitial.Value!.UpdatedAtUtc.AddTicks(1)
        });
        Assert.True(quarantined.IsSuccess, quarantined.ErrorMessage);
        var switchQuarantineToCommitted = await quarantinedStore.WriteAsync(quarantined.Value! with
        {
            PersistenceStatus = JsonDesignConversionPostEvaluationStateStore.PersistenceCommitted,
            ArtifactRecordId = "artifact-after-quarantine",
            CompletionBindingKind =
                JsonDesignConversionPostEvaluationStateStore
                    .CompletionBindingLocalArtifactDbRecord,
            CompletionBindingRef = "artifact-after-quarantine",
            LastErrorCode = string.Empty,
            UpdatedAtUtc = quarantined.Value!.UpdatedAtUtc.AddTicks(1)
        });
        AssertConflict(switchQuarantineToCommitted);
    }

    [Fact]
    public async Task WriteAsync_TwoIndependentInstancesSerializeAndRejectConcurrentStaleMutation()
    {
        var root = NewRoot("independent-instances");
        var firstStore = new JsonDesignConversionPostEvaluationStateStore(root);
        var secondStore = new JsonDesignConversionPostEvaluationStateStore(root);
        var initial = InitialReservedState("independent-instances");

        var duplicateCreates = await Task.WhenAll(
            firstStore.WriteAsync(initial),
            secondStore.WriteAsync(initial));
        Assert.All(duplicateCreates, result => Assert.True(result.IsSuccess, result.ErrorMessage));
        var read = await firstStore.ReadAsync(initial.StateRef);
        Assert.True(read.IsSuccess, read.ErrorMessage);
        Assert.Equal(2, read.Value!.Version);

        var settleCandidate = read.Value with
        {
            SettlementAttempted = true,
            UpdatedAtUtc = read.Value.UpdatedAtUtc.AddTicks(1)
        };
        var releaseCandidate = read.Value with
        {
            AccountingStatus =
                JsonDesignConversionPostEvaluationStateStore.AccountingReconciliationRequired,
            AccountingDesiredAction =
                JsonDesignConversionPostEvaluationStateStore.DesiredRelease,
            SettlementDefinitivelyNotApplied = true,
            ReleaseAttempted = true,
            UpdatedAtUtc = read.Value.UpdatedAtUtc.AddTicks(1)
        };
        var competing = await Task.WhenAll(
            firstStore.WriteAsync(settleCandidate),
            secondStore.WriteAsync(releaseCandidate));

        Assert.Single(competing, result => result.IsSuccess);
        Assert.Single(
            competing,
            result => !result.IsSuccess &&
                      result.ErrorCode ==
                      JsonDesignConversionPostEvaluationStateStore.ConflictErrorCode);
    }

    [Fact]
    public async Task ReadAsync_RootReplacementBetweenLeaseAndLeafOpenFailsWithoutReadingReplacement()
    {
        var root = NewRoot("handle-bound-root-swap");
        var state = InitialReservedState("handle-bound-root-swap");
        var seeded = await new JsonDesignConversionPostEvaluationStateStore(root)
            .WriteAsync(state);
        Assert.True(seeded.IsSuccess, seeded.ErrorMessage);

        var movedRoot = root + "-retained";
        var replacementBytes = StrictUtf8.GetBytes("replacement-root-must-not-be-read");
        var swapped = 0;
        var options = Options(
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxSingleStateBytes,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxStateFileCount,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxTotalStateBytes) with
        {
            BeforeHandleIoForTesting = (stage, path) =>
            {
                if (stage != DesignConversionHandleBoundIoStage.RootSegmentBeforeOpen ||
                    !string.Equals(
                        Path.GetFullPath(path),
                        Path.GetFullPath(root),
                        StringComparison.OrdinalIgnoreCase) ||
                    Interlocked.Exchange(ref swapped, 1) != 0)
                {
                    return;
                }

                Directory.Move(root, movedRoot);
                Directory.CreateDirectory(root);
                File.WriteAllBytes(
                    StatePath(root, state.StateRef),
                    replacementBytes);
            }
        };

        var read = await new JsonDesignConversionPostEvaluationStateStore(root, options)
            .ReadAsync(state.StateRef);

        Assert.False(read.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
            read.ErrorCode);
        Assert.Equal(
            replacementBytes,
            await File.ReadAllBytesAsync(StatePath(root, state.StateRef)));
        Assert.True(File.Exists(StatePath(movedRoot, state.StateRef)));
    }

    [Fact]
    public async Task ReadAsync_LeafReplacementAfterInventoryFailsWithoutReadingReplacement()
    {
        var root = NewRoot("handle-bound-leaf-swap");
        var state = InitialReservedState("handle-bound-leaf-swap");
        var seeded = await new JsonDesignConversionPostEvaluationStateStore(root)
            .WriteAsync(state);
        Assert.True(seeded.IsSuccess, seeded.ErrorMessage);

        var statePath = StatePath(root, state.StateRef);
        var retainedPath = statePath + ".retained";
        var originalBytes = await File.ReadAllBytesAsync(statePath);
        var replacementBytes = StrictUtf8.GetBytes("replacement-leaf-must-not-be-read");
        var stateLeafOpenCount = 0;
        var options = Options(
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxSingleStateBytes,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxStateFileCount,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxTotalStateBytes) with
        {
            BeforeHandleIoForTesting = (stage, path) =>
            {
                if (stage != DesignConversionHandleBoundIoStage.LeafBeforeOpen ||
                    !string.Equals(
                        Path.GetFullPath(path),
                        Path.GetFullPath(statePath),
                        StringComparison.OrdinalIgnoreCase) ||
                    Interlocked.Increment(ref stateLeafOpenCount) != 2)
                {
                    return;
                }

                File.Move(statePath, retainedPath);
                File.WriteAllBytes(statePath, replacementBytes);
            }
        };

        var read = await new JsonDesignConversionPostEvaluationStateStore(root, options)
            .ReadAsync(state.StateRef);

        Assert.False(read.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
            read.ErrorCode);
        Assert.Equal(replacementBytes, await File.ReadAllBytesAsync(statePath));
        Assert.Equal(originalBytes, await File.ReadAllBytesAsync(retainedPath));
    }

    [Fact]
    public async Task WriteAsync_FailureAfterAtomicRenameBeforeDirectoryFlushRollsBackPriorDurableState()
    {
        var root = NewRoot("directory-flush-rollback");
        var initial = InitialReservedState("directory-flush-rollback");
        var seeded = await new JsonDesignConversionPostEvaluationStateStore(root)
            .WriteAsync(initial);
        Assert.True(seeded.IsSuccess, seeded.ErrorMessage);
        var priorBytes = await File.ReadAllBytesAsync(StatePath(root, initial.StateRef));

        var injected = 0;
        var options = Options(
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxSingleStateBytes,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxStateFileCount,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxTotalStateBytes) with
        {
            BeforeHandleIoForTesting = (stage, _) =>
            {
                if (stage ==
                        DesignConversionHandleBoundIoStage.AfterAtomicRenameBeforeDirectoryFlush &&
                    Interlocked.Exchange(ref injected, 1) == 0)
                {
                    throw new IOException("injected-metadata-durability-failure");
                }
            }
        };
        var failed = await new JsonDesignConversionPostEvaluationStateStore(root, options)
            .WriteAsync(seeded.Value! with
            {
                UpdatedAtUtc = seeded.Value!.UpdatedAtUtc.AddTicks(1)
            });

        Assert.False(failed.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.WriteFailedErrorCode,
            failed.ErrorCode);
        Assert.Equal(priorBytes, await File.ReadAllBytesAsync(StatePath(root, initial.StateRef)));
        var restarted = await new JsonDesignConversionPostEvaluationStateStore(root)
            .ReadAsync(initial.StateRef);
        Assert.True(restarted.IsSuccess, restarted.ErrorMessage);
        Assert.Equal(seeded.Value, restarted.Value);
        Assert.DoesNotContain(
            Directory.EnumerateFiles(root),
            path => path.EndsWith(".tmp", StringComparison.Ordinal));
    }

    [Fact]
    public async Task WriteAsync_ExistingDestinationSwapAttemptBeforePublicationFailsWithoutOverwritingEitherIdentity()
    {
        var root = NewRoot("conditional-existing-swap");
        var initial = InitialReservedState("conditional-existing-swap");
        var seeded = await new JsonDesignConversionPostEvaluationStateStore(root)
            .WriteAsync(initial);
        Assert.True(seeded.IsSuccess, seeded.ErrorMessage);
        var destinationPath = StatePath(root, initial.StateRef);
        var priorBytes = await File.ReadAllBytesAsync(destinationPath);
        var priorEntry = CaptureEntry(root, Path.GetFileName(destinationPath));
        var outsideRoot = NewRoot("conditional-existing-swap-outside");
        Directory.CreateDirectory(outsideRoot);
        var attackerPath = Path.Combine(outsideRoot, "attacker.json");
        var attackerBytes = StrictUtf8.GetBytes("attacker-existing-destination-bytes");
        await File.WriteAllBytesAsync(attackerPath, attackerBytes);
        var attackerEntry = CaptureEntry(outsideRoot, "attacker.json");
        var swapBlocked = 0;
        var options = Options(
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxSingleStateBytes,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxStateFileCount,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxTotalStateBytes) with
        {
            BeforeHandleIoForTesting = (stage, path) =>
            {
                if (stage != DesignConversionHandleBoundIoStage.BeforeAtomicRename ||
                    !string.Equals(
                        Path.GetFullPath(path),
                        Path.GetFullPath(destinationPath),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                try
                {
                    File.Move(attackerPath, destinationPath, overwrite: true);
                }
                catch (Exception ex) when (
                    ex is IOException or UnauthorizedAccessException)
                {
                    Interlocked.Exchange(ref swapBlocked, 1);
                    throw;
                }
            }
        };

        var failed = await new JsonDesignConversionPostEvaluationStateStore(root, options)
            .WriteAsync(seeded.Value! with
            {
                UpdatedAtUtc = seeded.Value!.UpdatedAtUtc.AddTicks(1)
            });

        Assert.False(failed.IsSuccess);
        var backupPath =
            destinationPath + DesignConversionHandleBoundDirectory.PublicationBackupSuffix;
        if (OperatingSystem.IsLinux())
        {
            Assert.Equal(0, Volatile.Read(ref swapBlocked));
            Assert.Equal(
                JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
                failed.ErrorCode);
            Assert.Equal(attackerBytes, await File.ReadAllBytesAsync(destinationPath));
            Assert.Equal(
                attackerEntry.Identity,
                CaptureEntry(root, Path.GetFileName(destinationPath)).Identity);
            Assert.False(File.Exists(attackerPath));
            Assert.Equal(priorBytes, await File.ReadAllBytesAsync(backupPath));
            Assert.Equal(
                priorEntry.Identity,
                CaptureEntry(root, Path.GetFileName(backupPath)).Identity);
            Assert.DoesNotContain(
                Directory.EnumerateFiles(root),
                path => path.EndsWith(
                    JsonDesignConversionPostEvaluationStateStore.WriteTempSuffix,
                    StringComparison.Ordinal));
        }
        else
        {
            Assert.Equal(1, Volatile.Read(ref swapBlocked));
            Assert.Equal(priorBytes, await File.ReadAllBytesAsync(destinationPath));
            Assert.Equal(
                priorEntry.Identity,
                CaptureEntry(root, Path.GetFileName(destinationPath)).Identity);
            Assert.Equal(attackerBytes, await File.ReadAllBytesAsync(attackerPath));
            Assert.Equal(
                attackerEntry.Identity,
                CaptureEntry(outsideRoot, "attacker.json").Identity);
            Assert.False(File.Exists(backupPath));
        }
        var restarted = await new JsonDesignConversionPostEvaluationStateStore(root)
            .ReadAsync(initial.StateRef);
        if (OperatingSystem.IsLinux())
        {
            Assert.False(restarted.IsSuccess);
            Assert.Equal(
                JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
                restarted.ErrorCode);
            Assert.Contains(
                "manual reconciliation",
                restarted.ErrorMessage,
                StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            Assert.True(restarted.IsSuccess, restarted.ErrorMessage);
            Assert.Equal(seeded.Value, restarted.Value);
        }
    }

    [Fact]
    public async Task WriteAsync_MissingDestinationAppearanceBeforePublicationFailsWithoutOverwritingForeignLeaf()
    {
        var root = NewRoot("conditional-missing-appearance");
        var state = InitialReservedState("conditional-missing-appearance");
        var destinationPath = StatePath(root, state.StateRef);
        var outsideRoot = NewRoot("conditional-missing-appearance-outside");
        Directory.CreateDirectory(outsideRoot);
        var outsidePath = Path.Combine(outsideRoot, "foreign-source.json");
        var foreignBytes = StrictUtf8.GetBytes("foreign-appearing-destination-bytes");
        await File.WriteAllBytesAsync(outsidePath, foreignBytes);
        var appeared = 0;
        var options = Options(
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxSingleStateBytes,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxStateFileCount,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxTotalStateBytes) with
        {
            BeforeHandleIoForTesting = (stage, path) =>
            {
                if (stage != DesignConversionHandleBoundIoStage.BeforeAtomicRename ||
                    !string.Equals(
                        Path.GetFullPath(path),
                        Path.GetFullPath(destinationPath),
                        StringComparison.OrdinalIgnoreCase) ||
                    Interlocked.Exchange(ref appeared, 1) != 0)
                {
                    return;
                }

                File.Copy(outsidePath, destinationPath, overwrite: false);
            }
        };

        var failed = await new JsonDesignConversionPostEvaluationStateStore(root, options)
            .WriteAsync(state);

        Assert.False(failed.IsSuccess);
        Assert.Equal(1, Volatile.Read(ref appeared));
        Assert.Equal(foreignBytes, await File.ReadAllBytesAsync(destinationPath));
        Assert.Equal(foreignBytes, await File.ReadAllBytesAsync(outsidePath));
        Assert.False(File.Exists(
            destinationPath + DesignConversionHandleBoundDirectory.PublicationBackupSuffix));
        Assert.DoesNotContain(
            Directory.EnumerateFiles(root),
            path => path.EndsWith(
                JsonDesignConversionPostEvaluationStateStore.WriteTempSuffix,
                StringComparison.Ordinal));
        var restarted = await new JsonDesignConversionPostEvaluationStateStore(root)
            .ReadAsync(state.StateRef);
        Assert.False(restarted.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.ReadFailedErrorCode,
            restarted.ErrorCode);
        Assert.Equal(foreignBytes, await File.ReadAllBytesAsync(destinationPath));
    }

    [Fact]
    public async Task ReadAsync_InterruptedPublicationBackupWithoutDestinationRequiresManualReconciliation()
    {
        var root = NewRoot("publication-backup-recovery");
        var initial = InitialReservedState("publication-backup-recovery");
        var seeded = await new JsonDesignConversionPostEvaluationStateStore(root)
            .WriteAsync(initial);
        Assert.True(seeded.IsSuccess, seeded.ErrorMessage);
        var destinationPath = StatePath(root, initial.StateRef);
        var backupPath =
            destinationPath + DesignConversionHandleBoundDirectory.PublicationBackupSuffix;
        var priorBytes = await File.ReadAllBytesAsync(destinationPath);
        File.Move(destinationPath, backupPath, overwrite: false);

        var restarted = await new JsonDesignConversionPostEvaluationStateStore(root)
            .ReadAsync(initial.StateRef);

        Assert.False(restarted.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
            restarted.ErrorCode);
        Assert.Contains(
            "manual reconciliation",
            restarted.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(destinationPath));
        Assert.Equal(priorBytes, await File.ReadAllBytesAsync(backupPath));
    }

    [Fact]
    public async Task ReadAsync_AmbiguousPublicationBackupAndDestinationFailsClosedWithoutOverwritingEither()
    {
        var root = NewRoot("publication-backup-ambiguous");
        var initial = InitialReservedState("publication-backup-ambiguous");
        var seeded = await new JsonDesignConversionPostEvaluationStateStore(root)
            .WriteAsync(initial);
        Assert.True(seeded.IsSuccess, seeded.ErrorMessage);
        var destinationPath = StatePath(root, initial.StateRef);
        var destinationBytes = await File.ReadAllBytesAsync(destinationPath);
        var backupPath =
            destinationPath + DesignConversionHandleBoundDirectory.PublicationBackupSuffix;
        var backupBytes = StrictUtf8.GetBytes("ambiguous-prior-publication-bytes");
        await File.WriteAllBytesAsync(backupPath, backupBytes);

        var restarted = await new JsonDesignConversionPostEvaluationStateStore(root)
            .ReadAsync(initial.StateRef);

        Assert.False(restarted.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
            restarted.ErrorCode);
        Assert.Equal(destinationBytes, await File.ReadAllBytesAsync(destinationPath));
        Assert.Equal(backupBytes, await File.ReadAllBytesAsync(backupPath));
    }

    [Fact]
    public async Task ReadAsync_InvalidPublicationBackupNameFailsClosedWithoutMutation()
    {
        var root = NewRoot("publication-backup-invalid-name");
        Directory.CreateDirectory(root);
        var backupPath = Path.Combine(
            root,
            "not-a-canonical-state.json" +
            DesignConversionHandleBoundDirectory.PublicationBackupSuffix);
        var backupBytes = StrictUtf8.GetBytes("invalid-backup-name");
        await File.WriteAllBytesAsync(backupPath, backupBytes);
        var impliedDestinationPath = backupPath[
            ..^DesignConversionHandleBoundDirectory.PublicationBackupSuffix.Length];

        var result = await new JsonDesignConversionPostEvaluationStateStore(root)
            .ListAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
            result.ErrorCode);
        Assert.False(File.Exists(impliedDestinationPath));
        Assert.Equal(backupBytes, await File.ReadAllBytesAsync(backupPath));
    }

    [Theory]
    [InlineData("malformed")]
    [InlineData("duplicate-envelope")]
    public async Task ReadAsync_InvalidPublicationBackupEnvelopeFailsClosedWithoutMutation(
        string corruption)
    {
        var root = NewRoot("publication-backup-envelope-" + corruption);
        var state = InitialReservedState("publication-backup-envelope-" + corruption);
        var fixture = await SeedBackupOnlyAsync(root, state);
        byte[] invalidBytes;
        if (corruption == "malformed")
        {
            invalidBytes = StrictUtf8.GetBytes("{");
        }
        else
        {
            using var document = JsonDocument.Parse(fixture.BackupBytes);
            invalidBytes = StrictUtf8.GetBytes(
                "{\"schemaVersion\":\"" +
                JsonDesignConversionPostEvaluationStateStore.SchemaVersion +
                "\",\"schemaVersion\":\"" +
                JsonDesignConversionPostEvaluationStateStore.SchemaVersion +
                "\",\"state\":" +
                document.RootElement.GetProperty("state").GetRawText() +
                "}");
        }
        await File.WriteAllBytesAsync(fixture.BackupPath, invalidBytes);

        var result = await new JsonDesignConversionPostEvaluationStateStore(root)
            .ReadAsync(state.StateRef);

        Assert.False(result.IsSuccess);
        Assert.False(File.Exists(fixture.DestinationPath));
        Assert.Equal(invalidBytes, await File.ReadAllBytesAsync(fixture.BackupPath));
    }

    [Fact]
    public async Task ReadAsync_PublicationBackupWithWrongFilenameStateRefFailsClosedWithoutMutation()
    {
        var root = NewRoot("publication-backup-wrong-state-ref");
        var stored = InitialReservedState("publication-backup-wrong-state-ref-stored");
        var expected = InitialReservedState("publication-backup-wrong-state-ref-expected");
        var fixture = await SeedBackupOnlyAsync(
            root,
            stored,
            expected.StateRef);

        var result = await new JsonDesignConversionPostEvaluationStateStore(root)
            .ReadAsync(expected.StateRef);

        Assert.False(result.IsSuccess);
        Assert.False(File.Exists(fixture.DestinationPath));
        Assert.Equal(
            fixture.BackupBytes,
            await File.ReadAllBytesAsync(fixture.BackupPath));
    }

    [Fact]
    public async Task ReadAsync_PublicationBackupWithInvalidReservationBindingFailsClosedWithoutMutation()
    {
        var root = NewRoot("publication-backup-invalid-reservation");
        var state = InitialReservedState("publication-backup-invalid-reservation");
        var fixture = await SeedBackupOnlyAsync(root, state);
        var forged = state with
        {
            ReservationRef = state.StateRef
        };
        var forgedBytes = SerializeStateEnvelope(forged);
        await File.WriteAllBytesAsync(fixture.BackupPath, forgedBytes);

        var result = await new JsonDesignConversionPostEvaluationStateStore(root)
            .ReadAsync(state.StateRef);

        Assert.False(result.IsSuccess);
        Assert.False(File.Exists(fixture.DestinationPath));
        Assert.Equal(forgedBytes, await File.ReadAllBytesAsync(fixture.BackupPath));
    }

    [Fact]
    public async Task ReadAsync_OversizedPublicationBackupFailsQuotaWithoutMutation()
    {
        var root = NewRoot("publication-backup-oversized");
        var state = InitialReservedState("publication-backup-oversized");
        var fixture = await SeedBackupOnlyAsync(root, state);
        var maximum = fixture.BackupBytes.LongLength - 1L;
        var options = Options(
            maximum,
            maxCount: 8,
            maxTotal: maximum);

        var result = await new JsonDesignConversionPostEvaluationStateStore(root, options)
            .ReadAsync(state.StateRef);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.QuotaExceededErrorCode,
            result.ErrorCode);
        Assert.False(File.Exists(fixture.DestinationPath));
        Assert.Equal(
            fixture.BackupBytes,
            await File.ReadAllBytesAsync(fixture.BackupPath));
    }

    [Theory]
    [InlineData("total")]
    [InlineData("count")]
    public async Task ReadAsync_PublicationBackupInventoryOverflowFailsWithoutMutation(
        string quota)
    {
        var root = NewRoot("publication-backup-inventory-" + quota);
        var first = InitialReservedState("publication-backup-inventory-" + quota + "-first");
        var second = InitialReservedState("publication-backup-inventory-" + quota + "-second");
        var store = new JsonDesignConversionPostEvaluationStateStore(root);
        var firstCreated = await store.WriteAsync(first);
        var secondCreated = await store.WriteAsync(second);
        Assert.True(firstCreated.IsSuccess, firstCreated.ErrorMessage);
        Assert.True(secondCreated.IsSuccess, secondCreated.ErrorMessage);
        var secondDestination = StatePath(root, second.StateRef);
        var secondBackup =
            secondDestination + DesignConversionHandleBoundDirectory.PublicationBackupSuffix;
        var secondBytes = await File.ReadAllBytesAsync(secondDestination);
        File.Move(secondDestination, secondBackup, overwrite: false);
        var firstBytes = await File.ReadAllBytesAsync(StatePath(root, first.StateRef));
        var maximumSingle = Math.Max(firstBytes.LongLength, secondBytes.LongLength);
        var options = quota == "count"
            ? Options(
                maximumSingle,
                maxCount: 1,
                maxTotal: firstBytes.LongLength + secondBytes.LongLength)
            : Options(
                maximumSingle,
                maxCount: 8,
                maxTotal: firstBytes.LongLength + secondBytes.LongLength - 1L);

        var result = await new JsonDesignConversionPostEvaluationStateStore(root, options)
            .ReadAsync(second.StateRef);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.QuotaExceededErrorCode,
            result.ErrorCode);
        Assert.False(File.Exists(secondDestination));
        Assert.Equal(secondBytes, await File.ReadAllBytesAsync(secondBackup));
    }

    [Fact]
    public async Task ReadAsync_EntryCeilingAbortsBeforeOpeningAllAndNeverRecoversBackup()
    {
        var root = NewRoot("bounded-first-inventory");
        var recoverable = InitialReservedState("bounded-first-inventory-recoverable");
        var fixture = await SeedBackupOnlyAsync(root, recoverable);
        for (var index = 0; index < 20; index++)
        {
            var leaf =
                JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
                    "bounded-first-inventory-extra-" + index) +
                ".json" +
                (index % 2 == 0
                    ? string.Empty
                    : DesignConversionHandleBoundDirectory.PublicationBackupSuffix);
            await File.WriteAllBytesAsync(
                Path.Combine(root, leaf),
                StrictUtf8.GetBytes("bounded-entry-" + index));
        }
        var before = Directory.EnumerateFiles(root)
            .ToDictionary(
                path => Path.GetFileName(path),
                File.ReadAllBytes,
                StringComparer.Ordinal);
        var lockAcquired = 0;
        var leafOpensAfterLock = 0;
        var renameOrRecoveryStages = 0;
        const int maximumEntryCeiling = 2; // one persisted entry + required lock
        var options = Options(
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxSingleStateBytes,
            maxCount: 1,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxTotalStateBytes) with
        {
            BeforeHandleIoForTesting = (stage, _) =>
            {
                if (stage == DesignConversionHandleBoundIoStage.MutationLockAcquired)
                {
                    Interlocked.Exchange(ref lockAcquired, 1);
                    return;
                }
                if (stage == DesignConversionHandleBoundIoStage.LeafBeforeOpen &&
                    Volatile.Read(ref lockAcquired) == 1)
                {
                    Interlocked.Increment(ref leafOpensAfterLock);
                }
                if (stage is
                    DesignConversionHandleBoundIoStage.BeforeAtomicRename or
                    DesignConversionHandleBoundIoStage.AfterAtomicRenameBeforeDirectoryFlush or
                    DesignConversionHandleBoundIoStage
                        .BeforePublicationBackupRecoveryRename)
                {
                    Interlocked.Increment(ref renameOrRecoveryStages);
                }
            }
        };

        var result = await new JsonDesignConversionPostEvaluationStateStore(root, options)
            .ReadAsync(recoverable.StateRef);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.QuotaExceededErrorCode,
            result.ErrorCode);
        Assert.InRange(
            Volatile.Read(ref leafOpensAfterLock),
            0,
            maximumEntryCeiling);
        Assert.Equal(0, Volatile.Read(ref renameOrRecoveryStages));
        Assert.False(File.Exists(fixture.DestinationPath));
        var after = Directory.EnumerateFiles(root)
            .ToDictionary(
                path => Path.GetFileName(path),
                File.ReadAllBytes,
                StringComparer.Ordinal);
        Assert.Equal(
            before.Keys.OrderBy(name => name, StringComparer.Ordinal),
            after.Keys.OrderBy(name => name, StringComparer.Ordinal));
        foreach (var pair in before)
            Assert.Equal(pair.Value, after[pair.Key]);
        Assert.Equal(
            fixture.BackupBytes,
            await File.ReadAllBytesAsync(fixture.BackupPath));
    }

    [Fact]
    public void Constructor_RejectsEntryCeilingArithmeticOverflow()
    {
        var root = NewRoot("bounded-first-inventory-options-overflow");
        var options = Options(
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxSingleStateBytes,
            int.MaxValue,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxTotalStateBytes);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new JsonDesignConversionPostEvaluationStateStore(root, options));
        Assert.False(Directory.Exists(root));
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(long.MinValue)]
    [InlineData(65_537L)]
    public void DirectoryQueryByteCount_RejectsInvalidNativeCounts(long information)
    {
        var exception = Assert.Throws<InvalidDataException>(
            () => DesignConversionHandleBoundDirectory
                .ValidateDirectoryQueryByteCount(information));

        Assert.Equal(
            "Handle-bound directory enumeration returned an invalid byte count.",
            exception.Message);
    }

    [Theory]
    [InlineData(1L, 1)]
    [InlineData(65_536L, 65_536)]
    public void DirectoryQueryByteCount_AcceptsInclusiveNativeBounds(
        long information,
        int expected)
    {
        Assert.Equal(
            expected,
            DesignConversionHandleBoundDirectory
                .ValidateDirectoryQueryByteCount(information));
    }

    [Fact]
    public void DirectoryParser_CeilingPlusOneNeverMaterializesOverflowRecord()
    {
        var buffer = BuildDirectoryInformationBuffer(
            "first.json",
            "second.json",
            "invalid/name-that-must-not-be-validated");
        var materialized = 0;
        var callback = 0;

        Assert.Throws<DesignConversionHandleBoundEnumerationLimitException>(
            () => DesignConversionHandleBoundDirectory
                .ParseFileIdBothDirectoryInformationBuffer(
                    buffer,
                    buffer.Length,
                    maximumEntryCount: 2,
                    observedEntryCount: 0,
                    raw =>
                    {
                        materialized++;
                        return Encoding.Unicode.GetString(raw.Span);
                    },
                    _ => callback++));

        Assert.Equal(2, materialized);
        Assert.Equal(2, callback);
    }

    [Fact]
    public void DirectoryParser_DotEntriesConsumeNoCeilingOrMaterialization()
    {
        var buffer = BuildDirectoryInformationBuffer(".", "..", "only-entry.json");
        var materialized = 0;
        var callback = 0;

        var observed = DesignConversionHandleBoundDirectory
            .ParseFileIdBothDirectoryInformationBuffer(
                buffer,
                buffer.Length,
                maximumEntryCount: 1,
                observedEntryCount: 0,
                raw =>
                {
                    materialized++;
                    return Encoding.Unicode.GetString(raw.Span);
                },
                _ => callback++);

        Assert.Equal(1, observed);
        Assert.Equal(1, materialized);
        Assert.Equal(1, callback);
    }

    [Fact]
    public void DirectoryParser_RejectsEmbeddedNulLeafName()
    {
        var buffer = BuildDirectoryInformationBuffer("bad\0name.json");

        Assert.Throws<InvalidDataException>(
            () => DesignConversionHandleBoundDirectory
                .ParseFileIdBothDirectoryInformationBuffer(
                    buffer,
                    buffer.Length,
                    maximumEntryCount: 8,
                    observedEntryCount: 0,
                    static raw =>
                        new string(MemoryMarshal.Cast<byte, char>(raw.Span)),
                    _ => { }));
    }

    [Fact]
    public void DirectoryParser_PreservesWindowsLegalMultibyteLeafName()
    {
        var leafName = new string('\u754c', 86);
        var buffer = BuildDirectoryInformationBuffer(leafName);
        string? observedName = null;

        // Этот Windows-формат каталога хранит имя в UTF-16. Его нельзя
        // отклонять только потому, что то же имя заняло бы 258 байт в UTF-8.
        var observedCount = DesignConversionHandleBoundDirectory
            .ParseFileIdBothDirectoryInformationBuffer(
                buffer,
                buffer.Length,
                maximumEntryCount: 8,
                observedEntryCount: 0,
                static raw => new string(MemoryMarshal.Cast<byte, char>(raw.Span)),
                name => observedName = name);

        Assert.Equal(258, Encoding.UTF8.GetByteCount(leafName));
        Assert.Equal(1, observedCount);
        Assert.Equal(leafName, observedName);
    }

    [Fact]
    public void InventoryObservedSummary_BoundsExamplesAndEscapesControlCharacters()
    {
        var observed = Enumerable.Range(0, 20)
            .Select(index =>
                $"state-{index:D2}\n" + new string('x', 120))
            .ToArray();

        var summary =
            JsonDesignConversionPostEvaluationStateStore
                .BuildInventoryObservedSummary(
                    observed,
                    JsonDesignConversionPostEvaluationStateStore
                        .MutationLockFileName,
                    "missing-temp\0name",
                    lockObserved: false,
                    ownedTempObserved: false);

        Assert.Contains("Observed: count=20", summary, StringComparison.Ordinal);
        Assert.Contains("missing=", summary, StringComparison.Ordinal);
        Assert.Contains("\\u0000", summary, StringComparison.Ordinal);
        Assert.Contains("\\u000A", summary, StringComparison.Ordinal);
        Assert.Contains("state-07", summary, StringComparison.Ordinal);
        Assert.DoesNotContain("state-08", summary, StringComparison.Ordinal);
        Assert.DoesNotContain("\0", summary, StringComparison.Ordinal);
        Assert.DoesNotContain("\n", summary, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("too-short-header")]
    [InlineData("name-overlaps-next")]
    [InlineData("next-too-small")]
    [InlineData("next-misaligned")]
    [InlineData("next-out-of-buffer")]
    [InlineData("next-negative")]
    [InlineData("name-odd")]
    [InlineData("name-negative")]
    [InlineData("name-overflow")]
    [InlineData("bytes-over-fixed-buffer")]
    [InlineData("zero-success")]
    [InlineData("terminal-trailing-record")]
    public void DirectoryParser_RejectsMalformedStructureBeforeMaterialization(
        string mutation)
    {
        var (buffer, bytesReturned) = BuildMalformedDirectoryInformationBuffer(
            mutation);
        var materialized = 0;
        var callback = 0;

        Assert.Throws<InvalidDataException>(
            () => DesignConversionHandleBoundDirectory
                .ParseFileIdBothDirectoryInformationBuffer(
                    buffer,
                    bytesReturned,
                    maximumEntryCount: 8,
                    observedEntryCount: 0,
                    raw =>
                    {
                        materialized++;
                        return Encoding.Unicode.GetString(raw.Span);
                    },
                    _ => callback++));

        Assert.Equal(0, materialized);
        Assert.Equal(0, callback);
    }

    [Fact]
    public async Task WriteAsync_PostCommitCleanupFailurePreservesCommittedDestinationAndPriorBackup()
    {
        var root = NewRoot("post-commit-before-cleanup");
        var initial = InitialReservedState("post-commit-before-cleanup");
        var seeded = await new JsonDesignConversionPostEvaluationStateStore(root)
            .WriteAsync(initial);
        Assert.True(seeded.IsSuccess, seeded.ErrorMessage);
        var destinationPath = StatePath(root, initial.StateRef);
        var backupPath =
            destinationPath + DesignConversionHandleBoundDirectory.PublicationBackupSuffix;
        var priorBytes = await File.ReadAllBytesAsync(destinationPath);
        var candidate = seeded.Value! with
        {
            UpdatedAtUtc = seeded.Value!.UpdatedAtUtc.AddTicks(1)
        };
        var injected = 0;
        var options = Options(
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxSingleStateBytes,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxStateFileCount,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxTotalStateBytes) with
        {
            BeforeHandleIoForTesting = (stage, _) =>
            {
                if (stage ==
                        DesignConversionHandleBoundIoStage
                            .AfterDurableCommitBeforeBackupCleanup &&
                    Interlocked.Exchange(ref injected, 1) == 0)
                {
                    throw new IOException("injected-post-commit-cleanup-failure");
                }
            }
        };

        var failed = await new JsonDesignConversionPostEvaluationStateStore(root, options)
            .WriteAsync(candidate);

        Assert.False(failed.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.PublicationCleanupRequiredErrorCode,
            failed.ErrorCode);
        Assert.Equal(1, Volatile.Read(ref injected));
        var committedBytes = await File.ReadAllBytesAsync(destinationPath);
        Assert.NotEqual(priorBytes, committedBytes);
        Assert.Equal(priorBytes, await File.ReadAllBytesAsync(backupPath));
        var ambiguousRead = await new JsonDesignConversionPostEvaluationStateStore(root)
            .ReadAsync(initial.StateRef);
        Assert.False(ambiguousRead.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
            ambiguousRead.ErrorCode);
        Assert.Equal(committedBytes, await File.ReadAllBytesAsync(destinationPath));
        Assert.Equal(priorBytes, await File.ReadAllBytesAsync(backupPath));

        var validationRoot = NewRoot("post-commit-before-cleanup-strict-read");
        Directory.CreateDirectory(validationRoot);
        await File.WriteAllBytesAsync(
            StatePath(validationRoot, initial.StateRef),
            committedBytes);
        var strictRead = await new JsonDesignConversionPostEvaluationStateStore(validationRoot)
            .ReadAsync(initial.StateRef);
        Assert.True(strictRead.IsSuccess, strictRead.ErrorMessage);
        Assert.Equal(candidate.UpdatedAtUtc, strictRead.Value!.UpdatedAtUtc);
        Assert.Equal(seeded.Value.Version + 1L, strictRead.Value.Version);
    }

    [Fact]
    public async Task WriteAsync_FinalCleanupFlushFailureKeepsCommittedDestinationReadable()
    {
        var root = NewRoot("post-commit-final-flush");
        var initial = InitialReservedState("post-commit-final-flush");
        var seeded = await new JsonDesignConversionPostEvaluationStateStore(root)
            .WriteAsync(initial);
        Assert.True(seeded.IsSuccess, seeded.ErrorMessage);
        var destinationPath = StatePath(root, initial.StateRef);
        var priorBytes = await File.ReadAllBytesAsync(destinationPath);
        var candidate = seeded.Value! with
        {
            UpdatedAtUtc = seeded.Value!.UpdatedAtUtc.AddTicks(1)
        };
        var injected = 0;
        var options = Options(
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxSingleStateBytes,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxStateFileCount,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxTotalStateBytes) with
        {
            BeforeHandleIoForTesting = (stage, _) =>
            {
                if (stage ==
                        DesignConversionHandleBoundIoStage
                            .AfterBackupDeleteBeforeDirectoryFlush &&
                    Interlocked.Exchange(ref injected, 1) == 0)
                {
                    throw new IOException("injected-final-cleanup-flush-failure");
                }
            }
        };

        var failed = await new JsonDesignConversionPostEvaluationStateStore(root, options)
            .WriteAsync(candidate);

        Assert.False(failed.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.PublicationCleanupRequiredErrorCode,
            failed.ErrorCode);
        Assert.Equal(1, Volatile.Read(ref injected));
        Assert.NotEqual(priorBytes, await File.ReadAllBytesAsync(destinationPath));
        Assert.False(File.Exists(
            destinationPath + DesignConversionHandleBoundDirectory.PublicationBackupSuffix));
        var restarted = await new JsonDesignConversionPostEvaluationStateStore(root)
            .ReadAsync(initial.StateRef);
        Assert.True(restarted.IsSuccess, restarted.ErrorMessage);
        Assert.Equal(candidate.UpdatedAtUtc, restarted.Value!.UpdatedAtUtc);
        Assert.Equal(seeded.Value.Version + 1L, restarted.Value.Version);
    }

    [Fact]
    public async Task WriteAndRead_ReservationBackedStateRequiresHashOfExactReservationIdentity()
    {
        var artifactRoot = NewRoot("reservation-binding");
        var root = Path.Combine(
            artifactRoot,
            JsonDesignConversionPostEvaluationStateStore.DirectoryName);
        Directory.CreateDirectory(root);
        var valid = InitialReservedState("reservation-binding");
        var unrelatedRef = JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
            "unrelated-canonical-state-ref");

        var rejectedNew = await new JsonDesignConversionPostEvaluationStateStore(root)
            .WriteAsync(valid with { StateRef = unrelatedRef });
        AssertConflict(rejectedNew);

        var store = new JsonDesignConversionPostEvaluationStateStore(root);
        var created = await store.WriteAsync(valid);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        var attempted = await store.WriteAsync(created.Value! with
        {
            SettlementAttempted = true,
            UpdatedAtUtc = created.Value!.UpdatedAtUtc.AddTicks(1)
        });
        Assert.True(attempted.IsSuccess, attempted.ErrorMessage);
        var settled = await store.WriteAsync(attempted.Value! with
        {
            AccountingStatus = JsonDesignConversionPostEvaluationStateStore.AccountingSettled,
            AccountingDesiredAction = JsonDesignConversionPostEvaluationStateStore.DesiredNone,
            UpdatedAtUtc = attempted.Value!.UpdatedAtUtc.AddTicks(1)
        });
        Assert.True(settled.IsSuccess, settled.ErrorMessage);
        var committed = await store.WriteAsync(settled.Value! with
        {
            PersistenceStatus = JsonDesignConversionPostEvaluationStateStore.PersistenceCommitted,
            LedgerRecordId = "ledger-binding",
            ArtifactRecordId = "artifact-binding",
            CompletionBindingKind =
                JsonDesignConversionPostEvaluationStateStore
                    .CompletionBindingLocalArtifactDbRecord,
            CompletionBindingRef = "artifact-binding",
            UpdatedAtUtc = settled.Value!.UpdatedAtUtc.AddTicks(1)
        });
        Assert.True(committed.IsSuccess, committed.ErrorMessage);

        var validPath = StatePath(root, valid.StateRef);
        var substitutedPath = StatePath(root, unrelatedRef);
        var json = await File.ReadAllTextAsync(validPath, StrictUtf8);
        var substituted = json.Replace(
            valid.StateRef,
            unrelatedRef,
            StringComparison.Ordinal);
        Assert.NotEqual(json, substituted);
        await File.WriteAllTextAsync(validPath, substituted, StrictUtf8);
        File.Move(validPath, substitutedPath);

        var rejectedRead = await store.ReadAsync(unrelatedRef);
        Assert.False(rejectedRead.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.ReadFailedErrorCode,
            rejectedRead.ErrorCode);
        Assert.False(await JsonDesignConversionPostEvaluationStateStore.IsCommittedForTrainingAsync(
            artifactRoot,
            unrelatedRef,
            "artifact-binding"));
    }

    [Theory]
    [InlineData("user")]
    [InlineData("period-start")]
    [InlineData("period-end")]
    [InlineData("combined")]
    public async Task WriteAsync_NearMissNoReservationSentinelCannotBypassReservationHashBinding(
        string mutation)
    {
        var root = NewRoot("no-reservation-write-" + mutation);
        var candidate = MutateNoReservationSentinel(
            InitialNoReservationState("no-reservation-write-" + mutation),
            mutation);

        var result = await new JsonDesignConversionPostEvaluationStateStore(root)
            .WriteAsync(candidate);

        AssertConflict(result);
        Assert.False(File.Exists(StatePath(root, candidate.StateRef)));
    }

    [Theory]
    [InlineData("user")]
    [InlineData("period-start")]
    [InlineData("period-end")]
    [InlineData("combined")]
    public async Task ForgedNearMissNoReservationSentinelFailsStrictReadListAndEligibility(
        string mutation)
    {
        var artifactRoot = NewRoot("no-reservation-forged-" + mutation);
        var store = NewArtifactStore(artifactRoot);
        var sentinel = InitialNoReservationState(
            "no-reservation-forged-" + mutation);
        var created = await store.WriteAsync(sentinel);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        var committed = await store.WriteAsync(created.Value! with
        {
            PersistenceStatus =
                JsonDesignConversionPostEvaluationStateStore.PersistenceCommitted,
            LedgerRecordId = "no-reservation-ledger-" + mutation,
            ArtifactRecordId = "no-reservation-artifact-" + mutation,
            CompletionBindingKind =
                JsonDesignConversionPostEvaluationStateStore
                    .CompletionBindingLocalArtifactDbRecord,
            CompletionBindingRef = "no-reservation-artifact-" + mutation,
            UpdatedAtUtc = created.Value!.UpdatedAtUtc.AddTicks(1)
        });
        Assert.True(committed.IsSuccess, committed.ErrorMessage);

        var forged = MutateNoReservationSentinel(committed.Value!, mutation);
        var forgedBytes = JsonSerializer.SerializeToUtf8Bytes(
            new
            {
                schemaVersion =
                    JsonDesignConversionPostEvaluationStateStore.SchemaVersion,
                state = forged
            },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        await File.WriteAllBytesAsync(
            StatePath(store.RootDirectory, forged.StateRef),
            forgedBytes);

        var read = await new JsonDesignConversionPostEvaluationStateStore(
                store.RootDirectory)
            .ReadAsync(forged.StateRef);
        var listed = await new JsonDesignConversionPostEvaluationStateStore(
                store.RootDirectory)
            .ListAsync();

        Assert.False(read.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.ReadFailedErrorCode,
            read.ErrorCode);
        Assert.False(listed.IsSuccess);
        Assert.False(await JsonDesignConversionPostEvaluationStateStore.IsCommittedForTrainingAsync(
            artifactRoot,
            forged.StateRef,
            forged.ArtifactRecordId));
        Assert.Equal(
            forgedBytes,
            await File.ReadAllBytesAsync(
                StatePath(store.RootDirectory, forged.StateRef)));
    }

    [Fact]
    public async Task ReadAsync_OversizedStateFailsBeforeAllocation()
    {
        var root = NewRoot("oversized-read");
        var store = new JsonDesignConversionPostEvaluationStateStore(root);
        var state = InitialReservedState("oversized-read");
        var written = await store.WriteAsync(state);
        Assert.True(written.IsSuccess, written.ErrorMessage);
        await using (var stream = new FileStream(
                         StatePath(root, state.StateRef),
                         FileMode.Open,
                         FileAccess.Write,
                         FileShare.None))
        {
            stream.SetLength(store.MaxSingleStateBytes + 1L);
        }

        var read = await store.ReadAsync(state.StateRef);

        Assert.False(read.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.QuotaExceededErrorCode,
            read.ErrorCode);
    }

    [Fact]
    public async Task WriteAsync_EnforcesSingleStateFileCountAndTotalByteQuotas()
    {
        var countRoot = NewRoot("count-quota");
        var countStore = new JsonDesignConversionPostEvaluationStateStore(
            countRoot,
            Options(maxSingle: 4096, maxCount: 1, maxTotal: 8192));
        var first = await countStore.WriteAsync(InitialReservedState("count-first"));
        var second = await countStore.WriteAsync(InitialReservedState("count-second"));
        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.QuotaExceededErrorCode,
            second.ErrorCode);

        var totalRoot = NewRoot("total-quota");
        var totalStore = new JsonDesignConversionPostEvaluationStateStore(
            totalRoot,
            Options(maxSingle: 4096, maxCount: 8, maxTotal: 4096));
        var largeFirst = await totalStore.WriteAsync(
            InitialReservedState("total-first") with { UserRef = new string('a', 2500) });
        var largeSecond = await totalStore.WriteAsync(
            InitialReservedState("total-second") with { UserRef = new string('b', 2500) });
        Assert.True(largeFirst.IsSuccess, largeFirst.ErrorMessage);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.QuotaExceededErrorCode,
            largeSecond.ErrorCode);

        var singleRoot = NewRoot("single-quota");
        var singleStore = new JsonDesignConversionPostEvaluationStateStore(
            singleRoot,
            Options(maxSingle: 1024, maxCount: 8, maxTotal: 4096));
        var oversizeWrite = await singleStore.WriteAsync(
            InitialReservedState("single-oversize") with { UserRef = new string('x', 2000) });
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.QuotaExceededErrorCode,
            oversizeWrite.ErrorCode);
    }

    [Fact]
    public async Task ListAsync_RejectsFileCountAndTotalByteQuotaExceededOnDisk()
    {
        var countRoot = NewRoot("list-count-quota");
        var seedStore = new JsonDesignConversionPostEvaluationStateStore(countRoot);
        var firstState = InitialReservedState("list-count-first");
        var first = await seedStore.WriteAsync(firstState);
        Assert.True(first.IsSuccess, first.ErrorMessage);
        var secondState = InitialReservedState("list-count-second");
        var firstJson = await File.ReadAllTextAsync(
            StatePath(countRoot, firstState.StateRef),
            StrictUtf8);
        var secondJson = firstJson.Replace(
            firstState.StateRef,
            secondState.StateRef,
            StringComparison.Ordinal);
        await File.WriteAllTextAsync(
            StatePath(countRoot, secondState.StateRef),
            secondJson,
            StrictUtf8);
        var countList = await new JsonDesignConversionPostEvaluationStateStore(
                countRoot,
                Options(maxSingle: 4096, maxCount: 1, maxTotal: 8192))
            .ListAsync();
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.QuotaExceededErrorCode,
            countList.ErrorCode);

        var totalBytes = new FileInfo(StatePath(countRoot, firstState.StateRef)).Length +
                         new FileInfo(StatePath(countRoot, secondState.StateRef)).Length;
        var totalList = await new JsonDesignConversionPostEvaluationStateStore(
                countRoot,
                Options(
                    maxSingle: totalBytes - 1L,
                    maxCount: 4,
                    maxTotal: totalBytes - 1L))
            .ListAsync();
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.QuotaExceededErrorCode,
            totalList.ErrorCode);
    }

    [Fact]
    public async Task ListAsync_AbandonedTempAndTornCanonicalStateFailClosed()
    {
        var tempRoot = NewRoot("abandoned-temp");
        var tempStore = new JsonDesignConversionPostEvaluationStateStore(tempRoot);
        var state = InitialReservedState("abandoned-temp");
        var written = await tempStore.WriteAsync(state);
        Assert.True(written.IsSuccess, written.ErrorMessage);
        var abandonedTemp = Path.Combine(
            tempRoot,
            $"{state.StateRef}.json.{Guid.NewGuid():N}" +
            JsonDesignConversionPostEvaluationStateStore.WriteTempSuffix);
        await File.WriteAllTextAsync(abandonedTemp, "{", StrictUtf8);
        var tempList = await tempStore.ListAsync();
        Assert.False(tempList.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
            tempList.ErrorCode);

        var tornRoot = NewRoot("torn-state");
        Directory.CreateDirectory(tornRoot);
        var tornState = InitialReservedState("torn-state");
        await File.WriteAllTextAsync(
            StatePath(tornRoot, tornState.StateRef),
            "{\"schemaVersion\":",
            StrictUtf8);
        var tornRead = await new JsonDesignConversionPostEvaluationStateStore(tornRoot)
            .ReadAsync(tornState.StateRef);
        Assert.False(tornRead.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.ReadFailedErrorCode,
            tornRead.ErrorCode);
    }

    [Fact]
    public async Task WriteAsync_CancellationAndCrossProcessLockTimeoutAreBounded()
    {
        var cancelledStore = new JsonDesignConversionPostEvaluationStateStore(
            NewRoot("cancelled"));
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        var cancelledResult = await cancelledStore.WriteAsync(
            InitialReservedState("cancelled"),
            cancelled.Token);
        Assert.Equal("CANCELLED", cancelledResult.ErrorCode);

        var lockRoot = NewRoot("lock-timeout");
        Directory.CreateDirectory(lockRoot);
        var lockPath = Path.Combine(
            lockRoot,
            JsonDesignConversionPostEvaluationStateStore.MutationLockFileName);
        await using var heldLock = new FileStream(
            lockPath,
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.None);
        var timeoutStore = new JsonDesignConversionPostEvaluationStateStore(
            lockRoot,
            Options(
                maxSingle: 4096,
                maxCount: 8,
                maxTotal: 8192,
                lockTimeout: TimeSpan.FromMilliseconds(150),
                retry: TimeSpan.FromMilliseconds(20)));

        var timeoutResult = await timeoutStore.WriteAsync(
            InitialReservedState("lock-timeout"));

        Assert.False(timeoutResult.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.MutationLockTimeoutErrorCode,
            timeoutResult.ErrorCode);
    }

    [Fact]
    public async Task WriteAsync_HeldMutationLockBlocksWindowsNamespaceTamperingAndSerializesIndependentStores()
    {
        var root = NewRoot("held-mutation-lock");
        var firstLockAcquired = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var secondLockAcquired = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using var releaseFirst = new ManualResetEventSlim(initialState: false);
        var firstOptions = Options(
            maxSingle: 4096,
            maxCount: 8,
            maxTotal: 8192,
            lockTimeout: TimeSpan.FromSeconds(5),
            retry: TimeSpan.FromMilliseconds(10)) with
        {
            BeforeHandleIoForTesting = (stage, _) =>
            {
                if (stage != DesignConversionHandleBoundIoStage.MutationLockAcquired)
                    return;
                firstLockAcquired.TrySetResult(true);
                Assert.True(releaseFirst.Wait(TimeSpan.FromSeconds(10)));
            }
        };
        var secondOptions = Options(
            maxSingle: 4096,
            maxCount: 8,
            maxTotal: 8192,
            lockTimeout: TimeSpan.FromSeconds(5),
            retry: TimeSpan.FromMilliseconds(10)) with
        {
            BeforeHandleIoForTesting = (stage, _) =>
            {
                if (stage == DesignConversionHandleBoundIoStage.MutationLockAcquired)
                    secondLockAcquired.TrySetResult(true);
            }
        };
        var firstStore = new JsonDesignConversionPostEvaluationStateStore(
            root,
            firstOptions);
        var secondStore = new JsonDesignConversionPostEvaluationStateStore(
            root,
            secondOptions);

        var firstWrite = Task.Run(() => firstStore.WriteAsync(
            InitialReservedState("held-mutation-lock-first")));
        await firstLockAcquired.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var lockPath = Path.Combine(
            root,
            JsonDesignConversionPostEvaluationStateStore.MutationLockFileName);
        var renamedLockPath = lockPath + ".renamed";

        if (OperatingSystem.IsWindows())
        {
            Assert.ThrowsAny<IOException>(() => File.Move(lockPath, renamedLockPath));
            Assert.ThrowsAny<IOException>(() => File.Delete(lockPath));
            Assert.ThrowsAny<IOException>(() =>
            {
                using var replacement = new FileStream(
                    lockPath,
                    FileMode.Create,
                    FileAccess.ReadWrite,
                    FileShare.None);
            });
        }

        Assert.True(File.Exists(lockPath));
        Assert.False(File.Exists(renamedLockPath));

        var secondWrite = secondStore.WriteAsync(
            InitialReservedState("held-mutation-lock-second"));
        await Task.Delay(TimeSpan.FromMilliseconds(100));
        Assert.False(secondLockAcquired.Task.IsCompleted);
        Assert.False(secondWrite.IsCompleted);

        releaseFirst.Set();
        var firstResult = await firstWrite.WaitAsync(TimeSpan.FromSeconds(5));
        var secondResult = await secondWrite.WaitAsync(TimeSpan.FromSeconds(5));
        await secondLockAcquired.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(firstResult.IsSuccess, firstResult.ErrorMessage);
        Assert.True(secondResult.IsSuccess, secondResult.ErrorMessage);
        Assert.True(File.Exists(lockPath));
        Assert.Equal(0L, new FileInfo(lockPath).Length);
    }

    [PostEvaluationStateStoreReparseFact]
    public async Task ListAsync_RootReparsePointFailsClosedWithoutFollowingTarget()
    {
        var parent = NewRoot("root-reparse-parent");
        var target = NewRoot("root-reparse-target");
        Directory.CreateDirectory(parent);
        Directory.CreateDirectory(target);
        var linkedRoot = Path.Combine(parent, "linked-state-root");
        Directory.CreateSymbolicLink(linkedRoot, target);
        Assert.NotEqual(
            FileAttributes.None,
            File.GetAttributes(linkedRoot) & FileAttributes.ReparsePoint);

        var result = await new JsonDesignConversionPostEvaluationStateStore(linkedRoot)
            .ListAsync();

        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
            result.ErrorCode);
    }

    [PostEvaluationStateStoreReparseFact]
    public async Task WriteAsync_ExistingParentSegmentReparsePointFailsClosed()
    {
        var parent = NewRoot("segment-reparse-parent");
        var target = NewRoot("segment-reparse-target");
        Directory.CreateDirectory(parent);
        Directory.CreateDirectory(target);
        var link = Path.Combine(parent, "linked-segment");
        Directory.CreateSymbolicLink(link, target);
        var store = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(link, "state-root"));

        var result = await store.WriteAsync(InitialReservedState("segment-reparse"));

        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
            result.ErrorCode);
        Assert.Empty(Directory.EnumerateFileSystemEntries(target));
    }

    [PostEvaluationStateStoreReparseFact]
    public async Task ReadAsync_ActiveStateFileReparsePointFailsClosedWithoutFollowingTarget()
    {
        var sourceRoot = NewRoot("active-reparse-source");
        var sourceStore = new JsonDesignConversionPostEvaluationStateStore(sourceRoot);
        var state = InitialReservedState("active-reparse");
        var sourceWrite = await sourceStore.WriteAsync(state);
        Assert.True(sourceWrite.IsSuccess, sourceWrite.ErrorMessage);
        var sourcePath = StatePath(sourceRoot, state.StateRef);
        var sourceBytes = await File.ReadAllBytesAsync(sourcePath);

        var linkedRoot = NewRoot("active-reparse-linked");
        Directory.CreateDirectory(linkedRoot);
        var linkedPath = StatePath(linkedRoot, state.StateRef);
        File.CreateSymbolicLink(linkedPath, sourcePath);
        Assert.NotEqual(
            FileAttributes.None,
            File.GetAttributes(linkedPath) & FileAttributes.ReparsePoint);

        var result = await new JsonDesignConversionPostEvaluationStateStore(linkedRoot)
            .ReadAsync(state.StateRef);

        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
            result.ErrorCode);
        Assert.Equal(sourceBytes, await File.ReadAllBytesAsync(sourcePath));
    }

    [PostEvaluationStateStoreReparseFact]
    public async Task WriteAsync_MutationLockReparsePointFailsClosedWithoutFollowingTarget()
    {
        var sourceRoot = NewRoot("lock-reparse-source");
        Directory.CreateDirectory(sourceRoot);
        var sourceLock = Path.Combine(sourceRoot, "outside.lock");
        await File.WriteAllBytesAsync(sourceLock, []);

        var linkedRoot = NewRoot("lock-reparse-linked");
        Directory.CreateDirectory(linkedRoot);
        var linkedLock = Path.Combine(
            linkedRoot,
            JsonDesignConversionPostEvaluationStateStore.MutationLockFileName);
        File.CreateSymbolicLink(linkedLock, sourceLock);

        var result = await new JsonDesignConversionPostEvaluationStateStore(linkedRoot)
            .WriteAsync(InitialReservedState("lock-reparse"));

        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
            result.ErrorCode);
        Assert.Empty(await File.ReadAllBytesAsync(sourceLock));
    }

    [PostEvaluationStateStoreReparseFact]
    public async Task ListAsync_AbandonedTempReparsePointFailsClosedWithoutFollowingTarget()
    {
        var sourceRoot = NewRoot("temp-reparse-source");
        Directory.CreateDirectory(sourceRoot);
        var sourceTemp = Path.Combine(sourceRoot, "outside.tmp");
        await File.WriteAllTextAsync(sourceTemp, "outside", StrictUtf8);
        var outsideBytes = await File.ReadAllBytesAsync(sourceTemp);

        var linkedRoot = NewRoot("temp-reparse-linked");
        Directory.CreateDirectory(linkedRoot);
        var state = InitialReservedState("temp-reparse");
        var linkedTemp = Path.Combine(
            linkedRoot,
            $"{state.StateRef}.json.{Guid.NewGuid():N}" +
            JsonDesignConversionPostEvaluationStateStore.WriteTempSuffix);
        File.CreateSymbolicLink(linkedTemp, sourceTemp);

        var result = await new JsonDesignConversionPostEvaluationStateStore(linkedRoot)
            .ListAsync();

        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
            result.ErrorCode);
        Assert.Equal(outsideBytes, await File.ReadAllBytesAsync(sourceTemp));
    }

    private static JsonDesignConversionPostEvaluationStateStore NewArtifactStore(
        string artifactRoot)
        => new(
            Path.Combine(
                artifactRoot,
                JsonDesignConversionPostEvaluationStateStore.DirectoryName));

    private static async Task<TrainingDenialFixture>
        SeedCommittedTrainingDenialAsync(string scope)
    {
        var artifactRoot = NewRoot(scope);
        var store = NewArtifactStore(artifactRoot);
        var created = await store.WriteAsync(
            InitialNoReservationState(scope));
        Assert.True(created.IsSuccess, created.ErrorMessage);
        var artifactRecordId = "artifact-" + scope;
        var denied = await store.DenyTrainingEligibilityAsync(
            created.Value!.StateRef,
            artifactRecordId,
            "TEST_COMMIT_PENDING");
        Assert.True(denied.IsSuccess, denied.ErrorMessage);
        var committed = await store.WriteAsync(
            created.Value with
            {
                PersistenceStatus =
                    JsonDesignConversionPostEvaluationStateStore
                        .PersistenceCommitted,
                ArtifactRecordId = artifactRecordId,
                CompletionBindingKind =
                    JsonDesignConversionPostEvaluationStateStore
                        .CompletionBindingLocalArtifactDbRecord,
                CompletionBindingRef = artifactRecordId,
                UpdatedAtUtc =
                    created.Value.UpdatedAtUtc.AddTicks(1),
                Version = created.Value.Version
            });
        Assert.True(committed.IsSuccess, committed.ErrorMessage);
        var denialDirectory =
            store.RootDirectory +
            JsonDesignConversionPostEvaluationStateStore
                .TrainingEligibilityDenialDirectorySuffix;
        var denialPath = Assert.Single(
            Directory.EnumerateFiles(
                denialDirectory,
                "*" +
                JsonDesignConversionPostEvaluationStateStore
                    .TrainingEligibilityDenialFileSuffix,
                SearchOption.TopDirectoryOnly));
        return new TrainingDenialFixture(
            artifactRoot,
            store,
            committed.Value!,
            artifactRecordId,
            denialDirectory,
            denialPath);
    }

    private static async Task<PublicationBackupFixture> SeedBackupOnlyAsync(
        string root,
        DesignConversionPostEvaluationState state,
        string? backupStateRef = null)
    {
        var seeded = await new JsonDesignConversionPostEvaluationStateStore(root)
            .WriteAsync(state);
        Assert.True(seeded.IsSuccess, seeded.ErrorMessage);
        var sourcePath = StatePath(root, state.StateRef);
        var destinationPath = StatePath(root, backupStateRef ?? state.StateRef);
        var backupPath =
            destinationPath + DesignConversionHandleBoundDirectory.PublicationBackupSuffix;
        var bytes = await File.ReadAllBytesAsync(sourcePath);
        File.Move(sourcePath, backupPath, overwrite: false);
        return new PublicationBackupFixture(
            destinationPath,
            backupPath,
            bytes);
    }

    private static byte[] SerializeStateEnvelope(
        DesignConversionPostEvaluationState state)
        => JsonSerializer.SerializeToUtf8Bytes(
            new
            {
                schemaVersion =
                    JsonDesignConversionPostEvaluationStateStore.SchemaVersion,
                state
            },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private static byte[] SerializeLegacyStateEnvelope(
        DesignConversionPostEvaluationState state)
        => JsonSerializer.SerializeToUtf8Bytes(
            new
            {
                schemaVersion =
                    JsonDesignConversionPostEvaluationStateStore.LegacySchemaVersion,
                state = new
                {
                    state.StateRef,
                    state.ReservationRef,
                    state.UserRef,
                    state.PeriodStartUtc,
                    state.PeriodEndExclusiveUtc,
                    state.ChargedCostUsd,
                    state.AccountingStatus,
                    state.AccountingDesiredAction,
                    state.SettlementAttempted,
                    state.SettlementDefinitivelyNotApplied,
                    state.ReleaseAttempted,
                    state.PersistenceStatus,
                    state.LedgerRecordId,
                    state.ArtifactRecordId,
                    state.LastErrorCode,
                    state.UpdatedAtUtc,
                    state.Version
                }
            },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private static byte[] BuildDirectoryInformationBuffer(params string[] names)
    {
        const int nameOffset = 104;
        var encoded = names
            .Select(name => Encoding.Unicode.GetBytes(name))
            .ToArray();
        var recordLengths = encoded
            .Select((name, index) =>
                index == encoded.Length - 1
                    ? checked(nameOffset + name.Length)
                    : checked((nameOffset + name.Length + 7) & ~7))
            .ToArray();
        var buffer = new byte[recordLengths.Sum()];
        var offset = 0;
        for (var index = 0; index < encoded.Length; index++)
        {
            var nextOffset = index == encoded.Length - 1
                ? 0
                : recordLengths[index];
            BinaryPrimitives.WriteInt32LittleEndian(
                buffer.AsSpan(offset, sizeof(int)),
                nextOffset);
            BinaryPrimitives.WriteInt32LittleEndian(
                buffer.AsSpan(offset + 60, sizeof(int)),
                encoded[index].Length);
            encoded[index].CopyTo(buffer.AsSpan(offset + nameOffset));
            offset += recordLengths[index];
        }
        return buffer;
    }

    private static (byte[] Buffer, int BytesReturned)
        BuildMalformedDirectoryInformationBuffer(string mutation)
    {
        const int nameOffset = 104;
        switch (mutation)
        {
            case "too-short-header":
                return (new byte[nameOffset - 1], nameOffset - 1);
            case "name-overlaps-next":
            {
                var buffer = BuildDirectoryInformationBuffer("overlap", "next");
                BinaryPrimitives.WriteInt32LittleEndian(
                    buffer.AsSpan(0, sizeof(int)),
                    nameOffset);
                return (buffer, buffer.Length);
            }
            case "next-too-small":
            {
                var buffer = BuildDirectoryInformationBuffer("first", "next");
                BinaryPrimitives.WriteInt32LittleEndian(
                    buffer.AsSpan(0, sizeof(int)),
                    nameOffset - 8);
                return (buffer, buffer.Length);
            }
            case "next-misaligned":
            {
                var buffer = BuildDirectoryInformationBuffer("first", "next");
                BinaryPrimitives.WriteInt32LittleEndian(
                    buffer.AsSpan(0, sizeof(int)),
                    nameOffset + 1);
                return (buffer, buffer.Length);
            }
            case "next-out-of-buffer":
            {
                var buffer = BuildDirectoryInformationBuffer("first", "next");
                BinaryPrimitives.WriteInt32LittleEndian(
                    buffer.AsSpan(0, sizeof(int)),
                    checked((buffer.Length + 15) & ~7));
                return (buffer, buffer.Length);
            }
            case "next-negative":
            {
                var buffer = BuildDirectoryInformationBuffer("first", "next");
                BinaryPrimitives.WriteInt32LittleEndian(
                    buffer.AsSpan(0, sizeof(int)),
                    -8);
                return (buffer, buffer.Length);
            }
            case "name-odd":
            case "name-negative":
            case "name-overflow":
            {
                var buffer = new byte[nameOffset];
                var nameLength = mutation switch
                {
                    "name-odd" => 1,
                    "name-negative" => -2,
                    _ => int.MaxValue - 1
                };
                BinaryPrimitives.WriteInt32LittleEndian(
                    buffer.AsSpan(60, sizeof(int)),
                    nameLength);
                return (buffer, buffer.Length);
            }
            case "bytes-over-fixed-buffer":
            {
                var buffer =
                    new byte[DesignConversionHandleBoundDirectory.DirectoryBufferBytes + 1];
                return (buffer, buffer.Length);
            }
            case "zero-success":
                return (new byte[1], 0);
            case "terminal-trailing-record":
            {
                var terminal = BuildDirectoryInformationBuffer("terminal");
                Array.Resize(ref terminal, terminal.Length + nameOffset);
                return (terminal, terminal.Length);
            }
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(mutation),
                    mutation,
                    null);
        }
    }

    private static DesignConversionPostEvaluationState InitialReservedState(string scope)
    {
        var reservationRef = "reservation-" + scope;
        var stateRef = JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
            reservationRef);
        return new DesignConversionPostEvaluationState(
            stateRef,
            reservationRef,
            "user-" + scope,
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch.AddMonths(1),
            0.25m,
            JsonDesignConversionPostEvaluationStateStore.AccountingPending,
            JsonDesignConversionPostEvaluationStateStore.DesiredSettle,
            SettlementAttempted: false,
            SettlementDefinitivelyNotApplied: false,
            ReleaseAttempted: false,
            JsonDesignConversionPostEvaluationStateStore.PersistenceProvisional,
            LedgerRecordId: string.Empty,
            ArtifactRecordId: string.Empty,
            CompletionBindingKind: string.Empty,
            CompletionBindingRef: string.Empty,
            LastErrorCode: string.Empty,
            UpdatedAtUtc: new DateTimeOffset(2026, 7, 30, 0, 0, 0, TimeSpan.Zero),
            Version: 1L);
    }

    private static DesignConversionPostEvaluationState InitialNoReservationState(string scope)
    {
        var stateRef = JsonDesignConversionPostEvaluationStateStore.BuildStateRef(scope);
        return new DesignConversionPostEvaluationState(
            stateRef,
            stateRef,
            "not-applicable",
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch.AddTicks(1),
            0m,
            JsonDesignConversionPostEvaluationStateStore.AccountingSettled,
            JsonDesignConversionPostEvaluationStateStore.DesiredNone,
            SettlementAttempted: false,
            SettlementDefinitivelyNotApplied: false,
            ReleaseAttempted: false,
            JsonDesignConversionPostEvaluationStateStore.PersistenceProvisional,
            LedgerRecordId: string.Empty,
            ArtifactRecordId: string.Empty,
            CompletionBindingKind: string.Empty,
            CompletionBindingRef: string.Empty,
            LastErrorCode: string.Empty,
            UpdatedAtUtc: new DateTimeOffset(2026, 7, 30, 0, 0, 0, TimeSpan.Zero),
            Version: 1L);
    }

    private static DesignConversionPostEvaluationState MutateNoReservationSentinel(
        DesignConversionPostEvaluationState sentinel,
        string mutation)
        => mutation switch
        {
            "user" => sentinel with
            {
                UserRef = "not-the-no-reservation-sentinel"
            },
            "period-start" => sentinel with
            {
                PeriodStartUtc =
                    JsonDesignConversionPostEvaluationStateStore.NoReservationPeriodStartUtc
                        .AddTicks(1)
            },
            "period-end" => sentinel with
            {
                PeriodEndExclusiveUtc =
                    JsonDesignConversionPostEvaluationStateStore
                        .NoReservationPeriodEndExclusiveUtc
                        .AddTicks(1)
            },
            "combined" => sentinel with
            {
                UserRef = "not-the-no-reservation-sentinel",
                PeriodStartUtc =
                    JsonDesignConversionPostEvaluationStateStore.NoReservationPeriodStartUtc
                        .AddTicks(1),
                PeriodEndExclusiveUtc =
                    JsonDesignConversionPostEvaluationStateStore
                        .NoReservationPeriodEndExclusiveUtc
                        .AddTicks(2)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null)
        };

    private static DesignConversionPostEvaluationStateStoreOptions Options(
        long maxSingle,
        int maxCount,
        long maxTotal,
        TimeSpan? lockTimeout = null,
        TimeSpan? retry = null)
        => new(
            maxSingle,
            maxCount,
            maxTotal,
            lockTimeout ?? TimeSpan.FromSeconds(2),
            retry ?? TimeSpan.FromMilliseconds(10));

    private static string StatePath(string root, string stateRef)
        => Path.Combine(root, stateRef + ".json");

    private static DesignConversionHandleBoundEntry CaptureEntry(
        string root,
        string leafName)
    {
        using var directory =
            DesignConversionHandleBoundDirectory.AcquireExisting(
                root,
                beforeIo: null);
        using var held = directory.OpenOrCreate(
            JsonDesignConversionPostEvaluationStateStore
                .MutationLockFileName,
            FileShare.None,
            writeThrough: true);
        return directory
            .EnumerateEntries(
                maximumEntryCount: 512,
                JsonDesignConversionPostEvaluationStateStore
                    .MutationLockFileName,
                held)
            .Single(entry => string.Equals(
                entry.Name,
                leafName,
                StringComparison.Ordinal));
    }

    private static uint CaptureLinkCount(string root, string leafName)
    {
        using var directory =
            DesignConversionHandleBoundDirectory.AcquireExisting(
                root,
                beforeIo: null);
        using var held = directory.OpenOrCreate(
            JsonDesignConversionPostEvaluationStateStore
                .MutationLockFileName,
            FileShare.None,
            writeThrough: true);
        using var stream = directory.TryOpenExisting(
            leafName,
            FileShare.Read | FileShare.Write | FileShare.Delete,
            writeThrough: false)
            ?? throw new FileNotFoundException(
                "Expected handle-bound entry was missing.",
                leafName);
        // Удержанный lock handle сохраняет тот же handle-bound режим проверки,
        // что и production inventory: link count читается у реального leaf.
        _ = held.Length;
        return directory.GetLinkCount(stream, leafName);
    }

    private static string NewRepoRootStateRoot(string scope)
        => Path.Combine(
            DesignConversionPostEvaluationEligibilityTestFixture
                .CreateHandleBoundTempRoot(scope),
            JsonDesignConversionPostEvaluationStateStore.DirectoryName);

    private static string NewRoot(string scope)
        => Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "post-evaluation-state-" + scope);

    private static string BuildTestTempLeaf(string stateRef)
        => $"{stateRef}.json.0123456789abcdef0123456789abcdef" +
           JsonDesignConversionPostEvaluationStateStore.WriteTempSuffix;

    private static bool TryCreateHardLink(string existingPath, string newPath)
        => OperatingSystem.IsWindows()
            ? CreateHardLinkWindows(newPath, existingPath, IntPtr.Zero)
            : LinkLinux(existingPath, newPath) == 0;

    private static void AssertConflict<T>(AgentInfrastructure.OperationResult<T> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.ConflictErrorCode,
            result.ErrorCode);
    }

    private sealed record PublicationBackupFixture(
        string DestinationPath,
        string BackupPath,
        byte[] BackupBytes);

    private sealed record TrainingDenialFixture(
        string ArtifactRoot,
        JsonDesignConversionPostEvaluationStateStore Store,
        DesignConversionPostEvaluationState State,
        string ArtifactRecordId,
        string DenialDirectory,
        string DenialPath);

    [DllImport("libc", EntryPoint = "link", SetLastError = true)]
    private static extern int LinkLinux(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string oldPath,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string newPath);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "CreateHardLinkW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern bool CreateHardLinkWindows(
        string lpFileName,
        string lpExistingFileName,
        IntPtr lpSecurityAttributes);
}

file sealed class PostEvaluationStateStoreReparseFactAttribute : FactAttribute
{
    public PostEvaluationStateStoreReparseFactAttribute()
        => Skip = PostEvaluationStateStoreReparseTestSupport.GetCapabilitySkipReason();
}

file static class PostEvaluationStateStoreReparseTestSupport
{
    // HRESULT_FROM_WIN32(ERROR_PRIVILEGE_NOT_HELD).
    private const int HResultPrivilegeNotHeld = unchecked((int)0x80070522);

    public static string? GetCapabilitySkipReason()
    {
        if (!OperatingSystem.IsWindows())
            return null;

        var root = Path.Combine(
            Path.GetTempPath(),
            $"post-evaluation-state-reparse-capability-{Guid.NewGuid():N}");
        var target = Path.Combine(root, "target");
        var link = Path.Combine(root, "link");
        try
        {
            Directory.CreateDirectory(target);
            try
            {
                Directory.CreateSymbolicLink(link, target);
                return null;
            }
            catch (Exception exception)
                when ((exception is IOException or UnauthorizedAccessException) &&
                      exception.HResult == HResultPrivilegeNotHeld)
            {
                return "REPARSE_TEST_PLATFORM_LIMITATION: " +
                       $"{exception.GetType().FullName}, HResult=0x{exception.HResult:X8}: " +
                       "creating symbolic links requires SeCreateSymbolicLinkPrivilege.";
            }
        }
        finally
        {
            try
            {
                if (Directory.Exists(link))
                    Directory.Delete(link);
                if (Directory.Exists(root))
                    Directory.Delete(root, recursive: true);
            }
            catch
            {
                // Capability probe cleanup cannot alter the product verdict.
            }
        }
    }
}
