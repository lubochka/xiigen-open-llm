// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Diagnostics;
using System.Security;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;

/// <summary>
/// Durable authority for post-evaluation accounting reconciliation and
/// training eligibility. A provisional artifact is never training eligible
/// unless this store contains the matching committed state.
/// </summary>
public interface IDesignConversionPostEvaluationStateStore
{
    Task<OperationResult<DesignConversionPostEvaluationState>> WriteAsync(
        DesignConversionPostEvaluationState state,
        CancellationToken cancellationToken = default);

    Task<OperationResult<DesignConversionPostEvaluationState?>> ReadAsync(
        string stateRef,
        CancellationToken cancellationToken = default);

    Task<OperationResult<IReadOnlyList<DesignConversionPostEvaluationState>>> ListAsync(
        CancellationToken cancellationToken = default);

    Task<OperationResult<DesignConversionPostEvaluationState>>
        DenyTrainingEligibilityAsync(
            string stateRef,
            string expectedArtifactRecordId,
            string errorCode,
            CancellationToken cancellationToken = default)
        => Task.FromResult(
            OperationResult<DesignConversionPostEvaluationState>.Fail(
                "Post-evaluation training eligibility denial is unavailable.",
                JsonDesignConversionPostEvaluationStateStore
                    .TrainingEligibilityDenyFailedErrorCode));

    Task<OperationResult<DesignConversionPostEvaluationState>>
        ReleaseTrainingEligibilityDenialAsync(
            string stateRef,
            string expectedArtifactRecordId,
            CancellationToken cancellationToken = default)
        => Task.FromResult(
            OperationResult<DesignConversionPostEvaluationState>.Fail(
                "Post-evaluation training eligibility denial release is unavailable.",
                JsonDesignConversionPostEvaluationStateStore
                    .TrainingEligibilityDenyReleaseFailedErrorCode));
}

public interface IDesignConversionPostEvaluationStateRootProvider
{
    string PostEvaluationStateRootDirectory { get; }
}

public sealed record DesignConversionPostEvaluationState(
    string StateRef,
    string ReservationRef,
    string UserRef,
    DateTimeOffset PeriodStartUtc,
    DateTimeOffset PeriodEndExclusiveUtc,
    decimal ChargedCostUsd,
    string AccountingStatus,
    string AccountingDesiredAction,
    bool SettlementAttempted,
    bool SettlementDefinitivelyNotApplied,
    bool ReleaseAttempted,
    string PersistenceStatus,
    string LedgerRecordId,
    string ArtifactRecordId,
    string CompletionBindingKind,
    string CompletionBindingRef,
    string LastErrorCode,
    DateTimeOffset UpdatedAtUtc,
    long Version)
{
    [System.Text.Json.Serialization.JsonIgnore]
    public bool TrainingEligibilityDenied { get; init; }
}

public sealed record DesignConversionPostEvaluationStateStoreOptions(
    long MaxSingleStateBytes,
    int MaxStateFileCount,
    long MaxTotalStateBytes,
    TimeSpan MutationLockTimeout,
    TimeSpan MutationLockRetryDelay)
{
    public static DesignConversionPostEvaluationStateStoreOptions Default { get; } =
        new(
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxSingleStateBytes,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxStateFileCount,
            JsonDesignConversionPostEvaluationStateStore.DefaultMaxTotalStateBytes,
            JsonDesignConversionPostEvaluationStateStore.DefaultMutationLockTimeout,
            JsonDesignConversionPostEvaluationStateStore.DefaultMutationLockRetryDelay);

    internal Action<DesignConversionHandleBoundIoStage, string>? BeforeHandleIoForTesting
    {
        get;
        init;
    }

    internal Action<string>? BeforeTrainingEligibilityDenialDeleteForTesting
    {
        get;
        init;
    }

    internal Action<string>? AfterTrainingEligibilityDenialDeleteForTesting
    {
        get;
        init;
    }

    internal Action<DesignConversionHandleBoundIoStage, string>?
        BeforeTrainingEligibilityDenialHandleIoForTesting
    {
        get;
        init;
    }
}

