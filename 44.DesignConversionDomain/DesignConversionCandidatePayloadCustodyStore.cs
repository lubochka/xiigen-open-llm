// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Diagnostics;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class DesignConversionCandidatePayloadCustodyEntry
{
    private readonly DesignConversionDpoPayloadCustodySecret _secret;

    internal DesignConversionCandidatePayloadCustodyEntry(
        string trainingRecordId,
        string bindingSha256,
        string governanceSha256,
        string promptSha256,
        string chosenResponseSha256,
        string rejectedResponseSha256,
        DesignConversionDpoPayloadCustodySecret secret)
    {
        TrainingRecordId = trainingRecordId;
        BindingSha256 = bindingSha256;
        GovernanceSha256 = governanceSha256;
        PromptSha256 = promptSha256;
        ChosenResponseSha256 = chosenResponseSha256;
        RejectedResponseSha256 = rejectedResponseSha256;
        _secret = secret;
    }

    internal string TrainingRecordId { get; }
    internal string BindingSha256 { get; }
    internal string GovernanceSha256 { get; }
    internal string PromptSha256 { get; }
    internal string ChosenResponseSha256 { get; }
    internal string RejectedResponseSha256 { get; }
    [System.Text.Json.Serialization.JsonIgnore]
    internal DesignConversionDpoPayloadCustodySecret Secret => _secret;

    private string DebuggerDisplay =>
        $"PayloadCustodyEntry(record={TrainingRecordId}, binding={BindingSha256}, raw=[REDACTED])";

    public override string ToString() => DebuggerDisplay;
}

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class DesignConversionCandidatePayloadCustodyDraft
{
    private readonly IReadOnlyList<DesignConversionCandidatePayloadCustodyEntry> _entries;

    internal DesignConversionCandidatePayloadCustodyDraft(
        string transactionId,
        string manifestSha256,
        string bindingSetSha256,
        string authorityRecordId,
        string authorityIdentitySha256,
        IReadOnlyList<DesignConversionCandidatePayloadCustodyEntry> entries)
    {
        TransactionId = transactionId;
        ManifestSha256 = manifestSha256;
        BindingSetSha256 = bindingSetSha256;
        AuthorityRecordId = authorityRecordId;
        AuthorityIdentitySha256 = authorityIdentitySha256;
        _entries = new ImmutableEntryList(entries);
    }

    internal string TransactionId { get; }
    internal string ManifestSha256 { get; }
    internal string BindingSetSha256 { get; }
    internal string AuthorityRecordId { get; }
    internal string AuthorityIdentitySha256 { get; }
    [System.Text.Json.Serialization.JsonIgnore]
    internal IReadOnlyList<DesignConversionCandidatePayloadCustodyEntry> Entries => _entries;

    private string DebuggerDisplay =>
        $"PayloadCustodyDraft(tx={TransactionId}, entries={_entries.Count}, raw=[REDACTED])";

    public override string ToString() => DebuggerDisplay;
}

internal sealed class DesignConversionCandidatePayloadCustodyCommitment
{
    internal DesignConversionCandidatePayloadCustodyCommitment(
        string transactionId,
        string manifestSha256,
        string bindingSetSha256,
        string authorityRecordId,
        string authorityFingerprintSha256,
        IReadOnlyList<string> bindings)
    {
        TransactionId = transactionId;
        ManifestSha256 = manifestSha256;
        BindingSetSha256 = bindingSetSha256;
        AuthorityRecordId = authorityRecordId;
        AuthorityFingerprintSha256 = authorityFingerprintSha256;
        Bindings = new ImmutableStringList(bindings);
    }

    internal string TransactionId { get; }
    internal string ManifestSha256 { get; }
    internal string BindingSetSha256 { get; }
    internal string AuthorityRecordId { get; }
    internal string AuthorityFingerprintSha256 { get; }
    internal IReadOnlyList<string> Bindings { get; }
}

internal sealed record DesignConversionCandidatePayloadCustodyStageReceipt(
    DesignConversionCandidatePayloadCustodyCommitment Commitment,
    string StagedEnvelopeSha256);

internal sealed class DesignConversionCandidatePayloadCustodyCrashException : Exception
{
    internal DesignConversionCandidatePayloadCustodyCrashException(
        string point,
        Exception innerException)
        : base($"Injected candidate payload custody crash at {point}.", innerException)
    {
    }
}

internal sealed class ImmutableStringList : IReadOnlyList<string>
{
    private readonly string[] _items;

    internal ImmutableStringList(IEnumerable<string> items)
        => _items = (items ?? throw new ArgumentNullException(nameof(items))).ToArray();

