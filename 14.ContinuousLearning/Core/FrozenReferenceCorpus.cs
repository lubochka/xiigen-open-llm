// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

/// <summary>
/// Сторона замороженного корпуса: строка либо допускается к обучающему
/// материалу, либо остаётся только для отложенной проверки.
/// </summary>
public enum FrozenReferencePartition
{
    Train,
    HeldOut
}

/// <summary>
/// Паспорт ответа закрытой платной модели, который можно использовать только
/// как эталон измерения и нельзя переименовать в учителя для DPO.
/// </summary>
public sealed record PaidReferenceProvenance
{
    private PaidReferenceProvenance(
        string providerRef,
        string modelId,
        string modelVersion,
        string requestId,
        DateTimeOffset capturedAtUtc,
        string referenceAnswerSha256,
        string canonicalRepresentation,
        string provenanceSha256,
        string? referenceAnswerRawUtf8Sha256 = null)
    {
        ProviderRef = providerRef;
        ModelId = modelId;
        ModelVersion = modelVersion;
        RequestId = requestId;
        CapturedAtUtc = capturedAtUtc;
        ReferenceAnswerSha256 = referenceAnswerSha256;
        CanonicalRepresentation = canonicalRepresentation;
        ProvenanceSha256 = provenanceSha256;
        ReferenceAnswerRawUtf8Sha256 = referenceAnswerRawUtf8Sha256;
    }

    /// <summary>Поставщик закрытой платной модели, без raw-secret и payload.</summary>
    public string ProviderRef { get; }

    /// <summary>Идентификатор закрытой платной модели, выпустившей эталон.</summary>
    public string ModelId { get; }

    /// <summary>Версия модели или снимка модели на момент получения эталона.</summary>
    public string ModelVersion { get; }

    /// <summary>Стабильный id запроса, по которому эталон был получен.</summary>
    public string RequestId { get; }

    /// <summary>UTC-время получения закрытого платного эталона.</summary>
    public DateTimeOffset CapturedAtUtc { get; }

    /// <summary>SHA-256 нормализованного текста эталонного ответа.</summary>
    public string ReferenceAnswerSha256 { get; }

    /// <summary>Отдельный hash фактических UTF-8 bytes для raw v2; v1 JSON не меняется.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ReferenceAnswerRawUtf8Sha256 { get; }

    internal bool IsRawUtf8 => ReferenceAnswerRawUtf8Sha256 is not null;

    /// <summary>Явный флаг: источник — закрытый платный reference, не open teacher.</summary>
    public bool ClosedPaidReference { get; } = true;

    /// <summary>Эти байты никогда не становятся обучающим примером.</summary>
    public bool PaidReferenceOutputNeverTrainable { get; } = true;

    /// <summary>Эти байты никогда не становятся chosen/rejected стороной DPO.</summary>
    public bool PaidReferenceOutputNeverDpoPairSide { get; } = true;

    public string CanonicalRepresentation { get; }

    public string ProvenanceSha256 { get; }

    public static OperationResult<PaidReferenceProvenance> Create(
        string providerRef,
        string modelId,
        string modelVersion,
        string requestId,
        DateTimeOffset capturedAtUtc,
        string referenceAnswerSha256)
    {
        var normalizedProvider = FrozenReferenceCanonicalization.NormalizeIdentity(providerRef);
        var normalizedModel = FrozenReferenceCanonicalization.NormalizeIdentity(modelId);
        var normalizedVersion = FrozenReferenceCanonicalization.NormalizeIdentity(modelVersion);
        var normalizedRequest = FrozenReferenceCanonicalization.NormalizeIdentity(requestId);
        if (!normalizedProvider.IsSuccess || normalizedProvider.Value is null)
            return normalizedProvider.MapError<PaidReferenceProvenance>();
        if (!normalizedModel.IsSuccess || normalizedModel.Value is null)
            return normalizedModel.MapError<PaidReferenceProvenance>();
        if (!normalizedVersion.IsSuccess || normalizedVersion.Value is null)
            return normalizedVersion.MapError<PaidReferenceProvenance>();
        if (!normalizedRequest.IsSuccess || normalizedRequest.Value is null)
            return normalizedRequest.MapError<PaidReferenceProvenance>();
        if (!FrozenReferenceCanonicalization.IsUtc(capturedAtUtc))
        {
            return Failure(
                "Paid reference capture timestamp must be caller-supplied UTC.",
                "PAID_REFERENCE_CAPTURED_AT_UTC_INVALID");
        }

        if (!FrozenReferenceCanonicalization.IsCanonicalSha256Ref(referenceAnswerSha256))
        {
            return Failure(
                "Paid reference answer hash must be a canonical sha256 ref.",
                "PAID_REFERENCE_HASH_NONCANONICAL");
        }

        var canonical = new StringBuilder();
        FrozenReferenceCanonicalization.Append(canonical, "schema", "xiigen.closed-paid-reference-provenance.v1");
        FrozenReferenceCanonicalization.Append(canonical, "provider_ref", normalizedProvider.Value);
        FrozenReferenceCanonicalization.Append(canonical, "model_id", normalizedModel.Value);
        FrozenReferenceCanonicalization.Append(canonical, "model_version", normalizedVersion.Value);
        FrozenReferenceCanonicalization.Append(canonical, "request_id", normalizedRequest.Value);
        FrozenReferenceCanonicalization.Append(canonical, "captured_at_utc", FrozenReferenceCanonicalization.FormatUtc(capturedAtUtc));
        FrozenReferenceCanonicalization.Append(canonical, "reference_answer_sha256", referenceAnswerSha256);
        FrozenReferenceCanonicalization.Append(canonical, "closed_paid_reference", "true");
        FrozenReferenceCanonicalization.Append(canonical, "paid_reference_output_never_trainable", "true");
        FrozenReferenceCanonicalization.Append(canonical, "paid_reference_output_never_dpo_pair_side", "true");
        var canonicalRepresentation = canonical.ToString();

        return OperationResult<PaidReferenceProvenance>.Ok(
            new PaidReferenceProvenance(
                normalizedProvider.Value,
                normalizedModel.Value,
                normalizedVersion.Value,
                normalizedRequest.Value,
                capturedAtUtc,
                referenceAnswerSha256,
                canonicalRepresentation,
                FrozenReferenceCanonicalization.Sha256Ref(canonicalRepresentation)));
    }

    public static OperationResult<PaidReferenceProvenance> CreateRawUtf8(
        string providerRef, string modelId, string modelVersion, string requestId,
        DateTimeOffset capturedAtUtc, string rawReferenceAnswer)
    {
        var reference = FrozenReferenceCanonicalization.ValidateRawContent(rawReferenceAnswer);
        if (!reference.IsSuccess || reference.Value is null)
            return reference.MapError<PaidReferenceProvenance>();
        var normalizedProvider = FrozenReferenceCanonicalization.NormalizeIdentity(providerRef);
        var normalizedModel = FrozenReferenceCanonicalization.NormalizeIdentity(modelId);
        var normalizedVersion = FrozenReferenceCanonicalization.NormalizeIdentity(modelVersion);
        var normalizedRequest = FrozenReferenceCanonicalization.NormalizeIdentity(requestId);
        if (!normalizedProvider.IsSuccess || normalizedProvider.Value is null)
            return normalizedProvider.MapError<PaidReferenceProvenance>();
        if (!normalizedModel.IsSuccess || normalizedModel.Value is null)
            return normalizedModel.MapError<PaidReferenceProvenance>();
        if (!normalizedVersion.IsSuccess || normalizedVersion.Value is null)
            return normalizedVersion.MapError<PaidReferenceProvenance>();
        if (!normalizedRequest.IsSuccess || normalizedRequest.Value is null)
            return normalizedRequest.MapError<PaidReferenceProvenance>();
        if (!FrozenReferenceCanonicalization.IsUtc(capturedAtUtc))
            return Failure("Paid reference capture timestamp must be caller-supplied UTC.", "PAID_REFERENCE_CAPTURED_AT_UTC_INVALID");

        var rawHash = FrozenReferenceCanonicalization.RawUtf8Sha256Ref(reference.Value);
        var canonical = new StringBuilder();
        FrozenReferenceCanonicalization.Append(canonical, "schema", "xiigen.closed-paid-reference-provenance.v2");
        FrozenReferenceCanonicalization.Append(canonical, "provider_ref", normalizedProvider.Value);
        FrozenReferenceCanonicalization.Append(canonical, "model_id", normalizedModel.Value);
        FrozenReferenceCanonicalization.Append(canonical, "model_version", normalizedVersion.Value);
        FrozenReferenceCanonicalization.Append(canonical, "request_id", normalizedRequest.Value);
        FrozenReferenceCanonicalization.Append(canonical, "captured_at_utc", FrozenReferenceCanonicalization.FormatUtc(capturedAtUtc));
        FrozenReferenceCanonicalization.Append(canonical, "reference_answer_raw_utf8_sha256", rawHash);
        FrozenReferenceCanonicalization.Append(canonical, "closed_paid_reference", "true");
        FrozenReferenceCanonicalization.Append(canonical, "paid_reference_output_never_trainable", "true");
        FrozenReferenceCanonicalization.Append(canonical, "paid_reference_output_never_dpo_pair_side", "true");
        var representation = canonical.ToString();
        return OperationResult<PaidReferenceProvenance>.Ok(new PaidReferenceProvenance(
            normalizedProvider.Value, normalizedModel.Value, normalizedVersion.Value, normalizedRequest.Value,
            capturedAtUtc, string.Empty, representation,
            FrozenReferenceCanonicalization.RawUtf8Sha256Ref(representation), rawHash));
    }

