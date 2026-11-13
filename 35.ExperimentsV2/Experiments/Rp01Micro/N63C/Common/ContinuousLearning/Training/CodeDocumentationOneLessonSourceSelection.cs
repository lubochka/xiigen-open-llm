// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

/// <summary>Descriptive startup material, never a path or effect capability.</summary>
internal sealed record CodeDocumentationOneLessonSourceV2(
    string SourceId, string Role, string DomainSlug, string DesignPatternSlug,
    string DatasetVersion, string ManifestFileSha256, long ManifestFileByteLength,
    string SealedManifestSha256);

internal sealed record CodeDocumentationOneLessonPairSelectionV2(
    string TrainingPairId, string SourceId);

/// <summary>Declared teaching pins and links only; these records grant no material access.</summary>
internal sealed record CodeDocumentationOneLessonTeachingSourceV2( // Declare reference address and byte pins without issuing corpus authority.
    string SourceId, string CorpusId, string DomainSlug, string SubjectSlug, // Declare reference address and byte pins without issuing corpus authority.
    string CorpusVersion, string ManifestFileSha256, long ManifestFileByteLength, // Declare reference address and byte pins without issuing corpus authority.
    string CorpusSha256, string PayloadSha256, long PayloadByteLength); // Declare reference address and byte pins without issuing corpus authority.

internal sealed record CodeDocumentationOneLessonTeachingItemV2( // Keep the declared source-entry link and fingerprint together.
    string ItemId, string SourceId, string ReferenceEntryId, // Keep the declared source-entry link and fingerprint together.
    string SourceEntityFingerprint); // Keep the declared source-entry link and fingerprint together.

internal sealed record CodeDocumentationOneLessonTeachingCategoryBindingV2( // Retain each declared category and its complete item order.
    string Category, string[] OrderedItemIds); // Retain each declared category and its complete item order.

internal sealed record CodeDocumentationOneLessonTeachingCurriculumV2( // Carry the complete externally declared table without approving its policy.
    string PolicyId, string PolicyVersion, // Carry the complete externally declared table without approving its policy.
    CodeDocumentationOneLessonTeachingSourceV2[] Sources, // Carry the complete externally declared table without approving its policy.
    CodeDocumentationOneLessonTeachingItemV2[] Items, // Carry the complete externally declared table without approving its policy.
    CodeDocumentationOneLessonTeachingCategoryBindingV2[] CategoryBindings); // Carry the complete externally declared table without approving its policy.

/// <summary>
/// Complete frozen SOURCE preimage. Creation/replay are pure; fresh readback
/// uses the existing dataset owner. A returned snapshot retains no file lease.
/// </summary>
internal sealed class CodeDocumentationOneLessonSourceSelection
{
    internal const string InvalidErrorCode = "ONE_LESSON_SOURCE_SELECTION_INVALID";
    internal IReadOnlyList<CodeDocumentationOneLessonSourceV2> Sources { get; }
    internal IReadOnlyList<CodeDocumentationOneLessonPairSelectionV2> PairSelections { get; }
    internal string SourceSelectionSha256 { get; }
    private readonly CodeDocumentationOneLessonTeachingCurriculumV2? teachingCurriculum; // Keep owned arrays private and return only detached curriculum snapshots.
    internal CodeDocumentationOneLessonTeachingCurriculumV2? TeachingCurriculum // Keep owned arrays private and return only detached curriculum snapshots.
        => CopyCurriculum(teachingCurriculum); // Keep owned arrays private and return only detached curriculum snapshots.
    private static readonly UTF8Encoding StrictUtf8 = new(false, true); // Keep owned arrays private and return only detached curriculum snapshots.

