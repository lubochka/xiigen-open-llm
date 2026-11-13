// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Tokenizer;
using TransformerModel;
using System.Globalization; // Format inherited repeat numbers with the calibration owner's invariant D3 contract.
using System.Security.Cryptography; // Hash only bounded run text when the inherited repeat suffix exceeds its limit.
using System.Text; // Encode bounded run identities without serializing models or completed graphs.

/// <summary>
/// Серверные входы решения о качестве для CodeDocumentation. HTTP-клиент не
/// приносит закрытый корпус, проверяющего, параметры генерации или правило
/// погрешности.
/// </summary>
public sealed record CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext(
    FrozenReferenceCorpus Corpus,
    RepositoryOwnedReferenceCorpusManifest CorpusManifest,
    BpeTokenizer Tokenizer,
    ILegalTeacherEvaluationInferenceClient EvaluationClient,
    ContinuousLearningTp1DpoConfiguration EvaluatorConfiguration,
    CodeDocumentationSavedModelAssessmentGenerationSettings GenerationSettings,
    MeasurementNoisePolicyConfig NoisePolicyConfig,
    TimeSpan MaximumDuration,
    DateTimeOffset ObservedAtUtc,
    IReadOnlyList<string> EvidenceRefs)
{ // Private retention is a runtime dependency, not client-supplied output coordinates.
    [System.Text.Json.Serialization.JsonIgnore] // Runtime dependency never enters legacy wire, source metadata JSON or caller payloads.
    public ICodeDocumentationPrivateAnswerCapture? PrivateAnswerCapture { get; init; } // Legacy quality contexts keep no new writes by default.
} // Both baseline and trial assessment requests receive the same configured private owner.

public sealed record CodeDocumentationRepositoryOwnedDpoTrainingQualityGatePlan(
    CodeDocumentationSavedModelAssessmentCalibrationPlan BaselinePlan,
    int ExpectedTotalEvaluationCallCount);

public sealed record CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality(
    CodeDocumentationSavedModelAssessmentCalibrationResult Calibration,
    int ExpectedEvaluationCallCount,
    int ObservedEvaluationCallCount);

public sealed record CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionEvidence(
    bool Accepted,
    string TrainingPairId,
    string Status,
    double BaselineMeanCorrectFraction,
    double TrialMeanCorrectFraction,
    double Difference,
    double NoiseFloor,
    int ExpectedEvaluationCallCount,
    int ObservedEvaluationCallCount,
    CodeDocumentationSavedModelAssessmentCalibrationReport BaselineCalibration,
    CodeDocumentationSavedModelAssessmentCalibrationReport TrialCalibration,
    IReadOnlyList<string> EvidenceRefs);

internal enum CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition // Describe the complete comparison without granting publication or replay authority.
{ // Keep the finite outcome vocabulary beside the quality owner.
    Improved, // Both calibrated floors prove a positive change in different weights.
    Regressed, // Both calibrated floors prove a negative change in different weights.
    Indistinguishable, // A complete exam does not prove a direction under both floors.
    IdenticalWeights, // Complete exams used byte-identical serialized model weights.
    InvalidOrIncomplete // Reserve invalid assessment classification for failures, never a successful negative.
} // End the internal comparison vocabulary.

internal sealed record CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult( // Retain the actual complete assessment graphs for the later same-history consumer.
    CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition Disposition, // State the comparison outcome after complete graph admission.
    CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality Baseline, // Preserve the original baseline object without another calibration.
    CodeDocumentationSavedModelAssessmentCalibrationResult Trial, // Preserve the single completed trial series and representative answers.
    CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionEvidence Evidence); // Reuse the existing comparison evidence, including both floor statuses.

/// <summary>
/// Повторяет обязательный экзамен до и после одного урока CodeDocumentation.
/// Этот сервис сам не обучает и не публикует: он только повторяет тот же
/// закрытый замер и классифицирует сдвиг действующей политикой погрешности.
/// </summary>
public sealed class CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
{
    public const string QualityGateInvalidErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_QUALITY_GATE_INVALID";
    public const string QualityGateBudgetExceededErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_QUALITY_GATE_BUDGET_EXCEEDED";
    public const string QualityGateAssessmentFailedErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_QUALITY_GATE_ASSESSMENT_FAILED";
    public const string QualityGateConditionMismatchErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_QUALITY_GATE_CONDITION_MISMATCH";
    public const string QualityGateNotImprovedErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_QUALITY_GATE_NOT_IMPROVED";
    public const string BaselineAlreadySufficientErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_QUALITY_BASELINE_ALREADY_SUFFICIENT";

    private const int MaximumAssessmentRunIdLength = 256;
    private const int MaximumEvidenceRefCount = 128;
    private const int MaximumEvidenceRefLength = 512;
    private const double FinalCertificationTargetCorrectFraction = 0.95d;

    internal static string DescribeFinalCertificationTargetCorrectFractionR()
        => FinalCertificationTargetCorrectFraction.ToString("R", CultureInfo.InvariantCulture);

    public static OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingQualityGatePlan>
        PlanQualityGate(
            FrozenReferenceCorpus? corpus,
            CodeDocumentationSavedModelAssessmentGenerationSettings? generationSettings,
            ContinuousLearningTp1DpoConfiguration? evaluatorConfiguration,
            MeasurementNoisePolicyConfig? noisePolicyConfig)
    {
        var baseline =
            CodeDocumentationSavedModelAssessmentCalibrationService
                .PlanCalibration(
                    corpus,
                    generationSettings,
                    evaluatorConfiguration,
                    noisePolicyConfig);
        if (!baseline.IsSuccess || baseline.Value is null)
        {
            return baseline.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingQualityGatePlan>();
        }

        int expectedTotalCalls;
        try
        {
            expectedTotalCalls =
                checked(baseline.Value.ExpectedEvaluationCallCount * 2);
        }
        catch (OverflowException)
        {
            return BudgetExceeded<
                CodeDocumentationRepositoryOwnedDpoTrainingQualityGatePlan>();
        }

        if (evaluatorConfiguration is null ||
            expectedTotalCalls <= 0 ||
            expectedTotalCalls > evaluatorConfiguration.MaxTeacherCalls)
        {
            return BudgetExceeded<
                CodeDocumentationRepositoryOwnedDpoTrainingQualityGatePlan>();
        }

        return OperationResult<
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGatePlan>.Ok(
            new CodeDocumentationRepositoryOwnedDpoTrainingQualityGatePlan(
                baseline.Value,
                expectedTotalCalls));
    }

