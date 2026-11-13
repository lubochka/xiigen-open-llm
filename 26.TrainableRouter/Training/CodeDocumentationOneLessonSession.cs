// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using Tokenizer;

/// <summary>
/// Immutable inputs that identify one CodeDocumentation lesson. The objective
/// ID is the lookup key; the remaining fields are frozen on first creation and
/// must match on every reopen.
/// </summary>
internal sealed record CodeDocumentationOneLessonSessionBinding(
    string ObjectiveId,
    string GrantId,
    string GrantVersion,
    string TrainingRunId,
    string CandidateSha256,
    string ParentManifestSha256,
    string ParentWeightsSha256,
    string ConfigurationSha256,
    string ExamConditionSha256)
{
    [JsonIgnore] internal int FormatVersion { get; init; } = 1;
    [JsonIgnore] internal CodeDocumentationOneLessonDescriptorsV2? DescriptorsV2 { get; init; }
}

internal sealed record CodeDocumentationOneLessonContinuationCapacityV2(
    int SlotCount, int MaximumFrames, long MaximumBytes, int PrefixFrames, long PrefixBytes)
{
    internal int RemainingFrames => MaximumFrames - PrefixFrames;
    internal long RemainingBytes => MaximumBytes - PrefixBytes;
}

/// <summary>
/// Monotonic counts for effects owned by lower-level services. This session
/// records accounting only; their receipts remain the authority for effect data.
/// </summary>
internal sealed record CodeDocumentationOneLessonEffectAccounting(
    long BaselineDispatched = 0,
    long BaselineCompleted = 0,
    long TeachingDispatched = 0,
    long TeachingCompleted = 0,
    long EvaluatorDispatched = 0,
    long EvaluatorCompleted = 0,
    long OptimizerEntries = 0,
    bool EffectOutcomeUnknown = false);

internal enum CodeDocumentationOneLessonTerminalStateV1
{
    TERMINAL_NEGATIVE_PARENT_RETAINED,
    TERMINAL_PROMOTED,
}

// Marker types keep references category-specific without duplicating receipt
// payloads in the session history.
internal sealed class CodeDocumentationOneLessonTrialEvaluationReceiptV1 { }
internal sealed class CodeDocumentationOneLessonQualityDecisionReceiptV1 { }
internal sealed class CodeDocumentationOneLessonDispositionReceiptV1 { }
internal sealed class CodeDocumentationOneLessonPublicationReceiptV1 { }
internal sealed class CodeDocumentationOneLessonFinalReopenReceiptV1 { }

internal sealed record CodeDocumentationOneLessonTypedReceiptRefV1<TReceipt>(
    string ReceiptId,
    string ReceiptSha256);

/// <summary>
/// Typed references for the later trusted receipt-verification path to pass to
/// the session owner. Constructing this internal envelope does not itself prove
/// or verify the referenced receipt bytes; no receipt verifier is added here.
/// </summary>
internal sealed record CodeDocumentationOneLessonVerifiedTerminalReceiptRefsV1(
    string ObjectiveId,
    string SessionId,
    string TrainingRunId,
    string BindingSha256,
    CodeDocumentationOneLessonTypedReceiptRefV1<CodeDocumentationOneLessonTrialEvaluationReceiptV1> TrialEvaluationReceipt,
    CodeDocumentationOneLessonTypedReceiptRefV1<CodeDocumentationOneLessonQualityDecisionReceiptV1> QualityDecisionReceipt,
    CodeDocumentationOneLessonTypedReceiptRefV1<CodeDocumentationOneLessonDispositionReceiptV1> DispositionReceipt,
    CodeDocumentationOneLessonTypedReceiptRefV1<CodeDocumentationOneLessonPublicationReceiptV1>? PublicationReceipt,
    CodeDocumentationOneLessonTypedReceiptRefV1<CodeDocumentationOneLessonFinalReopenReceiptV1> FinalReopenReceipt);

internal sealed record CodeDocumentationOneLessonCanonicalOutcomeV1(
    string OutcomeId,
    int SchemaVersion,
    string ObjectiveId,
    string SessionId,
    string TrainingRunId,
    string BindingSha256,
    CodeDocumentationOneLessonTerminalStateV1 TerminalState,
    CodeDocumentationOneLessonTypedReceiptRefV1<CodeDocumentationOneLessonTrialEvaluationReceiptV1> TrialEvaluationReceipt,
    CodeDocumentationOneLessonTypedReceiptRefV1<CodeDocumentationOneLessonQualityDecisionReceiptV1> QualityDecisionReceipt,
    CodeDocumentationOneLessonTypedReceiptRefV1<CodeDocumentationOneLessonDispositionReceiptV1> DispositionReceipt,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    CodeDocumentationOneLessonTypedReceiptRefV1<CodeDocumentationOneLessonPublicationReceiptV1>? PublicationReceipt,
    CodeDocumentationOneLessonTypedReceiptRefV1<CodeDocumentationOneLessonFinalReopenReceiptV1> FinalReopenReceipt);

internal sealed record CodeDocumentationOneLessonSessionSnapshot(
    string SessionId,
    string HistoryRelativePath,
    string HistorySha256,
    CodeDocumentationOneLessonSessionBinding Binding,
    long Revision,
    CodeDocumentationOneLessonEffectAccounting Effects,
    CodeDocumentationOneLessonCanonicalOutcomeV1? TerminalOutcome = null,
    string? TerminalOutcomeSha256 = null);

internal sealed record CodeDocumentationOneLessonCanonicalOutcomeReadbackV1(
    JsonElement CanonicalOutcome,
    string OutcomeSha256,
    long Revision);

internal sealed record CodeDocumentationOneLessonCanonicalOutcomeReadbackV2(
    JsonElement CanonicalOutcome,
    string OutcomeSha256,
    long Revision);

internal sealed record CodeDocumentationOneLessonSessionOpen(
    CodeDocumentationOneLessonSession Session,
    bool Created) : IDisposable
{
    public void Dispose() => Session.Dispose();
}

/// <summary>
/// Small repository-owned objective record. It serializes writers with an
/// owner-only lock, stores immutable revision frames, and rejects stale or
/// decreasing transitions. Model, optimizer, pair, and evaluation bytes remain
/// owned by their existing lower-level stores.
/// </summary>
internal sealed class CodeDocumentationOneLessonSession : IDisposable
{
    internal const string ErrorPrefix = "CODE_DOCUMENTATION_ONE_LESSON_SESSION_";
    internal const string ObjectiveConflictErrorCode = ErrorPrefix + "OBJECTIVE_CONFLICT";
    internal const string BusyErrorCode = ErrorPrefix + "BUSY";
    internal const string StaleRevisionErrorCode = ErrorPrefix + "STALE_REVISION";
    internal const string NonMonotonicErrorCode = ErrorPrefix + "NON_MONOTONIC";
    internal const string RecoveryRequiredErrorCode = ErrorPrefix + "RECOVERY_REQUIRED";
    internal const string InvalidErrorCode = ErrorPrefix + "INVALID";
    internal const string UnavailableErrorCode = ErrorPrefix + "UNAVAILABLE";
    internal const string OptimizerBudgetExceededErrorCode = ErrorPrefix + "OPTIMIZER_BUDGET_EXCEEDED";
    internal const string TerminalConflictErrorCode = ErrorPrefix + "TERMINAL_CONFLICT";
    internal const string CapacityInvalidErrorCode = ErrorPrefix + "CAPACITY_INVALID";
    internal const string CapacityExceededErrorCode = ErrorPrefix + "CAPACITY_EXCEEDED";
    internal const string BaselinePrerequisiteUnavailableErrorCode = ErrorPrefix + "BASELINE_PREREQUISITE_UNAVAILABLE";
    internal const string UsableDiagnosisMissingErrorCode = ErrorPrefix + "USABLE_DIAGNOSIS_MISSING";

    private const int CurrentFormatVersion = 1;
    private const int MaximumObjectiveIdBytes = 512;
    private const int MaximumHistoryFrames = 256;
    private const long MaximumHistoryBytes = 2L * 1024L * 1024L;
    private const string SessionsDirectoryName = "code-documentation-one-lesson-sessions";
    private const string HistoryFileName = "session.jsonl";
    private const string LockFileName = ".owner.lock";

    private static readonly UTF8Encoding Utf8 = new(false, true);
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter<CodeDocumentationOneLessonTerminalStateV1>(allowIntegerValues: false) },
    };

    private readonly string _historyPath;
    private readonly string _historyRelativePath;
    private readonly string _lockPath;
    private readonly string _sessionId;
    private readonly FileStream _ownerLock;
    private FileStream? _v2History;
    private FileStream? _nativePreparedRequest, _nativePreparedSchema;
    private byte[]? _nativePreparedRequestBytes, _nativePreparedSchemaBytes;
    private bool _nativePreparedPoisoned, _nativePreparationIssuing;
    private string? _nativePreparationNonce;
    private NativeSelectionPreparationReceipt? _nativePreparationReceipt;
    private NativeSelectionLaunchBindingV1? _nativeLaunchBinding; // A transient same-operation capability, never serialized/imported.
    private CodeDocumentationNativeCodexSelectionHost? _nativeLaunchHost; // Bind the exact fixed consumer object.
    private CodeDocumentationNativeCodexLaunchInputOwner? _nativeLaunchInputs; // Keep actual launch-input handles until final checks and invalidation complete.
    private string? _syntheticNativeLaunchDirectory; // Only the explicit internal synthetic fixture wrapper sets this under the held CAS.
    private bool _nativeLaunchIssuing, _nativeStartedAttempt; // Short private issuance and monotonic no-relaunch reservation are distinct.
    private FileStream? _nativeStartedFile; // Hold unknown/partial attempt evidence instead of deleting it.
    private byte[]? _nativeStartedBytes; // Advance only after full durable stable readback.
    internal (int ReservedAttempts, int CompletedAttempts, bool OutcomeUnknown) NativeStartedAccounting // Retain monotonic unknown native-attempt accounting.
        => (_nativeStartedAttempt ? 1 : 0, 0, _nativeStartedAttempt); // This increment has no completion or genuine process execution.
    private Exception? _nativePreparedFamilyFinalFailure;
    internal string? NativePreparedFamilyFinalErrorForTesting { get; private set; }
    private byte[]? _v2Prefix;
    private CodeDocumentationOneLessonContinuationCapacityV2? _capacityV2;
    private CodeDocumentationOneLessonGrantV1? _earlyGrant;
    private SyntheticStudentLeaseIntent? _studentLease; // Fix Synthetic response capacity at first open; decoded markers cannot create this intent.
    private FullMaterialStorageFamilyIntent? _fullMaterialLease; // The genuine catalog fixes this family before header parsing.
    private CodeDocumentationFullMaterialPrefixV1? _fullMaterialCompleted;
    private bool _fullMaterialPoisoned, _fullMaterialIssuing;
    private byte[]? _fullMaterialExpectedPrefix; // Advance only from confirmed own append readback, never late runtime fields.
    private MaterialSelectionDispatchReceipt? _fullMaterialDispatch;
    private readonly AsyncLocal<CodeDocumentationOneLessonSession?> _fullMaterialCheckOwner = new(); // Pending-prefix access is confined to finite checks, never the callback.
    internal (int ReservedInvocations, int CompletedInvocations, bool OutcomeUnknown) FullMaterialSelectionAccounting
        => _fullMaterialLease is null ? (0, 0, false) : (_fullMaterialPoisoned || (_capacityV2?.PrefixFrames ?? 0) >= 2 ? 1 : 0,
            _capacityV2?.PrefixFrames == 3 ? 1 : 0, _fullMaterialPoisoned && _capacityV2?.PrefixFrames != 3);
    internal Action<string,int,int,int,int>? FullMaterialCapacityReservedForTesting { get; set; } // Observe each actual maximum serialized candidate reservation before any history/callback effect.
    internal string? FullMaterialInvocationIdForTesting { get; set; } // Deterministic Synthetic wire-boundary vectors still require the private lease, actual Started and concrete nonce/issuer; the string grants no authority.
    private int _studentResponseInFlight; // Serialize the single isolated response and refuse concurrent or reentrant dispatch.
    private CodeDocumentationRepositoryOwnedPreparedBaselineJournal.SyntheticSelectedTaskStudentOperation? _activeSelectedStudentOperation; // Keep transient resources private to this dispatcher; recipients receive no model or operation.
    private bool _studentAttemptStarted, _studentPoisoned; // Preserve a consumed/ambiguous response attempt without releasing its slot.
    private bool _studentIssuing; // Confine readback minting to the checked dispatcher point, not a concurrent caller observing an in-flight flag.
    private bool _teacherContextIssuing; // Confine Synthetic history issuance to this held completed-only operation, never a caller DTO.
    private readonly AsyncLocal<CodeDocumentationOneLessonSession?> _studentCheckOwner = new(); // Allow only this logical operation's finite zero-effect owner checks while Started is held.
    internal Action<string>? SelectedStudentDurablePointForTesting { get; set; } // Observe actual durable boundaries without replacing inference or importing a permit.
    internal (int ReservedGenerationSlots, int CompletedGenerationSlots, bool OutcomeUnknown) SelectedStudentAccounting // Keep isolated response accounting distinct from frozen evaluator counters.
        => _studentLease is null ? (0, 0, false) : (_studentAttemptStarted || (_capacityV2?.PrefixFrames ?? 0) >= 3 ? 1 : 0, // Count conservative reservations independently from actual Generate dispatch.
            _capacityV2?.PrefixFrames == 4 ? 1 : 0, (_studentAttemptStarted || (_capacityV2?.PrefixFrames ?? 0) >= 3) && _capacityV2?.PrefixFrames != 4); // Completed replay retains one slot and never creates another Generate allowance.
    private CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline? _ordinaryBaseline;
    private byte[]? _ordinaryBaselineBytes;
    private byte[]? _ordinaryTokenizerBytes;
    private string[]? _ordinaryEvidence;
    private CodeDocumentationRepositoryOwnedModelCheckpointStartRequest? _ordinaryStart;
    private byte[]? _ordinaryStartBytes;
    private byte[]? _ordinaryContextBytes;
    private byte[]? _ordinaryReferenceBytes;
    private byte[]? _ordinaryGrantBytes;
    private byte[]? _ordinaryBindingBytes;
    private ILegalTeacherEvaluationInferenceClient? _ordinaryEvaluationClient;
    private CodeDocumentationTeachingSelectionFrameV1? _selectionFrame; // Retain selection state on this held owner; attempts and test fault observations issue no external authority.
    private SelectionInFlight? _selectionInFlight; // Retain selection state on this held owner; attempts and test fault observations issue no external authority.
    private bool _selectionAttemptStarted; // Retain selection state on this held owner; attempts and test fault observations issue no external authority.
    internal Action<string>? SelectionDurablePointForTesting { get; set; } // Retain selection state on this held owner; attempts and test fault observations issue no external authority.
    internal CodeDocumentationOneLessonCanonicalOutcomeReadbackV2? EarlyOutcome { get; private set; }
    private CodeDocumentationOneLessonSessionSnapshot _snapshot;
    private bool _disposed;

    private CodeDocumentationOneLessonSession(
        string historyPath,
        string historyRelativePath,
        string lockPath,
        string sessionId,
        FileStream ownerLock,
        CodeDocumentationOneLessonSessionSnapshot snapshot,
        FileStream? v2History = null,
        byte[]? v2Prefix = null,
        CodeDocumentationOneLessonContinuationCapacityV2? capacityV2 = null)
    {
        _historyPath = historyPath;
        _historyRelativePath = historyRelativePath;
        _lockPath = lockPath;
        _sessionId = sessionId;
        _ownerLock = ownerLock;
        _snapshot = snapshot;
        _v2History = v2History;
        _v2Prefix = v2Prefix;
        _capacityV2 = capacityV2;
    }

    internal CodeDocumentationOneLessonSessionSnapshot Snapshot => _snapshot;
    internal CodeDocumentationOneLessonContinuationCapacityV2? CapacityV2 => _capacityV2;

    internal static string ComputeBindingSha256(
        CodeDocumentationOneLessonSessionBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);
        if (binding.FormatVersion == 2)
            return Sha256(Utf8.GetBytes("code-documentation-one-lesson-binding-v2\n")
                .Concat(BindingBytesV2(binding)).ToArray());
        var canonical = "code-documentation-one-lesson-binding-v1\n" +
            JsonSerializer.Serialize(binding, Json);
        return Convert.ToHexString(SHA256.HashData(Utf8.GetBytes(canonical)))
            .ToLowerInvariant();
    }

    internal static OperationResult<CodeDocumentationOneLessonSessionOpen> OpenOrCreate(
        RepositoryOwnedModelArtifactRoot? modelArtifactRoot,
        CodeDocumentationOneLessonSessionBinding? binding,
        int maxTeacherCalls = 0)
    {
        if (binding?.FormatVersion == 2)
        {
            var local = OpenCapacityValidationV2(modelArtifactRoot, binding, maxTeacherCalls);
            if (!local.IsSuccess || local.Value is null)
                return local;
            using var validation = local.Value;
            // This increment exports no baseline/receipt capability. A local
            // capacity lease is not authority to write Genesis or invoke effects.
            return Failure<CodeDocumentationOneLessonSessionOpen>(
                BaselinePrerequisiteUnavailableErrorCode,
                "Local V2 capacity is valid; the trusted ORIGINAL S04.03A baseline producer/readback is unavailable.");
        }
        if (binding is not null && (binding.FormatVersion != 1 || binding.DescriptorsV2 is not null))
            return Invalid<CodeDocumentationOneLessonSessionOpen>();
        if (!ValidRoot(modelArtifactRoot) || !ValidBinding(binding))
            return Invalid<CodeDocumentationOneLessonSessionOpen>();

        FileStream? ownerLock = null;
        try
        {
            var root = modelArtifactRoot!;
            var currentRoot = RepositoryOwnedTrainingArtifactLayout
                .ResolveModelArtifactRoot(root.RepositoryRoot);
            if (!currentRoot.IsSuccess || currentRoot.Value is null ||
                !SamePath(currentRoot.Value.ModelArtifactRoot, root.ModelArtifactRoot))
            {
                return Invalid<CodeDocumentationOneLessonSessionOpen>();
            }

            var sessionId = Sha256(binding!.ObjectiveId);
            var sessionsRoot = Path.GetFullPath(Path.Combine(
                currentRoot.Value.ModelArtifactRoot,
                SessionsDirectoryName));
            var sessionDirectory = Path.GetFullPath(Path.Combine(sessionsRoot, sessionId));
            var historyPath = Path.Combine(sessionDirectory, HistoryFileName);
            var lockPath = Path.GetFullPath(Path.Combine(sessionDirectory, LockFileName));
            var relativePath = Path.GetRelativePath(root.RepositoryRoot, historyPath)
                .Replace(Path.DirectorySeparatorChar, '/');

            if (!Inside(currentRoot.Value.ModelArtifactRoot, sessionsRoot) ||
                !Inside(sessionsRoot, sessionDirectory) ||
                SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(sessionsRoot) ||
                SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(sessionDirectory))
            {
                return Invalid<CodeDocumentationOneLessonSessionOpen>();
            }

            EnsurePrivateDirectory(sessionsRoot);
            var sessionExisted = Directory.Exists(sessionDirectory);
            EnsurePrivateDirectory(sessionDirectory);

            try
            {
                ownerLock = OpenOwnerLock(lockPath);
            }
            catch (IOException exception) when (IsLockContention(exception))
            {
                return Failure<CodeDocumentationOneLessonSessionOpen>(
                    BusyErrorCode,
                    "Another process currently owns this one-lesson objective.");
            }

            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(ownerLock.SafeFileHandle, lockPath))
                return Unavailable<CodeDocumentationOneLessonSessionOpen>();

            var lockBytes = Utf8.GetBytes("s04-one-lesson-session-v1\n" + sessionId + "\n");
            if (ownerLock.Length == 0)
            {
                if (sessionExisted)
                    return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>();
                ownerLock.Write(lockBytes);
                ownerLock.Flush(flushToDisk: true);
            }
            else if (!ReadStream(ownerLock, 1024).AsSpan().SequenceEqual(lockBytes))
            {
                return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>();
            }

            var hasHistory = File.Exists(historyPath);
            if (!sessionExisted && !hasHistory)
            {
                var genesis = new SessionFrame(
                    "session",
                    CurrentFormatVersion,
                    sessionId,
                    binding,
                    0,
                    "",
                    new CodeDocumentationOneLessonEffectAccounting(),
                    null);
                var bytes = FrameBytes(genesis);
                var published = PublishNewHistory(historyPath, bytes);
                if (!published)
                    return Unavailable<CodeDocumentationOneLessonSessionOpen>();

                var createdSnapshot = new CodeDocumentationOneLessonSessionSnapshot(
                    sessionId,
                    relativePath,
                    Sha256(bytes.Concat(new byte[] { (byte)'\n' }).ToArray()),
                    binding,
                    0,
                    genesis.Effects,
                    null,
                    null);
                var created = new CodeDocumentationOneLessonSession(
                    historyPath,
                    relativePath,
                    lockPath,
                    sessionId,
                    ownerLock,
                    createdSnapshot);
                ownerLock = null;
                return OperationResult<CodeDocumentationOneLessonSessionOpen>.Ok(
                    new(created, Created: true));
            }

            if (!hasHistory)
                return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>();

            var entries = Directory.GetFileSystemEntries(sessionDirectory);
            if (entries.Length != 2 ||
                entries.Any(path => Directory.Exists(path)) ||
                !entries.Any(path => SamePath(path, historyPath)) ||
                !entries.Any(path => SamePath(path, lockPath)))
            {
                return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>();
            }

            var history = ReadHistory(historyPath, sessionId);
            if (!history.IsSuccess || history.Value is null)
                return history.MapError<CodeDocumentationOneLessonSessionOpen>();

            if (history.Value.Binding != binding)
            {
                return Failure<CodeDocumentationOneLessonSessionOpen>(
                    ObjectiveConflictErrorCode,
                    "The objective already exists with different immutable lesson inputs.");
            }

            var reopened = new CodeDocumentationOneLessonSession(
                historyPath,
                relativePath,
                lockPath,
                sessionId,
                ownerLock,
                new CodeDocumentationOneLessonSessionSnapshot(
                    sessionId,
                    relativePath,
                    history.Value.HistorySha256,
                    history.Value.Binding,
                    history.Value.Revision,
                    history.Value.Effects,
                    history.Value.TerminalOutcome,
                    history.Value.TerminalOutcomeSha256));
            ownerLock = null;
            return OperationResult<CodeDocumentationOneLessonSessionOpen>.Ok(
                new(reopened, Created: false));
        }
        catch (JsonException)
        {
            return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<CodeDocumentationOneLessonSessionOpen>();
        }
        finally
        {
            ownerLock?.Dispose();
        }
    }

    internal OperationResult<CodeDocumentationOneLessonSessionSnapshot> Advance(
        long expectedRevision,
        CodeDocumentationOneLessonEffectAccounting? nextEffects)
    {
        if (_capacityV2 is not null)
            return RecoveryRequired<CodeDocumentationOneLessonSessionSnapshot>();
        if (_disposed || nextEffects is null || !ValidEffects(nextEffects))
            return Invalid<CodeDocumentationOneLessonSessionSnapshot>();
        if (_snapshot.TerminalOutcome is not null)
            return RecoveryRequired<CodeDocumentationOneLessonSessionSnapshot>();
        if (expectedRevision != _snapshot.Revision)
            return Failure<CodeDocumentationOneLessonSessionSnapshot>(
                StaleRevisionErrorCode,
                "The session revision changed before this transition.");
        if (_snapshot.Revision + 1 >= MaximumHistoryFrames)
            return RecoveryRequired<CodeDocumentationOneLessonSessionSnapshot>();
        if (!Monotonic(_snapshot.Effects, nextEffects))
            return Failure<CodeDocumentationOneLessonSessionSnapshot>(
                NonMonotonicErrorCode,
                "Effect accounting cannot decrease or clear an unknown outcome.");
        if (nextEffects.OptimizerEntries > 1)
            return Failure<CodeDocumentationOneLessonSessionSnapshot>(
                OptimizerBudgetExceededErrorCode,
                "One objective cannot record more than one optimizer entry.");

        try
        {
            var expectedLockBytes = Utf8.GetBytes(
                "s04-one-lesson-session-v1\n" + _sessionId + "\n");
            if (!ReadStream(_ownerLock, 1024).AsSpan().SequenceEqual(expectedLockBytes))
                return RecoveryRequired<CodeDocumentationOneLessonSessionSnapshot>();
            var history = ReadHistory(_historyPath, _sessionId);
            if (!history.IsSuccess || history.Value is null)
                return history.MapError<CodeDocumentationOneLessonSessionSnapshot>();
            if (history.Value.TerminalOutcome is not null)
                return RecoveryRequired<CodeDocumentationOneLessonSessionSnapshot>();
            if (history.Value.Revision != expectedRevision ||
                !StringComparer.Ordinal.Equals(
                    history.Value.HistorySha256,
                    _snapshot.HistorySha256))
            {
                return Failure<CodeDocumentationOneLessonSessionSnapshot>(
                    StaleRevisionErrorCode,
                    "The durable session advanced after this snapshot was read.");
            }

            var frame = new SessionFrame(
                "transition",
                CurrentFormatVersion,
                _sessionId,
                _snapshot.Binding,
                expectedRevision + 1,
                history.Value.TipSha256,
                nextEffects,
                null);
            var frameBytes = FrameBytes(frame);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    _ownerLock.SafeFileHandle,
                    _lockPath))
            {
                return RecoveryRequired<CodeDocumentationOneLessonSessionSnapshot>();
            }
            var appended = AppendFrame(_historyPath, frameBytes, history.Value.HistoryBytes);
            if (!appended.IsSuccess || appended.Value is null)
                return appended.MapError<CodeDocumentationOneLessonSessionSnapshot>();

            var updated = new CodeDocumentationOneLessonSessionSnapshot(
                _sessionId,
                _historyRelativePath,
                appended.Value.HistorySha256,
                _snapshot.Binding,
                frame.Revision,
                nextEffects,
                null,
                null);
            _snapshot = updated;
            return OperationResult<CodeDocumentationOneLessonSessionSnapshot>.Ok(updated);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<CodeDocumentationOneLessonSessionSnapshot>();
        }
    }

    internal OperationResult<CodeDocumentationOneLessonSessionSnapshot> RecordTerminalOutcome(
        long expectedRevision,
        CodeDocumentationOneLessonVerifiedTerminalReceiptRefsV1? verifiedReceiptRefs)
    {
        if (_capacityV2 is not null)
            return RecoveryRequired<CodeDocumentationOneLessonSessionSnapshot>();
        if (_disposed ||
            verifiedReceiptRefs is null ||
            !ValidVerifiedTerminalReceiptRefs(verifiedReceiptRefs) ||
            !ReceiptReferencesMatchBinding(verifiedReceiptRefs, _snapshot.Binding, _sessionId))
            return Invalid<CodeDocumentationOneLessonSessionSnapshot>();

        try
        {
            var expectedLockBytes = Utf8.GetBytes(
                "s04-one-lesson-session-v1\n" + _sessionId + "\n");
            if (!ReadStream(_ownerLock, 1024).AsSpan().SequenceEqual(expectedLockBytes) ||
                !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    _ownerLock.SafeFileHandle,
                    _lockPath))
            {
                return RecoveryRequired<CodeDocumentationOneLessonSessionSnapshot>();
            }

            var history = ReadHistory(_historyPath, _sessionId);
            if (!history.IsSuccess || history.Value is null)
                return history.MapError<CodeDocumentationOneLessonSessionSnapshot>();

            // A false uncertainty bit cannot close an effect whose dispatch
            // has no matching durable completion receipt.
            if (HasUnresolvedEffects(history.Value.Effects))
                return RecoveryRequired<CodeDocumentationOneLessonSessionSnapshot>();

            if (history.Value.TerminalOutcome is not null)
            {
                if (!SameReceiptReferences(history.Value.TerminalOutcome, verifiedReceiptRefs))
                    return Failure<CodeDocumentationOneLessonSessionSnapshot>(
                        TerminalConflictErrorCode,
                        "A different terminal outcome is already durable for this objective.");
                if (expectedRevision != history.Value.Revision &&
                    expectedRevision != history.Value.Revision - 1)
                {
                    return Failure<CodeDocumentationOneLessonSessionSnapshot>(
                        StaleRevisionErrorCode,
                        "The terminal outcome was committed at a different session revision.");
                }

                var existing = SnapshotFromHistory(history.Value);
                _snapshot = existing;
                return OperationResult<CodeDocumentationOneLessonSessionSnapshot>.Ok(existing);
            }

            if (_snapshot.TerminalOutcome is not null)
                return RecoveryRequired<CodeDocumentationOneLessonSessionSnapshot>();
            if (expectedRevision != _snapshot.Revision ||
                history.Value.Revision != expectedRevision ||
                !StringComparer.Ordinal.Equals(
                    history.Value.HistorySha256,
                    _snapshot.HistorySha256))
            {
                return Failure<CodeDocumentationOneLessonSessionSnapshot>(
                    StaleRevisionErrorCode,
                    "The durable session advanced after this snapshot was read.");
            }
            if (HasUnresolvedEffects(_snapshot.Effects) ||
                expectedRevision + 1 >= MaximumHistoryFrames)
            {
                return RecoveryRequired<CodeDocumentationOneLessonSessionSnapshot>();
            }

            // A later trusted receipt verifier must supply this typed reference
            // set. The owner supplies identity and derives terminal state from
            // publication-reference presence; it never accepts a caller-authored
            // canonical outcome or state value.
            var terminalState = verifiedReceiptRefs.PublicationReceipt is null
                ? CodeDocumentationOneLessonTerminalStateV1.TERMINAL_NEGATIVE_PARENT_RETAINED
                : CodeDocumentationOneLessonTerminalStateV1.TERMINAL_PROMOTED;
            var outcome = new CodeDocumentationOneLessonCanonicalOutcomeV1(
                Guid.NewGuid().ToString("N"),
                1,
                _snapshot.Binding.ObjectiveId,
                _sessionId,
                _snapshot.Binding.TrainingRunId,
                ComputeBindingSha256(_snapshot.Binding),
                terminalState,
                verifiedReceiptRefs.TrialEvaluationReceipt,
                verifiedReceiptRefs.QualityDecisionReceipt,
                verifiedReceiptRefs.DispositionReceipt,
                verifiedReceiptRefs.PublicationReceipt,
                verifiedReceiptRefs.FinalReopenReceipt);
            var frame = new SessionFrame(
                "terminal",
                CurrentFormatVersion,
                _sessionId,
                _snapshot.Binding,
                expectedRevision + 1,
                history.Value.TipSha256,
                _snapshot.Effects,
                outcome);
            var frameBytes = FrameBytes(frame);
            var appended = AppendFrame(_historyPath, frameBytes, history.Value.HistoryBytes);
            if (!appended.IsSuccess || appended.Value is null)
                return appended.MapError<CodeDocumentationOneLessonSessionSnapshot>();

            var updated = SnapshotFromHistory(appended.Value);
            _snapshot = updated;
            return OperationResult<CodeDocumentationOneLessonSessionSnapshot>.Ok(updated);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<CodeDocumentationOneLessonSessionSnapshot>();
        }
    }

    /// <summary>
    /// Reopens the same owner-controlled history and reads back the exact final
    /// terminal frame. This frame contains only receipt IDs and hashes; this
    /// owner currently has no admitted server-owned resolver for the referenced
    /// artifact bytes. Therefore structural/frame-hash validity alone cannot
    /// authorize replay, and the read fails closed until that verifier exists.
    /// </summary>
    internal OperationResult<CodeDocumentationOneLessonCanonicalOutcomeReadbackV1>
        ReadCanonicalOutcome(
            CodeDocumentationOneLessonSelectorV1? selector,
            string? bindingSha256,
            string? outcomeId,
            string? outcomeSha256)
    {
        if (_disposed ||
            !CodeDocumentationOneLessonSelectorV1.IsValidSelectorShape(selector) ||
            !Sha256Hex(bindingSha256) ||
            !Guid.TryParseExact(outcomeId, "N", out var outcomeGuid) ||
            !StringComparer.Ordinal.Equals(outcomeId, outcomeGuid.ToString("N")) ||
            !Sha256Hex(outcomeSha256))
        {
            return Invalid<CodeDocumentationOneLessonCanonicalOutcomeReadbackV1>();
        }

        if (!StringComparer.Ordinal.Equals(
                selector!.ObjectiveId,
                _snapshot.Binding.ObjectiveId) ||
            !StringComparer.Ordinal.Equals(
                bindingSha256,
                ComputeBindingSha256(_snapshot.Binding)))
        {
            return Failure<CodeDocumentationOneLessonCanonicalOutcomeReadbackV1>(
                ObjectiveConflictErrorCode,
                "The terminal replay selector or immutable binding differs from the owned objective.");
        }

        try
        {
            var expectedLockBytes = Utf8.GetBytes(
                "s04-one-lesson-session-v1\n" + _sessionId + "\n");
            if (!ReadStream(_ownerLock, 1024).AsSpan().SequenceEqual(expectedLockBytes) ||
                !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    _ownerLock.SafeFileHandle,
                    _lockPath))
            {
                return RecoveryRequired<CodeDocumentationOneLessonCanonicalOutcomeReadbackV1>();
            }

            var history = ReadHistory(_historyPath, _sessionId);
            if (!history.IsSuccess || history.Value is null ||
                history.Value.Binding != _snapshot.Binding ||
                history.Value.Revision != _snapshot.Revision ||
                !StringComparer.Ordinal.Equals(
                    history.Value.HistorySha256,
                    _snapshot.HistorySha256) ||
                HasUnresolvedEffects(history.Value.Effects) ||
                history.Value.TerminalOutcome is null ||
                history.Value.TerminalOutcomeSha256 is null)
            {
                return RecoveryRequired<CodeDocumentationOneLessonCanonicalOutcomeReadbackV1>();
            }

            var outcome = history.Value.TerminalOutcome;
            if (!ValidCanonicalOutcome(outcome, history.Value.Binding, _sessionId))
                return RecoveryRequired<CodeDocumentationOneLessonCanonicalOutcomeReadbackV1>();

            if (!StringComparer.Ordinal.Equals(outcome.OutcomeId, outcomeId) ||
                !StringComparer.Ordinal.Equals(
                    history.Value.TerminalOutcomeSha256,
                    outcomeSha256))
            {
                return Failure<CodeDocumentationOneLessonCanonicalOutcomeReadbackV1>(
                    TerminalConflictErrorCode,
                    "The requested terminal outcome does not match the final durable frame.");
            }

            // The active S04 contract requires reading and validating every
            // referenced receipt artifact, its canonical bytes/hash and its
            // binding/order/completeness links before replay. The typed refs
            // above contain no server-owned path mapping, and no production
            // receipt-chain reader is wired to this owner. Do not turn a
            // setup-created frame with plausible IDs/hashes into completion.
            return RecoveryRequired<CodeDocumentationOneLessonCanonicalOutcomeReadbackV1>();
        }
        catch (JsonException)
        {
            return RecoveryRequired<CodeDocumentationOneLessonCanonicalOutcomeReadbackV1>();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<CodeDocumentationOneLessonCanonicalOutcomeReadbackV1>();
        }
    }

    private CodeDocumentationOneLessonSessionSnapshot SnapshotFromHistory(HistoryReadback history)
        => new(
            _sessionId,
            _historyRelativePath,
            history.HistorySha256,
            history.Binding,
            history.Revision,
            history.Effects,
            history.TerminalOutcome,
            history.TerminalOutcomeSha256);

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _nativePreparationReceipt?.Invalidate();
        _nativeLaunchBinding?.Invalidate(); // End launch authority before any resource handle is released.
        // Each retained owner is released even if another stream reports a disposal failure.
        try { _nativeLaunchInputs?.Dispose(); } // Release the held launch-input owner even on an interrupted operation.
        finally { try { _nativeStartedFile?.Dispose(); } finally { try { _nativePreparedRequest?.Dispose(); } // End launch-input ownership and release every retained resource.
        finally { try { _nativePreparedSchema?.Dispose(); } finally { try { _v2History?.Dispose(); } finally { _ownerLock.Dispose(); } } } } } // Release every family/lock handle independently.
    }

    /// <summary>
    /// Structural capacity owner only, under the existing objective lock. It
    /// writes no Genesis, consumes no receipts and permits no effects. Reopening
    /// an unsupported receipt or an ambiguous Started prefix fails closed.
    /// </summary>
    private sealed class SyntheticStudentLeaseIntent // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
    { // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        private readonly CodeDocumentationOneLessonGrantV1 grant; // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        private readonly byte[] bindingBytes; // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        private SyntheticStudentLeaseIntent(CodeDocumentationOneLessonGrantV1 value, CodeDocumentationOneLessonSessionBinding binding) // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        { grant = value; bindingBytes = BindingBytesV2(binding); } // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        internal bool Matches(CodeDocumentationOneLessonSessionBinding binding, CodeDocumentationOneLessonGrantV1? current) // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
            => ReferenceEquals(grant, current) && bindingBytes.AsSpan().SequenceEqual(BindingBytesV2(binding)); // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        internal static SyntheticStudentLeaseIntent? Capture(CodeDocumentationOneLessonGrantCatalog catalog, // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
            CodeDocumentationOneLessonSelectorV1 selector, CodeDocumentationOneLessonSessionBinding binding) // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        { // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
            var resolved = catalog.Resolve(selector); // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
            if (!resolved.IsSuccess || resolved.Value is not { FormatVersion: 2 } value || value.SourceSelection?.TeachingCurriculum is null) return null; // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
            var actual = new CodeDocumentationOneLessonSessionBinding(value.ObjectiveId, value.GrantId, value.GrantVersion, value.TrainingRunId, // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
                value.CandidateSha256, value.ParentManifestSha256, value.ParentWeightsSha256, value.ConfigurationSha256, value.ExamConditionSha256) // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
                { FormatVersion = 2, DescriptorsV2 = value.DescriptorsV2 }; // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
            return binding.FormatVersion == 2 && BindingBytesV2(actual).AsSpan().SequenceEqual(BindingBytesV2(binding)) // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
                ? new(value, binding) : null; // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        } // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
    } // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
    internal static OperationResult<CodeDocumentationOneLessonSessionOpen> OpenSyntheticSelectedStudentCapacityForTesting( // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        RepositoryOwnedModelArtifactRoot root, CodeDocumentationOneLessonSessionBinding binding, // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector) // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
    { // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        if (catalog is null || selector is null || binding is null) return CapacityInvalid<CodeDocumentationOneLessonSessionOpen>(); // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        var intent = SyntheticStudentLeaseIntent.Capture(catalog, selector, binding); // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        if (intent is null) return CapacityInvalid<CodeDocumentationOneLessonSessionOpen>(); // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        return OpenCapacityValidationV2Core(root, binding, catalog.MaxTeacherCalls, catalog.Resolve(selector).Value, intent); // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
    } // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
    private static byte[] SyntheticStudentGenesisBytes(CodeDocumentationOneLessonSessionBinding binding, string session, // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        CodeDocumentationOneLessonGrantV1 grant) // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
    { // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        var genesis = EarlyFrames(binding, session, grant, new(new string('a', 32), new string('b', 64)))[0]; // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        var fields = genesis.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.Clone(), StringComparer.Ordinal); // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        fields.Add("StudentResponseLease", CodeDocumentationOneLessonV2Wire.SelectedStudentLeaseProjection()); // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
        return CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(fields)).Append((byte)'\n').ToArray(); // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.
    } // Mint only a private capacity intent from the genuine frozen catalog, preserving the old opener and Genesis reservation.

    private static bool V2PhysicalLinesWithin(byte[] bytes, int maximum) // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
    { // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        var start = 0; // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        for (var i = 0; i < bytes.Length; i++) // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        { // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
            if (i - start + 1 > maximum) return false; // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
            if (bytes[i] == (byte)'\n') start = i + 1; // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        } // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        return true; // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
    } // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
    private static byte[] V2BoundedFirstLine(byte[] bytes, int maximum) // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
    { // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        var end = Array.IndexOf(bytes, (byte)'\n'); // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        if (end < 0 || end + 1 > maximum) throw new JsonException("Unbounded or incomplete Genesis."); // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        return bytes.AsSpan(0, end).ToArray(); // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
    } // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
    private static byte[][]? SyntheticStudentPhysicalLines(byte[] bytes) // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
    { // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        if (bytes.Length == 0) return []; // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        if (bytes[^1] != (byte)'\n' || bytes.Contains((byte)'\r')) return null; // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        var lines = new List<byte[]>(); var start = 0; // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        for (var i = 0; i < bytes.Length; i++) // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        { // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
            var maximum = lines.Count switch { 0 => 2585, 1 => 32768, 2 => 136139, 3 => 396458, _ => 0 }; // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
            if (maximum == 0 || i - start + 1 > maximum) return null; // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
            if (bytes[i] != (byte)'\n') continue; // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
            lines.Add(bytes.AsSpan(start, i - start + 1).ToArray()); start = i + 1; // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        } // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        return start == bytes.Length ? lines.ToArray() : null; // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
    } // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
    private static OperationResult<CodeDocumentationTeachingSelectionFrameV1> DecodeSyntheticStudentChoice(byte[] bytes, // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        CodeDocumentationOneLessonSessionBinding binding, string session, CodeDocumentationOneLessonGrantV1 grant, // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        SyntheticStudentLeaseIntent intent) // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
    { // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        if (!intent.Matches(binding, grant)) return RecoveryRequired<CodeDocumentationTeachingSelectionFrameV1>(); // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        var lines = SyntheticStudentPhysicalLines(bytes); // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        if (lines is null || lines.Length < 2 || // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
            !lines[0].AsSpan().SequenceEqual(SyntheticStudentGenesisBytes(binding, session, grant))) // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
            return RecoveryRequired<CodeDocumentationTeachingSelectionFrameV1>(); // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        var decoded = CodeDocumentationOneLessonV2Wire.DecodeTeachingSelectionFrame(lines[1], SelectionProjection(binding)); // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        if (!decoded.IsSuccess || decoded.Value is null) return decoded; // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
        return decoded.Value.SessionId == session && decoded.Value.BindingSha256 == ComputeBindingSha256(binding) && // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
            decoded.Value.GenesisFrameSha256 == Sha256(lines[0].AsSpan(0, lines[0].Length - 1).ToArray()) && // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
            decoded.Value.Payload.SourceSha256 == grant.SourceSelection!.SourceSelectionSha256 // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
            ? decoded : RecoveryRequired<CodeDocumentationTeachingSelectionFrameV1>(); // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.
    } // Scan physical line sizes before UTF8/JSON conversion and authenticate bounded private Genesis and selection before any large parse.

    private static OperationResult<CodeDocumentationOneLessonContinuationCapacityV2> ValidateSyntheticStudentPrefix(byte[] bytes, // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
        CodeDocumentationOneLessonSessionBinding binding, string session, CodeDocumentationOneLessonContinuationCapacityV2 capacity, // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
        CodeDocumentationOneLessonGrantV1 grant, bool allowIncomplete, SyntheticStudentLeaseIntent intent) // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
    { // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
        if (!intent.Matches(binding, grant)) return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>(); // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
        if (bytes.LongLength > capacity.MaximumBytes) return CapacityExceeded<CodeDocumentationOneLessonContinuationCapacityV2>(); // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
        var lines = SyntheticStudentPhysicalLines(bytes); // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
        if (lines is null) return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>(); // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
        if (lines.Length == 0) return OperationResult<CodeDocumentationOneLessonContinuationCapacityV2>.Ok(capacity with { PrefixFrames = 0, PrefixBytes = 0 }); // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
        if (!lines[0].AsSpan().SequenceEqual(SyntheticStudentGenesisBytes(binding, session, grant))) // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
            return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>(); // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
        if (lines.Length == 1) return allowIncomplete // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
            ? OperationResult<CodeDocumentationOneLessonContinuationCapacityV2>.Ok(capacity with { PrefixFrames = 1, PrefixBytes = bytes.LongLength }) // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
            : RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>(); // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
        var choice = DecodeSyntheticStudentChoice(bytes, binding, session, grant, intent); // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
        if (!choice.IsSuccess || choice.Value is null) return choice.MapError<CodeDocumentationOneLessonContinuationCapacityV2>(); // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
        if (lines.Length >= 3) // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
        { // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
            var started = CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentStarted(lines[2], SelectionProjection(binding)); // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
            if (!started.IsSuccess || started.Value?.Request is not { } request || // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
                started.Value.SessionId != session || request.SelectionFrameSha256 != Sha256(lines[1].AsSpan(0, lines[1].Length - 1).ToArray()) || // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
                request.OriginalReceiptId != choice.Value.Payload.OriginalReceiptId || // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
                request.OriginalReceiptSha256 != choice.Value.Payload.OriginalReceiptSha256 || // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
                request.SourceSha256 != choice.Value.Payload.SourceSha256 || request.FrozenContextSha256 != choice.Value.Payload.FrozenContextSha256 || // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
                request.MaterialAdmissionEvidenceSha256 != choice.Value.Payload.MaterialAdmissionEvidenceSha256 || // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
                request.ItemId != choice.Value.Payload.ItemId || request.SourceId != choice.Value.Payload.SourceId || // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
                request.ReferenceEntryId != choice.Value.Payload.ReferenceEntryId || request.SourceEntityFingerprint != choice.Value.Payload.SourceEntityFingerprint || // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
                request.ParentManifestSha256 != binding.ParentManifestSha256 || request.ParentWeightsSha256 != binding.ParentWeightsSha256 || // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
                request.ConfigurationSha256 != binding.ConfigurationSha256) // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
                return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>(); // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
            if (lines.Length == 3 && !allowIncomplete) return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>(); // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
            if (lines.Length == 4) // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
            { // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
                var completed = CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentCompleted(lines[3], SelectionProjection(binding), // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
                    request, Sha256(lines[2].AsSpan(0, lines[2].Length - 1).ToArray())); // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
                if (!completed.IsSuccess || completed.Value?.SessionId != session) // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
                    return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>(); // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
            } // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
        } // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
        return OperationResult<CodeDocumentationOneLessonContinuationCapacityV2>.Ok(capacity with { PrefixFrames = lines.Length, PrefixBytes = bytes.LongLength }); // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.
    } // Allow only the explicitly authorized four-position response grammar and refuse reopened Started without completion.

    internal static OperationResult<CodeDocumentationOneLessonSessionOpen> OpenCapacityValidationV2(
        RepositoryOwnedModelArtifactRoot? root,
        CodeDocumentationOneLessonSessionBinding? binding,
        int maxTeacherCalls,
        CodeDocumentationOneLessonGrantV1? earlyGrant = null)
        => OpenCapacityValidationV2Core(root, binding, maxTeacherCalls, earlyGrant, null); // Preserve exact default/generic formulas and never infer private mode from history syntax.

    private sealed class FullMaterialStorageFamilyIntent
    {
        private readonly CodeDocumentationOneLessonGrantV1 grant;
        private readonly byte[] binding;
        private FullMaterialStorageFamilyIntent(CodeDocumentationOneLessonGrantV1 current, CodeDocumentationOneLessonSessionBinding value)
        { grant = current; binding = BindingBytesV2(value); }
        internal bool Matches(CodeDocumentationOneLessonSessionBinding value, CodeDocumentationOneLessonGrantV1? current)
            => ReferenceEquals(grant, current) && binding.AsSpan().SequenceEqual(BindingBytesV2(value));
        internal static FullMaterialStorageFamilyIntent? Capture(CodeDocumentationOneLessonGrantCatalog catalog,
            CodeDocumentationOneLessonSelectorV1 selector, CodeDocumentationOneLessonSessionBinding value)
        {
            var resolved = catalog.Resolve(selector);
            if (!resolved.IsSuccess || resolved.Value is not { FormatVersion: 2 } current || current.SourceSelection?.TeachingCurriculum is null) return null;
            var actual = new CodeDocumentationOneLessonSessionBinding(current.ObjectiveId, current.GrantId, current.GrantVersion, current.TrainingRunId,
                current.CandidateSha256, current.ParentManifestSha256, current.ParentWeightsSha256, current.ConfigurationSha256, current.ExamConditionSha256)
                { FormatVersion = 2, DescriptorsV2 = current.DescriptorsV2 };
            return value.FormatVersion == 2 && BindingBytesV2(value).AsSpan().SequenceEqual(BindingBytesV2(actual)) ? new(current, value) : null;
        }
    }

    internal static OperationResult<CodeDocumentationOneLessonSessionOpen> OpenSyntheticFullMaterialSelectionForTesting(
        RepositoryOwnedModelArtifactRoot root, CodeDocumentationOneLessonSessionBinding binding,
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector)
    {
        if (catalog is null || selector is null || binding is null) return CapacityInvalid<CodeDocumentationOneLessonSessionOpen>();
        var intent = FullMaterialStorageFamilyIntent.Capture(catalog, selector, binding);
        return intent is null ? CapacityInvalid<CodeDocumentationOneLessonSessionOpen>() :
            OpenCapacityValidationV2Core(root, binding, catalog.MaxTeacherCalls, catalog.Resolve(selector).Value, null, intent);
    }

    private static OperationResult<CodeDocumentationOneLessonContinuationCapacityV2> ValidateFullMaterialPrefix(byte[] bytes,
        CodeDocumentationOneLessonSessionBinding binding, string session, CodeDocumentationOneLessonContinuationCapacityV2 capacity, bool incomplete)
    {
        var parsed = CodeDocumentationOneLessonV2Wire.DecodeFullMaterialPrefix(bytes, SelectionProjection(binding), session, incomplete);
        return parsed.IsSuccess && parsed.Value is not null ? OperationResult<CodeDocumentationOneLessonContinuationCapacityV2>.Ok(
            capacity with { PrefixFrames = parsed.Value.Frames, PrefixBytes = bytes.Length }) : RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>();
    }

    internal bool HasCompletedFullMaterialSelection => _fullMaterialLease is not null && _fullMaterialCompleted?.Frames == 3;
    private OperationResult<T> InspectCompletedFullMaterialAdmissionCore<T>(Func<OperationResult<T>> inspect)
    {
        if (!HasCompletedFullMaterialSelection || _disposed || _fullMaterialPoisoned || Interlocked.CompareExchange(ref _selectionMaterialInFlight,1,0) != 0) return RecoveryRequired<T>();
        _fullMaterialCheckOwner.Value = this;
        try { return inspect(); } // Fixed wrappers below use the existing sanitized diagnosis/whole ORIGINAL exclusion; no material/provider/effect callback is accepted.
        finally { _fullMaterialCheckOwner.Value = null; Interlocked.Exchange(ref _selectionMaterialInFlight,0); }
    }
    internal OperationResult<OrdinaryDiagnostic> ClassifyCompletedFullMaterialBaseline(
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog,
        CodeDocumentationOneLessonSelectorV1 selector, CancellationToken token = default)
        => InspectCompletedFullMaterialAdmissionCore(() => ClassifyOrdinaryBaseline(baseline,catalog,selector,token));
    internal OperationResult<OrdinaryOriginalTeachingExclusion> InspectCompletedFullMaterialExclusion(
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog,
        CodeDocumentationOneLessonSelectorV1 selector, CancellationToken token = default)
        => InspectCompletedFullMaterialAdmissionCore(() => InspectOrdinaryOriginalTeachingExclusion(baseline,catalog,selector,token));

    internal async Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>>
        ReattachCompletedFullMaterialBaselineAsync(CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector,
            CodeDocumentationRepositoryOwnedModelCheckpointStartRequest start, CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context,
            CancellationToken token = default)
    {
        if (catalog is null || selector is null || start is null || context is null || _disposed || _fullMaterialLease is null || _fullMaterialCompleted?.Request is not { } request ||
            Interlocked.CompareExchange(ref _selectionMaterialInFlight, 1, 0) != 0)
            return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>();
        Exception? primary = null; OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>? staged = null;
        OperationResult<bool> finalReceiving = RecoveryRequired<bool>(), finalOriginal = RecoveryRequired<bool>(); Exception? secondary = null;
        byte[]? prefixBefore = null; CodeDocumentationOneLessonGrantCatalog? frozenCatalog = null;
        var frozenSelector = selector with { }; var rootBefore = start.RepositoryRoot;
        var expected = new CodeDocumentationPreparedBaselineExternalReference(request.OriginalReceiptId, request.OriginalReceiptSha256);
        try
        {
            try
            {
                var prefix = ReadCapacityPrefixV2();
                if (!prefix.IsSuccess || prefix.Value!.PrefixFrames != 3 || !_fullMaterialLease.Matches(_snapshot.Binding,catalog.Resolve(selector).Value))
                    staged = RecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>();
                else
                {
                    var ownership = CheckPreparedBaselineOwnership(catalog,selector,start,_snapshot.Binding,BindingBytesV2(_snapshot.Binding),ComputeBindingSha256(_snapshot.Binding));
                    if(!ownership.IsSuccess) staged = ownership.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>();
                    else
                    {
                        prefixBefore = _v2Prefix!.ToArray(); frozenCatalog = CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([_earlyGrant!],catalog.MaxTeacherCalls);
                        var existing = await new CodeDocumentationRepositoryOwnedPreparedBaselineJournal().InspectExistingOrdinaryExternalReferenceAsync(
                            frozenCatalog,frozenSelector,rootBefore,expected,token).ConfigureAwait(false); // Only an authenticated receiving root may read saved ORIGINAL before raw recipients/evaluator factories.
                        staged = !existing.IsSuccess ? existing.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>() :
                            await ConsumeOrdinaryBaselineCoreAsync(catalog,selector,start,context,token,expected,true).ConfigureAwait(false); // Nonnull expectation chooses the existing-only receiver/shared fresh capture core.
                    }
                }
            }
            catch(Exception ex) when(ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { primary = ex; }
            finally
            {
                try
                {
                    var current = ReadCapacityPrefixV2();
                    finalReceiving = current.IsSuccess && current.Value!.PrefixFrames==3 && prefixBefore is not null && prefixBefore.AsSpan().SequenceEqual(_v2Prefix) &&
                        _fullMaterialLease.Matches(_snapshot.Binding,catalog.Resolve(selector).Value) ? OperationResult<bool>.Ok(true) : RecoveryRequired<bool>();
                }
                catch(Exception ex) when(ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { secondary = ex; }
                if(frozenCatalog is not null)
                {
                    try { finalOriginal = await new CodeDocumentationRepositoryOwnedPreparedBaselineJournal().InspectExistingOrdinaryExternalReferenceAsync(frozenCatalog,frozenSelector,rootBefore,expected,CancellationToken.None).ConfigureAwait(false); }
                    catch(Exception ex) when(ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { if(secondary is null) secondary = ex; else secondary.Data["FullMaterialAttachmentOriginalFailure"] = ex; }
                } // Separate None existing-only attempt cannot be skipped by receiving failure or cancellation.
                if(primary is not null)
                {
                    if(!finalReceiving.IsSuccess) primary.Data["FullMaterialAttachmentReceivingError"] = finalReceiving.ErrorCode;
                    if(!finalOriginal.IsSuccess) primary.Data["FullMaterialAttachmentOriginalError"] = finalOriginal.ErrorCode;
                    if(secondary is not null) primary.Data["FullMaterialAttachmentFinalFailure"] = secondary;
                }
            }
        }
        finally { Interlocked.Exchange(ref _selectionMaterialInFlight, 0); }
        if(primary is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(primary).Throw();
        if(secondary is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(secondary).Throw();
        if(staged is { IsSuccess:false }) return staged; // Preserve the actual primary business owner refusal rather than replacing it with a generic final error.
        if(!finalReceiving.IsSuccess) return finalReceiving.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>();
        if(!finalOriginal.IsSuccess) return finalOriginal.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>();
        return staged ?? RecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>();
    }

    private static OperationResult<CodeDocumentationOneLessonSessionOpen> OpenCapacityValidationV2Core( // Share custody with an explicit private intent, not a config or decoded effect permit.
        RepositoryOwnedModelArtifactRoot? root, CodeDocumentationOneLessonSessionBinding? binding, // Retain the same repository and complete immutable objective lookup.
        int maxTeacherCalls, CodeDocumentationOneLessonGrantV1? earlyGrant, SyntheticStudentLeaseIntent? studentLease,
        FullMaterialStorageFamilyIntent? fullMaterialLease = null) // Incoming JSON cannot choose the typed family.
    {
        if (!ValidRoot(root) || !ValidBinding(binding) || binding!.FormatVersion != 2 ||
            !CodeDocumentationOneLessonV2Wire.ValidDescriptors(binding.DescriptorsV2) ||
            binding.ObjectiveId.Length > 128 || binding.GrantId.Length > 128 || binding.GrantVersion.Length > 64 ||
            binding.TrainingRunId.Length > 128 ||
            new[] { binding.ObjectiveId, binding.GrantId, binding.GrantVersion }.Any(x => x != x.Trim()))
            return CapacityInvalid<CodeDocumentationOneLessonSessionOpen>();

        CodeDocumentationOneLessonContinuationCapacityV2 capacity;
        try
        {
            var descriptor = binding.DescriptorsV2!;
            // Deliberately checked before the derived bounds: no wrapped count
            // may be mistaken for a legal small continuation.
            var n = checked(descriptor.CaseCount * descriptor.RepeatCount);
            var calls = checked(2 * n);
            if (descriptor.CaseCount is < 1 or > 200 || descriptor.RepeatCount < 2 ||
                maxTeacherCalls is < 0 or > 10_000) // Retain the original bounded generic count and teacher-budget grammar.
                return CapacityInvalid<CodeDocumentationOneLessonSessionOpen>(); // Invalid counts grant no continuation lease.
            if (calls > maxTeacherCalls) // Require both full evaluation series to fit the supplied finite budget.
                return CapacityExceeded<CodeDocumentationOneLessonSessionOpen>(); // Preserve the existing budget refusal without reducing repeats.
            capacity = fullMaterialLease is not null ? new(n, 3, CodeDocumentationOneLessonV2Wire.FullMaterialTotalMaximumBytes, 0, 0) : studentLease is not null // Keep separate fixed full-material slots without raising legacy limits.
                ? new(n, checked(20 + 2 * n), checked(627_846L + 6_548L * n), 0, 0) // Retain the full promoted/Trial reserve plus exact Started/Completed ceilings.
                : earlyGrant is null // Add a selection reservation only to a genuine ordinary grant.
                ? new(n, checked(17 + 2 * n), checked(62_481L + 6_548L * n), 0, 0) // Preserve the generic frame and LF-inclusive byte ceilings exactly.
                : new(n, checked(18 + 2 * n), checked(95_249L + 6_548L * n), 0, 0); // Reserve one additional 32768-byte selection frame for ordinary dispatch.
            if (BindingBytesV2(binding).Length > 2_120)
                return CapacityExceeded<CodeDocumentationOneLessonSessionOpen>();
            if (fullMaterialLease is not null && (studentLease is not null || !fullMaterialLease.Matches(binding, earlyGrant)))
                return CapacityInvalid<CodeDocumentationOneLessonSessionOpen>();
            if (studentLease is not null && (!studentLease.Matches(binding, earlyGrant) || // A private intent must match the actual frozen grant and complete binding.
                BindingBytesV2(binding).Length > CodeDocumentationOneLessonV2Wire.SelectedStudentPrivateBindingMaximumBytes || // The narrower private supported subset absorbs the exact192-byte Genesis delta.
                SyntheticStudentGenesisBytes(binding, Sha256(binding.ObjectiveId), earlyGrant!).Length > 2585)) // Preserve the existing full Genesis ceiling before any custody write or baseline/model effect.
                return CapacityExceeded<CodeDocumentationOneLessonSessionOpen>(); // Do not truncate fields or silently expand the ordinary/common binding cap.
        }
        catch (OverflowException) { return CapacityInvalid<CodeDocumentationOneLessonSessionOpen>(); }
        catch (JsonException) { return CapacityInvalid<CodeDocumentationOneLessonSessionOpen>(); }

        FileStream? ownerLock = null;
        FileStream? history = null;
        FileStream? nativeRequest = null, nativeSchema = null;
        byte[]? nativeRequestBytes = null, nativeSchemaBytes = null;
        try
        {
            var sessionId = Sha256(binding.ObjectiveId);
            var sessionsRoot = Path.GetFullPath(Path.Combine(root!.ModelArtifactRoot, SessionsDirectoryName));
            var directory = Path.Combine(sessionsRoot, sessionId);
            var historyPath = Path.Combine(directory, HistoryFileName);
            var lockPath = Path.Combine(directory, LockFileName);
            var lockExisted = File.Exists(lockPath);
            if (!Inside(root.ModelArtifactRoot, sessionsRoot) || !Inside(sessionsRoot, directory) ||
                SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(directory))
                return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>();
            EnsurePrivateDirectory(sessionsRoot);
            EnsurePrivateDirectory(directory);
            try { ownerLock = OpenOwnerLock(lockPath); }
            catch (IOException exception) when (IsLockContention(exception))
            {
                return Failure<CodeDocumentationOneLessonSessionOpen>(BusyErrorCode,
                    "Another process currently owns this one-lesson objective.");
            }
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(ownerLock.SafeFileHandle, lockPath))
                return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>();
            var lockBytes = Utf8.GetBytes("s04-one-lesson-session-v2\n" + sessionId + "\n");
            if (ownerLock.Length == 0)
            {
                // An existing empty lock is incomplete custody, not a fresh claim.
                if (lockExisted || File.Exists(historyPath) || Directory.GetFileSystemEntries(directory).Length != 1)
                    return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>();
                ownerLock.Write(lockBytes); ownerLock.Flush(flushToDisk: true);
            }
            if (!ReadStream(ownerLock, 1024).AsSpan().SequenceEqual(lockBytes))
                return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>();
            var entries = Directory.GetFileSystemEntries(directory);
            var nativeRequestPath = Path.Combine(directory, CodeDocumentationNativeCodexSelectionWire.RequestFileName);
            var nativeSchemaPath = Path.Combine(directory, CodeDocumentationNativeCodexSelectionWire.SchemaFileName);
            var nativeStartedPath = Path.Combine(directory, CodeDocumentationNativeCodexSelectionWire.StartedFileName); // The genuine catalog/early-grant owner is already selected before native history inspection.
            if (File.Exists(nativeStartedPath)) return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>(); // Any reopened Started, including partial/unknown data, permanently refuses another launch; preserve bytes/timestamps/nonce.
            if (entries.Any(x => !SamePath(x, lockPath) && !SamePath(x, historyPath) && !SamePath(x, nativeRequestPath) && !SamePath(x, nativeSchemaPath)) || entries.Any(Directory.Exists))
                return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>();
            if (File.Exists(nativeRequestPath) != File.Exists(nativeSchemaPath)) return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>();
            if (File.Exists(nativeRequestPath))
            {
                nativeRequest = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(nativeRequestPath, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.SequentialScan);
                nativeSchema = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(nativeSchemaPath, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.SequentialScan);
                var requestRead = ReadNativePreparedFile(nativeRequest, nativeRequestPath, CodeDocumentationNativeCodexSelectionWire.RequestMaximumBytes);
                if (!requestRead.IsSuccess || requestRead.Value is null) return requestRead.MapError<CodeDocumentationOneLessonSessionOpen>();
                var schemaRead = ReadNativePreparedFile(nativeSchema, nativeSchemaPath, CodeDocumentationNativeCodexSelectionWire.FamilyMaximumBytes - nativeRequest.Length);
                if (!schemaRead.IsSuccess || schemaRead.Value is null) return schemaRead.MapError<CodeDocumentationOneLessonSessionOpen>();
                var decoded = CodeDocumentationNativeCodexSelectionWire.DecodePrepared(requestRead.Value);
                if (!decoded.IsSuccess || decoded.Value is null || decoded.Value.SessionId != sessionId ||
                    !decoded.Value.CopyBindingBytes().AsSpan().SequenceEqual(CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(SelectionProjection(binding)))) ||
                    !schemaRead.Value.AsSpan().SequenceEqual(CodeDocumentationNativeCodexSelectionWire.CopySchemaBytes())) return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>();
                nativeRequestBytes = requestRead.Value; nativeSchemaBytes = schemaRead.Value;
            }
            var prefix = Array.Empty<byte>();
            if (File.Exists(historyPath))
            {
                history = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(historyPath, FileMode.Open,
                    earlyGrant is null ? FileAccess.Read : FileAccess.ReadWrite, FileShare.Read, FileOptions.SequentialScan);
                if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(history.SafeFileHandle, historyPath))
                    return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>();
                if (history.Length == 0) return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>(); // Held, path-validated empty history is incomplete custody; the bounded reader also refuses it with null.
                if (history.Length > capacity.MaximumBytes) return CapacityExceeded<CodeDocumentationOneLessonSessionOpen>(); // Only observed held-file overage establishes this byte-budget refusal.
                var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(history, capacity.MaximumBytes);
                if (bytes is null) return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>(); // Short or unstable bounded reads establish incomplete custody, not a proven overflow.
                if (bytes.Length == 0) return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>(); // An existing empty history is incomplete custody, not fresh absent history.
                if (studentLease is null && fullMaterialLease is null && !V2PhysicalLinesWithin(bytes, 32768)) // Keep the old preparse fence; typed full mode bounds its own three lines.
                    return CapacityExceeded<CodeDocumentationOneLessonSessionOpen>(); // A Kind or Genesis marker cannot opt the default reader into large response parsing.
                if (earlyGrant is not null)
                {
                    var firstLine = V2BoundedFirstLine(bytes, fullMaterialLease is null ? 2585 : CodeDocumentationOneLessonV2Wire.FullMaterialGenesisMaximumBytes); // The typed lease selects the bound before header parsing.
                    using var first = JsonDocument.Parse(firstLine);
                    if (!first.RootElement.TryGetProperty("BindingV2", out var priorBinding) ||
                        !CodeDocumentationOneLessonV2Wire.CanonicalBytes(priorBinding).AsSpan().SequenceEqual(BindingBytesV2(binding)))
                        return Failure<CodeDocumentationOneLessonSessionOpen>(ObjectiveConflictErrorCode,
                            "The durable objective binding differs from the current immutable grant.");
                }
                var parsed = fullMaterialLease is not null ? ValidateFullMaterialPrefix(bytes, binding, sessionId, capacity, false) : earlyGrant is null ? ValidatePrefixV2(bytes, binding, sessionId, capacity) : // A fresh full-family host accepts only complete matching history.
                    ValidateOrdinaryPrefixV2(bytes, binding, sessionId, capacity, earlyGrant, allowIncomplete: false, studentLease: studentLease); // Authorize exact small private prefix before parsing bounded response positions.
                if (!parsed.IsSuccess || parsed.Value is null)
                    return parsed.MapError<CodeDocumentationOneLessonSessionOpen>();
                capacity = parsed.Value;
                prefix = bytes;
            }
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(ownerLock.SafeFileHandle, lockPath) ||
                (history is not null && !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(history.SafeFileHandle, historyPath)))
                return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>();
            var relative = Path.GetRelativePath(root.RepositoryRoot, historyPath).Replace(Path.DirectorySeparatorChar, '/');
            var snapshot = new CodeDocumentationOneLessonSessionSnapshot(sessionId, relative, Sha256(prefix), binding,
                Math.Max(0, capacity.PrefixFrames - 1), new());
            var session = new CodeDocumentationOneLessonSession(historyPath, relative, lockPath, sessionId,
                ownerLock, snapshot, history, prefix, capacity);
            if (nativeRequest is not null && checked(nativeRequest.Length + nativeSchema!.Length + ownerLock.Length + (history?.Length ?? 0)) > CodeDocumentationNativeCodexSelectionWire.SessionMaximumBytes)
                return CapacityExceeded<CodeDocumentationOneLessonSessionOpen>();
            session._nativePreparedRequest = nativeRequest; session._nativePreparedSchema = nativeSchema;
            session._nativePreparedRequestBytes = nativeRequestBytes; session._nativePreparedSchemaBytes = nativeSchemaBytes;
            session._earlyGrant = earlyGrant;
            session._studentLease = studentLease; // Retain the original private intent; decoder syntax cannot mint it on reopen.
            session._fullMaterialLease = fullMaterialLease;
            if (fullMaterialLease is not null && capacity.PrefixFrames == 3)
                session._fullMaterialCompleted = CodeDocumentationOneLessonV2Wire.DecodeFullMaterialPrefix(prefix, SelectionProjection(binding), sessionId, false).Value;
            if(fullMaterialLease is null && earlyGrant is not null && (capacity.PrefixFrames==2 || studentLease is not null && capacity.PrefixFrames==4)) // Full readback is intentionally incompatible with legacy selection authority.
                session._selectionFrame=DecodeSelectionPrefix(prefix,binding,sessionId,earlyGrant,studentLease).Value; // Keep exact selection replay and receiving ORIGINAL reattachment bound to this owner.
            if (earlyGrant is not null && capacity.PrefixFrames == 7)
                session.EarlyOutcome = EarlyReadback(prefix);
            ownerLock = null; history = null; nativeRequest = null; nativeSchema = null;
            return OperationResult<CodeDocumentationOneLessonSessionOpen>.Ok(new(session, Created: false));
        }
        catch (JsonException) { return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>(); }
        catch (DecoderFallbackException) { return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>(); }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        { return RecoveryRequired<CodeDocumentationOneLessonSessionOpen>(); }
        finally { nativeRequest?.Dispose(); nativeSchema?.Dispose(); history?.Dispose(); ownerLock?.Dispose(); }
    }

    /// <summary>Consumes ORIGINAL under the held full lease, with detached mutable inputs and return guards.</summary>
    internal async Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>>
        ConsumeOrdinaryBaselineAsync(CodeDocumentationOneLessonGrantCatalog catalog,
            CodeDocumentationOneLessonSelectorV1 selector,
            CodeDocumentationRepositoryOwnedModelCheckpointStartRequest start,
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context,
            CancellationToken cancellationToken)
    { // Baseline replacement shares the material owner gate before any checks or mutation.
        if (_disposed || Interlocked.CompareExchange(ref _selectionMaterialInFlight, 1, 0) != 0) // Public re-entry always refuses even when AsyncLocal owner context flows.
            return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>(); // Do not replace the held baseline during a selection operation.
        try // Reattachment already owns this gate and retains its direct private core call.
        { // Only this acquisition may update the ordinary baseline fingerprints.
            return await ConsumeOrdinaryBaselineCoreAsync(catalog, selector, start, context, cancellationToken, // Preserve all original receipt, evaluator and binding guards.
                _selectionFrame is null ? null : new(_selectionFrame.Payload.OriginalReceiptId, _selectionFrame.Payload.OriginalReceiptSha256), false).ConfigureAwait(false); // Saved metadata keeps its expected ORIGINAL receipt.
        } // Finite baseline completion remains inside the shared owner boundary.
        finally { Interlocked.Exchange(ref _selectionMaterialInFlight, 0); } // Cancellation and refusal also release the sole public acquisition.
    } // No public already-owned or skip-guard capability is exposed.

    private async Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>>
        ConsumeOrdinaryBaselineCoreAsync(CodeDocumentationOneLessonGrantCatalog catalog,
            CodeDocumentationOneLessonSelectorV1 selector, CodeDocumentationRepositoryOwnedModelCheckpointStartRequest start,
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context, CancellationToken cancellationToken,
            CodeDocumentationPreparedBaselineExternalReference? expectedOriginal, bool completedFullMaterial)
    {
        var prefix = ReadCapacityPrefixV2();
        if (!prefix.IsSuccess) return prefix.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>();
        var receivingPrefix = completedFullMaterial ? _v2Prefix!.ToArray() : null; // Full reopen freezes its complete held bytes before invoking the existing receiver.
        if (_earlyGrant is null || (completedFullMaterial ? _fullMaterialLease is null || _fullMaterialCompleted?.Frames != 3 || prefix.Value!.PrefixFrames != 3 || expectedOriginal is null : !IsOrdinaryBaselinePrefix(prefix.Value!)) || // Completed-only existing receiver remains separate from the generic baseline writer.
            !ReferenceEquals(catalog.Resolve(selector).Value, _earlyGrant)) // Accept only fresh or completely bound ordinary baseline prefixes from the same immutable grant.
            return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>(); // Accept only fresh or completely bound ordinary baseline prefixes from the same immutable grant.
        var binding = _snapshot.Binding;
        var before = BindingBytesV2(binding);
        var bindingHash = ComputeBindingSha256(binding);
        var ownership = CheckPreparedBaselineOwnership(catalog, selector, start, binding, before, bindingHash);
        if (!ownership.IsSuccess) return ownership.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>();
        var contextValidation = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.ValidateContext(context);
        if (!contextValidation.IsSuccess)
            return contextValidation.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>();
        var selectedEvaluator = _earlyGrant.EvaluatorSelection!.OpenFresh(start.RepositoryRoot,
            context.GenerationSettings, context.EvaluatorConfiguration, context.NoisePolicyConfig,
            _earlyGrant.ConfigurationSha256, _earlyGrant.ExamConditionSha256);
        if (!selectedEvaluator.IsSuccess || selectedEvaluator.Value is null)
            return selectedEvaluator.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>();
        if (selectedEvaluator.Value.Corpus.CorpusSha256 != context.Corpus.CorpusSha256 ||
            context.CorpusManifest.CorpusSha256 != context.Corpus.CorpusSha256)
            return Invalid<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>();
        var tokenizerBytes = PreparedTokenizerBytes(context.Tokenizer);
        using var stream = new MemoryStream(tokenizerBytes, writable: false);
        var evidence = context.EvidenceRefs.ToArray();
        var modules = start.LoraConfig?.TargetModules.ToArray();
        var capturedStart = start with { ModelConfig = start.ModelConfig with { },
            RuntimeCompatibility = start.RuntimeCompatibility with { },
            LoraConfig = start.LoraConfig is null ? null : start.LoraConfig with { TargetModules = modules!.ToArray() },
            AdapterSeedAuthority = start.AdapterSeedAuthority is null ? null : start.AdapterSeedAuthority with { } };
        var captured = context with { Tokenizer = BpeTokenizer.Load(stream),
            CorpusManifest = context.CorpusManifest with { }, GenerationSettings = context.GenerationSettings with { },
            EvidenceRefs = Array.AsReadOnly(evidence.ToArray()),
            NoisePolicyConfig = new(context.NoisePolicyConfig.RepeatCount, context.NoisePolicyConfig.StandardDeviationMultiplier) };
        var startBytes = JsonSerializer.SerializeToUtf8Bytes(capturedStart);
        var contextBytes = OrdinaryContextBytes(captured);
        var grantBytes = OrdinaryGrantBytes(_earlyGrant);
        var opened = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PrepareOrdinaryBaselineAsync( // Supply the saved ORIGINAL expectation to the existing-only ordinary receiver before baseline reattachment.
            catalog, selector, capturedStart, captured, cancellationToken, // Supply the saved ORIGINAL expectation to the existing-only ordinary receiver before baseline reattachment.
            expectedOriginal).ConfigureAwait(false); // A full Completed receiver always supplies the saved nonnull ID/SHA and cannot create/evaluate a new baseline.
        prefix = ReadCapacityPrefixV2();
        if (!prefix.IsSuccess) return prefix.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>();
        if (completedFullMaterial && (receivingPrefix is null || !receivingPrefix.AsSpan().SequenceEqual(_v2Prefix)))
            return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>();
        ownership = CheckPreparedBaselineOwnership(catalog, selector, start, binding, before, bindingHash);
        if (!ownership.IsSuccess) return ownership.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>();
        if (!ReferenceEquals(catalog.Resolve(selector).Value, _earlyGrant) ||
            !JsonSerializer.SerializeToUtf8Bytes(start).AsSpan().SequenceEqual(startBytes) ||
            !OrdinaryContextBytes(context).AsSpan().SequenceEqual(contextBytes) ||
            !OrdinaryGrantBytes(_earlyGrant).AsSpan().SequenceEqual(grantBytes) ||
            !PreparedTokenizerBytes(context.Tokenizer).AsSpan().SequenceEqual(tokenizerBytes) ||
            !context.EvidenceRefs.SequenceEqual(evidence, StringComparer.Ordinal) ||
            (modules is not null && !start.LoraConfig!.TargetModules.SequenceEqual(modules, StringComparer.Ordinal)))
            return Invalid<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>();
        if (opened.IsSuccess)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ordinaryBaseline = opened.Value;
            _ordinaryBaselineBytes = JsonSerializer.SerializeToUtf8Bytes(opened.Value!.Readback.Baseline);
            _ordinaryTokenizerBytes = PreparedTokenizerBytes(opened.Value.Context.Tokenizer);
            _ordinaryEvidence = opened.Value.Context.EvidenceRefs.ToArray();
            _ordinaryStart = capturedStart;
            _ordinaryStartBytes = startBytes;
            _ordinaryContextBytes = OrdinaryContextBytes(opened.Value.Context);
            _ordinaryReferenceBytes = OrdinaryReferenceBytes(opened.Value);
            _ordinaryGrantBytes = grantBytes;
            _ordinaryBindingBytes = before;
            _ordinaryEvaluationClient = opened.Value.Context.EvaluationClient;
        }
        return opened;
    }

    /// <summary>Descriptive feedback only: no original/context pointer or effect authority.</summary>
    internal sealed class OrdinaryDiagnostic
    {
        internal OrdinaryDiagnostic(IReadOnlyList<string> categories, string receiptId, string receiptSha256)
        {
            Categories = Array.AsReadOnly(categories.ToArray());
            OriginalReceiptId = receiptId;
            OriginalReceiptSha256 = receiptSha256;
        }
        internal IReadOnlyList<string> Categories { get; }
        internal string OriginalReceiptId { get; }
        internal string OriginalReceiptSha256 { get; }
    }

    /// <summary>Classifies the same receiving-owner ORIGINAL while its ordinary lease is held.</summary>
    internal OperationResult<OrdinaryDiagnostic> ClassifyOrdinaryBaseline(
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline,
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector,
        CancellationToken cancellationToken = default)
    {
        OperationResult<OrdinaryDiagnostic> GuardReturn(OperationResult<OrdinaryDiagnostic> result)
        {
            var after = CheckOrdinaryDiagnosticOwnership(baseline, catalog, selector);
            cancellationToken.ThrowIfCancellationRequested();
            return after.IsSuccess ? result : after.MapError<OrdinaryDiagnostic>();
        }
        var before = CheckOrdinaryDiagnosticOwnership(baseline, catalog, selector);
        if (!before.IsSuccess) return GuardReturn(before.MapError<OrdinaryDiagnostic>());
        cancellationToken.ThrowIfCancellationRequested();
        var cases = baseline.Readback.Baseline.Calibration.RepresentativeAssessment.Cases;
        var pairs = CodeDocumentationSavedModelAssessmentService.ValidateSavedDiagnosticPairs(cases);
        if (!pairs.IsSuccess) return GuardReturn(pairs.MapError<OrdinaryDiagnostic>());
        var quality = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
            .BaselineRequiresNewTrainingPair(baseline.Readback.Baseline);
        if (!quality.IsSuccess) return GuardReturn(quality.MapError<OrdinaryDiagnostic>());
        var categories = CodeDocumentationSavedModelAssessmentService.ProjectSavedNegativeCategories(cases);
        if (categories.Count == 0)
            return GuardReturn(Failure<OrdinaryDiagnostic>(UsableDiagnosisMissingErrorCode,
                "The ORIGINAL requires a lesson, but its representative assessment has no usable closed diagnosis."));
        return GuardReturn(OperationResult<OrdinaryDiagnostic>.Ok(new(categories,
            baseline.ExternalReference.ReceiptId, baseline.ExternalReference.ReceiptSha256)));
    }

    /// <summary>Checks the held ORIGINAL against complete teaching material and exposed metadata; finite evidence grants no lesson or effect authority.</summary>
    internal OperationResult<OrdinaryOriginalTeachingExclusion> InspectOrdinaryOriginalTeachingExclusion( // Keep receiving-owner issuance behind the genuine Session protocol.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Require the exact producer baseline retained by this Session.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Require the same resolved startup grant and bound selector.
        CancellationToken cancellationToken = default) // Observe cancellation before teaching access and throughout calculation.
        => OrdinaryOriginalTeachingExclusion.InspectAndIssue(this, baseline, catalog, selector, cancellationToken); // The sole issuer performs the protocol rather than accepting caller-supplied metadata.

    internal sealed class OrdinaryOriginalTeachingExclusion // Immutable ORIGINAL-only evidence exposes no baseline, closed corpus, client or permit.
    { // Keep construction within the sole full-protocol issuer.
        private readonly CodeDocumentationOneLessonSession _owner; // Retain issuing identity privately without any pointer getter.
        internal string OriginalReceiptId { get; } // Expose only the existing opaque ORIGINAL receipt identity.
        internal string OriginalReceiptSha256 { get; } // Expose only the existing opaque receipt commitment.
        internal string BindingSha256 { get; } // Expose only the receiving Session binding commitment.
        internal CodeDocumentationOneLessonSourceSelection.DeclaredTeachingCorpusReadback TeachingIdentities { get; } // Expose the complete guarded immutable descriptive graph.

        private OrdinaryOriginalTeachingExclusion(CodeDocumentationOneLessonSession owner, // Prevent caller-created witnesses or metadata-only conversion helpers.
            string receiptId, string receiptHash, string bindingHash, // Capture only commitments after the issuer completes its bound protocol.
            CodeDocumentationOneLessonSourceSelection.DeclaredTeachingCorpusReadback teaching) // Retain the complete guarded graph without raw teaching or ORIGINAL texts.
        { // Own one issued immutable witness.
            _owner = owner; OriginalReceiptId = receiptId; OriginalReceiptSha256 = receiptHash; BindingSha256 = bindingHash; TeachingIdentities = teaching; // Publish safe values only after genuine receiving checks.
        } // The private constructor is called solely at the final successful issuer boundary.

        internal bool IsIssuedBy(CodeDocumentationOneLessonSession owner) // Allow identity testing without exposing the retained owner pointer.
            => ReferenceEquals(_owner, owner); // This comparison grants no effect capability.

        internal static OperationResult<OrdinaryOriginalTeachingExclusion> InspectAndIssue( // The only reachable issuer itself executes every genuine receiving step.
            CodeDocumentationOneLessonSession owner, CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Bind the held Session and its exact producer baseline.
            CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, CancellationToken token) // Bind the same grant and cooperative cancellation.
        { // Neither calculation DTOs nor caller-created graph envelopes can bypass this protocol.
            var originalOpened = false; // Reopen closed material on every later return, including overlap failures.
            OperationResult<bool> OpenBoundOriginal() // Reuse the existing strict complete corpus owner and all selected byte pins.
            { // Check the complete fresh corpus against the captured producer context.
                token.ThrowIfCancellationRequested(); // Do not enter synchronous owner I/O after observed cancellation.
                var opened = owner._earlyGrant!.CorpusSelection!.OpenFresh(owner._ordinaryStart!.RepositoryRoot, baseline.Context.GenerationSettings); // Resolve only the genuine bound selection.
                token.ThrowIfCancellationRequested(); // Reject cancellation observed after strict readback.
                if (!opened.IsSuccess) return opened.MapError<bool>(); // Preserve actual strict owner failures without claiming a valid exclusion.
                if (opened.Value is null || opened.Value.Corpus.CorpusSha256 != baseline.Context.Corpus.CorpusSha256 || // Compare complete actual corpus identity to the captured context.
                    opened.Value.Corpus.CanonicalRepresentation != baseline.Context.Corpus.CanonicalRepresentation || // Include unused original entries and their full contents.
                    baseline.Context.CorpusManifest.CorpusSha256 != opened.Value.Corpus.CorpusSha256) // Require producer manifest continuity as well as full canonical equality.
                    return RecoveryRequired<bool>(); // Reject closed-material staleness without returning any raw original metadata.
                return OperationResult<bool>.Ok(true); // A fresh complete closed snapshot is calculation input, not a retained file lease.
            } // No closed corpus object escapes this local protocol check.
            OperationResult<bool> CheckAfter() // Recheck both failure and success paths before publication or issuance.
            { // Changed receiving inputs override any staged exclusion or earlier failure.
                var after = owner.CheckOrdinaryDiagnosticOwnership(baseline, catalog, selector); // Recheck all held Session, baseline, grant, tokenizer and context snapshots.
                token.ThrowIfCancellationRequested(); // Cancel before any final acceptance.
                if (!after.IsSuccess) return after; // Never publish stale receiving-owner evidence.
                if (originalOpened) // Revalidate closed bytes after every teaching outcome, including failure.
                { var closed = OpenBoundOriginal(); if (!closed.IsSuccess) return closed; } // Preserve finite after-failure strict readback.
                after = owner.CheckOrdinaryDiagnosticOwnership(baseline, catalog, selector); // Cover receiving mutations during the final synchronous corpus open.
                token.ThrowIfCancellationRequested(); // Last cancellation boundary before returning staged safe evidence.
                return after; // Reject any late owner change rather than stale acceptance.
            } // Retain no atomic lease or future race proof.
            OperationResult<OrdinaryOriginalTeachingExclusion> GuardReturn(OperationResult<OrdinaryOriginalTeachingExclusion> result) // Guard bounded failure results without constructing a placeholder witness.
            { var after = CheckAfter(); return after.IsSuccess ? result : after.MapError<OrdinaryOriginalTeachingExclusion>(); } // The successful issuer also calls this same complete after-check directly.
            var before = owner.CheckOrdinaryDiagnosticOwnership(baseline, catalog, selector); // Gate all teaching access on genuine held receiving ownership.
            if (!before.IsSuccess) return GuardReturn(before.MapError<OrdinaryOriginalTeachingExclusion>()); // Foreign, absent and disposed receiving owners never open teaching.
            token.ThrowIfCancellationRequested(); // Pre-cancel before classification or teaching access.
            var diagnosis = owner.ClassifyOrdinaryBaseline(baseline, catalog, selector, token); // Use the actual saved-pair, sufficiency and closed-diagnosis predicate.
            if (!diagnosis.IsSuccess) return GuardReturn(diagnosis.MapError<OrdinaryOriginalTeachingExclusion>()); // Sufficient or unusable diagnoses never open teaching.
            if (owner._earlyGrant!.SourceSelection?.TeachingCurriculum is null) // Require explicitly bound passive teaching metadata.
                return GuardReturn(Failure<OrdinaryOriginalTeachingExclusion>("ONE_LESSON_TEACHING_IDENTITIES_UNAVAILABLE", "The startup grant has no declared teaching identities.")); // Absent teaching fails without opening a corpus.
            var closedBefore = OpenBoundOriginal(); // Verify fresh complete closed bytes before comparison capture.
            if (!closedBefore.IsSuccess) return GuardReturn(closedBefore.MapError<OrdinaryOriginalTeachingExclusion>()); // Reject original staleness before any teaching open.
            originalOpened = true; // Require the final complete closed recheck for all subsequent paths.
            var calibration = baseline.Readback.Baseline.Calibration; // Read only the same genuine producer's retained saved observations.
            var teaching = owner._earlyGrant.SourceSelection.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal( // Invoke the existing full teaching owner with private closed calculation inputs.
                owner._ordinaryStart!.RepositoryRoot, baseline.Context.Corpus, calibration.RepresentativeAssessment.Cases, calibration.Calibration.Observations, token); // Include the complete original corpus and all repeat answer cells.
            if (!teaching.IsSuccess || teaching.Value is null) return GuardReturn(teaching.MapError<OrdinaryOriginalTeachingExclusion>()); // Recheck receiving and closed state even after overlap or owner failure.
            var guarded = CheckAfter(); // Finish every genuine check before the sole constructor call.
            if (!guarded.IsSuccess) return guarded.MapError<OrdinaryOriginalTeachingExclusion>(); // Never mint a witness before the full protocol succeeds.
            token.ThrowIfCancellationRequested(); // Cancel immediately before immutable issuance.
            return OperationResult<OrdinaryOriginalTeachingExclusion>.Ok(new(owner, baseline.ExternalReference.ReceiptId, // Mint solely inside the genuine receiving issuer after all before/after checks.
                baseline.ExternalReference.ReceiptSha256, ComputeBindingSha256(owner._snapshot.Binding), teaching.Value)); // Expose only three opaque commitments and the whole guarded graph.
        } // Issuance does not append, evaluate, infer, train, update, or certify teaching admission.
    } // Preserve ORIGINAL-only scope and keep full material rights and lesson readiness open.

    private OperationResult<bool> CheckOrdinaryDiagnosticOwnership(
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline,
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector)
    {
        var prefix = ReadCapacityPrefixV2();
        if (!prefix.IsSuccess) return prefix.MapError<bool>();
        if (_earlyGrant is null || _ordinaryStart is null || _ordinaryBindingBytes is null ||
            _studentResponseInFlight != 0 && !ReferenceEquals(_studentCheckOwner.Value, this) || // A competing invocation receives no transient Started owner-check allowance.
            baseline is null || !ReferenceEquals(baseline, _ordinaryBaseline) || !IsOrdinaryDiagnosticPrefix(prefix.Value!) || // Permit a Genesis inspection only under this owner private in-flight token and unchanged custody.
            !ReferenceEquals(catalog.Resolve(selector).Value, _earlyGrant)) // Permit a Genesis inspection only under this owner private in-flight token and unchanged custody.
            return RecoveryRequired<bool>();
        try
        {
            var ownership = CheckPreparedBaselineOwnership(catalog, selector, _ordinaryStart,
                _snapshot.Binding, _ordinaryBindingBytes, Sha256(Utf8.GetBytes("code-documentation-one-lesson-binding-v2\n")
                    .Concat(_ordinaryBindingBytes).ToArray()));
            if (!ownership.IsSuccess) return ownership;
            if (!JsonSerializer.SerializeToUtf8Bytes(baseline.Readback.Baseline).AsSpan().SequenceEqual(_ordinaryBaselineBytes) ||
                !PreparedTokenizerBytes(baseline.Context.Tokenizer).AsSpan().SequenceEqual(_ordinaryTokenizerBytes) ||
                !baseline.Context.EvidenceRefs.SequenceEqual(_ordinaryEvidence!, StringComparer.Ordinal) ||
                !JsonSerializer.SerializeToUtf8Bytes(_ordinaryStart).AsSpan().SequenceEqual(_ordinaryStartBytes) ||
                !OrdinaryContextBytes(baseline.Context).AsSpan().SequenceEqual(_ordinaryContextBytes) ||
                !OrdinaryReferenceBytes(baseline).AsSpan().SequenceEqual(_ordinaryReferenceBytes) ||
                !OrdinaryGrantBytes(_earlyGrant).AsSpan().SequenceEqual(_ordinaryGrantBytes) ||
                !ReferenceEquals(baseline.Context.EvaluationClient, _ordinaryEvaluationClient))
                return RecoveryRequired<bool>();
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        { return RecoveryRequired<bool>(); }
    }

    // Runtime client counters are deliberately excluded. Only its held identity
    // and protected configuration participate in the in-memory binding.
    private sealed class SelectionInFlight(CodeDocumentationOneLessonSelectionDecisionProvider provider, // Hold one private nonce, copied Genesis and concrete issuer only during this single dispatch.
        CodeDocumentationTeachingSelectionInputV1 input,byte[] genesis) // Hold one private nonce, copied Genesis and concrete issuer only during this single dispatch.
    { // Begin hold one private nonce, copied Genesis and concrete issuer only during this single dispatch.
        internal readonly CodeDocumentationOneLessonSelectionDecisionProvider Provider=provider; // Hold one private nonce, copied Genesis and concrete issuer only during this single dispatch.
        internal readonly CodeDocumentationTeachingSelectionInputV1 Input=input; // Hold one private nonce, copied Genesis and concrete issuer only during this single dispatch.
        internal readonly byte[] Genesis=genesis.ToArray(); // Hold one private nonce, copied Genesis and concrete issuer only during this single dispatch.
        internal readonly Guid Nonce=Guid.NewGuid(); // Hold one private nonce, copied Genesis and concrete issuer only during this single dispatch.
        internal bool InvocationConsumed; // Hold one private nonce, copied Genesis and concrete issuer only during this single dispatch.
    } // Finish hold one private nonce, copied Genesis and concrete issuer only during this single dispatch.
    private bool IsOrdinaryBaselinePrefix(CodeDocumentationOneLessonContinuationCapacityV2 prefix) // Distinguish fresh, saved and private in-flight prefixes without accepting a restart Genesis.
        => prefix.PrefixFrames==0 || (_selectionFrame is not null && (prefix.PrefixFrames==2 || // Preserve original saved choice reattachment without creating a fresh ORIGINAL.
            _studentLease is not null && prefix.PrefixFrames==4)); // Completed response replay reattaches only the same existing ORIGINAL and never Generates.
    private bool IsOrdinaryDiagnosticPrefix(CodeDocumentationOneLessonContinuationCapacityV2 prefix) // Distinguish fresh, saved and private in-flight prefixes without accepting a restart Genesis.
        => IsOrdinaryBaselinePrefix(prefix) || (_fullMaterialLease is not null && prefix.PrefixFrames is >= 1 and <= 3 &&
            _selectionMaterialInFlight == 1 && ReferenceEquals(_fullMaterialCheckOwner.Value,this)) || // No generic baseline/writer prefix is widened; only this operation's finite checks can inspect full mode.
            (_studentLease is not null && prefix.PrefixFrames==3 && // Permit only finite same-operation checks of a durable Started prefix.
            _studentResponseInFlight != 0 && ReferenceEquals(_studentCheckOwner.Value, this)) || // Restart and competing calls obtain no in-flight authority.
            (prefix.PrefixFrames==1 && _selectionInFlight is {} flight && // Distinguish fresh, saved and private in-flight prefixes without accepting a restart Genesis.
            flight.Nonce!=Guid.Empty && _v2Prefix!.AsSpan().SequenceEqual(flight.Genesis)); // Distinguish fresh, saved and private in-flight prefixes without accepting a restart Genesis.
    private static CodeDocumentationOneLessonBindingProjectionV2 SelectionProjection(CodeDocumentationOneLessonSessionBinding b) // Distinguish fresh, saved and private in-flight prefixes without accepting a restart Genesis.
        => new(b.ObjectiveId,b.GrantId,b.GrantVersion,b.TrainingRunId,b.CandidateSha256,b.ParentManifestSha256, // Distinguish fresh, saved and private in-flight prefixes without accepting a restart Genesis.
            b.ParentWeightsSha256,b.ConfigurationSha256,b.ExamConditionSha256,b.DescriptorsV2!); // Distinguish fresh, saved and private in-flight prefixes without accepting a restart Genesis.

    internal bool HasSavedSelectionBinding => _selectionFrame is not null; // Check the saved ORIGINAL claim and byte identity through the existing journal before any evaluator factory.
    internal Task<OperationResult<bool>> InspectExistingOrdinarySelectionOriginalAsync( // Check the saved ORIGINAL claim and byte identity through the existing journal before any evaluator factory.
        CodeDocumentationOneLessonGrantCatalog catalog,CodeDocumentationOneLessonSelectorV1 selector, // Check the saved ORIGINAL claim and byte identity through the existing journal before any evaluator factory.
        string repositoryRoot,CancellationToken token) // Check the saved ORIGINAL claim and byte identity through the existing journal before any evaluator factory.
    { // Begin check the saved ORIGINAL claim and byte identity through the existing journal before any evaluator factory.
        var prefix=ReadCapacityPrefixV2(); // Check the saved ORIGINAL claim and byte identity through the existing journal before any evaluator factory.
        if(!prefix.IsSuccess || _selectionFrame is null || !IsOrdinaryBaselinePrefix(prefix.Value!) || // Completed private response replay must still authenticate the saved ORIGINAL before any evaluator factory.
            !ReferenceEquals(catalog.Resolve(selector).Value,_earlyGrant)) // Check the saved ORIGINAL claim and byte identity through the existing journal before any evaluator factory.
            return Task.FromResult(RecoveryRequired<bool>()); // Check the saved ORIGINAL claim and byte identity through the existing journal before any evaluator factory.
        return new CodeDocumentationRepositoryOwnedPreparedBaselineJournal().InspectExistingOrdinaryExternalReferenceAsync( // Check the saved ORIGINAL claim and byte identity through the existing journal before any evaluator factory.
            catalog,selector,repositoryRoot,new(_selectionFrame.Payload.OriginalReceiptId, // Check the saved ORIGINAL claim and byte identity through the existing journal before any evaluator factory.
                _selectionFrame.Payload.OriginalReceiptSha256),token); // Check the saved ORIGINAL claim and byte identity through the existing journal before any evaluator factory.
    } // Finish check the saved ORIGINAL claim and byte identity through the existing journal before any evaluator factory.

    internal sealed class SelectionBindingReadback // Issue replay readback only from this held Session after payload and final owner checks.
    { // Begin issue replay readback only from this held Session after payload and final owner checks.
        private readonly CodeDocumentationOneLessonSession owner; // Issue replay readback only from this held Session after payload and final owner checks.
        internal CodeDocumentationTeachingSelectionPayloadV1 Payload {get;} // Issue replay readback only from this held Session after payload and final owner checks.
        internal string PrefixSha256 {get;} // Issue replay readback only from this held Session after payload and final owner checks.
        internal string BindingSha256 => ComputeBindingSha256(owner._snapshot.Binding); // Let the actual recipient compare the genuine receiving binding without exposing an owner pointer.
        internal bool HasCurrentIssuingOwner => IsIssuedBy(owner); // A disposed issuing Session cannot authorize a detached recipient call.
        private SelectionBindingReadback(CodeDocumentationOneLessonSession owner, // Issue replay readback only from this held Session after payload and final owner checks.
            CodeDocumentationTeachingSelectionPayloadV1 payload,string hash) // Issue replay readback only from this held Session after payload and final owner checks.
        {this.owner=owner;Payload=payload;PrefixSha256=hash;} // Issue replay readback only from this held Session after payload and final owner checks.
        internal bool IsIssuedBy(CodeDocumentationOneLessonSession expected) // Issue replay readback only from this held Session after payload and final owner checks.
            => ReferenceEquals(owner,expected) && !owner._disposed; // Issue replay readback only from this held Session after payload and final owner checks.
        internal static OperationResult<SelectionBindingReadback> InspectAndIssue(CodeDocumentationOneLessonSession owner, // Issue replay readback only from this held Session after payload and final owner checks.
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Issue replay readback only from this held Session after payload and final owner checks.
            CodeDocumentationOneLessonGrantCatalog catalog,CodeDocumentationOneLessonSelectorV1 selector,CancellationToken token) // Issue replay readback only from this held Session after payload and final owner checks.
        { // Begin issue replay readback only from this held Session after payload and final owner checks.
            var prefix=owner.ReadCapacityPrefixV2(); // Issue replay readback only from this held Session after payload and final owner checks.
            if(!prefix.IsSuccess || owner._selectionFrame is null || !owner.IsOrdinaryDiagnosticPrefix(prefix.Value!)) // Permit genuine saved choice readback during finite same-owner response checks or completed replay only.
                return RecoveryRequired<SelectionBindingReadback>(); // Issue replay readback only from this held Session after payload and final owner checks.
            var p=owner._selectionFrame.Payload; // Issue replay readback only from this held Session after payload and final owner checks.
            var checkedPayload=owner.CheckSelectionPayload(baseline,catalog,selector,p,token); // Issue replay readback only from this held Session after payload and final owner checks.
            if(!checkedPayload.IsSuccess) return checkedPayload.MapError<SelectionBindingReadback>(); // Issue replay readback only from this held Session after payload and final owner checks.
            var after=owner.CheckOrdinaryDiagnosticOwnership(baseline,catalog,selector); // Issue replay readback only from this held Session after payload and final owner checks.
            token.ThrowIfCancellationRequested(); // Issue replay readback only from this held Session after payload and final owner checks.
            if(!after.IsSuccess) return after.MapError<SelectionBindingReadback>(); // Issue replay readback only from this held Session after payload and final owner checks.
            return OperationResult<SelectionBindingReadback>.Ok(new(owner,p,Sha256(owner._v2Prefix!))); // Issue replay readback only from this held Session after payload and final owner checks.
        } // Finish issue replay readback only from this held Session after payload and final owner checks.
    } // Finish issue replay readback only from this held Session after payload and final owner checks.
    internal OperationResult<SelectionBindingReadback> TryReadOrdinarySelectionBinding( // Issue replay readback only from this held Session after payload and final owner checks.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Issue replay readback only from this held Session after payload and final owner checks.
        CodeDocumentationOneLessonGrantCatalog catalog,CodeDocumentationOneLessonSelectorV1 selector, // Issue replay readback only from this held Session after payload and final owner checks.
        CancellationToken token=default) => SelectionBindingReadback.InspectAndIssue(this,baseline,catalog,selector,token); // Issue replay readback only from this held Session after payload and final owner checks.

    private int _selectionMaterialInFlight; // One finite same-owner preparation/inspection excludes reentrant or concurrent full-packet consumers.
    private ContinuousLearningModelServerStartup? _teachingDisclosureStartup; // Retain the exact protected startup only for the current CAS-held disclosure operation.
    private CodeDocumentationOneLessonTeachingDisclosureApproval? _teachingDisclosureApproval; // Retain the actual catalog approval object whose full graph is being disclosed.
    private CodeDocumentationOneLessonTeachingDisclosureApproval? _teachingDisclosureRevisionInput; // Retain exact catalog membership for final revision checking only; successful resolution still owns disclosure issuance.
    private CodeDocumentationOneLessonSourceSelection.DeclaredTeachingCorpusReadback? _teachingDisclosureGraph; // Retain the authenticated complete graph, including unused owner entries.
    private CodeDocumentationTeachingSelectionInputV1? _teachingDisclosureInput; // Retain complete canonical input and ordered diagnosis for receipt equality checks.
    private byte[]? _teachingDisclosureBinding; // Retain the full canonical receiving binding rather than a caller binding digest.
    private bool _teachingDisclosureIssuing; // Permit private receipt construction only during the authenticated issuing scope.
    private PreparedTeachingDisclosureReceipt? _teachingDisclosureReceipt; // Authenticate one active disclosure receipt by reference during this operation.
    private Exception? _teachingDisclosureRevisionFinalFailure; // Keep the independent revision final exception separate from the original five finals.
    internal bool TeachingDisclosureInspectionRefusalForTesting { get; set; } // Allow the existing test boundary to stage a fixed-inspector refusal without replacing the recipient.
    internal string? TeachingDisclosureRevisionFinalErrorForTesting { get; private set; } // Expose only the last bounded revision-final code/type for exact fault assertions.
    internal string? TeachingDisclosureStagedErrorForTesting { get; private set; } // Expose the staged consumer refusal separately from final owner arbitration.
    internal string? ActiveTeachingDisclosureEvidenceSha256 => _teachingDisclosureApproval?.EvidenceSha256; // Return approval evidence only from this operation's retained approval object.
    internal bool IsActiveTeachingDisclosureContext(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext context) // Authenticate the actual prepared context against current Session receipt and full input.
        => ReferenceEquals(context, _selectionMaterialPrepared) && _teachingDisclosureReceipt?.IsCurrentFor(this) == true && // Require context reference identity and the active genuine disclosure receipt.
           context.CopyBindingBytes().AsSpan().SequenceEqual(_teachingDisclosureBinding) && context.MetadataInput.CopyBytes().AsSpan().SequenceEqual(_teachingDisclosureInput!.CopyBytes()); // Require exact complete binding/input bytes so equivalent hashes cannot substitute a context.

    internal sealed class PreparedTeachingDisclosureReceipt // Keep disclosure authority in an opaque receipt tied to the held Session operation.
    { // Retain actual owners privately; callers cannot reconstruct receipt authority from fields.
        private readonly CodeDocumentationOneLessonSession owner; // Bind receipt lifetime to this concrete Session, not another reopened instance.
        private readonly ContinuousLearningModelServerStartup startup; // Bind provenance and revision to the actual protected startup instance.
        private readonly CodeDocumentationOneLessonTeachingDisclosureApproval approval; // Bind to the actual retained catalog approval, not a copied declaration DTO.
        private readonly CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline; // Retain the same genuine baseline used for authenticated material preparation.
        private readonly CodeDocumentationOneLessonSourceSelection.DeclaredTeachingCorpusReadback graph; // Retain the complete actual graph checked against all approved entries.
        private readonly CodeDocumentationTeachingSelectionInputV1 input; // Retain the canonical full input used to construct the fixed recipient.
        private readonly byte[] binding, prefix; // Own defensive complete receiving binding and prefix snapshots.
        private bool valid = true; // Make receipt invalidation explicit and permanent after operation release.
        private PreparedTeachingDisclosureReceipt(CodeDocumentationOneLessonSession owner) // Construct a receipt only from already-held private issuing fields.
        { // Capture all authenticated operation owners without accepting caller-supplied replacements.
            this.owner = owner; startup = owner._teachingDisclosureStartup!; approval = owner._teachingDisclosureApproval!; // Retain the concrete Session, protected startup and approval objects by reference.
            baseline = owner._ordinaryBaseline!; graph = owner._teachingDisclosureGraph!; input = owner._teachingDisclosureInput!; // Retain the genuine baseline, full graph and canonical input held at issuance.
            binding = owner._teachingDisclosureBinding!.ToArray(); prefix = owner._v2Prefix!.ToArray(); // Detach binding and prefix arrays so later caller edits cannot change receipt snapshots.
        } // Finish private receipt capture without exporting owner handles.
        internal static OperationResult<PreparedTeachingDisclosureReceipt> Issue(CodeDocumentationOneLessonSession owner) // Issue only inside the existing CAS and short authenticated disclosure issuing scope.
        { // Refuse incomplete, disposed or unheld owner state before constructing any receipt.
            if (!owner._teachingDisclosureIssuing || owner._selectionMaterialInFlight != 1 || owner._disposed || owner._teachingDisclosureStartup is null || owner._teachingDisclosureApproval is null || owner._teachingDisclosureGraph is null || owner._teachingDisclosureInput is null || owner._teachingDisclosureBinding is null || owner._ordinaryBaseline is null || owner._v2Prefix is null) return RecoveryRequired<PreparedTeachingDisclosureReceipt>(); // Require every actual owner/input/binding/prefix field and the held issuing flag.
            var revision = owner._teachingDisclosureStartup.CheckProtectedTeachingDisclosureRevision(owner._teachingDisclosureApproval); // Recheck the exact protected startup revision at the instant of issuance.
            return revision.IsSuccess ? OperationResult<PreparedTeachingDisclosureReceipt>.Ok(new(owner)) : revision.MapError<PreparedTeachingDisclosureReceipt>(); // Create the opaque receipt only on revision success, preserving the real refusal otherwise.
        } // Finish issuance without a public factory or serialized authority token.
        internal bool IsCurrentFor(CodeDocumentationOneLessonSession expected) // Check whether this receipt still belongs to the same live held operation.
            => valid && ReferenceEquals(owner, expected) && !owner._disposed && owner._selectionMaterialInFlight == 1 && ReferenceEquals(owner._teachingDisclosureReceipt, this) && // Require permanent validity, exact Session identity, held CAS and active receipt reference.
               ReferenceEquals(owner._teachingDisclosureStartup, startup) && ReferenceEquals(owner._teachingDisclosureApproval, approval) && ReferenceEquals(owner._ordinaryBaseline, baseline) && // Require unchanged startup, approval and genuine baseline reference owners.
               ReferenceEquals(owner._teachingDisclosureGraph, graph) && ReferenceEquals(owner._teachingDisclosureInput, input) && owner._v2Prefix is not null && prefix.AsSpan().SequenceEqual(owner._v2Prefix) && // Require the retained graph/input owners and exact complete prefix continuity.
               binding.AsSpan().SequenceEqual(owner._teachingDisclosureBinding) && startup.CheckProtectedTeachingDisclosureRevision(approval, CancellationToken.None).IsSuccess; // Require full binding equality and current protected revision with None cancellation.
        internal OperationResult<bool> MatchCompleteInput(CodeDocumentationOneLessonSession expected, CodeDocumentationOneLessonSourceSelection.DeclaredTeachingCorpusReadback actual, // Revalidate receipt ownership against the genuine actual material graph before capture.
            CodeDocumentationTeachingSelectionInputV1 actualInput, byte[] canonicalBinding) // Bind the full actual canonical input and receiving bytes, not scalar labels alone.
        { // Match complete approved owners before reconstructing the material recipient.
            if (!IsCurrentFor(expected) || !canonicalBinding.AsSpan().SequenceEqual(binding) || !actualInput.CopyBytes().AsSpan().SequenceEqual(input.CopyBytes())) return CodeDocumentationOneLessonTeachingDisclosureApproval.Refuse(CodeDocumentationOneLessonTeachingDisclosureApproval.OwnerMismatch); // Refuse foreign/inactive receipts and changed full binding/input bytes.
            var matched = approval.MatchCompleteGraph(actual); // Recheck every actual source/entry against the retained operator declaration.
            if (!matched.IsSuccess) return matched; // Preserve genuine graph refusal instead of continuing with partial input.
            var recaptured = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(input.OriginalReceiptId, input.OriginalReceiptSha256, input.Categories, actual); // Reconstruct the full input from actual graph and the retained ordered diagnosis/receipt.
            return recaptured.IsSuccess && recaptured.Value is not null && recaptured.Value.CopyBytes().AsSpan().SequenceEqual(input.CopyBytes()) ? OperationResult<bool>.Ok(true) : CodeDocumentationOneLessonTeachingDisclosureApproval.Refuse(CodeDocumentationOneLessonTeachingDisclosureApproval.Invalid); // Require exact reconstructed input bytes or return the bounded approval-invalid family.
        } // Finish full-input matching without granting selection or training authority.
        internal byte[] CopyApprovalBytes() => approval.CopyBytes(); // Return a defensive complete approval preimage for per-item evidence framing.
        internal void Invalidate() => valid = false; // Permanently invalidate this receipt before releasing the operation's CAS.
    } // End the receipt type without exposing a caller authority constructor.

    internal Task<OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.TeachingDisclosureInspection>> InspectStartupApprovedTeachingSelectionMaterialsAsync( // Expose one fixed protected inspection operation on this genuine Session.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Use the already-authenticated ordinary baseline; do not create a new evaluator or journal.
        ContinuousLearningModelServerStartup startup, CodeDocumentationOneLessonSelectorV1 selector, CancellationToken token = default) // Use the retained startup/selector and caller token for this bounded inspection only.
        => ExecuteSelectionMaterialBoundaryAsync(baseline, startup.OneLessonGrantCatalog, selector, null, SelectedMaterialRecipient.ProtectedStartupApprovalInspection, // Enter the existing selection-material CAS with the fixed protected recipient mode.
            receipt => Task.FromResult(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.InspectStartupApprovedSelectionMaterialContext(this, receipt)), token, startup: startup); // Invoke only DatasetAuthoring's fixed inspector and retain startup for independent final checks.

    internal Task<OperationResult<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>> CaptureStartupApprovedNativeSelectionAsync(
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline,
        ContinuousLearningModelServerStartup startup, CodeDocumentationOneLessonSelectorV1 selector, CancellationToken token = default)
        => ExecuteSelectionMaterialBoundaryAsync(baseline, startup.OneLessonGrantCatalog, selector, null,
            SelectedMaterialRecipient.ProtectedStartupNativePreparation,
            material => CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CaptureStartupApprovedNativeSelectionAsync(this, material, token), token, startup: startup);

    internal Task<OperationResult<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>> CaptureSyntheticNativeStartedFixtureAsync( // Enter the explicit synthetic fixture through the existing protected material lifecycle.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, ContinuousLearningModelServerStartup startup, // Enter the explicit synthetic fixture through the existing protected material lifecycle.
        CodeDocumentationOneLessonSelectorV1 selector, string fixtureDirectory, CancellationToken token = default) // Enter the explicit synthetic fixture through the existing protected material lifecycle.
        => ExecuteSelectionMaterialBoundaryAsync(baseline, startup.OneLessonGrantCatalog, selector, null, SelectedMaterialRecipient.ProtectedStartupNativePreparation, // Enter the explicit synthetic fixture through the existing protected material lifecycle.
            material => CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CaptureStartupApprovedNativeSelectionAsync(this, material, token), token, // Enter the explicit synthetic fixture through the existing protected material lifecycle.
            startup: startup, syntheticNativeLaunchDirectory: fixtureDirectory); // Explicitly labelled internal fixture uses the same full genuine material guards and fixed Host, never the ordinary endpoint.

    internal sealed class NativeSelectionPreparationReceipt
    {
        private readonly CodeDocumentationOneLessonSession owner;
        private readonly PreparedSelectionMaterialReceipt material;
        private readonly PreparedTeachingDisclosureReceipt disclosure;
        private readonly ContinuousLearningModelServerStartup startup;
        private readonly CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline;
        private readonly FileStream heldLock;
        private readonly byte[] binding;
        private bool invalidated;
        private NativeSelectionPreparationReceipt(CodeDocumentationOneLessonSession owner)
        {
            this.owner = owner; material = owner._selectionMaterialReceipt!; disclosure = owner._teachingDisclosureReceipt!;
            startup = owner._teachingDisclosureStartup!; baseline = owner._ordinaryBaseline!; heldLock = owner._ownerLock;
            binding = owner._teachingDisclosureBinding!.ToArray();
        }
        internal static OperationResult<NativeSelectionPreparationReceipt> Issue(CodeDocumentationOneLessonSession owner)
            => owner._nativePreparationIssuing && owner._selectionMaterialInFlight == 1 && !owner._disposed &&
               owner._selectionMaterialReceipt?.IsActiveFor(owner) == true && owner._teachingDisclosureReceipt?.IsCurrentFor(owner) == true &&
               owner._selectionMaterialPrepared is { Origin: "ProtectedStartupOperatorApproval" } && owner._teachingDisclosureBinding is not null
                ? OperationResult<NativeSelectionPreparationReceipt>.Ok(new(owner)) : RecoveryRequired<NativeSelectionPreparationReceipt>();
        internal bool IsActiveFor(CodeDocumentationOneLessonSession expected)
            => !invalidated && ReferenceEquals(owner, expected) && !owner._disposed && owner._selectionMaterialInFlight == 1 &&
               ReferenceEquals(owner._nativePreparationReceipt, this) && ReferenceEquals(owner._selectionMaterialReceipt, material) &&
               ReferenceEquals(owner._teachingDisclosureReceipt, disclosure) && ReferenceEquals(owner._teachingDisclosureStartup, startup) &&
               ReferenceEquals(owner._ordinaryBaseline, baseline) && ReferenceEquals(owner._ownerLock, heldLock) &&
               material.IsActiveFor(owner) && disclosure.IsCurrentFor(owner) && binding.AsSpan().SequenceEqual(owner._teachingDisclosureBinding);
        internal CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext Context => material.Context;
        internal string SessionId => owner._sessionId;
        internal string OwnedDirectory => Path.GetDirectoryName(owner._historyPath)!; // The fixed resource probe never receives a caller/checkout working root.
        internal string ApprovalEvidenceSha256 => owner._teachingDisclosureApproval!.EvidenceSha256;
        internal byte[] CopyApprovalBytes() => disclosure.CopyApprovalBytes();
        internal void Invalidate() => invalidated = true;
    }

    internal sealed class NativeSelectionLaunchBindingV1 // Confine launch capability to this same held Session operation.
    { // Confine launch capability to this same held Session operation.
        private readonly CodeDocumentationOneLessonSession owner; // Keep all authority-bearing owner references, not receipt metadata copies.
        private readonly NativeSelectionPreparationReceipt preparation; // Exact prepared recipient from this held operation.
        private readonly PreparedSelectionMaterialReceipt material; // Exact full permitted material recipient.
        private readonly PreparedTeachingDisclosureReceipt disclosure; // Actual successful resolver-issued disclosure authority.
        private readonly ContinuousLearningModelServerStartup startup; // Exact protected startup owner.
        private readonly CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline; // Exact held saved baseline owner.
        private readonly FileStream heldLock; // Original live Session custody handle.
        private readonly CodeDocumentationNativeCodexSelectionHost host; // Same fixed consumer object that prepared the request.
        private readonly CodeDocumentationNativeCodexLaunchInputOwner inputs; // Retained actual input handles, not JSON/hash authority.
        private readonly byte[] request, schema, binding; // Immutable full prepared/schema/binding preimages.
        private readonly CodeDocumentationNativeCodexSelectionWire.PreparedNativeSelectionData data; // Canonical prepared data is subordinate to all retained owners.
        private bool invalidated; // Invalidation is permanent and precedes CAS release.
        private NativeSelectionLaunchBindingV1(CodeDocumentationOneLessonSession owner) // Freeze exact launch-owner references and prepared preimages.
        { // Freeze exact launch-owner references and prepared preimages.
            this.owner = owner; preparation = owner._nativePreparationReceipt!; material = owner._selectionMaterialReceipt!; disclosure = owner._teachingDisclosureReceipt!; // Capture exact private owner references only in the short issuing scope.
            startup = owner._teachingDisclosureStartup!; baseline = owner._ordinaryBaseline!; heldLock = owner._ownerLock; host = owner._nativeLaunchHost!; inputs = owner._nativeLaunchInputs!; // Keep resources/consumer under the original CAS.
            request = owner._nativePreparedRequestBytes!.ToArray(); schema = owner._nativePreparedSchemaBytes!.ToArray(); binding = owner._teachingDisclosureBinding!.ToArray(); // Freeze complete bytes independently of mutable caller data.
            data = CodeDocumentationNativeCodexSelectionWire.DecodePrepared(request).Value!; // Issuer prechecks already admitted these exact bytes.
        } // Finish this bounded owner check without renewing authority or an attempt.
        internal static OperationResult<NativeSelectionLaunchBindingV1> Issue(CodeDocumentationOneLessonSession owner) // Require the short private issuing scope and genuine active owners.
            => owner._nativeLaunchIssuing && !owner._nativeStartedAttempt && !owner._disposed && owner._selectionMaterialInFlight == 1 && // Require the short private issuing scope and genuine active owners.
                owner._nativePreparationReceipt?.IsActiveFor(owner) == true && owner._nativeLaunchHost is not null && // Require the short private issuing scope and genuine active owners.
                owner._nativeLaunchInputs?.IsCurrentFor(owner) == true && owner._nativePreparedRequestBytes is not null && owner._nativePreparedSchemaBytes is not null && // Require the short private issuing scope and genuine active owners.
                owner._selectionMaterialReceipt?.IsActiveFor(owner) == true && owner._teachingDisclosureReceipt?.IsCurrentFor(owner) == true && // Require the short private issuing scope and genuine active owners.
                owner._teachingDisclosureBinding is not null && owner._teachingDisclosureStartup is not null && owner._ordinaryBaseline is not null // Require the short private issuing scope and genuine active owners.
                    ? OperationResult<NativeSelectionLaunchBindingV1>.Ok(new(owner)) : RecoveryRequired<NativeSelectionLaunchBindingV1>(); // Copied metadata, check-only revision input and caller provider choice cannot mint a capability.
        internal bool IsActiveFor(CodeDocumentationOneLessonSession expected, CodeDocumentationNativeCodexSelectionHost consumer) // Check original owner and fixed-consumer identity before using this capability.
            => !invalidated && ReferenceEquals(owner, expected) && ReferenceEquals(host, consumer) && !owner._disposed && owner._selectionMaterialInFlight == 1 && // Check original owner and fixed-consumer identity before using this capability.
                ReferenceEquals(owner._nativeLaunchBinding, this) && ReferenceEquals(owner._nativeLaunchHost, host) && ReferenceEquals(owner._nativeLaunchInputs, inputs) && // Check original owner and fixed-consumer identity before using this capability.
                ReferenceEquals(owner._nativePreparationReceipt, preparation) && ReferenceEquals(owner._selectionMaterialReceipt, material) && ReferenceEquals(owner._teachingDisclosureReceipt, disclosure) && // Check original owner and fixed-consumer identity before using this capability.
                ReferenceEquals(owner._teachingDisclosureStartup, startup) && ReferenceEquals(owner._ordinaryBaseline, baseline) && ReferenceEquals(owner._ownerLock, heldLock) && // Check original owner and fixed-consumer identity before using this capability.
                preparation.IsActiveFor(owner) && material.IsActiveFor(owner) && disclosure.IsCurrentFor(owner) && inputs.IsCurrentFor(owner) && // Check original owner and fixed-consumer identity before using this capability.
                request.AsSpan().SequenceEqual(owner._nativePreparedRequestBytes) && schema.AsSpan().SequenceEqual(owner._nativePreparedSchemaBytes) && binding.AsSpan().SequenceEqual(owner._teachingDisclosureBinding); // Require current complete held preimages/owners even immediately before consumption.
        internal CodeDocumentationNativeCodexSelectionWire.NativeStartedV1 CopyStartedData() // Project complete frozen launch inputs as data without minting authority.
        { // Project complete frozen launch inputs as data without minting authority.
            var launch = inputs.CopyCanonicalBytes(); // Descriptive data remains detached from the private binding.
            return new(owner._sessionId, data.Nonce, CodeDocumentationNativeCodexSelectionWire.Sha256(request), request.Length, // Project complete frozen launch inputs as data without minting authority.
                CodeDocumentationNativeCodexSelectionWire.Sha256(schema), schema.Length, CodeDocumentationNativeCodexSelectionWire.Sha256(binding), // Project complete frozen launch inputs as data without minting authority.
                CodeDocumentationNativeCodexSelectionWire.Sha256(data.CopyInputBytes()), preparation.ApprovalEvidenceSha256, // Project complete frozen launch inputs as data without minting authority.
                CodeDocumentationNativeCodexLaunchInputOwner.SyntheticKind, Convert.ToBase64String(launch), CodeDocumentationNativeCodexSelectionWire.Sha256(launch)); // Persist exact frozen launch inputs and all current capability bindings.
        } // Finish this bounded owner check without renewing authority or an attempt.
        internal void Invalidate() => invalidated = true; // Called before input disposal and unconditional CAS release.
    } // Finish this bounded owner check without renewing authority or an attempt.

    internal async Task<OperationResult<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>> PrepareNativeSelectionRequestUnderHeldMaterialAsync(
        PreparedSelectionMaterialReceipt material, CancellationToken token)
    {
        if (!material.IsActiveFor(this) || !IsActiveTeachingDisclosureContext(material.Context))
            return RecoveryRequired<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>();
        _nativePreparationIssuing = true; OperationResult<NativeSelectionPreparationReceipt> issued;
        try { issued = NativeSelectionPreparationReceipt.Issue(this); } finally { _nativePreparationIssuing = false; }
        if (!issued.IsSuccess || issued.Value is null) return issued.MapError<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>();
        _nativePreparationReceipt = issued.Value;
        var host = CodeDocumentationNativeCodexSelectionHost.FromHeldSession(this, issued.Value);
        if (!host.IsSuccess || host.Value is null) return host.MapError<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>();
        SelectionMaterialPointForTesting?.Invoke("native-preparation-fixed-consumer"); token.ThrowIfCancellationRequested();
        return await host.Value.PrepareOrReplayAsync(token).ConfigureAwait(false);
    }

    private string NativeRequestPath => Path.Combine(Path.GetDirectoryName(_historyPath)!, CodeDocumentationNativeCodexSelectionWire.RequestFileName);
    private string NativeSchemaPath => Path.Combine(Path.GetDirectoryName(_historyPath)!, CodeDocumentationNativeCodexSelectionWire.SchemaFileName);
    private string NativeStartedPath => Path.Combine(Path.GetDirectoryName(_historyPath)!, CodeDocumentationNativeCodexSelectionWire.StartedFileName); // Keep native attempt evidence in its genuine Session directory.

    internal OperationResult<bool> ConsumeNativeLaunchBoundaryUnderHeldMaterial(NativeSelectionPreparationReceipt receipt, // Resolve the fixed launch-input owner under the original material CAS.
        CodeDocumentationNativeCodexSelectionHost host, CancellationToken token) // Resolve the fixed launch-input owner under the original material CAS.
    { // Resolve the fixed launch-input owner under the original material CAS.
        if (!receipt.IsActiveFor(this) || _nativeStartedAttempt) return RecoveryRequired<bool>(); // Unknown/attempted state cannot regain an attempt or a new nonce.
        var family = CheckNativePreparedFamily(); if (!family.IsSuccess) return family; // Full preparation custody must remain current before resource acquisition.
        var resolved = _syntheticNativeLaunchDirectory is null // Resolve the fixed launch-input owner under the original material CAS.
            ? CodeDocumentationNativeCodexLaunchInputOwner.ResolveInstalledUnderHeldSession(this, receipt, token) // Resolve retained installed resources under this same receipt and token.
            : CodeDocumentationNativeCodexLaunchInputOwner.OpenSyntheticFixtureUnderHeldSession(this, receipt, _syntheticNativeLaunchDirectory); // The public ordinary path cannot choose synthetic input ownership.
        if (!resolved.IsSuccess || resolved.Value is null) return resolved.MapError<bool>(); // Missing semantic enforcement refuses before Started/selection creation; the finite resource probe has its own real Job/process evidence.
        _nativeLaunchInputs = resolved.Value; _nativeLaunchHost = host; _nativeLaunchIssuing = true; OperationResult<NativeSelectionLaunchBindingV1> issued; // Retain exact fixed consumer/resources under the same CAS.
        try { issued = NativeSelectionLaunchBindingV1.Issue(this); } finally { _nativeLaunchIssuing = false; } // Issuance ends before any observation or storage effect.
        if (!issued.IsSuccess || issued.Value is null) return issued.MapError<bool>(); _nativeLaunchBinding = issued.Value; // Only the genuine short-scope issuer can install this binding.
        SelectionMaterialPointForTesting?.Invoke("native-launch-binding-issued"); token.ThrowIfCancellationRequested(); // Observers cannot re-enter the held owner or import a receipt.
        return host.ConsumeStartedUnderHeldSession(issued.Value, token); // The exact Host prepares and consumes in this one active operation.
    } // Finish this bounded owner check without renewing authority or an attempt.

    internal OperationResult<bool> StoreNativeStartedUnderHeldLaunchBinding(CodeDocumentationNativeCodexSelectionHost host, // Reserve and durably confirm the whole native attempt before the creation seam.
        NativeSelectionLaunchBindingV1 launch, CancellationToken token) // Reserve and durably confirm the whole native attempt before the creation seam.
    { // Reserve and durably confirm the whole native attempt before the creation seam.
        if (!launch.IsActiveFor(this, host) || _nativeStartedAttempt || File.Exists(NativeStartedPath)) return RecoveryRequired<bool>(); // A copied/expired/foreign binding or preexisting attempt never gets a second launch.
        var current = CheckNativePreparedFamily(); if (!current.IsSuccess) return current; // Require complete immutable preparation/custody before reservation.
        byte[] started; // Reserve and durably confirm the whole native attempt before the creation seam.
        try { started = CodeDocumentationNativeCodexSelectionWire.WriteStarted(launch.CopyStartedData()); } // Calculate the whole LF-inclusive frame before creating any file.
        catch (CodeDocumentationNativeCodexSelectionWire.PreparationCapacityException) { return CapacityExceeded<bool>(); } // Refuse representation overage without truncating or expanding caps.
        var familyBytes = checked((long)_nativePreparedRequestBytes!.Length + _nativePreparedSchemaBytes!.Length + started.Length); // Charge the complete existing request/schema plus whole Started.
        if (familyBytes > CodeDocumentationNativeCodexSelectionWire.FamilyMaximumBytes || checked(familyBytes + _ownerLock.Length + (_v2History?.Length ?? 0)) > CodeDocumentationNativeCodexSelectionWire.SessionMaximumBytes) return CapacityExceeded<bool>(); // Preserve physical family and aggregate Session bounds before file creation.
        token.ThrowIfCancellationRequested(); if (!launch.IsActiveFor(this, host)) return RecoveryRequired<bool>(); // Recheck exact retained launch inputs after reservation.
        _nativeStartedAttempt = true; // A partial/unknown attempt consumes its slot permanently, even when no process can be created.
        _nativeStartedFile = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(NativeStartedPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, FileOptions.WriteThrough); // Never append over or replace unknown evidence.
        if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_nativeStartedFile.SafeFileHandle, NativeStartedPath)) return RecoveryRequired<bool>(); // Reject changed custody before writing.
        SelectionMaterialPointForTesting?.Invoke("native-started-file-created"); token.ThrowIfCancellationRequested(); // An interruption leaves the physical partial file and charged attempt intact.
        _nativeStartedFile.Write(started); _nativeStartedFile.Flush(true); // Durable full Started precedes the first process-creation seam, including suspended creation.
        SelectionMaterialPointForTesting?.Invoke("native-started-flushed"); token.ThrowIfCancellationRequested(); // A flushed but unconfirmed append remains unknown.
        var readback = ReadNativePreparedFile(_nativeStartedFile, NativeStartedPath, CodeDocumentationNativeCodexSelectionWire.StartedMaximumBytes); // Read the complete stable retained file before any process seam.
        if (!readback.IsSuccess || readback.Value is null) return readback.MapError<bool>(); // Partial/unstable/overbound data never reaches process creation.
        if (!readback.Value.AsSpan().SequenceEqual(started) || !CodeDocumentationNativeCodexSelectionWire.DecodeStarted(readback.Value).IsSuccess) return RecoveryRequired<bool>(); // Both full byte equality and closed grammar are mandatory.
        _nativeStartedBytes = readback.Value; SelectionMaterialPointForTesting?.Invoke("native-started-readback-staged"); token.ThrowIfCancellationRequested(); // Advance only from confirmed own complete readback.
        if (!launch.IsActiveFor(this, host)) return RecoveryRequired<bool>(); // Changed image/config/permission/binding after Started refuses and preserves unknown evidence.
        return OperationResult<bool>.Ok(true); // This proves durable storage only; it grants no installed process permission.
    } // Finish this bounded owner check without renewing authority or an attempt.

    internal sealed record NativePreparationReplaySeed(int WireVersion, string Nonce);
    internal CodeDocumentationNativeCodexWindowsProcess.Observation? LastInstalledNativeResourceObservationForTesting { get; private set; }
    internal void RecordInstalledNativeResourceObservationUnderHeldSession(NativeSelectionPreparationReceipt receipt,
        CodeDocumentationNativeCodexWindowsProcess.Observation observation)
    {
        if (!receipt.IsActiveFor(this)) throw new InvalidOperationException("An installed resource observation requires the active held Session.");
        LastInstalledNativeResourceObservationForTesting = observation with
        { StandardOutput = observation.StandardOutput.ToArray(), StandardError = observation.StandardError.ToArray() };
    }
    internal OperationResult<NativePreparationReplaySeed> ReadOrAllocateNativePreparationNonceUnderHeldMaterial(NativeSelectionPreparationReceipt receipt)
    {
        if (!receipt.IsActiveFor(this) || _nativeStartedAttempt || File.Exists(NativeStartedPath)) return RecoveryRequired<NativePreparationReplaySeed>(); // A same-owner replay cannot renew an attempted/unknown launch slot or nonce.
        var current = CheckNativePreparedFamily();
        if (!current.IsSuccess) return current.MapError<NativePreparationReplaySeed>();
        if (_nativePreparedRequestBytes is not null)
        {
            var decoded = CodeDocumentationNativeCodexSelectionWire.DecodePrepared(_nativePreparedRequestBytes);
            return decoded.IsSuccess && decoded.Value is not null ? OperationResult<NativePreparationReplaySeed>.Ok(new(decoded.Value.SchemaVersion, decoded.Value.Nonce)) : decoded.MapError<NativePreparationReplaySeed>();
        }
        _nativePreparationNonce ??= Guid.NewGuid().ToString("N");
        return OperationResult<NativePreparationReplaySeed>.Ok(new(2, _nativePreparationNonce));
    }

    internal OperationResult<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection> StoreNativePreparationUnderHeldMaterial(
        NativeSelectionPreparationReceipt receipt, byte[] request, CancellationToken token)
    {
        if (!receipt.IsActiveFor(this)) return RecoveryRequired<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>();
        var current = CheckNativePreparedFamily();
        if (!current.IsSuccess) return current.MapError<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>();
        var schema = CodeDocumentationNativeCodexSelectionWire.CopySchemaBytes();
        if (request.Length > CodeDocumentationNativeCodexSelectionWire.RequestMaximumBytes ||
            checked((long)request.Length + schema.Length) > CodeDocumentationNativeCodexSelectionWire.FamilyMaximumBytes ||
            checked((long)request.Length + schema.Length + _ownerLock.Length + (_v2History?.Length ?? 0)) > CodeDocumentationNativeCodexSelectionWire.SessionMaximumBytes)
            return CapacityExceeded<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>();
        var decoded = CodeDocumentationNativeCodexSelectionWire.DecodePrepared(request);
        if (!decoded.IsSuccess || decoded.Value is null || !MatchesNativePreparedBinding(decoded.Value))
            return RecoveryRequired<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>();
        var replayed = _nativePreparedRequestBytes is not null;
        if (replayed)
        {
            if (!request.AsSpan().SequenceEqual(_nativePreparedRequestBytes) || !schema.AsSpan().SequenceEqual(_nativePreparedSchemaBytes))
                return RecoveryRequired<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>();
        }
        else
        {
            token.ThrowIfCancellationRequested();
            _nativePreparedPoisoned = true; // Any partial/ambiguous write remains evidence, never a fresh retry.
            _nativePreparedRequest = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(NativeRequestPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, FileOptions.WriteThrough);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_nativePreparedRequest.SafeFileHandle, NativeRequestPath))
                return RecoveryRequired<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>();
            _nativePreparedRequest.Write(request); _nativePreparedRequest.Flush(true);
            SelectionMaterialPointForTesting?.Invoke("native-preparation-request-flushed"); token.ThrowIfCancellationRequested();
            _nativePreparedSchema = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(NativeSchemaPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, FileOptions.WriteThrough);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_nativePreparedSchema.SafeFileHandle, NativeSchemaPath))
                return RecoveryRequired<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>();
            _nativePreparedSchema.Write(schema); _nativePreparedSchema.Flush(true);
            var first = ReadNativePreparedFile(_nativePreparedRequest, NativeRequestPath, CodeDocumentationNativeCodexSelectionWire.RequestMaximumBytes);
            var second = ReadNativePreparedFile(_nativePreparedSchema, NativeSchemaPath, CodeDocumentationNativeCodexSelectionWire.FamilyMaximumBytes - request.Length);
            if (!first.IsSuccess || first.Value is null) return first.MapError<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>();
            if (!second.IsSuccess || second.Value is null) return second.MapError<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>();
            if (!first.Value.AsSpan().SequenceEqual(request) || !second.Value.AsSpan().SequenceEqual(schema))
                return RecoveryRequired<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>();
            _nativePreparedRequestBytes = first.Value; _nativePreparedSchemaBytes = second.Value; _nativePreparedPoisoned = false;
        }
        var final = CheckNativePreparedFamily();
        if (!final.IsSuccess) return final.MapError<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>();
        if (!receipt.IsActiveFor(this)) return RecoveryRequired<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>();
        SelectionMaterialPointForTesting?.Invoke("native-preparation-readback-staged"); token.ThrowIfCancellationRequested();
        return OperationResult<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>.Ok(new(true, replayed,
            CodeDocumentationNativeCodexSelectionWire.Sha256(request), request.Length, CodeDocumentationNativeCodexSelectionWire.Sha256(schema), schema.Length, receipt.Context.Candidates.Count));
    }

    private bool MatchesNativePreparedBinding(CodeDocumentationNativeCodexSelectionWire.PreparedNativeSelectionData data)
        => data.SessionId == _sessionId && data.CopyBindingBytes().AsSpan().SequenceEqual(
            CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(SelectionProjection(_snapshot.Binding))));

    private static OperationResult<byte[]> ReadNativePreparedFile(FileStream stream, string path, long bound)
    {
        if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path) ||
            SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) || !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path)) return RecoveryRequired<byte[]>();
        if (stream.Length > bound) return CapacityExceeded<byte[]>();
        if (stream.Length == 0) return RecoveryRequired<byte[]>();
        var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, bound);
        if (bytes is null || bytes.Length == 0 || !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path)) return RecoveryRequired<byte[]>();
        return OperationResult<byte[]>.Ok(bytes);
    }

    private OperationResult<bool> CheckNativePreparedFamily()
    {
        if (_disposed || _nativePreparedPoisoned || !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_ownerLock.SafeFileHandle, _lockPath)) return RecoveryRequired<bool>();
        var directory = Path.GetDirectoryName(_historyPath)!;
        if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(directory) || Directory.GetFileSystemEntries(directory).Any(p =>
            Directory.Exists(p) || !SamePath(p, _lockPath) && !SamePath(p, _historyPath) && !SamePath(p, NativeRequestPath) && !SamePath(p, NativeSchemaPath) && !SamePath(p, NativeStartedPath))) return RecoveryRequired<bool>(); // The separately checked native attempt never changes legacy/Synthetic grammar.
        if (File.Exists(NativeStartedPath) && !_nativeStartedAttempt) return RecoveryRequired<bool>(); // An unowned preexisting Started cannot be adopted by a prepared owner.
        if (_nativePreparedRequestBytes is null)
            return _nativePreparedRequest is null && _nativePreparedSchema is null && _nativePreparedSchemaBytes is null &&
                !File.Exists(NativeRequestPath) && !File.Exists(NativeSchemaPath) ? OperationResult<bool>.Ok(true) : RecoveryRequired<bool>();
        if (_nativePreparedRequest is null || _nativePreparedSchema is null || _nativePreparedSchemaBytes is null) return RecoveryRequired<bool>();
        var request = ReadNativePreparedFile(_nativePreparedRequest, NativeRequestPath, CodeDocumentationNativeCodexSelectionWire.RequestMaximumBytes);
        var schema = ReadNativePreparedFile(_nativePreparedSchema, NativeSchemaPath, CodeDocumentationNativeCodexSelectionWire.FamilyMaximumBytes - _nativePreparedRequest.Length);
        if (!request.IsSuccess || request.Value is null) return request.MapError<bool>();
        if (!schema.IsSuccess || schema.Value is null) return schema.MapError<bool>();
        if (checked(_nativePreparedRequest.Length + _nativePreparedSchema.Length + (_nativeStartedFile?.Length ?? 0)) > CodeDocumentationNativeCodexSelectionWire.FamilyMaximumBytes || // Reserve and durably confirm the whole native attempt before the creation seam.
            checked(_nativePreparedRequest.Length + _nativePreparedSchema.Length + (_nativeStartedFile?.Length ?? 0) + _ownerLock.Length + (_v2History?.Length ?? 0)) > CodeDocumentationNativeCodexSelectionWire.SessionMaximumBytes) return CapacityExceeded<bool>(); // Charge retained physical Started even when its effect is unknown.
        var decoded = CodeDocumentationNativeCodexSelectionWire.DecodePrepared(request.Value);
        return decoded.IsSuccess && decoded.Value is not null && MatchesNativePreparedBinding(decoded.Value) &&
            request.Value.AsSpan().SequenceEqual(_nativePreparedRequestBytes) && schema.Value.AsSpan().SequenceEqual(_nativePreparedSchemaBytes) &&
            schema.Value.AsSpan().SequenceEqual(CodeDocumentationNativeCodexSelectionWire.CopySchemaBytes()) ? OperationResult<bool>.Ok(true) : RecoveryRequired<bool>();
    }
    private OperationResult<bool> CheckNativeStartedFamily() // Independently validate retained partial or complete native attempt evidence.
    { // Independently validate retained partial or complete native attempt evidence.
        if (!_nativeStartedAttempt) return !File.Exists(NativeStartedPath) && _nativeStartedFile is null && _nativeStartedBytes is null ? OperationResult<bool>.Ok(true) : RecoveryRequired<bool>(); // No data record can initialize native attempt ownership.
        if (_nativeStartedFile is null || _nativeStartedBytes is null) return RecoveryRequired<bool>(); // A partial append stays recovery-required without cleanup/reset.
        var actual = ReadNativePreparedFile(_nativeStartedFile, NativeStartedPath, CodeDocumentationNativeCodexSelectionWire.StartedMaximumBytes); // Independently check full stable attempt custody with None semantics.
        if (!actual.IsSuccess || actual.Value is null) return actual.MapError<bool>(); // Preserve actual overage versus incomplete/unstable classifications.
        var decoded = CodeDocumentationNativeCodexSelectionWire.DecodeStarted(actual.Value); // Closed decoding remains data only.
        return decoded.IsSuccess && decoded.Value is not null && decoded.Value.SessionId == _sessionId && actual.Value.AsSpan().SequenceEqual(_nativeStartedBytes) // Independently validate retained partial or complete native attempt evidence.
            ? OperationResult<bool>.Ok(true) : RecoveryRequired<bool>(); // Exact frozen Started is required even after a primary failure/cancellation.
    } // Finish this bounded owner check without renewing authority or an attempt.
    private bool _selectionMaterialIssuing; // Only the completed preparation boundary may mint its private receipt.
    private CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext? _selectionMaterialPrepared; // Retain the exact prepared context solely during this operation.
    private PreparedSelectionMaterialReceipt? _selectionMaterialReceipt; // Authenticate the one active consumer by reference, never by caller hashes.
    private SelectionMaterialFinalSnapshot? _selectionMaterialFinalSnapshot; // Retain authenticated detached check inputs before the consumer can change receiving metadata.
    private Exception? _selectionMaterialReceivingFinalFailure, _selectionMaterialBaselineFinalFailure, _selectionMaterialSourceFinalFailure; // Retain independent actual owner exceptions until diagnostics transfer to the primary.
    internal Action<string>? SelectionMaterialPointForTesting { get; set; } // Portable faults observe the real recipient/final check sequence without changing ordinary composition.
    internal string? SelectionMaterialSecondaryErrorForTesting { get; private set; } // Keep a bounded secondary refusal visible while preserving the primary exception object/token.
    internal string? SelectionMaterialReceivingFinalErrorForTesting { get; private set; } // Report the receiving check's bounded code/type without disclosing protected material.
    internal string? SelectionMaterialBaselineFinalErrorForTesting { get; private set; } // Preserve the existing saved-baseline owner's lower code even when receiving memory also fails.
    internal string? SelectionMaterialSourceFinalErrorForTesting { get; private set; } // Report independent complete ORIGINAL/SOURCE refusal even when receiving ownership failed first.

    private sealed class SelectionMaterialFinalSnapshot // These private check inputs cannot issue a receipt, choice or effect capability.
    { // Detach the authenticated pre-consumer owners instead of rereading substituted baseline/grant fields during finally.
        private readonly string root; // Keep the already authenticated repository root.
        private readonly CodeDocumentationOneLessonCorpusSelection originalSelection; // Own copied complete ORIGINAL pins and ordered case IDs.
        private readonly CodeDocumentationOneLessonSourceSelection source; // Own copied complete SOURCE descriptors, links and curriculum.
        private readonly FrozenReferenceCorpus original; // Keep a rebuilt complete immutable ORIGINAL solely for private exclusion checks.
        private readonly CodeDocumentationSavedModelAssessmentGenerationSettings generation; // Copy all scalar settings required by the existing strict ORIGINAL owner.
        private readonly IReadOnlyList<CodeDocumentationSavedModelAssessmentCaseResult> cases; // Copy every actual representative result before any consumer callback.
        private readonly IReadOnlyList<CodeDocumentationSavedModelAssessmentCalibrationObservation> observations; // Copy every repeat and its nested identity lists.
        private readonly CodeDocumentationOneLessonGrantCatalog baselineCatalog; // Freeze the authenticated resolver before a recipient can substitute the live grant.
        private readonly CodeDocumentationOneLessonSelectorV1 baselineSelector; // Retain detached lookup keys used only by existing-only journal inspection.
        private readonly CodeDocumentationPreparedBaselineExternalReference baselineReference; // Copy expected scalars; only the journal's fresh custody read authenticates them.
        private SelectionMaterialFinalSnapshot(string root, CodeDocumentationOneLessonCorpusSelection selected, CodeDocumentationOneLessonSourceSelection source, FrozenReferenceCorpus original, // Construction follows authenticated copying and complete identity equality.
            CodeDocumentationSavedModelAssessmentGenerationSettings generation, IReadOnlyList<CodeDocumentationSavedModelAssessmentCaseResult> cases, IReadOnlyList<CodeDocumentationSavedModelAssessmentCalibrationObservation> observations, // Closed inputs stay inside this private transient owner.
            CodeDocumentationOneLessonGrantCatalog baselineCatalog, CodeDocumentationOneLessonSelectorV1 baselineSelector, CodeDocumentationPreparedBaselineExternalReference baselineReference) // Frozen lookup and expectations never grant creation, measurement or effects.
        { this.root = root; originalSelection = selected; this.source = source; this.original = original; this.generation = generation; this.cases = cases; this.observations = observations; this.baselineCatalog = baselineCatalog; this.baselineSelector = baselineSelector; this.baselineReference = baselineReference; } // Retain only detached check data, no client or permit.
        internal static OperationResult<SelectionMaterialFinalSnapshot> Capture(CodeDocumentationOneLessonSession owner, CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector) // Called only after the new operation authenticates the held receiving owner.
        { // Copy authenticated lookup inputs for the existing-only check before raw extraction and for independent final attempts.
            if (owner._earlyGrant?.CorpusSelection is null || owner._earlyGrant.SourceSelection?.TeachingCurriculum is null || owner._ordinaryStart is null) return Failure<SelectionMaterialFinalSnapshot>("ONE_LESSON_TEACHING_IDENTITIES_UNAVAILABLE", "The startup grant has no declared teaching identities."); // Absent teaching supplies no authenticated material snapshot or arbitrary reopen.
            var selected = CodeDocumentationOneLessonCorpusSelection.Create(owner._earlyGrant!.CorpusSelection!.Corpus, owner._earlyGrant.CorpusSelection.CaseIds); // Reuse the immutable descriptor factory to detach all ORIGINAL pins.
            var frozenSource = CodeDocumentationOneLessonSourceSelection.Create(owner._earlyGrant.SourceSelection!.Sources, owner._earlyGrant.SourceSelection.PairSelections, owner._earlyGrant.SourceSelection.TeachingCurriculum); // Detach the complete SOURCE, including unused rows and order.
            var corpus = baseline.Context.Corpus; var frozenOriginal = FrozenReferenceCorpus.Build(corpus.CorpusName, corpus.CorpusVersion, corpus.FrozenAtUtc, corpus.Entries.Select(e => e with { })); // Rebuild copied immutable entries through the genuine canonical owner.
            if (!selected.IsSuccess || selected.Value is null || !frozenSource.IsSuccess || frozenSource.Value is null || !frozenOriginal.IsSuccess || frozenOriginal.Value is null || // Partial copying never enables material reopens.
                frozenOriginal.Value.CorpusSha256 != corpus.CorpusSha256 || frozenOriginal.Value.CanonicalRepresentation != corpus.CanonicalRepresentation || // Bind the complete protected contents to the already authenticated producer.
                corpus.CorpusSha256 != baseline.Context.CorpusManifest.CorpusSha256 || frozenSource.Value.SourceSelectionSha256 != owner._earlyGrant.SourceSelection.SourceSelectionSha256) return RecoveryRequired<SelectionMaterialFinalSnapshot>(); // Do not retain a mismatching snapshot or expose closed identity values.
            var calibration = baseline.Readback.Baseline.Calibration; // Read actual saved observations only while receiving fields are authenticated.
            var copiedCases = Array.AsReadOnly(calibration.RepresentativeAssessment.Cases.Select(c => c with { }).ToArray()); // Separate each record and the outer collection from later substitutions.
            var copiedObservations = Array.AsReadOnly(calibration.Calibration.Observations.Select(o => o with { CaseIds = Array.AsReadOnly(o.CaseIds.ToArray()), StudentAnswerSha256 = Array.AsReadOnly(o.StudentAnswerSha256.ToArray()), ObservedEvaluationRequestIds = Array.AsReadOnly(o.ObservedEvaluationRequestIds.ToArray()) }).ToArray()); // Detach all repeat collections, not only the outer array.
            var frozenCatalog = CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([owner._earlyGrant!], catalog.MaxTeacherCalls); // Reuse the existing defensive attachment factory on the already authenticated held grant.
            var frozenSelector = selector with { }; var resolved = frozenCatalog.Resolve(frozenSelector); // Preserve actual lookup keys and reject a failed attachment copy before raw extraction.
            if (!resolved.IsSuccess || resolved.Value is null) return resolved.MapError<SelectionMaterialFinalSnapshot>(); // No substituted or incomplete resolver can redirect later journal access.
            var reference = new CodeDocumentationPreparedBaselineExternalReference(baseline.ExternalReference.ReceiptId, baseline.ExternalReference.ReceiptSha256); // Freeze the authenticated expected ORIGINAL reference before consumer code runs.
            var snapshot = new SelectionMaterialFinalSnapshot(owner._ordinaryStart!.RepositoryRoot, selected.Value, frozenSource.Value, frozenOriginal.Value, baseline.Context.GenerationSettings with { }, copiedCases, copiedObservations, frozenCatalog, frozenSelector, reference); // Keep the validated root/settings/owners independent of later receiving changes.
            return OperationResult<SelectionMaterialFinalSnapshot>.Ok(snapshot); // Retain the authenticated lookup even if its next existing-only disk check refuses; final checks must reopen that same owner.
        } // No mutable receiving record is consulted by these later material checks.
        internal OperationResult<bool> CheckOriginal() // Independently reopen the complete frozen ORIGINAL with no caller cancellation.
        { // OpenFresh validates every manifest/payload/address/case pin through the existing strict owner.
            var opened = originalSelection.OpenFresh(root, generation); // Use only the detached authenticated inputs.
            if (!opened.IsSuccess) return opened.MapError<bool>(); // Preserve the actual owner refusal.
            return opened.Value is not null && opened.Value.Corpus.CorpusSha256 == original.CorpusSha256 && opened.Value.Corpus.CanonicalRepresentation == original.CanonicalRepresentation // Compare every protected canonical cell, including unused originals.
                ? OperationResult<bool>.Ok(true) : RecoveryRequired<bool>(); // No protected object or text escapes this final check.
        } // The receiving check's outcome never gates this attempt.
        internal OperationResult<CodeDocumentationOneLessonSourceSelection.DeclaredTeachingCorpusReadback> CheckSource() // Independently reopen all frozen SOURCE owners against complete ORIGINAL exclusion inputs.
            => source.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal(root, original, cases, observations, CancellationToken.None); // This traversal does not consult the potentially changed Session/baseline/grant.
        internal OperationResult<bool> CheckBaselineReference() // Reopen the exact saved ORIGINAL transaction without model, evaluator or baseline creation.
            => new CodeDocumentationRepositoryOwnedPreparedBaselineJournal().InspectExistingOrdinaryExternalReferenceAsync(baselineCatalog, baselineSelector, root, baselineReference, CancellationToken.None).GetAwaiter().GetResult(); // Existing-only owner uses zero-time gate acquisition, claim/baseline rereads and detached frozen expectations.
    } // Transient private data is released with the receipt and CAS on every path.

    internal sealed class PreparedSelectionMaterialReceipt // Context data alone cannot mint same-Session invocation authority.
    { // Private receiving identity and prefix bind complete input to this held owner.
        private bool invalidated; // Track invalidation of the existing material receipt for protected-operation cleanup.
        internal void Invalidate() => invalidated = true; // Make protected release invalidate material lineage without changing ordinary Synthetic lifetime.
        private readonly CodeDocumentationOneLessonSession owner; // Do not expose the held owner or its handles to the callback.
        private readonly byte[] prefix; // A later append/reopen invalidates the old receipt for this issuing instance.
        internal CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext Context { get; } // Expose only the complete immutable context-only recipient.
        private PreparedSelectionMaterialReceipt(CodeDocumentationOneLessonSession owner, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext context) // The checked private issuer is the sole constructor caller.
        { // Freeze the exact complete context and current prefix before consumption.
            this.owner = owner; Context = context; prefix = owner._v2Prefix!.ToArray(); // Caller edits cannot change the receipt's receiving snapshot.
        } // No durable choice, permit or native origin is issued.
        internal bool IsIssuedBy(CodeDocumentationOneLessonSession expected) // A foreign, disposed or changed issuing owner cannot reuse the receipt.
            => !invalidated && ReferenceEquals(owner, expected) && !owner._disposed && owner._v2Prefix is not null && prefix.AsSpan().SequenceEqual(owner._v2Prefix) && // Preserve exact prefix continuity on the same live receiving instance.
                Context.BindingSha256 == ComputeBindingSha256(owner._snapshot.Binding); // Include the full receiving binding rather than an objective label alone.
        internal bool IsActiveFor(CodeDocumentationOneLessonSession expected) // Only Session's guarded operation may dispatch this complete packet.
            => IsIssuedBy(expected) && owner._selectionMaterialInFlight == 1 && ReferenceEquals(owner._selectionMaterialReceipt, this) && // A returned data receipt cannot start an unguarded provider callback.
                ReferenceEquals(owner._selectionMaterialPrepared, Context); // Only the exact currently prepared recipient is active.
        internal static OperationResult<PreparedSelectionMaterialReceipt> Issue(CodeDocumentationOneLessonSession owner, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext context) // Internal access still cannot skip the private transient issuer state.
            => owner._selectionMaterialIssuing && owner._selectionMaterialInFlight == 1 && ReferenceEquals(owner._selectionMaterialPrepared, context) && !owner._disposed // Require completed receiving checks and exact context identity.
                ? OperationResult<PreparedSelectionMaterialReceipt>.Ok(new(owner, context)) : RecoveryRequired<PreparedSelectionMaterialReceipt>(); // Do not promote a caller-created context or digest into Session authority.
    } // The same receipt type serves pure preparation and guarded Synthetic inspection.

    private OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext> PrepareSelectionMaterialCore( // Shared core does not acquire CAS recursively inside inspection.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog, // Reuse the exact held saved ORIGINAL and current grant.
        CodeDocumentationOneLessonSelectorV1 selector, IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>? admissions, SelectedMaterialRecipient recipient, CancellationToken token) // Only fixed private wrappers choose the raw material recipient.
    { // Verify every admission before calling the new raw batch extraction owner.
        var owned = CheckOrdinaryDiagnosticOwnership(baseline, catalog, selector); // Gate even descriptive teaching access on the genuine receiving owner.
        if (!owned.IsSuccess) return owned.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(); // Never pass a foreign/disposed baseline into the recipient.
        token.ThrowIfCancellationRequested(); var copied = admissions?.ToArray(); // Detach caller outer aliases before policy and membership checks.
        var existing = _selectionMaterialFinalSnapshot?.CheckBaselineReference() ?? RecoveryRequired<bool>(); // Reuse detached trusted expectations to authenticate saved claim/baseline before any new raw extraction.
        if (!existing.IsSuccess) return existing.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(); // Preserve Busy/Recovery from the existing-only journal owner; do not create or measure a replacement.
        var diagnosis = ClassifyOrdinaryBaseline(baseline, catalog, selector, token); // Sanitize through the existing owner rather than caller categories.
        if (!diagnosis.IsSuccess || diagnosis.Value is null) return diagnosis.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(); // Preserve sufficient/missing-diagnosis branches.
        var witness = InspectOrdinaryOriginalTeachingExclusion(baseline, catalog, selector, token); // Guard all ORIGINAL/teaching identities and unused raw rows privately before new payload extraction.
        if (!witness.IsSuccess || witness.Value is null) return witness.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(); // Actual custody/closed alias refusal takes precedence.
        var input = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(diagnosis.Value.OriginalReceiptId, diagnosis.Value.OriginalReceiptSha256, diagnosis.Value.Categories, witness.Value.TeachingIdentities); // Capture the complete actual canonical graph and sanitized order.
        if (!input.IsSuccess || input.Value is null) return input.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(); // Invalid graph input cannot be completed with raw caller material.
        var eligible = witness.Value.TeachingIdentities.CategoryBindings.Where(c => input.Value.Categories.Contains(c.Category, StringComparer.Ordinal)).SelectMany(c => c.OrderedItemIds).ToHashSet(StringComparer.Ordinal); // Compute membership only; declared orders remain in the full input.
        var items = witness.Value.TeachingIdentities.Items.Where(i => eligible.Contains(i.DeclaredItem.ItemId)).ToArray(); // Retain complete eligible item order without choosing a task.
        if (recipient is SelectedMaterialRecipient.ProtectedStartupApprovalInspection or SelectedMaterialRecipient.ProtectedStartupNativePreparation) // Both fixed protected recipients use identical approval and raw-owner guards.
            return PrepareStartupApprovedSelectionMaterialCore(baseline, witness.Value.TeachingIdentities, input.Value, witness.Value.BindingSha256, token); // Pass the genuine witness/full input to protected preparation before Synthetic admission matching.
        if (items.Length == 0 || copied is null || copied.Length != items.Length || copied.Any(a => a is null) || // Null/missing/extra capabilities never reach the new raw operation.
            copied.Select(a => a.Item.ItemId).Distinct(StringComparer.Ordinal).Count() != copied.Length) // Duplicates cannot hide a missing eligible admission.
            return Failure<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, "Every eligible item requires one opaque admission."); // Return bounded refusal with no raw material.
        foreach (var item in items) // Validate all capabilities before obtaining any new task/reference payload.
        { // Do not silently drop a denied candidate or substitute a first eligible item.
            var matches = copied.Where(a => a.Item == item.DeclaredItem).ToArray(); // Bind all exact source/entry/fingerprint endpoints.
            if (matches.Length != 1) return Failure<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, "Candidate admission does not match the complete eligible set."); // Foreign or extra items refuse the entire packet.
            var check = matches[0].MatchesSelectionInput(witness.Value.BindingSha256, input.Value, item.DeclaredItem); // Reuse existing purpose/policy and complete privately captured context semantics.
            if (!check.IsSuccess) return check.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(); // Preserve exact license/quarantine/context refusal.
        } // No new raw payload extraction occurred before every capability passed.
        if (recipient != SelectedMaterialRecipient.SyntheticForTesting || copied.Any(a => a.Origin != CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialOrigin.SyntheticTest)) // Exact matching of every candidate does not turn synthetic or unknown lineage into ordinary disclosure rights.
            return Failure<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, "Teaching material admissions do not authorize this recipient."); // Refuse before new raw Open/Capture while retaining all independent final-owner checks.
        var calibration = baseline.Readback.Baseline.Calibration; // Keep all protected representative/repeat observations private.
        var exposed = copied.SelectMany(a => a.Governance.EvidenceRefs.Append(a.EvidenceSha256)).Append(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialPolicy).ToArray(); // Guard all newly exposed admission/policy metadata against closed aliases.
        var guarded = _earlyGrant!.SourceSelection!.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal(_ordinaryStart!.RepositoryRoot, baseline.Context.Corpus, calibration.RepresentativeAssessment.Cases, calibration.Calibration.Observations, token, exposed); // Existing strict exclusion remains mandatory before the new raw boundary.
        if (!guarded.IsSuccess || guarded.Value is null) return guarded.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(); // No evidence-string laundering or partial raw continuation.
        var material = _earlyGrant.SourceSelection.OpenTeachingSelectionMaterialsExcludingOriginal(_ordinaryStart.RepositoryRoot, input.Value.Categories, baseline.Context.Corpus, calibration.RepresentativeAssessment.Cases, calibration.Calibration.Observations, token); // Obtain real eligible bytes only after all opaque admissions pass.
        if (!material.IsSuccess || material.Value is null) return material.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(); // Discard every staged cell on strict owner refusal.
        var bindingBytes = CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(SelectionProjection(_snapshot.Binding))); // Reuse the existing full binding canonical writer without inventing a public frame.
        var captured = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CaptureSelectionMaterialContext(material.Value, witness.Value.BindingSha256, bindingBytes, input.Value, copied); // Reach the actual full existing authoring recipient.
        token.ThrowIfCancellationRequested(); return captured; // Recipient-triggered cancellation remains primary before any receipt issuance.
    } // No exposure, model, selection append, permit, authoring call or optimizer entry occurs.

    private OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext> PrepareStartupApprovedSelectionMaterialCore( // Prepare the full recipient from retained approval and actual owners under the held CAS.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Keep the same genuine ordinary baseline throughout protected preparation.
        CodeDocumentationOneLessonSourceSelection.DeclaredTeachingCorpusReadback graph, CodeDocumentationTeachingSelectionInputV1 input, string binding, CancellationToken token) // Use authenticated full graph/input/binding and caller cancellation, never caller raw candidate DTOs.
    { // Validate fresh owner state and approval before opening a new full raw material batch.
        if (_teachingDisclosureStartup is null || _earlyGrant is null || _v2Prefix is null || _v2Prefix.Length != 0 || _selectionAttemptStarted || _selectionInFlight is not null || HasSavedSelectionBinding || _studentAttemptStarted || _fullMaterialPoisoned || _fullMaterialCompleted is not null || _fullMaterialDispatch is not null) // Reject saved/started/poisoned/nonempty or incomplete owner state for fresh disclosure.
            return CodeDocumentationOneLessonTeachingDisclosureApproval.Refuse(CodeDocumentationOneLessonTeachingDisclosureApproval.OwnerMismatch).MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(); // Return exact owner mismatch without opening raw material on an invalid state.
        _teachingDisclosureRevisionInput = _teachingDisclosureStartup.OneLessonGrantCatalog.FindTeachingApproval(_earlyGrant); // Preserve the genuine check input even when current revision refuses before disclosure authority exists.
        var resolved = _teachingDisclosureStartup.ResolveProtectedTeachingDisclosureApproval(_earlyGrant); // Resolve approval only from this startup and the exact retained early grant.
        if (!resolved.IsSuccess || resolved.Value is null) return resolved.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(); // Preserve approval absence/membership/revision refusal before exposing material.
        _teachingDisclosureApproval = resolved.Value; // Retain that actual approval object for receipt issuance and independent revision finals.
        var matched = resolved.Value.MatchCompleteGraph(graph); // Match all approved sources/entries against the actual complete witness graph.
        if (!matched.IsSuccess) return matched.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(); // Withhold full material when any approved owner field differs.
        var calibration = baseline.Readback.Baseline.Calibration; // Use the genuine frozen baseline calibration to guard ORIGINAL exclusion.
        var guarded = _earlyGrant.SourceSelection!.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal(_ordinaryStart!.RepositoryRoot, baseline.Context.Corpus, // Reopen declared source identities through the existing full ORIGINAL exclusion owner.
            calibration.RepresentativeAssessment.Cases, calibration.Calibration.Observations, token, resolved.Value.ExposedMetadata().Append(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialPolicy).ToArray()); // Include real baseline cases/observations and every approval metadata scalar in alias checking.
        if (!guarded.IsSuccess || guarded.Value is null) return guarded.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(); // Preserve the exact exclusion/owner refusal without starting a raw material batch.
        matched = resolved.Value.MatchCompleteGraph(guarded.Value); // Rematch the independently guarded full graph against the same retained approval.
        if (!matched.IsSuccess) return matched.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(); // Refuse any graph drift between the witness and guarded owner readback.
        _teachingDisclosureGraph = guarded.Value; _teachingDisclosureInput = input; // Retain the guarded actual graph and the full canonical input for opaque issuance.
        _teachingDisclosureBinding = CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(SelectionProjection(_snapshot.Binding))); // Freeze the complete canonical receiving binding from the actual Session snapshot.
        _teachingDisclosureIssuing = true; OperationResult<PreparedTeachingDisclosureReceipt> issued; // Open only the short private disclosure issuing scope and stage its actual result.
        try { issued = PreparedTeachingDisclosureReceipt.Issue(this); } finally { _teachingDisclosureIssuing = false; } // Balance that issuing flag even if receipt construction fails or throws.
        if (!issued.IsSuccess || issued.Value is null) return issued.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(); // Preserve issuance refusal before any raw owner material opens.
        _teachingDisclosureReceipt = issued.Value; // Retain the genuine issued receipt by reference for the fixed recipient operation.
        SelectionMaterialPointForTesting?.Invoke("teaching-disclosure-receipt-issued"); token.ThrowIfCancellationRequested(); // Observe issuance for fault controls and preserve caller cancellation before raw read.
        if (!issued.Value.IsCurrentFor(this)) return CodeDocumentationOneLessonTeachingDisclosureApproval.Refuse(CodeDocumentationOneLessonTeachingDisclosureApproval.Stale).MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(); // Refuse a revoked/changed/disposed receipt before the new full material batch.
        var material = _earlyGrant.SourceSelection.OpenTeachingSelectionMaterialsExcludingOriginal(_ordinaryStart.RepositoryRoot, input.Categories, baseline.Context.Corpus, // Open all eligible full task/reference/provenance through the existing source owner/exclusion path.
            calibration.RepresentativeAssessment.Cases, calibration.Calibration.Observations, token); // Carry genuine closed baseline cases/observations and caller token into that batch.
        if (!material.IsSuccess || material.Value is null) return material.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(); // Return the genuine raw owner refusal rather than replacing missing material.
        var captured = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CaptureStartupApprovedSelectionMaterialContext(this, issued.Value, material.Value, binding, _teachingDisclosureBinding, input); // Send actual full readback and genuine receipt to DatasetAuthoring's fixed constructor.
        token.ThrowIfCancellationRequested(); return captured; // Keep recipient cancellation primary before returning its staged context.
    } // Finish protected preparation without append, training, provider dispatch or lesson authority.

    private OperationResult<bool> FinishStartupApprovedSelectionMaterialChecks(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Combine the unchanged five owner finals with one independent startup revision final.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, byte[]? prefix, bool nativePrepared = false) // Keep the same catalog, selector and pre-operation full prefix snapshot.
    { // Attempt both final families even when an earlier check refuses or throws.
        Exception? fiveFailure = null; OperationResult<bool> five = RecoveryRequired<bool>(), revision = RecoveryRequired<bool>(); // Stage separate five-check and revision outcomes so the original priority is preserved.
        _teachingDisclosureRevisionFinalFailure = null; TeachingDisclosureRevisionFinalErrorForTesting = null; // Clear only this operation's prior revision-final diagnostic/exception.
        try { five = FinishSelectionMaterialChecks(baseline, catalog, selector, prefix); } // Run the original five independent None checks without changing their arbitration.
        catch (Exception ex) when (ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { fiveFailure = ex; } // Retain their exact nonfatal/OCE object while still attempting startup revision.
        try // Attempt the independent revision check regardless of the original five-check outcome.
        { // Use retained startup/approval provenance for the sixth final, never caller data.
            revision = _teachingDisclosureStartup is not null && _teachingDisclosureRevisionInput is not null // Require exact retained membership independently of whether disclosure resolution succeeded.
                ? _teachingDisclosureStartup.CheckProtectedTeachingDisclosureRevision(_teachingDisclosureRevisionInput, CancellationToken.None) // Check provenance, approval identity and current revision with None; this input grants no receipt or payload.
                : CodeDocumentationOneLessonTeachingDisclosureApproval.Refuse(CodeDocumentationOneLessonTeachingDisclosureApproval.Unavailable); // Keep missing retained approval as an exact unavailable refusal.
            if (revision.IsSuccess && _teachingDisclosureReceipt is not null && !_teachingDisclosureReceipt.IsCurrentFor(this)) revision = CodeDocumentationOneLessonTeachingDisclosureApproval.Refuse(CodeDocumentationOneLessonTeachingDisclosureApproval.OwnerMismatch); // Refuse an otherwise current revision if the actual receipt no longer owns this operation.
            SelectionMaterialPointForTesting?.Invoke("teaching-disclosure-revision-final-none"); // Observe the completed sixth attempt for ordering/count and secondary-exception controls.
        } // Finish revision attempt before combining it with the original final family.
        catch (Exception ex) when (ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { _teachingDisclosureRevisionFinalFailure = ex; } // Retain the exact revision exception independently of the earlier five-check exception.
        finally { TeachingDisclosureRevisionFinalErrorForTesting = _teachingDisclosureRevisionFinalFailure?.GetType().Name ?? (revision.IsSuccess ? null : revision.ErrorCode); } // Expose only this attempt's bounded actual error code/type for diagnostics.
        OperationResult<bool> family = OperationResult<bool>.Ok(true);
        _nativePreparedFamilyFinalFailure = null; NativePreparedFamilyFinalErrorForTesting = null;
        if (nativePrepared)
        {
            try { family = CheckNativeSelectionFamiliesAtFinal(); } // Preparation, Started and retained launch-input finals are independent even after primary refusal/cancellation.
            catch (Exception ex) when (ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { _nativePreparedFamilyFinalFailure = ex; }
            finally { NativePreparedFamilyFinalErrorForTesting = _nativePreparedFamilyFinalFailure?.GetType().Name ?? (family.IsSuccess ? null : family.ErrorCode); }
        }
        if (fiveFailure is not null) // Keep the original five-check exception primary when both families fail.
        { // Attach revision evidence before rethrowing the exact original exception object.
            if (_teachingDisclosureRevisionFinalFailure is not null) fiveFailure.Data["SelectionMaterialTeachingDisclosureRevisionFinalFailure"] = _teachingDisclosureRevisionFinalFailure; // Retain the exact revision secondary exception on the original final exception.
            if (TeachingDisclosureRevisionFinalErrorForTesting is { } error) fiveFailure.Data["SelectionMaterialTeachingDisclosureRevisionFinalError"] = error; // Retain the bounded revision code/type without exposing any raw material.
            if (_nativePreparedFamilyFinalFailure is not null) fiveFailure.Data["NativePreparedFamilyFinalFailure"] = _nativePreparedFamilyFinalFailure;
            if (NativePreparedFamilyFinalErrorForTesting is { } familyError) fiveFailure.Data["NativePreparedFamilyFinalError"] = familyError;
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(fiveFailure).Throw(); // Rethrow the same original five-check exception via exception dispatch information.
        } // Finish secondary attachment without changing original exception priority.
        if (!five.IsSuccess) return five; // Keep an original five-check business refusal ahead of revision refusal or exception.
        if (_teachingDisclosureRevisionFinalFailure is not null)
        {
            if (_nativePreparedFamilyFinalFailure is not null) _teachingDisclosureRevisionFinalFailure.Data["NativePreparedFamilyFinalFailure"] = _nativePreparedFamilyFinalFailure;
            if (NativePreparedFamilyFinalErrorForTesting is { } familyError) _teachingDisclosureRevisionFinalFailure.Data["NativePreparedFamilyFinalError"] = familyError;
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(_teachingDisclosureRevisionFinalFailure).Throw();
        }
        if (!revision.IsSuccess) return revision;
        if (_nativePreparedFamilyFinalFailure is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(_nativePreparedFamilyFinalFailure).Throw();
        return family; // Five, revision, then family arbitrate only after all independent attempts.
    } // Finish final arbitration without accepting the staged consumer refusal prematurely.

    private OperationResult<bool> FinishSelectionMaterialChecks(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Finite final checks do not inherit caller cancellation.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, byte[]? prefix) // Bind the exact prefix captured before preparation/consumer work.
    { // Failure cannot authorize a staged context or Synthetic choice.
        OperationResult<bool> receiving = RecoveryRequired<bool>(), existing = RecoveryRequired<bool>(), material = RecoveryRequired<bool>(); // Independent memory, saved-baseline and full material attempts each retain their own outcome.
        _selectionMaterialReceivingFinalFailure = null; _selectionMaterialBaselineFinalFailure = null; _selectionMaterialSourceFinalFailure = null; // Retain exceptions from this final boundary separately.
        SelectionMaterialReceivingFinalErrorForTesting = null; SelectionMaterialBaselineFinalErrorForTesting = null; SelectionMaterialSourceFinalErrorForTesting = null; // Clear only the last bounded outcome before actual attempts.
        var frozen = _selectionMaterialFinalSnapshot; // Never reconstruct saved-baseline or material lookup from late receiving fields.
        if (frozen is not null) // A foreign owner supplies no authenticated detached lookup and gains no disk read.
        { // Saved-baseline failure cannot suppress current receiving or full material attempts.
            try { existing = frozen.CheckBaselineReference(); SelectionMaterialPointForTesting?.Invoke("baseline-reference-final-none"); } catch (Exception ex) when (ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { _selectionMaterialBaselineFinalFailure = ex; } // None existing-only inspection keeps exact claim/baseline checks and zero-time Busy refusal.
            SelectionMaterialBaselineFinalErrorForTesting = _selectionMaterialBaselineFinalFailure?.GetType().Name ?? (existing.IsSuccess ? null : existing.ErrorCode); // Preserve this actual bounded lower cause independently of memory validation.
        } // No new journal, measurement or creation capability is used.
        try // Receiving validation may fail or throw without suppressing any authenticated material reopen below.
        { // The old chained helper and its other callers are deliberately unchanged.
            receiving = CheckOrdinaryDiagnosticOwnership(baseline, catalog, selector); SelectionMaterialPointForTesting?.Invoke("receiving-final-none"); // Validate current held custody/binding/context without caller cancellation.
            if (receiving.IsSuccess && (prefix is null || _v2Prefix is null || !prefix.AsSpan().SequenceEqual(_v2Prefix))) receiving = RecoveryRequired<bool>(); // Require exact receiving prefix continuity.
            if (receiving.IsSuccess && _selectionMaterialPrepared is { } context) // Diagnose only under a currently valid receiving owner.
            { // Compare sanitized receipt/category scalars independently of the later SOURCE graph traversal.
                var diagnostic = ClassifyOrdinaryBaseline(baseline, catalog, selector, CancellationToken.None); // This genuine current diagnosis has no inference or exposure effect.
                if (!diagnostic.IsSuccess || diagnostic.Value is null) receiving = diagnostic.MapError<bool>(); // Preserve the real receiving refusal.
                else if (diagnostic.Value.OriginalReceiptId != context.MetadataInput.OriginalReceiptId || diagnostic.Value.OriginalReceiptSha256 != context.MetadataInput.OriginalReceiptSha256 || !diagnostic.Value.Categories.SequenceEqual(context.MetadataInput.Categories, StringComparer.Ordinal)) receiving = RecoveryRequired<bool>(); // Bind exact sanitized diagnostic order without trusting substituted fields.
            } // No early return precedes the material checks.
        } // Receiving exceptions are secondary evidence, not permission to skip SOURCE.
        catch (Exception ex) when (ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { _selectionMaterialReceivingFinalFailure = ex; } // Retain the exact actual receiving failure object.
        finally { SelectionMaterialReceivingFinalErrorForTesting = _selectionMaterialReceivingFinalFailure?.GetType().Name ?? (receiving.IsSuccess ? null : receiving.ErrorCode); } // Expose only bounded type/code diagnostics.
        if (frozen is not null) // A foreign owner that never authenticated supplies no snapshot and gains no material read.
        { // Original-before, complete SOURCE and Original-after are each attempted even if another attempt fails or throws.
            OperationResult<bool> originalBefore = RecoveryRequired<bool>(), originalAfter = RecoveryRequired<bool>(); // Keep all three actual owner outcomes.
            OperationResult<CodeDocumentationOneLessonSourceSelection.DeclaredTeachingCorpusReadback> source = RecoveryRequired<CodeDocumentationOneLessonSourceSelection.DeclaredTeachingCorpusReadback>(); // Stage the complete private graph only for final continuity checking.
            try { originalBefore = frozen.CheckOriginal(); SelectionMaterialPointForTesting?.Invoke("original-before-final-none"); } catch (Exception ex) when (ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { _selectionMaterialSourceFinalFailure ??= ex; } // None check of genuine closed owner, regardless of receiving failure.
            try { source = frozen.CheckSource(); SelectionMaterialPointForTesting?.Invoke("source-reopen-final-none"); } catch (Exception ex) when (ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { if (_selectionMaterialSourceFinalFailure is null) _selectionMaterialSourceFinalFailure = ex; else _selectionMaterialSourceFinalFailure.Data["SelectionMaterialSourceReopenFailure"] = ex; } // Always attempt every frozen teaching owner/exclusion traversal.
            try { originalAfter = frozen.CheckOriginal(); SelectionMaterialPointForTesting?.Invoke("original-after-final-none"); } catch (Exception ex) when (ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { if (_selectionMaterialSourceFinalFailure is null) _selectionMaterialSourceFinalFailure = ex; else _selectionMaterialSourceFinalFailure.Data["SelectionMaterialOriginalAfterFailure"] = ex; } // Closed after-check also survives a failed SOURCE traversal.
            material = !source.IsSuccess || source.Value is null ? source.MapError<bool>() : !originalBefore.IsSuccess ? originalBefore : originalAfter; // Preserve actual SOURCE refusal separately from receiving failure.
            if (material.IsSuccess && _selectionMaterialPrepared is { } context) // Reconstruct graph bytes only from the frozen authenticated material owners.
            { // The immutable packet supplies its previously frozen sanitized receipt/order, never late receiving metadata.
                var input = context.MetadataInput; var current = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(input.OriginalReceiptId, input.OriginalReceiptSha256, input.Categories, source.Value!); // Include every used/unused SOURCE field in final canonical continuity.
                if (!current.IsSuccess || current.Value is null) material = current.MapError<bool>(); // Preserve malformed current SOURCE refusal.
                else if (!current.Value.CopyBytes().AsSpan().SequenceEqual(input.CopyBytes())) material = RecoveryRequired<bool>(); // Digest equality alone never replaces full input equality.
            } // No receiving failure can suppress these actual attempts.
        } // A missing authenticated snapshot remains a refusal, not an arbitrary source read.
        SelectionMaterialSourceFinalErrorForTesting = _selectionMaterialSourceFinalFailure?.GetType().Name ?? (material.IsSuccess ? null : material.ErrorCode); // Preserve independent actual owner outcome without protected details.
        SelectionMaterialPointForTesting?.Invoke("receiving-final-attempted"); if (frozen is not null) SelectionMaterialPointForTesting?.Invoke("source-final-attempted"); // Observe actual completed attempts; unauthenticated owners cannot report a SOURCE traversal that never ran.
        if (frozen is not null) SelectionMaterialPointForTesting?.Invoke("baseline-final-attempted"); // Observe only a completed genuine existing-only baseline attempt.
        if (_selectionMaterialReceivingFinalFailure is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(_selectionMaterialReceivingFinalFailure).Throw(); // Propagate actual secondary exception only after all material attempts.
        if (_selectionMaterialBaselineFinalFailure is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(_selectionMaterialBaselineFinalFailure).Throw(); // Preserve the actual saved-baseline exception after every independent owner attempt.
        if (_selectionMaterialSourceFinalFailure is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(_selectionMaterialSourceFinalFailure).Throw(); // Preserve the material exception object if no receiving exception came first.
        SelectionMaterialPointForTesting?.Invoke("final-none-checked"); // Inject secondary faults only after the real complete None checks finish.
        return !receiving.IsSuccess ? receiving : !existing.IsSuccess ? existing : material; // Success requires memory, saved-baseline custody and complete material continuity; keep exact lower refusal codes.
    } // The surrounding unconditional finally releases CAS even when this hook throws.

    private sealed record FreshFullMaterialFamilyStaging( // Temporary storage references carry no semantic or disclosure authority.
        CodeDocumentationOneLessonContinuationCapacityV2 PreviousCapacity, FullMaterialStorageFamilyIntent? PreviousIntent, byte[] Prefix, // Restore only these exact pre-effect owned references.
        CodeDocumentationOneLessonContinuationCapacityV2 InstalledCapacity, FullMaterialStorageFamilyIntent InstalledIntent); // An attempted append invalidates restoration even if frames remain zero.

    private OperationResult<FreshFullMaterialFamilyStaging> StageFreshFullMaterialFamily( // Called only inside the continuous acquired material boundary after frozen final inputs.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector) // Exact held baseline and grant reference remain required.
    { // Inspect actual empty custody instead of objective.Created or a caller label.
        var current = ReadCapacityPrefixV2(); // Reopen the owner and prove absent durable history.
        if (!current.IsSuccess || _disposed || _earlyGrant is not { FormatVersion: 2 } || _snapshot.Binding.FormatVersion != 2 || // A closed or foreign binding never stages a family.
            _fullMaterialLease is not null || _fullMaterialPoisoned || _studentPoisoned || _studentLease is not null || _studentAttemptStarted || _studentResponseInFlight != 0 || // No old storage/student attempt can be migrated.
            _selectionFrame is not null || _selectionAttemptStarted || _selectionInFlight is not null || _fullMaterialDispatch is not null || _fullMaterialCompleted is not null || // Selection attempts and completed state remain closed.
            EarlyOutcome is not null || _snapshot.TerminalOutcome is not null || _snapshot.Effects != new CodeDocumentationOneLessonEffectAccounting() || // Terminal/effect history cannot become fresh.
            _capacityV2 is null || current.Value!.PrefixFrames != 0 || current.Value.PrefixBytes != 0 || _v2Prefix is not { Length: 0 } || _v2History is not null || // Exact confirmed empty memory and no history handle are mandatory.
            File.Exists(_historyPath) || Directory.Exists(_historyPath) || !ReferenceEquals(baseline, _ordinaryBaseline) || !ReferenceEquals(catalog.Resolve(selector).Value, _earlyGrant)) // Existing empty files are recovery, never fresh evidence.
            return RecoveryRequired<FreshFullMaterialFamilyStaging>(); // Do not mutate any rejected owner's family.
        var owned = CheckOrdinaryDiagnosticOwnership(baseline, catalog, selector); // Verify all start/context/tokenizer/evidence/evaluator/root fingerprints.
        if (!owned.IsSuccess) return owned.MapError<FreshFullMaterialFamilyStaging>(); // Preserve the exact ownership refusal.
        var diagnosis = ClassifyOrdinaryBaseline(baseline, catalog, selector, CancellationToken.None); // Reclassify the same genuine insufficient baseline.
        if (!diagnosis.IsSuccess) return diagnosis.MapError<FreshFullMaterialFamilyStaging>(); // Sufficient or unusable diagnosis never enters a teaching family.
        var exclusion = InspectOrdinaryOriginalTeachingExclusion(baseline, catalog, selector, CancellationToken.None); // Recheck all ORIGINAL and declared SOURCE rows under the same gate.
        if (!exclusion.IsSuccess) return exclusion.MapError<FreshFullMaterialFamilyStaging>(); // No whole-source exclusion can be skipped.
        var intent = FullMaterialStorageFamilyIntent.Capture(catalog, selector, _snapshot.Binding); // Exact canonical format-two binding is storage custody only.
        if (intent is null) return CapacityInvalid<FreshFullMaterialFamilyStaging>(); // No guessed family or Native authority is minted.
        try // Use the same checked capacity arithmetic as the existing opener.
        { // No teacher or total lesson limit is increased.
            var descriptor = _snapshot.Binding.DescriptorsV2!; var n = checked(descriptor.CaseCount * descriptor.RepeatCount); // Preserve the immutable case/repeat count.
            if (descriptor.CaseCount is < 1 or > 200 || descriptor.RepeatCount < 2 || catalog.MaxTeacherCalls is < 0 or > 10_000) return CapacityInvalid<FreshFullMaterialFamilyStaging>(); // Keep original descriptor bounds.
            if (checked(2 * n) > catalog.MaxTeacherCalls) return CapacityExceeded<FreshFullMaterialFamilyStaging>(); // Both complete series still consume the existing grant budget.
            var capacity = new CodeDocumentationOneLessonContinuationCapacityV2(n, 3, CodeDocumentationOneLessonV2Wire.FullMaterialTotalMaximumBytes, 0, 0); // Exactly the existing three-frame 57364-byte family.
            var staged = new FreshFullMaterialFamilyStaging(_capacityV2, _fullMaterialLease, _v2Prefix, capacity, intent); // Retain unchanged reference identity for narrow rollback.
            _capacityV2 = capacity; _fullMaterialLease = intent; return OperationResult<FreshFullMaterialFamilyStaging>.Ok(staged); // Continue directly to the existing full binder without releasing CAS.
        } // No durable append or reservation occurs during staging.
        catch (OverflowException) { return CapacityInvalid<FreshFullMaterialFamilyStaging>(); } // Arithmetic failure grants no effect.
    } // This private family intent is never an issuer or a public mode flag.

    private void RestoreFreshFullMaterialFamilyBeforeEffects(FreshFullMaterialFamilyStaging staged) // Cleanup only the references this operation actually installed.
    { // Poison is set before the FIRST attempted append, even an empty created history file.
        var unchanged = ReferenceEquals(_capacityV2, staged.InstalledCapacity) && ReferenceEquals(_fullMaterialLease, staged.InstalledIntent) && // Changed capacity/intent cannot be overwritten by rollback.
            ReferenceEquals(_v2Prefix, staged.Prefix) && _v2Prefix is { Length: 0 } && _capacityV2.PrefixFrames == 0 && _capacityV2.PrefixBytes == 0 && // Same exact confirmed empty prefix only.
            _v2History is null && !File.Exists(_historyPath) && !Directory.Exists(_historyPath) && !_fullMaterialPoisoned && !_studentPoisoned && // Unknown attempted persistence is never erased or retried.
            !_selectionAttemptStarted && !_studentAttemptStarted && _selectionInFlight is null && _fullMaterialCompleted is null; // No durable or semantic state is cleared.
        if (!unchanged) return; // Retain full-family poison/unknown on every ambiguous path.
        var receivingSafe = _disposed || CheckOrdinaryDiagnosticOwnership(_ordinaryBaseline!, _freshFullMaterialCatalog!, _freshFullMaterialSelector!).IsSuccess; // Dispose only closes handles; it never reopens them or changes the owned empty references.
        if (!receivingSafe || SelectionMaterialBaselineFinalErrorForTesting is not null || SelectionMaterialSourceFinalErrorForTesting is not null) { _fullMaterialPoisoned = true; return; } // Unproved custody/source retains fail-closed recovery.
        _capacityV2 = staged.PreviousCapacity; _fullMaterialLease = staged.PreviousIntent; // Restore proved own unchanged pre-effect storage only; disposed stays disposed.
    } // Restoration does not reset attempts, handles, history or terminal state.
    private CodeDocumentationOneLessonGrantCatalog? _freshFullMaterialCatalog; // Transient exact owner inputs exist only under the acquired boundary.
    private CodeDocumentationOneLessonSelectorV1? _freshFullMaterialSelector; // Cleanup clears this reference before releasing CAS.

    private async Task<OperationResult<T>> ExecuteSelectionMaterialBoundaryAsync<T>( // Preparation and inspection share one genuine receiving owner lifecycle.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog, // Reuse held baseline/catalog rather than a new evaluator or coordinator.
        CodeDocumentationOneLessonSelectorV1 selector, IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>? admissions, // Full eligible opaque admission set remains required.
        SelectedMaterialRecipient recipient, Func<PreparedSelectionMaterialReceipt, Task<OperationResult<T>>> consume, CancellationToken token, bool fullMaterial = false, bool freshFullMaterial = false, ContinuousLearningModelServerStartup? startup = null, string? syntheticNativeLaunchDirectory = null) // Only the explicit internal fixture wrapper supplies synthetic launch inputs.
    { // This generic private helper is not a public arbitrary effect callback.
        if (_disposed || Interlocked.CompareExchange(ref _selectionMaterialInFlight, 1, 0) != 0) return RecoveryRequired<T>(); // Refuse concurrency/reentrancy before a second raw recipient/provider invocation.
        if (fullMaterial) _fullMaterialCheckOwner.Value = this; // A short owner scope permits independent checks of its own pending prefix.
        _teachingDisclosureStartup = startup; // Retain startup only after acquiring the existing CAS so another operation cannot replace it.
        _syntheticNativeLaunchDirectory = syntheticNativeLaunchDirectory; // A concurrent/reentrant refusal cannot replace this operation's fixture owner.
        TeachingDisclosureStagedErrorForTesting = null; TeachingDisclosureRevisionFinalErrorForTesting = null; // Clear prior staged/revision diagnostics at the start of this held operation.
        Exception? primary = null; OperationResult<T>? staged = null; OperationResult<bool>? final = null; byte[]? prefix = null; // Separate primary, secondary and staged result lifetimes.
        FreshFullMaterialFamilyStaging? freshStaging = null; // Retain temporary own references until all final checks complete.
        SelectionMaterialSecondaryErrorForTesting = null; // Clear only this operation's bounded secondary diagnostic.
        SelectionMaterialReceivingFinalErrorForTesting = null; SelectionMaterialBaselineFinalErrorForTesting = null; SelectionMaterialSourceFinalErrorForTesting = null; // No previous operation supplies final refusal evidence for this one.
        try // All transient issuer state and CAS are released unconditionally.
        { // Capture and consume before final acceptance, preserving exact primary exception identity.
            try // Local error handling is confined to this new read-only boundary.
            { // No durable selection/effect state changes are made.
                var owned = CheckOrdinaryDiagnosticOwnership(baseline, catalog, selector); // Authenticate current stable prefix/custody before snapshotting.
                if (!owned.IsSuccess) staged = owned.MapError<T>(); // Preserve a primary business refusal without raw extraction.
                else // Only a genuine held receiving owner reaches the shared core.
                { // The public preparation wrapper does not recursively reacquire CAS.
                    prefix = _v2Prefix!.ToArray(); var frozen = SelectionMaterialFinalSnapshot.Capture(this, baseline, catalog, selector); // Freeze genuine check inputs before any recipient/provider can change receiving metadata.
                    if (!frozen.IsSuccess || frozen.Value is null) staged = frozen.MapError<T>(); // No incomplete snapshot can authorize preparation or final raw reopens.
                    else _selectionMaterialFinalSnapshot = frozen.Value; // Retain the private complete originals/source independently until unconditional release.
                    OperationResult<FreshFullMaterialFamilyStaging>? entered = null; // Fresh staging and binder are one operation under the same CAS.
                    if (freshFullMaterial && frozen.IsSuccess && frozen.Value is not null) // Freeze independent final inputs BEFORE staging or a test callback.
                    { // The snapshot remains available for all None finals even when cancellation interrupts entry.
                        _freshFullMaterialCatalog = catalog; _freshFullMaterialSelector = selector; // Keep exact custody inputs for proved own rollback.
                        SelectionMaterialPointForTesting?.Invoke("fresh-before-stage"); token.ThrowIfCancellationRequested(); // Exact primary OCE before temporary mutation is preserved.
                        entered = StageFreshFullMaterialFamily(baseline, catalog, selector); // Reclassify/exclude and stage without release/reacquire.
                        if (entered.IsSuccess && entered.Value is not null) { freshStaging = entered.Value; SelectionMaterialPointForTesting?.Invoke("fresh-after-stage"); token.ThrowIfCancellationRequested(); } // Callback re-entry still sees CAS held.
                    } // An unsuccessful entry cannot run material preparation.
                    var entryValid = !freshFullMaterial || entered is { IsSuccess: true, Value: not null }; // Storage custody never grants material disclosure.
                    var prepared = !entryValid ? (entered is null ? RecoveryRequired<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>() : entered.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>()) : frozen.IsSuccess && frozen.Value is not null ? PrepareSelectionMaterialCore(baseline, catalog, selector, Array.AsReadOnly(admissions?.ToArray() ?? []), recipient, token) : frozen.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext>(); // Run complete admission and fixed-recipient checks only after authenticated copying.
                    if (!prepared.IsSuccess || prepared.Value is null) staged = prepared.MapError<T>(); // No receipt or callback on incomplete/invalid material.
                    else // A complete recipient still requires final preparation checks before receipt issuance.
                    { // Keep the exact actual context active for the sole private issuer.
                        _selectionMaterialPrepared = prepared.Value; var prechecked = startup is null ? FinishSelectionMaterialChecks(baseline, catalog, selector, prefix) : FinishStartupApprovedSelectionMaterialChecks(baseline, catalog, selector, prefix, recipient == SelectedMaterialRecipient.ProtectedStartupNativePreparation); // Complete None owner checks before any full packet is passed to a callback.
                        if (!prechecked.IsSuccess) staged = prechecked.MapError<T>(); // Late mutation withholds the staged context.
                        else // Current complete owners authorize only this explicitly Synthetic inspection data.
                        { // Private issuer state exists only during receipt construction.
                            _selectionMaterialIssuing = true; OperationResult<PreparedSelectionMaterialReceipt> receipt; // Internal caller access cannot mint authority outside this short scope.
                            try { receipt = PreparedSelectionMaterialReceipt.Issue(this, prepared.Value); } finally { _selectionMaterialIssuing = false; } // Balance issuer state on every constructor outcome.
                            if (!receipt.IsSuccess || receipt.Value is null) staged = receipt.MapError<T>(); // Failed issuance never reaches a consumer.
                            else { _selectionMaterialReceipt = receipt.Value; token.ThrowIfCancellationRequested(); staged = await consume(receipt.Value).ConfigureAwait(false); if (startup is not null) { TeachingDisclosureStagedErrorForTesting = staged.IsSuccess ? null : staged.ErrorCode; SelectionMaterialPointForTesting?.Invoke("teaching-disclosure-inspection-result-staged"); } token.ThrowIfCancellationRequested(); } // Execute the one fixed consumer with complete private input.
                        } // No saved choice or effect permit is minted here.
                    } // Finish preparation and the optional single Synthetic callback.
                } // End genuine receiving-owner capture.
            } // Retain primary exceptions before independent final None checks.
            catch (Exception ex) when (ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { primary = ex; } // OCE remains the exact original object/token, independent of shared classifier semantics.
            finally // Final validation is attempted for success, refusal, cancellation and failure.
            { // A secondary cleanup exception cannot replace the primary or retain CAS.
                try { final = startup is null ? FinishSelectionMaterialChecks(baseline, catalog, selector, fullMaterial ? _fullMaterialExpectedPrefix ?? prefix : prefix) : FinishStartupApprovedSelectionMaterialChecks(baseline, catalog, selector, prefix, recipient == SelectedMaterialRecipient.ProtectedStartupNativePreparation); } // Only confirmed generated append readback can advance expected continuity.
                catch (Exception ex) when (ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) // Treat secondary exceptions only within this bounded final check.
                { final = RecoveryRequired<bool>(); SelectionMaterialSecondaryErrorForTesting = ex.GetType().Name; if (primary is null) primary = ex; else primary.Data["SelectionMaterialFinalFailure"] = ex; } // Preserve primary identity and retain the exact distinct secondary exception diagnostic.
                if (final is { IsSuccess: false }) SelectionMaterialSecondaryErrorForTesting ??= final.ErrorCode; // A failed owner result is visible without raw data in diagnostics.
                if (primary is not null) // Keep both final outcomes separate when cancellation/consumer failure is primary.
                { // Diagnostics contain bounded codes/types and actual exception objects, never closed material.
                    if (SelectionMaterialReceivingFinalErrorForTesting is { } receivingError) primary.Data["SelectionMaterialFinalReceivingError"] = receivingError; // Receiving refusal cannot hide the independent material result.
                    if (SelectionMaterialBaselineFinalErrorForTesting is { } baselineError) primary.Data["SelectionMaterialFinalBaselineError"] = baselineError; // Saved-baseline refusal remains visible even when memory or SOURCE also fails.
                    if (SelectionMaterialSourceFinalErrorForTesting is { } sourceError) primary.Data["SelectionMaterialFinalSourceError"] = sourceError; // Retain actual frozen ORIGINAL/SOURCE refusal even when receiving failed first.
                    if (_selectionMaterialReceivingFinalFailure is not null) primary.Data["SelectionMaterialFinalReceivingFailure"] = _selectionMaterialReceivingFinalFailure; // Preserve exact secondary exception identity separately.
                    if (_selectionMaterialBaselineFinalFailure is not null) primary.Data["SelectionMaterialFinalBaselineFailure"] = _selectionMaterialBaselineFinalFailure; // Retain the actual baseline exception separately without replacing primary/OCE/token.
                    if (_selectionMaterialSourceFinalFailure is not null) primary.Data["SelectionMaterialFinalSourceFailure"] = _selectionMaterialSourceFinalFailure; // A distinct material exception remains inspectable without replacing primary/OCE/token.
                    if (TeachingDisclosureRevisionFinalErrorForTesting is { } revisionError) primary.Data["SelectionMaterialTeachingDisclosureRevisionFinalError"] = revisionError; // Attach the bounded sixth-final refusal separately when a primary exception already exists.
                    if (_teachingDisclosureRevisionFinalFailure is not null) primary.Data["SelectionMaterialTeachingDisclosureRevisionFinalFailure"] = _teachingDisclosureRevisionFinalFailure; // Retain the exact sixth-final exception object without replacing the primary OCE/token.
                    if (NativePreparedFamilyFinalErrorForTesting is { } familyError) primary.Data["NativePreparedFamilyFinalError"] = familyError;
                    if (_nativePreparedFamilyFinalFailure is not null) primary.Data["NativePreparedFamilyFinalFailure"] = _nativePreparedFamilyFinalFailure;
                } // Outer finally releases the transient exceptions after diagnostic transfer.
            } // Both pre-consumer and post-consumer checks have finite explicit boundaries.
        } // Unconditional release encloses even secondary final-check failure.
        finally
        {
            try { if (freshStaging is not null) RestoreFreshFullMaterialFamilyBeforeEffects(freshStaging); }
            catch (Exception rollbackFailure)
            {
                _fullMaterialPoisoned = true;
                if (primary is null) primary = rollbackFailure;
                else primary.Data["SelectionMaterialRollbackFailure"] = rollbackFailure;
            }
            finally
            {
                _freshFullMaterialCatalog = null; _freshFullMaterialSelector = null;
                _nativeLaunchBinding?.Invalidate(); _nativeLaunchBinding = null; _nativeLaunchIssuing = false; // Invalidate launch authority before releasing any resource or original receipt.
                try { _nativeLaunchInputs?.Dispose(); } // Independent input disposal cannot strand the held Session CAS.
                catch (Exception resourceFailure) { if (primary is null) primary = resourceFailure; else primary.Data["NativeLaunchInputReleaseFailure"] = resourceFailure; } // Keep exact primary/OCE identity with actual secondary cleanup evidence.
                finally { _nativeLaunchInputs = null; _nativeLaunchHost = null; _syntheticNativeLaunchDirectory = null; } // Resource references end before CAS release; durable Started and charged state remain.
                _nativePreparationReceipt?.Invalidate(); _nativePreparationReceipt = null; _nativePreparationIssuing = false; _nativePreparationNonce = null;
                _nativePreparedFamilyFinalFailure = null; // Stored family/handles persist; transient preparation ownership ends before CAS release.
                _teachingDisclosureReceipt?.Invalidate(); if (startup is not null) _selectionMaterialReceipt?.Invalidate(); _teachingDisclosureReceipt = null; _teachingDisclosureIssuing = false; // Invalidate both protected receipts before clearing issuance state and releasing the CAS.
                _teachingDisclosureApproval = null; _teachingDisclosureRevisionInput = null; _teachingDisclosureGraph = null; _teachingDisclosureInput = null; _teachingDisclosureBinding = null; _teachingDisclosureStartup = null; _teachingDisclosureRevisionFinalFailure = null; // Clear disclosure authority and check-only revision input on every outcome before CAS release.
                _fullMaterialCheckOwner.Value = null; _fullMaterialDispatch = null; _fullMaterialIssuing = false; _fullMaterialExpectedPrefix = null;
                _selectionMaterialIssuing = false; _selectionMaterialReceipt = null; _selectionMaterialPrepared = null; _selectionMaterialFinalSnapshot = null;
                _selectionMaterialReceivingFinalFailure = null; _selectionMaterialBaselineFinalFailure = null; _selectionMaterialSourceFinalFailure = null;
                Interlocked.Exchange(ref _selectionMaterialInFlight, 0);
            }
        } // Rollback failure never replaces the primary OCE or strands the owner gate.
        if (primary is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(primary).Throw(); // Rethrow the same primary/OCE object and token after all final attempts/release.
        if (final is null || !final.IsSuccess) return final is null ? RecoveryRequired<T>() : final.MapError<T>(); // Late owner mutation overrides staged success/refusal without effects.
        return staged ?? RecoveryRequired<T>(); // Return only the actual complete consumer outcome after final acceptance.
    } // No automatic retry, append, new budget or Native route is introduced.

    private OperationResult<bool> CheckNativeSelectionFamiliesAtFinal() // Attempt every native storage and input final before choosing the outward result.
    { // Attempt every native storage and input final before choosing the outward result.
        OperationResult<bool> prepared = RecoveryRequired<bool>(), started = RecoveryRequired<bool>(), inputs = RecoveryRequired<bool>(); // Keep three finite independent native family results.
        Exception? preparedFailure = null, startedFailure = null, inputFailure = null; // No earlier failure skips a later retained-owner check.
        try { prepared = CheckNativePreparedFamily(); SelectionMaterialPointForTesting?.Invoke("native-prepared-family-final-none"); } // Attempt every native storage and input final before choosing the outward result.
        catch (Exception ex) when (ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { preparedFailure = ex; } // Preserve exact original family exception identity.
        try { started = CheckNativeStartedFamily(); SelectionMaterialPointForTesting?.Invoke("native-started-family-final-none"); } // Attempt every native storage and input final before choosing the outward result.
        catch (Exception ex) when (ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { startedFailure = ex; } // Independently retain actual Started failure.
        try { inputs = _nativeLaunchInputs is null || _nativeLaunchInputs.IsCurrentFor(this) ? OperationResult<bool>.Ok(true) : RecoveryRequired<bool>(); SelectionMaterialPointForTesting?.Invoke("native-launch-input-final-none"); } // Attempt every native storage and input final before choosing the outward result.
        catch (Exception ex) when (ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { inputFailure = ex; } // Independently check retained inputs without caller cancellation.
        var primary = preparedFailure ?? startedFailure ?? inputFailure; // Preserve existing prepared-family precedence ahead of new storage/input checks.
        if (primary is not null) // Attempt every native storage and input final before choosing the outward result.
        { // Attempt every native storage and input final before choosing the outward result.
            if (startedFailure is not null && !ReferenceEquals(primary, startedFailure)) primary.Data["NativeStartedFamilyFinalFailure"] = startedFailure; // Keep actual secondary exceptions on the primary.
            if (inputFailure is not null && !ReferenceEquals(primary, inputFailure)) primary.Data["NativeLaunchInputFinalFailure"] = inputFailure; // Preserve independently observed input failure.
            if (!started.IsSuccess) primary.Data["NativeStartedFamilyFinalError"] = started.ErrorCode; if (!inputs.IsSuccess) primary.Data["NativeLaunchInputFinalError"] = inputs.ErrorCode; // Bounded error data never includes raw material.
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(primary).Throw(); // Rethrow the same primary object after every native attempt.
        } // Finish this bounded owner check without renewing authority or an attempt.
        return !prepared.IsSuccess ? prepared : !started.IsSuccess ? started : inputs; // Native finals retain prepared, then Started, then launch-input priority.
    } // Finish this bounded owner check without renewing authority or an attempt.

    internal OperationResult<PreparedSelectionMaterialReceipt> PrepareOrdinarySelectionMaterialContext( // Pure preparation executes synchronously because its fixed consumer returns a completed task.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog, // Reuse current genuine held ORIGINAL/catalog.
        CodeDocumentationOneLessonSelectorV1 selector, IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>? admissions, CancellationToken token = default) // Only existing Synthetic opaque admissions can enter this increment.
        => ExecuteSelectionMaterialBoundaryAsync(baseline, catalog, selector, admissions, SelectedMaterialRecipient.Ordinary, receipt => Task.FromResult(OperationResult<PreparedSelectionMaterialReceipt>.Ok(receipt)), token).GetAwaiter().GetResult(); // Ordinary preparation cannot obtain raw material from synthetic eligibility.

    internal OperationResult<PreparedSelectionMaterialReceipt> PrepareOrdinarySelectionMaterialContextSyntheticForTesting( // Explicit fixture preparation shares every held-owner and final-release check.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog, // Keep the exact existing saved ORIGINAL and grant.
        CodeDocumentationOneLessonSelectorV1 selector, IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>? admissions, CancellationToken token = default) // The caller still supplies one opaque matching Synthetic admission for every eligible item.
        => ExecuteSelectionMaterialBoundaryAsync(baseline, catalog, selector, admissions, SelectedMaterialRecipient.SyntheticForTesting, receipt => Task.FromResult(OperationResult<PreparedSelectionMaterialReceipt>.Ok(receipt)), token).GetAwaiter().GetResult(); // Fixed fixture routing creates no native rights or new effect allowance.

    private CodeDocumentationFullMaterialRequestV1 CaptureFullMaterialRequest(
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext context, string invocation, string genesis)
    {
        var m = context.MetadataInput; var packet = context.CopyCanonicalBytes();
        return new("code-documentation-full-material-selection-v1", "SyntheticTest", "gpt-6-astra", "ultra", invocation, genesis,
            ComputeBindingSha256(_snapshot.Binding), Convert.ToBase64String(context.CopyBindingBytes()), Convert.ToBase64String(packet),
            packet.Length, context.PacketSha256, m.OriginalReceiptId, m.OriginalReceiptSha256, m.SourceSha256, m.FrozenContextSha256, m.Categories.ToArray());
    }

    private static CodeDocumentationTeachingSelectionPayloadV1 FullMaterialChoicePayload(
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext context,
        CodeDocumentationFullMaterialRequestV1 request, string started, CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice choice)
    {
        var bytes = CodeDocumentationOneLessonV2Wire.FullMaterialRequestBytes(request); var m = context.MetadataInput;
        var syntax = CodeDocumentationTeachingSelectionPayloadV1.Capture(m.OriginalReceiptId, m.OriginalReceiptSha256, m.SourceSha256, m.Categories,
            choice.ItemId, choice.SourceId, choice.ReferenceEntryId, choice.SourceEntityFingerprint, choice.Reason, m.FrozenContextSha256,
            choice.MaterialAdmissionEvidenceSha256, request.InvocationId, CodeDocumentationOneLessonV2Wire.FullMaterialSettingsSha256(bytes), new string('0',64));
        if (!syntax.IsSuccess || syntax.Value is null) throw new JsonException("Invalid issued full-material choice.");
        return CodeDocumentationTeachingSelectionPayloadV1.Capture(m.OriginalReceiptId, m.OriginalReceiptSha256, m.SourceSha256, m.Categories,
            choice.ItemId, choice.SourceId, choice.ReferenceEntryId, choice.SourceEntityFingerprint, choice.Reason, m.FrozenContextSha256,
            choice.MaterialAdmissionEvidenceSha256, request.InvocationId, syntax.Value.LaunchSettingsSha256,
            CodeDocumentationOneLessonV2Wire.FullMaterialAcknowledgment(bytes, started, syntax.Value)).Value!;
    }

    internal sealed class MaterialSelectionDispatchReceipt
    {
        private readonly CodeDocumentationOneLessonSession owner;
        private readonly CodeDocumentationOneLessonSelectionDecisionProvider provider;
        private readonly byte[] prefix;
        private readonly Guid nonce; // A transient concrete dispatch identity is not reconstructed from persisted hashes.
        private readonly PreparedSelectionMaterialReceipt prepared; // The genuine recipient receipt is reissued on confirmed Started, not borrowed from the empty pre-append prefix.
        private readonly CodeDocumentationFullMaterialRequestV1 request;
        private readonly string started;
        private int invocationConsumed; // One persisted Started permits one callback, even if the same active receipt is presented concurrently.
        internal CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext Context { get; }
        internal string InvocationId => request.InvocationId;
        internal string SettingsSha256 => CodeDocumentationOneLessonV2Wire.FullMaterialSettingsSha256(CodeDocumentationOneLessonV2Wire.FullMaterialRequestBytes(request));
        private MaterialSelectionDispatchReceipt(CodeDocumentationOneLessonSession current, CodeDocumentationOneLessonSelectionDecisionProvider issuer,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext context, CodeDocumentationFullMaterialRequestV1 input, string startedHash)
        { owner = current; provider = issuer; Context = context; request = input; started = startedHash; prefix = current._v2Prefix!.ToArray(); nonce = Guid.NewGuid(); prepared = current._selectionMaterialReceipt!; }
        internal bool IsActiveFor(CodeDocumentationOneLessonSession current, CodeDocumentationOneLessonSelectionDecisionProvider issuer)
            => ReferenceEquals(owner,current) && ReferenceEquals(provider,issuer) && nonce != Guid.Empty && !owner._disposed &&
                owner._selectionMaterialInFlight == 1 && ReferenceEquals(owner._fullMaterialDispatch,this) &&
                ReferenceEquals(owner._selectionMaterialPrepared,Context) && ReferenceEquals(owner._selectionMaterialReceipt,prepared) && prepared.IsActiveFor(owner) && prefix.AsSpan().SequenceEqual(owner._v2Prefix) && owner.CheckFullDispatchPrefix();
        internal bool TryConsumeInvocationFor(CodeDocumentationOneLessonSession current, CodeDocumentationOneLessonSelectionDecisionProvider issuer)
            => IsActiveFor(current,issuer) && Interlocked.CompareExchange(ref invocationConsumed,1,0) == 0;
        // Consumption never resets. IsActiveFor remains a non-consuming lineage check for the initial callback's return path.
        internal string Acknowledgment(CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice choice)
            => FullMaterialChoicePayload(Context, request, started, choice).LaunchAckSha256;
        internal static OperationResult<MaterialSelectionDispatchReceipt> Issue(CodeDocumentationOneLessonSession owner,
            CodeDocumentationOneLessonSelectionDecisionProvider provider, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext context,
            CodeDocumentationFullMaterialRequestV1 request, string started)
            => owner._fullMaterialIssuing && owner._selectionMaterialInFlight == 1 && owner._capacityV2?.PrefixFrames == 2 &&
                ReferenceEquals(owner._selectionMaterialPrepared,context) && owner._selectionMaterialReceipt?.IsActiveFor(owner) == true && ReferenceEquals(owner._fullMaterialCheckOwner.Value,owner) ?
                OperationResult<MaterialSelectionDispatchReceipt>.Ok(new(owner,provider,context,request,started)) : RecoveryRequired<MaterialSelectionDispatchReceipt>();
    }

    private bool CheckFullDispatchPrefix()
    {
        var prior = _fullMaterialCheckOwner.Value; _fullMaterialCheckOwner.Value = this;
        try { var current = ReadCapacityPrefixV2(); return current.IsSuccess && current.Value!.PrefixFrames == 2; }
        finally { _fullMaterialCheckOwner.Value = prior; } // Finite read permission never leaks into arbitrary callback code.
    }
    internal MaterialSelectionDispatchReceipt? FullMaterialDispatchForTesting => _fullMaterialDispatch; // Exposes only a genuinely issued object for foreign/expired receipt refusal controls.

    internal sealed class FullMaterialSelectionReadback
    {
        private readonly CodeDocumentationOneLessonSession owner;
        internal CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext Context { get; }
        internal CodeDocumentationTeachingSelectionPayloadV1 Choice { get; }
        internal string PrefixSha256 { get; }
        internal string Origin => "SyntheticTest";
        internal bool FullTeacherContextReady => false;
        private FullMaterialSelectionReadback(CodeDocumentationOneLessonSession owner,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext context, CodeDocumentationTeachingSelectionPayloadV1 choice)
        { this.owner = owner; Context = context; Choice = choice; PrefixSha256 = Sha256(owner._v2Prefix!); }
        internal bool IsIssuedBy(CodeDocumentationOneLessonSession current) => ReferenceEquals(owner,current) && !owner._disposed && PrefixSha256 == Sha256(owner._v2Prefix!);
        internal static OperationResult<FullMaterialSelectionReadback> Issue(CodeDocumentationOneLessonSession owner,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext context, CodeDocumentationTeachingSelectionPayloadV1 choice)
            => owner._fullMaterialIssuing && owner._selectionMaterialInFlight == 1 && owner._capacityV2?.PrefixFrames == 3 &&
                ReferenceEquals(owner._selectionMaterialPrepared,context) ? OperationResult<FullMaterialSelectionReadback>.Ok(new(owner,context,choice)) : RecoveryRequired<FullMaterialSelectionReadback>();
    }

    private OperationResult<bool> GuardFullMaterialChoice(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline,
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext context, CodeDocumentationTeachingSelectionPayloadV1 p, CancellationToken token)
    {
        if (context.Candidates.Count(c => c.ItemId == p.ItemId && c.SourceId == p.SourceId && c.ReferenceEntryId == p.ReferenceEntryId &&
            c.SourceEntityFingerprint == p.SourceEntityFingerprint && c.MaterialAdmissionEvidenceSha256 == p.MaterialAdmissionEvidenceSha256) != 1)
            return RecoveryRequired<bool>();
        var calibration = baseline.Readback.Baseline.Calibration;
        var guarded = _earlyGrant!.SourceSelection!.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal(_ordinaryStart!.RepositoryRoot,
            baseline.Context.Corpus, calibration.RepresentativeAssessment.Cases, calibration.Calibration.Observations, token, [p.Reason, p.MaterialAdmissionEvidenceSha256]);
        return guarded.IsSuccess ? OperationResult<bool>.Ok(true) : guarded.MapError<bool>();
    }

    internal Task<OperationResult<FullMaterialSelectionReadback>> BindOrdinaryFullMaterialSelectionAsync(
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog,
        CodeDocumentationOneLessonSelectorV1 selector, IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>? admissions,
        CodeDocumentationOneLessonSelectionDecisionProvider? provider, CancellationToken token = default)
        => BindFullMaterialSelectionCoreAsync(baseline, catalog, selector, admissions, provider, SelectedMaterialRecipient.Ordinary, token); // Ordinary fresh binding and Completed replay both pass through the raw-recipient refusal.

    internal Task<OperationResult<FullMaterialSelectionReadback>> BindOrdinaryFullMaterialSelectionSyntheticForTestingAsync( // Fixture binding retains the complete three-frame durable consumer.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog, // Reuse the same held owner without a second baseline.
        CodeDocumentationOneLessonSelectorV1 selector, IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>? admissions, // Every eligible fixture admission remains mandatory.
        CodeDocumentationOneLessonSelectionDecisionProvider? provider, CancellationToken token = default) // Only this named wrapper selects the synthetic recipient.
        => BindFullMaterialSelectionCoreAsync(baseline, catalog, selector, admissions, provider, SelectedMaterialRecipient.SyntheticForTesting, token); // No caller-controlled bool or wire value selects disclosure authority.

    internal Task<OperationResult<FullMaterialSelectionReadback>> BindFreshOrdinaryFullMaterialSelectionAsync( // Ordinary entry proves fresh storage custody without granting material rights.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog, // Reuse the exact held insufficient baseline.
        CodeDocumentationOneLessonSelectorV1 selector, IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>? admissions, // Require the complete detached opaque set.
        CodeDocumentationOneLessonSelectionDecisionProvider? provider, CancellationToken token = default) // Provider receives nothing before the ordinary recipient guard.
        => BindFullMaterialSelectionCoreAsync(baseline, catalog, selector, admissions, provider, SelectedMaterialRecipient.Ordinary, token, freshFamily: true); // The named wrapper fixes the recipient permanently.

    internal Task<OperationResult<FullMaterialSelectionReadback>> BindFreshSyntheticFullMaterialSelectionForTestingAsync( // Explicit fixture entry exercises the same fresh custody and durable binder.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog, // Never fabricate or remeasure baseline input.
        CodeDocumentationOneLessonSelectorV1 selector, IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>? admissions, // Every eligible fixture item still requires an admission.
        CodeDocumentationOneLessonSelectionDecisionProvider? provider, CancellationToken token = default) // Only this named wrapper chooses SyntheticTest disclosure.
        => BindFullMaterialSelectionCoreAsync(baseline, catalog, selector, admissions, provider, SelectedMaterialRecipient.SyntheticForTesting, token, freshFamily: true); // No JSON flag selects a recipient or Native authority.

    private Task<OperationResult<FullMaterialSelectionReadback>> BindFullMaterialSelectionCoreAsync( // Preserve the existing durable owner and unknown/no-repeat handling behind fixed wrappers.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog, // Use existing genuine baseline custody.
        CodeDocumentationOneLessonSelectorV1 selector, IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>? admissions, // Preserve complete eligibility and order.
        CodeDocumentationOneLessonSelectionDecisionProvider? provider, SelectedMaterialRecipient recipient, CancellationToken token, bool freshFamily = false) // Private entry policy selects storage custody only; every recipient guard remains.
    {
        if ((!freshFamily && _fullMaterialLease is null) || _disposed || _fullMaterialPoisoned) return Task.FromResult(RecoveryRequired<FullMaterialSelectionReadback>()); // Fresh entry stages storage only after the shared CAS is acquired.
        return ExecuteSelectionMaterialBoundaryAsync(baseline, catalog, selector, admissions, recipient,
            prepared => BindFullMaterialSelectionAlreadyOwnedAsync(baseline, catalog, selector, prepared, provider, token),
            token, fullMaterial: true, freshFullMaterial: freshFamily);
    }

    private async Task<OperationResult<FullMaterialSelectionReadback>> BindFullMaterialSelectionAlreadyOwnedAsync(
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog,
        CodeDocumentationOneLessonSelectorV1 selector, PreparedSelectionMaterialReceipt prepared,
        CodeDocumentationOneLessonSelectionDecisionProvider? provider, CancellationToken token)
    {
        // This private consumer is reached only by the continuously held material boundary and its issued receipt.
        var context = prepared.Context; var projection = SelectionProjection(_snapshot.Binding);
        var parsed = CodeDocumentationOneLessonV2Wire.DecodeFullMaterialPrefix(_v2Prefix!, projection, _sessionId, true);
        if (!parsed.IsSuccess || parsed.Value is null) return RecoveryRequired<FullMaterialSelectionReadback>();
        CodeDocumentationTeachingSelectionPayloadV1 choice;
        if (parsed.Value.Frames == 3 && parsed.Value.Request is { } saved && parsed.Value.Choice is { } savedChoice)
        {
            var current = CaptureFullMaterialRequest(context,saved.InvocationId,parsed.Value.GenesisSha256);
            if (!CodeDocumentationOneLessonV2Wire.FullMaterialRequestBytes(current).AsSpan().SequenceEqual(CodeDocumentationOneLessonV2Wire.FullMaterialRequestBytes(saved)))
                return RecoveryRequired<FullMaterialSelectionReadback>(); // Full independently reconstructed raw input, not only hashes or the selected item, must survive reopen.
            choice = savedChoice; // Provider resolution and callback are deliberately absent on this branch.
        }
        else
        {
            if (parsed.Value.Frames != 0 || provider is null || !provider.IsDurableMaterialSelection) return RecoveryRequired<FullMaterialSelectionReadback>();
            var m = context.MetadataInput; var genesis = CodeDocumentationOneLessonV2Wire.FullMaterialGenesisBytes(projection,_sessionId,m.OriginalReceiptId,m.OriginalReceiptSha256);
            var request = CaptureFullMaterialRequest(context,FullMaterialInvocationIdForTesting ?? Guid.NewGuid().ToString("N"),CodeDocumentationOneLessonV2Wire.FullMaterialFrameSha256(genesis));
            byte[] started;
            try
            {
                var requestBytes = CodeDocumentationOneLessonV2Wire.FullMaterialRequestBytes(request); // The real full request is bounded and roundtripped before any append/callback.
                using var roundtrip = JsonDocument.Parse(requestBytes);
                var decoded = JsonSerializer.Deserialize<CodeDocumentationFullMaterialRequestV1>(requestBytes)!;
                if (!CodeDocumentationOneLessonV2Wire.FullMaterialRequestBytes(decoded).AsSpan().SequenceEqual(requestBytes)) return CapacityExceeded<FullMaterialSelectionReadback>();
                started = CodeDocumentationOneLessonV2Wire.FullMaterialStartedBytes(projection,_sessionId,request);
                foreach (var candidate in context.Candidates) // Reserve the maximum whole Completed for every eligible outcome, never just the first choice.
                {
                    var worst = new CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice(candidate.ItemId,candidate.SourceId,candidate.ReferenceEntryId,
                        candidate.SourceEntityFingerprint,new string('\0',CodeDocumentationOneLessonV2Wire.FullMaterialReasonMaximumBytes),candidate.MaterialAdmissionEvidenceSha256);
                    var p = FullMaterialChoicePayload(context,request,CodeDocumentationOneLessonV2Wire.FullMaterialFrameSha256(started),worst);
                    var reservedCompleted = CodeDocumentationOneLessonV2Wire.FullMaterialCompletedBytes(projection,_sessionId,request,CodeDocumentationOneLessonV2Wire.FullMaterialFrameSha256(started),p);
                    if (!CodeDocumentationOneLessonV2Wire.FullMaterialPhysicalCapacityWithin(new[]{genesis,started,reservedCompleted})) return CapacityExceeded<FullMaterialSelectionReadback>();
                    FullMaterialCapacityReservedForTesting?.Invoke(candidate.ItemId,requestBytes.Length,genesis.Length,started.Length,reservedCompleted.Length);
                    var family = CodeDocumentationOneLessonV2Wire.DecodeFullMaterialPrefix(genesis.Concat(started).Concat(reservedCompleted).ToArray(),projection,_sessionId,false);
                    if (!family.IsSuccess) return CapacityExceeded<FullMaterialSelectionReadback>();
                }
            }
            catch (Exception ex) when (ex is JsonException or OverflowException or EncoderFallbackException) { return CapacityExceeded<FullMaterialSelectionReadback>(); }
            token.ThrowIfCancellationRequested(); _fullMaterialPoisoned = true; // From first append onward an unknown attempt cannot be retried even if the callback never starts.
            var appended = AppendSelectionBytes(genesis,"full-material-genesis"); if (!appended.IsSuccess) return appended.MapError<FullMaterialSelectionReadback>();
            appended = AppendSelectionBytes(started,"full-material-started"); if (!appended.IsSuccess) return appended.MapError<FullMaterialSelectionReadback>();
            var beforeDispatch = FinishSelectionMaterialChecks(baseline,catalog,selector,_fullMaterialExpectedPrefix);
            if (!beforeDispatch.IsSuccess) return beforeDispatch.MapError<FullMaterialSelectionReadback>(); // Started-hook corruption blocks the callback while retaining independent lower owner outcomes and unknown accounting.
            _selectionMaterialIssuing = true; OperationResult<PreparedSelectionMaterialReceipt> refreshed;
            try { refreshed = PreparedSelectionMaterialReceipt.Issue(this,context); } finally { _selectionMaterialIssuing = false; }
            if (!refreshed.IsSuccess || refreshed.Value is null) return refreshed.MapError<FullMaterialSelectionReadback>();
            _selectionMaterialReceipt = refreshed.Value; // Renew only the genuine current-prefix receipt; never recapture trusted baseline/grant/source inputs from late receiving fields.
            token.ThrowIfCancellationRequested(); _fullMaterialIssuing = true; OperationResult<MaterialSelectionDispatchReceipt> receipt;
            try { receipt = MaterialSelectionDispatchReceipt.Issue(this,provider,context,request,CodeDocumentationOneLessonV2Wire.FullMaterialFrameSha256(started)); } finally { _fullMaterialIssuing = false; }
            if (!receipt.IsSuccess || receipt.Value is null) return receipt.MapError<FullMaterialSelectionReadback>();
            _fullMaterialDispatch = receipt.Value; _fullMaterialCheckOwner.Value = null;
            OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.IssuedMaterialSelectionDecision> issued;
            try { issued = await provider.InvokePreparedMaterialSelectionAsync(this,receipt.Value,token).ConfigureAwait(false); }
            finally { _fullMaterialCheckOwner.Value = this; } // Callback gains no pending-prefix reader permission; final None checks always regain the private finite scope.
            if (!issued.IsSuccess || issued.Value is null || !issued.Value.IsIssuedBy(provider,receipt.Value) || !receipt.Value.IsActiveFor(this,provider))
                return issued.IsSuccess ? RecoveryRequired<FullMaterialSelectionReadback>() : issued.MapError<FullMaterialSelectionReadback>();
            choice = FullMaterialChoicePayload(context,request,CodeDocumentationOneLessonV2Wire.FullMaterialFrameSha256(started),issued.Value.Choice);
            var guarded = GuardFullMaterialChoice(baseline,context,choice,token); if (!guarded.IsSuccess) return guarded.MapError<FullMaterialSelectionReadback>();
            var beforeCompletion = FinishSelectionMaterialChecks(baseline,catalog,selector,_fullMaterialExpectedPrefix);
            if (!beforeCompletion.IsSuccess) return beforeCompletion.MapError<FullMaterialSelectionReadback>(); // A late receiving/baseline/ORIGINAL/SOURCE refusal cannot authorize a Completed write.
            token.ThrowIfCancellationRequested(); var completedSelectionFrame = CodeDocumentationOneLessonV2Wire.FullMaterialCompletedBytes(projection,_sessionId,request,CodeDocumentationOneLessonV2Wire.FullMaterialFrameSha256(started),choice);
            appended = AppendSelectionBytes(completedSelectionFrame,"full-material-completed"); if (!appended.IsSuccess) return appended.MapError<FullMaterialSelectionReadback>();
            _fullMaterialCompleted = CodeDocumentationOneLessonV2Wire.DecodeFullMaterialPrefix(_v2Prefix!,projection,_sessionId,false).Value;
            _fullMaterialPoisoned = false;
        }
        var finalChoice = GuardFullMaterialChoice(baseline,context,choice,token); if (!finalChoice.IsSuccess) return finalChoice.MapError<FullMaterialSelectionReadback>();
        _fullMaterialIssuing = true;
        try { return FullMaterialSelectionReadback.Issue(this,context,choice); } finally { _fullMaterialIssuing = false; }
    }

    internal Task<OperationResult<FullMaterialSelectionReadback>> TryReadOrdinaryFullMaterialSelectionAsync(
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog,
        CodeDocumentationOneLessonSelectorV1 selector, IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>? admissions,
        CancellationToken token = default)
        => BindOrdinaryFullMaterialSelectionAsync(baseline,catalog,selector,admissions,null,token); // The replay consumer has no provider to resolve or invoke.

    internal Task<OperationResult<FullMaterialSelectionReadback>> TryReadOrdinaryFullMaterialSelectionSyntheticForTestingAsync( // Explicit fixture replay reconstructs the exact complete current packet before returning a saved choice.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog, // Reuse the fresh receiving owner and existing saved baseline.
        CodeDocumentationOneLessonSelectorV1 selector, IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>? admissions, // Unknown or mismatched late admissions still refuse before raw extraction.
        CancellationToken token = default) // Replay never resolves a provider.
        => BindOrdinaryFullMaterialSelectionSyntheticForTestingAsync(baseline, catalog, selector, admissions, null, token); // Saved fixture bytes grant no ordinary raw recipient rights.

    internal Task<OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticSelectionMaterialInspection>> InspectOrdinarySelectionMaterialsAsync( // Actual full-packet Synthetic consumer uses the same guarded preparation core.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog, // Neither caller diagnosis nor a new baseline is accepted.
        CodeDocumentationOneLessonSelectorV1 selector, IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>? admissions, // Validate every eligible capability before new payload extraction.
        CodeDocumentationOneLessonSelectionDecisionProvider provider, CancellationToken token = default) // This additive mode is incompatible with persisted IssuedSelectionDecision.
        => InspectSelectionMaterialsCoreAsync(baseline, catalog, selector, admissions, provider, SelectedMaterialRecipient.Ordinary, token); // An ordinary inspection cannot disclose a matching Synthetic packet.

    internal Task<OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticSelectionMaterialInspection>> InspectOrdinarySelectionMaterialsSyntheticForTestingAsync( // Fixture inspection remains explicitly labelled at every caller.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog, // Reuse existing held custody rather than caller diagnosis.
        CodeDocumentationOneLessonSelectorV1 selector, IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>? admissions, // Complete admission matching is unchanged.
        CodeDocumentationOneLessonSelectionDecisionProvider provider, CancellationToken token = default) // No saved choice or effect is issued by this inspection.
        => InspectSelectionMaterialsCoreAsync(baseline, catalog, selector, admissions, provider, SelectedMaterialRecipient.SyntheticForTesting, token); // The private recipient cannot be selected through JSON or model labels.

    private Task<OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticSelectionMaterialInspection>> InspectSelectionMaterialsCoreAsync( // Fixed wrappers share the same provider and final-owner scope.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, CodeDocumentationOneLessonGrantCatalog catalog, // Keep the genuine receiving baseline and grant.
        CodeDocumentationOneLessonSelectorV1 selector, IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>? admissions, // Do not drop denied or unchosen candidates.
        CodeDocumentationOneLessonSelectionDecisionProvider provider, SelectedMaterialRecipient recipient, CancellationToken token) // Only named ordinary/fixture wrappers supply this private enum.
        => ExecuteSelectionMaterialBoundaryAsync(baseline, catalog, selector, admissions, recipient, async receipt => // Fixed internal callback keeps arbitrary consumers out of the public surface.
        { // Provider scalars remain untrusted until exact membership, reason and complete owners pass.
            var inspected = await provider.InspectPreparedMaterialAsync(this, receipt, token).ConfigureAwait(false); // Consume the actual full packet once through the existing concrete provider owner.
            if (!inspected.IsSuccess || inspected.Value is null) return inspected; // A refusal creates no saved choice or effect.
            if (!inspected.Value.IsIssuedBy(provider, receipt)) return RecoveryRequired<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticSelectionMaterialInspection>(); // Reject foreign provider/receipt lineage.
            var calibration = baseline.Readback.Baseline.Calibration; var choice = inspected.Value.Choice; // Validate only sanitized choice metadata, keeping closed material private.
            var guarded = _earlyGrant!.SourceSelection!.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal(_ordinaryStart!.RepositoryRoot, baseline.Context.Corpus, calibration.RepresentativeAssessment.Cases, calibration.Calibration.Observations, token, [choice.Reason, choice.MaterialAdmissionEvidenceSha256]); // Existing complete exclusion rejects closed reason/evidence aliases before returning inspection.
            return guarded.IsSuccess ? inspected : guarded.MapError<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticSelectionMaterialInspection>(); // Post-consumer final checks still run before any result escapes.
        }, token); // Preserve the caller's cancellation token without skipping final None checks.

    private enum SelectedMaterialRecipient { Ordinary, SyntheticForTesting, ProtectedStartupApprovalInspection, ProtectedStartupNativePreparation } // Fixed wrappers alone select each recipient.
    internal OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext> PrepareOrdinarySelectedTeachingMaterialContext( // Ordinary recipients require disclosure authority before any raw material.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Reuse the exact already-produced ORIGINAL; no evaluation factory or inference is called.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Bind the current genuine grant and selector throughout finite revalidation.
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission? admission, CancellationToken token = default) // Synthetic eligibility cannot authorize this ordinary recipient.
        => PrepareSelectedTeachingMaterialContext(baseline, catalog, selector, admission, SelectedMaterialRecipient.Ordinary, token); // Ordinary recipients never accept synthetic eligibility as disclosure rights.
    internal OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext> PrepareOrdinarySelectedTeachingMaterialContextSyntheticForTesting( // Keep fixture-only raw use explicit at every caller.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Retain the same held baseline owner.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Retain the actual receiving grant and selector.
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission? admission, CancellationToken token = default) // Test naming never admits missing or foreign eligibility.
        => PrepareSelectedTeachingMaterialContext(baseline, catalog, selector, admission, SelectedMaterialRecipient.SyntheticForTesting, token); // Share full-material capture and every final-owner check.
    private OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext> PrepareSelectedTeachingMaterialContext( // Keep recipient routing private rather than exposing a production guard switch.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Inspect the existing held baseline without another effect.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Preserve the existing actual ownership checks.
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission? admission, SelectedMaterialRecipient recipient, CancellationToken token) // Only the named wrappers choose this private recipient.
    { // Final checks also run when the primary read/recipient fails or observes cancellation.
        OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext> Capture() // Keep staged raw material private until the outer final owner checks finish.
        { // Validate receiving ownership before touching any selected payload.
            var owned = CheckOrdinaryDiagnosticOwnership(baseline, catalog, selector); // Include held custody, complete baseline/context and genuine owner identity.
            if (!owned.IsSuccess) return owned.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext>(); // Foreign/disposed/stale receiving owners never publish material.
            token.ThrowIfCancellationRequested(); // Cancellation cannot become material authorization.
            var saved = TryReadOrdinarySelectionBinding(baseline, catalog, selector, token); // Revalidate the actual saved choice without resolving the operator.
            if (!saved.IsSuccess || saved.Value is null) return saved.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext>(); // Preserve the exact saved-choice refusal.
            if (admission is null) return Failure<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext>(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, "Selected teaching material admission is required."); // A descriptive saved digest alone grants no payload admission.
            var witness = InspectOrdinaryOriginalTeachingExclusion(baseline, catalog, selector, token); // Reconstruct all protected ORIGINAL and used/unused teaching identities before payload access.
            if (!witness.IsSuccess || witness.Value is null) return witness.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext>(); // Preserve actual whole-owner and overlap errors.
            var p = saved.Value.Payload; // Use only the issued saved payload after receiving checks.
            var input = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(p.OriginalReceiptId, p.OriginalReceiptSha256, p.Categories, witness.Value.TeachingIdentities); // Rebuild the entire current canonical input from genuine owners.
            if (!input.IsSuccess || input.Value is null) return input.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext>(); // Invalid input never reaches a recipient.
            var matched = admission.Matches(ComputeBindingSha256(_snapshot.Binding), input.Value, p); // Bind the opaque object to whole receiving/SOURCE/input/selected-link/saved evidence identities.
            if (!matched.IsSuccess) return matched.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext>(); // Refuse stale, foreign, forged or wrong-purpose eligibility before raw publication.
            if (recipient != SelectedMaterialRecipient.SyntheticForTesting || admission.Origin != CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialOrigin.SyntheticTest) // Matching synthetic and unknown lineage cannot authorize an ordinary raw recipient.
                return Failure<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext>(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, "Selected teaching material admission does not authorize this recipient."); // Refuse before Open/Capture inside the existing finite final-owner scope.
            var calibration = baseline.Readback.Baseline.Calibration; // Reuse every saved ORIGINAL representative and repeat cell.
            var material = _earlyGrant!.SourceSelection!.OpenSelectedTeachingMaterialExcludingOriginal(_ordinaryStart!.RepositoryRoot, // Open only through the existing full SOURCE owner.
                p.ItemId, p.SourceId, p.ReferenceEntryId, p.SourceEntityFingerprint, baseline.Context.Corpus, // Preserve the exact nonordinal selected join and complete protected corpus.
                calibration.RepresentativeAssessment.Cases, calibration.Calibration.Observations, token); // Protect all saved answer bytes/digests and every unused teaching source.
            if (!material.IsSuccess || material.Value is null) return material.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext>(); // Discard staged bytes on genuine owner failure.
            token.ThrowIfCancellationRequested(); // Observe cancellation immediately before the actual payload recipient.
            var context = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CaptureSelectedTeachingMaterialContext(material.Value, admission, saved.Value); // Existing authoring consumes complete task/reference without a fake routing row or pair.
            token.ThrowIfCancellationRequested(); // Recipient-triggered cancellation remains primary and cannot publish a partial success.
            return context; // The outer finally must finish before this staged result is issued.
        } // Capture performs no history append, effect permit, author, optimizer or model call.
        OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext> result; // Stage success or exact primary refusal before finite cleanup.
        OperationResult<OrdinaryOriginalTeachingExclusion> final; // Existing full receiving/closed/teaching check owns final acceptance.
        try { result = Capture(); } // Primary exception/cancellation is preserved by normal stack unwinding.
        finally { final = FinishSelectionOwnerChecks(baseline, catalog, selector, "material"); } // CancellationToken.None inside this existing helper guarantees finite zero-effect final checks.
        if (!final.IsSuccess) return final.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext>(); // A late owner/ORIGINAL/unused SOURCE change invalidates staged material.
        return result; // Return complete detached bytes only after actual final revalidation succeeds.
    } // This checked boundary snapshot is not a future atomic lease or full teacher context.


    private OperationResult<T> WithSelectedStudentOwnerChecks<T>(Func<OperationResult<T>> check) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
    { // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        var previous = _studentCheckOwner.Value; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        _studentCheckOwner.Value = this; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        try { return check(); } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        finally { _studentCheckOwner.Value = previous; } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
    } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.

    private OperationResult<bool> AppendSelectedStudentBytes(byte[] line, string stage) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
    { // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        var prefix = WithSelectedStudentOwnerChecks(ReadCapacityPrefixV2); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        if (!prefix.IsSuccess) return prefix.MapError<bool>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        var next = _v2Prefix!.Concat(line).ToArray(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        var valid = ValidateSyntheticStudentPrefix(next, _snapshot.Binding, _sessionId, _capacityV2!, _earlyGrant!, true, _studentLease!); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        if (!valid.IsSuccess) return valid.MapError<bool>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        if (_v2History is null || !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_v2History.SafeFileHandle, _historyPath)) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            return RecoveryRequired<bool>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        var before = SecureLocalFileBoundary.ReadBoundedStableBytes(_v2History, _capacityV2!.MaximumBytes); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        if (before is null || !before.AsSpan().SequenceEqual(_v2Prefix)) return RecoveryRequired<bool>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        SelectedStudentDurablePointForTesting?.Invoke(stage + "-before-append"); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        _v2History.Position = _v2History.Length; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        _v2History.Write(line); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        _v2History.Flush(flushToDisk: true); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        SelectedStudentDurablePointForTesting?.Invoke(stage + "-flushed"); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_ownerLock.SafeFileHandle, _lockPath) || // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_v2History.SafeFileHandle, _historyPath)) return RecoveryRequired<bool>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        var readback = SecureLocalFileBoundary.ReadBoundedStableBytes(_v2History, _capacityV2.MaximumBytes); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        if (readback is null || !readback.AsSpan().SequenceEqual(next)) return RecoveryRequired<bool>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        valid = ValidateSyntheticStudentPrefix(readback, _snapshot.Binding, _sessionId, _capacityV2, _earlyGrant!, true, _studentLease!); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        if (!valid.IsSuccess) return valid.MapError<bool>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        _v2Prefix = readback; _capacityV2 = valid.Value; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        _snapshot = _snapshot with { Revision = valid.Value!.PrefixFrames - 1, HistorySha256 = Sha256(readback) }; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        SelectedStudentDurablePointForTesting?.Invoke(stage + "-readback"); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        return OperationResult<bool>.Ok(true); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
    } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.

    private static bool StudentResponseExcludesOriginal(string response, // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
    { // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        var digest = Sha256(Utf8.GetBytes(response)); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        var calibration = baseline.Readback.Baseline.Calibration; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        return !calibration.RepresentativeAssessment.Cases.Any(c => c.StudentAnswerSha256 == digest) && // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            !calibration.Calibration.Observations.Any(o => o.StudentAnswerSha256.Contains(digest, StringComparer.Ordinal)); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
    } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.

    internal sealed class StudentResponseReadback // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
    { // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        private readonly CodeDocumentationOneLessonSession owner; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        private readonly byte[] responseBytes; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        private readonly string prefixSha256; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        internal CodeDocumentationSelectedStudentRequestV1 Request { get; } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        internal string Response { get; } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        internal string ModelStateAfterSha256 { get; } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        internal string StartedFrameSha256 { get; } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        internal string CompletedFrameSha256 { get; } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        internal byte[] CopyResponseUtf8() => responseBytes.ToArray(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        private StudentResponseReadback(CodeDocumentationOneLessonSession issuing, // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            CodeDocumentationSelectedStudentRequestV1 request, CodeDocumentationSelectedStudentFrameV1 completed, byte[][] lines) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        { // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            owner = issuing; Request = request; Response = completed.Response!; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            ModelStateAfterSha256 = completed.ModelStateAfterSha256!; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            responseBytes = Utf8.GetBytes(Response); prefixSha256 = Sha256(issuing._v2Prefix!); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            StartedFrameSha256 = Sha256(lines[2].AsSpan(0, lines[2].Length - 1).ToArray()); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            CompletedFrameSha256 = Sha256(lines[3].AsSpan(0, lines[3].Length - 1).ToArray()); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        internal bool HasTeacherHistoryIssuingOwner => owner._teacherContextIssuing && owner._studentResponseInFlight == 1 && // A saved data snapshot cannot independently issue history approval.
            ReferenceEquals(owner._studentCheckOwner.Value, owner) && HasCurrentIssuingOwner; // Require this same logical held operation and exact current Completed prefix.
        internal bool HasCurrentIssuingOwner => !owner._disposed && owner._studentLease is not null && // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            owner._capacityV2?.PrefixFrames == 4 && owner._v2Prefix is not null && // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            prefixSha256 == Sha256(owner._v2Prefix) && // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            responseBytes.AsSpan().SequenceEqual(Utf8.GetBytes(Response)) && // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            owner.WithSelectedStudentOwnerChecks(owner.ReadCapacityPrefixV2).IsSuccess; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        internal static OperationResult<StudentResponseReadback> InspectAndIssue(CodeDocumentationOneLessonSession owner) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        { // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            if (!owner._studentIssuing || owner._studentLease is null || owner._studentResponseInFlight != 1 || // An in-flight flag alone does not authorize a caller to mint the private readback.
                !owner.WithSelectedStudentOwnerChecks(owner.ReadCapacityPrefixV2).IsSuccess || // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                owner._capacityV2?.PrefixFrames != 4) return RecoveryRequired<StudentResponseReadback>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            var lines = SyntheticStudentPhysicalLines(owner._v2Prefix!)!; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            var started = CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentStarted(lines[2], SelectionProjection(owner._snapshot.Binding)); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            if (!started.IsSuccess || started.Value?.Request is not { } request) return started.MapError<StudentResponseReadback>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            var completed = CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentCompleted(lines[3], SelectionProjection(owner._snapshot.Binding), // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                request, Sha256(lines[2].AsSpan(0, lines[2].Length - 1).ToArray())); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            if (!completed.IsSuccess || completed.Value is null) return completed.MapError<StudentResponseReadback>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            return OperationResult<StudentResponseReadback>.Ok(new(owner, request, completed.Value, lines)); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
    } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.

    internal async Task<OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>> PrepareOrdinarySelectedStudentContextForTestingAsync( // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission admission, CancellationToken cancellationToken = default) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
    { // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        if (_studentLease is null || _studentPoisoned || // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            Interlocked.CompareExchange(ref _studentResponseInFlight, 1, 0) != 0) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        CodeDocumentationRepositoryOwnedPreparedBaselineJournal.SyntheticSelectedTaskStudentOperation? operation = null; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        Exception? primary = null; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>? staged = null; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        OperationResult<OrdinaryOriginalTeachingExclusion>? final = null; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        async Task<OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>> Capture() // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        { // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            var prefix = WithSelectedStudentOwnerChecks(ReadCapacityPrefixV2); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            if (!prefix.IsSuccess || prefix.Value!.PrefixFrames is not (2 or 4)) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            var replay = prefix.Value.PrefixFrames == 4; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            if (!replay && _studentAttemptStarted) return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            var materialContext = WithSelectedStudentOwnerChecks(() => PrepareOrdinarySelectedTeachingMaterialContextSyntheticForTesting(baseline, catalog, selector, admission, cancellationToken)); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            if (!materialContext.IsSuccess || materialContext.Value is null) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                return materialContext.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            var p = _selectionFrame!.Payload; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            var calibration = baseline.Readback.Baseline.Calibration; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            var material = _earlyGrant!.SourceSelection!.OpenSelectedTeachingMaterialExcludingOriginal(_ordinaryStart!.RepositoryRoot, // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                p.ItemId, p.SourceId, p.ReferenceEntryId, p.SourceEntityFingerprint, baseline.Context.Corpus, // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                calibration.RepresentativeAssessment.Cases, calibration.Calibration.Observations, cancellationToken); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            if (!material.IsSuccess || material.Value is null) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                return material.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            var start = _ordinaryStart!; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            if (_earlyGrant.CheckpointSelection!.Parent.WeightsByteLength > start.MaximumCheckpointBytes) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                return CapacityExceeded<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            var lines = SyntheticStudentPhysicalLines(_v2Prefix!)!; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            var journal = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            var binding = SelectionProjection(_snapshot.Binding); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            if (replay) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            { // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                var saved = CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentStarted(lines[2], binding); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                if (!saved.IsSuccess || saved.Value?.Request is not { } savedRequest) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                    return saved.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                var checkedRequest = await journal.ValidateSyntheticSelectedStudentRequestAsync(catalog, selector, baseline.ExternalReference, // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                    start.RepositoryRoot, start.DomainSlug, start.DesignPatternSlug, start.ModelIdSlug, start.ModelVersion, // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                    start.ModelConfig, start.RuntimeCompatibility, baseline.Context, start.LoraConfig, start.AdapterSeedAuthority, // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                    material.Value, binding, lines[1], CodeDocumentationOneLessonV2Wire.SelectedStudentLeaseSha256(), savedRequest, cancellationToken).ConfigureAwait(false); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                if (!checkedRequest.IsSuccess) return checkedRequest.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            else // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            { // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                var prepared = await journal.PrepareSyntheticSelectedTaskStudentOperationAsync(catalog, selector, baseline.ExternalReference, // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                    start.RepositoryRoot, start.DomainSlug, start.DesignPatternSlug, start.ModelIdSlug, start.ModelVersion, // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                    start.ModelConfig, start.RuntimeCompatibility, baseline.Context, start.LoraConfig, start.AdapterSeedAuthority, // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                    material.Value, binding, lines[1], CodeDocumentationOneLessonV2Wire.SelectedStudentLeaseSha256(), cancellationToken).ConfigureAwait(false); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                if (!prepared.IsSuccess || prepared.Value is null) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                    return prepared.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                operation = prepared.Value; _activeSelectedStudentOperation = operation; // Retain only the current private operation until finite cleanup releases its model references.
                var before = WithSelectedStudentOwnerChecks(() => FinishSelectionOwnerChecks(baseline, catalog, selector, "student-prepared")); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                if (!before.IsSuccess) return before.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                var n = checked(_snapshot.Binding.DescriptorsV2!.CaseCount * _snapshot.Binding.DescriptorsV2.RepeatCount); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                if (_capacityV2!.RemainingFrames < 18 + 2 * n || _capacityV2.RemainingBytes < 592493L + 6548L * n) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                    return CapacityExceeded<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                cancellationToken.ThrowIfCancellationRequested(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                _studentAttemptStarted = true; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                var startedLine = CodeDocumentationOneLessonV2Wire.CanonicalSelectedStudentStartedFrameBytes(binding, _sessionId, operation.Request); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                var appended = AppendSelectedStudentBytes(startedLine, "student-started"); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                if (!appended.IsSuccess) return appended.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                SelectedStudentDurablePointForTesting?.Invoke("student-before-generate"); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                var generated = operation.ExecuteOnce(cancellationToken); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                SelectedStudentDurablePointForTesting?.Invoke("student-after-generate"); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                if (!generated.IsSuccess || generated.Value is null) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                    return generated.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                if (!operation.HasUnchangedState || !StudentResponseExcludesOriginal(generated.Value.Response, baseline)) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                    return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                var after = WithSelectedStudentOwnerChecks(() => FinishSelectionOwnerChecks(baseline, catalog, selector, "student-generated")); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                if (!after.IsSuccess) return after.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                cancellationToken.ThrowIfCancellationRequested(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                if (_capacityV2!.RemainingFrames < 17 + 2 * n || _capacityV2.RemainingBytes < 456354L + 6548L * n) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                    return CapacityExceeded<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                var completedLine = CodeDocumentationOneLessonV2Wire.CanonicalSelectedStudentCompletedFrameBytes(binding, _sessionId, // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                    operation.Request, Sha256(startedLine.AsSpan(0, startedLine.Length - 1).ToArray()), generated.Value.Response, generated.Value.ModelStateAfterSha256); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                appended = AppendSelectedStudentBytes(completedLine, "student-completed"); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                if (!appended.IsSuccess) return appended.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            var owners = WithSelectedStudentOwnerChecks(() => FinishSelectionOwnerChecks(baseline, catalog, selector, "student-complete")); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            if (!owners.IsSuccess) return owners.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            if (operation is not null && !operation.HasUnchangedState) return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Recheck private model state after append callbacks before issuing any recipient payload.
            OperationResult<StudentResponseReadback> response; _studentIssuing = true; // Open private issuance only after the complete durable prefix and full owner checks.
            try { response = StudentResponseReadback.InspectAndIssue(this); } finally { _studentIssuing = false; } // Close issuance before any recipient or external lifecycle callback.
            if (!response.IsSuccess || response.Value is null) return response.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            if (!StudentResponseExcludesOriginal(response.Value.Response, baseline)) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
                return RecoveryRequired<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            cancellationToken.ThrowIfCancellationRequested(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            var context = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CaptureSelectedStudentContext(materialContext.Value, response.Value); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            cancellationToken.ThrowIfCancellationRequested(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            return context; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        try { staged = await Capture().ConfigureAwait(false); } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        catch (Exception ex) when (SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { primary = ex; } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        finally // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        { // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            final = WithSelectedStudentOwnerChecks(() => FinishSelectionOwnerChecks(baseline, catalog, selector, "student")); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            try { if (operation is not null && !operation.HasUnchangedState) { staged = RecoveryRequired<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); if (primary is not null) primary.Data["SelectedStudentFinalStateMismatch"] = true; } } // Perform a finite final private-model check even after primary failure; never retry or expose a changed-state payload.
            catch (Exception ex) when (SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { if (primary is null) primary = ex; else primary.Data["SelectedStudentFinalStateFailure"] = ex; } // Preserve the exact primary exception while retaining a separate finite state-check failure.
            try { SelectedStudentDurablePointForTesting?.Invoke("student-final-owner-checked"); } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            catch (Exception ex) when (SnapshotExceptionGraphClassifier.IsNonFatal(ex)) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            { if (primary is null) primary = ex; else primary.Data["SelectedStudentFinalHookFailure"] = ex; } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            if (_studentAttemptStarted && _capacityV2?.PrefixFrames != 4) _studentPoisoned = true; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            operation?.Dispose(); _activeSelectedStudentOperation = null; // Release transient resources without erasing or retrying any durable effect.
            Interlocked.Exchange(ref _studentResponseInFlight, 0); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        if (primary is not null) // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        { // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            if (!final!.IsSuccess) primary.Data["SelectedStudentFinalOwnerErrorCode"] = final.ErrorCode; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(primary).Throw(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        if (!final!.IsSuccess) return final.MapError<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>(); // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
        return staged!; // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.
    } // Serialize the private one-shot Student lifecycle, persist exact output, and preserve primary failures through finite owner checks.

    private OperationResult<bool> CheckSelectionPayload( // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        CodeDocumentationOneLessonGrantCatalog catalog,CodeDocumentationOneLessonSelectorV1 selector, // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        CodeDocumentationTeachingSelectionPayloadV1 payload,CancellationToken token) // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
    { // Begin validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        var guard=CheckOrdinaryDiagnosticOwnership(baseline,catalog,selector); // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        if(!guard.IsSuccess) return guard; // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        var diagnosis=ClassifyOrdinaryBaseline(baseline,catalog,selector,token); // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        if(!diagnosis.IsSuccess) return diagnosis.MapError<bool>(); // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        var witness=InspectOrdinaryOriginalTeachingExclusion(baseline,catalog,selector,token); // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        if(!witness.IsSuccess || witness.Value is null) return witness.MapError<bool>(); // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        var input=CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(diagnosis.Value!.OriginalReceiptId, // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
            diagnosis.Value.OriginalReceiptSha256,diagnosis.Value.Categories,witness.Value.TeachingIdentities); // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        if(!input.IsSuccess || input.Value is null) return input.MapError<bool>(); // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        var graph=witness.Value.TeachingIdentities; // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        var item=graph.Items.SingleOrDefault(i=>i.DeclaredItem.ItemId==payload.ItemId); // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        if(payload.OriginalReceiptId!=baseline.ExternalReference.ReceiptId || // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
            payload.OriginalReceiptSha256!=baseline.ExternalReference.ReceiptSha256 || // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
            payload.SourceSha256!=graph.Selection.SourceSelectionSha256 || // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
            !payload.Categories.SequenceEqual(diagnosis.Value.Categories,StringComparer.Ordinal) || // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
            payload.FrozenContextSha256!=input.Value.FrozenContextSha256 || item is null || // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
            item.DeclaredItem.SourceId!=payload.SourceId || item.Entry.EntryId!=payload.ReferenceEntryId || // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
            item.Entry.SourceEntityFingerprint!=payload.SourceEntityFingerprint || // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
            !graph.CategoryBindings.Any(c=>payload.Categories.Contains(c.Category,StringComparer.Ordinal) && // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
                c.OrderedItemIds.Contains(payload.ItemId,StringComparer.Ordinal))) return RecoveryRequired<bool>(); // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        var calibration=baseline.Readback.Baseline.Calibration; // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        OperationResult<CodeDocumentationOneLessonSourceSelection.DeclaredTeachingCorpusReadback> metadata; // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        OperationResult<OrdinaryOriginalTeachingExclusion> final; // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        try // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        { // Begin validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
            SelectionDurablePointForTesting?.Invoke("metadata-before-open"); // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
            metadata=_earlyGrant!.SourceSelection!.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal( // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
                _ordinaryStart!.RepositoryRoot,baseline.Context.Corpus,calibration.RepresentativeAssessment.Cases, // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
                calibration.Calibration.Observations,token,[payload.Reason,payload.MaterialAdmissionEvidenceSha256, // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
                    payload.InvocationId,payload.LaunchSettingsSha256,payload.LaunchAckSha256, // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
                    CodeDocumentationTeachingSelectionPayloadV1.PolicyValue,"SyntheticTest","gpt-6-astra","ultra","UNKNOWN"]); // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        } // Finish validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        finally // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        { // Begin validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
            // Cancellation cannot skip finite zero-effect custody checks. The primary exception remains observable.
            final=FinishSelectionOwnerChecks(baseline,catalog,selector,"metadata"); // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        } // Finish validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        // Always finish receiving/closed/teaching rechecks, including a metadata refusal.
        if(!final.IsSuccess) return final.MapError<bool>(); // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        var finalInput=CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(diagnosis.Value.OriginalReceiptId, // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
            diagnosis.Value.OriginalReceiptSha256,diagnosis.Value.Categories,final.Value!.TeachingIdentities); // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        if(!finalInput.IsSuccess || finalInput.Value!.FrozenContextSha256!=input.Value.FrozenContextSha256) // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
            return RecoveryRequired<bool>(); // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
        return metadata.IsSuccess ? CheckOrdinaryDiagnosticOwnership(baseline,catalog,selector) : metadata.MapError<bool>(); // Validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.
    } // Finish validate the choice against frozen diagnostic categories and complete teaching graph, then recheck exclusion on every metadata exit.

    internal Task<OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContent>> PrepareOrdinarySelectedTeacherContentForTestingAsync( // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission materialAdmission, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedHistoryAdmission? savedHistoryAdmission, // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
        Func<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SyntheticHistoryAssessmentInput, CancellationToken, OperationResult<bool>>? assessment, CancellationToken cancellationToken = default) // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
        => PrepareOrdinarySelectedTeacherProjectionAsync(SelectedMaterialRecipient.SyntheticForTesting, baseline, catalog, selector, materialAdmission, savedHistoryAdmission, assessment, // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
            (content, _) => OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContent>.Ok(content), cancellationToken); // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
    internal Task<OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherPreparedRequest>> PrepareOrdinarySelectedTeacherRequestAsync( // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission materialAdmission, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedHistoryAdmission? savedHistoryAdmission, // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
        Func<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SyntheticHistoryAssessmentInput, CancellationToken, OperationResult<bool>>? assessment, ContinuousLearningTp1DpoConfiguration configuration, // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
        ConfiguredQwenVocabularyLease native, CancellationToken cancellationToken = default) // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
        => PrepareOrdinarySelectedTeacherProjectionAsync(SelectedMaterialRecipient.Ordinary, baseline, catalog, selector, materialAdmission, savedHistoryAdmission, assessment, // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
            (content, token) => CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PrepareSelectedTeacherRequest(content, configuration, native, token), cancellationToken, native.ValidateCurrent); // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
    internal Task<OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherPreparedRequest>> PrepareOrdinarySelectedTeacherRequestNativeResourcesSyntheticForTestingAsync( // Native vocabulary resources exercise the synthetic fixture recipient without granting ordinary disclosure rights.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Native vocabulary resources exercise the synthetic fixture recipient without granting ordinary disclosure rights.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Native vocabulary resources exercise the synthetic fixture recipient without granting ordinary disclosure rights.
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission materialAdmission, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedHistoryAdmission? savedHistoryAdmission, // Native vocabulary resources exercise the synthetic fixture recipient without granting ordinary disclosure rights.
        Func<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SyntheticHistoryAssessmentInput, CancellationToken, OperationResult<bool>>? assessment, ContinuousLearningTp1DpoConfiguration configuration, // Native vocabulary resources exercise the synthetic fixture recipient without granting ordinary disclosure rights.
        ConfiguredQwenVocabularyLease native, CancellationToken cancellationToken = default) // Native vocabulary resources exercise the synthetic fixture recipient without granting ordinary disclosure rights.
        => PrepareOrdinarySelectedTeacherProjectionAsync(SelectedMaterialRecipient.SyntheticForTesting, baseline, catalog, selector, materialAdmission, savedHistoryAdmission, assessment, // Native vocabulary resources exercise the synthetic fixture recipient without granting ordinary disclosure rights.
            (content, token) => CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PrepareSelectedTeacherRequest(content, configuration, native, token), cancellationToken, native.ValidateCurrent); // Native vocabulary resources exercise the synthetic fixture recipient without granting ordinary disclosure rights.
    internal Task<OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherPreparedRequest>> PrepareOrdinarySelectedTeacherRequestSyntheticForTestingAsync( // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission materialAdmission, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedHistoryAdmission? savedHistoryAdmission, // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
        Func<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SyntheticHistoryAssessmentInput, CancellationToken, OperationResult<bool>>? assessment, ContinuousLearningTp1DpoConfiguration configuration, // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
        Func<string, CancellationToken, OperationResult<int[]>> provider, CancellationToken cancellationToken = default) // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
        => PrepareOrdinarySelectedTeacherProjectionAsync(SelectedMaterialRecipient.SyntheticForTesting, baseline, catalog, selector, materialAdmission, savedHistoryAdmission, assessment, // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.
            (content, token) => CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PrepareSelectedTeacherRequestSyntheticForTesting(content, configuration, provider, token), cancellationToken); // Fixed recipient wrappers preserve the existing CAS, primary exception and final owner release.

    private async Task<OperationResult<T>> PrepareOrdinarySelectedTeacherProjectionAsync<T>( // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        SelectedMaterialRecipient recipient, // Only fixed wrappers choose the ordinary or explicitly synthetic material entry.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission materialAdmission, // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedHistoryAdmission? savedHistoryAdmission, // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        Func<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SyntheticHistoryAssessmentInput, CancellationToken, OperationResult<bool>>? assessment, // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        Func<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContent, CancellationToken, OperationResult<T>> projection, // Project before both final None checks and before releasing the held issuer.
        CancellationToken cancellationToken = default, Func<CancellationToken, OperationResult<bool>>? finalProjectionCheck = null) // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
    { // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        if (_studentLease is null || _studentPoisoned || _disposed || Interlocked.CompareExchange(ref _studentResponseInFlight, 1, 0) != 0) // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            return RecoveryRequired<T>(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        var previous = _studentCheckOwner.Value; _studentCheckOwner.Value = this; // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        Exception? primary = null; // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        OperationResult<T>? staged = null; // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        OperationResult<bool>? finalResource = null; // Revalidate the held native resource with None before exposing any staged projection.
        OperationResult<OrdinaryOriginalTeachingExclusion>? finalOwner = null; // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        OperationResult<CodeDocumentationOneLessonSourceSelection.DeclaredContextHistoryReadback>? finalHistory = null; // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        CodeDocumentationOneLessonSourceSelection.DeclaredContextHistoryReadback? capturedHistory = null; // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        OperationResult<CodeDocumentationOneLessonSourceSelection.DeclaredContextHistoryReadback> ReadHistory(CancellationToken token) // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        { // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            if (_earlyGrant?.SourceSelection is null || _ordinaryStart is null || baseline is null) // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                return RecoveryRequired<CodeDocumentationOneLessonSourceSelection.DeclaredContextHistoryReadback>(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            var calibration = baseline.Readback.Baseline.Calibration; // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            return _earlyGrant.SourceSelection.OpenDeclaredContextHistoryExcludingOriginal(_ordinaryStart.RepositoryRoot, baseline.Context.Corpus, // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                calibration.RepresentativeAssessment.Cases, calibration.Calibration.Observations, token); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        } // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        async Task<OperationResult<T>> Capture() // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        { // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            var prefix = ReadCapacityPrefixV2(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            if (!prefix.IsSuccess || prefix.Value?.PrefixFrames != 4) // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                return RecoveryRequired<T>(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            if ((savedHistoryAdmission is null) == (assessment is null)) // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                return Failure<T>( // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                    savedHistoryAdmission is null ? CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.HistoryAdmissionRequiredErrorCode : // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.HistoryAdmissionInvalidErrorCode, "Exactly one Synthetic history issuance or saved admission is required."); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            var material = recipient == SelectedMaterialRecipient.SyntheticForTesting // Each fixed wrapper uses its explicitly named material entry before the second raw reopen.
                ? PrepareOrdinarySelectedTeachingMaterialContextSyntheticForTesting(baseline, catalog, selector, materialAdmission, cancellationToken) // Synthetic Teacher fixtures share the same full-material capture and final checks as the Student fixture.
                : PrepareOrdinarySelectedTeachingMaterialContext(baseline, catalog, selector, materialAdmission, cancellationToken); // The ordinary Teacher request always reaches the guarded ordinary material entry.
            if (!material.IsSuccess || material.Value is null) return material.MapError<T>(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            var p = _selectionFrame!.Payload; var calibration = baseline.Readback.Baseline.Calibration; // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            var payload = _earlyGrant!.SourceSelection!.OpenSelectedTeachingMaterialExcludingOriginal(_ordinaryStart!.RepositoryRoot, // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                p.ItemId, p.SourceId, p.ReferenceEntryId, p.SourceEntityFingerprint, baseline.Context.Corpus, // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                calibration.RepresentativeAssessment.Cases, calibration.Calibration.Observations, cancellationToken); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            if (!payload.IsSuccess || payload.Value is null) return payload.MapError<T>(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            var history = ReadHistory(cancellationToken); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            if (!history.IsSuccess || history.Value is null) return history.MapError<T>(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            capturedHistory = history.Value; // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            var lines = SyntheticStudentPhysicalLines(_v2Prefix!)!; var binding = SelectionProjection(_snapshot.Binding); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            var started = CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentStarted(lines[2], binding); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            if (!started.IsSuccess || started.Value?.Request is not { } request) return started.MapError<T>(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            var start = _ordinaryStart; var journal = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            var validated = await journal.ValidateSyntheticSelectedStudentRequestAsync(catalog, selector, baseline.ExternalReference, // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                start.RepositoryRoot, start.DomainSlug, start.DesignPatternSlug, start.ModelIdSlug, start.ModelVersion, // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                start.ModelConfig, start.RuntimeCompatibility, baseline.Context, start.LoraConfig, start.AdapterSeedAuthority, // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                payload.Value, binding, lines[1], CodeDocumentationOneLessonV2Wire.SelectedStudentLeaseSha256(), request, cancellationToken).ConfigureAwait(false); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            if (!validated.IsSuccess) return validated.MapError<T>(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            OperationResult<StudentResponseReadback> response; _studentIssuing = true; // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            try { response = StudentResponseReadback.InspectAndIssue(this); } finally { _studentIssuing = false; } // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            if (!response.IsSuccess || response.Value is null) return response.MapError<T>(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            if (!StudentResponseExcludesOriginal(response.Value.Response, baseline)) return RecoveryRequired<T>(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            var student = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CaptureSelectedStudentContext(material.Value, response.Value); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            if (!student.IsSuccess || student.Value is null) return student.MapError<T>(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            cancellationToken.ThrowIfCancellationRequested(); _teacherContextIssuing = true; // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            try // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            { // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                var admission = savedHistoryAdmission is null // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                    ? CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedHistoryAdmission.SyntheticForTesting(student.Value, history.Value, assessment!, cancellationToken) // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                    : OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedHistoryAdmission>.Ok(savedHistoryAdmission); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                if (!admission.IsSuccess || admission.Value is null) return admission.MapError<T>(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                var result = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContent.Capture(student.Value, history.Value, admission.Value); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                cancellationToken.ThrowIfCancellationRequested(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
                if (!result.IsSuccess || result.Value is null) return result.MapError<T>(); // Refuse capture before the fixed recipient sees incomplete context.
                return projection(result.Value, cancellationToken); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            } // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            finally { _teacherContextIssuing = false; } // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        } // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        try // Release transient issuing state even if a secondary cleanup exception escapes.
        { // Keep primary capture and finite final checks inside one unconditional release scope.
            try { staged = await Capture().ConfigureAwait(false); } // Capture the actual recipient outcome without returning before final None checks.
            catch (OperationCanceledException ex) { primary = ex; } // Cancellation is primary here; the shared snapshot classifier deliberately excludes OCE.
            catch (Exception ex) when (SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { primary = ex; } // Retain other nonfatal primary exceptions without broadening the shared classifier.
            finally // Attempt both owners for success, cancellation and primary failure.
            { // A secondary OCE must not replace the primary or suppress the other final owner.
                try { finalOwner = FinishSelectionOwnerChecks(baseline, catalog, selector, "teacher-context"); } // The existing owner boundary performs its complete check with CancellationToken.None.
                catch (Exception ex) when (ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) // Treat cleanup cancellation only within this held operation, retaining fatal exception propagation.
                { // Record owner cancellation separately while continuing the full history readback.
                    finalOwner = RecoveryRequired<OrdinaryOriginalTeachingExclusion>(); // An interrupted final owner cannot authorize a staged context.
                    if (primary is null) primary = ex; else primary.Data["TeacherContextFinalOwnerFailure"] = ex; // Preserve the exact original exception object when cleanup also fails.
                } // Complete the first final attempt before checking every historical owner.
                try // History validation remains finite and independent of the cancelled caller token.
                { // Read the entire history even after owner cancellation.
                    finalHistory = ReadHistory(CancellationToken.None); // Never turn caller cancellation into a skipped historical mutation check.
                    if (finalHistory.IsSuccess && finalHistory.Value is not null && capturedHistory is not null && !capturedHistory.SameGraph(finalHistory.Value)) // Bind the final full graph to the pre-assessment snapshot.
                        finalHistory = Failure<CodeDocumentationOneLessonSourceSelection.DeclaredContextHistoryReadback>(CodeDocumentationOneLessonSourceSelection.HistoryInvalidErrorCode, "Historical owner changed during context capture."); // A changed graph refuses the whole staged context, including saved opaque admissions.
                } // Finish the independent history check before invoking the final observation hook.
                catch (Exception ex) when (ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) // A secondary history OCE has the same local treatment as other cleanup failures.
                { // Keep the primary identity/token while retaining the distinct history failure.
                    finalHistory = RecoveryRequired<CodeDocumentationOneLessonSourceSelection.DeclaredContextHistoryReadback>(); // A cancelled history check never counts as admission.
                    if (primary is null) primary = ex; else primary.Data["TeacherContextFinalHistoryFailure"] = ex; // Expose secondary history diagnostics without replacing the primary.
                } // Both final checks have now been attempted for nonfatal/cancellation outcomes.
                if (finalProjectionCheck is not null) // Native resource checks do not inherit the cancelled caller token.
                { // Preserve the primary failure while retaining an independent final resource refusal.
                    try { finalResource = finalProjectionCheck(CancellationToken.None); } // Rehash the full retained resource graph after both source-owner attempts.
                    catch (Exception ex) when (ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) // Confine secondary resource exceptions to this held consumer.
                    { // A failed final resource check cannot authorize the staged body.
                        finalResource = Failure<bool>(ConfiguredQwenVocabularyLease.Changed, "Configured vocabulary changed during preparation."); // Fail closed independently of source validity.
                        if (primary is null) primary = ex; else primary.Data["TeacherContextFinalResourceFailure"] = ex; // Retain the exact original cancellation/exception identity.
                    } // Continue unconditional issuer/CAS release after the final resource attempt.
                } // The old content-only consumer has no native check and retains its previous behavior.
                try { SelectedStudentDurablePointForTesting?.Invoke("teacher-context-final-owner-checked"); } // Observe the completed finite cleanup without granting another inference.
                catch (Exception ex) when (ex is OperationCanceledException || SnapshotExceptionGraphClassifier.IsNonFatal(ex)) // Final-hook cancellation is secondary when capture already failed.
                { if (primary is null) primary = ex; else primary.Data["TeacherContextFinalHookFailure"] = ex; } // Retain final-hook diagnostics and let the outer finally release the owner.
            } // No recipient escapes while any of these owner checks failed.
        } // Keep all cancellation handling confined to this consumer method.
        finally // Release flags, logical owner and CAS independently of primary/secondary exception paths.
        { // Completed replay must not inherit an abandoned in-flight allowance.
            _teacherContextIssuing = false; _studentIssuing = false; // Remove both temporary issuer allowances before exposing any result or exception.
            _studentCheckOwner.Value = previous; // Restore the prior logical owner rather than leaking this held invocation.
            Interlocked.Exchange(ref _studentResponseInFlight, 0); // Release the CAS even when a final observation throws cancellation.
        } // Primary propagation below occurs only after unconditional state release.
        if (primary is not null) // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        { // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            if (finalResource is { IsSuccess: false }) primary.Data["TeacherContextFinalResourceErrorCode"] = finalResource.ErrorCode; // Record secondary resource refusal without replacing the primary.
            if (!finalOwner!.IsSuccess) primary.Data["TeacherContextFinalOwnerErrorCode"] = finalOwner.ErrorCode; // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            if (!finalHistory!.IsSuccess) primary.Data["TeacherContextFinalHistoryErrorCode"] = finalHistory.ErrorCode; // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(primary).Throw(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        } // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        if (!finalOwner!.IsSuccess) return finalOwner.MapError<T>(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        if (!finalHistory!.IsSuccess) return finalHistory.MapError<T>(); // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
        if (finalResource is { IsSuccess: false }) return finalResource.MapError<T>(); // A stale or disposed configured resource refuses the staged request.
        return staged!; // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.
    } // Reopen Completed only, bind the entire context and finish both None checks before returning; never Generate or append.


    private OperationResult<OrdinaryOriginalTeachingExclusion> FinishSelectionOwnerChecks( // Perform finite zero-effect final owner checks even when provider or metadata cancellation is primary.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Perform finite zero-effect final owner checks even when provider or metadata cancellation is primary.
        CodeDocumentationOneLessonGrantCatalog catalog,CodeDocumentationOneLessonSelectorV1 selector,string stage) // Perform finite zero-effect final owner checks even when provider or metadata cancellation is primary.
    { // Begin perform finite zero-effect final owner checks even when provider or metadata cancellation is primary.
        try // Perform finite zero-effect final owner checks even when provider or metadata cancellation is primary.
        { // Begin perform finite zero-effect final owner checks even when provider or metadata cancellation is primary.
            var result=InspectOrdinaryOriginalTeachingExclusion(baseline,catalog,selector,CancellationToken.None); // Perform finite zero-effect final owner checks even when provider or metadata cancellation is primary.
            SelectionDurablePointForTesting?.Invoke(stage+"-final-owner-checked"); // Perform finite zero-effect final owner checks even when provider or metadata cancellation is primary.
            return result; // Perform finite zero-effect final owner checks even when provider or metadata cancellation is primary.
        } // Finish perform finite zero-effect final owner checks even when provider or metadata cancellation is primary.
        catch(Exception ex) when(SnapshotExceptionGraphClassifier.IsNonFatal(ex)) // Perform finite zero-effect final owner checks even when provider or metadata cancellation is primary.
        {return RecoveryRequired<OrdinaryOriginalTeachingExclusion>();} // Perform finite zero-effect final owner checks even when provider or metadata cancellation is primary.
    } // Finish perform finite zero-effect final owner checks even when provider or metadata cancellation is primary.

    private enum SelectionReceiverMode { Ordinary, SyntheticForTesting } // Only the explicitly named internal fixture entry admits the closed Synthetic provider factories.
    internal Task<OperationResult<SelectionBindingReadback>> BindOrdinarySelectionDecisionAsync( // Ordinary fresh ingress cannot turn a Synthetic callback into a real teaching choice.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Preserve the same genuine baseline and receiving owner checks.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Preserve the current catalog and selector binding.
        OrdinaryOriginalTeachingExclusion witness, CodeDocumentationOneLessonSelectionDecisionProvider provider, // Caller metadata cannot choose the private receiver mode.
        CancellationToken token=default) // Saved Completed metadata replay remains compatible.
        => BindSelectionDecisionCoreAsync(baseline,catalog,selector,witness,provider,SelectionReceiverMode.Ordinary,token); // Route ordinary callers through the fresh Synthetic refusal.
    internal Task<OperationResult<SelectionBindingReadback>> BindOrdinarySelectionDecisionSyntheticForTestingAsync( // Explicitly labelled fixture ingress shares all durable owner and no-repeat checks.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // A fixture still needs the actual saved baseline.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // A fixture still needs the exact live binding.
        OrdinaryOriginalTeachingExclusion witness, CodeDocumentationOneLessonSelectionDecisionProvider provider, // The closed factories retain SyntheticTest lineage.
        CancellationToken token=default) // Preserve primary cancellation and final owner checks.
        => BindSelectionDecisionCoreAsync(baseline,catalog,selector,witness,provider,SelectionReceiverMode.SyntheticForTesting,token); // No production caller uses this fixture entry.
    private async Task<OperationResult<SelectionBindingReadback>> BindSelectionDecisionCoreAsync( // Legacy metadata participates in the same owner exclusion as fresh material.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Preserve the genuine current baseline.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Fixed catalog identities remain mandatory.
        OrdinaryOriginalTeachingExclusion witness, CodeDocumentationOneLessonSelectionDecisionProvider provider, // The existing witness and provider cannot bypass entry ownership.
        SelectionReceiverMode receiverMode, CancellationToken token) // Recipient mode remains private to named wrappers.
    { // Acquire before prefix checks, attempt marking or a before-Genesis callback.
        if (_disposed || Interlocked.CompareExchange(ref _selectionMaterialInFlight, 1, 0) != 0) return RecoveryRequired<SelectionBindingReadback>(); // Public callback re-entry refuses unconditionally.
        try { return await BindSelectionDecisionAlreadyOwnedAsync(baseline, catalog, selector, witness, provider, receiverMode, token).ConfigureAwait(false); } // Run the unchanged metadata grammar under custody.
        finally { Interlocked.Exchange(ref _selectionMaterialInFlight, 0); } // Keep the gate through all finite legacy owner checks and cleanup.
    } // No recursive acquisition occurs in the private owned core.

    private async Task<OperationResult<SelectionBindingReadback>> BindSelectionDecisionAlreadyOwnedAsync( // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        CodeDocumentationOneLessonGrantCatalog catalog,CodeDocumentationOneLessonSelectorV1 selector, // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        OrdinaryOriginalTeachingExclusion witness,CodeDocumentationOneLessonSelectionDecisionProvider provider, // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        SelectionReceiverMode receiverMode,CancellationToken token) // Only the two internal entrypoints fix receiver mode; no JSON/model label can change it.
    { // Begin consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        var prefix=ReadCapacityPrefixV2(); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        if(!prefix.IsSuccess) return prefix.MapError<SelectionBindingReadback>(); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        if(prefix.Value!.PrefixFrames==2) return TryReadOrdinarySelectionBinding(baseline,catalog,selector,token); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        if(prefix.Value.PrefixFrames!=0 || _selectionAttemptStarted || provider is null || witness is null || // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            !witness.IsIssuedBy(this)) return RecoveryRequired<SelectionBindingReadback>(); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        var before=CheckOrdinaryDiagnosticOwnership(baseline,catalog,selector); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        if(!before.IsSuccess) return before.MapError<SelectionBindingReadback>(); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        var diagnosis=ClassifyOrdinaryBaseline(baseline,catalog,selector,token); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        if(!diagnosis.IsSuccess) return diagnosis.MapError<SelectionBindingReadback>(); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        var fresh=InspectOrdinaryOriginalTeachingExclusion(baseline,catalog,selector,token); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        if(!fresh.IsSuccess || fresh.Value is null) return fresh.MapError<SelectionBindingReadback>(); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        var input=CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(diagnosis.Value!.OriginalReceiptId, // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            diagnosis.Value.OriginalReceiptSha256,diagnosis.Value.Categories,fresh.Value.TeachingIdentities); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        var prior=CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(witness.OriginalReceiptId, // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            witness.OriginalReceiptSha256,diagnosis.Value.Categories,witness.TeachingIdentities); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        if(!input.IsSuccess || input.Value is null || !prior.IsSuccess || prior.Value is null || // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            prior.Value.FrozenContextSha256!=input.Value.FrozenContextSha256 || // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            witness.BindingSha256!=ComputeBindingSha256(_snapshot.Binding)) return RecoveryRequired<SelectionBindingReadback>(); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        if(receiverMode==SelectionReceiverMode.Ordinary) return CodeDocumentationOneLessonV2Wire.SelectionInvalid<SelectionBindingReadback>(); // Every existing sealed provider factory is Synthetic; refuse before attempt, Genesis or callback.
        _selectionAttemptStarted=true; // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        try // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        { // Begin consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            token.ThrowIfCancellationRequested(); SelectionDurablePointForTesting?.Invoke("before-genesis"); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            var genesis=_studentLease is null ? CodeDocumentationOneLessonV2Wire.CanonicalBytes(EarlyFrames(_snapshot.Binding,_sessionId,_earlyGrant!, // Preserve legacy Genesis bytes when private response mode was not explicitly opened.
                baseline.ExternalReference)[0]).Append((byte)'\n').ToArray() : SyntheticStudentGenesisBytes(_snapshot.Binding,_sessionId,_earlyGrant!); // Commit private capacity intent before operator invocation and retain durable no-repeat selection semantics.
            var append=AppendSelectionBytes(genesis,"genesis"); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            if(!append.IsSuccess) return append.MapError<SelectionBindingReadback>(); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            // The private token is born ONLY after durable flush and full readback.
            _selectionInFlight=new(provider,input.Value,genesis); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            var flight=_selectionInFlight; // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            before=CheckOrdinaryDiagnosticOwnership(baseline,catalog,selector); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            if(!before.IsSuccess) return before.MapError<SelectionBindingReadback>(); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            token.ThrowIfCancellationRequested(); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            flight.InvocationConsumed=true; // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.IssuedSelectionDecision> issued; // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            OperationResult<OrdinaryOriginalTeachingExclusion> after; // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            try // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            { // Begin consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
                issued=await provider.InvokeAsync(flight.Input,token).ConfigureAwait(false); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
                SelectionDurablePointForTesting?.Invoke("provider-returned"); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            } // Finish consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            finally {after=FinishSelectionOwnerChecks(baseline,catalog,selector,"provider");} // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            if(!after.IsSuccess) return after.MapError<SelectionBindingReadback>(); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            if(!issued.IsSuccess || issued.Value is null) return issued.MapError<SelectionBindingReadback>(); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            if(!ReferenceEquals(flight,_selectionInFlight) || !flight.InvocationConsumed || // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
                !ReferenceEquals(provider,flight.Provider) || !issued.Value.IsIssuedBy(provider)) // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
                return RecoveryRequired<SelectionBindingReadback>(); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            var valid=CheckSelectionPayload(baseline,catalog,selector,issued.Value.Payload,token); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            if(!valid.IsSuccess) return valid.MapError<SelectionBindingReadback>(); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            token.ThrowIfCancellationRequested(); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            var frame=CodeDocumentationOneLessonV2Wire.CanonicalTeachingSelectionFrameBytes(SelectionProjection(_snapshot.Binding), // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
                _sessionId,ComputeBindingSha256(_snapshot.Binding),Sha256(genesis[..^1]),issued.Value.Payload); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            append=AppendSelectionBytes(frame,"decision"); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            if(!append.IsSuccess) return append.MapError<SelectionBindingReadback>(); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
            var decoded=DecodeSelectionPrefix(_v2Prefix!,_snapshot.Binding,_sessionId,_earlyGrant!,_studentLease); // Decode the just-written Genesis with this held owner's private intent, exactly as reopen does.
            if(!decoded.IsSuccess || decoded.Value is null) return decoded.MapError<SelectionBindingReadback>(); // Refuse an invalid durable choice without issuing readback or retrying the consumed provider.
            _selectionFrame=decoded.Value; // Retain only the choice authenticated against this owner's ordinary or private Genesis grammar.
            return TryReadOrdinarySelectionBinding(baseline,catalog,selector,token); // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        } // Finish consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        catch(OperationCanceledException) {throw;} // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        catch(Exception ex) when(SnapshotExceptionGraphClassifier.IsNonFatal(ex)) // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        {return RecoveryRequired<SelectionBindingReadback>();} // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
        finally {_selectionInFlight=null;} // Consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.
    } // Finish consume one invocation only after durable Genesis readback, preserve primary cancellation and never repeat unknown effects.

    private OperationResult<bool> AppendSelectionBytes(byte[] lfInclusiveLine,string stage) // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
    { // Begin append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
        var prefix=ReadCapacityPrefixV2(); // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
        if(!prefix.IsSuccess) return prefix.MapError<bool>(); // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
        try // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
        { // Begin append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
            var next=_v2Prefix!.Concat(lfInclusiveLine).ToArray(); // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
            var valid=_fullMaterialLease is not null ? ValidateFullMaterialPrefix(next,_snapshot.Binding,_sessionId,_capacityV2!,true) : ValidateOrdinaryPrefixV2(next,_snapshot.Binding,_sessionId,_capacityV2!,_earlyGrant!,true,_studentLease); // Validate under the typed mode fixed before Genesis.
            if(!valid.IsSuccess) return valid.MapError<bool>(); // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
            if(_v2History is null) _v2History=SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(_historyPath, // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
                FileMode.CreateNew,FileAccess.ReadWrite,FileShare.Read,FileOptions.SequentialScan); // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
            if(!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_v2History.SafeFileHandle,_historyPath) || // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
                !SecureLocalFileBoundary.ReadBoundedStableBytes(_v2History,_capacityV2!.MaximumBytes)!.AsSpan().SequenceEqual(_v2Prefix)) // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
                return RecoveryRequired<bool>(); // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
            SelectionDurablePointForTesting?.Invoke(stage+"-before-write"); // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
            _v2History.Position=_v2History.Length; _v2History.Write(lfInclusiveLine); // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
            SelectionDurablePointForTesting?.Invoke(stage+"-after-write"); // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
            _v2History.Flush(flushToDisk:true); // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
            SelectionDurablePointForTesting?.Invoke(stage+"-flushed"); // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
            if(!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_ownerLock.SafeFileHandle,_lockPath) || // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
                !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_v2History.SafeFileHandle,_historyPath)) // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
                return RecoveryRequired<bool>(); // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
            var readback=SecureLocalFileBoundary.ReadBoundedStableBytes(_v2History,_capacityV2.MaximumBytes); // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
            if(readback is null || !readback.AsSpan().SequenceEqual(next)) return RecoveryRequired<bool>(); // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
            valid=_fullMaterialLease is not null ? ValidateFullMaterialPrefix(readback,_snapshot.Binding,_sessionId,_capacityV2,true) : ValidateOrdinaryPrefixV2(readback,_snapshot.Binding,_sessionId,_capacityV2,_earlyGrant!,true,_studentLease); // Revalidate exact flushed bytes under the same typed mode.
            if(!valid.IsSuccess) return valid.MapError<bool>(); // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
            _v2Prefix=readback; _capacityV2=valid.Value; // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
            if (_fullMaterialLease is not null && ReferenceEquals(_fullMaterialCheckOwner.Value,this)) _fullMaterialExpectedPrefix=readback.ToArray(); // A later hook cannot replace the expected prefix with arbitrary receiving memory.
            _snapshot=_snapshot with {Revision=valid.Value!.PrefixFrames-1,HistorySha256=Sha256(readback)}; // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
            SelectionDurablePointForTesting?.Invoke(stage+"-readback"); // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
            return OperationResult<bool>.Ok(true); // Append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
        } // Finish append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.
        catch(OperationCanceledException) when(_fullMaterialLease is not null) {throw;} // Full-family primary cancellation survives append instrumentation and independent None checks.
        catch(Exception ex) when(SnapshotExceptionGraphClassifier.IsNonFatal(ex)) {return RecoveryRequired<bool>();} // Preserve existing business refusal for partial durable writes.
    } // Finish append under the held custody handles, flush, reread exact bytes, and publish memory state only after validation.

    private static OperationResult<CodeDocumentationTeachingSelectionFrameV1> DecodeSelectionPrefix(byte[] bytes, // Authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
        CodeDocumentationOneLessonSessionBinding binding,string session,CodeDocumentationOneLessonGrantV1 grant, SyntheticStudentLeaseIntent? studentLease = null) // Decode saved choice with an explicit private mode, never an untrusted marker.
    { // Begin authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
        try // Authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
        { // Begin authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
            if (studentLease is not null) return DecodeSyntheticStudentChoice(bytes,binding,session,grant,studentLease); // Authenticate bounded private Genesis/selection before touching any response JSON.
            if (!V2PhysicalLinesWithin(bytes,32768)) return RecoveryRequired<CodeDocumentationTeachingSelectionFrameV1>(); // Refuse large untrusted lines before legacy UTF8/JSON conversion.
            var lines=Utf8.GetString(bytes).Split('\n'); // Authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
            if(lines.Length!=3 || lines[2].Length!=0 || bytes.Contains((byte)'\r')) // Authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
                return RecoveryRequired<CodeDocumentationTeachingSelectionFrameV1>(); // Authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
            var genesis=Utf8.GetBytes(lines[0]); // Authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
            var expected=CodeDocumentationOneLessonV2Wire.CanonicalBytes(EarlyFrames(binding,session,grant, // Authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
                new(new string('a',32),new string('b',64)))[0]); // Authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
            if(genesis.Length+1>2585 || !genesis.AsSpan().SequenceEqual(expected)) // Authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
                return RecoveryRequired<CodeDocumentationTeachingSelectionFrameV1>(); // Authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
            var decoded=CodeDocumentationOneLessonV2Wire.DecodeTeachingSelectionFrame(Utf8.GetBytes(lines[1]+"\n"),SelectionProjection(binding)); // Authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
            if(!decoded.IsSuccess || decoded.Value is null) return decoded; // Authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
            return decoded.Value.SessionId==session && decoded.Value.BindingSha256==ComputeBindingSha256(binding) && // Authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
                grant.SourceSelection is {} source && decoded.Value.Payload.SourceSha256==source.SourceSelectionSha256 && // Reject a canonical foreign SOURCE before the saved selection can be reattached or consumed.
                decoded.Value.GenesisFrameSha256==Sha256(genesis) // Authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
                ? decoded : RecoveryRequired<CodeDocumentationTeachingSelectionFrameV1>(); // Authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
        } // Finish authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
        catch(Exception ex) when(SnapshotExceptionGraphClassifier.IsNonFatal(ex)) // Authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
        {return RecoveryRequired<CodeDocumentationTeachingSelectionFrameV1>();} // Authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
    } // Finish authenticate the exact two-frame zero-effect prefix, immutable binding and original Genesis hash.
    private static OperationResult<CodeDocumentationOneLessonContinuationCapacityV2> ValidateOrdinaryPrefixV2(byte[] bytes, // Charge a complete selection prefix to ordinary capacity and delegate all other prefixes to existing early validation.
        CodeDocumentationOneLessonSessionBinding binding,string session,CodeDocumentationOneLessonContinuationCapacityV2 capacity, // Charge a complete selection prefix to ordinary capacity and delegate all other prefixes to existing early validation.
        CodeDocumentationOneLessonGrantV1 grant,bool allowIncomplete,SyntheticStudentLeaseIntent? studentLease = null) // Retain conditional grammar behind its genuine private intent.
    { // Begin charge a complete selection prefix to ordinary capacity and delegate all other prefixes to existing early validation.
        if (studentLease is not null) return ValidateSyntheticStudentPrefix(bytes,binding,session,capacity,grant,allowIncomplete,studentLease); // A private intent and exact small prefix must authorize the larger response positions.
        if (!V2PhysicalLinesWithin(bytes,32768)) return CapacityExceeded<CodeDocumentationOneLessonContinuationCapacityV2>(); // Default ordinary readers never infer a larger family ceiling from JSON Kind.
        var selection=DecodeSelectionPrefix(bytes,binding,session,grant); // Charge a complete selection prefix to ordinary capacity and delegate all other prefixes to existing early validation.
        if(selection.IsSuccess) // Charge a complete selection prefix to ordinary capacity and delegate all other prefixes to existing early validation.
        { // Begin charge a complete selection prefix to ordinary capacity and delegate all other prefixes to existing early validation.
            if(bytes.LongLength>capacity.MaximumBytes || capacity.MaximumFrames<2) // Charge a complete selection prefix to ordinary capacity and delegate all other prefixes to existing early validation.
                return CapacityExceeded<CodeDocumentationOneLessonContinuationCapacityV2>(); // Charge a complete selection prefix to ordinary capacity and delegate all other prefixes to existing early validation.
            return OperationResult<CodeDocumentationOneLessonContinuationCapacityV2>.Ok(capacity with {PrefixFrames=2,PrefixBytes=bytes.LongLength}); // Charge a complete selection prefix to ordinary capacity and delegate all other prefixes to existing early validation.
        } // Finish charge a complete selection prefix to ordinary capacity and delegate all other prefixes to existing early validation.
        return ValidateEarlyPrefixV2(bytes,binding,session,capacity,grant,allowIncomplete); // Charge a complete selection prefix to ordinary capacity and delegate all other prefixes to existing early validation.
    } // Finish charge a complete selection prefix to ordinary capacity and delegate all other prefixes to existing early validation.

    private static byte[] OrdinaryContextBytes(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context)
        => JsonSerializer.SerializeToUtf8Bytes(new { context.Corpus, context.CorpusManifest,
            context.EvaluatorConfiguration, context.GenerationSettings, context.NoisePolicyConfig,
            context.MaximumDuration, context.ObservedAtUtc, context.EvidenceRefs });

    private static byte[] OrdinaryReferenceBytes(
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline)
    {
        var r = baseline.Readback.Reference;
        return JsonSerializer.SerializeToUtf8Bytes(new { baseline.Readback.Address, baseline.Readback.RequestSha256,
            baseline.Readback.Sha256, baseline.Readback.ByteLength, baseline.Readback.Classification,
            r.FormatVersion, r.RepositoryRoot, r.ObjectiveSha256, r.BaselineRoot, r.BaselineRootRelativePath,
            r.BaselinePath, r.ClaimPath, r.LockPath, ReferenceRequest = r.RequestSha256, ReferenceSha = r.Sha256,
            ReferenceLength = r.ByteLength, ReferenceClassification = r.Classification,
            baseline.ExternalReference.ReceiptId, baseline.ExternalReference.ReceiptSha256 });
    }

    private static byte[] OrdinaryGrantBytes(CodeDocumentationOneLessonGrantV1 grant)
        => JsonSerializer.SerializeToUtf8Bytes(new { grant, grant.DescriptorsV2,
            grant.SourceSelection!.Sources, grant.SourceSelection.PairSelections,
            grant.CheckpointSelection!.Parent, grant.CheckpointSelection.Candidate,
            grant.CorpusSelection!.Corpus, grant.CorpusSelection.CaseIds,
            Plan = CodeDocumentationOneLessonV2Wire.CanonicalEvaluationPlanBytes(grant.EvaluationPlanSelection!),
            grant.EvaluatorSelection!.Evaluator, Profile = grant.ProfileSelection!.Snapshot });

    /// <summary>Only the sufficient branch may start. An interrupted durable prefix is never resumed.</summary>
    internal OperationResult<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2> CompleteEarlyBaselineSufficient( // Sufficient completion shares the selection owner gate.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline, // Existing sufficient baseline rules remain authoritative.
        CodeDocumentationBatchFileRunner batch, CodeDocumentationBatchFileRunner.CodeDocumentationProfileInputsOpenResult material, // Preserve the real profile training owner.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // No alternative grant is accepted.
        RepositoryOwnedModelArtifactRoot root, RepositoryOwnedModelRuntimeCompatibility compatibility, CancellationToken cancellationToken) // Retain runtime and cancellation binding.
    { // Exclude competing family staging before checks or terminal append.
        if (_disposed || Interlocked.CompareExchange(ref _selectionMaterialInFlight, 1, 0) != 0) return RecoveryRequired<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>(); // Fresh full entry cannot race terminal completion.
        try { SelectionDurablePointForTesting?.Invoke("early-sufficient-entered"); return CompleteEarlyBaselineSufficientAlreadyOwned(baseline, batch, material, catalog, selector, root, compatibility, cancellationToken); } // Observe the acquired gate before the original sufficient implementation.
        finally { Interlocked.Exchange(ref _selectionMaterialInFlight, 0); } // Every finite completion/refusal releases its own gate.
    } // The private core never acquires again.

    private OperationResult<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2> CompleteEarlyBaselineSufficientAlreadyOwned(
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline,
        CodeDocumentationBatchFileRunner batch,
        CodeDocumentationBatchFileRunner.CodeDocumentationProfileInputsOpenResult material,
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector,
        RepositoryOwnedModelArtifactRoot root, RepositoryOwnedModelRuntimeCompatibility compatibility,
        CancellationToken cancellationToken)
    {
        var prefix = ReadCapacityPrefixV2();
        if (!prefix.IsSuccess) return prefix.MapError<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>();
        if (_studentLease is not null || _studentResponseInFlight != 0 || _earlyGrant is null || baseline is null || !ReferenceEquals(baseline, _ordinaryBaseline) || prefix.Value!.PrefixFrames != 0 || // Private response mode grants no sufficient terminal or ordinary effect entry.
            !ReferenceEquals(catalog.Resolve(selector).Value, _earlyGrant))
            return RecoveryRequired<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>();
        if (!JsonSerializer.SerializeToUtf8Bytes(baseline.Readback.Baseline).AsSpan().SequenceEqual(_ordinaryBaselineBytes) ||
            !PreparedTokenizerBytes(baseline.Context.Tokenizer).AsSpan().SequenceEqual(_ordinaryTokenizerBytes) ||
            !baseline.Context.EvidenceRefs.SequenceEqual(_ordinaryEvidence!,StringComparer.Ordinal))
            return RecoveryRequired<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>();
        var quality = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.BaselineRequiresNewTrainingPair(baseline.Readback.Baseline);
        if (quality.IsSuccess || quality.ErrorCode != CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.BaselineAlreadySufficientErrorCode ||
            baseline.Readback.Classification != "NO_TRAINING_NEEDED")
            return Invalid<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>();
        var n = checked(_earlyGrant.DescriptorsV2!.CaseCount * _earlyGrant.DescriptorsV2.RepeatCount);
        if (baseline.Readback.Baseline.ExpectedEvaluationCallCount != n ||
            baseline.Readback.Baseline.ObservedEvaluationCallCount != n)
            return Invalid<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>();
        cancellationToken.ThrowIfCancellationRequested();
        var source = _earlyGrant.SourceSelection!.OpenFresh(root.RepositoryRoot);
        if (!source.IsSuccess) return source.MapError<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>();
        var parent = _earlyGrant.CheckpointSelection!.OpenFresh(root.RepositoryRoot, compatibility);
        if (!parent.IsSuccess) return parent.MapError<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>();
        var evaluator = _earlyGrant.EvaluatorSelection!.OpenFresh(root.RepositoryRoot, baseline.Context.GenerationSettings,
            baseline.Context.EvaluatorConfiguration, baseline.Context.NoisePolicyConfig,
            _earlyGrant.ConfigurationSha256, _earlyGrant.ExamConditionSha256);
        if (!evaluator.IsSuccess) return evaluator.MapError<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>();
        var freshMaterial = batch.OpenProfileInputsFresh(catalog, selector, root);
        if (!freshMaterial.IsSuccess) return freshMaterial.MapError<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>();
        if (!CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(material.Selection).AsSpan()
                .SequenceEqual(CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(freshMaterial.Value!.Selection)))
            return Invalid<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>();
        var frames = EarlyFrames(_snapshot.Binding, _sessionId, _earlyGrant, baseline.ExternalReference);
        try
        {
        for (var i = 0; i < frames.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (i == 3)
            {
                // Revision 2 has been flushed and exactly reread before the real Train invocation.
                var trained = batch.TrainSelectedProfileOnly(material, catalog, selector, root, this, cancellationToken);
                if (!trained.IsSuccess || trained.Value is null || trained.Value.Count == 0)
                    return RecoveryRequired<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>();
            }
            if (i == 5)
            {
                var final = _earlyGrant.CheckpointSelection.OpenFresh(root.RepositoryRoot, compatibility);
                if (!final.IsSuccess || final.Value!.CheckpointId != parent.Value!.CheckpointId)
                    return RecoveryRequired<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>();
            }
            if (!ReferenceEquals(catalog.Resolve(selector).Value, _earlyGrant))
                return RecoveryRequired<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>();
            var appended = AppendEarlyFrame(frames[i]);
            if (!appended.IsSuccess) return appended.MapError<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>();
        }
        EarlyOutcome = EarlyReadback(_v2Prefix!);
        return OperationResult<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>.Ok(EarlyOutcome);
        }
        catch (OperationCanceledException)
        {
            // A durable started prefix cannot authorize a second Train after cancellation.
            return RecoveryRequired<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        { return RecoveryRequired<CodeDocumentationOneLessonCanonicalOutcomeReadbackV2>(); }
    }

    internal OperationResult<bool> GuardProfileTrainV2(CodeDocumentationOneLessonProfileSelection profile)
    {
        var prefix = ReadCapacityPrefixV2();
        if (!prefix.IsSuccess) return prefix.MapError<bool>();
        return prefix.Value!.PrefixFrames == 3 && _earlyGrant?.ProfileSelection?.ProfileSelectionSha256 == profile.ProfileSelectionSha256
            ? OperationResult<bool>.Ok(true) : RecoveryRequired<bool>();
    }

    private OperationResult<bool> AppendEarlyFrame(JsonElement frame)
    {
        var prefix = ReadCapacityPrefixV2();
        if (!prefix.IsSuccess) return prefix.MapError<bool>();
        try
        {
            var line = CodeDocumentationOneLessonV2Wire.CanonicalBytes(frame);
            var next = _v2Prefix!.Concat(line).Append((byte)'\n').ToArray();
            var valid = ValidateEarlyPrefixV2(next, _snapshot.Binding, _sessionId, _capacityV2!, _earlyGrant!, true);
            if (!valid.IsSuccess) return valid.MapError<bool>();
            if (_v2History is null)
                _v2History = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(_historyPath, FileMode.CreateNew,
                    FileAccess.ReadWrite, FileShare.Read, FileOptions.SequentialScan);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_v2History.SafeFileHandle, _historyPath) ||
                !SecureLocalFileBoundary.ReadBoundedStableBytes(_v2History, _capacityV2!.MaximumBytes)!.AsSpan().SequenceEqual(_v2Prefix))
                return RecoveryRequired<bool>();
            _v2History.Position = _v2History.Length;
            _v2History.Write(line); _v2History.WriteByte((byte)'\n'); _v2History.Flush(flushToDisk: true);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_ownerLock.SafeFileHandle, _lockPath) ||
                !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_v2History.SafeFileHandle, _historyPath))
                return RecoveryRequired<bool>();
            var readback = SecureLocalFileBoundary.ReadBoundedStableBytes(_v2History, _capacityV2.MaximumBytes);
            if (readback is null || !readback.AsSpan().SequenceEqual(next)) return RecoveryRequired<bool>();
            valid = ValidateEarlyPrefixV2(readback, _snapshot.Binding, _sessionId, _capacityV2, _earlyGrant!, true);
            if (!valid.IsSuccess) return valid.MapError<bool>();
            _v2Prefix = readback; _capacityV2 = valid.Value;
            _snapshot = _snapshot with { Revision = valid.Value!.PrefixFrames - 1, HistorySha256 = Sha256(readback) };
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        { return RecoveryRequired<bool>(); }
    }

    internal static string EarlyReceiptId(string session, string type, int slot, string binding)
        => Sha256("xiigen-one-lesson-receipt-id-v2\n" + session + "\n" + type + "\n" +
            slot.ToString(CultureInfo.InvariantCulture) + "\n" + binding)[..32];

    internal static string DecisionInputsSha256(string binding, CodeDocumentationPreparedBaselineExternalReference external,
        int caseCount, int repeatCount, string noiseIdentity)
    {
        var count = checked(caseCount * repeatCount).ToString(CultureInfo.InvariantCulture);
        var value = JsonSerializer.SerializeToElement(new { SchemaVersion = 2, BindingSha256 = binding,
            ExternalBaselineRef = new { external.ReceiptId, external.ReceiptSha256 }, CaseCount = caseCount,
            RepeatCount = repeatCount, ExpectedEvaluationCallCountDecimal = count, ObservedEvaluationCallCountDecimal = count,
            NoisePolicyIdentitySha256 = noiseIdentity,
            FinalCertificationTargetCorrectFractionR = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.DescribeFinalCertificationTargetCorrectFractionR(),
            QualityPolicy = "BaselineRequiresNewTrainingPair", Decision = "BaselineSufficient" });
        return Sha256(Utf8.GetBytes("xiigen-s04-decisioninputs-v2\n").Concat(CodeDocumentationOneLessonV2Wire.CanonicalBytes(value)).ToArray());
    }

    private static JsonElement[] EarlyFrames(CodeDocumentationOneLessonSessionBinding binding, string session,
        CodeDocumentationOneLessonGrantV1 grant, CodeDocumentationPreparedBaselineExternalReference external)
    {
        static JsonElement Canonical(object value)
        {
            using var doc = JsonDocument.Parse(CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(value)));
            return doc.RootElement.Clone();
        }
        var hash = ComputeBindingSha256(binding);
        var parent = grant.CheckpointSelection!.Parent;
        var checkpointId = Sha256(Utf8.GetBytes("xiigen-s04-checkpoint-id-v2\n").Concat(
            CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(parent))).ToArray());
        JsonElement Receipt(string type, int slot, object payload) => Canonical(new { SchemaVersion = 2, ReceiptType = type,
            ReceiptId = EarlyReceiptId(session, type, slot, hash), binding.ObjectiveId, SessionId = session,
            binding.TrainingRunId, BindingSha256 = hash, Payload = payload });
        object Ref(JsonElement receipt) => new { ReceiptId = receipt.GetProperty("ReceiptId").GetString(),
            ReceiptSha256 = Sha256(CodeDocumentationOneLessonV2Wire.CanonicalBytes(receipt)) };
        var qd = Receipt("QualityDecision", 2, new { Decision = "BaselineSufficient",
            ExternalBaselineRef = new { external.ReceiptId, external.ReceiptSha256 },
            DecisionInputsSha256 = DecisionInputsSha256(hash, external, grant.DescriptorsV2!.CaseCount,
                grant.DescriptorsV2.RepeatCount, grant.EvaluatorSelection!.Evaluator.NoisePolicyIdentitySha256) });
        var disposition = Receipt("Disposition", 3, new { Disposition = "RetainParent", QualityDecisionRef = Ref(qd),
            SelectedCheckpointSha256 = checkpointId });
        var final = Receipt("FinalReopen", 6, new { SelectedRole = "Parent", CheckpointSha256 = checkpointId,
            parent.ManifestSha256, parent.WeightsSha256,
            WeightsByteLengthDecimal = parent.WeightsByteLength.ToString(CultureInfo.InvariantCulture), PredecessorReceiptRef = Ref(disposition) });
        var state = "TERMINAL_NEGATIVE_PARENT_RETAINED";
        var outcomeId = Sha256("xiigen-one-lesson-outcome-id-v2\n" + session + "\n" + hash + "\n" + state + "\n" +
            final.GetProperty("ReceiptId").GetString())[..32];
        var outcome = Canonical(new { OutcomeId = outcomeId, SchemaVersion = 2, binding.ObjectiveId,
            SessionId = session, binding.TrainingRunId, BindingSha256 = hash, TerminalState = state,
            QualityDecisionReceipt = Ref(qd), DispositionReceipt = Ref(disposition), FinalReopenReceipt = Ref(final) });
        using var bindingDoc = JsonDocument.Parse(BindingBytesV2(binding));
        var effects = new { BaselineDispatched = "0", BaselineCompleted = "0", TeachingDispatched = "0",
            TeachingCompleted = "0", EvaluatorDispatched = "0", EvaluatorCompleted = "0", OptimizerEntries = "0", EffectOutcomeUnknown = false };
        var frames = new JsonElement[7]; var previous = "";
        for (var i = 0; i < frames.Length; i++)
        {
            var frame = new Dictionary<string, object?> { ["Kind"] = i == 0 ? "session" : i < 3 ? "transition" : i < 6 ? "receipt" : "terminal",
                ["FormatVersion"] = 2, ["SessionId"] = session, ["BindingV2"] = bindingDoc.RootElement,
                ["BindingSha256"] = hash, ["Revision"] = i.ToString(CultureInfo.InvariantCulture),
                ["PreviousFrameSha256"] = previous, ["Effects"] = effects };
            if (i is 1 or 2) frame["TransitionEvent"] = i == 1 ? "RunnerStarted" : "ProfileTrainingStarted";
            if (i is >= 3 and <= 5) frame["ProgressReceipt"] = i == 3 ? qd : i == 4 ? disposition : final;
            if (i == 6) frame["TerminalOutcome"] = outcome;
            frames[i] = Canonical(frame); previous = Sha256(CodeDocumentationOneLessonV2Wire.CanonicalBytes(frames[i]));
        }
        return frames;
    }

    private static OperationResult<CodeDocumentationOneLessonContinuationCapacityV2> ValidateEarlyPrefixV2(byte[] bytes,
        CodeDocumentationOneLessonSessionBinding binding, string session, CodeDocumentationOneLessonContinuationCapacityV2 capacity,
        CodeDocumentationOneLessonGrantV1 grant, bool allowIncomplete)
    {
        try
        {
            if (!V2PhysicalLinesWithin(bytes,32768)) return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>(); // Reject private-sized syntax before any legacy UTF8 or JSON parsing.
            if (bytes.LongLength > capacity.MaximumBytes) return CapacityExceeded<CodeDocumentationOneLessonContinuationCapacityV2>();
            if (bytes.Length == 0 || bytes[^1] != (byte)'\n' || bytes.Contains((byte)'\r'))
                return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>();
            var lines = Utf8.GetString(bytes).Split('\n'); var count = lines.Length - 1;
            if (count is < 1 or > 7 || (!allowIncomplete && count != 7) || grant.CheckpointSelection is null ||
                grant.EvaluatorSelection is null || grant.DescriptorsV2 != binding.DescriptorsV2)
                return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>();
            var external = new CodeDocumentationPreparedBaselineExternalReference(new string('a', 32), new string('b', 64));
            if (count > 3)
            {
                using var qdDoc = JsonDocument.Parse(lines[3]);
                var receipt = CodeDocumentationOneLessonProgressReceiptV2.Decode(qdDoc.RootElement.GetProperty("ProgressReceipt"), binding);
                if (!receipt.IsSuccess) return receipt.MapError<CodeDocumentationOneLessonContinuationCapacityV2>();
                var p = receipt.Value!.Payload.GetProperty("ExternalBaselineRef");
                external = new(p.GetProperty("ReceiptId").GetString()!, p.GetProperty("ReceiptSha256").GetString()!);
            }
            var expected = EarlyFrames(binding, session, grant, external);
            for (var i = 0; i < count; i++)
            {
                var raw = Utf8.GetBytes(lines[i]); using var doc = JsonDocument.Parse(raw, new JsonDocumentOptions { MaxDepth = 8 });
                var frame = doc.RootElement;
                var cap = i == 0 ? 2585 : i < 3 ? 2839 : i < 6 ? ReceiptLineCeilingV2(frame) : 4865;
                if (raw.Length + 1 > cap) return CapacityExceeded<CodeDocumentationOneLessonContinuationCapacityV2>();
                if (i is >= 3 and <= 5)
                {
                    var decoded = CodeDocumentationOneLessonProgressReceiptV2.Decode(frame.GetProperty("ProgressReceipt"), binding);
                    if (!decoded.IsSuccess) return decoded.MapError<CodeDocumentationOneLessonContinuationCapacityV2>();
                }
                if (i == 6)
                {
                    var decoded = CodeDocumentationOneLessonTerminalOutcomeV2.Decode(frame.GetProperty("TerminalOutcome"), binding);
                    if (!decoded.IsSuccess) return decoded.MapError<CodeDocumentationOneLessonContinuationCapacityV2>();
                }
                if (!raw.AsSpan().SequenceEqual(CodeDocumentationOneLessonV2Wire.CanonicalBytes(expected[i])))
                    return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>();
            }
            return OperationResult<CodeDocumentationOneLessonContinuationCapacityV2>.Ok(capacity with { PrefixFrames = count, PrefixBytes = bytes.LongLength });
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        { return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>(); }
    }

    private static CodeDocumentationOneLessonCanonicalOutcomeReadbackV2 EarlyReadback(byte[] bytes)
    {
        var line = Utf8.GetString(bytes).Split('\n')[6]; using var doc = JsonDocument.Parse(line);
        var outcome = doc.RootElement.GetProperty("TerminalOutcome").Clone();
        return new(outcome, Sha256(CodeDocumentationOneLessonV2Wire.CanonicalBytes(outcome)), 6);
    }

    /// <summary>Reconstructs the lease from the held raw prefix; never switches journal handles.</summary>
    internal OperationResult<CodeDocumentationOneLessonContinuationCapacityV2> ReadCapacityPrefixV2()
    {
        if (_fullMaterialLease is not null && (_fullMaterialPoisoned || _capacityV2?.PrefixFrames is 1 or 2) && !ReferenceEquals(_fullMaterialCheckOwner.Value,this)) return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>(); // A persisted unknown invocation never grants retry/rechoice access.
        if (_studentResponseInFlight != 0 && !ReferenceEquals(_studentCheckOwner.Value,this)) return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>(); // During response dispatch only private finite checks may inspect the prefix; competing ordinary entry points gain no transient effect allowance.
        if ((_studentPoisoned || _studentLease is not null && _capacityV2?.PrefixFrames == 3) && !ReferenceEquals(_studentCheckOwner.Value,this)) return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>(); // Unknown Started is readable only by finite checks of the same active owner, never a new effect caller.
        if (_disposed || _capacityV2 is null || _v2Prefix is null)
            return CapacityInvalid<CodeDocumentationOneLessonContinuationCapacityV2>();
        try
        {
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_ownerLock.SafeFileHandle, _lockPath) ||
                !ReadStream(_ownerLock, 1024).AsSpan().SequenceEqual(Utf8.GetBytes("s04-one-lesson-session-v2\n" + _sessionId + "\n")))
                return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>();
            if (_v2History is null)
                return File.Exists(_historyPath) || Directory.Exists(_historyPath)
                    ? RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>()
                    : OperationResult<CodeDocumentationOneLessonContinuationCapacityV2>.Ok(_capacityV2);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_v2History.SafeFileHandle, _historyPath))
                return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>();
            if (_v2History.Length > _capacityV2.MaximumBytes) return CapacityExceeded<CodeDocumentationOneLessonContinuationCapacityV2>(); // Observe the admitted held-file byte limit separately from stability/read refusals.
            var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(_v2History, _capacityV2.MaximumBytes);
            if (bytes is null)
                return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>(); // Empty, short and unstable history deny custody; existing private pending/unknown access guards remain above.
            if (!bytes.AsSpan().SequenceEqual(_v2Prefix))
                return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>();
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_ownerLock.SafeFileHandle, _lockPath) ||
                !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_v2History.SafeFileHandle, _historyPath))
                return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>();
            return _fullMaterialLease is not null ? ValidateFullMaterialPrefix(bytes, _snapshot.Binding, _sessionId, _capacityV2, true) : _earlyGrant is null ? ValidatePrefixV2(bytes, _snapshot.Binding, _sessionId, _capacityV2) : // Typed full scope stays distinct from generic/legacy grammar.
                ValidateOrdinaryPrefixV2(bytes, _snapshot.Binding, _sessionId, _capacityV2, _earlyGrant, allowIncomplete: true, studentLease: _studentLease); // Keep response parsing conditional on the original held private intent.
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        { return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>(); }
    }

    /// <summary>
    /// Consumes committed engineering evidence under this still-held V2 lease.
    /// A successful read grants no append, calibration, training or publication.
    /// </summary>
    internal async Task<OperationResult<CodeDocumentationPreparedBaselineReadback>> ConsumePreparedBaselineAsync(
        CodeDocumentationOneLessonGrantCatalog catalog,
        CodeDocumentationOneLessonSelectorV1 selector,
        CodeDocumentationRepositoryOwnedModelCheckpointStartRequest acceptedParentRequest,
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext qualityContext,
        CancellationToken cancellationToken = default)
    {
        // Custody refusal precedes cancellation and every original-baseline access.
        var prefix = ReadCapacityPrefixV2();
        if (!prefix.IsSuccess || prefix.Value is null)
            return prefix.MapError<CodeDocumentationPreparedBaselineReadback>();
        cancellationToken.ThrowIfCancellationRequested();
        if (catalog is null || selector is null || acceptedParentRequest is null || qualityContext is null ||
            acceptedParentRequest.ModelConfig is null || acceptedParentRequest.RuntimeCompatibility is null)
            return Invalid<CodeDocumentationPreparedBaselineReadback>();

        var binding = _snapshot.Binding with { DescriptorsV2 = _snapshot.Binding.DescriptorsV2! with { } };
        var bindingBytes = BindingBytesV2(binding);
        var bindingHash = ComputeBindingSha256(binding);
        var ownership = CheckPreparedBaselineOwnership(catalog, selector, acceptedParentRequest,
            binding, bindingBytes, bindingHash);
        if (!ownership.IsSuccess) return ownership.MapError<CodeDocumentationPreparedBaselineReadback>();

        CodeDocumentationRepositoryOwnedModelCheckpointStartRequest capturedStart;
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext capturedContext;
        byte[] tokenizerBytes;
        string[] evidence;
        string[]? modules;
        try
        {
            tokenizerBytes = PreparedTokenizerBytes(qualityContext.Tokenizer);
            // Save leaves its stream at the end; this new read-only stream starts at zero.
            using var input = new MemoryStream(tokenizerBytes, writable: false);
            var tokenizer = BpeTokenizer.Load(input);
            if (!PreparedTokenizerBytes(tokenizer).AsSpan().SequenceEqual(tokenizerBytes))
                return Invalid<CodeDocumentationPreparedBaselineReadback>();
            evidence = qualityContext.EvidenceRefs.ToArray();
            modules = acceptedParentRequest.LoraConfig?.TargetModules.ToArray();
            capturedStart = acceptedParentRequest with
            {
                ModelConfig = acceptedParentRequest.ModelConfig with { },
                RuntimeCompatibility = acceptedParentRequest.RuntimeCompatibility with { },
                LoraConfig = acceptedParentRequest.LoraConfig is null ? null :
                    acceptedParentRequest.LoraConfig with { TargetModules = modules!.ToArray() },
                AdapterSeedAuthority = acceptedParentRequest.AdapterSeedAuthority is null ? null :
                    acceptedParentRequest.AdapterSeedAuthority with { },
            };
            capturedContext = qualityContext with
            {
                // Corpus and evaluator own immutable full collections/audit salt.
                CorpusManifest = qualityContext.CorpusManifest with { },
                Tokenizer = tokenizer,
                GenerationSettings = qualityContext.GenerationSettings with { },
                NoisePolicyConfig = new(qualityContext.NoisePolicyConfig.RepeatCount,
                    qualityContext.NoisePolicyConfig.StandardDeviationMultiplier),
                EvidenceRefs = Array.AsReadOnly(evidence.ToArray()),
            };
        }
        catch (Exception exception) when (exception is not OperationCanceledException &&
            SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        { return Invalid<CodeDocumentationPreparedBaselineReadback>(); }

        // Await only the real original-owner operation. No scheduling/test seam is added.
        var opened = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
            .ReadPreparedBaselinePrerequisiteAsync(catalog, selector, capturedStart, capturedContext,
                cancellationToken).ConfigureAwait(false);

        // Every returned lower result, including failure, passes the same return boundary.
        prefix = ReadCapacityPrefixV2();
        if (!prefix.IsSuccess || prefix.Value is null)
            return prefix.MapError<CodeDocumentationPreparedBaselineReadback>();
        ownership = CheckPreparedBaselineOwnership(catalog, selector, acceptedParentRequest,
            binding, bindingBytes, bindingHash);
        if (!ownership.IsSuccess) return ownership.MapError<CodeDocumentationPreparedBaselineReadback>();
        try
        {
            if (!PreparedTokenizerBytes(qualityContext.Tokenizer).AsSpan().SequenceEqual(tokenizerBytes) ||
                !qualityContext.EvidenceRefs.SequenceEqual(evidence, StringComparer.Ordinal) ||
                (modules is not null && (acceptedParentRequest.LoraConfig?.TargetModules is not { } currentModules ||
                    !currentModules.SequenceEqual(modules, StringComparer.Ordinal))))
                return Invalid<CodeDocumentationPreparedBaselineReadback>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException &&
            SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        { return Invalid<CodeDocumentationPreparedBaselineReadback>(); }
        if (opened.IsSuccess) cancellationToken.ThrowIfCancellationRequested();
        return opened;
    }

    private OperationResult<bool> CheckPreparedBaselineOwnership(
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector,
        CodeDocumentationRepositoryOwnedModelCheckpointStartRequest start,
        CodeDocumentationOneLessonSessionBinding expected, byte[] expectedBytes, string expectedHash)
    {
        var resolved = catalog.Resolve(selector);
        if (!resolved.IsSuccess || resolved.Value is null) return resolved.MapError<bool>();
        var grant = resolved.Value;
        var current = _snapshot.Binding;
        var catalogBinding = new CodeDocumentationOneLessonSessionBinding(grant.ObjectiveId, grant.GrantId,
            grant.GrantVersion, grant.TrainingRunId, grant.CandidateSha256, grant.ParentManifestSha256,
            grant.ParentWeightsSha256, grant.ConfigurationSha256, grant.ExamConditionSha256)
            { FormatVersion = grant.FormatVersion, DescriptorsV2 = grant.DescriptorsV2 };
        if (current.FormatVersion != 2 || catalogBinding != expected || current != expected ||
            !BindingBytesV2(current).AsSpan().SequenceEqual(expectedBytes) ||
            ComputeBindingSha256(current) != expectedHash)
            return Failure<bool>(ObjectiveConflictErrorCode, "The held V2 session and current catalog disagree.");
        var root = RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(start.RepositoryRoot);
        if (!root.IsSuccess || root.Value is null || !SamePath(_historyPath,
            Path.Combine(root.Value.ModelArtifactRoot, SessionsDirectoryName, _sessionId, HistoryFileName)))
            return Invalid<bool>();
        return OperationResult<bool>.Ok(true);
    }

    private static byte[] PreparedTokenizerBytes(BpeTokenizer tokenizer)
    {
        using var output = new MemoryStream();
        tokenizer.Save(output);
        return output.ToArray();
    }

    /// <summary>
    /// Logical prefix plus remaining worst-case reservation under this owner's
    /// lock. This does not allocate disk space or permit an effect/append.
    /// </summary>
    internal OperationResult<CodeDocumentationOneLessonContinuationCapacityV2> ValidateContinuationCapacityV2(
        int prefixFrames, long prefixBytes, int remainingFrames, long remainingWorstCaseBytes)
    {
        var current = ReadCapacityPrefixV2();
        if (!current.IsSuccess || current.Value is null) return current;
        if (prefixFrames < current.Value.PrefixFrames || prefixBytes < current.Value.PrefixBytes ||
            remainingFrames < 0 || remainingWorstCaseBytes < 0)
            return CapacityInvalid<CodeDocumentationOneLessonContinuationCapacityV2>();
        try
        {
            if (checked(prefixFrames + remainingFrames) > current.Value.MaximumFrames ||
                checked(prefixBytes + remainingWorstCaseBytes) > current.Value.MaximumBytes)
                return CapacityExceeded<CodeDocumentationOneLessonContinuationCapacityV2>();
            return current;
        }
        catch (OverflowException) { return CapacityInvalid<CodeDocumentationOneLessonContinuationCapacityV2>(); }
    }

    private static byte[] BindingBytesV2(CodeDocumentationOneLessonSessionBinding binding)
    {
        using var document = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(new
        {
            binding.ObjectiveId, binding.GrantId, binding.GrantVersion, binding.TrainingRunId,
            binding.CandidateSha256, binding.ParentManifestSha256, binding.ParentWeightsSha256,
            binding.ConfigurationSha256, binding.ExamConditionSha256, binding.DescriptorsV2,
        }));
        return CodeDocumentationOneLessonV2Wire.CanonicalBytes(document.RootElement);
    }

    private static OperationResult<CodeDocumentationOneLessonContinuationCapacityV2> ValidatePrefixV2(
        byte[] bytes, CodeDocumentationOneLessonSessionBinding binding, string sessionId,
        CodeDocumentationOneLessonContinuationCapacityV2 capacity)
    {
        if (!V2PhysicalLinesWithin(bytes,32768)) return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>(); // Preserve the ordinary/generic global preparse fence independently of untrusted kind or Genesis markers.
        if (bytes.LongLength > capacity.MaximumBytes) return CapacityExceeded<CodeDocumentationOneLessonContinuationCapacityV2>();
        if (bytes.Length == 0 || bytes[^1] != (byte)'\n' || bytes.Contains((byte)'\r'))
            return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>();
        var lines = Utf8.GetString(bytes).Split('\n');
        var frames = lines.Length - 1;
        if (frames > capacity.MaximumFrames) return CapacityExceeded<CodeDocumentationOneLessonContinuationCapacityV2>();
        var expectedBinding = BindingBytesV2(binding);
        var bindingHash = ComputeBindingSha256(binding);
        var previousHash = "";
        var effectsBefore = new CodeDocumentationOneLessonEffectAccounting();
        var unsupportedOrAmbiguous = false;
        for (var index = 0; index < frames; index++)
        {
            var line = Utf8.GetBytes(lines[index]);
            using var document = JsonDocument.Parse(line, new JsonDocumentOptions { MaxDepth = 8 });
            var frame = document.RootElement;
            if (frame.ValueKind != JsonValueKind.Object || !frame.TryGetProperty("Kind", out var kindValue) ||
                kindValue.ValueKind != JsonValueKind.String)
                return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>();
            var kind = kindValue.GetString();
            var ceiling = kind switch
            {
                "session" => 2_585, "transition" => 2_839, "TrialSlotStarted" => 3_176,
                "TrialSlotCompleted" => 3_372, "terminal" => 4_865,
                "receipt" => ReceiptLineCeilingV2(frame), _ => 0,
            };
            if (ceiling == 0) return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>();
            if (line.Length + 1 > ceiling) return CapacityExceeded<CodeDocumentationOneLessonContinuationCapacityV2>();
            var common = new[] { "Kind", "FormatVersion", "SessionId", "BindingV2", "BindingSha256", "Revision", "PreviousFrameSha256", "Effects" };
            var payload = kind switch
            { "transition" => "TransitionEvent", "TrialSlotStarted" => "TrialSlotStarted", "TrialSlotCompleted" => "TrialSlotCompleted",
              "receipt" => "ProgressReceipt", "terminal" => "TerminalOutcome", _ => null };
            if (!CodeDocumentationOneLessonV2Wire.ExactFields(frame, payload is null ? common : common.Append(payload).ToArray()) ||
                frame.GetProperty("FormatVersion").GetRawText() != "2" ||
                frame.GetProperty("SessionId").ValueKind != JsonValueKind.String || frame.GetProperty("SessionId").GetString() != sessionId ||
                frame.GetProperty("BindingSha256").ValueKind != JsonValueKind.String || frame.GetProperty("BindingSha256").GetString() != bindingHash ||
                !CanonicalDecimal(frame.GetProperty("Revision"), out var revision) || revision != index ||
                frame.GetProperty("PreviousFrameSha256").ValueKind != JsonValueKind.String ||
                frame.GetProperty("PreviousFrameSha256").GetString() != previousHash ||
                !CodeDocumentationOneLessonV2Wire.CanonicalBytes(frame.GetProperty("BindingV2")).AsSpan().SequenceEqual(expectedBinding) ||
                !line.AsSpan().SequenceEqual(CodeDocumentationOneLessonV2Wire.CanonicalBytes(frame)) ||
                !ReadEffectsV2(frame.GetProperty("Effects"), out var effects) || !Monotonic(effectsBefore, effects) || effects.OptimizerEntries > 1 ||
                (index == 0 ? kind != "session" || effects != effectsBefore : kind == "session"))
                return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>();
            if (kind == "transition")
            {
                var transition = frame.GetProperty("TransitionEvent");
                if (transition.ValueKind != JsonValueKind.String || transition.GetString() is not
                    ("RunnerStarted" or "ProfileTrainingStarted" or "TeacherAuthoringStarted" or "TrainerUpdateStarted" or "EnterEvaluationOnly"))
                    return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>();
                unsupportedOrAmbiguous = true;
            }
            else if (kind is "TrialSlotStarted" or "TrialSlotCompleted")
            {
                var decoded = CodeDocumentationOneLessonTrialSlotV2.Decode( // Check the complete nested raw slot cap after the unchanged common frame guards.
                    frame.GetProperty(kind!), kind == "TrialSlotStarted", binding); // Select only the exact trusted envelope kind and expected binding syntax.
                if (!decoded.IsSuccess) // Preserve either nested capacity or ordinary malformed-slot recovery without a partial value.
                    return decoded.MapError<CodeDocumentationOneLessonContinuationCapacityV2>(); // Carry the pure decoder's stable refusal through the owner path.
                unsupportedOrAmbiguous = true; // Closed slot syntax grants no chain, assessment or replay authority and permits no effects.
            }
            else if (kind == "receipt")
            {
                var decoded = CodeDocumentationOneLessonProgressReceiptV2.Decode(
                    frame.GetProperty("ProgressReceipt"), binding);
                if (!decoded.IsSuccess)
                    return decoded.MapError<CodeDocumentationOneLessonContinuationCapacityV2>();
                // Shape and object hash describe untrusted bytes. They do not
                // resolve refs, establish a fixed slot, or permit receipt replay.
                unsupportedOrAmbiguous = true;
            }
            else if (kind == "terminal")
            {
                var decoded = CodeDocumentationOneLessonTerminalOutcomeV2.Decode( // Validate the complete untrusted nested terminal bytes after common frame guards.
                    frame.GetProperty("TerminalOutcome"), binding); // Require this frame's expected binding without granting chain authority.
                if (!decoded.IsSuccess) // Preserve nested capacity or recovery refusal with no partially decoded value.
                    return decoded.MapError<CodeDocumentationOneLessonContinuationCapacityV2>(); // Carry the decoder's stable error code through the actual owner path.
                // Closed syntax and owned bytes describe an untrusted outcome.
                // Reference resolution, terminal chain authority and replay
                // remain unverified; structural success permits no effects.
                unsupportedOrAmbiguous = true; // Keep even a structurally valid terminal prefix unavailable for replay.
            }
            if (effects != new CodeDocumentationOneLessonEffectAccounting()) unsupportedOrAmbiguous = true;
            effectsBefore = effects;
            previousHash = Sha256(line);
        }
        if (unsupportedOrAmbiguous) return RecoveryRequired<CodeDocumentationOneLessonContinuationCapacityV2>();
        return OperationResult<CodeDocumentationOneLessonContinuationCapacityV2>.Ok(capacity with { PrefixFrames = frames, PrefixBytes = bytes.LongLength });
    }

    private static int ReceiptLineCeilingV2(JsonElement frame)
    {
        if (!frame.TryGetProperty("ProgressReceipt", out var receipt) || receipt.ValueKind != JsonValueKind.Object ||
            !receipt.TryGetProperty("ReceiptType", out var type) || type.ValueKind != JsonValueKind.String) return 0;
        return type.GetString() switch
        {
            "SealedPair" => 3_980, "RunnerCompletion" => 4_171, "Baseline" => 4_050,
            "VerifiedAppliedUpdate" => 4_046, "TrialEvaluation" => 4_183, "QualityDecision" => 4_199,
            "Disposition" => 3_903, "PublicationStarted" => 4_025, "PublicationReceipt" => 4_171,
            "FinalReopen" => 4_108, _ => 0,
        };
    }

    private static bool CanonicalDecimal(JsonElement value, out long result)
    {
        result = 0;
        if (value.ValueKind != JsonValueKind.String) return false;
        var text = value.GetString();
        return text is { Length: >= 1 and <= 19 } && (text.Length == 1 || text[0] != '0') &&
            long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out result);
    }

    private static bool ReadEffectsV2(JsonElement value, out CodeDocumentationOneLessonEffectAccounting effects)
    {
        effects = new();
        var names = new[] { "BaselineDispatched", "BaselineCompleted", "TeachingDispatched", "TeachingCompleted", "EvaluatorDispatched", "EvaluatorCompleted", "OptimizerEntries" };
        if (!CodeDocumentationOneLessonV2Wire.ExactFields(value, names.Append("EffectOutcomeUnknown").ToArray()) ||
            value.GetProperty("EffectOutcomeUnknown").ValueKind is not (JsonValueKind.True or JsonValueKind.False)) return false;
        var counts = new long[7];
        for (var i = 0; i < names.Length; i++) if (!CanonicalDecimal(value.GetProperty(names[i]), out counts[i])) return false;
        effects = new(counts[0], counts[1], counts[2], counts[3], counts[4], counts[5], counts[6], value.GetProperty("EffectOutcomeUnknown").GetBoolean());
        return ValidEffects(effects);
    }

    internal static bool ValidSlotShapeV2(JsonElement value, bool started, CodeDocumentationOneLessonDescriptorsV2 descriptor, int slots) // Share the unchanged closed slot grammar with the pure decoder, without introducing slot authority.
    {
        if (!started)
        {
            if (!CodeDocumentationOneLessonV2Wire.ExactFields(value, "SlotId", "Observation") ||
                value.GetProperty("SlotId").ValueKind != JsonValueKind.String || !Sha256Hex(value.GetProperty("SlotId").GetString())) return false;
            var observation = value.GetProperty("Observation");
            if (!CodeDocumentationOneLessonV2Wire.ExactFields(observation, "CaseId", "Passed", "ReasonCode", "StudentAnswerSha256",
                    "StudentAnswerCharCount", "EvaluationRequestIdSha256", "ModelSerializationAfterSha256")) return false;
            return observation.GetProperty("CaseId").ValueKind == JsonValueKind.String &&
                observation.GetProperty("CaseId").GetString() is { Length: 71 } id && id.StartsWith("sha256:", StringComparison.Ordinal) && Sha256Hex(id[7..]) &&
                observation.GetProperty("Passed").ValueKind is JsonValueKind.True or JsonValueKind.False &&
                observation.GetProperty("ReasonCode").ValueKind == JsonValueKind.String &&
                observation.GetProperty("ReasonCode").GetString() is
                    (CodeDocumentationSavedModelAssessmentService.CorrectReasonCode or
                     CodeDocumentationSavedModelAssessmentService.MissingEssentialBehaviorReasonCode or
                     CodeDocumentationSavedModelAssessmentService.InventedBehaviorReasonCode or
                     CodeDocumentationSavedModelAssessmentService.WrongPurposeOrResultReasonCode or
                     CodeDocumentationSavedModelAssessmentService.UnclearOrUnverifiableReasonCode) &&
                BoundedIntegerV2(observation.GetProperty("StudentAnswerCharCount"), 0, 65_536) &&
                new[] { "StudentAnswerSha256", "EvaluationRequestIdSha256", "ModelSerializationAfterSha256" }
                    .All(name => observation.GetProperty(name).ValueKind == JsonValueKind.String && Sha256Hex(observation.GetProperty(name).GetString()));
        }
        return CodeDocumentationOneLessonV2Wire.ExactFields(value, "SlotId", "PlanSha256", "RepeatOrdinal", "CaseOrdinal", "SlotOrdinal",
                "ModelSerializationBeforeSha256", "ReservedEvaluatorAttemptOrdinal") &&
            new[] { "SlotId", "PlanSha256", "ModelSerializationBeforeSha256" }.All(name => value.GetProperty(name).ValueKind == JsonValueKind.String && Sha256Hex(value.GetProperty(name).GetString())) &&
            BoundedIntegerV2(value.GetProperty("RepeatOrdinal"), 1, descriptor.RepeatCount) &&
            BoundedIntegerV2(value.GetProperty("CaseOrdinal"), 1, descriptor.CaseCount) &&
            BoundedIntegerV2(value.GetProperty("SlotOrdinal"), 1, slots) &&
            BoundedIntegerV2(value.GetProperty("ReservedEvaluatorAttemptOrdinal"), 1, 10_000);
    }

    private static bool BoundedIntegerV2(JsonElement value, int minimum, int maximum)
        => value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) && number >= minimum && number <= maximum &&
           value.GetRawText() == number.ToString(CultureInfo.InvariantCulture);

    private static OperationResult<T> CapacityInvalid<T>()
        => Failure<T>(CapacityInvalidErrorCode, "The V2 continuation binding, count, arithmetic or teacher budget is invalid.");
    private static OperationResult<T> CapacityExceeded<T>()
        => Failure<T>(CapacityExceededErrorCode, "The V2 full continuation or LF-inclusive prefix exceeds its finite capacity.");

    private static OperationResult<HistoryReadback> ReadHistory(string path, string sessionId)
    {
        try
        {
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) ||
                !File.Exists(path) ||
                !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path))
            {
                return RecoveryRequired<HistoryReadback>();
            }

            using var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                FileOptions.SequentialScan);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path))
                return RecoveryRequired<HistoryReadback>();
            var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, MaximumHistoryBytes);
            if (bytes is null || bytes.Length == 0 || bytes[^1] != (byte)'\n' || bytes.Contains((byte)'\r'))
                return RecoveryRequired<HistoryReadback>();

            var text = Utf8.GetString(bytes);
            var lines = text.Split('\n', StringSplitOptions.None);
            if (lines.Length is < 2 or > MaximumHistoryFrames + 1 || lines[^1].Length != 0)
                return RecoveryRequired<HistoryReadback>();

            CodeDocumentationOneLessonSessionBinding? binding = null;
            CodeDocumentationOneLessonEffectAccounting? priorEffects = null;
            string priorHash = "";
            SessionFrame? last = null;
            for (var index = 0; index < lines.Length - 1; index++)
            {
                if (lines[index].Length == 0)
                    return RecoveryRequired<HistoryReadback>();
                var lineBytes = Utf8.GetBytes(lines[index]);
                if (!ValidTerminalWireShape(lineBytes))
                    return RecoveryRequired<HistoryReadback>();
                var frame = JsonSerializer.Deserialize<SessionFrame>(lineBytes, Json);
                if (frame is null ||
                    frame.FormatVersion != CurrentFormatVersion ||
                    !StringComparer.Ordinal.Equals(frame.SessionId, sessionId) ||
                    frame.Revision != index ||
                    !ValidBinding(frame.Binding) ||
                    !ValidEffects(frame.Effects) ||
                    !StringComparer.Ordinal.Equals(frame.PreviousFrameSha256, priorHash) ||
                    (index == 0 && frame.Kind != "session") ||
                    (index > 0 && frame.Kind is not "transition" and not "terminal") ||
                    (frame.Kind == "terminal" && frame.TerminalOutcome is null) ||
                    (frame.Kind != "terminal" && frame.TerminalOutcome is not null) ||
                    (frame.TerminalOutcome is not null &&
                     (index != lines.Length - 2 ||
                      HasUnresolvedEffects(frame.Effects) ||
                      !ValidCanonicalOutcome(frame.TerminalOutcome, frame.Binding, sessionId))))
                {
                    return RecoveryRequired<HistoryReadback>();
                }

                if (index == 0)
                {
                    if (frame.Effects != new CodeDocumentationOneLessonEffectAccounting())
                        return RecoveryRequired<HistoryReadback>();
                    binding = frame.Binding;
                }
                else if (frame.Binding != binding ||
                         priorEffects is null ||
                         !Monotonic(priorEffects, frame.Effects) ||
                         frame.Effects.OptimizerEntries > 1 ||
                         (frame.TerminalOutcome is not null && frame.Effects != priorEffects))
                {
                    return RecoveryRequired<HistoryReadback>();
                }

                priorHash = Sha256(lineBytes);
                priorEffects = frame.Effects;
                last = frame;
            }

            if (binding is null || last is null)
                return RecoveryRequired<HistoryReadback>();

            return OperationResult<HistoryReadback>.Ok(new(
                binding,
                last.Revision,
                last.Effects,
                priorHash,
                last.TerminalOutcome,
                last.TerminalOutcome is null ? null : priorHash,
                bytes));
        }
        catch (JsonException)
        {
            return RecoveryRequired<HistoryReadback>();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<HistoryReadback>();
        }
    }

    private static OperationResult<HistoryReadback> AppendFrame(
        string path,
        byte[] frameBytes,
        byte[] expectedPreviousBytes)
    {
        try
        {
            using (var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
                       path,
                       FileMode.Open,
                       FileAccess.ReadWrite,
                       FileShare.None,
                       FileOptions.WriteThrough))
            {
                if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path))
                    return RecoveryRequired<HistoryReadback>();
                var current = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, MaximumHistoryBytes);
                if (current is null || !current.AsSpan().SequenceEqual(expectedPreviousBytes))
                    return RecoveryRequired<HistoryReadback>();
                var frameWithNewline = frameBytes.Concat(new byte[] { (byte)'\n' }).ToArray();
                if (current.LongLength + frameWithNewline.LongLength > MaximumHistoryBytes)
                    return RecoveryRequired<HistoryReadback>();
                stream.Position = stream.Length;
                stream.Write(frameWithNewline);
                stream.Flush(flushToDisk: true);
                var readback = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, MaximumHistoryBytes);
                if (readback is null || !readback.AsSpan().SequenceEqual(current.Concat(frameWithNewline).ToArray()))
                    return RecoveryRequired<HistoryReadback>();
            }

            return ReadHistory(path, JsonSerializer.Deserialize<SessionFrame>(frameBytes, Json)!.SessionId);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<HistoryReadback>();
        }
    }

    private static bool PublishNewHistory(string historyPath, byte[] frameBytes)
    {
        var stagePath = historyPath + ".stage";
        if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(stagePath) ||
            File.Exists(historyPath) || File.Exists(stagePath) || Directory.Exists(historyPath))
        {
            return false;
        }

        using (var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
                   stagePath,
                   FileMode.CreateNew,
                   FileAccess.ReadWrite,
                   FileShare.None,
                   FileOptions.WriteThrough))
        {
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, stagePath))
                return false;
            stream.Write(frameBytes);
            stream.WriteByte((byte)'\n');
            stream.Flush(flushToDisk: true);
        }

        if (!SecureLocalFileBoundary.PublishAtomicDurable(stagePath, historyPath))
            return false;
        var readback = ReadHistory(historyPath, JsonSerializer.Deserialize<SessionFrame>(frameBytes, Json)!.SessionId);
        return readback.IsSuccess && readback.Value is { Revision: 0 };
    }

    private static void EnsurePrivateDirectory(string path)
    {
        if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path))
            throw new IOException("A repository session path contains a reparse point.");
        if (!Directory.Exists(path))
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(path);
        if (!Directory.Exists(path) ||
            !SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(path))
        {
            throw new IOException("The repository session directory is not privately owned.");
        }
    }

    private static FileStream OpenOwnerLock(string path)
    {
        try
        {
            return SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
                path,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None,
                FileOptions.WriteThrough);
        }
        catch (IOException exception) when (IsMissingFile(exception))
        {
            try
            {
                return SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
                    path,
                    FileMode.CreateNew,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    FileOptions.WriteThrough);
            }
            catch (IOException) when (File.Exists(path))
            {
                // Another opener created the canonical lock first. Reopen it;
                // FileShare.None will distinguish a live owner from a free lock.
                return SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
                    path,
                    FileMode.Open,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    FileOptions.WriteThrough);
            }
        }
    }

    private static bool IsMissingFile(IOException exception)
        => exception is FileNotFoundException ||
           (exception.HResult & 0xffff) == 2;

    private static bool IsLockContention(IOException exception)
    {
        var nativeCode = exception.HResult & 0xffff;
        return OperatingSystem.IsWindows()
            ? nativeCode is 32 or 33
            : nativeCode is 11 or 13;
    }

    private static byte[] ReadStream(FileStream stream, long maximumBytes)
    {
        var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, maximumBytes);
        if (bytes is null)
            throw new IOException("The owner lock is incomplete or exceeds its bound.");
        return bytes;
    }

    private static byte[] FrameBytes(SessionFrame frame)
        => JsonSerializer.SerializeToUtf8Bytes(frame, Json);

    private static bool ValidRoot(RepositoryOwnedModelArtifactRoot? root)
    {
        if (root is null || string.IsNullOrWhiteSpace(root.RepositoryRoot) ||
            string.IsNullOrWhiteSpace(root.ModelArtifactRoot) ||
            !SecureLocalFileBoundary.IsSupportedPlatform())
        {
            return false;
        }

        var resolved = RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(root.RepositoryRoot);
        return resolved.IsSuccess && resolved.Value is not null &&
               SamePath(resolved.Value.ModelArtifactRoot, root.ModelArtifactRoot);
    }

    private static bool ValidBinding(CodeDocumentationOneLessonSessionBinding? binding)
        => binding is not null &&
           ValidText(binding.ObjectiveId, MaximumObjectiveIdBytes) &&
           ValidText(binding.GrantId, 128) &&
           ValidText(binding.GrantVersion, 64) &&
           RepositoryOwnedTrainingArtifactLayout.IsValidModelTrainingAttemptRunId(binding.TrainingRunId) &&
           Sha256Hex(binding.CandidateSha256) &&
           Sha256Hex(binding.ParentManifestSha256) &&
           Sha256Hex(binding.ParentWeightsSha256) &&
           Sha256Hex(binding.ConfigurationSha256) &&
           Sha256Hex(binding.ExamConditionSha256);

    // Pure inherited syntax/count checks for a decoder caller. This does not
    // open an owner, validate selected artifacts, or issue session authority.
    internal static bool ValidProgressReceiptBindingV2(CodeDocumentationOneLessonSessionBinding? binding)
        => ValidBinding(binding) && binding!.FormatVersion == 2 &&
           binding.ObjectiveId.Length <= 128 && binding.GrantId.Length <= 128 &&
           binding.GrantVersion.Length <= 64 && binding.TrainingRunId.Length <= 128 &&
           new[] { binding.ObjectiveId, binding.GrantId, binding.GrantVersion }.All(x => x == x.Trim()) &&
           CodeDocumentationOneLessonV2Wire.ValidDescriptors(binding.DescriptorsV2) &&
           binding.DescriptorsV2 is { CaseCount: >= 1 and <= 200, RepeatCount: >= 2 and <= 5000 } d &&
           (long)d.CaseCount * d.RepeatCount <= 5000;

    private static bool ValidVerifiedTerminalReceiptRefs(
        CodeDocumentationOneLessonVerifiedTerminalReceiptRefsV1? receipts)
        => receipts is not null &&
           ValidText(receipts.ObjectiveId, MaximumObjectiveIdBytes) &&
           ValidText(receipts.SessionId, 128) &&
           RepositoryOwnedTrainingArtifactLayout.IsValidModelTrainingAttemptRunId(receipts.TrainingRunId) &&
           Sha256Hex(receipts.BindingSha256) &&
           ValidReceiptRef(receipts.TrialEvaluationReceipt) &&
           ValidReceiptRef(receipts.QualityDecisionReceipt) &&
           ValidReceiptRef(receipts.DispositionReceipt) &&
           (receipts.PublicationReceipt is null || ValidReceiptRef(receipts.PublicationReceipt)) &&
           ValidReceiptRef(receipts.FinalReopenReceipt);

    private static bool ReceiptReferencesMatchBinding(
        CodeDocumentationOneLessonVerifiedTerminalReceiptRefsV1 receipts,
        CodeDocumentationOneLessonSessionBinding binding,
        string sessionId)
        => StringComparer.Ordinal.Equals(receipts.ObjectiveId, binding.ObjectiveId) &&
           StringComparer.Ordinal.Equals(receipts.SessionId, sessionId) &&
           StringComparer.Ordinal.Equals(receipts.TrainingRunId, binding.TrainingRunId) &&
           StringComparer.Ordinal.Equals(receipts.BindingSha256, ComputeBindingSha256(binding));

    private static bool ValidCanonicalOutcome(
        CodeDocumentationOneLessonCanonicalOutcomeV1? outcome,
        CodeDocumentationOneLessonSessionBinding binding,
        string sessionId)
    {
        if (outcome is null ||
            outcome.SchemaVersion != 1 ||
            !Guid.TryParseExact(outcome.OutcomeId, "N", out var outcomeGuid) ||
            !StringComparer.Ordinal.Equals(outcome.OutcomeId, outcomeGuid.ToString("N")) ||
            !StringComparer.Ordinal.Equals(outcome.ObjectiveId, binding.ObjectiveId) ||
            !StringComparer.Ordinal.Equals(outcome.SessionId, sessionId) ||
            !StringComparer.Ordinal.Equals(outcome.TrainingRunId, binding.TrainingRunId) ||
            !StringComparer.Ordinal.Equals(outcome.BindingSha256, ComputeBindingSha256(binding)) ||
            !ValidReceiptRef(outcome.TrialEvaluationReceipt) ||
            !ValidReceiptRef(outcome.QualityDecisionReceipt) ||
            !ValidReceiptRef(outcome.DispositionReceipt) ||
            !ValidReceiptRef(outcome.FinalReopenReceipt))
        {
            return false;
        }

        return outcome.TerminalState switch
        {
            CodeDocumentationOneLessonTerminalStateV1.TERMINAL_NEGATIVE_PARENT_RETAINED
                => outcome.PublicationReceipt is null,
            CodeDocumentationOneLessonTerminalStateV1.TERMINAL_PROMOTED
                => ValidReceiptRef(outcome.PublicationReceipt),
            _ => false,
        };
    }

    private static bool ValidReceiptRef<TReceipt>(
        CodeDocumentationOneLessonTypedReceiptRefV1<TReceipt>? receipt)
        => receipt is not null &&
           ValidText(receipt.ReceiptId, 256) &&
           Sha256Hex(receipt.ReceiptSha256);

    private static bool SameReceiptReferences(
        CodeDocumentationOneLessonCanonicalOutcomeV1 outcome,
        CodeDocumentationOneLessonVerifiedTerminalReceiptRefsV1 receipts)
        => outcome.TrialEvaluationReceipt == receipts.TrialEvaluationReceipt &&
           outcome.QualityDecisionReceipt == receipts.QualityDecisionReceipt &&
           outcome.DispositionReceipt == receipts.DispositionReceipt &&
           outcome.PublicationReceipt == receipts.PublicationReceipt &&
           outcome.FinalReopenReceipt == receipts.FinalReopenReceipt;

    private static bool ValidTerminalWireShape(byte[] frameBytes)
    {
        using var document = JsonDocument.Parse(frameBytes);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            return false;

        var terminalCount = 0;
        var terminalKind = false;
        JsonElement terminal = default;
        foreach (var property in root.EnumerateObject())
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(property.Name, "TerminalOutcome"))
            {
                if (!StringComparer.Ordinal.Equals(property.Name, "TerminalOutcome") || ++terminalCount != 1)
                    return false;
                terminal = property.Value;
            }

            if (StringComparer.OrdinalIgnoreCase.Equals(property.Name, "Kind") &&
                property.Value.ValueKind == JsonValueKind.String &&
                StringComparer.Ordinal.Equals(property.Value.GetString(), "terminal"))
            {
                terminalKind = true;
            }
        }

        if (terminalCount == 0 && !terminalKind)
            return true;

        var envelopeFields = new[]
        {
            "Kind", "FormatVersion", "SessionId", "Binding", "Revision",
            "PreviousFrameSha256", "Effects", "TerminalOutcome",
        };
        var bindingFields = new[]
        {
            "ObjectiveId", "GrantId", "GrantVersion", "TrainingRunId", "CandidateSha256",
            "ParentManifestSha256", "ParentWeightsSha256", "ConfigurationSha256", "ExamConditionSha256",
        };
        var effectFields = new[]
        {
            "BaselineDispatched", "BaselineCompleted", "TeachingDispatched", "TeachingCompleted",
            "EvaluatorDispatched", "EvaluatorCompleted", "OptimizerEntries", "EffectOutcomeUnknown",
        };
        if (!ExactJsonFields(root, envelopeFields, Array.Empty<string>()) ||
            !ExactJsonFields(root.GetProperty("Binding"), bindingFields, Array.Empty<string>()) ||
            !ExactJsonFields(root.GetProperty("Effects"), effectFields, Array.Empty<string>()))
        {
            return false;
        }

        if (terminal.ValueKind != JsonValueKind.Object)
            return false;

        var requiredOutcomeFields = new[]
        {
            "OutcomeId", "SchemaVersion", "ObjectiveId", "SessionId", "TrainingRunId",
            "BindingSha256", "TerminalState", "TrialEvaluationReceipt",
            "QualityDecisionReceipt", "DispositionReceipt", "FinalReopenReceipt",
        };
        if (!ExactJsonFields(terminal, requiredOutcomeFields, new[] { "PublicationReceipt" }) ||
            !ValidReceiptRefWire(terminal.GetProperty("TrialEvaluationReceipt")) ||
            !ValidReceiptRefWire(terminal.GetProperty("QualityDecisionReceipt")) ||
            !ValidReceiptRefWire(terminal.GetProperty("DispositionReceipt")) ||
            !ValidReceiptRefWire(terminal.GetProperty("FinalReopenReceipt")))
        {
            return false;
        }

        var terminalState = terminal.GetProperty("TerminalState");
        if (terminalState.ValueKind != JsonValueKind.String)
            return false;
        var state = terminalState.GetString();
        var hasPublication = terminal.TryGetProperty("PublicationReceipt", out var publication);
        return state switch
        {
            nameof(CodeDocumentationOneLessonTerminalStateV1.TERMINAL_NEGATIVE_PARENT_RETAINED)
                => !hasPublication,
            nameof(CodeDocumentationOneLessonTerminalStateV1.TERMINAL_PROMOTED)
                => hasPublication && ValidReceiptRefWire(publication),
            _ => false,
        };
    }

    private static bool ValidReceiptRefWire(JsonElement receipt)
    {
        if (!ExactJsonFields(receipt, new[] { "ReceiptId", "ReceiptSha256" }, Array.Empty<string>()))
            return false;
        var id = receipt.GetProperty("ReceiptId");
        var sha256 = receipt.GetProperty("ReceiptSha256");
        return id.ValueKind == JsonValueKind.String &&
               ValidText(id.GetString(), 256) &&
               sha256.ValueKind == JsonValueKind.String &&
               Sha256Hex(sha256.GetString());
    }

    private static bool ExactJsonFields(
        JsonElement element,
        IReadOnlyCollection<string> required,
        IReadOnlyCollection<string> optional)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return false;
        var known = required.Concat(optional).ToArray();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            var canonical = known.FirstOrDefault(name =>
                StringComparer.OrdinalIgnoreCase.Equals(name, property.Name));
            if (canonical is null ||
                !StringComparer.Ordinal.Equals(canonical, property.Name) ||
                !seen.Add(canonical))
            {
                return false;
            }
        }

        return required.All(seen.Contains);
    }

    private static bool ValidEffects(CodeDocumentationOneLessonEffectAccounting effects)
        => effects.BaselineDispatched >= 0 &&
           effects.BaselineCompleted >= 0 &&
           effects.BaselineCompleted <= effects.BaselineDispatched &&
           effects.TeachingDispatched >= 0 &&
           effects.TeachingCompleted >= 0 &&
           effects.TeachingCompleted <= effects.TeachingDispatched &&
           effects.EvaluatorDispatched >= 0 &&
           effects.EvaluatorCompleted >= 0 &&
           effects.EvaluatorCompleted <= effects.EvaluatorDispatched &&
           effects.OptimizerEntries >= 0;

    internal static bool HasUnresolvedEffects(CodeDocumentationOneLessonEffectAccounting effects)
        => effects.EffectOutcomeUnknown ||
           effects.BaselineDispatched > effects.BaselineCompleted ||
           effects.TeachingDispatched > effects.TeachingCompleted ||
           effects.EvaluatorDispatched > effects.EvaluatorCompleted;

    private static bool Monotonic(
        CodeDocumentationOneLessonEffectAccounting current,
        CodeDocumentationOneLessonEffectAccounting next)
        => next.BaselineDispatched >= current.BaselineDispatched &&
           next.BaselineCompleted >= current.BaselineCompleted &&
           next.TeachingDispatched >= current.TeachingDispatched &&
           next.TeachingCompleted >= current.TeachingCompleted &&
           next.EvaluatorDispatched >= current.EvaluatorDispatched &&
           next.EvaluatorCompleted >= current.EvaluatorCompleted &&
           next.OptimizerEntries >= current.OptimizerEntries &&
           (!current.EffectOutcomeUnknown || next.EffectOutcomeUnknown);

    private static bool ValidText(string? value, int maxBytes)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsControl))
            return false;

        try
        {
            return Utf8.GetByteCount(value) <= maxBytes;
        }
        catch (EncoderFallbackException)
        {
            return false;
        }
    }

    private static bool Sha256Hex(string? value)
        => value is { Length: 64 } &&
           value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static string Sha256(string value)
        => Convert.ToHexString(SHA256.HashData(Utf8.GetBytes(value))).ToLowerInvariant();

    private static string Sha256(byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();
    }

    private static bool Inside(string parent, string child)
    {
        var fullParent = Path.TrimEndingDirectorySeparator(Path.GetFullPath(parent));
        var fullChild = Path.GetFullPath(child);
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return fullChild.StartsWith(fullParent + Path.DirectorySeparatorChar, comparison);
    }

    private static bool SamePath(string left, string right)
        => StringComparerForPath.Equals(Path.GetFullPath(left), Path.GetFullPath(right));

    private static StringComparer StringComparerForPath
        => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    private static OperationResult<T> Invalid<T>()
        => Failure<T>(InvalidErrorCode, "The one-lesson session binding or repository root is invalid.");

    private static OperationResult<T> RecoveryRequired<T>()
        => Failure<T>(RecoveryRequiredErrorCode, "The durable one-lesson session is incomplete or inconsistent.");

    private static OperationResult<T> Unavailable<T>()
        => Failure<T>(UnavailableErrorCode, "The durable one-lesson session could not be safely opened or written.");

    private static OperationResult<T> Failure<T>(string code, string message)
        => OperationResult<T>.Fail(message, code);

    private sealed record SessionFrame(
        string Kind,
        int FormatVersion,
        string SessionId,
        CodeDocumentationOneLessonSessionBinding Binding,
        long Revision,
        string PreviousFrameSha256,
        CodeDocumentationOneLessonEffectAccounting Effects,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        CodeDocumentationOneLessonCanonicalOutcomeV1? TerminalOutcome);

    private sealed record HistoryReadback(
        CodeDocumentationOneLessonSessionBinding Binding,
        long Revision,
        CodeDocumentationOneLessonEffectAccounting Effects,
        string TipSha256,
        CodeDocumentationOneLessonCanonicalOutcomeV1? TerminalOutcome,
        string? TerminalOutcomeSha256,
        byte[] HistoryBytes)
    {
        internal string HistorySha256 => Sha256(HistoryBytes);
    }
}
