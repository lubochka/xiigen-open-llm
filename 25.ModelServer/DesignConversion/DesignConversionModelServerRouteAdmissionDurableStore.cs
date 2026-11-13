// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal enum DesignConversionModelServerRouteAdmissionDurableReplayKind
{
    Miss,
    Reserved,
    Replayed,
    Conflict,
    FailedClosed
}

internal sealed record DesignConversionModelServerRouteAdmissionDurableReplayResult(
    DesignConversionModelServerRouteAdmissionDurableReplayKind Kind,
    DesignConversionModelServerHttpResponse? Response = null,
    DesignConversionModelServerRouteAdmissionDurableReplayLease? Lease = null,
    string ErrorRef = "none")
{
    public static DesignConversionModelServerRouteAdmissionDurableReplayResult Miss { get; } =
        new(DesignConversionModelServerRouteAdmissionDurableReplayKind.Miss);

    public static DesignConversionModelServerRouteAdmissionDurableReplayResult Reserved(
        DesignConversionModelServerRouteAdmissionDurableReplayLease lease)
        => new(DesignConversionModelServerRouteAdmissionDurableReplayKind.Reserved, Lease: lease);

    public static DesignConversionModelServerRouteAdmissionDurableReplayResult Replayed(
        DesignConversionModelServerHttpResponse response)
        => new(DesignConversionModelServerRouteAdmissionDurableReplayKind.Replayed, Response: response);

    public static DesignConversionModelServerRouteAdmissionDurableReplayResult Conflict()
        => new(DesignConversionModelServerRouteAdmissionDurableReplayKind.Conflict);

    public static DesignConversionModelServerRouteAdmissionDurableReplayResult FailedClosed(string errorRef)
        => new(DesignConversionModelServerRouteAdmissionDurableReplayKind.FailedClosed, ErrorRef: errorRef);
}

internal sealed record DesignConversionModelServerRouteAdmissionDurablePersistResult(
    bool IsSuccess,
    string ErrorRef)
{
    public static DesignConversionModelServerRouteAdmissionDurablePersistResult Success { get; } =
        new(true, "none");

    public static DesignConversionModelServerRouteAdmissionDurablePersistResult FailedClosed(string errorRef)
        => new(false, errorRef);
}

internal sealed class DesignConversionModelServerRouteAdmissionDurableReplayLease : IDisposable
{
    private readonly FileStream _stream;
    private readonly string _lockPath;
    private bool _disposed;

    public DesignConversionModelServerRouteAdmissionDurableReplayLease(
        FileStream stream,
        string lockPath,
        string recordPath,
        string keyRef,
        string scopeRef)
    {
        _stream = stream;
        _lockPath = lockPath;
        RecordPath = recordPath;
        KeyRef = keyRef;
        ScopeRef = scopeRef;
    }

    public string RecordPath { get; }

    public string KeyRef { get; }

    public string ScopeRef { get; }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _stream.Dispose();
        try
        {
            if (File.Exists(_lockPath))
                File.Delete(_lockPath);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}

internal sealed class DesignConversionModelServerRouteAdmissionDurableStore
{
    private const int SchemaVersion = 1;
    private const int MaxResponseBodyBytes = 1024 * 1024;
    private const int MaxRecordBytes = MaxResponseBodyBytes + 64 * 1024;
    private const int MaxLockBytes = 4096;
    private const int MaxContentTypeLength = 256;
    private const int MaxHeaderCount = 32;
    private const int MaxHeaderNameLength = 128;
    private const int MaxHeaderValueLength = 1024;
    private const int MaxErrorRefLength = 96;
    private const string LockKind = "design_conversion_route_admission_idempotency_inflight";
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private static readonly TimeSpan LockWaitTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan LockPollInterval = TimeSpan.FromMilliseconds(50);
    private static readonly TimeSpan TempFileMaxAge = TimeSpan.FromMinutes(10);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    private readonly string? _root;
    private readonly string? _recordsRoot;
    private readonly string? _locksRoot;
    private readonly string? _initializationError;

    private DesignConversionModelServerRouteAdmissionDurableStore(
        string? root,
        string? initializationError)
    {
        _root = root;
        _recordsRoot = root is null ? null : Path.Combine(root, "records");
        _locksRoot = root is null ? null : Path.Combine(root, "locks");
        _initializationError = initializationError;
    }

    public string StoreState => _initializationError is not null
        ? "failed_closed"
        : "enabled";

    public static DesignConversionModelServerRouteAdmissionDurableStore? CreateIfConfigured(string? storeRoot)
    {
        if (string.IsNullOrWhiteSpace(storeRoot))
            return null;

        try
        {
            return new DesignConversionModelServerRouteAdmissionDurableStore(
                Path.GetFullPath(storeRoot),
                initializationError: null);
        }
        catch (Exception ex) when (IsSafeIoException(ex))
        {
            return new DesignConversionModelServerRouteAdmissionDurableStore(
                root: null,
                initializationError: "durable_store_root_invalid");
        }
    }

