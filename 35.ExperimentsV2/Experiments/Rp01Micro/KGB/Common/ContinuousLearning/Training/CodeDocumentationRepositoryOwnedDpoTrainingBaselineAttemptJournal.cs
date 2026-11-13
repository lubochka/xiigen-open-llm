// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO;
using LoRA;
using TrainingLoop.Optimizers;
using TransformerModel.Config;

/// <summary>Current trusted inputs; the detached reference never selects an IO path.</summary>
internal sealed record CodeDocumentationPreparedBaselineAttemptInputs(
    CodeDocumentationPreparedBaselineReference Reference,
    CodeDocumentationOneLessonGrantCatalog Catalog,
    CodeDocumentationOneLessonSelectorV1 Selector,
    ModelConfig ModelConfig,
    RepositoryOwnedModelRuntimeCompatibility RuntimeCompatibility,
    CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext QualityGateContext,
    LoraConfig? OriginalLoraConfig,
    LoraPairRandomSeedAuthority? OriginalAdapterSeeds,
    RepositoryOwnedDpoDatasetAddress LessonDatasetAddress,
    RepositoryOwnedDpoDatasetRequestIdentity LessonExpectedRequestIdentity);

/// <summary>
/// Неизменяемая исходная запись одной попытки CodeDocumentation DPO до урока.
/// Она связывает законно созданный каталог попытки, принятую модель, одну
/// запечатанную пару и настоящий исходный экзамен. Это не полный снимок
/// восстановления: оптимизатор, генераторы случайности и сами веса не
/// сериализуются в JSON.
/// </summary>
public sealed record CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState(
    int FormatVersion,
    string TrainingRunId,
    string TrainingRunIdSha256,
    string AttemptRootRelativePath,
    string AttemptClaimRelativePath,
    string DomainSlug,
    string DesignPatternSlug,
    string ModelIdSlug,
    string ModelVersion,
    CodeDocumentationSavedModelAssessmentModelPassport AcceptedModelPassport,
    string DpoManifestRelativePath,
    string DpoManifestSha256,
    string TrainingPairId,
    string TrainingPairRelativePath,
    string TrainingPairSha256,
    CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality BaselineQuality);

public sealed record CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback(
    string BaselineStatePath,
    string BaselineStateRelativePath,
    string BaselineStateSha256,
    long BaselineStateByteLength,
    CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState State);

/// <summary>
/// Пишет baseline.json только внутри уже созданного executor'ом каталога
/// попытки и сразу открывает этот файл обратно. Любая ошибка записи, подмена
/// идентичности или расхождение с исходной оценкой останавливает маршрут до
/// запуска тренера.
/// </summary>
public sealed class CodeDocumentationPreExecutorBaselineEligibility // Only the journal owner can attest clean pre-executor state.
{ // This prospective proof does not claim historical optimizer counters.
    internal CodeDocumentationPreExecutorBaselineEligibility(RepositoryOwnedModelTrainingAttemptAddress address) { Address = address; } // Preserve the authenticated canonical address.
    internal RepositoryOwnedModelTrainingAttemptAddress Address { get; } // No client-selected artifact path enters recovery.
} // The executor must still repeat all its ordinary reservation checks.

public static class CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
{
    public const int CurrentFormatVersion = 1;
    public const string BaselineStateFileName = "baseline.json";
    public const string InvalidErrorCode =
        "CODE_DOCUMENTATION_DPO_BASELINE_ATTEMPT_STATE_INVALID";
    public const string UnavailableErrorCode =
        "CODE_DOCUMENTATION_DPO_BASELINE_ATTEMPT_STATE_UNAVAILABLE";

    public const string TrainingAttemptRecoveryRequiredErrorCode = // Share the existing coordinator refusal without introducing a second recovery protocol.
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_ATTEMPT_RECOVERY_REQUIRED";
    private const string PairReservationLockFileName = ".dpo-update.lock"; // One filesystem lock covers all run IDs of this canonical model/version.
    private const int MaximumSiblingAttempts = 4096; // Bound discovery work; exceeding it is unknown state, never a fresh run.