    private static OperationResult<PaidReferenceProvenance> Failure(
        string message,
        string code)
        => OperationResult<PaidReferenceProvenance>.Fail(message, code);
}

/// <summary>
/// Один неизменяемый пример frozen-reference корпуса: вход владельца предмета,
/// платный эталон, provenance эталона, предмет, сторона разреза и fingerprint
/// исходной сущности.
/// </summary>
public sealed record FrozenReferenceEntry
{
    private FrozenReferenceEntry(
        string entryId,
        string ownerInput,
        string paidModelReferenceAnswer,
        PaidReferenceProvenance? paidReferenceProvenance,
        string domainTag,
        string subjectId,
        FrozenReferencePartition partition,
        string sourceEntityFingerprint,
        string canonicalRepresentation,
        NativeCapturedReferenceProvenance? nativeCaptureProvenance = null) // Explicit alternative origin; no fake legacy model identity.
    {
        NativeCaptureProvenance = nativeCaptureProvenance; // Only the native factory supplies this immutable origin.
        EntryId = entryId;
        OwnerInput = ownerInput;
        PaidModelReferenceAnswer = paidModelReferenceAnswer;
        PaidReferenceProvenance = paidReferenceProvenance;
        DomainTag = domainTag;
        SubjectId = subjectId;
        Partition = partition;
        SourceEntityFingerprint = sourceEntityFingerprint;
        CanonicalRepresentation = canonicalRepresentation;
    }

    public string EntryId { get; }

    public string OwnerInput { get; }

