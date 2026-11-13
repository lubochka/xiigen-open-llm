// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed class DesignConversionLocalArtifactDbTrainingIntakeDurabilityTests
{
    private const string JournalFileName = "training-intake-requests.jsonl";
    private const string LockFileName = JournalFileName + ".mutation.lock";
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
    public void Constructor_RejectsNonPositiveTrainingIntakeRequestsMaxBytes()
    {
        var root = CreateTempRoot("training-intake-quota-validation");

        var zero = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new JsonlDesignConversionLocalArtifactDb(root, trainingIntakeRequestsMaxBytes: 0));
        var negative = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new JsonlDesignConversionLocalArtifactDb(root, trainingIntakeRequestsMaxBytes: -1));
        var defaultDb = new JsonlDesignConversionLocalArtifactDb(root);
        var customDb = new JsonlDesignConversionLocalArtifactDb(root, trainingIntakeRequestsMaxBytes: 17);

        Assert.Equal("trainingIntakeRequestsMaxBytes", zero.ParamName);
        Assert.Equal("trainingIntakeRequestsMaxBytes", negative.ParamName);
        Assert.True(JsonlDesignConversionLocalArtifactDb.DefaultTrainingIntakeRequestsMaxBytes > 0);
        Assert.Equal(
            JsonlDesignConversionLocalArtifactDb.DefaultTrainingIntakeRequestsMaxBytes,
            defaultDb.TrainingIntakeRequestsMaxBytes);
        Assert.Equal(17, customDb.TrainingIntakeRequestsMaxBytes);
    }

    [Fact]
    public async Task CreateTrainingIntakeAsync_RequestJournalQuotaFailsBeforeCopyOnWriteAndReplayStillSucceeds()
    {
        var root = CreateTempRoot("training-intake-request-quota");
        var journalPath = Path.Combine(root, JournalFileName);
        const string firstTenant = "tenant:training-intake-request-quota-first";
        const string firstIdempotencyKey = "idempotency:training-intake-request-quota-first";
        const string secondTenant = "tenant:training-intake-request-quota-second";
        const string secondIdempotencyKey = "idempotency:training-intake-request-quota-second";
        const string sensitivePayloadMarker = "PRIVATE_SENSITIVE_QUOTA_PAYLOAD_SENTINEL_7F3A9C";
        var firstAdmission = BuildCreateAdmissionMetadata(firstTenant, firstIdempotencyKey);
        var secondAdmission = BuildCreateAdmissionMetadata(secondTenant, secondIdempotencyKey);
        var firstRequestedAt = DateTimeOffset.UnixEpoch.AddMinutes(70);
        var secondRequestedAt = DateTimeOffset.UnixEpoch.AddMinutes(71);
        const string firstReason = "seed row before training intake request quota";
        var secondReason = $"second row should fail closed before copy on write {sensitivePayloadMarker}";

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var firstHandoff =
                await DesignConversionPostEvaluationEligibilityTestFixture
                    .SeedApprovedHandoffAsync(
                        setupDb,
                        "request-quota-first");
            var secondHandoff =
                await DesignConversionPostEvaluationEligibilityTestFixture
                    .SeedApprovedHandoffAsync(
                        setupDb,
                        "request-quota-second");
            var firstDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                recordsAuthorityMaxBytes: 1,
                trainingIntakeRequestsMaxBytes: JsonlDesignConversionLocalArtifactDb.DefaultTrainingIntakeRequestsMaxBytes);
            var first = await firstDb.CreateTrainingIntakeAsync(
                firstHandoff,
                "requester:training-intake-request-quota",
                firstReason,
                firstRequestedAt,
                admissionBoundaryMetadata: firstAdmission);

            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.True(IsCreatedSideEffect(first.Value!));
            var firstBytes = await File.ReadAllBytesAsync(journalPath);
            var exactCap = firstBytes.LongLength;
            Assert.True(exactCap > 0);
            Assert.Equal(1, firstDb.RecordsAuthorityMaxBytes);
            Assert.True(firstDb.TrainingIntakeRequestsMaxBytes >= exactCap);
            Assert.Single(ReadPhysicalJournalRecords(journalPath));
            AssertNoTempArtifacts(root);

            var cappedDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                recordsAuthorityMaxBytes: 1,
                trainingIntakeRequestsMaxBytes: exactCap);
            var overCap = await cappedDb.CreateTrainingIntakeAsync(
                secondHandoff,
                "requester:training-intake-request-quota",
                secondReason,
                secondRequestedAt,
                admissionBoundaryMetadata: secondAdmission);
            var afterFailureBytes = await File.ReadAllBytesAsync(journalPath);

            Assert.False(overCap.IsSuccess);
            Assert.Null(overCap.Value);
            Assert.Equal(
                JsonlDesignConversionLocalArtifactDb.TrainingIntakeRequestsQuotaExceededErrorCode,
                overCap.ErrorCode);
            Assert.Equal(
                JsonlDesignConversionLocalArtifactDb.TrainingIntakeRequestsQuotaExceededSafeMessage,
                overCap.ErrorMessage);
            Assert.NotEqual(
                JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedErrorCode,
                overCap.ErrorCode);
            Assert.NotEqual(
                JsonlDesignConversionLocalArtifactDb.LocalArtifactDbAuthorityQuotaExceededErrorCode,
                overCap.ErrorCode);
            Assert.Equal(firstBytes, afterFailureBytes);
            Assert.Single(ReadPhysicalJournalRecords(journalPath));
            AssertNoTempArtifacts(root);

            var returnedErrorText = string.Join('|', overCap.ErrorCode, overCap.ErrorMessage);
            foreach (var forbidden in new[]
            {
                root,
                journalPath,
                JournalFileName,
                TempSuffix,
                "IOException",
                "UnauthorizedAccessException",
                "SecurityException",
                "System.IO",
                firstTenant,
                firstIdempotencyKey,
                secondTenant,
                secondIdempotencyKey,
                sensitivePayloadMarker
            })
            {
                Assert.DoesNotContain(forbidden, returnedErrorText, StringComparison.OrdinalIgnoreCase);
            }

            var replayUnderExactCap = await new JsonlDesignConversionLocalArtifactDb(
                    root,
                    recordsAuthorityMaxBytes: 1,
                    trainingIntakeRequestsMaxBytes: exactCap)
                .CreateTrainingIntakeAsync(
                    firstHandoff,
                    "requester:training-intake-request-quota",
                    firstReason,
                    firstRequestedAt,
                    admissionBoundaryMetadata: firstAdmission);
            var afterReplayBytes = await File.ReadAllBytesAsync(journalPath);

            Assert.True(replayUnderExactCap.IsSuccess, replayUnderExactCap.ErrorMessage);
            Assert.True(IsReplay(replayUnderExactCap.Value!));
            Assert.Equal(first.Value!.IntakeId, replayUnderExactCap.Value!.IntakeId);
            Assert.Equal(firstBytes, afterReplayBytes);
            Assert.Single(ReadPhysicalJournalRecords(journalPath));
            AssertNoTempArtifacts(root);

            var raisedCap = JsonlDesignConversionLocalArtifactDb.DefaultTrainingIntakeRequestsMaxBytes;
            Assert.True(raisedCap > exactCap);
            var raisedRetry = await new JsonlDesignConversionLocalArtifactDb(
                    root,
                    recordsAuthorityMaxBytes: 1,
                    trainingIntakeRequestsMaxBytes: raisedCap)
                .CreateTrainingIntakeAsync(
                    secondHandoff,
                    "requester:training-intake-request-quota",
                    secondReason,
                    secondRequestedAt,
                    admissionBoundaryMetadata: secondAdmission);
            var afterRaisedRetryBytes = await File.ReadAllBytesAsync(journalPath);
            var secondReplay = await new JsonlDesignConversionLocalArtifactDb(
                    root,
                    recordsAuthorityMaxBytes: 1,
                    trainingIntakeRequestsMaxBytes: raisedCap)
                .CreateTrainingIntakeAsync(
                    secondHandoff,
                    "requester:training-intake-request-quota",
                    secondReason,
                    secondRequestedAt,
                    admissionBoundaryMetadata: secondAdmission);
            var afterSecondReplayBytes = await File.ReadAllBytesAsync(journalPath);
            var physicalRecords = ReadPhysicalJournalRecords(journalPath);

            Assert.True(raisedRetry.IsSuccess, raisedRetry.ErrorMessage);
            Assert.True(IsCreatedSideEffect(raisedRetry.Value!));
            Assert.True(afterRaisedRetryBytes.LongLength > firstBytes.LongLength);
            Assert.Equal(2, physicalRecords.Count);
            Assert.Equal(first.Value.IntakeId, physicalRecords[0].IntakeId);
            Assert.Equal(raisedRetry.Value!.IntakeId, physicalRecords[1].IntakeId);
            Assert.True(secondReplay.IsSuccess, secondReplay.ErrorMessage);
            Assert.True(IsReplay(secondReplay.Value!));
            Assert.Equal(raisedRetry.Value.IntakeId, secondReplay.Value!.IntakeId);
            Assert.Equal(afterRaisedRetryBytes, afterSecondReplayBytes);
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task CreateTrainingIntakeAsync_DifferentKeys_PersistTwoUtf8JournalRowsInAppendOrder()
    {
        var root = CreateTempRoot("training-intake-durable-two-rows");
        var journalPath = Path.Combine(root, JournalFileName);

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var first = await CreateAcceptedIntakeAsync(
                db,
                scope: "two-rows",
                tenant: "tenant:r7-b1-two-rows",
                idempotencyKey: "idempotency:r7-b1-two-rows-001",
                reason: "durable two row append first",
                requestedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(1));
            var second = await CreateAcceptedIntakeAsync(
                db,
                scope: "two-rows",
                tenant: "tenant:r7-b1-two-rows",
                idempotencyKey: "idempotency:r7-b1-two-rows-002",
                reason: "durable two row append second",
                requestedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(2));

            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.True(second.IsSuccess, second.ErrorMessage);

            var physicalRecords = ReadPhysicalJournalRecords(journalPath);
            var reloaded = await new JsonlDesignConversionLocalArtifactDb(root).ListTrainingIntakeRequestsAsync();

            Assert.True(reloaded.IsSuccess, reloaded.ErrorMessage);
            Assert.Equal(2, physicalRecords.Count);
            Assert.Equal(first.Value!.IntakeId, physicalRecords[0].IntakeId);
            Assert.Equal(second.Value!.IntakeId, physicalRecords[1].IntakeId);
            Assert.Equal(
                [first.Value.IntakeId, second.Value.IntakeId],
                reloaded.Value!.Select(record => record.IntakeId).ToArray());
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task CreateTrainingIntakeAsync_ValidZeroCandidateHandoff_PersistsAuditRowAndReplaysWithoutTraining()
    {
        var root = CreateTempRoot("training-intake-durable-zero-candidate-audit");
        var journalPath = Path.Combine(root, JournalFileName);
        var lockPath = Path.Combine(root, LockFileName);
        var handoff = BuildEmptyApprovedHandoff("zero-candidate-audit");
        var admission = BuildCreateAdmissionMetadata(
            "tenant:n9-7-zero-candidate-audit",
            "idempotency:n9-7-zero-candidate-audit");

        try
        {
            var created = await new JsonlDesignConversionLocalArtifactDb(root).CreateTrainingIntakeAsync(
                handoff,
                "requester:n9-7-durable-test",
                "durable audit intake for zero accepted candidates",
                DateTimeOffset.UnixEpoch.AddMinutes(4),
                admissionBoundaryMetadata: admission);

            Assert.True(created.IsSuccess, created.ErrorMessage);
            Assert.NotNull(created.Value);
            Assert.Equal(0, created.Value!.AcceptedCandidateCount);
            Assert.Empty(created.Value.CandidateRefs);
            Assert.Equal(string.Empty, created.Value.IntakeRef);
            Assert.True(IsCreatedSideEffect(created.Value));
            Assert.Equal("0", ReadMetadata(created.Value, "accepted_candidate_count"));
            Assert.True(File.Exists(journalPath));
            Assert.True(File.Exists(lockPath));
            Assert.Equal(0, new FileInfo(lockPath).Length);

            var afterCreateBytes = await File.ReadAllBytesAsync(journalPath);
            var physicalRecord = Assert.Single(ReadPhysicalJournalRecords(journalPath));
            Assert.Equal(created.Value.IntakeId, physicalRecord.IntakeId);
            Assert.Equal(0, physicalRecord.AcceptedCandidateCount);
            Assert.Empty(physicalRecord.CandidateRefs);
            Assert.True(IsCreatedSideEffect(physicalRecord));

            var reloaded = await new JsonlDesignConversionLocalArtifactDb(root).ListTrainingIntakeRequestsAsync();
            Assert.True(reloaded.IsSuccess, reloaded.ErrorMessage);
            var reloadedRecord = Assert.Single(reloaded.Value!);
            Assert.Equal(created.Value.IntakeId, reloadedRecord.IntakeId);
            Assert.Equal(0, reloadedRecord.AcceptedCandidateCount);
            Assert.Empty(reloadedRecord.CandidateRefs);

            var preflight = await new JsonlDesignConversionLocalArtifactDb(root).RunTrainingIntakePreflightAsync(
                created.Value.IntakeId);
            Assert.True(preflight.IsSuccess, preflight.ErrorMessage);
            Assert.Equal(created.Value.IntakeId, preflight.Value!.TrainingIntakeId);
            Assert.Equal(0, preflight.Value.AcceptedCandidateCount);
            Assert.False(preflight.Value.MayStartTraining);
            Assert.Contains("no_accepted_candidates", preflight.Value.BlockReasons);

            var replay = await new JsonlDesignConversionLocalArtifactDb(root).CreateTrainingIntakeAsync(
                handoff,
                "requester:n9-7-durable-test",
                "durable audit intake for zero accepted candidates",
                DateTimeOffset.UnixEpoch.AddMinutes(4),
                admissionBoundaryMetadata: admission);

            Assert.True(replay.IsSuccess, replay.ErrorMessage);
            Assert.NotNull(replay.Value);
            Assert.True(IsReplay(replay.Value!));
            Assert.Equal(created.Value.IntakeId, replay.Value.IntakeId);
            Assert.Equal(afterCreateBytes, await File.ReadAllBytesAsync(journalPath));
            Assert.Single(ReadPhysicalJournalRecords(journalPath));
            Assert.Equal(0, new FileInfo(lockPath).Length);
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task ListTrainingIntakeRequestsAsync_MalformedLineAfterBlank_ReturnsSafeOperationResultAndPreservesBytes()
    {
        var root = CreateTempRoot("training-intake-malformed-line");
        var journalPath = Path.Combine(root, JournalFileName);
        const string privateMarker = "PRIVATE_N9_1A_TRAINING_INTAKE_SENTINEL";
        var malformedLine = $"{{ malformed: \"{privateMarker}\"";

        try
        {
            var created = await CreateAcceptedIntakeAsync(
                new JsonlDesignConversionLocalArtifactDb(root),
                scope: "malformed-line-seed",
                tenant: "tenant:n9-1a-intake-malformed",
                idempotencyKey: "idempotency:n9-1a-intake-malformed",
                reason: "seed row before malformed intake reader line",
                requestedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(3));
            Assert.True(created.IsSuccess, created.ErrorMessage);

            var validRead = await new JsonlDesignConversionLocalArtifactDb(root).ListTrainingIntakeRequestsAsync();
            Assert.True(validRead.IsSuccess, validRead.ErrorMessage);
            var validRecord = Assert.Single(validRead.Value!);
            Assert.Equal(created.Value!.IntakeId, validRecord.IntakeId);

            await File.AppendAllTextAsync(
                journalPath,
                Environment.NewLine + malformedLine + Environment.NewLine,
                StrictUtf8);
            var bytesBeforeRead = await File.ReadAllBytesAsync(journalPath);

            var listed = await new JsonlDesignConversionLocalArtifactDb(root).ListTrainingIntakeRequestsAsync();

            Assert.False(listed.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_TRAINING_INTAKE_LINE_INVALID", listed.ErrorCode);
            Assert.Contains("line 3", listed.ErrorMessage, StringComparison.Ordinal);
            Assert.Equal(bytesBeforeRead, await File.ReadAllBytesAsync(journalPath));
            AssertNoTempArtifacts(root);

            var serializedError = JsonSerializer.Serialize(
                new { listed.ErrorCode, listed.ErrorMessage },
                DesignConversionCheckpointJsonOptions.Standard);
            var returnedErrorText = string.Join('|', listed.ErrorCode, listed.ErrorMessage, serializedError);
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
    public async Task ListTrainingIntakeRequestsAsync_LockedJournal_ReturnsSafeReadFailurePreservesBytesAndRecovers()
    {
        var root = CreateTempRoot("training-intake-locked-read");
        var journalPath = Path.Combine(root, JournalFileName);
        const string tenant = "tenant:n9-1b-intake-locked-read";
        const string idempotencyKey = "idempotency:n9-1b-intake-locked-read";
        const string privateMarker = "PRIVATE_N9_1B_TRAINING_INTAKE_LOCKED_READ_SENTINEL";
        const string expectedErrorCode = "DESIGN_CONVERSION_TRAINING_INTAKE_READ_FAILED";
        const string expectedErrorMessage = "DesignConversion training intake requests could not be read.";

        try
        {
            var created = await CreateAcceptedIntakeAsync(
                new JsonlDesignConversionLocalArtifactDb(root),
                scope: "locked-read-seed",
                tenant,
                idempotencyKey,
                reason: $"seed row before locked intake read {privateMarker}",
                requestedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(6));
            Assert.True(created.IsSuccess, created.ErrorMessage);
            var bytesBeforeRead = await File.ReadAllBytesAsync(journalPath);

            OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>> lockedRead;
            await using (var held = new FileStream(
                journalPath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None))
            {
                lockedRead = await new JsonlDesignConversionLocalArtifactDb(root).ListTrainingIntakeRequestsAsync();

                Assert.False(lockedRead.IsSuccess);
                Assert.Equal(expectedErrorCode, lockedRead.ErrorCode);
                Assert.Equal(expectedErrorMessage, lockedRead.ErrorMessage);
                AssertNoTempArtifacts(root);
            }

            Assert.Equal(bytesBeforeRead, await File.ReadAllBytesAsync(journalPath));
            AssertNoTempArtifacts(root);

            var returnedErrorText = string.Join('|', lockedRead.ErrorCode, lockedRead.ErrorMessage);
            foreach (var forbidden in new[]
            {
                root,
                journalPath,
                JournalFileName,
                TempSuffix,
                "IOException",
                "UnauthorizedAccessException",
                "SecurityException",
                "System.IO",
                tenant,
                idempotencyKey,
                privateMarker
            })
            {
                Assert.DoesNotContain(forbidden, returnedErrorText, StringComparison.OrdinalIgnoreCase);
            }

            var recovered = await new JsonlDesignConversionLocalArtifactDb(root).ListTrainingIntakeRequestsAsync();

            Assert.True(recovered.IsSuccess, recovered.ErrorMessage);
            var recoveredRecord = Assert.Single(recovered.Value!);
            Assert.Equal(created.Value!.IntakeId, recoveredRecord.IntakeId);
            Assert.Equal(tenant, ReadMetadata(recoveredRecord, "training_intake_tenant_id_ref"));
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task CreateTrainingIntakeAsync_InjectedPublicationFailureAfterTempFlush_FailsClosedPreservesBytesAndRetryAppendsOnce()
    {
        var root = CreateTempRoot("training-intake-durable-locked-destination");
        var journalPath = Path.Combine(root, JournalFileName);
        const string tenant = "tenant:r7-b1-locked";
        const string idempotencyKey = "idempotency:r7-b1-locked-002";
        const string privateMarker = "PRIVATE_R7_B1_TRAINING_INTAKE_PUBLICATION_FAILURE";

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var first = await CreateAcceptedIntakeAsync(
                setupDb,
                scope: "locked-destination",
                tenant,
                idempotencyKey: "idempotency:r7-b1-locked-001",
                reason: "seed row before locked destination",
                requestedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(10));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var originalBytes = await File.ReadAllBytesAsync(journalPath);

            var retryHandoff =
                await DesignConversionPostEvaluationEligibilityTestFixture
                    .SeedApprovedHandoffAsync(
                        setupDb,
                        "locked-destination-retry");
            var admission = BuildCreateAdmissionMetadata(
                tenant,
                idempotencyKey);
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
                    // Одноразовый отказ моделирует саму публикацию и не зависит от FileShare платформы.
                    if (Interlocked.Exchange(ref remainingFailures, 0) == 1)
                    {
                        throw new IOException(
                            $"Injected request journal publication failure: {root}|{journalPath}|{tenant}|{privateMarker}");
                    }
                });

            var lockedResult = await faultingDb.CreateTrainingIntakeAsync(
                retryHandoff,
                "requester:r7-b1-durable-test",
                $"retry row after portable publication failure {privateMarker}",
                DateTimeOffset.UnixEpoch.AddMinutes(11),
                admissionBoundaryMetadata: admission);

            AssertWriteFailed(lockedResult);
            Assert.Equal(
                DesignConversionTrainingJournalPublicationKind.TrainingIntakeRequests,
                observedKind);
            Assert.Equal(1, Volatile.Read(ref publicationCallbackCount));
            Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
            AssertNoTempArtifacts(root);

            var returnedErrorText = string.Join('|', lockedResult.ErrorCode, lockedResult.ErrorMessage);
            foreach (var forbidden in new[] { root, journalPath, tenant, idempotencyKey, privateMarker })
                Assert.DoesNotContain(forbidden, returnedErrorText, StringComparison.OrdinalIgnoreCase);

            // Повтор тем же экземпляром доказывает, что отказ освободил gate и не оставил скрытого дубля.
            var retry = await faultingDb.CreateTrainingIntakeAsync(
                    retryHandoff,
                    "requester:r7-b1-durable-test",
                    $"retry row after portable publication failure {privateMarker}",
                    DateTimeOffset.UnixEpoch.AddMinutes(11),
                    admissionBoundaryMetadata: admission)
                .WaitAsync(TimeSpan.FromSeconds(10));
            var records = ReadPhysicalJournalRecords(journalPath);

            Assert.True(retry.IsSuccess, retry.ErrorMessage);
            Assert.True(IsCreatedSideEffect(retry.Value!));
            Assert.Equal(2, records.Count);
            Assert.Equal(first.Value!.IntakeId, records[0].IntakeId);
            Assert.Equal(retry.Value!.IntakeId, records[1].IntakeId);
            Assert.Equal(1, records.Count(record => record.IntakeId == retry.Value.IntakeId));
            Assert.Equal(2, Volatile.Read(ref publicationCallbackCount));
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task CreateTrainingIntakeAsync_ExternalMutationLockHeld_CancelsWithoutLeakAndRetryReplaysWithoutDuplicate()
    {
        var root = CreateTempRoot("training-intake-durable-external-create-lock");
        var journalPath = Path.Combine(root, JournalFileName);
        var lockPath = Path.Combine(root, LockFileName);
        const string tenant = "tenant:n9-3b-external-create-lock";
        const string idempotencyKey = "idempotency:n9-3b-external-create-lock";
        const string privateMarker = "PRIVATE_N9_3B_CREATE_LOCK_SENTINEL";

        try
        {
            var seed = await CreateAcceptedIntakeAsync(
                new JsonlDesignConversionLocalArtifactDb(root),
                scope: "external-create-lock-seed",
                tenant: "tenant:n9-3b-external-create-lock-seed",
                idempotencyKey: "idempotency:n9-3b-external-create-lock-seed",
                reason: "seed row before external create mutation lock",
                requestedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(12));
            Assert.True(seed.IsSuccess, seed.ErrorMessage);
            var originalBytes = await File.ReadAllBytesAsync(journalPath);
            var handoff =
                await DesignConversionPostEvaluationEligibilityTestFixture
                    .SeedApprovedHandoffAsync(
                        new JsonlDesignConversionLocalArtifactDb(root),
                        "external-create-lock-retry");
            var admission = BuildCreateAdmissionMetadata(tenant, idempotencyKey);

            Directory.CreateDirectory(root);
            await using (var held = DesignConversionDeterministicMutationLock.AcquireForTesting(lockPath))
            {
                Assert.Equal(0, held.Stream.Length);
                using var cts = new CancellationTokenSource();
                var blockedCreate = new JsonlDesignConversionLocalArtifactDb(root).CreateTrainingIntakeAsync(
                    handoff,
                    "requester:n9-3b-durable-test",
                    "create waits while external mutation lock is held",
                    DateTimeOffset.UnixEpoch.AddMinutes(13),
                    cts.Token,
                    admission);

                var delay = Task.Delay(TimeSpan.FromMilliseconds(150));
                var completed = await Task.WhenAny(blockedCreate, delay);

                Assert.Same(delay, completed);
                Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
                var existingRecord = Assert.Single(ReadPhysicalJournalRecords(journalPath));
                Assert.Equal(seed.Value!.IntakeId, existingRecord.IntakeId);

                cts.Cancel();
                var cancelled = await blockedCreate.WaitAsync(TimeSpan.FromSeconds(10));

                Assert.False(cancelled.IsSuccess);
                Assert.Equal("CANCELLED", cancelled.ErrorCode);
                Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
                Assert.Single(ReadPhysicalJournalRecords(journalPath));

                var returnedErrorText = string.Join('|', cancelled.ErrorCode, cancelled.ErrorMessage);
                foreach (var forbidden in new[]
                {
                    root,
                    journalPath,
                    lockPath,
                    JournalFileName,
                    LockFileName,
                    tenant,
                    idempotencyKey,
                    privateMarker
                })
                {
                    Assert.DoesNotContain(forbidden, returnedErrorText, StringComparison.OrdinalIgnoreCase);
                }

                Assert.Equal(0, held.Stream.Length);
            }

            Assert.True(File.Exists(lockPath));
            Assert.Equal(0, new FileInfo(lockPath).Length);

            var retry = await new JsonlDesignConversionLocalArtifactDb(root).CreateTrainingIntakeAsync(
                handoff,
                "requester:n9-3b-durable-test",
                "create waits while external mutation lock is held",
                DateTimeOffset.UnixEpoch.AddMinutes(13),
                admissionBoundaryMetadata: admission);
            var replay = await new JsonlDesignConversionLocalArtifactDb(root).CreateTrainingIntakeAsync(
                handoff,
                "requester:n9-3b-durable-test",
                "create waits while external mutation lock is held",
                DateTimeOffset.UnixEpoch.AddMinutes(13),
                admissionBoundaryMetadata: admission);
            var records = ReadPhysicalJournalRecords(journalPath);

            Assert.True(retry.IsSuccess, retry.ErrorMessage);
            Assert.True(replay.IsSuccess, replay.ErrorMessage);
            Assert.True(IsCreatedSideEffect(retry.Value!));
            Assert.True(IsReplay(replay.Value!));
            Assert.Equal(retry.Value!.IntakeId, replay.Value!.IntakeId);
            Assert.Equal(2, records.Count);
            Assert.Equal(1, records.Count(record => record.IntakeId == retry.Value.IntakeId));
            Assert.Equal(0, new FileInfo(lockPath).Length);
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task CreateTrainingIntakeAsync_PreCancelledToken_PreservesExistingJournalBytesAndLeavesNoTemp()
    {
        var root = CreateTempRoot("training-intake-durable-cancelled");
        var journalPath = Path.Combine(root, JournalFileName);

        try
        {
            var seed = await CreateAcceptedIntakeAsync(
                new JsonlDesignConversionLocalArtifactDb(root),
                scope: "cancelled",
                tenant: "tenant:r7-b1-cancelled",
                idempotencyKey: "idempotency:r7-b1-cancelled-001",
                reason: "seed row before cancelled create",
                requestedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(20));
            Assert.True(seed.IsSuccess, seed.ErrorMessage);
            var originalBytes = await File.ReadAllBytesAsync(journalPath);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var cancelledDb = new JsonlDesignConversionLocalArtifactDb(root);
            var cancelledHandoff =
                await DesignConversionPostEvaluationEligibilityTestFixture
                    .SeedApprovedHandoffAsync(
                        cancelledDb,
                        "cancelled-second");
            var cancelled = await cancelledDb.CreateTrainingIntakeAsync(
                cancelledHandoff,
                "requester:r7-b1-durable-test",
                "pre cancelled create should not touch journal",
                DateTimeOffset.UnixEpoch.AddMinutes(21),
                cts.Token,
                BuildCreateAdmissionMetadata(
                    "tenant:r7-b1-cancelled",
                    "idempotency:r7-b1-cancelled-002"));

            Assert.False(cancelled.IsSuccess);
            Assert.Equal("CANCELLED", cancelled.ErrorCode);
            Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
            Assert.Single(ReadPhysicalJournalRecords(journalPath));
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task CreateTrainingIntakeAsync_ConcurrentSameTenantKeyPayloadAcrossInstances_AppendsOneRowAndReturnsOneReplay()
    {
        var root = CreateTempRoot("training-intake-durable-concurrent-same-key");
        var journalPath = Path.Combine(root, JournalFileName);

        try
        {
            var handoff =
                await DesignConversionPostEvaluationEligibilityTestFixture
                    .SeedApprovedHandoffAsync(
                        new JsonlDesignConversionLocalArtifactDb(root),
                        "concurrent-same-key");
            var admission = BuildCreateAdmissionMetadata(
                "tenant:r7-b1-concurrent-same",
                "idempotency:r7-b1-concurrent-same");
            var firstGateAcquired = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseFirstGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var secondWaitObserved = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var firstDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: async ct =>
                {
                    firstGateAcquired.TrySetResult(true);
                    await releaseFirstGate.Task.WaitAsync(ct);
                });
            var secondDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: waitCompletedSuccessfully =>
                {
                    secondWaitObserved.TrySetResult(waitCompletedSuccessfully);
                },
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null);
            var firstTask = firstDb.CreateTrainingIntakeAsync(
                handoff,
                "requester:r7-b1-durable-test",
                "same tenant key payload concurrent create",
                DateTimeOffset.UnixEpoch.AddMinutes(30),
                admissionBoundaryMetadata: admission);
            var results = await ObserveSecondCreateWaitsForSharedGateAndDrainAsync(
                firstTask,
                firstGateAcquired,
                releaseFirstGate,
                secondWaitObserved,
                () => secondDb.CreateTrainingIntakeAsync(
                    handoff,
                    "requester:r7-b1-durable-test",
                    "same tenant key payload concurrent create",
                    DateTimeOffset.UnixEpoch.AddMinutes(30),
                    admissionBoundaryMetadata: admission),
                "The second DB instance must observe a pending shared-gate wait while the first instance holds the gate.");
            var records = ReadPhysicalJournalRecords(journalPath);

            Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
            var firstResult = results[0].Value!;
            var secondResult = results[1].Value!;
            Assert.Equal(firstResult.IntakeId, secondResult.IntakeId);
            Assert.Equal(1, results.Count(result => IsCreatedSideEffect(result.Value!)));
            Assert.Equal(1, results.Count(result => IsReplay(result.Value!)));
            var record = Assert.Single(records);
            Assert.Equal(firstResult.IntakeId, record.IntakeId);
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task CreateTrainingIntakeAsync_DifferentNonIdempotentPayloadAcrossInstances_WaitsOnSharedRootGateAndPersistsBothRows()
    {
        var root = CreateTempRoot("training-intake-durable-deterministic-different-boundaries");
        var journalPath = Path.Combine(root, JournalFileName);

        try
        {
            var firstGateAcquired = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseFirstGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var secondWaitObserved = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var firstDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: async ct =>
                {
                    firstGateAcquired.TrySetResult(true);
                    await releaseFirstGate.Task.WaitAsync(ct);
                });
            var secondDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: waitCompletedSuccessfully =>
                {
                    secondWaitObserved.TrySetResult(waitCompletedSuccessfully);
                },
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null);
            var firstHandoff =
                await DesignConversionPostEvaluationEligibilityTestFixture
                    .SeedApprovedHandoffAsync(
                        new JsonlDesignConversionLocalArtifactDb(root),
                        "deterministic-idempotent-first");
            var secondHandoff =
                await DesignConversionPostEvaluationEligibilityTestFixture
                    .SeedApprovedHandoffAsync(
                        new JsonlDesignConversionLocalArtifactDb(root),
                        "deterministic-non-idempotent-second");
            var firstTask = firstDb.CreateTrainingIntakeAsync(
                firstHandoff,
                "requester:r7-b1-durable-test",
                "idempotent create holds the shared root journal gate",
                DateTimeOffset.UnixEpoch.AddMinutes(31),
                admissionBoundaryMetadata: BuildCreateAdmissionMetadata(
                    "tenant:r7-b1-deterministic-mixed-boundary",
                    "idempotency:r7-b1-deterministic-mixed-boundary"));
            var results = await ObserveSecondCreateWaitsForSharedGateAndDrainAsync(
                firstTask,
                firstGateAcquired,
                releaseFirstGate,
                secondWaitObserved,
                () => secondDb.CreateTrainingIntakeAsync(
                    secondHandoff,
                    "requester:r7-b1-durable-test",
                    "non idempotent create must share the root journal gate",
                    DateTimeOffset.UnixEpoch.AddMinutes(32)),
                "A non-idempotent create on another DB instance must wait on the same root journal gate.");

            Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
            var firstResult = results[0].Value!;
            var secondResult = results[1].Value!;
            Assert.NotEqual(firstResult.IntakeId, secondResult.IntakeId);

            var records = ReadPhysicalJournalRecords(journalPath);
            Assert.Equal(2, records.Count);
            Assert.Equal(1, records.Count(record => record.IntakeId == firstResult.IntakeId));
            Assert.Equal(1, records.Count(record => record.IntakeId == secondResult.IntakeId));

            var reloaded = await new JsonlDesignConversionLocalArtifactDb(root).ListTrainingIntakeRequestsAsync();
            Assert.True(reloaded.IsSuccess, reloaded.ErrorMessage);
            Assert.Equal(2, reloaded.Value!.Count);
            Assert.Equal(1, reloaded.Value.Count(record => record.IntakeId == firstResult.IntakeId));
            Assert.Equal(1, reloaded.Value.Count(record => record.IntakeId == secondResult.IntakeId));
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task CreateTrainingIntakeAsync_SameTenantKeyDifferentPayload_FailsClosedWithoutChangingJournal()
    {
        var root = CreateTempRoot("training-intake-durable-idempotency-conflict");
        var journalPath = Path.Combine(root, JournalFileName);
        const string tenant = "tenant:r7-b1-idempotency-conflict";
        const string idempotencyKey = "idempotency:r7-b1-idempotency-conflict";
        const string privateMarker = "PRIVATE_R7_B1_IDEMPOTENCY_CONFLICT_MARKER";

        try
        {
            var admission = BuildCreateAdmissionMetadata(tenant, idempotencyKey);
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var originalHandoff =
                await DesignConversionPostEvaluationEligibilityTestFixture
                    .SeedApprovedHandoffAsync(
                        db,
                        "idempotency-conflict-original");
            var differentHandoff =
                await DesignConversionPostEvaluationEligibilityTestFixture
                    .SeedApprovedHandoffAsync(
                        db,
                        "idempotency-conflict-different-handoff");
            var created = await db.CreateTrainingIntakeAsync(
                originalHandoff,
                "requester:r7-b1-durable-test",
                "original idempotent payload",
                DateTimeOffset.UnixEpoch.AddMinutes(35),
                admissionBoundaryMetadata: admission);
            Assert.True(created.IsSuccess, created.ErrorMessage);
            var originalBytes = await File.ReadAllBytesAsync(journalPath);

            var conflict = await db.CreateTrainingIntakeAsync(
                differentHandoff,
                "requester:r7-b1-durable-test",
                $"different payload containing {privateMarker}",
                DateTimeOffset.UnixEpoch.AddMinutes(36),
                admissionBoundaryMetadata: admission);

            Assert.False(conflict.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_TRAINING_INTAKE_IDEMPOTENCY_REPLAY_CONFLICT",
                conflict.ErrorCode);
            Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
            var record = Assert.Single(ReadPhysicalJournalRecords(journalPath));
            Assert.Equal(created.Value!.IntakeId, record.IntakeId);
            AssertNoTempArtifacts(root);

            var returnedErrorText = string.Join('|', conflict.ErrorCode, conflict.ErrorMessage);
            foreach (var forbidden in new[]
            {
                root,
                journalPath,
                JournalFileName,
                TempSuffix,
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
    public async Task CreateTrainingIntakeAsync_ConcurrentDifferentKeysAcrossInstances_StressOnlyRetainsEveryPhysicalRowExactlyOnce()
    {
        var root = CreateTempRoot("training-intake-durable-concurrent-different-keys");
        var journalPath = Path.Combine(root, JournalFileName);
        const int createCount = 8;

        try
        {
            var handoffs =
                new DesignConversionApprovedDpoHandoffArtifact[createCount];
            for (var index = 0; index < createCount; index++)
            {
                handoffs[index] =
                    await DesignConversionPostEvaluationEligibilityTestFixture
                        .SeedApprovedHandoffAsync(
                            new JsonlDesignConversionLocalArtifactDb(root),
                            $"concurrent-different-{index:00}");
            }

            using var ready = new CountdownEvent(createCount);
            var go = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var tasks = Enumerable.Range(0, createCount)
                .Select(index => StartTogether(
                    ready,
                    go,
                    () => new JsonlDesignConversionLocalArtifactDb(root)
                        .CreateTrainingIntakeAsync(
                            handoffs[index],
                            "requester:r7-b1-durable-test",
                            $"different key concurrent create {index:00}",
                            DateTimeOffset.UnixEpoch.AddMinutes(40 + index),
                            admissionBoundaryMetadata:
                                BuildCreateAdmissionMetadata(
                                    "tenant:r7-b1-concurrent-different",
                                    $"idempotency:r7-b1-concurrent-different-{index:00}"))))
                .ToArray();
            var allTasksReady = false;
            try
            {
                allTasksReady = ready.Wait(TimeSpan.FromSeconds(10));
            }
            finally
            {
                go.TrySetResult(true);
            }

            var results = await Task.WhenAll(tasks);
            Assert.True(allTasksReady, "Concurrent create tasks did not reach the start barrier.");
            Assert.All(results, result =>
            {
                Assert.True(result.IsSuccess, result.ErrorMessage);
                Assert.NotNull(result.Value);
            });
            var records = ReadPhysicalJournalRecords(journalPath);
            var resultIds = results.Select(result => result.Value!.IntakeId).ToArray();
            var physicalIds = records.Select(record => record.IntakeId).ToArray();

            Assert.All(results, result => Assert.True(IsCreatedSideEffect(result.Value!)));
            Assert.Equal(createCount, records.Count);
            Assert.Equal(createCount, resultIds.Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(createCount, physicalIds.Distinct(StringComparer.Ordinal).Count());
            Assert.Empty(resultIds.Except(physicalIds, StringComparer.Ordinal));
            Assert.Empty(physicalIds.Except(resultIds, StringComparer.Ordinal));
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RunTrainingIntakePreflightAsync_ExactSemanticDuplicateRowsCoalesceWithoutRewritingJournalAndExposeHashOnlyEvidence()
    {
        var root = CreateTempRoot("training-intake-semantic-duplicate-preflight");
        var journalPath = Path.Combine(root, JournalFileName);
        const string tenant = "tenant:w080-l2-semantic-duplicate-preflight";
        const string idempotencyKey = "idempotency:w080-l2-semantic-duplicate-preflight";
        const string persistedPrivateValue = "audit=[C:/Users/arbitrary-user/source.design]";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var handoff =
                await DesignConversionPostEvaluationEligibilityTestFixture
                    .SeedApprovedHandoffAsync(
                        db,
                        "semantic-duplicate-preflight");
            var created = await db.CreateTrainingIntakeAsync(
                handoff,
                "requester:w080-l2-semantic-duplicate-preflight",
                "seed semantic duplicate preflight authority",
                DateTimeOffset.UnixEpoch.AddMinutes(80),
                admissionBoundaryMetadata: BuildCreateAdmissionMetadata(tenant, idempotencyKey));
            Assert.True(created.IsSuccess, created.ErrorMessage);

            var durable = Assert.Single(ReadPhysicalJournalRecords(journalPath)) with
            {
                RequestReason = persistedPrivateValue
            };
            var equivalent = WithDifferentDerivedCreateReplayMetadata(durable);
            await WritePhysicalJournalRecordsAsync(journalPath, durable, equivalent);
            var beforePreflightBytes = await File.ReadAllBytesAsync(journalPath);

            var first = await db.RunTrainingIntakePreflightAsync(durable.IntakeId);
            var second = await new JsonlDesignConversionLocalArtifactDb(root)
                .RunTrainingIntakePreflightAsync(durable.IntakeId);
            var afterPreflightBytes = await File.ReadAllBytesAsync(journalPath);
            var listed = await db.ListTrainingIntakeRequestsAsync();

            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.True(second.IsSuccess, second.ErrorMessage);
            Assert.Equal(first.Value!.PreflightId, second.Value!.PreflightId);
            Assert.False(first.Value.MayStartTraining);
            Assert.Contains("raw_or_private_marker_detected", first.Value.BlockReasons);
            Assert.Equal(beforePreflightBytes, afterPreflightBytes);
            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            Assert.Equal(2, listed.Value!.Count);
            Assert.Equal(2, ReadPhysicalJournalRecords(journalPath).Count);

            var evidence = first.Value.BoundaryMetadata;
            Assert.Equal(
                "design-conversion-training-intake-semantic-fingerprint-v1",
                evidence["training_intake_semantic_duplicate_fingerprint_policy_version"]);
            Assert.Equal(
                "2",
                evidence["training_intake_semantic_duplicate_physical_row_count"]);
            Assert.Equal(
                bool.TrueString,
                evidence["training_intake_exact_semantic_duplicates_coalesced"]);
            var fingerprint = evidence["training_intake_semantic_duplicate_fingerprint"];
            Assert.StartsWith("sha256:", fingerprint);
            Assert.Equal(71, fingerprint.Length);

            var projected = JsonSerializer.Serialize(first.Value, CompactJsonOptions);
            foreach (var rawFingerprintInput in new[]
            {
                persistedPrivateValue,
                durable.RequestedByRef,
                tenant,
                idempotencyKey
            })
            {
                Assert.DoesNotContain(
                    rawFingerprintInput,
                    projected,
                    StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Theory]
    [InlineData("requested_by_ref")]
    [InlineData("request_reason")]
    [InlineData("requested_at_utc")]
    [InlineData("tenant_ref")]
    [InlineData("candidate_custody_hash")]
    [InlineData("supported_schema")]
    [InlineData("candidate_order")]
    [InlineData("list_order")]
    public async Task RunTrainingIntakePreflightAsync_SameIdWithDifferentAuthorityFieldRemainsAmbiguous(
        string authorityMutation)
    {
        var root = CreateTempRoot($"training-intake-semantic-conflict-{authorityMutation}");
        var journalPath = Path.Combine(root, JournalFileName);

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var created = await CreateAcceptedIntakeAsync(
                db,
                scope: $"semantic-conflict-{authorityMutation}",
                tenant: $"tenant:w080-l2-semantic-conflict-{authorityMutation}",
                idempotencyKey: $"idempotency:w080-l2-semantic-conflict-{authorityMutation}",
                reason: $"semantic conflict authority {authorityMutation}",
                requestedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(81));
            Assert.True(created.IsSuccess, created.ErrorMessage);

            var durable = Assert.Single(ReadPhysicalJournalRecords(journalPath));
            var pair = BuildAuthorityConflictPair(durable, authorityMutation);
            await WritePhysicalJournalRecordsAsync(journalPath, pair.First, pair.Second);
            var beforePreflightBytes = await File.ReadAllBytesAsync(journalPath);

            var preflight = await db.RunTrainingIntakePreflightAsync(durable.IntakeId);
            var afterPreflightBytes = await File.ReadAllBytesAsync(journalPath);
            var listed = await db.ListTrainingIntakeRequestsAsync();

            Assert.False(preflight.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_TRAINING_INTAKE_PREFLIGHT_AMBIGUOUS",
                preflight.ErrorCode);
            Assert.Equal(beforePreflightBytes, afterPreflightBytes);
            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            Assert.Equal(2, listed.Value!.Count);
            Assert.Equal(2, ReadPhysicalJournalRecords(journalPath).Count);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task CreateTrainingIntakeAsync_ReplayWithExactSemanticDuplicateRowsReturnsOneReplayWithoutAppend()
    {
        var root = CreateTempRoot("training-intake-semantic-duplicate-create-replay");
        var journalPath = Path.Combine(root, JournalFileName);
        const string tenant = "tenant:w080-l2-semantic-duplicate-create-replay";
        const string idempotencyKey = "idempotency:w080-l2-semantic-duplicate-create-replay";
        const string reason = "exact semantic duplicate create replay";
        var requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(82);

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var handoff =
                await DesignConversionPostEvaluationEligibilityTestFixture
                    .SeedApprovedHandoffAsync(
                        db,
                        "semantic-duplicate-create-replay");
            var admission = BuildCreateAdmissionMetadata(tenant, idempotencyKey);
            var created = await db.CreateTrainingIntakeAsync(
                handoff,
                "requester:w080-l2-semantic-duplicate-create-replay",
                reason,
                requestedAtUtc,
                admissionBoundaryMetadata: admission);
            Assert.True(created.IsSuccess, created.ErrorMessage);

            var durable = Assert.Single(ReadPhysicalJournalRecords(journalPath));
            await WritePhysicalJournalRecordsAsync(
                journalPath,
                durable,
                WithDifferentDerivedCreateReplayMetadata(durable));
            var beforeReplayBytes = await File.ReadAllBytesAsync(journalPath);

            var replay = await new JsonlDesignConversionLocalArtifactDb(root)
                .CreateTrainingIntakeAsync(
                    handoff,
                    "requester:w080-l2-semantic-duplicate-create-replay",
                    reason,
                    requestedAtUtc,
                    admissionBoundaryMetadata: admission);
            var afterReplayBytes = await File.ReadAllBytesAsync(journalPath);

            Assert.True(replay.IsSuccess, replay.ErrorMessage);
            Assert.True(IsReplay(replay.Value!));
            Assert.Equal(durable.IntakeId, replay.Value!.IntakeId);
            Assert.Equal(beforeReplayBytes, afterReplayBytes);
            Assert.Equal(2, ReadPhysicalJournalRecords(journalPath).Count);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task CreateTrainingIntakeAsync_SameIdempotencyBoundaryContainingAnyConflictingPayloadFailsClosedWithoutAppend()
    {
        var root = CreateTempRoot("training-intake-semantic-conflicting-create-replay");
        var journalPath = Path.Combine(root, JournalFileName);
        const string tenant = "tenant:w080-l2-semantic-conflicting-create-replay";
        const string idempotencyKey = "idempotency:w080-l2-semantic-conflicting-create-replay";
        const string reason = "semantic conflicting create replay baseline";
        const string conflictingReason = "semantic conflicting authority reason marker";
        var requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(83);

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var handoff =
                await DesignConversionPostEvaluationEligibilityTestFixture
                    .SeedApprovedHandoffAsync(
                        db,
                        "semantic-conflicting-create-replay");
            var admission = BuildCreateAdmissionMetadata(tenant, idempotencyKey);
            var created = await db.CreateTrainingIntakeAsync(
                handoff,
                "requester:w080-l2-semantic-conflicting-create-replay",
                reason,
                requestedAtUtc,
                admissionBoundaryMetadata: admission);
            Assert.True(created.IsSuccess, created.ErrorMessage);

            var durable = Assert.Single(ReadPhysicalJournalRecords(journalPath));
            var equivalent = WithDifferentDerivedCreateReplayMetadata(durable);
            var conflictingFingerprint = durable with
            {
                RequestReason = conflictingReason
            };
            var conflictingId = equivalent with
            {
                IntakeId = $"{durable.IntakeId}-conflicting-id"
            };
            await WritePhysicalJournalRecordsAsync(
                journalPath,
                durable,
                equivalent,
                conflictingFingerprint,
                conflictingId);
            var beforeReplayBytes = await File.ReadAllBytesAsync(journalPath);

            var replay = await new JsonlDesignConversionLocalArtifactDb(root)
                .CreateTrainingIntakeAsync(
                    handoff,
                    "requester:w080-l2-semantic-conflicting-create-replay",
                    reason,
                    requestedAtUtc,
                    admissionBoundaryMetadata: admission);
            var afterReplayBytes = await File.ReadAllBytesAsync(journalPath);

            Assert.False(replay.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_TRAINING_INTAKE_IDEMPOTENCY_REPLAY_CONFLICT",
                replay.ErrorCode);
            Assert.Equal(beforeReplayBytes, afterReplayBytes);
            Assert.Equal(4, ReadPhysicalJournalRecords(journalPath).Count);
            var returnedErrorText = string.Join('|', replay.ErrorCode, replay.ErrorMessage);
            foreach (var privateInput in new[]
            {
                tenant,
                idempotencyKey,
                conflictingReason
            })
            {
                Assert.DoesNotContain(
                    privateInput,
                    returnedErrorText,
                    StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task CreateTrainingIntakeAsync_WriteFailure_ReturnsStableSafeErrorWithoutPathTempExceptionTenantKeyOrPrivateMarker()
    {
        var root = CreateTempRoot("training-intake-durable-leak-scan");
        var journalPath = Path.Combine(root, JournalFileName);
        const string tenant = "tenant:r7-b1-leak-scan";
        const string idempotencyKey = "idempotency:r7-b1-leak-scan";
        const string privateMarker = "PRIVATE_R7_B1_TRAINING_INTAKE_DURABILITY_MARKER";

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var seed = await CreateAcceptedIntakeAsync(
                setupDb,
                scope: "leak-scan-seed",
                tenant,
                idempotencyKey: "idempotency:r7-b1-leak-scan-seed",
                reason: "seed row before leak scan",
                requestedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(50));
            Assert.True(seed.IsSuccess, seed.ErrorMessage);
            var originalBytes = await File.ReadAllBytesAsync(journalPath);
            var handoff =
                await DesignConversionPostEvaluationEligibilityTestFixture
                    .SeedApprovedHandoffAsync(
                        setupDb,
                        "leak-scan-failure");
            var remainingFailures = 1;
            var publicationCallbackCount = 0;
            DesignConversionTrainingJournalPublicationKind? observedKind = null;
            var db = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                trainingJournalBeforeAtomicPublishCallbackForTesting: kind =>
                {
                    observedKind = kind;
                    Interlocked.Increment(ref publicationCallbackCount);
                    // Маркер намеренно содержит приватные данные: наружу всё равно должна выйти только стабильная ошибка.
                    if (Interlocked.Exchange(ref remainingFailures, 0) == 1)
                    {
                        throw new IOException(
                            $"Injected request journal failure: {root}|{journalPath}|{tenant}|{idempotencyKey}|{privateMarker}");
                    }
                });
            var result = await db.CreateTrainingIntakeAsync(
                handoff,
                "requester:r7-b1-durable-test",
                $"leak scan failure should hide marker {privateMarker}",
                DateTimeOffset.UnixEpoch.AddMinutes(51),
                admissionBoundaryMetadata: BuildCreateAdmissionMetadata(tenant, idempotencyKey));

            AssertWriteFailed(result);
            Assert.Equal(
                DesignConversionTrainingJournalPublicationKind.TrainingIntakeRequests,
                observedKind);
            Assert.Equal(1, Volatile.Read(ref publicationCallbackCount));
            var returnedErrorText = string.Join('|', result.ErrorCode, result.ErrorMessage);
            foreach (var forbidden in new[]
            {
                root,
                journalPath,
                JournalFileName,
                TempSuffix,
                "IOException",
                "UnauthorizedAccessException",
                "SecurityException",
                "System.IO",
                tenant,
                idempotencyKey,
                privateMarker
            })
            {
                Assert.DoesNotContain(forbidden, returnedErrorText, StringComparison.OrdinalIgnoreCase);
            }

            Assert.Equal(originalBytes, await File.ReadAllBytesAsync(journalPath));
            Assert.Single(ReadPhysicalJournalRecords(journalPath));
            AssertNoTempArtifacts(root);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    private static async Task<OperationResult<DesignConversionTrainingIntakeRecord>> CreateAcceptedIntakeAsync(
        JsonlDesignConversionLocalArtifactDb db,
        string scope,
        string tenant,
        string idempotencyKey,
        string reason,
        DateTimeOffset requestedAtUtc)
    {
        var handoff =
            await DesignConversionPostEvaluationEligibilityTestFixture
                .SeedApprovedHandoffAsync(db, scope);
        return await db.CreateTrainingIntakeAsync(
            handoff,
            "requester:r7-b1-durable-test",
            reason,
            requestedAtUtc,
            admissionBoundaryMetadata: BuildCreateAdmissionMetadata(tenant, idempotencyKey));
    }

    private static async Task<OperationResult<DesignConversionTrainingIntakeRecord>> StartTogether(
        CountdownEvent ready,
        TaskCompletionSource<bool> go,
        Func<Task<OperationResult<DesignConversionTrainingIntakeRecord>>> action)
    {
        ready.Signal();
        await go.Task.WaitAsync(TimeSpan.FromSeconds(10));
        return await action();
    }

    private static async Task<OperationResult<DesignConversionTrainingIntakeRecord>[]> ObserveSecondCreateWaitsForSharedGateAndDrainAsync(
        Task<OperationResult<DesignConversionTrainingIntakeRecord>> firstTask,
        TaskCompletionSource<bool> firstGateAcquired,
        TaskCompletionSource<bool> releaseFirstGate,
        TaskCompletionSource<bool> secondWaitObserved,
        Func<Task<OperationResult<DesignConversionTrainingIntakeRecord>>> startSecond,
        string assertionMessage)
    {
        Task<OperationResult<DesignConversionTrainingIntakeRecord>>? secondTask = null;
        OperationResult<DesignConversionTrainingIntakeRecord>[]? results = null;
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

    private static IReadOnlyDictionary<string, string> BuildCreateAdmissionMetadata(
        string tenant,
        string idempotencyKey)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] = tenant,
            ["training_intake_idempotency_key_ref"] = idempotencyKey
        };

    private static DesignConversionTrainingIntakeRecord WithDifferentDerivedCreateReplayMetadata(
        DesignConversionTrainingIntakeRecord record)
    {
        var metadata = new Dictionary<string, string>(record.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["training_intake_admission_boundary_scope"] = "derived-scope-ignored-by-semantic-fingerprint",
            ["training_intake_create_replay_cache_boundary"] = "derived-cache-boundary-ignored-by-semantic-fingerprint",
            ["training_intake_replay_cache_checked"] = bool.FalseString,
            ["training_intake_idempotency_replay_hit"] = bool.TrueString,
            ["training_intake_idempotency_created_new_side_effect"] = bool.FalseString,
            ["training_intake_idempotent_replay_result"] = bool.TrueString,
            ["training_intake_idempotency_side_effect_boundary"] = "derived-side-effect-boundary-ignored-by-semantic-fingerprint"
        };
        return record with { BoundaryMetadata = metadata };
    }

    private static (DesignConversionTrainingIntakeRecord First, DesignConversionTrainingIntakeRecord Second)
        BuildAuthorityConflictPair(
            DesignConversionTrainingIntakeRecord record,
            string authorityMutation)
    {
        switch (authorityMutation)
        {
            case "requested_by_ref":
                return (record, record with
                {
                    RequestedByRef = $"{record.RequestedByRef}-conflict"
                });
            case "request_reason":
                return (record, record with
                {
                    RequestReason = $"{record.RequestReason}-conflict"
                });
            case "requested_at_utc":
                return (record, record with
                {
                    RequestedAtUtc = record.RequestedAtUtc.AddTicks(1)
                });
            case "tenant_ref":
                return (record, record with
                {
                    BoundaryMetadata = WithMetadataValue(
                        record.BoundaryMetadata,
                        "training_intake_tenant_id_ref",
                        "tenant:w080-l2-conflicting-authority")
                });
            case "candidate_custody_hash":
            {
                var candidate = Assert.Single(record.CandidateRefs);
                var candidateMetadata = WithMetadataValue(
                    candidate.BoundaryMetadata,
                    "payload_custody_training_record_binding_sha256",
                    HashRef("semantic-conflict", "candidate-custody"));
                return (record, record with
                {
                    CandidateRefs =
                    [
                        candidate with { BoundaryMetadata = candidateMetadata }
                    ]
                });
            }
            case "supported_schema":
            {
                var existingSchema = ReadMetadata(record, "schema_version");
                var alternateSchema = string.Equals(
                    existingSchema,
                    JsonlDesignConversionLocalArtifactDb.TrainingIntakeSchemaVersion,
                    StringComparison.Ordinal)
                        ? JsonlDesignConversionLocalArtifactDb.TrainingIntakeCustodySchemaVersion
                        : JsonlDesignConversionLocalArtifactDb.TrainingIntakeSchemaVersion;
                return (record, record with
                {
                    BoundaryMetadata = WithMetadataValue(
                        record.BoundaryMetadata,
                        "schema_version",
                        alternateSchema)
                });
            }
            case "candidate_order":
            {
                var firstCandidate = Assert.Single(record.CandidateRefs);
                var secondCandidate = firstCandidate with
                {
                    CandidateId = $"{firstCandidate.CandidateId}-order-peer",
                    TrainingRecordId = $"{firstCandidate.TrainingRecordId}-order-peer",
                    HandoffRecordHash = HashRef("semantic-conflict", "candidate-order-peer")
                };
                var first = record with
                {
                    ApprovedCandidateCount = 2,
                    AcceptedCandidateCount = 2,
                    CandidateRefs = [firstCandidate, secondCandidate]
                };
                return (first, first with
                {
                    CandidateRefs = [secondCandidate, firstCandidate]
                });
            }
            case "list_order":
            {
                var first = record with
                {
                    Warnings = ["semantic-list-alpha", "semantic-list-beta"]
                };
                return (first, first with
                {
                    Warnings = ["semantic-list-beta", "semantic-list-alpha"]
                });
            }
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(authorityMutation),
                    authorityMutation,
                    "Unknown semantic authority mutation.");
        }
    }

    private static IReadOnlyDictionary<string, string> WithMetadataValue(
        IReadOnlyDictionary<string, string> source,
        string key,
        string value)
    {
        var metadata = new Dictionary<string, string>(source, StringComparer.Ordinal)
        {
            [key] = value
        };
        return metadata;
    }

    private static async Task WritePhysicalJournalRecordsAsync(
        string journalPath,
        params DesignConversionTrainingIntakeRecord[] records)
    {
        var text = string.Join(
            Environment.NewLine,
            records.Select(record => JsonSerializer.Serialize(record, CompactJsonOptions)))
            + Environment.NewLine;
        await File.WriteAllTextAsync(journalPath, text, StrictUtf8);
    }

    private static DesignConversionApprovedDpoHandoffArtifact BuildApprovedHandoff(string scope)
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
            ApprovedByRef: "reviewer:r7-b1-durable-test",
            ApprovalReason: "approved handoff row for create training intake durability test",
            ApprovedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(1).ToString("O"),
            SourceManifestRef: $"artifact://r7-b1/{scope}/source-manifest",
            RecordHash: HashRef(scope, "record"),
            InternalUnitStateHash: HashRef(scope, "internal-unit-state"),
            SourceStructureHash: HashRef(scope, "source-structure"),
            ChosenPayloadHash: HashRef(scope, "chosen"),
            RejectedPayloadHash: HashRef(scope, "rejected"),
            TieBreakerHash: HashRef(scope, "tie-breaker"),
            ChosenHtmlRef: $"artifact://r7-b1/{scope}/chosen.html",
            ChosenCssRef: $"artifact://r7-b1/{scope}/chosen.css",
            RejectedHtmlRef: $"artifact://r7-b1/{scope}/rejected.html",
            RejectedCssRef: $"artifact://r7-b1/{scope}/rejected.css",
            DesignTreeRef: $"artifact://r7-b1/{scope}/design-tree.json",
            OriginalTreeRef: $"artifact://r7-b1/{scope}/original-tree.json",
            SourceArtifactRefs: [$"artifact://r7-b1/{scope}/source.png"],
            RenderArtifactRefs: [$"artifact://r7-b1/{scope}/render.png"],
            ComparisonArtifactRefs: [$"artifact://r7-b1/{scope}/comparison.png"],
            EvidenceRefs: ["r7-b1-training-intake-durability"],
            BoundaryMetadata: BuildHandoffBoundaryMetadata(1));

        return new DesignConversionApprovedDpoHandoffArtifact(
            ArtifactId: $"approved-dpo-handoff-{scope}",
            ArtifactRef: $"memory://r7-b1/{scope}/approved-dpo-handoff",
            SchemaVersion: JsonlDesignConversionLocalArtifactDb.ApprovedDpoHandoffSchemaVersion,
            ExportedAtUtc: DateTimeOffset.UnixEpoch,
            ApprovedCandidateCount: 1,
            Rows: [row],
            Warnings: ["training_handoff_only", "not_trained"],
            BoundaryMetadata: BuildHandoffBoundaryMetadata(1));
    }

    private static DesignConversionApprovedDpoHandoffArtifact BuildEmptyApprovedHandoff(string scope)
        => new(
            ArtifactId: $"approved-dpo-handoff-{scope}",
            ArtifactRef: $"memory://r7-b1/{scope}/approved-dpo-handoff",
            SchemaVersion: JsonlDesignConversionLocalArtifactDb.ApprovedDpoHandoffSchemaVersion,
            ExportedAtUtc: DateTimeOffset.UnixEpoch,
            ApprovedCandidateCount: 0,
            Rows: [],
            Warnings: ["no_approved_candidates", "training_handoff_only", "not_trained"],
            BoundaryMetadata: BuildHandoffBoundaryMetadata(0));

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

    private static IReadOnlyList<DesignConversionTrainingIntakeRecord> ReadPhysicalJournalRecords(string journalPath)
    {
        var bytes = File.ReadAllBytes(journalPath);
        var utf8Preamble = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: true,
            throwOnInvalidBytes: true).GetPreamble();
        Assert.False(bytes.AsSpan().StartsWith(utf8Preamble), "Training intake journal must not have a UTF-8 BOM.");

        var text = StrictUtf8.GetString(bytes);
        Assert.DoesNotContain("\uFFFD", text, StringComparison.Ordinal);

        var segments = text.Split(Environment.NewLine, StringSplitOptions.None);
        Assert.True(segments.Length >= 2, "Training intake journal must end with exactly one platform newline.");
        Assert.Empty(segments[^1]);
        Assert.All(segments[..^1], segment => Assert.NotEmpty(segment));

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

    private static bool IsCreatedSideEffect(DesignConversionTrainingIntakeRecord record)
        => string.Equals(
               ReadMetadata(record, "training_intake_idempotency_created_new_side_effect"),
               bool.TrueString,
               StringComparison.Ordinal)
           && string.Equals(
               ReadMetadata(record, "training_intake_idempotency_replay_hit"),
               bool.FalseString,
               StringComparison.Ordinal);

    private static bool IsReplay(DesignConversionTrainingIntakeRecord record)
        => string.Equals(
               ReadMetadata(record, "training_intake_idempotency_created_new_side_effect"),
               bool.FalseString,
               StringComparison.Ordinal)
           && string.Equals(
               ReadMetadata(record, "training_intake_idempotency_replay_hit"),
               bool.TrueString,
               StringComparison.Ordinal);

    private static string ReadMetadata(DesignConversionTrainingIntakeRecord record, string key)
        => record.BoundaryMetadata.TryGetValue(key, out var value)
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
        => Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), suffix);

    private static void DeleteDirectoryIfExists(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }
}
