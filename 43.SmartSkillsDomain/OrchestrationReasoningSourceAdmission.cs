// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;

public sealed record OrchestrationReasoningSourceAdmissionReceipt(
    int SchemaVersion,
    string PayloadSha256,
    string ManifestSha256,
    string LedgerSha256,
    string EvidenceSha256,
    string AllowedSourceKind,
    IReadOnlyList<string> RowIds,
    IReadOnlyList<string> SourceIds,
    IReadOnlyList<string> PairIds,
    IReadOnlyList<string> SourceRefs,
    IReadOnlyList<string> SourceSha256s,
    string LearnedPatternsSha256,
    string StateSha256,
    IReadOnlyList<string> ParentReceiptSha256s,
    string ReceiptSha256);

/// <summary>
/// Public persistence reference only. Possessing or deserializing this DTO does
/// not grant runtime admission authority.
/// </summary>
public sealed record OrchestrationReasoningSourceMaterialReference(
    string PayloadRef,
    string ManifestRef,
    string LedgerRef,
    string EvidenceRef);

/// <summary>
/// Serializable persistence package. This is evidence needed to ask the policy
/// to reconstruct authority; possessing or deserializing it grants no runtime
/// authority by itself.
/// </summary>
public sealed record OrchestrationReasoningAdmittedLearnedStatePersistencePackage(
    int SchemaVersion,
    OrchestrationReasoningLearnedGuideStateDescriptor State,
    OrchestrationReasoningSourceAdmissionReceipt Receipt,
    IReadOnlyList<OrchestrationReasoningSourceMaterialReference> SourceMaterials);

/// <summary>
/// Opaque runtime capability. It has no public constructor, JSON constructor,
/// setters, or public factory that can bind caller-supplied state.
/// </summary>
public sealed class OrchestrationReasoningAdmittedLearnedState
{
    private readonly object _authority;
    private readonly IReadOnlyList<OrchestrationReasoningVerifiedSourceMaterial> _verifiedSources;

    internal OrchestrationReasoningAdmittedLearnedState(
        OrchestrationReasoningLearnedGuideStateDescriptor state,
        OrchestrationReasoningSourceAdmissionReceipt receipt,
        IReadOnlyList<OrchestrationReasoningVerifiedSourceMaterial> verifiedSources,
        object authority)
    {
        State = state;
        Receipt = receipt;
        _verifiedSources = verifiedSources;
        _authority = authority;
    }

    public OrchestrationReasoningLearnedGuideStateDescriptor State { get; }

    public OrchestrationReasoningSourceAdmissionReceipt Receipt { get; }

    internal IReadOnlyList<OrchestrationReasoningVerifiedSourceMaterial> VerifiedSources
        => _verifiedSources;

    internal object Authority => _authority;
}

public sealed record OrchestrationReasoningSourceAdmissionProof(
    int SchemaVersion,
    OrchestrationReasoningSourceAdmissionReceipt Receipt,
    IReadOnlyList<OrchestrationReasoningSourceMaterialReference> SourceMaterials);

internal sealed record OrchestrationReasoningSourceAdmissionDecision(
    string AllowedSourceKind,
    IReadOnlyList<string> RowIds,
    IReadOnlyList<string> SourceIds,
    IReadOnlyList<string> PairIds,
    IReadOnlyList<string> SourceRefs,
    IReadOnlyList<string> SourceSha256s,
    string PayloadSha256,
    string ManifestSha256,
    string LedgerSha256,
    string EvidenceSha256);

internal sealed record OrchestrationReasoningVerifiedSourceMaterial(
    string PayloadPath,
    string ManifestPath,
    string LedgerPath,
    string EvidencePath,
    byte[] PayloadBytes,
    byte[] ManifestBytes,
    byte[] LedgerBytes,
    byte[] EvidenceBytes,
    OrchestrationReasoningSourceAdmissionDecision Decision);

/// <summary>
/// Evidence-bound R17 admission. Input booleans and source labels never grant
/// authority: the ledger and evidence must bind the exact payload and manifest
/// hashes and all row/source/pair/ref identities.
/// </summary>
public static class OrchestrationReasoningSourceAdmission
{
    private static readonly object RuntimeAuthority = new();
    private static readonly ConcurrentDictionary<string, OrchestrationReasoningAdmittedLearnedState>
        RuntimeReferences = new(StringComparer.Ordinal);

    public const string CanonicalLedgerPath =
        "training-data/orchestration-reasoning/source-admission-ledger.json";

    public const string RebuildRequiredCode =
        "ORCHESTRATION_REASONING_SOURCE_ADMISSION_REBUILD_REQUIRED";

    public const string AdmissionRejectedCode =
        "ORCHESTRATION_REASONING_SOURCE_ADMISSION_REJECTED";

    public const string StateAdmissionInvalidCode =
        "ORCHESTRATION_REASONING_STATE_SOURCE_ADMISSION_INVALID";

    public const string ProvenanceRebuildRequiredCode =
        "ORCHESTRATION_REASONING_CHECKPOINT_SOURCE_PROVENANCE_REBUILD_REQUIRED";

