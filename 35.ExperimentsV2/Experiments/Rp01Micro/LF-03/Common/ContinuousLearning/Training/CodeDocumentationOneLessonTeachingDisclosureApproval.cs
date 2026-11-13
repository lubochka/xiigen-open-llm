// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
namespace ContinuousLearning; // Keep the approval declaration in the same assembly namespace as its protected catalog owner.

using System.Globalization; // Use invariant timestamp and integer formatting for the operator declaration grammar.
using System.Security.Cryptography; // Derive the approval evidence digest from its complete canonical bytes.
using System.Text; // Encode declaration and alias-check metadata with explicit UTF-8 bytes.
using System.Text.Json; // Read the closed JSON declaration without a caller-owned mutable DTO.
using AgentInfrastructure; // Return bounded owner refusals through the existing operation-result contract.

/// <summary>Immutable operator declaration. Only the retained protected startup can authorize its use.</summary>
internal sealed class CodeDocumentationOneLessonTeachingDisclosureApproval // Keep approval construction private to decoding the retained operator declaration.
{ // Retain declaration data without exporting a public authority factory.
    internal const string Unavailable = "CODE_DOCUMENTATION_ONE_LESSON_TEACHING_DISCLOSURE_APPROVAL_UNAVAILABLE"; // Distinguish absence of protected startup approval from a malformed declaration.
    internal const string Invalid = "CODE_DOCUMENTATION_ONE_LESSON_TEACHING_DISCLOSURE_APPROVAL_INVALID"; // Identify a declaration whose full approved graph no longer matches the actual owners.
    internal const string OwnerMismatch = "CODE_DOCUMENTATION_ONE_LESSON_TEACHING_DISCLOSURE_APPROVAL_OWNER_MISMATCH"; // Distinguish a foreign grant or approval object from this catalog's retained owner.
    internal const string Stale = "CODE_DOCUMENTATION_ONE_LESSON_TEACHING_DISCLOSURE_APPROVAL_STALE"; // Identify process revision changes or startup revocation after approval capture.
    private readonly JsonElement declaration; // Own a detached declaration so the input document cannot replace approved fields later.
    private readonly byte[] canonical; // Retain the full canonical approval preimage rather than only its digest.
    private readonly int schemaVersion; // Freeze the exact admitted reference commitment schema with the declaration.
    internal string GrantId { get; } // Expose the declared grant identity for exact retained catalog membership.
    internal string GrantVersion { get; } // Expose the declared grant version for the same identity/version membership key.
    internal string EvidenceSha256 { get; } // Report the digest of the full declaration as evidence, not as authority by itself.
    private CodeDocumentationOneLessonTeachingDisclosureApproval(JsonElement value) // Construct an immutable approval only after the closed decoder has admitted every field.
    { // Freeze the validated declaration before deriving any membership or evidence values.
        declaration = value.Clone(); canonical = CodeDocumentationOneLessonV2Wire.CanonicalTeachingDisclosureBytes(declaration); // Detach the JSON and serialize all approved fields with the restricted canonical writer.
        schemaVersion = declaration.GetProperty("SchemaVersion").GetInt32(); // Retain only the decoder's exact legacy1 or raw2 discriminator.
        GrantId = declaration.GetProperty("GrantId").GetString()!; // Retain the already-validated grant id used to locate the exact catalog owner.
        GrantVersion = declaration.GetProperty("GrantVersion").GetString()!; // Retain the already-validated grant version without a caller alias or fallback.
        EvidenceSha256 = Convert.ToHexString(SHA256.HashData(canonical)).ToLowerInvariant(); // Commit the complete canonical approval bytes with a lowercase SHA256 evidence value.
    } // Finish private approval capture without retaining the caller's document lifetime.
    internal byte[] CopyBytes() => canonical.ToArray(); // Return a defensive copy so callers cannot mutate the retained approval preimage.
    internal IEnumerable<string> ExposedMetadata() => Strings(declaration).Append(Encoding.UTF8.GetString(canonical)); // Send all names, scalars and canonical JSON through the existing ORIGINAL alias guard.
    private static IEnumerable<string> Strings(JsonElement value) // Traverse approval metadata recursively so nested unused entries also enter alias checking.
    { // Collect the complete scalar/name surface without treating a digest as a substitute.
        if (value.ValueKind == JsonValueKind.Object) foreach (var p in value.EnumerateObject()) { yield return p.Name; foreach (var s in Strings(p.Value)) yield return s; } // Include every object property name and recursively include its nested approved value.
        else if (value.ValueKind == JsonValueKind.Array) foreach (var v in value.EnumerateArray()) foreach (var s in Strings(v)) yield return s; // Include every array element in declaration order, including unused source rows.
        else yield return value.ValueKind == JsonValueKind.String ? value.GetString()! : value.GetRawText(); // Preserve string contents and canonical scalar spellings for ORIGINAL exclusion checking.
    } // Finish metadata traversal without dropping non-string declaration fields.
    internal static CodeDocumentationOneLessonTeachingDisclosureApproval? Decode(JsonElement value, CodeDocumentationOneLessonGrantV1 grant) // Decode only a complete bounded operator declaration for this already-retained grant.
    { // Reject the declaration before constructing any immutable approval on malformed input.
        if (!Fields(value, "SchemaVersion,ApprovalId,ApprovalRevision,GrantId,GrantVersion,ApproverId,ApprovedAtUtc,OperatorDecision,Use,Recipient,Model,ReasoningEffort,Evidence,SourceSelection,AllTeachingSources") || // Require exactly the declared approval fields so extra or missing authority claims fail.
            !(Integer(value.GetProperty("SchemaVersion"), 1, 1) || Integer(value.GetProperty("SchemaVersion"), 2, 2)) || !Integer(value.GetProperty("ApprovalRevision"), 1, int.MaxValue) || // Admit only exact legacy1 or raw2 and a positive canonical integer revision.
            !Text(value, "ApprovalId", 80) || !value.GetProperty("ApprovalId").GetString()!.All(c => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-' or '_') || // Bound the approval id and restrict its alphabet to the accepted identity grammar.
            !Text(value, "ApproverId", 256) || !EqualsText(value, "GrantId", grant.GrantId) || !EqualsText(value, "GrantVersion", grant.GrantVersion) || // Bound the approver identity and match this exact grant id and version.
            !EqualsText(value, "OperatorDecision", "PermitFullTeachingSourceDisclosure") || !EqualsText(value, "Use", "PedagogicalSelectionContextOnly") || // Require the fixed disclosure decision and pedagogical-context-only use.
            !EqualsText(value, "Recipient", "ServerOwnedNativeCodexPedagogicalSelection") || !EqualsText(value, "Model", "gpt-6-astra") || !EqualsText(value, "ReasoningEffort", "ultra") || // Require the fixed native pedagogical recipient and retained Astra/ultra role labels.
            !Text(value, "ApprovedAtUtc", 28) || !DateTimeOffset.TryParseExact(value.GetProperty("ApprovedAtUtc").GetString(), "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var at) || // Parse only the exact seven-fractional-digit UTC timestamp under invariant culture.
            at > DateTimeOffset.UtcNow || at.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture) != value.GetProperty("ApprovedAtUtc").GetString() || // Reject future timestamps and alternate spellings of the same time value.
            grant.SourceSelection?.TeachingCurriculum is null || grant.CheckpointSelection is null || grant.CorpusSelection is null || grant.EvaluationPlanSelection is null || grant.EvaluatorSelection is null || // Require all full V2 source, checkpoint, corpus, plan and evaluator attachments.
            !Array(value.GetProperty("Evidence"), 1, 32) || !Array(value.GetProperty("AllTeachingSources"), 1, 8)) return null; // Bound evidence and all-source arrays before traversing any declared source row.
        var evidence = new HashSet<string>(StringComparer.Ordinal); // Track evidence references ordinally so duplicates cannot inflate approval support.
        foreach (var e in value.GetProperty("Evidence").EnumerateArray()) // Validate each declared evidence row rather than trusting the outer array alone.
            if (!Fields(e, "EvidenceRef,EvidenceSha256") || !Text(e, "EvidenceRef", 512) || !Hash(e, "EvidenceSha256") || !evidence.Add(e.GetProperty("EvidenceRef").GetString()!)) return null; // Require closed evidence fields, bounded references, canonical digests and unique refs.
        if (!Encoding.UTF8.GetBytes(value.GetProperty("SourceSelection").GetRawText()).AsSpan().SequenceEqual(CodeDocumentationOneLessonV2Wire.CanonicalSourceSelectionBytes(grant.SourceSelection))) return null; // Bind the full declared SourceSelection bytes to the exact retained V2 selection.
        var descriptors = grant.SourceSelection.TeachingCurriculum.Sources; // Use the grant's actual complete curriculum descriptors as the membership allowlist.
        var sources = value.GetProperty("AllTeachingSources"); // Read the approval's complete source table, including sources with no eligible candidate.
        if (sources.GetArrayLength() != descriptors.Length) return null; // Reject missing or extra source rows before any raw material batch can open.
        var ids = new HashSet<string>(StringComparer.Ordinal); // Track unique source ids so one duplicated row cannot stand in for another owner.
        var referenceField = value.GetProperty("SchemaVersion").GetInt32() == 2 ? "ReferenceAnswerRawUtf8Sha256" : "ReferenceAnswerSha256"; // Give each admitted schema one disjoint reference field, without aliases or fallback.
        foreach (var s in sources.EnumerateArray()) // Validate every full declared source and its entry table against retained descriptors.
        { // Inspect a source's closed shape and owner identity before validating its entries.
            if (!Fields(s, "Descriptor,Entries") || !Array(s.GetProperty("Entries"), 1, 10_000) || !Fields(s.GetProperty("Descriptor"), "SourceId,CorpusId,DomainSlug,SubjectSlug,CorpusVersion,ManifestFileSha256,ManifestFileByteLength,CorpusSha256,PayloadSha256,PayloadByteLength")) return null; // Require exact source/descriptor fields and a nonempty entry array bounded at10000.
            var d = s.GetProperty("Descriptor"); // Read the descriptor only after its closed field set has been established.
            if (!Text(d, "SourceId", 128) || !ids.Add(d.GetProperty("SourceId").GetString()!)) return null; // Require a bounded unique source id for this complete approval graph.
            var matches = descriptors.Where(x => x.SourceId == d.GetProperty("SourceId").GetString()).ToArray(); // Locate the descriptor with this exact source id in the retained grant.
            if (matches.Length != 1 || !CodeDocumentationOneLessonV2Wire.CanonicalTeachingDisclosureBytes(d).AsSpan().SequenceEqual(CodeDocumentationOneLessonV2Wire.CanonicalTeachingDisclosureBytes(JsonSerializer.SerializeToElement(matches[0])))) return null; // Require one descriptor and equality of every canonical descriptor field, not just its id.
            var entries = new HashSet<string>(StringComparer.Ordinal); // Track unique entry ids within this source so duplicate declarations cannot hide omissions.
            foreach (var e in s.GetProperty("Entries").EnumerateArray()) // Validate every entry's ownership hashes and legal eligibility declaration.
            { // Inspect each entry before adding any raw task/reference material to a recipient.
                if (!Fields(e, "EntryId,SourceEntityFingerprint,OwnerInputSha256," + referenceField + ",PaidReferenceProvenanceSha256,Eligibility") || // Require exactly the schema-owned reference field and the unchanged complete entry fields.
                    !Text(e, "EntryId", 256) || !Text(e, "SourceEntityFingerprint", 256) || !entries.Add(e.GetProperty("EntryId").GetString()!) || // Bound entry/fingerprint text and reject duplicate entry ids within the source.
                    !Hash(e, "OwnerInputSha256", true) || !Hash(e, referenceField, true) || !Hash(e, "PaidReferenceProvenanceSha256", true)) return null; // Require canonical owner-input, schema-owned reference and paid-provenance SHA256 refs.
                var g = e.GetProperty("Eligibility"); // Read the entry's eligibility only after the closed entry shape is known.
                if (!Fields(g, "LicenseAccepted,IsQuarantined,EvidenceRefs") || g.GetProperty("LicenseAccepted").ValueKind != JsonValueKind.True || g.GetProperty("IsQuarantined").ValueKind != JsonValueKind.False || !Array(g.GetProperty("EvidenceRefs"), 1, 32)) return null; // Require accepted license, no quarantine and a bounded nonempty eligibility evidence list.
                var refs = new HashSet<string>(StringComparer.Ordinal); // Track eligibility references ordinally for this particular entry.
                foreach (var eRef in g.GetProperty("EvidenceRefs").EnumerateArray()) if (!String(eRef, 512) || !refs.Add(eRef.GetString()!)) return null; // Require bounded unique evidence text for every entry eligibility assertion.
                var policy = TrainingDataGovernanceAdmission.ValidateDecision(new(true, false, refs.ToArray())); // Apply the existing training-data governance decision validation to the declared legal flags.
                if (!policy.IsSuccess) return null; // Refuse governance-invalid eligibility rather than converting approval text into rights.
            } // Finish validating this entry before continuing to the next declared entry.
        } // Finish the source's full entries before continuing to another declared owner.
        var bytes = CodeDocumentationOneLessonV2Wire.CanonicalTeachingDisclosureBytes(value); // Canonicalize the complete declaration only after all grammar and membership checks pass.
        return Encoding.UTF8.GetBytes(value.GetRawText()).AsSpan().SequenceEqual(bytes) ? new(value) : null; // Accept only byte-canonical JSON and construct the private immutable approval from it.
    } // Finish the closed decoder without admitting partial or alternative declaration spellings.
    internal OperationResult<bool> MatchCompleteGraph(CodeDocumentationOneLessonSourceSelection.DeclaredTeachingCorpusReadback graph) // Recheck the approved full graph against actual sealed owner readback before disclosure.
    { // Compare full source selection and every declared entry without choosing a teaching task.
        if (!Encoding.UTF8.GetBytes(declaration.GetProperty("SourceSelection").GetRawText()).AsSpan().SequenceEqual(CodeDocumentationOneLessonV2Wire.CanonicalSourceSelectionBytes(graph.Selection)) || graph.Sources.Count != declaration.GetProperty("AllTeachingSources").GetArrayLength()) return Refuse(Invalid); // Require canonical selection equality and exact source count on the actual graph.
        foreach (var s in graph.Sources) // Inspect each actual source, including owners not used by an eligible candidate.
        { // Find that source's full approved entry table before comparing its actual entries.
            var rawUtf8 = schemaVersion == 2; // Select only the immutable decoder-owned reference schema.
            if (s.Manifest.Schema != (rawUtf8 ? RepositoryOwnedReferenceCorpusManifestSealer.RawUtf8Schema : RepositoryOwnedReferenceCorpusManifestSealer.CurrentSchema) || // Bind the declaration to the actual strict owner's homogeneous corpus schema.
                s.Manifest.Context != (rawUtf8 ? RepositoryOwnedReferenceCorpusManifestSealer.RawUtf8Context : RepositoryOwnedReferenceCorpusManifestSealer.CurrentContext)) return Refuse(Invalid); // Reject mixed, unknown or mismatched owner versions before disclosure.
            var supplied = declaration.GetProperty("AllTeachingSources").EnumerateArray().SingleOrDefault(x => x.GetProperty("Descriptor").GetProperty("SourceId").GetString() == s.Descriptor.SourceId); // Locate the one declaration for the actual sealed source id.
            if (supplied.ValueKind != JsonValueKind.Object || supplied.GetProperty("Entries").GetArrayLength() != s.Entries.Count) return Refuse(Invalid); // Reject missing declarations and mismatched entry counts before entry comparison.
            var entries = supplied.GetProperty("Entries").EnumerateArray().ToArray(); // Preserve the approved entry order for comparison with actual sealed owner order.
            for (var i = 0; i < entries.Length; i++) // Compare every entry position, rather than accepting unordered hash membership.
            { // Bind this approved row to the actual entry at the same position.
                var e = s.Entries[i]; var a = entries[i]; // Read the actual and declared entry together for full identity/provenance equality.
                if (e.PaidReferenceProvenance.IsRawUtf8 != rawUtf8 || (rawUtf8 && e.PaidReferenceProvenance.ReferenceAnswerSha256 != string.Empty)) return Refuse(Invalid); // Require the known sealed provenance version and disjoint raw/legacy fields.
                if (!EqualsText(a, "EntryId", e.EntryId) || !EqualsText(a, "SourceEntityFingerprint", e.SourceEntityFingerprint) || !EqualsText(a, "OwnerInputSha256", e.OwnerInputSha256) || // Require exact entry id, fingerprint and complete owner-input digest equality.
                    !EqualsText(a, rawUtf8 ? "ReferenceAnswerRawUtf8Sha256" : "ReferenceAnswerSha256", rawUtf8 ? e.PaidReferenceProvenance.ReferenceAnswerRawUtf8Sha256! : e.PaidReferenceProvenance.ReferenceAnswerSha256) || // Match the actual factory-owned commitment without reinterpreting a normalized digest as raw.
                    !EqualsText(a, "PaidReferenceProvenanceSha256", e.PaidReferenceProvenance.ProvenanceSha256)) return Refuse(Invalid); // Require the full actual paid-provenance commitment as well as the reference digest.
            } // Finish this complete entry comparison before moving to the next position.
        } // Finish this source comparison without omitting unused actual entries.
        return OperationResult<bool>.Ok(true); // Confirm full approved graph equality only after every source and entry matches.
    } // Finish graph matching without granting selection or training authority.
    internal static OperationResult<bool> Refuse(string code) => OperationResult<bool>.Fail("Protected teaching disclosure approval is unavailable or no longer matches its owner.", code); // Return the bounded approval family without leaking raw teaching material in an error.
    private static bool Fields(JsonElement e, string fields) => CodeDocumentationOneLessonV2Wire.ExactFields(e, fields.Split(',')); // Reuse exact field-set validation to reject duplicate, extra and missing JSON members.
    private static bool Array(JsonElement e, int min, int max) => e.ValueKind == JsonValueKind.Array && e.GetArrayLength() >= min && e.GetArrayLength() <= max; // Check both JSON array kind and the declaration-specific minimum/maximum counts.
    private static bool String(JsonElement e, int max) => e.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(e.GetString()) && !e.GetString()!.Any(char.IsControl) && e.GetString() == e.GetString()!.Trim() && CodeDocumentationOneLessonV2Wire.ValidUtf8Text(e.GetString()!, max); // Require trimmed nonblank control-free valid UTF-8 text under the given bound.
    private static bool Text(JsonElement e, string p, int max) => String(e.GetProperty(p), max); // Apply the bounded text grammar to one required declaration property.
    private static bool EqualsText(JsonElement e, string p, string expected) => e.GetProperty(p).ValueKind == JsonValueKind.String && e.GetProperty(p).GetString() == expected; // Compare a required string property ordinally to its fixed contract value.
    private static bool Integer(JsonElement e, int min, int max) => e.ValueKind == JsonValueKind.Number && e.TryGetInt32(out var n) && n >= min && n <= max && e.GetRawText() == n.ToString(CultureInfo.InvariantCulture); // Reject non-integer, out-of-range and noncanonical numeric spellings.
    private static bool Hash(JsonElement e, string p, bool owner = false) => e.GetProperty(p).ValueKind == JsonValueKind.String && (owner ? FrozenReferenceCanonicalization.IsCanonicalSha256Ref(e.GetProperty(p).GetString()) : CodeDocumentationOneLessonV2Wire.Hash(e.GetProperty(p).GetString())); // Use the existing owner-ref or evidence-digest grammar according to the field's role.
} // End the immutable declaration type without exposing mutable state or authority imports.
