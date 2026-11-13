// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace ContinuousLearning;

using System.Buffers;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using TransformerModel;

internal sealed partial class FileDpoPairStore :
    IDpoPairExternalConsumptionTransactionCoordinator
{
    private static readonly DateTimeOffset MaximumCanonicalUtcTimestamp =
        DateTimeOffset.MaxValue;
    private static readonly object ExternalConsumptionMintingKey = new();
    private Action? _afterExternalManifestWrittenBeforeReadyForTesting = null;
    private Action? _afterExternalReadyWrittenBeforeCommitForTesting = null;
    internal const int MaximumTransactionPairCount = 1024;
    internal const int MaximumTransactionDomainCount = 256;
    internal const int MaximumRetainedTransactionCount = 10_000;
    internal const string NoParentAdapterIdentity = "none";
    internal const int MaximumReplayLedgerEntryCount =
        ServerOwnedLegalTrainingTeacherAdmissionAuthority
            .MaximumConsumedTeacherRequestIds;
    private const int MaximumTransactionSourceIdCount = 100_000;
    private const int MaximumTransactionTargetModuleCount = 256;
    private const int LegacyTransactionSchemaVersion = 3;
    private const int AtomicGroupTransactionSchemaVersion = 4;
    private const int TransactionSchemaVersion = 5;
    private const int LegacyReplayLedgerSchemaVersion = 1;
    private const int AtomicGroupReplayLedgerSchemaVersion = 2;
    private const int ReplayLedgerSchemaVersion = 3;
    private const string TransactionSuffix = ".dpo-transaction.json";
    private const string ReplayLedgerFileName = ".dpo-replay-ledger.json";
    private const string ExternalConsumptionArtifactSuffix =
        ".dpo-external-consumption.json";
    private const string ExternalConsumptionContext =
        "xiigen.dpo-external-consumption.v1";
    private const string ExternalParentProvenanceContext =
        "xiigen.training-teacher-provenance.v1";
    private const string ExternalConsumptionRecoveryRequiredErrorCode =
        "DPO_EXTERNAL_CONSUMPTION_RECOVERY_REQUIRED";
    private const string ExternalConsumptionInvalidErrorCode =
        "DPO_EXTERNAL_CONSUMPTION_INVALID";
    private const string TransactionCorruptMessage =
        "DPO pair transaction storage contains an invalid record.";
    private const long MaximumAdapterBytes = 2L * 1024L * 1024L * 1024L;
    private static readonly JsonSerializerOptions TransactionJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    internal OperationResult<TransactionHandle?> PrepareTransaction(
        IReadOnlyList<string> domains)
        => ExecuteTransactionMutation(() => PrepareTransactionCore(domains));

    private OperationResult<TransactionHandle?> PrepareTransactionCore(
        IReadOnlyList<string> domains)
    {
        var canonicalDomains = CanonicalizeDomains(domains);
        if (!canonicalDomains.IsSuccess || canonicalDomains.Value is null)
            return canonicalDomains.MapError<TransactionHandle?>();

        var pairs = LoadSnapshot(pairIdentityForCollision: null);
        if (!pairs.IsSuccess || pairs.Value is null)
            return pairs.MapError<TransactionHandle?>();
        var transactions = LoadTransactions();
        if (!transactions.IsSuccess || transactions.Value is null)
            return transactions.MapError<TransactionHandle?>();
        var replayLedger = LoadReplayLedger();
        if (!replayLedger.IsSuccess || replayLedger.Value is null)
            return replayLedger.MapError<TransactionHandle?>();
        var committed = transactions.Value.Where(static value =>
                value.State == DpoTransactionState.Committed)
            .ToArray();
        if (committed.Length != 0)
        {
            var compaction = CompactCommittedTransactions(committed);
            if (!compaction.IsSuccess)
                return compaction.MapError<TransactionHandle?>();
            transactions = LoadTransactions();
            if (!transactions.IsSuccess || transactions.Value is null)
                return transactions.MapError<TransactionHandle?>();
            replayLedger = LoadReplayLedger();
            if (!replayLedger.IsSuccess || replayLedger.Value is null)
                return replayLedger.MapError<TransactionHandle?>();
        }
        var active = transactions.Value
            .Where(static value => value.State != DpoTransactionState.Committed)
            .ToArray();
        if (active.Length > 1)
            return FailTransactionCorrupt<TransactionHandle?>();
        if (active.Length == 1)
        {
            var existing = active[0];
            if (existing.Kind != DpoTransactionKind.AdapterPublication)
                return RecoveryRequired<TransactionHandle?>();
            var validated = ValidateTransactionPairs(existing, pairs.Value);
            return validated.IsSuccess
                ? OperationResult<TransactionHandle?>.Ok(
                    new TransactionHandle(this, existing.TransactionId))
                : validated.MapError<TransactionHandle?>();
        }

        if (transactions.Value.Count >= MaximumRetainedTransactionCount)
            return FailTransactionCapacity<TransactionHandle?>();

        var retainedPairIds = transactions.Value
            .SelectMany(static value => value.Pairs)
            .Select(static value => value.PairId)
            .Concat(replayLedger.Value.Entries.SelectMany(ReplayPairIds))
            .ToHashSet(StringComparer.Ordinal);
        var domainSet = canonicalDomains.Value.ToHashSet(StringComparer.Ordinal);
        var selected = SelectAtomicTrainingUnits(
            pairs.Value,
            retainedPairIds,
            domainSet,
            _maximumTransactionPairCount);
        if (!selected.IsSuccess || selected.Value is null)
            return selected.MapError<TransactionHandle?>();
        var selectedPairs = selected.Value;
        if (selectedPairs.Length == 0)
            return OperationResult<TransactionHandle?>.Ok(null);
        if (!HasReplayLedgerEntryCapacity(
                replayLedger.Value.Entries.Length,
                selectedPairs.Count(static pair => pair.GroupCommitmentSha256.Length == 0)
                + selectedPairs.Where(static pair => pair.GroupCommitmentSha256.Length != 0)
                    .Select(static pair => pair.GroupCommitmentSha256)
                    .Distinct(StringComparer.Ordinal).Count()))
        {
            return FailReplayLedgerCapacity<TransactionHandle?>();
        }

        var readiness = ValidateTrainingReadyPairs(selectedPairs);
        if (!readiness.IsSuccess)
            return readiness.MapError<TransactionHandle?>();

        var receipt = TrainingSourceProvenance.BuildDpoPublicationReceipt(
            selectedPairs.Select(static value => value.PairId)
                .OrderBy(static value => value, StringComparer.Ordinal),
            canonicalDomains.Value);
        if (!receipt.IsSuccess || receipt.Value is null)
            return receipt.MapError<TransactionHandle?>();
        if (transactions.Value.Any(value => StringComparer.Ordinal.Equals(
                value.TransactionId,
                receipt.Value.TransactionId)))
        {
            return FailTransactionCorrupt<TransactionHandle?>();
        }

        var requestBindings = BuildRequestBindings(
            selectedPairs,
            transactions.Value,
            replayLedger.Value);
        if (!requestBindings.IsSuccess || requestBindings.Value is null)
            return requestBindings.MapError<TransactionHandle?>();
        var replayCapacity = ValidateReplayLedgerReservation(
            replayLedger.Value,
            requestBindings.Value);
        if (!replayCapacity.IsSuccess)
            return replayCapacity.MapError<TransactionHandle?>();

        var record = new DpoTransactionRecord(
            TransactionSchemaVersion,
            receipt.Value.TransactionId,
            receipt.Value.PairBatchSha256,
            DateTimeOffset.UtcNow,
            DpoTransactionState.Prepared,
            selectedPairs.Select(static value =>
                    new DpoTransactionPair(
                        value.PairId,
                        value.Domain,
                        value.GroupCommitmentSha256,
                        value.GroupOrdinal,
                        value.GroupRole))
                .ToArray(),
            canonicalDomains.Value.Select(static value =>
                    new DpoTransactionDomain(
                        value,
                        DpoTransactionDomainState.Prepared,
                        Seed: null,
                        AdapterSha256: "",
                        TrainedAt: default))
                .ToArray(),
            requestBindings.Value,
            RecordSha256: "");
        var write = WriteTransaction(record);
        return write.IsSuccess
            ? OperationResult<TransactionHandle?>.Ok(
                new TransactionHandle(this, record.TransactionId))
            : write.MapError<TransactionHandle?>();
    }

    internal OperationResult<ExternalConsumptionTransactionHandle?>
        PrepareExternalConsumptionTransaction(
            DpoPairAtomicGroupReceipt receipt,
            string domain)
        => ExecuteTransactionMutation(() =>
            PrepareExternalConsumptionTransactionCore(receipt, domain));

    OperationResult<ExternalConsumptionTransactionHandle?>
        IDpoPairExternalConsumptionTransactionCoordinator
            .PrepareExternalConsumptionTransaction(
                DpoPairAtomicGroupReceipt receipt,
                string domain)
        => PrepareExternalConsumptionTransaction(receipt, domain);

    internal OperationResult<ExternalConsumptionSnapshot>
        ResolveCommittedExternalConsumptionSnapshot(
            DpoPairAtomicGroupReceipt receipt,
            string domain,
            DpoTrainingContentBatch expectedParentContent,
            TrainingTeacherProvenance expectedParentProvenance,
            DpoExternalConsumerKind expectedConsumerKind)
    {
        var canonicalDomains = CanonicalizeDomains([domain]);
        if (_storageRoot is null
            || receipt is null
            || expectedParentContent is null
            || expectedParentProvenance is null
            || !canonicalDomains.IsSuccess
            || canonicalDomains.Value is null
            || !StringComparer.Ordinal.Equals(canonicalDomains.Value[0], domain)
            || !TrainingSourceProvenance.IsCanonicalSha256(
                receipt.GroupCommitmentSha256)
            || string.IsNullOrWhiteSpace(receipt.RequestId)
            || receipt.OrderedPairIds is null
            || receipt.OrderedPairIds.Count != 2
            || expectedParentContent.Rows.Count != 1
            || !TrainingSourceProvenance.IsCanonicalSha256(
                expectedParentContent.Sha256)
            || !TrainingSourceProvenance.IsCanonicalSha256(
                expectedParentContent.Rows[0].Sha256)
            || !IsPersistableProvenance(expectedParentProvenance)
            || !StringComparer.Ordinal.Equals(
                expectedParentProvenance.RequestId,
                receipt.RequestId)
            || expectedParentProvenance.InvocationCount != 1
            || !StringComparer.Ordinal.Equals(
                expectedParentProvenance.TrainingContentSha256,
                expectedParentContent.Rows[0].Sha256)
            || expectedConsumerKind != DpoExternalConsumerKind
                .DesignConversionDualTargetTeacherAggregate)
        {
            return InvalidExternalConsumption<ExternalConsumptionSnapshot>();
        }

        var expectedParentProvenanceSha256 =
            ComputeExternalParentProvenanceSha256(expectedParentProvenance);
        if (!TrainingSourceProvenance.IsCanonicalSha256(
                expectedParentProvenanceSha256))
        {
            return InvalidExternalConsumption<ExternalConsumptionSnapshot>();
        }

        var transactionId = ComputeExternalTransactionId(
            receipt.GroupCommitmentSha256,
            domain);
        var replay = LoadReplayLedger();
        if (!replay.IsSuccess || replay.Value is null)
            return replay.MapError<ExternalConsumptionSnapshot>();
        var replayMatches = replay.Value.Entries.Where(value =>
                value.Kind == DpoTransactionKind.ExternalConsumption
                && StringComparer.Ordinal.Equals(value.GroupId,
                    receipt.GroupCommitmentSha256)
                && StringComparer.Ordinal.Equals(value.RequestId,
                    receipt.RequestId)
                && StringComparer.Ordinal.Equals(
                    value.ExternalConsumption?.TransactionId,
                    transactionId))
            .ToArray();
        if (replayMatches.Length > 1)
            return InvalidExternalConsumption<ExternalConsumptionSnapshot>();

        if (File.Exists(TransactionPath(transactionId)))
        {
            var transaction = ReadTransaction(transactionId);
            if (!transaction.IsSuccess || transaction.Value is null)
                return transaction.MapError<ExternalConsumptionSnapshot>();
            var manifest = ReadExternalConsumptionManifest(transactionId);
            if (!manifest.IsSuccess || manifest.Value is null)
                return manifest.MapError<ExternalConsumptionSnapshot>();
            if (!ManifestMatchesTransaction(manifest.Value, transaction.Value))
                return InvalidExternalConsumption<ExternalConsumptionSnapshot>();
            var snapshot = BuildExternalSnapshot(transaction.Value);
            if (!snapshot.IsSuccess || snapshot.Value is null)
                return snapshot;
            if (replayMatches.Length == 1)
            {
                var retainedReplayEntry = replayMatches[0];
                if (!ReplayMatchesExternalReceipt(
                        retainedReplayEntry,
                        receipt,
                        domain,
                        transactionId))
                {
                    return InvalidExternalConsumption<ExternalConsumptionSnapshot>();
                }
                var retainedReplaySnapshot = BuildExternalSnapshot(
                    retainedReplayEntry);
                if (!retainedReplaySnapshot.IsSuccess ||
                    retainedReplaySnapshot.Value is null ||
                    !ExternalSnapshotsHaveExactIdentity(
                        snapshot.Value,
                        retainedReplaySnapshot.Value))
                {
                    return InvalidExternalConsumption<ExternalConsumptionSnapshot>();
                }
            }
            return CommittedExternalSnapshotMatchesExpected(
                    snapshot.Value,
                    transaction.Value.ExternalConsumption?.ParentProvenance,
                    receipt,
                    domain,
                    expectedParentContent,
                    expectedParentProvenance,
                    expectedParentProvenanceSha256,
                    expectedConsumerKind)
                ? snapshot
                : InvalidExternalConsumption<ExternalConsumptionSnapshot>();
        }

        var replayEntry = replayMatches.Length == 1 ? replayMatches[0] : null;
        if (replayEntry is null ||
            replayEntry.ExternalConsumption is null ||
            !ReplayMatchesExternalReceipt(
                replayEntry,
                receipt,
                domain,
                transactionId))
        {
            return RecoveryRequired<ExternalConsumptionSnapshot>();
        }

        var replaySnapshot = BuildExternalSnapshot(replayEntry);
        if (!replaySnapshot.IsSuccess || replaySnapshot.Value is null)
            return replaySnapshot;
        return CommittedExternalSnapshotMatchesExpected(
                replaySnapshot.Value,
                replayEntry.ExternalConsumption.ParentProvenance,
                receipt,
                domain,
                expectedParentContent,
                expectedParentProvenance,
                expectedParentProvenanceSha256,
                expectedConsumerKind)
            ? replaySnapshot
            : InvalidExternalConsumption<ExternalConsumptionSnapshot>();
    }

    private static bool ExternalSnapshotsHaveExactIdentity(
        ExternalConsumptionSnapshot left,
        ExternalConsumptionSnapshot right)
        => StringComparer.Ordinal.Equals(left.TransactionId, right.TransactionId)
           && StringComparer.Ordinal.Equals(
               left.GroupCommitmentSha256,
               right.GroupCommitmentSha256)
           && StringComparer.Ordinal.Equals(left.RequestId, right.RequestId)
           && StringComparer.Ordinal.Equals(left.Domain, right.Domain)
           && left.State == right.State
           && StringComparer.Ordinal.Equals(
               left.ParentBatchSha256,
               right.ParentBatchSha256)
           && StringComparer.Ordinal.Equals(
               left.ParentContentSha256,
               right.ParentContentSha256)
           && StringComparer.Ordinal.Equals(
               left.ParentProvenanceSha256,
               right.ParentProvenanceSha256)
           && left.OrderedPairIds.SequenceEqual(
               right.OrderedPairIds,
               StringComparer.Ordinal)
           && left.OrderedContentSha256s.SequenceEqual(
               right.OrderedContentSha256s,
               StringComparer.Ordinal)
           && left.ConsumerKind == right.ConsumerKind
           && StringComparer.Ordinal.Equals(left.ResultSha256, right.ResultSha256)
           && StringComparer.Ordinal.Equals(
               left.CheckpointSha256,
               right.CheckpointSha256)
           && StringComparer.Ordinal.Equals(left.ManifestSha256, right.ManifestSha256);

    private OperationResult<ExternalConsumptionTransactionHandle?>
        PrepareExternalConsumptionTransactionCore(
            DpoPairAtomicGroupReceipt receipt,
            string domain)
    {
        var canonicalDomains = CanonicalizeDomains([domain]);
        if (receipt is null
            || !canonicalDomains.IsSuccess
            || canonicalDomains.Value is null
            || !StringComparer.Ordinal.Equals(canonicalDomains.Value[0], domain)
            || !TrainingSourceProvenance.IsCanonicalSha256(
                receipt.GroupCommitmentSha256)
            || string.IsNullOrWhiteSpace(receipt.RequestId)
            || receipt.OrderedPairIds is null
            || receipt.OrderedPairIds.Count != 2)
        {
            return InvalidExternalConsumption<ExternalConsumptionTransactionHandle?>();
        }

        var transactionId = ComputeExternalTransactionId(
            receipt.GroupCommitmentSha256,
            domain);
        var replay = LoadReplayLedger();
        if (!replay.IsSuccess || replay.Value is null)
            return replay.MapError<ExternalConsumptionTransactionHandle?>();
        var replayEntry = replay.Value.Entries.SingleOrDefault(value =>
            StringComparer.Ordinal.Equals(
                value.GroupId,
                receipt.GroupCommitmentSha256));
        if (replayEntry is not null)
        {
            if (!ReplayMatchesExternalReceipt(
                    replayEntry,
                    receipt,
                    domain,
                    transactionId))
            {
                return InvalidExternalConsumption<
                    ExternalConsumptionTransactionHandle?>();
            }
            var retained = LoadTransactions();
            if (!retained.IsSuccess || retained.Value is null)
                return retained.MapError<ExternalConsumptionTransactionHandle?>();
            var committedExternal = retained.Value.SingleOrDefault(value =>
                StringComparer.Ordinal.Equals(value.TransactionId, transactionId));
            if (committedExternal is not null)
            {
                if (committedExternal.Kind != DpoTransactionKind.ExternalConsumption
                    || committedExternal.State != DpoTransactionState.Committed)
                {
                    return FailTransactionCorrupt<
                        ExternalConsumptionTransactionHandle?>();
                }
                var compacted = CompactCommittedTransactions([committedExternal]);
                if (!compacted.IsSuccess)
                {
                    return compacted.MapError<
                        ExternalConsumptionTransactionHandle?>();
                }
            }
            return OperationResult<ExternalConsumptionTransactionHandle?>.Ok(
                new ExternalConsumptionTransactionHandle(this, transactionId));
        }

        var snapshot = LoadSnapshot(pairIdentityForCollision: null);
        if (!snapshot.IsSuccess || snapshot.Value is null)
            return snapshot.MapError<ExternalConsumptionTransactionHandle?>();
        if (!snapshot.Value.GroupsByIdentity.TryGetValue(
                receipt.GroupCommitmentSha256,
                out var group)
            || !AtomicGroupMatchesExternalReceipt(group, receipt, domain))
        {
            return InvalidExternalConsumption<ExternalConsumptionTransactionHandle?>();
        }

        var transactions = LoadTransactions();
        if (!transactions.IsSuccess || transactions.Value is null)
            return transactions.MapError<ExternalConsumptionTransactionHandle?>();
        var committed = transactions.Value.Where(static value =>
                value.State == DpoTransactionState.Committed)
            .ToArray();
        if (committed.Length != 0)
        {
            var compacted = CompactCommittedTransactions(committed);
            if (!compacted.IsSuccess)
                return compacted.MapError<ExternalConsumptionTransactionHandle?>();
            replay = LoadReplayLedger();
            if (!replay.IsSuccess || replay.Value is null)
                return replay.MapError<ExternalConsumptionTransactionHandle?>();
            replayEntry = replay.Value.Entries.SingleOrDefault(value =>
                StringComparer.Ordinal.Equals(
                    value.GroupId,
                    receipt.GroupCommitmentSha256));
            if (replayEntry is not null)
            {
                return ReplayMatchesExternalReceipt(
                        replayEntry,
                        receipt,
                        domain,
                        transactionId)
                    ? OperationResult<ExternalConsumptionTransactionHandle?>.Ok(
                        new ExternalConsumptionTransactionHandle(this, transactionId))
                    : InvalidExternalConsumption<ExternalConsumptionTransactionHandle?>();
            }
            transactions = LoadTransactions();
            if (!transactions.IsSuccess || transactions.Value is null)
                return transactions.MapError<ExternalConsumptionTransactionHandle?>();
        }

        var active = transactions.Value.Where(static value =>
                value.State != DpoTransactionState.Committed)
            .ToArray();
        if (active.Length > 1)
            return FailTransactionCorrupt<ExternalConsumptionTransactionHandle?>();
        if (active.Length == 1)
        {
            var existing = active[0];
            if (existing.Kind != DpoTransactionKind.ExternalConsumption
                || !StringComparer.Ordinal.Equals(
                    existing.TransactionId,
                    transactionId)
                || !ValidateTransactionPairs(existing, snapshot.Value).IsSuccess)
            {
                return OperationResult<ExternalConsumptionTransactionHandle?>.Fail(
                    "Another DPO transaction is already active.",
                    "DPO_TRANSACTION_BUSY");
            }
            if (existing.ExternalConsumption is null)
                return FailTransactionCorrupt<ExternalConsumptionTransactionHandle?>();
            if (existing.ExternalConsumption.State is
                    DpoExternalConsumptionState.MutationStarted
                    or DpoExternalConsumptionState.ExternalArtifactReady)
            {
                var recovered = RecoverExternalConsumption(existing);
                if (!recovered.IsSuccess)
                    return recovered.MapError<ExternalConsumptionTransactionHandle?>();
            }
            return OperationResult<ExternalConsumptionTransactionHandle?>.Ok(
                new ExternalConsumptionTransactionHandle(this, transactionId));
        }

        if (transactions.Value.Count >= MaximumRetainedTransactionCount)
            return FailTransactionCapacity<ExternalConsumptionTransactionHandle?>();
        if (!HasReplayLedgerEntryCapacity(replay.Value.Entries.Length, 1))
            return FailReplayLedgerCapacity<ExternalConsumptionTransactionHandle?>();
        if (replay.Value.Entries.Any(value =>
                StringComparer.Ordinal.Equals(value.RequestId, group.RequestId)
                || ReplayPairIds(value).Intersect(
                    group.Children.Select(static child => child.Pair.PairId),
                    StringComparer.Ordinal).Any()))
        {
            return OperationResult<ExternalConsumptionTransactionHandle?>.Fail(
                "The exact teacher request was already consumed.",
                LegalTrainingTeacherAdmissionAuthority.DuplicateRequestIdErrorCode);
        }

        var pairs = group.Children.OrderBy(static child => child.Ordinal)
            .Select(child => new DpoTransactionPair(
                child.Pair.PairId,
                child.Pair.Domain,
                group.GroupCommitmentSha256,
                child.Ordinal,
                child.Role))
            .ToArray();
        var request = new DpoTransactionRequest(
            group.RequestId,
            group.GroupCommitmentSha256,
            PairId: string.Empty,
            DpoTransactionRequestState.Reserved,
            group.GroupCommitmentSha256,
            pairs.Select(static value => value.PairId).ToArray(),
            group.Children.OrderBy(static child => child.Ordinal)
                .Select(static child => child.ContentSha256).ToArray());
        var record = new DpoTransactionRecord(
            TransactionSchemaVersion,
            transactionId,
            group.GroupCommitmentSha256,
            DateTimeOffset.UtcNow,
            DpoTransactionState.Prepared,
            pairs,
            [new DpoTransactionDomain(
                domain,
                DpoTransactionDomainState.Prepared,
                Seed: null,
                AdapterSha256: string.Empty,
                TrainedAt: default)],
            [request],
            RecordSha256: string.Empty,
            DpoTransactionKind.ExternalConsumption,
            new DpoExternalConsumptionRecord(
                DpoExternalConsumptionState.Reserved,
                DpoExternalConsumerKind.DesignConversionDualTargetTeacherAggregate,
                group.GroupCommitmentSha256,
                group.RequestId,
                ParentBatchSha256: string.Empty,
                ParentContentSha256: string.Empty,
                ParentProvenance: null,
                ParentProvenanceSha256: string.Empty,
                ResultSha256: string.Empty,
                CheckpointSha256: string.Empty,
                ManifestSha256: string.Empty));
        var write = WriteTransaction(record);
        return write.IsSuccess
            ? OperationResult<ExternalConsumptionTransactionHandle?>.Ok(
                new ExternalConsumptionTransactionHandle(this, transactionId))
            : write.MapError<ExternalConsumptionTransactionHandle?>();
    }

    private static bool AtomicGroupMatchesExternalReceipt(
        StoredAtomicGroupRecord group,
        DpoPairAtomicGroupReceipt receipt,
        string domain)
        => StringComparer.Ordinal.Equals(
               group.GroupCommitmentSha256,
               receipt.GroupCommitmentSha256)
           && StringComparer.Ordinal.Equals(group.RequestId, receipt.RequestId)
           && group.InvocationCount == 1
           && group.Children.OrderBy(static child => child.Ordinal)
               .Select(static child => child.Pair.PairId)
               .SequenceEqual(receipt.OrderedPairIds, StringComparer.Ordinal)
           && group.Children.All(child =>
               StringComparer.Ordinal.Equals(child.Pair.Domain, domain));

    private static string ComputeExternalTransactionId(string groupId, string domain)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendIdentityField(hash, ExternalConsumptionContext);
        AppendIdentityField(hash, groupId);
        AppendIdentityField(hash, domain);
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static string ComputeExternalParentProvenanceSha256(
        TrainingTeacherProvenance? provenance)
    {
        if (provenance is null)
            return string.Empty;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendIdentityField(hash, ExternalParentProvenanceContext);
        AppendIdentityField(hash, ((int)provenance.SourceKind).ToString(
            System.Globalization.CultureInfo.InvariantCulture));
        AppendIdentityField(hash, provenance.Family is { } family
            ? ((int)family).ToString(System.Globalization.CultureInfo.InvariantCulture)
            : "null");
        AppendIdentityField(hash, provenance.AccessMode is { } accessMode
            ? ((int)accessMode).ToString(System.Globalization.CultureInfo.InvariantCulture)
            : "null");
        AppendIdentityField(hash, provenance.HostingProvider is { } provider
            ? ((int)provider).ToString(System.Globalization.CultureInfo.InvariantCulture)
            : "null");
        AppendIdentityField(hash, provenance.ModelId);
        AppendIdentityField(hash, provenance.ModelVersion);
        AppendIdentityField(hash, provenance.ModelLicense is { } license
            ? ((int)license).ToString(System.Globalization.CultureInfo.InvariantCulture)
            : "null");
        AppendIdentityField(hash, provenance.ProviderTermsChecked ? "1" : "0");
        AppendIdentityField(
            hash,
            provenance.ProviderTermsAllowTraining ? "1" : "0");
        AppendIdentityField(hash, provenance.ClosedProprietaryModel ? "1" : "0");
        AppendIdentityField(
            hash,
            provenance.TeacherOutputUsedForTraining ? "1" : "0");
        AppendIdentityField(hash, provenance.RequestId);
        AppendIdentityField(hash, provenance.InvocationCount.ToString(
            System.Globalization.CultureInfo.InvariantCulture));
        AppendIdentityField(hash, provenance.ModelArtifactDigestKind);
        AppendIdentityField(hash, provenance.ModelArtifactDigest);
        AppendIdentityField(hash, provenance.TrainingContentSha256);
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private bool ReplayMatchesExternalReceipt(
        DpoReplayLedgerEntry entry,
        DpoPairAtomicGroupReceipt receipt,
        string domain,
        string transactionId)
    {
        if (entry.Kind != DpoTransactionKind.ExternalConsumption
            || entry.ExternalConsumption is not { } external
            || !StringComparer.Ordinal.Equals(
                entry.GroupId,
                receipt.GroupCommitmentSha256)
            || !StringComparer.Ordinal.Equals(entry.RequestId, receipt.RequestId)
            || !ReplayPairIds(entry).SequenceEqual(
                receipt.OrderedPairIds,
                StringComparer.Ordinal)
            || !StringComparer.Ordinal.Equals(external.Domain, domain)
            || !StringComparer.Ordinal.Equals(external.TransactionId, transactionId))
        {
            return false;
        }
        var manifest = ReadExternalConsumptionManifest(transactionId);
        return manifest.IsSuccess
               && manifest.Value is not null
               && ManifestMatchesReplay(manifest.Value, entry);
    }

    private static bool ManifestMatchesReplay(
        DpoExternalConsumptionManifest manifest,
        DpoReplayLedgerEntry entry)
        => entry.ExternalConsumption is { } external
           && ValidateExternalConsumptionManifest(manifest)
           && StringComparer.Ordinal.Equals(
               manifest.TransactionId,
               external.TransactionId)
           && StringComparer.Ordinal.Equals(
               manifest.GroupCommitmentSha256,
               entry.GroupId)
           && StringComparer.Ordinal.Equals(manifest.RequestId, entry.RequestId)
           && StringComparer.Ordinal.Equals(manifest.Domain, external.Domain)
           && StringComparer.Ordinal.Equals(
               manifest.ParentBatchSha256,
               external.ParentBatchSha256)
           && StringComparer.Ordinal.Equals(
               manifest.ParentContentSha256,
               external.ParentContentSha256)
           && StringComparer.Ordinal.Equals(
               manifest.ParentProvenanceSha256,
               external.ParentProvenanceSha256)
           && StringComparer.Ordinal.Equals(
               external.ParentProvenanceSha256,
               ComputeExternalParentProvenanceSha256(
                   external.ParentProvenance))
           && manifest.OrderedPairIds.SequenceEqual(
               ReplayPairIds(entry),
               StringComparer.Ordinal)
           && manifest.OrderedContentSha256s.SequenceEqual(
               ReplayContentDigests(entry),
               StringComparer.Ordinal)
           && manifest.ConsumerKind == external.ConsumerKind
           && StringComparer.Ordinal.Equals(
               manifest.ResultSha256,
               external.ResultSha256)
           && StringComparer.Ordinal.Equals(
               manifest.CheckpointSha256,
               external.CheckpointSha256)
           && StringComparer.Ordinal.Equals(
               manifest.ManifestSha256,
               external.ManifestSha256);

    private static bool CommittedExternalSnapshotMatchesExpected(
        ExternalConsumptionSnapshot snapshot,
        TrainingTeacherProvenance? parentProvenance,
        DpoPairAtomicGroupReceipt receipt,
        string domain,
        DpoTrainingContentBatch expectedParentContent,
        TrainingTeacherProvenance expectedParentProvenance,
        string expectedParentProvenanceSha256,
        DpoExternalConsumerKind expectedConsumerKind)
    {
        if (snapshot.State != DpoExternalConsumptionState.Committed
            || parentProvenance is null
            || !StringComparer.Ordinal.Equals(snapshot.Domain, domain)
            || snapshot.ConsumerKind != expectedConsumerKind
            || !StringComparer.Ordinal.Equals(
                snapshot.TransactionId,
                ComputeExternalTransactionId(
                    receipt.GroupCommitmentSha256,
                    domain))
            || !StringComparer.Ordinal.Equals(
                snapshot.GroupCommitmentSha256,
                receipt.GroupCommitmentSha256)
            || !StringComparer.Ordinal.Equals(
                snapshot.RequestId,
                receipt.RequestId)
            || !StringComparer.Ordinal.Equals(
                snapshot.ParentBatchSha256,
                expectedParentContent.Sha256)
            || !StringComparer.Ordinal.Equals(
                snapshot.ParentContentSha256,
                expectedParentContent.Rows[0].Sha256)
            || !StringComparer.Ordinal.Equals(
                snapshot.ParentProvenanceSha256,
                expectedParentProvenanceSha256)
            || !ProvenanceMatches(parentProvenance, expectedParentProvenance)
            || !snapshot.OrderedPairIds.SequenceEqual(
                receipt.OrderedPairIds,
                StringComparer.Ordinal)
            || !TrainingSourceProvenance.IsCanonicalSha256(
                snapshot.ResultSha256)
            || !TrainingSourceProvenance.IsCanonicalSha256(
                snapshot.CheckpointSha256)
            || !TrainingSourceProvenance.IsCanonicalSha256(
                snapshot.ManifestSha256))
        {
            return false;
        }

        return true;
    }

    private static bool ProvenanceMatches(
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
           && left.ProviderTermsAllowTraining ==
              right.ProviderTermsAllowTraining
           && left.ClosedProprietaryModel == right.ClosedProprietaryModel
           && left.TeacherOutputUsedForTraining ==
              right.TeacherOutputUsedForTraining
           && StringComparer.Ordinal.Equals(left.RequestId, right.RequestId)
           && left.InvocationCount == right.InvocationCount
           && StringComparer.Ordinal.Equals(
               left.ModelArtifactDigestKind,
               right.ModelArtifactDigestKind)
           && StringComparer.Ordinal.Equals(
               left.ModelArtifactDigest,
               right.ModelArtifactDigest)
           && StringComparer.Ordinal.Equals(
               left.TrainingContentSha256,
               right.TrainingContentSha256);

    private OperationResult<ExternalConsumptionSnapshot> ReadExternalConsumption(
        string transactionId)
        => ExecuteTransactionMutation(() =>
        {
            if (!TrainingSourceProvenance.IsCanonicalSha256(transactionId))
                return InvalidExternalConsumption<ExternalConsumptionSnapshot>();
            if (File.Exists(TransactionPath(transactionId)))
            {
                var transaction = ReadTransaction(transactionId);
                if (!transaction.IsSuccess || transaction.Value is null)
                    return transaction.MapError<ExternalConsumptionSnapshot>();
                return BuildExternalSnapshot(transaction.Value);
            }
            var replay = LoadReplayLedger();
            if (!replay.IsSuccess || replay.Value is null)
                return replay.MapError<ExternalConsumptionSnapshot>();
            var entry = replay.Value.Entries.SingleOrDefault(value =>
                value.Kind == DpoTransactionKind.ExternalConsumption
                && StringComparer.Ordinal.Equals(
                    value.ExternalConsumption?.TransactionId,
                    transactionId));
            return entry is null || entry.ExternalConsumption is null
                ? InvalidExternalConsumption<ExternalConsumptionSnapshot>()
                : ReplayMatchesExternalReceipt(
                    entry,
                    new DpoPairAtomicGroupReceipt(
                        entry.GroupId ?? string.Empty,
                        entry.RequestId,
                        Array.AsReadOnly(ReplayPairIds(entry).ToArray())),
                    entry.ExternalConsumption.Domain,
                    transactionId)
                    ? BuildExternalSnapshot(entry)
                    : RecoveryRequired<ExternalConsumptionSnapshot>();
        });

    private static OperationResult<ExternalConsumptionSnapshot> BuildExternalSnapshot(
        DpoTransactionRecord transaction)
    {
        if (transaction.Kind != DpoTransactionKind.ExternalConsumption
            || transaction.ExternalConsumption is not { } external)
        {
            return InvalidExternalConsumption<ExternalConsumptionSnapshot>();
        }
        var request = transaction.Requests.Single();
        return OperationResult<ExternalConsumptionSnapshot>.Ok(new(
            transaction.TransactionId,
            external.GroupCommitmentSha256,
            external.RequestId,
            transaction.Domains.Single().Domain,
            external.State,
            external.ParentBatchSha256,
            external.ParentContentSha256,
            external.ParentProvenanceSha256,
            Array.AsReadOnly(RequestPairIds(request).ToArray()),
            Array.AsReadOnly(RequestContentDigests(request).ToArray()),
            external.ConsumerKind,
            external.ResultSha256,
            external.CheckpointSha256,
            external.ManifestSha256));
    }

    private static OperationResult<ExternalConsumptionSnapshot> BuildExternalSnapshot(
        DpoReplayLedgerEntry entry)
    {
        if (entry.Kind != DpoTransactionKind.ExternalConsumption
            || entry.ExternalConsumption is not { } external)
        {
            return InvalidExternalConsumption<ExternalConsumptionSnapshot>();
        }
        return OperationResult<ExternalConsumptionSnapshot>.Ok(new(
            external.TransactionId,
            entry.GroupId ?? string.Empty,
            entry.RequestId,
            external.Domain,
            DpoExternalConsumptionState.Committed,
            external.ParentBatchSha256,
            external.ParentContentSha256,
            external.ParentProvenanceSha256,
            Array.AsReadOnly(ReplayPairIds(entry).ToArray()),
            Array.AsReadOnly(ReplayContentDigests(entry).ToArray()),
            external.ConsumerKind,
            external.ResultSha256,
            external.CheckpointSha256,
            external.ManifestSha256));
    }

    private OperationResult<bool> BindExternalAggregate(
        string transactionId,
        DpoTrainingContentBatch trainingContent,
        TrainingTeacherProvenance provenance)
        => ExecuteTransactionMutation(() =>
        {
            if (trainingContent is null
                || provenance is null
                || trainingContent.Rows.Count != 1
                || !TrainingSourceProvenance.IsCanonicalSha256(trainingContent.Sha256)
                || !TrainingSourceProvenance.IsCanonicalSha256(
                    trainingContent.Rows[0].Sha256))
            {
                return InvalidExternalConsumption<bool>();
            }
            var transaction = ReadTransaction(transactionId);
            if (!transaction.IsSuccess || transaction.Value is null)
                return transaction.MapError<bool>();
            var record = transaction.Value;
            if (record.Kind != DpoTransactionKind.ExternalConsumption
                || record.ExternalConsumption is not { } external
                || !IsPersistableProvenance(provenance)
                || !StringComparer.Ordinal.Equals(
                    provenance.RequestId,
                    external.RequestId)
                || provenance.InvocationCount != 1
                || !StringComparer.Ordinal.Equals(
                    provenance.TrainingContentSha256,
                    trainingContent.Rows[0].Sha256))
            {
                return InvalidExternalConsumption<bool>();
            }
            var provenanceSha256 =
                ComputeExternalParentProvenanceSha256(provenance);
            if (!TrainingSourceProvenance.IsCanonicalSha256(provenanceSha256))
                return InvalidExternalConsumption<bool>();
            if (external.State == DpoExternalConsumptionState.AggregateBound)
            {
                return StringComparer.Ordinal.Equals(
                           external.ParentBatchSha256,
                           trainingContent.Sha256)
                       && StringComparer.Ordinal.Equals(
                           external.ParentContentSha256,
                           trainingContent.Rows[0].Sha256)
                       && Equals(external.ParentProvenance, provenance)
                       && StringComparer.Ordinal.Equals(
                           external.ParentProvenanceSha256,
                           provenanceSha256)
                    ? OperationResult<bool>.Ok(true)
                    : InvalidExternalConsumption<bool>();
            }
            if (external.State != DpoExternalConsumptionState.Reserved)
                return RecoveryRequired<bool>();
            return WriteTransaction(record with
            {
                State = DpoTransactionState.Active,
                ExternalConsumption = external with
                {
                    State = DpoExternalConsumptionState.AggregateBound,
                    ParentBatchSha256 = trainingContent.Sha256,
                    ParentContentSha256 = trainingContent.Rows[0].Sha256,
                    ParentProvenance = provenance,
                    ParentProvenanceSha256 = provenanceSha256,
                },
            });
        });

    private OperationResult<ExternalMutationAuthorization> BeginExternalMutation(
        string transactionId)
        => ExecuteTransactionMutation(() =>
        {
            var transaction = ReadTransaction(transactionId);
            if (!transaction.IsSuccess || transaction.Value is null)
                return transaction.MapError<ExternalMutationAuthorization>();
            var record = transaction.Value;
            if (record.Kind != DpoTransactionKind.ExternalConsumption
                || record.ExternalConsumption is not { } external)
            {
                return InvalidExternalConsumption<ExternalMutationAuthorization>();
            }
            if (external.State != DpoExternalConsumptionState.AggregateBound
                || record.Requests.Single().State !=
                    DpoTransactionRequestState.Reserved)
            {
                return RecoveryRequired<ExternalMutationAuthorization>();
            }
            var write = WriteTransaction(record with
            {
                State = DpoTransactionState.Active,
                Requests = [record.Requests[0] with
                {
                    State = DpoTransactionRequestState.Consumed,
                }],
                ExternalConsumption = external with
                {
                    State = DpoExternalConsumptionState.MutationStarted,
                },
            });
            return write.IsSuccess
                ? OperationResult<ExternalMutationAuthorization>.Ok(
                    new ExternalMutationAuthorization(
                        this,
                        transactionId,
                        ExternalConsumptionMintingKey))
                : write.MapError<ExternalMutationAuthorization>();
        });

    private OperationResult<ExternalConsumptionCompletionReceipt>
        CompleteExternalConsumption(
            string transactionId,
            ExternalMutationAuthorization authorization,
            DpoExternalConsumptionResult result)
        => ExecuteTransactionMutation(() =>
        {
            if (authorization is null
                || result is null
                || !Enum.IsDefined(result.ConsumerKind)
                || result.ConsumerKind !=
                    DpoExternalConsumerKind.DesignConversionDualTargetTeacherAggregate
                || !TrainingSourceProvenance.IsCanonicalSha256(result.ResultSha256)
                || !TrainingSourceProvenance.IsCanonicalSha256(
                    result.CheckpointSha256))
            {
                return InvalidExternalConsumption<
                    ExternalConsumptionCompletionReceipt>();
            }
            var transaction = ReadTransaction(transactionId);
            if (!transaction.IsSuccess || transaction.Value is null)
                return transaction.MapError<ExternalConsumptionCompletionReceipt>();
            var record = transaction.Value;
            if (record.Kind != DpoTransactionKind.ExternalConsumption
                || record.ExternalConsumption is not { } external
                || external.State != DpoExternalConsumptionState.MutationStarted
                || external.ConsumerKind != result.ConsumerKind)
            {
                return RecoveryRequired<ExternalConsumptionCompletionReceipt>();
            }
            if (!authorization.TryClaim(this, transactionId))
                return InvalidExternalConsumption<
                    ExternalConsumptionCompletionReceipt>();

            var request = record.Requests.Single();
            var manifest = new DpoExternalConsumptionManifest(
                SchemaVersion: 1,
                ExternalConsumptionContext,
                transactionId,
                external.GroupCommitmentSha256,
                external.RequestId,
                record.Domains.Single().Domain,
                external.ParentBatchSha256,
                external.ParentContentSha256,
                external.ParentProvenanceSha256,
                RequestPairIds(request).ToArray(),
                RequestContentDigests(request).ToArray(),
                result.ConsumerKind,
                result.ResultSha256,
                result.CheckpointSha256,
                ManifestSha256: string.Empty);
            var written = WriteExternalConsumptionManifest(manifest);
            if (!written.IsSuccess || written.Value is null)
                return written.MapError<ExternalConsumptionCompletionReceipt>();
            var manifestBoundary = InvokeExternalBoundaryForTesting(
                _afterExternalManifestWrittenBeforeReadyForTesting);
            if (!manifestBoundary.IsSuccess)
            {
                return manifestBoundary.MapError<
                    ExternalConsumptionCompletionReceipt>();
            }

            var ready = record with
            {
                State = DpoTransactionState.ExternalConsumptionComplete,
                ExternalConsumption = external with
                {
                    State = DpoExternalConsumptionState.ExternalArtifactReady,
                    ResultSha256 = result.ResultSha256,
                    CheckpointSha256 = result.CheckpointSha256,
                    ManifestSha256 = written.Value.ManifestSha256,
                },
            };
            var readyWrite = WriteTransaction(ready);
            if (!readyWrite.IsSuccess)
                return readyWrite.MapError<ExternalConsumptionCompletionReceipt>();
            var readyBoundary = InvokeExternalBoundaryForTesting(
                _afterExternalReadyWrittenBeforeCommitForTesting);
            if (!readyBoundary.IsSuccess)
            {
                return readyBoundary.MapError<
                    ExternalConsumptionCompletionReceipt>();
            }
            var committed = CommitExternalConsumption(ready, written.Value);
            return committed.IsSuccess
                ? OperationResult<ExternalConsumptionCompletionReceipt>.Ok(
                    new ExternalConsumptionCompletionReceipt(
                        written.Value,
                        ExternalConsumptionMintingKey))
                : committed.MapError<ExternalConsumptionCompletionReceipt>();
        });

    private static OperationResult<bool> InvokeExternalBoundaryForTesting(
        Action? boundary)
    {
        try
        {
            boundary?.Invoke();
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FailUnavailable<bool>();
        }
    }

    private OperationResult<bool> RecoverExternalConsumption(
        DpoTransactionRecord record)
    {
        if (record.ExternalConsumption is not { } external)
            return FailTransactionCorrupt<bool>();
        if (external.State == DpoExternalConsumptionState.MutationStarted)
        {
            var manifest = ReadExternalConsumptionManifest(record.TransactionId);
            if (!manifest.IsSuccess || manifest.Value is null
                || !ManifestMatchesTransaction(manifest.Value, record))
            {
                return RecoveryRequired<bool>();
            }
            record = record with
            {
                State = DpoTransactionState.ExternalConsumptionComplete,
                ExternalConsumption = external with
                {
                    State = DpoExternalConsumptionState.ExternalArtifactReady,
                    ResultSha256 = manifest.Value.ResultSha256,
                    CheckpointSha256 = manifest.Value.CheckpointSha256,
                    ManifestSha256 = manifest.Value.ManifestSha256,
                },
            };
            var readyWrite = WriteTransaction(record);
            if (!readyWrite.IsSuccess)
                return readyWrite;
        }
        if (record.ExternalConsumption?.State ==
            DpoExternalConsumptionState.ExternalArtifactReady)
        {
            var manifest = ReadExternalConsumptionManifest(record.TransactionId);
            if (!manifest.IsSuccess || manifest.Value is null
                || !ManifestMatchesTransaction(manifest.Value, record))
            {
                return RecoveryRequired<bool>();
            }
            return CommitExternalConsumption(record, manifest.Value);
        }
        return record.ExternalConsumption?.State ==
               DpoExternalConsumptionState.Committed
            ? CompactCommittedTransactions([record])
            : OperationResult<bool>.Ok(true);
    }

    private OperationResult<bool> CommitExternalConsumption(
        DpoTransactionRecord record,
        DpoExternalConsumptionManifest manifest)
    {
        if (!ManifestMatchesTransaction(manifest, record)
            || record.ExternalConsumption is not { } external
            || external.State != DpoExternalConsumptionState.ExternalArtifactReady)
        {
            return RecoveryRequired<bool>();
        }
        var committed = record with
        {
            State = DpoTransactionState.Committed,
            ExternalConsumption = external with
            {
                State = DpoExternalConsumptionState.Committed,
            },
        };
        var write = WriteTransaction(committed);
        if (!write.IsSuccess)
            return write;
        var readback = ReadTransaction(record.TransactionId);
        if (!readback.IsSuccess
            || readback.Value?.ExternalConsumption?.State !=
                DpoExternalConsumptionState.Committed)
        {
            return FailTransactionCorrupt<bool>();
        }
        return CompactCommittedTransactions([committed]);
    }

    private OperationResult<bool> ReleaseExternalIfUnmutated(string transactionId)
        => ExecuteTransactionMutation(() =>
        {
            if (!File.Exists(TransactionPath(transactionId)))
                return OperationResult<bool>.Ok(false);
            var transaction = ReadTransaction(transactionId);
            if (!transaction.IsSuccess || transaction.Value is null)
                return transaction.MapError<bool>();
            var external = transaction.Value.ExternalConsumption;
            if (transaction.Value.Kind != DpoTransactionKind.ExternalConsumption
                || external is null)
            {
                return InvalidExternalConsumption<bool>();
            }
            if (external.State is not (
                    DpoExternalConsumptionState.Reserved
                    or DpoExternalConsumptionState.AggregateBound)
                || transaction.Value.Requests.Any(static request =>
                    request.State != DpoTransactionRequestState.Reserved))
            {
                return OperationResult<bool>.Ok(false);
            }
            try
            {
                File.Delete(TransactionPath(transactionId));
                return OperationResult<bool>.Ok(true);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return FailUnavailable<bool>();
            }
        });

    private OperationResult<DpoExternalConsumptionManifest>
        WriteExternalConsumptionManifest(DpoExternalConsumptionManifest manifest)
    {
        var hash = ComputeExternalConsumptionManifestSha256(manifest);
        if (!hash.IsSuccess || hash.Value is null)
            return hash.MapError<DpoExternalConsumptionManifest>();
        var canonical = manifest with { ManifestSha256 = hash.Value };
        if (!ValidateExternalConsumptionManifest(canonical))
            return InvalidExternalConsumption<DpoExternalConsumptionManifest>();
        var path = ExternalConsumptionManifestPath(canonical.TransactionId);
        if (File.Exists(path))
        {
            var existing = ReadExternalConsumptionManifest(canonical.TransactionId);
            return existing.IsSuccess
                   && existing.Value is not null
                   && ExternalConsumptionManifestsEqual(existing.Value, canonical)
                ? OperationResult<DpoExternalConsumptionManifest>.Ok(existing.Value)
                : RecoveryRequired<DpoExternalConsumptionManifest>();
        }
        var temporary = Path.Combine(
            _storageRoot!,
            $".{canonical.TransactionId}.{Guid.NewGuid():N}.external.tmp");
        try
        {
            using (var stream = OpenPrivateFile(
                       temporary,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       FileOptions.WriteThrough))
            {
                using var bounded = new BoundedTransactionWriteStream(
                    stream,
                    hash: null,
                    MaximumRecordBytes);
                using var writer = new Utf8JsonWriter(bounded);
                WriteExternalConsumptionManifestJson(writer, canonical);
                writer.Flush();
                stream.Flush(flushToDisk: true);
                if (!OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, temporary)
                    || !HasSafeUnixFileMode(temporary))
                {
                    throw new IOException(
                        "External-consumption manifest staging path changed.");
                }
            }
            File.Move(temporary, path);
            var readback = ReadExternalConsumptionManifest(canonical.TransactionId);
            return readback.IsSuccess
                   && readback.Value is not null
                   && ExternalConsumptionManifestsEqual(readback.Value, canonical)
                ? readback
                : RecoveryRequired<DpoExternalConsumptionManifest>();
        }
        catch (IOException) when (File.Exists(path))
        {
            DeleteTemporaryFile(temporary);
            var existing = ReadExternalConsumptionManifest(canonical.TransactionId);
            return existing.IsSuccess
                   && existing.Value is not null
                   && ExternalConsumptionManifestsEqual(existing.Value, canonical)
                ? existing
                : RecoveryRequired<DpoExternalConsumptionManifest>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            DeleteTemporaryFile(temporary);
            return FailUnavailable<DpoExternalConsumptionManifest>();
        }
    }

    private OperationResult<DpoExternalConsumptionManifest>
        ReadExternalConsumptionManifest(string transactionId)
    {
        var path = ExternalConsumptionManifestPath(transactionId);
        if (!File.Exists(path))
            return RecoveryRequired<DpoExternalConsumptionManifest>();
        try
        {
            var fullPath = Path.GetFullPath(path);
            if (_storageRoot is null
                || !PathsEqual(Path.GetDirectoryName(fullPath)!, _storageRoot)
                || AnyExistingPathSegmentIsReparsePoint(fullPath)
                || !HasSafeUnixFileMode(fullPath))
            {
                return RecoveryRequired<DpoExternalConsumptionManifest>();
            }
            using var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.SequentialScan);
            if (!OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, fullPath))
                return RecoveryRequired<DpoExternalConsumptionManifest>();
            var bytes = ReadBoundedStableBytes(stream);
            if (!bytes.IsSuccess
                || bytes.Value is null
                || !HasBoundedTransactionJsonTokens(bytes.Value))
            {
                return RecoveryRequired<DpoExternalConsumptionManifest>();
            }
            using (var document = JsonDocument.Parse(bytes.Value))
            {
                if (!HasNoDuplicateProperties(document.RootElement))
                    return RecoveryRequired<DpoExternalConsumptionManifest>();
            }
            var manifest = JsonSerializer.Deserialize<DpoExternalConsumptionManifest>(
                bytes.Value,
                TransactionJsonOptions);
            if (manifest is null
                || !ValidateExternalConsumptionManifest(manifest)
                || !StringComparer.Ordinal.Equals(
                    Path.GetFileName(fullPath),
                    manifest.TransactionId + ExternalConsumptionArtifactSuffix))
            {
                return RecoveryRequired<DpoExternalConsumptionManifest>();
            }
            var hash = ComputeExternalConsumptionManifestSha256(manifest);
            return hash.IsSuccess
                   && StringComparer.Ordinal.Equals(
                       hash.Value,
                       manifest.ManifestSha256)
                ? OperationResult<DpoExternalConsumptionManifest>.Ok(manifest)
                : RecoveryRequired<DpoExternalConsumptionManifest>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return RecoveryRequired<DpoExternalConsumptionManifest>();
        }
    }

    private static OperationResult<string>
        ComputeExternalConsumptionManifestSha256(
            DpoExternalConsumptionManifest manifest)
    {
        if (!ValidateExternalConsumptionManifest(
                manifest with { ManifestSha256 = string.Empty }))
        {
            return InvalidExternalConsumption<string>();
        }
        try
        {
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            using var bounded = new BoundedTransactionWriteStream(
                destination: null,
                hash,
                MaximumRecordBytes);
            using var writer = new Utf8JsonWriter(bounded);
            WriteExternalConsumptionManifestJson(
                writer,
                manifest with { ManifestSha256 = string.Empty });
            writer.Flush();
            return OperationResult<string>.Ok(
                Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidExternalConsumption<string>();
        }
    }

    private static void WriteExternalConsumptionManifestJson(
        Utf8JsonWriter writer,
        DpoExternalConsumptionManifest manifest)
    {
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", manifest.SchemaVersion);
        writer.WriteString("context", manifest.Context);
        writer.WriteString("transactionId", manifest.TransactionId);
        writer.WriteString("groupCommitmentSha256", manifest.GroupCommitmentSha256);
        writer.WriteString("requestId", manifest.RequestId);
        writer.WriteString("domain", manifest.Domain);
        writer.WriteString("parentBatchSha256", manifest.ParentBatchSha256);
        writer.WriteString("parentContentSha256", manifest.ParentContentSha256);
        writer.WriteString(
            "parentProvenanceSha256",
            manifest.ParentProvenanceSha256);
        writer.WriteStartArray("orderedPairIds");
        foreach (var pairId in manifest.OrderedPairIds)
            writer.WriteStringValue(pairId);
        writer.WriteEndArray();
        writer.WriteStartArray("orderedContentSha256s");
        foreach (var digest in manifest.OrderedContentSha256s)
            writer.WriteStringValue(digest);
        writer.WriteEndArray();
        writer.WriteNumber("consumerKind", (int)manifest.ConsumerKind);
        writer.WriteString("resultSha256", manifest.ResultSha256);
        writer.WriteString("checkpointSha256", manifest.CheckpointSha256);
        writer.WriteString("manifestSha256", manifest.ManifestSha256);
        writer.WriteEndObject();
    }

    private static bool ValidateExternalConsumptionManifest(
        DpoExternalConsumptionManifest manifest)
        => manifest is not null
           && manifest.SchemaVersion == 1
           && StringComparer.Ordinal.Equals(
               manifest.Context,
               ExternalConsumptionContext)
           && TrainingSourceProvenance.IsCanonicalSha256(manifest.TransactionId)
           && TrainingSourceProvenance.IsCanonicalSha256(
               manifest.GroupCommitmentSha256)
           && !string.IsNullOrWhiteSpace(manifest.RequestId)
           && !string.IsNullOrWhiteSpace(manifest.Domain)
           && TrainingSourceProvenance.IsCanonicalSha256(
               manifest.ParentBatchSha256)
           && TrainingSourceProvenance.IsCanonicalSha256(
               manifest.ParentContentSha256)
           && TrainingSourceProvenance.IsCanonicalSha256(
               manifest.ParentProvenanceSha256)
           && manifest.OrderedPairIds is { Length: 2 }
           && manifest.OrderedContentSha256s is { Length: 2 }
           && manifest.OrderedPairIds.All(
               TrainingSourceProvenance.IsCanonicalSha256)
           && manifest.OrderedContentSha256s.All(
               TrainingSourceProvenance.IsCanonicalSha256)
           && manifest.OrderedPairIds.Distinct(StringComparer.Ordinal).Count() == 2
           && Enum.IsDefined(manifest.ConsumerKind)
           && manifest.ConsumerKind ==
               DpoExternalConsumerKind.DesignConversionDualTargetTeacherAggregate
           && TrainingSourceProvenance.IsCanonicalSha256(manifest.ResultSha256)
           && TrainingSourceProvenance.IsCanonicalSha256(
               manifest.CheckpointSha256)
           && (manifest.ManifestSha256.Length == 0
               || TrainingSourceProvenance.IsCanonicalSha256(
                   manifest.ManifestSha256))
           && StringComparer.Ordinal.Equals(
               manifest.TransactionId,
               ComputeExternalTransactionId(
                   manifest.GroupCommitmentSha256,
                   manifest.Domain));

    private static bool ManifestMatchesTransaction(
        DpoExternalConsumptionManifest manifest,
        DpoTransactionRecord transaction)
    {
        if (!ValidateExternalConsumptionManifest(manifest)
            || transaction.ExternalConsumption is not { } external
            || !StringComparer.Ordinal.Equals(
                manifest.TransactionId,
                transaction.TransactionId)
            || !StringComparer.Ordinal.Equals(
                manifest.GroupCommitmentSha256,
                external.GroupCommitmentSha256)
            || !StringComparer.Ordinal.Equals(manifest.RequestId, external.RequestId)
            || !StringComparer.Ordinal.Equals(
                manifest.Domain,
                transaction.Domains.Single().Domain)
            || !StringComparer.Ordinal.Equals(
                manifest.ParentBatchSha256,
                external.ParentBatchSha256)
            || !StringComparer.Ordinal.Equals(
                manifest.ParentContentSha256,
                external.ParentContentSha256)
            || !StringComparer.Ordinal.Equals(
                manifest.ParentProvenanceSha256,
                external.ParentProvenanceSha256)
            || !StringComparer.Ordinal.Equals(
                external.ParentProvenanceSha256,
                ComputeExternalParentProvenanceSha256(
                    external.ParentProvenance))
            || !manifest.OrderedPairIds.SequenceEqual(
                RequestPairIds(transaction.Requests.Single()),
                StringComparer.Ordinal)
            || !manifest.OrderedContentSha256s.SequenceEqual(
                RequestContentDigests(transaction.Requests.Single()),
                StringComparer.Ordinal)
            || manifest.ConsumerKind != external.ConsumerKind)
        {
            return false;
        }
        return external.State == DpoExternalConsumptionState.MutationStarted
               || StringComparer.Ordinal.Equals(
                   manifest.ResultSha256,
                   external.ResultSha256)
               && StringComparer.Ordinal.Equals(
                   manifest.CheckpointSha256,
                   external.CheckpointSha256)
               && StringComparer.Ordinal.Equals(
                   manifest.ManifestSha256,
                   external.ManifestSha256);
    }

    private static bool ExternalConsumptionManifestsEqual(
        DpoExternalConsumptionManifest left,
        DpoExternalConsumptionManifest right)
        => left == right
           || left.SchemaVersion == right.SchemaVersion
           && StringComparer.Ordinal.Equals(left.Context, right.Context)
           && StringComparer.Ordinal.Equals(left.TransactionId, right.TransactionId)
           && StringComparer.Ordinal.Equals(
               left.GroupCommitmentSha256,
               right.GroupCommitmentSha256)
           && StringComparer.Ordinal.Equals(left.RequestId, right.RequestId)
           && StringComparer.Ordinal.Equals(left.Domain, right.Domain)
           && StringComparer.Ordinal.Equals(
               left.ParentBatchSha256,
               right.ParentBatchSha256)
           && StringComparer.Ordinal.Equals(
               left.ParentContentSha256,
               right.ParentContentSha256)
           && StringComparer.Ordinal.Equals(
               left.ParentProvenanceSha256,
               right.ParentProvenanceSha256)
           && left.OrderedPairIds.SequenceEqual(
               right.OrderedPairIds,
               StringComparer.Ordinal)
           && left.OrderedContentSha256s.SequenceEqual(
               right.OrderedContentSha256s,
               StringComparer.Ordinal)
           && left.ConsumerKind == right.ConsumerKind
           && StringComparer.Ordinal.Equals(left.ResultSha256, right.ResultSha256)
           && StringComparer.Ordinal.Equals(
               left.CheckpointSha256,
               right.CheckpointSha256)
           && StringComparer.Ordinal.Equals(
               left.ManifestSha256,
               right.ManifestSha256);

    private string ExternalConsumptionManifestPath(string transactionId)
        => Path.Combine(
            _storageRoot!,
            transactionId + ExternalConsumptionArtifactSuffix);

    private OperationResult<IReadOnlyList<string>> CanonicalizeDomains(
        IReadOnlyList<string> domains)
    {
        if (domains is null
            || domains.Count == 0
            || domains.Count > MaximumTransactionDomainCount
            || domains.Any(static value =>
                string.IsNullOrWhiteSpace(value)
                || value.Length > 256
                || !StringComparer.Ordinal.Equals(value, value.Trim())
                || value.Any(char.IsControl)))
        {
            return OperationResult<IReadOnlyList<string>>.Fail(
                "The DPO transaction domain batch is invalid.",
                "DPO_TRANSACTION_INPUT_INVALID");
        }

        var ordered = domains
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
        if (ordered.Length != domains.Count)
        {
            return OperationResult<IReadOnlyList<string>>.Fail(
                "The DPO transaction domain batch contains duplicates.",
                "DPO_TRANSACTION_INPUT_INVALID");
        }
        return OperationResult<IReadOnlyList<string>>.Ok(
            Array.AsReadOnly(ordered));
    }

    private static OperationResult<StoredDpoPairRecord[]> SelectAtomicTrainingUnits(
        StoreSnapshot snapshot,
        IReadOnlySet<string> retainedPairIds,
        IReadOnlySet<string> domains,
        int maximumPairs)
    {
        var candidates = snapshot.RecordsByIdentity.Values
            .Where(value => !retainedPairIds.Contains(value.PairId)
                            && domains.Contains(value.Domain))
            .GroupBy(static value => value.GroupCommitmentSha256.Length == 0
                ? value.PairId
                : value.GroupCommitmentSha256,
                StringComparer.Ordinal)
            .OrderBy(static value => value.Key, StringComparer.Ordinal);
        var selected = new List<StoredDpoPairRecord>(maximumPairs);
        foreach (var candidate in candidates)
        {
            var unit = candidate.First().GroupCommitmentSha256.Length == 0
                ? candidate.ToArray()
                : candidate.OrderBy(static value => value.GroupOrdinal).ToArray();
            if (unit.Length == 0)
                continue;
            if (unit[0].GroupCommitmentSha256.Length != 0
                && (unit.Length != 2
                    || unit[0].GroupOrdinal != 0
                    || unit[0].GroupRole != DpoPairAtomicGroupRole.CriticUnit
                    || unit[1].GroupOrdinal != 1
                    || unit[1].GroupRole != DpoPairAtomicGroupRole.ImproverUnit))
            {
                return FailTransactionCorrupt<StoredDpoPairRecord[]>();
            }
            if (selected.Count > maximumPairs - unit.Length)
                continue;
            selected.AddRange(unit);
        }
        return OperationResult<StoredDpoPairRecord[]>.Ok(selected.ToArray());
    }

    private static OperationResult<DpoTransactionRequest[]> BuildRequestBindings(
        IReadOnlyList<StoredDpoPairRecord> selected,
        IReadOnlyList<DpoTransactionRecord> retainedTransactions,
        DpoReplayLedgerRecord replayLedger)
    {
        var historical = new HashSet<string>(StringComparer.Ordinal);
        foreach (var transaction in retainedTransactions)
        {
            foreach (var request in transaction.Requests)
            {
                if (!historical.Add(request.RequestId))
                    return FailTransactionCorrupt<DpoTransactionRequest[]>();
            }
        }
        foreach (var replay in replayLedger.Entries)
        {
            if (!historical.Add(replay.RequestId))
                return FailTransactionCorrupt<DpoTransactionRequest[]>();
        }

        var current = new List<DpoTransactionRequest>();
        for (var index = 0; index < selected.Count; index++)
        {
            var pair = selected[index];
            var groupId = pair.GroupCommitmentSha256;
            var unit = groupId.Length == 0
                ? new[] { pair }
                : selected.Skip(index).Take(2).ToArray();
            if (groupId.Length != 0)
                index++;
            if (unit.Any(static value => value.TeacherProvenance is null))
            {
                return OperationResult<DpoTransactionRequest[]>.Fail(
                    "Typed teacher provenance is required before a DPO pair can enter a training transaction.",
                    LegalTrainingTeacherAdmissionAuthority
                        .ProvenanceRequiredErrorCode);
            }
            var provenance = unit[0].TeacherProvenance!;
            if (historical.Contains(provenance.RequestId)
                || current.Any(value => StringComparer.Ordinal.Equals(
                    value.RequestId,
                    provenance.RequestId))
                || groupId.Length != 0
                    && (unit.Length != 2
                        || unit.Any(value => !StringComparer.Ordinal.Equals(
                            value.GroupCommitmentSha256,
                            groupId))
                        || unit.Any(value => !SameTeacherInvocation(
                            provenance,
                            value.TeacherProvenance!))))
            {
                return OperationResult<DpoTransactionRequest[]>.Fail(
                    "A teacher request id has already been retained by a DPO transaction.",
                    LegalTrainingTeacherAdmissionAuthority.DuplicateRequestIdErrorCode);
            }
            var projected = DpoPairTrainingContentAuthority.Project(
                Array.AsReadOnly(unit.Select(ToPair).ToArray()));
            if (!projected.IsSuccess
                || projected.Value is null
                || projected.Value.Rows.Count != unit.Length
                || unit.Where((value, childIndex) => !StringComparer.Ordinal.Equals(
                        projected.Value.Rows[childIndex].Sha256,
                        value.TeacherProvenance!.TrainingContentSha256))
                    .Any())
            {
                return OperationResult<DpoTransactionRequest[]>.Fail(
                    "A teacher-backed DPO pair does not match its exact content provenance.",
                    "DPO_TRANSACTION_TEACHER_BINDING_INVALID");
            }
            current.Add(new DpoTransactionRequest(
                provenance.RequestId,
                groupId.Length == 0
                    ? provenance.TrainingContentSha256
                    : groupId,
                groupId.Length == 0 ? pair.PairId : string.Empty,
                DpoTransactionRequestState.Reserved,
                groupId,
                unit.Select(static value => value.PairId).ToArray(),
                unit.Select(static value =>
                    value.TeacherProvenance!.TrainingContentSha256).ToArray()));
        }

        return OperationResult<DpoTransactionRequest[]>.Ok(
            current
                .OrderBy(static value => value.RequestId, StringComparer.Ordinal)
                .ToArray());
    }

    internal static bool HasReplayLedgerEntryCapacity(
        int retainedEntryCount,
        int requestedEntryCount)
        => retainedEntryCount is >= 0 and <= MaximumReplayLedgerEntryCount
           && requestedEntryCount >= 0
           && requestedEntryCount
               <= MaximumReplayLedgerEntryCount - retainedEntryCount;

    private static OperationResult<bool> ValidateReplayLedgerReservation(
        DpoReplayLedgerRecord replayLedger,
        IReadOnlyList<DpoTransactionRequest> requests)
    {
        if (!HasReplayLedgerEntryCapacity(
                replayLedger.Entries.Length,
                requests.Count))
        {
            return FailReplayLedgerCapacity<bool>();
        }
        var projected = new DpoReplayLedgerRecord(
            ReplayLedgerSchemaVersion,
            replayLedger.Entries.Concat(requests.Select(static request =>
                    new DpoReplayLedgerEntry(
                        request.PairId,
                        request.RequestId,
                        request.ContentSha256,
                        request.GroupId,
                        request.PairIds,
                        request.ContentSha256s)))
                .OrderBy(static value => value.RequestId, StringComparer.Ordinal)
                .ToArray(),
            RecordSha256: string.Empty);
        var hash = ComputeReplayLedgerSha256(projected);
        return hash.IsSuccess
            ? OperationResult<bool>.Ok(true)
            : hash.MapError<bool>();
    }

    private static OperationResult<bool> ValidateTrainingReadyPairs(
        IReadOnlyList<StoredDpoPairRecord> selected)
    {
        if (selected.Any(static value => value.SchemaVersion < CurrentSchemaVersion))
            return LegacyContentBindingRejected<bool>();

        var pairs = selected.Select(ToPair).ToArray();
        if (pairs.Any(static value => value.TeacherProvenance is null))
        {
            return OperationResult<bool>.Fail(
                "Typed teacher provenance is required before a DPO pair can enter a training transaction.",
                LegalTrainingTeacherAdmissionAuthority
                    .ProvenanceRequiredErrorCode);
        }
        if (pairs.Any(static value =>
                value.TeacherProvenance!.SourceKind ==
                    TrainingTeacherSourceKind.DeterministicNoExternalTeacher
                && !LegalTrainingTeacherAdmissionAuthority
                    .IsExactDeterministicNoExternalTeacherProvenance(
                        value.TeacherProvenance)))
        {
            return OperationResult<bool>.Fail(
                "Deterministic DPO pair provenance must prove that no external teacher output was used before training readiness.",
                LegalTrainingTeacherAdmissionAuthority
                    .ProvenanceInvalidErrorCode);
        }
        var governance = TrainingDataGovernanceAdmission.Validate(pairs);
        if (!governance.IsSuccess)
            return governance;
        var content = DpoPairTrainingContentAuthority.Project(pairs);
        if (!content.IsSuccess || content.Value is null
            || content.Value.Rows.Count != pairs.Length)
        {
            return content.MapError<bool>();
        }
        for (var index = 0; index < pairs.Length; index++)
        {
            if (!StringComparer.Ordinal.Equals(
                    pairs[index].TeacherProvenance!.TrainingContentSha256,
                    content.Value.Rows[index].Sha256))
            {
                return OperationResult<bool>.Fail(
                    "A teacher-backed DPO pair does not match its exact content provenance.",
                    "DPO_TRANSACTION_TEACHER_BINDING_INVALID");
            }
        }
        return OperationResult<bool>.Ok(true);
    }

    private OperationResult<bool> ValidateTransactionPairs(
        DpoTransactionRecord transaction,
        StoreSnapshot snapshot)
    {
        foreach (var pair in transaction.Pairs)
        {
            if (!snapshot.RecordsByIdentity.TryGetValue(pair.PairId, out var stored)
                || !StringComparer.Ordinal.Equals(stored.Domain, pair.Domain))
            {
                return FailTransactionCorrupt<bool>();
            }
        }
        var expected = BuildRequestBindings(
            transaction.Pairs.Select(value =>
                    snapshot.RecordsByIdentity[value.PairId])
                .ToArray(),
            [],
            new DpoReplayLedgerRecord(
                ReplayLedgerSchemaVersion,
                [],
                string.Empty));
        if (!expected.IsSuccess || expected.Value is null
            || expected.Value.Length != transaction.Requests.Length)
            return FailTransactionCorrupt<bool>();
        for (var index = 0; index < expected.Value.Length; index++)
        {
            var expectedRequest = expected.Value[index];
            var actual = transaction.Requests[index];
            if (!TransactionRequestsEqual(
                    expectedRequest with { State = actual.State },
                    actual))
            {
                return FailTransactionCorrupt<bool>();
            }
        }
        return OperationResult<bool>.Ok(true);
    }

    private static bool TransactionRequestsEqual(
        DpoTransactionRequest left,
        DpoTransactionRequest right)
        => StringComparer.Ordinal.Equals(left.RequestId, right.RequestId)
           && StringComparer.Ordinal.Equals(left.ContentSha256, right.ContentSha256)
           && StringComparer.Ordinal.Equals(left.PairId, right.PairId)
           && left.State == right.State
           && StringComparer.Ordinal.Equals(left.GroupId ?? "", right.GroupId ?? "")
           && RequestPairIds(left).SequenceEqual(
               RequestPairIds(right),
               StringComparer.Ordinal)
           && RequestContentDigests(left).SequenceEqual(
               RequestContentDigests(right),
               StringComparer.Ordinal);

    private OperationResult<TransactionSnapshot> ReadTransactionSnapshot(
        string transactionId)
        => ExecuteTransactionMutation(() =>
            ReadTransactionSnapshotCore(transactionId));

    private OperationResult<TransactionSnapshot> ReadTransactionSnapshotCore(
        string transactionId)
    {
        var pairs = LoadSnapshot(pairIdentityForCollision: null);
        if (!pairs.IsSuccess || pairs.Value is null)
            return pairs.MapError<TransactionSnapshot>();
        var transaction = ReadTransaction(transactionId);
        if (!transaction.IsSuccess || transaction.Value is null)
            return transaction.MapError<TransactionSnapshot>();
        var validated = ValidateTransactionPairs(transaction.Value, pairs.Value);
        if (!validated.IsSuccess)
            return validated.MapError<TransactionSnapshot>();
        return BuildSnapshot(transaction.Value, pairs.Value);
    }

    private OperationResult<TransactionSnapshot> BuildSnapshot(
        DpoTransactionRecord transaction,
        StoreSnapshot pairs)
    {
        var receipt = TrainingSourceProvenance.BuildDpoPublicationReceipt(
            transaction.Pairs.Select(static value => value.PairId)
                .OrderBy(static value => value, StringComparer.Ordinal),
            transaction.Domains.Select(static value => value.Domain));
        if (!receipt.IsSuccess || receipt.Value is null
            || !StringComparer.Ordinal.Equals(
                receipt.Value.TransactionId,
                transaction.TransactionId)
            || !StringComparer.Ordinal.Equals(
                receipt.Value.PairBatchSha256,
                transaction.PairBatchSha256))
        {
            return FailTransactionCorrupt<TransactionSnapshot>();
        }

        var selectedPairs = transaction.Pairs.Select(value =>
            new TransactionPairSnapshot(
                value.PairId,
                ToPair(pairs.RecordsByIdentity[value.PairId]))).ToArray();
        var domains = new List<TransactionDomainSnapshot>(
            transaction.Domains.Length);
        foreach (var domain in transaction.Domains)
        {
            TrainedAdapterArtifact? artifact = null;
            if (domain.State is DpoTransactionDomainState.CandidateReady
                or DpoTransactionDomainState.Published)
            {
                if (domain.Seed is null
                    || !TrainingSourceProvenance.IsCanonicalSha256(
                        domain.AdapterSha256))
                {
                    return FailTransactionCorrupt<TransactionSnapshot>();
                }
                var inspected = InspectCandidate(
                    CandidatePath(transaction.TransactionId, domain.Domain));
                if (!inspected.IsSuccess
                    || !StringComparer.Ordinal.Equals(
                        inspected.Value,
                        domain.AdapterSha256))
                {
                    return OperationResult<TransactionSnapshot>.Fail(
                        "A durable DPO candidate is missing or no longer matches its immutable receipt.",
                        "DPO_TRANSACTION_CANDIDATE_RECOVERY_REQUIRED");
                }
                artifact = new TrainedAdapterArtifact(
                    CandidatePath(transaction.TransactionId, domain.Domain),
                    BuildCandidateProvenance(
                        transaction,
                        domain.Seed,
                        domain.AdapterSha256));
            }
            domains.Add(new TransactionDomainSnapshot(
                domain.Domain,
                domain.State,
                domain.Seed?.TrainingExamples ?? 0,
                domain.TrainedAt,
                artifact));
        }

        return OperationResult<TransactionSnapshot>.Ok(
            new TransactionSnapshot(
                receipt.Value,
                Array.AsReadOnly(selectedPairs),
                domains.AsReadOnly(),
                transaction.State));
    }

    private OperationResult<CandidateReservation> PrepareCandidate(
        string transactionId,
        string domain,
        CandidateProvenanceSeed seed)
    {
        var mutation = ExecuteTransactionMutation(() =>
            PrepareCandidateCore(transactionId, domain, seed));
        return mutation;
    }

    private OperationResult<CandidateReservation> PrepareCandidateCore(
        string transactionId,
        string domain,
        CandidateProvenanceSeed seed)
    {
        var transactionResult = ReadTransaction(transactionId);
        if (!transactionResult.IsSuccess || transactionResult.Value is null)
            return transactionResult.MapError<CandidateReservation>();
        var transaction = transactionResult.Value;
        if (transaction.State == DpoTransactionState.Committed)
        {
            return OperationResult<CandidateReservation>.Fail(
                "The DPO transaction is already committed.",
                "DPO_TRANSACTION_ALREADY_COMMITTED");
        }
        var canonicalSeed = CanonicalizeSeed(seed);
        if (!canonicalSeed.IsSuccess || canonicalSeed.Value is null)
            return canonicalSeed.MapError<CandidateReservation>();
        seed = canonicalSeed.Value;
        var domainIndex = Array.FindIndex(
            transaction.Domains,
            value => StringComparer.Ordinal.Equals(value.Domain, domain));
        if (domainIndex < 0 || !IsCanonicalSeed(seed))
            return InvalidCandidate<CandidateReservation>();

        var current = transaction.Domains[domainIndex];
        if (current.Seed is not null && !CandidateSeedEquals(current.Seed, seed))
        {
            if (current.State is DpoTransactionDomainState.MutationStarted
                or DpoTransactionDomainState.CandidateReady
                or DpoTransactionDomainState.Published)
            {
                return OperationResult<CandidateReservation>.Fail(
                    "The exact supervised input changed after durable DPO mutation began; an existing exact candidate is required for recovery.",
                    "DPO_TRANSACTION_RECOVERY_REQUIRED");
            }
            return OperationResult<CandidateReservation>.Fail(
                "The DPO candidate provenance changed during transaction resume.",
                "DPO_TRANSACTION_INPUT_CHANGED");
        }

        if (current.Seed is null)
        {
            var domains = transaction.Domains.ToArray();
            domains[domainIndex] = current with
            {
                State = DpoTransactionDomainState.CandidatePrepared,
                Seed = seed,
            };
            transaction = transaction with
            {
                State = DpoTransactionState.Active,
                Domains = domains,
            };
            var futureCapacity = PreflightFutureCandidateStates(transaction);
            if (!futureCapacity.IsSuccess)
                return futureCapacity.MapError<CandidateReservation>();
            var write = WriteTransaction(transaction);
            if (!write.IsSuccess)
                return write.MapError<CandidateReservation>();
            current = domains[domainIndex];
        }

        var recovered = TryRecoverCandidate(transaction, domainIndex);
        if (!recovered.IsSuccess)
            return recovered.MapError<CandidateReservation>();
        if (recovered.Value is not null)
        {
            return OperationResult<CandidateReservation>.Ok(
                new CandidateReservation(
                    this,
                    StagingPath(transactionId, domain),
                    recovered.Value,
                    null));
        }

        var lease = AcquireCandidateLease(transactionId, domain);
        if (!lease.IsSuccess || lease.Value is null)
            return lease.MapError<CandidateReservation>();
        return OperationResult<CandidateReservation>.Ok(
            new CandidateReservation(
                this,
                StagingPath(transactionId, domain),
                null,
                lease.Value));
    }

    private OperationResult<TrainedAdapterArtifact?> TryRecoverCandidate(
        DpoTransactionRecord transaction,
        int domainIndex)
    {
        var domain = transaction.Domains[domainIndex];
        if (domain.Seed is null)
            return OperationResult<TrainedAdapterArtifact?>.Ok(null);
        var candidatePath = CandidatePath(transaction.TransactionId, domain.Domain);
        var stagingPath = StagingPath(transaction.TransactionId, domain.Domain);
        if (domain.State is DpoTransactionDomainState.CandidateReady
            or DpoTransactionDomainState.Published)
        {
            if (!File.Exists(candidatePath))
            {
                return OperationResult<TrainedAdapterArtifact?>.Fail(
                    "A durable DPO candidate is missing and cannot be retrained or replaced.",
                    "DPO_TRANSACTION_CANDIDATE_RECOVERY_REQUIRED");
            }
            var immutable = InspectCandidate(candidatePath);
            if (!immutable.IsSuccess
                || !StringComparer.Ordinal.Equals(
                    immutable.Value,
                    domain.AdapterSha256))
            {
                return OperationResult<TrainedAdapterArtifact?>.Fail(
                    "A durable DPO candidate no longer matches its immutable receipt.",
                    "DPO_TRANSACTION_CANDIDATE_RECOVERY_REQUIRED");
            }
            return OperationResult<TrainedAdapterArtifact?>.Ok(
                new TrainedAdapterArtifact(
                    candidatePath,
                    BuildCandidateProvenance(
                        transaction,
                        domain.Seed,
                        domain.AdapterSha256)));
        }
        var path = File.Exists(candidatePath)
            ? candidatePath
            : File.Exists(stagingPath)
                ? stagingPath
                : null;
        if (path is null)
        {
            return domain.State == DpoTransactionDomainState.MutationStarted
                ? RecoveryRequired<TrainedAdapterArtifact?>()
                : OperationResult<TrainedAdapterArtifact?>.Ok(null);
        }

        var inspected = InspectCandidate(path);
        if (!inspected.IsSuccess || inspected.Value is null)
        {
            if (PathsEqual(path, stagingPath)
                && domain.State != DpoTransactionDomainState.MutationStarted)
            {
                DeleteTemporaryFile(stagingPath);
                return OperationResult<TrainedAdapterArtifact?>.Ok(null);
            }
            return domain.State == DpoTransactionDomainState.MutationStarted
                ? RecoveryRequired<TrainedAdapterArtifact?>()
                : inspected.MapError<TrainedAdapterArtifact?>();
        }
        if (PathsEqual(path, stagingPath))
        {
            try
            {
                File.Move(stagingPath, candidatePath);
            }
            catch (IOException) when (File.Exists(candidatePath))
            {
                DeleteTemporaryFile(stagingPath);
                var concurrent = InspectCandidate(candidatePath);
                if (!concurrent.IsSuccess || concurrent.Value is null
                    || !StringComparer.Ordinal.Equals(
                        concurrent.Value,
                        inspected.Value))
                {
                    return FailTransactionCorrupt<TrainedAdapterArtifact?>();
                }
            }
        }

        if (domain.State is not (DpoTransactionDomainState.CandidateReady
            or DpoTransactionDomainState.Published)
            || !StringComparer.Ordinal.Equals(
                domain.AdapterSha256,
                inspected.Value))
        {
            var domains = transaction.Domains.ToArray();
            domains[domainIndex] = domain with
            {
                State = DpoTransactionDomainState.CandidateReady,
                AdapterSha256 = inspected.Value,
                TrainedAt = domain.TrainedAt == default
                    ? DateTimeOffset.UtcNow
                    : domain.TrainedAt,
            };
            var write = WriteTransaction(transaction with { Domains = domains });
            if (!write.IsSuccess)
                return write.MapError<TrainedAdapterArtifact?>();
            domain = domains[domainIndex];
        }

        return OperationResult<TrainedAdapterArtifact?>.Ok(
            new TrainedAdapterArtifact(
                candidatePath,
                BuildCandidateProvenance(
                        transaction,
                        domain.Seed!,
                    inspected.Value)));
    }

    private OperationResult<bool> PreflightFutureCandidateStates(
        DpoTransactionRecord transaction)
    {
        var domains = transaction.Domains.Select(static domain =>
        {
            if (domain.Seed is null)
                return domain;
            return domain with
            {
                State = DpoTransactionDomainState.Published,
                AdapterSha256 = domain.AdapterSha256.Length == 0
                    ? new string('f', 64)
                    : domain.AdapterSha256,
                TrainedAt = domain.TrainedAt == default
                    ? MaximumCanonicalUtcTimestamp
                    : domain.TrainedAt,
            };
        }).ToArray();
        var requests = transaction.Requests.Select(static request =>
            request with
            {
                State = DpoTransactionRequestState.Consumed,
            }).ToArray();
        var hash = ComputeTransactionRecordSha256(transaction with
        {
            State = DpoTransactionState.Committed,
            Domains = domains,
            Requests = requests,
        });
        return hash.IsSuccess
            ? OperationResult<bool>.Ok(true)
            : hash.MapError<bool>();
    }

    private OperationResult<string> InspectCandidate(string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            if (_storageRoot is null
                || !PathsEqual(Path.GetDirectoryName(fullPath)!, _storageRoot)
                || AnyExistingPathSegmentIsReparsePoint(_storageRoot)
                || AnyExistingPathSegmentIsReparsePoint(fullPath)
                || !HasSafeUnixFileMode(fullPath))
            {
                return FailTransactionCorrupt<string>();
            }
            using var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                64 * 1024,
                FileOptions.SequentialScan);
            if (!OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, fullPath)
                || stream.Length <= 0
                || stream.Length > MaximumAdapterBytes)
            {
                return FailTransactionCorrupt<string>();
            }
            var hash = TrainingSourceProvenance.Sha256(stream);
            stream.Position = 0;
            LlmModel.ValidateAdapterCheckpointArtifact(stream);
            return OperationResult<string>.Ok(hash);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FailTransactionCorrupt<string>();
        }
    }

    private OperationResult<bool> LoadExistingAdapterCheckpointForCandidate(
        string path,
        string expectedAdapterSha256,
        LlmModel model)
    {
        // WHY: crash recovery may finish only the missing full checkpoint, so
        // it must reopen the already accepted adapter candidate through the
        // same safe path, size, hash, and format boundary before model bytes
        // are restored for repository-owned publication.
        ArgumentNullException.ThrowIfNull(model);

        try
        {
            var fullPath = Path.GetFullPath(path);
            if (_storageRoot is null
                || !TrainingSourceProvenance.IsCanonicalSha256(
                    expectedAdapterSha256)
                || !PathsEqual(Path.GetDirectoryName(fullPath)!, _storageRoot)
                || AnyExistingPathSegmentIsReparsePoint(_storageRoot)
                || AnyExistingPathSegmentIsReparsePoint(fullPath)
                || !HasSafeUnixFileMode(fullPath))
            {
                return FailTransactionCorrupt<bool>();
            }

            using var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                64 * 1024,
                FileOptions.SequentialScan);
            if (!OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    fullPath)
                || stream.Length <= 0
                || stream.Length > MaximumAdapterBytes)
            {
                return FailTransactionCorrupt<bool>();
            }

            var hash = TrainingSourceProvenance.Sha256(stream);
            if (!StringComparer.Ordinal.Equals(hash, expectedAdapterSha256))
            {
                return OperationResult<bool>.Fail(
                    "A durable DPO candidate no longer matches its immutable receipt.",
                    "DPO_TRANSACTION_CANDIDATE_RECOVERY_REQUIRED");
            }

            stream.Position = 0;
            LlmModel.ValidateAdapterCheckpointArtifact(stream);
            stream.Position = 0;
            model.LoadAdapterCheckpoint(stream);
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                "A durable DPO candidate could not be restored into the repository-owned model checkpoint.",
                "DPO_TRANSACTION_CANDIDATE_RECOVERY_REQUIRED");
        }
    }

    private OperationResult<CandidateLease> AcquireCandidateLease(
        string transactionId,
        string domain)
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
            return FailUnavailable<CandidateLease>();
        var path = CandidateLockPath(transactionId, domain);
        try
        {
            var stream = OpenPrivateFile(
                path,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None,
                FileOptions.WriteThrough);
            if (stream.Length == 0)
            {
                stream.SetLength(1);
                stream.Flush(flushToDisk: true);
            }
            if (stream.Length != 1
                || !OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path)
                || !HasSafeUnixFileMode(path))
            {
                stream.Dispose();
                return FailTransactionCorrupt<CandidateLease>();
            }
            stream.Lock(0, 1);
            return OperationResult<CandidateLease>.Ok(new CandidateLease(stream));
        }
        catch (IOException exception) when (IsLockContention(
            exception,
            OperatingSystem.IsWindows()))
        {
            return OperationResult<CandidateLease>.Fail(
                "The DPO transaction domain is already being trained.",
                "DPO_TRANSACTION_DOMAIN_BUSY");
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FailUnavailable<CandidateLease>();
        }
    }

    private OperationResult<string> SaveAdapterCheckpointForCandidate(
        string path,
        LlmModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        string? fullPath = null;
        try
        {
            fullPath = Path.GetFullPath(path);
            if (_storageRoot is null
                || !PathsEqual(Path.GetDirectoryName(fullPath)!, _storageRoot)
                || AnyExistingPathSegmentIsReparsePoint(_storageRoot)
                || AnyExistingPathSegmentIsReparsePoint(fullPath))
            {
                return FailTransactionCorrupt<string>();
            }

            // Промежуточный файл кандидата обученных весов создаётся и
            // заполняется только здесь, внутри защищённого хранилища
            // FileDpoPairStore. Тренер отдаёт модель, но не получает права
            // самому создать файл по промежуточному пути StagingPath:
            // доступ только владельцу, проверка того, что открытый файловый
            // дескриптор ведёт к ожидаемому файлу, и принудительная запись на
            // диск остаются у владельца хранилища.
            using var stream = OpenPrivateFile(
                fullPath,
                FileMode.CreateNew,
                FileAccess.ReadWrite,
                FileShare.None,
                FileOptions.WriteThrough,
                64 * 1024);
            if (!OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, fullPath)
                || !HasSafeUnixFileMode(fullPath)
                || stream.Length != 0)
            {
                return FailTransactionCorrupt<string>();
            }

            model.SaveAdapterCheckpoint(stream);
            stream.Flush(flushToDisk: true);
            if (!OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, fullPath)
                || !HasSafeUnixFileMode(fullPath)
                || stream.Length <= 0
                || stream.Length > MaximumAdapterBytes)
            {
                return RecoveryRequired<string>();
            }

            // Хеш и формат проверяются на том же открытом файловом
            // дескрипторе до его закрытия. Если запись оборвалась или
            // дескриптор неожиданно ведёт не к тому файлу, транзакция
            // остаётся в состоянии начатой мутации MutationStarted:
            // следующий запуск должен потребовать восстановление, а не
            // подменить кандидат обученных весов.
            stream.Position = 0;
            var hash = TrainingSourceProvenance.Sha256(stream);
            stream.Position = 0;
            model.ValidateCurrentAdapterCheckpointArtifact(stream);
            return OperationResult<string>.Ok(hash);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            if (fullPath is not null && File.Exists(fullPath))
                return RecoveryRequired<string>();
            return OperationResult<string>.Fail(
                "The durable DPO candidate adapter checkpoint could not be saved.",
                "ADAPTER_CHECKPOINT_SAVE_FAILED");
        }
    }

    private OperationResult<bool> ValidateTeacherBatch(
        string transactionId,
        string domain,
        DpoTrainingContentBatch trainingContent,
        IReadOnlyList<TrainingTeacherProvenance?> provenance)
        => ExecuteTransactionMutation(() =>
        {
            var transaction = ReadTransaction(transactionId);
            if (!transaction.IsSuccess || transaction.Value is null)
                return transaction.MapError<bool>();
            var domainIndex = Array.FindIndex(
                transaction.Value.Domains,
                value => StringComparer.Ordinal.Equals(value.Domain, domain));
            if (domainIndex < 0)
                return InvalidTeacherResume();
            var domainState = transaction.Value.Domains[domainIndex];
            var pairIds = transaction.Value.Pairs
                .Where(value => StringComparer.Ordinal.Equals(value.Domain, domain))
                .Select(static value => value.PairId)
                .ToArray();
            if (pairIds.Length != trainingContent.Rows.Count
                || provenance.Count != trainingContent.Rows.Count)
            {
                return InvalidTeacherResume();
            }
            var requests = transaction.Value.Requests
                .Where(value => RequestPairIds(value).Any(pairId =>
                    pairIds.Contains(pairId, StringComparer.Ordinal)))
                .ToArray();
            var bindings = pairIds.Select(pairId => requests
                    .SelectMany(request => RequestPairIds(request)
                        .Select((childPairId, childIndex) => new
                        {
                            Request = request,
                            PairId = childPairId,
                            Content = RequestContentDigests(request)[childIndex],
                        }))
                    .SingleOrDefault(value => StringComparer.Ordinal.Equals(
                        value.PairId,
                        pairId)))
                .ToArray();
            if (bindings.Any(static value => value is null))
                return InvalidTeacherResume();
            for (var index = 0; index < bindings.Length; index++)
            {
                var binding = bindings[index]!;
                if (provenance[index] is not { } row
                    || !StringComparer.Ordinal.Equals(
                        binding.Request.RequestId,
                        row.RequestId)
                    || !StringComparer.Ordinal.Equals(
                        binding.Content,
                        row.TrainingContentSha256)
                    || !StringComparer.Ordinal.Equals(
                        binding.Content,
                        trainingContent.Rows[index].Sha256))
                {
                    return InvalidTeacherResume();
                }
            }
            if (domainState.State is DpoTransactionDomainState.CandidateReady
                or DpoTransactionDomainState.Published)
            {
                if (requests.Any(static value =>
                        value.State != DpoTransactionRequestState.Consumed))
                {
                    return RecoveryRequired<bool>();
                }

                // WHY: retrying an already accepted durable adapter candidate
                // must be allowed to reach PrepareCandidate. That is the only
                // place that can compare the exact supervised seed and return
                // ExistingArtifact; without this narrow pass-through, callers
                // fail at DPO admission and the repository-owned checkpoint
                // fail-closed guard is unreachable.
                var recovered = TryRecoverCandidate(transaction.Value, domainIndex);
                if (!recovered.IsSuccess)
                    return recovered.MapError<bool>();
                return recovered.Value is null
                    ? RecoveryRequired<bool>()
                    : OperationResult<bool>.Ok(true);
            }
            if (domainState.State is DpoTransactionDomainState.MutationStarted
                || requests.Any(static value =>
                    value.State != DpoTransactionRequestState.Reserved))
            {
                return RecoveryRequired<bool>();
            }
            return OperationResult<bool>.Ok(true);
        });

    private OperationResult<bool> BeginMutation(
        string transactionId,
        string domain)
        => ExecuteTransactionMutation(() =>
        {
            var transaction = ReadTransaction(transactionId);
            if (!transaction.IsSuccess || transaction.Value is null)
                return transaction.MapError<bool>();
            var domainIndex = Array.FindIndex(
                transaction.Value.Domains,
                value => StringComparer.Ordinal.Equals(value.Domain, domain));
            if (domainIndex < 0
                || transaction.Value.Domains[domainIndex].Seed is null)
            {
                return InvalidCandidate<bool>();
            }

            var futureCapacity = PreflightFutureCandidateStates(
                transaction.Value);
            if (!futureCapacity.IsSuccess)
                return futureCapacity;

            var domains = transaction.Value.Domains.ToArray();
            if (domains[domainIndex].State ==
                DpoTransactionDomainState.CandidatePrepared)
            {
                domains[domainIndex] = domains[domainIndex] with
                {
                    State = DpoTransactionDomainState.MutationStarted,
                };
            }
            var pairIds = transaction.Value.Pairs
                .Where(value => StringComparer.Ordinal.Equals(value.Domain, domain))
                .Select(static value => value.PairId)
                .ToHashSet(StringComparer.Ordinal);
            var requests = transaction.Value.Requests
                .Select(value => RequestPairIds(value).Any(pairIds.Contains)
                    ? value with { State = DpoTransactionRequestState.Consumed }
                    : value)
                .ToArray();
            return WriteTransaction(transaction.Value with
            {
                State = DpoTransactionState.Active,
                Domains = domains,
                Requests = requests,
            });
        });

    private OperationResult<TrainedAdapterArtifact> CompleteCandidate(
        string transactionId,
        string domain)
        => ExecuteTransactionMutation(() =>
        {
            var transaction = ReadTransaction(transactionId);
            if (!transaction.IsSuccess || transaction.Value is null)
                return transaction.MapError<TrainedAdapterArtifact>();
            var domainIndex = Array.FindIndex(
                transaction.Value.Domains,
                value => StringComparer.Ordinal.Equals(value.Domain, domain));
            if (domainIndex < 0
                || transaction.Value.Domains[domainIndex].Seed is null
                || transaction.Value.Domains[domainIndex].State
                    == DpoTransactionDomainState.CandidatePrepared)
            {
                return InvalidCandidate<TrainedAdapterArtifact>();
            }
            var recovered = TryRecoverCandidate(transaction.Value, domainIndex);
            return recovered.IsSuccess && recovered.Value is not null
                ? OperationResult<TrainedAdapterArtifact>.Ok(recovered.Value)
                : recovered.MapError<TrainedAdapterArtifact>();
        });

    private OperationResult<bool> RecordPublished(
        string transactionId,
        string domain,
        ModelAdapterProvenance provenance)
        => ExecuteTransactionMutation(() =>
        {
            var transaction = ReadTransaction(transactionId);
            if (!transaction.IsSuccess || transaction.Value is null)
                return transaction.MapError<bool>();
            var domainIndex = Array.FindIndex(
                transaction.Value.Domains,
                value => StringComparer.Ordinal.Equals(value.Domain, domain));
            if (domainIndex < 0
                || transaction.Value.Domains[domainIndex].Seed is null
                || transaction.Value.Domains[domainIndex].State is not
                    (DpoTransactionDomainState.CandidateReady
                        or DpoTransactionDomainState.Published))
            {
                return InvalidCandidate<bool>();
            }
            var expected = BuildCandidateProvenance(
                transaction.Value,
                transaction.Value.Domains[domainIndex].Seed!,
                transaction.Value.Domains[domainIndex].AdapterSha256);
            if (!CandidateProvenanceEquals(expected, provenance))
            {
                return OperationResult<bool>.Fail(
                    "Registry publication provenance does not match the exact DPO transaction candidate.",
                    "DPO_TRANSACTION_REGISTRY_MISMATCH");
            }
            var domains = transaction.Value.Domains.ToArray();
            domains[domainIndex] = domains[domainIndex] with
            {
                State = DpoTransactionDomainState.Published,
            };
            var allPublished = domains.All(static value =>
                value.State == DpoTransactionDomainState.Published);
            return WriteTransaction(transaction.Value with
            {
                State = allPublished
                    ? DpoTransactionState.PublicationComplete
                    : DpoTransactionState.Active,
                Domains = domains,
            });
        });

    private OperationResult<bool> CommitDrain(string transactionId)
        => ExecuteTransactionMutation(() =>
        {
            var transaction = ReadTransaction(transactionId);
            if (!transaction.IsSuccess || transaction.Value is null)
                return transaction.MapError<bool>();
            if (transaction.Value.State == DpoTransactionState.Committed)
                return CompactCommittedTransactions([transaction.Value]);
            if (transaction.Value.State != DpoTransactionState.PublicationComplete
                || transaction.Value.Domains.Any(static value =>
                    value.State != DpoTransactionDomainState.Published))
            {
                return OperationResult<bool>.Fail(
                    "The DPO transaction cannot drain before every exact registry publication is verified.",
                    "DPO_TRANSACTION_PUBLICATION_INCOMPLETE");
            }

            var committed = transaction.Value with
            {
                State = DpoTransactionState.Committed,
            };
            var write = WriteTransaction(committed);
            if (!write.IsSuccess)
                return write;
            var readback = ReadTransaction(transactionId);
            if (!readback.IsSuccess || readback.Value is null
                || readback.Value.State != DpoTransactionState.Committed)
            {
                return FailTransactionCorrupt<bool>();
            }

            // The committed receipt is the durable tombstone.  Only after its
            // exact readback may the selected queue records be compacted.
            return CompactCommittedTransactions([committed]);
        });

    private OperationResult<bool> CompactCommittedTransactions(
        IReadOnlyList<DpoTransactionRecord> committed)
    {
        if (committed.Count == 0)
            return OperationResult<bool>.Ok(true);
        var replay = LoadReplayLedger();
        if (!replay.IsSuccess || replay.Value is null)
            return replay.MapError<bool>();
        var byPair = replay.Value.Entries
            .SelectMany(static entry => ReplayPairIds(entry)
                .Select(pairId => (pairId, entry)))
            .ToDictionary(static value => value.pairId,
                static value => value.entry,
                StringComparer.Ordinal);
        var byRequest = replay.Value.Entries.ToDictionary(
            static value => value.RequestId,
            StringComparer.Ordinal);

        foreach (var transaction in committed)
        {
            if (transaction.State != DpoTransactionState.Committed)
                return FailTransactionCorrupt<bool>();
            foreach (var request in transaction.Requests)
            {
                var entry = new DpoReplayLedgerEntry(
                    request.PairId,
                    request.RequestId,
                    request.ContentSha256,
                    request.GroupId,
                    RequestPairIds(request).ToArray(),
                    RequestContentDigests(request).ToArray(),
                    transaction.Kind,
                    BuildExternalReplayEvidence(transaction));
                if (RequestPairIds(request).Any(pairId =>
                        byPair.TryGetValue(pairId, out var existingPair)
                        && !ReplayEntriesEqual(existingPair, entry))
                    || (byRequest.TryGetValue(
                            request.RequestId,
                            out var existingRequest)
                        && !ReplayEntriesEqual(existingRequest, entry)))
                {
                    return FailTransactionCorrupt<bool>();
                }
                foreach (var pairId in RequestPairIds(request))
                    byPair[pairId] = entry;
                byRequest[request.RequestId] = entry;
            }
        }
        if (byRequest.Count > MaximumReplayLedgerEntryCount)
        {
            return FailReplayLedgerCapacity<bool>();
        }

        var ledger = new DpoReplayLedgerRecord(
            ReplayLedgerSchemaVersion,
            byRequest.Values.OrderBy(static value => value.RequestId, StringComparer.Ordinal)
                .ToArray(),
            RecordSha256: string.Empty);
        var write = WriteReplayLedger(ledger);
        if (!write.IsSuccess)
            return write;

        try
        {
            _afterReplayLedgerWrittenBeforePairCompactionForTesting?.Invoke();
            foreach (var transaction in committed)
            {
                var pairCompaction = CompactCommittedPairs(transaction);
                if (!pairCompaction.IsSuccess)
                    return pairCompaction;
                File.Delete(TransactionPath(transaction.TransactionId));
                foreach (var domain in transaction.Domains)
                {
                    DeleteTemporaryFile(StagingPath(
                        transaction.TransactionId,
                        domain.Domain));
                    DeleteTemporaryFile(CandidateLockPath(
                        transaction.TransactionId,
                        domain.Domain));
                }
            }
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FailUnavailable<bool>();
        }
    }

    private OperationResult<bool> CompactCommittedPairs(
        DpoTransactionRecord committed)
    {
        foreach (var group in committed.Pairs
                     .Where(static pair => !string.IsNullOrEmpty(pair.GroupId))
                     .GroupBy(static pair => pair.GroupId!, StringComparer.Ordinal))
        {
            var path = Path.Combine(_storageRoot!, group.Key + AtomicGroupSuffix);
            try
            {
                if (!File.Exists(path))
                    continue;
                var exact = ReadAtomicGroup(path);
                if (!exact.IsSuccess
                    || exact.Value is null
                    || !exact.Value.Children.Select(static child => child.Pair.PairId)
                        .SequenceEqual(
                            group.OrderBy(static pair => pair.GroupOrdinal)
                                .Select(static pair => pair.PairId),
                            StringComparer.Ordinal))
                {
                    return FailTransactionCorrupt<bool>();
                }
                File.Delete(path);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return FailUnavailable<bool>();
            }
        }
        foreach (var pair in committed.Pairs)
        {
            if (!string.IsNullOrEmpty(pair.GroupId))
                continue;
            var path = Path.Combine(_storageRoot!, pair.PairId + CanonicalSuffix);
            try
            {
                if (!PathsEqual(Path.GetDirectoryName(path)!, _storageRoot!)
                    || AnyExistingPathSegmentIsReparsePoint(path))
                {
                    return FailTransactionCorrupt<bool>();
                }
                if (!File.Exists(path))
                    continue;
                var exact = ReadCanonicalRecord(path, pair.PairId);
                if (!exact.IsSuccess
                    || exact.Value is null
                    || !StringComparer.Ordinal.Equals(
                        exact.Value.PairId,
                        pair.PairId)
                    || !StringComparer.Ordinal.Equals(
                        exact.Value.Domain,
                        pair.Domain))
                {
                    return FailTransactionCorrupt<bool>();
                }
                File.Delete(path);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                // The committed receipt remains the authoritative tombstone.
                // A later preparation or explicit commit retry will re-enter
                // this exact-record compaction path under the same root lock.
                return FailUnavailable<bool>();
            }
        }
        return OperationResult<bool>.Ok(true);
    }

    private static DpoExternalReplayEvidence? BuildExternalReplayEvidence(
        DpoTransactionRecord transaction)
    {
        if (transaction.Kind != DpoTransactionKind.ExternalConsumption)
            return null;
        var external = transaction.ExternalConsumption
            ?? throw new InvalidDataException(TransactionCorruptMessage);
        return new DpoExternalReplayEvidence(
            transaction.TransactionId,
            transaction.Domains.Single().Domain,
            external.ParentBatchSha256,
            external.ParentContentSha256,
            external.ParentProvenance
                ?? throw new InvalidDataException(TransactionCorruptMessage),
            external.ParentProvenanceSha256,
            external.ConsumerKind,
            external.ResultSha256,
            external.CheckpointSha256,
            external.ManifestSha256);
    }

    private OperationResult<bool> ReleaseIfUnmutated(string transactionId)
        => ExecuteTransactionMutation(() =>
        {
            var transaction = ReadTransaction(transactionId);
            if (!transaction.IsSuccess || transaction.Value is null)
                return transaction.MapError<bool>();
            if (transaction.Value.State == DpoTransactionState.Committed)
                return OperationResult<bool>.Ok(false);
            if (transaction.Value.Domains.Any(static value =>
                    value.State is DpoTransactionDomainState.MutationStarted
                        or DpoTransactionDomainState.CandidateReady
                        or DpoTransactionDomainState.Published)
                || transaction.Value.Requests.Any(static value =>
                    value.State == DpoTransactionRequestState.Consumed))
            {
                return OperationResult<bool>.Ok(false);
            }
            try
            {
                File.Delete(TransactionPath(transactionId));
                foreach (var domain in transaction.Value.Domains)
                {
                    DeleteTemporaryFile(StagingPath(transactionId, domain.Domain));
                    DeleteTemporaryFile(CandidateLockPath(transactionId, domain.Domain));
                }
                return OperationResult<bool>.Ok(true);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return FailUnavailable<bool>();
            }
        });

    private OperationResult<T> ExecuteTransactionMutation<T>(
        Func<OperationResult<T>> action)
    {
        if (_storageRoot is null || _mutationLockPath is null)
            return FailUnavailable<T>();
        var timeoutBudget = Stopwatch.StartNew();
        var processLeaseResult = AcquireProcessMutationLease(timeoutBudget);
        if (!processLeaseResult.IsSuccess || processLeaseResult.Value is null)
            return processLeaseResult.MapError<T>();
        using var processLease = processLeaseResult.Value;
        var lockRootReady = EnsureWritableRoot();
        if (!lockRootReady.IsSuccess)
            return lockRootReady.MapError<T>();
        var leaseResult = AcquireMutationLease(timeoutBudget);
        if (!leaseResult.IsSuccess || leaseResult.Value is null)
            return leaseResult.MapError<T>();
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
                ? FailUnavailable<T>()
                : rootReady.MapError<T>();
        }
        return action();
    }

    private OperationResult<IReadOnlyList<DpoTransactionRecord>> LoadTransactions()
    {
        if (_storageRoot is null)
            return FailUnavailable<IReadOnlyList<DpoTransactionRecord>>();
        try
        {
            if (!Directory.Exists(_storageRoot))
            {
                return OperationResult<IReadOnlyList<DpoTransactionRecord>>.Ok(
                    Array.Empty<DpoTransactionRecord>());
            }
            var paths = Directory.EnumerateFiles(
                    _storageRoot,
                    $"*{TransactionSuffix}",
                    SearchOption.TopDirectoryOnly)
                .Take(MaximumRetainedTransactionCount + 1)
                .OrderBy(static value => value, StringComparer.Ordinal)
                .ToArray();
            if (paths.Length > MaximumRetainedTransactionCount)
                return FailTransactionCorrupt<IReadOnlyList<DpoTransactionRecord>>();
            var records = new List<DpoTransactionRecord>(paths.Length);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var path in paths)
            {
                var record = ReadTransactionPath(path);
                if (!record.IsSuccess || record.Value is null)
                    return record.MapError<IReadOnlyList<DpoTransactionRecord>>();
                if (!ids.Add(record.Value.TransactionId))
                    return FailTransactionCorrupt<IReadOnlyList<DpoTransactionRecord>>();
                records.Add(record.Value);
            }
            return OperationResult<IReadOnlyList<DpoTransactionRecord>>.Ok(
                records.AsReadOnly());
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FailUnavailable<IReadOnlyList<DpoTransactionRecord>>();
        }
    }

    private OperationResult<DpoReplayLedgerRecord> LoadReplayLedger()
    {
        var path = Path.Combine(_storageRoot!, ReplayLedgerFileName);
        if (!File.Exists(path))
        {
            return OperationResult<DpoReplayLedgerRecord>.Ok(
                new DpoReplayLedgerRecord(
                    ReplayLedgerSchemaVersion,
                    [],
                    string.Empty));
        }
        try
        {
            var fullPath = Path.GetFullPath(path);
            if (_storageRoot is null
                || !PathsEqual(Path.GetDirectoryName(fullPath)!, _storageRoot)
                || AnyExistingPathSegmentIsReparsePoint(fullPath)
                || !HasSafeUnixFileMode(fullPath))
            {
                return FailTransactionCorrupt<DpoReplayLedgerRecord>();
            }
            using var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.SequentialScan);
            if (!OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, fullPath))
                return FailTransactionCorrupt<DpoReplayLedgerRecord>();
            var bytes = ReadBoundedStableBytes(stream);
            if (!bytes.IsSuccess
                || bytes.Value is null
                || !HasBoundedTransactionJsonTokens(bytes.Value))
            {
                return FailTransactionCorrupt<DpoReplayLedgerRecord>();
            }
            using (var document = JsonDocument.Parse(bytes.Value))
            {
                if (!HasNoDuplicateProperties(document.RootElement))
                    return FailTransactionCorrupt<DpoReplayLedgerRecord>();
            }
            var record = JsonSerializer.Deserialize<DpoReplayLedgerRecord>(
                bytes.Value,
                TransactionJsonOptions);
            if (record is null || !ValidateReplayLedger(record))
                return FailTransactionCorrupt<DpoReplayLedgerRecord>();
            var hash = ComputeReplayLedgerSha256(record);
            return hash.IsSuccess
                   && StringComparer.Ordinal.Equals(
                       hash.Value,
                       record.RecordSha256)
                ? OperationResult<DpoReplayLedgerRecord>.Ok(record)
                : FailTransactionCorrupt<DpoReplayLedgerRecord>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FailTransactionCorrupt<DpoReplayLedgerRecord>();
        }
    }

    private OperationResult<bool> WriteReplayLedger(DpoReplayLedgerRecord record)
    {
        if (!ValidateReplayLedger(record))
            return FailTransactionCorrupt<bool>();
        var hash = ComputeReplayLedgerSha256(record);
        if (!hash.IsSuccess || hash.Value is null)
            return hash.MapError<bool>();
        var canonical = record with { RecordSha256 = hash.Value };
        var path = Path.Combine(_storageRoot!, ReplayLedgerFileName);
        var temporary = Path.Combine(
            _storageRoot!,
            $".{Guid.NewGuid():N}.replay-ledger.tmp");
        try
        {
            using (var stream = OpenPrivateFile(
                       temporary,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       FileOptions.WriteThrough))
            {
                using (var bounded = new BoundedTransactionWriteStream(
                           stream,
                           hash: null,
                           MaximumRecordBytes))
                using (var writer = new Utf8JsonWriter(bounded))
                {
                    WriteReplayLedgerJson(writer, canonical);
                    writer.Flush();
                }
                stream.Flush(flushToDisk: true);
                if (!OpenedHandleMatchesRequestedPath(
                        stream.SafeFileHandle,
                        temporary)
                    || !HasSafeUnixFileMode(temporary))
                {
                    throw new IOException("DPO replay-ledger staging path changed.");
                }
            }
            File.Move(temporary, path, overwrite: true);
            var readback = LoadReplayLedger();
            return readback.IsSuccess
                   && readback.Value is not null
                   && ReplayLedgersEqual(readback.Value, canonical)
                ? OperationResult<bool>.Ok(true)
                : FailTransactionCorrupt<bool>();
        }
        catch (TransactionRecordCapacityException)
        {
            DeleteTemporaryFile(temporary);
            return FailReplayLedgerCapacity<bool>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            DeleteTemporaryFile(temporary);
            return FailUnavailable<bool>();
        }
    }

    private static OperationResult<string> ComputeReplayLedgerSha256(
        DpoReplayLedgerRecord record)
    {
        if (!HasBoundedReplayLedgerFields(record))
            return FailReplayLedgerCapacity<string>();
        try
        {
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            using var bounded = new BoundedTransactionWriteStream(
                destination: null,
                hash,
                MaximumRecordBytes);
            using (var writer = new Utf8JsonWriter(bounded))
            {
                WriteReplayLedgerJson(
                    writer,
                    record with { RecordSha256 = string.Empty });
                writer.Flush();
            }
            var digest = Convert.ToHexString(hash.GetHashAndReset())
                .ToLowerInvariant();
            using var finalBounded = new BoundedTransactionWriteStream(
                destination: null,
                hash: null,
                MaximumRecordBytes);
            using (var finalWriter = new Utf8JsonWriter(finalBounded))
            {
                WriteReplayLedgerJson(
                    finalWriter,
                    record with { RecordSha256 = digest });
                finalWriter.Flush();
            }
            return OperationResult<string>.Ok(digest);
        }
        catch (TransactionRecordCapacityException)
        {
            return FailReplayLedgerCapacity<string>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FailTransactionCorrupt<string>();
        }
    }

    private static void WriteReplayLedgerJson(
        Utf8JsonWriter writer,
        DpoReplayLedgerRecord record)
    {
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", record.SchemaVersion);
        writer.WriteStartArray("entries");
        foreach (var entry in record.Entries)
        {
            writer.WriteStartObject();
            writer.WriteString("pairId", entry.PairId);
            writer.WriteString("requestId", entry.RequestId);
            writer.WriteString("contentSha256", entry.ContentSha256);
            if (record.SchemaVersion >= AtomicGroupReplayLedgerSchemaVersion)
            {
                writer.WriteString("groupId", entry.GroupId ?? string.Empty);
                writer.WriteStartArray("pairIds");
                foreach (var pairId in ReplayPairIds(entry))
                    writer.WriteStringValue(pairId);
                writer.WriteEndArray();
                writer.WriteStartArray("contentSha256s");
                foreach (var digest in ReplayContentDigests(entry))
                    writer.WriteStringValue(digest);
                writer.WriteEndArray();
            }
            if (record.SchemaVersion >= ReplayLedgerSchemaVersion)
            {
                writer.WriteNumber("kind", (int)entry.Kind);
                writer.WritePropertyName("externalConsumption");
                JsonSerializer.Serialize(
                    writer,
                    entry.ExternalConsumption,
                    TransactionJsonOptions);
            }
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteString("recordSha256", record.RecordSha256);
        writer.WriteEndObject();
    }

    private static bool ValidateReplayLedger(DpoReplayLedgerRecord record)
        => record.SchemaVersion is LegacyReplayLedgerSchemaVersion
               or AtomicGroupReplayLedgerSchemaVersion
               or ReplayLedgerSchemaVersion
           && HasBoundedReplayLedgerFields(record)
           && record.Entries.All(value => IsValidReplayEntry(
               value,
               record.SchemaVersion))
           && (record.SchemaVersion == LegacyReplayLedgerSchemaVersion
               ? record.Entries.Select(static value => value.PairId)
               : record.Entries.Select(static value => value.RequestId))
               .SequenceEqual(
                   (record.SchemaVersion == LegacyReplayLedgerSchemaVersion
                       ? record.Entries.Select(static value => value.PairId)
                       : record.Entries.Select(static value => value.RequestId))
                       .OrderBy(static value => value, StringComparer.Ordinal),
                   StringComparer.Ordinal)
           && record.Entries.SelectMany(ReplayPairIds)
               .Distinct(StringComparer.Ordinal).Count() == record.Entries.Length
               + record.Entries.Count(static entry =>
                   !string.IsNullOrEmpty(entry.GroupId))
           && record.Entries.Select(static value => value.RequestId)
               .Distinct(StringComparer.Ordinal).Count() == record.Entries.Length
           && (record.RecordSha256.Length == 0
               || TrainingSourceProvenance.IsCanonicalSha256(
                   record.RecordSha256));

    private static bool IsValidReplayEntry(
        DpoReplayLedgerEntry entry,
        int schemaVersion)
    {
        if (entry is null
            || string.IsNullOrWhiteSpace(entry.RequestId)
            || !TrainingSourceProvenance.IsCanonicalSha256(entry.ContentSha256))
        {
            return false;
        }
        if (schemaVersion == LegacyReplayLedgerSchemaVersion)
        {
            return TrainingSourceProvenance.IsCanonicalSha256(entry.PairId)
                   && string.IsNullOrEmpty(entry.GroupId)
                   && ReplayPairIds(entry).Count == 1
                   && ReplayContentDigests(entry).Count == 1;
        }
        if (schemaVersion < ReplayLedgerSchemaVersion
            && (entry.Kind != DpoTransactionKind.AdapterPublication
                || entry.ExternalConsumption is not null)
            || schemaVersion >= ReplayLedgerSchemaVersion
                && (!Enum.IsDefined(entry.Kind)
                    || entry.Kind == DpoTransactionKind.AdapterPublication
                        && entry.ExternalConsumption is not null
                    || entry.Kind == DpoTransactionKind.ExternalConsumption
                        && !IsValidExternalReplayEvidence(entry)))
        {
            return false;
        }
        var groupId = entry.GroupId ?? string.Empty;
        var pairIds = ReplayPairIds(entry);
        var contents = ReplayContentDigests(entry);
        if (pairIds.Count != contents.Count
            || pairIds.Any(static value =>
                !TrainingSourceProvenance.IsCanonicalSha256(value))
            || contents.Any(static value =>
                !TrainingSourceProvenance.IsCanonicalSha256(value)))
        {
            return false;
        }
        return groupId.Length == 0
            ? pairIds.Count == 1
              && StringComparer.Ordinal.Equals(entry.PairId, pairIds[0])
              && StringComparer.Ordinal.Equals(entry.ContentSha256, contents[0])
            : pairIds.Count == 2
              && TrainingSourceProvenance.IsCanonicalSha256(groupId)
              && entry.PairId.Length == 0
              && StringComparer.Ordinal.Equals(entry.ContentSha256, groupId);
    }

    private static bool IsValidExternalReplayEvidence(DpoReplayLedgerEntry entry)
    {
        var external = entry.ExternalConsumption;
        return external is not null
               && !string.IsNullOrWhiteSpace(external.Domain)
               && TrainingSourceProvenance.IsCanonicalSha256(external.TransactionId)
               && TrainingSourceProvenance.IsCanonicalSha256(
                   external.ParentBatchSha256)
               && TrainingSourceProvenance.IsCanonicalSha256(
                   external.ParentContentSha256)
               && IsPersistableProvenance(external.ParentProvenance)
               && StringComparer.Ordinal.Equals(
                   external.ParentProvenance.RequestId,
                   entry.RequestId)
               && external.ParentProvenance.InvocationCount == 1
               && StringComparer.Ordinal.Equals(
                   external.ParentProvenance.TrainingContentSha256,
                   external.ParentContentSha256)
               && TrainingSourceProvenance.IsCanonicalSha256(
                   external.ParentProvenanceSha256)
               && StringComparer.Ordinal.Equals(
                   external.ParentProvenanceSha256,
                   ComputeExternalParentProvenanceSha256(
                       external.ParentProvenance))
               && Enum.IsDefined(external.ConsumerKind)
               && external.ConsumerKind ==
                   DpoExternalConsumerKind.DesignConversionDualTargetTeacherAggregate
               && TrainingSourceProvenance.IsCanonicalSha256(external.ResultSha256)
               && TrainingSourceProvenance.IsCanonicalSha256(
                   external.CheckpointSha256)
               && TrainingSourceProvenance.IsCanonicalSha256(
                   external.ManifestSha256)
               && !string.IsNullOrEmpty(entry.GroupId)
               && ReplayPairIds(entry).Count == 2
               && ReplayContentDigests(entry).Count == 2
               && StringComparer.Ordinal.Equals(
                   external.TransactionId,
                   ComputeExternalTransactionId(entry.GroupId!, external.Domain));
    }

    private static bool ReplayLedgersEqual(
        DpoReplayLedgerRecord left,
        DpoReplayLedgerRecord right)
        => left.SchemaVersion == right.SchemaVersion
           && StringComparer.Ordinal.Equals(
               left.RecordSha256,
               right.RecordSha256)
           && left.Entries.Length == right.Entries.Length
           && left.Entries.Zip(right.Entries).All(static pair =>
               ReplayEntriesEqual(pair.First, pair.Second));

    private static bool ReplayEntriesEqual(
        DpoReplayLedgerEntry left,
        DpoReplayLedgerEntry right)
        => StringComparer.Ordinal.Equals(left.PairId, right.PairId)
           && StringComparer.Ordinal.Equals(left.RequestId, right.RequestId)
           && StringComparer.Ordinal.Equals(left.ContentSha256, right.ContentSha256)
           && StringComparer.Ordinal.Equals(left.GroupId ?? "", right.GroupId ?? "")
           && ReplayPairIds(left).SequenceEqual(
               ReplayPairIds(right),
               StringComparer.Ordinal)
           && ReplayContentDigests(left).SequenceEqual(
               ReplayContentDigests(right),
               StringComparer.Ordinal)
           && left.Kind == right.Kind
           && Equals(left.ExternalConsumption, right.ExternalConsumption);

    private static IReadOnlyList<string> RequestPairIds(
        DpoTransactionRequest request)
        => request.PairIds is { Length: > 0 }
            ? request.PairIds
            : string.IsNullOrEmpty(request.PairId)
                ? Array.Empty<string>()
                : [request.PairId];

    private static IReadOnlyList<string> RequestContentDigests(
        DpoTransactionRequest request)
        => request.ContentSha256s is { Length: > 0 }
            ? request.ContentSha256s
            : string.IsNullOrEmpty(request.ContentSha256)
                ? Array.Empty<string>()
                : [request.ContentSha256];

    private static IReadOnlyList<string> ReplayPairIds(DpoReplayLedgerEntry entry)
        => entry.PairIds is { Length: > 0 }
            ? entry.PairIds
            : string.IsNullOrEmpty(entry.PairId)
                ? Array.Empty<string>()
                : [entry.PairId];

    private static IReadOnlyList<string> ReplayContentDigests(
        DpoReplayLedgerEntry entry)
        => entry.ContentSha256s is { Length: > 0 }
            ? entry.ContentSha256s
            : string.IsNullOrEmpty(entry.ContentSha256)
                ? Array.Empty<string>()
                : [entry.ContentSha256];

    private static bool HasBoundedReplayLedgerFields(DpoReplayLedgerRecord record)
    {
        if (record is null
            || record.Entries is null
            || record.Entries.Length > MaximumReplayLedgerEntryCount)
        {
            return false;
        }
        long aggregate = 0;
        bool Add(string? value)
        {
            if (value is null)
                return false;
            var length = Encoding.UTF8.GetByteCount(value);
            if (length > MaximumFieldBytes
                || length > MaximumRecordBytes - aggregate)
            {
                return false;
            }
            aggregate += length;
            return true;
        }
        try
        {
            if (!Add(record.RecordSha256))
                return false;
            foreach (var entry in record.Entries)
            {
                if (entry is null
                    || !Add(entry.PairId)
                    || !Add(entry.RequestId)
                    || !Add(entry.ContentSha256)
                    || record.SchemaVersion >= AtomicGroupReplayLedgerSchemaVersion
                        && (!Add(entry.GroupId)
                            || ReplayPairIds(entry).Count > 2
                            || ReplayContentDigests(entry).Count > 2
                            || ReplayPairIds(entry).Any(value => !Add(value))
                            || ReplayContentDigests(entry).Any(value => !Add(value))))
                {
                    return false;
                }
                if (record.SchemaVersion >= ReplayLedgerSchemaVersion
                    && entry.ExternalConsumption is { } external
                    && (!Add(external.TransactionId)
                        || !Add(external.Domain)
                        || !Add(external.ParentBatchSha256)
                        || !Add(external.ParentContentSha256)
                        || !Add(external.ParentProvenanceSha256)
                        || !Add(external.ResultSha256)
                        || !Add(external.CheckpointSha256)
                        || !Add(external.ManifestSha256)))
                {
                    return false;
                }
            }
            return true;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private OperationResult<DpoTransactionRecord> ReadTransaction(
        string transactionId)
    {
        if (!TrainingSourceProvenance.IsCanonicalSha256(transactionId))
            return FailTransactionCorrupt<DpoTransactionRecord>();
        return ReadTransactionPath(TransactionPath(transactionId));
    }

    private OperationResult<DpoTransactionRecord> ReadTransactionPath(string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            if (_storageRoot is null
                || !PathsEqual(Path.GetDirectoryName(fullPath)!, _storageRoot)
                || AnyExistingPathSegmentIsReparsePoint(fullPath)
                || !HasSafeUnixFileMode(fullPath))
            {
                return FailTransactionCorrupt<DpoTransactionRecord>();
            }
            using var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.SequentialScan);
            if (!OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, fullPath))
                return FailTransactionCorrupt<DpoTransactionRecord>();
            var bytes = ReadBoundedStableBytes(stream);
            if (!bytes.IsSuccess || bytes.Value is null)
                return bytes.MapError<DpoTransactionRecord>();
            if (!HasBoundedTransactionJsonTokens(bytes.Value))
                return FailTransactionCorrupt<DpoTransactionRecord>();
            using (var document = JsonDocument.Parse(bytes.Value))
            {
                if (!HasNoDuplicateProperties(document.RootElement))
                    return FailTransactionCorrupt<DpoTransactionRecord>();
            }
            var record = JsonSerializer.Deserialize<DpoTransactionRecord>(
                bytes.Value,
                TransactionJsonOptions);
            if (record is null || !ValidateTransactionRecord(record))
                return FailTransactionCorrupt<DpoTransactionRecord>();
            var expectedName = record.TransactionId + TransactionSuffix;
            if (!StringComparer.Ordinal.Equals(Path.GetFileName(fullPath), expectedName))
                return FailTransactionCorrupt<DpoTransactionRecord>();
            var expectedHash = ComputeTransactionRecordSha256(record);
            return expectedHash.IsSuccess
                   && StringComparer.Ordinal.Equals(
                       expectedHash.Value,
                       record.RecordSha256)
                ? OperationResult<DpoTransactionRecord>.Ok(record)
                : FailTransactionCorrupt<DpoTransactionRecord>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FailTransactionCorrupt<DpoTransactionRecord>();
        }
    }

    private OperationResult<bool> WriteTransaction(DpoTransactionRecord record)
    {
        if (!ValidateTransactionRecord(record))
            return FailTransactionCorrupt<bool>();
        var recordHash = ComputeTransactionRecordSha256(record);
        if (!recordHash.IsSuccess || recordHash.Value is null)
            return recordHash.MapError<bool>();
        var canonical = record with
        {
            RecordSha256 = recordHash.Value,
        };
        if (!ValidateTransactionRecord(canonical))
            return FailTransactionCorrupt<bool>();
        var path = TransactionPath(canonical.TransactionId);
        var temporary = Path.Combine(
            _storageRoot!,
            $".{canonical.TransactionId}.{Guid.NewGuid():N}.transaction.tmp");
        try
        {
            using (var stream = OpenPrivateFile(
                temporary,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                FileOptions.WriteThrough))
            {
                using (var bounded = new BoundedTransactionWriteStream(
                           stream,
                           hash: null,
                           _maximumTransactionRecordBytes))
                using (var writer = new Utf8JsonWriter(bounded))
                {
                    WriteTransactionRecordJson(writer, canonical);
                    writer.Flush();
                    if (bounded.BytesWritten <= 0)
                        throw new TransactionRecordCapacityException();
                }
                stream.Flush(flushToDisk: true);
                if (!OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, temporary)
                    || !HasSafeUnixFileMode(temporary))
                {
                    throw new IOException("DPO transaction staging path changed.");
                }
            }
            File.Move(temporary, path, overwrite: true);
            var readback = ReadTransactionPath(path);
            return readback.IsSuccess
                ? OperationResult<bool>.Ok(true)
                : readback.MapError<bool>();
        }
        catch (TransactionRecordCapacityException)
        {
            DeleteTemporaryFile(temporary);
            return FailCapacity<bool>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            DeleteTemporaryFile(temporary);
            return FailUnavailable<bool>();
        }
    }

    private OperationResult<string> ComputeTransactionRecordSha256(
        DpoTransactionRecord record)
    {
        if (!HasBoundedTransactionFields(record))
            return FailCapacity<string>();
        try
        {
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            using var bounded = new BoundedTransactionWriteStream(
                destination: null,
                hash,
                _maximumTransactionRecordBytes);
            using (var writer = new Utf8JsonWriter(bounded))
            {
                WriteTransactionRecordJson(
                    writer,
                    record with { RecordSha256 = "" });
                writer.Flush();
            }
            if (bounded.BytesWritten <= 0)
                return FailCapacity<string>();
            var digest = Convert.ToHexString(hash.GetHashAndReset())
                .ToLowerInvariant();
            using var finalBounded = new BoundedTransactionWriteStream(
                destination: null,
                hash: null,
                _maximumTransactionRecordBytes);
            using (var finalWriter = new Utf8JsonWriter(finalBounded))
            {
                WriteTransactionRecordJson(
                    finalWriter,
                    record with { RecordSha256 = digest });
                finalWriter.Flush();
            }
            return OperationResult<string>.Ok(digest);
        }
        catch (TransactionRecordCapacityException)
        {
            return FailCapacity<string>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FailTransactionCorrupt<string>();
        }
    }

    private static void WriteTransactionRecordJson(
        Utf8JsonWriter writer,
        DpoTransactionRecord record)
    {
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", record.SchemaVersion);
        writer.WriteString("transactionId", record.TransactionId);
        writer.WriteString("pairBatchSha256", record.PairBatchSha256);
        writer.WriteString("preparedAt", record.PreparedAt);
        writer.WriteNumber("state", (int)record.State);
        if (record.SchemaVersion >= TransactionSchemaVersion)
        {
            writer.WriteNumber("kind", (int)record.Kind);
            writer.WritePropertyName("externalConsumption");
            if (record.ExternalConsumption is null)
                writer.WriteNullValue();
            else
                WriteExternalConsumptionRecordJson(writer, record.ExternalConsumption);
        }
        writer.WriteStartArray("pairs");
        foreach (var pair in record.Pairs)
        {
            writer.WriteStartObject();
            writer.WriteString("pairId", pair.PairId);
            writer.WriteString("domain", pair.Domain);
            if (record.SchemaVersion >= AtomicGroupTransactionSchemaVersion)
            {
                writer.WriteString("groupId", pair.GroupId ?? string.Empty);
                writer.WriteNumber("groupOrdinal", pair.GroupOrdinal);
                writer.WritePropertyName("groupRole");
                if (pair.GroupRole is null)
                    writer.WriteNullValue();
                else
                    writer.WriteNumberValue((int)pair.GroupRole.Value);
            }
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteStartArray("domains");
        foreach (var domain in record.Domains)
        {
            writer.WriteStartObject();
            writer.WriteString("domain", domain.Domain);
            writer.WriteNumber("state", (int)domain.State);
            writer.WritePropertyName("seed");
            if (domain.Seed is null)
            {
                writer.WriteNullValue();
            }
            else
            {
                WriteCandidateSeedJson(writer, domain.Seed);
            }
            writer.WriteString("adapterSha256", domain.AdapterSha256);
            writer.WriteString("trainedAt", domain.TrainedAt);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteStartArray("requests");
        foreach (var request in record.Requests)
        {
            writer.WriteStartObject();
            writer.WriteString("requestId", request.RequestId);
            writer.WriteString("contentSha256", request.ContentSha256);
            writer.WriteString("pairId", request.PairId);
            writer.WriteNumber("state", (int)request.State);
            if (record.SchemaVersion >= AtomicGroupTransactionSchemaVersion)
            {
                writer.WriteString("groupId", request.GroupId ?? string.Empty);
                writer.WriteStartArray("pairIds");
                foreach (var pairId in RequestPairIds(request))
                    writer.WriteStringValue(pairId);
                writer.WriteEndArray();
                writer.WriteStartArray("contentSha256s");
                foreach (var digest in RequestContentDigests(request))
                    writer.WriteStringValue(digest);
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteString("recordSha256", record.RecordSha256);
        writer.WriteEndObject();
    }

    private static void WriteExternalConsumptionRecordJson(
        Utf8JsonWriter writer,
        DpoExternalConsumptionRecord record)
    {
        writer.WriteStartObject();
        writer.WriteNumber("state", (int)record.State);
        writer.WriteNumber("consumerKind", (int)record.ConsumerKind);
        writer.WriteString("groupCommitmentSha256", record.GroupCommitmentSha256);
        writer.WriteString("requestId", record.RequestId);
        writer.WriteString("parentBatchSha256", record.ParentBatchSha256);
        writer.WriteString("parentContentSha256", record.ParentContentSha256);
        writer.WritePropertyName("parentProvenance");
        JsonSerializer.Serialize(writer, record.ParentProvenance, TransactionJsonOptions);
        writer.WriteString(
            "parentProvenanceSha256",
            record.ParentProvenanceSha256);
        writer.WriteString("resultSha256", record.ResultSha256);
        writer.WriteString("checkpointSha256", record.CheckpointSha256);
        writer.WriteString("manifestSha256", record.ManifestSha256);
        writer.WriteEndObject();
    }

    private static void WriteCandidateSeedJson(
        Utf8JsonWriter writer,
        CandidateProvenanceSeed seed)
    {
        writer.WriteStartObject();
        writer.WriteStartObject("trainingSources");
        writer.WriteStartArray("sourceIds");
        foreach (var sourceId in seed.TrainingSources.SourceIds)
            writer.WriteStringValue(sourceId);
        writer.WriteEndArray();
        writer.WriteString("sha256", seed.TrainingSources.Sha256);
        writer.WriteEndObject();
        writer.WriteString("parentAdapterIdentity", seed.ParentAdapterIdentity);
        writer.WriteString("baseCheckpointSha256", seed.BaseCheckpointSha256);
        writer.WriteString("tokenizerSha256", seed.TokenizerSha256);
        writer.WriteStartObject("model");
        writer.WriteNumber("vocabSize", seed.Model.VocabSize);
        writer.WriteNumber("contextLen", seed.Model.ContextLen);
        writer.WriteNumber("hiddenDim", seed.Model.HiddenDim);
        writer.WriteNumber("numLayers", seed.Model.NumLayers);
        writer.WriteNumber("numHeads", seed.Model.NumHeads);
        writer.WriteNumber("numKvHeads", seed.Model.NumKvHeads);
        writer.WriteNumber("ffnHiddenDim", seed.Model.FfnHiddenDim);
        writer.WriteNumber("ropeTheta", seed.Model.RopeTheta);
        writer.WriteNumber("rmsNormEps", seed.Model.RmsNormEps);
        writer.WriteString("sha256", seed.Model.Sha256);
        writer.WriteEndObject();
        writer.WriteStartObject("lora");
        writer.WriteNumber("rank", seed.Lora.Rank);
        writer.WriteNumber("alpha", seed.Lora.Alpha);
        writer.WriteNumber("scaling", seed.Lora.Scaling);
        writer.WriteNumber("dropout", seed.Lora.Dropout);
        writer.WriteStartArray("orderedTargetModules");
        foreach (var module in seed.Lora.OrderedTargetModules)
            writer.WriteStringValue(module);
        writer.WriteEndArray();
        writer.WriteString("sha256", seed.Lora.Sha256);
        writer.WriteEndObject();
        writer.WriteNumber("trainingExamples", seed.TrainingExamples);
        writer.WriteString(
            "trainingExampleBatchSha256",
            seed.TrainingExampleBatchSha256);
        writer.WriteEndObject();
    }

    private static bool HasBoundedTransactionFields(DpoTransactionRecord record)
    {
        if (record is null
            || record.Pairs is null
            || record.Domains is null
            || record.Requests is null
            || record.Pairs.Length > MaximumTransactionPairCount
            || record.Domains.Length > MaximumTransactionDomainCount
            || record.Requests.Length > MaximumTransactionPairCount)
        {
            return false;
        }

        long aggregateBytes = 0;
        bool Add(string? value)
        {
            if (value is null)
                return false;
            var bytes = Encoding.UTF8.GetByteCount(value);
            if (bytes > MaximumFieldBytes
                || bytes > MaximumRecordBytes - aggregateBytes)
            {
                return false;
            }
            aggregateBytes += bytes;
            return true;
        }

        try
        {
            if (!Add(record.TransactionId)
                || !Add(record.PairBatchSha256)
                || !Add(record.RecordSha256))
            {
                return false;
            }
            foreach (var pair in record.Pairs)
            {
                if (pair is null
                    || !Add(pair.PairId)
                    || !Add(pair.Domain)
                    || record.SchemaVersion >= AtomicGroupTransactionSchemaVersion
                        && !Add(pair.GroupId))
                    return false;
            }
            foreach (var request in record.Requests)
            {
                if (request is null
                    || !Add(request.RequestId)
                    || !Add(request.ContentSha256)
                    || !Add(request.PairId)
                    || record.SchemaVersion >= AtomicGroupTransactionSchemaVersion
                        && (!Add(request.GroupId)
                            || RequestPairIds(request).Count > 2
                            || RequestContentDigests(request).Count > 2
                            || RequestPairIds(request).Any(value => !Add(value))
                            || RequestContentDigests(request).Any(value => !Add(value))))
                {
                    return false;
                }
            }
            foreach (var domain in record.Domains)
            {
                if (domain is null
                    || !Add(domain.Domain)
                    || !Add(domain.AdapterSha256))
                {
                    return false;
                }
                if (domain.Seed is not { } seed)
                    continue;
                if (seed.TrainingSources is null
                    || seed.TrainingSources.SourceIds is null
                    || seed.Model is null
                    || seed.Lora is null
                    || seed.Lora.OrderedTargetModules is null
                    || seed.TrainingSources.SourceIds.Count
                        > MaximumTransactionSourceIdCount
                    || seed.Lora.OrderedTargetModules.Count
                        > MaximumTransactionTargetModuleCount
                    || !Add(seed.TrainingSources.Sha256)
                    || !Add(seed.ParentAdapterIdentity)
                    || !Add(seed.BaseCheckpointSha256)
                    || !Add(seed.TokenizerSha256)
                    || !Add(seed.Model.Sha256)
                    || !Add(seed.Lora.Sha256)
                    || !Add(seed.TrainingExampleBatchSha256))
                {
                    return false;
                }
                foreach (var sourceId in seed.TrainingSources.SourceIds)
                {
                    if (!Add(sourceId))
                        return false;
                }
                foreach (var module in seed.Lora.OrderedTargetModules)
                {
                    if (!Add(module))
                        return false;
                }
            }
            if (record.SchemaVersion >= TransactionSchemaVersion
                && record.ExternalConsumption is { } external
                && (!Add(external.GroupCommitmentSha256)
                    || !Add(external.RequestId)
                    || !Add(external.ParentBatchSha256)
                    || !Add(external.ParentContentSha256)
                    || !Add(external.ParentProvenanceSha256)
                    || !Add(external.ResultSha256)
                    || !Add(external.CheckpointSha256)
                    || !Add(external.ManifestSha256)))
            {
                return false;
            }
            return true;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool HasBoundedTransactionJsonTokens(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length <= 0 || (long)bytes.Length > MaximumRecordBytes)
            return false;
        try
        {
            var reader = new Utf8JsonReader(bytes, new JsonReaderOptions
            {
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 64,
            });
            var arrays = new Stack<TransactionJsonArrayFrame>();
            string? propertyName = null;
            var tokens = 0;
            while (reader.Read())
            {
                if (++tokens > 1_000_000)
                    return false;
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    if (reader.ValueSpan.Length > 128)
                        return false;
                    propertyName = reader.GetString();
                    continue;
                }
                if (reader.TokenType == JsonTokenType.String
                    && reader.ValueSpan.Length > MaximumFieldBytes)
                {
                    return false;
                }
                if (arrays.Count != 0
                    && reader.CurrentDepth == arrays.Peek().Depth + 1
                    && reader.TokenType != JsonTokenType.EndArray)
                {
                    var frame = arrays.Pop();
                    frame.Count++;
                    if (frame.Count > MaximumJsonArrayCount(frame.Name))
                        return false;
                    arrays.Push(frame);
                }
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    arrays.Push(new TransactionJsonArrayFrame(
                        propertyName ?? string.Empty,
                        reader.CurrentDepth,
                        0));
                }
                else if (reader.TokenType == JsonTokenType.EndArray)
                {
                    if (arrays.Count == 0
                        || arrays.Peek().Depth != reader.CurrentDepth)
                    {
                        return false;
                    }
                    arrays.Pop();
                }
                propertyName = null;
            }
            return arrays.Count == 0 && reader.BytesConsumed == bytes.Length;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static int MaximumJsonArrayCount(string name)
        => name switch
        {
            "pairs" or "requests" => MaximumTransactionPairCount,
            "domains" => MaximumTransactionDomainCount,
            "entries" => MaximumReplayLedgerEntryCount,
            "sourceIds" => MaximumTransactionSourceIdCount,
            "orderedTargetModules" => MaximumTransactionTargetModuleCount,
            _ => MaximumTransactionSourceIdCount,
        };

    private static bool ValidateTransactionRecord(DpoTransactionRecord record)
    {
        if (record.SchemaVersion is not (
                LegacyTransactionSchemaVersion or
                AtomicGroupTransactionSchemaVersion or
                TransactionSchemaVersion)
            || !TrainingSourceProvenance.IsCanonicalSha256(record.TransactionId)
            || !TrainingSourceProvenance.IsCanonicalSha256(record.PairBatchSha256)
            || record.PreparedAt == default
            || record.PreparedAt.Offset != TimeSpan.Zero
            || !Enum.IsDefined(record.State)
            || !Enum.IsDefined(record.Kind)
            || record.SchemaVersion < TransactionSchemaVersion
                && (record.Kind != DpoTransactionKind.AdapterPublication
                    || record.ExternalConsumption is not null)
            || record.SchemaVersion >= TransactionSchemaVersion
                && record.Kind == DpoTransactionKind.AdapterPublication
                && record.ExternalConsumption is not null
            || record.SchemaVersion >= TransactionSchemaVersion
                && record.Kind == DpoTransactionKind.ExternalConsumption
                && record.ExternalConsumption is null
            || record.Pairs is null
            || record.Domains is null
            || record.Requests is null
            || !HasBoundedTransactionFields(record)
            || record.Pairs.Length is <= 0 or > MaximumTransactionPairCount
            || record.Domains.Length is <= 0 or > MaximumTransactionDomainCount
            || record.Pairs.Any(value => !IsValidTransactionPair(
                value,
                record.SchemaVersion))
            || record.Domains.Any(static value =>
                string.IsNullOrWhiteSpace(value.Domain)
                || !Enum.IsDefined(value.State)
                || value.TrainedAt != default && value.TrainedAt.Offset != TimeSpan.Zero)
            || record.Requests.Any(value => !IsValidTransactionRequest(
                value,
                record.SchemaVersion))
            || !HasCanonicalTransactionPairOrder(record.Pairs)
            || !record.Domains.Select(static value => value.Domain)
                .SequenceEqual(
                    record.Domains.Select(static value => value.Domain)
                        .OrderBy(static value => value, StringComparer.Ordinal),
                    StringComparer.Ordinal)
            || !record.Requests.Select(static value => value.RequestId)
                .SequenceEqual(
                    record.Requests.Select(static value => value.RequestId)
                        .OrderBy(static value => value, StringComparer.Ordinal),
                    StringComparer.Ordinal)
            || record.Pairs.Select(static value => value.PairId)
                .Distinct(StringComparer.Ordinal).Count() != record.Pairs.Length
            || record.Pairs.Where(static pair => !string.IsNullOrEmpty(pair.GroupId))
                .GroupBy(static pair => pair.GroupId!, StringComparer.Ordinal)
                .Any(static group => group.Count() != 2
                    || !group.Select(static pair => pair.GroupOrdinal)
                        .SequenceEqual([0, 1]))
            || record.Domains.Select(static value => value.Domain)
                .Distinct(StringComparer.Ordinal).Count() != record.Domains.Length
            || record.Requests.Select(static value => value.RequestId)
                .Distinct(StringComparer.Ordinal).Count() != record.Requests.Length
            || record.RecordSha256.Length != 0
                && !TrainingSourceProvenance.IsCanonicalSha256(
                    record.RecordSha256))
        {
            return false;
        }

        if (record.Kind == DpoTransactionKind.ExternalConsumption)
            return HasCanonicalExternalTransactionState(record);

        var receipt = TrainingSourceProvenance.BuildDpoPublicationReceipt(
            record.Pairs.Select(static value => value.PairId)
                .OrderBy(static value => value, StringComparer.Ordinal),
            record.Domains.Select(static value => value.Domain));
        return receipt.IsSuccess
               && receipt.Value is not null
               && StringComparer.Ordinal.Equals(
                   receipt.Value.TransactionId,
                   record.TransactionId)
               && StringComparer.Ordinal.Equals(
                   receipt.Value.PairBatchSha256,
                   record.PairBatchSha256)
               && record.Domains.All(static value =>
                   value.Seed is null || IsCanonicalSeed(value.Seed))
               && HasCanonicalTransactionState(record);
    }

    private static bool HasCanonicalExternalTransactionState(
        DpoTransactionRecord record)
    {
        var external = record.ExternalConsumption;
        if (record.SchemaVersion != TransactionSchemaVersion
            || external is null
            || !Enum.IsDefined(external.State)
            || !Enum.IsDefined(external.ConsumerKind)
            || external.ConsumerKind !=
                DpoExternalConsumerKind.DesignConversionDualTargetTeacherAggregate
            || record.Pairs.Length != 2
            || record.Domains.Length != 1
            || record.Requests.Length != 1
            || record.Domains[0] is not
                { State: DpoTransactionDomainState.Prepared, Seed: null }
            || record.Domains[0].AdapterSha256.Length != 0
            || record.Domains[0].TrainedAt != default
            || record.Pairs.Any(pair => !StringComparer.Ordinal.Equals(
                pair.GroupId,
                external.GroupCommitmentSha256))
            || !record.Pairs.Select(static pair => pair.GroupOrdinal)
                .SequenceEqual([0, 1])
            || !TrainingSourceProvenance.IsCanonicalSha256(
                external.GroupCommitmentSha256)
            || !StringComparer.Ordinal.Equals(
                record.PairBatchSha256,
                external.GroupCommitmentSha256)
            || !StringComparer.Ordinal.Equals(
                record.TransactionId,
                ComputeExternalTransactionId(
                    external.GroupCommitmentSha256,
                    record.Domains[0].Domain))
            || !StringComparer.Ordinal.Equals(
                record.Requests[0].RequestId,
                external.RequestId)
            || !StringComparer.Ordinal.Equals(
                record.Requests[0].GroupId,
                external.GroupCommitmentSha256)
            || !RequestPairIds(record.Requests[0]).SequenceEqual(
                record.Pairs.Select(static pair => pair.PairId),
                StringComparer.Ordinal)
            || RequestContentDigests(record.Requests[0]).Count != 2)
        {
            return false;
        }

        var beforeStart = external.State is
            DpoExternalConsumptionState.Reserved
            or DpoExternalConsumptionState.AggregateBound;
        var hasParent = external.ParentProvenance is not null
            && TrainingSourceProvenance.IsCanonicalSha256(
                external.ParentBatchSha256)
            && TrainingSourceProvenance.IsCanonicalSha256(
                external.ParentContentSha256)
            && IsPersistableProvenance(external.ParentProvenance)
            && StringComparer.Ordinal.Equals(
                external.ParentProvenance.RequestId,
                external.RequestId)
            && external.ParentProvenance.InvocationCount == 1
            && StringComparer.Ordinal.Equals(
                external.ParentProvenance.TrainingContentSha256,
                external.ParentContentSha256)
            && TrainingSourceProvenance.IsCanonicalSha256(
                external.ParentProvenanceSha256)
            && StringComparer.Ordinal.Equals(
                external.ParentProvenanceSha256,
                ComputeExternalParentProvenanceSha256(
                    external.ParentProvenance));
        if ((external.State == DpoExternalConsumptionState.Reserved
                && (external.ParentProvenance is not null
                    || external.ParentBatchSha256.Length != 0
                    || external.ParentContentSha256.Length != 0
                    || external.ParentProvenanceSha256.Length != 0))
            || (external.State != DpoExternalConsumptionState.Reserved
                && !hasParent))
        {
            return false;
        }

        var artifactReady = external.State is
            DpoExternalConsumptionState.ExternalArtifactReady
            or DpoExternalConsumptionState.Committed;
        if (artifactReady !=
            (TrainingSourceProvenance.IsCanonicalSha256(external.ResultSha256)
             && TrainingSourceProvenance.IsCanonicalSha256(
                 external.CheckpointSha256)
             && TrainingSourceProvenance.IsCanonicalSha256(
                 external.ManifestSha256)))
        {
            return false;
        }
        if (!artifactReady
            && (external.ResultSha256.Length != 0
                || external.CheckpointSha256.Length != 0
                || external.ManifestSha256.Length != 0))
        {
            return false;
        }

        var expectedRequestState = beforeStart
            ? DpoTransactionRequestState.Reserved
            : DpoTransactionRequestState.Consumed;
        var expectedTransactionState = external.State switch
        {
            DpoExternalConsumptionState.Reserved => DpoTransactionState.Prepared,
            DpoExternalConsumptionState.AggregateBound => DpoTransactionState.Active,
            DpoExternalConsumptionState.MutationStarted => DpoTransactionState.Active,
            DpoExternalConsumptionState.ExternalArtifactReady =>
                DpoTransactionState.ExternalConsumptionComplete,
            DpoExternalConsumptionState.Committed => DpoTransactionState.Committed,
            _ => (DpoTransactionState)(-1),
        };
        return record.Requests[0].State == expectedRequestState
               && record.State == expectedTransactionState;
    }

    private static bool IsValidTransactionPair(
        DpoTransactionPair pair,
        int schemaVersion)
    {
        if (pair is null
            || !TrainingSourceProvenance.IsCanonicalSha256(pair.PairId)
            || string.IsNullOrWhiteSpace(pair.Domain))
        {
            return false;
        }
        if (schemaVersion == LegacyTransactionSchemaVersion)
            return string.IsNullOrEmpty(pair.GroupId);
        var groupId = pair.GroupId ?? string.Empty;
        return groupId.Length == 0
            ? pair.GroupOrdinal == -1 && pair.GroupRole is null
            : TrainingSourceProvenance.IsCanonicalSha256(groupId)
              && pair.GroupOrdinal is 0 or 1
              && pair.GroupRole == (pair.GroupOrdinal == 0
                  ? DpoPairAtomicGroupRole.CriticUnit
                  : DpoPairAtomicGroupRole.ImproverUnit);
    }

    private static bool IsValidTransactionRequest(
        DpoTransactionRequest request,
        int schemaVersion)
    {
        if (request is null
            || string.IsNullOrWhiteSpace(request.RequestId)
            || !TrainingSourceProvenance.IsCanonicalSha256(request.ContentSha256)
            || !Enum.IsDefined(request.State))
        {
            return false;
        }
        if (schemaVersion == LegacyTransactionSchemaVersion)
            return TrainingSourceProvenance.IsCanonicalSha256(request.PairId);
        var groupId = request.GroupId ?? string.Empty;
        var pairIds = RequestPairIds(request);
        var contents = RequestContentDigests(request);
        if (pairIds.Count != contents.Count
            || pairIds.Any(static value =>
                !TrainingSourceProvenance.IsCanonicalSha256(value))
            || contents.Any(static value =>
                !TrainingSourceProvenance.IsCanonicalSha256(value)))
        {
            return false;
        }
        return groupId.Length == 0
            ? pairIds.Count == 1
              && StringComparer.Ordinal.Equals(request.PairId, pairIds[0])
              && StringComparer.Ordinal.Equals(request.ContentSha256, contents[0])
            : pairIds.Count == 2
              && TrainingSourceProvenance.IsCanonicalSha256(groupId)
              && request.PairId.Length == 0
              && StringComparer.Ordinal.Equals(request.ContentSha256, groupId);
    }

    private static bool HasCanonicalTransactionPairOrder(
        IReadOnlyList<DpoTransactionPair> pairs)
    {
        var expected = pairs.OrderBy(static pair =>
                string.IsNullOrEmpty(pair.GroupId) ? pair.PairId : pair.GroupId,
                StringComparer.Ordinal)
            .ThenBy(static pair => pair.GroupOrdinal)
            .Select(static pair => pair.PairId);
        return pairs.Select(static pair => pair.PairId)
            .SequenceEqual(expected, StringComparer.Ordinal);
    }

    private static bool HasCanonicalTransactionState(
        DpoTransactionRecord record)
    {
        var domains = record.Domains.ToDictionary(
            static value => value.Domain,
            StringComparer.Ordinal);
        var pairs = record.Pairs.ToDictionary(
            static value => value.PairId,
            StringComparer.Ordinal);
        if (record.Pairs.Any(value => !domains.ContainsKey(value.Domain))
            || record.Requests.Any(value => RequestPairIds(value).Any(
                pairId => !pairs.ContainsKey(pairId))))
        {
            return false;
        }

        foreach (var domain in record.Domains)
        {
            var candidateExists = domain.State is
                DpoTransactionDomainState.CandidateReady
                or DpoTransactionDomainState.Published;
            if (domain.State == DpoTransactionDomainState.Prepared)
            {
                if (domain.Seed is not null
                    || domain.AdapterSha256.Length != 0
                    || domain.TrainedAt != default)
                {
                    return false;
                }
            }
            else if (domain.State is DpoTransactionDomainState.CandidatePrepared
                     or DpoTransactionDomainState.MutationStarted)
            {
                if (domain.Seed is null
                    || domain.AdapterSha256.Length != 0
                    || domain.TrainedAt != default)
                {
                    return false;
                }
            }
            else if (!candidateExists
                     || domain.Seed is null
                     || !TrainingSourceProvenance.IsCanonicalSha256(
                         domain.AdapterSha256)
                     || domain.TrainedAt == default
                     || domain.TrainedAt.Offset != TimeSpan.Zero)
            {
                return false;
            }
        }

        foreach (var request in record.Requests)
        {
            var mutationStarted = RequestPairIds(request).Any(pairId =>
                domains[pairs[pairId].Domain].State is
                    DpoTransactionDomainState.MutationStarted
                    or DpoTransactionDomainState.CandidateReady
                    or DpoTransactionDomainState.Published);
            if ((mutationStarted
                    && request.State != DpoTransactionRequestState.Consumed)
                || (!mutationStarted
                    && request.State != DpoTransactionRequestState.Reserved))
            {
                return false;
            }
        }

        var allPrepared = record.Domains.All(static value =>
            value.State == DpoTransactionDomainState.Prepared);
        var allPublished = record.Domains.All(static value =>
            value.State == DpoTransactionDomainState.Published);
        return record.State switch
        {
            DpoTransactionState.Prepared => allPrepared,
            DpoTransactionState.Active => !allPrepared && !allPublished,
            DpoTransactionState.PublicationComplete => allPublished,
            DpoTransactionState.Committed => allPublished,
            _ => false,
        };
    }

    private static bool IsCanonicalSeed(CandidateProvenanceSeed seed)
    {
        if (seed is null
            || seed.TrainingExamples <= 0
            || seed.TrainingSources is null
            || seed.Model is null
            || seed.Lora is null
            || seed.Lora.OrderedTargetModules is null
            || !IsCanonicalParentAdapterIdentity(seed.ParentAdapterIdentity)
            || !TrainingSourceProvenance.IsCanonicalSha256(
                seed.TrainingExampleBatchSha256)
            || !TrainingSourceProvenance.IsCanonicalSha256(
                seed.BaseCheckpointSha256)
            || !TrainingSourceProvenance.IsCanonicalSha256(seed.TokenizerSha256))
        {
            return false;
        }
        var manifest = TrainingSourceProvenance.BuildManifest(
            seed.TrainingSources.SourceIds);
        if (!manifest.IsSuccess || manifest.Value is null
            || !StringComparer.Ordinal.Equals(
                manifest.Value.Sha256,
                seed.TrainingSources.Sha256))
        {
            return false;
        }
        try
        {
            var model = TrainingSourceProvenance.Identify(
                new TransformerModel.Config.ModelConfig
                {
                    VocabSize = seed.Model.VocabSize,
                    ContextLen = seed.Model.ContextLen,
                    HiddenDim = seed.Model.HiddenDim,
                    NumLayers = seed.Model.NumLayers,
                    NumHeads = seed.Model.NumHeads,
                    NumKvHeads = seed.Model.NumKvHeads,
                    FfnHiddenDim = seed.Model.FfnHiddenDim,
                    RopeTheta = seed.Model.RopeTheta,
                    RmsNormEps = seed.Model.RmsNormEps,
                });
            var lora = TrainingSourceProvenance.Identify(new LoRA.LoraConfig
            {
                Rank = seed.Lora.Rank,
                Alpha = seed.Lora.Alpha,
                Dropout = seed.Lora.Dropout,
                TargetModules = seed.Lora.OrderedTargetModules.ToArray(),
            });
            return model == seed.Model && LoraIdentityEquals(lora, seed.Lora);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static OperationResult<CandidateProvenanceSeed> CanonicalizeSeed(
        CandidateProvenanceSeed seed)
    {
        if (!IsCanonicalSeed(seed))
            return InvalidCandidate<CandidateProvenanceSeed>();
        var manifest = TrainingSourceProvenance.BuildManifest(
            seed.TrainingSources.SourceIds);
        if (!manifest.IsSuccess || manifest.Value is null)
            return manifest.MapError<CandidateProvenanceSeed>();
        try
        {
            var model = TrainingSourceProvenance.Identify(
                new TransformerModel.Config.ModelConfig
                {
                    VocabSize = seed.Model.VocabSize,
                    ContextLen = seed.Model.ContextLen,
                    HiddenDim = seed.Model.HiddenDim,
                    NumLayers = seed.Model.NumLayers,
                    NumHeads = seed.Model.NumHeads,
                    NumKvHeads = seed.Model.NumKvHeads,
                    FfnHiddenDim = seed.Model.FfnHiddenDim,
                    RopeTheta = seed.Model.RopeTheta,
                    RmsNormEps = seed.Model.RmsNormEps,
                });
            var lora = TrainingSourceProvenance.Identify(new LoRA.LoraConfig
            {
                Rank = seed.Lora.Rank,
                Alpha = seed.Lora.Alpha,
                Dropout = seed.Lora.Dropout,
                TargetModules = seed.Lora.OrderedTargetModules.ToArray(),
            });
            return OperationResult<CandidateProvenanceSeed>.Ok(
                new CandidateProvenanceSeed(
                    manifest.Value,
                    seed.ParentAdapterIdentity,
                    seed.BaseCheckpointSha256,
                    seed.TokenizerSha256,
                    model,
                    lora,
                    seed.TrainingExamples,
                    seed.TrainingExampleBatchSha256));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidCandidate<CandidateProvenanceSeed>();
        }
    }

    private static bool CandidateSeedEquals(
        CandidateProvenanceSeed left,
        CandidateProvenanceSeed right)
        => left.TrainingExamples == right.TrainingExamples
           && StringComparer.Ordinal.Equals(
               left.ParentAdapterIdentity,
               right.ParentAdapterIdentity)
           && StringComparer.Ordinal.Equals(
               left.TrainingExampleBatchSha256,
               right.TrainingExampleBatchSha256)
           && StringComparer.Ordinal.Equals(
               left.BaseCheckpointSha256,
               right.BaseCheckpointSha256)
           && StringComparer.Ordinal.Equals(
               left.TokenizerSha256,
               right.TokenizerSha256)
           && left.Model == right.Model
           && LoraIdentityEquals(left.Lora, right.Lora)
           && StringComparer.Ordinal.Equals(
               left.TrainingSources.Sha256,
               right.TrainingSources.Sha256)
           && left.TrainingSources.SourceIds.SequenceEqual(
               right.TrainingSources.SourceIds,
               StringComparer.Ordinal);

    private static bool LoraIdentityEquals(
        LoraConfigurationIdentity left,
        LoraConfigurationIdentity right)
        => left.Rank == right.Rank
           && left.Alpha.Equals(right.Alpha)
           && left.Scaling.Equals(right.Scaling)
           && left.Dropout.Equals(right.Dropout)
           && StringComparer.Ordinal.Equals(left.Sha256, right.Sha256)
           && left.OrderedTargetModules.SequenceEqual(
               right.OrderedTargetModules,
               StringComparer.Ordinal);

    private ModelAdapterProvenance BuildCandidateProvenance(
        DpoTransactionRecord transaction,
        CandidateProvenanceSeed seed,
        string adapterSha256)
    {
        var receipt = TrainingSourceProvenance.BuildDpoPublicationReceipt(
            transaction.Pairs.Select(static value => value.PairId)
                .OrderBy(static value => value, StringComparer.Ordinal),
            transaction.Domains.Select(static value => value.Domain));
        if (!receipt.IsSuccess || receipt.Value is null)
            throw new InvalidDataException(TransactionCorruptMessage);
        return new ModelAdapterProvenance(
            seed.TrainingSources,
            adapterSha256,
            seed.BaseCheckpointSha256,
            seed.TokenizerSha256,
            seed.Model,
            seed.Lora,
            receipt.Value);
    }

    private static bool CandidateProvenanceEquals(
        ModelAdapterProvenance left,
        ModelAdapterProvenance right)
        => left.TrainingSources is not null
           && right.TrainingSources is not null
           && left.TrainingSources.SourceIds is not null
           && right.TrainingSources.SourceIds is not null
           && left.Model == right.Model
           && left.Lora is not null
           && right.Lora is not null
           && left.Lora.OrderedTargetModules is not null
           && right.Lora.OrderedTargetModules is not null
           && StringComparer.Ordinal.Equals(
               left.TrainingSources.Sha256,
               right.TrainingSources.Sha256)
           && left.TrainingSources.SourceIds.SequenceEqual(
               right.TrainingSources.SourceIds,
               StringComparer.Ordinal)
           && StringComparer.Ordinal.Equals(
               left.AdapterSha256,
               right.AdapterSha256)
           && StringComparer.Ordinal.Equals(
               left.BaseCheckpointSha256,
               right.BaseCheckpointSha256)
           && StringComparer.Ordinal.Equals(
               left.TokenizerSha256,
               right.TokenizerSha256)
           && left.Lora.Rank == right.Lora.Rank
           && left.Lora.Alpha.Equals(right.Lora.Alpha)
           && left.Lora.Scaling.Equals(right.Lora.Scaling)
           && left.Lora.Dropout.Equals(right.Lora.Dropout)
           && StringComparer.Ordinal.Equals(left.Lora.Sha256, right.Lora.Sha256)
           && left.Lora.OrderedTargetModules.SequenceEqual(
               right.Lora.OrderedTargetModules,
               StringComparer.Ordinal)
           && DpoReceiptEquals(left.DpoPublication, right.DpoPublication);

    private static bool DpoReceiptEquals(
        DpoAdapterPublicationReceipt? left,
        DpoAdapterPublicationReceipt? right)
        => left is not null
           && right is not null
           && left.OrderedPairIds is not null
           && right.OrderedPairIds is not null
           && left.OrderedDomains is not null
           && right.OrderedDomains is not null
           && StringComparer.Ordinal.Equals(
               left.TransactionId,
               right.TransactionId)
           && StringComparer.Ordinal.Equals(
               left.PairBatchSha256,
               right.PairBatchSha256)
           && left.OrderedPairIds.SequenceEqual(
               right.OrderedPairIds,
               StringComparer.Ordinal)
           && left.OrderedDomains.SequenceEqual(
               right.OrderedDomains,
               StringComparer.Ordinal);

    private static bool HasNoDuplicateProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name)
                    || !HasNoDuplicateProperties(property.Value))
                {
                    return false;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (!HasNoDuplicateProperties(item))
                    return false;
            }
        }
        return true;
    }

    private string TransactionPath(string transactionId)
        => Path.Combine(_storageRoot!, transactionId + TransactionSuffix);

    private string CandidatePath(string transactionId, string domain)
        => Path.Combine(
            _storageRoot!,
            $".dpo-candidate-{transactionId}-{DomainKey(domain)}.lora");

    private string StagingPath(string transactionId, string domain)
        => Path.Combine(
            _storageRoot!,
            $".dpo-candidate-{transactionId}-{DomainKey(domain)}.stage");

    private string CandidateLockPath(string transactionId, string domain)
        => Path.Combine(
            _storageRoot!,
            $".dpo-candidate-{transactionId}-{DomainKey(domain)}.lock");

    private static string DomainKey(string domain)
        => Convert.ToHexString(SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(domain)))
            .ToLowerInvariant();

    private static OperationResult<bool> InvalidTeacherResume()
        => OperationResult<bool>.Fail(
            "The teacher batch does not match its exact durable DPO transaction reservation.",
            "DPO_TRANSACTION_TEACHER_BINDING_MISMATCH");

    private static bool IsCanonicalParentAdapterIdentity(string identity)
        => StringComparer.Ordinal.Equals(identity, NoParentAdapterIdentity)
           || TrainingSourceProvenance.IsCanonicalSha256(identity);

    private static OperationResult<T> RecoveryRequired<T>()
        => OperationResult<T>.Fail(
            "Durable DPO mutation began without exact recoverable artifact evidence.",
            "DPO_TRANSACTION_RECOVERY_REQUIRED");

    private static OperationResult<T> LegacyContentBindingRejected<T>()
        => OperationResult<T>.Fail(
            "Legacy DPO pair content binding cannot enter a current durable DPO training transaction.",
            "DPO_TRANSACTION_LEGACY_CONTENT_BINDING_REJECTED");

    private static OperationResult<T> InvalidExternalConsumption<T>()
        => OperationResult<T>.Fail(
            "The exact external DPO consumption transaction is invalid.",
            ExternalConsumptionInvalidErrorCode);

    private static OperationResult<T> FailTransactionCapacity<T>()
        => OperationResult<T>.Fail(
            "The bounded durable DPO transaction ledger is full.",
            "DPO_TRANSACTION_CAPACITY_EXCEEDED");

    private static OperationResult<T> FailReplayLedgerCapacity<T>()
        => OperationResult<T>.Fail(
            "The bounded durable DPO replay ledger is full.",
            "DPO_REPLAY_LEDGER_CAPACITY_EXCEEDED");

    private static OperationResult<T> InvalidCandidate<T>()
        => OperationResult<T>.Fail(
            "The DPO transaction candidate is invalid.",
            "DPO_TRANSACTION_CANDIDATE_INVALID");

    private static OperationResult<T> FailTransactionCorrupt<T>()
        => OperationResult<T>.Fail(
            TransactionCorruptMessage,
            "DPO_TRANSACTION_STORE_CORRUPT");

    internal sealed class TransactionHandle
    {
        private readonly FileDpoPairStore _owner;

        internal TransactionHandle(FileDpoPairStore owner, string transactionId)
        {
            _owner = owner;
            TransactionId = transactionId;
        }

        internal string TransactionId { get; }

        internal OperationResult<TransactionSnapshot> Read()
            => _owner.ReadTransactionSnapshot(TransactionId);

        internal OperationResult<CandidateReservation> PrepareCandidate(
            string domain,
            CandidateProvenanceSeed seed)
            => _owner.PrepareCandidate(TransactionId, domain, seed);

        internal OperationResult<bool> ValidateTeacherBatch(
            string domain,
            DpoTrainingContentBatch trainingContent,
            IReadOnlyList<TrainingTeacherProvenance?> provenance)
            => _owner.ValidateTeacherBatch(
                TransactionId,
                domain,
                trainingContent,
                provenance);

        internal OperationResult<bool> BeginMutation(string domain)
            => _owner.BeginMutation(TransactionId, domain);

        internal OperationResult<TrainedAdapterArtifact> CompleteCandidate(
            string domain)
            => _owner.CompleteCandidate(TransactionId, domain);

        internal OperationResult<bool> RecordPublished(
            string domain,
            ModelAdapterProvenance provenance)
            => _owner.RecordPublished(TransactionId, domain, provenance);

        internal OperationResult<bool> CommitDrain()
            => _owner.CommitDrain(TransactionId);

        internal OperationResult<bool> ReleaseIfUnmutated()
            => _owner.ReleaseIfUnmutated(TransactionId);
    }

    internal sealed class ExternalConsumptionTransactionHandle
    {
        private readonly FileDpoPairStore _owner;

        internal ExternalConsumptionTransactionHandle(
            FileDpoPairStore owner,
            string transactionId)
        {
            _owner = owner;
            TransactionId = transactionId;
        }

        internal string TransactionId { get; }

        internal OperationResult<ExternalConsumptionSnapshot> Read()
            => _owner.ReadExternalConsumption(TransactionId);

        internal OperationResult<bool> BindAggregate(
            DpoTrainingContentBatch trainingContent,
            TrainingTeacherProvenance provenance)
            => _owner.BindExternalAggregate(
                TransactionId,
                trainingContent,
                provenance);

        internal OperationResult<ExternalMutationAuthorization> BeginMutation()
            => _owner.BeginExternalMutation(TransactionId);

        internal OperationResult<ExternalConsumptionCompletionReceipt> Complete(
            ExternalMutationAuthorization authorization,
            DpoExternalConsumptionResult result)
            => _owner.CompleteExternalConsumption(
                TransactionId,
                authorization,
                result);

        internal OperationResult<bool> ReleaseIfUnmutated()
            => _owner.ReleaseExternalIfUnmutated(TransactionId);
    }

    internal sealed class ExternalMutationAuthorization
    {
        private readonly FileDpoPairStore _owner;
        private readonly string _transactionId;
        private int _claimed;

        internal ExternalMutationAuthorization(
            FileDpoPairStore owner,
            string transactionId,
            object mintingKey)
        {
            if (!ReferenceEquals(mintingKey, ExternalConsumptionMintingKey))
                throw new InvalidOperationException("Store minting is required.");
            _owner = owner;
            _transactionId = transactionId;
        }

        internal bool TryClaim(FileDpoPairStore owner, string transactionId)
            => ReferenceEquals(_owner, owner)
               && StringComparer.Ordinal.Equals(_transactionId, transactionId)
               && Interlocked.CompareExchange(ref _claimed, 1, 0) == 0;
    }

    internal sealed class ExternalConsumptionCompletionReceipt
    {
        internal ExternalConsumptionCompletionReceipt(
            DpoExternalConsumptionManifest manifest,
            object mintingKey)
        {
            if (!ReferenceEquals(mintingKey, ExternalConsumptionMintingKey))
                throw new InvalidOperationException("Store minting is required.");
            TransactionId = manifest.TransactionId;
            GroupCommitmentSha256 = manifest.GroupCommitmentSha256;
            RequestId = manifest.RequestId;
            ParentBatchSha256 = manifest.ParentBatchSha256;
            ParentContentSha256 = manifest.ParentContentSha256;
            ParentProvenanceSha256 = manifest.ParentProvenanceSha256;
            OrderedPairIds = Array.AsReadOnly(manifest.OrderedPairIds.ToArray());
            OrderedContentSha256s = Array.AsReadOnly(
                manifest.OrderedContentSha256s.ToArray());
            ConsumerKind = manifest.ConsumerKind;
            ResultSha256 = manifest.ResultSha256;
            CheckpointSha256 = manifest.CheckpointSha256;
            ManifestSha256 = manifest.ManifestSha256;
        }

        internal string TransactionId { get; }
        internal string GroupCommitmentSha256 { get; }
        internal string RequestId { get; }
        internal string ParentBatchSha256 { get; }
        internal string ParentContentSha256 { get; }
        internal string ParentProvenanceSha256 { get; }
        internal IReadOnlyList<string> OrderedPairIds { get; }
        internal IReadOnlyList<string> OrderedContentSha256s { get; }
        internal DpoExternalConsumerKind ConsumerKind { get; }
        internal string ResultSha256 { get; }
        internal string CheckpointSha256 { get; }
        internal string ManifestSha256 { get; }
    }

    internal sealed record ExternalConsumptionSnapshot(
        string TransactionId,
        string GroupCommitmentSha256,
        string RequestId,
        string Domain,
        DpoExternalConsumptionState State,
        string ParentBatchSha256,
        string ParentContentSha256,
        string ParentProvenanceSha256,
        IReadOnlyList<string> OrderedPairIds,
        IReadOnlyList<string> OrderedContentSha256s,
        DpoExternalConsumerKind ConsumerKind,
        string ResultSha256,
        string CheckpointSha256,
        string ManifestSha256);

    internal sealed class CandidateReservation : IDisposable
    {
        private readonly FileDpoPairStore _owner;
        private CandidateLease? _lease;

        internal CandidateReservation(
            FileDpoPairStore owner,
            string stagingPath,
            TrainedAdapterArtifact? existingArtifact,
            CandidateLease? lease)
        {
            _owner = owner;
            StagingPath = stagingPath;
            ExistingArtifact = existingArtifact;
            _lease = lease;
        }

        internal string StagingPath { get; }
        internal TrainedAdapterArtifact? ExistingArtifact { get; }

        internal OperationResult<string> SaveAdapterCheckpoint(LlmModel model)
        {
            ArgumentNullException.ThrowIfNull(model);
            if (ExistingArtifact is not null || _lease is null)
                return InvalidCandidate<string>();
            return _owner.SaveAdapterCheckpointForCandidate(StagingPath, model);
        }

        internal OperationResult<bool> LoadExistingAdapterCheckpoint(
            LlmModel model)
        {
            // WHY: callers that recover an already durable candidate may only
            // load those exact bytes; a fresh lease still means the mutation
            // has not produced an accepted artifact and is therefore invalid.
            ArgumentNullException.ThrowIfNull(model);
            if (ExistingArtifact is null || _lease is not null)
                return InvalidCandidate<bool>();
            return _owner.LoadExistingAdapterCheckpointForCandidate(
                ExistingArtifact.AdapterPath,
                ExistingArtifact.Provenance.AdapterSha256,
                model);
        }

        public void Dispose()
            => Interlocked.Exchange(ref _lease, null)?.Dispose();
    }

    internal sealed record CandidateProvenanceSeed(
        TrainingSourceManifest TrainingSources,
        string ParentAdapterIdentity,
        string BaseCheckpointSha256,
        string TokenizerSha256,
        ModelConfigurationIdentity Model,
        LoraConfigurationIdentity Lora,
        int TrainingExamples,
        string TrainingExampleBatchSha256);

    internal sealed record TransactionPairSnapshot(string PairId, DpoPair Pair);

    internal sealed record TransactionDomainSnapshot(
        string Domain,
        DpoTransactionDomainState State,
        int TrainingExamples,
        DateTimeOffset TrainedAt,
        TrainedAdapterArtifact? Artifact);

    internal sealed record TransactionSnapshot(
        DpoAdapterPublicationReceipt Receipt,
        IReadOnlyList<TransactionPairSnapshot> Pairs,
        IReadOnlyList<TransactionDomainSnapshot> Domains,
        DpoTransactionState State)
    {
        internal bool MutationStarted => Domains.Any(static value =>
            value.State is DpoTransactionDomainState.MutationStarted
                or DpoTransactionDomainState.CandidateReady
                or DpoTransactionDomainState.Published);
    }

    internal enum DpoTransactionState
    {
        Prepared = 0,
        Active = 1,
        PublicationComplete = 2,
        Committed = 3,
        ExternalConsumptionComplete = 4,
    }

    internal enum DpoTransactionKind
    {
        AdapterPublication = 0,
        ExternalConsumption = 1,
    }

    internal enum DpoExternalConsumptionState
    {
        Reserved = 0,
        AggregateBound = 1,
        MutationStarted = 2,
        ExternalArtifactReady = 3,
        Committed = 4,
    }

    internal enum DpoTransactionDomainState
    {
        Prepared = 0,
        CandidatePrepared = 1,
        MutationStarted = 2,
        CandidateReady = 3,
        Published = 4,
    }

    private enum DpoTransactionRequestState
    {
        Reserved = 0,
        Consumed = 1,
    }

    private sealed record DpoTransactionPair(
        string PairId,
        string Domain,
        string? GroupId = null,
        int GroupOrdinal = -1,
        DpoPairAtomicGroupRole? GroupRole = null);

    private sealed record DpoTransactionDomain(
        string Domain,
        DpoTransactionDomainState State,
        CandidateProvenanceSeed? Seed,
        string AdapterSha256,
        DateTimeOffset TrainedAt);

    private sealed record DpoTransactionRequest(
        string RequestId,
        string ContentSha256,
        string PairId,
        DpoTransactionRequestState State,
        string? GroupId = null,
        string[]? PairIds = null,
        string[]? ContentSha256s = null);

    private sealed record DpoTransactionRecord(
        int SchemaVersion,
        string TransactionId,
        string PairBatchSha256,
        DateTimeOffset PreparedAt,
        DpoTransactionState State,
        DpoTransactionPair[] Pairs,
        DpoTransactionDomain[] Domains,
        DpoTransactionRequest[] Requests,
        string RecordSha256,
        DpoTransactionKind Kind = DpoTransactionKind.AdapterPublication,
        DpoExternalConsumptionRecord? ExternalConsumption = null);

    private sealed record DpoExternalConsumptionRecord(
        DpoExternalConsumptionState State,
        DpoExternalConsumerKind ConsumerKind,
        string GroupCommitmentSha256,
        string RequestId,
        string ParentBatchSha256,
        string ParentContentSha256,
        TrainingTeacherProvenance? ParentProvenance,
        string ParentProvenanceSha256,
        string ResultSha256,
        string CheckpointSha256,
        string ManifestSha256);

    internal sealed record DpoExternalConsumptionManifest(
        int SchemaVersion,
        string Context,
        string TransactionId,
        string GroupCommitmentSha256,
        string RequestId,
        string Domain,
        string ParentBatchSha256,
        string ParentContentSha256,
        string ParentProvenanceSha256,
        string[] OrderedPairIds,
        string[] OrderedContentSha256s,
        DpoExternalConsumerKind ConsumerKind,
        string ResultSha256,
        string CheckpointSha256,
        string ManifestSha256);

    private sealed record DpoReplayLedgerEntry(
        string PairId,
        string RequestId,
        string ContentSha256,
        string? GroupId = null,
        string[]? PairIds = null,
        string[]? ContentSha256s = null,
        DpoTransactionKind Kind = DpoTransactionKind.AdapterPublication,
        DpoExternalReplayEvidence? ExternalConsumption = null);

    private sealed record DpoExternalReplayEvidence(
        string TransactionId,
        string Domain,
        string ParentBatchSha256,
        string ParentContentSha256,
        TrainingTeacherProvenance ParentProvenance,
        string ParentProvenanceSha256,
        DpoExternalConsumerKind ConsumerKind,
        string ResultSha256,
        string CheckpointSha256,
        string ManifestSha256);

    private sealed record DpoReplayLedgerRecord(
        int SchemaVersion,
        DpoReplayLedgerEntry[] Entries,
        string RecordSha256);

    private sealed class TransactionJsonArrayFrame(
        string name,
        int depth,
        int count)
    {
        internal string Name { get; } = name;
        internal int Depth { get; } = depth;
        internal int Count { get; set; } = count;
    }

    private sealed class TransactionRecordCapacityException : Exception;

    private sealed class BoundedTransactionWriteStream(
        Stream? destination,
        IncrementalHash? hash,
        long maximumBytes) : IBufferWriter<byte>, IDisposable
    {
        private byte[] _buffer = ArrayPool<byte>.Shared.Rent(16 * 1024);
        internal long BytesWritten { get; private set; }

        public void Advance(int count)
        {
            if (count < 0
                || count > _buffer.Length
                || count > maximumBytes - BytesWritten)
            {
                throw new TransactionRecordCapacityException();
            }
            hash?.AppendData(_buffer.AsSpan(0, count));
            destination?.Write(_buffer.AsSpan(0, count));
            BytesWritten += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            var required = Math.Max(sizeHint, 1);
            var remaining = maximumBytes - BytesWritten;
            if (required > remaining
                || required > MaximumFieldBytes + 1024L)
            {
                throw new TransactionRecordCapacityException();
            }
            if (_buffer.Length < required)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
                _buffer = ArrayPool<byte>.Shared.Rent(required);
            }
            return _buffer.AsMemory(
                0,
                checked((int)Math.Min(_buffer.Length, remaining)));
        }

        public Span<byte> GetSpan(int sizeHint = 0)
            => GetMemory(sizeHint).Span;

        public void Dispose()
        {
            var buffer = Interlocked.Exchange(ref _buffer, []);
            if (buffer.Length != 0)
                ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    internal sealed class CandidateLease(FileStream stream) : IDisposable
    {
        private FileStream? _stream = stream;

        public void Dispose()
        {
            var owned = Interlocked.Exchange(ref _stream, null);
            if (owned is null)
                return;
            try
            {
                if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
                    owned.Unlock(0, 1);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
            }
            owned.Dispose();
        }
    }
}
