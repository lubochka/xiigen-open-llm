// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

/// <summary>
/// Composes target admission with numeric measurement while preserving each boundary's errors.
/// </summary>
public static class DynamicGraphMeasurementCoordinator
{
    private const string InvalidUntrainedHonestyTargetLevelCode =
        "DYNAMIC_GRAPH_UNTRAINED_HONESTY_TARGET_LEVEL_INVALID";
    private const string InvalidUntrainedHonestyTargetLevelMessage =
        "Dynamic graph untrained honesty can only be associated with an L2 logical-unit target.";
    private const string InvalidUntrainedHonestyL2ProbeResultCode =
        "INVALID_UNTRAINED_HONESTY_L2_PROBE_RESULT";
    private const string InvalidUntrainedHonestyL2ProbeResultMessage =
        "Dynamic graph L2 untrained honesty probe result is invalid.";
    private const string UntrainedHonestyTargetMismatchCode =
        "DYNAMIC_GRAPH_UNTRAINED_HONESTY_TARGET_MISMATCH";
    private const string UntrainedHonestyTargetMismatchMessage =
        "Dynamic graph untrained honesty target does not match the measured L2 producer.";
    private const string NoTrainedStateToken = "NO_TRAINED_STATE_LOADED";
    private const string NoSemanticClassificationToken = "NO_SEMANTIC_CLASSIFICATION_PERFORMED";
    private const string InvalidAblationTargetLevelCode =
        "DYNAMIC_GRAPH_ABLATION_TARGET_LEVEL_INVALID";
    private const string InvalidAblationTargetLevelMessage =
        "Dynamic graph ablation can only be associated with an L2 logical-unit or L3 logical-flow target.";
    private const string InvalidNoveltyTargetLevelCode =
        "DYNAMIC_GRAPH_NOVELTY_TARGET_LEVEL_INVALID";
    private const string InvalidNoveltyTargetLevelMessage =
        "Dynamic graph novelty can only be associated with an L2 logical-unit or L3 logical-flow target.";
    private const string InvalidTrainingSpeedTargetLevelCode =
        "DYNAMIC_GRAPH_TRAINING_SPEED_TARGET_LEVEL_INVALID";
    private const string InvalidTrainingSpeedTargetLevelMessage =
        "Dynamic graph training speed can only be associated with an L5 learning-machine target.";
    private const string InvalidEdgeOrderQualityTargetLevelCode =
        "DYNAMIC_GRAPH_EDGE_ORDER_QUALITY_TARGET_LEVEL_INVALID";
    private const string InvalidEdgeOrderQualityTargetLevelMessage =
        "Dynamic graph edge order quality can only be associated with an L3 logical-flow target.";
    private const string EdgeOrderQualityTargetMismatchCode =
        "DYNAMIC_GRAPH_EDGE_ORDER_QUALITY_TARGET_MISMATCH";
    private const string EdgeOrderQualityTargetMismatchMessage =
        "Dynamic graph edge order quality target does not match the measured edge.";
    private const string InvalidEdgeIdentityQualityTargetLevelCode =
        "DYNAMIC_GRAPH_EDGE_IDENTITY_QUALITY_TARGET_LEVEL_INVALID";
    private const string InvalidEdgeIdentityQualityTargetLevelMessage =
        "Dynamic graph edge identity quality can only be associated with an L3 logical-flow target.";
    private const string EdgeIdentityQualityTargetMismatchCode =
        "DYNAMIC_GRAPH_EDGE_IDENTITY_QUALITY_TARGET_MISMATCH";
    private const string EdgeIdentityQualityTargetMismatchMessage =
        "Dynamic graph edge identity quality target does not match the measured edge.";
    private const string InvalidEdgeAuthorityControlTargetLevelCode =
        "DYNAMIC_GRAPH_EDGE_AUTHORITY_CONTROL_TARGET_LEVEL_INVALID";
    private const string InvalidEdgeAuthorityControlTargetLevelMessage =
        "Dynamic graph edge authority control can only be associated with an L3 logical-flow target.";
    private const string EdgeAuthorityControlTargetMismatchCode =
        "DYNAMIC_GRAPH_EDGE_AUTHORITY_CONTROL_TARGET_MISMATCH";
    private const string EdgeAuthorityControlTargetMismatchMessage =
        "Dynamic graph edge authority control target does not match the measured edge.";
    private const string InvalidEdgeQualityEvidenceTargetLevelCode =
        "DYNAMIC_GRAPH_EDGE_QUALITY_EVIDENCE_TARGET_LEVEL_INVALID";
    private const string InvalidEdgeQualityEvidenceTargetLevelMessage =
        "Dynamic graph edge quality evidence can only be associated with an L3 logical-flow target.";
    private const string EdgeQualityEvidenceTargetMismatchCode =
        "DYNAMIC_GRAPH_EDGE_QUALITY_EVIDENCE_TARGET_MISMATCH";
    private const string EdgeQualityEvidenceTargetMismatchMessage =
        "Dynamic graph edge quality evidence target does not match the measured edge.";