    private CodeDocumentationOneLessonSourceSelection(
        CodeDocumentationOneLessonSourceV2[] sources,
        CodeDocumentationOneLessonPairSelectionV2[] pairSelections, // Hash the captured validated curriculum with the existing SOURCE preimage.
        CodeDocumentationOneLessonTeachingCurriculumV2? curriculum) // Hash the captured validated curriculum with the existing SOURCE preimage.
    {
        Sources = new ReadOnlyCollection<CodeDocumentationOneLessonSourceV2>(sources);
        PairSelections = new ReadOnlyCollection<CodeDocumentationOneLessonPairSelectionV2>(pairSelections);
        teachingCurriculum = curriculum; // Hash the captured validated curriculum with the existing SOURCE preimage.
        var bytes = CodeDocumentationOneLessonV2Wire.CanonicalSourceSelectionBytes(this);
        var prefix = Encoding.UTF8.GetBytes("xiigen-s04-source-selection-v2\n");
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(prefix);
        hash.AppendData(bytes);
        SourceSelectionSha256 = Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    internal static OperationResult<CodeDocumentationOneLessonSourceSelection> Create(
        IEnumerable<CodeDocumentationOneLessonSourceV2>? sources,
        IEnumerable<CodeDocumentationOneLessonPairSelectionV2>? pairSelections)
        => Create(sources, pairSelections, null); // Admit the complete detached curriculum through the existing source factory.

    internal static OperationResult<CodeDocumentationOneLessonSourceSelection> Create( // Admit the complete detached curriculum through the existing source factory.
        IEnumerable<CodeDocumentationOneLessonSourceV2>? sources, // Admit the complete detached curriculum through the existing source factory.
        IEnumerable<CodeDocumentationOneLessonPairSelectionV2>? pairSelections, // Admit the complete detached curriculum through the existing source factory.
        CodeDocumentationOneLessonTeachingCurriculumV2? curriculum) // Admit the complete detached curriculum through the existing source factory.
    {
        // WHY: validate and hash the SAME detached snapshot. Caller array mutation
        // between validation and copying must never admit unvalidated descendants.
        var capturedCurriculum = CopyCurriculum(curriculum); // Admit the complete detached curriculum through the existing source factory.
        if (capturedCurriculum is not null && !ValidCurriculum(capturedCurriculum)) return Invalid(); // Admit the complete detached curriculum through the existing source factory.
        if (sources is null || pairSelections is null) return Invalid();
        var frozen = new List<CodeDocumentationOneLessonSourceV2>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var addresses = new HashSet<(string, string, string)>();
        string? previous = null;
        foreach (var source in sources)
        {
            if (frozen.Count == 8 || source is null || !Token(source.SourceId) ||
                source.Role != "TrainingPairs" ||
                !RepositoryOwnedTrainingArtifactLayout.ValidateSlug(source.DomainSlug) ||
                !RepositoryOwnedTrainingArtifactLayout.ValidateSlug(source.DesignPatternSlug) ||
                !RepositoryOwnedTrainingArtifactLayout.ValidateSlug(source.DatasetVersion) ||
                !CodeDocumentationOneLessonV2Wire.Hash(source.ManifestFileSha256) ||
                !CodeDocumentationOneLessonV2Wire.Hash(source.SealedManifestSha256) ||
                source.ManifestFileByteLength is < 1 or > 8_388_608 ||
                !ids.Add(source.SourceId) ||
                !addresses.Add((source.DomainSlug, source.DesignPatternSlug, source.DatasetVersion)) ||
                (previous is not null && StringComparer.Ordinal.Compare(previous, source.SourceId) >= 0))
                return Invalid();
            frozen.Add(source);
            previous = source.SourceId;
        }
        if (frozen.Count == 0) return Invalid();
        CodeDocumentationOneLessonPairSelectionV2? mapping = null;
        foreach (var pair in pairSelections)
        {
            if (mapping is not null || pair is null ||
                !CodeDocumentationOneLessonV2Wire.Hash(pair.TrainingPairId) ||
                !Token(pair.SourceId) || !ids.Contains(pair.SourceId)) return Invalid();
            mapping = pair;
        }
        return mapping is null ? Invalid() :
            OperationResult<CodeDocumentationOneLessonSourceSelection>.Ok(
                new(frozen.ToArray(), [mapping], capturedCurriculum)); // Admit the complete detached curriculum through the existing source factory.
    } // Admit the complete detached curriculum through the existing source factory.

    private static CodeDocumentationOneLessonTeachingCurriculumV2? CopyCurriculum( // Copy every record and nested array before validation or returning a view.
        CodeDocumentationOneLessonTeachingCurriculumV2? value) // Copy every record and nested array before validation or returning a view.
        => value is null ? null : value with // Copy every record and nested array before validation or returning a view.
        { // Copy every record and nested array before validation or returning a view.
            Sources = value.Sources?.Select(s => s is null ? null! : s with { }).ToArray()!, // Copy every record and nested array before validation or returning a view.
            Items = value.Items?.Select(i => i is null ? null! : i with { }).ToArray()!, // Copy every record and nested array before validation or returning a view.
            CategoryBindings = value.CategoryBindings?.Select(b => b is null ? null! : b with // Copy every record and nested array before validation or returning a view.
            { OrderedItemIds = b.OrderedItemIds?.ToArray()! }).ToArray()!, // Copy every record and nested array before validation or returning a view.
        }; // Copy every record and nested array before validation or returning a view.

    private static bool ValidCurriculum(CodeDocumentationOneLessonTeachingCurriculumV2 value) // Validate all declared leaves and links without material IO or policy selection.
    { // Validate all declared leaves and links without material IO or policy selection.
        if (!Utf8Strings(value.PolicyId, value.PolicyVersion) || // Validate all declared leaves and links without material IO or policy selection.
            !Token(value.PolicyId) || !Token(value.PolicyVersion) || // Validate all declared leaves and links without material IO or policy selection.
            value.Sources is not { Length: > 0 } || value.Items is not { Length: > 0 } || // Validate all declared leaves and links without material IO or policy selection.
            value.CategoryBindings is not { Length: > 0 }) return false; // Validate all declared leaves and links without material IO or policy selection.
        var sources = new HashSet<string>(StringComparer.Ordinal); // Validate all declared leaves and links without material IO or policy selection.
        var addresses = new HashSet<(string, string, string)>(); // Validate all declared leaves and links without material IO or policy selection.
        foreach (var source in value.Sources) // Validate all declared leaves and links without material IO or policy selection.
        { // Validate all declared leaves and links without material IO or policy selection.
            if (source is null || !Utf8Strings(source.SourceId, source.CorpusId, source.DomainSlug, // Validate all declared leaves and links without material IO or policy selection.
                source.SubjectSlug, source.CorpusVersion, source.ManifestFileSha256, // Validate all declared leaves and links without material IO or policy selection.
                source.CorpusSha256, source.PayloadSha256) || // Validate all declared leaves and links without material IO or policy selection.
                !Token(source.SourceId) || !Token(source.CorpusId) || // Validate all declared leaves and links without material IO or policy selection.
                !RepositoryOwnedTrainingArtifactLayout.ValidateSlug(source.DomainSlug) || // Validate all declared leaves and links without material IO or policy selection.
                !RepositoryOwnedTrainingArtifactLayout.ValidateSlug(source.SubjectSlug) || // Validate all declared leaves and links without material IO or policy selection.
                !RepositoryOwnedTrainingArtifactLayout.ValidateSlug(source.CorpusVersion) || // Validate all declared leaves and links without material IO or policy selection.
                !CodeDocumentationOneLessonV2Wire.Hash(source.ManifestFileSha256) || // Validate all declared leaves and links without material IO or policy selection.
                !CodeDocumentationOneLessonV2Wire.Hash(source.CorpusSha256) || // Validate all declared leaves and links without material IO or policy selection.
                !CodeDocumentationOneLessonV2Wire.Hash(source.PayloadSha256) || // Validate all declared leaves and links without material IO or policy selection.
                source.ManifestFileByteLength is < 1 or > RepositoryOwnedReferenceCorpusManifestSealer.MaximumManifestBytes || // Validate all declared leaves and links without material IO or policy selection.
                source.PayloadByteLength is < 1 or > RepositoryOwnedReferenceCorpusManifestSealer.MaximumPayloadBytes || // Validate all declared leaves and links without material IO or policy selection.
                !sources.Add(source.SourceId) || // Validate all declared leaves and links without material IO or policy selection.
                !addresses.Add((source.DomainSlug, source.SubjectSlug, source.CorpusVersion))) return false; // Validate all declared leaves and links without material IO or policy selection.
        } // Validate all declared leaves and links without material IO or policy selection.
        var items = new HashSet<string>(StringComparer.Ordinal); // Validate all declared leaves and links without material IO or policy selection.
        var entries = new HashSet<(string, string)>(); // Validate all declared leaves and links without material IO or policy selection.
        var usedSources = new HashSet<string>(StringComparer.Ordinal); // Validate all declared leaves and links without material IO or policy selection.
        foreach (var item in value.Items) // Validate all declared leaves and links without material IO or policy selection.
        { // Validate all declared leaves and links without material IO or policy selection.
            if (item is null || !Utf8Strings(item.ItemId, item.SourceId, item.ReferenceEntryId, // Validate all declared leaves and links without material IO or policy selection.
                    item.SourceEntityFingerprint) || !Token(item.ItemId) || !Token(item.SourceId) || // Validate all declared leaves and links without material IO or policy selection.
                !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(item.ReferenceEntryId) || // Validate all declared leaves and links without material IO or policy selection.
                !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(item.SourceEntityFingerprint) || // Validate all declared leaves and links without material IO or policy selection.
                !items.Add(item.ItemId) || !sources.Contains(item.SourceId) || // Validate all declared leaves and links without material IO or policy selection.
                !entries.Add((item.SourceId, item.ReferenceEntryId))) return false; // Validate all declared leaves and links without material IO or policy selection.
            usedSources.Add(item.SourceId); // Validate all declared leaves and links without material IO or policy selection.
        } // Validate all declared leaves and links without material IO or policy selection.
        var categories = new HashSet<string>(StringComparer.Ordinal); // Validate all declared leaves and links without material IO or policy selection.
        var usedItems = new HashSet<string>(StringComparer.Ordinal); // Validate all declared leaves and links without material IO or policy selection.
        foreach (var binding in value.CategoryBindings) // Validate all declared leaves and links without material IO or policy selection.
        { // Validate all declared leaves and links without material IO or policy selection.
            if (binding is null || !Utf8Strings(binding.Category) || !Token(binding.Category) || // Validate all declared leaves and links without material IO or policy selection.
                !categories.Add(binding.Category) || binding.OrderedItemIds is not { Length: > 0 }) return false; // Validate all declared leaves and links without material IO or policy selection.
            var ordered = new HashSet<string>(StringComparer.Ordinal); // Validate all declared leaves and links without material IO or policy selection.
            foreach (var id in binding.OrderedItemIds) // Validate all declared leaves and links without material IO or policy selection.
            { // Validate all declared leaves and links without material IO or policy selection.
                if (!Utf8Strings(id) || !Token(id) || !items.Contains(id) || !ordered.Add(id)) return false; // Validate all declared leaves and links without material IO or policy selection.
                usedItems.Add(id); // Validate all declared leaves and links without material IO or policy selection.
            } // Validate all declared leaves and links without material IO or policy selection.
        } // Validate all declared leaves and links without material IO or policy selection.
        return usedSources.SetEquals(sources) && usedItems.SetEquals(items); // Validate all declared leaves and links without material IO or policy selection.
    } // Validate all declared leaves and links without material IO or policy selection.

    private static bool Utf8Strings(params string?[] values) // Reject null or malformed original Unicode before JSON can replace a surrogate.
    { // Reject null or malformed original Unicode before JSON can replace a surrogate.
        try // Reject null or malformed original Unicode before JSON can replace a surrogate.
        { // Reject null or malformed original Unicode before JSON can replace a surrogate.
            foreach (var value in values) // Reject null or malformed original Unicode before JSON can replace a surrogate.
            { // Reject null or malformed original Unicode before JSON can replace a surrogate.
                if (value is null) return false; // Reject null or malformed original Unicode before JSON can replace a surrogate.
                _ = StrictUtf8.GetByteCount(value); // Reject null or malformed original Unicode before JSON can replace a surrogate.
            } // Reject null or malformed original Unicode before JSON can replace a surrogate.
            return true; // Reject null or malformed original Unicode before JSON can replace a surrogate.
        } // Reject null or malformed original Unicode before JSON can replace a surrogate.
        catch (EncoderFallbackException) { return false; } // Reject null or malformed original Unicode before JSON can replace a surrogate.
    }

    internal OperationResult<CodeDocumentationOneLessonSourceSelectionOpenResult> OpenFresh(
        string repositoryRoot)
    {
        var observedSources = new List<CodeDocumentationOneLessonSourceV2>();
        var mapping = PairSelections[0];
        RepositoryOwnedDpoDatasetAddress? selectedAddress = null;
        RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult? selectedManifest = null;
        RepositoryOwnedDpoDatasetManifestPair? selectedRow = null;
        string? selectedSourceId = null;
        var matches = 0;
        foreach (var source in Sources)
        {
            var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
                repositoryRoot, source.DomainSlug, source.DesignPatternSlug, source.DatasetVersion);
            if (!address.IsSuccess || address.Value is null)
                return address.MapError<CodeDocumentationOneLessonSourceSelectionOpenResult>();
            var opened = RepositoryOwnedDpoDatasetManifestSealer.OpenManifestFileIdentityOnly(address.Value);
            if (!opened.IsSuccess || opened.Value is null)
                return opened.MapError<CodeDocumentationOneLessonSourceSelectionOpenResult>();
            var readback = Readback(source, opened.Value.Manifest,
                opened.Value.ManifestFileSha256, opened.Value.ManifestFileByteLength);
            if (readback != source) return InvalidOpen();
            observedSources.Add(readback);
            foreach (var row in opened.Value.Manifest.OrderedPairs)
            {
                if (!StringComparer.Ordinal.Equals(row.PairId, mapping.TrainingPairId)) continue;
                matches++;
                selectedAddress = address.Value;
                selectedManifest = opened.Value;
                selectedRow = row;
                selectedSourceId = source.SourceId;
            }
        }
        if (matches != 1 || selectedAddress is null || selectedManifest is null ||
            selectedRow is null || selectedSourceId != mapping.SourceId) return InvalidOpen();

        // WHY: this is a second, intentional source stage. Fresh full owner
        // semantics exclude held rows; the integrity-only bypass is not authority.
        var full = RepositoryOwnedDpoDatasetManifestSealer.OpenSealedFileIdentity(
            selectedAddress, selectedManifest.RequestIdentity);
        if (!full.IsSuccess || full.Value is null)
            return full.MapError<CodeDocumentationOneLessonSourceSelectionOpenResult>();
        var selectedSource = Sources.Single(s => s.SourceId == selectedSourceId);
        if (Readback(selectedSource, full.Value.Manifest, full.Value.ManifestFileSha256,
                full.Value.ManifestFileByteLength) != selectedSource) return InvalidOpen();
        var actualRows = full.Value.Manifest.OrderedPairs.Where(r => r.PairId == mapping.TrainingPairId).ToArray();
        var actualPairs = full.Value.Pairs.Where(p =>
            FileDpoPairStore.ComputeIdentity(p, schemaVersion: 5) == mapping.TrainingPairId).ToArray();
        if (actualRows.Length != 1 || actualRows[0] != selectedRow || actualPairs.Length != 1)
            return InvalidOpen();
        var observed = Create(observedSources, PairSelections, TeachingCurriculum); // Reconstruct all teaching metadata while opening only the existing DPO owner.
        if (!observed.IsSuccess || observed.Value is null ||
            observed.Value.SourceSelectionSha256 != SourceSelectionSha256) return InvalidOpen();
        return OperationResult<CodeDocumentationOneLessonSourceSelectionOpenResult>.Ok(
            new(observed.Value, selectedSourceId, actualRows[0], actualPairs[0]));
    }

    /// <summary>
    /// Explicit whole-curriculum byte/pin/link readback through the existing owner.
    /// Existing custody preparation may change modes. No texts, authority or lease
    /// are returned; finite revalidation does not promise future file immutability.
    /// </summary>
    internal OperationResult<DeclaredTeachingCorpusReadback> OpenDeclaredTeachingCorpusIdentities( // Keep teaching I/O separate from passive startup and DPO-only OpenFresh.
        string repositoryRoot, CancellationToken cancellationToken = default) // Observe cancellation before entering synchronous existing owner operations.
        => DeclaredTeachingCorpusReadback.Open(this, repositoryRoot, cancellationToken); // Publish only the verified complete immutable graph.

    internal OperationResult<DeclaredTeachingCorpusReadback> OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal( // Calculate ORIGINAL-only exclusion without granting admission authority.
        string repositoryRoot, FrozenReferenceCorpus originalCorpus, // Receive the complete closed corpus rather than its evaluated subset.
        IReadOnlyList<CodeDocumentationSavedModelAssessmentCaseResult> representativeCases, // Receive the saved representative identities and exact answer digests.
        IReadOnlyList<CodeDocumentationSavedModelAssessmentCalibrationObservation> observations, // Receive every actual repeat and every saved answer cell.
        CancellationToken cancellationToken = default, // Observe cancellation without allowing it to authorize another material owner.
        IReadOnlyList<string>? additionalExposedMetadata = null) // Copy new operator labels/reason into the same finite ORIGINAL comparison.
        => DeclaredTeachingCorpusReadback.OpenExcludingOriginal(this, repositoryRoot, originalCorpus, representativeCases, observations, cancellationToken, additionalExposedMetadata); // Return calculation evidence only; Session owns issuance.

    internal sealed class DeclaredTeachingCorpusReadback // Represent identity evidence only, with construction confined to real owner readback.
    { // Own the full result snapshot without retaining raw entries or file handles.
        internal CodeDocumentationOneLessonSourceSelection Selection { get; } // Retain the same complete SOURCE commitment through its existing immutable owner.
        internal IReadOnlyList<DeclaredTeachingSourceReadback> Sources { get; } // Preserve all declared sources in their supplied order.
        internal IReadOnlyList<DeclaredTeachingItemReadback> Items { get; } // Preserve every declared item and its exact typed source-entry join.
        internal IReadOnlyList<DeclaredTeachingCategoryReadback> CategoryBindings { get; } // Preserve descriptive category order without issuing policy authority.

        private DeclaredTeachingCorpusReadback(CodeDocumentationOneLessonSourceSelection selection, // Prevent caller-created trusted result envelopes.
            IEnumerable<DeclaredTeachingSourceReadback> sources, IEnumerable<DeclaredTeachingItemReadback> items, // Capture completed source and item nodes before publication.
            IEnumerable<DeclaredTeachingCategoryReadback> bindings) // Capture every descriptive ordered category link.
        { // Own all graph collections even when staging lists remain local.
            Selection = selection; // Reuse the immutable validated SOURCE owner.
            Sources = Array.AsReadOnly(sources.ToArray()); // Detach the source collection from staging aliases.
            Items = Array.AsReadOnly(items.ToArray()); // Detach the item collection from staging aliases.
            CategoryBindings = Array.AsReadOnly(bindings.ToArray()); // Detach the binding collection from staging aliases.
        } // Complete immutable result construction only after finite checks.

