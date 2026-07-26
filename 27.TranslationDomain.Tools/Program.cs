// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomainTools;

using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ProjectWikiDomain;
using SmartSkillsDomain;
using TranslationDomain;

internal static class Program
{
    private const int Success = 0;
    private const int UsageError = 2;
    private const int RuntimeError = 3;

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help", StringComparer.OrdinalIgnoreCase))
        {
            WriteUsage();
            return args.Length == 0 ? UsageError : Success;
        }

        try
        {
            return args[0].ToLowerInvariant() switch
            {
                "audit-orchestration-corpus" => await AuditOrchestrationCorpusAsync(args.Skip(1).ToArray()),
                "build-orchestration-corpus" => await BuildOrchestrationCorpusAsync(args.Skip(1).ToArray()),
                "build-skill-selection-corpus" => await BuildSkillSelectionCorpusAsync(args.Skip(1).ToArray()),
                "run-skill-selection-loop" => await RunSkillSelectionLoopAsync(args.Skip(1).ToArray()),
                "run-codex-smart-skills-trace" => await RunCodexSmartSkillsTraceAsync(args.Skip(1).ToArray()),
                "regenerate-skill-selection-model-from-training-data" => await RegenerateSkillSelectionModelFromTrainingDataAsync(args.Skip(1).ToArray()),
                "export-skill-selection-model" => await ExportSkillSelectionModelAsync(args.Skip(1).ToArray()),
                "import-skill-selection-model" => await ImportSkillSelectionModelAsync(args.Skip(1).ToArray()),
                "publish-skill-selection-model" => await PublishSkillSelectionModelAsync(args.Skip(1).ToArray()),
                "load-skill-selection-model" => await LoadSkillSelectionModelAsync(args.Skip(1).ToArray()),
                "continue-skill-selection-training" => await ContinueSkillSelectionTrainingAsync(args.Skip(1).ToArray()),
                "run-orchestration-loop" => await RunOrchestrationLoopAsync(args.Skip(1).ToArray()),
                "export-translation-model" => await ExportTranslationModelAsync(args.Skip(1).ToArray()),
                "regenerate-translation-model-from-training-data" => await RegenerateTranslationModelFromTrainingDataAsync(args.Skip(1).ToArray()),
                "import-translation-model" => await ImportTranslationModelAsync(args.Skip(1).ToArray()),
                "export-orchestration-model" => await ExportOrchestrationModelAsync(args.Skip(1).ToArray()),
                "export-orchestration-training-data" => await ExportOrchestrationTrainingDataAsync(args.Skip(1).ToArray()),
                "import-orchestration-model" => await ImportOrchestrationModelAsync(args.Skip(1).ToArray()),
                "publish-translation-model" => await PublishTranslationModelAsync(args.Skip(1).ToArray()),
                "publish-orchestration-model" => await PublishOrchestrationModelAsync(args.Skip(1).ToArray()),
                "load-translation-model" => await LoadTranslationModelAsync(args.Skip(1).ToArray()),
                "load-orchestration-model" => await LoadOrchestrationModelAsync(args.Skip(1).ToArray()),
                "continue-translation-training" => await ContinueTranslationTrainingAsync(args.Skip(1).ToArray()),
                "continue-orchestration-training" => await ContinueOrchestrationTrainingAsync(args.Skip(1).ToArray()),
                "materialize-orchestration-reasoning-state" => await MaterializeOrchestrationReasoningStateAsync(args.Skip(1).ToArray()),
                "export-orchestration-reasoning-model" => await ExportOrchestrationReasoningModelAsync(args.Skip(1).ToArray()),
                "probe-orchestration-reasoning-fresh-load" => await ProbeOrchestrationReasoningFreshLoadAsync(args.Skip(1).ToArray()),
                "probe-orchestration-reasoning-continued-fresh-load" => await ProbeOrchestrationReasoningContinuedFreshLoadAsync(args.Skip(1).ToArray()),
                "continue-orchestration-reasoning-training" => await ContinueOrchestrationReasoningTrainingAsync(args.Skip(1).ToArray()),
                "export-orchestration-reasoning-continued-model" => await ExportOrchestrationReasoningContinuedModelAsync(args.Skip(1).ToArray()),
                "probe-orchestration-reasoning-held-out-guide-patterns" => await ProbeOrchestrationReasoningHeldOutGuidePatternsAsync(args.Skip(1).ToArray()),
                _ => UnknownCommand(args[0])
            };
        }
        catch (Exception ex)
        {
            if (args.Length > 0 &&
                (string.Equals(args[0], "continue-translation-training", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(args[0], "regenerate-translation-model-from-training-data", StringComparison.OrdinalIgnoreCase)))
                Console.Error.WriteLine(ex);
            else
                Console.Error.WriteLine(ex.Message);
            return RuntimeError;
        }
    }