    public string PaidModelReferenceAnswer { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Legacy serialized entries remain unchanged when this origin is present.
    public PaidReferenceProvenance? PaidReferenceProvenance { get; } // Known-model provenance is absent for the explicit native origin.

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Do not add a null member to any existing v1/v2 JSON entry.
    public NativeCapturedReferenceProvenance? NativeCaptureProvenance { get; } // Descriptive native capture is distinct from an issuing-model passport.

    public string DomainTag { get; }

    public string SubjectId { get; }

    public FrozenReferencePartition Partition { get; }

    public string SourceEntityFingerprint { get; }

    public string CanonicalRepresentation { get; }

    internal bool IsRawCodeDocumentation => NativeCaptureProvenance is not null || PaidReferenceProvenance?.IsRawUtf8 == true; // Both explicit raw origins preserve exact answer bytes.

    internal const string RawCodeDocumentationDomain = "csharp-code-documentation";

    internal static string ComputeNormalizedTextSha256(string value)
    {
        var normalized = FrozenReferenceCanonicalization.NormalizeContent(value);
        if (!normalized.IsSuccess || normalized.Value is null)
            throw new ArgumentException(normalized.ErrorMessage, nameof(value));
        return FrozenReferenceCanonicalization.Sha256Ref(normalized.Value);
    }

    public static OperationResult<FrozenReferenceEntry> Create(
        string ownerInput,
        string paidModelReferenceAnswer,
        PaidReferenceProvenance paidReferenceProvenance,
        string domainTag,
        string subjectId,
        FrozenReferencePartition partition,
        string sourceEntityFingerprint)
    {
        if (paidReferenceProvenance is null)
        {
            return Failure(
                "Paid reference provenance is required.",
                "FROZEN_REFERENCE_PROVENANCE_REQUIRED");
        }

        if (!paidReferenceProvenance.ClosedPaidReference ||
            !paidReferenceProvenance.PaidReferenceOutputNeverTrainable ||
            !paidReferenceProvenance.PaidReferenceOutputNeverDpoPairSide)
        {
            return Failure(
                "Paid reference provenance cannot grant training or DPO-side authority.",
                "PAID_REFERENCE_TRAINING_AUTHORITY_INVALID");
        }

        if (!Enum.IsDefined(partition))
        {
            return Failure(
                "Frozen reference partition is invalid.",
                "FROZEN_REFERENCE_PARTITION_INVALID");
        }

        var normalizedOwnerInput = FrozenReferenceCanonicalization.NormalizeContent(ownerInput);
        var normalizedReference = FrozenReferenceCanonicalization.NormalizeContent(paidModelReferenceAnswer);
        var normalizedDomain = FrozenReferenceCanonicalization.NormalizeIdentity(domainTag);
        var normalizedSubject = FrozenReferenceCanonicalization.NormalizeIdentity(subjectId);
        if (!normalizedOwnerInput.IsSuccess || normalizedOwnerInput.Value is null)
            return normalizedOwnerInput.MapError<FrozenReferenceEntry>();
        if (!normalizedReference.IsSuccess || normalizedReference.Value is null)
            return normalizedReference.MapError<FrozenReferenceEntry>();
        if (!normalizedDomain.IsSuccess || normalizedDomain.Value is null)
            return normalizedDomain.MapError<FrozenReferenceEntry>();
        if (!normalizedSubject.IsSuccess || normalizedSubject.Value is null)
            return normalizedSubject.MapError<FrozenReferenceEntry>();
        if (string.IsNullOrWhiteSpace(normalizedOwnerInput.Value) ||
            string.IsNullOrWhiteSpace(normalizedReference.Value))
        {
            return Failure(
                "Frozen reference owner input and paid reference answer are required.",
                "FROZEN_REFERENCE_CONTENT_REQUIRED");
        }

        if (!FrozenReferenceCanonicalization.IsCanonicalSha256Ref(sourceEntityFingerprint))
        {
            return Failure(
                "Source entity fingerprint must be a canonical sha256 ref.",
                "FROZEN_REFERENCE_SOURCE_HASH_NONCANONICAL");
        }

        var referenceHash =
            FrozenReferenceCanonicalization.Sha256Ref(normalizedReference.Value);
        if (!StringComparer.Ordinal.Equals(
                referenceHash,
                paidReferenceProvenance.ReferenceAnswerSha256))
        {
            return Failure(
                "Paid reference provenance hash does not match the normalized reference answer.",
                "PAID_REFERENCE_HASH_MISMATCH");
        }

        var canonical = new StringBuilder();
        FrozenReferenceCanonicalization.Append(canonical, "schema", "xiigen.frozen-reference-entry.v1");
        FrozenReferenceCanonicalization.Append(canonical, "domain_tag", normalizedDomain.Value);
        FrozenReferenceCanonicalization.Append(canonical, "subject_id", normalizedSubject.Value);
        FrozenReferenceCanonicalization.Append(canonical, "partition", CanonicalPartition(partition));
        FrozenReferenceCanonicalization.Append(canonical, "source_entity_fingerprint", sourceEntityFingerprint);
        FrozenReferenceCanonicalization.Append(canonical, "owner_input", normalizedOwnerInput.Value);
        FrozenReferenceCanonicalization.Append(canonical, "paid_model_reference_answer", normalizedReference.Value);
        FrozenReferenceCanonicalization.Append(canonical, "paid_reference_provenance", paidReferenceProvenance.CanonicalRepresentation);

        var canonicalRepresentation = canonical.ToString();
        return OperationResult<FrozenReferenceEntry>.Ok(
            new FrozenReferenceEntry(
                FrozenReferenceCanonicalization.Sha256Ref(canonicalRepresentation),
                normalizedOwnerInput.Value,
                normalizedReference.Value,
                paidReferenceProvenance,
                normalizedDomain.Value,
                normalizedSubject.Value,
                partition,
                sourceEntityFingerprint,
                canonicalRepresentation));
    }

    public static OperationResult<FrozenReferenceEntry> CreateRawCodeDocumentation(
        string ownerInput, string paidModelReferenceAnswer, PaidReferenceProvenance paidReferenceProvenance,
        string subjectId, FrozenReferencePartition partition, string sourceEntityFingerprint)
    {
        if (paidReferenceProvenance is null || !paidReferenceProvenance.IsRawUtf8 ||
            !paidReferenceProvenance.ClosedPaidReference || !paidReferenceProvenance.PaidReferenceOutputNeverTrainable ||
            !paidReferenceProvenance.PaidReferenceOutputNeverDpoPairSide)
            return Failure("Raw C# reference requires raw v2 closed-reference provenance.", "FROZEN_REFERENCE_PROVENANCE_REQUIRED");
        if (!Enum.IsDefined(partition))
            return Failure("Frozen reference partition is invalid.", "FROZEN_REFERENCE_PARTITION_INVALID");
        var input = FrozenReferenceCanonicalization.ValidateRawContent(ownerInput);
        var reference = FrozenReferenceCanonicalization.ValidateRawContent(paidModelReferenceAnswer);
        var subject = FrozenReferenceCanonicalization.NormalizeIdentity(subjectId);
        if (!input.IsSuccess || input.Value is null) return input.MapError<FrozenReferenceEntry>();
        if (!reference.IsSuccess || reference.Value is null) return reference.MapError<FrozenReferenceEntry>();
        if (!subject.IsSuccess || subject.Value is null) return subject.MapError<FrozenReferenceEntry>();
        if (!FrozenReferenceCanonicalization.IsCanonicalSha256Ref(sourceEntityFingerprint))
            return Failure("Source entity fingerprint must be a canonical sha256 ref.", "FROZEN_REFERENCE_SOURCE_HASH_NONCANONICAL");
        if (!StringComparer.Ordinal.Equals(FrozenReferenceCanonicalization.RawUtf8Sha256Ref(reference.Value),
                paidReferenceProvenance.ReferenceAnswerRawUtf8Sha256))
            return Failure("Paid reference provenance hash does not match the raw reference answer.", "PAID_REFERENCE_HASH_MISMATCH");

        var canonical = new StringBuilder();
        FrozenReferenceCanonicalization.Append(canonical, "schema", "xiigen.frozen-reference-entry.v2");
        FrozenReferenceCanonicalization.Append(canonical, "domain_tag", RawCodeDocumentationDomain);
        FrozenReferenceCanonicalization.Append(canonical, "subject_id", subject.Value);
        FrozenReferenceCanonicalization.Append(canonical, "partition", CanonicalPartition(partition));
        FrozenReferenceCanonicalization.Append(canonical, "source_entity_fingerprint", sourceEntityFingerprint);
        FrozenReferenceCanonicalization.Append(canonical, "owner_input", input.Value);
        FrozenReferenceCanonicalization.Append(canonical, "paid_model_reference_answer", reference.Value);
        FrozenReferenceCanonicalization.Append(canonical, "paid_reference_provenance", paidReferenceProvenance.CanonicalRepresentation);
        var representation = canonical.ToString();
        return OperationResult<FrozenReferenceEntry>.Ok(new FrozenReferenceEntry(
            FrozenReferenceCanonicalization.RawUtf8Sha256Ref(representation), input.Value, reference.Value,
            paidReferenceProvenance, RawCodeDocumentationDomain, subject.Value, partition,
            sourceEntityFingerprint, representation));
    }

    public static OperationResult<FrozenReferenceEntry> CreateNativeCapturedCodeDocumentation( // Bind raw C# material to the explicit closed native capture origin.
        string ownerInput, string referenceAnswer, NativeCapturedReferenceProvenance provenance, // Provenance conveys no Teacher or admission authority.
        string subjectId, FrozenReferencePartition partition, string sourceEntityFingerprint) // Keep the existing explicit source partition/fingerprint contracts.
    { // Preserve the exact raw source/answer and fully bound provenance.
        if (provenance is null || !provenance.ClosedReference || !provenance.ReferenceOutputNeverTrainable || !provenance.ReferenceOutputNeverDpoPairSide) // Recreate fixed reference-only restrictions.
            return Failure("Native capture reference provenance is required.", "FROZEN_REFERENCE_PROVENANCE_REQUIRED"); // Never fall back to a known-model passport.
        if (!Enum.IsDefined(partition)) return Failure("Frozen reference partition is invalid.", "FROZEN_REFERENCE_PARTITION_INVALID"); // Retain the existing partition admission rule.
        var input = FrozenReferenceCanonicalization.ValidateRawContent(ownerInput); // Validate without line-ending or Unicode normalization.
        var reference = FrozenReferenceCanonicalization.ValidateRawContent(referenceAnswer); // Validate all original answer bytes before hash comparison.
        var subject = FrozenReferenceCanonicalization.NormalizeIdentity(subjectId); // Identity normalization remains separate from raw content.
        if (!input.IsSuccess || input.Value is null) return input.MapError<FrozenReferenceEntry>(); // Preserve the actual content error.
        if (!reference.IsSuccess || reference.Value is null) return reference.MapError<FrozenReferenceEntry>(); // Refuse invalid reference bytes.
        if (!subject.IsSuccess || subject.Value is null) return subject.MapError<FrozenReferenceEntry>(); // Refuse invalid owner identity.
        if (!FrozenReferenceCanonicalization.IsCanonicalSha256Ref(sourceEntityFingerprint)) // A fingerprint is descriptive identity only.
            return Failure("Source entity fingerprint must be canonical.", "FROZEN_REFERENCE_SOURCE_HASH_NONCANONICAL"); // Retain exact canonical hash grammar.
        if (FrozenReferenceCanonicalization.RawUtf8Sha256Ref(reference.Value) != provenance.ReferenceAnswerRawUtf8Sha256) // Do not bind a different answer under matching metadata.
            return Failure("Native reference answer hash does not match.", "PAID_REFERENCE_HASH_MISMATCH"); // Refuse altered captured answer bytes before corpus creation.
        var canonical = new StringBuilder(); // Reuse exact length-framed entry identity.
        FrozenReferenceCanonicalization.Append(canonical, "schema", "xiigen.frozen-native-capture-reference-entry.v1"); // Keep this origin distinct from both existing schemas.
        FrozenReferenceCanonicalization.Append(canonical, "domain_tag", RawCodeDocumentationDomain); // Limit the new entry to the actual C# scientific consumer.
        FrozenReferenceCanonicalization.Append(canonical, "subject_id", subject.Value); // Preserve the subject identity.
        FrozenReferenceCanonicalization.Append(canonical, "partition", CanonicalPartition(partition)); // Preserve the original frozen split.
        FrozenReferenceCanonicalization.Append(canonical, "source_entity_fingerprint", sourceEntityFingerprint); // Bind the source family commitment.
        FrozenReferenceCanonicalization.Append(canonical, "owner_input", input.Value); // Bind exact owner input bytes rather than an edited prompt.
        FrozenReferenceCanonicalization.Append(canonical, "paid_model_reference_answer", reference.Value); // Preserve the historical consumer answer field without a known-model identity claim.
        FrozenReferenceCanonicalization.Append(canonical, "native_capture_provenance", provenance.CanonicalRepresentation); // Bind every nullable capture metadata field and fixed restrictions.
        var representation = canonical.ToString(); // Complete the exact entry preimage before deriving its ID.
        return OperationResult<FrozenReferenceEntry>.Ok(new( // Construct one immutable entry with exactly one origin.
            FrozenReferenceCanonicalization.RawUtf8Sha256Ref(representation), input.Value, reference.Value, null, // Native origin leaves the legacy passport absent.
            RawCodeDocumentationDomain, subject.Value, partition, sourceEntityFingerprint, representation, provenance)); // Preserve the full capture provenance and raw material.
    } // No model call, right grant or training occurs in entry construction.

    internal static string CanonicalPartition(FrozenReferencePartition partition)
        => partition switch
        {
            FrozenReferencePartition.Train => "train",
            FrozenReferencePartition.HeldOut => "held-out",
            _ => throw new ArgumentOutOfRangeException(nameof(partition))
        };

    private static OperationResult<FrozenReferenceEntry> Failure(string message, string code)
        => OperationResult<FrozenReferenceEntry>.Fail(message, code);
}

public sealed record FrozenReferenceCorpusSplitProof(
    IReadOnlyList<string> TrainSourceEntityFingerprints,
    IReadOnlyList<string> HeldOutSourceEntityFingerprints,
    IReadOnlyList<string> SourceEntityFingerprints,
    int CrossPartitionSourceEntityOverlapCount,
    string Method,
    double OverlapThreshold);

/// <summary>
/// Детерминированный снимок одного предметного корпуса эталонов. Version и
/// FrozenAtUtc входят в hash, поэтому новая заморозка не маскируется старым id.
/// </summary>
public sealed record FrozenReferenceCorpus
{
    private FrozenReferenceCorpus(
        string corpusName,
        string corpusVersion,
        DateTimeOffset frozenAtUtc,
        string domainTag,
        string subjectId,
        IReadOnlyList<FrozenReferenceEntry> entries,
        IReadOnlyList<string> entryNames,
        string corpusSha256,
        string canonicalRepresentation,
        FrozenReferenceCorpusSplitProof splitProof)
    {
        CorpusName = corpusName;
        CorpusVersion = corpusVersion;
        FrozenAtUtc = frozenAtUtc;
        DomainTag = domainTag;
        SubjectId = subjectId;
        Entries = entries;
        EntryNames = entryNames;
        CorpusSha256 = corpusSha256;
        CanonicalRepresentation = canonicalRepresentation;
        SplitProof = splitProof;
    }

