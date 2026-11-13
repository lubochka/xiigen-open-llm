// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning; // Keep private native intake under the existing reference-corpus owner.

using System.Globalization; // Admit exact owner UTC timestamps without local-time inference.
using System.Security.Cryptography; // Check the explicitly pinned complete private intake bytes.
using System.Text; // Reject malformed UTF-8 before parsing or entry construction.
using System.Text.Json; // Parse one closed strict private operator schema.
using AgentInfrastructure; // Return typed failures and reopened owner results.
using AgentInfrastructure.Logical; // Preserve the existing nonfatal exception-graph boundary without swallowing cancellation.

public sealed record NativeReferenceCorpusCaseBinding(string SourceCaseId, string EntryId); // Descriptive scientific ID mapping, never a training or scoring authority.
public sealed record NativeReferenceCorpusSealResult(RepositoryOwnedReferenceCorpusOpenResult Opened, IReadOnlyList<NativeReferenceCorpusCaseBinding> CaseBindings); // Return the actual reopened owner snapshot for local assessment callers.

/// <summary>Доверенный научный оператор подаёт закрытый intake; owner сохраняет и заново открывает корпус без model calls.</summary>
public static class RepositoryOwnedNativeReferenceCorpusIntake // A strict offline entry into the existing sealer, not a new trainer or approval bypass.
{ // Input assertions remain descriptive; custody and hashes do not certify their truth.
    public const string Schema = "xiigen.native-reference-corpus-intake.v1"; // Public documented private input ABI.
    public const string InvalidErrorCode = "NATIVE_REFERENCE_CORPUS_INTAKE_INVALID"; // Keep errors free of closed contents and arbitrary operator labels.
    private static readonly UTF8Encoding StrictUtf8 = new(false, true); // Raw reference bytes must round-trip unchanged.

