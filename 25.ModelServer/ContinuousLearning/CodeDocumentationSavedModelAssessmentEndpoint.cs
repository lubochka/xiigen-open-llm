// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer;

using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using LoRA;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Tokenizer;
using TrainableRouter.Training;
using TransformerModel.Config;

/// <summary>
/// Запрос запускает только серверный экзамен уже сохранённой модели
/// документации. Клиент не приносит качество, путь к весам, путь к корпусу,
/// модель проверяющего или порт Qwen.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record CodeDocumentationSavedModelAssessmentHttpRequest(
    string DomainSlug,
    string DesignPatternSlug,
    string AssessmentRunId,
    IReadOnlyList<string> ProvenanceRefs);

/// <summary>
/// Финальный clean-certification маршрут принимает только координаты и
/// provenance запроса. Корпус, checkpoint, evaluator, threshold/noise/store и
/// budget выбираются сервером из accepted checkpoint + certification profile.
/// </summary>
internal sealed record CodeDocumentationCertificationHttpRequest(
    string DomainSlug,
    string DesignPatternSlug,
    string CertificationRunId,
    IReadOnlyList<string> ProvenanceRefs);

/// <summary>
/// Public aggregate receipt for a saved-model assessment. The detailed report
/// remains an in-process evaluator artifact; this projection deliberately
/// excludes per-case answers, reasons, generation settings and evaluator call
/// identifiers from the HTTP response.
/// </summary>
internal sealed record CodeDocumentationSavedModelAssessmentPublicReceipt(
    bool IsSuccess,
    string AssessmentRunId,
    string DomainSlug,
    string DesignPatternSlug,
    int EvaluatedCaseCount,
    int PassedCaseCount,
    double CorrectFraction,
    string CorpusVersion,
    string CorpusManifestSha256,
    string CorpusSha256,
    string GenerationSettingsSha256,
    string ModelSourceKind,
    string ModelIdentitySha256,
    string ModelManifestSha256,
    string SavedWeightsSha256,
    string ModelSerializationBeforeSha256,
    long ModelSerializationBeforeByteLength,
    string ModelSerializationAfterSha256,
    long ModelSerializationAfterByteLength,
    string EvaluationProfileId,
    string EvaluationProfileConfigurationSha256,
    IReadOnlyList<string> EvidenceRefs)
{
    internal static CodeDocumentationSavedModelAssessmentPublicReceipt From(
        CodeDocumentationSavedModelAssessmentReport assessment)
        => new(
            assessment.IsSuccess,
            assessment.AssessmentRunId,
            assessment.DomainSlug,
            assessment.DesignPatternSlug,
            assessment.EvaluatedCaseCount,
            assessment.PassedCaseCount,
            assessment.CorrectFraction,
            assessment.CorpusVersion,
            assessment.CorpusManifestSha256,
            assessment.CorpusSha256,
            assessment.GenerationSettingsSha256,
            assessment.ModelSourceKind,
            assessment.ModelIdentitySha256,
            assessment.ModelManifestSha256,
            assessment.SavedWeightsSha256,
            assessment.ModelSerializationBeforeSha256,
            assessment.ModelSerializationBeforeByteLength,
            assessment.ModelSerializationAfterSha256,
            assessment.ModelSerializationAfterByteLength,
            assessment.Evaluator.EvaluationProfileId,
            assessment.Evaluator.EvaluationProfileConfigurationSha256,
            assessment.EvidenceRefs.ToArray());
}

/// <summary>
/// Public calibration aggregate. Per-repeat scores, case ids, student-answer
/// hashes and evaluator request ids remain inside the server-owned report.
/// </summary>
internal sealed record CodeDocumentationSavedModelAssessmentPublicCalibration(
    bool IsSuccess,
    string CalibrationRunId,
    int RepeatCount,
    int HeldOutCaseCount,
    int ExpectedEvaluationCallCount,
    int ObservedEvaluationCallCount,
    string EvaluationSetId,
    string NoisePolicyIdentitySha256,
    double MeanScore,
    double SampleStandardDeviation,
    double BinomialLowerBound,
    double NoiseFloor,
    DateTimeOffset CalibratedAtUtc,
    IReadOnlyList<string> EvidenceRefs)
{
    internal static CodeDocumentationSavedModelAssessmentPublicCalibration From(
        CodeDocumentationSavedModelAssessmentCalibrationReport calibration)
        => new(
            calibration.IsSuccess,
            calibration.CalibrationRunId,
            calibration.RepeatCount,
            calibration.HeldOutCaseCount,
            calibration.ExpectedEvaluationCallCount,
            calibration.ObservedEvaluationCallCount,
            calibration.EvaluationSetId,
            calibration.NoisePolicyIdentitySha256,
            calibration.MeasurementNoisePassport.MeanScore,
            calibration.MeasurementNoisePassport.SampleStandardDeviation,
            calibration.MeasurementNoisePassport.BinomialLowerBound,
            calibration.MeasurementNoisePassport.NoiseFloor,
            calibration.MeasurementNoisePassport.CalibratedAtUtc,
            calibration.EvidenceRefs.ToArray());
}

internal sealed record CodeDocumentationSavedModelAssessmentHttpResponse(
    bool IsSuccess,
    string HumanMessage,
    string? ErrorCode,
    string? ErrorMessage,
    string? SavedWeightsBeforeSha256,
    long? SavedWeightsBeforeByteLength,
    string? SavedWeightsAfterSha256,
    long? SavedWeightsAfterByteLength,
    CodeDocumentationSavedModelAssessmentPublicReceipt? Assessment,
    CodeDocumentationSavedModelAssessmentPublicCalibration? Calibration)
{
    internal static CodeDocumentationSavedModelAssessmentHttpResponse Success(
        CodeDocumentationSavedModelAssessmentReport assessment,
        SavedWeightsReceipt before,
        SavedWeightsReceipt after)
    {
        var percentText = (assessment.CorrectFraction * 100d)
            .ToString("0.##", CultureInfo.InvariantCulture);
        return new(
            IsSuccess: true,
            HumanMessage:
            "Сохранённая модель документации загружена из принятой версии и ответила на закрытые проверочные задания. " +
            $"Проверено {assessment.EvaluatedCaseCount} заданий: верно {assessment.PassedCaseCount} " +
            $"из {assessment.EvaluatedCaseCount} ({percentText}%). " +
            "Отдельный проверяющий сопоставил каждый ответ с закрытым эталоном; сохранённые веса и учебные пары не изменялись.",
            ErrorCode: null,
            ErrorMessage: null,
            before.Sha256,
            before.ByteLength,
            after.Sha256,
            after.ByteLength,
            CodeDocumentationSavedModelAssessmentPublicReceipt.From(assessment),
            Calibration: null);
    }

    internal static CodeDocumentationSavedModelAssessmentHttpResponse Success(
        CodeDocumentationSavedModelAssessmentCalibrationResult result,
        SavedWeightsReceipt before,
        SavedWeightsReceipt after)
    {
        var assessment = result.RepresentativeAssessment;
        var calibration = result.Calibration;
        var meanText = (calibration.MeasurementNoisePassport.MeanScore * 100d)
            .ToString("0.##", CultureInfo.InvariantCulture);
        var noiseText = (calibration.MeasurementNoisePassport.NoiseFloor * 100d)
            .ToString("0.##", CultureInfo.InvariantCulture);
        var representativeText = (assessment.CorrectFraction * 100d)
            .ToString("0.##", CultureInfo.InvariantCulture);
        return new(
            IsSuccess: true,
            HumanMessage:
            "Сохранённая модель документации загружена из принятой версии и несколько раз ответила на те же закрытые задания. " +
            $"Первый подробный экзамен: верно {assessment.PassedCaseCount} из {assessment.EvaluatedCaseCount} ({representativeText}%). " +
            $"Серия: {calibration.RepeatCount} повторов по {calibration.HeldOutCaseCount} заданий; средняя доля верных ответов {meanText}%, " +
            $"по действующему правилу изменение не больше {noiseText} процентных пунктов не подтверждает улучшения. " +
            "Отдельный проверяющий заново сопоставлял каждый ответ с закрытым эталоном; сохранённые веса и учебные пары не изменялись.",
            ErrorCode: null,
            ErrorMessage: null,
            before.Sha256,
            before.ByteLength,
            after.Sha256,
            after.ByteLength,
            CodeDocumentationSavedModelAssessmentPublicReceipt.From(assessment),
            CodeDocumentationSavedModelAssessmentPublicCalibration.From(calibration));
    }

    internal static CodeDocumentationSavedModelAssessmentHttpResponse Failure(
        string? errorCode,
        string? errorMessage)
        => new(
            IsSuccess: false,
            HumanMessage:
            "Оценка сохранённой модели документации не завершена; результат качества не принят этим маршрутом.",
            string.IsNullOrWhiteSpace(errorCode)
                ? CodeDocumentationSavedModelAssessmentEndpoint
                    .AssessmentUnavailableErrorCode
                : errorCode,
            string.IsNullOrWhiteSpace(errorMessage)
                ? "Оценка сохранённой модели документации недоступна."
                : errorMessage,
            SavedWeightsBeforeSha256: null,
            SavedWeightsBeforeByteLength: null,
            SavedWeightsAfterSha256: null,
            SavedWeightsAfterByteLength: null,
            Assessment: null,
            Calibration: null);
}