    /// <summary>
    /// Admits an L2 logical-unit target and binds it to an already-produced,
    /// registry-backed untrained-honesty probe result.
    /// </summary>
    public static OperationResult<DynamicGraphTargetUntrainedHonestyMeasurement> BindL2UntrainedHonesty(
        DynamicGraphMeasurementTarget? target,
        DynamicGraphUntrainedHonestyL2ProbeResult? probeResult)
    {
        var targetValidation = DynamicGraphMeasurementTargetGuard.Validate(target);
        if (!targetValidation.IsSuccess)
            return targetValidation.MapError<DynamicGraphTargetUntrainedHonestyMeasurement>();

        var ownedTarget = targetValidation.Value!;
        if (ownedTarget.Level != DynamicGraphMeasurementTargetLevel.L2LogicalUnit)
        {
            return OperationResult<DynamicGraphTargetUntrainedHonestyMeasurement>.Fail(
                InvalidUntrainedHonestyTargetLevelMessage,
                InvalidUntrainedHonestyTargetLevelCode);
        }

        DynamicGraphUntrainedHonestyL2ProducerEvidence? producer;
        DynamicGraphUntrainedHonestyMeasurement? honesty;
        string? unitId;
        string? unitRole;
        string? domainTag;
        string? version;
        long registrationOrder;
        string? producerTypeName;
        string? inputTypeName;
        string? outputTypeName;
        string? evaluationId;
        bool runtimeReportedNotReady;
        bool hasNoTrainedStateEvidence;
        bool hasNoSemanticClassificationEvidence;
        bool meetsUntrainedHonestyGate;
        string[]? runtimeEvidence;
        string[]? inspectedTypeNames;
        string[]? forbiddenShortcutMatches;
        try
        {
            producer = probeResult?.Producer;
            honesty = probeResult?.Honesty;
            unitId = producer?.UnitId;
            unitRole = producer?.UnitRole;
            domainTag = producer?.DomainTag;
            version = producer?.Version;
            registrationOrder = producer?.RegistrationOrder ?? 0;
            producerTypeName = producer?.ProducerTypeName;
            inputTypeName = producer?.InputTypeName;
            outputTypeName = producer?.OutputTypeName;
            evaluationId = honesty?.EvaluationId;
            runtimeReportedNotReady = honesty?.RuntimeReportedNotReady ?? false;
            hasNoTrainedStateEvidence = honesty?.HasNoTrainedStateEvidence ?? false;
            hasNoSemanticClassificationEvidence = honesty?.HasNoSemanticClassificationEvidence ?? false;
            meetsUntrainedHonestyGate = honesty?.MeetsUntrainedHonestyGate ?? false;

            // Own the three carrier boundaries in deterministic contract order.
            runtimeEvidence = honesty?.RuntimeEvidence?.ToArray();
            inspectedTypeNames = honesty?.InspectedTypeNames?.ToArray();
            forbiddenShortcutMatches = honesty?.ForbiddenShortcutMatches?.ToArray();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidUntrainedHonestyL2ProbeResult();
        }

        if (producer is null
            || honesty is null
            || !IsCanonicalValue(unitId)
            || !IsCanonicalValue(unitRole)
            || !IsCanonicalValue(domainTag)
            || !IsCanonicalValue(version)
            || registrationOrder <= 0
            || !IsCanonicalValue(producerTypeName)
            || !IsCanonicalValue(inputTypeName)
            || !IsCanonicalValue(outputTypeName)
            || !IsCanonicalValue(evaluationId)
            || runtimeEvidence is null
            || inspectedTypeNames is null
            || forbiddenShortcutMatches is null
            || !IsCanonicalOrdinalSet(runtimeEvidence)
            || !IsCanonicalOrdinalSet(inspectedTypeNames, requireNonEmpty: true)
            || !IsCanonicalOrdinalSet(forbiddenShortcutMatches)
            || !inspectedTypeNames.Contains(producerTypeName!, StringComparer.Ordinal))
        {
            return InvalidUntrainedHonestyL2ProbeResult();
        }

        var recomputedHasNoTrainedStateEvidence = runtimeEvidence.Contains(
            NoTrainedStateToken,
            StringComparer.Ordinal);
        var recomputedHasNoSemanticClassificationEvidence = runtimeEvidence.Contains(
            NoSemanticClassificationToken,
            StringComparer.Ordinal);
        var recomputedGate = runtimeReportedNotReady
                             && recomputedHasNoTrainedStateEvidence
                             && recomputedHasNoSemanticClassificationEvidence
                             && forbiddenShortcutMatches.Length == 0;
        if (hasNoTrainedStateEvidence != recomputedHasNoTrainedStateEvidence
            || hasNoSemanticClassificationEvidence != recomputedHasNoSemanticClassificationEvidence
            || meetsUntrainedHonestyGate != recomputedGate)
        {
            return InvalidUntrainedHonestyL2ProbeResult();
        }

        if (!StringComparer.Ordinal.Equals(ownedTarget.TargetId, unitId))
        {
            return OperationResult<DynamicGraphTargetUntrainedHonestyMeasurement>.Fail(
                UntrainedHonestyTargetMismatchMessage,
                UntrainedHonestyTargetMismatchCode);
        }

        var ownedProducer = new DynamicGraphUntrainedHonestyL2ProducerEvidence(
            unitId!,
            unitRole!,
            domainTag!,
            version!,
            registrationOrder,
            producerTypeName!,
            inputTypeName!,
            outputTypeName!);
        var ownedHonesty = new DynamicGraphUntrainedHonestyMeasurement(
            evaluationId!,
            runtimeReportedNotReady,
            hasNoTrainedStateEvidence,
            hasNoSemanticClassificationEvidence,
            Array.AsReadOnly(runtimeEvidence.ToArray()),
            Array.AsReadOnly(inspectedTypeNames.ToArray()),
            Array.AsReadOnly(forbiddenShortcutMatches.ToArray()),
            meetsUntrainedHonestyGate);

        return OperationResult<DynamicGraphTargetUntrainedHonestyMeasurement>.Ok(
            new DynamicGraphTargetUntrainedHonestyMeasurement(
                ownedTarget,
                new DynamicGraphUntrainedHonestyL2ProbeResult(ownedProducer, ownedHonesty)));
    }

