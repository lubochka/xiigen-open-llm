// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

/// <summary>
/// Supported repository-owned code-documentation DPO address profile.
/// </summary>
public sealed record CodeDocumentationRepositoryOwnedDpoDomainProfile(
    string DomainSlug,
    string DesignPatternSlug,
    string DisplayName,
    string TeacherPromptDomain,
    string RequiredTeacherFocus);

/// <summary>
/// Material-purpose classification owned by the CodeDocumentation authoring
/// boundary. Raw routing decisions do not carry this security-relevant fact.
/// </summary>
public enum CodeDocumentationRepositoryOwnedDpoMaterialPurpose
{
    ApprovedTeachingReference,
    CertificationExam,
    DiagnosticFeedback,
    Unknown
}

/// <summary>
/// Validated source envelope required before repository-owned DPO authoring.
/// It carries only source identity and routing rows; closed payloads are never
/// copied into this adapter.
/// </summary>
public sealed record CodeDocumentationRepositoryOwnedDpoAuthoringSource
{
    private readonly IReadOnlyList<RoutingDecisionRecord> _referenceRows;

    private CodeDocumentationRepositoryOwnedDpoAuthoringSource(
        IReadOnlyList<RoutingDecisionRecord> referenceRows,
        CodeDocumentationRepositoryOwnedDpoMaterialPurpose materialPurpose,
        string sourceIdentity,
        bool closedExamExposed,
        string referenceRowsIdentitySha256)
    {
        _referenceRows = referenceRows;
        MaterialPurpose = materialPurpose;
        SourceIdentity = sourceIdentity;
        ClosedExamExposed = closedExamExposed;
        ReferenceRowsIdentitySha256 = referenceRowsIdentitySha256;
    }

    // The approval owns its nested candidate arrays. Public observations must
    // never hand those arrays back to callers, even before resolver entry.
    public IReadOnlyList<RoutingDecisionRecord> ReferenceRows
        => Array.AsReadOnly(CopyReferenceRows(_referenceRows));
    public CodeDocumentationRepositoryOwnedDpoMaterialPurpose MaterialPurpose { get; }
    public string SourceIdentity { get; }
    public bool ClosedExamExposed { get; }
    public string ReferenceRowsIdentitySha256 { get; }

    internal static OperationResult<
        CodeDocumentationRepositoryOwnedDpoAuthoringSource> Create(
        IReadOnlyList<RoutingDecisionRecord>? referenceRows,
        CodeDocumentationRepositoryOwnedDpoMaterialPurpose materialPurpose,
        string sourceIdentity,
        bool closedExamExposed)
    {
        if (referenceRows is null || referenceRows.Count == 0 ||
            referenceRows.Any(static row =>
                !CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .HasReferenceRowShape(row)) ||
            !Enum.IsDefined(materialPurpose) ||
            string.IsNullOrWhiteSpace(sourceIdentity) ||
            sourceIdentity.Length > 256 ||
            !StringComparer.Ordinal.Equals(sourceIdentity, sourceIdentity.Trim()) ||
            sourceIdentity.Any(char.IsControl))
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoAuthoringSource>.Fail(
                "CodeDocumentation DPO authoring source identity and rows are invalid.",
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .AuthoringSourceInvalidErrorCode);
        }

