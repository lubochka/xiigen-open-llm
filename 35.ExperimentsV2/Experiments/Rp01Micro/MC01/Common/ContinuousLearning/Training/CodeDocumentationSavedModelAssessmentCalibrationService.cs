// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

/// <summary>
/// Сервер заранее считает, сколько раз проверяющий будет вызван в серии:
/// число одинаковых экзаменов умножается на число закрытых заданий.
/// Такой план не приходит из HTTP-запроса и должен быть принят до первого
/// обращения к проверяющей модели.
/// </summary>
public sealed record CodeDocumentationSavedModelAssessmentCalibrationPlan(
    int RepeatCount,
    int HeldOutCaseCount,
    int ExpectedEvaluationCallCount);

/// <summary>
/// Один полный повтор экзамена сохранённой модели. Здесь нет закрытых эталонов
/// и свободных объяснений проверяющего: наружу выходят только счётчики,
/// хэши условий и фактические идентификаторы обращений к проверяющему.
/// </summary>
public sealed record CodeDocumentationSavedModelAssessmentCalibrationObservation(
    string AssessmentRunId,
    int EvaluatedCaseCount,
    int PassedCaseCount,
    double CorrectFraction,
    string GenerationSettingsSha256,
    string ModelSerializationBeforeSha256,
    string ModelSerializationAfterSha256,
    IReadOnlyList<string> CaseIds,
    IReadOnlyList<string> StudentAnswerSha256,
    IReadOnlyList<string> ObservedEvaluationRequestIds)
{ // Retain every repeat, not only the representative assessment's answers.
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] // Default legacy JSON remains byte-shape compatible.
    public IReadOnlyList<CodeDocumentationPrivateArtifactReference>? PrivateRawAnswers { get; init; } // References remain private; full answers are never embedded in calibration output.
} // Keep the existing positional constructor unchanged.

/// <summary>
/// Паспорт разброса для оценки уже принятой сохранённой модели. Он строится
/// только из настоящих повторных экзаменов в тех же условиях и не принимает
/// готовые баллы от клиента.
/// </summary>
public sealed record CodeDocumentationSavedModelAssessmentCalibrationReport(
    bool IsSuccess,
    string HumanMessage,
    string CalibrationRunId,
    int RepeatCount,
    int HeldOutCaseCount,
    int ExpectedEvaluationCallCount,
    int ObservedEvaluationCallCount,
    string EvaluationSetId,
    string NoisePolicyIdentitySha256,
    HeldOutScorerIdentity ScorerIdentity,
    MeasurementNoisePassport MeasurementNoisePassport,
    IReadOnlyList<CodeDocumentationSavedModelAssessmentCalibrationObservation>
        Observations,
    IReadOnlyList<string> EvidenceRefs);

/// <summary>
/// Результат живого маршрута: первый одиночный экзамен остаётся подробной
/// карточкой ответов, а калибровка показывает порог правила для этой оценки.
/// </summary>
public sealed record CodeDocumentationSavedModelAssessmentCalibrationResult(
    CodeDocumentationSavedModelAssessmentReport RepresentativeAssessment,
    CodeDocumentationSavedModelAssessmentCalibrationReport Calibration);

/// <summary>
/// Повторяет уже существующий экзамен сохранённой модели документации и
/// передаёт реальные результаты в общую политику расчёта погрешности.
/// Сервис не меняет веса, не создаёт DPO-пары и не принимает решение об
/// улучшении после урока.
/// </summary>
public sealed class CodeDocumentationSavedModelAssessmentCalibrationService
{
    public const string CalibrationRequestInvalidErrorCode =
        "CODE_DOCUMENTATION_SAVED_MODEL_ASSESSMENT_CALIBRATION_REQUEST_INVALID";
    public const string CalibrationBudgetExceededErrorCode =
        "CODE_DOCUMENTATION_SAVED_MODEL_ASSESSMENT_CALIBRATION_BUDGET_EXCEEDED";
    public const string CalibrationConditionMismatchErrorCode =
        "CODE_DOCUMENTATION_SAVED_MODEL_ASSESSMENT_CALIBRATION_CONDITION_MISMATCH";