    /// <summary>
    /// Admits an L2 logical-unit or L3 logical-flow target before reading ablation evidence.
    /// </summary>
    public static OperationResult<DynamicGraphTargetAblationMeasurement> MeasureAblation(
        DynamicGraphMeasurementTarget? target,
        DynamicGraphAccuracyMeasurement? subject,
        DynamicGraphAccuracyMeasurement? ablated)
    {
        var targetValidation = DynamicGraphMeasurementTargetGuard.Validate(target);
        if (!targetValidation.IsSuccess)
            return targetValidation.MapError<DynamicGraphTargetAblationMeasurement>();

        if (targetValidation.Value!.Level is not DynamicGraphMeasurementTargetLevel.L2LogicalUnit
            and not DynamicGraphMeasurementTargetLevel.L3LogicalFlow)
        {
            return OperationResult<DynamicGraphTargetAblationMeasurement>.Fail(
                InvalidAblationTargetLevelMessage,
                InvalidAblationTargetLevelCode);
        }

        var ablationMeasurement = DynamicGraphAblationMeter.Measure(subject, ablated);
        if (!ablationMeasurement.IsSuccess)
            return ablationMeasurement.MapError<DynamicGraphTargetAblationMeasurement>();

        return OperationResult<DynamicGraphTargetAblationMeasurement>.Ok(
            new DynamicGraphTargetAblationMeasurement(
                targetValidation.Value,
                ablationMeasurement.Value!));
    }