    public string CorpusName { get; }

    public string CorpusVersion { get; }

    public DateTimeOffset FrozenAtUtc { get; }

    public string DomainTag { get; }

    public string SubjectId { get; }

    public IReadOnlyList<FrozenReferenceEntry> Entries { get; }

    public IReadOnlyList<string> EntryNames { get; }

    public int Count => Entries.Count;

    public string CorpusSha256 { get; }

    public string CanonicalRepresentation { get; }

    public FrozenReferenceCorpusSplitProof SplitProof { get; }

    internal bool IsRawCodeDocumentation => Entries[0].IsRawCodeDocumentation;

    internal bool IsNativeCapture => Entries[0].NativeCaptureProvenance is not null; // A corpus has one explicit origin variant after Build rejects mixtures.

    public static OperationResult<FrozenReferenceCorpus> Build(
        string corpusName,
        string corpusVersion,
        DateTimeOffset frozenAtUtc,
        IEnumerable<FrozenReferenceEntry?> entries)
    {
        var normalizedName = FrozenReferenceCanonicalization.NormalizeIdentity(corpusName);
        var normalizedVersion = FrozenReferenceCanonicalization.NormalizeIdentity(corpusVersion);
        if (!normalizedName.IsSuccess || normalizedName.Value is null)
            return normalizedName.MapError<FrozenReferenceCorpus>();
        if (!normalizedVersion.IsSuccess || normalizedVersion.Value is null)
            return normalizedVersion.MapError<FrozenReferenceCorpus>();
        if (!FrozenReferenceCanonicalization.IsUtc(frozenAtUtc))
        {
            return Failure(
                "Frozen corpus timestamp must be caller-supplied UTC.",
                "FROZEN_REFERENCE_CORPUS_FROZEN_AT_UTC_INVALID");
        }

        if (entries is null)
        {
            return Failure(
                "Frozen reference entries are required.",
                "FROZEN_REFERENCE_CORPUS_REQUIRED");
        }

        var snapshot = Snapshot(entries, maximumCount: 10_000);
        if (!snapshot.IsSuccess || snapshot.Value is null)
            return snapshot.MapError<FrozenReferenceCorpus>();
        if (snapshot.Value.Count == 0)
        {
            return Failure(
                "Frozen reference corpus must contain at least one entry.",
                "FROZEN_REFERENCE_CORPUS_EMPTY");
        }

        if (snapshot.Value.Select(static entry => entry.IsRawCodeDocumentation).Distinct().Count() != 1)
            return Failure("Frozen reference corpus versions cannot be mixed.", "FROZEN_REFERENCE_CORPUS_VERSION_MISMATCH");

        if (snapshot.Value.Select(static entry => entry.NativeCaptureProvenance is not null).Distinct().Count() != 1) // Raw native capture and known-model raw v2 are separate origin contracts.
            return Failure("Frozen reference corpus origins cannot be mixed.", "FROZEN_REFERENCE_CORPUS_ORIGIN_MISMATCH"); // No payload branch may silently reinterpret a different origin.

        var domains = snapshot.Value
            .Select(static entry => entry.DomainTag)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
        if (domains.Length != 1)
        {
            return Failure(
                "Frozen reference corpus must contain exactly one domain.",
                "FROZEN_REFERENCE_CORPUS_DOMAIN_MISMATCH");
        }

        var subjects = snapshot.Value
            .Select(static entry => entry.SubjectId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
        if (subjects.Length != 1)
        {
            return Failure(
                "Frozen reference corpus must contain exactly one subject.",
                "FROZEN_REFERENCE_CORPUS_SUBJECT_MISMATCH");
        }

        var duplicateId = snapshot.Value
            .GroupBy(static entry => entry.EntryId, StringComparer.Ordinal)
            .FirstOrDefault(static group => group.Count() > 1)
            ?.Key;
        if (duplicateId is not null)
        {
            return Failure(
                "Frozen reference entry ids must be unique.",
                "FROZEN_REFERENCE_ENTRY_ID_DUPLICATE");
        }

        var crossPartitionLeak = snapshot.Value
            .GroupBy(static entry => entry.SourceEntityFingerprint, StringComparer.Ordinal)
            .FirstOrDefault(static group =>
                group.Select(static entry => entry.Partition).Distinct().Count() > 1);
        if (crossPartitionLeak is not null)
        {
            // Разрез проверяется до hash: иначе canonical digest закрепил бы
            // утечку train→held-out как валидный immutable corpus.
            return Failure(
                "A source entity cannot appear in both train and held-out partitions.",
                "FROZEN_REFERENCE_SOURCE_CROSS_PARTITION_LEAKAGE");
        }

        var ordered = snapshot.Value
            .OrderBy(static entry => entry.EntryId, StringComparer.Ordinal)
            .ToArray();
        var canonical = new StringBuilder();
        FrozenReferenceCanonicalization.Append(canonical, "schema", ordered[0].NativeCaptureProvenance is not null
            ? "xiigen.frozen-native-capture-reference-corpus.v1" : ordered[0].IsRawCodeDocumentation
            ? "xiigen.frozen-reference-corpus.v2" : "xiigen.frozen-reference-corpus.v1");
        FrozenReferenceCanonicalization.Append(canonical, "corpus_name", normalizedName.Value);
        FrozenReferenceCanonicalization.Append(canonical, "corpus_version", normalizedVersion.Value);
        FrozenReferenceCanonicalization.Append(canonical, "frozen_at_utc", FrozenReferenceCanonicalization.FormatUtc(frozenAtUtc));
        FrozenReferenceCanonicalization.Append(canonical, "domain_tag", domains[0]);
        FrozenReferenceCanonicalization.Append(canonical, "subject_id", subjects[0]);
        FrozenReferenceCanonicalization.Append(canonical, "entry_count", ordered.Length.ToString(CultureInfo.InvariantCulture));
        for (var index = 0; index < ordered.Length; index++)
        {
            FrozenReferenceCanonicalization.Append(
                canonical,
                "entry." + index.ToString(CultureInfo.InvariantCulture) + ".id",
                ordered[index].EntryId);
            FrozenReferenceCanonicalization.Append(
                canonical,
                "entry." + index.ToString(CultureInfo.InvariantCulture) + ".body",
                ordered[index].CanonicalRepresentation);
        }

        var trainSources = DistinctSources(ordered, FrozenReferencePartition.Train);
        var heldOutSources = DistinctSources(ordered, FrozenReferencePartition.HeldOut);
        var allSources = Array.AsReadOnly(
            trainSources.Concat(heldOutSources)
                .OrderBy(static value => value, StringComparer.Ordinal)
                .ToArray());
        var splitProof = new FrozenReferenceCorpusSplitProof(
            trainSources,
            heldOutSources,
            allSources,
            CrossPartitionSourceEntityOverlapCount: 0,
            Method: "exact-source-entity-fingerprint",
            OverlapThreshold: 1D);

        return OperationResult<FrozenReferenceCorpus>.Ok(
            new FrozenReferenceCorpus(
                normalizedName.Value,
                normalizedVersion.Value,
                frozenAtUtc,
                domains[0],
                subjects[0],
                Array.AsReadOnly(ordered),
                Array.AsReadOnly(ordered.Select(static entry => entry.EntryId).ToArray()),
                FrozenReferenceCanonicalization.Sha256Ref(canonical.ToString()),
                canonical.ToString(),
                splitProof));
    }

    public FrozenReferenceEntry? FindEntry(string referenceEntryId)
        => Entries.FirstOrDefault(entry =>
            StringComparer.Ordinal.Equals(entry.EntryId, referenceEntryId));

    private static ReadOnlyCollection<string> DistinctSources(
        IReadOnlyList<FrozenReferenceEntry> entries,
        FrozenReferencePartition partition)
        => Array.AsReadOnly(
            entries.Where(entry => entry.Partition == partition)
                .Select(static entry => entry.SourceEntityFingerprint)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(static value => value, StringComparer.Ordinal)
                .ToArray());

    private static OperationResult<IReadOnlyList<FrozenReferenceEntry>> Snapshot(
        IEnumerable<FrozenReferenceEntry?> entries,
        int maximumCount)
    {
        var snapshot = new List<FrozenReferenceEntry>();
        try
        {
            using var enumerator = entries.GetEnumerator();
            while (snapshot.Count <= maximumCount && enumerator.MoveNext())
            {
                if (enumerator.Current is null)
                {
                    return FailureList(
                        "Frozen reference entries cannot contain null rows.",
                        "FROZEN_REFERENCE_ENTRY_REQUIRED");
                }

                snapshot.Add(enumerator.Current);
            }
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // Cancellation is caller-owned control flow; converting it into an
            // OperationResult would make shutdown look like corpus validation.
            return FailureList(
                "Frozen reference entries could not be snapshotted.",
                "FROZEN_REFERENCE_CORPUS_SNAPSHOT_FAILED");
        }

        if (snapshot.Count > maximumCount)
        {
            return FailureList(
                "Frozen reference corpus exceeds its bounded capacity.",
                "FROZEN_REFERENCE_CORPUS_TOO_LARGE");
        }

        return OperationResult<IReadOnlyList<FrozenReferenceEntry>>.Ok(
            Array.AsReadOnly(snapshot.ToArray()));
    }

    private static OperationResult<IReadOnlyList<FrozenReferenceEntry>> FailureList(
        string message,
        string code)
        => OperationResult<IReadOnlyList<FrozenReferenceEntry>>.Fail(message, code);

    private static OperationResult<FrozenReferenceCorpus> Failure(string message, string code)
        => OperationResult<FrozenReferenceCorpus>.Fail(message, code);
}

/// <summary>
/// Человеческая и машинная личность правила сопоставления ответа ученика с
/// эталоном. Это прибор измерения, а не обучаемый DPO-судья.
/// </summary>
public sealed record ReferenceMatchingRuleIdentity
{
    private ReferenceMatchingRuleIdentity(
        string domainTag,
        string ruleId,
        string ruleVersion,
        string humanDescription,
        string canonicalRepresentation,
        string identitySha256)
    {
        DomainTag = domainTag;
        RuleId = ruleId;
        RuleVersion = ruleVersion;
        HumanDescription = humanDescription;
        CanonicalRepresentation = canonicalRepresentation;
        IdentitySha256 = identitySha256;
    }