    public static readonly TimeSpan DefaultMaximumDuration =
        TimeSpan.FromMinutes(5);

    private const int MaximumAssessmentRunIdLength = 256;
    private const string MetricProfileId =
        "code-documentation-case-pass-fraction-v1";

    public static OperationResult<CodeDocumentationSavedModelAssessmentCalibrationPlan>
        PlanCalibration(
            FrozenReferenceCorpus? corpus,
            CodeDocumentationSavedModelAssessmentGenerationSettings? settings,
            ContinuousLearningTp1DpoConfiguration? evaluatorConfiguration,
            MeasurementNoisePolicyConfig? config = null)
    {
        config ??= MeasurementNoisePolicyConfig.Canonical;
        if (corpus is null ||
            settings is null ||
            evaluatorConfiguration is null ||
            config.RepeatCount < 2)
        {
            return InvalidPlan();
        }

        var heldOutCaseCount = corpus.Entries.Count(
            static entry => entry.Partition == FrozenReferencePartition.HeldOut);
        if (heldOutCaseCount == 0)
        {
            return OperationResult<CodeDocumentationSavedModelAssessmentCalibrationPlan>.Fail(
                "В запечатанном корпусе нет отложенных заданий для проверки сохранённой модели.",
                CodeDocumentationSavedModelAssessmentService.HeldOutMissingErrorCode);
        }

        if (heldOutCaseCount > settings.MaxHeldOutCases)
        {
            return OperationResult<CodeDocumentationSavedModelAssessmentCalibrationPlan>.Fail(
                "Серверный профиль разрешает меньше отложенных заданий, чем содержит запечатанный корпус.",
                CodeDocumentationSavedModelAssessmentService.HeldOutTooLargeErrorCode);
        }

        int expectedEvaluationCallCount;
        try
        {
            expectedEvaluationCallCount = checked(
                heldOutCaseCount * config.RepeatCount);
        }
        catch (OverflowException)
        {
            return BudgetExceeded();
        }

        if (expectedEvaluationCallCount <= 0 ||
            expectedEvaluationCallCount >
            evaluatorConfiguration.MaxTeacherCalls)
        {
            return BudgetExceeded();
        }

        return OperationResult<CodeDocumentationSavedModelAssessmentCalibrationPlan>.Ok(
            new CodeDocumentationSavedModelAssessmentCalibrationPlan(
                config.RepeatCount,
                heldOutCaseCount,
                expectedEvaluationCallCount));
    }

