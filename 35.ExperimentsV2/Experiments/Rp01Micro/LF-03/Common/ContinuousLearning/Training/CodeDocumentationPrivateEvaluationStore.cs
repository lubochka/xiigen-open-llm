// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
namespace ContinuousLearning; // Evaluation retention never becomes a training dataset.

using System.Security.Cryptography; // Bind retained bytes without exposing answers.
using System.Text; // Preserve exact decoded text with strict UTF8.
using System.Text.Json; // Store complete private graphs and hash-bound metadata.
using System.Text.Json.Serialization; // Reject unknown fields in new private contracts.
using AgentInfrastructure; // Keep I/O refusals within the business result boundary.
using AgentInfrastructure.Logical; // Preserve cancellation and classify only nonfatal storage failures.

public sealed record CodeDocumentationPrivateArtifactReference(string RelativePath, string Sha256, long ByteLength); // Safe reference without raw evaluation text.
public sealed record CodeDocumentationRawAnswerBinding(string AssessmentRunId, string CaseId, string OwnerInputSha256, string SettingsSha256, string CorpusManifestSha256, string CorpusSha256, CodeDocumentationSavedModelAssessmentModelPassport Model); // Bind every arm/repeat/case to exact evaluation conditions.
public sealed record CodeDocumentationRawAnswerArtifact(CodeDocumentationRawAnswerBinding Binding, CodeDocumentationPrivateArtifactReference RawUtf8, int CharCount); // Raw text lives only in the private UTF8 file.
public interface ICodeDocumentationPrivateAnswerCapture // Inject retention rather than selecting paths in an assessment request.
{ // The configured owner chooses all physical addresses.
    OperationResult<CodeDocumentationPrivateArtifactReference> Capture(CodeDocumentationRawAnswerBinding binding, string answer, CancellationToken token = default); // Save before the generated string can be discarded.
} // No raw text is logged or returned by this interface.

public sealed record CodeDocumentationScientificEvaluationAccounting(string HistoricalEvaluatorCalls, int NewDispatchIntents, int NewCallsIssued, int ReusedRawAnswers, int ReusedEvaluatorResults, int CompletedLogicalResults); // Logical completeness and lifetime physical costs are different observations.
public sealed record CodeDocumentationRetainedAssessmentSlot(bool Found, string Answer, CodeDocumentationPrivateArtifactReference? Metadata, TeacherEvaluationInferenceResult? Evaluation); // Missing is an explicit value; corrupt storage is an error.
public sealed record CodeDocumentationRetainedEvaluation(string Schema, CodeDocumentationRawAnswerBinding Binding, string RawSha256, string DispatchId, TeacherEvaluationInferenceResult Result); // Preserve the genuine checker response and complete provenance, never a fabricated permit.
internal sealed record CodeDocumentationEvaluationDispatch(string Schema, CodeDocumentationRawAnswerBinding Binding, string RawSha256, string DispatchId); // A durable issue intent without a result means UNKNOWN external effect.
public sealed class CodeDocumentationRetainedBaselineRecovery : ICodeDocumentationPrivateAnswerCapture // Server-admitted capability, never a request resume boolean.
{ // Its fixed slot set is derived from the unchanged original conditions.
    internal CodeDocumentationRetainedBaselineRecovery(CodeDocumentationPrivateEvaluationStore store, IReadOnlyDictionary<string, CodeDocumentationRawAnswerBinding> slots) { Store = store; Slots = slots; } // Only the storage admission owner constructs recovery.
    internal CodeDocumentationPrivateEvaluationStore Store { get; } // Keep immutable storage ownership through all repeat copies.
    internal IReadOnlyDictionary<string, CodeDocumentationRawAnswerBinding> Slots { get; } // Keys are deterministic original run/case digests.
    internal OperationResult<CodeDocumentationRetainedAssessmentSlot> Open(CodeDocumentationRawAnswerBinding binding, CancellationToken token) => Slots.TryGetValue(CodeDocumentationPrivateEvaluationStore.SlotKey(binding), out var expected) && CodeDocumentationPrivateEvaluationStore.SameBinding(expected, binding) ? Store.OpenRetainedSlot(binding, token) : OperationResult<CodeDocumentationRetainedAssessmentSlot>.Fail("Retained baseline slot is outside the admitted original set.", CodeDocumentationPrivateEvaluationStore.Invalid); // Misbound repeat/model/arm cannot fall back to generation.
    public OperationResult<CodeDocumentationPrivateArtifactReference> Capture(CodeDocumentationRawAnswerBinding binding, string answer, CancellationToken token = default) => Slots.TryGetValue(CodeDocumentationPrivateEvaluationStore.SlotKey(binding), out var expected) && CodeDocumentationPrivateEvaluationStore.SameBinding(expected, binding) ? Store.Capture(binding, answer, token) : OperationResult<CodeDocumentationPrivateArtifactReference>.Fail("Retained baseline slot binding changed.", CodeDocumentationPrivateEvaluationStore.Invalid); // Generate only a genuinely missing admitted slot.
} // No optimizer or publication authority is granted.