    public string DomainTag { get; }

    public string RuleId { get; }

    public string RuleVersion { get; }

    public string HumanDescription { get; }

    public string CanonicalRepresentation { get; }

    public string IdentitySha256 { get; }

    public static OperationResult<ReferenceMatchingRuleIdentity> Create(
        string domainTag,
        string ruleId,
        string ruleVersion,
        string humanDescription)
    {
        var normalizedDomain = FrozenReferenceCanonicalization.NormalizeIdentity(domainTag);
        var normalizedRule = FrozenReferenceCanonicalization.NormalizeIdentity(ruleId);
        var normalizedVersion = FrozenReferenceCanonicalization.NormalizeIdentity(ruleVersion);
        var normalizedDescription = FrozenReferenceCanonicalization.NormalizeHumanDescription(humanDescription);
        if (!normalizedDomain.IsSuccess || normalizedDomain.Value is null)
            return normalizedDomain.MapError<ReferenceMatchingRuleIdentity>();
        if (!normalizedRule.IsSuccess || normalizedRule.Value is null)
            return normalizedRule.MapError<ReferenceMatchingRuleIdentity>();
        if (!normalizedVersion.IsSuccess || normalizedVersion.Value is null)
            return normalizedVersion.MapError<ReferenceMatchingRuleIdentity>();
        if (!normalizedDescription.IsSuccess || normalizedDescription.Value is null)
            return normalizedDescription.MapError<ReferenceMatchingRuleIdentity>();

        var canonical = new StringBuilder();
        FrozenReferenceCanonicalization.Append(canonical, "schema", "xiigen.reference-matching-rule-identity.v1");
        FrozenReferenceCanonicalization.Append(canonical, "domain_tag", normalizedDomain.Value);
        FrozenReferenceCanonicalization.Append(canonical, "rule_id", normalizedRule.Value);
        FrozenReferenceCanonicalization.Append(canonical, "rule_version", normalizedVersion.Value);
        FrozenReferenceCanonicalization.Append(canonical, "human_description", normalizedDescription.Value);
        var canonicalRepresentation = canonical.ToString();

        return OperationResult<ReferenceMatchingRuleIdentity>.Ok(
            new ReferenceMatchingRuleIdentity(
                normalizedDomain.Value,
                normalizedRule.Value,
                normalizedVersion.Value,
                normalizedDescription.Value,
                canonicalRepresentation,
                FrozenReferenceCanonicalization.Sha256Ref(canonicalRepresentation)));
    }
}

/// <summary>
/// Одна ручно известная calibration-пара: она обязана ссылаться на entry того
/// же frozen corpus, чтобы калибровка не подписывала чужие reference-байты.
/// </summary>
public sealed record ReferenceMatchingCalibrationPair
{
    private ReferenceMatchingCalibrationPair(
        string pairId,
        string referenceEntryId,
        string candidateAnswer,
        string referenceAnswer,
        string referenceAnswerSha256,
        string canonicalRepresentation,
        string pairSha256)
    {
        PairId = pairId;
        ReferenceEntryId = referenceEntryId;
        CandidateAnswer = candidateAnswer;
        ReferenceAnswer = referenceAnswer;
        ReferenceAnswerSha256 = referenceAnswerSha256;
        CanonicalRepresentation = canonicalRepresentation;
        PairSha256 = pairSha256;
    }

    public string PairId { get; }

    public string ReferenceEntryId { get; }

    public string CandidateAnswer { get; }

    public string ReferenceAnswer { get; }

    public string ReferenceAnswerSha256 { get; }

    public string CanonicalRepresentation { get; }

    public string PairSha256 { get; }

