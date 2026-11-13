// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning; // Extend the same corpus owner rather than create another persistence path.

using System.Text.Json; // Keep native capture JSON strict, bounded and explicit about null fields.
using AgentInfrastructure; // Preserve the existing owner failure contract.

public static partial class RepositoryOwnedReferenceCorpusManifestSealer // Native capture shares custody, stable reads, no-overwrite sealing and whole-corpus reconstruction.
{ // Add only an explicit native schema/codec to the existing sealer.
    public const string NativeCaptureSchema = "repository-owned-native-reference-corpus-manifest-v1"; // Distinct manifest origin prevents v1/v2 reinterpretation.
    public const string NativeCaptureContext = "continuous-learning-frozen-native-capture-reference-corpus-v1"; // Descriptive context conveys no grant or backend attestation.
    public const string NativeCapturePayloadSchema = "repository-owned-native-reference-corpus-payload-v1"; // Distinct payload requires native provenance on every entry.
    private const string NativeCaptureProvenanceProperty = "nativeCaptureProvenance"; // Native payloads emit no paidReferenceProvenance identity surrogate.
    internal static readonly string[] CaptureFieldNames = [ // One exact complete metadata ABI shared by private intake and owner readback.
        "runtimeId", "runtimeVersion", "captureBoundary", "requestedModelAlias", "requestedReasoningEffort", // Actual runtime and requested profile are distinct facts.
        "observedProviderId", "observedModelId", "observedModelVersion", "observedServerRequestId", "threadId", // Nullable observed values never fall back to a request alias.
        "requestRawUtf8Sha256", "responseRawUtf8Sha256", "stdoutSha256", "stderrSha256", "captureRecordSha256", // Exact hashes bind private capture artifacts without embedding them.
        "startedAtUtc", "capturedAtUtc", "inputTokens", "outputTokens", "reasoningTokens"]; // UTC and actual nullable usage complete the preimage.

    /// <summary>
    /// Reopens only the explicitly native origin with the caller's expected seal pins.
    /// No clone preparation, security repair or model/rights grant occurs on this path.
    /// </summary>
    public static OperationResult<RepositoryOwnedReferenceCorpusOpenResult> OpenSealedNativeCapture( // Native-only callers require origin before any filesystem preparation.
        RepositoryOwnedReferenceCorpusAddress address, string expectedCorpusSha256, string expectedPayloadSha256, string expectedManifestSha256) // Manifest pin is its sealed self-hash; core separately binds actual raw manifest bytes.
    { // Never delegate to generic OpenSealed: a legacy replacement must fail before its preparation branch.
        var validation = ValidateAddress(address); // Admit only the existing typed repository address and no-reparse path.
        if (!validation.IsSuccess) return validation.MapError<RepositoryOwnedReferenceCorpusOpenResult>(); // Preserve ordinary content-free owner refusal.
        if (!FrozenReferenceCanonicalization.IsCanonicalSha256Ref(expectedCorpusSha256) || // Require the unchanged canonical whole-corpus identity grammar.
            !IsCanonicalSha256Hex(expectedPayloadSha256) || !IsCanonicalSha256Hex(expectedManifestSha256)) // Require both complete bare lowercase seal pins.
            return Invalid<RepositoryOwnedReferenceCorpusOpenResult>(); // Malformed expectations authorize no preparation or payload read.
        var discovery = ReadStrictManifest(address.ManifestPath); // Read bounded stable bytes and verify the complete manifest self-hash before selecting the native path.
        if (!discovery.IsSuccess || discovery.Value is null) return discovery.MapError<RepositoryOwnedReferenceCorpusOpenResult>(); // Missing/malformed headers fail without changing modes.
        var manifest = discovery.Value.Manifest; // Retain the actual header and its separately measured raw-file hash.
        if (!IsNativeManifest(manifest) || !ValidateManifestForAddress(address, manifest).IsSuccess || // Reject known/unknown/mixed origins and substituted address coordinates.
            manifest.CorpusSha256 != expectedCorpusSha256 || manifest.PayloadSha256 != expectedPayloadSha256 || manifest.ManifestSha256 != expectedManifestSha256) // Admit only the exact complete expected seal.
            return Invalid<RepositoryOwnedReferenceCorpusOpenResult>(); // A valid legacy header or changed native seal cannot fall back to general preparation.
        return ProjectOpenResult(OpenNativeCaptureWithoutPreparation(address, discovery.Value)); // Core rechecks raw manifest identity, private handles and whole canonical content without a preparer.
    } // These expected hashes bind content; they do not attest backend identity or grant assessment/training rights.

