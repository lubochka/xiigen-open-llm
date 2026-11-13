// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using Tokenizer;

/// <summary>
/// Server-owned request for the final independent CodeDocumentation
/// certification exam. The caller provides already-owned process objects and a
/// server-issued certification scope; no HTTP caller value can name the model,
/// corpus, evaluator, session store, or checkpoint identity directly.
/// </summary>
internal sealed record CodeDocumentationCertificationServiceRequest(
    CodeDocumentationRepositoryOwnedModelCheckpointStartRequest StartRequest,
    ServerOwnedCodeDocumentationCertificationScope Authority,
    string ExpectedStoreId,
    BpeTokenizer Tokenizer,
    Func<OperationResult<ILegalTeacherEvaluationInferenceClient>>
        CreateEvaluationClient,
    string CertificationRunId,
    DateTimeOffset ObservedAtUtc,
    IReadOnlyList<string>? EvidenceRefs = null,
    TimeSpan? MaximumDuration = null);

/// <summary>
/// Redacted final-certification result. A completed negative exam is durable
/// and successful as an execution result, but Certified remains false until the
/// target predicate is proven beyond measurement noise.
/// </summary>
internal sealed record CodeDocumentationCertificationServiceResult(
    bool Certified,
    bool Replayed,
    string SessionId,
    string AdmissionIdentitySha256,
    string ReceiptIdentitySha256,
    MeasurementNoiseComparison TargetComparison,
    CodeDocumentationCertificationSessionReceipt Receipt);

/// <summary>
/// Thin TrainableRouter orchestration for S03.10 final certification. Storage,
/// admission, calibration, receipt validation, model loading and checkpoint
/// hashing remain delegated to their existing owners.
/// </summary>
internal sealed class CodeDocumentationCertificationService
{
    public const string InvalidErrorCode =
        "CODE_DOCUMENTATION_CERTIFICATION_INVALID";
    public const string AdmissionRejectedErrorCode =
        "CODE_DOCUMENTATION_CERTIFICATION_ADMISSION_REJECTED";
    public const string ParentCheckpointRequiredErrorCode =
        "CODE_DOCUMENTATION_CERTIFICATION_PARENT_CHECKPOINT_REQUIRED";
    public const string CheckpointIdentityMismatchErrorCode =
        "CODE_DOCUMENTATION_CERTIFICATION_CHECKPOINT_IDENTITY_MISMATCH";
    public const string SavedWeightsChangedErrorCode =
        "CODE_DOCUMENTATION_CERTIFICATION_SAVED_WEIGHTS_CHANGED";
    public const string CalibrationFailedErrorCode =
        "CODE_DOCUMENTATION_CERTIFICATION_CALIBRATION_FAILED";
    public const string CompletionRejectedErrorCode =
        "CODE_DOCUMENTATION_CERTIFICATION_COMPLETION_REJECTED";
    public const string EvaluationClientUnavailableErrorCode =
        "CODE_DOCUMENTATION_CERTIFICATION_EVALUATOR_UNAVAILABLE";
    public const string CleanupFailedErrorCode =
        "CODE_DOCUMENTATION_CERTIFICATION_CLEANUP_FAILED";
    public const string CancelledErrorCode =
        "CODE_DOCUMENTATION_CERTIFICATION_CANCELLED";

    private const int MaximumAssessmentRunIdLength = 256;
    private const int CalibrationRunIdSuffixLength = 11;
    private const int MaximumRunIdLength =
        MaximumAssessmentRunIdLength - CalibrationRunIdSuffixLength;
    private const int MaximumRunIdUtf8Bytes = 1024;
    private const int MaximumRequestEvidenceRefs = 64;
    private const int MaximumAssessmentEvidenceRefs = 128;
    private const int MaximumEvidenceRefLength = 512;
    private static readonly TimeSpan MaximumCalibrationDuration =
        TimeSpan.FromMilliseconds(int.MaxValue);

