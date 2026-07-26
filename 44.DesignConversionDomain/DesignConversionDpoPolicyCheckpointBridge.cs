// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO;
using DPO.Models;
using TensorCore.Core;
using TransformerModel;
using TransformerModel.Config;

public sealed record DesignConversionDpoPolicyRuntimeCheckpointBridgeResult(
    int SourceRecordCount,
    int BridgePreferenceCount,
    int DpoStepCount,
    string PolicyCheckpointPath,
    string PolicyCheckpointSha256,
    string RuntimeCheckpointPath,
    LearningMachineExport RuntimeExport,
    DesignConversionMachine RuntimeMachine,
    DesignConversionMachineCheckpoint RuntimeCheckpoint,
    bool PolicyCheckpointFreshLoadVerified,
    IReadOnlyList<string> IngredientIds,
    IReadOnlyList<string> EvidenceRefs);

public static class DesignConversionDpoPolicyCheckpointBridge
{
    private const int MaxBridgePreferenceCount = 8;
    private const int PolicyVocabSize = 512;
    private const int PolicyContextLength = 64;

    public static async Task<OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>> ExecuteAndConvertAsync(
        IReadOnlyList<DesignConversionDpoTrainingRecord>? records,
        string outputDirectory,
        string runId,
        string runtimeCheckpointPath,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                "DesignConversion DpoTrainer policy bridge cancelled.",
                "CANCELLED");

        if (records is null)
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                "DesignConversion DPO training records are required for the policy bridge.",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_RECORDS_REQUIRED");

        if (string.IsNullOrWhiteSpace(outputDirectory))
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                "DesignConversion DpoTrainer policy bridge requires an output directory.",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_OUTPUT_DIRECTORY_REQUIRED");

        if (string.IsNullOrWhiteSpace(runtimeCheckpointPath))
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                "DesignConversion DpoTrainer policy bridge requires a runtime checkpoint path.",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_RUNTIME_CHECKPOINT_REQUIRED");

        var trainableRows = records
            .Where(IsTrainableRow)
            .OrderByDescending(row => row.UsefulnessScore)
            .ThenBy(row => row.RecordId, StringComparer.Ordinal)
            .Take(MaxBridgePreferenceCount)
            .ToArray();
        if (trainableRows.Length == 0)
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                "No DesignConversion DPO training rows are trainable for the policy bridge.",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_NO_TRAINABLE_ROWS");

        try
        {
            var policyCheckpointDirectory = Path.Combine(
                Path.GetFullPath(outputDirectory),
                $"{ToSafeFileStem(runId)}-dpo-policy-checkpoints");
            Directory.CreateDirectory(policyCheckpointDirectory);

            var config = CreateTinyPolicyConfig();
            var policy = CreateLlmModel(config);
            var reference = CreateLlmModel(config);
            var trainer = CreateDpoTrainer(policy, reference, policyCheckpointDirectory);
            var batches = trainableRows
                .Select(BuildVectorBatch)
                .ToArray();
            trainer.StepVector(batches, 1);

            var policyCheckpointPath = Path.Combine(
                policyCheckpointDirectory,
                DpoTrainer.BuildPolicyCheckpointFileName(1));
            if (!File.Exists(policyCheckpointPath))
                return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                    "DpoTrainer policy checkpoint was not written.",
                    "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_NOT_WRITTEN");

