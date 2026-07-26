// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed class DesignConversionLocalArtifactDbTrainingLocalPromotionDurabilityTests
{
    private const string PromotionJournalFileName = "training-intake-local-promotions.jsonl";
    private const string PromotionMutationLockFileName = PromotionJournalFileName + ".mutation.lock";
    private const string TempSuffix = ".copyonwrite.tmp";

    private static readonly Encoding StrictUtf8 = new UTF8Encoding(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private static readonly JsonSerializerOptions CompactPromotionJournalJsonOptions = new(
        DesignConversionCheckpointJsonOptions.Standard)
    {
        WriteIndented = false
    };

    [Fact]
    public async Task RunTrainingIntakeAndPromoteLocalAsync_DifferentBlockedPromotionKeys_PersistStrictRowsAndReplayDoesNotAppend()
    {
        var root = CreateTempRoot("training-local-promotion-durable-two-rows");
        var journalPath = Path.Combine(root, PromotionJournalFileName);

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(db, "two-rows");
            var options = BuildPromotionOptions("r7-b3-two-rows-run");
            var firstAdmission = BuildAdmissionMetadata(
                "tenant:r7-b3-two-rows",
                "idempotency:r7-b3-two-rows-001");
            var secondAdmission = BuildAdmissionMetadata(
                "tenant:r7-b3-two-rows",
                "idempotency:r7-b3-two-rows-002");
            var promotionOutputDirectory = Path.Combine(root, "blocked-promotion-output");

            var first = await RunPromoteWithAdmissionAsync(
                db,
                intake.IntakeId,
                promotionOutputDirectory,
                options,
                firstAdmission);
            var second = await RunPromoteWithAdmissionAsync(
                db,
                intake.IntakeId,
                promotionOutputDirectory,
                options,
                secondAdmission);

            var firstPromotion = AssertBlockedPromotionSuccess(first, intake.IntakeId, options.RunId);
            var secondPromotion = AssertBlockedPromotionSuccess(second, intake.IntakeId, options.RunId);
            Assert.True(IsCreatedSideEffect(firstPromotion));
            Assert.True(IsCreatedSideEffect(secondPromotion));

            var physicalRecords = ReadPhysicalPromotionJournalRecords(journalPath);
            Assert.Equal(2, physicalRecords.Count);
            Assert.Equal(
                ["idempotency:r7-b3-two-rows-001", "idempotency:r7-b3-two-rows-002"],
                physicalRecords
                    .Select(record => ReadMetadata(record, "training_intake_idempotency_key_ref"))
                    .ToArray());
            Assert.All(physicalRecords, record =>
            {
                Assert.False(record.LocalPromotionReady);
                Assert.False(record.TrainingRun.TrainingRunnerInvoked);
            });
            var originalBytes = await File.ReadAllBytesAsync(journalPath);

            var replay = await RunPromoteWithAdmissionAsync(
                new JsonlDesignConversionLocalArtifactDb(root),
                intake.IntakeId,
                promotionOutputDirectory,
                options,
                firstAdmission);

            var replayPromotion = AssertBlockedPromotionSuccess(replay, intake.IntakeId, options.RunId);
            Assert.True(IsReplay(replayPromotion));
            Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
            Assert.Equal(2, ReadPhysicalPromotionJournalRecords(journalPath).Count);
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAndPromoteLocalAsync_LockedExistingPromotionJournal_FailsSafelyPreservesBytesAndRetryAppendsOnce()
    {
        var root = CreateTempRoot("training-local-promotion-durable-locked");
        var journalPath = Path.Combine(root, PromotionJournalFileName);
        const string tenant = "tenant:r7-b3-locked";
        const string idempotencyKey = "idempotency:r7-b3-locked-retry";
        const string privateMarker = "PRIVATE_R7_B3_LOCAL_PROMOTION_LOCKED_MARKER";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(db, "locked");
            var seed = await RunPromoteWithAdmissionAsync(
                db,
                intake.IntakeId,
                Path.Combine(root, "seed-output"),
                BuildPromotionOptions("r7-b3-locked-seed"),
                BuildAdmissionMetadata(tenant, "idempotency:r7-b3-locked-seed"));
            AssertBlockedPromotionSuccess(seed, intake.IntakeId, "r7-b3-locked-seed");
            var originalBytes = await File.ReadAllBytesAsync(journalPath);

            OperationResult<DesignConversionTrainingIntakeLocalPromotionResult> lockedResult;
            await using (var held = new FileStream(
                journalPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                lockedResult = await RunPromoteWithAdmissionAsync(
                    new JsonlDesignConversionLocalArtifactDb(root),
                    intake.IntakeId,
                    Path.Combine(root, "retry-output"),
                    BuildPromotionOptions($"r7-b3-locked-failed-{privateMarker}"),
                    BuildAdmissionMetadata(tenant, idempotencyKey));

                AssertWriteFailed(lockedResult);
                AssertNoTempArtifacts(root);
            }

            Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));

            var retry = await RunPromoteWithAdmissionAsync(
                new JsonlDesignConversionLocalArtifactDb(root),
                intake.IntakeId,
                Path.Combine(root, "retry-output"),
                BuildPromotionOptions("r7-b3-locked-retry"),
                BuildAdmissionMetadata(tenant, idempotencyKey));
            var retryPromotion = AssertBlockedPromotionSuccess(retry, intake.IntakeId, "r7-b3-locked-retry");
            var records = ReadPhysicalPromotionJournalRecords(journalPath);

            Assert.True(IsCreatedSideEffect(retryPromotion));
            Assert.Equal(2, records.Count);
            Assert.Equal(1, records.Count(record => record.TrainingRun.RunId == "r7-b3-locked-retry"));
            AssertNoTempArtifacts(root);

            var returnedErrorText = string.Join('|', lockedResult.ErrorCode, lockedResult.ErrorMessage);
            foreach (var forbidden in new[]
            {
                root,
                journalPath,
                PromotionJournalFileName,
                TempSuffix,
                "IOException",
                "UnauthorizedAccessException",
                tenant,
                idempotencyKey,
                privateMarker
            })
            {
                Assert.DoesNotContain(forbidden, returnedErrorText, StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAndPromoteLocalAsync_HeldMutationLease_CancelsWithoutJournalOrArtifactsAndRetryReplaysOnce()
    {
        var root = CreateTempRoot("training-local-promotion-durable-held-mutation-lease");
        var journalPath = Path.Combine(root, PromotionJournalFileName);
        var lockPath = Path.Combine(root, PromotionMutationLockFileName);
        const string tenant = "tenant:r7-b3-held-promotion-lease";
        const string idempotencyKey = "idempotency:r7-b3-held-promotion-lease-retry";

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(setupDb, "held-promotion-lease");
            var seed = await RunPromoteWithAdmissionAsync(
                setupDb,
                intake.IntakeId,
                Path.Combine(root, "seed-output"),
                BuildPromotionOptions("r7-b3-held-promotion-lease-seed"),
                BuildAdmissionMetadata(tenant, "idempotency:r7-b3-held-promotion-lease-seed"));
            AssertBlockedPromotionSuccess(seed, intake.IntakeId, "r7-b3-held-promotion-lease-seed");
            var originalBytes = await File.ReadAllBytesAsync(journalPath);
            var blockedOutputDirectory = Path.Combine(root, "held-lease-output");
            var options = BuildPromotionOptions("r7-b3-held-promotion-lease-retry");
            var admission = BuildAdmissionMetadata(tenant, idempotencyKey);

            OperationResult<DesignConversionTrainingIntakeLocalPromotionResult> cancelled;
            Directory.CreateDirectory(root);
            await using (var held = new FileStream(
                lockPath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize: 1,
                FileOptions.Asynchronous))
            {
                AssertPromotionMutationLockFileEmpty(lockPath);
                AssertDeterministicMutationLockHeld(lockPath);
                using var cts = new CancellationTokenSource();
                var pending = RunPromoteWithAdmissionAsync(
                    new JsonlDesignConversionLocalArtifactDb(root),
                    intake.IntakeId,
                    blockedOutputDirectory,
                    options,
                    admission,
                    cts.Token);

                await AssertTaskStillPendingAsync(
                    pending,
                    "Promote-local must remain pending while an external FileShare.None lease holds the deterministic mutation lock.");
                cts.Cancel();
                cancelled = await pending.WaitAsync(TimeSpan.FromSeconds(10));

                AssertPromotionMutationLockFileEmpty(lockPath);
                Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
                Assert.Single(ReadPhysicalPromotionJournalRecords(journalPath));
                Assert.False(
                    Directory.Exists(blockedOutputDirectory),
                    "A cancelled promote-local attempt blocked on the mutation lease must not create promotion/model artifact state.");
                AssertNoTempArtifacts(root);
            }

            Assert.False(cancelled.IsSuccess);
            Assert.Equal("CANCELLED", cancelled.ErrorCode);
            AssertPromotionMutationLockFileEmpty(lockPath);

            var retry = await RunPromoteWithAdmissionAsync(
                new JsonlDesignConversionLocalArtifactDb(root),
                intake.IntakeId,
                blockedOutputDirectory,
                options,
                admission);
            var retryPromotion = AssertBlockedPromotionSuccess(retry, intake.IntakeId, options.RunId);
            Assert.True(IsCreatedSideEffect(retryPromotion));
            var afterRetryBytes = await File.ReadAllBytesAsync(journalPath);
            var retryRecords = ReadPhysicalPromotionJournalRecords(journalPath);
            Assert.Equal(2, retryRecords.Count);
            Assert.Equal(1, retryRecords.Count(record => record.TrainingRun.RunId == options.RunId));
            AssertPromotionMutationLockFileEmpty(lockPath);

            var replay = await RunPromoteWithAdmissionAsync(
                new JsonlDesignConversionLocalArtifactDb(root),
                intake.IntakeId,
                blockedOutputDirectory,
                options,
                admission);
            var replayPromotion = AssertBlockedPromotionSuccess(replay, intake.IntakeId, options.RunId);
            Assert.True(IsReplay(replayPromotion));
            Assert.Equal(afterRetryBytes, await File.ReadAllBytesAsync(journalPath));
            Assert.Equal(2, ReadPhysicalPromotionJournalRecords(journalPath).Count);
            AssertPromotionMutationLockFileEmpty(lockPath);
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAndPromoteLocalAsync_ConcurrentSameKeyAcrossInstances_AppendsOneRowAndReturnsOneReplay()
    {
        var root = CreateTempRoot("training-local-promotion-durable-same-key-gate");
        var journalPath = Path.Combine(root, PromotionJournalFileName);

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(setupDb, "same-key-gate");
            var options = BuildPromotionOptions("r7-b3-same-key-gate-run");
            var admission = BuildAdmissionMetadata(
                "tenant:r7-b3-same-key-gate",
                "idempotency:r7-b3-same-key-gate");
            var promotionOutputDirectory = Path.Combine(root, "same-key-output");
            var firstGateAcquired = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseFirstGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var secondWaitObserved = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var firstDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeGateWaitTaskObserverForTesting: null,
                runTrainingIntakeGateAcquiredCallbackForTesting: null,
                promotionUploader: null,
                runTrainingIntakeLocalPromotionGateWaitTaskObserverForTesting: null,
                runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting: async ct =>
                {
                    firstGateAcquired.TrySetResult(true);
                    await releaseFirstGate.Task.WaitAsync(ct);
                });
            var secondDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeGateWaitTaskObserverForTesting: null,
                runTrainingIntakeGateAcquiredCallbackForTesting: null,
                promotionUploader: null,
                runTrainingIntakeLocalPromotionGateWaitTaskObserverForTesting: waitCompletedSuccessfully =>
                {
                    secondWaitObserved.TrySetResult(waitCompletedSuccessfully);
                },
                runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting: null);
            var firstTask = RunPromoteWithAdmissionAsync(
                firstDb,
                intake.IntakeId,
                promotionOutputDirectory,
                options,
                admission);

            var results = await ObserveSecondPromotionWaitsForSharedGateAndDrainAsync(
                firstTask,
                firstGateAcquired,
                releaseFirstGate,
                secondWaitObserved,
                () => RunPromoteWithAdmissionAsync(
                    secondDb,
                    intake.IntakeId,
                    promotionOutputDirectory,
                    options,
                    admission),
                "The second DB instance must observe a pending shared promotion-gate wait while the first instance holds the gate.");

            Assert.All(results, result => AssertBlockedPromotionSuccess(result, intake.IntakeId, options.RunId));
            Assert.Equal(1, results.Count(result => IsCreatedSideEffect(result.Value!)));
            Assert.Equal(1, results.Count(result => IsReplay(result.Value!)));
            var record = Assert.Single(ReadPhysicalPromotionJournalRecords(journalPath));
            Assert.Equal(options.RunId, record.TrainingRun.RunId);
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAndPromoteLocalAsync_DistinctTenantsAndKeysAcrossInstances_WaitOnRootSharedGateAndAppendTwoRows()
    {
        var root = CreateTempRoot("training-local-promotion-durable-distinct-admissions-gate");
        var journalPath = Path.Combine(root, PromotionJournalFileName);

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(setupDb, "distinct-admissions-gate");
            var firstOptions = BuildPromotionOptions("r7-b3-distinct-admissions-first-run");
            var secondOptions = BuildPromotionOptions("r7-b3-distinct-admissions-second-run");
            var firstAdmission = BuildAdmissionMetadata(
                "tenant:r7-b3-distinct-admissions-first",
                "idempotency:r7-b3-distinct-admissions-first");
            var secondAdmission = BuildAdmissionMetadata(
                "tenant:r7-b3-distinct-admissions-second",
                "idempotency:r7-b3-distinct-admissions-second");
            var firstGateAcquired = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseFirstGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var secondWaitObserved = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var firstDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeGateWaitTaskObserverForTesting: null,
                runTrainingIntakeGateAcquiredCallbackForTesting: null,
                promotionUploader: null,
                runTrainingIntakeLocalPromotionGateWaitTaskObserverForTesting: null,
                runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting: async ct =>
                {
                    firstGateAcquired.TrySetResult(true);
                    await releaseFirstGate.Task.WaitAsync(ct);
                });
            var secondDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeGateWaitTaskObserverForTesting: null,
                runTrainingIntakeGateAcquiredCallbackForTesting: null,
                promotionUploader: null,
                runTrainingIntakeLocalPromotionGateWaitTaskObserverForTesting: waitCompletedSuccessfully =>
                {
                    secondWaitObserved.TrySetResult(waitCompletedSuccessfully);
                },
                runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting: null);
            var firstTask = RunPromoteWithAdmissionAsync(
                firstDb,
                intake.IntakeId,
                Path.Combine(root, "distinct-admissions-first-output"),
                firstOptions,
                firstAdmission);

            var results = await ObserveSecondPromotionWaitsForSharedGateAndDrainAsync(
                firstTask,
                firstGateAcquired,
                releaseFirstGate,
                secondWaitObserved,
                () => RunPromoteWithAdmissionAsync(
                    secondDb,
                    intake.IntakeId,
                    Path.Combine(root, "distinct-admissions-second-output"),
                    secondOptions,
                    secondAdmission),
                "A different tenant and idempotency key must still wait while another DB instance holds the root promotion gate.");

            var firstPromotion = AssertBlockedPromotionSuccess(results[0], intake.IntakeId, firstOptions.RunId);
            var secondPromotion = AssertBlockedPromotionSuccess(results[1], intake.IntakeId, secondOptions.RunId);
            Assert.True(IsCreatedSideEffect(firstPromotion));
            Assert.True(IsCreatedSideEffect(secondPromotion));
            var records = ReadPhysicalPromotionJournalRecords(journalPath);
            Assert.Equal(2, records.Count);
            Assert.Equal(
                [
                    "idempotency:r7-b3-distinct-admissions-first",
                    "idempotency:r7-b3-distinct-admissions-second"
                ],
                records
                    .Select(record => ReadMetadata(record, "training_intake_idempotency_key_ref"))
                    .ToArray());
            Assert.All(records, record => Assert.False(record.LocalPromotionReady));
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAndPromoteLocalAsync_AcquiredCallbackFailure_ReleasesSharedPromotionGate()
    {
        var root = CreateTempRoot("training-local-promotion-durable-acquired-callback-failure");
        var journalPath = Path.Combine(root, PromotionJournalFileName);

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(setupDb, "acquired-callback-failure");
            var callbackFailure = new InvalidOperationException(
                "R7-B3 unique promotion gate-acquired callback failure.");
            var throwingDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeGateWaitTaskObserverForTesting: null,
                runTrainingIntakeGateAcquiredCallbackForTesting: null,
                promotionUploader: null,
                runTrainingIntakeLocalPromotionGateWaitTaskObserverForTesting: null,
                runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting: _ => throw callbackFailure);

            var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                RunPromoteWithAdmissionAsync(
                    throwingDb,
                    intake.IntakeId,
                    Path.Combine(root, "acquired-callback-failure-first-output"),
                    BuildPromotionOptions("r7-b3-acquired-callback-failure-first"),
                    BuildAdmissionMetadata(
                        "tenant:r7-b3-acquired-callback-failure",
                        "idempotency:r7-b3-acquired-callback-failure-first")));

            Assert.Same(callbackFailure, thrown);
            Assert.Equal("R7-B3 unique promotion gate-acquired callback failure.", thrown.Message);
            Assert.False(File.Exists(journalPath));
            AssertNoTempArtifacts(root);

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var retry = await RunPromoteWithAdmissionAsync(
                new JsonlDesignConversionLocalArtifactDb(root),
                intake.IntakeId,
                Path.Combine(root, "acquired-callback-failure-retry-output"),
                BuildPromotionOptions("r7-b3-acquired-callback-failure-retry"),
                BuildAdmissionMetadata(
                    "tenant:r7-b3-acquired-callback-failure-retry",
                    "idempotency:r7-b3-acquired-callback-failure-retry"),
                timeout.Token);

            var retryPromotion = AssertBlockedPromotionSuccess(
                retry,
                intake.IntakeId,
                "r7-b3-acquired-callback-failure-retry");
            Assert.True(IsCreatedSideEffect(retryPromotion));
            var record = Assert.Single(ReadPhysicalPromotionJournalRecords(journalPath));
            Assert.Equal(
                "idempotency:r7-b3-acquired-callback-failure-retry",
                ReadMetadata(record, "training_intake_idempotency_key_ref"));
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAndPromoteLocalAsync_ImmediateWaitObserverFailure_ReleasesSharedPromotionGate()
    {
        var root = CreateTempRoot("training-local-promotion-durable-observer-failure");
        var journalPath = Path.Combine(root, PromotionJournalFileName);

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(setupDb, "observer-failure");
            var observerFailure = new InvalidOperationException(
                "R7-B3 unique immediate promotion wait observer failure.");
            var immediateAcquisitionObserved = false;
            var throwingDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeGateWaitTaskObserverForTesting: null,
                runTrainingIntakeGateAcquiredCallbackForTesting: null,
                promotionUploader: null,
                runTrainingIntakeLocalPromotionGateWaitTaskObserverForTesting: waitCompletedSuccessfully =>
                {
                    immediateAcquisitionObserved = waitCompletedSuccessfully;
                    Assert.True(
                        waitCompletedSuccessfully,
                        "The first promotion must acquire the uncontended shared promotion gate synchronously.");
                    throw observerFailure;
                },
                runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting: null);

            var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                RunPromoteWithAdmissionAsync(
                    throwingDb,
                    intake.IntakeId,
                    Path.Combine(root, "observer-failure-first-output"),
                    BuildPromotionOptions("r7-b3-observer-failure-first"),
                    BuildAdmissionMetadata(
                        "tenant:r7-b3-observer-failure",
                        "idempotency:r7-b3-observer-failure-first")));

            Assert.Same(observerFailure, thrown);
            Assert.True(immediateAcquisitionObserved);
            Assert.False(File.Exists(journalPath));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var second = await RunPromoteWithAdmissionAsync(
                new JsonlDesignConversionLocalArtifactDb(root),
                intake.IntakeId,
                Path.Combine(root, "observer-failure-retry-output"),
                BuildPromotionOptions("r7-b3-observer-failure-retry"),
                BuildAdmissionMetadata(
                    "tenant:r7-b3-observer-failure",
                    "idempotency:r7-b3-observer-failure-retry"),
                timeout.Token);

            AssertBlockedPromotionSuccess(second, intake.IntakeId, "r7-b3-observer-failure-retry");
            var record = Assert.Single(ReadPhysicalPromotionJournalRecords(journalPath));
            Assert.Equal("r7-b3-observer-failure-retry", record.TrainingRun.RunId);
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAndPromoteLocalAsync_PreCancelledToken_PreservesExistingPromotionJournalBytesAndLeavesNoTemp()
    {
        var root = CreateTempRoot("training-local-promotion-durable-cancelled");
        var journalPath = Path.Combine(root, PromotionJournalFileName);

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(db, "cancelled");
            var seed = await RunPromoteWithAdmissionAsync(
                db,
                intake.IntakeId,
                Path.Combine(root, "cancelled-seed-output"),
                BuildPromotionOptions("r7-b3-cancelled-seed"),
                BuildAdmissionMetadata(
                    "tenant:r7-b3-cancelled",
                    "idempotency:r7-b3-cancelled-seed"));
            AssertBlockedPromotionSuccess(seed, intake.IntakeId, "r7-b3-cancelled-seed");
            var originalBytes = await File.ReadAllBytesAsync(journalPath);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var cancelled = await RunPromoteWithAdmissionAsync(
                new JsonlDesignConversionLocalArtifactDb(root),
                intake.IntakeId,
                Path.Combine(root, "cancelled-second-output"),
                BuildPromotionOptions("r7-b3-cancelled-second"),
                BuildAdmissionMetadata(
                    "tenant:r7-b3-cancelled",
                    "idempotency:r7-b3-cancelled-second"),
                cts.Token);

            Assert.False(cancelled.IsSuccess);
            Assert.Equal("CANCELLED", cancelled.ErrorCode);
            Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
            Assert.Single(ReadPhysicalPromotionJournalRecords(journalPath));
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RollbackTrainingIntakeLocalPromotionAsync_FailsClosedWhenLocalPromotionJournalCannotReserveMaximumRow()
    {
        var root = CreateTempRoot("training-local-promotion-durable-rollback-quota");
        var journalPath = Path.Combine(root, PromotionJournalFileName);
        const string privatePathMarker = "C:/Users/private-user/private/rollback-quota-marker.txt";
        const string privateRequesterMarker = "PRIVATE_R7_B3_ROLLBACK_QUOTA_REQUESTER_MARKER";
        const string privateReasonMarker = "PRIVATE_R7_B3_ROLLBACK_QUOTA_REASON_MARKER";

        try
        {
            Directory.CreateDirectory(root);
            var first = BuildReadyPromotionRecord("rollback-quota-first", priorRef: string.Empty);
            var firstRef = ReadMetadata(first, "local_promotion_active_local_model_ref");
            var second = BuildReadyPromotionRecord("rollback-quota-second", firstRef);
            var secondRef = ReadMetadata(second, "local_promotion_active_local_model_ref");
            await WritePromotionJournalRecordsAsync(journalPath, first, second);

            var originalBytes = await File.ReadAllBytesAsync(journalPath);
            var originalRecords = ReadPhysicalPromotionJournalRecords(journalPath);
            Assert.Equal(2, originalRecords.Count);
            var originalActiveRef = ResolvePhysicalActiveRef(originalRecords);
            Assert.Equal(secondRef, originalActiveRef);
            AssertNoTempArtifacts(root);

            var lowQuota = originalBytes.LongLength
                + JsonlDesignConversionLocalArtifactDb.MaxTrainingIntakeLocalPromotionJournalRowByteLength
                - 1;
            var quotaDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                promotionUploader: null,
                recordsAuthorityMaxBytes: JsonlDesignConversionLocalArtifactDb.DefaultRecordsAuthorityMaxBytes,
                trainingIntakeRequestsMaxBytes: JsonlDesignConversionLocalArtifactDb.DefaultTrainingIntakeRequestsMaxBytes,
                trainingIntakeRunsMaxBytes: JsonlDesignConversionLocalArtifactDb.DefaultTrainingIntakeRunsMaxBytes,
                trainingIntakeLocalPromotionsMaxBytes: lowQuota);

            var quota = await quotaDb.RollbackTrainingIntakeLocalPromotionAsync(
                secondRef,
                $"requester:r7-b3-rollback-quota:{privateRequesterMarker}",
                $"rollback quota guard must not leak {privateReasonMarker} {privatePathMarker}",
                DateTimeOffset.UnixEpoch.AddMinutes(430));

            Assert.False(quota.IsSuccess);
            Assert.Equal(
                JsonlDesignConversionLocalArtifactDb.TrainingIntakeLocalPromotionsQuotaExceededErrorCode,
                quota.ErrorCode);
            Assert.Equal(
                JsonlDesignConversionLocalArtifactDb.TrainingIntakeLocalPromotionsQuotaExceededSafeMessage,
                quota.ErrorMessage);
            var quotaErrorEnvelope = JsonSerializer.Serialize(new { quota.ErrorCode, quota.ErrorMessage });
            var quotaErrorText = string.Join('|', quota.ErrorCode, quota.ErrorMessage, quotaErrorEnvelope);
            foreach (var forbidden in new[]
            {
                root,
                journalPath,
                PromotionJournalFileName,
                TempSuffix,
                firstRef,
                secondRef,
                privatePathMarker,
                privateRequesterMarker,
                privateReasonMarker,
                "IOException",
                "UnauthorizedAccessException",
                "Exception"
            })
            {
                Assert.DoesNotContain(forbidden, quotaErrorText, StringComparison.OrdinalIgnoreCase);
            }

            Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
            var failedRecords = ReadPhysicalPromotionJournalRecords(journalPath);
            Assert.Equal(originalRecords.Count, failedRecords.Count);
            Assert.Equal(originalActiveRef, ResolvePhysicalActiveRef(failedRecords));
            Assert.DoesNotContain(
                failedRecords,
                record => string.Equals(
                    ReadMetadata(record, "local_promotion_record_kind"),
                    "rollback",
                    StringComparison.Ordinal));
            AssertNoTempArtifacts(root);

            var capacityAtReservation = originalBytes.LongLength
                + JsonlDesignConversionLocalArtifactDb.MaxTrainingIntakeLocalPromotionJournalRowByteLength;
            var appendDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                promotionUploader: null,
                recordsAuthorityMaxBytes: JsonlDesignConversionLocalArtifactDb.DefaultRecordsAuthorityMaxBytes,
                trainingIntakeRequestsMaxBytes: JsonlDesignConversionLocalArtifactDb.DefaultTrainingIntakeRequestsMaxBytes,
                trainingIntakeRunsMaxBytes: JsonlDesignConversionLocalArtifactDb.DefaultTrainingIntakeRunsMaxBytes,
                trainingIntakeLocalPromotionsMaxBytes: capacityAtReservation);
            var requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(431);
            var retry = await appendDb.RollbackTrainingIntakeLocalPromotionAsync(
                secondRef,
                "requester:r7-b3-rollback-quota-retry",
                "rollback after one maximum row capacity is available",
                requestedAtUtc);
            var records = ReadPhysicalPromotionJournalRecords(journalPath);

            Assert.True(retry.IsSuccess, retry.ErrorMessage);
            Assert.True(retry.Value!.RollbackApplied);
            Assert.Equal(secondRef, retry.Value.RolledBackLocalModelRef);
            Assert.Equal(firstRef, retry.Value.RestoredActiveLocalModelRef);
            Assert.Equal(3, records.Count);
            var rollbackRecord = Assert.Single(records.Where(record =>
                string.Equals(
                    ReadMetadata(record, "local_promotion_record_kind"),
                    "rollback",
                    StringComparison.Ordinal)));
            Assert.Same(records[2], rollbackRecord);
            Assert.Equal(
                requestedAtUtc.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
                ReadMetadata(rollbackRecord, "local_promotion_rollback_requested_at_utc"));
            Assert.Equal(firstRef, ResolvePhysicalActiveRef(records));
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RollbackTrainingIntakeLocalPromotionAsync_LockedJournal_FailsSafelyPreservesBytesAndRetryAppendsOnce()
    {
        var root = CreateTempRoot("training-local-promotion-durable-rollback-locked");
        var journalPath = Path.Combine(root, PromotionJournalFileName);
        const string privateMarker = "PRIVATE_R7_B3_ROLLBACK_LOCKED_MARKER";

        try
        {
            Directory.CreateDirectory(root);
            var first = BuildReadyPromotionRecord("rollback-locked-first", priorRef: string.Empty);
            var firstRef = ReadMetadata(first, "local_promotion_active_local_model_ref");
            var second = BuildReadyPromotionRecord("rollback-locked-second", firstRef);
            var secondRef = ReadMetadata(second, "local_promotion_active_local_model_ref");
            await WritePromotionJournalRecordsAsync(journalPath, first, second);
            var originalBytes = await File.ReadAllBytesAsync(journalPath);

            OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult> lockedResult;
            await using (var held = new FileStream(
                journalPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                lockedResult = await new JsonlDesignConversionLocalArtifactDb(root)
                    .RollbackTrainingIntakeLocalPromotionAsync(
                        secondRef,
                        "requester:r7-b3-rollback-locked",
                        $"rollback while journal is locked {privateMarker}",
                        DateTimeOffset.UnixEpoch.AddMinutes(400));

                AssertWriteFailed(lockedResult);
                AssertNoTempArtifacts(root);
            }

            Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));

            var retry = await new JsonlDesignConversionLocalArtifactDb(root)
                .RollbackTrainingIntakeLocalPromotionAsync(
                    secondRef,
                    "requester:r7-b3-rollback-locked",
                    "rollback after journal unlock",
                    DateTimeOffset.UnixEpoch.AddMinutes(401));
            var records = ReadPhysicalPromotionJournalRecords(journalPath);

            Assert.True(retry.IsSuccess, retry.ErrorMessage);
            Assert.True(retry.Value!.RollbackApplied);
            Assert.Equal(firstRef, retry.Value.RestoredActiveLocalModelRef);
            Assert.Equal(3, records.Count);
            Assert.Equal(1, records.Count(record =>
                string.Equals(ReadMetadata(record, "local_promotion_record_kind"), "rollback", StringComparison.Ordinal)));
            Assert.Equal(firstRef, ResolvePhysicalActiveRef(records));
            AssertNoTempArtifacts(root);

            var returnedErrorText = string.Join('|', lockedResult.ErrorCode, lockedResult.ErrorMessage);
            foreach (var forbidden in new[]
            {
                root,
                journalPath,
                PromotionJournalFileName,
                TempSuffix,
                "IOException",
                "UnauthorizedAccessException",
                secondRef,
                privateMarker
            })
            {
                Assert.DoesNotContain(forbidden, returnedErrorText, StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RollbackTrainingIntakeLocalPromotionAsync_HeldMutationLease_CancelsWithoutJournalMutationAndRetryAppendsOnce()
    {
        var root = CreateTempRoot("training-local-promotion-durable-rollback-held-mutation-lease");
        var journalPath = Path.Combine(root, PromotionJournalFileName);
        var lockPath = Path.Combine(root, PromotionMutationLockFileName);

        try
        {
            Directory.CreateDirectory(root);
            var first = BuildReadyPromotionRecord("rollback-held-lease-first", priorRef: string.Empty);
            var firstRef = ReadMetadata(first, "local_promotion_active_local_model_ref");
            var second = BuildReadyPromotionRecord("rollback-held-lease-second", firstRef);
            var secondRef = ReadMetadata(second, "local_promotion_active_local_model_ref");
            await WritePromotionJournalRecordsAsync(journalPath, first, second);
            var originalBytes = await File.ReadAllBytesAsync(journalPath);

            OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult> cancelled;
            await using (var held = new FileStream(
                lockPath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize: 1,
                FileOptions.Asynchronous))
            {
                AssertPromotionMutationLockFileEmpty(lockPath);
                AssertDeterministicMutationLockHeld(lockPath);
                using var cts = new CancellationTokenSource();
                var pending = new JsonlDesignConversionLocalArtifactDb(root)
                    .RollbackTrainingIntakeLocalPromotionAsync(
                        secondRef,
                        "requester:r7-b3-rollback-held-lease",
                        "rollback must not pass the externally held local promotion mutation lease",
                        DateTimeOffset.UnixEpoch.AddMinutes(420),
                        cts.Token);

                await AssertTaskStillPendingAsync(
                    pending,
                    "Rollback must remain pending while an external FileShare.None lease holds the deterministic mutation lock.");
                cts.Cancel();
                cancelled = await pending.WaitAsync(TimeSpan.FromSeconds(10));

                AssertPromotionMutationLockFileEmpty(lockPath);
                Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
                var cancelledRecords = ReadPhysicalPromotionJournalRecords(journalPath);
                Assert.Equal(2, cancelledRecords.Count);
                Assert.Equal(secondRef, ResolvePhysicalActiveRef(cancelledRecords));
                AssertNoTempArtifacts(root);
            }

            Assert.False(cancelled.IsSuccess);
            Assert.Equal("CANCELLED", cancelled.ErrorCode);
            AssertPromotionMutationLockFileEmpty(lockPath);

            var retry = await new JsonlDesignConversionLocalArtifactDb(root)
                .RollbackTrainingIntakeLocalPromotionAsync(
                    secondRef,
                    "requester:r7-b3-rollback-held-lease",
                    "rollback after local promotion mutation lease release",
                    DateTimeOffset.UnixEpoch.AddMinutes(421));
            var records = ReadPhysicalPromotionJournalRecords(journalPath);

            Assert.True(retry.IsSuccess, retry.ErrorMessage);
            Assert.True(retry.Value!.RollbackApplied);
            Assert.Equal(firstRef, retry.Value.RestoredActiveLocalModelRef);
            Assert.Equal(3, records.Count);
            Assert.Equal(1, records.Count(record =>
                string.Equals(ReadMetadata(record, "local_promotion_record_kind"), "rollback", StringComparison.Ordinal)));
            Assert.Equal(firstRef, ResolvePhysicalActiveRef(records));
            AssertPromotionMutationLockFileEmpty(lockPath);
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RollbackTrainingIntakeLocalPromotionAsync_WaitsForInFlightBlockedPromotionThenRestoresPriorActiveState()
    {
        var root = CreateTempRoot("training-local-promotion-durable-rollback-ordering");
        var journalPath = Path.Combine(root, PromotionJournalFileName);

        try
        {
            Directory.CreateDirectory(root);
            var first = BuildReadyPromotionRecord("rollback-ordering-first", priorRef: string.Empty);
            var firstRef = ReadMetadata(first, "local_promotion_active_local_model_ref");
            var second = BuildReadyPromotionRecord("rollback-ordering-second", firstRef);
            var secondRef = ReadMetadata(second, "local_promotion_active_local_model_ref");
            await WritePromotionJournalRecordsAsync(journalPath, first, second);
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(setupDb, "rollback-ordering-third");
            var thirdGateAcquired = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseThirdGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var rollbackWaitObserved = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var thirdDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeGateWaitTaskObserverForTesting: null,
                runTrainingIntakeGateAcquiredCallbackForTesting: null,
                promotionUploader: null,
                runTrainingIntakeLocalPromotionGateWaitTaskObserverForTesting: null,
                runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting: async ct =>
                {
                    thirdGateAcquired.TrySetResult(true);
                    await releaseThirdGate.Task.WaitAsync(ct);
                });
            var rollbackDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeGateWaitTaskObserverForTesting: null,
                runTrainingIntakeGateAcquiredCallbackForTesting: null,
                promotionUploader: null,
                runTrainingIntakeLocalPromotionGateWaitTaskObserverForTesting: waitCompletedSuccessfully =>
                {
                    rollbackWaitObserved.TrySetResult(waitCompletedSuccessfully);
                },
                runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting: null);
            var thirdTask = RunPromoteWithAdmissionAsync(
                thirdDb,
                intake.IntakeId,
                Path.Combine(root, "third-output"),
                BuildPromotionOptions("r7-b3-rollback-ordering-third"),
                BuildAdmissionMetadata(
                    "tenant:r7-b3-rollback-ordering",
                    "idempotency:r7-b3-rollback-ordering-third"));

            Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>>? rollbackTask = null;
            OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>? third = null;
            OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>? rollback = null;
            Exception? coordinationFailure = null;
            try
            {
                await thirdGateAcquired.Task.WaitAsync(TimeSpan.FromSeconds(10));
                rollbackTask = rollbackDb.RollbackTrainingIntakeLocalPromotionAsync(
                    secondRef,
                    "requester:r7-b3-rollback-ordering",
                    "rollback must wait for the in-flight blocked promotion before restoring prior active state",
                    DateTimeOffset.UnixEpoch.AddMinutes(410));
                var rollbackWaitCompletedSuccessfully = await rollbackWaitObserved.Task.WaitAsync(TimeSpan.FromSeconds(10));
                Assert.False(
                    rollbackWaitCompletedSuccessfully,
                    "Rollback on another DB instance must observe a pending wait on the shared promotion gate.");
            }
            catch (Exception ex)
            {
                coordinationFailure = ex;
            }
            finally
            {
                releaseThirdGate.TrySetResult(true);
                try
                {
                    if (rollbackTask is null)
                    {
                        third = await thirdTask;
                    }
                    else
                    {
                        await Task.WhenAll((Task)thirdTask, rollbackTask);
                        third = await thirdTask;
                        rollback = await rollbackTask;
                    }
                }
                catch (Exception drainFailure)
                {
                    coordinationFailure = coordinationFailure is null
                        ? drainFailure
                        : new AggregateException(coordinationFailure, drainFailure);
                }
            }

            if (coordinationFailure is not null)
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(coordinationFailure).Throw();

            Assert.NotNull(third);
            Assert.NotNull(rollback);
            var thirdPromotion = AssertBlockedPromotionSuccess(
                third!,
                intake.IntakeId,
                "r7-b3-rollback-ordering-third");
            Assert.True(IsCreatedSideEffect(thirdPromotion));
            Assert.True(rollback!.IsSuccess, rollback.ErrorMessage);
            Assert.True(rollback.Value!.RollbackApplied);
            Assert.True(rollback.Value.PriorStateRestored);
            Assert.Equal(secondRef, rollback.Value.RolledBackLocalModelRef);
            Assert.Equal(firstRef, rollback.Value.RestoredActiveLocalModelRef);
            var records = ReadPhysicalPromotionJournalRecords(journalPath);
            Assert.Equal(4, records.Count);
            Assert.True(records[0].LocalPromotionReady);
            Assert.True(records[1].LocalPromotionReady);
            Assert.Equal(
                "idempotency:r7-b3-rollback-ordering-third",
                ReadMetadata(records[2], "training_intake_idempotency_key_ref"));
            Assert.False(records[2].LocalPromotionReady);
            Assert.Equal("rollback", ReadMetadata(records[3], "local_promotion_record_kind"));
            Assert.Equal(firstRef, ResolvePhysicalActiveRef(records));
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    private static async Task<DesignConversionTrainingIntakeRecord> CreateBlockedIntakeAsync(
        JsonlDesignConversionLocalArtifactDb db,
        string scope)
    {
        var created = await db.CreateTrainingIntakeAsync(
            BuildAcceptedMissingSourceProofHandoff(scope),
            "requester:r7-b3-training-local-promotion-durability",
            $"blocked local promotion durability intake {scope}",
            DateTimeOffset.UnixEpoch.AddMinutes(300),
            admissionBoundaryMetadata: BuildAdmissionMetadata(
                $"tenant:r7-b3-create-{scope}",
                $"idempotency:r7-b3-create-{scope}"));
        Assert.True(created.IsSuccess, created.ErrorMessage);

        var preflight = await db.RunTrainingIntakePreflightAsync(created.Value!.IntakeId);
        Assert.True(preflight.IsSuccess, preflight.ErrorMessage);
        Assert.False(preflight.Value!.MayStartTraining);
        Assert.Equal("missing", preflight.Value.SourceManifestStatus);
        Assert.Equal("missing", preflight.Value.SourceDisjointStatus);

        return created.Value;
    }

    private static Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> RunPromoteWithAdmissionAsync(
        JsonlDesignConversionLocalArtifactDb db,
        string intakeId,
        string promotionOutputDirectory,
        DesignConversionTrainingPipelineOptions options,
        IReadOnlyDictionary<string, string> admission,
        CancellationToken ct = default)
        => DesignConversionTrainingIntakeRouteAdmissionContext.RunAsync(
            admission,
            () => db.RunTrainingIntakeAndPromoteLocalAsync(
                intakeId,
                promotionOutputDirectory,
                options,
                ct));

    private static async Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>[]> ObserveSecondPromotionWaitsForSharedGateAndDrainAsync(
        Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> firstTask,
        TaskCompletionSource<bool> firstGateAcquired,
        TaskCompletionSource<bool> releaseFirstGate,
        TaskCompletionSource<bool> secondWaitObserved,
        Func<Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>>> startSecond,
        string assertionMessage)
    {
        Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>>? secondTask = null;
        OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>[]? results = null;
        Exception? coordinationFailure = null;

        try
        {
            await firstGateAcquired.Task.WaitAsync(TimeSpan.FromSeconds(10));
            secondTask = startSecond();
            var secondWaitCompletedSuccessfully = await secondWaitObserved.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.False(secondWaitCompletedSuccessfully, assertionMessage);
        }
        catch (Exception ex)
        {
            coordinationFailure = ex;
        }
        finally
        {
            releaseFirstGate.TrySetResult(true);
            try
            {
                results = secondTask is null
                    ? [await firstTask]
                    : await Task.WhenAll(firstTask, secondTask);
            }
            catch (Exception drainFailure)
            {
                coordinationFailure = coordinationFailure is null
                    ? drainFailure
                    : new AggregateException(coordinationFailure, drainFailure);
            }
        }

        if (coordinationFailure is not null)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(coordinationFailure).Throw();

        return results!;
    }

    private static DesignConversionTrainingIntakeLocalPromotionResult AssertBlockedPromotionSuccess(
        OperationResult<DesignConversionTrainingIntakeLocalPromotionResult> result,
        string intakeId,
        string runId)
    {
        Assert.True(result.IsSuccess, result.ErrorMessage);
        var promotion = result.Value!;
        Assert.Equal(intakeId, promotion.TrainingRun.TrainingIntakeId);
        Assert.Equal(runId, promotion.TrainingRun.RunId);
        Assert.False(promotion.TrainingRun.MayStartTraining);
        Assert.Equal("missing", promotion.TrainingRun.SourceManifestStatus);
        Assert.Equal("missing", promotion.TrainingRun.SourceDisjointStatus);
        Assert.False(promotion.TrainingRun.TrainingRunnerInvoked);
        Assert.False(promotion.TrainingRun.ModelUploadReady);
        Assert.False(promotion.LocalPromotionReady);
        Assert.False(promotion.ExternalCloudUploadAttempted);
        Assert.Null(promotion.Publication);
        Assert.Null(promotion.PromotedLocalModel);
        Assert.Contains("local_promotion_blocked_training_intake_preflight", promotion.BlockReasons);
        return promotion;
    }

    private static IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult> ReadPhysicalPromotionJournalRecords(
        string journalPath)
    {
        var bytes = File.ReadAllBytes(journalPath);
        var utf8Preamble = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: true,
            throwOnInvalidBytes: true).GetPreamble();
        Assert.False(bytes.AsSpan().StartsWith(utf8Preamble), "Promotion journal must not have a UTF-8 BOM.");

        var text = StrictUtf8.GetString(bytes);
        Assert.DoesNotContain("\uFFFD", text, StringComparison.Ordinal);
        Assert.True(
            text.EndsWith(Environment.NewLine, StringComparison.Ordinal),
            "Promotion journal must end with exactly one platform newline.");

        var segments = text.Split(Environment.NewLine, StringSplitOptions.None);
        Assert.True(segments.Length >= 2, "Promotion journal must contain at least one framed row.");
        Assert.Empty(segments[^1]);
        Assert.All(segments[..^1], segment => Assert.False(string.IsNullOrWhiteSpace(segment)));

        return segments[..^1]
            .Select(segment =>
            {
                var record = JsonSerializer.Deserialize<DesignConversionTrainingIntakeLocalPromotionResult>(
                    segment,
                    DesignConversionCheckpointJsonOptions.Standard);
                Assert.NotNull(record);
                return record;
            })
            .ToArray();
    }

    private static DesignConversionApprovedDpoHandoffArtifact BuildAcceptedMissingSourceProofHandoff(string scope)
    {
        var row = new DesignConversionApprovedDpoHandoffRow(
            LocalArtifactRecordId: $"local-record-{scope}",
            CandidateId: $"candidate-{scope}",
            TrainingRecordId: $"training-record-{scope}",
            UnitId: "critic",
            UsefulnessScore: 0.91,
            ApprovalStatus: "approved",
            ReviewStatus: JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewApprovedStatus,
            ApprovedForTraining: true,
            TrainingHandoffOnly: true,
            ApprovedByRef: "reviewer:r7-b3-training-local-promotion-durability",
            ApprovalReason: "approved handoff row for blocked local promotion durability test",
            ApprovedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(1).ToString("O"),
            SourceManifestRef: string.Empty,
            RecordHash: HashRef(scope, "record"),
            InternalUnitStateHash: HashRef(scope, "internal-unit-state"),
            SourceStructureHash: HashRef(scope, "source-structure"),
            ChosenPayloadHash: HashRef(scope, "chosen"),
            RejectedPayloadHash: HashRef(scope, "rejected"),
            TieBreakerHash: HashRef(scope, "tie-breaker"),
            ChosenHtmlRef: $"artifact://r7-b3/{scope}/chosen.html",
            ChosenCssRef: $"artifact://r7-b3/{scope}/chosen.css",
            RejectedHtmlRef: $"artifact://r7-b3/{scope}/rejected.html",
            RejectedCssRef: $"artifact://r7-b3/{scope}/rejected.css",
            DesignTreeRef: $"artifact://r7-b3/{scope}/design-tree.json",
            OriginalTreeRef: $"artifact://r7-b3/{scope}/original-tree.json",
            SourceArtifactRefs: [$"artifact://r7-b3/{scope}/source.png"],
            RenderArtifactRefs: [$"artifact://r7-b3/{scope}/render.png"],
            ComparisonArtifactRefs: [$"artifact://r7-b3/{scope}/comparison.png"],
            EvidenceRefs: ["r7-b3-training-local-promotion-durability"],
            BoundaryMetadata: BuildHandoffBoundaryMetadata(1));

        return new DesignConversionApprovedDpoHandoffArtifact(
            ArtifactId: $"approved-dpo-handoff-{scope}",
            ArtifactRef: $"memory://r7-b3/{scope}/approved-dpo-handoff",
            SchemaVersion: JsonlDesignConversionLocalArtifactDb.ApprovedDpoHandoffSchemaVersion,
            ExportedAtUtc: DateTimeOffset.UnixEpoch,
            ApprovedCandidateCount: 1,
            Rows: [row],
            Warnings: ["training_handoff_only", "not_trained"],
            BoundaryMetadata: BuildHandoffBoundaryMetadata(1));
    }

    private static IReadOnlyDictionary<string, string> BuildHandoffBoundaryMetadata(int rowCount)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["schema_version"] = JsonlDesignConversionLocalArtifactDb.ApprovedDpoHandoffSchemaVersion,
            ["approved_candidate_count"] = rowCount.ToString(),
            ["training_handoff_only"] = bool.TrueString,
            ["not_trained"] = bool.TrueString,
            ["weights_not_produced"] = bool.TrueString,
            ["model_upload_not_ready"] = bool.TrueString,
            ["product_not_ready"] = bool.TrueString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString
        };

    private static DesignConversionTrainingPipelineOptions BuildPromotionOptions(string runId)
        => new(
            RunId: runId,
            BatchSize: 1);

    private static IReadOnlyDictionary<string, string> BuildAdmissionMetadata(
        string tenant,
        string idempotencyKey)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] = tenant,
            ["training_intake_idempotency_key_ref"] = idempotencyKey
        };

    private static async Task WritePromotionJournalRecordsAsync(
        string journalPath,
        params DesignConversionTrainingIntakeLocalPromotionResult[] records)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(journalPath)!);
        var text = string.Concat(records.Select(record =>
            JsonSerializer.Serialize(record, CompactPromotionJournalJsonOptions) + Environment.NewLine));
        await File.WriteAllTextAsync(journalPath, text, StrictUtf8);
    }

    private static DesignConversionTrainingIntakeLocalPromotionResult BuildReadyPromotionRecord(
        string scope,
        string priorRef)
    {
        var activeRef = $"local://design-conversion/promoted-local-model/{scope}";
        var modelId = $"{scope}-model";
        var checkpointSha = HashRef(scope, "checkpoint");
        var manifestId = $"{scope}-manifest";
        var manifestRef = $"manifest://r7-b3/{scope}";
        var checkpointPath = $"artifact://r7-b3/{scope}/checkpoint.json";
        var model = new DesignConversionPromotedLocalModel(
            ModelId: modelId,
            ManifestId: manifestId,
            ManifestRef: manifestRef,
            CheckpointPath: checkpointPath,
            CheckpointSha256: checkpointSha,
            MachineId: $"{scope}-machine",
            DomainTag: "design-conversion",
            Version: "v1",
            LearnedWeightArtifactCount: 1,
            LearnedWeightObservationCount: 1,
            CheckpointFreshLoadVerified: true,
            CheckpointContainsRealTrainedWeights: true,
            PromotionManifestClaimed: true,
            LocalOnly: true,
            EvidenceRefs: [$"{scope}:local-model-evidence"]);
        var manifest = new DesignConversionTrainingManifest(
            ManifestId: manifestId,
            MachineId: $"{scope}-machine",
            DomainTag: "design-conversion",
            Version: "v1",
            CheckpointRef: checkpointPath,
            ManifestRef: manifestRef,
            CreatedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(350),
            AcceptedRecordCount: 1,
            QuarantinedRecordCount: 0,
            UnitIds: ["critic"],
            Batches:
            [
                new DesignConversionTrainingManifestBatch(
                    BatchId: $"{scope}-batch",
                    UnitId: "critic",
                    RecordCount: 1,
                    RecordIds: [$"{scope}-record"],
                    SourceIds: [$"{scope}-source"])
            ],
            EvidenceRefs: [$"{scope}:manifest-evidence"],
            ModelServerUploadAttempted: false,
            ModelServerPromotionClaimed: false,
            DeterministicLogicClaimedAsLearning: false)
        {
            PromotionUploadRef = activeRef,
            PromotionUploadedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(351),
            LocalPromotionReceiptWritten = true,
            PromotionReceiptKind = "local_receipt_only",
            PromotionArtifacts = new DesignConversionTrainingManifestArtifacts(
                checkpointPath,
                $"artifact://r7-b3/{scope}/tokenizer.json",
                $"artifact://r7-b3/{scope}/policy.json")
        };
        var publication = new DesignConversionPromotionPublicationResult(
            new DesignConversionPromotionDecision(
                DesignConversionPromotionDecisionKind.Promote,
                Reasons: [],
                EvidenceRefs: [$"{scope}:promotion-decision"]),
            manifest,
            new DesignConversionPromotionUploadReceipt(
                UploadId: $"{scope}-receipt",
                RemoteRef: activeRef,
                UploadedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(351),
                EvidenceRefs: [$"{scope}:receipt-evidence"])
            {
                PublicationKind = DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
                UploadAttempted = false,
                UploadSucceeded = false,
                VerificationSucceeded = false,
                PublicationStatus = DesignConversionPromotionPublicationStatus.NotUploaded
            })
        {
            PublicationKind = DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
            PublicationStatus = DesignConversionPromotionPublicationStatus.NotUploaded
        };
        var run = new DesignConversionTrainingIntakeRunResult(
            RunId: $"{scope}-run",
            TrainingIntakeId: $"{scope}-intake",
            PreflightId: $"{scope}-preflight",
            AcceptedCandidateCount: 1,
            ResolvedTrainingRecordCount: 1,
            MayStartTraining: true,
            SourceManifestStatus: "proven",
            SourceDisjointStatus: "proven",
            TrainingRunnerInvoked: true,
            LocalLifecycleHookAttempted: true,
            TrainingStarted: true,
            CheckpointMutated: true,
            CheckpointCandidateCreated: true,
            WeightsProduced: true,
            ModelUploadReady: true,
            TrainingExecutionBoundary: "seeded_ready_local_promotion_test_boundary",
            TrainingExecutionStatus: "completed",
            RunnerBlockedReason: string.Empty,
            CheckpointStatus: "created",
            CheckpointMutationStatus: "mutated",
            WeightProofStatus: "real_weights_proven",
            MeasurementGateStatus: DesignConversionMeasurementGateStatus.Passed,
            PromotionGateStatus: "PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS",
            UploadReadinessStatus: "upload_ready_local_promotion_gate_passed_no_external_upload",
            PromotionBlockedReasons: [],
            RunnerEvidence: null,
            Warnings: [],
            BlockReasons: [],
            BoundaryMetadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["product_ready"] = bool.FalseString
            });
        var priorModelId = string.IsNullOrWhiteSpace(priorRef)
            ? string.Empty
            : $"{PriorScopeFromRef(priorRef)}-model";
        var priorCheckpoint = string.IsNullOrWhiteSpace(priorRef)
            ? string.Empty
            : HashRef(PriorScopeFromRef(priorRef), "checkpoint");

        return new DesignConversionTrainingIntakeLocalPromotionResult(
            TrainingRun: run,
            LocalPromotionReady: true,
            ExternalCloudUploadAttempted: false,
            PromotionOutputDirectory: $"artifact://r7-b3/{scope}/output",
            PromotedCheckpointPath: checkpointPath,
            Publication: publication,
            PromotedLocalModel: model,
            EvidenceRefs:
            [
                "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_PROMOTION_READY",
                "DESIGN_CONVERSION_LOCAL_PROMOTION_ACTIVE_STATE_RECORDED"
            ],
            Warnings: ["local_promotion_ready"],
            BlockReasons: [],
            BoundaryMetadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["schema_version"] = JsonlDesignConversionLocalArtifactDb.TrainingIntakeLocalPromotionSchemaVersion,
                ["local_promotion_record_kind"] = "promotion",
                ["local_promotion_ready"] = bool.TrueString,
                ["local_promotion_not_ready"] = bool.FalseString,
                ["local_promotion_active_local_model_ref"] = activeRef,
                ["local_promotion_active_model_id"] = modelId,
                ["local_promotion_active_checkpoint_sha256"] = checkpointSha,
                ["local_promotion_prior_state_recorded"] = bool.TrueString,
                ["local_promotion_prior_state_kind"] = string.IsNullOrWhiteSpace(priorRef) ? "none" : "local_promotion_ref",
                ["local_promotion_prior_active_local_model_ref"] = priorRef,
                ["local_promotion_prior_active_model_id"] = priorModelId,
                ["local_promotion_prior_active_checkpoint_sha256"] = priorCheckpoint,
                ["local_promotion_rollback_supported"] = (!string.IsNullOrWhiteSpace(priorRef)).ToString(),
                ["promotion_upload_ref"] = activeRef,
                ["local_promoted_model_id"] = modelId,
                ["local_promoted_checkpoint_sha256"] = checkpointSha,
                ["deployment_verified"] = bool.FalseString,
                ["product_ready"] = bool.FalseString
            });
    }

    private static string PriorScopeFromRef(string priorRef)
        => priorRef.Split('/').Last();

    private static string ResolvePhysicalActiveRef(
        IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult> records)
    {
        var active = string.Empty;
        foreach (var record in records)
        {
            if (string.Equals(ReadMetadata(record, "local_promotion_record_kind"), "rollback", StringComparison.Ordinal))
            {
                active = ReadMetadata(record, "local_promotion_rollback_restored_active_local_model_ref");
                continue;
            }

            if (record.LocalPromotionReady)
                active = ReadMetadata(record, "local_promotion_active_local_model_ref");
        }

        return active;
    }

    private static string HashRef(string scope, string part)
    {
        var hash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                Encoding.UTF8.GetBytes($"{scope}|{part}"))).ToLowerInvariant();
        return $"sha256:{hash}";
    }

    private static bool IsCreatedSideEffect(DesignConversionTrainingIntakeLocalPromotionResult result)
        => string.Equals(
               ReadMetadata(result, "training_intake_idempotency_created_new_side_effect"),
               bool.TrueString,
               StringComparison.Ordinal)
           && string.Equals(
               ReadMetadata(result, "training_intake_idempotency_replay_hit"),
               bool.FalseString,
               StringComparison.Ordinal);

    private static bool IsReplay(DesignConversionTrainingIntakeLocalPromotionResult result)
        => string.Equals(
               ReadMetadata(result, "training_intake_idempotency_created_new_side_effect"),
               bool.FalseString,
               StringComparison.Ordinal)
           && string.Equals(
               ReadMetadata(result, "training_intake_idempotency_replay_hit"),
               bool.TrueString,
               StringComparison.Ordinal);

    private static string ReadMetadata(
        DesignConversionTrainingIntakeLocalPromotionResult result,
        string key)
        => ReadMetadata(result.BoundaryMetadata, key);

    private static string ReadMetadata(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null && metadata.TryGetValue(key, out var value)
            ? value
            : string.Empty;

    private static void AssertWriteFailed<T>(OperationResult<T> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedErrorCode, result.ErrorCode);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedSafeMessage, result.ErrorMessage);
    }

    private static async Task AssertTaskStillPendingAsync<T>(Task<T> task, string message)
    {
        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromMilliseconds(250)));
        Assert.NotSame(task, completed);
        Assert.False(task.IsCompleted, message);
    }

    private static void AssertDeterministicMutationLockHeld(string lockPath)
    {
        var ex = Assert.Throws<IOException>(() =>
        {
            using var blocked = new FileStream(
                lockPath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize: 1);
        });
        var nativeErrorCode = ex.HResult & 0xFFFF;
        Assert.True(
            nativeErrorCode is 32 or 33,
            $"Expected Windows sharing or lock violation 32/33, got {nativeErrorCode}.");
    }

    private static void AssertPromotionMutationLockFileEmpty(string lockPath)
    {
        Assert.True(File.Exists(lockPath), "Promotion mutation lock file should be deterministic and stable.");
        Assert.Equal(0, new FileInfo(lockPath).Length);
    }

    private static void AssertNoTempArtifacts(string root)
    {
        if (!Directory.Exists(root))
            return;

        Assert.DoesNotContain(
            Directory.EnumerateFiles(root),
            path => Path.GetFileName(path).Contains(TempSuffix, StringComparison.Ordinal));
    }

    private static string CreateTempRoot(string suffix)
        => Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), suffix);

    private static void DeleteDirectoryIfExists(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }
}
