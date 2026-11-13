// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using DPO;
using DPO.Models;
using LoRA;
using TensorCore.Core;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

public sealed class DesignConversionLoraCheckpointConfiguration :
    IEquatable<DesignConversionLoraCheckpointConfiguration>
{
    private readonly string[] _targetModules;

    public int Rank { get; }
    public float Alpha { get; }
    public float Dropout { get; }
    public IReadOnlyList<string> TargetModules =>
        Array.AsReadOnly(_targetModules.ToArray());

    public DesignConversionLoraCheckpointConfiguration(LoraConfig config)
    {
        var canonical = LoraApplicator.ValidateAndCanonicalize(config);
        Rank = canonical.Rank;
        Alpha = canonical.Alpha;
        Dropout = canonical.Dropout;
        _targetModules = canonical.TargetModules.ToArray();
    }

    public LoraConfig ToLoraConfig()
        => new()
        {
            Rank = Rank,
            Alpha = Alpha,
            Dropout = Dropout,
            TargetModules = _targetModules.ToArray()
        };

    public bool Equals(DesignConversionLoraCheckpointConfiguration? other)
        => other is not null
           && Rank == other.Rank
           && Alpha.Equals(other.Alpha)
           && Dropout.Equals(other.Dropout)
           && _targetModules.SequenceEqual(other._targetModules, StringComparer.Ordinal);

    public override bool Equals(object? obj)
        => obj is DesignConversionLoraCheckpointConfiguration other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Rank);
        hash.Add(Alpha);
        hash.Add(Dropout);
        foreach (var target in _targetModules)
            hash.Add(target, StringComparer.Ordinal);
        return hash.ToHashCode();
    }
}

public sealed record DesignConversionDpoTrainingHandoffResult(
    int RecordCount,
    int PreferenceCount,
    int DpoStepCount,
    string[] MetricFilePaths,
    string[] PolicyCheckpointFilePaths,
    string[] IngredientIds,
    DesignConversionLoraCheckpointConfiguration LoraConfiguration,
    DesignConversionDpoTrainingHandoffAblationGate QualityGate,
    LegalTrainingTeacherAdmission LegalTeacherAdmission,
    string[] EvidenceRefs);

public sealed record DesignConversionDpoPolicyRuntimeCheckpointBridgeResult(
    string PolicyCheckpointPath,
    string PolicyCheckpointSha256,
    string RuntimeCheckpointPath,
    LearningMachineExport RuntimeExport,
    DesignConversionMachineCheckpoint RuntimeCheckpoint,
    bool PolicyCheckpointFreshLoadVerified,
    DesignConversionLoraCheckpointConfiguration LoraConfiguration,
    string[] EvidenceRefs,
    DesignConversionDpoPolicyCheckpointRef? PolicyCheckpointRef = null);

public sealed record DesignConversionDpoTrainingHandoffAblationGate(
    int EvaluatedRecordCount,
    double PreferredScore,
    double BaselineScore,
    double ScoreDelta,
    double Threshold,
    bool PassesQualityGate,
    string GateReason,
    IReadOnlyList<string> EvidenceRefs);

public static class DesignConversionDpoTrainingHandoff
{
    private static readonly AsyncLocal<Action<string>?> RuntimeTransactionObserver = new();
    private static readonly AsyncLocal<Action?> PolicyCheckpointFreshLoadObserver = new();
    private static readonly AsyncLocal<Action<ServerOwnedLegalTrainingTeacherAdmissionLease>?>
        TrainingLeaseObserver = new();
    private static readonly AsyncLocal<Action<string>?> MutationBoundaryObserver = new();
    private static readonly AsyncLocal<Action<LogicalDpoTrainingRequest>?>
        TrainingRequestObserver = new();
    private static readonly AsyncLocal<Action<IReadOnlyList<DesignConversionDpoTrainingRecord>>?>
        TrainingRowsObserver = new();
    private static readonly AsyncLocal<Action<LearningBatch<global::DesignConversionDomain.DesignConversionDomain>>?>
        RuntimeOptimizerBatchObserver = new();
    internal static Action<string>? RuntimeTransactionStageObserverForTesting
    {
        get => RuntimeTransactionObserver.Value;
        set => RuntimeTransactionObserver.Value = value;
    }

    internal static Action? PolicyCheckpointFreshLoadObserverForTesting
    {
        get => PolicyCheckpointFreshLoadObserver.Value;
        set => PolicyCheckpointFreshLoadObserver.Value = value;
    }

    internal static Action<ServerOwnedLegalTrainingTeacherAdmissionLease>?
        TrainingLeaseObserverForTesting
    {
        get => TrainingLeaseObserver.Value;
        set => TrainingLeaseObserver.Value = value;
    }

    internal static Action<string>? MutationBoundaryObserverForTesting
    {
        get => MutationBoundaryObserver.Value;
        set => MutationBoundaryObserver.Value = value;
    }

    internal static Action<LogicalDpoTrainingRequest>? TrainingRequestObserverForTesting
    {
        get => TrainingRequestObserver.Value;
        set => TrainingRequestObserver.Value = value;
    }

    internal static Action<IReadOnlyList<DesignConversionDpoTrainingRecord>>?
        TrainingRowsObserverForTesting
    {
        get => TrainingRowsObserver.Value;
        set => TrainingRowsObserver.Value = value;
    }

    internal static Action<LearningBatch<global::DesignConversionDomain.DesignConversionDomain>>?
        RuntimeOptimizerBatchObserverForTesting
    {
        get => RuntimeOptimizerBatchObserver.Value;
        set => RuntimeOptimizerBatchObserver.Value = value;
    }
    private const string HandoffEvidence = "DesignConversion:DpoTrainingHandoff";
    private const double LocalAblationScoreDeltaThreshold = 0.10;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    [Obsolete(
        "Use the overload that requires ServerOwnedLegalTrainingTeacherAdmissionAuthority.",
        error: false)]
    public static Task<OperationResult<DesignConversionDpoTrainingHandoffResult>> ExecuteAsync(
        IEnumerable<DesignConversionDpoTrainingRecord>? records,
        IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
        BpeTokenizer tokenizer,
        string checkpointDirectory,
        LoraConfig? loraConfig = null,
        int maxStepCount = 1,
        CancellationToken cancellationToken = default)
        => Task.FromResult(cancellationToken.IsCancellationRequested
            ? OperationResult<DesignConversionDpoTrainingHandoffResult>.Fail(
                "DesignConversion DPO training handoff cancelled.",
                "CANCELLED")
            : LegalTeacherConfigurationRequired<DesignConversionDpoTrainingHandoffResult>());

    public static Task<OperationResult<DesignConversionDpoTrainingHandoffResult>> ExecuteAsync(
        ServerOwnedLegalTrainingTeacherAdmissionAuthority legalTeacherAdmissionAuthority,
        IEnumerable<DesignConversionDpoTrainingRecord>? records,
        IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
        BpeTokenizer tokenizer,
        string checkpointDirectory,
        LoraConfig? loraConfig = null,
        int maxStepCount = 1,
        CancellationToken cancellationToken = default)
        => ExecuteWithAuthorityAsync(
            legalTeacherAdmissionAuthority,
            records,
            policyBaseProvider,
            tokenizer,
            checkpointDirectory,
            loraConfig,
            maxStepCount,
            cancellationToken);