    private static bool IsNativeManifest(RepositoryOwnedReferenceCorpusManifest manifest) => // Require one explicit complete schema/context/domain tuple.
        manifest.Schema == NativeCaptureSchema && manifest.Context == NativeCaptureContext && manifest.Domain == FrozenReferenceEntry.RawCodeDocumentationDomain; // No origin inference from null legacy fields.

    private static OperationResult<RepositoryOwnedReferenceCorpusFileIdentityOpenResult> OpenNativeCaptureWithoutPreparation( // Native readback never calls the clone preparer or repairs existing modes.
        RepositoryOwnedReferenceCorpusAddress address, StrictManifestReadback discovery) // Use the complete manifest actually reconstructed from stable bounded bytes.
    { // Admit the full native header and address before entering its existing strict content reader.
        if (!IsNativeManifest(discovery.Manifest) || !ValidateManifestForAddress(address, discovery.Manifest).IsSuccess) // A native namespace member alone is insufficient to admit an origin.
            return Invalid<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>(); // Refuse mixed native/legacy tuples before any preparation or closed content read.
        return OpenSealedFileIdentityCore(address, discovery.ManifestFileSha256); // Pin actual raw manifest identity; core refuses origin/file changes and verifies private handles.
    } // Descriptive origin detection conveys no rights, grant or model execution authority.

    private static bool EnsureNativeCaptureCustodyRoot(RepositoryOwnedReferenceCorpusAddress address) // Create only the new native corpus directory under the admitted typed address.
    { // Existing roots must already have private custody; no foreign ACL or mode is repaired.
        if (!SecureLocalFileBoundary.IsSupportedPlatform() || AnyExistingPathSegmentIsReparsePoint(address.CorpusRoot)) return false; // Preserve unsupported/no-reparse refusal before any allocation.
        if (Directory.Exists(address.CorpusRoot)) return SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(address.CorpusRoot); // Refuse an unsafe existing root instead of changing its security.
        if (File.Exists(address.CorpusRoot)) return false; // A file cannot be converted into the private corpus directory.
        Directory.CreateDirectory(Path.GetDirectoryName(address.CorpusRoot)!); // Create only missing canonical address ancestors; no existing security is changed.
        if (AnyExistingPathSegmentIsReparsePoint(address.CorpusRoot)) return false; // Revalidate all actual path segments after bounded address allocation.
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(address.CorpusRoot); // Existing custody helper applies owner/SYSTEM/Admin ACL only on this newly allocated native root.
        return !AnyExistingPathSegmentIsReparsePoint(address.CorpusRoot) && SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(address.CorpusRoot); // Require actual private custody rather than pathname existence.
    } // Failed partial allocation is not a sealed corpus and does not authorize retry/overwrite of data.

    private static bool NativeCaptureFilesHaveCustody(RepositoryOwnedReferenceCorpusAddress address) => // Verify current native content custody before reading Gold.
        SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(address.CorpusRoot) && // Native corpus root must retain its private directory boundary.
        SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(address.ManifestPath) && // Require the stored manifest's actual strict private ACL/mode.
        SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(PayloadPath(address)); // Require current payload custody before any closed body parser.