    /// <summary>
    /// Admits the measurement target before reading any downstream accuracy evidence.
    /// </summary>
    public static OperationResult<DynamicGraphTargetAccuracyMeasurement> MeasureAccuracy(
        DynamicGraphMeasurementTarget? target,
        double liveMarginAccuracy,
        LearningEvaluationResult? before,
        LearningEvaluationResult? after,
        DynamicGraphMeasurementSplitProof? splitProof)
    {
        var targetValidation = DynamicGraphMeasurementTargetGuard.Validate(target);
        if (!targetValidation.IsSuccess)
            return targetValidation.MapError<DynamicGraphTargetAccuracyMeasurement>();

        var accuracyMeasurement = DynamicGraphAccuracyMeter.Measure(
            liveMarginAccuracy,
            before,
            after,
            splitProof);
        if (!accuracyMeasurement.IsSuccess)
            return accuracyMeasurement.MapError<DynamicGraphTargetAccuracyMeasurement>();

        return OperationResult<DynamicGraphTargetAccuracyMeasurement>.Ok(
            new DynamicGraphTargetAccuracyMeasurement(
                targetValidation.Value!,
                accuracyMeasurement.Value!));
    }

    /// <summary>
    /// Admits an L2 logical-unit or L3 logical-flow target before reading novelty evidence.
    /// </summary>
    public static OperationResult<DynamicGraphTargetNoveltyMeasurement> MeasureNovelty(
        DynamicGraphMeasurementTarget? target,
        DynamicGraphNoveltyObservation? observation)
    {
        var targetValidation = DynamicGraphMeasurementTargetGuard.Validate(target);
        if (!targetValidation.IsSuccess)
            return targetValidation.MapError<DynamicGraphTargetNoveltyMeasurement>();

        if (targetValidation.Value!.Level is not DynamicGraphMeasurementTargetLevel.L2LogicalUnit
            and not DynamicGraphMeasurementTargetLevel.L3LogicalFlow)
        {
            return OperationResult<DynamicGraphTargetNoveltyMeasurement>.Fail(
                InvalidNoveltyTargetLevelMessage,
                InvalidNoveltyTargetLevelCode);
        }

        var noveltyMeasurement = DynamicGraphNoveltyMeter.Measure(observation);
        if (!noveltyMeasurement.IsSuccess)
            return noveltyMeasurement.MapError<DynamicGraphTargetNoveltyMeasurement>();

        return OperationResult<DynamicGraphTargetNoveltyMeasurement>.Ok(
            new DynamicGraphTargetNoveltyMeasurement(
                targetValidation.Value,
                noveltyMeasurement.Value!));
    }

