// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning; // Keep paid-reference sealing and strict reopen in the existing ContinuousLearning owner.

using System.Diagnostics; // Measure the bounded process-gate wait against actual elapsed time.
using System.Globalization; // Parse exact UTC timestamps and invariant numeric identities.
using System.Security.Cryptography; // Hash exact stable bytes and compare complete canonical byte commitments.
using System.Text; // Support exact encoding of persisted identity bytes.
using System.Text.Json; // Parse bounded strict owner JSON and serialize its fixed legacy field order.
using AgentInfrastructure; // Return strict corpus admission and filesystem failures through shared operation results.
using AgentInfrastructure.Logical; // Use the existing secured local-file custody boundary.

/// <summary>
/// Immutable manifest for one repository-owned frozen paid-reference corpus.
/// </summary>
public sealed record RepositoryOwnedReferenceCorpusManifest( // Represent the complete immutable seal for one frozen paid-reference corpus.
    string Schema, // Pin schema discriminator in this complete immutable value.
    string Context, // Pin paid-reference custody context in this complete immutable value.
    string Domain, // Pin corpus domain in this complete immutable value.
    string Subject, // Pin corpus subject in this complete immutable value.
    string CorpusVersion, // Pin frozen corpus version in this complete immutable value.
    string CorpusName, // Pin human corpus name in this complete immutable value.
    string FrozenAtUtc, // Pin corpus freeze timestamp in this complete immutable value.
    int EntryCount, // Pin complete payload entry count in this complete immutable value.
    string CorpusSha256, // Pin whole-corpus canonical identity in this complete immutable value.
    string RepositoryRelativeCorpusDirectory, // Pin repository-relative corpus directory in this complete immutable value.
    string RepositoryRelativePayloadPath, // Pin repository-relative payload locator in this complete immutable value.
    long PayloadByteLength, // Pin exact raw payload length in this complete immutable value.
    string PayloadSha256, // Pin exact raw payload digest in this complete immutable value.
    string RepositoryRelativeManifestPath, // Pin repository-relative manifest locator in this complete immutable value.
    string ManifestSha256); // Pin sealed manifest self-hash in this complete immutable value.

/// <summary>
/// Freshly reopened and verified repository-owned frozen reference corpus.
/// </summary>
public sealed record RepositoryOwnedReferenceCorpusOpenResult( // Return the complete sealed manifest with its factory-reconstructed frozen corpus.
    RepositoryOwnedReferenceCorpusManifest Manifest, // Return the validated complete seal accompanying this corpus.
    FrozenReferenceCorpus Corpus); // Return the entire factory-reconstructed corpus accompanying that seal.

/// <summary>
/// Describes one fully verified open, including the exact manifest file bytes.
/// This snapshot is not a retained lease or permission for a later effect.
/// </summary>
internal sealed record RepositoryOwnedReferenceCorpusFileIdentityOpenResult( // Carry strict raw manifest-file identity separately from the sealed self-hash.
    RepositoryOwnedReferenceCorpusManifest Manifest, // Return the seal parsed from the same stable manifest-file read.
    FrozenReferenceCorpus Corpus, // Return the entire corpus reconstructed under that seal.
    string ManifestFileSha256, // Pin raw manifest-file digest in this complete immutable value.
    long ManifestFileByteLength); // Pin raw manifest-file length in this complete immutable value.

/// <summary>
/// Writes and reopens sealed repository-owned paid-reference corpus snapshots.
/// </summary>
public static partial class RepositoryOwnedReferenceCorpusManifestSealer // Own no-overwrite corpus sealing and whole-corpus strict reconstruction.
{ // Own sealed corpus publication, strict parsing and complete domain reconstruction.
    public const string InvalidErrorCode = // Expose invalid input or strict-format rejection through the stable owner error code.
        "REPOSITORY_OWNED_REFERENCE_CORPUS_MANIFEST_INVALID"; // Identify invalid corpus input and strict representation failures.
    public const string ConflictErrorCode = // Expose no-overwrite disagreement through the stable conflict error code.
        "REPOSITORY_OWNED_REFERENCE_CORPUS_MANIFEST_CONFLICT"; // Identify a conflicting previously sealed corpus at the same address.
    public const string BusyErrorCode = // Expose bounded process-gate timeout through the stable busy error code.
        "REPOSITORY_OWNED_REFERENCE_CORPUS_MANIFEST_BUSY"; // Identify contention that outlasts the seal gate wait.
    public const string UnavailableErrorCode = // Expose failed filesystem custody or access through the stable unavailable code.
        "REPOSITORY_OWNED_REFERENCE_CORPUS_MANIFEST_UNAVAILABLE"; // Identify storage or custody failures without admitting a corpus.

    public const string CurrentSchema = // Declare the exact supported manifest schema for strict readback.
        "repository-owned-reference-corpus-manifest-v1"; // Distinguish this manifest format from other JSON objects.
    public const string CurrentContext = // Declare the paid-reference corpus context admitted by this owner.
        "continuous-learning-frozen-paid-reference-corpus-v1"; // Limit the manifest to the frozen paid-reference corpus context.
    public const string CurrentPayloadSchema = // Declare the exact supported payload schema for strict reconstruction.
        "repository-owned-reference-corpus-payload-v1"; // Distinguish the complete corpus payload format from the manifest format.

    public const string RawUtf8Schema = "repository-owned-reference-corpus-manifest-v2";
    public const string RawUtf8Context = "continuous-learning-frozen-paid-reference-corpus-v2";
    public const string RawUtf8PayloadSchema = "repository-owned-reference-corpus-payload-v2";
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public const string PayloadFileName = "corpus.json"; // Fix the payload filename used by the typed address and strict manifest checks.

    private const string PayloadTempPrefix = ".corpus."; // Mark only payload staging files created by this sealer.
    private const string ManifestTempPrefix = ".manifest."; // Mark only manifest staging files created by this sealer.
    private const string TempSuffix = ".tmp"; // Complete the owner-specific temporary filenames for bounded cleanup.
    private const int FileBufferSize = 81920; // Bound each file-copy buffer while processing the permitted payload size.
    private const int HashHexLength = 64; // Require all bare owner digests to contain exactly 64 lowercase hex characters.
    private const int ManifestJsonMaxDepth = 8; // Bound strict manifest nesting to eight levels.
    private const int PayloadJsonMaxDepth = 12; // Allow nested payload entries/provenance within twelve parser levels.
    private const int MaximumEntryCount = 10_000; // Reject manifests/payloads declaring more than 10,000 entries.
    private const int MaximumIdentityTextLength = 256; // Cap each persisted identity string at 256 characters.
    private const int MaximumUtcTextLength = 64; // Cap persisted UTC strings before exact timestamp parsing.
    private const int MaximumRepositoryRelativePathLength = 1024; // Cap repository-relative locator strings before confinement checks.
    private const int MaximumContentTextLength = 131_072; // Cap each persisted input or reference answer at 131,072 characters.
    private const int MaximumCanonicalTextLength = 262_144; // Cap complete canonical representation strings at 262,144 characters.
    internal const long MaximumManifestBytes = 2L * 1024L * 1024L; // Limit raw manifest reads and publication to 2 MiB.
    internal const long MaximumPayloadBytes = 32L * 1024L * 1024L; // Limit raw payload reads and publication to 32 MiB.
    private static readonly TimeSpan DefaultSealProcessGateTimeout = // Bound process-local seal contention before returning busy.
        TimeSpan.FromSeconds(10); // Wait at most ten seconds for the per-root seal gate.
    private static readonly UnixFileMode OwnerDirectoryMode = // Define the Unix mode needed for private corpus-directory custody.
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute; // Allow only the owning user to read, write and traverse that directory.
    private static readonly object ProcessGateRegistrySync = new(); // Serialize process-gate registry membership and reference-count updates.
    private static readonly Dictionary<string, ProcessGateEntry> ProcessGateRegistry = // Share one process-local seal semaphore per canonical corpus root.
        new(OperatingSystem.IsWindows() // Choose the existing platform comparison for canonical corpus-root gate keys.
            ? StringComparer.OrdinalIgnoreCase // Share one gate for Windows root paths that differ only in case.
            : StringComparer.Ordinal); // Keep distinct case-sensitive root identities on non-Windows platforms.

    private const string SchemaProperty = "schema"; // Use this exact wire spelling for the format discriminator.
    private const string ContextProperty = "context"; // Use this exact wire spelling for the paid-reference corpus context.
    private const string DomainProperty = "domain"; // Use this exact wire spelling for the corpus domain slug.
    private const string SubjectProperty = "subject"; // Use this exact wire spelling for the corpus subject slug.
    private const string CorpusVersionProperty = "corpusVersion"; // Use this exact wire spelling for the frozen corpus version.
    private const string CorpusNameProperty = "corpusName"; // Use this exact wire spelling for the factory-derived corpus display name.
    private const string FrozenAtUtcProperty = "frozenAtUtc"; // Use this exact wire spelling for the canonical corpus freeze timestamp.
    private const string EntryCountProperty = "entryCount"; // Use this exact wire spelling for the complete declared entry count.
    private const string CorpusSha256Property = "corpusSha256"; // Keep the exact corpus sha256 wire member name fixed across strict readback and persistence.
    private const string RepositoryRelativeCorpusDirectoryProperty = // Define the exact repository relative corpus directory wire member name once for both strict parsing and persistence.
        "repositoryRelativeCorpusDirectory"; // Use this exact wire spelling for the typed corpus directory locator.
    private const string RepositoryRelativePayloadPathProperty = // Define the exact repository relative payload path wire member name once for both strict parsing and persistence.
        "repositoryRelativePayloadPath"; // Use this exact wire spelling for the confined payload locator.
    private const string PayloadByteLengthProperty = "payloadByteLength"; // Use this exact wire spelling for the exact raw payload length.
    private const string PayloadSha256Property = "payloadSha256"; // Keep the exact payload sha256 wire member name fixed across strict readback and persistence.
    private const string RepositoryRelativeManifestPathProperty = // Define the exact repository relative manifest path wire member name once for both strict parsing and persistence.
        "repositoryRelativeManifestPath"; // Use this exact wire spelling for the typed manifest locator.
    private const string ManifestSha256Property = "manifestSha256"; // Keep the exact manifest sha256 wire member name fixed across strict readback and persistence.
    private const string CanonicalRepresentationProperty = // Define the exact canonical representation wire member name once for both strict parsing and persistence.
        "canonicalRepresentation"; // Use this exact wire spelling for the complete owner canonical representation.
    private const string EntriesProperty = "entries"; // Use this exact wire spelling for the complete persisted entry array.
    private const string EntryIdProperty = "entryId"; // Use this exact wire spelling for the canonical frozen-entry identity.
    private const string OwnerInputProperty = "ownerInput"; // Use this exact wire spelling for the original owner input.
    private const string PaidModelReferenceAnswerProperty = // Define the exact paid model reference answer wire member name once for both strict parsing and persistence.
        "paidModelReferenceAnswer"; // Use this exact wire spelling for the closed paid-reference answer.
    private const string PartitionProperty = "partition"; // Use this exact wire spelling for the explicit train or held-out partition.
    private const string SourceEntityFingerprintProperty = // Define the exact source entity fingerprint wire member name once for both strict parsing and persistence.
        "sourceEntityFingerprint"; // Use this exact wire spelling for the independent source fingerprint.
    private const string PaidReferenceProvenanceProperty = // Define the exact paid reference provenance wire member name once for both strict parsing and persistence.
        "paidReferenceProvenance"; // Use this exact wire spelling for the complete nested paid provenance.
    private const string ProviderRefProperty = "providerRef"; // Use this exact wire spelling for the paid-reference provider identity.
    private const string ModelIdProperty = "modelId"; // Use this exact wire spelling for the paid-reference model identity.
    private const string ModelVersionProperty = "modelVersion"; // Use this exact wire spelling for the paid-reference model version.
    private const string RequestIdProperty = "requestId"; // Use this exact wire spelling for the paid-reference request identity.
    private const string CapturedAtUtcProperty = "capturedAtUtc"; // Use this exact wire spelling for the canonical paid-reference capture timestamp.
    private const string ReferenceAnswerSha256Property = // Define the exact reference answer sha256 wire member name once for both strict parsing and persistence.
        "referenceAnswerSha256"; // Use this exact wire spelling for the closed reference-answer digest.
    private const string ProvenanceSha256Property = "provenanceSha256"; // Keep the exact provenance sha256 wire member name fixed across strict readback and persistence.
    private const string ReferenceAnswerRawUtf8Sha256Property = "referenceAnswerRawUtf8Sha256";

    /// <summary>
    /// Alias for callers that name the operation as sealing rather than persisting.
    /// </summary>
    public static OperationResult<RepositoryOwnedReferenceCorpusManifest> Seal( // Route sealing through the same payload-first, manifest-last owner.
        RepositoryOwnedReferenceCorpusAddress address, // Use the admitted typed corpus address for complete input validation.
        FrozenReferenceCorpus corpus) // Use the reconstructed frozen corpus for complete input validation.
        => PersistAndSeal(address, corpus); // Validate address, custody and process ownership before publishing corpus bytes.

    /// <summary>
    /// Persists corpus.json first and manifest.json last under the supplied address.
    /// </summary>
    public static OperationResult<RepositoryOwnedReferenceCorpusManifest> // Validate address, custody and process ownership before publishing corpus bytes.
        PersistAndSeal( // Name the persist and seal operation whose typed parameters follow.
            RepositoryOwnedReferenceCorpusAddress address, // Use the admitted typed corpus location for custody checks and publication.
            FrozenReferenceCorpus corpus) // Seal the entire factory-owned frozen corpus without dropping entries.
    { // Hold the process gate while checking custody and sealing payload before manifest.
        var validation = ValidateAddressAndCorpus(address, corpus); // Require the corpus domain, version and identity to match its typed repository address.
        if (!validation.IsSuccess) // Require success of the address or corpus admission result before using its value.
            return validation.MapError<RepositoryOwnedReferenceCorpusManifest>(); // Propagate failure from the address or corpus admission result without returning an admitted value.

        try // Bound nonfatal failures around persist and seal.
        { // Hold the process gate while checking custody and sealing payload before manifest while retaining the existing exception boundary.
            if (corpus.IsNativeCapture && !EnsureNativeCaptureCustodyRoot(address)) return Unavailable<RepositoryOwnedReferenceCorpusManifest>(); // Allocate only this new native root under strict private custody; existing unsafe roots are refused.
            var canonicalCorpusRoot = CanonicalRoot(address.CorpusRoot); // Normalize a root consistently for confinement and process-gate identity.
            var timeoutBudget = Stopwatch.StartNew(); // Measure actual elapsed wait time for the bounded process gate.
            var processLeaseResult = AcquireProcessSealLease( // Bound process-local sealing contention under the canonical corpus-root gate.
                canonicalCorpusRoot, // Choose the canonical corpus root whose gate this attempt must acquire.
                timeoutBudget); // Charge gate waiting against this running elapsed-time budget.
            if (!processLeaseResult.IsSuccess || processLeaseResult.Value is null) // Require success of the bounded gate acquisition result before using its value.
                return processLeaseResult // Preserve bounded gate acquisition result without manufacturing replacement authority.
                    .MapError<RepositoryOwnedReferenceCorpusManifest>(); // Propagate the preceding original owner failure without returning a successful snapshot.

            using var processLease = processLeaseResult.Value; // Hold the root-specific process gate until sealing and final strict readback finish.
            // Перед записью создаём/проверяем каталог, но ещё не публикуем
            // manifest: без payload manifest был бы ложным утверждением.
            var rootReady = EnsureWritableCorpusRoot(address); // Require a confined, non-reparse corpus directory with safe owner permissions.
            if (!rootReady.IsSuccess) // Require success of the safe writable-root check before using its value.
                return rootReady.MapError< // Propagate failure from the safe writable-root check without returning an admitted value.
                    RepositoryOwnedReferenceCorpusManifest>(); // Complete the original writable-root failure projection without admitting a manifest.

            if (!corpus.IsNativeCapture) // Known-model schemas retain their existing broad clone-custody preparation; native origin must never enter it.
            { // Preserve the legacy preparer and its result contract only for legacy corpus publication.
                var custodyContext = RepositoryOwnedTrainingArtifactCloneCustodyPreparer // The existing legacy preparation may normalize other repository artifact modes on Linux.
                    .PreparePublicContextForStrictReaders(address.RepositoryRoot); // No native operation calls this broad repository mutator.
                if (!custodyContext.IsSuccess || custodyContext.Value is null || !custodyContext.Value.CoversReferenceCorpusRoot(address.RepositoryRoot)) // Preserve the actual legacy custody coverage requirement.
                    return Unavailable<RepositoryOwnedReferenceCorpusManifest>(); // Legacy preparation failure remains unavailable.
            } // Native custody is verified on its exact leaf and opened files without repair.

            rootReady = EnsureWritableCorpusRoot(address); // Require a confined, non-reparse corpus directory with safe owner permissions.
            var revalidation = ValidateAddressAndCorpus(address, corpus); // Require the corpus domain, version and identity to match its typed repository address.
            if (!rootReady.IsSuccess // Require success of the safe writable-root check before using its value.
                || !revalidation.IsSuccess // Require success of the post-custody address recheck before using its value.
                || !PathsEqual(address.CorpusRoot, canonicalCorpusRoot) // Reject a corpus root that changed identity after custody preparation.
                || AnyExistingPathSegmentIsReparsePoint(address.CorpusRoot) // Reject reparse traversal inside the resolved corpus boundary.
                || !HasSafeUnixDirectoryMode(address.CorpusRoot) // Require owner-only corpus-directory access on Unix.
                || corpus.IsNativeCapture && !SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(address.CorpusRoot)) // Native leaf must retain actual strict custody after process-gate acquisition.
            { // Reject changed address, unsafe root or failed post-custody revalidation before publishing.
                if (!rootReady.IsSuccess) // Require success of the safe writable-root check before using its value.
                    return rootReady.MapError< // Propagate failure from the safe writable-root check without returning an admitted value.
                        RepositoryOwnedReferenceCorpusManifest>(); // Complete the post-custody writable-root failure projection without admitting a manifest.
                if (!revalidation.IsSuccess) // Require success of the post-custody address recheck before using its value.
                    return revalidation.MapError< // Propagate failure from the post-custody address recheck without returning an admitted value.
                        RepositoryOwnedReferenceCorpusManifest>(); // Complete the post-custody address revalidation failure projection without admitting a manifest.
                return Unavailable<RepositoryOwnedReferenceCorpusManifest>(); // Report failed custody or nonfatal filesystem access without admission.
            } // Reject changed address, unsafe root or failed post-custody revalidation before publishing.

            return PersistAndSealUnderProcessLease(address, corpus); // Preserve idempotent sealed bytes and publish payload before its manifest.
        } // Hold the process gate while checking custody and sealing payload before manifest while retaining the existing exception boundary.
        catch (Exception exception) // Convert only the permitted failure category into this owner result; fatal failures remain outside this catch.
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Convert only the permitted failure category into this owner result; fatal failures remain outside this catch.
        { // Return the existing PersistAndSeal failure result after the permitted nonfatal exception.
            return Unavailable<RepositoryOwnedReferenceCorpusManifest>(); // Report failed custody or nonfatal filesystem access without admission.
        } // Return the existing PersistAndSeal failure result after the permitted nonfatal exception.
    } // Hold the process gate while checking custody and sealing payload before manifest.

    /// <summary>
    /// Alias for callers that name the operation as a strict open.
    /// </summary>
    public static OperationResult<RepositoryOwnedReferenceCorpusOpenResult> Open( // Route callers through strict sealed-corpus reconstruction.
        RepositoryOwnedReferenceCorpusAddress address) // Use the admitted typed corpus address for complete input validation.
        => OpenSealed(address); // Project the public corpus snapshot from strict file-identity readback.

    /// <summary>
    /// Reopens a sealed corpus from disk and reconstructs it through public factories.
    /// </summary>
    public static OperationResult<RepositoryOwnedReferenceCorpusOpenResult> // Project the public corpus snapshot from strict file-identity readback.
        OpenSealed(RepositoryOwnedReferenceCorpusAddress address) // Strictly reopen the complete corpus through this admitted typed address.
        => ProjectOpenResult(OpenSealedFileIdentity(address)); // Require validated address and reference-corpus custody before strict readback.