    private static FileStream OpenCorpusFile(bool nativeCapture, string path, FileMode mode, FileAccess access, FileShare share, FileOptions options, int bufferSize) => // Keep one native-only secure allocation seam in the same owner.
        nativeCapture ? SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, mode, access, share, options, bufferSize) // New closed native staging/read uses the existing strict private handle boundary.
            : SecureLocalFileBoundary.OpenOwnerOnlyFile(path, mode, access, share, options, bufferSize); // Both legacy schemas retain unchanged file open/create semantics.

    internal static bool TryReadNativeCapture(JsonElement element, out NativeReferenceCaptureMetadata? capture) // Parse descriptive operator facts without inferring missing metadata.
    { // Reject extra, duplicate, omitted, mistyped or oversized capture members.
        capture = null; // Failure never returns a usable partial capture.
        if (!ObjectHasExactProperties(element, CaptureFieldNames)) return false; // Every nullable field must be explicitly present, even when absent in the native stream.
        var text = new string?[17]; // The first seventeen ABI members are strings or canonical UTC text.
        for (var i = 0; i < text.Length; i++) // Read the complete ordered text ABI without silently dropping fields.
        { // Only the four observed backend identity members may be null.
            var value = element.GetProperty(CaptureFieldNames[i]); // Exact-property validation guarantees one occurrence.
            if (i is >= 5 and <= 8 && value.ValueKind == JsonValueKind.Null) continue; // Preserve absent returned identity as actual null.
            if (value.ValueKind != JsonValueKind.String || (text[i] = value.GetString()) is null || text[i]!.Length > MaximumIdentityTextLength) return false; // Bound every non-null metadata string.
        } // Missing returned metadata does not become an empty/UNAVAILABLE passport.
        if (!TryParseCanonicalUtc(text[15]!, out var start) || !TryParseCanonicalUtc(text[16]!, out var end)) return false; // Require owner-canonical UTC text.
        var usage = new long?[3]; // Unknown actual usage remains nullable.
        for (var i = 0; i < usage.Length; i++) // Parse all counters with exact Int64 semantics.
        { // Negative, fractional or overflowing numbers are not valid observed usage.
            var value = element.GetProperty(CaptureFieldNames[17 + i]); // Read one explicitly present usage member.
            if (value.ValueKind == JsonValueKind.Null) continue; // Do not invent zero usage when the stream omitted a counter.
            if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt64(out var number) || number < 0) return false; // Reject malformed actual counters.
            usage[i] = number; // Preserve the actual supplied count without deriving a total.
        } // All nullable counters participate in the provenance hash.
        capture = new(text[0]!, text[1]!, text[2]!, text[3]!, text[4]!, text[5], text[6], text[7], text[8], // Reconstruct distinct requested and observed identities.
            text[9]!, text[10]!, text[11]!, text[12]!, text[13]!, text[14]!, start, end, usage[0], usage[1], usage[2]); // Preserve thread, capture hashes, interval and actual usage.
        return true; // Factory validation still validates safe identity/hash grammars and the interval.
    } // A valid JSON shape is not proof of the operator's provenance assertions.

    private static void WriteNativeProvenance(Utf8JsonWriter writer, NativeCapturedReferenceProvenance provenance) // Emit the complete factory-owned native provenance in fixed order.
    { // No provider/model/request placeholders are emitted.
        writer.WriteStartObject(); writer.WriteStartObject("capture"); // Keep descriptive metadata in a separate nested object.
        var c = provenance.Capture; // Read the immutable factory-owned capture snapshot.
        var values = new string?[] { c.RuntimeId, c.RuntimeVersion, c.CaptureBoundary, c.RequestedModelAlias, c.RequestedReasoningEffort, // Preserve actual runtime and requested profile independently.
            c.ObservedProviderId, c.ObservedModelId, c.ObservedModelVersion, c.ObservedServerRequestId, c.ThreadId, // Utf8JsonWriter emits explicit null observed values.
            c.RequestRawUtf8Sha256, c.ResponseRawUtf8Sha256, c.StdoutSha256, c.StderrSha256, c.CaptureRecordSha256, // Bind the complete captured artifacts.
            FrozenReferenceCanonicalization.FormatUtc(c.StartedAtUtc), FrozenReferenceCanonicalization.FormatUtc(c.CapturedAtUtc) }; // Keep exact canonical UTC wire values.
        for (var i = 0; i < values.Length; i++) writer.WriteString(CaptureFieldNames[i], values[i]); // Emit every text/null capture member in ABI order.
        var usage = new long?[] { c.InputTokens, c.OutputTokens, c.ReasoningTokens }; // Preserve actual unknown counters separately from numeric zero.
        for (var i = 0; i < usage.Length; i++) // Emit the complete nullable usage tuple.
        { // Explicit JSON null is part of the canonical native payload.
            if (usage[i] is { } number) writer.WriteNumber(CaptureFieldNames[17 + i], number); // Emit an actual supplied integer.
            else writer.WriteNull(CaptureFieldNames[17 + i]); // Emit unknown rather than a fabricated numeric observation.
        } // No unobserved totals or backend metadata are derived.
        writer.WriteEndObject(); // Finish the full capture metadata object.
        writer.WriteString(ReferenceAnswerRawUtf8Sha256Property, provenance.ReferenceAnswerRawUtf8Sha256); // Bind exact per-entry answer bytes.
        writer.WriteString(ProvenanceSha256Property, provenance.ProvenanceSha256); // Preserve the hash of every capture field and fixed restriction.
        writer.WriteString(CanonicalRepresentationProperty, provenance.CanonicalRepresentation); writer.WriteEndObject(); // Preserve the complete preimage for strict reconstruction.
    } // Fixed closed-reference restrictions are recreated by the factory, never read from mutable flags.

    private static bool TryReadNativePayloadEntry(JsonElement element, out RepositoryOwnedReferenceCorpusPayloadEntry entry) // Admit the separate native entry schema and full origin.
    { // Share the legacy field bounds and strict property-set primitive.
        entry = EmptyEntry(); // Leave no partially reconstructed value on failure.
        string[] fields = [EntryIdProperty, OwnerInputProperty, PaidModelReferenceAnswerProperty, DomainProperty, SubjectProperty, PartitionProperty, SourceEntityFingerprintProperty, CanonicalRepresentationProperty]; // Preserve common owner fields with a separate provenance member.
        if (!ObjectHasExactProperties(element, [.. fields, NativeCaptureProvenanceProperty])) return false; // Reject mixed paid/native origin objects or unknown flags.
        var values = new string[fields.Length]; // Stage every entry field before construction.
        for (var i = 0; i < values.Length; i++) // Parse complete bounded raw content and identity fields.
        { // Apply the original owner limits by field purpose.
            var maximum = i is 1 or 2 ? MaximumContentTextLength : i == 7 ? MaximumCanonicalTextLength : MaximumIdentityTextLength; // Reuse existing content/canonical/identity ceilings.
            if (!TryReadString(element, fields[i], maximum, out values[i])) return false; // Reject absent or mistyped fields without normalization.
        } // The domain factory subsequently validates exact raw content and hash grammar.
        var p = element.GetProperty(NativeCaptureProvenanceProperty); // Read exactly the explicitly selected native origin object.
        if (!ObjectHasExactProperties(p, "capture", ReferenceAnswerRawUtf8Sha256Property, ProvenanceSha256Property, CanonicalRepresentationProperty) || // Reject a claimed grant/backend identity member not owned by this schema.
            !TryReadNativeCapture(p.GetProperty("capture"), out var capture) || capture is null || // Parse every nullable capture field before factory reconstruction.
            !TryReadString(p, ReferenceAnswerRawUtf8Sha256Property, 71, out var rawHash) || // Read the original exact-reference hash for comparison.
            !TryReadString(p, ProvenanceSha256Property, 71, out var provenanceHash) || // Read the full metadata commitment.
            !TryReadString(p, CanonicalRepresentationProperty, MaximumCanonicalTextLength, out var representation)) return false; // Read the whole preimage rather than accepting a digest alone.
        var rebuilt = NativeCapturedReferenceProvenance.Create(capture, values[2]); // Recreate fixed restrictions and the canonical nullable metadata preimage.
        if (!rebuilt.IsSuccess || rebuilt.Value is null || rebuilt.Value.ReferenceAnswerRawUtf8Sha256 != rawHash || // Refuse a changed raw reference even when outer bytes were resealed.
            rebuilt.Value.ProvenanceSha256 != provenanceHash || rebuilt.Value.CanonicalRepresentation != representation) return false; // Refuse metadata tampering behind stale canonical commitments.
        entry = new(values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], null, rebuilt.Value); // Keep legacy issuing-model passport absent.
        return true; // Entry and whole-corpus identities still must reconstruct and match the seal.
    } // Reading a syntactically valid native record creates no external rights or consent.

    private static OperationResult<FrozenReferenceEntry> ReconstructNativeEntry(RepositoryOwnedReferenceCorpusPayload payload, RepositoryOwnedReferenceCorpusPayloadEntry stored) // Bind full native entry to its explicit owner header.
    { // Reconstruct raw source/reference through the new origin-specific factory.
        if (stored.PaidReferenceProvenance is not null || stored.NativeCaptureProvenance is null || stored.Domain != payload.Domain || stored.Subject != payload.Subject || // Reject legacy/native mixtures and header substitution.
            !TryParsePartition(stored.Partition, out var partition)) return Invalid<FrozenReferenceEntry>(); // Preserve the existing split grammar.
        var entry = FrozenReferenceEntry.CreateNativeCapturedCodeDocumentation(stored.OwnerInput, stored.PaidModelReferenceAnswer, // Preserve exact owner/reference strings from the strict payload.
            stored.NativeCaptureProvenance, stored.Subject, partition, stored.SourceEntityFingerprint); // Bind full capture provenance and source family.
        return entry.IsSuccess && entry.Value is not null && entry.Value.EntryId == stored.EntryId && entry.Value.CanonicalRepresentation == stored.CanonicalRepresentation // Require the complete entry preimage and its derived identity to match.
            ? entry : Invalid<FrozenReferenceEntry>(); // Refuse altered native bytes/metadata before live assessment inputs exist.
    } // The existing owner then compares all ordered entries and complete corpus/manifest identities.
} // All filesystem and no-overwrite behavior remains in the existing owner.
