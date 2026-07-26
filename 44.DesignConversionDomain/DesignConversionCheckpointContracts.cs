// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Text.Encodings.Web;
using System.Text.Json;

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
    bool DeterministicLogicClaimedAsLearning);

public sealed record DesignConversionRuntimeUnitCheckpoint(
    string UnitId,
    string UnitRole,
    string DomainTag,
    string ImplementationStatus,
    IReadOnlyList<string> EvidenceRefs,
    bool LearnedWeightsPresent,
    IReadOnlyList<DesignConversionLearnedWeightCheckpoint>? LearnedWeights = null);

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

public sealed record DesignConversionMachineLoadResult(
    DesignConversionMachine Machine,
    DesignConversionMachineCheckpoint Checkpoint,
    string CheckpointSha256);

public static class DesignConversionCheckpointJsonOptions
{
    public static JsonSerializerOptions Standard { get; } = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };
}
