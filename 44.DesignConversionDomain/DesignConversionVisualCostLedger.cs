// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Collections.Concurrent;
using System.Globalization;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;

public interface IDesignConversionVisualCostLedger
{
    Task<OperationResult<DesignConversionVisualCostLedgerRecord>> AppendRecordAsync(
        DesignConversionVisualCostLedgerRecord record,
        CancellationToken cancellationToken = default);

    Task<OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>> ListRecordsAsync(
        CancellationToken cancellationToken = default);

    Task<OperationResult<DesignConversionVisualCostLedgerPeriodAggregate>> AggregatePeriodAsync(
        string userRef,
        DateTimeOffset periodStartUtc,
        DateTimeOffset periodEndExclusiveUtc,
        CancellationToken cancellationToken = default);
}

public sealed record DesignConversionVisualCostLedgerRecord(
    string RecordId,
    string LedgerKind,
    string SchemaVersion,
    string EvaluationJobRef,
    string SubmittedRequestRef,
    decimal TotalCostUsd,
    bool MeasurementComplete,
    int MissingMeasurementCount,
    int EvaluationAttemptCount,
    string CostSource,
    DateTimeOffset RecordedAtUtc,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> Metadata)
{
    public string UserRef { get; init; } = JsonlDesignConversionVisualCostLedger.LegacyUnattributedUserRef;
    public string SubmittedTier { get; init; } = JsonlDesignConversionVisualCostLedger.UnknownSubmittedTier;
    public string PeriodKind { get; init; } = JsonlDesignConversionVisualCostLedger.NotPeriodEnforceablePeriodKind;
    public DateTimeOffset PeriodStartUtc { get; init; } = DateTimeOffset.UnixEpoch;
    public DateTimeOffset PeriodEndExclusiveUtc { get; init; } = DateTimeOffset.UnixEpoch;
}

public sealed record DesignConversionVisualCostLedgerPeriodAggregate(
    string UserRef,
    string PeriodKind,
    DateTimeOffset PeriodStartUtc,
    DateTimeOffset PeriodEndExclusiveUtc,
    decimal TotalMeasuredCostUsd,
    int RecordCount,
    bool MeasurementComplete,
    bool HasMissingMeasurements,
    int MissingMeasurementCount,
    int IncompleteRecordCount);

public sealed class JsonlDesignConversionVisualCostLedger : IDesignConversionVisualCostLedger
{
    public const string LedgerKind = "design_conversion_visual_cost_operational_ledger";
    public const string SchemaVersion = "design-conversion-visual-cost-ledger-v1";
    public const string RecordsFileName = "visual-cost-ledger.jsonl";
    public const string SegmentFileNamePrefix = RecordsFileName + ".segment-";
    public const long DefaultMaxJournalBytes = 1L * 1024L * 1024L * 1024L;
    public const string AppendFailedErrorCode = "DESIGN_CONVERSION_VISUAL_COST_LEDGER_APPEND_FAILED";
    public const string AppendFailedSafeMessage = "DesignConversion visual-cost ledger record could not be written.";
    public const string ReadFailedErrorCode = "DESIGN_CONVERSION_VISUAL_COST_LEDGER_READ_FAILED";
    public const string JournalQuotaExceededErrorCode = "DESIGN_CONVERSION_VISUAL_COST_LEDGER_JOURNAL_QUOTA_EXCEEDED";
    public const string JournalQuotaExceededSafeMessage = "DesignConversion visual-cost ledger journal quota was exceeded.";
    public const string RecordRequiredErrorCode = "DESIGN_CONVERSION_VISUAL_COST_LEDGER_RECORD_REQUIRED";
    public const string InvalidRecordErrorCode = "DESIGN_CONVERSION_VISUAL_COST_LEDGER_RECORD_INVALID";
    public const string CancelledErrorCode = "CANCELLED";
    public const string RecordIdPrefix = "dcm-visual-cost-ledger-record";
    public const string EvaluationJobRefPrefix = "dcm-visual-cost-ledger-job";
    public const string SubmittedRequestRefPrefix = "dcm-visual-cost-ledger-request";
    public const string EvidenceRefPrefix = "dcm-visual-cost-ledger-evidence";
    public const string UserRefPrefix = "dcm-visual-cost-ledger-userref";
    public const string UserRefDomainSeparator = "design-conversion.visual-cost-ledger.user-ref.v1";
    public const string LegacyUnattributedUserRef = UserRefPrefix + "-unattributed-legacy";
    public const string UtcCalendarMonthPeriodKind = "utc_calendar_month";
    public const string NotPeriodEnforceablePeriodKind = "unattributed_not_period_enforceable";
    public const string UnknownSubmittedTier = "unknown";
    public const string UnrecognizedSubmittedTier = "unrecognized_submitted_context";
    public const string VisualCostLedgerRootEnvironmentVariable = "DESIGN_CONVERSION_VISUAL_COST_LEDGER_ROOT";
    public const string UserRefSourceRequiredErrorCode = "DESIGN_CONVERSION_VISUAL_COST_LEDGER_USER_REF_SOURCE_REQUIRED";
    public const string InvalidUserRefErrorCode = "DESIGN_CONVERSION_VISUAL_COST_LEDGER_USER_REF_INVALID";
    public const string InvalidPeriodErrorCode = "DESIGN_CONVERSION_VISUAL_COST_LEDGER_PERIOD_INVALID";

