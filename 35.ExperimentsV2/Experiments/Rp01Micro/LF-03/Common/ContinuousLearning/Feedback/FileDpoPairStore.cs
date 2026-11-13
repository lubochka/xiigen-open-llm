// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Buffers.Binary;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO.Models;
using Microsoft.Win32.SafeHandles;

// Durable, server-rooted storage owned by FeedbackRouter.
// One canonical file per semantic pair makes retries idempotent and keeps
// partially written temporary files outside the reader's enumeration pattern.
// Windows uses a non-delete/non-write-shared handle plus final-path checks.
// Linux uses /proc/self/fd final-path checks plus two bounded reads from the
// same handle. Other Unix platforms fail closed because an equivalent opened
// handle path check is not implemented here.
// Every mutation also owns a normalized-root process gate and then an explicit
// byte-range lock on the root-backed lock file. This coordinates cooperating
// callers on one host only when the root is on a local locking-capable
// filesystem, even when implicit Unix FileShare locking is disabled. Unsupported
// or denied explicit locks fail closed. Hostile external writers and every
// distributed/network filesystem are outside the guarantee.
// On Windows, files inherit ACLs from the server-owned configured root. The
// deployment must provision that root with an owner-only ACL; this class never
// broadens inherited ACLs and does not claim to verify Windows ACL policy.
internal sealed partial class FileDpoPairStore : IDpoPairBatchReservationStore
{
    internal const int DefaultMaximumRecordCount = 10_000;
    private const int LegacySchemaVersion = 1;
    private const int ProvenanceSchemaVersion = 2;
    private const int ArtifactProvenanceSchemaVersion = 3;
    private const int ContentBoundSchemaVersion = 4;
    private const int CurrentSchemaVersion = 5;
    private const long MaximumRecordBytes = 16L * 1024L * 1024L;
    private const int MaximumFieldBytes = 4 * 1024 * 1024;
    private static readonly TimeSpan DefaultMutationLockTimeout = TimeSpan.FromSeconds(10);
    private const string CanonicalSuffix = ".dpo-pair.json";
    private const string CurrentRecordDomain = "xiigen.dpo-pair.v5";
    private const string AtomicGroupSuffix = ".dpo-pair-group.json";
    private const string AtomicGroupDomain = "xiigen.dpo-pair-atomic-group.v1";
    private const string MutationLockFileName = ".dpo-pair-store.lock";
    private const string BatchReservationFileName =
        ".dpo-pair-batch-reservation.json";
    private const string BatchReservationSchemaVersion =
        "dpo-pair-batch-reservation-v1";
    private const int MaximumBatchReservationBytes = 2 * 1024 * 1024;
    private const string CapacityMessage = "DPO pair storage capacity is exhausted.";
    private const string BusyMessage = "DPO pair storage is busy.";
    private const string UnavailableMessage = "DPO pair storage is unavailable.";
    private const string CorruptMessage = "DPO pair storage contains an invalid record.";
    private const string CollisionMessage = "DPO pair identity collision was detected.";
    private const long MutationLockOffset = 0;
    private const long MutationLockLength = 1;
    private static readonly UnixFileMode OwnerDirectoryMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
    private static readonly object ProcessGateRegistrySync = new();
    private static readonly Dictionary<string, ProcessGateEntry> ProcessGateRegistry =
        new(OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal);
    private readonly object _gate = new();
    private readonly string? _storageRoot;
    private readonly string? _mutationLockPath;
    private readonly int _maximumRecordCount;
    private readonly int _maximumTransactionPairCount;
    private readonly long _maximumTransactionRecordBytes;
    private readonly Action? _afterMutationLockAcquiredForTesting;
    private readonly Func<IOException?>? _mutationRegionLockFailureForTesting;
    private readonly bool _corruptMutationLockLengthAfterAcquisitionForTesting;
    private readonly bool _unsupportedPlatformForTesting;
    private readonly Action?
        _afterReplayLedgerWrittenBeforePairCompactionForTesting;
    private readonly TimeSpan _mutationLockTimeout;