    public static OperationResult<NativeReferenceCorpusSealResult> SealFromPrivateFile( // Explicit operator call is the only intake effect entry.
        string trustedRepositoryRoot, string privateInputPath, string expectedInputSha256) // The trusted root and exact raw digest are caller-owned execution inputs, not grants.
    { // Validate current private custody before reading, then use the existing owner to seal and reopen.
        try // Convert nonfatal filesystem/parser failures to a bounded content-free refusal.
        { // Do not parse or print a private input until its actual boundary and digest match.
            var resolved = RepositoryOwnedTrainingArtifactLayout.ResolveReferenceCorpusArtifactRoot(trustedRepositoryRoot); // Reuse actual repository sentinel/root admission.
            if (!resolved.IsSuccess || resolved.Value is null || !BareHash(expectedInputSha256)) return Invalid(); // Reject invalid trusted-root shape or noncanonical digest.
            var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(trustedRepositoryRoot)); // Compute the exact separator-bounded confinement prefix.
            var path = Path.GetFullPath(privateInputPath); // Resolve the explicitly supplied private file only.
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal; // Reuse platform path comparison for confinement.
            if (!path.StartsWith(root + Path.DirectorySeparatorChar, comparison) || !SecureLocalFileBoundary.IsSupportedPlatform() || // Sibling roots and unsupported custody are not fallback inputs.
                SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) || !SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(Path.GetDirectoryName(path)!)) return Invalid(); // Require the input's own private parent and no reparse traversal.
            using var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.SequentialScan); // Open an existing private file without modifying its custody.
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path)) return Invalid(); // Bind the actual opened handle before trusting input bytes.
            var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, RepositoryOwnedReferenceCorpusManifestSealer.MaximumPayloadBytes); // Compare two complete reads on the same handle under the existing 32 MiB cap.
            if (bytes is null || !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path) || // Recheck the actual handle after stable reading.
                SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) || !SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(Path.GetDirectoryName(path)!) || !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path) || // Refuse custody/path changes rather than repairing a foreign input.
                Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant() != expectedInputSha256) return Invalid(); // Hash only the complete stable bytes actually parsed below.
            _ = StrictUtf8.GetCharCount(bytes); // Reject malformed actual UTF-8 before JSON parsing.
            using var json = JsonDocument.Parse(bytes, new() { MaxDepth = 12, AllowTrailingCommas = false, CommentHandling = JsonCommentHandling.Disallow }); // No alternate relaxed input representation is accepted.
            var e = json.RootElement; // Read the private intake only after all custody/raw-pin checks.
            if (!Exact(e, "schema", "corpusName", "corpusVersion", "frozenAtUtc", "subject", "capture", "entries") || // Explicit input schema has no grant/flags/provider identity shortcuts.
                !Text(e, "schema", 256, out var schema) || schema != Schema || !Text(e, "corpusName", 256, out var name) || // Require the single supported ABI and bounded owner name.
                !Text(e, "corpusVersion", 256, out var version) || !Text(e, "subject", 256, out var subject) || // Version/subject subsequently pass the actual typed layout grammar.
                !Text(e, "frozenAtUtc", 64, out var time) || !Utc(time, out var frozen) || // Freeze identity is caller-supplied canonical UTC.
                !RepositoryOwnedReferenceCorpusManifestSealer.TryReadNativeCapture(e.GetProperty("capture"), out var capture) || capture is null || // Read all nullable observed identity and usage fields explicitly.
                e.GetProperty("entries").ValueKind != JsonValueKind.Array) return Invalid(); // Require a complete finite entry sequence, never guessed contents.
            var entries = new List<FrozenReferenceEntry>(); var bindings = new List<NativeReferenceCorpusCaseBinding>(); var caseIds = new HashSet<string>(StringComparer.Ordinal); // Stage exact raw owner entries and scientific ID mappings locally.
            foreach (var row in e.GetProperty("entries").EnumerateArray()) // Process the complete private intake without first-entry truncation.
            { // Refuse the entire intake on one malformed or altered row.
                if (entries.Count >= 10_000 || !Exact(row, "caseId", "ownerInput", "referenceAnswer", "partition", "sourceEntityFingerprint", "ownerInputRawUtf8Sha256", "referenceAnswerRawUtf8Sha256") || // Bound entries and reject duplicate/unknown members.
                    !Text(row, "caseId", 256, out var caseId) || !FrozenReferenceCanonicalization.NormalizeIdentity(caseId).IsSuccess || !caseIds.Add(caseId) || // Require a unique safe scientific alias without claiming source independence.
                    !Text(row, "ownerInput", 65_536, out var input) || !Text(row, "referenceAnswer", 65_536, out var answer) || // Preserve entire raw source and answer strings unchanged.
                    !Text(row, "partition", 16, out var split) || split is not ("train" or "held-out") || // Only the existing owner split is admitted.
                    !Text(row, "sourceEntityFingerprint", 71, out var family) || // The corpus factory owns fingerprint grammar and cross-partition leakage refusal.
                    !Text(row, "ownerInputRawUtf8Sha256", 71, out var inputHash) || !Text(row, "referenceAnswerRawUtf8Sha256", 71, out var answerHash) || // Require explicit original material commitments.
                    FrozenReferenceCanonicalization.RawUtf8Sha256Ref(input) != inputHash || FrozenReferenceCanonicalization.RawUtf8Sha256Ref(answer) != answerHash) return Invalid(); // An edited source/reference cannot cross the intake boundary under old hashes.
                var p = NativeCapturedReferenceProvenance.Create(capture, answer); // Recreate fixed closed-reference restrictions and the complete nullable capture preimage.
                if (!p.IsSuccess || p.Value is null) return Invalid(); // Malformed capture cannot fall back to an invented known-model identity.
                var entry = FrozenReferenceEntry.CreateNativeCapturedCodeDocumentation(input, answer, p.Value, subject, // Reuse origin-specific raw validation and identity construction.
                    split == "train" ? FrozenReferencePartition.Train : FrozenReferencePartition.HeldOut, family); // Keep the actual owner split and source-family commitments.
                if (!entry.IsSuccess || entry.Value is null) return Invalid(); // Return no partial valid subset.
                entries.Add(entry.Value); bindings.Add(new(caseId, entry.Value.EntryId)); // Retain complete raw entries internally and only opaque scientific aliases for receipts.
            } // Zero rows and source-family leakage are rejected by the existing corpus factory.
            var corpus = FrozenReferenceCorpus.Build(name, version, frozen, entries); // Reuse complete canonical corpus identity and source-disjoint split validation.
            if (!corpus.IsSuccess || corpus.Value is null || frozen < capture.CapturedAtUtc) return Invalid(); // A freeze cannot precede the declared capture completion.
            var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(root, corpus.Value.DomainTag, subject, version); // Derive the sole canonical owner output layout.
            if (!address.IsSuccess || address.Value is null) return address.MapError<NativeReferenceCorpusSealResult>(); // Preserve existing layout refusal without an alternate output root.
            var sealedResult = RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(address.Value, corpus.Value); // Existing custody and payload-first/manifest-last no-overwrite publication own the effect.
            if (!sealedResult.IsSuccess || sealedResult.Value is null) return sealedResult.MapError<NativeReferenceCorpusSealResult>(); // A partial/failed write never becomes a successful receipt.
            var reopened = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedNativeCapture(address.Value, sealedResult.Value.CorpusSha256,
                sealedResult.Value.PayloadSha256, sealedResult.Value.ManifestSha256); // Require the exact native seal before any legacy preparation can occur on a replaced header.
            if (!reopened.IsSuccess || reopened.Value is null) return reopened.MapError<NativeReferenceCorpusSealResult>(); // Preserve real reopen/custody failures.
            if (reopened.Value.Manifest != sealedResult.Value || reopened.Value.Corpus.CorpusSha256 != corpus.Value.CorpusSha256 || // Require the exact owner seal and complete corpus commitment.
                reopened.Value.Corpus.CanonicalRepresentation != corpus.Value.CanonicalRepresentation) return Invalid(); // Byte/metadata drift is not an accepted scientific input.
            return OperationResult<NativeReferenceCorpusSealResult>.Ok(new(reopened.Value, Array.AsReadOnly(bindings.ToArray()))); // Return a live reopened snapshot and opaque aliases for assessment ownership.
        } // Successful sealing describes persistence only, never learning or model quality.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return Invalid(); } // Do not log private JSON or suppress cancellation/fatal exceptions.
    } // No Teacher, Student, native AI or optimizer call exists in this entry.

    private static bool Exact(JsonElement e, params string[] names) // Strict closed property sets reject duplicates as well as omissions/extras.
    { // Keep parser failure content-free and deterministic.
        if (e.ValueKind != JsonValueKind.Object) return false; // Arrays/scalars cannot substitute for schema objects.
        var expected = new HashSet<string>(names, StringComparer.Ordinal); var seen = new HashSet<string>(StringComparer.Ordinal); // Require exact-case member identity.
        foreach (var p in e.EnumerateObject()) if (!expected.Contains(p.Name) || !seen.Add(p.Name)) return false; // Refuse unknown or duplicate keys.
        return seen.Count == expected.Count; // Every required member occurs exactly once.
    } // No values are printed or indexed.
    private static bool Text(JsonElement e, string name, int cap, out string text) // Bounded exact JSON string admission.
    { // Stage text only after the member set has been validated.
        text = string.Empty; var p = e.GetProperty(name); // Preserve an unusable failure carrier on type mismatch.
        return p.ValueKind == JsonValueKind.String && (text = p.GetString()!) is not null && text.Length <= cap; // No trimming, line-ending or Unicode rewrite occurs.
    } // Domain factories reject blank/unsafe content and identities.
    private static bool Utc(string text, out DateTimeOffset value) => DateTimeOffset.TryParseExact(text, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out value) && // No implicit local offset conversion.
        FrozenReferenceCanonicalization.IsUtc(value) && FrozenReferenceCanonicalization.FormatUtc(value) == text; // Accept the same exact UTC representation as sealed owner JSON.
    private static bool BareHash(string? s) => s is { Length: 64 } && s.AsSpan().IndexOfAnyExcept("0123456789abcdef") < 0; // CLI pin hashes actual raw file bytes with lowercase SHA-256.
    private static OperationResult<NativeReferenceCorpusSealResult> Invalid() => OperationResult<NativeReferenceCorpusSealResult>.Fail("Private native reference intake is invalid or unavailable.", InvalidErrorCode); // Never disclose raw private contents in refusal output.
} // Trusted operator assertions are content bindings, not signed provider or licensing evidence.