    public int CountCompletedRecordsBounded(int capacity)
    {
        var safe = EnsureSafeStore();
        if (!safe.IsSuccess || _recordsRoot is null)
            return 0;

        try
        {
            return Directory
                .EnumerateFiles(_recordsRoot, "*.json", SearchOption.TopDirectoryOnly)
                .Take(Math.Clamp(capacity, 1, 4096) + 1)
                .Count();
        }
        catch (Exception ex) when (IsSafeIoException(ex))
        {
            return 0;
        }
    }

    public DesignConversionModelServerRouteAdmissionDurableReplayResult TryReadCompleted(
        string scope,
        string idempotencyKey,
        byte[] requestDigest,
        DateTimeOffset now,
        TimeSpan ttl,
        int capacity)
    {
        var replayKey = DurableReplayKey.Create(scope, idempotencyKey);
        var safe = EnsureSafeStore();
        if (!safe.IsSuccess)
            return DesignConversionModelServerRouteAdmissionDurableReplayResult.FailedClosed(safe.ErrorRef);

        var loaded = TryLoadCompletedRecord(replayKey, requestDigest, now);
        if (loaded.Kind != DesignConversionModelServerRouteAdmissionDurableReplayKind.Miss)
            return loaded;

        PruneCompletedRecords(now, ttl, capacity, replayKey.RecordFileName);
        return DesignConversionModelServerRouteAdmissionDurableReplayResult.Miss;
    }

    public async Task<DesignConversionModelServerRouteAdmissionDurableReplayResult> TryReserveForExecutionAsync(
        string scope,
        string idempotencyKey,
        byte[] requestDigest,
        Func<DateTimeOffset> utcNow,
        TimeSpan ttl,
        int capacity,
        CancellationToken cancellationToken)
    {
        var replayKey = DurableReplayKey.Create(scope, idempotencyKey);
        var safe = EnsureSafeStore();
        if (!safe.IsSuccess)
            return DesignConversionModelServerRouteAdmissionDurableReplayResult.FailedClosed(safe.ErrorRef);

        var deadline = utcNow() + LockWaitTimeout;
        while (utcNow() < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var existing = TryReadCompleted(scope, idempotencyKey, requestDigest, utcNow(), ttl, capacity);
            if (existing.Kind != DesignConversionModelServerRouteAdmissionDurableReplayKind.Miss)
                return existing;

            var leaseAttempt = TryAcquireLease(replayKey, requestDigest, utcNow(), ttl);
            if (leaseAttempt.Kind == DesignConversionModelServerRouteAdmissionDurableReplayKind.Reserved)
            {
                var lease = leaseAttempt.Lease!;
                var afterLock = TryLoadCompletedRecord(replayKey, requestDigest, utcNow());
                if (afterLock.Kind != DesignConversionModelServerRouteAdmissionDurableReplayKind.Miss)
                {
                    lease.Dispose();
                    return afterLock;
                }

                return DesignConversionModelServerRouteAdmissionDurableReplayResult.Reserved(lease);
            }

            if (leaseAttempt.Kind is DesignConversionModelServerRouteAdmissionDurableReplayKind.Conflict or
                DesignConversionModelServerRouteAdmissionDurableReplayKind.FailedClosed)
            {
                return leaseAttempt;
            }

            await Task.Delay(LockPollInterval, cancellationToken).ConfigureAwait(false);
        }

        return DesignConversionModelServerRouteAdmissionDurableReplayResult.FailedClosed(
            "durable_idempotency_lock_timeout");
    }

    public DesignConversionModelServerRouteAdmissionDurablePersistResult TryPersistCompleted(
        DesignConversionModelServerRouteAdmissionDurableReplayLease? lease,
        string scope,
        string idempotencyKey,
        byte[] requestDigest,
        DesignConversionModelServerHttpResponse response,
        DateTimeOffset now,
        TimeSpan ttl,
        int capacity)
    {
        if (lease is null)
            return DesignConversionModelServerRouteAdmissionDurablePersistResult.FailedClosed(
                "durable_idempotency_lock_missing");

        var safe = EnsureSafeStore();
        if (!safe.IsSuccess)
            return DesignConversionModelServerRouteAdmissionDurablePersistResult.FailedClosed(safe.ErrorRef);

        var replayKey = DurableReplayKey.Create(scope, idempotencyKey);
        if (!string.Equals(lease.KeyRef, replayKey.KeyRef, StringComparison.Ordinal) ||
            !string.Equals(lease.ScopeRef, replayKey.ScopeRef, StringComparison.Ordinal) ||
            !string.Equals(lease.RecordPath, RecordPath(replayKey), StringComparison.Ordinal))
        {
            return DesignConversionModelServerRouteAdmissionDurablePersistResult.FailedClosed(
                "durable_idempotency_lock_scope_mismatch");
        }

        if (!TryValidateResponseBounds(response, out var responseBodySha256, out var headers, out var responseError))
            return DesignConversionModelServerRouteAdmissionDurablePersistResult.FailedClosed(responseError);

        var record = new DurableReplayRecordDto
        {
            SchemaVersion = SchemaVersion,
            RecordKind = "design_conversion_route_admission_idempotency_replay",
            KeyRef = replayKey.KeyRef,
            ScopeRef = replayKey.ScopeRef,
            RequestDigest = DigestRef(requestDigest),
            ResponseStatusCode = response.StatusCode,
            ResponseContentType = response.ContentType,
            ResponseBodyJson = response.BodyJson,
            ResponseBodySha256 = responseBodySha256,
            ResponseHeaders = headers,
            JobIdRef = TryReadJobIdRef(response.BodyJson),
            CreatedAtUtc = now.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            ExpiresAtUtc = now.Add(ttl).ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)
        };
        record.IntegritySha256 = ComputeIntegrity(record);