    public const string ProofNotFoundCode =
        "ORCHESTRATION_REASONING_ADMISSION_PROOF_NOT_FOUND";

    public const string ProofHashMismatchCode =
        "ORCHESTRATION_REASONING_ADMISSION_PROOF_HASH_MISMATCH";

    public const string ProofImmutableConflictCode =
        "ORCHESTRATION_REASONING_ADMISSION_PROOF_IMMUTABLE_CONFLICT";

    public const string ProofSourceMaterialOutsideBundleCode =
        "ORCHESTRATION_REASONING_ADMISSION_PROOF_SOURCE_MATERIAL_OUTSIDE_BUNDLE";

    public const string ProjectInternalSelfAuthored = "project_internal_self_authored";
    public const string ILearningDynamicGraph = "ilearning_dynamic_graph";

    private static readonly HashSet<string> AllowedKinds = new(StringComparer.Ordinal)
    {
        ProjectInternalSelfAuthored,
        ILearningDynamicGraph
    };

    private static readonly HashSet<string> DeniedIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "or01u-sonnet-clinerules-001",
        "or01u-sonnet-cursorrules-001",
        "or01u-sonnet-gitattributes-001",
        "or01u-sonnet-gitignore-001",
        "or-expanded-v1-pair-001",
        "or-expanded-v1-pair-002",
        "or-expanded-v1-pair-003"
    };

    private static readonly HashSet<string> DeniedHashes = new(StringComparer.OrdinalIgnoreCase)
    {
        "af174a555ca99292c0c87210665536dec6d520f2bd00532397ccf9318383fb05",
        "a106495ccbfc9cbc2651d8070fc0b4c75d2d3b769c97b793d69eb8826299d564",
        "15ee6946aa77f025b11e04ad8f73c801009ebbf56e365a4fe6d2bb43a08d64f5",
        "fe16ff2ddcb4eac73110997e47eb3893b6158ae726b37fb20ba3977303f8ae0c"
    };

    private const int MaximumHostCheckSetLabelLength = 64;

    private static readonly HashSet<string> AllowedHostCheckSetLabels = new(StringComparer.Ordinal)
    {
        "claude",
        "codex",
        "core-local-eval",
        "core-only",
        "core-only-no-host-runtime",
        "cursor",
        "extension-api-core",
        "host-boundary-from-request",
        "repo-local-fixture",
        "test-temp-fixture"
    };

    internal static async Task<OperationResult<OrchestrationReasoningVerifiedSourceMaterial>> AdmitAsync(
        string payloadPath,
        string manifestPath,
        string? ledgerPath,
        string? evidencePath,
        CancellationToken ct = default)
    {
        var resolvedLedgerPath = string.IsNullOrWhiteSpace(ledgerPath)
            ? CanonicalLedgerPath
            : ledgerPath;

        if (!File.Exists(resolvedLedgerPath))
            return OperationResult<OrchestrationReasoningVerifiedSourceMaterial>.Fail(
                "The production source-admission ledger is absent; rebuild with verified R17 evidence before materialization or training.",
                RebuildRequiredCode);
        if (string.IsNullOrWhiteSpace(evidencePath) || !File.Exists(evidencePath))
            return OperationResult<OrchestrationReasoningVerifiedSourceMaterial>.Fail(
                "Verified source-admission evidence is required.",
                AdmissionRejectedCode);
        if (!File.Exists(payloadPath) || !File.Exists(manifestPath))
            return OperationResult<OrchestrationReasoningVerifiedSourceMaterial>.Fail(
                "Payload and manifest must exist before source admission.",
                AdmissionRejectedCode);

        try
        {
            var payloadBytes = await File.ReadAllBytesAsync(payloadPath, ct);
            var manifestBytes = await File.ReadAllBytesAsync(manifestPath, ct);
            var ledgerBytes = await File.ReadAllBytesAsync(resolvedLedgerPath, ct);
            var evidenceBytes = await File.ReadAllBytesAsync(evidencePath, ct);
            return AdmitVerifiedSourceMaterial(
                payloadPath,
                manifestPath,
                resolvedLedgerPath,
                evidencePath!,
                payloadBytes,
                manifestBytes,
                ledgerBytes,
                evidenceBytes);
        }
        catch (Exception ex) when (ex is JsonException or IOException or InvalidDataException)
        {
            return OperationResult<OrchestrationReasoningVerifiedSourceMaterial>.Fail(
                ex.Message,
                AdmissionRejectedCode);
        }
    }

    internal static OperationResult<OrchestrationReasoningVerifiedSourceMaterial> AdmitVerifiedSourceMaterial(
        string payloadPath,
        string manifestPath,
        string ledgerPath,
        string evidencePath,
        byte[] payloadBytes,
        byte[] manifestBytes,
        byte[] ledgerBytes,
        byte[] evidenceBytes)
    {
        if (string.IsNullOrWhiteSpace(payloadPath)
            || string.IsNullOrWhiteSpace(manifestPath)
            || string.IsNullOrWhiteSpace(ledgerPath)
            || string.IsNullOrWhiteSpace(evidencePath))
        {
            return Reject("Payload, manifest, ledger, and evidence paths are required for source admission.");
        }

        try
        {
            var payloadSha = OrchestrationReasoningCanonicalState.HashBytes(payloadBytes);
            var manifestSha = OrchestrationReasoningCanonicalState.HashBytes(manifestBytes);
            var ledgerSha = OrchestrationReasoningCanonicalState.HashBytes(ledgerBytes);
            var evidenceSha = OrchestrationReasoningCanonicalState.HashBytes(evidenceBytes);

            using var ledgerDocument = JsonDocument.Parse(ledgerBytes);
            using var evidenceDocument = JsonDocument.Parse(evidenceBytes);
            var ledgerRoot = ledgerDocument.RootElement;
            var evidenceRoot = evidenceDocument.RootElement;

            if (ContainsHardDeniedMaterial(ledgerRoot) || ContainsHardDeniedMaterial(evidenceRoot))
                return Reject("R17 hard-denied Sonnet or derived expanded-v1 material is present.");

            if (!EqualsHash(evidenceRoot, "payload_sha256", payloadSha)
                || !EqualsHash(evidenceRoot, "manifest_sha256", manifestSha)
                || !EqualsHash(evidenceRoot, "ledger_sha256", ledgerSha))
            {
                return Reject("Admission evidence does not bind the exact payload, manifest, and ledger hashes.");
            }

            var entries = RequiredArray(ledgerRoot, "entries")
                .EnumerateArray()
                .ToArray();
            if (entries.Length == 0)
                return Reject("Source-admission ledger contains no entries.");

            var rows = ParsePayloadRows(payloadBytes);
            if (rows.Count == 0)
                return Reject("Payload contains no rows.");
            if (rows.Any(ContainsHardDeniedMaterial))
                return Reject("R17 hard-denied Sonnet or derived expanded-v1 material is present.");

            var bindings = entries.ToDictionary(
                entry => FirstRequiredString(entry, "pair_id", "row_id", "source_row_id"),
                entry => entry,
                StringComparer.Ordinal);
            var rowIds = new List<string>();
            var sourceIds = new List<string>();
            var pairIds = new List<string>();
            var sourceRefs = new List<string>();
            var sourceHashes = new List<string>();
            var adaptiveRowHashes = new List<string>();
            var adaptiveChosenRefs = new List<string>();
            var adaptiveRejectedRefs = new List<string>();
            var kinds = new HashSet<string>(StringComparer.Ordinal);

            foreach (var row in rows)
            {
                var adaptive = IsAdaptiveSignal(row);
                var pairId = adaptive
                    ? FirstRequiredString(row, "signalId", "signal_id")
                    : FirstRequiredString(row, "pair_id", "training_pair_id");
                var rowId = adaptive
                    ? FirstRequiredString(row, "signalId", "signal_id")
                    : FirstRequiredString(row, "source_row_id", "row_id");
                var sourceId = adaptive
                    ? FirstRequiredString(row, "sourcePromptLedgerId", "source_prompt_ledger_id")
                    : FirstOptionalString(row, "source_id", "source_candidate_row_id") ?? rowId;
                var sourceRef = adaptive
                    ? FirstRequiredString(row, "chosenRef", "chosen_ref")
                    : FirstRequiredString(row, "source_ref");
                var sourceHash = (adaptive
                    ? FirstRequiredString(row, "rawPromptSha256", "raw_prompt_sha256")
                    : FirstRequiredString(row, "source_sha256")).ToLowerInvariant();

                if (!bindings.TryGetValue(pairId, out var entry)
                    && !bindings.TryGetValue(rowId, out entry))
                {
                    return Reject($"No verified ledger entry binds payload pair '{pairId}'.");
                }

                var kind = FirstRequiredString(entry, "source_kind");
                if (!AllowedKinds.Contains(kind))
                    return Reject($"Source kind '{kind}' is not admissible.");
                if (!MatchesAny(entry, pairId, "pair_id", "training_pair_id")
                    || !MatchesAny(entry, rowId, "row_id", "source_row_id", "signal_id")
                    || !MatchesAny(entry, sourceId, "source_id", "source_candidate_row_id", "source_prompt_ledger_id")
                    || !MatchesAny(entry, sourceRef, "source_ref", "chosen_ref")
                    || !MatchesAny(entry, sourceHash, "source_sha256", "raw_prompt_sha256"))
                {
                    return Reject($"Ledger entry does not bind every identity for pair '{pairId}'.");
                }

                if (adaptive)
                {
                    var chosenRef = FirstRequiredString(row, "chosenRef", "chosen_ref");
                    var rejectedRef = FirstRequiredString(row, "rejectedRef", "rejected_ref");
                    var rowSha = HashCanonicalJson(row);
                    if (!string.Equals(kind, ILearningDynamicGraph, StringComparison.Ordinal)
                        || !MatchesAny(entry, chosenRef, "chosen_ref")
                        || !MatchesAny(entry, rejectedRef, "rejected_ref")
                        || !MatchesAny(entry, rowSha, "row_sha256"))
                    {
                        return Reject($"Dynamic-graph ledger entry does not bind the exact signal bytes and refs for '{rowId}'.");
                    }
                    adaptiveRowHashes.Add(rowSha);
                    adaptiveChosenRefs.Add(chosenRef);
                    adaptiveRejectedRefs.Add(rejectedRef);
                }

                kinds.Add(kind);
                rowIds.Add(rowId);
                sourceIds.Add(sourceId);
                pairIds.Add(pairId);
                sourceRefs.Add(sourceRef);
                sourceHashes.Add(sourceHash);
            }
            if (bindings.Count != rows.Count
                || rowIds.Distinct(StringComparer.Ordinal).Count() != rowIds.Count
                || pairIds.Distinct(StringComparer.Ordinal).Count() != pairIds.Count)
            {
                return Reject("Ledger and payload must have a one-to-one binding with unique row and pair ids.");
            }

            var evidenceKind = FirstRequiredString(evidenceRoot, "verified_source_kind", "source_kind");
            if (kinds.Count != 1 || !kinds.Contains(evidenceKind) || !AllowedKinds.Contains(evidenceKind))
                return Reject("Evidence source kind does not exactly match the verified ledger entries.");

            if (!SetEquals(evidenceRoot, "row_ids", rowIds)
                || !SetEquals(evidenceRoot, "source_ids", sourceIds)
                || !SetEquals(evidenceRoot, "pair_ids", pairIds)
                || !SetEquals(evidenceRoot, "source_refs", sourceRefs)
                || !SetEquals(evidenceRoot, "source_sha256s", sourceHashes))
            {
                return Reject("Evidence does not bind the exact row/source/pair/ref/hash sets.");
            }
            if (string.Equals(evidenceKind, ILearningDynamicGraph, StringComparison.Ordinal))
            {
                using var manifestDocument = JsonDocument.Parse(manifestBytes);
                var manifestRoot = manifestDocument.RootElement;
                if (!EqualsHash(manifestRoot, "payloads_sha256", payloadSha)
                    || !MatchesInt(manifestRoot, "row_count", rows.Count)
                    || !MatchesAny(manifestRoot, ILearningDynamicGraph, "source_kind")
                    || !SetEquals(evidenceRoot, "row_sha256s", adaptiveRowHashes)
                    || !SetEquals(evidenceRoot, "chosen_refs", adaptiveChosenRefs)
                    || !SetEquals(evidenceRoot, "rejected_refs", adaptiveRejectedRefs))
                {
                    return Reject("Dynamic-graph manifest/evidence does not bind the exact batch, rows, and chosen/rejected refs.");
                }
            }

            var decision = new OrchestrationReasoningSourceAdmissionDecision(
                    evidenceKind,
                    NormalizeSet(rowIds),
                    NormalizeSet(sourceIds),
                    NormalizeSet(pairIds),
                    NormalizeSet(sourceRefs),
                    NormalizeSet(sourceHashes),
                    payloadSha,
                    manifestSha,
                    ledgerSha,
                    evidenceSha);
            return OperationResult<OrchestrationReasoningVerifiedSourceMaterial>.Ok(
                new OrchestrationReasoningVerifiedSourceMaterial(
                    Path.GetFullPath(payloadPath),
                    Path.GetFullPath(manifestPath),
                    Path.GetFullPath(ledgerPath),
                    Path.GetFullPath(evidencePath),
                    payloadBytes,
                    manifestBytes,
                    ledgerBytes,
                    evidenceBytes,
                    decision));
        }
        catch (Exception ex) when (ex is JsonException or IOException or InvalidDataException)
        {
            return OperationResult<OrchestrationReasoningVerifiedSourceMaterial>.Fail(
                ex.Message,
                AdmissionRejectedCode);
        }
    }

    internal static OrchestrationReasoningAdmittedLearnedState CreateAdmitted(
        OrchestrationReasoningLearnedGuideStateDescriptor state,
        IReadOnlyList<OrchestrationReasoningVerifiedSourceMaterial> verifiedSources,
        IReadOnlyList<string>? parentReceiptSha256s = null)
    {
        if (verifiedSources is null || verifiedSources.Count == 0)
            throw new InvalidDataException("At least one verified source material is required.");
        var decisions = verifiedSources.Select(source => source.Decision).ToArray();
        var receipt = new OrchestrationReasoningSourceAdmissionReceipt(
            SchemaVersion: 1,
            PayloadSha256: CombineHashes(decisions.Select(decision => decision.PayloadSha256)),
            ManifestSha256: CombineHashes(decisions.Select(decision => decision.ManifestSha256)),
            LedgerSha256: CombineHashes(decisions.Select(decision => decision.LedgerSha256)),
            EvidenceSha256: CombineHashes(decisions.Select(decision => decision.EvidenceSha256)),
            AllowedSourceKind: CombineKinds(decisions.Select(decision => decision.AllowedSourceKind)),
            RowIds: NormalizeSet(decisions.SelectMany(decision => decision.RowIds)),
            SourceIds: NormalizeSet(decisions.SelectMany(decision => decision.SourceIds)),
            PairIds: NormalizeSet(decisions.SelectMany(decision => decision.PairIds)),
            SourceRefs: NormalizeSet(decisions.SelectMany(decision => decision.SourceRefs)),
            SourceSha256s: NormalizeSet(decisions.SelectMany(decision => decision.SourceSha256s)),
            LearnedPatternsSha256: state.LearnedPatternsSha256 ?? string.Empty,
            StateSha256: state.StateSha256,
            ParentReceiptSha256s: NormalizeSet(parentReceiptSha256s ?? []),
            ReceiptSha256: string.Empty);
        receipt = receipt with
        {
            ReceiptSha256 = OrchestrationReasoningCanonicalState.ComputeReceiptSha256(receipt)
        };
        return new OrchestrationReasoningAdmittedLearnedState(
            state,
            receipt,
            verifiedSources.ToArray(),
            RuntimeAuthority);
    }

    public static OperationResult<bool> Validate(
        OrchestrationReasoningAdmittedLearnedState? admittedState)
    {
        if (admittedState?.State is null || admittedState.Receipt is null)
            return Invalid("Admitted state and receipt are required.");
        if (!ReferenceEquals(admittedState.Authority, RuntimeAuthority)
            || admittedState.VerifiedSources.Count == 0)
        {
            return Invalid("Runtime admission authority is absent.");
        }
        if (admittedState.Receipt.AllowedSourceKind
            .Split('+', StringSplitOptions.RemoveEmptyEntries)
            .Any(kind => !AllowedKinds.Contains(kind)))
            return Invalid("Admission receipt source kind is not allowed.");
        return ValidateReceiptAgainstState(admittedState.State, admittedState.Receipt);
    }

    internal static OperationResult<bool> ValidateReceiptAgainstState(
        OrchestrationReasoningLearnedGuideStateDescriptor? state,
        OrchestrationReasoningSourceAdmissionReceipt? receipt)
    {
        if (state is null || receipt is null)
            return Invalid("Learned state and persistence receipt are required.");
        if (receipt.RowIds is null
            || receipt.SourceIds is null
            || receipt.PairIds is null
            || receipt.SourceRefs is null
            || receipt.SourceSha256s is null
            || receipt.ParentReceiptSha256s is null)
        {
            return Invalid("Admission receipt identity collections are required.");
        }
        if (!OrchestrationReasoningCanonicalState.TryValidate(state, out var stateError))
            return Invalid(stateError);
        if (state.LearnedPatterns!.Any(PatternContainsHardDeniedMaterial))
            return Invalid("Admitted learned state contains R17 hard-denied material.");
        if (!string.Equals(receipt.LearnedPatternsSha256, state.LearnedPatternsSha256, StringComparison.Ordinal)
            || !string.Equals(receipt.StateSha256, state.StateSha256, StringComparison.Ordinal))
            return Invalid("Admission receipt does not bind the learned pattern and state hashes.");

        try
        {
            if (receipt.SchemaVersion != 1
                || receipt.RowIds.Count == 0
                || receipt.SourceIds.Count == 0
                || receipt.PairIds.Count == 0
                || receipt.SourceRefs.Count == 0
                || receipt.SourceSha256s.Count == 0)
                return Invalid("Admission receipt identity bindings are incomplete.");
            if (receipt.RowIds
                    .Concat(receipt.SourceIds)
                    .Concat(receipt.PairIds)
                    .Concat(receipt.SourceRefs)
                    .Concat(receipt.SourceSha256s)
                    .Any(IsHardDeniedString))
                return Invalid("Admission receipt contains R17 hard-denied material.");
            var receiptHash = OrchestrationReasoningCanonicalState.ComputeReceiptSha256(receipt);
            if (!string.Equals(receiptHash, receipt.ReceiptSha256, StringComparison.Ordinal))
                return Invalid("Admission receipt hash mismatch.");
        }
        catch (Exception ex) when (ex is InvalidDataException or ArgumentException)
        {
            return Invalid(ex.Message);
        }
        return OperationResult<bool>.Ok(true);
    }

    internal static OperationResult<OrchestrationReasoningAdmittedLearnedState> Merge(
        OrchestrationReasoningAdmittedLearnedState existing,
        OrchestrationReasoningAdmittedLearnedState delta,
        string stateId,
        string guideStateRef,
        string sourceBaseCommit)
    {
        var existingValidation = Validate(existing);
        if (!existingValidation.IsSuccess)
            return existingValidation.MapError<OrchestrationReasoningAdmittedLearnedState>();
        var deltaValidation = Validate(delta);
        if (!deltaValidation.IsSuccess)
            return deltaValidation.MapError<OrchestrationReasoningAdmittedLearnedState>();

        var existingIds = existing.State.LearnedPatterns!
            .Select(pattern => pattern.PatternId)
            .ToHashSet(StringComparer.Ordinal);
        if (delta.State.LearnedPatterns!.Any(pattern => existingIds.Contains(pattern.PatternId)))
            return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(
                "Delta learned pattern id collides with existing learned state.",
                "ORCHESTRATION_REASONING_CONTINUE_DUPLICATE_PATTERN_ID");

        try
        {
            var mergedPatterns = existing.State.LearnedPatterns!
                .Concat(delta.State.LearnedPatterns!)
                .ToArray();
            var mergedState = OrchestrationReasoningCanonicalState.CreateDescriptor(
                stateId,
                guideStateRef,
                $"{existing.State.ManifestId};delta:{delta.State.ManifestId}",
                existing.State.ManifestSha256,
                sourceBaseCommit,
                mergedPatterns);
            var merged = CreateAdmitted(
                mergedState,
                existing.VerifiedSources.Concat(delta.VerifiedSources).ToArray(),
                [existing.Receipt.ReceiptSha256, delta.Receipt.ReceiptSha256]);
            return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Ok(merged);
        }
        catch (Exception ex) when (ex is InvalidDataException or ArgumentException)
        {
            return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(
                ex.Message,
                StateAdmissionInvalidCode);
        }
    }

    public static string CreateRuntimeReference(
        OrchestrationReasoningAdmittedLearnedState admittedState)
    {
        var validation = Validate(admittedState);
        if (!validation.IsSuccess)
            throw new InvalidOperationException(validation.ErrorMessage);
        var reference = $"orchestration-reasoning-admitted:{Guid.NewGuid():N}";
        RuntimeReferences[reference] = admittedState;
        return reference;
    }

    internal static OperationResult<OrchestrationReasoningAdmittedLearnedState> ResolveRuntimeReference(
        string reference)
        => !string.IsNullOrWhiteSpace(reference)
            && RuntimeReferences.TryRemove(reference, out var admitted)
            && Validate(admitted).IsSuccess
            ? OperationResult<OrchestrationReasoningAdmittedLearnedState>.Ok(admitted)
            : OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(
                "A live opaque admitted-state reference is required; serialized state and receipts are not authority.",
                StateAdmissionInvalidCode);

    public static OrchestrationReasoningAdmittedLearnedStatePersistencePackage CreatePersistencePackage(
        OrchestrationReasoningAdmittedLearnedState admittedState)
    {
        var validation = Validate(admittedState);
        if (!validation.IsSuccess)
            throw new InvalidOperationException(validation.ErrorMessage);
        return new OrchestrationReasoningAdmittedLearnedStatePersistencePackage(
            SchemaVersion: 1,
            State: admittedState.State,
            Receipt: admittedState.Receipt,
            SourceMaterials: GetSourceReferences(admittedState));
    }

    public static async Task<OperationResult<OrchestrationReasoningAdmittedLearnedState>> RehydrateAsync(
        OrchestrationReasoningAdmittedLearnedStatePersistencePackage? package,
        CancellationToken ct = default)
    {
        if (package is null
            || package.SchemaVersion != 1
            || package.State is null
            || package.Receipt is null
            || package.SourceMaterials is null
            || package.SourceMaterials.Count == 0
            || package.SourceMaterials.Any(source =>
                source is null
                || string.IsNullOrWhiteSpace(source.PayloadRef)
                || string.IsNullOrWhiteSpace(source.ManifestRef)
                || string.IsNullOrWhiteSpace(source.LedgerRef)
                || string.IsNullOrWhiteSpace(source.EvidenceRef)))
        {
            return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(
                "Persistence package schema, state, receipt, and independently verifiable source materials are required.",
                ProofNotFoundCode);
        }

        var persistenceValidation = ValidateReceiptAgainstState(package.State, package.Receipt);
        if (!persistenceValidation.IsSuccess)
            return persistenceValidation.MapError<OrchestrationReasoningAdmittedLearnedState>();

        var reconstructedSources = new List<OrchestrationReasoningAdmittedLearnedState>();
        foreach (var source in package.SourceMaterials)
        {
            var result = await AdmitAsync(
                source.PayloadRef,
                source.ManifestRef,
                source.LedgerRef,
                source.EvidenceRef,
                ct);
            if (!result.IsSuccess)
                return result.MapError<OrchestrationReasoningAdmittedLearnedState>();
            var patternResult = OrchestrationReasoningTrainer.MaterializePatternsFromPayload(
                result.Value!.PayloadBytes);
            if (!patternResult.IsSuccess)
                return patternResult.MapError<OrchestrationReasoningAdmittedLearnedState>();

            try
            {
                using var manifestDocument = JsonDocument.Parse(result.Value.ManifestBytes);
                var manifest = manifestDocument.RootElement;
                if (!manifest.TryGetProperty("source_pairs_sha256", out var sourcePairsElement)
                    || sourcePairsElement.ValueKind != JsonValueKind.String
                    || string.IsNullOrWhiteSpace(sourcePairsElement.GetString()))
                {
                    return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(
                        "Source manifest does not contain a bound source_pairs_sha256.",
                        ProofHashMismatchCode);
                }

                var sourceState = OrchestrationReasoningTrainer.BuildLearnedState(
                    patternResult.Value!,
                    result.Value.Decision.ManifestSha256,
                    sourcePairsElement.GetString()!,
                    result.Value.ManifestPath);
                reconstructedSources.Add(CreateAdmitted(sourceState, [result.Value]));
            }
            catch (Exception ex) when (ex is JsonException or InvalidDataException or ArgumentException)
            {
                return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(
                    ex.Message,
                    ProofHashMismatchCode);
            }
        }

        var reconstructed = reconstructedSources[0];
        for (var index = 1; index < reconstructedSources.Count; index++)
        {
            var delta = reconstructedSources[index];
            var merge = Merge(
                reconstructed,
                delta,
                $"{reconstructed.State.StateId}:continued:{delta.State.StateId}",
                $"{reconstructed.State.GuideStateRef}:continued",
                $"{reconstructed.State.SourceBaseCommit}+delta:{delta.State.SourceBaseCommit}");
            if (!merge.IsSuccess)
                return merge;
            reconstructed = merge.Value!;
        }

        if (!OrchestrationReasoningCanonicalState.SerializeStateCore(reconstructed.State)
                .AsSpan()
                .SequenceEqual(OrchestrationReasoningCanonicalState.SerializeStateCore(package.State))
            || !OrchestrationReasoningCanonicalState.SerializePatterns(reconstructed.State.LearnedPatterns ?? [])
                .AsSpan()
                .SequenceEqual(OrchestrationReasoningCanonicalState.SerializePatterns(
                    package.State.LearnedPatterns ?? []))
            || !string.Equals(reconstructed.State.StateSha256, package.State.StateSha256, StringComparison.Ordinal)
            || !OrchestrationReasoningCanonicalState.SerializeReceiptCore(reconstructed.Receipt)
                .AsSpan()
                .SequenceEqual(OrchestrationReasoningCanonicalState.SerializeReceiptCore(package.Receipt))
            || !string.Equals(reconstructed.Receipt.ReceiptSha256, package.Receipt.ReceiptSha256, StringComparison.Ordinal))
        {
            return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(
                "Reverified source materials do not canonically reproduce the persisted state and receipt.",
                ProofHashMismatchCode);
        }

        return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Ok(reconstructed);
    }

    internal static IReadOnlyList<OrchestrationReasoningSourceMaterialReference> GetSourceReferences(
        OrchestrationReasoningAdmittedLearnedState admittedState)
        => admittedState.VerifiedSources
            .Select(source => new OrchestrationReasoningSourceMaterialReference(
                source.PayloadPath,
                source.ManifestPath,
                source.LedgerPath,
                source.EvidencePath))
            .ToArray();

    public static string GetProofPath(string checkpointPath)
    {
        var full = Path.GetFullPath(checkpointPath);
        var directory = Path.GetDirectoryName(full) ?? string.Empty;
        var stem = Path.GetFileNameWithoutExtension(full);
        return Path.Combine(directory, $"{stem}.source-admission-proof.json");
    }

    public static bool ContainsCommonCheckpointMarkers(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
            return false;
        var names = root.EnumerateObject()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return names.Contains("learnedState")
            || names.Contains("learned_state")
            || names.Contains("acceptedRefIds")
            || names.Contains("accepted_ref_ids")
            || names.Contains("sourceManifestRef")
            || names.Contains("source_manifest_ref");
    }

    private static OperationResult<OrchestrationReasoningVerifiedSourceMaterial> Reject(string message)
        => OperationResult<OrchestrationReasoningVerifiedSourceMaterial>.Fail(message, AdmissionRejectedCode);

    private static OperationResult<bool> Invalid(string message)
        => OperationResult<bool>.Fail(message, StateAdmissionInvalidCode);

    private static IReadOnlyList<JsonElement> ParsePayloadRows(byte[] bytes)
    {
        var text = Encoding.UTF8.GetString(bytes);
        var rows = new List<JsonElement>();
        foreach (var line in text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            using var document = JsonDocument.Parse(line);
            rows.Add(document.RootElement.Clone());
        }
        return rows;
    }

    private static bool ContainsHardDeniedMaterial(JsonElement element)
        => ContainsHardDeniedMaterial(element, containingPropertyName: null);

    private static bool ContainsHardDeniedMaterial(JsonElement element, string? containingPropertyName)
    {
        if (IsHostCheckSetsProperty(containingPropertyName))
            return HostCheckSetsContainUnsafeLabels(element);

        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject()
                .Any(property => ContainsHardDeniedMaterial(property.Value, property.Name)),
            JsonValueKind.Array => element.EnumerateArray()
                .Any(item => ContainsHardDeniedMaterial(item, containingPropertyName: null)),
            JsonValueKind.String => IsHardDeniedString(element.GetString() ?? string.Empty),
            _ => false
        };
    }

    private static bool PatternContainsHardDeniedMaterial(OrchestrationReasoningLearnedPattern pattern)
    {
        var identityBearingValues = new[]
            {
                pattern.PatternId,
                pattern.ScenarioRef,
            }
            .Concat(pattern.RequiredTaskTypes ?? [])
            .Concat(pattern.RequiredConstraintSignals ?? [])
            .Concat(pattern.RequiredRequirementSignals ?? [])
            .Concat(pattern.RequiredDeliverableSignals ?? [])
            .Concat(pattern.RequiredEvidenceSignals ?? [])
            .Concat(pattern.PhaseFamilies ?? [])
            .Concat(pattern.GateFamilies ?? [])
            .Concat(pattern.AssetFamilies ?? [])
            .Concat(pattern.RiskFlags ?? [])
            .Concat(pattern.NextGranularizationActions ?? []);

        // HostCheckSets names execution surfaces such as codex/cursor/claude.
        // Those words are not teacher/model authority here, so they need their
        // own narrow host-label gate instead of the paid-model identity scan.
        return identityBearingValues.Any(value => value is not null && IsHardDeniedString(value))
            || HostCheckSetsContainUnsafeLabels(pattern.HostCheckSets);
    }

    private static bool IsHardDeniedString(string value)
    {
        var normalized = value.Trim();
        return ForbiddenModelIdentityRegistry.ContainsForbiddenIdentity(normalized)
            || DeniedIds.Any(id => normalized.Contains(id, StringComparison.OrdinalIgnoreCase))
            || DeniedHashes.Contains(normalized)
            || normalized.StartsWith("or-learned-pattern:or-expanded-v1-pair-", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("or-expanded-v1-derived", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsHostCheckSetsProperty(string? propertyName)
        => string.Equals(propertyName, "hostCheckSets", StringComparison.Ordinal)
            || string.Equals(propertyName, "host_check_sets", StringComparison.Ordinal);

    private static bool HostCheckSetsContainUnsafeLabels(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
            return true;

        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String
                || IsUnsafeHostCheckSetLabel(item.GetString()))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HostCheckSetsContainUnsafeLabels(IEnumerable<string>? labels)
        => (labels ?? []).Any(IsUnsafeHostCheckSetLabel);

    private static bool IsUnsafeHostCheckSetLabel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        var label = value.Trim();
        if (!string.Equals(value, label, StringComparison.Ordinal)
            || label.Length > MaximumHostCheckSetLabelLength
            || label.Any(char.IsControl)
            || label.Contains("://", StringComparison.Ordinal)
            || label.Contains('/')
            || label.Contains('\\')
            || label.Contains(':'))
        {
            // Host labels are short local execution-surface names. Paths, URIs,
            // digests and padded values are source/model references in disguise.
            return true;
        }

        return !AllowedHostCheckSetLabels.Contains(label);
    }

    private static bool EqualsHash(JsonElement root, string propertyName, string expected)
        => root.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.String
            && string.Equals(value.GetString(), expected, StringComparison.Ordinal);

    private static bool MatchesInt(JsonElement root, string propertyName, int expected)
        => root.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out var actual)
            && actual == expected;

    private static JsonElement RequiredArray(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Array)
            throw new JsonException($"Required array '{name}' is missing.");
        return value;
    }

    private static string FirstRequiredString(JsonElement element, params string[] names)
        => FirstOptionalString(element, names)
            ?? throw new JsonException($"Required string '{string.Join("|", names)}' is missing.");

    private static string? FirstOptionalString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (element.TryGetProperty(name, out var value)
                && value.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(value.GetString()))
            {
                return value.GetString();
            }
        }
        return null;
    }

    private static bool MatchesAny(JsonElement element, string expected, params string[] names)
        => names.Any(name => element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
            && string.Equals(value.GetString(), expected, StringComparison.OrdinalIgnoreCase));

    private static bool IsAdaptiveSignal(JsonElement row)
        => FirstOptionalString(row, "targetTrainingAdapter", "target_training_adapter")
            is "IOrchestrationReasoningAdaptiveDpoTraining";

    internal static string HashCanonicalJson(JsonElement element)
        => OrchestrationReasoningCanonicalState.HashBytes(
            Encoding.UTF8.GetBytes(element.GetRawText()));

    private static bool SetEquals(JsonElement root, string name, IEnumerable<string> expected)
    {
        if (!root.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Array)
            return false;
        var actual = value.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .ToHashSet(StringComparer.Ordinal);
        return actual.SetEquals(expected);
    }

    private static IReadOnlyList<string> NormalizeSet(IEnumerable<string> values)
        => values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(OrchestrationReasoningCanonicalState.NormalizeString)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static string CombineHashes(IEnumerable<string> hashes)
    {
        var normalized = NormalizeSet(hashes);
        return normalized.Count == 1
            ? normalized[0]
            : OrchestrationReasoningCanonicalState.HashBytes(
                Encoding.UTF8.GetBytes(string.Join("\n", normalized)));
    }

    private static string CombineKinds(IEnumerable<string> kinds)
        => string.Join("+", NormalizeSet(kinds));
}