public sealed class JsonDesignConversionPostEvaluationStateStore
    : IDesignConversionPostEvaluationStateStore
{
    public const string DirectoryName = "post-evaluation-finalization";
    public const string SchemaVersion = "design-conversion-post-evaluation-state-v2";
    public const string LegacySchemaVersion = "design-conversion-post-evaluation-state-v1";
    public const string StateRefPrefix = "dcm-post-evaluation-state-";
    public const string AccountingPending = "pending";
    public const string AccountingReconciliationRequired = "reconciliation_required";
    public const string AccountingSettled = "settled";
    public const string AccountingReleased = "released";
    public const string DesiredSettle = "settle";
    public const string DesiredRelease = "release";
    public const string DesiredNone = "none";
    public const string PersistenceProvisional = "provisional";
    public const string PersistenceCommitted = "committed";
    public const string PersistenceQuarantined = "quarantined";
    public const string CompletionBindingLocalArtifactDbRecord =
        "local_artifact_db_record";
    public const string CompletionBindingVisualCostLedgerRecord =
        "visual_cost_ledger_record";
    public const string ReadFailedErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_STATE_READ_FAILED";
    public const string WriteFailedErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_STATE_WRITE_FAILED";
    public const string ConflictErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_STATE_CONFLICT";
    public const string TrustBoundaryErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_STATE_TRUST_BOUNDARY_FAILED";
    public const string QuotaExceededErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_STATE_QUOTA_EXCEEDED";
    public const string MutationLockTimeoutErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_STATE_LOCK_TIMEOUT";
    public const string PublicationCleanupRequiredErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_STATE_PUBLICATION_CLEANUP_REQUIRED";
    public const string LegacyCompletionBindingUnverifiableErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_LEGACY_COMPLETION_BINDING_UNVERIFIABLE";
    public const string TrainingEligibilityDenyFailedErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_TRAINING_ELIGIBILITY_DENY_FAILED";
    public const string TrainingEligibilityDenyReleaseFailedErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_TRAINING_ELIGIBILITY_DENY_RELEASE_FAILED";
    public const string TrainingEligibilityDenialSchemaVersion =
        "design-conversion-post-evaluation-training-denial-v1";
    internal const string TrainingEligibilityDenialDirectorySuffix =
        ".training-denials";
    internal const string TrainingEligibilityDenialMutationLockFileName =
        ".training-denials.mutation.lock";
    internal const string TrainingEligibilityDenialFileSuffix =
        ".deny.json";
    internal const string TrainingEligibilityDenialReleaseWitnessSuffix =
        ".release.witness.json";

    public const long DefaultMaxSingleStateBytes = 64L * 1024L;
    public const int DefaultMaxStateFileCount = 4096;
    public const long DefaultMaxTotalStateBytes = 64L * 1024L * 1024L;
    public static readonly TimeSpan DefaultMutationLockTimeout = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan DefaultMutationLockRetryDelay = TimeSpan.FromMilliseconds(25);

    public const string StateRefMetadataKey = "post_evaluation_state_ref";
    public const string PersistenceStatusMetadataKey = "post_evaluation_persistence_status";
    public const string TrainingEligibleMetadataKey = "post_evaluation_training_eligible";
    public const string NoReservationUserRef = "not-applicable";
    public static readonly DateTimeOffset NoReservationPeriodStartUtc =
        DateTimeOffset.UnixEpoch;
    public static readonly DateTimeOffset NoReservationPeriodEndExclusiveUtc =
        DateTimeOffset.UnixEpoch.AddTicks(1);

    internal const string MutationLockFileName = ".post-evaluation-state.mutation.lock";
    internal const string WriteTempSuffix = ".write.tmp";

    private static readonly JsonSerializerOptions JsonOptions =
        new(DesignConversionCheckpointJsonOptions.Standard)
        {
            WriteIndented = false
        };
    private static readonly Encoding StrictUtf8 =
        new UTF8Encoding(false, true);

    private static readonly string[] EnvelopePropertyNames =
    [
        "schemaVersion",
        "state"
    ];

    private static readonly string[] StatePropertyNames =
    [
        "stateRef",
        "reservationRef",
        "userRef",
        "periodStartUtc",
        "periodEndExclusiveUtc",
        "chargedCostUsd",
        "accountingStatus",
        "accountingDesiredAction",
        "settlementAttempted",
        "settlementDefinitivelyNotApplied",
        "releaseAttempted",
        "persistenceStatus",
        "ledgerRecordId",
        "artifactRecordId",
        "completionBindingKind",
        "completionBindingRef",
        "lastErrorCode",
        "updatedAtUtc",
        "version"
    ];

    private static readonly string[] TrainingEligibilityDenialPropertyNames =
    [
        "schemaVersion",
        "stateRef",
        "artifactRecordId",
        "errorCode"
    ];

    private static readonly string[] LegacyStatePropertyNames =
    [
        "stateRef",
        "reservationRef",
        "userRef",
        "periodStartUtc",
        "periodEndExclusiveUtc",
        "chargedCostUsd",
        "accountingStatus",
        "accountingDesiredAction",
        "settlementAttempted",
        "settlementDefinitivelyNotApplied",
        "releaseAttempted",
        "persistenceStatus",
        "ledgerRecordId",
        "artifactRecordId",
        "lastErrorCode",
        "updatedAtUtc",
        "version"
    ];

    private static readonly object GateSync = new();
    private static readonly Dictionary<string, InProcessGateEntry> InProcessGates =
        new(GetPathComparer());
    private const int MaxObservedInventoryExamples = 8;
    private const int MaxObservedInventoryExampleCharacters = 80;

    private readonly string _rootDirectory;
    private readonly DesignConversionPostEvaluationStateStoreOptions _options;

    public JsonDesignConversionPostEvaluationStateStore(
        string rootDirectory,
        DesignConversionPostEvaluationStateStoreOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A post-evaluation state root is required.", nameof(rootDirectory));
        if (!Path.IsPathFullyQualified(rootDirectory))
            throw new ArgumentException(
                "A post-evaluation state root must be fully qualified.",
                nameof(rootDirectory));

        _options = options ?? DesignConversionPostEvaluationStateStoreOptions.Default;
        ValidateOptions(_options);
        _rootDirectory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootDirectory));
    }

    public string RootDirectory => _rootDirectory;

    public long MaxSingleStateBytes => _options.MaxSingleStateBytes;

    public int MaxStateFileCount => _options.MaxStateFileCount;

    public long MaxTotalStateBytes => _options.MaxTotalStateBytes;

    public TimeSpan MutationLockTimeout => _options.MutationLockTimeout;

    public static string BuildStateRef(string idempotencySource)
    {
        if (string.IsNullOrWhiteSpace(idempotencySource))
            throw new ArgumentException("A post-evaluation idempotency source is required.", nameof(idempotencySource));

        return StateRefPrefix +
               Convert.ToHexString(
                       System.Security.Cryptography.SHA256.HashData(
                           Encoding.UTF8.GetBytes(idempotencySource)))
                   .ToLowerInvariant();
    }

    public Task<OperationResult<DesignConversionPostEvaluationState>> WriteAsync(
        DesignConversionPostEvaluationState state,
        CancellationToken cancellationToken = default)
        => WriteCoreAsync(state, cancellationToken);

    public async Task<OperationResult<DesignConversionPostEvaluationState>>
        DenyTrainingEligibilityAsync(
            string stateRef,
            string expectedArtifactRecordId,
            string errorCode,
            CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled<DesignConversionPostEvaluationState>();
        if (!IsCanonicalStateRef(stateRef) ||
            string.IsNullOrWhiteSpace(expectedArtifactRecordId) ||
            string.IsNullOrWhiteSpace(errorCode))
        {
            return Conflict<DesignConversionPostEvaluationState>();
        }

        try
        {
            var marker = new TrainingEligibilityDenialEnvelope(
                TrainingEligibilityDenialSchemaVersion,
                stateRef,
                expectedArtifactRecordId,
                errorCode.Trim());
            var verified = await PersistTrainingEligibilityDenialAsync(
                marker,
                cancellationToken);
            if (!verified.IsSuccess ||
                verified.Value is null ||
                !HasExactTrainingEligibilityDenial(
                    verified.Value,
                    stateRef,
                    expectedArtifactRecordId))
            {
                return TrainingEligibilityDenyFailed();
            }

            var read = await ReadAsync(stateRef, cancellationToken);
            return read.IsSuccess && read.Value is not null
                ? OperationResult<DesignConversionPostEvaluationState>.Ok(
                    read.Value)
                : TrainingEligibilityDenyFailed();
        }
        catch (OperationCanceledException)
        {
            return Cancelled<DesignConversionPostEvaluationState>();
        }
        catch (Exception ex) when (
            IsStorageOrParseException(ex) ||
            ex is ArgumentException or InvalidOperationException)
        {
            return TrainingEligibilityDenyFailed();
        }
    }

    public async Task<OperationResult<DesignConversionPostEvaluationState>>
        ReleaseTrainingEligibilityDenialAsync(
            string stateRef,
            string expectedArtifactRecordId,
            CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled<DesignConversionPostEvaluationState>();
        if (!IsCanonicalStateRef(stateRef) ||
            string.IsNullOrWhiteSpace(expectedArtifactRecordId))
        {
            return Conflict<DesignConversionPostEvaluationState>();
        }

        var current = await ReadAsync(stateRef, cancellationToken);
        if (!current.IsSuccess ||
            current.Value is null ||
            !string.Equals(
                current.Value.PersistenceStatus,
                PersistenceCommitted,
                StringComparison.Ordinal) ||
            !HasExactLocalArtifactCompletionBinding(
                current.Value,
                expectedArtifactRecordId))
        {
            return TrainingEligibilityDenyReleaseFailed();
        }

        var markerLeaf = GetTrainingEligibilityDenialLeaf(
            stateRef,
            expectedArtifactRecordId);
        var lease = await AcquireTrainingEligibilityDenialLeaseAsync(
            cancellationToken);
        if (!lease.IsSuccess)
            return TrainingEligibilityDenyReleaseFailed();
        await using var heldLease = lease.Value!;

        var inventory = InspectTrainingEligibilityDenialInventoryNoLock(
            heldLease.Directory,
            heldLease.CrossProcessLock);
        if (!inventory.IsSuccess)
            return TrainingEligibilityDenyReleaseFailed();
        var entry = inventory.Value!.Entries.SingleOrDefault(candidate =>
            string.Equals(candidate.Entry.Name, markerLeaf, StringComparison.Ordinal));
        var witnessLeaf =
            GetTrainingEligibilityDenialReleaseWitnessLeaf(
                stateRef,
                expectedArtifactRecordId);
        var existingWitness =
            inventory.Value.Entries.SingleOrDefault(candidate =>
                string.Equals(
                    candidate.Entry.Name,
                    witnessLeaf,
                    StringComparison.Ordinal));
        if (entry is null)
        {
            if (existingWitness is null ||
                !HasExactTrainingEligibilityDenial(
                    existingWitness.Denial,
                    stateRef,
                    expectedArtifactRecordId))
            {
                return TrainingEligibilityDenyReleaseFailed();
            }

            return DeleteTrainingEligibilityReleaseWitnessNoLock(
                heldLease.Directory,
                existingWitness,
                current.Value);
        }

        if (!HasExactTrainingEligibilityDenial(
                entry.Denial,
                stateRef,
                expectedArtifactRecordId))
        {
            return TrainingEligibilityDenyReleaseFailed();
        }

        if ((existingWitness is not null &&
             !Equals(existingWitness.Denial, entry.Denial)) ||
            (existingWitness is null &&
             !CreateTrainingEligibilityReleaseWitnessNoLock(
                 heldLease.Directory,
                 witnessLeaf,
                 entry.Denial,
                 inventory.Value.Entries.Count,
                 inventory.Value.TotalBytes)))
        {
            return TrainingEligibilityDenyReleaseFailed();
        }

        var witnessedInventory =
            InspectTrainingEligibilityDenialInventoryNoLock(
                heldLease.Directory,
                heldLease.CrossProcessLock);
        if (!witnessedInventory.IsSuccess)
            return TrainingEligibilityDenyReleaseFailed();
        var witnessEntry =
            witnessedInventory.Value!.Entries.SingleOrDefault(candidate =>
                string.Equals(
                    candidate.Entry.Name,
                    witnessLeaf,
                    StringComparison.Ordinal));
        if (witnessEntry is null ||
            !Equals(witnessEntry.Denial, entry.Denial))
        {
            return TrainingEligibilityDenyReleaseFailed();
        }

        var deletionDurablyCommitted = false;
        try
        {
            deletionDurablyCommitted =
                heldLease.Directory.DeleteIfRegular(
                    markerLeaf,
                    entry.Entry.Identity) ==
                DesignConversionHandleBoundDeleteDisposition.DurableDelete;
        }
        catch (DesignConversionHandleBoundPostCommitCleanupException)
        {
            deletionDurablyCommitted = true;
        }
        catch (DesignConversionHandleBoundDeleteDispositionException ex) when (
            ex.Disposition ==
            DesignConversionHandleBoundDeleteDisposition
                .NamespaceEntryRemovedDirectoryFlushNotConfirmed)
        {
            // The primary marker is not committed as released until its
            // directory durability is confirmed. The recovery witness created
            // above remains authoritative and keeps training denied.
            return TrainingEligibilityDenyReleaseFailed();
        }
        catch (OperationCanceledException)
        {
            return Cancelled<DesignConversionPostEvaluationState>();
        }
        catch (Exception ex) when (
            IsStorageOrParseException(ex) ||
            ex is ArgumentException or InvalidOperationException)
        {
            return TrainingEligibilityDenyReleaseFailed();
        }

        if (!deletionDurablyCommitted)
            return TrainingEligibilityDenyReleaseFailed();

        // The exact marker deletion + directory flush is the semantic commit
        // point. The pre-created witness makes every earlier failure visibly
        // fail closed even if marker restoration itself becomes impossible.
        // Cleanup after this point may still fail, but it cannot report
        // success while a durable witness keeps eligibility denied. A later
        // release call recognizes witness-only state as an explicit,
        // identity-bound reconciliation attempt.
        return DeleteTrainingEligibilityReleaseWitnessNoLock(
            heldLease.Directory,
            witnessEntry,
            current.Value);
    }

    private static OperationResult<DesignConversionPostEvaluationState>
        DeleteTrainingEligibilityReleaseWitnessNoLock(
            DesignConversionHandleBoundDirectory directory,
            TrainingEligibilityDenialInventoryEntry witnessEntry,
            DesignConversionPostEvaluationState current)
    {
        try
        {
            var disposition =
                directory.DeleteIfRegular(
                    witnessEntry.Entry.Name,
                    witnessEntry.Entry.Identity);
            if (disposition !=
                DesignConversionHandleBoundDeleteDisposition.DurableDelete)
            {
                return TrainingEligibilityDenyReleaseFailed();
            }
        }
        catch (DesignConversionHandleBoundPostCommitCleanupException)
        {
            return OperationResult<
                DesignConversionPostEvaluationState>.Ok(current);
        }
        catch (DesignConversionHandleBoundDeleteDispositionException ex) when (
            ex.Disposition ==
            DesignConversionHandleBoundDeleteDisposition
                .NamespaceEntryRemovedDirectoryFlushNotConfirmed)
        {
            // This is the final recovery-witness cleanup only. The handle-bound
            // delete proved that the current namespace entry is gone. Returning
            // failure here would contradict a fresh eligibility read. If a
            // crash restores the witness because the directory flush was not
            // durable, the restored witness only re-establishes denial.
            return OperationResult<
                DesignConversionPostEvaluationState>.Ok(current);
        }
        catch (Exception ex) when (
            IsStorageOrParseException(ex) ||
            ex is ArgumentException or InvalidOperationException)
        {
            return TrainingEligibilityDenyReleaseFailed();
        }

        return OperationResult<DesignConversionPostEvaluationState>.Ok(
            current);
    }

    private async Task<OperationResult<DesignConversionPostEvaluationState>>
        WriteCoreAsync(
            DesignConversionPostEvaluationState state,
            CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled<DesignConversionPostEvaluationState>();
        if (!IsCanonicalStateRef(state?.StateRef))
            return Conflict<DesignConversionPostEvaluationState>();

        var lease = await AcquireStoreLeaseAsync(cancellationToken);
        if (!lease.IsSuccess)
            return lease.MapError<DesignConversionPostEvaluationState>();

        await using var heldLease = lease.Value!;
        var stateLeaf = GetStateFileName(state!.StateRef);
        var tempLeaf = string.Empty;
        try
        {
            var inventory = PrepareInventoryNoLock(
                heldLease.Directory,
                heldLease.CrossProcessLock,
                ownedTempLeaf: null);
            if (!inventory.IsSuccess)
                return inventory.MapError<DesignConversionPostEvaluationState>();

            var existingEntry = inventory.Value!.StateFiles.SingleOrDefault(
                entry => string.Equals(entry.Name, stateLeaf, StringComparison.Ordinal));
            var existing = ReadStateFileNoLock(
                heldLease.Directory,
                stateLeaf,
                state.StateRef,
                allowMissing: true,
                existingEntry?.Identity);
            if (!existing.IsSuccess)
                return existing.MapError<DesignConversionPostEvaluationState>();

            var merged = existing.Value is null
                ? NormalizeNew(state)
                : MergeMonotonic(existing.Value, state);
            if (!merged.IsSuccess)
                return merged;

            var envelopeBytes = JsonSerializer.SerializeToUtf8Bytes(
                new StateEnvelope(SchemaVersion, merged.Value!),
                JsonOptions);
            if (envelopeBytes.LongLength <= 0L ||
                envelopeBytes.LongLength > _options.MaxSingleStateBytes)
            {
                return QuotaExceeded<DesignConversionPostEvaluationState>();
            }

            var isNewState = existing.Value is null;
            if (isNewState && inventory.Value!.StateFiles.Count >= _options.MaxStateFileCount)
                return QuotaExceeded<DesignConversionPostEvaluationState>();
            if (!TryAddWithinLimit(
                    inventory.Value!.TotalBytes,
                    envelopeBytes.LongLength,
                    _options.MaxTotalStateBytes))
            {
                return QuotaExceeded<DesignConversionPostEvaluationState>();
            }

            tempLeaf = BuildWriteTempFileName(state.StateRef);
            using (var temp = heldLease.Directory.CreateNew(
                       tempLeaf,
                       FileShare.None,
                       writeThrough: true))
            {
                temp.SetLength(0L);
                await temp.WriteAsync(envelopeBytes, cancellationToken);
                await temp.FlushAsync(cancellationToken);
                temp.Flush(flushToDisk: true);

                cancellationToken.ThrowIfCancellationRequested();
                var publicationInventory = PrepareInventoryNoLock(
                    heldLease.Directory,
                    heldLease.CrossProcessLock,
                    tempLeaf,
                    temp);
                if (!publicationInventory.IsSuccess)
                    return publicationInventory.MapError<DesignConversionPostEvaluationState>();
                if (publicationInventory.Value!.TotalBytes > _options.MaxTotalStateBytes ||
                    publicationInventory.Value.StateFiles.Count > _options.MaxStateFileCount)
                {
                    return QuotaExceeded<DesignConversionPostEvaluationState>();
                }

                var destinationExpectation = BuildPublicationDestinationExpectationNoLock(
                    heldLease.Directory,
                    stateLeaf,
                    state.StateRef,
                    publicationInventory.Value.StateFiles.SingleOrDefault(
                        entry => string.Equals(
                            entry.Name,
                            stateLeaf,
                            StringComparison.Ordinal)));
                if (!destinationExpectation.IsSuccess)
                {
                    return destinationExpectation
                        .MapError<DesignConversionPostEvaluationState>();
                }

                heldLease.Directory.AtomicReplaceAndFlush(
                    temp,
                    tempLeaf,
                    stateLeaf,
                    checked((int)_options.MaxSingleStateBytes),
                    destinationExpectation.Value);
                tempLeaf = string.Empty;
            }

            var publishedInventory = PrepareInventoryNoLock(
                heldLease.Directory,
                heldLease.CrossProcessLock,
                ownedTempLeaf: null);
            if (!publishedInventory.IsSuccess)
                return publishedInventory.MapError<DesignConversionPostEvaluationState>();
            var publishedEntry = publishedInventory.Value!.StateFiles.SingleOrDefault(
                entry => string.Equals(entry.Name, stateLeaf, StringComparison.Ordinal));
            var published = ReadStateFileNoLock(
                heldLease.Directory,
                stateLeaf,
                state.StateRef,
                allowMissing: false,
                publishedEntry?.Identity);
            return published.IsSuccess && published.Value is not null
                ? OperationResult<DesignConversionPostEvaluationState>.Ok(published.Value)
                : ReadFailed<DesignConversionPostEvaluationState>();
        }
        catch (OperationCanceledException)
        {
            return Cancelled<DesignConversionPostEvaluationState>();
        }
        catch (DesignConversionHandleBoundPostCommitCleanupException)
        {
            return PublicationCleanupRequired<DesignConversionPostEvaluationState>();
        }
        catch (DesignConversionHandleBoundTrustException ex)
        {
            return OperationResult<DesignConversionPostEvaluationState>.Fail(
                ex.Message,
                TrustBoundaryErrorCode);
        }
        catch (Exception ex) when (IsStorageOrParseException(ex))
        {
            return WriteFailed<DesignConversionPostEvaluationState>();
        }
        finally
        {
            TryDeleteOwnedTempNoThrow(heldLease.Directory, tempLeaf);
        }
    }

    public async Task<OperationResult<DesignConversionPostEvaluationState?>> ReadAsync(
        string stateRef,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled<DesignConversionPostEvaluationState?>();
        if (!IsCanonicalStateRef(stateRef))
            return Conflict<DesignConversionPostEvaluationState?>();

        var lease = await AcquireStoreLeaseAsync(cancellationToken);
        if (!lease.IsSuccess)
            return lease.MapError<DesignConversionPostEvaluationState?>();
        await using var heldLease = lease.Value!;

        var stateLeaf = GetStateFileName(stateRef);
        var inventory = PrepareInventoryNoLock(
            heldLease.Directory,
            heldLease.CrossProcessLock,
            ownedTempLeaf: null);
        if (!inventory.IsSuccess)
            return inventory.MapError<DesignConversionPostEvaluationState?>();
        var entry = inventory.Value!.StateFiles.SingleOrDefault(
            candidate => string.Equals(candidate.Name, stateLeaf, StringComparison.Ordinal));
        return ReadStateFileNoLock(
            heldLease.Directory,
            stateLeaf,
            stateRef,
            allowMissing: true,
            entry?.Identity);
    }

    public async Task<OperationResult<IReadOnlyList<DesignConversionPostEvaluationState>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled<IReadOnlyList<DesignConversionPostEvaluationState>>();

        var lease = await AcquireStoreLeaseAsync(cancellationToken);
        if (!lease.IsSuccess)
            return lease.MapError<IReadOnlyList<DesignConversionPostEvaluationState>>();
        await using var heldLease = lease.Value!;

        var inventory = PrepareInventoryNoLock(
            heldLease.Directory,
            heldLease.CrossProcessLock,
            ownedTempLeaf: null);
        if (!inventory.IsSuccess)
            return inventory.MapError<IReadOnlyList<DesignConversionPostEvaluationState>>();

        var records = new List<DesignConversionPostEvaluationState>(
            inventory.Value!.StateFiles.Count);
        foreach (var stateFile in inventory.Value.StateFiles)
        {
            if (cancellationToken.IsCancellationRequested)
                return Cancelled<IReadOnlyList<DesignConversionPostEvaluationState>>();

            var expectedStateRef = Path.GetFileNameWithoutExtension(stateFile.Name);
            var read = ReadStateFileNoLock(
                heldLease.Directory,
                stateFile.Name,
                expectedStateRef,
                allowMissing: false,
                stateFile.Identity);
            if (!read.IsSuccess || read.Value is null)
                return read.MapError<IReadOnlyList<DesignConversionPostEvaluationState>>();
            records.Add(read.Value);
        }

        var denialInventory =
            await ReadTrainingEligibilityDenialsAsync(cancellationToken);
        if (!denialInventory.IsSuccess)
        {
            return denialInventory.MapError<
                IReadOnlyList<DesignConversionPostEvaluationState>>();
        }

        var denials = denialInventory.Value!;
        var snapshot = records
            .Select(record => record with
            {
                TrainingEligibilityDenied = denials.Any(denial =>
                    HasExactTrainingEligibilityDenial(
                        denial,
                        record.StateRef,
                        record.ArtifactRecordId))
            })
            .OrderBy(record => record.StateRef, StringComparer.Ordinal)
            .ToArray();
        return OperationResult<IReadOnlyList<DesignConversionPostEvaluationState>>.Ok(
            snapshot);
    }

    public static async Task<bool> IsCommittedForTrainingAsync(
        string artifactRootDirectory,
        string? stateRef,
        string? expectedArtifactRecordId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(artifactRootDirectory) ||
            !Path.IsPathFullyQualified(artifactRootDirectory) ||
            !IsCanonicalStateRef(stateRef) ||
            string.IsNullOrWhiteSpace(expectedArtifactRecordId))
        {
            return false;
        }

        try
        {
            var store = new JsonDesignConversionPostEvaluationStateStore(
                Path.Combine(
                    Path.GetFullPath(artifactRootDirectory),
                    DirectoryName));
            var inventory = await store.ListAsync(cancellationToken);
            var matching = inventory.IsSuccess
                ? inventory.Value!
                    .Where(candidate =>
                        string.Equals(
                            candidate.StateRef,
                            stateRef,
                            StringComparison.Ordinal))
                    .ToArray()
                : [];
            return matching.Length == 1 &&
                   !matching[0].TrainingEligibilityDenied &&
                   string.Equals(
                       matching[0].PersistenceStatus,
                       PersistenceCommitted,
                       StringComparison.Ordinal) &&
                   string.Equals(
                       matching[0].ArtifactRecordId,
                       expectedArtifactRecordId,
                       StringComparison.Ordinal) &&
                   string.Equals(
                       matching[0].CompletionBindingKind,
                       CompletionBindingLocalArtifactDbRecord,
                       StringComparison.Ordinal) &&
                   string.Equals(
                       matching[0].CompletionBindingRef,
                       expectedArtifactRecordId,
                       StringComparison.Ordinal);
        }
        catch (Exception ex) when (
            IsStorageOrParseException(ex) ||
            ex is ArgumentException or InvalidOperationException)
        {
            return false;
        }
    }

    public static bool IsCanonicalStateRef(string? stateRef)
        => !string.IsNullOrWhiteSpace(stateRef) &&
           stateRef.StartsWith(StateRefPrefix, StringComparison.Ordinal) &&
           stateRef.Length == StateRefPrefix.Length + 64 &&
           stateRef.AsSpan(StateRefPrefix.Length).ToString().All(
               ch => ch is >= '0' and <= '9' or >= 'a' and <= 'f');

    private async Task<OperationResult<StoreLease>> AcquireStoreLeaseAsync(
        CancellationToken cancellationToken)
    {
        var gateKey = NormalizePathKey(_rootDirectory);
        var stopwatch = Stopwatch.StartNew();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_options.MutationLockTimeout);

        InProcessGateLease? inProcess = null;
        DesignConversionHandleBoundDirectory? directory = null;
        try
        {
            inProcess = await AcquireInProcessGateAsync(gateKey, timeout.Token);
            directory = DesignConversionHandleBoundDirectory.AcquireOrCreate(
                _rootDirectory,
                _options.BeforeHandleIoForTesting);

            while (true)
            {
                timeout.Token.ThrowIfCancellationRequested();
                try
                {
                    var stream = directory.OpenOrCreate(
                        MutationLockFileName,
                        FileShare.None,
                        writeThrough: true);
                    if (stream.Length != 0L)
                    {
                        stream.Dispose();
                        directory.Dispose();
                        await inProcess.DisposeAsync();
                        return TrustBoundaryFailed<StoreLease>();
                    }

                    try
                    {
                        _options.BeforeHandleIoForTesting?.Invoke(
                            DesignConversionHandleBoundIoStage.MutationLockAcquired,
                            Path.Combine(_rootDirectory, MutationLockFileName));
                        return OperationResult<StoreLease>.Ok(
                            new StoreLease(stream, directory, inProcess));
                    }
                    catch
                    {
                        stream.Dispose();
                        throw;
                    }
                }
                catch (DesignConversionHandleBoundSharingViolationException)
                {
                    var remaining = _options.MutationLockTimeout - stopwatch.Elapsed;
                    if (remaining <= TimeSpan.Zero)
                        throw new OperationCanceledException(timeout.Token);
                    await Task.Delay(
                        remaining < _options.MutationLockRetryDelay
                            ? remaining
                            : _options.MutationLockRetryDelay,
                        timeout.Token);
                }
            }
        }
        catch (OperationCanceledException)
        {
            directory?.Dispose();
            if (inProcess is not null)
                await inProcess.DisposeAsync();
            return cancellationToken.IsCancellationRequested
                ? Cancelled<StoreLease>()
                : LockTimeout<StoreLease>();
        }
        catch (Exception ex) when (IsStorageOrParseException(ex))
        {
            directory?.Dispose();
            if (inProcess is not null)
                await inProcess.DisposeAsync();
            return TrustBoundaryFailed<StoreLease>();
        }
    }

    private OperationResult<StoreInventory> PrepareInventoryNoLock(
        DesignConversionHandleBoundDirectory directory,
        FileStream crossProcessLock,
        string? ownedTempLeaf,
        FileStream? ownedTempStream = null)
    {
        var inventory = InspectInventoryNoLock(
            directory,
            crossProcessLock,
            ownedTempLeaf,
            ownedTempStream);
        if (!inventory.IsSuccess)
            return inventory;

        if (inventory.Value!.WriteTemps.Count > 0)
        {
            if (ownedTempLeaf is not null)
                return TrustBoundaryFailed<StoreInventory>();

            var reconciled =
                ReconcileRestartedSameIdentityWriteTempsNoLock(
                    directory,
                    inventory.Value);
            if (!reconciled.IsSuccess)
                return reconciled;

            inventory = InspectInventoryNoLock(
                directory,
                crossProcessLock,
                ownedTempLeaf: null);
            if (!inventory.IsSuccess)
                return inventory;
        }

        if (inventory.Value!.PublicationBackups.Count == 0)
            return inventory;

        // После перезапуска одно имя backup-файла не доказывает, что файл
        // создан именно нашей прерванной транзакцией. Поэтому recovery из
        // backup больше не продвигает его в живой state автоматически:
        // оператор должен вручную сверить destination/backup, а код сохраняет
        // оба файла нетронутыми.
        return OperationResult<StoreInventory>.Fail(
            "Post-evaluation state publication backup requires manual reconciliation before restart recovery can continue; all destination and backup files were preserved.",
            TrustBoundaryErrorCode);
    }

    private OperationResult<StoreInventory> InspectInventoryNoLock(
        DesignConversionHandleBoundDirectory directory,
        FileStream crossProcessLock,
        string? ownedTempLeaf,
        FileStream? ownedTempStream = null)
    {
        try
        {
            if ((ownedTempLeaf is null) != (ownedTempStream is null))
            {
                return OperationResult<StoreInventory>.Fail(
                    "Owned temporary state file name and handle must be checked together.",
                    TrustBoundaryErrorCode);
            }
            var maximumEntryCount = checked(
                _options.MaxStateFileCount +
                _options.MaxStateFileCount +
                1 +
                (ownedTempLeaf is null ? 0 : 1));
            var namePreflight = PreflightInventoryNamesNoLock(
                directory,
                crossProcessLock,
                ownedTempLeaf,
                maximumEntryCount);
            if (!namePreflight.IsSuccess)
                return namePreflight.MapError<StoreInventory>();

            var stateFiles = new List<DesignConversionHandleBoundEntry>();
            var publicationBackups = new List<DesignConversionHandleBoundEntry>();
            var writeTemps = new List<StoreWriteTempEntry>();
            long totalBytes = 0L;
            var persistedEntryCount = 0;
            var lockObserved = false;
            var ownedTempObserved = false;
            var observedNames = new List<string>();
            foreach (var entry in directory.EnumerateEntries(
                         maximumEntryCount,
                         MutationLockFileName,
                         crossProcessLock,
                         ownedTempLeaf,
                         ownedTempStream))
            {
                observedNames.Add(entry.Name);
                var isLock = string.Equals(
                    entry.Name,
                    MutationLockFileName,
                    StringComparison.Ordinal);
                var isOwnedTemp = ownedTempLeaf is not null &&
                                  string.Equals(entry.Name, ownedTempLeaf, StringComparison.Ordinal);
                lockObserved |= isLock;
                ownedTempObserved |= isOwnedTemp;
                var isState = TryGetStateRefFromFileName(entry.Name, out _);
                var hasBackupSuffix = entry.Name.EndsWith(
                    DesignConversionHandleBoundDirectory.PublicationBackupSuffix,
                    StringComparison.Ordinal);
                var isCanonicalBackup = hasBackupSuffix &&
                                        TryGetPublicationBackupTarget(
                                            entry.Name,
                                            out _,
                                            out _);
                var tempDestinationLeafName = string.Empty;
                var tempStateRef = string.Empty;
                var isWriteTemp = !isOwnedTemp &&
                                  TryGetWriteTempTarget(
                                      entry.Name,
                                      out tempDestinationLeafName,
                                      out tempStateRef);
                if (!isLock &&
                    !isOwnedTemp &&
                    !isState &&
                    !isCanonicalBackup &&
                    !isWriteTemp)
                {
                    return OperationResult<StoreInventory>.Fail(
                        $"Unexpected post-evaluation state file '{entry.Name}' was found before publication.",
                        TrustBoundaryErrorCode);
                }

                if (entry.Length < 0L ||
                    !TryAddWithinLimit(totalBytes, entry.Length, _options.MaxTotalStateBytes))
                {
                    return QuotaExceeded<StoreInventory>();
                }

                totalBytes = checked(totalBytes + entry.Length);
                if (isLock && entry.Length != 0L)
                {
                    return OperationResult<StoreInventory>.Fail(
                        "Post-evaluation state mutation lock file must stay empty.",
                        TrustBoundaryErrorCode);
                }
                if (isState || isCanonicalBackup)
                {
                    if (entry.Length <= 0L || entry.Length > _options.MaxSingleStateBytes)
                        return QuotaExceeded<StoreInventory>();
                    persistedEntryCount++;
                    if (persistedEntryCount > _options.MaxStateFileCount)
                        return QuotaExceeded<StoreInventory>();
                    if (isState)
                        stateFiles.Add(entry);
                    else
                        publicationBackups.Add(entry);
                }
                else if (isOwnedTemp &&
                         (entry.Length <= 0L || entry.Length > _options.MaxSingleStateBytes))
                {
                    return QuotaExceeded<StoreInventory>();
                }
                else if (isWriteTemp)
                {
                    if (entry.Length <= 0L || entry.Length > _options.MaxSingleStateBytes)
                        return QuotaExceeded<StoreInventory>();
                    writeTemps.Add(
                        new StoreWriteTempEntry(
                            entry,
                            tempDestinationLeafName,
                            tempStateRef));
                }
            }
            if (!lockObserved ||
                (ownedTempLeaf is not null && !ownedTempObserved))
            {
                return OperationResult<StoreInventory>.Fail(
                    ownedTempLeaf is null
                        ? "Post-evaluation state inventory did not observe the held mutation lock. " +
                          BuildInventoryObservedSummary(
                              observedNames,
                              MutationLockFileName,
                              ownedTempLeaf,
                              lockObserved,
                              ownedTempObserved)
                        : "Post-evaluation state inventory did not observe held files. " +
                          $"lockObserved={lockObserved}; ownedTempObserved={ownedTempObserved}; " +
                          BuildInventoryObservedSummary(
                              observedNames,
                              MutationLockFileName,
                              ownedTempLeaf,
                              lockObserved,
                              ownedTempObserved),
                    TrustBoundaryErrorCode);
            }

            return OperationResult<StoreInventory>.Ok(
                new StoreInventory(
                    stateFiles.OrderBy(item => item.Name, StringComparer.Ordinal).ToArray(),
                    publicationBackups
                        .OrderBy(item => item.Name, StringComparer.Ordinal)
                        .ToArray(),
                    writeTemps
                        .OrderBy(item => item.Entry.Name, StringComparer.Ordinal)
                        .ToArray(),
                    totalBytes));
        }
        catch (DesignConversionHandleBoundEnumerationLimitException)
        {
            return QuotaExceeded<StoreInventory>();
        }
        catch (Exception ex) when (
            ex is InvalidDataException or DesignConversionHandleBoundTrustException)
        {
            return TrustBoundaryFailed<StoreInventory>();
        }
        catch (Exception ex) when (IsStorageOrParseException(ex))
        {
            return ReadFailed<StoreInventory>();
        }
    }

    private OperationResult<bool> PreflightInventoryNamesNoLock(
        DesignConversionHandleBoundDirectory directory,
        FileStream crossProcessLock,
        string? ownedTempLeaf,
        int maximumEntryCount)
    {
        var persistedEntryCount = 0;
        foreach (var name in directory.EnumerateNames(
                     maximumEntryCount,
                     MutationLockFileName,
                     crossProcessLock))
        {
            var isLock = string.Equals(
                name,
                MutationLockFileName,
                StringComparison.Ordinal);
            var isOwnedTemp = ownedTempLeaf is not null &&
                              string.Equals(name, ownedTempLeaf, StringComparison.Ordinal);
            var isState = TryGetStateRefFromFileName(name, out _);
            var hasBackupSuffix = name.EndsWith(
                DesignConversionHandleBoundDirectory.PublicationBackupSuffix,
                StringComparison.Ordinal);
            var isCanonicalBackup = hasBackupSuffix &&
                                    TryGetPublicationBackupTarget(
                                        name,
                                        out _,
                                        out _);
            var isWriteTemp = !isOwnedTemp &&
                              TryGetWriteTempTarget(name, out _, out _);
            if (!isLock &&
                !isOwnedTemp &&
                !isState &&
                !isCanonicalBackup &&
                !isWriteTemp)
            {
                return OperationResult<bool>.Fail(
                    $"Unexpected post-evaluation state file '{name}' was found before publication.",
                    TrustBoundaryErrorCode);
            }

            if (!isState && !isCanonicalBackup)
                continue;

            // Сначала считаем только имена persisted state/backup: превышение
            // квоты не должно открывать лишний leaf handle или запускать recovery.
            persistedEntryCount++;
            if (persistedEntryCount > _options.MaxStateFileCount)
                return QuotaExceeded<bool>();
        }

        return OperationResult<bool>.Ok(true);
    }

    private OperationResult<StoreInventory>
        ReconcileRestartedSameIdentityWriteTempsNoLock(
            DesignConversionHandleBoundDirectory directory,
            StoreInventory inventory)
    {
        foreach (var temp in inventory.WriteTemps)
        {
            try
            {
                var stateEntry = inventory.StateFiles.SingleOrDefault(entry =>
                    string.Equals(
                        entry.Name,
                        temp.DestinationLeafName,
                        StringComparison.Ordinal));
                // Recovery после linkat crash window разрешён только когда
                // temp и canonical state — один и тот же device+inode и размер.
                if (stateEntry is null ||
                    stateEntry.Identity != temp.Entry.Identity ||
                    stateEntry.Length != temp.Entry.Length)
                {
                    return TrustBoundaryFailed<StoreInventory>();
                }

                using var stateStream = directory.TryOpenExisting(
                    stateEntry.Name,
                    FileShare.Read,
                    writeThrough: false,
                    stateEntry.Identity);
                using var tempStream = directory.TryOpenExisting(
                    temp.Entry.Name,
                    FileShare.Read | FileShare.Write | FileShare.Delete,
                    writeThrough: true,
                    temp.Entry.Identity);
                if (stateStream is null || tempStream is null)
                    return TrustBoundaryFailed<StoreInventory>();

                // Читаем canonical state перед cleanup: даже правильная identity
                // не спасает повреждённый payload, его нельзя молча считать good.
                var parsed = ReadStrictStateStreamNoLock(
                    directory,
                    stateStream,
                    stateEntry.Name,
                    temp.StateRef,
                    stateEntry.Identity);
                if (!parsed.IsSuccess)
                    return parsed.MapError<StoreInventory>();

                // Оба handle'а должны видеть ровно две ссылки; иначе это не наш
                // незавершённый fallback, а изменённая namespace-ситуация.
                if (directory.GetLinkCount(stateStream, stateEntry.Name) != 2 ||
                    directory.GetLinkCount(tempStream, temp.Entry.Name) != 2)
                {
                    return TrustBoundaryFailed<StoreInventory>();
                }

                // Это не удаление "любого tmp": temp и canonical state уже
                // доказаны двумя именами одного валидного inode после fallback.
                var disposition = directory.DeleteHeldRegular(
                    tempStream,
                    temp.Entry.Name,
                    temp.Entry.Identity,
                    expectedLinkCount: 2);
                if (disposition !=
                    DesignConversionHandleBoundDeleteDisposition.DurableDelete)
                {
                    return PublicationCleanupRequired<StoreInventory>();
                }
            }
            catch (DesignConversionHandleBoundPostCommitCleanupException)
            {
                return PublicationCleanupRequired<StoreInventory>();
            }
            catch (DesignConversionHandleBoundDeleteDispositionException ex) when (
                ex.Disposition ==
                DesignConversionHandleBoundDeleteDisposition
                    .NamespaceEntryRemovedDirectoryFlushNotConfirmed)
            {
                return PublicationCleanupRequired<StoreInventory>();
            }
            catch (Exception ex) when (
                ex is InvalidDataException or DesignConversionHandleBoundTrustException)
            {
                return TrustBoundaryFailed<StoreInventory>();
            }
            catch (Exception ex) when (IsStorageOrParseException(ex))
            {
                return ReadFailed<StoreInventory>();
            }
        }

        return OperationResult<StoreInventory>.Ok(inventory);
    }

    private OperationResult<DesignConversionPostEvaluationState?> ReadStateFileNoLock(
        DesignConversionHandleBoundDirectory directory,
        string leafName,
        string expectedStateRef,
        bool allowMissing,
        DesignConversionFileIdentity? expectedIdentity)
    {
        try
        {
            var fileName = Path.GetFileName(leafName);
            if (!TryGetStateRefFromFileName(fileName, out var filenameStateRef) ||
                !string.Equals(filenameStateRef, expectedStateRef, StringComparison.Ordinal))
            {
                return ReadFailed<DesignConversionPostEvaluationState?>();
            }

            using var stream = directory.TryOpenExisting(
                leafName,
                FileShare.Read,
                writeThrough: false,
                expectedIdentity);
            if (stream is null)
            {
                return allowMissing
                    ? OperationResult<DesignConversionPostEvaluationState?>.Ok(null)
                    : ReadFailed<DesignConversionPostEvaluationState?>();
            }

            var parsed = ReadStrictStateStreamNoLock(
                directory,
                stream,
                leafName,
                expectedStateRef,
                expectedIdentity);
            return parsed.IsSuccess
                ? OperationResult<DesignConversionPostEvaluationState?>.Ok(parsed.Value)
                : parsed.MapError<DesignConversionPostEvaluationState?>();
        }
        catch (Exception ex) when (
            ex is InvalidDataException or DesignConversionHandleBoundTrustException)
        {
            return TrustBoundaryFailed<DesignConversionPostEvaluationState?>();
        }
        catch (Exception ex) when (IsStorageOrParseException(ex))
        {
            return ReadFailed<DesignConversionPostEvaluationState?>();
        }
    }

    private OperationResult<DesignConversionPostEvaluationState> ReadStrictStateStreamNoLock(
        DesignConversionHandleBoundDirectory directory,
        FileStream stream,
        string leafName,
        string expectedStateRef,
        DesignConversionFileIdentity? expectedIdentity)
    {
        if (stream.Length <= 0L || stream.Length > _options.MaxSingleStateBytes)
            return QuotaExceeded<DesignConversionPostEvaluationState>();

        var expectedLength = checked((int)stream.Length);
        var bytes = new byte[expectedLength];
        stream.Position = 0L;
        var totalRead = 0;
        while (totalRead < expectedLength)
        {
            var read = stream.Read(bytes, totalRead, expectedLength - totalRead);
            if (read == 0)
                return ReadFailed<DesignConversionPostEvaluationState>();
            totalRead += read;
        }

        if (stream.ReadByte() != -1)
            return QuotaExceeded<DesignConversionPostEvaluationState>();
        if (bytes.Length >= 3 &&
            bytes[0] == 0xEF &&
            bytes[1] == 0xBB &&
            bytes[2] == 0xBF)
        {
            return ReadFailed<DesignConversionPostEvaluationState>();
        }

        var actualIdentity = directory.GetIdentity(stream, leafName);
        if (expectedIdentity is not null &&
            actualIdentity != expectedIdentity.Value)
        {
            return TrustBoundaryFailed<DesignConversionPostEvaluationState>();
        }

        return ParseStrictEnvelope(bytes, expectedStateRef);
    }

    private static OperationResult<DesignConversionPostEvaluationState> ParseStrictEnvelope(
        ReadOnlyMemory<byte> utf8,
        string expectedStateRef)
    {
        try
        {
            using var document = JsonDocument.Parse(
                utf8,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = 4
                });
            var root = document.RootElement;
            if (!HasExactUniqueProperties(root, EnvelopePropertyNames) ||
                root.GetProperty("schemaVersion").ValueKind != JsonValueKind.String)
            {
                return ReadFailed<DesignConversionPostEvaluationState>();
            }

            var schemaVersion = root.GetProperty("schemaVersion").GetString();
            var stateElement = root.GetProperty("state");
            DesignConversionPostEvaluationState? state;
            if (string.Equals(schemaVersion, SchemaVersion, StringComparison.Ordinal))
            {
                if (!HasExactUniqueProperties(stateElement, StatePropertyNames))
                    return ReadFailed<DesignConversionPostEvaluationState>();
                state = stateElement.Deserialize<DesignConversionPostEvaluationState>(JsonOptions);
            }
            else if (string.Equals(schemaVersion, LegacySchemaVersion, StringComparison.Ordinal))
            {
                if (!HasExactUniqueProperties(stateElement, LegacyStatePropertyNames))
                    return ReadFailed<DesignConversionPostEvaluationState>();
                var legacy = stateElement.Deserialize<LegacyPostEvaluationState>(JsonOptions);
                state = legacy is null ? null : UpgradeLegacyState(legacy);
            }
            else
            {
                return ReadFailed<DesignConversionPostEvaluationState>();
            }

            if (state is null ||
                !string.Equals(state.StateRef, expectedStateRef, StringComparison.Ordinal))
            {
                return ReadFailed<DesignConversionPostEvaluationState>();
            }

            var normalized = ValidateStateSemantics(state);
            return normalized.IsSuccess
                ? OperationResult<DesignConversionPostEvaluationState>.Ok(state)
                : ReadFailed<DesignConversionPostEvaluationState>();
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException)
        {
            return ReadFailed<DesignConversionPostEvaluationState>();
        }
    }

    private static DesignConversionPostEvaluationState UpgradeLegacyState(
        LegacyPostEvaluationState legacy)
    {
        var persistenceStatus = legacy.PersistenceStatus;
        var lastErrorCode = legacy.LastErrorCode;

        if (legacy.PersistenceStatus == PersistenceCommitted)
        {
            // V1 did not persist the completion authority that made a write
            // terminal. ArtifactRecordId/LedgerRecordId are observations, not
            // proof that the corresponding durable target exists. Reading a
            // v1 committed label therefore preserves the evidence fields but
            // quarantines the state until a fresh v2 execution proves and
            // binds an exact completion target.
            persistenceStatus = PersistenceQuarantined;
            lastErrorCode = LegacyCompletionBindingUnverifiableErrorCode;
        }

        return new DesignConversionPostEvaluationState(
            legacy.StateRef,
            legacy.ReservationRef,
            legacy.UserRef,
            legacy.PeriodStartUtc,
            legacy.PeriodEndExclusiveUtc,
            legacy.ChargedCostUsd,
            legacy.AccountingStatus,
            legacy.AccountingDesiredAction,
            legacy.SettlementAttempted,
            legacy.SettlementDefinitivelyNotApplied,
            legacy.ReleaseAttempted,
            persistenceStatus,
            legacy.LedgerRecordId,
            legacy.ArtifactRecordId,
            CompletionBindingKind: string.Empty,
            CompletionBindingRef: string.Empty,
            lastErrorCode,
            legacy.UpdatedAtUtc,
            legacy.Version);
    }

    private static bool HasExactUniqueProperties(
        JsonElement element,
        IReadOnlyCollection<string> requiredNames)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return false;

        var remaining = new HashSet<string>(requiredNames, StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!remaining.Remove(property.Name))
                return false;
        }

        return remaining.Count == 0;
    }

    private static OperationResult<DesignConversionPostEvaluationState> NormalizeNew(
        DesignConversionPostEvaluationState state)
    {
        var semantic = ValidateStateSemantics(state);
        if (!semantic.IsSuccess ||
            state.Version != 1L ||
            state.PersistenceStatus != PersistenceProvisional ||
            !string.IsNullOrWhiteSpace(state.LedgerRecordId) ||
            !string.IsNullOrWhiteSpace(state.ArtifactRecordId) ||
            !string.IsNullOrWhiteSpace(state.CompletionBindingKind) ||
            !string.IsNullOrWhiteSpace(state.CompletionBindingRef) ||
            !string.IsNullOrWhiteSpace(state.LastErrorCode))
        {
            return Conflict<DesignConversionPostEvaluationState>();
        }

        var isNoReservationInitial =
            state.AccountingStatus == AccountingSettled &&
            state.AccountingDesiredAction == DesiredNone &&
            string.Equals(state.ReservationRef, state.StateRef, StringComparison.Ordinal) &&
            state.ChargedCostUsd == 0m &&
            !state.SettlementAttempted &&
            !state.SettlementDefinitivelyNotApplied &&
            !state.ReleaseAttempted;
        var isReservedInitial =
            state.AccountingStatus == AccountingPending &&
            state.AccountingDesiredAction == DesiredSettle &&
            !state.SettlementAttempted &&
            !state.SettlementDefinitivelyNotApplied &&
            !state.ReleaseAttempted;
        return isNoReservationInitial || isReservedInitial
            ? OperationResult<DesignConversionPostEvaluationState>.Ok(state)
            : Conflict<DesignConversionPostEvaluationState>();
    }

    private static OperationResult<DesignConversionPostEvaluationState> MergeMonotonic(
        DesignConversionPostEvaluationState existing,
        DesignConversionPostEvaluationState candidate)
    {
        if (!ValidateStateSemantics(candidate).IsSuccess ||
            candidate.Version != existing.Version ||
            existing.Version >= long.MaxValue ||
            candidate.UpdatedAtUtc < existing.UpdatedAtUtc ||
            !HasSameImmutableIdentity(existing, candidate) ||
            ((existing.SettlementAttempted || candidate.SettlementAttempted) &&
             existing.ChargedCostUsd != candidate.ChargedCostUsd) ||
            (existing.SettlementAttempted && !candidate.SettlementAttempted) ||
            (existing.SettlementDefinitivelyNotApplied &&
             !candidate.SettlementDefinitivelyNotApplied) ||
            (existing.ReleaseAttempted && !candidate.ReleaseAttempted) ||
            !CanBindImmutableResultId(existing.LedgerRecordId, candidate.LedgerRecordId) ||
            !CanBindImmutableResultId(existing.ArtifactRecordId, candidate.ArtifactRecordId) ||
            !CanBindImmutableResultId(
                existing.CompletionBindingKind,
                candidate.CompletionBindingKind) ||
            !CanBindImmutableResultId(
                existing.CompletionBindingRef,
                candidate.CompletionBindingRef) ||
             !IsAllowedAccountingTransition(existing, candidate) ||
             !IsAllowedPersistenceTransition(existing, candidate))
        {
            return Conflict<DesignConversionPostEvaluationState>();
        }

        return OperationResult<DesignConversionPostEvaluationState>.Ok(
            candidate with
            {
                Version = checked(existing.Version + 1L)
            });
    }

    private static OperationResult<bool> ValidateStateSemantics(
        DesignConversionPostEvaluationState state)
    {
        if (!IsCanonicalStateRef(state.StateRef) ||
            string.IsNullOrWhiteSpace(state.ReservationRef) ||
            string.IsNullOrWhiteSpace(state.UserRef) ||
            !IsUtc(state.PeriodStartUtc) ||
            !IsUtc(state.PeriodEndExclusiveUtc) ||
            !IsUtc(state.UpdatedAtUtc) ||
            state.PeriodEndExclusiveUtc <= state.PeriodStartUtc ||
            state.ChargedCostUsd < 0m ||
            state.Version <= 0L ||
            !IsKnownAccountingStatus(state.AccountingStatus) ||
            !IsKnownDesiredAction(state.AccountingDesiredAction) ||
            !IsKnownPersistenceStatus(state.PersistenceStatus) ||
            !HasValidReservationBinding(state) ||
            !HasConsistentAccountingShape(state) ||
            !HasConsistentPersistenceShape(state))
        {
            return Conflict<bool>();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool HasValidReservationBinding(
        DesignConversionPostEvaluationState state)
    {
        if (IsCanonicalNoReservationSentinel(state))
            return true;

        return !string.Equals(state.ReservationRef, state.StateRef, StringComparison.Ordinal) &&
               string.Equals(
                   state.StateRef,
                   BuildStateRef(state.ReservationRef),
                   StringComparison.Ordinal);
    }

    public static bool IsCanonicalNoReservationSentinel(
        DesignConversionPostEvaluationState state)
        =>
            string.Equals(state.ReservationRef, state.StateRef, StringComparison.Ordinal) &&
            string.Equals(state.UserRef, NoReservationUserRef, StringComparison.Ordinal) &&
            state.PeriodStartUtc == NoReservationPeriodStartUtc &&
            state.PeriodEndExclusiveUtc == NoReservationPeriodEndExclusiveUtc &&
            state.ChargedCostUsd == 0m &&
            state.AccountingStatus == AccountingSettled &&
            state.AccountingDesiredAction == DesiredNone &&
            !state.SettlementAttempted &&
            !state.SettlementDefinitivelyNotApplied &&
            !state.ReleaseAttempted;

    private static bool HasConsistentAccountingShape(DesignConversionPostEvaluationState state)
    {
        if (state.AccountingStatus == AccountingPending)
        {
            return state.AccountingDesiredAction == DesiredSettle
                ? !state.SettlementDefinitivelyNotApplied && !state.ReleaseAttempted
                : state.AccountingDesiredAction == DesiredRelease &&
                  state.SettlementDefinitivelyNotApplied;
        }

        if (state.AccountingStatus == AccountingReconciliationRequired)
        {
            return state.AccountingDesiredAction == DesiredSettle
                ? state.SettlementAttempted &&
                  !state.SettlementDefinitivelyNotApplied &&
                  !state.ReleaseAttempted
                : state.AccountingDesiredAction == DesiredRelease &&
                  state.SettlementDefinitivelyNotApplied;
        }

        if (state.AccountingStatus == AccountingSettled)
        {
            var noReservation = string.Equals(
                state.ReservationRef,
                state.StateRef,
                StringComparison.Ordinal);
            return state.AccountingDesiredAction == DesiredNone &&
                   !state.SettlementDefinitivelyNotApplied &&
                   !state.ReleaseAttempted &&
                   (state.SettlementAttempted || noReservation);
        }

        return state.AccountingStatus == AccountingReleased &&
               state.AccountingDesiredAction == DesiredNone &&
               state.SettlementDefinitivelyNotApplied &&
               state.ReleaseAttempted;
    }

    private static bool HasConsistentPersistenceShape(DesignConversionPostEvaluationState state)
    {
        if (state.PersistenceStatus == PersistenceProvisional)
        {
            return string.IsNullOrWhiteSpace(state.LedgerRecordId) &&
                   string.IsNullOrWhiteSpace(state.ArtifactRecordId) &&
                   string.IsNullOrWhiteSpace(state.CompletionBindingKind) &&
                   string.IsNullOrWhiteSpace(state.CompletionBindingRef);
        }

        if (state.PersistenceStatus == PersistenceCommitted)
        {
            if (!IsKnownCompletionBindingKind(
                    state.CompletionBindingKind) ||
                string.IsNullOrWhiteSpace(state.CompletionBindingRef))
            {
                return false;
            }

            if (state.CompletionBindingKind ==
                CompletionBindingLocalArtifactDbRecord)
            {
                return !string.IsNullOrWhiteSpace(state.ArtifactRecordId) &&
                       string.Equals(
                           state.CompletionBindingRef,
                           state.ArtifactRecordId,
                           StringComparison.Ordinal);
            }

            return !string.IsNullOrWhiteSpace(state.LedgerRecordId) &&
                   string.Equals(
                       state.CompletionBindingRef,
                       state.LedgerRecordId,
                       StringComparison.Ordinal);
        }

        return state.PersistenceStatus == PersistenceQuarantined &&
               !string.IsNullOrWhiteSpace(state.LastErrorCode);
    }

    private static bool IsAllowedAccountingTransition(
        DesignConversionPostEvaluationState existing,
        DesignConversionPostEvaluationState candidate)
    {
        if (existing.AccountingStatus is AccountingSettled or AccountingReleased)
        {
            return candidate.AccountingStatus == existing.AccountingStatus &&
                   candidate.AccountingDesiredAction == DesiredNone;
        }

        if (existing.AccountingStatus == AccountingReconciliationRequired)
        {
            if (candidate.AccountingStatus == AccountingReconciliationRequired)
            {
                return candidate.AccountingDesiredAction == existing.AccountingDesiredAction;
            }

            return candidate.AccountingStatus switch
            {
                AccountingSettled => existing.AccountingDesiredAction == DesiredSettle,
                AccountingReleased => existing.AccountingDesiredAction == DesiredRelease,
                _ => false
            };
        }

        if (existing.AccountingStatus != AccountingPending)
            return false;
        if (candidate.AccountingStatus == AccountingPending)
        {
            return candidate.AccountingDesiredAction == existing.AccountingDesiredAction ||
                   (existing.AccountingDesiredAction == DesiredSettle &&
                    candidate.AccountingDesiredAction == DesiredRelease &&
                    candidate.SettlementDefinitivelyNotApplied);
        }

        if (candidate.AccountingStatus == AccountingReconciliationRequired)
        {
            return candidate.AccountingDesiredAction == existing.AccountingDesiredAction ||
                   (existing.AccountingDesiredAction == DesiredSettle &&
                    candidate.AccountingDesiredAction == DesiredRelease &&
                    candidate.SettlementDefinitivelyNotApplied);
        }

        return candidate.AccountingStatus switch
        {
            AccountingSettled => existing.AccountingDesiredAction == DesiredSettle,
            AccountingReleased => candidate.SettlementDefinitivelyNotApplied,
            _ => false
        };
    }

    private static bool IsAllowedPersistenceTransition(
        DesignConversionPostEvaluationState existing,
        DesignConversionPostEvaluationState candidate)
    {
        if (existing.PersistenceStatus == PersistenceCommitted)
            return candidate.PersistenceStatus == PersistenceCommitted;
        if (existing.PersistenceStatus == PersistenceQuarantined)
            return candidate.PersistenceStatus == PersistenceQuarantined;
        return existing.PersistenceStatus == PersistenceProvisional &&
               candidate.PersistenceStatus is
                   PersistenceProvisional or PersistenceCommitted or PersistenceQuarantined;
    }

    private static bool HasSameImmutableIdentity(
        DesignConversionPostEvaluationState existing,
        DesignConversionPostEvaluationState candidate)
        => string.Equals(existing.StateRef, candidate.StateRef, StringComparison.Ordinal) &&
           string.Equals(existing.ReservationRef, candidate.ReservationRef, StringComparison.Ordinal) &&
           string.Equals(existing.UserRef, candidate.UserRef, StringComparison.Ordinal) &&
           existing.PeriodStartUtc == candidate.PeriodStartUtc &&
           existing.PeriodEndExclusiveUtc == candidate.PeriodEndExclusiveUtc;

    private static bool CanBindImmutableResultId(string existing, string candidate)
        => string.IsNullOrWhiteSpace(existing)
            ? true
            : string.Equals(existing, candidate, StringComparison.Ordinal);

    private static bool IsKnownAccountingStatus(string status)
        => status is
            AccountingPending or
            AccountingReconciliationRequired or
            AccountingSettled or
            AccountingReleased;

    private static bool IsKnownDesiredAction(string action)
        => action is DesiredSettle or DesiredRelease or DesiredNone;

    private static bool IsKnownPersistenceStatus(string status)
        => status is PersistenceProvisional or PersistenceCommitted or PersistenceQuarantined;

    private static bool IsKnownCompletionBindingKind(string kind)
        => kind is
            CompletionBindingLocalArtifactDbRecord or
            CompletionBindingVisualCostLedgerRecord;

    private static bool HasExactLocalArtifactCompletionBinding(
        DesignConversionPostEvaluationState state,
        string expectedArtifactRecordId)
        => !string.IsNullOrWhiteSpace(expectedArtifactRecordId) &&
           string.Equals(
               state.ArtifactRecordId,
               expectedArtifactRecordId,
               StringComparison.Ordinal) &&
           string.Equals(
               state.CompletionBindingKind,
               CompletionBindingLocalArtifactDbRecord,
               StringComparison.Ordinal) &&
           string.Equals(
               state.CompletionBindingRef,
               expectedArtifactRecordId,
               StringComparison.Ordinal);

    private static bool IsUtc(DateTimeOffset value)
        => value.Offset == TimeSpan.Zero &&
           value != DateTimeOffset.MinValue &&
           value != DateTimeOffset.MaxValue;

    private static string GetStateFileName(string stateRef)
        => stateRef + ".json";

    private static string BuildWriteTempFileName(string stateRef)
        => $"{stateRef}.json.{Guid.NewGuid():N}{WriteTempSuffix}";

    private static bool TryGetWriteTempTarget(
        string tempLeafName,
        out string destinationLeafName,
        out string stateRef)
    {
        destinationLeafName = string.Empty;
        stateRef = string.Empty;
        if (!tempLeafName.EndsWith(WriteTempSuffix, StringComparison.Ordinal))
            return false;

        var stem = tempLeafName[..^WriteTempSuffix.Length];
        var nonceSeparator = stem.LastIndexOf('.');
        if (nonceSeparator <= 0 || nonceSeparator == stem.Length - 1)
            return false;

        var nonce = stem[(nonceSeparator + 1)..];
        if (nonce.Length != 32 ||
            !nonce.All(static ch => ch is >= '0' and <= '9' or >= 'a' and <= 'f'))
        {
            return false;
        }

        // Только temp от нашего writer'а может указывать на canonical state
        // leaf; прочие `.tmp` остаются foreign и проходят через fail-closed.
        var candidateDestination = stem[..nonceSeparator];
        if (!TryGetStateRefFromFileName(candidateDestination, out var candidateStateRef))
            return false;

        destinationLeafName = candidateDestination;
        stateRef = candidateStateRef;
        return true;
    }

    private OperationResult<DesignConversionHandleBoundDestinationExpectation>
        BuildPublicationDestinationExpectationNoLock(
            DesignConversionHandleBoundDirectory directory,
            string leafName,
            string expectedStateRef,
            DesignConversionHandleBoundEntry? existingEntry)
    {
        if (existingEntry is null)
            return OperationResult<DesignConversionHandleBoundDestinationExpectation>
                .Ok(DesignConversionHandleBoundDestinationExpectation.Absent);

        try
        {
            using var stream = directory.TryOpenExisting(
                leafName,
                FileShare.Read,
                writeThrough: false,
                existingEntry.Identity);
            if (stream is null)
                return TrustBoundaryFailed<
                    DesignConversionHandleBoundDestinationExpectation>();
            if (stream.Length != existingEntry.Length)
                return TrustBoundaryFailed<
                    DesignConversionHandleBoundDestinationExpectation>();

            var parsed = ReadStrictStateStreamNoLock(
                directory,
                stream,
                leafName,
                expectedStateRef,
                existingEntry.Identity);
            if (!parsed.IsSuccess)
                return parsed.MapError<
                    DesignConversionHandleBoundDestinationExpectation>();

            var length = checked((int)stream.Length);
            var bytes = new byte[length];
            stream.Position = 0L;
            stream.ReadExactly(bytes);
            if (stream.ReadByte() != -1)
                return QuotaExceeded<
                    DesignConversionHandleBoundDestinationExpectation>();

            return OperationResult<DesignConversionHandleBoundDestinationExpectation>
                .Ok(DesignConversionHandleBoundDestinationExpectation.Exact(
                    existingEntry.Identity,
                    existingEntry.Length,
                    "sha256:" +
                    Convert.ToHexString(
                            System.Security.Cryptography.SHA256.HashData(bytes))
                        .ToLowerInvariant()));
        }
        catch (Exception ex) when (
            ex is InvalidDataException or DesignConversionHandleBoundTrustException)
        {
            return TrustBoundaryFailed<
                DesignConversionHandleBoundDestinationExpectation>();
        }
        catch (Exception ex) when (IsStorageOrParseException(ex))
        {
            return ReadFailed<
                DesignConversionHandleBoundDestinationExpectation>();
        }
    }

    private static bool TryGetStateRefFromFileName(string fileName, out string stateRef)
    {
        stateRef = string.Empty;
        if (!fileName.EndsWith(".json", StringComparison.Ordinal))
            return false;
        var candidate = fileName[..^".json".Length];
        if (!IsCanonicalStateRef(candidate))
            return false;
        stateRef = candidate;
        return true;
    }

    private static bool TryGetPublicationBackupTarget(
        string backupLeafName,
        out string destinationLeafName,
        out string stateRef)
    {
        destinationLeafName = string.Empty;
        stateRef = string.Empty;
        if (!backupLeafName.EndsWith(
                DesignConversionHandleBoundDirectory.PublicationBackupSuffix,
                StringComparison.Ordinal))
        {
            return false;
        }

        var candidateDestination =
            backupLeafName[..^DesignConversionHandleBoundDirectory.PublicationBackupSuffix.Length];
        if (!TryGetStateRefFromFileName(candidateDestination, out var candidateStateRef))
            return false;

        destinationLeafName = candidateDestination;
        stateRef = candidateStateRef;
        return true;
    }

    internal static string BuildInventoryObservedSummary(
        IReadOnlyList<string> observedNames,
        string lockLeaf,
        string? ownedTempLeaf,
        bool lockObserved,
        bool ownedTempObserved)
    {
        var missing = new List<string>(capacity: 2);
        if (!lockObserved)
            missing.Add(SanitizeObservedInventoryName(lockLeaf));
        if (ownedTempLeaf is not null && !ownedTempObserved)
            missing.Add(SanitizeObservedInventoryName(ownedTempLeaf));

        var examples = observedNames
            .Take(MaxObservedInventoryExamples)
            .Select(SanitizeObservedInventoryName)
            .ToArray();
        return "Observed: count=" +
               observedNames.Count +
               "; missing=" +
               (missing.Count == 0 ? "none" : string.Join("|", missing)) +
               "; examples=" +
               (examples.Length == 0 ? "none" : string.Join("|", examples));
    }

    private static string SanitizeObservedInventoryName(string name)
    {
        var builder = new StringBuilder(
            Math.Min(name.Length, MaxObservedInventoryExampleCharacters));
        foreach (var ch in name)
        {
            if (builder.Length >= MaxObservedInventoryExampleCharacters)
            {
                builder.Append("...");
                break;
            }

            builder.Append(char.IsControl(ch) || ch == '\0'
                ? "\\u" + ((int)ch).ToString("X4")
                : ch);
        }

        return builder.ToString();
    }

    private static void TryDeleteOwnedTempNoThrow(
        DesignConversionHandleBoundDirectory directory,
        string tempLeaf)
    {
        if (string.IsNullOrWhiteSpace(tempLeaf) ||
            !tempLeaf.EndsWith(WriteTempSuffix, StringComparison.Ordinal))
        {
            return;
        }

        try
        {
            directory.DeleteIfRegular(tempLeaf);
        }
        catch (Exception ex) when (IsStorageOrParseException(ex))
        {
            // A crash/hostile replacement is retained for deterministic fail-closed recovery.
        }
    }

    private static async Task<InProcessGateLease> AcquireInProcessGateAsync(
        string key,
        CancellationToken cancellationToken)
    {
        InProcessGateEntry entry;
        lock (GateSync)
        {
            if (!InProcessGates.TryGetValue(key, out entry!))
            {
                entry = new InProcessGateEntry();
                InProcessGates.Add(key, entry);
            }

            entry.ReferenceCount++;
        }

        try
        {
            await entry.Semaphore.WaitAsync(cancellationToken);
            return new InProcessGateLease(key, entry);
        }
        catch
        {
            ReleaseGateReference(key, entry, releaseSemaphore: false);
            throw;
        }
    }

    private static void ReleaseGateReference(
        string key,
        InProcessGateEntry entry,
        bool releaseSemaphore)
    {
        if (releaseSemaphore)
            entry.Semaphore.Release();

        lock (GateSync)
        {
            entry.ReferenceCount--;
            if (entry.ReferenceCount == 0 &&
                InProcessGates.TryGetValue(key, out var current) &&
                ReferenceEquals(current, entry))
            {
                InProcessGates.Remove(key);
                entry.Semaphore.Dispose();
            }
        }
    }

    private static string GetTrainingEligibilityDenialLeaf(
        string stateRef,
        string artifactRecordId)
    {
        var artifactHash = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    Encoding.UTF8.GetBytes(artifactRecordId)))
            .ToLowerInvariant();
        return $"{stateRef}.{artifactHash}{TrainingEligibilityDenialFileSuffix}";
    }

    private static string GetTrainingEligibilityDenialReleaseWitnessLeaf(
        string stateRef,
        string artifactRecordId)
        => GetTrainingEligibilityDenialLeaf(
               stateRef,
               artifactRecordId) +
           TrainingEligibilityDenialReleaseWitnessSuffix;

    private async Task<OperationResult<TrainingEligibilityDenialEnvelope?>>
        PersistTrainingEligibilityDenialAsync(
            TrainingEligibilityDenialEnvelope marker,
            CancellationToken cancellationToken)
    {
        var lease = await AcquireTrainingEligibilityDenialLeaseAsync(
            cancellationToken);
        if (!lease.IsSuccess)
        {
            return lease.MapError<
                TrainingEligibilityDenialEnvelope?>();
        }

        await using var heldLease = lease.Value!;
        return PersistTrainingEligibilityDenialNoLock(
            heldLease.Directory,
            heldLease.CrossProcessLock,
            marker);
    }

    private async Task<OperationResult<
        IReadOnlyList<TrainingEligibilityDenialEnvelope>>>
        ReadTrainingEligibilityDenialsAsync(
            CancellationToken cancellationToken)
    {
        var lease = await AcquireTrainingEligibilityDenialLeaseAsync(
            cancellationToken);
        if (!lease.IsSuccess)
        {
            return lease.MapError<
                IReadOnlyList<TrainingEligibilityDenialEnvelope>>();
        }

        await using var heldLease = lease.Value!;
        var inventory = InspectTrainingEligibilityDenialInventoryNoLock(
            heldLease.Directory,
            heldLease.CrossProcessLock);
        return inventory.IsSuccess
            ? OperationResult<
                IReadOnlyList<TrainingEligibilityDenialEnvelope>>.Ok(
                inventory.Value!.Entries
                    .Select(entry => entry.Denial)
                    .ToArray())
            : inventory.MapError<
                IReadOnlyList<TrainingEligibilityDenialEnvelope>>();
    }

    private async Task<OperationResult<TrainingEligibilityDenialLease>>
        AcquireTrainingEligibilityDenialLeaseAsync(
            CancellationToken cancellationToken)
    {
        var denialRoot =
            _rootDirectory + TrainingEligibilityDenialDirectorySuffix;
        var gateKey = NormalizePathKey(denialRoot);
        var stopwatch = Stopwatch.StartNew();
        using var timeout =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);
        timeout.CancelAfter(_options.MutationLockTimeout);

        InProcessGateLease? inProcess = null;
        DesignConversionHandleBoundDirectory? directory = null;
        try
        {
            inProcess = await AcquireInProcessGateAsync(
                gateKey,
                timeout.Token);
            directory = DesignConversionHandleBoundDirectory.AcquireOrCreate(
                denialRoot,
                ObserveTrainingEligibilityDenialHandleIo);

            while (true)
            {
                timeout.Token.ThrowIfCancellationRequested();
                try
                {
                    var stream = directory.OpenOrCreate(
                        TrainingEligibilityDenialMutationLockFileName,
                        FileShare.None,
                        writeThrough: true);
                    if (stream.Length != 0L)
                    {
                        stream.Dispose();
                        directory.Dispose();
                        await inProcess.DisposeAsync();
                        return TrustBoundaryFailed<
                            TrainingEligibilityDenialLease>();
                    }

                    try
                    {
                        ObserveTrainingEligibilityDenialHandleIo(
                            DesignConversionHandleBoundIoStage
                                .MutationLockAcquired,
                            Path.Combine(
                                denialRoot,
                                TrainingEligibilityDenialMutationLockFileName));
                        return OperationResult<
                            TrainingEligibilityDenialLease>.Ok(
                            new TrainingEligibilityDenialLease(
                                stream,
                                directory,
                                inProcess));
                    }
                    catch
                    {
                        stream.Dispose();
                        throw;
                    }
                }
                catch (DesignConversionHandleBoundSharingViolationException)
                {
                    var remaining =
                        _options.MutationLockTimeout - stopwatch.Elapsed;
                    if (remaining <= TimeSpan.Zero)
                    {
                        throw new OperationCanceledException(
                            timeout.Token);
                    }

                    await Task.Delay(
                        remaining < _options.MutationLockRetryDelay
                            ? remaining
                            : _options.MutationLockRetryDelay,
                        timeout.Token);
                }
            }
        }
        catch (OperationCanceledException)
        {
            directory?.Dispose();
            if (inProcess is not null)
                await inProcess.DisposeAsync();
            return cancellationToken.IsCancellationRequested
                ? Cancelled<TrainingEligibilityDenialLease>()
                : LockTimeout<TrainingEligibilityDenialLease>();
        }
        catch (Exception ex) when (IsStorageOrParseException(ex))
        {
            directory?.Dispose();
            if (inProcess is not null)
                await inProcess.DisposeAsync();
            return TrustBoundaryFailed<TrainingEligibilityDenialLease>();
        }
    }

    private void ObserveTrainingEligibilityDenialHandleIo(
        DesignConversionHandleBoundIoStage stage,
        string path)
    {
        _options.BeforeTrainingEligibilityDenialHandleIoForTesting?.Invoke(
            stage,
            path);
        if (stage == DesignConversionHandleBoundIoStage.BeforeDelete)
        {
            _options.BeforeTrainingEligibilityDenialDeleteForTesting?.Invoke(
                path);
        }
        else if (
            stage ==
            DesignConversionHandleBoundIoStage
                .AfterDeleteMarkBeforeDirectoryFlush)
        {
            _options.AfterTrainingEligibilityDenialDeleteForTesting?.Invoke(
                path);
        }
    }

    private OperationResult<TrainingEligibilityDenialInventory>
        InspectTrainingEligibilityDenialInventoryNoLock(
            DesignConversionHandleBoundDirectory directory,
            FileStream crossProcessLock)
    {
        try
        {
            var entries = directory.EnumerateEntries(
                checked(_options.MaxStateFileCount + 1),
                TrainingEligibilityDenialMutationLockFileName,
                crossProcessLock);
            var markerEntries =
                new List<TrainingEligibilityDenialInventoryEntry>();
            var semanticIdentities =
                new Dictionary<
                    string,
                    TrainingEligibilityDenialInventoryEntry>(
                StringComparer.Ordinal);
            long totalBytes = 0L;

            foreach (var entry in entries)
            {
                if (string.Equals(
                        entry.Name,
                        TrainingEligibilityDenialMutationLockFileName,
                        StringComparison.Ordinal))
                {
                    if (entry.Length != 0L)
                    {
                        return TrustBoundaryFailed<
                            TrainingEligibilityDenialInventory>();
                    }

                    continue;
                }

                var isReleaseWitness = entry.Name.EndsWith(
                    TrainingEligibilityDenialReleaseWitnessSuffix,
                    StringComparison.Ordinal);
                if (!isReleaseWitness &&
                    !entry.Name.EndsWith(
                        TrainingEligibilityDenialFileSuffix,
                        StringComparison.Ordinal))
                {
                    return TrustBoundaryFailed<
                        TrainingEligibilityDenialInventory>();
                }

                if (entry.Length <= 0L ||
                    entry.Length > _options.MaxSingleStateBytes ||
                    !TryAddWithinLimit(
                        totalBytes,
                        entry.Length,
                        _options.MaxTotalStateBytes))
                {
                    return QuotaExceeded<
                        TrainingEligibilityDenialInventory>();
                }

                totalBytes += entry.Length;
                using var stream = directory.TryOpenExisting(
                    entry.Name,
                    FileShare.Read,
                    writeThrough: false,
                    entry.Identity);
                if (stream is null)
                {
                    return TrustBoundaryFailed<
                        TrainingEligibilityDenialInventory>();
                }

                var denial = ReadStrictTrainingEligibilityDenialNoLock(
                    directory,
                    stream,
                    entry);
                if (!denial.IsSuccess || denial.Value is null)
                {
                    return denial.MapError<
                        TrainingEligibilityDenialInventory>();
                }

                var semanticIdentity =
                    denial.Value.StateRef + "\0" +
                    denial.Value.ArtifactRecordId;
                var inventoryEntry =
                    new TrainingEligibilityDenialInventoryEntry(
                        entry,
                        denial.Value,
                        isReleaseWitness);
                if (semanticIdentities.TryGetValue(
                        semanticIdentity,
                        out var prior))
                {
                    if (prior.IsReleaseWitness == isReleaseWitness ||
                        !Equals(prior.Denial, denial.Value))
                    {
                        return TrustBoundaryFailed<
                            TrainingEligibilityDenialInventory>();
                    }
                }
                else
                {
                    semanticIdentities.Add(
                        semanticIdentity,
                        inventoryEntry);
                }

                markerEntries.Add(inventoryEntry);
            }

            if (markerEntries.Count > _options.MaxStateFileCount)
            {
                return QuotaExceeded<
                    TrainingEligibilityDenialInventory>();
            }

            return OperationResult<
                TrainingEligibilityDenialInventory>.Ok(
                new TrainingEligibilityDenialInventory(
                    markerEntries
                        .OrderBy(
                            entry => entry.Entry.Name,
                            StringComparer.Ordinal)
                        .ToArray(),
                    totalBytes));
        }
        catch (DesignConversionHandleBoundEnumerationLimitException)
        {
            return QuotaExceeded<TrainingEligibilityDenialInventory>();
        }
        catch (Exception ex) when (
            ex is InvalidDataException or
                DesignConversionHandleBoundTrustException)
        {
            return TrustBoundaryFailed<
                TrainingEligibilityDenialInventory>();
        }
        catch (Exception ex) when (IsStorageOrParseException(ex))
        {
            return ReadFailed<TrainingEligibilityDenialInventory>();
        }
    }

    private OperationResult<TrainingEligibilityDenialEnvelope?>
        PersistTrainingEligibilityDenialNoLock(
            DesignConversionHandleBoundDirectory directory,
            FileStream crossProcessLock,
            TrainingEligibilityDenialEnvelope marker)
    {
        var inventory = InspectTrainingEligibilityDenialInventoryNoLock(
            directory,
            crossProcessLock);
        if (!inventory.IsSuccess)
        {
            return inventory.MapError<
                TrainingEligibilityDenialEnvelope?>();
        }

        var markerLeaf = GetTrainingEligibilityDenialLeaf(
            marker.StateRef,
            marker.ArtifactRecordId);
        var existing = inventory.Value!.Entries.SingleOrDefault(entry =>
            string.Equals(
                entry.Entry.Name,
                markerLeaf,
                StringComparison.Ordinal));
        if (existing is not null)
        {
            return HasExactTrainingEligibilityDenial(
                existing.Denial,
                marker.StateRef,
                marker.ArtifactRecordId)
                ? OperationResult<
                    TrainingEligibilityDenialEnvelope?>.Ok(
                    existing.Denial)
                : TrustBoundaryFailed<
                    TrainingEligibilityDenialEnvelope?>();
        }

        var markerBytes = JsonSerializer.SerializeToUtf8Bytes(
            marker,
            JsonOptions);
        if (markerBytes.LongLength <= 0L ||
            markerBytes.LongLength > _options.MaxSingleStateBytes ||
            inventory.Value.Entries.Count >= _options.MaxStateFileCount ||
            !TryAddWithinLimit(
                inventory.Value.TotalBytes,
                markerBytes.LongLength,
                _options.MaxTotalStateBytes))
        {
            return QuotaExceeded<
                TrainingEligibilityDenialEnvelope?>();
        }

        try
        {
            using var stream = directory.CreateNew(
                markerLeaf,
                FileShare.Read,
                writeThrough: true);
            stream.Write(markerBytes);
            stream.Flush(flushToDisk: true);
            directory.FlushDirectoryMetadata();
            var identity = directory.GetIdentity(stream, markerLeaf);
            var verified = ReadStrictTrainingEligibilityDenialNoLock(
                directory,
                stream,
                new DesignConversionHandleBoundEntry(
                    markerLeaf,
                    stream.Length,
                    identity));
            return verified.IsSuccess &&
                   verified.Value is not null &&
                   HasExactTrainingEligibilityDenial(
                       verified.Value,
                       marker.StateRef,
                       marker.ArtifactRecordId)
                ? OperationResult<
                    TrainingEligibilityDenialEnvelope?>.Ok(
                    verified.Value)
                : TrustBoundaryFailed<
                    TrainingEligibilityDenialEnvelope?>();
        }
        catch (Exception ex) when (
            ex is InvalidDataException or
                DesignConversionHandleBoundTrustException)
        {
            return TrustBoundaryFailed<
                TrainingEligibilityDenialEnvelope?>();
        }
        catch (Exception ex) when (IsStorageOrParseException(ex))
        {
            return ReadFailed<
                TrainingEligibilityDenialEnvelope?>();
        }
    }

    private bool CreateTrainingEligibilityReleaseWitnessNoLock(
        DesignConversionHandleBoundDirectory directory,
        string witnessLeaf,
        TrainingEligibilityDenialEnvelope marker,
        int currentMarkerCount,
        long currentTotalBytes)
    {
        var witnessBytes = JsonSerializer.SerializeToUtf8Bytes(
            marker,
            JsonOptions);
        if (witnessBytes.LongLength <= 0L ||
            witnessBytes.LongLength > _options.MaxSingleStateBytes ||
            currentMarkerCount >= _options.MaxStateFileCount ||
            !TryAddWithinLimit(
                currentTotalBytes,
                witnessBytes.LongLength,
                _options.MaxTotalStateBytes))
        {
            return false;
        }

        try
        {
            using var witness = directory.CreateNew(
                witnessLeaf,
                FileShare.None,
                writeThrough: true);
            witness.Write(witnessBytes);
            witness.Flush(flushToDisk: true);
            directory.FlushDirectoryMetadata();
            return witness.Length == witnessBytes.LongLength;
        }
        catch (Exception ex) when (
            IsStorageOrParseException(ex) ||
            ex is ArgumentException or InvalidOperationException)
        {
            return false;
        }
    }

    private OperationResult<TrainingEligibilityDenialEnvelope>
        ReadStrictTrainingEligibilityDenialNoLock(
            DesignConversionHandleBoundDirectory directory,
            FileStream stream,
            DesignConversionHandleBoundEntry expectedEntry)
    {
        if (expectedEntry.Length <= 0L ||
            expectedEntry.Length > _options.MaxSingleStateBytes ||
            stream.Length != expectedEntry.Length)
        {
            return QuotaExceeded<
                TrainingEligibilityDenialEnvelope>();
        }

        var bytes = new byte[checked((int)expectedEntry.Length)];
        stream.Position = 0L;
        stream.ReadExactly(bytes);
        if (stream.ReadByte() != -1 ||
            bytes.Length >= 3 &&
            bytes[0] == 0xEF &&
            bytes[1] == 0xBB &&
            bytes[2] == 0xBF)
        {
            return ReadFailed<
                TrainingEligibilityDenialEnvelope>();
        }

        var identity = directory.GetIdentity(
            stream,
            expectedEntry.Name);
        if (identity != expectedEntry.Identity)
        {
            return TrustBoundaryFailed<
                TrainingEligibilityDenialEnvelope>();
        }

        try
        {
            _ = StrictUtf8.GetString(bytes);
            using var document = JsonDocument.Parse(
                bytes,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = 2
                });
            var root = document.RootElement;
            if (!HasExactUniqueProperties(
                    root,
                    TrainingEligibilityDenialPropertyNames) ||
                TrainingEligibilityDenialPropertyNames.Any(name =>
                    root.GetProperty(name).ValueKind !=
                    JsonValueKind.String))
            {
                return ReadFailed<
                    TrainingEligibilityDenialEnvelope>();
            }

            var marker = new TrainingEligibilityDenialEnvelope(
                root.GetProperty("schemaVersion").GetString()!,
                root.GetProperty("stateRef").GetString()!,
                root.GetProperty("artifactRecordId").GetString()!,
                root.GetProperty("errorCode").GetString()!);
            return string.Equals(
                       marker.SchemaVersion,
                       TrainingEligibilityDenialSchemaVersion,
                       StringComparison.Ordinal) &&
                   IsCanonicalStateRef(marker.StateRef) &&
                   !string.IsNullOrWhiteSpace(marker.ArtifactRecordId) &&
                   !string.IsNullOrWhiteSpace(marker.ErrorCode) &&
                   (string.Equals(
                        expectedEntry.Name,
                        GetTrainingEligibilityDenialLeaf(
                            marker.StateRef,
                            marker.ArtifactRecordId),
                        StringComparison.Ordinal) ||
                    string.Equals(
                        expectedEntry.Name,
                        GetTrainingEligibilityDenialReleaseWitnessLeaf(
                            marker.StateRef,
                            marker.ArtifactRecordId),
                        StringComparison.Ordinal))
                ? OperationResult<
                    TrainingEligibilityDenialEnvelope>.Ok(marker)
                : ReadFailed<
                    TrainingEligibilityDenialEnvelope>();
        }
        catch (Exception ex) when (
            ex is JsonException or DecoderFallbackException or
                InvalidOperationException)
        {
            return ReadFailed<
                TrainingEligibilityDenialEnvelope>();
        }
    }

    private static bool HasExactTrainingEligibilityDenial(
        TrainingEligibilityDenialEnvelope denial,
        string stateRef,
        string artifactRecordId)
        => string.Equals(
               denial.SchemaVersion,
               TrainingEligibilityDenialSchemaVersion,
               StringComparison.Ordinal) &&
           string.Equals(
               denial.StateRef,
               stateRef,
               StringComparison.Ordinal) &&
           string.Equals(
               denial.ArtifactRecordId,
               artifactRecordId,
               StringComparison.Ordinal);

    private static void ValidateOptions(
        DesignConversionPostEvaluationStateStoreOptions options)
    {
        if (options.MaxSingleStateBytes <= 0L)
            throw new ArgumentOutOfRangeException(nameof(options), "Maximum single state bytes must be positive.");
        if (options.MaxSingleStateBytes > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(options), "Maximum single state bytes must fit a bounded buffer.");
        if (options.MaxStateFileCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Maximum state file count must be positive.");
        if (options.MaxStateFileCount > (int.MaxValue - 2) / 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "Maximum state file count cannot be represented with required lock, write-temp, and owned-temp inventory slots.");
        }
        if (options.MaxTotalStateBytes < options.MaxSingleStateBytes)
            throw new ArgumentOutOfRangeException(nameof(options), "Maximum total bytes must cover one maximum state.");
        if (options.MutationLockTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(options), "Mutation lock timeout must be positive.");
        if (options.MutationLockRetryDelay <= TimeSpan.Zero ||
            options.MutationLockRetryDelay > options.MutationLockTimeout)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Mutation lock retry delay is outside the timeout.");
        }
    }

    private static bool TryAddWithinLimit(long left, long right, long limit)
        => left >= 0L &&
           right >= 0L &&
           left <= limit &&
           right <= limit - left;

    private static string NormalizePathKey(string path)
        => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

    private static StringComparer GetPathComparer()
        => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    private static bool IsStorageOrParseException(Exception exception)
        => exception is
            IOException or
            UnauthorizedAccessException or
            SecurityException or
            JsonException or
            DecoderFallbackException or
            NotSupportedException;

    private static OperationResult<T> Conflict<T>()
        => OperationResult<T>.Fail(
            "DesignConversion post-evaluation state conflicts with durable identity or state transitions.",
            ConflictErrorCode);

    private static OperationResult<T> ReadFailed<T>()
        => OperationResult<T>.Fail(
            "DesignConversion post-evaluation state could not be read safely.",
            ReadFailedErrorCode);

    private static OperationResult<T> WriteFailed<T>()
        => OperationResult<T>.Fail(
            "DesignConversion post-evaluation state could not be written safely.",
            WriteFailedErrorCode);

    private static OperationResult<T> TrustBoundaryFailed<T>()
        => OperationResult<T>.Fail(
            "DesignConversion post-evaluation state trust boundary validation failed.",
            TrustBoundaryErrorCode);

    private static OperationResult<T> QuotaExceeded<T>()
        => OperationResult<T>.Fail(
            "DesignConversion post-evaluation state storage quota was exceeded.",
            QuotaExceededErrorCode);

    private static OperationResult<T> LockTimeout<T>()
        => OperationResult<T>.Fail(
            "DesignConversion post-evaluation state mutation lock timed out.",
            MutationLockTimeoutErrorCode);

    private static OperationResult<T> PublicationCleanupRequired<T>()
        => OperationResult<T>.Fail(
            "DesignConversion post-evaluation state was durably published, but publication cleanup requires reconciliation.",
            PublicationCleanupRequiredErrorCode);

    private static OperationResult<DesignConversionPostEvaluationState>
        TrainingEligibilityDenyFailed()
        => OperationResult<DesignConversionPostEvaluationState>.Fail(
            "Post-evaluation training eligibility could not be denied durably.",
            TrainingEligibilityDenyFailedErrorCode);

    private static OperationResult<DesignConversionPostEvaluationState>
        TrainingEligibilityDenyReleaseFailed()
        => OperationResult<DesignConversionPostEvaluationState>.Fail(
            "Post-evaluation training eligibility denial could not be released safely.",
            TrainingEligibilityDenyReleaseFailedErrorCode);

    private static OperationResult<T> Cancelled<T>()
        => OperationResult<T>.Fail(
            "DesignConversion post-evaluation state operation was cancelled.",
            "CANCELLED");

    private sealed record StateEnvelope(
        string SchemaVersion,
        DesignConversionPostEvaluationState State);

    private sealed record TrainingEligibilityDenialEnvelope(
        string SchemaVersion,
        string StateRef,
        string ArtifactRecordId,
        string ErrorCode);

    private sealed record TrainingEligibilityDenialInventoryEntry(
        DesignConversionHandleBoundEntry Entry,
        TrainingEligibilityDenialEnvelope Denial,
        bool IsReleaseWitness);

    private sealed record TrainingEligibilityDenialInventory(
        IReadOnlyList<TrainingEligibilityDenialInventoryEntry> Entries,
        long TotalBytes);

    private sealed record LegacyPostEvaluationState(
        string StateRef,
        string ReservationRef,
        string UserRef,
        DateTimeOffset PeriodStartUtc,
        DateTimeOffset PeriodEndExclusiveUtc,
        decimal ChargedCostUsd,
        string AccountingStatus,
        string AccountingDesiredAction,
        bool SettlementAttempted,
        bool SettlementDefinitivelyNotApplied,
        bool ReleaseAttempted,
        string PersistenceStatus,
        string LedgerRecordId,
        string ArtifactRecordId,
        string LastErrorCode,
        DateTimeOffset UpdatedAtUtc,
        long Version);

    private sealed record StoreInventory(
        IReadOnlyList<DesignConversionHandleBoundEntry> StateFiles,
        IReadOnlyList<DesignConversionHandleBoundEntry> PublicationBackups,
        IReadOnlyList<StoreWriteTempEntry> WriteTemps,
        long TotalBytes);

    private sealed record StoreWriteTempEntry(
        DesignConversionHandleBoundEntry Entry,
        string DestinationLeafName,
        string StateRef);

    private sealed class InProcessGateEntry
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);

        public int ReferenceCount { get; set; }
    }

    private sealed class InProcessGateLease(
        string key,
        InProcessGateEntry entry) : IAsyncDisposable
    {
        private int _disposed;

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
                ReleaseGateReference(key, entry, releaseSemaphore: true);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class StoreLease(
        FileStream crossProcessLock,
        DesignConversionHandleBoundDirectory directory,
        InProcessGateLease inProcessGate) : IAsyncDisposable
    {
        private int _disposed;

        public DesignConversionHandleBoundDirectory Directory => directory;

        public FileStream CrossProcessLock => crossProcessLock;

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;
            await crossProcessLock.DisposeAsync();
            directory.Dispose();
            await inProcessGate.DisposeAsync();
        }
    }

    private sealed class TrainingEligibilityDenialLease(
        FileStream crossProcessLock,
        DesignConversionHandleBoundDirectory directory,
        InProcessGateLease inProcessGate) : IAsyncDisposable
    {
        private int _disposed;

        public DesignConversionHandleBoundDirectory Directory => directory;

        public FileStream CrossProcessLock => crossProcessLock;

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;
            await crossProcessLock.DisposeAsync();
            directory.Dispose();
            await inProcessGate.DisposeAsync();
        }
    }
}