    /// <summary>
    /// Reopens and reconstructs the whole sealed corpus before returning raw
    /// manifest-file identity separately from its canonical sealed self-hash.
    /// </summary>
    internal static OperationResult<RepositoryOwnedReferenceCorpusFileIdentityOpenResult> // Require validated address and reference-corpus custody before strict readback.
        OpenSealedFileIdentity(RepositoryOwnedReferenceCorpusAddress address) // Strictly reopen this address and retain its raw manifest identity.
    { // Validate typed address and current custody before strict corpus readback.
        var validation = ValidateAddress(address); // Reject an address that no longer matches the canonical reference-corpus layout.
        if (!validation.IsSuccess) // Require success of the address or corpus admission result before using its value.
            return validation.MapError<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>(); // Propagate failure from the address or corpus admission result without returning an admitted value.

        var discovery = ReadStrictManifest(address.ManifestPath); // Detect the actual sealed origin from bounded stable bytes and a verified complete self-hash before any custody repair.
        if (!discovery.IsSuccess || discovery.Value is null) // Missing/malformed manifests now fail before broad preparation, including legacy malformed inputs.
            return discovery.MapError<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>(); // Preserve the strict read failure without changing unrelated artifact modes.
        if (discovery.Value.Manifest.Schema == NativeCaptureSchema || discovery.Value.Manifest.Context == NativeCaptureContext) // Native namespace selection comes from the actual manifest, never a caller-provided scalar.
            return OpenNativeCaptureWithoutPreparation(address, discovery.Value); // Require the complete native tuple/address and exact discovered file pin before read-only native core reopen.

        var custodyContext = RepositoryOwnedTrainingArtifactCloneCustodyPreparer // Valid known-model schemas retain their existing clone-custody preparation.
            .PreparePublicContextForStrictReaders(address.RepositoryRoot); // Prepare custody only for the supplied fixed repository root.
        return custodyContext.IsSuccess // Preserve strict-reader custody context without manufacturing replacement authority.
               && custodyContext.Value is not null // Require a non-null successfully prepared custody context.
               && custodyContext.Value.CoversReferenceCorpusRoot( // Require custody coverage for this exact repository reference-corpus root.
                   address.RepositoryRoot) // Require that context to cover this exact repository reference-corpus root.
            ? OpenSealedFileIdentityCore(address) // Verify raw manifest, bounded payload and reconstructed provenance before returning identities.
            : Unavailable<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>(); // Report failed custody or nonfatal filesystem access without admission.
    } // Validate typed address and current custody before strict corpus readback.

    private static OperationResult<RepositoryOwnedReferenceCorpusManifest> // Preserve idempotent sealed bytes and publish payload before its manifest.
        PersistAndSealUnderProcessLease( // Name the persist and seal under process lease operation whose typed parameters follow.
            RepositoryOwnedReferenceCorpusAddress address, // Publish only beneath the admitted typed corpus address.
            FrozenReferenceCorpus corpus) // Project this entire frozen corpus into its payload and seal.
    { // Preserve identical seals and publish complete payload bytes before manifest bytes.
        var payload = BuildPayload(corpus); // Preserve the complete frozen corpus representation and every entry in its payload.
        var payloadBytes = WriteCanonicalPayloadBytes(payload); // Emit the existing complete canonical corpus payload.
        var manifest = BuildManifest(address, corpus, payloadBytes); // Commit canonical corpus and payload identities into the sealed manifest.
        var manifestBytes = WriteCanonicalManifestBytes(manifest); // Serialize every sealed manifest field, including the final self-hash, for publication.
        var payloadPath = PayloadPath(address); // Resolve the fixed payload file beneath the supplied corpus root.

        if (File.Exists(address.ManifestPath)) // Distinguish already-published bytes from the owned staging publication path.
        { // Keep an already sealed corpus immutable; return it only for identical full content.
            var existing = OpenSealedCore(address); // Reuse strict readback for seal-time idempotency checks.
            if (!existing.IsSuccess || existing.Value is null) // Require success of the already-sealed owner snapshot before using its value.
                return existing // Preserve already-sealed owner snapshot without manufacturing replacement authority.
                    .MapError<RepositoryOwnedReferenceCorpusManifest>(); // Propagate the preceding original owner failure without returning a successful snapshot.
            // Повтор с теми же байтами идемпотентен; другой corpus с тем же
            // version не перезаписывается и не маскируется свежим manifest.
            return ManifestsEqual(existing.Value.Manifest, manifest) // Compare every sealed manifest identity required for idempotent publication.
                   && PayloadCorpusMatches(existing.Value.Corpus, corpus) // Require exact canonical corpus equality for idempotent publication.
                   && CanonicalBytesEqual( // Compare the complete canonical byte sequences without summary substitution.
                       WriteCanonicalManifestBytes(existing.Value.Manifest), // Compare the existing sealed manifest bytes, including its final self-hash, with the proposed bytes.
                       manifestBytes) // Require the proposed complete manifest bytes to equal the existing sealed bytes.
                ? OperationResult<RepositoryOwnedReferenceCorpusManifest>.Ok( // Return the validated typed owner value after the preceding admission checks.
                    existing.Value.Manifest) // Return the existing verified manifest only when all idempotency comparisons match.
                : Conflict<RepositoryOwnedReferenceCorpusManifest>(); // Report conflicting sealed bytes without overwriting owner artifacts.
        } // Keep an already sealed corpus immutable; return it only for identical full content.

        if (File.Exists(payloadPath)) // Distinguish already-published bytes from the owned staging publication path.
        { // Reuse existing payload bytes only when the entire canonical payload is identical.
            var existingPayload = ReadBoundedStableFileBytes( // Read the complete bounded stable bytes used by later digest and parser checks.
                payloadPath, // Read the existing payload at the confined corpus.json locator.
                MaximumPayloadBytes, requireCustody: corpus.IsNativeCapture); // Native existing payload must have current private custody; no unsafe file is repaired.
            if (!existingPayload.IsSuccess || existingPayload.Value is null) // Require success of the existing stable payload bytes before using its value.
                return existingPayload // Preserve existing stable payload bytes without manufacturing replacement authority.
                    .MapError<RepositoryOwnedReferenceCorpusManifest>(); // Propagate the preceding original owner failure without returning a successful snapshot.
            if (!CanonicalBytesEqual(existingPayload.Value, payloadBytes)) // Compare the complete canonical byte sequences without summary substitution.
                return Conflict<RepositoryOwnedReferenceCorpusManifest>(); // Report conflicting sealed bytes without overwriting owner artifacts.
        } // Reuse existing payload bytes only when the entire canonical payload is identical.
        else // Publish the payload only when no payload file already exists.
        { // Publish the absent payload before permitting manifest publication.
            var payloadPublished = PublishBytesWithoutOverwrite( // Publish only owned staged bytes without overwriting a conflicting sealed artifact.
                address, // Confine this payload publication to the typed corpus address.
                payloadPath, // Publish the complete payload at the fixed corpus.json locator.
                payloadBytes, // Publish this exact complete canonical payload byte array.
                MaximumPayloadBytes, // Apply the 32 MiB raw payload cap to the current owner file.
                PayloadTempPrefix, corpus.IsNativeCapture); // Native staging uses strict private custody while legacy creation is unchanged.
            if (!payloadPublished.IsSuccess) // Require success of the payload publication result before using its value.
                return payloadPublished // Preserve payload publication result without manufacturing replacement authority.
                    .MapError<RepositoryOwnedReferenceCorpusManifest>(); // Propagate the preceding original owner failure without returning a successful snapshot.
        } // Publish the absent payload before permitting manifest publication.

        if (File.Exists(address.ManifestPath)) // Distinguish already-published bytes from the owned staging publication path.
        { // Check a racing manifest through strict readback instead of overwriting its seal.
            var racedExisting = OpenSealedCore(address); // Reuse strict readback for seal-time idempotency checks.
            if (!racedExisting.IsSuccess || racedExisting.Value is null) // Require success of the manifest published by a racing owner before using its value.
                return racedExisting // Preserve manifest published by a racing owner without manufacturing replacement authority.
                    .MapError<RepositoryOwnedReferenceCorpusManifest>(); // Propagate the preceding original owner failure without returning a successful snapshot.
            return ManifestsEqual(racedExisting.Value.Manifest, manifest) // Compare every sealed manifest identity required for idempotent publication.
                   && PayloadCorpusMatches(racedExisting.Value.Corpus, corpus) // Require exact canonical corpus equality for idempotent publication.
                ? OperationResult<RepositoryOwnedReferenceCorpusManifest>.Ok( // Return the validated typed owner value after the preceding admission checks.
                    racedExisting.Value.Manifest) // Return the racing owner's verified manifest only for identical complete content.
                : Conflict<RepositoryOwnedReferenceCorpusManifest>(); // Report conflicting sealed bytes without overwriting owner artifacts.
        } // Check a racing manifest through strict readback instead of overwriting its seal.

        // Manifest пишется последним: после этой точки открыватель обязан
        // найти уже полностью опубликованный payload с тем же length/hash.
        var manifestPublished = PublishBytesWithoutOverwrite( // Publish only owned staged bytes without overwriting a conflicting sealed artifact.
            address, // Confine the final manifest publication to this typed corpus address.
            address.ManifestPath, // Publish the seal at the typed manifest.json locator.
            manifestBytes, // Publish this exact complete manifest byte array, including its final self-hash.
            MaximumManifestBytes, // Apply the 2 MiB raw manifest cap to the current owner file.
            ManifestTempPrefix, corpus.IsNativeCapture); // Native manifest uses the same strict private staging boundary.
        if (!manifestPublished.IsSuccess) // Require success of the final manifest publication result before using its value.
            return manifestPublished // Preserve final manifest publication result without manufacturing replacement authority.
                .MapError<RepositoryOwnedReferenceCorpusManifest>(); // Propagate the preceding original owner failure without returning a successful snapshot.

        var opened = OpenSealedCore(address); // Reuse strict readback for seal-time idempotency checks.
        return opened.IsSuccess && opened.Value is not null // Return a manifest only after the newly sealed corpus strictly reopens.
            ? OperationResult<RepositoryOwnedReferenceCorpusManifest>.Ok( // Return the validated typed owner value after the preceding admission checks.
                opened.Value.Manifest) // Expose the manifest returned by that final strict reopen.
            : opened.MapError<RepositoryOwnedReferenceCorpusManifest>(); // Preserve the strict reopen failure rather than claim successful publication.
    } // Preserve identical seals and publish complete payload bytes before manifest bytes.

    private static OperationResult<RepositoryOwnedReferenceCorpusOpenResult> // Reuse strict readback for seal-time idempotency checks.
        OpenSealedCore(RepositoryOwnedReferenceCorpusAddress address) // Delegate typed-address reopen to the strict file-identity core.
        => ProjectOpenResult(OpenSealedFileIdentityCore(address)); // Verify raw manifest, bounded payload and reconstructed provenance before returning identities.

    private static OperationResult<RepositoryOwnedReferenceCorpusOpenResult> // Expose manifest and corpus without leaking the internal raw-file identity carrier.
        ProjectOpenResult( // Name the project open result operation whose typed parameters follow.
            OperationResult<RepositoryOwnedReferenceCorpusFileIdentityOpenResult> opened) // Project the strict successful snapshot or preserve its original failure.
        => opened.IsSuccess && opened.Value is not null // Expose public snapshots only after the internal strict readback succeeded.
            ? OperationResult<RepositoryOwnedReferenceCorpusOpenResult>.Ok( // Return the validated typed owner value after the preceding admission checks.
                new RepositoryOwnedReferenceCorpusOpenResult( // Construct the complete repository owned reference corpus open result only after the preceding owner checks.
                    opened.Value.Manifest, // Keep the verified sealed manifest in the public snapshot.
                    opened.Value.Corpus)) // Keep the fully reconstructed corpus in the public snapshot.
            : opened.MapError<RepositoryOwnedReferenceCorpusOpenResult>(); // Preserve the strict file-identity failure without exposing partial corpus data.

    private static OperationResult<RepositoryOwnedReferenceCorpusFileIdentityOpenResult> // Verify raw manifest, bounded payload and reconstructed provenance before returning identities.
        OpenSealedFileIdentityCore(RepositoryOwnedReferenceCorpusAddress address, string? expectedNativeManifestFileSha256 = null) // Native discovery pins both origin and the exact manifest file before core reopen.
    { // Verify stable raw pins and reconstruct every entry before returning a corpus.
        var validation = ValidateAddress(address); // Reject an address that no longer matches the canonical reference-corpus layout.
        if (!validation.IsSuccess) // Require success of the address or corpus admission result before using its value.
            return validation.MapError<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>(); // Propagate failure from the address or corpus admission result without returning an admitted value.

        try // Bound nonfatal failures around open sealed file identity core.
        { // Verify stable raw pins and reconstruct every entry before returning a corpus while retaining the existing exception boundary.
            if (!File.Exists(address.ManifestPath)) // Reject an absent sealed manifest before any strict corpus readback.
                return Invalid<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>(); // Reject invalid descriptive input with the existing ordinary failure contract.

            var readback = ReadStrictManifest(address.ManifestPath); // Read bounded stable manifest bytes and retain their raw identity independently of the sealed self-hash.
            if (!readback.IsSuccess || readback.Value is null) // Require success of the strict manifest plus raw-file identity before using its value.
                return readback.MapError<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>(); // Propagate failure from the strict manifest plus raw-file identity without returning an admitted value.
            var manifest = readback.Value.Manifest; // Use the manifest parsed from the same stable raw-file readback.
            if (expectedNativeManifestFileSha256 is not null && (!IsNativeManifest(manifest) || readback.Value.ManifestFileSha256 != expectedNativeManifestFileSha256)) // Refuse a discovery-to-core branch swap before any payload read.
                return Invalid<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>(); // Native discovery cannot silently fall back to a replacement legacy origin.
            if (IsNativeManifest(manifest)) // The explicit native origin adds strict private-handle custody without changing legacy reads.
            { // Recheck the same complete seal through a custody-verified native read handle.
                if (!NativeCaptureFilesHaveCustody(address)) return Unavailable<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>(); // Verify native leaf and both files before closed payload read.
                var privateReadback = ReadStrictManifest(address.ManifestPath, requireCustody: true); // Bind actual manifest handle security using the existing strict boundary.
                if (!privateReadback.IsSuccess || privateReadback.Value is null || privateReadback.Value.ManifestFileSha256 != readback.Value.ManifestFileSha256 || privateReadback.Value.Manifest != manifest) // Require this same exact native seal, not a later replacement snapshot.
                    return Unavailable<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>(); // Refuse changed manifest bytes or custody before native content parsing.
            } // This is a bounded current readback, not a retained lease or new authority.

            var manifestValidation = ValidateManifestForAddress( // Bind strict manifest identity and relative paths to the supplied typed address.
                address, // Compare every manifest coordinate with this typed corpus address.
                manifest); // Validate this complete parsed seal before using its payload locator.
            if (!manifestValidation.IsSuccess) // Require success of the manifest-to-address admission result before using its value.
                return manifestValidation // Preserve manifest-to-address admission result without manufacturing replacement authority.
                    .MapError<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>(); // Propagate the preceding original owner failure without returning a successful snapshot.

            var payloadPath = ResolvePayloadPath(address, manifest); // Confine the manifest-selected payload to the canonical reference-corpus address.
            if (!payloadPath.IsSuccess || payloadPath.Value is null) // Require success of the confined payload locator before using its value.
                return payloadPath // Preserve confined payload locator without manufacturing replacement authority.
                    .MapError<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>(); // Propagate the preceding original owner failure without returning a successful snapshot.

            var payloadBytes = ReadBoundedStableFileBytes( // Read the complete bounded stable bytes used by later digest and parser checks.
                payloadPath.Value, // Read the payload only from the already-confined resolved locator.
                MaximumPayloadBytes, requireCustody: IsNativeManifest(manifest)); // Native payload reads bind actual handle custody; legacy bounds remain unchanged.
            if (!payloadBytes.IsSuccess || payloadBytes.Value is null) // Require success of the exact bounded payload bytes before using its value.
                return payloadBytes // Preserve exact bounded payload bytes without manufacturing replacement authority.
                    .MapError<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>(); // Propagate the preceding original owner failure without returning a successful snapshot.
            // Payload bytes are verified before JSON parsing, so modified paid
            // answers never enter domain factories under a trusted manifest.
            if (payloadBytes.Value.LongLength != manifest.PayloadByteLength // Require exact payload length before parsing paid-reference content.
                || !StringComparer.Ordinal.Equals( // Require the actual payload-byte digest to equal the sealed manifest digest.
                    ComputeSha256(payloadBytes.Value), // Hash the exact owner byte sequence with lowercase SHA-256.
                    manifest.PayloadSha256)) // Require the exact payload digest pinned by the already-validated manifest.
            { // Reject changed raw payload hash or length before parsing closed answers.
                return Invalid<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>(); // Reject invalid descriptive input with the existing ordinary failure contract.
            } // Reject changed raw payload hash or length before parsing closed answers.

            var payload = ReadStrictPayload(payloadBytes.Value, IsRawManifest(manifest) || IsNativeManifest(manifest)); // Parse strict UTF-8 payload bytes with the existing bounded JSON policy.
            if (!payload.IsSuccess || payload.Value is null) // Require success of the complete owner payload before using its value.
                return payload.MapError<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>(); // Propagate failure from the complete owner payload without returning an admitted value.

            var corpus = ReconstructCorpusFromPayload( // Rebuild the full frozen corpus through its public factories and compare all committed representations.
                manifest, // Use the already validated sealed manifest as reconstruction authority.
                payload.Value); // Reconstruct all entries from the completely parsed payload.
            if (!corpus.IsSuccess || corpus.Value is null) // Require success of full frozen corpus snapshot before using its value.
                return corpus.MapError<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>(); // Propagate failure from full frozen corpus snapshot without returning an admitted value.

            return OperationResult<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>.Ok( // Return the validated typed owner value after the preceding admission checks.
                new RepositoryOwnedReferenceCorpusFileIdentityOpenResult( // Construct the complete repository owned reference corpus file identity open result only after the preceding owner checks.
                    manifest, // Return the manifest validated against the typed address.
                    corpus.Value, // Return the full factory-reconstructed corpus.
                    readback.Value.ManifestFileSha256, // Return the raw digest computed from the exact stable manifest byte array.
                    readback.Value.ManifestFileByteLength)); // Return the length of that same stable manifest byte array.
        } // Verify stable raw pins and reconstruct every entry before returning a corpus while retaining the existing exception boundary.
        catch (Exception exception) // Convert only the permitted failure category into this owner result; fatal failures remain outside this catch.
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Convert only the permitted failure category into this owner result; fatal failures remain outside this catch.
        { // Return the existing OpenSealedFileIdentityCore failure result after the permitted nonfatal exception.
            return Unavailable<RepositoryOwnedReferenceCorpusFileIdentityOpenResult>(); // Report failed custody or nonfatal filesystem access without admission.
        } // Return the existing OpenSealedFileIdentityCore failure result after the permitted nonfatal exception.
    } // Verify stable raw pins and reconstruct every entry before returning a corpus.

    private static OperationResult<bool> ValidateAddressAndCorpus( // Require the corpus domain, version and identity to match its typed repository address.
        RepositoryOwnedReferenceCorpusAddress address, // Use this typed address as the authority for corpus domain, subject and version.
        FrozenReferenceCorpus corpus) // Validate the complete frozen corpus against that address before sealing.
    { // Require frozen corpus identity and size to match the typed repository address.
        var addressValidation = ValidateAddress(address); // Reject an address that no longer matches the canonical reference-corpus layout.
        if (!addressValidation.IsSuccess) // Require success of address validation before using its value.
            return addressValidation; // Preserve typed-address failure before inspecting corpus content.
        if (corpus is null) // Reject missing full frozen corpus snapshot before owner use.
            return Invalid<bool>(); // Reject invalid descriptive input with the existing ordinary failure contract.
        // Address identity is the immutable snapshot coordinate. A corpus may
        // be sealed there only when its own public factory identity agrees.
        if (!StringComparer.Ordinal.Equals(address.DomainSlug, corpus.DomainTag) // Require the corpus domain to equal the typed address domain.
            || !StringComparer.Ordinal.Equals(address.SubjectSlug, corpus.SubjectId) // Require the corpus subject to equal the typed address subject.
            || !StringComparer.Ordinal.Equals( // Require exact agreement between the admitted typed corpus address's frozen corpus version and the reconstructed frozen corpus's frozen corpus version.
                address.CorpusVersion, // Use the typed address's frozen-version coordinate.
                corpus.CorpusVersion) // Require the corpus factory's version to equal that coordinate.
            || corpus.Count <= 0 // Reject an empty frozen corpus before publication.
            || corpus.Count > MaximumEntryCount // Keep the full entry count within the strict owner cap.
            || !FrozenReferenceCanonicalization.IsCanonicalSha256Ref( // Require the corpus identity to use the existing canonical sha256:lower64 grammar.
                corpus.CorpusSha256)) // Require a canonical prefixed whole-corpus identity before sealing.
        { // Reject corpus identity/count mismatches before accepting it for this address.
            return Invalid<bool>(); // Reject invalid descriptive input with the existing ordinary failure contract.
        } // Reject corpus identity/count mismatches before accepting it for this address.

        return OperationResult<bool>.Ok(true); // Return the validated typed owner value after the preceding admission checks.
    } // Require frozen corpus identity and size to match the typed repository address.