    internal static async Task<OperationResult<DesignConversionDpoTrainingHandoffResult>> ExecuteWithAuthorityAsync(
        ServerOwnedLegalTrainingTeacherAdmissionAuthority? legalTeacherAdmissionAuthority,
        IEnumerable<DesignConversionDpoTrainingRecord>? records,
        IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
        BpeTokenizer tokenizer,
        string checkpointDirectory,
        LoraConfig? loraConfig = null,
        int maxStepCount = 1,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<DesignConversionDpoTrainingHandoffResult>.Fail(
                "DesignConversion DPO training handoff cancelled.",
                "CANCELLED");
        if (legalTeacherAdmissionAuthority is null ||
            !legalTeacherAdmissionAuthority.IsAvailable)
        {
            return LegalTeacherConfigurationRequired<DesignConversionDpoTrainingHandoffResult>();
        }
        if (tokenizer is null)
            return OperationResult<DesignConversionDpoTrainingHandoffResult>.Fail("Tokenizer is required.", "DESIGN_CONVERSION_DPO_TOKENIZER_REQUIRED");
        if (string.IsNullOrWhiteSpace(checkpointDirectory))
            return OperationResult<DesignConversionDpoTrainingHandoffResult>.Fail("Checkpoint directory is required.", "DESIGN_CONVERSION_DPO_CHECKPOINT_REQUIRED");

        DesignConversionLoraCheckpointConfiguration carriedLoraConfiguration;
        try
        {
            carriedLoraConfiguration = new DesignConversionLoraCheckpointConfiguration(
                loraConfig ?? new LoraConfig());
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<DesignConversionDpoTrainingHandoffResult>.Fail(
                exception.Message,
                "DESIGN_CONVERSION_DPO_LORA_CONFIG_INVALID");
        }

        var sourceRowsSnapshot = SnapshotTrainingRows(records);
        if (!sourceRowsSnapshot.IsSuccess || sourceRowsSnapshot.Value is null)
            return sourceRowsSnapshot.MapError<DesignConversionDpoTrainingHandoffResult>();
        var sourceRows = sourceRowsSnapshot.Value;
        if (sourceRows.Length == 0)
            return OperationResult<DesignConversionDpoTrainingHandoffResult>.Fail("At least one DesignConversion DPO training record is required.", "DESIGN_CONVERSION_DPO_RECORDS_REQUIRED");

        var trainableRows = sourceRows
            .Where(IsTrainableRow)
            .ToArray();
        if (trainableRows.Length == 0)
            return OperationResult<DesignConversionDpoTrainingHandoffResult>.Fail("No DesignConversion DPO training rows are allowed for local handoff.", "DESIGN_CONVERSION_DPO_NO_TRAINABLE_ROWS");

        var finalPreferences = trainableRows
            .Select(BuildPreference)
            .ToArray();
        var preparedExecution = LogicalDpoTrainingUnit.PrepareExecution(
            finalPreferences,
            tokenizer);
        if (!preparedExecution.IsSuccess || preparedExecution.Value is null)
        {
            return preparedExecution
                .MapError<DesignConversionDpoTrainingHandoffResult>();
        }
        var finalOptimizer = DesignConversionDpoTrainingContentAuthority
            .ProjectFinalOptimizerInputs(
                preparedExecution.Value.OptimizerInputs,
                trainableRows
                    .Select(static row => row.TeacherProvenance)
                    .ToArray(),
                static input => input.Prompt,
                static input => input.Chosen,
                static input => input.Rejected,
                static input => input.TeachingVector,
                static _ => null,
                static _ =>
                    "design-conversion-handoff-logical-dpo-v1");
        if (!finalOptimizer.IsSuccess || finalOptimizer.Value is null)
            return finalOptimizer
                .MapError<DesignConversionDpoTrainingHandoffResult>();
        if (finalOptimizer.Value.Inputs.Count !=
                preparedExecution.Value.OptimizerInputs.Count ||
            finalOptimizer.Value.Inputs
                .Where((input, index) => !ReferenceEquals(
                    input,
                    preparedExecution.Value.OptimizerInputs[index]))
                .Any())
        {
            return OperationResult<
                DesignConversionDpoTrainingHandoffResult>.Fail(
                "The exact prepared DPO optimizer inputs changed before admission.",
                BoundedDpoTrainingContentProjector.InvalidErrorCode);
        }
        var leaseResult = legalTeacherAdmissionAuthority.AcquireTrainingLease(
            finalOptimizer.Value.Content,
            finalOptimizer.Value.Provenance);
        if (!leaseResult.IsSuccess || leaseResult.Value is null)
            return leaseResult.MapError<DesignConversionDpoTrainingHandoffResult>();
        var lease = leaseResult.Value;
        try
        {
            TrainingLeaseObserverForTesting?.Invoke(lease);
            TrainingRowsObserverForTesting?.Invoke(sourceRows);
            var legalTeacherAdmission = SnapshotAdmission(lease.Admission);
            var canonicalLoraConfig = carriedLoraConfiguration.ToLoraConfig();
            var qualityGate = EvaluateLocalAblationQualityGate(trainableRows);
            if (!qualityGate.PassesQualityGate)
            {
                var finalPermit = AcquireMutationPermit(lease, "execute_quality_final");
                if (!finalPermit.IsSuccess || finalPermit.Value is null)
                    return finalPermit.MapError<DesignConversionDpoTrainingHandoffResult>();
                using (finalPermit.Value)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return CancelledHandoff();
                    return OperationResult<DesignConversionDpoTrainingHandoffResult>.Ok(new DesignConversionDpoTrainingHandoffResult(
                        sourceRows.Length,
                        trainableRows.Length,
                        DpoStepCount: 0,
                        MetricFilePaths: [],
                        PolicyCheckpointFilePaths: [],
                        IngredientIds: [],
                        LoraConfiguration: carriedLoraConfiguration,
                        QualityGate: qualityGate,
                        LegalTeacherAdmission: legalTeacherAdmission,
                        EvidenceRefs: qualityGate.EvidenceRefs
                            .Concat(legalTeacherAdmission.EvidenceRefs)
                            .Append(HandoffEvidence)
                            .Distinct(StringComparer.Ordinal)
                            .ToArray()));
                }
            }

            if (policyBaseProvider is null)
                return PolicyBaseProviderRequired<DesignConversionDpoTrainingHandoffResult>();

            var policyBase = await CreateAndValidatePolicyBaseAsync(
                policyBaseProvider,
                lease,
                cancellationToken);
            var policyBoundaryPermit = AcquireMutationPermit(lease, "execute_policy_result");
            if (!policyBoundaryPermit.IsSuccess || policyBoundaryPermit.Value is null)
                return policyBoundaryPermit.MapError<DesignConversionDpoTrainingHandoffResult>();
            using (policyBoundaryPermit.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                    return CancelledHandoff();
                if (!policyBase.IsSuccess)
                    return policyBase.MapError<DesignConversionDpoTrainingHandoffResult>();
            }

            return await ExecuteTrainingAsync(
                sourceRows,
                trainableRows,
                finalPreferences,
                preparedExecution.Value,
                policyBase.Value!.ModelSet.Policy,
                policyBase.Value.ModelSet.Reference,
                tokenizer,
                checkpointDirectory,
                carriedLoraConfiguration,
                canonicalLoraConfig,
                policyBase.Value.ModelSet.RandomContext!,
                qualityGate,
                legalTeacherAdmission,
                lease,
                maxStepCount,
                cancellationToken);
        }
        finally
        {
            lease.Dispose();
        }
    }

    private static async Task<OperationResult<DesignConversionDpoTrainingHandoffResult>> ExecuteTrainingAsync(
        DesignConversionDpoTrainingRecord[] sourceRows,
        DesignConversionDpoTrainingRecord[] trainableRows,
        LogicalDpoPreferenceExample[] preferences,
        LogicalDpoPreparedExecution preparedExecution,
        LlmModel policyModel,
        LlmModel referenceModel,
        BpeTokenizer tokenizer,
        string checkpointDirectory,
        DesignConversionLoraCheckpointConfiguration carriedLoraConfiguration,
        LoraConfig canonicalLoraConfig,
        DesignConversionDpoPolicyRandomContext randomContext,
        DesignConversionDpoTrainingHandoffAblationGate qualityGate,
        LegalTrainingTeacherAdmission legalTeacherAdmission,
        ServerOwnedLegalTrainingTeacherAdmissionLease lease,
        int maxStepCount,
        CancellationToken cancellationToken)
    {
        var admissionCapabilities = lease.IssueAdmissionCapabilities();
        if (!admissionCapabilities.IsSuccess ||
            admissionCapabilities.Value is null)
        {
            return admissionCapabilities
                .MapError<DesignConversionDpoTrainingHandoffResult>();
        }

        var request = new LogicalDpoTrainingRequest(
            policyModel,
            referenceModel,
            tokenizer,
            checkpointDirectory,
            preferences,
            canonicalLoraConfig,
            MaxStepCount: Math.Max(1, maxStepCount),
            AdmissionCapabilities: admissionCapabilities.Value)
        {
            PreparedExecution = preparedExecution,
            AdapterSeedAuthority = new LogicalDpoAdapterSeedAuthority(
                randomContext.RootInitializationSeed,
                randomContext.PolicyDropoutStreamDomain,
                randomContext.ReferenceDropoutStreamDomain)
        };
        TrainingRequestObserverForTesting?.Invoke(request);

        OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>> unitResult;
        var logicalPermit = AcquireMutationPermit(lease, "execute_logical");
        if (!logicalPermit.IsSuccess || logicalPermit.Value is null)
            return logicalPermit.MapError<DesignConversionDpoTrainingHandoffResult>();
        using (logicalPermit.Value)
        {
            if (cancellationToken.IsCancellationRequested)
                return CancelledHandoff();
            unitResult = await new LogicalDpoTrainingUnit().ExecuteAsync(
                new LogicalUnitRequest<LogicalDpoTrainingRequest>(
                    $"design-conversion-dpo-handoff-{Guid.NewGuid():N}",
                    LogicalDpoTrainingUnit.Id,
                    LogicalDpoTrainingUnit.Role,
                    LogicalDpoTrainingUnit.DomainTag,
                    request,
                    new LogicalExecutionFlags(),
                    new Dictionary<string, object>
                    {
                        ["design_conversion_dpo_record_count"] = sourceRows.Length,
                        ["design_conversion_dpo_preference_count"] = preferences.Length,
                        ["legal_teacher_admission"] = BuildLegalTeacherMetadata(legalTeacherAdmission),
                        ["boundary"] = HandoffEvidence
                    }),
                cancellationToken);
        }

        var finalPermit = AcquireMutationPermit(lease, "execute_final");
        if (!finalPermit.IsSuccess || finalPermit.Value is null)
            return finalPermit.MapError<DesignConversionDpoTrainingHandoffResult>();
        using (finalPermit.Value)
        {
            if (cancellationToken.IsCancellationRequested)
                return CancelledHandoff();
            if (!unitResult.IsSuccess)
                return unitResult.MapError<DesignConversionDpoTrainingHandoffResult>();

            var output = unitResult.Value!.Output;
            var ingredientIds = output.IngredientIds.Distinct(StringComparer.Ordinal).ToArray();
            var evidenceRefs = new HashSet<string>(StringComparer.Ordinal)
            {
                HandoffEvidence,
                LogicalDpoTrainingUnit.Id
            };
            foreach (var evidenceId in unitResult.Value.EvidenceIds)
                evidenceRefs.Add(evidenceId);
            foreach (var stepVectorIngredient in ingredientIds.Where(DesignConversionDpoTrainerRouteBGate.IsStepVectorIngredient))
                evidenceRefs.Add(stepVectorIngredient);
            foreach (var qualityEvidence in qualityGate.EvidenceRefs)
                evidenceRefs.Add(qualityEvidence);
            foreach (var legalTeacherEvidence in legalTeacherAdmission.EvidenceRefs)
                evidenceRefs.Add(legalTeacherEvidence);
            var policyCheckpointFilePaths = FindPolicyCheckpointFilePaths(checkpointDirectory);
            foreach (var policyCheckpointFilePath in policyCheckpointFilePaths)
                evidenceRefs.Add($"dpo_policy_checkpoint:{Path.GetFileName(policyCheckpointFilePath)}");
            var routeBGate = DesignConversionDpoTrainerRouteBGate.ValidateTrainingHandoff(
                output.DpoStepCount,
                ingredientIds,
                policyCheckpointFilePaths);
            if (!routeBGate.IsSuccess)
                return routeBGate.MapError<DesignConversionDpoTrainingHandoffResult>();
            evidenceRefs.Add(DesignConversionDpoTrainerRouteBGate.GateId);

            return OperationResult<DesignConversionDpoTrainingHandoffResult>.Ok(new DesignConversionDpoTrainingHandoffResult(
                sourceRows.Length,
                output.PreferenceCount,
                output.DpoStepCount,
                output.MetricFilePaths,
                policyCheckpointFilePaths,
                ingredientIds,
                carriedLoraConfiguration,
                qualityGate,
                legalTeacherAdmission,
                evidenceRefs.ToArray()));
        }
    }

    [Obsolete(
        "Use the overload that requires ServerOwnedLegalTrainingTeacherAdmissionAuthority.",
        error: false)]
    public static Task<OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>> ConvertPolicyCheckpointToRuntimeCheckpointAsync(
        DesignConversionDpoTrainingRecord record,
        string policyCheckpointPath,
        IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
        DesignConversionLoraCheckpointConfiguration? loraConfiguration,
        string runtimeCheckpointPath,
        CancellationToken cancellationToken = default)
        => Task.FromResult(cancellationToken.IsCancellationRequested
            ? OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                "Bridge cancelled.",
                "CANCELLED")
            : LegalTeacherConfigurationRequired<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>());

    public static Task<OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>> ConvertPolicyCheckpointToRuntimeCheckpointAsync(
        ServerOwnedLegalTrainingTeacherAdmissionAuthority legalTeacherAdmissionAuthority,
        DesignConversionDpoTrainingRecord record,
        string policyCheckpointPath,
        IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
        DesignConversionLoraCheckpointConfiguration? loraConfiguration,
        string runtimeCheckpointPath,
        CancellationToken cancellationToken = default)
        => ConvertWithAuthorityAsync(
            legalTeacherAdmissionAuthority,
            record,
            policyCheckpointPath,
            policyBaseProvider,
            loraConfiguration,
            runtimeCheckpointPath,
            cancellationToken);

    internal static async Task<OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>> ConvertWithAuthorityAsync(
        ServerOwnedLegalTrainingTeacherAdmissionAuthority? legalTeacherAdmissionAuthority,
        DesignConversionDpoTrainingRecord record,
        string policyCheckpointPath,
        IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
        DesignConversionLoraCheckpointConfiguration? loraConfiguration,
        string runtimeCheckpointPath,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail("Bridge cancelled.", "CANCELLED");
        if (legalTeacherAdmissionAuthority is null ||
            !legalTeacherAdmissionAuthority.IsAvailable)
        {
            return LegalTeacherConfigurationRequired<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        }
        if (string.IsNullOrWhiteSpace(policyCheckpointPath))
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail("Policy checkpoint path is required.", "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REQUIRED");
        if (loraConfiguration is null)
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                "Carried LoRA checkpoint configuration is required.",
                "DESIGN_CONVERSION_DPO_POLICY_LORA_CONFIG_REQUIRED");
        if (string.IsNullOrWhiteSpace(runtimeCheckpointPath))
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail("Runtime checkpoint path is required.", "DESIGN_CONVERSION_RUNTIME_CHECKPOINT_REQUIRED");
        if (record is null)
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail("DPO training record is required.", "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_RECORD_REQUIRED");
        var recordSnapshot = SnapshotTrainingRow(record);
        if (!recordSnapshot.IsSuccess || recordSnapshot.Value is null)
            return recordSnapshot.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        var ownedRecord = recordSnapshot.Value;
        if (!IsTrainableRow(ownedRecord))
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail("DPO training record is not trainable for policy-runtime bridge.", "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_RECORD_NOT_TRAINABLE");
        if (!File.Exists(policyCheckpointPath))
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail($"Policy checkpoint not found: {policyCheckpointPath}", "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_NOT_FOUND");
        var preparedPayloadResult = DesignConversionDpoRuntimeCheckpointPublisher
            .ReadPolicyPayload(policyCheckpointPath, cancellationToken);
        if (!preparedPayloadResult.IsSuccess ||
            preparedPayloadResult.Value is null)
        {
            return preparedPayloadResult.MapError<
                DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        }
        if (preparedPayloadResult.Value.Length == 0)
        {
            return OperationResult<
                DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                "Policy checkpoint payload is empty.",
                "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_EMPTY");
        }
        var preparedPolicyCheckpointPayload = preparedPayloadResult.Value;
        var preparedPolicyCheckpointSha256 = ComputeSha256(
            preparedPolicyCheckpointPayload);
        var preparedRuntimeBatch = BuildPolicyRuntimeBridgeBatch(
            ownedRecord,
            policyCheckpointPath,
            preparedPolicyCheckpointSha256);
        var finalOptimizer = DesignConversionDpoTrainingContentAuthority
            .ProjectFinalOptimizerInputs(
                preparedRuntimeBatch.TrainingItems,
                [ownedRecord.TeacherProvenance],
                static item => item.Prompt,
                static item => item.ChosenResponse,
                static item => item.RejectedResponse,
                static _ => null,
                static _ => null,
                static item => string.Concat(
                    "design-conversion-policy-runtime-learning-item-v1|item=",
                    item.ItemId));
        if (!finalOptimizer.IsSuccess || finalOptimizer.Value is null)
            return finalOptimizer.MapError<
                DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        RuntimeOptimizerBatchObserverForTesting?.Invoke(preparedRuntimeBatch);
        var leaseResult = legalTeacherAdmissionAuthority.AcquireTrainingLease(
            finalOptimizer.Value.Content,
            finalOptimizer.Value.Provenance);
        if (!leaseResult.IsSuccess || leaseResult.Value is null)
            return leaseResult.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        var lease = leaseResult.Value;
        try
        {
            TrainingLeaseObserverForTesting?.Invoke(lease);
            var machineTrainingAdmission =
                DesignConversionMachineTrainingAdmission.Mint(
                    finalOptimizer.Value,
                    preparedRuntimeBatch,
                    lease);
            if (!machineTrainingAdmission.IsSuccess ||
                machineTrainingAdmission.Value is null)
            {
                return machineTrainingAdmission.MapError<
                    DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            }
            var legalTeacherAdmission = SnapshotAdmission(lease.Admission);
            var legalTeacherAdmissionCore =
                DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.CaptureAdmission(
                    legalTeacherAdmission);
            if (!legalTeacherAdmissionCore.IsSuccess || legalTeacherAdmissionCore.Value is null)
                return legalTeacherAdmissionCore.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            if (policyBaseProvider is null)
                return PolicyBaseProviderRequired<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();

            var policyBase = await CreateAndValidatePolicyBaseAsync(
                policyBaseProvider,
                lease,
                cancellationToken);
            var policyBoundaryPermit = AcquireMutationPermit(lease, "convert_policy_result");
            if (!policyBoundaryPermit.IsSuccess || policyBoundaryPermit.Value is null)
                return policyBoundaryPermit.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            using (policyBoundaryPermit.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Cancelled<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
                if (!policyBase.IsSuccess)
                    return policyBase.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            }

            var freshLoadTarget = policyBase.Value!.ModelSet.FreshLoadTarget;
            int loraAdapterCount;
            string initialWeightSha256;
            var loraPermit = AcquireMutationPermit(lease, "convert_lora");
            if (!loraPermit.IsSuccess || loraPermit.Value is null)
                return loraPermit.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            using (loraPermit.Value)
            {
                try
                {
                    var canonicalLoraConfig = LoraApplicator.ValidateAndCanonicalize(
                        loraConfiguration.ToLoraConfig());
                    var randomContext = policyBase.Value.ModelSet.RandomContext!;
                    LoraApplicator.ValidateFreshApplication(
                        freshLoadTarget,
                        canonicalLoraConfig);
                    var attached = LoraApplicator.Apply(
                        freshLoadTarget,
                        canonicalLoraConfig,
                        (blockIndex, projection) =>
                            LoraAdapterRandomSeedDerivation.Derive(
                                randomContext.RootInitializationSeed,
                                blockIndex,
                                projection,
                                randomContext.FreshLoadDropoutStreamDomain));
                    if (attached.Count == 0)
                    {
                        return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                            "Carried LoRA checkpoint configuration attached no live adapters.",
                            "DESIGN_CONVERSION_DPO_POLICY_LORA_ATTACH_FAILED");
                    }

                    loraAdapterCount = attached.Count;
                    initialWeightSha256 =
                        DesignConversionDpoPolicyBaseMetadata
                            .ComputeInitialWeightSha256(freshLoadTarget);
                    if (string.Equals(
                            policyBase.Value.ModelSet.Identity.BaseCheckpointSha256,
                            initialWeightSha256,
                            StringComparison.Ordinal))
                    {
                        return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                            "DesignConversion DPO policy base model set is invalid or incompatible.",
                            "DESIGN_CONVERSION_DPO_POLICY_BASE_MODEL_SET_INVALID");
                    }
                }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                        "Carried LoRA checkpoint configuration is invalid or unsupported.",
                        "DESIGN_CONVERSION_DPO_POLICY_LORA_ATTACH_FAILED");
                }
            }

            OperationResult<byte[]> freshLoadResult;
            var freshLoadPermit = AcquireMutationPermit(lease, "convert_fresh_load");
            if (!freshLoadPermit.IsSuccess || freshLoadPermit.Value is null)
                return freshLoadPermit.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            using (freshLoadPermit.Value)
            {
                var candidatePayload = preparedPolicyCheckpointPayload;
                if (candidatePayload.Length == 0)
                {
                    freshLoadResult = OperationResult<byte[]>.Fail(
                        "Policy checkpoint payload is empty.",
                        "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_EMPTY");
                }
                else
                {
                    try
                    {
                        PolicyCheckpointFreshLoadObserverForTesting?.Invoke();
                        using var checkpointStream = new MemoryStream(
                            candidatePayload,
                            writable: false);
                        freshLoadTarget.LoadCheckpoint(checkpointStream);
                        freshLoadResult = OperationResult<byte[]>.Ok(
                            candidatePayload);
                    }
                    catch (OperationCanceledException)
                        when (cancellationToken.IsCancellationRequested)
                    {
                        freshLoadResult = Cancelled<byte[]>();
                    }
                    catch (Exception exception)
                        when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                    {
                        freshLoadResult = OperationResult<byte[]>.Fail(
                            "Policy checkpoint fresh-load failed.",
                            "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_FRESH_LOAD_FAILED");
                    }
                }
            }

            var freshLoadBoundaryPermit = AcquireMutationPermit(
                lease,
                "convert_fresh_load_result");
            if (!freshLoadBoundaryPermit.IsSuccess || freshLoadBoundaryPermit.Value is null)
            {
                return freshLoadBoundaryPermit
                    .MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            }

            byte[] policyCheckpointPayload;
            using (freshLoadBoundaryPermit.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Cancelled<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
                if (!freshLoadResult.IsSuccess || freshLoadResult.Value is null)
                {
                    return freshLoadResult
                        .MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
                }

                policyCheckpointPayload = freshLoadResult.Value;
            }

            var policyCheckpointSha256 = preparedPolicyCheckpointSha256;
            var policyCheckpointFileName = Path.GetFileName(policyCheckpointPath);
            if (string.IsNullOrWhiteSpace(policyCheckpointFileName))
                return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                    "Policy checkpoint sibling name is invalid.",
                    "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REQUIRED");
            var dpoStepCount = ParseDpoStepCountFromFileName(policyCheckpointPath);
            var loraContext = BuildLoraContext(loraConfiguration, loraAdapterCount);
            var machine = new DesignConversionMachine();
            MutationBoundaryObserverForTesting?.Invoke("convert_train");
            var train = await machine.TrainAdmittedBatchAsync(
                machineTrainingAdmission.Value,
                cancellationToken);

            DesignConversionMachineCheckpoint checkpointValue;
            string[] bridgeEvidenceRefs;
            DesignConversionDpoPolicyCheckpointRef policyCheckpointRef;
            var trainBoundaryPermit = AcquireMutationPermit(lease, "convert_train_result");
            if (!trainBoundaryPermit.IsSuccess || trainBoundaryPermit.Value is null)
                return trainBoundaryPermit.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            using (trainBoundaryPermit.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Cancelled<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
                if (!train.IsSuccess)
                    return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(train.ErrorMessage, train.ErrorCode);

                var checkpoint = machine.ExportCheckpoint();
                if (!checkpoint.IsSuccess)
                    return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(checkpoint.ErrorMessage, checkpoint.ErrorCode);
                checkpointValue = checkpoint.Value!;

                bridgeEvidenceRefs = BuildBridgeEvidenceRefs(
                    sourceRecordCount: 1,
                    bridgePreferenceCount: 1,
                    dpoStepCount,
                    policyCheckpointPath,
                    policyCheckpointSha256,
                    policyBase.Value.ModelSet.Identity,
                    policyBase.Value.ConfigSha256,
                    initialWeightSha256,
                    loraContext,
                    policyBase.Value.ModelSet.RandomContext!)
                    .Concat(legalTeacherAdmission.EvidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray();
                var policyCheckpointRefResult = BuildPolicyCheckpointRef(
                    sourceRecordCount: 1,
                    bridgePreferenceCount: 1,
                    dpoStepCount,
                    policyCheckpointFileName,
                    policyCheckpointSha256,
                    policyCheckpointPayload.Length,
                    policyBase.Value.ModelSet.Policy.Config,
                    policyBase.Value.ModelSet.Identity,
                    policyBase.Value.ConfigSha256,
                    initialWeightSha256,
                    loraContext,
                    policyBase.Value.ModelSet.RandomContext!,
                    bridgeEvidenceRefs,
                    legalTeacherAdmissionCore.Value);
                if (!policyCheckpointRefResult.IsSuccess || policyCheckpointRefResult.Value is null)
                    return policyCheckpointRefResult.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
                policyCheckpointRef = policyCheckpointRefResult.Value;
                var preparedRouteBGate = DesignConversionDpoTrainerRouteBGate
                    .ValidateTypedRuntimeCheckpoint(
                        policyCheckpointRef,
                        dpoStepCount,
                        bridgeEvidenceRefs,
                        callerFreshLoadVerified: true);
                if (!preparedRouteBGate.IsSuccess)
                    return preparedRouteBGate.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            }
            var bridgedCheckpoint = AddDpoPolicyCheckpointEvidence(
                checkpointValue,
                bridgeEvidenceRefs,
                policyCheckpointRef);

            OperationResult<DesignConversionMachineLoadResult> published;
            var publicationPermit = AcquireMutationPermit(lease, "convert_publish");
            if (!publicationPermit.IsSuccess || publicationPermit.Value is null)
                return publicationPermit.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            using (publicationPermit.Value)
            {
                published = await PublishRuntimeTransactionAsync(
                    policyCheckpointPath,
                    policyCheckpointFileName,
                    policyCheckpointPayload,
                    runtimeCheckpointPath,
                    bridgedCheckpoint,
                    dpoStepCount,
                    bridgeEvidenceRefs,
                    cancellationToken);
            }

            var finalPermit = AcquireMutationPermit(lease, "convert_final");
            if (!finalPermit.IsSuccess || finalPermit.Value is null)
                return finalPermit.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            using (finalPermit.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Cancelled<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
                if (!published.IsSuccess || published.Value is null)
                    return published.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
                var loaded = published.Value;

                var runtimeExport = new LearningMachineExport(
                    MachineId: DesignConversionMachine.MachineId,
                    DomainTag: DesignConversionMachine.DomainTag,
                    Version: loaded.Checkpoint.Version,
                    CheckpointRef: Path.GetFileName(runtimeCheckpointPath),
                    ManifestRef: $"{DesignConversionMachine.MachineId}/{loaded.Checkpoint.Version}/manifest.json",
                    AccuracyAtExport: loaded.Checkpoint.AccuracyAfterLastBatch,
                    ExportedAtUtc: loaded.Checkpoint.ExportedAtUtc);

                return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Ok(
                    new DesignConversionDpoPolicyRuntimeCheckpointBridgeResult(
                        PolicyCheckpointPath: policyCheckpointFileName,
                        PolicyCheckpointSha256: policyCheckpointSha256,
                        RuntimeCheckpointPath: Path.GetFileName(runtimeCheckpointPath),
                        RuntimeExport: runtimeExport,
                        RuntimeCheckpoint: loaded.Checkpoint,
                        PolicyCheckpointFreshLoadVerified: true,
                        LoraConfiguration: loraConfiguration,
                        EvidenceRefs: bridgeEvidenceRefs
                            .Concat(loaded.Checkpoint.EvidenceRefs)
                            .Distinct(StringComparer.Ordinal)
                            .Order(StringComparer.Ordinal)
                            .ToArray(),
                        PolicyCheckpointRef: loaded.Checkpoint.DpoPolicyCheckpointRef));
            }
        }
        finally
        {
            lease.Dispose();
        }
    }

    private sealed record HandoffValidatedPolicyBase(
        DesignConversionDpoPolicyBaseModelSet ModelSet,
        string ConfigSha256);

    private static async Task<OperationResult<HandoffValidatedPolicyBase>> CreateAndValidatePolicyBaseAsync(
        IDesignConversionDpoPolicyBaseProvider policyBaseProvider,
        ServerOwnedLegalTrainingTeacherAdmissionLease lease,
        CancellationToken cancellationToken)
    {
        var permit = AcquireMutationPermit(lease, "policy_provider_and_validation");
        if (!permit.IsSuccess || permit.Value is null)
            return permit.MapError<HandoffValidatedPolicyBase>();
        using var mutationPermit = permit.Value;
        OperationResult<DesignConversionDpoPolicyBaseModelSet> provided;
        try
        {
            provided = await policyBaseProvider.CreateModelSetAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<HandoffValidatedPolicyBase>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<HandoffValidatedPolicyBase>.Fail(
                "DesignConversion DPO policy base provider failed.",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_FAILED");
        }

        if (cancellationToken.IsCancellationRequested ||
            !provided.IsSuccess && string.Equals(provided.ErrorCode, "CANCELLED", StringComparison.Ordinal))
        {
            return Cancelled<HandoffValidatedPolicyBase>();
        }

        if (!provided.IsSuccess && string.Equals(
                provided.ErrorCode,
                ServerOwnedLegalTrainingTeacherAdmissionAuthority.AdmissionRevokedErrorCode,
                StringComparison.Ordinal))
        {
            return provided.MapError<HandoffValidatedPolicyBase>();
        }

        if (!provided.IsSuccess)
            return OperationResult<HandoffValidatedPolicyBase>.Fail(
                "DesignConversion DPO policy base provider failed.",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_FAILED");

        return ValidatePolicyBase(provided.Value);
    }

    private static OperationResult<HandoffValidatedPolicyBase> ValidatePolicyBase(
        DesignConversionDpoPolicyBaseModelSet? modelSet)
    {
        if (modelSet is null ||
            modelSet.Policy is null ||
            modelSet.Reference is null ||
            modelSet.FreshLoadTarget is null)
        {
            return PolicyBaseModelSetInvalid();
        }

        if (modelSet.RandomContext is null)
        {
            return OperationResult<HandoffValidatedPolicyBase>.Fail(
                "DesignConversion DPO policy random context is required.",
                DesignConversionDpoPolicyCheckpointBridge
                    .PolicyRandomContextRequiredErrorCode);
        }

        if (!DesignConversionDpoPolicyRandomContextContract.IsValidCurrent(
                modelSet.RandomContext))
        {
            return OperationResult<HandoffValidatedPolicyBase>.Fail(
                "DesignConversion DPO policy random context is invalid.",
                DesignConversionDpoPolicyCheckpointBridge
                    .PolicyRandomContextInvalidErrorCode);
        }

        // Источник модели и серверная публикация используют одну общую границу:
        // до создания артефактов наружу проходит только безопасная логическая идентичность.
        if (!DesignConversionDpoPolicyBaseMetadata.IsValidIdentity(modelSet.Identity))
            return OperationResult<HandoffValidatedPolicyBase>.Fail(
                "DesignConversion DPO policy base identity is invalid.",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_INVALID");

        try
        {
            if (ReferenceEquals(modelSet.Policy, modelSet.Reference) ||
                ReferenceEquals(modelSet.Policy, modelSet.FreshLoadTarget) ||
                ReferenceEquals(modelSet.Reference, modelSet.FreshLoadTarget) ||
                !IsValidPolicyBaseConfig(modelSet.Policy.Config) ||
                !PolicyBaseConfigsEqual(modelSet.Policy.Config, modelSet.Reference.Config) ||
                !PolicyBaseConfigsEqual(modelSet.Policy.Config, modelSet.FreshLoadTarget.Config))
            {
                return PolicyBaseModelSetInvalid();
            }

            var policyEntries = modelSet.Policy.GetCheckpointParameterEntries();
            var referenceEntries = modelSet.Reference.GetCheckpointParameterEntries();
            var freshEntries = modelSet.FreshLoadTarget.GetCheckpointParameterEntries();
            if (policyEntries.Count == 0 ||
                !ParameterTopologiesMatch(policyEntries, referenceEntries) ||
                !ParameterTopologiesMatch(policyEntries, freshEntries) ||
                !ParametersAreExclusive(policyEntries, referenceEntries, freshEntries) ||
                !AllParameterValuesAreFinite(policyEntries) ||
                !AllParameterValuesAreFinite(referenceEntries) ||
                !AllParameterValuesAreFinite(freshEntries) ||
                !ParameterValuesBitwiseEqual(policyEntries, referenceEntries) ||
                !ParameterValuesBitwiseEqual(policyEntries, freshEntries))
            {
                return PolicyBaseModelSetInvalid();
            }

            return OperationResult<HandoffValidatedPolicyBase>.Ok(new HandoffValidatedPolicyBase(
                modelSet,
                ComputeConfigSha256(modelSet.Policy.Config)));
        }
        catch (Exception exception)
            when ((exception is InvalidOperationException
                   or ArgumentException
                   or OverflowException) &&
                  SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return PolicyBaseModelSetInvalid();
        }
    }

    private static bool IsValidPolicyBaseConfig(ModelConfig? config)
        => config is not null &&
           config.VocabSize >= 256 &&
           config.ContextLen >= 2 &&
           config.HiddenDim > 0 &&
           config.NumLayers > 0 &&
           config.NumHeads > 0 &&
           config.NumKvHeads > 0 &&
           config.FfnHiddenDim > 0 &&
           config.HiddenDim % config.NumHeads == 0 &&
           config.NumHeads % config.NumKvHeads == 0 &&
           float.IsFinite(config.RopeTheta) &&
           config.RopeTheta > 0 &&
           float.IsFinite(config.RmsNormEps) &&
           config.RmsNormEps > 0;

    private static bool PolicyBaseConfigsEqual(ModelConfig? left, ModelConfig? right)
        => left is not null &&
           right is not null &&
           left.VocabSize == right.VocabSize &&
           left.ContextLen == right.ContextLen &&
           left.HiddenDim == right.HiddenDim &&
           left.NumLayers == right.NumLayers &&
           left.NumHeads == right.NumHeads &&
           left.NumKvHeads == right.NumKvHeads &&
           left.FfnHiddenDim == right.FfnHiddenDim &&
           BitConverter.SingleToInt32Bits(left.RopeTheta) == BitConverter.SingleToInt32Bits(right.RopeTheta) &&
           BitConverter.SingleToInt32Bits(left.RmsNormEps) == BitConverter.SingleToInt32Bits(right.RmsNormEps);

    private static bool ParameterTopologiesMatch(
        IReadOnlyList<LlmCheckpointParameterEntry> expected,
        IReadOnlyList<LlmCheckpointParameterEntry> actual)
    {
        if (expected.Count != actual.Count)
            return false;

        for (var index = 0; index < expected.Count; index++)
        {
            if (!string.Equals(expected[index].Key, actual[index].Key, StringComparison.Ordinal) ||
                !expected[index].Parameter.Value.Shape.Dimensions.SequenceEqual(
                    actual[index].Parameter.Value.Shape.Dimensions))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ParametersAreExclusive(
        params IReadOnlyList<LlmCheckpointParameterEntry>[] modelEntries)
    {
        var parameters = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var tensors = new HashSet<object>(ReferenceEqualityComparer.Instance);
        foreach (var entries in modelEntries)
        {
            foreach (var entry in entries)
            {
                if (!parameters.Add(entry.Parameter) ||
                    !tensors.Add(entry.Parameter.Value) ||
                    !tensors.Add(entry.Parameter.Value.Storage))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool AllParameterValuesAreFinite(IReadOnlyList<LlmCheckpointParameterEntry> entries)
        => entries.All(entry => entry.Parameter.Value.Data.All(float.IsFinite));

    private static bool ParameterValuesBitwiseEqual(
        IReadOnlyList<LlmCheckpointParameterEntry> expected,
        IReadOnlyList<LlmCheckpointParameterEntry> actual)
    {
        if (!ParameterTopologiesMatch(expected, actual))
            return false;

        for (var entryIndex = 0; entryIndex < expected.Count; entryIndex++)
        {
            var expectedData = expected[entryIndex].Parameter.Value.Data;
            var actualData = actual[entryIndex].Parameter.Value.Data;
            if (expectedData.Length != actualData.Length)
                return false;

            for (var valueIndex = 0; valueIndex < expectedData.Length; valueIndex++)
            {
                if (BitConverter.SingleToInt32Bits(expectedData[valueIndex]) !=
                    BitConverter.SingleToInt32Bits(actualData[valueIndex]))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static string ComputeConfigSha256(ModelConfig config)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendHashString(hash, "design-conversion-dpo-policy-config-v1");
        AppendHashInt32(hash, config.VocabSize);
        AppendHashInt32(hash, config.ContextLen);
        AppendHashInt32(hash, config.HiddenDim);
        AppendHashInt32(hash, config.NumLayers);
        AppendHashInt32(hash, config.NumHeads);
        AppendHashInt32(hash, config.NumKvHeads);
        AppendHashInt32(hash, config.FfnHiddenDim);
        AppendHashInt32(hash, BitConverter.SingleToInt32Bits(config.RopeTheta));
        AppendHashInt32(hash, BitConverter.SingleToInt32Bits(config.RmsNormEps));
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void AppendHashString(IncrementalHash hash, string value)
    {
        var bytes = StrictUtf8.GetBytes(value);
        AppendHashInt32(hash, bytes.Length);
        hash.AppendData(bytes);
    }

    private static void AppendHashInt32(IncrementalHash hash, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        hash.AppendData(bytes);
    }

    private static OperationResult<T> PolicyBaseProviderRequired<T>()
        => OperationResult<T>.Fail(
            "DesignConversion DPO policy base provider is required.",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_REQUIRED");

    private static OperationResult<IDisposable> AcquireMutationPermit(
        ServerOwnedLegalTrainingTeacherAdmissionLease lease,
        string boundary)
    {
        MutationBoundaryObserverForTesting?.Invoke(boundary);
        return lease.AcquireMutationPermit();
    }

    private static OperationResult<HandoffValidatedPolicyBase> PolicyBaseModelSetInvalid()
        => OperationResult<HandoffValidatedPolicyBase>.Fail(
            "DesignConversion DPO policy base provider returned an invalid model set.",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_MODEL_SET_INVALID");

    private static OperationResult<T> Cancelled<T>()
        => OperationResult<T>.Fail(
            "DesignConversion DPO policy bridge cancelled.",
            "CANCELLED");

    private static OperationResult<DesignConversionDpoTrainingHandoffResult>
        CancelledHandoff()
        => OperationResult<DesignConversionDpoTrainingHandoffResult>.Fail(
            "DesignConversion DPO training handoff cancelled.",
            "CANCELLED");

    private static bool IsTrainableRow(DesignConversionDpoTrainingRecord row)
        => row.License.AllowsTraining
           && !row.License.ContainsUserPrivateData
           && !row.Quarantine.IsQuarantined;

    public static IReadOnlyDictionary<string, object> BuildLegalTeacherMetadata(
        LegalTrainingTeacherAdmission admission)
    {
        ArgumentNullException.ThrowIfNull(admission);
        var ownedAdmission = SnapshotAdmission(admission);
        return new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(
            new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["policy_identity_ref"] = ownedAdmission.PolicyIdentityRef,
                ["external_teacher_invocation_count"] = ownedAdmission.ExternalTeacherInvocationCount,
                ["training_content_batch_ref"] = ownedAdmission.TrainingContentBatchRef,
                ["receipts"] = ownedAdmission.Receipts,
                ["evidence_refs"] = ownedAdmission.EvidenceRefs
            });
    }

    private static LegalTrainingTeacherAdmission SnapshotAdmission(
        LegalTrainingTeacherAdmission admission)
        => new(
            admission.Receipts.Select(static receipt => receipt with { }).ToArray(),
            admission.ExternalTeacherInvocationCount,
            admission.PolicyIdentityRef,
            admission.EvidenceRefs.ToArray())
        {
            TrainingContentBatchRef = admission.TrainingContentBatchRef
        };

    private static OperationResult<DesignConversionDpoTrainingRecord> SnapshotTrainingRow(
        DesignConversionDpoTrainingRecord record)
        => DesignConversionDpoTrainingContentAuthority.CaptureRecord(record);

    private static OperationResult<DesignConversionDpoTrainingRecord[]> SnapshotTrainingRows(
        IEnumerable<DesignConversionDpoTrainingRecord>? records)
        => DesignConversionDpoTrainingContentAuthority.CaptureRecords(records);

    private static OperationResult<TResult> LegalTeacherConfigurationRequired<TResult>()
        => OperationResult<TResult>.Fail(
            "Server-owned legal teacher admission configuration is required.",
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode);

    private static string[] FindPolicyCheckpointFilePaths(string checkpointDirectory)
        => Directory.Exists(checkpointDirectory)
            ? Directory.GetFiles(
                    checkpointDirectory,
                    $"{DpoTrainer.PolicyCheckpointFilePrefix}*{DpoTrainer.PolicyCheckpointFileExtension}")
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray()
            : [];

    private static LearningBatch<global::DesignConversionDomain.DesignConversionDomain> BuildPolicyRuntimeBridgeBatch(
        DesignConversionDpoTrainingRecord record,
        string policyCheckpointPath,
        string policyCheckpointSha256)
    {
        var unitId = DesignConversionDpoUnits.TryNormalize(record.UnitId, out var normalizedUnitId)
            ? normalizedUnitId
            : record.UnitId;
        var optimizerRecord = record with { UnitId = unitId };
        var prompt = DesignConversionDpoDatasetBuilder.BuildOptimizerPromptPayload(
            optimizerRecord,
            DesignConversionMachine.DomainTag);
        var chosen = DesignConversionDpoDatasetBuilder.BuildOptimizerResponsePayload(record.Chosen);
        var rejected = DesignConversionDpoDatasetBuilder.BuildOptimizerResponsePayload(record.Rejected);

        return new LearningBatch<global::DesignConversionDomain.DesignConversionDomain>(
            BatchId: $"dpo-policy-runtime-bridge:{record.RecordId}",
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: $"{record.RecordId}:dpo-policy-runtime-bridge",
                    Prompt: prompt,
                    ChosenResponse: chosen,
                    RejectedResponse: rejected)
            ],
            SourceIds:
            [
                DesignConversionDpoTrainerRouteBGate.DerivationSourceSystem,
                $"dpo_policy_sha256:{policyCheckpointSha256}",
                $"dpo_policy_file:{Path.GetFileName(policyCheckpointPath)}",
                $"design_conversion_dpo_record:{record.RecordId}"
            ]);
    }

    private static IReadOnlyList<string> BuildBridgeEvidenceRefs(
        int sourceRecordCount,
        int bridgePreferenceCount,
        int dpoStepCount,
        string policyCheckpointPath,
        string policyCheckpointSha256,
        DesignConversionDpoPolicyBaseIdentity policyBaseIdentity,
        string configSha256,
        string initialWeightSha256,
        DesignConversionDpoPolicyCheckpointLoraContext? loraContext,
        DesignConversionDpoPolicyRandomContext randomContext)
    {
        var refs = new List<string>
        {
            DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence,
            DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence,
            "DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_SOURCE_11_DPO",
            "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PRODUCT_READY_FALSE",
            DesignConversionMachine.DpoPolicyDirectInferenceConfiguredEvidence,
            "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PAYLOAD_FILE_PERSISTED",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_INJECTED",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_VALIDATED",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_MODELS_DISTINCT",
            "DESIGN_CONVERSION_DPO_POLICY_INITIAL_POLICY_REFERENCE_BITWISE_EQUAL",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_AND_INITIAL_WEIGHT_HASHES_DISTINCT",
            DesignConversionDpoTrainerRouteBGate.GateId,
            DesignConversionDpoTrainerRouteBGate.TrainerSourceSystem,
            DesignConversionDpoTrainerRouteBGate.StepVectorIngredient,
            $"dpo_policy_checkpoint_sha256:{policyCheckpointSha256}",
            $"dpo_policy_checkpoint_file:{Path.GetFileName(policyCheckpointPath)}",
            $"dpo_policy_base_provider_kind:{policyBaseIdentity.ProviderKind}",
            $"dpo_policy_base_configuration_kind:{policyBaseIdentity.ConfigurationKind}",
            $"dpo_policy_base_model_id:{policyBaseIdentity.BaseModelId}",
            $"dpo_policy_base_revision:{policyBaseIdentity.BaseRevision}",
            $"dpo_policy_base_checkpoint_sha256:{policyBaseIdentity.BaseCheckpointSha256}",
            $"dpo_policy_config_sha256:{configSha256}",
            $"dpo_policy_initial_weight_sha256:{initialWeightSha256}",
            $"dpo_policy_bridge_source_record_count:{sourceRecordCount.ToString(CultureInfo.InvariantCulture)}",
            $"dpo_policy_bridge_preference_count:{bridgePreferenceCount.ToString(CultureInfo.InvariantCulture)}",
            $"dpo_policy_step_count:{dpoStepCount.ToString(CultureInfo.InvariantCulture)}"
        };
        refs.AddRange(
            DesignConversionDpoPolicyRandomContextContract.BuildRequiredEvidenceRefs(
                randomContext));

        if (loraContext is not null)
        {
            refs.Add(DesignConversionMachine.DpoPolicyLoraAppliedEvidence);
            refs.Add("10.LoRA:LoraApplicator");
            refs.Add($"dpo_policy_lora_rank:{loraContext.Rank.ToString(CultureInfo.InvariantCulture)}");
            refs.Add($"dpo_policy_lora_alpha:{loraContext.Alpha.ToString("0.######", CultureInfo.InvariantCulture)}");
            refs.Add($"dpo_policy_lora_dropout:{loraContext.Dropout.ToString("0.######", CultureInfo.InvariantCulture)}");
            refs.Add($"dpo_policy_lora_target_modules:{string.Join(",", loraContext.TargetModules)}");
            refs.Add($"dpo_policy_lora_adapter_count:{loraContext.AdapterCount.ToString(CultureInfo.InvariantCulture)}");
        }

        return refs.ToArray();
    }

    private static OperationResult<DesignConversionDpoPolicyCheckpointRef> BuildPolicyCheckpointRef(
        int sourceRecordCount,
        int bridgePreferenceCount,
        int dpoStepCount,
        string policyCheckpointFileName,
        string policyCheckpointSha256,
        int policyCheckpointByteLength,
        ModelConfig config,
        DesignConversionDpoPolicyBaseIdentity policyBaseIdentity,
        string configSha256,
        string initialWeightSha256,
        DesignConversionDpoPolicyCheckpointLoraContext? loraContext,
        DesignConversionDpoPolicyRandomContext randomContext,
        IReadOnlyList<string> bridgeEvidenceRefs,
        DesignConversionDpoLegalTeacherAdmissionCore legalTeacherAdmissionCore)
    {
        var finalEvidenceRefs = MergeEvidenceRefs(bridgeEvidenceRefs, [
            "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_TYPED",
            "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_NO_ABSOLUTE_PATH",
            "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_PAYLOAD_FILE_SIBLING",
            "DESIGN_CONVERSION_DPO_LEGAL_TEACHER_ADMISSION_COMMITMENT_TYPED",
            "DESIGN_CONVERSION_DPO_LEGAL_TEACHER_ADMISSION_COMMITMENT_NO_RAW_IDENTITY"
        ]);
        var commitment = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.FinalizeCommitment(
            legalTeacherAdmissionCore,
            finalEvidenceRefs);
        if (!commitment.IsSuccess || commitment.Value is null)
            return commitment.MapError<DesignConversionDpoPolicyCheckpointRef>();

        var checkpointRef = new DesignConversionDpoPolicyCheckpointRef(
            ArtifactKind: DesignConversionDpoTrainerRouteBGate.PolicyCheckpointArtifactKind,
            PolicyCheckpointFileName: policyCheckpointFileName,
            Sha256: policyCheckpointSha256,
            SourceSystem: DesignConversionDpoTrainerRouteBGate.TrainerSourceSystem,
            SourceMechanism: DesignConversionDpoTrainerRouteBGate.FreshLoadedSourceMechanism,
            RuntimeBridgeKind: DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind,
            FreshLoadVerified: true,
            ConvertedToRuntimeWeights: true,
            SourceRecordCount: sourceRecordCount,
            BridgePreferenceCount: bridgePreferenceCount,
            DpoStepCount: dpoStepCount,
            TrainingContext: (new DesignConversionDpoPolicyCheckpointTrainingContext(
                ConfigKind: policyBaseIdentity.ConfigurationKind,
                VocabSize: config.VocabSize,
                ContextLength: config.ContextLen,
                HiddenDim: config.HiddenDim,
                NumLayers: config.NumLayers,
                NumHeads: config.NumHeads,
                NumKvHeads: config.NumKvHeads,
                FfnHiddenDim: config.FfnHiddenDim,
                RawPayloadPersisted: true,
                AbsolutePathPersisted: false,
                SchemaVersion: 4,
                RopeTheta: config.RopeTheta,
                RmsNormEps: config.RmsNormEps,
                ProviderKind: policyBaseIdentity.ProviderKind,
                BaseModelId: policyBaseIdentity.BaseModelId,
                BaseRevision: policyBaseIdentity.BaseRevision,
                BaseCheckpointSha256: policyBaseIdentity.BaseCheckpointSha256,
                ConfigSha256: configSha256,
                InitialWeightSha256: initialWeightSha256,
                Lora: loraContext)) with
                {
                    LegalTeacherAdmissionCommitment = commitment.Value,
                    RandomContext = randomContext
                },
            EvidenceRefs: finalEvidenceRefs,
            PolicyCheckpointByteLength: policyCheckpointByteLength);
        return OperationResult<DesignConversionDpoPolicyCheckpointRef>.Ok(checkpointRef);
    }

    private static DesignConversionDpoPolicyCheckpointLoraContext BuildLoraContext(
        DesignConversionLoraCheckpointConfiguration loraConfiguration,
        int adapterCount)
        => new(
            loraConfiguration.Rank,
            loraConfiguration.Alpha,
            loraConfiguration.Dropout,
            loraConfiguration.TargetModules.ToArray(),
            adapterCount);

    private static int ParseDpoStepCountFromFileName(string policyCheckpointPath)
    {
        var fileName = Path.GetFileName(policyCheckpointPath);
        if (!fileName.StartsWith(DpoTrainer.PolicyCheckpointFilePrefix, StringComparison.Ordinal) ||
            !fileName.EndsWith(DpoTrainer.PolicyCheckpointFileExtension, StringComparison.Ordinal))
            return 1;

        var stepText = fileName[
            DpoTrainer.PolicyCheckpointFilePrefix.Length..
            ^DpoTrainer.PolicyCheckpointFileExtension.Length];
        return int.TryParse(stepText, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedStep) &&
               parsedStep > 0
            ? parsedStep
            : 1;
    }

    private static DesignConversionMachineCheckpoint AddDpoPolicyCheckpointEvidence(
        DesignConversionMachineCheckpoint checkpoint,
        IReadOnlyList<string> bridgeEvidenceRefs,
        DesignConversionDpoPolicyCheckpointRef policyCheckpointRef)
        => checkpoint with
        {
            CriticUnit = AddDpoPolicyCheckpointEvidence(checkpoint.CriticUnit, bridgeEvidenceRefs),
            ImproverUnit = AddDpoPolicyCheckpointEvidence(checkpoint.ImproverUnit, bridgeEvidenceRefs),
            EvidenceRefs = MergeEvidenceRefs(checkpoint.EvidenceRefs, bridgeEvidenceRefs),
            DpoPolicyCheckpointRef = policyCheckpointRef
        };

    private static DesignConversionRuntimeUnitCheckpoint AddDpoPolicyCheckpointEvidence(
        DesignConversionRuntimeUnitCheckpoint unit,
        IReadOnlyList<string> bridgeEvidenceRefs)
    {
        var weights = unit.LearnedWeights ?? [];
        if (weights.Count == 0)
            return unit;

        return unit with
        {
            ImplementationStatus = unit.ImplementationStatus.Contains("dpo_trainer_policy_checkpoint", StringComparison.Ordinal)
                ? unit.ImplementationStatus
                : $"{unit.ImplementationStatus}_with_dpo_trainer_policy_checkpoint_converted",
            EvidenceRefs = MergeEvidenceRefs(unit.EvidenceRefs, bridgeEvidenceRefs),
            LearnedWeights = weights
                .Select(weight => weight with
                {
                    Derivation = AppendDpoTrainerDerivation(weight.Derivation),
                    EvidenceRefs = MergeEvidenceRefs(weight.EvidenceRefs, bridgeEvidenceRefs)
                })
                .ToArray()
        };
    }

    private static string AppendDpoTrainerDerivation(string derivation)
    {
        return DesignConversionDpoTrainerRouteBGate.AppendFreshLoadedPolicyCheckpointDerivation(derivation);
    }

    private static string[] MergeEvidenceRefs(
        IEnumerable<string>? current,
        IEnumerable<string>? additions)
        => (current ?? [])
            .Concat(additions ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static async Task<OperationResult<DesignConversionMachineLoadResult>>
        PublishRuntimeTransactionAsync(
        string policyCheckpointPath,
        string policyCheckpointFileName,
        byte[] policyCheckpointPayload,
        string runtimeCheckpointPath,
        DesignConversionMachineCheckpoint bridgedCheckpoint,
        int dpoStepCount,
        IReadOnlyList<string> bridgeEvidenceRefs,
        CancellationToken cancellationToken)
        => await DesignConversionDpoRuntimeCheckpointPublisher.PublishAsync(
            policyCheckpointPath,
            policyCheckpointFileName,
            policyCheckpointPayload,
            runtimeCheckpointPath,
            bridgedCheckpoint,
            dpoStepCount,
            bridgeEvidenceRefs,
            RuntimeTransactionStageObserverForTesting,
            cancellationToken);

    private static string ComputeSha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static DesignConversionDpoTrainingHandoffAblationGate EvaluateLocalAblationQualityGate(
        IReadOnlyList<DesignConversionDpoTrainingRecord> rows)
    {
        if (rows.Count == 0)
        {
            return new DesignConversionDpoTrainingHandoffAblationGate(
                EvaluatedRecordCount: 0,
                PreferredScore: 0,
                BaselineScore: 0,
                ScoreDelta: 0,
                Threshold: LocalAblationScoreDeltaThreshold,
                PassesQualityGate: false,
                GateReason: "no_trainable_records_for_quality_gate",
                EvidenceRefs:
                [
                    "DESIGN_CONVERSION_DPO_HANDOFF_LOCAL_ABLATION_GATE_FAILED",
                    "DESIGN_CONVERSION_DPO_HANDOFF_NO_REAL_TRAINED_WEIGHTS_CLAIM"
                ]);
        }

        var preferredScore = Math.Round(rows.Average(row => row.Chosen.Verdict.Score), 6);
        var baselineScore = Math.Round(rows.Average(row => row.Rejected.Verdict.Score), 6);
        var scoreDelta = Math.Round(preferredScore - baselineScore, 6);
        var reasons = new List<string>();

        if (!IsFiniteGateScore(preferredScore) || !IsFiniteGateScore(baselineScore))
            reasons.Add("local_ablation_score_invalid");
        if (rows.Any(row => row.Chosen.Verdict.Score <= row.Rejected.Verdict.Score))
            reasons.Add("preferred_score_not_above_baseline");
        if (scoreDelta < LocalAblationScoreDeltaThreshold)
            reasons.Add($"score_delta_below_threshold:{Format(scoreDelta)}<{Format(LocalAblationScoreDeltaThreshold)}");
        if (rows.Any(row => !row.Chosen.Accepted || row.Rejected.Accepted))
            reasons.Add("preferred_rejected_acceptance_flags_not_separated");
        if (rows.Any(row => !row.Chosen.Verdict.Passed || row.Rejected.Verdict.Passed))
            reasons.Add("preferred_rejected_verdicts_not_separated");

        var passes = reasons.Count == 0;
        var evidenceRefs = new List<string>
        {
            "DESIGN_CONVERSION_DPO_HANDOFF_LOCAL_ABLATION_GATE",
            passes
                ? "DESIGN_CONVERSION_DPO_HANDOFF_LOCAL_ABLATION_GATE_PASSED"
                : "DESIGN_CONVERSION_DPO_HANDOFF_LOCAL_ABLATION_GATE_FAILED",
            "DESIGN_CONVERSION_DPO_HANDOFF_NO_REAL_TRAINED_WEIGHTS_CLAIM",
            "DESIGN_CONVERSION_DPO_HANDOFF_PRODUCT_READY_FALSE",
            $"baseline_score:{Format(baselineScore)}",
            $"preferred_score:{Format(preferredScore)}",
            $"score_delta:{Format(scoreDelta)}",
            $"threshold:{Format(LocalAblationScoreDeltaThreshold)}",
            $"passes_quality_gate:{passes.ToString().ToLowerInvariant()}"
        };

        return new DesignConversionDpoTrainingHandoffAblationGate(
            EvaluatedRecordCount: rows.Count,
            PreferredScore: preferredScore,
            BaselineScore: baselineScore,
            ScoreDelta: scoreDelta,
            Threshold: LocalAblationScoreDeltaThreshold,
            PassesQualityGate: passes,
            GateReason: passes
                ? "local_ablation_quality_gate_passed"
                : string.Join(";", reasons.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)),
            EvidenceRefs: evidenceRefs
                .Concat(reasons.Select(reason => $"quality_gate_reason:{reason}"))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }

    private static bool IsFiniteGateScore(double value)
        => !double.IsNaN(value) && !double.IsInfinity(value);

    private static LogicalDpoPreferenceExample BuildPreference(DesignConversionDpoTrainingRecord row)
    {
        var teachingVector = row.InternalVectorPayload.Count == 0
            ? null
            : row.InternalVectorPayload.Select(value => (float)value).ToArray();

        return new LogicalDpoPreferenceExample(
            DesignConversionDpoDatasetBuilder.BuildOptimizerPromptPayload(
                row,
                DesignConversionMachine.DomainTag),
            DesignConversionDpoDatasetBuilder.BuildOptimizerResponsePayload(row.Chosen),
            DesignConversionDpoDatasetBuilder.BuildOptimizerResponsePayload(row.Rejected),
            teachingVector)
        {
            // ModelServer handoff копирует только уже выбранные виды авторов:
            // reinference из deterministic provenance здесь был бы новым
            // неаудируемым маршрутом допуска.
            ChosenAuthorKind = row.ChosenAuthorKind,
            RejectedAuthorKind = row.RejectedAuthorKind
        };
    }

    private static string Format(double value)
        => value.ToString("0.######", CultureInfo.InvariantCulture);

    private static string HashRef(string? value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty))).ToLowerInvariant();
}