        internal static OperationResult<DeclaredTeachingCorpusReadback> Open( // The only construction path performs genuine owner readback, never a DTO trust conversion.
            CodeDocumentationOneLessonSourceSelection owner, string repositoryRoot, CancellationToken token) // Bind this lookup to one complete captured SOURCE.
            => OpenWhole(owner, repositoryRoot, token, null); // Preserve the existing identity-only protocol without ORIGINAL comparisons.

        internal static OperationResult<DeclaredTeachingCorpusReadback> OpenExcludingOriginal( // Capture comparison data privately; it conveys no producer or Session authority.
            CodeDocumentationOneLessonSourceSelection owner, string repositoryRoot, FrozenReferenceCorpus corpus, // Bind the same whole teaching owner to complete closed material.
            IReadOnlyList<CodeDocumentationSavedModelAssessmentCaseResult> cases, // Include every representative cell without inventing a trusted report.
            IReadOnlyList<CodeDocumentationSavedModelAssessmentCalibrationObservation> observations, CancellationToken token,
            IReadOnlyList<string>? additionalExposedMetadata = null) // Include all repeat cells and cooperative cancellation.
        { // Keep raw closed data local to this finite calculation.
            var original = OriginalComparison.Capture(corpus, cases, observations, token); // Copy and validate comparison grammars before teaching I/O.
            var metadata=additionalExposedMetadata?.ToArray(); // Detach and validate exposed operator metadata before comparing it against every protected ORIGINAL cell.
            if(metadata is not null && metadata.Any(x=>x is null || !CodeDocumentationOneLessonV2Wire.ValidUtf8Text(x,1_048_576))) // Detach and validate exposed operator metadata before comparing it against every protected ORIGINAL cell.
                return InvalidTeaching(); // Detach and validate exposed operator metadata before comparing it against every protected ORIGINAL cell.
            return original is null ? InvalidTeaching() : OpenWhole(owner, repositoryRoot, token, original,metadata); // Reject malformed calculation inputs rather than minting an empty exclusion.
        } // No raw comparison data survives the returned safe graph.

        private static OperationResult<DeclaredTeachingCorpusReadback> OpenWhole( // Share the one genuine owner traversal between both calculation modes.
            CodeDocumentationOneLessonSourceSelection owner, string repositoryRoot, CancellationToken token, OriginalComparison? original,
            string[]? additionalExposedMetadata = null) // Keep closed material private and optional only for the legacy identity API.
        { // Stage the whole graph and publish none on any failure.
            token.ThrowIfCancellationRequested(); // Pre-cancel before root resolution or filesystem custody preparation.
            var curriculum = owner.TeachingCurriculum; // Capture one detached full curriculum rather than repeatedly consulting getters.
            if (curriculum is null) return InvalidTeaching(); // Absent explicit teaching metadata fails before any I/O.
            var capture = Create(owner.Sources, owner.PairSelections, curriculum); // Revalidate and recopy the same complete SOURCE using the existing factory.
            if (!capture.IsSuccess || capture.Value is null || capture.Value.SourceSelectionSha256 != owner.SourceSelectionSha256) // Require the captured preimage to retain its original commitment.
                return InvalidTeaching(); // Reject a changed or incomplete snapshot without returning partial evidence.
            var captured = capture.Value; // Use only this validated immutable selection for the remaining operation.
            curriculum = captured.TeachingCurriculum!; // Consume only the factory-owned captured table after complete SOURCE revalidation.
            var sources = new List<DeclaredTeachingSourceReadback>(); // Stage every source in supplied order without quotas or sorting.
            var addresses = new List<RepositoryOwnedReferenceCorpusAddress>(); // Retain exact typed addresses for the finite second strict open.
            var sourceIndex = new Dictionary<string, DeclaredTeachingSourceReadback>(StringComparer.Ordinal); // Scope all item joins to their exact declared source namespace.
            var originalOverlap = false; // Stage semantic failure while completing genuine whole-owner pins, joins and reopens.
            foreach (var descriptor in curriculum.Sources) // Open every declared corpus, including later sources.
            { // Derive typed addresses and defer all security/parser work to the existing owner.
                token.ThrowIfCancellationRequested(); // Check cancellation before address resolution and owner I/O.
                var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress( // Derive no caller-supplied file path or alternate authority.
                    repositoryRoot, descriptor.DomainSlug, descriptor.SubjectSlug, descriptor.CorpusVersion); // Pin the exact declared domain, subject and version.
                if (!address.IsSuccess || address.Value is null) return address.MapError<DeclaredTeachingCorpusReadback>(); // Preserve the existing address guard and bounded error.
                token.ThrowIfCancellationRequested(); // Do not enter the synchronous owner with already observed cancellation.
                var opened = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(address.Value); // Reconstruct the whole corpus from stable owner-verified manifest and payload bytes.
                token.ThrowIfCancellationRequested(); // Cancellation after custody preparation rejects publication without claiming rollback.
                if (!opened.IsSuccess || opened.Value is null) return opened.MapError<DeclaredTeachingCorpusReadback>(); // Preserve genuine owner failures rather than fabricating an empty graph.
                if (opened.Value.Corpus.IsNativeCapture) return InvalidTeaching(); // This teaching wire requires a known-model passport; native scientific origin cannot be relabelled.
                var observed = TeachingDescriptor(descriptor, opened.Value); // Compare every byte and address pin while keeping labels descriptive.
                if (observed is null || observed != descriptor || opened.Value.Manifest.EntryCount != opened.Value.Corpus.Count) // Require the whole owner seal and declared pins to agree.
                    return InvalidTeaching(); // Reject pin disagreement using a generic message without closed text.
                var entries = new List<DeclaredTeachingEntryIdentity>(); // Project every actual row, including unused and held-out entries.
                var ids = new HashSet<string>(StringComparer.Ordinal); // Require an unambiguous actual entry identity within this source.
                foreach (var entry in opened.Value.Corpus.Entries) // Preserve owner entry order and avoid evaluation-selection limits.
                { // Copy only safe identity fields from each factory-reconstructed entry.
                    token.ThrowIfCancellationRequested(); // Bound cancellation latency in full corpus projection.
                    if (!ids.Add(entry.EntryId) || entry.DomainTag != observed.DomainSlug || entry.SubjectId != observed.SubjectSlug) // Ensure each projected row belongs to its exact corpus namespace.
                        return InvalidTeaching(); // Reject any ambiguous or foreign actual entry without exposing its content.
                    if (original is not null) originalOverlap |= original.OverlapsEntry(entry, token); // Compare every actual row and finish whole-owner validation before returning semantic failure.
                    entries.Add(new(entry.EntryId, entry.SourceEntityFingerprint, entry.DomainTag, entry.SubjectId, // Copy whole-row and source identities from the actual owner row.
                        entry.Partition, FrozenReferenceEntry.ComputeNormalizedTextSha256(entry.OwnerInput), entry.PaidReferenceProvenance)); // Retain descriptive content commitment and immutable closed-reference restrictions only.
                } // Finish complete safe entry projection before adding this source.
                if (entries.Count != opened.Value.Manifest.EntryCount) return InvalidTeaching(); // Never publish a truncated corpus projection.
                var source = new DeclaredTeachingSourceReadback(observed, opened.Value.Manifest, // Retain the complete immutable manifest and exact copied descriptor.
                    opened.Value.ManifestFileSha256, opened.Value.ManifestFileByteLength, entries); // Keep raw-file pins separate from manifest self-seal.
                sources.Add(source); sourceIndex.Add(observed.SourceId, source); addresses.Add(address.Value); // Preserve source order and its typed address for final revalidation.
            } // Complete the first genuine owner open for every source.
            var items = new List<DeclaredTeachingItemReadback>(); // Stage all declared items in their original order.
            var itemIndex = new Dictionary<string, DeclaredTeachingItemReadback>(StringComparer.Ordinal); // Resolve categories through already checked item nodes.
            foreach (var declared in curriculum.Items) // Join every item, without first-choice fallbacks.
            { // Validate the exact source/reference/fingerprint edge against actual rows.
                token.ThrowIfCancellationRequested(); // Observe cancellation during complete item joining.
                if (!sourceIndex.TryGetValue(declared.SourceId, out var source)) return InvalidTeaching(); // Reject a source namespace that was not opened.
                var matches = source.Entries.Where(e => e.EntryId == declared.ReferenceEntryId).ToArray(); // Match only within this exact genuine source.
                if (matches.Length != 1 || matches[0].SourceEntityFingerprint != declared.SourceEntityFingerprint || // Require one actual row with its declared independent source fingerprint.
                    matches[0].DomainTag != source.Descriptor.DomainSlug || matches[0].SubjectId != source.Descriptor.SubjectSlug) // Preserve entry-to-source address agreement.
                    return InvalidTeaching(); // Reject absent, foreign or altered links without leaking raw material.
                var item = new DeclaredTeachingItemReadback(declared with { }, source, matches[0]); // Retain direct typed links to the verified source and safe row.
                items.Add(item); itemIndex.Add(declared.ItemId, item); // Preserve declared item order and exact identity lookup.
            } // Finish every declared item before resolving categories.
            var bindings = new List<DeclaredTeachingCategoryReadback>(); // Stage all descriptive categories, not a runtime policy.
            foreach (var declared in curriculum.CategoryBindings) // Preserve every category and its complete supplied ordering.
            { // Link every ordered cell to an already verified item node.
                token.ThrowIfCancellationRequested(); // Observe cancellation at each binding boundary.
                var linked = new List<DeclaredTeachingItemReadback>(); // Own this category's complete typed item sequence.
                foreach (var id in declared.OrderedItemIds) // Retain declared order without sorting or truncation.
                { // Verify each typed category edge before publication.
                    token.ThrowIfCancellationRequested(); // Bound cancellation latency even for a long category.
                    if (!itemIndex.TryGetValue(id, out var item)) return InvalidTeaching(); // Reject any unresolved category cell.
                    linked.Add(item); // Retain the already checked item, rather than a detached substitute.
                } // Complete all links in this category.
                bindings.Add(new(declared.Category, linked)); // Own read-only nested views before returning the graph.
            } // Complete every descriptive category binding.
            var observedCurriculum = curriculum with { Sources = sources.Select(s => s.Descriptor with { }).ToArray() }; // Reconstruct the whole observed source table while retaining all captured links.
            var rebuilt = Create(captured.Sources, captured.PairSelections, observedCurriculum); // Use the existing SOURCE writer/domain/hash rather than introducing another serializer.
            if (!rebuilt.IsSuccess || rebuilt.Value is null || rebuilt.Value.SourceSelectionSha256 != captured.SourceSelectionSha256) // Require exact complete SOURCE commitment continuity.
                return InvalidTeaching(); // Reject any observed preimage disagreement before the second open.
            for (var i = 0; i < sources.Count; i++) // Re-open every source, including the earliest, after the complete join.
            { // Finite revalidation observes staleness without inventing a retained atomic lease.
                token.ThrowIfCancellationRequested(); // Cancellation precedes every repeated owner operation.
                var opened = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(addresses[i]); // Reuse the same genuine strict owner and exact typed address.
                token.ThrowIfCancellationRequested(); // Reject cancellation noticed after synchronous revalidation.
                if (!opened.IsSuccess || opened.Value is null) return opened.MapError<DeclaredTeachingCorpusReadback>(); // Preserve owner errors and discard the entire staged graph.
                if (TeachingDescriptor(sources[i].Descriptor, opened.Value) != sources[i].Descriptor) return InvalidTeaching(); // Require the same complete pins on every second open.
            } // Finish all source rechecks before result construction.
            var result = new DeclaredTeachingCorpusReadback(rebuilt.Value, sources, items, bindings); // Finish owning every graph view before the final publication boundary.
            if (original is not null) // Guard every string that the complete safe graph would expose.
            { // Begin visit all newly exposed choice metadata through the existing ORIGINAL overlap comparison without early exit.
                foreach (var text in ExposedMetadata(result)) // Traverse existing typed fields explicitly without reflection, filtering or callbacks.
                    originalOverlap |= original.OverlapsText(text, token); // Visit every exposed field even when an earlier alias already failed.
                foreach(var text in additionalExposedMetadata ?? []) // Visit all newly exposed choice metadata through the existing ORIGINAL overlap comparison without early exit.
                    originalOverlap |= original.OverlapsText(text,token); // Visit all newly exposed choice metadata through the existing ORIGINAL overlap comparison without early exit.
            } // Finish visit all newly exposed choice metadata through the existing ORIGINAL overlap comparison without early exit.
            if (originalOverlap) return OriginalOverlap(); // Return semantic failure only after complete actual pins, joins, order and every-source reopens pass.
            token.ThrowIfCancellationRequested(); // Last cancellation boundary immediately before complete graph publication.
            return OperationResult<DeclaredTeachingCorpusReadback>.Ok(result); // Publish only full immutable descriptive identity evidence.
        } // Retain no corpus/entry/raw-text/lease authority after returning.