    private static OperationResult<bool> ValidateAddress( // Reject an address that no longer matches the canonical reference-corpus layout.
        RepositoryOwnedReferenceCorpusAddress address) // Validate the supplied typed layout before any corpus owner access.
    { // Require the canonical typed layout and reject reparse or directory aliases.
        if (address is null // Reject missing the typed reference-corpus address before owner use.
            || !RepositoryOwnedTrainingArtifactLayout // Revalidate the supplied typed address against the canonical repository layout.
                .IsValidReferenceCorpusAddress(address) // Require the existing address validator to admit this exact typed locator.
            || AnyExistingPathSegmentIsReparsePoint(address.CorpusRoot) // Reject reparse traversal inside the resolved corpus boundary.
            || AnyExistingPathSegmentIsReparsePoint(address.ManifestPath) // Reject reparse traversal inside the resolved corpus boundary.
            || Directory.Exists(address.ManifestPath)) // Reject a directory occupying the required owner artifact file locator.
        { // Reject invalid typed layout, reparse traversal or a directory at manifest.json.
            return Invalid<bool>(); // Reject invalid descriptive input with the existing ordinary failure contract.
        } // Reject invalid typed layout, reparse traversal or a directory at manifest.json.

        return OperationResult<bool>.Ok(true); // Return the validated typed owner value after the preceding admission checks.
    } // Require the canonical typed layout and reject reparse or directory aliases.

    private static OperationResult<bool> ValidateManifestForAddress( // Bind strict manifest identity and relative paths to the supplied typed address.
        RepositoryOwnedReferenceCorpusAddress address, // Use this typed address as the authority for all manifest coordinates.
        RepositoryOwnedReferenceCorpusManifest manifest) // Validate all identity, locator and size fields of this complete manifest.
    { // Bind all manifest identities, paths and bounds to the actual typed address.
        if (!(StringComparer.Ordinal.Equals(manifest.Schema, CurrentSchema) &&
                StringComparer.Ordinal.Equals(manifest.Context, CurrentContext) || IsRawManifest(manifest) || IsNativeManifest(manifest))
            || !StringComparer.Ordinal.Equals(manifest.Domain, address.DomainSlug) // Require manifest domain to equal the typed address domain.
            || !StringComparer.Ordinal.Equals(manifest.Subject, address.SubjectSlug) // Require manifest subject to equal the typed address subject.
            || !StringComparer.Ordinal.Equals( // Require exact agreement between the parsed sealed manifest's frozen corpus version and the admitted typed corpus address's frozen corpus version.
                manifest.CorpusVersion, // Read the manifest's frozen-version coordinate for address comparison.
                address.CorpusVersion) // Require that version to equal the typed address version.
            || !StringComparer.Ordinal.Equals( // Require exact agreement between the parsed sealed manifest's repository-relative corpus directory and the admitted typed corpus address's typed repository-relative corpus directory.
                manifest.RepositoryRelativeCorpusDirectory, // Read the manifest's repository-relative corpus directory.
                address.CorpusRootRelativePath) // Require that directory to equal the existing typed layout coordinate.
            || !StringComparer.Ordinal.Equals( // Require exact agreement between the parsed sealed manifest's repository-relative manifest locator and the admitted typed corpus address's typed repository-relative manifest locator.
                manifest.RepositoryRelativeManifestPath, // Read the manifest's repository-relative manifest locator.
                address.ManifestRelativePath) // Require that locator to equal the typed sealed-manifest path.
            || !StringComparer.Ordinal.Equals( // Require the manifest payload locator to equal the fixed typed-address locator.
                manifest.RepositoryRelativePayloadPath, // Read the manifest's repository-relative payload locator.
                PayloadRelativePath(address)) // Derive the fixed payload locator from the typed corpus address.
            || manifest.EntryCount <= 0 // Reject a manifest declaring no frozen entries.
            || manifest.EntryCount > MaximumEntryCount // Keep declared entries within the existing full owner cap.
            || manifest.PayloadByteLength <= 0 // Reject a nonpositive raw payload length.
            || manifest.PayloadByteLength > MaximumPayloadBytes // Keep declared payload length within the unchanged 32 MiB cap.
            || !IsCanonicalSha256Hex(manifest.PayloadSha256) // Require the existing lowercase bare 64-character digest grammar.
            || !IsCanonicalSha256Hex(manifest.ManifestSha256) // Require the existing lowercase bare 64-character digest grammar.
            || !FrozenReferenceCanonicalization.IsCanonicalSha256Ref( // Require the sealed whole-corpus identity to use canonical sha256:lower64 spelling.
                manifest.CorpusSha256) // Validate the complete corpus identity carried by this seal.
            || !TryParseCanonicalUtc(manifest.FrozenAtUtc, out _) // Require exact canonical UTC round-trip text for persisted provenance timestamps.
            || !FrozenReferenceCanonicalization // Validate the human corpus name through frozen-reference identity normalization.
                .NormalizeIdentity(manifest.CorpusName) // Validate this corpus name; do not interpret it as a training-attempt identity.
                .IsSuccess) // Reject a corpus name the existing normalization owner cannot admit.
        { // Reject manifest identity, path, size, timestamp or human-name disagreement.
            return Invalid<bool>(); // Reject invalid descriptive input with the existing ordinary failure contract.
        } // Reject manifest identity, path, size, timestamp or human-name disagreement.

        return OperationResult<bool>.Ok(true); // Return the validated typed owner value after the preceding admission checks.
    } // Bind all manifest identities, paths and bounds to the actual typed address.

    private static bool IsRawManifest(RepositoryOwnedReferenceCorpusManifest manifest)
        => manifest.Schema == RawUtf8Schema && manifest.Context == RawUtf8Context &&
           manifest.Domain == FrozenReferenceEntry.RawCodeDocumentationDomain;

    private static RepositoryOwnedReferenceCorpusManifest BuildManifest( // Commit canonical corpus and payload identities into the sealed manifest.
        RepositoryOwnedReferenceCorpusAddress address, // Derive manifest coordinates solely from this admitted typed address.
        FrozenReferenceCorpus corpus, // Commit this whole corpus identity, freeze time and full entry count.
        byte[] payloadBytes) // Commit the length and digest of these exact complete payload bytes.
    { // Commit actual frozen corpus and payload identities with the legacy manifest self-hash.
        var unsigned = new RepositoryOwnedReferenceCorpusManifest( // Retain all actual corpus/payload pins with an empty manifest self-hash.
            corpus.IsNativeCapture ? NativeCaptureSchema : corpus.IsRawCodeDocumentation ? RawUtf8Schema : CurrentSchema, // Use the existing sealed-manifest schema discriminator.
            corpus.IsNativeCapture ? NativeCaptureContext : corpus.IsRawCodeDocumentation ? RawUtf8Context : CurrentContext, // Use the existing paid-reference custody context.
            address.DomainSlug, // Copy the admitted typed address's corpus domain.
            address.SubjectSlug, // Copy the admitted typed address's corpus subject.
            address.CorpusVersion, // Copy the admitted typed address's frozen version.
            corpus.CorpusName, // Copy the actual corpus's human name into the manifest.
            FrozenReferenceCanonicalization.FormatUtc(corpus.FrozenAtUtc), // Use the existing exact UTC wire spelling for the owner timestamp.
            corpus.Count, // Commit the entire frozen corpus entry count.
            corpus.CorpusSha256, // Commit the whole canonical frozen-corpus identity.
            address.CorpusRootRelativePath, // Commit the typed layout's exact corpus directory.
            PayloadRelativePath(address), // Derive the fixed payload locator from the typed corpus address.
            payloadBytes.LongLength, // Commit the length of the actual complete payload byte array.
            ComputeSha256(payloadBytes), // Hash the exact owner byte sequence with lowercase SHA-256.
            address.ManifestRelativePath, // Commit the typed layout's exact manifest locator.
            string.Empty); // Keep the self-hash member present with an empty value in the legacy hash preimage.
        return unsigned with // Copy the complete unsigned projection and fill its computed self-hash.
        { // Serialize the manifest with the cleared self-hash to derive its sealed hash.
            ManifestSha256 = ComputeSha256( // Hash the exact owner byte sequence with lowercase SHA-256.
                WriteCanonicalManifestBytes(unsigned)), // Hash the fixed legacy representation with an empty self-hash field.
        }; // Finish build manifest.
    } // Commit actual frozen corpus and payload identities with the legacy manifest self-hash.

    private static RepositoryOwnedReferenceCorpusPayload BuildPayload( // Preserve the complete frozen corpus representation and every entry in its payload.
        FrozenReferenceCorpus corpus) // Project every header and entry from this complete factory-owned corpus.
        => new( // Construct a complete persisted payload from the actual frozen corpus.
            corpus.IsNativeCapture ? NativeCapturePayloadSchema : corpus.IsRawCodeDocumentation ? RawUtf8PayloadSchema : CurrentPayloadSchema, // Use the fixed payload schema discriminator.
            corpus.IsNativeCapture ? NativeCaptureContext : corpus.IsRawCodeDocumentation ? RawUtf8Context : CurrentContext, // Use the fixed paid-reference custody context.
            corpus.CorpusName, // Preserve the reconstructed frozen corpus's human corpus name in the complete persisted corpus representation.
            corpus.CorpusVersion, // Preserve the reconstructed frozen corpus's frozen corpus version in the complete persisted corpus representation.
            FrozenReferenceCanonicalization.FormatUtc(corpus.FrozenAtUtc), // Use the existing exact UTC wire spelling for the owner timestamp.
            corpus.DomainTag, // Preserve the actual corpus domain in its payload.
            corpus.SubjectId, // Preserve the actual corpus subject in its payload.
            corpus.Count, // Preserve the full actual entry count in the payload header.
            corpus.CorpusSha256, // Preserve the reconstructed frozen corpus's whole-corpus canonical identity in the complete persisted corpus representation.
            corpus.CanonicalRepresentation, // Preserve the reconstructed frozen corpus's full canonical representation in the complete persisted corpus representation.
            Array.AsReadOnly(corpus.Entries // Snapshot every frozen corpus entry; the payload cannot retain a mutable list.
                .Select(ToPayloadEntry) // Copy every frozen corpus entry into its complete persisted payload representation.
                .ToArray())); // Freeze the complete projected entry array behind a read-only wrapper.

    private static RepositoryOwnedReferenceCorpusPayloadEntry ToPayloadEntry( // Carry the complete entry and paid provenance into the owner payload representation.
        FrozenReferenceEntry entry) // Project this whole frozen entry and its closed paid provenance.
        => new( // Construct one complete persisted entry from its factory-owned value.
            entry.EntryId, // Preserve the reconstructed frozen entry's canonical frozen-entry identity in the complete persisted corpus representation.
            entry.OwnerInput, // Preserve the reconstructed frozen entry's owner input for this closed case in the complete persisted corpus representation.
            entry.PaidModelReferenceAnswer, // Preserve the reconstructed frozen entry's closed paid-reference answer in the complete persisted corpus representation.
            entry.DomainTag, // Preserve this frozen entry's domain.
            entry.SubjectId, // Preserve this frozen entry's subject.
            FrozenReferenceEntry.CanonicalPartition(entry.Partition), // Use the existing explicit wire name for the frozen entry partition.
            entry.SourceEntityFingerprint, // Preserve the reconstructed frozen entry's independent source fingerprint in the complete persisted corpus representation.
            entry.CanonicalRepresentation, // Preserve this entry's full canonical representation for fresh identity reconstruction.
            entry.PaidReferenceProvenance is { } paid ? ToPayloadProvenance(paid) : null, entry.NativeCaptureProvenance); // Preserve every provenance identity used to reconstruct the paid reference.

    private static RepositoryOwnedReferenceCorpusPayloadProvenance // Preserve every provenance identity used to reconstruct the paid reference.
        ToPayloadProvenance(PaidReferenceProvenance provenance) // Copy all closed paid-provenance fields into the persisted payload.
        => new( // Construct the complete persisted paid-provenance value.
            provenance.ProviderRef, // Preserve the reconstructed paid-reference provenance's paid-reference provider identity in the complete persisted corpus representation.
            provenance.ModelId, // Preserve the reconstructed paid-reference provenance's paid-reference model identity in the complete persisted corpus representation.
            provenance.ModelVersion, // Preserve the reconstructed paid-reference provenance's paid-reference model version in the complete persisted corpus representation.
            provenance.RequestId, // Preserve the reconstructed paid-reference provenance's paid-reference capture request identity in the complete persisted corpus representation.
            FrozenReferenceCanonicalization.FormatUtc(provenance.CapturedAtUtc), // Use the existing exact UTC wire spelling for the owner timestamp.
            provenance.ReferenceAnswerSha256, // Preserve the reconstructed paid-reference provenance's normalized paid-reference answer digest in the complete persisted corpus representation.
            provenance.ProvenanceSha256, // Preserve the reconstructed paid-reference provenance's complete provenance digest in the complete persisted corpus representation.
            provenance.CanonicalRepresentation,
            provenance.ReferenceAnswerRawUtf8Sha256); // Preserve the full paid-provenance canonical representation.

    private static OperationResult<FrozenReferenceCorpus> // Rebuild the full frozen corpus through its public factories and compare all committed representations.
        ReconstructCorpusFromPayload( // Name the reconstruct corpus from payload operation whose typed parameters follow.
            RepositoryOwnedReferenceCorpusManifest manifest, // Use the admitted seal as the comparison authority during full reconstruction.
            RepositoryOwnedReferenceCorpusPayload payload) // Reconstruct every stored entry from this complete parsed payload.
    { // Rebuild all entries through factories and compare the entire corpus commitment.
        if (!PayloadHeaderMatchesManifest(manifest, payload) // Compare all payload header identities with the trusted sealed manifest.
            || !TryParseCanonicalUtc(payload.FrozenAtUtc, out var frozenAtUtc)) // Require exact canonical UTC round-trip text for persisted provenance timestamps.
        { // Reject mismatched payload headers or a noncanonical freeze timestamp.
            return Invalid<FrozenReferenceCorpus>(); // Reject invalid descriptive input with the existing ordinary failure contract.
        } // Reject mismatched payload headers or a noncanonical freeze timestamp.

        var entries = new List<FrozenReferenceEntry>(payload.EntryCount); // Allocate space for every declared entry before factory reconstruction.
        foreach (var payloadEntry in payload.Entries) // Visit the entire corpus entry sequence without implicit truncation.
        { // Reconstruct every persisted entry with its closed paid provenance.
            var entry = ReconstructEntry(payload, payloadEntry); // Revalidate paid provenance and frozen entry identity through domain factories.
            if (!entry.IsSuccess || entry.Value is null) // Require success of the full frozen entry before using its value.
                return entry.MapError<FrozenReferenceCorpus>(); // Propagate failure from the full frozen entry without returning an admitted value.
            entries.Add(entry.Value); // Retain this fully reconstructed entry after provenance and identity validation.
        } // Reconstruct every persisted entry with its closed paid provenance.

        var corpus = FrozenReferenceCorpus.Build( // Rebuild the entire frozen corpus through its public factory.
            payload.CorpusName, // Preserve the complete parsed payload's human corpus name in the complete persisted corpus representation.
            payload.CorpusVersion, // Preserve the complete parsed payload's frozen corpus version in the complete persisted corpus representation.
            frozenAtUtc, // Use the exactly parsed persisted UTC time in the corpus factory.
            entries); // Supply every reconstructed entry to that factory without truncation.
        if (!corpus.IsSuccess || corpus.Value is null) // Require success of full frozen corpus snapshot before using its value.
            return Invalid<FrozenReferenceCorpus>(); // Reject invalid descriptive input with the existing ordinary failure contract.

        if (!ReconstructedCorpusMatchesPayloadAndManifest( // Require reconstructed entries and canonical corpus identity to match both owner representations.
                manifest, // Compare the reconstructed corpus with the complete admitted seal.
                payload, // Compare it also with the complete persisted payload representation.
                corpus.Value)) // Check the whole factory result rather than an identity-only surrogate.
        { // Reject a factory-rebuilt corpus that differs from payload or sealed manifest.
            return Invalid<FrozenReferenceCorpus>(); // Reject invalid descriptive input with the existing ordinary failure contract.
        } // Reject a factory-rebuilt corpus that differs from payload or sealed manifest.

        return OperationResult<FrozenReferenceCorpus>.Ok(corpus.Value); // Return the validated typed owner value after the preceding admission checks.
    } // Rebuild all entries through factories and compare the entire corpus commitment.

    private static OperationResult<FrozenReferenceEntry> ReconstructEntry( // Revalidate paid provenance and frozen entry identity through domain factories.
        RepositoryOwnedReferenceCorpusPayload payload, // Use this payload header to require the entry domain/subject to agree.
        RepositoryOwnedReferenceCorpusPayloadEntry payloadEntry) // Reconstruct the complete stored entry and its nested paid provenance.
    { // Recreate closed paid provenance and entry identity through the public factories.
        if (payload.Schema == NativeCapturePayloadSchema) return ReconstructNativeEntry(payload, payloadEntry); // Reconstruct the explicit native origin without inventing legacy identity.
        if (payloadEntry.PaidReferenceProvenance is null || payloadEntry.NativeCaptureProvenance is not null) return Invalid<FrozenReferenceEntry>(); // Legacy schemas require their complete known-model origin.
        if (!StringComparer.Ordinal.Equals(payloadEntry.Domain, payload.Domain) // Reject an entry whose domain differs from its payload header.
            || !StringComparer.Ordinal.Equals(payloadEntry.Subject, payload.Subject) // Reject an entry whose subject differs from its payload header.
            || !TryParsePartition(payloadEntry.Partition, out var partition) // Admit only the existing explicit frozen-reference partitions.
            || !TryParseCanonicalUtc( // Require exact canonical UTC round-trip text for persisted provenance timestamps.
                payloadEntry.PaidReferenceProvenance.CapturedAtUtc, // Parse the persisted paid-reference capture time before provenance reconstruction.
                out var capturedAtUtc)) // Retain the explicitly parsed captured at utc for the subsequent complete owner checks.
        { // Reject entry domain/subject, partition or capture-time disagreement before factories.
            return Invalid<FrozenReferenceEntry>(); // Reject invalid descriptive input with the existing ordinary failure contract.
        } // Reject entry domain/subject, partition or capture-time disagreement before factories.

        var raw = payload.Schema == RawUtf8PayloadSchema;
        var storedProvenance = payloadEntry.PaidReferenceProvenance;
        var provenance = raw
            ? PaidReferenceProvenance.CreateRawUtf8(storedProvenance.ProviderRef, storedProvenance.ModelId,
                storedProvenance.ModelVersion, storedProvenance.RequestId, capturedAtUtc,
                payloadEntry.PaidModelReferenceAnswer)
            : PaidReferenceProvenance.Create(storedProvenance.ProviderRef, storedProvenance.ModelId,
                storedProvenance.ModelVersion, storedProvenance.RequestId, capturedAtUtc,
                storedProvenance.ReferenceAnswerSha256);
        if (!provenance.IsSuccess || provenance.Value is null) // Require success of the paid-reference provenance before using its value.
            return Invalid<FrozenReferenceEntry>(); // Reject invalid descriptive input with the existing ordinary failure contract.
        // Stored flags are not read from JSON. The public factory recreates
        // ClosedPaidReference and both never-trainable/DPO-side guards.
        if (!StringComparer.Ordinal.Equals(provenance.Value.ReferenceAnswerRawUtf8Sha256,
                storedProvenance.ReferenceAnswerRawUtf8Sha256) ||
            !StringComparer.Ordinal.Equals(provenance.Value.ReferenceAnswerSha256, storedProvenance.ReferenceAnswerSha256) ||
            !provenance.Value.ClosedPaidReference // Require the reconstructed answer to retain its closed paid-reference provenance.
            || !provenance.Value.PaidReferenceOutputNeverTrainable // Reject provenance that could admit this paid reference answer as trainable output.
            || !provenance.Value.PaidReferenceOutputNeverDpoPairSide // Reject provenance that could admit this paid reference answer as a DPO pair side.
            || !StringComparer.Ordinal.Equals( // Require exact agreement between provenance's complete provenance digest and payloadEntry.PaidReferenceProvenance's complete provenance digest.
                provenance.Value.ProvenanceSha256, // Compare the reconstructed complete provenance digest.
                payloadEntry.PaidReferenceProvenance.ProvenanceSha256) // Require it to equal the persisted complete provenance digest.
            || !StringComparer.Ordinal.Equals( // Require exact agreement between provenance's full canonical representation and payloadEntry.PaidReferenceProvenance's full canonical representation.
                provenance.Value.CanonicalRepresentation, // Compare the reconstructed full provenance representation.
                payloadEntry.PaidReferenceProvenance.CanonicalRepresentation)) // Require it to equal the full persisted provenance representation.
        { // Reject reconstructed provenance that loses closed-output guards or canonical identity.
            return Invalid<FrozenReferenceEntry>(); // Reject invalid descriptive input with the existing ordinary failure contract.
        } // Reject reconstructed provenance that loses closed-output guards or canonical identity.

        var entry = raw
            ? FrozenReferenceEntry.CreateRawCodeDocumentation(payloadEntry.OwnerInput, payloadEntry.PaidModelReferenceAnswer,
                provenance.Value, payloadEntry.Subject, partition, payloadEntry.SourceEntityFingerprint)
            : FrozenReferenceEntry.Create(payloadEntry.OwnerInput, payloadEntry.PaidModelReferenceAnswer,
                provenance.Value, payloadEntry.Domain, payloadEntry.Subject, partition, payloadEntry.SourceEntityFingerprint);
        if (!entry.IsSuccess || entry.Value is null) // Require success of the full frozen entry before using its value.
            return Invalid<FrozenReferenceEntry>(); // Reject invalid descriptive input with the existing ordinary failure contract.

        if (!StringComparer.Ordinal.Equals(entry.Value.EntryId, payloadEntry.EntryId) // Require the factory-rederived frozen-entry ID to equal its persisted ID.
            || !StringComparer.Ordinal.Equals( // Require exact agreement between entry's full canonical representation and the persisted frozen-entry representation's full canonical representation.
                entry.Value.CanonicalRepresentation, // Compare the full reconstructed entry representation.
                payloadEntry.CanonicalRepresentation)) // Require it to equal the full persisted entry representation.
        { // Reject a rebuilt entry whose ID or complete canonical representation changed.
            return Invalid<FrozenReferenceEntry>(); // Reject invalid descriptive input with the existing ordinary failure contract.
        } // Reject a rebuilt entry whose ID or complete canonical representation changed.

        return OperationResult<FrozenReferenceEntry>.Ok(entry.Value); // Return the validated typed owner value after the preceding admission checks.
    } // Recreate closed paid provenance and entry identity through the public factories.