    public async Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality>>
        CalibrateBaselineAsync(
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext?
                context,
            LlmModel? acceptedModel,
            CodeDocumentationSavedModelAssessmentModelPassport?
                acceptedPassport,
            string assessmentRunId,
            CancellationToken cancellationToken = default)
    {
        var validation = ValidateContext(context);
        if (!validation.IsSuccess)
            return validation.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality>();
        if (acceptedModel is null ||
            acceptedPassport is null ||
            !IsBoundedText(assessmentRunId, MaximumAssessmentRunIdLength) ||
            !StringComparer.Ordinal.Equals(
                acceptedPassport.ModelSourceKind,
                CodeDocumentationSavedModelAssessmentModelPassport
                    .AcceptedRepositoryCheckpointSourceKind))
        {
            return Invalid<
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality>();
        }

        var plan = PlanQualityGate(
            context!.Corpus,
            context.GenerationSettings,
            context.EvaluatorConfiguration,
            context.NoisePolicyConfig);
        if (!plan.IsSuccess || plan.Value is null)
            return plan.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality>();

        var request = BuildAssessmentRequest(
            context,
            acceptedModel,
            acceptedPassport,
            assessmentRunId);
        var calibrated = await new CodeDocumentationSavedModelAssessmentCalibrationService()
            .CalibrateAsync(
                request,
                context.NoisePolicyConfig,
                context.MaximumDuration,
                cancellationToken)
            .ConfigureAwait(false);
        if (!calibrated.IsSuccess || calibrated.Value is null)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality>.Fail(
                calibrated.ErrorMessage,
                string.IsNullOrWhiteSpace(calibrated.ErrorCode)
                    ? QualityGateAssessmentFailedErrorCode
                    : calibrated.ErrorCode);
        }