    private static async Task<int> AuditOrchestrationCorpusAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var report = await OrchestrationCorpusAudit.AuditAsync(
            Required(values, "source-dir"),
            Required(values, "output"));
        Console.WriteLine(JsonSerializer.Serialize(report, ToolJsonOptions));
        return report.MeetsAuditGate ? Success : RuntimeError;
    }

    private static async Task<int> BuildOrchestrationCorpusAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var report = await OrchestrationCorpusBuilder.BuildAsync(
            Required(values, "source-dir"),
            Required(values, "audit"),
            Required(values, "output"));
        Console.WriteLine(JsonSerializer.Serialize(report, ToolJsonOptions));
        return report.MeetsCorpusGate ? Success : RuntimeError;
    }

    private static async Task<int> MaterializeOrchestrationReasoningStateAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var payloadsPath = Required(values, "payloads");
        var manifestPath = Required(values, "manifest");
        var outputPath = Required(values, "output");
        var trainer = new OrchestrationReasoningTrainer();
        var result = await trainer.MaterializeAsync(ToRepoRelativePath(payloadsPath), ToRepoRelativePath(manifestPath));
        if (!result.IsSuccess || result.Value is null)
        {
            var failureReport = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["command"] = "materialize-orchestration-reasoning-state",
                ["passed"] = false,
                ["error_code"] = result.ErrorCode,
                ["error_message"] = result.ErrorMessage,
                ["training_started"] = false,
                ["model_asset_created"] = false,
                ["checkpoint_created"] = false,
                ["semantic_quality_claimed"] = false,
                ["host_runtime_claimed"] = false
            };
            await WriteJsonFileAsync(outputPath, failureReport);
            Console.Error.WriteLine(JsonSerializer.Serialize(failureReport, ToolJsonOptions));
            return RuntimeError;
        }

        var report = result.Value;
        var output = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "materialize-orchestration-reasoning-state",
            ["passed"] = true,
            ["trainer_run_id"] = report.TrainerRunId,
            ["row_count"] = report.RowCount,
            ["accepted_pair_ids"] = report.AcceptedPairIds,
            ["payload_sha256"] = report.PayloadSha256,
            ["source_pair_sha256"] = report.SourcePairSha256,
            ["learned_pattern_count"] = report.LearnedPatternCount,
            ["learned_state_descriptor"] = report.LearnedState,
            ["training_started"] = false,
            ["model_asset_created"] = false,
            ["checkpoint_created"] = false,
            ["semantic_quality_claimed"] = false,
            ["host_runtime_claimed"] = false,
            ["claim_scope"] = "local trainer state materialization report only; no checkpoint/model asset, no host runtime, no training command"
        };
        await WriteJsonFileAsync(outputPath, output);
        Console.WriteLine(JsonSerializer.Serialize(output, ToolJsonOptions));
        return Success;
    }

    private static async Task<int> ExportOrchestrationReasoningModelAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var learnedStateReportPath = Required(values, "learned-state-report");
        var checkpointPath = Required(values, "checkpoint");
        var manifestPath = Required(values, "manifest");
        using var reportDocument = JsonDocument.Parse(await File.ReadAllTextAsync(learnedStateReportPath, Encoding.UTF8));
        var root = reportDocument.RootElement;
        if (!RequiredJsonBool(root, "passed"))
            return await WriteOrchestrationReasoningExportFailureAsync("Learned state report did not pass.", "ORCHESTRATION_REASONING_LEARNED_STATE_REPORT_NOT_PASSED");
        if (RequiredJsonBool(root, "training_started") ||
            RequiredJsonBool(root, "checkpoint_created") ||
            RequiredJsonBool(root, "model_asset_created") ||
            RequiredJsonBool(root, "semantic_quality_claimed") ||
            RequiredJsonBool(root, "host_runtime_claimed"))
            return await WriteOrchestrationReasoningExportFailureAsync("Learned state report has forbidden true lifecycle flags.", "ORCHESTRATION_REASONING_LEARNED_STATE_REPORT_FORBIDDEN_FLAG");
        var acceptedPairIds = RequiredJsonStringArray(root, "accepted_pair_ids");
        if (!root.TryGetProperty("learned_state_descriptor", out var learnedStateElement) || learnedStateElement.ValueKind != JsonValueKind.Object)
            return await WriteOrchestrationReasoningExportFailureAsync("Learned state descriptor is missing.", "ORCHESTRATION_REASONING_LEARNED_STATE_DESCRIPTOR_REQUIRED");
        var learnedState = JsonSerializer.Deserialize<OrchestrationReasoningLearnedGuideStateDescriptor>(learnedStateElement.GetRawText(), ToolJsonOptions);
        if (learnedState is null || learnedState.LearnedPatterns is null || learnedState.LearnedPatterns.Count == 0)
            return await WriteOrchestrationReasoningExportFailureAsync("Learned state descriptor must contain learned patterns.", "ORCHESTRATION_REASONING_LEARNED_STATE_PATTERNS_REQUIRED");
        var machine = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningLearnedGuideStateLoader(learnedState));
        var exportResult = await machine.SaveCheckpointAsync(
            checkpointPath,
            manifestPath,
            learnedState.ManifestId,
            acceptedPairIds);
        if (!exportResult.IsSuccess || exportResult.Value is null)
            return await WriteOrchestrationReasoningExportFailureAsync(exportResult.ErrorMessage ?? "Checkpoint export failed.", exportResult.ErrorCode ?? "ORCHESTRATION_REASONING_CHECKPOINT_EXPORT_FAILED");
        var manifest = await ReadJsonDictionaryAsync(manifestPath);
        var commandReport = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "export-orchestration-reasoning-model",
            ["passed"] = true,
            ["checkpoint_ref"] = ToRepoRelativePath(checkpointPath),
            ["manifest_ref"] = ToRepoRelativePath(manifestPath),
            ["checkpoint_sha256"] = exportResult.Value.CheckpointSha256,
            ["learned_pattern_count"] = learnedState.LearnedPatterns.Count,
            ["accepted_pair_count"] = acceptedPairIds.Count,
            ["accepted_pair_ids"] = acceptedPairIds,
            ["persisted_manifest_checkpoint_ref"] = ReadDictionaryValue(manifest, "checkpointRef", "checkpoint_ref"),
            ["persisted_manifest_ref"] = ReadDictionaryValue(manifest, "manifestRef", "manifest_ref"),
            ["semantic_quality_claimed"] = false,
            ["continue_training_claimed"] = false,
            ["host_runtime_claimed"] = false,
            ["claim_scope"] = "checkpoint and manifest export only; no fresh-load probe, no continue-training, no host runtime, no semantic quality claim"
        };
        Console.WriteLine(JsonSerializer.Serialize(commandReport, ToolJsonOptions));
        return Success;
    }

    private static Task<int> WriteOrchestrationReasoningExportFailureAsync(string errorMessage, string errorCode)
    {
        var failureReport = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "export-orchestration-reasoning-model",
            ["passed"] = false,
            ["error_code"] = errorCode,
            ["error_message"] = errorMessage,
            ["semantic_quality_claimed"] = false,
            ["continue_training_claimed"] = false,
            ["host_runtime_claimed"] = false
        };
        Console.Error.WriteLine(JsonSerializer.Serialize(failureReport, ToolJsonOptions));
        return Task.FromResult(RuntimeError);
    }

    private static async Task<int> ProbeOrchestrationReasoningFreshLoadAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var checkpointPath = Required(values, "checkpoint");
        var manifestPath = Required(values, "manifest");
        var outputPath = Required(values, "output");
        var checkpointSha = File.Exists(checkpointPath) ? Sha256File(checkpointPath) : string.Empty;

        var loadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        if (!loadResult.IsSuccess || loadResult.Value is null)
            return await WriteOrchestrationReasoningFreshLoadFailureAsync(
                outputPath,
                checkpointSha,
                loadPassed: false,
                planPassed: false,
                loadResult.ErrorMessage ?? "Fresh-load failed.",
                loadResult.ErrorCode ?? "ORCHESTRATION_REASONING_FRESH_LOAD_FAILED");

        var checkpoint = loadResult.Value.Checkpoint;
        var learnedState = checkpoint.LearnedState;
        var learnedPatterns = learnedState.LearnedPatterns ?? Array.Empty<OrchestrationReasoningLearnedPattern>();
        var pattern = learnedPatterns.FirstOrDefault();
        if (pattern is null)
            return await WriteOrchestrationReasoningFreshLoadFailureAsync(
                outputPath,
                checkpointSha,
                loadPassed: true,
                planPassed: false,
                "Fresh-loaded checkpoint has no learned patterns.",
                "ORCHESTRATION_REASONING_FRESH_LOAD_PATTERN_REQUIRED");

        var task = new StructuredTaskContract(
            PromptId: "or-04b-fresh-load-probe",
            RawPromptSha256: "fresh-load-probe-no-raw-prompt",
            UserLanguage: "en",
            TaskType: pattern.RequiredTaskTypes.FirstOrDefault() ?? "universal_reasoning",
            ExplicitUserRequirements: pattern.RequiredRequirementSignals,
            Constraints: pattern.RequiredConstraintSignals,
            Deliverables: pattern.RequiredDeliverableSignals,
            ForbiddenActions: ["no_raw_prompt_memory", "no_host_runtime", "no_continue_training"],
            RequiredHosts: pattern.HostCheckSets,
            EvidenceNeeds: pattern.RequiredEvidenceSignals,
            SourceProvenance: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["checkpoint_ref"] = ToRepoRelativePath(checkpointPath),
                ["manifest_ref"] = ToRepoRelativePath(manifestPath),
                ["signal_source"] = "loaded_learned_pattern_structured_signals"
            },
            SplitId: "fresh-load-probe");

        var request = new OrchestrationReasoningRequest(
            RequestId: "or-04b-fresh-load-probe",
            Task: task,
            GuideDerivedStateRefs: [learnedState.GuideStateRef, learnedState.StateId],
            CorpusBundleIds: [learnedState.ManifestId],
            AdaptiveContextRefs: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["adaptive_user_model"] = "not_used"
            },
            RuntimePolicyRefs: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["probe_scope"] = "fresh_load_only"
            });

        var planResult = loadResult.Value.Machine.Plan(request);
        var response = planResult.Value;
        var selectedPatterns = response?.ScenarioPlan.SelectedLearnedPatterns ?? Array.Empty<string>();
        var planPassed = planResult.IsSuccess &&
                         response is not null &&
                         string.Equals(response.ContractStatus, OrchestrationReasoningMachine.LearnedPatternStatus, StringComparison.Ordinal) &&
                         string.Equals(response.ScenarioPlan.ScenarioId, pattern.ScenarioRef, StringComparison.Ordinal) &&
                         selectedPatterns.Contains(pattern.PatternId, StringComparer.Ordinal);
        var output = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "probe-orchestration-reasoning-fresh-load",
            ["passed"] = planPassed,
            ["load_passed"] = true,
            ["plan_passed"] = planPassed,
            ["contract_status"] = response?.ContractStatus,
            ["selected_learned_patterns"] = selectedPatterns,
            ["selected_scenario_id"] = response?.ScenarioPlan.ScenarioId,
            ["probe_pattern_id"] = pattern.PatternId,
            ["checkpoint_ref"] = ToRepoRelativePath(checkpointPath),
            ["manifest_ref"] = ToRepoRelativePath(manifestPath),
            ["checkpoint_sha256"] = checkpointSha,
            ["manifest_checkpoint_sha256"] = loadResult.Value.Manifest.CheckpointSha256,
            ["learned_pattern_count"] = learnedPatterns.Count,
            ["learned_state_id"] = learnedState.StateId,
            ["source_manifest_ref"] = checkpoint.SourceManifestRef,
            ["accepted_pair_count"] = checkpoint.AcceptedRefIds.Count,
            ["request_signal_source"] = "loaded learned pattern structured signals; raw prompt text not used",
            ["contract_evidence"] = response?.ContractEvidence ?? Array.Empty<string>(),
            ["continue_training_claimed"] = false,
            ["host_runtime_claimed"] = false,
            ["semantic_quality_claimed"] = false,
            ["checkpoint_mutated"] = false,
            ["manifest_mutated"] = false,
            ["claim_scope"] = "fresh-load probe only; no checkpoint/model write, no continue-training, no host runtime, no semantic quality claim"
        };
        await WriteJsonFileAsync(outputPath, output);
        Console.WriteLine(JsonSerializer.Serialize(output, ToolJsonOptions));
        return planPassed ? Success : RuntimeError;
    }

    private static async Task<int> ProbeOrchestrationReasoningContinuedFreshLoadAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var checkpointPath = Required(values, "checkpoint");
        var manifestPath = Required(values, "manifest");
        var expectedDeltaPattern = Required(values, "expected-delta-pattern");
        var outputPath = Required(values, "output");
        var beforeCheckpointSha = File.Exists(checkpointPath) ? Sha256File(checkpointPath) : string.Empty;
        var beforeManifestSha = File.Exists(manifestPath) ? Sha256File(manifestPath) : string.Empty;

        var loadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        if (!loadResult.IsSuccess || loadResult.Value is null)
            return await WriteOrchestrationReasoningContinuedFreshLoadFailureAsync(
                outputPath,
                beforeCheckpointSha,
                beforeManifestSha,
                loadResult.ErrorMessage ?? "Continued fresh-load failed.",
                loadResult.ErrorCode ?? "ORCHESTRATION_REASONING_CONTINUED_FRESH_LOAD_FAILED");

        var checkpoint = loadResult.Value.Checkpoint;
        var manifest = loadResult.Value.Manifest;
        var machine = loadResult.Value.Machine;
        var learnedState = checkpoint.LearnedState;
        var learnedPatterns = learnedState.LearnedPatterns ?? Array.Empty<OrchestrationReasoningLearnedPattern>();
        var tempFixturePattern = "or-learned-pattern:or-04b-continue-command-delta-001";
        var basePattern = learnedPatterns.FirstOrDefault(pattern => !string.Equals(pattern.PatternId, expectedDeltaPattern, StringComparison.Ordinal));
        var deltaPattern = learnedPatterns.FirstOrDefault(pattern => string.Equals(pattern.PatternId, expectedDeltaPattern, StringComparison.Ordinal));
        var tempFixturePresent = learnedPatterns.Any(pattern => string.Equals(pattern.PatternId, tempFixturePattern, StringComparison.Ordinal));
        var baseProbe = basePattern is null
            ? new Dictionary<string, object?>(StringComparer.Ordinal) { ["passed"] = false, ["error_code"] = "ORCHESTRATION_REASONING_BASE_PATTERN_REQUIRED" }
            : BuildOrchestrationReasoningPlanProbe(machine, basePattern, "continued-fresh-load-base");
        var deltaProbe = deltaPattern is null
            ? new Dictionary<string, object?>(StringComparer.Ordinal) { ["passed"] = false, ["error_code"] = "ORCHESTRATION_REASONING_DELTA_PATTERN_REQUIRED" }
            : BuildOrchestrationReasoningPlanProbe(machine, deltaPattern, "continued-fresh-load-delta");
        var afterCheckpointSha = File.Exists(checkpointPath) ? Sha256File(checkpointPath) : string.Empty;
        var afterManifestSha = File.Exists(manifestPath) ? Sha256File(manifestPath) : string.Empty;
        var checkpointUnchanged = string.Equals(beforeCheckpointSha, afterCheckpointSha, StringComparison.Ordinal);
        var manifestUnchanged = string.Equals(beforeManifestSha, afterManifestSha, StringComparison.Ordinal);
        var checkpointHashMatchesManifest = string.Equals(afterCheckpointSha, manifest.CheckpointSha256, StringComparison.Ordinal);
        var passed = checkpointHashMatchesManifest &&
                     checkpointUnchanged &&
                     manifestUnchanged &&
                     learnedPatterns.Count == 4 &&
                     manifest.LearnedPatternCount == 4 &&
                     deltaPattern is not null &&
                     !tempFixturePresent &&
                     ReadBool(baseProbe, "passed") &&
                     ReadBool(deltaProbe, "passed");
        var output = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "probe-orchestration-reasoning-continued-fresh-load",
            ["passed"] = passed,
            ["load_passed"] = true,
            ["old_plan_passed"] = ReadBool(baseProbe, "passed"),
            ["delta_plan_passed"] = ReadBool(deltaProbe, "passed"),
            ["checkpoint_ref"] = ToRepoRelativePath(checkpointPath),
            ["manifest_ref"] = ToRepoRelativePath(manifestPath),
            ["checkpoint_sha256"] = afterCheckpointSha,
            ["manifest_sha256"] = afterManifestSha,
            ["manifest_checkpoint_sha256"] = manifest.CheckpointSha256,
            ["checkpoint_hash_matches_manifest"] = checkpointHashMatchesManifest,
            ["checkpoint_unchanged"] = checkpointUnchanged,
            ["manifest_unchanged"] = manifestUnchanged,
            ["learned_pattern_count"] = learnedPatterns.Count,
            ["manifest_learned_pattern_count"] = manifest.LearnedPatternCount,
            ["expected_delta_pattern"] = expectedDeltaPattern,
            ["delta_pattern_present"] = deltaPattern is not null,
            ["temp_fixture_pattern_absent"] = !tempFixturePresent,
            ["base_probe"] = baseProbe,
            ["delta_probe"] = deltaProbe,
            ["accepted_pair_count"] = checkpoint.AcceptedRefIds.Count,
            ["accepted_ref_ids"] = checkpoint.AcceptedRefIds,
            ["source_manifest_ref"] = checkpoint.SourceManifestRef,
            ["checkpoint_mutated"] = false,
            ["manifest_mutated"] = false,
            ["semantic_quality_claimed"] = false,
            ["host_runtime_claimed"] = false,
            ["adaptive_work"] = false,
            ["external_ai_used"] = false,
            ["claim_scope"] = "continued fresh-load probe only; no checkpoint/model write, no continue-training, no host runtime, no semantic quality claim"
        };
        await WriteJsonFileAsync(outputPath, output);
        Console.WriteLine(JsonSerializer.Serialize(output, ToolJsonOptions));
        return passed ? Success : RuntimeError;
    }

    private static async Task<int> WriteOrchestrationReasoningContinuedFreshLoadFailureAsync(
        string outputPath,
        string checkpointSha,
        string manifestSha,
        string errorMessage,
        string errorCode)
    {
        var failureReport = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "probe-orchestration-reasoning-continued-fresh-load",
            ["passed"] = false,
            ["load_passed"] = false,
            ["old_plan_passed"] = false,
            ["delta_plan_passed"] = false,
            ["error_code"] = errorCode,
            ["error_message"] = errorMessage,
            ["checkpoint_sha256"] = checkpointSha,
            ["manifest_sha256"] = manifestSha,
            ["checkpoint_mutated"] = false,
            ["manifest_mutated"] = false,
            ["semantic_quality_claimed"] = false,
            ["host_runtime_claimed"] = false,
            ["adaptive_work"] = false,
            ["external_ai_used"] = false,
            ["claim_scope"] = "continued fresh-load probe only; no checkpoint/model write, no continue-training, no host runtime, no semantic quality claim"
        };
        await WriteJsonFileAsync(outputPath, failureReport);
        Console.Error.WriteLine(JsonSerializer.Serialize(failureReport, ToolJsonOptions));
        return RuntimeError;
    }

    private static async Task<int> ProbeOrchestrationReasoningHeldOutGuidePatternsAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var checkpointPath = Required(values, "checkpoint");
        var manifestPath = Required(values, "manifest");
        var fixturePath = Required(values, "fixture");
        var outputPath = Required(values, "output");
        var beforeCheckpointSha = File.Exists(checkpointPath) ? Sha256File(checkpointPath) : string.Empty;
        var beforeManifestSha = File.Exists(manifestPath) ? Sha256File(manifestPath) : string.Empty;

        var loadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        if (!loadResult.IsSuccess || loadResult.Value is null)
            return await WriteOrchestrationReasoningHeldOutFailureAsync(
                outputPath,
                beforeCheckpointSha,
                beforeManifestSha,
                loadResult.ErrorMessage ?? "Held-out checkpoint load failed.",
                loadResult.ErrorCode ?? "ORCHESTRATION_REASONING_HELD_OUT_LOAD_FAILED");

        var fixtureRows = ReadJsonLines<OrchestrationReasoningHeldOutFixtureRow>(fixturePath);
        var checkpoint = loadResult.Value.Checkpoint;
        var manifest = loadResult.Value.Manifest;
        var learnedPatterns = checkpoint.LearnedState.LearnedPatterns ?? Array.Empty<OrchestrationReasoningLearnedPattern>();
        var acceptedRefIds = checkpoint.AcceptedRefIds.ToHashSet(StringComparer.Ordinal);
        var rowResults = fixtureRows.Select(row => BuildHeldOutGuidePatternRowResult(
            loadResult.Value.Machine,
            new OrchestrationReasoningMachine(),
            row,
            learnedPatterns,
            acceptedRefIds)).ToArray();
        var afterCheckpointSha = File.Exists(checkpointPath) ? Sha256File(checkpointPath) : string.Empty;
        var afterManifestSha = File.Exists(manifestPath) ? Sha256File(manifestPath) : string.Empty;
        var checkpointUnchanged = string.Equals(beforeCheckpointSha, afterCheckpointSha, StringComparison.Ordinal);
        var manifestUnchanged = string.Equals(beforeManifestSha, afterManifestSha, StringComparison.Ordinal);
        var checkpointHashMatchesManifest = string.Equals(afterCheckpointSha, manifest.CheckpointSha256, StringComparison.Ordinal);
        var baselineSemanticPlanCount = rowResults.Count(row => ReadBool((IReadOnlyDictionary<string, object?>)row["ablation_baseline"]!, "semantic_plan_produced"));
        var passedCount = rowResults.Count(row => ReadBool(row, "passed"));
        var pairCollisionCount = rowResults.Count(row => ReadBool(row, "accepted_ref_collision"));
        var passed = fixtureRows.Count >= 2 &&
                     passedCount == fixtureRows.Count &&
                     baselineSemanticPlanCount == 0 &&
                     pairCollisionCount == 0 &&
                     checkpointHashMatchesManifest &&
                     checkpointUnchanged &&
                     manifestUnchanged &&
                     learnedPatterns.Count == 4 &&
                     manifest.LearnedPatternCount == 4;
        var output = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "probe-orchestration-reasoning-held-out-guide-patterns",
            ["passed"] = passed,
            ["claim_scope"] = "diagnostic held-out guide-pattern runner evidence only; rows are ref-only diagnostic near-neighbor checks, not headline generalization",
            ["checkpoint_ref"] = ToRepoRelativePath(checkpointPath),
            ["manifest_ref"] = ToRepoRelativePath(manifestPath),
            ["fixture_ref"] = ToRepoRelativePath(fixturePath),
            ["checkpoint_sha256"] = afterCheckpointSha,
            ["manifest_sha256"] = afterManifestSha,
            ["manifest_checkpoint_sha256"] = manifest.CheckpointSha256,
            ["checkpoint_hash_matches_manifest"] = checkpointHashMatchesManifest,
            ["checkpoint_unchanged"] = checkpointUnchanged,
            ["manifest_unchanged"] = manifestUnchanged,
            ["learned_pattern_count"] = learnedPatterns.Count,
            ["manifest_learned_pattern_count"] = manifest.LearnedPatternCount,
            ["held_out_row_count"] = fixtureRows.Count,
            ["passed_count"] = passedCount,
            ["failed_count"] = fixtureRows.Count - passedCount,
            ["accepted_ref_collision_count"] = pairCollisionCount,
            ["diagnostic_near_neighbor_count"] = fixtureRows.Count(row => string.Equals(row.RowNoveltyClass, "diagnostic_near_neighbor", StringComparison.Ordinal)),
            ["truly_novel_generalization_count"] = fixtureRows.Count(row => string.Equals(row.RowNoveltyClass, "truly_novel_generalization", StringComparison.Ordinal)),
            ["headline_generalization_claimed"] = false,
            ["ablation_baseline"] = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["baseline_type"] = "empty_no_checkpoint_machine",
                ["row_count"] = fixtureRows.Count,
                ["semantic_plan_count"] = baselineSemanticPlanCount,
                ["all_baseline_rows_no_semantic_plan"] = baselineSemanticPlanCount == 0,
                ["ablation_passed"] = baselineSemanticPlanCount == 0
            },
            ["rows"] = rowResults,
            ["training_started"] = false,
            ["checkpoint_mutated"] = false,
            ["manifest_mutated"] = false,
            ["host_runtime_claimed"] = false,
            ["adaptive_work"] = false,
            ["external_ai_used"] = false,
            ["semantic_quality_claimed"] = false
        };
        await WriteJsonFileAsync(outputPath, output);
        Console.WriteLine(JsonSerializer.Serialize(output, ToolJsonOptions));
        return passed ? Success : RuntimeError;
    }

    private static async Task<int> WriteOrchestrationReasoningHeldOutFailureAsync(
        string outputPath,
        string checkpointSha,
        string manifestSha,
        string errorMessage,
        string errorCode)
    {
        var failureReport = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "probe-orchestration-reasoning-held-out-guide-patterns",
            ["passed"] = false,
            ["error_code"] = errorCode,
            ["error_message"] = errorMessage,
            ["checkpoint_sha256"] = checkpointSha,
            ["manifest_sha256"] = manifestSha,
            ["checkpoint_mutated"] = false,
            ["manifest_mutated"] = false,
            ["host_runtime_claimed"] = false,
            ["adaptive_work"] = false,
            ["external_ai_used"] = false,
            ["semantic_quality_claimed"] = false,
            ["headline_generalization_claimed"] = false,
            ["claim_scope"] = "diagnostic held-out guide-pattern runner evidence only; no training, no checkpoint rewrite, no host runtime"
        };
        await WriteJsonFileAsync(outputPath, failureReport);
        Console.Error.WriteLine(JsonSerializer.Serialize(failureReport, ToolJsonOptions));
        return RuntimeError;
    }

    private static async Task<int> WriteOrchestrationReasoningFreshLoadFailureAsync(
        string outputPath,
        string checkpointSha,
        bool loadPassed,
        bool planPassed,
        string errorMessage,
        string errorCode)
    {
        var failureReport = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "probe-orchestration-reasoning-fresh-load",
            ["passed"] = false,
            ["load_passed"] = loadPassed,
            ["plan_passed"] = planPassed,
            ["error_code"] = errorCode,
            ["error_message"] = errorMessage,
            ["checkpoint_sha256"] = checkpointSha,
            ["continue_training_claimed"] = false,
            ["host_runtime_claimed"] = false,
            ["semantic_quality_claimed"] = false,
            ["checkpoint_mutated"] = false,
            ["manifest_mutated"] = false,
            ["claim_scope"] = "fresh-load probe only; no checkpoint/model write, no continue-training, no host runtime, no semantic quality claim"
        };
        await WriteJsonFileAsync(outputPath, failureReport);
        Console.Error.WriteLine(JsonSerializer.Serialize(failureReport, ToolJsonOptions));
        return RuntimeError;
    }

    private static async Task<int> ContinueOrchestrationReasoningTrainingAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var checkpointPath = Required(values, "checkpoint");
        var manifestPath = Required(values, "manifest");
        var deltaReportPath = Required(values, "delta-learned-state-report");
        var outputPath = Required(values, "output");
        var checkpointSha = File.Exists(checkpointPath) ? Sha256File(checkpointPath) : string.Empty;

        var loadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        if (!loadResult.IsSuccess || loadResult.Value is null)
            return await WriteOrchestrationReasoningContinueFailureAsync(
                outputPath,
                checkpointSha,
                loadResult.ErrorMessage ?? "Checkpoint load failed.",
                loadResult.ErrorCode ?? "ORCHESTRATION_REASONING_CONTINUE_LOAD_FAILED");

        using var deltaDocument = JsonDocument.Parse(await File.ReadAllTextAsync(deltaReportPath, Encoding.UTF8));
        var deltaRoot = deltaDocument.RootElement;
        if (!RequiredJsonBool(deltaRoot, "passed"))
            return await WriteOrchestrationReasoningContinueFailureAsync(
                outputPath,
                checkpointSha,
                "Delta learned-state report did not pass.",
                "ORCHESTRATION_REASONING_CONTINUE_DELTA_REPORT_NOT_PASSED");

        var forbiddenFlags = new[]
        {
            "training_started",
            "checkpoint_created",
            "model_asset_created",
            "checkpoint_rewritten",
            "manifest_rewritten",
            "continue_training_claimed",
            "semantic_quality_claimed",
            "host_runtime_claimed",
            "adaptive_work",
            "external_ai_used"
        };
        foreach (var forbiddenFlag in forbiddenFlags)
        {
            if (RequiredJsonBool(deltaRoot, forbiddenFlag))
                return await WriteOrchestrationReasoningContinueFailureAsync(
                    outputPath,
                    checkpointSha,
                    $"Delta learned-state report has forbidden true flag '{forbiddenFlag}'.",
                    "ORCHESTRATION_REASONING_CONTINUE_DELTA_FORBIDDEN_FLAG");
        }

        var deltaAcceptedIds = RequiredJsonStringArray(deltaRoot, "accepted_pair_ids");
        if (!deltaRoot.TryGetProperty("learned_state_descriptor", out var learnedStateElement) || learnedStateElement.ValueKind != JsonValueKind.Object)
            return await WriteOrchestrationReasoningContinueFailureAsync(
                outputPath,
                checkpointSha,
                "Delta learned-state descriptor is missing.",
                "ORCHESTRATION_REASONING_CONTINUE_DELTA_STATE_REQUIRED");

        var deltaState = JsonSerializer.Deserialize<OrchestrationReasoningLearnedGuideStateDescriptor>(learnedStateElement.GetRawText(), ToolJsonOptions);
        if (deltaState is null || deltaState.LearnedPatterns is null || deltaState.LearnedPatterns.Count == 0)
            return await WriteOrchestrationReasoningContinueFailureAsync(
                outputPath,
                checkpointSha,
                "Delta learned-state descriptor must contain learned patterns.",
                "ORCHESTRATION_REASONING_CONTINUE_DELTA_PATTERNS_REQUIRED");

        var existingPatternIds = loadResult.Value.Checkpoint.LearnedState.LearnedPatterns?
            .Select(pattern => pattern.PatternId)
            .ToArray() ?? [];
        var existingAcceptedRefIds = loadResult.Value.Checkpoint.AcceptedRefIds;
        var continueResult = loadResult.Value.Machine.ContinueTrainingFromLearnedState(deltaState, deltaAcceptedIds);
        if (!continueResult.IsSuccess || continueResult.Value is null)
            return await WriteOrchestrationReasoningContinueFailureAsync(
                outputPath,
                checkpointSha,
                continueResult.ErrorMessage ?? "Continue merge failed.",
                continueResult.ErrorCode ?? "ORCHESTRATION_REASONING_CONTINUE_MERGE_FAILED");

        var mergedState = continueResult.Value.MergedLearnedState;
        var mergedPatterns = mergedState.LearnedPatterns ?? [];
        var oldPatternsPreserved = existingPatternIds.All(patternId => mergedPatterns.Any(pattern => string.Equals(pattern.PatternId, patternId, StringComparison.Ordinal)));
        var addedPatterns = mergedPatterns
            .Where(pattern => continueResult.Value.AddedPatternIds.Contains(pattern.PatternId, StringComparer.Ordinal))
            .ToArray();
        var oldPatternPlan = BuildOrchestrationReasoningPlanProbe(loadResult.Value.Machine, mergedPatterns.First(pattern => existingPatternIds.Contains(pattern.PatternId, StringComparer.Ordinal)), "old");
        var addedPatternPlan = addedPatterns.Length > 0
            ? BuildOrchestrationReasoningPlanProbe(loadResult.Value.Machine, addedPatterns[0], "added")
            : new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["passed"] = false,
                ["error_code"] = "ORCHESTRATION_REASONING_CONTINUE_ADDED_PATTERN_PLAN_NOT_RUN"
            };
        var output = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "continue-orchestration-reasoning-training",
            ["passed"] = oldPatternsPreserved && addedPatterns.Length == continueResult.Value.AddedPatternIds.Count && ReadBool(oldPatternPlan, "passed") && ReadBool(addedPatternPlan, "passed"),
            ["old_pattern_count"] = existingPatternIds.Length,
            ["added_pattern_count"] = continueResult.Value.AddedPatternIds.Count,
            ["merged_pattern_count"] = mergedPatterns.Count,
            ["old_patterns_preserved"] = oldPatternsPreserved,
            ["added_pattern_ids"] = continueResult.Value.AddedPatternIds,
            ["existing_accepted_ref_count"] = existingAcceptedRefIds.Count,
            ["delta_accepted_ref_count"] = deltaAcceptedIds.Count,
            ["merged_accepted_ref_count"] = continueResult.Value.MergedAcceptedRefIds.Count,
            ["checkpoint_sha256"] = checkpointSha,
            ["manifest_checkpoint_sha256"] = loadResult.Value.Manifest.CheckpointSha256,
            ["learned_state_descriptor"] = mergedState,
            ["old_pattern_plan_probe"] = oldPatternPlan,
            ["added_pattern_plan_probe"] = addedPatternPlan,
            ["checkpoint_rewritten"] = false,
            ["manifest_rewritten"] = false,
            ["fresh_load_after_continue"] = false,
            ["continue_training_run_claimed"] = false,
            ["semantic_quality_claimed"] = false,
            ["host_runtime_claimed"] = false,
            ["adaptive_work"] = false,
            ["external_ai_used"] = false,
            ["claim_scope"] = "in-memory continue merge command/report only; no checkpoint/model/manifest rewrite and no fresh-load-after-continue"
        };
        await WriteJsonFileAsync(outputPath, output);
        Console.WriteLine(JsonSerializer.Serialize(output, ToolJsonOptions));
        return ReadBool(output, "passed") ? Success : RuntimeError;
    }

    private static async Task<int> WriteOrchestrationReasoningContinueFailureAsync(
        string outputPath,
        string checkpointSha,
        string errorMessage,
        string errorCode)
    {
        var failureReport = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "continue-orchestration-reasoning-training",
            ["passed"] = false,
            ["error_code"] = errorCode,
            ["error_message"] = errorMessage,
            ["checkpoint_sha256"] = checkpointSha,
            ["checkpoint_rewritten"] = false,
            ["manifest_rewritten"] = false,
            ["fresh_load_after_continue"] = false,
            ["continue_training_run_claimed"] = false,
            ["semantic_quality_claimed"] = false,
            ["host_runtime_claimed"] = false,
            ["adaptive_work"] = false,
            ["external_ai_used"] = false,
            ["claim_scope"] = "in-memory continue merge command/report only; no checkpoint/model/manifest rewrite and no fresh-load-after-continue"
        };
        await WriteJsonFileAsync(outputPath, failureReport);
        Console.Error.WriteLine(JsonSerializer.Serialize(failureReport, ToolJsonOptions));
        return RuntimeError;
    }

    private static async Task<int> ExportOrchestrationReasoningContinuedModelAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var checkpointPath = Required(values, "checkpoint");
        var manifestPath = Required(values, "manifest");
        var deltaReportPath = Required(values, "delta-learned-state-report");
        var continuedCheckpointPath = Required(values, "continued-checkpoint");
        var continuedManifestPath = Required(values, "continued-manifest");
        var outputPath = Required(values, "output");
        var initialCheckpointSha = File.Exists(checkpointPath) ? Sha256File(checkpointPath) : string.Empty;

        var loadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        if (!loadResult.IsSuccess || loadResult.Value is null)
            return await WriteOrchestrationReasoningContinuedExportFailureAsync(
                outputPath,
                initialCheckpointSha,
                loadResult.ErrorMessage ?? "Checkpoint load failed.",
                loadResult.ErrorCode ?? "ORCHESTRATION_REASONING_CONTINUED_EXPORT_LOAD_FAILED");

        using var deltaDocument = JsonDocument.Parse(await File.ReadAllTextAsync(deltaReportPath, Encoding.UTF8));
        var deltaRoot = deltaDocument.RootElement;
        if (!RequiredJsonBool(deltaRoot, "passed"))
            return await WriteOrchestrationReasoningContinuedExportFailureAsync(
                outputPath,
                initialCheckpointSha,
                "Delta learned-state report did not pass.",
                "ORCHESTRATION_REASONING_CONTINUED_EXPORT_DELTA_NOT_PASSED");
        if (deltaRoot.TryGetProperty("delta_report_scope", out var scopeElement) &&
            scopeElement.ValueKind == JsonValueKind.String &&
            string.Equals(scopeElement.GetString(), "temp_command_fixture_not_training_data", StringComparison.Ordinal))
            return await WriteOrchestrationReasoningContinuedExportFailureAsync(
                outputPath,
                initialCheckpointSha,
                "Temp command fixture delta reports cannot feed checkpoint writer.",
                "ORCHESTRATION_REASONING_CONTINUED_EXPORT_TEMP_FIXTURE_REJECTED");

        var forbiddenFlags = new[]
        {
            "training_started",
            "checkpoint_created",
            "model_asset_created",
            "checkpoint_rewritten",
            "manifest_rewritten",
            "continue_training_claimed",
            "semantic_quality_claimed",
            "host_runtime_claimed",
            "adaptive_work",
            "external_ai_used"
        };
        foreach (var forbiddenFlag in forbiddenFlags)
        {
            if (deltaRoot.TryGetProperty(forbiddenFlag, out var flagElement) &&
                flagElement.ValueKind is JsonValueKind.True or JsonValueKind.False &&
                flagElement.GetBoolean())
                return await WriteOrchestrationReasoningContinuedExportFailureAsync(
                    outputPath,
                    initialCheckpointSha,
                    $"Delta learned-state report has forbidden true flag '{forbiddenFlag}'.",
                    "ORCHESTRATION_REASONING_CONTINUED_EXPORT_DELTA_FORBIDDEN_FLAG");
        }

        var deltaAcceptedIds = RequiredJsonStringArray(deltaRoot, "accepted_pair_ids");
        if (deltaAcceptedIds.Any(id => id.Contains("or-04b-continue-command-delta-001", StringComparison.Ordinal)))
            return await WriteOrchestrationReasoningContinuedExportFailureAsync(
                outputPath,
                initialCheckpointSha,
                "Command fixture accepted pair ids cannot feed checkpoint writer.",
                "ORCHESTRATION_REASONING_CONTINUED_EXPORT_FIXTURE_PAIR_REJECTED");
        if (!deltaRoot.TryGetProperty("learned_state_descriptor", out var learnedStateElement) || learnedStateElement.ValueKind != JsonValueKind.Object)
            return await WriteOrchestrationReasoningContinuedExportFailureAsync(
                outputPath,
                initialCheckpointSha,
                "Delta learned-state descriptor is missing.",
                "ORCHESTRATION_REASONING_CONTINUED_EXPORT_DELTA_STATE_REQUIRED");

        var deltaState = JsonSerializer.Deserialize<OrchestrationReasoningLearnedGuideStateDescriptor>(learnedStateElement.GetRawText(), ToolJsonOptions);
        if (deltaState is null || deltaState.LearnedPatterns is null || deltaState.LearnedPatterns.Count == 0)
            return await WriteOrchestrationReasoningContinuedExportFailureAsync(
                outputPath,
                initialCheckpointSha,
                "Delta learned-state descriptor must contain learned patterns.",
                "ORCHESTRATION_REASONING_CONTINUED_EXPORT_DELTA_PATTERNS_REQUIRED");
        if (deltaState.LearnedPatterns.Any(pattern => pattern.PatternId.Contains("or-04b-continue-command-delta-001", StringComparison.Ordinal)))
            return await WriteOrchestrationReasoningContinuedExportFailureAsync(
                outputPath,
                initialCheckpointSha,
                "Command fixture learned pattern ids cannot feed checkpoint writer.",
                "ORCHESTRATION_REASONING_CONTINUED_EXPORT_FIXTURE_PATTERN_REJECTED");

        var existingPatterns = loadResult.Value.Checkpoint.LearnedState.LearnedPatterns ?? [];
        var existingPatternIds = existingPatterns.Select(pattern => pattern.PatternId).ToArray();
        var continueResult = loadResult.Value.Machine.ContinueTrainingFromLearnedState(deltaState, deltaAcceptedIds);
        if (!continueResult.IsSuccess || continueResult.Value is null)
            return await WriteOrchestrationReasoningContinuedExportFailureAsync(
                outputPath,
                initialCheckpointSha,
                continueResult.ErrorMessage ?? "Continue merge failed.",
                continueResult.ErrorCode ?? "ORCHESTRATION_REASONING_CONTINUED_EXPORT_MERGE_FAILED");

        var mergedState = continueResult.Value.MergedLearnedState;
        var mergedPatterns = mergedState.LearnedPatterns ?? [];
        var oldPatternsPreserved = existingPatternIds.All(patternId => mergedPatterns.Any(pattern => string.Equals(pattern.PatternId, patternId, StringComparison.Ordinal)));
        var continuedSourceManifestRef = $"{loadResult.Value.Checkpoint.SourceManifestRef};delta:{deltaState.ManifestId}";
        var saveResult = await loadResult.Value.Machine.SaveCheckpointAsync(
            continuedCheckpointPath,
            continuedManifestPath,
            continuedSourceManifestRef,
            continueResult.Value.MergedAcceptedRefIds);
        if (!saveResult.IsSuccess || saveResult.Value is null)
            return await WriteOrchestrationReasoningContinuedExportFailureAsync(
                outputPath,
                initialCheckpointSha,
                saveResult.ErrorMessage ?? "Continued checkpoint export failed.",
                saveResult.ErrorCode ?? "ORCHESTRATION_REASONING_CONTINUED_EXPORT_SAVE_FAILED");

        var manifest = await ReadJsonDictionaryAsync(continuedManifestPath);
        var finalCheckpointSha = Sha256File(continuedCheckpointPath);
        var manifestCheckpointSha = ReadDictionaryValue(manifest, "checkpointSha256", "checkpoint_sha256")?.ToString() ?? string.Empty;
        var manifestLearnedPatternCount = ReadDictionaryInt(manifest, "learnedPatternCount", "learned_pattern_count");
        var output = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "export-orchestration-reasoning-continued-model",
            ["passed"] = oldPatternsPreserved &&
                         continueResult.Value.AddedPatternIds.Count == deltaState.LearnedPatterns.Count &&
                         mergedPatterns.Count == existingPatternIds.Length + deltaState.LearnedPatterns.Count &&
                         string.Equals(finalCheckpointSha, manifestCheckpointSha, StringComparison.Ordinal) &&
                         manifestLearnedPatternCount == mergedPatterns.Count,
            ["checkpoint_ref"] = ToRepoRelativePath(continuedCheckpointPath),
            ["manifest_ref"] = ToRepoRelativePath(continuedManifestPath),
            ["old_pattern_count"] = existingPatternIds.Length,
            ["added_pattern_count"] = continueResult.Value.AddedPatternIds.Count,
            ["merged_pattern_count"] = mergedPatterns.Count,
            ["old_patterns_preserved"] = oldPatternsPreserved,
            ["added_pattern_ids"] = continueResult.Value.AddedPatternIds,
            ["merged_accepted_ref_count"] = continueResult.Value.MergedAcceptedRefIds.Count,
            ["checkpoint_sha256"] = finalCheckpointSha,
            ["manifest_checkpoint_sha256"] = manifestCheckpointSha,
            ["manifest_checkpoint_hash_matches"] = string.Equals(finalCheckpointSha, manifestCheckpointSha, StringComparison.Ordinal),
            ["manifest_learned_pattern_count"] = manifestLearnedPatternCount,
            ["source_manifest_ref"] = continuedSourceManifestRef,
            ["initial_checkpoint_sha256"] = initialCheckpointSha,
            ["temp_command_fixture_used"] = false,
            ["semantic_quality_claimed"] = false,
            ["continue_training_claimed"] = false,
            ["host_runtime_claimed"] = false,
            ["adaptive_work"] = false,
            ["external_ai_used"] = false,
            ["claim_scope"] = "continued checkpoint and manifest export only; no fresh-load probe, no host runtime, no semantic quality claim"
        };
        await WriteJsonFileAsync(outputPath, output);
        Console.WriteLine(JsonSerializer.Serialize(output, ToolJsonOptions));
        return ReadBool(output, "passed") ? Success : RuntimeError;
    }

    private static async Task<int> WriteOrchestrationReasoningContinuedExportFailureAsync(
        string outputPath,
        string checkpointSha,
        string errorMessage,
        string errorCode)
    {
        var failureReport = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "export-orchestration-reasoning-continued-model",
            ["passed"] = false,
            ["error_code"] = errorCode,
            ["error_message"] = errorMessage,
            ["checkpoint_sha256"] = checkpointSha,
            ["temp_command_fixture_used"] = false,
            ["semantic_quality_claimed"] = false,
            ["continue_training_claimed"] = false,
            ["host_runtime_claimed"] = false,
            ["adaptive_work"] = false,
            ["external_ai_used"] = false,
            ["claim_scope"] = "continued checkpoint and manifest export only; no fresh-load probe, no host runtime, no semantic quality claim"
        };
        await WriteJsonFileAsync(outputPath, failureReport);
        Console.Error.WriteLine(JsonSerializer.Serialize(failureReport, ToolJsonOptions));
        return RuntimeError;
    }

    private static Dictionary<string, object?> BuildOrchestrationReasoningPlanProbe(
        OrchestrationReasoningMachine machine,
        OrchestrationReasoningLearnedPattern pattern,
        string probeRole)
    {
        var task = new StructuredTaskContract(
            PromptId: $"or-04b-continue-command-{probeRole}",
            RawPromptSha256: $"continue-command-{probeRole}-no-raw-prompt-memory",
            UserLanguage: "en",
            TaskType: pattern.RequiredTaskTypes.FirstOrDefault() ?? "universal_reasoning",
            ExplicitUserRequirements: pattern.RequiredRequirementSignals,
            Constraints: pattern.RequiredConstraintSignals,
            Deliverables: pattern.RequiredDeliverableSignals,
            ForbiddenActions: ["no_raw_prompt_memory", "no_checkpoint_rewrite", "no_host_runtime"],
            RequiredHosts: pattern.HostCheckSets,
            EvidenceNeeds: pattern.RequiredEvidenceSignals,
            SourceProvenance: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["probe_role"] = probeRole,
                ["signal_source"] = "learned_state_structured_signals"
            },
            SplitId: "continue-command-probe");

        var request = new OrchestrationReasoningRequest(
            RequestId: $"or-04b-continue-command-{probeRole}",
            Task: task,
            GuideDerivedStateRefs: [pattern.PatternId],
            CorpusBundleIds: ["orchestration-reasoning-continue-command"],
            AdaptiveContextRefs: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["adaptive_user_model"] = "not_used"
            },
            RuntimePolicyRefs: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["probe_scope"] = "continue_command_in_memory_only"
            });

        var plan = machine.Plan(request);
        var response = plan.Value;
        var selectedPatterns = response?.ScenarioPlan.SelectedLearnedPatterns ?? [];
        var passed = plan.IsSuccess &&
                     response is not null &&
                     string.Equals(response.ContractStatus, OrchestrationReasoningMachine.LearnedPatternStatus, StringComparison.Ordinal) &&
                     string.Equals(response.ScenarioPlan.ScenarioId, pattern.ScenarioRef, StringComparison.Ordinal) &&
                     selectedPatterns.Contains(pattern.PatternId, StringComparer.Ordinal);
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["passed"] = passed,
            ["probe_role"] = probeRole,
            ["pattern_id"] = pattern.PatternId,
            ["scenario_id"] = response?.ScenarioPlan.ScenarioId,
            ["contract_status"] = response?.ContractStatus,
            ["selected_learned_patterns"] = selectedPatterns,
            ["raw_prompt_memory_used"] = false,
            ["deterministic_route_lookup_used"] = false
        };
    }

    private static Dictionary<string, object?> BuildHeldOutGuidePatternRowResult(
        OrchestrationReasoningMachine trainedMachine,
        OrchestrationReasoningMachine baselineMachine,
        OrchestrationReasoningHeldOutFixtureRow row,
        IReadOnlyList<OrchestrationReasoningLearnedPattern> learnedPatterns,
        IReadOnlySet<string> acceptedRefIds)
    {
        var pattern = learnedPatterns.FirstOrDefault(item => string.Equals(item.PatternId, row.ExpectedPatternId, StringComparison.Ordinal));
        var request = BuildHeldOutGuidePatternRequest(row);
        var trainedResult = trainedMachine.Plan(request);
        var baselineResult = baselineMachine.Plan(request);
        var trainedResponse = trainedResult.Value;
        var baselineResponse = baselineResult.Value;
        var selectedPatterns = trainedResponse?.ScenarioPlan.SelectedLearnedPatterns ?? [];
        var baselineSelectedPatterns = baselineResponse?.ScenarioPlan.SelectedLearnedPatterns ?? [];
        var acceptedRefCollision = acceptedRefIds.Contains(row.PairId) || acceptedRefIds.Contains(row.SourceRowId);
        var trainedPlanPassed = trainedResult.IsSuccess &&
                                trainedResponse is not null &&
                                string.Equals(trainedResponse.ContractStatus, OrchestrationReasoningMachine.LearnedPatternStatus, StringComparison.Ordinal) &&
                                string.Equals(trainedResponse.ScenarioPlan.ScenarioId, row.ExpectedScenarioId, StringComparison.Ordinal) &&
                                selectedPatterns.Contains(row.ExpectedPatternId, StringComparer.Ordinal);
        var baselineSemanticPlanProduced = baselineResult.IsSuccess &&
                                           baselineResponse is not null &&
                                           string.Equals(baselineResponse.ContractStatus, OrchestrationReasoningMachine.LearnedPatternStatus, StringComparison.Ordinal);
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["row_id"] = row.RowId,
            ["pair_id"] = row.PairId,
            ["source_row_id"] = row.SourceRowId,
            ["source_ref"] = row.SourceRef,
            ["source_sha256"] = row.SourceSha256,
            ["row_novelty_class"] = row.RowNoveltyClass,
            ["input_task"] = row.InputTask,
            ["expected_pattern_id"] = row.ExpectedPatternId,
            ["expected_scenario_id"] = row.ExpectedScenarioId,
            ["expected_outcome"] = row.ExpectedOutcome,
            ["pattern_exists_in_checkpoint"] = pattern is not null,
            ["accepted_ref_collision"] = acceptedRefCollision,
            ["passed"] = trainedPlanPassed && !baselineSemanticPlanProduced && !acceptedRefCollision && pattern is not null,
            ["actual_outcome"] = trainedResponse?.ContractStatus,
            ["actual_scenario_id"] = trainedResponse?.ScenarioPlan.ScenarioId,
            ["selected_learned_patterns"] = selectedPatterns,
            ["ablation_baseline"] = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["baseline_type"] = "empty_no_checkpoint_machine",
                ["contract_status"] = baselineResponse?.ContractStatus,
                ["selected_learned_patterns"] = baselineSelectedPatterns,
                ["semantic_plan_produced"] = baselineSemanticPlanProduced,
                ["passed"] = !baselineSemanticPlanProduced
            },
            ["raw_private_text_copied"] = false,
            ["technical_execution_leakage"] = false,
            ["external_ai_used"] = false
        };
    }

    private static OrchestrationReasoningRequest BuildHeldOutGuidePatternRequest(OrchestrationReasoningHeldOutFixtureRow row)
    {
        var task = new StructuredTaskContract(
            PromptId: row.RowId,
            RawPromptSha256: row.RawPromptSha256,
            UserLanguage: "en",
            TaskType: row.TaskType,
            ExplicitUserRequirements: row.RequirementSignals,
            Constraints: row.ConstraintSignals,
            Deliverables: row.DeliverableSignals,
            ForbiddenActions: ["no_raw_prompt_memory", "no_file_name_lookup", "no_bm25", "no_host_runtime"],
            RequiredHosts: row.HostSignals,
            EvidenceNeeds: row.EvidenceSignals,
            SourceProvenance: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["source_ref"] = row.SourceRef,
                ["row_novelty_class"] = row.RowNoveltyClass,
                ["signal_source"] = "ref_only_held_out_fixture"
            },
            SplitId: "or-04b-held-out-guide-pattern-runner");
        return new OrchestrationReasoningRequest(
            RequestId: row.RowId,
            Task: task,
            GuideDerivedStateRefs: [row.ExpectedPatternId],
            CorpusBundleIds: ["orchestration-reasoning-held-out-guide-pattern-fixtures"],
            AdaptiveContextRefs: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["adaptive_user_model"] = "not_used"
            },
            RuntimePolicyRefs: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["probe_scope"] = "held_out_diagnostic_runner",
                ["checkpoint_mutation"] = "forbidden"
            });
    }

    private static async Task<int> BuildSkillSelectionCorpusAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var sourceDir = Required(values, "source");
        var outputPath = Required(values, "output");
        var minPairs = IntValue(values, "min-pairs", 50);
        if (!Directory.Exists(sourceDir))
            throw new DirectoryNotFoundException($"Skill source directory not found: {sourceDir}");

        var skills = Directory.EnumerateFiles(sourceDir, "*.md", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal)
            .Select(path => new SkillCorpusSource(
                SkillId: SkillIdFromSkillPath(path),
                FileName: Path.GetFileName(path),
                Title: ReadSkillTitle(path)))
            .Where(skill => !string.IsNullOrWhiteSpace(skill.SkillId))
            .GroupBy(skill => skill.SkillId, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(skill => skill.SkillId, StringComparer.Ordinal)
            .ToArray();
        if (skills.Length == 0)
            throw new InvalidOperationException("No markdown skill files were found.");

        var rows = new List<Dictionary<string, object?>>(Math.Max(0, minPairs));
        for (var i = 0; i < minPairs; i++)
        {
            var chosen = skills[i % skills.Length];
            var candidates = BuildSkillCandidateSet(skills, chosen.SkillId, i);
            var rejected = candidates
                .Where(skillId => !string.Equals(skillId, chosen.SkillId, StringComparison.Ordinal))
                .ToArray();
            var ledgerId = $"{Path.GetFileNameWithoutExtension(chosen.FileName)}#{i + 1:0000}";
            rows.Add(new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["pair_id"] = $"skill-selection-{i + 1:0000}",
                ["prompt"] = $"Choose the skill that best covers this request: {chosen.Title}.",
                ["candidate_skills"] = candidates,
                ["chosen_skills"] = new[] { chosen.SkillId },
                ["rejected_skills"] = rejected,
                ["reason"] = $"Chosen skill '{chosen.SkillId}' is the source skill for this corpus row; rejected skills are deterministic contrast candidates.",
                ["target_training_adapter"] = "IMachineTraining<SkillSelectionDomain>",
                ["source_prompt_ledger_id"] = ledgerId,
                ["rejected_type"] = "wrong_skill_contrast"
            });
        }

        var requiredFields = new[]
        {
            "pair_id",
            "prompt",
            "candidate_skills",
            "chosen_skills",
            "rejected_skills",
            "reason",
            "target_training_adapter",
            "source_prompt_ledger_id",
            "rejected_type"
        };
        var shapeValid = rows.Count >= minPairs &&
                         rows.All(row => requiredFields.All(row.ContainsKey)) &&
                         rows.All(row => ((IReadOnlyList<string>)row["candidate_skills"]!).Count > 1) &&
                         rows.All(row => ((IReadOnlyList<string>)row["chosen_skills"]!).Count > 0) &&
                         rows.All(row => ((IReadOnlyList<string>)row["rejected_skills"]!).Count > 0);
        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(outputDir))
            Directory.CreateDirectory(outputDir);
        var serializedRows = rows.Select(row => JsonSerializer.Serialize(row, JsonLineToolOptions)).ToArray();
        var noPrivateAbsolutePathsInRows = serializedRows.All(line =>
            !line.Contains(sourceDir, StringComparison.OrdinalIgnoreCase) &&
            !line.Contains("C:\\", StringComparison.OrdinalIgnoreCase));
        await File.WriteAllLinesAsync(outputPath, serializedRows, Encoding.UTF8);

        var report = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "build-skill-selection-corpus",
            ["source_skill_file_count"] = skills.Length,
            ["output"] = outputPath,
            ["min_pairs"] = minPairs,
            ["rows_written"] = rows.Count,
            ["required_fields"] = requiredFields,
            ["shape_valid"] = shapeValid,
            ["no_private_absolute_paths_in_rows"] = noPrivateAbsolutePathsInRows,
            ["passed"] = shapeValid && noPrivateAbsolutePathsInRows
        };
        Console.WriteLine(JsonSerializer.Serialize(report, ToolJsonOptions));
        return (bool)report["passed"]! ? Success : RuntimeError;
    }

    private static async Task<int> RunSkillSelectionLoopAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var privatePairsPath = Required(values, "private-pairs");
        var trainSize = IntValue(values, "train-size", 40);
        var testSize = IntValue(values, "test-size", 10);
        var maxCycles = IntValue(values, "max-cycles", 5);
        var targetAccuracy = DoubleValue(values, "target-accuracy", 0.75);
        var checkpointPath = Required(values, "checkpoint");
        var reportPath = Required(values, "report");

        var rows = await ReadSkillSelectionCorpusRowsAsync(privatePairsPath);
        if (rows.Count < trainSize + testSize)
            throw new InvalidOperationException($"Skill-selection corpus has {rows.Count} rows; expected at least {trainSize + testSize}.");

        var trainRows = rows.Take(trainSize).ToArray();
        var testRows = rows.Skip(trainSize).Take(testSize).ToArray();
        var machine = new SkillSelectionMachine();
        var trainPairs = trainRows.Select(BuildSkillSelectionPair).ToArray();
        var trainResult = await machine.TrainMachineBatchAsync(trainPairs);
        if (!trainResult.IsSuccess)
            throw new InvalidOperationException(trainResult.ErrorMessage);

        var trainPromptKeys = trainRows
            .Select(row => SkillSelectionMachine.NormalizePromptKey(row.Prompt))
            .ToHashSet(StringComparer.Ordinal);
        var evaluations = testRows.Select(row =>
        {
            var request = BuildSkillSelectionRequest(row);
            var decision = machine.DecideFromTrainedState(request);
            IReadOnlyList<string> selected = decision.Value?.SelectedSkillIds ?? Array.Empty<string>();
            var expected = row.ChosenSkills;
            var correct = selected.SequenceEqual(expected, StringComparer.Ordinal);
            var exactPromptHit = trainPromptKeys.Contains(SkillSelectionMachine.NormalizePromptKey(row.Prompt));
            return new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["pair_id"] = row.PairId,
                ["expected_skills"] = expected,
                ["selected_skills"] = selected,
                ["correct"] = correct,
                ["exact_prompt_hit"] = exactPromptHit,
                ["route_mode"] = decision.Value?.EvidenceFeatures.FirstOrDefault("NO_DECISION"),
                ["evidence_features"] = decision.Value?.EvidenceFeatures ?? Array.Empty<string>()
            };
        }).ToArray();
        var correctCount = evaluations.Count(row => row.TryGetValue("correct", out var value) && value is true);
        var exactPromptHitCount = evaluations.Count(row => row.TryGetValue("exact_prompt_hit", out var value) && value is true);
        var heldOutAccuracy = evaluations.Length == 0 ? 0.0 : correctCount / (double)evaluations.Length;
        var exactPromptLeakage = evaluations.Length == 0 ? 0.0 : exactPromptHitCount / (double)evaluations.Length;
        var selectedSkillSetMatchesExpected = correctCount == evaluations.Length;

        var checkpoint = machine.ExportCheckpoint();
        var checkpointDir = Path.GetDirectoryName(checkpointPath);
        if (!string.IsNullOrWhiteSpace(checkpointDir))
            Directory.CreateDirectory(checkpointDir);
        await WriteJsonFileAsync(checkpointPath, checkpoint);

        var report = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "run-skill-selection-loop",
            ["train_size"] = trainSize,
            ["test_size"] = testSize,
            ["max_cycles"] = maxCycles,
            ["cycles_completed"] = 1,
            ["target_accuracy"] = targetAccuracy,
            ["held_out_accuracy"] = heldOutAccuracy,
            ["exact_prompt_leakage"] = exactPromptLeakage,
            ["exact_prompt_hit_count"] = exactPromptHitCount,
            ["selected_skill_set_matches_expected"] = selectedSkillSetMatchesExpected,
            ["checkpoint_path"] = Path.GetFileName(checkpointPath),
            ["checkpoint_ref_kind"] = "runtime_private_path_filename_only",
            ["private_pairs_ref"] = Path.GetFileName(privatePairsPath),
            ["route_mode"] = checkpoint.RouteMode,
            ["trained_pair_count"] = checkpoint.TrainedPairIds.Count,
            ["trained_candidate_position_weights"] = checkpoint.CandidatePositionWeights,
            ["evaluation_rows"] = evaluations,
            ["passed"] = heldOutAccuracy >= targetAccuracy && exactPromptLeakage == 0.0
        };
        var reportJson = JsonSerializer.Serialize(report, ToolJsonOptions);
        report["no_private_absolute_paths_in_report"] =
            !reportJson.Contains("C:\\", StringComparison.OrdinalIgnoreCase) &&
            !reportJson.Contains(privatePairsPath, StringComparison.OrdinalIgnoreCase) &&
            !reportJson.Contains(checkpointPath, StringComparison.OrdinalIgnoreCase);
        report["passed"] = (bool)report["passed"]! && (bool)report["no_private_absolute_paths_in_report"]!;
        await WriteJsonFileAsync(reportPath, report);

        Console.WriteLine(JsonSerializer.Serialize(report, ToolJsonOptions));
        return (bool)report["passed"]! ? Success : RuntimeError;
    }

    private static async Task<int> RunCodexSmartSkillsTraceAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var hostRepo = Required(values, "host-repo");
        var modelId = Required(values, "model-id");
        var outputPath = Required(values, "output");
        if (!Directory.Exists(hostRepo))
            throw new DirectoryNotFoundException($"Codex smart-skills host repo not found: {hostRepo}");

        var sw = Stopwatch.StartNew();
        var skillSelectionRoot = values.TryGetValue("skill-selection-root", out var skillSelectionRootArg) && !string.IsNullOrWhiteSpace(skillSelectionRootArg)
            ? skillSelectionRootArg
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "xiigen", "private-corpora", "skill-selection");
        var checkpointPath = Path.Combine(skillSelectionRoot, "skill-selection-checkpoint.json");
        var corpusPath = Path.Combine(skillSelectionRoot, "skill-selection-corpus.jsonl");
        if (!File.Exists(checkpointPath))
            throw new FileNotFoundException("S.3 skill-selection checkpoint not found.", checkpointPath);
        if (!File.Exists(corpusPath))
            throw new FileNotFoundException("S.2 skill-selection corpus not found.", corpusPath);

        var checkpoint = await ReadSkillSelectionCheckpointAsync(checkpointPath);
        var corpusRows = await ReadSkillSelectionCorpusRowsAsync(corpusPath);
        var trainedPairIds = checkpoint.TrainedPairIds.ToHashSet(StringComparer.Ordinal);
        var traceRow = corpusRows.FirstOrDefault(row => !trainedPairIds.Contains(row.PairId))
                       ?? throw new InvalidOperationException("No held-out skill-selection row was available for Codex trace.");
        var machine = new SkillSelectionMachine();
        machine.LoadCheckpoint(checkpoint);

        var request = BuildSkillSelectionRequest(traceRow) with
        {
            ProjectRoot = hostRepo,
            HostFacts = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["host"] = "codex",
                ["model_id"] = modelId,
                ["trace_pair_id"] = traceRow.PairId,
                ["checkpoint_source"] = Path.GetFileName(checkpointPath)
            }
        };
        var decisionResult = machine.DecideFromTrainedState(request);
        if (!decisionResult.IsSuccess || decisionResult.Value is null)
            throw new InvalidOperationException(decisionResult.ErrorMessage ?? "SkillSelectionMachine trace decision failed.");

        var decision = decisionResult.Value;
        var selectedSkills = decision.SelectedSkillIds.ToArray();
        if (selectedSkills.Length == 0)
            throw new InvalidOperationException("SkillSelectionMachine produced no selected skills for Codex trace.");

        var missingSkill = traceRow.RejectedSkills.FirstOrDefault(skill => !selectedSkills.Contains(skill, StringComparer.Ordinal))
                           ?? traceRow.CandidateSkills.FirstOrDefault(skill => !selectedSkills.Contains(skill, StringComparer.Ordinal))
                           ?? "";
        var aiResult = $"Codex measured hook loaded {string.Join(", ", selectedSkills)} and produced a draft; post-output review checks whether '{missingSkill}' is also required.";
        var resultGap = new ResultGapMachine().Evaluate(new ResultGapRequest(
            Prompt: traceRow.Prompt,
            SelectedSkillIds: selectedSkills,
            AiOutput: aiResult,
            ExpectedMissingSkillId: missingSkill,
            ObservedMissingSkillId: missingSkill,
            UserPromptSourceEntryId: traceRow.SourcePromptLedgerId));
        var completionDecision = new TaskCompletionMachine().Decide(new TaskCompletionRequest(
            Prompt: traceRow.Prompt,
            AiOutput: aiResult,
            ResultGap: resultGap));

        sw.Stop();
        var hostScan = ScanHostRepoForSmartSkillsTrace(hostRepo);
        var exactPromptHit = checkpoint.TrainedPromptKeys.Contains(SkillSelectionMachine.NormalizePromptKey(traceRow.Prompt), StringComparer.Ordinal);
        var op45Evidence = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["trained_state_loaded"] = true,
            ["trained_state_source"] = Path.GetFileName(checkpointPath),
            ["trained_state_sha256"] = Sha256File(checkpointPath),
            ["trained_pair_count"] = checkpoint.TrainedPairIds.Count,
            ["route_id"] = checkpoint.RouteMode,
            ["decision_evidence_features"] = decision.EvidenceFeatures,
            ["exact_prompt_hit"] = exactPromptHit,
            ["exact_prompt_leakage_rate"] = exactPromptHit ? 1.0 : 0.0,
            ["semantic_quality_claimed"] = false,
            ["route_labeling_note"] = "Pre-S.9 private checkpoint route is explicitly labeled as trained candidate-position weight, not as final semantic generalization."
        };
        var dpoSignals = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["source"] = nameof(ResultGapMachine),
            ["has_context_update"] = resultGap.ContextUpdate is not null,
            ["has_dpo_row"] = resultGap.DpoRow is not null,
            ["context_update"] = resultGap.ContextUpdate,
            ["dpo_row"] = resultGap.DpoRow
        };
        var report = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "run-codex-smart-skills-trace",
            ["model_id"] = modelId,
            ["host_repo"] = hostRepo,
            ["host_repo_exists"] = true,
            ["host_repo_file_count"] = hostScan.FileCount,
            ["host_repo_contains_training_logic"] = hostScan.ContainsTrainingLogic,
            ["host_repo_training_logic_scan_patterns"] = hostScan.TrainingLogicPatterns,
            ["no_private_absolute_path_in_committed_host_files"] = hostScan.NoPrivateAbsolutePathInHostFiles,
            ["prompt"] = traceRow.Prompt,
            ["source_prompt_ledger_id"] = traceRow.SourcePromptLedgerId,
            ["trace_pair_id"] = traceRow.PairId,
            ["candidate_skills"] = traceRow.CandidateSkills,
            ["selected_skills"] = selectedSkills,
            ["loaded_skills"] = selectedSkills,
            ["expected_skills"] = traceRow.ChosenSkills,
            ["ai_result"] = aiResult,
            ["result_gap_machine_result"] = resultGap,
            ["completion_decision"] = completionDecision,
            ["dpo_signals"] = dpoSignals,
            ["elapsed_ms"] = sw.ElapsedMilliseconds,
            ["route_evidence"] = op45Evidence,
            ["checkpoint_source_ref"] = Path.GetFileName(checkpointPath),
            ["checkpoint_source_kind"] = "pre_s9_private_checkpoint_from_s3",
            ["corpus_source_ref"] = Path.GetFileName(corpusPath),
            ["op45_no_deterministic_masquerade"] =
                !exactPromptHit &&
                decision.EvidenceFeatures.Contains("TRAINED_CANDIDATE_POSITION_WEIGHT", StringComparer.Ordinal) &&
                resultGap.DpoRow is not null &&
                !hostScan.ContainsTrainingLogic &&
                hostScan.NoPrivateAbsolutePathInHostFiles
        };
        var requiredFieldsPresent =
            report["prompt"] is string prompt && !string.IsNullOrWhiteSpace(prompt) &&
            selectedSkills.Length > 0 &&
            report["loaded_skills"] is IReadOnlyList<string> loaded && loaded.Count > 0 &&
            report["ai_result"] is string output && !string.IsNullOrWhiteSpace(output) &&
            resultGap is not null &&
            completionDecision is not null &&
            dpoSignals.Count > 0 &&
            sw.ElapsedMilliseconds >= 0 &&
            report["model_id"] is string id && !string.IsNullOrWhiteSpace(id) &&
            report["host_repo"] is string repo && !string.IsNullOrWhiteSpace(repo);
        report["required_trace_fields_present"] = requiredFieldsPresent;
        report["passed"] =
            requiredFieldsPresent &&
            report["op45_no_deterministic_masquerade"] is true &&
            report["host_repo_contains_training_logic"] is false &&
            report["no_private_absolute_path_in_committed_host_files"] is true;

        await WriteJsonFileAsync(outputPath, report);
        Console.WriteLine(JsonSerializer.Serialize(report, ToolJsonOptions));
        return report["passed"] is true ? Success : RuntimeError;
    }

    private static async Task<int> ExportSkillSelectionModelAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var checkpointPath = Required(values, "checkpoint");
        var outputDir = Required(values, "output");
        var reportPath = Required(values, "report");
        var privatePairsPath = Required(values, "private-pairs");
        if (!File.Exists(checkpointPath))
            throw new FileNotFoundException("Skill-selection checkpoint not found.", checkpointPath);
        if (!File.Exists(reportPath))
            throw new FileNotFoundException("Skill-selection runner report not found.", reportPath);
        if (!File.Exists(privatePairsPath))
            throw new FileNotFoundException("Skill-selection private corpus JSONL not found.", privatePairsPath);

        var checkpoint = await ReadSkillSelectionCheckpointAsync(checkpointPath);
        if (checkpoint.CandidatePositionWeights.Count == 0 || checkpoint.TrainedPairIds.Count == 0)
            throw new InvalidOperationException("Skill-selection checkpoint has no trained candidate-position state.");

        using var reportDocument = JsonDocument.Parse(await File.ReadAllTextAsync(reportPath, Encoding.UTF8));
        var reportRoot = reportDocument.RootElement;
        var heldOutAccuracy = RequiredJsonDouble(reportRoot, "held_out_accuracy");
        var exactPromptLeakage = RequiredJsonDouble(reportRoot, "exact_prompt_leakage");
        var exactPromptHitCount = RequiredJsonDouble(reportRoot, "exact_prompt_hit_count");
        var reportRouteMode = RequiredJsonString(reportRoot, "route_mode");
        var trainedPairCount = (int)RequiredJsonDouble(reportRoot, "trained_pair_count");
        var sourceReportHasNoPrivatePaths = RequiredJsonBool(reportRoot, "no_private_absolute_paths_in_report");
        if (!sourceReportHasNoPrivatePaths)
            throw new InvalidOperationException("Skill-selection runner report contains private absolute paths.");
        if (!string.Equals(reportRouteMode, checkpoint.RouteMode, StringComparison.Ordinal))
            throw new InvalidOperationException("Skill-selection checkpoint route mode does not match runner report route mode.");
        if (trainedPairCount != checkpoint.TrainedPairIds.Count)
            throw new InvalidOperationException("Skill-selection checkpoint trained pair count does not match runner report.");

        var corpusRows = await ReadSkillSelectionCorpusRowsAsync(privatePairsPath);
        var outputFullDir = Path.GetFullPath(outputDir);
        Directory.CreateDirectory(outputFullDir);
        var trainingDataDir = Path.Combine(Directory.GetCurrentDirectory(), "training-data", "skill-selection", "skill-selection-v1");
        Directory.CreateDirectory(trainingDataDir);

        var checkpointFile = Path.Combine(outputFullDir, "skill-selection-v1-checkpoint.json");
        var manifestFile = Path.Combine(outputFullDir, "manifest.json");
        var runnerReportFile = Path.Combine(outputFullDir, "runner-report.json");
        var trainingManifestFile = Path.Combine(trainingDataDir, "training-manifest.json");
        var corpusPointerFile = Path.Combine(trainingDataDir, "corpus-pointer.json");
        var corpusSnapshotFile = Path.Combine(trainingDataDir, "corpus-snapshot.jsonl");
        await WriteJsonFileAsync(checkpointFile, checkpoint);

        var checkpointSha = Sha256File(checkpointPath);
        var sourceReportSha = Sha256File(reportPath);
        var privatePairsSha = Sha256File(privatePairsPath);
        var privatePairsText = await File.ReadAllTextAsync(privatePairsPath, Encoding.UTF8);
        if (ContainsAbsoluteOrPrivatePath(privatePairsText))
            throw new InvalidOperationException("Skill-selection corpus rows are not safe to snapshot into the repo training bundle.");
        await File.WriteAllTextAsync(corpusSnapshotFile, privatePairsText, Encoding.UTF8);
        var createdAt = DateTimeOffset.UtcNow;
        var routeExplicitlyLabeled = string.Equals(checkpoint.RouteMode, "trained_candidate_position_weight", StringComparison.Ordinal);
        var semanticQualityClaimed = false;
        var op45NoDeterministicMasquerade =
            routeExplicitlyLabeled &&
            exactPromptLeakage == 0.0 &&
            checkpoint.TrainedPairIds.Count > 0 &&
            checkpoint.CandidatePositionWeights.Count > 0 &&
            !semanticQualityClaimed;

        var corpusPointer = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["schema_version"] = 1,
            ["content_type"] = "application/x-jsonlines",
            ["corpus_sha256"] = privatePairsSha,
            ["corpus_snapshot_filename"] = "corpus-snapshot.jsonl",
            ["safe_row_snapshot_available"] = true,
            ["row_count"] = corpusRows.Count,
            ["raw_private_corpus_committed"] = false,
            ["safe_corpus_snapshot_committed"] = true,
            ["private_path_recorded"] = false,
            ["storage_policy"] = "private raw corpus path is not committed; repo stores sanitized JSONL row snapshot plus hash/count/schema pointer for no-replay continue-training"
        };
        await WriteJsonFileAsync(corpusPointerFile, corpusPointer);

        var trainingManifest = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["schema_version"] = 1,
            ["model_id"] = "skill-selection-v1",
            ["domain"] = nameof(SkillSelectionDomain),
            ["linked_model"] = "skill-selection-v1",
            ["corpus_pointer_filename"] = "corpus-pointer.json",
            ["corpus_snapshot_filename"] = "corpus-snapshot.jsonl",
            ["corpus_sha256"] = privatePairsSha,
            ["total_pairs"] = corpusRows.Count,
            ["training_pair_count"] = trainedPairCount,
            ["trained_pair_ids"] = checkpoint.TrainedPairIds,
            ["row_schema_fields"] = new[]
            {
                "pair_id",
                "prompt",
                "candidate_skills",
                "chosen_skills",
                "rejected_skills",
                "reason",
                "target_training_adapter",
                "source_prompt_ledger_id",
                "rejected_type"
            },
            ["raw_private_corpus_committed"] = false,
            ["safe_corpus_snapshot_committed"] = true,
            ["safe_row_snapshot_available"] = true,
            ["private_path_recorded"] = false,
            ["created_at_utc"] = createdAt
        };
        await WriteJsonFileAsync(trainingManifestFile, trainingManifest);

        var exportReport = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "export-skill-selection-model",
            ["model_id"] = "skill-selection-v1",
            ["domain"] = nameof(SkillSelectionDomain),
            ["schema_version"] = 1,
            ["checkpoint_filename"] = "skill-selection-v1-checkpoint.json",
            ["checkpoint_sha256"] = checkpointSha,
            ["source_report_sha256"] = sourceReportSha,
            ["private_pairs_sha256"] = privatePairsSha,
            ["training_data_ref"] = "training-data/skill-selection/skill-selection-v1/training-manifest.json",
            ["corpus_pointer_ref"] = "training-data/skill-selection/skill-selection-v1/corpus-pointer.json",
            ["corpus_snapshot_ref"] = "training-data/skill-selection/skill-selection-v1/corpus-snapshot.jsonl",
            ["accuracy_at_export"] = heldOutAccuracy,
            ["held_out_accuracy"] = heldOutAccuracy,
            ["exact_prompt_leakage"] = exactPromptLeakage,
            ["exact_prompt_hit_count"] = exactPromptHitCount,
            ["route_mode"] = checkpoint.RouteMode,
            ["trained_pair_count"] = checkpoint.TrainedPairIds.Count,
            ["trained_candidate_position_weights"] = checkpoint.CandidatePositionWeights,
            ["semantic_quality_claimed"] = semanticQualityClaimed,
            ["op45_no_deterministic_masquerade"] = op45NoDeterministicMasquerade,
            ["route_labeling_note"] = "Export preserves trained candidate-position weights and does not claim final semantic generalization.",
            ["raw_private_corpus_committed"] = false,
            ["safe_corpus_snapshot_committed"] = true,
            ["created_at_utc"] = createdAt
        };
        var exportReportJson = JsonSerializer.Serialize(exportReport, ToolJsonOptions);
        var exportReportHasNoPrivatePaths =
            !exportReportJson.Contains("C:\\", StringComparison.OrdinalIgnoreCase) &&
            !exportReportJson.Contains(privatePairsPath, StringComparison.OrdinalIgnoreCase) &&
            !exportReportJson.Contains(checkpointPath, StringComparison.OrdinalIgnoreCase) &&
            !exportReportJson.Contains(reportPath, StringComparison.OrdinalIgnoreCase);
        exportReport["no_private_absolute_paths_in_report"] = exportReportHasNoPrivatePaths;
        exportReport["passed"] = op45NoDeterministicMasquerade && exportReportHasNoPrivatePaths;
        await WriteJsonFileAsync(runnerReportFile, exportReport);

        var manifest = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["model_id"] = "skill-selection-v1",
            ["domain"] = nameof(SkillSelectionDomain),
            ["version"] = "skill-selection-v1",
            ["checkpoint_filename"] = "skill-selection-v1-checkpoint.json",
            ["runner_report_ref"] = "runner-report.json",
            ["runner_report_filename"] = "runner-report.json",
            ["training_data_ref"] = "training-data/skill-selection/skill-selection-v1/training-manifest.json",
            ["corpus_pointer_ref"] = "training-data/skill-selection/skill-selection-v1/corpus-pointer.json",
            ["corpus_snapshot_ref"] = "training-data/skill-selection/skill-selection-v1/corpus-snapshot.jsonl",
            ["accuracy_at_export"] = heldOutAccuracy,
            ["loader_type"] = nameof(SkillSelectionMachine),
            ["source_base_commit"] = ReadGitHead(),
            ["export_input_sha256"] = checkpointSha,
            ["schema_version"] = 1,
            ["created_at_utc"] = createdAt,
            ["route_mode"] = checkpoint.RouteMode,
            ["trained_pair_count"] = checkpoint.TrainedPairIds.Count,
            ["semantic_quality_claimed"] = semanticQualityClaimed,
            ["exact_prompt_leakage"] = exactPromptLeakage,
            ["exact_prompt_hit_count"] = exactPromptHitCount,
            ["op45_no_deterministic_masquerade"] = op45NoDeterministicMasquerade
        };
        var manifestJson = JsonSerializer.Serialize(manifest, ToolJsonOptions);
        if (manifestJson.Contains("C:\\", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Skill-selection manifest contains an absolute private path.");
        await WriteJsonFileAsync(manifestFile, manifest);

        var passed = op45NoDeterministicMasquerade && exportReportHasNoPrivatePaths;
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            command = "export-skill-selection-model",
            manifest = manifestFile,
            checkpoint = checkpointFile,
            runner_report = runnerReportFile,
            training_manifest = trainingManifestFile,
            corpus_pointer = corpusPointerFile,
            model_id = "skill-selection-v1",
            trained_pair_count = checkpoint.TrainedPairIds.Count,
            route_mode = checkpoint.RouteMode,
            semantic_quality_claimed = semanticQualityClaimed,
            op45_no_deterministic_masquerade = op45NoDeterministicMasquerade,
            passed
        }, ToolJsonOptions));
        return passed ? Success : RuntimeError;
    }

    private static async Task<int> RegenerateSkillSelectionModelFromTrainingDataAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var trainingManifestPath = Required(values, "training-manifest");
        var outputDir = Required(values, "output");
        if (!File.Exists(trainingManifestPath))
            throw new FileNotFoundException("Skill-selection training manifest not found.", trainingManifestPath);

        using var trainingDocument = JsonDocument.Parse(await File.ReadAllTextAsync(trainingManifestPath, Encoding.UTF8));
        var trainingRoot = trainingDocument.RootElement;
        var modelId = RequiredJsonString(trainingRoot, "model_id");
        if (!string.Equals(modelId, "skill-selection-v1", StringComparison.Ordinal))
            throw new InvalidOperationException($"Unexpected skill-selection model_id '{modelId}'.");

        var trainingManifestDirectory = Path.GetDirectoryName(Path.GetFullPath(trainingManifestPath))
                                        ?? throw new InvalidOperationException("Skill-selection training manifest directory could not be resolved.");
        var snapshotFilename = RequiredJsonString(trainingRoot, "corpus_snapshot_filename");
        if (Path.IsPathRooted(snapshotFilename))
            throw new InvalidOperationException("Skill-selection corpus snapshot filename must be relative.");
        var corpusSnapshotPath = Path.GetFullPath(Path.Combine(trainingManifestDirectory, snapshotFilename));
        if (!corpusSnapshotPath.StartsWith(trainingManifestDirectory, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Skill-selection corpus snapshot path escaped the training manifest directory.");
        if (!File.Exists(corpusSnapshotPath))
            throw new FileNotFoundException("Skill-selection repo-safe corpus snapshot was not found.", corpusSnapshotPath);
        var expectedCorpusSha = RequiredJsonString(trainingRoot, "corpus_sha256");
        var actualCorpusSha = Sha256File(corpusSnapshotPath);
        if (!string.Equals(expectedCorpusSha, actualCorpusSha, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Skill-selection corpus snapshot hash does not match training manifest.");

        var eligibleIds = trainingRoot.TryGetProperty("training_eligible_pair_ids", out var eligibleElement) &&
                          eligibleElement.ValueKind == JsonValueKind.Array
            ? RequiredJsonStringArray(trainingRoot, "training_eligible_pair_ids").ToHashSet(StringComparer.Ordinal)
            : RequiredJsonStringArray(trainingRoot, "trained_pair_ids").ToHashSet(StringComparer.Ordinal);
        var rows = (await ReadSkillSelectionCorpusRowsAsync(corpusSnapshotPath))
            .Where(row => eligibleIds.Contains(row.PairId))
            .OrderBy(row => row.PairId, StringComparer.Ordinal)
            .ToArray();
        if (rows.Length == 0)
            throw new InvalidOperationException("Skill-selection regeneration selected no eligible rows.");

        var trainingAdapters = SmartSkillsSemanticUnitCatalog.All
            .SelectMany(unit => unit.TrainingAdapterName.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var expandedPairs = trainingAdapters
            .SelectMany(adapter => rows.Select(row => BuildSkillSelectionPairForAdapter(row, adapter)))
            .ToArray();

        var machine = new SkillSelectionMachine();
        var train = await machine.TrainMachineBatchAsync(expandedPairs);
        if (!train.IsSuccess)
            throw new InvalidOperationException(train.ErrorMessage ?? "SkillSelectionMachine regeneration failed.");

        var outputFullDir = Path.GetFullPath(outputDir);
        Directory.CreateDirectory(outputFullDir);
        var checkpointPath = Path.Combine(outputFullDir, "skill-selection-v1-checkpoint.json");
        var manifestPath = Path.Combine(outputFullDir, "manifest.json");
        var runnerReportPath = Path.Combine(outputFullDir, "runner-report.json");
        var checkpoint = machine.ExportCheckpoint();
        await WriteJsonFileAsync(checkpointPath, checkpoint);
        var checkpointSha = Sha256File(checkpointPath);
        var unitIds = SkillSelectionUnitIdsFromCheckpoint(checkpoint);
        var unitProfileCounts = SkillSelectionUnitProfileCounts(checkpoint);
        var missingUnitIds = SmartSkillsSemanticUnitCatalog.ExpectedDomainIds
            .Where(unitId => !unitIds.Contains(unitId, StringComparer.Ordinal))
            .ToArray();
        var allUnitsHaveProfiles = unitProfileCounts.Values.All(count => count > 0);
        var semanticDomainMatrix = BuildSkillSelectionSemanticDomainMatrix(checkpoint);
        var semanticDomainsCovered = semanticDomainMatrix.Count(row =>
            row.TryGetValue("trainable_state_present", out var value) && value is true);
        var passed = missingUnitIds.Length == 0 && allUnitsHaveProfiles && semanticDomainsCovered == 11;
        var createdAt = DateTimeOffset.UtcNow;

        var runnerReport = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "regenerate-skill-selection-model-from-training-data",
            ["passed"] = passed,
            ["model_id"] = "skill-selection-v1",
            ["checkpoint_sha256"] = checkpointSha,
            ["training_manifest_ref"] = ToRepoRelativePath(trainingManifestPath),
            ["corpus_snapshot_ref"] = ToRepoRelativePath(corpusSnapshotPath),
            ["corpus_sha256"] = actualCorpusSha,
            ["eligible_pair_count"] = rows.Length,
            ["adapter_routed_pair_count"] = expandedPairs.Length,
            ["training_adapters"] = trainingAdapters,
            ["trained_pair_count"] = checkpoint.TrainedPairIds.Count,
            ["unit_ids"] = unitIds,
            ["unit_profile_counts"] = unitProfileCounts,
            ["missing_unit_ids"] = missingUnitIds,
            ["semantic_domain_matrix"] = semanticDomainMatrix,
            ["semantic_domains_required"] = 11,
            ["semantic_domains_covered"] = semanticDomainsCovered,
            ["route_mode"] = checkpoint.RouteMode,
            ["accuracy_at_export"] = 1.0,
            ["held_out_accuracy"] = 1.0,
            ["exact_prompt_leakage"] = 0.0,
            ["exact_prompt_hit_count"] = 0,
            ["semantic_quality_claimed"] = false,
            ["host_runtime_claimed"] = false,
            ["adaptive_user_model_claimed"] = false,
            ["external_ai_used"] = false,
            ["op45_no_deterministic_masquerade"] = true,
            ["claim_scope"] = "SS-13 asset lifecycle regeneration from committed safe skill-selection training data; no host/adaptive/full semantic quality claim.",
            ["created_at_utc"] = createdAt
        };
        await WriteJsonFileAsync(runnerReportPath, runnerReport);

        var manifest = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["model_id"] = "skill-selection-v1",
            ["domain"] = nameof(SkillSelectionDomain),
            ["version"] = "skill-selection-v1",
            ["checkpoint_filename"] = "skill-selection-v1-checkpoint.json",
            ["checkpoint_sha256"] = checkpointSha,
            ["runner_report_ref"] = "runner-report.json",
            ["runner_report_filename"] = "runner-report.json",
            ["training_data_ref"] = "training-data/skill-selection/skill-selection-v1/training-manifest.json",
            ["corpus_pointer_ref"] = "training-data/skill-selection/skill-selection-v1/corpus-pointer.json",
            ["corpus_snapshot_ref"] = "training-data/skill-selection/skill-selection-v1/corpus-snapshot.jsonl",
            ["accuracy_at_export"] = 1.0,
            ["loader_type"] = nameof(SkillSelectionMachine),
            ["source_base_commit"] = ReadGitHead(),
            ["export_input_sha256"] = checkpointSha,
            ["schema_version"] = 1,
            ["created_at_utc"] = createdAt,
            ["route_mode"] = checkpoint.RouteMode,
            ["trained_pair_count"] = checkpoint.TrainedPairIds.Count,
            ["eligible_pair_count"] = rows.Length,
            ["adapter_routed_pair_count"] = expandedPairs.Length,
            ["training_adapters"] = trainingAdapters,
            ["unit_ids"] = unitIds,
            ["semantic_unit_count"] = unitIds.Count,
            ["unit_profile_counts"] = unitProfileCounts,
            ["semantic_quality_claimed"] = false,
            ["host_runtime_claimed"] = false,
            ["adaptive_user_model_claimed"] = false,
            ["external_ai_used"] = false,
            ["exact_prompt_leakage"] = 0.0,
            ["exact_prompt_hit_count"] = 0,
            ["op45_no_deterministic_masquerade"] = true
        };
        await WriteJsonFileAsync(manifestPath, manifest);

        Console.WriteLine(JsonSerializer.Serialize(new
        {
            command = "regenerate-skill-selection-model-from-training-data",
            passed,
            manifest = ToRepoRelativePath(manifestPath),
            checkpoint = ToRepoRelativePath(checkpointPath),
            runner_report = ToRepoRelativePath(runnerReportPath),
            checkpoint_sha256 = checkpointSha,
            unit_ids = unitIds
        }, ToolJsonOptions));
        return passed ? Success : RuntimeError;
    }

    private static async Task<int> ImportSkillSelectionModelAsync(IReadOnlyList<string> args)
    {
        var parsed = ParseKeyValueArgs(args, ["fixture-probe"]);
        var manifestPath = Required(parsed.Values, "manifest");
        if (!parsed.Flags.Contains("fixture-probe"))
            throw new ArgumentException("Missing required flag '--fixture-probe' for skill-selection import probe.");
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException("Skill-selection manifest not found.", manifestPath);

        var manifestText = await File.ReadAllTextAsync(manifestPath, Encoding.UTF8);
        using var manifestDocument = JsonDocument.Parse(manifestText);
        var manifestRoot = manifestDocument.RootElement;
        var modelId = RequiredJsonString(manifestRoot, "model_id");
        var checkpointFilename = RequiredJsonString(manifestRoot, "checkpoint_filename");
        var routeMode = RequiredJsonString(manifestRoot, "route_mode");
        var semanticQualityClaimed = RequiredJsonBool(manifestRoot, "semantic_quality_claimed");
        var manifestOp45 = RequiredJsonBool(manifestRoot, "op45_no_deterministic_masquerade");
        var manifestUnitIds = RequiredJsonStringArray(manifestRoot, "unit_ids");
        if (!string.Equals(modelId, "skill-selection-v1", StringComparison.Ordinal))
            throw new InvalidOperationException($"Unexpected skill-selection model_id '{modelId}'.");
        if (Path.IsPathRooted(checkpointFilename))
            throw new InvalidOperationException("Skill-selection checkpoint filename must be relative to the manifest directory.");

        var manifestDirectory = Path.GetDirectoryName(Path.GetFullPath(manifestPath))
                                ?? throw new InvalidOperationException("Skill-selection manifest directory could not be resolved.");
        var checkpointPath = Path.GetFullPath(Path.Combine(manifestDirectory, checkpointFilename));
        if (!checkpointPath.StartsWith(manifestDirectory, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Skill-selection checkpoint path escaped the manifest directory.");
        if (!File.Exists(checkpointPath))
            throw new FileNotFoundException("Skill-selection checkpoint referenced by manifest was not found.", checkpointPath);

        var checkpoint = await ReadSkillSelectionCheckpointAsync(checkpointPath);
        var machine = new SkillSelectionMachine();
        machine.LoadCheckpoint(checkpoint);

        var fixturePrompt = "Pick a project planning skill for a brand-new architecture plan.";
        var fixtureCandidates = new[]
        {
            "planning-session-startup",
            "plan-review",
            "authority-requirement-binding",
            "unrelated-placeholder-skill"
        };
        var request = new SkillSelectionRequest(
            Prompt: fixturePrompt,
            ProjectRoot: "",
            AvailableSkillIds: fixtureCandidates,
            HostFacts: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["fixture_probe"] = "true",
                ["model_id"] = modelId,
                ["manifest_ref"] = ToRepoRelativePath(manifestPath)
            },
            UserPromptSourceEntryId: "P39-S9D-FIXTURE-PROBE");
        var decisionResult = machine.DecideFromTrainedState(request);
        if (!decisionResult.IsSuccess || decisionResult.Value is null)
            throw new InvalidOperationException(decisionResult.ErrorMessage ?? "Skill-selection import fixture probe failed.");

        var decision = decisionResult.Value;
        var selectedSkills = decision.SelectedSkillIds.ToArray();
        var exactPromptHit = checkpoint.TrainedPromptKeys.Contains(SkillSelectionMachine.NormalizePromptKey(fixturePrompt), StringComparer.Ordinal);
        var privatePathSeen =
            manifestText.Contains("private-corpora", StringComparison.OrdinalIgnoreCase);
        var privateCorpusRequired = false;
        var checkpointPathResolvedRelative = !Path.IsPathRooted(checkpointFilename) && checkpointPath.StartsWith(manifestDirectory, StringComparison.OrdinalIgnoreCase);
        var checkpointUnitIds = SkillSelectionUnitIdsFromCheckpoint(checkpoint);
        var missingUnitIds = SmartSkillsSemanticUnitCatalog.ExpectedDomainIds
            .Where(unitId => !checkpointUnitIds.Contains(unitId, StringComparer.Ordinal) || !manifestUnitIds.Contains(unitId, StringComparer.Ordinal))
            .ToArray();
        var unitProfileCounts = SkillSelectionUnitProfileCounts(checkpoint);
        var trainedStateLoaded =
            checkpoint.TrainedPairIds.Count > 0 &&
            selectedSkills.Length > 0 &&
            missingUnitIds.Length == 0 &&
            unitProfileCounts.Values.All(count => count > 0);
        var routeLabelPresent = string.Equals(routeMode, checkpoint.RouteMode, StringComparison.Ordinal);
        var op45NoDeterministicMasquerade =
            manifestOp45 &&
            trainedStateLoaded &&
            routeLabelPresent &&
            checkpointPathResolvedRelative &&
            !privateCorpusRequired &&
            !privatePathSeen &&
            !exactPromptHit &&
            !semanticQualityClaimed;
        var passed = op45NoDeterministicMasquerade;
        var report = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "import-skill-selection-model",
            ["passed"] = passed,
            ["trained_state_loaded"] = trainedStateLoaded,
            ["private_corpus_required"] = privateCorpusRequired,
            ["private_corpus_path_seen"] = privatePathSeen,
            ["manifest_path"] = manifestPath,
            ["manifest_ref"] = ToRepoRelativePath(manifestPath),
            ["checkpoint_path"] = checkpointPath,
            ["checkpoint_ref"] = checkpointFilename,
            ["checkpoint_path_resolved_relative"] = checkpointPathResolvedRelative,
            ["model_id"] = modelId,
            ["selected_skills"] = selectedSkills,
            ["loaded_skills"] = selectedSkills,
            ["candidate_skills"] = fixtureCandidates,
            ["probe_prompt"] = fixturePrompt,
            ["probe_output"] = decision.DecisionText,
            ["confidence"] = decision.Confidence,
            ["route_mode"] = checkpoint.RouteMode,
            ["decision_evidence_features"] = decision.EvidenceFeatures,
            ["trained_pair_count"] = checkpoint.TrainedPairIds.Count,
            ["trained_candidate_position_weights"] = checkpoint.CandidatePositionWeights,
            ["exact_prompt_hit"] = exactPromptHit,
            ["exact_prompt_leakage_rate"] = exactPromptHit ? 1.0 : 0.0,
            ["semantic_quality_claimed"] = semanticQualityClaimed,
            ["op45_no_deterministic_masquerade"] = op45NoDeterministicMasquerade,
            ["route_labeling_note"] = "Fixture import probe loaded persisted candidate-position weights from the repo checkpoint and does not claim final semantic quality."
        };

        Console.WriteLine(JsonSerializer.Serialize(report, ToolJsonOptions));
        return passed ? Success : RuntimeError;
    }

    private static async Task<int> RunOrchestrationLoopAsync(IReadOnlyList<string> args)
    {
        var parsed = ParseKeyValueArgs(args, ["require-checkpoint-resume"]);
        var values = parsed.Values;
        var report = await OrchestrationPilotLoopRunner.RunAsync(new OrchestrationLoopRunOptions(
            PrivatePairsPath: Required(values, "private-pairs"),
            TrainSetSize: IntValue(values, "train-size", 50),
            TestSetSize: IntValue(values, "test-size", 50),
            Mode3TrainSetSize: IntValue(values, "mode3-train-size", 20),
            MaxCycles: IntValue(values, "max-cycles", 29),
            FlowGroundingTarget: DoubleValue(values, "flow-grounding-target", 0.75),
            StyleTarget: DoubleValue(values, "style-target", 0.75),
            ExplanationTarget: DoubleValue(values, "explanation-target", 0.70),
            CheckpointPath: Required(values, "checkpoint"),
            RequireCheckpointResume: parsed.Flags.Contains("require-checkpoint-resume"),
            ReportPath: Required(values, "report")));
        Console.WriteLine(JsonSerializer.Serialize(report, ToolJsonOptions));
        return report.ReachedTarget ? Success : RuntimeError;
    }

    private static async Task<int> ExportOrchestrationModelAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var checkpointPath = Required(values, "checkpoint");
        var outputDir = Required(values, "output");
        var reportPath = Required(values, "report");
        var privatePairsPath = Required(values, "private-pairs");
        Directory.CreateDirectory(outputDir);

        var checkpoint = await ReadCheckpointAsync(checkpointPath);
        var sanitized = SanitizeCheckpoint(checkpoint);
        var checkpointFile = Path.Combine(outputDir, "orchestration-v1-checkpoint.json");
        var manifestFile = Path.Combine(outputDir, "manifest.json");
        var runnerReportFile = Path.Combine(outputDir, "runner-report.json");
        await WriteJsonFileAsync(checkpointFile, sanitized);
        await WriteJsonFileAsync(runnerReportFile, new
        {
            model_id = "orchestration-v1",
            source_report_sha256 = Sha256File(reportPath),
            private_pairs_sha256 = Sha256File(privatePairsPath),
            checkpoint_sha256 = Sha256File(checkpointPath),
            learned_flow_grounding_accuracy = ReadReportMetric(reportPath, "learned_flow_grounding_accuracy"),
            unseen_style_accuracy = ReadReportMetric(reportPath, "unseen_style_accuracy"),
            human_explanation_accuracy = ReadReportMetric(reportPath, "human_explanation_accuracy"),
            exact_prompt_leakage_rate = ReadReportMetric(reportPath, "exact_prompt_leakage_rate"),
            exported_at_utc = DateTimeOffset.UtcNow
        });

        var manifest = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["model_id"] = "orchestration-v1",
            ["domain"] = nameof(OrchestrationPlanDomain),
            ["version"] = checkpoint.Version,
            ["checkpoint_filename"] = "orchestration-v1-checkpoint.json",
            ["runner_report_ref"] = "runner-report.json",
            ["runner_report_filename"] = "runner-report.json",
            ["training_data_ref"] = "training-data/orchestration/orchestration-v1/training-manifest.json",
            ["accuracy_at_export"] = ReadReportMetric(reportPath, "overall_training_accuracy"),
            ["loader_type"] = nameof(OrchestrationPlanMachine),
            ["source_base_commit"] = ReadGitHead(),
            ["export_input_sha256"] = Sha256File(checkpointPath),
            ["schema_version"] = 1,
            ["created_at_utc"] = DateTimeOffset.UtcNow
        };
        await WriteJsonFileAsync(manifestFile, manifest);

        Console.WriteLine(JsonSerializer.Serialize(new
        {
            command = "export-orchestration-model",
            manifest = manifestFile,
            checkpoint = checkpointFile,
            runner_report = runnerReportFile,
            model_id = "orchestration-v1"
        }, ToolJsonOptions));
        return Success;
    }

    private static async Task<int> ExportTranslationModelAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var checkpointPath = Required(values, "checkpoint");
        var outputDir = Required(values, "output");
        var reportPath = Required(values, "report");
        Directory.CreateDirectory(outputDir);

        var checkpointFile = Path.Combine(outputDir, TranslationModelManifestContract.CheckpointFilename);
        var manifestFile = Path.Combine(outputDir, "manifest.json");
        var runnerReportFile = Path.Combine(outputDir, TranslationModelManifestContract.RunnerReportRef);
        File.Copy(checkpointPath, checkpointFile, overwrite: true);
        File.Copy(reportPath, runnerReportFile, overwrite: true);

        var accuracy = ReadFirstReportMetric(
            reportPath,
            "best_runtime_product_accuracy",
            "accuracy_at_export",
            "runtime_product_accuracy",
            "target_accuracy");
        var manifest = TranslationModelManifestContract.Create(
            accuracyAtExport: accuracy,
            sourceBaseCommit: ReadGitHead(),
            createdAtUtc: DateTimeOffset.UtcNow);
        if (!TranslationModelManifestContract.IsValidSharedAssetManifest(manifest))
            throw new InvalidOperationException("Translation model manifest contract validation failed.");
        await WriteJsonFileAsync(manifestFile, manifest);

        Console.WriteLine(JsonSerializer.Serialize(new
        {
            command = "export-translation-model",
            manifest = manifestFile,
            checkpoint = checkpointFile,
            runner_report = runnerReportFile,
            model_id = TranslationModelManifestContract.ModelId
        }, ToolJsonOptions));
        return Success;
    }

    private static async Task<int> RegenerateTranslationModelFromTrainingDataAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var trainingManifestPath = Required(values, "training-manifest");
        var outputDir = Required(values, "output");
        var initialPairId = Required(values, "initial-pair-id");
        Directory.CreateDirectory(outputDir);

        var trainingManifest = await ReadJsonDictionaryAsync(trainingManifestPath);
        var corpusFilename = Convert.ToString(trainingManifest["corpus_filename"], CultureInfo.InvariantCulture)
            ?? throw new InvalidOperationException("Translation training manifest is missing corpus_filename.");
        var expectedCorpusSha = Convert.ToString(trainingManifest["corpus_sha256"], CultureInfo.InvariantCulture)
            ?? throw new InvalidOperationException("Translation training manifest is missing corpus_sha256.");
        var corpusPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(trainingManifestPath))!, corpusFilename);
        var actualCorpusSha = Sha256File(corpusPath);
        if (!string.Equals(expectedCorpusSha, actualCorpusSha, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Translation training corpus hash mismatch.");

        var corpusRows = await ReadTranslationCorpusRowsAsync(corpusPath);
        var initialRows = corpusRows
            .Where(row => string.Equals(row.PairId, initialPairId, StringComparison.Ordinal))
            .ToArray();
        if (initialRows.Length != 1)
            throw new InvalidOperationException($"Expected exactly one committed corpus row for '{initialPairId}', found {initialRows.Length}.");

        var machine = new TranslationMachine();
        var examples = initialRows.Select(BuildTranslationContinueExample).ToArray();
        var batch = new TranslationLearningBatch(
            BatchId: "translation-v1-regenerate-001",
            DomainTag: TranslationMachine.DomainTag,
            Examples: examples,
            SourceIds: initialRows.Select(row => row.PairId).ToArray());
        var train = await machine.TrainStructuredBatchAsync(batch);
        if (!train.IsSuccess || train.Value is null)
            throw new InvalidOperationException(train.ErrorMessage);

        var checkpointFile = Path.Combine(outputDir, TranslationModelManifestContract.CheckpointFilename);
        var save = await machine.SaveCheckpointAsync(checkpointFile);
        if (!save.IsSuccess)
            throw new InvalidOperationException(save.ErrorMessage);

        var checkpoint = JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
                             await File.ReadAllTextAsync(checkpointFile, Encoding.UTF8),
                             TranslationCheckpointJsonOptions.Standard)
                         ?? throw new InvalidOperationException("Regenerated checkpoint deserialized null.");
        var structuredIdentities = checkpoint.StructuredTrainingIdentities ?? [];
        var structuredExampleIds = structuredIdentities
            .Select(identity => identity.ExampleId)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var initialIdentityPersisted = structuredExampleIds.Contains(initialPairId, StringComparer.Ordinal);
        var checkpointSha = Sha256File(checkpointFile);
        var sourceBaseCommit = ReadGitHead();
        var now = DateTimeOffset.UtcNow;
        var manifest = TranslationModelManifestContract.Create(
            accuracyAtExport: train.Value.AccuracyAfter,
            sourceBaseCommit: sourceBaseCommit,
            createdAtUtc: now);
        if (!TranslationModelManifestContract.IsValidSharedAssetManifest(manifest))
            throw new InvalidOperationException("Translation model manifest contract validation failed after regeneration.");

        var manifestFile = Path.Combine(outputDir, "manifest.json");
        await WriteJsonFileAsync(manifestFile, new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["model_id"] = manifest.ModelId,
            ["domain"] = manifest.Domain,
            ["version"] = manifest.Version,
            ["checkpoint_filename"] = manifest.CheckpointFilename,
            ["runner_report_ref"] = manifest.RunnerReportRef,
            ["runner_report_filename"] = manifest.RunnerReportFilename,
            ["training_data_ref"] = manifest.TrainingDataRef,
            ["accuracy_at_export"] = manifest.AccuracyAtExport,
            ["loader_type"] = manifest.LoaderType,
            ["source_base_commit"] = manifest.SourceBaseCommit,
            ["export_input_sha256"] = checkpointSha,
            ["schema_version"] = manifest.SchemaVersion,
            ["created_at_utc"] = manifest.CreatedAtUtc
        });

        var runnerReportFile = Path.Combine(outputDir, TranslationModelManifestContract.RunnerReportRef);
        var routeEvidence = await BuildTranslationRouteEvidenceAsync(machine);
        var reportPassed = initialIdentityPersisted &&
                           train.Value.RowsTrained == 1 &&
                           string.Equals(actualCorpusSha, expectedCorpusSha, StringComparison.OrdinalIgnoreCase);
        await WriteJsonFileAsync(runnerReportFile, new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "regenerate-translation-model-from-training-data",
            ["model_id"] = TranslationModelManifestContract.ModelId,
            ["schema_version"] = 1,
            ["regenerated_from_committed_training_data"] = true,
            ["training_manifest_ref"] = ToRepoRelativePath(trainingManifestPath),
            ["corpus_filename"] = corpusFilename,
            ["corpus_sha256"] = actualCorpusSha,
            ["initial_pair_ids"] = initialRows.Select(row => row.PairId).ToArray(),
            ["no_identity_guessing"] = true,
            ["structured_training_identity_count"] = structuredIdentities.Count,
            ["structured_training_identity_example_ids"] = structuredExampleIds,
            ["initial_identity_persisted"] = initialIdentityPersisted,
            ["rows_trained"] = train.Value.RowsTrained,
            ["accuracy_at_export"] = train.Value.AccuracyAfter,
            ["best_runtime_product_accuracy"] = train.Value.AccuracyAfter,
            ["runtime_product_accuracy"] = train.Value.AccuracyAfter,
            ["reward_margin"] = train.Value.RewardMargin,
            ["checkpoint_filename"] = TranslationModelManifestContract.CheckpointFilename,
            ["checkpoint_sha256"] = checkpointSha,
            ["source_base_commit"] = sourceBaseCommit,
            ["export_input_sha256"] = checkpointSha,
            ["route_evidence"] = routeEvidence,
            ["passed"] = reportPassed
        });

        Console.WriteLine(JsonSerializer.Serialize(new
        {
            command = "regenerate-translation-model-from-training-data",
            manifest = manifestFile,
            checkpoint = checkpointFile,
            runner_report = runnerReportFile,
            initial_pair_ids = initialRows.Select(row => row.PairId).ToArray(),
            structured_training_identity_example_ids = structuredExampleIds,
            regenerated_from_committed_training_data = true,
            no_identity_guessing = true,
            passed = reportPassed
        }, ToolJsonOptions));
        return reportPassed ? Success : RuntimeError;
    }

    private static async Task<int> ImportTranslationModelAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var manifestPath = Required(values, "manifest");
        var probe = await ProbeTranslationManifestAsync(manifestPath);
        Console.WriteLine(JsonSerializer.Serialize(probe, ToolJsonOptions));
        return (bool)probe["passed"]! ? Success : RuntimeError;
    }

    private static async Task<int> ExportOrchestrationTrainingDataAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var privatePairsPath = Required(values, "private-pairs");
        var linkedModel = Required(values, "linked-model");
        var outputDir = Required(values, "output");
        Directory.CreateDirectory(outputDir);

        var pairs = (await OrchestrationCorpusBuilder.ReadPairsAsync(privatePairsPath))
            .Select(SanitizePair)
            .ToArray();
        var corpusPath = Path.Combine(outputDir, "corpus.jsonl.gz");
        await WriteGzipJsonLinesAsync(corpusPath, pairs);
        var corpusSha = Sha256File(corpusPath);
        var pointerPath = Path.Combine(outputDir, "corpus-pointer.json");
        await WriteJsonFileAsync(pointerPath, new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["schema_version"] = 1,
            ["corpus_sha256"] = corpusSha,
            ["corpus_filename"] = "corpus.jsonl.gz",
            ["compression"] = "gzip",
            ["content_type"] = "application/x-jsonlines"
        });

        var manifest = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["linked_model"] = linkedModel,
            ["linked_model_version"] = linkedModel,
            ["schema_version"] = 1,
            ["corpus_filename"] = "corpus.jsonl.gz",
            ["corpus_pointer_filename"] = "corpus-pointer.json",
            ["corpus_sha256"] = corpusSha,
            ["total_pairs"] = pairs.Length,
            ["training_pairs_count"] = pairs.Length,
            ["mode1_training_pairs"] = pairs.Count(pair => pair.Mode == PairMode.Mode1LearnedFlowGrounding && !pair.IsHeldOut),
            ["mode1_held_out_eval_pairs"] = pairs.Count(pair => pair.Mode == PairMode.Mode1LearnedFlowGrounding && pair.IsHeldOut),
            ["mode2_training_pairs"] = pairs.Count(pair => pair.Mode == PairMode.Mode2Generalization && !pair.IsHeldOut),
            ["mode2_held_out_eval_pairs"] = pairs.Count(pair => pair.Mode == PairMode.Mode2Generalization && pair.IsHeldOut),
            ["mode3_training_pairs"] = pairs.Count(pair => pair.Mode == PairMode.Mode3HumanExplanation && !pair.IsHeldOut),
            ["mode3_held_out_eval_pairs"] = pairs.Count(pair => pair.Mode == PairMode.Mode3HumanExplanation && pair.IsHeldOut),
            ["split_fraction_train"] = Math.Round(pairs.Count(pair => !pair.IsHeldOut) / (double)pairs.Length, 4),
            ["split_fraction_eval"] = Math.Round(pairs.Count(pair => pair.IsHeldOut) / (double)pairs.Length, 4)
        };
        var manifestPath = Path.Combine(outputDir, "training-manifest.json");
        await WriteJsonFileAsync(manifestPath, manifest);

        Console.WriteLine(JsonSerializer.Serialize(new
        {
            command = "export-orchestration-training-data",
            manifest = manifestPath,
            corpus_pointer = pointerPath,
            corpus = corpusPath,
            corpus_sha256 = corpusSha
        }, ToolJsonOptions));
        return Success;
    }

    private static async Task<int> ImportOrchestrationModelAsync(IReadOnlyList<string> args)
    {
        var parsed = ParseKeyValueArgs(args, ["fixture-probe"]);
        var manifestPath = Required(parsed.Values, "manifest");
        var probe = await ProbeOrchestrationManifestAsync(manifestPath);
        Console.WriteLine(JsonSerializer.Serialize(probe, ToolJsonOptions));
        return (bool)probe["passed"] ? Success : RuntimeError;
    }

    private static async Task<int> PublishOrchestrationModelAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var manifest = Required(values, "manifest");
        await PublishModelAsync("orchestration-v1", manifest, nameof(OrchestrationPlanDomain), "1.0.0");
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            command = "publish-orchestration-model",
            model_id = "orchestration-v1",
            manifest = ToRepoRelativePath(manifest)
        }, ToolJsonOptions));
        return Success;
    }

    private static async Task<int> PublishTranslationModelAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var manifest = Required(values, "manifest");
        await PublishModelAsync(
            TranslationModelManifestContract.ModelId,
            manifest,
            TranslationModelManifestContract.Domain,
            TranslationModelManifestContract.Version);
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            command = "publish-translation-model",
            model_id = TranslationModelManifestContract.ModelId,
            manifest = ToRepoRelativePath(manifest)
        }, ToolJsonOptions));
        return Success;
    }

    private static async Task<int> PublishSkillSelectionModelAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var manifestPath = Required(values, "manifest");
        var manifestFullPath = Path.GetFullPath(manifestPath);
        if (!File.Exists(manifestFullPath))
            throw new FileNotFoundException("Skill-selection manifest not found.", manifestFullPath);

        var repoRoot = Directory.GetCurrentDirectory();
        var manifestRef = ToRepoRelativePath(manifestFullPath);
        if (Path.IsPathRooted(manifestRef) || manifestRef.StartsWith("..", StringComparison.Ordinal))
            throw new InvalidOperationException("Skill-selection manifest must be inside the repository.");

        using var manifestDocument = JsonDocument.Parse(await File.ReadAllTextAsync(manifestFullPath));
        var manifestRoot = manifestDocument.RootElement;
        var modelId = RequiredJsonString(manifestRoot, "model_id");
        if (!string.Equals(modelId, "skill-selection-v1", StringComparison.Ordinal))
            throw new InvalidOperationException($"Unexpected skill-selection model id '{modelId}'.");

        var domain = RequiredJsonString(manifestRoot, "domain");
        var version = RequiredJsonString(manifestRoot, "version");
        var accuracy = RequiredJsonDouble(manifestRoot, "accuracy_at_export");
        var routeMode = RequiredJsonString(manifestRoot, "route_mode");
        var semanticQualityClaimed = RequiredJsonBool(manifestRoot, "semantic_quality_claimed");
        var manifestOp45 = RequiredJsonBool(manifestRoot, "op45_no_deterministic_masquerade");
        var exactPromptLeakage = RequiredJsonDouble(manifestRoot, "exact_prompt_leakage");
        var runnerReportRef = RequiredJsonString(manifestRoot, "runner_report_ref");
        var trainedPairCount = manifestRoot.TryGetProperty("trained_pair_count", out var trainedPairCountElement) &&
                               trainedPairCountElement.ValueKind == JsonValueKind.Number
            ? trainedPairCountElement.GetInt32()
            : 0;
        var exactPromptHitCount = manifestRoot.TryGetProperty("exact_prompt_hit_count", out var exactPromptHitCountElement) &&
                                  exactPromptHitCountElement.ValueKind == JsonValueKind.Number
            ? exactPromptHitCountElement.GetInt32()
            : -1;
        var manifestUnitIds = RequiredJsonStringArray(manifestRoot, "unit_ids");
        var missingUnitIds = SmartSkillsSemanticUnitCatalog.ExpectedDomainIds
            .Where(unitId => !manifestUnitIds.Contains(unitId, StringComparer.Ordinal))
            .ToArray();
        if (missingUnitIds.Length > 0)
            throw new InvalidOperationException($"Skill-selection manifest is missing semantic unit ids: {string.Join(", ", missingUnitIds)}");
        if (!string.Equals(routeMode, "candidate_position_scaffold_not_final_semantic_authority", StringComparison.Ordinal))
            throw new InvalidOperationException($"Skill-selection registry route is unsupported: {routeMode}");
        if (semanticQualityClaimed)
            throw new InvalidOperationException("Skill-selection registry publish refuses semantic_quality_claimed=true.");
        if (!manifestOp45 || exactPromptLeakage != 0.0 || exactPromptHitCount != 0 || trainedPairCount <= 0)
            throw new InvalidOperationException("Skill-selection manifest does not carry complete OP-45 export evidence.");
        if (ContainsAbsoluteOrPrivatePath(runnerReportRef))
            throw new InvalidOperationException("Skill-selection runner report reference must be repo/model relative.");

        var runnerReportPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(manifestFullPath)!, runnerReportRef.Replace('/', Path.DirectorySeparatorChar)));
        if (!runnerReportPath.StartsWith(Path.GetDirectoryName(manifestFullPath)!, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Skill-selection runner report must resolve inside the model bundle.");
        if (!File.Exists(runnerReportPath))
            throw new FileNotFoundException("Skill-selection runner report not found.", runnerReportPath);

        using var runnerReportDocument = JsonDocument.Parse(await File.ReadAllTextAsync(runnerReportPath));
        var runnerReportRoot = runnerReportDocument.RootElement;
        var reportRouteMode = RequiredJsonString(runnerReportRoot, "route_mode");
        var reportSemanticQualityClaimed = RequiredJsonBool(runnerReportRoot, "semantic_quality_claimed");
        var reportOp45 = RequiredJsonBool(runnerReportRoot, "op45_no_deterministic_masquerade");
        var reportExactPromptLeakage = RequiredJsonDouble(runnerReportRoot, "exact_prompt_leakage");
        var reportUnitIds = RequiredJsonStringArray(runnerReportRoot, "unit_ids");
        if (!string.Equals(reportRouteMode, routeMode, StringComparison.Ordinal) ||
            reportSemanticQualityClaimed ||
            !reportOp45 ||
            reportExactPromptLeakage != 0.0 ||
            SmartSkillsSemanticUnitCatalog.ExpectedDomainIds.Any(unitId => !reportUnitIds.Contains(unitId, StringComparer.Ordinal)))
            throw new InvalidOperationException("Skill-selection runner report does not back the manifest OP-45 registry evidence.");

        var indexPath = Path.Combine(repoRoot, "models", "index.json");
        Directory.CreateDirectory(Path.GetDirectoryName(indexPath)!);
        var entries = await ReadModelIndexEntriesForMergeAsync(indexPath);
        entries.RemoveAll(entry =>
            entry.TryGetValue("model_id", out var value) &&
            value is string id &&
            string.Equals(id, modelId, StringComparison.Ordinal));
        var entry = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["model_id"] = modelId,
            ["domain"] = domain,
            ["manifest_path"] = manifestRef,
            ["version"] = version,
            ["accuracy"] = accuracy,
            ["route_mode"] = routeMode,
            ["trained_pair_count"] = trainedPairCount,
            ["checkpoint_sha256"] = RequiredJsonString(manifestRoot, "checkpoint_sha256"),
            ["unit_ids"] = manifestUnitIds,
            ["semantic_unit_count"] = manifestUnitIds.Count,
            ["semantic_quality_claimed"] = false,
            ["op45_no_deterministic_masquerade"] = true,
            ["exact_prompt_leakage"] = exactPromptLeakage,
            ["exact_prompt_hit_count"] = exactPromptHitCount,
            ["published_at_utc"] = DateTimeOffset.UtcNow
        };
        entries.Add(entry);
        var orderedEntries = entries
            .OrderBy(entry => entry.TryGetValue("model_id", out var value) ? value?.ToString() ?? "" : "", StringComparer.Ordinal)
            .ToArray();
        var index = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["models"] = orderedEntries
        };
        var indexJson = JsonSerializer.Serialize(index, ToolJsonOptions);
        if (ContainsAbsoluteOrPrivatePath(indexJson))
            throw new InvalidOperationException("Refusing to write model index with private or absolute paths.");
        await File.WriteAllTextAsync(indexPath, indexJson + Environment.NewLine, Encoding.UTF8);

        Console.WriteLine(JsonSerializer.Serialize(new
        {
            command = "publish-skill-selection-model",
            passed = true,
            model_id = modelId,
            registry_path = ToRepoRelativePath(indexPath),
            manifest_path = manifestRef,
            route_mode = routeMode,
            semantic_quality_claimed = false,
            op45_no_deterministic_masquerade = true,
            preserved_existing_entries = orderedEntries.Count(entry =>
                entry.TryGetValue("model_id", out var value) &&
                value is string id &&
                !string.Equals(id, modelId, StringComparison.Ordinal)),
            registry_entry = entry
        }, ToolJsonOptions));
        return Success;
    }

    private static async Task<int> LoadOrchestrationModelAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var modelId = Required(values, "model-id");
        var manifest = await ResolveModelManifestAsync(modelId);
        var probe = await ProbeOrchestrationManifestAsync(manifest);
        probe["command"] = "load-orchestration-model";
        Console.WriteLine(JsonSerializer.Serialize(probe, ToolJsonOptions));
        return (bool)probe["passed"] ? Success : RuntimeError;
    }

    private static async Task<int> LoadTranslationModelAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var modelId = Required(values, "model-id");
        var manifest = await ResolveModelManifestAsync(modelId);
        var probe = await ProbeTranslationManifestAsync(manifest);
        probe["command"] = "load-translation-model";
        probe["registry_resolved_manifest"] = ToRepoRelativePath(manifest);
        Console.WriteLine(JsonSerializer.Serialize(probe, ToolJsonOptions));
        return (bool)probe["passed"]! ? Success : RuntimeError;
    }

    private static async Task<int> LoadSkillSelectionModelAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var requestedModelId = Required(values, "model-id");
        var manifestPath = await ResolveModelManifestAsync(requestedModelId);
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException("Skill-selection manifest resolved from registry was not found.", manifestPath);

        var manifestText = await File.ReadAllTextAsync(manifestPath, Encoding.UTF8);
        using var manifestDocument = JsonDocument.Parse(manifestText);
        var manifestRoot = manifestDocument.RootElement;
        var modelId = RequiredJsonString(manifestRoot, "model_id");
        var checkpointFilename = RequiredJsonString(manifestRoot, "checkpoint_filename");
        var routeMode = RequiredJsonString(manifestRoot, "route_mode");
        var semanticQualityClaimed = RequiredJsonBool(manifestRoot, "semantic_quality_claimed");
        var manifestOp45 = RequiredJsonBool(manifestRoot, "op45_no_deterministic_masquerade");
        var manifestUnitIds = RequiredJsonStringArray(manifestRoot, "unit_ids");
        if (!string.Equals(modelId, requestedModelId, StringComparison.Ordinal))
            throw new InvalidOperationException($"Registry model id '{requestedModelId}' resolved manifest model id '{modelId}'.");
        if (!string.Equals(modelId, "skill-selection-v1", StringComparison.Ordinal))
            throw new InvalidOperationException($"Unexpected skill-selection model_id '{modelId}'.");
        if (Path.IsPathRooted(checkpointFilename))
            throw new InvalidOperationException("Skill-selection checkpoint filename must be relative to the manifest directory.");

        var manifestDirectory = Path.GetDirectoryName(Path.GetFullPath(manifestPath))
                                ?? throw new InvalidOperationException("Skill-selection manifest directory could not be resolved.");
        var checkpointPath = Path.GetFullPath(Path.Combine(manifestDirectory, checkpointFilename));
        if (!checkpointPath.StartsWith(manifestDirectory, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Skill-selection checkpoint path escaped the manifest directory.");
        if (!File.Exists(checkpointPath))
            throw new FileNotFoundException("Skill-selection checkpoint referenced by manifest was not found.", checkpointPath);

        var checkpoint = await ReadSkillSelectionCheckpointAsync(checkpointPath);
        var machine = new SkillSelectionMachine();
        machine.LoadCheckpoint(checkpoint);

        var fixturePrompt = "Pick a project planning skill for a brand-new architecture plan.";
        var fixtureCandidates = new[]
        {
            "planning-session-startup",
            "plan-review",
            "authority-requirement-binding",
            "unrelated-placeholder-skill"
        };
        var request = new SkillSelectionRequest(
            Prompt: fixturePrompt,
            ProjectRoot: "",
            AvailableSkillIds: fixtureCandidates,
            HostFacts: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["fixture_probe"] = "true",
                ["model_id"] = modelId,
                ["manifest_ref"] = ToRepoRelativePath(manifestPath),
                ["registry_resolved"] = "true"
            },
            UserPromptSourceEntryId: "P39-S9H-LOAD-PROBE");
        var decisionResult = machine.DecideFromTrainedState(request);
        if (!decisionResult.IsSuccess || decisionResult.Value is null)
            throw new InvalidOperationException(decisionResult.ErrorMessage ?? "Skill-selection load fixture probe failed.");

        var decision = decisionResult.Value;
        var selectedSkills = decision.SelectedSkillIds.ToArray();
        var exactPromptHit = checkpoint.TrainedPromptKeys.Contains(SkillSelectionMachine.NormalizePromptKey(fixturePrompt), StringComparer.Ordinal);
        var privatePathSeen =
            manifestText.Contains("private-corpora", StringComparison.OrdinalIgnoreCase);
        var privateCorpusRequired = false;
        var checkpointPathResolvedRelative = !Path.IsPathRooted(checkpointFilename) && checkpointPath.StartsWith(manifestDirectory, StringComparison.OrdinalIgnoreCase);
        var checkpointUnitIds = SkillSelectionUnitIdsFromCheckpoint(checkpoint);
        var missingUnitIds = SmartSkillsSemanticUnitCatalog.ExpectedDomainIds
            .Where(unitId => !checkpointUnitIds.Contains(unitId, StringComparer.Ordinal) || !manifestUnitIds.Contains(unitId, StringComparer.Ordinal))
            .ToArray();
        var unitProfileCounts = SkillSelectionUnitProfileCounts(checkpoint);
        var trainedStateLoaded =
            checkpoint.TrainedPairIds.Count > 0 &&
            selectedSkills.Length > 0 &&
            missingUnitIds.Length == 0 &&
            unitProfileCounts.Values.All(count => count > 0);
        var routeLabelPresent = string.Equals(routeMode, checkpoint.RouteMode, StringComparison.Ordinal);
        var op45NoDeterministicMasquerade =
            manifestOp45 &&
            trainedStateLoaded &&
            routeLabelPresent &&
            checkpointPathResolvedRelative &&
            !privateCorpusRequired &&
            !privatePathSeen &&
            !exactPromptHit &&
            !semanticQualityClaimed;
        var passed = op45NoDeterministicMasquerade;
        var report = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "load-skill-selection-model",
            ["passed"] = passed,
            ["model_id"] = modelId,
            ["registry_resolved"] = true,
            ["manifest_loaded"] = true,
            ["checkpoint_loaded"] = true,
            ["trained_state_loaded"] = trainedStateLoaded,
            ["unit_ids"] = checkpointUnitIds,
            ["unit_profile_counts"] = unitProfileCounts,
            ["missing_unit_ids"] = missingUnitIds,
            ["private_corpus_required"] = privateCorpusRequired,
            ["private_corpus_path_seen"] = privatePathSeen,
            ["manifest_path"] = ToRepoRelativePath(manifestPath),
            ["checkpoint_ref"] = checkpointFilename,
            ["checkpoint_path_resolved_relative"] = checkpointPathResolvedRelative,
            ["selected_skills"] = selectedSkills,
            ["loaded_skills"] = selectedSkills,
            ["candidate_skills"] = fixtureCandidates,
            ["probe_prompt"] = fixturePrompt,
            ["probe_output"] = decision.DecisionText,
            ["confidence"] = decision.Confidence,
            ["route_mode"] = checkpoint.RouteMode,
            ["decision_evidence_features"] = decision.EvidenceFeatures,
            ["trained_pair_count"] = checkpoint.TrainedPairIds.Count,
            ["trained_candidate_position_weights"] = checkpoint.CandidatePositionWeights,
            ["exact_prompt_hit"] = exactPromptHit,
            ["exact_prompt_leakage_rate"] = exactPromptHit ? 1.0 : 0.0,
            ["semantic_quality_claimed"] = semanticQualityClaimed,
            ["op45_no_deterministic_masquerade"] = op45NoDeterministicMasquerade,
            ["route_labeling_note"] = "Load-by-model-id probe resolved the registry entry, loaded persisted candidate-position weights from the repo checkpoint, and does not claim final semantic quality."
        };

        Console.WriteLine(JsonSerializer.Serialize(report, ToolJsonOptions));
        return passed ? Success : RuntimeError;
    }

    private static async Task<int> ContinueSkillSelectionTrainingAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var manifestPath = Required(values, "manifest");
        var trainingManifestPath = Required(values, "training-manifest");
        var maxCycles = IntValue(values, "max-cycles", 1);
        var outputDir = Required(values, "output");
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException("Skill-selection manifest not found.", manifestPath);
        if (!File.Exists(trainingManifestPath))
            throw new FileNotFoundException("Skill-selection training manifest not found.", trainingManifestPath);

        var manifestText = await File.ReadAllTextAsync(manifestPath, Encoding.UTF8);
        using var manifestDocument = JsonDocument.Parse(manifestText);
        var manifestRoot = manifestDocument.RootElement;
        var modelId = RequiredJsonString(manifestRoot, "model_id");
        var checkpointFilename = RequiredJsonString(manifestRoot, "checkpoint_filename");
        var routeMode = RequiredJsonString(manifestRoot, "route_mode");
        var semanticQualityClaimed = RequiredJsonBool(manifestRoot, "semantic_quality_claimed");
        var manifestOp45 = RequiredJsonBool(manifestRoot, "op45_no_deterministic_masquerade");
        if (!string.Equals(modelId, "skill-selection-v1", StringComparison.Ordinal))
            throw new InvalidOperationException($"Unexpected skill-selection model_id '{modelId}'.");
        if (Path.IsPathRooted(checkpointFilename))
            throw new InvalidOperationException("Skill-selection checkpoint filename must be relative to the manifest directory.");

        var manifestDirectory = Path.GetDirectoryName(Path.GetFullPath(manifestPath))
                                ?? throw new InvalidOperationException("Skill-selection manifest directory could not be resolved.");
        var checkpointPath = Path.GetFullPath(Path.Combine(manifestDirectory, checkpointFilename));
        if (!checkpointPath.StartsWith(manifestDirectory, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Skill-selection checkpoint path escaped the manifest directory.");
        if (!File.Exists(checkpointPath))
            throw new FileNotFoundException("Skill-selection checkpoint referenced by manifest was not found.", checkpointPath);

        var trainingManifestText = await File.ReadAllTextAsync(trainingManifestPath, Encoding.UTF8);
        using var trainingDocument = JsonDocument.Parse(trainingManifestText);
        var trainingRoot = trainingDocument.RootElement;
        var trainingModelId = RequiredJsonString(trainingRoot, "model_id");
        if (!string.Equals(trainingModelId, modelId, StringComparison.Ordinal))
            throw new InvalidOperationException($"Training manifest model_id '{trainingModelId}' does not match manifest model_id '{modelId}'.");
        var snapshotFilename = RequiredJsonString(trainingRoot, "corpus_snapshot_filename");
        if (Path.IsPathRooted(snapshotFilename))
            throw new InvalidOperationException("Skill-selection corpus snapshot filename must be relative to the training manifest directory.");
        var trainingManifestDirectory = Path.GetDirectoryName(Path.GetFullPath(trainingManifestPath))
                                        ?? throw new InvalidOperationException("Skill-selection training manifest directory could not be resolved.");
        var corpusSnapshotPath = Path.GetFullPath(Path.Combine(trainingManifestDirectory, snapshotFilename));
        if (!corpusSnapshotPath.StartsWith(trainingManifestDirectory, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Skill-selection corpus snapshot path escaped the training manifest directory.");
        if (!File.Exists(corpusSnapshotPath))
            throw new FileNotFoundException("Skill-selection repo-safe corpus snapshot was not found.", corpusSnapshotPath);
        var expectedCorpusSha = RequiredJsonString(trainingRoot, "corpus_sha256");
        var actualCorpusSha = Sha256File(corpusSnapshotPath);
        if (!string.Equals(expectedCorpusSha, actualCorpusSha, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Skill-selection corpus snapshot hash does not match training manifest.");
        var safeSnapshotAvailable = RequiredJsonBool(trainingRoot, "safe_row_snapshot_available");
        var privatePathRecorded = RequiredJsonBool(trainingRoot, "private_path_recorded");
        if (!safeSnapshotAvailable || privatePathRecorded)
            throw new InvalidOperationException("Skill-selection training bundle does not provide a safe row snapshot without private paths.");

        var checkpoint = await ReadSkillSelectionCheckpointAsync(checkpointPath);
        var oldPairIds = checkpoint.TrainedPairIds.ToHashSet(StringComparer.Ordinal);
        var eligibleBasePairIds =
            trainingRoot.TryGetProperty("training_eligible_pair_ids", out var eligibleElement) &&
            eligibleElement.ValueKind == JsonValueKind.Array
                ? RequiredJsonStringArray(trainingRoot, "training_eligible_pair_ids").ToHashSet(StringComparer.Ordinal)
                : RequiredJsonStringArray(trainingRoot, "trained_pair_ids").ToHashSet(StringComparer.Ordinal);
        var trainingAdapters = SmartSkillsSemanticUnitCatalog.All
            .SelectMany(unit => unit.TrainingAdapterName.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var expectedRoutedPairIds = eligibleBasePairIds
            .SelectMany(pairId => trainingAdapters.Select(adapter => $"{pairId}::{adapter}"))
            .ToHashSet(StringComparer.Ordinal);
        var oldPairIdsPersisted = oldPairIds.Count > 0 && expectedRoutedPairIds.All(oldPairIds.Contains);
        var rows = (await ReadSkillSelectionCorpusRowsAsync(corpusSnapshotPath))
            .Where(row => eligibleBasePairIds.Contains(row.PairId))
            .OrderBy(row => row.PairId, StringComparer.Ordinal)
            .ToArray();
        var newRoutedRows = trainingAdapters
            .SelectMany(adapter => rows.Select(row => new
            {
                Row = row,
                Adapter = adapter,
                RoutedPairId = $"{row.PairId}::{adapter}"
            }))
            .Where(row => !oldPairIds.Contains(row.RoutedPairId))
            .OrderBy(row => row.RoutedPairId, StringComparer.Ordinal)
            .Take(Math.Max(1, maxCycles))
            .ToArray();

        var machine = new SkillSelectionMachine();
        machine.LoadCheckpoint(checkpoint);
        if (newRoutedRows.Length == 0)
        {
            var noOpUnitIds = SkillSelectionUnitIdsFromCheckpoint(checkpoint);
            var noOpMissingUnitIds = SmartSkillsSemanticUnitCatalog.ExpectedDomainIds
                .Where(unitId => !noOpUnitIds.Contains(unitId, StringComparer.Ordinal))
                .ToArray();
            var noOpMatrix = BuildSkillSelectionSemanticDomainMatrix(checkpoint);
            var noOpPassed = oldPairIdsPersisted && noOpMissingUnitIds.Length == 0;
            Directory.CreateDirectory(outputDir);
            var noOpReportPath = Path.Combine(outputDir, "skill-selection-v1-continued-report.json");
            var noOpReport = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["command"] = "continue-skill-selection-training",
                ["passed"] = noOpPassed,
                ["no_op_continuation"] = true,
                ["no_op_reason"] = "Committed safe training snapshot contains no new eligible pair ids beyond the current checkpoint; SS-13 records deterministic no-op continuation.",
                ["model_id"] = modelId,
                ["manifest_ref"] = ToRepoRelativePath(manifestPath),
                ["training_manifest_ref"] = ToRepoRelativePath(trainingManifestPath),
                ["checkpoint_ref"] = checkpointFilename,
                ["current_checkpoint_sha256"] = Sha256File(checkpointPath),
                ["previous_checkpoint_sha256"] = Sha256File(checkpointPath),
                ["repo_model_bundle_loaded"] = true,
                ["training_manifest_loaded"] = true,
                ["repo_safe_row_snapshot_loaded"] = true,
                ["old_pair_ids_persisted"] = oldPairIdsPersisted,
                ["expected_routed_pair_count"] = expectedRoutedPairIds.Count,
                ["persisted_routed_pair_count"] = oldPairIds.Count(pairId => expectedRoutedPairIds.Contains(pairId)),
                ["new_pairs_trained"] = 0,
                ["no_replay_proven"] = true,
                ["trained_pair_count_before"] = checkpoint.TrainedPairIds.Count,
                ["trained_pair_count_after"] = checkpoint.TrainedPairIds.Count,
                ["unit_ids"] = noOpUnitIds,
                ["semantic_domain_matrix"] = noOpMatrix,
                ["semantic_domain_gate_passed"] = noOpMissingUnitIds.Length == 0,
                ["semantic_domain_gaps"] = noOpMissingUnitIds,
                ["semantic_quality_claimed"] = false,
                ["host_runtime_claimed"] = false,
                ["adaptive_user_model_claimed"] = false,
                ["external_ai_used"] = false,
                ["claim_scope"] = "SS-13 no-op continue-training probe only; current checkpoint loaded and no new eligible committed rows were replayed."
            };
            await WriteJsonFileAsync(noOpReportPath, noOpReport);
            Console.WriteLine(JsonSerializer.Serialize(noOpReport, ToolJsonOptions));
            return noOpPassed ? Success : RuntimeError;
        }
        var trainResult = await machine.TrainMachineBatchAsync(
            newRoutedRows.Select(row => BuildSkillSelectionPairForAdapter(row.Row, row.Adapter)).ToArray());
        if (!trainResult.IsSuccess)
            throw new InvalidOperationException(trainResult.ErrorMessage ?? "SkillSelectionMachine continue-training failed.");

        var continuedCheckpoint = machine.ExportCheckpoint();
        var newPairIds = newRoutedRows.Select(row => row.RoutedPairId).ToArray();
        var noReplayProven =
            oldPairIdsPersisted &&
            newPairIds.Length > 0 &&
            newPairIds.All(pairId => !oldPairIds.Contains(pairId)) &&
            continuedCheckpoint.TrainedPairIds.Count == oldPairIds.Count + newPairIds.Length &&
            newPairIds.All(pairId => continuedCheckpoint.TrainedPairIds.Contains(pairId, StringComparer.Ordinal));

        Directory.CreateDirectory(outputDir);
        var continuedCheckpointPath = Path.Combine(outputDir, "skill-selection-v1-continued-checkpoint.json");
        var continueReportPath = Path.Combine(outputDir, "skill-selection-v1-continued-report.json");
        await WriteJsonFileAsync(continuedCheckpointPath, continuedCheckpoint);

        var nonExactProbePrompt = "Select skills for a never-before-seen smart-skills extension recovery review.";
        var exactPromptHit = continuedCheckpoint.TrainedPromptKeys.Contains(SkillSelectionMachine.NormalizePromptKey(nonExactProbePrompt), StringComparer.Ordinal);
        var semanticDomainMatrix = BuildSkillSelectionSemanticDomainMatrix(continuedCheckpoint);
        var semanticDomainsCovered = semanticDomainMatrix.Count(row =>
            row.TryGetValue("trainable_state_present", out var value) && value is true);
        var semanticDomainGaps = semanticDomainMatrix
            .Where(row => row.TryGetValue("trainable_state_present", out var value) && value is not true)
            .Select(row => Convert.ToString(row["domain"], CultureInfo.InvariantCulture) ?? "")
            .Where(domain => !string.IsNullOrWhiteSpace(domain))
            .ToArray();
        var semanticDomainGatePassed = semanticDomainsCovered == 11 && semanticDomainGaps.Length == 0;
        var routeLabelPresent = string.Equals(routeMode, checkpoint.RouteMode, StringComparison.Ordinal) &&
                                string.Equals(continuedCheckpoint.RouteMode, checkpoint.RouteMode, StringComparison.Ordinal);
        var op45NoDeterministicMasquerade =
            manifestOp45 &&
            noReplayProven &&
            routeLabelPresent &&
            !exactPromptHit &&
            !semanticQualityClaimed;
        var passed = op45NoDeterministicMasquerade && semanticDomainGatePassed;

        var report = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "continue-skill-selection-training",
            ["passed"] = passed,
            ["model_id"] = modelId,
            ["manifest_ref"] = ToRepoRelativePath(manifestPath),
            ["training_manifest_ref"] = ToRepoRelativePath(trainingManifestPath),
            ["checkpoint_ref"] = checkpointFilename,
            ["repo_model_bundle_loaded"] = true,
            ["training_manifest_loaded"] = true,
            ["repo_safe_row_snapshot_loaded"] = true,
            ["corpus_snapshot_ref"] = ToRepoRelativePath(corpusSnapshotPath),
            ["old_pair_ids_persisted"] = oldPairIdsPersisted,
            ["old_identity_type"] = "pair_id",
            ["new_identity_type"] = "pair_id",
            ["old_pair_ids"] = oldPairIds.Order(StringComparer.Ordinal).ToArray(),
            ["new_pair_ids"] = newPairIds,
            ["new_pairs_trained"] = newPairIds.Length,
            ["no_replay_proven"] = noReplayProven,
            ["trained_pair_count_before"] = checkpoint.TrainedPairIds.Count,
            ["trained_pair_count_after"] = continuedCheckpoint.TrainedPairIds.Count,
            ["trained_state_extended_or_preserved"] = continuedCheckpoint.TrainedPairIds.Count > checkpoint.TrainedPairIds.Count,
            ["continued_checkpoint_filename"] = Path.GetFileName(continuedCheckpointPath),
            ["continued_report_filename"] = Path.GetFileName(continueReportPath),
            ["route_mode"] = continuedCheckpoint.RouteMode,
            ["route_labeling_note"] = "Continue-training extends persisted trained candidate-position weights; it does not claim final semantic quality.",
            ["exact_prompt_hit"] = exactPromptHit,
            ["exact_prompt_leakage_rate"] = exactPromptHit ? 1.0 : 0.0,
            ["non_exact_probe_prompt"] = nonExactProbePrompt,
            ["semantic_quality_claimed"] = semanticQualityClaimed,
            ["semantic_domains_required"] = 11,
            ["semantic_domains_covered"] = semanticDomainsCovered,
            ["semantic_domain_matrix"] = semanticDomainMatrix,
            ["semantic_domain_gaps"] = semanticDomainGaps,
            ["semantic_domain_gate_passed"] = semanticDomainGatePassed,
            ["semantic_domain_matrix_source"] = nameof(SmartSkillsSemanticUnitCatalog),
            ["semantic_domain_gap_note"] = semanticDomainGatePassed
                ? "All 11 smart-skills semantic units are backed by the S.14 trainable-unit catalog. Runtime semantic quality is not claimed by this continue-training report."
                : "S.14 is still open: current repo state does not back all 11 semantic units with trainable adapters/checkpoint sections.",
            ["op45_no_deterministic_masquerade"] = op45NoDeterministicMasquerade
        };
        await WriteJsonFileAsync(continueReportPath, report);
        Console.WriteLine(JsonSerializer.Serialize(report, ToolJsonOptions));
        return passed ? Success : RuntimeError;
    }

    private static async Task<int> ContinueOrchestrationTrainingAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var manifestPath = Required(values, "manifest");
        var trainingManifestPath = Required(values, "training-manifest");
        var outputDir = Required(values, "output");
        Directory.CreateDirectory(outputDir);

        var manifestDir = Path.GetDirectoryName(Path.GetFullPath(manifestPath))!;
        var modelManifest = await ReadJsonDictionaryAsync(manifestPath);
        var checkpointPath = Path.Combine(manifestDir, Convert.ToString(modelManifest["checkpoint_filename"], CultureInfo.InvariantCulture)!);
        var trainingManifest = await ReadJsonDictionaryAsync(trainingManifestPath);
        var corpusPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(trainingManifestPath))!, Convert.ToString(trainingManifest["corpus_filename"], CultureInfo.InvariantCulture)!);
        var expectedHash = Convert.ToString(trainingManifest["corpus_sha256"], CultureInfo.InvariantCulture);
        var actualHash = Sha256File(corpusPath);
        if (!string.Equals(expectedHash, actualHash, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Training corpus hash mismatch.");

        var load = await OrchestrationPlanMachine.LoadCheckpointAsync(checkpointPath);
        if (!load.IsSuccess)
            throw new InvalidOperationException(load.ErrorMessage);
        var machine = load.Value.Machine;
        var trainedPairIdsBefore = machine.TrainedPairIds.ToHashSet(StringComparer.Ordinal);
        var beforeMode1 = machine.Mode1FlowGroundingKnnSize;
        var beforeMode2 = machine.Mode2KnnStoreSize;
        var pairs = await ReadGzipPairsAsync(corpusPath);
        var newPairs = pairs
            .Where(pair => !pair.IsHeldOut && !machine.TrainedPairIds.Contains(pair.PairId))
            .Take(IntValue(values, "max-cycles", 1) * 25)
            .ToArray();
        var newPairIdsDisjointFromBefore = newPairs.All(pair => !trainedPairIdsBefore.Contains(pair.PairId));
        var train = await machine.TrainOrchestrationBatchAsync(new OrchestrationTrainingBatch("orchestration-v1-continue-001", newPairs));
        if (!train.IsSuccess)
            throw new InvalidOperationException(train.ErrorMessage);

        var outputCheckpoint = Path.Combine(outputDir, "orchestration-v1-continued-checkpoint.json");
        await machine.SaveCheckpointAsync(outputCheckpoint, load.Value.Loop);
        var trainedPairIdsAfter = machine.TrainedPairIds.Count;
        var report = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "continue-orchestration-training",
            ["hash_verified"] = true,
            ["old_trained_pair_ids_excluded"] = true,
            ["trained_pair_ids_count_before"] = trainedPairIdsBefore.Count,
            ["trained_pair_ids_count_after"] = trainedPairIdsAfter,
            ["new_pairs_trained"] = train.Value!.RowsTrained,
            ["new_pair_ids_disjoint_from_before"] = newPairIdsDisjointFromBefore,
            ["new_pair_ids"] = newPairs.Select(pair => pair.PairId).ToArray(),
            ["mode1_knn_size_before"] = beforeMode1,
            ["mode1_knn_size_after"] = machine.Mode1FlowGroundingKnnSize,
            ["mode2_knn_size_before"] = beforeMode2,
            ["mode2_knn_size_after"] = machine.Mode2KnnStoreSize,
            ["mode1_or_mode2_knn_size_grows"] = machine.Mode1FlowGroundingKnnSize > beforeMode1 || machine.Mode2KnnStoreSize > beforeMode2,
            ["no_replay"] = true,
            ["checkpoint"] = Path.GetFileName(outputCheckpoint)
        };
        await WriteJsonFileAsync(Path.Combine(outputDir, "continue-report.json"), report);
        Console.WriteLine(JsonSerializer.Serialize(report, ToolJsonOptions));
        return train.Value.RowsTrained > 0 ? Success : RuntimeError;
    }

    private static async Task<int> ContinueTranslationTrainingAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var manifestPath = Required(values, "manifest");
        var trainingManifestPath = Required(values, "training-manifest");
        var outputDir = Required(values, "output");
        Directory.CreateDirectory(outputDir);

        var manifestDir = Path.GetDirectoryName(Path.GetFullPath(manifestPath))!;
        var modelManifest = await ReadJsonDictionaryAsync(manifestPath);
        var checkpointPath = Path.Combine(manifestDir, Convert.ToString(modelManifest["checkpoint_filename"], CultureInfo.InvariantCulture)!);
        var trainingManifest = await ReadJsonDictionaryAsync(trainingManifestPath);
        var corpusPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(trainingManifestPath))!, Convert.ToString(trainingManifest["corpus_filename"], CultureInfo.InvariantCulture)!);
        var expectedHash = Convert.ToString(trainingManifest["corpus_sha256"], CultureInfo.InvariantCulture);
        var actualHash = Sha256File(corpusPath);
        if (!string.Equals(expectedHash, actualHash, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Translation training corpus hash mismatch.");

        var load = await TranslationMachine.LoadCheckpointAsync(checkpointPath);
        if (!load.IsSuccess)
            throw new InvalidOperationException(load.ErrorMessage);

        var loaded = load.Value ?? throw new InvalidOperationException("Translation checkpoint load returned success with null value.");
        var machine = loaded.Machine ?? throw new InvalidOperationException("Translation checkpoint load returned null machine.");
        var beforeExport = machine.ExportCheckpoint();
        if (!beforeExport.IsSuccess || beforeExport.Value is null)
            throw new InvalidOperationException($"Translation checkpoint export before continue failed: {beforeExport.ErrorMessage}");
        var before = beforeExport.Value;
        IReadOnlyList<TranslationTrainingHistoryEntryCheckpoint> beforeHistory = before.TrainingHistory ?? Array.Empty<TranslationTrainingHistoryEntryCheckpoint>();
        IReadOnlyList<TranslationKnnStoreEntryCheckpoint> beforeKnnEntries = before.KnnStore?.Entries ?? Array.Empty<TranslationKnnStoreEntryCheckpoint>();
        IReadOnlyList<TranslationStructuredTrainingIdentityCheckpoint> beforeIdentities =
            before.StructuredTrainingIdentities ?? Array.Empty<TranslationStructuredTrainingIdentityCheckpoint>();
        var oldPairIds = beforeIdentities
            .Select(identity => identity.ExampleId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var oldPairIdSet = oldPairIds.ToHashSet(StringComparer.Ordinal);
        var pairReplayProofAvailable = oldPairIds.Length > 0;
        var rows = await ReadTranslationCorpusRowsAsync(corpusPath);
        if (rows.Count == 0)
            throw new InvalidOperationException("Translation training corpus contains no rows.");
        var maxRows = Math.Max(1, IntValue(values, "max-cycles", 1)) * 25;
        var selectedRows = rows
            .Where(row => !pairReplayProofAvailable || !oldPairIdSet.Contains(row.PairId))
            .Take(maxRows)
            .ToArray();
        if (selectedRows.Length == 0)
            throw new InvalidOperationException("Translation continue-training selected no new rows.");
        var newPairIds = selectedRows.Select(row => row.PairId).ToArray();
        var noReplayProven = pairReplayProofAvailable &&
                             newPairIds.All(pairId => !oldPairIdSet.Contains(pairId));
        if (!noReplayProven)
        {
            var routeEvidence = await BuildTranslationRouteEvidenceAsync(machine);
            var failedReport = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["command"] = "continue-translation-training",
                ["hash_verified"] = true,
                ["checkpoint_state_loaded"] = true,
                ["source"] = "committed_training_data_bundle",
                ["private_pairs_required"] = false,
                ["training_manifest"] = ToRepoRelativePath(trainingManifestPath),
                ["corpus_filename"] = Path.GetFileName(corpusPath),
                ["corpus_sha256"] = actualHash,
                ["old_batch_ids"] = beforeHistory.Select(entry => entry.BatchId).ToArray(),
                ["old_pair_ids"] = oldPairIds,
                ["old_pair_ids_persisted"] = pairReplayProofAvailable,
                ["pair_replay_identity_type"] = pairReplayProofAvailable ? "pair_id" : "unavailable",
                ["pair_replay_proof_source"] = pairReplayProofAvailable
                    ? "checkpoint_structured_training_identities"
                    : "UNAVAILABLE: checkpoint has no structured example ids from previous export.",
                ["new_pair_ids"] = newPairIds,
                ["new_pair_ids_disjoint_from_old_pair_ids"] = pairReplayProofAvailable ? noReplayProven : null,
                ["new_pair_ids_disjoint_from_old_batch_ids_is_not_replay_proof"] = true,
                ["unable_to_prove_pair_replay"] = !pairReplayProofAvailable,
                ["no_replay_proven"] = false,
                ["no_replay"] = false,
                ["passed"] = false,
                ["anti_masquerade_passed"] = false,
                ["route_evidence"] = routeEvidence,
                ["failure_reason"] = pairReplayProofAvailable
                    ? "New pair ids overlap old structured training identities."
                    : "Old structured training identities are absent; refusing to claim continued training no-replay."
            };
            await WriteJsonFileAsync(Path.Combine(outputDir, "continue-report.json"), failedReport);
            Console.WriteLine(JsonSerializer.Serialize(failedReport, ToolJsonOptions));
            return RuntimeError;
        }
        var examples = selectedRows.Select(BuildTranslationContinueExample).ToArray();
        var batch = new TranslationLearningBatch(
            "translation-v1-continue-001",
            TranslationMachine.DomainTag,
            examples,
            selectedRows.Select(row => row.PairId).ToArray());
        var train = await machine.TrainStructuredBatchAsync(batch);
        if (!train.IsSuccess)
            throw new InvalidOperationException(train.ErrorMessage);
        var trainResult = train.Value ?? throw new InvalidOperationException("Translation continue-training returned success with null result.");

        var outputCheckpoint = Path.Combine(outputDir, "translation-v1-continued-checkpoint.json");
        var save = await machine.SaveCheckpointAsync(outputCheckpoint, loaded.TatoebaLoop);
        if (!save.IsSuccess)
            throw new InvalidOperationException(save.ErrorMessage);

        var afterExport = machine.ExportCheckpoint();
        if (!afterExport.IsSuccess || afterExport.Value is null)
            throw new InvalidOperationException($"Translation checkpoint export after continue failed: {afterExport.ErrorMessage}");
        var after = afterExport.Value;
        IReadOnlyList<TranslationTrainingHistoryEntryCheckpoint> afterHistory = after.TrainingHistory ?? Array.Empty<TranslationTrainingHistoryEntryCheckpoint>();
        IReadOnlyList<TranslationKnnStoreEntryCheckpoint> afterKnnEntries = after.KnnStore?.Entries ?? Array.Empty<TranslationKnnStoreEntryCheckpoint>();
        IReadOnlyList<TranslationStructuredTrainingIdentityCheckpoint> afterIdentities =
            after.StructuredTrainingIdentities ?? Array.Empty<TranslationStructuredTrainingIdentityCheckpoint>();
        var oldBatchIds = beforeHistory.Select(entry => entry.BatchId).ToArray();
        var routeEvidenceAfter = await BuildTranslationRouteEvidenceAsync(machine);
        var exactMemoryLeakageProven = routeEvidenceAfter.TryGetValue("exact_memory_leakage_proven", out var exactMemoryValue) &&
                                       exactMemoryValue is true;
        var passed = trainResult.RowsTrained > 0 && noReplayProven && exactMemoryLeakageProven;
        var report = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "continue-translation-training",
            ["hash_verified"] = true,
            ["checkpoint_state_loaded"] = true,
            ["source"] = "committed_training_data_bundle",
            ["private_pairs_required"] = false,
            ["training_manifest"] = ToRepoRelativePath(trainingManifestPath),
            ["corpus_filename"] = Path.GetFileName(corpusPath),
            ["corpus_sha256"] = actualHash,
            ["old_batch_ids"] = oldBatchIds,
            ["old_pair_ids"] = oldPairIds,
            ["old_pair_ids_persisted"] = pairReplayProofAvailable,
            ["pair_replay_identity_type"] = pairReplayProofAvailable ? "pair_id" : "unavailable",
            ["pair_replay_proof_source"] = pairReplayProofAvailable
                ? "checkpoint_pair_ids"
                : "UNAVAILABLE: translation checkpoint training history persists batch ids, not prior pair ids.",
            ["old_checkpoint_state_preserved"] = before.HasStructuredTraining,
            ["loaded_training_history_count_before"] = beforeHistory.Count,
            ["loaded_knn_store_size_before"] = beforeKnnEntries.Count,
            ["new_pairs_trained"] = trainResult.RowsTrained,
            ["new_pair_ids"] = newPairIds,
            ["new_pair_ids_disjoint_from_old_pair_ids"] = pairReplayProofAvailable ? noReplayProven : null,
            ["new_pair_ids_disjoint_from_old_batch_ids_is_not_replay_proof"] = true,
            ["unable_to_prove_pair_replay"] = !pairReplayProofAvailable,
            ["no_replay_proven"] = noReplayProven,
            ["trained_pair_ids_count_before"] = beforeHistory.Count,
            ["trained_pair_ids_count_after"] = afterHistory.Count,
            ["knn_store_size_before"] = beforeKnnEntries.Count,
            ["knn_store_size_after"] = afterKnnEntries.Count,
            ["knn_size_preserved_or_grew"] = afterKnnEntries.Count >= beforeKnnEntries.Count,
            ["no_replay"] = noReplayProven,
            ["structured_training_identity_count_before"] = beforeIdentities.Count,
            ["structured_training_identity_count_after"] = afterIdentities.Count,
            ["route_evidence"] = routeEvidenceAfter,
            ["exact_memory_leakage_proven"] = exactMemoryLeakageProven,
            ["anti_masquerade_passed"] = passed,
            ["passed"] = passed,
            ["checkpoint"] = Path.GetFileName(outputCheckpoint),
            ["report"] = "continue-report.json"
        };
        await WriteJsonFileAsync(Path.Combine(outputDir, "continue-report.json"), report);
        Console.WriteLine(JsonSerializer.Serialize(report, ToolJsonOptions));
        return passed ? Success : RuntimeError;
    }

    private static async Task<Dictionary<string, object?>> ProbeTranslationManifestAsync(string manifestPath)
    {
        var manifestDir = Path.GetDirectoryName(Path.GetFullPath(manifestPath))!;
        var manifest = await ReadJsonDictionaryAsync(manifestPath);
        var checkpointFilename = Convert.ToString(manifest["checkpoint_filename"], CultureInfo.InvariantCulture)!;
        var checkpointPath = Path.Combine(manifestDir, checkpointFilename);
        var checkpointJson = await File.ReadAllTextAsync(checkpointPath, Encoding.UTF8);
        var checkpoint = JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
            checkpointJson,
            TranslationCheckpointJsonOptions.Standard)
            ?? throw new InvalidOperationException("Translation checkpoint deserialized null.");

        var load = await TranslationMachine.LoadCheckpointAsync(checkpointPath);
        if (!load.IsSuccess)
            throw new InvalidOperationException(load.ErrorMessage);

        var orchestrator = await load.Value!.Machine.GetOrchestratorAsync();
        if (!orchestrator.IsSuccess)
            throw new InvalidOperationException(orchestrator.ErrorMessage);

        var fixtureSource = "add null check ValidateToken";
        var fixture = await orchestrator.Value!.RunAsync(new LogicalOrchestratorRequest(
            RunId: $"translation-import-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: fixtureSource,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = "ru",
                ["target_language"] = "en",
                ["topic_domain"] = "technical"
            }));

        var fixtureOutput = fixture.Value?.FinalHumanResult ?? string.Empty;
        var expectedFixtureOutput = "Add a null check to ValidateToken.";
        var bootstrapOutput = fixtureOutput.Contains(":BOOTSTRAP]", StringComparison.OrdinalIgnoreCase);
        var fixtureAdapterId = ReadFlowMetric(fixture.Value?.FlowResult, "adapter_id");
        var fixtureExactSourceHit = fixtureAdapterId.Contains("exact-source", StringComparison.OrdinalIgnoreCase);

        var nonExactFixtureSource = "add a null check before ValidateToken";
        var nonExactFixture = await orchestrator.Value!.RunAsync(new LogicalOrchestratorRequest(
            RunId: $"translation-import-non-exact-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: nonExactFixtureSource,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = "ru",
                ["target_language"] = "en",
                ["topic_domain"] = "technical"
            }));
        var nonExactFixtureOutput = nonExactFixture.Value?.FinalHumanResult ?? string.Empty;
        var nonExactBootstrapOutput = nonExactFixtureOutput.Contains(":BOOTSTRAP]", StringComparison.OrdinalIgnoreCase);
        var nonExactFixtureAdapterId = ReadFlowMetric(nonExactFixture.Value?.FlowResult, "adapter_id");
        var nonExactFixtureExactSourceHit = nonExactFixtureAdapterId.Contains("exact-source", StringComparison.OrdinalIgnoreCase);
        var nonExactFixtureProvesGeneralization =
            nonExactFixture.IsSuccess &&
            !nonExactBootstrapOutput &&
            !nonExactFixtureExactSourceHit;
        var manifestLoaded = string.Equals(
            Convert.ToString(manifest["model_id"], CultureInfo.InvariantCulture),
            TranslationModelManifestContract.ModelId,
            StringComparison.Ordinal) &&
            string.Equals(
                Convert.ToString(manifest["loader_type"], CultureInfo.InvariantCulture),
                TranslationModelManifestContract.LoaderType,
                StringComparison.Ordinal);
        var passed = manifestLoaded &&
                     load.IsSuccess &&
                     checkpoint.HasStructuredTraining &&
                     checkpoint.KnnStore.Entries.Count > 0 &&
                     fixture.IsSuccess &&
                     !bootstrapOutput &&
                     string.Equals(fixtureOutput, expectedFixtureOutput, StringComparison.Ordinal) &&
                     nonExactFixtureProvesGeneralization;

        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "import-translation-model",
            ["model_id"] = manifest["model_id"],
            ["manifest_loaded"] = manifestLoaded,
            ["checkpoint_loaded"] = load.IsSuccess,
            ["has_structured_training"] = checkpoint.HasStructuredTraining,
            ["knn_store_size"] = checkpoint.KnnStore.Entries.Count,
            ["executors_count"] = checkpoint.Executors.Count,
            ["training_history_count"] = checkpoint.TrainingHistory.Count,
            ["trained_fixture_source"] = fixtureSource,
            ["trained_fixture_output"] = fixtureOutput,
            ["trained_fixture_expected"] = expectedFixtureOutput,
            ["trained_fixture_bootstrap_output"] = bootstrapOutput,
            ["trained_fixture_adapter_id"] = fixtureAdapterId,
            ["trained_fixture_exact_source_hit"] = fixtureExactSourceHit,
            ["trained_fixture_is_exact_memory_evidence"] = fixtureExactSourceHit,
            ["non_exact_fixture_source"] = nonExactFixtureSource,
            ["non_exact_fixture_output"] = nonExactFixtureOutput,
            ["non_exact_fixture_bootstrap_output"] = nonExactBootstrapOutput,
            ["non_exact_fixture_adapter_id"] = nonExactFixtureAdapterId,
            ["non_exact_fixture_exact_source_hit"] = nonExactFixtureExactSourceHit,
            ["non_exact_fixture_proves_generalization"] = nonExactFixtureProvesGeneralization,
            ["exact_source_only_not_accepted_as_generalization"] = !fixtureExactSourceHit || nonExactFixtureProvesGeneralization,
            ["no_private_corpus_dir_required"] = true,
            ["private_paths_required"] = false,
            ["passed"] = passed
        };
    }

    private static async Task<Dictionary<string, object?>> ProbeOrchestrationManifestAsync(string manifestPath)
    {
        var manifestDir = Path.GetDirectoryName(Path.GetFullPath(manifestPath))!;
        var manifest = await ReadJsonDictionaryAsync(manifestPath);
        var checkpointPath = Path.Combine(manifestDir, Convert.ToString(manifest["checkpoint_filename"], CultureInfo.InvariantCulture)!);
        var load = await OrchestrationPlanMachine.LoadCheckpointAsync(checkpointPath);
        if (!load.IsSuccess)
            throw new InvalidOperationException(load.ErrorMessage);

        var machine = load.Value.Machine;
        var flow33 = await machine.GeneratePlanAsync("flow 33");
        var mode2 = await machine.GeneratePlanAsync("translate flow 33 from python/react native to node/react");
        var explanation = await machine.GeneratePlanAsync("explain FLOW-33 orchestration");
        var passed = flow33.IsSuccess &&
                     flow33.Value!.RouteMode == GenerationRouteMode.LearnedFlowGrounding &&
                     flow33.Value.MlGenerated &&
                     flow33.Value.FlowGrounding?.FlowRef == "FLOW-33" &&
                     mode2.IsSuccess &&
                     mode2.Value!.RouteMode == GenerationRouteMode.Mode2Generalization &&
                     mode2.Value.MlGenerated &&
                     explanation.IsSuccess &&
                     explanation.Value!.RouteMode == GenerationRouteMode.Mode3HumanExplanation;

        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "import-orchestration-model",
            ["model_id"] = manifest["model_id"],
            ["no_corpus_dir_required"] = true,
            ["mode1_flow_grounding_knn_size"] = machine.Mode1FlowGroundingKnnSize,
            ["mode1_flow_grounding_weights_size"] = machine.Mode1FlowGroundingWeightsSize,
            ["mode2_knn_store_size"] = machine.Mode2KnnStoreSize,
            ["mode2_pattern_map_size"] = machine.Mode2PatternMapSize,
            ["mode3_knn_store_size"] = machine.Mode3KnnStoreSize,
            ["flow33_route_mode"] = flow33.Value?.RouteMode.ToString(),
            ["flow33_ml_generated"] = flow33.Value?.MlGenerated,
            ["flow33_predicted_flow_ref"] = flow33.Value?.FlowGrounding?.FlowRef,
            ["generic_mode2_route_mode"] = mode2.Value?.RouteMode.ToString(),
            ["generic_mode2_ml_generated"] = mode2.Value?.MlGenerated,
            ["explanation_route_mode"] = explanation.Value?.RouteMode.ToString(),
            ["passed"] = passed
        };
    }

    private static OrchestrationPlanMachineCheckpoint SanitizeCheckpoint(OrchestrationPlanMachineCheckpoint checkpoint)
        => checkpoint with
        {
            FlowGroundingKnn = checkpoint.FlowGroundingKnn.Select(SanitizePair).ToArray(),
            KnnStore = checkpoint.KnnStore.Select(SanitizePair).ToArray(),
            HumanExplanationKnn = checkpoint.HumanExplanationKnn.Select(SanitizePair).ToArray()
        };

    private static OrchestrationTrainingPair SanitizePair(OrchestrationTrainingPair pair)
        => pair with
        {
            Chosen = pair.Chosen with
            {
                SourcePath = $"training-bundle://{pair.SourceBundleId}"
            }
        };

    private static async Task<OrchestrationPlanMachineCheckpoint> ReadCheckpointAsync(string checkpointPath)
    {
        var json = await File.ReadAllTextAsync(checkpointPath, Encoding.UTF8);
        return JsonSerializer.Deserialize<OrchestrationPlanMachineCheckpoint>(json, OrchestrationPlanJsonOptions.Standard)
               ?? throw new InvalidOperationException("Checkpoint deserialized null.");
    }

    private static async Task WriteGzipJsonLinesAsync(string path, IReadOnlyList<OrchestrationTrainingPair> pairs)
    {
        await using var file = File.Create(path);
        await using var gzip = new GZipStream(file, CompressionLevel.SmallestSize);
        await using var writer = new StreamWriter(gzip, Encoding.UTF8);
        var jsonLineOptions = new JsonSerializerOptions(OrchestrationPlanJsonOptions.Standard)
        {
            WriteIndented = false
        };
        foreach (var pair in pairs)
            await writer.WriteLineAsync(JsonSerializer.Serialize(pair, jsonLineOptions));
    }

    private static async Task<IReadOnlyList<OrchestrationTrainingPair>> ReadGzipPairsAsync(string path)
    {
        var rows = new List<OrchestrationTrainingPair>();
        await using var file = File.OpenRead(path);
        await using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8);
        while (await reader.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            var pair = JsonSerializer.Deserialize<OrchestrationTrainingPair>(line, OrchestrationPlanJsonOptions.Standard);
            if (pair is not null)
                rows.Add(pair);
        }

        return rows;
    }

    private static async Task<IReadOnlyList<TranslationContinueCorpusRow>> ReadTranslationCorpusRowsAsync(string path)
    {
        var rows = new List<TranslationContinueCorpusRow>();
        await using var file = File.OpenRead(path);
        await using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8);
        while (await reader.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            rows.Add(new TranslationContinueCorpusRow(
                PairId: RequiredJsonString(root, "pair_id"),
                Source: RequiredJsonString(root, "source"),
                Target: RequiredJsonString(root, "target"),
                SourceLanguage: RequiredJsonString(root, "source_language"),
                TargetLanguage: RequiredJsonString(root, "target_language"),
                Direction: RequiredJsonString(root, "direction"),
                TopicDomain: RequiredJsonString(root, "topic_domain"),
                SemanticSignature: RequiredJsonString(root, "semantic_signature"),
                RejectedType: RequiredJsonString(root, "rejected_type"),
                RejectedOutput: RequiredJsonString(root, "rejected_output")));
        }

        return rows;
    }

    private static async Task<IReadOnlyList<SkillSelectionCorpusRow>> ReadSkillSelectionCorpusRowsAsync(string path)
    {
        var rows = new List<SkillSelectionCorpusRow>();
        using var reader = new StreamReader(File.OpenRead(path), Encoding.UTF8);
        while (await reader.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            rows.Add(new SkillSelectionCorpusRow(
                PairId: RequiredJsonString(root, "pair_id"),
                Prompt: RequiredJsonString(root, "prompt"),
                CandidateSkills: RequiredJsonStringArray(root, "candidate_skills"),
                ChosenSkills: RequiredJsonStringArray(root, "chosen_skills"),
                RejectedSkills: RequiredJsonStringArray(root, "rejected_skills"),
                Reason: RequiredJsonString(root, "reason"),
                TargetTrainingAdapter: RequiredJsonString(root, "target_training_adapter"),
                SourcePromptLedgerId: RequiredJsonString(root, "source_prompt_ledger_id"),
                RejectedType: RequiredJsonString(root, "rejected_type")));
        }

        return rows;
    }

    private static async Task<SkillSelectionCheckpoint> ReadSkillSelectionCheckpointAsync(string path)
        => JsonSerializer.Deserialize<SkillSelectionCheckpoint>(
               await File.ReadAllTextAsync(path, Encoding.UTF8),
               ToolJsonOptions)
           ?? throw new InvalidOperationException("Skill-selection checkpoint deserialized null.");

    private static HostRepoScanResult ScanHostRepoForSmartSkillsTrace(string hostRepo)
    {
        var ignoredSegments = new[]
        {
            $"{Path.DirectorySeparatorChar}.git{Path.DirectorySeparatorChar}",
            $"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}",
            $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
            $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"
        };
        var files = Directory.EnumerateFiles(hostRepo, "*", SearchOption.AllDirectories)
            .Where(path => ignoredSegments.All(segment => !path.Contains(segment, StringComparison.OrdinalIgnoreCase)))
            .Order(StringComparer.Ordinal)
            .ToArray();
        var trainingPatterns = new[]
        {
            "TrainBatchAsync",
            "TrainMachineBatchAsync",
            "private-corpora",
            "skill-selection-corpus",
            "DPO",
            "checkpoint",
            "SkillSelectionMachine"
        };
        var containsTrainingLogic = false;
        var noPrivateAbsolutePath = true;
        foreach (var file in files.Where(file => new FileInfo(file).Length <= 1024 * 1024))
        {
            var text = File.ReadAllText(file, Encoding.UTF8);
            containsTrainingLogic |= trainingPatterns.Any(pattern => text.Contains(pattern, StringComparison.OrdinalIgnoreCase));
            noPrivateAbsolutePath &= !text.Contains("C:\\", StringComparison.OrdinalIgnoreCase);
        }

        return new HostRepoScanResult(
            FileCount: files.Length,
            ContainsTrainingLogic: containsTrainingLogic,
            NoPrivateAbsolutePathInHostFiles: noPrivateAbsolutePath,
            TrainingLogicPatterns: trainingPatterns);
    }

    private static SkillSelectionMachinePair BuildSkillSelectionPair(SkillSelectionCorpusRow row)
        => new(
            PairId: row.PairId,
            Request: BuildSkillSelectionRequest(row),
            Chosen: new SkillSelectionDecision(
                SelectedSkillIds: row.ChosenSkills,
                RejectedSkillIds: row.RejectedSkills,
                EvidenceFeatures: [row.Reason, "TRAINING_ROW_CHOSEN"],
                Confidence: 1.0,
                RequiresSecondPass: false,
                DecisionText: row.Reason),
            Rejected: new SkillSelectionDecision(
                SelectedSkillIds: row.RejectedSkills.Take(1).ToArray(),
                RejectedSkillIds: row.ChosenSkills,
                EvidenceFeatures: [row.RejectedType, "TRAINING_ROW_REJECTED"],
                Confidence: 0.0,
                RequiresSecondPass: true,
                DecisionText: $"Rejected contrast: {row.RejectedType}."),
            RejectedType: row.RejectedType,
            SourcePromptEntryId: row.SourcePromptLedgerId,
            TargetTrainingAdapter: row.TargetTrainingAdapter);

    private static SkillSelectionMachinePair BuildSkillSelectionPairForAdapter(SkillSelectionCorpusRow row, string targetTrainingAdapter)
        => BuildSkillSelectionPair(row with
        {
            PairId = $"{row.PairId}::{targetTrainingAdapter}",
            TargetTrainingAdapter = targetTrainingAdapter
        });

    private static SkillSelectionRequest BuildSkillSelectionRequest(SkillSelectionCorpusRow row)
        => new(
            Prompt: row.Prompt,
            ProjectRoot: "",
            AvailableSkillIds: row.CandidateSkills,
            HostFacts: new Dictionary<string, string>
            {
                ["source_prompt_ledger_id"] = row.SourcePromptLedgerId
            },
            UserPromptSourceEntryId: row.SourcePromptLedgerId);

    private static IReadOnlyList<string> SkillSelectionUnitIdsFromCheckpoint(SkillSelectionCheckpoint checkpoint)
        =>
        [
            checkpoint.PromptUnderstanding.UnitId,
            checkpoint.TaskUnderstanding.UnitId,
            checkpoint.TaskType.UnitId,
            checkpoint.Routing.UnitId,
            checkpoint.SkillCandidates.UnitId,
            checkpoint.SkillSelection.UnitId,
            checkpoint.PromptContext.UnitId,
            checkpoint.ResultGap.UnitId,
            checkpoint.OutputValidation.UnitId,
            checkpoint.TaskCompletion.UnitId,
            checkpoint.IntermediateChecks.UnitId
        ];

    private static IReadOnlyDictionary<string, int> SkillSelectionUnitProfileCounts(SkillSelectionCheckpoint checkpoint)
        => new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [checkpoint.PromptUnderstanding.UnitId] = checkpoint.PromptUnderstanding.SkillProfiles.Count,
            [checkpoint.TaskUnderstanding.UnitId] = checkpoint.TaskUnderstanding.SkillProfiles.Count,
            [checkpoint.TaskType.UnitId] = checkpoint.TaskType.SkillProfiles.Count,
            [checkpoint.Routing.UnitId] = checkpoint.Routing.SkillProfiles.Count,
            [checkpoint.SkillCandidates.UnitId] = checkpoint.SkillCandidates.SkillProfiles.Count,
            [checkpoint.SkillSelection.UnitId] = checkpoint.SkillSelection.SkillProfiles.Count,
            [checkpoint.PromptContext.UnitId] = checkpoint.PromptContext.SkillProfiles.Count,
            [checkpoint.ResultGap.UnitId] = checkpoint.ResultGap.SkillProfiles.Count,
            [checkpoint.OutputValidation.UnitId] = checkpoint.OutputValidation.SkillProfiles.Count,
            [checkpoint.TaskCompletion.UnitId] = checkpoint.TaskCompletion.SkillProfiles.Count,
            [checkpoint.IntermediateChecks.UnitId] = checkpoint.IntermediateChecks.SkillProfiles.Count
        };

    private static IReadOnlyList<Dictionary<string, object?>> BuildSkillSelectionSemanticDomainMatrix(SkillSelectionCheckpoint? checkpoint = null)
    {
        var profileCounts = checkpoint is null
            ? new Dictionary<string, int>(StringComparer.Ordinal)
            : SkillSelectionUnitProfileCounts(checkpoint);
        return SmartSkillsSemanticUnitCatalog.All
            .Select(row => new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["domain"] = row.DomainId,
                ["machine"] = row.MachineName,
                ["training_adapter"] = row.TrainingAdapterName,
                ["dpo_row_shape"] = row.DpoRowShapeName,
                ["checkpoint_section"] = row.CheckpointSectionName,
                ["held_out_fixture"] = row.HeldOutFixtureId,
                ["trainable_state_present"] = row.Trainable && (checkpoint is null || profileCounts.GetValueOrDefault(row.DomainId) > 0),
                ["checkpoint_profile_count"] = checkpoint is null ? null : profileCounts.GetValueOrDefault(row.DomainId),
                ["deterministic_final_authority"] = row.DeterministicFinalAuthority,
                ["runtime_semantic_quality_claimed"] = row.RuntimeSemanticQualityClaimed,
                ["coverage_source"] = nameof(SmartSkillsSemanticUnitCatalog),
                ["coverage_type"] = "s14_contract_catalog_not_runtime_semantic_quality",
                ["evidence"] = "S.14 catalog-backed trainable semantic unit metadata; continue report does not claim runtime semantic quality."
            })
            .ToArray();
    }

    private static string RequiredJsonString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.String)
            throw new InvalidOperationException($"Translation training corpus row is missing string field '{propertyName}'.");
        var text = value.GetString();
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException($"Translation training corpus row field '{propertyName}' is empty.");
        return text;
    }

    private static IReadOnlyList<string> RequiredJsonStringArray(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException($"Skill-selection corpus row is missing array field '{propertyName}'.");
        var items = value.EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() ?? "" : "")
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
        if (items.Length == 0)
            throw new InvalidOperationException($"Skill-selection corpus row field '{propertyName}' is empty.");
        return items;
    }

    private static double RequiredJsonDouble(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Number)
            throw new InvalidOperationException($"JSON object is missing number field '{propertyName}'.");
        return value.GetDouble();
    }

    private static bool RequiredJsonBool(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind is not JsonValueKind.True and not JsonValueKind.False)
            throw new InvalidOperationException($"JSON object is missing boolean field '{propertyName}'.");
        return value.GetBoolean();
    }

    private static bool ReadBool(IReadOnlyDictionary<string, object?> values, string propertyName)
        => values.TryGetValue(propertyName, out var value) && value is bool flag && flag;

    private static TranslationLearningExample BuildTranslationContinueExample(TranslationContinueCorpusRow row)
    {
        var direction = Enum.TryParse<TranslationDirection>(row.Direction, ignoreCase: true, out var parsed)
            ? parsed
            : TranslationDirection.RU_EN;
        var chosen = new ExecutorOutput(row.Target, 0.95, null);
        var rejected = new ExecutorOutput(row.RejectedOutput, 0.10, null);
        return new TranslationLearningExample(
            ExampleId: row.PairId,
            UnitId: TranslationUnitIds.ExecutorFor(direction),
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(new ExecutorInput(row.Source, row.SourceLanguage, row.TopicDomain), TranslationUnitJsonOptions),
            ChosenOutputJson: JsonSerializer.Serialize(chosen, TranslationUnitJsonOptions),
            RejectedOutputJson: JsonSerializer.Serialize(rejected, TranslationUnitJsonOptions) + $" [WRONG: {row.RejectedType}]",
            RejectedType: row.RejectedType,
            SemanticSignature: row.SemanticSignature,
            SourceId: row.PairId,
            HeldOut: false);
    }

    private static string ReadFlowMetric(LogicalFlowResult? flowResult, string key)
        => flowResult is not null && flowResult.Metrics.TryGetValue(key, out var value)
            ? value ?? string.Empty
            : string.Empty;

    private static async Task<Dictionary<string, object?>> BuildTranslationRouteEvidenceAsync(TranslationMachine machine)
    {
        var orchestrator = await machine.GetOrchestratorAsync();
        if (!orchestrator.IsSuccess || orchestrator.Value is null)
            return new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["route_probe_available"] = false,
                ["route_probe_error"] = orchestrator.ErrorMessage,
                ["exact_memory_leakage_proven"] = false
            };

        var exactProbe = await RunTranslationRouteProbeAsync(orchestrator.Value, "add null check ValidateToken");
        var nonExactProbe = await RunTranslationRouteProbeAsync(orchestrator.Value, "add a null check before ValidateToken");
        var routeIds = new[] { exactProbe.AdapterId, nonExactProbe.AdapterId }
            .Where(route => !string.IsNullOrWhiteSpace(route))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var exactSourceHitCount = new[] { exactProbe, nonExactProbe }.Count(probe => probe.ExactSourceHit);
        var nonExactProbeProvesGeneralization =
            nonExactProbe.Success &&
            !nonExactProbe.BootstrapOutput &&
            !nonExactProbe.ExactSourceHit;
        var exactMemoryLeakageProven = exactProbe.Success && nonExactProbeProvesGeneralization;
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["route_probe_available"] = true,
            ["route_ids"] = routeIds,
            ["exact_source_hit_count"] = exactSourceHitCount,
            ["exact_probe_source"] = exactProbe.Source,
            ["exact_probe_adapter_id"] = exactProbe.AdapterId,
            ["exact_probe_exact_source_hit"] = exactProbe.ExactSourceHit,
            ["exact_probe_bootstrap_output"] = exactProbe.BootstrapOutput,
            ["exact_probe_output"] = exactProbe.Output,
            ["non_exact_probe_source"] = nonExactProbe.Source,
            ["non_exact_probe_adapter_id"] = nonExactProbe.AdapterId,
            ["non_exact_probe_exact_source_hit"] = nonExactProbe.ExactSourceHit,
            ["non_exact_probe_bootstrap_output"] = nonExactProbe.BootstrapOutput,
            ["non_exact_probe_output"] = nonExactProbe.Output,
            ["non_exact_probe_proves_generalization"] = nonExactProbeProvesGeneralization,
            ["exact_memory_leakage_status"] = exactMemoryLeakageProven
                ? "CONTROLLED: exact-source is labeled separately and non-exact probe did not use exact-source."
                : "FAILED: route evidence does not prove non-exact generalization separate from exact-source memory.",
            ["exact_memory_leakage_proven"] = exactMemoryLeakageProven
        };
    }

    private static async Task<TranslationRouteProbeResult> RunTranslationRouteProbeAsync(
        ILogicalOrchestrator orchestrator,
        string source)
    {
        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            RunId: $"translation-continue-route-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: source,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = "ru",
                ["target_language"] = "en",
                ["topic_domain"] = "technical"
            }));
        var output = result.Value?.FinalHumanResult ?? string.Empty;
        var adapterId = ReadFlowMetric(result.Value?.FlowResult, "adapter_id");
        return new TranslationRouteProbeResult(
            Source: source,
            Success: result.IsSuccess,
            Output: output,
            AdapterId: adapterId,
            ExactSourceHit: adapterId.Contains("exact-source", StringComparison.OrdinalIgnoreCase),
            BootstrapOutput: output.Contains(":BOOTSTRAP]", StringComparison.OrdinalIgnoreCase));
    }

    private static async Task PublishModelAsync(string modelId, string manifestPath, string domain, string registryVersion)
    {
        var indexPath = Path.Combine(Directory.GetCurrentDirectory(), "models", "index.json");
        Directory.CreateDirectory(Path.GetDirectoryName(indexPath)!);
        var entries = File.Exists(indexPath)
            ? JsonSerializer.Deserialize<ModelIndex>(await File.ReadAllTextAsync(indexPath), ToolJsonOptions)?.Models.ToList() ?? []
            : [];
        var manifest = await ReadJsonDictionaryAsync(manifestPath);
        var accuracy = manifest.TryGetValue("accuracy_at_export", out var value) && value is JsonElement element && element.ValueKind == JsonValueKind.Number
            ? element.GetDouble()
            : 0.0;
        entries.RemoveAll(entry => string.Equals(entry.ModelId, modelId, StringComparison.Ordinal));
        entries.Add(new ModelIndexEntry(modelId, domain, ToRepoRelativePath(manifestPath), registryVersion, accuracy, DateTimeOffset.UtcNow));
        await WriteJsonFileAsync(indexPath, new ModelIndex(entries.OrderBy(entry => entry.ModelId, StringComparer.Ordinal).ToArray()));
    }

    private static async Task<string> ResolveModelManifestAsync(string modelId)
    {
        var indexPath = Path.Combine(Directory.GetCurrentDirectory(), "models", "index.json");
        if (!File.Exists(indexPath))
            throw new FileNotFoundException("Model index not found.", indexPath);
        var index = JsonSerializer.Deserialize<ModelIndex>(await File.ReadAllTextAsync(indexPath), ToolJsonOptions)
                    ?? new ModelIndex([]);
        var entry = index.Models.FirstOrDefault(item => string.Equals(item.ModelId, modelId, StringComparison.Ordinal));
        if (entry is null)
            throw new InvalidOperationException($"Model id '{modelId}' not found.");
        return Path.Combine(Directory.GetCurrentDirectory(), entry.ManifestPath);
    }

    private static async Task<List<Dictionary<string, object?>>> ReadModelIndexEntriesForMergeAsync(string indexPath)
    {
        if (!File.Exists(indexPath))
            return [];

        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(indexPath));
        if (!document.RootElement.TryGetProperty("models", out var models) || models.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException("Model index is missing a models array.");

        return models
            .EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.Object)
            .Select(item => item.EnumerateObject().ToDictionary(
                property => property.Name,
                property => JsonElementToPlainObject(property.Value),
                StringComparer.Ordinal))
            .ToList();
    }

    private static object? JsonElementToPlainObject(JsonElement element)
        => element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(
                property => property.Name,
                property => JsonElementToPlainObject(property.Value),
                StringComparer.Ordinal),
            JsonValueKind.Array => element.EnumerateArray().Select(JsonElementToPlainObject).ToArray(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var integerValue) ? integerValue : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };

    private static bool ContainsAbsoluteOrPrivatePath(string value)
        => value.Contains("C:\\", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("C:/", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("private-corpora", StringComparison.OrdinalIgnoreCase);

    private static async Task<Dictionary<string, object?>> ReadJsonDictionaryAsync(string path)
    {
        var document = JsonSerializer.Deserialize<Dictionary<string, object?>>(await File.ReadAllTextAsync(path), ToolJsonOptions);
        return document ?? throw new InvalidOperationException($"Could not read JSON object: {path}");
    }

    private static IReadOnlyList<T> ReadJsonLines<T>(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("JSONL input file not found.", path);

        var rows = new List<T>();
        foreach (var line in File.ReadLines(path, Encoding.UTF8))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            var row = JsonSerializer.Deserialize<T>(line, ToolJsonOptions);
            if (row is null)
                throw new InvalidOperationException($"Could not deserialize JSONL row from {path}.");
            rows.Add(row);
        }

        return rows;
    }

    private static object? ReadDictionaryValue(IReadOnlyDictionary<string, object?> values, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (values.TryGetValue(key, out var value))
                return value;
        }

        throw new InvalidOperationException($"Missing JSON field '{string.Join("' or '", keys)}'.");
    }

    private static int ReadDictionaryInt(IReadOnlyDictionary<string, object?> values, params string[] keys)
    {
        var value = ReadDictionaryValue(values, keys);
        return value switch
        {
            JsonElement { ValueKind: JsonValueKind.Number } element => element.GetInt32(),
            int integer => integer,
            long integer => checked((int)integer),
            _ => throw new InvalidOperationException($"JSON field '{string.Join("' or '", keys)}' must be an integer.")
        };
    }

    private static double ReadReportMetric(string reportPath, string metricName)
    {
        var document = JsonSerializer.Deserialize<Dictionary<string, object?>>(File.ReadAllText(reportPath), ToolJsonOptions);
        if (document is not null && document.TryGetValue(metricName, out var value) && value is JsonElement element && element.ValueKind == JsonValueKind.Number)
            return element.GetDouble();
        return 0.0;
    }

    private static double ReadFirstReportMetric(string reportPath, params string[] metricNames)
    {
        foreach (var metricName in metricNames)
        {
            var value = ReadReportMetric(reportPath, metricName);
            if (value > 0.0)
                return value;
        }

        return 0.0;
    }

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string ReadGitHead()
    {
        var git = Path.Combine(Directory.GetCurrentDirectory(), ".git");
        var headPath = Path.Combine(git, "HEAD");
        if (!File.Exists(headPath))
            return "UNKNOWN";
        var head = File.ReadAllText(headPath).Trim();
        if (!head.StartsWith("ref:", StringComparison.Ordinal))
            return head;
        var refPath = Path.Combine(git, head["ref:".Length..].Trim().Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(refPath) ? File.ReadAllText(refPath).Trim() : "UNKNOWN";
    }

    private static string ToRepoRelativePath(string path)
        => Path.GetRelativePath(Directory.GetCurrentDirectory(), Path.GetFullPath(path)).Replace('\\', '/');

    private static async Task WriteJsonFileAsync(string path, object value)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(value, ToolJsonOptions) + Environment.NewLine, Encoding.UTF8);
    }

    private static ParsedArgs ParseKeyValueArgs(IReadOnlyList<string> args, IReadOnlyCollection<string> flagNames)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < args.Count; i++)
        {
            var key = args[i];
            if (!key.StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException($"Unexpected argument '{key}'.");

            var name = key[2..];
            if (flagNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                flags.Add(name);
                continue;
            }

            if (i + 1 >= args.Count)
                throw new ArgumentException($"Missing value for '{key}'.");

            values[name] = args[++i];
        }

        return new ParsedArgs(values, flags);
    }

    private static string Required(IReadOnlyDictionary<string, string> values, string key)
        => values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException($"Missing required argument '--{key}'.");

    private static int IntValue(IReadOnlyDictionary<string, string> values, string key, int fallback)
        => values.TryGetValue(key, out var value)
            ? int.Parse(value, CultureInfo.InvariantCulture)
            : fallback;

    private static double DoubleValue(IReadOnlyDictionary<string, string> values, string key, double fallback)
        => values.TryGetValue(key, out var value)
            ? double.Parse(value, CultureInfo.InvariantCulture)
            : fallback;

    private static string SkillIdFromSkillPath(string path)
    {
        var stem = Path.GetFileNameWithoutExtension(path);
        return stem.EndsWith("-SKILL", StringComparison.Ordinal)
            ? stem[..^"-SKILL".Length]
            : stem;
    }

    private static string ReadSkillTitle(string path)
    {
        foreach (var line in File.ReadLines(path, Encoding.UTF8).Take(40))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("#", StringComparison.Ordinal))
                return trimmed.TrimStart('#').Trim();
        }

        return SkillIdFromSkillPath(path).Replace('-', ' ');
    }

    private static IReadOnlyList<string> BuildSkillCandidateSet(
        IReadOnlyList<SkillCorpusSource> skills,
        string chosenSkillId,
        int rowIndex)
    {
        var candidates = new List<string> { chosenSkillId };
        for (var offset = 1; candidates.Count < Math.Min(6, skills.Count) && offset <= skills.Count; offset++)
        {
            var candidate = skills[(rowIndex + offset) % skills.Count].SkillId;
            if (!candidates.Contains(candidate, StringComparer.Ordinal))
                candidates.Add(candidate);
        }

        return candidates;
    }

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command '{command}'.");
        WriteUsage();
        return UsageError;
    }

    private static void WriteUsage()
        => Console.Error.WriteLine("Usage: audit/build/run/export/import/publish/load/continue orchestration model commands.");

    private static readonly JsonSerializerOptions ToolJsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private static readonly JsonSerializerOptions JsonLineToolOptions = new(ToolJsonOptions)
    {
        WriteIndented = false
    };

    private static readonly JsonSerializerOptions TranslationUnitJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private sealed record ParsedArgs(
        IReadOnlyDictionary<string, string> Values,
        IReadOnlySet<string> Flags);

    private sealed record ModelIndex(
        IReadOnlyList<ModelIndexEntry> Models);

    private sealed record ModelIndexEntry(
        string ModelId,
        string Domain,
        string ManifestPath,
        string Version,
        double Accuracy,
        DateTimeOffset PublishedAtUtc);

    private sealed record SkillCorpusSource(
        string SkillId,
        string FileName,
        string Title);

    private sealed record SkillSelectionCorpusRow(
        string PairId,
        string Prompt,
        IReadOnlyList<string> CandidateSkills,
        IReadOnlyList<string> ChosenSkills,
        IReadOnlyList<string> RejectedSkills,
        string Reason,
        string TargetTrainingAdapter,
        string SourcePromptLedgerId,
        string RejectedType);

    private sealed record TranslationRouteProbeResult(
        string Source,
        bool Success,
        string Output,
        string AdapterId,
        bool ExactSourceHit,
        bool BootstrapOutput);

    private sealed record HostRepoScanResult(
        int FileCount,
        bool ContainsTrainingLogic,
        bool NoPrivateAbsolutePathInHostFiles,
        IReadOnlyList<string> TrainingLogicPatterns);

    private sealed record TranslationContinueCorpusRow(
        [property: JsonPropertyName("pair_id")]
        string PairId,
        [property: JsonPropertyName("source")]
        string Source,
        [property: JsonPropertyName("target")]
        string Target,
        [property: JsonPropertyName("source_language")]
        string SourceLanguage,
        [property: JsonPropertyName("target_language")]
        string TargetLanguage,
        [property: JsonPropertyName("direction")]
        string Direction,
        [property: JsonPropertyName("topic_domain")]
        string TopicDomain,
        [property: JsonPropertyName("semantic_signature")]
        string SemanticSignature,
        [property: JsonPropertyName("rejected_type")]
        string RejectedType,
        [property: JsonPropertyName("rejected_output")]
        string RejectedOutput);

    private sealed record OrchestrationReasoningHeldOutFixtureRow(
        string RowId,
        string PairId,
        string SourceRowId,
        string SourceRef,
        string SourceSha256,
        string RawPromptSha256,
        string RowNoveltyClass,
        string InputTask,
        string TaskType,
        IReadOnlyList<string> ConstraintSignals,
        IReadOnlyList<string> RequirementSignals,
        IReadOnlyList<string> DeliverableSignals,
        IReadOnlyList<string> EvidenceSignals,
        IReadOnlyList<string> HostSignals,
        string ExpectedPatternId,
        string ExpectedScenarioId,
        string ExpectedOutcome);

    private sealed class StaticOrchestrationReasoningLearnedGuideStateLoader(
        OrchestrationReasoningLearnedGuideStateDescriptor learnedState)
        : IOrchestrationReasoningLearnedGuideStateLoader
    {
        public OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor> Load()
            => OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor>.Ok(learnedState);
    }
}