    private static bool PayloadHeaderMatchesManifest( // Compare all payload header identities with the trusted sealed manifest.
        RepositoryOwnedReferenceCorpusManifest manifest, // Use the sealed header as the equality target for all payload identities.
        RepositoryOwnedReferenceCorpusPayload payload) // Compare this complete payload header and declared entry count with that seal.
        => (IsNativeManifest(manifest)
                ? payload.Schema == NativeCapturePayloadSchema && payload.Context == NativeCaptureContext && payload.Domain == FrozenReferenceEntry.RawCodeDocumentationDomain // Native tuple stays distinct from known-model raw v2.
                : IsRawManifest(manifest)
                ? payload.Schema == RawUtf8PayloadSchema && payload.Context == RawUtf8Context &&
                    payload.Domain == FrozenReferenceEntry.RawCodeDocumentationDomain
                : manifest.Schema == CurrentSchema && manifest.Context == CurrentContext &&
                    payload.Schema == CurrentPayloadSchema && payload.Context == CurrentContext)
           && StringComparer.Ordinal.Equals(payload.Domain, manifest.Domain) // Require payload and sealed manifest to agree on domain.
           && StringComparer.Ordinal.Equals(payload.Subject, manifest.Subject) // Require payload and sealed manifest to agree on subject.
           && StringComparer.Ordinal.Equals( // Require exact agreement between the complete parsed payload's frozen corpus version and the parsed sealed manifest's frozen corpus version.
               payload.CorpusVersion, // Compare this frozen version with the corresponding field in the other complete snapshot.
               manifest.CorpusVersion) // Compare this frozen version with the corresponding field in the other complete snapshot.
           && StringComparer.Ordinal.Equals(payload.CorpusName, manifest.CorpusName) // Require payload and sealed manifest to agree on human corpus name.
           && StringComparer.Ordinal.Equals( // Require exact agreement between the complete parsed payload's corpus freeze timestamp and the parsed sealed manifest's corpus freeze timestamp.
               payload.FrozenAtUtc, // Compare this freeze timestamp with the corresponding field in the other complete snapshot.
               manifest.FrozenAtUtc) // Compare this freeze timestamp with the corresponding field in the other complete snapshot.
           && payload.EntryCount == manifest.EntryCount // Require payload and manifest to declare the same full entry count.
           && payload.Entries.Count == payload.EntryCount // Require the complete entry array to equal the declared payload count.
           && StringComparer.Ordinal.Equals( // Require exact agreement between the complete parsed payload's whole-corpus canonical identity and the parsed sealed manifest's whole-corpus canonical identity.
               payload.CorpusSha256, // Compare this whole-corpus digest with the corresponding field in the other complete snapshot.
               manifest.CorpusSha256); // Compare this whole-corpus digest with the corresponding field in the other complete snapshot.

    private static bool ReconstructedCorpusMatchesPayloadAndManifest( // Require reconstructed entries and canonical corpus identity to match both owner representations.
        RepositoryOwnedReferenceCorpusManifest manifest, // Use the admitted seal for whole-corpus header equality.
        RepositoryOwnedReferenceCorpusPayload payload, // Use the whole payload for header and ordered entry-ID equality.
        FrozenReferenceCorpus corpus) // Compare every factory-reconstructed field and entry with both persisted representations.
    { // Compare every reconstructed header and ordered entry ID with both persisted owners.
        if (!PayloadCorpusMatchesManifest(manifest, corpus) // Compare full corpus identity against the sealed manifest.
            || !PayloadCorpusMatchesPayload(payload, corpus) // Compare full corpus identity against the persisted payload.
            || payload.Entries.Count != corpus.EntryNames.Count) // Require the entire persisted entry list and reconstructed ID list to have equal length.
        { // Reject header/count disagreement before comparing every ordered entry ID.
            return false; // Reject this failed strict owner predicate without a snapshot.
        } // Reject header/count disagreement before comparing every ordered entry ID.

        for (var index = 0; index < payload.Entries.Count; index++) // Compare every persisted entry ID with its reconstructed ID at the same position.
        { // Compare the complete persisted/reconstructed entry sequence at each ordinal.
            if (!StringComparer.Ordinal.Equals( // Require every persisted entry ID to match the reconstructed entry at the same index.
                    payload.Entries[index].EntryId, // Read this persisted entry's canonical ID at its original index.
                    corpus.EntryNames[index])) // Require the reconstructed entry ID at that same index to match.
            { // Reject a persisted entry ID differing from the reconstructed ID at this index.
                return false; // Reject this failed strict owner predicate without a snapshot.
            } // Reject a persisted entry ID differing from the reconstructed ID at this index.
        } // Compare the complete persisted/reconstructed entry sequence at each ordinal.

        return true; // Accept only after every preceding exact owner check passed.
    } // Compare every reconstructed header and ordered entry ID with both persisted owners.

    private static bool PayloadCorpusMatchesManifest( // Compare full corpus identity against the sealed manifest.
        RepositoryOwnedReferenceCorpusManifest manifest, // Require the reconstructed corpus header to agree with this complete seal.
        FrozenReferenceCorpus corpus) // Check this whole factory corpus against the manifest header.
        => StringComparer.Ordinal.Equals(corpus.DomainTag, manifest.Domain) // Require reconstructed corpus domain to equal the sealed manifest domain.
           && StringComparer.Ordinal.Equals(corpus.SubjectId, manifest.Subject) // Require reconstructed corpus subject to equal the sealed manifest subject.
           && StringComparer.Ordinal.Equals( // Require exact agreement between the reconstructed frozen corpus's frozen corpus version and the parsed sealed manifest's frozen corpus version.
               corpus.CorpusVersion, // Compare this frozen version with the corresponding field in the other complete snapshot.
               manifest.CorpusVersion) // Compare this frozen version with the corresponding field in the other complete snapshot.
           && StringComparer.Ordinal.Equals(corpus.CorpusName, manifest.CorpusName) // Require reconstructed corpus name to equal the sealed manifest name.
           && StringComparer.Ordinal.Equals( // Require the manifest timestamp to equal the corpus owner canonical UTC spelling.
               FrozenReferenceCanonicalization.FormatUtc(corpus.FrozenAtUtc), // Use the existing exact UTC wire spelling for the owner timestamp.
               manifest.FrozenAtUtc) // Compare this freeze timestamp with the corresponding field in the other complete snapshot.
           && corpus.Count == manifest.EntryCount // Require the reconstructed full entry count to equal the manifest count.
           && StringComparer.Ordinal.Equals( // Require exact agreement between the reconstructed frozen corpus's whole-corpus canonical identity and the parsed sealed manifest's whole-corpus canonical identity.
               corpus.CorpusSha256, // Compare this whole-corpus digest with the corresponding field in the other complete snapshot.
               manifest.CorpusSha256); // Compare this whole-corpus digest with the corresponding field in the other complete snapshot.

    private static bool PayloadCorpusMatchesPayload( // Compare full corpus identity against the persisted payload.
        RepositoryOwnedReferenceCorpusPayload payload, // Require the factory corpus to agree with this complete payload representation.
        FrozenReferenceCorpus corpus) // Compare the whole reconstructed corpus, including its canonical representation.
        => StringComparer.Ordinal.Equals(payload.Domain, corpus.DomainTag) // Require persisted payload domain to equal reconstructed corpus domain.
           && StringComparer.Ordinal.Equals(payload.Subject, corpus.SubjectId) // Require persisted payload subject to equal reconstructed corpus subject.
           && StringComparer.Ordinal.Equals( // Require exact agreement between the complete parsed payload's frozen corpus version and the reconstructed frozen corpus's frozen corpus version.
               payload.CorpusVersion, // Compare this frozen version with the corresponding field in the other complete snapshot.
               corpus.CorpusVersion) // Compare this frozen version with the corresponding field in the other complete snapshot.
           && StringComparer.Ordinal.Equals(payload.CorpusName, corpus.CorpusName) // Require persisted and reconstructed corpus names to match.
           && StringComparer.Ordinal.Equals( // Require the payload timestamp to equal the reconstructed corpus canonical UTC spelling.
               payload.FrozenAtUtc, // Compare this freeze timestamp with the corresponding field in the other complete snapshot.
               FrozenReferenceCanonicalization.FormatUtc(corpus.FrozenAtUtc)) // Use the existing exact UTC wire spelling for the owner timestamp.
           && payload.EntryCount == corpus.Count // Require persisted and reconstructed full entry counts to match.
           && StringComparer.Ordinal.Equals( // Require exact agreement between the complete parsed payload's whole-corpus canonical identity and the reconstructed frozen corpus's whole-corpus canonical identity.
               payload.CorpusSha256, // Compare this whole-corpus digest with the corresponding field in the other complete snapshot.
               corpus.CorpusSha256) // Compare this whole-corpus digest with the corresponding field in the other complete snapshot.
           && StringComparer.Ordinal.Equals( // Require exact agreement between the complete parsed payload's full canonical representation and the reconstructed frozen corpus's full canonical representation.
               payload.CanonicalRepresentation, // Compare this full canonical representation with the corresponding field in the other complete snapshot.
               corpus.CanonicalRepresentation); // Compare this full canonical representation with the corresponding field in the other complete snapshot.

    private static bool PayloadCorpusMatches( // Require exact canonical corpus equality for idempotent publication.
        FrozenReferenceCorpus left, // Use the already sealed corpus as the complete idempotency comparison target.
        FrozenReferenceCorpus right) // Require the proposed corpus to match the same full canonical content.
        => StringComparer.Ordinal.Equals(left.CorpusSha256, right.CorpusSha256) // Require both candidate corpora to have the same canonical whole-corpus digest.
           && StringComparer.Ordinal.Equals( // Require exact agreement between the first comparison snapshot's full canonical representation and the second comparison snapshot's full canonical representation.
               left.CanonicalRepresentation, // Compare this full canonical representation with the corresponding field in the other complete snapshot.
               right.CanonicalRepresentation) // Compare this full canonical representation with the corresponding field in the other complete snapshot.
           && left.EntryNames.SequenceEqual(right.EntryNames, StringComparer.Ordinal); // Require every ordered frozen-entry ID to match for idempotent publication.

    private static OperationResult<StrictManifestReadback> // Read stable raw manifest bytes and verify the legacy cleared-field self-hash.
        ReadStrictManifest(string manifestPath, bool requireCustody = false) // Native origin additionally requires the same strict private handle boundary.
    { // Validate bounded stable manifest bytes and preserve their independent raw-file identity.
        var bytes = ReadBoundedStableFileBytes(manifestPath, MaximumManifestBytes, requireCustody); // Read the complete bounded stable bytes used by later digest and parser checks.
        if (!bytes.IsSuccess || bytes.Value is null) // Require success of the exact owner byte sequence before using its value.
            return bytes.MapError<StrictManifestReadback>(); // Propagate failure from the exact owner byte sequence without returning an admitted value.

        try // Bound nonfatal failures around read strict manifest.
        { // Validate bounded stable manifest bytes and preserve their independent raw-file identity while retaining the existing exception boundary.
            using var document = JsonDocument.Parse( // Parse only the bounded strict UTF-8 representation under the existing JSON policy.
                bytes.Value, // Parse the exact stable manifest bytes whose raw identity will be returned.
                new JsonDocumentOptions // Specify strict parsing options for this bounded persisted owner JSON.
                { // Configure the strict manifest JSON parser without relaxed syntax.
                    AllowTrailingCommas = false, // Reject trailing commas in the persisted strict owner JSON.
                    CommentHandling = JsonCommentHandling.Disallow, // Reject JSON comments instead of admitting an alternate persisted representation.
                    MaxDepth = ManifestJsonMaxDepth, // Keep manifest nesting within the existing strict parser depth.
                }); // Complete the strict manifest parser policy before reading its root.
            if (!TryReadManifestElement( // Require exact manifest members, bounded primitive values and canonical identities.
                    document.RootElement, // Validate the complete manifest root object.
                    out var manifest)) // Retain the explicitly parsed manifest for the subsequent complete owner checks.
            { // Reject a manifest that cannot supply its entire exact required field set.
                return Invalid<StrictManifestReadback>(); // Reject invalid descriptive input with the existing ordinary failure contract.
            } // Reject a manifest that cannot supply its entire exact required field set.

            var selfHash = ComputeSha256(WriteCanonicalManifestBytes( // Derive the legacy self-hash from the full fixed representation with its hash value cleared.
                manifest with // Copy the parsed manifest so only its self-hash value can be cleared.
                { // Clear the self-hash value in a copy while retaining every other manifest field.
                    ManifestSha256 = string.Empty, // Clear only the self-hash value before recomputing the manifest commitment.
                })); // Hash that complete cleared-field copy using the unchanged legacy writer.
            if (!StringComparer.Ordinal.Equals( // Compare the rederived cleared-field commitment with the stored self-hash.
                    selfHash, // Use the rederived manifest self-hash as the comparison value.
                    manifest.ManifestSha256)) // Require the stored sealed manifest commitment to equal that value.
            { // Reject a stored manifest self-hash differing from its rederived commitment.
                return Invalid<StrictManifestReadback>(); // Reject invalid descriptive input with the existing ordinary failure contract.
            } // Reject a stored manifest self-hash differing from its rederived commitment.

            // Parsing and raw identity use the identical stable byte array.
            // The bounded helper compares two reads on its one secured handle.
            return OperationResult<StrictManifestReadback>.Ok( // Return the validated typed owner value after the preceding admission checks.
                new StrictManifestReadback( // Construct the complete strict manifest readback only after the preceding owner checks.
                    manifest, // Return the complete validated manifest with the same raw-file identity.
                    ComputeSha256(bytes.Value), // Hash the exact owner byte sequence with lowercase SHA-256.
                    bytes.Value.LongLength)); // Return the exact byte length of the same stable array used for parsing and hashing.
        } // Validate bounded stable manifest bytes and preserve their independent raw-file identity while retaining the existing exception boundary.
        catch (Exception exception) // Convert only the permitted failure category into this owner result; fatal failures remain outside this catch.
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Convert only the permitted failure category into this owner result; fatal failures remain outside this catch.
        { // Return the existing ReadStrictManifest failure result after the permitted nonfatal exception.
            return Invalid<StrictManifestReadback>(); // Reject invalid descriptive input with the existing ordinary failure contract.
        } // Return the existing ReadStrictManifest failure result after the permitted nonfatal exception.
    } // Validate bounded stable manifest bytes and preserve their independent raw-file identity.

    private static OperationResult<RepositoryOwnedReferenceCorpusPayload> // Parse strict UTF-8 payload bytes with the existing bounded JSON policy.
        ReadStrictPayload(byte[] payloadBytes, bool rawUtf8) // Parse these bounded raw payload bytes as the exact closed payload schema.
    { // Parse only bounded complete payload bytes under the existing strict JSON policy.
        if (payloadBytes.LongLength <= 0 // Reject an empty payload before strict JSON parsing.
            || payloadBytes.LongLength > MaximumPayloadBytes) // Reject payload bytes exceeding the existing 32 MiB owner cap.
        { // Reject empty or oversized payload bytes before strict parsing.
            return Invalid<RepositoryOwnedReferenceCorpusPayload>(); // Reject invalid descriptive input with the existing ordinary failure contract.
        } // Reject empty or oversized payload bytes before strict parsing.

        try // Bound nonfatal failures around read strict payload.
        { // Parse only bounded complete payload bytes under the existing strict JSON policy while retaining the existing exception boundary.
            if (rawUtf8) _ = StrictUtf8.GetCharCount(payloadBytes); // Reject malformed actual raw UTF-8 before JSON reconstruction.
            using var document = JsonDocument.Parse( // Parse only the bounded strict UTF-8 representation under the existing JSON policy.
                payloadBytes, // Parse the exact payload bytes already checked against the seal.
                new JsonDocumentOptions // Specify strict parsing options for this bounded persisted owner JSON.
                { // Configure strict complete-payload parsing without relaxed JSON syntax.
                    AllowTrailingCommas = false, // Reject trailing commas in the persisted strict owner JSON.
                    CommentHandling = JsonCommentHandling.Disallow, // Reject JSON comments instead of admitting an alternate persisted representation.
                    MaxDepth = PayloadJsonMaxDepth, // Keep payload nesting within the existing strict parser depth.
                }); // Complete the payload parser policy before reading its root.
            return TryReadPayloadElement(document.RootElement, out var payload) // Require exact payload members and reconstruct the whole bounded entry sequence.
                ? OperationResult<RepositoryOwnedReferenceCorpusPayload>.Ok( // Return the validated typed owner value after the preceding admission checks.
                    payload) // Return the complete parsed payload after all entries have been read.
                : Invalid<RepositoryOwnedReferenceCorpusPayload>(); // Reject invalid descriptive input with the existing ordinary failure contract.
        } // Parse only bounded complete payload bytes under the existing strict JSON policy while retaining the existing exception boundary.
        catch (Exception exception) // Convert only the permitted failure category into this owner result; fatal failures remain outside this catch.
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Convert only the permitted failure category into this owner result; fatal failures remain outside this catch.
        { // Return the existing ReadStrictPayload failure result after the permitted nonfatal exception.
            return Invalid<RepositoryOwnedReferenceCorpusPayload>(); // Reject invalid descriptive input with the existing ordinary failure contract.
        } // Return the existing ReadStrictPayload failure result after the permitted nonfatal exception.
    } // Parse only bounded complete payload bytes under the existing strict JSON policy.