    public static OperationResult<ReferenceMatchingCalibrationPair> Create(
        string pairId,
        string referenceEntryId,
        string candidateAnswer,
        string referenceAnswer)
    {
        var normalizedId = FrozenReferenceCanonicalization.NormalizeIdentity(pairId);
        var normalizedEntryId = FrozenReferenceCanonicalization.NormalizeHashIdentity(referenceEntryId);
        var normalizedCandidate = FrozenReferenceCanonicalization.NormalizeContent(candidateAnswer);
        var normalizedReference = FrozenReferenceCanonicalization.NormalizeContent(referenceAnswer);
        if (!normalizedId.IsSuccess || normalizedId.Value is null)
            return normalizedId.MapError<ReferenceMatchingCalibrationPair>();
        if (!normalizedEntryId.IsSuccess || normalizedEntryId.Value is null)
            return normalizedEntryId.MapError<ReferenceMatchingCalibrationPair>();
        if (!normalizedCandidate.IsSuccess || normalizedCandidate.Value is null)
            return normalizedCandidate.MapError<ReferenceMatchingCalibrationPair>();
        if (!normalizedReference.IsSuccess || normalizedReference.Value is null)
            return normalizedReference.MapError<ReferenceMatchingCalibrationPair>();
        if (string.IsNullOrWhiteSpace(normalizedCandidate.Value) ||
            string.IsNullOrWhiteSpace(normalizedReference.Value))
        {
            return Failure(
                "Calibration candidate and reference answers are required.",
                "REFERENCE_MATCHING_CALIBRATION_PAIR_CONTENT_REQUIRED");
        }

        var referenceHash =
            FrozenReferenceCanonicalization.Sha256Ref(normalizedReference.Value);
        var canonical = new StringBuilder();
        FrozenReferenceCanonicalization.Append(canonical, "schema", "xiigen.reference-matching-calibration-pair.v1");
        FrozenReferenceCanonicalization.Append(canonical, "pair_id", normalizedId.Value);
        FrozenReferenceCanonicalization.Append(canonical, "reference_entry_id", normalizedEntryId.Value);
        FrozenReferenceCanonicalization.Append(canonical, "candidate_answer", normalizedCandidate.Value);
        FrozenReferenceCanonicalization.Append(canonical, "reference_answer", normalizedReference.Value);
        FrozenReferenceCanonicalization.Append(canonical, "reference_answer_sha256", referenceHash);
        var canonicalRepresentation = canonical.ToString();
        return OperationResult<ReferenceMatchingCalibrationPair>.Ok(
            new ReferenceMatchingCalibrationPair(
                normalizedId.Value,
                normalizedEntryId.Value,
                normalizedCandidate.Value,
                normalizedReference.Value,
                referenceHash,
                canonicalRepresentation,
                FrozenReferenceCanonicalization.Sha256Ref(canonicalRepresentation)));
    }

    private static OperationResult<ReferenceMatchingCalibrationPair> Failure(
        string message,
        string code)
        => OperationResult<ReferenceMatchingCalibrationPair>.Fail(message, code);
}

/// <summary>
/// Вход калибровки с двумя явными наборами: заведомо верные пары для false miss
/// и заведомо чужие пары для false hit.
/// </summary>
public sealed record ReferenceMatchingCalibrationInput
{
    private ReferenceMatchingCalibrationInput(
        FrozenReferenceCorpus corpus,
        ReferenceMatchingRuleIdentity ruleIdentity,
        IReadOnlyList<ReferenceMatchingCalibrationPair> knownPositivePairs,
        IReadOnlyList<ReferenceMatchingCalibrationPair> knownNegativePairs,
        string canonicalRepresentation,
        string calibrationSetSha256)
    {
        Corpus = corpus;
        RuleIdentity = ruleIdentity;
        KnownPositivePairs = knownPositivePairs;
        KnownNegativePairs = knownNegativePairs;
        CanonicalRepresentation = canonicalRepresentation;
        CalibrationSetSha256 = calibrationSetSha256;
    }

    public FrozenReferenceCorpus Corpus { get; }

    public ReferenceMatchingRuleIdentity RuleIdentity { get; }

    public IReadOnlyList<ReferenceMatchingCalibrationPair> KnownPositivePairs { get; }

    public IReadOnlyList<ReferenceMatchingCalibrationPair> KnownNegativePairs { get; }

    public string CanonicalRepresentation { get; }

    public string CalibrationSetSha256 { get; }

    public static OperationResult<ReferenceMatchingCalibrationInput> Create(
        FrozenReferenceCorpus corpus,
        ReferenceMatchingRuleIdentity ruleIdentity,
        IEnumerable<ReferenceMatchingCalibrationPair?> knownPositivePairs,
        IEnumerable<ReferenceMatchingCalibrationPair?> knownNegativePairs)
    {
        if (corpus is null)
        {
            return Failure(
                "Frozen reference corpus is required for matching calibration.",
                "REFERENCE_MATCHING_CORPUS_REQUIRED");
        }

        if (ruleIdentity is null)
        {
            return Failure(
                "Reference matching rule identity is required.",
                "REFERENCE_MATCHING_RULE_REQUIRED");
        }

        if (!StringComparer.Ordinal.Equals(corpus.DomainTag, ruleIdentity.DomainTag))
        {
            return Failure(
                "Reference matching rule domain must match the frozen corpus domain.",
                "REFERENCE_MATCHING_RULE_CORPUS_DOMAIN_MISMATCH");
        }

        var positives = SnapshotPairs(
            knownPositivePairs,
            "REFERENCE_MATCHING_POSITIVE_PAIRS_REQUIRED");
        if (!positives.IsSuccess || positives.Value is null)
            return positives.MapError<ReferenceMatchingCalibrationInput>();
        var negatives = SnapshotPairs(
            knownNegativePairs,
            "REFERENCE_MATCHING_NEGATIVE_PAIRS_REQUIRED");
        if (!negatives.IsSuccess || negatives.Value is null)
            return negatives.MapError<ReferenceMatchingCalibrationInput>();
        if (positives.Value.Count == 0)
        {
            return Failure(
                "Known-positive calibration denominator must be greater than zero.",
                "REFERENCE_MATCHING_POSITIVE_DENOMINATOR_INVALID");
        }

        if (negatives.Value.Count == 0)
        {
            return Failure(
                "Known-negative calibration denominator must be greater than zero.",
                "REFERENCE_MATCHING_NEGATIVE_DENOMINATOR_INVALID");
        }

        var allPairs = positives.Value.Concat(negatives.Value).ToArray();
        var duplicatePairId = allPairs
            .GroupBy(static pair => pair.PairId, StringComparer.Ordinal)
            .FirstOrDefault(static group => group.Count() > 1)
            ?.Key;
        if (duplicatePairId is not null)
        {
            return Failure(
                "Calibration pair ids must be unique across positive and negative sets.",
                "REFERENCE_MATCHING_CALIBRATION_PAIR_ID_DUPLICATE");
        }

        foreach (var pair in allPairs)
        {
            var entry = corpus.FindEntry(pair.ReferenceEntryId);
            if (entry is null)
            {
                return Failure(
                    "Calibration pair references an entry outside the frozen corpus.",
                    "REFERENCE_MATCHING_CALIBRATION_ENTRY_UNKNOWN");
            }

            if (entry.PaidReferenceProvenance is null || !StringComparer.Ordinal.Equals( // This generic legacy calibration contract supports normalized v1 pairs only.
                    pair.ReferenceAnswerSha256,
                    entry.PaidReferenceProvenance.ReferenceAnswerSha256))
            {
                // Pair text is rebound to the exact frozen entry hash before
                // calibration; otherwise one corpus hash could bless arbitrary answers.
                return Failure(
                    "Calibration reference answer does not match the frozen entry.",
                    "REFERENCE_MATCHING_CALIBRATION_REFERENCE_MISMATCH");
            }
        }

        var orderedPositives = positives.Value
            .OrderBy(static pair => pair.PairId, StringComparer.Ordinal)
            .ToArray();
        var orderedNegatives = negatives.Value
            .OrderBy(static pair => pair.PairId, StringComparer.Ordinal)
            .ToArray();
        var canonical = new StringBuilder();
        FrozenReferenceCanonicalization.Append(canonical, "schema", "xiigen.reference-matching-calibration-input.v1");
        FrozenReferenceCanonicalization.Append(canonical, "corpus_sha256", corpus.CorpusSha256);
        FrozenReferenceCanonicalization.Append(canonical, "corpus_version", corpus.CorpusVersion);
        FrozenReferenceCanonicalization.Append(canonical, "corpus_frozen_at_utc", FrozenReferenceCanonicalization.FormatUtc(corpus.FrozenAtUtc));
        FrozenReferenceCanonicalization.Append(canonical, "corpus_entry_count", corpus.Count.ToString(CultureInfo.InvariantCulture));
        FrozenReferenceCanonicalization.Append(canonical, "rule_identity_sha256", ruleIdentity.IdentitySha256);
        FrozenReferenceCanonicalization.Append(canonical, "known_positive_count", orderedPositives.Length.ToString(CultureInfo.InvariantCulture));
        AppendCalibrationPairs(canonical, "known_positive", orderedPositives);
        FrozenReferenceCanonicalization.Append(canonical, "known_negative_count", orderedNegatives.Length.ToString(CultureInfo.InvariantCulture));
        AppendCalibrationPairs(canonical, "known_negative", orderedNegatives);
        var canonicalRepresentation = canonical.ToString();

        return OperationResult<ReferenceMatchingCalibrationInput>.Ok(
            new ReferenceMatchingCalibrationInput(
                corpus,
                ruleIdentity,
                Array.AsReadOnly(orderedPositives),
                Array.AsReadOnly(orderedNegatives),
                canonicalRepresentation,
                FrozenReferenceCanonicalization.Sha256Ref(canonicalRepresentation)));
    }

