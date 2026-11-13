// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using TensorCore.Core;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;

/// <summary>
/// Shared DesignConversion adapter into the neutral bounded DPO content
/// authority. It also guards every collection before the existing immutable
/// record-clone helpers allocate from it.
/// </summary>
internal static class DesignConversionDpoTrainingContentAuthority
{
    internal const string CanonicalContext =
        "design-conversion-canonical-dpo-row-v1";
    internal const int MaximumNestedCollectionCount = 4_096;
    internal const int MaximumMetadataCount = 1_024;
    internal const long MaximumSnapshotUtf8Bytes = 32L * 1_048_576;
    internal const long MaximumSnapshotElements = 131_072;
    private const string DualTargetTeacherAggregateContextVersion =
        "design-conversion-dual-target-teacher-aggregate-v1";
    private static readonly string[] DualTargetTeacherUnitOrder =
    [
        DesignConversionDpoUnits.CriticUnit,
        DesignConversionDpoUnits.ImproverUnit
    ];
    private static readonly string[] DualTargetTeacherAttestationKeys =
    [
        "runtime_teacher_authored_candidate",
        "runtime_teacher_selected_preference",
        "teacher_authoring_schema",
        "teacher_authoring_output_hash",
        "teacher_authoring_incumbent_ref",
        "teacher_authoring_generated_ref",
        "teacher_inference_profile_id",
        "teacher_inference_profile_configuration_sha256"
    ];

    internal static OperationResult<DesignConversionDpoTrainingRecord[]>
        CaptureRecords(
            IEnumerable<DesignConversionDpoTrainingRecord>? records,
            bool requireTeacherProvenance = true)
    {
        if (records is null)
            return CaptureInvalid();

        var owned = new List<DesignConversionDpoTrainingRecord>();
        var budget = new SnapshotBudget();
        var missingRequiredTeacherProvenance = false;
        try
        {
            using var enumerator = records.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (owned.Count ==
                    BoundedDpoTrainingContentProjector.MaximumRowCount)
                {
                    return CaptureLimitExceeded();
                }
                if (!TryCaptureRecord(
                        enumerator.Current,
                        budget,
                        requireTeacherProvenance: false,
                        out var captured))
                {
                    return CaptureLimitExceeded();
                }
                missingRequiredTeacherProvenance |=
                    requireTeacherProvenance &&
                    captured.TeacherProvenance is null;
                owned.Add(captured);
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return CaptureInvalid();
        }

        if (owned.Count == 0)
            return CaptureInvalid();
        if (missingRequiredTeacherProvenance)
            return CaptureTeacherProvenanceRequired();
        return OperationResult<DesignConversionDpoTrainingRecord[]>.Ok(
            owned.ToArray());
    }

    internal static OperationResult<DesignConversionStoredDpoExample[]>
        CaptureExamples(
            IEnumerable<DesignConversionStoredDpoExample>? examples)
    {
        if (examples is null)
            return CaptureExamplesInvalid();
        var owned = new List<DesignConversionStoredDpoExample>();
        var budget = new SnapshotBudget();
        try
        {
            using var enumerator = examples.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (owned.Count ==
                    BoundedDpoTrainingContentProjector.MaximumRowCount)
                {
                    return CaptureExamplesInvalid();
                }
                var example = enumerator.Current;
                if (example is null ||
                    !TryCaptureRecord(
                        new DesignConversionDpoTrainingRecord(
                            example.ExampleId,
                            example.UnitId,
                            example.InternalUnitStatePayload,
                            example.InternalVectorPayload,
                            example.SourceStructurePayload,
                            example.Chosen,
                            example.Rejected,
                            example.TieBreakerPayload,
                            example.UsefulnessScore,
                            example.NegativeQualityReason,
                            example.NegativeReasonTaxonomy,
                            example.Provenance,
                            example.License,
                            new DesignConversionQuarantineState(
                                false,
                                string.Empty,
                                []),
                            example.TeacherProvenance)
                        {
                            ChosenAuthorKind = example.ChosenAuthorKind,
                            RejectedAuthorKind = example.RejectedAuthorKind
                        },
                        budget,
                        requireTeacherProvenance: true,
                        out var captured) ||
                    !TryCaptureStrings(
                        example.TeacherEvidenceRefs,
                        budget,
                        out var evidence) ||
                    !TryCaptureStringDictionary(
                        example.Metadata,
                        budget,
                        out var metadata))
                {
                    return CaptureExamplesInvalid();
                }
                owned.Add(new DesignConversionStoredDpoExample(
                    captured.RecordId,
                    captured.UnitId,
                    captured.InternalUnitStatePayload,
                    captured.InternalVectorPayload,
                    captured.SourceStructurePayload,
                    captured.Chosen,
                    captured.Rejected,
                    captured.TieBreakerPayload,
                    captured.UsefulnessScore,
                    captured.NegativeQualityReason,
                    captured.NegativeReasonTaxonomy,
                    captured.Provenance,
                    captured.License,
                    evidence,
                    metadata,
                    captured.TeacherProvenance)
                {
                    ChosenAuthorKind = captured.ChosenAuthorKind,
                    RejectedAuthorKind = captured.RejectedAuthorKind
                });
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return CaptureExamplesInvalid();
        }
        return owned.Count == 0
            ? CaptureExamplesInvalid()
            : OperationResult<DesignConversionStoredDpoExample[]>.Ok(
                owned.ToArray());
    }

    internal static OperationResult<DesignConversionDpoTrainingRecord>
        CaptureRecord(
            DesignConversionDpoTrainingRecord? record,
            bool requireTeacherProvenance = true)
    {
        try
        {
            return TryCaptureRecord(
                record,
                new SnapshotBudget(),
                requireTeacherProvenance,
                out var captured)
                ? OperationResult<DesignConversionDpoTrainingRecord>.Ok(
                    captured)
                : CaptureInvalidRecord();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return CaptureInvalidRecord();
        }
    }

    internal static bool TryCapturePartition(
        IReadOnlyList<DesignConversionDpoTrainingRecord>? trainRecords,
        IReadOnlyList<DesignConversionDpoTrainingRecord>? heldOutRecords,
        int maximumTrainRecords,
        out DesignConversionDpoTrainingRecord[] train,
        out DesignConversionDpoTrainingRecord[] heldOut)
    {
        train = null!;
        heldOut = null!;
        if (trainRecords is null || heldOutRecords is null ||
            maximumTrainRecords <= 0)
        {
            return false;
        }
        try
        {
            var trainCount = Math.Min(
                trainRecords.Count,
                maximumTrainRecords);
            var heldOutCount = heldOutRecords.Count;
            if (trainCount <= 0 || heldOutCount <= 0 ||
                (long)trainCount + heldOutCount >
                    BoundedDpoTrainingContentProjector.MaximumRowCount)
            {
                return false;
            }

            var budget = new SnapshotBudget();
            train = new DesignConversionDpoTrainingRecord[trainCount];
            for (var index = 0; index < trainCount; index++)
            {
                if (!TryCaptureRecord(
                        trainRecords[index],
                        budget,
                        requireTeacherProvenance: true,
                        out train[index]))
                {
                    return false;
                }
            }
            heldOut = new DesignConversionDpoTrainingRecord[heldOutCount];
            for (var index = 0; index < heldOutCount; index++)
            {
                if (!TryCaptureRecord(
                        heldOutRecords[index],
                        budget,
                        requireTeacherProvenance: true,
                        out heldOut[index]))
                {
                    return false;
                }
            }
            return true;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            train = null!;
            heldOut = null!;
            return false;
        }
    }

    /// <summary>
    /// Projects the exact already-built optimizer objects and returns those
    /// same object references with provenance rebound to the resulting row
    /// digests. Finite float values are expanded exactly to doubles; this is
    /// an injective canonical representation of every finite IEEE-754 float,
    /// including signed zero. An optional weight is appended after the vector
    /// and its boundary is committed in the row context.
    /// </summary>
    internal static OperationResult<DesignConversionFinalOptimizerBatch<T>>
        ProjectFinalOptimizerInputs<T>(
            IReadOnlyList<T>? optimizerInputs,
            IReadOnlyList<TrainingTeacherProvenance?>? provenance,
            Func<T, string> prompt,
            Func<T, string> chosen,
            Func<T, string> rejected,
            Func<T, IReadOnlyList<float>?> teachingVector,
            Func<T, float?> weight,
            Func<T, string> context)
        where T : class
        => ProjectFinalOptimizerInputsCore(
            optimizerInputs,
            provenance,
            prompt,
            chosen,
            rejected,
            teachingVector,
            weight,
            context,
            validateOptimizerSides: true);

    private static OperationResult<DesignConversionFinalOptimizerBatch<T>>
        ProjectFinalOptimizerInputsCore<T>(
            IReadOnlyList<T>? optimizerInputs,
            IReadOnlyList<TrainingTeacherProvenance?>? provenance,
            Func<T, string> prompt,
            Func<T, string> chosen,
            Func<T, string> rejected,
            Func<T, IReadOnlyList<float>?> teachingVector,
            Func<T, float?> weight,
            Func<T, string> context,
            bool validateOptimizerSides)
        where T : class
    {
        if (optimizerInputs is null || provenance is null)
            return FinalInputInvalid<T>();

        try
        {
            var count = optimizerInputs.Count;
            if (count is <= 0 or >
                    BoundedDpoTrainingContentProjector.MaximumRowCount ||
                provenance.Count != count)
            {
                return FinalInputInvalid<T>();
            }

            var exactInputs = new T[count];
            var rows = new DpoTrainingContentRowInput[count];
            for (var index = 0; index < count; index++)
            {
                var input = optimizerInputs[index];
                var rowProvenance = provenance[index];
                if (input is null || rowProvenance is null)
                    return FinalInputInvalid<T>();

                var vector = teachingVector(input);
                var vectorCount = vector?.Count ?? 0;
                var rowWeight = weight(input);
                if (vectorCount >
                        BoundedDpoTrainingContentProjector.MaximumVectorLength -
                        (rowWeight.HasValue ? 1 : 0) ||
                    rowWeight.HasValue && !float.IsFinite(rowWeight.Value))
                {
                    return FinalInputInvalid<T>();
                }

                var canonicalVector = new double[
                    vectorCount + (rowWeight.HasValue ? 1 : 0)];
                for (var vectorIndex = 0;
                     vectorIndex < vectorCount;
                     vectorIndex++)
                {
                    var value = vector![vectorIndex];
                    if (!float.IsFinite(value))
                        return FinalInputInvalid<T>();
                    canonicalVector[vectorIndex] = value;
                }
                if (rowWeight.HasValue)
                    canonicalVector[^1] = rowWeight.Value;

                var chosenText = chosen(input);
                var rejectedText = rejected(input);
                if (validateOptimizerSides)
                {
                    var sideContract =
                        DesignConversionDpoOptimizerSideContract.ValidatePair(
                            chosenText,
                            rejectedText);
                    if (!sideContract.IsSuccess)
                    {
                        return sideContract.MapError<
                            DesignConversionFinalOptimizerBatch<T>>();
                    }
                }

                exactInputs[index] = input;
                rows[index] = new DpoTrainingContentRowInput(
                    prompt(input),
                    chosenText,
                    rejectedText,
                    canonicalVector,
                    string.Concat(
                        context(input),
                        "|float_vector_length=",
                        vectorCount.ToString(
                            System.Globalization.CultureInfo.InvariantCulture),
                        "|float_weight=",
                        rowWeight.HasValue ? "tail" : "absent"));
            }

            var content = BoundedDpoTrainingContentProjector.Project(rows);
            if (!content.IsSuccess || content.Value is null)
                return content.MapError<
                    DesignConversionFinalOptimizerBatch<T>>();

            var boundProvenance =
                new TrainingTeacherProvenance?[count];
            for (var index = 0; index < count; index++)
            {
                boundProvenance[index] = provenance[index]! with
                {
                    TrainingContentSha256 =
                        content.Value.Rows[index].Sha256
                };
            }
            return OperationResult<
                DesignConversionFinalOptimizerBatch<T>>.Ok(
                new DesignConversionFinalOptimizerBatch<T>(
                    exactInputs,
                    boundProvenance,
                    content.Value));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FinalInputInvalid<T>();
        }
    }

    internal static OperationResult<
        DesignConversionFinalOptimizerBatch<
            DesignConversionCompositeOptimizerInput>>
        ProjectCompositeOptimizerInputs(
            IReadOnlyList<LearningTrainingItem>? pipelineInputs,
            IReadOnlyList<TrainingTeacherProvenance?>? pipelineProvenance,
            IReadOnlyList<DpoPair>? persistedPairs,
            IReadOnlyList<DesignConversionDpoEvaluatedPackedBatch>?
                bridgeInputs)
        => ProjectCompositeOptimizerInputsCore(
            pipelineInputs,
            pipelineProvenance,
            persistedPairs,
            bridgeInputs,
            pipelineFinalOptimizer: null,
            bridgeFinalOptimizer: null);

    internal static OperationResult<CompositeOptimizerCommitment>
        ProjectCompositeOptimizerInputs(
            DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
                pipelineFinalOptimizer,
            IReadOnlyList<DpoPair>? persistedPairs,
            DesignConversionFinalOptimizerBatch<
                DesignConversionDpoEvaluatedPackedBatch>?
                bridgeFinalOptimizer)
    {
        if (!IsCanonicalFinalOptimizer(pipelineFinalOptimizer) ||
            bridgeFinalOptimizer is not null &&
            !IsCanonicalFinalOptimizer(bridgeFinalOptimizer) ||
            persistedPairs is null)
        {
            return CompositeInvalid();
        }

        DpoPair[] exactPairs;
        try
        {
            exactPairs = persistedPairs.ToArray();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return CompositeInvalid();
        }

        var projected = ProjectCompositeOptimizerInputsCore(
            pipelineFinalOptimizer!.Inputs,
            pipelineFinalOptimizer.Provenance,
            exactPairs,
            bridgeFinalOptimizer?.Inputs ?? [],
            pipelineFinalOptimizer,
            bridgeFinalOptimizer);
        if (!projected.IsSuccess || projected.Value is null)
            return projected.MapError<CompositeOptimizerCommitment>();

        return OperationResult<CompositeOptimizerCommitment>.Ok(
            new CompositeOptimizerCommitment(
                projected.Value,
                pipelineFinalOptimizer,
                bridgeFinalOptimizer,
                exactPairs));
    }