    public int Count => _items.Length;
    public string this[int index] => _items[index];
    public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)_items).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal sealed class ImmutableEntryList
    : IReadOnlyList<DesignConversionCandidatePayloadCustodyEntry>
{
    private readonly DesignConversionCandidatePayloadCustodyEntry[] _items;

    internal ImmutableEntryList(
        IEnumerable<DesignConversionCandidatePayloadCustodyEntry> items)
        => _items = (items ?? throw new ArgumentNullException(nameof(items))).ToArray();

    public int Count => _items.Length;
    public DesignConversionCandidatePayloadCustodyEntry this[int index] => _items[index];
    public IEnumerator<DesignConversionCandidatePayloadCustodyEntry> GetEnumerator()
        => ((IEnumerable<DesignConversionCandidatePayloadCustodyEntry>)_items).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Dedicated recoverable custody transaction store.  One envelope owns every
/// teacher-authored payload belonging to one authority record.  Files are never
/// discovered by payload hashes: readers resolve the exact committed transaction
/// named by the redacted authority record.
/// </summary>
internal sealed class DesignConversionCandidatePayloadCustodyStore
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal sealed class CommittedEvidence
    {
        internal CommittedEvidence(
            DesignConversionCandidatePayloadCustodyCommitment commitment)
            => Commitment = commitment;

        internal DesignConversionCandidatePayloadCustodyCommitment Commitment { get; }

        private string DebuggerDisplay =>
            $"CommittedPayloadCustodyEvidence(tx={Commitment.TransactionId}, bindings={Commitment.Bindings.Count})";

        public override string ToString() => DebuggerDisplay;
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal sealed class ResolvedSecretEnvelope
    {
        private readonly IReadOnlyList<DesignConversionCandidatePayloadCustodyEntry>
            _entries;

        private ResolvedSecretEnvelope(
            string transactionId,
            IReadOnlyList<DesignConversionCandidatePayloadCustodyEntry> entries)
        {
            TransactionId = transactionId;
            _entries = new ImmutableEntryList(entries);
        }

        internal static ResolvedSecretEnvelope CreateForMintedEvidence(
            CommittedEvidence evidence,
            string transactionId,
            IReadOnlyList<DesignConversionCandidatePayloadCustodyEntry> entries)
        {
            if (!MintedEvidence.TryGetValue(evidence, out var mint) ||
                !string.Equals(
                    mint.Commitment.TransactionId,
                    transactionId,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Resolved custody envelopes require store-minted evidence.");
            }
            return new ResolvedSecretEnvelope(transactionId, entries);
        }

        internal string TransactionId { get; }
        [System.Text.Json.Serialization.JsonIgnore]
        internal IReadOnlyList<DesignConversionCandidatePayloadCustodyEntry> Entries =>
            _entries;

        private string DebuggerDisplay =>
            $"ResolvedPayloadCustodyEnvelope(tx={TransactionId}, entries={_entries.Count}, raw=[REDACTED])";

        public override string ToString() => DebuggerDisplay;
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal sealed class TrainingCapability
    {
        internal TrainingCapability(int bindingCount)
            => BindingCount = bindingCount;

        private int BindingCount { get; }

        private string DebuggerDisplay =>
            $"PayloadCustodyTrainingCapability(bindings={BindingCount})";

        public override string ToString() => DebuggerDisplay;
    }

    internal const string DirectoryName = "candidate-payload-custody";
    internal const string TransactionMetadataKey = "payload_custody_transaction_id";
    internal const string ManifestMetadataKey = "payload_custody_manifest_sha256";
    internal const string BindingSetMetadataKey = "payload_custody_binding_set_sha256";
    internal const string EntryCountMetadataKey = "payload_custody_entry_count";
    internal const string SchemaVersion = "design-conversion-candidate-payload-custody-v1";
    internal const int MaximumEntries = 64;
    internal const int MaximumStagedTransactions = 256;
    private const int MaximumFieldBytes = 4 * 1024 * 1024;
    private const int MaximumEnvelopeBytes = 16 * 1024 * 1024;
    private const string StageSuffix = ".custody-stage.json";
    private const string CommittedSuffix = ".custody-committed.json";
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly ConditionalWeakTable<CommittedEvidence, EvidenceMint>
        MintedEvidence = new();
    private static readonly ConditionalWeakTable<ResolvedSecretEnvelope, EnvelopeMint>
        MintedEnvelopes = new();
    private static readonly ConditionalWeakTable<
        DesignConversionDpoPayloadCustodySecret,
        SecretMint> MintedSecrets = new();
    private static readonly ConditionalWeakTable<TrainingCapability, CapabilityMint>
        MintedTrainingCapabilities = new();
    private readonly object _storeIdentity = new();

    private sealed record EntryMint(
        string TrainingRecordId,
        string BindingSha256,
        string GovernanceSha256,
        string PromptSha256,
        string ChosenResponseSha256,
        string RejectedResponseSha256);

    private sealed class EvidenceMint
    {
        private readonly Dictionary<string, EntryMint> _entries;

        internal EvidenceMint(
            object storeIdentity,
            DesignConversionCandidatePayloadCustodyCommitment commitment,
            string envelopeSha256,
            IEnumerable<EntryMint> entries)
        {
            StoreIdentity = storeIdentity;
            Commitment = SnapshotCommitment(commitment);
            EnvelopeSha256 = envelopeSha256;
            _entries = entries.ToDictionary(
                static entry => entry.BindingSha256,
                StringComparer.Ordinal);
        }

        internal object StoreIdentity { get; }
        internal DesignConversionCandidatePayloadCustodyCommitment Commitment { get; }
        internal string EnvelopeSha256 { get; }
        internal IEnumerable<EntryMint> Entries => _entries.Values;
        internal bool TryGetEntry(string binding, out EntryMint entry)
            => _entries.TryGetValue(binding, out entry!);
    }

    private sealed record EnvelopeMint(
        object StoreIdentity,
        string TransactionId,
        string EnvelopeSha256,
        CommittedEvidence Evidence);

    private sealed record SecretMint(
        object StoreIdentity,
        string TransactionId,
        string EnvelopeSha256,
        string BindingSha256);

    private sealed record CapabilityEntryMint(
        object StoreIdentity,
        string TransactionId,
        string EnvelopeSha256,
        EntryMint Entry);

    private sealed class CapabilityMint
    {
        private readonly Dictionary<string, CapabilityEntryMint> _entries;

        internal CapabilityMint(
            object storeIdentity,
            IEnumerable<CapabilityEntryMint> entries)
        {
            var snapshot = entries.ToArray();
            if (snapshot.Any(entry =>
                    !ReferenceEquals(entry.StoreIdentity, storeIdentity)))
            {
                throw new InvalidOperationException(
                    "Capability entries must originate from one custody store.");
            }
            StoreIdentity = storeIdentity;
            _entries = snapshot.ToDictionary(
                static entry => entry.Entry.BindingSha256,
                StringComparer.Ordinal);
        }

        internal object StoreIdentity { get; }
        internal bool TryGetEntry(string binding, out CapabilityEntryMint entry)
            => _entries.TryGetValue(binding, out entry!);
        internal int Count => _entries.Count;
    }
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling =
            System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
        WriteIndented = false
    };
    private readonly string _root;
    private readonly string _stageRoot;
    private readonly string _committedRoot;
    private readonly Action<string>? _faultInjectionForTesting;

    private sealed class LinuxCustodyDirectories : IDisposable
    {
        internal LinuxCustodyDirectories(
            DesignConversionHandleBoundDirectory root,
            DesignConversionHandleBoundDirectory stage,
            DesignConversionHandleBoundDirectory committed)
        {
            Root = root;
            Stage = stage;
            Committed = committed;
        }

        internal DesignConversionHandleBoundDirectory Root { get; }
        internal DesignConversionHandleBoundDirectory Stage { get; }
        internal DesignConversionHandleBoundDirectory Committed { get; }

        public void Dispose()
        {
            Committed.Dispose();
            Stage.Dispose();
            Root.Dispose();
        }
    }

    internal DesignConversionCandidatePayloadCustodyStore(
        string artifactRoot,
        Action<string>? faultInjectionForTesting = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artifactRoot);
        var canonicalArtifactRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(artifactRoot));
        _root = Path.Combine(canonicalArtifactRoot, DirectoryName);
        _stageRoot = Path.Combine(_root, "staged");
        _committedRoot = Path.Combine(_root, "committed");
        _faultInjectionForTesting = faultInjectionForTesting;
    }

    internal bool HasExistingState => OperatingSystem.IsLinux()
        ? HasExistingLinuxState()
        : Directory.Exists(_root) || File.Exists(_root);

    internal static OperationResult<DesignConversionCandidatePayloadCustodyDraft> CreateDraft(
        string authorityRecordId,
        string authorityIdentitySha256,
        IReadOnlyList<DesignConversionCandidatePayloadCustodyEntry> entries)
    {
        if (!IsSafeIdentity(authorityRecordId, 512) ||
            !IsSha256Ref(authorityIdentitySha256) ||
            entries is null || entries.Count is < 1 or > MaximumEntries)
        {
            return Fail<DesignConversionCandidatePayloadCustodyDraft>();
        }

        var ordered = entries
            .OrderBy(static entry => entry.BindingSha256, StringComparer.Ordinal)
            .ThenBy(static entry => entry.TrainingRecordId, StringComparer.Ordinal)
            .ToArray();
        if (ordered.Select(static entry => entry.BindingSha256)
                .Distinct(StringComparer.Ordinal).Count() != ordered.Length ||
            ordered.Select(static entry => entry.TrainingRecordId)
                .Distinct(StringComparer.Ordinal).Count() != ordered.Length ||
            ordered.Any(static entry => !ValidateEntry(entry)))
        {
            return Fail<DesignConversionCandidatePayloadCustodyDraft>();
        }

        var bindingSetSha256 = Sha256Ref(string.Join("\n",
            ordered.Select(static entry => entry.BindingSha256)));
        var transactionId = HashHex(string.Join("|",
        [
            SchemaVersion,
            authorityRecordId,
            authorityIdentitySha256,
            bindingSetSha256
        ]));
        var manifestSha256 = BuildManifestSha256(
            transactionId,
            authorityRecordId,
            authorityIdentitySha256,
            bindingSetSha256,
            ordered);

        return OperationResult<DesignConversionCandidatePayloadCustodyDraft>.Ok(
            new DesignConversionCandidatePayloadCustodyDraft(
                transactionId,
                manifestSha256,
                bindingSetSha256,
                authorityRecordId,
                authorityIdentitySha256,
                ordered));
    }

    internal static OperationResult<DesignConversionCandidatePayloadCustodyCommitment> BindAuthority(
        DesignConversionCandidatePayloadCustodyDraft draft,
        string authorityFingerprintSha256)
    {
        if (draft is null || !IsSha256Ref(authorityFingerprintSha256))
            return Fail<DesignConversionCandidatePayloadCustodyCommitment>();

        return OperationResult<DesignConversionCandidatePayloadCustodyCommitment>.Ok(
            new DesignConversionCandidatePayloadCustodyCommitment(
                draft.TransactionId,
                draft.ManifestSha256,
                draft.BindingSetSha256,
                draft.AuthorityRecordId,
                authorityFingerprintSha256,
                draft.Entries.Select(static entry => entry.BindingSha256).ToArray()));
    }

    internal OperationResult<DesignConversionCandidatePayloadCustodyStageReceipt> Prepare(
        DesignConversionCandidatePayloadCustodyDraft draft,
        DesignConversionCandidatePayloadCustodyCommitment commitment)
    {
        if (OperatingSystem.IsLinux())
        {
            // Linux-путь хранит служебный конверт не по строковым путям, а через
            // удержанные каталоги `stage` и `committed`.
            return PrepareLinux(draft, commitment);
        }

        if (!CommitmentMatchesDraft(commitment, draft))
            return Fail<DesignConversionCandidatePayloadCustodyStageReceipt>();

        string? newlyCreatedStagePath = null;
        try
        {
            if (!EnsurePrivateRoots())
                return Fail<DesignConversionCandidatePayloadCustodyStageReceipt>();

            var envelope = ToPersistedEnvelope(draft, commitment);
            var bytes = SerializeBounded(envelope);
            if (bytes is null)
                return Fail<DesignConversionCandidatePayloadCustodyStageReceipt>();
            var envelopeSha256 = Sha256Ref(bytes);
            var stagePath = StagePath(commitment.TransactionId);
            var committedPath = CommittedPath(commitment.TransactionId);

            if (File.Exists(committedPath))
            {
                var committed = ReadEnvelope(committedPath);
                if (!committed.IsSuccess || !SameEnvelope(committed.Value!, envelope))
                    return Fail<DesignConversionCandidatePayloadCustodyStageReceipt>();
                return OperationResult<DesignConversionCandidatePayloadCustodyStageReceipt>.Ok(
                    new DesignConversionCandidatePayloadCustodyStageReceipt(
                        commitment,
                        envelopeSha256));
            }

            if (File.Exists(stagePath))
            {
                var staged = ReadEnvelope(stagePath);
                if (!staged.IsSuccess || !SameEnvelope(staged.Value!, envelope))
                    return Fail<DesignConversionCandidatePayloadCustodyStageReceipt>();
            }
            else
            {
                newlyCreatedStagePath = stagePath;
                WriteNewPrivateFile(stagePath, bytes);
                var readback = ReadEnvelope(stagePath);
                if (!readback.IsSuccess || !SameEnvelope(readback.Value!, envelope))
                {
                    TryDeleteNewStage(newlyCreatedStagePath);
                    return Fail<DesignConversionCandidatePayloadCustodyStageReceipt>();
                }
                InjectCrashForTesting("after_stage_publish");
            }

            return OperationResult<DesignConversionCandidatePayloadCustodyStageReceipt>.Ok(
                new DesignConversionCandidatePayloadCustodyStageReceipt(
                    commitment,
                    envelopeSha256));
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            TryDeleteNewStage(newlyCreatedStagePath);
            return Fail<DesignConversionCandidatePayloadCustodyStageReceipt>();
        }
    }

    internal OperationResult<CommittedEvidence> Commit(
        DesignConversionCandidatePayloadCustodyStageReceipt receipt)
    {
        if (OperatingSystem.IsLinux())
        {
            // Коммит на Linux публикует подготовленный конверт только через
            // закреплённые файловые дескрипторы каталогов.
            return CommitLinux(receipt);
        }

        try
        {
            if (receipt is null || !EnsurePrivateRoots())
                return Fail<CommittedEvidence>();
            var stagePath = StagePath(receipt.Commitment.TransactionId);
            var committedPath = CommittedPath(receipt.Commitment.TransactionId);

            if (File.Exists(committedPath))
            {
                var replay = ResolveCommitted(receipt.Commitment);
                if (!replay.IsSuccess)
                    return replay;
                if (File.Exists(stagePath))
                {
                    var stage = ReadEnvelope(stagePath);
                    var committed = ReadEnvelope(committedPath);
                    if (!stage.IsSuccess || !committed.IsSuccess ||
                        !SameEnvelope(stage.Value!, committed.Value!))
                    {
                        return Fail<CommittedEvidence>();
                    }
                    File.Delete(stagePath);
                    if (OperatingSystem.IsLinux() &&
                        !SecureLocalFileBoundary.FlushParentDirectory(stagePath))
                        return Fail<CommittedEvidence>();
                }
                return replay;
            }

            var staged = ReadEnvelope(stagePath);
            if (!staged.IsSuccess || !EnvelopeMatchesCommitment(staged.Value!, receipt.Commitment) ||
                !string.Equals(Sha256Ref(SerializeBounded(staged.Value!)!),
                    receipt.StagedEnvelopeSha256, StringComparison.Ordinal))
            {
                return Fail<CommittedEvidence>();
            }

            InjectCrashForTesting("before_commit_rename");
            if (!SecureLocalFileBoundary.PublishAtomicDurable(
                    stagePath,
                    committedPath))
            {
                return Fail<CommittedEvidence>();
            }
            var published = ReadEnvelope(committedPath);
            if (!published.IsSuccess || File.Exists(stagePath) ||
                !EnvelopeMatchesCommitment(published.Value!, receipt.Commitment))
                return Fail<CommittedEvidence>();
            InjectCrashForTesting("after_commit_rename");
            return ResolveCommitted(receipt.Commitment);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return Fail<CommittedEvidence>();
        }
    }

    internal OperationResult<bool> Abort(DesignConversionCandidatePayloadCustodyStageReceipt receipt)
    {
        if (OperatingSystem.IsLinux())
        {
            // Отмена на Linux удаляет только тот подготовленный файл, чья
            // файловая личность совпала с удержанным дескриптором.
            return AbortLinux(receipt);
        }

        try
        {
            if (receipt is null)
                return Fail<bool>();
            var stagePath = StagePath(receipt.Commitment.TransactionId);
            if (!File.Exists(stagePath))
                return OperationResult<bool>.Ok(true);
            var staged = ReadEnvelope(stagePath);
            if (!staged.IsSuccess || !EnvelopeMatchesCommitment(staged.Value!, receipt.Commitment))
                return Fail<bool>();
            InjectCrashForTesting("before_abort_delete");
            File.Delete(stagePath);
            if (OperatingSystem.IsLinux() &&
                !SecureLocalFileBoundary.FlushParentDirectory(stagePath))
                return Fail<bool>();
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return Fail<bool>();
        }
    }

    internal OperationResult<bool> VerifyAborted(
        DesignConversionCandidatePayloadCustodyStageReceipt receipt)
    {
        if (OperatingSystem.IsLinux())
        {
            // Проверка отмены на Linux не доверяет отсутствию пути без
            // открытия обоих служебных каталогов с запретом следовать ссылкам.
            return VerifyAbortedLinux(receipt);
        }

        try
        {
            if (receipt is null || !ValidateCommitment(receipt.Commitment) ||
                !ValidateExistingPrivateRoots())
            {
                return Fail<bool>();
            }
            var stagePath = StagePath(receipt.Commitment.TransactionId);
            var committedPath = CommittedPath(receipt.Commitment.TransactionId);
            if (HasExistingReparsePoint(stagePath) ||
                HasExistingReparsePoint(committedPath) ||
                File.Exists(stagePath) ||
                File.Exists(committedPath))
            {
                return Fail<bool>();
            }
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return Fail<bool>();
        }
    }

    internal OperationResult<CommittedEvidence> ResolveCommitted(
        DesignConversionCandidatePayloadCustodyCommitment commitment)
    {
        if (OperatingSystem.IsLinux())
        {
            // `Resolve` на Linux сначала заново доказывает конверт в
            // `committed`, затем создаёт внутреннее доказательство хранилища.
            return ResolveCommittedLinux(commitment);
        }

        try
        {
            if (commitment is null || !ValidateCommitment(commitment) ||
                !ValidateExistingPrivateRoots())
            {
                return Fail<CommittedEvidence>();
            }
            var envelope = ReadEnvelope(CommittedPath(commitment.TransactionId));
            if (!envelope.IsSuccess || !EnvelopeMatchesCommitment(envelope.Value!, commitment))
                return Fail<CommittedEvidence>();

            var serialized = SerializeBounded(envelope.Value!);
            if (serialized is null)
                return Fail<CommittedEvidence>();
            var canonicalCommitment = SnapshotCommitment(commitment);
            var evidence = new CommittedEvidence(canonicalCommitment);
            MintedEvidence.Add(evidence, new EvidenceMint(
                _storeIdentity,
                canonicalCommitment,
                Sha256Ref(serialized),
                envelope.Value!.Entries.Select(ToEntryMint)));
            return OperationResult<CommittedEvidence>.Ok(evidence);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return Fail<CommittedEvidence>();
        }
    }

    internal OperationResult<ResolvedSecretEnvelope> ResolveSecretEnvelope(
        CommittedEvidence evidence)
    {
        if (!TryGetEvidenceMint(evidence, out var evidenceMint) ||
            !ReferenceEquals(evidenceMint.StoreIdentity, _storeIdentity))
            return Fail<ResolvedSecretEnvelope>();
        var envelope = OperatingSystem.IsLinux()
            ? ReadCommittedEnvelopeLinux(evidenceMint.Commitment.TransactionId)
            : ReadEnvelope(CommittedPath(evidenceMint.Commitment.TransactionId));
        if (!envelope.IsSuccess ||
            !EnvelopeMatchesCommitment(envelope.Value!, evidenceMint.Commitment))
        {
            return Fail<ResolvedSecretEnvelope>();
        }
        var serialized = SerializeBounded(envelope.Value!);
        if (serialized is null || !string.Equals(
                Sha256Ref(serialized),
                evidenceMint.EnvelopeSha256,
                StringComparison.Ordinal))
        {
            return Fail<ResolvedSecretEnvelope>();
        }
        var entries = envelope.Value!.Entries.Select(ToEntry).ToArray();
        if (entries.Any(static entry => entry is null))
            return Fail<ResolvedSecretEnvelope>();
        var exactEntries = entries.Select(static entry => entry!).ToArray();
        var resolved = ResolvedSecretEnvelope.CreateForMintedEvidence(
            evidence,
            evidenceMint.Commitment.TransactionId,
            exactEntries);
        MintedEnvelopes.Add(resolved, new EnvelopeMint(
            _storeIdentity,
            evidenceMint.Commitment.TransactionId,
            evidenceMint.EnvelopeSha256,
            evidence));
        foreach (var entry in exactEntries)
        {
            MintedSecrets.Add(entry.Secret, new SecretMint(
                _storeIdentity,
                evidenceMint.Commitment.TransactionId,
                evidenceMint.EnvelopeSha256,
                entry.BindingSha256));
        }
        return OperationResult<ResolvedSecretEnvelope>.Ok(resolved);
    }

    internal OperationResult<bool> Reconcile(
        IReadOnlyDictionary<string, DesignConversionCandidatePayloadCustodyCommitment> authorities)
    {
        if (OperatingSystem.IsLinux())
        {
            // `Reconcile` на Linux доводит `stage` и `committed` к журналу
            // полномочий через ту же закреплённую границу, без `File.Move`.
            return ReconcileLinux(authorities);
        }

        try
        {
            if (authorities is null || !EnsurePrivateRoots())
                return Fail<bool>();
            var staged = EnumerateStagedBounded(_stageRoot);
            if (staged is null)
                return Fail<bool>();

            var stagedIds = staged.ToDictionary(ParseTransactionId, StringComparer.Ordinal);
            var committedAuthorityIds = new HashSet<string>(StringComparer.Ordinal);
            var currentAuthorityEnvelopes = new List<PersistedEnvelope>();
            foreach (var authorityPair in authorities
                         .OrderBy(static pair => pair.Key, StringComparer.Ordinal))
            {
                var tx = authorityPair.Key;
                var authority = authorityPair.Value;
                if (!string.Equals(
                        tx,
                        authority.TransactionId,
                        StringComparison.Ordinal) ||
                    !ValidateCommitment(authority))
                {
                    return Fail<bool>();
                }

                var committedPath = CommittedPath(tx);
                if (File.Exists(committedPath))
                {
                    if (HasExistingReparsePoint(committedPath) ||
                        !IsExpectedTransactionFile(committedPath, CommittedSuffix))
                        return Fail<bool>();
                    var committedEnvelope = ReadEnvelope(committedPath);
                    if (!committedEnvelope.IsSuccess ||
                        !EnvelopeMatchesCommitment(committedEnvelope.Value!, authority))
                        return Fail<bool>();
                    if (stagedIds.Remove(tx, out var stagePath))
                    {
                        var stageEnvelope = ReadEnvelope(stagePath);
                        if (!stageEnvelope.IsSuccess ||
                            !SameEnvelope(stageEnvelope.Value!, committedEnvelope.Value!))
                            return Fail<bool>();
                        File.Delete(stagePath);
                        if (OperatingSystem.IsLinux() &&
                            !SecureLocalFileBoundary.FlushParentDirectory(stagePath))
                            return Fail<bool>();
                    }

                    if (!committedAuthorityIds.Add(tx))
                        return Fail<bool>();
                    currentAuthorityEnvelopes.Add(committedEnvelope.Value!);
                    continue;
                }

                if (!stagedIds.Remove(tx, out var authorityStagePath))
                    return Fail<bool>();

                var authorityStageEnvelope = ReadEnvelope(authorityStagePath);
                if (!authorityStageEnvelope.IsSuccess ||
                    !EnvelopeMatchesCommitment(authorityStageEnvelope.Value!, authority))
                    return Fail<bool>();
                if (!SecureLocalFileBoundary.PublishAtomicDurable(
                        authorityStagePath,
                        committedPath))
                {
                    return Fail<bool>();
                }
                var recoveredEnvelope = ReadEnvelope(committedPath);
                if (!recoveredEnvelope.IsSuccess ||
                    File.Exists(authorityStagePath) ||
                    !EnvelopeMatchesCommitment(recoveredEnvelope.Value!, authority) ||
                    !committedAuthorityIds.Add(tx))
                    return Fail<bool>();
                currentAuthorityEnvelopes.Add(recoveredEnvelope.Value!);
            }

            foreach (var (tx, stagePath) in stagedIds)
            {
                if (authorities.ContainsKey(tx))
                    return Fail<bool>();

                File.Delete(stagePath);
                if (OperatingSystem.IsLinux() &&
                    !SecureLocalFileBoundary.FlushParentDirectory(stagePath))
                    return Fail<bool>();
            }

            foreach (var committedPath in Directory.EnumerateFiles(
                             _committedRoot,
                             "*",
                             SearchOption.TopDirectoryOnly)
                         .Order(StringComparer.Ordinal))
            {
                if (HasExistingReparsePoint(committedPath) ||
                    !IsExpectedTransactionFile(committedPath, CommittedSuffix))
                    return Fail<bool>();

                var tx = ParseTransactionId(committedPath);
                if (committedAuthorityIds.Contains(tx))
                    continue;

                var committedEnvelope = ReadEnvelope(committedPath);
                if (!committedEnvelope.IsSuccess ||
                    !IsImmediatePredecessorOfCurrentAuthority(
                        committedEnvelope.Value!,
                        currentAuthorityEnvelopes))
                {
                    return Fail<bool>();
                }

                File.Delete(committedPath);
                if (OperatingSystem.IsLinux() &&
                    !SecureLocalFileBoundary.FlushParentDirectory(committedPath))
                    return Fail<bool>();
            }

            // An authority commitment without either durable state is never a
            // successful recovery.
            if (authorities.Keys.Any(tx => !committedAuthorityIds.Contains(tx)))
                return Fail<bool>();

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return Fail<bool>();
        }
    }

    private static bool IsImmediatePredecessorOfCurrentAuthority(
        PersistedEnvelope predecessor,
        IReadOnlyList<PersistedEnvelope> currentAuthorityEnvelopes)
        => ValidateEnvelope(predecessor) &&
           currentAuthorityEnvelopes.Any(current =>
               ValidateEnvelope(current) &&
               string.Equals(
                   current.AuthorityRecordId,
                   predecessor.AuthorityRecordId,
                   StringComparison.Ordinal) &&
               string.Equals(
                   current.AuthorityIdentitySha256,
                   predecessor.AuthorityFingerprintSha256,
                   StringComparison.Ordinal));

    internal static DesignConversionCandidatePayloadCustodyEntry? FindExactEntry(
        ResolvedSecretEnvelope envelope,
        CommittedEvidence evidence,
        string bindingSha256)
    {
        if (!TryGetEvidenceMint(evidence, out var evidenceMint) ||
            envelope is null ||
            !MintedEnvelopes.TryGetValue(envelope, out var envelopeMint) ||
            !ReferenceEquals(evidenceMint.StoreIdentity, envelopeMint.StoreIdentity) ||
            !ReferenceEquals(envelopeMint.Evidence, evidence) ||
            !string.Equals(
                envelope.TransactionId,
                evidenceMint.Commitment.TransactionId,
                StringComparison.Ordinal) ||
            !string.Equals(
                envelopeMint.EnvelopeSha256,
                evidenceMint.EnvelopeSha256,
                StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(bindingSha256))
            return null;
        var matches = envelope.Entries
            .Where(entry => string.Equals(entry.BindingSha256, bindingSha256, StringComparison.Ordinal))
            .Take(2)
            .ToArray();
        return matches.Length == 1 ? matches[0] : null;
    }

    internal static bool IsStoreMinted(
        CommittedEvidence? evidence)
        => TryGetEvidenceMint(evidence, out _);

    internal static OperationResult<TrainingCapability> MintTrainingCapability(
        IReadOnlyList<CommittedEvidence>? evidence)
    {
        if (evidence is null || evidence.Count == 0)
        {
            return Fail<TrainingCapability>();
        }
        var mints = new List<EvidenceMint>(evidence.Count);
        foreach (var item in evidence)
        {
            if (!TryGetEvidenceMint(item, out var mint))
                return Fail<TrainingCapability>();
            mints.Add(mint);
        }
        var storeIdentity = mints[0].StoreIdentity;
        if (mints.Any(mint => !ReferenceEquals(mint.StoreIdentity, storeIdentity)))
            return Fail<TrainingCapability>();
        var entries = mints.SelectMany(mint => mint.Entries.Select(entry =>
            new CapabilityEntryMint(
                mint.StoreIdentity,
                mint.Commitment.TransactionId,
                mint.EnvelopeSha256,
                entry))).ToArray();
        if (entries.Length == 0 ||
            entries.Select(static entry => entry.Entry.BindingSha256)
                .Distinct(StringComparer.Ordinal).Count() != entries.Length)
        {
            return Fail<TrainingCapability>();
        }
        var capability = new TrainingCapability(entries.Length);
        MintedTrainingCapabilities.Add(
            capability,
            new CapabilityMint(storeIdentity, entries));
        return OperationResult<TrainingCapability>.Ok(capability);
    }

    internal static bool Authorizes(
        TrainingCapability? capability,
        DesignConversionDpoTrainingRecord record)
        => capability is not null &&
           MintedTrainingCapabilities.TryGetValue(capability, out var capabilityMint) &&
           ExactRecordMatchesMint(record, capabilityMint);

    internal static bool HasExactCapabilityAuthorityBinding(
        TrainingCapability? capability,
        DesignConversionDpoPayloadCustodySecret? secret)
        => TryGetCapabilityEntry(capability, secret, out _);

    internal static bool IsExactStoreBackedRecord(
        DesignConversionDpoTrainingRecord record,
        IReadOnlyList<CommittedEvidence>? evidence)
    {
        if (record?.PayloadCustodySecret is null || evidence is null)
            return false;
        var matches = evidence.Where(item =>
        {
            if (!TryGetEvidenceMint(item, out var mint))
                return false;
            return ExactRecordMatchesMint(record, mint);
        }).Take(2).ToArray();
        return matches.Length == 1;
    }

    private static bool TryGetEvidenceMint(
        CommittedEvidence? evidence,
        out EvidenceMint mint)
    {
        mint = null!;
        if (evidence is null || !MintedEvidence.TryGetValue(evidence, out var found))
            return false;
        mint = found;
        return true;
    }

    private static bool ExactRecordMatchesMint(
        DesignConversionDpoTrainingRecord record,
        EvidenceMint mint)
    {
        if (record is null)
            return false;
        var secret = record.PayloadCustodySecret;
        if (secret is null ||
            !MintedSecrets.TryGetValue(secret, out var secretMint) ||
            !ReferenceEquals(secretMint.StoreIdentity, mint.StoreIdentity) ||
            !string.Equals(secretMint.TransactionId,
                mint.Commitment.TransactionId, StringComparison.Ordinal) ||
            !string.Equals(secretMint.EnvelopeSha256,
                mint.EnvelopeSha256, StringComparison.Ordinal) ||
            !mint.TryGetEntry(secretMint.BindingSha256, out var entry))
        {
            return false;
        }
        return ExactRecordMatchesEntry(record, secret, entry);
    }

    private static bool ExactRecordMatchesMint(
        DesignConversionDpoTrainingRecord record,
        CapabilityMint mint)
    {
        if (record is null)
            return false;
        var secret = record.PayloadCustodySecret;
        if (!TryGetCapabilityEntry(mint, secret, out var entry))
        {
            return false;
        }
        return ExactRecordMatchesEntry(record, secret!, entry.Entry);
    }

    private static bool TryGetCapabilityEntry(
        TrainingCapability? capability,
        DesignConversionDpoPayloadCustodySecret? secret,
        out CapabilityEntryMint entry)
    {
        entry = null!;
        return capability is not null &&
               MintedTrainingCapabilities.TryGetValue(capability, out var mint) &&
               TryGetCapabilityEntry(mint, secret, out entry);
    }

    private static bool TryGetCapabilityEntry(
        CapabilityMint mint,
        DesignConversionDpoPayloadCustodySecret? secret,
        out CapabilityEntryMint entry)
    {
        entry = null!;
        if (secret is null ||
            !MintedSecrets.TryGetValue(secret, out var secretMint) ||
            !ReferenceEquals(secretMint.StoreIdentity, mint.StoreIdentity) ||
            !mint.TryGetEntry(secretMint.BindingSha256, out var found) ||
            !ReferenceEquals(found.StoreIdentity, mint.StoreIdentity) ||
            !ReferenceEquals(found.StoreIdentity, secretMint.StoreIdentity) ||
            !string.Equals(found.TransactionId,
                secretMint.TransactionId, StringComparison.Ordinal) ||
            !string.Equals(found.EnvelopeSha256,
                secretMint.EnvelopeSha256, StringComparison.Ordinal))
        {
            return false;
        }
        entry = found;
        return true;
    }

    private static bool ExactRecordMatchesEntry(
        DesignConversionDpoTrainingRecord record,
        DesignConversionDpoPayloadCustodySecret secret,
        EntryMint entry)
        => string.Equals(record.RecordId, entry.TrainingRecordId, StringComparison.Ordinal) &&
           string.Equals(secret.TrainingRecordBindingSha256,
               entry.BindingSha256, StringComparison.Ordinal) &&
           string.Equals(secret.GovernanceSha256,
               entry.GovernanceSha256, StringComparison.Ordinal) &&
           string.Equals(secret.PromptSha256,
               entry.PromptSha256, StringComparison.Ordinal) &&
           string.Equals(secret.ChosenResponseSha256,
               entry.ChosenResponseSha256, StringComparison.Ordinal) &&
           string.Equals(secret.RejectedResponseSha256,
               entry.RejectedResponseSha256, StringComparison.Ordinal) &&
           JsonlDesignConversionLocalArtifactDb
               .ValidateAndMaterializePayloadCustody(record).IsSuccess;

    private OperationResult<DesignConversionCandidatePayloadCustodyStageReceipt> PrepareLinux(
        DesignConversionCandidatePayloadCustodyDraft draft,
        DesignConversionCandidatePayloadCustodyCommitment commitment)
    {
        if (!CommitmentMatchesDraft(commitment, draft))
            return Fail<DesignConversionCandidatePayloadCustodyStageReceipt>();

        string? newlyCreatedStageLeaf = null;
        try
        {
            using var directories = OpenLinuxPrivateRoots(createMissing: true);
            if (directories is null)
                return Fail<DesignConversionCandidatePayloadCustodyStageReceipt>();

            var envelope = ToPersistedEnvelope(draft, commitment);
            var bytes = SerializeBounded(envelope);
            if (bytes is null)
                return Fail<DesignConversionCandidatePayloadCustodyStageReceipt>();
            var envelopeSha256 = Sha256Ref(bytes);
            var stageLeaf = StageLeafName(commitment.TransactionId);
            var committedLeaf = CommittedLeafName(commitment.TransactionId);

            var committed = ReadEnvelope(directories.Committed, committedLeaf);
            if (committed.IsSuccess)
            {
                if (!SameEnvelope(committed.Value!, envelope))
                    return Fail<DesignConversionCandidatePayloadCustodyStageReceipt>();
                return OperationResult<DesignConversionCandidatePayloadCustodyStageReceipt>.Ok(
                    new DesignConversionCandidatePayloadCustodyStageReceipt(
                        commitment,
                        envelopeSha256));
            }
            if (directories.Committed.ContainsNameNoFollow(
                    MaximumStagedTransactions + MaximumEntries + 1,
                    committedLeaf))
            {
                return Fail<DesignConversionCandidatePayloadCustodyStageReceipt>();
            }

            var staged = ReadEnvelope(directories.Stage, stageLeaf);
            if (staged.IsSuccess)
            {
                if (!SameEnvelope(staged.Value!, envelope))
                    return Fail<DesignConversionCandidatePayloadCustodyStageReceipt>();
            }
            else
            {
                if (directories.Stage.ContainsNameNoFollow(
                        MaximumStagedTransactions + 1,
                        stageLeaf))
                {
                    return Fail<DesignConversionCandidatePayloadCustodyStageReceipt>();
                }

                newlyCreatedStageLeaf = stageLeaf;
                WriteNewPrivateFile(directories.Stage, stageLeaf, bytes);
                var readback = ReadEnvelope(directories.Stage, stageLeaf);
                if (!readback.IsSuccess || !SameEnvelope(readback.Value!, envelope))
                {
                    TryDeleteNewStageLinux(newlyCreatedStageLeaf);
                    return Fail<DesignConversionCandidatePayloadCustodyStageReceipt>();
                }
                InjectCrashForTesting("after_stage_publish");
            }

            return OperationResult<DesignConversionCandidatePayloadCustodyStageReceipt>.Ok(
                new DesignConversionCandidatePayloadCustodyStageReceipt(
                    commitment,
                    envelopeSha256));
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            TryDeleteNewStageLinux(newlyCreatedStageLeaf);
            return Fail<DesignConversionCandidatePayloadCustodyStageReceipt>();
        }
    }

    private OperationResult<CommittedEvidence> CommitLinux(
        DesignConversionCandidatePayloadCustodyStageReceipt receipt)
    {
        try
        {
            if (receipt is null)
                return Fail<CommittedEvidence>();
            using var directories = OpenLinuxPrivateRoots(createMissing: true);
            if (directories is null)
                return Fail<CommittedEvidence>();

            var stageLeaf = StageLeafName(receipt.Commitment.TransactionId);
            var committedLeaf = CommittedLeafName(receipt.Commitment.TransactionId);
            var committedEnvelope = ReadEnvelope(directories.Committed, committedLeaf);
            if (committedEnvelope.IsSuccess)
            {
                var replay = ResolveCommittedLinux(receipt.Commitment);
                if (!replay.IsSuccess)
                    return replay;
                var stageEnvelope = ReadEnvelope(directories.Stage, stageLeaf);
                if (stageEnvelope.IsSuccess)
                {
                    if (!SameEnvelope(stageEnvelope.Value!, committedEnvelope.Value!))
                        return Fail<CommittedEvidence>();
                    DeleteExistingLinuxLeaf(directories.Stage, stageLeaf);
                }
                else if (directories.Stage.ContainsNameNoFollow(
                             MaximumStagedTransactions + 1,
                             stageLeaf))
                {
                    return Fail<CommittedEvidence>();
                }
                return replay;
            }
            if (directories.Committed.ContainsNameNoFollow(
                    MaximumStagedTransactions + MaximumEntries + 1,
                    committedLeaf))
            {
                return Fail<CommittedEvidence>();
            }

            using var stagedStream = directories.Stage.TryOpenExisting(
                stageLeaf,
                FileShare.None,
                writeThrough: true);
            if (stagedStream is null)
                return Fail<CommittedEvidence>();
            var staged = ReadEnvelope(directories.Stage, stageLeaf, stagedStream);
            if (!staged.IsSuccess ||
                !EnvelopeMatchesCommitment(staged.Value!, receipt.Commitment) ||
                !string.Equals(
                    Sha256Ref(SerializeBounded(staged.Value!)!),
                    receipt.StagedEnvelopeSha256,
                    StringComparison.Ordinal))
            {
                return Fail<CommittedEvidence>();
            }

            InjectCrashForTesting("before_commit_rename");
            directories.Stage.RenameToDirectoryNoReplaceAndFlush(
                stagedStream,
                stageLeaf,
                directories.Committed,
                committedLeaf);
            var published = ReadEnvelope(directories.Committed, committedLeaf);
            if (!published.IsSuccess ||
                directories.Stage.ContainsNameNoFollow(
                    MaximumStagedTransactions + 1,
                    stageLeaf) ||
                !EnvelopeMatchesCommitment(published.Value!, receipt.Commitment))
            {
                return Fail<CommittedEvidence>();
            }
            InjectCrashForTesting("after_commit_rename");
            return ResolveCommittedLinux(receipt.Commitment);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return Fail<CommittedEvidence>();
        }
    }

    private OperationResult<bool> AbortLinux(
        DesignConversionCandidatePayloadCustodyStageReceipt receipt)
    {
        try
        {
            if (receipt is null)
                return Fail<bool>();
            using var directories = OpenLinuxPrivateRoots(createMissing: false);
            if (directories is null)
                return OperationResult<bool>.Ok(true);

            var stageLeaf = StageLeafName(receipt.Commitment.TransactionId);
            using var stageStream = directories.Stage.TryOpenExisting(
                stageLeaf,
                FileShare.None,
                writeThrough: true);
            if (stageStream is null)
                return OperationResult<bool>.Ok(true);
            var staged = ReadEnvelope(directories.Stage, stageLeaf, stageStream);
            if (!staged.IsSuccess ||
                !EnvelopeMatchesCommitment(staged.Value!, receipt.Commitment))
            {
                return Fail<bool>();
            }

            InjectCrashForTesting("before_abort_delete");
            var identity = directories.Stage.GetIdentity(stageStream, stageLeaf);
            directories.Stage.DeleteHeldRegular(stageStream, stageLeaf, identity);
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return Fail<bool>();
        }
    }

    private OperationResult<bool> VerifyAbortedLinux(
        DesignConversionCandidatePayloadCustodyStageReceipt receipt)
    {
        try
        {
            if (receipt is null || !ValidateCommitment(receipt.Commitment))
                return Fail<bool>();
            using var directories = OpenLinuxPrivateRoots(createMissing: false);
            if (directories is null)
                return Fail<bool>();
            var stageLeaf = StageLeafName(receipt.Commitment.TransactionId);
            var committedLeaf = CommittedLeafName(receipt.Commitment.TransactionId);
            using var stage = directories.Stage.TryOpenExistingReadOnly(stageLeaf);
            using var committed = directories.Committed.TryOpenExistingReadOnly(committedLeaf);
            if (stage is not null || committed is not null)
                return Fail<bool>();
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return Fail<bool>();
        }
    }

    private OperationResult<CommittedEvidence> ResolveCommittedLinux(
        DesignConversionCandidatePayloadCustodyCommitment commitment)
    {
        try
        {
            if (commitment is null || !ValidateCommitment(commitment))
                return Fail<CommittedEvidence>();
            var envelope = ReadCommittedEnvelopeLinux(commitment.TransactionId);
            if (!envelope.IsSuccess || !EnvelopeMatchesCommitment(envelope.Value!, commitment))
                return Fail<CommittedEvidence>();

            var serialized = SerializeBounded(envelope.Value!);
            if (serialized is null)
                return Fail<CommittedEvidence>();
            var canonicalCommitment = SnapshotCommitment(commitment);
            var evidence = new CommittedEvidence(canonicalCommitment);
            MintedEvidence.Add(evidence, new EvidenceMint(
                _storeIdentity,
                canonicalCommitment,
                Sha256Ref(serialized),
                envelope.Value!.Entries.Select(ToEntryMint)));
            return OperationResult<CommittedEvidence>.Ok(evidence);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return Fail<CommittedEvidence>();
        }
    }

    private OperationResult<bool> ReconcileLinux(
        IReadOnlyDictionary<string, DesignConversionCandidatePayloadCustodyCommitment> authorities)
    {
        try
        {
            if (authorities is null)
                return Fail<bool>();
            using var directories = OpenLinuxPrivateRoots(createMissing: true);
            if (directories is null)
                return Fail<bool>();

            var staged = EnumerateStagedBounded(directories.Stage);
            if (staged is null)
                return Fail<bool>();

            var stagedIds = staged.ToDictionary(ParseTransactionId, StringComparer.Ordinal);
            var committedAuthorityIds = new HashSet<string>(StringComparer.Ordinal);
            var currentAuthorityEnvelopes = new List<PersistedEnvelope>();
            foreach (var authorityPair in authorities
                         .OrderBy(static pair => pair.Key, StringComparer.Ordinal))
            {
                var tx = authorityPair.Key;
                var authority = authorityPair.Value;
                if (!string.Equals(
                        tx,
                        authority.TransactionId,
                        StringComparison.Ordinal) ||
                    !ValidateCommitment(authority))
                {
                    return Fail<bool>();
                }

                var committedLeaf = CommittedLeafName(tx);
                var committedEnvelope = ReadEnvelope(directories.Committed, committedLeaf);
                if (!committedEnvelope.IsSuccess &&
                    stagedIds.TryGetValue(tx, out var linkedStageLeaf) &&
                    TryCompleteLinkedPublishRecovery(
                        directories.Stage,
                        linkedStageLeaf,
                        directories.Committed,
                        committedLeaf,
                        authority))
                {
                    stagedIds.Remove(tx);
                    committedEnvelope = ReadEnvelope(directories.Committed, committedLeaf);
                }
                if (committedEnvelope.IsSuccess)
                {
                    if (!EnvelopeMatchesCommitment(committedEnvelope.Value!, authority))
                        return Fail<bool>();
                    if (stagedIds.Remove(tx, out var stageLeaf))
                    {
                        var stageEnvelope = ReadEnvelope(directories.Stage, stageLeaf);
                        if (!stageEnvelope.IsSuccess ||
                            !SameEnvelope(stageEnvelope.Value!, committedEnvelope.Value!))
                        {
                            return Fail<bool>();
                        }
                        DeleteExistingLinuxLeaf(directories.Stage, stageLeaf);
                    }

                    if (!committedAuthorityIds.Add(tx))
                        return Fail<bool>();
                    currentAuthorityEnvelopes.Add(committedEnvelope.Value!);
                    continue;
                }
                if (directories.Committed.ContainsNameNoFollow(
                        Math.Max(MaximumStagedTransactions + authorities.Count + 1, 1024),
                        committedLeaf))
                {
                    return Fail<bool>();
                }

                if (!stagedIds.Remove(tx, out var authorityStageLeaf))
                    return Fail<bool>();

                using var authorityStageStream = directories.Stage.TryOpenExisting(
                    authorityStageLeaf,
                    FileShare.None,
                    writeThrough: true);
                if (authorityStageStream is null)
                    return Fail<bool>();
                var authorityStageEnvelope = ReadEnvelope(
                    directories.Stage,
                    authorityStageLeaf,
                    authorityStageStream);
                if (!authorityStageEnvelope.IsSuccess ||
                    !EnvelopeMatchesCommitment(authorityStageEnvelope.Value!, authority))
                {
                    return Fail<bool>();
                }
                directories.Stage.RenameToDirectoryNoReplaceAndFlush(
                    authorityStageStream,
                    authorityStageLeaf,
                    directories.Committed,
                    committedLeaf);
                var recoveredEnvelope = ReadEnvelope(directories.Committed, committedLeaf);
                if (!recoveredEnvelope.IsSuccess ||
                    directories.Stage.ContainsNameNoFollow(
                        MaximumStagedTransactions + 1,
                        authorityStageLeaf) ||
                    !EnvelopeMatchesCommitment(recoveredEnvelope.Value!, authority) ||
                    !committedAuthorityIds.Add(tx))
                {
                    return Fail<bool>();
                }
                currentAuthorityEnvelopes.Add(recoveredEnvelope.Value!);
            }

            foreach (var (tx, stageLeaf) in stagedIds)
            {
                if (authorities.ContainsKey(tx))
                    return Fail<bool>();
                DeleteExistingLinuxLeaf(directories.Stage, stageLeaf);
            }

            foreach (var committedLeaf in EnumerateCommittedLinux(directories.Committed))
            {
                var tx = ParseTransactionId(committedLeaf);
                if (committedAuthorityIds.Contains(tx))
                    continue;

                var committedEnvelope = ReadEnvelope(directories.Committed, committedLeaf);
                if (!committedEnvelope.IsSuccess ||
                    !IsImmediatePredecessorOfCurrentAuthority(
                        committedEnvelope.Value!,
                        currentAuthorityEnvelopes))
                {
                    return Fail<bool>();
                }

                DeleteExistingLinuxLeaf(directories.Committed, committedLeaf);
            }

            if (authorities.Keys.Any(tx => !committedAuthorityIds.Contains(tx)))
                return Fail<bool>();

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return Fail<bool>();
        }
    }

    private bool EnsurePrivateRoots()
    {
        if (OperatingSystem.IsLinux())
            return OpenLinuxPrivateRoots(createMissing: true) is { } directories &&
                   DisposeAndReturnTrue(directories);

        var artifactRoot = Path.GetDirectoryName(_root);
        if (!SecureLocalFileBoundary.IsSupportedPlatform() ||
            string.IsNullOrWhiteSpace(artifactRoot) ||
            !ValidatePathBoundary(_root) ||
            HasExistingReparsePoint(artifactRoot) ||
            HasExistingReparsePoint(_root))
            return false;
        // The artifact root is not itself a secret-bearing custody boundary,
        // but it must exist before the Windows CreateDirectoryW call can
        // atomically apply the protected ACL to the first custody directory.
        Directory.CreateDirectory(artifactRoot);
        if (HasExistingReparsePoint(artifactRoot))
            return false;
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(_root);
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(_stageRoot);
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(_committedRoot);
        return ValidateExistingPrivateRoots();
    }

    private bool ValidateExistingPrivateRoots()
        => OperatingSystem.IsLinux()
            ? OpenLinuxPrivateRoots(createMissing: false) is { } directories &&
              DisposeAndReturnTrue(directories)
            : Directory.Exists(_root) && Directory.Exists(_stageRoot) &&
           Directory.Exists(_committedRoot) &&
           !HasExistingReparsePoint(_root) &&
           !HasExistingReparsePoint(_stageRoot) &&
           !HasExistingReparsePoint(_committedRoot) &&
           SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(_root) &&
           SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(_stageRoot) &&
           SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(_committedRoot);

    private void InjectCrashForTesting(string point)
    {
        if (_faultInjectionForTesting is null)
            return;
        try
        {
            _faultInjectionForTesting(point);
        }
        catch (Exception exception)
        {
            throw new DesignConversionCandidatePayloadCustodyCrashException(
                point,
                exception);
        }
    }

    private bool ValidatePathBoundary(string path)
    {
        var full = Path.GetFullPath(path);
        var prefix = Path.TrimEndingDirectorySeparator(_root) + Path.DirectorySeparatorChar;
        return string.Equals(full, _root, PathComparison) ||
               full.StartsWith(prefix, PathComparison);
    }

    private string StagePath(string transactionId) => SafeChild(_stageRoot, transactionId + StageSuffix);
    private string CommittedPath(string transactionId) => SafeChild(_committedRoot, transactionId + CommittedSuffix);
    private static string StageLeafName(string transactionId) => transactionId + StageSuffix;
    private static string CommittedLeafName(string transactionId) => transactionId + CommittedSuffix;

    private static bool DisposeAndReturnTrue(IDisposable disposable)
    {
        disposable.Dispose();
        return true;
    }

    private bool HasExistingLinuxState()
    {
        try
        {
            using var root = DesignConversionHandleBoundDirectory.AcquireExisting(
                _root,
                beforeIo: null);
            root.ValidateLinuxCustodyRootTrustContract();
            return true;
        }
        catch (DesignConversionHandleBoundNotFoundException)
        {
            return false;
        }
        catch (IOException)
        {
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return true;
        }
    }

    private LinuxCustodyDirectories? OpenLinuxPrivateRoots(bool createMissing)
    {
        if (!OperatingSystem.IsLinux())
            return null;

        DesignConversionHandleBoundDirectory? root = null;
        DesignConversionHandleBoundDirectory? stage = null;
        DesignConversionHandleBoundDirectory? committed = null;
        try
        {
            root = createMissing
                ? DesignConversionHandleBoundDirectory.AcquireOrCreate(_root, beforeIo: null)
                : DesignConversionHandleBoundDirectory.AcquireExisting(_root, beforeIo: null);
            root.ValidateLinuxCustodyRootTrustContract();
            stage = createMissing
                ? DesignConversionHandleBoundDirectory.AcquireOrCreate(_stageRoot, beforeIo: null)
                : DesignConversionHandleBoundDirectory.AcquireExisting(_stageRoot, beforeIo: null);
            stage.ValidateLinuxCustodyRootTrustContract();
            committed = createMissing
                ? DesignConversionHandleBoundDirectory.AcquireOrCreate(_committedRoot, beforeIo: null)
                : DesignConversionHandleBoundDirectory.AcquireExisting(_committedRoot, beforeIo: null);
            committed.ValidateLinuxCustodyRootTrustContract();
            return new LinuxCustodyDirectories(root, stage, committed);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            committed?.Dispose();
            stage?.Dispose();
            root?.Dispose();
            return null;
        }
    }

    private static string SafeChild(string root, string fileName)
    {
        if (!IsLowerHex(fileName.AsSpan(0, 64)) || fileName[64] != '.')
            throw new InvalidDataException("Invalid custody transaction id.");
        var fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        var full = Path.GetFullPath(Path.Combine(fullRoot, fileName));
        if (!full.StartsWith(fullRoot + Path.DirectorySeparatorChar, PathComparison))
            throw new InvalidDataException("Invalid custody path.");
        return full;
    }

    private static void WriteNewPrivateFile(string path, byte[] bytes)
    {
        if (bytes.LongLength > MaximumEnvelopeBytes || HasExistingReparsePoint(path))
            throw new InvalidDataException("Invalid custody envelope path.");
        using (var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
                   path,
                   FileMode.CreateNew,
                   FileAccess.Write,
                   FileShare.None,
                   FileOptions.WriteThrough,
                   4096))
        {
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    path) ||
                !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path))
            {
                throw new InvalidDataException("Invalid custody envelope path.");
            }
            stream.Write(bytes);
            stream.Flush(flushToDisk: true);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    path) ||
                !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path))
            {
                throw new InvalidDataException("Invalid custody envelope path.");
            }
        }
        if (HasExistingReparsePoint(path) ||
            !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path) ||
            (OperatingSystem.IsLinux() &&
             !SecureLocalFileBoundary.FlushParentDirectory(path)))
            throw new InvalidDataException("Invalid custody envelope path.");
    }

    private static void WriteNewPrivateFile(
        DesignConversionHandleBoundDirectory directory,
        string leafName,
        byte[] bytes)
    {
        if (bytes.LongLength > MaximumEnvelopeBytes)
            throw new InvalidDataException("Invalid custody envelope bytes.");
        using var stream = directory.CreateNew(
            leafName,
            FileShare.None,
            writeThrough: true);
        directory.ValidateLinuxCustodyFileTrustContract(stream, leafName);
        stream.Write(bytes);
        stream.Flush(flushToDisk: true);
        directory.ValidateLinuxCustodyFileTrustContract(stream, leafName);
        directory.FlushDirectoryMetadata();
    }

    private static void TryDeleteNewStage(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;
        try
        {
            if (File.Exists(path) && !HasExistingReparsePoint(path))
            {
                File.Delete(path);
                if (OperatingSystem.IsLinux())
                    _ = SecureLocalFileBoundary.FlushParentDirectory(path);
            }
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            // A normal prepare failure still reports fail-closed. Recovery will
            // classify any stage that could not be removed under the authority lease.
        }
    }

    private void TryDeleteNewStageLinux(string? leafName)
    {
        if (string.IsNullOrWhiteSpace(leafName))
            return;
        try
        {
            using var directories = OpenLinuxPrivateRoots(createMissing: false);
            if (directories is null)
                return;
            DeleteExistingLinuxLeaf(directories.Stage, leafName);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            // Обычная ошибка подготовки уже даёт безопасный отказ; поздняя
            // сверка решит, можно ли удалить подготовленный файл под живым журналом.
        }
    }

    private static OperationResult<PersistedEnvelope> ReadEnvelope(string path)
    {
        try
        {
            if (!SecureLocalFileBoundary.IsSupportedPlatform() ||
                !File.Exists(path) || HasExistingReparsePoint(path) ||
                !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path))
                return Fail<PersistedEnvelope>();
            using var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                FileOptions.SequentialScan,
                4096);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    path))
                return Fail<PersistedEnvelope>();
            var first = SecureLocalFileBoundary.ReadBoundedStableBytes(
                stream,
                MaximumEnvelopeBytes);
            if (first is null ||
                !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    path) ||
                HasExistingReparsePoint(path) ||
                !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path))
                return Fail<PersistedEnvelope>();
            _ = StrictUtf8.GetString(first);
            var envelope = ParseExactEnvelope(first);
            return envelope is not null && ValidateEnvelope(envelope)
                ? OperationResult<PersistedEnvelope>.Ok(envelope)
                : Fail<PersistedEnvelope>();
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return Fail<PersistedEnvelope>();
        }
    }

    private OperationResult<PersistedEnvelope> ReadCommittedEnvelopeLinux(
        string transactionId)
    {
        try
        {
            using var directories = OpenLinuxPrivateRoots(createMissing: false);
            if (directories is null)
                return Fail<PersistedEnvelope>();
            return ReadEnvelope(directories.Committed, CommittedLeafName(transactionId));
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return Fail<PersistedEnvelope>();
        }
    }

    private static OperationResult<PersistedEnvelope> ReadEnvelope(
        DesignConversionHandleBoundDirectory directory,
        string leafName,
        uint expectedLinkCount = 1)
    {
        try
        {
            using var stream = directory.TryOpenExistingReadOnly(leafName);
            return stream is null
                ? Fail<PersistedEnvelope>()
                : ReadEnvelope(directory, leafName, stream, expectedLinkCount);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return Fail<PersistedEnvelope>();
        }
    }

    private static OperationResult<PersistedEnvelope> ReadEnvelope(
        DesignConversionHandleBoundDirectory directory,
        string leafName,
        FileStream stream,
        uint expectedLinkCount = 1)
    {
        try
        {
            directory.ValidateLinuxCustodyFileTrustContract(
                stream,
                leafName,
                expectedLinkCount);
            var first = SecureLocalFileBoundary.ReadBoundedStableBytes(
                stream,
                MaximumEnvelopeBytes);
            if (first is null)
                return Fail<PersistedEnvelope>();
            directory.ValidateLinuxCustodyFileTrustContract(
                stream,
                leafName,
                expectedLinkCount);
            _ = StrictUtf8.GetString(first);
            var envelope = ParseExactEnvelope(first);
            return envelope is not null && ValidateEnvelope(envelope)
                ? OperationResult<PersistedEnvelope>.Ok(envelope)
                : Fail<PersistedEnvelope>();
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return Fail<PersistedEnvelope>();
        }
    }

    private static void DeleteExistingLinuxLeaf(
        DesignConversionHandleBoundDirectory directory,
        string leafName)
    {
        using var stream = directory.TryOpenExisting(
            leafName,
            FileShare.None,
            writeThrough: true);
        if (stream is null)
        {
            throw new DesignConversionHandleBoundTrustException(
                "Handle-bound Linux custody leaf disappeared before deletion.");
        }

        var identity = directory.GetIdentity(stream, leafName);
        directory.DeleteHeldRegular(stream, leafName, identity);
    }

    private static bool TryCompleteLinkedPublishRecovery(
        DesignConversionHandleBoundDirectory stageDirectory,
        string stageLeafName,
        DesignConversionHandleBoundDirectory committedDirectory,
        string committedLeafName,
        DesignConversionCandidatePayloadCustodyCommitment authority)
    {
        try
        {
            using var stage = stageDirectory.TryOpenExistingReadOnly(stageLeafName);
            using var committed = committedDirectory.TryOpenExistingReadOnly(
                committedLeafName);
            if (stage is null || committed is null)
                return false;

            var stageIdentity = stageDirectory.GetIdentity(stage, stageLeafName);
            var committedIdentity = committedDirectory.GetIdentity(
                committed,
                committedLeafName);
            if (stageIdentity != committedIdentity ||
                stageDirectory.GetLinkCount(stage, stageLeafName) != 2 ||
                committedDirectory.GetLinkCount(committed, committedLeafName) != 2)
            {
                return false;
            }

            var linkedEnvelope = ReadEnvelope(
                stageDirectory,
                stageLeafName,
                stage,
                expectedLinkCount: 2);
            if (!linkedEnvelope.IsSuccess ||
                !EnvelopeMatchesCommitment(linkedEnvelope.Value!, authority))
            {
                return false;
            }

            stageDirectory.DeleteHeldRegular(
                stage,
                stageLeafName,
                stageIdentity,
                expectedLinkCount: 2);
            var recovered = ReadEnvelope(committedDirectory, committedLeafName);
            return recovered.IsSuccess &&
                   EnvelopeMatchesCommitment(recovered.Value!, authority);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return false;
        }
    }

    private static string[]? EnumerateStagedBounded(string root)
    {
        var paths = Directory.EnumerateFiles(root, "*", SearchOption.TopDirectoryOnly)
            .Take(MaximumStagedTransactions + 1).ToArray();
        if (paths.Length > MaximumStagedTransactions || paths.Any(path =>
                HasExistingReparsePoint(path) ||
                !IsExpectedTransactionFile(path, StageSuffix)))
            return null;
        Array.Sort(paths, StringComparer.Ordinal);
        return paths;
    }

    private static string[]? EnumerateStagedBounded(
        DesignConversionHandleBoundDirectory directory)
    {
        try
        {
            var names = directory.EnumerateNames(MaximumStagedTransactions + 1)
                .ToArray();
            if (names.Length > MaximumStagedTransactions ||
                names.Any(name => !IsExpectedTransactionFile(name, StageSuffix)))
            {
                return null;
            }
            Array.Sort(names, StringComparer.Ordinal);
            return names;
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return null;
        }
    }

    private static string[] EnumerateCommittedLinux(
        DesignConversionHandleBoundDirectory directory)
    {
        var names = directory.EnumerateNames(int.MaxValue)
            .ToArray();
        if (names.Any(name => !IsExpectedTransactionFile(name, CommittedSuffix)))
        {
            throw new InvalidDataException(
                "Invalid committed custody transaction file.");
        }
        Array.Sort(names, StringComparer.Ordinal);
        return names;
    }

    private static bool IsExpectedTransactionFile(string path, string suffix)
    {
        var name = Path.GetFileName(path);
        return name.Length == 64 + suffix.Length &&
               name.EndsWith(suffix, StringComparison.Ordinal) &&
               IsLowerHex(name.AsSpan(0, 64));
    }

    private static string ParseTransactionId(string path)
    {
        var name = Path.GetFileName(path);
        var id = name[..64];
        if (!IsLowerHex(id.AsSpan()))
            throw new InvalidDataException("Invalid custody transaction file.");
        return id;
    }

    private static byte[]? SerializeBounded(PersistedEnvelope envelope)
    {
        try
        {
            using var memory = new MemoryStream();
            using var bounded = new BoundedWriteStream(memory, MaximumEnvelopeBytes);
            using (var writer = new Utf8JsonWriter(bounded))
            {
                writer.WriteStartObject();
                writer.WriteString("schemaVersion", envelope.SchemaVersion);
                writer.WriteString("transactionId", envelope.TransactionId);
                writer.WriteString("manifestSha256", envelope.ManifestSha256);
                writer.WriteString("bindingSetSha256", envelope.BindingSetSha256);
                writer.WriteString("authorityRecordId", envelope.AuthorityRecordId);
                writer.WriteString("authorityIdentitySha256", envelope.AuthorityIdentitySha256);
                writer.WriteString("authorityFingerprintSha256", envelope.AuthorityFingerprintSha256);
                writer.WriteStartArray("entries");
                foreach (var entry in envelope.Entries)
                {
                    writer.WriteStartObject();
                    writer.WriteString("trainingRecordId", entry.TrainingRecordId);
                    writer.WriteString("bindingSha256", entry.BindingSha256);
                    writer.WriteString("governanceSha256", entry.GovernanceSha256);
                    writer.WriteString("promptSha256", entry.PromptSha256);
                    writer.WriteString("chosenResponseSha256", entry.ChosenResponseSha256);
                    writer.WriteString("rejectedResponseSha256", entry.RejectedResponseSha256);
                    writer.WriteString("payloadVersion", entry.PayloadVersion);
                    writer.WriteString("prompt", entry.Prompt);
                    writer.WriteString("chosenResponse", entry.ChosenResponse);
                    writer.WriteString("rejectedResponse", entry.RejectedResponse);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
                writer.Flush();
            }
            return memory.Length is > 0 and <= MaximumEnvelopeBytes
                ? memory.ToArray()
                : null;
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return null;
        }
    }

    private static PersistedEnvelope? ParseExactEnvelope(byte[] bytes)
    {
        using var document = JsonDocument.Parse(
            bytes,
            new JsonDocumentOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 6
            });
        var root = document.RootElement;
        if (!HasExactProperties(root,
            [
                "schemaVersion", "transactionId", "manifestSha256",
                "bindingSetSha256", "authorityRecordId", "authorityIdentitySha256",
                "authorityFingerprintSha256", "entries"
            ]) ||
            !TryGetString(root, "schemaVersion", out var schemaVersion) ||
            !TryGetString(root, "transactionId", out var transactionId) ||
            !TryGetString(root, "manifestSha256", out var manifestSha256) ||
            !TryGetString(root, "bindingSetSha256", out var bindingSetSha256) ||
            !TryGetString(root, "authorityRecordId", out var authorityRecordId) ||
            !TryGetString(root, "authorityIdentitySha256", out var authorityIdentitySha256) ||
            !TryGetString(root, "authorityFingerprintSha256", out var authorityFingerprintSha256) ||
            !root.TryGetProperty("entries", out var entryArray) ||
            entryArray.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var entries = new List<PersistedEntry>();
        foreach (var element in entryArray.EnumerateArray())
        {
            if (entries.Count >= MaximumEntries ||
                !HasExactProperties(element,
                [
                    "trainingRecordId", "bindingSha256", "governanceSha256",
                    "promptSha256", "chosenResponseSha256", "rejectedResponseSha256",
                    "payloadVersion", "prompt", "chosenResponse", "rejectedResponse"
                ]) ||
                !TryGetString(element, "trainingRecordId", out var trainingRecordId) ||
                !TryGetString(element, "bindingSha256", out var bindingSha256) ||
                !TryGetString(element, "governanceSha256", out var governanceSha256) ||
                !TryGetString(element, "promptSha256", out var promptSha256) ||
                !TryGetString(element, "chosenResponseSha256", out var chosenResponseSha256) ||
                !TryGetString(element, "rejectedResponseSha256", out var rejectedResponseSha256) ||
                !TryGetString(element, "payloadVersion", out var payloadVersion) ||
                !TryGetString(element, "prompt", out var prompt) ||
                !TryGetString(element, "chosenResponse", out var chosenResponse) ||
                !TryGetString(element, "rejectedResponse", out var rejectedResponse))
            {
                return null;
            }
            entries.Add(new PersistedEntry(
                trainingRecordId,
                bindingSha256,
                governanceSha256,
                promptSha256,
                chosenResponseSha256,
                rejectedResponseSha256,
                payloadVersion,
                prompt,
                chosenResponse,
                rejectedResponse));
        }
        return entries.Count == 0
            ? null
            : new PersistedEnvelope(
                schemaVersion,
                transactionId,
                manifestSha256,
                bindingSetSha256,
                authorityRecordId,
                authorityIdentitySha256,
                authorityFingerprintSha256,
                entries);
    }

    private static bool HasExactProperties(
        JsonElement element,
        IReadOnlyList<string> expected)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return false;
        var properties = element.EnumerateObject().ToArray();
        return properties.Length == expected.Count &&
               properties.Select(static property => property.Name)
                   .Distinct(StringComparer.Ordinal).Count() == expected.Count &&
               expected.All(name => properties.Any(property =>
                   string.Equals(property.Name, name, StringComparison.Ordinal)));
    }

    private static bool TryGetString(
        JsonElement element,
        string name,
        out string value)
    {
        value = string.Empty;
        return element.TryGetProperty(name, out var property) &&
               property.ValueKind == JsonValueKind.String &&
               (value = property.GetString() ?? string.Empty) is not null;
    }

    private static PersistedEnvelope ToPersistedEnvelope(
        DesignConversionCandidatePayloadCustodyDraft draft,
        DesignConversionCandidatePayloadCustodyCommitment commitment)
        => new(
            SchemaVersion,
            commitment.TransactionId,
            commitment.ManifestSha256,
            commitment.BindingSetSha256,
            commitment.AuthorityRecordId,
            draft.AuthorityIdentitySha256,
            commitment.AuthorityFingerprintSha256,
            draft.Entries.Select(entry => new PersistedEntry(
                entry.TrainingRecordId,
                entry.BindingSha256,
                entry.GovernanceSha256,
                entry.PromptSha256,
                entry.ChosenResponseSha256,
                entry.RejectedResponseSha256,
                entry.Secret.Version,
                entry.Secret.Prompt,
                entry.Secret.ChosenResponse,
                entry.Secret.RejectedResponse)).ToArray());

    private static DesignConversionCandidatePayloadCustodyEntry? ToEntry(PersistedEntry entry)
    {
        var secret = new DesignConversionDpoPayloadCustodySecret(
            entry.PayloadVersion,
            entry.Prompt,
            entry.ChosenResponse,
            entry.RejectedResponse,
            entry.PromptSha256,
            entry.ChosenResponseSha256,
            entry.RejectedResponseSha256,
            entry.GovernanceSha256,
            entry.BindingSha256);
        var result = new DesignConversionCandidatePayloadCustodyEntry(
            entry.TrainingRecordId,
            entry.BindingSha256,
            entry.GovernanceSha256,
            entry.PromptSha256,
            entry.ChosenResponseSha256,
            entry.RejectedResponseSha256,
            secret);
        return ValidateEntry(result) ? result : null;
    }

    private static EntryMint ToEntryMint(PersistedEntry entry)
        => new(
            entry.TrainingRecordId,
            entry.BindingSha256,
            entry.GovernanceSha256,
            entry.PromptSha256,
            entry.ChosenResponseSha256,
            entry.RejectedResponseSha256);

    private static DesignConversionCandidatePayloadCustodyCommitment SnapshotCommitment(
        DesignConversionCandidatePayloadCustodyCommitment commitment)
        => new(
            commitment.TransactionId,
            commitment.ManifestSha256,
            commitment.BindingSetSha256,
            commitment.AuthorityRecordId,
            commitment.AuthorityFingerprintSha256,
            commitment.Bindings);

    private static bool ValidateEnvelope(PersistedEnvelope envelope)
        => string.Equals(envelope.SchemaVersion, SchemaVersion, StringComparison.Ordinal) &&
           IsLowerHex(envelope.TransactionId.AsSpan()) &&
           IsSha256Ref(envelope.ManifestSha256) &&
           IsSha256Ref(envelope.BindingSetSha256) &&
           IsSafeIdentity(envelope.AuthorityRecordId, 512) &&
           IsSha256Ref(envelope.AuthorityIdentitySha256) &&
           IsSha256Ref(envelope.AuthorityFingerprintSha256) &&
           envelope.Entries is { Count: > 0 and <= MaximumEntries } &&
           envelope.Entries.Select(static entry => entry.BindingSha256)
               .Distinct(StringComparer.Ordinal).Count() == envelope.Entries.Count &&
           envelope.Entries.Select(static entry => entry.TrainingRecordId)
               .Distinct(StringComparer.Ordinal).Count() == envelope.Entries.Count &&
           envelope.Entries.All(entry => ToEntry(entry) is not null) &&
           string.Equals(envelope.BindingSetSha256,
               Sha256Ref(string.Join("\n", envelope.Entries
                   .OrderBy(static entry => entry.BindingSha256, StringComparer.Ordinal)
                   .Select(static entry => entry.BindingSha256))), StringComparison.Ordinal) &&
           string.Equals(envelope.ManifestSha256,
               BuildManifestSha256(
                   envelope.TransactionId,
                   envelope.AuthorityRecordId,
                   envelope.AuthorityIdentitySha256,
                   envelope.BindingSetSha256,
                   envelope.Entries.Select(entry => ToEntry(entry)!).ToArray()),
               StringComparison.Ordinal);

    private static bool ValidateEntry(DesignConversionCandidatePayloadCustodyEntry entry)
        => entry is not null &&
           entry.Secret is not null &&
           string.Equals(
               entry.Secret.Version,
               DesignConversionDpoGenerator.PayloadCustodyVersion,
               StringComparison.Ordinal) &&
           IsSafeIdentity(entry.TrainingRecordId, 512) &&
           IsSha256Ref(entry.BindingSha256) &&
           IsSha256Ref(entry.GovernanceSha256) &&
           IsSha256Ref(entry.PromptSha256) &&
           IsSha256Ref(entry.ChosenResponseSha256) &&
           IsSha256Ref(entry.RejectedResponseSha256) &&
           IsValidBoundedUtf8(entry.Secret.Prompt) &&
           IsValidBoundedUtf8(entry.Secret.ChosenResponse) &&
           IsValidBoundedUtf8(entry.Secret.RejectedResponse) &&
           string.Equals(entry.PromptSha256, Sha256Ref(entry.Secret.Prompt), StringComparison.Ordinal) &&
           string.Equals(entry.ChosenResponseSha256, Sha256Ref(entry.Secret.ChosenResponse), StringComparison.Ordinal) &&
           string.Equals(entry.RejectedResponseSha256, Sha256Ref(entry.Secret.RejectedResponse), StringComparison.Ordinal) &&
           string.Equals(entry.BindingSha256, entry.Secret.TrainingRecordBindingSha256, StringComparison.Ordinal) &&
           string.Equals(entry.GovernanceSha256, entry.Secret.GovernanceSha256, StringComparison.Ordinal);

    private static bool CommitmentMatchesDraft(
        DesignConversionCandidatePayloadCustodyCommitment commitment,
        DesignConversionCandidatePayloadCustodyDraft draft)
        => commitment is not null && draft is not null && ValidateCommitment(commitment) &&
           string.Equals(commitment.TransactionId, draft.TransactionId, StringComparison.Ordinal) &&
           string.Equals(commitment.ManifestSha256, draft.ManifestSha256, StringComparison.Ordinal) &&
           string.Equals(commitment.BindingSetSha256, draft.BindingSetSha256, StringComparison.Ordinal) &&
           string.Equals(commitment.AuthorityRecordId, draft.AuthorityRecordId, StringComparison.Ordinal) &&
           commitment.Bindings.SequenceEqual(
               draft.Entries.Select(static entry => entry.BindingSha256), StringComparer.Ordinal);

    private static bool ValidateCommitment(DesignConversionCandidatePayloadCustodyCommitment commitment)
        => IsLowerHex(commitment.TransactionId.AsSpan()) &&
           IsSha256Ref(commitment.ManifestSha256) &&
           IsSha256Ref(commitment.BindingSetSha256) &&
           IsSafeIdentity(commitment.AuthorityRecordId, 512) &&
           IsSha256Ref(commitment.AuthorityFingerprintSha256) &&
           commitment.Bindings is { Count: > 0 and <= MaximumEntries } &&
           commitment.Bindings.All(IsSha256Ref) &&
           commitment.Bindings.Distinct(StringComparer.Ordinal).Count() == commitment.Bindings.Count &&
           string.Equals(commitment.BindingSetSha256,
               Sha256Ref(string.Join("\n", commitment.Bindings)), StringComparison.Ordinal);

    private static bool EnvelopeMatchesCommitment(
        PersistedEnvelope envelope,
        DesignConversionCandidatePayloadCustodyCommitment commitment)
        => ValidateEnvelope(envelope) && ValidateCommitment(commitment) &&
           string.Equals(envelope.TransactionId, commitment.TransactionId, StringComparison.Ordinal) &&
           string.Equals(envelope.ManifestSha256, commitment.ManifestSha256, StringComparison.Ordinal) &&
           string.Equals(envelope.BindingSetSha256, commitment.BindingSetSha256, StringComparison.Ordinal) &&
           string.Equals(envelope.AuthorityRecordId, commitment.AuthorityRecordId, StringComparison.Ordinal) &&
           string.Equals(envelope.AuthorityFingerprintSha256,
               commitment.AuthorityFingerprintSha256, StringComparison.Ordinal) &&
           envelope.Entries.Select(static entry => entry.BindingSha256)
               .SequenceEqual(commitment.Bindings, StringComparer.Ordinal);

    private static bool SameEnvelope(PersistedEnvelope left, PersistedEnvelope right)
    {
        var leftBytes = SerializeBounded(left);
        var rightBytes = SerializeBounded(right);
        return leftBytes is not null && rightBytes is not null &&
               leftBytes.AsSpan().SequenceEqual(rightBytes);
    }

    private static string BuildManifestSha256(
        string transactionId,
        string authorityRecordId,
        string authorityIdentitySha256,
        string bindingSetSha256,
        IReadOnlyList<DesignConversionCandidatePayloadCustodyEntry> entries)
        => Sha256Ref(JsonSerializer.Serialize(new
        {
            schemaVersion = SchemaVersion,
            transactionId,
            authorityRecordId,
            authorityIdentitySha256,
            bindingSetSha256,
            entries = entries.OrderBy(static entry => entry.BindingSha256, StringComparer.Ordinal)
                .Select(static entry => new
                {
                    entry.TrainingRecordId,
                    entry.BindingSha256,
                    entry.GovernanceSha256,
                    entry.PromptSha256,
                    entry.ChosenResponseSha256,
                    entry.RejectedResponseSha256
                })
        }, JsonOptions));

    private static bool IsValidBoundedUtf8(string value)
    {
        try
        {
            return StrictUtf8.GetByteCount(value ?? string.Empty) <= MaximumFieldBytes;
        }
        catch (EncoderFallbackException)
        {
            return false;
        }
    }

    private static bool IsSafeIdentity(string value, int maximumBytes)
    {
        if (string.IsNullOrWhiteSpace(value) || value.IndexOfAny(['\r', '\n', '\0']) >= 0)
            return false;
        try
        {
            return StrictUtf8.GetByteCount(value) <= maximumBytes;
        }
        catch (EncoderFallbackException)
        {
            return false;
        }
    }

    private static bool IsSha256Ref(string value)
        => value is { Length: 71 } && value.StartsWith("sha256:", StringComparison.Ordinal) &&
           IsLowerHex(value.AsSpan(7));

    private static bool IsLowerHex(ReadOnlySpan<char> value)
        => value.Length == 64 && value.IndexOfAnyExceptInRange('0', '9') < 0
            ? true
            : value.Length == 64 && value.ToString().All(static character =>
                character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static string HashHex(string value)
        => Convert.ToHexString(SHA256.HashData(StrictUtf8.GetBytes(value))).ToLowerInvariant();

    private static string Sha256Ref(string value) => "sha256:" + HashHex(value);
    private static string Sha256Ref(byte[] bytes)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static bool HasExistingReparsePoint(string path)
        => SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path);

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    private static bool IsNonFatal(Exception exception)
        => exception is not OutOfMemoryException and not StackOverflowException and not AccessViolationException
           and not DesignConversionCandidatePayloadCustodyCrashException;

    private static OperationResult<T> Fail<T>()
        => OperationResult<T>.Fail(
            "DesignConversion candidate payload custody is unavailable or corrupt.",
            "DESIGN_CONVERSION_CANDIDATE_PAYLOAD_CUSTODY_FAILED");

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    private sealed class PersistedEnvelope
    {
        private readonly IReadOnlyList<PersistedEntry> _entries;

        internal PersistedEnvelope(
            string schemaVersion,
            string transactionId,
            string manifestSha256,
            string bindingSetSha256,
            string authorityRecordId,
            string authorityIdentitySha256,
            string authorityFingerprintSha256,
            IReadOnlyList<PersistedEntry> entries)
        {
            SchemaVersion = schemaVersion;
            TransactionId = transactionId;
            ManifestSha256 = manifestSha256;
            BindingSetSha256 = bindingSetSha256;
            AuthorityRecordId = authorityRecordId;
            AuthorityIdentitySha256 = authorityIdentitySha256;
            AuthorityFingerprintSha256 = authorityFingerprintSha256;
            _entries = entries;
        }

        internal string SchemaVersion { get; }
        internal string TransactionId { get; }
        internal string ManifestSha256 { get; }
        internal string BindingSetSha256 { get; }
        internal string AuthorityRecordId { get; }
        internal string AuthorityIdentitySha256 { get; }
        internal string AuthorityFingerprintSha256 { get; }
        [System.Text.Json.Serialization.JsonIgnore]
        internal IReadOnlyList<PersistedEntry> Entries => _entries;

        private string DebuggerDisplay =>
            $"PersistedPayloadCustodyEnvelope(tx={TransactionId}, entries={_entries.Count}, raw=[REDACTED])";

        public override string ToString() => DebuggerDisplay;
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    private sealed class PersistedEntry
    {
        private readonly string _prompt;
        private readonly string _chosenResponse;
        private readonly string _rejectedResponse;

        internal PersistedEntry(
            string trainingRecordId,
            string bindingSha256,
            string governanceSha256,
            string promptSha256,
            string chosenResponseSha256,
            string rejectedResponseSha256,
            string payloadVersion,
            string prompt,
            string chosenResponse,
            string rejectedResponse)
        {
            TrainingRecordId = trainingRecordId;
            BindingSha256 = bindingSha256;
            GovernanceSha256 = governanceSha256;
            PromptSha256 = promptSha256;
            ChosenResponseSha256 = chosenResponseSha256;
            RejectedResponseSha256 = rejectedResponseSha256;
            PayloadVersion = payloadVersion;
            _prompt = prompt;
            _chosenResponse = chosenResponse;
            _rejectedResponse = rejectedResponse;
        }

        internal string TrainingRecordId { get; }
        internal string BindingSha256 { get; }
        internal string GovernanceSha256 { get; }
        internal string PromptSha256 { get; }
        internal string ChosenResponseSha256 { get; }
        internal string RejectedResponseSha256 { get; }
        internal string PayloadVersion { get; }
        [System.Text.Json.Serialization.JsonIgnore]
        internal string Prompt => _prompt;
        [System.Text.Json.Serialization.JsonIgnore]
        internal string ChosenResponse => _chosenResponse;
        [System.Text.Json.Serialization.JsonIgnore]
        internal string RejectedResponse => _rejectedResponse;

        private string DebuggerDisplay =>
            $"PersistedPayloadCustodyEntry(record={TrainingRecordId}, binding={BindingSha256}, raw=[REDACTED])";

        public override string ToString() => DebuggerDisplay;
    }

    private sealed class BoundedWriteStream(Stream inner, long maximumLength)
        : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => inner.Length;
        public override long Position
        {
            get => inner.Position;
            set => throw new NotSupportedException();
        }

        public override void Flush() => inner.Flush();
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureCapacity(count);
            inner.Write(buffer, offset, count);
        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureCapacity(buffer.Length);
            inner.Write(buffer);
        }
        private void EnsureCapacity(int count)
        {
            if (count < 0 || inner.Length > maximumLength - count)
                throw new InvalidDataException("Custody envelope exceeds its storage bound.");
        }
        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();
        public override void SetLength(long value) =>
            throw new NotSupportedException();
    }
}
