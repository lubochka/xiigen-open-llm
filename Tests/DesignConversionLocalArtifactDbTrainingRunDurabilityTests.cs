// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed class DesignConversionLocalArtifactDbTrainingRunDurabilityTests
{
    private const string RequestJournalFileName = "training-intake-requests.jsonl";
    private const string RunJournalFileName = "training-intake-runs.jsonl";
    private const string RunJournalMutationLockFileName = RunJournalFileName + ".mutation.lock";
    private const string TempSuffix = ".copyonwrite.tmp";

    private static readonly Encoding StrictUtf8 = new UTF8Encoding(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private static readonly JsonSerializerOptions CompactJsonOptions =
        new(DesignConversionCheckpointJsonOptions.Standard)
        {
            WriteIndented = false
        };

    [Fact]
    public async Task RunTrainingIntakeAsync_MissingMeasurementFailsBeforeRunJournalOrPairStoreMutation()
    {
        var root = CreateTempRoot("training-run-missing-measurement");
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(db, "missing-measurement");
            var result = await db.RunTrainingIntakeAsync(
                intake.IntakeId,
                BuildRunOptions("missing-measurement") with
                {
                    MeasurementGateRequest = null
                },
                admissionBoundaryMetadata: BuildRunAdmissionMetadata(
                    "tenant:missing-measurement",
                    "idempotency:missing-measurement"));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_CALLER_ADMISSION_MEASUREMENT_NOT_PASSED",
                result.ErrorCode);
            Assert.False(File.Exists(Path.Combine(root, RunJournalFileName)));
            Assert.False(Directory.Exists(Path.Combine(
                root,
                JsonlDesignConversionLocalArtifactDb.TrainingIntakeDpoPairStoreDirectoryName)));
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Theory]
    [InlineData("below_score")]
    [InlineData("runtime")]
    [InlineData("cost")]
    [InlineData("ram")]
    [InlineData("leakage")]
    [InlineData("hidden_determinism")]
    [InlineData("ablation")]
    [InlineData("source_disjoint")]
    public async Task RunTrainingIntakeAsync_PresentFailingMeasurementLeavesRunAndPairStoresAbsent(
        string failure)
    {
        var root = CreateTempRoot($"training-run-failing-measurement-{failure}");
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(db, failure);
            var result = await db.RunTrainingIntakeAsync(
                intake.IntakeId,
                BuildRunOptions(failure) with
                {
                    MeasurementGateRequest = BuildFailingAdmissionMeasurement(
                        failure)
                },
                admissionBoundaryMetadata: BuildRunAdmissionMetadata(
                    $"tenant:{failure}",
                    $"idempotency:{failure}"));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_CALLER_ADMISSION_MEASUREMENT_NOT_PASSED",
                result.ErrorCode);
            Assert.False(File.Exists(Path.Combine(root, RunJournalFileName)));
            Assert.False(Directory.Exists(Path.Combine(
                root,
                JsonlDesignConversionLocalArtifactDb
                    .TrainingIntakeDpoPairStoreDirectoryName)));
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAsync_SameKeyChangedMeasurementContractConflictsWithoutAppend()
    {
        var root = CreateTempRoot("training-run-measurement-conflict");
        var journalPath = Path.Combine(root, RunJournalFileName);
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(db, "measurement-conflict");
            var admission = BuildRunAdmissionMetadata(
                "tenant:measurement-conflict",
                "idempotency:measurement-conflict");
            var options = BuildRunOptions("measurement-conflict");
            var first = await db.RunTrainingIntakeAsync(
                intake.IntakeId,
                options,
                admissionBoundaryMetadata: admission);
            var before = await File.ReadAllBytesAsync(journalPath);
            var conflict = await db.RunTrainingIntakeAsync(
                intake.IntakeId,
                options with
                {
                    MeasurementGateRequest = options.MeasurementGateRequest! with
                    {
                        GateId = "admission:measurement-conflict:changed"
                    }
                },
                admissionBoundaryMetadata: admission);

            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.False(conflict.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_TRAINING_INTAKE_RUN_IDEMPOTENCY_REPLAY_CONFLICT",
                conflict.ErrorCode);
            Assert.Equal(before, await File.ReadAllBytesAsync(journalPath));
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAsync_SameKeyReorderedMeasurementRefsReplaysWithoutAppend()
    {
        var root = CreateTempRoot("training-run-measurement-reordered-replay");
        var journalPath = Path.Combine(root, RunJournalFileName);
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(db, "measurement-reordered");
            var admission = BuildRunAdmissionMetadata(
                "tenant:measurement-reordered",
                "idempotency:measurement-reordered");
            var options = BuildRunOptions("measurement-reordered");
            var first = await db.RunTrainingIntakeAsync(
                intake.IntakeId,
                options,
                admissionBoundaryMetadata: admission);
            var before = await File.ReadAllBytesAsync(journalPath);
            var reordered = options with
            {
                MeasurementGateRequest = options.MeasurementGateRequest! with
                {
                    HeldOutMetrics = options.MeasurementGateRequest.HeldOutMetrics! with
                    {
                        EvidenceRefs = options.MeasurementGateRequest.HeldOutMetrics
                            .EvidenceRefs.Reverse().ToArray()
                    },
                    IntegrityProof = options.MeasurementGateRequest.IntegrityProof! with
                    {
                        EvidenceRefs = options.MeasurementGateRequest.IntegrityProof
                            .EvidenceRefs.Reverse().ToArray()
                    }
                }
            };
            var replay = await db.RunTrainingIntakeAsync(
                intake.IntakeId,
                reordered,
                admissionBoundaryMetadata: admission);

            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.True(replay.IsSuccess, replay.ErrorMessage);
            Assert.True(IsReplay(replay.Value!));
            Assert.Equal(before, await File.ReadAllBytesAsync(journalPath));
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAsync_DifferentKeys_PersistTwoUtf8RunRowsAndFreshReplayDoesNotAppend()
    {
        var root = CreateTempRoot("training-run-durable-two-rows");
        var journalPath = Path.Combine(root, RunJournalFileName);

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(db, "two-rows");
            var options = BuildRunOptions("r7-b2-two-rows-run");
            var firstAdmission = BuildRunAdmissionMetadata(
                "tenant:r7-b2-two-rows",
                "idempotency:r7-b2-two-rows-001");
            var secondAdmission = BuildRunAdmissionMetadata(
                "tenant:r7-b2-two-rows",
                "idempotency:r7-b2-two-rows-002");

            var first = await db.RunTrainingIntakeAsync(intake.IntakeId, options, admissionBoundaryMetadata: firstAdmission);
            var second = await db.RunTrainingIntakeAsync(intake.IntakeId, options, admissionBoundaryMetadata: secondAdmission);

            var firstRun = AssertBlockedRunSuccess(first, intake.IntakeId, options.RunId);
            var secondRun = AssertBlockedRunSuccess(second, intake.IntakeId, options.RunId);
            Assert.True(IsCreatedSideEffect(firstRun));
            Assert.True(IsCreatedSideEffect(secondRun));

            var physicalRecords = ReadPhysicalRunJournalRecords(journalPath);
            Assert.Equal(2, physicalRecords.Count);
            Assert.Equal(
                ["idempotency:r7-b2-two-rows-001", "idempotency:r7-b2-two-rows-002"],
                physicalRecords
                    .Select(record => ReadMetadata(record, "training_intake_idempotency_key_ref"))
                    .ToArray());
            Assert.All(physicalRecords, record => Assert.False(record.TrainingRunnerInvoked));
            var originalBytes = await File.ReadAllBytesAsync(journalPath);

            var replay = await new JsonlDesignConversionLocalArtifactDb(root).RunTrainingIntakeAsync(
                intake.IntakeId,
                options,
                admissionBoundaryMetadata: firstAdmission);

            var replayRun = AssertBlockedRunSuccess(replay, intake.IntakeId, options.RunId);
            Assert.True(IsReplay(replayRun));
            Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
            Assert.Equal(2, ReadPhysicalRunJournalRecords(journalPath).Count);
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAsync_MalformedRunLineAfterBlank_ReturnsSafeOperationResultAndPreservesBytes()
    {
        var root = CreateTempRoot("training-run-malformed-line");
        var journalPath = Path.Combine(root, RunJournalFileName);
        const string privateMarker = "PRIVATE_N9_1A_TRAINING_RUN_SENTINEL";
        var malformedLine = $"{{ malformed: \"{privateMarker}\"";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(db, "malformed-run-line");
            var options = BuildRunOptions("r7-b2-malformed-run-line");
            var admission = BuildRunAdmissionMetadata(
                "tenant:n9-1a-run-malformed",
                "idempotency:n9-1a-run-malformed");
            var seed = await db.RunTrainingIntakeAsync(
                intake.IntakeId,
                options,
                admissionBoundaryMetadata: admission);
            AssertBlockedRunSuccess(seed, intake.IntakeId, options.RunId);

            var validReplay = await new JsonlDesignConversionLocalArtifactDb(root).RunTrainingIntakeAsync(
                intake.IntakeId,
                options,
                admissionBoundaryMetadata: admission);
            var replayRun = AssertBlockedRunSuccess(validReplay, intake.IntakeId, options.RunId);
            Assert.True(IsReplay(replayRun));
            Assert.Single(ReadPhysicalRunJournalRecords(journalPath));

            await File.AppendAllTextAsync(
                journalPath,
                Environment.NewLine + malformedLine + Environment.NewLine,
                StrictUtf8);
            var bytesBeforeRead = await File.ReadAllBytesAsync(journalPath);

            var failedReplay = await new JsonlDesignConversionLocalArtifactDb(root).RunTrainingIntakeAsync(
                intake.IntakeId,
                options,
                admissionBoundaryMetadata: admission);

            Assert.False(failedReplay.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_TRAINING_INTAKE_RUN_LINE_INVALID", failedReplay.ErrorCode);
            Assert.Contains("line 3", failedReplay.ErrorMessage, StringComparison.Ordinal);
            Assert.Equal(bytesBeforeRead, await File.ReadAllBytesAsync(journalPath));
            AssertNoTempArtifacts(root);

            var serializedError = JsonSerializer.Serialize(
                new { failedReplay.ErrorCode, failedReplay.ErrorMessage },
                DesignConversionCheckpointJsonOptions.Standard);
            var returnedErrorText = string.Join('|', failedReplay.ErrorCode, failedReplay.ErrorMessage, serializedError);
            foreach (var forbidden in new[]
            {
                root,
                journalPath,
                privateMarker,
                malformedLine
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
    public async Task RunTrainingIntakeAsync_InjectedPublicationFailureAfterTempFlush_FailsSafelyPreservesBytesAndRetryAppendsOnce()
    {
        var root = CreateTempRoot("training-run-durable-locked");
        var journalPath = Path.Combine(root, RunJournalFileName);
        const string tenant = "tenant:r7-b2-locked";
        const string idempotencyKey = "idempotency:r7-b2-locked-retry";
        const string privateMarker = "PRIVATE_R7_B2_TRAINING_RUN_LOCKED_MARKER";

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(setupDb, "locked");
            var seed = await setupDb.RunTrainingIntakeAsync(
                intake.IntakeId,
                BuildRunOptions("r7-b2-locked-seed"),
                admissionBoundaryMetadata: BuildRunAdmissionMetadata(tenant, "idempotency:r7-b2-locked-seed"));
            AssertBlockedRunSuccess(seed, intake.IntakeId, "r7-b2-locked-seed");
            var originalBytes = await File.ReadAllBytesAsync(journalPath);
            var remainingFailures = 1;
            var publicationCallbackCount = 0;
            DesignConversionTrainingJournalPublicationKind? observedKind = null;
            var faultingDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                trainingJournalBeforeAtomicPublishCallbackForTesting: kind =>
                {
                    observedKind = kind;
                    Interlocked.Increment(ref publicationCallbackCount);
                    // Одноразовый отказ попадает в границу публикации после fsync и одинаково работает на обеих ОС.
                    if (Interlocked.Exchange(ref remainingFailures, 0) == 1)
                    {
                        throw new IOException(
                            $"Injected run journal publication failure: {root}|{journalPath}|{tenant}|{idempotencyKey}|{privateMarker}");
                    }
                });

            var lockedResult = await faultingDb.RunTrainingIntakeAsync(
                intake.IntakeId,
                BuildRunOptions($"r7-b2-locked-failed-{privateMarker}"),
                admissionBoundaryMetadata: BuildRunAdmissionMetadata(tenant, idempotencyKey));

            AssertWriteFailed(lockedResult);
            Assert.Equal(
                DesignConversionTrainingJournalPublicationKind.TrainingIntakeRuns,
                observedKind);
            Assert.Equal(1, Volatile.Read(ref publicationCallbackCount));
            Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
            AssertNoTempArtifacts(root);

            // Повтор тем же экземпляром обязан получить освобождённый gate и записать единственную строку.
            var retry = await faultingDb.RunTrainingIntakeAsync(
                    intake.IntakeId,
                    BuildRunOptions("r7-b2-locked-retry"),
                    admissionBoundaryMetadata: BuildRunAdmissionMetadata(tenant, idempotencyKey))
                .WaitAsync(TimeSpan.FromSeconds(10));
            var retryRun = AssertBlockedRunSuccess(retry, intake.IntakeId, "r7-b2-locked-retry");
            var records = ReadPhysicalRunJournalRecords(journalPath);

            Assert.True(IsCreatedSideEffect(retryRun));
            Assert.Equal(2, records.Count);
            Assert.Equal(1, records.Count(record => record.RunId == "r7-b2-locked-retry"));
            Assert.Equal(2, Volatile.Read(ref publicationCallbackCount));
            AssertNoTempArtifacts(root);

            var returnedErrorText = string.Join('|', lockedResult.ErrorCode, lockedResult.ErrorMessage);
            foreach (var forbidden in new[]
            {
                root,
                journalPath,
                RunJournalFileName,
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
    public async Task RunTrainingIntakeAsync_UnreadableExistingRunJournal_FailsWithRunReadErrorPreservesBytesAndReplayDoesNotAppend()
    {
        var root = CreateTempRoot("training-run-durable-unreadable");
        var journalPath = Path.Combine(root, RunJournalFileName);
        const string tenant = "tenant:r7-b2-unreadable-run";
        const string idempotencyKey = "idempotency:r7-b2-unreadable-run-replay";
        const string sensitiveRunMarker = "sensitive-run-marker-r7-b2-unreadable-replay";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(db, "unreadable-run");
            var options = BuildRunOptions($"r7-b2-unreadable-{sensitiveRunMarker}");
            var admission = BuildRunAdmissionMetadata(tenant, idempotencyKey);
            var seed = await db.RunTrainingIntakeAsync(
                intake.IntakeId,
                options,
                admissionBoundaryMetadata: admission);
            var seedRun = AssertBlockedRunSuccess(seed, intake.IntakeId, options.RunId);
            Assert.True(IsCreatedSideEffect(seedRun));

            var originalBytes = await File.ReadAllBytesAsync(journalPath);
            var originalRecords = ReadPhysicalRunJournalRecords(journalPath);
            Assert.Single(originalRecords);

            OperationResult<DesignConversionTrainingIntakeRunResult> lockedResult;
            await using (var held = new FileStream(
                journalPath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None))
            {
                lockedResult = await new JsonlDesignConversionLocalArtifactDb(root).RunTrainingIntakeAsync(
                    intake.IntakeId,
                    options,
                    admissionBoundaryMetadata: admission);

                Assert.False(lockedResult.IsSuccess);
                Assert.Equal(
                    "DESIGN_CONVERSION_TRAINING_INTAKE_RUN_READ_FAILED",
                    lockedResult.ErrorCode);
                Assert.Equal(
                    "DesignConversion training intake runs could not be read safely.",
                    lockedResult.ErrorMessage);
                Assert.NotEqual("DESIGN_CONVERSION_LOCAL_ARTIFACT_DB_WRITE_FAILED", lockedResult.ErrorCode);
                AssertNoTempArtifacts(root);
            }

            Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
            Assert.Single(ReadPhysicalRunJournalRecords(journalPath));

            var returnedErrorText = JsonSerializer.Serialize(
                new { lockedResult.ErrorCode, lockedResult.ErrorMessage },
                DesignConversionCheckpointJsonOptions.Standard);
            foreach (var forbidden in new[]
            {
                root,
                journalPath,
                RunJournalFileName,
                TempSuffix,
                "IOException",
                "UnauthorizedAccessException",
                "SecurityException",
                "System.IO",
                tenant,
                idempotencyKey,
                sensitiveRunMarker
            })
            {
                Assert.DoesNotContain(forbidden, returnedErrorText, StringComparison.OrdinalIgnoreCase);
            }

            var replay = await new JsonlDesignConversionLocalArtifactDb(root).RunTrainingIntakeAsync(
                intake.IntakeId,
                options,
                admissionBoundaryMetadata: admission);
            var replayRun = AssertBlockedRunSuccess(replay, intake.IntakeId, options.RunId);

            Assert.True(IsReplay(replayRun));
            Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
            Assert.Single(ReadPhysicalRunJournalRecords(journalPath));
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAsync_ExternalRunMutationLeaseHeld_WaitsCancelsAndRetryAppendsOnce()
    {
        var root = CreateTempRoot("training-run-mutation-lease-held");
        var journalPath = Path.Combine(root, RunJournalFileName);
        var lockPath = BuildRunMutationLockPath(root);
        const string tenant = "tenant:n9-3a-run-lease";
        const string idempotencyKey = "idempotency:n9-3a-run-lease-wait";
        const string privateMarker = "PRIVATE_N9_3A_RUN_LEASE_SENTINEL";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(db, "mutation-lease");
            var seed = await db.RunTrainingIntakeAsync(
                intake.IntakeId,
                BuildRunOptions("n9-3a-run-lease-seed"),
                admissionBoundaryMetadata: BuildRunAdmissionMetadata(tenant, "idempotency:n9-3a-run-lease-seed"));
            AssertBlockedRunSuccess(seed, intake.IntakeId, "n9-3a-run-lease-seed");

            var originalBytes = await File.ReadAllBytesAsync(journalPath);
            var originalRows = ReadPhysicalRunJournalRecords(journalPath).Count;
            var options = BuildRunOptions("n9-3a-run-lease-after-release");
            var admission = BuildRunAdmissionMetadata(tenant, idempotencyKey);
            using var waitingCancellation = new CancellationTokenSource();

            await using (var heldLease = DesignConversionDeterministicMutationLock.AcquireForTesting(lockPath))
            {
                var waitingRun = new JsonlDesignConversionLocalArtifactDb(root).RunTrainingIntakeAsync(
                    intake.IntakeId,
                    options,
                    waitingCancellation.Token,
                    admission);

                await AssertTaskRemainsPendingAsync(
                    waitingRun,
                    TimeSpan.FromMilliseconds(300),
                    "The public training-intake run route must wait behind the external mutation lease.");
                Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
                Assert.Equal(originalRows, ReadPhysicalRunJournalRecords(journalPath).Count);

                waitingCancellation.Cancel();
                var cancelled = await waitingRun.WaitAsync(TimeSpan.FromSeconds(10));

                Assert.False(cancelled.IsSuccess);
                Assert.Equal("CANCELLED", cancelled.ErrorCode);
                Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
                Assert.Equal(originalRows, ReadPhysicalRunJournalRecords(journalPath).Count);

                var returnedErrorText = JsonSerializer.Serialize(
                    new { cancelled.ErrorCode, cancelled.ErrorMessage },
                    DesignConversionCheckpointJsonOptions.Standard);
                foreach (var forbidden in new[]
                {
                    root,
                    journalPath,
                    lockPath,
                    RunJournalMutationLockFileName,
                    tenant,
                    idempotencyKey,
                    privateMarker
                })
                {
                    Assert.DoesNotContain(forbidden, returnedErrorText, StringComparison.OrdinalIgnoreCase);
                }
            }

            AssertLockFileContainsNoPayload(lockPath, tenant, idempotencyKey, privateMarker);

            var retry = await new JsonlDesignConversionLocalArtifactDb(root).RunTrainingIntakeAsync(
                intake.IntakeId,
                options,
                admissionBoundaryMetadata: admission);
            var retryRun = AssertBlockedRunSuccess(retry, intake.IntakeId, options.RunId);
            var records = ReadPhysicalRunJournalRecords(journalPath);

            Assert.True(IsCreatedSideEffect(retryRun));
            Assert.Equal(originalRows + 1, records.Count);
            Assert.Equal(1, records.Count(record => record.RunId == options.RunId));
            AssertLockFileContainsNoPayload(lockPath, tenant, idempotencyKey, privateMarker);
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAsync_PreCancelledToken_PreservesExistingRunJournalBytesAndLeavesNoTemp()
    {
        var root = CreateTempRoot("training-run-durable-cancelled");
        var journalPath = Path.Combine(root, RunJournalFileName);

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(db, "cancelled");
            var seed = await db.RunTrainingIntakeAsync(
                intake.IntakeId,
                BuildRunOptions("r7-b2-cancelled-seed"),
                admissionBoundaryMetadata: BuildRunAdmissionMetadata(
                    "tenant:r7-b2-cancelled",
                    "idempotency:r7-b2-cancelled-seed"));
            AssertBlockedRunSuccess(seed, intake.IntakeId, "r7-b2-cancelled-seed");
            var originalBytes = await File.ReadAllBytesAsync(journalPath);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var cancelled = await new JsonlDesignConversionLocalArtifactDb(root).RunTrainingIntakeAsync(
                intake.IntakeId,
                BuildRunOptions("r7-b2-cancelled-second"),
                cts.Token,
                BuildRunAdmissionMetadata(
                    "tenant:r7-b2-cancelled",
                    "idempotency:r7-b2-cancelled-second"));

            Assert.False(cancelled.IsSuccess);
            Assert.Equal("CANCELLED", cancelled.ErrorCode);
            Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
            Assert.Single(ReadPhysicalRunJournalRecords(journalPath));
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAsync_ImmediateWaitObserverFailure_ReleasesSharedRunGate()
    {
        var root = CreateTempRoot("training-run-durable-observer-failure");
        var journalPath = Path.Combine(root, RunJournalFileName);

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(setupDb, "observer-failure");
            var observerFailure = new InvalidOperationException(
                "R7-B2 unique immediate run wait observer failure.");
            var immediateAcquisitionObserved = false;
            var throwingDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeGateWaitTaskObserverForTesting: waitCompletedSuccessfully =>
                {
                    immediateAcquisitionObserved = waitCompletedSuccessfully;
                    Assert.True(
                        waitCompletedSuccessfully,
                        "The first run must acquire the uncontended shared run gate synchronously.");
                    throw observerFailure;
                },
                runTrainingIntakeGateAcquiredCallbackForTesting: null);

            var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                throwingDb.RunTrainingIntakeAsync(
                    intake.IntakeId,
                    BuildRunOptions("r7-b2-observer-failure-first"),
                    admissionBoundaryMetadata: BuildRunAdmissionMetadata(
                        "tenant:r7-b2-observer-failure",
                        "idempotency:r7-b2-observer-failure-first")));

            Assert.Same(observerFailure, thrown);
            Assert.True(immediateAcquisitionObserved);
            Assert.False(File.Exists(journalPath));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var second = await new JsonlDesignConversionLocalArtifactDb(root).RunTrainingIntakeAsync(
                intake.IntakeId,
                BuildRunOptions("r7-b2-observer-failure-retry"),
                timeout.Token,
                BuildRunAdmissionMetadata(
                    "tenant:r7-b2-observer-failure",
                    "idempotency:r7-b2-observer-failure-retry"));

            AssertBlockedRunSuccess(second, intake.IntakeId, "r7-b2-observer-failure-retry");
            var record = Assert.Single(ReadPhysicalRunJournalRecords(journalPath));
            Assert.Equal("r7-b2-observer-failure-retry", record.RunId);
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAsync_ConcurrentSameKeyAcrossInstances_AppendsOneRowAndReturnsOneReplay()
    {
        var root = CreateTempRoot("training-run-durable-same-key-gate");
        var journalPath = Path.Combine(root, RunJournalFileName);

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(setupDb, "same-key-gate");
            var options = BuildRunOptions("r7-b2-same-key-gate-run");
            var admission = BuildRunAdmissionMetadata(
                "tenant:r7-b2-same-key-gate",
                "idempotency:r7-b2-same-key-gate");
            var firstGateAcquired = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseFirstGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var secondWaitObserved = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var firstDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeGateWaitTaskObserverForTesting: null,
                runTrainingIntakeGateAcquiredCallbackForTesting: async ct =>
                {
                    firstGateAcquired.TrySetResult(true);
                    await releaseFirstGate.Task.WaitAsync(ct);
                });
            var secondDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeGateWaitTaskObserverForTesting: waitCompletedSuccessfully =>
                {
                    secondWaitObserved.TrySetResult(waitCompletedSuccessfully);
                },
                runTrainingIntakeGateAcquiredCallbackForTesting: null);
            var firstTask = firstDb.RunTrainingIntakeAsync(
                intake.IntakeId,
                options,
                admissionBoundaryMetadata: admission);

            var results = await ObserveSecondRunWaitsForSharedGateAndDrainAsync(
                firstTask,
                firstGateAcquired,
                releaseFirstGate,
                secondWaitObserved,
                () => secondDb.RunTrainingIntakeAsync(
                    intake.IntakeId,
                    options,
                    admissionBoundaryMetadata: admission),
                "The second DB instance must observe a pending shared run-gate wait while the first instance holds the gate.");

            Assert.All(results, result => AssertBlockedRunSuccess(result, intake.IntakeId, options.RunId));
            Assert.Equal(1, results.Count(result => IsCreatedSideEffect(result.Value!)));
            Assert.Equal(1, results.Count(result => IsReplay(result.Value!)));
            var record = Assert.Single(ReadPhysicalRunJournalRecords(journalPath));
            Assert.Equal(options.RunId, record.RunId);
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAsync_NonIdempotentRunWaitsOnGlobalRunGateAndPersistsDistinctRow()
    {
        var root = CreateTempRoot("training-run-durable-global-gate");
        var journalPath = Path.Combine(root, RunJournalFileName);

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(setupDb, "global-gate");
            var firstOptions = BuildRunOptions("r7-b2-global-gate-first");
            var secondOptions = BuildRunOptions("r7-b2-global-gate-second-no-metadata");
            var firstGateAcquired = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseFirstGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var secondWaitObserved = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var firstDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeGateWaitTaskObserverForTesting: null,
                runTrainingIntakeGateAcquiredCallbackForTesting: async ct =>
                {
                    firstGateAcquired.TrySetResult(true);
                    await releaseFirstGate.Task.WaitAsync(ct);
                });
            var secondDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeGateWaitTaskObserverForTesting: waitCompletedSuccessfully =>
                {
                    secondWaitObserved.TrySetResult(waitCompletedSuccessfully);
                },
                runTrainingIntakeGateAcquiredCallbackForTesting: null);
            var firstTask = firstDb.RunTrainingIntakeAsync(
                intake.IntakeId,
                firstOptions,
                admissionBoundaryMetadata: BuildRunAdmissionMetadata(
                    "tenant:r7-b2-global-gate",
                    "idempotency:r7-b2-global-gate"));

            var results = await ObserveSecondRunWaitsForSharedGateAndDrainAsync(
                firstTask,
                firstGateAcquired,
                releaseFirstGate,
                secondWaitObserved,
                () => secondDb.RunTrainingIntakeAsync(intake.IntakeId, secondOptions),
                "A non-idempotent run on another DB instance must wait on the same root run gate.");
            var records = ReadPhysicalRunJournalRecords(journalPath);

            Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
            Assert.Equal(2, records.Count);
            Assert.Contains(records, record => record.RunId == firstOptions.RunId);
            Assert.Contains(records, record => record.RunId == secondOptions.RunId);
            Assert.All(records, record => Assert.False(record.TrainingRunnerInvoked));
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAsync_SameTenantKeyDifferentOptions_FailsConflictWithoutChangingBytes()
    {
        var root = CreateTempRoot("training-run-durable-conflict");
        var journalPath = Path.Combine(root, RunJournalFileName);

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(db, "conflict");
            var admission = BuildRunAdmissionMetadata(
                "tenant:r7-b2-conflict",
                "idempotency:r7-b2-conflict");
            var created = await db.RunTrainingIntakeAsync(
                intake.IntakeId,
                BuildRunOptions("r7-b2-conflict-original"),
                admissionBoundaryMetadata: admission);
            AssertBlockedRunSuccess(created, intake.IntakeId, "r7-b2-conflict-original");
            var originalBytes = await File.ReadAllBytesAsync(journalPath);

            var conflict = await new JsonlDesignConversionLocalArtifactDb(root).RunTrainingIntakeAsync(
                intake.IntakeId,
                BuildRunOptions("r7-b2-conflict-changed-options"),
                admissionBoundaryMetadata: admission);

            Assert.False(conflict.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_TRAINING_INTAKE_RUN_IDEMPOTENCY_REPLAY_CONFLICT",
                conflict.ErrorCode);
            Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
            Assert.Single(ReadPhysicalRunJournalRecords(journalPath));
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAsync_ExactSemanticDuplicateRequestRowsResolveOneAuthorityAndAppendOneBlockedRun()
    {
        var root = CreateTempRoot("training-run-semantic-duplicate-request-authority");
        var requestJournalPath = Path.Combine(root, RequestJournalFileName);
        var runJournalPath = Path.Combine(root, RunJournalFileName);

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateBlockedIntakeAsync(
                db,
                "semantic-duplicate-request-authority");
            var durable = Assert.Single(ReadPhysicalRequestJournalRecords(requestJournalPath));
            var duplicateMetadata = new Dictionary<string, string>(
                durable.BoundaryMetadata,
                StringComparer.Ordinal)
            {
                ["training_intake_admission_boundary_scope"] = "derived-run-fixture-scope",
                ["training_intake_create_replay_cache_boundary"] = "derived-run-fixture-cache",
                ["training_intake_replay_cache_checked"] = bool.FalseString,
                ["training_intake_idempotency_replay_hit"] = bool.TrueString,
                ["training_intake_idempotency_created_new_side_effect"] = bool.FalseString,
                ["training_intake_idempotent_replay_result"] = bool.TrueString,
                ["training_intake_idempotency_side_effect_boundary"] = "derived-run-fixture-side-effect"
            };
            var duplicate = durable with { BoundaryMetadata = duplicateMetadata };
            await WritePhysicalRequestJournalRecordsAsync(
                requestJournalPath,
                durable,
                duplicate);
            var beforeRequestBytes = await File.ReadAllBytesAsync(requestJournalPath);
            var options = BuildRunOptions("w080-l2-semantic-duplicate-blocked-run");
            var admission = BuildRunAdmissionMetadata(
                "tenant:w080-l2-semantic-duplicate-blocked-run",
                "idempotency:w080-l2-semantic-duplicate-blocked-run");

            var first = await db.RunTrainingIntakeAsync(
                intake.IntakeId,
                options,
                admissionBoundaryMetadata: admission);
            var firstRun = AssertBlockedRunSuccess(first, intake.IntakeId, options.RunId);
            var afterFirstRunBytes = await File.ReadAllBytesAsync(runJournalPath);
            var replay = await new JsonlDesignConversionLocalArtifactDb(root)
                .RunTrainingIntakeAsync(
                    intake.IntakeId,
                    options,
                    admissionBoundaryMetadata: admission);
            var replayRun = AssertBlockedRunSuccess(replay, intake.IntakeId, options.RunId);
            var afterReplayBytes = await File.ReadAllBytesAsync(runJournalPath);

            Assert.True(IsCreatedSideEffect(firstRun));
            Assert.True(IsReplay(replayRun));
            Assert.Equal(beforeRequestBytes, await File.ReadAllBytesAsync(requestJournalPath));
            Assert.Equal(afterFirstRunBytes, afterReplayBytes);
            Assert.Single(ReadPhysicalRunJournalRecords(runJournalPath));
            Assert.Equal(2, ReadPhysicalRequestJournalRecords(requestJournalPath).Count);
            Assert.False(firstRun.TrainingRunnerInvoked);
            Assert.False(replayRun.TrainingRunnerInvoked);
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public void PhysicalRunJournalReader_RejectsBomBlankDoubleNewlineMalformedAndNullJsonRows()
    {
        var root = CreateTempRoot("training-run-physical-reader");

        try
        {
            Directory.CreateDirectory(root);
            AssertPhysicalReaderRejects<Xunit.Sdk.FalseException>(
                Path.Combine(root, "bom.jsonl"),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetPreamble()
                    .Concat(StrictUtf8.GetBytes("{}" + Environment.NewLine))
                    .ToArray());
            AssertPhysicalReaderRejects<Xunit.Sdk.AllException>(
                Path.Combine(root, "interior-blank.jsonl"),
                StrictUtf8.GetBytes("null" + Environment.NewLine + Environment.NewLine + "null" + Environment.NewLine));
            AssertPhysicalReaderRejects<Xunit.Sdk.AllException>(
                Path.Combine(root, "double-terminal-newline.jsonl"),
                StrictUtf8.GetBytes("{}" + Environment.NewLine + Environment.NewLine));
            AssertPhysicalReaderRejects<JsonException>(
                Path.Combine(root, "malformed.jsonl"),
                StrictUtf8.GetBytes("{not-json}" + Environment.NewLine));
            AssertPhysicalReaderRejects<Xunit.Sdk.NotNullException>(
                Path.Combine(root, "null.jsonl"),
                StrictUtf8.GetBytes("null" + Environment.NewLine));
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
        var handoff =
            await DesignConversionPostEvaluationEligibilityTestFixture
                .SeedApprovedHandoffAsync(
                    db,
                    scope);
        var created = await db.CreateTrainingIntakeAsync(
            handoff,
            "requester:r7-b2-training-run-durability",
            $"blocked training run durability intake {scope}",
            DateTimeOffset.UnixEpoch.AddMinutes(200),
            admissionBoundaryMetadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["training_intake_tenant_id_ref"] = $"tenant:r7-b2-create-{scope}",
                ["training_intake_idempotency_key_ref"] = $"idempotency:r7-b2-create-{scope}"
            });
        Assert.True(created.IsSuccess, created.ErrorMessage);

        var preflight = await db.RunTrainingIntakePreflightAsync(created.Value!.IntakeId);
        Assert.True(preflight.IsSuccess, preflight.ErrorMessage);
        Assert.False(preflight.Value!.MayStartTraining);
        Assert.Equal("proven", preflight.Value.SourceManifestStatus);
        Assert.Equal(
            "blocked_same_source",
            preflight.Value.SourceDisjointStatus);
        Assert.Contains(
            "source_disjoint_blocked_same_source",
            preflight.Value.BlockReasons);

        return created.Value;
    }

    private static async Task<OperationResult<DesignConversionTrainingIntakeRunResult>[]> ObserveSecondRunWaitsForSharedGateAndDrainAsync(
        Task<OperationResult<DesignConversionTrainingIntakeRunResult>> firstTask,
        TaskCompletionSource<bool> firstGateAcquired,
        TaskCompletionSource<bool> releaseFirstGate,
        TaskCompletionSource<bool> secondWaitObserved,
        Func<Task<OperationResult<DesignConversionTrainingIntakeRunResult>>> startSecond,
        string assertionMessage)
    {
        Task<OperationResult<DesignConversionTrainingIntakeRunResult>>? secondTask = null;
        OperationResult<DesignConversionTrainingIntakeRunResult>[]? results = null;
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

    private static async Task AssertTaskRemainsPendingAsync<T>(
        Task<T> task,
        TimeSpan interval,
        string assertionMessage)
    {
        var completed = await Task.WhenAny(task, Task.Delay(interval));
        Assert.False(ReferenceEquals(task, completed), assertionMessage);
    }

    private static DesignConversionTrainingIntakeRunResult AssertBlockedRunSuccess(
        OperationResult<DesignConversionTrainingIntakeRunResult> result,
        string intakeId,
        string runId)
    {
        Assert.True(result.IsSuccess, result.ErrorMessage);
        var run = result.Value!;
        Assert.Equal(intakeId, run.TrainingIntakeId);
        Assert.Equal(runId, run.RunId);
        Assert.False(run.MayStartTraining);
        Assert.Equal("proven", run.SourceManifestStatus);
        Assert.Equal("blocked_same_source", run.SourceDisjointStatus);
        Assert.Contains(
            "source_disjoint_blocked_same_source",
            run.BlockReasons);
        Assert.False(run.TrainingRunnerInvoked);
        Assert.False(run.LocalLifecycleHookAttempted);
        Assert.False(run.TrainingStarted);
        Assert.False(run.CheckpointMutated);
        Assert.False(run.CheckpointCandidateCreated);
        Assert.False(run.WeightsProduced);
        Assert.False(run.ModelUploadReady);
        Assert.Equal("blocked_before_runner_by_training_intake_preflight", run.TrainingExecutionBoundary);
        Assert.Equal("blocked_before_runner_by_training_intake_preflight", run.TrainingExecutionStatus);
        Assert.Null(run.RunnerEvidence);
        return run;
    }

    private static IReadOnlyList<DesignConversionTrainingIntakeRunResult> ReadPhysicalRunJournalRecords(string journalPath)
    {
        var bytes = File.ReadAllBytes(journalPath);
        var utf8Preamble = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: true,
            throwOnInvalidBytes: true).GetPreamble();
        Assert.False(bytes.AsSpan().StartsWith(utf8Preamble), "Training intake run journal must not have a UTF-8 BOM.");

        var text = StrictUtf8.GetString(bytes);
        Assert.DoesNotContain("\uFFFD", text, StringComparison.Ordinal);
        Assert.True(
            text.EndsWith(Environment.NewLine, StringComparison.Ordinal),
            "Training intake run journal must end with exactly one platform newline.");

        var segments = text.Split(Environment.NewLine, StringSplitOptions.None);
        Assert.True(segments.Length >= 2, "Training intake run journal must contain at least one framed row.");
        Assert.Empty(segments[^1]);
        Assert.All(segments[..^1], segment => Assert.False(string.IsNullOrWhiteSpace(segment)));

        return segments[..^1]
            .Select(segment =>
            {
                var record = JsonSerializer.Deserialize<DesignConversionTrainingIntakeRunResult>(
                    segment,
                    DesignConversionCheckpointJsonOptions.Standard);
                Assert.NotNull(record);
                return record;
            })
            .ToArray();
    }

    private static IReadOnlyList<DesignConversionTrainingIntakeRecord> ReadPhysicalRequestJournalRecords(
        string journalPath)
    {
        var text = StrictUtf8.GetString(File.ReadAllBytes(journalPath));
        Assert.EndsWith(Environment.NewLine, text, StringComparison.Ordinal);
        var segments = text.Split(Environment.NewLine, StringSplitOptions.None);
        Assert.Empty(segments[^1]);
        Assert.All(segments[..^1], segment => Assert.False(string.IsNullOrWhiteSpace(segment)));
        return segments[..^1]
            .Select(segment =>
            {
                var record = JsonSerializer.Deserialize<DesignConversionTrainingIntakeRecord>(
                    segment,
                    DesignConversionCheckpointJsonOptions.Standard);
                Assert.NotNull(record);
                return record;
            })
            .ToArray();
    }

    private static async Task WritePhysicalRequestJournalRecordsAsync(
        string journalPath,
        params DesignConversionTrainingIntakeRecord[] records)
    {
        var text = string.Join(
            Environment.NewLine,
            records.Select(record => JsonSerializer.Serialize(record, CompactJsonOptions)))
            + Environment.NewLine;
        await File.WriteAllTextAsync(journalPath, text, StrictUtf8);
    }

    private static void AssertPhysicalReaderRejects<TException>(string path, byte[] bytes)
        where TException : Exception
    {
        File.WriteAllBytes(path, bytes);
        Assert.Throws<TException>(() => ReadPhysicalRunJournalRecords(path));
    }

    private static string BuildRunMutationLockPath(string root)
        => Path.Combine(root, RunJournalMutationLockFileName);

    private static void AssertLockFileContainsNoPayload(
        string lockPath,
        string tenant,
        string idempotencyKey,
        string privateMarker)
    {
        Assert.True(File.Exists(lockPath));
        var bytes = File.ReadAllBytes(lockPath);
        Assert.Empty(bytes);

        var lockText = StrictUtf8.GetString(bytes);
        Assert.DoesNotContain(tenant, lockText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(idempotencyKey, lockText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(privateMarker, lockText, StringComparison.OrdinalIgnoreCase);
    }

    private static DesignConversionTrainingPipelineOptions BuildRunOptions(string runId)
        => new(
            RunId: runId,
            BatchSize: 1,
            MeasurementGateRequest: BuildPassingAdmissionMeasurement(runId));

    private static DesignConversionMeasurementGateRequest
        BuildPassingAdmissionMeasurement(string scope)
        => new(
            GateId: $"admission:{scope}",
            UnitId: DesignConversionDpoUnits.CriticUnit,
            HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                1,
                0.95,
                10,
                0.01m,
                64,
                [
                    $"artifact://measurement/{scope}/heldout-a.json",
                    $"artifact://measurement/{scope}/heldout-b.json"
                ]),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                true,
                false,
                false,
                true,
                [
                    $"artifact://measurement/{scope}/integrity-a.json",
                    $"artifact://measurement/{scope}/integrity-b.json"
                ]),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                0.90,
                100,
                1m,
                512));

    private static DesignConversionMeasurementGateRequest
        BuildFailingAdmissionMeasurement(string failure)
    {
        var passing = BuildPassingAdmissionMeasurement(failure);
        return failure switch
        {
            "below_score" => passing with
            {
                HeldOutMetrics = passing.HeldOutMetrics! with
                {
                    HeldOutQualityScore = 0.5
                }
            },
            "runtime" => passing with
            {
                HeldOutMetrics = passing.HeldOutMetrics! with { RuntimeMs = 101 }
            },
            "cost" => passing with
            {
                HeldOutMetrics = passing.HeldOutMetrics! with { CostUsd = 1.01m }
            },
            "ram" => passing with
            {
                HeldOutMetrics = passing.HeldOutMetrics! with { PeakRamMb = 513 }
            },
            "leakage" => passing with
            {
                IntegrityProof = passing.IntegrityProof! with { LeakageDetected = true }
            },
            "hidden_determinism" => passing with
            {
                IntegrityProof = passing.IntegrityProof! with
                {
                    HiddenDeterminismDetected = true
                }
            },
            "ablation" => passing with
            {
                IntegrityProof = passing.IntegrityProof! with { AblationPassed = false }
            },
            "source_disjoint" => passing with
            {
                IntegrityProof = passing.IntegrityProof! with { SourceDisjoint = false }
            },
            _ => throw new ArgumentOutOfRangeException(nameof(failure))
        };
    }

    private static IReadOnlyDictionary<string, string> BuildRunAdmissionMetadata(
        string tenant,
        string idempotencyKey)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] = tenant,
            ["training_intake_idempotency_key_ref"] = idempotencyKey
        };

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
            ApprovedByRef: "reviewer:r7-b2-training-run-durability",
            ApprovalReason: "approved handoff row for blocked training run durability test",
            ApprovedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(1).ToString("O"),
            SourceManifestRef: string.Empty,
            RecordHash: HashRef(scope, "record"),
            InternalUnitStateHash: HashRef(scope, "internal-unit-state"),
            SourceStructureHash: HashRef(scope, "source-structure"),
            ChosenPayloadHash: HashRef(scope, "chosen"),
            RejectedPayloadHash: HashRef(scope, "rejected"),
            TieBreakerHash: HashRef(scope, "tie-breaker"),
            ChosenHtmlRef: $"artifact://r7-b2/{scope}/chosen.html",
            ChosenCssRef: $"artifact://r7-b2/{scope}/chosen.css",
            RejectedHtmlRef: $"artifact://r7-b2/{scope}/rejected.html",
            RejectedCssRef: $"artifact://r7-b2/{scope}/rejected.css",
            DesignTreeRef: $"artifact://r7-b2/{scope}/design-tree.json",
            OriginalTreeRef: $"artifact://r7-b2/{scope}/original-tree.json",
            SourceArtifactRefs: [$"artifact://r7-b2/{scope}/source.png"],
            RenderArtifactRefs: [$"artifact://r7-b2/{scope}/render.png"],
            ComparisonArtifactRefs: [$"artifact://r7-b2/{scope}/comparison.png"],
            EvidenceRefs: ["r7-b2-training-run-durability"],
            BoundaryMetadata: BuildHandoffBoundaryMetadata(1));

        return new DesignConversionApprovedDpoHandoffArtifact(
            ArtifactId: $"approved-dpo-handoff-{scope}",
            ArtifactRef: $"memory://r7-b2/{scope}/approved-dpo-handoff",
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

    private static string HashRef(string scope, string part)
    {
        var hash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                Encoding.UTF8.GetBytes($"{scope}|{part}"))).ToLowerInvariant();
        return $"sha256:{hash}";
    }

    private static bool IsCreatedSideEffect(DesignConversionTrainingIntakeRunResult result)
        => string.Equals(
               ReadMetadata(result, "training_intake_idempotency_created_new_side_effect"),
               bool.TrueString,
               StringComparison.Ordinal)
           && string.Equals(
               ReadMetadata(result, "training_intake_idempotency_replay_hit"),
               bool.FalseString,
               StringComparison.Ordinal);

    private static bool IsReplay(DesignConversionTrainingIntakeRunResult result)
        => string.Equals(
               ReadMetadata(result, "training_intake_idempotency_created_new_side_effect"),
               bool.FalseString,
               StringComparison.Ordinal)
           && string.Equals(
               ReadMetadata(result, "training_intake_idempotency_replay_hit"),
               bool.TrueString,
               StringComparison.Ordinal);

    private static string ReadMetadata(DesignConversionTrainingIntakeRunResult result, string key)
        => result.BoundaryMetadata.TryGetValue(key, out var value)
            ? value
            : string.Empty;

    private static void AssertWriteFailed<T>(OperationResult<T> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedErrorCode, result.ErrorCode);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedSafeMessage, result.ErrorMessage);
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
        => DesignConversionPostEvaluationEligibilityTestFixture
            .CreateHandleBoundTempRoot(suffix);

    private static void DeleteDirectoryIfExists(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }
}