        internal static CodeDocumentationOneLessonTeachingSourceV2? TeachingDescriptor( // Share exact owner pin reconstruction with selected payload readback; this issues no material rights.
            CodeDocumentationOneLessonTeachingSourceV2 declared, RepositoryOwnedReferenceCorpusFileIdentityOpenResult opened) // Compare raw-file and canonical owner commitments without confusing their grammars.
        { // Validate canonical prefixes before removing them for the bare SOURCE hash field.
            var manifest = opened.Manifest; // Use the manifest reconstructed by the strict existing owner.
            if (!FrozenReferenceCanonicalization.IsCanonicalSha256Ref(manifest.CorpusSha256) || // Reject malformed prefixed owner hashes before slicing.
                !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(opened.Corpus.CorpusSha256) || // Require the actual reconstructed corpus to use the same canonical grammar.
                manifest.CorpusSha256 != opened.Corpus.CorpusSha256) return null; // Require actual corpus and seal whole-content agreement.
            return new(declared.SourceId, declared.CorpusId, manifest.Domain, manifest.Subject, manifest.CorpusVersion, // Labels remain copied namespace labels, never owner-derived approval.
                opened.ManifestFileSha256, opened.ManifestFileByteLength, manifest.CorpusSha256[7..], manifest.PayloadSha256, manifest.PayloadByteLength); // Pin every exact raw and canonical byte identity from the owner.
        } // Return no text or future immutability guarantee.

        private static OperationResult<DeclaredTeachingCorpusReadback> InvalidTeaching() // Keep link and pin errors bounded and independent of closed material.
            => OperationResult<DeclaredTeachingCorpusReadback>.Fail("Declared teaching identities do not match the complete source snapshot.", InvalidErrorCode); // Preserve the existing SOURCE error domain without granting authority.

        private static OperationResult<DeclaredTeachingCorpusReadback> OriginalOverlap() // Never include protected material in the new error domain.
            => OperationResult<DeclaredTeachingCorpusReadback>.Fail("Declared teaching material overlaps the ORIGINAL.", "ONE_LESSON_TEACHING_ORIGINAL_OVERLAP"); // Describe only the failed prerequisite, not a closed identity or admission decision.

        private static IEnumerable<string> ExposedMetadata(DeclaredTeachingCorpusReadback graph) // Enumerate every existing exposed string through its actual typed owner.
        { // Repeated typed links need not be revisited because their nodes are traversed completely below.
            yield return graph.Selection.SourceSelectionSha256; // Include the SOURCE commitment itself.
            foreach (var source in graph.Selection.Sources) // Include every DPO descriptor rather than only teaching rows.
            { // Preserve whole SOURCE metadata without omitting descriptive labels.
                yield return source.SourceId; yield return source.Role; yield return source.DomainSlug; yield return source.DesignPatternSlug; // Include all address and label fields.
                yield return source.DatasetVersion; yield return source.ManifestFileSha256; yield return source.SealedManifestSha256; // Include both exact raw and sealed commitments.
            } // Complete all DPO descriptor strings.
            foreach (var pair in graph.Selection.PairSelections) // Include every declared training-pair link.
            { yield return pair.TrainingPairId; yield return pair.SourceId; } // Both link endpoints are exposed metadata.
            var curriculum = graph.Selection.TeachingCurriculum!; // Use the already validated full immutable curriculum view.
            yield return curriculum.PolicyId; yield return curriculum.PolicyVersion; // Policy labels remain descriptive and still require disclosure guarding.
            foreach (var source in graph.Sources) // Include each complete actual source node and every entry's safe projection.
            { // Explicitly cover descriptor, manifest and provenance strings.
                var d = source.Descriptor; // Use the exact descriptor present in both graph and curriculum.
                yield return d.SourceId; yield return d.CorpusId; yield return d.DomainSlug; yield return d.SubjectSlug; yield return d.CorpusVersion; // Include every source namespace string.
                yield return d.ManifestFileSha256; yield return d.CorpusSha256; yield return d.PayloadSha256; yield return source.ManifestFileSha256; // Include all exposed bare digest fields.
                var m = source.Manifest; // Traverse the actual immutable manifest rather than a partial DTO.
                yield return m.Schema; yield return m.Context; yield return m.Domain; yield return m.Subject; yield return m.CorpusVersion; yield return m.CorpusName; // Include all manifest identity labels.
                yield return m.FrozenAtUtc; yield return m.CorpusSha256; yield return m.RepositoryRelativeCorpusDirectory; yield return m.RepositoryRelativePayloadPath; // Include timestamp, canonical commitment and locators.
                yield return m.PayloadSha256; yield return m.RepositoryRelativeManifestPath; yield return m.ManifestSha256; // Include the remaining raw and self-seal metadata.
                foreach (var e in source.Entries) // Include unused actual entries as well as declared item targets.
                { // Include all raw-content-free identity and provenance fields.
                    yield return e.EntryId; yield return e.SourceEntityFingerprint; yield return e.DomainTag; yield return e.SubjectId; yield return e.OwnerInputSha256; // Cover every projected entry string.
                    foreach (var text in ProvenanceStrings(e.PaidReferenceProvenance)) yield return text; // Include the whole immutable provenance and its canonical representation.
                } // Complete every actual source row.
            } // Complete all source metadata nodes.
            foreach (var item in graph.Items) // Typed source and entry links point to fully visited nodes.
            { // Cover every declared item field, including its independent namespace label.
                yield return item.DeclaredItem.ItemId; yield return item.DeclaredItem.SourceId; yield return item.DeclaredItem.ReferenceEntryId; yield return item.DeclaredItem.SourceEntityFingerprint; // Cover all declared link endpoints.
            } // Complete all item metadata.
            foreach (var category in graph.CategoryBindings) // Preserve every category and supplied ordered identity cell.
            { yield return category.Category; foreach (var id in category.OrderedItemIds) yield return id; } // Cover both descriptive category labels and complete order metadata.
        } // Numeric and boolean fields disclose no protected material strings.

        private static IEnumerable<string> ProvenanceStrings(PaidReferenceProvenance p) // Traverse the existing immutable provenance explicitly.
        { // These teaching strings are checked against material-specific ORIGINAL references, not shared vendor labels.
            yield return p.ProviderRef; yield return p.ModelId; yield return p.ModelVersion; yield return p.RequestId; yield return FrozenReferenceCanonicalization.FormatUtc(p.CapturedAtUtc); // Cover every provenance label and timestamp in its existing canonical spelling.
            yield return p.ReferenceAnswerSha256; yield return p.CanonicalRepresentation; yield return p.ProvenanceSha256; // Cover answer, full canonical provenance and its commitment.
        } // No authority is inferred from provenance restrictions.

        internal sealed class OriginalComparison // Share the same ORIGINAL-only comparison with history reading; this calculation issues no rights.
        { // Own linear-size closed identities, full canonical texts and exact saved digests.
            private readonly HashSet<string> _references = new(StringComparer.Ordinal); // Store only actual material-specific canonical references and saved case identities.
            private readonly HashSet<string> _referenceDigests = new(StringComparer.Ordinal); // Protect whole bare components only after their canonical material references are validated.
            private readonly HashSet<string> _answers = new(StringComparer.Ordinal); // Keep report bare digests separate from canonical prefixed references.
            private readonly HashSet<string> _texts = new(StringComparer.Ordinal); // Retain complete normalized inputs and paid answers without snippets or quotas.

            internal static OriginalComparison? Capture(FrozenReferenceCorpus corpus, // Receive calculation data rather than a producer certification.
                IReadOnlyList<CodeDocumentationSavedModelAssessmentCaseResult> cases, // Copy every representative saved cell.
                IReadOnlyList<CodeDocumentationSavedModelAssessmentCalibrationObservation> observations, CancellationToken token) // Copy all repeat cells and observe cancellation during traversal.
            { // Validate the two hash grammars independently before accepting comparison input.
                token.ThrowIfCancellationRequested(); // Cancel before closed material capture or teaching opens.
                if (corpus is null || cases is null || observations is null || !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(corpus.CorpusSha256)) return null; // Reject absent or malformed whole-corpus calculation inputs.
                if (corpus.IsNativeCapture) return null; // Legacy teaching/ORIGINAL comparison needs a separate future native-origin disclosure contract.
                var value = new OriginalComparison(); // Retain comparison storage only for this finite owner operation.
                value._references.Add(corpus.CorpusSha256); // Protect the exact complete ORIGINAL corpus commitment.
                foreach (var entry in corpus.Entries) // Include every closed corpus row, even if the evaluator never selected it.
                { // Capture actual material-specific references, never shared domain or vendor names.
                    token.ThrowIfCancellationRequested(); // Bound cancellation during full corpus capture.
                    value._references.Add(entry.EntryId); value._references.Add(entry.SourceEntityFingerprint); // Protect actual row and source identities.
                    value._references.Add(FrozenReferenceEntry.ComputeNormalizedTextSha256(entry.OwnerInput)); // Protect the canonical owner-input commitment.
                    value._references.Add(entry.IsRawCodeDocumentation
                        ? entry.PaidReferenceProvenance.ReferenceAnswerRawUtf8Sha256!
                        : entry.PaidReferenceProvenance.ReferenceAnswerSha256); // Select the factory-owned version's commitment; validate every captured reference below.
                    if (entry.IsRawCodeDocumentation) value._references.Add(FrozenReferenceCanonicalization.RawUtf8Sha256Ref(entry.OwnerInput)); // Protect exact raw owner bytes in addition to conservative normalized aliases.
                    value._references.Add(entry.PaidReferenceProvenance.ProvenanceSha256); // Protect the complete paid provenance commitment.
                    value._texts.Add(Normalize(entry.OwnerInput)); value._texts.Add(Normalize(entry.PaidModelReferenceAnswer)); // Retain the full canonical closed content with no trimming.
                } // Complete all ORIGINAL entries before report capture.
                foreach (var cell in cases) // Include all representative saved answer cells and case identities.
                { // Bare report answer hashes have no sha256 prefix.
                    token.ThrowIfCancellationRequested(); // Observe cancellation for each saved cell.
                    if (cell is null || !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(cell.CaseId) || !BareDigest(cell.StudentAnswerSha256)) return null; // Reject malformed saved case identity or bare answer grammar rather than silently dropping a cell.
                    value._references.Add(cell.CaseId); value._answers.Add(cell.StudentAnswerSha256); // Keep identity matching distinct from exact UTF8 answer hashing.
                } // Complete representative capture.
                foreach (var repeat in observations) // Include all repeats without assuming their deterministic answers differ.
                { // Traverse every identity and digest cell, including the last one.
                    token.ThrowIfCancellationRequested(); // Cancel at each repeat boundary.
                    if (repeat is null || repeat.CaseIds is null || repeat.StudentAnswerSha256 is null || repeat.CaseIds.Count != repeat.StudentAnswerSha256.Count) return null; // Reject incomplete parallel saved arrays.
                    foreach (var id in repeat.CaseIds) { token.ThrowIfCancellationRequested(); if (!FrozenReferenceCanonicalization.IsCanonicalSha256Ref(id)) return null; value._references.Add(id); } // Validate and copy every saved case identity, including null cells.
                    foreach (var digest in repeat.StudentAnswerSha256) // Do not collapse traversal to a representative or first cell.
                    { token.ThrowIfCancellationRequested(); if (!BareDigest(digest)) return null; value._answers.Add(digest); } // Validate and retain each exact bare digest.
                } // Complete full repeat capture.
                foreach (var reference in value._references) // Derive bare material components only from already captured canonical references.
                { token.ThrowIfCancellationRequested(); if (!FrozenReferenceCanonicalization.IsCanonicalSha256Ref(reference)) return null; value._referenceDigests.Add(reference[7..]); } // Validate before slicing; keep report answer grammar independent.
                return value; // Return private calculation storage, not an authority token.
            } // Capture no runtime clients, file handles or producer pointers.