        var recordJson = JsonSerializer.Serialize(record, JsonOptions);
        if (Encoding.UTF8.GetByteCount(recordJson) > MaxRecordBytes)
            return DesignConversionModelServerRouteAdmissionDurablePersistResult.FailedClosed(
                "durable_idempotency_record_too_large");

        try
        {
            Directory.CreateDirectory(_recordsRoot!);
            PruneCompletedRecords(now, ttl, capacity, replayKey.RecordFileName);
            var recordPath = RecordPath(replayKey);
            if (ExistingPathIsReparsePoint(recordPath))
                return DesignConversionModelServerRouteAdmissionDurablePersistResult.FailedClosed(
                    "durable_idempotency_record_reparse_point");

            var tempPath = Path.Combine(
                _recordsRoot!,
                replayKey.RecordFileName + "." + Guid.NewGuid().ToString("N") + ".tmp");
            try
            {
                var bytes = Encoding.UTF8.GetBytes(recordJson);
                using (var stream = new FileStream(
                           tempPath,
                           FileMode.CreateNew,
                           FileAccess.Write,
                           FileShare.None,
                           4096,
                           FileOptions.WriteThrough))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(flushToDisk: true);
                }

                File.Move(tempPath, recordPath, overwrite: true);
            }
            finally
            {
                TryDeleteFile(tempPath);
            }