    /// <summary>
    /// Admits the measurement target before reading any downstream run-speed observation.
    /// </summary>
    public static OperationResult<DynamicGraphTargetRunSpeedMeasurement> MeasureRunSpeed(
        DynamicGraphMeasurementTarget? target,
        DynamicGraphRunSpeedObservation? observation)
    {
        var targetValidation = DynamicGraphMeasurementTargetGuard.Validate(target);
        if (!targetValidation.IsSuccess)
            return targetValidation.MapError<DynamicGraphTargetRunSpeedMeasurement>();

        var runSpeedMeasurement = DynamicGraphRunSpeedMeter.Measure(observation);
        if (!runSpeedMeasurement.IsSuccess)
            return runSpeedMeasurement.MapError<DynamicGraphTargetRunSpeedMeasurement>();

        return OperationResult<DynamicGraphTargetRunSpeedMeasurement>.Ok(
            new DynamicGraphTargetRunSpeedMeasurement(
                targetValidation.Value!,
                runSpeedMeasurement.Value!));
    }

    /// <summary>
    /// Admits an L5 measurement target before reading an already-produced batch result.
    /// </summary>
    public static OperationResult<DynamicGraphTargetTrainingSpeedMeasurement> MeasureTrainingSpeed(
        DynamicGraphMeasurementTarget? target,
        LearningBatchResult? result)
    {
        var targetValidation = DynamicGraphMeasurementTargetGuard.Validate(target);
        if (!targetValidation.IsSuccess)
            return targetValidation.MapError<DynamicGraphTargetTrainingSpeedMeasurement>();

        if (targetValidation.Value!.Level != DynamicGraphMeasurementTargetLevel.L5LearningMachine)
        {
            return OperationResult<DynamicGraphTargetTrainingSpeedMeasurement>.Fail(
                InvalidTrainingSpeedTargetLevelMessage,
                InvalidTrainingSpeedTargetLevelCode);
        }

        var trainingSpeedMeasurement = DynamicGraphSpeedMeter.Measure(result);
        if (!trainingSpeedMeasurement.IsSuccess)
            return trainingSpeedMeasurement.MapError<DynamicGraphTargetTrainingSpeedMeasurement>();

        return OperationResult<DynamicGraphTargetTrainingSpeedMeasurement>.Ok(
            new DynamicGraphTargetTrainingSpeedMeasurement(
                targetValidation.Value,
                trainingSpeedMeasurement.Value!));
    }

    /// <summary>
    /// Admits an L3 logical-flow target before measuring and binding per-edge authority-control evidence.
    /// </summary>
    public static OperationResult<DynamicGraphTargetEdgeAuthorityControlMeasurement> MeasureEdgeAuthorityControl(
        DynamicGraphMeasurementTarget? target,
        EdgeAuthorityExecutionResult? result)
    {
        var targetValidation = DynamicGraphMeasurementTargetGuard.Validate(target);
        if (!targetValidation.IsSuccess)
            return targetValidation.MapError<DynamicGraphTargetEdgeAuthorityControlMeasurement>();

        if (targetValidation.Value!.Level != DynamicGraphMeasurementTargetLevel.L3LogicalFlow)
        {
            return OperationResult<DynamicGraphTargetEdgeAuthorityControlMeasurement>.Fail(
                InvalidEdgeAuthorityControlTargetLevelMessage,
                InvalidEdgeAuthorityControlTargetLevelCode);
        }

        var edgeAuthorityControlMeasurement = DynamicGraphEdgeAuthorityControlMeter.Measure(result);
        if (!edgeAuthorityControlMeasurement.IsSuccess)
        {
            return edgeAuthorityControlMeasurement
                .MapError<DynamicGraphTargetEdgeAuthorityControlMeasurement>();
        }

        if (!StringComparer.Ordinal.Equals(
                targetValidation.Value.TargetId,
                edgeAuthorityControlMeasurement.Value!.AuthorityEvidence.EdgeId))
        {
            return OperationResult<DynamicGraphTargetEdgeAuthorityControlMeasurement>.Fail(
                EdgeAuthorityControlTargetMismatchMessage,
                EdgeAuthorityControlTargetMismatchCode);
        }

        return OperationResult<DynamicGraphTargetEdgeAuthorityControlMeasurement>.Ok(
            new DynamicGraphTargetEdgeAuthorityControlMeasurement(
                targetValidation.Value,
                edgeAuthorityControlMeasurement.Value));
    }

