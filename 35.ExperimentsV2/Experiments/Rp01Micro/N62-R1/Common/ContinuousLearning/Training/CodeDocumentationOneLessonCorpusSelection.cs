// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning; // Keep complete corpus commitments beside the strict repository corpus owner.

using System.Collections.ObjectModel; // Expose copied case IDs through a read-only collection.
using System.Security.Cryptography; // Hash the complete domain-separated corpus descriptor preimage.
using System.Text; // Encode the fixed corpus commitment domain as UTF-8.
using AgentInfrastructure; // Return pure admission and fresh reopen failures through the shared result contract.

/// <summary>Descriptive startup pins for a closed corpus, never a path or effect capability.</summary>
internal sealed record CodeDocumentationOneLessonCorpusV2( // Describe every closed corpus pin needed for full fresh owner equality.
    string CorpusId, string Purpose, string DomainSlug, string SubjectSlug, // Pin the corpus key, closed-evaluation purpose and repository domain/subject slugs.
    string CorpusVersion, string ManifestFileSha256, string CorpusSha256, // Pin version, exact manifest-file digest and complete corpus identity.
    long ManifestFileByteLength, string PayloadSha256, long PayloadByteLength); // Pin exact manifest length and raw payload digest/length for strict fresh readback.

/// <summary>
/// Complete immutable corpus and held-out commitment. Pure admission/replay
/// performs no I/O; fresh verification delegates the entire payload to its owner.
/// </summary>
internal sealed class CodeDocumentationOneLessonCorpusSelection // Own a complete copied corpus descriptor and its ordered held-out commitment.
{ // Keep copied corpus pins and all ordered cases together.
    internal const string InvalidErrorCode = "ONE_LESSON_CORPUS_SELECTION_INVALID"; // Identify invalid corpus selection and fresh disagreement through one stable error code.
    private readonly string[] _caseIds; // Retain a private case-ID array that callers cannot mutate.
    internal CodeDocumentationOneLessonCorpusV2 Corpus { get; } // Expose the copied complete descriptor pins required for fresh owner equality.
    internal IReadOnlyList<string> CaseIds { get; } // Expose every copied ordered case ID through the immutable selection.
    internal string CorpusDescriptorSha256 { get; } // Expose the hash of the full descriptor and ordered case sequence.

