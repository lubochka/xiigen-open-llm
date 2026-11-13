// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class DesignConversionDomain { }

public sealed record DesignConversionMachineCheckpoint(
    int SchemaVersion,
    string MachineId,
    string DomainTag,
    string Version,
    double AccuracyAfterLastBatch,
    bool HasTrainingScaffoldHistory,
    DateTimeOffset ExportedAtUtc,
    DesignConversionRuntimeUnitCheckpoint CriticUnit,
    DesignConversionRuntimeUnitCheckpoint ImproverUnit,
    IReadOnlyList<DesignConversionTrainingHistoryEntryCheckpoint> TrainingHistory,
    IReadOnlyList<string> EvidenceRefs,
    bool DeterministicLogicClaimedAsLearning,
    DesignConversionDpoPolicyCheckpointRef? DpoPolicyCheckpointRef = null);

public sealed record DesignConversionRuntimeUnitCheckpoint(
    string UnitId,
    string UnitRole,
    string DomainTag,
    string ImplementationStatus,
    IReadOnlyList<string> EvidenceRefs,
    bool LearnedWeightsPresent,
    IReadOnlyList<DesignConversionLearnedWeightCheckpoint>? LearnedWeights = null);

public sealed record DesignConversionDpoPolicyCheckpointRef(
    string ArtifactKind,
    string PolicyCheckpointFileName,
    string Sha256,
    string SourceSystem,
    string SourceMechanism,
    string RuntimeBridgeKind,
    bool FreshLoadVerified,
    bool ConvertedToRuntimeWeights,
    int SourceRecordCount,
    int BridgePreferenceCount,
    int DpoStepCount,
    DesignConversionDpoPolicyCheckpointTrainingContext TrainingContext,
    IReadOnlyList<string> EvidenceRefs,
    int PolicyCheckpointByteLength = 0)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DesignConversionDpoHeldOutEvaluation? EvaluatedHeldOutAcceptanceAuthority { get; init; }
}

public sealed record DesignConversionDpoPolicyRandomContext(
    int SchemaVersion,
    ulong RootInitializationSeed,
    string TrainingConfigurationFingerprint,
    string DerivationScheme,
    string InitializationDomain,
    string DropoutDomain,
    string PolicyDropoutStreamDomain,
    string ReferenceDropoutStreamDomain,
    string FreshLoadDropoutStreamDomain,
    string ReproducibilityScope);

public sealed record DesignConversionDpoPolicyCheckpointTrainingContext(
    string ConfigKind,
    int VocabSize,
    int ContextLength,
    int HiddenDim,
    int NumLayers,
    int NumHeads,
    int NumKvHeads,
    int FfnHiddenDim,
    bool RawPayloadPersisted,
    bool AbsolutePathPersisted,
    int SchemaVersion = 1,
    float RopeTheta = 0,
    float RmsNormEps = 0,
    string? ProviderKind = null,
    string? BaseModelId = null,
    string? BaseRevision = null,
    string? BaseCheckpointSha256 = null,
    string? ConfigSha256 = null,
    string? InitialWeightSha256 = null,
    DesignConversionDpoPolicyCheckpointLoraContext? Lora = null)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DesignConversionDpoLegalTeacherAdmissionCommitment?
        LegalTeacherAdmissionCommitment { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DesignConversionDpoPolicyRandomContext? RandomContext { get; init; }
}

public sealed record DesignConversionDpoPolicyCheckpointLoraContext(
    int Rank,
    float Alpha,
    float Dropout,
    IReadOnlyList<string> TargetModules,
    int AdapterCount);

public sealed record DesignConversionLearnedWeightCheckpoint(
    string FeatureKey,
    double Weight,
    int ObservationCount,
    string LastBatchId,
    string Derivation,
    IReadOnlyList<string> SourceIdentityHashes,
    IReadOnlyList<string> EvidenceRefs);

public sealed record DesignConversionTrainingHistoryEntryCheckpoint(
    string BatchId,
    int RowsTrained,
    double AccuracyBefore,
    double AccuracyAfter,
    double RewardMargin,
    long CpuMs,
    long PeakRamMb,
    long ElapsedMs,
    IReadOnlyList<string> PromotedUnitIds);

/// <summary>
/// Loader-produced evidence that the runtime checkpoint's sibling DPO policy
/// payload was validated and installed as the machine's direct scorer. The
/// internal constructor prevents metadata-only callers from manufacturing the
/// proof without passing through <see cref="DesignConversionMachine.LoadCheckpointAsync(string, CancellationToken)"/>.
/// </summary>
public sealed class DesignConversionVerifiedDpoPolicyRuntimeScorerLoaded
{
    internal DesignConversionVerifiedDpoPolicyRuntimeScorerLoaded(
        string runtimeCheckpointPath,
        string runtimeCheckpointSha256,
        int runtimeCheckpointByteLength,
        string policyCheckpointPath,
        string policyCheckpointSha256,
        int policyCheckpointByteLength,
        DesignConversionDpoPolicyCheckpointRef policyCheckpointRef)
    {
        RuntimeCheckpointPath = runtimeCheckpointPath;
        RuntimeCheckpointSha256 = runtimeCheckpointSha256;
        RuntimeCheckpointByteLength = runtimeCheckpointByteLength;
        PolicyCheckpointPath = policyCheckpointPath;
        PolicyCheckpointSha256 = policyCheckpointSha256;
        PolicyCheckpointByteLength = policyCheckpointByteLength;
        PolicyCheckpointRef = policyCheckpointRef;
    }

    public string RuntimeCheckpointPath { get; }
    public string RuntimeCheckpointSha256 { get; }
    public int RuntimeCheckpointByteLength { get; }
    public string PolicyCheckpointPath { get; }
    public string PolicyCheckpointSha256 { get; }
    public int PolicyCheckpointByteLength { get; }
    public DesignConversionDpoPolicyCheckpointRef PolicyCheckpointRef { get; }
}

public sealed record DesignConversionMachineLoadResult(
    DesignConversionMachine Machine,
    DesignConversionMachineCheckpoint Checkpoint,
    string CheckpointSha256)
{
    public DesignConversionVerifiedDpoPolicyRuntimeScorerLoaded?
        VerifiedDpoPolicyRuntimeScorerLoaded { get; init; }
}

public static class DesignConversionCheckpointJsonOptions
{
    public static JsonSerializerOptions Standard { get; } = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };
}