    private static void AppendCalibrationPairs(
        StringBuilder canonical,
        string prefix,
        IReadOnlyList<ReferenceMatchingCalibrationPair> pairs)
    {
        for (var index = 0; index < pairs.Count; index++)
        {
            FrozenReferenceCanonicalization.Append(
                canonical,
                prefix + "." + index.ToString(CultureInfo.InvariantCulture) + ".id",
                pairs[index].PairId);
            FrozenReferenceCanonicalization.Append(
                canonical,
                prefix + "." + index.ToString(CultureInfo.InvariantCulture) + ".body",
                pairs[index].CanonicalRepresentation);
        }
    }

    private static OperationResult<IReadOnlyList<ReferenceMatchingCalibrationPair>> SnapshotPairs(
        IEnumerable<ReferenceMatchingCalibrationPair?> pairs,
        string requiredCode)
    {
        if (pairs is null)
        {
            return OperationResult<IReadOnlyList<ReferenceMatchingCalibrationPair>>.Fail(
                "Calibration pairs are required.",
                requiredCode);
        }

        var snapshot = new List<ReferenceMatchingCalibrationPair>();
        try
        {
            using var enumerator = pairs.GetEnumerator();
            while (snapshot.Count <= 10_000 && enumerator.MoveNext())
            {
                if (enumerator.Current is null)
                {
                    return OperationResult<IReadOnlyList<ReferenceMatchingCalibrationPair>>.Fail(
                        "Calibration pairs cannot contain null rows.",
                        "REFERENCE_MATCHING_CALIBRATION_PAIR_REQUIRED");
                }

                snapshot.Add(enumerator.Current);
            }
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // Cancellation/fatal nodes inside iterator exception graphs must keep
            // their original flow-control meaning instead of becoming bad input.
            return OperationResult<IReadOnlyList<ReferenceMatchingCalibrationPair>>.Fail(
                "Calibration pairs could not be snapshotted.",
                "REFERENCE_MATCHING_CALIBRATION_SNAPSHOT_FAILED");
        }

        if (snapshot.Count > 10_000)
        {
            return OperationResult<IReadOnlyList<ReferenceMatchingCalibrationPair>>.Fail(
                "Calibration pairs exceed their bounded capacity.",
                "REFERENCE_MATCHING_CALIBRATION_TOO_LARGE");
        }

        return OperationResult<IReadOnlyList<ReferenceMatchingCalibrationPair>>.Ok(
            Array.AsReadOnly(snapshot.ToArray()));
    }

    private static OperationResult<ReferenceMatchingCalibrationInput> Failure(
        string message,
        string code)
        => OperationResult<ReferenceMatchingCalibrationInput>.Fail(message, code);
}

public delegate bool ReferenceMatchingRulePredicate(
    string candidateAnswer,
    string referenceAnswer);

public sealed record ReferenceMatchingRuleCalibrationResult(
    ReferenceMatchingRuleIdentity RuleIdentity,
    string CorpusSha256,
    int CorpusEntryCount,
    string CalibrationSetSha256,
    int KnownPositiveCount,
    int FalseMissCount,
    double FalseMissRate,
    IReadOnlyList<string> FalseMissPairIds,
    int KnownNegativeCount,
    int FalseHitCount,
    double FalseHitRate,
    IReadOnlyList<string> FalseHitPairIds,
    string ResultSha256);

public static class ReferenceMatchingRuleCalibrationEvaluator
{
    public static OperationResult<ReferenceMatchingRuleCalibrationResult> Evaluate(
        ReferenceMatchingCalibrationInput input,
        ReferenceMatchingRulePredicate matchingRule)
    {
        if (input is null)
        {
            return Failure(
                "Reference matching calibration input is required.",
                "REFERENCE_MATCHING_CALIBRATION_INPUT_REQUIRED");
        }

        if (matchingRule is null)
        {
            return Failure(
                "Reference matching rule predicate is required.",
                "REFERENCE_MATCHING_RULE_PREDICATE_REQUIRED");
        }

        var falseMisses = new List<string>();
        var falseHits = new List<string>();
        try
        {
            foreach (var pair in input.KnownPositivePairs)
            {
                if (!matchingRule(pair.CandidateAnswer, pair.ReferenceAnswer))
                    falseMisses.Add(pair.PairId);
            }

            foreach (var pair in input.KnownNegativePairs)
            {
                if (matchingRule(pair.CandidateAnswer, pair.ReferenceAnswer))
                    falseHits.Add(pair.PairId);
            }
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // Predicate cancellation belongs to the caller's orchestration path;
            // swallowing it here would record an artificial failed calibration.
            return Failure(
                "Reference matching rule predicate failed during calibration.",
                "REFERENCE_MATCHING_RULE_PREDICATE_FAILED");
        }

        var positiveDenominator = input.KnownPositivePairs.Count;
        var negativeDenominator = input.KnownNegativePairs.Count;
        if (positiveDenominator <= 0 || negativeDenominator <= 0)
        {
            return Failure(
                "Reference matching calibration denominators must be greater than zero.",
                "REFERENCE_MATCHING_DENOMINATOR_INVALID");
        }

        var orderedFalseMisses = Array.AsReadOnly(
            falseMisses.OrderBy(static value => value, StringComparer.Ordinal).ToArray());
        var orderedFalseHits = Array.AsReadOnly(
            falseHits.OrderBy(static value => value, StringComparer.Ordinal).ToArray());
        var orderedKnownPositiveIds = PairIds(input.KnownPositivePairs);
        var orderedKnownNegativeIds = PairIds(input.KnownNegativePairs);
        var falseMissRate = (double)orderedFalseMisses.Count / positiveDenominator;
        var falseHitRate = (double)orderedFalseHits.Count / negativeDenominator;
        var canonical = new StringBuilder();
        FrozenReferenceCanonicalization.Append(canonical, "schema", "xiigen.reference-matching-calibration-result.v2");
        FrozenReferenceCanonicalization.Append(canonical, "corpus_sha256", input.Corpus.CorpusSha256);
        FrozenReferenceCanonicalization.Append(canonical, "corpus_entry_count", input.Corpus.Count.ToString(CultureInfo.InvariantCulture));
        FrozenReferenceCanonicalization.Append(canonical, "rule_identity_sha256", input.RuleIdentity.IdentitySha256);
        FrozenReferenceCanonicalization.Append(canonical, "calibration_set_sha256", input.CalibrationSetSha256);
        FrozenReferenceCanonicalization.Append(canonical, "known_positive_count", positiveDenominator.ToString(CultureInfo.InvariantCulture));
        AppendIds(canonical, "known_positive", orderedKnownPositiveIds);
        FrozenReferenceCanonicalization.Append(canonical, "false_miss_count", orderedFalseMisses.Count.ToString(CultureInfo.InvariantCulture));
        FrozenReferenceCanonicalization.Append(canonical, "false_miss_rate", falseMissRate.ToString("R", CultureInfo.InvariantCulture));
        AppendIds(canonical, "false_miss", orderedFalseMisses);
        FrozenReferenceCanonicalization.Append(canonical, "known_negative_count", negativeDenominator.ToString(CultureInfo.InvariantCulture));
        AppendIds(canonical, "known_negative", orderedKnownNegativeIds);
        FrozenReferenceCanonicalization.Append(canonical, "false_hit_count", orderedFalseHits.Count.ToString(CultureInfo.InvariantCulture));
        FrozenReferenceCanonicalization.Append(canonical, "false_hit_rate", falseHitRate.ToString("R", CultureInfo.InvariantCulture));
        AppendIds(canonical, "false_hit", orderedFalseHits);
        var resultSha256 = FrozenReferenceCanonicalization.Sha256Ref(canonical.ToString());

        // False miss и false hit не сливаются: строгий прибор и слишком мягкий
        // прибор ломают разные будущие решения о качестве ученика.
        return OperationResult<ReferenceMatchingRuleCalibrationResult>.Ok(
            new ReferenceMatchingRuleCalibrationResult(
                input.RuleIdentity,
                input.Corpus.CorpusSha256,
                input.Corpus.Count,
                input.CalibrationSetSha256,
                positiveDenominator,
                orderedFalseMisses.Count,
                falseMissRate,
                orderedFalseMisses,
                negativeDenominator,
                orderedFalseHits.Count,
                falseHitRate,
                orderedFalseHits,
                resultSha256));
    }