    public async Task<OperationResult<CodeDocumentationSavedModelAssessmentCalibrationResult>>
        CalibrateAsync(
            CodeDocumentationSavedModelAssessmentRequest? request,
            MeasurementNoisePolicyConfig? config = null,
            TimeSpan? maximumDuration = null,
            CancellationToken cancellationToken = default)
    {
        var validation = ValidateRequest(request, maximumDuration);
        if (!validation.IsSuccess)
            return validation.MapError<CodeDocumentationSavedModelAssessmentCalibrationResult>();

        config ??= MeasurementNoisePolicyConfig.Canonical;
        var noisePolicyIdentity = config.IdentitySha256;
        var plan = PlanCalibration(
            request!.Corpus,
            request.GenerationSettings,
            request.EvaluatorConfiguration,
            config);
        if (!plan.IsSuccess || plan.Value is null)
            return plan.MapError<CodeDocumentationSavedModelAssessmentCalibrationResult>();

        var failureStage = "calibration-initialization"; // Diagnostic stage contains no exam inputs or answers.
        try
        {
            using var deadline =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);
            var duration = ResolveMaximumDuration(maximumDuration, request.EvaluatorConfiguration);
            if (duration != Timeout.InfiniteTimeSpan)
                deadline.CancelAfter(duration);

            var singleAssessment =
                new CodeDocumentationSavedModelAssessmentService();
            var observations =
                new List<CodeDocumentationSavedModelAssessmentCalibrationObservation>(
                    plan.Value.RepeatCount);
            var repeatedScores = new List<double>(plan.Value.RepeatCount);
            var observedRequestIds = new HashSet<string>(StringComparer.Ordinal);
            CodeDocumentationSavedModelAssessmentReport? representative = null;

            for (var index = 0; index < plan.Value.RepeatCount; index++)
            {
                deadline.Token.ThrowIfCancellationRequested();
                var repeatRunId = BuildRepeatAssessmentRunId(
                    request.AssessmentRunId,
                    index + 1);
                failureStage = "assessment-repeat-" + (index + 1).ToString("D3", CultureInfo.InvariantCulture); // Identify the failing repeat without disclosing its request text.
                var assessed = await singleAssessment
                    .AssessAsync(
                        request with { AssessmentRunId = repeatRunId },
                        deadline.Token)
                    .ConfigureAwait(false);
                if (!assessed.IsSuccess || assessed.Value is null)
                    return assessed.MapError<CodeDocumentationSavedModelAssessmentCalibrationResult>();

                if (representative is null)
                {
                    representative = assessed.Value;
                }
                else if (!ConditionsMatch(representative, assessed.Value))
                {
                    return ConditionMismatch();
                }

                var requestIds = assessed.Value.Evaluator
                    .ObservedEvaluationRequestIds
                    .ToArray();
                if (requestIds.Length != assessed.Value.EvaluatedCaseCount ||
                    requestIds.Any(id => !observedRequestIds.Add(id)))
                {
                    return ConditionMismatch();
                }

                repeatedScores.Add(assessed.Value.CorrectFraction);
                observations.Add(ToObservation(assessed.Value) with { PrivateRawAnswers = request.PrivateAnswerCapture is null ? null : assessed.Value.Cases.Select(c => c.PrivateRawAnswer!).ToArray() }); // Preserve all case refs for every actual repeat without changing score semantics.
            }

            if (representative is null ||
                observedRequestIds.Count != plan.Value.ExpectedEvaluationCallCount)
            {
                return ConditionMismatch();
            }

            deadline.Token.ThrowIfCancellationRequested();

            failureStage = "calibration-passport"; // Distinguish completion construction from a failed single assessment.
            var scorerIdentity = BuildScorerIdentity(representative);
            var evaluationSetId = BuildEvaluationSetId(
                representative,
                noisePolicyIdentity);
            var passport = MeasurementNoisePolicy.BuildPassport(
                scorerIdentity,
                evaluationSetId,
                plan.Value.HeldOutCaseCount,
                repeatedScores,
                config,
                request.ObservedAtUtc);
            var report =
                new CodeDocumentationSavedModelAssessmentCalibrationReport(
                    IsSuccess: true,
                    HumanMessage: BuildHumanMessage(
                        plan.Value,
                        passport),
                    "saved-model-calibration:" + request.AssessmentRunId,
                    plan.Value.RepeatCount,
                    plan.Value.HeldOutCaseCount,
                    plan.Value.ExpectedEvaluationCallCount,
                    observedRequestIds.Count,
                    evaluationSetId,
                    noisePolicyIdentity,
                    scorerIdentity,
                    passport,
                    observations.ToArray(),
                    BuildCalibrationEvidenceRefs(
                        representative,
                        passport,
                        noisePolicyIdentity));
            return OperationResult<CodeDocumentationSavedModelAssessmentCalibrationResult>.Ok(
                new CodeDocumentationSavedModelAssessmentCalibrationResult(
                    representative,
                    report));
        }
        catch (OperationCanceledException)
        {
            return OperationResult<CodeDocumentationSavedModelAssessmentCalibrationResult>.Fail(
                "Серия оценки сохранённой модели документации отменена до полного набора повторов; паспорт погрешности не построен.",
                CodeDocumentationSavedModelAssessmentService.CancelledErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<CodeDocumentationSavedModelAssessmentCalibrationResult>.Fail(
                "Серия оценки сохранённой модели документации не смогла безопасно завершиться; паспорт погрешности не построен. " +
                DescribeFailureCause(failureStage, exception), // Retain bounded cause identity without exposing Exception.Message, paths or closed answers.
                CalibrationConditionMismatchErrorCode);
        }
    }