    private static bool TryReadManifestElement( // Require exact manifest members, bounded primitive values and canonical identities.
        JsonElement element, // Read the manifest object only after checking its exact member set.
        out RepositoryOwnedReferenceCorpusManifest manifest) // Return the complete parsed manifest only when every required field is valid.
    { // Read every required manifest field before constructing its complete parsed value.
        manifest = EmptyManifest(); // Initialize the strict parser failure carrier without accepting it as a manifest.
        if (!ObjectHasExactProperties( // Reject unknown, duplicate or absent JSON members before reading values.
                element, // Check this object for its exact closed member set.
                SchemaProperty, // Select the exact schema discriminator wire member for this operation.
                ContextProperty, // Select the exact paid-reference custody context wire member for this operation.
                DomainProperty, // Select the exact corpus domain wire member for this operation.
                SubjectProperty, // Select the exact corpus subject wire member for this operation.
                CorpusVersionProperty, // Select the exact frozen corpus version wire member for this operation.
                CorpusNameProperty, // Select the exact human corpus name wire member for this operation.
                FrozenAtUtcProperty, // Select the exact corpus freeze timestamp wire member for this operation.
                EntryCountProperty, // Select the exact complete payload entry count wire member for this operation.
                CorpusSha256Property, // Select the exact whole-corpus canonical identity wire member for this operation.
                RepositoryRelativeCorpusDirectoryProperty, // Select the exact repository-relative corpus directory wire member for this operation.
                RepositoryRelativePayloadPathProperty, // Select the exact repository-relative payload locator wire member for this operation.
                PayloadByteLengthProperty, // Select the exact raw payload length wire member for this operation.
                PayloadSha256Property, // Select the exact raw payload digest wire member for this operation.
                RepositoryRelativeManifestPathProperty, // Select the exact repository-relative manifest locator wire member for this operation.
                ManifestSha256Property) // Select the exact sealed manifest self-hash wire member for this operation.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the format discriminator from this strict object.
                SchemaProperty, // Select the exact schema discriminator wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the format discriminator.
                out var schema) // Retain the explicitly parsed schema for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the paid-reference corpus context from this strict object.
                ContextProperty, // Select the exact paid-reference custody context wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the paid-reference corpus context.
                out var context) // Retain the explicitly parsed context for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the corpus domain slug from this strict object.
                DomainProperty, // Select the exact corpus domain wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the corpus domain slug.
                out var domain) // Retain the explicitly parsed domain for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the corpus subject slug from this strict object.
                SubjectProperty, // Select the exact corpus subject wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the corpus subject slug.
                out var subject) // Retain the explicitly parsed subject for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the frozen corpus version from this strict object.
                CorpusVersionProperty, // Select the exact frozen corpus version wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the frozen corpus version.
                out var corpusVersion) // Retain the explicitly parsed corpus version for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the factory-derived corpus display name from this strict object.
                CorpusNameProperty, // Select the exact human corpus name wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the factory-derived corpus display name.
                out var corpusName) // Retain the explicitly parsed corpus name for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the canonical corpus freeze timestamp from this strict object.
                FrozenAtUtcProperty, // Select the exact corpus freeze timestamp wire member for this operation.
                MaximumUtcTextLength, // Apply the 64-character UTC-text limit to the canonical corpus freeze timestamp.
                out var frozenAtUtc) // Retain the explicitly parsed frozen at utc for the subsequent complete owner checks.
            || !TryReadInt(element, EntryCountProperty, out var entryCount) // Require the selected JSON count to fit its existing Int32 owner bound.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the whole-corpus identity from this strict object.
                CorpusSha256Property, // Select the exact whole-corpus canonical identity wire member for this operation.
                "sha256:".Length + HashHexLength, // Bound this owner digest to the exact sha256: prefix plus 64 lowercase hex characters.
                out var corpusSha256) // Retain the explicitly parsed corpus sha256 for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the typed corpus directory locator from this strict object.
                RepositoryRelativeCorpusDirectoryProperty, // Select the exact repository-relative corpus directory wire member for this operation.
                MaximumRepositoryRelativePathLength, // Apply the 1024-character relative-path limit to the typed corpus directory locator.
                out var corpusDirectory) // Retain the explicitly parsed corpus directory for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the confined payload locator from this strict object.
                RepositoryRelativePayloadPathProperty, // Select the exact repository-relative payload locator wire member for this operation.
                MaximumRepositoryRelativePathLength, // Apply the 1024-character relative-path limit to the confined payload locator.
                out var payloadPath) // Retain the explicitly parsed payload path for the subsequent complete owner checks.
            || !TryReadLong( // Require the selected JSON length to fit its existing Int64 owner bound.
                element, // Read the exact raw payload length from this strict object.
                PayloadByteLengthProperty, // Select the exact raw payload length wire member for this operation.
                out var payloadByteLength) // Retain the explicitly parsed payload byte length for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the exact raw payload digest from this strict object.
                PayloadSha256Property, // Select the exact raw payload digest wire member for this operation.
                HashHexLength, // Cap the bare raw-payload digest at exactly 64 characters.
                out var payloadSha256) // Retain the explicitly parsed payload sha256 for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the typed manifest locator from this strict object.
                RepositoryRelativeManifestPathProperty, // Select the exact repository-relative manifest locator wire member for this operation.
                MaximumRepositoryRelativePathLength, // Apply the 1024-character relative-path limit to the typed manifest locator.
                out var manifestPath) // Retain the explicitly parsed manifest path for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the sealed manifest self-hash from this strict object.
                ManifestSha256Property, // Select the exact sealed manifest self-hash wire member for this operation.
                HashHexLength, // Cap the bare sealed-manifest digest at exactly 64 characters.
                out var manifestSha256)) // Retain the explicitly parsed manifest sha256 for the subsequent complete owner checks.
        { // Reject the whole manifest when any required field/type or exact member set fails.
            return false; // Reject this failed strict owner predicate without a snapshot.
        } // Reject the whole manifest when any required field/type or exact member set fails.

        manifest = new RepositoryOwnedReferenceCorpusManifest( // Construct the complete manifest from all strictly parsed fields.
            schema, // Preserve schema in the complete persisted corpus representation.
            context, // Preserve context in the complete persisted corpus representation.
            domain, // Preserve domain in the complete persisted corpus representation.
            subject, // Preserve subject in the complete persisted corpus representation.
            corpusVersion, // Preserve corpus version in the complete persisted corpus representation.
            corpusName, // Preserve corpus name in the complete persisted corpus representation.
            frozenAtUtc, // Preserve frozen at utc in the complete persisted corpus representation.
            entryCount, // Preserve the declared complete entry count in the complete persisted corpus representation.
            corpusSha256, // Preserve corpus sha256 in the complete persisted corpus representation.
            corpusDirectory, // Preserve the parsed repository-relative corpus directory in the complete persisted corpus representation.
            payloadPath, // Preserve the confined payload locator in the complete persisted corpus representation.
            payloadByteLength, // Preserve the parsed raw payload length in the complete persisted corpus representation.
            payloadSha256, // Preserve the parsed raw payload digest in the complete persisted corpus representation.
            manifestPath, // Preserve the sealed-manifest locator in the complete persisted corpus representation.
            manifestSha256); // Preserve the parsed manifest self-hash in the complete persisted corpus representation.
        return true; // Accept only after every preceding exact owner check passed.
    } // Read every required manifest field before constructing its complete parsed value.

    private static bool TryReadPayloadElement( // Require exact payload members and reconstruct the whole bounded entry sequence.
        JsonElement element, // Read the payload object and every declared entry under its closed schema.
        out RepositoryOwnedReferenceCorpusPayload payload) // Return the complete parsed payload only after every entry has been read.
    { // Read the complete payload header and every declared entry before returning it.
        payload = EmptyPayload(); // Initialize the strict parser failure carrier without accepting it as a corpus.
        if (!ObjectHasExactProperties( // Reject unknown, duplicate or absent JSON members before reading values.
                element, // Check this object for its exact closed member set.
                SchemaProperty, // Select the exact schema discriminator wire member for this operation.
                ContextProperty, // Select the exact paid-reference custody context wire member for this operation.
                CorpusNameProperty, // Select the exact human corpus name wire member for this operation.
                CorpusVersionProperty, // Select the exact frozen corpus version wire member for this operation.
                FrozenAtUtcProperty, // Select the exact corpus freeze timestamp wire member for this operation.
                DomainProperty, // Select the exact corpus domain wire member for this operation.
                SubjectProperty, // Select the exact corpus subject wire member for this operation.
                EntryCountProperty, // Select the exact complete payload entry count wire member for this operation.
                CorpusSha256Property, // Select the exact whole-corpus canonical identity wire member for this operation.
                CanonicalRepresentationProperty, // Select the exact full canonical representation wire member for this operation.
                EntriesProperty) // Select the exact entire frozen-entry sequence wire member for this operation.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the format discriminator from this strict object.
                SchemaProperty, // Select the exact schema discriminator wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the format discriminator.
                out var schema) // Retain the explicitly parsed schema for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the paid-reference corpus context from this strict object.
                ContextProperty, // Select the exact paid-reference custody context wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the paid-reference corpus context.
                out var context) // Retain the explicitly parsed context for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the factory-derived corpus display name from this strict object.
                CorpusNameProperty, // Select the exact human corpus name wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the factory-derived corpus display name.
                out var corpusName) // Retain the explicitly parsed corpus name for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the frozen corpus version from this strict object.
                CorpusVersionProperty, // Select the exact frozen corpus version wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the frozen corpus version.
                out var corpusVersion) // Retain the explicitly parsed corpus version for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the canonical corpus freeze timestamp from this strict object.
                FrozenAtUtcProperty, // Select the exact corpus freeze timestamp wire member for this operation.
                MaximumUtcTextLength, // Apply the 64-character UTC-text limit to the canonical corpus freeze timestamp.
                out var frozenAtUtc) // Retain the explicitly parsed frozen at utc for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the corpus domain slug from this strict object.
                DomainProperty, // Select the exact corpus domain wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the corpus domain slug.
                out var domain) // Retain the explicitly parsed domain for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the corpus subject slug from this strict object.
                SubjectProperty, // Select the exact corpus subject wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the corpus subject slug.
                out var subject) // Retain the explicitly parsed subject for the subsequent complete owner checks.
            || !TryReadInt(element, EntryCountProperty, out var entryCount) // Require the selected JSON count to fit its existing Int32 owner bound.
            || entryCount <= 0 // Reject an empty declared payload entry sequence.
            || entryCount > MaximumEntryCount // Keep the declared full entry count within the strict 10,000-entry cap.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the whole-corpus identity from this strict object.
                CorpusSha256Property, // Select the exact whole-corpus canonical identity wire member for this operation.
                "sha256:".Length + HashHexLength, // Bound this owner digest to the exact sha256: prefix plus 64 lowercase hex characters.
                out var corpusSha256) // Retain the explicitly parsed corpus sha256 for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the complete owner canonical representation from this strict object.
                CanonicalRepresentationProperty, // Select the exact full canonical representation wire member for this operation.
                MaximumCanonicalTextLength, // Apply the 262,144-character canonical-text limit to the complete owner canonical representation.
                out var canonicalRepresentation) // Retain the explicitly parsed canonical representation for the subsequent complete owner checks.
            || !element.TryGetProperty(EntriesProperty, out var entriesElement) // Require the explicit wire member rather than infer a missing value.
            || entriesElement.ValueKind != JsonValueKind.Array) // Require the exact Array JSON primitive or container before reading this owner member.
        { // Reject incomplete payload headers, invalid counts or a missing complete entry array.
            return false; // Reject this failed strict owner predicate without a snapshot.
        } // Reject incomplete payload headers, invalid counts or a missing complete entry array.

        var raw = schema == RawUtf8PayloadSchema && context == RawUtf8Context &&
            domain == FrozenReferenceEntry.RawCodeDocumentationDomain;
        var native = schema == NativeCapturePayloadSchema && context == NativeCaptureContext && domain == FrozenReferenceEntry.RawCodeDocumentationDomain; // Explicit native tuple has its own strict entry branch.
        if (!raw && !native && !(schema == CurrentPayloadSchema && context == CurrentContext)) return false;

        var entries = new List<RepositoryOwnedReferenceCorpusPayloadEntry>( // Accumulate every persisted entry before declaring the complete corpus valid.
            entryCount); // Reserve storage for the declared complete entry count.
        foreach (var entryElement in entriesElement.EnumerateArray()) // Visit the entire corpus entry sequence without implicit truncation.
        { // Parse every entry before accepting the declared complete payload count.
            if (entries.Count >= MaximumEntryCount // Reject an additional entry after the strict full entry cap is reached.
                || !TryReadPayloadEntry(entryElement, raw, native, out var entry)) // Reject missing, extra or invalid entry fields before domain reconstruction.
            { // Reject an entry beyond the hard cap or an incomplete/invalid entry representation.
                return false; // Reject this failed strict owner predicate without a snapshot.
            } // Reject an entry beyond the hard cap or an incomplete/invalid entry representation.

            entries.Add(entry); // Preserve this successfully parsed payload entry in its complete supplied sequence.
        } // Parse every entry before accepting the declared complete payload count.

        if (entries.Count != entryCount) // Require the parsed complete entry count to equal its declared count.
            return false; // Reject this failed strict owner predicate without a snapshot.

        payload = new RepositoryOwnedReferenceCorpusPayload( // Construct the complete parsed payload with all validated entry representations.
            schema, // Preserve schema in the complete persisted corpus representation.
            context, // Preserve context in the complete persisted corpus representation.
            corpusName, // Preserve corpus name in the complete persisted corpus representation.
            corpusVersion, // Preserve corpus version in the complete persisted corpus representation.
            frozenAtUtc, // Preserve frozen at utc in the complete persisted corpus representation.
            domain, // Preserve domain in the complete persisted corpus representation.
            subject, // Preserve subject in the complete persisted corpus representation.
            entryCount, // Preserve the declared complete entry count in the complete persisted corpus representation.
            corpusSha256, // Preserve corpus sha256 in the complete persisted corpus representation.
            canonicalRepresentation, // Preserve the persisted full canonical representation in the complete persisted corpus representation.
            Array.AsReadOnly(entries.ToArray())); // Freeze every parsed entry in a read-only snapshot before returning the payload.
        return true; // Accept only after every preceding exact owner check passed.
    } // Read the complete payload header and every declared entry before returning it.

    private static bool TryReadPayloadEntry( // Reject missing, extra or invalid entry fields before domain reconstruction.
        JsonElement element, bool rawUtf8, bool nativeCapture, // Read one complete entry and its nested provenance from this strict object.
        out RepositoryOwnedReferenceCorpusPayloadEntry entry) // Return the whole parsed entry rather than an identity-only summary.
    { // Read all frozen-entry fields and its complete nested paid provenance.
        entry = EmptyEntry(); // Initialize the strict parser failure carrier without accepting it as an entry.
        if (nativeCapture) return TryReadNativePayloadEntry(element, out entry); // Native schema accepts only its separate origin object.
        if (!ObjectHasExactProperties( // Reject unknown, duplicate or absent JSON members before reading values.
                element, // Check this object for its exact closed member set.
                EntryIdProperty, // Select the exact canonical frozen-entry identity wire member for this operation.
                OwnerInputProperty, // Select the exact owner input for this closed case wire member for this operation.
                PaidModelReferenceAnswerProperty, // Select the exact closed paid-reference answer wire member for this operation.
                DomainProperty, // Select the exact corpus domain wire member for this operation.
                SubjectProperty, // Select the exact corpus subject wire member for this operation.
                PartitionProperty, // Select the exact train/held-out partition wire member for this operation.
                SourceEntityFingerprintProperty, // Select the exact independent source fingerprint wire member for this operation.
                CanonicalRepresentationProperty, // Select the exact full canonical representation wire member for this operation.
                PaidReferenceProvenanceProperty) // Select the exact complete closed-answer provenance wire member for this operation.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the canonical frozen-entry identity from this strict object.
                EntryIdProperty, // Select the exact canonical frozen-entry identity wire member for this operation.
                "sha256:".Length + HashHexLength, // Bound this owner digest to the exact sha256: prefix plus 64 lowercase hex characters.
                out var entryId) // Retain the explicitly parsed entry id for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the original owner input from this strict object.
                OwnerInputProperty, // Select the exact owner input for this closed case wire member for this operation.
                MaximumContentTextLength, // Apply the 131,072-character input/answer limit to the original owner input.
                out var ownerInput) // Retain the explicitly parsed owner input for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the closed paid-reference answer from this strict object.
                PaidModelReferenceAnswerProperty, // Select the exact closed paid-reference answer wire member for this operation.
                MaximumContentTextLength, // Apply the 131,072-character input/answer limit to the closed paid-reference answer.
                out var referenceAnswer) // Retain the explicitly parsed reference answer for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the corpus domain slug from this strict object.
                DomainProperty, // Select the exact corpus domain wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the corpus domain slug.
                out var domain) // Retain the explicitly parsed domain for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the corpus subject slug from this strict object.
                SubjectProperty, // Select the exact corpus subject wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the corpus subject slug.
                out var subject) // Retain the explicitly parsed subject for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the explicit train or held-out partition from this strict object.
                PartitionProperty, // Select the exact train/held-out partition wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the explicit train or held-out partition.
                out var partition) // Retain the explicitly parsed partition for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the independent source fingerprint from this strict object.
                SourceEntityFingerprintProperty, // Select the exact independent source fingerprint wire member for this operation.
                "sha256:".Length + HashHexLength, // Bound this owner digest to the exact sha256: prefix plus 64 lowercase hex characters.
                out var sourceFingerprint) // Retain the explicitly parsed source fingerprint for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the complete owner canonical representation from this strict object.
                CanonicalRepresentationProperty, // Select the exact full canonical representation wire member for this operation.
                MaximumCanonicalTextLength, // Apply the 262,144-character canonical-text limit to the complete owner canonical representation.
                out var canonicalRepresentation) // Retain the explicitly parsed canonical representation for the subsequent complete owner checks.
            || !element.TryGetProperty( // Require the explicit wire member rather than infer a missing value.
                PaidReferenceProvenanceProperty, // Select the exact complete closed-answer provenance wire member for this operation.
                out var provenanceElement) // Retain the explicitly parsed provenance element for the subsequent complete owner checks.
            || !TryReadPayloadProvenance( // Require the full bounded paid-provenance representation.
                provenanceElement, rawUtf8, // Strictly read the complete nested provenance object for this entry.
                out var provenance)) // Retain the explicitly parsed provenance for the subsequent complete owner checks.
        { // Reject an incomplete entry or its missing/invalid nested paid provenance.
            return false; // Reject this failed strict owner predicate without a snapshot.
        } // Reject an incomplete entry or its missing/invalid nested paid provenance.

        entry = new RepositoryOwnedReferenceCorpusPayloadEntry( // Construct this entry from its complete strictly parsed values and provenance.
            entryId, // Preserve entry id in the complete persisted corpus representation.
            ownerInput, // Preserve owner input in the complete persisted corpus representation.
            referenceAnswer, // Preserve the closed paid-reference answer in the complete persisted corpus representation.
            domain, // Preserve domain in the complete persisted corpus representation.
            subject, // Preserve subject in the complete persisted corpus representation.
            partition, // Preserve the admitted frozen-reference partition in the complete persisted corpus representation.
            sourceFingerprint, // Preserve the independent source fingerprint in the complete persisted corpus representation.
            canonicalRepresentation, // Preserve the persisted full canonical representation in the complete persisted corpus representation.
            provenance); // Retain the full parsed provenance for later factory reconstruction.
        return true; // Accept only after every preceding exact owner check passed.
    } // Read all frozen-entry fields and its complete nested paid provenance.

    private static bool TryReadPayloadProvenance( // Require the full bounded paid-provenance representation.
        JsonElement element, bool rawUtf8, // Read every required paid-provenance field from this strict object.
        out RepositoryOwnedReferenceCorpusPayloadProvenance provenance) // Return the full parsed provenance for later owner-factory reconstruction.
    { // Read all eight provenance fields before factory reconstruction.
        provenance = EmptyProvenance(); // Initialize the strict parser failure carrier without accepting it as provenance.
        if (!ObjectHasExactProperties( // Reject unknown, duplicate or absent JSON members before reading values.
                element, // Check this object for its exact closed member set.
                ProviderRefProperty, // Select the exact paid-reference provider identity wire member for this operation.
                ModelIdProperty, // Select the exact paid-reference model identity wire member for this operation.
                ModelVersionProperty, // Select the exact paid-reference model version wire member for this operation.
                RequestIdProperty, // Select the exact paid-reference capture request identity wire member for this operation.
                CapturedAtUtcProperty, // Select the exact paid-reference capture timestamp wire member for this operation.
                rawUtf8 ? ReferenceAnswerRawUtf8Sha256Property : ReferenceAnswerSha256Property, // Select only the raw-v2 or legacy-v1 commitment field required by this version.
                ProvenanceSha256Property, // Select the exact complete provenance digest wire member for this operation.
                CanonicalRepresentationProperty) // Select the exact full canonical representation wire member for this operation.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the paid-reference provider identity from this strict object.
                ProviderRefProperty, // Select the exact paid-reference provider identity wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the paid-reference provider identity.
                out var providerRef) // Retain the explicitly parsed provider ref for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the paid-reference model identity from this strict object.
                ModelIdProperty, // Select the exact paid-reference model identity wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the paid-reference model identity.
                out var modelId) // Retain the explicitly parsed model id for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the paid-reference model version from this strict object.
                ModelVersionProperty, // Select the exact paid-reference model version wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the paid-reference model version.
                out var modelVersion) // Retain the explicitly parsed model version for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the paid-reference request identity from this strict object.
                RequestIdProperty, // Select the exact paid-reference capture request identity wire member for this operation.
                MaximumIdentityTextLength, // Apply the 256-character identity limit to the paid-reference request identity.
                out var requestId) // Retain the explicitly parsed request id for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the canonical paid-reference capture timestamp from this strict object.
                CapturedAtUtcProperty, // Select the exact paid-reference capture timestamp wire member for this operation.
                MaximumUtcTextLength, // Apply the 64-character UTC-text limit to the canonical paid-reference capture timestamp.
                out var capturedAtUtc) // Retain the explicitly parsed captured at utc for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the closed reference-answer digest from this strict object.
                rawUtf8 ? ReferenceAnswerRawUtf8Sha256Property : ReferenceAnswerSha256Property, // Select only the raw-v2 or legacy-v1 commitment field required by this version.
                "sha256:".Length + HashHexLength, // Bound this owner digest to the exact sha256: prefix plus 64 lowercase hex characters.
                out var referenceAnswerSha256) // Retain the explicitly parsed reference answer sha256 for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the complete provenance digest from this strict object.
                ProvenanceSha256Property, // Select the exact complete provenance digest wire member for this operation.
                "sha256:".Length + HashHexLength, // Bound this owner digest to the exact sha256: prefix plus 64 lowercase hex characters.
                out var provenanceSha256) // Retain the explicitly parsed provenance sha256 for the subsequent complete owner checks.
            || !TryReadString( // Require a bounded non-null JSON string for the selected owner member.
                element, // Read the complete owner canonical representation from this strict object.
                CanonicalRepresentationProperty, // Select the exact full canonical representation wire member for this operation.
                MaximumCanonicalTextLength, // Apply the 262,144-character canonical-text limit to the complete owner canonical representation.
                out var canonicalRepresentation)) // Retain the explicitly parsed canonical representation for the subsequent complete owner checks.
        { // Reject the complete provenance value when any exact field cannot be read.
            return false; // Reject this failed strict owner predicate without a snapshot.
        } // Reject the complete provenance value when any exact field cannot be read.

        provenance = new RepositoryOwnedReferenceCorpusPayloadProvenance( // Construct the complete paid provenance from all eight strictly read fields.
            providerRef, // Preserve provider ref in the complete persisted corpus representation.
            modelId, // Preserve model id in the complete persisted corpus representation.
            modelVersion, // Preserve model version in the complete persisted corpus representation.
            requestId, // Preserve request id in the complete persisted corpus representation.
            capturedAtUtc, // Preserve the exactly parsed capture timestamp in the complete persisted corpus representation.
            rawUtf8 ? string.Empty : referenceAnswerSha256, // Preserve reference answer sha256 in the complete persisted corpus representation.
            provenanceSha256, // Preserve provenance sha256 in the complete persisted corpus representation.
            canonicalRepresentation, rawUtf8 ? referenceAnswerSha256 : null); // Preserve the persisted full canonical representation in the complete persisted corpus representation.
        return true; // Accept only after every preceding exact owner check passed.
    } // Read all eight provenance fields before factory reconstruction.

    private static OperationResult<string> ResolvePayloadPath( // Confine the manifest-selected payload to the canonical reference-corpus address.
        RepositoryOwnedReferenceCorpusAddress address, // Use the typed corpus root and fixed payload locator as confinement authority.
        RepositoryOwnedReferenceCorpusManifest manifest) // Validate the manifest-selected locator against that typed address.
    { // Require the fixed corpus.json locator inside both corpus and repository boundaries.
        if (Path.IsPathRooted(manifest.RepositoryRelativePayloadPath) // Reject a rooted supplied locator that could escape the typed repository address.
            || manifest.RepositoryRelativePayloadPath.Contains('\\') // Reject backslash spelling in the repository-relative payload locator.
            || manifest.RepositoryRelativePayloadPath.Contains("//") // Reject doubled path separators in the payload locator.
            || manifest.RepositoryRelativePayloadPath == "." // Reject the current-directory alias as a payload locator.
            || manifest.RepositoryRelativePayloadPath == ".." // Reject the parent-directory alias as a payload locator.
            || manifest.RepositoryRelativePayloadPath.StartsWith( // Check the exact prefix used to confine a path or identify an owned staging name.
                "../", // Reject a leading parent traversal in the repository-relative payload locator.
                StringComparison.Ordinal) // Use exact ordinal spelling when testing that traversal prefix.
            || !StringComparer.Ordinal.Equals( // Require the manifest locator to equal the fixed corpus.json relative path.
                manifest.RepositoryRelativePayloadPath, // Compare the manifest-selected locator with the fixed typed payload locator.
                PayloadRelativePath(address))) // Derive the fixed payload locator from the typed corpus address.
        { // Reject payload traversal/alias spelling or disagreement with the fixed typed locator.
            return Invalid<string>(); // Reject invalid descriptive input with the existing ordinary failure contract.
        } // Reject payload traversal/alias spelling or disagreement with the fixed typed locator.

        var payloadPath = PayloadPath(address); // Resolve the fixed payload file beneath the supplied corpus root.
        if (!IsStrictDescendant(payloadPath, address.CorpusRoot) // Require the resolved payload to be strictly inside its typed corpus root.
            || !IsStrictDescendant(payloadPath, address.RepositoryRoot) // Require a path strictly beneath the resolved repository boundary.
            || Directory.Exists(payloadPath) // Reject a directory occupying the required owner artifact file locator.
            || AnyExistingPathSegmentIsReparsePoint(payloadPath)) // Reject reparse traversal inside the resolved corpus boundary.
        { // Reject payload locators outside either root, directories or reparse traversal.
            return Invalid<string>(); // Reject invalid descriptive input with the existing ordinary failure contract.
        } // Reject payload locators outside either root, directories or reparse traversal.

        return OperationResult<string>.Ok(payloadPath); // Return the validated typed owner value after the preceding admission checks.
    } // Require the fixed corpus.json locator inside both corpus and repository boundaries.

    private static OperationResult<bool> PublishBytesWithoutOverwrite( // Publish only owned staged bytes without overwriting a conflicting sealed artifact.
        RepositoryOwnedReferenceCorpusAddress address, // Use this typed corpus root to confine both staging and final publication.
        string finalPath, // Publish at this fixed final locator without overwriting another artifact.
        byte[] bytes, // Write and verify the entire admitted publication byte sequence.
        long maximumBytes, // Limit final readback to the applicable manifest or payload byte cap.
        string tempPrefix, bool nativeCapture = false) // Native origin opts into strict private staging without changing either legacy schema.
    { // Publish only this attempt's secured staging bytes and verify the final readback.
        var temporaryPath = Path.Combine( // Resolve the explicit fixed child locator beneath its already-admitted parent.
            address.CorpusRoot, // Create this unique staging path only inside the typed corpus root.
            tempPrefix + Guid.NewGuid().ToString("N") + TempSuffix); // Create a unique staging name within this exact admitted corpus root.
        var published = false; // Track whether the owned staged path was moved to its final locator.
        try // Bound nonfatal failures around publish bytes without overwrite.
        { // Publish only this attempt's secured staging bytes and verify the final readback while retaining the existing exception boundary.
            if (!IsStrictDescendant(finalPath, address.CorpusRoot) // Reject a final artifact path outside the typed corpus root.
                || AnyExistingPathSegmentIsReparsePoint(finalPath) // Reject reparse traversal inside the resolved corpus boundary.
                || Directory.Exists(finalPath)) // Reject a directory occupying the required owner artifact file locator.
            { // Reject unsafe final publication paths before creating this attempt's staging file.
                return Unavailable<bool>(); // Report failed custody or nonfatal filesystem access without admission.
            } // Reject unsafe final publication paths before creating this attempt's staging file.

            using (var stream = OpenCorpusFile(nativeCapture, // Use existing strict native custody or unchanged legacy file creation.
                       temporaryPath, // Create only this attempt's uniquely named staging file.
                       FileMode.CreateNew, // Create only this attempt's unique staging file; never overwrite an existing artifact.
                       FileAccess.Write, // Open the owned staging handle solely for writing the complete publication bytes.
                       FileShare.None, // Exclude other handles while this attempt writes its owned staging bytes.
                       FileOptions.WriteThrough, // Request durable staging writes before the later explicit disk flush and publication.
                       FileBufferSize)) // Use the existing buffer size for this owned staging write handle.
            { // Write and flush the complete bytes on this attempt's secured staging handle.
                stream.Write(bytes); // Write the admitted byte sequence to the owned staging stream.
                stream.Flush(flushToDisk: true); // Flush the staged complete owner bytes before publishing their locator.
                if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath( // Bind the opened file handle to the requested path before trusting its bytes.
                        stream.SafeFileHandle, // Check custody against this exact already-opened file handle.
                        temporaryPath)) // Require the opened handle to belong to this attempt's unique staging path.
                { // Reject a staging handle that does not belong to this unique requested path.
                    return Unavailable<bool>(); // Report failed custody or nonfatal filesystem access without admission.
                } // Reject a staging handle that does not belong to this unique requested path.
            } // Write and flush the complete bytes on this attempt's secured staging handle.

            File.Move(temporaryPath, finalPath); // Publish the owned staged artifact with the existing no-overwrite move.
            published = true; // Record the move so cleanup does not delete a published artifact.
            var readback = ReadBoundedStableFileBytes(finalPath, maximumBytes, requireCustody: nativeCapture); // Read the complete bounded stable bytes used by later digest and parser checks.
            if (!readback.IsSuccess || readback.Value is null) // Require the bounded stable read of the newly published bytes before equality comparison.
                return readback.MapError<bool>(); // Propagate the published-file read failure without claiming publication success.
            return CanonicalBytesEqual(readback.Value, bytes) // Compare the complete canonical byte sequences without summary substitution.
                ? OperationResult<bool>.Ok(true) // Report that this attempt published and verified the exact complete bytes.
                : Unavailable<bool>(); // Report failed custody or nonfatal filesystem access without admission.
        } // Publish only this attempt's secured staging bytes and verify the final readback while retaining the existing exception boundary.
        catch (IOException) when (File.Exists(finalPath)) // Inspect an existing final file when a no-overwrite move loses a race.
        { // Check a racing final file and accept only identical complete bytes.
            var existing = ReadBoundedStableFileBytes(finalPath, maximumBytes, requireCustody: nativeCapture); // Read the complete bounded stable bytes used by later digest and parser checks.
            if (!existing.IsSuccess || existing.Value is null) // Require a bounded stable read of the racing owner's existing file.
                return existing.MapError<bool>(); // Propagate the racing-file read failure instead of treating it as matching bytes.
            return CanonicalBytesEqual(existing.Value, bytes) // Compare the complete canonical byte sequences without summary substitution.
                ? OperationResult<bool>.Ok(false) // Report matching bytes published by a racing owner, without claiming this attempt performed the move.
                : Conflict<bool>(); // Report conflicting sealed bytes without overwriting owner artifacts.
        } // Finish the no-overwrite race check without replacing the other owner's file.
        catch (Exception exception) // Convert only the permitted failure category into this owner result; fatal failures remain outside this catch.
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Convert only the permitted failure category into this owner result; fatal failures remain outside this catch.
        { // Return the existing PublishBytesWithoutOverwrite failure result after the permitted nonfatal exception.
            return Unavailable<bool>(); // Report failed custody or nonfatal filesystem access without admission.
        } // Return the existing PublishBytesWithoutOverwrite failure result after the permitted nonfatal exception.
        finally // Clean up this attempt's remaining staging file even when publication fails.
        { // Clean only the owned staging path when the final move did not publish it.
            if (!published) // Clean staging only when this attempt did not publish its final locator.
                DeleteOwnStaging(address, temporaryPath); // Delete only the staging file owned by this publication attempt.
        } // Clean only the owned staging path when the final move did not publish it.
    } // Publish only this attempt's secured staging bytes and verify the final readback.

    private static OperationResult<byte[]> ReadBoundedStableFileBytes( // Read the complete bounded stable bytes used by later digest and parser checks.
        string path, // Open only this resolved artifact locator for secured stable readback.
        long maximumBytes, bool requireCustody = false) // Native file reads add strict custody without altering legacy size/path semantics.
    { // Bind one secured read handle to the requested file and reject changing bytes.
        try // Bound nonfatal failures around read bounded stable file bytes.
        { // Bind one secured read handle to the requested file and reject changing bytes while retaining the existing exception boundary.
            var fullPath = Path.GetFullPath(path); // Resolve the complete filesystem path before confinement or handle identity checks.
            if (!SecureLocalFileBoundary.IsSupportedPlatform() // Require a supported strict-file platform before opening an owner artifact.
                || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint( // Reject reparse traversal inside the resolved corpus boundary.
                    fullPath) // Reject reparse traversal on the actual fully resolved path before opening.
                || !File.Exists(fullPath)) // Reject a missing artifact on this read-only owner path.
            { // Reject unsupported custody, reparse traversal or an absent file before opening.
                return Invalid<byte[]>(); // Reject invalid descriptive input with the existing ordinary failure contract.
            } // Reject unsupported custody, reparse traversal or an absent file before opening.

            using var stream = OpenCorpusFile(requireCustody, // Choose strict native custody or unchanged ordinary legacy handle creation.
                fullPath, // Open the fully resolved artifact path for one read-only stable-byte check.
                FileMode.Open, // Open an existing corpus artifact; this read path never creates a file.
                FileAccess.Read, // Open the artifact without write access.
                FileShare.Read, // Permit other readers while excluding a concurrent writer on this handle.
                FileOptions.SequentialScan, // Read the complete admitted artifact sequentially through the same handle.
                FileBufferSize); // Use the unchanged bounded chunk size for the stable owner read.
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath( // Bind the opened file handle to the requested path before trusting its bytes.
                    stream.SafeFileHandle, // Check custody against this exact already-opened file handle.
                    fullPath)) // Bind that handle to the actual fully resolved requested file.
            { // Reject a read handle that does not belong to the requested full path.
                return Invalid<byte[]>(); // Reject invalid descriptive input with the existing ordinary failure contract.
            } // Reject a read handle that does not belong to the requested full path.

            var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes( // Read only the admitted length and reject changes to the same opened file.
                stream, // Read the same secured handle twice for the owner's stable-byte check.
                maximumBytes); // Enforce the applicable raw-file cap before trusting either read.
            if (bytes is null // Reject missing the exact owner byte sequence before owner use.
                || bytes.Length == 0 // Reject an empty byte read before JSON parsing and pin validation.
                || !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath( // Bind the opened file handle to the requested path before trusting its bytes.
                    stream.SafeFileHandle, // Check custody against this exact already-opened file handle.
                    fullPath) // Recheck that the handle still belongs to the requested file after reading.
                || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint( // Reject reparse traversal inside the resolved corpus boundary.
                    fullPath)) // Recheck reparse traversal after the stable-byte read.
            { // Reject empty/unstable reads or custody changes observed after reading.
                return Invalid<byte[]>(); // Reject invalid descriptive input with the existing ordinary failure contract.
            } // Reject empty/unstable reads or custody changes observed after reading.

            return OperationResult<byte[]>.Ok(bytes); // Return the validated typed owner value after the preceding admission checks.
        } // Bind one secured read handle to the requested file and reject changing bytes while retaining the existing exception boundary.
        catch (Exception exception) // Convert only the permitted failure category into this owner result; fatal failures remain outside this catch.
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Convert only the permitted failure category into this owner result; fatal failures remain outside this catch.
        { // Return the existing ReadBoundedStableFileBytes failure result after the permitted nonfatal exception.
            return Unavailable<byte[]>(); // Report failed custody or nonfatal filesystem access without admission.
        } // Return the existing ReadBoundedStableFileBytes failure result after the permitted nonfatal exception.
    } // Bind one secured read handle to the requested file and reject changing bytes.

    private static OperationResult<bool> EnsureWritableCorpusRoot( // Require a confined, non-reparse corpus directory with safe owner permissions.
        RepositoryOwnedReferenceCorpusAddress address) // Check and establish only this confined typed corpus directory.
    { // Require a confined corpus directory with the existing owner-only custody.
        try // Bound nonfatal failures around ensure writable corpus root.
        { // Require a confined corpus directory with the existing owner-only custody while retaining the existing exception boundary.
            if (!SecureLocalFileBoundary.IsSupportedPlatform() // Require a supported strict-file platform before opening an owner artifact.
                || AnyExistingPathSegmentIsReparsePoint(address.CorpusRoot)) // Reject reparse traversal inside the resolved corpus boundary.
            { // Reject unsupported custody or reparse traversal before using the corpus directory.
                return Unavailable<bool>(); // Report failed custody or nonfatal filesystem access without admission.
            } // Reject unsupported custody or reparse traversal before using the corpus directory.

            if (Directory.Exists(address.CorpusRoot)) // Validate custody of the existing corpus directory before reusing it for publication.
            { // Reuse an existing corpus directory only under safe owner permissions.
                if (!HasSafeUnixDirectoryMode(address.CorpusRoot)) // Require owner-only corpus-directory access on Unix.
                    return Unavailable<bool>(); // Report failed custody or nonfatal filesystem access without admission.
            } // Reuse an existing corpus directory only under safe owner permissions.
            else if (File.Exists(address.CorpusRoot)) // Distinguish the existing owner artifact from an absent path before read or publication.
            { // Reject a regular file occupying the required corpus directory.
                return Unavailable<bool>(); // Report failed custody or nonfatal filesystem access without admission.
            } // Reject a regular file occupying the required corpus directory.
            else if (OperatingSystem.IsWindows()) // Use ordinary directory creation on Windows under the existing custody boundary.
            { // Create the confined corpus directory with the existing Windows custody path.
                Directory.CreateDirectory(address.CorpusRoot); // Create only the already-confined typed corpus root.
            } // Create the confined corpus directory with the existing Windows custody path.
            else // Apply explicit private directory permissions on the non-Windows path.
            { // Create the confined corpus directory with explicit owner-only Unix permissions.
                Directory.CreateDirectory(address.CorpusRoot, OwnerDirectoryMode); // Create the confined corpus root with owner-only Unix permissions.
            } // Create the confined corpus directory with explicit owner-only Unix permissions.

            if (AnyExistingPathSegmentIsReparsePoint(address.CorpusRoot) // Reject reparse traversal inside the resolved corpus boundary.
                || !HasSafeUnixDirectoryMode(address.CorpusRoot)) // Require owner-only corpus-directory access on Unix.
            { // Recheck reparse traversal and owner permissions after directory preparation.
                return Unavailable<bool>(); // Report failed custody or nonfatal filesystem access without admission.
            } // Recheck reparse traversal and owner permissions after directory preparation.

            return OperationResult<bool>.Ok(true); // Return the validated typed owner value after the preceding admission checks.
        } // Require a confined corpus directory with the existing owner-only custody while retaining the existing exception boundary.
        catch (Exception exception) // Convert only the permitted failure category into this owner result; fatal failures remain outside this catch.
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Convert only the permitted failure category into this owner result; fatal failures remain outside this catch.
        { // Return the existing EnsureWritableCorpusRoot failure result after the permitted nonfatal exception.
            return Unavailable<bool>(); // Report failed custody or nonfatal filesystem access without admission.
        } // Return the existing EnsureWritableCorpusRoot failure result after the permitted nonfatal exception.
    } // Require a confined corpus directory with the existing owner-only custody.

    private static OperationResult<ProcessSealLease> AcquireProcessSealLease( // Bound process-local sealing contention under the canonical corpus-root gate.
        string canonicalCorpusRoot, // Index and acquire the shared gate for this canonical corpus root.
        Stopwatch timeoutBudget) // Limit this gate acquisition by the same running timeout budget.
    { // Retain a root-specific registry reference and wait within the existing timeout.
        ProcessGateEntry entry; // Retain the process registry entry acquired under the canonical corpus-root key.
        lock (ProcessGateRegistrySync) // Serialize access to the shared process-gate registry.
        { // Keep lookup, creation and reference retention atomic under the registry lock.
            if (!ProcessGateRegistry.TryGetValue(canonicalCorpusRoot, out entry!)) // Reuse the registered gate for this exact canonical corpus root when present.
            { // Register one semaphore entry for a previously unseen canonical corpus root.
                entry = new ProcessGateEntry(); // Create one semaphore registry entry for this previously unregistered corpus root.
                ProcessGateRegistry.Add(canonicalCorpusRoot, entry); // Register the newly created gate under its canonical corpus-root key.
            } // Register one semaphore entry for a previously unseen canonical corpus root.

            checked // Reject registry reference-count overflow instead of wrapping the ownership count.
            { // Reject reference-count overflow while retaining this attempt's gate reference.
                entry.ReferenceCount++; // Retain one registry reference for this attempt before waiting on the gate.
            } // Reject reference-count overflow while retaining this attempt's gate reference.
        } // Keep lookup, creation and reference retention atomic under the registry lock.

        try // Bound nonfatal failures around acquire process seal lease.
        { // Retain a root-specific registry reference and wait within the existing timeout while retaining the existing exception boundary.
            var remaining = DefaultSealProcessGateTimeout - timeoutBudget.Elapsed; // Subtract actual elapsed time from the existing seal-gate wait budget.
            if (remaining <= TimeSpan.Zero || !entry.Semaphore.Wait(remaining)) // Wait only within the remaining seal-gate budget.
            { // Release the registry reference and report busy after the bounded gate wait fails.
                ReleaseProcessGateReference(canonicalCorpusRoot, entry); // Release this attempt's reference after failing to acquire the gate within its budget.
                return Busy<ProcessSealLease>(); // Report bounded process-gate contention.
            } // Release the registry reference and report busy after the bounded gate wait fails.

            return OperationResult<ProcessSealLease>.Ok( // Return the validated typed owner value after the preceding admission checks.
                new ProcessSealLease(canonicalCorpusRoot, entry)); // Bind the acquired semaphore and registry reference to one disposable lease for this root.
        } // Retain a root-specific registry reference and wait within the existing timeout while retaining the existing exception boundary.
        catch (Exception exception) // Convert only the permitted failure category into this owner result; fatal failures remain outside this catch.
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Convert only the permitted failure category into this owner result; fatal failures remain outside this catch.
        { // Release the retained registry reference after a nonfatal gate acquisition failure.
            ReleaseProcessGateReference(canonicalCorpusRoot, entry); // Release this attempt's registry reference after a nonfatal acquisition failure.
            return Unavailable<ProcessSealLease>(); // Report failed custody or nonfatal filesystem access without admission.
        } // Release the retained registry reference after a nonfatal gate acquisition failure.
    } // Retain a root-specific registry reference and wait within the existing timeout.

    private static void ReleaseProcessGateReference( // Release one registry reference and dispose the gate only after the last reference leaves.
        string canonicalCorpusRoot, // Remove only this root's gate when its final registry reference leaves.
        ProcessGateEntry entry) // Receive the exact process-gate registry entry whose lease or reference is being released.
    { // Remove a root gate only after its final registry reference leaves.
        SemaphoreSlim? semaphoreToDispose = null; // Delay disposal until the final registry reference is removed under lock.
        lock (ProcessGateRegistrySync) // Serialize access to the shared process-gate registry.
        { // Remove the gate only while its last reference and registry identity are locked.
            entry.ReferenceCount--; // Drop the calling attempt's registry reference under the registry lock.
            if (entry.ReferenceCount != 0) // Keep the gate registered while any other attempt still holds a reference.
                return; // Leave the still-referenced semaphore available for the remaining attempts.
            if (!ProcessGateRegistry.TryGetValue( // Require the registry to retain this same root entry before removing the last reference.
                    canonicalCorpusRoot, // Look up the exact canonical root whose final reference is being released.
                    out var registered) // Read the actual registered gate entry for the exact object-identity check.
                || !ReferenceEquals(registered, entry)) // Require the same registry entry before changing its references.
            { // Report loss of the exact registered entry instead of releasing another gate.
                throw new InvalidOperationException( // Report a corrupted process-gate registry invariant instead of releasing an unrelated entry.
                    "Reference corpus process-gate registry identity was lost."); // Report loss of this exact registry entry instead of releasing an unrelated gate.
            } // Report loss of the exact registered entry instead of releasing another gate.

            ProcessGateRegistry.Remove(canonicalCorpusRoot); // Remove only the no-longer-owned registry entry under its lock.
            semaphoreToDispose = entry.Semaphore; // Retain the now-unreferenced semaphore for disposal outside the registry lock.
        } // Remove the gate only while its last reference and registry identity are locked.

        semaphoreToDispose.Dispose(); // Dispose the removed semaphore after all attempt references have left.
    } // Remove a root gate only after its final registry reference leaves.

    private static void ReleaseOwnedProcessGate( // Release the acquired process gate and its registry reference.
        string canonicalCorpusRoot, // Release the semaphore acquired for this exact canonical corpus root.
        ProcessGateEntry entry) // Receive the exact process-gate registry entry whose lease or reference is being released.
    { // Release the acquired semaphore and always release its registry reference.
        try // Release the acquired semaphore while guaranteeing registry-reference release.
        { // Attempt semaphore release before the guaranteed reference cleanup.
            entry.Semaphore.Release(); // Release the acquired process-local gate.
        } // Finish the semaphore release attempt before its finally cleanup.
        finally // Release the owned registry reference even when semaphore release throws.
        { // Release this lease's registry reference even if semaphore release fails.
            ReleaseProcessGateReference(canonicalCorpusRoot, entry); // Drop this lease's registry reference after its semaphore release attempt.
        } // Release this lease's registry reference even if semaphore release fails.
    } // Release the acquired semaphore and always release its registry reference.

    private static byte[] WriteCanonicalManifestBytes( // Serialize all manifest fields in fixed legacy order; hash callers clear only the self-hash value.
        RepositoryOwnedReferenceCorpusManifest manifest) // Serialize this complete manifest in the unchanged legacy field order.
    { // Emit all legacy manifest fields in their unchanged fixed wire order.
        using var buffer = new MemoryStream(); // Accumulate all legacy manifest fields in a private byte buffer.
        using (var writer = new Utf8JsonWriter(buffer)) // Serialize fixed owner JSON into the private canonical byte buffer.
        { // Finish the complete legacy manifest JSON before taking its byte array.
            writer.WriteStartObject(); // Start the unchanged complete manifest JSON object.
            writer.WriteString(SchemaProperty, manifest.Schema); // Persist the parsed sealed manifest's schema discriminator under schema property in the fixed legacy field order.
            writer.WriteString(ContextProperty, manifest.Context); // Persist the parsed sealed manifest's paid-reference custody context under context property in the fixed legacy field order.
            writer.WriteString(DomainProperty, manifest.Domain); // Persist the parsed sealed manifest's corpus domain under domain property in the fixed legacy field order.
            writer.WriteString(SubjectProperty, manifest.Subject); // Persist the parsed sealed manifest's corpus subject under subject property in the fixed legacy field order.
            writer.WriteString(CorpusVersionProperty, manifest.CorpusVersion); // Persist the parsed sealed manifest's frozen corpus version under corpus version property in the fixed legacy field order.
            writer.WriteString(CorpusNameProperty, manifest.CorpusName); // Persist the parsed sealed manifest's human corpus name under corpus name property in the fixed legacy field order.
            writer.WriteString(FrozenAtUtcProperty, manifest.FrozenAtUtc); // Persist the parsed sealed manifest's corpus freeze timestamp under frozen at utc property in the fixed legacy field order.
            writer.WriteNumber(EntryCountProperty, manifest.EntryCount); // Persist the parsed sealed manifest's complete payload entry count under entry count property in the fixed legacy field order.
            writer.WriteString(CorpusSha256Property, manifest.CorpusSha256); // Persist the parsed sealed manifest's whole-corpus canonical identity under corpus sha256 property in the fixed legacy field order.
            writer.WriteString( // Persist the parsed sealed manifest's repository-relative corpus directory under repository relative corpus directory property in the fixed legacy field order.
                RepositoryRelativeCorpusDirectoryProperty, // Select the exact repository-relative corpus directory wire member for this operation.
                manifest.RepositoryRelativeCorpusDirectory); // Write the exact typed repository-relative corpus directory.
            writer.WriteString( // Persist the parsed sealed manifest's repository-relative payload locator under repository relative payload path property in the fixed legacy field order.
                RepositoryRelativePayloadPathProperty, // Select the exact repository-relative payload locator wire member for this operation.
                manifest.RepositoryRelativePayloadPath); // Write the exact repository-relative payload locator.
            writer.WriteNumber( // Persist the parsed sealed manifest's exact raw payload length under payload byte length property in the fixed legacy field order.
                PayloadByteLengthProperty, // Select the exact raw payload length wire member for this operation.
                manifest.PayloadByteLength); // Write the complete raw payload byte length.
            writer.WriteString(PayloadSha256Property, manifest.PayloadSha256); // Persist the parsed sealed manifest's exact raw payload digest under payload sha256 property in the fixed legacy field order.
            writer.WriteString( // Persist the parsed sealed manifest's repository-relative manifest locator under repository relative manifest path property in the fixed legacy field order.
                RepositoryRelativeManifestPathProperty, // Select the exact repository-relative manifest locator wire member for this operation.
                manifest.RepositoryRelativeManifestPath); // Write the exact repository-relative manifest locator.
            writer.WriteString(ManifestSha256Property, manifest.ManifestSha256); // Serialize the supplied self-hash value; hash derivation supplies an empty value.
            writer.WriteEndObject(); // Close the complete manifest object after its supplied self-hash value.
            writer.Flush(); // Finish writing the owned byte representation before publication.
        } // Finish the complete legacy manifest JSON before taking its byte array.

        return buffer.ToArray(); // Return the complete canonical serialized owner bytes after closing the writer.
    } // Emit all legacy manifest fields in their unchanged fixed wire order.

    private static byte[] WriteCanonicalPayloadBytes( // Emit the existing complete canonical corpus payload.
        RepositoryOwnedReferenceCorpusPayload payload) // Serialize this complete payload header and every stored entry.
    { // Emit the complete frozen-corpus payload and every persisted entry.
        using var buffer = new MemoryStream(); // Accumulate the complete payload and every frozen entry in a private byte buffer.
        using (var writer = new Utf8JsonWriter(buffer)) // Serialize fixed owner JSON into the private canonical byte buffer.
        { // Finish the complete payload and entry array before taking its byte array.
            writer.WriteStartObject(); // Start the complete frozen-corpus payload JSON object.
            writer.WriteString(SchemaProperty, payload.Schema); // Persist the complete parsed payload's schema discriminator under schema property in the fixed legacy field order.
            writer.WriteString(ContextProperty, payload.Context); // Persist the complete parsed payload's paid-reference custody context under context property in the fixed legacy field order.
            writer.WriteString(CorpusNameProperty, payload.CorpusName); // Persist the complete parsed payload's human corpus name under corpus name property in the fixed legacy field order.
            writer.WriteString(CorpusVersionProperty, payload.CorpusVersion); // Persist the complete parsed payload's frozen corpus version under corpus version property in the fixed legacy field order.
            writer.WriteString(FrozenAtUtcProperty, payload.FrozenAtUtc); // Persist the complete parsed payload's corpus freeze timestamp under frozen at utc property in the fixed legacy field order.
            writer.WriteString(DomainProperty, payload.Domain); // Persist the complete parsed payload's corpus domain under domain property in the fixed legacy field order.
            writer.WriteString(SubjectProperty, payload.Subject); // Persist the complete parsed payload's corpus subject under subject property in the fixed legacy field order.
            writer.WriteNumber(EntryCountProperty, payload.EntryCount); // Persist the complete parsed payload's complete payload entry count under entry count property in the fixed legacy field order.
            writer.WriteString(CorpusSha256Property, payload.CorpusSha256); // Persist the complete parsed payload's whole-corpus canonical identity under corpus sha256 property in the fixed legacy field order.
            writer.WriteString( // Persist the complete parsed payload's full canonical representation under canonical representation property in the fixed legacy field order.
                CanonicalRepresentationProperty, // Select the exact full canonical representation wire member for this operation.
                payload.CanonicalRepresentation); // Persist the full corpus canonical representation for reconstruction.
            writer.WriteStartArray(EntriesProperty); // Begin the complete entry sequence in the existing owner payload.
            foreach (var entry in payload.Entries) // Visit the entire corpus entry sequence without implicit truncation.
                WritePayloadEntry(writer, entry); // Emit each frozen entry and its exact paid provenance in the canonical payload.
            writer.WriteEndArray(); // Finish the full persisted entry sequence.
            writer.WriteEndObject(); // Close the complete payload after its entire entry array.
            writer.Flush(); // Finish writing the owned byte representation before publication.
        } // Finish the complete payload and entry array before taking its byte array.

        return buffer.ToArray(); // Return the complete canonical serialized owner bytes after closing the writer.
    } // Emit the complete frozen-corpus payload and every persisted entry.

    private static void WritePayloadEntry( // Emit each frozen entry and its exact paid provenance in the canonical payload.
        Utf8JsonWriter writer, // Write this entry into the caller's existing complete payload writer.
        RepositoryOwnedReferenceCorpusPayloadEntry entry) // Write all stored entry fields and the complete nested provenance.
    { // Emit one complete frozen entry and its protected closed provenance.
        writer.WriteStartObject(); // Start one complete frozen-entry object.
        writer.WriteString(EntryIdProperty, entry.EntryId); // Persist the reconstructed frozen entry's canonical frozen-entry identity under entry id property in the fixed legacy field order.
        writer.WriteString(OwnerInputProperty, entry.OwnerInput); // Persist the reconstructed frozen entry's owner input for this closed case under owner input property in the fixed legacy field order.
        writer.WriteString( // Persist the reconstructed frozen entry's closed paid-reference answer under paid model reference answer property in the fixed legacy field order.
            PaidModelReferenceAnswerProperty, // Select the exact closed paid-reference answer wire member for this operation.
            entry.PaidModelReferenceAnswer); // Preserve the reconstructed frozen entry's closed paid-reference answer in the complete persisted corpus representation.
        writer.WriteString(DomainProperty, entry.Domain); // Persist the reconstructed frozen entry's corpus domain under domain property in the fixed legacy field order.
        writer.WriteString(SubjectProperty, entry.Subject); // Persist the reconstructed frozen entry's corpus subject under subject property in the fixed legacy field order.
        writer.WriteString(PartitionProperty, entry.Partition); // Persist the reconstructed frozen entry's train/held-out partition under partition property in the fixed legacy field order.
        writer.WriteString( // Persist the reconstructed frozen entry's independent source fingerprint under source entity fingerprint property in the fixed legacy field order.
            SourceEntityFingerprintProperty, // Select the exact independent source fingerprint wire member for this operation.
            entry.SourceEntityFingerprint); // Preserve the reconstructed frozen entry's independent source fingerprint in the complete persisted corpus representation.
        writer.WriteString( // Persist the reconstructed frozen entry's full canonical representation under canonical representation property in the fixed legacy field order.
            CanonicalRepresentationProperty, // Select the exact full canonical representation wire member for this operation.
            entry.CanonicalRepresentation); // Persist the full frozen-entry canonical representation.
        if (entry.NativeCaptureProvenance is { } native) // The native factory owns a distinct complete origin object.
        { // Emit no known-model identity members for native capture.
            writer.WritePropertyName(NativeCaptureProvenanceProperty); WriteNativeProvenance(writer, native); // Preserve the complete explicit origin in its canonical field order.
        } // Keep native capture separate from legacy passport serialization.
        else // Both known-model schemas retain their original emitted JSON.
        { // Legacy entries keep the fixed paid provenance member and field order.
            writer.WritePropertyName(PaidReferenceProvenanceProperty); // Emit the unchanged legacy origin member.
            WritePayloadProvenance(writer, entry.PaidReferenceProvenance!); // Strict reconstruction requires a non-null known-model origin.
        } // No null or new member is emitted for existing legacy entries.
        writer.WriteEndObject(); // Close this entry only after emitting its entire paid provenance.
    } // Emit one complete frozen entry and its protected closed provenance.

    private static void WritePayloadProvenance( // Emit every paid-reference provenance member in the existing fixed wire order.
        Utf8JsonWriter writer, // Write provenance into the caller's existing entry object.
        RepositoryOwnedReferenceCorpusPayloadProvenance provenance) // Write all eight stored provenance identity and representation fields.
    { // Emit all closed paid-provenance members in their existing fixed order.
        writer.WriteStartObject(); // Start the complete closed paid-provenance JSON object.
        writer.WriteString(ProviderRefProperty, provenance.ProviderRef); // Persist the reconstructed paid-reference provenance's paid-reference provider identity under provider ref property in the fixed legacy field order.
        writer.WriteString(ModelIdProperty, provenance.ModelId); // Persist the reconstructed paid-reference provenance's paid-reference model identity under model id property in the fixed legacy field order.
        writer.WriteString(ModelVersionProperty, provenance.ModelVersion); // Persist the reconstructed paid-reference provenance's paid-reference model version under model version property in the fixed legacy field order.
        writer.WriteString(RequestIdProperty, provenance.RequestId); // Persist the reconstructed paid-reference provenance's paid-reference capture request identity under request id property in the fixed legacy field order.
        writer.WriteString(CapturedAtUtcProperty, provenance.CapturedAtUtc); // Persist the reconstructed paid-reference provenance's paid-reference capture timestamp under captured at utc property in the fixed legacy field order.
        writer.WriteString( // Persist the reconstructed paid-reference provenance's normalized paid-reference answer digest under reference answer sha256 property in the fixed legacy field order.
            provenance.ReferenceAnswerRawUtf8Sha256 is null ? ReferenceAnswerSha256Property : ReferenceAnswerRawUtf8Sha256Property, // Select only the raw-v2 or legacy-v1 commitment field required by this version.
            provenance.ReferenceAnswerRawUtf8Sha256 ?? provenance.ReferenceAnswerSha256); // Preserve the reconstructed paid-reference provenance's normalized paid-reference answer digest in the complete persisted corpus representation.
        writer.WriteString(ProvenanceSha256Property, provenance.ProvenanceSha256); // Persist the reconstructed paid-reference provenance's complete provenance digest under provenance sha256 property in the fixed legacy field order.
        writer.WriteString( // Persist the reconstructed paid-reference provenance's full canonical representation under canonical representation property in the fixed legacy field order.
            CanonicalRepresentationProperty, // Select the exact full canonical representation wire member for this operation.
            provenance.CanonicalRepresentation); // Preserve the reconstructed paid-reference provenance's full canonical representation in the complete persisted corpus representation.
        writer.WriteEndObject(); // Close provenance only after all eight identity/representation fields.
    } // Emit all closed paid-provenance members in their existing fixed order.

    private static bool ObjectHasExactProperties( // Reject unknown, duplicate or absent JSON members before reading values.
        JsonElement element, // Inspect the entire strict object whose exact property set is required.
        params string[] expectedNames) // Use the complete allowed property set for this specific strict object.
    { // Reject unknown or duplicate members and require the complete exact property set.
        if (element.ValueKind != JsonValueKind.Object) // Require the exact Object JSON primitive or container before reading this owner member.
            return false; // Reject this failed strict owner predicate without a snapshot.

        var expected = new HashSet<string>(expectedNames, StringComparer.Ordinal); // Build exact-case allowed names for this strict object.
        var seen = new HashSet<string>(StringComparer.Ordinal); // Track exact-case names already seen to reject duplicates.
        foreach (var property in element.EnumerateObject()) // Visit the entire JSON member sequence to reject duplicates without implicit truncation.
        { // Reject unknown or repeated properties while inspecting the whole strict object.
            if (!expected.Contains(property.Name) || !seen.Add(property.Name)) // Reject an unknown or duplicate JSON member before comparing the complete property set.
                return false; // Reject this failed strict owner predicate without a snapshot.
        } // Reject unknown or repeated properties while inspecting the whole strict object.

        return seen.Count == expected.Count; // Require every allowed property to appear exactly once.
    } // Reject unknown or duplicate members and require the complete exact property set.

    private static bool TryReadString( // Require a bounded non-null JSON string for the selected owner member.
        JsonElement element, // Read the requested field from the already shape-checked strict object.
        string propertyName, // Select the exact existing wire member name.
        int maximumChars, // Apply this field's existing character ceiling.
        out string value) // Return only the parsed bounded string through the out parameter.
    { // Require the exact JSON string field within its specific character cap.
        value = string.Empty; // Initialize the string failure carrier before checking member type.
        if (!element.TryGetProperty(propertyName, out var property) // Require the explicit wire member rather than infer a missing value.
            || property.ValueKind != JsonValueKind.String) // Require the exact String JSON primitive or container before reading this owner member.
        { // Reject an absent member or a token other than the required JSON string.
            return false; // Reject this failed strict owner predicate without a snapshot.
        } // Reject an absent member or a token other than the required JSON string.

        value = property.GetString() ?? string.Empty; // Read the admitted JSON string without accepting another primitive type.
        return value.Length <= maximumChars; // Admit this parsed string only within its specific character ceiling.
    } // Require the exact JSON string field within its specific character cap.

    private static bool TryReadInt( // Require the selected JSON count to fit its existing Int32 owner bound.
        JsonElement element, // Read the selected count from its containing strict JSON object.
        string propertyName, // Select the exact count member required by the enclosing schema.
        out int value) // Return only a JSON number that fits the owner Int32 representation.
    { // Require the selected count field to be an exact Int32 JSON number.
        value = 0; // Initialize the integer failure carrier before member/type parsing.
        return element.TryGetProperty(propertyName, out var property) // Require the explicit wire member rather than infer a missing value.
               && property.ValueKind == JsonValueKind.Number // Require the exact Number JSON primitive or container before reading this owner member.
               && property.TryGetInt32(out value); // Require an exact Int32 integer rather than a floating, wrapped or oversized value.
    } // Require the selected count field to be an exact Int32 JSON number.

    private static bool TryReadLong( // Require the selected JSON length to fit its existing Int64 owner bound.
        JsonElement element, // Read the selected byte length from its containing strict JSON object.
        string propertyName, // Select the exact byte-length member required by the enclosing schema.
        out long value) // Return only a JSON number that fits the owner Int64 representation.
    { // Require the selected length field to be an exact Int64 JSON number.
        value = 0; // Initialize the length failure carrier before member/type parsing.
        return element.TryGetProperty(propertyName, out var property) // Require the explicit wire member rather than infer a missing value.
               && property.ValueKind == JsonValueKind.Number // Require the exact Number JSON primitive or container before reading this owner member.
               && property.TryGetInt64(out value); // Require an exact Int64 integer for the admitted owner length or wire value.
    } // Require the selected length field to be an exact Int64 JSON number.

    private static bool TryParseCanonicalUtc( // Require exact canonical UTC round-trip text for persisted provenance timestamps.
        string value, // Validate this timestamp text without normalizing alternate spellings.
        out DateTimeOffset parsed) // Return the exactly parsed timestamp for canonical UTC equality checking.
    { // Require exact invariant round-trip UTC spelling without normalization aliases.
        if (!DateTimeOffset.TryParseExact( // Require the exact canonical persisted timestamp representation.
                value, // Parse this exact timestamp text rather than normalize another spelling.
                "O", // Require round-trip timestamp text in the existing O format.
                CultureInfo.InvariantCulture, // Parse the persisted UTC timestamp using an invariant culture.
                DateTimeStyles.None, // Apply no implicit timestamp conversion or relaxed parsing flags.
                out parsed)) // Retain the exactly parsed timestamp for the subsequent canonical UTC equality check.
        { // Reject timestamps that do not parse in the exact invariant round-trip format.
            return false; // Reject this failed strict owner predicate without a snapshot.
        } // Reject timestamps that do not parse in the exact invariant round-trip format.

        return FrozenReferenceCanonicalization.IsUtc(parsed) // Require the persisted owner timestamp to be canonical UTC.
               && StringComparer.Ordinal.Equals( // Require the original timestamp text to equal the owner canonical UTC spelling.
                   FrozenReferenceCanonicalization.FormatUtc(parsed), // Use the existing exact UTC wire spelling for the owner timestamp.
                   value); // Require exact byte-equivalent UTC spelling, without timestamp normalization aliases.
    } // Require exact invariant round-trip UTC spelling without normalization aliases.

    private static bool TryParsePartition( // Admit only the existing explicit frozen-reference partitions.
        string value, // Recognize only the persisted train and held-out partition spellings.
        out FrozenReferencePartition partition) // Return the corresponding explicit owner partition only after exact recognition.
    { // Map only the existing train and held-out spellings to explicit partitions.
        if (StringComparer.Ordinal.Equals(value, "train")) // Recognize only the existing train partition spelling.
        { // Admit only the exact train spelling for the Train partition.
            partition = FrozenReferencePartition.Train; // Reconstruct the Train enum from that exact spelling.
            return true; // Admit this explicitly recognized train partition.
        } // Admit only the exact train spelling for the Train partition.

        if (StringComparer.Ordinal.Equals(value, "held-out")) // Recognize only the existing held-out partition spelling.
        { // Admit only the exact held-out spelling for the HeldOut partition.
            partition = FrozenReferencePartition.HeldOut; // Reconstruct the HeldOut enum from that exact spelling.
            return true; // Admit this explicitly recognized held-out partition.
        } // Admit only the exact held-out spelling for the HeldOut partition.

        partition = default; // Initialize only the failure carrier for an unrecognized partition.
        return false; // Reject every partition spelling outside train and held-out.
    } // Map only the existing train and held-out spellings to explicit partitions.

    private static string PayloadRelativePath( // Derive the fixed payload locator from the typed corpus address.
        RepositoryOwnedReferenceCorpusAddress address) // Use the admitted typed corpus address for complete input validation.
        => address.CorpusRootRelativePath + "/" + PayloadFileName; // Build the exact repository-relative corpus.json path from the typed directory.

    private static string PayloadPath(RepositoryOwnedReferenceCorpusAddress address) // Resolve the fixed payload file beneath the supplied corpus root.
        => Path.GetFullPath(Path.Combine(address.CorpusRoot, PayloadFileName)); // Resolve the explicit fixed child locator beneath its already-admitted parent.

    private static string CanonicalRoot(string path) // Normalize a root consistently for confinement and process-gate identity.
        => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path)); // Resolve the complete filesystem path before confinement or handle identity checks.

    private static string ComputeSha256(byte[] bytes) // Hash the exact owner byte sequence with lowercase SHA-256.
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(); // Hash the exact committed byte sequence with SHA-256.

    private static bool IsCanonicalSha256Hex(string? value) // Require the existing lowercase bare 64-character digest grammar.
        => value is { Length: HashHexLength } // Require exactly the existing 64-character digest width.
           && value.All(static character => character is >= '0' and <= '9' // Permit ASCII decimal digits in the bare SHA-256 digest.
               or >= 'a' and <= 'f'); // Permit lowercase a through f; uppercase digest aliases remain invalid.

    private static bool CanonicalBytesEqual(byte[] left, byte[] right) // Compare the complete canonical byte sequences without summary substitution.
        => left.Length == right.Length // Require equal complete byte lengths before fixed-time commitment comparison.
           && CryptographicOperations.FixedTimeEquals(left, right); // Compare complete canonical byte commitments without early prefix equality.

    private static bool ManifestsEqual( // Compare every sealed manifest identity required for idempotent publication.
        RepositoryOwnedReferenceCorpusManifest left, // Use the first comparison snapshot for complete input validation.
        RepositoryOwnedReferenceCorpusManifest right) // Use the second comparison snapshot for complete input validation.
        => StringComparer.Ordinal.Equals(left.Schema, right.Schema) // Require both sealed manifests to agree on schema.
           && StringComparer.Ordinal.Equals(left.Context, right.Context) // Require both sealed manifests to agree on paid-reference context.
           && StringComparer.Ordinal.Equals(left.Domain, right.Domain) // Require both sealed manifests to agree on corpus domain.
           && StringComparer.Ordinal.Equals(left.Subject, right.Subject) // Require both sealed manifests to agree on corpus subject.
           && StringComparer.Ordinal.Equals( // Require exact agreement between the first comparison snapshot's frozen corpus version and the second comparison snapshot's frozen corpus version.
               left.CorpusVersion, // Compare this frozen version with the corresponding field in the other complete snapshot.
               right.CorpusVersion) // Compare this frozen version with the corresponding field in the other complete snapshot.
           && StringComparer.Ordinal.Equals(left.CorpusName, right.CorpusName) // Require both sealed manifests to agree on corpus name.
           && StringComparer.Ordinal.Equals(left.FrozenAtUtc, right.FrozenAtUtc) // Require both sealed manifests to agree on exact freeze timestamp.
           && left.EntryCount == right.EntryCount // Require both sealed manifests to agree on complete entry count.
           && StringComparer.Ordinal.Equals(left.CorpusSha256, right.CorpusSha256) // Require both sealed manifests to agree on whole-corpus canonical digest.
           && StringComparer.Ordinal.Equals( // Require exact agreement between the first comparison snapshot's repository-relative corpus directory and the second comparison snapshot's repository-relative corpus directory.
               left.RepositoryRelativeCorpusDirectory, // Compare this corpus directory locator with the corresponding field in the other complete snapshot.
               right.RepositoryRelativeCorpusDirectory) // Compare this corpus directory locator with the corresponding field in the other complete snapshot.
           && StringComparer.Ordinal.Equals( // Require exact agreement between the first comparison snapshot's repository-relative payload locator and the second comparison snapshot's repository-relative payload locator.
               left.RepositoryRelativePayloadPath, // Compare this payload locator with the corresponding field in the other complete snapshot.
               right.RepositoryRelativePayloadPath) // Compare this payload locator with the corresponding field in the other complete snapshot.
           && left.PayloadByteLength == right.PayloadByteLength // Require both sealed manifests to agree on exact raw payload length.
           && StringComparer.Ordinal.Equals( // Require exact agreement between the first comparison snapshot's exact raw payload digest and the second comparison snapshot's exact raw payload digest.
               left.PayloadSha256, // Compare this raw payload digest with the corresponding field in the other complete snapshot.
               right.PayloadSha256) // Compare this raw payload digest with the corresponding field in the other complete snapshot.
           && StringComparer.Ordinal.Equals( // Require exact agreement between the first comparison snapshot's repository-relative manifest locator and the second comparison snapshot's repository-relative manifest locator.
               left.RepositoryRelativeManifestPath, // Compare this manifest locator with the corresponding field in the other complete snapshot.
               right.RepositoryRelativeManifestPath) // Compare this manifest locator with the corresponding field in the other complete snapshot.
           && StringComparer.Ordinal.Equals( // Require exact agreement between the first comparison snapshot's sealed manifest self-hash and the second comparison snapshot's sealed manifest self-hash.
               left.ManifestSha256, // Compare this sealed self-hash with the corresponding field in the other complete snapshot.
               right.ManifestSha256); // Compare this sealed self-hash with the corresponding field in the other complete snapshot.

    private static bool PathsEqual(string first, string second) // Use the platform path comparison required by the typed repository owner.
        => string.Equals( // Compare fully resolved roots under the platform's path identity rule.
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(first)), // Resolve the complete filesystem path before confinement or handle identity checks.
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(second)), // Resolve the complete filesystem path before confinement or handle identity checks.
            OperatingSystem.IsWindows() // Select the existing platform path comparison policy.
                ? StringComparison.OrdinalIgnoreCase // Compare confined Windows paths using the existing case-insensitive platform policy.
                : StringComparison.Ordinal); // Compare confined non-Windows paths using the existing case-sensitive platform policy.

    private static bool IsStrictDescendant(string candidate, string root) // Require a path strictly beneath the resolved repository boundary.
    { // Use a separator-terminated root prefix and reject the root itself.
        var normalizedCandidate = Path.TrimEndingDirectorySeparator( // Normalize only the trailing separator for exact root comparisons.
            Path.GetFullPath(candidate)); // Resolve the candidate to a full path before the separator-bounded descendant check.
        var normalizedRoot = Path.TrimEndingDirectorySeparator( // Normalize only the trailing separator for exact root comparisons.
            Path.GetFullPath(root)); // Resolve the root to a full path before that descendant check.
        if (PathsEqual(normalizedCandidate, normalizedRoot)) // Reject the root itself; ownership requires a strict descendant.
            return false; // Do not treat the boundary directory itself as an owned child path.
        var prefix = normalizedRoot + Path.DirectorySeparatorChar; // Include the separator so a sibling sharing the root's prefix cannot pass.
        return normalizedCandidate.StartsWith( // Check the exact prefix used to confine a path or identify an owned staging name.
            prefix, // Test against that separator-terminated root boundary.
            OperatingSystem.IsWindows() // Select the existing platform path comparison policy.
                ? StringComparison.OrdinalIgnoreCase // Compare confined Windows paths using the existing case-insensitive platform policy.
                : StringComparison.Ordinal); // Compare confined non-Windows paths using the existing case-sensitive platform policy.
    } // Use a separator-terminated root prefix and reject the root itself.

    private static bool HasSafeUnixDirectoryMode(string path) // Reuse owner-only directory custody on platforms that enforce Unix modes.
        => SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(path); // Require owner-only directory custody for strict corpus readers.

    private static bool AnyExistingPathSegmentIsReparsePoint(string fullPath) // Reject reparse traversal through the existing filesystem safety owner.
        => SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(fullPath); // Reject reparse traversal through the existing filesystem safety owner.

    private static void DeleteOwnStaging( // Delete only the staging file owned by this publication attempt.
        RepositoryOwnedReferenceCorpusAddress address, // Use this corpus root as the strict immediate-parent cleanup boundary.
        string temporaryPath) // Consider only the uniquely named staging file owned by this attempt.
    { // Delete only this attempt's recognized non-reparse staging file in the corpus root.
        try // Bound nonfatal failures around delete own staging.
        { // Delete only this attempt's recognized non-reparse staging file in the corpus root while retaining the existing exception boundary.
            var fullPath = Path.GetFullPath(temporaryPath); // Resolve the complete filesystem path before confinement or handle identity checks.
            var fileName = Path.GetFileName(fullPath); // Extract only the final file name for the bounded staging ownership check.
            if (PathsEqual(Path.GetDirectoryName(fullPath)!, address.CorpusRoot) // Require the staging file's immediate parent to equal this corpus root.
                && (fileName.StartsWith(PayloadTempPrefix, StringComparison.Ordinal) // Recognize only this owner's payload staging-name prefix.
                    || fileName.StartsWith( // Alternatively recognize only this owner's manifest staging-name prefix.
                        ManifestTempPrefix, // Recognize only the existing manifest staging prefix owned by this publication attempt.
                        StringComparison.Ordinal)) // Keep staging-marker matching exact-case on every platform.
                && fileName.EndsWith(TempSuffix, StringComparison.Ordinal) // Check the explicit suffix required by the existing staging or wire contract.
                && !SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint( // Reject reparse traversal inside the resolved corpus boundary.
                    fullPath) // Reject reparse traversal on the resolved owned staging path.
                && File.Exists(fullPath)) // Delete only if the already-confined locator for this attempt's own temporary file still exists.
            { // Delete only the verified immediate-child staging file owned by this attempt.
                File.Delete(fullPath); // Remove only the staging locator whose ownership was checked by this attempt.
            } // Delete only the verified immediate-child staging file owned by this attempt.
        } // Delete only this attempt's recognized non-reparse staging file in the corpus root while retaining the existing exception boundary.
        catch (Exception exception) // Suppress only permitted nonfatal errors from this bounded staging cleanup.
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Keep fatal cleanup errors outside the existing filtered catch.
        { // Leave failed staging cleanup confined to this corpus version; never delete other artifacts.
            // Очистка staging-файла не расширяется за пределы версии корпуса;
            // оставшийся temp не является запечатанным corpus или manifest.
        } // Leave failed staging cleanup confined to this corpus version; never delete other artifacts.
    } // Delete only this attempt's recognized non-reparse staging file in the corpus root.

    private static RepositoryOwnedReferenceCorpusManifest EmptyManifest() // Initialize the strict parser failure carrier without accepting it as a manifest.
        => new( // Build only a non-admitted empty manifest parser carrier.
            string.Empty, // Leave strict owner schema empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave reference-corpus owner context empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave typed corpus domain empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave typed corpus subject empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave frozen corpus version empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave human corpus name empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave canonical corpus freeze time empty only in this non-admitted parser failure carrier.
            0, // Leave the complete entry count zero in the manifest failure carrier.
            string.Empty, // Leave canonical whole-corpus digest empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave typed relative corpus directory empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave typed relative payload locator empty only in this non-admitted parser failure carrier.
            0, // Leave raw payload length zero in the manifest failure carrier.
            string.Empty, // Leave raw payload digest empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave typed relative manifest locator empty only in this non-admitted parser failure carrier.
            string.Empty); // Leave canonical sealed manifest self-hash empty only in this non-admitted parser failure carrier.

    private static RepositoryOwnedReferenceCorpusPayload EmptyPayload() // Initialize the strict parser failure carrier without accepting it as a corpus.
        => new( // Build only a non-admitted empty payload parser carrier.
            string.Empty, // Leave strict owner schema empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave reference-corpus owner context empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave human corpus name empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave frozen corpus version empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave canonical corpus freeze time empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave typed corpus domain empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave typed corpus subject empty only in this non-admitted parser failure carrier.
            0, // Leave the declared entry count zero in the payload failure carrier.
            string.Empty, // Leave canonical whole-corpus digest empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave complete canonical corpus representation empty only in this non-admitted parser failure carrier.
            Array.Empty<RepositoryOwnedReferenceCorpusPayloadEntry>()); // Initialize a non-admitted parser failure carrier with an empty collection.

    private static RepositoryOwnedReferenceCorpusPayloadEntry EmptyEntry() // Initialize the strict parser failure carrier without accepting it as an entry.
        => new( // Build only a non-admitted empty entry parser carrier.
            string.Empty, // Leave canonical frozen entry identity empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave frozen owner input empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave paid-reference answer protected by provenance empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave typed corpus domain empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave typed corpus subject empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave explicit frozen-reference partition empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave independent source-entity fingerprint empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave the complete entry canonical representation empty in this failure carrier.
            EmptyProvenance()); // Initialize the strict parser failure carrier without accepting it as provenance.

    private static RepositoryOwnedReferenceCorpusPayloadProvenance EmptyProvenance() // Initialize the strict parser failure carrier without accepting it as provenance.
        => new( // Build only a non-admitted empty paid-provenance parser carrier.
            string.Empty, // Leave paid-reference provider key empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave paid-reference model key empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave paid-reference model version empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave paid-reference request identity empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave canonical paid-reference capture time empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave normalized reference-answer digest empty only in this non-admitted parser failure carrier.
            string.Empty, // Leave complete provenance digest empty only in this non-admitted parser failure carrier.
            string.Empty); // Leave the complete provenance canonical representation empty in this failure carrier.

    private static OperationResult<T> Invalid<T>() // Reject invalid descriptive input with the existing ordinary failure contract.
        => OperationResult<T>.Fail( // Return an ordinary failed result without a usable admitted value.
            "Repository-owned frozen reference corpus manifest is invalid.", // Report invalid sealed-corpus identity without revealing closed content.
            InvalidErrorCode); // Return the stable invalid-manifest code with this failed owner result.

    private static OperationResult<T> Conflict<T>() // Report conflicting sealed bytes without overwriting owner artifacts.
        => OperationResult<T>.Fail( // Return an ordinary failed result without a usable admitted value.
            "Repository-owned frozen reference corpus already seals different content.", // Report a conflicting sealed corpus without overwriting its bytes.
            ConflictErrorCode); // Return the stable conflict code without replacing an existing seal.

    private static OperationResult<T> Busy<T>() // Report bounded process-gate contention.
        => OperationResult<T>.Fail( // Return an ordinary failed result without a usable admitted value.
            "Repository-owned frozen reference corpus storage is busy.", // Report that the bounded process-gate wait could not acquire ownership.
            BusyErrorCode); // Return the stable busy code after the bounded gate wait fails.

    private static OperationResult<T> Unavailable<T>() // Report failed custody or nonfatal filesystem access without admission.
        => OperationResult<T>.Fail( // Return an ordinary failed result without a usable admitted value.
            "Repository-owned frozen reference corpus storage is unavailable.", // Report unavailable strict filesystem custody without accepting corpus data.
            UnavailableErrorCode); // Return the stable unavailable code after storage or custody failure.

    private sealed record StrictManifestReadback( // Carry stable raw manifest identity alongside its strictly validated parsed seal.
        RepositoryOwnedReferenceCorpusManifest Manifest, // Retain the complete manifest parsed from these same stable raw bytes.
        string ManifestFileSha256, // Pin raw manifest-file digest in this complete immutable value.
        long ManifestFileByteLength); // Pin raw manifest-file length in this complete immutable value.

    private sealed record RepositoryOwnedReferenceCorpusPayload( // Represent the complete persisted corpus header and full entry sequence.
        string Schema, // Pin schema discriminator in this complete immutable value.
        string Context, // Pin paid-reference custody context in this complete immutable value.
        string CorpusName, // Pin human corpus name in this complete immutable value.
        string CorpusVersion, // Pin frozen corpus version in this complete immutable value.
        string FrozenAtUtc, // Pin corpus freeze timestamp in this complete immutable value.
        string Domain, // Pin corpus domain in this complete immutable value.
        string Subject, // Pin corpus subject in this complete immutable value.
        int EntryCount, // Pin complete payload entry count in this complete immutable value.
        string CorpusSha256, // Pin whole-corpus canonical identity in this complete immutable value.
        string CanonicalRepresentation, // Pin full canonical representation in this complete immutable value.
        IReadOnlyList<RepositoryOwnedReferenceCorpusPayloadEntry> Entries); // Pin entire frozen-entry sequence in this complete immutable value.

    private sealed record RepositoryOwnedReferenceCorpusPayloadEntry( // Represent one complete persisted frozen entry including its closed paid provenance.
        string EntryId, // Pin canonical frozen-entry identity in this complete immutable value.
        string OwnerInput, // Pin owner input for this closed case in this complete immutable value.
        string PaidModelReferenceAnswer, // Pin closed paid-reference answer in this complete immutable value.
        string Domain, // Pin corpus domain in this complete immutable value.
        string Subject, // Pin corpus subject in this complete immutable value.
        string Partition, // Pin train/held-out partition in this complete immutable value.
        string SourceEntityFingerprint, // Pin independent source fingerprint in this complete immutable value.
        string CanonicalRepresentation, // Pin full canonical representation in this complete immutable value.
        RepositoryOwnedReferenceCorpusPayloadProvenance? PaidReferenceProvenance, // Null only for the separately tagged native-capture schema.
        NativeCapturedReferenceProvenance? NativeCaptureProvenance = null); // Reconstructed immutable native origin retains all nullable capture facts. // Pin complete closed-answer provenance in this complete immutable value.

    private sealed record RepositoryOwnedReferenceCorpusPayloadProvenance( // Represent all eight persisted paid-reference provenance fields.
        string ProviderRef, // Pin paid-reference provider identity in this complete immutable value.
        string ModelId, // Pin paid-reference model identity in this complete immutable value.
        string ModelVersion, // Pin paid-reference model version in this complete immutable value.
        string RequestId, // Pin paid-reference capture request identity in this complete immutable value.
        string CapturedAtUtc, // Pin paid-reference capture timestamp in this complete immutable value.
        string ReferenceAnswerSha256, // Pin normalized paid-reference answer digest in this complete immutable value.
        string ProvenanceSha256, // Pin complete provenance digest in this complete immutable value.
        string CanonicalRepresentation, string? ReferenceAnswerRawUtf8Sha256 = null); // Pin full canonical representation in this complete immutable value.

    private sealed class ProcessGateEntry // Keep one root-specific process semaphore and its locked live reference count.
    { // Track one semaphore and all attempts retaining its registry reference.
        public SemaphoreSlim Semaphore { get; } = new(1, 1); // Keep one process-local semaphore per corpus root to serialize sealing attempts.
        public int ReferenceCount { get; set; } // Track live registry references under the registry lock before allowing gate disposal.
    } // Finish process gate entry.

    private sealed class ProcessSealLease( // Own one acquired process gate whose release is atomically claimed once.
        string canonicalCorpusRoot, // Retain the canonical root key whose acquired gate this lease must release.
        ProcessGateEntry entry) : IDisposable // Receive the exact process-gate registry entry whose lease or reference is being released.
    { // Own the acquired root gate until this lease is disposed.
        private ProcessGateEntry? entryToRelease = entry; // Retain the one registry entry this lease must release exactly once.

        public void Dispose() // Release this acquired process lease exactly once.
        { // Claim and release this exact acquired process lease once.
            var owned = Interlocked.Exchange(ref entryToRelease, null); // Atomically claim the lease release so it occurs once.
            if (owned is not null) // Release the process-gate entry only if this Dispose call claimed the live lease.
                ReleaseOwnedProcessGate(canonicalCorpusRoot, owned); // Release the acquired process gate and its registry reference.
        } // Claim and release this exact acquired process lease once.
    } // Finish process seal lease.
} // Finish repository owned reference corpus manifest sealer.