            var freshPolicy = CreateLlmModel(config);
            freshPolicy.LoadCheckpoint(policyCheckpointPath);
            var policyCheckpointSha256 = await ComputeFileSha256Async(policyCheckpointPath, ct);
            var runtimeMachine = new DesignConversionMachine();
            var train = await runtimeMachine.TrainBatchAsync(
                BuildPolicyRuntimeBridgeBatch(trainableRows, policyCheckpointPath, policyCheckpointSha256),
                ct);
            if (!train.IsSuccess)
                return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                    train.ErrorMessage,
                    train.ErrorCode);

            var checkpoint = runtimeMachine.ExportCheckpoint();
            if (!checkpoint.IsSuccess)
                return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                    checkpoint.ErrorMessage,
                    checkpoint.ErrorCode);

            var bridgeEvidenceRefs = BuildBridgeEvidenceRefs(
                trainableRows.Length,
                policyCheckpointPath,
                policyCheckpointSha256);
            var bridgedCheckpoint = AddDpoPolicyCheckpointEvidence(checkpoint.Value!, bridgeEvidenceRefs);
            await WriteRuntimeCheckpointAsync(bridgedCheckpoint, runtimeCheckpointPath, ct);

            var loaded = await DesignConversionMachine.LoadCheckpointAsync(runtimeCheckpointPath, ct);
            if (!loaded.IsSuccess)
                return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                    loaded.ErrorMessage,
                    loaded.ErrorCode);

            var runtimeExport = new LearningMachineExport(
                MachineId: DesignConversionMachine.MachineId,
                DomainTag: DesignConversionMachine.DomainTag,
                Version: loaded.Value!.Checkpoint.Version,
                CheckpointRef: Path.GetFullPath(runtimeCheckpointPath),
                ManifestRef: $"{DesignConversionMachine.MachineId}/{loaded.Value.Checkpoint.Version}/manifest.json",
                AccuracyAtExport: loaded.Value.Checkpoint.AccuracyAfterLastBatch,
                ExportedAtUtc: loaded.Value.Checkpoint.ExportedAtUtc);

            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Ok(
                new DesignConversionDpoPolicyRuntimeCheckpointBridgeResult(
                    SourceRecordCount: records.Count,
                    BridgePreferenceCount: trainableRows.Length,
                    DpoStepCount: 1,
                    PolicyCheckpointPath: Path.GetFullPath(policyCheckpointPath),
                    PolicyCheckpointSha256: policyCheckpointSha256,
                    RuntimeCheckpointPath: Path.GetFullPath(runtimeCheckpointPath),
                    RuntimeExport: runtimeExport,
                    RuntimeMachine: loaded.Value.Machine,
                    RuntimeCheckpoint: loaded.Value.Checkpoint,
                    PolicyCheckpointFreshLoadVerified: true,
                    IngredientIds:
                    [
                        "03.TransformerModel:LlmModel",
                        "11.DPO:DpoTrainer",
                        "11.DPO:DpoTrainer.StepVector"
                    ],
                    EvidenceRefs: bridgeEvidenceRefs
                        .Concat(loaded.Value.Checkpoint.EvidenceRefs)
                        .Distinct(StringComparer.Ordinal)
                        .Order(StringComparer.Ordinal)
                        .ToArray()));
        }
        catch (Exception ex) when (ex is IOException
                                   or UnauthorizedAccessException
                                   or InvalidOperationException
                                   or ArgumentException)
        {
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                ex.Message,
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_FAILED");
        }
    }

    private static bool IsTrainableRow(DesignConversionDpoTrainingRecord row)
        => row is not null &&
           row.License.AllowsTraining &&
           !row.License.ContainsUserPrivateData &&
           !row.Quarantine.IsQuarantined;

    private static ModelConfig CreateTinyPolicyConfig()
        => new()
        {
            VocabSize = PolicyVocabSize,
            ContextLen = PolicyContextLength,
            HiddenDim = 16,
            NumLayers = 1,
            NumHeads = 4,
            NumKvHeads = 2,
            FfnHiddenDim = 32
        };

    private static LlmModel CreateLlmModel(ModelConfig config)
        => new(config);

    private static DpoTrainer CreateDpoTrainer(
        LlmModel policy,
        LlmModel reference,
        string policyCheckpointDirectory)
        => new(policy, reference, policyCheckpointDirectory, 1e-5f);

    private static DpoVectorBatch BuildVectorBatch(DesignConversionDpoTrainingRecord row)
        => new(
            BuildBatch(row),
            row.InternalVectorPayload.Count == 0
                ? [1f]
                : row.InternalVectorPayload
                    .Take(PolicyContextLength)
                    .Select(value => (float)Math.Clamp(value, -2.0, 2.0))
                    .ToArray(),
            Weight: Math.Max(0.01f, (float)Math.Clamp(row.UsefulnessScore, 0.01, 1.0)));

    private static DpoBatch BuildBatch(DesignConversionDpoTrainingRecord row)
    {
        var prompt = string.Join(
            " ",
            "design-conversion",
            NormalizeUnitId(row.UnitId),
            HashText(row.Provenance.SourceManifestRef),
            HashText(row.SourceStructurePayload),
            row.UsefulnessScore.ToString("0.###", CultureInfo.InvariantCulture),
            row.NegativeReasonTaxonomy);
        var chosen = $"chosen {row.Chosen.Verdict.Score:0.###} accepted {row.Chosen.Accepted}";
        var rejected = $"rejected {row.Rejected.Verdict.Score:0.###} accepted {row.Rejected.Accepted}";
        var chosenIds = EncodeIds($"{prompt} {chosen}");
        var rejectedIds = EncodeIds($"{prompt} {rejected}");
        var length = Math.Max(chosenIds.Count, rejectedIds.Count);

        return new DpoBatch
        {
            ChosenIds = ToTensor(chosenIds, length),
            ChosenMask = ToMask(chosenIds.Count, length),
            RejectedIds = ToTensor(rejectedIds, length),
            RejectedMask = ToMask(rejectedIds.Count, length)
        };
    }

    private static IReadOnlyList<int> EncodeIds(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        var maxLength = Math.Max(2, PolicyContextLength);
        var ids = bytes
            .Take(maxLength)
            .Select(value => Math.Clamp((int)value, 0, PolicyVocabSize - 1))
            .ToList();
        while (ids.Count < 2)
            ids.Add(0);
        return ids;
    }

    private static Tensor ToTensor(IReadOnlyList<int> ids, int length)
    {
        var data = new float[length];
        for (var index = 0; index < length; index++)
            data[index] = index < ids.Count ? ids[index] : 0;

        return new Tensor(data, new TensorShape(1, length));
    }

    private static Tensor ToMask(int sourceLength, int length)
    {
        var data = new float[length];
        for (var index = 0; index < length; index++)
            data[index] = index < sourceLength ? 1f : 0f;

        return new Tensor(data, new TensorShape(1, length));
    }

    private static LearningBatch<DesignConversionDomain> BuildPolicyRuntimeBridgeBatch(
        IReadOnlyList<DesignConversionDpoTrainingRecord> rows,
        string policyCheckpointPath,
        string policyCheckpointSha256)
        => new(
            BatchId: $"dpo-policy-runtime-bridge:{policyCheckpointSha256[..12]}",
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems: rows
                .Select(row => BuildPolicyRuntimeBridgeItem(row, policyCheckpointPath, policyCheckpointSha256))
                .ToArray(),
            SourceIds: rows
                .Select(row => $"design_conversion_dpo_record:{HashText(row.RecordId)}")
                .Concat(
                [
                    "11.DPO/DpoTrainer",
                    $"dpo_policy_sha256:{policyCheckpointSha256}",
                    $"dpo_policy_file:{Path.GetFileName(policyCheckpointPath)}"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray());

    private static LearningTrainingItem BuildPolicyRuntimeBridgeItem(
        DesignConversionDpoTrainingRecord row,
        string policyCheckpointPath,
        string policyCheckpointSha256)
    {
        var unitId = NormalizeUnitId(row.UnitId);
        var prompt = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["unit_id"] = unitId,
            ["source_kind"] = row.Provenance.SourceKind,
            ["source_manifest_hash"] = HashText($"{row.Provenance.SourceManifestRef}|{policyCheckpointSha256}"),
            ["source_identity_hash"] = policyCheckpointSha256,
            ["source_structure_hash"] = HashText(row.SourceStructurePayload),
            ["internal_vector"] = row.InternalVectorPayload.Take(PolicyContextLength).ToArray(),
            ["pair_family"] = "dpo_trainer_policy_checkpoint",
            ["dpo_policy_checkpoint_sha256"] = policyCheckpointSha256,
            ["dpo_policy_checkpoint_file"] = Path.GetFileName(policyCheckpointPath)
        }, DesignConversionCheckpointJsonOptions.Standard);
        var chosen = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["visual_score"] = row.Chosen.Verdict.Score,
            ["accepted"] = row.Chosen.Accepted,
            ["verdict_passed"] = row.Chosen.Verdict.Passed,
            ["defect_categories"] = row.Chosen.Verdict.Defects.Select(defect => defect.Category).ToArray()
        }, DesignConversionCheckpointJsonOptions.Standard);
        var rejected = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["visual_score"] = row.Rejected.Verdict.Score,
            ["accepted"] = row.Rejected.Accepted,
            ["verdict_passed"] = row.Rejected.Verdict.Passed,
            ["defect_categories"] = row.Rejected.Verdict.Defects.Select(defect => defect.Category).ToArray()
        }, DesignConversionCheckpointJsonOptions.Standard);

        return new LearningTrainingItem(
            ItemId: $"{HashText(row.RecordId)}:dpo-policy-runtime-bridge",
            Prompt: prompt,
            ChosenResponse: chosen,
            RejectedResponse: rejected);
    }

    private static IReadOnlyList<string> BuildBridgeEvidenceRefs(
        int bridgePreferenceCount,
        string policyCheckpointPath,
        string policyCheckpointSha256)
        =>
        [
            DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence,
            DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence,
            "DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_SOURCE_11_DPO",
            "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PRODUCT_READY_FALSE",
            "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_NO_RAW_PAYLOAD",
            "11.DPO:DpoTrainer",
            "11.DPO:DpoTrainer.StepVector",
            $"dpo_policy_checkpoint_sha256:{policyCheckpointSha256}",
            $"dpo_policy_checkpoint_file:{Path.GetFileName(policyCheckpointPath)}",
            $"dpo_policy_bridge_preference_count:{bridgePreferenceCount.ToString(CultureInfo.InvariantCulture)}"
        ];

    private static DesignConversionMachineCheckpoint AddDpoPolicyCheckpointEvidence(
        DesignConversionMachineCheckpoint checkpoint,
        IReadOnlyList<string> bridgeEvidenceRefs)
        => checkpoint with
        {
            CriticUnit = AddDpoPolicyCheckpointEvidence(checkpoint.CriticUnit, bridgeEvidenceRefs),
            ImproverUnit = AddDpoPolicyCheckpointEvidence(checkpoint.ImproverUnit, bridgeEvidenceRefs),
            EvidenceRefs = MergeEvidenceRefs(checkpoint.EvidenceRefs, bridgeEvidenceRefs)
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
        var current = derivation ?? string.Empty;
        return current.Contains("source=11.DPO/DpoTrainer", StringComparison.Ordinal)
            ? current
            : $"{current};source=11.DPO/DpoTrainer;policy_checkpoint=fresh_loaded_converted";
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

    private static async Task WriteRuntimeCheckpointAsync(
        DesignConversionMachineCheckpoint checkpoint,
        string runtimeCheckpointPath,
        CancellationToken ct)
    {
        var dir = Path.GetDirectoryName(runtimeCheckpointPath);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(checkpoint, DesignConversionCheckpointJsonOptions.Standard);
        var tmpPath = runtimeCheckpointPath + ".tmp";
        await File.WriteAllTextAsync(tmpPath, json, Encoding.UTF8, ct);
        File.Move(tmpPath, runtimeCheckpointPath, overwrite: true);
    }

    private static async Task<string> ComputeFileSha256Async(
        string path,
        CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static string NormalizeUnitId(string unitId)
        => DesignConversionDpoUnits.TryNormalize(unitId, out var normalized)
            ? normalized
            : unitId;

    private static string HashText(string? value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty))).ToLowerInvariant();

    private static string ToSafeFileStem(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string((string.IsNullOrWhiteSpace(value) ? "design-conversion-dpo-policy-bridge" : value)
            .Select(ch => invalid.Contains(ch) || char.IsWhiteSpace(ch) ? '-' : ch)
            .ToArray()).Trim('-');
        return string.IsNullOrWhiteSpace(cleaned)
            ? "design-conversion-dpo-policy-bridge"
            : cleaned;
    }
}