    private static OperationResult<
        DesignConversionFinalOptimizerBatch<
            DesignConversionCompositeOptimizerInput>>
        ProjectCompositeOptimizerInputsCore(
            IReadOnlyList<LearningTrainingItem>? pipelineInputs,
            IReadOnlyList<TrainingTeacherProvenance?>? pipelineProvenance,
            IReadOnlyList<DpoPair>? persistedPairs,
            IReadOnlyList<DesignConversionDpoEvaluatedPackedBatch>?
                bridgeInputs,
            DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
                pipelineFinalOptimizer,
            DesignConversionFinalOptimizerBatch<
                DesignConversionDpoEvaluatedPackedBatch>?
                bridgeFinalOptimizer)
    {
        if (pipelineInputs is null || pipelineProvenance is null ||
            persistedPairs is null || bridgeInputs is null ||
            pipelineInputs.Count == 0 ||
            persistedPairs.Count != pipelineInputs.Count ||
            bridgeInputs.Count > pipelineInputs.Count ||
            pipelineInputs.Count != pipelineProvenance.Count)
        {
            return FinalInputInvalid<DesignConversionCompositeOptimizerInput>();
        }

        try
        {
            var bridgeByRequestId = new Dictionary<string,
                (DesignConversionDpoEvaluatedPackedBatch Input, int Ordinal)>(
                    StringComparer.Ordinal);
            for (var bridgeIndex = 0;
                 bridgeIndex < bridgeInputs.Count;
                 bridgeIndex++)
            {
                var bridge = bridgeInputs[bridgeIndex];
                if (bridge?.Provenance is not { } teacher ||
                    string.IsNullOrWhiteSpace(teacher.RequestId) ||
                    !bridgeByRequestId.TryAdd(
                        teacher.RequestId,
                        (bridge, bridgeIndex)))
                {
                    return FinalInputInvalid<
                        DesignConversionCompositeOptimizerInput>();
                }
                var bridgeSideContract =
                    DesignConversionDpoOptimizerSideContract.ValidatePair(
                        bridge.Chosen,
                        bridge.Rejected);
                if (!bridgeSideContract.IsSuccess)
                {
                    return bridgeSideContract.MapError<
                        DesignConversionFinalOptimizerBatch<
                            DesignConversionCompositeOptimizerInput>>();
                }
            }

            var requestIds = new HashSet<string>(StringComparer.Ordinal);
            var matchedBridgeRequestIds = new HashSet<string>(
                StringComparer.Ordinal);
            var composite = new DesignConversionCompositeOptimizerInput[
                pipelineInputs.Count];
            for (var index = 0; index < composite.Length; index++)
            {
                var pipeline = pipelineInputs[index];
                var persistedPair = persistedPairs[index];
                var pipelineTeacher = pipelineProvenance[index];
                var persistedTeacher = persistedPair?.TeacherProvenance;
                if (pipeline is null || persistedPair is null ||
                    pipelineTeacher is null || persistedTeacher is null ||
                    persistedPair.GovernanceDecision is null ||
                    string.IsNullOrWhiteSpace(pipelineTeacher.RequestId) ||
                    !requestIds.Add(pipelineTeacher.RequestId) ||
                    !string.Equals(
                        pipelineTeacher.RequestId,
                        persistedTeacher.RequestId,
                        StringComparison.Ordinal))
                {
                    return FinalInputInvalid<
                        DesignConversionCompositeOptimizerInput>();
                }

                var pipelineSideContract =
                    DesignConversionDpoOptimizerSideContract.ValidatePair(
                        pipeline.ChosenResponse,
                        pipeline.RejectedResponse);
                if (!pipelineSideContract.IsSuccess)
                {
                    return pipelineSideContract.MapError<
                        DesignConversionFinalOptimizerBatch<
                            DesignConversionCompositeOptimizerInput>>();
                }
                var persistedSideContract =
                    DesignConversionDpoOptimizerSideContract.ValidatePair(
                        persistedPair.Chosen,
                        persistedPair.Rejected);
                if (!persistedSideContract.IsSuccess)
                {
                    return persistedSideContract.MapError<
                        DesignConversionFinalOptimizerBatch<
                            DesignConversionCompositeOptimizerInput>>();
                }

                bridgeByRequestId.TryGetValue(
                    pipelineTeacher.RequestId,
                    out var bridgeMatch);
                var bridge = bridgeMatch.Input;
                if (bridge is not null)
                    matchedBridgeRequestIds.Add(pipelineTeacher.RequestId);
                composite[index] = new DesignConversionCompositeOptimizerInput(
                    pipeline,
                    persistedPair,
                    bridge,
                    PipelineOrdinal: index,
                    PipelineChildBatchSha256:
                        pipelineFinalOptimizer?.Content.Sha256 ??
                        "untyped_pipeline_child",
                    PipelineChildRowSha256:
                        pipelineFinalOptimizer?.Content.Rows[index].Sha256 ??
                        "untyped_pipeline_row",
                    BridgeOrdinal: bridge is null
                        ? null
                        : bridgeMatch.Ordinal,
                    BridgeChildBatchSha256: bridge is null
                        ? "stage_absent"
                        : bridgeFinalOptimizer?.Content.Sha256 ??
                            "untyped_bridge_child",
                    BridgeChildRowSha256: bridge is null
                        ? "stage_absent"
                        : bridgeFinalOptimizer?.Content
                            .Rows[bridgeMatch.Ordinal].Sha256 ??
                            "untyped_bridge_row");
            }
            if (matchedBridgeRequestIds.Count != bridgeByRequestId.Count)
            {
                return FinalInputInvalid<
                    DesignConversionCompositeOptimizerInput>();
            }

            return ProjectFinalOptimizerInputsCore(
                composite,
                pipelineProvenance,
                static input => FrameCompositeField(
                    "pipeline.prompt",
                    input.PipelineInput.Prompt,
                    "persisted_pair.prompt",
                    input.PersistedPair.Prompt) + "|" +
                    FrameCompositeValue(
                        "bridge.prompt",
                        input.BridgeInput?.Prompt ?? "stage_absent"),
                static input => FrameCompositeField(
                    "pipeline.chosen",
                    input.PipelineInput.ChosenResponse,
                    "persisted_pair.chosen",
                    input.PersistedPair.Chosen) + "|" +
                    FrameCompositeValue(
                        "bridge.chosen",
                        input.BridgeInput?.Chosen ?? "stage_absent"),
                static input => FrameCompositeField(
                    "pipeline.rejected",
                    input.PipelineInput.RejectedResponse,
                    "persisted_pair.rejected",
                    input.PersistedPair.Rejected) + "|" +
                    FrameCompositeValue(
                        "bridge.rejected",
                        input.BridgeInput?.Rejected ?? "stage_absent"),
                static input => input.BridgeInput?.VectorBatch.TeachingVector,
                static input => input.BridgeInput?.VectorBatch.Weight,
                static input => string.Concat(
                    "design-conversion-composite-final-optimizer-v3|",
                    FrameCompositeValue(
                        "pipeline.child_batch_sha256",
                        input.PipelineChildBatchSha256),
                    "|pipeline.child_ordinal=",
                    input.PipelineOrdinal.ToString(
                        System.Globalization.CultureInfo.InvariantCulture),
                    "|",
                    FrameCompositeValue(
                        "pipeline.child_row_sha256",
                        input.PipelineChildRowSha256),
                    "|",
                    "bridge.stage=",
                    input.BridgeInput is null ? "absent" : "present",
                    "|",
                    FrameCompositeValue(
                        "bridge.child_batch_sha256",
                        input.BridgeChildBatchSha256),
                    "|bridge.child_ordinal=",
                    input.BridgeOrdinal?.ToString(
                        System.Globalization.CultureInfo.InvariantCulture) ??
                        "absent",
                    "|",
                    FrameCompositeValue(
                        "bridge.child_row_sha256",
                        input.BridgeChildRowSha256),
                    "|",
                    FrameCompositeValue(
                        "pipeline.item_id",
                        input.PipelineInput.ItemId),
                    "|",
                    FrameCompositeValue(
                        "persisted_pair.domain",
                        input.PersistedPair.Domain),
                    "|",
                    FrameCompositeValue(
                        "persisted_pair.captured_at_utc",
                        input.PersistedPair.CapturedAt.ToUniversalTime()
                            .ToString("O")),
                    "|",
                    FrameCompositeValue(
                        "persisted_pair.content_sha256",
                        input.PersistedPair.TeacherProvenance!
                            .TrainingContentSha256),
                    "|",
                    FramePersistedPairMetadata(input.PersistedPair),
                    "|",
                    FrameCompositeValue(
                        "bridge.record_identity_sha256",
                        input.BridgeInput?.RecordIdentitySha256 ??
                        "stage_absent"),
                    "|",
                    FrameCompositeValue(
                        "bridge.input_sha256",
                        input.BridgeInput?.InputSha256 ?? "stage_absent"),
                    "|",
                    FrameCompositeValue(
                        "bridge.execution_sha256",
                        input.BridgeInput is null
                            ? "stage_absent"
                            : ComputeEvaluatedBridgeExecutionSha256(
                                input.BridgeInput,
                                input.BridgeOrdinal ?? -1)),
                    "|bridge.float_weight_bits=",
                    input.BridgeInput is { } bridgeInput
                        ? BitConverter.SingleToInt32Bits(
                                bridgeInput.VectorBatch.Weight)
                            .ToString(
                                "x8",
                                System.Globalization.CultureInfo.InvariantCulture)
                        : "absent"),
                validateOptimizerSides: false);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FinalInputInvalid<DesignConversionCompositeOptimizerInput>();
        }
    }

    internal static string BuildEvaluatedBridgeFinalOptimizerContext(
        DesignConversionDpoEvaluatedPackedBatch input)
        => string.Concat(
            "design-conversion-evaluated-vector-final-v2|",
            FrameCompositeValue(
                "bridge.execution_sha256",
                ComputeEvaluatedBridgeExecutionSha256(input, ordinal: -1)));

    private static string ComputeEvaluatedBridgeExecutionSha256(
        DesignConversionDpoEvaluatedPackedBatch input,
        int ordinal)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendBridgeInt32(hash, ordinal);
        AppendBridgeString(hash, input.Prompt);
        AppendBridgeString(hash, input.Chosen);
        AppendBridgeString(hash, input.Rejected);
        AppendBridgeString(hash, input.RecordIdentitySha256);
        AppendBridgeString(hash, input.InputSha256);
        AppendBridgeTensor(hash, input.VectorBatch.Batch.ChosenIds);
        AppendBridgeTensor(hash, input.VectorBatch.Batch.ChosenMask);
        AppendBridgeTensor(hash, input.VectorBatch.Batch.RejectedIds);
        AppendBridgeTensor(hash, input.VectorBatch.Batch.RejectedMask);
        AppendBridgeInt32(hash, input.VectorBatch.TeachingVector.Length);
        foreach (var value in input.VectorBatch.TeachingVector)
            AppendBridgeInt32(hash, BitConverter.SingleToInt32Bits(value));
        AppendBridgeInt32(
            hash,
            BitConverter.SingleToInt32Bits(input.VectorBatch.Weight));

        var provenance = input.Provenance;
        AppendBridgeInt32(hash, provenance is null ? 0 : 1);
        if (provenance is not null)
        {
            AppendBridgeInt32(hash, (int)provenance.SourceKind);
            AppendBridgeInt32(hash, (int?)provenance.Family ?? -1);
            AppendBridgeInt32(hash, (int?)provenance.AccessMode ?? -1);
            AppendBridgeInt32(hash, (int?)provenance.HostingProvider ?? -1);
            AppendBridgeString(hash, provenance.ModelId);
            AppendBridgeString(hash, provenance.ModelVersion);
            AppendBridgeInt32(hash, (int?)provenance.ModelLicense ?? -1);
            AppendBridgeInt32(hash, provenance.ProviderTermsChecked ? 1 : 0);
            AppendBridgeInt32(
                hash,
                provenance.ProviderTermsAllowTraining ? 1 : 0);
            AppendBridgeInt32(hash, provenance.ClosedProprietaryModel ? 1 : 0);
            AppendBridgeInt32(
                hash,
                provenance.TeacherOutputUsedForTraining ? 1 : 0);
            AppendBridgeString(hash, provenance.RequestId);
            AppendBridgeInt32(hash, provenance.InvocationCount);
            AppendBridgeString(hash, provenance.ModelArtifactDigestKind);
            AppendBridgeString(hash, provenance.ModelArtifactDigest);
            AppendBridgeString(hash, provenance.TrainingContentSha256);
        }

        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void AppendBridgeTensor(IncrementalHash hash, Tensor tensor)
    {
        AppendBridgeInt32(hash, tensor.Shape.Rank);
        foreach (var dimension in tensor.Shape.Dimensions)
            AppendBridgeInt32(hash, dimension);
        AppendBridgeInt32(hash, tensor.Data.Length);
        foreach (var value in tensor.Data)
            AppendBridgeInt32(hash, BitConverter.SingleToInt32Bits(value));
    }

    private static void AppendBridgeString(IncrementalHash hash, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        AppendBridgeInt32(hash, bytes.Length);
        hash.AppendData(bytes);
    }

    private static void AppendBridgeInt32(IncrementalHash hash, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        hash.AppendData(bytes);
    }

    internal static bool IsCanonicalFinalOptimizer<T>(
        DesignConversionFinalOptimizerBatch<T>? optimizer)
        where T : class
    {
        if (optimizer is null || optimizer.Inputs.Count == 0 ||
            optimizer.Inputs.Count != optimizer.Provenance.Count ||
            optimizer.Inputs.Count != optimizer.Content.Rows.Count ||
            !IsSha256Hex(optimizer.Content.Sha256))
        {
            return false;
        }

        for (var index = 0; index < optimizer.Content.Rows.Count; index++)
        {
            if (optimizer.Inputs[index] is null ||
                optimizer.Provenance[index] is null ||
                !IsSha256Hex(optimizer.Content.Rows[index].Sha256))
            {
                return false;
            }
        }

        return true;
    }

