// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using DPO;

namespace DesignConversionDomain;

/// <summary>Enforces the canonical 11.DPO DpoTrainer route-B publication boundary.</summary>
public static class DesignConversionDpoTrainerRouteBGate
{
    public const string GateId = "REAL_TRAINER_ROUTE_B_GATE";
    public const string FailureCode = "DESIGN_CONVERSION_DPO_TRAINER_ROUTE_B_GATE_FAILED";
    public const string FailureMessage =
        "DesignConversion DPO trainer route-B output is incomplete or inconsistent.";
    public const string TransformerModelIngredient = "03.TransformerModel:LlmModel";
    public const string LoraApplicatorIngredient = "10.LoRA:LoraApplicator";
    public const string TrainerSourceSystem = "11.DPO:DpoTrainer";
    public const string DerivationSourceSystem = "11.DPO/DpoTrainer";
    public const string StepVectorIngredient = "11.DPO:DpoTrainer.StepVector";
    public const string PolicyCheckpointArtifactKind = "11.DPO:DpoTrainer.policy_checkpoint";
    public const string FreshLoadedSourceMechanism = "11.DPO:DpoTrainer.StepVector:fresh_loaded";
    public const string FreshLoadedDerivationMarker =
        "source=11.DPO/DpoTrainer;policy_checkpoint=fresh_loaded_converted";

    /// <summary>Returns the canonical runtime ingredients required for route-B publication.</summary>
    public static string[] RuntimeIngredientIds()
        =>
        [
            TransformerModelIngredient,
            LoraApplicatorIngredient,
            TrainerSourceSystem,
            StepVectorIngredient
        ];

    /// <summary>Returns whether an ingredient identifies the canonical DpoTrainer vector step.</summary>
    public static bool IsStepVectorIngredient(string? value)
        => string.Equals(value, StepVectorIngredient, StringComparison.Ordinal);

    /// <summary>Returns whether a derivation carries the canonical DpoTrainer source marker.</summary>
    public static bool HasTrainerSourceDerivation(string? derivation)
        => !string.IsNullOrEmpty(derivation) &&
           (derivation.Contains($"source={DerivationSourceSystem}", StringComparison.Ordinal) ||
            derivation.Contains($"source={TrainerSourceSystem}", StringComparison.Ordinal));

    /// <summary>Adds the canonical fresh-loaded DpoTrainer derivation marker once.</summary>
    public static string AppendFreshLoadedPolicyCheckpointDerivation(string? derivation)
    {
        var current = derivation ?? string.Empty;
        return HasTrainerSourceDerivation(current)
            ? current
            : $"{current};{FreshLoadedDerivationMarker}";
    }

    /// <summary>Validates completed training output before a handoff result is published.</summary>
    public static OperationResult<bool> ValidateTrainingHandoff(
        int completedDpoStepCount,
        IReadOnlyList<string>? ingredientIds,
        IReadOnlyList<string>? policyCheckpointFilePaths)
    {
        if (completedDpoStepCount <= 0 ||
            !ContainsExact(ingredientIds, TrainerSourceSystem) ||
            !ContainsExact(ingredientIds, StepVectorIngredient))
        {
            return Failed();
        }

        var expectedFileName = DpoTrainer.BuildPolicyCheckpointFileName(completedDpoStepCount);
        if (policyCheckpointFilePaths is null ||
            !policyCheckpointFilePaths.Any(path =>
                !string.IsNullOrWhiteSpace(path) &&
                string.Equals(Path.GetFileName(path), expectedFileName, StringComparison.Ordinal)))
        {
            return Failed();
        }

        return OperationResult<bool>.Ok(true);
    }

    /// <summary>Validates the final loaded typed checkpoint before runtime bridge publication.</summary>
    public static OperationResult<bool> ValidateTypedRuntimeCheckpoint(
        DesignConversionDpoPolicyCheckpointRef? checkpointRef,
        int actualCompletedDpoStepCount,
        IReadOnlyList<string>? runtimeIngredientIds,
        bool callerFreshLoadVerified)
    {
        if (checkpointRef is null ||
            actualCompletedDpoStepCount <= 0 ||
            !callerFreshLoadVerified ||
            !checkpointRef.FreshLoadVerified ||
            !checkpointRef.ConvertedToRuntimeWeights ||
            checkpointRef.TrainingContext is null ||
            checkpointRef.TrainingContext.SchemaVersion != 4 ||
            !DesignConversionDpoPolicyRandomContextContract.IsValidCurrent(
                checkpointRef.TrainingContext.RandomContext) ||
            !DesignConversionDpoPolicyRandomContextContract.HasRequiredEvidenceRefs(
                checkpointRef.TrainingContext.RandomContext,
                checkpointRef.EvidenceRefs) ||
            !string.Equals(
                checkpointRef.RuntimeBridgeKind,
                DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind,
                StringComparison.Ordinal) ||
            !checkpointRef.TrainingContext.RawPayloadPersisted ||
            checkpointRef.TrainingContext.AbsolutePathPersisted ||
            checkpointRef.PolicyCheckpointByteLength <= 0 ||
            !DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Validate(
                checkpointRef.TrainingContext.LegalTeacherAdmissionCommitment) ||
            !DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.MatchesCheckpointEvidence(
                checkpointRef.TrainingContext.LegalTeacherAdmissionCommitment,
                checkpointRef.EvidenceRefs) ||
            checkpointRef.DpoStepCount != actualCompletedDpoStepCount ||
            !string.Equals(checkpointRef.ArtifactKind, PolicyCheckpointArtifactKind, StringComparison.Ordinal) ||
            !string.Equals(checkpointRef.SourceSystem, TrainerSourceSystem, StringComparison.Ordinal) ||
            !string.Equals(checkpointRef.SourceMechanism, FreshLoadedSourceMechanism, StringComparison.Ordinal) ||
            !string.Equals(
                checkpointRef.PolicyCheckpointFileName,
                DpoTrainer.BuildPolicyCheckpointFileName(actualCompletedDpoStepCount),
                StringComparison.Ordinal) ||
            !IsCanonicalSha256(checkpointRef.Sha256) ||
            !ContainsExact(runtimeIngredientIds, TrainerSourceSystem) ||
            !ContainsExact(runtimeIngredientIds, StepVectorIngredient))
        {
            return Failed();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool ContainsExact(IReadOnlyList<string>? values, string expected)
        => values is not null && values.Any(value => string.Equals(value, expected, StringComparison.Ordinal));

    private static bool IsCanonicalSha256(string? value)
        => value is not null &&
           value.Length == 64 &&
           value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static OperationResult<bool> Failed()
        => OperationResult<bool>.Fail(FailureMessage, FailureCode);
}
