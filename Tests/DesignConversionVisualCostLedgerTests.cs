// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed class DesignConversionVisualCostLedgerTests
{
    private const string SafeReadFailureMessage = "DesignConversion visual-cost ledger records could not be read.";

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void Constructor_NonPositiveMaxJournalBytes_Throws(long maxJournalBytes)
    {
        var root = NewTempRoot("visual-cost-ledger-invalid-quota");

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new JsonlDesignConversionVisualCostLedger(root, maxJournalBytes));

        Assert.Equal("maxJournalBytes", exception.ParamName);
    }

    [Fact]
    public void Constructor_PositiveMaxJournalBytes_ExposesConfiguredAndDefaultLimits()
    {
        var root = NewTempRoot("visual-cost-ledger-configured-quota");

        var configured = new JsonlDesignConversionVisualCostLedger(root, 4096L);
        var defaulted = new JsonlDesignConversionVisualCostLedger(root);

        Assert.Equal(4096L, configured.MaxJournalBytes);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.DefaultMaxJournalBytes, defaulted.MaxJournalBytes);
    }

    [Fact]
    public async Task ListRecordsAsync_ExistingValidJournalAboveConfiguredLimit_FailsClosedBeforeWholeFileRead()
    {
        var root = NewTempRoot("visual-cost-ledger-list-quota");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);

        try
        {
            var seed = new JsonlDesignConversionVisualCostLedger(root);
            var appended = await seed.AppendRecordAsync(BuildValidRecord("list-quota-seed"));
            Assert.True(appended.IsSuccess, appended.ErrorMessage);
            var expectedBytes = await File.ReadAllBytesAsync(recordsPath);

            var result = await new JsonlDesignConversionVisualCostLedger(root, expectedBytes.LongLength - 1L)
                .ListRecordsAsync();
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            AssertVisualCostLedgerQuotaFailure(result, root, recordsPath);
            Assert.Equal(expectedBytes, afterBytes);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_DistinctRecordAtConfiguredLimit_RollsActiveSegmentAndPreservesBothRecords()
    {
        var root = NewTempRoot("visual-cost-ledger-append-quota-distinct");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);

        try
        {
            var seed = new JsonlDesignConversionVisualCostLedger(root);
            var first = await seed.AppendRecordAsync(BuildValidRecord("append-quota-rollover-a"));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var archivedBytes = await File.ReadAllBytesAsync(recordsPath);
            var secondRecord = BuildValidRecord("append-quota-rollover-b") with
            {
                UserRef = first.Value!.UserRef,
                SubmittedTier = first.Value.SubmittedTier,
                PeriodKind = first.Value.PeriodKind,
                PeriodStartUtc = first.Value.PeriodStartUtc,
                PeriodEndExclusiveUtc = first.Value.PeriodEndExclusiveUtc
            };

            var result = await new JsonlDesignConversionVisualCostLedger(root, archivedBytes.LongLength)
                .AppendRecordAsync(secondRecord);
            var activeBytes = await File.ReadAllBytesAsync(recordsPath);
            var segmentFiles = Directory
                .EnumerateFiles(root, JsonlDesignConversionVisualCostLedger.SegmentFileNamePrefix + "*")
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            var readback = await new JsonlDesignConversionVisualCostLedger(root, archivedBytes.LongLength)
                .ListRecordsAsync();
            var aggregate = await new JsonlDesignConversionVisualCostLedger(root, archivedBytes.LongLength)
                .AggregatePeriodAsync(first.Value!.UserRef, first.Value.PeriodStartUtc, first.Value.PeriodEndExclusiveUtc);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Single(segmentFiles);
            Assert.Equal(JsonlDesignConversionVisualCostLedger.RecordsFileName + ".segment-000001", Path.GetFileName(segmentFiles[0]));
            Assert.Equal(archivedBytes, await File.ReadAllBytesAsync(segmentFiles[0]));
            Assert.Equal(SerializeJsonlLine(result.Value!), activeBytes);
            Assert.True(readback.IsSuccess, readback.ErrorMessage);
            Assert.Collection(
                readback.Value!,
                record => AssertSameDurableVisualCostRecord(first.Value!, record),
                record => AssertSameDurableVisualCostRecord(result.Value!, record));
            Assert.True(aggregate.IsSuccess, aggregate.ErrorMessage);
            Assert.Equal(2, aggregate.Value!.RecordCount);
            Assert.Equal(first.Value.TotalCostUsd + result.Value!.TotalCostUsd, aggregate.Value.TotalMeasuredCostUsd);
            Assert.True(aggregate.Value.MeasurementComplete);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_IdenticalReplayAtConfiguredLimit_SucceedsAndPreservesBytes()
    {
        var root = NewTempRoot("visual-cost-ledger-append-quota-replay");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);
        var record = BuildValidRecord("append-quota-replay");

        try
        {
            var seed = new JsonlDesignConversionVisualCostLedger(root);
            var appended = await seed.AppendRecordAsync(record);
            Assert.True(appended.IsSuccess, appended.ErrorMessage);
            var expectedBytes = await File.ReadAllBytesAsync(recordsPath);

            var replay = await new JsonlDesignConversionVisualCostLedger(root, expectedBytes.LongLength)
                .AppendRecordAsync(record);
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            Assert.True(replay.IsSuccess, replay.ErrorMessage);
            AssertSameDurableVisualCostRecord(appended.Value!, replay.Value!);
            Assert.Equal(expectedBytes, afterBytes);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_ConcurrentDistinctRecordsAcrossTwoInstancesDuringRolloverPreservesEveryRecordExactlyOnce()
    {
        var root = NewTempRoot("visual-cost-ledger-concurrent-rollover");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);

        try
        {
            var seedLedger = new JsonlDesignConversionVisualCostLedger(root);
            var seed = await seedLedger.AppendRecordAsync(BuildValidRecord("concurrent-roll-00"));
            Assert.True(seed.IsSuccess, seed.ErrorMessage);
            var maxJournalBytes = (await File.ReadAllBytesAsync(recordsPath)).LongLength;

            var firstInstance = new JsonlDesignConversionVisualCostLedger(root, maxJournalBytes);
            var secondInstance = new JsonlDesignConversionVisualCostLedger(root, maxJournalBytes);
            var appendRecords = Enumerable
                .Range(1, 12)
                .Select(index => BuildValidRecord($"concurrent-roll-{index:00}") with
                {
                    UserRef = seed.Value!.UserRef,
                    SubmittedTier = seed.Value.SubmittedTier,
                    PeriodKind = seed.Value.PeriodKind,
                    PeriodStartUtc = seed.Value.PeriodStartUtc,
                    PeriodEndExclusiveUtc = seed.Value.PeriodEndExclusiveUtc
                })
                .ToArray();
            var startGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var appendTasks = appendRecords
                .Select((record, index) => Task.Run(async () =>
                {
                    await startGate.Task;
                    var ledger = index % 2 == 0 ? firstInstance : secondInstance;
                    return await ledger.AppendRecordAsync(record);
                }))
                .ToArray();

            startGate.SetResult();
            var appendResults = await Task.WhenAll(appendTasks);
            var thirdInstance = new JsonlDesignConversionVisualCostLedger(root, maxJournalBytes);
            var readback = await thirdInstance.ListRecordsAsync();
            var aggregate = await thirdInstance.AggregatePeriodAsync(
                seed.Value!.UserRef,
                seed.Value.PeriodStartUtc,
                seed.Value.PeriodEndExclusiveUtc);
            var firstSnapshot = SnapshotJournalFileBytes(root);
            var repeatedReadback = await new JsonlDesignConversionVisualCostLedger(root, maxJournalBytes)
                .ListRecordsAsync();
            var secondSnapshot = SnapshotJournalFileBytes(root);
            var segmentFiles = Directory
                .EnumerateFiles(root, JsonlDesignConversionVisualCostLedger.SegmentFileNamePrefix + "*")
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

            Assert.All(appendResults, result => Assert.True(result.IsSuccess, result.ErrorMessage));
            Assert.True(readback.IsSuccess, readback.ErrorMessage);
            Assert.True(repeatedReadback.IsSuccess, repeatedReadback.ErrorMessage);
            Assert.True(aggregate.IsSuccess, aggregate.ErrorMessage);
            Assert.Equal(13, readback.Value!.Count);
            Assert.Equal(
                13,
                readback.Value.Select(record => record.RecordId).Distinct(StringComparer.Ordinal).Count());
            foreach (var expected in new[] { seed.Value! }.Concat(appendResults.Select(result => result.Value!)))
            {
                var actual = Assert.Single(
                    readback.Value,
                    record => string.Equals(record.RecordId, expected.RecordId, StringComparison.Ordinal));
                AssertSameDurableVisualCostRecord(expected, actual);
            }

            Assert.Equal(
                Enumerable
                    .Range(1, 12)
                    .Select(index => JsonlDesignConversionVisualCostLedger.RecordsFileName + $".segment-{index:000000}")
                    .ToArray(),
                segmentFiles.Select(Path.GetFileName).ToArray());
            Assert.True(File.Exists(recordsPath));
            foreach (var journalPath in new[] { recordsPath }.Concat(segmentFiles))
            {
                var length = new FileInfo(journalPath).Length;
                Assert.True(length > 0L, journalPath);
                Assert.True(length <= maxJournalBytes, journalPath);
            }

            Assert.Empty(Directory.EnumerateFiles(root, $"{JsonlDesignConversionVisualCostLedger.RecordsFileName}*.bounded-append.intent*"));
            Assert.Equal(13, aggregate.Value!.RecordCount);
            Assert.Equal(
                seed.Value.TotalCostUsd + appendResults.Sum(result => result.Value!.TotalCostUsd),
                aggregate.Value.TotalMeasuredCostUsd);
            Assert.True(aggregate.Value.MeasurementComplete);
            Assert.Equal(
                readback.Value.Select(record => record.RecordId).OrderBy(recordId => recordId, StringComparer.Ordinal).ToArray(),
                repeatedReadback.Value!.Select(record => record.RecordId).OrderBy(recordId => recordId, StringComparer.Ordinal).ToArray());
            Assert.Equal(firstSnapshot.Keys.ToArray(), secondSnapshot.Keys.ToArray());
            foreach (var entry in firstSnapshot)
                Assert.Equal(entry.Value, secondSnapshot[entry.Key]);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_SingleRecordLargerThanConfiguredLimit_FailsQuotaWithoutPartialFiles()
    {
        var root = NewTempRoot("visual-cost-ledger-single-line-quota");
        var sourceRoot = NewTempRoot("visual-cost-ledger-single-line-source");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);

        try
        {
            var candidate = await new JsonlDesignConversionVisualCostLedger(sourceRoot)
                .AppendRecordAsync(BuildValidRecord("single-line-quota"));
            Assert.True(candidate.IsSuccess, candidate.ErrorMessage);
            var candidateLineBytes = SerializeJsonlLine(candidate.Value!);

            var result = await new JsonlDesignConversionVisualCostLedger(root, candidateLineBytes.LongLength - 1L)
                .AppendRecordAsync(candidate.Value!);

            AssertVisualCostLedgerQuotaFailure(result, root, recordsPath);
            Assert.False(File.Exists(recordsPath));
            Assert.Empty(Directory.EnumerateFiles(root, JsonlDesignConversionVisualCostLedger.SegmentFileNamePrefix + "*"));
            Assert.Empty(Directory.EnumerateFiles(root, $"{JsonlDesignConversionVisualCostLedger.RecordsFileName}*.bounded-append.intent*"));
        }
        finally
        {
            DeleteTempRoot(root);
            DeleteTempRoot(sourceRoot);
        }
    }

    [Fact]
    public async Task ListRecordsAsync_DuplicateRecordIdAcrossSegmentAndActiveFailsClosed()
    {
        var root = NewTempRoot("visual-cost-ledger-segment-duplicate");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);

        try
        {
            var seed = new JsonlDesignConversionVisualCostLedger(root);
            var first = await seed.AppendRecordAsync(BuildValidRecord("segment-duplicate-a"));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var firstBytes = await File.ReadAllBytesAsync(recordsPath);
            var second = await new JsonlDesignConversionVisualCostLedger(root, firstBytes.LongLength)
                .AppendRecordAsync(BuildValidRecord("segment-duplicate-b"));
            Assert.True(second.IsSuccess, second.ErrorMessage);
            var segmentPath = Assert.Single(Directory.EnumerateFiles(
                root,
                JsonlDesignConversionVisualCostLedger.SegmentFileNamePrefix + "*"));
            var segmentBytes = await File.ReadAllBytesAsync(segmentPath);
            await File.WriteAllBytesAsync(recordsPath, segmentBytes);

            var result = await new JsonlDesignConversionVisualCostLedger(root, firstBytes.LongLength)
                .ListRecordsAsync();
            var activeBytes = await File.ReadAllBytesAsync(recordsPath);

            AssertVisualCostLedgerReadFailure(
                result,
                root,
                recordsPath,
                "PRIVATE_MARKER_C:/Users/lubao/private/visual-cost-ledger.jsonl#duplicate");
            Assert.Equal(segmentBytes, activeBytes);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task ListRecordsAsync_MalformedSegmentFailsClosedAndPreservesActiveJournal()
    {
        var root = NewTempRoot("visual-cost-ledger-malformed-segment");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);
        var segmentPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.SegmentFileNamePrefix + "000001");

        try
        {
            var seed = new JsonlDesignConversionVisualCostLedger(root);
            var active = await seed.AppendRecordAsync(BuildValidRecord("malformed-segment-active"));
            Assert.True(active.IsSuccess, active.ErrorMessage);
            var activeBytes = await File.ReadAllBytesAsync(recordsPath);
            await File.WriteAllTextAsync(segmentPath, "{\"not\":\"a valid ledger record\"}" + Environment.NewLine);

            var result = await new JsonlDesignConversionVisualCostLedger(root).ListRecordsAsync();
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            AssertVisualCostLedgerReadFailure(
                result,
                root,
                recordsPath,
                "PRIVATE_MARKER_C:/Users/lubao/private/visual-cost-ledger.jsonl#malformed-segment");
            Assert.Equal(activeBytes, afterBytes);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task ListRecordsAsync_SegmentDirectoryFailsClosedAndPreservesActiveJournal()
    {
        var root = NewTempRoot("visual-cost-ledger-segment-directory");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);
        var segmentDirectoryPath = Path.Combine(
            root,
            JsonlDesignConversionVisualCostLedger.SegmentFileNamePrefix + "000001");

        try
        {
            var seed = new JsonlDesignConversionVisualCostLedger(root);
            var active = await seed.AppendRecordAsync(BuildValidRecord("segment-directory-active"));
            Assert.True(active.IsSuccess, active.ErrorMessage);
            var activeBytes = await File.ReadAllBytesAsync(recordsPath);
            Directory.CreateDirectory(segmentDirectoryPath);

            var result = await new JsonlDesignConversionVisualCostLedger(root).ListRecordsAsync();
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            AssertVisualCostLedgerReadFailure(
                result,
                root,
                recordsPath,
                "PRIVATE_MARKER_C:/Users/lubao/private/visual-cost-ledger.jsonl#segment-directory");
            Assert.Equal(activeBytes, afterBytes);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task ListRecordsAsync_NonCanonicalSegmentAliasFailsClosedAndPreservesActiveJournal()
    {
        var root = NewTempRoot("visual-cost-ledger-segment-alias");
        var sourceRoot = NewTempRoot("visual-cost-ledger-segment-alias-source");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);
        var sourceRecordsPath = Path.Combine(sourceRoot, JsonlDesignConversionVisualCostLedger.RecordsFileName);
        var segmentAliasPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.SegmentFileNamePrefix + "1");

        try
        {
            var seed = new JsonlDesignConversionVisualCostLedger(root);
            var active = await seed.AppendRecordAsync(BuildValidRecord("segment-alias-active"));
            Assert.True(active.IsSuccess, active.ErrorMessage);
            var activeBytes = await File.ReadAllBytesAsync(recordsPath);

            var sourceSeed = new JsonlDesignConversionVisualCostLedger(sourceRoot);
            var source = await sourceSeed.AppendRecordAsync(BuildValidRecord("segment-alias-source"));
            Assert.True(source.IsSuccess, source.ErrorMessage);
            await File.WriteAllBytesAsync(segmentAliasPath, await File.ReadAllBytesAsync(sourceRecordsPath));

            var result = await new JsonlDesignConversionVisualCostLedger(root).ListRecordsAsync();
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            AssertVisualCostLedgerReadFailure(
                result,
                root,
                recordsPath,
                "PRIVATE_MARKER_C:/Users/lubao/private/visual-cost-ledger.jsonl#segment-alias");
            Assert.Equal(activeBytes, afterBytes);
        }
        finally
        {
            DeleteTempRoot(root);
            DeleteTempRoot(sourceRoot);
        }
    }

    [DesignConversionJournalReparsePointFact]
    public async Task ListRecordsAsync_RootDirectoryReparsePointFailsClosedWithoutFollowingExternalJournal()
    {
        var targetRoot = NewTempRoot("visual-cost-ledger-root-reparse-target");
        var linkRoot = NewTempRoot("visual-cost-ledger-root-reparse-link");
        var targetRecordsPath = Path.Combine(targetRoot, JsonlDesignConversionVisualCostLedger.RecordsFileName);

        try
        {
            var targetStore = new JsonlDesignConversionVisualCostLedger(targetRoot);
            var targetRecord = await targetStore.AppendRecordAsync(BuildValidRecord("root-reparse-target"));
            Assert.True(targetRecord.IsSuccess, targetRecord.ErrorMessage);
            var targetBytes = await File.ReadAllBytesAsync(targetRecordsPath);
            Directory.CreateDirectory(Path.GetDirectoryName(linkRoot)!);
            Directory.CreateSymbolicLink(linkRoot, targetRoot);
            Assert.NotEqual(FileAttributes.None, File.GetAttributes(linkRoot) & FileAttributes.ReparsePoint);

            var result = await new JsonlDesignConversionVisualCostLedger(linkRoot).ListRecordsAsync();

            AssertVisualCostLedgerReadFailure(
                result,
                linkRoot,
                Path.Combine(linkRoot, JsonlDesignConversionVisualCostLedger.RecordsFileName),
                targetRoot);
            Assert.Equal(targetBytes, await File.ReadAllBytesAsync(targetRecordsPath));
            Assert.Empty(Directory.EnumerateFiles(
                targetRoot,
                $"{JsonlDesignConversionVisualCostLedger.RecordsFileName}*.bounded-append.intent*"));
        }
        finally
        {
            if (Directory.Exists(linkRoot))
                Directory.Delete(linkRoot);
            DeleteTempRoot(Path.GetDirectoryName(linkRoot)!);
            DeleteTempRoot(targetRoot);
        }
    }

    [DesignConversionJournalReparsePointFact]
    public async Task ListRecordsAsync_ActiveJournalFileReparsePointFailsClosedWithoutFollowingExternalJournal()
    {
        var root = NewTempRoot("visual-cost-ledger-active-reparse");
        var sourceRoot = NewTempRoot("visual-cost-ledger-active-reparse-source");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);
        var sourceRecordsPath = Path.Combine(sourceRoot, JsonlDesignConversionVisualCostLedger.RecordsFileName);

        try
        {
            var source = await new JsonlDesignConversionVisualCostLedger(sourceRoot)
                .AppendRecordAsync(BuildValidRecord("active-reparse-source"));
            Assert.True(source.IsSuccess, source.ErrorMessage);
            var sourceBytes = await File.ReadAllBytesAsync(sourceRecordsPath);
            Directory.CreateDirectory(root);
            File.CreateSymbolicLink(recordsPath, sourceRecordsPath);
            Assert.NotEqual(FileAttributes.None, File.GetAttributes(recordsPath) & FileAttributes.ReparsePoint);

            var result = await new JsonlDesignConversionVisualCostLedger(root).ListRecordsAsync();

            AssertVisualCostLedgerReadFailure(result, root, recordsPath, sourceRecordsPath);
            Assert.Equal(sourceBytes, await File.ReadAllBytesAsync(sourceRecordsPath));
            AssertNoLedgerAppendArtifacts(root);
        }
        finally
        {
            if (File.Exists(recordsPath))
                File.Delete(recordsPath);
            DeleteTempRoot(root);
            DeleteTempRoot(sourceRoot);
        }
    }

    [DesignConversionJournalReparsePointFact]
    public async Task ListRecordsAsync_CanonicalSegmentFileReparsePointFailsClosedWithoutFollowingExternalJournal()
    {
        var root = NewTempRoot("visual-cost-ledger-segment-reparse");
        var sourceRoot = NewTempRoot("visual-cost-ledger-segment-reparse-source");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);
        var sourceRecordsPath = Path.Combine(sourceRoot, JsonlDesignConversionVisualCostLedger.RecordsFileName);
        var segmentPath = Path.Combine(
            root,
            JsonlDesignConversionVisualCostLedger.SegmentFileNamePrefix + "000001");

        try
        {
            var active = await new JsonlDesignConversionVisualCostLedger(root)
                .AppendRecordAsync(BuildValidRecord("segment-reparse-active"));
            var source = await new JsonlDesignConversionVisualCostLedger(sourceRoot)
                .AppendRecordAsync(BuildValidRecord("segment-reparse-source"));
            Assert.True(active.IsSuccess, active.ErrorMessage);
            Assert.True(source.IsSuccess, source.ErrorMessage);
            var activeBytes = await File.ReadAllBytesAsync(recordsPath);
            var sourceBytes = await File.ReadAllBytesAsync(sourceRecordsPath);
            File.CreateSymbolicLink(segmentPath, sourceRecordsPath);
            Assert.NotEqual(FileAttributes.None, File.GetAttributes(segmentPath) & FileAttributes.ReparsePoint);

            var result = await new JsonlDesignConversionVisualCostLedger(root).ListRecordsAsync();

            AssertVisualCostLedgerReadFailure(result, root, recordsPath, sourceRecordsPath);
            Assert.Equal(activeBytes, await File.ReadAllBytesAsync(recordsPath));
            Assert.Equal(sourceBytes, await File.ReadAllBytesAsync(sourceRecordsPath));
            AssertNoLedgerAppendArtifacts(root);
        }
        finally
        {
            if (File.Exists(segmentPath))
                File.Delete(segmentPath);
            DeleteTempRoot(root);
            DeleteTempRoot(sourceRoot);
        }
    }

    [Fact]
    public async Task ListRecordsAsync_InvalidUtf8Ledger_ReturnsSafeFailureAndPreservesBytes()
    {
        var root = NewTempRoot("visual-cost-ledger-list-invalid-utf8");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);
        const string privateMarker = "PRIVATE_MARKER_C:/Users/lubao/private/visual-cost-ledger.jsonl#list";
        var expectedBytes = BuildInvalidUtf8JsonlBytes(privateMarker);

        try
        {
            Directory.CreateDirectory(root);
            await File.WriteAllBytesAsync(recordsPath, expectedBytes);

            var result = await new JsonlDesignConversionVisualCostLedger(root).ListRecordsAsync();
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            AssertVisualCostLedgerReadFailure(result, root, recordsPath, privateMarker);
            Assert.Equal(expectedBytes, afterBytes);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task AggregatePeriodAsync_InvalidUtf8Ledger_ReturnsSafeFailureAndPreservesBytes()
    {
        var root = NewTempRoot("visual-cost-ledger-aggregate-invalid-utf8");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);
        const string privateMarker = "PRIVATE_MARKER_C:/Users/lubao/private/visual-cost-ledger.jsonl#aggregate";
        var expectedBytes = BuildInvalidUtf8JsonlBytes(privateMarker);
        var userRef = JsonlDesignConversionVisualCostLedger
            .BuildPseudonymousUserRef("visual-cost-ledger-invalid-utf8-user")
            .Value!;
        var period = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(DateTimeOffset.UnixEpoch);

        try
        {
            Directory.CreateDirectory(root);
            await File.WriteAllBytesAsync(recordsPath, expectedBytes);

            var result = await new JsonlDesignConversionVisualCostLedger(root)
                .AggregatePeriodAsync(userRef, period.StartUtc, period.EndExclusiveUtc);
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            AssertVisualCostLedgerReadFailure(result, root, recordsPath, privateMarker);
            Assert.Equal(expectedBytes, afterBytes);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_InvalidUtf8Ledger_ReturnsSafeReadFailureAndDoesNotOverwriteCorruptFile()
    {
        var root = NewTempRoot("visual-cost-ledger-append-invalid-utf8");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);
        const string privateMarker = "PRIVATE_MARKER_C:/Users/lubao/private/visual-cost-ledger.jsonl#append";
        var expectedBytes = BuildInvalidUtf8JsonlBytes(privateMarker);

        try
        {
            Directory.CreateDirectory(root);
            await File.WriteAllBytesAsync(recordsPath, expectedBytes);

            var result = await new JsonlDesignConversionVisualCostLedger(root)
                .AppendRecordAsync(BuildValidRecord("append-invalid-utf8"));
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            AssertVisualCostLedgerReadFailure(result, root, recordsPath, privateMarker);
            Assert.Equal(expectedBytes, afterBytes);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_JournalOpenWithoutDeleteShare_AppendsInPlaceAndPreservesPrefix()
    {
        var root = NewTempRoot("visual-cost-ledger-shared-open-append");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);

        try
        {
            var seed = new JsonlDesignConversionVisualCostLedger(root);
            var first = await seed.AppendRecordAsync(BuildValidRecord("shared-open-prefix"));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var prefixBytes = await File.ReadAllBytesAsync(recordsPath);
            byte[] appendedViaHeldOpen;

            await using (var heldOpen = new FileStream(
                recordsPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                var initialLength = heldOpen.Length;
                var initialPosition = heldOpen.Position;
                var append = await new JsonlDesignConversionVisualCostLedger(root)
                    .AppendRecordAsync(BuildValidRecord("shared-open-appended"));

                Assert.True(append.IsSuccess, append.ErrorMessage);
                Assert.Equal(prefixBytes.LongLength, initialLength);
                Assert.Equal(0L, initialPosition);
                Assert.True(heldOpen.Length > initialLength);
                heldOpen.Position = initialLength;
                appendedViaHeldOpen = new byte[checked((int)(heldOpen.Length - initialLength))];
                await heldOpen.ReadExactlyAsync(appendedViaHeldOpen);
            }

            var afterBytes = await File.ReadAllBytesAsync(recordsPath);
            var readback = await new JsonlDesignConversionVisualCostLedger(root).ListRecordsAsync();

            AssertBytePrefix(prefixBytes, afterBytes);
            Assert.Equal(appendedViaHeldOpen, afterBytes.Skip(prefixBytes.Length).ToArray());
            Assert.Equal(2, File.ReadAllLines(recordsPath).Length);
            Assert.True(readback.IsSuccess, readback.ErrorMessage);
            Assert.Equal(2, readback.Value!.Count);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task ListRecordsAsync_ProtocolOwnedInterruptedFinalAppend_RecoversExactlyOneRecord()
    {
        var root = NewTempRoot("visual-cost-ledger-recover-owned-partial");
        var sourceRoot = NewTempRoot("visual-cost-ledger-recover-line-source");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);

        try
        {
            var seed = new JsonlDesignConversionVisualCostLedger(root);
            var first = await seed.AppendRecordAsync(BuildValidRecord("recover-owned-existing"));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var prefixBytes = await File.ReadAllBytesAsync(recordsPath);

            var candidate = await new JsonlDesignConversionVisualCostLedger(sourceRoot)
                .AppendRecordAsync(BuildValidRecord("recover-owned-appended"));
            Assert.True(candidate.IsSuccess, candidate.ErrorMessage);
            var candidateLineBytes = SerializeJsonlLine(candidate.Value!);
            var interruptedByteCount = Math.Max(1, candidateLineBytes.Length / 2);
            await DesignConversionBoundedJsonlAppendJournal.PublishInterruptedAppendForTestingAsync(
                root,
                recordsPath,
                JsonlDesignConversionVisualCostLedger.RecordsFileName,
                candidateLineBytes,
                prefixBytes.LongLength,
                interruptedByteCount);

            var recovered = await new JsonlDesignConversionVisualCostLedger(root).ListRecordsAsync();
            var secondRead = await new JsonlDesignConversionVisualCostLedger(root).ListRecordsAsync();
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            Assert.True(recovered.IsSuccess, recovered.ErrorMessage);
            Assert.Equal(2, recovered.Value!.Count);
            Assert.True(secondRead.IsSuccess, secondRead.ErrorMessage);
            Assert.Equal(2, secondRead.Value!.Count);
            AssertBytePrefix(prefixBytes, afterBytes);
            Assert.Equal(candidateLineBytes, afterBytes.Skip(prefixBytes.Length).ToArray());
            Assert.Empty(Directory.EnumerateFiles(root, $"{JsonlDesignConversionVisualCostLedger.RecordsFileName}*.bounded-append.intent*"));
        }
        finally
        {
            DeleteTempRoot(root);
            DeleteTempRoot(sourceRoot);
        }
    }

    [Fact]
    public async Task ListRecordsAsync_ProtocolOwnedAppendBeyondConfiguredQuota_FailsClosedAndPreservesIntent()
    {
        var root = NewTempRoot("visual-cost-ledger-recover-over-quota");
        var sourceRoot = NewTempRoot("visual-cost-ledger-recover-over-quota-source");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);
        var intentPath = recordsPath + ".bounded-append.intent";

        try
        {
            var seed = new JsonlDesignConversionVisualCostLedger(root);
            var first = await seed.AppendRecordAsync(BuildValidRecord("recover-over-quota-existing"));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var prefixBytes = await File.ReadAllBytesAsync(recordsPath);

            var candidate = await new JsonlDesignConversionVisualCostLedger(sourceRoot)
                .AppendRecordAsync(BuildValidRecord("recover-over-quota-appended"));
            Assert.True(candidate.IsSuccess, candidate.ErrorMessage);
            var candidateLineBytes = SerializeJsonlLine(candidate.Value!);
            var maxJournalBytes = checked(prefixBytes.LongLength + candidateLineBytes.LongLength - 1L);
            await DesignConversionBoundedJsonlAppendJournal.PublishInterruptedAppendForTestingAsync(
                root,
                recordsPath,
                JsonlDesignConversionVisualCostLedger.RecordsFileName,
                candidateLineBytes,
                prefixBytes.LongLength,
                appendedByteCount: 1);
            var expectedBytes = await File.ReadAllBytesAsync(recordsPath);
            Assert.True(File.Exists(intentPath));

            var result = await new JsonlDesignConversionVisualCostLedger(root, maxJournalBytes).ListRecordsAsync();
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            Assert.False(result.IsSuccess);
            Assert.Equal(JsonlDesignConversionVisualCostLedger.ReadFailedErrorCode, result.ErrorCode);
            Assert.Equal(SafeReadFailureMessage, result.ErrorMessage);
            Assert.Equal(expectedBytes, afterBytes);
            Assert.True(File.Exists(intentPath));
        }
        finally
        {
            DeleteTempRoot(root);
            DeleteTempRoot(sourceRoot);
        }
    }

    [Fact]
    public async Task ListRecordsAsync_OversizedStableAppendIntent_FailsClosedAndPreservesJournalAndIntent()
    {
        var root = NewTempRoot("visual-cost-ledger-oversized-stable-intent");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);
        var intentPath = recordsPath + ".bounded-append.intent";

        try
        {
            var seed = new JsonlDesignConversionVisualCostLedger(root);
            var first = await seed.AppendRecordAsync(BuildValidRecord("oversized-stable-intent-existing"));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var expectedJournalBytes = await File.ReadAllBytesAsync(recordsPath);
            var maxJournalBytes = checked(expectedJournalBytes.LongLength + 32L);
            var oversizedIntentBytes = Encoding.UTF8.GetBytes(
                "{\"RecordBase64\":\"" + new string('A', 64 * 1024) + "\"}");
            Assert.True(oversizedIntentBytes.LongLength > maxJournalBytes + 4096L);
            await File.WriteAllBytesAsync(intentPath, oversizedIntentBytes);

            var result = await new JsonlDesignConversionVisualCostLedger(root, maxJournalBytes).ListRecordsAsync();
            var afterJournalBytes = await File.ReadAllBytesAsync(recordsPath);
            var afterIntentBytes = await File.ReadAllBytesAsync(intentPath);

            Assert.False(result.IsSuccess);
            Assert.Equal(JsonlDesignConversionVisualCostLedger.ReadFailedErrorCode, result.ErrorCode);
            Assert.Equal(SafeReadFailureMessage, result.ErrorMessage);
            Assert.Equal(expectedJournalBytes, afterJournalBytes);
            Assert.Equal(oversizedIntentBytes, afterIntentBytes);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Theory]
    [InlineData(1, "AAAA")]
    [InlineData(2, "AAAA")]
    public async Task ListRecordsAsync_StableIntentBase64DecodesPastDeclaredCount_FailsClosedAndPreservesJournalAndIntent(
        int declaredRecordByteCount,
        string forgedRecordBase64)
    {
        var root = NewTempRoot("visual-cost-ledger-base64-declared-count");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);
        var intentPath = recordsPath + ".bounded-append.intent";

        try
        {
            var seed = new JsonlDesignConversionVisualCostLedger(root);
            var first = await seed.AppendRecordAsync(BuildValidRecord($"base64-declared-count-{declaredRecordByteCount}"));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var expectedJournalBytes = await File.ReadAllBytesAsync(recordsPath);
            var intentBytes = BuildStableAppendIntentBytes(
                expectedJournalBytes.LongLength,
                declaredRecordByteCount,
                forgedRecordBase64);
            await File.WriteAllBytesAsync(intentPath, intentBytes);

            var result = await new JsonlDesignConversionVisualCostLedger(
                    root,
                    checked(expectedJournalBytes.LongLength + declaredRecordByteCount))
                .ListRecordsAsync();
            var afterJournalBytes = await File.ReadAllBytesAsync(recordsPath);
            var afterIntentBytes = await File.ReadAllBytesAsync(intentPath);

            Assert.False(result.IsSuccess);
            Assert.Equal(JsonlDesignConversionVisualCostLedger.ReadFailedErrorCode, result.ErrorCode);
            Assert.Equal(SafeReadFailureMessage, result.ErrorMessage);
            Assert.Equal(expectedJournalBytes, afterJournalBytes);
            Assert.Equal(intentBytes, afterIntentBytes);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_ExistingJournalWithoutLineTerminator_FailsClosedAndPreservesBytes()
    {
        var root = NewTempRoot("visual-cost-ledger-unterminated-existing-journal");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);

        try
        {
            var seed = new JsonlDesignConversionVisualCostLedger(root);
            var first = await seed.AppendRecordAsync(BuildValidRecord("unterminated-existing"));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var terminatedBytes = await File.ReadAllBytesAsync(recordsPath);
            var contentLength = terminatedBytes.Length;
            while (contentLength > 0 && terminatedBytes[contentLength - 1] is (byte)'\r' or (byte)'\n')
                contentLength--;
            var expectedBytes = terminatedBytes.AsSpan(0, contentLength).ToArray();
            await File.WriteAllBytesAsync(recordsPath, expectedBytes);

            var result = await new JsonlDesignConversionVisualCostLedger(root)
                .AppendRecordAsync(BuildValidRecord("unterminated-new"));
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            Assert.False(result.IsSuccess);
            Assert.Equal(JsonlDesignConversionVisualCostLedger.AppendFailedErrorCode, result.ErrorCode);
            Assert.Equal(expectedBytes, afterBytes);
            Assert.False(File.Exists(recordsPath + ".bounded-append.intent"));
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task ListRecordsAsync_ProtocolOwnedAppendAfterDuplicateIdentityPrefix_FailsClosedWithoutRecoveryMutation()
    {
        var root = NewTempRoot("visual-cost-ledger-recover-duplicate-prefix");
        var sourceRoot = NewTempRoot("visual-cost-ledger-recover-duplicate-prefix-source");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);
        var intentPath = recordsPath + ".bounded-append.intent";

        try
        {
            var seed = new JsonlDesignConversionVisualCostLedger(root);
            var first = await seed.AppendRecordAsync(BuildValidRecord("recover-duplicate-existing"));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var firstLineBytes = await File.ReadAllBytesAsync(recordsPath);
            var duplicatePrefixBytes = firstLineBytes.Concat(firstLineBytes).ToArray();
            await File.WriteAllBytesAsync(recordsPath, duplicatePrefixBytes);

            var candidate = await new JsonlDesignConversionVisualCostLedger(sourceRoot)
                .AppendRecordAsync(BuildValidRecord("recover-duplicate-appended"));
            Assert.True(candidate.IsSuccess, candidate.ErrorMessage);
            var candidateLineBytes = SerializeJsonlLine(candidate.Value!);
            await DesignConversionBoundedJsonlAppendJournal.PublishInterruptedAppendForTestingAsync(
                root,
                recordsPath,
                JsonlDesignConversionVisualCostLedger.RecordsFileName,
                candidateLineBytes,
                duplicatePrefixBytes.LongLength,
                appendedByteCount: 1);
            var expectedBytes = await File.ReadAllBytesAsync(recordsPath);
            Assert.True(File.Exists(intentPath));

            var result = await new JsonlDesignConversionVisualCostLedger(root).ListRecordsAsync();
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            Assert.False(result.IsSuccess);
            Assert.Equal(JsonlDesignConversionVisualCostLedger.ReadFailedErrorCode, result.ErrorCode);
            Assert.Equal(expectedBytes, afterBytes);
            Assert.True(File.Exists(intentPath));
        }
        finally
        {
            DeleteTempRoot(root);
            DeleteTempRoot(sourceRoot);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_UnownedIncompleteTail_FailsClosedAndPreservesBytes()
    {
        var root = NewTempRoot("visual-cost-ledger-unowned-tail");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);

        try
        {
            var seed = new JsonlDesignConversionVisualCostLedger(root);
            var first = await seed.AppendRecordAsync(BuildValidRecord("unowned-tail-existing"));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var prefixBytes = await File.ReadAllBytesAsync(recordsPath);
            var corruptTail = Encoding.UTF8.GetBytes("{\"recordId\":\"not-finished\"");
            await File.WriteAllBytesAsync(recordsPath, prefixBytes.Concat(corruptTail).ToArray());
            var expectedBytes = await File.ReadAllBytesAsync(recordsPath);

            var append = await new JsonlDesignConversionVisualCostLedger(root)
                .AppendRecordAsync(BuildValidRecord("unowned-tail-new"));
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            Assert.False(append.IsSuccess);
            Assert.Equal(JsonlDesignConversionVisualCostLedger.ReadFailedErrorCode, append.ErrorCode);
            Assert.Equal(expectedBytes, afterBytes);
            Assert.Empty(Directory.EnumerateFiles(root, $"{JsonlDesignConversionVisualCostLedger.RecordsFileName}*.bounded-append.intent*"));
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    private static byte[] BuildInvalidUtf8JsonlBytes(string privateMarker)
    {
        var bytes = new List<byte>();
        bytes.AddRange(Encoding.ASCII.GetBytes("{\"marker\":\"" + privateMarker + "\"}"));
        bytes.AddRange(new byte[] { 0x0A, 0xC3, 0x28, 0x0A });
        bytes.AddRange(Encoding.ASCII.GetBytes(privateMarker));
        return bytes.ToArray();
    }

    private static byte[] BuildStableAppendIntentBytes(
        long startOffset,
        int recordByteCount,
        string recordBase64)
    {
        var declaredBytes = new byte[recordByteCount];
        var intent = new
        {
            Magic = "design-conversion-bounded-jsonl-append-intent",
            Version = 1,
            RecordsFileName = JsonlDesignConversionVisualCostLedger.RecordsFileName,
            StartOffset = startOffset,
            RecordByteCount = recordByteCount,
            RecordSha256 = Convert.ToHexString(SHA256.HashData(declaredBytes)).ToLowerInvariant(),
            RecordBase64 = recordBase64
        };

        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(intent));
    }

    private static byte[] SerializeJsonlLine(DesignConversionVisualCostLedgerRecord record)
    {
        var jsonOptions = new JsonSerializerOptions(DesignConversionCheckpointJsonOptions.Standard)
        {
            WriteIndented = false
        };
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(record, jsonOptions) + Environment.NewLine);
    }

    private static SortedDictionary<string, byte[]> SnapshotJournalFileBytes(string root)
    {
        var snapshot = new SortedDictionary<string, byte[]>(StringComparer.Ordinal);
        foreach (var path in Directory
                     .EnumerateFiles(root)
                     .Where(path =>
                     {
                         var fileName = Path.GetFileName(path);
                         return string.Equals(fileName, JsonlDesignConversionVisualCostLedger.RecordsFileName, StringComparison.Ordinal) ||
                             fileName.StartsWith(JsonlDesignConversionVisualCostLedger.SegmentFileNamePrefix, StringComparison.Ordinal);
                     })
                     .OrderBy(Path.GetFileName, StringComparer.Ordinal))
        {
            snapshot.Add(Path.GetFileName(path), File.ReadAllBytes(path));
        }

        return snapshot;
    }

    private static void AssertBytePrefix(byte[] expectedPrefix, byte[] actual)
    {
        Assert.True(actual.Length > expectedPrefix.Length);
        Assert.Equal(expectedPrefix, actual.Take(expectedPrefix.Length).ToArray());
    }

    private static DesignConversionVisualCostLedgerRecord BuildValidRecord(string suffix)
    {
        var recordedAtUtc = DateTimeOffset.UnixEpoch.AddDays(1);
        var period = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(recordedAtUtc);
        var userRef = JsonlDesignConversionVisualCostLedger
            .BuildPseudonymousUserRef("visual-cost-ledger-user-" + suffix)
            .Value!;

        return new DesignConversionVisualCostLedgerRecord(
            RecordId: "pending",
            LedgerKind: JsonlDesignConversionVisualCostLedger.LedgerKind,
            SchemaVersion: JsonlDesignConversionVisualCostLedger.SchemaVersion,
            EvaluationJobRef: JsonlDesignConversionVisualCostLedger.BuildCanonicalEvaluationJobRef("job-" + suffix),
            SubmittedRequestRef: JsonlDesignConversionVisualCostLedger.BuildCanonicalSubmittedRequestRef("request-" + suffix),
            TotalCostUsd: 0.125m,
            MeasurementComplete: true,
            MissingMeasurementCount: 0,
            EvaluationAttemptCount: 1,
            CostSource: DesignConversionVisualCostAccounting.UnknownCostSource,
            RecordedAtUtc: recordedAtUtc,
            EvidenceRefs: [],
            Metadata: new Dictionary<string, string>()) with
        {
            UserRef = userRef,
            SubmittedTier = "paid",
            PeriodKind = JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind,
            PeriodStartUtc = period.StartUtc,
            PeriodEndExclusiveUtc = period.EndExclusiveUtc
        };
    }

    private static void AssertVisualCostLedgerReadFailure<T>(
        OperationResult<T> result,
        string root,
        string recordsPath,
        string privateMarker)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.ReadFailedErrorCode, result.ErrorCode);
        Assert.Equal(SafeReadFailureMessage, result.ErrorMessage);
        Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(recordsPath, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/lubao/private", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(JsonlDesignConversionVisualCostLedger.RecordsFileName, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(nameof(DecoderFallbackException), result.ErrorMessage, StringComparison.Ordinal);
        Assert.Null(result.Value);
    }

    private static void AssertSameDurableVisualCostRecord(
        DesignConversionVisualCostLedgerRecord expected,
        DesignConversionVisualCostLedgerRecord actual)
    {
        Assert.Equal(expected.RecordId, actual.RecordId);
        Assert.Equal(expected.LedgerKind, actual.LedgerKind);
        Assert.Equal(expected.SchemaVersion, actual.SchemaVersion);
        Assert.Equal(expected.EvaluationJobRef, actual.EvaluationJobRef);
        Assert.Equal(expected.SubmittedRequestRef, actual.SubmittedRequestRef);
        Assert.Equal(expected.TotalCostUsd, actual.TotalCostUsd);
        Assert.Equal(expected.MeasurementComplete, actual.MeasurementComplete);
        Assert.Equal(expected.MissingMeasurementCount, actual.MissingMeasurementCount);
        Assert.Equal(expected.EvaluationAttemptCount, actual.EvaluationAttemptCount);
        Assert.Equal(expected.CostSource, actual.CostSource);
        Assert.Equal(expected.RecordedAtUtc, actual.RecordedAtUtc);
        Assert.Equal(expected.EvidenceRefs, actual.EvidenceRefs);
        Assert.Equal(expected.Metadata, actual.Metadata);
        Assert.Equal(expected.UserRef, actual.UserRef);
        Assert.Equal(expected.SubmittedTier, actual.SubmittedTier);
        Assert.Equal(expected.PeriodKind, actual.PeriodKind);
        Assert.Equal(expected.PeriodStartUtc, actual.PeriodStartUtc);
        Assert.Equal(expected.PeriodEndExclusiveUtc, actual.PeriodEndExclusiveUtc);
    }

    private static void AssertVisualCostLedgerQuotaFailure<T>(
        OperationResult<T> result,
        string root,
        string recordsPath)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.JournalQuotaExceededErrorCode, result.ErrorCode);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.JournalQuotaExceededSafeMessage, result.ErrorMessage);
        Assert.DoesNotContain(root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(recordsPath, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/lubao/private", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(JsonlDesignConversionVisualCostLedger.RecordsFileName, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Null(result.Value);
    }

    private static string NewTempRoot(string prefix)
        => Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), prefix);

    private static void DeleteTempRoot(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    private static void AssertNoLedgerAppendArtifacts(string root)
    {
        Assert.Empty(Directory.EnumerateFiles(
            root,
            $"{JsonlDesignConversionVisualCostLedger.RecordsFileName}.*.append.tmp"));
        Assert.Empty(Directory.EnumerateFiles(
            root,
            $"{JsonlDesignConversionVisualCostLedger.RecordsFileName}*.bounded-append.intent*"));
    }
}

internal sealed class DesignConversionJournalReparsePointFactAttribute : FactAttribute
{
    public DesignConversionJournalReparsePointFactAttribute()
        => Skip = DesignConversionJournalReparsePointTestSupport.GetCapabilitySkipReason();
}

internal static class DesignConversionJournalReparsePointTestSupport
{
    private const int HResultPrivilegeNotHeld = unchecked((int)0x80070522);

    public static string? GetCapabilitySkipReason()
    {
        if (!OperatingSystem.IsWindows())
            return null;

        var root = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-journal-reparse-capability-{Guid.NewGuid():N}");
        var targetFile = Path.Combine(root, "target.jsonl");
        var fileLink = Path.Combine(root, "link.jsonl");
        var targetDirectory = Path.Combine(root, "target-directory");
        var directoryLink = Path.Combine(root, "link-directory");
        try
        {
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(targetDirectory);
            File.WriteAllText(targetFile, "{}");
            try
            {
                File.CreateSymbolicLink(fileLink, targetFile);
                Directory.CreateSymbolicLink(directoryLink, targetDirectory);
                return null;
            }
            catch (Exception exception)
                when (exception is IOException or UnauthorizedAccessException &&
                      exception.HResult == HResultPrivilegeNotHeld)
            {
                return "REPARSE_TEST_PLATFORM_LIMITATION: " +
                    $"{exception.GetType().FullName}, HResult=0x{exception.HResult:X8}: " +
                    exception.Message;
            }
        }
        finally
        {
            if (File.Exists(fileLink))
                File.Delete(fileLink);
            if (Directory.Exists(directoryLink))
                Directory.Delete(directoryLink);
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }
}