public sealed class CodeDocumentationPrivateEvaluationStore : ICodeDocumentationPrivateAnswerCapture // Fixed private subtree for one genuine training attempt identity.
{ // Use existing custody primitives on both supported platforms.
    public const string Invalid = "CODE_DOCUMENTATION_PRIVATE_EVALUATION_ARTIFACT_INVALID"; // A changed or unsafe artifact cannot be reused.
    private static readonly UTF8Encoding Utf8 = new(false, true); // Reject lossy replacement of malformed text.
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = false, RespectRequiredConstructorParameters = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow }; // Missing constructor booleans/counts and differently cased aliases cannot silently acquire defaults in a real verdict.
    private int _newDispatchIntents, _newCallsIssued, _reusedRawAnswers, _reusedEvaluatorResults, _completedLogicalResults; // Counters cover only this invocation; earlier lost calls remain UNKNOWN.
    internal CodeDocumentationScientificEvaluationAccounting Accounting(bool recovery) => new(recovery ? "UNKNOWN" : "NO_PRIOR_SCIENTIFIC_INTENT", _newDispatchIntents, _newCallsIssued, _reusedRawAnswers, _reusedEvaluatorResults, _completedLogicalResults); // Never report 120 logical slots as 120 lifetime physical calls.
    internal void RecordCallIssued() => Interlocked.Increment(ref _newCallsIssued); // Called only when the actual current permit reports issuance.
    internal void RecordCompletedSlot(bool reusedRaw, bool reusedResult) { if (reusedRaw) Interlocked.Increment(ref _reusedRawAnswers); if (reusedResult) Interlocked.Increment(ref _reusedEvaluatorResults); Interlocked.Increment(ref _completedLogicalResults); } // Count validated logical outcomes independently of dispatch effects.
    private readonly string _repositoryRoot, _root, _relativeRoot; // Neither client nor corpus supplies an output path.
    private CodeDocumentationPrivateEvaluationStore(RepositoryOwnedModelTrainingAttemptAddress address) // Accept only the layout owner's canonical address.
    { // Keep this subtree outside canonical model checkpoint discovery.
        _repositoryRoot = address.RepositoryRoot; // Retain the trusted canonical checkout.
        _relativeRoot = $"training-artifacts/private-evaluation/{address.DomainSlug}/{address.DesignPatternSlug}/{address.ModelIdSlug}/{address.ModelVersion}/{address.TrainingRunIdSha256}"; // Reuse the layout owner's safe run digest; a run ID may contain Windows-invalid ':' and never becomes a path segment.
        _root = Path.Combine(_repositoryRoot, _relativeRoot.Replace('/', Path.DirectorySeparatorChar)); // Derive one fixed server-owned destination.
    } // Construction has no file or model effects.
    public static OperationResult<CodeDocumentationPrivateEvaluationStore> Create(RepositoryOwnedModelTrainingAttemptAddress address) // Expose a typed layout check before storage.
        => RepositoryOwnedTrainingArtifactLayout.IsValidModelTrainingAttemptAddress(address) // Reuse existing root/slug/run identity validation.
            ? OperationResult<CodeDocumentationPrivateEvaluationStore>.Ok(new(address)) // No directory is created merely to configure retention.
            : OperationResult<CodeDocumentationPrivateEvaluationStore>.Fail("Private evaluation requires a canonical attempt address.", Invalid); // Reject arbitrary caller output locations.
    internal static string SlotKey(CodeDocumentationRawAnswerBinding binding) => Hash(Utf8.GetBytes(binding.AssessmentRunId + "\n" + binding.CaseId)); // Preserve the original deterministic Capture address.
    internal static bool SameBinding(CodeDocumentationRawAnswerBinding left, CodeDocumentationRawAnswerBinding right) => JsonSerializer.Serialize(left, Json) == JsonSerializer.Serialize(right, Json); // Compare every current model/input/settings/corpus field.
    public OperationResult<FileStream> AcquireScientificInvocationLease() // All explicit fresh, recovery and publish operations share this per-run exclusion boundary.
    { // This lock is separate from the executor's short model/version pair lock.
        FileStream? stream = null; // Dispose a refused handle without deleting any evidence.
        try // A changed namespace or contention refuses immediately with no retry.
        { // Reuse normal private directory creation and validation without making an intent.
            EnsurePrivateDirectory(_root); // Protect all private ancestors before opening the lock.
            var path = Resolve(".invocation.lock"); // The original run identity determines the address.
            if (Directory.Exists(path) || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path)) return Fail<FileStream>(); // Never replace a foreign namespace.
            try { stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None, FileOptions.WriteThrough); } // Existing locks require genuine owner custody.
            catch (IOException exception) when (exception is FileNotFoundException || (exception.HResult & 0xffff) == 2) { stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, FileOptions.WriteThrough); } // Only clean absence permits creation; a concurrent creator is refused.
            if (!Custody(path) || !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path)) { stream.Dispose(); return Fail<FileStream>(); } // Authenticate the actual opened handle.
            if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux()) stream.Lock(0, 1); else { stream.Dispose(); return Fail<FileStream>(); } // Preserve process-visible locking on supported platforms.
            return OperationResult<FileStream>.Ok(stream); // Disposal releases exclusivity, preserving the lock history file.
        } // No scientific effect is inferred from acquiring a lease.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { stream?.Dispose(); return Fail<FileStream>(); } // Contention, permissions and I/O are refusals.
    } // The caller retains the handle through its entire scientific invocation.
    public OperationResult<CodeDocumentationRetainedBaselineRecovery> AdmitRetainedBaseline(CodeDocumentationPreExecutorBaselineEligibility eligibility, IReadOnlyList<CodeDocumentationRawAnswerBinding> expected, CancellationToken token = default) // Admit only exact original slots after journal-owner eligibility.
    { // Complete inventory validation happens before any new generation or evaluator dispatch.
        try // Unknown names and partial files remain visible refusals.
        { // Do not accept a capability for another canonical model/version/run.
            if (eligibility is null || new CodeDocumentationPrivateEvaluationStore(eligibility.Address)._root != _root || expected.Count != 60) return Fail<CodeDocumentationRetainedBaselineRecovery>(); // First-RP01 C0 has exactly twelve families and five repeats.
            var slots = expected.ToDictionary(SlotKey, StringComparer.Ordinal); // Duplicate original addresses fail rather than collapse.
            EnsurePrivateDirectory(_root); // Revalidate custody through every ancestor without changing existing permissions.
            foreach (var path in Directory.GetFileSystemEntries(_root)) // No Trial/held/publication or unexplained pending file belongs to this branch.
                if (Path.GetFileName(path) is not ("intent.json" or "answers" or ".invocation.lock")) return Fail<CodeDocumentationRetainedBaselineRecovery>(); // Refuse post-executor or unknown private state.
            var answers = Resolve("answers"); // The owner alone derives the retained answer directory.
            if (File.Exists(answers) || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(answers)) return Fail<CodeDocumentationRetainedBaselineRecovery>(); // A redirected or mistyped directory is not absent.
            if (Directory.Exists(answers)) // An absent readable parent has no saved slots.
            { // Every file must belong to one original baseline slot.
                if (!SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(answers)) return Fail<CodeDocumentationRetainedBaselineRecovery>(); // Never repair foreign custody.
                var entries = Directory.GetFileSystemEntries(answers); // Enumeration failure is not clean absence.
                if (entries.Length > 4096) return Fail<CodeDocumentationRetainedBaselineRecovery>(); // Bound unknown dispatch history and storage inspection.
                foreach (var path in entries) // Validate the full inventory, including dispatch intent records.
                { // Sidecars carry only genuine checker evidence within private custody.
                    var name = Path.GetFileName(path); var key = name.Split('.')[0]; // Original SHA address precedes the fixed owner suffix.
                    if (!slots.TryGetValue(key, out var binding) || !Custody(path) || Directory.Exists(path)) return Fail<CodeDocumentationRetainedBaselineRecovery>(); // Reject extra case/arm, symlink, directory or unreadable item.
                    if (name == key + ".json" || name == key + ".utf8" || name == key + ".evaluation.json") continue; // These formats are checked together below.
                    if (!name.StartsWith(key + ".dispatch-", StringComparison.Ordinal) || !name.EndsWith(".json", StringComparison.Ordinal)) return Fail<CodeDocumentationRetainedBaselineRecovery>(); // Unknown and pending files refuse.
                    var dispatch = ReadNamed<CodeDocumentationEvaluationDispatch>("answers/" + name, token); // Verify strict schema, actual bytes and custody.
                    if (!dispatch.IsSuccess || dispatch.Value is null || dispatch.Value.Schema != "evaluation-dispatch-v1" || !SameBinding(dispatch.Value.Binding, binding) || name != key + ".dispatch-" + dispatch.Value.DispatchId + ".json" || !Guid.TryParseExact(dispatch.Value.DispatchId, "N", out _)) return Fail<CodeDocumentationRetainedBaselineRecovery>(); // A previous issued-without-result dispatch remains UNKNOWN, never zero calls.
                    var raw = OpenRetainedSlot(binding, token); // Require a complete bound raw pair even when the external result was lost.
                    if (!raw.IsSuccess || raw.Value is not { Found: true } || Hash(Utf8.GetBytes(raw.Value.Answer)) != dispatch.Value.RawSha256) return Fail<CodeDocumentationRetainedBaselineRecovery>(); // No orphan dispatch can admit generation.
                } // No file is deleted or renamed by recovery admission.
            } // Validate absent and retained pairs alike.
            foreach (var binding in expected) { var opened = OpenRetainedSlot(binding, token); if (!opened.IsSuccess) return opened.MapError<CodeDocumentationRetainedBaselineRecovery>(); } // Reject raw/meta/result orphans before the first effect.
            return OperationResult<CodeDocumentationRetainedBaselineRecovery>.Ok(new(this, slots)); // This capability grants reuse of exactly the original fixed set.
        } // Legacy captures retain their existing behavior.
        catch (OperationCanceledException) { throw; } // Preserve caller cancellation.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return Fail<CodeDocumentationRetainedBaselineRecovery>(); } // Unknown inventory/binding state refuses.
    } // This method does not prove historical optimizer entry count.
    internal OperationResult<CodeDocumentationRetainedAssessmentSlot> OpenRetainedSlot(CodeDocumentationRawAnswerBinding binding, CancellationToken token) // Reopen immutable raw and optional complete checker result.
    { // Absence is established from a readable directory listing, not a failed open.
        try // Do not turn malformed or inaccessible data into a missing slot.
        { // Derive every path from the original assessment identity.
            var key = SlotKey(binding); var directory = Resolve("answers"); // No client artifact path is used.
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(directory) || File.Exists(directory)) return Fail<CodeDocumentationRetainedAssessmentSlot>(); // Unsafe namespace refuses.
            var present = Directory.GetFileSystemEntries(_root).Any(p => string.Equals(p, directory, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)); // Establish clean absence from a readable parent listing.
            if (present && ((File.GetAttributes(directory) & FileAttributes.Directory) == 0 || !SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(directory))) return Fail<CodeDocumentationRetainedAssessmentSlot>(); // An inaccessible or mistyped existing entry is not absence.
            var entries = present ? Directory.GetFileSystemEntries(directory).Select(Path.GetFileName).ToHashSet(StringComparer.Ordinal) : new HashSet<string?>(StringComparer.Ordinal); // A listing error propagates as UNKNOWN.
            var metadata = entries.Contains(key + ".json"); var raw = entries.Contains(key + ".utf8"); var evaluation = entries.Contains(key + ".evaluation.json"); // Distinguish complete absence from every partial state.
            if (!metadata && !raw && !evaluation && !entries.Any(n => n!.StartsWith(key + ".dispatch-", StringComparison.Ordinal))) return OperationResult<CodeDocumentationRetainedAssessmentSlot>.Ok(new(false, "", null, null)); // Only a genuinely empty slot may generate.
            if (!metadata || !raw) return Fail<CodeDocumentationRetainedAssessmentSlot>(); // Orphan raw/metadata/result/dispatch never causes regeneration.
            var reference = NamedReference("answers/" + key + ".json", token); // Bind the actual metadata bytes under custody.
            if (!reference.IsSuccess || reference.Value is null) return reference.MapError<CodeDocumentationRetainedAssessmentSlot>(); // Readback error refuses.
            var artifact = Read<CodeDocumentationRawAnswerArtifact>(reference.Value, token); // Strict parser rejects unknown/duplicate fields.
            if (!artifact.IsSuccess || artifact.Value is null || !SameBinding(artifact.Value.Binding, binding) || artifact.Value.RawUtf8.RelativePath != _relativeRoot + "/answers/" + key + ".utf8") return Fail<CodeDocumentationRetainedAssessmentSlot>(); // Full current input/model/settings/corpus binding is mandatory.
            var bytes = ReadBytes(artifact.Value.RawUtf8, token); // Validate actual UTF8 length and SHA under a stable handle.
            if (!bytes.IsSuccess || bytes.Value is null) return bytes.MapError<CodeDocumentationRetainedAssessmentSlot>(); // Tampered or truncated raw refuses.
            var answer = Utf8.GetString(bytes.Value); // Strict UTF8 preserves the exact saved response.
            if (answer.Length != artifact.Value.CharCount) return Fail<CodeDocumentationRetainedAssessmentSlot>(); // Validate UTF16 char count too.
            TeacherEvaluationInferenceResult? result = null; // Missing verdict requires a genuine current dispatch for this same raw.
            if (evaluation) // Cached provenance is validated again by the ordinary assessment owner.
            { // The result is private evaluation-only data.
                var saved = ReadNamed<CodeDocumentationRetainedEvaluation>("answers/" + key + ".evaluation.json", token); // Parse actual strict complete result bytes.
                if (!saved.IsSuccess || saved.Value is null || saved.Value.Schema != "retained-evaluation-v1" || saved.Value.Result is null || !Guid.TryParseExact(saved.Value.DispatchId, "N", out _) || !SameBinding(saved.Value.Binding, binding) || saved.Value.RawSha256 != artifact.Value.RawUtf8.Sha256) return Fail<CodeDocumentationRetainedAssessmentSlot>(); // No stale checker response may move to another slot.
                var dispatch = ReadNamed<CodeDocumentationEvaluationDispatch>("answers/" + key + ".dispatch-" + saved.Value.DispatchId + ".json", token); // A saved result requires its genuine durable dispatch record.
                if (!dispatch.IsSuccess || dispatch.Value is null || dispatch.Value.Schema != "evaluation-dispatch-v1" || !SameBinding(dispatch.Value.Binding, binding) || dispatch.Value.RawSha256 != saved.Value.RawSha256 || dispatch.Value.DispatchId != saved.Value.DispatchId) return Fail<CodeDocumentationRetainedAssessmentSlot>(); // Never infer a permit from a fabricated cached result.
                result = saved.Value.Result; // The ordinary profile/provenance/parser path will validate this exact response.
            } // No evaluator or Student call was issued here.
            return OperationResult<CodeDocumentationRetainedAssessmentSlot>.Ok(new(true, answer, reference.Value, result)); // Exact text stays inside the assessment owner.
        } // No preview or body is logged.
        catch (OperationCanceledException) { throw; } // Preserve explicit cancellation.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return Fail<CodeDocumentationRetainedAssessmentSlot>(); } // Refuse malformed and unknown state.
    } // Raw reuse never means training use.
    internal OperationResult<string> BeginEvaluation(CodeDocumentationRawAnswerBinding binding, string answer, CancellationToken token) // Record each newly attempted physical dispatch independently of logical result count.
    { // Pending external effects survive interruption as UNKNOWN.
        var id = Guid.NewGuid().ToString("N"); // Distinguish attempts without claiming provider idempotency.
        var saved = Put("answers/" + SlotKey(binding) + ".dispatch-" + id + ".json", new CodeDocumentationEvaluationDispatch("evaluation-dispatch-v1", binding, Hash(Utf8.GetBytes(answer)), id), token, requireNew: true); // Persist before issuing the current call permit.
        if (saved.IsSuccess) Interlocked.Increment(ref _newDispatchIntents); // A persisted issue intent is a possible call; it is not a confirmed result.
        return saved.IsSuccess ? OperationResult<string>.Ok(id) : saved.MapError<string>(); // A failed write prevents the dispatch.
    } // This intent counts as a possible call, not a proven completed call.
    internal OperationResult<CodeDocumentationPrivateArtifactReference> SaveEvaluation(CodeDocumentationRawAnswerBinding binding, string answer, string dispatchId, TeacherEvaluationInferenceResult result, CancellationToken token) // Persist genuine complete output/provenance before advancing to another case.
        => Put("answers/" + SlotKey(binding) + ".evaluation.json", new CodeDocumentationRetainedEvaluation("retained-evaluation-v1", binding, Hash(Utf8.GetBytes(answer)), dispatchId, result), token, requireNew: true); // Never overwrite a previous score or select a preferable answer.
    private OperationResult<T> ReadNamed<T>(string name, CancellationToken token) // Custody anchors retained metadata; expected scientific binding is checked by its caller.
    { // No self-declared hash is trusted as proof of current conditions.
        var reference = NamedReference(name, token); // Snapshot actual stable bytes first.
        return reference.IsSuccess && reference.Value is not null ? Read<T>(reference.Value, token) : reference.MapError<T>(); // Re-read with the exact byte digest and strict parser.
    } // Changed bytes between reads refuse.
    private OperationResult<CodeDocumentationPrivateArtifactReference> NamedReference(string name, CancellationToken token) // Internal deterministic names only.
    { // Capture the exact metadata bytes under the same private owner boundary.
        try // Missing or unsafe files are not empty slots here.
        { // Use a stable custody handle with the existing storage bound.
            token.ThrowIfCancellationRequested(); var path = Resolve(name); // Respect caller lifetime before I/O.
            if (!Custody(path)) return Fail<CodeDocumentationPrivateArtifactReference>(); // Reject permission or namespace drift.
            using var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.SequentialScan); // Authenticate private retained metadata.
            var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, 67_108_864); // All metadata must be complete and bounded.
            if (bytes is null || !Custody(path) || !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path)) return Fail<CodeDocumentationPrivateArtifactReference>(); // Verify handle/path through the full read.
            return OperationResult<CodeDocumentationPrivateArtifactReference>.Ok(new(_relativeRoot + "/" + name, Hash(bytes), bytes.Length)); // Bind actual persisted metadata bytes.
        } // No semantic raw answer enters error output.
        catch (OperationCanceledException) { throw; } // Preserve explicit cancellation.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return Fail<CodeDocumentationPrivateArtifactReference>(); } // Unknown state refuses.
    } // Reference construction alone grants no scientific authority.
    private void EnsurePrivateDirectory(string directory) // Use exactly the existing private subtree permission policy.
    { // Existing directories are checked, never normalized.
        var current = _repositoryRoot; // Begin at the trusted canonical root.
        foreach (var segment in Path.GetRelativePath(_repositoryRoot, directory).Split(Path.DirectorySeparatorChar)) // Walk all private ancestors.
        { // Shared training-artifacts remains governed by its existing owner.
            current = Path.Combine(current, segment); // Advance one fixed internal segment.
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(current) || File.Exists(current)) throw new IOException(); // Refuse redirection or mistyped directories.
            if (!Directory.Exists(current)) SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(current); // Create only genuinely absent directories.
            if (current != Path.Combine(_repositoryRoot, "training-artifacts") && !SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(current)) throw new IOException(); // Never broaden or repair existing foreign custody.
        } // All retained files remain under the original run.
    } // Public callers convert storage exceptions to typed refusals.
    public OperationResult<CodeDocumentationPrivateArtifactReference> Capture(CodeDocumentationRawAnswerBinding binding, string answer, CancellationToken token = default) // Preserve exact UTF8 and its entire binding before evaluator execution.
    { // Raw output is evaluation-only private material.
        try // Return safe error codes rather than raw answer/path exception details.
        { // Use both repeat run and case identity for deterministic create-only storage.
            token.ThrowIfCancellationRequested(); // Honor the host/caller lifetime.
            var key = Hash(Utf8.GetBytes(binding.AssessmentRunId + "\n" + binding.CaseId)); // A repeated case in another calibration repeat has a distinct file.
            var raw = PutBytes("answers/" + key + ".utf8", Utf8.GetBytes(answer), token); // Never trim, normalize, preview or replace control characters.
            if (!raw.IsSuccess || raw.Value is null) return raw; // A partial or changed previous write cannot be silently overwritten.
            return Put("answers/" + key + ".json", new CodeDocumentationRawAnswerArtifact(binding, raw.Value, answer.Length), token); // Bind exact raw hash/bytes/UTF16 char count to all evaluation identities.
        } // Finish retention before evaluation or string release.
        catch (OperationCanceledException) { throw; } // Cancellation does not pretend the evaluation completed.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return Fail<CodeDocumentationPrivateArtifactReference>(); } // Keep raw values out of error text.
    } // Only an artifact reference escapes this method.
    public OperationResult<CodeDocumentationPrivateArtifactReference> Put<T>(string name, T value, CancellationToken token = default, bool requireNew = false) // Fresh intent requires an exclusive first write; graph readback remains idempotent for exact existing bytes.
    { // Serialize once and hash the actual persisted bytes.
        try { return PutBytes(name, JsonSerializer.SerializeToUtf8Bytes(value, Json), token, requireNew); } // Existing files must match byte-for-byte and cannot reacquire a fresh intent.
        catch (OperationCanceledException) { throw; } // Preserve explicit caller cancellation.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return Fail<CodeDocumentationPrivateArtifactReference>(); } // Do not reveal private content in diagnostics.
    } // No replace/update operation exists.
    public OperationResult<T> Read<T>(CodeDocumentationPrivateArtifactReference reference, CancellationToken token = default) // Reopen a supplied immutable reference only inside this run's fixed private root.
    { // Check bytes and schema before trusting any stored graph.
        var bytes = ReadBytes(reference, token); // Reuse exact custody/hash/length readback.
        if (!bytes.IsSuccess || bytes.Value is null) return bytes.MapError<T>(); // Propagate drift refusal.
        try // Reject duplicate members independently of the JSON deserializer.
        { // A review artifact cannot choose a last-wins value for one binding.
            using var document = JsonDocument.Parse(bytes.Value); // Parse without executing content.
            if (Duplicates(document.RootElement)) return Fail<T>(); // Every nested object has one value per property.
            var value = JsonSerializer.Deserialize<T>(bytes.Value, Json); // Unknown fields fail in new contracts.
            return value is null ? Fail<T>() : OperationResult<T>.Ok(value); // Never return a null successful business value.
        } // Finish exact private readback.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return Fail<T>(); } // Refuse malformed JSON without reflecting contents.
    } // Stored text remains data, not instructions.
    public OperationResult<byte[]> ReadBytes(CodeDocumentationPrivateArtifactReference reference, CancellationToken token = default) // Hash-bound reads support raw-answer verification without logging.
    { // Resolve only this owner's relative references.
        try // Preserve nonfatal custody and drift refusals.
        { // All answers and graph references must remain beneath this exact run.
            token.ThrowIfCancellationRequested(); // Readback obeys caller/host lifetime.
            if (!reference.RelativePath.StartsWith(_relativeRoot + "/", StringComparison.Ordinal)) return Fail<byte[]>(); // Refuse cross-run or rooted references.
            var path = Resolve(reference.RelativePath[(_relativeRoot.Length + 1)..]); // Validate every path segment through one resolver.
            if (reference.ByteLength is < 0 or > 67_108_864 || !Custody(path)) return Fail<byte[]>(); // Empty decoded answers also have exact retained UTF8.
            using var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.SequentialScan); // Reuse stable custody handles.
            var bytes = reference.ByteLength == 0 ? stream.Length == 0 && stream.ReadByte() == -1 && stream.Length == 0 ? Array.Empty<byte>() : null : SecureLocalFileBoundary.ReadBoundedStableBytes(stream, reference.ByteLength); // Preserve a genuinely empty decoded answer; the existing nonempty stable-read owner remains unchanged.
            if (bytes is null || bytes.Length != reference.ByteLength || Hash(bytes) != reference.Sha256 || !Custody(path) || !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path)) return Fail<byte[]>(); // Recheck exact bytes and path after reading.
            return OperationResult<byte[]>.Ok(bytes); // Caller decides whether to decode raw evaluation UTF8.
        } // End stable readback without normalizing content.
        catch (OperationCanceledException) { throw; } // Preserve cancellation rather than declaring corrupt data.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return Fail<byte[]>(); } // No paths/raw text escape.
    } // Hash-bound references do not grant training use.
    public OperationResult<CodeDocumentationPrivateArtifactReference> OpenReference(string name, string expectedSha256, CancellationToken token = default) // Reconstruct only a caller-specified hash expectation, never trust a stored self-declared hash.
    { // The fixed private name is resolved by this owner.
        try // Existing partial files remain drift refusals.
        { // Read length as a bound, then validate all actual bytes and custody.
            var reference = new CodeDocumentationPrivateArtifactReference(_relativeRoot + "/" + name, expectedSha256, new FileInfo(Resolve(name)).Length); // No external output path can be introduced.
            var read = ReadBytes(reference, token); // Existing exact-byte owner performs the effect readback.
            return read.IsSuccess ? OperationResult<CodeDocumentationPrivateArtifactReference>.Ok(reference) : read.MapError<CodeDocumentationPrivateArtifactReference>(); // A hash mismatch never creates a new artifact.
        } // Finish hash-bound lookup.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return Fail<CodeDocumentationPrivateArtifactReference>(); } // Keep private names/content out of errors.
    } // This is read-only recovery, not authority from existence.
    public OperationResult<bool> HasHoldIntent() // Ordinary routes must fail closed even when a held attempt has only partial scientific output.
    { // Existence causes refusal, never permission or a completed-state claim.
        try { var path = Resolve("intent.json"); return OperationResult<bool>.Ok(File.Exists(path) || Directory.Exists(path) || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path)); } // A changed namespace cannot reopen automatic publication.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return Fail<bool>(); } // Unknown intent state is a refusal.
    } // No I/O mutation occurs for legacy runs with no intent.
    public static OperationResult<byte[]> ReadTrustedInput(string repositoryRoot, string relativePath, string expectedSha256, bool requirePrivateCustody, CancellationToken token = default) // Hash-bound server configuration/review input, never a client-selected output destination.
    { // Public protocol bytes need integrity; private grants and verdicts additionally require custody.
        try // Preserve bounded safe error reporting.
        { // Reject path escape before reading any external artifact.
            if (string.IsNullOrWhiteSpace(relativePath) || relativePath.Contains('\\') || relativePath.Contains(':') || relativePath.Split('/').Any(s => s.Length == 0 || s is "." or "..")) return Fail<byte[]>(); // Only canonical repository-relative sources are accepted.
            var path = Path.Combine(repositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)); // Resolve from the already trusted root.
            token.ThrowIfCancellationRequested(); // Caller/host controls lifetime.
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) || (requirePrivateCustody && (!SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(Path.GetDirectoryName(path)!) || !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path)))) return Fail<byte[]>(); // Refuse redirected/broad private sources.
            using var stream = requirePrivateCustody ? SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.SequentialScan) : new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read); // Public sources still use stable read-only handles.
            var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, 2_097_152); // Bound policy/source/review inputs independently of evaluation graph storage.
            if (bytes is null || Hash(bytes) != expectedSha256 || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) || !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path)) return Fail<byte[]>(); // Recheck actual hash and namespace after the complete read.
            return OperationResult<byte[]>.Ok(bytes); // Raw bytes stay inside the trusted evaluation owner.
        } // Finish read-only binding.
        catch (OperationCanceledException) { throw; } // Preserve explicit cancellation.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return Fail<byte[]>(); } // Do not echo private file contents.
    } // Operator-supplied hashes are a trust anchor, not cryptographic identity attestation.
    private OperationResult<CodeDocumentationPrivateArtifactReference> PutBytes(string name, byte[] bytes, CancellationToken token, bool requireNew = false) // Only the owner supplies bounded internal names and freshness semantics.
    { // Fail closed on unsafe existing directories and partial files.
        if (bytes.Length > 67_108_864) return Fail<CodeDocumentationPrivateArtifactReference>(); // Bound UTF8 and complete graph retention, including empty answers.
        var path = Resolve(name); // No arbitrary path or traversal is accepted.
        if (requireNew && (File.Exists(path) || Directory.Exists(path))) return Fail<CodeDocumentationPrivateArtifactReference>(); // A prior intent cannot admit another fresh C0/optimizer invocation.
        var current = _repositoryRoot; // Create custody only for this private subtree, not the whole checkout.
        foreach (var segment in Path.GetRelativePath(_repositoryRoot, Path.GetDirectoryName(path)!).Split(Path.DirectorySeparatorChar)) // Walk each directory before opening a file.
        { // Preserve existing shared training-artifacts while requiring custody from private-evaluation downward.
            current = Path.Combine(current, segment); // Resolve the next fixed child.
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(current)) return Fail<CodeDocumentationPrivateArtifactReference>(); // Refuse namespace redirection.
            if (!Directory.Exists(current)) SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(current); // New private directories are protected at creation.
            if (current != Path.Combine(_repositoryRoot, "training-artifacts") && !SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(current)) return Fail<CodeDocumentationPrivateArtifactReference>(); // Existing private trees must already be owner-only.
        } // No permission broadening is performed.
        var reference = new CodeDocumentationPrivateArtifactReference(_relativeRoot + "/" + name, Hash(bytes), bytes.Length); // Hash actual exact bytes, not a declaration.
        token.ThrowIfCancellationRequested(); // Refuse cancellation before writing.
        if (requireNew || !File.Exists(path)) // Exclusive fresh intent must attempt no-overwrite publication even if another writer won after the earlier existence check.
        { // Exclusive creation prevents two writers from replacing each other.
            var pending = path + ".pending-" + Guid.NewGuid().ToString("N"); // An incomplete private file never masquerades as a published intent or completed graph.
            using (var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(pending, FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.WriteThrough)) // Apply the existing private handle boundary.
            { stream.Write(bytes); stream.Flush(true); } // Flush all bytes before atomic name publication.
            if (!SecureLocalFileBoundary.PublishAtomicDurable(pending, path)) return Fail<CodeDocumentationPrivateArtifactReference>(); // Reuse existing no-overwrite durable rename/parent flush; preserve failed pending evidence.
        } // Finish create-only persistence.
        var readback = ReadBytes(reference, token); // Verify the effect through actual disk bytes.
        return readback.IsSuccess ? OperationResult<CodeDocumentationPrivateArtifactReference>.Ok(reference) : readback.MapError<CodeDocumentationPrivateArtifactReference>(); // A partial/conflicting file remains a refusal.
    } // No raw file deletion/retry is hidden here.
    private string Resolve(string name) // Internal names and persisted refs obey the same bounded subtree rule.
    { // Treat a name as data, never as a shell expression.
        if (string.IsNullOrWhiteSpace(name) || name.Contains('\\') || name.Contains(':') || name.Split('/').Any(s => s.Length == 0 || s is "." or ".." || s.Any(char.IsControl))) throw new IOException(); // Reject escaping or ambiguous paths.
        return Path.Combine(_root, name.Replace('/', Path.DirectorySeparatorChar)); // All accepted names are relative to this private root.
    } // Caller-facing methods convert invalid names to typed refusals.
    private bool Custody(string path) => !SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) && SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(Path.GetDirectoryName(path)!) && SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path); // Refuse changed namespace or broad private-file modes.
    private static bool Duplicates(JsonElement element) => element.ValueKind == JsonValueKind.Object ? element.EnumerateObject().GroupBy(p => p.Name, StringComparer.Ordinal).Any(g => g.Count() != 1) || element.EnumerateObject().Any(p => Duplicates(p.Value)) : element.ValueKind == JsonValueKind.Array && element.EnumerateArray().Any(Duplicates); // Reject duplicate bindings at any depth.
    internal static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(); // Stable byte digest shared by the private scientific owners.
    private static OperationResult<T> Fail<T>() => OperationResult<T>.Fail("Private evaluation artifact custody, bytes or binding changed.", Invalid); // Keep errors independent of protected material.
} // The store is for evaluation retention only; no training-data export path is provided.