        // Capture all material once at approval, then derive the unchanged V1
        // identity from that same private snapshot rather than caller aliases.
        var snapshotRows = CopyReferenceRows(referenceRows);
        return OperationResult<
            CodeDocumentationRepositoryOwnedDpoAuthoringSource>.Ok(
            new(
                Array.AsReadOnly(snapshotRows),
                materialPurpose,
                sourceIdentity,
                closedExamExposed,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .ComputeReferenceRowsIdentitySha256(snapshotRows)));
    }

    private static RoutingDecisionRecord[] CopyReferenceRows(
        IReadOnlyList<RoutingDecisionRecord> referenceRows)
        => referenceRows.Select(static row => row with
        {
            RejectedCandidates = row.RejectedCandidates
                .Select(static candidate => candidate with { })
                .ToArray(),
            ScoreGap = row.ScoreGap is null ? null : row.ScoreGap with { },
            ThresholdsRef = row.ThresholdsRef is null
                ? null
                : row.ThresholdsRef with { }
        }).ToArray();

    internal OperationResult<bool> ValidateForAuthoring()
    {
        if (MaterialPurpose !=
                CodeDocumentationRepositoryOwnedDpoMaterialPurpose
                    .ApprovedTeachingReference ||
            ClosedExamExposed ||
            _referenceRows is null ||
            _referenceRows.Count == 0 ||
            string.IsNullOrWhiteSpace(SourceIdentity) ||
            SourceIdentity.Length > 256 ||
            !StringComparer.Ordinal.Equals(SourceIdentity, SourceIdentity.Trim()) ||
            SourceIdentity.Any(char.IsControl))
        {
            return OperationResult<bool>.Fail(
                "Certification or diagnostic material cannot enter CodeDocumentation DPO authoring.",
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .AuthoringSourceNotTrainableErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    internal bool MatchesReferenceRows(
        IReadOnlyList<RoutingDecisionRecord>? referenceRows)
        => referenceRows is not null &&
           StringComparer.Ordinal.Equals(
               ReferenceRowsIdentitySha256,
               CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                   .ComputeReferenceRowsIdentitySha256(referenceRows));
}

/// <summary>
/// Настройки живой ветки создания DPO для code-documentation domains.
/// Учитель нужен только при честном промахе по диску: если manifest уже есть,
/// runner обязан открыть сохранённые пары и не просить Qwen писать их заново.
/// </summary>
public sealed record CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext(
    Func<OperationResult<ITrainingTeacherInferenceClient>> CreateTeacherClient,
    RepositoryOwnedDpoOpenTeacherIdentity OpenTeacher,
    RepositoryOwnedDpoGenerationPolicyIdentity GenerationPolicy,
    RepositoryOwnedDpoCreatorIdentity Creator,
    int MaxPairsToAuthor = 50,
    CodeDocumentationRepositoryOwnedDpoDomainProfile? DomainProfile = null)
{
    public TrainingTeacherInferenceProfileEvidence InferenceProfile { get; init; } = LocalQwenOllamaInferenceProfileAuthority.Authoring; // Existing contexts retain the legacy passport; production sets the exact process-selected authoring evidence.

    public OperationResult<bool> Validate()
    {
        var profile = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveContextDomainProfile(DomainProfile);
        if (!profile.IsSuccess)
            return profile.MapError<bool>();

        if (!(LocalQwenOllamaInferenceProfileAuthority.IsCanonicalAuthoring(InferenceProfile) || LocalQwenOllamaInferenceProfileAuthority.IsCanonicalCpuAuthoringProfile(InferenceProfile)) ||
            LocalQwenOllamaInferenceProfileAuthority.IsCanonicalCpuAuthoringProfile(InferenceProfile) && !CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.HasCanonicalCpuPolicy(this, profile.Value!))
            return OperationResult<bool>.Fail("The selected authoring profile does not match the sealed policy.", CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.TeacherProvenanceMismatchErrorCode); // Reject forged/mismatched selection before opening a teacher factory.

        if (CreateTeacherClient is null ||
            MaxPairsToAuthor is <= 0 or > 1_000)
        {
            return OperationResult<bool>.Fail(
                $"{profile.Value!.DisplayName} DPO authoring needs a local open-weight teacher factory and a bounded positive pair count.",
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.InvalidErrorCode);
        }

        // WHY: validate the immutable teacher passport without constructing
        // the client. A repository hit must stay factory-free and call-free.
        var identity = RepositoryOwnedDpoDatasetManifestSealer
            .CreateRequestIdentity(
                [new string('0', 64)],
                OpenTeacher,
                GenerationPolicy,
                Creator);
        if (!identity.IsSuccess)
        {
            return OperationResult<bool>.Fail(
                $"{profile.Value!.DisplayName} DPO authoring identities are invalid.",
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.InvalidErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }
}

/// <summary>
/// Reference routing decisions and the exact request identity to seal beside
/// the teacher-authored DPO rows. The paid/reference answer participates only
/// by hash; its text is never allowed to become chosen or rejected.
/// </summary>
public sealed record CodeDocumentationRepositoryOwnedDpoDatasetAuthoringRequest(
    IReadOnlyList<RoutingDecisionRecord> PaidReferenceRows,
    RepositoryOwnedDpoDatasetRequestIdentity RequestIdentity,
    CodeDocumentationRepositoryOwnedDpoAuthoringSource AuthoringSource);

/// <summary>
/// Creates repository-owned DPO rows for code-documentation domains using
/// the admitted local Qwen teacher. The class deliberately stores rich
/// ContinuousLearning.DpoPair rows, not the older lightweight RoutingDpoPair.
/// </summary>
public static class CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
{
    internal const string SelectedMaterialAdmissionInvalidErrorCode = "ONE_LESSON_SELECTED_MATERIAL_ADMISSION_INVALID"; // Opaque material mismatch never authorizes disclosure or training.
    private static readonly UTF8Encoding SelectedMaterialUtf8 = new(false, true); // Preserve full strict UTF8 payload and commitment bytes.
    private static readonly AsyncLocal<Action<PartialSelectedMaterialContext>?> SelectedMaterialObserver = new(); // Isolate the test observer to the genuine caller's execution context.
    internal static Action<PartialSelectedMaterialContext>? SelectedMaterialCapturedForTesting // Observe the actual payload recipient, without issuing admission authority.
    { // Callers restore their scoped observer in finally.
        get => SelectedMaterialObserver.Value; // Read only this execution context's observer.
        set => SelectedMaterialObserver.Value = value; // Test composition may observe or inject failure at the actual recipient boundary.
    } // No observer is registered by normal Program composition.

    internal enum SelectedTeachingMaterialOrigin { Unknown = 0, SyntheticTest = 1 } // Default lineage never establishes recipient disclosure rights.
    internal sealed class SelectedTeachingMaterialAdmission // This private synthetic eligibility object is not independent real-rights certification.
    { // No public constructor, wire import, Native factory or scalar-to-rights conversion exists.
        internal SelectedTeachingMaterialOrigin Origin { get; } // Factory-owned lineage is available before any recipient opens raw material.
        internal string BindingSha256 { get; } // Bind eligibility to the complete receiving Session identity.
        internal string SourceSha256 { get; } // Bind every used and unused SOURCE cell.
        internal string FrozenContextSha256 { get; } // Bind the complete canonical operator input context.
        internal string EvidenceSha256 { get; } // Derive this digest inside the owner, never accept it from a caller.
        internal CodeDocumentationOneLessonTeachingItemV2 Item { get; } // Bind the exact declared selected join without permitting ordinal fallback.
        internal CodeDocumentationOneLessonSourceSelection.DeclaredTeachingEntryIdentity Entry { get; } // Bind actual task/reference/provenance commitments.
        internal CodeDocumentationRepositoryOwnedDpoMaterialPurpose Purpose { get; } // Preserve the existing authoring purpose owner.
        internal TrainingDataGovernanceDecision Governance { get; } // Immutable capture-time policy remains synthetic eligibility only.
        private readonly byte[] inputBytes; // Freeze the complete canonical operator input independently of caller aliases.
        private SelectedTeachingMaterialAdmission(string binding, CodeDocumentationTeachingSelectionInputV1 input, // Only the synthetic factory can construct this owner-bound object.
            CodeDocumentationOneLessonSourceSelection.DeclaredTeachingItemReadback item, CodeDocumentationRepositoryOwnedDpoMaterialPurpose purpose, TrainingDataGovernanceDecision governance) // Capture the actual graph join and validated existing decision.
        { // Own immutable fields before deriving their fixed-order commitment.
            Origin = SelectedTeachingMaterialOrigin.SyntheticTest; // The sole private synthetic constructor cannot issue ordinary disclosure rights.
            BindingSha256 = binding; SourceSha256 = input.SourceSha256; FrozenContextSha256 = input.FrozenContextSha256; // Retain the whole receiving and SOURCE/context identities.
            Item = item.DeclaredItem with { }; Entry = item.Entry with { }; Purpose = purpose; Governance = governance; // Copy immutable selected metadata and policy values.
            inputBytes = input.CopyBytes(); EvidenceSha256 = ComputeEvidence(); // Derive evidence from the same privately captured complete preimage.
        } // Synthetic lineage cannot be upgraded by matching its digest.
        internal static OperationResult<SelectedTeachingMaterialAdmission> CaptureSyntheticForTesting( // The only factory validates genuine owner graph and existing capture-time policy.
            CodeDocumentationOneLessonSourceSelection.DeclaredTeachingCorpusReadback graph, CodeDocumentationTeachingSelectionInputV1 input, // Recapture the entire typed input from the genuine graph.
            string bindingSha256, string itemId, CodeDocumentationRepositoryOwnedDpoMaterialPurpose purpose, TrainingDataGovernanceDecision? governance) // Caller scalars convey no real-rights certification.
        { // Refuse forbidden purposes before retaining any material eligibility.
            if (purpose != CodeDocumentationRepositoryOwnedDpoMaterialPurpose.ApprovedTeachingReference) // Certification, diagnostic and unknown purposes stay closed.
                return OperationResult<SelectedTeachingMaterialAdmission>.Fail("Certification or diagnostic material cannot enter CodeDocumentation DPO authoring.", AuthoringSourceNotTrainableErrorCode); // Reuse the existing authoring-purpose refusal.
            var policy = TrainingDataGovernanceAdmission.ValidateDecision(governance); // Reuse the exact license/quarantine/evidence policy without a fake pair.
            if (!policy.IsSuccess) return policy.MapError<SelectedTeachingMaterialAdmission>(); // Preserve the existing ordered governance error.
            if (graph is null || input is null || !CodeDocumentationOneLessonV2Wire.Hash(bindingSha256)) return InvalidSelectedMaterial<SelectedTeachingMaterialAdmission>(); // Reject malformed receiving inputs before construction.
            var recaptured = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(input.OriginalReceiptId, input.OriginalReceiptSha256, input.Categories, graph); // Rebuild every canonical input field from the real graph.
            if (!recaptured.IsSuccess || recaptured.Value is null || recaptured.Value.SourceSha256 != input.SourceSha256 || // Independently supplied typed scalars must match the genuine recapture as well as its bytes.
                recaptured.Value.FrozenContextSha256 != input.FrozenContextSha256 || !recaptured.Value.CopyBytes().AsSpan().SequenceEqual(input.CopyBytes())) return InvalidSelectedMaterial<SelectedTeachingMaterialAdmission>(); // A valid context hash alone cannot replace the full canonical input.
            var matches = graph.Items.Where(i => i.DeclaredItem.ItemId == itemId).ToArray(); // Select the explicitly supplied exact declared identity.
            if (matches.Length != 1 || !graph.CategoryBindings.Any(c => input.Categories.Contains(c.Category, StringComparer.Ordinal) && c.OrderedItemIds.Contains(itemId, StringComparer.Ordinal))) // Require one genuine selected join in the actual diagnostic category bindings.
                return InvalidSelectedMaterial<SelectedTeachingMaterialAdmission>(); // No arbitrary first category or item is substituted.
            return OperationResult<SelectedTeachingMaterialAdmission>.Ok(new(bindingSha256, recaptured.Value, matches[0], purpose, governance!)); // Issue synthetic-only eligibility after actual graph and policy validation.
        } // This creates no disk state, effect permit or production provider.
        internal OperationResult<bool> MatchesSelectionInput(string binding, CodeDocumentationTeachingSelectionInputV1 input, CodeDocumentationOneLessonTeachingItemV2 item) // Validate this genuine Synthetic admission before any new pre-selection payload extraction.
        { // Existing capture-time purpose/policy and exact full input remain authoritative only for Synthetic lineage.
            var policy = TrainingDataGovernanceAdmission.ValidateDecision(Governance); // Reuse the existing license/quarantine/evidence rules without a fabricated pair or permit.
            if (!policy.IsSuccess) return policy; // Preserve actual ordered policy refusal.
            if (Purpose != CodeDocumentationRepositoryOwnedDpoMaterialPurpose.ApprovedTeachingReference || binding != BindingSha256 || // No diagnostic/certification/foreign receiving binding enters selection context.
                input.SourceSha256 != SourceSha256 || input.FrozenContextSha256 != FrozenContextSha256 || // Match the entire actual SOURCE and metadata preimage identity.
                !input.CopyBytes().AsSpan().SequenceEqual(inputBytes) || item != Item) return InvalidSelectedMaterial<bool>(); // Digest equality never replaces exact privately retained bytes and item join.
            return OperationResult<bool>.Ok(true); // This result grants no genuine rights, Native origin or saved decision.
        } // Existing saved-choice Matches below retains its stronger payload/evidence checks.
        internal OperationResult<bool> Matches(string binding, CodeDocumentationTeachingSelectionInputV1 input, CodeDocumentationTeachingSelectionPayloadV1 payload) // Revalidate every saved choice and receiving binding before payload publication.
        { // Immutable capture still requires the exact current frozen input and saved choice.
            var policy = TrainingDataGovernanceAdmission.ValidateDecision(Governance); // Retain the same existing policy on consumption.
            if (!policy.IsSuccess) return policy; // Preserve genuine policy refusals.
            if (Purpose != CodeDocumentationRepositoryOwnedDpoMaterialPurpose.ApprovedTeachingReference || binding != BindingSha256 || // No foreign receiving Session or purpose enters the recipient.
                input.SourceSha256 != SourceSha256 || input.FrozenContextSha256 != FrozenContextSha256 || !input.CopyBytes().AsSpan().SequenceEqual(inputBytes) || // Reconfirm the entire actual input, rather than its digest alone.
                payload.SourceSha256 != SourceSha256 || payload.FrozenContextSha256 != FrozenContextSha256 || payload.MaterialAdmissionEvidenceSha256 != EvidenceSha256 || // Require the owner-derived evidence saved with the choice.
                payload.OriginalReceiptId != input.OriginalReceiptId || payload.OriginalReceiptSha256 != input.OriginalReceiptSha256 || !payload.Categories.SequenceEqual(input.Categories, StringComparer.Ordinal) || // Preserve exact ORIGINAL and diagnostic order bindings.
                payload.ItemId != Item.ItemId || payload.SourceId != Item.SourceId || payload.ReferenceEntryId != Item.ReferenceEntryId || payload.SourceEntityFingerprint != Item.SourceEntityFingerprint) // Compare all endpoints of the actual selected join.
                return InvalidSelectedMaterial<bool>(); // Descriptive hashes, stale/foreign choices and substituted items fail before raw publication.
            return OperationResult<bool>.Ok(true); // Eligibility remains explicitly synthetic and purpose-bound.
        } // No model, author, trainer or permit is resolved here.
        private string ComputeEvidence() // Commit full immutable input and ordered existing policy in one private in-memory preimage.
        { // BinaryWriter writes each Int32 in little-endian; UTF8 fields use explicit byte lengths.
            using var buffer = new MemoryStream(); using var writer = new BinaryWriter(buffer, SelectedMaterialUtf8, leaveOpen: true); // Own the preimage without creating a file or wire schema.
            writer.Write(SelectedMaterialUtf8.GetBytes("xiigen-s04-selected-material-admission-v1\n")); // Use the exact actual-newline domain separator.
            void Bytes(byte[] bytes) { writer.Write(bytes.Length); writer.Write(bytes); } // Prefix each complete field with its Int32 little-endian byte length.
            void Field(string value) => Bytes(SelectedMaterialUtf8.GetBytes(value)); // Reject invalid UTF16 and preserve every UTF8 byte.
            Field(BindingSha256); Bytes(inputBytes); Field(Item.ItemId); Field(Item.SourceId); Field(Item.ReferenceEntryId); Field(Item.SourceEntityFingerprint); // Fix receiving/input/selected-join field order.
            Field(Entry.OwnerInputSha256); Field(Entry.PaidReferenceProvenance.ReferenceAnswerSha256); Field(Entry.PaidReferenceProvenance.ProvenanceSha256); // Bind actual task, reference and immutable provenance commitments.
            Field(Purpose.ToString()); Field(Governance.LicenseAccepted ? "true" : "false"); Field(Governance.IsQuarantined ? "true" : "false"); // Preserve existing purpose and governance flags as length-prefixed UTF8.
            writer.Write(Governance.EvidenceRefs.Count); foreach (var evidence in Governance.EvidenceRefs) Field(evidence); // Preserve the exact count and supplied evidence order.
            writer.Flush(); return Convert.ToHexString(SHA256.HashData(buffer.ToArray())).ToLowerInvariant(); // Derive the lowercase SHA256 from this same complete private preimage.
        } // Digest equality alone cannot construct or import this object.
    } // End the closed synthetic admission owner.

    internal const string SelectionMaterialPolicy = "Judge task-specific pedagogical suitability first. Use the supplied curriculum order only to break otherwise equivalent eligible choices."; // A fixed procedural instruction is not a category ranking or a semantic decision.
    private static readonly AsyncLocal<Action<PreparedSelectionMaterialContext>?> SelectionMaterialRecipientObserver = new(); // Fault controls observe the real complete recipient only in their own execution context.
    internal static Action<PreparedSelectionMaterialContext>? SelectionMaterialCapturedForTesting // Production composition does not register this observer.
    { // Tests restore the previous scoped observer in finally.
        get => SelectionMaterialRecipientObserver.Value; // Read only the current synthetic observation seam.
        set => SelectionMaterialRecipientObserver.Value = value; // No observer can issue rights or Session authority.
    } // Retain no global callback across concurrent operations.
    internal sealed record SelectionMaterialCandidate(string ItemId, string SourceId, string ReferenceEntryId, // Immutable exact candidate join, independent of any selected ordinal.
        string SourceEntityFingerprint, string OwnerInput, string PaidModelReferenceAnswer, // Keep full owner task and context-only reference strings.
        PaidReferenceProvenance PaidReferenceProvenance, string MaterialAdmissionEvidenceSha256); // Preserve actual reference provenance and Synthetic opaque admission commitment.

    internal sealed class PreparedSelectionMaterialContext // Full recipient data is separate from the Session-issued receipt and cannot authorize a decision.
    { // Only this actual recipient constructs the privately copied canonical packet.
        internal string Origin { get; } // Only the private recipient constructor fixes lineage.
        internal bool FullTeacherContextReady => false; // This pre-selection context contains no matching Student/history/teacher admission.
        internal string BindingSha256 { get; } // Identify the complete receiving Session binding.
        internal CodeDocumentationTeachingSelectionInputV1 MetadataInput { get; } // Retain the complete canonical descriptive graph and sanitized diagnosis.
        internal IReadOnlyList<SelectionMaterialCandidate> Candidates { get; } // Preserve every eligible raw cell in declared item order.
        internal string PacketSha256 { get; } // Derive the commitment from full privately copied bytes, never a caller digest.
        private readonly byte[] bindingBytes, packetBytes; // Caller byte mutations cannot alter this context or its commitment.
        internal byte[] CopyBindingBytes() => bindingBytes.ToArray(); // Return a defensive complete canonical binding snapshot.
        internal byte[] CopyCanonicalBytes() => packetBytes.ToArray(); // Return full length-framed input, with no truncation or model rendering claim.
        private PreparedSelectionMaterialContext(string binding, byte[] canonicalBinding, CodeDocumentationTeachingSelectionInputV1 input, // Construction follows genuine batch reconstruction and opaque admission matching.
            IReadOnlyList<SelectionMaterialCandidate> candidates, string origin = "SyntheticTest") // The outer candidate array is detached from all callers.
        { // Own all complete input fields before deriving the byte commitment.
            Origin = origin; BindingSha256 = binding; bindingBytes = canonicalBinding.ToArray(); MetadataInput = input; // MetadataInput itself owns immutable canonical bytes and copied categories.
            Candidates = Array.AsReadOnly(candidates.Select(c => c with { }).ToArray()); // Copy every immutable raw candidate record in its supplied order.
            packetBytes = CanonicalPacket(); PacketSha256 = Convert.ToHexString(SHA256.HashData(packetBytes)).ToLowerInvariant(); // Commit complete canonical bytes including all task/reference content.
        } // This context retains no corpus handles, clients, permits or owner authority.
        private byte[] CanonicalPacket() // Fixed length-framed preimage reuses the existing admission owner's BinaryWriter convention.
        { // Int32 lengths/counts are little-endian and strings are strict complete UTF8.
            using var buffer = new MemoryStream(); using var writer = new BinaryWriter(buffer, SelectedMaterialUtf8, leaveOpen: true); // Build only an in-memory packet, never a new journal schema.
            writer.Write(SelectedMaterialUtf8.GetBytes("code-documentation-selection-material-input-v1\n")); // Fix the exact newline domain separator independently of model prompt rendering.
            void Bytes(byte[] bytes) { writer.Write(bytes.Length); writer.Write(bytes); } // Prefix every complete byte field without abbreviation.
            void Field(string value) => Bytes(SelectedMaterialUtf8.GetBytes(value)); // Preserve strict UTF8 including Unicode/newlines/full suffixes.
            Field(SelectionMaterialPolicy); Field(BindingSha256); Bytes(bindingBytes); Bytes(MetadataInput.CopyBytes()); // Bind procedure, receiving owner and every descriptive graph cell.
            writer.Write(MetadataInput.Categories.Count); foreach (var category in MetadataInput.Categories) Field(category); // Keep actual sanitized diagnostic order, separate from curriculum order.
            writer.Write(Candidates.Count); // Charge every unique eligible candidate to the complete input preimage.
            foreach (var candidate in Candidates) // Declared order is retained without alphabetical or category priority sorting.
            { // Raw content is context-only and tied to the exact actual owner join.
                Field(candidate.ItemId); Field(candidate.SourceId); Field(candidate.ReferenceEntryId); Field(candidate.SourceEntityFingerprint); // Preserve all link endpoints rather than an ordinal.
                Field(candidate.OwnerInput); Field(candidate.PaidModelReferenceAnswer); // Full content participates directly, not only through hashes.
                Field(candidate.PaidReferenceProvenance.CanonicalRepresentation); Field(candidate.MaterialAdmissionEvidenceSha256); // Preserve immutable reference restrictions and exact Synthetic admission evidence.
            } // Complete all candidate bytes before returning the preimage.
            writer.Flush(); return buffer.ToArray(); // Return the exact finite packet without a file or effect.
        } // PacketSha256 is evidence of this complete immutable context only.
        internal static OperationResult<PreparedSelectionMaterialContext> CaptureProtected( // Construct the protected full material context only from actual owner readback and receipt.
            CodeDocumentationOneLessonSession owner, CodeDocumentationOneLessonSession.PreparedTeachingDisclosureReceipt approval, // Require the concrete held Session and its opaque genuine approval receipt.
            CodeDocumentationOneLessonSourceSelection.TeachingSelectionMaterialReadback material, // Accept the complete material readback from the existing source owner, not caller candidates.
            string binding, byte[] canonicalBinding, CodeDocumentationTeachingSelectionInputV1 input) // Bind the actual receiving bytes and complete canonical graph input to that readback.
        { // Reconstruct every eligible raw candidate while retaining all full-input ownership checks.
            var matched = approval.MatchCompleteInput(owner, material.Graph, input, canonicalBinding); // Match current receipt, complete graph and binding/input bytes before copying raw fields.
            if (!matched.IsSuccess) return matched.MapError<PreparedSelectionMaterialContext>(); // Preserve exact approval/owner refusal without creating a context.
            var recaptured = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(input.OriginalReceiptId, input.OriginalReceiptSha256, input.Categories, material.Graph); // Reconstruct canonical input from the actual graph and retained ordered diagnosis.
            if (!recaptured.IsSuccess || recaptured.Value is null || !recaptured.Value.CopyBytes().AsSpan().SequenceEqual(input.CopyBytes())) return InvalidSelectedMaterial<PreparedSelectionMaterialContext>(); // Reject any complete input-byte drift before selecting eligible readback cells.
            var eligible = material.Graph.CategoryBindings.Where(c => input.Categories.Contains(c.Category, StringComparer.Ordinal)).SelectMany(c => c.OrderedItemIds).ToHashSet(StringComparer.Ordinal); // Compute eligible membership from genuine category bindings without choosing a task.
            var items = material.Graph.Items.Where(i => eligible.Contains(i.DeclaredItem.ItemId)).ToArray(); // Preserve the actual graph's eligible item order for complete candidate construction.
            if (material.Materials.Count != items.Length || items.Length == 0) return InvalidSelectedMaterial<PreparedSelectionMaterialContext>(); // Require exactly one raw cell per eligible item and at least one eligible candidate.
            var cells = new List<SelectionMaterialCandidate>(); // Collect complete immutable task/reference/provenance records, not digests of their content.
            for (var i = 0; i < items.Length; i++) // Visit each eligible item in the same order as the genuine material batch.
            { // Verify this raw cell's actual owner bindings before recording its full content.
                var cell = material.Materials[i]; var item = items[i]; var entry = item.Entry; // Read the matching actual material cell, item and sealed entry at this position.
                if (cell.Item.DeclaredItem != item.DeclaredItem || cell.Item.Entry != entry || // Require the cell's complete declared item and sealed entry to match the genuine graph.
                    FrozenReferenceEntry.ComputeNormalizedTextSha256(cell.OwnerInput) != entry.OwnerInputSha256 || // Bind the full normalized task text to the sealed owner-input digest.
                    !TryGetSelectedReferenceHash(cell.PaidModelReferenceAnswer, entry.PaidReferenceProvenance, out _) || cell.PaidReferenceProvenance != entry.PaidReferenceProvenance) return InvalidSelectedMaterial<PreparedSelectionMaterialContext>(); // Reuse the known factory-owned legacy or exact UTF8 content guard and require full sealed provenance equality.
                using var preimage = new MemoryStream(); using var writer = new BinaryWriter(preimage, SelectedMaterialUtf8, leaveOpen: true); // Frame per-item evidence in memory without writing a file or creating an effect permit.
                writer.Write(SelectedMaterialUtf8.GetBytes("code-documentation-startup-teaching-disclosure-v1\n")); // Use a distinct protected disclosure domain so Synthetic evidence cannot alias it.
                void Bytes(byte[] bytes) { writer.Write(bytes.Length); writer.Write(bytes); } // Length-frame each complete byte field to prevent ambiguous concatenated evidence.
                Bytes(approval.CopyApprovalBytes()); Bytes(canonicalBinding); Bytes(input.CopyBytes()); // Commit complete approval, receiving binding and canonical full-input preimages.
                Bytes(SelectedMaterialUtf8.GetBytes(item.DeclaredItem.ItemId)); Bytes(SelectedMaterialUtf8.GetBytes(item.DeclaredItem.SourceId)); // Commit the genuine item/source identities for this particular eligible candidate.
                Bytes(SelectedMaterialUtf8.GetBytes(entry.EntryId)); Bytes(SelectedMaterialUtf8.GetBytes(entry.SourceEntityFingerprint)); // Commit actual entry identity and fingerprint to the same framed evidence.
                Bytes(SelectedMaterialUtf8.GetBytes(entry.OwnerInputSha256)); Bytes(SelectedMaterialUtf8.GetBytes(entry.PaidReferenceProvenance.CanonicalRepresentation)); writer.Flush(); // Commit sealed owner-input digest and full canonical paid provenance, then finish the frame.
                var evidence = Convert.ToHexString(SHA256.HashData(preimage.ToArray())).ToLowerInvariant(); // Derive lowercase evidence from that complete per-item protected preimage.
                var d = item.DeclaredItem; cells.Add(new(d.ItemId, d.SourceId, d.ReferenceEntryId, d.SourceEntityFingerprint, cell.OwnerInput, cell.PaidModelReferenceAnswer, cell.PaidReferenceProvenance, evidence)); // Retain full actual task/reference/provenance alongside its genuine identities and evidence.
            } // Finish this complete cell before constructing the next eligible candidate.
            var final = approval.MatchCompleteInput(owner, material.Graph, input, canonicalBinding); // Recheck active approval/owner and full graph/input after all raw cells were reconstructed.
            if (!final.IsSuccess) return final.MapError<PreparedSelectionMaterialContext>(); // Withhold the context if ownership or complete input changed during reconstruction.
            var context = new PreparedSelectionMaterialContext(binding, canonicalBinding, recaptured.Value, cells, "ProtectedStartupOperatorApproval"); // Set protected in-memory lineage only through this private genuine-readback constructor path.
            SelectionMaterialCapturedForTesting?.Invoke(context); // Observe the actual complete context for bounded capture/fault tests.
            return OperationResult<PreparedSelectionMaterialContext>.Ok(context); // Return the full immutable context without selection, teacher readiness or training authority.
        } // Finish capture without a caller-import or arbitrary recipient factory.

        internal static OperationResult<PreparedSelectionMaterialContext> Capture( // The sole constructor path consumes genuine raw owner readback and every opaque admission.
            CodeDocumentationOneLessonSourceSelection.TeachingSelectionMaterialReadback material, string binding, byte[] canonicalBinding, // Receiving Session supplies the actual canonical binding, not a public DTO import.
            CodeDocumentationTeachingSelectionInputV1 input, IReadOnlyList<SelectedTeachingMaterialAdmission> admissions) // Already checked admissions are independently revalidated at the actual recipient.
        { // Reject incomplete or changed context before publication.
            var recaptured = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(input.OriginalReceiptId, input.OriginalReceiptSha256, input.Categories, material.Graph); // Reconstruct all metadata bytes from the actual complete batch graph.
            if (!recaptured.IsSuccess || recaptured.Value is null) return recaptured.MapError<PreparedSelectionMaterialContext>(); // Preserve genuine canonical graph refusal.
            if (!recaptured.Value.CopyBytes().AsSpan().SequenceEqual(input.CopyBytes()) || canonicalBinding.Length == 0 || admissions.Count != material.Materials.Count) return InvalidSelectedMaterial<PreparedSelectionMaterialContext>(); // Never accept a partial batch or metadata hash-only substitute.
            var cells = new List<SelectionMaterialCandidate>(); // Stage every independently matched full recipient cell.
            foreach (var cell in material.Materials) // Raw order follows the actual complete eligible batch.
            { // Match one genuine opaque admission to each exact item, not caller ordinal.
                var matches = admissions.Where(a => a.Item == cell.Item.DeclaredItem).ToArray(); // Duplicate/foreign items cannot disappear by dictionary overwrite.
                if (matches.Length != 1) return InvalidSelectedMaterial<PreparedSelectionMaterialContext>(); // Missing or ambiguous admission withholds the whole packet.
                var admission = matches[0]; var checkedAdmission = admission.MatchesSelectionInput(binding, input, cell.Item.DeclaredItem); // Recheck complete privately held policy/context/item semantics at the recipient.
                if (!checkedAdmission.IsSuccess) return checkedAdmission.MapError<PreparedSelectionMaterialContext>(); // Preserve real purpose/license/quarantine refusal.
                if (cell.Item.Entry != admission.Entry || FrozenReferenceEntry.ComputeNormalizedTextSha256(cell.OwnerInput) != admission.Entry.OwnerInputSha256 || // Bind full task bytes to the genuine descriptive row retained at admission.
                    FrozenReferenceEntry.ComputeNormalizedTextSha256(cell.PaidModelReferenceAnswer) != admission.Entry.PaidReferenceProvenance.ReferenceAnswerSha256 || // Reference reconstruction must retain full owner content identity.
                    cell.PaidReferenceProvenance != admission.Entry.PaidReferenceProvenance) return InvalidSelectedMaterial<PreparedSelectionMaterialContext>(); // Provenance restrictions cannot be substituted by a matching label.
                var item = cell.Item.DeclaredItem; cells.Add(new(item.ItemId, item.SourceId, item.ReferenceEntryId, item.SourceEntityFingerprint, cell.OwnerInput, cell.PaidModelReferenceAnswer, cell.PaidReferenceProvenance, admission.EvidenceSha256)); // Preserve complete immutable recipient data without pair projection.
            } // Finish all eligible raw cells before the private constructor.
            var context = new PreparedSelectionMaterialContext(binding, canonicalBinding, recaptured.Value, cells); // Actual complete recipient, not an unused caller-created envelope.
            SelectionMaterialCapturedForTesting?.Invoke(context); // Tests inject faults at the real recipient before Session final checks.
            return OperationResult<PreparedSelectionMaterialContext>.Ok(context); // Session still owns issuance and final receiving/SOURCE/ORIGINAL acceptance.
        } // No teacher, exposure, dataset authoring, selection append or optimizer call occurs.
    } // End the full pre-selection context-only recipient.
    internal static OperationResult<PreparedSelectionMaterialContext> CaptureSelectionMaterialContext( // Keep the actual recipient inside the existing DatasetAuthoring owner.
        CodeDocumentationOneLessonSourceSelection.TeachingSelectionMaterialReadback material, string binding, byte[] canonicalBinding, // Genuine batch and held binding are required, never raw caller strings alone.
        CodeDocumentationTeachingSelectionInputV1 input, IReadOnlyList<SelectedTeachingMaterialAdmission> admissions) // Every eligible admission remains separately opaque and Synthetic.
        => PreparedSelectionMaterialContext.Capture(material, binding, canonicalBinding, input, admissions); // Delegate to the sole checked constructor path.

    internal sealed record TeachingDisclosureInspection(string PacketSha256, int CandidateCount, string ApprovalEvidenceSha256); // Expose only packet digest, candidate count and approval evidence as the fixed inspection result.

    internal static Task<OperationResult<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>> CaptureStartupApprovedNativeSelectionAsync(
        CodeDocumentationOneLessonSession owner, CodeDocumentationOneLessonSession.PreparedSelectionMaterialReceipt material, CancellationToken token)
    {
        var context = material.Context;
        var input = context.MetadataInput;
        using var document = JsonDocument.Parse(input.CopyBytes());
        var curriculum = document.RootElement.GetProperty("TeachingGraph").GetProperty("Selection").GetProperty("TeachingCurriculum");
        var ids = curriculum.GetProperty("CategoryBindings").EnumerateArray().Where(c => input.Categories.Contains(c.GetProperty("Category").GetString()!, StringComparer.Ordinal))
            .SelectMany(c => c.GetProperty("OrderedItemIds").EnumerateArray().Select(i => i.GetString()!)).ToHashSet(StringComparer.Ordinal);
        var eligible = curriculum.GetProperty("Items").EnumerateArray().Where(item => ids.Contains(item.GetProperty("ItemId").GetString()!)).ToArray();
        if (!material.IsActiveFor(owner) || context.Origin != "ProtectedStartupOperatorApproval" || context.FullTeacherContextReady ||
            !owner.IsActiveTeachingDisclosureContext(context) || context.Candidates.Count == 0 ||
            Convert.ToHexString(SHA256.HashData(context.CopyCanonicalBytes())).ToLowerInvariant() != context.PacketSha256 ||
            context.Candidates.Count != eligible.Length || context.Candidates.Where((c, i) => c.ItemId != eligible[i].GetProperty("ItemId").GetString() ||
                c.SourceId != eligible[i].GetProperty("SourceId").GetString() || c.ReferenceEntryId != eligible[i].GetProperty("ReferenceEntryId").GetString() ||
                c.SourceEntityFingerprint != eligible[i].GetProperty("SourceEntityFingerprint").GetString()).Any())
            return Task.FromResult(InvalidSelectedMaterial<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>());
        if (owner.TeachingDisclosureInspectionRefusalForTesting)
            return Task.FromResult(InvalidSelectedMaterial<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>());
        return owner.PrepareNativeSelectionRequestUnderHeldMaterialAsync(material, token);
    }

    internal static OperationResult<PreparedSelectionMaterialContext> CaptureStartupApprovedSelectionMaterialContext( // Route Session's protected capture to the private genuine-readback constructor.
        CodeDocumentationOneLessonSession owner, CodeDocumentationOneLessonSession.PreparedTeachingDisclosureReceipt approval, // Carry the concrete owner and its actual opaque receipt, never a serialized capability.
        CodeDocumentationOneLessonSourceSelection.TeachingSelectionMaterialReadback material, // Carry the actual full source material readback into the fixed recipient.
        string binding, byte[] canonicalBinding, CodeDocumentationTeachingSelectionInputV1 input) // Carry complete receiving and canonical input bytes without a digest-only shortcut.
        => PreparedSelectionMaterialContext.CaptureProtected(owner, approval, material, binding, canonicalBinding, input); // Delegate to the full checked reconstruction before producing any protected context.

    internal static OperationResult<TeachingDisclosureInspection> InspectStartupApprovedSelectionMaterialContext( // Inspect only the active protected context retained by the actual Session operation.
        CodeDocumentationOneLessonSession owner, CodeDocumentationOneLessonSession.PreparedSelectionMaterialReceipt material) // Require the Session-issued material receipt on that same concrete owner.
    { // Authenticate lineage and complete packet before returning bounded outward evidence.
        var context = material.Context; // Read the actual context from the opaque material receipt rather than caller DTO input.
        if (!material.IsActiveFor(owner) || context.Origin != "ProtectedStartupOperatorApproval" || context.FullTeacherContextReady || // Require active receipt, protected origin and false full-Teacher readiness.
            !owner.IsActiveTeachingDisclosureContext(context) || context.Candidates.Count == 0 || // Require same active Session context and at least one complete candidate.
            Convert.ToHexString(SHA256.HashData(context.CopyCanonicalBytes())).ToLowerInvariant() != context.PacketSha256) return InvalidSelectedMaterial<TeachingDisclosureInspection>(); // Recompute the complete packet commitment and refuse altered recipient bytes.
        if (owner.TeachingDisclosureInspectionRefusalForTesting) return InvalidSelectedMaterial<TeachingDisclosureInspection>(); // Keep the test-only staged refusal at the actual fixed inspector after real admission.
        return OperationResult<TeachingDisclosureInspection>.Ok(new(context.PacketSha256, context.Candidates.Count, owner.ActiveTeachingDisclosureEvidenceSha256!)); // Return only digest/count/approval evidence, never raw task or reference text.
    } // Finish inspection without provider dispatch, append or training effects.

    internal sealed class PartialSelectedMaterialContext // Actual partial payload is deliberately incompatible with teacher/pair/trainer inputs.
    { // A private recipient snapshot carries no weak response, fabricated routing row or lesson-ready claim.
        internal string OwnerInput { get; } // Preserve the complete actual selected task.
        internal string PaidModelReferenceAnswer { get; } // Preserve the paid reference solely as context-only material.
        internal PaidReferenceProvenance PaidReferenceProvenance { get; } // Retain immutable paid-reference restrictions.
        internal string SourceSha256 { get; } // Identify the whole SOURCE actually rechecked at the boundary.
        internal string BindingSha256 { get; } // Identify the receiving Session binding.
        internal string FrozenContextSha256 { get; } // Identify the complete saved operator context.
        internal string MaterialAdmissionEvidenceSha256 { get; } // Identify the actual opaque synthetic eligibility object.
        internal CodeDocumentationOneLessonTeachingItemV2 Item { get; } // Identify the genuine nonordinal selected join.
        internal string SelectionReason { get; } // Retain the ORIGINAL-guarded saved semantic reason.
        internal string Origin => "SyntheticTest"; // A public scalar policy cannot certify real rights or Native readiness.
        private readonly byte[] taskBytes, referenceBytes; // Prevent byte-array aliases from mutating the captured context.
        internal byte[] CopyOwnerInputUtf8() => taskBytes.ToArray(); // Return the full task snapshot, with no truncation.
        internal byte[] CopyPaidReferenceUtf8() => referenceBytes.ToArray(); // Return the full context-only reference snapshot.
        private PartialSelectedMaterialContext(CodeDocumentationOneLessonSourceSelection.SelectedTeachingMaterialReadback material, SelectedTeachingMaterialAdmission admission, CodeDocumentationTeachingSelectionPayloadV1 payload) // Construction remains inside the actual recipient owner.
        { // Preserve genuine owner payload without converting it into pair sides.
            OwnerInput = material.OwnerInput; PaidModelReferenceAnswer = material.PaidModelReferenceAnswer; PaidReferenceProvenance = material.PaidReferenceProvenance; // Copy the complete actual immutable payload.
            taskBytes = material.CopyOwnerInputUtf8(); referenceBytes = material.CopyPaidReferenceUtf8(); // Own complete detached strict UTF8 buffers.
            SourceSha256 = admission.SourceSha256; BindingSha256 = admission.BindingSha256; FrozenContextSha256 = admission.FrozenContextSha256; // Bind the received bytes to the checked context owner.
            MaterialAdmissionEvidenceSha256 = admission.EvidenceSha256; Item = material.Item.DeclaredItem with { }; SelectionReason = payload.Reason; // Preserve exact selected identity, evidence and guarded reason.
        } // Full weak Student response/history/tokenizer/teacher gates remain open.
        internal static OperationResult<PartialSelectedMaterialContext> Capture(CodeDocumentationOneLessonSourceSelection.SelectedTeachingMaterialReadback material, SelectedTeachingMaterialAdmission admission, CodeDocumentationOneLessonSession.SelectionBindingReadback receiving) // The sole constructor path validates genuine material, opaque admission and issued saved choice.
        { // No weaker internal factory can create a partial context from unchecked payload syntax.
            if (!receiving.HasCurrentIssuingOwner) return InvalidSelectedMaterial<PartialSelectedMaterialContext>(); // Reject a detached saved readback after its genuine owner was disposed.
            var p = receiving.Payload; // Consume the actual saved receiving choice rather than a caller DTO.
            var input = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(p.OriginalReceiptId, p.OriginalReceiptSha256, p.Categories, material.Graph); // Recapture all actual recipient context bytes from the complete genuine graph.
            if (!input.IsSuccess || input.Value is null) return input.MapError<PartialSelectedMaterialContext>(); // Malformed current input cannot reach the constructor.
            var matched = admission.Matches(receiving.BindingSha256, input.Value, p); // Compare the actual issuing Session binding, frozen context and saved evidence at this recipient.
            if (!matched.IsSuccess) return matched.MapError<PartialSelectedMaterialContext>(); // Preserve exact admission refusal before full payload capture.
            if (material.Graph.Selection.SourceSelectionSha256 != admission.SourceSha256 || material.Item.DeclaredItem != admission.Item || // Require the same complete SOURCE and selected join.
                material.Item.Entry != admission.Entry || FrozenReferenceEntry.ComputeNormalizedTextSha256(material.OwnerInput) != admission.Entry.OwnerInputSha256 || // Match the actual full task against its genuine commitment.
                !TryGetSelectedReferenceHash(material.PaidModelReferenceAnswer, admission.Entry.PaidReferenceProvenance, out _) || // Select the factory-owned v1 normalized or v2 exact UTF8 commitment without fallback.
                material.PaidReferenceProvenance != admission.Entry.PaidReferenceProvenance || p.MaterialAdmissionEvidenceSha256 != admission.EvidenceSha256 || // Retain exact provenance and saved opaque eligibility digest.
                p.SourceSha256 != admission.SourceSha256 || p.ItemId != admission.Item.ItemId || p.SourceId != admission.Item.SourceId || p.ReferenceEntryId != admission.Item.ReferenceEntryId || p.SourceEntityFingerprint != admission.Item.SourceEntityFingerprint) // Preserve every saved selected-link endpoint at the recipient.
                return InvalidSelectedMaterial<PartialSelectedMaterialContext>(); // Discard all staged bytes on any mismatch.
            var context = new PartialSelectedMaterialContext(material, admission, p); // This is the only private constructor call and consumes full actual payload.
            SelectedMaterialCapturedForTesting?.Invoke(context); // Tests observe/fault the genuine recipient; production composition has no observer.
            return OperationResult<PartialSelectedMaterialContext>.Ok(context); // Session must still finish its finite ORIGINAL/whole SOURCE/receiving checks.
        } // No teacher, ResolveOrAuthorSinglePair, dataset sealer or trainer is called.
    } // End the explicit partial payload type.
    internal static OperationResult<PartialSelectedMaterialContext> CaptureSelectedTeachingMaterialContext( // The existing authoring owner is the first actual selected-payload recipient.
        CodeDocumentationOneLessonSourceSelection.SelectedTeachingMaterialReadback material, SelectedTeachingMaterialAdmission admission, CodeDocumentationOneLessonSession.SelectionBindingReadback receiving) // Require genuine reconstruction, opaque eligibility and issued saved choice.
        => PartialSelectedMaterialContext.Capture(material, admission, receiving); // Keep the sole constructor behind all recipient checks, with no digest-only factory.
    private static OperationResult<T> InvalidSelectedMaterial<T>() // Keep material refusal bounded and free of raw task/reference egress.
        => OperationResult<T>.Fail("Selected teaching material admission does not match the held context.", SelectedMaterialAdmissionInvalidErrorCode); // Scalar hash equality does not grant rights.

    private static bool TryGetSelectedReferenceHash(string reference, PaidReferenceProvenance provenance, out string referenceHash) // Reuse the two immutable provenance factories; this content guard issues no rights.
    { // Validate the canonical digest before a history consumer can inspect its bare alias.
        referenceHash = provenance.IsRawUtf8 ? provenance.ReferenceAnswerRawUtf8Sha256! : provenance.ReferenceAnswerSha256; // A present raw field selects v2 even when malformed, never legacy fallback.
        if (!FrozenReferenceCanonicalization.IsCanonicalSha256Ref(referenceHash) || // Empty, malformed or noncanonical hashes remain closed.
            (provenance.IsRawUtf8 && provenance.ReferenceAnswerSha256 != string.Empty)) return false; // Raw v2 must not overload the legacy normalized field.
        var expected = provenance.IsRawUtf8 ? FrozenReferenceCanonicalization.RawUtf8Sha256Ref(reference) : FrozenReferenceEntry.ComputeNormalizedTextSha256(reference); // Preserve the existing v1 calculation and exact v2 bytes.
        if (!StringComparer.Ordinal.Equals(expected, referenceHash)) return false; // A valid hash grammar cannot replace matching full content.
        var reconstructed = provenance.IsRawUtf8 // Recreate the known factory schema instead of admitting an unknown provenance version.
            ? PaidReferenceProvenance.CreateRawUtf8(provenance.ProviderRef, provenance.ModelId, provenance.ModelVersion, provenance.RequestId, provenance.CapturedAtUtc, reference) // Raw factory owns complete canonical v2 provenance.
            : PaidReferenceProvenance.Create(provenance.ProviderRef, provenance.ModelId, provenance.ModelVersion, provenance.RequestId, provenance.CapturedAtUtc, referenceHash); // The unchanged legacy factory owns canonical v1 provenance.
        return reconstructed.IsSuccess && reconstructed.Value == provenance; // Full immutable factory equality rejects altered schemas and restriction fields.
    } // This helper never normalizes stored raw strings or constructs a training pair.


    private static readonly AsyncLocal<Action<PartialSelectedStudentContext>?> SelectedStudentObserver = new(); // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
    internal static Action<PartialSelectedStudentContext>? SelectedStudentContextCapturedForTesting // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
    { // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        get => SelectedStudentObserver.Value; // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        set => SelectedStudentObserver.Value = value; // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
    } // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
    internal sealed class PartialSelectedStudentContext // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
    { // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        internal PartialSelectedMaterialContext Material { get; } // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        internal CodeDocumentationSelectedStudentRequestV1 Request { get; } // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        internal string StudentResponse { get; } // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        internal string StudentResponseSha256 { get; } // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        internal string ModelStateAfterSha256 { get; } // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        internal string StartedFrameSha256 { get; } // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        internal string CompletedFrameSha256 { get; } // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        internal string Origin => "SyntheticTest"; // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        internal bool FullTeacherContextReady => false; // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        private readonly byte[] responseBytes; // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        private readonly CodeDocumentationOneLessonSession.StudentResponseReadback receivingResponse; // Keep the private current receiving owner, not a scalar history-approval label.
        internal bool HasTeacherHistoryIssuingOwner => receivingResponse.HasTeacherHistoryIssuingOwner; // Only the held completed-context operation may issue Synthetic history acceptance.
        internal byte[] CopyStudentResponseUtf8() => responseBytes.ToArray(); // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        private PartialSelectedStudentContext(PartialSelectedMaterialContext material, CodeDocumentationOneLessonSession.StudentResponseReadback response) // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        { // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
            Material = material; Request = response.Request with { GenerationSettings = response.Request.GenerationSettings with { } }; // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
            receivingResponse = response; // Retain this operation's owner-confined readback without exporting a model or lease.
            StudentResponse = response.Response; responseBytes = response.CopyResponseUtf8(); // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
            StudentResponseSha256 = CodeDocumentationOneLessonV2Wire.SelectedStudentBytesSha256(responseBytes); // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
            ModelStateAfterSha256 = response.ModelStateAfterSha256; // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
            StartedFrameSha256 = response.StartedFrameSha256; CompletedFrameSha256 = response.CompletedFrameSha256; // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        } // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        internal static OperationResult<PartialSelectedStudentContext> Capture(PartialSelectedMaterialContext material, // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
            CodeDocumentationOneLessonSession.StudentResponseReadback response) // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        { // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
            if (material is null || response is null || !response.HasCurrentIssuingOwner) // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
                return InvalidSelectedMaterial<PartialSelectedStudentContext>(); // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
            var request = response.Request; // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
            var task = material.CopyOwnerInputUtf8(); // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
            var raw = response.CopyResponseUtf8(); // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
            if (!CodeDocumentationOneLessonV2Wire.ValidSelectedStudentRequest(request) || // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
                !CodeDocumentationOneLessonV2Wire.ValidSelectedStudentResponse(request, response.Response, response.ModelStateAfterSha256, out var validated) || // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
                !validated.AsSpan().SequenceEqual(raw) || !task.AsSpan().SequenceEqual(SelectedMaterialUtf8.GetBytes(request.Task)) || // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
                CodeDocumentationOneLessonV2Wire.SelectedStudentBytesSha256(task) != request.TaskSha256 || // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
                request.TaskUtf8ByteLength != task.Length.ToString(System.Globalization.CultureInfo.InvariantCulture) || // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
                material.BindingSha256 != request.BindingSha256 || material.SourceSha256 != request.SourceSha256 || // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
                material.FrozenContextSha256 != request.FrozenContextSha256 || // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
                material.MaterialAdmissionEvidenceSha256 != request.MaterialAdmissionEvidenceSha256 || // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
                material.Item.ItemId != request.ItemId || material.Item.SourceId != request.SourceId || // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
                material.Item.ReferenceEntryId != request.ReferenceEntryId || material.Item.SourceEntityFingerprint != request.SourceEntityFingerprint || // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
                request.ModelStateBeforeSha256 != response.ModelStateAfterSha256) // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
                return InvalidSelectedMaterial<PartialSelectedStudentContext>(); // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
            var context = new PartialSelectedStudentContext(material, response); // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
            SelectedStudentContextCapturedForTesting?.Invoke(context); // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
            return OperationResult<PartialSelectedStudentContext>.Ok(context); // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        } // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
    } // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
    internal static OperationResult<PartialSelectedStudentContext> CaptureSelectedStudentContext( // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        PartialSelectedMaterialContext material, CodeDocumentationOneLessonSession.StudentResponseReadback response) // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.
        => PartialSelectedStudentContext.Capture(material, response); // Consume only issued full task/material and durable raw Student bytes; this partial recipient grants no Teacher or pair effect.

    internal const string HistoryAdmissionRequiredErrorCode = "ONE_LESSON_HISTORY_ADMISSION_REQUIRED"; // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
    internal const string HistoryAdmissionInvalidErrorCode = "ONE_LESSON_HISTORY_ADMISSION_INVALID"; // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
    internal const string TeacherContentTooLargeErrorCode = "ONE_LESSON_WHOLE_CONTEXT_TOO_LARGE"; // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
    internal const int MaximumTeacherContentUtf8Bytes = 65_536; // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
    private static readonly AsyncLocal<Action<SelectedTeacherContent>?> TeacherContentObserver = new(); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
    internal static Action<SelectedTeacherContent>? SelectedTeacherContentCapturedForTesting // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
    { get => TeacherContentObserver.Value; set => TeacherContentObserver.Value = value; } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.

    internal sealed class SyntheticHistoryAssessmentInput // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
    { // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        internal PartialSelectedStudentContext Student { get; } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        internal CodeDocumentationOneLessonSourceSelection.DeclaredContextHistoryReadback History { get; } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        private SyntheticHistoryAssessmentInput(PartialSelectedStudentContext student, CodeDocumentationOneLessonSourceSelection.DeclaredContextHistoryReadback history) // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        { Student = student; History = history; } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        internal static SyntheticHistoryAssessmentInput Capture(PartialSelectedStudentContext student, CodeDocumentationOneLessonSourceSelection.DeclaredContextHistoryReadback history) // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            => new(student, history); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
    } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.

    internal sealed class SelectedHistoryAdmission // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
    { // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        internal string EvidenceSha256 { get; } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        private readonly SyntheticHistoryAssessmentInput input; // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        private readonly string requestSha256; // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        private SelectedHistoryAdmission(SyntheticHistoryAssessmentInput captured) // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        { // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            input = captured; requestSha256 = CodeDocumentationOneLessonV2Wire.SelectedStudentRequestSha256(captured.Student.Request); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream, SelectedMaterialUtf8, leaveOpen: true); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            writer.Write(Encoding.ASCII.GetBytes("code-documentation-teacher-history-admission-v1")); writer.Write((byte)0); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            writer.Write(requestSha256); writer.Write(captured.Student.StartedFrameSha256); writer.Write(captured.Student.CompletedFrameSha256); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            writer.Write(captured.History.HistorySha256); writer.Write(captured.History.SourceSha256); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            foreach (var bytes in new[] { captured.Student.Material.CopyOwnerInputUtf8(), captured.Student.Material.CopyPaidReferenceUtf8(), captured.Student.CopyStudentResponseUtf8() }) // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            { writer.Write(bytes.Length); writer.Write(bytes); } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            writer.Flush(); EvidenceSha256 = Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant(); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        internal static OperationResult<SelectedHistoryAdmission> SyntheticForTesting(PartialSelectedStudentContext student, // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            CodeDocumentationOneLessonSourceSelection.DeclaredContextHistoryReadback history, // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            Func<SyntheticHistoryAssessmentInput, CancellationToken, OperationResult<bool>> assessment, CancellationToken token) // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        { // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            if (student is null || history is null || assessment is null || !student.HasTeacherHistoryIssuingOwner || // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                student.Material.SourceSha256 != history.SourceSha256 || history.Pairs.Count != 2) // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                return InvalidHistory<SelectedHistoryAdmission>(); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            var reference = student.Material.PaidModelReferenceAnswer; // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            if (!TryGetSelectedReferenceHash(reference, student.Material.PaidReferenceProvenance, out var referenceHash)) return InvalidHistory<SelectedHistoryAdmission>(); // Reject malformed or unknown commitments before Contains or bare-hash slicing.
            foreach (var row in history.Pairs) // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            { // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                var policy = TrainingDataGovernanceAdmission.ValidateDecision(row.Pair.GovernanceDecision); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                if (!policy.IsSuccess) return policy.MapError<SelectedHistoryAdmission>(); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                if (new[] { row.Pair.Chosen, row.Pair.Rejected }.Any(s => s.Contains(reference, StringComparison.Ordinal) || // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                    s.Contains(referenceHash, StringComparison.Ordinal) || s.Contains(referenceHash[7..], StringComparison.Ordinal))) // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                    return InvalidHistory<SelectedHistoryAdmission>(); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            token.ThrowIfCancellationRequested(); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            var captured = SyntheticHistoryAssessmentInput.Capture(student, history); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            var decision = assessment(captured, token); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            token.ThrowIfCancellationRequested(); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            if (!decision.IsSuccess) return decision.MapError<SelectedHistoryAdmission>(); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            if (!decision.Value || !student.HasTeacherHistoryIssuingOwner) return InvalidHistory<SelectedHistoryAdmission>(); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            return OperationResult<SelectedHistoryAdmission>.Ok(new(captured)); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        internal bool Matches(PartialSelectedStudentContext student, CodeDocumentationOneLessonSourceSelection.DeclaredContextHistoryReadback history) // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            => student.HasTeacherHistoryIssuingOwner && input.History.SameGraph(history) && // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                requestSha256 == CodeDocumentationOneLessonV2Wire.SelectedStudentRequestSha256(student.Request) && // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                input.Student.StartedFrameSha256 == student.StartedFrameSha256 && input.Student.CompletedFrameSha256 == student.CompletedFrameSha256 && // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                input.Student.Material.BindingSha256 == student.Material.BindingSha256 && // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                input.Student.Material.SourceSha256 == student.Material.SourceSha256 && // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                input.Student.Material.FrozenContextSha256 == student.Material.FrozenContextSha256 && // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                input.Student.Material.MaterialAdmissionEvidenceSha256 == student.Material.MaterialAdmissionEvidenceSha256 && // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                input.Student.Material.CopyOwnerInputUtf8().AsSpan().SequenceEqual(student.Material.CopyOwnerInputUtf8()) && // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                input.Student.Material.CopyPaidReferenceUtf8().AsSpan().SequenceEqual(student.Material.CopyPaidReferenceUtf8()) && // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                input.Student.CopyStudentResponseUtf8().AsSpan().SequenceEqual(student.CopyStudentResponseUtf8()); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
    } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.

    internal sealed class SelectedTeacherContent // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
    { // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        internal PartialSelectedStudentContext Student { get; } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        internal CodeDocumentationOneLessonSourceSelection.DeclaredContextHistoryReadback History { get; } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        internal SelectedHistoryAdmission Admission { get; } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        internal string TeacherTokenizerAdmission => "Unavailable"; // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        internal bool FullTeacherContextReady => false; // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        internal string ContentSha256 { get; } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        private readonly byte[] packet; // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        internal byte[] CopyTeacherContentUtf8() => packet.ToArray(); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        private SelectedTeacherContent(PartialSelectedStudentContext student, CodeDocumentationOneLessonSourceSelection.DeclaredContextHistoryReadback history, // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            SelectedHistoryAdmission admission, byte[] bytes) // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        { // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            Student = student; History = history; Admission = admission; packet = bytes; // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            hash.AppendData(Encoding.ASCII.GetBytes("code-documentation-selected-teacher-content-v1")); hash.AppendData(new byte[] { 0 }); hash.AppendData(packet); // Keep the exact ASCII-plus-NUL hash domain without overload ambiguity.
            ContentSha256 = Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant(); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        internal static OperationResult<SelectedTeacherContent> Capture(PartialSelectedStudentContext student, // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            CodeDocumentationOneLessonSourceSelection.DeclaredContextHistoryReadback history, SelectedHistoryAdmission admission) // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        { // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            if (!admission.Matches(student, history)) return InvalidHistory<SelectedTeacherContent>(); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            try // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            { // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                using var stream = new TeacherContentStream(); using var w = new Utf8JsonWriter(stream); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                w.WriteStartObject(); w.WriteString("bindingSha256", student.Material.BindingSha256); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                w.WriteString("completedFrameSha256", student.CompletedFrameSha256); w.WriteString("frozenContextSha256", student.Material.FrozenContextSha256); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                w.WritePropertyName("history"); history.WriteHistory(w); w.WriteString("historyAdmissionSha256", admission.EvidenceSha256); w.WriteString("historySha256", history.HistorySha256); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                w.WriteString("materialAdmissionSha256", student.Material.MaterialAdmissionEvidenceSha256); w.WriteString("origin", "SyntheticTest"); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                w.WriteString("schema", "code-documentation-selected-teacher-content-v1"); w.WriteString("selectionFrameSha256", student.Request.SelectionFrameSha256); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                w.WriteString("sourceSha256", student.Material.SourceSha256); w.WriteString("startedFrameSha256", student.StartedFrameSha256); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                w.WriteString("studentRequestSha256", CodeDocumentationOneLessonV2Wire.SelectedStudentRequestSha256(student.Request)); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                w.WriteString("task", student.Material.OwnerInput); w.WriteString("teachingReference", student.Material.PaidModelReferenceAnswer); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                w.WriteString("weakStudentResponse", student.StudentResponse); w.WriteEndObject(); w.Flush(); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                var result = new SelectedTeacherContent(student, history, admission, stream.ToArray()); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                SelectedTeacherContentCapturedForTesting?.Invoke(result); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
                return OperationResult<SelectedTeacherContent>.Ok(result); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            catch (TeacherContentLimitException) // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
            { return OperationResult<SelectedTeacherContent>.Fail("The entire Teacher content exceeds its byte fence; no token fit is certified.", TeacherContentTooLargeErrorCode); } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
    } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
    // The authoring owner wraps a token receipt while the held session still owns every source and history check.
    internal sealed class SelectedTeacherPreparedRequest // Count instructions, rules and the entire canonical context under the current held issuer.
    { // Count instructions, rules and the entire canonical context under the current held issuer.
        internal SelectedTeacherContent Content { get; } // Count instructions, rules and the entire canonical context under the current held issuer.
        internal LocalQwenOllamaTeacherInferenceClient.PreparedAuthoringRequest Request { get; } // Count instructions, rules and the entire canonical context under the current held issuer.
        internal bool FullTeacherContextReady => false; // Count instructions, rules and the entire canonical context under the current held issuer.
        private SelectedTeacherPreparedRequest(SelectedTeacherContent content, LocalQwenOllamaTeacherInferenceClient.PreparedAuthoringRequest request) // Count instructions, rules and the entire canonical context under the current held issuer.
        { Content = content; Request = request; } // Count instructions, rules and the entire canonical context under the current held issuer.
        internal static OperationResult<SelectedTeacherPreparedRequest> Prepare(SelectedTeacherContent content, // Count instructions, rules and the entire canonical context under the current held issuer.
            ContinuousLearningTp1DpoConfiguration configuration, ConfiguredQwenVocabularyLease? native, // Count instructions, rules and the entire canonical context under the current held issuer.
            Func<string, CancellationToken, OperationResult<int[]>>? syntheticForTesting, CancellationToken token) // Count instructions, rules and the entire canonical context under the current held issuer.
        { // Count instructions, rules and the entire canonical context under the current held issuer.
            token.ThrowIfCancellationRequested(); // Count instructions, rules and the entire canonical context under the current held issuer.
            if (!content.Admission.Matches(content.Student, content.History)) return InvalidHistory<SelectedTeacherPreparedRequest>(); // Count instructions, rules and the entire canonical context under the current held issuer.
            using var stream = new MemoryStream(); // Count instructions, rules and the entire canonical context under the current held issuer.
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { MaxDepth = 64 })) // Count instructions, rules and the entire canonical context under the current held issuer.
            { // Count instructions, rules and the entire canonical context under the current held issuer.
                writer.WriteStartObject(); writer.WriteString("instruction", PairOutputInstruction); // Count instructions, rules and the entire canonical context under the current held issuer.
                writer.WriteStartArray("rules"); // Count instructions, rules and the entire canonical context under the current held issuer.
                writer.WriteStringValue("Chosen and rejected must be authored by the permitted external open Chinese teacher; follow the selected task's content and language."); // Bind the author's model role without imposing an unrequested answer language.
                writer.WriteStringValue("Do not copy the paid teaching reference; give a concrete rejected_reason."); // Count instructions, rules and the entire canonical context under the current held issuer.
                writer.WriteStringValue("History is complete context only and never optimizer training input."); // Count instructions, rules and the entire canonical context under the current held issuer.
                writer.WriteEndArray(); writer.WriteString("schema", "code-documentation-selected-teacher-prompt-v1"); // Count instructions, rules and the entire canonical context under the current held issuer.
                writer.WritePropertyName("selected_context"); writer.WriteRawValue(content.CopyTeacherContentUtf8(), skipInputValidation: false); // Count instructions, rules and the entire canonical context under the current held issuer.
                writer.WriteEndObject(); writer.Flush(); // Count instructions, rules and the entire canonical context under the current held issuer.
            } // Count instructions, rules and the entire canonical context under the current held issuer.
            var prompt = new UTF8Encoding(false, true).GetString(stream.ToArray()); // Count instructions, rules and the entire canonical context under the current held issuer.
            var prepared = LocalQwenOllamaTeacherInferenceClient.PreparedAuthoringRequest.Prepare(prompt, content.ContentSha256, // Count instructions, rules and the entire canonical context under the current held issuer.
                content.Student.Origin, configuration, native, syntheticForTesting, token); // Count instructions, rules and the entire canonical context under the current held issuer.
            if (!prepared.IsSuccess || prepared.Value is null) return prepared.MapError<SelectedTeacherPreparedRequest>(); // Count instructions, rules and the entire canonical context under the current held issuer.
            token.ThrowIfCancellationRequested(); // Count instructions, rules and the entire canonical context under the current held issuer.
            return OperationResult<SelectedTeacherPreparedRequest>.Ok(new(content, prepared.Value)); // Count instructions, rules and the entire canonical context under the current held issuer.
        } // Count instructions, rules and the entire canonical context under the current held issuer.
    } // Count instructions, rules and the entire canonical context under the current held issuer.
    internal static OperationResult<SelectedTeacherPreparedRequest> PrepareSelectedTeacherRequest(SelectedTeacherContent content, // Count instructions, rules and the entire canonical context under the current held issuer.
        ContinuousLearningTp1DpoConfiguration configuration, ConfiguredQwenVocabularyLease native, CancellationToken token) // Count instructions, rules and the entire canonical context under the current held issuer.
        => SelectedTeacherPreparedRequest.Prepare(content, configuration, native, null, token); // Count instructions, rules and the entire canonical context under the current held issuer.
    internal static OperationResult<SelectedTeacherPreparedRequest> PrepareSelectedTeacherRequestSyntheticForTesting(SelectedTeacherContent content, // Count instructions, rules and the entire canonical context under the current held issuer.
        ContinuousLearningTp1DpoConfiguration configuration, Func<string, CancellationToken, OperationResult<int[]>> provider, CancellationToken token) // Count instructions, rules and the entire canonical context under the current held issuer.
        => SelectedTeacherPreparedRequest.Prepare(content, configuration, null, provider, token); // Count instructions, rules and the entire canonical context under the current held issuer.


    private static OperationResult<T> InvalidHistory<T>() // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        => OperationResult<T>.Fail("Synthetic historical acceptance is absent, stale or does not match the exact held content.", HistoryAdmissionInvalidErrorCode); // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
    private sealed class TeacherContentLimitException : IOException { } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
    private sealed class TeacherContentStream : MemoryStream // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
    { // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        internal TeacherContentStream() : base(MaximumTeacherContentUtf8Bytes) { } // Fix only destination capacity; Utf8JsonWriter buffers and total heap remain outside this byte-fence claim.
        private void Check(int count) { if (Position + count > MaximumTeacherContentUtf8Bytes) throw new TeacherContentLimitException(); } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        public override void Write(byte[] buffer, int offset, int count) { Check(count); base.Write(buffer, offset, count); } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        public override void Write(ReadOnlySpan<byte> buffer) { Check(buffer.Length); base.Write(buffer); } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
        public override void WriteByte(byte value) { Check(1); base.WriteByte(value); } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.
    } // Bind full context-only snapshots to an invoked private Synthetic assessment; bytes never certify Teacher token fit.


    public const string CSharpDomainSlug = "csharp-code-documentation";
    public const string DomainSlug = CSharpDomainSlug;
    public const string AngularDomainSlug = "angular-code-documentation";
    public const string SqlDomainSlug = "sql-code-documentation";
    public const string DesignPatternSlug = "ilearning-unit";
    public const string DefaultDatasetVersion = "v1";
    public const string InvalidErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_AUTHORING_INVALID";
    public const string TeacherFailedErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_AUTHORING_TEACHER_FAILED";
    public const string TeacherOutputInvalidErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_AUTHORING_OUTPUT_INVALID";
    public const string TeacherProvenanceMismatchErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_AUTHORING_PROVENANCE_MISMATCH";
    public const string AuthoringSourceInvalidErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_AUTHORING_SOURCE_INVALID";
    public const string AuthoringSourceNotTrainableErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_AUTHORING_SOURCE_NOT_TRAINABLE";
    public const string AuthoringSourceRequiredErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_AUTHORING_SOURCE_REQUIRED";
    public const string AuthoringSourceBindingMismatchErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_AUTHORING_SOURCE_BINDING_MISMATCH";

    private const string PairOutputInstruction =
        "Return only JSON with string fields: chosen, rejected, rejected_reason.";

    public static CodeDocumentationRepositoryOwnedDpoDomainProfile
        CSharpDomainProfile { get; } = new(
            CSharpDomainSlug,
            DesignPatternSlug,
            "C#/.NET code documentation",
            "C#/.NET code documentation",
            "framework, language-version, caller/callee and ILearningUnit evidence");

    public static CodeDocumentationRepositoryOwnedDpoDomainProfile
        AngularDomainProfile { get; } = new(
            AngularDomainSlug,
            DesignPatternSlug,
            "Angular code documentation",
            "Angular/TypeScript code documentation",
            "component/service/module, template-binding, dependency-injection, RxJS and ILearningUnit evidence");

    public static CodeDocumentationRepositoryOwnedDpoDomainProfile
        SqlDomainProfile { get; } = new(
            SqlDomainSlug,
            DesignPatternSlug,
            "SQL code documentation",
            "SQL stored procedure and query documentation",
            "parameters, tables read or written, result shape, transaction behavior, side effects, caller evidence and ILearningUnit evidence");

    private static readonly JsonDocumentOptions JsonDocumentOptions = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = 4
    };

    public static OperationResult<CodeDocumentationRepositoryOwnedDpoDomainProfile>
        ResolveSupportedDomainProfile(string domainSlug)
        => domainSlug switch
        {
            CSharpDomainSlug => OperationResult<
                CodeDocumentationRepositoryOwnedDpoDomainProfile>.Ok(
                CSharpDomainProfile),
            AngularDomainSlug => OperationResult<
                CodeDocumentationRepositoryOwnedDpoDomainProfile>.Ok(
                AngularDomainProfile),
            SqlDomainSlug => OperationResult<
                CodeDocumentationRepositoryOwnedDpoDomainProfile>.Ok(
                SqlDomainProfile),
            _ => OperationResult<
                CodeDocumentationRepositoryOwnedDpoDomainProfile>.Fail(
                "Code documentation repository-owned DPO authoring supports only allowlisted domain profiles.",
                InvalidErrorCode)
        };

    public static OperationResult<CodeDocumentationRepositoryOwnedDpoDomainProfile>
        ResolveContextDomainProfile(
            CodeDocumentationRepositoryOwnedDpoDomainProfile? domainProfile)
    {
        if (domainProfile is null)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoDomainProfile>.Ok(
                CSharpDomainProfile);
        }

        var supported = ResolveSupportedDomainProfile(
            domainProfile.DomainSlug);
        if (!supported.IsSuccess || supported.Value is null)
            return supported;

        return domainProfile == supported.Value
            ? supported
            : OperationResult<
                CodeDocumentationRepositoryOwnedDpoDomainProfile>.Fail(
                "Code documentation repository-owned DPO authoring profile must match an allowlisted immutable profile.",
                InvalidErrorCode);
    }

    public static CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext
        CreateProductionContext(
            ContinuousLearningTp1DpoConfiguration configuration,
            Func<OperationResult<ITrainingTeacherInferenceClient>>
                createTeacherClient,
            int maxPairsToAuthor = 50,
            CodeDocumentationRepositoryOwnedDpoDomainProfile? domainProfile =
                null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(createTeacherClient);

        var domain = domainProfile ?? CSharpDomainProfile;
        var boundedPairCount = Math.Min(
            maxPairsToAuthor,
            configuration.MaxTeacherCalls);
        var qwenProfile = configuration.AuthoringProfile; // Use the same immutable passport as native preparation, actual body and deadline.
        var policyConfig = ComputePolicyConfiguration(domain, qwenProfile, boundedPairCount); // Legacy inputs retain the identical preimage and identity.
        var creatorConfig = ComputeTextSha256(string.Join(
            "\n",
            $"{domain.DomainSlug}-repository-owned-dpo-authoring-v1",
            configuration.ModelId,
            configuration.ExpectedOllamaManifestDigest,
            configuration.ExpectedOllamaModelLayerDigest,
            policyConfig));

        return new CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext(
            createTeacherClient,
            new RepositoryOwnedDpoOpenTeacherIdentity(
                OpenWeightTeacherFamily.Qwen.ToString(),
                configuration.ModelId,
                configuration.ExpectedOllamaManifestDigest,
                LegalTrainingTeacherAdmissionAuthority
                    .OllamaModelLayerSha256DigestKind,
                configuration.ExpectedOllamaModelLayerDigest),
            new RepositoryOwnedDpoGenerationPolicyIdentity(
                $"{domain.DomainSlug}-local-qwen-dpo-pair-generation",
                "v1",
                policyConfig),
            new RepositoryOwnedDpoCreatorIdentity(
                $"{domain.DomainSlug}-repository-owned-dpo-dataset-authoring",
                "v1",
                creatorConfig),
            boundedPairCount,
            domain) { InferenceProfile = qwenProfile }; // Bind the selected profile to the actual response consumer, not only to an unused descriptor.
    }

    private static string ComputePolicyConfiguration(CodeDocumentationRepositoryOwnedDpoDomainProfile domain, TrainingTeacherInferenceProfileEvidence profile, int count) // Keep one producer for the generation-policy commitment and its CPU consumer check.
        => ComputeTextSha256(string.Join("\n", $"{domain.DomainSlug}-repository-owned-dpo-generation-policy-v1", profile.ProfileId, profile.ConfigurationSha256, count.ToString(CultureInfo.InvariantCulture))); // Preserve the original framing for the legacy authoring identity.

    internal static bool HasCanonicalCpuPolicy(CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext context, CodeDocumentationRepositoryOwnedDpoDomainProfile domain) // A caller cannot relabel a legacy manifest by changing only a profile property.
    { // Recompute the exact production commitments from retained teacher identity, finite pair budget and CPU passport.
        if (context.OpenTeacher is null) return false; // Malformed CPU identity is a pre-effect refusal, never a null dereference.
        if (!LocalQwenOllamaInferenceProfileAuthority.IsCanonicalCpuAuthoringProfile(context.InferenceProfile)) return false;
        var policy = ComputePolicyConfiguration(domain, context.InferenceProfile, context.MaxPairsToAuthor);
        var creator = ComputeTextSha256(string.Join("\n", $"{domain.DomainSlug}-repository-owned-dpo-authoring-v1", context.OpenTeacher.ModelId, context.OpenTeacher.ModelVersion, context.OpenTeacher.ModelArtifactSha256, policy)); // Bind the same exact manifest/layer identity as production composition.
        return context.GenerationPolicy == new RepositoryOwnedDpoGenerationPolicyIdentity($"{domain.DomainSlug}-local-qwen-dpo-pair-generation", "v1", policy) && // Require complete generation identity rather than hash-only metadata.
            context.Creator == new RepositoryOwnedDpoCreatorIdentity($"{domain.DomainSlug}-repository-owned-dpo-dataset-authoring", "v1", creator); // Prevent a stale creator from accepting a newly relabelled CPU response.
    } // This is an identity check, not a source licence or optimizer grant.

    public static OperationResult<RepositoryOwnedDpoDatasetAddress>
        CreateDefaultAddress(
            string repositoryRoot,
            string datasetVersion = DefaultDatasetVersion)
        => CreateAddress(repositoryRoot, CSharpDomainProfile, datasetVersion);

    public static OperationResult<RepositoryOwnedDpoDatasetAddress>
        CreateAddress(
            string repositoryRoot,
            string domainSlug,
            string datasetVersion = DefaultDatasetVersion)
    {
        var profile = ResolveSupportedDomainProfile(domainSlug);
        return profile.IsSuccess && profile.Value is not null
            ? CreateAddress(repositoryRoot, profile.Value, datasetVersion)
            : profile.MapError<RepositoryOwnedDpoDatasetAddress>();
    }

    public static OperationResult<RepositoryOwnedDpoDatasetAddress>
        CreateAddress(
            string repositoryRoot,
            CodeDocumentationRepositoryOwnedDpoDomainProfile? domainProfile,
            string datasetVersion = DefaultDatasetVersion)
    {
        var profile = ResolveContextDomainProfile(domainProfile);
        if (!profile.IsSuccess || profile.Value is null)
            return profile.MapError<RepositoryOwnedDpoDatasetAddress>();

        return RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            repositoryRoot,
            profile.Value.DomainSlug,
            profile.Value.DesignPatternSlug,
            datasetVersion);
    }

    private static OperationResult<bool> ValidateAddressMatchesProfile(
        RepositoryOwnedDpoDatasetAddress? address,
        CodeDocumentationRepositoryOwnedDpoDomainProfile profile)
    {
        if (address is null ||
            profile is null ||
            !RepositoryOwnedTrainingArtifactLayout.IsValidDpoDatasetAddress(
                address) ||
            !StringComparer.Ordinal.Equals(
                address.DomainSlug,
                profile.DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                address.DesignPatternSlug,
                profile.DesignPatternSlug))
        {
            return OperationResult<bool>.Fail(
                "Code documentation repository-owned DPO address does not match the selected allowlisted domain profile.",
                InvalidErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    public static OperationResult<
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoringRequest>
        PrepareRequest(
            CodeDocumentationRepositoryOwnedDpoAuthoringSource authoringSource,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext
                authoringContext)
    {
        if (authoringSource is null)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoringRequest>.Fail(
                "CodeDocumentation DPO authoring requires a validated source envelope.",
                AuthoringSourceRequiredErrorCode);
        }

        var sourceValidation = authoringSource.ValidateForAuthoring();
        if (!sourceValidation.IsSuccess)
            return sourceValidation.MapError<
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoringRequest>();

        return PrepareRequestCore(
            authoringSource.ReferenceRows,
            authoringContext,
            authoringSource);
    }

    public static OperationResult<
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoringRequest>
        PrepareRequest(
            IReadOnlyList<RoutingDecisionRecord> paidReferenceRows,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext
                authoringContext)
        => OperationResult<
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoringRequest>.Fail(
            "CodeDocumentation DPO authoring requires an explicit source-purpose envelope; raw routing decisions are not sufficient.",
            AuthoringSourceRequiredErrorCode);

    private static OperationResult<
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoringRequest>
        PrepareRequestCore(
            IReadOnlyList<RoutingDecisionRecord> paidReferenceRows,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext
                authoringContext,
            CodeDocumentationRepositoryOwnedDpoAuthoringSource authoringSource)
    {
        if (authoringContext is null)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoringRequest>.Fail(
                "Code documentation repository-owned DPO authoring needs an open-teacher authoring context.",
                InvalidErrorCode);
        }

        var validation = authoringContext.Validate();
        if (!validation.IsSuccess ||
            paidReferenceRows is null ||
            paidReferenceRows.Count == 0)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoringRequest>.Fail(
                "Code documentation repository-owned DPO authoring needs valid reference routing decisions and an open-teacher authoring context.",
                InvalidErrorCode);
        }

        var rows = paidReferenceRows
            .Where(IsTrainReady)
            .Take(authoringContext.MaxPairsToAuthor)
            .ToArray();
        if (rows.Length == 0)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoringRequest>.Fail(
                "Code documentation repository-owned DPO authoring found no train-ready reference decisions.",
                InvalidErrorCode);
        }

        var identity = RepositoryOwnedDpoDatasetManifestSealer
            .CreateRequestIdentity(
                rows.Select(static (row, index) =>
                        ComputeTeacherRequestRowSha256(row, index))
                    .ToArray(),
                authoringContext.OpenTeacher,
                authoringContext.GenerationPolicy,
                authoringContext.Creator);
        return identity.IsSuccess && identity.Value is not null
            ? OperationResult<
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoringRequest>.Ok(
                    new CodeDocumentationRepositoryOwnedDpoDatasetAuthoringRequest(
                        Array.AsReadOnly(rows),
                        identity.Value,
                        authoringSource))
            : identity.MapError<
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoringRequest>();
    }

    public static async Task<OperationResult<RepositoryOwnedDpoDatasetResolution>>
        ResolveOrAuthorAsync(
            RepositoryOwnedDpoDatasetAddress address,
            IReadOnlyList<RoutingDecisionRecord> paidReferenceRows,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext
                authoringContext,
            CancellationToken cancellationToken = default)
    {
        var source = CodeDocumentationRepositoryOwnedDpoAuthoringSource.Create(
            paidReferenceRows,
            CodeDocumentationRepositoryOwnedDpoMaterialPurpose.Unknown,
            "untyped-routing-decision-source",
            closedExamExposed: false);
        if (!source.IsSuccess || source.Value is null)
            return source.MapError<RepositoryOwnedDpoDatasetResolution>();

        return await ResolveOrAuthorAsync(
                address,
                source.Value,
                authoringContext,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<OperationResult<RepositoryOwnedDpoDatasetResolution>>
        ResolveOrAuthorAsync(
            RepositoryOwnedDpoDatasetAddress address,
            CodeDocumentationRepositoryOwnedDpoAuthoringSource authoringSource,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext
                authoringContext,
            CancellationToken cancellationToken = default)
    {
        var prepared = PrepareRequest(authoringSource, authoringContext);
        if (!prepared.IsSuccess || prepared.Value is null)
            return prepared.MapError<RepositoryOwnedDpoDatasetResolution>();

        var profile = ResolveContextDomainProfile(
            authoringContext.DomainProfile);
        if (!profile.IsSuccess || profile.Value is null)
            return profile.MapError<RepositoryOwnedDpoDatasetResolution>();

        var addressValidation = ValidateAddressMatchesProfile(
            address,
            profile.Value);
        if (!addressValidation.IsSuccess)
            return addressValidation
                .MapError<RepositoryOwnedDpoDatasetResolution>();

        // Человеческий смысл: Qwen получает право писать пары только после
        // честного промаха по repo-диску. Если manifest уже совпал, этот delegate
        // не вызывается, значит повторный запуск не тратит модель и не дрожит.
        return await RepositoryOwnedDpoDatasetResolver
            .OpenExistingOrCreateAndSealAsync(
                address,
                prepared.Value.RequestIdentity,
                (datasetAddress, token) => AuthorPairsAsync(
                    datasetAddress,
                    prepared.Value,
                    authoringContext,
                    token),
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Resolves exactly one approved lesson without narrowing the legacy bulk
    /// route. Approval is checked before disk access; callbacks see a detached
    /// row and the opened pair must belong to that concrete lesson.
    /// </summary>
    public static async Task<OperationResult<RepositoryOwnedDpoDatasetResolution>>
        ResolveOrAuthorSinglePairAsync(
            RepositoryOwnedDpoDatasetAddress address,
            CodeDocumentationRepositoryOwnedDpoAuthoringSource authoringSource,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext
                authoringContext,
            CancellationToken cancellationToken = default)
    {
        if (authoringSource is null)
        {
            return OperationResult<RepositoryOwnedDpoDatasetResolution>.Fail(
                "CodeDocumentation DPO authoring requires a validated source envelope.",
                AuthoringSourceRequiredErrorCode);
        }

        var sourceValidation = authoringSource.ValidateForAuthoring();
        if (!sourceValidation.IsSuccess)
            return sourceValidation.MapError<RepositoryOwnedDpoDatasetResolution>();

        var originalRows = authoringSource.ReferenceRows;
        if (originalRows.Count != 1 || authoringContext is null ||
            authoringContext.MaxPairsToAuthor != 1 ||
            !HasReferenceRowShape(originalRows[0]) ||
            !IsTrainReady(originalRows[0]))
        {
            return OperationResult<RepositoryOwnedDpoDatasetResolution>.Fail(
                "A single CodeDocumentation lesson requires exactly one train-ready source row and a one-pair context.",
                InvalidErrorCode);
        }

        if (!authoringSource.MatchesReferenceRows(originalRows))
            return SinglePairBindingMismatch();

        // Snapshot every field, including the non-winning candidates visible
        // in the trainer prompt. Never pass the caller's nested array to an
        // async callback under the already validated v1 request identity.
        var row = originalRows[0];
        RoutingDecisionRecord[] detachedRows =
        [
            row with
            {
                RejectedCandidates = row.RejectedCandidates
                    .Select(static candidate => candidate with { })
                    .ToArray(),
                ScoreGap = row.ScoreGap is null ? null : row.ScoreGap with { },
                ThresholdsRef = row.ThresholdsRef is null
                    ? null
                    : row.ThresholdsRef with { }
            }
        ];
        if (!authoringSource.MatchesReferenceRows(detachedRows))
            return SinglePairBindingMismatch();

        var detachedSource = CodeDocumentationRepositoryOwnedDpoAuthoringSource.Create(
            detachedRows,
            authoringSource.MaterialPurpose,
            authoringSource.SourceIdentity,
            authoringSource.ClosedExamExposed);
        if (!detachedSource.IsSuccess || detachedSource.Value is null)
            return detachedSource.MapError<RepositoryOwnedDpoDatasetResolution>();

        var prepared = PrepareRequestCore(
            detachedRows, authoringContext, detachedSource.Value);
        if (!prepared.IsSuccess || prepared.Value is null)
            return prepared.MapError<RepositoryOwnedDpoDatasetResolution>();

        var profile = ResolveContextDomainProfile(authoringContext.DomainProfile);
        if (!profile.IsSuccess || profile.Value is null)
            return profile.MapError<RepositoryOwnedDpoDatasetResolution>();
        var addressValidation = ValidateAddressMatchesProfile(address, profile.Value);
        if (!addressValidation.IsSuccess)
            return addressValidation.MapError<RepositoryOwnedDpoDatasetResolution>();

        var request = prepared.Value;
        var resolved = await RepositoryOwnedDpoDatasetResolver
            .OpenExistingOrCreateAndSealAsync(
                address,
                request.RequestIdentity,
                (datasetAddress, token) => AuthorPairsAsync(
                    datasetAddress, request, authoringContext, token, // Use the captured strict request when the resolver authorizes cache-miss authoring.
                    includeApprovedTask: true), // Carry the approved full task only on this strict cache-miss path.
                cancellationToken)
            .ConfigureAwait(false);
        if (!resolved.IsSuccess || resolved.Value is null)
            return resolved;

        var dataset = resolved.Value.Dataset;
        if (dataset.Pairs.Count != 1 || dataset.Manifest.PairCount != 1 ||
            dataset.Manifest.OrderedPairs.Count != 1)
        {
            return OperationResult<RepositoryOwnedDpoDatasetResolution>.Fail(
                "The opened CodeDocumentation lesson must contain exactly one sealed pair.",
                InvalidErrorCode);
        }

        var pair = dataset.Pairs[0];
        var capturedRow = detachedRows[0];
        var governance = new TrainingDataGovernanceDecision(
            licenseAccepted: true,
            isQuarantined: false,
            evidenceRefs: BuildGovernanceEvidenceRefs(
                address, request.RequestIdentity, capturedRow, ordinal: 0));
        if (!StringComparer.Ordinal.Equals(
                dataset.Manifest.RequestIdentity.RequestIdentitySha256,
                request.RequestIdentity.RequestIdentitySha256) ||
            !StringComparer.Ordinal.Equals(
                dataset.Manifest.OrderedPairs[0].PairId,
                FileDpoPairStore.ComputeIdentity(pair, schemaVersion: 5)) ||
            !StringComparer.Ordinal.Equals(pair.Prompt, BuildTrainerPrompt(capturedRow)) ||
            !governance.Equals(pair.GovernanceDecision))
        {
            return SinglePairBindingMismatch();
        }

        return resolved;
    }

    private static OperationResult<RepositoryOwnedDpoDatasetResolution>
        SinglePairBindingMismatch()
        => OperationResult<RepositoryOwnedDpoDatasetResolution>.Fail(
            "The CodeDocumentation lesson does not match its approved source and request binding.",
            AuthoringSourceBindingMismatchErrorCode);

    internal static bool HasReferenceRowShape(RoutingDecisionRecord? row)
        => row is not null &&
           row.RejectedCandidates is { Length: > 0 } &&
           row.RejectedCandidates.All(static candidate => candidate is not null);

    internal static async Task<OperationResult<
        IReadOnlyList<ContinuousLearning.DpoPair>>>
        AuthorPairsAsync(
            RepositoryOwnedDpoDatasetAddress address,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoringRequest request,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext
                authoringContext,
            CancellationToken cancellationToken = default, // Preserve existing callers' cancellation boundary during dataset authoring.
            bool includeApprovedTask = false) // Bulk calls retain the original teacher bytes unless the strict resolver selects an envelope.
    {
        if (authoringContext is null)
        {
            return OperationResult<
                IReadOnlyList<ContinuousLearning.DpoPair>>.Fail(
                "Code documentation repository-owned DPO authoring needs an open-teacher authoring context.",
                InvalidErrorCode);
        }

        var profile = ResolveContextDomainProfile(
            authoringContext.DomainProfile);
        if (!profile.IsSuccess || profile.Value is null)
            return profile.MapError<
                IReadOnlyList<ContinuousLearning.DpoPair>>();

        var authoringValidation = authoringContext.Validate();
        if (!authoringValidation.IsSuccess)
            return authoringValidation.MapError<
                IReadOnlyList<ContinuousLearning.DpoPair>>();

        var sourceValidation = request?.AuthoringSource
            ?.ValidateForAuthoring();
        var addressValidation = ValidateAddressMatchesProfile(
            address,
            profile.Value);
        if (!addressValidation.IsSuccess ||
            request is null ||
            sourceValidation is null ||
            !sourceValidation.IsSuccess ||
            request.PaidReferenceRows is null ||
            request.PaidReferenceRows.Count == 0 ||
            !RepositoryOwnedDpoDatasetManifestSealer.IsValidRequestIdentity(
                request.RequestIdentity) ||
            request.RequestIdentity.OpenTeacher != authoringContext.OpenTeacher ||
            request.RequestIdentity.GenerationPolicy !=
                authoringContext.GenerationPolicy ||
                request.RequestIdentity.Creator != authoringContext.Creator)
        {
            return OperationResult<
                IReadOnlyList<ContinuousLearning.DpoPair>>.Fail(
                $"{profile.Value.DisplayName} repository-owned DPO authoring request does not match the canonical address and open-teacher identity.",
                InvalidErrorCode);
        }

        OperationResult<ITrainingTeacherInferenceClient> createdClient;
        try
        {
            createdClient = authoringContext.CreateTeacherClient();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<
                IReadOnlyList<ContinuousLearning.DpoPair>>.Fail(
                    $"The local open-weight teacher client could not be created for {profile.Value.DisplayName} DPO authoring.",
                    TeacherFailedErrorCode);
        }

        if (!createdClient.IsSuccess || createdClient.Value is null)
            return createdClient.MapError<
                IReadOnlyList<ContinuousLearning.DpoPair>>();

        using var teacherClient = createdClient.Value;
        var declaredProfile = teacherClient.AuthoringProfile; // Capture the real factory/wrapper passport once before the pair loop or permit creation.
        if ((declaredProfile is not null && declaredProfile != authoringContext.InferenceProfile) || (LocalQwenOllamaInferenceProfileAuthority.IsCanonicalCpuAuthoringProfile(authoringContext.InferenceProfile) && declaredProfile is null))
            return OperationResult<IReadOnlyList<ContinuousLearning.DpoPair>>.Fail("The teacher client profile differs from the selected authoring policy.", TeacherProvenanceMismatchErrorCode); // A known wrong passport cannot issue a call even when the expected context is legacy.
        var pairs = new List<ContinuousLearning.DpoPair>(
            request.PaidReferenceRows.Count);
        for (var index = 0; index < request.PaidReferenceRows.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var authored = await AuthorOnePairAsync(
                    address,
                    request.RequestIdentity,
                    request.PaidReferenceRows[index],
                    index,
                    profile.Value,
                    teacherClient,
                    authoringContext.InferenceProfile, // Carry the already validated exact selected passport to response provenance admission.
                    includeApprovedTask, // Forward the resolver's transport choice to this approved row's teacher call.
                    cancellationToken)
                .ConfigureAwait(false);
            if (!authored.IsSuccess || authored.Value is null)
                return authored.MapError<
                    IReadOnlyList<ContinuousLearning.DpoPair>>();

            pairs.Add(authored.Value);
        }

        return OperationResult<
            IReadOnlyList<ContinuousLearning.DpoPair>>.Ok(
            Array.AsReadOnly(pairs.ToArray()));
    }

    private static async Task<OperationResult<ContinuousLearning.DpoPair>>
        AuthorOnePairAsync(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        RoutingDecisionRecord paidReferenceRow,
        int ordinal,
        CodeDocumentationRepositoryOwnedDpoDomainProfile profile,
        ITrainingTeacherInferenceClient teacherClient,
        TrainingTeacherInferenceProfileEvidence expectedInferenceProfile, // Admit only the profile already sealed by this concrete authoring context.
        bool includeApprovedTask, // Select complete-task transport while retaining the existing context builder.
        CancellationToken cancellationToken)
    {
        var permit = new SingleUseTrainingTeacherCallPermit();
        var prompt = BuildTeacherPrompt(
            address,
            identity,
            paidReferenceRow,
            ordinal,
            profile);
        // The strict lesson carries the approved full task separately from
        // routing context. JSON preserves its text and escapes line breaks
        // rejected by the existing local teacher's transport validator.
        if (includeApprovedTask) // Apply lossless task transport only to strict authoring selected by the resolver.
        { // Keep validation and the envelope inside the same strict transport boundary.
            try // Check approved source text before JSON can substitute malformed Unicode.
            { // Validate both original texts before any InferAsync call can consume them.
                // JSON can replace malformed UTF-16. Validate both original
                // strings first so the strict lesson never teaches from a
                // normalized substitute for its approved source text.
                var strictUtf8 = new UTF8Encoding( // Reject source text that the teacher could not receive without Unicode replacement.
                    encoderShouldEmitUTF8Identifier: false, // Keep this text validation aligned with the existing BOM-free teacher transport.
                    throwOnInvalidBytes: true); // Refuse unpaired surrogates instead of admitting replacement text into the lesson.
                _ = strictUtf8.GetByteCount(paidReferenceRow.Prompt); // Verify the full approved task can reach UTF8 without rewriting its Unicode.
                _ = strictUtf8.GetByteCount(prompt); // Verify all legacy context fields before wrapping them in the strict request.
            } // Finish checking both originals before serializing the teacher request.
            catch (EncoderFallbackException) // Classify malformed approved text as an invalid request rather than a teacher failure.
            { // Stop here so no teacher call or sealed pair consumes replacement text.
                return OperationResult<ContinuousLearning.DpoPair>.Fail( // Use the existing authoring refusal contract for this unusable lesson.
                    "The approved teaching task or authoring context is not valid UTF-8 text.", // Explain that the approved task or context cannot be delivered without loss.
                    InvalidErrorCode); // Preserve callers' existing invalid-request handling for this strict refusal.
            } // Leave the rejected lesson before JSON or inference can consume malformed text.
            prompt = JsonSerializer.Serialize(new // Build the strict request from the unchanged approved task and existing context.
            { // Keep the full task separate from routing context in the same teacher request.
                approved_task = paidReferenceRow.Prompt, // Carry the private approved full task rather than its task_slice summary.
                authoring_context = prompt // Retain the existing domain, paid-reference and routing fields as teaching context.
            }); // Escape transport control characters while preserving both decoded source strings.
        } // Finish strict transport; bulk authoring continues with its original prompt.
        OperationResult<TrainingTeacherInferenceResult> teacher;
        try
        {
            teacher = await teacherClient
                .InferAsync(prompt, permit, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<ContinuousLearning.DpoPair>.Fail(
                $"The local open-weight teacher failed while authoring a {profile.DisplayName} DPO pair.",
                TeacherFailedErrorCode);
        }

        if (!teacher.IsSuccess || teacher.Value is null)
        {
            return OperationResult<ContinuousLearning.DpoPair>.Fail(
                string.IsNullOrWhiteSpace(teacher.ErrorMessage)
                    ? $"The local open-weight teacher did not return a {profile.DisplayName} DPO pair."
                    : teacher.ErrorMessage,
                string.IsNullOrWhiteSpace(teacher.ErrorCode)
                    ? TeacherFailedErrorCode
                    : teacher.ErrorCode);
        }

        if (!permit.CallIssued)
        {
            return OperationResult<ContinuousLearning.DpoPair>.Fail(
                $"The local open-weight teacher did not consume the one-call {profile.DisplayName} authoring permit.",
                TeacherFailedErrorCode);
        }

        var provenanceValidation = ValidateTeacherProvenance(
            teacher.Value,
            identity,
            profile, expectedInferenceProfile); // Returned evidence must equal the pre-call selected profile.
        if (!provenanceValidation.IsSuccess)
            return provenanceValidation.MapError<DpoPair>();

        var parsed = ParseTeacherOutput(teacher.Value.Output, paidReferenceRow);
        if (!parsed.IsSuccess || parsed.Value is null)
            return parsed.MapError<ContinuousLearning.DpoPair>();

        var passport = teacher.Value.TrainingTeacherProvenance;
        var pair = new ContinuousLearning.DpoPair(
            Prompt: BuildTrainerPrompt(paidReferenceRow),
            Chosen: parsed.Value.Chosen,
            Rejected: BuildRejectedSide(
                parsed.Value.Rejected,
                parsed.Value.RejectedReason),
            Domain: address.DomainSlug,
            CapturedAt: DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = passport,
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                evidenceRefs: BuildGovernanceEvidenceRefs(
                    address,
                    identity,
                    paidReferenceRow,
                ordinal)),
            ChosenAuthorship = new DpoPairSideAuthorship(
                DPO.Models.DpoPairSideAuthorKind.OpenTeacher,
                passport),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DPO.Models.DpoPairSideAuthorKind.OpenTeacher,
                passport)
        };

        // Привязка делается после финальной сборки текста пары: так паспорт
        // учителя закрепляется за теми байтами, которые реально попадут в Git.
        var bound = DpoPairTrainingContentAuthority.Bind(pair);
        return bound.IsSuccess && bound.Value is not null
            ? bound
            : OperationResult<ContinuousLearning.DpoPair>.Fail(
                bound.ErrorMessage,
                string.IsNullOrWhiteSpace(bound.ErrorCode)
                    ? TeacherOutputInvalidErrorCode
                    : bound.ErrorCode);
    }

    private static bool IsTrainReady(RoutingDecisionRecord row)
        => row is not null &&
           IsBoundedText(row.RunId) &&
           IsBoundedText(row.RequestId) &&
           IsBoundedText(row.Prompt) &&
           IsBoundedText(row.TaskSlice) &&
           IsBoundedText(row.ChosenUnitId) &&
           IsBoundedText(row.CorrectReason) &&
           IsBoundedText(row.WrongRouteConsequence) &&
           row.RejectedCandidates is { Length: > 0 } &&
           row.RejectedCandidates.Any(static candidate =>
               !string.IsNullOrWhiteSpace(candidate.UnitId));

    private static string BuildTeacherPrompt(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        RoutingDecisionRecord row,
        int ordinal,
        CodeDocumentationRepositoryOwnedDpoDomainProfile profile)
        => string.Join(
            "\n",
            $"You are the allowed local open-weight Chinese teacher for {profile.TeacherPromptDomain} DPO dataset authoring.",
            "Goal: create a DPO pair that helps the local model approach the paid reference answer without storing the paid reference as either DPO side.",
            PairOutputInstruction,
            "Rules:",
            "- chosen must be a teacher-authored documentation-routing answer, not a verbatim copy of paid_reference.",
            "- rejected must be a weaker teacher-authored answer, not a verbatim copy of paid_reference.",
            "- rejected_reason must explain the concrete weakness in human language.",
            $"- keep {profile.RequiredTeacherFocus} visible.",
            "Repository dataset:",
            $"domain={address.DomainSlug}",
            $"design_pattern={address.DesignPatternSlug}",
            $"dataset_version={address.DatasetVersion}",
            $"request_identity_sha256={identity.RequestIdentitySha256}",
            $"row_ordinal={ordinal.ToString(CultureInfo.InvariantCulture)}",
            $"run_id={row.RunId}",
            $"request_id={row.RequestId}",
            $"task_slice={row.TaskSlice}",
            $"paid_reference={BuildPaidReferenceAnswer(row)}",
            $"paid_reference_sha256={ComputeTextSha256(BuildPaidReferenceAnswer(row))}",
            $"weak_candidate={BuildWeakReferenceCandidate(row)}");

    private static OperationResult<AuthoredPair> ParseTeacherOutput(
        string output,
        RoutingDecisionRecord paidReferenceRow)
    {
        if (!IsBoundedText(output))
        {
            return OperationResult<AuthoredPair>.Fail(
                "The local open-weight teacher output is empty or too large.",
                TeacherOutputInvalidErrorCode);
        }

        AuthoredPairDto? dto;
        try
        {
            using var document = JsonDocument.Parse(output, JsonDocumentOptions);
            if (document.RootElement.ValueKind is not JsonValueKind.Object ||
                document.RootElement.EnumerateObject().Count() != 3 ||
                !HasOnlyStringProperty(document.RootElement, "chosen") ||
                !HasOnlyStringProperty(document.RootElement, "rejected") ||
                !HasOnlyStringProperty(document.RootElement, "rejected_reason"))
            {
                return OperationResult<AuthoredPair>.Fail(
                    "The local open-weight teacher must return exactly the three required JSON string fields.",
                    TeacherOutputInvalidErrorCode);
            }

            dto = new AuthoredPairDto(
                document.RootElement.GetProperty("chosen").GetString(),
                document.RootElement.GetProperty("rejected").GetString(),
                document.RootElement.GetProperty("rejected_reason").GetString());
        }
        catch (JsonException)
        {
            return OperationResult<AuthoredPair>.Fail(
                "The local open-weight teacher must return JSON with chosen, rejected and rejected_reason.",
                TeacherOutputInvalidErrorCode);
        }

        var chosen = NormalizeTeacherSide(dto?.Chosen);
        var rejected = NormalizeTeacherSide(dto?.Rejected);
        var reason = NormalizeReason(dto?.RejectedReason);
        var paidReference = BuildPaidReferenceAnswer(paidReferenceRow);
        if (!IsBoundedText(chosen) ||
            !IsBoundedText(rejected) ||
            !IsBoundedText(reason) ||
            string.Equals(chosen, rejected, StringComparison.Ordinal) ||
            MatchesPaidReference(chosen, paidReference) ||
            MatchesPaidReference(rejected, paidReference))
        {
            return OperationResult<AuthoredPair>.Fail(
                "The local open-weight teacher must author two non-empty DPO sides, and neither side may be the paid reference answer.",
                TeacherOutputInvalidErrorCode);
        }

        return OperationResult<AuthoredPair>.Ok(new AuthoredPair(
            chosen!,
            rejected!,
            reason!));
    }

    internal static OperationResult<bool> ValidateTeacherProvenance(
        TrainingTeacherInferenceResult teacher,
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        CodeDocumentationRepositoryOwnedDpoDomainProfile profile, // Domain semantics remain independent of the runtime budget.
        TrainingTeacherInferenceProfileEvidence expectedInferenceProfile) // The response cannot choose a different valid profile after the effect.
    {
        var provenance = teacher.TrainingTeacherProvenance;
        var expectedTeacher = identity.OpenTeacher;
        if (teacher.InferenceProfile != expectedInferenceProfile || // Require exact selected ID/hash, never any arbitrary known authoring evidence.
            !(LocalQwenOllamaInferenceProfileAuthority.IsCanonicalAuthoring(expectedInferenceProfile) || LocalQwenOllamaInferenceProfileAuthority.IsCanonicalCpuAuthoringProfile(expectedInferenceProfile)) ||
            provenance is null ||
            provenance.SourceKind is not TrainingTeacherSourceKind
                .ExternalTeacherOutput ||
            provenance.Family is not OpenWeightTeacherFamily.Qwen ||
            provenance.AccessMode is not TrainingTeacherAccessMode
                .LocalHostedOpenWeight ||
            provenance.HostingProvider is not TrainingTeacherHostingProvider
                .Ollama ||
            provenance.ModelLicense is not TrainingTeacherModelLicense.Apache20 ||
            provenance.ClosedProprietaryModel ||
            !provenance.TeacherOutputUsedForTraining ||
            provenance.InvocationCount <= 0 ||
            !string.Equals(
                provenance.ModelId,
                expectedTeacher.ModelId,
                StringComparison.Ordinal) ||
            !string.Equals(
                provenance.ModelVersion,
                expectedTeacher.ModelVersion,
                StringComparison.Ordinal) ||
            !string.Equals(
                provenance.ModelArtifactDigestKind,
                expectedTeacher.ModelArtifactDigestKind,
                StringComparison.Ordinal) ||
            !string.Equals(
                provenance.ModelArtifactDigest,
                expectedTeacher.ModelArtifactSha256,
                StringComparison.Ordinal))
        {
            return OperationResult<bool>.Fail(
                $"The local open-weight teacher provenance does not match the sealed {profile.DisplayName} DPO request identity.",
                TeacherProvenanceMismatchErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static string BuildTrainerPrompt(RoutingDecisionRecord row)
        => string.Join(
            "\n",
            $"Prompt: {row.Prompt}",
            $"Task slice: {row.TaskSlice}",
            $"Candidate units: {string.Join(", ", row.RejectedCandidates.Select(candidate => candidate.UnitId).Prepend(row.ChosenUnitId))}");

    private static string BuildPaidReferenceAnswer(RoutingDecisionRecord row)
        => $"Route to {row.ChosenUnitId}. {row.CorrectReason}";

    private static string BuildWeakReferenceCandidate(RoutingDecisionRecord row)
    {
        var rejected = row.RejectedCandidates
            .OrderByDescending(candidate => candidate.Score)
            .First();
        return $"Route to {rejected.UnitId}. {row.WrongRouteConsequence}";
    }

    private static string BuildRejectedSide(string rejected, string reason)
        => rejected.Contains("[WRONG:", StringComparison.Ordinal)
            ? rejected
            : $"{rejected} [WRONG: open_teacher_dpo_rejected - {reason}]";

    private static IReadOnlyList<string> BuildGovernanceEvidenceRefs(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        RoutingDecisionRecord row,
        int ordinal)
        =>
        [
            "sha256:" + ComputeTextSha256("code-documentation-dpo-authoring\n" +
                                          address.ManifestRelativePath),
            "sha256:" + ComputeTextSha256("paid-reference\n" +
                                          BuildPaidReferenceAnswer(row)),
            "sha256:" + ComputeTextSha256("request-row\n" + row.RequestId),
            "sha256:" + ComputeTextSha256("task-slice\n" + row.TaskSlice),
            "sha256:" + ComputeTextSha256("row-ordinal\n" +
                                          ordinal.ToString(
                                              CultureInfo.InvariantCulture)),
            "sha256:" + identity.RequestIdentitySha256
        ];

    private static bool HasOnlyStringProperty(
        JsonElement root,
        string propertyName)
    {
        var matches = root.EnumerateObject()
            .Where(property => string.Equals(
                property.Name,
                propertyName,
                StringComparison.Ordinal))
            .ToArray();
        return matches.Length == 1 &&
               matches[0].Value.ValueKind is JsonValueKind.String;
    }

    private static string? NormalizeTeacherSide(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();

    private static string? NormalizeReason(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();

    private static bool MatchesPaidReference(string? side, string paidReference)
        => string.Equals(
            side?.Trim(),
            paidReference?.Trim(),
            StringComparison.Ordinal);

    private static bool IsBoundedText(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           Encoding.UTF8.GetByteCount(value) <= 65_536 &&
           !value.Any(static character => char.IsControl(character) &&
                                          character is not '\n' and not '\r' and not '\t');

    private static string ComputeTextSha256(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    internal static string ComputeReferenceRowsIdentitySha256(
        IReadOnlyList<RoutingDecisionRecord> rows)
        => "sha256:" + ComputeTextSha256(string.Join(
            "\n",
            new[]
            {
                "code-documentation-authoring-rows-v1",
                rows.Count.ToString(CultureInfo.InvariantCulture)
            }.Concat(rows.Select((row, index) =>
                ComputeTeacherRequestRowSha256(row, index)))));

    private static string ComputeTeacherRequestRowSha256(
        RoutingDecisionRecord row,
        int ordinal)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteNumber("row_ordinal", ordinal);
            writer.WriteString("run_id", row.RunId);
            writer.WriteString("request_id", row.RequestId);
            writer.WriteString("prompt", row.Prompt);
            writer.WriteString("task_slice", row.TaskSlice);
            writer.WriteString("chosen_unit_id", row.ChosenUnitId);
            writer.WriteString("paid_reference", BuildPaidReferenceAnswer(row));
            writer.WriteString("weak_candidate", BuildWeakReferenceCandidate(row));
            writer.WriteEndObject();
            writer.Flush();
        }

        return Convert.ToHexString(SHA256.HashData(buffer.ToArray()))
            .ToLowerInvariant();
    }

    private sealed record AuthoredPair(
        string Chosen,
        string Rejected,
        string RejectedReason);

    private sealed record AuthoredPairDto(
        [property: JsonPropertyName("chosen")] string? Chosen,
        [property: JsonPropertyName("rejected")] string? Rejected,
        [property: JsonPropertyName("rejected_reason")] string? RejectedReason);

    internal sealed class SingleUseTrainingTeacherCallPermit
        : ITrainingTeacherCallPermit
    {
        private int _issued;

        public bool CallIssued => Volatile.Read(ref _issued) != 0;

        public OperationResult<int> TryIssue()
            => Interlocked.CompareExchange(ref _issued, 1, 0) == 0
                ? OperationResult<int>.Ok(1)
                : OperationResult<int>.Fail(
                    "The code documentation DPO authoring permit allows exactly one local teacher call for one row.",
                    TeacherFailedErrorCode);
    }
}
