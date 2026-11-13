// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace ContinuousLearning;

using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

/// <summary>
/// Хранилище единственной возможности экзамена. Текущий history-anchor — доверенный
/// сохраняемый вход, а не доказательство отсутствия совместного rollback anchor и данных.
/// Production host обязан обеспечить его сохранность независимо от восстановления sessions.
/// </summary>
internal sealed class CodeDocumentationCertificationSessionStore : IDisposable
{
    internal const string ErrorPrefix = "CODE_DOCUMENTATION_CERTIFICATION_SESSION_";
    internal const string DirectoryName = ".code-documentation-certification-v1";
    internal const string InitializationStateDirectoryName =
        ".code-documentation-certification-v1-state";
    internal const string InitializationMarkerFileName = "initialized.json";
    internal const string Policy = "no-continuation-after-reservation-v1";
    internal const int MaximumEventBytes = 2 * 1024 * 1024;
    internal const int MaximumHistoryBytes = 2 * 1024 * 1024;
    internal const int MaximumScanBytes = 64 * 1024 * 1024;
    internal const int MaximumSessions = 256;
    internal const int MaximumMaterialExposures = 256;
    internal const int MaximumMaterialExposureKeys = 11_000;
    internal const int MaximumMaterialExposureSourceKeys = 10_000;
    internal const int MaximumAuthoringMaterialExposureContentKeys = 1_000;
    internal const int MaximumDiagnosticMaterialExposureContentKeys = 200;
    internal const int MaximumHistoryFrames = 1 + 3 * MaximumSessions + MaximumMaterialExposures;
    private const string EventSchema = "code-documentation-certification-session-envelope-v1";
    private const string AnchorSchema = "code-documentation-certification-history-envelope-v1";
    private const string MaterialExposureEventSchema = "code-documentation-material-exposure-envelope-v1";
    private const string MaterialExposureIdentitySchema = "code-documentation-material-exposure-identity-v1";
    private const string InitializationStateSchema =
        "code-documentation-certification-history-state-v1";
    private static readonly UTF8Encoding Utf8 = new(false, true);
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        MaxDepth = 16
    };

    // Только точки сбоя текущей операции: ни обхода custody, ни альтернативного I/O.
    // AsyncLocal не переносит hook между параллельными независимыми fixtures.
    internal static readonly AsyncLocal<Action<string>?> FaultInjection = new();
    // Только непубличная диагностика CI: без payload, путей и Exception.Message.
    // CachedLastPInvokeError — наблюдение на границе catch, не доказательство причины.
    internal static readonly AsyncLocal<Action<FailureObservation>?> FailureDiagnostic = new();
    internal sealed record FailureObservation(string Boundary, string ExceptionType, int HResult,
        int CachedLastPInvokeError, int? PathLength, string ThrowingMember);
    private readonly string _repository;
    private readonly string _root;
    private readonly string _storeId;
    private readonly object _gate = new();
    private readonly ServerOwnedCertificationHistoryAnchor _anchor;
    private FileStream? _lease;
    private CodeDocumentationCertificationSessionReservation? _active;
    private CodeDocumentationCertificationSessionStartPermit? _permit;
    private CodeDocumentationMaterialExposureUse? _materialUse;
    private bool _disposed;

    private CodeDocumentationCertificationSessionStore(string repository, string root, string storeId)
    {
        _repository = repository;
        _root = root;
        _storeId = storeId;
        _anchor = new(this);
    }

    internal static OperationResult<CodeDocumentationCertificationSessionStore> CreateNewForServerOwnedRoot(
        string trustedRepositoryRoot, string expectedStoreId)
        => Result(() =>
        {
            var store = Resolve(trustedRepositoryRoot, expectedStoreId);
            Check(!Directory.Exists(store._root) && !File.Exists(store._root), "STORE_EXISTS");
            Check(!Directory.Exists(store.InitializationStateRoot) &&
                !File.Exists(store.InitializationStateRoot), "HISTORY_EXPECTED");
            Hit("initialization-absent");
            PrivateDirectory(store._root);
            Hit("initialization-directory");
            var success = false;
            try
            {
                // CreateNew решает гонку; проигравший никогда не удаляет каталог победителя.
                store._lease = OpenPrivate(store.PathFor(".owner.lock"), FileMode.CreateNew, FileAccess.ReadWrite);
                var marker = store.LockBytes();
                store._lease.Write(marker);
                store._lease.Flush(true);
                CheckBytes(store._lease, marker, 256, "STORE_INVALID");
                Hit("initialization-lock");
                PrivateDirectory(store.PathFor("sessions"));
                PrivateDirectory(store.PathFor("history-anchor"));
                var bytes = Envelope(new StoreEvent("store", 1, expectedStoreId, store._repository, Policy));
                store._anchor.Initialize(bytes);
                Hit("initialization-genesis");
                Hit("initialization-before-store-publish");
                store.PublishUnanchored("store.json", bytes);
                store.ReadHistory();
                store.PublishInitializationMarker();
                success = true;
                return store;
            }
            finally { store.CloseLease(!success); }
        });

    internal static OperationResult<CodeDocumentationCertificationSessionStore> OpenExistingForServerOwnedRoot(
        string trustedRepositoryRoot, string expectedStoreId)
        => Result(() =>
        {
            var store = Resolve(trustedRepositoryRoot, expectedStoreId);
            Check(Directory.Exists(store._root), "STORE_MISSING");
            var success = false;
            try
            {
                store.Acquire();
                store.ReadHistory();
                store.ValidateInitializationMarker();
                success = true;
                return store;
            }
            finally { store.CloseLease(!success); }
        });

    internal OperationResult<CodeDocumentationCertificationSessionReservation> ReserveOrReplay(
        ServerOwnedCodeDocumentationCertificationScope authority, string diagnosticRunId,
        CancellationToken cancellationToken = default)
        => Result(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            Check(Id(diagnosticRunId), "INPUT_INVALID");
            Check(authority is not null, "INPUT_INVALID");
            var admitted = CodeDocumentationCertificationAdmission.TryCreate(
                authority, authority.Candidate, authority.Corpus, authority.Index);
            Check(admitted.IsSuccess && admitted.Value is not null, "INPUT_INVALID");
            var admission = BoundAdmission.Capture(admitted.Value!);
            var keys = ExposureKeys(authority);
            lock (_gate)
            {
                Acquire();
                var retainLease = false;
                var success = false;
                try
                {
                    var history = ReadHistory();
                    var sessionId = admission.AdmissionIdentitySha256[7..];
                    Check(!history.Exposures.Values.Any(exposure => exposure.MaterialExposureKeys.Intersect(keys).Any()),
                        "SOURCE_ALREADY_EXPOSED");
                    if (history.Sessions.TryGetValue(sessionId, out var existing))
                    {
                        Check(Equal(existing.Reservation.Admission, admission), "IDENTITY_MISMATCH");
                        Check(existing.Completion is not null, "INCOMPLETE_EXPOSED");
                        var replayReceipt = new CodeDocumentationCertificationSessionReceipt(existing.Completion!.Receipt,
                            existing.Reservation.Admission.AdmissionIdentitySha256,
                            "sha256:" + history.Anchor.Events[EventPath(sessionId, "completed")].Sha256);
                        success = true;
                        return CodeDocumentationCertificationSessionReservation.Mint(this, sessionId,
                            existing.Reservation.DiagnosticRunId, existing.Reservation.ReservedAtUtc,
                            replayReceipt, isReplay: true);
                    }
                    Check(!history.Sessions.Values.Any(s => s.Reservation.ExposureKeys.Intersect(keys).Any()),
                        "SOURCE_ALREADY_EXPOSED");
                    Check(history.Sessions.Count < MaximumSessions, "LIMIT_EXCEEDED");
                    cancellationToken.ThrowIfCancellationRequested();
                    var reservation = new ReservationEvent("reservation", 1, _storeId, sessionId, Policy,
                        diagnosticRunId, DateTimeOffset.UtcNow, admission, authority.Evaluator.MaxTeacherCalls, keys);
                    ValidateReservation(reservation);
                    var reservationBytes = Envelope(reservation);
                    _anchor.EnsureAppendFits(history.Anchor, sessionId, "reservation", reservationBytes);
                    PrivateDirectory(PathFor("sessions/" + sessionId));
                    Commit(history.Anchor, sessionId, "reservation", reservationBytes);
                    cancellationToken.ThrowIfCancellationRequested();
                    _active = CodeDocumentationCertificationSessionReservation.Mint(this, sessionId,
                        diagnosticRunId, reservation.ReservedAtUtc, null, isReplay: false);
                    retainLease = true;
                    success = true;
                    return _active;
                }
                finally { if (!retainLease) CloseLease(!success); }
            }
        });

    internal OperationResult<CodeDocumentationMaterialExposureUse> RecordMaterialExposureAndHoldLease(
        CodeDocumentationMaterialSnapshot material, CertificationExposureRole role,
        CancellationToken cancellationToken = default)
        => Result(() =>
        {
            Check(material is not null && material.Role == role && material.IsOwnedBy(this), "INVALID_PERMIT");
            Check(role is CertificationExposureRole.AuthoringOrTeaching or CertificationExposureRole.TeacherVisibleDiagnostic,
                "INPUT_INVALID");
            Check(ValidateOwnerBinding(material.TrustedRepositoryRoot, material.ExpectedStoreId).IsSuccess,
                "IDENTITY_MISMATCH");
            var materialRole = MaterialRole(role);
            Check(ShaReference(material.SourceBindingIdentitySha256), "INPUT_INVALID");
            if (role == CertificationExposureRole.TeacherVisibleDiagnostic)
                Check(Hex(material.DiagnosticCorpusManifestSha256), "INPUT_INVALID");
            else
                Check(material.DiagnosticCorpusManifestSha256 is null, "INPUT_INVALID");
            ValidateMaterialExposureKeys(material.MaterialExposureKeys, materialRole, "INPUT_INVALID");
            lock (_gate)
            {
                Acquire();
                var retainLease = false;
                var success = false;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var history = ReadHistory();
                    var identity = ComputeMaterialExposureIdentity(material);
                    var identityHex = identity[7..];
                    if (history.Exposures.TryGetValue(identityHex, out var existing))
                    {
                        Check(existing.Role == MaterialRole(role) && existing.ExposureIdentitySha256 == identity &&
                            existing.SourceBindingIdentitySha256 == material.SourceBindingIdentitySha256 &&
                            existing.DiagnosticCorpusManifestSha256 == material.DiagnosticCorpusManifestSha256 &&
                            existing.MaterialExposureKeys.SequenceEqual(material.MaterialExposureKeys), "HISTORY_MISMATCH");
                        var replay = CodeDocumentationMaterialExposureUse.Mint(this, identity, existing.Role);
                        _materialUse = replay;
                        retainLease = true;
                        success = true;
                        return replay;
                    }

                    Check(history.Exposures.Count < MaximumMaterialExposures, "LIMIT_EXCEEDED");
                    cancellationToken.ThrowIfCancellationRequested();
                    var exposure = new MaterialExposureEvent("material-exposure", 1, _storeId, identity,
                        MaterialRole(role), material.SourceBindingIdentitySha256, material.DiagnosticCorpusManifestSha256,
                        material.MaterialExposureKeys.ToArray(), DateTimeOffset.UtcNow);
                    ValidateMaterialExposure(exposure, identityHex);
                    var bytes = Envelope(exposure, MaterialExposureEventSchema);
                    _anchor.EnsureAppendFits(history.Anchor, identityHex, "material-exposure", bytes);
                    PrivateDirectory(PathFor("exposures"));
                    PrivateDirectory(PathFor("exposures/" + identityHex));
                    Commit(history.Anchor, identityHex, "material-exposure", bytes);
                    cancellationToken.ThrowIfCancellationRequested();
                    var readback = ReadHistory();
                    Check(readback.Exposures.TryGetValue(identityHex, out var committed) &&
                        committed.ExposureIdentitySha256 == identity &&
                        committed.MaterialExposureKeys.SequenceEqual(material.MaterialExposureKeys), "HISTORY_MISMATCH");
                    var use = CodeDocumentationMaterialExposureUse.Mint(this, identity, exposure.Role);
                    _materialUse = use;
                    retainLease = true;
                    success = true;
                    return use;
                }
                finally { if (!retainLease) CloseLease(!success); }
            }
        });

    internal OperationResult<CodeDocumentationMaterialSnapshot> IssueMaterialSnapshot(
        string trustedRepositoryRoot, string expectedStoreId,
        ServerOwnedCodeDocumentationCertificationScope authority, CertificationExposureRole role)
        => Result(() =>
        {
            Check(authority is not null, "INVALID_PERMIT");
            Check(role is CertificationExposureRole.AuthoringOrTeaching or CertificationExposureRole.TeacherVisibleDiagnostic,
                "INPUT_INVALID");
            Check(ValidateOwnerBinding(trustedRepositoryRoot, expectedStoreId).IsSuccess, "IDENTITY_MISMATCH");
            var admitted = CodeDocumentationCertificationAdmission.TryCreate(
                authority, authority.Candidate, authority.Corpus, authority.Index);
            Check(admitted.IsSuccess && admitted.Value is not null, "INPUT_INVALID");
            var keys = ExposureKeys(authority);
            var manifest = role == CertificationExposureRole.TeacherVisibleDiagnostic
                ? authority.Corpus.Manifest.ManifestSha256 : null;
            Check(role != CertificationExposureRole.TeacherVisibleDiagnostic || Hex(manifest), "INPUT_INVALID");
            Check(ShaReference(authority.IndexIdentity), "INPUT_INVALID");
            return CodeDocumentationMaterialSnapshot.Issue(this, trustedRepositoryRoot, expectedStoreId, role,
                authority.IndexIdentity, manifest, keys);
        });

    internal OperationResult<CodeDocumentationMaterialSnapshot> IssueDiagnosticMaterialSnapshot(
        string trustedRepositoryRoot, string expectedStoreId,
        RepositoryOwnedReferenceCorpusOpenResult corpus)
        => Result(() =>
        {
            Check(corpus is not null, "INVALID_PERMIT");
            Check(ValidateOwnerBinding(trustedRepositoryRoot, expectedStoreId).IsSuccess,
                "IDENTITY_MISMATCH");
            var disclosed = ValidateDiagnosticCorpus(corpus);
            var keys = DiagnosticExposureKeys(disclosed);
            var sourceBinding = ComputeDiagnosticSourceBindingIdentity(
                trustedRepositoryRoot,
                expectedStoreId,
                corpus);
            return CodeDocumentationMaterialSnapshot.Issue(
                this,
                trustedRepositoryRoot,
                expectedStoreId,
                CertificationExposureRole.TeacherVisibleDiagnostic,
                sourceBinding,
                corpus.Manifest.ManifestSha256,
                keys);
        });

    private static FrozenReferenceEntry[] ValidateDiagnosticCorpus(
        RepositoryOwnedReferenceCorpusOpenResult corpus)
    {
        Check(corpus.Manifest is not null && corpus.Corpus is not null, "INVALID_PERMIT");
        var manifest = corpus.Manifest;
        var sealedCorpus = corpus.Corpus;
        var entries = sealedCorpus.Entries;
        Check(entries is { Count: > 0 }, "INPUT_INVALID");
        Check(manifest.Schema == RepositoryOwnedReferenceCorpusManifestSealer.CurrentSchema &&
            manifest.Context == RepositoryOwnedReferenceCorpusManifestSealer.CurrentContext &&
            manifest.Domain == sealedCorpus.DomainTag &&
            manifest.Subject == sealedCorpus.SubjectId &&
            manifest.CorpusVersion == sealedCorpus.CorpusVersion &&
            manifest.CorpusName == sealedCorpus.CorpusName &&
            manifest.FrozenAtUtc == FrozenReferenceCanonicalization.FormatUtc(sealedCorpus.FrozenAtUtc) &&
            manifest.EntryCount == sealedCorpus.Count &&
            manifest.CorpusSha256 == sealedCorpus.CorpusSha256 &&
            ShaReference(manifest.CorpusSha256) &&
            manifest.RepositoryRelativeCorpusDirectory is { Length: > 0 } &&
            manifest.RepositoryRelativePayloadPath ==
                manifest.RepositoryRelativeCorpusDirectory + "/" +
                RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName &&
            manifest.PayloadByteLength > 0 &&
            Hex(manifest.PayloadSha256) &&
            Hex(manifest.ManifestSha256), "INPUT_INVALID");

        // Rebuild through the frozen-reference owner so duplicate entry ids,
        // cross-partition source leakage, row canonical identities and the
        // complete corpus digest are checked together before any disclosure.
        var rebuilt = FrozenReferenceCorpus.Build(
            sealedCorpus.CorpusName,
            sealedCorpus.CorpusVersion,
            sealedCorpus.FrozenAtUtc,
            entries);
        Check(rebuilt.IsSuccess && rebuilt.Value is not null &&
            rebuilt.Value.CorpusSha256 == sealedCorpus.CorpusSha256 &&
            rebuilt.Value.CanonicalRepresentation == sealedCorpus.CanonicalRepresentation &&
            rebuilt.Value.EntryNames.SequenceEqual(
                sealedCorpus.EntryNames, StringComparer.Ordinal), "INPUT_INVALID");

        // Assessment uses this exact owner-defined partition and ordering.
        // Train rows remain part of the containing corpus identity above, but
        // never become teacher-visible diagnostic exposure keys.
        var heldOut = entries
            .Where(static entry => entry.Partition == FrozenReferencePartition.HeldOut)
            .OrderBy(static entry => entry.EntryId, StringComparer.Ordinal)
            .ToArray();
        Check(heldOut.Length is > 0 and <= MaximumDiagnosticMaterialExposureContentKeys,
            "INPUT_INVALID");
        return heldOut;
    }

    internal OperationResult<CodeDocumentationCertificationSessionStartPermit> BeginEvaluation(
        CodeDocumentationCertificationSessionReservation reservation, CancellationToken cancellationToken = default)
        => Result(() =>
        {
            lock (_gate)
            {
                CheckLive(reservation);
                Check(_permit is null, "INVALID_PERMIT");
                var success = false;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var history = ReadHistory();
                    var session = history.Sessions[reservation.SessionId];
                    Check(session.Start is null && session.Completion is null, "INVALID_PERMIT");
                    var start = new StartedEvent("started", 1, _storeId, reservation.SessionId,
                        history.Anchor.Events[EventPath(reservation.SessionId, "reservation")].Sha256,
                        DateTimeOffset.UtcNow);
                    Commit(history.Anchor, reservation.SessionId, "started", Envelope(start));
                    cancellationToken.ThrowIfCancellationRequested();
                    _permit = CodeDocumentationCertificationSessionStartPermit.Mint();
                    success = true;
                    return _permit;
                }
                finally { if (!success) CloseLease(primaryFailure: true); }
            }
        });

    internal OperationResult<CodeDocumentationCertificationSessionReceipt> CompleteFromServerOwnedExecution(
        CodeDocumentationCertificationSessionStartPermit permit,
        CodeDocumentationSavedModelAssessmentCalibrationResult result, CancellationToken cancellationToken = default)
        => Result(() =>
        {
            lock (_gate)
            {
                Check(!_disposed && _lease is not null && _active is not null &&
                    permit is not null && ReferenceEquals(permit, _permit), "INVALID_PERMIT");
                var success = false;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var history = ReadHistory();
                    var sessionId = _active!.SessionId;
                    var session = history.Sessions[sessionId];
                    Check(session.Start is not null && session.Completion is null, "INVALID_PERMIT");
                    Check(result?.RepresentativeAssessment?.IsSuccess == true && result.Calibration?.IsSuccess == true,
                        "COMPLETION_INVALID");
                    ReceiptData receipt;
                    try
                    {
                        receipt = ReceiptData.Capture(result!);
                        ValidateReceipt(session.Reservation.Admission, receipt);
                    }
                    catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                    { throw new SessionFailure("COMPLETION_INVALID"); }
                    var completion = new CompletedEvent("completed", 1, _storeId, sessionId,
                        history.Anchor.Events[EventPath(sessionId, "reservation")].Sha256,
                        history.Anchor.Events[EventPath(sessionId, "started")].Sha256,
                        DateTimeOffset.UtcNow, receipt);
                    var completionBytes = Envelope(completion);
                    Commit(history.Anchor, sessionId, "completed", completionBytes);
                    Hit("completion-before-return");
                    cancellationToken.ThrowIfCancellationRequested();
                    var answer = new CodeDocumentationCertificationSessionReceipt(receipt,
                        session.Reservation.Admission.AdmissionIdentitySha256, "sha256:" + Hash(completionBytes));
                    success = true;
                    return answer;
                }
                finally { CloseLease(!success); }
            }
        });

    internal void Release(CodeDocumentationCertificationSessionReservation reservation)
    {
        lock (_gate)
            if (ReferenceEquals(reservation, _active)) CloseLease(primaryFailure: false);
    }

    internal void ReleaseMaterialUse(CodeDocumentationMaterialExposureUse use)
    {
        lock (_gate)
            if (ReferenceEquals(use, _materialUse)) CloseLease(primaryFailure: false);
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _disposed = true;
            CloseLease(primaryFailure: false);
        }
    }

    private void CheckLive(CodeDocumentationCertificationSessionReservation reservation)
        => Check(!_disposed && _lease is not null && reservation is not null &&
            ReferenceEquals(reservation, _active) && !reservation.IsReplay, "INVALID_PERMIT");

    private void Acquire()
    {
        Check(!_disposed && _lease is null, "STORAGE_UNAVAILABLE");
        Check(Directory.Exists(_root), "STORE_MISSING");
        CheckPrivateDirectory(_root);
        Check(File.Exists(PathFor(".owner.lock")), "STORE_INVALID");
        try { _lease = OpenPrivate(PathFor(".owner.lock"), FileMode.Open, FileAccess.ReadWrite); }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        { ObserveFailure("acquire-owner-lock", exception); throw new SessionFailure("STORAGE_UNAVAILABLE"); }
        try { CheckBytes(_lease, LockBytes(), 256, "STORE_INVALID"); }
        catch { CloseLease(primaryFailure: true); throw; }
    }

    private void CloseLease(bool primaryFailure)
    {
        var lease = _lease;
        _lease = null;
        _active = null;
        _permit = null;
        _materialUse = null;
        try { lease?.Dispose(); }
        catch (Exception exception) when (primaryFailure && SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { }
    }

    private static CodeDocumentationCertificationSessionStore Resolve(string repository, string storeId)
    {
        Check(Hex(storeId), "INPUT_INVALID");
        Check(!string.IsNullOrWhiteSpace(repository) && Path.IsPathFullyQualified(repository), "ROOT_INVALID");
        var resolved = RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(repository);
        Check(resolved.IsSuccess && resolved.Value is not null, "ROOT_INVALID");
        var canonical = Path.GetFullPath(repository).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var modelRoot = Path.Combine(canonical, "training-artifacts", "models");
        Check(Directory.Exists(modelRoot) && !SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(modelRoot), "ROOT_INVALID");
        return new(canonical, Path.Combine(modelRoot, DirectoryName), storeId);
    }

    internal string InitializationStateRoot =>
        Path.Combine(Path.GetDirectoryName(_root)!, InitializationStateDirectoryName);

    internal string InitializationMarkerPath =>
        Path.Combine(InitializationStateRoot, InitializationMarkerFileName);

    internal OperationResult<bool> ValidateOwnerBinding(string trustedRepositoryRoot, string expectedStoreId)
        => Result(() =>
        {
            Check(!_disposed, "STORAGE_UNAVAILABLE");
            var resolved = Resolve(trustedRepositoryRoot, expectedStoreId);
            Check(resolved._repository == _repository && resolved._root == _root && resolved._storeId == _storeId,
                "IDENTITY_MISMATCH");
            return true;
        });

    private string PathFor(string relative)
    {
        Check(relative.Length > 0 && !relative.Contains('\\') && !relative.Split('/').Any(s => s is "" or "." or ".."), "STORE_INVALID");
        var path = Path.GetFullPath(Path.Combine(_root, relative.Replace('/', Path.DirectorySeparatorChar)));
        Check(path.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
            !SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path), "STORE_INVALID");
        return path;
    }

    private static void PrivateDirectory(string path)
    {
        Check(!File.Exists(path) && !SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path), "STORE_INVALID");
        try { SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(path); }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        { ObserveFailure("create-private-directory", exception, path.Length); throw; }
        CheckPrivateDirectory(path);
    }

    private static void CheckPrivateDirectory(string path)
        => Check(Directory.Exists(path) && !SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) &&
            SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(path), "STORE_INVALID");

    private byte[] InitializationMarkerBytes()
        => JsonSerializer.SerializeToUtf8Bytes(
            new InitializationMarker(
                InitializationStateSchema,
                _repository,
                _storeId),
            Json);

    private void PublishInitializationMarker()
    {
        Check(!Directory.Exists(InitializationStateRoot) &&
            !File.Exists(InitializationStateRoot), "HISTORY_EXPECTED");
        PrivateDirectory(InitializationStateRoot);
        var marker = InitializationMarkerBytes();
        var stage = Path.Combine(
            InitializationStateRoot,
            InitializationMarkerFileName + ".stage");
        using (var stream = OpenPrivate(stage, FileMode.CreateNew, FileAccess.ReadWrite))
        {
            stream.Write(marker);
            stream.Flush(true);
            CheckBytes(stream, marker, MaximumEventBytes, "STORAGE_UNAVAILABLE");
        }
        Check(SecureLocalFileBoundary.PublishAtomicDurable(stage, InitializationMarkerPath),
            "STORAGE_UNAVAILABLE");
        ValidateInitializationMarker();
    }

    private void ValidateInitializationMarker()
    {
        Check(Directory.Exists(InitializationStateRoot), "HISTORY_INCOMPLETE");
        CheckPrivateDirectory(InitializationStateRoot);
        Check(!SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(InitializationMarkerPath),
            "STORE_INVALID");
        Check(File.Exists(InitializationMarkerPath), "HISTORY_INCOMPLETE");
        Check(new FileInfo(InitializationMarkerPath).Length <= MaximumEventBytes, "LIMIT_EXCEEDED");
        Check(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(InitializationMarkerPath),
            "STORE_INVALID");
        using var stream = OpenPrivate(InitializationMarkerPath, FileMode.Open, FileAccess.Read);
        var actual = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, MaximumEventBytes);
        Check(actual is not null && actual.SequenceEqual(InitializationMarkerBytes()), "HISTORY_MISMATCH");
    }

    private static FileStream OpenPrivate(string path, FileMode mode, FileAccess access)
    {
        FileStream stream;
        try { stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, mode, access, FileShare.None, FileOptions.WriteThrough); }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        { ObserveFailure("open-private-file", exception, path.Length); throw; }
        try
        {
            Check(SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path), "STORE_INVALID");
            return stream;
        }
        catch
        {
            try { stream.Dispose(); }
            catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { }
            throw;
        }
    }

    private byte[] Read(string relative, int bound = MaximumEventBytes)
    {
        var path = PathFor(relative);
        Check(File.Exists(path), "HISTORY_INCOMPLETE");
        Check(new FileInfo(path).Length <= bound, "LIMIT_EXCEEDED");
        using var stream = OpenPrivate(path, FileMode.Open, FileAccess.Read);
        var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, bound);
        Check(bytes is not null, "HISTORY_MISMATCH");
        return bytes!;
    }

    private byte[] LockBytes() => Utf8.GetBytes("certification-session-lock-v1\n" + _storeId + "\n");
    private static void CheckBytes(FileStream stream, byte[] expected, int bound, string error)
        => Check(SecureLocalFileBoundary.ReadBoundedStableBytes(stream, bound) is { } actual && actual.SequenceEqual(expected), error);

    private void PublishUnanchored(string relative, byte[] bytes)
    {
        var stage = Stage(relative, bytes);
        Check(SecureLocalFileBoundary.PublishAtomicDurable(stage, PathFor(relative)), "STORAGE_UNAVAILABLE");
        Check(Read(relative).SequenceEqual(bytes), "HISTORY_MISMATCH");
    }

    private string Stage(string relative, byte[] bytes)
    {
        Check(bytes.Length is > 0 and <= MaximumEventBytes, "LIMIT_EXCEEDED");
        var stage = PathFor(relative + ".stage");
        using (var stream = OpenPrivate(stage, FileMode.CreateNew, FileAccess.ReadWrite))
        {
            stream.Write(bytes);
            stream.Flush(true);
            CheckBytes(stream, bytes, MaximumEventBytes, "STORAGE_UNAVAILABLE");
        }
        return stage;
    }

    private void Commit(CertificationHistorySnapshot previous, string sessionId, string kind, byte[] bytes)
    {
        _anchor.EnsureAppendFits(previous, sessionId, kind, bytes);
        var relative = EventPath(sessionId, kind);
        var stage = Stage(relative, bytes);
        Hit(kind + "-stage-closed");
        _anchor.Append(previous, sessionId, kind, relative, bytes);
        Hit(kind + "-anchor-committed");
        Check(SecureLocalFileBoundary.PublishAtomicDurable(stage, PathFor(relative)), "STORAGE_UNAVAILABLE");
        Hit(kind + "-event-published");
        Check(Read(relative).SequenceEqual(bytes), "HISTORY_MISMATCH");
    }

    private History ReadHistory()
    {
        Check(_lease is not null && SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(_lease.SafeFileHandle, PathFor(".owner.lock")), "STORE_INVALID");
        CheckBytes(_lease!, LockBytes(), 256, "STORE_INVALID");
        CheckPrivateDirectory(_root);
        var snapshot = _anchor.ReadCurrent();
        Check(snapshot.Events.Keys.All(path => File.Exists(PathFor(path))), "HISTORY_INCOMPLETE");
        CheckPrivateDirectory(PathFor("sessions"));
        CheckPrivateDirectory(PathFor("history-anchor"));
        var actual = new HashSet<string>(StringComparer.Ordinal);
        long scanBytes = snapshot.AnchorByteLength + LockBytes().Length;
        void Add(string path)
        {
            var relative = Path.GetRelativePath(_root, path).Replace('\\', '/');
            Check(!SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path), "STORE_INVALID");
            scanBytes = checked(scanBytes + new FileInfo(path).Length);
            Check(scanBytes <= MaximumScanBytes, "LIMIT_EXCEEDED");
            Check(actual.Add(relative), "HISTORY_MISMATCH");
        }
        var hasExposures = false;
        foreach (var entry in BoundedSnapshot(Directory.EnumerateFileSystemEntries(_root), 5, "LIMIT_EXCEEDED"))
        {
            var name = Path.GetFileName(entry);
            Check(name is "store.json" or ".owner.lock" or "sessions" or "history-anchor" or "exposures", "HISTORY_MISMATCH");
            if (name == "store.json") Add(entry);
            if (name == "exposures")
            {
                Check(Directory.Exists(entry), "HISTORY_MISMATCH");
                CheckPrivateDirectory(entry);
                hasExposures = true;
            }
        }
        var anchorEntries = BoundedSnapshot(Directory.EnumerateFileSystemEntries(PathFor("history-anchor")), 1, "LIMIT_EXCEEDED");
        Check(anchorEntries.Length == 1 && Path.GetFileName(anchorEntries[0]) == "commitments.jsonl" &&
            File.Exists(anchorEntries[0]), "HISTORY_MISMATCH");
        var directories = BoundedSnapshot(Directory.EnumerateFileSystemEntries(PathFor("sessions")), MaximumSessions, "LIMIT_EXCEEDED");
        foreach (var directory in directories)
        {
            Check(Directory.Exists(directory) && Hex(Path.GetFileName(directory)), "HISTORY_MISMATCH");
            CheckPrivateDirectory(directory);
            var entries = BoundedSnapshot(Directory.EnumerateFileSystemEntries(directory), 3, "LIMIT_EXCEEDED");
            Check(entries.Length > 0, "HISTORY_MISMATCH");
            foreach (var file in entries)
            {
                Check(File.Exists(file) && Path.GetFileName(file) is "reservation.json" or "started.json" or "completed.json", "HISTORY_MISMATCH");
                Add(file);
            }
        }
        var exposureDirectories = Array.Empty<string>();
        if (hasExposures)
        {
            exposureDirectories = BoundedSnapshot(Directory.EnumerateFileSystemEntries(PathFor("exposures")),
                MaximumMaterialExposures, "LIMIT_EXCEEDED");
            Check(exposureDirectories.Length > 0, "HISTORY_MISMATCH");
            foreach (var directory in exposureDirectories)
            {
                Check(Directory.Exists(directory) && Hex(Path.GetFileName(directory)), "HISTORY_MISMATCH");
                CheckPrivateDirectory(directory);
                var entries = BoundedSnapshot(Directory.EnumerateFileSystemEntries(directory), 2, "LIMIT_EXCEEDED");
                Check(entries.Length == 1 && File.Exists(entries[0]) && Path.GetFileName(entries[0]) == "exposure.json",
                    "HISTORY_MISMATCH");
                Add(entries[0]);
            }
        }
        Check(snapshot.Events.Keys.All(actual.Contains), "HISTORY_INCOMPLETE");
        Check(actual.SetEquals(snapshot.Events.Keys), "HISTORY_MISMATCH");
        var bytesByPath = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        foreach (var (path, binding) in snapshot.Events)
        {
            var bytes = Read(path);
            Check(bytes.Length == binding.ByteLength && Hash(bytes) == binding.Sha256, "HISTORY_MISMATCH");
            bytesByPath.Add(path, bytes);
        }
        var store = ParseEnvelope<StoreEvent>(bytesByPath["store.json"]);
        Check(Equal(store, new StoreEvent("store", 1, _storeId, _repository, Policy)), "IDENTITY_MISMATCH");
        var sessions = new Dictionary<string, SessionData>(StringComparer.Ordinal);
        foreach (var directory in directories)
        {
            var id = Path.GetFileName(directory);
            var reservationPath = EventPath(id, "reservation");
            Check(bytesByPath.ContainsKey(reservationPath), "HISTORY_INCOMPLETE");
            var reservation = ParseEnvelope<ReservationEvent>(bytesByPath[reservationPath]);
            ValidateReservation(reservation);
            Check(reservation.StoreId == _storeId && reservation.SessionId == id, "IDENTITY_MISMATCH");
            StartedEvent? start = null;
            CompletedEvent? completion = null;
            if (bytesByPath.TryGetValue(EventPath(id, "started"), out var startBytes))
            {
                start = ParseEnvelope<StartedEvent>(startBytes);
                Check(start.Kind == "started" && start.FormatVersion == 1 && start.StoreId == _storeId && start.SessionId == id &&
                    start.ReservationEnvelopeSha256 == snapshot.Events[reservationPath].Sha256 &&
                    Utc(start.StartedAtUtc) && start.StartedAtUtc >= reservation.ReservedAtUtc, "HISTORY_MISMATCH");
            }
            if (bytesByPath.TryGetValue(EventPath(id, "completed"), out var completedBytes))
            {
                completion = ParseEnvelope<CompletedEvent>(completedBytes);
                Check(start is not null && completion.Kind == "completed" && completion.FormatVersion == 1 &&
                    completion.StoreId == _storeId && completion.SessionId == id &&
                    completion.ReservationEnvelopeSha256 == snapshot.Events[reservationPath].Sha256 &&
                    completion.StartedEnvelopeSha256 == snapshot.Events[EventPath(id, "started")].Sha256 &&
                    Utc(completion.CompletedAtUtc) && completion.CompletedAtUtc >= start!.StartedAtUtc, "HISTORY_MISMATCH");
                ValidateReceipt(reservation.Admission, completion.Receipt);
            }
            sessions.Add(id, new(reservation, start, completion));
        }
        var exposures = new Dictionary<string, MaterialExposureEvent>(StringComparer.Ordinal);
        foreach (var directory in exposureDirectories)
        {
            var id = Path.GetFileName(directory);
            var path = EventPath(id, "material-exposure");
            Check(bytesByPath.TryGetValue(path, out var exposureBytes), "HISTORY_INCOMPLETE");
            var exposure = ParseEnvelope<MaterialExposureEvent>(exposureBytes!, MaterialExposureEventSchema);
            ValidateMaterialExposure(exposure, id);
            Check(exposures.TryAdd(id, exposure), "HISTORY_MISMATCH");
        }
        Check(exposures.Count <= MaximumMaterialExposures, "LIMIT_EXCEEDED");
        return new(snapshot, sessions, exposures);
    }

    private sealed class ServerOwnedCertificationHistoryAnchor(CodeDocumentationCertificationSessionStore owner)
    {
        private const string RelativePath = "history-anchor/commitments.jsonl";

        internal void Initialize(byte[] storeBytes)
        {
            var genesis = new Genesis("history-genesis", 1, owner._storeId, owner._repository, Policy, 0, "",
                "store.json", storeBytes.Length, Hash(storeBytes));
            var line = Envelope(genesis, AnchorSchema).Concat(new byte[] { 10 }).ToArray();
            owner.PublishUnanchored(RelativePath, line);
            ReadCurrent();
        }

        internal CertificationHistorySnapshot ReadCurrent()
        {
            var bytes = owner.Read(RelativePath, MaximumHistoryBytes);
            Hit("history-bytes-read");
            return Parse(bytes);
        }

        internal void Append(CertificationHistorySnapshot expected, string sessionId, string kind, string path, byte[] eventBytes)
        {
            EnsureAppendFits(expected, sessionId, kind, eventBytes);
            using var stream = OpenPrivate(owner.PathFor(RelativePath), FileMode.Open, FileAccess.ReadWrite);
            var previousBytes = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, MaximumHistoryBytes);
            Check(previousBytes is not null, "HISTORY_MISMATCH");
            var current = Parse(previousBytes!);
            Check(current.Sequence == expected.Sequence && current.Tip == expected.Tip, "HISTORY_MISMATCH");
            Check(!current.Events.ContainsKey(path), "HISTORY_MISMATCH");
            var commit = new HistoryCommit("history-commit", 1, owner._storeId, current.Sequence + 1, current.Tip,
                sessionId, kind, path, eventBytes.Length, Hash(eventBytes));
            var frame = Envelope(commit, AnchorSchema).Concat(new byte[] { 10 }).ToArray();
            Check(current.Sequence + 2 <= MaximumHistoryFrames && previousBytes!.Length + frame.Length <= MaximumHistoryBytes,
                "LIMIT_EXCEEDED");
            stream.Position = stream.Length;
            var half = frame.Length / 2;
            stream.Write(frame.AsSpan(0, half));
            Hit(kind + "-anchor-partial");
            stream.Write(frame.AsSpan(half));
            stream.Flush(true);
            var readbackBytes = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, MaximumHistoryBytes);
            Check(readbackBytes is not null && readbackBytes.SequenceEqual(previousBytes.Concat(frame)), "HISTORY_MISMATCH");
            var readback = Parse(readbackBytes!);
            Check(readback.Sequence == commit.Sequence && readback.Events[path] == new EventBinding(eventBytes.Length, Hash(eventBytes)), "HISTORY_MISMATCH");
        }

        internal void EnsureAppendFits(CertificationHistorySnapshot snapshot, string sessionId, string kind, byte[] eventBytes)
        {
            Check(eventBytes.Length is > 0 and <= MaximumEventBytes, "LIMIT_EXCEEDED");
            var next = new HistoryCommit("history-commit", 1, owner._storeId, snapshot.Sequence + 1, snapshot.Tip,
                sessionId, kind, EventPath(sessionId, kind), eventBytes.Length, Hash(eventBytes));
            var nextAnchorLength = checked(snapshot.AnchorByteLength + Envelope(next, AnchorSchema).Length + 1);
            Check(snapshot.Sequence + 2 <= MaximumHistoryFrames && nextAnchorLength <= MaximumHistoryBytes &&
                nextAnchorLength + owner.LockBytes().Length + snapshot.Events.Values.Sum(e => (long)e.ByteLength) + eventBytes.Length <= MaximumScanBytes,
                "LIMIT_EXCEEDED");
        }

        private CertificationHistorySnapshot Parse(byte[] bytes)
        {
            Check(bytes.Length > 0 && bytes[^1] == 10 && !bytes.Contains((byte)13), "HISTORY_MISMATCH");
            var text = Utf8.GetString(bytes);
            var lines = new List<string>();
            for (var position = 0; position < text.Length;)
            {
                var end = text.IndexOf('\n', position);
                Check(end > position, "HISTORY_MISMATCH");
                Check(lines.Count < MaximumHistoryFrames, "LIMIT_EXCEEDED");
                lines.Add(text[position..end]);
                position = end + 1;
            }
            Check(lines.Count > 0, "HISTORY_MISMATCH");
            var genesisBytes = Utf8.GetBytes(lines[0]);
            var genesis = ParseEnvelope<Genesis>(genesisBytes, AnchorSchema);
            Hit("history-genesis-parsed");
            Check(genesis.Kind == "history-genesis" && genesis.FormatVersion == 1 && genesis.StoreId == owner._storeId &&
                genesis.CanonicalRepositoryRoot == owner._repository && genesis.PolicyId == Policy && genesis.Sequence == 0 &&
                genesis.PreviousTipSha256 == "" && genesis.EventPath == "store.json" &&
                genesis.EventByteLength is > 0 and <= MaximumEventBytes && Hex(genesis.EventSha256), "HISTORY_MISMATCH");
            var events = new Dictionary<string, EventBinding>(StringComparer.Ordinal)
            { ["store.json"] = new(genesis.EventByteLength, genesis.EventSha256) };
            var stages = new Dictionary<string, int>(StringComparer.Ordinal);
            var exposureIds = new HashSet<string>(StringComparer.Ordinal);
            var tip = Hash(genesisBytes);
            for (var i = 1; i < lines.Count; i++)
            {
                var frame = Utf8.GetBytes(lines[i]);
                var commit = ParseEnvelope<HistoryCommit>(frame, AnchorSchema);
                Check(Hex(commit.SessionId), "HISTORY_MISMATCH");
                var stage = commit.EventKind switch { "reservation" => 1, "started" => 2, "completed" => 3, _ => 0 };
                Check(commit.Kind == "history-commit" && commit.FormatVersion == 1 && commit.StoreId == owner._storeId &&
                    commit.Sequence == i && commit.PreviousTipSha256 == tip && Hex(commit.SessionId) &&
                    commit.EventPath == EventPath(commit.SessionId, commit.EventKind) &&
                    commit.EventByteLength is > 0 and <= MaximumEventBytes && Hex(commit.EventSha256) &&
                    events.TryAdd(commit.EventPath, new(commit.EventByteLength, commit.EventSha256)), "HISTORY_MISMATCH");
                if (commit.EventKind == "material-exposure")
                {
                    Check(exposureIds.Add(commit.SessionId), "HISTORY_MISMATCH");
                    Check(exposureIds.Count <= MaximumMaterialExposures, "LIMIT_EXCEEDED");
                }
                else
                {
                    stages.TryGetValue(commit.SessionId, out var previousStage);
                    Check(stage > 0 && stage == previousStage + 1, "HISTORY_MISMATCH");
                    stages[commit.SessionId] = stage;
                    Check(stages.Count <= MaximumSessions, "LIMIT_EXCEEDED");
                }
                tip = Hash(frame);
            }
            Check(stages.Count <= MaximumSessions && exposureIds.Count <= MaximumMaterialExposures, "LIMIT_EXCEEDED");
            return new(lines.Count - 1, tip, bytes.Length, events);
        }
    }

    private sealed class CertificationHistorySnapshot
    {
        internal CertificationHistorySnapshot(int sequence, string tip, int anchorByteLength, Dictionary<string, EventBinding> events)
        { Sequence = sequence; Tip = tip; AnchorByteLength = anchorByteLength; Events = new ReadOnlyDictionary<string, EventBinding>(new Dictionary<string, EventBinding>(events, StringComparer.Ordinal)); }
        internal int Sequence { get; }
        internal string Tip { get; }
        internal int AnchorByteLength { get; }
        internal IReadOnlyDictionary<string, EventBinding> Events { get; }
    }

    private static ExposureKey[] ExposureKeys(ServerOwnedCodeDocumentationCertificationScope authority)
    {
        var nodes = authority.Index.Sources.ToDictionary(s => s.SourceEntityFingerprint, StringComparer.Ordinal);
        var visited = authority.Corpus.Corpus.Entries.Select(e => e.SourceEntityFingerprint).ToHashSet(StringComparer.Ordinal);
        var pending = new Stack<string>(visited);
        while (pending.TryPop(out var fingerprint))
        {
            foreach (var ancestor in nodes[fingerprint].Ancestors)
            {
                if (!visited.Add(ancestor)) continue;
                Check(visited.Count <= 10000, "LIMIT_EXCEEDED");
                pending.Push(ancestor);
            }
        }
        return visited.Select(v => new ExposureKey("source", v))
            .Concat(authority.Corpus.Corpus.Entries.Select(e => new ExposureKey("content", FrozenReferenceEntry.ComputeNormalizedTextSha256(e.OwnerInput))))
            .Distinct().OrderBy(e => e.Kind, StringComparer.Ordinal).ThenBy(e => e.Digest, StringComparer.Ordinal).ToArray();
    }

    private static ExposureKey[] DiagnosticExposureKeys(
        IReadOnlyList<FrozenReferenceEntry> disclosed)
        => disclosed
            .Select(entry => new ExposureKey("source", entry.SourceEntityFingerprint))
            .Concat(disclosed.Select(entry => new ExposureKey(
                "content",
                FrozenReferenceEntry.ComputeNormalizedTextSha256(entry.OwnerInput))))
            .Distinct()
            .OrderBy(entry => entry.Kind, StringComparer.Ordinal)
            .ThenBy(entry => entry.Digest, StringComparer.Ordinal)
            .ToArray();

    private static string ComputeDiagnosticSourceBindingIdentity(
        string trustedRepositoryRoot,
        string expectedStoreId,
        RepositoryOwnedReferenceCorpusOpenResult corpus)
        => "sha256:" + Hash(Utf8.GetBytes(
            "code-documentation-diagnostic-source-binding-v1\n" +
            trustedRepositoryRoot + "\n" +
            expectedStoreId + "\n" +
            corpus.Manifest.ManifestSha256 + "\n" +
            corpus.Manifest.CorpusSha256 + "\n" +
            corpus.Corpus.CanonicalRepresentation));

    private string ComputeMaterialExposureIdentity(CodeDocumentationMaterialSnapshot material)
    {
        var value = new MaterialExposureIdentity(MaterialExposureIdentitySchema, _storeId,
            MaterialRole(material.Role), material.SourceBindingIdentitySha256,
            material.DiagnosticCorpusManifestSha256, material.MaterialExposureKeys.ToArray());
        return "sha256:" + Hash(JsonSerializer.SerializeToUtf8Bytes(value, Json));
    }

    private static string MaterialRole(CertificationExposureRole role)
        => role switch
        {
            CertificationExposureRole.AuthoringOrTeaching => "authoring-or-teaching",
            CertificationExposureRole.TeacherVisibleDiagnostic => "teacher-visible-diagnostic",
            _ => string.Empty
        };

    private static void ValidateMaterialExposureKeys(
        IReadOnlyList<ExposureKey>? keys, string role, string error)
    {
        Check(keys is { Count: > 0 and <= MaximumMaterialExposureKeys }, error);
        Check(keys!.All(key => key is not null && (key.Kind is "source" or "content") && ShaReference(key.Digest)), error);
        Check(keys.SequenceEqual(keys.Distinct().OrderBy(key => key.Kind, StringComparer.Ordinal)
            .ThenBy(key => key.Digest, StringComparer.Ordinal)), error);
        var sourceCount = keys.Count(key => key.Kind == "source");
        var contentCount = keys.Count(key => key.Kind == "content");
        var contentLimit = role == "teacher-visible-diagnostic"
            ? MaximumDiagnosticMaterialExposureContentKeys
            : MaximumAuthoringMaterialExposureContentKeys;
        Check(sourceCount > 0 && sourceCount <= MaximumMaterialExposureSourceKeys &&
            contentCount > 0 && contentCount <= contentLimit &&
            checked(sourceCount + contentCount) <= MaximumMaterialExposureKeys, error);
    }

    private void ValidateMaterialExposure(MaterialExposureEvent value, string expectedId)
    {
        Check(value is not null && value.Kind == "material-exposure" && value.FormatVersion == 1 &&
            value.StoreId == _storeId && Hex(expectedId) && ShaReference(value.ExposureIdentitySha256) &&
            value.ExposureIdentitySha256 == "sha256:" + expectedId &&
            value.Role is "authoring-or-teaching" or "teacher-visible-diagnostic" &&
            ShaReference(value.SourceBindingIdentitySha256) && Utc(value.FirstRecordedAtUtc), "HISTORY_MISMATCH");
        if (value.Role == "teacher-visible-diagnostic")
            Check(Hex(value.DiagnosticCorpusManifestSha256), "HISTORY_MISMATCH");
        else
            Check(value.DiagnosticCorpusManifestSha256 is null, "HISTORY_MISMATCH");
        ValidateMaterialExposureKeys(value.MaterialExposureKeys, value.Role, "HISTORY_MISMATCH");
        var identity = new MaterialExposureIdentity(MaterialExposureIdentitySchema, _storeId, value.Role,
            value.SourceBindingIdentitySha256, value.DiagnosticCorpusManifestSha256,
            value.MaterialExposureKeys.ToArray());
        Check(value.ExposureIdentitySha256 == "sha256:" + Hash(JsonSerializer.SerializeToUtf8Bytes(identity, Json)),
            "HISTORY_MISMATCH");
    }

    private static void ValidateReservation(ReservationEvent value)
    {
        Check(value.Kind == "reservation" && value.FormatVersion == 1 && Hex(value.StoreId) && Hex(value.SessionId) &&
            value.PolicyId == Policy && Id(value.DiagnosticRunId) && Utc(value.ReservedAtUtc) && value.Admission is not null &&
            "sha256:" + value.SessionId == value.Admission.AdmissionIdentitySha256, "HISTORY_MISMATCH");
        var admission = value.Admission;
        Check(admission.Schema == CodeDocumentationCertificationAdmission.CurrentSchema &&
            admission.Purpose == CodeDocumentationCertificationScopeDeclaration.CertificationPurpose &&
            Id(admission.ScopeId) && Id(admission.ScopeVersion) && Id(admission.ExposureIndexVersion) && Utc(admission.CoverageCutUtc) &&
            ShaReference(admission.ScopeIdentitySha256) && ShaReference(admission.ExposureIndexIdentitySha256) &&
            ShaReference(admission.EvaluationConditionsIdentitySha256) && ShaReference(admission.AdmissionIdentitySha256) &&
            CertificationValidation.Candidate(admission.Candidate) && CertificationValidation.Generation(admission.GenerationSettings) &&
            admission.CorpusManifest is not null && admission.CaseIds is { Length: > 0 and <= 200 } &&
            admission.CaseIds.All(Id) && admission.CaseIds.SequenceEqual(admission.CaseIds.Distinct().Order(StringComparer.Ordinal)) &&
            admission.RepeatCount == 5 && admission.StandardDeviationMultiplier == 2d &&
            admission.NoisePolicyIdentitySha256 == MeasurementNoisePolicyConfig.Canonical.IdentitySha256 &&
            admission.MetricProfileId == CodeDocumentationCertificationAdmission.Metric &&
            admission.TargetPolicyId == CodeDocumentationCertificationAdmission.TargetPolicy && admission.Target == .95d &&
            admission.ExpectedEvaluationCallCount == checked(admission.CaseIds.Length * admission.RepeatCount) &&
            value.MaximumEvaluationCalls >= admission.ExpectedEvaluationCallCount &&
            Hex(admission.GenerationSettingsSha256) && admission.Evaluator is not null, "HISTORY_MISMATCH");
        Check(value.ExposureKeys is { Length: > 0 and <= 10200 } &&
            value.ExposureKeys.All(k => k is not null && k.Kind is "source" or "content" && ShaReference(k.Digest)) &&
            value.ExposureKeys.SequenceEqual(value.ExposureKeys.Distinct().OrderBy(k => k.Kind, StringComparer.Ordinal).ThenBy(k => k.Digest, StringComparer.Ordinal)) &&
            value.ExposureKeys.Count(k => k.Kind == "source") is > 0 and <= 10000 &&
            value.ExposureKeys.Count(k => k.Kind == "content") == admission.CaseIds.Length, "HISTORY_MISMATCH");
    }

    private static void ValidateReceipt(BoundAdmission admission, ReceiptData receipt)
    {
        Check(receipt is not null && receipt.Representative is not null && receipt.Observations is not null && receipt.MeasurementNoisePassport is not null,
            "COMPLETION_INVALID");
        var rep = receipt.Representative;
        var candidate = admission.Candidate;
        var corpus = admission.CorpusManifest;
        var evaluator = admission.Evaluator.AsReceipt();
        var h = admission.CaseIds.Length;
        var expectedSet = CodeDocumentationSavedModelAssessmentCalibrationService.BuildEvaluationSetId(
            candidate.DomainSlug, candidate.DesignPatternSlug, corpus.CorpusVersion, corpus.ManifestSha256, corpus.CorpusSha256,
            admission.GenerationSettingsSha256, evaluator, admission.NoisePolicyIdentitySha256);
        var expectedScorer = new HeldOutScorerIdentity(candidate.DomainSlug, evaluator.ExpectedModelId,
            evaluator.ExpectedOllamaManifestDigest, evaluator.EvaluationProfileId, admission.MetricProfileId);
        Check(Id(receipt.CalibrationRunId) && receipt.EvaluationSetId == expectedSet && Equal(receipt.ScorerIdentity, expectedScorer) &&
            receipt.NoisePolicyIdentitySha256 == admission.NoisePolicyIdentitySha256 && receipt.RepeatCount == 5 &&
            receipt.HeldOutCaseCount == h && receipt.ExpectedEvaluationCallCount == admission.ExpectedEvaluationCallCount &&
            receipt.ObservedEvaluationCallCount == admission.ExpectedEvaluationCallCount && receipt.Observations.Length == 5,
            "COMPLETION_INVALID");
        Check(rep.DomainSlug == candidate.DomainSlug && rep.DesignPatternSlug == candidate.DesignPatternSlug &&
            rep.CorpusVersion == corpus.CorpusVersion && rep.CorpusManifestSha256 == corpus.ManifestSha256 && rep.CorpusSha256 == corpus.CorpusSha256 &&
            Equal(rep.GenerationSettings, admission.GenerationSettings) && rep.GenerationSettingsSha256 == admission.GenerationSettingsSha256 &&
            rep.ModelSourceKind == candidate.ModelSourceKind && rep.ModelManifestSha256 == candidate.ManifestSha256 && rep.SavedWeightsSha256 == candidate.WeightsSha256 &&
            rep.RepositoryRelativeModelIdentityPath == (candidate.RepositoryRelativeModelIdentityPath.Length == 0 ? candidate.RepositoryRelativeManifestPath : candidate.RepositoryRelativeModelIdentityPath) &&
            rep.ModelIdentitySha256 == (candidate.ModelIdentitySha256.Length == 0 ? candidate.ManifestSha256 : candidate.ModelIdentitySha256) &&
            rep.ModelSerializationBeforeByteLength > 0 && rep.ModelSerializationBeforeByteLength <= candidate.MaximumModelSerializationBytes &&
            rep.ModelSerializationBeforeByteLength == rep.ModelSerializationAfterByteLength && Hex(rep.ModelSerializationBeforeSha256) &&
            rep.ModelSerializationBeforeSha256 == rep.ModelSerializationAfterSha256 && SameEvaluator(rep.Evaluator, evaluator) &&
            rep.Cases is not null && rep.Cases.Select(c => c.CaseId).SequenceEqual(admission.CaseIds) &&
            rep.Cases.All(c => Hex(c.StudentAnswerSha256)), "COMPLETION_INVALID");
        var runs = new HashSet<string>(StringComparer.Ordinal);
        var requests = new HashSet<string>(StringComparer.Ordinal);
        var scores = new List<double>();
        foreach (var observation in receipt.Observations)
        {
            Check(observation is not null && Id(observation.AssessmentRunId) && runs.Add(observation.AssessmentRunId) &&
                observation.EvaluatedCaseCount == h && observation.PassedCaseCount >= 0 && observation.PassedCaseCount <= h &&
                double.IsFinite(observation.CorrectFraction) && observation.CorrectFraction == (double)observation.PassedCaseCount / h &&
                observation.GenerationSettingsSha256 == admission.GenerationSettingsSha256 &&
                observation.ModelSerializationBeforeSha256 == rep.ModelSerializationBeforeSha256 &&
                observation.ModelSerializationAfterSha256 == rep.ModelSerializationBeforeSha256 &&
                observation.CaseIds is not null && observation.CaseIds.SequenceEqual(admission.CaseIds) &&
                observation.StudentAnswerSha256 is not null && observation.StudentAnswerSha256.Count == h && observation.StudentAnswerSha256.All(Hex) &&
                observation.ObservedEvaluationRequestIds is not null && observation.ObservedEvaluationRequestIds.Count == h &&
                observation.ObservedEvaluationRequestIds.All(id => Id(id) && requests.Add(id)), "COMPLETION_INVALID");
            scores.Add((double)observation.PassedCaseCount / h);
        }
        var first = receipt.Observations[0];
        Check(requests.Count == admission.ExpectedEvaluationCallCount && rep.Cases.Count(c => c.Passed) == first.PassedCaseCount &&
            rep.Cases.Select(c => c.StudentAnswerSha256).SequenceEqual(first.StudentAnswerSha256) &&
            rep.Evaluator.ObservedEvaluationRequestIds.SequenceEqual(first.ObservedEvaluationRequestIds), "COMPLETION_INVALID");
        var p = receipt.MeasurementNoisePassport;
        var passport = new MeasurementNoisePassport(p.ScorerIdentity, p.EvaluationSetId, p.ScoredRows, p.RepeatCount,
            p.StandardDeviationMultiplier, p.RepeatedScores, p.MeanScore, p.SampleStandardDeviation, p.BinomialLowerBound, p.NoiseFloor, p.CalibratedAtUtc);
        Check(MeasurementNoisePolicy.TryValidatePassport(passport, out _) && Equal(p.ScorerIdentity, expectedScorer) &&
            p.EvaluationSetId == expectedSet && p.ScoredRows == h && p.RepeatCount == 5 && p.StandardDeviationMultiplier == 2 &&
            p.RepeatedScores.SequenceEqual(scores) && Utc(p.CalibratedAtUtc), "COMPLETION_INVALID");
        var recomputed = MeasurementNoisePolicy.BuildPassport(expectedScorer, expectedSet, h, scores,
            MeasurementNoisePolicyConfig.Canonical, p.CalibratedAtUtc);
        Check(p.MeanScore == recomputed.MeanScore && p.SampleStandardDeviation == recomputed.SampleStandardDeviation &&
            p.BinomialLowerBound == recomputed.BinomialLowerBound && p.NoiseFloor == recomputed.NoiseFloor, "COMPLETION_INVALID");
    }

    private static bool SameEvaluator(CodeDocumentationSavedModelAssessmentEvaluatorReceipt a, CodeDocumentationSavedModelAssessmentEvaluatorReceipt b)
        => a is not null && a.ExpectedModelId == b.ExpectedModelId && a.ExpectedOllamaManifestDigest == b.ExpectedOllamaManifestDigest &&
            a.ExpectedOllamaModelLayerDigest == b.ExpectedOllamaModelLayerDigest && a.EvaluationProfileId == b.EvaluationProfileId &&
            a.EvaluationProfileConfigurationSha256 == b.EvaluationProfileConfigurationSha256;

    private static byte[] Envelope<T>(T payload, string schema = EventSchema)
    {
        var element = JsonSerializer.SerializeToElement(payload, Json);
        var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(element, Json);
        return JsonSerializer.SerializeToUtf8Bytes(new EnvelopeData(schema, payloadBytes.Length, Hash(payloadBytes), element), Json);
    }

    private static T ParseEnvelope<T>(byte[] bytes, string schema = EventSchema)
    {
        Check(bytes.Length is > 0 and <= MaximumEventBytes, "LIMIT_EXCEEDED");
        // Strict UTF8 + duplicate detection precede deserialization; canonical round-trip
        // additionally rejects missing/defaulted fields, alternate order and trailing content.
        Utf8.GetString(bytes);
        using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 16 });
        ValidateJson(document.RootElement);
        var envelope = JsonSerializer.Deserialize<EnvelopeData>(bytes, Json);
        Check(envelope is not null && envelope.Schema == schema && Hex(envelope.PayloadSha256), "HISTORY_MISMATCH");
        var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(envelope!.Payload, Json);
        Check(envelope.PayloadByteLength == payloadBytes.Length && envelope.PayloadSha256 == Hash(payloadBytes), "HISTORY_MISMATCH");
        var value = envelope.Payload.Deserialize<T>(Json);
        Check(value is not null && Envelope(value, schema).SequenceEqual(bytes), "HISTORY_MISMATCH");
        return value!;
    }

    private static void ValidateJson(JsonElement element, string? field = null)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var fields = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            { Check(fields.Add(property.Name), "HISTORY_MISMATCH"); ValidateJson(property.Value, property.Name); }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            var maximum = field switch
            {
                "observations" or "repeatedScores" => 5,
                "caseIds" or "cases" or "studentAnswerSha256" or "observedEvaluationRequestIds" => 200,
                "exposureKeys" => 10200,
                "materialExposureKeys" => MaximumMaterialExposureKeys,
                _ => 10200
            };
            Check(element.GetArrayLength() <= maximum, "LIMIT_EXCEEDED");
            foreach (var item in element.EnumerateArray()) ValidateJson(item);
        }
    }

    private static bool Equal<T>(T a, T b) => JsonSerializer.SerializeToUtf8Bytes(a, Json).SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(b, Json));
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static bool Hex(string? value) => value is { Length: 64 } && value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');
    private static bool ShaReference(string? value) => value is { Length: 71 } && value.StartsWith("sha256:", StringComparison.Ordinal) && Hex(value[7..]);
    private static bool Id(string? value) => !string.IsNullOrWhiteSpace(value) && Utf8.GetByteCount(value) <= 1024 && !value.Any(char.IsControl);
    private static bool Utc(DateTimeOffset value) => value != default && value.Offset == TimeSpan.Zero;
    private static string EventPath(string sessionId, string kind)
        => kind == "material-exposure"
            ? "exposures/" + sessionId + "/exposure.json"
            : "sessions/" + sessionId + "/" + kind + ".json";
    private static void Hit(string name) => FaultInjection.Value?.Invoke(name);
    private static void ObserveFailure(string boundary, Exception exception, int? pathLength = null)
    {
        var cachedError = System.Runtime.InteropServices.Marshal.GetLastPInvokeError();
        var observer = FailureDiagnostic.Value;
        if (observer is null) return;
        try
        {
            var member = exception.TargetSite;
            observer(new(boundary, exception.GetType().FullName ?? exception.GetType().Name,
                exception.HResult, cachedError, pathLength,
                member is null ? "unavailable" : member.DeclaringType?.FullName + "." + member.Name));
        }
        catch (Exception diagnosticException) when (SnapshotExceptionGraphClassifier.IsNonFatal(diagnosticException))
        { /* A diagnostic observer cannot replace the original failure. */ }
    }
    private static void Check(bool condition, string suffix) { if (!condition) throw new SessionFailure(suffix); }
    private sealed class SessionFailure(string suffix) : Exception { internal string Suffix { get; } = suffix; }
    private static OperationResult<T> Result<T>(Func<T> action,
        [System.Runtime.CompilerServices.CallerMemberName] string operation = "")
    {
        try { return OperationResult<T>.Ok(action()); }
        catch (SessionFailure failure)
        { ObserveFailure(operation + ":" + failure.Suffix, failure); return Failure<T>(failure.Suffix); }
        catch (JsonException exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        { ObserveFailure(operation, exception); return Failure<T>("HISTORY_MISMATCH"); }
        catch (DecoderFallbackException exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        { ObserveFailure(operation, exception); return Failure<T>("HISTORY_MISMATCH"); }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        { ObserveFailure(operation, exception); return Failure<T>("STORAGE_UNAVAILABLE"); }
    }
    private static OperationResult<T> Failure<T>(string suffix)
        => OperationResult<T>.Fail("Состояние независимого экзамена не прошло проверку; новая попытка не разрешена.", ErrorPrefix + suffix);

    private sealed record EnvelopeData(string Schema, int PayloadByteLength, string PayloadSha256, JsonElement Payload);
    private sealed record StoreEvent(string Kind, int FormatVersion, string StoreId, string CanonicalRepositoryRoot, string PolicyId);
    private sealed record InitializationMarker(string Schema, string Repository, string StoreId);
    private sealed record Genesis(string Kind, int FormatVersion, string StoreId, string CanonicalRepositoryRoot, string PolicyId,
        int Sequence, string PreviousTipSha256, string EventPath, int EventByteLength, string EventSha256);
    private sealed record HistoryCommit(string Kind, int FormatVersion, string StoreId, int Sequence, string PreviousTipSha256,
        string SessionId, string EventKind, string EventPath, int EventByteLength, string EventSha256);
    private sealed record EventBinding(int ByteLength, string Sha256);
    internal sealed record ExposureKey(string Kind, string Digest);
    private sealed record MaterialExposureIdentity(string Schema, string StoreId, string Role,
        string SourceBindingIdentitySha256, string? DiagnosticCorpusManifestSha256, ExposureKey[] MaterialExposureKeys);
    private sealed record MaterialExposureEvent(string Kind, int FormatVersion, string StoreId,
        string ExposureIdentitySha256, string Role, string SourceBindingIdentitySha256,
        string? DiagnosticCorpusManifestSha256, ExposureKey[] MaterialExposureKeys, DateTimeOffset FirstRecordedAtUtc);
    private sealed record ReservationEvent(string Kind, int FormatVersion, string StoreId, string SessionId, string PolicyId,
        string DiagnosticRunId, DateTimeOffset ReservedAtUtc, BoundAdmission Admission, int MaximumEvaluationCalls, ExposureKey[] ExposureKeys);
    private sealed record StartedEvent(string Kind, int FormatVersion, string StoreId, string SessionId,
        string ReservationEnvelopeSha256, DateTimeOffset StartedAtUtc);
    private sealed record CompletedEvent(string Kind, int FormatVersion, string StoreId, string SessionId,
        string ReservationEnvelopeSha256, string StartedEnvelopeSha256, DateTimeOffset CompletedAtUtc, ReceiptData Receipt);
    private sealed record SessionData(ReservationEvent Reservation, StartedEvent? Start, CompletedEvent? Completion);
    private sealed record History(CertificationHistorySnapshot Anchor, Dictionary<string, SessionData> Sessions,
        Dictionary<string, MaterialExposureEvent> Exposures);

    private sealed record BoundAdmission(string Schema, string Purpose, string ScopeId, string ScopeVersion, DateTimeOffset CoverageCutUtc,
        string ScopeIdentitySha256, string ExposureIndexIdentitySha256, string ExposureIndexVersion, string EvaluationConditionsIdentitySha256,
        string AdmissionIdentitySha256, CodeDocumentationSavedModelAssessmentModelPassport Candidate,
        RepositoryOwnedReferenceCorpusManifest CorpusManifest, string[] CaseIds,
        CodeDocumentationSavedModelAssessmentGenerationSettings GenerationSettings, string GenerationSettingsSha256,
        ExpectedEvaluatorData Evaluator, string NoisePolicyIdentitySha256, int RepeatCount,
        double StandardDeviationMultiplier, string MetricProfileId, string TargetPolicyId, double Target, int ExpectedEvaluationCallCount)
    {
        internal static BoundAdmission Capture(CodeDocumentationCertificationAdmission a) => new(a.Schema, a.Purpose, a.ScopeId, a.ScopeVersion,
            a.CoverageCutUtc, a.ScopeIdentitySha256, a.ExposureIndexIdentitySha256, a.ExposureIndexVersion, a.EvaluationConditionsIdentitySha256,
            a.AdmissionIdentitySha256, a.Candidate, a.CorpusManifest, BoundedSnapshot(a.CaseIds, 200, "LIMIT_EXCEEDED"), a.GenerationSettings, a.GenerationSettingsSha256,
            ExpectedEvaluatorData.Capture(a.Evaluator), a.NoisePolicyIdentitySha256, a.RepeatCount, a.StandardDeviationMultiplier, a.MetricProfileId, a.TargetPolicyId, a.Target,
            a.ExpectedEvaluationCallCount);
    }

    private sealed record ExpectedEvaluatorData(string ExpectedModelId, string ExpectedOllamaManifestDigest, string ExpectedOllamaModelLayerDigest,
        string EvaluationProfileId, string EvaluationProfileConfigurationSha256)
    {
        internal static ExpectedEvaluatorData Capture(CodeDocumentationSavedModelAssessmentEvaluatorReceipt e)
            => new(e.ExpectedModelId, e.ExpectedOllamaManifestDigest, e.ExpectedOllamaModelLayerDigest, e.EvaluationProfileId, e.EvaluationProfileConfigurationSha256);
        internal CodeDocumentationSavedModelAssessmentEvaluatorReceipt AsReceipt()
            => new(ExpectedModelId, ExpectedOllamaManifestDigest, ExpectedOllamaModelLayerDigest, EvaluationProfileId, EvaluationProfileConfigurationSha256, Array.Empty<string>());
    }

    internal sealed record CaseData(string CaseId, bool Passed, string StudentAnswerSha256);
    internal sealed record RepresentativeData(string DomainSlug, string DesignPatternSlug, string CorpusVersion, string CorpusManifestSha256,
        string CorpusSha256, CodeDocumentationSavedModelAssessmentGenerationSettings GenerationSettings, string GenerationSettingsSha256,
        string ModelSourceKind, string RepositoryRelativeModelIdentityPath, string ModelIdentitySha256, string ModelManifestSha256,
        string SavedWeightsSha256, string ModelSerializationBeforeSha256, long ModelSerializationBeforeByteLength,
        string ModelSerializationAfterSha256, long ModelSerializationAfterByteLength, CodeDocumentationSavedModelAssessmentEvaluatorReceipt Evaluator,
        IReadOnlyList<CaseData> Cases);
    internal sealed record NoiseData(HeldOutScorerIdentity ScorerIdentity, string EvaluationSetId, int ScoredRows, int RepeatCount,
        double StandardDeviationMultiplier, IReadOnlyList<double> RepeatedScores, double MeanScore, double SampleStandardDeviation,
        double BinomialLowerBound, double NoiseFloor, DateTimeOffset CalibratedAtUtc);
    internal sealed record ReceiptData(string CalibrationRunId, string EvaluationSetId, HeldOutScorerIdentity ScorerIdentity,
        string NoisePolicyIdentitySha256, int RepeatCount, int HeldOutCaseCount, int ExpectedEvaluationCallCount, int ObservedEvaluationCallCount,
        RepresentativeData Representative, CodeDocumentationSavedModelAssessmentCalibrationObservation[] Observations, NoiseData MeasurementNoisePassport)
    {
        internal static ReceiptData Capture(CodeDocumentationSavedModelAssessmentCalibrationResult result)
        {
            var r = result.RepresentativeAssessment;
            var c = result.Calibration;
            var p = c.MeasurementNoisePassport;
            var cases = BoundedSnapshot(r.Cases, 200, "COMPLETION_INVALID");
            var observations = BoundedSnapshot(c.Observations, 5, "COMPLETION_INVALID");
            var scores = BoundedSnapshot(p.RepeatedScores, 5, "COMPLETION_INVALID");
            Check(r.EvaluatedCaseCount == cases.Length && r.PassedCaseCount == cases.Count(item => item.Passed) &&
                double.IsFinite(r.CorrectFraction) && r.CorrectFraction == (double)r.PassedCaseCount / r.EvaluatedCaseCount &&
                observations.Length == 5 && scores.Length == 5 && r.AssessmentRunId == observations[0].AssessmentRunId, "COMPLETION_INVALID");
            return new(c.CalibrationRunId, c.EvaluationSetId, c.ScorerIdentity, c.NoisePolicyIdentitySha256, c.RepeatCount, c.HeldOutCaseCount,
                c.ExpectedEvaluationCallCount, c.ObservedEvaluationCallCount,
                new(r.DomainSlug, r.DesignPatternSlug, r.CorpusVersion, r.CorpusManifestSha256, r.CorpusSha256,
                    r.GenerationSettings, r.GenerationSettingsSha256, r.ModelSourceKind, r.RepositoryRelativeModelIdentityPath,
                    r.ModelIdentitySha256, r.ModelManifestSha256, r.SavedWeightsSha256, r.ModelSerializationBeforeSha256,
                    r.ModelSerializationBeforeByteLength, r.ModelSerializationAfterSha256, r.ModelSerializationAfterByteLength,
                    CopyEvaluator(r.Evaluator), Array.AsReadOnly(cases.Select(item => new CaseData(item.CaseId, item.Passed, item.StudentAnswerSha256)).ToArray())),
                observations.Select(CopyObservation).ToArray(),
                new(p.ScorerIdentity, p.EvaluationSetId, p.ScoredRows, p.RepeatCount, p.StandardDeviationMultiplier,
                    Array.AsReadOnly(scores), p.MeanScore, p.SampleStandardDeviation, p.BinomialLowerBound, p.NoiseFloor, p.CalibratedAtUtc));
        }
    }

    internal static CodeDocumentationSavedModelAssessmentEvaluatorReceipt CopyEvaluator(CodeDocumentationSavedModelAssessmentEvaluatorReceipt r)
        => r with { ObservedEvaluationRequestIds = Array.AsReadOnly(BoundedSnapshot(r.ObservedEvaluationRequestIds, 200, "COMPLETION_INVALID")) };
    internal static CodeDocumentationSavedModelAssessmentCalibrationObservation CopyObservation(CodeDocumentationSavedModelAssessmentCalibrationObservation o)
        => o with { CaseIds = Array.AsReadOnly(BoundedSnapshot(o.CaseIds, 200, "COMPLETION_INVALID")),
            StudentAnswerSha256 = Array.AsReadOnly(BoundedSnapshot(o.StudentAnswerSha256, 200, "COMPLETION_INVALID")),
            ObservedEvaluationRequestIds = Array.AsReadOnly(BoundedSnapshot(o.ObservedEvaluationRequestIds, 200, "COMPLETION_INVALID")) };

    private static T[] BoundedSnapshot<T>(IEnumerable<T>? source, int maximum, string error)
    {
        Check(source is not null, error);
        var declared = source is IReadOnlyCollection<T> collection ? collection.Count : (int?)null;
        Check(declared is null || declared >= 0 && declared <= maximum, error);
        var copied = new List<T>();
        var iterator = source!.GetEnumerator();
        var success = false;
        try
        {
            while (iterator.MoveNext())
            {
                Check(copied.Count < maximum, error);
                copied.Add(iterator.Current);
            }
            Check(declared is null || declared == copied.Count, error);
            success = true;
            return copied.ToArray();
        }
        finally
        {
            try { iterator.Dispose(); }
            catch (Exception exception) when (!success && SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { }
        }
    }
}

internal sealed class CodeDocumentationCertificationSessionReservation : IDisposable
{
    private readonly CodeDocumentationCertificationSessionStore _owner;
    private CodeDocumentationCertificationSessionReservation(CodeDocumentationCertificationSessionStore owner, string sessionId,
        string diagnosticRunId, DateTimeOffset reservedAtUtc, CodeDocumentationCertificationSessionReceipt? receipt, bool isReplay)
    { _owner = owner; SessionId = sessionId; DiagnosticRunId = diagnosticRunId; ReservedAtUtc = reservedAtUtc; Receipt = receipt; IsReplay = isReplay; }
    internal static CodeDocumentationCertificationSessionReservation Mint(CodeDocumentationCertificationSessionStore owner, string id,
        string run, DateTimeOffset at, CodeDocumentationCertificationSessionReceipt? receipt, bool isReplay) => new(owner, id, run, at, receipt, isReplay);
    internal string SessionId { get; }
    internal string DiagnosticRunId { get; }
    internal DateTimeOffset ReservedAtUtc { get; }
    internal bool IsReplay { get; }
    internal CodeDocumentationCertificationSessionReceipt? Receipt { get; }
    public void Dispose() => _owner.Release(this);
}

internal sealed class CodeDocumentationCertificationSessionStartPermit
{
    private CodeDocumentationCertificationSessionStartPermit() { }
    internal static CodeDocumentationCertificationSessionStartPermit Mint() => new();
}

/// <summary>Только числовые/идентификационные копии, без закрытых текстов и writable passport arrays.</summary>
internal sealed class CodeDocumentationCertificationSessionReceipt
{
    internal CodeDocumentationCertificationSessionReceipt(CodeDocumentationCertificationSessionStore.ReceiptData value,
        string admissionIdentitySha256, string receiptIdentitySha256)
    {
        AdmissionIdentitySha256 = admissionIdentitySha256; ReceiptIdentitySha256 = receiptIdentitySha256;
        CalibrationRunId = value.CalibrationRunId; EvaluationSetId = value.EvaluationSetId; ScorerIdentity = value.ScorerIdentity;
        NoisePolicyIdentitySha256 = value.NoisePolicyIdentitySha256; RepeatCount = value.RepeatCount; HeldOutCaseCount = value.HeldOutCaseCount;
        ExpectedEvaluationCallCount = value.ExpectedEvaluationCallCount; ObservedEvaluationCallCount = value.ObservedEvaluationCallCount;
        Representative = value.Representative with { Evaluator = CodeDocumentationCertificationSessionStore.CopyEvaluator(value.Representative.Evaluator),
            Cases = Array.AsReadOnly(value.Representative.Cases.Select(c => c with { }).ToArray()) };
        Observations = Array.AsReadOnly(value.Observations.Select(CodeDocumentationCertificationSessionStore.CopyObservation).ToArray());
        MeasurementNoisePassport = value.MeasurementNoisePassport with { RepeatedScores = Array.AsReadOnly(value.MeasurementNoisePassport.RepeatedScores.ToArray()) };
    }
    public string AdmissionIdentitySha256 { get; }
    public string ReceiptIdentitySha256 { get; }
    public string CalibrationRunId { get; }
    public string EvaluationSetId { get; }
    public HeldOutScorerIdentity ScorerIdentity { get; }
    public string NoisePolicyIdentitySha256 { get; }
    public int RepeatCount { get; }
    public int HeldOutCaseCount { get; }
    public int ExpectedEvaluationCallCount { get; }
    public int ObservedEvaluationCallCount { get; }
    public CodeDocumentationCertificationSessionStore.RepresentativeData Representative { get; }
    public IReadOnlyList<CodeDocumentationSavedModelAssessmentCalibrationObservation> Observations { get; }
    public CodeDocumentationCertificationSessionStore.NoiseData MeasurementNoisePassport { get; }
}