    public FileDpoPairStore(
        string? storageRoot,
        int maximumRecordCount = DefaultMaximumRecordCount,
        Action? afterMutationLockAcquiredForTesting = null,
        TimeSpan? mutationLockTimeoutForTesting = null,
        Func<IOException?>? mutationRegionLockFailureForTesting = null,
        bool corruptMutationLockLengthAfterAcquisitionForTesting = false,
        bool unsupportedPlatformForTesting = false,
        long? maximumTransactionRecordBytesForTesting = null,
        Action? afterReplayLedgerWrittenBeforePairCompactionForTesting = null,
        int? maximumTransactionPairCountForTesting = null)
    {
        if (maximumRecordCount <= 0
            || maximumRecordCount > DefaultMaximumRecordCount)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumRecordCount));
        }

        _maximumRecordCount = maximumRecordCount;
        _maximumTransactionPairCount =
            maximumTransactionPairCountForTesting ?? MaximumTransactionPairCount;
        if (_maximumTransactionPairCount <= 0
            || _maximumTransactionPairCount > MaximumTransactionPairCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumTransactionPairCountForTesting));
        }
        _maximumTransactionRecordBytes =
            maximumTransactionRecordBytesForTesting ?? MaximumRecordBytes;
        if (_maximumTransactionRecordBytes <= 0
            || _maximumTransactionRecordBytes > MaximumRecordBytes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumTransactionRecordBytesForTesting));
        }
        _afterMutationLockAcquiredForTesting = afterMutationLockAcquiredForTesting;
        _mutationRegionLockFailureForTesting =
            mutationRegionLockFailureForTesting;
        _corruptMutationLockLengthAfterAcquisitionForTesting =
            corruptMutationLockLengthAfterAcquisitionForTesting;
        _unsupportedPlatformForTesting = unsupportedPlatformForTesting;
        _afterReplayLedgerWrittenBeforePairCompactionForTesting =
            afterReplayLedgerWrittenBeforePairCompactionForTesting;
        _mutationLockTimeout =
            mutationLockTimeoutForTesting ?? DefaultMutationLockTimeout;
        if (_mutationLockTimeout <= TimeSpan.Zero
            || _mutationLockTimeout > DefaultMutationLockTimeout)
        {
            throw new ArgumentOutOfRangeException(
                nameof(mutationLockTimeoutForTesting));
        }
        if (string.IsNullOrWhiteSpace(storageRoot))
            return;

        try
        {
            _storageRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(storageRoot));
            _mutationLockPath = Path.Combine(
                _storageRoot,
                MutationLockFileName);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            _storageRoot = null;
        }
    }

    public OperationResult<string> Add(DpoPair pair)
    {
        var validation = ValidatePair(pair);
        if (!validation.IsSuccess)
            return validation.MapError<string>();

        if (_storageRoot is null || _mutationLockPath is null)
            return FailUnavailable<string>();

        var timeoutBudget = Stopwatch.StartNew();
        var processLeaseResult = AcquireProcessMutationLease(timeoutBudget);
        if (!processLeaseResult.IsSuccess)
            return processLeaseResult.MapError<string>();
        using var processLease = processLeaseResult.Value!;

        // The lock file lives inside the configured root. Creating the
        // directory is an idempotent prerequisite; root integrity and
        // writability are checked again while both mutation leases are held.
        var lockRootReady = EnsureWritableRoot();
        if (!lockRootReady.IsSuccess)
            return lockRootReady.MapError<string>();

        var leaseResult = AcquireMutationLease(timeoutBudget);
        if (!leaseResult.IsSuccess)
            return leaseResult.MapError<string>();
        // Declaration order is intentional: the OS lease is disposed before
        // the process lease on every return path.
        using var lease = leaseResult.Value!;
        _afterMutationLockAcquiredForTesting?.Invoke();

        var rootReady = EnsureWritableRoot();
        if (!rootReady.IsSuccess
            || !lease.MatchesRequestedPath(_mutationLockPath)
            || !lease.HasExpectedLength()
            || AnyExistingPathSegmentIsReparsePoint(_storageRoot)
            || AnyExistingPathSegmentIsReparsePoint(_mutationLockPath))
        {
            if (!rootReady.IsSuccess)
                return rootReady.MapError<string>();
            return FailUnavailable<string>();
        }

        var schemaVersion = GetWriteSchemaVersion(pair);
        var identity = ComputeIdentity(pair, schemaVersion);
        var snapshot = LoadSnapshot(pairIdentityForCollision: identity);
        if (!snapshot.IsSuccess)
            return snapshot.MapError<string>();
        var reservation = ReadBatchReservation();
        if (!reservation.IsSuccess)
            return reservation.MapError<string>();
        var retainedReservation = reservation.Value;
        if (retainedReservation is not null &&
            !retainedReservation.PairIds.Contains(identity, StringComparer.Ordinal))
        {
            return FailBusy<string>();
        }
        var transactions = LoadTransactions();
        if (!transactions.IsSuccess || transactions.Value is null)
            return transactions.MapError<string>();
        var replayLedger = LoadReplayLedger();
        if (!replayLedger.IsSuccess || replayLedger.Value is null)
            return replayLedger.MapError<string>();

        if (snapshot.Value!.RecordsByIdentity.TryGetValue(identity, out var existing))
        {
            if (!SameSemanticPair(existing, pair))
                return FailCollision<string>();
            if (retainedReservation is not null &&
                retainedReservation.PairIds.All(pairId =>
                    snapshot.Value.RecordsByIdentity.ContainsKey(pairId)))
            {
                DeleteCompletedBatchReservation();
            }
            return OperationResult<string>.Ok(identity);
        }

        // A retained transaction receipt is a durable tombstone for its exact
        // semantic pair identity. Replayed identical feedback remains
        // idempotent and never re-enters the pending queue.
        if (transactions.Value.Any(transaction => transaction.Pairs.Any(pair =>
                StringComparer.Ordinal.Equals(pair.PairId, identity)))
            || replayLedger.Value.Entries.Any(entry =>
                ReplayPairIds(entry).Contains(identity, StringComparer.Ordinal)))
        {
            return OperationResult<string>.Ok(identity);
        }

        if (pair.TeacherProvenance is { } provenance
            && (snapshot.Value.RecordsByIdentity.Values.Any(value =>
                    StringComparer.Ordinal.Equals(
                        value.TeacherProvenance?.RequestId,
                        provenance.RequestId))
                || transactions.Value.Any(transaction =>
                    transaction.Requests.Any(request =>
                        StringComparer.Ordinal.Equals(
                            request.RequestId,
                            provenance.RequestId)))
                || replayLedger.Value.Entries.Any(entry =>
                    StringComparer.Ordinal.Equals(
                        entry.RequestId,
                        provenance.RequestId))))
        {
            return OperationResult<string>.Fail(
                "A teacher request id has already been retained by durable DPO storage.",
                LegalTrainingTeacherAdmissionAuthority
                    .DuplicateRequestIdErrorCode);
        }

        if (transactions.Value.Count >= MaximumRetainedTransactionCount)
            return FailTransactionCapacity<string>();
        if (!HasReplayLedgerEntryCapacity(
                replayLedger.Value.Entries.Length,
                requestedEntryCount: 1))
        {
            return FailReplayLedgerCapacity<string>();
        }
        if (snapshot.Value.RecordsByIdentity.Count >= _maximumRecordCount)
            return FailCapacity<string>();

        var written = WriteCanonicalRecord(identity, pair, schemaVersion);
        if (!written.IsSuccess || retainedReservation is null)
            return written;
        var afterWrite = LoadSnapshot(pairIdentityForCollision: null);
        if (afterWrite.IsSuccess && retainedReservation.PairIds.All(
                pairId => afterWrite.Value!.RecordsByIdentity.ContainsKey(pairId)))
        {
            DeleteCompletedBatchReservation();
        }
        return written;
    }

    public OperationResult<DpoPairBatchReservation> ReserveBatch(
        string reservationId,
        IReadOnlyList<DpoPair> pairs)
    {
        if (string.IsNullOrWhiteSpace(reservationId) ||
            pairs is null ||
            pairs.Count == 0 ||
            pairs.Count > _maximumRecordCount)
        {
            return OperationResult<DpoPairBatchReservation>.Fail(
                "DPO pair batch reservation is invalid.",
                "INVALID_DPO_PAIR_BATCH_RESERVATION");
        }
        foreach (var pair in pairs)
        {
            var validation = ValidatePair(pair);
            if (!validation.IsSuccess)
                return validation.MapError<DpoPairBatchReservation>();
        }
        if (_storageRoot is null || _mutationLockPath is null)
            return FailUnavailable<DpoPairBatchReservation>();

        var timeoutBudget = Stopwatch.StartNew();
        var processLeaseResult = AcquireProcessMutationLease(timeoutBudget);
        if (!processLeaseResult.IsSuccess)
            return processLeaseResult.MapError<DpoPairBatchReservation>();
        using var processLease = processLeaseResult.Value!;
        var lockRootReady = EnsureWritableRoot();
        if (!lockRootReady.IsSuccess)
            return lockRootReady.MapError<DpoPairBatchReservation>();
        var leaseResult = AcquireMutationLease(timeoutBudget);
        if (!leaseResult.IsSuccess)
            return leaseResult.MapError<DpoPairBatchReservation>();
        using var lease = leaseResult.Value!;
        _afterMutationLockAcquiredForTesting?.Invoke();
        var rootReady = EnsureWritableRoot();
        if (!rootReady.IsSuccess ||
            !lease.MatchesRequestedPath(_mutationLockPath) ||
            !lease.HasExpectedLength() ||
            AnyExistingPathSegmentIsReparsePoint(_storageRoot))
        {
            return rootReady.IsSuccess
                ? FailUnavailable<DpoPairBatchReservation>()
                : rootReady.MapError<DpoPairBatchReservation>();
        }

        var identifiedPairs = pairs
            .Select(pair => (
                Pair: pair,
                Identity: ComputeIdentity(pair, GetWriteSchemaVersion(pair))))
            .ToArray();
        if (identifiedPairs.Select(item => item.Identity)
                .Distinct(StringComparer.Ordinal).Count() != identifiedPairs.Length)
        {
            return OperationResult<DpoPairBatchReservation>.Fail(
                "DPO pair batch reservation contains a duplicate semantic identity.",
                "DPO_PAIR_BATCH_RESERVATION_DUPLICATE_IDENTITY");
        }
        var requestIds = identifiedPairs
            .Select(item => item.Pair.TeacherProvenance?.RequestId)
            .Where(requestId => !string.IsNullOrWhiteSpace(requestId))
            .ToArray();
        if (requestIds.Distinct(StringComparer.Ordinal).Count() !=
            requestIds.Length)
        {
            return OperationResult<DpoPairBatchReservation>.Fail(
                "A teacher request id is duplicated within the DPO pair batch.",
                LegalTrainingTeacherAdmissionAuthority
                    .DuplicateRequestIdErrorCode);
        }
        var identities = identifiedPairs
            .Select(item => item.Identity)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var batchSha256 = BuildBatchReservationSha256(identities);
        var requested = new StoredBatchReservation(
            BatchReservationSchemaVersion,
            1,
            reservationId,
            batchSha256,
            identities.Length,
            identities);
        var existing = ReadBatchReservation();
        if (!existing.IsSuccess)
            return existing.MapError<DpoPairBatchReservation>();
        var snapshot = LoadSnapshot(pairIdentityForCollision: null);
        if (!snapshot.IsSuccess)
            return snapshot.MapError<DpoPairBatchReservation>();
        var transactions = LoadTransactions();
        if (!transactions.IsSuccess || transactions.Value is null)
            return transactions.MapError<DpoPairBatchReservation>();
        var replayLedger = LoadReplayLedger();
        if (!replayLedger.IsSuccess || replayLedger.Value is null)
            return replayLedger.MapError<DpoPairBatchReservation>();
        if (existing.Value is { } retained)
        {
            if (string.Equals(
                    retained.ReservationId,
                    requested.ReservationId,
                    StringComparison.Ordinal) &&
                string.Equals(
                    retained.BatchSha256,
                    requested.BatchSha256,
                    StringComparison.Ordinal) &&
                retained.PairIds.SequenceEqual(
                    requested.PairIds,
                    StringComparer.Ordinal))
            {
                return OperationResult<DpoPairBatchReservation>.Ok(
                    ToPublicReservation(retained));
            }
            if (!retained.PairIds.All(
                    id => snapshot.Value!.RecordsByIdentity.ContainsKey(id)))
            {
                return FailBusy<DpoPairBatchReservation>();
            }
            DeleteCompletedBatchReservation();
        }

        foreach (var item in identifiedPairs)
        {
            if (snapshot.Value!.RecordsByIdentity.TryGetValue(
                    item.Identity,
                    out var existingPair) &&
                !SameSemanticPair(existingPair, item.Pair))
            {
                return FailCollision<DpoPairBatchReservation>();
            }
            if (transactions.Value.Any(transaction => transaction.Pairs.Any(
                    pair => string.Equals(
                        pair.PairId,
                        item.Identity,
                        StringComparison.Ordinal))) ||
                replayLedger.Value.Entries.Any(entry => string.Equals(
                    entry.PairId,
                    item.Identity,
                    StringComparison.Ordinal)))
            {
                return OperationResult<DpoPairBatchReservation>.Fail(
                    "A reserved DPO pair identity has already been drained.",
                    "DPO_PAIR_BATCH_RESERVATION_IDENTITY_ALREADY_DRAINED");
            }

            var requestId = item.Pair.TeacherProvenance?.RequestId;
            if (string.IsNullOrWhiteSpace(requestId) ||
                snapshot.Value.RecordsByIdentity.ContainsKey(item.Identity))
            {
                continue;
            }
            if (snapshot.Value.RecordsByIdentity.Values.Any(value =>
                    string.Equals(
                        value.TeacherProvenance?.RequestId,
                        requestId,
                        StringComparison.Ordinal)) ||
                transactions.Value.Any(transaction => transaction.Requests.Any(
                    request => string.Equals(
                        request.RequestId,
                        requestId,
                        StringComparison.Ordinal))) ||
                replayLedger.Value.Entries.Any(entry => string.Equals(
                    entry.RequestId,
                    requestId,
                    StringComparison.Ordinal)))
            {
                return OperationResult<DpoPairBatchReservation>.Fail(
                    "A teacher request id has already been retained by durable DPO storage.",
                    LegalTrainingTeacherAdmissionAuthority
                        .DuplicateRequestIdErrorCode);
            }
        }
        var missingCount = identities.Count(
            id => !snapshot.Value!.RecordsByIdentity.ContainsKey(id));
        if (transactions.Value.Count >= MaximumRetainedTransactionCount)
            return FailTransactionCapacity<DpoPairBatchReservation>();
        if (!HasReplayLedgerEntryCapacity(
                replayLedger.Value.Entries.Length,
                Math.Max(1, missingCount)))
        {
            return FailReplayLedgerCapacity<DpoPairBatchReservation>();
        }
        if (missingCount > _maximumRecordCount -
            snapshot.Value!.RecordsByIdentity.Count)
        {
            return FailCapacity<DpoPairBatchReservation>();
        }
        var written = WriteBatchReservation(requested);
        return written.IsSuccess
            ? OperationResult<DpoPairBatchReservation>.Ok(
                ToPublicReservation(requested))
            : written.MapError<DpoPairBatchReservation>();
    }

    public OperationResult<bool> ReleaseBatch(
        DpoPairBatchReservation reservation)
    {
        if (reservation is null ||
            string.IsNullOrWhiteSpace(reservation.ReservationId) ||
            !IsCanonicalSha256Ref(reservation.BatchSha256) ||
            reservation.PairCount <= 0)
        {
            return OperationResult<bool>.Fail(
                "DPO pair batch reservation release is invalid.",
                "INVALID_DPO_PAIR_BATCH_RESERVATION");
        }
        if (_storageRoot is null || _mutationLockPath is null)
            return FailUnavailable<bool>();
        var timeoutBudget = Stopwatch.StartNew();
        var processLeaseResult = AcquireProcessMutationLease(timeoutBudget);
        if (!processLeaseResult.IsSuccess)
            return processLeaseResult.MapError<bool>();
        using var processLease = processLeaseResult.Value!;
        var lockRootReady = EnsureWritableRoot();
        if (!lockRootReady.IsSuccess)
            return lockRootReady;
        var leaseResult = AcquireMutationLease(timeoutBudget);
        if (!leaseResult.IsSuccess)
            return leaseResult.MapError<bool>();
        using var lease = leaseResult.Value!;
        _afterMutationLockAcquiredForTesting?.Invoke();
        var rootReady = EnsureWritableRoot();
        if (!rootReady.IsSuccess ||
            !lease.MatchesRequestedPath(_mutationLockPath) ||
            !lease.HasExpectedLength() ||
            AnyExistingPathSegmentIsReparsePoint(_storageRoot))
        {
            return rootReady.IsSuccess
                ? FailUnavailable<bool>()
                : rootReady;
        }
        var retained = ReadBatchReservation();
        if (!retained.IsSuccess)
            return retained.MapError<bool>();
        if (retained.Value is null)
            return OperationResult<bool>.Ok(true);
        if (!string.Equals(
                retained.Value.ReservationId,
                reservation.ReservationId,
                StringComparison.Ordinal) ||
            !string.Equals(
                retained.Value.BatchSha256,
                reservation.BatchSha256,
                StringComparison.Ordinal) ||
            retained.Value.PairCount != reservation.PairCount)
        {
            return FailCollision<bool>();
        }
        try
        {
            var path = Path.Combine(_storageRoot, BatchReservationFileName);
            if (AnyExistingPathSegmentIsReparsePoint(path))
                return FailUnavailable<bool>();
            File.Delete(path);
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FailUnavailable<bool>();
        }
    }

    private sealed record StoredBatchReservation(
        string SchemaVersion,
        int Version,
        string ReservationId,
        string BatchSha256,
        int PairCount,
        IReadOnlyList<string> PairIds);

    private static DpoPairBatchReservation ToPublicReservation(
        StoredBatchReservation reservation)
        => new(
            reservation.ReservationId,
            reservation.BatchSha256,
            reservation.PairCount);

    private static string BuildBatchReservationSha256(
        IReadOnlyList<string> sortedPairIds)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(string.Join("\n", sortedPairIds))))
            .ToLowerInvariant();

    private static bool IsCanonicalSha256Ref(string value)
        => value is { Length: 71 } &&
           value.StartsWith("sha256:", StringComparison.Ordinal) &&
           value.AsSpan(7).ToString().All(character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static bool IsCanonicalSha256Hex(string value)
        => value is { Length: 64 }
           && value.All(character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private OperationResult<StoredBatchReservation?> ReadBatchReservation()
    {
        var path = Path.Combine(_storageRoot!, BatchReservationFileName);
        if (!File.Exists(path))
            return OperationResult<StoredBatchReservation?>.Ok(null);
        try
        {
            if (AnyExistingPathSegmentIsReparsePoint(path))
                return FailCorrupt<StoredBatchReservation?>();
            var info = new FileInfo(path);
            if (info.Length <= 0 || info.Length > MaximumBatchReservationBytes)
                return FailCorrupt<StoredBatchReservation?>();
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.SequentialScan);
            if (!OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path))
                return FailCorrupt<StoredBatchReservation?>();
            var bytes = new byte[checked((int)info.Length)];
            stream.ReadExactly(bytes);
            var reservation = JsonSerializer.Deserialize<StoredBatchReservation>(
                bytes);
            if (reservation is null ||
                !string.Equals(
                    reservation.SchemaVersion,
                    BatchReservationSchemaVersion,
                    StringComparison.Ordinal) ||
                reservation.Version != 1 ||
                string.IsNullOrWhiteSpace(reservation.ReservationId) ||
                !IsCanonicalSha256Ref(reservation.BatchSha256) ||
                reservation.PairIds is null ||
                reservation.PairIds.Count == 0 ||
                reservation.PairIds.Count > _maximumRecordCount ||
                reservation.PairCount != reservation.PairIds.Count ||
                reservation.PairIds.Distinct(StringComparer.Ordinal).Count() !=
                reservation.PairIds.Count ||
                !reservation.PairIds.SequenceEqual(
                    reservation.PairIds.Order(StringComparer.Ordinal),
                    StringComparer.Ordinal) ||
                !string.Equals(
                    reservation.BatchSha256,
                    BuildBatchReservationSha256(reservation.PairIds),
                    StringComparison.Ordinal) ||
                !bytes.AsSpan().SequenceEqual(
                    JsonSerializer.SerializeToUtf8Bytes(reservation)))
            {
                return FailCorrupt<StoredBatchReservation?>();
            }
            return OperationResult<StoredBatchReservation?>.Ok(reservation);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FailCorrupt<StoredBatchReservation?>();
        }
    }

    private OperationResult<bool> WriteBatchReservation(
        StoredBatchReservation reservation)
    {
        var canonicalPath = Path.Combine(_storageRoot!, BatchReservationFileName);
        var temporaryPath = Path.Combine(
            _storageRoot!,
            $".{Guid.NewGuid():N}.dpo-pair-batch-reservation.tmp");
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(reservation);
            if (bytes.Length > MaximumBatchReservationBytes ||
                File.Exists(canonicalPath))
            {
                return FailUnavailable<bool>();
            }
            using (var stream = OpenPrivateFile(
                       temporaryPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       FileOptions.WriteThrough,
                       4096))
            {
                stream.Write(bytes);
                stream.Flush(flushToDisk: true);
                if (!OpenedHandleMatchesRequestedPath(
                        stream.SafeFileHandle,
                        temporaryPath))
                {
                    throw new IOException(
                        "DPO pair reservation temporary path integrity check failed.");
                }
            }
            File.Move(temporaryPath, canonicalPath);
            var retained = ReadBatchReservation();
            if (!retained.IsSuccess ||
                retained.Value is null ||
                !string.Equals(
                    retained.Value.ReservationId,
                    reservation.ReservationId,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    retained.Value.BatchSha256,
                    reservation.BatchSha256,
                    StringComparison.Ordinal) ||
                retained.Value.PairCount != reservation.PairCount ||
                !retained.Value.PairIds.SequenceEqual(
                    reservation.PairIds,
                    StringComparer.Ordinal))
            {
                return FailCorrupt<bool>();
            }
            using var readback = new FileStream(
                canonicalPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.SequentialScan);
            if (readback.Length != bytes.Length ||
                !OpenedHandleMatchesRequestedPath(
                    readback.SafeFileHandle,
                    canonicalPath))
            {
                return FailCorrupt<bool>();
            }
            var retainedBytes = new byte[bytes.Length];
            readback.ReadExactly(retainedBytes);
            if (!retainedBytes.AsSpan().SequenceEqual(bytes))
                return FailCorrupt<bool>();
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            DeleteTemporaryFile(temporaryPath);
            return FailUnavailable<bool>();
        }
    }

    private void DeleteCompletedBatchReservation()
    {
        try
        {
            var path = Path.Combine(_storageRoot!, BatchReservationFileName);
            if (File.Exists(path) && !AnyExistingPathSegmentIsReparsePoint(path))
                File.Delete(path);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // A completed marker is harmless: the next reservation recognizes
            // its entire batch as present and retries cleanup under the lease.
        }
    }

    internal static OperationResult<DpoPairAtomicGroupEnvelope>
        CreateAtomicGroupEnvelope(DpoPair critic, DpoPair improver)
    {
        var pairs = new[] { critic, improver };
        if (pairs.Any(static pair => pair is null || !ValidatePair(pair).IsSuccess)
            || pairs.Any(static pair =>
                GetWriteSchemaVersion(pair) != CurrentSchemaVersion)
            || critic.TeacherProvenance is not { } criticProvenance
            || improver.TeacherProvenance is not { } improverProvenance
            || criticProvenance.InvocationCount != 1
            || !SameTeacherInvocation(criticProvenance, improverProvenance)
            || !StringComparer.Ordinal.Equals(critic.Domain, improver.Domain))
        {
            return InvalidAtomicGroup<DpoPairAtomicGroupEnvelope>();
        }

        var children = pairs.Select((pair, ordinal) =>
        {
            var role = ordinal == 0
                ? DpoPairAtomicGroupRole.CriticUnit
                : DpoPairAtomicGroupRole.ImproverUnit;
            return new DpoPairAtomicGroupChild(
                ordinal,
                role,
                ComputeIdentity(pair, CurrentSchemaVersion),
                pair.TeacherProvenance!.TrainingContentSha256,
                pair);
        }).ToArray();
        var commitment = ComputeAtomicGroupCommitment(
            criticProvenance.RequestId,
            criticProvenance.InvocationCount,
            children);
        return OperationResult<DpoPairAtomicGroupEnvelope>.Ok(
            new DpoPairAtomicGroupEnvelope(
                DpoPairAtomicGroupEnvelope.CurrentSchemaVersion,
                commitment,
                criticProvenance.RequestId,
                criticProvenance.InvocationCount,
                Array.AsReadOnly(children)));
    }

    public OperationResult<DpoPairAtomicGroupReceipt> AddAtomicGroup(
        DpoPairAtomicGroupEnvelope group)
    {
        var capture = CaptureAtomicGroup(group);
        if (!capture.IsSuccess || capture.Value is null)
            return capture.MapError<DpoPairAtomicGroupReceipt>();
        group = capture.Value;
        var validation = ValidateAtomicGroup(group);
        if (!validation.IsSuccess)
            return validation.MapError<DpoPairAtomicGroupReceipt>();
        if (_storageRoot is null || _mutationLockPath is null)
            return FailUnavailable<DpoPairAtomicGroupReceipt>();

        var timeoutBudget = Stopwatch.StartNew();
        var processLeaseResult = AcquireProcessMutationLease(timeoutBudget);
        if (!processLeaseResult.IsSuccess || processLeaseResult.Value is null)
            return processLeaseResult.MapError<DpoPairAtomicGroupReceipt>();
        using var processLease = processLeaseResult.Value;
        var lockRootReady = EnsureWritableRoot();
        if (!lockRootReady.IsSuccess)
            return lockRootReady.MapError<DpoPairAtomicGroupReceipt>();
        var leaseResult = AcquireMutationLease(timeoutBudget);
        if (!leaseResult.IsSuccess || leaseResult.Value is null)
            return leaseResult.MapError<DpoPairAtomicGroupReceipt>();
        using var lease = leaseResult.Value;
        _afterMutationLockAcquiredForTesting?.Invoke();
        var rootReady = EnsureWritableRoot();
        if (!rootReady.IsSuccess
            || !lease.MatchesRequestedPath(_mutationLockPath)
            || !lease.HasExpectedLength()
            || AnyExistingPathSegmentIsReparsePoint(_storageRoot)
            || AnyExistingPathSegmentIsReparsePoint(_mutationLockPath))
        {
            return rootReady.IsSuccess
                ? FailUnavailable<DpoPairAtomicGroupReceipt>()
                : rootReady.MapError<DpoPairAtomicGroupReceipt>();
        }

        var snapshot = LoadSnapshot(pairIdentityForCollision: null);
        if (!snapshot.IsSuccess || snapshot.Value is null)
            return snapshot.MapError<DpoPairAtomicGroupReceipt>();
        var transactions = LoadTransactions();
        if (!transactions.IsSuccess || transactions.Value is null)
            return transactions.MapError<DpoPairAtomicGroupReceipt>();
        var replay = LoadReplayLedger();
        if (!replay.IsSuccess || replay.Value is null)
            return replay.MapError<DpoPairAtomicGroupReceipt>();

        if (snapshot.Value.GroupsByIdentity.TryGetValue(
                group.GroupCommitmentSha256,
                out var existingGroup))
        {
            return AtomicGroupsEqual(existingGroup, group)
                ? OperationResult<DpoPairAtomicGroupReceipt>.Ok(
                    ToAtomicGroupReceipt(group))
                : FailCollision<DpoPairAtomicGroupReceipt>();
        }
        var pairIds = group.Children.Select(static child => child.PairId).ToArray();
        var retainedTransaction = transactions.Value
            .SelectMany(static transaction => transaction.Requests)
            .SingleOrDefault(request => StringComparer.Ordinal.Equals(
                request.RequestId,
                group.RequestId));
        var retainedReplay = replay.Value.Entries.SingleOrDefault(entry =>
            StringComparer.Ordinal.Equals(entry.RequestId, group.RequestId));
        if (retainedTransaction is not null
            || retainedReplay is not null)
        {
            var exact = retainedTransaction is not null
                ? StringComparer.Ordinal.Equals(
                      retainedTransaction.GroupId,
                      group.GroupCommitmentSha256)
                  && RequestPairIds(retainedTransaction).SequenceEqual(
                      pairIds,
                      StringComparer.Ordinal)
                : StringComparer.Ordinal.Equals(
                      retainedReplay!.GroupId,
                      group.GroupCommitmentSha256)
                  && ReplayPairIds(retainedReplay).SequenceEqual(
                      pairIds,
                      StringComparer.Ordinal);
            return exact
                ? OperationResult<DpoPairAtomicGroupReceipt>.Ok(
                    ToAtomicGroupReceipt(group))
                : OperationResult<DpoPairAtomicGroupReceipt>.Fail(
                    "A teacher request id has already been retained by durable DPO storage.",
                    LegalTrainingTeacherAdmissionAuthority.DuplicateRequestIdErrorCode);
        }
        if (pairIds.Any(snapshot.Value.RecordsByIdentity.ContainsKey)
            || transactions.Value.Any(transaction => transaction.Pairs.Any(pair =>
                pairIds.Contains(pair.PairId, StringComparer.Ordinal)))
            || replay.Value.Entries.Any(entry => ReplayPairIds(entry).Any(pairId =>
                pairIds.Contains(pairId, StringComparer.Ordinal))))
        {
            return FailCollision<DpoPairAtomicGroupReceipt>();
        }
        if (snapshot.Value.RecordsByIdentity.Values.Any(value =>
                StringComparer.Ordinal.Equals(
                    value.TeacherProvenance?.RequestId,
                    group.RequestId))
            || transactions.Value.Any(transaction => transaction.Requests.Any(request =>
                StringComparer.Ordinal.Equals(request.RequestId, group.RequestId)))
            || replay.Value.Entries.Any(entry => StringComparer.Ordinal.Equals(
                entry.RequestId,
                group.RequestId)))
        {
            return OperationResult<DpoPairAtomicGroupReceipt>.Fail(
                "A teacher request id has already been retained by durable DPO storage.",
                LegalTrainingTeacherAdmissionAuthority.DuplicateRequestIdErrorCode);
        }
        if (snapshot.Value.RecordsByIdentity.Count > _maximumRecordCount - 2)
            return FailCapacity<DpoPairAtomicGroupReceipt>();
        if (!HasReplayLedgerEntryCapacity(replay.Value.Entries.Length, 1))
            return FailReplayLedgerCapacity<DpoPairAtomicGroupReceipt>();

        return WriteAtomicGroup(group);
    }

    private static OperationResult<DpoPairAtomicGroupEnvelope> CaptureAtomicGroup(
        DpoPairAtomicGroupEnvelope group)
    {
        if (group is null || group.Children is null)
            return InvalidAtomicGroup<DpoPairAtomicGroupEnvelope>();
        try
        {
            var children = group.Children.Take(3).ToArray();
            return children.Length == 2
                ? OperationResult<DpoPairAtomicGroupEnvelope>.Ok(group with
                {
                    Children = Array.AsReadOnly(children),
                })
                : InvalidAtomicGroup<DpoPairAtomicGroupEnvelope>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidAtomicGroup<DpoPairAtomicGroupEnvelope>();
        }
    }

    public OperationResult<IReadOnlyList<DpoPair>> GetAll()
        => ExecuteTransactionMutation(() =>
        {
            var snapshot = LoadSnapshot(pairIdentityForCollision: null);
            if (!snapshot.IsSuccess)
                return snapshot.MapError<IReadOnlyList<DpoPair>>();
            var transactions = LoadTransactions();
            if (!transactions.IsSuccess || transactions.Value is null)
                return transactions.MapError<IReadOnlyList<DpoPair>>();
            var replayLedger = LoadReplayLedger();
            if (!replayLedger.IsSuccess || replayLedger.Value is null)
                return replayLedger.MapError<IReadOnlyList<DpoPair>>();
            var retainedPairIds = transactions.Value
                .SelectMany(static transaction => transaction.Pairs)
                .Select(static pair => pair.PairId)
                .Concat(replayLedger.Value.Entries.SelectMany(ReplayPairIds))
                .ToHashSet(StringComparer.Ordinal);

            return OperationResult<IReadOnlyList<DpoPair>>.Ok(
                Array.AsReadOnly(snapshot.Value!.RecordsByIdentity.Values
                    .Where(value => !retainedPairIds.Contains(value.PairId))
                    .OrderBy(static value => value.GroupCommitmentSha256.Length == 0
                            ? value.PairId
                            : value.GroupCommitmentSha256,
                        StringComparer.Ordinal)
                    .ThenBy(static value => value.GroupOrdinal)
                    .Select(ToPair)
                    .ToArray()));
        });

    internal OperationResult<IReadOnlyList<DpoPair>>
        ReadManifestIntegrityPairs(IReadOnlyList<string> expectedPairIds)
        => ExecuteTransactionMutation(() =>
        {
            if (expectedPairIds is null
                || expectedPairIds.Count == 0
                || expectedPairIds.Count > _maximumRecordCount
                || expectedPairIds.Distinct(StringComparer.Ordinal).Count()
                != expectedPairIds.Count
                || expectedPairIds.Any(static pairId =>
                    !IsCanonicalSha256Hex(pairId)))
            {
                return FailCorrupt<IReadOnlyList<DpoPair>>();
            }

            var snapshot = LoadSnapshot(pairIdentityForCollision: null);
            if (!snapshot.IsSuccess || snapshot.Value is null)
                return snapshot.MapError<IReadOnlyList<DpoPair>>();

            // WHY: GetAll() is the queue-availability view and must hide pairs
            // already held by active transactions or replay recovery. A sealed
            // dataset manifest is immutable integrity evidence instead, so it
            // must read the exact canonical rows the manifest names, including
            // currently held rows, and reject any stored-row delta.
            if (snapshot.Value.RecordsByIdentity.Count != expectedPairIds.Count)
                return FailCorrupt<IReadOnlyList<DpoPair>>();

            var pairs = new DpoPair[expectedPairIds.Count];
            for (var index = 0; index < expectedPairIds.Count; index++)
            {
                var pairId = expectedPairIds[index];
                if (!snapshot.Value.RecordsByIdentity.TryGetValue(
                        pairId,
                        out var record))
                {
                    return FailCorrupt<IReadOnlyList<DpoPair>>();
                }

                var pair = ToPair(record);
                var computedPairId = ComputeIdentity(pair, CurrentSchemaVersion);
                if (!StringComparer.Ordinal.Equals(computedPairId, pairId))
                    return FailCorrupt<IReadOnlyList<DpoPair>>();

                pairs[index] = pair;
            }

            return OperationResult<IReadOnlyList<DpoPair>>.Ok(
                Array.AsReadOnly(pairs));
        });

    private OperationResult<ProcessMutationLease> AcquireProcessMutationLease(
        Stopwatch timeoutBudget)
    {
        if (!IsSupportedPlatform())
            return FailUnavailable<ProcessMutationLease>();

        ProcessGateEntry entry;
        lock (ProcessGateRegistrySync)
        {
            if (!ProcessGateRegistry.TryGetValue(_storageRoot!, out entry!))
            {
                entry = new ProcessGateEntry();
                ProcessGateRegistry.Add(_storageRoot!, entry);
            }
            checked
            {
                entry.ReferenceCount++;
            }
        }

        try
        {
            var remaining = _mutationLockTimeout - timeoutBudget.Elapsed;
            if (remaining <= TimeSpan.Zero || !entry.Semaphore.Wait(remaining))
            {
                ReleaseProcessGateReference(_storageRoot!, entry);
                return FailBusy<ProcessMutationLease>();
            }

            return OperationResult<ProcessMutationLease>.Ok(
                new ProcessMutationLease(_storageRoot!, entry));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            ReleaseProcessGateReference(_storageRoot!, entry);
            return FailUnavailable<ProcessMutationLease>();
        }
    }

    private OperationResult<MutationLease> AcquireMutationLease(
        Stopwatch timeoutBudget)
    {
        if (!IsSupportedPlatform())
            return FailUnavailable<MutationLease>();

        while (true)
        {
            if (_mutationLockTimeout - timeoutBudget.Elapsed <= TimeSpan.Zero)
                return FailBusy<MutationLease>();

            FileStream? stream = null;
            MutationLease? lease = null;
            try
            {
                if (AnyExistingPathSegmentIsReparsePoint(_storageRoot!)
                    || AnyExistingPathSegmentIsReparsePoint(_mutationLockPath!)
                    || (File.Exists(_mutationLockPath!)
                        && !HasSafeUnixFileMode(_mutationLockPath!)))
                {
                    return FailUnavailable<MutationLease>();
                }

                stream = OpenPrivateFile(
                    _mutationLockPath!,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    FileOptions.WriteThrough);
                if (!OpenedHandleMatchesRequestedPath(
                        stream.SafeFileHandle,
                        _mutationLockPath!)
                    || AnyExistingPathSegmentIsReparsePoint(_storageRoot!)
                    || AnyExistingPathSegmentIsReparsePoint(_mutationLockPath!)
                    || !HasSafeUnixFileMode(_mutationLockPath!))
                {
                    stream.Dispose();
                    return FailUnavailable<MutationLease>();
                }

                if (stream.Length == 0)
                {
                    stream.SetLength(MutationLockLength);
                    stream.Flush(flushToDisk: true);
                }
                if (stream.Length != MutationLockLength)
                {
                    stream.Dispose();
                    return FailUnavailable<MutationLease>();
                }

                var injectedFailure =
                    _mutationRegionLockFailureForTesting?.Invoke();
                if (injectedFailure is not null)
                    throw injectedFailure;
                if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
                {
                    stream.Lock(
                        MutationLockOffset,
                        MutationLockLength);
                }
                else
                {
                    stream.Dispose();
                    return FailUnavailable<MutationLease>();
                }
                lease = new MutationLease(stream);
                if (_corruptMutationLockLengthAfterAcquisitionForTesting)
                    stream.SetLength(MutationLockLength + 1);
                if (!lease.MatchesRequestedPath(_mutationLockPath!)
                    || !lease.HasExpectedLength()
                    || AnyExistingPathSegmentIsReparsePoint(_storageRoot!)
                    || AnyExistingPathSegmentIsReparsePoint(_mutationLockPath!)
                    || !HasSafeUnixFileMode(_mutationLockPath!))
                {
                    lease.Dispose();
                    return FailUnavailable<MutationLease>();
                }

                return OperationResult<MutationLease>.Ok(lease);
            }
            catch (IOException exception)
                when (IsLockContention(exception, OperatingSystem.IsWindows()))
            {
                lease?.Dispose();
                stream?.Dispose();
                var remaining = _mutationLockTimeout - timeoutBudget.Elapsed;
                if (remaining <= TimeSpan.Zero)
                    return FailBusy<MutationLease>();

                Thread.Sleep(
                    remaining < TimeSpan.FromMilliseconds(25)
                        ? remaining
                        : TimeSpan.FromMilliseconds(25));
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                lease?.Dispose();
                stream?.Dispose();
                return FailUnavailable<MutationLease>();
            }
        }
    }

    internal static bool IsLockContention(
        IOException exception,
        bool isWindows)
    {
        var nativeCode = exception.HResult & 0xFFFF;
        return isWindows
            ? nativeCode is 32 or 33
            : nativeCode is 11 or 13;
    }

    private OperationResult<StoreSnapshot> LoadSnapshot(string? pairIdentityForCollision)
    {
        if (!IsSupportedPlatform())
            return FailUnavailable<StoreSnapshot>();
        if (_storageRoot is null)
            return FailUnavailable<StoreSnapshot>();

        try
        {
            if (AnyExistingPathSegmentIsReparsePoint(_storageRoot))
                return FailUnavailable<StoreSnapshot>();

            if (!Directory.Exists(_storageRoot))
            {
                return File.Exists(_storageRoot)
                    ? FailUnavailable<StoreSnapshot>()
                    : OperationResult<StoreSnapshot>.Ok(StoreSnapshot.Empty);
            }

            if (AnyExistingPathSegmentIsReparsePoint(_storageRoot))
                return FailUnavailable<StoreSnapshot>();
            if (!HasSafeUnixDirectoryMode(_storageRoot)
                || (_mutationLockPath is not null
                    && File.Exists(_mutationLockPath)
                    && (AnyExistingPathSegmentIsReparsePoint(_mutationLockPath)
                        || !HasSafeUnixFileMode(_mutationLockPath))))
            {
                return FailUnavailable<StoreSnapshot>();
            }

            var paths = Directory
                .EnumerateFiles(_storageRoot, $"*{CanonicalSuffix}", SearchOption.TopDirectoryOnly)
                .Take(_maximumRecordCount + 1)
                .ToArray();
            var groupPaths = Directory
                .EnumerateFiles(_storageRoot, $"*{AtomicGroupSuffix}", SearchOption.TopDirectoryOnly)
                .Take((_maximumRecordCount / 2) + 1)
                .ToArray();
            if (AnyExistingPathSegmentIsReparsePoint(_storageRoot))
                return FailUnavailable<StoreSnapshot>();
            if (paths.Length > _maximumRecordCount
                || groupPaths.Length > _maximumRecordCount / 2
                || paths.Length + (2 * groupPaths.Length) > _maximumRecordCount)
                return FailCorrupt<StoreSnapshot>();
            Array.Sort(paths, StringComparer.Ordinal);
            Array.Sort(groupPaths, StringComparer.Ordinal);
            var records = new Dictionary<string, StoredDpoPairRecord>(StringComparer.Ordinal);
            var groups = new Dictionary<string, StoredAtomicGroupRecord>(StringComparer.Ordinal);
            var pairs = new List<DpoPair>(paths.Length + (2 * groupPaths.Length));

            foreach (var path in paths)
            {
                var record = ReadCanonicalRecord(path, pairIdentityForCollision);
                if (!record.IsSuccess)
                    return record.MapError<StoreSnapshot>();
                if (!records.TryAdd(record.Value!.PairId, record.Value))
                    return FailCorrupt<StoreSnapshot>();
                pairs.Add(ToPair(record.Value));
            }
            foreach (var path in groupPaths)
            {
                var group = ReadAtomicGroup(path);
                if (!group.IsSuccess || group.Value is null)
                    return group.MapError<StoreSnapshot>();
                if (!groups.TryAdd(group.Value.GroupCommitmentSha256, group.Value))
                    return FailCorrupt<StoreSnapshot>();
                foreach (var child in group.Value.Children)
                {
                    var grouped = child.Pair with
                    {
                        GroupCommitmentSha256 = group.Value.GroupCommitmentSha256,
                        GroupOrdinal = child.Ordinal,
                        GroupRole = child.Role,
                    };
                    if (!records.TryAdd(grouped.PairId, grouped))
                        return FailCorrupt<StoreSnapshot>();
                    pairs.Add(ToPair(grouped));
                }
            }
            if (AnyExistingPathSegmentIsReparsePoint(_storageRoot))
                return FailUnavailable<StoreSnapshot>();

            return OperationResult<StoreSnapshot>.Ok(
                new StoreSnapshot(records, groups, pairs.AsReadOnly()));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FailUnavailable<StoreSnapshot>();
        }
    }

    private OperationResult<bool> EnsureWritableRoot()
    {
        try
        {
            if (AnyExistingPathSegmentIsReparsePoint(_storageRoot!))
                return FailUnavailable<bool>();
            if (Directory.Exists(_storageRoot!))
            {
                if (!HasSafeUnixDirectoryMode(_storageRoot!))
                    return FailUnavailable<bool>();
            }
            else if (OperatingSystem.IsWindows())
            {
                Directory.CreateDirectory(_storageRoot!);
            }
            else
            {
                Directory.CreateDirectory(_storageRoot!, OwnerDirectoryMode);
            }
            if (AnyExistingPathSegmentIsReparsePoint(_storageRoot!))
                return FailUnavailable<bool>();
            if (!HasSafeUnixDirectoryMode(_storageRoot!))
                return FailUnavailable<bool>();
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FailUnavailable<bool>();
        }
    }

    private OperationResult<string> WriteCanonicalRecord(
        string identity,
        DpoPair pair,
        int schemaVersion)
    {
        var canonicalPath = Path.Combine(_storageRoot!, identity + CanonicalSuffix);
        var temporaryPath = Path.Combine(
            _storageRoot!,
            $".{identity}.{Guid.NewGuid():N}.tmp");

        try
        {
            if (AnyExistingPathSegmentIsReparsePoint(_storageRoot!)
                || AnyExistingPathSegmentIsReparsePoint(canonicalPath))
            {
                return FailUnavailable<string>();
            }

            using (var stream = OpenPrivateFile(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                FileOptions.WriteThrough,
                bufferSize: 4096))
            {
                if (!OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, temporaryPath)
                    || !HasSafeUnixFileMode(temporaryPath))
                    throw new IOException("DPO pair temporary path integrity check failed.");

                using var boundedStream = new MaximumLengthWriteStream(
                    stream,
                    MaximumRecordBytes);
                using var writer = new Utf8JsonWriter(boundedStream);
                WriteCanonicalPairJson(writer, identity, pair, schemaVersion);
                writer.Flush();
                boundedStream.Flush();
                stream.Flush(flushToDisk: true);
                if (!OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, temporaryPath)
                    || !HasSafeUnixFileMode(temporaryPath))
                    throw new IOException("DPO pair temporary path integrity check failed.");
            }
            if (AnyExistingPathSegmentIsReparsePoint(_storageRoot!)
                || AnyExistingPathSegmentIsReparsePoint(temporaryPath)
                || AnyExistingPathSegmentIsReparsePoint(canonicalPath))
            {
                DeleteTemporaryFile(temporaryPath);
                return FailUnavailable<string>();
            }

            try
            {
                File.Move(temporaryPath, canonicalPath);
            }
            catch (IOException) when (File.Exists(canonicalPath))
            {
                DeleteTemporaryFile(temporaryPath);
                return ResolveConcurrentPublication(canonicalPath, identity, pair);
            }
            if (AnyExistingPathSegmentIsReparsePoint(_storageRoot!)
                || AnyExistingPathSegmentIsReparsePoint(canonicalPath))
            {
                return FailUnavailable<string>();
            }

            var published = ReadCanonicalRecord(canonicalPath, identity);
            if (!published.IsSuccess)
                return published.MapError<string>();
            if (!SameSemanticPair(published.Value!, pair))
                return FailCollision<string>();

            return OperationResult<string>.Ok(identity);
        }
        catch (DpoPairRecordTooLargeException)
        {
            DeleteTemporaryFile(temporaryPath);
            return OperationResult<string>.Fail(
                "DPO pair fields exceed the storage limit.",
                "INVALID_DPO_PAIR");
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            DeleteTemporaryFile(temporaryPath);
            return FailUnavailable<string>();
        }
    }

    private static void WriteCanonicalPairJson(
        Utf8JsonWriter writer,
        string identity,
        DpoPair pair,
        int schemaVersion)
    {
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", schemaVersion);
        if (schemaVersion == CurrentSchemaVersion)
            writer.WriteString("recordDomain", CurrentRecordDomain);
        writer.WriteString("pairId", identity);
        writer.WriteString("prompt", pair.Prompt);
        writer.WriteString("chosen", pair.Chosen);
        writer.WriteString("rejected", pair.Rejected);
        writer.WriteString("domain", pair.Domain);
        WriteCanonicalCapturedAt(writer, pair.CapturedAt);
        if (schemaVersion != LegacySchemaVersion)
            WriteTrainingMetadata(writer, pair, schemaVersion);
        if (schemaVersion == CurrentSchemaVersion)
            WriteSideAuthorship(writer, pair);
        writer.WriteEndObject();
    }

    private OperationResult<DpoPairAtomicGroupReceipt> WriteAtomicGroup(
        DpoPairAtomicGroupEnvelope group)
    {
        var path = Path.Combine(
            _storageRoot!,
            group.GroupCommitmentSha256 + AtomicGroupSuffix);
        var temporary = Path.Combine(
            _storageRoot!,
            $".{group.GroupCommitmentSha256}.{Guid.NewGuid():N}.group.tmp");
        try
        {
            using (var stream = OpenPrivateFile(
                       temporary,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       FileOptions.WriteThrough))
            using (var bounded = new MaximumLengthWriteStream(
                       stream,
                       MaximumRecordBytes))
            using (var writer = new Utf8JsonWriter(bounded))
            {
                WriteAtomicGroupJson(writer, group);
                writer.Flush();
                bounded.Flush();
                stream.Flush(flushToDisk: true);
                if (!OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, temporary)
                    || !HasSafeUnixFileMode(temporary))
                {
                    throw new IOException("DPO pair-group staging path changed.");
                }
            }
            File.Move(temporary, path);
            var readback = ReadAtomicGroup(path);
            return readback.IsSuccess
                   && readback.Value is not null
                   && AtomicGroupsEqual(readback.Value, group)
                ? OperationResult<DpoPairAtomicGroupReceipt>.Ok(
                    ToAtomicGroupReceipt(group))
                : FailCorrupt<DpoPairAtomicGroupReceipt>();
        }
        catch (DpoPairRecordTooLargeException)
        {
            DeleteTemporaryFile(temporary);
            return InvalidAtomicGroup<DpoPairAtomicGroupReceipt>();
        }
        catch (IOException) when (File.Exists(path))
        {
            DeleteTemporaryFile(temporary);
            var existing = ReadAtomicGroup(path);
            return existing.IsSuccess
                   && existing.Value is not null
                   && AtomicGroupsEqual(existing.Value, group)
                ? OperationResult<DpoPairAtomicGroupReceipt>.Ok(
                    ToAtomicGroupReceipt(group))
                : FailCollision<DpoPairAtomicGroupReceipt>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            DeleteTemporaryFile(temporary);
            return FailUnavailable<DpoPairAtomicGroupReceipt>();
        }
    }

    private static void WriteAtomicGroupJson(
        Utf8JsonWriter writer,
        DpoPairAtomicGroupEnvelope group)
    {
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", group.SchemaVersion);
        writer.WriteString("domain", AtomicGroupDomain);
        writer.WriteString(
            "groupCommitmentSha256",
            group.GroupCommitmentSha256);
        writer.WriteString("requestId", group.RequestId);
        writer.WriteNumber("invocationCount", group.InvocationCount);
        writer.WriteStartArray("children");
        foreach (var child in group.Children)
        {
            writer.WriteStartObject();
            writer.WriteNumber("ordinal", child.Ordinal);
            writer.WriteString("role", child.Role.ToString());
            writer.WriteString("contentSha256", child.ContentSha256);
            writer.WritePropertyName("pair");
            WriteCanonicalPairJson(
                writer,
                child.PairId,
                child.Pair,
                CurrentSchemaVersion);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static OperationResult<StoredAtomicGroupRecord> ReadAtomicGroup(
        string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            if (AnyExistingPathSegmentIsReparsePoint(fullPath)
                || !HasSafeUnixFileMode(fullPath))
            {
                return FailCorrupt<StoredAtomicGroupRecord>();
            }
            using var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.SequentialScan);
            if (!OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, fullPath))
                return FailCorrupt<StoredAtomicGroupRecord>();
            var bytes = ReadBoundedStableBytes(stream);
            if (!bytes.IsSuccess || bytes.Value is null)
                return bytes.MapError<StoredAtomicGroupRecord>();
            using var document = JsonDocument.Parse(bytes.Value, new JsonDocumentOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 12,
            });
            var root = document.RootElement;
            if (!HasNoDuplicateProperties(root))
                return FailCorrupt<StoredAtomicGroupRecord>();
            var names = root.EnumerateObject().Select(static value => value.Name)
                .ToHashSet(StringComparer.Ordinal);
            if (root.ValueKind != JsonValueKind.Object
                || names.Count != 6
                || !names.SetEquals([
                    "schemaVersion", "domain", "groupCommitmentSha256",
                    "requestId", "invocationCount", "children"])
                || !root.TryGetProperty("schemaVersion", out var schema)
                || !schema.TryGetInt32(out var schemaVersion)
                || schemaVersion != DpoPairAtomicGroupEnvelope.CurrentSchemaVersion
                || !TryGetString(root, "domain", out var domain)
                || !StringComparer.Ordinal.Equals(domain, AtomicGroupDomain)
                || !TryGetString(root, "groupCommitmentSha256", out var groupId)
                || !TryGetString(root, "requestId", out var requestId)
                || !root.TryGetProperty("invocationCount", out var invocation)
                || !invocation.TryGetInt32(out var invocationCount)
                || !root.TryGetProperty("children", out var childrenElement)
                || childrenElement.ValueKind != JsonValueKind.Array)
            {
                return FailCorrupt<StoredAtomicGroupRecord>();
            }
            var elements = childrenElement.EnumerateArray().ToArray();
            if (elements.Length != 2)
                return FailCorrupt<StoredAtomicGroupRecord>();
            var children = new StoredAtomicGroupChild[2];
            for (var index = 0; index < elements.Length; index++)
            {
                var element = elements[index];
                var childNames = element.EnumerateObject()
                    .Select(static value => value.Name)
                    .ToHashSet(StringComparer.Ordinal);
                if (element.ValueKind != JsonValueKind.Object
                    || childNames.Count != 4
                    || !childNames.SetEquals([
                        "ordinal", "role", "contentSha256", "pair"])
                    || !element.TryGetProperty("ordinal", out var ordinalElement)
                    || !ordinalElement.TryGetInt32(out var ordinal)
                    || !TryGetString(element, "role", out var roleText)
                    || !Enum.TryParse<DpoPairAtomicGroupRole>(
                        roleText,
                        ignoreCase: false,
                        out var role)
                    || !TryGetString(
                        element,
                        "contentSha256",
                        out var contentSha256)
                    || !element.TryGetProperty("pair", out var pairElement)
                    || !TryReadExactRecord(pairElement, out var pair)
                    || !StringComparer.Ordinal.Equals(
                        pair.TeacherProvenance?.TrainingContentSha256,
                        contentSha256))
                {
                    return FailCorrupt<StoredAtomicGroupRecord>();
                }
                children[index] = new StoredAtomicGroupChild(
                    ordinal,
                    role,
                    contentSha256,
                    pair);
            }
            var envelope = new DpoPairAtomicGroupEnvelope(
                schemaVersion,
                groupId,
                requestId,
                invocationCount,
                Array.AsReadOnly(children.Select(static child =>
                    new DpoPairAtomicGroupChild(
                        child.Ordinal,
                        child.Role,
                        child.Pair.PairId,
                        child.ContentSha256,
                        ToPair(child.Pair))).ToArray()));
            var valid = ValidateAtomicGroup(envelope);
            var expectedFile = groupId + AtomicGroupSuffix;
            return valid.IsSuccess
                   && StringComparer.Ordinal.Equals(Path.GetFileName(fullPath), expectedFile)
                ? OperationResult<StoredAtomicGroupRecord>.Ok(
                    new StoredAtomicGroupRecord(
                        schemaVersion,
                        groupId,
                        requestId,
                        invocationCount,
                        children))
                : FailCorrupt<StoredAtomicGroupRecord>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FailCorrupt<StoredAtomicGroupRecord>();
        }
    }

    private static void WriteCanonicalCapturedAt(
        Utf8JsonWriter writer,
        DateTimeOffset capturedAt)
    {
        // This value is produced solely by the invariant round-trip formatter,
        // whose fixed alphabet is safe as a validated JSON string literal. Keep
        // its historical '+' byte without relaxing escaping for user content.
        var value = capturedAt
            .ToUniversalTime()
            .ToString("O", CultureInfo.InvariantCulture);
        writer.WritePropertyName("capturedAt");
        writer.WriteRawValue($"\"{value}\"");
    }

    private OperationResult<string> ResolveConcurrentPublication(
        string canonicalPath,
        string identity,
        DpoPair pair)
    {
        var existing = ReadCanonicalRecord(canonicalPath, identity);
        if (!existing.IsSuccess)
            return existing.MapError<string>();
        return SameSemanticPair(existing.Value!, pair)
            ? OperationResult<string>.Ok(identity)
            : FailCollision<string>();
    }

    private static OperationResult<StoredDpoPairRecord> ReadCanonicalRecord(
        string path,
        string? pairIdentityForCollision)
    {
        try
        {
            var requestedPath = Path.GetFullPath(path);
            if (AnyExistingPathSegmentIsReparsePoint(requestedPath)
                || !HasSafeUnixFileMode(requestedPath))
                return FailCorrupt<StoredDpoPairRecord>();

            using var stream = new FileStream(
                requestedPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.SequentialScan);
            if (!OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, requestedPath))
                return FailCorrupt<StoredDpoPairRecord>();

            var bytes = ReadBoundedStableBytes(stream);
            if (!bytes.IsSuccess)
                return bytes.MapError<StoredDpoPairRecord>();
            if (!OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, requestedPath)
                || AnyExistingPathSegmentIsReparsePoint(requestedPath)
                || !HasSafeUnixFileMode(requestedPath))
            {
                return FailCorrupt<StoredDpoPairRecord>();
            }

            using var document = JsonDocument.Parse(
                bytes.Value!,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = 8,
                });
            if (!TryReadExactRecord(document.RootElement, out var record))
                return FailCorrupt<StoredDpoPairRecord>();

            var fileName = Path.GetFileName(requestedPath);
            var expectedFileName = record.PairId + CanonicalSuffix;
            if (!StringComparer.Ordinal.Equals(fileName, expectedFileName))
                return FailCorrupt<StoredDpoPairRecord>();

            var computedIdentity = ComputeIdentity(
                ToPair(record),
                record.SchemaVersion);
            if (!StringComparer.Ordinal.Equals(computedIdentity, record.PairId))
            {
                return StringComparer.Ordinal.Equals(
                    pairIdentityForCollision,
                    record.PairId)
                    ? FailCollision<StoredDpoPairRecord>()
                    : FailCorrupt<StoredDpoPairRecord>();
            }

            return OperationResult<StoredDpoPairRecord>.Ok(record);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FailCorrupt<StoredDpoPairRecord>();
        }
    }

    private static OperationResult<byte[]> ReadBoundedStableBytes(FileStream stream)
    {
        var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(
            stream,
            MaximumRecordBytes);
        return bytes is null
            ? FailCorrupt<byte[]>()
            : OperationResult<byte[]>.Ok(bytes);
    }

    private static FileStream OpenPrivateFile(
        string path,
        FileMode mode,
        FileAccess access,
        FileShare share,
        FileOptions options,
        int bufferSize = 1)
        => SecureLocalFileBoundary.OpenOwnerOnlyFile(
            path, mode, access, share, options, bufferSize);

    private static bool HasSafeUnixDirectoryMode(string path)
        => SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(path);

    private static bool HasSafeUnixFileMode(string path)
        => SecureLocalFileBoundary.HasOwnerOnlyFileMode(path);

    private bool IsSupportedPlatform()
        => !_unsupportedPlatformForTesting
            && SecureLocalFileBoundary.IsSupportedPlatform();

    internal static int GetProcessGateReferenceCountForTesting(string storageRoot)
    {
        var normalizedRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(storageRoot));
        lock (ProcessGateRegistrySync)
        {
            return ProcessGateRegistry.TryGetValue(normalizedRoot, out var entry)
                ? entry.ReferenceCount
                : 0;
        }
    }

    private static void ReleaseProcessGateReference(
        string storageRoot,
        ProcessGateEntry entry)
    {
        SemaphoreSlim? semaphoreToDispose = null;
        lock (ProcessGateRegistrySync)
        {
            entry.ReferenceCount--;
            if (entry.ReferenceCount != 0)
                return;
            if (!ProcessGateRegistry.TryGetValue(storageRoot, out var registered)
                || !ReferenceEquals(registered, entry))
            {
                throw new InvalidOperationException(
                    "DPO pair process-gate registry identity was lost.");
            }

            ProcessGateRegistry.Remove(storageRoot);
            semaphoreToDispose = entry.Semaphore;
        }
        semaphoreToDispose.Dispose();
    }

    private static void ReleaseOwnedProcessGate(
        string storageRoot,
        ProcessGateEntry entry)
    {
        try
        {
            entry.Semaphore.Release();
        }
        finally
        {
            ReleaseProcessGateReference(storageRoot, entry);
        }
    }

    private static bool OpenedHandleMatchesRequestedPath(
        SafeFileHandle handle,
        string requestedPath)
        => SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
            handle,
            requestedPath);

    private static bool PathsEqual(string first, string second)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(first)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(second)),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    private static bool AnyExistingPathSegmentIsReparsePoint(string fullPath)
        => SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(fullPath);

    private static bool TryReadExactRecord(
        JsonElement root,
        out StoredDpoPairRecord record)
    {
        record = default!;
        if (root.ValueKind != JsonValueKind.Object)
            return false;

        if (!root.TryGetProperty("schemaVersion", out var schema)
            || schema.ValueKind != JsonValueKind.Number
            || !schema.TryGetInt32(out var schemaVersion)
            || schemaVersion is not (
                LegacySchemaVersion
                or ProvenanceSchemaVersion
                or ArtifactProvenanceSchemaVersion
                or ContentBoundSchemaVersion
                or CurrentSchemaVersion))
        {
            return false;
        }

        var properties = root.EnumerateObject().ToArray();
        var expectedPropertyCount = schemaVersion switch
        {
            LegacySchemaVersion => 7,
            CurrentSchemaVersion => 10,
            _ => 8,
        };
        if (properties.Length != expectedPropertyCount)
            return false;
        var names = properties
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);
        var expectedNames = new HashSet<string>(
            [
                "schemaVersion",
                "pairId",
                "prompt",
                "chosen",
                "rejected",
                "domain",
                "capturedAt",
            ],
            StringComparer.Ordinal);
        if (schemaVersion == CurrentSchemaVersion)
            expectedNames.Add("recordDomain");
        if (schemaVersion != LegacySchemaVersion)
            expectedNames.Add("teacherProvenance");
        if (schemaVersion == CurrentSchemaVersion)
            expectedNames.Add("sideAuthorship");
        if (names.Count != expectedNames.Count || !names.SetEquals(expectedNames))
        {
            return false;
        }

        if (schemaVersion == CurrentSchemaVersion
            && (!TryGetString(root, "recordDomain", out var recordDomain)
                || !StringComparer.Ordinal.Equals(
                    recordDomain,
                    CurrentRecordDomain)))
        {
            return false;
        }

        if (!TryGetString(root, "pairId", out var pairId)
            || !IsCanonicalIdentity(pairId)
            || !TryGetString(root, "prompt", out var prompt)
            || !TryGetString(root, "chosen", out var chosen)
            || !TryGetString(root, "rejected", out var rejected)
            || !TryGetString(root, "domain", out var domain)
            || !TryGetString(root, "capturedAt", out var capturedAtText)
            || !DateTimeOffset.TryParseExact(
                capturedAtText,
                "O",
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var capturedAt))
        {
            return false;
        }

        TrainingTeacherProvenance? teacherProvenance = null;
        TrainingDataGovernanceDecision? governanceDecision = null;
        DpoPairSideAuthorship? chosenAuthorship = null;
        DpoPairSideAuthorship? rejectedAuthorship = null;
        if (schemaVersion != LegacySchemaVersion
            && (!root.TryGetProperty("teacherProvenance", out var metadataElement)
                || !TryReadTrainingMetadata(
                    metadataElement,
                    schemaVersion,
                    out teacherProvenance,
                    out governanceDecision)))
        {
            return false;
        }
        if (schemaVersion == CurrentSchemaVersion
            && (!root.TryGetProperty("sideAuthorship", out var authorshipElement)
                || !TryReadSideAuthorships(
                    authorshipElement,
                    out chosenAuthorship,
                    out rejectedAuthorship)))
        {
            return false;
        }

        var pair = new DpoPair(
            prompt,
            chosen,
            rejected,
            domain,
            capturedAt)
        {
            TeacherProvenance = teacherProvenance,
            GovernanceDecision = governanceDecision,
            ChosenAuthorship = chosenAuthorship,
            RejectedAuthorship = rejectedAuthorship,
        };
        if (!ValidatePair(pair).IsSuccess)
            return false;
        if (schemaVersion >= ContentBoundSchemaVersion)
        {
            var projected = DpoPairTrainingContentAuthority.Project([pair]);
            if (!projected.IsSuccess || projected.Value is null ||
                teacherProvenance is null ||
                !FixedTimeDigestEquals(
                    projected.Value.Rows[0].Sha256,
                    teacherProvenance.TrainingContentSha256))
            {
                return false;
            }
        }

        record = new StoredDpoPairRecord(
            schemaVersion,
            pairId,
            prompt,
            chosen,
            rejected,
            domain,
            capturedAt,
            teacherProvenance,
            governanceDecision,
            chosenAuthorship,
            rejectedAuthorship);
        return true;
    }

    private static bool FixedTimeDigestEquals(string left, string right)
    {
        if (!LegalTrainingTeacherAdmissionAuthority
                .IsCanonicalSha256Digest(left) ||
            !LegalTrainingTeacherAdmissionAuthority
                .IsCanonicalSha256Digest(right))
        {
            return false;
        }
        try
        {
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(left),
                Convert.FromHexString(right));
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static void WriteTrainingMetadata(
        Utf8JsonWriter writer,
        DpoPair pair,
        int schemaVersion)
    {
        writer.WritePropertyName("teacherProvenance");
        if (pair.TeacherProvenance is null && pair.GovernanceDecision is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        writer.WritePropertyName("provenance");
        WriteTeacherProvenanceValue(writer, pair.TeacherProvenance, schemaVersion);
        writer.WritePropertyName("governanceDecision");
        WriteGovernanceDecisionValue(writer, pair.GovernanceDecision);
        writer.WriteEndObject();
    }

    private static void WriteTeacherProvenanceValue(
        Utf8JsonWriter writer,
        TrainingTeacherProvenance? provenance,
        int schemaVersion)
    {
        if (provenance is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        writer.WriteString("sourceKind", provenance.SourceKind.ToString());
        WriteNullableEnum(writer, "family", provenance.Family);
        WriteNullableEnum(writer, "accessMode", provenance.AccessMode);
        WriteNullableEnum(writer, "hostingProvider", provenance.HostingProvider);
        writer.WriteString("modelId", provenance.ModelId);
        writer.WriteString("modelVersion", provenance.ModelVersion);
        WriteNullableEnum(writer, "modelLicense", provenance.ModelLicense);
        writer.WriteBoolean("providerTermsChecked", provenance.ProviderTermsChecked);
        writer.WriteBoolean(
            "providerTermsAllowTraining",
            provenance.ProviderTermsAllowTraining);
        writer.WriteBoolean("closedProprietaryModel", provenance.ClosedProprietaryModel);
        writer.WriteBoolean(
            "teacherOutputUsedForTraining",
            provenance.TeacherOutputUsedForTraining);
        writer.WriteString("requestId", provenance.RequestId);
        writer.WriteNumber("invocationCount", provenance.InvocationCount);
        if (schemaVersion >= ArtifactProvenanceSchemaVersion)
        {
            writer.WriteString(
                "modelArtifactDigestKind",
                provenance.ModelArtifactDigestKind);
            writer.WriteString(
                "modelArtifactDigest",
                provenance.ModelArtifactDigest);
        }
        if (schemaVersion >= ContentBoundSchemaVersion)
        {
            writer.WriteString(
                "trainingContentSha256",
                provenance.TrainingContentSha256);
        }
        writer.WriteEndObject();
    }

    private static void WriteGovernanceDecisionValue(
        Utf8JsonWriter writer,
        TrainingDataGovernanceDecision? governance)
    {
        if (governance is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        writer.WriteBoolean("licenseAccepted", governance.LicenseAccepted);
        writer.WriteBoolean("isQuarantined", governance.IsQuarantined);
        writer.WriteStartArray("evidenceRefs");
        foreach (var evidenceRef in governance.EvidenceRefs)
            writer.WriteStringValue(evidenceRef);
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteSideAuthorship(Utf8JsonWriter writer, DpoPair pair)
    {
        writer.WriteStartObject("sideAuthorship");
        writer.WritePropertyName("chosen");
        WriteSideAuthorshipValue(writer, pair.ChosenAuthorship!);
        writer.WritePropertyName("rejected");
        WriteSideAuthorshipValue(writer, pair.RejectedAuthorship!);
        writer.WriteEndObject();
    }

    private static void WriteSideAuthorshipValue(
        Utf8JsonWriter writer,
        DpoPairSideAuthorship authorship)
    {
        writer.WriteStartObject();
        writer.WriteString("authorKind", authorship.AuthorKind.ToString());
        writer.WritePropertyName("openTeacherPassport");
        WriteTeacherProvenanceValue(
            writer,
            authorship.OpenTeacherPassport,
            CurrentSchemaVersion);
        writer.WriteEndObject();
    }

    private static void WriteNullableEnum<TEnum>(
        Utf8JsonWriter writer,
        string propertyName,
        TEnum? value)
        where TEnum : struct, Enum
    {
        if (value is null)
            writer.WriteNull(propertyName);
        else
            writer.WriteString(propertyName, value.Value.ToString());
    }

    private static bool TryReadTrainingMetadata(
        JsonElement element,
        int schemaVersion,
        out TrainingTeacherProvenance? provenance,
        out TrainingDataGovernanceDecision? governance)
    {
        provenance = null;
        governance = null;
        if (element.ValueKind == JsonValueKind.Null)
            return schemaVersion < ContentBoundSchemaVersion;
        if (element.ValueKind != JsonValueKind.Object)
            return false;

        var properties = element.EnumerateObject().ToArray();
        if (properties.Length != 2)
            return false;
        var names = properties
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);
        if (names.Count != 2
            || !names.SetEquals(["provenance", "governanceDecision"])
            || !element.TryGetProperty("provenance", out var provenanceElement)
            || !TryReadTeacherProvenanceValue(
                provenanceElement,
                schemaVersion,
                out provenance)
            || !element.TryGetProperty("governanceDecision", out var governanceElement)
            || !TryReadGovernanceDecisionValue(governanceElement, out governance))
        {
            return false;
        }

        return schemaVersion < ContentBoundSchemaVersion || provenance is not null;
    }

    private static bool TryReadTeacherProvenanceValue(
        JsonElement element,
        int schemaVersion,
        out TrainingTeacherProvenance? provenance)
    {
        provenance = null;
        if (element.ValueKind == JsonValueKind.Null)
            return schemaVersion < ContentBoundSchemaVersion;
        if (element.ValueKind != JsonValueKind.Object)
            return false;

        var properties = element.EnumerateObject().ToArray();
        var names = properties
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);
        var expectedNames = new HashSet<string>(
            [
                    "sourceKind",
                    "family",
                    "accessMode",
                    "hostingProvider",
                    "modelId",
                    "modelVersion",
                    "modelLicense",
                    "providerTermsChecked",
                    "providerTermsAllowTraining",
                    "closedProprietaryModel",
                    "teacherOutputUsedForTraining",
                    "requestId",
                    "invocationCount",
            ],
            StringComparer.Ordinal);
        if (schemaVersion >= ArtifactProvenanceSchemaVersion)
        {
            expectedNames.Add("modelArtifactDigestKind");
            expectedNames.Add("modelArtifactDigest");
        }
        if (schemaVersion >= ContentBoundSchemaVersion)
            expectedNames.Add("trainingContentSha256");
        if (properties.Length != expectedNames.Count
            || names.Count != expectedNames.Count
            || !names.SetEquals(expectedNames))
        {
            return false;
        }

        if (!TryGetEnum(element, "sourceKind", out TrainingTeacherSourceKind sourceKind)
            || !TryGetNullableEnum(
                element,
                "family",
                out OpenWeightTeacherFamily? family)
            || !TryGetNullableEnum(
                element,
                "accessMode",
                out TrainingTeacherAccessMode? accessMode)
            || !TryGetNullableEnum(
                element,
                "hostingProvider",
                out TrainingTeacherHostingProvider? hostingProvider)
            || !TryGetString(element, "modelId", out var modelId)
            || !TryGetString(element, "modelVersion", out var modelVersion)
            || !TryGetNullableEnum(
                element,
                "modelLicense",
                out TrainingTeacherModelLicense? modelLicense)
            || !TryGetBoolean(
                element,
                "providerTermsChecked",
                out var providerTermsChecked)
            || !TryGetBoolean(
                element,
                "providerTermsAllowTraining",
                out var providerTermsAllowTraining)
            || !TryGetBoolean(
                element,
                "closedProprietaryModel",
                out var closedProprietaryModel)
            || !TryGetBoolean(
                element,
                "teacherOutputUsedForTraining",
                out var teacherOutputUsedForTraining)
            || !TryGetString(element, "requestId", out var requestId)
            || !element.TryGetProperty("invocationCount", out var invocationElement)
            || invocationElement.ValueKind != JsonValueKind.Number
            || !invocationElement.TryGetInt32(out var invocationCount))
        {
            return false;
        }

        var modelArtifactDigestKind = string.Empty;
        var modelArtifactDigest = string.Empty;
        if (schemaVersion >= ArtifactProvenanceSchemaVersion
            && (!TryGetString(
                    element,
                    "modelArtifactDigestKind",
                    out modelArtifactDigestKind)
                || !TryGetString(
                    element,
                    "modelArtifactDigest",
                    out modelArtifactDigest)
                     || !(schemaVersion >= ContentBoundSchemaVersion &&
                         modelArtifactDigestKind.Length == 0 &&
                         modelArtifactDigest.Length == 0 ||
                     IsCanonicalV3TeacherProvenance(
                         sourceKind,
                         family,
                         accessMode,
                         hostingProvider,
                         modelId,
                         modelVersion,
                         modelLicense,
                         providerTermsChecked,
                         providerTermsAllowTraining,
                         closedProprietaryModel,
                         teacherOutputUsedForTraining,
                         requestId,
                         invocationCount,
                         modelArtifactDigestKind,
                         modelArtifactDigest))))
        {
            return false;
        }
        var trainingContentSha256 = string.Empty;
        if (schemaVersion >= ContentBoundSchemaVersion &&
            (!TryGetString(
                 element,
                 "trainingContentSha256",
                 out trainingContentSha256) ||
             !LegalTrainingTeacherAdmissionAuthority
                 .IsCanonicalSha256Digest(trainingContentSha256)))
        {
            return false;
        }

        provenance = new TrainingTeacherProvenance(
            sourceKind,
            family,
            accessMode,
            hostingProvider,
            modelId,
            modelVersion,
            modelLicense,
            providerTermsChecked,
            providerTermsAllowTraining,
            closedProprietaryModel,
            teacherOutputUsedForTraining,
            requestId,
            invocationCount)
        {
            ModelArtifactDigestKind = modelArtifactDigestKind,
            ModelArtifactDigest = modelArtifactDigest,
            TrainingContentSha256 = trainingContentSha256
        };
        return true;
    }

    private static bool TryReadGovernanceDecisionValue(
        JsonElement element,
        out TrainingDataGovernanceDecision? governance)
    {
        governance = null;
        if (element.ValueKind == JsonValueKind.Null)
            return true;
        if (element.ValueKind != JsonValueKind.Object)
            return false;

        var properties = element.EnumerateObject().ToArray();
        var names = properties
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);
        if (properties.Length != 3
            || names.Count != 3
            || !names.SetEquals(
                ["licenseAccepted", "isQuarantined", "evidenceRefs"])
            || !TryGetBoolean(element, "licenseAccepted", out var licenseAccepted)
            || !TryGetBoolean(element, "isQuarantined", out var isQuarantined)
            || !element.TryGetProperty("evidenceRefs", out var evidenceElement)
            || evidenceElement.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        var evidenceRefs = new List<string>();
        foreach (var evidenceRef in evidenceElement.EnumerateArray())
        {
            if (evidenceRef.ValueKind != JsonValueKind.String
                || evidenceRefs.Count == 32)
            {
                return false;
            }

            evidenceRefs.Add(evidenceRef.GetString() ?? string.Empty);
        }

        governance = new TrainingDataGovernanceDecision(
            licenseAccepted,
            isQuarantined,
            evidenceRefs);
        return true;
    }

    private static bool TryReadSideAuthorships(
        JsonElement element,
        out DpoPairSideAuthorship? chosen,
        out DpoPairSideAuthorship? rejected)
    {
        chosen = null;
        rejected = null;
        if (element.ValueKind != JsonValueKind.Object)
            return false;

        var properties = element.EnumerateObject().ToArray();
        var names = properties
            .Select(static property => property.Name)
            .ToHashSet(StringComparer.Ordinal);
        if (properties.Length != 2
            || names.Count != 2
            || !names.SetEquals(["chosen", "rejected"])
            || !element.TryGetProperty("chosen", out var chosenElement)
            || !TryReadSideAuthorshipValue(chosenElement, out chosen)
            || !element.TryGetProperty("rejected", out var rejectedElement)
            || !TryReadSideAuthorshipValue(rejectedElement, out rejected))
        {
            return false;
        }

        return chosen is not null && rejected is not null;
    }

    private static bool TryReadSideAuthorshipValue(
        JsonElement element,
        out DpoPairSideAuthorship? authorship)
    {
        authorship = null;
        if (element.ValueKind != JsonValueKind.Object)
            return false;

        var properties = element.EnumerateObject().ToArray();
        var names = properties
            .Select(static property => property.Name)
            .ToHashSet(StringComparer.Ordinal);
        if (properties.Length != 2
            || names.Count != 2
            || !names.SetEquals(["authorKind", "openTeacherPassport"])
            || !TryGetEnum(element, "authorKind", out DpoPairSideAuthorKind authorKind)
            || !DpoPairSideAuthorshipPolicy.IsSupportedAuthorKind(authorKind)
            || !element.TryGetProperty(
                "openTeacherPassport",
                out var passportElement))
        {
            return false;
        }

        TrainingTeacherProvenance? passport = null;
        if (passportElement.ValueKind == JsonValueKind.Null)
        {
            if (authorKind is DpoPairSideAuthorKind.OpenTeacher)
                return false;
        }
        else
        {
            if (authorKind is not DpoPairSideAuthorKind.OpenTeacher
                || !TryReadTeacherProvenanceValue(
                    passportElement,
                    CurrentSchemaVersion,
                    out passport)
                || !DpoPairSideAuthorshipPolicy.IsPermittedOpenTeacherPassport(
                    passport,
                    requireTrainingContentSha256: true))
            {
                return false;
            }
        }

        authorship = new DpoPairSideAuthorship(authorKind, passport);
        return true;
    }

    private static bool TryGetEnum<TEnum>(
        JsonElement root,
        string propertyName,
        out TEnum value)
        where TEnum : struct, Enum
    {
        value = default;
        return root.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String
            && Enum.TryParse(
                property.GetString(),
                ignoreCase: false,
                out value)
            && Enum.IsDefined(value);
    }

    private static bool TryGetNullableEnum<TEnum>(
        JsonElement root,
        string propertyName,
        out TEnum? value)
        where TEnum : struct, Enum
    {
        value = null;
        if (!root.TryGetProperty(propertyName, out var property))
            return false;
        if (property.ValueKind == JsonValueKind.Null)
            return true;
        if (property.ValueKind != JsonValueKind.String
            || !Enum.TryParse(
                property.GetString(),
                ignoreCase: false,
                out TEnum parsed)
            || !Enum.IsDefined(parsed))
        {
            return false;
        }

        value = parsed;
        return true;
    }

    private static bool TryGetBoolean(
        JsonElement root,
        string propertyName,
        out bool value)
    {
        value = false;
        if (!root.TryGetProperty(propertyName, out var property)
            || property.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            return false;
        }

        value = property.GetBoolean();
        return true;
    }

    private static bool TryGetString(
        JsonElement root,
        string propertyName,
        out string value)
    {
        value = "";
        if (!root.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString() ?? "";
        return true;
    }

    private static OperationResult<bool> ValidatePair(DpoPair pair)
    {
        if (pair is null)
            return OperationResult<bool>.Fail(
                "DPO pair cannot be null.",
                "NULL_DPO_PAIR");
        if (string.IsNullOrWhiteSpace(pair.Prompt)
            || string.IsNullOrWhiteSpace(pair.Chosen)
            || string.IsNullOrWhiteSpace(pair.Rejected)
            || string.IsNullOrWhiteSpace(pair.Domain))
        {
            return OperationResult<bool>.Fail(
                "DPO pair fields cannot be empty.",
                "INVALID_DPO_PAIR");
        }
        var promptBytes = Encoding.UTF8.GetByteCount(pair.Prompt);
        var chosenBytes = Encoding.UTF8.GetByteCount(pair.Chosen);
        var rejectedBytes = Encoding.UTF8.GetByteCount(pair.Rejected);
        var domainBytes = Encoding.UTF8.GetByteCount(pair.Domain);
        if (!IsPersistableProvenance(pair.TeacherProvenance)
            || !IsPersistableGovernance(pair.GovernanceDecision))
        {
            return OperationResult<bool>.Fail(
                "DPO pair training metadata is invalid.",
                "INVALID_DPO_PAIR");
        }
        if (!IsPersistableSideAuthorship(pair))
        {
            return OperationResult<bool>.Fail(
                "DPO pair side authorship is invalid.",
                "INVALID_DPO_PAIR");
        }
        if (!IsPersistableProvenanceAuthorshipLink(pair))
        {
            return OperationResult<bool>.Fail(
                "DPO pair provenance does not match side authorship.",
                "INVALID_DPO_PAIR");
        }
        var provenanceBytes = GetProvenanceStringBytes(pair.TeacherProvenance);
        var governanceBytes = GetGovernanceStringBytes(pair.GovernanceDecision);
        var authorshipBytes = GetSideAuthorshipStringBytes(pair.ChosenAuthorship)
            + GetSideAuthorshipStringBytes(pair.RejectedAuthorship);
        var totalFieldBytes = (long)promptBytes
            + chosenBytes
            + rejectedBytes
            + domainBytes
            + provenanceBytes
            + governanceBytes
            + authorshipBytes;
        if (promptBytes > MaximumFieldBytes
            || chosenBytes > MaximumFieldBytes
            || rejectedBytes > MaximumFieldBytes
            || domainBytes > MaximumFieldBytes
            || totalFieldBytes > MaximumRecordBytes - 4096)
        {
            return OperationResult<bool>.Fail(
                "DPO pair fields exceed the storage limit.",
                "INVALID_DPO_PAIR");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static int GetWriteSchemaVersion(DpoPair pair)
    {
        var provenance = pair.TeacherProvenance;
        if (pair.ChosenAuthorship is not null
            && pair.RejectedAuthorship is not null
            && provenance is not null
            && LegalTrainingTeacherAdmissionAuthority.IsCanonicalSha256Digest(
                provenance.TrainingContentSha256))
        {
            return CurrentSchemaVersion;
        }
        if (provenance is null)
            return ProvenanceSchemaVersion;
        if (LegalTrainingTeacherAdmissionAuthority.IsCanonicalSha256Digest(
                provenance.TrainingContentSha256))
        {
            return ContentBoundSchemaVersion;
        }
        return provenance.ModelArtifactDigestKind.Length != 0 &&
               provenance.ModelArtifactDigest.Length != 0
            ? ArtifactProvenanceSchemaVersion
            : ProvenanceSchemaVersion;
    }

    internal static string ComputeIdentity(DpoPair pair, int schemaVersion)
    {
        var usesLegacyNullMetadataIdentity =
            schemaVersion == ProvenanceSchemaVersion
            && pair.TeacherProvenance is null
            && pair.GovernanceDecision is null;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendIdentityField(
            hash,
            schemaVersion switch
            {
                LegacySchemaVersion => "xiigen.dpo-pair.v1",
                ProvenanceSchemaVersion when usesLegacyNullMetadataIdentity =>
                    "xiigen.dpo-pair.v1",
                ProvenanceSchemaVersion => "xiigen.dpo-pair.v2",
                ArtifactProvenanceSchemaVersion => "xiigen.dpo-pair.v3",
                ContentBoundSchemaVersion => "xiigen.dpo-pair.v4",
                CurrentSchemaVersion => CurrentRecordDomain,
                _ => throw new ArgumentOutOfRangeException(nameof(schemaVersion)),
            });
        AppendIdentityField(hash, pair.Domain);
        AppendIdentityField(hash, pair.Prompt);
        AppendIdentityField(hash, pair.Chosen);
        AppendIdentityField(hash, pair.Rejected);
        if (schemaVersion != LegacySchemaVersion
            && !usesLegacyNullMetadataIdentity)
        {
            AppendIdentityField(hash, pair.TeacherProvenance is null ? "0" : "1");
            if (pair.TeacherProvenance is { } provenance)
                AppendTeacherProvenanceIdentity(hash, provenance, schemaVersion);
            AppendIdentityField(hash, pair.GovernanceDecision is null ? "0" : "1");
            if (pair.GovernanceDecision is { } governance)
                AppendGovernanceIdentity(hash, governance);
        }
        if (schemaVersion == CurrentSchemaVersion)
        {
            AppendSideAuthorshipIdentity(hash, pair.ChosenAuthorship!);
            AppendSideAuthorshipIdentity(hash, pair.RejectedAuthorship!);
        }
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void AppendSideAuthorshipIdentity(
        IncrementalHash hash,
        DpoPairSideAuthorship authorship)
    {
        AppendIdentityField(
            hash,
            ((int)authorship.AuthorKind).ToString(CultureInfo.InvariantCulture));
        AppendIdentityField(hash, authorship.OpenTeacherPassport is null ? "0" : "1");
        if (authorship.OpenTeacherPassport is { } passport)
            AppendTeacherProvenanceIdentity(hash, passport, CurrentSchemaVersion);
    }

    private static OperationResult<bool> ValidateAtomicGroup(
        DpoPairAtomicGroupEnvelope group)
    {
        if (group is null
            || group.SchemaVersion != DpoPairAtomicGroupEnvelope.CurrentSchemaVersion
            || group.InvocationCount != 1
            || !IsSafePersistedIdentity(group.RequestId)
            || !TrainingSourceProvenance.IsCanonicalSha256(
                group.GroupCommitmentSha256)
            || group.Children is null
            || group.Children.Count != 2)
        {
            return InvalidAtomicGroup<bool>();
        }
        DpoPairAtomicGroupChild[] children;
        try
        {
            children = group.Children.Take(3).ToArray();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidAtomicGroup<bool>();
        }
        if (children.Length != 2
            || children[0] is not { Ordinal: 0, Role: DpoPairAtomicGroupRole.CriticUnit }
            || children[1] is not { Ordinal: 1, Role: DpoPairAtomicGroupRole.ImproverUnit }
            || StringComparer.Ordinal.Equals(children[0].PairId, children[1].PairId))
        {
            return InvalidAtomicGroup<bool>();
        }
        foreach (var child in children)
        {
            if (child.Pair is null
                || !ValidatePair(child.Pair).IsSuccess
                || GetWriteSchemaVersion(child.Pair) != CurrentSchemaVersion
                || child.Pair.TeacherProvenance is not { } provenance
                || provenance.InvocationCount != 1
                || !StringComparer.Ordinal.Equals(provenance.RequestId, group.RequestId)
                || !StringComparer.Ordinal.Equals(
                    provenance.TrainingContentSha256,
                    child.ContentSha256)
                || !StringComparer.Ordinal.Equals(
                    ComputeIdentity(child.Pair, CurrentSchemaVersion),
                    child.PairId))
            {
                return InvalidAtomicGroup<bool>();
            }
        }
        var first = children[0].Pair.TeacherProvenance!;
        var second = children[1].Pair.TeacherProvenance!;
        if (!SameTeacherInvocation(first, second)
            || !StringComparer.Ordinal.Equals(
                children[0].Pair.Domain,
                children[1].Pair.Domain)
            || !StringComparer.Ordinal.Equals(
                ComputeAtomicGroupCommitment(
                    group.RequestId,
                    group.InvocationCount,
                    children),
                group.GroupCommitmentSha256))
        {
            return InvalidAtomicGroup<bool>();
        }
        var aggregateBytes = children.Sum(static child =>
            (long)Encoding.UTF8.GetByteCount(child.Pair.Prompt)
            + Encoding.UTF8.GetByteCount(child.Pair.Chosen)
            + Encoding.UTF8.GetByteCount(child.Pair.Rejected)
            + Encoding.UTF8.GetByteCount(child.Pair.Domain)
            + GetProvenanceStringBytes(child.Pair.TeacherProvenance)
            + GetGovernanceStringBytes(child.Pair.GovernanceDecision));
        if (aggregateBytes > MaximumRecordBytes - 8192)
            return InvalidAtomicGroup<bool>();
        return OperationResult<bool>.Ok(true);
    }

    private static bool SameTeacherInvocation(
        TrainingTeacherProvenance left,
        TrainingTeacherProvenance right)
        => left.SourceKind == right.SourceKind
           && left.Family == right.Family
           && left.AccessMode == right.AccessMode
           && left.HostingProvider == right.HostingProvider
           && StringComparer.Ordinal.Equals(left.ModelId, right.ModelId)
           && StringComparer.Ordinal.Equals(left.ModelVersion, right.ModelVersion)
           && left.ModelLicense == right.ModelLicense
           && left.ProviderTermsChecked == right.ProviderTermsChecked
           && left.ProviderTermsAllowTraining == right.ProviderTermsAllowTraining
           && left.ClosedProprietaryModel == right.ClosedProprietaryModel
           && left.TeacherOutputUsedForTraining == right.TeacherOutputUsedForTraining
           && StringComparer.Ordinal.Equals(left.RequestId, right.RequestId)
           && left.InvocationCount == right.InvocationCount
           && StringComparer.Ordinal.Equals(
               left.ModelArtifactDigestKind,
               right.ModelArtifactDigestKind)
           && StringComparer.Ordinal.Equals(
               left.ModelArtifactDigest,
               right.ModelArtifactDigest);

    private static string ComputeAtomicGroupCommitment(
        string requestId,
        int invocationCount,
        IReadOnlyList<DpoPairAtomicGroupChild> children)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendIdentityField(hash, AtomicGroupDomain);
        AppendIdentityField(
            hash,
            DpoPairAtomicGroupEnvelope.CurrentSchemaVersion.ToString(
                CultureInfo.InvariantCulture));
        AppendIdentityField(hash, requestId);
        AppendIdentityField(
            hash,
            invocationCount.ToString(CultureInfo.InvariantCulture));
        AppendIdentityField(hash, children.Count.ToString(CultureInfo.InvariantCulture));
        foreach (var child in children)
        {
            AppendIdentityField(hash, child.Ordinal.ToString(CultureInfo.InvariantCulture));
            AppendIdentityField(hash, child.Role.ToString());
            AppendIdentityField(hash, child.PairId);
            AppendIdentityField(hash, child.ContentSha256);
        }
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static DpoPairAtomicGroupReceipt ToAtomicGroupReceipt(
        DpoPairAtomicGroupEnvelope group)
        => new(
            group.GroupCommitmentSha256,
            group.RequestId,
            Array.AsReadOnly(group.Children
                .OrderBy(static child => child.Ordinal)
                .Select(static child => child.PairId)
                .ToArray()));

    private static bool AtomicGroupsEqual(
        StoredAtomicGroupRecord stored,
        DpoPairAtomicGroupEnvelope incoming)
        => StringComparer.Ordinal.Equals(
               stored.GroupCommitmentSha256,
               incoming.GroupCommitmentSha256)
           && StringComparer.Ordinal.Equals(stored.RequestId, incoming.RequestId)
           && stored.InvocationCount == incoming.InvocationCount
           && stored.Children.Length == incoming.Children.Count
           && stored.Children.Select(static child => child.Pair.PairId)
               .SequenceEqual(
                   incoming.Children.Select(static child => child.PairId),
                   StringComparer.Ordinal);

    private static bool IsPersistableProvenance(
        TrainingTeacherProvenance? provenance)
    {
        if (provenance is null)
            return true;

        return Enum.IsDefined(provenance.SourceKind)
               && IsDefined(provenance.Family)
               && IsDefined(provenance.AccessMode)
               && IsDefined(provenance.HostingProvider)
               && IsDefined(provenance.ModelLicense)
               && provenance.ModelId is not null
               && provenance.ModelVersion is not null
               && IsPersistableArtifactIdentity(provenance)
               && (provenance.TrainingContentSha256.Length == 0 ||
                   LegalTrainingTeacherAdmissionAuthority
                       .IsCanonicalSha256Digest(
                           provenance.TrainingContentSha256))
               && provenance.RequestId is not null;
    }

    private static bool IsPersistableGovernance(
        TrainingDataGovernanceDecision? governance)
        => governance is null
           || governance.EvidenceRefs.Count <= 32
              && governance.EvidenceRefs.All(static evidenceRef =>
                  evidenceRef is not null
                  && Encoding.UTF8.GetByteCount(evidenceRef) <= MaximumFieldBytes);

    private static bool IsPersistableSideAuthorship(DpoPair pair)
    {
        var chosen = pair.ChosenAuthorship;
        var rejected = pair.RejectedAuthorship;
        if (chosen is null || rejected is null)
            return chosen is null && rejected is null;

        var provenance = pair.TeacherProvenance;
        return provenance is not null
               && LegalTrainingTeacherAdmissionAuthority.IsCanonicalSha256Digest(
                   provenance.TrainingContentSha256)
               && IsPersistableSideAuthorship(chosen, provenance)
               && IsPersistableSideAuthorship(rejected, provenance);
    }

    private static bool IsPersistableSideAuthorship(
        DpoPairSideAuthorship authorship,
        TrainingTeacherProvenance pairPassport)
    {
        if (!Enum.IsDefined(authorship.AuthorKind)
            || !DpoPairSideAuthorshipPolicy.IsSupportedAuthorKind(
                authorship.AuthorKind))
        {
            return false;
        }

        if (authorship.AuthorKind is not DpoPairSideAuthorKind.OpenTeacher)
            return authorship.OpenTeacherPassport is null;

        // OpenTeacher допускается только как ссылка на тот же разрешенный
        // passport, которым уже связан контент пары; отдельный "похожий" паспорт
        // не должен стать второй правдой.
        return DpoPairSideAuthorshipPolicy.IsPermittedOpenTeacherPassport(
                   authorship.OpenTeacherPassport,
                   requireTrainingContentSha256: true)
               && DpoPairSideAuthorshipPolicy.SamePassportIdentity(
                   authorship.OpenTeacherPassport,
                   pairPassport,
                   includeTrainingContentSha256: true);
    }

    private static bool IsPersistableProvenanceAuthorshipLink(DpoPair pair)
    {
        var chosen = pair.ChosenAuthorship;
        var rejected = pair.RejectedAuthorship;
        if (chosen is null || rejected is null)
            return chosen is null && rejected is null;

        var provenance = pair.TeacherProvenance;
        if (provenance is null)
            return false;

        if (provenance.SourceKind ==
            TrainingTeacherSourceKind.DeterministicNoExternalTeacher)
        {
            // Для User/Local/Deterministic v5-пары deterministic provenance
            // должен оставаться тем же отрицательным сертификатом, что и на
            // юридической границе, иначе публичный record можно подделать
            // модельными полями без OpenTeacher-стороны.
            return LegalTrainingTeacherAdmissionAuthority
                   .IsExactDeterministicNoExternalTeacherProvenance(provenance)
                   && chosen.AuthorKind is not DpoPairSideAuthorKind.OpenTeacher
                   && rejected.AuthorKind is not DpoPairSideAuthorKind.OpenTeacher;
        }

        if (provenance.SourceKind != TrainingTeacherSourceKind.ExternalTeacherOutput)
            return false;

        // Внешний паспорт обязан указывать хотя бы на одну действительно
        // написанную учителем сторону. Для DPO, где локальная Qwen честно
        // написала и хороший, и слабый ответ, обе стороны могут ссылаться на
        // тот же content-bound паспорт; разные паспорта отсеиваются выше.
        var chosenMatches = IsSameOpenTeacherSide(chosen, provenance);
        var rejectedMatches = IsSameOpenTeacherSide(rejected, provenance);
        return chosenMatches || rejectedMatches;
    }

    private static bool IsSameOpenTeacherSide(
        DpoPairSideAuthorship authorship,
        TrainingTeacherProvenance pairPassport)
        => authorship.AuthorKind is DpoPairSideAuthorKind.OpenTeacher
           && DpoPairSideAuthorshipPolicy.SamePassportIdentity(
               authorship.OpenTeacherPassport,
               pairPassport,
               includeTrainingContentSha256: true);

    private static bool IsDefined<TEnum>(TEnum? value)
        where TEnum : struct, Enum
        => value is null || Enum.IsDefined(value.Value);

    private static long GetProvenanceStringBytes(
        TrainingTeacherProvenance? provenance)
    {
        if (provenance is null)
            return 0;

        return (long)Encoding.UTF8.GetByteCount(provenance.ModelId)
            + Encoding.UTF8.GetByteCount(provenance.ModelVersion)
            + Encoding.UTF8.GetByteCount(provenance.ModelArtifactDigestKind)
            + Encoding.UTF8.GetByteCount(provenance.ModelArtifactDigest)
            + Encoding.UTF8.GetByteCount(provenance.TrainingContentSha256)
            + Encoding.UTF8.GetByteCount(provenance.RequestId);
    }

    private static long GetGovernanceStringBytes(
        TrainingDataGovernanceDecision? governance)
        => governance?.EvidenceRefs.Sum(static evidenceRef =>
            (long)Encoding.UTF8.GetByteCount(evidenceRef)) ?? 0;

    private static long GetSideAuthorshipStringBytes(
        DpoPairSideAuthorship? authorship)
        => authorship is null
            ? 0
            : GetProvenanceStringBytes(authorship.OpenTeacherPassport);

    private static void AppendTeacherProvenanceIdentity(
        IncrementalHash hash,
        TrainingTeacherProvenance provenance,
        int schemaVersion)
    {
        AppendIdentityField(
            hash,
            ((int)provenance.SourceKind).ToString(CultureInfo.InvariantCulture));
        AppendIdentityField(hash, NullableEnumIdentity(provenance.Family));
        AppendIdentityField(hash, NullableEnumIdentity(provenance.AccessMode));
        AppendIdentityField(hash, NullableEnumIdentity(provenance.HostingProvider));
        AppendIdentityField(hash, provenance.ModelId);
        AppendIdentityField(hash, provenance.ModelVersion);
        AppendIdentityField(hash, NullableEnumIdentity(provenance.ModelLicense));
        AppendIdentityField(hash, provenance.ProviderTermsChecked ? "1" : "0");
        AppendIdentityField(hash, provenance.ProviderTermsAllowTraining ? "1" : "0");
        AppendIdentityField(hash, provenance.ClosedProprietaryModel ? "1" : "0");
        AppendIdentityField(hash, provenance.TeacherOutputUsedForTraining ? "1" : "0");
        AppendIdentityField(hash, provenance.RequestId);
        AppendIdentityField(
            hash,
            provenance.InvocationCount.ToString(CultureInfo.InvariantCulture));
        if (schemaVersion >= ArtifactProvenanceSchemaVersion)
        {
            AppendIdentityField(hash, provenance.ModelArtifactDigestKind);
            AppendIdentityField(hash, provenance.ModelArtifactDigest);
        }
        if (schemaVersion >= ContentBoundSchemaVersion)
            AppendIdentityField(hash, provenance.TrainingContentSha256);
    }

    private static bool IsPersistableArtifactIdentity(
        TrainingTeacherProvenance provenance)
    {
        if (provenance.ModelArtifactDigestKind is null
            || provenance.ModelArtifactDigest is null)
        {
            return false;
        }

        var bothEmpty = provenance.ModelArtifactDigestKind.Length == 0
                        && provenance.ModelArtifactDigest.Length == 0;
        return bothEmpty || IsCanonicalV3TeacherProvenance(provenance);
    }

    private static bool IsCanonicalV3TeacherProvenance(
        TrainingTeacherProvenance provenance)
        => IsCanonicalV3TeacherProvenance(
            provenance.SourceKind,
            provenance.Family,
            provenance.AccessMode,
            provenance.HostingProvider,
            provenance.ModelId,
            provenance.ModelVersion,
            provenance.ModelLicense,
            provenance.ProviderTermsChecked,
            provenance.ProviderTermsAllowTraining,
            provenance.ClosedProprietaryModel,
            provenance.TeacherOutputUsedForTraining,
            provenance.RequestId,
            provenance.InvocationCount,
            provenance.ModelArtifactDigestKind,
            provenance.ModelArtifactDigest);

    private static bool IsCanonicalV3TeacherProvenance(
        TrainingTeacherSourceKind sourceKind,
        OpenWeightTeacherFamily? family,
        TrainingTeacherAccessMode? accessMode,
        TrainingTeacherHostingProvider? hostingProvider,
        string? modelId,
        string? modelVersion,
        TrainingTeacherModelLicense? modelLicense,
        bool providerTermsChecked,
        bool providerTermsAllowTraining,
        bool closedProprietaryModel,
        bool teacherOutputUsedForTraining,
        string? requestId,
        int invocationCount,
        string? digestKind,
        string? digest)
        => sourceKind == TrainingTeacherSourceKind.ExternalTeacherOutput
           && family == OpenWeightTeacherFamily.Qwen
           && accessMode == TrainingTeacherAccessMode.LocalHostedOpenWeight
           && hostingProvider == TrainingTeacherHostingProvider.Ollama
           && modelLicense == TrainingTeacherModelLicense.Apache20
           && !providerTermsChecked
           && !providerTermsAllowTraining
           && !closedProprietaryModel
           && teacherOutputUsedForTraining
           && invocationCount > 0
           && LegalTrainingTeacherAdmissionAuthority
               .IsCanonicalLocalOllamaModelIdForFamily(
                   OpenWeightTeacherFamily.Qwen,
                   modelId)
           && IsSafePersistedIdentity(requestId)
           && LegalTrainingTeacherAdmissionAuthority
               .IsCanonicalLocalOllamaArtifactIdentity(
                   modelVersion,
                   digestKind,
                   digest);

    private static bool IsSafePersistedIdentity(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && value.Length <= 256
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && !value.Any(char.IsControl);

    private static string NullableEnumIdentity<TEnum>(TEnum? value)
        where TEnum : struct, Enum
        => value is null
            ? "null"
            : Convert.ToInt32(value.Value, CultureInfo.InvariantCulture)
                .ToString(CultureInfo.InvariantCulture);

    private static void AppendGovernanceIdentity(
        IncrementalHash hash,
        TrainingDataGovernanceDecision governance)
    {
        AppendIdentityField(hash, governance.LicenseAccepted ? "1" : "0");
        AppendIdentityField(hash, governance.IsQuarantined ? "1" : "0");
        AppendIdentityField(
            hash,
            governance.EvidenceRefs.Count.ToString(CultureInfo.InvariantCulture));
        foreach (var evidenceRef in governance.EvidenceRefs)
            AppendIdentityField(hash, evidenceRef);
    }

    private static void AppendIdentityField(IncrementalHash hash, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        Span<byte> length = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(length, bytes.Length);
        hash.AppendData(length);
        hash.AppendData(bytes);
    }

    private static bool IsCanonicalIdentity(string value)
        => value.Length == 64
           && value.All(character => character is >= '0' and <= '9'
               or >= 'a' and <= 'f');

    private static bool SameSemanticPair(StoredDpoPairRecord stored, DpoPair incoming)
        => StringComparer.Ordinal.Equals(stored.Domain, incoming.Domain)
           && StringComparer.Ordinal.Equals(stored.Prompt, incoming.Prompt)
           && StringComparer.Ordinal.Equals(stored.Chosen, incoming.Chosen)
           && StringComparer.Ordinal.Equals(stored.Rejected, incoming.Rejected)
           && Equals(stored.TeacherProvenance, incoming.TeacherProvenance)
           && Equals(stored.GovernanceDecision, incoming.GovernanceDecision)
           && Equals(stored.ChosenAuthorship, incoming.ChosenAuthorship)
           && Equals(stored.RejectedAuthorship, incoming.RejectedAuthorship);

    private static DpoPair ToPair(StoredDpoPairRecord record)
        => new(
            record.Prompt,
            record.Chosen,
            record.Rejected,
            record.Domain,
            record.CapturedAt)
        {
            TeacherProvenance = record.TeacherProvenance,
            GovernanceDecision = record.GovernanceDecision,
            ChosenAuthorship = record.ChosenAuthorship,
            RejectedAuthorship = record.RejectedAuthorship,
        };

    private void DeleteTemporaryFile(string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            if (_storageRoot is not null
                && PathsEqual(Path.GetDirectoryName(fullPath)!, _storageRoot)
                && !AnyExistingPathSegmentIsReparsePoint(_storageRoot)
                && !AnyExistingPathSegmentIsReparsePoint(fullPath)
                && File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // Cleanup is best-effort and must not hide the primary store result.
        }
    }

    private static OperationResult<T> FailUnavailable<T>()
        => OperationResult<T>.Fail(
            UnavailableMessage,
            "DPO_PAIR_STORE_UNAVAILABLE");

    private static OperationResult<T> FailBusy<T>()
        => OperationResult<T>.Fail(
            BusyMessage,
            "DPO_PAIR_STORE_BUSY");

    private static OperationResult<T> FailCapacity<T>()
        => OperationResult<T>.Fail(
            CapacityMessage,
            "DPO_PAIR_STORE_CAPACITY");

    private static OperationResult<T> FailCorrupt<T>()
        => OperationResult<T>.Fail(
            CorruptMessage,
            "DPO_PAIR_STORE_CORRUPT");

    private static OperationResult<T> FailCollision<T>()
        => OperationResult<T>.Fail(
            CollisionMessage,
            "DPO_PAIR_ID_COLLISION");

    private static OperationResult<T> InvalidAtomicGroup<T>()
        => OperationResult<T>.Fail(
            "The atomic DPO pair group is invalid.",
            "DPO_PAIR_ATOMIC_GROUP_INVALID");

    private sealed record StoredDpoPairRecord(
        int SchemaVersion,
        string PairId,
        string Prompt,
        string Chosen,
        string Rejected,
        string Domain,
        DateTimeOffset CapturedAt,
        TrainingTeacherProvenance? TeacherProvenance,
        TrainingDataGovernanceDecision? GovernanceDecision,
        DpoPairSideAuthorship? ChosenAuthorship,
        DpoPairSideAuthorship? RejectedAuthorship)
    {
        internal string GroupCommitmentSha256 { get; init; } = string.Empty;
        internal int GroupOrdinal { get; init; } = -1;
        internal DpoPairAtomicGroupRole? GroupRole { get; init; }
    }

    private sealed record StoredAtomicGroupChild(
        int Ordinal,
        DpoPairAtomicGroupRole Role,
        string ContentSha256,
        StoredDpoPairRecord Pair);

    private sealed record StoredAtomicGroupRecord(
        int SchemaVersion,
        string GroupCommitmentSha256,
        string RequestId,
        int InvocationCount,
        StoredAtomicGroupChild[] Children);

    private sealed record StoreSnapshot(
        IReadOnlyDictionary<string, StoredDpoPairRecord> RecordsByIdentity,
        IReadOnlyDictionary<string, StoredAtomicGroupRecord> GroupsByIdentity,
        IReadOnlyList<DpoPair> OrderedPairs)
    {
        public static StoreSnapshot Empty { get; } = new(
            new Dictionary<string, StoredDpoPairRecord>(StringComparer.Ordinal),
            new Dictionary<string, StoredAtomicGroupRecord>(StringComparer.Ordinal),
            Array.Empty<DpoPair>());
    }

    private sealed class ProcessGateEntry
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
        public int ReferenceCount { get; set; }
    }

    private sealed class ProcessMutationLease(
        string storageRoot,
        ProcessGateEntry entry) : IDisposable
    {
        private ProcessGateEntry? _entry = entry;

        public void Dispose()
        {
            var owned = Interlocked.Exchange(ref _entry, null);
            if (owned is not null)
                ReleaseOwnedProcessGate(storageRoot, owned);
        }
    }

    private sealed class MutationLease(FileStream stream) : IDisposable
    {
        private FileStream? _stream = stream;

        public bool MatchesRequestedPath(string requestedPath)
        {
            var owned = Volatile.Read(ref _stream);
            return owned is not null
                && OpenedHandleMatchesRequestedPath(
                    owned.SafeFileHandle,
                    requestedPath);
        }

        public bool HasExpectedLength()
        {
            var owned = Volatile.Read(ref _stream);
            return owned is not null
                && owned.Length == MutationLockLength;
        }

        public void Dispose()
        {
            var owned = Interlocked.Exchange(ref _stream, null);
            if (owned is null)
                return;
            try
            {
                if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
                {
                    owned.Unlock(
                        MutationLockOffset,
                        MutationLockLength);
                }
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                // Closing the handle below unconditionally releases any OS
                // byte-range lock if an explicit unlock cannot complete.
            }
            finally
            {
                owned.Dispose();
            }
        }
    }

    private sealed class MaximumLengthWriteStream(
        Stream inner,
        long maximumLength) : Stream
    {
        private long _written;

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => _written;

        public override long Position
        {
            get => _written;
            set => throw new NotSupportedException();
        }

        public override void Flush()
            => inner.Flush();

        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => Write(buffer.AsSpan(offset, count));

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length > maximumLength - _written)
                throw new DpoPairRecordTooLargeException();
            inner.Write(buffer);
            _written += buffer.Length;
        }

        public override void WriteByte(byte value)
        {
            Span<byte> buffer = stackalloc byte[1];
            buffer[0] = value;
            Write(buffer);
        }

        protected override void Dispose(bool disposing)
        {
            // The caller owns the underlying file handle so it can verify the
            // opened final path after the JSON writer has flushed.
            base.Dispose(disposing);
        }
    }

    private sealed class DpoPairRecordTooLargeException : IOException
    {
    }
}