            internal bool OverlapsEntry(FrozenReferenceEntry entry, CancellationToken token) // Check the actual raw teaching row only after genuine owner reconstruction.
                => OverlapsText(entry.EntryId, token) || OverlapsText(entry.SourceEntityFingerprint, token) || // Reject actual identity or source reuse.
                    OverlapsText(entry.OwnerInput, token) || OverlapsText(entry.PaidModelReferenceAnswer, token) || // Check whole normalized content and exact saved-answer byte hashes.
                    ProvenanceStrings(entry.PaidReferenceProvenance).Any(text => OverlapsText(text, token)); // Check every actual provenance string before safe projection.

            internal bool OverlapsText(string text, CancellationToken token) // Guard raw candidate content and every exposed metadata string uniformly.
            { // Keep exact UTF8 answer hashing separate from normalized content comparison.
                token.ThrowIfCancellationRequested(); // Bound cancellation at every candidate string.
                if (_references.Contains(text) || _referenceDigests.Contains(text) || _answers.Contains(text) || _answers.Contains(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant())) return true; // Reject complete prefixed or bare material references, literal report digests and exact saved answer bytes.
                var normalized = Normalize(text); // Apply the frozen corpus NFC/newline rules without trimming or case folding.
                if (_texts.Contains(normalized)) return true; // Fast-path exact full canonical content reuse.
                for (var i = 0; i < text.Length; i++) // Scan embedded material references and bare digests without a quadratic reference-set loop.
                { // Existing identity and content limits bound actual scanned input length.
                    token.ThrowIfCancellationRequested(); // Cancel inside long metadata and canonical representation scans.
                    if (i + 71 <= text.Length && text.AsSpan(i, 7).SequenceEqual("sha256:")) // Consider canonical reference lexemes only when their full prefix fits.
                    { var reference = text.Substring(i, 71); if (FrozenReferenceCanonicalization.IsCanonicalSha256Ref(reference) && _references.Contains(reference)) return true; } // Reject a complete protected material reference inside a larger string.
                    if (i + 64 <= text.Length && text[i] is >= '0' and <= '9' or >= 'a' and <= 'f') // Consider exact lowercase bare report digest lexemes separately.
                    { var digest = text.Substring(i, 64); if (BareDigest(digest) && (_answers.Contains(digest) || _referenceDigests.Contains(digest))) return true; } // Reject full bare protected components without mixing canonical-reference and report grammars.
                } // Finish the complete string scan without truncation.
                foreach (var closed in _texts) // Compare full canonical ORIGINAL texts, never paraphrase claims or partial snippets.
                { // Bound computation by actual owner-limited inputs rather than invented admission quotas.
                    token.ThrowIfCancellationRequested(); // Cancel inside the per-entry/per-closed-text comparison loop.
                    if (closed.Length <= normalized.Length && normalized.Contains(closed, StringComparison.Ordinal)) return true; // Reject exact full closed-content inclusion in candidate material or metadata.
                } // Finish every eligible full-text comparison.
                return false; // A finite ORIGINAL-only calculation makes no semantic paraphrase or full certification claim.
            } // Return no captured raw material.

            private static string Normalize(string text) // Match the existing frozen corpus content canonicalization without changing candidate byte hashing.
                => text.Normalize(NormalizationForm.FormC).Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n'); // Preserve all other content, including whitespace and case.
            private static bool BareDigest(string text) // Validate real saved report digests independently from manifest canonical references.
                => text is { Length: 64 } && text.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f'); // Accept exactly 64 lowercase hexadecimal characters with no prefix.
        } // Release private closed comparison inputs after the synchronous operation.
    } // End the only complete verified graph construction path.

    private static readonly AsyncLocal<Action<string>?> SelectionMaterialReadObserver = new(); // Scope synthetic read faults to this execution context, never a global production callback.
    internal static Action<string>? SelectionMaterialsReadPointForTesting // Observe the actual new payload extraction; identity/exclusion reconstruction is a separate prerequisite.
    { // Restore this scoped hook in test finally blocks.
        get => SelectionMaterialReadObserver.Value; // Expose only the current synthetic observer.
        set => SelectionMaterialReadObserver.Value = value; // Ordinary composition supplies no observer.
    } // This seam issues neither admission nor raw material authority.

    internal OperationResult<TeachingSelectionMaterialReadback> OpenTeachingSelectionMaterialsExcludingOriginal( // Reconstruct every eligible candidate without picking a lesson or truncating the graph.
        string root, IReadOnlyList<string> orderedCategories, FrozenReferenceCorpus original, // Session supplies genuine sanitized categories and keeps the closed owner private.
        IReadOnlyList<CodeDocumentationSavedModelAssessmentCaseResult> cases, // Preserve every saved representative ORIGINAL cell.
        IReadOnlyList<CodeDocumentationSavedModelAssessmentCalibrationObservation> observations, CancellationToken token = default) // Preserve every repeat and cancellation before new raw extraction.
        => TeachingSelectionMaterialReadback.Open(this, root, orderedCategories, original, cases, observations, token); // Only the strict owner constructs this detached descriptive readback.

    internal sealed class TeachingSelectionMaterialReadback // Raw reconstruction is not teaching rights or Session issuance.
    { // Private construction prevents caller-authored cells from masquerading as actual owner readback.
        internal sealed record MaterialCell(DeclaredTeachingItemReadback Item, string OwnerInput, // Immutable strings preserve the exact complete canonical task.
            string PaidModelReferenceAnswer, PaidReferenceProvenance PaidReferenceProvenance); // Paid answer is selection context only, never trainable pair content.
        internal DeclaredTeachingCorpusReadback Graph { get; } // Retain all declared sources, rows and supplied bindings, including unused material.
        internal IReadOnlyList<MaterialCell> Materials { get; } // Candidate order follows declared item order rather than alphabetic/category ranking.
        private TeachingSelectionMaterialReadback(DeclaredTeachingCorpusReadback graph, IEnumerable<MaterialCell> cells) // Only this owner traversal calls the constructor.
        { // Copy the outer collection before any consumer can observe it.
            Graph = graph; Materials = Array.AsReadOnly(cells.ToArray()); // Nested item/provenance records and strings are immutable owner projections.
        } // No corpus handles or future atomic lease survive capture.
        internal static OperationResult<TeachingSelectionMaterialReadback> Open(CodeDocumentationOneLessonSourceSelection owner, // Share genuine whole-owner exclusion with the selected-material path.
            string root, IReadOnlyList<string> categories, FrozenReferenceCorpus original, // Caller paths never replace typed declared corpus addresses.
            IReadOnlyList<CodeDocumentationSavedModelAssessmentCaseResult> cases, // Keep all protected cases inside the owner calculation.
            IReadOnlyList<CodeDocumentationSavedModelAssessmentCalibrationObservation> observations, CancellationToken token) // Cancellation discards the entire staged candidate set.
        { // Admission checking belongs to the receiving Session before this new payload operation.
            token.ThrowIfCancellationRequested(); // Refuse pre-cancelled extraction before any new raw owner I/O.
            var copiedCategories = categories?.ToArray(); // Detach caller collection before eligibility traversal.
            if (!CodeDocumentationOneLessonV2Wire.SelectionCategories(copiedCategories)) return Invalid(); // Only genuine finite diagnostic categories are meaningful here.
            var before = owner.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal(root, original, cases, observations, token); // Verify all used/unused rows and closed aliases before staging raw cells.
            if (!before.IsSuccess || before.Value is null) return before.MapError<TeachingSelectionMaterialReadback>(); // Preserve actual strict owner/exclusion refusals.
            var graph = before.Value; // Retain the complete independently reconstructed graph.
            var eligible = graph.CategoryBindings.Where(c => copiedCategories!.Contains(c.Category, StringComparer.Ordinal)) // Category order is descriptive; this computes membership only.
                .SelectMany(c => c.OrderedItemIds).ToHashSet(StringComparer.Ordinal); // Shared items receive one complete payload cell.
            if (eligible.Count == 0) return Invalid(); // Never substitute the first item when the eligible union is empty.
            var wanted = graph.Items.Where(i => eligible.Contains(i.DeclaredItem.ItemId)).ToArray(); // Preserve declared item order without sorting or dropping eligible joins.
            if (wanted.Length != eligible.Count) return Invalid(); // Every binding ID must resolve exactly once.
            var staged = new Dictionary<string, MaterialCell>(StringComparer.Ordinal); // Stage exact joined cells while corpora remain private.
            foreach (var source in graph.Sources) // Complete source checks already include unused sources; only eligible sources need new payload extraction.
            { // A needed source is read once for all of its eligible items.
                var items = wanted.Where(i => i.Source.Descriptor.SourceId == source.Descriptor.SourceId).ToArray(); // Join by actual verified namespace, never an ordinal.
                if (items.Length == 0) continue; // Do not disclose raw material outside the eligible set.
                var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(root, source.Descriptor.DomainSlug, source.Descriptor.SubjectSlug, source.Descriptor.CorpusVersion); // Resolve the existing typed owner address.
                if (!address.IsSuccess || address.Value is null) return address.MapError<TeachingSelectionMaterialReadback>(); // Preserve address/custody failures.
                token.ThrowIfCancellationRequested(); SelectionMaterialsReadPointForTesting?.Invoke("raw-before-open"); // Tests can observe actual extraction after all Session admissions passed.
                var opened = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(address.Value); // Read genuine complete canonical corpus bytes through the existing strict owner.
                token.ThrowIfCancellationRequested(); // Do not publish a synchronous read completed after cancellation.
                if (!opened.IsSuccess || opened.Value is null) return opened.MapError<TeachingSelectionMaterialReadback>(); // No empty fallback or fabricated row on owner failure.
                if (DeclaredTeachingCorpusReadback.TeachingDescriptor(source.Descriptor, opened.Value) != source.Descriptor || opened.Value.Manifest != source.Manifest) return Invalid(); // Preserve all whole-source manifest/payload pins.
                foreach (var item in items) // Retain every eligible exact item/source/entry/fingerprint join.
                { // Actual full bytes must agree with the previously guarded descriptive row.
                    var entries = opened.Value.Corpus.Entries.Where(e => e.EntryId == item.Entry.EntryId).ToArray(); // Reject ambiguous or missing rows rather than choosing an ordinal.
                    if (entries.Length != 1) return Invalid(); // One exact declared entry is required for each candidate.
                    var entry = entries[0]; // Consume the genuine owner row without routing or DPO projection.
                    if (entry.SourceEntityFingerprint != item.Entry.SourceEntityFingerprint || entry.DomainTag != item.Entry.DomainTag || entry.SubjectId != item.Entry.SubjectId || // Keep source/entry identity and namespace agreement.
                        entry.Partition != item.Entry.Partition || FrozenReferenceEntry.ComputeNormalizedTextSha256(entry.OwnerInput) != item.Entry.OwnerInputSha256 || // Train partition stays descriptive while full task commitment is checked.
                        entry.PaidReferenceProvenance != item.Entry.PaidReferenceProvenance) return Invalid(); // Reference provenance binds actual complete reference bytes reconstructed by the owner.
                    staged.Add(item.DeclaredItem.ItemId, new(item, entry.OwnerInput, entry.PaidModelReferenceAnswer, entry.PaidReferenceProvenance)); // Immutable full strings are retained only inside the staged result.
                } // Finish all eligible cells in this corpus.
            } // Finish every required raw owner read.
            SelectionMaterialsReadPointForTesting?.Invoke("raw-staged"); // Portable faults exercise the actual late whole-owner recheck on Windows and Linux.
            var after = owner.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal(root, original, cases, observations, token); // Reopen all sources/rows, including every unused cell, after staging.
            if (!after.IsSuccess || after.Value is null) return after.MapError<TeachingSelectionMaterialReadback>(); // Discard staged task/reference bytes on any late owner or ORIGINAL failure.
            if (after.Value.Selection.SourceSelectionSha256 != graph.Selection.SourceSelectionSha256 || staged.Count != wanted.Length) return Invalid(); // Require complete SOURCE continuity and full candidate count.
            token.ThrowIfCancellationRequested(); // Last cancellation boundary before descriptive batch publication.
            return OperationResult<TeachingSelectionMaterialReadback>.Ok(new(after.Value, wanted.Select(i => staged[i.DeclaredItem.ItemId]))); // Return every full cell in declared order, with no rights/selection/effect authority.
        } // No file, journal, exposure or model effect is created here.
        private static OperationResult<TeachingSelectionMaterialReadback> Invalid() // Keep raw material and protected identities out of errors.
            => OperationResult<TeachingSelectionMaterialReadback>.Fail("Teaching selection material does not match the complete SOURCE.", InvalidErrorCode); // Reuse the SOURCE refusal domain.
    } // End the strict batch descriptive readback.