            PruneCompletedRecords(now, ttl, capacity, replayKey.RecordFileName);
            return DesignConversionModelServerRouteAdmissionDurablePersistResult.Success;
        }
        catch (Exception ex) when (IsSafeIoException(ex))
        {
            return DesignConversionModelServerRouteAdmissionDurablePersistResult.FailedClosed(
                "durable_idempotency_record_write_failed");
        }
    }

    private DesignConversionModelServerRouteAdmissionDurableReplayResult TryLoadCompletedRecord(
        DurableReplayKey replayKey,
        byte[] requestDigest,
        DateTimeOffset now)
    {
        var path = RecordPath(replayKey);
        if (!File.Exists(path))
            return DesignConversionModelServerRouteAdmissionDurableReplayResult.Miss;
        if (ExistingPathIsReparsePoint(path))
            return DesignConversionModelServerRouteAdmissionDurableReplayResult.FailedClosed(
                "durable_idempotency_record_reparse_point");

        try
        {
            var snapshot = TryReadBoundedStrictUtf8Snapshot(path, MaxRecordBytes);
            if (!snapshot.IsSuccess)
            {
                TryDeleteFile(path);
                return DesignConversionModelServerRouteAdmissionDurableReplayResult.FailedClosed(
                    snapshot.ErrorRef);
            }

            var record = JsonSerializer.Deserialize<DurableReplayRecordDto>(snapshot.Json, JsonOptions);
            var validation = ValidateRecord(record, replayKey, requestDigest, now);
            if (string.Equals(validation, "expired", StringComparison.Ordinal))
            {
                TryDeleteFile(path);
                return DesignConversionModelServerRouteAdmissionDurableReplayResult.Miss;
            }

            if (!string.Equals(validation, "ok", StringComparison.Ordinal))
            {
                TryDeleteFile(path);
                return DesignConversionModelServerRouteAdmissionDurableReplayResult.FailedClosed(validation);
            }

            if (!string.Equals(record!.RequestDigest, DigestRef(requestDigest), StringComparison.Ordinal))
                return DesignConversionModelServerRouteAdmissionDurableReplayResult.Conflict();

            return DesignConversionModelServerRouteAdmissionDurableReplayResult.Replayed(
                new DesignConversionModelServerHttpResponse(
                    record.ResponseStatusCode,
                    record.ResponseContentType ?? "application/json; charset=utf-8",
                    record.ResponseBodyJson ?? string.Empty,
                    record.ResponseHeaders ?? new Dictionary<string, string>(StringComparer.Ordinal)));
        }
        catch (Exception ex) when (ex is JsonException || IsSafeIoException(ex))
        {
            TryDeleteFile(path);
            return DesignConversionModelServerRouteAdmissionDurableReplayResult.FailedClosed(
                "durable_idempotency_record_corrupt");
        }
    }

    private string ValidateRecord(
        DurableReplayRecordDto? record,
        DurableReplayKey replayKey,
        byte[] requestDigest,
        DateTimeOffset now)
    {
        if (record is null)
            return "durable_idempotency_record_corrupt";
        if (record.SchemaVersion != SchemaVersion)
            return "durable_idempotency_record_schema_unsupported";
        if (!string.Equals(record.RecordKind, "design_conversion_route_admission_idempotency_replay", StringComparison.Ordinal))
            return "durable_idempotency_record_kind_invalid";
        if (!string.Equals(record.KeyRef, replayKey.KeyRef, StringComparison.Ordinal) ||
            !string.Equals(record.ScopeRef, replayKey.ScopeRef, StringComparison.Ordinal))
            return "durable_idempotency_record_scope_mismatch";
        if (string.IsNullOrWhiteSpace(record.RequestDigest) ||
            !record.RequestDigest.StartsWith("sha256:design-conversion-route-admission-request-digest:", StringComparison.Ordinal))
            return "durable_idempotency_record_digest_invalid";
        if (record.ResponseStatusCode is < 100 or > 599)
            return "durable_idempotency_record_status_invalid";
        if (string.IsNullOrWhiteSpace(record.ResponseContentType) ||
            record.ResponseContentType.Length > MaxContentTypeLength)
            return "durable_idempotency_record_content_type_invalid";
        if (record.ResponseBodyJson is null ||
            Encoding.UTF8.GetByteCount(record.ResponseBodyJson) > MaxResponseBodyBytes)
            return "durable_idempotency_record_body_oversized";
        if (!string.Equals(record.ResponseBodySha256, Sha256Ref("response-body", record.ResponseBodyJson), StringComparison.Ordinal))
            return "durable_idempotency_record_body_checksum_mismatch";
        if (!HeadersWithinBounds(record.ResponseHeaders))
            return "durable_idempotency_record_headers_invalid";
        if (!DateTimeOffset.TryParse(
                record.CreatedAtUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var created))
        {
            return "durable_idempotency_record_created_invalid";
        }

        if (!DateTimeOffset.TryParse(
                record.ExpiresAtUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var expires))
        {
            return "durable_idempotency_record_expires_invalid";
        }

        if (expires <= created)
            return "durable_idempotency_record_expiry_invalid";
        if (now >= expires)
            return "expired";
        if (!string.Equals(record.IntegritySha256, ComputeIntegrity(record), StringComparison.Ordinal))
            return "durable_idempotency_record_integrity_mismatch";

        if (!string.Equals(record.RequestDigest, DigestRef(requestDigest), StringComparison.Ordinal))
            return "ok";

        return "ok";
    }

    private DesignConversionModelServerRouteAdmissionDurableReplayResult TryAcquireLease(
        DurableReplayKey replayKey,
        byte[] requestDigest,
        DateTimeOffset now,
        TimeSpan ttl)
    {
        var lockPath = LockPath(replayKey);
        try
        {
            var existing = TryInspectExistingLock(lockPath, replayKey, requestDigest, now);
            if (existing.Kind is DesignConversionModelServerRouteAdmissionDurableReplayKind.Conflict or
                DesignConversionModelServerRouteAdmissionDurableReplayKind.FailedClosed)
            {
                return existing;
            }

            var stream = new FileStream(
                lockPath,
                FileMode.CreateNew,
                FileAccess.ReadWrite,
                FileShare.None,
                4096,
                FileOptions.WriteThrough);
            var createdAtUtc = now.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
            var expiresAtUtc = SafeAdd(now, ttl).ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
            var lockRecord = new DurableReplayLockDto
            {
                SchemaVersion = SchemaVersion,
                LockKind = LockKind,
                KeyRef = replayKey.KeyRef,
                ScopeRef = replayKey.ScopeRef,
                RequestDigest = DigestRef(requestDigest),
                CreatedAtUtc = createdAtUtc,
                ExpiresAtUtc = expiresAtUtc
            };
            lockRecord.IntegritySha256 = ComputeLockIntegrity(lockRecord);
            var content = JsonSerializer.Serialize(lockRecord, JsonOptions);
            if (Encoding.UTF8.GetByteCount(content) > MaxLockBytes)
            {
                stream.Dispose();
                TryDeleteFile(lockPath);
                return DesignConversionModelServerRouteAdmissionDurableReplayResult.FailedClosed(
                    "durable_idempotency_lock_too_large");
            }

            var bytes = Encoding.UTF8.GetBytes(content);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush(flushToDisk: true);
            stream.Position = 0;
            return DesignConversionModelServerRouteAdmissionDurableReplayResult.Reserved(
                new DesignConversionModelServerRouteAdmissionDurableReplayLease(
                    stream,
                    lockPath,
                    RecordPath(replayKey),
                    replayKey.KeyRef,
                    replayKey.ScopeRef));
        }
        catch (IOException)
        {
            return DesignConversionModelServerRouteAdmissionDurableReplayResult.Miss;
        }
        catch (UnauthorizedAccessException)
        {
            return DesignConversionModelServerRouteAdmissionDurableReplayResult.Miss;
        }
    }

    private DesignConversionModelServerRouteAdmissionDurableReplayResult TryInspectExistingLock(
        string lockPath,
        DurableReplayKey replayKey,
        byte[] requestDigest,
        DateTimeOffset now)
    {
        if (!File.Exists(lockPath))
            return DesignConversionModelServerRouteAdmissionDurableReplayResult.Miss;
        if (ExistingPathIsReparsePoint(lockPath))
            return DesignConversionModelServerRouteAdmissionDurableReplayResult.FailedClosed(
                "durable_idempotency_lock_reparse_point");

        try
        {
            var info = new FileInfo(lockPath);
            if (!info.Exists)
                return DesignConversionModelServerRouteAdmissionDurableReplayResult.Miss;
            if (info.Length > MaxLockBytes)
                return DesignConversionModelServerRouteAdmissionDurableReplayResult.FailedClosed(
                    "durable_idempotency_lock_oversized");

            string json;
            using (var stream = new FileStream(
                       lockPath,
                       FileMode.Open,
                       FileAccess.Read,
                       FileShare.None,
                       4096,
                       FileOptions.SequentialScan))
            {
                if (stream.Length > MaxLockBytes)
                    return DesignConversionModelServerRouteAdmissionDurableReplayResult.FailedClosed(
                        "durable_idempotency_lock_oversized");

                using var reader = new StreamReader(
                    stream,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: false);
                json = reader.ReadToEnd();
            }

            if (Encoding.UTF8.GetByteCount(json) > MaxLockBytes)
                return DesignConversionModelServerRouteAdmissionDurableReplayResult.FailedClosed(
                    "durable_idempotency_lock_oversized");

            var lockRecord = JsonSerializer.Deserialize<DurableReplayLockDto>(json, JsonOptions);
            var validation = ValidateLockRecord(lockRecord, replayKey, requestDigest, now);
            if (string.Equals(validation, "expired", StringComparison.Ordinal))
            {
                TryDeleteFile(lockPath);
                return File.Exists(lockPath)
                    ? DesignConversionModelServerRouteAdmissionDurableReplayResult.FailedClosed(
                        "durable_idempotency_lock_delete_failed")
                    : DesignConversionModelServerRouteAdmissionDurableReplayResult.Miss;
            }

            if (string.Equals(validation, "conflict", StringComparison.Ordinal))
                return DesignConversionModelServerRouteAdmissionDurableReplayResult.Conflict();

            return DesignConversionModelServerRouteAdmissionDurableReplayResult.FailedClosed(validation);
        }
        catch (IOException)
        {
            return DesignConversionModelServerRouteAdmissionDurableReplayResult.Miss;
        }
        catch (UnauthorizedAccessException)
        {
            return DesignConversionModelServerRouteAdmissionDurableReplayResult.FailedClosed(
                "durable_idempotency_lock_unreadable");
        }
        catch (JsonException)
        {
            return DesignConversionModelServerRouteAdmissionDurableReplayResult.FailedClosed(
                "durable_idempotency_lock_corrupt");
        }
        catch (Exception ex) when (IsSafeIoException(ex))
        {
            return DesignConversionModelServerRouteAdmissionDurableReplayResult.FailedClosed(
                "durable_idempotency_lock_unreadable");
        }
    }

    private static string ValidateLockRecord(
        DurableReplayLockDto? lockRecord,
        DurableReplayKey replayKey,
        byte[] requestDigest,
        DateTimeOffset now)
    {
        if (lockRecord is null)
            return "durable_idempotency_lock_corrupt";
        if (lockRecord.SchemaVersion != SchemaVersion)
            return "durable_idempotency_lock_schema_unsupported";
        if (!string.Equals(lockRecord.LockKind, LockKind, StringComparison.Ordinal))
            return "durable_idempotency_lock_kind_invalid";
        if (!string.Equals(lockRecord.KeyRef, replayKey.KeyRef, StringComparison.Ordinal) ||
            !string.Equals(lockRecord.ScopeRef, replayKey.ScopeRef, StringComparison.Ordinal))
        {
            return "durable_idempotency_lock_scope_mismatch";
        }

        if (string.IsNullOrWhiteSpace(lockRecord.RequestDigest) ||
            !lockRecord.RequestDigest.StartsWith("sha256:design-conversion-route-admission-request-digest:", StringComparison.Ordinal))
        {
            return "durable_idempotency_lock_digest_invalid";
        }

        if (!DateTimeOffset.TryParse(
                lockRecord.CreatedAtUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var created))
        {
            return "durable_idempotency_lock_created_invalid";
        }

        if (!DateTimeOffset.TryParse(
                lockRecord.ExpiresAtUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var expires))
        {
            return "durable_idempotency_lock_expires_invalid";
        }

        if (expires <= created)
            return "durable_idempotency_lock_expiry_invalid";
        if (!string.Equals(lockRecord.IntegritySha256, ComputeLockIntegrity(lockRecord), StringComparison.Ordinal))
            return "durable_idempotency_lock_integrity_mismatch";

        if (now >= expires)
            return "expired";

        return string.Equals(lockRecord.RequestDigest, DigestRef(requestDigest), StringComparison.Ordinal)
            ? "durable_idempotency_lock_in_doubt"
            : "conflict";
    }

    private StoreSafetyResult EnsureSafeStore()
    {
        if (_initializationError is not null || _root is null || _recordsRoot is null || _locksRoot is null)
            return StoreSafetyResult.Failed(_initializationError ?? "durable_store_root_invalid");

        try
        {
            if (AnyExistingPathSegmentIsReparsePoint(_root))
                return StoreSafetyResult.Failed("durable_store_root_reparse_point");

            Directory.CreateDirectory(_root);
            Directory.CreateDirectory(_recordsRoot);
            Directory.CreateDirectory(_locksRoot);
            if (DirectoryHasReparsePoint(_root) ||
                DirectoryHasReparsePoint(_recordsRoot) ||
                DirectoryHasReparsePoint(_locksRoot) ||
                AnyImmediateChildIsReparsePoint(_recordsRoot) ||
                AnyImmediateChildIsReparsePoint(_locksRoot))
            {
                return StoreSafetyResult.Failed("durable_store_root_reparse_point");
            }

            return StoreSafetyResult.Success;
        }
        catch (Exception ex) when (IsSafeIoException(ex))
        {
            return StoreSafetyResult.Failed("durable_store_root_unavailable");
        }
    }

    private void PruneCompletedRecords(
        DateTimeOffset now,
        TimeSpan ttl,
        int capacity,
        string? protectedRecordFileName)
    {
        if (_recordsRoot is null)
            return;

        try
        {
            foreach (var tempPath in Directory.EnumerateFiles(_recordsRoot, "*.tmp", SearchOption.TopDirectoryOnly))
            {
                var tempInfo = new FileInfo(tempPath);
                if (now - tempInfo.CreationTimeUtc >= TempFileMaxAge)
                    TryDeleteFile(tempPath);
            }

            var records = new List<(string Path, DateTimeOffset CreatedAtUtc, DateTimeOffset ExpiresAtUtc)>();
            foreach (var path in Directory.EnumerateFiles(_recordsRoot, "*.json", SearchOption.TopDirectoryOnly))
            {
                var fileName = Path.GetFileName(path);
                if (string.Equals(fileName, protectedRecordFileName, StringComparison.Ordinal))
                    continue;

                if (TryReadRecordTimes(path, out var createdAt, out var expiresAt))
                {
                    if (now >= expiresAt || now - createdAt >= ttl)
                    {
                        TryDeleteFile(path);
                        continue;
                    }

                    records.Add((path, createdAt, expiresAt));
                }
                else
                {
                    TryDeleteFile(path);
                }
            }

            var boundedCapacity = Math.Clamp(capacity, 1, 4096);
            var overflow = records.Count - boundedCapacity + 1;
            if (overflow <= 0)
                return;

            foreach (var record in records
                         .OrderBy(item => item.CreatedAtUtc)
                         .Take(overflow))
            {
                TryDeleteFile(record.Path);
            }
        }
        catch (Exception ex) when (IsSafeIoException(ex))
        {
        }
    }

    private bool TryReadRecordTimes(
        string path,
        out DateTimeOffset createdAt,
        out DateTimeOffset expiresAt)
    {
        createdAt = DateTimeOffset.MinValue;
        expiresAt = DateTimeOffset.MinValue;

        try
        {
            if (ExistingPathIsReparsePoint(path))
                return false;

            var snapshot = TryReadBoundedStrictUtf8Snapshot(path, MaxRecordBytes);
            if (!snapshot.IsSuccess)
                return false;

            var record = JsonSerializer.Deserialize<DurableReplayRecordDto>(snapshot.Json, JsonOptions);
            if (record is null ||
                record.SchemaVersion != SchemaVersion ||
                !DateTimeOffset.TryParse(
                    record.CreatedAtUtc,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out createdAt) ||
                !DateTimeOffset.TryParse(
                    record.ExpiresAtUtc,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out expiresAt))
            {
                return false;
            }

            return true;
        }
        catch (Exception ex) when (ex is JsonException || IsSafeIoException(ex))
        {
            return false;
        }
    }

    private static bool TryValidateResponseBounds(
        DesignConversionModelServerHttpResponse response,
        out string responseBodySha256,
        out IReadOnlyDictionary<string, string> headers,
        out string errorRef)
    {
        responseBodySha256 = string.Empty;
        headers = new Dictionary<string, string>(StringComparer.Ordinal);
        errorRef = "none";

        if (string.IsNullOrWhiteSpace(response.ContentType) ||
            response.ContentType.Length > MaxContentTypeLength)
        {
            errorRef = "durable_idempotency_response_content_type_invalid";
            return false;
        }

        if (response.BodyJson is null ||
            Encoding.UTF8.GetByteCount(response.BodyJson) > MaxResponseBodyBytes)
        {
            errorRef = "durable_idempotency_response_body_oversized";
            return false;
        }

        var safeHeaders = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in response.Headers ?? new Dictionary<string, string>())
        {
            if (safeHeaders.Count >= MaxHeaderCount ||
                string.IsNullOrWhiteSpace(pair.Key) ||
                pair.Key.Length > MaxHeaderNameLength ||
                pair.Value is null ||
                pair.Value.Length > MaxHeaderValueLength ||
                pair.Key.Any(char.IsControl) ||
                pair.Value.Any(char.IsControl))
            {
                errorRef = "durable_idempotency_response_headers_invalid";
                return false;
            }

            safeHeaders[pair.Key] = pair.Value;
        }

        responseBodySha256 = Sha256Ref("response-body", response.BodyJson);
        headers = safeHeaders;
        return true;
    }

    private static bool HeadersWithinBounds(IReadOnlyDictionary<string, string>? headers)
    {
        if (headers is null)
            return true;
        if (headers.Count > MaxHeaderCount)
            return false;

        return headers.All(pair =>
            !string.IsNullOrWhiteSpace(pair.Key) &&
            pair.Key.Length <= MaxHeaderNameLength &&
            pair.Value is not null &&
            pair.Value.Length <= MaxHeaderValueLength &&
            !pair.Key.Any(char.IsControl) &&
            !pair.Value.Any(char.IsControl));
    }

    private string RecordPath(DurableReplayKey replayKey)
        => Path.Combine(_recordsRoot!, replayKey.RecordFileName);

    private string LockPath(DurableReplayKey replayKey)
        => Path.Combine(_locksRoot!, replayKey.RecordFileName + ".lock");

    private static string? TryReadJobIdRef(string responseBodyJson)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBodyJson);
            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty("jobId", out var jobIdElement) &&
                jobIdElement.ValueKind == JsonValueKind.String)
            {
                var jobId = jobIdElement.GetString();
                return string.IsNullOrWhiteSpace(jobId)
                    ? null
                    : Sha256Ref("public-job-id", jobId.Trim());
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }

    private static string ComputeIntegrity(DurableReplayRecordDto record)
    {
        var material = string.Join(
            "\n",
            "design-conversion-route-admission-durable-record-v1",
            record.SchemaVersion.ToString(CultureInfo.InvariantCulture),
            record.RecordKind ?? string.Empty,
            record.KeyRef ?? string.Empty,
            record.ScopeRef ?? string.Empty,
            record.RequestDigest ?? string.Empty,
            record.ResponseStatusCode.ToString(CultureInfo.InvariantCulture),
            record.ResponseContentType ?? string.Empty,
            record.ResponseBodySha256 ?? string.Empty,
            record.ResponseBodyJson ?? string.Empty,
            SerializeHeadersForIntegrity(record.ResponseHeaders),
            record.JobIdRef ?? string.Empty,
            record.CreatedAtUtc ?? string.Empty,
            record.ExpiresAtUtc ?? string.Empty);
        return Sha256Ref("record-integrity", material);
    }

    private static string ComputeLockIntegrity(DurableReplayLockDto lockRecord)
    {
        var material = string.Join(
            "\n",
            "design-conversion-route-admission-durable-lock-v1",
            lockRecord.SchemaVersion.ToString(CultureInfo.InvariantCulture),
            lockRecord.LockKind ?? string.Empty,
            lockRecord.KeyRef ?? string.Empty,
            lockRecord.ScopeRef ?? string.Empty,
            lockRecord.RequestDigest ?? string.Empty,
            lockRecord.CreatedAtUtc ?? string.Empty,
            lockRecord.ExpiresAtUtc ?? string.Empty);
        return Sha256Ref("lock-integrity", material);
    }

    private static string SerializeHeadersForIntegrity(IReadOnlyDictionary<string, string>? headers)
        => string.Join(
            "\n",
            (headers ?? new Dictionary<string, string>())
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => pair.Key + ":" + pair.Value));

    private static string DigestRef(byte[] digest)
        => "sha256:design-conversion-route-admission-request-digest:" +
           Convert.ToHexString(digest).ToLowerInvariant();

    private static string Sha256Ref(string domain, string value)
        => "sha256:design-conversion-route-admission-" + domain + ":" + Sha256Hex(domain + "\n" + value);

    private static string Sha256Hex(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        try
        {
            return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        }
        finally
        {
            CryptographicOperations.ZeroMemory(bytes);
        }
    }

    private static DateTimeOffset SafeAdd(DateTimeOffset value, TimeSpan offset)
    {
        if (offset <= TimeSpan.Zero)
            return value;

        try
        {
            return value.Add(offset);
        }
        catch (ArgumentOutOfRangeException)
        {
            return DateTimeOffset.MaxValue;
        }
    }

    private static bool DirectoryHasReparsePoint(string path)
        => Directory.Exists(path) &&
           new DirectoryInfo(path).Attributes.HasFlag(FileAttributes.ReparsePoint);

    private static bool AnyImmediateChildIsReparsePoint(string path)
    {
        try
        {
            if (!Directory.Exists(path))
                return false;

            return Directory
                .EnumerateFileSystemEntries(path, "*", SearchOption.TopDirectoryOnly)
                .Any(ExistingPathIsReparsePoint);
        }
        catch (Exception ex) when (IsSafeIoException(ex))
        {
            return true;
        }
    }

    private static bool ExistingPathIsReparsePoint(string path)
    {
        try
        {
            return File.Exists(path) &&
                   File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint);
        }
        catch (Exception ex) when (IsSafeIoException(ex))
        {
            return false;
        }
    }

    private static bool AnyExistingPathSegmentIsReparsePoint(string fullPath)
    {
        var directory = new DirectoryInfo(fullPath);
        while (directory is not null)
        {
            if (directory.Exists &&
                directory.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                return true;
            }

            directory = directory.Parent;
        }

        return false;
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex) when (IsSafeIoException(ex))
        {
        }
    }

    private static bool IsSafeIoException(Exception ex)
        => ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException or PathTooLongException;

    private static BoundedStrictUtf8SnapshotResult TryReadBoundedStrictUtf8Snapshot(
        string path,
        int maxBytes)
    {
        try
        {
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete,
                4096,
                FileOptions.SequentialScan);
            return TryReadBoundedStrictUtf8Snapshot(stream, maxBytes);
        }
        catch (Exception ex) when (IsSafeIoException(ex))
        {
            return BoundedStrictUtf8SnapshotResult.Corrupt;
        }
    }

    internal static BoundedStrictUtf8SnapshotResult TryReadBoundedStrictUtf8Snapshot(
        Stream stream,
        int maxBytes)
    {
        try
        {
            if (!stream.CanRead || maxBytes < 0)
                return BoundedStrictUtf8SnapshotResult.Corrupt;
            if (stream.CanSeek)
                stream.Position = 0;

            var declaredLength = stream.Length;
            if (declaredLength < 0 || declaredLength > maxBytes || declaredLength > int.MaxValue)
                return BoundedStrictUtf8SnapshotResult.Oversized;

            var bytes = declaredLength == 0
                ? Array.Empty<byte>()
                : new byte[(int)declaredLength];
            var offset = 0;
            while (offset < bytes.Length)
            {
                var read = stream.Read(bytes, offset, bytes.Length - offset);
                if (read <= 0)
                    return BoundedStrictUtf8SnapshotResult.Corrupt;
                offset += read;
            }

            if (stream.ReadByte() >= 0)
                return BoundedStrictUtf8SnapshotResult.Oversized;

            return BoundedStrictUtf8SnapshotResult.Success(StrictUtf8.GetString(bytes));
        }
        catch (DecoderFallbackException)
        {
            return BoundedStrictUtf8SnapshotResult.Corrupt;
        }
        catch (Exception ex) when (IsSafeIoException(ex))
        {
            return BoundedStrictUtf8SnapshotResult.Corrupt;
        }
    }

    internal sealed record BoundedStrictUtf8SnapshotResult(
        bool IsSuccess,
        string Json,
        string ErrorRef)
    {
        public static BoundedStrictUtf8SnapshotResult Corrupt { get; } =
            new(false, string.Empty, "durable_idempotency_record_corrupt");

        public static BoundedStrictUtf8SnapshotResult Oversized { get; } =
            new(false, string.Empty, "durable_idempotency_record_oversized");

        public static BoundedStrictUtf8SnapshotResult Success(string json)
            => new(true, json, "none");
    }

    private sealed record StoreSafetyResult(bool IsSuccess, string ErrorRef)
    {
        public static StoreSafetyResult Success { get; } = new(true, "none");

        public static StoreSafetyResult Failed(string errorRef)
            => new(false, SanitizeErrorRef(errorRef));
    }

    private sealed record DurableReplayKey(
        string RecordFileName,
        string KeyRef,
        string ScopeRef)
    {
        public static DurableReplayKey Create(string scope, string idempotencyKey)
        {
            var scopeRef = Sha256Ref("scope-ref", scope);
            var keyHash = Sha256Hex(
                "design-conversion-route-admission-idempotency-key-v1\n" +
                scope +
                "\n" +
                idempotencyKey);
            return new DurableReplayKey(
                keyHash + ".json",
                "sha256:design-conversion-route-admission-idempotency-key:" + keyHash,
                scopeRef);
        }
    }

    private sealed class DurableReplayRecordDto
    {
        public int SchemaVersion { get; set; }

        public string? RecordKind { get; set; }

        public string? KeyRef { get; set; }

        public string? ScopeRef { get; set; }

        public string? RequestDigest { get; set; }

        public int ResponseStatusCode { get; set; }

        public string? ResponseContentType { get; set; }

        public string? ResponseBodyJson { get; set; }

        public string? ResponseBodySha256 { get; set; }

        public IReadOnlyDictionary<string, string>? ResponseHeaders { get; set; }

        public string? JobIdRef { get; set; }

        public string? CreatedAtUtc { get; set; }

        public string? ExpiresAtUtc { get; set; }

        public string? IntegritySha256 { get; set; }
    }

    private sealed class DurableReplayLockDto
    {
        public int SchemaVersion { get; set; }

        public string? LockKind { get; set; }

        public string? KeyRef { get; set; }

        public string? ScopeRef { get; set; }

        public string? RequestDigest { get; set; }

        public string? CreatedAtUtc { get; set; }

        public string? ExpiresAtUtc { get; set; }

        public string? IntegritySha256 { get; set; }
    }

    private static string SanitizeErrorRef(string? errorRef)
    {
        if (string.IsNullOrWhiteSpace(errorRef))
            return "durable_idempotency_failed_closed";

        var safe = new string(errorRef
            .Where(ch => ch is >= 'a' and <= 'z' or >= '0' and <= '9' or '_')
            .Take(MaxErrorRefLength)
            .ToArray());
        return string.IsNullOrWhiteSpace(safe)
            ? "durable_idempotency_failed_closed"
            : safe;
    }
}