        return OperationResult<
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality>.Ok(
            new CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality(
                calibrated.Value,
                plan.Value.BaselinePlan.ExpectedEvaluationCallCount,
                calibrated.Value.Calibration.ObservedEvaluationCallCount));
    }

    public static OperationResult<bool> BaselineRequiresNewTrainingPair(
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality? baseline)
    {
        if (baseline is null ||
            baseline.Calibration?.Calibration?.MeasurementNoisePassport is null)
        {
            return Invalid<bool>();
        }

        var passport = baseline.Calibration.Calibration.MeasurementNoisePassport;
        if (!MeasurementNoisePolicy.TryValidatePassport(passport, out _))
            return Invalid<bool>();

        var comparison = MeasurementNoisePolicy.ClassifyDifference(
            passport.MeanScore - FinalCertificationTargetCorrectFraction,
            passport);
        if (comparison.DifferenceProven &&
            StringComparer.Ordinal.Equals(
                comparison.Status,
                MeasurementNoisePolicy.ImprovedStatus))
        {
            return OperationResult<bool>.Fail(
                "Исходная принятая модель уже выше целевой доли верных ответов с учётом действующей погрешности; новый урок не запускается, нужен отдельный итоговый экзамен.",
                BaselineAlreadySufficientErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    public async Task<OperationResult<
            CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionEvidence>> // Preserve the public quality gate result contract.
        EvaluateTrialAsync( // Adapt one detailed assessment to the existing improvement-only caller.
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext?
                context, // Forward the existing server-owned exam context.
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality? baseline, // Reuse the already measured baseline.
            LlmModel? trialModel, // Assess the caller's one trial model.
            CodeDocumentationSavedModelAssessmentModelPassport? trialPassport, // Preserve the caller's existing trial source passport.
            string assessmentRunId, // Preserve the trial run identity.
            string trainingPairId, // Preserve the one lesson pair identity.
            CancellationToken cancellationToken = default) // Keep the existing cancellation propagation.
    { // Translate the detailed result only after its single trial series.
        var detailed = await EvaluateTrialDetailedAsync(context, baseline, trialModel, // Invoke the quality owner once, without baseline remeasurement.
            trialPassport, assessmentRunId, trainingPairId, cancellationToken).ConfigureAwait(false); // Await the same calibration and cancellation path.
        if (!detailed.IsSuccess || detailed.Value is null) // Failed and incomplete exams remain failures to the public caller.
            return detailed.MapError<CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionEvidence>(); // Preserve the original error code and message.
        return MapCompletedTrial(detailed.Value);
    }

    internal static OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionEvidence>
        MapCompletedTrial(CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult detailed)
    {
        if (detailed.Disposition == CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Improved)
            return OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionEvidence>.Ok(detailed.Evidence);
        return NotImproved<CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionEvidence>( // Keep the legacy refusal for every complete valid negative.
            detailed.Disposition == CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.IdenticalWeights // Preserve the distinct existing identical-weight message.
                ? "Пробные веса совпали с принятой моделью; такой результат не считается улучшением." // Explain the actual serialized-weight identity.
                : "Пробная модель документации не показала убедительного улучшения по действующему правилу погрешности; принятая версия не изменена."); // Preserve the existing non-improvement message for other complete negatives.
    } // End the compatibility adapter.

    internal async Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>> // Expose descriptive internal evidence without changing the public permission contract.
        EvaluateTrialDetailedAsync( // Measure the trial once and retain both complete graphs.
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext? context, // Require the same server-owned exam inputs.
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality? baseline, // Accept the original already completed baseline series.
            LlmModel? trialModel, // Use the one existing trial checkpoint model.
            CodeDocumentationSavedModelAssessmentModelPassport? trialPassport, // Keep the existing trial-checkpoint source admission.
            string assessmentRunId, // Identify this trial calibration series.
            string trainingPairId, // Bind the evidence to the one existing lesson pair.
            CancellationToken cancellationToken = default) // Preserve the original calibration cancellation path.
    { // Admit full baseline evidence before issuing trial calls.
        var validation = ValidateContext(context); // Reuse the existing server-context validator.
        if (!validation.IsSuccess) // Reject invalid server inputs before planning or measurement.
            return validation.MapError<CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>(); // Preserve the existing context error.
        if (baseline is null ||
            baseline.Calibration is null ||
            trialModel is null ||
            trialPassport is null ||
            !IsBoundedText(assessmentRunId, MaximumAssessmentRunIdLength) ||
            !IsBoundedText(trainingPairId, MaximumEvidenceRefLength) ||
            !StringComparer.Ordinal.Equals(
                trialPassport.ModelSourceKind,
                CodeDocumentationSavedModelAssessmentModelPassport
                    .TrialTrainingAttemptCheckpointSourceKind))
        {
            return Invalid<CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>(); // Keep invalid source inputs outside complete negative outcomes.
        }

        var plan = PlanQualityGate(
            context!.Corpus,
            context.GenerationSettings,
            context.EvaluatorConfiguration,
            context.NoisePolicyConfig);
        if (!plan.IsSuccess || plan.Value is null)
            return plan.MapError<CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>(); // Preserve current plan and budget errors.

        var request = BuildAssessmentRequest(
            context,
            trialModel,
            trialPassport,
            assessmentRunId);
        var completeBaseline = ValidateCompleteCalibration(baseline.Calibration, request, // Check concrete observations rather than trusting baseline summary counters.
            context.NoisePolicyConfig, plan.Value.BaselinePlan, CodeDocumentationSavedModelAssessmentModelPassport.AcceptedRepositoryCheckpointSourceKind); // Admit accepted-source and intrinsic run consistency before trial effects.
        if (!completeBaseline.IsSuccess) // Fail incomplete or foreign baselines before any trial evaluation.
            return completeBaseline.MapError<CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>(); // Preserve invalid-passport versus graph-mismatch classification.
        if (baseline.ExpectedEvaluationCallCount != plan.Value.BaselinePlan.ExpectedEvaluationCallCount || // Require the original wrapper's planned count to match the current plan.
            baseline.ObservedEvaluationCallCount != plan.Value.BaselinePlan.ExpectedEvaluationCallCount) // Require the original wrapper's observed count to be complete.
            return ConditionMismatch<CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>(); // Do not issue calls for a mismatched original wrapper.

        var trial = await new CodeDocumentationSavedModelAssessmentCalibrationService()
            .CalibrateAsync(
                request,
                context.NoisePolicyConfig,
                context.MaximumDuration,
                cancellationToken)
            .ConfigureAwait(false);
        if (!trial.IsSuccess || trial.Value is null)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>.Fail( // Retain the trial calibration failure rather than constructing a negative result.
                trial.ErrorMessage,
                string.IsNullOrWhiteSpace(trial.ErrorCode)
                    ? QualityGateAssessmentFailedErrorCode
                    : trial.ErrorCode);
        }

        return CompareCompletedTrialDetailed(request, context.NoisePolicyConfig, baseline, trial.Value, trainingPairId); // Classify the one successful trial through the same effect-free completed-graph owner.
    } // Finish measuring without a second comparison or calibration.

    internal static OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult> CompareCompletedTrialDetailed( // Describe admitted completed evidence without creating any new effect authority.
        CodeDocumentationSavedModelAssessmentRequest? currentTrialRequest, // Carry the genuinely constructed current exam and trial metadata without invoking its model or client.
        MeasurementNoisePolicyConfig? noisePolicyConfig, // Bind both complete series to the current configured uncertainty policy.
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality? baseline, // Preserve the original complete baseline wrapper and graph.
        CodeDocumentationSavedModelAssessmentCalibrationResult? trial, // Consume the sole already completed trial series.
        string trainingPairId) // Validate a bounded evidence label; durable pair provenance belongs to the later Session owner.
    { // Revalidate all current inputs and complete graphs before identity or scores.
        if (currentTrialRequest is not { Model: not null, Tokenizer: not null, ModelPassport: not null, Corpus: not null, CorpusManifest: not null, EvaluationClient: not null, EvaluatorConfiguration: not null, GenerationSettings: not null } || // Require genuine references while keeping this comparison effect free.
            noisePolicyConfig is null || currentTrialRequest.ObservedAtUtc == default || currentTrialRequest.ObservedAtUtc.Offset != TimeSpan.Zero || // Require the current configured policy and UTC observation time without inventing a measuring duration.
            !SafeEvidenceRefs(currentTrialRequest.EvidenceRefs) || !IsBoundedText(currentTrialRequest.AssessmentRunId, MaximumAssessmentRunIdLength - "-repeat-000".Length) || // Preserve safe evidence and the calibration owner's maximum root length.
            !IsBoundedText(trainingPairId, MaximumEvidenceRefLength) || // A malformed label cannot become successful descriptive evidence.
            !CurrentTrialPassportIsValid(currentTrialRequest.ModelPassport, currentTrialRequest.Corpus) || // Validate current source grammar independently of matching report fields.
            !CurrentCompletedExamMetadataIsValid(currentTrialRequest)) // Preserve the assessment owner's current settings and sealed-corpus metadata admission.
            return Invalid<CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>(); // Retain invalid-input failure instead of Ok InvalidOrIncomplete.
        var request = currentTrialRequest; // Use the existing metadata carrier without reconstructing or mutating it.
        var plan = PlanQualityGate(request.Corpus, request.GenerationSettings, request.EvaluatorConfiguration, noisePolicyConfig); // Reuse the existing checked two-series budget and planning errors.
        if (!plan.IsSuccess || plan.Value is null) // A rejected current plan cannot admit historical complete-looking evidence.
            return plan.MapError<CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>(); // Preserve the existing plan error category and message.
        var completeBaseline = ValidateCompleteCalibration(baseline?.Calibration, request, noisePolicyConfig, plan.Value.BaselinePlan, CodeDocumentationSavedModelAssessmentModelPassport.AcceptedRepositoryCheckpointSourceKind); // Recheck original source, every repeat, passport and full graph after any measuring await.
        if (!completeBaseline.IsSuccess) // Refuse an original whose arrays or passport changed during the trial series.
            return completeBaseline.MapError<CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>(); // Preserve invalid versus incomplete classification after the await.
        if (baseline!.ExpectedEvaluationCallCount != plan.Value.BaselinePlan.ExpectedEvaluationCallCount || baseline.ObservedEvaluationCallCount != plan.Value.BaselinePlan.ExpectedEvaluationCallCount) // Recheck original wrapper counts as well as the complete graph after the await.
            return ConditionMismatch<CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>(); // Reject incomplete original wrapper counts before classification.
        var completeTrial = ValidateCompleteCalibration(trial, request, noisePolicyConfig, plan.Value.BaselinePlan, CodeDocumentationSavedModelAssessmentModelPassport.TrialTrainingAttemptCheckpointSourceKind); // Apply the same complete graph admission to the already measured trial.
        if (!completeTrial.IsSuccess) // Keep incomplete trial evidence outside successful dispositions.
            return completeTrial.MapError<CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>(); // Preserve the exact graph or passport error category.
        var trialAssessment = trial!.RepresentativeAssessment; // Read only admitted trial report metadata, preserving the original report object.
        var modelPassport = request.ModelPassport; // Compare the fields that the actual trial report retains from its source passport.
        if (!StringComparer.Ordinal.Equals(trial.Calibration.CalibrationRunId, "saved-model-calibration:" + request.AssessmentRunId) || // Bind the full trial root to the current request before comparing scores.
            !StringComparer.Ordinal.Equals(trialAssessment.RepositoryRelativeModelIdentityPath, modelPassport.RepositoryRelativeModelIdentityPath) || // Preserve the actual trial checkpoint identity path.
            !StringComparer.Ordinal.Equals(trialAssessment.ModelIdentitySha256, modelPassport.ModelIdentitySha256) || // Bind the report to the current source identity hash.
            !StringComparer.Ordinal.Equals(trialAssessment.ModelManifestSha256, modelPassport.ManifestSha256) || // Require the actual manifest identity field to agree even when it is empty for a trial.
            !StringComparer.Ordinal.Equals(trialAssessment.SavedWeightsSha256, modelPassport.WeightsSha256) || // Bind the saved trial weights identity to this current request.
            !StringComparer.Ordinal.Equals(trialAssessment.ModelSerializationBeforeSha256, modelPassport.WeightsSha256) || // Require the observed pre-exam serialization to match the saved current trial.
            !StringComparer.Ordinal.Equals(trialAssessment.ModelSerializationAfterSha256, modelPassport.WeightsSha256) || // Require unchanged observed serialization after the completed trial.
            trialAssessment.ModelSerializationBeforeByteLength != modelPassport.WeightsByteLength || trialAssessment.ModelSerializationAfterByteLength != modelPassport.WeightsByteLength || // Bind both actual serialized lengths to the saved current trial length.
            !SameExamConditions(baseline.Calibration, trial)) // Preserve exact same-exam conditions for the two admitted graphs.
        { // A complete foreign source or run remains a failed comparison.
            return ConditionMismatch< // Keep source and exam mismatches outside complete dispositions.
                CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>(); // Preserve same-exam failure semantics.
        } // Finish current source and run binding.

        var observedCalls = baseline.ObservedEvaluationCallCount + // Count actual admitted original responses.
                            trial.Calibration.ObservedEvaluationCallCount; // Add only the already completed trial responses.
        if (observedCalls != plan.Value.ExpectedTotalEvaluationCallCount) // Require the actual two-series response total to match the current plan.
        { // Incomplete totals cannot produce descriptive success.
            return ConditionMismatch< // Preserve the existing total-count failure category.
                CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>(); // Reject disagreement with the total planned checker budget.
        } // Finish the complete response-total check.

        if (!RequestIdsAreUniqueAcrossSeries( // Reuse the existing concrete cross-series ID check without truncating repeats.
                baseline.Calibration, // Inspect every admitted original response ID.
                trial, plan.Value.ExpectedTotalEvaluationCallCount)) // Require full concrete ID count as well as cross-series uniqueness.
        { // Reused response IDs cannot attest separate completed exams.
            return ConditionMismatch< // Preserve the existing cross-series mismatch category.
                CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>(); // Reject reused response identities across the complete pair of exams.
        } // Finish full cross-series response admission.

        var baselineAssessment = // Read the admitted original representative without remeasuring or reconstructing it.
            baseline.Calibration.RepresentativeAssessment; // Retain the actual original model serialization identity.
        var identicalWeights = baselineAssessment.ModelSerializationBeforeByteLength == // Compare actual checkpoint serialization lengths after complete exam admission.
                trialAssessment.ModelSerializationBeforeByteLength && // Compare actual serialized lengths before deciding identical weights.
            StringComparer.Ordinal.Equals( // Preserve exact Ordinal SHA comparison for weight identity.
                baselineAssessment.ModelSerializationBeforeSha256, // Use the admitted original serialized hash.
                trialAssessment.ModelSerializationBeforeSha256); // Require actual serialized SHA equality for identical weights.

        var baselinePassport = // Retain the original validated uncertainty passport.
            baseline.Calibration.Calibration.MeasurementNoisePassport; // Use the actual original calibrated floor.
        var trialPassportNoise = // Retain the trial validated uncertainty passport.
            trial.Calibration.MeasurementNoisePassport; // Use the actual trial calibrated floor.
        var baselineMean = baseline.Calibration.Calibration.Observations.Average(static observation => observation.CorrectFraction); // Derive the baseline mean from the admitted completed observations.
        var trialMean = trial.Calibration.Observations.Average(static observation => observation.CorrectFraction); // Derive the trial mean from every admitted completed observation.
        var comparison = MeasurementNoisePolicy.ClassifyDifference( // Keep the existing sole statistical policy owner.
            trialMean - baselineMean, // Compare actual complete means without an epsilon or a caller score.
            baselinePassport); // Classify against the original calibrated floor.
        var trialComparison = MeasurementNoisePolicy.ClassifyDifference( // Apply that same existing policy to the other admitted passport.
            trialMean - baselineMean, // Preserve the identical complete-series difference for both floor checks.
            trialPassportNoise); // Classify against the actual trial calibrated floor.
        if (comparison.NoiseFloor is not { } baselineFloor || trialComparison.NoiseFloor is not { } trialFloor) // Never replace an absent calibrated floor with zero.
            return Invalid<CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>(); // Keep a lost passport outside every successful comparison.
        var effectiveNoiseFloor = Math.Max(baselineFloor, trialFloor); // Report the greater of the two actual validated calibrated floors.
        var disposition = identicalWeights // Give actual weight identity priority over any scripted score direction.
            ? CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.IdenticalWeights // A complete identical-weight exam remains descriptive, never accepted.
            : BothComparisonsProve(comparison, trialComparison, MeasurementNoisePolicy.ImprovedStatus) // Ask the unchanged policy whether both floors prove improvement.
                ? CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Improved // Admit positive direction only when both current policy comparisons prove it.
                : BothComparisonsProve(comparison, trialComparison, MeasurementNoisePolicy.RegressedStatus) // Ask the same policy whether both floors prove regression.
                    ? CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Regressed // Retain complete proven regression as a detailed negative.
                    : CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Indistinguishable; // Either blocking floor prevents a proven overall direction.
        var status = disposition switch // Bind overall evidence status to the actual complete disposition.
        { // Preserve the policy's existing direction and uncertainty vocabulary.
            CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Improved => MeasurementNoisePolicy.ImprovedStatus, // Publish only the agreed positive classification.
            CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Regressed => MeasurementNoisePolicy.RegressedStatus, // Report the agreed negative classification.
            CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.IdenticalWeights => "identical_weights", // Identify actual serialized-weight identity independently of scores.
            _ => MeasurementNoisePolicy.IndistinguishableStatus // Do not carry a lone improved comparison into overall evidence.
        }; // Finish the complete-result status binding.

        return OperationResult< // Return complete descriptive evidence without a receipt or publication capability.
            CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>.Ok( // Return a descriptive complete result for all four valid dispositions.
            new CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult(disposition, baseline, trial, // Preserve both original complete graph objects without reconstruction.
            new CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionEvidence( // Preserve the existing evidence contract and its ordered references.
                Accepted: disposition == CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Improved, // Preserve acceptance exclusively for proven improvement.
                trainingPairId, // Retain the validated bounded label without fabricating durable pair provenance.
                status, // Use the actual overall disposition while retaining both measured statuses below.
                baselineMean, // Report the complete original observation mean.
                trialMean, // Report the complete trial observation mean.
                comparison.Difference, // Preserve the existing policy's actual difference.
                effectiveNoiseFloor, // Report MAX of both present validated floors.
                plan.Value.ExpectedTotalEvaluationCallCount, // Retain the existing planned two-series budget.
                observedCalls, // Report actual complete concrete responses.
                baseline.Calibration.Calibration, // Preserve the original calibration report object.
                trial.Calibration, // Preserve the actual completed trial report object.
                BuildEvidenceRefs( // Build only existing safe descriptive refs in their original order.
                    request.EvidenceRefs, // Reuse the genuine current request's server evidence refs.
                    baseline.Calibration, // Retain the original calibration run reference.
                    trial, // Retain the single completed trial calibration run reference.
                    trainingPairId, // Retain the bounded evidence label.
                    comparison, // Preserve the actual original-floor comparison status.
                    trialComparison)))); // Preserve both floor classifications and the existing evidence references.
    } // Finish pure comparison without evaluating, calibrating, writing or serializing a model or graph.

    private static bool BothComparisonsProve(MeasurementNoiseComparison baseline, MeasurementNoiseComparison trial, string status) // Combine only classifications produced by the existing noise policy.
        => baseline.DifferenceProven && trial.DifferenceProven && // Require proof against both calibrated floors.
           StringComparer.Ordinal.Equals(baseline.Status, status) && StringComparer.Ordinal.Equals(trial.Status, status); // Require both actual policy directions to agree.

    private static OperationResult<bool> ValidateCompleteCalibration( // Admit complete observed evidence inside its current quality owner.
        CodeDocumentationSavedModelAssessmentCalibrationResult? result, // Accept only the existing complete calibration graph.
        CodeDocumentationSavedModelAssessmentRequest request, // Bind admission to the already planned current exam.
        MeasurementNoisePolicyConfig config, // Retain the current server-owned repeat and uncertainty settings.
        CodeDocumentationSavedModelAssessmentCalibrationPlan plan, // Use the existing bounded checker-call plan.
        string expectedSourceKind) // Bind original and trial graphs to their respective existing source kinds.
    { // Validate before identical weights or any successful negative result.
        if (result?.Calibration is not { } report || result.RepresentativeAssessment is not { } representative) // Missing graph roots cannot represent a complete exam.
            return ConditionMismatch<bool>(); // Keep missing graph evidence a condition mismatch.
        var passport = report.MeasurementNoisePassport; // Take the actual passport attached to this completed series.
        if (!MeasurementNoisePolicy.TryValidatePassport(passport, out _)) // Delegate all standalone statistical validation to the existing policy.
            return Invalid<bool>(); // Missing or internally invalid passports retain the required invalid error.
        if (!report.IsSuccess || !representative.IsSuccess || report.EvidenceRefs is null || representative.EvidenceRefs is null || // Require both reports to describe successful complete evidence.
            !CodeDocumentationSavedModelAssessmentCalibrationService.BaselineMatchesCurrentExam(result, request, config, plan) || // Reuse the current calibration owner's exact exam identity admission for either series.
            representative.Evaluator.ObservedEvaluationRequestIds is null || // Require the representative's concrete evaluator receipts.
            representative.ModelSerializationBeforeByteLength <= 0 || // Require a concrete serialized model rather than an empty fingerprint.
            representative.ModelSerializationBeforeByteLength != representative.ModelSerializationAfterByteLength || // Require the representative exam to preserve serialization length.
            string.IsNullOrWhiteSpace(representative.ModelSerializationBeforeSha256) || // Require a concrete serialization identity for this series.
            !StringComparer.Ordinal.Equals(representative.ModelSerializationBeforeSha256, representative.ModelSerializationAfterSha256)) // Require the representative exam to preserve its serialized weights.
            return ConditionMismatch<bool>(); // Reject incomplete evidence before comparing scores.
        const string calibrationPrefix = "saved-model-calibration:"; // Extract the source root rather than incorrectly bounding the wrapped calibration ID.
        if (!StringComparer.Ordinal.Equals(representative.ModelSourceKind, expectedSourceKind) || // Reject a wrong source before async trial effects as well as pure comparison.
            report.CalibrationRunId is null || !report.CalibrationRunId.StartsWith(calibrationPrefix, StringComparison.Ordinal)) // Require the actual calibration owner's root wrapper.
            return ConditionMismatch<bool>(); // A foreign source kind or malformed calibration root is not complete evidence.
        var rootRunId = report.CalibrationRunId[calibrationPrefix.Length..]; // Preserve the entire original root including valid roots whose wrapper exceeds 256 characters.
        if (!IsBoundedText(rootRunId, MaximumAssessmentRunIdLength - "-repeat-000".Length)) // Match the calibration owner's maximum 245-character source root exactly.
            return ConditionMismatch<bool>(); // Reject malformed intrinsic roots before any downstream trial call.
        var ids = new HashSet<string>(StringComparer.Ordinal); // Count actual unique checker identities in this series.
        var repeatNumber = 0; // Bind every completed repeat in its actual observed order.
        foreach (var observation in report.Observations) // Inspect every completed repeat, preserving its ordered graph.
        { // Require one concrete response per held-out case in every repeat.
            if (observation is null || observation.CaseIds is null || observation.StudentAnswerSha256 is null || observation.ObservedEvaluationRequestIds is null || // Reject missing observation members without trusting report counters.
                observation.CaseIds.Count != plan.HeldOutCaseCount || observation.StudentAnswerSha256.Count != plan.HeldOutCaseCount || // Require complete case and student-answer arrays.
                observation.ObservedEvaluationRequestIds.Count != plan.HeldOutCaseCount || // Require the actual response-ID array to contain every planned case.
                !StringComparer.Ordinal.Equals(observation.AssessmentRunId, BuildCompletedRepeatRunId(rootRunId, ++repeatNumber)) || // Bind even non-first repeats to the original source-derived run ID, including inherited large-repeat fallback.
                observation.PassedCaseCount < 0 || observation.PassedCaseCount > observation.EvaluatedCaseCount || // Require realizable pass counts for this complete exam.
                !double.IsFinite(observation.CorrectFraction) || observation.CorrectFraction != observation.PassedCaseCount / (double)observation.EvaluatedCaseCount || // Bind each finite score to its actual completed counters without a new numeric tolerance.
                observation.StudentAnswerSha256.Any(string.IsNullOrWhiteSpace) || // Require every generated answer to retain its concrete fingerprint.
                !StringComparer.Ordinal.Equals(observation.ModelSerializationBeforeSha256, representative.ModelSerializationBeforeSha256) || // Bind each repeat to this series' actual model identity.
                !StringComparer.Ordinal.Equals(observation.ModelSerializationAfterSha256, representative.ModelSerializationBeforeSha256) || // Require unchanged weights across every repeat.
                observation.ObservedEvaluationRequestIds.Any(id => string.IsNullOrWhiteSpace(id) || !ids.Add(id))) // Require nonempty Ordinal-unique concrete response identities.
                return ConditionMismatch<bool>(); // Reject incomplete or internally inconsistent observation graphs.
        } // Finish checking all repeats before trusting their aggregate.
        var first = report.Observations[0]; // Bind the retained detailed answer card to the actual first repeat.
        if (ids.Count != plan.ExpectedEvaluationCallCount || // Require concrete IDs to attain the planned series total.
            representative.PassedCaseCount != first.PassedCaseCount || representative.CorrectFraction != first.CorrectFraction || // Require representative score and counts to describe the first observation.
            representative.Cases.Count(static item => item.Passed) != first.PassedCaseCount || // Require the detailed answer results to agree with their pass counter.
            !StringComparer.Ordinal.Equals(representative.AssessmentRunId, first.AssessmentRunId) || // Keep representative and first repeat run identities aligned.
            !representative.Evaluator.ObservedEvaluationRequestIds.SequenceEqual(first.ObservedEvaluationRequestIds, StringComparer.Ordinal) || // Bind representative evaluator receipts to the concrete first-repeat responses.
            !representative.Cases.Select(static item => item.StudentAnswerSha256).SequenceEqual(first.StudentAnswerSha256, StringComparer.Ordinal) || // Bind representative answer fingerprints to the first-repeat graph.
            passport.RepeatCount != config.RepeatCount || passport.StandardDeviationMultiplier != config.StandardDeviationMultiplier || // Require the actual current server repeat and uncertainty settings.
            !passport.RepeatedScores.SequenceEqual(report.Observations.Select(static observation => observation.CorrectFraction)) || // Bind ordered passport scores to every completed observation.
            !MeasurementNoisePolicy.TryValidatePassportForMeasurement(passport, report.ScorerIdentity, report.EvaluationSetId, // Reuse the sole policy owner to bind the passport to this scorer and evaluation set.
                plan.HeldOutCaseCount, report.Observations.Average(static observation => observation.CorrectFraction), out _)) // Bind scored rows and actual completed mean through the existing policy.
            return ConditionMismatch<bool>(); // A standalone valid foreign passport does not attest this graph.
        return OperationResult<bool>.Ok(true); // Admit a full current-exam graph without issuing permission or persistence evidence.
    } // End the bounded complete-calibration admission.

    private static bool CurrentCompletedExamMetadataIsValid(CodeDocumentationSavedModelAssessmentRequest request) // Admit only metadata that the actual single-assessment owner accepts, without running it.
    { // Inspect current configured fields without serializing or generating a model.
        var settings = request.GenerationSettings; // Keep the existing generation settings object and its exact owner bounds.
        var manifest = request.CorpusManifest; // Retain the current sealed manifest object without reopening files.
        var corpus = request.Corpus; // Bind the manifest to the actual current corpus references.
        return settings.MaxNewTokens is > 0 and <= 2048 && settings.Temperature is >= 0f and <= 2f && float.IsFinite(settings.Temperature) && // Preserve the existing finite generation length and temperature constraints.
               settings.TopK is >= 0 and <= 10000 && settings.TopP is > 0f and <= 1f && float.IsFinite(settings.TopP) && // Preserve the existing top-k and finite top-p constraints.
               settings.MaxHeldOutCases is >= 1 and <= 200 && settings.MaxAnswerChars is >= 1 and <= 65536 && // Preserve the actual assessment owner's case and answer bounds without changing capacities.
               StringComparer.Ordinal.Equals(manifest.Domain, corpus.DomainTag) && StringComparer.Ordinal.Equals(manifest.Subject, corpus.SubjectId) && // Bind the current manifest's domain and subject to its actual corpus.
               StringComparer.Ordinal.Equals(manifest.CorpusVersion, corpus.CorpusVersion) && StringComparer.Ordinal.Equals(manifest.CorpusSha256, corpus.CorpusSha256) && // Preserve the current corpus version and content identity.
               manifest.EntryCount == corpus.Count && CompletedSourceSha256(manifest.ManifestSha256); // Require the existing complete entry count and valid sealed manifest SHA.
    } // Finish the current metadata-only exam admission.

    private static bool CurrentTrialPassportIsValid(CodeDocumentationSavedModelAssessmentModelPassport passport, FrozenReferenceCorpus corpus) // Preserve the assessment owner's current trial metadata admission without model or filesystem access.
        => IsBoundedText(passport.DomainSlug, 256) && IsBoundedText(passport.DesignPatternSlug, 256) && // Require the existing bounded domain and pattern identities.
           IsBoundedText(passport.ModelIdSlug, 256) && IsBoundedText(passport.ModelVersion, 256) && passport.CheckpointStep > 0 && // Require a concrete bounded model identity and positive checkpoint step.
           SafeCompletedSourcePath(passport.RepositoryRelativeWeightsPath) && CompletedSourceSha256(passport.WeightsSha256) && // Preserve the existing safe relative weights path and hexadecimal saved identity.
           passport.WeightsByteLength > 0 && passport.MaximumModelSerializationBytes >= passport.WeightsByteLength && // Require concrete saved bytes within the already configured serialization bound.
           StringComparer.Ordinal.Equals(passport.DomainSlug, corpus.DomainTag) && // Preserve the assessment owner's current corpus-domain source binding.
           StringComparer.Ordinal.Equals(passport.ModelSourceKind, CodeDocumentationSavedModelAssessmentModelPassport.TrialTrainingAttemptCheckpointSourceKind) && // Require the existing trial-attempt source kind.
           string.IsNullOrEmpty(passport.RepositoryRelativeManifestPath) && string.IsNullOrEmpty(passport.ManifestSha256) && // A trial source cannot carry an accepted-model manifest identity.
           SafeCompletedSourcePath(passport.RepositoryRelativeModelIdentityPath) && CompletedSourceSha256(passport.ModelIdentitySha256); // Reject malformed trial source metadata even if a cloned graph matches it.

    private static bool SafeCompletedSourcePath(string? path) // Reproduce the existing assessment owner's metadata-only relative path grammar.
        => IsBoundedText(path, MaximumEvidenceRefLength) && !path!.StartsWith("/", StringComparison.Ordinal) && // Require bounded trimmed source metadata below a relative root.
           !path.StartsWith("\\", StringComparison.Ordinal) && !path.Contains('\\', StringComparison.Ordinal) && // Preserve the source owner's forward-slash relative path format.
           !path.Contains("..", StringComparison.Ordinal) && !(path.Length >= 3 && path[1] == ':' && path[2] is '\\' or '/' && char.IsLetter(path[0])); // Preserve the assessment owner's exact traversal and leading-drive checks without imposing a new source policy.

    private static bool CompletedSourceSha256(string? sha256) // Validate only the existing source identity text, without hashing model bytes.
        => sha256 is { Length: 64 } && sha256.All(static c => c is >= '0' and <= '9' or >= 'a' and <= 'f'); // Preserve the assessment owner's lowercase hexadecimal SHA256 grammar.

    private static string BuildCompletedRepeatRunId(string rootRunId, int repeatNumber) // Reproduce the existing calibration owner's bounded run metadata without invoking calibration.
    { // Operate only on the admitted bounded root text.
        var suffix = "-repeat-" + repeatNumber.ToString("D3", CultureInfo.InvariantCulture); // Preserve the growing invariant D3 suffix from the actual producer.
        if (rootRunId.Length + suffix.Length <= MaximumAssessmentRunIdLength) // Keep the producer's direct root when the whole repeat ID fits.
            return rootRunId + suffix; // Preserve exact direct repeat IDs through the source's length boundary.
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rootRunId))).ToLowerInvariant()[..16]; // Hash only bounded root UTF8 text, retaining the producer's lowercase SHA16 fallback.
        var prefixLength = MaximumAssessmentRunIdLength - suffix.Length - "-sha256-".Length - hash.Length; // Match the producer's exact room for its inherited fallback prefix.
        return rootRunId[..prefixLength] + "-sha256-" + hash + suffix; // Preserve producer IDs from repeat 1000 onward without truncating observations.
    } // Finish metadata-only repeat identity construction.

    internal static CodeDocumentationSavedModelAssessmentRequest
        BuildAssessmentRequest(
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context,
            LlmModel model,
            CodeDocumentationSavedModelAssessmentModelPassport passport,
            string assessmentRunId)
        => new(
            model,
            context.Tokenizer,
            passport,
            context.Corpus,
            context.CorpusManifest,
            context.EvaluationClient,
            context.EvaluatorConfiguration,
            context.GenerationSettings,
            assessmentRunId,
            context.ObservedAtUtc,
            context.EvidenceRefs)
        { PrivateAnswerCapture = context.PrivateAnswerCapture }; // Propagate the same private retention dependency into every baseline/trial repeat.

    internal static bool SameExamConditions(
        CodeDocumentationSavedModelAssessmentCalibrationResult baseline,
        CodeDocumentationSavedModelAssessmentCalibrationResult trial)
    {
        var baselineReport = baseline.Calibration;
        var trialReport = trial.Calibration;
        var baselineRepresentative = baseline.RepresentativeAssessment;
        var trialRepresentative = trial.RepresentativeAssessment;
        return baselineReport.RepeatCount == trialReport.RepeatCount &&
               baselineReport.HeldOutCaseCount == trialReport.HeldOutCaseCount &&
               baselineReport.ExpectedEvaluationCallCount ==
               trialReport.ExpectedEvaluationCallCount &&
               StringComparer.Ordinal.Equals(
                   baselineReport.EvaluationSetId,
                   trialReport.EvaluationSetId) &&
               StringComparer.Ordinal.Equals(
                   baselineReport.NoisePolicyIdentitySha256,
                   trialReport.NoisePolicyIdentitySha256) &&
               StringComparer.Ordinal.Equals(
                   baselineRepresentative.DomainSlug,
                   trialRepresentative.DomainSlug) &&
               StringComparer.Ordinal.Equals(
                   baselineRepresentative.DesignPatternSlug,
                   trialRepresentative.DesignPatternSlug) &&
               StringComparer.Ordinal.Equals(
                   baselineRepresentative.CorpusVersion,
                   trialRepresentative.CorpusVersion) &&
               StringComparer.Ordinal.Equals(
                   baselineRepresentative.CorpusManifestSha256,
                   trialRepresentative.CorpusManifestSha256) &&
               StringComparer.Ordinal.Equals(
                   baselineRepresentative.CorpusSha256,
                   trialRepresentative.CorpusSha256) &&
               Equals(
                   baselineRepresentative.GenerationSettings,
                   trialRepresentative.GenerationSettings) &&
               StringComparer.Ordinal.Equals(
                   baselineRepresentative.GenerationSettingsSha256,
                   trialRepresentative.GenerationSettingsSha256) &&
               MeasurementNoisePolicy.ScorerIdentitiesMatch(
                   baselineReport.ScorerIdentity,
                   trialReport.ScorerIdentity);
    }

    private static bool RequestIdsAreUniqueAcrossSeries(
        CodeDocumentationSavedModelAssessmentCalibrationResult baseline,
        CodeDocumentationSavedModelAssessmentCalibrationResult trial, int expectedTotal) // Require the concrete two-series checker total in addition to uniqueness.
    {
        var requestIds = new HashSet<string>(StringComparer.Ordinal);
        return baseline.Calibration.Observations
                   .SelectMany(static observation =>
                       observation.ObservedEvaluationRequestIds)
                   .Concat(trial.Calibration.Observations.SelectMany(
                       static observation =>
                           observation.ObservedEvaluationRequestIds))
                   .All(requestIds.Add) && requestIds.Count == expectedTotal; // Prove complete actual IDs across both already validated series.
    }

    private static IReadOnlyList<string> BuildEvidenceRefs(
        IReadOnlyList<string> evidenceRefs, // Accept the existing current request's evidence without reconstructing a measuring context.
        CodeDocumentationSavedModelAssessmentCalibrationResult baseline,
        CodeDocumentationSavedModelAssessmentCalibrationResult trial,
        string trainingPairId,
        MeasurementNoiseComparison baselineComparison,
        MeasurementNoiseComparison trialComparison)
    {
        var refs = new List<string>(evidenceRefs.Count + 10) // Preserve existing reference capacity and ordered descriptive contents.
        {
            "code-documentation-quality-gate:baseline-calibrated",
            "code-documentation-quality-gate:trial-calibrated",
            "code-documentation-quality-gate:single-new-pair:" +
            trainingPairId,
            "measurement-noise-status:baseline-floor:" +
            baselineComparison.Status,
            "measurement-noise-status:trial-floor:" +
            trialComparison.Status,
            "baseline-calibration-run:" +
            baseline.Calibration.CalibrationRunId,
            "trial-calibration-run:" +
            trial.Calibration.CalibrationRunId,
        };
        refs.AddRange(evidenceRefs); // Append current server refs in the same order as the legacy measuring result.
        return Array.AsReadOnly(refs.ToArray());
    }

    internal static OperationResult<bool> ValidateContext(
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext? context)
        => context is not null &&
           context.Corpus is not null &&
           context.CorpusManifest is not null &&
           context.Tokenizer is not null &&
           context.EvaluationClient is not null &&
           context.EvaluatorConfiguration is not null &&
           context.GenerationSettings is not null &&
           context.NoisePolicyConfig is not null &&
           CodeDocumentationSavedModelAssessmentCalibrationService.IsValidMaximumDuration(context.MaximumDuration, context.EvaluatorConfiguration) &&
           context.ObservedAtUtc != default &&
           context.ObservedAtUtc.Offset == TimeSpan.Zero &&
           SafeEvidenceRefs(context.EvidenceRefs)
            ? OperationResult<bool>.Ok(true)
            : Invalid<bool>();

    private static bool SafeEvidenceRefs(IReadOnlyList<string>? evidenceRefs)
        => evidenceRefs is not null &&
           evidenceRefs.Count > 0 &&
           evidenceRefs.Count <= MaximumEvidenceRefCount &&
           evidenceRefs.All(static value =>
               IsBoundedText(value, MaximumEvidenceRefLength) &&
               !value!.StartsWith("/", StringComparison.Ordinal) &&
               !value.StartsWith("\\", StringComparison.Ordinal) &&
               !value.Contains('\\', StringComparison.Ordinal) &&
               !value.Contains("/mnt/", StringComparison.OrdinalIgnoreCase) &&
               !value.Contains("file://", StringComparison.OrdinalIgnoreCase) &&
               !ContainsDriveRoot(value));

    private static bool IsBoundedText(string? value, int maximumLength)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= maximumLength &&
           StringComparer.Ordinal.Equals(value, value.Trim()) &&
           !value.Any(char.IsControl);

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

    private static OperationResult<T> Invalid<T>()
        => OperationResult<T>.Fail(
            "CodeDocumentation repository-owned DPO quality gate requires server-owned held-out corpus, checker, generation settings, noise policy, UTC observation time, and evidence.",
            QualityGateInvalidErrorCode);

    private static OperationResult<T> BudgetExceeded<T>()
        => OperationResult<T>.Fail(
            "CodeDocumentation repository-owned DPO quality gate exceeds the server checker-call budget before the first evaluation call.",
            QualityGateBudgetExceededErrorCode);

    private static OperationResult<T> ConditionMismatch<T>()
        => OperationResult<T>.Fail(
            "CodeDocumentation repository-owned DPO quality gate did not run the same held-out exam before and after the trial lesson.",
            QualityGateConditionMismatchErrorCode);

    private static OperationResult<T> NotImproved<T>(string message)
        => OperationResult<T>.Fail(message, QualityGateNotImprovedErrorCode);
}