    internal OperationResult<SelectedTeachingMaterialReadback> OpenSelectedTeachingMaterialExcludingOriginal( // Receive an exact selected join, never an item ordinal or caller path.
        string repositoryRoot, string itemId, string sourceId, string entryId, string fingerprint, // Bind all selected endpoints to the genuine full SOURCE.
        FrozenReferenceCorpus originalCorpus, IReadOnlyList<CodeDocumentationSavedModelAssessmentCaseResult> cases, // Protect the complete closed corpus and saved representative cells.
        IReadOnlyList<CodeDocumentationSavedModelAssessmentCalibrationObservation> observations, CancellationToken token = default) // Protect every saved repeat before and after payload capture.
        => SelectedTeachingMaterialReadback.Open(this, repositoryRoot, itemId, sourceId, entryId, fingerprint, originalCorpus, cases, observations, token); // Construction remains behind real owner reads.

    internal sealed class SelectedTeachingMaterialReadback // A detached payload reconstruction conveys no license or effect capability.
    { // Keep complete canonical owner strings, with privately owned strict UTF8 snapshots.
        internal DeclaredTeachingCorpusReadback Graph { get; } // Retain the fully rechecked graph, including unused sources.
        internal DeclaredTeachingItemReadback Item { get; } // Retain the exact verified selected source-entry join.
        internal string OwnerInput { get; } // Preserve the complete actual task string after owner canonicalization.
        internal string PaidModelReferenceAnswer { get; } // Preserve the complete paid reference for a later purpose-checked context consumer only.
        internal PaidReferenceProvenance PaidReferenceProvenance { get; } // Immutable provenance retains its closed-reference restrictions.
        private readonly byte[] ownerInputUtf8, paidReferenceUtf8; // Caller byte-array writes cannot change this detached snapshot.
        internal byte[] CopyOwnerInputUtf8() => ownerInputUtf8.ToArray(); // Return a fresh full UTF8 snapshot, without truncation.
        internal byte[] CopyPaidReferenceUtf8() => paidReferenceUtf8.ToArray(); // Return a fresh full reference snapshot, never a trainable projection.
        private SelectedTeachingMaterialReadback(DeclaredTeachingCorpusReadback graph, DeclaredTeachingItemReadback item, FrozenReferenceEntry entry) // Callers cannot manufacture a trusted raw-payload envelope.
        { // Copy only the entry actually reconstructed by the selected strict owner.
            Graph = graph; Item = item; OwnerInput = entry.OwnerInput; PaidModelReferenceAnswer = entry.PaidModelReferenceAnswer; // Own exact immutable strings with their genuine full graph.
            PaidReferenceProvenance = entry.PaidReferenceProvenance; // Preserve the actual immutable provenance object.
            ownerInputUtf8 = StrictUtf8.GetBytes(OwnerInput); paidReferenceUtf8 = StrictUtf8.GetBytes(PaidModelReferenceAnswer); // Snapshot complete strict UTF8 strings without normalization or filtering here.
        } // Retain no corpus handle, path or future atomic lease.
        internal static OperationResult<SelectedTeachingMaterialReadback> Open(CodeDocumentationOneLessonSourceSelection owner, // The only issuer performs complete ORIGINAL-excluding reconstruction.
            string root, string itemId, string sourceId, string entryId, string fingerprint, FrozenReferenceCorpus original, // Require all exact selected identities and the actual protected owner.
            IReadOnlyList<CodeDocumentationSavedModelAssessmentCaseResult> cases, IReadOnlyList<CodeDocumentationSavedModelAssessmentCalibrationObservation> observations, CancellationToken token) // Keep every original comparison cell in both finite checks.
        { // Stage payload privately until every source passes final revalidation.
            var before = owner.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal(root, original, cases, observations, token); // Reuse the one complete graph traversal before selected material I/O.
            if (!before.IsSuccess || before.Value is null) return before.MapError<SelectedTeachingMaterialReadback>(); // Preserve genuine custody, pin and ORIGINAL-overlap refusals.
            var matches = before.Value.Items.Where(i => i.DeclaredItem.ItemId == itemId && i.DeclaredItem.SourceId == sourceId && // Join within the declared source namespace, without first-item fallback.
                i.DeclaredItem.ReferenceEntryId == entryId && i.DeclaredItem.SourceEntityFingerprint == fingerprint).ToArray(); // Match every selected endpoint exactly.
            if (matches.Length != 1) return InvalidMaterial(); // Absent or ambiguous selected joins never publish text.
            var selected = matches[0]; var descriptor = selected.Source.Descriptor; // Use the genuine node, not caller-supplied owner metadata.
            var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(root, descriptor.DomainSlug, descriptor.SubjectSlug, descriptor.CorpusVersion); // Resolve only the existing typed owner address.
            if (!address.IsSuccess || address.Value is null) return address.MapError<SelectedTeachingMaterialReadback>(); // Preserve the existing address refusal.
            token.ThrowIfCancellationRequested(); // Do not enter synchronous selected owner I/O after cancellation.
            var opened = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(address.Value); // Reconstruct the actual complete selected corpus through its strict owner.
            token.ThrowIfCancellationRequested(); // Refuse payload publication when cancellation follows owner I/O.
            if (!opened.IsSuccess || opened.Value is null) return opened.MapError<SelectedTeachingMaterialReadback>(); // Preserve real owner failures instead of fabricating bytes.
            if (DeclaredTeachingCorpusReadback.TeachingDescriptor(descriptor, opened.Value) != descriptor || opened.Value.Manifest != selected.Source.Manifest) return InvalidMaterial(); // Compare complete descriptor/raw/seal/payload pins and the genuine manifest.
            var entries = opened.Value.Corpus.Entries.Where(e => e.EntryId == entryId).ToArray(); // Resolve exactly one actual row in the selected corpus.
            if (entries.Length != 1 || entries[0].SourceEntityFingerprint != fingerprint || entries[0].DomainTag != descriptor.DomainSlug || // Preserve source identity and actual namespace agreement.
                entries[0].SubjectId != descriptor.SubjectSlug || entries[0].Partition != selected.Entry.Partition || // Preserve actual partition without treating Train as permission.
                FrozenReferenceEntry.ComputeNormalizedTextSha256(entries[0].OwnerInput) != selected.Entry.OwnerInputSha256 || // Bind the full task to the graph's actual task commitment.
                entries[0].PaidReferenceProvenance != selected.Entry.PaidReferenceProvenance) return InvalidMaterial(); // Bind the full paid reference to its immutable genuine provenance.
            var staged = new SelectedTeachingMaterialReadback(before.Value, selected, entries[0]); // Copy actual task/reference bytes before the final full graph check.
            var after = owner.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal(root, original, cases, observations, token); // Reopen every source and row, including unused material, after capture.
            if (!after.IsSuccess || after.Value is null) return after.MapError<SelectedTeachingMaterialReadback>(); // Discard staged text on any late ORIGINAL, custody or byte failure.
            if (after.Value.Selection.SourceSelectionSha256 != before.Value.Selection.SourceSelectionSha256 || // Require the identical complete SOURCE preimage.
                !after.Value.Items.Any(i => i.DeclaredItem == selected.DeclaredItem && i.Source.Descriptor == descriptor && i.Entry == selected.Entry)) return InvalidMaterial(); // Reconfirm the same full selected join within that graph.
            token.ThrowIfCancellationRequested(); // Last cancellation boundary before detached material publication.
            return OperationResult<SelectedTeachingMaterialReadback>.Ok(staged); // Return genuine checked bytes, with no material-rights or model authority.
        } // End finite selected-payload capture without retaining an owner lease.
        private static OperationResult<SelectedTeachingMaterialReadback> InvalidMaterial() // Keep errors bounded and free of raw protected material.
            => OperationResult<SelectedTeachingMaterialReadback>.Fail("Selected teaching material does not match the complete SOURCE.", InvalidErrorCode); // Preserve the existing SOURCE error domain.
    } // End the descriptive raw material reconstruction owner.

    internal sealed record DeclaredTeachingEntryIdentity( // Expose safe descriptive commitments rather than the raw frozen entry.
        string EntryId, string SourceEntityFingerprint, string DomainTag, string SubjectId, // Retain the actual row's exact namespace and source identity.
        FrozenReferencePartition Partition, string OwnerInputSha256, PaidReferenceProvenance PaidReferenceProvenance); // Train is descriptive; immutable closed-reference restrictions remain intact.

