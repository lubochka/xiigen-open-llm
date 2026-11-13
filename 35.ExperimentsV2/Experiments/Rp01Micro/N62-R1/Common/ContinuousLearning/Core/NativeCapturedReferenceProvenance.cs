// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning; // Keep descriptive closed-reference provenance with the existing corpus owner.

using System.Globalization; // Render usage counters in a culture-independent hash preimage.
using System.Text; // Frame every nullable capture field before hashing.
using AgentInfrastructure; // Return ordinary validation failures without issuing authority.

/// <summary>Описание операторского native capture. Запрошенный профиль не подтверждает backend identity.</summary>
public sealed record NativeReferenceCaptureMetadata( // Immutable descriptive input, never a provider attestation or a training grant.
    string RuntimeId, string RuntimeVersion, string CaptureBoundary, // Identify the actual local capture runtime, not a backend model version.
    string RequestedModelAlias, string RequestedReasoningEffort, // Retain the requested profile separately from returned metadata.
    string? ObservedProviderId, string? ObservedModelId, string? ObservedModelVersion, string? ObservedServerRequestId, // Null means absent from the observed capture.
    string ThreadId, string RequestRawUtf8Sha256, string ResponseRawUtf8Sha256, // Bind the captured request and full response file, separately from each reference answer.
    string StdoutSha256, string StderrSha256, string CaptureRecordSha256, // Bind all actual capture artifacts without embedding their private contents.
    DateTimeOffset StartedAtUtc, DateTimeOffset CapturedAtUtc, // Preserve caller-observed UTC timing; no clock or inference call occurs here.
    long? InputTokens, long? OutputTokens, long? ReasoningTokens); // Null usage stays unknown rather than becoming zero.

/// <summary>Закрытый эталон из native capture с неполным наблюдаемым паспортом; только для измерения.</summary>
public sealed record NativeCapturedReferenceProvenance // A separate origin variant leaves the known-model v1/v2 contract intact.
{ // Own immutable capture commitments and fixed restrictions through the factory below.
    private NativeCapturedReferenceProvenance(NativeReferenceCaptureMetadata capture, string rawHash, string canonical) // Construct only after bounded metadata and exact answer validation.
    { // Retain the full descriptive capture preimage rather than a requested-model identity surrogate.
        Capture = capture; ReferenceAnswerRawUtf8Sha256 = rawHash; CanonicalRepresentation = canonical; // Preserve the factory-owned metadata and exact reference bytes commitment.
        ProvenanceSha256 = FrozenReferenceCanonicalization.RawUtf8Sha256Ref(canonical); // SHA-256 binds nullable values and every capture field.
    } // This constructor cannot create provider, rights, admission or signature authority.

    public NativeReferenceCaptureMetadata Capture { get; } // Capture facts are supplied by a trusted scientific operator and are not independently attested here.
    public string ReferenceAnswerRawUtf8Sha256 { get; } // Exact per-entry UTF-8 reference hash preserves CRLF and decomposed Unicode.
    public string CanonicalRepresentation { get; } // Length-framed complete capture metadata and fixed restrictions.
    public string ProvenanceSha256 { get; } // Content identity, never proof that operator-supplied facts are true.
    public bool ClosedReference { get; } = true; // The new origin remains closed reference evidence.
    public bool ReferenceOutputNeverTrainable { get; } = true; // No capture field can change closed-reference training restrictions.
    public bool ReferenceOutputNeverDpoPairSide { get; } = true; // No capture field can authorize chosen/rejected use.

