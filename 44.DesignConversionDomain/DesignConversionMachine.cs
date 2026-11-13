// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Collections.Immutable;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO;
using LoRA;
using TensorCore.Core;
using TransformerModel;
using TransformerModel.Config;

public sealed record DesignConversionLoadedLearnedWeightAuthoritySummary(
    bool HasRealLearnedWeightAuthority,
    bool DpoPolicyAuthorityLoaded,
    int EmbeddedLearnedWeightArtifactCount,
    int DpoPolicyLearnedWeightArtifactCount,
    int TotalLearnedWeightArtifactCount,
    int EmbeddedLearnedWeightObservationCount,
    int DpoPolicyLearnedWeightObservationCount,
    int TotalLearnedWeightObservationCount);

public sealed class DesignConversionMachine : ILearningMachine<DesignConversionDomain>
{
    public const string MachineId = "design-conversion-machine";
    public const string DomainTag = "DesignConversionDomain";
    public const int CheckpointSchemaVersion = 1;
    public const string RuntimeScaffoldEvidence = "DESIGN_CONVERSION_MACHINE_RUNTIME_SCAFFOLD";
    public const string CheckpointLifecycleEvidence = "DESIGN_CONVERSION_MACHINE_CHECKPOINT_LIFECYCLE";
    public const string LearnedWeightsPersistedEvidence = "DESIGN_CONVERSION_LOCAL_CHECKPOINT_FEATURE_WEIGHTS_PERSISTED_NOT_TRAINED_MODEL";
    public const string LearnedWeightsSanitizedFieldsEvidence = "DESIGN_CONVERSION_LOCAL_CHECKPOINT_FEATURE_WEIGHTS_DERIVED_FROM_SANITIZED_FIELDS_NOT_TRAINED_MODEL";
    public const string LearnedInferenceAppliedEvidence = "DESIGN_CONVERSION_RUNTIME_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_APPLIED_NOT_LEARNED_AUTHORITY";
    public const string LearnedInferenceNoRawPayloadEvidence = "DESIGN_CONVERSION_RUNTIME_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_NO_RAW_PAYLOAD_NOT_TRAINED_MODEL";
    public const string LearnedInferenceCriticAdjustmentEvidence = "DESIGN_CONVERSION_RUNTIME_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_CRITIC_NOT_TRAINED_MODEL";
    public const string LearnedInferenceImproverEvidence = "DESIGN_CONVERSION_RUNTIME_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_IMPROVER_NOT_TRAINED_MODEL";
    public const string LocalCheckpointFeatureAdjustmentEvidence = "DESIGN_CONVERSION_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_APPLIED_NOT_TRAINED_AUTHORITY";
    public const string DpoTrainerPolicyCheckpointFreshLoadedEvidence = "DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_FRESH_LOADED";
    public const string DpoTrainerPolicyCheckpointConvertedEvidence = "DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_CONVERTED_TO_RUNTIME_WEIGHTS";
    public const string DpoTrainerPolicyCheckpointRuntimeScoringEvidence = "DESIGN_CONVERSION_RUNTIME_DPO_TRAINER_POLICY_CHECKPOINT_CONVERTED_SCORING";
    public const string CheckpointWriteConflictCode = "CHECKPOINT_WRITE_CONFLICT";
    public const string ExportCheckpointNotBoundCode = "EXPORT_CHECKPOINT_NOT_BOUND";
    public const string ExportCheckpointFileMissingCode = "EXPORT_CHECKPOINT_FILE_MISSING";
    public const string ExportCheckpointHashMismatchCode = "EXPORT_CHECKPOINT_HASH_MISMATCH";
    public const string ExportCheckpointVersionStaleCode = "EXPORT_CHECKPOINT_VERSION_STALE";
    public const string ImportCheckpointMetadataMismatchCode = "IMPORT_CHECKPOINT_METADATA_MISMATCH";
    public const string CheckpointDpoPolicyRefInvalidCode = "CHECKPOINT_DPO_POLICY_REF_INVALID";
    public const string GradientBridgeInputEmptyCode = "DESIGN_CONVERSION_MACHINE_GRADIENT_BRIDGE_INPUT_EMPTY";
    public const string GradientBridgeFailedCode = "DESIGN_CONVERSION_MACHINE_GRADIENT_BRIDGE_FAILED";
    public const string GradientBridgeInvalidResultCode = "DESIGN_CONVERSION_MACHINE_GRADIENT_BRIDGE_INVALID_RESULT";
    public const string GradientBridgeVersionConflictCode = "DESIGN_CONVERSION_MACHINE_GRADIENT_BRIDGE_VERSION_CONFLICT";
    public const string RuntimeScaffoldNotTrainedModelWarning = "RUNTIME_SCAFFOLD_NOT_TRAINED_MODEL";
    public const string HeldOutAccuracyNotMeasuredEvidence = "DESIGN_CONVERSION_HELD_OUT_ACCURACY_NOT_MEASURED_DURING_TRAINBATCH";
    public const string LocalPreferenceFitSignalOnlyEvidence = "DESIGN_CONVERSION_LOCAL_PREFERENCE_FIT_SIGNAL_NOT_HELD_OUT_ACCURACY";
    public const string EvaluateAsyncHeldOutAccuracyMeasuredEvidence = "DESIGN_CONVERSION_HELD_OUT_ACCURACY_MEASURED_DURING_EVALUATE_ASYNC";
    public const string EvaluateAsyncSanitizedLocalCheckpointFeatureFitEvidence = "DESIGN_CONVERSION_EVALUATE_ASYNC_SANITIZED_LOCAL_CHECKPOINT_HELD_OUT_FEATURE_FIT_SCOPE";
    public const string EvaluateAsyncNotTrainedVisualQaAuthorityEvidence = "DESIGN_CONVERSION_EVALUATE_ASYNC_NOT_TRAINED_VISUAL_QA_AUTHORITY";
    public const string DpoPolicyCheckpointPayloadNotFoundCode = "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_PAYLOAD_NOT_FOUND";
    public const string DpoPolicyCheckpointPayloadLengthMismatchCode = "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_PAYLOAD_LENGTH_MISMATCH";
    public const string DpoPolicyCheckpointPayloadShaMismatchCode = "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_PAYLOAD_SHA_MISMATCH";
    public const string DpoPolicyCheckpointPayloadModelLoadFailedCode = "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_PAYLOAD_MODEL_LOAD_FAILED";
    public const string DpoPolicyRuntimeSchemaUnsupportedCode =
        "DESIGN_CONVERSION_DPO_POLICY_RUNTIME_SCHEMA_UNSUPPORTED";
    public const string DpoPolicyRuntimeSchemaUnsupportedMessage =
        "Direct DPO policy runtime scoring requires checkpoint training context schema 4.";
    public const string DpoPolicyDirectInferenceRuntimeBridgeKind = "dpo_policy_checkpoint_payload_file_sibling_validated_direct_policy_inference";
    public const string DpoPolicyLegacySiblingPayloadRuntimeBridgeKind = "dpo_policy_checkpoint_payload_file_sibling_validated_not_direct_policy_inference";
    public const string DpoPolicyIdentityMetadataRuntimeBridgeKind = "dpo_policy_checkpoint_identity_metadata_not_direct_policy_inference";
    public const string DpoPolicyDirectInferenceConfiguredEvidence = "DESIGN_CONVERSION_DPO_POLICY_DIRECT_RUNTIME_INFERENCE_CONFIGURED_NOT_G3_ACCEPTANCE";
    public const string DpoPolicyDirectInferenceScoreEvidence = "DESIGN_CONVERSION_RUNTIME_DPO_POLICY_DIRECT_INFERENCE_SCORE_APPLIED_BASELINE_ACCEPTANCE";
    public const string DpoPolicyDirectInferenceAcceptanceAuthorityConfiguredEvidence = "DESIGN_CONVERSION_DPO_POLICY_DIRECT_RUNTIME_INFERENCE_CONFIGURED_ACCEPTANCE_AUTHORITY";
    public const string DpoPolicyDirectInferenceAcceptanceAuthorityScoreEvidence = "DESIGN_CONVERSION_RUNTIME_DPO_POLICY_DIRECT_INFERENCE_SCORE_APPLIED_ACCEPTANCE_AUTHORITY";
    public const string DpoPolicyDirectInferenceAcceptanceAuthorityEvidence = "DESIGN_CONVERSION_DIRECT_DPO_POLICY_ACCEPTANCE_AUTHORITY_ACTIVE";
    public const string DpoRuntimeCheckpointAccuracyFromHeldOutEvaluationEvidence = "DESIGN_CONVERSION_DPO_RUNTIME_CHECKPOINT_ACCURACY_FROM_HELDOUT_EVALUATION";
    public const string DpoRuntimeCheckpointAccuracyCurrentCycleHeldOutEvaluationScopeEvidence = "DESIGN_CONVERSION_DPO_RUNTIME_CHECKPOINT_ACCURACY_SCOPE_CURRENT_CYCLE_HELDOUT_EVALUATION";
    public const string DpoPolicyDirectInferenceNoRawPayloadEvidence = "DESIGN_CONVERSION_RUNTIME_DPO_POLICY_DIRECT_INFERENCE_NO_RAW_PAYLOAD";
    public const string DpoPolicyDirectInferenceFailedCode = "DESIGN_CONVERSION_DPO_POLICY_DIRECT_INFERENCE_FAILED";
    public const string DpoPolicyRuntimePairContentUnavailableCode = "DESIGN_CONVERSION_DPO_POLICY_RUNTIME_PAIR_CONTENT_UNAVAILABLE";
    public const string DpoPolicyRuntimePairContentUnavailableEvidence = "DESIGN_CONVERSION_DPO_POLICY_RUNTIME_CONFIGURED_PAIR_CONTENT_UNAVAILABLE_BASELINE_PRESERVED";
    public const string DpoPolicyRuntimePairContentUnavailableWarning = "DESIGN_CONVERSION_RUNTIME_DIRECT_DPO_POLICY_CONFIGURED_PAIR_CONTENT_UNAVAILABLE_BASELINE_PRESERVED";
    public const string DpoPolicyLoraAppliedEvidence = "DESIGN_CONVERSION_44_PRODUCTION_DPO_POLICY_LORA_APPLIED";
    public const string DpoPolicyRuntimeLoraTopologyEvidence = "DESIGN_CONVERSION_RUNTIME_DPO_POLICY_LORA_TOPOLOGY_REBUILT";
    public const string DpoPolicyContinuationRuntimeEncodingAppliedEvidence = "DESIGN_CONVERSION_RUNTIME_DPO_CONTINUATION_RUNTIME_ENCODING_APPLIED";

    private const string CheckpointShapeInvalidCode = "CHECKPOINT_SHAPE_INVALID";
    private const string CheckpointRuntimeUnitIdentityMismatchCode = "CHECKPOINT_RUNTIME_UNIT_IDENTITY_MISMATCH";
    private const string CheckpointNumericValuesInvalidCode = "CHECKPOINT_NUMERIC_VALUES_INVALID";
    private const string ImportCheckpointRefRequiredCode = "IMPORT_CHECKPOINT_REF_REQUIRED";
    private const string ImportCheckpointRefNotAbsoluteCode = "IMPORT_CHECKPOINT_REF_NOT_ABSOLUTE";
    private const string ImportManifestRefRequiredCode = "IMPORT_MANIFEST_REF_REQUIRED";
    private const string ImportAccuracyInvalidCode = "IMPORT_ACCURACY_INVALID";
    private const string VersionUnsupportedCode = "VERSION_UNSUPPORTED";
    private const double HeldOutFeatureFitAcceptanceThreshold = 0.80;
    private const double HeldOutExpectedVisualScoreThreshold = 0.80;
    private const double LocalCheckpointAdjustmentScale = 2.0;
    private const double LocalCheckpointAdjustmentClamp = 0.5;
    private const string GradientBridgeUpdateDerivation = "gradient_bridge_update";
    private const string DpoPolicyContinuationRuntimeEncodingBoundEvidence = "DESIGN_CONVERSION_DPO_CONTINUATION_RUNTIME_ENCODING_BOUND";
    private const string DpoRuntimeEncodingSchemaEvidencePrefix = "dpo_runtime_encoding_schema:";
    private const string DpoRuntimeEncodingSourceDescriptorSchemaEvidencePrefix = "dpo_runtime_encoding_source_descriptor_schema:";
    private const string DpoRuntimeEncodingTokenMappingEvidencePrefix = "dpo_runtime_encoding_token_mapping:";
    private const string DpoRuntimeEncodingTokenIdMinimumEvidencePrefix = "dpo_runtime_encoding_token_id_min:";
    private const string DpoRuntimeEncodingTokenIdMaximumEvidencePrefix = "dpo_runtime_encoding_token_id_max:";
    private const string DpoRuntimeEncodingContextLengthEvidencePrefix = "dpo_runtime_encoding_context_length:";
    private const string DpoRuntimeEncodingTokenizerSha256EvidencePrefix = "dpo_runtime_encoding_tokenizer_sha256:";
    private const string DpoRuntimeEncodingVocabularySha256EvidencePrefix = "dpo_runtime_encoding_vocabulary_sha256:";
    private static readonly Func<string, CancellationToken, Task<byte[]>> DefaultCheckpointByteReader = File.ReadAllBytesAsync;
    private static readonly Func<CancellationToken, Task> DefaultCheckpointStateCapture = _ => Task.CompletedTask;
    private static readonly AsyncLocal<Func<string, CancellationToken, Task<byte[]>>?> CheckpointByteReaderOverride = new();
    private static readonly AsyncLocal<Func<CancellationToken, Task>?> CheckpointStateCaptureOverride = new();
    private static readonly AsyncLocal<Action<string>?> TrainingEntryObserver =
        new();

    private readonly DesignConversionRoutingFlow _flow;
    private readonly DesignConversionMachineEvaluationOptions _evaluationOptions;
    private readonly IDesignConversionMachineGradientBridge? _gradientBridge;
    private readonly object _stateGate = new();
    private readonly List<LearningBatchResult> _trainingHistory = [];
    private readonly Dictionary<string, Dictionary<string, LearnedWeightAccumulator>> _learnedWeightsByUnit = new(StringComparer.Ordinal);

    private string _currentVersion = "0.1.0";
    private double _accuracyAfterLastBatch;
    private bool _hasTrainingScaffoldHistory;
    private BoundCheckpointSnapshot? _boundCheckpointSnapshot;
    private DesignConversionDpoPolicyCheckpointRef? _dpoPolicyCheckpointRef;
    private DpoPolicyRuntimeScorer? _dpoPolicyRuntimeScorer;
    private ImmutableArray<string> _importedTopLevelCheckpointEvidenceRefs = ImmutableArray<string>.Empty;
    private bool _hasImportedTopLevelCheckpointEvidenceRefs;
    private bool _hasEvaluateAsyncHeldOutAccuracyEvidence;
    private long _stateGeneration;

    public DesignConversionMachine() : this(new DesignConversionRoutingFlow()) { }

    public DesignConversionMachine(DesignConversionRoutingFlow flow)
        : this(flow, DesignConversionMachineEvaluationOptions.Default)
    {
    }

    public DesignConversionMachine(
        DesignConversionRoutingFlow flow,
        DesignConversionMachineEvaluationOptions evaluationOptions)
    {
        _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        _evaluationOptions = evaluationOptions ?? throw new ArgumentNullException(nameof(evaluationOptions));
    }

    public DesignConversionMachine(
        DesignConversionRoutingFlow flow,
        IDesignConversionMachineGradientBridge gradientBridge)
        : this(
            flow,
            gradientBridge,
            DesignConversionMachineEvaluationOptions.Default)
    {
    }

    public DesignConversionMachine(
        DesignConversionRoutingFlow flow,
        IDesignConversionMachineGradientBridge gradientBridge,
        DesignConversionMachineEvaluationOptions evaluationOptions)
    {
        _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        _evaluationOptions = evaluationOptions ?? throw new ArgumentNullException(nameof(evaluationOptions));
        _gradientBridge = gradientBridge ?? throw new ArgumentNullException(nameof(gradientBridge));
    }

    public DesignConversionRoutingFlow Flow => _flow;

    internal static Action<string>? TrainingEntryObserverForTesting
    {
        get => TrainingEntryObserver.Value;
        set => TrainingEntryObserver.Value = value;
    }

    public Task<OperationResult<LearningBatchResult>> TrainBatchAsync(
        LearningBatch<DesignConversionDomain> batch,
        CancellationToken ct = default)
        => Task.FromResult(OperationResult<LearningBatchResult>.Fail(
            "Server-owned legal training authority is required for DesignConversion Machine mutation.",
            LegalTrainingTeacherAdmissionAuthority
                .ConfigurationRequiredErrorCode));

    internal async Task<OperationResult<LearningBatchResult>>
        TrainAdmittedBatchAsync(
            DesignConversionMachineTrainingAdmission admission,
            CancellationToken ct = default)
    {
        if (admission is null)
        {
            return OperationResult<LearningBatchResult>.Fail(
                "Exact admitted DesignConversion Machine training content is required.",
                LegalTrainingTeacherAdmissionAuthority
                    .TrainingContentInvalidErrorCode);
        }
        if (ct.IsCancellationRequested)
            return TrainingCancelled();

        var permit = admission.AcquireMutationPermit();
        if (!permit.IsSuccess || permit.Value is null)
            return permit.MapError<LearningBatchResult>();

        using (permit.Value)
        {
            TrainingEntryObserver.Value?.Invoke("admitted");
            return await TrainBatchCoreAsync(admission.Batch, ct);
        }
    }