    internal sealed class DeclaredTeachingSourceReadback // Bind one complete owner manifest to all of its safe entry identities.
    { // Own entry views and copied immutable records.
        internal CodeDocumentationOneLessonTeachingSourceV2 Descriptor { get; } // Preserve every declared pin and the descriptive labels.
        internal RepositoryOwnedReferenceCorpusManifest Manifest { get; } // Return the full owner-verified immutable manifest.
        internal string ManifestFileSha256 { get; } // Retain exact raw manifest-file digest separately from its self-seal.
        internal long ManifestFileByteLength { get; } // Retain the exact stable raw-file length.
        internal IReadOnlyList<DeclaredTeachingEntryIdentity> Entries { get; } // Retain the entire safe entry sequence through an owned read-only view.
        internal DeclaredTeachingSourceReadback(CodeDocumentationOneLessonTeachingSourceV2 descriptor, // Construct only descriptive nodes; complete trusted results require real Open.
            RepositoryOwnedReferenceCorpusManifest manifest, string rawHash, long rawLength, IEnumerable<DeclaredTeachingEntryIdentity> entries) // Capture complete owner metadata, never raw corpus content.
        { // Detach all collection storage from staging lists.
            Descriptor = descriptor with { }; Manifest = manifest with { }; // Own the immutable metadata records while preserving every field.
            ManifestFileSha256 = rawHash; ManifestFileByteLength = rawLength; // Preserve raw-file identity without conflating its canonical seal.
            Entries = Array.AsReadOnly(entries.ToArray()); // Prevent writes through the exposed collection interface.
        } // Finish safe source-node construction without an authority or lease.
    } // Retain no raw corpus or entry object.

    internal sealed record DeclaredTeachingItemReadback( // Join declared metadata directly to its verified source and safe entry node.
        CodeDocumentationOneLessonTeachingItemV2 DeclaredItem, DeclaredTeachingSourceReadback Source, DeclaredTeachingEntryIdentity Entry); // Immutable links describe integrity rather than material admission.

    internal sealed class DeclaredTeachingCategoryReadback // Retain complete descriptive ordering without selecting runtime priority.
    { // Own nested ordered item views.
        internal string Category { get; } // Copy the declared category token without treating it as approved policy.
        internal IReadOnlyList<string> OrderedItemIds { get; } // Preserve every original identity cell through immutable owned storage.
        internal IReadOnlyList<DeclaredTeachingItemReadback> Items { get; } // Expose typed links to the same verified graph item nodes.
        internal DeclaredTeachingCategoryReadback(string category, IEnumerable<DeclaredTeachingItemReadback> items) // Construct safe descriptive bindings before complete graph publication.
        { // Capture item sequence once so its identities and links cannot diverge.
            Category = category; var owned = items.ToArray(); // Own the full supplied order without sorting.
            Items = Array.AsReadOnly(owned); // Reject mutations through the exposed typed item view.
            OrderedItemIds = Array.AsReadOnly(owned.Select(i => i.DeclaredItem.ItemId).ToArray()); // Own matching identity cells independently of mutable caller arrays.
        } // Complete an immutable category node without policy capability.
    } // End descriptive ordered category metadata.

    internal const string HistoryRequiredErrorCode = "ONE_LESSON_WHOLE_HISTORY_REQUIRED"; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    internal const string HistoryInvalidErrorCode = "ONE_LESSON_HISTORY_INVALID"; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.