    /// <summary>
    /// Admits an L3 logical-flow target before composing correlated edge-authority,
    /// novelty, and numeric ablation evidence for the target edge.
    /// </summary>
    public static OperationResult<DynamicGraphTargetEdgeQualityEvidenceMeasurement> MeasureEdgeQualityEvidence(
        DynamicGraphMeasurementTarget? target,
        EdgeAuthorityExecutionResult? authorityResult,
        DynamicGraphNoveltyObservation? noveltyObservation,
        DynamicGraphAccuracyMeasurement? subject,
        DynamicGraphAccuracyMeasurement? ablated)
    {
        var targetValidation = DynamicGraphMeasurementTargetGuard.Validate(target);
        if (!targetValidation.IsSuccess)
            return targetValidation.MapError<DynamicGraphTargetEdgeQualityEvidenceMeasurement>();

        if (targetValidation.Value!.Level != DynamicGraphMeasurementTargetLevel.L3LogicalFlow)
        {
            return OperationResult<DynamicGraphTargetEdgeQualityEvidenceMeasurement>.Fail(
                InvalidEdgeQualityEvidenceTargetLevelMessage,
                InvalidEdgeQualityEvidenceTargetLevelCode);
        }

        var edgeQualityEvidence = DynamicGraphEdgeQualityEvidenceMeter.Measure(
            authorityResult,
            noveltyObservation,
            subject,
            ablated);
        if (!edgeQualityEvidence.IsSuccess)
            return edgeQualityEvidence.MapError<DynamicGraphTargetEdgeQualityEvidenceMeasurement>();

        if (!StringComparer.Ordinal.Equals(
                targetValidation.Value.TargetId,
                edgeQualityEvidence.Value!.EdgeAuthorityControl.AuthorityEvidence.EdgeId))
        {
            return OperationResult<DynamicGraphTargetEdgeQualityEvidenceMeasurement>.Fail(
                EdgeQualityEvidenceTargetMismatchMessage,
                EdgeQualityEvidenceTargetMismatchCode);
        }

        return OperationResult<DynamicGraphTargetEdgeQualityEvidenceMeasurement>.Ok(
            new DynamicGraphTargetEdgeQualityEvidenceMeasurement(
                targetValidation.Value,
                edgeQualityEvidence.Value));
    }

    /// <summary>
    /// Admits an L3 logical-flow target before measuring and binding edge-identity evidence.
    /// </summary>
    public static OperationResult<DynamicGraphTargetEdgeIdentityQualityMeasurement> MeasureEdgeIdentityQuality(
        DynamicGraphMeasurementTarget? target,
        EdgeAuthorityExecutionResult? result)
    {
        var targetValidation = DynamicGraphMeasurementTargetGuard.Validate(target);
        if (!targetValidation.IsSuccess)
            return targetValidation.MapError<DynamicGraphTargetEdgeIdentityQualityMeasurement>();

        if (targetValidation.Value!.Level != DynamicGraphMeasurementTargetLevel.L3LogicalFlow)
        {
            return OperationResult<DynamicGraphTargetEdgeIdentityQualityMeasurement>.Fail(
                InvalidEdgeIdentityQualityTargetLevelMessage,
                InvalidEdgeIdentityQualityTargetLevelCode);
        }

        var edgeIdentityQualityMeasurement = DynamicGraphEdgeIdentityQualityMeter.Measure(result);
        if (!edgeIdentityQualityMeasurement.IsSuccess)
            return edgeIdentityQualityMeasurement.MapError<DynamicGraphTargetEdgeIdentityQualityMeasurement>();

        if (!StringComparer.Ordinal.Equals(
                targetValidation.Value.TargetId,
                edgeIdentityQualityMeasurement.Value!.EdgeId))
        {
            return OperationResult<DynamicGraphTargetEdgeIdentityQualityMeasurement>.Fail(
                EdgeIdentityQualityTargetMismatchMessage,
                EdgeIdentityQualityTargetMismatchCode);
        }

        return OperationResult<DynamicGraphTargetEdgeIdentityQualityMeasurement>.Ok(
            new DynamicGraphTargetEdgeIdentityQualityMeasurement(
                targetValidation.Value,
                edgeIdentityQualityMeasurement.Value));
    }

