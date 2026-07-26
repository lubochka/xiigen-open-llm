// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
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
    public async Task AppendRecordAsync_DistinctRecordAtConfiguredLimit_FailsQuotaAndPreservesBytes()
    {
        var root = NewTempRoot("visual-cost-ledger-append-quota-distinct");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);

        try
        {
            var seed = new JsonlDesignConversionVisualCostLedger(root);
            var appended = await seed.AppendRecordAsync(BuildValidRecord("append-quota-seed"));
            Assert.True(appended.IsSuccess, appended.ErrorMessage);
            var expectedBytes = await File.ReadAllBytesAsync(recordsPath);

            var result = await new JsonlDesignConversionVisualCostLedger(root, expectedBytes.LongLength)
                .AppendRecordAsync(BuildValidRecord("append-quota-distinct"));
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
    public async Task ListRecordsAsync_InvalidUtf8Ledger_ReturnsSafeFailureAndPreservesBytes()
    {
        var root = NewTempRoot("visual-cost-ledger-list-invalid-utf8");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName);
        const string privateMarker = "PRIVATE_MARKER_C:/Users/private-user/private/visual-cost-ledger.jsonl#list";
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
        const string privateMarker = "PRIVATE_MARKER_C:/Users/private-user/private/visual-cost-ledger.jsonl#aggregate";
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
        const string privateMarker = "PRIVATE_MARKER_C:/Users/private-user/private/visual-cost-ledger.jsonl#append";
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

    private static byte[] BuildInvalidUtf8JsonlBytes(string privateMarker)
    {
        var bytes = new List<byte>();
        bytes.AddRange(Encoding.ASCII.GetBytes("{\"marker\":\"" + privateMarker + "\"}"));
        bytes.AddRange(new byte[] { 0x0A, 0xC3, 0x28, 0x0A });
        bytes.AddRange(Encoding.ASCII.GetBytes(privateMarker));
        return bytes.ToArray();
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
        Assert.DoesNotContain("C:/Users/", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
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
        Assert.DoesNotContain("C:/Users/", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
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
}
