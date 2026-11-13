// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;

public sealed record OrchestrationReasoningTrainerReport(
    string TrainerRunId,
    int RowCount,
    IReadOnlyList<string> AcceptedPairIds,
    string PayloadSha256,
    string SourcePairSha256,
    int LearnedPatternCount,
    bool TrainingStarted,
    bool ModelAssetCreated,
    bool CheckpointCreated,
    bool SemanticQualityClaimed,
    bool ExcludedTechnicalRefsTrainable,
    OrchestrationReasoningLearnedGuideStateDescriptor LearnedState,
    OrchestrationReasoningAdmittedLearnedState AdmittedState);

public sealed class OrchestrationReasoningTrainer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public async Task<OperationResult<OrchestrationReasoningTrainerReport>> MaterializeAsync(
        string payloadPath,
        string manifestPath,
        string? sourceAdmissionLedgerPath = null,
        string? sourceAdmissionEvidencePath = null,
        string? learnedStateManifestRef = null,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<OrchestrationReasoningTrainerReport>.Fail("Trainer materialization cancelled.", "CANCELLED");
        if (string.IsNullOrWhiteSpace(payloadPath))
            return OperationResult<OrchestrationReasoningTrainerReport>.Fail("Payload path is required.", "ORCHESTRATION_REASONING_TRAINER_PAYLOAD_PATH_REQUIRED");
        if (string.IsNullOrWhiteSpace(manifestPath))
            return OperationResult<OrchestrationReasoningTrainerReport>.Fail("Payload manifest path is required.", "ORCHESTRATION_REASONING_TRAINER_MANIFEST_PATH_REQUIRED");
        if (!File.Exists(payloadPath))
            return OperationResult<OrchestrationReasoningTrainerReport>.Fail("Payload file not found.", "ORCHESTRATION_REASONING_TRAINER_PAYLOAD_NOT_FOUND");
        if (!File.Exists(manifestPath))
            return OperationResult<OrchestrationReasoningTrainerReport>.Fail("Payload manifest file not found.", "ORCHESTRATION_REASONING_TRAINER_MANIFEST_NOT_FOUND");

        try
        {
            var admission = await OrchestrationReasoningSourceAdmission.AdmitAsync(
                payloadPath,
                manifestPath,
                sourceAdmissionLedgerPath,
                sourceAdmissionEvidencePath,
                ct);
            if (!admission.IsSuccess)
                return OperationResult<OrchestrationReasoningTrainerReport>.Fail(
                    admission.ErrorMessage,
                    admission.ErrorCode);

            var manifestJson = await File.ReadAllTextAsync(manifestPath, Encoding.UTF8, ct);
            using var manifestDocument = JsonDocument.Parse(manifestJson);
            var manifest = manifestDocument.RootElement;
            var payloadSha = await HashFileAsync(payloadPath, ct);
            var manifestPayloadSha = RequiredString(manifest, "payloads_sha256");
            if (!string.Equals(payloadSha, manifestPayloadSha, StringComparison.Ordinal))
                return OperationResult<OrchestrationReasoningTrainerReport>.Fail("Payload hash does not match manifest.", "ORCHESTRATION_REASONING_TRAINER_PAYLOAD_HASH_MISMATCH");
            if (RequiredBool(manifest, "raw_private_text_copied"))
                return OperationResult<OrchestrationReasoningTrainerReport>.Fail("Manifest reports raw private text copied.", "ORCHESTRATION_REASONING_TRAINER_RAW_PRIVATE_TEXT_REJECTED");
            if (RequiredBool(manifest, "technical_execution_leakage"))
                return OperationResult<OrchestrationReasoningTrainerReport>.Fail("Manifest reports technical execution leakage.", "ORCHESTRATION_REASONING_TRAINER_TECHNICAL_LEAKAGE_REJECTED");
            if (RequiredBool(manifest, "external_ai_used"))
                return OperationResult<OrchestrationReasoningTrainerReport>.Fail("Manifest reports external AI use.", "ORCHESTRATION_REASONING_TRAINER_EXTERNAL_AI_REJECTED");
            if (RequiredBool(manifest, "training_started"))
                return OperationResult<OrchestrationReasoningTrainerReport>.Fail("Manifest reports training already started.", "ORCHESTRATION_REASONING_TRAINER_STARTED_FLAG_REJECTED");

            var payloadRows = await ReadPayloadRowsAsync(payloadPath, ct);
            if (payloadRows.Count == 0)
                return OperationResult<OrchestrationReasoningTrainerReport>.Fail("At least one payload row is required.", "ORCHESTRATION_REASONING_TRAINER_PAYLOAD_ROWS_REQUIRED");
            if (payloadRows.Count != RequiredInt(manifest, "row_count"))
                return OperationResult<OrchestrationReasoningTrainerReport>.Fail("Payload row count does not match manifest.", "ORCHESTRATION_REASONING_TRAINER_ROW_COUNT_MISMATCH");

            var patterns = new List<OrchestrationReasoningLearnedPattern>();
            var acceptedPairIds = new List<string>();
            foreach (var row in payloadRows)
            {
                var rowResult = BuildPattern(row);
                if (!rowResult.IsSuccess)
                    return OperationResult<OrchestrationReasoningTrainerReport>.Fail(rowResult.ErrorMessage, rowResult.ErrorCode);
                patterns.Add(rowResult.Value!);
                acceptedPairIds.Add(RequiredString(row, "pair_id"));
            }

            if (patterns.Count == 0)
                return OperationResult<OrchestrationReasoningTrainerReport>.Fail("No learned patterns were materialized.", "ORCHESTRATION_REASONING_TRAINER_NO_LEARNED_PATTERNS");

            var sourcePairsSha = RequiredString(manifest, "source_pairs_sha256");
            var learnedStateManifestId = ResolveLearnedStateManifestId(
                admission.Value!.ManifestPath,
                learnedStateManifestRef);
            if (!learnedStateManifestId.IsSuccess)
                return learnedStateManifestId.MapError<OrchestrationReasoningTrainerReport>();
            var learnedState = BuildLearnedState(
                patterns,
                admission.Value.Decision.ManifestSha256,
                sourcePairsSha,
                learnedStateManifestId.Value!);
            var admittedState = OrchestrationReasoningSourceAdmission.CreateAdmitted(
                learnedState,
                [admission.Value]);
            var report = new OrchestrationReasoningTrainerReport(
                TrainerRunId: "or-04b-trainer-implementation-01",
                RowCount: payloadRows.Count,
                AcceptedPairIds: acceptedPairIds,
                PayloadSha256: payloadSha,
                SourcePairSha256: sourcePairsSha,
                LearnedPatternCount: patterns.Count,
                TrainingStarted: false,
                ModelAssetCreated: false,
                CheckpointCreated: false,
                SemanticQualityClaimed: false,
                ExcludedTechnicalRefsTrainable: false,
                LearnedState: learnedState,
                AdmittedState: admittedState);

            return OperationResult<OrchestrationReasoningTrainerReport>.Ok(report);
        }
        catch (JsonException ex)
        {
            return OperationResult<OrchestrationReasoningTrainerReport>.Fail(ex.Message, "ORCHESTRATION_REASONING_TRAINER_JSON_INVALID");
        }
        catch (IOException ex)
        {
            return OperationResult<OrchestrationReasoningTrainerReport>.Fail(ex.Message, "ORCHESTRATION_REASONING_TRAINER_IO_FAILED");
        }
    }

    private static async Task<IReadOnlyList<JsonElement>> ReadPayloadRowsAsync(
        string payloadPath,
        CancellationToken ct)
    {
        var rows = new List<JsonElement>();
        foreach (var line in await File.ReadAllLinesAsync(payloadPath, Encoding.UTF8, ct))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            using var document = JsonDocument.Parse(line);
            rows.Add(document.RootElement.Clone());
        }

        return rows;
    }

    internal static OperationResult<IReadOnlyList<OrchestrationReasoningLearnedPattern>>
        MaterializePatternsFromPayload(byte[] payloadBytes)
    {
        try
        {
            var rows = new List<JsonElement>();
            foreach (var line in Encoding.UTF8.GetString(payloadBytes).Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                using var document = JsonDocument.Parse(line);
                rows.Add(document.RootElement.Clone());
            }
            var patterns = new List<OrchestrationReasoningLearnedPattern>();
            foreach (var row in rows)
            {
                var result = BuildPattern(row);
                if (!result.IsSuccess)
                    return result.MapError<IReadOnlyList<OrchestrationReasoningLearnedPattern>>();
                patterns.Add(result.Value!);
            }
            return OperationResult<IReadOnlyList<OrchestrationReasoningLearnedPattern>>.Ok(patterns);
        }
        catch (JsonException ex)
        {
            return OperationResult<IReadOnlyList<OrchestrationReasoningLearnedPattern>>.Fail(
                ex.Message,
                "ORCHESTRATION_REASONING_TRAINER_JSON_INVALID");
        }
    }

    private static OperationResult<OrchestrationReasoningLearnedPattern> BuildPattern(JsonElement row)
    {
        var pairId = RequiredString(row, "pair_id");
        RequiredString(row, "source_row_id");
        RequiredString(row, "approved_pair_ref", required: false);
        var errorCode = RequiredString(row, "error_code");
        RequiredString(row, "chosen_payload_ref");
        RequiredString(row, "rejected_payload_ref");
        RequiredString(row, "chosen_feature_contract_ref");
        RequiredString(row, "rejected_feature_contract_ref");
        var trainerInputRef = RequiredString(row, "trainer_input_contract_ref");
        var chosenRef = RequiredString(row, "chosen_ref");
        RequiredString(row, "rejected_ref");
        RequiredString(row, "source_ref");
        RequiredString(row, "source_sha256");
        if (!string.Equals(RequiredString(row, "model_leg"), "common", StringComparison.Ordinal))
            return OperationResult<OrchestrationReasoningLearnedPattern>.Fail("Only common-leg payload rows are accepted in this trainer slice.", "ORCHESTRATION_REASONING_TRAINER_COMMON_LEG_REQUIRED");
        if (!string.Equals(RequiredString(row, "abstraction_layer"), "universal_reasoning", StringComparison.Ordinal))
            return OperationResult<OrchestrationReasoningLearnedPattern>.Fail("Only universal reasoning payload rows are accepted.", "ORCHESTRATION_REASONING_TRAINER_UNIVERSAL_LAYER_REQUIRED");
        if (RequiredBool(row, "raw_private_text_copied") || RequiredBool(row, "technical_execution_leakage") || RequiredBool(row, "external_ai_used") || RequiredBool(row, "training_started"))
            return OperationResult<OrchestrationReasoningLearnedPattern>.Fail("Payload row has a forbidden readiness or leakage flag.", "ORCHESTRATION_REASONING_TRAINER_FORBIDDEN_ROW_FLAG");
        if (!RequiredBool(row, "dpo_payload_authored"))
            return OperationResult<OrchestrationReasoningLearnedPattern>.Fail("Payload row must be authored before trainer materialization.", "ORCHESTRATION_REASONING_TRAINER_PAYLOAD_NOT_AUTHORED");
        if (RequiredBool(row, "dpo_train_ready"))
            return OperationResult<OrchestrationReasoningLearnedPattern>.Fail("Payload row must not be train-ready in this trainer materialization slice.", "ORCHESTRATION_REASONING_TRAINER_TRAIN_READY_REJECTED");

        var excludedRefs = RequiredStringArray(row, "excluded_technical_execution_refs");
        var chosenFeatures = RequiredObject(row, "chosen_scenario_plan_features");
        var rejectedFeatures = RequiredObject(row, "rejected_scenario_plan_features");
        if (ContainsExcludedTechnicalRef(chosenFeatures, rejectedFeatures, trainerInputRef, excludedRefs))
            return OperationResult<OrchestrationReasoningLearnedPattern>.Fail("Excluded technical execution refs must remain metadata-only.", "ORCHESTRATION_REASONING_TRAINER_EXCLUDED_REF_LEAK");

        var intentSignals = RequiredStringArray(chosenFeatures, "intent_signals");
        var reasoningSteps = RequiredStringArray(chosenFeatures, "reasoning_steps");
        var evidenceExpectations = RequiredStringArray(chosenFeatures, "evidence_expectations");
        var decisionGuards = RequiredStringArray(chosenFeatures, "decision_guards");
        var universalPattern = RequiredString(row, "universal_reasoning_pattern");
        var rejectedSignals = RequiredStringArray(rejectedFeatures, "near_miss_signals");
        if (intentSignals.Count == 0 || reasoningSteps.Count == 0 || evidenceExpectations.Count == 0 || decisionGuards.Count == 0 || rejectedSignals.Count == 0)
            return OperationResult<OrchestrationReasoningLearnedPattern>.Fail("Chosen/rejected feature payloads must be concrete.", "ORCHESTRATION_REASONING_TRAINER_FEATURES_REQUIRED");

        if (row.TryGetProperty("learned_pattern", out var learnedPattern)
            && learnedPattern.ValueKind == JsonValueKind.Object)
        {
            var fixturePattern = JsonSerializer.Deserialize<OrchestrationReasoningLearnedPattern>(
                learnedPattern.GetRawText(),
                JsonOptions);
            if (fixturePattern is null)
                return OperationResult<OrchestrationReasoningLearnedPattern>.Fail(
                    "Bound learned pattern could not be parsed.",
                    "ORCHESTRATION_REASONING_TRAINER_FEATURES_REQUIRED");
            return OperationResult<OrchestrationReasoningLearnedPattern>.Ok(fixturePattern);
        }

        var pattern = new OrchestrationReasoningLearnedPattern(
            PatternId: $"or-learned-pattern:{pairId}",
            ScenarioRef: chosenRef,
            RequiredTaskTypes: ["universal_reasoning"],
            RequiredConstraintSignals: intentSignals,
            RequiredRequirementSignals: reasoningSteps,
            RequiredDeliverableSignals: [universalPattern, .. decisionGuards],
            RequiredEvidenceSignals: evidenceExpectations,
            PhaseFamilies: ["orchestration-reasoning", "dpo-materialization"],
            GateFamilies: ["dpo-universalization", "anti-leakage", errorCode],
            AssetFamilies: ["llm_mvp_core", "training-data/orchestration-reasoning"],
            HostCheckSets: ["core-only"],
            RiskFlags: ["no_checkpoint_claim", "no_semantic_quality_metric_claim", "excluded_technical_refs_metadata_only"],
            NextGranularizationActions: ["run parent review before train-ready mutation", "implement checkpoint writer only in a later accepted gate"],
            Confidence: 0.8);

        return OperationResult<OrchestrationReasoningLearnedPattern>.Ok(pattern);
    }

    internal static OrchestrationReasoningLearnedGuideStateDescriptor BuildLearnedState(
        IReadOnlyList<OrchestrationReasoningLearnedPattern> patterns,
        string manifestSha,
        string sourcePairsSha,
        string manifestId)
        => OrchestrationReasoningCanonicalState.CreateDescriptor(
            stateId: "orchestration-reasoning-state:common:or-04b-trainer-implementation-01",
            guideStateRef: "guide-state:orchestration-reasoning:common:trainer-materialized:v2",
            manifestId: manifestId.Replace('\\', '/'),
            manifestSha256: manifestSha,
            sourceBaseCommit: $"source-pairs-sha256:{sourcePairsSha}",
            patterns);

    private static OperationResult<string> ResolveLearnedStateManifestId(
        string verifiedManifestPath,
        string? learnedStateManifestRef)
    {
        if (string.IsNullOrWhiteSpace(learnedStateManifestRef))
            return OperationResult<string>.Ok(verifiedManifestPath);

        var portableRef = learnedStateManifestRef.Trim().Replace('\\', '/');
        if (portableRef.StartsWith("/", StringComparison.Ordinal)
            || portableRef.Contains(":/", StringComparison.Ordinal)
            || portableRef.Contains(":\\", StringComparison.Ordinal)
            || portableRef.Contains(':')
            || portableRef.Contains(';'))
        {
            return OperationResult<string>.Fail(
                "Portable learned-state manifest ref must be a plain bundle-local path without drive, URI, or merge syntax.",
                "ORCHESTRATION_REASONING_TRAINER_PORTABLE_MANIFEST_REF_INVALID");
        }

        var segments = portableRef
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0
            || segments.Any(segment => string.Equals(segment, ".", StringComparison.Ordinal)
                || string.Equals(segment, "..", StringComparison.Ordinal)))
        {
            return OperationResult<string>.Fail(
                "Portable learned-state manifest ref must name a file inside the checkpoint bundle.",
                "ORCHESTRATION_REASONING_TRAINER_PORTABLE_MANIFEST_REF_INVALID");
        }

        return OperationResult<string>.Ok(string.Join('/', segments)); // The persisted id is platform-neutral, bundle-local, and still bound by the verified manifest hash.
    }

    private static bool ContainsExcludedTechnicalRef(
        JsonElement chosenFeatures,
        JsonElement rejectedFeatures,
        string trainerInputRef,
        IReadOnlyList<string> excludedRefs)
    {
        var trainableText = JsonSerializer.Serialize(chosenFeatures, JsonOptions) + JsonSerializer.Serialize(rejectedFeatures, JsonOptions) + trainerInputRef;
        return excludedRefs.Any(excludedRef => trainableText.Contains(excludedRef, StringComparison.OrdinalIgnoreCase));
    }

    private static JsonElement RequiredObject(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Object)
            throw new JsonException($"Required object '{name}' is missing.");
        return value;
    }

    private static string RequiredString(JsonElement element, string name, bool required = true)
    {
        if (!element.TryGetProperty(name, out var value))
        {
            if (!required)
                return string.Empty;
            throw new JsonException($"Required string '{name}' is missing.");
        }
        if (value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
            throw new JsonException($"Required string '{name}' is empty.");
        return value.GetString()!;
    }

    private static bool RequiredBool(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            throw new JsonException($"Required boolean '{name}' is missing.");
        return value.GetBoolean();
    }

    private static int RequiredInt(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var number))
            throw new JsonException($"Required integer '{name}' is missing.");
        return number;
    }

    private static IReadOnlyList<string> RequiredStringArray(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Array)
            throw new JsonException($"Required array '{name}' is missing.");
        return value.EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() : null)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item!)
            .ToArray();
    }

    private static async Task<string> HashFileAsync(string path, CancellationToken ct)
        => HashBytes(await File.ReadAllBytesAsync(path, ct));

    private static string HashBytes(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