    internal static bool MatchesExactPipelineFinalOptimizer(
        DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
            finalOptimizer)
    {
        if (!IsCanonicalFinalOptimizer(finalOptimizer))
            return false;

        var references = new HashSet<LearningTrainingItem>(
            ReferenceEqualityComparer.Instance);
        for (var index = 0;
             index < finalOptimizer!.Inputs.Count;
             index++)
        {
            var input = finalOptimizer.Inputs[index];
            var row = finalOptimizer.Content.Rows[index];
            var provenance = finalOptimizer.Provenance[index];
            if (!references.Add(input) ||
                !string.Equals(
                    input.Prompt,
                    row.Prompt,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    input.ChosenResponse,
                    row.Chosen,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    input.RejectedResponse,
                    row.Rejected,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    provenance!.TrainingContentSha256,
                    row.Sha256,
                    StringComparison.Ordinal) ||
                !DesignConversionDpoOptimizerSideContract.ValidatePair(
                    input.ChosenResponse,
                    input.RejectedResponse).IsSuccess)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsSha256Hex(string? value)
        => value is { Length: 64 } && value.All(static character =>
            character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static OperationResult<CompositeOptimizerCommitment>
        CompositeInvalid()
        => OperationResult<CompositeOptimizerCommitment>.Fail(
            "The exact DesignConversion composite optimizer commitment is invalid.",
            BoundedDpoTrainingContentProjector.InvalidErrorCode);

    internal sealed class CompositeOptimizerCommitment
    {
        private readonly ReadOnlyCollection<DpoPair> _persistedPairs;

        internal CompositeOptimizerCommitment(
            DesignConversionFinalOptimizerBatch<
                DesignConversionCompositeOptimizerInput> finalOptimizer,
            DesignConversionFinalOptimizerBatch<LearningTrainingItem>
                pipelineFinalOptimizer,
            DesignConversionFinalOptimizerBatch<
                DesignConversionDpoEvaluatedPackedBatch>?
                bridgeFinalOptimizer,
            DpoPair[] persistedPairs)
        {
            FinalOptimizer = finalOptimizer;
            PipelineFinalOptimizer = pipelineFinalOptimizer;
            BridgeFinalOptimizer = bridgeFinalOptimizer;
            _persistedPairs = Array.AsReadOnly(persistedPairs);
        }

        internal DesignConversionFinalOptimizerBatch<
            DesignConversionCompositeOptimizerInput> FinalOptimizer { get; }
        internal DesignConversionFinalOptimizerBatch<LearningTrainingItem>
            PipelineFinalOptimizer { get; }
        internal DesignConversionFinalOptimizerBatch<
            DesignConversionDpoEvaluatedPackedBatch>?
            BridgeFinalOptimizer { get; }

        internal bool MatchesExactChildren(
            DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
                pipelineFinalOptimizer,
            DesignConversionFinalOptimizerBatch<
                DesignConversionDpoEvaluatedPackedBatch>?
                bridgeFinalOptimizer)
        {
            if (!ReferenceEquals(
                    PipelineFinalOptimizer,
                    pipelineFinalOptimizer) ||
                !ReferenceEquals(
                    BridgeFinalOptimizer,
                    bridgeFinalOptimizer) ||
                !IsCanonicalFinalOptimizer(FinalOptimizer) ||
                !IsCanonicalFinalOptimizer(PipelineFinalOptimizer) ||
                BridgeFinalOptimizer is not null &&
                !IsCanonicalFinalOptimizer(BridgeFinalOptimizer) ||
                FinalOptimizer.Inputs.Count !=
                    PipelineFinalOptimizer.Inputs.Count ||
                _persistedPairs.Count != PipelineFinalOptimizer.Inputs.Count)
            {
                return false;
            }

            var reprojection = ProjectCompositeOptimizerInputsCore(
                PipelineFinalOptimizer.Inputs,
                PipelineFinalOptimizer.Provenance,
                _persistedPairs,
                BridgeFinalOptimizer?.Inputs ?? [],
                PipelineFinalOptimizer,
                BridgeFinalOptimizer);
            if (!reprojection.IsSuccess || reprojection.Value is null ||
                !string.Equals(
                    reprojection.Value.Content.Sha256,
                    FinalOptimizer.Content.Sha256,
                    StringComparison.Ordinal) ||
                reprojection.Value.Content.Rows.Count !=
                    FinalOptimizer.Content.Rows.Count)
            {
                return false;
            }
            for (var index = 0;
                 index < FinalOptimizer.Content.Rows.Count;
                 index++)
            {
                if (!string.Equals(
                        reprojection.Value.Content.Rows[index].Sha256,
                        FinalOptimizer.Content.Rows[index].Sha256,
                        StringComparison.Ordinal))
                {
                    return false;
                }
            }

            var bridgeByRequestId = new Dictionary<string,
                (DesignConversionDpoEvaluatedPackedBatch Input, int Ordinal)>(
                    StringComparer.Ordinal);
            if (BridgeFinalOptimizer is not null)
            {
                for (var index = 0;
                     index < BridgeFinalOptimizer.Inputs.Count;
                     index++)
                {
                    var bridge = BridgeFinalOptimizer.Inputs[index];
                    if (bridge.Provenance is not { } provenance ||
                        string.IsNullOrWhiteSpace(provenance.RequestId) ||
                        !bridgeByRequestId.TryAdd(
                            provenance.RequestId,
                            (bridge, index)))
                    {
                        return false;
                    }
                }
            }

            var matchedBridgeCount = 0;
            for (var index = 0;
                 index < PipelineFinalOptimizer.Inputs.Count;
                 index++)
            {
                var pipeline = PipelineFinalOptimizer.Inputs[index];
                var pipelineRow = PipelineFinalOptimizer.Content.Rows[index];
                var composite = FinalOptimizer.Inputs[index];
                var parentRow = FinalOptimizer.Content.Rows[index];
                var pair = _persistedPairs[index];
                if (!ReferenceEquals(composite.PipelineInput, pipeline) ||
                    !ReferenceEquals(composite.PersistedPair, pair) ||
                    composite.PipelineOrdinal != index ||
                    !string.Equals(
                        composite.PipelineChildBatchSha256,
                        PipelineFinalOptimizer.Content.Sha256,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        composite.PipelineChildRowSha256,
                        pipelineRow.Sha256,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        pipeline.Prompt,
                        pipelineRow.Prompt,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        pipeline.ChosenResponse,
                        pipelineRow.Chosen,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        pipeline.RejectedResponse,
                        pipelineRow.Rejected,
                        StringComparison.Ordinal) ||
                    !parentRow.Context.Contains(
                        FrameCompositeValue(
                            "pipeline.child_batch_sha256",
                            PipelineFinalOptimizer.Content.Sha256),
                        StringComparison.Ordinal) ||
                    !parentRow.Context.Contains(
                        FrameCompositeValue(
                            "pipeline.child_row_sha256",
                            pipelineRow.Sha256),
                        StringComparison.Ordinal))
                {
                    return false;
                }

                var sideContract =
                    DesignConversionDpoOptimizerSideContract.ValidatePair(
                        pipeline.ChosenResponse,
                        pipeline.RejectedResponse);
                if (!sideContract.IsSuccess ||
                    pair.TeacherProvenance is not { } pairTeacher ||
                    PipelineFinalOptimizer.Provenance[index] is not
                        { } pipelineTeacher ||
                    !string.Equals(
                        pairTeacher.RequestId,
                        pipelineTeacher.RequestId,
                        StringComparison.Ordinal))
                {
                    return false;
                }

                bridgeByRequestId.TryGetValue(
                    pipelineTeacher.RequestId,
                    out var expectedBridge);
                if (expectedBridge.Input is null)
                {
                    if (composite.BridgeInput is not null ||
                        composite.BridgeOrdinal is not null ||
                        !string.Equals(
                            composite.BridgeChildBatchSha256,
                            "stage_absent",
                            StringComparison.Ordinal) ||
                        !string.Equals(
                            composite.BridgeChildRowSha256,
                            "stage_absent",
                            StringComparison.Ordinal))
                    {
                        return false;
                    }
                }
                else
                {
                    matchedBridgeCount++;
                    var bridgeRow = BridgeFinalOptimizer!.Content
                        .Rows[expectedBridge.Ordinal];
                    if (!ReferenceEquals(
                            composite.BridgeInput,
                            expectedBridge.Input) ||
                        composite.BridgeOrdinal != expectedBridge.Ordinal ||
                        !string.Equals(
                            composite.BridgeChildBatchSha256,
                            BridgeFinalOptimizer.Content.Sha256,
                            StringComparison.Ordinal) ||
                        !string.Equals(
                            composite.BridgeChildRowSha256,
                            bridgeRow.Sha256,
                            StringComparison.Ordinal) ||
                        !string.Equals(
                            expectedBridge.Input.Prompt,
                            bridgeRow.Prompt,
                            StringComparison.Ordinal) ||
                        !string.Equals(
                            expectedBridge.Input.Chosen,
                            bridgeRow.Chosen,
                            StringComparison.Ordinal) ||
                        !string.Equals(
                            expectedBridge.Input.Rejected,
                            bridgeRow.Rejected,
                            StringComparison.Ordinal))
                    {
                        return false;
                    }
                }
            }

            return matchedBridgeCount == bridgeByRequestId.Count;
        }
    }

    private static string FrameCompositeField(
        string firstLabel,
        string first,
        string secondLabel,
        string second)
        => string.Concat(
            FrameCompositeValue(firstLabel, first),
            "|",
            FrameCompositeValue(secondLabel, second));

    private static string FramePersistedPairMetadata(DpoPair pair)
    {
        var teacher = pair.TeacherProvenance!;
        var governance = pair.GovernanceDecision!;
        var fields = new List<string>(24 + governance.EvidenceRefs.Count)
        {
            FrameCompositeValue(
                "persisted_pair.teacher.source_kind",
                ((int)teacher.SourceKind).ToString(
                    System.Globalization.CultureInfo.InvariantCulture)),
            FrameNullableEnum(
                "persisted_pair.teacher.family",
                teacher.Family),
            FrameNullableEnum(
                "persisted_pair.teacher.access_mode",
                teacher.AccessMode),
            FrameNullableEnum(
                "persisted_pair.teacher.hosting_provider",
                teacher.HostingProvider),
            FrameCompositeValue(
                "persisted_pair.teacher.model_id",
                teacher.ModelId),
            FrameCompositeValue(
                "persisted_pair.teacher.model_version",
                teacher.ModelVersion),
            FrameNullableEnum(
                "persisted_pair.teacher.model_license",
                teacher.ModelLicense),
            FrameCompositeValue(
                "persisted_pair.teacher.provider_terms_checked",
                teacher.ProviderTermsChecked ? "1" : "0"),
            FrameCompositeValue(
                "persisted_pair.teacher.provider_terms_allow_training",
                teacher.ProviderTermsAllowTraining ? "1" : "0"),
            FrameCompositeValue(
                "persisted_pair.teacher.closed_proprietary_model",
                teacher.ClosedProprietaryModel ? "1" : "0"),
            FrameCompositeValue(
                "persisted_pair.teacher.output_used_for_training",
                teacher.TeacherOutputUsedForTraining ? "1" : "0"),
            FrameCompositeValue(
                "persisted_pair.teacher.request_id",
                teacher.RequestId),
            FrameCompositeValue(
                "persisted_pair.teacher.invocation_count",
                teacher.InvocationCount.ToString(
                    System.Globalization.CultureInfo.InvariantCulture)),
            FrameCompositeValue(
                "persisted_pair.teacher.artifact_digest_kind",
                teacher.ModelArtifactDigestKind),
            FrameCompositeValue(
                "persisted_pair.teacher.artifact_digest",
                teacher.ModelArtifactDigest),
            FrameCompositeValue(
                "persisted_pair.teacher.training_content_sha256",
                teacher.TrainingContentSha256),
            FrameCompositeValue(
                "persisted_pair.governance.license_accepted",
                governance.LicenseAccepted ? "1" : "0"),
            FrameCompositeValue(
                "persisted_pair.governance.is_quarantined",
                governance.IsQuarantined ? "1" : "0"),
            FrameCompositeValue(
                "persisted_pair.governance.evidence_count",
                governance.EvidenceRefs.Count.ToString(
                    System.Globalization.CultureInfo.InvariantCulture))
        };
        for (var index = 0; index < governance.EvidenceRefs.Count; index++)
        {
            fields.Add(FrameCompositeValue(
                string.Concat(
                    "persisted_pair.governance.evidence[",
                    index.ToString(
                        System.Globalization.CultureInfo.InvariantCulture),
                    "]"),
                governance.EvidenceRefs[index]));
        }
        return string.Join('|', fields);
    }

    private static string FrameNullableEnum<TEnum>(
        string label,
        TEnum? value)
        where TEnum : struct, Enum
        => FrameCompositeValue(
            label,
            value.HasValue
                ? Convert.ToInt32(
                        value.Value,
                        System.Globalization.CultureInfo.InvariantCulture)
                    .ToString(
                        System.Globalization.CultureInfo.InvariantCulture)
                : "null");

    private static string FrameCompositeValue(string label, string value)
        => string.Concat(
            label,
            ":utf8=",
            System.Text.Encoding.UTF8.GetByteCount(value)
                .ToString(System.Globalization.CultureInfo.InvariantCulture),
            ":",
            value);

    internal static OperationResult<DpoTrainingContentBatch> Project(
        IReadOnlyList<DesignConversionDpoTrainingRecord>? records)
    {
        if (records is null ||
            records.Count is <= 0 or >
                BoundedDpoTrainingContentProjector.MaximumRowCount)
        {
            return InvalidProjection();
        }

        try
        {
            var inputs = new DpoTrainingContentRowInput[records.Count];
            for (var index = 0; index < records.Count; index++)
            {
                var record = records[index];
                if (!ValidateBeforeSnapshot(record))
                    return InvalidProjection();
                var item = DesignConversionTrainingPipeline
                    .ToCanonicalTrainingItem(record);
                var context = string.Concat(
                    CanonicalContext,
                    "|record=",
                    record.RecordId,
                    "|unit=",
                    record.UnitId);
                inputs[index] = new DpoTrainingContentRowInput(
                    item.Prompt,
                    item.ChosenResponse,
                    item.RejectedResponse,
                    record.InternalVectorPayload,
                    context);
            }
            return BoundedDpoTrainingContentProjector.Project(inputs);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidProjection();
        }
    }

    internal static OperationResult<
        DesignConversionDualTargetTeacherAggregateAdmission>
        AcquireDualTargetTeacherAggregateAdmission(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority?
                legalTeacherAdmissionAuthority,
            IReadOnlyList<DesignConversionDpoTrainingRecord>? records,
            IReadOnlyList<DesignConversionTrainingBatch>? preparedBatches,
            DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
                finalOptimizer)
    {
        if (legalTeacherAdmissionAuthority is null)
            return DualTargetTeacherInvalid<
                DesignConversionDualTargetTeacherAggregateAdmission>();

        var commitment = ProjectDualTargetTeacherAggregate(
            records,
            preparedBatches,
            finalOptimizer);
        if (!commitment.IsSuccess || commitment.Value is null)
        {
            return commitment.MapError<
                DesignConversionDualTargetTeacherAggregateAdmission>();
        }

        var lease = legalTeacherAdmissionAuthority.AcquireTrainingLease(
            commitment.Value.ParentContent,
            [commitment.Value.ParentProvenance]);
        if (!lease.IsSuccess || lease.Value is null)
        {
            return lease.MapError<
                DesignConversionDualTargetTeacherAggregateAdmission>();
        }

        var binding = DesignConversionPipelineTrainingBinding
            .MintDualTargetTeacherAggregate(
                commitment.Value,
                finalOptimizer,
                lease.Value);
        if (!binding.IsSuccess || binding.Value is null)
        {
            lease.Value.Dispose();
            return binding.MapError<
                DesignConversionDualTargetTeacherAggregateAdmission>();
        }

        return OperationResult<
            DesignConversionDualTargetTeacherAggregateAdmission>.Ok(
                new DesignConversionDualTargetTeacherAggregateAdmission(
                    binding.Value,
                    lease.Value,
                    commitment.Value));
    }

    internal static OperationResult<
        DesignConversionDualTargetTeacherExternalConsumptionScope>
        AcquireDualTargetTeacherExternalConsumptionScope(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority?
                legalTeacherAdmissionAuthority,
            IReadOnlyList<DesignConversionDpoTrainingRecord>? records,
            IReadOnlyList<DesignConversionTrainingBatch>? preparedBatches,
            DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
                finalOptimizer,
            FileDpoPairStore.ExternalConsumptionTransactionHandle?
                externalTransaction)
    {
        if (externalTransaction is null)
        {
            return DualTargetTeacherInvalid<
                DesignConversionDualTargetTeacherExternalConsumptionScope>();
        }

        var projected = ProjectDualTargetTeacherAggregate(
            records,
            preparedBatches,
            finalOptimizer);
        if (!projected.IsSuccess || projected.Value is null)
        {
            return projected.MapError<
                DesignConversionDualTargetTeacherExternalConsumptionScope>();
        }

        var externalGroup = projected.Value.ProjectExternalAtomicGroup();
        if (!externalGroup.IsSuccess || externalGroup.Value is null)
        {
            return externalGroup.MapError<
                DesignConversionDualTargetTeacherExternalConsumptionScope>();
        }
        var expectedGroup =
            DesignConversionExpectedExternalGroupIdentity.Create(
                externalGroup.Value);
        if (!expectedGroup.IsSuccess || expectedGroup.Value is null)
        {
            return expectedGroup.MapError<
                DesignConversionDualTargetTeacherExternalConsumptionScope>();
        }

        var beforeAdmission = externalTransaction.Read();
        if (!beforeAdmission.IsSuccess || beforeAdmission.Value is null)
        {
            return beforeAdmission.MapError<
                DesignConversionDualTargetTeacherExternalConsumptionScope>();
        }
        if (beforeAdmission.Value.State is
                FileDpoPairStore.DpoExternalConsumptionState.MutationStarted or
                FileDpoPairStore.DpoExternalConsumptionState.ExternalArtifactReady or
                FileDpoPairStore.DpoExternalConsumptionState.Committed)
        {
            return OperationResult<
                DesignConversionDualTargetTeacherExternalConsumptionScope>.Fail(
                    "The external DPO transaction requires recovery and cannot be retrained.",
                    "DPO_TRANSACTION_RECOVERY_REQUIRED");
        }
        if (!expectedGroup.Value.Matches(
                beforeAdmission.Value,
                allowReserved: true,
                allowAggregateBound: true))
        {
            return DualTargetTeacherInvalid<
                DesignConversionDualTargetTeacherExternalConsumptionScope>();
        }

        var aggregate = AcquireDualTargetTeacherAggregateAdmission(
            legalTeacherAdmissionAuthority,
            records,
            preparedBatches,
            finalOptimizer);
        if (!aggregate.IsSuccess || aggregate.Value is null)
        {
            return aggregate.MapError<
                DesignConversionDualTargetTeacherExternalConsumptionScope>();
        }

        var admission = aggregate.Value;
        var bound = externalTransaction.BindAggregate(
            admission.Commitment.ParentContent,
            admission.Commitment.ParentProvenance);
        if (!bound.IsSuccess)
        {
            admission.Dispose();
            return bound.MapError<
                DesignConversionDualTargetTeacherExternalConsumptionScope>();
        }

        var snapshot = externalTransaction.Read();
        if (!snapshot.IsSuccess || snapshot.Value is null ||
            !DesignConversionDualTargetTeacherExternalConsumptionScope
                .MatchesBoundAggregate(
                    snapshot.Value,
                    admission.Commitment,
                    expectedGroup.Value))
        {
            admission.Dispose();
            return snapshot.IsSuccess
                ? DualTargetTeacherInvalid<
                    DesignConversionDualTargetTeacherExternalConsumptionScope>()
                : snapshot.MapError<
                    DesignConversionDualTargetTeacherExternalConsumptionScope>();
        }

        return DesignConversionDualTargetTeacherExternalConsumptionScope.Create(
            admission,
            externalTransaction,
            snapshot.Value,
            expectedGroup.Value);
    }

    internal static OperationResult<DualTargetTeacherAggregateCommitment>
        ProjectDualTargetTeacherAggregate(
            IReadOnlyList<DesignConversionDpoTrainingRecord>? records,
            IReadOnlyList<DesignConversionTrainingBatch>? preparedBatches,
            DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
                finalOptimizer)
    {
        if (records is null || preparedBatches is null ||
            finalOptimizer is null || records.Count < 2 ||
            preparedBatches.Count != 2 || finalOptimizer.Inputs.Count != 2)
        {
            return DualTargetTeacherInvalid<
                DualTargetTeacherAggregateCommitment>();
        }

        try
        {
            var exactRecords = new DesignConversionDpoTrainingRecord[2];
            for (var index = 0; index < 2; index++)
            {
                var batch = preparedBatches[index];
                if (batch is null || batch.Records.Count != 1 ||
                    batch.TrainingItems.Count != 1 ||
                    !ReferenceEquals(
                        batch.TrainingItems[0],
                        finalOptimizer.Inputs[index]) ||
                    !records.Any(record =>
                        ReferenceEquals(record, batch.Records[0])))
                {
                    return DualTargetTeacherInvalid<
                        DualTargetTeacherAggregateCommitment>();
                }
                exactRecords[index] = batch.Records[0];
            }

            var aggregateRequestId = exactRecords[0].TeacherProvenance?.RequestId;
            var aggregateAuthoringOutput = ReadRequiredMetadata(
                exactRecords[0],
                "teacher_authoring_output_hash");
            if (string.IsNullOrWhiteSpace(aggregateRequestId) ||
                string.IsNullOrWhiteSpace(aggregateAuthoringOutput) ||
                records.Count(record =>
                    string.Equals(
                        record?.TeacherProvenance?.RequestId,
                        aggregateRequestId,
                        StringComparison.Ordinal)) != 2 ||
                records.Count(record =>
                    IsTeacherAuthoredAggregateChild(record) &&
                    string.Equals(
                        ReadRequiredMetadata(
                            record,
                            "teacher_authoring_output_hash"),
                        aggregateAuthoringOutput,
                        StringComparison.Ordinal)) != 2)
            {
                return DualTargetTeacherInvalid<
                    DualTargetTeacherAggregateCommitment>();
            }
            var projection = BuildDualTargetTeacherAggregateProjection(
                exactRecords,
                finalOptimizer);
            if (!projection.IsSuccess || projection.Value is null)
            {
                return projection.MapError<
                    DualTargetTeacherAggregateCommitment>();
            }

            return OperationResult<DualTargetTeacherAggregateCommitment>.Ok(
                new DualTargetTeacherAggregateCommitment(
                    exactRecords,
                    finalOptimizer,
                    projection.Value.ParentContent,
                    projection.Value.ParentProvenance));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return DualTargetTeacherInvalid<
                DualTargetTeacherAggregateCommitment>();
        }
    }

    private static OperationResult<DualTargetTeacherAggregateProjection>
        BuildDualTargetTeacherAggregateProjection(
            IReadOnlyList<DesignConversionDpoTrainingRecord> records,
            DesignConversionFinalOptimizerBatch<LearningTrainingItem>
                finalOptimizer)
    {
        if (records.Count != 2 || finalOptimizer.Inputs.Count != 2 ||
            finalOptimizer.Provenance.Count != 2 ||
            finalOptimizer.Content.Rows.Count != 2 ||
            !MatchesExactPipelineFinalOptimizer(finalOptimizer))
        {
            return DualTargetTeacherInvalid<
                DualTargetTeacherAggregateProjection>();
        }

        var recordProjection = Project(records);
        if (!recordProjection.IsSuccess || recordProjection.Value is null ||
            recordProjection.Value.Rows.Count != 2)
        {
            return DualTargetTeacherInvalid<
                DualTargetTeacherAggregateProjection>();
        }

        var firstTeacher = records[0].TeacherProvenance;
        if (firstTeacher is null ||
            firstTeacher.SourceKind !=
                TrainingTeacherSourceKind.ExternalTeacherOutput ||
            firstTeacher.InvocationCount != 1 ||
            string.IsNullOrWhiteSpace(firstTeacher.RequestId))
        {
            return DualTargetTeacherInvalid<
                DualTargetTeacherAggregateProjection>();
        }

        var contextFields = new List<string>(96)
        {
            FrameCompositeValue(
                "aggregate.context_version",
                DualTargetTeacherAggregateContextVersion),
            FrameCompositeValue("aggregate.child_count", "2"),
            FrameCompositeValue(
                "aggregate.teacher_request_sha256",
                Sha256RefForAggregate(firstTeacher.RequestId)),
            FrameCompositeValue(
                "aggregate.teacher_identity_sha256",
                HashTeacherIdentityForAggregate(firstTeacher))
        };

        for (var index = 0; index < 2; index++)
        {
            var record = records[index];
            var item = finalOptimizer.Inputs[index];
            var optimizerRow = finalOptimizer.Content.Rows[index];
            var recordRow = recordProjection.Value.Rows[index];
            var teacher = record.TeacherProvenance;
            var optimizerTeacher = finalOptimizer.Provenance[index];
            var expectedUnit = DualTargetTeacherUnitOrder[index];
            if (record is null || item is null || teacher is null ||
                optimizerTeacher is null ||
                !string.Equals(record.UnitId, expectedUnit,
                    StringComparison.Ordinal) ||
                !IsCanonicalSha256Reference(record.RecordId) ||
                !string.Equals(item.ItemId, record.RecordId,
                    StringComparison.Ordinal) ||
                !TeacherIdentityEquals(firstTeacher, teacher) ||
                !TeacherIdentityEquals(firstTeacher, optimizerTeacher) ||
                teacher.InvocationCount != 1 ||
                optimizerTeacher.InvocationCount != 1 ||
                !HasSameTeacherAuthoredRecordSource(records[0], record) ||
                !HasSameTeacherAttestation(records[0], record))
            {
                return DualTargetTeacherInvalid<
                    DualTargetTeacherAggregateProjection>();
            }

            var canonical = DesignConversionTrainingPipeline
                .ToCanonicalTrainingItem(record);
            var custody = JsonlDesignConversionLocalArtifactDb
                .ValidateAndMaterializePayloadCustody(record);
            if (!custody.IsSuccess || custody.Value is null ||
                record.PayloadCustodySecret is not { } payload ||
                !IsCanonicalSha256Reference(payload.PromptSha256) ||
                !IsCanonicalSha256Reference(payload.ChosenResponseSha256) ||
                !IsCanonicalSha256Reference(payload.RejectedResponseSha256) ||
                !IsCanonicalSha256Reference(payload.GovernanceSha256) ||
                !IsCanonicalSha256Reference(
                    payload.TrainingRecordBindingSha256) ||
                !string.Equals(canonical.ItemId, item.ItemId,
                    StringComparison.Ordinal) ||
                !string.Equals(canonical.Prompt, item.Prompt,
                    StringComparison.Ordinal) ||
                !string.Equals(canonical.ChosenResponse,
                    item.ChosenResponse, StringComparison.Ordinal) ||
                !string.Equals(canonical.RejectedResponse,
                    item.RejectedResponse, StringComparison.Ordinal) ||
                !string.Equals(item.Prompt, optimizerRow.Prompt,
                    StringComparison.Ordinal) ||
                !string.Equals(item.ChosenResponse, optimizerRow.Chosen,
                    StringComparison.Ordinal) ||
                !string.Equals(item.RejectedResponse, optimizerRow.Rejected,
                    StringComparison.Ordinal) ||
                !string.Equals(optimizerTeacher.TrainingContentSha256,
                    optimizerRow.Sha256, StringComparison.Ordinal))
            {
                return DualTargetTeacherInvalid<
                    DualTargetTeacherAggregateProjection>();
            }

            var prefix = string.Concat(
                "aggregate.child[",
                index.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
                "].");
            contextFields.Add(FrameCompositeValue(prefix + "target", expectedUnit));
            contextFields.Add(FrameCompositeValue(
                prefix + "record_id_sha256",
                Sha256RefForAggregate(record.RecordId)));
            contextFields.Add(FrameCompositeValue(
                prefix + "record_batch_sha256",
                recordProjection.Value.Sha256));
            contextFields.Add(FrameCompositeValue(
                prefix + "record_row_sha256",
                recordRow.Sha256));
            contextFields.Add(FrameCompositeValue(
                prefix + "optimizer_batch_sha256",
                finalOptimizer.Content.Sha256));
            contextFields.Add(FrameCompositeValue(
                prefix + "optimizer_row_sha256",
                optimizerRow.Sha256));
            contextFields.Add(FrameCompositeValue(
                prefix + "prompt_sha256",
                Sha256RefForAggregate(item.Prompt)));
            contextFields.Add(FrameCompositeValue(
                prefix + "chosen_sha256",
                Sha256RefForAggregate(item.ChosenResponse)));
            contextFields.Add(FrameCompositeValue(
                prefix + "rejected_sha256",
                Sha256RefForAggregate(item.RejectedResponse)));
            contextFields.Add(FrameCompositeValue(
                prefix + "custody_prompt_sha256",
                payload.PromptSha256));
            contextFields.Add(FrameCompositeValue(
                prefix + "custody_chosen_sha256",
                payload.ChosenResponseSha256));
            contextFields.Add(FrameCompositeValue(
                prefix + "custody_rejected_sha256",
                payload.RejectedResponseSha256));
            contextFields.Add(FrameCompositeValue(
                prefix + "custody_governance_sha256",
                payload.GovernanceSha256));
            contextFields.Add(FrameCompositeValue(
                prefix + "custody_binding_sha256",
                payload.TrainingRecordBindingSha256));
        }

        if (ReferenceEquals(records[0], records[1]) ||
            ReferenceEquals(finalOptimizer.Inputs[0],
                finalOptimizer.Inputs[1]) ||
            string.Equals(records[0].RecordId, records[1].RecordId,
                StringComparison.Ordinal) ||
            !string.Equals(finalOptimizer.Inputs[0].ChosenResponse,
                finalOptimizer.Inputs[1].ChosenResponse,
                StringComparison.Ordinal) ||
            !string.Equals(finalOptimizer.Inputs[0].RejectedResponse,
                finalOptimizer.Inputs[1].RejectedResponse,
                StringComparison.Ordinal))
        {
            return DualTargetTeacherInvalid<
                DualTargetTeacherAggregateProjection>();
        }

        foreach (var key in DualTargetTeacherAttestationKeys)
        {
            contextFields.Add(FrameCompositeValue(
                string.Concat("aggregate.attestation.", key, ".sha256"),
                Sha256RefForAggregate(ReadRequiredMetadata(records[0], key))));
        }

        var context = string.Join('|', contextFields);
        var aggregateIdentity = Sha256RefForAggregate(context);
        var parent = BoundedDpoTrainingContentProjector.Project(
        [
            new DpoTrainingContentRowInput(
                Prompt: Sha256RefForAggregate(
                    string.Concat("parent-prompt|", aggregateIdentity)),
                Chosen: Sha256RefForAggregate(
                    string.Concat("parent-critic|", recordProjection.Value.Rows[0].Sha256)),
                Rejected: Sha256RefForAggregate(
                    string.Concat("parent-improver|", recordProjection.Value.Rows[1].Sha256)),
                TeachingVector: null,
                Context: context)
        ]);
        if (!parent.IsSuccess || parent.Value is null ||
            parent.Value.Rows.Count != 1)
        {
            return DualTargetTeacherInvalid<
                DualTargetTeacherAggregateProjection>();
        }

        return OperationResult<DualTargetTeacherAggregateProjection>.Ok(
            new DualTargetTeacherAggregateProjection(
                parent.Value,
                firstTeacher with
                {
                    TrainingContentSha256 =
                        parent.Value.Rows[0].Sha256
                }));
    }

    private static bool HasSameTeacherAuthoredRecordSource(
        DesignConversionDpoTrainingRecord expected,
        DesignConversionDpoTrainingRecord actual)
        => string.Equals(expected.Provenance.SourceRequestId,
               actual.Provenance.SourceRequestId, StringComparison.Ordinal) &&
           string.Equals(expected.Provenance.SourceKind,
               actual.Provenance.SourceKind, StringComparison.Ordinal) &&
           string.Equals(expected.Provenance.SourceManifestRef,
               actual.Provenance.SourceManifestRef, StringComparison.Ordinal) &&
           string.Equals(expected.Provenance.ReviewerRef,
               actual.Provenance.ReviewerRef, StringComparison.Ordinal) &&
           expected.Provenance.CreatedAtUtc == actual.Provenance.CreatedAtUtc &&
           string.Equals(expected.License.LicenseId,
               actual.License.LicenseId, StringComparison.Ordinal) &&
           string.Equals(expected.License.RightsScope,
               actual.License.RightsScope, StringComparison.Ordinal) &&
           expected.License.AllowsTraining == actual.License.AllowsTraining &&
           expected.License.ContainsUserPrivateData ==
               actual.License.ContainsUserPrivateData;

    private static bool IsTeacherAuthoredAggregateChild(
        DesignConversionDpoTrainingRecord? record)
        => record is not null &&
           string.Equals(
               ReadRequiredMetadata(record, "runtime_teacher_authored_candidate"),
               bool.TrueString,
               StringComparison.OrdinalIgnoreCase);

    private static bool HasSameTeacherAttestation(
        DesignConversionDpoTrainingRecord expected,
        DesignConversionDpoTrainingRecord actual)
    {
        foreach (var key in DualTargetTeacherAttestationKeys)
        {
            var expectedValue = ReadRequiredMetadata(expected, key);
            var actualValue = ReadRequiredMetadata(actual, key);
            if (string.IsNullOrWhiteSpace(expectedValue) ||
                !string.Equals(expectedValue, actualValue,
                    StringComparison.Ordinal))
            {
                return false;
            }
        }
        return true;
    }

    private static string ReadRequiredMetadata(
        DesignConversionDpoTrainingRecord record,
        string key)
        => record.Provenance.Metadata is not null &&
           record.Provenance.Metadata.TryGetValue(key, out var value)
            ? value ?? string.Empty
            : string.Empty;

    private static bool TeacherIdentityEquals(
        TrainingTeacherProvenance expected,
        TrainingTeacherProvenance actual)
        => expected.SourceKind == actual.SourceKind &&
           expected.Family == actual.Family &&
           expected.AccessMode == actual.AccessMode &&
           expected.HostingProvider == actual.HostingProvider &&
           string.Equals(expected.ModelId, actual.ModelId,
               StringComparison.Ordinal) &&
           string.Equals(expected.ModelVersion, actual.ModelVersion,
               StringComparison.Ordinal) &&
           expected.ModelLicense == actual.ModelLicense &&
           expected.ProviderTermsChecked == actual.ProviderTermsChecked &&
           expected.ProviderTermsAllowTraining ==
               actual.ProviderTermsAllowTraining &&
           expected.ClosedProprietaryModel ==
               actual.ClosedProprietaryModel &&
           expected.TeacherOutputUsedForTraining ==
               actual.TeacherOutputUsedForTraining &&
           string.Equals(expected.RequestId, actual.RequestId,
               StringComparison.Ordinal) &&
           expected.InvocationCount == actual.InvocationCount &&
           string.Equals(expected.ModelArtifactDigestKind,
               actual.ModelArtifactDigestKind, StringComparison.Ordinal) &&
           string.Equals(expected.ModelArtifactDigest,
               actual.ModelArtifactDigest, StringComparison.Ordinal);

    private static string HashTeacherIdentityForAggregate(
        TrainingTeacherProvenance teacher)
        => Sha256RefForAggregate(string.Join('|',
        [
            FrameCompositeValue("source_kind", ((int)teacher.SourceKind).ToString(System.Globalization.CultureInfo.InvariantCulture)),
            FrameCompositeValue("family", ((int?)teacher.Family ?? -1).ToString(System.Globalization.CultureInfo.InvariantCulture)),
            FrameCompositeValue("access_mode", ((int?)teacher.AccessMode ?? -1).ToString(System.Globalization.CultureInfo.InvariantCulture)),
            FrameCompositeValue("hosting_provider", ((int?)teacher.HostingProvider ?? -1).ToString(System.Globalization.CultureInfo.InvariantCulture)),
            FrameCompositeValue("model_id", teacher.ModelId),
            FrameCompositeValue("model_version", teacher.ModelVersion),
            FrameCompositeValue("model_license", ((int?)teacher.ModelLicense ?? -1).ToString(System.Globalization.CultureInfo.InvariantCulture)),
            FrameCompositeValue("provider_terms_checked", teacher.ProviderTermsChecked ? "1" : "0"),
            FrameCompositeValue("provider_terms_allow_training", teacher.ProviderTermsAllowTraining ? "1" : "0"),
            FrameCompositeValue("closed_proprietary_model", teacher.ClosedProprietaryModel ? "1" : "0"),
            FrameCompositeValue("output_used_for_training", teacher.TeacherOutputUsedForTraining ? "1" : "0"),
            FrameCompositeValue("request_id", teacher.RequestId),
            FrameCompositeValue("invocation_count", teacher.InvocationCount.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            FrameCompositeValue("artifact_digest_kind", teacher.ModelArtifactDigestKind),
            FrameCompositeValue("artifact_digest", teacher.ModelArtifactDigest)
        ]));

    private static string Sha256RefForAggregate(string value)
        => string.Concat(
            "sha256:",
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
                .ToLowerInvariant());

    private static bool IsCanonicalSha256Reference(string? value)
        => value is { Length: 71 } &&
           value.StartsWith("sha256:", StringComparison.Ordinal) &&
           value.AsSpan(7).ToArray().All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static OperationResult<T> DualTargetTeacherInvalid<T>()
        => OperationResult<T>.Fail(
            "The exact atomic Critic and Improver teacher aggregate is invalid.",
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode);

    private sealed record DualTargetTeacherAggregateProjection(
        DpoTrainingContentBatch ParentContent,
        TrainingTeacherProvenance ParentProvenance);

    internal sealed class DualTargetTeacherAggregateCommitment
    {
        private readonly ReadOnlyCollection<DesignConversionDpoTrainingRecord>
            _records;
        private readonly DesignConversionFinalOptimizerBatch<LearningTrainingItem>
            _finalOptimizer;

        internal DualTargetTeacherAggregateCommitment(
            DesignConversionDpoTrainingRecord[] records,
            DesignConversionFinalOptimizerBatch<LearningTrainingItem>
                finalOptimizer,
            DpoTrainingContentBatch parentContent,
            TrainingTeacherProvenance parentProvenance)
        {
            _records = Array.AsReadOnly(records);
            _finalOptimizer = finalOptimizer;
            ParentContent = parentContent;
            ParentProvenance = parentProvenance;
        }

        internal DpoTrainingContentBatch ParentContent { get; }
        internal TrainingTeacherProvenance ParentProvenance { get; }
        internal DesignConversionFinalOptimizerBatch<LearningTrainingItem>
            FinalOptimizer => _finalOptimizer;

        internal bool MatchesExactChildren(
            DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
                finalOptimizer)
        {
            if (!ReferenceEquals(_finalOptimizer, finalOptimizer))
                return false;
            var projected = BuildDualTargetTeacherAggregateProjection(
                _records,
                _finalOptimizer);
            return projected.IsSuccess && projected.Value is not null &&
                   string.Equals(projected.Value.ParentContent.Sha256,
                       ParentContent.Sha256, StringComparison.Ordinal) &&
                   string.Equals(
                       projected.Value.ParentContent.Rows[0].Sha256,
                       ParentContent.Rows[0].Sha256,
                       StringComparison.Ordinal) &&
                   TeacherIdentityEquals(
                       ParentProvenance,
                       projected.Value.ParentProvenance) &&
                   string.Equals(
                       ParentProvenance.TrainingContentSha256,
                       projected.Value.ParentProvenance.TrainingContentSha256,
                       StringComparison.Ordinal);
        }

        internal OperationResult<LearningBatch<DesignConversionDomain>>
            BuildAtomicMachineBatch(string runId)
        {
            if (!MatchesExactChildren(_finalOptimizer))
            {
                return DualTargetTeacherInvalid<
                    LearningBatch<DesignConversionDomain>>();
            }
            // The admission boundary requires these exact committed references;
            // it immediately deep-copies them before the Machine consumes them.
            var items = _finalOptimizer.Inputs.ToArray();
            var sources = _records
                .Select(static record => record.Provenance.SourceManifestRef)
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            return OperationResult<LearningBatch<DesignConversionDomain>>.Ok(
                new LearningBatch<DesignConversionDomain>(
                    string.Concat(runId, ":dual-target-teacher-aggregate"),
                    DesignConversionMachine.DomainTag,
                    Array.AsReadOnly(items),
                    Array.AsReadOnly(sources)));
        }

        internal OperationResult<DpoPairAtomicGroupEnvelope>
            ProjectExternalAtomicGroup()
        {
            if (!MatchesExactChildren(_finalOptimizer) || _records.Count != 2 ||
                !string.Equals(_records[0].UnitId,
                    DesignConversionDpoUnits.CriticUnit,
                    StringComparison.Ordinal) ||
                !string.Equals(_records[1].UnitId,
                    DesignConversionDpoUnits.ImproverUnit,
                    StringComparison.Ordinal))
            {
                return DualTargetTeacherInvalid<
                    DpoPairAtomicGroupEnvelope>();
            }

            var critic = DesignConversionTrainingIntakeDpoPairAuthority
                .Project(_records[0]);
            if (!critic.IsSuccess || critic.Value is null)
                return critic.MapError<DpoPairAtomicGroupEnvelope>();
            var improver = DesignConversionTrainingIntakeDpoPairAuthority
                .Project(_records[1]);
            if (!improver.IsSuccess || improver.Value is null)
                return improver.MapError<DpoPairAtomicGroupEnvelope>();
            return DpoPairAtomicGroupEnvelope.Create(
                critic.Value,
                improver.Value);
        }
    }

    internal static bool ValidateBeforeSnapshot(
        DesignConversionDpoTrainingRecord? record)
    {
        if (record is null)
            return false;
        try
        {
            var budget = new SnapshotBudget();
            return AddText(budget, record.RecordId) &&
                   AddText(budget, record.UnitId) &&
                   AddText(budget, record.InternalUnitStatePayload) &&
                   AddVector(budget, record.InternalVectorPayload) &&
                   AddText(budget, record.SourceStructurePayload) &&
                   ValidateOutput(budget, record.Chosen) &&
                   ValidateOutput(budget, record.Rejected) &&
                   AddOptionalText(budget, record.TieBreakerPayload) &&
                   double.IsFinite(record.UsefulnessScore) &&
                   AddText(budget, record.NegativeQualityReason) &&
                   AddText(budget, record.NegativeReasonTaxonomy) &&
                   ValidateProvenance(budget, record.Provenance) &&
                   ValidateLicense(budget, record.License) &&
                   ValidateQuarantine(budget, record.Quarantine) &&
                   IsValidOptionalPairAuthorKind(record.ChosenAuthorKind) &&
                   IsValidOptionalPairAuthorKind(record.RejectedAuthorKind) &&
                   record.TeacherProvenance is not null;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool TryCaptureRecord(
        DesignConversionDpoTrainingRecord? record,
        SnapshotBudget budget,
        bool requireTeacherProvenance,
        out DesignConversionDpoTrainingRecord captured)
    {
        captured = null!;
        if (record is null ||
            !AddText(budget, record.RecordId) ||
            !AddText(budget, record.UnitId) ||
            !AddText(budget, record.InternalUnitStatePayload) ||
            !TryCaptureVector(
                record.InternalVectorPayload,
                budget,
                out var vector) ||
            !AddText(budget, record.SourceStructurePayload) ||
            !TryCaptureOutput(record.Chosen, budget, out var chosen) ||
            !TryCaptureOutput(record.Rejected, budget, out var rejected) ||
            !AddOptionalText(budget, record.TieBreakerPayload) ||
            !double.IsFinite(record.UsefulnessScore) ||
            !AddText(budget, record.NegativeQualityReason) ||
            !AddText(budget, record.NegativeReasonTaxonomy) ||
            !TryCaptureProvenance(
                record.Provenance,
                budget,
                out var sourceProvenance) ||
            !TryCaptureLicense(record.License, budget, out var license) ||
            !TryCaptureQuarantine(
                record.Quarantine,
                budget,
                out var quarantine) ||
            !IsValidOptionalPairAuthorKind(record.ChosenAuthorKind) ||
            !IsValidOptionalPairAuthorKind(record.RejectedAuthorKind) ||
            requireTeacherProvenance && record.TeacherProvenance is null ||
            !TryCaptureTeacherProvenance(
                record.TeacherProvenance,
                budget,
                out var teacherProvenance))
        {
            return false;
        }

        captured = new DesignConversionDpoTrainingRecord(
            record.RecordId,
            record.UnitId,
            record.InternalUnitStatePayload,
            vector,
            record.SourceStructurePayload,
            chosen,
            rejected,
            record.TieBreakerPayload,
            record.UsefulnessScore,
            record.NegativeQualityReason,
            record.NegativeReasonTaxonomy,
            sourceProvenance,
            license,
            quarantine,
            teacherProvenance)
        {
            // Author-kind поля копируются вместе со снимком: это юридический
            // паспорт пары, а не выводимое из токенов состояние.
            ChosenAuthorKind = record.ChosenAuthorKind,
            RejectedAuthorKind = record.RejectedAuthorKind,
            PayloadCustodySecret = record.PayloadCustodySecret
        };
        return true;
    }

    private static bool IsValidOptionalPairAuthorKind(
        DpoPairSideAuthorKind? authorKind)
        => authorKind is null ||
           Enum.IsDefined(typeof(DpoPairSideAuthorKind), authorKind.Value);

    private static bool TryCaptureVector(
        IReadOnlyList<double>? values,
        SnapshotBudget budget,
        out double[] captured)
    {
        captured = null!;
        if (values is null)
            return false;
        var count = values.Count;
        if (count > BoundedDpoTrainingContentProjector.MaximumVectorLength ||
            !budget.AddElements(count))
        {
            return false;
        }
        captured = new double[count];
        for (var index = 0; index < count; index++)
        {
            var value = values[index];
            if (!double.IsFinite(value))
                return false;
            captured[index] = value;
        }
        return true;
    }

    private static bool TryCaptureOutput(
        DesignConversionImprovementOutput? output,
        SnapshotBudget budget,
        out DesignConversionImprovementOutput captured)
    {
        captured = null!;
        if (output is null ||
            !AddText(budget, output.OutputId) ||
            !AddText(budget, output.CandidateId) ||
            !AddText(budget, output.Html) ||
            !AddText(budget, output.Css) ||
            !TryCaptureVerdict(output.Verdict, budget, out var verdict) ||
            !TryCaptureQaResults(output.QaResults, budget, out var qa) ||
            !TryCaptureStrings(output.EvidenceRefs, budget, out var evidence))
        {
            return false;
        }
        captured = output with
        {
            Verdict = verdict,
            QaResults = qa,
            EvidenceRefs = evidence
        };
        return true;
    }

    private static bool TryCaptureVerdict(
        VisualVerdict? verdict,
        SnapshotBudget budget,
        out VisualVerdict captured)
    {
        captured = null!;
        if (verdict is null ||
            !AddText(budget, verdict.VerdictId) ||
            !double.IsFinite(verdict.Score) ||
            !TryCaptureDefects(verdict.Defects, budget, out var defects) ||
            !TryCaptureStrings(
                verdict.EvidenceRefs,
                budget,
                out var evidence))
        {
            return false;
        }
        captured = verdict with
        {
            Defects = defects,
            EvidenceRefs = evidence
        };
        return true;
    }

    private static bool TryCaptureDefects(
        IReadOnlyList<VisualDefect>? defects,
        SnapshotBudget budget,
        out VisualDefect[] captured)
    {
        captured = null!;
        if (!TryBoundCount(defects, budget, out var count))
            return false;
        captured = new VisualDefect[count];
        for (var index = 0; index < count; index++)
        {
            var defect = defects![index];
            if (defect is null ||
                !AddText(budget, defect.DefectId) ||
                !AddText(budget, defect.Category) ||
                !AddText(budget, defect.Severity) ||
                !AddText(budget, defect.Description) ||
                !double.IsFinite(defect.X) ||
                !double.IsFinite(defect.Y) ||
                !double.IsFinite(defect.Width) ||
                !double.IsFinite(defect.Height) ||
                !TryCaptureStrings(
                    defect.EvidenceImageIds,
                    budget,
                    out var imageIds))
            {
                return false;
            }
            captured[index] = defect with
            {
                EvidenceImageIds = imageIds
            };
        }
        return true;
    }

    private static bool TryCaptureQaResults(
        IReadOnlyList<RenderQaResultRecord>? results,
        SnapshotBudget budget,
        out RenderQaResultRecord[] captured)
    {
        captured = null!;
        if (!TryBoundCount(results, budget, out var count))
            return false;
        captured = new RenderQaResultRecord[count];
        for (var index = 0; index < count; index++)
        {
            var result = results![index];
            if (result is null ||
                !AddText(budget, result.ResultId) ||
                !AddText(budget, result.RequestId) ||
                !TryCaptureImage(result.Expected, budget, out var expected) ||
                !TryCaptureImage(result.Actual, budget, out var actual) ||
                !TryCaptureVerdict(
                    result.Verdict,
                    budget,
                    out var verdict) ||
                !TryCaptureDoubleDictionary(
                    result.Metrics,
                    budget,
                    out var metrics) ||
                !TryCaptureStrings(
                    result.Warnings,
                    budget,
                    out var warnings))
            {
                return false;
            }
            captured[index] = result with
            {
                Expected = expected,
                Actual = actual,
                Verdict = verdict,
                Metrics = metrics,
                Warnings = warnings
            };
        }
        return true;
    }

    private static bool TryCaptureImage(
        RenderQaImageRecord? image,
        SnapshotBudget budget,
        out RenderQaImageRecord captured)
    {
        captured = null!;
        if (image is null || image.Resolution is null ||
            !AddText(budget, image.ImageId) ||
            !AddText(budget, image.Label) ||
            !AddText(budget, image.Resolution.ResolutionId) ||
            image.Resolution.Width < 0 || image.Resolution.Height < 0 ||
            !double.IsFinite(image.Resolution.DeviceScaleFactor) ||
            !AddText(budget, image.Uri) ||
            !AddText(budget, image.Sha256) ||
            !TryCaptureStringDictionary(
                image.Metadata,
                budget,
                out var metadata))
        {
            return false;
        }
        captured = image with
        {
            Resolution = image.Resolution with { },
            Metadata = metadata
        };
        return true;
    }

    private static bool TryCaptureProvenance(
        DesignConversionDpoProvenance? provenance,
        SnapshotBudget budget,
        out DesignConversionDpoProvenance captured)
    {
        captured = null!;
        if (provenance is null ||
            !AddText(budget, provenance.SourceRequestId) ||
            !AddText(budget, provenance.SourceKind) ||
            !AddText(budget, provenance.SourceManifestRef) ||
            !AddText(budget, provenance.ReviewerRef) ||
            !ValidateTimestamp(provenance.CreatedAtUtc) ||
            !TryCaptureStringDictionary(
                provenance.Metadata,
                budget,
                out var metadata))
        {
            return false;
        }
        captured = provenance with { Metadata = metadata };
        return true;
    }

    private static bool TryCaptureLicense(
        DesignConversionLicenseScope? license,
        SnapshotBudget budget,
        out DesignConversionLicenseScope captured)
    {
        captured = null!;
        if (license is null ||
            !AddText(budget, license.LicenseId) ||
            !AddText(budget, license.RightsScope) ||
            !TryCaptureStringDictionary(
                license.Metadata,
                budget,
                out var metadata))
        {
            return false;
        }
        captured = license with { Metadata = metadata };
        return true;
    }

    private static bool TryCaptureQuarantine(
        DesignConversionQuarantineState? quarantine,
        SnapshotBudget budget,
        out DesignConversionQuarantineState captured)
    {
        captured = null!;
        if (quarantine is null ||
            !AddText(budget, quarantine.Reason) ||
            !TryCaptureStrings(
                quarantine.EvidenceRefs,
                budget,
                out var evidence))
        {
            return false;
        }
        captured = quarantine with { EvidenceRefs = evidence };
        return true;
    }

    private static bool TryCaptureTeacherProvenance(
        TrainingTeacherProvenance? provenance,
        SnapshotBudget budget,
        out TrainingTeacherProvenance? captured)
    {
        captured = null;
        if (provenance is null)
            return true;
        if (!AddText(budget, provenance.ModelId) ||
            !AddText(budget, provenance.ModelVersion) ||
            !AddText(budget, provenance.ModelArtifactDigestKind) ||
            !AddText(budget, provenance.ModelArtifactDigest) ||
            !AddText(budget, provenance.TrainingContentSha256) ||
            !AddText(budget, provenance.RequestId))
        {
            return false;
        }
        captured = provenance with { };
        return true;
    }

    private static bool TryCaptureStrings(
        IReadOnlyList<string>? values,
        SnapshotBudget budget,
        out string[] captured)
    {
        captured = null!;
        if (!TryBoundCount(values, budget, out var count))
            return false;
        captured = new string[count];
        for (var index = 0; index < count; index++)
        {
            var value = values![index];
            if (!AddText(budget, value))
                return false;
            captured[index] = value;
        }
        return true;
    }

    private static bool TryCaptureStringDictionary(
        IReadOnlyDictionary<string, string>? values,
        SnapshotBudget budget,
        out IReadOnlyDictionary<string, string> captured)
    {
        captured = null!;
        if (values is null)
            return false;
        var owned = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in values)
        {
            if (owned.Count == MaximumMetadataCount ||
                !budget.AddElements(1) ||
                !AddText(budget, pair.Key) ||
                !AddText(budget, pair.Value) ||
                !owned.TryAdd(pair.Key, pair.Value))
            {
                return false;
            }
        }
        captured = new ReadOnlyDictionary<string, string>(owned);
        return true;
    }

    private static bool TryCaptureDoubleDictionary(
        IReadOnlyDictionary<string, double>? values,
        SnapshotBudget budget,
        out IReadOnlyDictionary<string, double> captured)
    {
        captured = null!;
        if (values is null)
            return false;
        var owned = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var pair in values)
        {
            if (owned.Count == MaximumMetadataCount ||
                !budget.AddElements(1) ||
                !AddText(budget, pair.Key) ||
                !double.IsFinite(pair.Value) ||
                !owned.TryAdd(pair.Key, pair.Value))
            {
                return false;
            }
        }
        captured = new ReadOnlyDictionary<string, double>(owned);
        return true;
    }

    private static bool TryBoundCount<T>(
        IReadOnlyList<T>? values,
        SnapshotBudget budget,
        out int count)
    {
        count = 0;
        if (values is null)
            return false;
        count = values.Count;
        return count <= MaximumNestedCollectionCount &&
               budget.AddElements(count);
    }

    private static bool ValidateOutput(
        SnapshotBudget budget,
        DesignConversionImprovementOutput? output)
        => output is not null &&
           AddText(budget, output.OutputId) &&
           AddText(budget, output.CandidateId) &&
           AddText(budget, output.Html) &&
           AddText(budget, output.Css) &&
           ValidateVerdict(budget, output.Verdict) &&
           AddCollection(budget, output.QaResults, ValidateQaResult) &&
           AddStrings(budget, output.EvidenceRefs);

    private static bool ValidateVerdict(
        SnapshotBudget budget,
        VisualVerdict? verdict)
        => verdict is not null &&
           AddText(budget, verdict.VerdictId) &&
           double.IsFinite(verdict.Score) &&
           AddCollection(budget, verdict.Defects, ValidateDefect) &&
           AddStrings(budget, verdict.EvidenceRefs);

    private static bool ValidateDefect(
        SnapshotBudget budget,
        VisualDefect? defect)
        => defect is not null &&
           AddText(budget, defect.DefectId) &&
           AddText(budget, defect.Category) &&
           AddText(budget, defect.Severity) &&
           AddText(budget, defect.Description) &&
           double.IsFinite(defect.X) &&
           double.IsFinite(defect.Y) &&
           double.IsFinite(defect.Width) &&
           double.IsFinite(defect.Height) &&
           AddStrings(budget, defect.EvidenceImageIds);

    private static bool ValidateQaResult(
        SnapshotBudget budget,
        RenderQaResultRecord? result)
        => result is not null &&
           AddText(budget, result.ResultId) &&
           AddText(budget, result.RequestId) &&
           ValidateImage(budget, result.Expected) &&
           ValidateImage(budget, result.Actual) &&
           ValidateVerdict(budget, result.Verdict) &&
           AddDoubleDictionary(budget, result.Metrics) &&
           AddStrings(budget, result.Warnings);

    private static bool ValidateImage(
        SnapshotBudget budget,
        RenderQaImageRecord? image)
        => image is not null &&
           AddText(budget, image.ImageId) &&
           AddText(budget, image.Label) &&
           image.Resolution is not null &&
           AddText(budget, image.Resolution.ResolutionId) &&
           image.Resolution.Width >= 0 &&
           image.Resolution.Height >= 0 &&
           double.IsFinite(image.Resolution.DeviceScaleFactor) &&
           AddText(budget, image.Uri) &&
           AddText(budget, image.Sha256) &&
           AddStringDictionary(budget, image.Metadata);

    private static bool ValidateProvenance(
        SnapshotBudget budget,
        DesignConversionDpoProvenance? provenance)
        => provenance is not null &&
           AddText(budget, provenance.SourceRequestId) &&
           AddText(budget, provenance.SourceKind) &&
           AddText(budget, provenance.SourceManifestRef) &&
           AddText(budget, provenance.ReviewerRef) &&
           ValidateTimestamp(provenance.CreatedAtUtc) &&
           AddStringDictionary(budget, provenance.Metadata);

    private static bool ValidateLicense(
        SnapshotBudget budget,
        DesignConversionLicenseScope? license)
        => license is not null &&
           AddText(budget, license.LicenseId) &&
           AddText(budget, license.RightsScope) &&
           AddStringDictionary(budget, license.Metadata);

    private static bool ValidateQuarantine(
        SnapshotBudget budget,
        DesignConversionQuarantineState? quarantine)
        => quarantine is not null &&
           AddText(budget, quarantine.Reason) &&
           AddStrings(budget, quarantine.EvidenceRefs);

    private static bool AddVector(
        SnapshotBudget budget,
        IReadOnlyList<double>? vector)
    {
        if (vector is null ||
            vector.Count > BoundedDpoTrainingContentProjector.MaximumVectorLength ||
            !budget.AddElements(vector.Count))
        {
            return false;
        }
        for (var index = 0; index < vector.Count; index++)
        {
            if (!double.IsFinite(vector[index]))
                return false;
        }
        return true;
    }

    private static bool AddStrings(
        SnapshotBudget budget,
        IReadOnlyList<string>? values)
        => AddCollection(
            budget,
            values,
            static (state, value) => AddText(state, value));

    private static bool AddStringDictionary(
        SnapshotBudget budget,
        IReadOnlyDictionary<string, string>? values)
    {
        if (values is null || values.Count > MaximumMetadataCount ||
            !budget.AddElements(values.Count))
        {
            return false;
        }
        foreach (var pair in values)
        {
            if (!AddText(budget, pair.Key) || !AddText(budget, pair.Value))
                return false;
        }
        return true;
    }

    private static bool AddDoubleDictionary(
        SnapshotBudget budget,
        IReadOnlyDictionary<string, double>? values)
    {
        if (values is null || values.Count > MaximumMetadataCount ||
            !budget.AddElements(values.Count))
        {
            return false;
        }
        foreach (var pair in values)
        {
            if (!AddText(budget, pair.Key) || !double.IsFinite(pair.Value))
                return false;
        }
        return true;
    }

    private static bool AddCollection<T>(
        SnapshotBudget budget,
        IReadOnlyList<T>? values,
        Func<SnapshotBudget, T, bool> validate)
    {
        if (values is null || values.Count > MaximumNestedCollectionCount ||
            !budget.AddElements(values.Count))
        {
            return false;
        }
        for (var index = 0; index < values.Count; index++)
        {
            if (!validate(budget, values[index]))
                return false;
        }
        return true;
    }

    private static bool AddOptionalText(
        SnapshotBudget budget,
        string? value)
        => value is null || AddText(budget, value);

    private static bool AddText(SnapshotBudget budget, string? value)
    {
        if (!IsBounded(
                value,
                BoundedDpoTrainingContentProjector.MaximumTextUtf8Bytes))
        {
            return false;
        }
        return budget.AddUtf8Bytes(Encoding.UTF8.GetByteCount(value!));
    }

    private static bool IsBounded(string? value, int maximumUtf8Bytes)
        => value is not null &&
           Encoding.UTF8.GetByteCount(value) <= maximumUtf8Bytes;

    private static bool ValidateTimestamp(DateTimeOffset value)
        => value != default && value.Offset >= TimeSpan.FromHours(-14) &&
           value.Offset <= TimeSpan.FromHours(14);

    private static OperationResult<DpoTrainingContentBatch>
        InvalidProjection()
        => OperationResult<DpoTrainingContentBatch>.Fail(
            "DesignConversion DPO training content is invalid or exceeds a bounded limit.",
            BoundedDpoTrainingContentProjector.LimitExceededErrorCode);

    private static OperationResult<DesignConversionDpoTrainingRecord[]>
        CaptureInvalid()
        => OperationResult<DesignConversionDpoTrainingRecord[]>.Fail(
            "DesignConversion training records could not be captured safely.",
            LegalTrainingTeacherAdmissionAuthority.EnumerationFailedErrorCode);

    private static OperationResult<DesignConversionDpoTrainingRecord[]>
        CaptureLimitExceeded()
        => OperationResult<DesignConversionDpoTrainingRecord[]>.Fail(
            "DesignConversion training records exceed a bounded capture limit.",
            BoundedDpoTrainingContentProjector.LimitExceededErrorCode);

    private static OperationResult<DesignConversionDpoTrainingRecord[]>
        CaptureTeacherProvenanceRequired()
        => OperationResult<DesignConversionDpoTrainingRecord[]>.Fail(
            "Typed teacher provenance is required for every DesignConversion training record.",
            LegalTrainingTeacherAdmissionAuthority.ProvenanceRequiredErrorCode);

    private static OperationResult<DesignConversionDpoTrainingRecord>
        CaptureInvalidRecord()
        => OperationResult<DesignConversionDpoTrainingRecord>.Fail(
            "DesignConversion training record could not be captured safely.",
            BoundedDpoTrainingContentProjector.LimitExceededErrorCode);

    private static OperationResult<DesignConversionStoredDpoExample[]>
        CaptureExamplesInvalid()
        => OperationResult<DesignConversionStoredDpoExample[]>.Fail(
            "DesignConversion training examples exceed a bounded capture limit.",
            BoundedDpoTrainingContentProjector.LimitExceededErrorCode);

    private static OperationResult<DesignConversionFinalOptimizerBatch<T>>
        FinalInputInvalid<T>()
        where T : class
        => OperationResult<DesignConversionFinalOptimizerBatch<T>>.Fail(
            "The exact final DesignConversion optimizer input is invalid.",
            BoundedDpoTrainingContentProjector.InvalidErrorCode);

    private sealed class SnapshotBudget
    {
        private long _utf8Bytes;
        private long _elements;

        internal bool AddUtf8Bytes(int count)
        {
            _utf8Bytes = checked(_utf8Bytes + count);
            return _utf8Bytes <= MaximumSnapshotUtf8Bytes;
        }

        internal bool AddElements(int count)
        {
            _elements = checked(_elements + count);
            return _elements <= MaximumSnapshotElements;
        }
    }
}

/// <summary>
/// The single production projection from one approved DesignConversion record
/// into the exact durable DPO pair consumed by training-intake storage and by
/// external atomic-group identity checks.
/// </summary>
internal static class DesignConversionTrainingIntakeDpoPairAuthority
{
    private static readonly JsonSerializerOptions GovernanceJsonOptions =
        new(DesignConversionCheckpointJsonOptions.Standard)
        {
            WriteIndented = false
        };

    internal static OperationResult<DpoPair> Project(
        DesignConversionDpoTrainingRecord? record,
        string? repositoryOwnedDomainTag = null)
    {
        if (record is null ||
            !JsonlDesignConversionLocalArtifactDb.IsRecordTrainable(record))
        {
            return OperationResult<DpoPair>.Fail(
                "DesignConversion training-intake DPO pair is not approved for durable training storage.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_DPO_PAIR_NOT_TRAINABLE");
        }

        try
        {
            var item = DesignConversionTrainingPipeline
                .ToCanonicalTrainingItem(record);
            var governance = new TrainingDataGovernanceDecision(
                licenseAccepted: record.License.AllowsTraining,
                isQuarantined: record.Quarantine.IsQuarantined,
                evidenceRefs: BuildGovernanceEvidenceRefs(record));
            if (!TryProjectSideAuthorships(
                    record,
                    out var chosenAuthorship,
                    out var rejectedAuthorship))
            {
                return OperationResult<DpoPair>.Fail(
                    "DesignConversion training-intake DPO pair authorship is invalid.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_DPO_PAIR_NOT_TRAINABLE");
            }
            // WHY: the legacy local training-intake path keeps the machine's
            // internal domain tag, while repository-owned datasets must store
            // pairs under the exact domain slug present in their canonical
            // training-data/dpo/... address. Otherwise the sealer rejects the
            // manifest because the bytes do not belong to the requested domain.
            var domainTag = string.IsNullOrWhiteSpace(repositoryOwnedDomainTag)
                ? DesignConversionMachine.DomainTag
                : repositoryOwnedDomainTag;
            var pair = new DpoPair(
                item.Prompt,
                item.ChosenResponse,
                item.RejectedResponse,
                domainTag,
                record.Provenance.CreatedAtUtc)
            {
                TeacherProvenance = record.TeacherProvenance,
                GovernanceDecision = governance,
                ChosenAuthorship = chosenAuthorship,
                RejectedAuthorship = rejectedAuthorship
            };
            return DpoPairTrainingContentAuthority.Bind(pair);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<DpoPair>.Fail(
                "DesignConversion training-intake DPO pair projection failed.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_DPO_PAIR_NOT_TRAINABLE");
        }
    }

    private static bool TryProjectSideAuthorships(
        DesignConversionDpoTrainingRecord record,
        out DpoPairSideAuthorship? chosen,
        out DpoPairSideAuthorship? rejected)
    {
        chosen = null;
        rejected = null;
        if (!record.ChosenAuthorKind.HasValue ||
            !record.RejectedAuthorKind.HasValue ||
            record.TeacherProvenance is null)
        {
            return false;
        }

        try
        {
            chosen = ProjectSideAuthorship(
                record.ChosenAuthorKind.Value,
                record.TeacherProvenance);
            rejected = ProjectSideAuthorship(
                record.RejectedAuthorKind.Value,
                record.TeacherProvenance);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            chosen = null;
            rejected = null;
            return false;
        }

        if (record.TeacherProvenance.SourceKind ==
            TrainingTeacherSourceKind.DeterministicNoExternalTeacher)
        {
            return chosen.AuthorKind is not DpoPairSideAuthorKind.OpenTeacher
                   && rejected.AuthorKind is not DpoPairSideAuthorKind.OpenTeacher;
        }

        if (record.TeacherProvenance.SourceKind !=
            TrainingTeacherSourceKind.ExternalTeacherOutput)
        {
            return false;
        }

        // DesignConversion переносит raw author-kind до shared Bind: так v5
        // фиксирует фактическую сторону OpenTeacher, а не откатывается в v4.
        return (chosen.AuthorKind is DpoPairSideAuthorKind.OpenTeacher) ^
               (rejected.AuthorKind is DpoPairSideAuthorKind.OpenTeacher);
    }

    private static DpoPairSideAuthorship ProjectSideAuthorship(
        DpoPairSideAuthorKind authorKind,
        TrainingTeacherProvenance teacherProvenance)
        => authorKind is DpoPairSideAuthorKind.OpenTeacher
            ? new DpoPairSideAuthorship(authorKind, teacherProvenance)
            : new DpoPairSideAuthorship(authorKind);

    private static IReadOnlyList<string> BuildGovernanceEvidenceRefs(
        DesignConversionDpoTrainingRecord record)
    {
        var refs = (record.Quarantine.EvidenceRefs ?? [])
            .Where(IsSha256Ref)
            .Distinct(StringComparer.Ordinal)
            .Take(31)
            .ToList();
        var governanceSnapshot = JsonSerializer.Serialize(new
        {
            record_id = record.RecordId,
            license_id = record.License.LicenseId,
            rights_scope = record.License.RightsScope,
            allows_training = record.License.AllowsTraining,
            contains_user_private_data = record.License.ContainsUserPrivateData,
            quarantine = new
            {
                is_quarantined = record.Quarantine.IsQuarantined,
                reason = record.Quarantine.Reason,
                evidence_refs = record.Quarantine.EvidenceRefs
            },
            source_manifest_ref = record.Provenance.SourceManifestRef,
            reviewer_ref = record.Provenance.ReviewerRef
        }, GovernanceJsonOptions);
        var digestRef = string.Concat(
            "sha256:",
            Convert.ToHexString(SHA256.HashData(
                    Encoding.UTF8.GetBytes(governanceSnapshot)))
                .ToLowerInvariant());
        if (!refs.Contains(digestRef, StringComparer.Ordinal))
            refs.Add(digestRef);
        return refs.ToArray();
    }

    private static bool IsSha256Ref(string? value)
        => value is { Length: 71 } &&
           value.StartsWith("sha256:", StringComparison.Ordinal) &&
           value.AsSpan(7).ToArray().All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');
}

/// <summary>
/// Immutable authority carrier for the exact optimizer objects used after
/// admission. It owns the input-reference sequence, rebound provenance, and
/// neutral content commitment as one indivisible snapshot.
/// </summary>
internal sealed class DesignConversionFinalOptimizerBatch<T>
    where T : class
{
    private readonly ReadOnlyCollection<T> _inputs;
    private readonly ReadOnlyCollection<TrainingTeacherProvenance?>
        _provenance;

    internal DesignConversionFinalOptimizerBatch(
        T[] inputs,
        TrainingTeacherProvenance?[] provenance,
        DpoTrainingContentBatch content)
    {
        _inputs = Array.AsReadOnly(inputs);
        _provenance = Array.AsReadOnly(provenance);
        Content = content;
    }

    internal IReadOnlyList<T> Inputs => _inputs;
    internal IReadOnlyList<TrainingTeacherProvenance?> Provenance =>
        _provenance;
    internal DpoTrainingContentBatch Content { get; }
}

/// <summary>
/// Opaque authority binding for one exact prepared Pipeline optimizer. A
/// direct binding is backed by that optimizer's exact content lease; a
/// composite binding is derived from the parent composite lease while
/// retaining the same exact Pipeline child commitment.
/// </summary>
internal sealed class DesignConversionPipelineTrainingBinding
{
    private readonly ServerOwnedLegalTrainingTeacherAdmissionLease _lease;
    private readonly DesignConversionFinalOptimizerBatch<LearningTrainingItem>
        _pipelineFinalOptimizer;
    private readonly DesignConversionDpoTrainingContentAuthority
        .CompositeOptimizerCommitment? _compositeCommitment;
    private readonly DesignConversionDpoTrainingContentAuthority
        .DualTargetTeacherAggregateCommitment? _dualTargetTeacherCommitment;
    private object? _externalConsumptionScopeIdentity;

    private DesignConversionPipelineTrainingBinding(
        DesignConversionFinalOptimizerBatch<LearningTrainingItem>
            pipelineFinalOptimizer,
            ServerOwnedLegalTrainingTeacherAdmissionLease lease,
            DesignConversionDpoTrainingContentAuthority
                .CompositeOptimizerCommitment? compositeCommitment,
            DesignConversionDpoTrainingContentAuthority
                .DualTargetTeacherAggregateCommitment?
                dualTargetTeacherCommitment = null)
    {
        _pipelineFinalOptimizer = pipelineFinalOptimizer;
        _lease = lease;
        _compositeCommitment = compositeCommitment;
        _dualTargetTeacherCommitment = dualTargetTeacherCommitment;
    }

    internal static OperationResult<DesignConversionPipelineTrainingBinding>
        MintExact(
            DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
                pipelineFinalOptimizer,
            ServerOwnedLegalTrainingTeacherAdmissionLease? lease)
    {
        if (pipelineFinalOptimizer is not null)
        {
            foreach (var input in pipelineFinalOptimizer.Inputs)
            {
                if (input is null)
                    return Invalid();
                var sideContract =
                    DesignConversionDpoOptimizerSideContract.ValidatePair(
                        input.ChosenResponse,
                        input.RejectedResponse);
                if (!sideContract.IsSuccess)
                {
                    return sideContract.MapError<
                        DesignConversionPipelineTrainingBinding>();
                }
            }
        }

        if (!DesignConversionDpoTrainingContentAuthority
                .MatchesExactPipelineFinalOptimizer(
                    pipelineFinalOptimizer) ||
            lease is null ||
            !lease.MatchesExactTrainingContent(
                pipelineFinalOptimizer!.Content))
        {
            return Invalid();
        }

        return OperationResult<DesignConversionPipelineTrainingBinding>.Ok(
            new DesignConversionPipelineTrainingBinding(
                pipelineFinalOptimizer,
                lease,
                compositeCommitment: null));
    }

    internal static OperationResult<DesignConversionPipelineTrainingBinding>
        MintComposite(
            DesignConversionDpoTrainingContentAuthority
                .CompositeOptimizerCommitment? compositeCommitment,
            DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
                pipelineFinalOptimizer,
            DesignConversionFinalOptimizerBatch<
                DesignConversionDpoEvaluatedPackedBatch>?
                bridgeFinalOptimizer,
            ServerOwnedLegalTrainingTeacherAdmissionLease? parentLease)
    {
        if (compositeCommitment is null || parentLease is null ||
            !DesignConversionDpoTrainingContentAuthority
                .MatchesExactPipelineFinalOptimizer(
                    pipelineFinalOptimizer) ||
            !compositeCommitment.MatchesExactChildren(
                pipelineFinalOptimizer,
                bridgeFinalOptimizer) ||
            !parentLease.MatchesExactTrainingContent(
                compositeCommitment.FinalOptimizer.Content))
        {
            return Invalid();
        }

        return OperationResult<DesignConversionPipelineTrainingBinding>.Ok(
            new DesignConversionPipelineTrainingBinding(
                pipelineFinalOptimizer!,
                parentLease,
                compositeCommitment));
    }

    internal static OperationResult<DesignConversionPipelineTrainingBinding>
        MintDualTargetTeacherAggregate(
            DesignConversionDpoTrainingContentAuthority
                .DualTargetTeacherAggregateCommitment? aggregateCommitment,
            DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
                pipelineFinalOptimizer,
            ServerOwnedLegalTrainingTeacherAdmissionLease? parentLease)
    {
        if (aggregateCommitment is null || parentLease is null ||
            !ReferenceEquals(
                aggregateCommitment.FinalOptimizer,
                pipelineFinalOptimizer) ||
            !aggregateCommitment.MatchesExactChildren(
                pipelineFinalOptimizer) ||
            !parentLease.MatchesExactTrainingContent(
                aggregateCommitment.ParentContent))
        {
            return Invalid();
        }

        return OperationResult<DesignConversionPipelineTrainingBinding>.Ok(
            new DesignConversionPipelineTrainingBinding(
                pipelineFinalOptimizer!,
                parentLease,
                compositeCommitment: null,
                dualTargetTeacherCommitment: aggregateCommitment));
    }

    internal bool MatchesExactPreparedFinal(
        DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
            pipelineFinalOptimizer)
    {
        if (!MatchesExactPreparedChildren(pipelineFinalOptimizer))
        {
            return false;
        }

        if (_dualTargetTeacherCommitment is not null)
        {
            return _lease.MatchesExactTrainingContent(
                       _dualTargetTeacherCommitment.ParentContent);
        }

        return _compositeCommitment is null
            ? _lease.MatchesExactTrainingContent(
                _pipelineFinalOptimizer.Content)
            : _lease.MatchesExactTrainingContent(
                  _compositeCommitment.FinalOptimizer.Content);
    }

    private bool MatchesExactPreparedChildren(
        DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
            pipelineFinalOptimizer)
    {
        if (!ReferenceEquals(
                _pipelineFinalOptimizer,
                pipelineFinalOptimizer) ||
            !DesignConversionDpoTrainingContentAuthority
                .MatchesExactPipelineFinalOptimizer(
                    _pipelineFinalOptimizer))
        {
            return false;
        }

        if (_dualTargetTeacherCommitment is not null)
        {
            return _compositeCommitment is null &&
                   _dualTargetTeacherCommitment.MatchesExactChildren(
                       _pipelineFinalOptimizer);
        }

        return _compositeCommitment is null ||
               _compositeCommitment.MatchesExactChildren(
                   _pipelineFinalOptimizer,
                   _compositeCommitment.BridgeFinalOptimizer);
    }

    internal bool MatchesExactPreparedBridge(
        DesignConversionFinalOptimizerBatch<
            DesignConversionDpoEvaluatedPackedBatch>?
            bridgeFinalOptimizer)
        => _compositeCommitment is not null &&
           ReferenceEquals(
               _compositeCommitment.BridgeFinalOptimizer,
               bridgeFinalOptimizer) &&
           MatchesExactPreparedFinal(_pipelineFinalOptimizer);

    internal bool MatchesExactLease(
        ServerOwnedLegalTrainingTeacherAdmissionLease? lease)
        => ReferenceEquals(_lease, lease);

    internal bool MatchesExactBoundContent()
        => MatchesExactPreparedFinal(_pipelineFinalOptimizer);

    internal bool IsDualTargetTeacherAggregate =>
        _dualTargetTeacherCommitment is not null;

    internal bool RequiresExternalConsumptionExecution =>
        Volatile.Read(ref _externalConsumptionScopeIdentity) is not null;

    internal bool TryBindExternalConsumptionExecution(object scopeIdentity)
        => scopeIdentity is not null &&
           ReferenceEquals(
               Interlocked.CompareExchange(
                   ref _externalConsumptionScopeIdentity,
                   scopeIdentity,
                   null) ?? scopeIdentity,
               scopeIdentity);

    internal bool MatchesExternalConsumptionExecution(object? scopeIdentity)
        => scopeIdentity is not null &&
           ReferenceEquals(
               Volatile.Read(ref _externalConsumptionScopeIdentity),
               scopeIdentity);

    internal OperationResult<LearningBatch<DesignConversionDomain>>
        BuildAtomicDualTargetTeacherBatch(string runId)
        => _dualTargetTeacherCommitment is null
            ? OperationResult<LearningBatch<DesignConversionDomain>>.Fail(
                "The exact atomic Critic and Improver teacher aggregate is required.",
                LegalTrainingTeacherAdmissionAuthority
                    .TrainingContentInvalidErrorCode)
            : _dualTargetTeacherCommitment.BuildAtomicMachineBatch(runId);

    internal OperationResult<IDisposable> AcquireMutationPermit()
    {
        if (!MatchesExactPreparedChildren(_pipelineFinalOptimizer))
        {
            return OperationResult<IDisposable>.Fail(
                "Exact admitted DesignConversion Pipeline training content is required.",
                LegalTrainingTeacherAdmissionAuthority
                    .TrainingContentInvalidErrorCode);
        }

        var permit = _lease.AcquireMutationPermit();
        if (!permit.IsSuccess || permit.Value is null)
            return permit;

        if (MatchesExactPreparedFinal(_pipelineFinalOptimizer))
            return permit;

        permit.Value.Dispose();
        return OperationResult<IDisposable>.Fail(
            "Exact admitted DesignConversion Pipeline training content is required.",
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode);
    }

    private static OperationResult<DesignConversionPipelineTrainingBinding>
        Invalid()
        => OperationResult<DesignConversionPipelineTrainingBinding>.Fail(
            "Exact admitted DesignConversion Pipeline training content is required.",
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode);
}

/// <summary>
/// Owns the one legal parent lease and opaque Pipeline binding for an atomic
/// Critic/Improver teacher-authored aggregate. Child rows are not independently
/// admitted and cannot obtain a second lease through this carrier.
/// </summary>
internal sealed class DesignConversionDualTargetTeacherAggregateAdmission :
    IDisposable
{
    private readonly ServerOwnedLegalTrainingTeacherAdmissionLease _lease;
    private bool _disposed;

    internal DesignConversionDualTargetTeacherAggregateAdmission(
        DesignConversionPipelineTrainingBinding binding,
        ServerOwnedLegalTrainingTeacherAdmissionLease lease,
        DesignConversionDpoTrainingContentAuthority
            .DualTargetTeacherAggregateCommitment commitment)
    {
        Binding = binding;
        _lease = lease;
        Commitment = commitment;
    }

    internal DesignConversionPipelineTrainingBinding Binding { get; }
    internal DesignConversionDpoTrainingContentAuthority
        .DualTargetTeacherAggregateCommitment Commitment { get; }
    internal ServerOwnedLegalTrainingTeacherAdmissionLease Lease => _lease;

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _lease.Dispose();
    }
}

/// <summary>
/// Immutable identity projected from the exact H2a Critic/Improver records.
/// It is independent of, and must exactly match, the live store snapshot.
/// </summary>
internal sealed record DesignConversionExpectedExternalGroupIdentity(
    string GroupCommitmentSha256,
    string RequestId,
    IReadOnlyList<string> OrderedPairIds,
    IReadOnlyList<string> OrderedContentSha256s)
{
    internal static OperationResult<
        DesignConversionExpectedExternalGroupIdentity> Create(
            DpoPairAtomicGroupEnvelope? envelope)
    {
        if (envelope is null ||
            envelope.SchemaVersion !=
                DpoPairAtomicGroupEnvelope.CurrentSchemaVersion ||
            envelope.InvocationCount != 1 ||
            !TrainingSourceProvenance.IsCanonicalSha256(
                envelope.GroupCommitmentSha256) ||
            string.IsNullOrWhiteSpace(envelope.RequestId) ||
            envelope.Children is not { Count: 2 })
        {
            return Invalid();
        }

        var children = envelope.Children
            .OrderBy(static child => child.Ordinal)
            .ToArray();
        if (children[0].Ordinal != 0 ||
            children[0].Role != DpoPairAtomicGroupRole.CriticUnit ||
            children[1].Ordinal != 1 ||
            children[1].Role != DpoPairAtomicGroupRole.ImproverUnit ||
            children.Select(static child => child.PairId).Any(
                static value =>
                    !TrainingSourceProvenance.IsCanonicalSha256(value)) ||
            children.Select(static child => child.ContentSha256).Any(
                static value =>
                    !TrainingSourceProvenance.IsCanonicalSha256(value)))
        {
            return Invalid();
        }

        return OperationResult<
            DesignConversionExpectedExternalGroupIdentity>.Ok(new(
                envelope.GroupCommitmentSha256,
                envelope.RequestId,
                Array.AsReadOnly(children
                    .Select(static child => child.PairId).ToArray()),
                Array.AsReadOnly(children
                    .Select(static child => child.ContentSha256).ToArray())));
    }

    internal bool Matches(
        FileDpoPairStore.ExternalConsumptionSnapshot? snapshot,
        bool allowReserved,
        bool allowAggregateBound)
        => snapshot is not null &&
           (allowReserved && snapshot.State ==
                FileDpoPairStore.DpoExternalConsumptionState.Reserved ||
            allowAggregateBound && snapshot.State ==
                FileDpoPairStore.DpoExternalConsumptionState.AggregateBound) &&
           snapshot.ConsumerKind ==
               DpoExternalConsumerKind
                   .DesignConversionDualTargetTeacherAggregate &&
           string.Equals(
               snapshot.Domain,
               DesignConversionMachine.DomainTag,
               StringComparison.Ordinal) &&
           string.Equals(
               snapshot.GroupCommitmentSha256,
               GroupCommitmentSha256,
               StringComparison.Ordinal) &&
           string.Equals(
               snapshot.RequestId,
               RequestId,
               StringComparison.Ordinal) &&
           snapshot.OrderedPairIds.SequenceEqual(
               OrderedPairIds,
               StringComparer.Ordinal) &&
           snapshot.OrderedContentSha256s.SequenceEqual(
               OrderedContentSha256s,
               StringComparer.Ordinal);

    private static OperationResult<
        DesignConversionExpectedExternalGroupIdentity> Invalid()
        => OperationResult<
            DesignConversionExpectedExternalGroupIdentity>.Fail(
                "The exact external DesignConversion atomic group identity is invalid.",
                LegalTrainingTeacherAdmissionAuthority
                    .TrainingContentInvalidErrorCode);
}

/// <summary>
/// Opaque, single-execution carrier joining the one legal H2a parent lease to
/// the one store-minted H2b1c external transaction.  Neither the transaction
/// handle nor its single-use mutation authorization can be replaced after the
/// aggregate is bound.
/// </summary>
internal sealed class DesignConversionDualTargetTeacherExternalConsumptionScope :
    IDisposable
{
    private readonly DesignConversionDualTargetTeacherAggregateAdmission
        _aggregate;
    private readonly FileDpoPairStore.ExternalConsumptionTransactionHandle
        _externalTransaction;
    private readonly FileDpoPairStore.ExternalConsumptionSnapshot
        _boundSnapshot;
    private readonly DesignConversionExpectedExternalGroupIdentity
        _expectedGroup;
    private FileDpoPairStore.ExternalMutationAuthorization? _authorization;
    private bool _beginAttempted;
    private bool _completed;
    private bool _disposed;
    private DesignConversionExternalConsumptionCommitEvidence?
        _commitEvidence;

    private DesignConversionDualTargetTeacherExternalConsumptionScope(
        DesignConversionDualTargetTeacherAggregateAdmission aggregate,
        FileDpoPairStore.ExternalConsumptionTransactionHandle
            externalTransaction,
        FileDpoPairStore.ExternalConsumptionSnapshot boundSnapshot,
        DesignConversionExpectedExternalGroupIdentity expectedGroup)
    {
        _aggregate = aggregate;
        _externalTransaction = externalTransaction;
        _boundSnapshot = boundSnapshot;
        _expectedGroup = expectedGroup;
    }

    internal static OperationResult<
        DesignConversionDualTargetTeacherExternalConsumptionScope> Create(
            DesignConversionDualTargetTeacherAggregateAdmission aggregate,
            FileDpoPairStore.ExternalConsumptionTransactionHandle
                externalTransaction,
            FileDpoPairStore.ExternalConsumptionSnapshot boundSnapshot,
            DesignConversionExpectedExternalGroupIdentity expectedGroup)
    {
        var scope =
            new DesignConversionDualTargetTeacherExternalConsumptionScope(
                aggregate,
                externalTransaction,
                boundSnapshot,
                expectedGroup);
        if (!aggregate.Binding.TryBindExternalConsumptionExecution(scope))
        {
            scope.Dispose();
            return Invalid<
                DesignConversionDualTargetTeacherExternalConsumptionScope>();
        }
        return OperationResult<
            DesignConversionDualTargetTeacherExternalConsumptionScope>.Ok(
                scope);
    }

    internal DesignConversionPipelineTrainingBinding Binding =>
        _aggregate.Binding;

    internal ServerOwnedLegalTrainingTeacherAdmissionLease Lease =>
        _aggregate.Lease;

    internal DesignConversionExternalConsumptionCommitEvidence?
        CommitEvidence => _commitEvidence;

    internal bool MatchesExactPreparedFinal(
        DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
            finalOptimizer)
        => !_disposed &&
           _aggregate.Commitment.MatchesExactChildren(finalOptimizer) &&
           Binding.MatchesExactPreparedFinal(finalOptimizer) &&
           MatchesLiveIdentity(FileDpoPairStore.DpoExternalConsumptionState.AggregateBound);

    internal OperationResult<bool> BeginMutation()
    {
        if (_disposed || _beginAttempted || _completed ||
            !MatchesLiveIdentity(
                FileDpoPairStore.DpoExternalConsumptionState.AggregateBound))
        {
            return Invalid<bool>();
        }

        _beginAttempted = true;
        var authorization = _externalTransaction.BeginMutation();
        if (!authorization.IsSuccess || authorization.Value is null)
            return authorization.MapError<bool>();

        _authorization = authorization.Value;
        return OperationResult<bool>.Ok(true);
    }

    internal OperationResult<DesignConversionExternalConsumptionCommitEvidence>
        Complete(string resultSha256, string checkpointSha256)
    {
        if (_disposed || !_beginAttempted || _completed ||
            _authorization is null ||
            !TrainingSourceProvenance.IsCanonicalSha256(resultSha256) ||
            !TrainingSourceProvenance.IsCanonicalSha256(checkpointSha256) ||
            !MatchesLiveIdentity(
                FileDpoPairStore.DpoExternalConsumptionState.MutationStarted))
        {
            return Invalid<DesignConversionExternalConsumptionCommitEvidence>();
        }

        var completion = _externalTransaction.Complete(
            _authorization,
            new DpoExternalConsumptionResult(
                DpoExternalConsumerKind
                    .DesignConversionDualTargetTeacherAggregate,
                resultSha256,
                checkpointSha256));
        if (!completion.IsSuccess || completion.Value is null)
        {
            return completion.MapError<
                DesignConversionExternalConsumptionCommitEvidence>();
        }

        var receipt = completion.Value;
        if (!MatchesCompletion(receipt, resultSha256, checkpointSha256))
            return Invalid<DesignConversionExternalConsumptionCommitEvidence>();

        _commitEvidence = new DesignConversionExternalConsumptionCommitEvidence(
                receipt.TransactionId,
                receipt.GroupCommitmentSha256,
                receipt.RequestId,
                receipt.ParentBatchSha256,
                receipt.ParentContentSha256,
                receipt.ParentProvenanceSha256,
                Array.AsReadOnly(receipt.OrderedPairIds.ToArray()),
                Array.AsReadOnly(receipt.OrderedContentSha256s.ToArray()),
                resultSha256,
                checkpointSha256,
                receipt.ManifestSha256);
        _completed = true;
        return OperationResult<
            DesignConversionExternalConsumptionCommitEvidence>.Ok(
                _commitEvidence);
    }

    internal static bool MatchesBoundAggregate(
        FileDpoPairStore.ExternalConsumptionSnapshot snapshot,
        DesignConversionDpoTrainingContentAuthority
            .DualTargetTeacherAggregateCommitment commitment,
        DesignConversionExpectedExternalGroupIdentity expectedGroup)
        => snapshot is not null && commitment is not null &&
           expectedGroup is not null &&
           expectedGroup.Matches(
               snapshot,
               allowReserved: false,
               allowAggregateBound: true) &&
           snapshot.State ==
               FileDpoPairStore.DpoExternalConsumptionState.AggregateBound &&
           snapshot.ConsumerKind ==
               DpoExternalConsumerKind
                   .DesignConversionDualTargetTeacherAggregate &&
           string.Equals(
               snapshot.RequestId,
               commitment.ParentProvenance.RequestId,
               StringComparison.Ordinal) &&
           string.Equals(
               snapshot.ParentBatchSha256,
               commitment.ParentContent.Sha256,
               StringComparison.Ordinal) &&
           string.Equals(
               snapshot.ParentContentSha256,
               commitment.ParentContent.Rows.Single().Sha256,
               StringComparison.Ordinal) &&
           TrainingSourceProvenance.IsCanonicalSha256(
               snapshot.ParentProvenanceSha256);

    private bool MatchesLiveIdentity(
        FileDpoPairStore.DpoExternalConsumptionState requiredState)
    {
        var live = _externalTransaction.Read();
        return live.IsSuccess && live.Value is not null &&
               live.Value.State == requiredState &&
               MatchesIdentity(live.Value);
    }

    private bool MatchesIdentity(
        FileDpoPairStore.ExternalConsumptionSnapshot snapshot)
        => MatchesExpectedGroup(snapshot) &&
           string.Equals(snapshot.TransactionId, _boundSnapshot.TransactionId,
               StringComparison.Ordinal) &&
           string.Equals(snapshot.GroupCommitmentSha256,
               _boundSnapshot.GroupCommitmentSha256,
               StringComparison.Ordinal) &&
           string.Equals(snapshot.RequestId, _boundSnapshot.RequestId,
               StringComparison.Ordinal) &&
           string.Equals(snapshot.Domain, _boundSnapshot.Domain,
               StringComparison.Ordinal) &&
           string.Equals(snapshot.ParentBatchSha256,
               _boundSnapshot.ParentBatchSha256,
               StringComparison.Ordinal) &&
           string.Equals(snapshot.ParentContentSha256,
               _boundSnapshot.ParentContentSha256,
               StringComparison.Ordinal) &&
           string.Equals(snapshot.ParentProvenanceSha256,
               _boundSnapshot.ParentProvenanceSha256,
               StringComparison.Ordinal) &&
           snapshot.OrderedPairIds.SequenceEqual(
               _boundSnapshot.OrderedPairIds,
               StringComparer.Ordinal) &&
           snapshot.OrderedContentSha256s.SequenceEqual(
               _boundSnapshot.OrderedContentSha256s,
               StringComparer.Ordinal) &&
           snapshot.ConsumerKind == _boundSnapshot.ConsumerKind;

    private bool MatchesExpectedGroup(
        FileDpoPairStore.ExternalConsumptionSnapshot snapshot)
        => string.Equals(
               snapshot.Domain,
               DesignConversionMachine.DomainTag,
               StringComparison.Ordinal) &&
           string.Equals(
               snapshot.GroupCommitmentSha256,
               _expectedGroup.GroupCommitmentSha256,
               StringComparison.Ordinal) &&
           string.Equals(
               snapshot.RequestId,
               _expectedGroup.RequestId,
               StringComparison.Ordinal) &&
           snapshot.OrderedPairIds.SequenceEqual(
               _expectedGroup.OrderedPairIds,
               StringComparer.Ordinal) &&
           snapshot.OrderedContentSha256s.SequenceEqual(
               _expectedGroup.OrderedContentSha256s,
               StringComparer.Ordinal);

    private bool MatchesCompletion(
        FileDpoPairStore.ExternalConsumptionCompletionReceipt receipt,
        string resultSha256,
        string checkpointSha256)
        => string.Equals(receipt.TransactionId, _boundSnapshot.TransactionId,
               StringComparison.Ordinal) &&
           string.Equals(receipt.GroupCommitmentSha256,
               _expectedGroup.GroupCommitmentSha256,
               StringComparison.Ordinal) &&
           string.Equals(receipt.RequestId, _boundSnapshot.RequestId,
               StringComparison.Ordinal) &&
           string.Equals(receipt.RequestId, _expectedGroup.RequestId,
               StringComparison.Ordinal) &&
           string.Equals(receipt.ParentBatchSha256,
               _boundSnapshot.ParentBatchSha256,
               StringComparison.Ordinal) &&
           string.Equals(receipt.ParentContentSha256,
               _boundSnapshot.ParentContentSha256,
               StringComparison.Ordinal) &&
           string.Equals(receipt.ParentProvenanceSha256,
               _boundSnapshot.ParentProvenanceSha256,
               StringComparison.Ordinal) &&
           receipt.OrderedPairIds.SequenceEqual(
               _expectedGroup.OrderedPairIds,
               StringComparer.Ordinal) &&
           receipt.OrderedContentSha256s.SequenceEqual(
               _expectedGroup.OrderedContentSha256s,
               StringComparer.Ordinal) &&
           receipt.ConsumerKind ==
               DpoExternalConsumerKind
                   .DesignConversionDualTargetTeacherAggregate &&
           string.Equals(receipt.ResultSha256, resultSha256,
               StringComparison.Ordinal) &&
           string.Equals(receipt.CheckpointSha256, checkpointSha256,
               StringComparison.Ordinal) &&
           TrainingSourceProvenance.IsCanonicalSha256(
               receipt.ManifestSha256);

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _aggregate.Dispose();
    }

    private static OperationResult<T> Invalid<T>()
        => OperationResult<T>.Fail(
            "The exact external DesignConversion aggregate authority is invalid.",
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode);
}

internal sealed record DesignConversionExternalConsumptionCommitEvidence(
    string TransactionId,
    string GroupCommitmentSha256,
    string RequestId,
    string ParentBatchSha256,
    string ParentContentSha256,
    string ParentProvenanceSha256,
    IReadOnlyList<string> OrderedPairIds,
    IReadOnlyList<string> OrderedContentSha256s,
    string ResultSha256,
    string CheckpointSha256,
    string ManifestSha256);

/// <summary>
/// Opaque, immutable authority for one exact DesignConversion Machine
/// mutation. Only the content authority can mint it from an already-admitted
/// optimizer commitment and the exact selected optimizer object references.
/// </summary>
internal sealed class DesignConversionMachineTrainingAdmission
{
    private readonly DesignConversionPipelineTrainingBinding _binding;

    private DesignConversionMachineTrainingAdmission(
        LearningBatch<DesignConversionDomain> batch,
        DesignConversionPipelineTrainingBinding binding,
        string finalOptimizerSha256,
        string[] selectedRowSha256)
    {
        Batch = batch;
        _binding = binding;
        FinalOptimizerSha256 = finalOptimizerSha256;
        SelectedRowSha256 = Array.AsReadOnly(selectedRowSha256);
    }

    internal LearningBatch<DesignConversionDomain> Batch { get; }
    internal string FinalOptimizerSha256 { get; }
    internal IReadOnlyList<string> SelectedRowSha256 { get; }

    internal OperationResult<IDisposable> AcquireMutationPermit()
        => _binding.AcquireMutationPermit();

    internal static OperationResult<DesignConversionMachineTrainingAdmission>
        Mint(
            DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
                finalOptimizer,
            LearningBatch<DesignConversionDomain>? selectedBatch,
            ServerOwnedLegalTrainingTeacherAdmissionLease? lease)
    {
        var binding = DesignConversionPipelineTrainingBinding.MintExact(
            finalOptimizer,
            lease);
        return !binding.IsSuccess || binding.Value is null
            ? binding.MapError<DesignConversionMachineTrainingAdmission>()
            : Mint(finalOptimizer, selectedBatch, binding.Value);
    }

    internal static OperationResult<DesignConversionMachineTrainingAdmission>
        Mint(
            DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
                finalOptimizer,
            LearningBatch<DesignConversionDomain>? selectedBatch,
            DesignConversionPipelineTrainingBinding? binding)
    {
        if (finalOptimizer is null || selectedBatch is null ||
            binding is null ||
            !binding.MatchesExactPreparedFinal(finalOptimizer) ||
            !string.Equals(
                selectedBatch.DomainTag,
                DesignConversionMachine.DomainTag,
                StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(selectedBatch.BatchId))
        {
            return Invalid();
        }

        try
        {
            var selectedItems = selectedBatch.TrainingItems;
            var sourceIds = selectedBatch.SourceIds;
            if (selectedItems is null || sourceIds is null ||
                selectedItems.Count is <= 0 or >
                    BoundedDpoTrainingContentProjector.MaximumRowCount ||
                finalOptimizer.Inputs.Count !=
                    finalOptimizer.Content.Rows.Count ||
                finalOptimizer.Provenance.Count !=
                    finalOptimizer.Content.Rows.Count)
            {
                return Invalid();
            }

            var allReferences = new HashSet<LearningTrainingItem>(
                ReferenceEqualityComparer.Instance);
            foreach (var input in finalOptimizer.Inputs)
            {
                if (input is null || !allReferences.Add(input))
                    return Invalid();
            }

            var selectedReferences = new HashSet<LearningTrainingItem>(
                ReferenceEqualityComparer.Instance);
            var captured = new LearningTrainingItem[selectedItems.Count];
            var rowSha256 = new string[selectedItems.Count];
            var previousFinalIndex = -1;
            for (var selectedIndex = 0;
                 selectedIndex < selectedItems.Count;
                 selectedIndex++)
            {
                var item = selectedItems[selectedIndex];
                if (item is null || !selectedReferences.Add(item))
                    return Invalid();

                var finalIndex = -1;
                for (var index = 0;
                     index < finalOptimizer.Inputs.Count;
                     index++)
                {
                    if (ReferenceEquals(item, finalOptimizer.Inputs[index]))
                    {
                        finalIndex = index;
                        break;
                    }
                }
                if (finalIndex <= previousFinalIndex)
                    return Invalid();

                var committed = finalOptimizer.Content.Rows[finalIndex];
                if (!string.Equals(
                        item.Prompt,
                        committed.Prompt,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        item.ChosenResponse,
                        committed.Chosen,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        item.RejectedResponse,
                        committed.Rejected,
                        StringComparison.Ordinal))
                {
                    return Invalid();
                }

                var sideContract =
                    DesignConversionDpoOptimizerSideContract.ValidatePair(
                        item.ChosenResponse,
                        item.RejectedResponse);
                if (!sideContract.IsSuccess)
                {
                    return sideContract.MapError<
                        DesignConversionMachineTrainingAdmission>();
                }

                captured[selectedIndex] = new LearningTrainingItem(
                    item.ItemId,
                    item.Prompt,
                    item.ChosenResponse,
                    item.RejectedResponse);
                rowSha256[selectedIndex] = committed.Sha256;
                previousFinalIndex = finalIndex;
            }

            var capturedSourceIds = new string[sourceIds.Count];
            for (var index = 0; index < sourceIds.Count; index++)
            {
                if (sourceIds[index] is not { } sourceId)
                    return Invalid();
                capturedSourceIds[index] = sourceId;
            }

            var capturedBatch = new LearningBatch<DesignConversionDomain>(
                selectedBatch.BatchId,
                DesignConversionMachine.DomainTag,
                Array.AsReadOnly(captured),
                Array.AsReadOnly(capturedSourceIds));
            return OperationResult<
                DesignConversionMachineTrainingAdmission>.Ok(
                    new DesignConversionMachineTrainingAdmission(
                        capturedBatch,
                        binding,
                        finalOptimizer.Content.Sha256,
                        rowSha256));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid();
        }
    }

    private static OperationResult<DesignConversionMachineTrainingAdmission>
        Invalid()
        => OperationResult<DesignConversionMachineTrainingAdmission>.Fail(
            "Exact admitted DesignConversion Machine training content is required.",
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode);
}

internal sealed record DesignConversionCompositeOptimizerInput(
    LearningTrainingItem PipelineInput,
    DpoPair PersistedPair,
    DesignConversionDpoEvaluatedPackedBatch? BridgeInput,
    int PipelineOrdinal = -1,
    string PipelineChildBatchSha256 = "untyped_pipeline_child",
    string PipelineChildRowSha256 = "untyped_pipeline_row",
    int? BridgeOrdinal = null,
    string BridgeChildBatchSha256 = "stage_absent",
    string BridgeChildRowSha256 = "stage_absent");