    public async Task<OperationResult<CodeDocumentationCertificationServiceResult>>
        CertifyAsync(
            CodeDocumentationCertificationServiceRequest? request,
            CancellationToken cancellationToken = default)
    {
        var cleanupStarted = false;
        try
        {
            var validation = ValidateRequest(request);
            if (!validation.IsSuccess)
                return validation.MapError<CodeDocumentationCertificationServiceResult>();

            cancellationToken.ThrowIfCancellationRequested();
            var safeRequest = request!;

            var admitted = CodeDocumentationCertificationAdmission.TryCreate(
                safeRequest.Authority,
                safeRequest.Authority.Candidate,
                safeRequest.Authority.Corpus,
                safeRequest.Authority.Index);
            if (!admitted.IsSuccess || admitted.Value is null)
            {
                return Failure(
                    "The server-owned certification scope was not admitted as an independent clean final exam.",
                    AdmissionRejectedErrorCode);
            }

            var started = PrepareExactCurrentCheckpoint(
                safeRequest,
                cancellationToken);
            if (!started.IsSuccess || started.Value is null)
                return started.MapError<CodeDocumentationCertificationServiceResult>();

            var beforeWeights = CaptureSavedWeights(
                safeRequest.StartRequest.RepositoryRoot,
                started.Value.ParentManifest!.RepositoryRelativeWeightsPath,
                started.Value.ParentManifest.WeightsSha256,
                started.Value.ParentManifest.WeightsByteLength,
                safeRequest.StartRequest.MaximumCheckpointBytes);
            if (!beforeWeights.IsSuccess || beforeWeights.Value is null)
                return beforeWeights.MapError<CodeDocumentationCertificationServiceResult>();

            var plan =
                CodeDocumentationSavedModelAssessmentCalibrationService
                    .PlanCalibration(
                        safeRequest.Authority.Corpus.Corpus,
                        safeRequest.Authority.Generation,
                        safeRequest.Authority.Evaluator,
                        safeRequest.Authority.Noise);
            if (!plan.IsSuccess || plan.Value is null ||
                plan.Value.ExpectedEvaluationCallCount !=
                admitted.Value.ExpectedEvaluationCallCount)
            {
                return Failure(
                    "The certification calibration plan no longer matches the admitted final-exam conditions.",
                    plan.ErrorCode is { Length: > 0 }
                        ? plan.ErrorCode
                        : CalibrationFailedErrorCode);
            }

            var evidenceRefs = BuildEvidenceRefs(
                safeRequest,
                started.Value.EvidenceRefs);
            if (!evidenceRefs.IsSuccess || evidenceRefs.Value is null)
                return evidenceRefs.MapError<CodeDocumentationCertificationServiceResult>();

            var storeResult = OpenOrCreateStore(
                safeRequest.StartRequest.RepositoryRoot,
                safeRequest.ExpectedStoreId);
            if (!storeResult.IsSuccess || storeResult.Value is null)
                return storeResult.MapError<CodeDocumentationCertificationServiceResult>();

            var store = storeResult.Value;
            CodeDocumentationCertificationSessionReservation? reservation = null;
            OperationResult<CodeDocumentationCertificationServiceResult>? primaryResult = null;
            ExceptionDispatchInfo? primaryException = null;
            try
            {
                var reservationResult = store.ReserveOrReplay(
                    safeRequest.Authority,
                    safeRequest.CertificationRunId,
                    cancellationToken);
                if (!reservationResult.IsSuccess || reservationResult.Value is null)
                {
                    primaryResult = reservationResult.MapError<
                        CodeDocumentationCertificationServiceResult>();
                }
                else
                {
                    reservation = reservationResult.Value;
                    primaryResult = await ExecuteReservedAsync(
                            safeRequest,
                            started.Value,
                            beforeWeights.Value,
                            evidenceRefs.Value,
                            store,
                            reservation,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                primaryResult = Failure(
                    "The independent CodeDocumentation certification exam was cancelled before a completed receipt could be issued.",
                    CancelledErrorCode);
            }
            catch (Exception exception)
            {
                primaryException = ExceptionDispatchInfo.Capture(exception);
            }

            cleanupStarted = true;
            var cleanupException = DisposeResources(reservation, store);
            return ReconcileOutcome(
                primaryResult,
                primaryException,
                cleanupException);
        }
        catch (OperationCanceledException)
            when (!cleanupStarted && cancellationToken.IsCancellationRequested)
        {
            return Failure(
                "The independent CodeDocumentation certification exam was cancelled before a completed receipt could be issued.",
                CancelledErrorCode);
        }
    }

    private static async Task<
        OperationResult<CodeDocumentationCertificationServiceResult>>
        ExecuteReservedAsync(
            CodeDocumentationCertificationServiceRequest request,
            CodeDocumentationRepositoryOwnedModelCheckpointStart started,
            SavedWeightsIdentity beforeWeights,
            IReadOnlyList<string> evidenceRefs,
            CodeDocumentationCertificationSessionStore store,
            CodeDocumentationCertificationSessionReservation reservation,
            CancellationToken cancellationToken)
    {
        if (reservation.IsReplay)
        {
            if (reservation.Receipt is null)
            {
                return Failure(
                    "The certification session replay did not include a completed receipt.",
                    CompletionRejectedErrorCode);
            }

            return OperationResult<CodeDocumentationCertificationServiceResult>
                .Ok(ProjectResult(
                    reservation.SessionId,
                    reservation.Receipt,
                    replayed: true));
        }

        var startPermit = store.BeginEvaluation(
            reservation,
            cancellationToken);
        if (!startPermit.IsSuccess || startPermit.Value is null)
            return startPermit.MapError<CodeDocumentationCertificationServiceResult>();

        cancellationToken.ThrowIfCancellationRequested();

        OperationResult<ILegalTeacherEvaluationInferenceClient> evaluationClient;
        try
        {
            evaluationClient = request.CreateEvaluationClient();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Failure(
                "The independent certification evaluator could not be acquired after the durable session started.",
                EvaluationClientUnavailableErrorCode);
        }

        if (!evaluationClient.IsSuccess || evaluationClient.Value is null)
        {
            return Failure(
                "The independent certification evaluator could not be acquired after the durable session started.",
                EvaluationClientUnavailableErrorCode);
        }

        var assessmentRequest =
            new CodeDocumentationSavedModelAssessmentRequest(
                started.Model,
                request.Tokenizer,
                request.Authority.Candidate,
                request.Authority.Corpus.Corpus,
                request.Authority.Corpus.Manifest,
                evaluationClient.Value,
                request.Authority.Evaluator,
                request.Authority.Generation,
                request.CertificationRunId,
                request.ObservedAtUtc,
                evidenceRefs);

        var calibrated =
            await new CodeDocumentationSavedModelAssessmentCalibrationService()
                .CalibrateAsync(
                    assessmentRequest,
                    request.Authority.Noise,
                    request.MaximumDuration,
                    cancellationToken)
                .ConfigureAwait(false);
        if (!calibrated.IsSuccess || calibrated.Value is null)
        {
            return Failure(
                calibrated.ErrorMessage is { Length: > 0 }
                    ? calibrated.ErrorMessage
                    : "The independent certification calibration did not complete.",
                calibrated.ErrorCode is { Length: > 0 }
                    ? calibrated.ErrorCode
                    : CalibrationFailedErrorCode);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var afterWeights = CaptureSavedWeights(
            request.StartRequest.RepositoryRoot,
            started.ParentManifest!.RepositoryRelativeWeightsPath,
            started.ParentManifest.WeightsSha256,
            started.ParentManifest.WeightsByteLength,
            request.StartRequest.MaximumCheckpointBytes);
        if (!afterWeights.IsSuccess || afterWeights.Value is null)
            return afterWeights.MapError<CodeDocumentationCertificationServiceResult>();
        if (afterWeights.Value != beforeWeights)
        {
            return Failure(
                "The accepted saved checkpoint bytes changed during final certification.",
                SavedWeightsChangedErrorCode);
        }

        var stillCurrent = PrepareExactCurrentCheckpoint(
            request,
            cancellationToken);
        if (!stillCurrent.IsSuccess || stillCurrent.Value is null)
            return stillCurrent.MapError<CodeDocumentationCertificationServiceResult>();

        var completed = store.CompleteFromServerOwnedExecution(
            startPermit.Value,
            calibrated.Value,
            cancellationToken);
        if (!completed.IsSuccess || completed.Value is null)
        {
            return Failure(
                completed.ErrorMessage is { Length: > 0 }
                    ? completed.ErrorMessage
                    : "The durable certification receipt was rejected.",
                CompletionRejectedErrorCode);
        }

        return OperationResult<CodeDocumentationCertificationServiceResult>
            .Ok(ProjectResult(
                reservation.SessionId,
                completed.Value,
                replayed: false));
    }

    private static Exception? DisposeResources(
        CodeDocumentationCertificationSessionReservation? reservation,
        CodeDocumentationCertificationSessionStore store)
    {
        Exception? first = null;
        try { reservation?.Dispose(); }
        catch (Exception exception) { first = exception; }

        try { store.Dispose(); }
        catch (Exception exception)
        {
            first = first is null
                ? exception
                : new AggregateException(first, exception);
        }

        return first;
    }

    private static OperationResult<CodeDocumentationCertificationServiceResult>
        ReconcileOutcome(
            OperationResult<CodeDocumentationCertificationServiceResult>? primaryResult,
            ExceptionDispatchInfo? primaryException,
            Exception? cleanupException)
    {
        if (primaryException is not null)
        {
            if (cleanupException is not null &&
                !SnapshotExceptionGraphClassifier.IsNonFatal(cleanupException))
            {
                throw new AggregateException(
                    primaryException.SourceException,
                    cleanupException);
            }

            primaryException.Throw();
        }

        if (primaryResult is null)
        {
            if (cleanupException is not null)
                ExceptionDispatchInfo.Capture(cleanupException).Throw();

            return Failure(
                "The certification operation ended without a durable outcome.",
                CompletionRejectedErrorCode);
        }

        if (cleanupException is null)
            return primaryResult;

        if (!SnapshotExceptionGraphClassifier.IsNonFatal(cleanupException))
            ExceptionDispatchInfo.Capture(cleanupException).Throw();

        return primaryResult.IsSuccess
            ? Failure(
                "The certification result was durable, but session cleanup failed.",
                CleanupFailedErrorCode)
            : primaryResult;
    }

    private static OperationResult<CodeDocumentationRepositoryOwnedModelCheckpointStart>
        PrepareExactCurrentCheckpoint(
            CodeDocumentationCertificationServiceRequest request,
            CancellationToken cancellationToken)
    {
        var started =
            CodeDocumentationRepositoryOwnedModelCheckpointStartService
                .PrepareStart(request.StartRequest, cancellationToken);
        if (!started.IsSuccess || started.Value is null)
            return started;

        var parent = started.Value.ParentManifest;
        if (parent is null)
        {
            return OperationResult<CodeDocumentationRepositoryOwnedModelCheckpointStart>
                .Fail(
                    "Independent certification requires an already accepted repository checkpoint; cold start is not certifiable.",
                    ParentCheckpointRequiredErrorCode);
        }

        var passport = CreateAcceptedPassport(
            parent,
            request.StartRequest.MaximumCheckpointBytes);
        if (passport != request.Authority.Candidate)
        {
            return OperationResult<CodeDocumentationRepositoryOwnedModelCheckpointStart>
                .Fail(
                    "The current repository checkpoint identity does not exactly match the admitted final-certification candidate.",
                    CheckpointIdentityMismatchErrorCode);
        }

        return started;
    }

    private static CodeDocumentationSavedModelAssessmentModelPassport
        CreateAcceptedPassport(
            RepositoryOwnedModelCheckpointManifest parent,
            long maximumModelSerializationBytes)
        => new(
            parent.Domain,
            parent.DesignPattern,
            parent.ModelId,
            parent.Version,
            parent.Step,
            parent.RepositoryRelativeManifestPath,
            parent.ManifestSha256,
            parent.RepositoryRelativeWeightsPath,
            parent.WeightsSha256,
            parent.WeightsByteLength,
            maximumModelSerializationBytes,
            CodeDocumentationSavedModelAssessmentModelPassport
                .AcceptedRepositoryCheckpointSourceKind,
            parent.RepositoryRelativeManifestPath,
            parent.ManifestSha256);

    private static OperationResult<CodeDocumentationCertificationSessionStore>
        OpenOrCreateStore(
            string repositoryRoot,
            string expectedStoreId)
    {
        var opened =
            CodeDocumentationCertificationSessionStore
                .OpenExistingForServerOwnedRoot(
                    repositoryRoot,
                    expectedStoreId);
        if (opened.IsSuccess || opened.ErrorCode !=
            CodeDocumentationCertificationSessionStore.ErrorPrefix +
            "STORE_MISSING")
        {
            return opened;
        }

        var created =
            CodeDocumentationCertificationSessionStore
                .CreateNewForServerOwnedRoot(
                    repositoryRoot,
                    expectedStoreId);
        if (created.IsSuccess || created.ErrorCode is not
                (CodeDocumentationCertificationSessionStore.ErrorPrefix +
                 "STORE_EXISTS") and not
                (CodeDocumentationCertificationSessionStore.ErrorPrefix +
                 "HISTORY_EXPECTED"))
        {
            return created;
        }

        return CodeDocumentationCertificationSessionStore
            .OpenExistingForServerOwnedRoot(
                repositoryRoot,
                expectedStoreId);
    }

    private static CodeDocumentationCertificationServiceResult ProjectResult(
        string sessionId,
        CodeDocumentationCertificationSessionReceipt receipt,
        bool replayed)
    {
        var passport = new MeasurementNoisePassport(
            receipt.MeasurementNoisePassport.ScorerIdentity,
            receipt.MeasurementNoisePassport.EvaluationSetId,
            receipt.MeasurementNoisePassport.ScoredRows,
            receipt.MeasurementNoisePassport.RepeatCount,
            receipt.MeasurementNoisePassport.StandardDeviationMultiplier,
            receipt.MeasurementNoisePassport.RepeatedScores,
            receipt.MeasurementNoisePassport.MeanScore,
            receipt.MeasurementNoisePassport.SampleStandardDeviation,
            receipt.MeasurementNoisePassport.BinomialLowerBound,
            receipt.MeasurementNoisePassport.NoiseFloor,
            receipt.MeasurementNoisePassport.CalibratedAtUtc);
        var comparison = MeasurementNoisePolicy.ClassifyDifference(
            passport.MeanScore -
            CodeDocumentationCertificationAdmission.TargetCorrectFraction,
            passport);
        var certified =
            comparison.DifferenceProven &&
            comparison.Status == MeasurementNoisePolicy.ImprovedStatus;
        return new(
            certified,
            replayed,
            sessionId,
            receipt.AdmissionIdentitySha256,
            receipt.ReceiptIdentitySha256,
            comparison,
            receipt);
    }

    private static OperationResult<SavedWeightsIdentity> CaptureSavedWeights(
        string repositoryRoot,
        string repositoryRelativeWeightsPath,
        string expectedSha256,
        long expectedByteLength,
        long maximumByteLength)
    {
        try
        {
            if (!IsSafeRepositoryRelativePath(repositoryRelativeWeightsPath) ||
                !IsCanonicalSha256(expectedSha256) ||
                expectedByteLength <= 0 ||
                maximumByteLength <= 0 ||
                expectedByteLength > maximumByteLength)
            {
                return SavedWeightsUnavailable();
            }

            var root = Path.GetFullPath(repositoryRoot).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            var fullPath = Path.GetFullPath(Path.Combine(
                root,
                repositoryRelativeWeightsPath.Replace(
                    '/',
                    Path.DirectorySeparatorChar)));
            var comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
            if (!fullPath.StartsWith(
                    root + Path.DirectorySeparatorChar,
                    comparison) ||
                SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    fullPath) ||
                !File.Exists(fullPath))
            {
                return SavedWeightsUnavailable();
            }

            var declaredLength = new FileInfo(fullPath).Length;
            if (declaredLength != expectedByteLength ||
                declaredLength > maximumByteLength)
            {
                return SavedWeightsUnavailable();
            }

            using var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    fullPath) ||
                SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    fullPath) ||
                stream.Length != declaredLength)
            {
                return SavedWeightsUnavailable();
            }

            var sha256 = Convert
                .ToHexString(SHA256.HashData(stream))
                .ToLowerInvariant();
            if (!StringComparer.Ordinal.Equals(sha256, expectedSha256))
                return SavedWeightsUnavailable();

            return OperationResult<SavedWeightsIdentity>.Ok(
                new SavedWeightsIdentity(
                    repositoryRelativeWeightsPath,
                    sha256,
                    declaredLength));
        }
        catch (Exception exception)
            when (exception is IOException or UnauthorizedAccessException or
                ArgumentException or NotSupportedException)
        {
            return SavedWeightsUnavailable();
        }
    }

    private static OperationResult<IReadOnlyList<string>> BuildEvidenceRefs(
        CodeDocumentationCertificationServiceRequest request,
        IReadOnlyList<string>? startEvidenceRefs)
    {
        if (startEvidenceRefs is null)
        {
            return OperationResult<IReadOnlyList<string>>.Fail(
                "The accepted checkpoint did not provide bounded assessment evidence.",
                InvalidErrorCode);
        }

        var refs = new List<string>(
            2 + startEvidenceRefs.Count +
            (request.EvidenceRefs?.Count ?? 0))
        {
            "trainable-router-certification-service:s03.10",
            "certification-run:" + request.CertificationRunId,
        };
        if (request.EvidenceRefs is not null)
            refs.AddRange(request.EvidenceRefs);
        refs.AddRange(startEvidenceRefs);

        if (refs.Count == 0 || refs.Count > MaximumAssessmentEvidenceRefs ||
            refs.Any(static value => !IsEvidenceRef(value)))
        {
            return OperationResult<IReadOnlyList<string>>.Fail(
                "The merged CodeDocumentation assessment evidence exceeds the downstream bounds.",
                InvalidErrorCode);
        }

        return OperationResult<IReadOnlyList<string>>.Ok(
            Array.AsReadOnly(refs.ToArray()));
    }

    private static OperationResult<bool> ValidateRequest(
        CodeDocumentationCertificationServiceRequest? request)
    {
        if (request is null ||
            request.StartRequest is null ||
            request.Authority is null ||
            request.Tokenizer is null ||
            request.CreateEvaluationClient is null ||
            !IsCanonicalSha256(request.ExpectedStoreId) ||
            !IsRunId(request.CertificationRunId) ||
            request.ObservedAtUtc == default ||
            request.ObservedAtUtc.Offset != TimeSpan.Zero ||
            request.MaximumDuration is { } duration &&
            (duration <= TimeSpan.Zero || duration > MaximumCalibrationDuration) ||
            request.EvidenceRefs is { Count: > MaximumRequestEvidenceRefs } ||
            request.EvidenceRefs is not null &&
            request.EvidenceRefs.Any(static value =>
                !IsEvidenceRef(value)))
        {
            return OperationResult<bool>.Fail(
                "The CodeDocumentation certification request is not a server-owned, bounded final-exam request.",
                InvalidErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool IsEvidenceRef(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= MaximumEvidenceRefLength &&
           StringComparer.Ordinal.Equals(value, value.Trim()) &&
           !value.Any(char.IsControl) &&
           !value.Contains('\\', StringComparison.Ordinal) &&
           !value.StartsWith("/", StringComparison.Ordinal) &&
           !value.StartsWith("file://", StringComparison.OrdinalIgnoreCase) &&
           !ContainsDriveRoot(value);

    private static bool IsSafeRepositoryRelativePath(string? value)
        => IsEvidenceRef(value) &&
           !value!.Contains("..", StringComparison.Ordinal);

    private static bool ContainsDriveRoot(string value)
        => value.Length >= 3 &&
           value[1] == ':' &&
           (value[2] == '\\' || value[2] == '/') &&
           char.IsLetter(value[0]);

    private static bool IsRunId(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= MaximumRunIdLength &&
           System.Text.Encoding.UTF8.GetByteCount(value) <=
           MaximumRunIdUtf8Bytes &&
           StringComparer.Ordinal.Equals(value, value.Trim()) &&
           !value.Any(char.IsControl);

    private static bool IsCanonicalSha256(string? value)
        => value is { Length: 64 } &&
           value.All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static OperationResult<SavedWeightsIdentity> SavedWeightsUnavailable()
        => OperationResult<SavedWeightsIdentity>.Fail(
            "The accepted saved checkpoint bytes are unavailable or no longer match the admitted manifest.",
            SavedWeightsChangedErrorCode);

    private static OperationResult<CodeDocumentationCertificationServiceResult>
        Failure(string message, string code)
        => OperationResult<CodeDocumentationCertificationServiceResult>.Fail(
            message,
            code);

    private sealed record SavedWeightsIdentity(
        string RepositoryRelativePath,
        string Sha256,
        long ByteLength);
}
