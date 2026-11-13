// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using DesignConversionDomain;

public sealed class DesignConversionLocalArtifactDbAuthorityDurabilityTests
{
    private const string AuthorityMutationLockFileName = "conversion-results.jsonl.mutation.lock";

    [Fact]
    public async Task AppendRecordAsync_FirstAndSecondAuthorityCommitsAreAtomicJsonlAndFreshLoadStable()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-authority-durable");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var first = await db.AppendRecordAsync(BuildRecord(
                recordId: "authority-durable-record-a",
                jobId: "authority-durable-job-a",
                requestId: "authority-durable-request-a",
                createdAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(1)));
            var second = await db.AppendRecordAsync(BuildRecord(
                recordId: "authority-durable-record-b",
                jobId: "authority-durable-job-b",
                requestId: "authority-durable-request-b",
                createdAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(2)));

            var fresh = new JsonlDesignConversionLocalArtifactDb(root);
            var listed = await fresh.ListRecordsAsync();
            var lines = await File.ReadAllLinesAsync(recordsPath);
            var index = await ReadIndexAsync(indexPath);
            var authorityBytes = await File.ReadAllBytesAsync(recordsPath);
            var nonEmptyLines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();

            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.True(second.IsSuccess, second.ErrorMessage);
            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            Assert.Equal(
                ["authority-durable-record-a", "authority-durable-record-b"],
                listed.Value!.Select(record => record.RecordId).ToArray());
            Assert.Equal(
                ["authority-durable-record-a", "authority-durable-record-b"],
                nonEmptyLines.Select(line => DeserializeRecord(line).RecordId).ToArray());
            Assert.Equal(
                1,
                nonEmptyLines.Count(line => string.Equals(
                    DeserializeRecord(line).RecordId,
                    "authority-durable-record-a",
                    StringComparison.Ordinal)));
            Assert.Equal(
                1,
                nonEmptyLines.Count(line => string.Equals(
                    DeserializeRecord(line).RecordId,
                    "authority-durable-record-b",
                    StringComparison.Ordinal)));
            Assert.NotNull(index);
            Assert.Equal(2, index!.RecordCount);
            Assert.Equal(2, index.RecordLocators.Count);
            Assert.Equal(authorityBytes.LongLength, index.RecordsFileLength);
            AssertAuthorityLocator(authorityBytes, index.RecordLocators[0], "authority-durable-record-a", 0, 1);
            AssertAuthorityLocator(
                authorityBytes,
                index.RecordLocators[1],
                "authority-durable-record-b",
                index.RecordLocators[0].ByteLength,
                2);
            Assert.Empty(EnumerateAuthorityCommitTempFiles(root));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_ExternalAuthorityMutationLockHeld_WaitsThenCommitsOnceAndFreshReadSeesRecord()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-authority-external-lock");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var lockPath = Path.Combine(root, AuthorityMutationLockFileName);
        const string blockedRecordId = "authority-durable-external-lock-record";

        try
        {
            Directory.CreateDirectory(root);
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var held = DesignConversionDeterministicMutationLock.AcquireForTesting(lockPath);

            try
            {
                var append = db.AppendRecordAsync(BuildRecord(
                    recordId: blockedRecordId,
                    jobId: "authority-durable-external-lock-job",
                    requestId: "authority-durable-external-lock-request"));

                await AssertTaskRemainsPendingAsync(append, TimeSpan.FromMilliseconds(150));
                Assert.False(File.Exists(recordsPath));
                Assert.Equal(0, held.Stream.Length);

                await held.DisposeAsync();
                var result = await append.WaitAsync(TimeSpan.FromSeconds(10));
                var fresh = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();
                var physicalIds = (await File.ReadAllLinesAsync(recordsPath))
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => DeserializeRecord(line).RecordId)
                    .ToArray();

                Assert.True(result.IsSuccess, result.ErrorMessage);
                Assert.Equal(blockedRecordId, result.Value!.RecordId);
                Assert.True(fresh.IsSuccess, fresh.ErrorMessage);
                Assert.Equal([blockedRecordId], fresh.Value!.Select(record => record.RecordId).ToArray());
                Assert.Equal([blockedRecordId], physicalIds);
                Assert.Equal(1, physicalIds.Count(id => string.Equals(id, blockedRecordId, StringComparison.Ordinal)));
                Assert.True(File.Exists(lockPath));
                Assert.Equal(0, new FileInfo(lockPath).Length);
                Assert.Empty(EnumerateAuthorityCommitTempFiles(root));
                Assert.Empty(EnumerateIndexWriteTempFiles(root));
            }
            finally
            {
                await held.DisposeAsync();
            }
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_CancelledWhileWaitingForExternalAuthorityMutationLockDoesNotMutateAuthority()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-authority-lock-cancelled");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        var lockPath = Path.Combine(root, AuthorityMutationLockFileName);
        const string cancelledRecordId = "authority-durable-external-lock-cancelled-record";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var seed = await db.AppendRecordAsync(BuildRecord(
                recordId: "authority-durable-external-lock-cancel-seed",
                jobId: "authority-durable-external-lock-cancel-seed-job",
                requestId: "authority-durable-external-lock-cancel-seed-request"));
            Assert.True(seed.IsSuccess, seed.ErrorMessage);

            var originalAuthorityBytes = await File.ReadAllBytesAsync(recordsPath);
            var originalIndexBytes = await File.ReadAllBytesAsync(indexPath);
            var held = DesignConversionDeterministicMutationLock.AcquireForTesting(lockPath);

            try
            {
                using var cancellation = new CancellationTokenSource();
                var append = new JsonlDesignConversionLocalArtifactDb(root).AppendRecordAsync(
                    BuildRecord(
                        recordId: cancelledRecordId,
                        jobId: "authority-durable-external-lock-cancelled-job",
                        requestId: "authority-durable-external-lock-cancelled-request"),
                    cancellation.Token);

                await AssertTaskRemainsPendingAsync(append, TimeSpan.FromMilliseconds(150));
                Assert.Equal(originalAuthorityBytes, await File.ReadAllBytesAsync(recordsPath));
                Assert.Equal(originalIndexBytes, await File.ReadAllBytesAsync(indexPath));

                cancellation.Cancel();
                var cancelled = await append.WaitAsync(TimeSpan.FromSeconds(10));
                var afterCancelAuthorityBytes = await File.ReadAllBytesAsync(recordsPath);
                var afterCancelIndexBytes = await File.ReadAllBytesAsync(indexPath);
                var fresh = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();

                Assert.False(cancelled.IsSuccess);
                Assert.Null(cancelled.Value);
                Assert.Equal("CANCELLED", cancelled.ErrorCode);
                Assert.Equal(originalAuthorityBytes, afterCancelAuthorityBytes);
                Assert.Equal(originalIndexBytes, afterCancelIndexBytes);
                Assert.True(fresh.IsSuccess, fresh.ErrorMessage);
                Assert.Equal([seed.Value!.RecordId], fresh.Value!.Select(record => record.RecordId).ToArray());
                Assert.DoesNotContain(
                    fresh.Value!,
                    record => string.Equals(record.RecordId, cancelledRecordId, StringComparison.Ordinal));
                Assert.Equal(0, held.Stream.Length);
                Assert.Empty(EnumerateAuthorityCommitTempFiles(root));
                Assert.Empty(EnumerateIndexWriteTempFiles(root));
            }
            finally
            {
                await held.DisposeAsync();
            }
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Constructor_RejectsNonPositiveRecordsAuthorityMaxBytes()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-authority-quota-validation");

        var zero = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new JsonlDesignConversionLocalArtifactDb(root, recordsAuthorityMaxBytes: 0));
        var negative = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new JsonlDesignConversionLocalArtifactDb(root, recordsAuthorityMaxBytes: -1));
        var defaultDb = new JsonlDesignConversionLocalArtifactDb(root);
        var customDb = new JsonlDesignConversionLocalArtifactDb(root, recordsAuthorityMaxBytes: 17);

        Assert.Equal("recordsAuthorityMaxBytes", zero.ParamName);
        Assert.Equal("recordsAuthorityMaxBytes", negative.ParamName);
        Assert.Equal(
            1L * 1024 * 1024 * 1024,
            JsonlDesignConversionLocalArtifactDb.DefaultRecordsAuthorityMaxBytes);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.DefaultRecordsAuthorityMaxBytes, defaultDb.RecordsAuthorityMaxBytes);
        Assert.Equal(17, customDb.RecordsAuthorityMaxBytes);
    }

    [Fact]
    public async Task AppendRecordAsync_AuthorityQuotaFailsClosedAndReplayDoesNotMutate()
    {
        var baseRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var measureRoot = Path.Combine(baseRoot, "measure");
        var root = Path.Combine(baseRoot, "exact-cap");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const string privateMarker = "PRIVATE_MARKER_N25_QUOTA_C:/Users/lubao/private/conversion-results.jsonl";
        var firstRecord = BuildRecord(
            recordId: "authority-durable-quota-first",
            jobId: "authority-durable-quota-first-job",
            requestId: "authority-durable-quota-first-request",
            createdAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(1));

        try
        {
            var measureDb = new JsonlDesignConversionLocalArtifactDb(measureRoot);
            var measuredFirst = await measureDb.AppendRecordAsync(firstRecord);
            var exactCap = (await File.ReadAllBytesAsync(Path.Combine(measureRoot, "conversion-results.jsonl"))).LongLength;
            Assert.True(measuredFirst.IsSuccess, measuredFirst.ErrorMessage);
            Assert.True(exactCap > 1);

            var exactDb = new JsonlDesignConversionLocalArtifactDb(root, recordsAuthorityMaxBytes: exactCap);
            var first = await exactDb.AppendRecordAsync(firstRecord);
            var firstAuthorityBytes = await File.ReadAllBytesAsync(recordsPath);
            var firstIndexBytes = await File.ReadAllBytesAsync(indexPath);
            var firstIndex = await ReadIndexAsync(indexPath);

            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.Equal(exactCap, exactDb.RecordsAuthorityMaxBytes);
            Assert.Equal(exactCap, firstAuthorityBytes.LongLength);
            Assert.NotNull(firstIndex);
            AssertAuthorityAndIndexChain(firstAuthorityBytes, firstIndex!, [firstRecord.RecordId]);
            Assert.Empty(EnumerateAuthorityCommitTempFiles(root));
            Assert.Empty(EnumerateIndexWriteTempFiles(root));

            var callbackCalls = 0;
            var cappedDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                authorityAppendBeforeOpenCallbackForTesting: _ =>
                {
                    callbackCalls++;
                    return Task.CompletedTask;
                },
                recordsAuthorityMaxBytes: exactCap);
            var overCap = await cappedDb.AppendRecordAsync(BuildRecord(
                recordId: "authority-durable-quota-second",
                privateMarker: privateMarker,
                jobId: "authority-durable-quota-second-job",
                requestId: "authority-durable-quota-second-request",
                createdAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(2)));
            var afterFailureAuthorityBytes = await File.ReadAllBytesAsync(recordsPath);
            var afterFailureIndexBytes = await File.ReadAllBytesAsync(indexPath);
            var fresh = await new JsonlDesignConversionLocalArtifactDb(root, recordsAuthorityMaxBytes: exactCap)
                .ListRecordsAsync();
            var failureText = $"{overCap.ErrorCode} {overCap.ErrorMessage}";

            Assert.False(overCap.IsSuccess);
            Assert.Null(overCap.Value);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbAuthorityQuotaExceededErrorCode, overCap.ErrorCode);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbAuthorityQuotaExceededSafeMessage, overCap.ErrorMessage);
            Assert.Equal(0, callbackCalls);
            AssertSafeFailureDoesNotLeak(failureText, root, recordsPath, privateMarker);
            Assert.Equal(firstAuthorityBytes, afterFailureAuthorityBytes);
            Assert.Equal(firstIndexBytes, afterFailureIndexBytes);
            Assert.True(fresh.IsSuccess, fresh.ErrorMessage);
            Assert.Equal([firstRecord.RecordId], fresh.Value!.Select(record => record.RecordId).ToArray());
            Assert.Empty(EnumerateAuthorityCommitTempFiles(root));
            Assert.Empty(EnumerateIndexWriteTempFiles(root));

            var replayUnderSmallerCap = await new JsonlDesignConversionLocalArtifactDb(
                    root,
                    recordsAuthorityMaxBytes: exactCap - 1)
                .AppendRecordAsync(firstRecord);
            var afterReplayAuthorityBytes = await File.ReadAllBytesAsync(recordsPath);
            var afterReplayIndexBytes = await File.ReadAllBytesAsync(indexPath);

            Assert.True(replayUnderSmallerCap.IsSuccess, replayUnderSmallerCap.ErrorMessage);
            Assert.Equal(firstRecord.RecordId, replayUnderSmallerCap.Value!.RecordId);
            Assert.Equal(firstAuthorityBytes, afterReplayAuthorityBytes);
            Assert.Equal(firstIndexBytes, afterReplayIndexBytes);
            Assert.Empty(EnumerateAuthorityCommitTempFiles(root));
            Assert.Empty(EnumerateIndexWriteTempFiles(root));
        }
        finally
        {
            if (Directory.Exists(baseRoot))
                Directory.Delete(baseRoot, recursive: true);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_GrowingDerivedIndexPersistsOrderedLocatorsAndCleansTemp()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-growing-index");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const int RecordCount = 240;
        var recordIds = Enumerable
            .Range(0, RecordCount)
            .Select(index => $"authority-durable-stream-index-record-{index:D4}-{new string('x', 96)}")
            .ToArray();

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            for (var index = 0; index < recordIds.Length; index++)
            {
                var appended = await db.AppendRecordAsync(BuildRecord(
                    recordId: recordIds[index],
                    jobId: $"authority-durable-stream-index-job-{index:D4}-{new string('j', 96)}",
                    requestId: $"authority-durable-stream-index-request-{index:D4}-{new string('r', 96)}",
                    createdAtUtc: DateTimeOffset.UnixEpoch.AddSeconds(index + 1)));

                Assert.True(appended.IsSuccess, appended.ErrorMessage);
            }

            var authorityBytes = await File.ReadAllBytesAsync(recordsPath);
            var indexBytes = await File.ReadAllBytesAsync(indexPath);
            var durableIndex = await ReadIndexAsync(indexPath);

            Assert.True(
                indexBytes.LongLength > 81920,
                $"Expected growing index to exceed one stream buffer; observed {indexBytes.LongLength} bytes.");
            Assert.NotNull(durableIndex);
            Assert.Equal(RecordCount, durableIndex!.RecordCount);
            Assert.Equal(RecordCount, durableIndex.RecordLocators.Count);
            Assert.Equal(recordIds, durableIndex.RecordLocators.Select(locator => locator.RecordId).ToArray());
            Assert.Equal(authorityBytes.LongLength, durableIndex.RecordsFileLength);

            var expectedOffset = 0L;
            for (var index = 0; index < durableIndex.RecordLocators.Count; index++)
            {
                Assert.Equal(expectedOffset, durableIndex.RecordLocators[index].ByteOffset);
                Assert.Equal(index + 1, durableIndex.RecordLocators[index].LineNumber);
                expectedOffset += durableIndex.RecordLocators[index].ByteLength;
            }

            Assert.Equal(authorityBytes.LongLength, expectedOffset);
            var middle = RecordCount / 2;
            AssertAuthorityLocator(authorityBytes, durableIndex.RecordLocators[0], recordIds[0], 0, 1);
            AssertAuthorityLocator(
                authorityBytes,
                durableIndex.RecordLocators[middle],
                recordIds[middle],
                durableIndex.RecordLocators[middle].ByteOffset,
                middle + 1);
            AssertAuthorityLocator(
                authorityBytes,
                durableIndex.RecordLocators[^1],
                recordIds[^1],
                durableIndex.RecordLocators[^1].ByteOffset,
                RecordCount);
            Assert.Empty(EnumerateAuthorityCommitTempFiles(root));
            Assert.Empty(EnumerateIndexWriteTempFiles(root));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_SharedWarmCacheAcrossInstancesKeepsDeferredReplayAndConflictCoherent()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-shared-warm-cache");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const int RecordCount = 96;
        var records = Enumerable
            .Range(0, RecordCount)
            .Select(index => BuildRecord(
                recordId: $"authority-durable-shared-cache-record-{index:D4}",
                jobId: $"authority-durable-shared-cache-job-{index:D4}",
                requestId: $"authority-durable-shared-cache-request-{index:D4}",
                createdAtUtc: DateTimeOffset.UnixEpoch.AddSeconds(index + 1)))
            .ToArray();
        var recordIds = records.Select(record => record.RecordId).ToArray();

        try
        {
            var firstDb = new JsonlDesignConversionLocalArtifactDb(root);
            var secondDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                derivedIndexBeforeAtomicPublishCallbackForTesting:
                    CreateOneShotDerivedIndexPublishFailure());
            var first = await firstDb.AppendRecordAsync(records[0]);
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var originalIndexBytes = await File.ReadAllBytesAsync(indexPath);
            var originalIndex = await ReadIndexAsync(indexPath);
            Assert.NotNull(originalIndex);
            Assert.Equal(1, originalIndex!.RecordCount);

            var deferred = await secondDb.AppendRecordAsync(records[1]);
            Assert.True(deferred.IsSuccess, deferred.ErrorMessage);
            Assert.Contains(
                JsonlDesignConversionLocalArtifactDb.DerivedIndexUpdateDeferredMarker,
                deferred.Value!.EvidenceRefs);
            Assert.Equal(originalIndexBytes, await File.ReadAllBytesAsync(indexPath));
            Assert.Empty(EnumerateIndexWriteTempFiles(root));
            Assert.Equal(
                recordIds.Take(2),
                (await File.ReadAllLinesAsync(recordsPath))
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => DeserializeRecord(line).RecordId));

            for (var index = 2; index < records.Length; index++)
            {
                var db = index % 2 == 0 ? firstDb : secondDb;
                var appended = await db.AppendRecordAsync(records[index]);
                Assert.True(appended.IsSuccess, appended.ErrorMessage);
            }

            var authorityBytes = await File.ReadAllBytesAsync(recordsPath);
            var durableIndex = await ReadIndexAsync(indexPath);
            var authorityRecordIds = (await File.ReadAllLinesAsync(recordsPath))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => DeserializeRecord(line).RecordId)
                .ToArray();
            var listedBySecond = await secondDb.ListRecordsAsync();

            Assert.NotNull(durableIndex);
            Assert.Equal(recordIds, authorityRecordIds);
            Assert.True(listedBySecond.IsSuccess, listedBySecond.ErrorMessage);
            Assert.Equal(recordIds, listedBySecond.Value!.Select(record => record.RecordId).ToArray());
            AssertAuthorityAndIndexChain(authorityBytes, durableIndex!, recordIds);

            var beforeReplayAuthorityBytes = await File.ReadAllBytesAsync(recordsPath);
            var replay = await firstDb.AppendRecordAsync(records[1]);
            var afterReplayAuthorityBytes = await File.ReadAllBytesAsync(recordsPath);
            var replayIndex = await ReadIndexAsync(indexPath);

            Assert.True(replay.IsSuccess, replay.ErrorMessage);
            Assert.Equal(records[1].RecordId, replay.Value!.RecordId);
            Assert.Equal(beforeReplayAuthorityBytes, afterReplayAuthorityBytes);
            Assert.NotNull(replayIndex);
            AssertAuthorityAndIndexChain(afterReplayAuthorityBytes, replayIndex!, recordIds);

            var beforeConflictAuthorityBytes = await File.ReadAllBytesAsync(recordsPath);
            var conflict = await firstDb.AppendRecordAsync(BuildRecord(
                recordId: records[1].RecordId,
                jobId: "authority-durable-shared-cache-conflict-job",
                requestId: "authority-durable-shared-cache-conflict-request",
                createdAtUtc: DateTimeOffset.UnixEpoch.AddHours(1)));
            var afterConflictAuthorityBytes = await File.ReadAllBytesAsync(recordsPath);
            var conflictIndex = await ReadIndexAsync(indexPath);

            Assert.False(conflict.IsSuccess);
            Assert.Null(conflict.Value);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbRecordIdConflictErrorCode, conflict.ErrorCode);
            Assert.Equal(beforeConflictAuthorityBytes, afterConflictAuthorityBytes);
            Assert.NotNull(conflictIndex);
            AssertAuthorityAndIndexChain(afterConflictAuthorityBytes, conflictIndex!, recordIds);
            Assert.Empty(EnumerateAuthorityCommitTempFiles(root));
            Assert.Empty(EnumerateIndexWriteTempFiles(root));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_WindowsDenyDeleteAllowWriteHandleCommitsInPlaceWithoutAuthorityTemp()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-authority-deny-delete");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const string appendedRecordId = "authority-durable-deny-delete-record";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var initial = await db.AppendRecordAsync(BuildRecord(
                recordId: "authority-durable-existing-record",
                jobId: "authority-durable-existing-job",
                requestId: "authority-durable-existing-request"));
            Assert.True(initial.IsSuccess, initial.ErrorMessage);

            var originalBytes = await File.ReadAllBytesAsync(recordsPath);
            var appended = BuildRecord(
                recordId: appendedRecordId,
                jobId: "authority-durable-deny-delete-job",
                requestId: "authority-durable-deny-delete-request");

            await using (var denyDeleteHandle = new FileStream(
                recordsPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                bufferSize: 4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                var committed = await db.AppendRecordAsync(appended);
                var bytesAfterAppend = await File.ReadAllBytesAsync(recordsPath);
                var durableIndex = await ReadIndexAsync(indexPath);
                var listed = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();

                Assert.True(committed.IsSuccess, committed.ErrorMessage);
                Assert.Equal(originalBytes, bytesAfterAppend.Take(originalBytes.Length).ToArray());
                Assert.True(listed.IsSuccess, listed.ErrorMessage);
                // Equal timestamps are API-sorted by RecordId; the authority-chain assertion below checks append order.
                Assert.Equal(
                    [appendedRecordId, "authority-durable-existing-record"],
                    listed.Value!.Select(record => record.RecordId).ToArray());
                Assert.NotNull(durableIndex);
                Assert.Equal(2, durableIndex!.RecordCount);
                Assert.Equal(2, durableIndex.RecordLocators.Count);
                Assert.Equal(originalBytes.LongLength, durableIndex.RecordLocators[1].ByteOffset);
                AssertAuthorityAndIndexChain(
                    bytesAfterAppend,
                    durableIndex,
                    ["authority-durable-existing-record", appendedRecordId]);
                Assert.Empty(EnumerateAuthorityCommitTempFiles(root));
            }
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_WindowsWriterDeniedHandleFailsSafelyAndRetryCommitsOnce()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-authority-writer-denied");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const string blockedRecordId = "authority-durable-writer-denied-record";
        const string privateMarker = "PRIVATE_MARKER_N14_4C_AUTHORITY_C:/Users/lubao/private/conversion-results.jsonl";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var initial = await db.AppendRecordAsync(BuildRecord(
                recordId: "authority-durable-writer-denied-existing",
                jobId: "authority-durable-writer-denied-existing-job",
                requestId: "authority-durable-writer-denied-existing-request"));
            Assert.True(initial.IsSuccess, initial.ErrorMessage);

            var originalBytes = await File.ReadAllBytesAsync(recordsPath);
            var originalIndexBytes = await File.ReadAllBytesAsync(indexPath);
            var blocked = BuildRecord(
                recordId: blockedRecordId,
                privateMarker: privateMarker,
                jobId: "authority-durable-writer-denied-job",
                requestId: "authority-durable-writer-denied-request");

            await using (var writerDeniedHandle = new FileStream(
                recordsPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                var denied = await db.AppendRecordAsync(blocked);
                var bytesAfterDeniedAppend = await File.ReadAllBytesAsync(recordsPath);
                var indexAfterDeniedAppend = await File.ReadAllBytesAsync(indexPath);
                var deniedText = System.Text.Json.JsonSerializer.Serialize(
                    denied,
                    DesignConversionCheckpointJsonOptions.Standard);

                Assert.False(denied.IsSuccess);
                Assert.Null(denied.Value);
                Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedErrorCode, denied.ErrorCode);
                Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedSafeMessage, denied.ErrorMessage);
                Assert.Equal(originalBytes, bytesAfterDeniedAppend);
                Assert.Equal(originalIndexBytes, indexAfterDeniedAppend);
                Assert.DoesNotContain(blockedRecordId, Encoding.UTF8.GetString(bytesAfterDeniedAppend), StringComparison.Ordinal);
                AssertSafeFailureDoesNotLeak(deniedText, root, recordsPath, privateMarker);
                Assert.Empty(EnumerateAuthorityCommitTempFiles(root));
            }

            var retry = await db.AppendRecordAsync(blocked);
            var listed = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();
            var finalRecordIds = (await File.ReadAllLinesAsync(recordsPath))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => DeserializeRecord(line).RecordId)
                .ToArray();
            var finalIndex = await ReadIndexAsync(indexPath);

            Assert.True(retry.IsSuccess, retry.ErrorMessage);
            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            Assert.Equal(1, listed.Value!.Count(record => string.Equals(record.RecordId, blockedRecordId, StringComparison.Ordinal)));
            Assert.Equal(1, finalRecordIds.Count(id => string.Equals(id, blockedRecordId, StringComparison.Ordinal)));
            Assert.NotNull(finalIndex);
            Assert.Equal(2, finalIndex!.RecordCount);
            Assert.Equal(2, finalIndex.RecordLocators.Count);
            Assert.Empty(EnumerateAuthorityCommitTempFiles(root));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_BoundedPartialTrailingAuthorityTailIsReplacedWithoutTempAndLocatorsStayExact()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-authority-partial-tail");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const string partialMarker = "N14_4C_PARTIAL_TRAILING_AUTHORITY_MARKER";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var initial = await db.AppendRecordAsync(BuildRecord(
                recordId: "authority-durable-tail-existing",
                jobId: "authority-durable-tail-existing-job",
                requestId: "authority-durable-tail-existing-request"));
            Assert.True(initial.IsSuccess, initial.ErrorMessage);

            var committedPrefixBytes = await File.ReadAllBytesAsync(recordsPath);
            await File.AppendAllTextAsync(
                recordsPath,
                $"{{\"recordId\":\"{partialMarker}\"",
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            var bytesWithTail = await File.ReadAllBytesAsync(recordsPath);
            Assert.Equal(committedPrefixBytes, bytesWithTail.Take(committedPrefixBytes.Length).ToArray());
            Assert.Contains(partialMarker, Encoding.UTF8.GetString(bytesWithTail), StringComparison.Ordinal);

            var appendAfterTail = await new JsonlDesignConversionLocalArtifactDb(root).AppendRecordAsync(BuildRecord(
                recordId: "authority-durable-tail-new",
                jobId: "authority-durable-tail-new-job",
                requestId: "authority-durable-tail-new-request"));
            var finalBytes = await File.ReadAllBytesAsync(recordsPath);
            var finalText = Encoding.UTF8.GetString(finalBytes);
            var finalIndex = await ReadIndexAsync(indexPath);
            var finalRecordIds = (await File.ReadAllLinesAsync(recordsPath))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => DeserializeRecord(line).RecordId)
                .ToArray();

            Assert.True(appendAfterTail.IsSuccess, appendAfterTail.ErrorMessage);
            Assert.Equal(committedPrefixBytes, finalBytes.Take(committedPrefixBytes.Length).ToArray());
            Assert.DoesNotContain(partialMarker, finalText, StringComparison.Ordinal);
            Assert.Equal(["authority-durable-tail-existing", "authority-durable-tail-new"], finalRecordIds);
            Assert.NotNull(finalIndex);
            Assert.Equal(2, finalIndex!.RecordCount);
            Assert.Equal(2, finalIndex.RecordLocators.Count);
            Assert.Equal(committedPrefixBytes.LongLength, finalIndex.RecordLocators[1].ByteOffset);
            AssertAuthorityAndIndexChain(finalBytes, finalIndex, finalRecordIds);
            Assert.Empty(EnumerateAuthorityCommitTempFiles(root));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_SourceFingerprintRaceFailsBeforeAppendingSameLengthReplacement()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-authority-source-race");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var initial = await setupDb.AppendRecordAsync(BuildRecord(
                recordId: "authority-durable-race-record-a",
                jobId: "authority-durable-race-job",
                requestId: "authority-durable-race-request"));
            Assert.True(initial.IsSuccess, initial.ErrorMessage);

            var originalBytes = await File.ReadAllBytesAsync(recordsPath);
            var originalIndexBytes = await File.ReadAllBytesAsync(indexPath);
            byte[] racedBytes = [];
            var raceDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                authorityAppendBeforeOpenCallbackForTesting: async _ =>
                {
                    var currentBytes = await File.ReadAllBytesAsync(recordsPath);
                    Assert.Equal(originalBytes, currentBytes);
                    var currentText = Encoding.UTF8.GetString(currentBytes);
                    var racedText = currentText.Replace(
                        "authority-durable-race-record-a",
                        "authority-durable-race-record-z",
                        StringComparison.Ordinal);
                    Assert.NotEqual(currentText, racedText);
                    racedBytes = Encoding.UTF8.GetBytes(racedText);
                    Assert.Equal(currentBytes.LongLength, racedBytes.LongLength);
                    await File.WriteAllBytesAsync(recordsPath, racedBytes);
                    File.SetLastWriteTimeUtc(
                        recordsPath,
                        File.GetLastWriteTimeUtc(recordsPath).AddSeconds(2));
                });

            var racedAppend = await raceDb.AppendRecordAsync(BuildRecord(
                recordId: "authority-durable-race-new",
                jobId: "authority-durable-race-new-job",
                requestId: "authority-durable-race-new-request"));
            var afterRaceBytes = await File.ReadAllBytesAsync(recordsPath);
            var afterRaceIndexBytes = await File.ReadAllBytesAsync(indexPath);
            var afterRaceText = Encoding.UTF8.GetString(afterRaceBytes);

            Assert.False(racedAppend.IsSuccess);
            Assert.Null(racedAppend.Value);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedErrorCode, racedAppend.ErrorCode);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedSafeMessage, racedAppend.ErrorMessage);
            Assert.NotEmpty(racedBytes);
            Assert.Equal(racedBytes, afterRaceBytes);
            Assert.Equal(originalIndexBytes, afterRaceIndexBytes);
            Assert.DoesNotContain("authority-durable-race-new", afterRaceText, StringComparison.Ordinal);
            Assert.Empty(EnumerateAuthorityCommitTempFiles(root));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_PreCancelledAppendLeavesAuthorityUnchangedAndCleansTemp()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-authority-pre-cancelled");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        const string cancelledRecordId = "authority-durable-cancelled-record";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var initial = await db.AppendRecordAsync(BuildRecord(
                recordId: "authority-durable-before-cancel",
                jobId: "authority-durable-before-cancel-job",
                requestId: "authority-durable-before-cancel-request"));
            Assert.True(initial.IsSuccess, initial.ErrorMessage);

            var originalBytes = await File.ReadAllBytesAsync(recordsPath);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            var cancelled = await db.AppendRecordAsync(BuildRecord(
                recordId: cancelledRecordId,
                jobId: "authority-durable-cancelled-job",
                requestId: "authority-durable-cancelled-request"), cancellation.Token);
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);
            var fresh = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();

            Assert.False(cancelled.IsSuccess);
            Assert.Null(cancelled.Value);
            Assert.Equal("CANCELLED", cancelled.ErrorCode);
            Assert.Equal(originalBytes, afterBytes);
            Assert.True(fresh.IsSuccess, fresh.ErrorMessage);
            Assert.DoesNotContain(fresh.Value!, record => string.Equals(record.RecordId, cancelledRecordId, StringComparison.Ordinal));
            Assert.Empty(EnumerateAuthorityCommitTempFiles(root));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_IndexDeferredMetadataKeepsDerivedIndexAndCrossFileDurabilityFalse()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-authority-index-deferred");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const string ExistingRecordId = "authority-durable-index-existing";
        const string DeferredRecordId = "authority-durable-index-deferred";
        const string HealingRecordId = "authority-durable-index-healed";

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var first = await setupDb.AppendRecordAsync(BuildRecord(
                recordId: ExistingRecordId,
                jobId: "authority-durable-index-existing-job",
                requestId: "authority-durable-index-existing-request"));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var originalIndexBytes = await File.ReadAllBytesAsync(indexPath);
            var db = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                derivedIndexBeforeAtomicPublishCallbackForTesting:
                    CreateOneShotDerivedIndexPublishFailure());

            var deferred = await db.AppendRecordAsync(BuildRecord(
                recordId: DeferredRecordId,
                jobId: "authority-durable-index-deferred-job",
                requestId: "authority-durable-index-deferred-request"));
            var returnedText = System.Text.Json.JsonSerializer.Serialize(
                deferred.Value,
                DesignConversionCheckpointJsonOptions.Standard);

            Assert.True(deferred.IsSuccess, deferred.ErrorMessage);
            Assert.Contains(
                JsonlDesignConversionLocalArtifactDb.AuthorityRecordCommittedMarker,
                deferred.Value!.EvidenceRefs);
            Assert.Contains(
                JsonlDesignConversionLocalArtifactDb.DerivedIndexUpdateDeferredMarker,
                deferred.Value.EvidenceRefs);
            Assert.Contains(
                JsonlDesignConversionLocalArtifactDb.DerivedIndexDurableFalseMarker,
                deferred.Value.EvidenceRefs);
            Assert.Equal(
                bool.TrueString,
                deferred.Value.Metadata["local_artifact_db_authority_record_committed"]);
            Assert.Equal(
                "deferred",
                deferred.Value.Metadata["local_artifact_db_derived_index_update"]);
            Assert.Equal(
                bool.FalseString,
                deferred.Value.Metadata["local_artifact_db_derived_index_durable"]);
            Assert.Equal(
                bool.FalseString,
                deferred.Value.Metadata["local_artifact_db_cross_file_atomicity_claimed"]);
            Assert.Equal(bool.FalseString, deferred.Value.Metadata["product_ready"]);
            Assert.Equal(bool.FalseString, deferred.Value.Metadata["model_readiness_claimed"]);
            Assert.DoesNotContain("local_artifact_db_derived_index_durable=True", returnedText, StringComparison.Ordinal);
            Assert.DoesNotContain("local_artifact_db_cross_file_atomicity_claimed=True", returnedText, StringComparison.Ordinal);
            Assert.Equal(originalIndexBytes, await File.ReadAllBytesAsync(indexPath));
            Assert.Empty(EnumerateIndexWriteTempFiles(root));
            Assert.Equal(
                [ExistingRecordId, DeferredRecordId],
                (await File.ReadAllLinesAsync(recordsPath))
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => DeserializeRecord(line).RecordId)
                    .ToArray());

            var healed = await db.AppendRecordAsync(BuildRecord(
                recordId: HealingRecordId,
                jobId: "authority-durable-index-healed-job",
                requestId: "authority-durable-index-healed-request"));
            var healedIndex = await ReadIndexAsync(indexPath);

            Assert.True(healed.IsSuccess, healed.ErrorMessage);
            Assert.NotNull(healedIndex);
            AssertAuthorityAndIndexChain(
                await File.ReadAllBytesAsync(recordsPath),
                healedIndex!,
                [ExistingRecordId, DeferredRecordId, HealingRecordId]);
            Assert.Empty(EnumerateIndexWriteTempFiles(root));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task CompleteCommittedAuthorityAppendAsync_PreCancelledCallerSkipsIndexAndReturnsDeferredSuccess()
    {
        var record = BuildRecord(
            recordId: "authority-durable-post-commit-pre-cancelled",
            jobId: "authority-durable-post-commit-pre-cancelled-job",
            requestId: "authority-durable-post-commit-pre-cancelled-request");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var writerCalled = false;

        var completed = await JsonlDesignConversionLocalArtifactDb.CompleteCommittedAuthorityAppendAsync(
            record,
            cancellation.Token,
            _ =>
            {
                writerCalled = true;
                return Task.FromResult(true);
            });

        Assert.True(completed.IsSuccess, completed.ErrorMessage);
        Assert.False(writerCalled);
        Assert.Contains(
            JsonlDesignConversionLocalArtifactDb.AuthorityRecordCommittedMarker,
            completed.Value!.EvidenceRefs);
        Assert.Contains(
            JsonlDesignConversionLocalArtifactDb.DerivedIndexUpdateDeferredMarker,
            completed.Value.EvidenceRefs);
        Assert.Contains(
            JsonlDesignConversionLocalArtifactDb.DerivedIndexDurableFalseMarker,
            completed.Value.EvidenceRefs);
        Assert.Equal("deferred", completed.Value.Metadata["local_artifact_db_derived_index_update"]);
        Assert.Equal(bool.FalseString, completed.Value.Metadata["local_artifact_db_derived_index_durable"]);
        Assert.Equal(bool.FalseString, completed.Value.Metadata["local_artifact_db_cross_file_atomicity_claimed"]);
    }

    [Fact]
    public async Task CompleteCommittedAuthorityAppendAsync_CancellationDuringIndexWriterUsesNonCancellableTokenAndReturnsSuccess()
    {
        var record = BuildRecord(
            recordId: "authority-durable-post-commit-mid-write-cancelled",
            jobId: "authority-durable-post-commit-mid-write-cancelled-job",
            requestId: "authority-durable-post-commit-mid-write-cancelled-request");
        using var cancellation = new CancellationTokenSource();
        var observedWriterToken = default(CancellationToken);
        var writerCompleted = false;

        var completed = await JsonlDesignConversionLocalArtifactDb.CompleteCommittedAuthorityAppendAsync(
            record,
            cancellation.Token,
            async writerToken =>
            {
                observedWriterToken = writerToken;
                cancellation.Cancel();
                await Task.Yield();
                writerToken.ThrowIfCancellationRequested();
                writerCompleted = true;
                return true;
            });

        Assert.True(completed.IsSuccess, completed.ErrorMessage);
        Assert.True(cancellation.IsCancellationRequested);
        Assert.True(writerCompleted);
        Assert.False(observedWriterToken.CanBeCanceled);
        Assert.False(observedWriterToken.IsCancellationRequested);
        Assert.Same(record, completed.Value);
        Assert.DoesNotContain(
            JsonlDesignConversionLocalArtifactDb.DerivedIndexUpdateDeferredMarker,
            completed.Value!.EvidenceRefs);
    }

    private static void AssertAuthorityLocator(
        byte[] authorityBytes,
        DesignConversionLocalArtifactDbIndexRecordLocator locator,
        string expectedRecordId,
        long expectedByteOffset,
        int expectedLineNumber)
    {
        Assert.Equal(expectedRecordId, locator.RecordId);
        Assert.Equal(expectedByteOffset, locator.ByteOffset);
        Assert.True(locator.ByteLength > 0);
        Assert.Equal(expectedLineNumber, locator.LineNumber);
        var lineBytes = authorityBytes
            .Skip((int)locator.ByteOffset)
            .Take((int)locator.ByteLength)
            .ToArray();
        Assert.Equal(locator.ByteLength, lineBytes.LongLength);
        Assert.Equal($"sha256:{HashBytes(lineBytes)}", locator.AuthorityLineSha256);
        Assert.Equal(expectedRecordId, DeserializeRecord(DecodeAuthorityLine(lineBytes, locator.ByteOffset == 0)).RecordId);
    }

    private static void AssertAuthorityAndIndexChain(
        byte[] authorityBytes,
        DesignConversionLocalArtifactDbIndex index,
        IReadOnlyList<string> expectedRecordIds)
    {
        Assert.Equal(expectedRecordIds.Count, index.RecordCount);
        Assert.Equal(expectedRecordIds.Count, index.RecordLocators.Count);
        Assert.Equal(expectedRecordIds, index.RecordLocators.Select(locator => locator.RecordId).ToArray());
        Assert.Equal(authorityBytes.LongLength, index.RecordsFileLength);

        var expectedOffset = 0L;
        for (var indexPosition = 0; indexPosition < index.RecordLocators.Count; indexPosition++)
        {
            var locator = index.RecordLocators[indexPosition];
            AssertAuthorityLocator(
                authorityBytes,
                locator,
                expectedRecordIds[indexPosition],
                expectedOffset,
                indexPosition + 1);
            expectedOffset += locator.ByteLength;
        }

        Assert.Equal(authorityBytes.LongLength, expectedOffset);
    }

    private static async Task<DesignConversionLocalArtifactDbIndex?> ReadIndexAsync(string indexPath)
    {
        var indexText = await File.ReadAllTextAsync(indexPath);
        return System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbIndex>(
            indexText,
            DesignConversionCheckpointJsonOptions.Standard);
    }

    private static Action CreateOneShotDerivedIndexPublishFailure()
    {
        var remainingFailures = 1;
        return () =>
        {
            // Один детерминированный отказ проверяет именно публикацию индекса и не зависит от правил FileShare платформы.
            if (Interlocked.Exchange(ref remainingFailures, 0) == 1)
                throw new IOException("Injected derived-index publication failure.");
        };
    }

    private static async Task AssertTaskRemainsPendingAsync(Task task, TimeSpan wait)
    {
        var delay = Task.Delay(wait);
        var completed = await Task.WhenAny(task, delay);
        Assert.Same(delay, completed);
    }

    private static DesignConversionLocalArtifactDbRecord DeserializeRecord(string line)
    {
        var record = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbRecord>(
            line,
            DesignConversionCheckpointJsonOptions.Standard);
        Assert.NotNull(record);
        return record!;
    }

    private static string DecodeAuthorityLine(byte[] lineBytes, bool firstLine)
    {
        var offset = 0;
        var length = lineBytes.Length;
        if (length > 0 && lineBytes[length - 1] == (byte)'\n')
            length--;
        if (length > 0 && lineBytes[length - 1] == (byte)'\r')
            length--;
        if (firstLine
            && length >= 3
            && lineBytes[0] == 0xEF
            && lineBytes[1] == 0xBB
            && lineBytes[2] == 0xBF)
        {
            offset = 3;
            length -= 3;
        }

        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true)
            .GetString(lineBytes, offset, length);
    }

    private static string HashBytes(byte[] value)
        => Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();

    private static string[] EnumerateAuthorityCommitTempFiles(string root)
        => Directory.Exists(root)
            ? Directory.EnumerateFiles(root, "conversion-results.jsonl.*.append.tmp", SearchOption.TopDirectoryOnly).ToArray()
            : [];

    private static string[] EnumerateIndexWriteTempFiles(string root)
        => Directory.Exists(root)
            ? Directory.EnumerateFiles(root, "conversion-results.index.json.*.write.tmp", SearchOption.TopDirectoryOnly).ToArray()
            : [];

    private static void AssertSafeFailureDoesNotLeak(
        string failureText,
        string root,
        string recordsPath,
        string privateMarker)
    {
        Assert.DoesNotContain(root, failureText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(recordsPath, failureText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("conversion-results.jsonl", failureText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".tmp", failureText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(privateMarker, failureText, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Users/lubao", failureText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IOException", failureText, StringComparison.Ordinal);
        Assert.DoesNotContain("UnauthorizedAccessException", failureText, StringComparison.Ordinal);
        Assert.DoesNotContain("Win32Exception", failureText, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", failureText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private", failureText, StringComparison.OrdinalIgnoreCase);
    }

    private static DesignConversionLocalArtifactDbRecord BuildRecord(
        string recordId,
        string? privateMarker = null,
        string jobId = "authority-durable-job",
        string requestId = "authority-durable-request",
        DateTimeOffset? createdAtUtc = null)
    {
        var htmlRef = privateMarker is null
            ? $"artifact://authority-durable/{recordId}/html"
            : $"artifact://authority-durable-private/{recordId}/html?marker={privateMarker}<main>private html</main>";
        var cssRef = privateMarker is null
            ? $"artifact://authority-durable/{recordId}/css"
            : $"artifact://authority-durable-private/{recordId}/css?marker={privateMarker}.private";
        var metadata = privateMarker is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>
            {
                ["private_marker"] = privateMarker,
                ["raw_html"] = $"<main>{privateMarker}</main>",
                ["raw_css"] = $".private {{ background: url('{privateMarker}'); }}"
            };

        return new(
            RecordId: recordId,
            DatabaseKind: JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
            JobId: jobId,
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma.ToString(),
            Tier: "local",
            SanitizedSourceRefs: new Dictionary<string, string>
            {
                ["source_manifest_ref"] = $"artifact://authority-durable/{recordId}/source-manifest"
            },
            ConversionResult: new DesignConversionConversionResultArtifact(
                ResultId: $"{recordId}:conversion-result",
                HtmlRef: htmlRef,
                HtmlSha256: string.Empty,
                CssRef: cssRef,
                CssSha256: string.Empty,
                Metadata: new Dictionary<string, string>()),
            RenderArtifactRefs:
            [
                new DesignConversionRenderArtifactDbRef(
                    ViewportId: "desktop",
                    ArtifactId: $"{recordId}:render:desktop",
                    Kind: "browser_screenshot",
                    UriRef: $"artifact://authority-durable/{recordId}/render.png",
                    Sha256: $"sha256:{recordId}-render",
                    ByteLength: 10,
                    Width: 1440,
                    Height: 900,
                    Metadata: new Dictionary<string, string>())
            ],
            VisualJudgeStatus: new DesignConversionVisualJudgeArtifactDbStatus(
                Status: DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                ProviderBoundary: DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                ExternalCallExecuted: false,
                EstimatedCostUsd: 0m,
                TierDecision: DesignConversionVisualQaBoundaryPolicy.CostCapMarker,
                EvidenceRefs: [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker],
                Metadata: new Dictionary<string, string>()),
            DpoCandidates: [],
            CreatedAtUtc: createdAtUtc ?? DateTimeOffset.UnixEpoch,
            EvidenceRefs:
            [
                JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker,
                JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
                JsonlDesignConversionLocalArtifactDb.ConversionResultMarker
            ],
            Metadata: metadata);
    }

}