    public static OperationResult<NativeCapturedReferenceProvenance> Create( // Admit descriptive metadata and exact reference bytes through one fixed factory.
        NativeReferenceCaptureMetadata capture, string rawReferenceAnswer) // Neither parameter conveys rights or execution consent.
    { // Validate the entire preimage before returning an immutable closed-reference origin.
        if (capture is null) return Invalid(); // Missing capture cannot be replaced by an invented provider identity.
        var reference = FrozenReferenceCanonicalization.ValidateRawContent(rawReferenceAnswer); // Reject blank, malformed or oversized raw content without normalization.
        if (!reference.IsSuccess || reference.Value is null) return reference.MapError<NativeCapturedReferenceProvenance>(); // Preserve the existing raw-content failure.
        var required = new[] { capture.RuntimeId, capture.RuntimeVersion, capture.CaptureBoundary, capture.RequestedModelAlias, capture.RequestedReasoningEffort, capture.ThreadId }; // Keep requested and observed identities in distinct fields.
        var observed = new[] { capture.ObservedProviderId, capture.ObservedModelId, capture.ObservedModelVersion, capture.ObservedServerRequestId }; // Explicit nullable identities are hashed even when no provider fields were returned.
        if (required.Any(static value => !ValidIdentity(value)) || observed.Any(static value => value is not null && !ValidIdentity(value))) return Invalid(); // Empty strings and unsafe identity aliases are not unknown metadata.
        var hashes = new[] { capture.RequestRawUtf8Sha256, capture.ResponseRawUtf8Sha256, capture.StdoutSha256, capture.StderrSha256, capture.CaptureRecordSha256 }; // Bind all five actual request/response/capture artifacts.
        if (hashes.Any(static value => !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(value)) || // Require canonical prefixed SHA-256 values rather than arbitrary labels.
            !FrozenReferenceCanonicalization.IsUtc(capture.StartedAtUtc) || !FrozenReferenceCanonicalization.IsUtc(capture.CapturedAtUtc) || capture.CapturedAtUtc < capture.StartedAtUtc || // Reject non-UTC or reversed observed intervals.
            capture.InputTokens is < 0 || capture.OutputTokens is < 0 || capture.ReasoningTokens is < 0) return Invalid(); // Negative usage cannot become a successful capture record.
        var rawHash = FrozenReferenceCanonicalization.RawUtf8Sha256Ref(reference.Value); // Compute the exact entry answer commitment locally.
        var canonical = new StringBuilder(); // Length framing disambiguates null, empty and arbitrary delimiter-containing text.
        void Field(string name, string? value) => FrozenReferenceCanonicalization.Append(canonical, name, value); // Reuse the existing corpus canonical primitive.
        Field("schema", "xiigen.closed-native-capture-reference-provenance.v1"); // Explicit origin discriminator prevents reinterpretation as known-model raw v2.
        Field("runtime_id", capture.RuntimeId); Field("runtime_version", capture.RuntimeVersion); Field("capture_boundary", capture.CaptureBoundary); // Hash the observed runtime/capture boundary.
        Field("requested_model_alias", capture.RequestedModelAlias); Field("requested_reasoning_effort", capture.RequestedReasoningEffort); // Hash the requested profile without promoting it to a returned identity.
        Field("observed_provider_id", capture.ObservedProviderId); Field("observed_model_id", capture.ObservedModelId); // Include explicitly absent provider/model metadata in the preimage.
        Field("observed_model_version", capture.ObservedModelVersion); Field("observed_server_request_id", capture.ObservedServerRequestId); // Include absent backend version/request id without placeholders.
        Field("thread_id", capture.ThreadId); Field("request_raw_utf8_sha256", capture.RequestRawUtf8Sha256); Field("response_raw_utf8_sha256", capture.ResponseRawUtf8Sha256); // Distinguish actual local thread from server request identity.
        Field("stdout_sha256", capture.StdoutSha256); Field("stderr_sha256", capture.StderrSha256); Field("capture_record_sha256", capture.CaptureRecordSha256); // Commit all capture evidence without logging its contents.
        Field("started_at_utc", FrozenReferenceCanonicalization.FormatUtc(capture.StartedAtUtc)); Field("captured_at_utc", FrozenReferenceCanonicalization.FormatUtc(capture.CapturedAtUtc)); // Hash canonical UTC representations.
        Field("input_tokens", capture.InputTokens?.ToString(CultureInfo.InvariantCulture)); Field("output_tokens", capture.OutputTokens?.ToString(CultureInfo.InvariantCulture)); Field("reasoning_tokens", capture.ReasoningTokens?.ToString(CultureInfo.InvariantCulture)); // Preserve absent usage as null rather than deriving unobserved totals.
        Field("reference_answer_raw_utf8_sha256", rawHash); Field("closed_reference", "true"); // Bind the exact answer and closed-reference origin.
        Field("reference_output_never_trainable", "true"); Field("reference_output_never_dpo_pair_side", "true"); // Fixed restrictions are recreated rather than loaded from JSON switches.
        return OperationResult<NativeCapturedReferenceProvenance>.Ok(new(capture, rawHash, canonical.ToString())); // Return descriptive capture identity only.
    } // No provider/model, Teacher, Student, rights or optimizer is invoked.

    private static bool ValidIdentity(string? value) => FrozenReferenceCanonicalization.NormalizeIdentity(value).IsSuccess; // Retain the existing bounded safe identity grammar.
    private static OperationResult<NativeCapturedReferenceProvenance> Invalid() => OperationResult<NativeCapturedReferenceProvenance>.Fail( // Keep failure messages free of private capture values.
        "Native reference capture metadata is invalid.", "NATIVE_REFERENCE_CAPTURE_INVALID"); // A malformed or unknown capture has no successful provenance value.
} // Native-capture provenance is a deterministic descriptive boundary, not a learner.