internal sealed record CodeDocumentationCertificationPublicReceipt(
    bool Certified,
    bool Replayed,
    string CertificationRunId,
    string DomainSlug,
    string DesignPatternSlug,
    string CorpusVersion,
    string CorpusManifestSha256,
    int HeldOutCaseCount,
    int RepeatCount,
    int ExpectedEvaluationCallCount,
    int ObservedEvaluationCallCount,
    double TargetCorrectFraction,
    double MeanScore,
    double BinomialLowerBound,
    double NoiseFloor,
    DateTimeOffset MeasurementCalibratedAtUtc,
    double TargetDifference,
    string TargetStatus,
    bool DifferenceProven,
    string AdmissionIdentitySha256,
    string ReceiptIdentitySha256)
{
    internal static CodeDocumentationCertificationPublicReceipt From(
        CodeDocumentationCertificationServiceResult result)
    {
        var receipt = result.Receipt;
        var passport = receipt.MeasurementNoisePassport;
        return new(
            result.Certified,
            result.Replayed,
            receipt.CalibrationRunId,
            receipt.Representative.DomainSlug,
            receipt.Representative.DesignPatternSlug,
            receipt.Representative.CorpusVersion,
            receipt.Representative.CorpusManifestSha256,
            receipt.HeldOutCaseCount,
            receipt.RepeatCount,
            receipt.ExpectedEvaluationCallCount,
            receipt.ObservedEvaluationCallCount,
            CodeDocumentationCertificationAdmission.TargetCorrectFraction,
            passport.MeanScore,
            passport.BinomialLowerBound,
            passport.NoiseFloor,
            passport.CalibratedAtUtc,
            result.TargetComparison.Difference,
            result.TargetComparison.Status,
            result.TargetComparison.DifferenceProven,
            result.AdmissionIdentitySha256,
            result.ReceiptIdentitySha256);
    }
}

internal sealed record CodeDocumentationCertificationHttpResponse(
    bool IsSuccess,
    string HumanMessage,
    string? ErrorCode,
    string? ErrorMessage,
    CodeDocumentationCertificationPublicReceipt? Certification)
{
    internal static CodeDocumentationCertificationHttpResponse Success(
        CodeDocumentationCertificationServiceResult result)
    {
        var certification = CodeDocumentationCertificationPublicReceipt.From(result);
        var meanText = (certification.MeanScore * 100d)
            .ToString("0.##", CultureInfo.InvariantCulture);
        return new(
            IsSuccess: true,
            HumanMessage:
            certification.Certified
                ? "Сохранённая модель документации прошла серверный clean-certification экзамен; результат подтверждён относительно целевого правила и шума измерения."
                : "Серверный clean-certification экзамен завершён, но результат не доказал прохождение целевого правила с учётом шума измерения. " +
                  $"Средняя доля верных ответов: {meanText}%.",
            ErrorCode: null,
            ErrorMessage: null,
            certification);
    }

    internal static CodeDocumentationCertificationHttpResponse Failure(
        string? errorCode)
        => new(
            IsSuccess: false,
            HumanMessage:
            "Финальная сертификация сохранённой модели документации не выдала сертификат.",
            string.IsNullOrWhiteSpace(errorCode)
                ? CodeDocumentationSavedModelAssessmentEndpoint
                    .CertificationUnavailableErrorCode
                : errorCode,
            "Финальная сертификация сохранённой модели документации недоступна или не завершена.",
            Certification: null);
}

internal interface ICodeDocumentationSavedModelAssessmentEvaluationClientFactory
{
    OperationResult<ILegalTeacherEvaluationInferenceClient> Create(
        ContinuousLearningTp1DpoConfiguration configuration);
}

/// <summary>
/// The certification route consumes a scope already issued by the server's
/// trusted history/provenance owner. The HTTP endpoint is not allowed to
/// synthesize a clean scope from the request corpus.
/// </summary>
internal interface ICodeDocumentationCertificationAuthorityProvider
{
    OperationResult<ServerOwnedCodeDocumentationCertificationScope> Resolve(
        string trustedRepositoryRoot,
        CodeDocumentationRepositoryOwnedModelCheckpointStartRequest startRequest,
        RepositoryOwnedModelCheckpointManifest parent,
        RepositoryOwnedReferenceCorpusOpenResult corpus,
        CodeDocumentationCertificationProfile profile,
        ContinuousLearningTp1DpoConfiguration evaluator);
}

internal sealed class
    DefaultCodeDocumentationSavedModelAssessmentEvaluationClientFactory :
    ICodeDocumentationSavedModelAssessmentEvaluationClientFactory
{
    public OperationResult<ILegalTeacherEvaluationInferenceClient> Create(
        ContinuousLearningTp1DpoConfiguration configuration)
        => ContinuousLearningTp1EvaluationComposition.CreateClient(
            configuration);
}

/// <summary>
/// Локальная серверная дверь исходной оценки сохранённой CodeDocumentation
/// модели. Маршрут читает принятую версию и закрытый корпус, но не публикует
/// новые веса и не создаёт учебные артефакты.
/// </summary>
internal static class CodeDocumentationSavedModelAssessmentEndpoint
{
    internal const string EndpointPath =
        "/internal/continuous-learning/code-documentation/repository-owned-model/assess-saved";
    internal const string CalibrationEndpointPath =
        EndpointPath + "/calibrate";
    internal const string CertificationEndpointPath =
        EndpointPath + "/certify";
    internal const int MaximumProvenanceRefCount = 64;
    internal const string InvalidRequestErrorCode =
        "CODE_DOCUMENTATION_SAVED_MODEL_ASSESSMENT_HTTP_REQUEST_INVALID";
    internal const string InvalidCertificationRequestErrorCode =
        "CODE_DOCUMENTATION_SAVED_MODEL_CERTIFICATION_HTTP_REQUEST_INVALID";
    internal const string AssessmentUnavailableErrorCode =
        "CODE_DOCUMENTATION_SAVED_MODEL_ASSESSMENT_UNAVAILABLE";
    internal const string CertificationUnavailableErrorCode =
        "CODE_DOCUMENTATION_SAVED_MODEL_CERTIFICATION_UNAVAILABLE";
    internal const string ProfileMissingErrorCode =
        "CODE_DOCUMENTATION_SAVED_MODEL_ASSESSMENT_PROFILE_MISSING";
    internal const string CertificationProfileMissingErrorCode =
        "CODE_DOCUMENTATION_SAVED_MODEL_CERTIFICATION_PROFILE_MISSING";
    internal const string CertificationAuthorityMissingErrorCode =
        "CODE_DOCUMENTATION_SAVED_MODEL_CERTIFICATION_AUTHORITY_MISSING";
    internal const string CorpusMismatchErrorCode =
        "CODE_DOCUMENTATION_SAVED_MODEL_ASSESSMENT_CORPUS_MISMATCH";
    internal const string StartMismatchErrorCode =
        "CODE_DOCUMENTATION_SAVED_MODEL_ASSESSMENT_START_MISMATCH";
    internal const string ParentModelRequiredErrorCode =
        "CODE_DOCUMENTATION_SAVED_MODEL_ASSESSMENT_PARENT_MODEL_REQUIRED";
    internal const string SavedWeightsChangedErrorCode =
        "CODE_DOCUMENTATION_SAVED_MODEL_ASSESSMENT_SAVED_WEIGHTS_CHANGED";
    internal const string WslRepositoryOwnedTrainingTrustedRepositoryRoot =
        "/mnt/c/Projects/llm_mvp_core";