    private CodeDocumentationOneLessonCorpusSelection( // Copy all selected corpus cases and retain their full descriptor commitment.
        CodeDocumentationOneLessonCorpusV2 corpus, string[] caseIds) // Retain the revalidated descriptor and privately copied complete ordered case array.
    { // Copy the descriptor/cases and calculate their complete commitment.
        Corpus = corpus; // Retain all copied descriptor pins rather than a name-only corpus summary.
        _caseIds = caseIds; // Retain the private array copied by the pure corpus admission factory.
        CaseIds = new ReadOnlyCollection<string>(_caseIds); // Expose every selected case through a mutation-rejecting wrapper.
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); // Create pure SHA-256 state for the domain-separated corpus selection preimage.
        hash.AppendData(Encoding.UTF8.GetBytes("xiigen-s04-corpusdescriptor-v2\n")); // Commit the existing corpus descriptor domain prefix before its full canonical preimage.
        hash.AppendData(CodeDocumentationOneLessonV2Wire.CanonicalCorpusSelectionBytes(this)); // Commit all canonical corpus descriptor and ordered case bytes after the domain prefix.
        CorpusDescriptorSha256 = Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant(); // Finalize the digest of the complete admitted byte sequence.
    } // Finish code documentation one lesson corpus selection.

    internal static OperationResult<CodeDocumentationOneLessonCorpusSelection> Create( // Purely validate and copy the complete corpus descriptor and ordered case sequence.
        CodeDocumentationOneLessonCorpusV2? corpus, IEnumerable<string>? caseIds) // Validate the full descriptor and every supplied ordered case before copying.
    { // Copy the complete corpus pins and strictly ordered full case set without I/O.
        if (corpus is null || caseIds is null || !Token(corpus.CorpusId) || // Require a complete descriptor, case sequence and valid opaque corpus key before copying.
            corpus.Purpose != "ClosedEvaluation" || // Require the exact closed-evaluation purpose rather than infer it from another partition.
            !RepositoryOwnedTrainingArtifactLayout.ValidateSlug(corpus.DomainSlug) || // Require the existing bounded repository slug grammar before deriving a path.
            !RepositoryOwnedTrainingArtifactLayout.ValidateSlug(corpus.SubjectSlug) || // Require the existing bounded repository slug grammar before deriving a path.
            !RepositoryOwnedTrainingArtifactLayout.ValidateSlug(corpus.CorpusVersion) || // Require the existing bounded repository slug grammar before deriving a path.
            !CodeDocumentationOneLessonV2Wire.Hash(corpus.ManifestFileSha256) || // Validate the selected digest against the existing bare lowercase SHA-256 grammar.
            !CodeDocumentationOneLessonV2Wire.Hash(corpus.CorpusSha256) || // Validate the selected digest against the existing bare lowercase SHA-256 grammar.
            !CodeDocumentationOneLessonV2Wire.Hash(corpus.PayloadSha256) || // Validate the selected digest against the existing bare lowercase SHA-256 grammar.
            corpus.ManifestFileByteLength is < 1 or > RepositoryOwnedReferenceCorpusManifestSealer.MaximumManifestBytes || // Require a positive raw manifest length within the unchanged 2 MiB owner cap.
            corpus.PayloadByteLength is < 1 or > RepositoryOwnedReferenceCorpusManifestSealer.MaximumPayloadBytes) // Require a positive raw payload length within the unchanged 32 MiB owner cap.
            return Invalid(); // Return normal invalid-selection failure without a usable immutable value.

        var frozen = new List<string>(); // Accumulate the complete admitted case sequence without exposing a mutable list.
        string? previous = null; // Track the preceding ID to enforce strict ordinal order and uniqueness.
        foreach (var id in caseIds) // Validate every supplied canonical case ID without sorting or truncation.
        { // Validate and append this supplied case in its original ordinal position.
            // Reject an unsorted or incomplete supplied selection; do not repair it.
            if (frozen.Count == 200 || !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(id) || // Reject a 201st ID or a noncanonical sha256:lower64 case reference.
                (previous is not null && StringComparer.Ordinal.Compare(previous, id) >= 0)) // Require each case to follow its predecessor strictly; equality rejects duplicates.
                return Invalid(); // Return normal invalid-selection failure without a usable immutable value.
            frozen.Add(id); // Preserve this validated ID in its supplied ordinal position.
            previous = id; // Advance the exact predecessor used by the next order check.
        } // Continue only after this case passed width, identity and ordering checks.
        return frozen.Count == 0 ? Invalid() : // Reject an empty complete selection before creating an immutable snapshot.
            OperationResult<CodeDocumentationOneLessonCorpusSelection>.Ok( // Return the validated typed owner value after the preceding admission checks.
                new(corpus with { }, frozen.ToArray())); // Copy the whole descriptor and admitted ordered sequence into the selection.
    } // Copy the complete corpus pins and strictly ordered full case set without I/O.

    internal OperationResult<CodeDocumentationOneLessonCorpusSelectionOpenResult> OpenFresh( // Verify all selected corpus pins and all actual held-out cases through the strict current owner.
        string repositoryRoot, CodeDocumentationSavedModelAssessmentGenerationSettings? settings) // Use the fixed repository root and actual generation settings for strict readback.
    { // Verify every selected corpus pin and actual held-out case through the strict owner.
        // The selected server validator runs before root resolution or any owner I/O.
        if (!CertificationValidation.Generation(settings)) return InvalidOpen(); // Run the current generation owner validator before root resolution or corpus I/O.
        var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress( // Derive the corpus location only from the fixed root and admitted domain, subject and version.
            repositoryRoot, Corpus.DomainSlug, Corpus.SubjectSlug, Corpus.CorpusVersion); // Build the typed address solely from that fixed root and selected corpus slugs.
        if (!address.IsSuccess || address.Value is null) // Require success of the typed reference-corpus address before using its value.
            return address.MapError<CodeDocumentationOneLessonCorpusSelectionOpenResult>(); // Propagate address validation failure without opening any corpus file.
        var opened = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(address.Value); // Require validated address and reference-corpus custody before strict readback.
        if (!opened.IsSuccess || opened.Value is null) // Require success of the strict fresh owner snapshot before using its value.
            return opened.MapError<CodeDocumentationOneLessonCorpusSelectionOpenResult>(); // Propagate failure from the strict fresh owner snapshot without returning an admitted value.

        var owner = opened.Value; // Use the same strict readback carrying reconstructed corpus and raw manifest-file identity.
        // Legacy corpus identities remain canonical prefixed references on disk.
        // Strip only after verifying both owner representations locally.
        if (!FrozenReferenceCanonicalization.IsCanonicalSha256Ref(owner.Manifest.CorpusSha256) || // Require the existing sha256:lower64 owner identity before stripping its prefix.
            !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(owner.Corpus.CorpusSha256) || // Require the existing sha256:lower64 owner identity before stripping its prefix.
            !StringComparer.Ordinal.Equals(owner.Manifest.CorpusSha256, owner.Corpus.CorpusSha256)) // Require exact agreement between owner.Manifest's whole-corpus canonical identity and owner.Corpus's whole-corpus canonical identity.
            return InvalidOpen(); // Reject fresh disagreement without exposing a snapshot or effect authority.
        var observedCorpus = new CodeDocumentationOneLessonCorpusV2( // Rebuild all ten descriptor pins from the actual strict owner snapshot.
            Corpus.CorpusId, Corpus.Purpose, owner.Manifest.Domain, owner.Manifest.Subject, // Keep selected key/purpose and take domain/subject from the strict owner.
            owner.Manifest.CorpusVersion, owner.ManifestFileSha256, owner.Corpus.CorpusSha256[7..], // Read actual version/raw manifest digest and strip only the verified corpus-hash prefix.
            owner.ManifestFileByteLength, owner.Manifest.PayloadSha256, owner.Manifest.PayloadByteLength); // Read actual manifest length and the manifest's exact raw payload pins.
        if (observedCorpus != Corpus) return InvalidOpen(); // Reject disagreement in any of the ten freshly reconstructed descriptor fields.

        var observedIds = owner.Corpus.Entries // Derive actual cases from the whole reconstructed owner corpus.
            .Where(static entry => entry.Partition == FrozenReferencePartition.HeldOut) // Select only actual held-out entries after full payload and provenance validation.
            .OrderBy(static entry => entry.EntryId, StringComparer.Ordinal) // Order the actual owner IDs for comparison with the canonical selected sequence.
            .Select(static entry => entry.EntryId) // Compare case identities without returning reference answers.
            .ToArray(); // Materialize the entire observed sequence before count and exact membership checks.
        if (observedIds.Length == 0 || observedIds.Length > 200 || // Reject an empty owner set or any owner set exceeding the selected contract maximum.
            observedIds.Length > settings!.MaxHeldOutCases || // Apply the actual current generation held-out limit to the full observed set.
            !observedIds.SequenceEqual(_caseIds, StringComparer.Ordinal)) // Require every actual case to equal the selected ordered sequence; a prefix cannot pass.
            return InvalidOpen(); // Reject fresh disagreement without exposing a snapshot or effect authority.

        var frozen = Create(observedCorpus, observedIds); // Revalidate and recopy the entire freshly observed descriptor and ordered case set.
        if (!frozen.IsSuccess || frozen.Value is null || // Require success of the newly validated immutable attachment before using its value.
            frozen.Value.CaseIds.Count != CaseIds.Count || // Reject disagreement in the complete ordered held-out case sequence before returning the complete snapshot.
            frozen.Value.CorpusDescriptorSha256 != CorpusDescriptorSha256) // Reject disagreement in complete corpus descriptor hash before returning the complete snapshot.
            return InvalidOpen(); // Reject reconstruction failure or disagreement in complete case count or descriptor hash.

        // Owner custody may prepare filesystem state; this snapshot retains no
        // stream or atomic cross-file lease and grants no downstream effect.
        return OperationResult<CodeDocumentationOneLessonCorpusSelectionOpenResult>.Ok( // Return the validated typed owner value after the preceding admission checks.
            new(frozen.Value, owner.Corpus)); // Return the copied complete selection and actual strict owner corpus snapshot.
    } // Verify every selected corpus pin and actual held-out case through the strict owner.

    private static bool Token(string? value) // Require the admitted opaque ASCII identity-token grammar.
        => value is { Length: >= 1 and <= 128 } && // Require the opaque corpus key to contain 1 through 128 characters.
           (value[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9') && // Require an ASCII letter or digit at the beginning of the opaque key.
           value.All(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' // Admit ASCII letters and digits throughout the opaque key.
               or '.' or '_' or ':' or '-'); // Allow only dot, underscore, colon or hyphen punctuation after its first character.

    private static OperationResult<CodeDocumentationOneLessonCorpusSelection> Invalid() // Reject invalid descriptive input with the existing ordinary failure contract.
        => OperationResult<CodeDocumentationOneLessonCorpusSelection>.Fail( // Return an ordinary failed result without a usable admitted value.
            "The complete startup corpus selection is invalid.", InvalidErrorCode); // Report invalid descriptive corpus input without returning a usable selection.

    private static OperationResult<CodeDocumentationOneLessonCorpusSelectionOpenResult> InvalidOpen() // Report disagreement with the complete selected commitment without returning authority.
        => OperationResult<CodeDocumentationOneLessonCorpusSelectionOpenResult>.Fail( // Return an ordinary failed result without a usable admitted value.
            "Fresh corpus readback does not match the complete startup held-out selection.", InvalidErrorCode); // Report strict owner disagreement without returning an effect-capable snapshot.
} // Finish code documentation one lesson corpus selection.

/// <summary>Immutable closed-evaluation snapshot, not an execution or publication receipt.</summary>
internal sealed record CodeDocumentationOneLessonCorpusSelectionOpenResult( // Carry only the copied complete selection and strict reconstructed corpus.
    CodeDocumentationOneLessonCorpusSelection Selection, FrozenReferenceCorpus Corpus); // Keep these two descriptive snapshots together without a stream, lease or effect receipt.