    private static string DescribeFailureCause(string stage, Exception exception)
    {
        var details = new StringBuilder("failureStage=" + stage); // Keep the stable error code while distinguishing the exception branch from condition refusal.
        Exception? cause = exception;
        for (var depth = 0; cause is not null && depth < 8; depth++) // Bound nested cause traversal independently of arbitrary exception payloads.
        {
            var type = cause.GetType().FullName ?? cause.GetType().Name; // Runtime type identity is diagnostic metadata, not private exception text.
            details.Append("; exception[").Append(depth).Append("]=").Append(type.AsSpan(0, Math.Min(type.Length, 256)))
                .Append("; HResult=0x").Append(cause.HResult.ToString("X8", CultureInfo.InvariantCulture)); // Preserve the actual HRESULT, including inner loader failures.
            cause = cause.InnerException;
        }
        if (cause is not null) details.Append("; inner-cause-chain-truncated"); // Do not claim an unbounded exception graph was reported.
        return details.ToString();
    }

    private static OperationResult<bool> ValidateRequest(
        CodeDocumentationSavedModelAssessmentRequest? request,
        TimeSpan? maximumDuration)
    {
        if (request is null ||
            !IsBoundedText(
                request.AssessmentRunId,
                MaximumAssessmentRunIdLength - "-repeat-000".Length) ||
            !IsValidMaximumDuration(maximumDuration, request.EvaluatorConfiguration))
        {
            return OperationResult<bool>.Fail(
                "Серия оценки требует серверный запрос и положительный срок либо явный профиль отмены вызывающим кодом.",
                CalibrationRequestInvalidErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    internal static bool IsValidMaximumDuration(TimeSpan? duration, ContinuousLearningTp1DpoConfiguration? configuration)
        => duration is null || duration > TimeSpan.Zero ||
           duration == Timeout.InfiniteTimeSpan && configuration?.EvaluationProfile == LocalQwenOllamaInferenceProfileAuthority.ManualEvaluation;

    internal static TimeSpan ResolveMaximumDuration(TimeSpan? duration, ContinuousLearningTp1DpoConfiguration configuration)
        => duration ?? (configuration.EvaluationDescriptor.CallerCancellationOnly ? Timeout.InfiniteTimeSpan : DefaultMaximumDuration);

    private static CodeDocumentationSavedModelAssessmentCalibrationObservation
        ToObservation(CodeDocumentationSavedModelAssessmentReport report)
        => new(
            report.AssessmentRunId,
            report.EvaluatedCaseCount,
            report.PassedCaseCount,
            report.CorrectFraction,
            report.GenerationSettingsSha256,
            report.ModelSerializationBeforeSha256,
            report.ModelSerializationAfterSha256,
            report.Cases.Select(static item => item.CaseId).ToArray(),
            report.Cases
                .Select(static item => item.StudentAnswerSha256)
                .ToArray(),
            report.Evaluator.ObservedEvaluationRequestIds.ToArray());

    // Admission compares the exam, not the accepted and trained model weights.
    // Actual response identity remains guarded during and after calibration.
    internal static bool BaselineMatchesCurrentExam(
        CodeDocumentationSavedModelAssessmentCalibrationResult? baseline,
        CodeDocumentationSavedModelAssessmentRequest currentRequest,
        MeasurementNoisePolicyConfig config,
        CodeDocumentationSavedModelAssessmentCalibrationPlan currentPlan)
    {
        if (baseline?.RepresentativeAssessment is not { } representative ||
            baseline.Calibration is not { } calibration ||
            representative.GenerationSettings is null ||
            representative.Evaluator is null ||
            representative.Cases is null ||
            calibration.Observations is null ||
            currentRequest?.Corpus is null ||
            currentRequest.CorpusManifest is null ||
            currentRequest.ModelPassport is null ||
            currentRequest.GenerationSettings is null ||
            currentRequest.EvaluatorConfiguration is null ||
            config is null || currentPlan is null)
        {
            return false;
        }

        var heldOut = currentRequest.Corpus.Entries
            .Where(static entry => entry.Partition == FrozenReferencePartition.HeldOut)
            .OrderBy(static entry => entry.EntryId, StringComparer.Ordinal)
            .ToArray();
        if (heldOut.Length == 0 ||
            heldOut.Length != currentPlan.HeldOutCaseCount ||
            currentPlan.RepeatCount != config.RepeatCount ||
            currentPlan.ExpectedEvaluationCallCount !=
                (long)heldOut.Length * config.RepeatCount ||
            representative.EvaluatedCaseCount != heldOut.Length ||
            representative.Cases.Count != heldOut.Length ||
            representative.Cases.Any(static item => item is null) ||
            calibration.RepeatCount != currentPlan.RepeatCount ||
            calibration.HeldOutCaseCount != heldOut.Length ||
            calibration.ExpectedEvaluationCallCount != currentPlan.ExpectedEvaluationCallCount ||
            calibration.ObservedEvaluationCallCount != currentPlan.ExpectedEvaluationCallCount ||
            calibration.Observations.Count != currentPlan.RepeatCount)
        {
            return false;
        }

        var caseIds = heldOut.Select(static entry => entry.EntryId).ToArray();
        var generationHash = CodeDocumentationSavedModelAssessmentService
            .ComputeGenerationSettingsSha256(currentRequest.GenerationSettings, heldOut);
        var profile = currentRequest.EvaluatorConfiguration.EvaluationProfile;
        // Expected server identity only: empty IDs do not attest any response.
        var evaluator = new CodeDocumentationSavedModelAssessmentEvaluatorReceipt(
            currentRequest.EvaluatorConfiguration.ModelId,
            currentRequest.EvaluatorConfiguration.ExpectedOllamaManifestDigest,
            currentRequest.EvaluatorConfiguration.ExpectedOllamaModelLayerDigest,
            profile.ProfileId,
            profile.ConfigurationSha256,
            Array.Empty<string>());
        var domain = currentRequest.Corpus.DomainTag;
        var pattern = currentRequest.ModelPassport.DesignPatternSlug;
        var version = currentRequest.Corpus.CorpusVersion;
        var manifestHash = currentRequest.CorpusManifest.ManifestSha256;
        var corpusHash = currentRequest.Corpus.CorpusSha256;
        var noiseIdentity = config.IdentitySha256;
        var requiredIdentities = new[]
        {
            domain, pattern, version, manifestHash, corpusHash, generationHash,
            evaluator.ExpectedModelId, evaluator.ExpectedOllamaManifestDigest,
            evaluator.ExpectedOllamaModelLayerDigest, evaluator.EvaluationProfileId,
            evaluator.EvaluationProfileConfigurationSha256, noiseIdentity,
            calibration.EvaluationSetId
        };
        if (requiredIdentities.Any(string.IsNullOrWhiteSpace) ||
            !StringComparer.Ordinal.Equals(representative.DomainSlug, domain) ||
            !StringComparer.Ordinal.Equals(representative.DesignPatternSlug, pattern) ||
            !StringComparer.Ordinal.Equals(representative.CorpusVersion, version) ||
            !StringComparer.Ordinal.Equals(representative.CorpusManifestSha256, manifestHash) ||
            !StringComparer.Ordinal.Equals(representative.CorpusSha256, corpusHash) ||
            !Equals(representative.GenerationSettings, currentRequest.GenerationSettings) ||
            !StringComparer.Ordinal.Equals(representative.GenerationSettingsSha256, generationHash) ||
            !StringComparer.Ordinal.Equals(calibration.NoisePolicyIdentitySha256, noiseIdentity) ||
            !EvaluatorMatches(representative.Evaluator, evaluator) ||
            !representative.Cases.Select(static item => item.CaseId)
                .SequenceEqual(caseIds, StringComparer.Ordinal) ||
            !calibration.Observations.All(observation =>
                observation is not null &&
                observation.EvaluatedCaseCount == heldOut.Length &&
                observation.CaseIds is not null &&
                observation.CaseIds.SequenceEqual(caseIds, StringComparer.Ordinal) &&
                StringComparer.Ordinal.Equals(observation.GenerationSettingsSha256, generationHash)))
        {
            return false;
        }

        var scorer = BuildCurrentScorerIdentity(domain, evaluator.ExpectedModelId, // Derive the current five-field scorer through the same owner used by full evaluator Fresh.
            evaluator.ExpectedOllamaManifestDigest, currentRequest.EvaluatorConfiguration);
        return MeasurementNoisePolicy.ScorerIdentitiesMatch(calibration.ScorerIdentity, scorer) &&
               StringComparer.Ordinal.Equals(calibration.EvaluationSetId,
                   BuildEvaluationSetId(domain, pattern, version, manifestHash, corpusHash,
                       generationHash, evaluator, noiseIdentity));
    }

    private static bool ConditionsMatch(
        CodeDocumentationSavedModelAssessmentReport expected,
        CodeDocumentationSavedModelAssessmentReport actual)
        => StringComparer.Ordinal.Equals(expected.DomainSlug, actual.DomainSlug) &&
           StringComparer.Ordinal.Equals(
               expected.DesignPatternSlug,
               actual.DesignPatternSlug) &&
           expected.EvaluatedCaseCount == actual.EvaluatedCaseCount &&
           StringComparer.Ordinal.Equals(
               expected.CorpusVersion,
               actual.CorpusVersion) &&
           StringComparer.Ordinal.Equals(
               expected.CorpusManifestSha256,
               actual.CorpusManifestSha256) &&
           StringComparer.Ordinal.Equals(
               expected.CorpusSha256,
               actual.CorpusSha256) &&
           Equals(expected.GenerationSettings, actual.GenerationSettings) &&
           StringComparer.Ordinal.Equals(
               expected.GenerationSettingsSha256,
               actual.GenerationSettingsSha256) &&
           StringComparer.Ordinal.Equals(
               expected.ModelSourceKind,
               actual.ModelSourceKind) &&
           StringComparer.Ordinal.Equals(
               expected.RepositoryRelativeModelIdentityPath,
               actual.RepositoryRelativeModelIdentityPath) &&
           StringComparer.Ordinal.Equals(
               expected.ModelIdentitySha256,
               actual.ModelIdentitySha256) &&
           StringComparer.Ordinal.Equals(
               expected.ModelManifestSha256,
               actual.ModelManifestSha256) &&
           StringComparer.Ordinal.Equals(
               expected.SavedWeightsSha256,
               actual.SavedWeightsSha256) &&
           expected.ModelSerializationBeforeByteLength ==
           actual.ModelSerializationBeforeByteLength &&
           expected.ModelSerializationAfterByteLength ==
           actual.ModelSerializationAfterByteLength &&
           StringComparer.Ordinal.Equals(
               expected.ModelSerializationBeforeSha256,
               actual.ModelSerializationBeforeSha256) &&
           StringComparer.Ordinal.Equals(
               expected.ModelSerializationAfterSha256,
               actual.ModelSerializationAfterSha256) &&
           EvaluatorMatches(expected.Evaluator, actual.Evaluator) &&
           SameCaseOrder(expected.Cases, actual.Cases);

    private static bool EvaluatorMatches(
        CodeDocumentationSavedModelAssessmentEvaluatorReceipt expected,
        CodeDocumentationSavedModelAssessmentEvaluatorReceipt actual)
        => StringComparer.Ordinal.Equals(
               expected.ExpectedModelId,
               actual.ExpectedModelId) &&
           StringComparer.Ordinal.Equals(
               expected.ExpectedOllamaManifestDigest,
               actual.ExpectedOllamaManifestDigest) &&
           StringComparer.Ordinal.Equals(
               expected.ExpectedOllamaModelLayerDigest,
               actual.ExpectedOllamaModelLayerDigest) &&
           StringComparer.Ordinal.Equals(
               expected.EvaluationProfileId,
               actual.EvaluationProfileId) &&
           StringComparer.Ordinal.Equals(
               expected.EvaluationProfileConfigurationSha256,
               actual.EvaluationProfileConfigurationSha256);

    private static bool SameCaseOrder(
        IReadOnlyList<CodeDocumentationSavedModelAssessmentCaseResult> expected,
        IReadOnlyList<CodeDocumentationSavedModelAssessmentCaseResult> actual)
    {
        if (expected.Count != actual.Count)
            return false;

        for (var index = 0; index < expected.Count; index++)
        {
            if (!StringComparer.Ordinal.Equals(
                    expected[index].CaseId,
                    actual[index].CaseId))
            {
                return false;
            }
        }

        return true;
    }

    internal static HeldOutScorerIdentity BuildCurrentScorerIdentity( // Own the exact current scorer tuple without exposing or duplicating the private metric identity.
        string domainSlug, string expectedModelId, string expectedOllamaManifestDigest,
        ContinuousLearningTp1DpoConfiguration? evaluatorConfiguration = null)
        => new(domainSlug, expectedModelId, expectedOllamaManifestDigest, // Preserve domain/model/manifest as the first three existing scorer fields.
            (evaluatorConfiguration?.EvaluationProfile ?? LocalQwenOllamaInferenceProfileAuthority.Evaluation).ProfileId, MetricProfileId);

    private static HeldOutScorerIdentity BuildScorerIdentity(
        CodeDocumentationSavedModelAssessmentReport representative)
        => new(
            representative.DomainSlug,
            representative.Evaluator.ExpectedModelId,
            representative.Evaluator.ExpectedOllamaManifestDigest,
            representative.Evaluator.EvaluationProfileId,
            MetricProfileId);

    private static string BuildEvaluationSetId(
        CodeDocumentationSavedModelAssessmentReport representative,
        string noisePolicyIdentity)
        => BuildEvaluationSetId(
            representative.DomainSlug,
            representative.DesignPatternSlug,
            representative.CorpusVersion,
            representative.CorpusManifestSha256,
            representative.CorpusSha256,
            representative.GenerationSettingsSha256,
            representative.Evaluator,
            noisePolicyIdentity);

    internal static string BuildEvaluationSetId(
        string domainSlug, string designPatternSlug, string corpusVersion,
        string corpusManifestSha256, string corpusSha256,
        string generationSettingsSha256,
        CodeDocumentationSavedModelAssessmentEvaluatorReceipt evaluator,
        string noisePolicyIdentity)
    {
        var canonical = string.Join(
            "\n",
            "code-documentation-saved-model-assessment-evaluation-set-v1",
            domainSlug,
            designPatternSlug,
            corpusVersion,
            corpusManifestSha256,
            corpusSha256,
            generationSettingsSha256,
            evaluator.ExpectedModelId,
            evaluator.ExpectedOllamaManifestDigest,
            evaluator.ExpectedOllamaModelLayerDigest,
            evaluator.EvaluationProfileId,
            evaluator.EvaluationProfileConfigurationSha256,
            MeasurementNoisePolicyConfig.IdentitySchema,
            noisePolicyIdentity,
            MetricProfileId);
        return "code-doc-assessment:" + Sha256HexOfText(canonical);
    }

    private static IReadOnlyList<string> BuildCalibrationEvidenceRefs(
        CodeDocumentationSavedModelAssessmentReport representative,
        MeasurementNoisePassport passport,
        string noisePolicyIdentity)
    {
        var refs = new List<string>(representative.EvidenceRefs.Count + 5)
        {
            "measurement-noise-policy:configured",
            "measurement-noise-policy-sha256:" + noisePolicyIdentity,
            "measurement-noise-repeat-count:" +
            passport.RepeatCount.ToString(
                CultureInfo.InvariantCulture),
            "measurement-noise-stddev-multiplier:" +
            passport.StandardDeviationMultiplier.ToString(
                CultureInfo.InvariantCulture),
            "representative-assessment-run:" +
            representative.AssessmentRunId,
        };
        refs.AddRange(representative.EvidenceRefs);
        return Array.AsReadOnly(refs.ToArray());
    }

    internal static string BuildRepeatAssessmentRunId( // Shared with same-intent recovery so original repeat addresses cannot drift.
        string baseRunId,
        int repeatNumber)
    {
        var suffix = "-repeat-" + repeatNumber.ToString("D3", CultureInfo.InvariantCulture);
        if (baseRunId.Length + suffix.Length <= MaximumAssessmentRunIdLength)
            return baseRunId + suffix;

        var hash = Sha256HexOfText(baseRunId)[..16];
        var maximumPrefixLength = MaximumAssessmentRunIdLength -
                                  suffix.Length -
                                  "-sha256-".Length -
                                  hash.Length;
        return baseRunId[..maximumPrefixLength] + "-sha256-" + hash + suffix;
    }

    private static string BuildHumanMessage(
        CodeDocumentationSavedModelAssessmentCalibrationPlan plan,
        MeasurementNoisePassport passport)
        => "Сохранённая модель документации проверена повторно без изменения весов и учебных пар: " +
           $"{plan.RepeatCount} одинаковых экзаменов по {plan.HeldOutCaseCount} закрытым заданиям. " +
           $"Средняя доля верных ответов {FormatPercent(passport.MeanScore)}%, " +
           $"по действующему правилу изменение не больше {FormatPercent(passport.NoiseFloor)} процентных пунктов не подтверждает улучшения. " +
           "Это паспорт исходной оценки, а не решение об улучшении после нового урока.";

    private static OperationResult<CodeDocumentationSavedModelAssessmentCalibrationPlan>
        InvalidPlan()
        => OperationResult<CodeDocumentationSavedModelAssessmentCalibrationPlan>.Fail(
            "Серия оценки сохранённой модели документации требует запечатанный корпус, серверные параметры генерации и серверный бюджет проверяющего.",
            CalibrationRequestInvalidErrorCode);

    private static OperationResult<CodeDocumentationSavedModelAssessmentCalibrationPlan>
        BudgetExceeded()
        => OperationResult<CodeDocumentationSavedModelAssessmentCalibrationPlan>.Fail(
            "Серия оценки сохранённой модели документации превысила серверный бюджет обращений к проверяющему до первого вызова.",
            CalibrationBudgetExceededErrorCode);

    private static OperationResult<CodeDocumentationSavedModelAssessmentCalibrationResult>
        ConditionMismatch()
        => OperationResult<CodeDocumentationSavedModelAssessmentCalibrationResult>.Fail(
            "Один из повторов оценки прошёл в других условиях или повторил идентификатор обращения к проверяющему; общий паспорт погрешности не построен.",
            CalibrationConditionMismatchErrorCode);

    private static bool IsBoundedText(string? value, int maximumLength)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= maximumLength &&
           StringComparer.Ordinal.Equals(value, value.Trim()) &&
           !value.Any(char.IsControl);

    private static string Sha256HexOfText(string text)
        => Convert
            .ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)))
            .ToLowerInvariant();

    private static string FormatPercent(double fraction)
        => (fraction * 100d).ToString("0.##", CultureInfo.InvariantCulture);
}