    private const int MaximumCoordinateTextLength = 128;
    private const int MaximumAssessmentRunIdLength = 256;
    private const int MaximumEvidenceRefLength = 512;
    private const int MaximumCertificationRequestUtf8Bytes = 8_192;
    private static readonly JsonDocumentOptions CertificationRequestJsonOptions = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = 8,
    };
    private static readonly string[] CertificationRequestFields =
    [
        "domainSlug",
        "designPatternSlug",
        "certificationRunId",
        "provenanceRefs",
    ];

    internal static IEndpointConventionBuilder MapCodeDocumentationSavedModelAssessment(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var assessment = endpoints.MapPost(
            EndpointPath,
            static (
                    CodeDocumentationSavedModelAssessmentHttpRequest? request,
                    IServiceProvider services,
                    CancellationToken cancellationToken) =>
                ExecuteAsync(
                    request,
                    services,
                    DateTimeOffset.UtcNow,
                    cancellationToken));
        endpoints.MapPost(
            CalibrationEndpointPath,
            static (
                    CodeDocumentationSavedModelAssessmentHttpRequest? request,
                    IServiceProvider services,
                    CancellationToken cancellationToken) =>
                ExecuteCalibrationAsync(
                    request,
                    services,
                    DateTimeOffset.UtcNow,
                    cancellationToken));
        endpoints.MapPost(
            CertificationEndpointPath,
            static (
                    HttpRequest request,
                    IServiceProvider services,
                    CancellationToken cancellationToken) =>
                ExecuteCertificationHttpAsync(
                    request,
                    services,
                    DateTimeOffset.UtcNow,
                    cancellationToken));
        return assessment;
    }

    internal static async Task<IResult> ExecuteAsync(
        CodeDocumentationSavedModelAssessmentHttpRequest? request,
        IServiceProvider services,
        DateTimeOffset observedAtUtc,
        CancellationToken cancellationToken = default)
        => await ExecuteCoreAsync(
                request,
                services,
                observedAtUtc,
                runCalibration: false,
                cancellationToken)
            .ConfigureAwait(false);

    internal static async Task<IResult> ExecuteCalibrationAsync(
        CodeDocumentationSavedModelAssessmentHttpRequest? request,
        IServiceProvider services,
        DateTimeOffset observedAtUtc,
        CancellationToken cancellationToken = default)
        => await ExecuteCoreAsync(
                request,
                services,
                observedAtUtc,
                runCalibration: true,
                cancellationToken)
            .ConfigureAwait(false);

    private static async Task<IResult> ExecuteCertificationHttpAsync(
        HttpRequest httpRequest,
        IServiceProvider services,
        DateTimeOffset observedAtUtc,
        CancellationToken cancellationToken)
    {
        var parsed = await ParseCertificationRequestAsync(
                httpRequest,
                cancellationToken)
            .ConfigureAwait(false);
        if (!parsed.IsSuccess || parsed.Value is null)
            return CertificationError(StatusCodes.Status400BadRequest, parsed.ErrorCode);

        if (observedAtUtc == default || observedAtUtc.Offset != TimeSpan.Zero)
            return CertificationError(
                StatusCodes.Status400BadRequest,
                InvalidCertificationRequestErrorCode);

        var request = parsed.Value;
        var profiles = CodeDocumentationCertificationProfileResolver
            .ResolveFromEnvironment();
        if (!profiles.IsSuccess || profiles.Value is null)
            return CertificationError(
                StatusCodes.Status503ServiceUnavailable,
                profiles.ErrorCode ?? CertificationProfileMissingErrorCode);
        if (!profiles.Value.TryGetValue(request.DomainSlug, out var profile))
        {
            return CertificationError(
                StatusCodes.Status503ServiceUnavailable,
                CertificationProfileMissingErrorCode);
        }

        var domainProfile = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveSupportedDomainProfile(request.DomainSlug);
        if (!domainProfile.IsSuccess || domainProfile.Value is null ||
            !StringComparer.Ordinal.Equals(
                domainProfile.Value.DesignPatternSlug,
                request.DesignPatternSlug) ||
            !StringComparer.Ordinal.Equals(
                profile.DesignPatternSlug,
                request.DesignPatternSlug))
        {
            return CertificationError(
                StatusCodes.Status409Conflict,
                CodeDocumentationCertificationService.InvalidErrorCode);
        }

        var startup = services.GetService<ContinuousLearningModelServerStartup>();
        var fileAccess = services.GetService<ServerOwnedContinuousLearningFileAccess>();
        if (startup is null || fileAccess is null)
        {
            return CertificationError(
                StatusCodes.Status503ServiceUnavailable,
                CertificationUnavailableErrorCode);
        }

        var context = startup.PrepareRepositoryOwnedTrainingExecutionContext(
            request.DomainSlug,
            fileAccess,
            cancellationToken);
        if (!context.IsSuccess || context.Value is null)
            return CertificationError(
                StatusCodes.Status503ServiceUnavailable,
                context.ErrorCode ?? CertificationUnavailableErrorCode);

        var executionStart = context.Value.RepositoryStartRequest;
        var startRequest = new CodeDocumentationRepositoryOwnedModelCheckpointStartRequest(
            executionStart.RepositoryRoot,
            executionStart.DomainSlug,
            executionStart.DesignPatternSlug,
            executionStart.ModelIdSlug,
            executionStart.ModelVersion,
            context.Value.ModelConfig,
            executionStart.RuntimeCompatibility,
            RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied,
            LoraConfig: context.Value.LoraConfig);
        var trustedAssessmentRoot = services.GetService<
            CodeDocumentationSavedModelAssessmentTrustedRoot>();
        if (trustedAssessmentRoot is null ||
            !trustedAssessmentRoot.Matches(startRequest.RepositoryRoot) ||
            !StringComparer.Ordinal.Equals(
                startRequest.DomainSlug,
                profile.DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                startRequest.DesignPatternSlug,
                profile.DesignPatternSlug))
        {
            return CertificationError(
                StatusCodes.Status409Conflict,
                StartMismatchErrorCode);
        }

        var adapterSeedAuthority = CodeDocumentationDpoSeedAuthorityResolver.Resolve(
            executionStart.DomainSlug,
            context.Value.RootInitializationSeed);
        if (!adapterSeedAuthority.IsSuccess || adapterSeedAuthority.Value is null)
            return CertificationError(
                StatusCodes.Status409Conflict,
                adapterSeedAuthority.ErrorCode ?? StartMismatchErrorCode);

        startRequest = startRequest with
        {
            AdapterSeedAuthority = adapterSeedAuthority.Value,
        };

        var started = CodeDocumentationRepositoryOwnedModelCheckpointStartService
            .PrepareStart(startRequest, cancellationToken);
        if (!started.IsSuccess || started.Value is null ||
            started.Value.ParentManifest is null)
        {
            return CertificationError(
                StatusCodes.Status409Conflict,
                started.ErrorCode ?? ParentModelRequiredErrorCode);
        }

        var corpus = OpenConfiguredCorpus(startRequest.RepositoryRoot, profile);
        if (!corpus.IsSuccess || corpus.Value is null)
        {
            return CertificationError(
                StatusCodes.Status409Conflict,
                corpus.ErrorCode ?? CorpusMismatchErrorCode);
        }

        var authorityProvider = services.GetService<
            ICodeDocumentationCertificationAuthorityProvider>();
        if (authorityProvider is null)
        {
            return CertificationError(
                StatusCodes.Status503ServiceUnavailable,
                CertificationAuthorityMissingErrorCode);
        }

        OperationResult<ServerOwnedCodeDocumentationCertificationScope> authority;
        try
        {
            authority = authorityProvider.Resolve(
                startRequest.RepositoryRoot,
                startRequest,
                started.Value.ParentManifest,
                corpus.Value,
                profile,
                startup.TeacherConfiguration);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return CertificationError(
                StatusCodes.Status409Conflict,
                CodeDocumentationCertificationService.CancelledErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return CertificationError(
                StatusCodes.Status409Conflict,
                CertificationUnavailableErrorCode);
        }

        if (!authority.IsSuccess || authority.Value is null)
        {
            return CertificationError(
                StatusCodes.Status409Conflict,
                authority.ErrorCode ?? CodeDocumentationCertificationService
                    .AdmissionRejectedErrorCode);
        }

        var parent = started.Value.ParentManifest;
        var selectedCandidate = new CodeDocumentationSavedModelAssessmentModelPassport(
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
            startRequest.MaximumCheckpointBytes,
            CodeDocumentationSavedModelAssessmentModelPassport
                .AcceptedRepositoryCheckpointSourceKind,
            parent.RepositoryRelativeManifestPath,
            parent.ManifestSha256);
        var admitted = CodeDocumentationCertificationAdmission.TryCreate(
            authority.Value,
            selectedCandidate,
            corpus.Value,
            authority.Value.Index);
        var declaration = authority.Value.Declaration;
        var canonicalNoise = MeasurementNoisePolicyConfig.Canonical;
        var selectedPlan = CodeDocumentationSavedModelAssessmentCalibrationService
            .PlanCalibration(
                corpus.Value.Corpus,
                profile.GenerationSettings,
                startup.TeacherConfiguration,
                canonicalNoise);
        if (!admitted.IsSuccess || admitted.Value is null ||
            !StringComparer.Ordinal.Equals(declaration.Domain, profile.DomainSlug) ||
            !StringComparer.Ordinal.Equals(declaration.DesignPattern, profile.DesignPatternSlug) ||
            !StringComparer.Ordinal.Equals(declaration.Subject, profile.SubjectSlug) ||
            !StringComparer.Ordinal.Equals(declaration.ScopeId, profile.ScopeId) ||
            !StringComparer.Ordinal.Equals(declaration.ScopeVersion, profile.ScopeVersion) ||
            !StringComparer.Ordinal.Equals(authority.Value.Index.Version, profile.ScopeVersion) ||
            !StringComparer.Ordinal.Equals(declaration.CoverageRevision, profile.CoverageRevision) ||
            declaration.CoverageCutUtc != profile.CoverageCutUtc ||
            !declaration.OwnerSnapshots.Any(owner =>
                StringComparer.Ordinal.Equals(owner.Id, profile.OwnerSnapshotId) &&
                StringComparer.Ordinal.Equals(owner.Version, profile.OwnerSnapshotVersion)) ||
            declaration.CertificationSourceBindings.Any(binding =>
                !StringComparer.Ordinal.Equals(binding.OwnerSnapshotId, profile.OwnerSnapshotId) ||
                !StringComparer.Ordinal.Equals(binding.OwnerSnapshotVersion, profile.OwnerSnapshotVersion)) ||
            authority.Value.Generation != profile.GenerationSettings ||
            !ReferenceEquals(authority.Value.Evaluator, startup.TeacherConfiguration) ||
            !StringComparer.Ordinal.Equals(authority.Value.Noise.IdentitySha256,
                canonicalNoise.IdentitySha256) ||
            authority.Value.Noise.RepeatCount != canonicalNoise.RepeatCount ||
            authority.Value.Noise.StandardDeviationMultiplier !=
                canonicalNoise.StandardDeviationMultiplier ||
            !selectedPlan.IsSuccess || selectedPlan.Value is null ||
            authority.Value.ExpectedCalls != selectedPlan.Value.ExpectedEvaluationCallCount)
        {
            return CertificationError(
                StatusCodes.Status409Conflict,
                CodeDocumentationCertificationService.AdmissionRejectedErrorCode);
        }

        var factory = services.GetService<
            ICodeDocumentationSavedModelAssessmentEvaluationClientFactory>()
            ?? new DefaultCodeDocumentationSavedModelAssessmentEvaluationClientFactory();
        ILegalTeacherEvaluationInferenceClient? ownedClient = null;
        var acquisitionAttempted = false;
        Func<OperationResult<ILegalTeacherEvaluationInferenceClient>>
            createEvaluationClient = () =>
            {
                if (acquisitionAttempted)
                {
                    return OperationResult<
                        ILegalTeacherEvaluationInferenceClient>.Fail(
                        "The certification evaluator acquisition callback was invoked more than once.",
                        CodeDocumentationCertificationService
                            .EvaluationClientUnavailableErrorCode);
                }

                acquisitionAttempted = true;
                var created = factory.Create(startup.TeacherConfiguration);
                if (created.IsSuccess && created.Value is not null)
                    ownedClient = created.Value;
                return created;
            };
        IResult? primaryResult = null;
        var primaryResultIsSuccess = false;
        ExceptionDispatchInfo? primaryException = null;
        try
        {
            var result = await new CodeDocumentationCertificationService()
                .CertifyAsync(
                    new CodeDocumentationCertificationServiceRequest(
                        startRequest,
                        authority.Value,
                        Program.ComputeServerOwnedMaterialBoundaryStoreId(
                            startRequest.RepositoryRoot),
                        context.Value.Tokenizer,
                        createEvaluationClient,
                        request.CertificationRunId,
                        observedAtUtc,
                        request.ProvenanceRefs),
                    cancellationToken)
                .ConfigureAwait(false);

            if (!result.IsSuccess || result.Value is null)
            {
                var statusCode = result.ErrorCode ==
                    CodeDocumentationCertificationService
                        .EvaluationClientUnavailableErrorCode
                    ? StatusCodes.Status503ServiceUnavailable
                    : StatusCodes.Status409Conflict;
                primaryResult = CertificationError(
                    statusCode,
                    statusCode == StatusCodes.Status503ServiceUnavailable
                        ? CertificationUnavailableErrorCode
                        : result.ErrorCode ?? CertificationUnavailableErrorCode);
            }
            else
            {
                primaryResult = Results.Json(
                    CodeDocumentationCertificationHttpResponse.Success(result.Value),
                    statusCode: StatusCodes.Status200OK);
                primaryResultIsSuccess = true;
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            primaryResult = CertificationError(
                StatusCodes.Status409Conflict,
                CodeDocumentationCertificationService.CancelledErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            primaryResult = CertificationError(
                StatusCodes.Status409Conflict,
                CertificationUnavailableErrorCode);
        }
        catch (Exception exception)
        {
            primaryException = ExceptionDispatchInfo.Capture(exception);
        }

        Exception? disposalException = null;
        if (ownedClient is not null)
        {
            try
            {
                ownedClient.Dispose();
            }
            catch (Exception exception)
            {
                disposalException = exception;
            }
        }

        if (primaryException is not null)
        {
            if (disposalException is not null &&
                !SnapshotExceptionGraphClassifier.IsNonFatal(disposalException))
            {
                throw new AggregateException(
                    primaryException.SourceException,
                    disposalException);
            }

            primaryException.Throw();
            return null!;
        }

        if (disposalException is not null)
        {
            if (!SnapshotExceptionGraphClassifier.IsNonFatal(disposalException))
            {
                ExceptionDispatchInfo.Capture(disposalException).Throw();
                return null!;
            }

            if (primaryResultIsSuccess)
            {
                return CertificationError(
                    StatusCodes.Status409Conflict,
                    CertificationUnavailableErrorCode);
            }
        }

        return primaryResult!;
    }

    private static async Task<OperationResult<CodeDocumentationCertificationHttpRequest>>
        ParseCertificationRequestAsync(
            HttpRequest request,
            CancellationToken cancellationToken)
    {
        if (request.ContentLength is > MaximumCertificationRequestUtf8Bytes)
        {
            return InvalidCertificationRequest<CodeDocumentationCertificationHttpRequest>();
        }

        try
        {
            using var body = await ReadBoundedCertificationRequestBodyAsync(
                    request.Body,
                    cancellationToken)
                .ConfigureAwait(false);
            if (body is null)
                return InvalidCertificationRequest<CodeDocumentationCertificationHttpRequest>();

            using var document = await JsonDocument.ParseAsync(
                    body,
                    CertificationRequestJsonOptions,
                    cancellationToken)
                .ConfigureAwait(false);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return InvalidCertificationRequest<CodeDocumentationCertificationHttpRequest>();

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (!CertificationRequestFields.Contains(
                        property.Name,
                        StringComparer.Ordinal) ||
                    !seen.Add(property.Name))
                {
                    return InvalidCertificationRequest<CodeDocumentationCertificationHttpRequest>();
                }
            }

            if (seen.Count != CertificationRequestFields.Length ||
                !TryReadJsonString(document.RootElement, "domainSlug", out var domain) ||
                !TryReadJsonString(document.RootElement, "designPatternSlug", out var pattern) ||
                !TryReadJsonString(document.RootElement, "certificationRunId", out var runId) ||
                !document.RootElement.TryGetProperty(
                    "provenanceRefs",
                    out var refsElement) ||
                refsElement.ValueKind != JsonValueKind.Array)
            {
                return InvalidCertificationRequest<CodeDocumentationCertificationHttpRequest>();
            }

            var refs = new List<string>();
            foreach (var item in refsElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String ||
                    item.GetString() is not { } value)
                {
                    return InvalidCertificationRequest<CodeDocumentationCertificationHttpRequest>();
                }

                refs.Add(value);
            }

            var parsed = new CodeDocumentationCertificationHttpRequest(
                domain!,
                pattern!,
                runId!,
                Array.AsReadOnly(refs.ToArray()));
            if (!IsBoundedRequiredText(parsed.DomainSlug, MaximumCoordinateTextLength) ||
                !IsBoundedRequiredText(parsed.DesignPatternSlug, MaximumCoordinateTextLength) ||
                !IsBoundedRequiredText(parsed.CertificationRunId, MaximumAssessmentRunIdLength) ||
                !AreSafeEvidenceRefs(parsed.ProvenanceRefs))
            {
                return InvalidCertificationRequest<CodeDocumentationCertificationHttpRequest>();
            }

            return OperationResult<CodeDocumentationCertificationHttpRequest>.Ok(parsed);
        }
        catch (JsonException)
        {
            return InvalidCertificationRequest<CodeDocumentationCertificationHttpRequest>();
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidCertificationRequest<CodeDocumentationCertificationHttpRequest>();
        }
    }

    private static async Task<MemoryStream?> ReadBoundedCertificationRequestBodyAsync(
        Stream requestBody,
        CancellationToken cancellationToken)
    {
        var body = new MemoryStream(MaximumCertificationRequestUtf8Bytes);
        var buffer = new byte[Math.Min(
            4096,
            MaximumCertificationRequestUtf8Bytes + 1)];
        var total = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var allowed = MaximumCertificationRequestUtf8Bytes + 1 - total;
            var read = await requestBody
                .ReadAsync(
                    buffer.AsMemory(0, Math.Min(buffer.Length, allowed)),
                    cancellationToken)
                .ConfigureAwait(false);
            if (read == 0)
                break;

            total += read;
            if (total > MaximumCertificationRequestUtf8Bytes)
            {
                await body.DisposeAsync().ConfigureAwait(false);
                return null;
            }

            body.Write(buffer, 0, read);
        }

        body.Position = 0;
        return body;
    }

    private static bool TryReadJsonString(
        JsonElement element,
        string name,
        out string? value)
    {
        if (element.TryGetProperty(name, out var property) &&
            property.ValueKind == JsonValueKind.String)
        {
            value = property.GetString();
            return value is not null;
        }

        value = null;
        return false;
    }

    private static OperationResult<T> InvalidCertificationRequest<T>()
        => OperationResult<T>.Fail(
            "Запрос финальной сертификации сохранённой модели документации некорректен.",
            InvalidCertificationRequestErrorCode);

    private static async Task<IResult> ExecuteCoreAsync(
        CodeDocumentationSavedModelAssessmentHttpRequest? request,
        IServiceProvider services,
        DateTimeOffset observedAtUtc,
        bool runCalibration,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateRequest(request, observedAtUtc);
        if (!validation.IsSuccess)
            return Error(StatusCodes.Status400BadRequest, validation);

        var profiles =
            CodeDocumentationSavedModelAssessmentProfileResolver
                .ResolveFromEnvironment();
        if (!profiles.IsSuccess || profiles.Value is null)
            return Error(StatusCodes.Status503ServiceUnavailable, profiles);
        if (!profiles.Value.TryGetValue(request!.DomainSlug, out var profile))
        {
            return Error(
                StatusCodes.Status503ServiceUnavailable,
                OperationResult<bool>.Fail(
                    "Для выбранного домена нет серверного профиля оценки сохранённой модели документации.",
                    ProfileMissingErrorCode));
        }

        var domainProfile = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveSupportedDomainProfile(request.DomainSlug);
        if (!domainProfile.IsSuccess || domainProfile.Value is null)
            return Error(StatusCodes.Status400BadRequest, domainProfile);
        if (!StringComparer.Ordinal.Equals(
                request.DesignPatternSlug,
                domainProfile.Value.DesignPatternSlug) ||
            !StringComparer.Ordinal.Equals(
                profile.DesignPatternSlug,
                request.DesignPatternSlug))
        {
            return Error(
                StatusCodes.Status409Conflict,
                OperationResult<bool>.Fail(
                    "Оценка сохранённой модели документации допускает только серверный профиль выбранного домена.",
                    ProfileMissingErrorCode));
        }

        var startup = services.GetService<ContinuousLearningModelServerStartup>();
        var fileAccess = services.GetService<ServerOwnedContinuousLearningFileAccess>();
        if (startup is null || fileAccess is null)
        {
            return Error(
                StatusCodes.Status503ServiceUnavailable,
                OperationResult<bool>.Fail(
                    "Оценка сохранённой модели документации недоступна: сервер ещё не подготовил контекст непрерывного обучения.",
                    AssessmentUnavailableErrorCode));
        }

        var context = startup.PrepareRepositoryOwnedTrainingExecutionContext(
            request.DomainSlug,
            fileAccess,
            cancellationToken);
        if (!context.IsSuccess || context.Value is null)
            return Error(StatusCodes.Status503ServiceUnavailable, context);

        var start = context.Value.RepositoryStartRequest;
        var trustedAssessmentRoot = services.GetService<
            CodeDocumentationSavedModelAssessmentTrustedRoot>();
        if (trustedAssessmentRoot is null ||
            !trustedAssessmentRoot.Matches(start.RepositoryRoot) ||
            !StringComparer.Ordinal.Equals(
                start.DomainSlug,
                profile.DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                start.DesignPatternSlug,
                profile.DesignPatternSlug))
        {
            return Error(
                StatusCodes.Status409Conflict,
                OperationResult<bool>.Fail(
                    "Оценка сохранённой модели документации должна работать только в серверной доверенной папке и серверном домене.",
                    StartMismatchErrorCode));
        }

        var adapterSeedAuthority = CodeDocumentationDpoSeedAuthorityResolver.Resolve(
            start.DomainSlug,
            context.Value.RootInitializationSeed);
        if (!adapterSeedAuthority.IsSuccess || adapterSeedAuthority.Value is null)
            return Error(StatusCodes.Status409Conflict, adapterSeedAuthority);

        var configuredCorpus = OpenConfiguredCorpus(
            start.RepositoryRoot,
            profile);
        if (!configuredCorpus.IsSuccess || configuredCorpus.Value is null)
            return Error(StatusCodes.Status409Conflict, configuredCorpus);

        var materialBoundary =
            services.GetService<CodeDocumentationMaterialBoundary>();
        if (materialBoundary is null)
        {
            var provider = services.GetService<
                ICodeDocumentationMaterialBoundaryProvider>();
            var issued = provider?.IssueTeacherVisibleDiagnostic(
                start.RepositoryRoot,
                configuredCorpus.Value);
            if (issued is { IsSuccess: true, Value: not null })
                materialBoundary = issued.Value;
        }
        if (materialBoundary is null ||
            !materialBoundary.MatchesAssessmentContext(
                start.RepositoryRoot,
                configuredCorpus.Value.Manifest.ManifestSha256))
        {
            return Error(
                StatusCodes.Status503ServiceUnavailable,
                OperationResult<bool>.Fail(
                    "Оценка сохранённой модели документации недоступна: сервер не выдал проверенную границу material exposure.",
                    AssessmentUnavailableErrorCode));
        }

        var materialExposure = materialBoundary.BeginDiagnosticDisclosure(
            cancellationToken);
        if (!materialExposure.IsSuccess || materialExposure.Value is null)
        {
            return Error(
                StatusCodes.Status503ServiceUnavailable,
                OperationResult<bool>.Fail(
                    "Оценка сохранённой модели документации остановлена: проверенная граница material exposure недействительна.",
                    AssessmentUnavailableErrorCode));
        }

        using var heldMaterialExposure = materialExposure.Value;

        var factory =
            services.GetService<ICodeDocumentationSavedModelAssessmentEvaluationClientFactory>()
            ?? new DefaultCodeDocumentationSavedModelAssessmentEvaluationClientFactory();
        return await AssessPreparedRepositoryModelCoreAsync(
                request,
                start.RepositoryRoot,
                start.DomainSlug,
                start.DesignPatternSlug,
                start.ModelIdSlug,
                start.ModelVersion,
                context.Value.ModelConfig,
                start.RuntimeCompatibility,
                context.Value.Tokenizer,
                context.Value.LoraConfig,
                startup.TeacherConfiguration,
                profile,
                context.Value.EvidenceRefs,
                factory,
                observedAtUtc,
                adapterSeedAuthority.Value,
                runCalibration,
                cancellationToken)
            .ConfigureAwait(false);
    }

    internal static async Task<IResult> AssessPreparedRepositoryModelAsync(
        CodeDocumentationSavedModelAssessmentHttpRequest request,
        string repositoryRoot,
        string domainSlug,
        string designPatternSlug,
        string modelIdSlug,
        string modelVersion,
        ModelConfig modelConfig,
        RepositoryOwnedModelRuntimeCompatibility runtimeCompatibility,
        BpeTokenizer tokenizer,
        LoraConfig loraConfig,
        ContinuousLearningTp1DpoConfiguration teacherConfiguration,
        CodeDocumentationSavedModelAssessmentProfile profile,
        IReadOnlyList<string> serverEvidenceRefs,
        ICodeDocumentationSavedModelAssessmentEvaluationClientFactory factory,
        DateTimeOffset observedAtUtc,
        CancellationToken cancellationToken = default)
        => await AssessPreparedRepositoryModelCoreAsync(
                request,
                repositoryRoot,
                domainSlug,
                designPatternSlug,
                modelIdSlug,
                modelVersion,
                modelConfig,
                runtimeCompatibility,
                tokenizer,
                loraConfig,
                teacherConfiguration,
                profile,
                serverEvidenceRefs,
                factory,
                observedAtUtc,
                adapterSeedAuthority: null,
                runCalibration: false,
                cancellationToken)
            .ConfigureAwait(false);

    internal static async Task<IResult> CalibratePreparedRepositoryModelAsync(
        CodeDocumentationSavedModelAssessmentHttpRequest request,
        string repositoryRoot,
        string domainSlug,
        string designPatternSlug,
        string modelIdSlug,
        string modelVersion,
        ModelConfig modelConfig,
        RepositoryOwnedModelRuntimeCompatibility runtimeCompatibility,
        BpeTokenizer tokenizer,
        LoraConfig loraConfig,
        ContinuousLearningTp1DpoConfiguration teacherConfiguration,
        CodeDocumentationSavedModelAssessmentProfile profile,
        IReadOnlyList<string> serverEvidenceRefs,
        ICodeDocumentationSavedModelAssessmentEvaluationClientFactory factory,
        DateTimeOffset observedAtUtc,
        CancellationToken cancellationToken = default)
        => await AssessPreparedRepositoryModelCoreAsync(
                request,
                repositoryRoot,
                domainSlug,
                designPatternSlug,
                modelIdSlug,
                modelVersion,
                modelConfig,
                runtimeCompatibility,
                tokenizer,
                loraConfig,
                teacherConfiguration,
                profile,
                serverEvidenceRefs,
                factory,
                observedAtUtc,
                adapterSeedAuthority: null,
                runCalibration: true,
                cancellationToken)
            .ConfigureAwait(false);

    private static async Task<IResult> AssessPreparedRepositoryModelCoreAsync(
        CodeDocumentationSavedModelAssessmentHttpRequest request,
        string repositoryRoot,
        string domainSlug,
        string designPatternSlug,
        string modelIdSlug,
        string modelVersion,
        ModelConfig modelConfig,
        RepositoryOwnedModelRuntimeCompatibility runtimeCompatibility,
        BpeTokenizer tokenizer,
        LoraConfig loraConfig,
        ContinuousLearningTp1DpoConfiguration teacherConfiguration,
        CodeDocumentationSavedModelAssessmentProfile profile,
        IReadOnlyList<string> serverEvidenceRefs,
        ICodeDocumentationSavedModelAssessmentEvaluationClientFactory factory,
        DateTimeOffset observedAtUtc,
        LoraPairRandomSeedAuthority? adapterSeedAuthority,
        bool runCalibration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(factory);

        if (!StringComparer.Ordinal.Equals(domainSlug, profile.DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                designPatternSlug,
                profile.DesignPatternSlug))
        {
            return Error(
                StatusCodes.Status409Conflict,
                OperationResult<bool>.Fail(
                    "Оценка сохранённой модели документации должна работать только с серверным доменом и шаблоном.",
                    StartMismatchErrorCode));
        }

        var corpus = OpenConfiguredCorpus(repositoryRoot, profile);
        if (!corpus.IsSuccess || corpus.Value is null)
            return Error(StatusCodes.Status409Conflict, corpus);

        var startRequest =
            new CodeDocumentationRepositoryOwnedModelCheckpointStartRequest(
                repositoryRoot,
                domainSlug,
                designPatternSlug,
                modelIdSlug,
                modelVersion,
                modelConfig,
                runtimeCompatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied,
                LoraConfig: loraConfig,
                AdapterSeedAuthority: adapterSeedAuthority);
        var started =
            CodeDocumentationRepositoryOwnedModelCheckpointStartService
                .PrepareStart(startRequest, cancellationToken);
        if (!started.IsSuccess || started.Value is null)
            return Error(StatusCodes.Status409Conflict, started);
        if (started.Value.ParentManifest is null)
        {
            return Error(
                StatusCodes.Status409Conflict,
                OperationResult<bool>.Fail(
                    "Оценка сохранённой модели документации требует уже принятую версию с диска; пустая новая модель здесь запрещена.",
                    ParentModelRequiredErrorCode));
        }

        var parent = started.Value.ParentManifest;
        var beforeWeights = await CaptureSavedWeightsAsync(
                repositoryRoot,
                parent.RepositoryRelativeWeightsPath,
                parent.WeightsSha256,
                parent.WeightsByteLength,
                startRequest.MaximumCheckpointBytes,
                cancellationToken)
            .ConfigureAwait(false);
        if (!beforeWeights.IsSuccess || beforeWeights.Value is null)
            return Error(StatusCodes.Status409Conflict, beforeWeights);
        var beforeWeightsReceipt = beforeWeights.Value;

        if (runCalibration)
        {
            var calibrationPlan =
                CodeDocumentationSavedModelAssessmentCalibrationService
                    .PlanCalibration(
                        corpus.Value.Corpus,
                        profile.GenerationSettings,
                        teacherConfiguration,
                        MeasurementNoisePolicyConfig.Canonical);
            if (!calibrationPlan.IsSuccess || calibrationPlan.Value is null)
                return Error(StatusCodes.Status409Conflict, calibrationPlan);
        }

        var client = factory.Create(teacherConfiguration);
        if (!client.IsSuccess || client.Value is null)
            return Error(StatusCodes.Status503ServiceUnavailable, client);

        var ownedClient = client.Value;
        IResult? primaryResult = null;
        var primaryResultIsSuccess = false;
        ExceptionDispatchInfo? primaryException = null;
        try
        {
            var assessmentRequest =
                new CodeDocumentationSavedModelAssessmentRequest(
                    started.Value.Model,
                    tokenizer,
                    CreatePassport(parent, startRequest.MaximumCheckpointBytes),
                    corpus.Value.Corpus,
                    corpus.Value.Manifest,
                    ownedClient,
                    teacherConfiguration,
                    profile.GenerationSettings,
                    request.AssessmentRunId,
                    observedAtUtc,
                    BuildEvidenceRefs(
                        request,
                        serverEvidenceRefs,
                        started.Value.EvidenceRefs,
                        corpus.Value.Manifest));

            if (runCalibration)
            {
                var calibrationService =
                    new CodeDocumentationSavedModelAssessmentCalibrationService();
                var calibrated = await calibrationService
                    .CalibrateAsync(
                        assessmentRequest,
                        MeasurementNoisePolicyConfig.Canonical,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                if (!calibrated.IsSuccess || calibrated.Value is null)
                {
                    primaryResult = Error(StatusCodes.Status409Conflict, calibrated);
                }
                else if (cancellationToken.IsCancellationRequested)
                {
                    primaryResult = Error(
                        StatusCodes.Status409Conflict,
                        CalibrationCancelled());
                }
                else
                {
                    var afterCalibrationWeights =
                        await CaptureUnchangedSavedWeightsAsync()
                            .ConfigureAwait(false);
                    if (!afterCalibrationWeights.IsSuccess ||
                        afterCalibrationWeights.Value is null)
                    {
                        primaryResult = Error(
                            StatusCodes.Status409Conflict,
                            afterCalibrationWeights);
                    }
                    else if (cancellationToken.IsCancellationRequested)
                    {
                        primaryResult = Error(
                            StatusCodes.Status409Conflict,
                            CalibrationCancelled());
                    }
                    else
                    {
                        primaryResult = Results.Json(
                            CodeDocumentationSavedModelAssessmentHttpResponse
                                .Success(
                                    calibrated.Value,
                                    beforeWeightsReceipt,
                                    afterCalibrationWeights.Value),
                            statusCode: StatusCodes.Status200OK);
                        primaryResultIsSuccess = true;
                    }
                }
            }
            else
            {
                var assessmentService =
                    new CodeDocumentationSavedModelAssessmentService();
                var assessed = await assessmentService
                    .AssessAsync(assessmentRequest, cancellationToken)
                    .ConfigureAwait(false);
                var afterAssessmentWeights =
                    await CaptureUnchangedSavedWeightsAsync()
                        .ConfigureAwait(false);
                if (!afterAssessmentWeights.IsSuccess ||
                    afterAssessmentWeights.Value is null)
                {
                    primaryResult = Error(
                        StatusCodes.Status409Conflict,
                        afterAssessmentWeights);
                }
                else if (!assessed.IsSuccess || assessed.Value is null)
                {
                    primaryResult = Error(StatusCodes.Status409Conflict, assessed);
                }
                else
                {
                    primaryResult = Results.Json(
                        CodeDocumentationSavedModelAssessmentHttpResponse
                            .Success(
                                assessed.Value,
                                beforeWeightsReceipt,
                                afterAssessmentWeights.Value),
                        statusCode: StatusCodes.Status200OK);
                    primaryResultIsSuccess = true;
                }
            }
        }
        catch (Exception exception)
        {
            primaryException = ExceptionDispatchInfo.Capture(exception);
        }

        Exception? disposalException = null;
        try
        {
            ownedClient.Dispose();
        }
        catch (Exception exception)
        {
            disposalException = exception;
        }

        if (primaryException is not null)
        {
            if (disposalException is not null &&
                !SnapshotExceptionGraphClassifier.IsNonFatal(disposalException))
            {
                throw new AggregateException(
                    primaryException.SourceException,
                    disposalException);
            }

            primaryException.Throw();
            return null!;
        }

        if (disposalException is not null)
        {
            if (!SnapshotExceptionGraphClassifier.IsNonFatal(disposalException))
            {
                ExceptionDispatchInfo.Capture(disposalException).Throw();
                return null!;
            }

            if (primaryResultIsSuccess)
            {
                return Results.Json(
                    CodeDocumentationSavedModelAssessmentHttpResponse.Failure(
                        runCalibration
                            ? CodeDocumentationSavedModelAssessmentCalibrationService
                                .CalibrationConditionMismatchErrorCode
                            : CodeDocumentationSavedModelAssessmentService
                                .EvaluationFailedErrorCode,
                        "Оценка сохранённой модели документации не завершена: клиент проверяющего не удалось корректно освободить."),
                    statusCode: StatusCodes.Status409Conflict);
            }
        }

        return primaryResult!;

        async Task<OperationResult<SavedWeightsReceipt>>
            CaptureUnchangedSavedWeightsAsync()
        {
            OperationResult<SavedWeightsReceipt> afterWeights;
            try
            {
                afterWeights = await CaptureSavedWeightsAsync(
                    repositoryRoot,
                    parent.RepositoryRelativeWeightsPath,
                    parent.WeightsSha256,
                    parent.WeightsByteLength,
                    startRequest.MaximumCheckpointBytes,
                    cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                return OperationResult<SavedWeightsReceipt>.Fail(
                    "Оценка сохранённой модели документации отменена до финальной проверки неизменности весов; результат качества не принят.",
                    CodeDocumentationSavedModelAssessmentService
                        .CancelledErrorCode);
            }

            if (!afterWeights.IsSuccess || afterWeights.Value is null)
                return afterWeights;

            if (!SavedWeightsMatch(beforeWeightsReceipt, afterWeights.Value))
            {
                return OperationResult<SavedWeightsReceipt>.Fail(
                    "Оценка остановлена: сохранённые байты весов изменились во время проверки.",
                    SavedWeightsChangedErrorCode);
            }

            return afterWeights;
        }
    }

    private static OperationResult<bool> CalibrationCancelled()
        => OperationResult<bool>.Fail(
            "Серия оценки сохранённой модели документации отменена до выдачи полного паспорта; результат качества не принят.",
            CodeDocumentationSavedModelAssessmentService.CancelledErrorCode);

    private static OperationResult<bool> ValidateRequest(
        CodeDocumentationSavedModelAssessmentHttpRequest? request,
        DateTimeOffset observedAtUtc)
    {
        if (request is null ||
            !IsBoundedRequiredText(
                request.DomainSlug,
                MaximumCoordinateTextLength) ||
            !IsBoundedRequiredText(
                request.DesignPatternSlug,
                MaximumCoordinateTextLength) ||
            !IsBoundedRequiredText(
                request.AssessmentRunId,
                MaximumAssessmentRunIdLength) ||
            !AreSafeEvidenceRefs(request.ProvenanceRefs) ||
            observedAtUtc == default ||
            observedAtUtc.Offset != TimeSpan.Zero)
        {
            return InvalidRequest();
        }

        return OperationResult<bool>.Ok(true);
    }

    internal static OperationResult<RepositoryOwnedReferenceCorpusOpenResult>
        OpenConfiguredCorpus(
            string repositoryRoot,
            CodeDocumentationSavedModelAssessmentProfile profile)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateReferenceCorpusAddress(
                repositoryRoot,
                profile.DomainSlug,
                profile.SubjectSlug,
                profile.CorpusVersion);
        if (!address.IsSuccess || address.Value is null)
            return address.MapError<RepositoryOwnedReferenceCorpusOpenResult>();

        var opened =
            RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(
                address.Value);
        if (!opened.IsSuccess || opened.Value is null)
            return opened;

        if (!StringComparer.Ordinal.Equals(
                opened.Value.Manifest.ManifestSha256,
                profile.ExpectedCorpusManifestSha256) ||
            !StringComparer.Ordinal.Equals(
                opened.Value.Manifest.Domain,
                profile.DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                opened.Value.Manifest.Subject,
                profile.SubjectSlug) ||
            !StringComparer.Ordinal.Equals(
                opened.Value.Manifest.CorpusVersion,
                profile.CorpusVersion) ||
            !StringComparer.Ordinal.Equals(
                opened.Value.Corpus.DomainTag,
                profile.DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                opened.Value.Corpus.SubjectId,
                profile.SubjectSlug) ||
            !StringComparer.Ordinal.Equals(
                opened.Value.Corpus.CorpusVersion,
                profile.CorpusVersion))
        {
            return OperationResult<RepositoryOwnedReferenceCorpusOpenResult>.Fail(
                "Запечатанный корпус не совпал с серверным профилем оценки сохранённой модели документации.",
                CorpusMismatchErrorCode);
        }

        return opened;
    }

    private static OperationResult<RepositoryOwnedReferenceCorpusOpenResult>
        OpenConfiguredCorpus(
            string repositoryRoot,
            CodeDocumentationCertificationProfile profile)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateReferenceCorpusAddress(
                repositoryRoot,
                profile.DomainSlug,
                profile.SubjectSlug,
                profile.CorpusVersion);
        if (!address.IsSuccess || address.Value is null)
            return address.MapError<RepositoryOwnedReferenceCorpusOpenResult>();

        var opened = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(
            address.Value);
        if (!opened.IsSuccess || opened.Value is null)
            return opened;

        if (!StringComparer.Ordinal.Equals(
                opened.Value.Manifest.ManifestSha256,
                profile.ExpectedCorpusManifestSha256) ||
            !StringComparer.Ordinal.Equals(
                opened.Value.Manifest.Domain,
                profile.DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                opened.Value.Manifest.Subject,
                profile.SubjectSlug) ||
            !StringComparer.Ordinal.Equals(
                opened.Value.Manifest.CorpusVersion,
                profile.CorpusVersion) ||
            !StringComparer.Ordinal.Equals(
                opened.Value.Corpus.DomainTag,
                profile.DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                opened.Value.Corpus.SubjectId,
                profile.SubjectSlug) ||
            !StringComparer.Ordinal.Equals(
                opened.Value.Corpus.CorpusVersion,
                profile.CorpusVersion))
        {
            return OperationResult<RepositoryOwnedReferenceCorpusOpenResult>.Fail(
                "Запечатанный корпус финальной сертификации не совпал с server-owned профилем.",
                CorpusMismatchErrorCode);
        }

        return opened;
    }

    private static CodeDocumentationSavedModelAssessmentModelPassport
        CreatePassport(
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
            maximumModelSerializationBytes);

    private static IReadOnlyList<string> BuildEvidenceRefs(
        CodeDocumentationSavedModelAssessmentHttpRequest request,
        IReadOnlyList<string> serverEvidenceRefs,
        IReadOnlyList<string> startEvidenceRefs,
        RepositoryOwnedReferenceCorpusManifest corpusManifest)
    {
        var refs = new List<string>(
            request.ProvenanceRefs.Count +
            serverEvidenceRefs.Count +
            startEvidenceRefs.Count +
            4)
        {
            "modelserver-route:" + EndpointPath,
            "modelserver-assessment-run:" + request.AssessmentRunId,
            "reference_corpus_manifest:" +
            corpusManifest.RepositoryRelativeManifestPath,
        };
        refs.AddRange(request.ProvenanceRefs);
        refs.AddRange(serverEvidenceRefs);
        refs.AddRange(startEvidenceRefs);
        return Array.AsReadOnly(refs.ToArray());
    }

    private static async Task<OperationResult<SavedWeightsReceipt>>
        CaptureSavedWeightsAsync(
        string repositoryRoot,
        string repositoryRelativeWeightsPath,
        string expectedSha256,
        long expectedByteLength,
        long maximumByteLength,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!IsSafeRepositoryRelativePath(repositoryRelativeWeightsPath) ||
                !IsHexSha256(expectedSha256) ||
                expectedByteLength <= 0 ||
                maximumByteLength <= 0 ||
                expectedByteLength > maximumByteLength ||
                !SecureLocalFileBoundary.IsSupportedPlatform())
            {
                return SavedWeightsUnavailable();
            }

            var path = Path.GetFullPath(Path.Combine(
                repositoryRoot,
                repositoryRelativeWeightsPath.Replace(
                    '/',
                    Path.DirectorySeparatorChar)));
            var root = Path.GetFullPath(repositoryRoot).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            if (!path.StartsWith(
                    root + Path.DirectorySeparatorChar,
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal) ||
                SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    path) ||
                !File.Exists(path))
            {
                return SavedWeightsUnavailable();
            }

            var declaredLength = new FileInfo(path).Length;
            if (declaredLength != expectedByteLength ||
                declaredLength > maximumByteLength)
            {
                return SavedWeightsUnavailable();
            }

            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    path) ||
                SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    path) ||
                stream.Length != declaredLength)
            {
                return SavedWeightsUnavailable();
            }

            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            var buffer = new byte[81920];
            long total = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var read = await stream
                    .ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)
                    .ConfigureAwait(false);
                if (read == 0)
                    break;
                if (read > expectedByteLength - total)
                    return SavedWeightsUnavailable();

                total += read;
                hash.AppendData(buffer.AsSpan(0, read));
            }

            var actualSha256 = Convert
                .ToHexString(hash.GetHashAndReset())
                .ToLowerInvariant();
            if (total != expectedByteLength ||
                !StringComparer.Ordinal.Equals(actualSha256, expectedSha256) ||
                !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    path) ||
                SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    path) ||
                stream.Length != total)
            {
                return SavedWeightsUnavailable();
            }

            return OperationResult<SavedWeightsReceipt>.Ok(
                new SavedWeightsReceipt(actualSha256, total));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return SavedWeightsUnavailable();
        }
    }

    private static OperationResult<SavedWeightsReceipt> SavedWeightsUnavailable()
        => OperationResult<SavedWeightsReceipt>.Fail(
            "Сохранённые байты весов не совпали с принятой версией модели.",
            SavedWeightsChangedErrorCode);

    private static bool SavedWeightsMatch(
        SavedWeightsReceipt before,
        SavedWeightsReceipt after)
        => before.ByteLength == after.ByteLength &&
           StringComparer.Ordinal.Equals(before.Sha256, after.Sha256);

    internal static bool RepositoryRootIsCanonical(string repositoryRoot)
    {
        try
        {
            if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
                return false;

            var expectedRoot = OperatingSystem.IsWindows()
                ? Program.RepositoryOwnedTrainingTrustedRepositoryRoot
                : WslRepositoryOwnedTrainingTrustedRepositoryRoot;
            var comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
            return string.Equals(
                NormalizeRepositoryRootForComparison(repositoryRoot),
                NormalizeRepositoryRootForComparison(expectedRoot),
                comparison);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static string NormalizeRepositoryRootForComparison(string path)
        => Path.GetFullPath(path).TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);

    private static bool AreSafeEvidenceRefs(IReadOnlyList<string>? evidenceRefs)
        => evidenceRefs is not null &&
           evidenceRefs.Count > 0 &&
           evidenceRefs.Count <= MaximumProvenanceRefCount &&
           evidenceRefs.All(IsSafeEvidenceRef);

    private static bool IsSafeEvidenceRef(string? value)
        => IsBoundedRequiredText(value, MaximumEvidenceRefLength) &&
           !value!.StartsWith("/", StringComparison.Ordinal) &&
           !value.StartsWith("\\", StringComparison.Ordinal) &&
           !value.Contains('\\', StringComparison.Ordinal) &&
           !value.Contains("/mnt/", StringComparison.OrdinalIgnoreCase) &&
           !value.Contains("file://", StringComparison.OrdinalIgnoreCase) &&
           !ContainsDriveRoot(value);

    private static bool IsBoundedRequiredText(string? value, int maximumLength)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= maximumLength &&
           StringComparer.Ordinal.Equals(value, value.Trim()) &&
           !value.Any(char.IsControl);

    private static bool IsSafeRepositoryRelativePath(string? path)
    {
        if (!IsBoundedRequiredText(path, MaximumEvidenceRefLength) ||
            path!.StartsWith("/", StringComparison.Ordinal) ||
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
                   segment is not "." and not "..");
    }

    private static bool ContainsDriveRoot(string value)
    {
        for (var index = 0; index + 2 < value.Length; index++)
        {
            if (char.IsAsciiLetter(value[index]) &&
                value[index + 1] == ':' &&
                value[index + 2] is '\\' or '/')
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsHexSha256(string? value)
        => value is { Length: 64 } &&
           value.All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static OperationResult<bool> InvalidRequest()
        => OperationResult<bool>.Fail(
            "Запрос оценки сохранённой модели документации некорректен.",
            InvalidRequestErrorCode);

    private static IResult Error<T>(
        int statusCode,
        OperationResult<T> result)
        => Results.Json(
            CodeDocumentationSavedModelAssessmentHttpResponse.Failure(
                result.ErrorCode,
                result.ErrorMessage),
            statusCode: statusCode);

    private static IResult CertificationError(
        int statusCode,
        string? errorCode)
        => Results.Json(
            CodeDocumentationCertificationHttpResponse.Failure(errorCode),
            statusCode: statusCode);
}

internal sealed record SavedWeightsReceipt(string Sha256, long ByteLength);