    private static void AppendIds(
        StringBuilder canonical,
        string prefix,
        IReadOnlyList<string> ids)
    {
        FrozenReferenceCanonicalization.Append(
            canonical,
            prefix + ".count",
            ids.Count.ToString(CultureInfo.InvariantCulture));
        for (var index = 0; index < ids.Count; index++)
        {
            FrozenReferenceCanonicalization.Append(
                canonical,
                prefix + "." + index.ToString(CultureInfo.InvariantCulture),
                ids[index]);
        }
    }

    private static IReadOnlyList<string> PairIds(
        IReadOnlyList<ReferenceMatchingCalibrationPair> pairs)
        => Array.AsReadOnly(
            pairs.Select(static pair => pair.PairId)
                .OrderBy(static value => value, StringComparer.Ordinal)
                .ToArray());

    private static OperationResult<ReferenceMatchingRuleCalibrationResult> Failure(
        string message,
        string code)
        => OperationResult<ReferenceMatchingRuleCalibrationResult>.Fail(message, code);
}

internal static class FrozenReferenceCanonicalization
{
    private const int MaximumIdentityChars = 256;
    private const int MaximumContentChars = 65_536;
    private const int MaximumHumanDescriptionChars = 4_096;

    internal static bool IsCanonicalSha256Ref(string? value)
        => value is { Length: 71 } &&
           value.StartsWith("sha256:", StringComparison.Ordinal) &&
           value.AsSpan("sha256:".Length).IndexOfAnyExcept("0123456789abcdef") < 0;

    internal static bool IsUtc(DateTimeOffset value)
        => value.Offset == TimeSpan.Zero && value != default;

    internal static string FormatUtc(DateTimeOffset value)
        => value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    internal static string Sha256Ref(string normalizedValue)
        => "sha256:" + Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(normalizedValue)))
            .ToLowerInvariant();

    internal static OperationResult<string> NormalizeIdentity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return OperationResult<string>.Fail(
                "Frozen reference identity is required.",
                "FROZEN_REFERENCE_IDENTITY_INVALID");
        }

        string normalized;
        try
        {
            normalized = value.Normalize(NormalizationForm.FormC);
        }
        catch (ArgumentException)
        {
            return OperationResult<string>.Fail(
                "Frozen reference identity must be valid Unicode.",
                "FROZEN_REFERENCE_IDENTITY_INVALID");
        }

        if (!StringComparer.Ordinal.Equals(normalized, normalized.Trim()) ||
            normalized.Length > MaximumIdentityChars ||
            !char.IsAsciiLetterOrDigit(normalized[0]) ||
            normalized.Any(static character =>
                !char.IsAsciiLetterOrDigit(character) &&
                character is not ('-' or '_' or '.' or ':')))
        {
            return OperationResult<string>.Fail(
                "Frozen reference identity is blank or unsafe.",
                "FROZEN_REFERENCE_IDENTITY_INVALID");
        }

        return OperationResult<string>.Ok(normalized);
    }

    internal static OperationResult<string> NormalizeHashIdentity(string? value)
    {
        if (!IsCanonicalSha256Ref(value))
        {
            return OperationResult<string>.Fail(
                "Frozen reference hash identity must be canonical.",
                "FROZEN_REFERENCE_HASH_IDENTITY_INVALID");
        }

        return OperationResult<string>.Ok(value!);
    }

    internal static OperationResult<string> NormalizeHumanDescription(string? value)
    {
        var normalized = NormalizeContent(value);
        if (!normalized.IsSuccess || normalized.Value is null)
            return normalized;
        var trimmed = normalized.Value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) ||
            trimmed.Length > MaximumHumanDescriptionChars)
        {
            return OperationResult<string>.Fail(
                "Reference matching rule must carry a bounded human description.",
                "REFERENCE_MATCHING_RULE_DESCRIPTION_INVALID");
        }

        return OperationResult<string>.Ok(trimmed);
    }

    internal static OperationResult<string> NormalizeContent(string? value)
    {
        if (value is null)
        {
            return OperationResult<string>.Fail(
                "Frozen reference content is required.",
                "FROZEN_REFERENCE_CONTENT_REQUIRED");
        }

        string normalized;
        try
        {
            normalized = value
                .Normalize(NormalizationForm.FormC)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n');
        }
        catch (ArgumentException)
        {
            return OperationResult<string>.Fail(
                "Frozen reference content must be valid Unicode.",
                "FROZEN_REFERENCE_CONTENT_INVALID");
        }

        if (normalized.Length > MaximumContentChars ||
            normalized.Any(static character =>
                char.IsControl(character) && character is not ('\n' or '\t')))
        {
            return OperationResult<string>.Fail(
                "Frozen reference content is unsafe or too large.",
                "FROZEN_REFERENCE_CONTENT_INVALID");
        }

        return OperationResult<string>.Ok(normalized);
    }

    internal static OperationResult<string> ValidateRawContent(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > MaximumContentChars ||
            value.Any(static character => char.IsControl(character) && character is not ('\r' or '\n' or '\t')))
            return OperationResult<string>.Fail("Raw frozen reference content is blank, unsafe or too large.", "FROZEN_REFERENCE_CONTENT_INVALID");
        try { _ = new UTF8Encoding(false, true).GetByteCount(value); }
        catch (EncoderFallbackException)
        {
            return OperationResult<string>.Fail("Raw frozen reference content must be valid UTF-8 text.", "FROZEN_REFERENCE_CONTENT_INVALID");
        }
        return OperationResult<string>.Ok(value); // Preserve CR/LF/TAB and decomposed Unicode exactly; identity normalization remains separate.
    }

    internal static string RawUtf8Sha256Ref(string value)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(new UTF8Encoding(false, true).GetBytes(value))).ToLowerInvariant();

    internal static void Append(StringBuilder canonical, string name, string? value)
    {
        // Length framing нужен, чтобы переносы строк и двоеточия внутри текста
        // не превращали две разные записи в одинаковую canonical строку.
        canonical.Append(name.Length.ToString(CultureInfo.InvariantCulture))
            .Append(':')
            .Append(name)
            .Append('=');
        if (value is null)
        {
            canonical.Append("N;");
            return;
        }

        canonical.Append('V')
            .Append(value.Length.ToString(CultureInfo.InvariantCulture))
            .Append(':')
            .Append(value)
            .Append(';');
    }
}