    /// <summary>Read-only reservation discovery; another run is never silently adopted or replayed.</summary>
    public static OperationResult<bool> RequireNoConflictingSiblingAttempt( // Keep reservation ownership in the existing baseline journal.
        RepositoryOwnedModelTrainingAttemptAddress address, IReadOnlyList<string> pairIds, // Use canonical caller addresses and actual pair content identities.
        string? sealedPairSha256 = null, CancellationToken cancellationToken = default) // A known sealed hash must agree with an attributed reservation.
    { // Discovery is repeated under the executor lock before any attempt claim.
        try // Any unreadable or unattributable sibling is unknown, not evidence of freshness.
        { // Inspect only the closed attempts directory of this model/version, without recursion.
            cancellationToken.ThrowIfCancellationRequested(); // Cancellation precedes all storage observation.
            if (!RepositoryOwnedTrainingArtifactLayout.IsValidModelTrainingAttemptAddress(address) || // Rebuild the complete canonical address first.
                pairIds.Count == 0 || pairIds.Any(id => !IsSha256(id)) || // Reject absent or noncanonical content identities.
                (sealedPairSha256 is not null && !IsSha256(sealedPairSha256))) // Reject a malformed supplied sealed-byte identity.
                return RecoveryRequired<bool>(); // An invalid guard cannot admit training.
            var attempts = Path.GetDirectoryName(address.AttemptRoot)!; // Derive discovery scope from the canonical address, never from saved JSON.
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(attempts) || File.Exists(attempts)) // Refuse redirected or mistyped discovery roots.
                return RecoveryRequired<bool>(); // Do not repair foreign storage during discovery.
            if (!Directory.Exists(attempts)) return OperationResult<bool>.Ok(true); // No attempts directory means no durable reservation exists yet.
            if (!SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(attempts)) return RecoveryRequired<bool>(); // Preserve the existing attempt directory boundary.
            var count = 0; // Bound the complete nonrecursive enumeration, including unknown entries.
            foreach (var entry in Directory.EnumerateFileSystemEntries(attempts)) // Every sibling must be attributable before a fresh update is admitted.
            { // The current run retains its existing coordinator recovery path.
                cancellationToken.ThrowIfCancellationRequested(); // Stop bounded discovery promptly when cancelled.
                if (++count > MaximumSiblingAttempts) return RecoveryRequired<bool>(); // Excess storage is unknown rather than silently truncated.
                if (StringComparerForPaths.Equals(entry, address.AttemptRoot)) continue; // Same-run recovery is not another reservation.
                if (Path.GetFileName(entry) == PairReservationLockFileName && File.Exists(entry) && // This known lock has no pair or outcome authority.
                    !SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(entry)) continue; // The lock owner separately authenticates its exclusive handle.
                if (!Directory.Exists(entry) || !IsSha256(Path.GetFileName(entry)) || // An unexpected entry cannot be treated as a fresh attempt.
                    SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(entry) || // Never follow sibling links or reparse points.
                    !SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(entry)) return RecoveryRequired<bool>(); // Refuse unattributable or unsafe attempt storage.
                var claimPath = Path.Combine(entry, RepositoryOwnedTrainingArtifactLayout.TrainingAttemptClaimFileName); // Derive the existing claim filename, not a path from JSON.
                if (!File.Exists(claimPath) || Directory.Exists(claimPath) || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(claimPath)) // Refuse unsafe claim paths before opening any handle.
                    return RecoveryRequired<bool>(); // A partial or redirected claim is unknown.
                using var claim = SecureLocalFileBoundary.OpenOwnerOnlyFile(claimPath, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.SequentialScan); // Read existing owner storage without changing it.
                if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(claim.SafeFileHandle, claimPath) || // Authenticate the opened claim handle.
                    !SecureLocalFileBoundary.HasOwnerOnlyFileMode(claimPath)) return RecoveryRequired<bool>(); // Require the existing file permission boundary.
                var claimBytes = SecureLocalFileBoundary.ReadBoundedStableBytes(claim, 4096); // Preserve the existing bounded stable claim reader.
                if (claimBytes is null) return RecoveryRequired<bool>(); // A partial claim is unknown.
                var lines = StrictTrialUtf8.GetString(claimBytes).Split(Environment.NewLine, StringSplitOptions.None); // Reject invalid UTF-8 and inspect the complete claim grammar.
                if (lines.Length != 4 || !lines[0].StartsWith("training_run_id=", StringComparison.Ordinal)) return RecoveryRequired<bool>(); // Do not infer a run from an arbitrary file.
                var sibling = RepositoryOwnedTrainingArtifactLayout.CreateModelTrainingAttemptAddress(address.RepositoryRoot, // Rebuild every sibling using the existing canonical layout owner.
                    address.DomainSlug, address.DesignPatternSlug, address.ModelIdSlug, address.ModelVersion, lines[0]["training_run_id=".Length..]); // The claim supplies only its bounded run identity.
                if (!sibling.IsSuccess || sibling.Value is null || !StringComparerForPaths.Equals(sibling.Value.AttemptRoot, entry) || // Bind the run digest to the discovered directory.
                    !ValidatePreparedClaim(sibling.Value).IsSuccess) return RecoveryRequired<bool>(); // Verify the complete canonical claim, stable handle and reparse/owner guards again.
                var reservation = ReadSiblingReservation(sibling.Value, cancellationToken); // Reuse the durable baseline format, never mint another pair journal.
                if (!reservation.IsSuccess || reservation.Value is null) return RecoveryRequired<bool>(); // Missing, corrupt or unsupported baseline state is unknown.
                if (pairIds.Contains(reservation.Value.TrainingPairId, StringComparer.Ordinal)) // A matching ID with equal sealed SHA is reserved; a different SHA is an identity conflict.
                    return sealedPairSha256 is not null && sealedPairSha256 != reservation.Value.TrainingPairSha256 // Compare the supplied current sealed SHA to the actually attributed sibling row, when present.
                        ? RecoveryRequired<bool>("The same reserved CodeDocumentation pair ID has a conflicting sealed byte hash; reconcile the existing attempt before another update.") // A hash conflict never becomes a fresh lesson.
                        : RecoveryRequired<bool>(); // The same reservation also refuses an equal hash or an ID-only direct caller.
                if (!ValidatePreparedClaim(sibling.Value).IsSuccess || // Refuse a claim that changed while the reservation was examined.
                    !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(claim.SafeFileHandle, claimPath)) return RecoveryRequired<bool>(); // Preserve opened-path identity through readback.
            } // A valid different pair does not consume this pair, but any unknown sibling fails closed.
            return OperationResult<bool>.Ok(true); // This is only a read-only precheck; the executor repeats it under its lock.
        } // Preserve cancellation and convert actual storage/parser failures to the existing recovery refusal.
        catch (OperationCanceledException) { throw; } // Cancellation never grants a new update.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return RecoveryRequired<bool>(); } // No missing or corrupt attempt is a cold-start fallback.
    } // End bounded sibling discovery.

    public static OperationResult<CodeDocumentationPreExecutorBaselineEligibility> AdmitPartialBaselineRecovery(RepositoryOwnedModelTrainingAttemptAddress address, string pairId, string sealedPairSha256, CancellationToken token = default) // Authenticate only the reached pre-executor branch under the existing pair lock.
    { // Release this short lock before calibration and the executor's normal lock acquisition.
        var acquired = AcquirePairReservationLock(address); // Retain every existing custody and concurrency guard.
        if (!acquired.IsSuccess || acquired.Value is null) return acquired.MapError<CodeDocumentationPreExecutorBaselineEligibility>(); // Contention never implies clean absence.
        using var lease = acquired.Value; // Disposal precedes the downstream executor invocation.
        try // Read the entire bounded namespace rather than accepting File.Exists false on access errors.
        { // A canonical attempt of any shape belongs to post-executor recovery.
            token.ThrowIfCancellationRequested(); // Preserve caller lifetime.
            var entries = Directory.GetFileSystemEntries(Path.GetDirectoryName(address.AttemptRoot)!); // A listing error is UNKNOWN, not absence.
            if (entries.Length > MaximumSiblingAttempts || entries.Any(p => StringComparerForPaths.Equals(p, address.AttemptRoot))) return RecoveryRequired<CodeDocumentationPreExecutorBaselineEligibility>(); // Never reopen claim, journal, Trial or held state through this branch.
            var siblings = RequireNoConflictingSiblingAttempt(address, [pairId], sealedPairSha256, token); // Preserve pair identity and malformed sibling refusals.
            if (!siblings.IsSuccess) return siblings.MapError<CodeDocumentationPreExecutorBaselineEligibility>(); // Unknown or reserved pairs cannot proceed.
            return OperationResult<CodeDocumentationPreExecutorBaselineEligibility>.Ok(new(address)); // Only the owner creates the typed pre-executor proof.
        } // The proof is structural eligibility under no external deletion, not a runtime counter.
        catch (OperationCanceledException) { throw; } // Cancellation grants no capability.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return RecoveryRequired<CodeDocumentationPreExecutorBaselineEligibility>(); } // Refuse unreadable or changing namespaces.
    } // The executor remains the sole optimizer owner.

    internal static OperationResult<FileStream> AcquirePairReservationLock(RepositoryOwnedModelTrainingAttemptAddress address) // The executor alone holds this effect boundary after legal admission.
    { // Returning the existing filesystem handle keeps release automatic on every executor exit.
        FileStream? stream = null; // Retain ownership until all custody checks have passed.
        try // Lock contention and unsafe storage both refuse a new mutation.
        { // Build the lock only inside the same canonical model/version attempts directory.
            if (!RepositoryOwnedTrainingArtifactLayout.IsValidModelTrainingAttemptAddress(address) || !SecureLocalFileBoundary.IsSupportedPlatform()) // Require the existing layout/platform boundary.
                return RecoveryRequired<FileStream>(); // Unsupported admission is fail closed.
            var attempts = Path.GetDirectoryName(address.AttemptRoot)!; // Scope all run IDs to their canonical model/version.
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(attempts) || File.Exists(attempts)) return RecoveryRequired<FileStream>(); // Do not follow or replace unsafe roots.
            if (!Directory.Exists(attempts)) SecureLocalFileBoundary.CreateOwnerOnlyDirectory(attempts); // Bootstrap only absent attempt storage using its existing owner boundary.
            if (!SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(attempts)) return RecoveryRequired<FileStream>(); // Never normalize a foreign existing directory.
            var path = Path.Combine(attempts, PairReservationLockFileName); // The lock path is deterministic across run IDs.
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) || Directory.Exists(path)) return RecoveryRequired<FileStream>(); // A foreign lock path is not repaired.
            try { stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None, FileOptions.WriteThrough); } // Prefer authenticated existing lock custody.
            catch (IOException exception) when (exception is FileNotFoundException || (exception.HResult & 0xffff) == 2) // Only an actual missing lock permits creation, including the existing Windows custody error shape.
            { // Preserve another concurrent creator's file rather than overwriting it.
                try { stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, FileOptions.WriteThrough); } // Exclusively create absent lock storage.
                catch (IOException) when (File.Exists(path)) { stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None, FileOptions.WriteThrough); } // Reopening still refuses a live or foreign owner.
            } // Existing storage is never truncated or deleted.
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path) || // Bind the lock handle to the canonical path.
                !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path) || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path)) // Use strict custody on Windows too.
            { stream.Dispose(); return RecoveryRequired<FileStream>(); } // Release a refused handle before returning.
            if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux()) stream.Lock(0, 1); // Add the filesystem range lock only on the supported platforms, visibly to the analyzer too.
            else { stream.Dispose(); return RecoveryRequired<FileStream>(); } // Unsupported range locking cannot admit mutation.
            return OperationResult<FileStream>.Ok(stream); // Disposal releases both locks without deleting reservation history.
        } // Refuse lock or custody failures before the attempt claim.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Preserve the existing nonfatal storage error discipline.
        { stream?.Dispose(); return RecoveryRequired<FileStream>("The model/version reservation lock is unavailable: " + exception.Message); } // Release every partial handle and retain the actual storage refusal for diagnosis.
    } // No global lease request ID or trainer state is changed.

    private static OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState> ReadSiblingReservation( // Only identify an existing reservation; this is not another attempt's recovery authority.
        RepositoryOwnedModelTrainingAttemptAddress address, CancellationToken token) // All IO paths remain derived by the layout/claim owner.
    { // Preserve both existing ordinary v1 and prepared v2 baseline encodings.
        var opened = ReadExisting(Path.Combine(address.AttemptRoot, BaselineStateFileName)); // Use the existing bounded stable owner reader.
        if (!opened.IsSuccess || opened.Value is null) return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState>(); // A claim without a baseline is an unknown reservation.
        using var document = JsonDocument.Parse(opened.Value.Bytes); // Inspect the complete baseline, not a filename or a marker string.
        var root = document.RootElement; // Saved path fields never select files for discovery.
        if (root.GetProperty("FormatVersion").GetInt32() == CurrentFormatVersion) // Ordinary attempts already have a full typed readback owner.
        { // Require its existing complete state validation rather than accepting a header-only v1 record.
            var ordinary = OpenExisting(address, token); // Validate the canonical run/model/lesson/baseline state.
            return ordinary.IsSuccess && ordinary.Value is not null && ordinary.Value.BaselineStateSha256 == opened.Value.Sha256 && // Require the full readback to agree with the first stable observation.
                CryptographicOperations.FixedTimeEquals(opened.Value.Bytes, JsonSerializer.SerializeToUtf8Bytes(ordinary.Value.State, JsonOptions)) // The actual v1 writer's canonical encoding rejects duplicate or alias identity fields.
                ? AttributeSiblingReservation(address, ordinary.Value.State, token) // Bind the complete canonical header to actual immutable manifest-row/raw-file evidence.
                : RecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState>(); // Failed readback cannot admit another update.
        } // Prepared v2 stores an original reference instead of duplicating the baseline quality graph.
        if (root.GetProperty("FormatVersion").GetInt32() != 2) return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState>(); // Unknown schema requires reconciliation.
        string Text(JsonElement element, string name) => element.GetProperty(name).GetString() ?? throw new JsonException("Null reservation identity."); // Invalid wire data is caught by the discovery boundary.
        var state = new CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState(2, address.TrainingRunId, address.TrainingRunIdSha256, // Rebuild canonical identity so altered headers fail the exact-wire comparison below.
            address.AttemptRootRelativePath, address.AttemptClaimRelativePath, address.DomainSlug, address.DesignPatternSlug, address.ModelIdSlug, address.ModelVersion, // Never accept saved paths as authority.
            root.GetProperty("AcceptedModelPassport").Deserialize<CodeDocumentationSavedModelAssessmentModelPassport>(JsonOptions)!, // Reject unknown passport fields through the existing strict typed schema.
            Text(root, "DpoManifestRelativePath"), Text(root, "DpoManifestSha256"), Text(root, "TrainingPairId"), Text(root, "TrainingPairRelativePath"), Text(root, "TrainingPairSha256"), null!); // Discovery does not hydrate or adopt another original graph.
        var savedReference = root.GetProperty("PreparedBaselineReference"); // Read only detached original expectations from the existing v2 wire.
        var reference = new CodeDocumentationPreparedBaselineReference(savedReference.GetProperty("FormatVersion").GetInt32(), // Reconstruct the existing reference type without granting IO authority.
            Text(savedReference, "RepositoryRoot"), Text(savedReference, "ObjectiveSha256"), Text(savedReference, "BaselineRoot"), Text(savedReference, "BaselineRootRelativePath"), // Preserve every reference coordinate for canonical comparison.
            Text(savedReference, "BaselinePath"), Text(savedReference, "ClaimPath"), Text(savedReference, "LockPath"), Text(savedReference, "RequestSha256"), Text(savedReference, "Sha256"), // No supplied coordinate is opened.
            savedReference.GetProperty("ByteLength").GetInt64(), Text(savedReference, "Classification")); // Preserve the exact original size and classification.
        var originalAddress = RepositoryOwnedTrainingArtifactLayout.CreatePreparedBaselineAddress(address.RepositoryRoot, // Independently rebuild the reference coordinate through its existing layout owner.
            address.DomainSlug, address.DesignPatternSlug, address.ModelIdSlug, address.ModelVersion, reference.ObjectiveSha256); // Keep discovery within the same canonical model/version.
        if (!originalAddress.IsSuccess || originalAddress.Value is null || !AcceptedPassportMatchesState(state) || !TrainingPairIdentityValid(state) || // Require a complete attributable model and sealed pair header.
            reference.FormatVersion != CodeDocumentationRepositoryOwnedPreparedBaselineJournal.FormatVersion || !IsSha256(reference.RequestSha256) || !IsSha256(reference.Sha256) || // Preserve existing reference schema and digest validity.
            reference.ByteLength is < 1 or > CodeDocumentationRepositoryOwnedPreparedBaselineJournal.MaximumBaselineBytes || reference.Classification != "NEEDS_LESSON" || // Only an original admitted for a lesson can reserve an update.
            !StringComparerForPaths.Equals(reference.RepositoryRoot, originalAddress.Value.RepositoryRoot) || !StringComparerForPaths.Equals(reference.BaselineRoot, originalAddress.Value.BaselineRoot) || // Bind all saved reference paths without opening them.
            reference.BaselineRootRelativePath != originalAddress.Value.BaselineRootRelativePath || !StringComparerForPaths.Equals(reference.BaselinePath, originalAddress.Value.BaselinePath) || // Reject detached coordinates from another scope.
            !StringComparerForPaths.Equals(reference.ClaimPath, originalAddress.Value.ClaimPath) || !StringComparerForPaths.Equals(reference.LockPath, originalAddress.Value.LockPath) || // No foreign reference can make a partial reservation look attributable.
            !CryptographicOperations.FixedTimeEquals(opened.Value.Bytes, EncodePrepared(state, reference))) // Canonical complete bytes reject duplicates, extra/missing fields and changed run/model headers.
            return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState>(); // Corrupt prepared state is unknown.
        return AttributeSiblingReservation(address, state, token); // Prepared headers require the same actual raw reservation attribution as ordinary headers.
    } // The existing coordinator remains the only owner of same-run resume.

    private static OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState> AttributeSiblingReservation( // Saved JSON provides expectations only; the existing sealer proves raw row/file attribution.
        RepositoryOwnedModelTrainingAttemptAddress attempt, CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState state, CancellationToken token) // Keep the discovered canonical model/version as the authority for scope.
    { // A detached baseline never chooses an arbitrary manifest or pair IO path.
        token.ThrowIfCancellationRequested(); // Cancellation precedes the bounded immutable observation.
        if (!IsSafeRepositoryRelativePath(state.DpoManifestRelativePath)) return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState>(); // Reject unsafe saved coordinates before deriving an address.
        var parts = state.DpoManifestRelativePath.Split('/'); // Read only the expected lexical domain/pattern/version coordinates.
        if (parts.Length < 4 || parts[^4] != attempt.DomainSlug || parts[^3] != attempt.DesignPatternSlug) // The saved dataset must belong to the canonical model's existing domain/pattern.
            return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState>(); // A foreign dataset cannot make the sibling attributable.
        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(attempt.RepositoryRoot, attempt.DomainSlug, attempt.DesignPatternSlug, parts[^2]); // The existing layout validates the version and derives every IO path.
        if (!address.IsSuccess || address.Value is null || address.Value.ManifestRelativePath != state.DpoManifestRelativePath) // Require the complete regenerated relative path, including root and manifest filename.
            return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState>(); // Do not open any saved arbitrary path.
        var opened = RepositoryOwnedDpoDatasetManifestSealer.OpenReservationManifestReadOnly(address.Value, token); // Read only immutable manifest/row/raw hashes without pair-store mutation or custody normalization.
        if (!opened.IsSuccess || opened.Value is null || opened.Value.Manifest.ManifestSha256 != state.DpoManifestSha256 || // Bind the actual canonical manifest semantic identity to this reservation.
            opened.Value.Manifest.PairCount != 1 || opened.Value.Manifest.OrderedPairs.Count != 1) // The existing CodeDocumentation lesson reserves exactly one sealed row.
            return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState>(); // Missing, corrupt, unsupported or changed evidence is unknown.
        var row = opened.Value.Manifest.OrderedPairs[0]; // The sealer already checked canonical paths and the actual raw bytes against this row.
        if (row.PairId != state.TrainingPairId || row.RepositoryRelativePairFilePath != state.TrainingPairRelativePath || row.PairFileSha256 != state.TrainingPairSha256) // Compare all saved expectations to the owner-attributed immutable row.
            return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState>(); // A canonically encoded but mismatched header cannot count as a distinct valid sibling.
        return OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState>.Ok(state); // This raw reservation identity proof never re-admits a DpoPair for training.
    } // No new pair journal or recovery route is introduced.

    private static OperationResult<T> RecoveryRequired<T>(string? detail = null) => OperationResult<T>.Fail( // Keep the ordinary consumer's existing recovery-required result shape.
        detail ?? "Another or unattributable CodeDocumentation DPO attempt requires reconciliation before a new update under this model/version.", TrainingAttemptRecoveryRequiredErrorCode); // Never replay an unknown attempt under a different run ID.

    private const long MaximumBaselineStateBytes = 2L * 1024L * 1024L;
    private const int HashHexLength = 64;
    private const int RichPairRecordSchemaVersion = 5;
    private const int MaximumIdentityLength = 256;
    private const int MaximumRepositoryRelativePathLength = 1024;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    internal sealed record PreparedSnapshot(
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState State,
        byte[] Bytes, string InputsSha256, string ManifestFileSha256);

    internal const int MaximumTrialStartedBytes = 64 * 1024;
    internal const int MaximumTrialCompletedBytes = 32 * 1024 * 1024;
    internal const string TrialStartedFileName = "trial-started.json";
    internal const string TrialCompletedFileName = "trial-completed.json";
    private const string StartedSchema = "xiigen.code-documentation.prepared-trial.started.v1";
    private const string CompletedSchema = "xiigen.code-documentation.prepared-trial.completed.v1";
    private static readonly JsonSerializerOptions TrialJson = new()
    {
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        MaxDepth = 32,
    };
    private static readonly UTF8Encoding StrictTrialUtf8 = new(false, true);

    internal sealed record PreparedTrialStarted(
        string Schema, int FormatVersion, string TrainingRunId, string TrainingRunIdSha256,
        string AttemptRootRelativePath, string BaselineStateSha256, long BaselineStateByteLength,
        string PreparedInputsSha256, string HistoricalInvocationSha256,
        string ContinuationManifestRelativePath, string ContinuationManifestSha256,
        long ContinuationManifestByteLength, CodeDocumentationSavedModelAssessmentModelPassport TrialPassport,
        string TrialAssessmentRunId, int ExpectedTrialEvaluationCallCount, long CompleteWireUpperBoundBytes);

    private sealed record PreparedTrialCompleted(string Schema, int FormatVersion,
        string StartedSha256, string Disposition, string TrialSha256, string DerivedEvidenceSha256,
        CodeDocumentationSavedModelAssessmentCalibrationResult Trial);

    internal sealed record PreparedTrialReadback(
        CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult Detailed,
        string StartedSha256, string CompletedSha256);

    /// <summary>Only the durable CreateNew winner can complete this series. Reopening never mints one.</summary>
    internal sealed class PreparedTrialDispatch
    {
        private PreparedTrialDispatch(PreparedTrialStarted started, byte[] bytes)
        { Started = started; Bytes = bytes; }
        private PreparedTrialStarted Started { get; }
        private byte[] Bytes { get; }
        private int _dispatchEntered;
        private int _completionEntered;

        internal static async Task<OperationResult<PreparedTrialDispatch>> ClaimAsync(
            RepositoryOwnedModelTrainingAttemptAddress address, CodeDocumentationPreparedBaselineAttemptInputs inputs,
            CodeDocumentationSavedModelAssessmentRequest request, string invocationSha, CancellationToken token)
        {
            try
            {
                var current = await CurrentTrialBindingAsync(address, inputs, request, invocationSha, token).ConfigureAwait(false);
                if (!current.IsSuccess || current.Value is null) return current.MapError<PreparedTrialDispatch>();
                var bytes = TrialBytes(current.Value.Started);
                if (bytes.Length is < 1 or > MaximumTrialStartedBytes) return Invalid<PreparedTrialDispatch>();
                WriteTrialCreateNew(address, TrialStartedFileName, bytes, MaximumTrialStartedBytes, token);
                var opened = ReadTrial(address, TrialStartedFileName, MaximumTrialStartedBytes);
                if (!bytes.AsSpan().SequenceEqual(opened)) return Invalid<PreparedTrialDispatch>();
                DecodeTrial<PreparedTrialStarted>(opened);
                var after = await CurrentTrialBindingAsync(address, inputs, request, invocationSha, token).ConfigureAwait(false);
                if (!after.IsSuccess || after.Value is null || !bytes.AsSpan().SequenceEqual(TrialBytes(after.Value.Started)) ||
                    !bytes.AsSpan().SequenceEqual(ReadTrial(address, TrialStartedFileName, MaximumTrialStartedBytes)))
                    return Invalid<PreparedTrialDispatch>();
                return OperationResult<PreparedTrialDispatch>.Ok(new(current.Value.Started, bytes));
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception e) when (SnapshotExceptionGraphClassifier.IsNonFatal(e)) { return Unavailable<PreparedTrialDispatch>(); }
        }

        internal async Task<OperationResult<bool>> ConsumeDispatchAsync(
            RepositoryOwnedModelTrainingAttemptAddress address, CodeDocumentationPreparedBaselineAttemptInputs inputs,
            CodeDocumentationSavedModelAssessmentRequest request, CancellationToken token)
        {
            if (Interlocked.CompareExchange(ref _dispatchEntered, 1, 0) != 0) return Invalid<bool>();
            try
            {
                var current = await CurrentTrialBindingAsync(address, inputs, request, Started.HistoricalInvocationSha256, token).ConfigureAwait(false);
                if (current.IsSuccess && current.Value is not null &&
                    Bytes.AsSpan().SequenceEqual(TrialBytes(current.Value.Started)) &&
                    Bytes.AsSpan().SequenceEqual(ReadTrial(address, TrialStartedFileName, MaximumTrialStartedBytes)))
                {
                    Volatile.Write(ref _dispatchEntered, 2);
                    return OperationResult<bool>.Ok(true);
                }
                return Invalid<bool>();
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception e) when (SnapshotExceptionGraphClassifier.IsNonFatal(e)) { return Unavailable<bool>(); }
        }

        internal async Task<OperationResult<PreparedTrialReadback>> CompleteAsync(
            RepositoryOwnedModelTrainingAttemptAddress address, CodeDocumentationPreparedBaselineAttemptInputs inputs,
            CodeDocumentationSavedModelAssessmentRequest request,
            CodeDocumentationSavedModelAssessmentCalibrationResult trial, string pairId, CancellationToken token)
        {
            if (Volatile.Read(ref _dispatchEntered) != 2 || Interlocked.Exchange(ref _completionEntered, 1) != 0)
                return Invalid<PreparedTrialReadback>();
            try
            {
                var snapshot = TrialBytes(trial);
                var current = await CurrentTrialBindingAsync(address, inputs, request, Started.HistoricalInvocationSha256, token).ConfigureAwait(false);
                if (!current.IsSuccess || current.Value is null ||
                    !Bytes.AsSpan().SequenceEqual(TrialBytes(current.Value.Started)) ||
                    !Bytes.AsSpan().SequenceEqual(ReadTrial(address, TrialStartedFileName, MaximumTrialStartedBytes)))
                    return Invalid<PreparedTrialReadback>();
                // Detach the graph into its single canonical value before any typed admission.
                var detached = DecodeTrial<CodeDocumentationSavedModelAssessmentCalibrationResult>(snapshot);
                var result = CompareTrial(current.Value, inputs, request, detached, pairId);
                if (!result.IsSuccess || result.Value is null) return result.MapError<PreparedTrialReadback>();
                var completed = EncodeCompleted(Bytes, inputs.Reference.Sha256, result.Value, snapshot);
                if (!snapshot.AsSpan().SequenceEqual(TrialBytes(trial)) ||
                    completed.Length > Started.CompleteWireUpperBoundBytes || completed.Length > MaximumTrialCompletedBytes)
                    return Invalid<PreparedTrialReadback>();
                var after = await CurrentTrialBindingAsync(address, inputs, request, Started.HistoricalInvocationSha256, token).ConfigureAwait(false);
                if (!after.IsSuccess || after.Value is null || !Bytes.AsSpan().SequenceEqual(TrialBytes(after.Value.Started)) ||
                    !Bytes.AsSpan().SequenceEqual(ReadTrial(address, TrialStartedFileName, MaximumTrialStartedBytes)) ||
                    !snapshot.AsSpan().SequenceEqual(TrialBytes(trial)))
                    return Invalid<PreparedTrialReadback>();
                WriteTrialCreateNew(address, TrialCompletedFileName, completed, MaximumTrialCompletedBytes, token);
                var opened = await OpenCompletedTrialAsync(address, inputs, request, pairId, token).ConfigureAwait(false);
                if (!opened.IsSuccess || opened.Value is null) return opened;
                return opened.Value.StartedSha256 == TrialHash(Bytes) && opened.Value.CompletedSha256 == TrialHash(completed)
                    ? opened : Invalid<PreparedTrialReadback>();
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception e) when (SnapshotExceptionGraphClassifier.IsNonFatal(e)) { return Unavailable<PreparedTrialReadback>(); }
        }
    }

    private sealed record CurrentTrialBinding(PreparedTrialStarted Started,
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality Original, string TrainingPairId);

    private static async Task<OperationResult<CurrentTrialBinding>> CurrentTrialBindingAsync(
        RepositoryOwnedModelTrainingAttemptAddress address, CodeDocumentationPreparedBaselineAttemptInputs inputs,
        CodeDocumentationSavedModelAssessmentRequest request, string historicalInvocationSha, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!ValidatePreparedClaim(address).IsSuccess || !IsTrialHash(historicalInvocationSha)) return Invalid<CurrentTrialBinding>();
        var inputHash = CapturePreparedInputs(inputs);
        var baseline = await OpenPreparedExistingAsync(address, inputs, token).ConfigureAwait(false);
        if (!baseline.IsSuccess || baseline.Value is null) return baseline.MapError<CurrentTrialBinding>();
        var marker = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(address, token);
        var p = request.ModelPassport;
        var bound = CompleteTrialUpperBoundBytes(inputs.QualityGateContext);
        if (!marker.IsSuccess || marker.Value is null || bound is null ||
            !CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.MatchesBaseline(marker.Value.Manifest, address, baseline.Value.State).IsSuccess ||
            marker.Value.Manifest.TrialWeightsRelativePath != p.RepositoryRelativeWeightsPath ||
            marker.Value.Manifest.TrialWeightsSha256 != p.WeightsSha256 || marker.Value.Manifest.TrialWeightsByteLength != p.WeightsByteLength ||
            p.ModelSourceKind != CodeDocumentationSavedModelAssessmentModelPassport.TrialTrainingAttemptCheckpointSourceKind ||
            inputHash != CapturePreparedInputs(inputs) || !ValidatePreparedClaim(address).IsSuccess)
            return Invalid<CurrentTrialBinding>();
        var manifest = marker.Value.Manifest;
        var optimizer = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(address.AttemptRoot,
            manifest.OptimizerStateStep, manifest.OptimizerStateSha256, token);
        if (!optimizer.IsSuccess || optimizer.Value is null ||
            optimizer.Value.OptimizerStateByteLength != manifest.OptimizerStateByteLength ||
            !CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.MatchesRuntime(manifest, address,
                inputs.ModelConfig, request.Model.GetCheckpointParameterEntries()
                    .Select(e => new AdamWParameterBinding(e.Key, e.Parameter)).ToArray(),
                DpoTrainer.ProductionOptimizerSettings, optimizer.Value.State).IsSuccess)
            return Invalid<CurrentTrialBinding>();
        if (manifest.RandomStateRequired && !RepositoryOwnedDpoRandomStateFile.OpenAndValidate(
                Path.Combine(address.AttemptRoot, RepositoryOwnedDpoRandomStateFile.BuildFileName(manifest.RandomStateStep)),
                manifest.RandomStateSha256, manifest.RandomStateByteLength, manifest.RandomStateStep,
                manifest.RandomStateTopologySha256, manifest.LoraConfigurationSha256).IsSuccess)
            return Invalid<CurrentTrialBinding>();
        if (!CurrentTrialWeightsMatch(address, p, token))
            return Invalid<CurrentTrialBinding>();
        var plan = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.PlanQualityGate(
            inputs.QualityGateContext.Corpus, inputs.QualityGateContext.GenerationSettings,
            inputs.QualityGateContext.EvaluatorConfiguration, inputs.QualityGateContext.NoisePolicyConfig);
        if (!plan.IsSuccess || plan.Value is null) return plan.MapError<CurrentTrialBinding>();
        return OperationResult<CurrentTrialBinding>.Ok(new(new(StartedSchema, 1, address.TrainingRunId,
            address.TrainingRunIdSha256, address.AttemptRootRelativePath, baseline.Value.BaselineStateSha256,
            baseline.Value.BaselineStateByteLength, inputHash, historicalInvocationSha, marker.Value.ManifestRelativePath,
            marker.Value.ManifestSha256, marker.Value.ManifestByteLength, p, request.AssessmentRunId,
            plan.Value.BaselinePlan.ExpectedEvaluationCallCount, bound.Value), baseline.Value.State.BaselineQuality, baseline.Value.State.TrainingPairId));
    }

    internal static async Task<OperationResult<PreparedTrialReadback>> OpenCompletedTrialAsync(
        RepositoryOwnedModelTrainingAttemptAddress address, CodeDocumentationPreparedBaselineAttemptInputs inputs,
        CodeDocumentationSavedModelAssessmentRequest request, string pairId, CancellationToken token = default)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            var startedBytes = ReadTrial(address, TrialStartedFileName, MaximumTrialStartedBytes);
            var started = DecodeTrial<PreparedTrialStarted>(startedBytes);
            var current = await CurrentTrialBindingAsync(address, inputs, request, started.HistoricalInvocationSha256, token).ConfigureAwait(false);
            if (!current.IsSuccess || current.Value is null || !startedBytes.AsSpan().SequenceEqual(TrialBytes(current.Value.Started)))
                return Invalid<PreparedTrialReadback>();
            var completedBytes = ReadTrial(address, TrialCompletedFileName, MaximumTrialCompletedBytes);
            var completed = DecodeTrial<PreparedTrialCompleted>(completedBytes);
            var trialBytes = TrialBytes(completed.Trial);
            var result = CompareTrial(current.Value, inputs, request, completed.Trial, pairId);
            if (!result.IsSuccess || result.Value is null) return result.MapError<PreparedTrialReadback>();
            if (completedBytes.Length > started.CompleteWireUpperBoundBytes ||
                !completedBytes.AsSpan().SequenceEqual(EncodeCompleted(startedBytes, inputs.Reference.Sha256, result.Value, trialBytes)))
                return Invalid<PreparedTrialReadback>();
            var after = await CurrentTrialBindingAsync(address, inputs, request, started.HistoricalInvocationSha256, token).ConfigureAwait(false);
            if (!after.IsSuccess || after.Value is null || !startedBytes.AsSpan().SequenceEqual(TrialBytes(after.Value.Started)) ||
                !startedBytes.AsSpan().SequenceEqual(ReadTrial(address, TrialStartedFileName, MaximumTrialStartedBytes)) ||
                !completedBytes.AsSpan().SequenceEqual(ReadTrial(address, TrialCompletedFileName, MaximumTrialCompletedBytes)))
                return Invalid<PreparedTrialReadback>();
            return OperationResult<PreparedTrialReadback>.Ok(new(result.Value, TrialHash(startedBytes), TrialHash(completedBytes)));
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e) when (SnapshotExceptionGraphClassifier.IsNonFatal(e)) { return Unavailable<PreparedTrialReadback>(); }
    }

    private static OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult> CompareTrial(
        CurrentTrialBinding binding, CodeDocumentationPreparedBaselineAttemptInputs inputs,
        CodeDocumentationSavedModelAssessmentRequest request, CodeDocumentationSavedModelAssessmentCalibrationResult trial, string pairId)
        => StringComparer.Ordinal.Equals(pairId, binding.TrainingPairId)
            ? CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.CompareCompletedTrialDetailed(
                request, inputs.QualityGateContext.NoisePolicyConfig, binding.Original, trial, binding.TrainingPairId)
            : Invalid<CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>();

    private static byte[] EncodeCompleted(byte[] startedBytes, string originalSha,
        CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult result, byte[] trialBytes)
    {
        var startedSha = TrialHash(startedBytes); var trialSha = TrialHash(trialBytes); var e = result.Evidence;
        var derived = TrialHash(TrialBytes(new
        {
            Schema = "xiigen.code-documentation.prepared-trial.evidence.v1", StartedSha256 = startedSha,
            OriginalExternalReferenceSha256 = originalSha, TrialSha256 = trialSha, Disposition = result.Disposition.ToString(),
            e.Accepted, e.TrainingPairId, e.Status, e.BaselineMeanCorrectFraction, e.TrialMeanCorrectFraction,
            e.Difference, e.NoiseFloor, e.ExpectedEvaluationCallCount, e.ObservedEvaluationCallCount, e.EvidenceRefs,
        }));
        return TrialBytes(new PreparedTrialCompleted(CompletedSchema, 1, startedSha, result.Disposition.ToString(), trialSha, derived, result.Trial));
    }

    internal static long? CompleteTrialUpperBoundBytes(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context)
    {
        try
        {
            var plan = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.PlanQualityGate(context.Corpus,
                context.GenerationSettings, context.EvaluatorConfiguration, context.NoisePolicyConfig);
            var ids = context.Corpus.Entries.Where(e => e.Partition == FrozenReferencePartition.HeldOut).Select(e => e.EntryId).ToArray();
            if (!plan.IsSuccess || plan.Value is null ||
                (long)ids.Length * plan.Value.BaselinePlan.RepeatCount != plan.Value.BaselinePlan.ExpectedEvaluationCallCount) return null;
            return CompleteTrialUpperBoundBytes(ids, plan.Value.BaselinePlan.RepeatCount);
        }
        catch (Exception e) when (SnapshotExceptionGraphClassifier.IsNonFatal(e)) { return null; }
    }

    internal static long? CompleteTrialUpperBoundBytes(IReadOnlyList<string> ids, int r)
    {
        try
        {
            if (ids.Count is < 1 or > 200 || ids.Any(id => string.IsNullOrWhiteSpace(id) || id.Length > 256)) return null;
            var c = ids.Count; var n = checked((long)c * r);
            if (r < 2 || n > 5000) return null;
            var hash = new string('f', 64);
            // This is a scalar size probe, never an assessment. Default safe JSON escaping
            // attains six bytes per UTF16 character; getters are included by the actual serializer.
            var envelope = TrialBytes(new CodeDocumentationSavedModelAssessmentCalibrationObservation(
                new string('\uffff', 256), int.MaxValue, int.MaxValue, double.MinValue, hash, hash, hash, [], [], [])).LongLength;
            if (envelope > 2304 || TrialBytes(double.MinValue).Length > 26) return null;
            var maximumCase = ids.Max(id => id.Length);
            var bound = checked(262144L + n * (6L * (256 + maximumCase) + 73) + r * (2304L + 26 + 2) +
                c * (6L * (1024 + maximumCase + 64) + 1024 + 6L * 256 + 3) + 512L * (6L * 512 + 32) + 262144L);
            return bound <= MaximumTrialCompletedBytes ? bound : null;
        }
        catch (Exception e) when (SnapshotExceptionGraphClassifier.IsNonFatal(e)) { return null; }
    }

    internal static byte[] TrialBytes<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value, TrialJson);
    private static string TrialHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static bool IsTrialHash(string text) => text is { Length: 64 } && text.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');
    internal static T DecodeTrial<T>(byte[] bytes)
    {
        StrictTrialUtf8.GetString(bytes);
        using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 32 });
        ValidateTrialJson(document.RootElement);
        var value = JsonSerializer.Deserialize<T>(bytes, TrialJson) ?? throw new JsonException("Null trial.");
        if (!bytes.AsSpan().SequenceEqual(TrialBytes(value))) throw new JsonException("Noncanonical trial.");
        return value;
    }

    private static void ValidateTrialJson(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Null) throw new JsonException("Null trial field.");
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            { if (!names.Add(property.Name)) throw new JsonException("Duplicate trial field."); ValidateTrialJson(property.Value); }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            if (element.GetArrayLength() > 10000) throw new JsonException("Trial collection exceeds bound.");
            foreach (var item in element.EnumerateArray()) ValidateTrialJson(item);
        }
        else if (element.ValueKind == JsonValueKind.String && element.GetString()!.Length > 16384)
            throw new JsonException("Trial string exceeds bound.");
    }

    private static byte[] ReadTrial(RepositoryOwnedModelTrainingAttemptAddress address, string name, int cap)
    {
        if (!ValidatePreparedClaim(address).IsSuccess) throw new IOException("Unsafe trial attempt.");
        var path = Path.Combine(address.AttemptRoot, name);
        if (!File.Exists(path) || Directory.Exists(path) || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) ||
            !SecureLocalFileBoundary.HasOwnerOnlyFileMode(path)) throw new IOException("Unavailable trial record.");
        using var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.SequentialScan);
        if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path)) throw new IOException("Trial handle changed.");
        var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, cap);
        if (bytes is null || bytes.Length == 0 || !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path) ||
            !ValidatePreparedClaim(address).IsSuccess || !SecureLocalFileBoundary.HasOwnerOnlyFileMode(path) ||
            SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path)) throw new IOException("Trial custody changed.");
        return bytes;
    }

    private static bool CurrentTrialWeightsMatch(RepositoryOwnedModelTrainingAttemptAddress address,
        CodeDocumentationSavedModelAssessmentModelPassport passport, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var path = Path.Combine(address.AttemptRoot, DpoTrainer.BuildPolicyCheckpointFileName(1));
        if (!ValidatePreparedClaim(address).IsSuccess || !File.Exists(path) || Directory.Exists(path) ||
            SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) || !SecureLocalFileBoundary.HasOwnerOnlyFileMode(path))
            return false;
        using var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.SequentialScan);
        var length = stream.Length;
        if (length < 1 || length != passport.WeightsByteLength || length > passport.MaximumModelSerializationBytes ||
            !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path)) return false;
        var first = ReadExactWeightHash(stream, length, token);
        if (first is null || stream.Position != length || stream.Length != length ||
            Convert.ToHexString(first).ToLowerInvariant() != passport.WeightsSha256) return false;
        stream.Position = 0;
        var second = ReadExactWeightHash(stream, length, token);
        token.ThrowIfCancellationRequested();
        return second is not null && CryptographicOperations.FixedTimeEquals(first, second) &&
            stream.Position == length && stream.Length == length &&
            SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path) &&
            ValidatePreparedClaim(address).IsSuccess && SecureLocalFileBoundary.HasOwnerOnlyFileMode(path) &&
            !SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path);
    }

    private static byte[]? ReadExactWeightHash(Stream stream, long length, CancellationToken token)
    {
        if (length < 1) return null;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[64 * 1024]; var remaining = length;
        while (remaining > 0)
        {
            token.ThrowIfCancellationRequested();
            var read = stream.Read(buffer, 0, (int)Math.Min(remaining, buffer.Length));
            if (read < 1) return null;
            hash.AppendData(buffer, 0, read); remaining -= read;
        }
        token.ThrowIfCancellationRequested();
        // One bounded EOF lookahead rejects a concurrently growing file without
        // allowing an unbounded hash-to-EOF read past the admitted exact length.
        return stream.ReadByte() < 0 ? hash.GetHashAndReset() : null;
    }

    private static void WriteTrialCreateNew(RepositoryOwnedModelTrainingAttemptAddress address, string name, byte[] bytes, int cap, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var path = Path.Combine(address.AttemptRoot, name);
        if (bytes.Length is < 1 || bytes.Length > cap || !ValidatePreparedClaim(address).IsSuccess ||
            Directory.Exists(path) || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path)) throw new IOException("Unsafe trial write.");
        using var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.WriteThrough);
        if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path) ||
            !SecureLocalFileBoundary.HasOwnerOnlyFileMode(path)) throw new IOException("Trial write custody failed.");
        stream.Write(bytes); stream.Flush(true);
        if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path) ||
            !ValidatePreparedClaim(address).IsSuccess || !SecureLocalFileBoundary.HasOwnerOnlyFileMode(path) ||
            SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path)) throw new IOException("Trial write custody changed.");
    }

    // This is a fresh original owner AND full pair owner read on every call.
    // No caller score, hydrated state or reference coordinate is effect authority.
    internal static async Task<OperationResult<PreparedSnapshot>> ValidatePreparedAsync(
        RepositoryOwnedModelTrainingAttemptAddress address,
        CodeDocumentationPreparedBaselineAttemptInputs inputs,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            if (inputs is null || !RepositoryOwnedTrainingArtifactLayout
                    .IsValidModelTrainingAttemptAddress(address))
                return Invalid<PreparedSnapshot>();
            if (inputs.Reference is null || new[] { inputs.Reference.RepositoryRoot,
                inputs.Reference.BaselineRoot, inputs.Reference.BaselineRootRelativePath,
                inputs.Reference.BaselinePath, inputs.Reference.ClaimPath, inputs.Reference.LockPath }
                .Any(static value => string.IsNullOrWhiteSpace(value) || value.Length > 4096 || value.Any(char.IsControl)))
                return Invalid<PreparedSnapshot>();
            var lesson = inputs.LessonDatasetAddress;
            var rebuilt = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
                address.RepositoryRoot, address.DomainSlug, address.DesignPatternSlug,
                lesson.DatasetVersion);
            if (!rebuilt.IsSuccess || rebuilt.Value is null ||
                JsonSerializer.Serialize(rebuilt.Value) != JsonSerializer.Serialize(lesson))
                return Invalid<PreparedSnapshot>();
            var fingerprint = CapturePreparedInputs(inputs);
            var original = await new CodeDocumentationRepositoryOwnedPreparedBaselineJournal()
                .OpenExistingByReferenceAsync(inputs.Reference, inputs.Catalog, inputs.Selector,
                    address.RepositoryRoot, address.DomainSlug, address.DesignPatternSlug,
                    address.ModelIdSlug, address.ModelVersion, inputs.ModelConfig,
                    inputs.RuntimeCompatibility, inputs.QualityGateContext,
                    inputs.OriginalLoraConfig, inputs.OriginalAdapterSeeds, cancellationToken)
                .ConfigureAwait(false);
            if (!original.IsSuccess || original.Value is null)
                return original.MapError<PreparedSnapshot>();
            var needsLesson = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                .BaselineRequiresNewTrainingPair(original.Value.Baseline);
            if (!needsLesson.IsSuccess || needsLesson.Value != true)
                return needsLesson.MapError<PreparedSnapshot>();
            if (original.Value.Classification != "NEEDS_LESSON")
                return Invalid<PreparedSnapshot>();

            var loaded = new RepositoryOwnedModelCheckpointLoader().LoadLatestCompatible(
                address.RepositoryRoot, address.DomainSlug, address.DesignPatternSlug,
                address.ModelIdSlug, address.ModelVersion, inputs.RuntimeCompatibility);
            if (!loaded.IsSuccess || loaded.Value is null)
                return loaded.MapError<PreparedSnapshot>();
            using var checkpoint = loaded.Value;
            var parent = checkpoint.Manifest;
            var passport = new CodeDocumentationSavedModelAssessmentModelPassport(
                parent.Domain, parent.DesignPattern, parent.ModelId, parent.Version, parent.Step,
                parent.RepositoryRelativeManifestPath, parent.ManifestSha256,
                parent.RepositoryRelativeWeightsPath, checkpoint.WeightsSha256,
                checkpoint.WeightsByteLength, checkpoint.WeightsByteLength,
                RepositoryRelativeModelIdentityPath: parent.RepositoryRelativeManifestPath,
                ModelIdentitySha256: parent.ManifestSha256);
            var current = RepositoryOwnedDpoDatasetManifestSealer.OpenSealedFileIdentity(
                rebuilt.Value, inputs.LessonExpectedRequestIdentity);
            if (!current.IsSuccess || current.Value is null)
                return current.MapError<PreparedSnapshot>();
            if (current.Value.Manifest.PairCount != 1 || current.Value.Pairs.Count != 1 ||
                current.Value.Manifest.OrderedPairs.Count != 1 ||
                parent.DpoDatasetManifest.RepositoryRelativeManifestPath == lesson.ManifestRelativePath)
                return Invalid<PreparedSnapshot>();
            // The new lesson must differ in content identity, not just in its directory.
            var parentVersion = parent.DpoDatasetManifest.RepositoryRelativeManifestPath.Split('/')[^2];
            var parentAddress = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
                address.RepositoryRoot, address.DomainSlug, address.DesignPatternSlug, parentVersion);
            if (!parentAddress.IsSuccess || parentAddress.Value is null)
                return Invalid<PreparedSnapshot>();
            var parentPairs = RepositoryOwnedDpoDatasetManifestSealer.OpenSealedForManifestIntegrity(
                parentAddress.Value);
            if (!parentPairs.IsSuccess || parentPairs.Value is null)
                return parentPairs.MapError<PreparedSnapshot>();
            var row = current.Value.Manifest.OrderedPairs[0];
            if (parentPairs.Value.Manifest.OrderedPairs.Any(p => p.PairId == row.PairId))
                return Invalid<PreparedSnapshot>();
            var state = new CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState(
                2, address.TrainingRunId, address.TrainingRunIdSha256,
                address.AttemptRootRelativePath, address.AttemptClaimRelativePath,
                address.DomainSlug, address.DesignPatternSlug, address.ModelIdSlug,
                address.ModelVersion, passport, lesson.ManifestRelativePath,
                current.Value.Manifest.ManifestSha256, row.PairId,
                row.RepositoryRelativePairFilePath, row.PairFileSha256, original.Value.Baseline);
            var valid = ValidateMatchesAttempt(address, state with { FormatVersion = CurrentFormatVersion });
            if (!valid.IsSuccess || fingerprint != CapturePreparedInputs(inputs))
                return Invalid<PreparedSnapshot>();
            return OperationResult<PreparedSnapshot>.Ok(new(state,
                EncodePrepared(state, original.Value.Reference), fingerprint,
                current.Value.ManifestFileSha256));
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid<PreparedSnapshot>();
        }
    }

    internal static string CapturePreparedInputs(CodeDocumentationPreparedBaselineAttemptInputs inputs)
    {
        using var tokenizer = new MemoryStream();
        inputs.QualityGateContext.Tokenizer.Save(tokenizer);
        var context = inputs.QualityGateContext;
        var grant = inputs.Catalog.Resolve(inputs.Selector);
        return Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new
        {
            Grant = grant.Value, inputs.Catalog.MaxTeacherCalls, inputs.Selector,
            inputs.ModelConfig, inputs.RuntimeCompatibility, context.CorpusManifest,
            context.Corpus.CorpusSha256, context.GenerationSettings, context.EvaluatorConfiguration,
            context.NoisePolicyConfig, context.MaximumDuration, context.ObservedAtUtc,
            context.EvidenceRefs, inputs.OriginalLoraConfig, inputs.OriginalAdapterSeeds,
            inputs.LessonDatasetAddress, inputs.LessonExpectedRequestIdentity,
            Tokenizer = Convert.ToHexString(SHA256.HashData(tokenizer.ToArray())),
        }))).ToLowerInvariant();
    }

    internal static Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>>
        WritePreparedAndReadBackAsync(RepositoryOwnedModelTrainingAttemptAddress address,
            CodeDocumentationPreparedBaselineAttemptInputs inputs, CancellationToken cancellationToken = default)
        => OpenPreparedCoreAsync(address, inputs, write: true, cancellationToken);

    internal static Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>>
        OpenPreparedExistingAsync(RepositoryOwnedModelTrainingAttemptAddress address,
            CodeDocumentationPreparedBaselineAttemptInputs inputs, CancellationToken cancellationToken = default)
        => OpenPreparedCoreAsync(address, inputs, write: false, cancellationToken);

    private static async Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>>
        OpenPreparedCoreAsync(RepositoryOwnedModelTrainingAttemptAddress address,
            CodeDocumentationPreparedBaselineAttemptInputs inputs, bool write, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Authenticate before any strict owner custody preparation. Those owners
            // may normalize their own artifact tree; an unsafe attempt must not be
            // admitted (or silently repaired) by that unrelated preparation.
            var claim = ValidatePreparedClaim(address);
            if (!claim.IsSuccess)
                return claim.MapError<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            var path = BuildBaselineStatePath(address);
            if (!path.IsSuccess || path.Value is null)
                return path.MapError<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            if (!write)
            {
                var existing = ReadExisting(path.Value);
                if (!existing.IsSuccess)
                    return existing.MapError<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            }
            else if (File.Exists(path.Value) || Directory.Exists(path.Value) ||
                SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path.Value))
                return Unavailable<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            var fresh = await ValidatePreparedAsync(address, inputs, cancellationToken).ConfigureAwait(false);
            if (!fresh.IsSuccess || fresh.Value is null)
                return fresh.MapError<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            claim = ValidatePreparedClaim(address);
            if (!claim.IsSuccess)
                return claim.MapError<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            var expected = fresh.Value.Bytes;
            if (expected.Length is < 1 || expected.LongLength >= MaximumBaselineStateBytes)
                return Invalid<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            if (write)
            {
                if (File.Exists(path.Value) || Directory.Exists(path.Value) ||
                    SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path.Value))
                    return Unavailable<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
                using var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(path.Value,
                    FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.WriteThrough);
                if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path.Value) ||
                    !SecureLocalFileBoundary.HasOwnerOnlyFileMode(path.Value))
                    return Unavailable<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
                stream.Write(expected);
                stream.Flush(true);
            }
            cancellationToken.ThrowIfCancellationRequested();
            var opened = ReadExisting(path.Value);
            if (!opened.IsSuccess || opened.Value is null)
                return opened.MapError<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            // Comparing against an independently freshly derived complete canonical wire also
            // rejects recursive duplicates, missing/unknown/case-alias fields, wrong types,
            // number/escape/order/whitespace aliases and trailing content; no default-filled DTO.
            using var document = JsonDocument.Parse(opened.Value.Bytes);
            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                !CryptographicOperations.FixedTimeEquals(expected, opened.Value.Bytes) ||
                !ValidatePreparedClaim(address).IsSuccess)
                return Invalid<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            return OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>.Ok(
                new(path.Value, address.AttemptRootRelativePath + "/" + BaselineStateFileName,
                    opened.Value.Sha256, opened.Value.Bytes.LongLength, fresh.Value.State));
        }
        catch (OperationCanceledException) { throw; }
        catch (JsonException) { return Invalid<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>(); }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
        }
    }

    private static OperationResult<bool> ValidatePreparedClaim(RepositoryOwnedModelTrainingAttemptAddress address)
    {
        if (address is null || !SecureLocalFileBoundary.IsSupportedPlatform() ||
            !Directory.Exists(address.AttemptRoot) ||
            SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(address.AttemptClaimPath) ||
            !SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(address.AttemptRoot) ||
            !SecureLocalFileBoundary.HasOwnerOnlyFileMode(address.AttemptClaimPath))
            return Unavailable<bool>();
        if (!RepositoryOwnedTrainingArtifactLayout.IsValidModelTrainingAttemptAddress(address))
            return Invalid<bool>();
        using var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(address.AttemptClaimPath,
            FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.SequentialScan);
        if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, address.AttemptClaimPath))
            return Unavailable<bool>();
        var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, 4096);
        var expected = Encoding.UTF8.GetBytes("training_run_id=" + address.TrainingRunId + Environment.NewLine +
            "training_run_id_sha256=" + address.TrainingRunIdSha256 + Environment.NewLine +
            "attempt_root=" + address.AttemptRootRelativePath + Environment.NewLine);
        // Existing owner-storage provenance, not a cryptographic issuer claim.
        return bytes is not null && CryptographicOperations.FixedTimeEquals(expected, bytes) &&
            SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(address.AttemptRoot) &&
            SecureLocalFileBoundary.HasOwnerOnlyFileMode(address.AttemptClaimPath) &&
            !SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(address.AttemptClaimPath) &&
            SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, address.AttemptClaimPath)
            ? OperationResult<bool>.Ok(true) : Invalid<bool>();
    }

    private static byte[] EncodePrepared(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState state,
        CodeDocumentationPreparedBaselineReference reference)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteNumber("FormatVersion", 2);
            writer.WriteString("TrainingRunId", state.TrainingRunId);
            writer.WriteString("TrainingRunIdSha256", state.TrainingRunIdSha256);
            writer.WriteString("AttemptRootRelativePath", state.AttemptRootRelativePath);
            writer.WriteString("AttemptClaimRelativePath", state.AttemptClaimRelativePath);
            writer.WriteString("DomainSlug", state.DomainSlug);
            writer.WriteString("DesignPatternSlug", state.DesignPatternSlug);
            writer.WriteString("ModelIdSlug", state.ModelIdSlug);
            writer.WriteString("ModelVersion", state.ModelVersion);
            writer.WriteStartObject("AcceptedModelPassport");
            var p = state.AcceptedModelPassport;
            writer.WriteString("DomainSlug", p.DomainSlug);
            writer.WriteString("DesignPatternSlug", p.DesignPatternSlug);
            writer.WriteString("ModelIdSlug", p.ModelIdSlug);
            writer.WriteString("ModelVersion", p.ModelVersion);
            writer.WriteNumber("CheckpointStep", p.CheckpointStep);
            writer.WriteString("RepositoryRelativeManifestPath", p.RepositoryRelativeManifestPath);
            writer.WriteString("ManifestSha256", p.ManifestSha256);
            writer.WriteString("RepositoryRelativeWeightsPath", p.RepositoryRelativeWeightsPath);
            writer.WriteString("WeightsSha256", p.WeightsSha256);
            writer.WriteNumber("WeightsByteLength", p.WeightsByteLength);
            writer.WriteNumber("MaximumModelSerializationBytes", p.MaximumModelSerializationBytes);
            writer.WriteString("ModelSourceKind", p.ModelSourceKind);
            writer.WriteString("RepositoryRelativeModelIdentityPath", p.RepositoryRelativeModelIdentityPath);
            writer.WriteString("ModelIdentitySha256", p.ModelIdentitySha256);
            writer.WriteEndObject();
            writer.WriteString("DpoManifestRelativePath", state.DpoManifestRelativePath);
            writer.WriteString("DpoManifestSha256", state.DpoManifestSha256);
            writer.WriteString("TrainingPairId", state.TrainingPairId);
            writer.WriteString("TrainingPairRelativePath", state.TrainingPairRelativePath);
            writer.WriteString("TrainingPairSha256", state.TrainingPairSha256);
            writer.WriteStartObject("PreparedBaselineReference");
            writer.WriteNumber("FormatVersion", reference.FormatVersion);
            writer.WriteString("RepositoryRoot", reference.RepositoryRoot);
            writer.WriteString("ObjectiveSha256", reference.ObjectiveSha256);
            writer.WriteString("BaselineRoot", reference.BaselineRoot);
            writer.WriteString("BaselineRootRelativePath", reference.BaselineRootRelativePath);
            writer.WriteString("BaselinePath", reference.BaselinePath);
            writer.WriteString("ClaimPath", reference.ClaimPath);
            writer.WriteString("LockPath", reference.LockPath);
            writer.WriteString("RequestSha256", reference.RequestSha256);
            writer.WriteString("Sha256", reference.Sha256);
            writer.WriteNumber("ByteLength", reference.ByteLength);
            writer.WriteString("Classification", reference.Classification);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
        return buffer.ToArray();
    }

    public static OperationResult<bool> MatchesSingleTrainingPair(
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState? state,
        IReadOnlyList<DpoPair>? pairs)
    {
        if (state is null ||
            pairs is null ||
            pairs.Count != 1 ||
            !TrainingPairIdentityValid(state))
        {
            return Invalid<bool>();
        }

        var pairId = FileDpoPairStore.ComputeIdentity(
            pairs[0],
            RichPairRecordSchemaVersion);
        if (!StringComparer.Ordinal.Equals(pairId, state.TrainingPairId))
        {
            return Invalid<bool>();
        }

        return OperationResult<bool>.Ok(true);
    }

    public static OperationResult<
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>
        WriteAndReadBack(
            RepositoryOwnedModelTrainingAttemptAddress? address,
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState? state,
            CancellationToken cancellationToken = default)
    {
        var validation = ValidateMatchesAttempt(address, state);
        if (!validation.IsSuccess)
        {
            return validation.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = BuildBaselineStatePath(address!);
            if (!path.IsSuccess || path.Value is null)
            {
                return path.MapError<
                    CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            }

            var bytes = JsonSerializer.SerializeToUtf8Bytes(
                state!,
                JsonOptions);
            if (bytes.Length <= 0 || bytes.LongLength > MaximumBaselineStateBytes)
            {
                return Invalid<
                    CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            }

            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    path.Value)
                || Directory.Exists(path.Value)
                || File.Exists(path.Value))
            {
                return Unavailable<
                    CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            }

            using (var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                       path.Value,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       FileOptions.WriteThrough))
            {
                if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                        stream.SafeFileHandle,
                        path.Value)
                    || !SecureLocalFileBoundary.HasOwnerOnlyFileMode(
                        path.Value))
                {
                    return Unavailable<
                        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
                }

                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(flushToDisk: true);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var readback = ReadExisting(path.Value);
            if (!readback.IsSuccess || readback.Value is null)
            {
                return readback.MapError<
                    CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            }

            if (!CryptographicOperations.FixedTimeEquals(
                    bytes,
                    readback.Value.Bytes))
            {
                return Invalid<
                    CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            }

            var restored = JsonSerializer.Deserialize<
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState>(
                readback.Value.Bytes,
                JsonOptions);
            validation = ValidateMatchesAttempt(address, restored);
            if (!validation.IsSuccess || restored is null)
            {
                return validation.MapError<
                    CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            }

            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>.Ok(
                new CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback(
                    path.Value,
                    address!.AttemptRootRelativePath + "/" + BaselineStateFileName,
                    readback.Value.Sha256,
                    readback.Value.Bytes.LongLength,
                    restored));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException)
        {
            return Unavailable<
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
        }
        catch (JsonException)
        {
            return Invalid<
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
        }
    }

    public static OperationResult<
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>
        OpenExisting(
            RepositoryOwnedModelTrainingAttemptAddress? address,
            CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (address is null ||
                !RepositoryOwnedTrainingArtifactLayout
                    .IsValidModelTrainingAttemptAddress(address))
            {
                return Invalid<
                    CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            }

            var path = BuildBaselineStatePath(address);
            if (!path.IsSuccess || path.Value is null)
            {
                return path.MapError<
                    CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            }

            var readback = ReadExisting(path.Value);
            if (!readback.IsSuccess || readback.Value is null)
            {
                return readback.MapError<
                    CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            }

            var restored = JsonSerializer.Deserialize<
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState>(
                readback.Value.Bytes,
                JsonOptions);
            var validation = ValidateMatchesAttempt(address, restored);
            if (!validation.IsSuccess || restored is null)
            {
                return validation.MapError<
                    CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
            }

            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>.Ok(
                new CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback(
                    path.Value,
                    address.AttemptRootRelativePath + "/" + BaselineStateFileName,
                    readback.Value.Sha256,
                    readback.Value.Bytes.LongLength,
                    restored));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (JsonException)
        {
            return Invalid<
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback>();
        }
    }

    private static OperationResult<StableFileBytes> ReadExisting(string path)
    {
        try
        {
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path)
                || !File.Exists(path)
                || Directory.Exists(path))
            {
                return Unavailable<StableFileBytes>();
            }

            using var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                FileOptions.SequentialScan);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    path)
                || !SecureLocalFileBoundary.HasOwnerOnlyFileMode(path))
            {
                return Unavailable<StableFileBytes>();
            }

            var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(
                stream,
                MaximumBaselineStateBytes);
            if (bytes is null)
                return Invalid<StableFileBytes>();

            return OperationResult<StableFileBytes>.Ok(
                new StableFileBytes(
                    bytes,
                    Convert
                        .ToHexString(SHA256.HashData(bytes))
                        .ToLowerInvariant()));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<StableFileBytes>();
        }
    }

    private static OperationResult<string> BuildBaselineStatePath(
        RepositoryOwnedModelTrainingAttemptAddress address)
    {
        if (address is null ||
            !RepositoryOwnedTrainingArtifactLayout
                .IsValidModelTrainingAttemptAddress(address))
        {
            return Invalid<string>();
        }

        var attemptRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(address.AttemptRoot));
        var path = Path.GetFullPath(Path.Combine(
            attemptRoot,
            BaselineStateFileName));
        if (!StringComparerForPaths.Equals(attemptRoot, address.AttemptRoot) ||
            !path.StartsWith(
                attemptRoot + Path.DirectorySeparatorChar,
                PathComparison))
        {
            return Invalid<string>();
        }

        return OperationResult<string>.Ok(path);
    }

    private static OperationResult<bool> ValidateMatchesAttempt(
        RepositoryOwnedModelTrainingAttemptAddress? address,
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState? state)
    {
        if (address is null ||
            state is null ||
            !RepositoryOwnedTrainingArtifactLayout
                .IsValidModelTrainingAttemptAddress(address) ||
            state.FormatVersion != CurrentFormatVersion ||
            !StringComparer.Ordinal.Equals(
                state.TrainingRunId,
                address.TrainingRunId) ||
            !StringComparer.Ordinal.Equals(
                state.TrainingRunIdSha256,
                address.TrainingRunIdSha256) ||
            !StringComparer.Ordinal.Equals(
                state.AttemptRootRelativePath,
                address.AttemptRootRelativePath) ||
            !StringComparer.Ordinal.Equals(
                state.AttemptClaimRelativePath,
                address.AttemptClaimRelativePath) ||
            !StringComparer.Ordinal.Equals(
                state.DomainSlug,
                address.DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                state.DesignPatternSlug,
                address.DesignPatternSlug) ||
            !StringComparer.Ordinal.Equals(
                state.ModelIdSlug,
                address.ModelIdSlug) ||
            !StringComparer.Ordinal.Equals(
                state.ModelVersion,
                address.ModelVersion) ||
            !AcceptedPassportMatchesState(state) ||
            !TrainingPairIdentityValid(state) ||
            !BaselineQualityMatchesState(state))
        {
            return Invalid<bool>();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool AcceptedPassportMatchesState(
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState state)
    {
        var passport = state.AcceptedModelPassport;
        return passport is not null &&
               StringComparer.Ordinal.Equals(
                   passport.ModelSourceKind,
                   CodeDocumentationSavedModelAssessmentModelPassport
                       .AcceptedRepositoryCheckpointSourceKind) &&
               StringComparer.Ordinal.Equals(
                   passport.DomainSlug,
                   state.DomainSlug) &&
               StringComparer.Ordinal.Equals(
                   passport.DesignPatternSlug,
                   state.DesignPatternSlug) &&
               StringComparer.Ordinal.Equals(
                   passport.ModelIdSlug,
                   state.ModelIdSlug) &&
               StringComparer.Ordinal.Equals(
                   passport.ModelVersion,
                   state.ModelVersion) &&
               passport.CheckpointStep > 0 &&
               passport.WeightsByteLength > 0 &&
               passport.MaximumModelSerializationBytes >=
               passport.WeightsByteLength &&
               IsSafeRepositoryRelativePath(
                   passport.RepositoryRelativeManifestPath) &&
               IsSafeRepositoryRelativePath(
                   passport.RepositoryRelativeWeightsPath) &&
               IsSafeRepositoryRelativePath(
                   passport.RepositoryRelativeModelIdentityPath) &&
               IsSha256(passport.ManifestSha256) &&
               IsSha256(passport.WeightsSha256) &&
               IsSha256(passport.ModelIdentitySha256);
    }

    private static bool TrainingPairIdentityValid(
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState state)
        => IsSafeRepositoryRelativePath(state.DpoManifestRelativePath) &&
           IsSha256(state.DpoManifestSha256) &&
           IsSha256(state.TrainingPairId) &&
           IsSafeRepositoryRelativePath(state.TrainingPairRelativePath) &&
           IsSha256(state.TrainingPairSha256);

    private static bool BaselineQualityMatchesState(
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState state)
    {
        var baseline = state.BaselineQuality;
        var report = baseline?.Calibration?.Calibration;
        var representative = baseline?.Calibration?.RepresentativeAssessment;
        if (baseline is null ||
            report is null ||
            representative is null ||
            !report.IsSuccess ||
            baseline.ExpectedEvaluationCallCount <= 0 ||
            baseline.ObservedEvaluationCallCount <= 0 ||
            baseline.ExpectedEvaluationCallCount !=
            report.ExpectedEvaluationCallCount ||
            baseline.ObservedEvaluationCallCount !=
            report.ObservedEvaluationCallCount ||
            report.Observations is null ||
            report.Observations.Count != report.RepeatCount ||
            report.HeldOutCaseCount <= 0 ||
            report.MeasurementNoisePassport is null ||
            report.ScorerIdentity is null ||
            !report.ScorerIdentity.Equals(
                report.MeasurementNoisePassport.ScorerIdentity) ||
            !StringComparer.Ordinal.Equals(
                report.EvaluationSetId,
                report.MeasurementNoisePassport.EvaluationSetId) ||
            representative.Cases is null ||
            representative.Cases.Count != report.HeldOutCaseCount ||
            !StringComparer.Ordinal.Equals(
                representative.DomainSlug,
                state.DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                representative.DesignPatternSlug,
                state.DesignPatternSlug) ||
            !StringComparer.Ordinal.Equals(
                representative.ModelSourceKind,
                CodeDocumentationSavedModelAssessmentModelPassport
                    .AcceptedRepositoryCheckpointSourceKind) ||
            !StringComparer.Ordinal.Equals(
                representative.RepositoryRelativeModelIdentityPath,
                state.AcceptedModelPassport.RepositoryRelativeModelIdentityPath) ||
            !StringComparer.Ordinal.Equals(
                representative.ModelIdentitySha256,
                state.AcceptedModelPassport.ModelIdentitySha256) ||
            !StringComparer.Ordinal.Equals(
                representative.ModelManifestSha256,
                state.AcceptedModelPassport.ManifestSha256) ||
            !StringComparer.Ordinal.Equals(
                representative.SavedWeightsSha256,
                state.AcceptedModelPassport.WeightsSha256) ||
            representative.ModelSerializationBeforeByteLength <= 0 ||
            representative.ModelSerializationAfterByteLength <= 0 ||
            representative.ModelSerializationBeforeByteLength >
            state.AcceptedModelPassport.MaximumModelSerializationBytes ||
            representative.ModelSerializationAfterByteLength >
            state.AcceptedModelPassport.MaximumModelSerializationBytes ||
            !StringComparer.Ordinal.Equals(
                representative.ModelSerializationBeforeSha256,
                representative.ModelSerializationAfterSha256))
        {
            return false;
        }

        foreach (var observation in report.Observations)
        {
            if (observation is null ||
                observation.EvaluatedCaseCount != report.HeldOutCaseCount ||
                observation.EvaluatedCaseCount <= 0 ||
                observation.PassedCaseCount < 0 ||
                observation.PassedCaseCount > observation.EvaluatedCaseCount ||
                observation.CaseIds is null ||
                observation.CaseIds.Count != report.HeldOutCaseCount ||
                observation.StudentAnswerSha256 is null ||
                observation.StudentAnswerSha256.Count !=
                report.HeldOutCaseCount ||
                observation.ObservedEvaluationRequestIds is null ||
                observation.ObservedEvaluationRequestIds.Count !=
                report.HeldOutCaseCount ||
                !double.IsFinite(observation.CorrectFraction) ||
                observation.CorrectFraction < 0d ||
                observation.CorrectFraction > 1d ||
                !IsSha256(observation.ModelSerializationBeforeSha256) ||
                !IsSha256(observation.ModelSerializationAfterSha256) ||
                !StringComparer.Ordinal.Equals(
                    observation.ModelSerializationBeforeSha256,
                    observation.ModelSerializationAfterSha256) ||
                !StringComparer.Ordinal.Equals(
                    observation.GenerationSettingsSha256,
                    representative.GenerationSettingsSha256))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsSafeRepositoryRelativePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            !StringComparer.Ordinal.Equals(path, path.Trim()) ||
            path.Length > MaximumRepositoryRelativePathLength ||
            path.Any(char.IsControl) ||
            path.StartsWith("/", StringComparison.Ordinal) ||
            path.StartsWith("\\", StringComparison.Ordinal) ||
            path.Contains('\\', StringComparison.Ordinal) ||
            path.Contains(':', StringComparison.Ordinal) ||
            path.Contains("//", StringComparison.Ordinal) ||
            Path.IsPathFullyQualified(path))
        {
            return false;
        }

        var segments = path.Split('/');
        return segments.Length > 0 &&
               segments.All(static segment =>
                   segment.Length > 0 &&
                   segment.Length <= MaximumIdentityLength &&
                   segment is not "." and not "..");
    }

    private static bool IsSha256(string? value)
        => value is { Length: HashHexLength } &&
           value.All(static c => c is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static OperationResult<T> Invalid<T>()
        => OperationResult<T>.Fail(
            "The CodeDocumentation DPO baseline attempt state does not match the sealed attempt, accepted model, single pair, or measured baseline exam.",
            InvalidErrorCode);

    private static OperationResult<T> Unavailable<T>()
        => OperationResult<T>.Fail(
            "The CodeDocumentation DPO baseline attempt state could not be safely persisted and reopened before training.",
            UnavailableErrorCode);

    private static StringComparison PathComparison
        => OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private static StringComparer StringComparerForPaths { get; }
        = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    private sealed record StableFileBytes(byte[] Bytes, string Sha256);
}
