// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

// This snapshot is the durable contract for cumulative teaching across rows and process restarts.
public sealed record TeachingRegistrySnapshot(
    string SnapshotId,
    string BundleId,
    int RowIndex,
    DateTimeOffset CreatedAtUtc,
    TeachingUnitRegistryEntry[] Units,
    TeachingFlowRegistryEntry[] Flows,
    GeneratedCapabilityCheckpointManifest[] GeneratedCapabilities,
    string[] CheckpointPaths);

// This entry captures the serializable metadata for one logical unit registration.
public sealed record TeachingUnitRegistryEntry(
    string UnitId,
    string UnitRole,
    string DomainTag,
    string Version,
    string TrustState,
    bool IsLearned,
    string InputTypeName,
    string OutputTypeName,
    long RegistrationOrder,
    string UnitKind);

// This entry captures the serializable metadata for one logical flow registration.
public sealed record TeachingFlowRegistryEntry(
    string FlowId,
    string ScenarioId,
    string Version,
    string InputTypeName,
    string PhaseId,
    string SubPhaseId,
    string TaskType,
    string[] DomainTags,
    long RegistrationOrder,
    string FlowKind);

// This manifest stores enough data to rebuild one generated learned capability unit.
public sealed record GeneratedCapabilityCheckpointManifest(
    string UnitId,
    string UnitRole,
    string DomainTag,
    string Version,
    string TrustState,
    string FlowId,
    string ScenarioId,
    string PhaseId,
    string SubPhaseId,
    string TaskType,
    string TrainingText,
    string OutputText,
    string ExampleId,
    string[] PredicateIds,
    string[] DomainTags,
    string PolicyCheckpointPath,
    string TokenizerPath,
    int VocabSize,
    int ContextLen,
    int HiddenDim,
    int NumLayers,
    int NumHeads,
    int NumKvHeads,
    int FfnHiddenDim,
    int LoraRank,
    float LoraAlpha,
    float LoraDropout,
    string[] LoraTargetModules,
    DateTimeOffset CreatedAtUtc,
    string UnitFamilyId = "",
    string? ParentUnitId = null,
    string? ParentPolicyCheckpointPath = null,
    string? ParentTokenizerPath = null,
    int TrainingStepCount = 1,
    string OriginalPrompt = "",
    string? ServerBaseUrl = null,
    string? ServerUnitEndpoint = null,
    string? ManifestSha256 = null,
    string? TrainingBranch = null,
    string? TrainingRunId = null,
    string? SourceDatasetSha256 = null,
    DateTimeOffset? UploadedAtUtc = null);

// These constants keep snapshot kind labels stable across the teaching runner and tests.
public static class TeachingRegistrySnapshotKinds
{
    public const string GeneratedCapabilityUnit = "generated_capability_transformer_knn";
    public const string DeterministicUnit = "deterministic";
    public const string GeneratedCapabilityFlow = "generated_capability";
    public const string UniversalNineNodeFlow = "universal_nine_node";
    public const string RegisteredFlow = "registered_flow";
}