    internal sealed record ContextHistoryPair(string SourceId, string ManifestFileSha256, // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        RepositoryOwnedDpoDatasetManifestPair Row, DpoPair Pair); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    internal sealed record ContextHistorySource(CodeDocumentationOneLessonSourceV2 Descriptor, RepositoryOwnedDpoDatasetManifest Manifest); // Preserve the full frozen request and ordered manifest alongside raw descriptor pins.

    internal sealed class DeclaredContextHistoryReadback // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    { // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        internal string SourceSha256 { get; } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        internal string HistorySha256 { get; } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        internal IReadOnlyList<ContextHistoryPair> Pairs { get; } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        internal IReadOnlyList<ContextHistorySource> Sources { get; } // Expose detached complete metadata, not only a hidden digest witness.
        private readonly string metadataSha256; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        private DeclaredContextHistoryReadback(string source, List<ContextHistoryPair> pairs, string metadata, IEnumerable<ContextHistorySource> sources) // Copy every metadata collection before the caller receives the graph.
        { // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            SourceSha256 = source; metadataSha256 = metadata; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            Sources = Array.AsReadOnly(sources.Select(s => new ContextHistorySource(s.Descriptor with { }, s.Manifest with // Keep the exact ordered manifest/request graph detached from staging lists.
            { OrderedPairs = Array.AsReadOnly(s.Manifest.OrderedPairs.Select(p => p with { }).ToArray()), RequestIdentity = s.Manifest.RequestIdentity with // Clone nested ordered lists; all scalar records are immutable.
                { PaidReferenceAnswers = s.Manifest.RequestIdentity.PaidReferenceAnswers is { } paid ? paid with // Raw-v2 has no paid reference; preserve its explicit complete source preimage.
                    { OrderedAnswerSha256 = Array.AsReadOnly(paid.OrderedAnswerSha256.ToArray()) } : null,
                  RawSource = s.Manifest.RequestIdentity.RawSource is { } raw ? raw with { } : null } })).ToArray()); // Do not expose mutable owner arrays.
            Pairs = Array.AsReadOnly(pairs.Select(p => p with { Pair = CopyHistoryPair(p.Pair), Row = p.Row with { } }).ToArray()); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            hash.AppendData(Encoding.ASCII.GetBytes("code-documentation-teacher-history-v1")); hash.AppendData(new byte[] { 0 }); // Preserve the reviewed ASCII-plus-NUL domain with an explicit byte-array overload.
            using var stream = new HistoryHashStream(hash); using var writer = new System.Text.Json.Utf8JsonWriter(stream); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            WriteHistory(writer); writer.Flush(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            HistorySha256 = Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        internal bool SameGraph(DeclaredContextHistoryReadback other) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            => SourceSha256 == other.SourceSha256 && HistorySha256 == other.HistorySha256 && metadataSha256 == other.metadataSha256; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        internal void WriteHistory(System.Text.Json.Utf8JsonWriter writer) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        { // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            writer.WriteStartArray(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            foreach (var row in Pairs) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            { // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                writer.WriteStartObject(); writer.WriteString("manifestFileSha256", row.ManifestFileSha256); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                writer.WritePropertyName("pair"); WriteHistoryPair(writer, row.Row.PairId, row.Pair); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                writer.WriteString("pairFileSha256", row.Row.PairFileSha256); writer.WriteString("pairId", row.Row.PairId); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                writer.WriteString("sourceId", row.SourceId); writer.WriteEndObject(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            writer.WriteEndArray(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        internal static OperationResult<DeclaredContextHistoryReadback> Open(CodeDocumentationOneLessonSourceSelection owner, // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            string root, FrozenReferenceCorpus original, IReadOnlyList<CodeDocumentationSavedModelAssessmentCaseResult> cases, // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            IReadOnlyList<CodeDocumentationSavedModelAssessmentCalibrationObservation> observations, CancellationToken token) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        { // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            var closed = DeclaredTeachingCorpusReadback.OriginalComparison.Capture(original, cases, observations, token); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            if (closed is null) return HistoryFailure<DeclaredContextHistoryReadback>(HistoryInvalidErrorCode); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            var metadata = new List<(CodeDocumentationOneLessonSourceV2 Source, RepositoryOwnedDpoDatasetAddress Address, RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult Open)>(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            var pairIds = new HashSet<string>(StringComparer.Ordinal); var total = 0; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            foreach (var source in owner.Sources) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            { // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                token.ThrowIfCancellationRequested(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(root, source.DomainSlug, source.DesignPatternSlug, source.DatasetVersion); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                if (!address.IsSuccess || address.Value is null) return address.MapError<DeclaredContextHistoryReadback>(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                var opened = RepositoryOwnedDpoDatasetManifestSealer.OpenManifestFileIdentityOnly(address.Value); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                if (!opened.IsSuccess || opened.Value is null) return opened.MapError<DeclaredContextHistoryReadback>(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                if (Readback(source, opened.Value.Manifest, opened.Value.ManifestFileSha256, opened.Value.ManifestFileByteLength) != source || // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                    HistoryMetadataStrings(source, opened.Value.Manifest).Any(s => closed.OverlapsText(s, token))) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                    return HistoryFailure<DeclaredContextHistoryReadback>(HistoryInvalidErrorCode); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                total = checked(total + opened.Value.Manifest.PairCount); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                if (opened.Value.Manifest.OrderedPairs.Any(p => !pairIds.Add(p.PairId))) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                    return HistoryFailure<DeclaredContextHistoryReadback>(HistoryInvalidErrorCode); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                metadata.Add((source, address.Value, opened.Value)); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            if (total != 2) return HistoryFailure<DeclaredContextHistoryReadback>(HistoryRequiredErrorCode); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            var pairs = new List<ContextHistoryPair>(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            foreach (var node in metadata) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            { // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                token.ThrowIfCancellationRequested(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                var full = RepositoryOwnedDpoDatasetManifestSealer.OpenSealedForManifestIntegrity(node.Address); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                if (!full.IsSuccess || full.Value is null) return full.MapError<DeclaredContextHistoryReadback>(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                if (!SameHistoryManifest(full.Value.Manifest, node.Open.Manifest)) return HistoryFailure<DeclaredContextHistoryReadback>(HistoryInvalidErrorCode); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                foreach (var row in full.Value.Manifest.OrderedPairs) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                { // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                    token.ThrowIfCancellationRequested(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                    var matches = full.Value.Pairs.Where(p => FileDpoPairStore.ComputeIdentity(p, schemaVersion: 5) == row.PairId).ToArray(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                    if (matches.Length != 1 || HistoryPairStrings(matches[0]).Any(s => closed.OverlapsText(s, token))) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                        return HistoryFailure<DeclaredContextHistoryReadback>(HistoryInvalidErrorCode); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                    var policy = TrainingDataGovernanceAdmission.ValidateDecision(matches[0].GovernanceDecision); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                    if (!policy.IsSuccess) return policy.MapError<DeclaredContextHistoryReadback>(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                    pairs.Add(new(node.Source.SourceId, node.Open.ManifestFileSha256, row, matches[0])); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                token.ThrowIfCancellationRequested(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                var after = RepositoryOwnedDpoDatasetManifestSealer.OpenManifestFileIdentityOnly(node.Address); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                if (!after.IsSuccess || after.Value is null) return after.MapError<DeclaredContextHistoryReadback>(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                if (Readback(node.Source, after.Value.Manifest, after.Value.ManifestFileSha256, after.Value.ManifestFileByteLength) != node.Source || // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                    !SameHistoryManifest(after.Value.Manifest, node.Open.Manifest)) return HistoryFailure<DeclaredContextHistoryReadback>(HistoryInvalidErrorCode); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            foreach (var node in metadata) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            { // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                token.ThrowIfCancellationRequested(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                var after = RepositoryOwnedDpoDatasetManifestSealer.OpenManifestFileIdentityOnly(node.Address); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                if (!after.IsSuccess || after.Value is null) return after.MapError<DeclaredContextHistoryReadback>(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                if (Readback(node.Source, after.Value.Manifest, after.Value.ManifestFileSha256, after.Value.ManifestFileByteLength) != node.Source || // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
                    !SameHistoryManifest(after.Value.Manifest, node.Open.Manifest)) return HistoryFailure<DeclaredContextHistoryReadback>(HistoryInvalidErrorCode); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            using var metadataHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            foreach (var node in metadata) foreach (var text in HistoryMetadataStrings(node.Source, node.Open.Manifest)) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            { var bytes = StrictUtf8.GetBytes(text); metadataHash.AppendData(Encoding.ASCII.GetBytes(bytes.Length.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":")); metadataHash.AppendData(bytes); } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            token.ThrowIfCancellationRequested(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            return OperationResult<DeclaredContextHistoryReadback>.Ok(new(owner.SourceSelectionSha256, pairs, Convert.ToHexString(metadataHash.GetHashAndReset()).ToLowerInvariant(), // Preserve raw metadata and its independent complete comparison digest.
                metadata.Select(n => new ContextHistorySource(n.Source, n.Open.Manifest)))); // Retain every actual frozen request/ordered row for recipient inspection.
        } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.

    internal OperationResult<DeclaredContextHistoryReadback> OpenDeclaredContextHistoryExcludingOriginal( // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        string root, FrozenReferenceCorpus original, IReadOnlyList<CodeDocumentationSavedModelAssessmentCaseResult> cases, // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        IReadOnlyList<CodeDocumentationSavedModelAssessmentCalibrationObservation> observations, CancellationToken token = default) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        => DeclaredContextHistoryReadback.Open(this, root, original, cases, observations, token); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.

    private static OperationResult<T> HistoryFailure<T>(string code) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        => OperationResult<T>.Fail("The entire declared historical context is required and must match its genuine owners.", code); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.

    private static bool SameHistoryManifest(RepositoryOwnedDpoDatasetManifest left, RepositoryOwnedDpoDatasetManifest right) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        => left.Schema == right.Schema && left.Context == right.Context && left.Domain == right.Domain && // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            left.DesignPattern == right.DesignPattern && left.DatasetVersion == right.DatasetVersion && // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            left.ManifestSha256 == right.ManifestSha256 && left.PairCount == right.PairCount && // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            left.OrderedPairs.SequenceEqual(right.OrderedPairs) && // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            HistoryRequestStrings(left.RequestIdentity).SequenceEqual(HistoryRequestStrings(right.RequestIdentity), StringComparer.Ordinal); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.

    private static IEnumerable<string> HistoryRequestStrings(RepositoryOwnedDpoDatasetRequestIdentity r) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    { // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        if (r.Schema == RepositoryOwnedDpoDatasetManifestSealer.RawRequestIdentitySchema)
        {
            // Retain every new field, rather than binding raw history to an opaque digest alone.
            yield return Convert.ToBase64String(RepositoryOwnedDpoDatasetManifestSealer.CopyCanonicalRequestIdentityBytes(r));
            yield break;
        }
        yield return r.Schema; yield return r.RequestIdentitySha256; yield return r.PaidReferenceAnswers!.AnswerCount.ToString(System.Globalization.CultureInfo.InvariantCulture); // Legacy history framing remains byte-for-byte unchanged.
        yield return r.PaidReferenceAnswers.CanonicalListSha256;
        foreach (var s in r.PaidReferenceAnswers.OrderedAnswerSha256) yield return s;
        yield return r.OpenTeacher.Family; yield return r.OpenTeacher.ModelId; yield return r.OpenTeacher.ModelVersion; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        yield return r.OpenTeacher.ModelArtifactDigestKind; yield return r.OpenTeacher.ModelArtifactSha256; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        yield return r.GenerationPolicy.StableName; yield return r.GenerationPolicy.Version; yield return r.GenerationPolicy.ConfigSha256; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        yield return r.Creator.StableName; yield return r.Creator.Version; yield return r.Creator.EffectiveConfigSha256; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    private static IEnumerable<string> HistoryMetadataStrings(CodeDocumentationOneLessonSourceV2 source, RepositoryOwnedDpoDatasetManifest manifest) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    { // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        yield return source.SourceId; yield return source.Role; yield return source.DomainSlug; yield return source.DesignPatternSlug; yield return source.DatasetVersion; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        yield return source.ManifestFileSha256; yield return source.ManifestFileByteLength.ToString(System.Globalization.CultureInfo.InvariantCulture); yield return source.SealedManifestSha256; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        yield return manifest.Schema; yield return manifest.Context; yield return manifest.Domain; yield return manifest.DesignPattern; yield return manifest.DatasetVersion; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        yield return manifest.ManifestSha256; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        foreach (var s in HistoryRequestStrings(manifest.RequestIdentity)) yield return s; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        foreach (var p in manifest.OrderedPairs) { yield return p.PairId; yield return p.RepositoryRelativePairFilePath; yield return p.PairFileSha256; } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    private static IEnumerable<string> HistoryProvenanceStrings(TrainingTeacherProvenance? p) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    { // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        if (p is null) yield break; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        yield return p.SourceKind.ToString(); yield return p.Family?.ToString() ?? ""; yield return p.AccessMode?.ToString() ?? ""; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        yield return p.HostingProvider?.ToString() ?? ""; yield return p.ModelLicense?.ToString() ?? ""; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        yield return p.ModelId; yield return p.ModelVersion; yield return p.RequestId; yield return p.ModelArtifactDigestKind; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        yield return p.ModelArtifactDigest; yield return p.TrainingContentSha256; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    private static IEnumerable<string> HistoryPairStrings(DpoPair p) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    { // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        yield return p.Prompt; yield return p.Chosen; yield return p.Rejected; yield return p.Domain; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        yield return p.CapturedAt.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        foreach (var s in HistoryProvenanceStrings(p.TeacherProvenance)) yield return s; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        if (p.GovernanceDecision is not null) foreach (var s in p.GovernanceDecision.EvidenceRefs) yield return s; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        foreach (var side in new[] { p.ChosenAuthorship, p.RejectedAuthorship }) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        { if (side is null) continue; yield return side.AuthorKind.ToString(); foreach (var s in HistoryProvenanceStrings(side.OpenTeacherPassport)) yield return s; } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    private static DpoPair CopyHistoryPair(DpoPair p) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        => p with { TeacherProvenance = p.TeacherProvenance is null ? null : p.TeacherProvenance with { }, // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            GovernanceDecision = p.GovernanceDecision is null ? null : new TrainingDataGovernanceDecision(p.GovernanceDecision.LicenseAccepted, p.GovernanceDecision.IsQuarantined, p.GovernanceDecision.EvidenceRefs.ToArray()), // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            ChosenAuthorship = p.ChosenAuthorship is null ? null : new(p.ChosenAuthorship.AuthorKind, p.ChosenAuthorship.OpenTeacherPassport is null ? null : p.ChosenAuthorship.OpenTeacherPassport with { }), // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
            RejectedAuthorship = p.RejectedAuthorship is null ? null : new(p.RejectedAuthorship.AuthorKind, p.RejectedAuthorship.OpenTeacherPassport is null ? null : p.RejectedAuthorship.OpenTeacherPassport with { }) }; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.

    private static void WriteHistoryPair(System.Text.Json.Utf8JsonWriter w, string id, DpoPair p) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    { // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        w.WriteStartObject(); w.WriteString("capturedAt", p.CapturedAt.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture)); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        w.WriteString("chosen", p.Chosen); w.WriteString("domain", p.Domain); w.WriteString("pairId", id); w.WriteString("prompt", p.Prompt); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        w.WriteString("recordDomain", "xiigen.dpo-pair.v5"); w.WriteString("rejected", p.Rejected); w.WriteNumber("schemaVersion", 5); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        w.WriteStartObject("sideAuthorship"); w.WritePropertyName("chosen"); WriteHistoryAuthorship(w, p.ChosenAuthorship); w.WritePropertyName("rejected"); WriteHistoryAuthorship(w, p.RejectedAuthorship); w.WriteEndObject(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        w.WriteStartObject("teacherProvenance"); w.WritePropertyName("governanceDecision"); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        if (p.GovernanceDecision is null) w.WriteNullValue(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        else { w.WriteStartObject(); w.WriteStartArray("evidenceRefs"); foreach (var s in p.GovernanceDecision.EvidenceRefs) w.WriteStringValue(s); w.WriteEndArray(); w.WriteBoolean("isQuarantined", p.GovernanceDecision.IsQuarantined); w.WriteBoolean("licenseAccepted", p.GovernanceDecision.LicenseAccepted); w.WriteEndObject(); } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        w.WritePropertyName("provenance"); WriteHistoryProvenance(w, p.TeacherProvenance); w.WriteEndObject(); w.WriteEndObject(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    private static void WriteHistoryAuthorship(System.Text.Json.Utf8JsonWriter w, DpoPairSideAuthorship? a) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    { // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        if (a is null) { w.WriteNullValue(); return; } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        w.WriteStartObject(); w.WriteString("authorKind", a.AuthorKind.ToString()); w.WritePropertyName("openTeacherPassport"); WriteHistoryProvenance(w, a.OpenTeacherPassport); w.WriteEndObject(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    private static void WriteHistoryProvenance(System.Text.Json.Utf8JsonWriter w, TrainingTeacherProvenance? p) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    { // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        if (p is null) { w.WriteNullValue(); return; } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        w.WriteStartObject(); WriteHistoryNullable(w, "accessMode", p.AccessMode?.ToString()); w.WriteBoolean("closedProprietaryModel", p.ClosedProprietaryModel); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        WriteHistoryNullable(w, "family", p.Family?.ToString()); WriteHistoryNullable(w, "hostingProvider", p.HostingProvider?.ToString()); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        w.WriteNumber("invocationCount", p.InvocationCount); w.WriteString("modelArtifactDigest", p.ModelArtifactDigest); w.WriteString("modelArtifactDigestKind", p.ModelArtifactDigestKind); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        w.WriteString("modelId", p.ModelId); WriteHistoryNullable(w, "modelLicense", p.ModelLicense?.ToString()); w.WriteString("modelVersion", p.ModelVersion); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        w.WriteBoolean("providerTermsAllowTraining", p.ProviderTermsAllowTraining); w.WriteBoolean("providerTermsChecked", p.ProviderTermsChecked); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        w.WriteString("requestId", p.RequestId); w.WriteString("sourceKind", p.SourceKind.ToString()); w.WriteBoolean("teacherOutputUsedForTraining", p.TeacherOutputUsedForTraining); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        w.WriteString("trainingContentSha256", p.TrainingContentSha256); w.WriteEndObject(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    private static void WriteHistoryNullable(System.Text.Json.Utf8JsonWriter w, string name, string? value) // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    { if (value is null) w.WriteNull(name); else w.WriteString(name, value); } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    private sealed class HistoryHashStream(IncrementalHash hash) : Stream // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    { // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        public override bool CanRead => false; public override bool CanSeek => false; public override bool CanWrite => true; // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        public override long Length => throw new NotSupportedException(); public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        public override void Flush() { } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        public override void Write(byte[] buffer, int offset, int count) => hash.AppendData(buffer, offset, count); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        public override void Write(ReadOnlySpan<byte> buffer) => hash.AppendData(buffer); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
        public override void SetLength(long value) => throw new NotSupportedException(); // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.
    } // Keep whole historical owner bytes ordered and disjoint; this data readback grants no training or Native acceptance.


    private static CodeDocumentationOneLessonSourceV2 Readback(
        CodeDocumentationOneLessonSourceV2 selected, RepositoryOwnedDpoDatasetManifest manifest,
        string rawHash, long rawLength)
        => new(selected.SourceId, selected.Role, manifest.Domain, manifest.DesignPattern,
            manifest.DatasetVersion, rawHash, rawLength, manifest.ManifestSha256);

    private static bool Token(string? value)
        => value is { Length: >= 1 and <= 128 } &&
           (value[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9') &&
           value.All(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9'
               or '.' or '_' or ':' or '-');

    private static OperationResult<CodeDocumentationOneLessonSourceSelection> Invalid()
        => OperationResult<CodeDocumentationOneLessonSourceSelection>.Fail(
            "The complete startup source selection is invalid.", InvalidErrorCode);

    private static OperationResult<CodeDocumentationOneLessonSourceSelectionOpenResult> InvalidOpen()
        => OperationResult<CodeDocumentationOneLessonSourceSelectionOpenResult>.Fail(
            "Fresh dataset readback does not prove the unique startup-selected pair.", InvalidErrorCode);
}

internal sealed record CodeDocumentationOneLessonSourceSelectionOpenResult(
    CodeDocumentationOneLessonSourceSelection Selection, string SourceId,
    RepositoryOwnedDpoDatasetManifestPair SelectedRow, DpoPair Pair);