    private static readonly JsonSerializerOptions JsonOptions = new(DesignConversionCheckpointJsonOptions.Standard)
    {
        WriteIndented = false
    };

    private static readonly Encoding StrictUtf8Encoding = new UTF8Encoding(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> MutationGates = new(GetPathKeyComparer());

    private readonly string _rootDirectory;
    private readonly string _recordsPath;
    private readonly long _maxJournalBytes;
    private readonly SemaphoreSlim _gate;

    public JsonlDesignConversionVisualCostLedger(
        string rootDirectory,
        long maxJournalBytes = DefaultMaxJournalBytes)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A visual-cost ledger root is required.", nameof(rootDirectory));
        if (maxJournalBytes <= 0L)
            throw new ArgumentOutOfRangeException(
                nameof(maxJournalBytes),
                "A visual-cost ledger journal size limit must be positive.");

        _rootDirectory = Path.GetFullPath(rootDirectory);
        _recordsPath = Path.Combine(_rootDirectory, RecordsFileName);
        _maxJournalBytes = maxJournalBytes;
        _gate = MutationGates.GetOrAdd(NormalizeMutationGateKey(_recordsPath), _ => new SemaphoreSlim(1, 1));
    }

    public string RootDirectory => _rootDirectory;

    public string RecordsPath => _recordsPath;

    public long MaxJournalBytes => _maxJournalBytes;

