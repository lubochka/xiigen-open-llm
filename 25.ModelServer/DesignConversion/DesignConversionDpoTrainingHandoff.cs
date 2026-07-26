// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;
using DPO;
using LoRA;
using Tokenizer;
using TransformerModel;

public sealed record DesignConversionDpoTrainingHandoffResult(
    int RecordCount,
    int PreferenceCount,
    int DpoStepCount,
    string[] MetricFilePaths,
    string[] PolicyCheckpointFilePaths,
    string[] IngredientIds,
    DesignConversionDpoTrainingHandoffAblationGate QualityGate,
    string[] EvidenceRefs);

public sealed record DesignConversionDpoPolicyRuntimeCheckpointBridgeResult(
    string PolicyCheckpointPath,
    string PolicyCheckpointSha256,
    string RuntimeCheckpointPath,
    LearningMachineExport RuntimeExport,
    DesignConversionMachineCheckpoint RuntimeCheckpoint,
    bool PolicyCheckpointFreshLoadVerified,
    string[] EvidenceRefs);

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
    private const int MaxCompactLength = 640;
    private const string HandoffEvidence = "DesignConversion:DpoTrainingHandoff";
    private const double LocalAblationScoreDeltaThreshold = 0.10;

    public static async Task<OperationResult<DesignConversionDpoTrainingHandoffResult>> ExecuteAsync(
        IEnumerable<DesignConversionDpoTrainingRecord>? records,
        LlmModel policyModel,
        LlmModel referenceModel,
        BpeTokenizer tokenizer,
        string checkpointDirectory,
        LoraConfig? loraConfig = null,
        int maxStepCount = 1,
        CancellationToken cancellationToken = default)
    {
        if (policyModel is null || referenceModel is null)
            return OperationResult<DesignConversionDpoTrainingHandoffResult>.Fail("Policy and reference models are required.", "DESIGN_CONVERSION_DPO_MODELS_REQUIRED");
        if (tokenizer is null)
            return OperationResult<DesignConversionDpoTrainingHandoffResult>.Fail("Tokenizer is required.", "DESIGN_CONVERSION_DPO_TOKENIZER_REQUIRED");
        if (string.IsNullOrWhiteSpace(checkpointDirectory))
            return OperationResult<DesignConversionDpoTrainingHandoffResult>.Fail("Checkpoint directory is required.", "DESIGN_CONVERSION_DPO_CHECKPOINT_REQUIRED");

        var sourceRows = (records ?? Array.Empty<DesignConversionDpoTrainingRecord>())
            .Where(row => row is not null)
            .ToArray();
        if (sourceRows.Length == 0)
            return OperationResult<DesignConversionDpoTrainingHandoffResult>.Fail("At least one DesignConversion DPO training record is required.", "DESIGN_CONVERSION_DPO_RECORDS_REQUIRED");

        var trainableRows = sourceRows
            .Where(IsTrainableRow)
            .ToArray();
        if (trainableRows.Length == 0)
            return OperationResult<DesignConversionDpoTrainingHandoffResult>.Fail("No DesignConversion DPO training rows are allowed for local handoff.", "DESIGN_CONVERSION_DPO_NO_TRAINABLE_ROWS");

        var qualityGate = EvaluateLocalAblationQualityGate(trainableRows);
        if (!qualityGate.PassesQualityGate)
        {
            return OperationResult<DesignConversionDpoTrainingHandoffResult>.Ok(new DesignConversionDpoTrainingHandoffResult(
                sourceRows.Length,
                trainableRows.Length,
                DpoStepCount: 0,
                MetricFilePaths: [],
                PolicyCheckpointFilePaths: [],
                IngredientIds: [],
                QualityGate: qualityGate,
                EvidenceRefs: qualityGate.EvidenceRefs
                    .Append(HandoffEvidence)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()));
        }

        var preferences = trainableRows
            .Select(BuildPreference)
            .ToArray();

        var request = new LogicalDpoTrainingRequest(
            policyModel,
            referenceModel,
            tokenizer,
            checkpointDirectory,
            preferences,
            loraConfig,
            MaxStepCount: Math.Max(1, maxStepCount));

        var unitResult = await new LogicalDpoTrainingUnit().ExecuteAsync(new LogicalUnitRequest<LogicalDpoTrainingRequest>(
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
                ["boundary"] = HandoffEvidence
            }), cancellationToken);

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
        foreach (var stepVectorIngredient in ingredientIds.Where(id => string.Equals(id, "11.DPO:DpoTrainer.StepVector", StringComparison.Ordinal)))
            evidenceRefs.Add(stepVectorIngredient);
        foreach (var qualityEvidence in qualityGate.EvidenceRefs)
            evidenceRefs.Add(qualityEvidence);
        var policyCheckpointFilePaths = FindPolicyCheckpointFilePaths(checkpointDirectory);
        foreach (var policyCheckpointFilePath in policyCheckpointFilePaths)
            evidenceRefs.Add($"dpo_policy_checkpoint:{Path.GetFileName(policyCheckpointFilePath)}");

        return OperationResult<DesignConversionDpoTrainingHandoffResult>.Ok(new DesignConversionDpoTrainingHandoffResult(
            sourceRows.Length,
            output.PreferenceCount,
            output.DpoStepCount,
            output.MetricFilePaths,
            policyCheckpointFilePaths,
            ingredientIds,
            qualityGate,
            evidenceRefs.ToArray()));
    }

    public static async Task<OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>> ConvertPolicyCheckpointToRuntimeCheckpointAsync(
        DesignConversionDpoTrainingRecord record,
        string policyCheckpointPath,
        LlmModel freshPolicyModel,
        string runtimeCheckpointPath,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail("Bridge cancelled.", "CANCELLED");
        if (record is null)
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail("DPO training record is required.", "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_RECORD_REQUIRED");
        if (!IsTrainableRow(record))
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail("DPO training record is not trainable for policy-runtime bridge.", "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_RECORD_NOT_TRAINABLE");
        if (string.IsNullOrWhiteSpace(policyCheckpointPath))
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail("Policy checkpoint path is required.", "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REQUIRED");
        if (!File.Exists(policyCheckpointPath))
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail($"Policy checkpoint not found: {policyCheckpointPath}", "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_NOT_FOUND");
        if (freshPolicyModel is null)
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail("Fresh policy model is required.", "DESIGN_CONVERSION_DPO_POLICY_FRESH_MODEL_REQUIRED");
        if (string.IsNullOrWhiteSpace(runtimeCheckpointPath))
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail("Runtime checkpoint path is required.", "DESIGN_CONVERSION_RUNTIME_CHECKPOINT_REQUIRED");

        try
        {
            freshPolicyModel.LoadCheckpoint(policyCheckpointPath);
        }
        catch (Exception ex)
        {
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(ex.Message, "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_FRESH_LOAD_FAILED");
        }

        var policyCheckpointSha256 = await ComputeFileSha256Async(policyCheckpointPath, cancellationToken);
        var machine = new DesignConversionMachine();
        var train = await machine.TrainBatchAsync(
            BuildPolicyRuntimeBridgeBatch(record, policyCheckpointPath, policyCheckpointSha256),
            cancellationToken);
        if (!train.IsSuccess)
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(train.ErrorMessage, train.ErrorCode);

        var checkpoint = machine.ExportCheckpoint();
        if (!checkpoint.IsSuccess)
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(checkpoint.ErrorMessage, checkpoint.ErrorCode);

        var bridgeEvidenceRefs = new[]
        {
            DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence,
            DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence,
            "DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_SOURCE_11_DPO",
            $"dpo_policy_checkpoint_sha256:{policyCheckpointSha256}",
            $"dpo_policy_checkpoint_file:{Path.GetFileName(policyCheckpointPath)}",
            "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PRODUCT_READY_FALSE"
        };
        var bridgedCheckpoint = AddDpoPolicyCheckpointEvidence(checkpoint.Value!, bridgeEvidenceRefs);

        try
        {
            var dir = Path.GetDirectoryName(runtimeCheckpointPath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(bridgedCheckpoint, DesignConversionCheckpointJsonOptions.Standard);
            var tmpPath = runtimeCheckpointPath + ".tmp";
            await File.WriteAllTextAsync(tmpPath, json, Encoding.UTF8, cancellationToken);
            File.Move(tmpPath, runtimeCheckpointPath, overwrite: true);
        }
        catch (Exception ex)
        {
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(ex.Message, "DESIGN_CONVERSION_RUNTIME_CHECKPOINT_WRITE_FAILED");
        }

        var loaded = await DesignConversionMachine.LoadCheckpointAsync(runtimeCheckpointPath, cancellationToken);
        if (!loaded.IsSuccess)
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(loaded.ErrorMessage, loaded.ErrorCode);

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
                PolicyCheckpointPath: Path.GetFullPath(policyCheckpointPath),
                PolicyCheckpointSha256: policyCheckpointSha256,
                RuntimeCheckpointPath: Path.GetFullPath(runtimeCheckpointPath),
                RuntimeExport: runtimeExport,
                RuntimeCheckpoint: loaded.Value.Checkpoint,
                PolicyCheckpointFreshLoadVerified: true,
                EvidenceRefs: bridgeEvidenceRefs
                    .Concat(loaded.Value.Checkpoint.EvidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray()));
    }

    private static bool IsTrainableRow(DesignConversionDpoTrainingRecord row)
        => row.License.AllowsTraining
           && !row.License.ContainsUserPrivateData
           && !row.Quarantine.IsQuarantined;

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
        var prompt = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["unit_id"] = unitId,
            ["source_kind"] = record.Provenance.SourceKind,
            ["source_manifest_hash"] = HashRef($"{record.Provenance.SourceManifestRef}|{policyCheckpointSha256}"),
            ["source_identity_hash"] = policyCheckpointSha256,
            ["source_structure_hash"] = HashRef(record.SourceStructurePayload),
            ["internal_vector"] = record.InternalVectorPayload.ToArray(),
            ["pair_family"] = "dpo_trainer_policy_checkpoint",
            ["dpo_policy_checkpoint_sha256"] = policyCheckpointSha256,
            ["dpo_policy_checkpoint_file"] = Path.GetFileName(policyCheckpointPath)
        });
        var chosen = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["visual_score"] = record.Chosen.Verdict.Score,
            ["accepted"] = record.Chosen.Accepted,
            ["verdict_passed"] = record.Chosen.Verdict.Passed,
            ["evidence_refs"] = record.Chosen.EvidenceRefs.ToArray()
        });
        var rejected = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["visual_score"] = record.Rejected.Verdict.Score,
            ["accepted"] = record.Rejected.Accepted,
            ["verdict_passed"] = record.Rejected.Verdict.Passed,
            ["evidence_refs"] = record.Rejected.EvidenceRefs.ToArray()
        });

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
                "11.DPO/DpoTrainer",
                $"dpo_policy_sha256:{policyCheckpointSha256}",
                $"dpo_policy_file:{Path.GetFileName(policyCheckpointPath)}",
                $"design_conversion_dpo_record:{record.RecordId}"
            ]);
    }

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

    private static async Task<string> ComputeFileSha256Async(string path, CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

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
            BuildPrompt(row),
            CompactOutput(row.Chosen),
            CompactOutput(row.Rejected),
            teachingVector);
    }

    private static string BuildPrompt(DesignConversionDpoTrainingRecord row)
    {
        var fields = new[]
        {
            $"unit={Compact(row.UnitId)}",
            $"record={Compact(row.RecordId)}",
            $"source_len={TextLength(row.SourceStructurePayload).ToString(CultureInfo.InvariantCulture)}",
            $"source_sha256={HashRef(row.SourceStructurePayload)}",
            $"state_len={TextLength(row.InternalUnitStatePayload).ToString(CultureInfo.InvariantCulture)}",
            $"state_sha256={HashRef(row.InternalUnitStatePayload)}",
            $"vector_count={row.InternalVectorPayload.Count.ToString(CultureInfo.InvariantCulture)}",
            $"usefulness={row.UsefulnessScore.ToString("0.###", CultureInfo.InvariantCulture)}",
            $"negative_taxonomy={Compact(row.NegativeReasonTaxonomy)}",
            $"source_manifest={Compact(row.Provenance.SourceManifestRef)}",
            $"reviewer={Compact(row.Provenance.ReviewerRef)}"
        };

        return string.Join(" | ", fields);
    }

    private static string CompactOutput(DesignConversionImprovementOutput output)
        => Compact(JsonSerializer.Serialize(new
        {
            output_id = output.OutputId,
            candidate_id = output.CandidateId,
            accepted = output.Accepted,
            verdict = new
            {
                output.Verdict.VerdictId,
                output.Verdict.Score,
                output.Verdict.Passed,
                defect_categories = output.Verdict.Defects.Select(defect => defect.Category).ToArray()
            },
            html_len = TextLength(output.Html),
            html_sha256 = HashRef(output.Html),
            css_len = TextLength(output.Css),
            css_sha256 = HashRef(output.Css),
            qa_result_ids = output.QaResults.Select(qa => qa.ResultId).ToArray(),
            evidence_refs = output.EvidenceRefs.ToArray()
        }));

    private static string Compact(string value)
    {
        var compact = string.Join(" ", (value ?? string.Empty)
            .Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return compact.Length <= MaxCompactLength ? compact : compact[..MaxCompactLength];
    }

    private static string Format(double value)
        => value.ToString("0.######", CultureInfo.InvariantCulture);

    private static int TextLength(string? value)
        => value?.Length ?? 0;

    private static string HashRef(string? value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty))).ToLowerInvariant();
}