    private async Task<OperationResult<LearningBatchResult>> TrainBatchCoreAsync(
        LearningBatch<DesignConversionDomain> batch,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<LearningBatchResult>.Fail("Training cancelled.", "CANCELLED");

        if (batch is null)
            return OperationResult<LearningBatchResult>.Fail("Training batch is required.", "BATCH_REQUIRED");

        if (!string.Equals(batch.DomainTag, DomainTag, StringComparison.Ordinal))
            return OperationResult<LearningBatchResult>.Fail(
                $"Training batch domain tag '{batch.DomainTag}' does not match '{DomainTag}'.",
                "DOMAIN_TAG_MISMATCH");

        if (batch.TrainingItems.Count == 0)
            return OperationResult<LearningBatchResult>.Fail(
                "Training batch must contain at least one item.",
                "BATCH_EMPTY");

        PreparedTrainingBatch preparedBatch;
        try
        {
            preparedBatch = PrepareTrainingBatch(batch);
        }
        catch
        {
            return GradientBridgeFailed();
        }

        if (_gradientBridge is null)
        {
            return await CommitLearningBatchAsync(
                preparedBatch.BatchId,
                preparedBatch.RowsTrained,
                preparedBatch.LearnedWeights,
                inputState: null,
                bridgeOwned: false,
                ct);
        }

        var authorizedSignals = BuildGradientBridgeAuthorizedSignals(preparedBatch.LearnedWeights);
        if (authorizedSignals.IsDefaultOrEmpty)
        {
            return OperationResult<LearningBatchResult>.Fail(
                "Gradient bridge requires at least one sanitized training signal.",
                GradientBridgeInputEmptyCode);
        }

        GradientBridgeInputState inputState;
        lock (_stateGate)
        {
            if (ct.IsCancellationRequested)
                return TrainingCancelled();

            if (!TryIncrementPatchVersion(_currentVersion, out _))
            {
                return OperationResult<LearningBatchResult>.Fail(
                    $"Current machine version '{_currentVersion}' cannot be incremented.",
                    VersionUnsupportedCode);
            }

            inputState = new GradientBridgeInputState(_currentVersion, _stateGeneration);
        }

        GradientBridgeAuthorizationSnapshot authorization;
        DesignConversionMachineGradientBridgeRequest request;
        try
        {
            authorization = CreateGradientBridgeAuthorizationSnapshot(
                preparedBatch.BatchId,
                inputState.Version,
                authorizedSignals);
            request = CreateGradientBridgeRequestSnapshot(authorization);
        }
        catch
        {
            return GradientBridgeFailed();
        }

        OperationResult<DesignConversionMachineGradientBridgeResult> bridgeResult;
        try
        {
            bridgeResult = await _gradientBridge.ComputeUpdatesAsync(request, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return TrainingCancelled();
        }
        catch
        {
            return GradientBridgeFailed();
        }

        if (ct.IsCancellationRequested)
            return TrainingCancelled();

        if (bridgeResult is null)
            return GradientBridgeFailed();

        if (!bridgeResult.IsSuccess)
        {
            return string.Equals(bridgeResult.ErrorCode, "CANCELLED", StringComparison.Ordinal) && ct.IsCancellationRequested
                ? TrainingCancelled()
                : GradientBridgeFailed();
        }

        DesignConversionMachineGradientBridgeResult bridgeResultSnapshot;
        try
        {
            bridgeResultSnapshot = CreateGradientBridgeResultSnapshot(bridgeResult.Value);
        }
        catch
        {
            return GradientBridgeFailed();
        }

        var validatedBridgeUpdates = ValidateGradientBridgeResult(bridgeResultSnapshot, authorization);
        if (!validatedBridgeUpdates.IsSuccess)
            return validatedBridgeUpdates.MapError<LearningBatchResult>();

        return await CommitLearningBatchAsync(
            preparedBatch.BatchId,
            preparedBatch.RowsTrained,
            new LearnedWeightBatch(
                validatedBridgeUpdates.Value!,
                Math.Round(bridgeResultSnapshot.AverageRewardMargin, 6)),
            inputState,
            bridgeOwned: true,
            ct);
    }

    private Task<OperationResult<LearningBatchResult>> CommitLearningBatchAsync(
        string batchId,
        int rowsTrained,
        LearnedWeightBatch appliedBatch,
        GradientBridgeInputState? inputState,
        bool bridgeOwned,
        CancellationToken ct)
    {
        lock (_stateGate)
        {
            if (ct.IsCancellationRequested)
                return Task.FromResult(TrainingCancelled());

            if (bridgeOwned &&
                (!string.Equals(_currentVersion, inputState?.Version, StringComparison.Ordinal) ||
                 _stateGeneration != inputState?.Generation))
                return Task.FromResult(GradientBridgeVersionConflict());

            if (!TryIncrementPatchVersion(_currentVersion, out var nextVersion))
            {
                return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                    $"Current machine version '{_currentVersion}' cannot be incremented.",
                    VersionUnsupportedCode));
            }

            var accuracyBefore = _accuracyAfterLastBatch;
            var preflight = PreflightLearningDeltas(appliedBatch.Deltas, bridgeOwned);
            if (!preflight.IsSuccess)
                return Task.FromResult(preflight.MapError<LearningBatchResult>());

            if (ct.IsCancellationRequested)
                return Task.FromResult(TrainingCancelled());

            var startMs = Environment.TickCount64;
            foreach (var delta in appliedBatch.Deltas)
                ApplyLearnedWeightDelta(delta);

            var accuracyAfter = CarryForwardAccuracyWithoutHeldOutMeasurement(accuracyBefore, appliedBatch);
            var elapsedMs = Math.Max(0, Environment.TickCount64 - startMs);
            var result = new LearningBatchResult(
                BatchId: batchId,
                RowsTrained: rowsTrained,
                AccuracyBefore: accuracyBefore,
                AccuracyAfter: accuracyAfter,
                RewardMargin: appliedBatch.AverageRewardMargin,
                CpuMs: elapsedMs,
                PeakRamMb: GC.GetTotalMemory(forceFullCollection: false) / (1024 * 1024),
                ElapsedMs: elapsedMs,
                PromotedUnitIds: []);

            _hasTrainingScaffoldHistory = true;
            _accuracyAfterLastBatch = accuracyAfter;
            _hasEvaluateAsyncHeldOutAccuracyEvidence = false;
            _trainingHistory.Add(result);
            _currentVersion = nextVersion;
            ClearImportedCheckpointAcceptanceAuthorityUnsafe();
            _stateGeneration++;
            return Task.FromResult(OperationResult<LearningBatchResult>.Ok(result));
        }
    }

    public Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(
        LearningEvaluationSet<DesignConversionDomain> evalSet,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<LearningEvaluationResult>.Fail("Evaluation cancelled.", "CANCELLED"));

        if (evalSet is null)
            return Task.FromResult(OperationResult<LearningEvaluationResult>.Fail("Evaluation set is required.", "EVAL_SET_REQUIRED"));

        if (!string.Equals(evalSet.DomainTag, DomainTag, StringComparison.Ordinal))
            return Task.FromResult(OperationResult<LearningEvaluationResult>.Fail(
                $"Evaluation set domain tag '{evalSet.DomainTag}' does not match '{DomainTag}'.",
                "DOMAIN_TAG_MISMATCH"));

        if (evalSet.EvaluationItems.Count == 0)
            return Task.FromResult(OperationResult<LearningEvaluationResult>.Fail(
                "Evaluation set must contain at least one item.",
                "EVAL_SET_EMPTY"));

        EvaluationStateSnapshot evaluationState;
        lock (_stateGate)
            evaluationState = CaptureEvaluationStateUnsafe();

        var rowScores = evalSet.EvaluationItems
            .Select(item => EvaluateHeldOutItem(item, evaluationState.WeightsByUnit))
            .ToArray();
        var correctRows = rowScores.Count(score => score.MatchesExpectedResponse);
        var accuracy = Math.Round(correctRows / (double)evalSet.EvaluationItems.Count, 6);
        var heldOutEvaluationAccuracyGateThreshold =
            _evaluationOptions.HeldOutEvaluationAccuracyGateThreshold;
        var passesGate = accuracy >= heldOutEvaluationAccuracyGateThreshold;
        var learnedWeightsPresent = HasLearnedWeights(evaluationState.WeightsByUnit);
        var gateNote = string.Join(
            " ",
            passesGate
                ? "DesignConversion held-out lifecycle gate passed:"
                : "DesignConversion held-out lifecycle gate failed:",
            $"accuracy={accuracy.ToString("0.0000", CultureInfo.InvariantCulture)}",
            passesGate ? ">=" : "<",
            $"threshold={heldOutEvaluationAccuracyGateThreshold.ToString("0.0000", CultureInfo.InvariantCulture)};",
            $"rows={evalSet.EvaluationItems.Count.ToString(CultureInfo.InvariantCulture)};",
            $"matched={correctRows.ToString(CultureInfo.InvariantCulture)};",
            $"row_fit_threshold={HeldOutFeatureFitAcceptanceThreshold.ToString("0.0000", CultureInfo.InvariantCulture)};",
            $"training_history_present={evaluationState.HasTrainingScaffoldHistory};",
            $"learned_weights_present={learnedWeightsPresent};",
            "scope=sanitized local checkpoint feature-fit held-out evaluation; not trained visual QA authority.");

        lock (_stateGate)
        {
            if (_stateGeneration == evaluationState.Generation)
            {
                _accuracyAfterLastBatch = accuracy;
                ClearImportedCheckpointAcceptanceAuthorityUnsafe();
                _hasEvaluateAsyncHeldOutAccuracyEvidence = true;
                _stateGeneration++;
            }
        }

        return Task.FromResult(OperationResult<LearningEvaluationResult>.Ok(new LearningEvaluationResult(
            EvaluationId: evalSet.EvaluationId,
            RowsEvaluated: evalSet.EvaluationItems.Count,
            Accuracy: accuracy,
            RewardMargin: 0.0,
            PassesGate: passesGate,
            GateNote: gateNote)));
    }

    public async Task<OperationResult<LearningMachineExport>> ExportAsync(CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<LearningMachineExport>.Fail("Export cancelled.", "CANCELLED");

        BoundCheckpointExportState capturedState;
        lock (_stateGate)
        {
            if (_boundCheckpointSnapshot is null)
            {
                return OperationResult<LearningMachineExport>.Fail(
                    "DesignConversion machine has no persisted checkpoint bound to its current local state.",
                    ExportCheckpointNotBoundCode);
            }

            if (!string.Equals(_boundCheckpointSnapshot.Version, _currentVersion, StringComparison.Ordinal))
            {
                return OperationResult<LearningMachineExport>.Fail(
                    $"Bound checkpoint version '{_boundCheckpointSnapshot.Version}' is stale for current machine version '{_currentVersion}'.",
                    ExportCheckpointVersionStaleCode);
            }

            capturedState = new BoundCheckpointExportState(
                _boundCheckpointSnapshot,
                _currentVersion,
                _accuracyAfterLastBatch,
                _stateGeneration);
        }

        var verification = await VerifyBoundCheckpointSnapshotAsync(capturedState.Snapshot, ct);
        if (!verification.IsSuccess)
            return verification.MapError<LearningMachineExport>();

        lock (_stateGate)
        {
            if (_stateGeneration != capturedState.Generation ||
                !string.Equals(_currentVersion, capturedState.Version, StringComparison.Ordinal) ||
                !Equals(_boundCheckpointSnapshot, capturedState.Snapshot))
            {
                return OperationResult<LearningMachineExport>.Fail(
                    "Machine state changed while its bound checkpoint was being verified.",
                    ExportCheckpointVersionStaleCode);
            }

            return OperationResult<LearningMachineExport>.Ok(CreateBoundExport(
                capturedState.Snapshot,
                capturedState.Accuracy,
                DateTimeOffset.UtcNow));
        }
    }

    public async Task<OperationResult<bool>> ImportAsync(
        LearningMachineExport export,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<bool>.Fail("Import cancelled.", "CANCELLED");

        if (export is null)
            return OperationResult<bool>.Fail("Export record is required.", "EXPORT_REQUIRED");

        if (!string.Equals(export.MachineId, MachineId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail(
                $"Export machine id '{export.MachineId}' does not match '{MachineId}'.",
                "MACHINE_ID_MISMATCH");

        if (!string.Equals(export.DomainTag, DomainTag, StringComparison.Ordinal))
            return OperationResult<bool>.Fail(
                $"Export domain tag '{export.DomainTag}' does not match '{DomainTag}'.",
                "DOMAIN_TAG_MISMATCH");

        if (!IsSupportedVersion(export.Version))
            return OperationResult<bool>.Fail(
                $"Export version '{export.Version}' is not supported.",
                VersionUnsupportedCode);

        if (!IsValidCheckpointAccuracy(export.AccuracyAtExport))
            return OperationResult<bool>.Fail(
                "Export accuracy is invalid.",
                ImportAccuracyInvalidCode);

        if (string.IsNullOrWhiteSpace(export.ManifestRef))
            return OperationResult<bool>.Fail(
                "Export manifest reference is required.",
                ImportManifestRefRequiredCode);

        var checkpointPath = ValidateImportCheckpointRef(export.CheckpointRef);
        if (!checkpointPath.IsSuccess)
            return checkpointPath.MapError<bool>();

        var load = await LoadCheckpointAsync(checkpointPath.Value!, ct);
        if (!load.IsSuccess)
            return OperationResult<bool>.Fail(load.ErrorMessage, load.ErrorCode);

        var loadedExport = await load.Value!.Machine.ExportAsync(ct);
        if (!loadedExport.IsSuccess)
            return OperationResult<bool>.Fail(loadedExport.ErrorMessage, loadedExport.ErrorCode);

        var metadataValidation = ValidateExportMatchesLoadedCheckpoint(
            export,
            load.Value.Checkpoint,
            load.Value.CheckpointSha256,
            loadedExport.Value!);
        if (!metadataValidation.IsSuccess)
            return metadataValidation;

        var importResult = ImportCheckpointCore(
            load.Value.Checkpoint,
            new BoundCheckpointSnapshot(
                checkpointPath.Value!,
                load.Value.Checkpoint.Version,
                load.Value.CheckpointSha256));
        if (!importResult.IsSuccess)
            return importResult;

        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<DesignConversionMachineCheckpoint> ExportCheckpoint()
        => OperationResult<DesignConversionMachineCheckpoint>.Ok(CaptureCheckpointState().Checkpoint);

    private CheckpointStateSnapshot CaptureCheckpointState()
    {
        lock (_stateGate)
            return new CheckpointStateSnapshot(BuildCheckpointUnsafe(), _stateGeneration);
    }

    private DesignConversionMachineCheckpoint BuildCheckpointUnsafe()
    {
        return new DesignConversionMachineCheckpoint(
            SchemaVersion: CheckpointSchemaVersion,
            MachineId: MachineId,
            DomainTag: DomainTag,
            Version: _currentVersion,
            AccuracyAfterLastBatch: _accuracyAfterLastBatch,
            HasTrainingScaffoldHistory: _hasTrainingScaffoldHistory,
            ExportedAtUtc: DateTimeOffset.UtcNow,
            CriticUnit: BuildRuntimeUnitCheckpoint(
                DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                DesignConversionRuntimeScaffoldCriticUnit.Role,
                DesignConversionRuntimeScaffoldCriticUnit.DomainTag,
                "runtime_scaffold_input_evidence_only",
                [
                    RuntimeScaffoldEvidence,
                    DesignConversionRuntimeScaffoldCriticUnit.InputEvidenceOnlyWarning
                ]),
            ImproverUnit: BuildRuntimeUnitCheckpoint(
                DesignConversionRuntimeScaffoldImproverUnit.UnitId,
                DesignConversionRuntimeScaffoldImproverUnit.Role,
                DesignConversionRuntimeScaffoldImproverUnit.DomainTag,
                "runtime_scaffold_candidate_shell",
                [
                    RuntimeScaffoldEvidence,
                    "DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE"
                ]),
            TrainingHistory: _trainingHistory
                .Select(history => new DesignConversionTrainingHistoryEntryCheckpoint(
                    BatchId: history.BatchId,
                    RowsTrained: history.RowsTrained,
                    AccuracyBefore: history.AccuracyBefore,
                    AccuracyAfter: history.AccuracyAfter,
                    RewardMargin: history.RewardMargin,
                    CpuMs: history.CpuMs,
                    PeakRamMb: history.PeakRamMb,
                    ElapsedMs: history.ElapsedMs,
                    PromotedUnitIds: history.PromotedUnitIds.ToArray()))
                .ToArray(),
            EvidenceRefs: BuildCheckpointEvidenceRefsUnsafe(),
            DeterministicLogicClaimedAsLearning: false,
            DpoPolicyCheckpointRef: SnapshotDpoPolicyCheckpointRef(_dpoPolicyCheckpointRef));

    }

    public OperationResult<bool> ImportCheckpoint(DesignConversionMachineCheckpoint checkpoint)
        => ImportCheckpointCore(checkpoint, boundCheckpointSnapshot: null);

    private OperationResult<bool> ImportCheckpointCore(
        DesignConversionMachineCheckpoint checkpoint,
        BoundCheckpointSnapshot? boundCheckpointSnapshot,
        DpoPolicyRuntimeScorer? dpoPolicyRuntimeScorer = null)
    {
        if (checkpoint is null)
            return OperationResult<bool>.Fail("Checkpoint is required.", "CHECKPOINT_REQUIRED");

        if (checkpoint.SchemaVersion != CheckpointSchemaVersion)
            return OperationResult<bool>.Fail(
                $"Checkpoint schema version {checkpoint.SchemaVersion} is not supported.",
                "CHECKPOINT_SCHEMA_UNSUPPORTED");

        if (!string.Equals(checkpoint.MachineId, MachineId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail(
                $"Checkpoint machine id '{checkpoint.MachineId}' does not match '{MachineId}'.",
                "MACHINE_ID_MISMATCH");

        if (!string.Equals(checkpoint.DomainTag, DomainTag, StringComparison.Ordinal))
            return OperationResult<bool>.Fail(
                $"Checkpoint domain tag '{checkpoint.DomainTag}' does not match '{DomainTag}'.",
                "DOMAIN_TAG_MISMATCH");

        if (!IsSupportedVersion(checkpoint.Version))
            return OperationResult<bool>.Fail(
                $"Checkpoint version '{checkpoint.Version}' is not supported.",
                VersionUnsupportedCode);

        var shapeValidation = ValidateCheckpointShape(checkpoint);
        if (!shapeValidation.IsSuccess)
            return shapeValidation.MapError<bool>();

        var directPolicyCheckpointRef = checkpoint.DpoPolicyCheckpointRef is { } suppliedPolicyRef &&
            IsDpoPolicyDirectInferenceRuntimeBridgeKind(
                suppliedPolicyRef.RuntimeBridgeKind)
            ? suppliedPolicyRef
            : null;
        if (directPolicyCheckpointRef?.TrainingContext is { } directTrainingContext &&
            directTrainingContext.SchemaVersion != 4)
        {
            return OperationResult<bool>.Fail(
                DpoPolicyRuntimeSchemaUnsupportedMessage,
                DpoPolicyRuntimeSchemaUnsupportedCode);
        }

        var dpoPolicyCheckpointRefValidation = ValidateDpoPolicyCheckpointRef(checkpoint.DpoPolicyCheckpointRef);
        if (!dpoPolicyCheckpointRefValidation.IsSuccess)
            return dpoPolicyCheckpointRefValidation;

        if (directPolicyCheckpointRef is not null)
        {
            if (dpoPolicyRuntimeScorer is null)
            {
                return OperationResult<bool>.Fail(
                    "Direct DPO policy inference checkpoint requires a loaded policy scorer snapshot.",
                    DpoPolicyCheckpointPayloadModelLoadFailedCode);
            }
        }

        var runtimeUnitIdentityValidation = ValidateRuntimeUnitIdentities(checkpoint);
        if (!runtimeUnitIdentityValidation.IsSuccess)
            return runtimeUnitIdentityValidation.MapError<bool>();

        var numericValidation = ValidateCheckpointNumericValues(checkpoint);
        if (!numericValidation.IsSuccess)
            return numericValidation.MapError<bool>();

        var importedTrainingHistory = checkpoint.TrainingHistory.Select(history => new LearningBatchResult(
            BatchId: history.BatchId,
            RowsTrained: history.RowsTrained,
            AccuracyBefore: history.AccuracyBefore,
            AccuracyAfter: history.AccuracyAfter,
            RewardMargin: history.RewardMargin,
            CpuMs: history.CpuMs,
            PeakRamMb: history.PeakRamMb,
            ElapsedMs: history.ElapsedMs,
            PromotedUnitIds: history.PromotedUnitIds.ToArray())).ToArray();
        var learnedWeightsImport = BuildLearnedWeightImport(checkpoint);
        if (!learnedWeightsImport.IsSuccess)
            return learnedWeightsImport.MapError<bool>();
        var dpoPolicyCheckpointRef = SnapshotDpoPolicyCheckpointRef(checkpoint.DpoPolicyCheckpointRef);
        var topLevelEvidenceRefs = SnapshotTopLevelCheckpointEvidenceRefs(checkpoint.EvidenceRefs);
        lock (_stateGate)
        {
            _currentVersion = checkpoint.Version;
            _accuracyAfterLastBatch = checkpoint.AccuracyAfterLastBatch;
            _hasTrainingScaffoldHistory = checkpoint.HasTrainingScaffoldHistory;
            _trainingHistory.Clear();
            _trainingHistory.AddRange(importedTrainingHistory);
            _learnedWeightsByUnit.Clear();
            foreach (var pair in learnedWeightsImport.Value!)
                _learnedWeightsByUnit[pair.Key] = pair.Value;

            _boundCheckpointSnapshot = boundCheckpointSnapshot;
            _dpoPolicyCheckpointRef = dpoPolicyCheckpointRef;
            _dpoPolicyRuntimeScorer = dpoPolicyRuntimeScorer;
            _importedTopLevelCheckpointEvidenceRefs = topLevelEvidenceRefs;
            _hasImportedTopLevelCheckpointEvidenceRefs = true;
            _hasEvaluateAsyncHeldOutAccuracyEvidence = false;
            _stateGeneration++;
        }

        return OperationResult<bool>.Ok(true);
    }

    private string[] BuildCheckpointEvidenceRefsUnsafe()
    {
        if (_hasImportedTopLevelCheckpointEvidenceRefs)
            return _importedTopLevelCheckpointEvidenceRefs.ToArray();

        var accuracyEvidenceRefs = _hasEvaluateAsyncHeldOutAccuracyEvidence
            ? new[]
            {
                EvaluateAsyncHeldOutAccuracyMeasuredEvidence,
                EvaluateAsyncSanitizedLocalCheckpointFeatureFitEvidence,
                EvaluateAsyncNotTrainedVisualQaAuthorityEvidence
            }
            : new[]
            {
                HeldOutAccuracyNotMeasuredEvidence,
                LocalPreferenceFitSignalOnlyEvidence
            };

        return new[]
            {
                RuntimeScaffoldEvidence,
                CheckpointLifecycleEvidence
            }
            .Concat(accuracyEvidenceRefs)
            .Concat(new[]
            {
                "NO_DPO_GENERATOR_OR_MODEL_PROMOTION_IN_SLICE_3",
                "DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE"
            })
            .Concat(_dpoPolicyRuntimeScorer is null
                ? Array.Empty<string>()
                : new[]
                {
                    DpoPolicyDirectInferenceConfiguredEvidence,
                    DpoPolicyDirectInferenceNoRawPayloadEvidence
                })
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private void ClearImportedTopLevelCheckpointEvidenceRefsUnsafe()
    {
        _importedTopLevelCheckpointEvidenceRefs = ImmutableArray<string>.Empty;
        _hasImportedTopLevelCheckpointEvidenceRefs = false;
    }

    private void ClearImportedCheckpointAcceptanceAuthorityUnsafe()
    {
        ClearImportedTopLevelCheckpointEvidenceRefsUnsafe();
        if (_dpoPolicyCheckpointRef?.EvaluatedHeldOutAcceptanceAuthority is not null)
        {
            _dpoPolicyCheckpointRef = _dpoPolicyCheckpointRef with
            {
                EvaluatedHeldOutAcceptanceAuthority = null
            };
        }
    }

    private static ImmutableArray<string> SnapshotTopLevelCheckpointEvidenceRefs(
        IReadOnlyList<string>? evidenceRefs)
        => evidenceRefs is null
            ? ImmutableArray<string>.Empty
            : evidenceRefs.ToImmutableArray();

    private static DesignConversionDpoPolicyCheckpointRef? SnapshotDpoPolicyCheckpointRef(
        DesignConversionDpoPolicyCheckpointRef? checkpointRef)
        => checkpointRef is null
            ? null
            : new DesignConversionDpoPolicyCheckpointRef(
                ArtifactKind: checkpointRef.ArtifactKind,
                PolicyCheckpointFileName: checkpointRef.PolicyCheckpointFileName,
                Sha256: checkpointRef.Sha256,
                SourceSystem: checkpointRef.SourceSystem,
                SourceMechanism: checkpointRef.SourceMechanism,
                RuntimeBridgeKind: checkpointRef.RuntimeBridgeKind,
                FreshLoadVerified: checkpointRef.FreshLoadVerified,
                ConvertedToRuntimeWeights: checkpointRef.ConvertedToRuntimeWeights,
                SourceRecordCount: checkpointRef.SourceRecordCount,
                BridgePreferenceCount: checkpointRef.BridgePreferenceCount,
                DpoStepCount: checkpointRef.DpoStepCount,
                TrainingContext: SnapshotDpoPolicyCheckpointTrainingContext(checkpointRef.TrainingContext),
                EvidenceRefs: checkpointRef.EvidenceRefs.ToArray(),
                PolicyCheckpointByteLength: checkpointRef.PolicyCheckpointByteLength)
            {
                EvaluatedHeldOutAcceptanceAuthority =
                    SnapshotDpoHeldOutAcceptanceAuthority(checkpointRef.EvaluatedHeldOutAcceptanceAuthority)
            };

    private static DesignConversionDpoPolicyCheckpointTrainingContext SnapshotDpoPolicyCheckpointTrainingContext(
        DesignConversionDpoPolicyCheckpointTrainingContext context)
        => context with
        {
            LegalTeacherAdmissionCommitment =
                DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Snapshot(
                    context.LegalTeacherAdmissionCommitment),
            RandomContext = context.RandomContext is null
                ? null
                : context.RandomContext with { },
            Lora = context.Lora is null
                ? null
                : context.Lora with
                {
                    TargetModules = context.Lora.TargetModules?.ToArray() ?? []
                }
        };

    private static DesignConversionDpoHeldOutEvaluation? SnapshotDpoHeldOutAcceptanceAuthority(
        DesignConversionDpoHeldOutEvaluation? evaluation)
        => evaluation is null
            ? null
            : evaluation with
            {
                G3Metrics = evaluation.G3Metrics is null
                    ? null
                    : evaluation.G3Metrics with { },
                CycleAccuracyLineage = evaluation.CycleAccuracyLineage is null
                    ? null
                    : evaluation.CycleAccuracyLineage with { }
            };

    private static OperationResult<bool> ValidateDpoPolicyCheckpointRef(
        DesignConversionDpoPolicyCheckpointRef? checkpointRef)
    {
        // A null reference is the intentional backward-compatible legacy shape.
        if (checkpointRef is null)
            return OperationResult<bool>.Ok(true);

        if (checkpointRef.TrainingContext is null ||
            checkpointRef.EvidenceRefs is null)
        {
            return CreateDpoPolicyCheckpointRefInvalid();
        }

        var usesSiblingPayload = IsDpoPolicySiblingPayloadRuntimeBridgeKind(checkpointRef.RuntimeBridgeKind);
        var usesIdentityMetadata = IsDpoPolicyIdentityMetadataRuntimeBridgeKind(checkpointRef.RuntimeBridgeKind);
        if (!string.Equals(
                checkpointRef.ArtifactKind,
                DesignConversionDpoTrainerRouteBGate.PolicyCheckpointArtifactKind,
                StringComparison.Ordinal) ||
            !string.Equals(
                checkpointRef.SourceSystem,
                "11.DPO:DpoTrainer",
                StringComparison.Ordinal) ||
            !string.Equals(
                checkpointRef.SourceMechanism,
                "11.DPO:DpoTrainer.StepVector:fresh_loaded",
                StringComparison.Ordinal) ||
            (!usesSiblingPayload && !usesIdentityMetadata))
        {
            return CreateDpoPolicyCheckpointRefInvalid();
        }

        if (!checkpointRef.FreshLoadVerified ||
            !checkpointRef.ConvertedToRuntimeWeights ||
            checkpointRef.SourceRecordCount <= 0 ||
            checkpointRef.BridgePreferenceCount <= 0 ||
            checkpointRef.BridgePreferenceCount > checkpointRef.SourceRecordCount ||
            checkpointRef.DpoStepCount <= 0 ||
            checkpointRef.DpoStepCount > 999_999)
        {
            return CreateDpoPolicyCheckpointRefInvalid();
        }

        var fileName = checkpointRef.PolicyCheckpointFileName;
        var expectedFileName = $"dpo_policy_step_{checkpointRef.DpoStepCount:D6}.bin";
        if (string.IsNullOrWhiteSpace(fileName) ||
            Path.IsPathFullyQualified(fileName) ||
            Path.IsPathRooted(fileName) ||
            !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal) ||
            !string.Equals(fileName, expectedFileName, StringComparison.Ordinal) ||
            fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            return CreateDpoPolicyCheckpointRefInvalid();
        }

        if (!IsCanonicalLowercaseSha256Hex(checkpointRef.Sha256))
            return CreateDpoPolicyCheckpointRefInvalid();

        var context = checkpointRef.TrainingContext;
        if (context.SchemaVersion is 0 or 1)
        {
            if (!usesIdentityMetadata ||
                context.LegalTeacherAdmissionCommitment is not null ||
                context.RandomContext is not null ||
                !ValidateLegacyDpoPolicyCheckpointTrainingContext(context) ||
                checkpointRef.PolicyCheckpointByteLength != 0)
            {
                return CreateDpoPolicyCheckpointRefInvalid();
            }
        }
        else if (context.SchemaVersion == 2)
        {
            if (context.LegalTeacherAdmissionCommitment is not null ||
                context.RandomContext is not null ||
                !ValidateInjectedDpoPolicyCheckpointTrainingContext(context, rawPayloadPersisted: usesSiblingPayload) ||
                usesSiblingPayload != context.RawPayloadPersisted ||
                (usesSiblingPayload
                    ? checkpointRef.PolicyCheckpointByteLength <= 0
                    : checkpointRef.PolicyCheckpointByteLength != 0))
            {
                return CreateDpoPolicyCheckpointRefInvalid();
            }
        }
        else if (context.SchemaVersion == 3)
        {
            if (!IsDpoPolicyDirectInferenceRuntimeBridgeKind(
                    checkpointRef.RuntimeBridgeKind) ||
                context.RandomContext is not null ||
                !usesSiblingPayload ||
                !context.RawPayloadPersisted ||
                context.AbsolutePathPersisted ||
                checkpointRef.PolicyCheckpointByteLength <= 0 ||
                !ValidateInjectedDpoPolicyCheckpointTrainingContext(
                    context,
                    rawPayloadPersisted: true) ||
                !DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Validate(
                    context.LegalTeacherAdmissionCommitment) ||
                !DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.MatchesCheckpointEvidence(
                    context.LegalTeacherAdmissionCommitment,
                    checkpointRef.EvidenceRefs))
            {
                return CreateDpoPolicyCheckpointRefInvalid();
            }
        }
        else if (context.SchemaVersion == 4)
        {
            if (!IsDpoPolicyDirectInferenceRuntimeBridgeKind(
                    checkpointRef.RuntimeBridgeKind) ||
                !usesSiblingPayload ||
                !context.RawPayloadPersisted ||
                context.AbsolutePathPersisted ||
                checkpointRef.PolicyCheckpointByteLength <= 0 ||
                !DesignConversionDpoPolicyRandomContextContract.IsValidCurrent(
                    context.RandomContext) ||
                !DesignConversionDpoPolicyRandomContextContract.HasRequiredEvidenceRefs(
                    context.RandomContext,
                    checkpointRef.EvidenceRefs) ||
                !ValidateInjectedDpoPolicyCheckpointTrainingContext(
                    context,
                    rawPayloadPersisted: true) ||
                !DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Validate(
                    context.LegalTeacherAdmissionCommitment) ||
                !DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.MatchesCheckpointEvidence(
                    context.LegalTeacherAdmissionCommitment,
                    checkpointRef.EvidenceRefs))
            {
                return CreateDpoPolicyCheckpointRefInvalid();
            }
        }
        else
        {
            return CreateDpoPolicyCheckpointRefInvalid();
        }

        if (checkpointRef.EvidenceRefs.Any(string.IsNullOrWhiteSpace))
            return CreateDpoPolicyCheckpointRefInvalid();

        if (RequiresEvaluatedPairAwareRuntimeEncoding(checkpointRef) &&
            !IsDpoPolicyDirectInferenceRuntimeBridgeKind(
                checkpointRef.RuntimeBridgeKind))
        {
            return CreateDpoPolicyCheckpointRefInvalid();
        }

        var requiredEvidenceRefs = BuildRequiredDpoPolicyEvidenceRefs(checkpointRef, context, usesSiblingPayload);
        if (requiredEvidenceRefs.Any(required =>
                !checkpointRef.EvidenceRefs.Contains(required, StringComparer.Ordinal)))
        {
            return CreateDpoPolicyCheckpointRefInvalid();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool IsDpoPolicySiblingPayloadRuntimeBridgeKind(string? runtimeBridgeKind)
        => IsDpoPolicyDirectInferenceRuntimeBridgeKind(runtimeBridgeKind) ||
           string.Equals(
               runtimeBridgeKind,
               DpoPolicyLegacySiblingPayloadRuntimeBridgeKind,
               StringComparison.Ordinal);

    private static bool IsDpoPolicyDirectInferenceRuntimeBridgeKind(string? runtimeBridgeKind)
        => string.Equals(
            runtimeBridgeKind,
            DpoPolicyDirectInferenceRuntimeBridgeKind,
            StringComparison.Ordinal);

    private static bool IsDpoPolicyIdentityMetadataRuntimeBridgeKind(string? runtimeBridgeKind)
        => string.Equals(
            runtimeBridgeKind,
            DpoPolicyIdentityMetadataRuntimeBridgeKind,
            StringComparison.Ordinal);

    private static string[] BuildRequiredDpoPolicyEvidenceRefs(
        DesignConversionDpoPolicyCheckpointRef checkpointRef,
        DesignConversionDpoPolicyCheckpointTrainingContext context,
        bool usesSiblingPayload)
    {
        var requiredEvidenceRefs = new List<string>
        {
            DpoTrainerPolicyCheckpointFreshLoadedEvidence,
            DpoTrainerPolicyCheckpointConvertedEvidence,
            "DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_SOURCE_11_DPO",
            "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PRODUCT_READY_FALSE",
            "11.DPO:DpoTrainer",
            "11.DPO:DpoTrainer.StepVector",
            $"dpo_policy_checkpoint_sha256:{checkpointRef.Sha256}",
            $"dpo_policy_checkpoint_file:{checkpointRef.PolicyCheckpointFileName}",
            $"dpo_policy_bridge_source_record_count:{checkpointRef.SourceRecordCount.ToString(CultureInfo.InvariantCulture)}",
            $"dpo_policy_bridge_preference_count:{checkpointRef.BridgePreferenceCount.ToString(CultureInfo.InvariantCulture)}",
            $"dpo_policy_step_count:{checkpointRef.DpoStepCount.ToString(CultureInfo.InvariantCulture)}",
            "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_TYPED",
            "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_NO_ABSOLUTE_PATH"
        };

        if (usesSiblingPayload)
        {
            requiredEvidenceRefs.Add("DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PAYLOAD_FILE_PERSISTED");
            requiredEvidenceRefs.Add("DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_PAYLOAD_FILE_SIBLING");
            if (IsDpoPolicyDirectInferenceRuntimeBridgeKind(checkpointRef.RuntimeBridgeKind))
                requiredEvidenceRefs.Add(DpoPolicyDirectInferenceConfiguredEvidence);
        }
        else
        {
            requiredEvidenceRefs.Add("DESIGN_CONVERSION_DPO_POLICY_BRIDGE_NO_RAW_PAYLOAD");
            requiredEvidenceRefs.Add("DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_NO_RAW_PAYLOAD");
        }

        if (context.SchemaVersion is 2 or 3 or 4)
        {
            requiredEvidenceRefs.AddRange(
            [
                "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_INJECTED",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_VALIDATED",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_MODELS_DISTINCT",
                "DESIGN_CONVERSION_DPO_POLICY_INITIAL_POLICY_REFERENCE_BITWISE_EQUAL",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_AND_INITIAL_WEIGHT_HASHES_DISTINCT",
                $"dpo_policy_base_provider_kind:{context.ProviderKind}",
                $"dpo_policy_base_configuration_kind:{context.ConfigKind}",
                $"dpo_policy_base_model_id:{context.BaseModelId}",
                $"dpo_policy_base_revision:{context.BaseRevision}",
                $"dpo_policy_base_checkpoint_sha256:{context.BaseCheckpointSha256}",
                $"dpo_policy_config_sha256:{context.ConfigSha256}",
                $"dpo_policy_initial_weight_sha256:{context.InitialWeightSha256}"
            ]);

            if (context.Lora is not null)
            {
                requiredEvidenceRefs.Add(DpoPolicyLoraAppliedEvidence);
                requiredEvidenceRefs.Add("10.LoRA:LoraApplicator");
                requiredEvidenceRefs.Add($"dpo_policy_lora_rank:{context.Lora.Rank.ToString(CultureInfo.InvariantCulture)}");
                requiredEvidenceRefs.Add($"dpo_policy_lora_alpha:{context.Lora.Alpha.ToString("0.######", CultureInfo.InvariantCulture)}");
                requiredEvidenceRefs.Add($"dpo_policy_lora_dropout:{context.Lora.Dropout.ToString("0.######", CultureInfo.InvariantCulture)}");
                requiredEvidenceRefs.Add($"dpo_policy_lora_target_modules:{string.Join(",", context.Lora.TargetModules)}");
                requiredEvidenceRefs.Add($"dpo_policy_lora_adapter_count:{context.Lora.AdapterCount.ToString(CultureInfo.InvariantCulture)}");
            }
        }

        if (context.SchemaVersion == 4)
        {
            requiredEvidenceRefs.AddRange(
                DesignConversionDpoPolicyRandomContextContract.BuildRequiredEvidenceRefs(
                    context.RandomContext!));
        }

        return requiredEvidenceRefs.ToArray();
    }

    private static bool ValidateLegacyDpoPolicyCheckpointTrainingContext(
        DesignConversionDpoPolicyCheckpointTrainingContext context)
    {
        var exactConfigKind =
            DesignConversionDpoPolicyBaseMetadata.IsLegacyConfigurationKind(context.ConfigKind);

        return exactConfigKind &&
               context.VocabSize > 0 &&
               context.ContextLength > 0 &&
               context.HiddenDim > 0 &&
               context.NumLayers > 0 &&
               context.NumHeads > 0 &&
               context.NumKvHeads > 0 &&
               context.FfnHiddenDim >= context.HiddenDim &&
               context.HiddenDim % context.NumHeads == 0 &&
               context.NumHeads % context.NumKvHeads == 0 &&
               !context.RawPayloadPersisted &&
               !context.AbsolutePathPersisted &&
               BitConverter.SingleToInt32Bits(context.RopeTheta) == 0 &&
               BitConverter.SingleToInt32Bits(context.RmsNormEps) == 0 &&
               context.ProviderKind is null &&
               context.BaseModelId is null &&
               context.BaseRevision is null &&
               context.BaseCheckpointSha256 is null &&
               context.ConfigSha256 is null &&
               context.InitialWeightSha256 is null &&
               context.Lora is null &&
               context.RandomContext is null &&
               context.LegalTeacherAdmissionCommitment is null;
    }

    private static bool ValidateInjectedDpoPolicyCheckpointTrainingContext(
        DesignConversionDpoPolicyCheckpointTrainingContext context,
        bool rawPayloadPersisted)
    {
        if (!DesignConversionDpoPolicyBaseMetadata.IsLogicalIdentifier(
                context.ConfigKind,
                160,
                allowSlash: false) ||
            DesignConversionDpoPolicyBaseMetadata.IsLegacyConfigurationKind(context.ConfigKind) ||
            context.VocabSize < 256 ||
            context.ContextLength < 2 ||
            context.HiddenDim <= 0 ||
            context.NumLayers <= 0 ||
            context.NumHeads <= 0 ||
            context.NumKvHeads <= 0 ||
            context.FfnHiddenDim <= 0 ||
            context.HiddenDim % context.NumHeads != 0 ||
            context.NumHeads % context.NumKvHeads != 0 ||
            !float.IsFinite(context.RopeTheta) ||
            context.RopeTheta <= 0 ||
            !float.IsFinite(context.RmsNormEps) ||
            context.RmsNormEps <= 0 ||
            context.RawPayloadPersisted != rawPayloadPersisted ||
            context.AbsolutePathPersisted ||
            !DesignConversionDpoPolicyBaseMetadata.IsCanonicalSha256(context.ConfigSha256) ||
            !DesignConversionDpoPolicyBaseMetadata.IsCanonicalSha256(context.InitialWeightSha256) ||
            !ValidateDpoPolicyLoraContext(context.Lora, context.NumLayers))
        {
            return false;
        }

        var identity = new DesignConversionDpoPolicyBaseIdentity(
            context.ProviderKind ?? string.Empty,
            context.ConfigKind,
            context.BaseModelId ?? string.Empty,
            context.BaseRevision ?? string.Empty,
            context.BaseCheckpointSha256 ?? string.Empty);
        return DesignConversionDpoPolicyBaseMetadata.IsValidIdentity(identity) &&
               !string.Equals(
                   context.BaseCheckpointSha256,
                   context.InitialWeightSha256,
                   StringComparison.Ordinal) &&
               string.Equals(
                   DesignConversionDpoPolicyBaseMetadata.ComputeConfigSha256(context),
                   context.ConfigSha256,
                   StringComparison.Ordinal);
    }

    private static bool ValidateDpoPolicyLoraContext(
        DesignConversionDpoPolicyCheckpointLoraContext? lora,
        int numLayers)
    {
        if (lora is null)
            return true;

        if (lora.TargetModules is null ||
            lora.AdapterCount <= 0 ||
            lora.AdapterCount != numLayers * lora.TargetModules.Count)
        {
            return false;
        }

        try
        {
            var canonical = LoraApplicator.ValidateAndCanonicalize(new LoraConfig
            {
                Rank = lora.Rank,
                Alpha = lora.Alpha,
                Dropout = lora.Dropout,
                TargetModules = lora.TargetModules.ToArray()
            });

            return canonical.Rank == lora.Rank &&
                   canonical.Alpha.Equals(lora.Alpha) &&
                   canonical.Dropout.Equals(lora.Dropout) &&
                   canonical.TargetModules.SequenceEqual(lora.TargetModules, StringComparer.Ordinal);
        }
        catch (Exception exception) when (exception is ArgumentException
                                          or InvalidOperationException
                                          or OverflowException)
        {
            return false;
        }
    }

    private static OperationResult<bool> CreateDpoPolicyCheckpointRefInvalid()
        => OperationResult<bool>.Fail(
            "Checkpoint DPO policy reference is invalid.",
            CheckpointDpoPolicyRefInvalidCode);

    private static async Task<OperationResult<DpoPolicyCheckpointPayloadSnapshot?>> ValidateDpoPolicyCheckpointPayloadFileAsync(
        DesignConversionDpoPolicyCheckpointRef? checkpointRef,
        string fullCheckpointPath,
        CancellationToken ct)
    {
        var shapeValidation = ValidateDpoPolicyCheckpointRef(checkpointRef);
        if (!shapeValidation.IsSuccess)
            return shapeValidation.MapError<DpoPolicyCheckpointPayloadSnapshot?>();

        if (checkpointRef is null ||
            !IsDpoPolicySiblingPayloadRuntimeBridgeKind(checkpointRef.RuntimeBridgeKind))
            return OperationResult<DpoPolicyCheckpointPayloadSnapshot?>.Ok(null);

        var checkpointDirectory = Path.GetDirectoryName(fullCheckpointPath);
        if (string.IsNullOrWhiteSpace(checkpointDirectory))
            return OperationResult<DpoPolicyCheckpointPayloadSnapshot?>.Fail(
                "DPO policy checkpoint directory is required for payload validation.",
                CheckpointShapeInvalidCode);

        string payloadPath;
        try
        {
            payloadPath = Path.GetFullPath(Path.Combine(checkpointDirectory, checkpointRef.PolicyCheckpointFileName));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return OperationResult<DpoPolicyCheckpointPayloadSnapshot?>.Fail(
                "DPO policy checkpoint payload file name is invalid.",
                CheckpointShapeInvalidCode);
        }

        var payloadDirectory = Path.GetDirectoryName(payloadPath);
        if (!string.Equals(payloadDirectory, checkpointDirectory, StringComparison.OrdinalIgnoreCase))
            return OperationResult<DpoPolicyCheckpointPayloadSnapshot?>.Fail(
                "DPO policy checkpoint payload must resolve next to the runtime checkpoint.",
                CheckpointShapeInvalidCode);

        if (!File.Exists(payloadPath))
            return OperationResult<DpoPolicyCheckpointPayloadSnapshot?>.Fail(
                "DPO policy checkpoint payload file was not found next to the runtime checkpoint.",
                DpoPolicyCheckpointPayloadNotFoundCode);

        byte[] payloadBytes;
        try
        {
            payloadBytes = await File.ReadAllBytesAsync(payloadPath, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return OperationResult<DpoPolicyCheckpointPayloadSnapshot?>.Fail("Load cancelled.", "CANCELLED");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return OperationResult<DpoPolicyCheckpointPayloadSnapshot?>.Fail(
                "DPO policy checkpoint payload file could not be read.",
                "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_PAYLOAD_READ_FAILED");
        }

        if (payloadBytes.Length != checkpointRef.PolicyCheckpointByteLength)
            return OperationResult<DpoPolicyCheckpointPayloadSnapshot?>.Fail(
                "DPO policy checkpoint payload byte length does not match metadata.",
                DpoPolicyCheckpointPayloadLengthMismatchCode);

        var payloadSha256 = Convert.ToHexString(SHA256.HashData(payloadBytes)).ToLowerInvariant();
        return string.Equals(payloadSha256, checkpointRef.Sha256, StringComparison.Ordinal)
            ? OperationResult<DpoPolicyCheckpointPayloadSnapshot?>.Ok(new DpoPolicyCheckpointPayloadSnapshot(payloadBytes, payloadSha256))
            : OperationResult<DpoPolicyCheckpointPayloadSnapshot?>.Fail(
                "DPO policy checkpoint payload hash does not match metadata.",
                DpoPolicyCheckpointPayloadShaMismatchCode);
    }

    private static OperationResult<bool> ValidateCheckpointNumericValues(DesignConversionMachineCheckpoint checkpoint)
    {
        if (!IsValidCheckpointAccuracy(checkpoint.AccuracyAfterLastBatch))
            return CreateCheckpointNumericValueInvalid(nameof(DesignConversionMachineCheckpoint.AccuracyAfterLastBatch));

        for (var i = 0; i < checkpoint.TrainingHistory.Count; i++)
        {
            var history = checkpoint.TrainingHistory[i];
            if (history.RowsTrained <= 0)
                return CreateCheckpointTrainingHistoryNumericValueInvalid(
                    i,
                    nameof(DesignConversionTrainingHistoryEntryCheckpoint.RowsTrained));

            if (!IsValidCheckpointAccuracy(history.AccuracyBefore))
                return CreateCheckpointTrainingHistoryNumericValueInvalid(
                    i,
                    nameof(DesignConversionTrainingHistoryEntryCheckpoint.AccuracyBefore));

            if (!IsValidCheckpointAccuracy(history.AccuracyAfter))
                return CreateCheckpointTrainingHistoryNumericValueInvalid(
                    i,
                    nameof(DesignConversionTrainingHistoryEntryCheckpoint.AccuracyAfter));

            if (!double.IsFinite(history.RewardMargin))
                return CreateCheckpointTrainingHistoryNumericValueInvalid(
                    i,
                    nameof(DesignConversionTrainingHistoryEntryCheckpoint.RewardMargin));

            if (history.CpuMs < 0)
                return CreateCheckpointTrainingHistoryNumericValueInvalid(
                    i,
                    nameof(DesignConversionTrainingHistoryEntryCheckpoint.CpuMs));

            if (history.PeakRamMb < 0)
                return CreateCheckpointTrainingHistoryNumericValueInvalid(
                    i,
                    nameof(DesignConversionTrainingHistoryEntryCheckpoint.PeakRamMb));

            if (history.ElapsedMs < 0)
                return CreateCheckpointTrainingHistoryNumericValueInvalid(
                    i,
                    nameof(DesignConversionTrainingHistoryEntryCheckpoint.ElapsedMs));
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool IsValidCheckpointAccuracy(double value)
        => double.IsFinite(value) && value >= 0.0 && value <= 1.0;

    private static OperationResult<bool> CreateCheckpointNumericValueInvalid(string field)
        => OperationResult<bool>.Fail(
            $"Checkpoint {field} is invalid.",
            CheckpointNumericValuesInvalidCode);

    private static OperationResult<bool> CreateCheckpointTrainingHistoryNumericValueInvalid(int index, string field)
        => OperationResult<bool>.Fail(
            $"Checkpoint training history entry at index {index} {field} is invalid.",
            CheckpointNumericValuesInvalidCode);

    private static OperationResult<bool> ValidateRuntimeUnitIdentities(DesignConversionMachineCheckpoint checkpoint)
    {
        var critic = ValidateRuntimeUnitIdentity(
            checkpoint.CriticUnit,
            "critic",
            DesignConversionRuntimeScaffoldCriticUnit.UnitId,
            DesignConversionRuntimeScaffoldCriticUnit.Role,
            DesignConversionRuntimeScaffoldCriticUnit.DomainTag);
        if (!critic.IsSuccess)
            return critic;

        return ValidateRuntimeUnitIdentity(
            checkpoint.ImproverUnit,
            "improver",
            DesignConversionRuntimeScaffoldImproverUnit.UnitId,
            DesignConversionRuntimeScaffoldImproverUnit.Role,
            DesignConversionRuntimeScaffoldImproverUnit.DomainTag);
    }

    private static OperationResult<bool> ValidateRuntimeUnitIdentity(
        DesignConversionRuntimeUnitCheckpoint unit,
        string slot,
        string expectedUnitId,
        string expectedRole,
        string expectedDomainTag)
    {
        if (!string.Equals(unit.UnitId, expectedUnitId, StringComparison.Ordinal))
            return CreateRuntimeUnitIdentityMismatch(slot, nameof(unit.UnitId));

        if (!string.Equals(unit.UnitRole, expectedRole, StringComparison.Ordinal))
            return CreateRuntimeUnitIdentityMismatch(slot, nameof(unit.UnitRole));

        if (!string.Equals(unit.DomainTag, expectedDomainTag, StringComparison.Ordinal))
            return CreateRuntimeUnitIdentityMismatch(slot, nameof(unit.DomainTag));

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> CreateRuntimeUnitIdentityMismatch(string slot, string field)
        => OperationResult<bool>.Fail(
            $"Checkpoint {slot} runtime unit {field} does not match the expected scaffold identity.",
            CheckpointRuntimeUnitIdentityMismatchCode);

    private static OperationResult<bool> ValidateCheckpointShape(DesignConversionMachineCheckpoint checkpoint)
    {
        if (checkpoint.CriticUnit is null)
            return OperationResult<bool>.Fail("Checkpoint critic unit is required.", CheckpointShapeInvalidCode);

        if (checkpoint.ImproverUnit is null)
            return OperationResult<bool>.Fail("Checkpoint improver unit is required.", CheckpointShapeInvalidCode);

        if (checkpoint.TrainingHistory is null)
            return OperationResult<bool>.Fail("Checkpoint training history is required.", CheckpointShapeInvalidCode);

        for (var i = 0; i < checkpoint.TrainingHistory.Count; i++)
        {
            var history = checkpoint.TrainingHistory[i];
            if (history is null)
                return OperationResult<bool>.Fail(
                    $"Checkpoint training history entry at index {i} is required.",
                    CheckpointShapeInvalidCode);

            if (history.PromotedUnitIds is null)
                return OperationResult<bool>.Fail(
                    $"Checkpoint training history entry at index {i} promoted unit ids are required.",
                    CheckpointShapeInvalidCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    public static bool CheckpointHasRealLearnedWeights(DesignConversionMachineCheckpoint? checkpoint)
        => RuntimeUnitHasRealLearnedWeights(checkpoint?.CriticUnit) ||
           RuntimeUnitHasRealLearnedWeights(checkpoint?.ImproverUnit);

    public static int CountRealLearnedWeightArtifacts(DesignConversionMachineCheckpoint? checkpoint)
        => CountRealLearnedWeightArtifacts(checkpoint?.CriticUnit) +
           CountRealLearnedWeightArtifacts(checkpoint?.ImproverUnit);

    public static int CountRealLearnedWeightObservations(DesignConversionMachineCheckpoint? checkpoint)
        => CountRealLearnedWeightObservations(checkpoint?.CriticUnit) +
           CountRealLearnedWeightObservations(checkpoint?.ImproverUnit);

    public DesignConversionLoadedLearnedWeightAuthoritySummary
        GetLoadedLearnedWeightAuthoritySummary()
    {
        lock (_stateGate)
        {
            var embeddedArtifactCount = _learnedWeightsByUnit.Values
                .Sum(static weights => weights.Count);
            var embeddedObservationCount = _learnedWeightsByUnit.Values
                .SelectMany(static weights => weights.Values)
                .Sum(static weight => weight.ObservationCount);
            var dpoPolicyAuthorityLoaded =
                _dpoPolicyRuntimeScorer is not null &&
                IsDpoPolicyDirectInferenceRuntimeBridgeKind(
                    _dpoPolicyCheckpointRef?.RuntimeBridgeKind);
            var dpoPolicyArtifactCount = dpoPolicyAuthorityLoaded ? 1 : 0;
            var dpoPolicyObservationCount = dpoPolicyAuthorityLoaded
                ? _dpoPolicyCheckpointRef!.BridgePreferenceCount
                : 0;
            var totalArtifactCount =
                embeddedArtifactCount + dpoPolicyArtifactCount;
            var totalObservationCount =
                embeddedObservationCount + dpoPolicyObservationCount;

            return new DesignConversionLoadedLearnedWeightAuthoritySummary(
                HasRealLearnedWeightAuthority: totalArtifactCount > 0,
                DpoPolicyAuthorityLoaded: dpoPolicyAuthorityLoaded,
                EmbeddedLearnedWeightArtifactCount: embeddedArtifactCount,
                DpoPolicyLearnedWeightArtifactCount: dpoPolicyArtifactCount,
                TotalLearnedWeightArtifactCount: totalArtifactCount,
                EmbeddedLearnedWeightObservationCount: embeddedObservationCount,
                DpoPolicyLearnedWeightObservationCount:
                    dpoPolicyObservationCount,
                TotalLearnedWeightObservationCount: totalObservationCount);
        }
    }

    public DesignConversionRoutingFlow CreateRuntimeRoutingFlow()
        => CreateRuntimeRoutingFlowSnapshot().Flow;

    private RuntimeRoutingFlowSnapshot CreateRuntimeRoutingFlowSnapshot()
    {
        lock (_stateGate)
        {
            var criticWeights = ExportLearnedWeights(DesignConversionRuntimeScaffoldCriticUnit.UnitId);
            var improverWeights = ExportLearnedWeights(DesignConversionRuntimeScaffoldImproverUnit.UnitId);
            var policyScorer = _dpoPolicyRuntimeScorer;
            var policyCheckpointRef = _dpoPolicyCheckpointRef;
            if (criticWeights.Count == 0 && improverWeights.Count == 0 && policyScorer is null)
                return new RuntimeRoutingFlowSnapshot(_flow, false, false);

            var policyAcceptanceAuthority = policyScorer is null
                ? null
                : BuildDirectDpoPolicyAcceptanceAuthoritySnapshot(
                    policyCheckpointRef,
                    _accuracyAfterLastBatch,
                    _evaluationOptions.HeldOutEvaluationAccuracyGateThreshold);
            return new RuntimeRoutingFlowSnapshot(
                new DesignConversionRoutingFlow(
                    criticWeights.Count > 0 || policyScorer is not null
                        ? new LearnedWeightCriticUnit(criticWeights, policyScorer, policyAcceptanceAuthority)
                        : null,
                    improverWeights.Count > 0 ? new LearnedWeightImproverUnit(improverWeights) : null),
                criticWeights.Count > 0 || improverWeights.Count > 0,
                policyScorer is not null);
        }
    }

    public async Task<OperationResult<LearningMachineExport>> SaveCheckpointAsync(
        string checkpointPath,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<LearningMachineExport>.Fail("Save cancelled.", "CANCELLED");

        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<LearningMachineExport>.Fail(
                "Checkpoint path is required.",
                "CHECKPOINT_PATH_REQUIRED");

        string fullCheckpointPath;
        try
        {
            fullCheckpointPath = Path.GetFullPath(checkpointPath);
        }
        catch (Exception ex)
        {
            return OperationResult<LearningMachineExport>.Fail(ex.Message, "CHECKPOINT_WRITE_FAILED");
        }

        var capturedState = CaptureCheckpointState();
        var checkpoint = capturedState.Checkpoint;
        try
        {
            await (CheckpointStateCaptureOverride.Value ?? DefaultCheckpointStateCapture)(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return OperationResult<LearningMachineExport>.Fail("Save cancelled.", "CANCELLED");
        }

        byte[] checkpointBytes;
        BoundCheckpointSnapshot expectedSnapshot;
        try
        {
            checkpointBytes = JsonSerializer.SerializeToUtf8Bytes(
                checkpoint,
                DesignConversionCheckpointJsonOptions.Standard);
            var checkpointSha256 = Convert.ToHexString(SHA256.HashData(checkpointBytes)).ToLowerInvariant();
            expectedSnapshot = new BoundCheckpointSnapshot(
                fullCheckpointPath,
                checkpoint.Version,
                checkpointSha256);
        }
        catch (Exception ex)
        {
            return OperationResult<LearningMachineExport>.Fail(ex.Message, "CHECKPOINT_WRITE_FAILED");
        }

        string? tmpPath = null;
        var checkpointMoved = false;
        try
        {
            var dir = Path.GetDirectoryName(fullCheckpointPath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var writerLock = TryAcquireCheckpointWriterLock(fullCheckpointPath);
            if (!writerLock.IsSuccess)
                return OperationResult<LearningMachineExport>.Fail(writerLock.ErrorMessage, writerLock.ErrorCode);

            await using var writerLease = writerLock.Value!;
            tmpPath = BuildCheckpointTempPath(fullCheckpointPath);
            await File.WriteAllBytesAsync(tmpPath, checkpointBytes, ct);

            lock (_stateGate)
            {
                if (ct.IsCancellationRequested)
                    return OperationResult<LearningMachineExport>.Fail("Save cancelled.", "CANCELLED");

                if (_stateGeneration != capturedState.Generation ||
                    !string.Equals(_currentVersion, checkpoint.Version, StringComparison.Ordinal))
                {
                    return OperationResult<LearningMachineExport>.Fail(
                        "Machine state changed while checkpoint bytes were being persisted.",
                        ExportCheckpointVersionStaleCode);
                }

                File.Move(tmpPath, fullCheckpointPath, overwrite: true);
                checkpointMoved = true;
                BindCheckpointSnapshotUnsafe(expectedSnapshot);
                _stateGeneration++;
                return OperationResult<LearningMachineExport>.Ok(CreateBoundExport(
                    expectedSnapshot,
                    checkpoint.AccuracyAfterLastBatch,
                    DateTimeOffset.UtcNow));
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return OperationResult<LearningMachineExport>.Fail("Save cancelled.", "CANCELLED");
        }
        catch (Exception ex)
        {
            return OperationResult<LearningMachineExport>.Fail(ex.Message, "CHECKPOINT_WRITE_FAILED");
        }
        finally
        {
            if (!checkpointMoved && tmpPath is not null)
                TryDeleteExactTempFile(tmpPath);
        }
    }

    private static async Task<OperationResult<string>> ReadFileSha256Async(
        string fullCheckpointPath,
        CancellationToken ct)
    {
        if (!File.Exists(fullCheckpointPath))
            return OperationResult<string>.Fail(
                $"Checkpoint file not found: {fullCheckpointPath}",
                "CHECKPOINT_NOT_FOUND");

        try
        {
            var bytes = await File.ReadAllBytesAsync(fullCheckpointPath, ct);
            return OperationResult<string>.Ok(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return OperationResult<string>.Fail("Checkpoint read cancelled.", "CANCELLED");
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Fail(ex.Message, "CHECKPOINT_READ_FAILED");
        }
    }

    public static string GetCheckpointWriterLockPath(string checkpointPath)
    {
        if (string.IsNullOrWhiteSpace(checkpointPath))
            throw new ArgumentException("Checkpoint path is required.", nameof(checkpointPath));

        var fullCheckpointPath = Path.GetFullPath(checkpointPath);
        var dir = Path.GetDirectoryName(fullCheckpointPath);
        var fileName = Path.GetFileName(fullCheckpointPath);
        return Path.Combine(dir ?? string.Empty, $".{fileName}.write.lock");
    }

    private static OperationResult<DesignConversionDeterministicMutationLease> TryAcquireCheckpointWriterLock(
        string fullCheckpointPath)
        => DesignConversionDeterministicMutationLock.TryAcquire(
            GetCheckpointWriterLockPath(fullCheckpointPath),
            "Checkpoint writer lock is already held for this path.",
            CheckpointWriteConflictCode,
            "CHECKPOINT_WRITE_FAILED");

    private static string BuildCheckpointTempPath(string fullCheckpointPath)
    {
        var dir = Path.GetDirectoryName(fullCheckpointPath) ?? string.Empty;
        var fileName = Path.GetFileName(fullCheckpointPath);
        return Path.Combine(dir, $".{fileName}.{Guid.NewGuid():N}.tmp");
    }

    private static void TryDeleteExactTempFile(string tmpPath)
    {
        try
        {
            if (File.Exists(tmpPath))
                File.Delete(tmpPath);
        }
        catch
        {
            // Best-effort cleanup only for this call's unique temp file.
        }
    }

    public static async Task<OperationResult<DesignConversionMachineLoadResult>> LoadCheckpointAsync(
        string checkpointPath,
        CancellationToken ct = default)
        => await LoadCheckpointAsync(
            checkpointPath,
            CheckpointByteReaderOverride.Value ?? DefaultCheckpointByteReader,
            ct);

    public static async Task<OperationResult<DesignConversionMachineLoadResult>> LoadCheckpointWithEvaluationOptionsAsync(
        string checkpointPath,
        DesignConversionMachineEvaluationOptions evaluationOptions,
        CancellationToken ct = default)
        => await LoadCheckpointAsync(
            checkpointPath,
            CheckpointByteReaderOverride.Value ?? DefaultCheckpointByteReader,
            evaluationOptions,
            ct);

    internal static IDisposable UseCheckpointByteReaderForCurrentAsyncFlow(
        Func<string, CancellationToken, Task<byte[]>> checkpointByteReader)
    {
        ArgumentNullException.ThrowIfNull(checkpointByteReader);

        var previous = CheckpointByteReaderOverride.Value;
        CheckpointByteReaderOverride.Value = checkpointByteReader;
        return new CheckpointByteReaderOverrideScope(previous);
    }

    internal static IDisposable UseCheckpointStateCaptureForCurrentAsyncFlow(
        Func<CancellationToken, Task> checkpointStateCapture)
    {
        ArgumentNullException.ThrowIfNull(checkpointStateCapture);

        var previous = CheckpointStateCaptureOverride.Value;
        CheckpointStateCaptureOverride.Value = checkpointStateCapture;
        return new CheckpointStateCaptureOverrideScope(previous);
    }

    internal static async Task<OperationResult<DesignConversionMachineLoadResult>> LoadCheckpointAsync(
        string checkpointPath,
        Func<string, CancellationToken, Task<byte[]>> checkpointByteReader,
        CancellationToken ct = default)
        => await LoadCheckpointAsync(
            checkpointPath,
            checkpointByteReader,
            DesignConversionMachineEvaluationOptions.Default,
            ct);

    internal static async Task<OperationResult<DesignConversionMachineLoadResult>> LoadCheckpointAsync(
        string checkpointPath,
        Func<string, CancellationToken, Task<byte[]>> checkpointByteReader,
        DesignConversionMachineEvaluationOptions evaluationOptions,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionMachineLoadResult>.Fail("Load cancelled.", "CANCELLED");

        if (evaluationOptions is null)
            return OperationResult<DesignConversionMachineLoadResult>.Fail(
                "Evaluation options are required.",
                "EVALUATION_OPTIONS_REQUIRED");

        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<DesignConversionMachineLoadResult>.Fail(
                "Checkpoint path is required.",
                "CHECKPOINT_PATH_REQUIRED");

        string fullCheckpointPath;
        try
        {
            fullCheckpointPath = Path.GetFullPath(checkpointPath);
        }
        catch (Exception ex)
        {
            return OperationResult<DesignConversionMachineLoadResult>.Fail(ex.Message, "CHECKPOINT_PATH_INVALID");
        }

        if (!File.Exists(fullCheckpointPath))
            return OperationResult<DesignConversionMachineLoadResult>.Fail(
                $"Checkpoint file not found: {fullCheckpointPath}",
                "CHECKPOINT_NOT_FOUND");

        DesignConversionMachineCheckpoint checkpoint;
        string checkpointSha256;
        int checkpointByteLength;
        DpoPolicyCheckpointPayloadSnapshot? dpoPolicyPayloadSnapshot;
        try
        {
            var checkpointBytes = await checkpointByteReader(fullCheckpointPath, ct);
            checkpointByteLength = checkpointBytes.Length;
            checkpointSha256 = Convert.ToHexString(SHA256.HashData(checkpointBytes)).ToLowerInvariant();
            var json = DecodeCheckpointJsonSnapshot(checkpointBytes);
            var parsed = JsonSerializer.Deserialize<DesignConversionMachineCheckpoint>(
                json,
                DesignConversionCheckpointJsonOptions.Standard);
            if (parsed is null)
                return OperationResult<DesignConversionMachineLoadResult>.Fail(
                    "Checkpoint deserialisation returned null.",
                    "CHECKPOINT_DESERIALIZE_FAILED");
            checkpoint = parsed;
            var policyCheckpointPayloadValidation =
                await ValidateDpoPolicyCheckpointPayloadFileAsync(checkpoint.DpoPolicyCheckpointRef, fullCheckpointPath, ct);
            if (!policyCheckpointPayloadValidation.IsSuccess)
                return policyCheckpointPayloadValidation.MapError<DesignConversionMachineLoadResult>();
            dpoPolicyPayloadSnapshot = policyCheckpointPayloadValidation.Value;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return OperationResult<DesignConversionMachineLoadResult>.Fail("Load cancelled.", "CANCELLED");
        }
        catch (Exception ex)
        {
            return OperationResult<DesignConversionMachineLoadResult>.Fail(ex.Message, "CHECKPOINT_READ_FAILED");
        }

        var dpoPolicyRuntimeScorer = CreateDpoPolicyRuntimeScorer(
            checkpoint.DpoPolicyCheckpointRef,
            dpoPolicyPayloadSnapshot,
            ct);
        if (!dpoPolicyRuntimeScorer.IsSuccess)
            return dpoPolicyRuntimeScorer.MapError<DesignConversionMachineLoadResult>();

        var machine = new DesignConversionMachine(new DesignConversionRoutingFlow(), evaluationOptions);
        var importResult = machine.ImportCheckpointCore(
            checkpoint,
            boundCheckpointSnapshot: null,
            dpoPolicyRuntimeScorer.Value);
        if (!importResult.IsSuccess)
            return OperationResult<DesignConversionMachineLoadResult>.Fail(importResult.ErrorMessage, importResult.ErrorCode);

        machine.BindCheckpointSnapshot(new BoundCheckpointSnapshot(
            fullCheckpointPath,
            checkpoint.Version,
            checkpointSha256));

        DesignConversionVerifiedDpoPolicyRuntimeScorerLoaded? verifiedDpoPolicyRuntime = null;
        if (dpoPolicyRuntimeScorer.Value is not null &&
            dpoPolicyPayloadSnapshot is not null &&
            checkpoint.DpoPolicyCheckpointRef is { } policyCheckpointRef)
        {
            var checkpointDirectory = Path.GetDirectoryName(fullCheckpointPath)!;
            var policyCheckpointPath = Path.GetFullPath(
                Path.Combine(
                    checkpointDirectory,
                    policyCheckpointRef.PolicyCheckpointFileName));
            verifiedDpoPolicyRuntime =
                new DesignConversionVerifiedDpoPolicyRuntimeScorerLoaded(
                    fullCheckpointPath,
                    checkpointSha256,
                    checkpointByteLength,
                    policyCheckpointPath,
                    dpoPolicyPayloadSnapshot.Sha256,
                    dpoPolicyPayloadSnapshot.Bytes.Length,
                    SnapshotDpoPolicyCheckpointRef(policyCheckpointRef)!);
        }

        return OperationResult<DesignConversionMachineLoadResult>.Ok(
            new DesignConversionMachineLoadResult(
                machine,
                checkpoint,
                checkpointSha256)
            {
                VerifiedDpoPolicyRuntimeScorerLoaded = verifiedDpoPolicyRuntime
            });
    }

    private static OperationResult<DpoPolicyRuntimeScorer?> CreateDpoPolicyRuntimeScorer(
        DesignConversionDpoPolicyCheckpointRef? checkpointRef,
        DpoPolicyCheckpointPayloadSnapshot? payloadSnapshot,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DpoPolicyRuntimeScorer?>.Fail("Load cancelled.", "CANCELLED");

        if (checkpointRef is null ||
            !IsDpoPolicyDirectInferenceRuntimeBridgeKind(checkpointRef.RuntimeBridgeKind))
        {
            return OperationResult<DpoPolicyRuntimeScorer?>.Ok(null);
        }

        if (checkpointRef.TrainingContext.SchemaVersion != 4)
        {
            return OperationResult<DpoPolicyRuntimeScorer?>.Fail(
                DpoPolicyRuntimeSchemaUnsupportedMessage,
                DpoPolicyRuntimeSchemaUnsupportedCode);
        }

        if (!DesignConversionDpoPolicyRandomContextContract.IsValidCurrent(
                checkpointRef.TrainingContext.RandomContext))
        {
            return OperationResult<DpoPolicyRuntimeScorer?>.Fail(
                "Checkpoint DPO policy reference is invalid.",
                CheckpointDpoPolicyRefInvalidCode);
        }

        if (payloadSnapshot is null)
        {
            return OperationResult<DpoPolicyRuntimeScorer?>.Fail(
                "Direct DPO policy inference checkpoint payload was not validated.",
                DpoPolicyCheckpointPayloadModelLoadFailedCode);
        }

        var config = BuildDpoPolicyModelConfig(checkpointRef.TrainingContext);
        var runtimeEncodingBinding = ExtractDpoPolicyRuntimeEncodingBinding(
            checkpointRef,
            config);
        if (!runtimeEncodingBinding.IsSuccess)
            return runtimeEncodingBinding.MapError<DpoPolicyRuntimeScorer?>();

        var model = new LlmModel(config);
        var loraConfig = BuildDpoPolicyRuntimeLoraConfig(checkpointRef.TrainingContext);
        if (!loraConfig.IsSuccess)
            return loraConfig.MapError<DpoPolicyRuntimeScorer?>();

        try
        {
            if (loraConfig.Value is not null)
            {
                var randomContext = checkpointRef.TrainingContext.RandomContext!;
                var adapters = LoraApplicator.Apply(
                    model,
                    loraConfig.Value,
                    (blockIndex, projection) =>
                        LoraAdapterRandomSeedDerivation.Derive(
                            randomContext.RootInitializationSeed,
                            blockIndex,
                            projection,
                            randomContext.PolicyDropoutStreamDomain));
                if (adapters.Count != checkpointRef.TrainingContext.Lora!.AdapterCount)
                {
                    return OperationResult<DpoPolicyRuntimeScorer?>.Fail(
                        "DPO policy checkpoint LoRA topology metadata did not match the runtime model.",
                        DpoPolicyCheckpointPayloadModelLoadFailedCode);
                }
            }

            using var stream = new MemoryStream(payloadSnapshot.Bytes, writable: false);
            model.LoadCheckpoint(stream);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return OperationResult<DpoPolicyRuntimeScorer?>.Fail("Load cancelled.", "CANCELLED");
        }
        catch (Exception)
        {
            return OperationResult<DpoPolicyRuntimeScorer?>.Fail(
                "DPO policy checkpoint payload passed metadata validation but could not be loaded as a full LlmModel checkpoint.",
                DpoPolicyCheckpointPayloadModelLoadFailedCode);
        }

        var evidenceRefs = BuildDpoPolicyRuntimeCheckpointEvidenceRefs(
            checkpointRef,
            loraTopologyRebuilt: loraConfig.Value is not null,
            runtimeEncodingBinding.Value?.EvidenceRefs ?? []);

        return OperationResult<DpoPolicyRuntimeScorer?>.Ok(new DpoPolicyRuntimeScorer(
            model,
            config,
            checkpointRef.Sha256,
            checkpointRef.TrainingContext.ConfigSha256 ?? string.Empty,
            evidenceRefs,
            runtimeEncodingBinding.Value?.Binding));
    }

    private static string[] BuildDpoPolicyRuntimeCheckpointEvidenceRefs(
        DesignConversionDpoPolicyCheckpointRef checkpointRef,
        bool loraTopologyRebuilt,
        IReadOnlyList<string> validatedRuntimeEncodingEvidenceRefs)
    {
        var requiredEvidenceRefs = BuildRequiredDpoPolicyEvidenceRefs(
            checkpointRef,
            checkpointRef.TrainingContext,
            usesSiblingPayload: true);
        var requiredEvidenceRefSet = new HashSet<string>(
            requiredEvidenceRefs,
            StringComparer.Ordinal);

        var evidenceRefs = checkpointRef.EvidenceRefs
            .Where(evidence =>
                requiredEvidenceRefSet.Contains(evidence) &&
                !IsReservedCheckpointProvidedDirectPolicyAuthorityEvidence(evidence))
            .Distinct(StringComparer.Ordinal);

        evidenceRefs = evidenceRefs.Concat(validatedRuntimeEncodingEvidenceRefs ?? []);

        if (loraTopologyRebuilt)
            evidenceRefs = evidenceRefs.Append(DpoPolicyRuntimeLoraTopologyEvidence);

        return evidenceRefs
            .Where(evidence => !string.Equals(
                evidence,
                DpoPolicyContinuationRuntimeEncodingAppliedEvidence,
                StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static OperationResult<DpoPolicyRuntimeEncodingBindingSnapshot?>
        ExtractDpoPolicyRuntimeEncodingBinding(
            DesignConversionDpoPolicyCheckpointRef checkpointRef,
            ModelConfig config)
    {
        var evidenceRefs = checkpointRef.EvidenceRefs ?? [];
        var hasAnyRuntimeEncodingEvidence =
            evidenceRefs.Any(evidence => string.Equals(
                evidence,
                DpoPolicyContinuationRuntimeEncodingBoundEvidence,
                StringComparison.Ordinal)) ||
            evidenceRefs.Any(IsDpoRuntimeEncodingMarkerEvidence);
        if (!hasAnyRuntimeEncodingEvidence)
        {
            return RuntimeEncodingBindingMatchesCheckpointContract(
                    checkpointRef,
                    binding: null)
                ? OperationResult<
                    DpoPolicyRuntimeEncodingBindingSnapshot?>.Ok(null)
                : CreateDpoPolicyRuntimeEncodingInvalid<
                    DpoPolicyRuntimeEncodingBindingSnapshot?>();
        }

        if (evidenceRefs.Count(evidence => string.Equals(
                evidence,
                DpoPolicyContinuationRuntimeEncodingBoundEvidence,
                StringComparison.Ordinal)) != 1)
        {
            return CreateDpoPolicyRuntimeEncodingInvalid<
                DpoPolicyRuntimeEncodingBindingSnapshot?>();
        }

        var bindingSchema = ReadRequiredDpoRuntimeEncodingEvidenceValue(
            evidenceRefs,
            DpoRuntimeEncodingSchemaEvidencePrefix);
        var sourceDescriptorSchema = ReadRequiredDpoRuntimeEncodingEvidenceValue(
            evidenceRefs,
            DpoRuntimeEncodingSourceDescriptorSchemaEvidencePrefix);
        var tokenMapping = ReadRequiredDpoRuntimeEncodingEvidenceValue(
            evidenceRefs,
            DpoRuntimeEncodingTokenMappingEvidencePrefix);
        var tokenIdMinimumText = ReadRequiredDpoRuntimeEncodingEvidenceValue(
            evidenceRefs,
            DpoRuntimeEncodingTokenIdMinimumEvidencePrefix);
        var tokenIdMaximumText = ReadRequiredDpoRuntimeEncodingEvidenceValue(
            evidenceRefs,
            DpoRuntimeEncodingTokenIdMaximumEvidencePrefix);
        var contextLengthText = ReadRequiredDpoRuntimeEncodingEvidenceValue(
            evidenceRefs,
            DpoRuntimeEncodingContextLengthEvidencePrefix);
        var tokenizerSha256 = ReadRequiredDpoRuntimeEncodingEvidenceValue(
            evidenceRefs,
            DpoRuntimeEncodingTokenizerSha256EvidencePrefix);
        var vocabularySha256 = ReadRequiredDpoRuntimeEncodingEvidenceValue(
            evidenceRefs,
            DpoRuntimeEncodingVocabularySha256EvidencePrefix);
        if (!bindingSchema.IsSuccess ||
            !sourceDescriptorSchema.IsSuccess ||
            !tokenMapping.IsSuccess ||
            !tokenIdMinimumText.IsSuccess ||
            !tokenIdMaximumText.IsSuccess ||
            !contextLengthText.IsSuccess ||
            !tokenizerSha256.IsSuccess ||
            !vocabularySha256.IsSuccess)
        {
            return CreateDpoPolicyRuntimeEncodingInvalid<
                DpoPolicyRuntimeEncodingBindingSnapshot?>();
        }

        if (!TryParseCanonicalInvariantInt(tokenIdMinimumText.Value, out var tokenIdMinimum) ||
            !TryParseCanonicalInvariantInt(tokenIdMaximumText.Value, out var tokenIdMaximum) ||
            !TryParseCanonicalInvariantInt(contextLengthText.Value, out var contextLength))
        {
            return CreateDpoPolicyRuntimeEncodingInvalid<
                DpoPolicyRuntimeEncodingBindingSnapshot?>();
        }

        var binding = new DesignConversionDpoRuntimeInferenceEncodingBinding(
            bindingSchema.Value!,
            sourceDescriptorSchema.Value!,
            tokenMapping.Value!,
            tokenIdMinimum,
            tokenIdMaximum,
            contextLength,
            tokenizerSha256.Value!,
            vocabularySha256.Value!);
        var validated =
            DesignConversionDpoPolicyCheckpointBridge
                .ValidateRuntimeInferenceEncodingBinding(binding, config);
        if (!validated.IsSuccess ||
            validated.Value is null ||
            !RuntimeEncodingBindingMatchesCheckpointContract(
                checkpointRef,
                validated.Value))
        {
            return CreateDpoPolicyRuntimeEncodingInvalid<
                DpoPolicyRuntimeEncodingBindingSnapshot?>();
        }

        return OperationResult<DpoPolicyRuntimeEncodingBindingSnapshot?>.Ok(
            new DpoPolicyRuntimeEncodingBindingSnapshot(
                validated.Value,
                [
                    DpoPolicyContinuationRuntimeEncodingBoundEvidence,
                    $"{DpoRuntimeEncodingSchemaEvidencePrefix}{validated.Value.BindingSchema}",
                    $"{DpoRuntimeEncodingSourceDescriptorSchemaEvidencePrefix}{validated.Value.SourceDescriptorSchema}",
                    $"{DpoRuntimeEncodingTokenMappingEvidencePrefix}{validated.Value.PolicyTokenMapping}",
                    $"{DpoRuntimeEncodingTokenIdMinimumEvidencePrefix}{validated.Value.TokenIdMinimum.ToString(CultureInfo.InvariantCulture)}",
                    $"{DpoRuntimeEncodingTokenIdMaximumEvidencePrefix}{validated.Value.TokenIdMaximum.ToString(CultureInfo.InvariantCulture)}",
                    $"{DpoRuntimeEncodingContextLengthEvidencePrefix}{validated.Value.ContextLength.ToString(CultureInfo.InvariantCulture)}",
                    $"{DpoRuntimeEncodingTokenizerSha256EvidencePrefix}{validated.Value.TokenizerArtifactSha256}",
                    $"{DpoRuntimeEncodingVocabularySha256EvidencePrefix}{validated.Value.TokenizerVocabularyIdentitySha256}"
                ]));
    }

    private static bool RuntimeEncodingBindingMatchesCheckpointContract(
        DesignConversionDpoPolicyCheckpointRef checkpointRef,
        DesignConversionDpoRuntimeInferenceEncodingBinding? binding)
    {
        if (!IsDpoPolicyDirectInferenceRuntimeBridgeKind(
                checkpointRef.RuntimeBridgeKind))
        {
            return false;
        }

        var requiresEvaluatedPairAware =
            RequiresEvaluatedPairAwareRuntimeEncoding(checkpointRef);
        if (requiresEvaluatedPairAware)
        {
            return binding is not null &&
                   string.Equals(
                       binding.SourceDescriptorSchema,
                       DesignConversionDpoSourceEncoderContractBuilder
                           .EvaluatedDescriptorSchema,
                       StringComparison.Ordinal);
        }

        return binding is null ||
               string.Equals(
                   binding.SourceDescriptorSchema,
                   DesignConversionDpoSourceEncoderContractBuilder
                       .DescriptorSchema,
                   StringComparison.Ordinal);
    }

    private static bool RequiresEvaluatedPairAwareRuntimeEncoding(
        DesignConversionDpoPolicyCheckpointRef checkpointRef)
    {
        var evaluation =
            checkpointRef.EvaluatedHeldOutAcceptanceAuthority;
        return evaluation is not null &&
                (string.Equals(
                    evaluation.Schema,
                    DesignConversionDpoPolicyCheckpointBridge.EvaluatedSchema,
                    StringComparison.Ordinal) ||
                string.Equals(
                    evaluation.Schema,
                    DesignConversionDpoPolicyCheckpointBridge.PreviousEvaluatedSchema,
                    StringComparison.Ordinal) ||
                string.Equals(
                    evaluation.ReferenceMode,
                    "immediate_parent",
                    StringComparison.Ordinal));
    }

    private static OperationResult<string> ReadRequiredDpoRuntimeEncodingEvidenceValue(
        IReadOnlyList<string> evidenceRefs,
        string prefix)
    {
        var matches = evidenceRefs
            .Where(evidence => evidence.StartsWith(prefix, StringComparison.Ordinal))
            .ToArray();
        if (matches.Length != 1)
            return CreateDpoPolicyRuntimeEncodingInvalid<string>();
        var value = matches[0][prefix.Length..];
        return string.IsNullOrEmpty(value)
            ? CreateDpoPolicyRuntimeEncodingInvalid<string>()
            : OperationResult<string>.Ok(value);
    }

    private static bool IsDpoRuntimeEncodingMarkerEvidence(string evidence)
        => evidence.StartsWith(DpoRuntimeEncodingSchemaEvidencePrefix, StringComparison.Ordinal) ||
           evidence.StartsWith(DpoRuntimeEncodingSourceDescriptorSchemaEvidencePrefix, StringComparison.Ordinal) ||
           evidence.StartsWith(DpoRuntimeEncodingTokenMappingEvidencePrefix, StringComparison.Ordinal) ||
           evidence.StartsWith(DpoRuntimeEncodingTokenIdMinimumEvidencePrefix, StringComparison.Ordinal) ||
           evidence.StartsWith(DpoRuntimeEncodingTokenIdMaximumEvidencePrefix, StringComparison.Ordinal) ||
           evidence.StartsWith(DpoRuntimeEncodingContextLengthEvidencePrefix, StringComparison.Ordinal) ||
           evidence.StartsWith(DpoRuntimeEncodingTokenizerSha256EvidencePrefix, StringComparison.Ordinal) ||
           evidence.StartsWith(DpoRuntimeEncodingVocabularySha256EvidencePrefix, StringComparison.Ordinal);

    private static bool TryParseCanonicalInvariantInt(string? value, out int parsed)
    {
        parsed = 0;
        if (string.IsNullOrEmpty(value) ||
            value.Any(ch => ch is < '0' or > '9') ||
            !int.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out parsed))
        {
            return false;
        }

        return string.Equals(
            parsed.ToString(CultureInfo.InvariantCulture),
            value,
            StringComparison.Ordinal);
    }

    private static OperationResult<T> CreateDpoPolicyRuntimeEncodingInvalid<T>()
        => OperationResult<T>.Fail(
            "Direct DPO policy continuation runtime encoding binding evidence is invalid.",
            DesignConversionDpoPolicyCheckpointBridge
                .ContinuationRuntimeEncodingInvalidErrorCode);

    private static bool IsReservedCheckpointProvidedDirectPolicyAuthorityEvidence(string evidence)
        => string.Equals(evidence, DpoPolicyDirectInferenceAcceptanceAuthorityEvidence, StringComparison.Ordinal) ||
           string.Equals(evidence, DpoPolicyDirectInferenceAcceptanceAuthorityScoreEvidence, StringComparison.Ordinal) ||
           string.Equals(evidence, DpoPolicyDirectInferenceAcceptanceAuthorityConfiguredEvidence, StringComparison.Ordinal) ||
           evidence.StartsWith("direct-dpo-policy-acceptance-authority", StringComparison.Ordinal);

    private static DirectDpoPolicyAcceptanceAuthoritySnapshot BuildDirectDpoPolicyAcceptanceAuthoritySnapshot(
        DesignConversionDpoPolicyCheckpointRef? checkpointRef,
        double checkpointAccuracyAfterLastBatch,
        double configuredThreshold)
    {
        if (!IsValidCheckpointAccuracy(checkpointAccuracyAfterLastBatch))
        {
            return new DirectDpoPolicyAcceptanceAuthoritySnapshot(
                false,
                "checkpoint_accuracy_invalid",
                checkpointAccuracyAfterLastBatch,
                configuredThreshold,
                []);
        }

        if (checkpointRef?.EvaluatedHeldOutAcceptanceAuthority is not { } evaluatedAuthority)
        {
            return new DirectDpoPolicyAcceptanceAuthoritySnapshot(
                false,
                "current_cycle_evaluated_heldout_runtime_evidence_missing",
                checkpointAccuracyAfterLastBatch,
                configuredThreshold,
                []);
        }

        if (!TryBuildCurrentCycleEvaluatedHeldOutRuntimeCheckpointEvidence(
                checkpointRef,
                evaluatedAuthority,
                checkpointAccuracyAfterLastBatch,
                out var canonicalHeldOutEvidenceRefs))
        {
            return new DirectDpoPolicyAcceptanceAuthoritySnapshot(
                false,
                "typed_evaluated_heldout_authority_snapshot_invalid",
                checkpointAccuracyAfterLastBatch,
                configuredThreshold,
                []);
        }

        var lineage = evaluatedAuthority.CycleAccuracyLineage!;
        var continuedTraining = string.Equals(
            evaluatedAuthority.ReferenceMode,
            "immediate_parent",
            StringComparison.Ordinal);
        var immediateParentLineage = lineage.HasParentMeasurement &&
            lineage.ParentCycleOrdinal is >= 1 &&
            lineage.CycleOrdinal == lineage.ParentCycleOrdinal.Value + 1;
        var g3Admission = DesignConversionG3AcceptancePolicy.Evaluate(
            evaluatedAuthority.G3Metrics!,
            new DesignConversionG3AcceptanceEvidenceChain(
                TypedProofVerified: true,
                FreshLoadVerified:
                    checkpointRef.FreshLoadVerified && evaluatedAuthority.FreshLoadVerified,
                ContinuedTrainingVerified: continuedTraining,
                ImmediateParentLineageVerified: immediateParentLineage),
            measurementNoisePassport: evaluatedAuthority.MeasurementNoisePassport);

        if (!g3Admission.Eligible)
            return new DirectDpoPolicyAcceptanceAuthoritySnapshot(
                false,
                g3Admission.Reason,
                checkpointAccuracyAfterLastBatch,
                configuredThreshold,
                canonicalHeldOutEvidenceRefs);

        return new DirectDpoPolicyAcceptanceAuthoritySnapshot(
            true,
            DesignConversionG3AcceptancePolicy.EligibleReason,
            checkpointAccuracyAfterLastBatch,
            configuredThreshold,
            canonicalHeldOutEvidenceRefs);
    }

    internal static (bool Active, string Reason) BuildDirectDpoG3AcceptanceAuthorityPreviewForTests(
        DesignConversionDpoPolicyCheckpointRef? checkpointRef,
        double checkpointAccuracyAfterLastBatch,
        double configuredRuntimeScoreThreshold =
            DesignConversionMachineEvaluationOptions.DefaultHeldOutEvaluationAccuracyGateThreshold)
    {
        var snapshot = BuildDirectDpoPolicyAcceptanceAuthoritySnapshot(
            checkpointRef,
            checkpointAccuracyAfterLastBatch,
            configuredRuntimeScoreThreshold);
        return (snapshot.Active, snapshot.Reason);
    }

    private static bool TryBuildCurrentCycleEvaluatedHeldOutRuntimeCheckpointEvidence(
        DesignConversionDpoPolicyCheckpointRef checkpointRef,
        DesignConversionDpoHeldOutEvaluation evaluation,
        double checkpointAccuracyAfterLastBatch,
        out string[] canonicalEvidenceRefs)
    {
        canonicalEvidenceRefs = [];
        var context = checkpointRef.TrainingContext;
        var currentSchema = string.Equals(
            evaluation.Schema,
            DesignConversionDpoPolicyCheckpointBridge.EvaluatedSchema,
            StringComparison.Ordinal);
        if (!currentSchema)
            return false;

        if (!string.Equals(evaluation.PackingId, DesignConversionDpoPolicyCheckpointBridge.EvaluatedPackingId, StringComparison.Ordinal) ||
            evaluation.VocabSize != context.VocabSize ||
            evaluation.ContextLength != context.ContextLength ||
            evaluation.DpoStepCount != checkpointRef.DpoStepCount ||
            evaluation.MaxBridgePreferenceCount < checkpointRef.BridgePreferenceCount ||
            !IsCanonicalLowercaseSha256Hex(evaluation.ChildPolicyCheckpointSha256) ||
            !IsCanonicalLowercaseSha256Hex(evaluation.ReferencePolicyCheckpointSha256) ||
            !string.Equals(evaluation.ChildPolicyCheckpointSha256, checkpointRef.Sha256, StringComparison.Ordinal) ||
            !string.Equals(evaluation.ReferencePolicyCheckpointSha256, context.BaseCheckpointSha256, StringComparison.Ordinal) ||
            string.Equals(evaluation.ChildPolicyCheckpointSha256, evaluation.ReferencePolicyCheckpointSha256, StringComparison.Ordinal) ||
            evaluation.ReferenceMode is not ("provider_fresh_step_zero" or "immediate_parent") ||
            !IsSafeDpoEvaluationDatasetId(evaluation.DatasetId) ||
            !IsCanonicalLowercaseSha256Hex(evaluation.PartitionSnapshotSha256) ||
            !IsCanonicalLowercaseSha256Hex(evaluation.TrainRecordSetSha256) ||
            !IsCanonicalLowercaseSha256Hex(evaluation.HeldOutRecordSetSha256) ||
            !IsCanonicalLowercaseSha256Hex(evaluation.TrainSourceSetSha256) ||
            !IsCanonicalLowercaseSha256Hex(evaluation.HeldOutSourceSetSha256) ||
            !IsCanonicalLowercaseSha256Hex(evaluation.SelectedTrainInputSha256) ||
            !IsCanonicalLowercaseSha256Hex(evaluation.HeldOutEvaluationInputSha256) ||
            string.Equals(evaluation.TrainRecordSetSha256, evaluation.HeldOutRecordSetSha256, StringComparison.Ordinal) ||
            string.Equals(evaluation.TrainSourceSetSha256, evaluation.HeldOutSourceSetSha256, StringComparison.Ordinal) ||
            string.Equals(evaluation.SelectedTrainInputSha256, evaluation.HeldOutEvaluationInputSha256, StringComparison.Ordinal) ||
            evaluation.TrainRecordCount != checkpointRef.BridgePreferenceCount ||
            evaluation.HeldOutRecordCount <= 0 ||
            evaluation.TrainRecordCount + evaluation.HeldOutRecordCount != checkpointRef.SourceRecordCount ||
            evaluation.EvaluatedHeldOutRecordCount != evaluation.HeldOutRecordCount ||
            evaluation.CorrectCount < 0 ||
            evaluation.CorrectCount > evaluation.EvaluatedHeldOutRecordCount ||
            !IsValidCheckpointAccuracy(evaluation.Accuracy) ||
            !evaluation.Accuracy.Equals(checkpointAccuracyAfterLastBatch) ||
            Math.Abs(evaluation.Accuracy - (double)evaluation.CorrectCount / evaluation.EvaluatedHeldOutRecordCount) > 1e-12 ||
            evaluation.G3Metrics is not { } g3Metrics ||
            !DesignConversionG3AcceptancePolicy.MetricsAreValid(g3Metrics) ||
            !DesignConversionG3AcceptancePolicy.MeasurementNoiseScopeMatches(
                g3Metrics,
                DesignConversionDpoPolicyCheckpointBridge
                    .EvaluatedMeasurementNoiseScorerIdentity,
                evaluation.DatasetId) ||
            g3Metrics.HeldOutCount != evaluation.EvaluatedHeldOutRecordCount ||
            g3Metrics.BaselineWinCount != evaluation.CorrectCount ||
            Math.Abs(g3Metrics.BaselineWinRate - evaluation.Accuracy) > 1e-12 ||
            !double.IsFinite(evaluation.MeanRelativeMargin) ||
            !double.IsFinite(evaluation.MeanChildMargin) ||
            !double.IsFinite(evaluation.MeanReferenceMargin) ||
            Math.Abs(evaluation.MeanRelativeMargin - (evaluation.MeanChildMargin - evaluation.MeanReferenceMargin)) > 1e-9 ||
            !evaluation.ReferenceFrozen ||
            !evaluation.SourceDisjoint ||
            !evaluation.HeldOutNotTrained ||
            !ValidateDpoHeldOutMeasurementNoise(evaluation) ||
            !ValidateDpoHeldOutCycleAccuracyLineage(
                evaluation.CycleAccuracyLineage,
                evaluation,
                legacySchema: false))
        {
            return false;
        }

        const string accuracyValuePrefix = "dpo_runtime_checkpoint_accuracy_value:";
        const string heldOutCountPrefix = "dpo_runtime_checkpoint_accuracy_evaluated_heldout_count:";
        const string inputSha256Prefix = "dpo_runtime_checkpoint_accuracy_input_sha256:";
        canonicalEvidenceRefs =
        [
            DpoRuntimeCheckpointAccuracyFromHeldOutEvaluationEvidence,
            DpoRuntimeCheckpointAccuracyCurrentCycleHeldOutEvaluationScopeEvidence,
            $"{accuracyValuePrefix}{evaluation.Accuracy.ToString("R", CultureInfo.InvariantCulture)}",
            $"{heldOutCountPrefix}{evaluation.EvaluatedHeldOutRecordCount.ToString(CultureInfo.InvariantCulture)}",
            $"{inputSha256Prefix}{evaluation.HeldOutEvaluationInputSha256}",
            $"g3_baseline_win_count:{g3Metrics.BaselineWinCount.ToString(CultureInfo.InvariantCulture)}",
            $"g3_baseline_win_rate:{g3Metrics.BaselineWinRate.ToString("R", CultureInfo.InvariantCulture)}",
            $"g3_absolute_child_preference_correct_count:{g3Metrics.AbsoluteChildPreferenceCorrectCount.ToString(CultureInfo.InvariantCulture)}",
            $"g3_absolute_child_preference_accuracy:{g3Metrics.AbsoluteChildPreferenceAccuracy.ToString("R", CultureInfo.InvariantCulture)}",
            $"measurement_noise_status:{evaluation.MeasurementNoiseStatus}"
        ];
        if (evaluation.MeasurementNoisePassport is not null)
        {
            canonicalEvidenceRefs = canonicalEvidenceRefs
                .Append($"measurement_noise_floor:{evaluation.MeasurementNoisePassport.NoiseFloor.ToString("R", CultureInfo.InvariantCulture)}")
                .ToArray();
        }
        return true;
    }

    private static bool ValidateDpoHeldOutMeasurementNoise(
        DesignConversionDpoHeldOutEvaluation evaluation)
    {
        if (evaluation.MeasurementNoisePassport is null)
        {
            return string.Equals(
                evaluation.MeasurementNoiseStatus,
                MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus,
                StringComparison.Ordinal);
        }

        if (!MeasurementNoisePolicy.TryValidatePassportForMeasurement(
                evaluation.MeasurementNoisePassport,
                DesignConversionDpoPolicyCheckpointBridge
                    .EvaluatedMeasurementNoiseScorerIdentity,
                evaluation.DatasetId,
                evaluation.EvaluatedHeldOutRecordCount,
                evaluation.Accuracy,
                out _))
        {
            return false;
        }

        var expected = MeasurementNoisePolicy.ClassifyDifference(
            evaluation.G3Metrics!.BaselineWinRate -
            DesignConversionG3AcceptancePolicyConfig.Canonical.MinimumBaselineWinRate,
            evaluation.MeasurementNoisePassport);
        return string.Equals(
            evaluation.MeasurementNoiseStatus,
            expected.Status,
            StringComparison.Ordinal);
    }

    private static bool ValidateDpoHeldOutCycleAccuracyLineage(
        DesignConversionDpoHeldOutCycleAccuracyLineage? lineage,
        DesignConversionDpoHeldOutEvaluation evaluation,
        bool legacySchema)
    {
        if (legacySchema)
            return lineage is null;

        if (lineage is null ||
            lineage.CycleOrdinal < 1 ||
            !double.IsFinite(lineage.CurrentMeasuredAccuracy) ||
            lineage.CurrentMeasuredAccuracy is < 0d or > 1d ||
            !lineage.CurrentMeasuredAccuracy.Equals(evaluation.Accuracy))
        {
            return false;
        }

        if (!lineage.HasParentMeasurement)
        {
            return string.Equals(
                       evaluation.ReferenceMode,
                       "provider_fresh_step_zero",
                       StringComparison.Ordinal) &&
                   lineage.CycleOrdinal == 1 &&
                   lineage.ParentCycleOrdinal is null &&
                   lineage.ParentMeasuredAccuracy is null &&
                   lineage.ParentPolicyCheckpointSha256 is null &&
                   lineage.ParentDatasetId is null &&
                   lineage.ParentHeldOutRecordSetSha256 is null &&
                   lineage.ParentHeldOutSourceSetSha256 is null &&
                   lineage.ParentHeldOutEvaluationInputSha256 is null &&
                   !lineage.ComparableHeldOut &&
                   lineage.AccuracyDelta is null;
        }

        if (!string.Equals(
                evaluation.ReferenceMode,
                "immediate_parent",
                StringComparison.Ordinal) ||
            lineage.ParentCycleOrdinal is not { } parentCycleOrdinal ||
            parentCycleOrdinal < 1 ||
            parentCycleOrdinal == int.MaxValue ||
            lineage.CycleOrdinal != parentCycleOrdinal + 1 ||
            lineage.ParentMeasuredAccuracy is not { } parentMeasuredAccuracy ||
            !double.IsFinite(parentMeasuredAccuracy) ||
            parentMeasuredAccuracy is < 0d or > 1d ||
            !IsCanonicalLowercaseSha256Hex(lineage.ParentPolicyCheckpointSha256) ||
            !string.Equals(
                lineage.ParentPolicyCheckpointSha256,
                evaluation.ReferencePolicyCheckpointSha256,
                StringComparison.Ordinal) ||
            !IsSafeDpoEvaluationDatasetId(lineage.ParentDatasetId) ||
            !IsCanonicalLowercaseSha256Hex(lineage.ParentHeldOutRecordSetSha256) ||
            !IsCanonicalLowercaseSha256Hex(lineage.ParentHeldOutSourceSetSha256) ||
            !IsCanonicalLowercaseSha256Hex(lineage.ParentHeldOutEvaluationInputSha256))
        {
            return false;
        }

        var comparableHeldOut =
            string.Equals(lineage.ParentDatasetId, evaluation.DatasetId, StringComparison.Ordinal) &&
            string.Equals(lineage.ParentHeldOutRecordSetSha256, evaluation.HeldOutRecordSetSha256, StringComparison.Ordinal) &&
            string.Equals(lineage.ParentHeldOutSourceSetSha256, evaluation.HeldOutSourceSetSha256, StringComparison.Ordinal) &&
            string.Equals(lineage.ParentHeldOutEvaluationInputSha256, evaluation.HeldOutEvaluationInputSha256, StringComparison.Ordinal);
        if (lineage.ComparableHeldOut != comparableHeldOut)
            return false;

        if (!comparableHeldOut)
            return lineage.AccuracyDelta is null;

        return lineage.AccuracyDelta is { } delta &&
               double.IsFinite(delta) &&
               Math.Abs(delta - (lineage.CurrentMeasuredAccuracy - parentMeasuredAccuracy)) <= 1e-12;
    }

    private static bool IsSafeDpoEvaluationDatasetId(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= 256 &&
           string.Equals(value, value.Trim(), StringComparison.Ordinal) &&
           value.All(character => char.IsAsciiLetterOrDigit(character) ||
               character is '-' or '_' or '.') &&
           !DesignConversionDpoLeakageGuard.ContainsForbiddenLeakage(value);

    private static OperationResult<LoraConfig?> BuildDpoPolicyRuntimeLoraConfig(
        DesignConversionDpoPolicyCheckpointTrainingContext context)
    {
        if (context.Lora is null)
            return OperationResult<LoraConfig?>.Ok(null);

        try
        {
            return OperationResult<LoraConfig?>.Ok(
                LoraApplicator.ValidateAndCanonicalize(new LoraConfig
                {
                    Rank = context.Lora.Rank,
                    Alpha = context.Lora.Alpha,
                    Dropout = context.Lora.Dropout,
                    TargetModules = context.Lora.TargetModules.ToArray()
                }));
        }
        catch (Exception exception) when (exception is ArgumentException
                                          or InvalidOperationException
                                          or OverflowException)
        {
            return OperationResult<LoraConfig?>.Fail(
                "DPO policy checkpoint LoRA metadata is invalid.",
                CheckpointDpoPolicyRefInvalidCode);
        }
    }

    private static ModelConfig BuildDpoPolicyModelConfig(
        DesignConversionDpoPolicyCheckpointTrainingContext context)
        => new()
        {
            VocabSize = context.VocabSize,
            ContextLen = context.ContextLength,
            HiddenDim = context.HiddenDim,
            NumLayers = context.NumLayers,
            NumHeads = context.NumHeads,
            NumKvHeads = context.NumKvHeads,
            FfnHiddenDim = context.FfnHiddenDim,
            RopeTheta = context.RopeTheta,
            RmsNormEps = context.RmsNormEps
        };

    private static OperationResult<string> ValidateImportCheckpointRef(string? checkpointRef)
    {
        if (string.IsNullOrWhiteSpace(checkpointRef))
            return OperationResult<string>.Fail(
                "Export checkpoint reference is required.",
                ImportCheckpointRefRequiredCode);

        if (!Path.IsPathFullyQualified(checkpointRef))
            return OperationResult<string>.Fail(
                "Export checkpoint reference must be an absolute local path.",
                ImportCheckpointRefNotAbsoluteCode);

        try
        {
            return OperationResult<string>.Ok(Path.GetFullPath(checkpointRef));
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Fail(ex.Message, ImportCheckpointRefNotAbsoluteCode);
        }
    }

    private static OperationResult<bool> ValidateExportMatchesLoadedCheckpoint(
        LearningMachineExport export,
        DesignConversionMachineCheckpoint checkpoint,
        string checkpointSha256,
        LearningMachineExport loadedExport)
    {
        var expectedManifestRef = BuildLogicalManifestRef(checkpoint.Version, checkpointSha256);
        if (!string.Equals(export.MachineId, checkpoint.MachineId, StringComparison.Ordinal) ||
            !string.Equals(export.DomainTag, checkpoint.DomainTag, StringComparison.Ordinal) ||
            !string.Equals(export.Version, checkpoint.Version, StringComparison.Ordinal) ||
            !string.Equals(export.Version, loadedExport.Version, StringComparison.Ordinal) ||
            !string.Equals(export.ManifestRef, expectedManifestRef, StringComparison.Ordinal) ||
            !string.Equals(loadedExport.ManifestRef, expectedManifestRef, StringComparison.Ordinal) ||
            !string.Equals(Path.GetFullPath(export.CheckpointRef), loadedExport.CheckpointRef, StringComparison.OrdinalIgnoreCase) ||
            export.AccuracyAtExport != checkpoint.AccuracyAfterLastBatch ||
            export.AccuracyAtExport != loadedExport.AccuracyAtExport)
        {
            return OperationResult<bool>.Fail(
                "Export metadata does not match the loaded checkpoint snapshot.",
                ImportCheckpointMetadataMismatchCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private async Task<OperationResult<bool>> VerifyBoundCheckpointSnapshotAsync(
        BoundCheckpointSnapshot boundSnapshot,
        CancellationToken ct)
    {
        if (!File.Exists(boundSnapshot.FullPath))
            return OperationResult<bool>.Fail(
                $"Bound checkpoint file not found: {boundSnapshot.FullPath}",
                ExportCheckpointFileMissingCode);

        var sha = await ReadFileSha256Async(boundSnapshot.FullPath, ct);
        if (!sha.IsSuccess)
            return OperationResult<bool>.Fail(
                sha.ErrorMessage,
                string.Equals(sha.ErrorCode, "CHECKPOINT_NOT_FOUND", StringComparison.Ordinal)
                    ? ExportCheckpointFileMissingCode
                    : "EXPORT_CHECKPOINT_READ_FAILED");

        if (!string.Equals(sha.Value, boundSnapshot.Sha256, StringComparison.Ordinal))
            return OperationResult<bool>.Fail(
                "Bound checkpoint file content no longer matches the saved snapshot.",
                ExportCheckpointHashMismatchCode);

        return OperationResult<bool>.Ok(true);
    }

    private void BindCheckpointSnapshot(BoundCheckpointSnapshot snapshot)
    {
        lock (_stateGate)
        {
            BindCheckpointSnapshotUnsafe(snapshot);
            _stateGeneration++;
        }
    }

    private void BindCheckpointSnapshotUnsafe(BoundCheckpointSnapshot snapshot)
        => _boundCheckpointSnapshot = snapshot;

    private LearningMachineExport CreateBoundExport(
        BoundCheckpointSnapshot snapshot,
        double accuracy,
        DateTimeOffset exportedAtUtc)
        => new(
            MachineId: MachineId,
            DomainTag: DomainTag,
            Version: snapshot.Version,
            CheckpointRef: snapshot.FullPath,
            ManifestRef: BuildLogicalManifestRef(snapshot.Version, snapshot.Sha256),
            AccuracyAtExport: accuracy,
            ExportedAtUtc: exportedAtUtc);

    private static string BuildLogicalManifestRef(string version, string sha256)
        => $"manifest://{MachineId}/{version}/sha256/{sha256}";

    private static string DecodeCheckpointJsonSnapshot(byte[] checkpointBytes)
    {
        var json = Encoding.UTF8.GetString(checkpointBytes);
        return json.Length > 0 && json[0] == '\uFEFF'
            ? json[1..]
            : json;
    }

    private sealed class CheckpointByteReaderOverrideScope(
        Func<string, CancellationToken, Task<byte[]>>? previous) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            CheckpointByteReaderOverride.Value = previous;
            _disposed = true;
        }
    }

    private sealed class CheckpointStateCaptureOverrideScope(
        Func<CancellationToken, Task>? previous) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            CheckpointStateCaptureOverride.Value = previous;
            _disposed = true;
        }
    }

    private DesignConversionRuntimeUnitCheckpoint BuildRuntimeUnitCheckpoint(
        string unitId,
        string unitRole,
        string domainTag,
        string implementationStatus,
        IReadOnlyList<string> baseEvidenceRefs)
    {
        var learnedWeights = ExportLearnedWeights(unitId);
        var hasLearnedWeights = learnedWeights.Count > 0;
        return new DesignConversionRuntimeUnitCheckpoint(
            UnitId: unitId,
            UnitRole: unitRole,
            DomainTag: domainTag,
            ImplementationStatus: hasLearnedWeights
                ? $"{implementationStatus}_with_local_learned_weight_artifact"
                : implementationStatus,
            EvidenceRefs: baseEvidenceRefs
                .Concat(hasLearnedWeights
                    ? [LearnedWeightsPersistedEvidence, LearnedWeightsSanitizedFieldsEvidence]
                    : [])
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            LearnedWeightsPresent: hasLearnedWeights,
            LearnedWeights: learnedWeights);
    }

    private IReadOnlyList<DesignConversionLearnedWeightCheckpoint> ExportLearnedWeights(string checkpointUnitId)
    {
        if (!_learnedWeightsByUnit.TryGetValue(checkpointUnitId, out var byFeature))
            return [];

        return byFeature.Values
            .Where(accumulator => accumulator.ObservationCount > 0)
            .OrderBy(accumulator => accumulator.FeatureKey, StringComparer.Ordinal)
            .Select(accumulator => accumulator.ToCheckpoint())
            .Where(IsValidLearnedWeight)
            .ToArray();
    }

    private EvaluationStateSnapshot CaptureEvaluationStateUnsafe()
    {
        var weightsByUnit = new Dictionary<string, IReadOnlyList<DesignConversionLearnedWeightCheckpoint>>(
            StringComparer.Ordinal)
        {
            [DesignConversionRuntimeScaffoldCriticUnit.UnitId] = ExportLearnedWeights(
                DesignConversionRuntimeScaffoldCriticUnit.UnitId),
            [DesignConversionRuntimeScaffoldImproverUnit.UnitId] = ExportLearnedWeights(
                DesignConversionRuntimeScaffoldImproverUnit.UnitId)
        };

        return new EvaluationStateSnapshot(
            _hasTrainingScaffoldHistory,
            _stateGeneration,
            weightsByUnit);
    }

    private static OperationResult<Dictionary<string, Dictionary<string, LearnedWeightAccumulator>>> BuildLearnedWeightImport(
        DesignConversionMachineCheckpoint checkpoint)
    {
        var stagedWeights = new Dictionary<string, Dictionary<string, LearnedWeightAccumulator>>(StringComparer.Ordinal);
        var critic = StageLearnedWeights(checkpoint.CriticUnit, stagedWeights);
        if (!critic.IsSuccess)
            return critic.MapError<Dictionary<string, Dictionary<string, LearnedWeightAccumulator>>>();

        var improver = StageLearnedWeights(checkpoint.ImproverUnit, stagedWeights);
        if (!improver.IsSuccess)
            return improver.MapError<Dictionary<string, Dictionary<string, LearnedWeightAccumulator>>>();

        return OperationResult<Dictionary<string, Dictionary<string, LearnedWeightAccumulator>>>.Ok(stagedWeights);
    }

    private static OperationResult<bool> StageLearnedWeights(
        DesignConversionRuntimeUnitCheckpoint unit,
        Dictionary<string, Dictionary<string, LearnedWeightAccumulator>> stagedWeights)
    {
        var weights = unit.LearnedWeights ?? [];
        if (!unit.LearnedWeightsPresent && weights.Count > 0)
        {
            return OperationResult<bool>.Fail(
                $"Checkpoint unit '{unit.UnitId}' contains learned weights but LearnedWeightsPresent is false.",
                "CHECKPOINT_LEARNED_WEIGHTS_INVALID");
        }

        if (unit.LearnedWeightsPresent &&
            (weights.Count == 0 ||
             weights.Any(weight => !IsValidLearnedWeight(weight)) ||
             HasDuplicateLearnedWeightFeatureKeys(weights)))
        {
            return OperationResult<bool>.Fail(
                $"Checkpoint unit '{unit.UnitId}' claims learned weights without a valid persisted artifact.",
                "CHECKPOINT_LEARNED_WEIGHTS_INVALID");
        }

        if (weights.Count == 0)
            return OperationResult<bool>.Ok(true);

        var byFeature = GetUnitAccumulatorMap(stagedWeights, unit.UnitId);
        foreach (var weight in weights)
        {
            byFeature[weight.FeatureKey] = LearnedWeightAccumulator.FromCheckpoint(weight);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool HasDuplicateLearnedWeightFeatureKeys(
        IReadOnlyList<DesignConversionLearnedWeightCheckpoint> weights)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var weight in weights)
        {
            if (weight is null)
                continue;

            if (!seen.Add(weight.FeatureKey))
                return true;
        }

        return false;
    }

    private static bool RuntimeUnitHasRealLearnedWeights(DesignConversionRuntimeUnitCheckpoint? unit)
        => unit?.LearnedWeightsPresent == true &&
           CountRealLearnedWeightArtifacts(unit) > 0;

    private static int CountRealLearnedWeightArtifacts(DesignConversionRuntimeUnitCheckpoint? unit)
        => unit?.LearnedWeights?
            .Count(IsValidLearnedWeight) ?? 0;

    private static int CountRealLearnedWeightObservations(DesignConversionRuntimeUnitCheckpoint? unit)
        => unit?.LearnedWeights?
            .Where(IsValidLearnedWeight)
            .Sum(weight => weight.ObservationCount) ?? 0;

    private static bool IsValidLearnedWeight(DesignConversionLearnedWeightCheckpoint? weight)
        => weight is not null &&
           !string.IsNullOrWhiteSpace(weight.FeatureKey) &&
           IsAllowedLearnedWeightFeatureKey(weight.FeatureKey) &&
           double.IsFinite(weight.Weight) &&
           weight.ObservationCount > 0 &&
           !string.IsNullOrWhiteSpace(weight.LastBatchId) &&
           !string.IsNullOrWhiteSpace(weight.Derivation) &&
           weight.SourceIdentityHashes is not null &&
           weight.SourceIdentityHashes.Count > 0 &&
           weight.SourceIdentityHashes.All(IsSha256Hex) &&
           weight.EvidenceRefs is not null &&
           weight.EvidenceRefs.Contains(LearnedWeightsSanitizedFieldsEvidence, StringComparer.Ordinal);

    private static bool IsAllowedLearnedWeightFeatureKey(string featureKey)
        => string.Equals(featureKey, "internal_vector:mean", StringComparison.Ordinal) ||
           string.Equals(featureKey, "internal_vector:dimension", StringComparison.Ordinal) ||
           string.Equals(featureKey, "source_kind:figma", StringComparison.Ordinal) ||
           string.Equals(featureKey, "source_kind:canva", StringComparison.Ordinal) ||
           string.Equals(featureKey, "source_kind:other", StringComparison.Ordinal) ||
           string.Equals(featureKey, "correction_category:contrast", StringComparison.Ordinal) ||
           string.Equals(featureKey, "correction_category:spacing", StringComparison.Ordinal) ||
           string.Equals(featureKey, "correction_category:overflow", StringComparison.Ordinal) ||
           string.Equals(featureKey, "correction_category:typography", StringComparison.Ordinal) ||
           string.Equals(featureKey, "correction_category:generic-visual-defect", StringComparison.Ordinal);

    private void ApplyLearnedWeightDelta(LearnedWeightDelta delta)
    {
        var byFeature = GetUnitAccumulatorMap(delta.CheckpointUnitId);
        if (!byFeature.TryGetValue(delta.FeatureKey, out var accumulator))
        {
            accumulator = new LearnedWeightAccumulator(
                delta.FeatureKey,
                delta.Derivation,
                delta.BatchId);
            byFeature[delta.FeatureKey] = accumulator;
        }

        accumulator.Apply(delta);
    }

    private Dictionary<string, LearnedWeightAccumulator> GetUnitAccumulatorMap(string checkpointUnitId)
    {
        if (!_learnedWeightsByUnit.TryGetValue(checkpointUnitId, out var byFeature))
        {
            byFeature = new Dictionary<string, LearnedWeightAccumulator>(StringComparer.Ordinal);
            _learnedWeightsByUnit[checkpointUnitId] = byFeature;
        }

        return byFeature;
    }

    private static Dictionary<string, LearnedWeightAccumulator> GetUnitAccumulatorMap(
        Dictionary<string, Dictionary<string, LearnedWeightAccumulator>> weightsByUnit,
        string checkpointUnitId)
    {
        if (!weightsByUnit.TryGetValue(checkpointUnitId, out var byFeature))
        {
            byFeature = new Dictionary<string, LearnedWeightAccumulator>(StringComparer.Ordinal);
            weightsByUnit[checkpointUnitId] = byFeature;
        }

        return byFeature;
    }

    private static PreparedTrainingBatch PrepareTrainingBatch(LearningBatch<DesignConversionDomain> batch)
    {
        var capturedBatch = new LearningBatch<DesignConversionDomain>(
            batch.BatchId,
            batch.DomainTag,
            (batch.TrainingItems ?? []).ToArray(),
            (batch.SourceIds ?? []).ToArray());

        return new PreparedTrainingBatch(
            capturedBatch.BatchId,
            capturedBatch.TrainingItems.Count,
            BuildLearnedWeightBatch(capturedBatch));
    }

    private static LearnedWeightBatch BuildLearnedWeightBatch(LearningBatch<DesignConversionDomain> batch)
    {
        var deltas = new List<LearnedWeightDelta>();
        var margins = new List<double>();
        var batchSourceHashes = (batch.SourceIds ?? [])
            .Where(source => !string.IsNullOrWhiteSpace(source))
            .Select(HashText)
            .ToArray();

        foreach (var item in batch.TrainingItems ?? [])
        {
            var itemDeltas = BuildLearnedWeightDeltas(
                batch.BatchId,
                item,
                batchSourceHashes,
                out var margin);
            deltas.AddRange(itemDeltas);
            if (margin.HasValue)
                margins.Add(margin.Value);
        }

        return new LearnedWeightBatch(
            Deltas: deltas,
            AverageRewardMargin: margins.Count == 0
                ? 0.0
                : Math.Round(margins.Average(), 6));
    }

    private static IReadOnlyList<LearnedWeightDelta> BuildLearnedWeightDeltas(
        string batchId,
        LearningTrainingItem item,
        IReadOnlyList<string> batchSourceHashes,
        out double? rewardMargin)
    {
        rewardMargin = null;
        if (!TryParseJsonObject(item.Prompt, out var prompt))
            return [];

        using (prompt)
        {
            if (!TryParseJsonObject(item.ChosenResponse, out var chosen) ||
                !TryParseJsonObject(item.RejectedResponse, out var rejected))
            {
                return [];
            }

            using (chosen)
            using (rejected)
            {
                var promptRoot = prompt.RootElement;
                var chosenRoot = chosen.RootElement;
                var rejectedRoot = rejected.RootElement;
                var unitId = NormalizeCheckpointUnitId(ReadString(promptRoot, "unit_id"));
                if (string.IsNullOrWhiteSpace(unitId))
                    return [];

                double preferenceMargin;
                if (TryReadDouble(chosenRoot, "visual_score", out var chosenScore) &&
                    TryReadDouble(rejectedRoot, "visual_score", out var rejectedScore))
                {
                    preferenceMargin = chosenScore - rejectedScore;
                }
                else
                {
                    if (string.Equals(
                            item.ChosenResponse,
                            item.RejectedResponse,
                            StringComparison.Ordinal))
                    {
                        return [];
                    }
                    preferenceMargin = 1.0;
                }

                var sourceHashes = new SortedSet<string>(batchSourceHashes, StringComparer.Ordinal);
                if (sourceHashes.Count == 0)
                    return [];

                var deltas = new List<LearnedWeightDelta>();
                var margin = Math.Round(preferenceMargin, 6);
                rewardMargin = margin;
                var vector = ReadDoubleArray(promptRoot, "internal_vector");
                if (vector.Count > 0)
                {
                    AddDelta(deltas, unitId, batchId, "internal_vector:mean", vector.Average(), "internal_vector_mean", sourceHashes);
                    AddDelta(deltas, unitId, batchId, "internal_vector:dimension", vector.Count, "internal_vector_dimension", sourceHashes);
                }

                var sourceKind = SanitizeFeatureToken(ReadString(promptRoot, "source_kind"));
                if (!string.IsNullOrWhiteSpace(sourceKind))
                    AddDelta(deltas, unitId, batchId, $"source_kind:{sourceKind}", 1.0, "source_kind_presence", sourceHashes);

                AddCorrectionCategoryDeltas(deltas, unitId, batchId, chosenRoot, rejectedRoot, sourceHashes);

                return deltas;
            }
        }
    }

    private static void AddCorrectionCategoryDeltas(
        List<LearnedWeightDelta> deltas,
        string unitId,
        string batchId,
        JsonElement chosenRoot,
        JsonElement rejectedRoot,
        IReadOnlyCollection<string> sourceHashes)
    {
        if (!string.Equals(unitId, DesignConversionRuntimeScaffoldImproverUnit.UnitId, StringComparison.Ordinal))
            return;

        var chosenRules = ReadCorrectionRuleIds(chosenRoot);
        var rejectedRules = ReadCorrectionRuleIds(rejectedRoot)
            .Except(chosenRules, StringComparer.Ordinal)
            .ToArray();

        foreach (var ruleId in chosenRules)
        {
            AddDelta(
                deltas,
                unitId,
                batchId,
                $"correction_category:{ruleId}",
                1.0,
                "chosen_correction_category_preference",
                sourceHashes);
        }

        foreach (var ruleId in rejectedRules)
        {
            AddDelta(
                deltas,
                unitId,
                batchId,
                $"correction_category:{ruleId}",
                -1.0,
                "rejected_correction_category_penalty",
                sourceHashes);
        }
    }

    private static IReadOnlyList<string> ReadCorrectionRuleIds(JsonElement element)
    {
        var rules = new SortedSet<string>(StringComparer.Ordinal);
        AddCorrectionRuleIfPresent(rules, ReadString(element, "correction_category"));
        AddCorrectionRuleIfPresent(rules, ReadString(element, "selected_correction_category"));
        AddCorrectionRuleIfPresent(rules, ReadString(element, "correction_rule"));
        AddCorrectionRuleIfPresent(rules, ReadString(element, "selected_correction_rule"));
        AddCorrectionRuleIfPresent(rules, ReadString(element, "defect_category"));
        AddCorrectionRuleArray(rules, element, "correction_categories");
        AddCorrectionRuleArray(rules, element, "defect_categories");

        if (element.TryGetProperty("metadata", out var metadata) &&
            metadata.ValueKind == JsonValueKind.Object)
        {
            AddCorrectionRuleIfPresent(rules, ReadString(metadata, "correction_category"));
            AddCorrectionRuleIfPresent(rules, ReadString(metadata, "selected_correction_category"));
            AddCorrectionRuleArray(rules, metadata, "correction_categories");
            AddCorrectionRuleArray(rules, metadata, "defect_categories");
        }

        return rules.ToArray();
    }

    private static void AddCorrectionRuleArray(SortedSet<string> rules, JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var child in property.EnumerateArray())
        {
            if (child.ValueKind == JsonValueKind.String)
                AddCorrectionRuleIfPresent(rules, child.GetString());
        }
    }

    private static void AddCorrectionRuleIfPresent(SortedSet<string> rules, string? signal)
    {
        var ruleId = DesignConversionRuntimeScaffoldImproverUnit.NormalizeCorrectionRuleId(signal ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(ruleId))
            rules.Add(ruleId);
    }

    private static void AddDelta(
        List<LearnedWeightDelta> deltas,
        string checkpointUnitId,
        string batchId,
        string featureKey,
        double contribution,
        string derivation,
        IReadOnlyCollection<string> sourceHashes)
    {
        if (!double.IsFinite(contribution))
            return;
        if (!IsAllowedLearnedWeightFeatureKey(featureKey))
            return;

        deltas.Add(new LearnedWeightDelta(
            CheckpointUnitId: checkpointUnitId,
            FeatureKey: featureKey,
            Contribution: Math.Round(contribution, 6),
            BatchId: batchId,
            Derivation: derivation,
            SourceIdentityHashes: sourceHashes.ToArray()));
    }

    private static ImmutableArray<GradientBridgeAuthorizedSignal>
        BuildGradientBridgeAuthorizedSignals(LearnedWeightBatch learnedBatch)
        => learnedBatch.Deltas
            .Where(delta =>
                IsCanonicalRuntimeUnitId(delta.CheckpointUnitId) &&
                IsAllowedLearnedWeightFeatureKey(delta.FeatureKey) &&
                double.IsFinite(delta.Contribution) &&
                delta.SourceIdentityHashes is { Count: > 0 } &&
                delta.SourceIdentityHashes.All(IsSha256Hex))
            .GroupBy(delta => (delta.CheckpointUnitId, delta.FeatureKey))
            .Select(group => new GradientBridgeAuthorizedSignal(
                CanonicalUnitId: group.Key.CheckpointUnitId,
                AllowedFeatureKey: group.Key.FeatureKey,
                SignalValue: Math.Round(group.Average(delta => delta.Contribution), 6),
                SourceIdentityHashes: group
                    .SelectMany(delta => delta.SourceIdentityHashes)
                    .Select(hash => hash.ToLowerInvariant())
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToImmutableArray()))
            .OrderBy(signal => signal.CanonicalUnitId, StringComparer.Ordinal)
            .ThenBy(signal => signal.AllowedFeatureKey, StringComparer.Ordinal)
            .ThenBy(signal => signal.SignalValue)
            .ToImmutableArray();

    private static GradientBridgeAuthorizationSnapshot CreateGradientBridgeAuthorizationSnapshot(
        string rawBatchId,
        string inputMachineVersion,
        ImmutableArray<GradientBridgeAuthorizedSignal> authorizedSignals)
        => new(
            rawBatchId,
            HashText(rawBatchId),
            inputMachineVersion,
            CloneAuthorizedSignals(authorizedSignals));

    private static DesignConversionMachineGradientBridgeRequest CreateGradientBridgeRequestSnapshot(
        GradientBridgeAuthorizationSnapshot authorization)
        => new(
            authorization.BatchIdentityHash,
            DomainTag,
            authorization.InputMachineVersion,
            authorization.AuthorizedSignals
                .Select(signal => new DesignConversionMachineGradientBridgeSanitizedSignal(
                    signal.CanonicalUnitId,
                    signal.AllowedFeatureKey,
                    signal.SignalValue))
                .ToImmutableArray());

    private static DesignConversionMachineGradientBridgeResult CreateGradientBridgeResultSnapshot(
        DesignConversionMachineGradientBridgeResult? result)
    {
        if (result is null)
            throw new InvalidOperationException("Gradient bridge result is required.");

        return new DesignConversionMachineGradientBridgeResult(
            result.BatchIdentityHash,
            result.DomainTag,
            result.InputMachineVersion,
            result.AverageRewardMargin,
            result.Updates.IsDefault
                ? default
                : result.Updates
                    .Select(update => update is null
                        ? throw new InvalidOperationException("Gradient bridge update is required.")
                        : new DesignConversionMachineGradientBridgeUpdate(
                            update.CanonicalUnitId,
                            update.AllowedFeatureKey,
                            update.Contribution))
                    .ToImmutableArray());
    }

    private static ImmutableArray<GradientBridgeAuthorizedSignal> CloneAuthorizedSignals(
        ImmutableArray<GradientBridgeAuthorizedSignal> signals)
    {
        if (signals.IsDefaultOrEmpty)
            throw new InvalidOperationException("Gradient bridge signals are required.");

        return signals
            .Select(signal => new GradientBridgeAuthorizedSignal(
                signal.CanonicalUnitId,
                signal.AllowedFeatureKey,
                signal.SignalValue,
                signal.SourceIdentityHashes.IsDefault
                    ? default
                    : CloneImmutableArray(signal.SourceIdentityHashes)))
            .ToImmutableArray();
    }

    private static ImmutableArray<T> CloneImmutableArray<T>(ImmutableArray<T> values)
        => values.IsDefault
            ? default
            : ImmutableArray.CreateRange(values.ToArray());

    private static OperationResult<IReadOnlyList<LearnedWeightDelta>> ValidateGradientBridgeResult(
        DesignConversionMachineGradientBridgeResult? result,
        GradientBridgeAuthorizationSnapshot authorization)
    {
        if (result is null ||
            !IsCanonicalLowercaseSha256Hex(result.BatchIdentityHash) ||
            !string.Equals(result.BatchIdentityHash, authorization.BatchIdentityHash, StringComparison.Ordinal) ||
            !string.Equals(result.DomainTag, DomainTag, StringComparison.Ordinal) ||
            !string.Equals(result.InputMachineVersion, authorization.InputMachineVersion, StringComparison.Ordinal) ||
            !double.IsFinite(result.AverageRewardMargin) ||
            result.Updates.IsDefaultOrEmpty)
        {
            return InvalidGradientBridgeResult();
        }

        var updateKeys = new HashSet<(string UnitId, string FeatureKey)>();
        var staged = new List<LearnedWeightDelta>();
        foreach (var update in result.Updates)
        {
            var matchingSignals = authorization.AuthorizedSignals
                .Where(signal =>
                    string.Equals(signal.CanonicalUnitId, update?.CanonicalUnitId, StringComparison.Ordinal) &&
                    string.Equals(signal.AllowedFeatureKey, update?.AllowedFeatureKey, StringComparison.Ordinal))
                .ToArray();
            var authorizedSourceHashes = matchingSignals
                .SelectMany(signal => signal.SourceIdentityHashes)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();
            if (update is null ||
                !IsCanonicalRuntimeUnitId(update.CanonicalUnitId) ||
                !IsAllowedLearnedWeightFeatureKey(update.AllowedFeatureKey) ||
                !double.IsFinite(update.Contribution) ||
                matchingSignals.Length == 0 ||
                !updateKeys.Add((update.CanonicalUnitId, update.AllowedFeatureKey)))
            {
                return InvalidGradientBridgeResult();
            }

            staged.Add(new LearnedWeightDelta(
                CheckpointUnitId: update.CanonicalUnitId,
                FeatureKey: update.AllowedFeatureKey,
                Contribution: Math.Round(update.Contribution, 6),
                BatchId: authorization.RawBatchId,
                Derivation: GradientBridgeUpdateDerivation,
                SourceIdentityHashes: authorizedSourceHashes));
        }

        return OperationResult<IReadOnlyList<LearnedWeightDelta>>.Ok(
            staged
                .OrderBy(delta => delta.CheckpointUnitId, StringComparer.Ordinal)
                .ThenBy(delta => delta.FeatureKey, StringComparer.Ordinal)
                .ToArray());
    }

    private OperationResult<bool> PreflightLearningDeltas(
        IReadOnlyList<LearnedWeightDelta> deltas,
        bool bridgeOwned)
    {
        var projectedAccumulators = new Dictionary<(string UnitId, string FeatureKey), (double WeightedSum, int ObservationCount)>();
        foreach (var delta in deltas)
        {
            var key = (delta.CheckpointUnitId, delta.FeatureKey);
            if (!projectedAccumulators.TryGetValue(key, out var projected))
            {
                var existing = TryGetLearnedWeightAccumulator(delta.CheckpointUnitId, delta.FeatureKey);
                projected = (existing?.WeightedSum ?? 0.0, existing?.ObservationCount ?? 0);
            }

            if (projected.ObservationCount == int.MaxValue)
                return CreatePreflightFailure(bridgeOwned);

            var nextWeightedSum = projected.WeightedSum + delta.Contribution;
            var nextObservationCount = projected.ObservationCount + 1;
            if (!double.IsFinite(nextWeightedSum) ||
                !double.IsFinite(Math.Round(nextWeightedSum / nextObservationCount, 6)))
            {
                return CreatePreflightFailure(bridgeOwned);
            }

            projectedAccumulators[key] = (nextWeightedSum, nextObservationCount);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> CreatePreflightFailure(bool bridgeOwned)
        => bridgeOwned
            ? InvalidGradientBridgeResult().MapError<bool>()
            : OperationResult<bool>.Fail(
                "Training batch cannot be applied without numeric overflow.",
                "LEARNED_WEIGHT_NUMERIC_OVERFLOW");

    private LearnedWeightAccumulator? TryGetLearnedWeightAccumulator(string checkpointUnitId, string featureKey)
        => _learnedWeightsByUnit.TryGetValue(checkpointUnitId, out var byFeature) &&
           byFeature.TryGetValue(featureKey, out var accumulator)
            ? accumulator
            : null;

    private static OperationResult<IReadOnlyList<LearnedWeightDelta>> InvalidGradientBridgeResult()
        => OperationResult<IReadOnlyList<LearnedWeightDelta>>.Fail(
            "Gradient bridge result is invalid.",
            GradientBridgeInvalidResultCode);

    private static bool IsCanonicalRuntimeUnitId(string? unitId)
        => string.Equals(unitId, DesignConversionRuntimeScaffoldCriticUnit.UnitId, StringComparison.Ordinal) ||
           string.Equals(unitId, DesignConversionRuntimeScaffoldImproverUnit.UnitId, StringComparison.Ordinal);

    private static OperationResult<LearningBatchResult> TrainingCancelled()
        => OperationResult<LearningBatchResult>.Fail("Training cancelled.", "CANCELLED");

    private static OperationResult<LearningBatchResult> GradientBridgeFailed()
        => OperationResult<LearningBatchResult>.Fail(
            "Gradient bridge did not return a usable result.",
            GradientBridgeFailedCode);

    private static OperationResult<LearningBatchResult> GradientBridgeVersionConflict()
        => OperationResult<LearningBatchResult>.Fail(
            "Gradient bridge result does not match current machine version.",
            GradientBridgeVersionConflictCode);

    // TrainBatchAsync does not perform held-out measurement; it carries forward
    // existing accuracy while checkpoint evidence records NOT_MEASURED markers.
    private static double CarryForwardAccuracyWithoutHeldOutMeasurement(
        double accuracyBefore,
        LearnedWeightBatch learnedBatch)
    {
        _ = learnedBatch;
        return accuracyBefore;
    }

    private static HeldOutEvaluationItemScore EvaluateHeldOutItem(
        LearningEvaluationItem item,
        IReadOnlyDictionary<string, IReadOnlyList<DesignConversionLearnedWeightCheckpoint>> weightsByUnit)
    {
        if (item is null || !TryParseJsonObject(item.Prompt, out var prompt))
            return HeldOutEvaluationItemScore.Unscored;

        using (prompt)
        {
            if (!TryParseJsonObject(item.ExpectedResponse, out var expected))
                return HeldOutEvaluationItemScore.Unscored;

            using (expected)
                return EvaluateHeldOutItem(prompt.RootElement, expected.RootElement, weightsByUnit);
        }
    }

    private static HeldOutEvaluationItemScore EvaluateHeldOutItem(
        JsonElement promptRoot,
        JsonElement expectedRoot,
        IReadOnlyDictionary<string, IReadOnlyList<DesignConversionLearnedWeightCheckpoint>> weightsByUnit)
    {
        if (!TryReadExpectedHeldOutAccepted(expectedRoot, out var expectedAccepted))
            return HeldOutEvaluationItemScore.Unscored;

        var unitId = NormalizeCheckpointUnitId(ReadString(promptRoot, "unit_id"));
        if (string.IsNullOrWhiteSpace(unitId))
            return HeldOutEvaluationItemScore.Unscored;

        var features = BuildHeldOutEvaluationFeatures(unitId, promptRoot);
        if (!TryPredictHeldOutFeatureFit(unitId, features, weightsByUnit, out var predictedFit))
            return HeldOutEvaluationItemScore.Unscored;

        var predictedAccepted = predictedFit >= HeldOutFeatureFitAcceptanceThreshold;
        return new HeldOutEvaluationItemScore(MatchesExpectedResponse: predictedAccepted == expectedAccepted);
    }

    private static bool TryPredictHeldOutFeatureFit(
        string unitId,
        IReadOnlyDictionary<string, double> features,
        IReadOnlyDictionary<string, IReadOnlyList<DesignConversionLearnedWeightCheckpoint>> weightsByUnit,
        out double predictedFit)
    {
        predictedFit = 0.0;
        if (features.Count == 0 ||
            !weightsByUnit.TryGetValue(unitId, out var learnedWeights) ||
            learnedWeights.Count == 0)
        {
            return false;
        }

        var contributions = BuildLocalCheckpointFeatureContributions(
            learnedWeights.Where(IsValidLearnedWeight).ToArray(),
            features);
        if (contributions.Count == 0)
            return false;

        predictedFit = Clamp01(Math.Round(
            0.5 + CalculateLocalCheckpointAdjustment(contributions),
            6));
        return true;
    }

    private static bool HasLearnedWeights(
        IReadOnlyDictionary<string, IReadOnlyList<DesignConversionLearnedWeightCheckpoint>> weightsByUnit)
        => weightsByUnit.Values.Any(weights => weights.Any(IsValidLearnedWeight));

    private static IReadOnlyDictionary<string, double> BuildHeldOutEvaluationFeatures(
        string unitId,
        JsonElement promptRoot)
    {
        var features = new Dictionary<string, double>(StringComparer.Ordinal);
        var vector = ReadDoubleArray(promptRoot, "internal_vector");
        if (vector.Count > 0)
        {
            AddHeldOutFeature(features, "internal_vector:mean", vector.Average());
            AddHeldOutFeature(features, "internal_vector:dimension", vector.Count);
        }

        var sourceKind = SanitizeFeatureToken(ReadString(promptRoot, "source_kind"));
        if (!string.IsNullOrWhiteSpace(sourceKind))
            AddHeldOutFeature(features, $"source_kind:{sourceKind}", 1.0);

        if (string.Equals(unitId, DesignConversionRuntimeScaffoldImproverUnit.UnitId, StringComparison.Ordinal))
        {
            foreach (var ruleId in ReadCorrectionRuleIds(promptRoot))
                AddHeldOutFeature(features, $"correction_category:{ruleId}", 1.0);
        }

        return features;
    }

    private static void AddHeldOutFeature(
        Dictionary<string, double> features,
        string featureKey,
        double value)
    {
        if (!double.IsFinite(value) || !IsAllowedLearnedWeightFeatureKey(featureKey))
            return;

        features[featureKey] = Math.Round(value, 6);
    }

    private static bool TryReadExpectedHeldOutAccepted(JsonElement expectedRoot, out bool expectedAccepted)
    {
        if (TryReadBoolean(expectedRoot, "expected_passed", out expectedAccepted) ||
            TryReadBoolean(expectedRoot, "passed", out expectedAccepted) ||
            TryReadBoolean(expectedRoot, "passes_gate", out expectedAccepted) ||
            TryReadBoolean(expectedRoot, "local_checkpoint_fit", out expectedAccepted))
        {
            return true;
        }

        if (TryReadDouble(expectedRoot, "visual_score", out var visualScore) ||
            TryReadDouble(expectedRoot, "expected_visual_score", out visualScore))
        {
            expectedAccepted = visualScore >= HeldOutExpectedVisualScoreThreshold;
            return true;
        }

        expectedAccepted = false;
        return false;
    }

    private static bool TryParseJsonObject(string? value, out JsonDocument document)
    {
        document = null!;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind == JsonValueKind.Object)
                return true;

            document.Dispose();
            document = null!;
            return false;
        }
        catch (JsonException)
        {
            document = null!;
            return false;
        }
    }

    private static string ReadString(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static bool TryReadDouble(JsonElement element, string propertyName, out double value)
    {
        value = 0;
        if (!element.TryGetProperty(propertyName, out var property))
            return false;

        if (property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out value))
            return double.IsFinite(value);

        if (property.ValueKind == JsonValueKind.String &&
            double.TryParse(property.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            return double.IsFinite(value);
        }

        return false;
    }

    private static bool TryReadBoolean(JsonElement element, string propertyName, out bool value)
    {
        value = false;
        if (!element.TryGetProperty(propertyName, out var property))
            return false;

        if (property.ValueKind == JsonValueKind.True)
        {
            value = true;
            return true;
        }

        if (property.ValueKind == JsonValueKind.False)
        {
            value = false;
            return true;
        }

        if (property.ValueKind == JsonValueKind.String &&
            bool.TryParse(property.GetString(), out value))
        {
            return true;
        }

        return false;
    }

    private static IReadOnlyList<double> ReadDoubleArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var values = new List<double>();
        foreach (var child in property.EnumerateArray())
        {
            if (child.ValueKind == JsonValueKind.Number &&
                child.TryGetDouble(out var value) &&
                double.IsFinite(value))
            {
                values.Add(value);
            }
        }

        return values;
    }

    private static string NormalizeCheckpointUnitId(string unitId)
    {
        if (string.Equals(unitId, DesignConversionDpoUnits.CriticUnit, StringComparison.Ordinal) ||
            string.Equals(unitId, DesignConversionRuntimeScaffoldCriticUnit.UnitId, StringComparison.Ordinal))
        {
            return DesignConversionRuntimeScaffoldCriticUnit.UnitId;
        }

        if (string.Equals(unitId, DesignConversionDpoUnits.ImproverUnit, StringComparison.Ordinal) ||
            string.Equals(unitId, DesignConversionRuntimeScaffoldImproverUnit.UnitId, StringComparison.Ordinal))
        {
            return DesignConversionRuntimeScaffoldImproverUnit.UnitId;
        }

        return string.Empty;
    }

    private static string SanitizeFeatureToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var builder = new StringBuilder();
        foreach (var ch in value.Trim().ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(ch))
                builder.Append(ch);
            else if (ch is '_' or '-' or ':')
                builder.Append('_');
        }

        return builder.ToString().Trim('_');
    }

    private static void AddHashIfPresent(SortedSet<string> hashes, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return;

        var normalized = hash.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)
            ? hash["sha256:".Length..]
            : hash;
        if (IsSha256Hex(normalized))
            hashes.Add(normalized.ToLowerInvariant());
    }

    private static bool IsSha256Hex(string? value)
        => value is not null &&
           value.Length == 64 &&
           value.All(ch => ch is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F');

    private static bool IsCanonicalLowercaseSha256Hex(string? value)
        => value is not null &&
           value.Length == 64 &&
           value.All(ch => ch is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static string HashText(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    public Task<OperationResult<ILogicalOrchestrator>> GetOrchestratorAsync(CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<ILogicalOrchestrator>.Fail(
                "GetOrchestrator cancelled.",
                "CANCELLED"));

        var runtimeFlow = CreateRuntimeRoutingFlowSnapshot();
        ILogicalOrchestrator orchestrator = new InlineDesignConversionOrchestrator(
            runtimeFlow.Flow,
            runtimeFlow.LocalCheckpointWeightsConfigured,
            runtimeFlow.DirectDpoPolicyRuntimeScorerConfigured);
        return Task.FromResult(OperationResult<ILogicalOrchestrator>.Ok(orchestrator));
    }

    private static bool IsSupportedVersion(string? version)
        => TryParseSupportedVersion(version, out _, out _, out _);

    private static bool TryIncrementPatchVersion(string? version, out string nextVersion)
    {
        nextVersion = string.Empty;
        if (!TryParseSupportedVersion(version, out var major, out var minor, out var patch) ||
            patch == int.MaxValue)
        {
            return false;
        }

        nextVersion = string.Join(
            ".",
            major.ToString(CultureInfo.InvariantCulture),
            minor.ToString(CultureInfo.InvariantCulture),
            (patch + 1).ToString(CultureInfo.InvariantCulture));
        return true;
    }

    private static bool TryParseSupportedVersion(
        string? version,
        out int major,
        out int minor,
        out int patch)
    {
        major = 0;
        minor = 0;
        patch = 0;

        if (string.IsNullOrWhiteSpace(version))
            return false;

        var parts = version.Split('.');
        if (parts.Length != 3)
            return false;

        if (!TryParseVersionComponent(parts[0], out major) ||
            !TryParseVersionComponent(parts[1], out minor) ||
            !TryParseVersionComponent(parts[2], out patch))
        {
            return false;
        }

        return major is 0 or 1;
    }

    private static bool TryParseVersionComponent(string component, out int value)
        => int.TryParse(
            component,
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out value);

    private sealed record LearnedWeightBatch(
        IReadOnlyList<LearnedWeightDelta> Deltas,
        double AverageRewardMargin);

    private sealed record PreparedTrainingBatch(
        string BatchId,
        int RowsTrained,
        LearnedWeightBatch LearnedWeights);

    private sealed record GradientBridgeAuthorizationSnapshot(
        string RawBatchId,
        string BatchIdentityHash,
        string InputMachineVersion,
        ImmutableArray<GradientBridgeAuthorizedSignal> AuthorizedSignals);

    private sealed record GradientBridgeAuthorizedSignal(
        string CanonicalUnitId,
        string AllowedFeatureKey,
        double SignalValue,
        ImmutableArray<string> SourceIdentityHashes);

    private sealed record GradientBridgeInputState(
        string Version,
        long Generation);

    private sealed record BoundCheckpointExportState(
        BoundCheckpointSnapshot Snapshot,
        string Version,
        double Accuracy,
        long Generation);

    private sealed record CheckpointStateSnapshot(
        DesignConversionMachineCheckpoint Checkpoint,
        long Generation);

    private sealed record RuntimeRoutingFlowSnapshot(
        DesignConversionRoutingFlow Flow,
        bool LocalCheckpointWeightsConfigured,
        bool DirectDpoPolicyRuntimeScorerConfigured);

    private sealed record EvaluationStateSnapshot(
        bool HasTrainingScaffoldHistory,
        long Generation,
        IReadOnlyDictionary<string, IReadOnlyList<DesignConversionLearnedWeightCheckpoint>> WeightsByUnit);

    private sealed record LearnedWeightDelta(
        string CheckpointUnitId,
        string FeatureKey,
        double Contribution,
        string BatchId,
        string Derivation,
        IReadOnlyList<string> SourceIdentityHashes);

    private sealed record LocalCheckpointFeatureContribution(
        string FeatureKey,
        double FeatureValue,
        double Weight,
        int ObservationCount,
        string Derivation,
        double WeightedContribution,
        IReadOnlyList<string> EvidenceRefs);

    private sealed record BoundCheckpointSnapshot(
        string FullPath,
        string Version,
        string Sha256);

    private sealed record DpoPolicyCheckpointPayloadSnapshot(
        byte[] Bytes,
        string Sha256);

    private sealed record DpoPolicyRuntimeEncodingBindingSnapshot(
        DesignConversionDpoRuntimeInferenceEncodingBinding Binding,
        IReadOnlyList<string> EvidenceRefs);

    private sealed record DirectDpoPolicyAcceptanceAuthoritySnapshot(
        bool Active,
        string Reason,
        double CheckpointAccuracyAfterLastBatch,
        double ConfiguredThreshold,
        IReadOnlyList<string> AuthorityEvidenceRefs);

    private sealed record DpoPolicyRuntimeScore(
        double Score,
        double Margin,
        IReadOnlyList<string> EvidenceRefs,
        string Summary,
        bool AcceptanceAuthorityActive,
        double AcceptanceThreshold,
        string AcceptanceAuthorityReason);

    private sealed record HeldOutEvaluationItemScore(bool MatchesExpectedResponse)
    {
        public static HeldOutEvaluationItemScore Unscored { get; } = new(MatchesExpectedResponse: false);
    }

    internal static string BuildDpoPolicyRuntimeCandidateSidePreviewForTests(
        string? html,
        string? css)
        => DesignConversionDpoPolicyCheckpointBridge.SerializePolicySideText(html, css);

    private sealed class DpoPolicyRuntimeScorer
    {
        private readonly LlmModel _model;
        private readonly ModelConfig _config;
        private readonly DesignConversionDpoRuntimeInferenceEncodingBinding?
            _runtimeEncodingBinding;
        private readonly string _policyCheckpointSha256;
        private readonly string _configSha256;
        private readonly string[] _checkpointEvidenceRefs;
        private readonly object _scoreSync = new();
        private readonly HashSet<string> _consumedPairReceipts =
            new(StringComparer.Ordinal);

        public DpoPolicyRuntimeScorer(
            LlmModel model,
            ModelConfig config,
            string policyCheckpointSha256,
            string configSha256,
            IReadOnlyList<string> checkpointEvidenceRefs,
            DesignConversionDpoRuntimeInferenceEncodingBinding? runtimeEncodingBinding = null)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(config);
            _model = model;
            _config = config;
            _runtimeEncodingBinding = runtimeEncodingBinding;
            _policyCheckpointSha256 = policyCheckpointSha256;
            _configSha256 = configSha256;
            _checkpointEvidenceRefs = (checkpointEvidenceRefs ?? [])
                .Where(evidence => !string.IsNullOrWhiteSpace(evidence))
                .ToArray();
        }

        public IReadOnlyList<string> BuildPairContentUnavailableEvidenceRefs()
            => new[]
                {
                    DpoPolicyDirectInferenceConfiguredEvidence,
                    DpoPolicyRuntimePairContentUnavailableEvidence,
                    DpoPolicyDirectInferenceNoRawPayloadEvidence,
                    DpoTrainerPolicyCheckpointFreshLoadedEvidence,
                    DpoTrainerPolicyCheckpointConvertedEvidence,
                    $"dpo_policy_checkpoint_sha256:{_policyCheckpointSha256}",
                    $"dpo_policy_config_sha256:{_configSha256}"
                }
                .Concat(_checkpointEvidenceRefs.Where(IsConfiguredPolicyEvidenceSafeWithoutRuntimePair))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

        private static bool IsConfiguredPolicyEvidenceSafeWithoutRuntimePair(string evidence)
            => !string.Equals(evidence, LearnedInferenceAppliedEvidence, StringComparison.Ordinal) &&
               !string.Equals(evidence, DpoTrainerPolicyCheckpointRuntimeScoringEvidence, StringComparison.Ordinal) &&
               !string.Equals(evidence, DpoPolicyDirectInferenceScoreEvidence, StringComparison.Ordinal) &&
               !string.Equals(evidence, DpoPolicyDirectInferenceAcceptanceAuthorityScoreEvidence, StringComparison.Ordinal) &&
               !string.Equals(evidence, DpoPolicyDirectInferenceAcceptanceAuthorityConfiguredEvidence, StringComparison.Ordinal) &&
               !string.Equals(evidence, DpoPolicyDirectInferenceAcceptanceAuthorityEvidence, StringComparison.Ordinal) &&
               !string.Equals(evidence, DpoPolicyContinuationRuntimeEncodingAppliedEvidence, StringComparison.Ordinal) &&
               !evidence.Contains("direct-dpo-policy-score-source", StringComparison.Ordinal) &&
               !evidence.Contains("direct-dpo-policy-acceptance-authority", StringComparison.Ordinal);

        public OperationResult<DpoPolicyRuntimeScore> Score(
            DesignConversionCriticUnitInput input,
            VisualVerdict baselineVerdict,
            DirectDpoPolicyAcceptanceAuthoritySnapshot acceptanceAuthority,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<DpoPolicyRuntimeScore>.Fail("Direct DPO policy inference cancelled.", "CANCELLED");
            if (input is null || input.Request is null || baselineVerdict is null)
                return OperationResult<DpoPolicyRuntimeScore>.Fail(
                    "Direct DPO policy inference input is malformed.",
                    DpoPolicyDirectInferenceFailedCode);

            try
            {
                var pair = input.DpoPolicyRuntimePair;
                if (_runtimeEncodingBinding is null ||
                    !DesignConversionDpoPolicyRuntimePairFactory
                        .TryValidateForRuntime(
                            pair,
                            input.Request,
                            DateTimeOffset.UtcNow,
                            out _))
                {
                    return PairContentUnavailable();
                }

                var generatedText =
                    BuildDpoPolicyRuntimeCandidateSidePreviewForTests(
                        pair!.Generated.Html,
                        pair.Generated.Css);
                var incumbentText =
                    BuildDpoPolicyRuntimeCandidateSidePreviewForTests(
                        pair.Incumbent.Html,
                        pair.Incumbent.Css);
                if (string.Equals(generatedText, incumbentText,
                        StringComparison.Ordinal))
                {
                    return PairContentUnavailable();
                }

                var batch = DesignConversionDpoPolicyCheckpointBridge
                    .BuildRuntimePolicyPairBatchForInference(
                        pair.BuildPolicyPrompt(),
                        generatedText,
                        incumbentText,
                        _runtimeEncodingBinding,
                        _config);
                if (!batch.IsSuccess || batch.Value is null)
                    return PairContentUnavailable();

                double generatedLogProbability;
                double incumbentLogProbability;
                lock (_scoreSync)
                {
                    if (!DesignConversionDpoPolicyRuntimePairFactory
                            .TryConsumeRuntimeAuthority(pair) ||
                        !_consumedPairReceipts.Add(
                            pair.IndependentEvaluation.ReceiptSha256))
                    {
                        return PairContentUnavailable();
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    generatedLogProbability =
                        DpoTrainer.EvaluateAverageNextTokenLogProbability(
                            _model,
                            batch.Value.ChosenIds,
                            batch.Value.ChosenMask,
                            cancellationToken);
                    incumbentLogProbability =
                        DpoTrainer.EvaluateAverageNextTokenLogProbability(
                            _model,
                            batch.Value.RejectedIds,
                            batch.Value.RejectedMask,
                            cancellationToken);
                }

                var margin = generatedLogProbability - incumbentLogProbability;
                if (!double.IsFinite(margin))
                    return PairContentUnavailable();
                var score = 1.0 / (1.0 + Math.Exp(-Math.Clamp(
                    margin,
                    -60.0,
                    60.0)));
                if (!double.IsFinite(score))
                    return PairContentUnavailable();

                var evidenceRefs = new[]
                    {
                        DpoPolicyDirectInferenceScoreEvidence,
                        DpoTrainerPolicyCheckpointRuntimeScoringEvidence,
                        DpoTrainerPolicyCheckpointFreshLoadedEvidence,
                        DpoTrainerPolicyCheckpointConvertedEvidence,
                        DpoPolicyContinuationRuntimeEncodingAppliedEvidence,
                        DpoPolicyDirectInferenceNoRawPayloadEvidence,
                        $"dpo_policy_checkpoint_sha256:{_policyCheckpointSha256}",
                        $"dpo_policy_config_sha256:{_configSha256}",
                        $"dpo_policy_pair_binding_sha256:{pair.PairBindingSha256}",
                        $"dpo_policy_pair_generated_content_sha256:{pair.Generated.ContentSha256}",
                        $"dpo_policy_pair_incumbent_content_sha256:{pair.Incumbent.ContentSha256}",
                        $"dpo_policy_pair_generated_evaluation_identity:{pair.Generated.EvaluationIdentity}",
                        $"dpo_policy_pair_incumbent_evaluation_identity:{pair.Incumbent.EvaluationIdentity}",
                        $"dpo_policy_pair_independent_receipt_sha256:{pair.IndependentEvaluation.ReceiptSha256}",
                        $"dpo_policy_pair_scorer_request_sha256:{pair.IndependentEvaluation.ScorerRequestSha256}",
                        $"dpo_policy_pair_scorer_output_sha256:{pair.IndependentEvaluation.ScorerOutputSha256}",
                        $"dpo_policy_pair_evaluation_profile_configuration_sha256:{pair.IndependentEvaluation.EvaluationProfileConfigurationSha256}",
                        $"dpo_policy_pair_evaluation_model_sha256:{pair.IndependentEvaluation.EvaluationModelSha256}",
                        $"dpo_policy_pair_artifact_root_binding:{pair.ArtifactRootBinding}",
                        "dpo_policy_pair_score_direction:generated_minus_incumbent",
                        $"dpo_policy_pair_generated_minus_incumbent_margin:{margin.ToString("R", CultureInfo.InvariantCulture)}",
                        $"dpo_policy_pair_sigmoid_score:{score.ToString("R", CultureInfo.InvariantCulture)}"
                    }
                    .Concat(_checkpointEvidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                return OperationResult<DpoPolicyRuntimeScore>.Ok(
                    new DpoPolicyRuntimeScore(
                        Score: Math.Round(score, 6),
                        Margin: Math.Round(margin, 6),
                        EvidenceRefs: evidenceRefs,
                        Summary:
                            "Direct DPO policy ranked the coordinator-owned exact generated/incumbent pair by generated-minus-incumbent average next-token log probability; independent evaluation established pair eligibility but supplied no policy label authority.",
                        AcceptanceAuthorityActive: acceptanceAuthority.Active,
                        AcceptanceThreshold: acceptanceAuthority.ConfiguredThreshold,
                        AcceptanceAuthorityReason: acceptanceAuthority.Reason));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return OperationResult<DpoPolicyRuntimeScore>.Fail("Direct DPO policy inference cancelled.", "CANCELLED");
            }
            catch (Exception)
            {
                return OperationResult<DpoPolicyRuntimeScore>.Fail(
                    "Direct DPO policy inference failed closed before producing trained-authority evidence.",
                    DpoPolicyDirectInferenceFailedCode);
            }
        }

        private static OperationResult<DpoPolicyRuntimeScore>
            PairContentUnavailable()
            => OperationResult<DpoPolicyRuntimeScore>.Fail(
                "Direct DPO policy runtime comparison content is unavailable; baseline verdict is preserved.",
                DpoPolicyRuntimePairContentUnavailableCode);

    }

    private sealed class LearnedWeightAccumulator
    {
        private readonly SortedSet<string> _sourceIdentityHashes = new(StringComparer.Ordinal);
        private readonly SortedSet<string> _evidenceRefs = new(StringComparer.Ordinal)
        {
            LearnedWeightsPersistedEvidence,
            LearnedWeightsSanitizedFieldsEvidence,
            "DESIGN_CONVERSION_LEARNED_WEIGHT_NO_RAW_TREE_HTML_SCREENSHOT"
        };
        private double _weightedSum;

        public LearnedWeightAccumulator(
            string featureKey,
            string derivation,
            string lastBatchId)
        {
            FeatureKey = featureKey;
            Derivation = derivation;
            LastBatchId = lastBatchId;
        }

        public string FeatureKey { get; }

        public string Derivation { get; private set; }

        public string LastBatchId { get; private set; }

        public int ObservationCount { get; private set; }

        public double WeightedSum => _weightedSum;

        public void Apply(LearnedWeightDelta delta)
        {
            _weightedSum += delta.Contribution;
            ObservationCount++;
            LastBatchId = delta.BatchId;
            Derivation = delta.Derivation;
            foreach (var hash in delta.SourceIdentityHashes)
                _sourceIdentityHashes.Add(hash);
        }

        public DesignConversionLearnedWeightCheckpoint ToCheckpoint()
            => new(
                FeatureKey: FeatureKey,
                Weight: Math.Round(_weightedSum / ObservationCount, 6),
                ObservationCount: ObservationCount,
                LastBatchId: LastBatchId,
                Derivation: Derivation,
                SourceIdentityHashes: _sourceIdentityHashes.ToArray(),
                EvidenceRefs: _evidenceRefs.ToArray());

        public static LearnedWeightAccumulator FromCheckpoint(DesignConversionLearnedWeightCheckpoint checkpoint)
        {
            var accumulator = new LearnedWeightAccumulator(
                checkpoint.FeatureKey,
                checkpoint.Derivation,
                checkpoint.LastBatchId)
            {
                _weightedSum = checkpoint.Weight * checkpoint.ObservationCount,
                ObservationCount = checkpoint.ObservationCount
            };

            foreach (var hash in checkpoint.SourceIdentityHashes ?? [])
                accumulator._sourceIdentityHashes.Add(hash);
            foreach (var evidenceRef in checkpoint.EvidenceRefs ?? [])
            {
                if (!string.IsNullOrWhiteSpace(evidenceRef))
                    accumulator._evidenceRefs.Add(evidenceRef);
            }

            return accumulator;
        }
    }

    private sealed class LearnedWeightCriticUnit(
        IReadOnlyList<DesignConversionLearnedWeightCheckpoint> learnedWeights,
        DpoPolicyRuntimeScorer? policyScorer = null,
        DirectDpoPolicyAcceptanceAuthoritySnapshot? policyAcceptanceAuthority = null) : IDesignConversionCriticUnit
    {
        private readonly DesignConversionRuntimeScaffoldCriticUnit _inner = new();
        private readonly DpoPolicyRuntimeScorer? _policyScorer = policyScorer;
        private readonly DirectDpoPolicyAcceptanceAuthoritySnapshot? _policyAcceptanceAuthority = policyAcceptanceAuthority;
        private readonly IReadOnlyList<DesignConversionLearnedWeightCheckpoint> _learnedWeights =
            (learnedWeights ?? [])
            .Where(IsValidLearnedWeight)
            .ToArray();

        public async Task<OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>> ExecuteAsync(
            LogicalUnitRequest<DesignConversionCriticUnitInput> request,
            CancellationToken cancellationToken = default)
        {
            var baseline = await _inner.ExecuteAsync(request, cancellationToken);
            if (!baseline.IsSuccess || baseline.Value is null)
                return baseline;

            var output = baseline.Value.Output;
            if (_policyScorer is not null)
                return ApplyDirectPolicyScore(baseline.Value, request.Input, output, _policyAcceptanceAuthority!, cancellationToken);

            if (_learnedWeights.Count == 0)
                return baseline;

            var contributions = BuildLocalCheckpointFeatureContributions(
                _learnedWeights,
                BuildCriticRuntimeFeatures(request.Input, output.Verdict));
            if (contributions.Count == 0)
                return baseline;

            var adjustment = CalculateLocalCheckpointAdjustment(contributions);
            var adjustedScore = Clamp01(Math.Round(output.Verdict.Score + adjustment, 6));
            var evidenceRefs = BuildLocalCheckpointFeatureAdjustmentEvidenceRefs(
                "critic",
                contributions,
                LearnedInferenceCriticAdjustmentEvidence);
            var adjustedVerdict = output.Verdict with
            {
                VerdictId = $"{output.Verdict.VerdictId}:local-checkpoint:{HashText(string.Join("|", evidenceRefs))[..12]}",
                Score = adjustedScore,
                Passed = output.Verdict.Passed,
                EvidenceRefs = output.Verdict.EvidenceRefs
                    .Concat(evidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()
            };
            var adjustedOutput = output with
            {
                Verdict = adjustedVerdict,
                QaEvidenceRefs = output.QaEvidenceRefs
                    .Concat(evidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()
            };

            return OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>.Ok(baseline.Value with
            {
                Output = adjustedOutput,
                Confidence = adjustedScore,
                PredicateIds = baseline.Value.PredicateIds
                    .Append("design_conversion_critic_local_checkpoint_feature_adjustment")
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                EvidenceIds = baseline.Value.EvidenceIds
                    .Concat(evidenceRefs)
                    .Append(BuildLocalCheckpointAdjustmentSummary("critic", output.Verdict.Score, adjustedScore, adjustment, contributions))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                Warnings = BuildLocalCheckpointAdjustmentWarnings(
                    baseline.Value.Warnings,
                    "DESIGN_CONVERSION_RUNTIME_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_APPLIED_CRITIC_NOT_TRAINED_AUTHORITY")
            });
        }

        private OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>> ApplyDirectPolicyScore(
            LogicalUnitResult<DesignConversionCriticUnitOutput> baseline,
            DesignConversionCriticUnitInput input,
            DesignConversionCriticUnitOutput output,
            DirectDpoPolicyAcceptanceAuthoritySnapshot acceptanceAuthority,
            CancellationToken cancellationToken)
        {
            var score = _policyScorer!.Score(input, output.Verdict, acceptanceAuthority, cancellationToken);
            if (!score.IsSuccess &&
                string.Equals(
                    score.ErrorCode,
                    DpoPolicyRuntimePairContentUnavailableCode,
                    StringComparison.Ordinal))
            {
                var unavailableEvidence = _policyScorer
                    .BuildPairContentUnavailableEvidenceRefs();
                return OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>.Ok(
                    baseline with
                    {
                        EvidenceIds = baseline.EvidenceIds
                            .Concat(unavailableEvidence)
                            .Distinct(StringComparer.Ordinal)
                            .ToArray(),
                        Warnings = baseline.Warnings
                            .Append(DpoPolicyRuntimePairContentUnavailableWarning)
                            .Distinct(StringComparer.Ordinal)
                            .ToArray()
                    });
            }
            if (!score.IsSuccess || score.Value is null)
                return OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>.Fail(
                    score.ErrorMessage,
                    score.ErrorCode);

            var evidenceRefs = output.Verdict.EvidenceRefs
                .Concat(score.Value.EvidenceRefs)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var directPolicyPassed = score.Value.Score >= score.Value.AcceptanceThreshold;
            var activeAuthorityPredicate = score.Value.AcceptanceAuthorityActive
                ? "design_conversion_critic_direct_dpo_policy_acceptance_authority"
                : "design_conversion_critic_baseline_visual_verdict_acceptance_authority";
            var authorityWarning = score.Value.AcceptanceAuthorityActive
                ? "DESIGN_CONVERSION_DIRECT_DPO_POLICY_SCORE_ACCEPTANCE_AUTHORITY_ACTIVE"
                : "DESIGN_CONVERSION_DIRECT_DPO_POLICY_SCORE_BASELINE_ACCEPTANCE_AUTHORITY_G3_PENDING";
            var adjustedVerdict = output.Verdict with
            {
                VerdictId = $"{output.Verdict.VerdictId}:direct-dpo-policy:{HashText(string.Join("|", score.Value.EvidenceRefs))[..12]}",
                Score = score.Value.Score,
                Passed = score.Value.AcceptanceAuthorityActive
                    ? directPolicyPassed
                    : output.Verdict.Passed,
                EvidenceRefs = evidenceRefs
            };
            var adjustedOutput = output with
            {
                Verdict = adjustedVerdict,
                QaEvidenceRefs = output.QaEvidenceRefs
                    .Concat(score.Value.EvidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()
            };

            return OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>.Ok(baseline with
            {
                Output = adjustedOutput,
                Confidence = score.Value.Score,
                PredicateIds = baseline.PredicateIds
                    .Append("design_conversion_critic_direct_dpo_policy_inference_score")
                    .Append(activeAuthorityPredicate)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                EvidenceIds = baseline.EvidenceIds
                    .Concat(score.Value.EvidenceRefs)
                    .Append(score.Value.Summary)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                Warnings = baseline.Warnings
                    .Append(authorityWarning)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()
            });
        }
    }

    private sealed class LearnedWeightImproverUnit(
        IReadOnlyList<DesignConversionLearnedWeightCheckpoint> learnedWeights) : IDesignConversionImproverUnit
    {
        private readonly DesignConversionRuntimeScaffoldImproverUnit _inner = new();
        private readonly IReadOnlyList<DesignConversionLearnedWeightCheckpoint> _learnedWeights =
            (learnedWeights ?? [])
            .Where(IsValidLearnedWeight)
            .ToArray();

        public async Task<OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>> ExecuteAsync(
            LogicalUnitRequest<DesignConversionImproverUnitInput> request,
            CancellationToken cancellationToken = default)
        {
            if (request is null || request.Input is null)
                return await _inner.ExecuteAsync(request!, cancellationToken);

            var contributions = BuildLocalCheckpointFeatureContributions(
                _learnedWeights,
                BuildImproverRuntimeFeatures(request.Input));
            var correctionPreferences = BuildLearnedCorrectionPreferences(contributions);
            var baseline = correctionPreferences.Count == 0
                ? await _inner.ExecuteAsync(request, cancellationToken)
                : await _inner.ExecuteWithLearnedCorrectionPreferencesAsync(request, correctionPreferences, cancellationToken);
            if (!baseline.IsSuccess || _learnedWeights.Count == 0 || baseline.Value is null)
                return baseline;

            var output = baseline.Value.Output;
            if (contributions.Count == 0)
                return baseline;

            var adjustment = CalculateLocalCheckpointAdjustment(contributions);
            var evidenceRefs = BuildLocalCheckpointFeatureAdjustmentEvidenceRefs(
                "improver",
                contributions,
                LearnedInferenceImproverEvidence);
            var summary = BuildLocalCheckpointAdjustmentSummary(
                "improver",
                request.Input.CriticVerdict.Score,
                Clamp01(request.Input.CriticVerdict.Score + adjustment),
                adjustment,
                contributions);
            var candidate = output.Candidate with
            {
                ChangeReasons = output.Candidate.ChangeReasons
                    .Append("Local checkpoint feature weights contributed sanitized runtime evidence for improver selection; this is not trained model authority.")
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                EvidenceRefs = output.Candidate.EvidenceRefs
                    .Concat(evidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                Metadata = output.Candidate.Metadata
                    .Concat(new Dictionary<string, string>
                    {
                        ["LearnedInferenceApplied"] = bool.TrueString,
                        ["LearnedFeatureContributionCount"] = contributions.Count.ToString(CultureInfo.InvariantCulture),
                        ["LocalCheckpointFeatureAdjustmentApplied"] = bool.TrueString,
                        ["TrainedModelAuthority"] = bool.FalseString,
                        ["LocalCheckpointFeatureContributionCount"] = contributions.Count.ToString(CultureInfo.InvariantCulture),
                        ["LocalCheckpointFeatureAdjustmentSummary"] = summary,
                        ["LocalCheckpointFeatureAdjustmentEvidenceRefs"] = string.Join("|", evidenceRefs),
                        ["local_checkpoint_feature_adjustment_applied"] = bool.TrueString,
                        [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] = DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue,
                        [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] = DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue,
                        ["local_checkpoint_weights_source"] = "local_checkpoint_sanitized_features_not_trained_model"
                    })
                    .GroupBy(pair => pair.Key, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal)
            };
            var improvedOutput = output.Output with
            {
                CandidateId = candidate.CandidateId,
                EvidenceRefs = output.Output.EvidenceRefs
                    .Concat(evidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()
            };
            var adjustedUnitOutput = output with
            {
                Candidate = candidate,
                Output = improvedOutput,
                QaEvidenceRefs = output.QaEvidenceRefs
                    .Concat(evidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()
            };

            return OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>.Ok(baseline.Value with
            {
                Output = adjustedUnitOutput,
                Confidence = Clamp01(Math.Round(baseline.Value.Confidence + adjustment, 6)),
                PredicateIds = baseline.Value.PredicateIds
                    .Append("design_conversion_improver_local_checkpoint_feature_adjustment")
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                EvidenceIds = baseline.Value.EvidenceIds
                    .Concat(evidenceRefs)
                    .Append(summary)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                Warnings = BuildLocalCheckpointAdjustmentWarnings(
                    baseline.Value.Warnings,
                    "DESIGN_CONVERSION_RUNTIME_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_APPLIED_IMPROVER_NOT_TRAINED_AUTHORITY")
            });
        }
    }

    private static IReadOnlyDictionary<string, double> BuildCriticRuntimeFeatures(
        DesignConversionCriticUnitInput input,
        VisualVerdict baselineVerdict)
    {
        _ = baselineVerdict;
        var features = new Dictionary<string, double>(StringComparer.Ordinal);

        var sourceKind = SanitizeFeatureToken(input.Request.SourceKind.ToString());
        if (!string.IsNullOrWhiteSpace(sourceKind))
            features[$"source_kind:{sourceKind}"] = 1.0;

        return features;
    }

    private static IReadOnlyDictionary<string, double> BuildImproverRuntimeFeatures(
        DesignConversionImproverUnitInput input)
    {
        var features = new Dictionary<string, double>(StringComparer.Ordinal);

        var sourceKind = SanitizeFeatureToken(input.Request.SourceKind.ToString());
        if (!string.IsNullOrWhiteSpace(sourceKind))
            features[$"source_kind:{sourceKind}"] = 1.0;

        foreach (var ruleId in DesignConversionRuntimeScaffoldImproverUnit.FindLocalCorrectionRuleIds(input.CriticVerdict.Defects))
            features[$"correction_category:{ruleId}"] = 1.0;

        return features;
    }

    private static IReadOnlyList<LocalCheckpointFeatureContribution> BuildLocalCheckpointFeatureContributions(
        IReadOnlyList<DesignConversionLearnedWeightCheckpoint> learnedWeights,
        IReadOnlyDictionary<string, double> runtimeFeatures)
        => learnedWeights
            .Where(IsValidLearnedWeight)
            .Where(weight => runtimeFeatures.TryGetValue(weight.FeatureKey, out var value) && double.IsFinite(value))
            .Select(weight =>
            {
                var featureValue = runtimeFeatures[weight.FeatureKey];
                return new LocalCheckpointFeatureContribution(
                    FeatureKey: weight.FeatureKey,
                    FeatureValue: Math.Round(featureValue, 6),
                    Weight: Math.Round(weight.Weight, 6),
                    ObservationCount: weight.ObservationCount,
                    Derivation: weight.Derivation,
                    WeightedContribution: Math.Round(featureValue * weight.Weight, 6),
                    EvidenceRefs: weight.EvidenceRefs ?? []);
            })
            .ToArray();

    private static IReadOnlyList<DesignConversionLearnedCorrectionPreference> BuildLearnedCorrectionPreferences(
        IReadOnlyList<LocalCheckpointFeatureContribution> contributions)
    {
        const string prefix = "correction_category:";
        return contributions
            .Where(contribution => contribution.FeatureKey.StartsWith(prefix, StringComparison.Ordinal))
            .Select(contribution => new DesignConversionLearnedCorrectionPreference(
                RuleId: contribution.FeatureKey[prefix.Length..],
                Weight: contribution.WeightedContribution,
                ObservationCount: contribution.ObservationCount,
                FeatureKey: contribution.FeatureKey,
                Derivation: contribution.Derivation))
            .ToArray();
    }

    private static double CalculateLocalCheckpointAdjustment(IReadOnlyList<LocalCheckpointFeatureContribution> contributions)
    {
        if (contributions.Count == 0)
            return 0.0;

        var rawAdjustment = contributions.Sum(contribution => contribution.WeightedContribution);
        var scaledAdjustment = Math.Tanh(rawAdjustment / LocalCheckpointAdjustmentScale) * LocalCheckpointAdjustmentClamp;
        return Math.Round(Math.Clamp(scaledAdjustment, -LocalCheckpointAdjustmentClamp, LocalCheckpointAdjustmentClamp), 6);
    }

    private static string[] BuildLocalCheckpointFeatureAdjustmentEvidenceRefs(
        string unit,
        IReadOnlyList<LocalCheckpointFeatureContribution> contributions,
        string unitEvidence)
    {
        var evidenceRefs = new[]
            {
                LearnedInferenceAppliedEvidence,
                LearnedInferenceNoRawPayloadEvidence,
                LocalCheckpointFeatureAdjustmentEvidence,
                unitEvidence,
                $"learned-inference:{unit}"
            }
            .Concat(contributions.SelectMany(contribution => contribution.EvidenceRefs ?? []))
            .Concat(contributions.Select(contribution =>
                $"local-checkpoint-feature-adjustment:{unit}:feature={SanitizeFeatureToken(contribution.FeatureKey)}:obs={contribution.ObservationCount}"));

        if (HasDpoTrainerPolicyCheckpointEvidence(contributions))
            evidenceRefs = evidenceRefs.Append(DpoTrainerPolicyCheckpointRuntimeScoringEvidence);

        return evidenceRefs
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string BuildLocalCheckpointAdjustmentSummary(
        string unit,
        double beforeScore,
        double afterScore,
        double adjustment,
        IReadOnlyList<LocalCheckpointFeatureContribution> contributions)
    {
        var authority = HasDpoTrainerPolicyCheckpointEvidence(contributions)
            ? "dpo_trainer_policy_checkpoint_converted_not_product_ready"
            : "not_trained_model";
        return string.Join(
            ";",
            "LocalCheckpointFeatureAdjustmentSummary",
            $"unit={unit}",
            $"features={contributions.Count.ToString(CultureInfo.InvariantCulture)}",
            $"adjustment={adjustment.ToString("0.######", CultureInfo.InvariantCulture)}",
            $"score_before={beforeScore.ToString("0.######", CultureInfo.InvariantCulture)}",
            $"score_after={afterScore.ToString("0.######", CultureInfo.InvariantCulture)}",
            "acceptance_authority=baseline_visual_verdict",
            $"adjustment_scale=tanh_bounded_{LocalCheckpointAdjustmentClamp.ToString("0.######", CultureInfo.InvariantCulture)}",
            $"authority={authority}");
    }

    private static bool HasDpoTrainerPolicyCheckpointEvidence(IReadOnlyList<LocalCheckpointFeatureContribution> contributions)
        => contributions.Any(contribution =>
            contribution.EvidenceRefs.Contains(DpoTrainerPolicyCheckpointConvertedEvidence, StringComparer.Ordinal) ||
            contribution.EvidenceRefs.Contains(DpoTrainerPolicyCheckpointFreshLoadedEvidence, StringComparer.Ordinal));

    private static string[] BuildLocalCheckpointAdjustmentWarnings(
        IReadOnlyList<string> warnings,
        string localAdjustmentWarning)
        => (warnings ?? [])
            .Append(RuntimeScaffoldNotTrainedModelWarning)
            .Append(localAdjustmentWarning)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static double Clamp01(double value)
        => Math.Clamp(value, 0.0, 1.0);

    private sealed class InlineDesignConversionOrchestrator(
        DesignConversionRoutingFlow flow,
        bool localCheckpointWeightsConfigured,
        bool directDpoPolicyRuntimeScorerConfigured) : ILogicalOrchestrator
    {
        public async Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
            LogicalOrchestratorRequest request,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<LogicalOrchestratorResult>.Fail(
                    "DesignConversion orchestrator cancelled.",
                    "CANCELLED");

            if (request is null)
                return OperationResult<LogicalOrchestratorResult>.Fail(
                    "Orchestrator request is required.",
                    "REQUEST_REQUIRED");

            var input = ReadInput(request.Metadata);
            if (input is null)
                return OperationResult<LogicalOrchestratorResult>.Fail(
                    "DesignConversionRuntimeFlowInput must be supplied in orchestrator metadata under 'runtime_flow_input' or 'design_conversion_input'.",
                    "DESIGN_CONVERSION_INPUT_REQUIRED");

            var flags = request.Flags ?? new LogicalExecutionFlags();
            var flowResult = await flow.ExecuteAsync(new LogicalFlowRequest<DesignConversionRuntimeFlowInput>(
                RunId: request.RunId,
                FlowId: DesignConversionRoutingFlow.FlowId,
                ScenarioId: DesignConversionRoutingFlow.ScenarioId,
                Input: input,
                Flags: flags,
                Metadata: request.Metadata), cancellationToken);

            if (!flowResult.IsSuccess)
                return OperationResult<LogicalOrchestratorResult>.Fail(flowResult.ErrorMessage, flowResult.ErrorCode);

            var normalizedFlowResult = NormalizeLocalCheckpointAdjustedFlowResult(
                flowResult.Value!,
                localCheckpointWeightsConfigured,
                directDpoPolicyRuntimeScorerConfigured);
            return OperationResult<LogicalOrchestratorResult>.Ok(new LogicalOrchestratorResult(
                ScenarioId: request.ScenarioId,
                SelectedFlowId: DesignConversionRoutingFlow.FlowId,
                OverallVerdict: normalizedFlowResult.OverallVerdict,
                FinalHumanResult: normalizedFlowResult.FinalHumanResult,
                AppliedFlags: flags,
                FlowResult: normalizedFlowResult)
            {
                Warnings = BuildOrchestratorWarnings(
                    normalizedFlowResult,
                    localCheckpointWeightsConfigured,
                    directDpoPolicyRuntimeScorerConfigured)
            });
        }

        private static LogicalFlowResult NormalizeLocalCheckpointAdjustedFlowResult(
            LogicalFlowResult result,
            bool localCheckpointWeightsConfigured,
            bool directDpoPolicyRuntimeScorerConfigured)
        {
            var localCheckpointAdjustmentApplied = result.EvidenceIds.Contains(LocalCheckpointFeatureAdjustmentEvidence, StringComparer.Ordinal);
            var directDpoPolicyInferenceApplied = HasDirectDpoPolicyInferenceApplied(result.EvidenceIds);
            var directDpoPolicyAcceptanceAuthority = HasDirectDpoPolicyAcceptanceAuthority(result.EvidenceIds);
            var directDpoPolicyRuntimePairContentUnavailable = result.EvidenceIds.Contains(
                DpoPolicyRuntimePairContentUnavailableEvidence,
                StringComparer.Ordinal);
            var modelAuthority = DesignConversionModelAuthorityBoundary.FromTrustedDirectDpoAcceptanceAuthority(
                directDpoPolicyAcceptanceAuthority);
            if (directDpoPolicyRuntimePairContentUnavailable)
            {
                var unavailableMetrics = result.Metrics
                    .Concat(new Dictionary<string, string>
                    {
                        ["local_checkpoint_weights_configured"] = localCheckpointWeightsConfigured.ToString(),
                        ["direct_dpo_policy_runtime_scorer_configured"] = directDpoPolicyRuntimeScorerConfigured.ToString(),
                        ["direct_dpo_policy_runtime_pair_content_available"] = bool.FalseString,
                        ["direct_dpo_policy_inference_applied"] = bool.FalseString,
                        ["direct_dpo_policy_score_authority"] = bool.FalseString,
                        ["direct_dpo_policy_acceptance_authority"] = bool.FalseString,
                        ["baseline_visual_verdict_acceptance_authority"] = bool.TrueString,
                        [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] = DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue,
                        [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] = DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue
                    })
                    .GroupBy(pair => pair.Key, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);
                DesignConversionModelAuthorityBoundary.ApplyTrustedSnapshot(
                    unavailableMetrics,
                    DesignConversionModelAuthorityBoundary.FromTrustedDirectDpoAcceptanceAuthority(false));
                return result with
                {
                    Metrics = unavailableMetrics,
                    Warnings = result.Warnings
                        .Append(DpoPolicyRuntimePairContentUnavailableWarning)
                        .Distinct(StringComparer.Ordinal)
                        .ToArray()
                };
            }

            if (!localCheckpointAdjustmentApplied && !directDpoPolicyInferenceApplied)
                return result;

            var contributionCount = result.EvidenceIds
                .Count(id => id.StartsWith("local-checkpoint-feature-adjustment:", StringComparison.Ordinal));
            var dpoTrainerPolicyCheckpointConverted = result.EvidenceIds.Contains(DpoTrainerPolicyCheckpointConvertedEvidence, StringComparer.Ordinal);
            var dpoTrainerPolicyCheckpointFreshLoaded = result.EvidenceIds.Contains(DpoTrainerPolicyCheckpointFreshLoadedEvidence, StringComparer.Ordinal);
            var localCheckpointWeightsSource = directDpoPolicyInferenceApplied
                ? "11.DPO/DpoTrainer:direct_policy_checkpoint_forward_logprob_margin"
                : dpoTrainerPolicyCheckpointConverted
                ? "11.DPO/DpoTrainer:dpo_policy_checkpoint_converted"
                : "local_checkpoint_sanitized_features_not_trained_model";
            var metrics = result.Metrics
                .Concat(new Dictionary<string, string>
                {
                    ["LocalCheckpointFeatureAdjustmentApplied"] = localCheckpointAdjustmentApplied.ToString(),
                    ["LocalCheckpointFeatureContributionCount"] = contributionCount.ToString(CultureInfo.InvariantCulture),
                    ["local_checkpoint_weights_configured"] = localCheckpointWeightsConfigured.ToString(),
                    ["local_checkpoint_weights_source"] = localCheckpointWeightsSource,
                    ["local_checkpoint_feature_adjustment_applied"] = localCheckpointAdjustmentApplied.ToString(),
                    ["dpo_trainer_policy_checkpoint_converted"] = dpoTrainerPolicyCheckpointConverted.ToString(),
                    ["dpo_trainer_policy_checkpoint_fresh_loaded"] = dpoTrainerPolicyCheckpointFreshLoaded.ToString(),
                    ["direct_dpo_policy_runtime_scorer_configured"] = directDpoPolicyRuntimeScorerConfigured.ToString(),
                    ["direct_dpo_policy_runtime_pair_content_available"] = directDpoPolicyInferenceApplied.ToString(),
                    ["direct_dpo_policy_inference_applied"] = directDpoPolicyInferenceApplied.ToString(),
                    ["direct_dpo_policy_score_authority"] = directDpoPolicyInferenceApplied.ToString(),
                    ["direct_dpo_policy_acceptance_authority"] = directDpoPolicyAcceptanceAuthority.ToString(),
                    ["baseline_visual_verdict_acceptance_authority"] = directDpoPolicyInferenceApplied
                        ? (!directDpoPolicyAcceptanceAuthority).ToString()
                        : bool.FalseString,
                    [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] = DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue,
                    [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] = DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue
                })
                .GroupBy(pair => pair.Key, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);
            DesignConversionModelAuthorityBoundary.ApplyTrustedSnapshot(metrics, modelAuthority);
            var warnings = directDpoPolicyInferenceApplied
                ? result.Warnings
                    .Append(directDpoPolicyAcceptanceAuthority
                        ? "DESIGN_CONVERSION_RUNTIME_DIRECT_DPO_POLICY_SCORE_ACCEPTANCE_AUTHORITY_ACTIVE"
                        : "DESIGN_CONVERSION_RUNTIME_DIRECT_DPO_POLICY_SCORE_BASELINE_ACCEPTANCE_AUTHORITY_G3_PENDING")
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()
                : result.Warnings
                    .Append("DESIGN_CONVERSION_RUNTIME_SCAFFOLD_NOT_LEARNED_OUTPUT")
                    .Append(RuntimeScaffoldNotTrainedModelWarning)
                    .Append("DESIGN_CONVERSION_RUNTIME_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_APPLIED_NOT_TRAINED_AUTHORITY")
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
            var summary = result.Summary with
            {
                PrimaryMetricName = directDpoPolicyInferenceApplied
                    ? "critic_direct_dpo_policy_logprob_score"
                    : "critic_local_checkpoint_feature_adjusted_score",
                ScalingSignalNote = directDpoPolicyInferenceApplied
                    ? directDpoPolicyAcceptanceAuthority
                        ? "Direct DPO policy checkpoint inference drove critic score via LlmModel.Forward/log-prob margin and owns pass/acceptance after canonical DEC-03 G3 admission."
                        : "Direct DPO policy checkpoint inference drove critic score via LlmModel.Forward/log-prob margin; baseline visual verdict still owns pass/acceptance until G3."
                    : dpoTrainerPolicyCheckpointConverted
                    ? "DpoTrainer policy checkpoint was fresh-loaded then converted into sanitized runtime feature weights; this is not product-ready trained-model authority."
                    : "Local checkpoint feature weights adjusted critic confidence; this is not trained model authority.",
                Notes = directDpoPolicyInferenceApplied
                    ? directDpoPolicyAcceptanceAuthority
                        ? "DesignConversion flow used a validated sibling full-policy checkpoint with current-cycle evaluated held-out runtime evidence; direct policy score controls per-request acceptance and no raw HTML/CSS/tree/path is exposed in runtime evidence."
                        : "DesignConversion flow used a validated sibling full-policy checkpoint for direct DPO policy scoring; ProductReady/G3 acceptance remains false and no raw HTML/CSS/tree/path is exposed in runtime evidence."
                    : dpoTrainerPolicyCheckpointConverted
                    ? "DesignConversion flow used a fresh-loaded 11.DPO/DpoTrainer policy checkpoint converted into sanitized runtime feature-weight wrappers; ProductReady/model training remains false; evidence contains sanitized feature keys and counts only."
                    : "DesignConversion flow used local checkpoint feature-weight wrappers over Critic and Improver scaffold contracts; ProductReady/model training remains false; evidence contains sanitized feature keys and counts only."
            };

            return result with
            {
                Summary = summary,
                Metrics = metrics,
                EvidenceIds = result.EvidenceIds
                    .Concat(directDpoPolicyInferenceApplied
                        ? directDpoPolicyAcceptanceAuthority
                            ?
                            [
                                DpoPolicyDirectInferenceAcceptanceAuthorityScoreEvidence,
                                DpoPolicyDirectInferenceAcceptanceAuthorityConfiguredEvidence,
                                DpoPolicyDirectInferenceAcceptanceAuthorityEvidence
                            ]
                            :
                            [
                                DpoPolicyDirectInferenceScoreEvidence,
                                DpoPolicyDirectInferenceConfiguredEvidence
                            ]
                        : new[] { LocalCheckpointFeatureAdjustmentEvidence })
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                Warnings = warnings
            };
        }

        private static string[] BuildOrchestratorWarnings(
            LogicalFlowResult result,
            bool localCheckpointWeightsConfigured,
            bool directDpoPolicyRuntimeScorerConfigured)
        {
            var directDpoPolicyInferenceApplied = HasDirectDpoPolicyInferenceApplied(result.EvidenceIds);
            if (result.EvidenceIds.Contains(
                    DpoPolicyRuntimePairContentUnavailableEvidence,
                    StringComparer.Ordinal))
            {
                return
                [
                    DpoPolicyRuntimePairContentUnavailableWarning,
                    DesignConversionRuntimeScaffoldCriticUnit.InputEvidenceOnlyWarning
                ];
            }

            if (directDpoPolicyInferenceApplied)
            {
                var directDpoPolicyAcceptanceAuthority = HasDirectDpoPolicyAcceptanceAuthority(result.EvidenceIds);
                return
                [
                    directDpoPolicyAcceptanceAuthority
                        ? "DESIGN_CONVERSION_MACHINE_RUNTIME_DIRECT_DPO_POLICY_SCORER_USED_ACCEPTANCE_AUTHORITY_ACTIVE"
                        : "DESIGN_CONVERSION_MACHINE_RUNTIME_DIRECT_DPO_POLICY_SCORER_USED_BASELINE_ACCEPTANCE_G3_PENDING",
                    DesignConversionRuntimeScaffoldCriticUnit.InputEvidenceOnlyWarning
                ];
            }

            var localCheckpointAdjustmentApplied = result.EvidenceIds.Contains(LearnedInferenceAppliedEvidence, StringComparer.Ordinal);
            if (localCheckpointAdjustmentApplied)
            {
                return
                [
                    "DESIGN_CONVERSION_MACHINE_RUNTIME_SCAFFOLD_NOT_LEARNED",
                    RuntimeScaffoldNotTrainedModelWarning,
                    "DESIGN_CONVERSION_MACHINE_RUNTIME_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_APPLIED_NOT_TRAINED_AUTHORITY",
                    DesignConversionRuntimeScaffoldCriticUnit.InputEvidenceOnlyWarning
                ];
            }

            return localCheckpointWeightsConfigured || directDpoPolicyRuntimeScorerConfigured
                ?
                [
                    directDpoPolicyRuntimeScorerConfigured
                        ? "DESIGN_CONVERSION_MACHINE_RUNTIME_DIRECT_DPO_POLICY_SCORER_CONFIGURED_NO_RUNTIME_SCORE"
                        : "DESIGN_CONVERSION_MACHINE_RUNTIME_LOCAL_CHECKPOINT_WEIGHTS_CONFIGURED_NO_MATCHING_RUNTIME_FEATURE_NOT_TRAINED_MODEL",
                    DesignConversionRuntimeScaffoldCriticUnit.InputEvidenceOnlyWarning
                ]
                :
                [
                    "DESIGN_CONVERSION_MACHINE_RUNTIME_SCAFFOLD_NOT_LEARNED",
                    DesignConversionRuntimeScaffoldCriticUnit.InputEvidenceOnlyWarning
                ];
        }

        private static bool HasDirectDpoPolicyInferenceApplied(IReadOnlyList<string> evidenceIds)
            => evidenceIds.Contains(DpoPolicyDirectInferenceScoreEvidence, StringComparer.Ordinal) ||
               evidenceIds.Contains(DpoPolicyDirectInferenceAcceptanceAuthorityScoreEvidence, StringComparer.Ordinal);

        private static bool HasDirectDpoPolicyAcceptanceAuthority(IReadOnlyList<string> evidenceIds)
            => evidenceIds.Contains(DpoPolicyDirectInferenceAcceptanceAuthorityEvidence, StringComparer.Ordinal) ||
               evidenceIds.Contains(DpoPolicyDirectInferenceAcceptanceAuthorityScoreEvidence, StringComparer.Ordinal);

        private static DesignConversionRuntimeFlowInput? ReadInput(IReadOnlyDictionary<string, object> metadata)
        {
            if (metadata.TryGetValue("runtime_flow_input", out var runtimeInput) &&
                runtimeInput is DesignConversionRuntimeFlowInput typedRuntimeInput)
                return typedRuntimeInput;

            if (metadata.TryGetValue("design_conversion_input", out var designInput) &&
                designInput is DesignConversionRuntimeFlowInput typedDesignInput)
                return typedDesignInput;

            return null;
        }
    }
}