    public static JsonlDesignConversionVisualCostLedger CreateDefault()
        => new(ResolveDefaultRoot(
            Environment.GetEnvironmentVariable(VisualCostLedgerRootEnvironmentVariable),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)));

    internal static string ResolveDefaultRoot(
        string? configuredRoot,
        string? localApplicationDataRoot)
    {
        if (!string.IsNullOrWhiteSpace(configuredRoot))
        {
            if (!Path.IsPathFullyQualified(configuredRoot))
            {
                throw new InvalidOperationException(
                    "DesignConversion visual-cost ledger configured root must be fully qualified.");
            }

            return Path.GetFullPath(configuredRoot);
        }

        if (string.IsNullOrWhiteSpace(localApplicationDataRoot) ||
            !Path.IsPathFullyQualified(localApplicationDataRoot))
        {
            throw new InvalidOperationException(
                "DesignConversion visual-cost ledger default root requires a fully qualified LocalApplicationData directory.");
        }

        return Path.Combine(
            Path.GetFullPath(localApplicationDataRoot),
            "llm_mvp_core",
            "design-conversion",
            "visual-cost-ledger");
    }

    public async Task<OperationResult<DesignConversionVisualCostLedgerRecord>> AppendRecordAsync(
        DesignConversionVisualCostLedgerRecord record,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled<DesignConversionVisualCostLedgerRecord>();
        if (record is null)
            return OperationResult<DesignConversionVisualCostLedgerRecord>.Fail(
                "DesignConversion visual-cost ledger record is required.",
                RecordRequiredErrorCode);

        var normalized = TryNormalizeAppendRecord(record);
        if (!normalized.IsSuccess)
            return normalized;
        string line;
        try
        {
            line = JsonSerializer.Serialize(normalized.Value!, JsonOptions);
        }
        catch (JsonException)
        {
            return AppendFailed<DesignConversionVisualCostLedgerRecord>();
        }

        try
        {
            await _gate.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return Cancelled<DesignConversionVisualCostLedgerRecord>();
        }

        try
        {
            var existing = await FindExistingRecordAsync(normalized.Value!.RecordId, cancellationToken);
            if (!existing.IsSuccess)
                return existing.MapError<DesignConversionVisualCostLedgerRecord>();
            if (existing.Value is not null)
            {
                if (!HasSameDurableRecordDimensions(existing.Value, normalized.Value!))
                {
                    return OperationResult<DesignConversionVisualCostLedgerRecord>.Fail(
                        "DesignConversion visual-cost ledger record identity conflicts with existing durable dimensions.",
                        InvalidRecordErrorCode);
                }

                return OperationResult<DesignConversionVisualCostLedgerRecord>.Ok(existing.Value);
            }

            var pathBoundary = EnsureJournalPathBoundaryNoLock();
            if (!pathBoundary.IsSuccess)
                return pathBoundary.MapError<DesignConversionVisualCostLedgerRecord>();

            Directory.CreateDirectory(_rootDirectory);
            pathBoundary = EnsureJournalPathBoundaryNoLock();
            if (!pathBoundary.IsSuccess)
                return pathBoundary.MapError<DesignConversionVisualCostLedgerRecord>();

            var appendedBytes = StrictUtf8Encoding.GetBytes(line + Environment.NewLine);
            var quota = EnsureAppendOrRolloverNoLock(appendedBytes);
            if (!quota.IsSuccess)
                return quota.MapError<DesignConversionVisualCostLedgerRecord>();

            var append = await DesignConversionBoundedJsonlAppendJournal.AppendNoLockAsync(
                _rootDirectory,
                _recordsPath,
                RecordsFileName,
                appendedBytes,
                TryGetRecoverableRecordIdentity,
                cancellationToken);
            if (!append)
                return AppendFailed<DesignConversionVisualCostLedgerRecord>();

            return OperationResult<DesignConversionVisualCostLedgerRecord>.Ok(normalized.Value);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<DesignConversionVisualCostLedgerRecord>();
        }
        catch (DecoderFallbackException)
        {
            return ReadFailed<DesignConversionVisualCostLedgerRecord>();
        }
        catch (JsonException)
        {
            return AppendFailed<DesignConversionVisualCostLedgerRecord>();
        }
        catch (IOException)
        {
            return AppendFailed<DesignConversionVisualCostLedgerRecord>();
        }
        catch (UnauthorizedAccessException)
        {
            return AppendFailed<DesignConversionVisualCostLedgerRecord>();
        }
        catch (SecurityException)
        {
            return AppendFailed<DesignConversionVisualCostLedgerRecord>();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>> ListRecordsAsync(
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();

        try
        {
            await _gate.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return Cancelled<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();
        }

        try
        {
            var records = await ReadRecordsNoLockAsync(cancellationToken);
            return records.IsSuccess
                ? OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>.Ok(records.Value!)
                : records.MapError<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();
        }
        catch (DecoderFallbackException)
        {
            return ReadFailed<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();
        }
        catch (IOException)
        {
            return ReadFailed<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();
        }
        catch (UnauthorizedAccessException)
        {
            return ReadFailed<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();
        }
        catch (SecurityException)
        {
            return ReadFailed<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<OperationResult<DesignConversionVisualCostLedgerPeriodAggregate>> AggregatePeriodAsync(
        string userRef,
        DateTimeOffset periodStartUtc,
        DateTimeOffset periodEndExclusiveUtc,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled<DesignConversionVisualCostLedgerPeriodAggregate>();
        if (!IsTechnicalUserRef(userRef))
        {
            return OperationResult<DesignConversionVisualCostLedgerPeriodAggregate>.Fail(
                "DesignConversion visual-cost ledger aggregate requires a canonical technical user ref.",
                InvalidUserRefErrorCode);
        }
        if (!IsExactUtcCalendarMonthPeriod(periodStartUtc, periodEndExclusiveUtc))
        {
            return OperationResult<DesignConversionVisualCostLedgerPeriodAggregate>.Fail(
                "DesignConversion visual-cost ledger aggregate requires an exact UTC calendar-month period.",
                InvalidPeriodErrorCode);
        }

        try
        {
            await _gate.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return Cancelled<DesignConversionVisualCostLedgerPeriodAggregate>();
        }

        try
        {
            var records = await ReadRecordsNoLockAsync(cancellationToken);
            if (!records.IsSuccess)
                return records.MapError<DesignConversionVisualCostLedgerPeriodAggregate>();

            var matches = records.Value!
                .Where(record =>
                    string.Equals(record.UserRef, userRef, StringComparison.Ordinal) &&
                    string.Equals(record.PeriodKind, UtcCalendarMonthPeriodKind, StringComparison.Ordinal) &&
                    record.PeriodStartUtc == periodStartUtc &&
                    record.PeriodEndExclusiveUtc == periodEndExclusiveUtc)
                .ToArray();
            var missingMeasurementCount = matches.Sum(record => Math.Max(0, record.MissingMeasurementCount));
            var incompleteRecordCount = matches.Count(record =>
                !record.MeasurementComplete || record.MissingMeasurementCount > 0);

            return OperationResult<DesignConversionVisualCostLedgerPeriodAggregate>.Ok(
                new DesignConversionVisualCostLedgerPeriodAggregate(
                    UserRef: userRef,
                    PeriodKind: UtcCalendarMonthPeriodKind,
                    PeriodStartUtc: periodStartUtc,
                    PeriodEndExclusiveUtc: periodEndExclusiveUtc,
                    TotalMeasuredCostUsd: matches.Sum(record => record.TotalCostUsd),
                    RecordCount: matches.Length,
                    MeasurementComplete: matches.Length > 0 && incompleteRecordCount == 0,
                    HasMissingMeasurements: missingMeasurementCount > 0,
                    MissingMeasurementCount: missingMeasurementCount,
                    IncompleteRecordCount: incompleteRecordCount));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<DesignConversionVisualCostLedgerPeriodAggregate>();
        }
        catch (DecoderFallbackException)
        {
            return ReadFailed<DesignConversionVisualCostLedgerPeriodAggregate>();
        }
        catch (IOException)
        {
            return ReadFailed<DesignConversionVisualCostLedgerPeriodAggregate>();
        }
        catch (UnauthorizedAccessException)
        {
            return ReadFailed<DesignConversionVisualCostLedgerPeriodAggregate>();
        }
        catch (SecurityException)
        {
            return ReadFailed<DesignConversionVisualCostLedgerPeriodAggregate>();
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<OperationResult<DesignConversionVisualCostLedgerRecord?>> FindExistingRecordAsync(
        string recordId,
        CancellationToken cancellationToken)
    {
        var records = await ReadRecordsNoLockAsync(cancellationToken);
        if (!records.IsSuccess)
            return records.MapError<DesignConversionVisualCostLedgerRecord?>();

        var matches = records.Value!
            .Where(record => string.Equals(record.RecordId, recordId, StringComparison.Ordinal))
            .ToArray();
        return matches.Length switch
        {
            0 => OperationResult<DesignConversionVisualCostLedgerRecord?>.Ok(null),
            1 => OperationResult<DesignConversionVisualCostLedgerRecord?>.Ok(matches[0]),
            _ => OperationResult<DesignConversionVisualCostLedgerRecord?>.Fail(
                "DesignConversion visual-cost ledger contains duplicate record identities.",
                AppendFailedErrorCode)
        };
    }

    private async Task<OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>> ReadRecordsNoLockAsync(
        CancellationToken cancellationToken)
    {
        var pathBoundary = EnsureJournalPathBoundaryNoLock();
        if (!pathBoundary.IsSuccess)
            return pathBoundary.MapError<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();

        var recovery = await RecoverBoundedAppendNoLockAsync(cancellationToken);
        if (!recovery.IsSuccess)
            return recovery.MapError<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();

        var journalPaths = TryGetReadableJournalPathsNoLock();
        if (!journalPaths.IsSuccess)
            return journalPaths.MapError<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();

        var records = new List<DesignConversionVisualCostLedgerRecord>();
        var recordIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var journalPath in journalPaths.Value!)
        {
            var quota = EnsureExistingJournalWithinQuotaNoLock(journalPath);
            if (!quota.IsSuccess)
                return quota.MapError<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();

            string[] lines;
            try
            {
                lines = await File.ReadAllLinesAsync(journalPath, StrictUtf8Encoding, cancellationToken);
            }
            catch (DecoderFallbackException)
            {
                return ReadFailed<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();
            }

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    return ReadFailed<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();

                DesignConversionVisualCostLedgerRecord? record;
                try
                {
                    record = JsonSerializer.Deserialize<DesignConversionVisualCostLedgerRecord>(line, JsonOptions);
                }
                catch (JsonException)
                {
                    return ReadFailed<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();
                }

                if (record is null)
                    return ReadFailed<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();

                var normalized = TryNormalizeReadRecord(record);
                if (!normalized.IsSuccess || !recordIds.Add(normalized.Value!.RecordId))
                    return ReadFailed<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();

                records.Add(normalized.Value!);
            }
        }

        return OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>.Ok(records);
    }

    private async Task<OperationResult<bool>> RecoverBoundedAppendNoLockAsync(CancellationToken cancellationToken)
    {
        try
        {
            var recovered = await DesignConversionBoundedJsonlAppendJournal.RecoverNoLockAsync(
                _rootDirectory,
                _recordsPath,
                RecordsFileName,
                _maxJournalBytes,
                TryGetRecoverableRecordIdentity,
                cancellationToken);
            return recovered
                ? OperationResult<bool>.Ok(true)
                : ReadFailed<bool>();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<bool>();
        }
        catch (IOException)
        {
            return ReadFailed<bool>();
        }
        catch (UnauthorizedAccessException)
        {
            return ReadFailed<bool>();
        }
        catch (SecurityException)
        {
            return ReadFailed<bool>();
        }
    }

    private OperationResult<bool> EnsureExistingJournalWithinQuotaNoLock(string journalPath)
    {
        var length = TryReadExistingJournalLengthNoLock(journalPath);
        if (!length.IsSuccess)
            return length.MapError<bool>();

        return length.Value <= _maxJournalBytes
            ? OperationResult<bool>.Ok(true)
            : JournalQuotaExceeded<bool>();
    }

    private OperationResult<bool> EnsureAppendOrRolloverNoLock(byte[] appendedBytes)
    {
        if (appendedBytes.LongLength > _maxJournalBytes)
            return JournalQuotaExceeded<bool>();

        var length = TryReadExistingJournalLengthNoLock(_recordsPath);
        if (!length.IsSuccess)
            return length.MapError<bool>();
        if (length.Value > _maxJournalBytes)
            return JournalQuotaExceeded<bool>();

        try
        {
            var projectedLength = checked(length.Value + appendedBytes.LongLength);
            if (projectedLength <= _maxJournalBytes)
                return OperationResult<bool>.Ok(true);
        }
        catch (OverflowException)
        {
            // A full valid active segment can still roll before appending a bounded record.
        }

        return RolloverActiveJournalNoLock();
    }

    private OperationResult<bool> RolloverActiveJournalNoLock()
    {
        var length = TryReadExistingJournalLengthNoLock(_recordsPath);
        if (!length.IsSuccess)
            return length.MapError<bool>();
        if (length.Value > _maxJournalBytes)
            return JournalQuotaExceeded<bool>();
        if (length.Value == 0L)
            return OperationResult<bool>.Ok(true);

        var target = TryBuildNextSegmentPathNoLock();
        if (!target.IsSuccess)
            return target.MapError<bool>();

        try
        {
            File.Move(_recordsPath, target.Value!, overwrite: false);
            return OperationResult<bool>.Ok(true);
        }
        catch (FileNotFoundException)
        {
            return OperationResult<bool>.Ok(true);
        }
        catch (DirectoryNotFoundException)
        {
            return OperationResult<bool>.Ok(true);
        }
        catch (IOException)
        {
            return AppendFailed<bool>();
        }
        catch (UnauthorizedAccessException)
        {
            return AppendFailed<bool>();
        }
        catch (SecurityException)
        {
            return AppendFailed<bool>();
        }
    }

    private OperationResult<string> TryBuildNextSegmentPathNoLock()
    {
        var segments = TryGetSegmentEntriesNoLock();
        if (!segments.IsSuccess)
            return segments.MapError<string>();

        long nextIndex;
        try
        {
            nextIndex = segments.Value!.Count == 0
                ? 1L
                : checked(segments.Value.Max(segment => segment.Index) + 1L);
        }
        catch (OverflowException)
        {
            return AppendFailed<string>();
        }

        for (var candidate = nextIndex; candidate > 0L && candidate < long.MaxValue; candidate++)
        {
            var path = BuildSegmentPath(candidate);
            if (!File.Exists(path) && !Directory.Exists(path))
                return OperationResult<string>.Ok(path);
        }

        return AppendFailed<string>();
    }

    private OperationResult<IReadOnlyList<string>> TryGetReadableJournalPathsNoLock()
    {
        var segments = TryGetSegmentEntriesNoLock();
        if (!segments.IsSuccess)
            return segments.MapError<IReadOnlyList<string>>();

        var paths = segments.Value!
            .Select(segment => segment.Path)
            .ToList();
        if (File.Exists(_recordsPath))
            paths.Add(_recordsPath);
        return OperationResult<IReadOnlyList<string>>.Ok(paths);
    }

    private OperationResult<IReadOnlyList<JournalSegment>> TryGetSegmentEntriesNoLock()
    {
        var pathBoundary = EnsureJournalPathBoundaryNoLock();
        if (!pathBoundary.IsSuccess)
            return pathBoundary.MapError<IReadOnlyList<JournalSegment>>();

        if (!Directory.Exists(_rootDirectory))
            return OperationResult<IReadOnlyList<JournalSegment>>.Ok([]);

        try
        {
            var segments = new List<JournalSegment>();
            var segmentIndices = new HashSet<long>();
            foreach (var path in Directory.EnumerateFileSystemEntries(
                         _rootDirectory,
                         SegmentFileNamePrefix + "*",
                         SearchOption.TopDirectoryOnly))
            {
                var attributes = File.GetAttributes(path);
                if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
                    return ReadFailed<IReadOnlyList<JournalSegment>>();

                var fileName = Path.GetFileName(path);
                if (!TryParseSegmentIndex(fileName, out var index))
                    return ReadFailed<IReadOnlyList<JournalSegment>>();
                if (!string.Equals(fileName, BuildSegmentFileName(index), StringComparison.Ordinal))
                    return ReadFailed<IReadOnlyList<JournalSegment>>();
                if (!segmentIndices.Add(index))
                    return ReadFailed<IReadOnlyList<JournalSegment>>();

                segments.Add(new JournalSegment(index, path));
            }

            return OperationResult<IReadOnlyList<JournalSegment>>.Ok(
                segments
                    .OrderBy(segment => segment.Index)
                    .Select(segment => segment with { Path = Path.GetFullPath(segment.Path) })
                    .ToArray());
        }
        catch (IOException)
        {
            return ReadFailed<IReadOnlyList<JournalSegment>>();
        }
        catch (UnauthorizedAccessException)
        {
            return ReadFailed<IReadOnlyList<JournalSegment>>();
        }
        catch (SecurityException)
        {
            return ReadFailed<IReadOnlyList<JournalSegment>>();
        }
    }

    private string BuildSegmentPath(long segmentIndex)
        => Path.Combine(_rootDirectory, BuildSegmentFileName(segmentIndex));

    private static string BuildSegmentFileName(long segmentIndex)
        => $"{SegmentFileNamePrefix}{segmentIndex:000000}";

    private static bool TryParseSegmentIndex(string fileName, out long index)
    {
        index = 0L;
        if (!fileName.StartsWith(SegmentFileNamePrefix, StringComparison.Ordinal))
            return false;

        var suffix = fileName[SegmentFileNamePrefix.Length..];
        return suffix.Length > 0 &&
               suffix.All(char.IsDigit) &&
               long.TryParse(suffix, NumberStyles.None, CultureInfo.InvariantCulture, out index) &&
               index > 0L;
    }

    private OperationResult<long> TryReadExistingJournalLengthNoLock(string journalPath)
    {
        try
        {
            var attributes = File.GetAttributes(journalPath);
            if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
                return ReadFailed<long>();

            return OperationResult<long>.Ok(new FileInfo(journalPath).Length);
        }
        catch (FileNotFoundException)
        {
            return OperationResult<long>.Ok(0L);
        }
        catch (DirectoryNotFoundException)
        {
            return OperationResult<long>.Ok(0L);
        }
        catch (IOException)
        {
            return ReadFailed<long>();
        }
        catch (UnauthorizedAccessException)
        {
            return ReadFailed<long>();
        }
        catch (SecurityException)
        {
            return ReadFailed<long>();
        }
    }

    private sealed record JournalSegment(long Index, string Path);

    private OperationResult<bool> EnsureJournalPathBoundaryNoLock()
    {
        try
        {
            if (Directory.Exists(_rootDirectory))
            {
                var rootAttributes = File.GetAttributes(_rootDirectory);
                if ((rootAttributes & FileAttributes.Directory) == 0 ||
                    (rootAttributes & FileAttributes.ReparsePoint) != 0)
                {
                    return ReadFailed<bool>();
                }
            }
            else if (File.Exists(_rootDirectory))
            {
                return ReadFailed<bool>();
            }

            try
            {
                var activeAttributes = File.GetAttributes(_recordsPath);
                if ((activeAttributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
                    return ReadFailed<bool>();
            }
            catch (FileNotFoundException)
            {
                // A missing active journal is valid before the first append.
            }
            catch (DirectoryNotFoundException)
            {
                // A missing root is valid before the first append.
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (IOException)
        {
            return ReadFailed<bool>();
        }
        catch (UnauthorizedAccessException)
        {
            return ReadFailed<bool>();
        }
        catch (SecurityException)
        {
            return ReadFailed<bool>();
        }
    }

    private static OperationResult<DesignConversionVisualCostLedgerRecord> TryNormalizeAppendRecord(
        DesignConversionVisualCostLedgerRecord record)
    {
        if (!IsCanonicalRef(record.EvaluationJobRef, EvaluationJobRefPrefix) ||
            !IsCanonicalRef(record.SubmittedRequestRef, SubmittedRequestRefPrefix))
        {
            return OperationResult<DesignConversionVisualCostLedgerRecord>.Fail(
                "DesignConversion visual-cost ledger record requires canonical technical job and request refs.",
                InvalidRecordErrorCode);
        }
        if (!HasValidEnforceableDimensions(record))
        {
            return OperationResult<DesignConversionVisualCostLedgerRecord>.Fail(
                "DesignConversion visual-cost ledger record requires canonical technical user and exact UTC calendar-month period dimensions.",
                InvalidRecordErrorCode);
        }

        return OperationResult<DesignConversionVisualCostLedgerRecord>.Ok(
            NormalizeCanonicalRecord(record, requireRecordIdMatch: false));
    }

    private static OperationResult<DesignConversionVisualCostLedgerRecord> TryNormalizeReadRecord(
        DesignConversionVisualCostLedgerRecord record)
    {
        if (!IsCanonicalRef(record.EvaluationJobRef, EvaluationJobRefPrefix) ||
            !IsCanonicalRef(record.SubmittedRequestRef, SubmittedRequestRefPrefix) ||
            !string.Equals(record.LedgerKind, LedgerKind, StringComparison.Ordinal) ||
            !string.Equals(record.SchemaVersion, SchemaVersion, StringComparison.Ordinal))
        {
            return ReadFailed<DesignConversionVisualCostLedgerRecord>();
        }
        if (!IsLegacyReadOnlyRecord(record) && !HasValidEnforceableDimensions(record))
            return ReadFailed<DesignConversionVisualCostLedgerRecord>();

        var normalized = NormalizeCanonicalRecord(record, requireRecordIdMatch: true);
        if (!string.Equals(record.RecordId, normalized.RecordId, StringComparison.Ordinal))
            return ReadFailed<DesignConversionVisualCostLedgerRecord>();

        if (!HasExpectedCanonicalEvidenceRefs(record.EvidenceRefs, normalized.RecordId))
            return ReadFailed<DesignConversionVisualCostLedgerRecord>();

        return OperationResult<DesignConversionVisualCostLedgerRecord>.Ok(normalized);
    }

    private static DesignConversionVisualCostLedgerRecord NormalizeCanonicalRecord(
        DesignConversionVisualCostLedgerRecord record,
        bool requireRecordIdMatch)
    {
        var accounting = new DesignConversionVisualCostAccounting(
            TotalCostUsd: Math.Max(0m, record.TotalCostUsd),
            MeasurementComplete: record.MeasurementComplete,
            MissingMeasurementCount: Math.Max(0, record.MissingMeasurementCount),
            EvaluationAttemptCount: Math.Max(0, record.EvaluationAttemptCount),
            CostSource: NormalizeCostSource(record.CostSource));
        var recordId = BuildRecordId(record.EvaluationJobRef, record.SubmittedRequestRef, accounting);
        if (requireRecordIdMatch && !string.Equals(record.RecordId, recordId, StringComparison.Ordinal))
            return record with { RecordId = recordId };

        return new DesignConversionVisualCostLedgerRecord(
            RecordId: recordId,
            LedgerKind: LedgerKind,
            SchemaVersion: SchemaVersion,
            EvaluationJobRef: record.EvaluationJobRef,
            SubmittedRequestRef: record.SubmittedRequestRef,
            TotalCostUsd: accounting.TotalCostUsd,
            MeasurementComplete: accounting.MeasurementComplete,
            MissingMeasurementCount: accounting.MissingMeasurementCount,
            EvaluationAttemptCount: accounting.EvaluationAttemptCount,
            CostSource: accounting.CostSource,
            RecordedAtUtc: record.RecordedAtUtc.ToUniversalTime(),
            EvidenceRefs: BuildEvidenceRefs(recordId),
            Metadata: BuildFixedMetadata(record.Metadata)) with
            {
                UserRef = IsLegacyReadOnlyRecord(record) ? LegacyUnattributedUserRef : record.UserRef,
                SubmittedTier = NormalizeSubmittedTier(record.SubmittedTier),
                PeriodKind = IsLegacyReadOnlyRecord(record) ? NotPeriodEnforceablePeriodKind : UtcCalendarMonthPeriodKind,
                PeriodStartUtc = IsLegacyReadOnlyRecord(record) ? DateTimeOffset.UnixEpoch : record.PeriodStartUtc,
                PeriodEndExclusiveUtc = IsLegacyReadOnlyRecord(record) ? DateTimeOffset.UnixEpoch : record.PeriodEndExclusiveUtc
            };
    }

    private static IReadOnlyList<string> BuildEvidenceRefs(string recordId)
        => [BuildCanonicalRef(EvidenceRefPrefix, recordId)];

    private static bool HasExpectedCanonicalEvidenceRefs(IReadOnlyList<string>? evidenceRefs, string recordId)
    {
        var expected = BuildEvidenceRefs(recordId);
        var actual = evidenceRefs ?? [];
        return actual.Count == expected.Count &&
               actual.Zip(expected).All(pair =>
                   string.Equals(pair.First, pair.Second, StringComparison.Ordinal) &&
                   IsCanonicalRef(pair.First, EvidenceRefPrefix));
    }

    private static IReadOnlyDictionary<string, string> BuildFixedMetadata(
        IReadOnlyDictionary<string, string>? source = null)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ledger_purpose"] = "operational_visual_cost",
            ["training_artifact_db"] = bool.FalseString,
            ["quota_enforcement"] = bool.FalseString,
            ["raw_payloads_persisted"] = bool.FalseString,
            ["user_ref_semantics"] = "pseudonymous_technical_scoped_sha256_not_anonymous_not_secret",
            ["tier_authority"] = "submitted_context_only",
            ["known_tiers_preserved"] = "free|paid|pro",
            ["period_source"] = "server_owned_recorded_at_utc_calendar_month",
            ["monthly_cap_enforcement"] = bool.FalseString,
            ["ledger_scope"] = "record_and_aggregate_only"
        };
        if (source is not null &&
            source.TryGetValue(
                JsonDesignConversionPostEvaluationStateStore.StateRefMetadataKey,
                out var stateRef) &&
            JsonDesignConversionPostEvaluationStateStore.IsCanonicalStateRef(stateRef))
        {
            metadata[JsonDesignConversionPostEvaluationStateStore.StateRefMetadataKey] =
                stateRef;
            metadata[JsonDesignConversionPostEvaluationStateStore.PersistenceStatusMetadataKey] =
                JsonDesignConversionPostEvaluationStateStore.PersistenceProvisional;
            metadata[JsonDesignConversionPostEvaluationStateStore.TrainingEligibleMetadataKey] =
                bool.FalseString;
        }

        return metadata;
    }

    private static string NormalizeCostSource(string? costSource)
    {
        var trimmed = string.IsNullOrWhiteSpace(costSource)
            ? DesignConversionVisualCostAccounting.UnknownCostSource
            : costSource.Trim();

        return trimmed switch
        {
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest => trimmed,
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceUnknownUnpriced => trimmed,
            DesignConversionVisualCostAccounting.UnknownCostSource => trimmed,
            DesignConversionVisualCostAccounting.MixedCostSource => trimmed,
            _ => DesignConversionVisualCostAccounting.UnknownCostSource
        };
    }

    private static string? TryGetRecoverableRecordIdentity(string line)
    {
        try
        {
            var record = JsonSerializer.Deserialize<DesignConversionVisualCostLedgerRecord>(line, JsonOptions);
            if (record is null)
                return null;

            var normalized = TryNormalizeReadRecord(record);
            return normalized.IsSuccess ? normalized.Value!.RecordId : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static StringComparer GetPathKeyComparer()
        => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    private static string NormalizeMutationGateKey(string recordsPath)
        => Path.GetFullPath(recordsPath);

    private static OperationResult<T> Cancelled<T>()
        => OperationResult<T>.Fail("DesignConversion visual-cost ledger operation cancelled.", CancelledErrorCode);

    private static OperationResult<T> AppendFailed<T>()
        => OperationResult<T>.Fail(AppendFailedSafeMessage, AppendFailedErrorCode);

    private static OperationResult<T> ReadFailed<T>()
        => OperationResult<T>.Fail(
            "DesignConversion visual-cost ledger records could not be read.",
            ReadFailedErrorCode);

    private static OperationResult<T> JournalQuotaExceeded<T>()
        => OperationResult<T>.Fail(
            JournalQuotaExceededSafeMessage,
            JournalQuotaExceededErrorCode);

    internal static string BuildRecordId(
        string evaluationJobRef,
        string submittedRequestRef,
        DesignConversionVisualCostAccounting accounting)
    {
        var normalizedAccounting = new DesignConversionVisualCostAccounting(
            TotalCostUsd: Math.Max(0m, accounting.TotalCostUsd),
            MeasurementComplete: accounting.MeasurementComplete,
            MissingMeasurementCount: Math.Max(0, accounting.MissingMeasurementCount),
            EvaluationAttemptCount: Math.Max(0, accounting.EvaluationAttemptCount),
            CostSource: NormalizeCostSource(accounting.CostSource));
        var identity = string.Join("|",
        [
            LedgerKind,
            SchemaVersion,
            evaluationJobRef,
            submittedRequestRef,
            normalizedAccounting.TotalCostUsd.ToString("G29", CultureInfo.InvariantCulture),
            normalizedAccounting.MeasurementComplete.ToString(CultureInfo.InvariantCulture),
            normalizedAccounting.MissingMeasurementCount.ToString(CultureInfo.InvariantCulture),
            normalizedAccounting.EvaluationAttemptCount.ToString(CultureInfo.InvariantCulture),
            normalizedAccounting.CostSource
        ]);

        return BuildCanonicalRef(RecordIdPrefix, identity);
    }

    internal static string BuildCanonicalEvaluationJobRef(string jobId)
        => BuildCanonicalRef(EvaluationJobRefPrefix, jobId);

    internal static string BuildCanonicalSubmittedRequestRef(string requestId)
        => BuildCanonicalRef(SubmittedRequestRefPrefix, requestId);

    internal static bool IsCanonicalRecordId(string recordId)
        => IsCanonicalRef(recordId, RecordIdPrefix);

    internal static OperationResult<string> BuildPseudonymousUserRef(string rawUserId)
    {
        if (string.IsNullOrWhiteSpace(rawUserId))
        {
            return OperationResult<string>.Fail(
                "DesignConversion visual-cost ledger user ref source is required.",
                UserRefSourceRequiredErrorCode);
        }

        var scopedInput = string.Join("\n", [UserRefDomainSeparator, rawUserId.Trim()]);
        return OperationResult<string>.Ok(BuildCanonicalRef(UserRefPrefix, scopedInput));
    }

    internal static bool IsTechnicalUserRef(string? userRef)
        => IsCanonicalRef(userRef, UserRefPrefix);

    internal static string NormalizeSubmittedTier(string? tier)
    {
        var normalized = string.IsNullOrWhiteSpace(tier)
            ? UnknownSubmittedTier
            : tier.Trim().ToLowerInvariant();

        return normalized switch
        {
            "free" => "free",
            "paid" => "paid",
            "pro" => "pro",
            UnknownSubmittedTier => UnknownSubmittedTier,
            _ => UnrecognizedSubmittedTier
        };
    }

    internal static (DateTimeOffset StartUtc, DateTimeOffset EndExclusiveUtc) BuildUtcCalendarMonthPeriod(
        DateTimeOffset recordedAtUtc)
    {
        var utc = recordedAtUtc.ToUniversalTime();
        var start = new DateTimeOffset(utc.Year, utc.Month, 1, 0, 0, 0, TimeSpan.Zero);
        return (start, start.AddMonths(1));
    }

    private static bool IsLegacyReadOnlyRecord(DesignConversionVisualCostLedgerRecord record)
        => string.Equals(record.UserRef, LegacyUnattributedUserRef, StringComparison.Ordinal) &&
           string.Equals(record.PeriodKind, NotPeriodEnforceablePeriodKind, StringComparison.Ordinal) &&
           record.PeriodStartUtc == DateTimeOffset.UnixEpoch &&
           record.PeriodEndExclusiveUtc == DateTimeOffset.UnixEpoch;

    private static bool HasValidEnforceableDimensions(DesignConversionVisualCostLedgerRecord record)
    {
        if (!IsTechnicalUserRef(record.UserRef) ||
            !string.Equals(record.PeriodKind, UtcCalendarMonthPeriodKind, StringComparison.Ordinal) ||
            !IsExactUtcCalendarMonthPeriod(record.PeriodStartUtc, record.PeriodEndExclusiveUtc))
        {
            return false;
        }

        var expectedPeriod = BuildUtcCalendarMonthPeriod(record.RecordedAtUtc.ToUniversalTime());
        return record.PeriodStartUtc == expectedPeriod.StartUtc &&
               record.PeriodEndExclusiveUtc == expectedPeriod.EndExclusiveUtc;
    }

    private static bool HasSameDurableRecordDimensions(
        DesignConversionVisualCostLedgerRecord existing,
        DesignConversionVisualCostLedgerRecord candidate)
        => string.Equals(existing.RecordId, candidate.RecordId, StringComparison.Ordinal) &&
           string.Equals(existing.LedgerKind, candidate.LedgerKind, StringComparison.Ordinal) &&
           string.Equals(existing.SchemaVersion, candidate.SchemaVersion, StringComparison.Ordinal) &&
           string.Equals(existing.EvaluationJobRef, candidate.EvaluationJobRef, StringComparison.Ordinal) &&
           string.Equals(existing.SubmittedRequestRef, candidate.SubmittedRequestRef, StringComparison.Ordinal) &&
           existing.TotalCostUsd == candidate.TotalCostUsd &&
           existing.MeasurementComplete == candidate.MeasurementComplete &&
           existing.MissingMeasurementCount == candidate.MissingMeasurementCount &&
           existing.EvaluationAttemptCount == candidate.EvaluationAttemptCount &&
           string.Equals(existing.CostSource, candidate.CostSource, StringComparison.Ordinal) &&
           existing.RecordedAtUtc == candidate.RecordedAtUtc &&
           string.Equals(existing.UserRef, candidate.UserRef, StringComparison.Ordinal) &&
           string.Equals(existing.SubmittedTier, candidate.SubmittedTier, StringComparison.Ordinal) &&
           string.Equals(existing.PeriodKind, candidate.PeriodKind, StringComparison.Ordinal) &&
           existing.PeriodStartUtc == candidate.PeriodStartUtc &&
           existing.PeriodEndExclusiveUtc == candidate.PeriodEndExclusiveUtc;

    private static bool IsExactUtcCalendarMonthPeriod(
        DateTimeOffset periodStartUtc,
        DateTimeOffset periodEndExclusiveUtc)
    {
        return periodStartUtc.Offset == TimeSpan.Zero &&
               periodEndExclusiveUtc.Offset == TimeSpan.Zero &&
               periodStartUtc.Day == 1 &&
               periodStartUtc.Hour == 0 &&
               periodStartUtc.Minute == 0 &&
               periodStartUtc.Second == 0 &&
               periodStartUtc.Millisecond == 0 &&
               periodStartUtc.Microsecond == 0 &&
               periodStartUtc.Nanosecond == 0 &&
               periodEndExclusiveUtc == periodStartUtc.AddMonths(1);
    }

    private static string BuildCanonicalRef(string prefix, string value)
        => $"{prefix}-{Convert.ToHexString(SHA256.HashData(StrictUtf8Encoding.GetBytes(value ?? string.Empty))).ToLowerInvariant()}";

    private static bool IsCanonicalRef(string? value, string prefix)
    {
        var expectedLength = prefix.Length + 1 + 64;
        return !string.IsNullOrWhiteSpace(value) &&
               value.Length == expectedLength &&
               value.StartsWith(prefix + "-", StringComparison.Ordinal) &&
               value.Skip(prefix.Length + 1).All(ch => ch is >= '0' and <= '9' or >= 'a' and <= 'f');
    }

}