    /// <summary>
    /// Admits an L3 logical-flow target before measuring and binding edge-order evidence.
    /// </summary>
    public static OperationResult<DynamicGraphTargetEdgeOrderQualityMeasurement> MeasureEdgeOrderQuality(
        DynamicGraphMeasurementTarget? target,
        EdgeAuthorityExecutionResult? result)
    {
        var targetValidation = DynamicGraphMeasurementTargetGuard.Validate(target);
        if (!targetValidation.IsSuccess)
            return targetValidation.MapError<DynamicGraphTargetEdgeOrderQualityMeasurement>();

        if (targetValidation.Value!.Level != DynamicGraphMeasurementTargetLevel.L3LogicalFlow)
        {
            return OperationResult<DynamicGraphTargetEdgeOrderQualityMeasurement>.Fail(
                InvalidEdgeOrderQualityTargetLevelMessage,
                InvalidEdgeOrderQualityTargetLevelCode);
        }

        var edgeOrderQualityMeasurement = DynamicGraphEdgeOrderQualityMeter.Measure(result);
        if (!edgeOrderQualityMeasurement.IsSuccess)
            return edgeOrderQualityMeasurement.MapError<DynamicGraphTargetEdgeOrderQualityMeasurement>();

        if (!StringComparer.Ordinal.Equals(
                targetValidation.Value.TargetId,
                edgeOrderQualityMeasurement.Value!.EdgeId))
        {
            return OperationResult<DynamicGraphTargetEdgeOrderQualityMeasurement>.Fail(
                EdgeOrderQualityTargetMismatchMessage,
                EdgeOrderQualityTargetMismatchCode);
        }

        return OperationResult<DynamicGraphTargetEdgeOrderQualityMeasurement>.Ok(
            new DynamicGraphTargetEdgeOrderQualityMeasurement(
                targetValidation.Value,
                edgeOrderQualityMeasurement.Value));
    }

    private static bool IsCanonicalOrdinalSet(
        IReadOnlyList<string> values,
        bool requireNonEmpty = false)
    {
        if (requireNonEmpty && values.Count == 0)
            return false;

        string? previous = null;
        for (var index = 0; index < values.Count; index++)
        {
            var value = values[index];
            if (!IsCanonicalValue(value)
                || (previous is not null
                    && StringComparer.Ordinal.Compare(previous, value) >= 0))
            {
                return false;
            }

            previous = value;
        }

        return true;
    }

    private static bool IsCanonicalValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !StringComparer.Ordinal.Equals(value, value.Trim()))
        {
            return false;
        }

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsControl(character)
                || character is '\u0085' or '\u2028' or '\u2029')
            {
                return false;
            }

            if (char.IsHighSurrogate(character))
            {
                if (index + 1 >= value.Length
                    || !char.IsLowSurrogate(value[index + 1]))
                {
                    return false;
                }

                index++;
                continue;
            }

            if (char.IsLowSurrogate(character))
                return false;
        }

        return true;
    }

    private static OperationResult<DynamicGraphTargetUntrainedHonestyMeasurement>
        InvalidUntrainedHonestyL2ProbeResult()
        => OperationResult<DynamicGraphTargetUntrainedHonestyMeasurement>.Fail(
            InvalidUntrainedHonestyL2ProbeResultMessage,
            InvalidUntrainedHonestyL2ProbeResultCode);
}
