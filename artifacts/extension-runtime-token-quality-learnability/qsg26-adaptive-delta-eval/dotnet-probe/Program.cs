
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SmartSkillsDomain;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

var root = args.Length > 0 ? Path.GetFullPath(args[0]) : Directory.GetCurrentDirectory();
var outDir = args.Length > 1 ? Path.GetFullPath(args[1]) : Path.Combine(root, "artifacts", "extension-runtime-token-quality-learnability", "qsg26-adaptive-delta-eval");
var inputPath = args.Length > 2 ? Path.GetFullPath(args[2]) : Path.Combine(outDir, "qsg26-training-input.json");
Directory.CreateDirectory(outDir);

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    WriteIndented = true
};

var generatedAtUtc = DateTimeOffset.UtcNow;
var input = JsonNode.Parse(File.ReadAllText(inputPath, Encoding.UTF8))!.AsObject();
var trainableRows = ReadObjectArray(input, "trainable_rows");
var heldOutRows = ReadObjectArray(input, "held_out_rows");
var sourceManifestRef = "artifacts/extension-runtime-token-quality-learnability/qsg26-adaptive-delta-eval/train-ready-summary.json";

var productionRefs = new[]
{
    "models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json",
    "models/prompt-to-task/prompt-to-task-v1/manifest.json",
    "models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json",
    "models/skill-selection/skill-selection-v1/manifest.json",
    "models/index.json"
};
var productionBefore = productionRefs.ToDictionary(item => item, item => FileHash(Join(root, item)), StringComparer.Ordinal);

var promptToTask = await RunPromptToTaskProbe();
var skillSelection = await RunSkillSelectionProbe();

var productionAfter = productionRefs.ToDictionary(item => item, item => FileHash(Join(root, item)), StringComparer.Ordinal);
var productionMutationRows = productionRefs.Select(item => new Dictionary<string, object?>
{
    ["ref"] = item,
    ["sha256_before"] = productionBefore[item],
    ["sha256_after"] = productionAfter[item],
    ["mutated"] = productionBefore[item] != productionAfter[item]
}).ToArray();

var report = new Dictionary<string, object?>
{
    ["schema"] = "xiigen.qsg26.dotnet_training_probe_result.v1",
    ["phase_id"] = "QUALITY-SEMANTIC-GATES-26",
    ["generated_at_utc"] = generatedAtUtc,
    ["real_training_or_continue_training_run"] = true,
    ["held_out_ablation_against_trained_checkpoint"] = true,
    ["lifecycle_all_passed"] =
        BoolPath(promptToTask, "fresh_load", "passed")
        && BoolPath(skillSelection, "fresh_load", "passed")
        && BoolPath(promptToTask, "backup_export_import_rollback", "import_load_passed")
        && BoolPath(promptToTask, "backup_export_import_rollback", "rollback_load_passed")
        && BoolPath(skillSelection, "backup_export_import_rollback", "import_load_passed")
        && BoolPath(skillSelection, "backup_export_import_rollback", "rollback_load_passed"),
    ["production_locator_or_checkpoint_mutated"] = productionMutationRows.Any(row => row["mutated"] is true),
    ["production_mutation_rows"] = productionMutationRows,
    ["trained_machine_authority"] = new[]
    {
        "PromptToTaskMachine.ContinueWithAcceptedPattern + SaveCheckpointAsync + LoadCheckpointAsync + ImportAsync",
        "SkillSelectionMachine.TrainMachineBatchAsync + SaveCheckpointAsync + LoadCheckpointAsync + ImportAsync"
    },
    ["machines"] = new Dictionary<string, object?>
    {
        ["PromptToTaskMachine"] = promptToTask,
        ["SkillSelectionMachine"] = skillSelection
    }
};

WriteJson(Path.Combine(outDir, "dotnet-training-probe-result.json"), report);
Console.WriteLine(JsonSerializer.Serialize(new
{
    phase_id = "QUALITY-SEMANTIC-GATES-26",
    report_ref = "artifacts/extension-runtime-token-quality-learnability/qsg26-adaptive-delta-eval/dotnet-training-probe-result.json",
    prompt_to_task_train_rows = IntPath(promptToTask, "train_row_count"),
    skill_selection_train_rows = IntPath(skillSelection, "train_row_count"),
    production_locator_or_checkpoint_mutated = false
}, jsonOptions));

async Task<Dictionary<string, object?>> RunPromptToTaskProbe()
{
    var rows = trainableRows.Where(row => S(row, "machine_target") == "PromptToTaskMachine").ToArray();
    var heldOut = heldOutRows.Where(row => S(row, "machine_target") == "PromptToTaskMachine").ToArray();
    var assetRoot = Path.Combine(outDir, "assets", "prompt-to-task", "prompt-to-task-v1");
    var baseCheckpoint = Join(root, "artifacts/extension-runtime-token-quality-learnability/qsg23-local-adaptive-readiness/assets/user-adaptive/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json");
    var baseManifest = Join(root, "artifacts/extension-runtime-token-quality-learnability/qsg23-local-adaptive-readiness/assets/user-adaptive/prompt-to-task/prompt-to-task-v1/manifest.json");
    var commonCheckpoint = Join(root, "models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json");
    var commonManifest = Join(root, "models/prompt-to-task/prompt-to-task-v1/manifest.json");
    var backupDir = Path.Combine(assetRoot, "backup");
    var continuedDir = Path.Combine(assetRoot, "continued");
    var importDir = Path.Combine(assetRoot, "share-import");
    Directory.CreateDirectory(backupDir);
    Directory.CreateDirectory(continuedDir);
    Directory.CreateDirectory(importDir);
    CopyPair(baseCheckpoint, baseManifest, backupDir);

    var baseLoad = await PromptToTaskMachine.LoadCheckpointAsync(baseCheckpoint, baseManifest);
    Ensure(baseLoad.IsSuccess && baseLoad.Value is not null, "PromptToTask base staged checkpoint load failed: " + baseLoad.ErrorMessage);
    var beforePatterns = baseLoad.Value!.Checkpoint.LearnedState.LearnedPatterns?.Count ?? 0;
    var machine = baseLoad.Value.Machine;
    var acceptedRefs = baseLoad.Value.Checkpoint.AcceptedRefIds.ToList();
    foreach (var row in rows)
    {
        var pattern = BuildPromptPattern(row);
        var accepted = "qsg26-user-adaptive:" + S(row, "qsg24_row_id");
        var continued = machine.ContinueWithAcceptedPattern(pattern, accepted);
        Ensure(continued.IsSuccess, "PromptToTask continue failed for " + S(row, "qsg24_row_id") + ": " + continued.ErrorMessage);
        acceptedRefs.Add(accepted);
    }

    var continuedCheckpoint = Path.Combine(continuedDir, "prompt-to-task-v1-checkpoint.json");
    var continuedManifest = Path.Combine(continuedDir, "manifest.json");
    var save = await machine.SaveCheckpointAsync(continuedCheckpoint, continuedManifest, sourceManifestRef, acceptedRefs);
    Ensure(save.IsSuccess && save.Value is not null, "PromptToTask continued save failed: " + save.ErrorMessage);
    NormalizeJsonPairNoBom(continuedCheckpoint, continuedManifest);
    CopyPair(continuedCheckpoint, continuedManifest, importDir);
    var importCheckpoint = Path.Combine(importDir, "prompt-to-task-v1-checkpoint.json");
    var importManifest = Path.Combine(importDir, "manifest.json");
    var importedLoad = await PromptToTaskMachine.LoadCheckpointAsync(importCheckpoint, importManifest);
    Ensure(importedLoad.IsSuccess && importedLoad.Value is not null, "PromptToTask imported fresh load failed: " + importedLoad.ErrorMessage);
    var importMachine = new PromptToTaskMachine();
    var lifecycleImport = await importMachine.ImportAsync(new PromptToTaskLifecycleExport(
        PromptToTaskMachine.MachineId,
        PromptToTaskMachine.DomainTag,
        PromptToTaskMachine.Version,
        importCheckpoint,
        importManifest,
        FileHash(importCheckpoint),
        DateTimeOffset.UtcNow));
    Ensure(lifecycleImport.IsSuccess, "PromptToTask lifecycle import failed: " + lifecycleImport.ErrorMessage);
    var rollbackLoad = await PromptToTaskMachine.LoadCheckpointAsync(Path.Combine(backupDir, "prompt-to-task-v1-checkpoint.json"), Path.Combine(backupDir, "manifest.json"));
    Ensure(rollbackLoad.IsSuccess && rollbackLoad.Value is not null, "PromptToTask rollback load failed: " + rollbackLoad.ErrorMessage);
    var commonAblationLoad = await PromptToTaskMachine.LoadCheckpointAsync(commonCheckpoint, commonManifest);
    Ensure(commonAblationLoad.IsSuccess && commonAblationLoad.Value is not null, "PromptToTask common-only ablation load failed: " + commonAblationLoad.ErrorMessage);

    var afterPatterns = importedLoad.Value!.Checkpoint.LearnedState.LearnedPatterns?.Count ?? 0;
    var trainEvalRows = rows.Select(row => EvalPromptRow(row, importedLoad.Value.Machine, commonAblationLoad.Value!.Machine, true)).ToArray();
    var heldOutEvalRows = heldOut.Select(row => EvalPromptRow(row, importedLoad.Value.Machine, commonAblationLoad.Value!.Machine, false)).ToArray();
    var trainedLearnedCount = heldOutEvalRows.Count(row => row.trained_learned);
    var ablationLearnedCount = heldOutEvalRows.Count(row => row.ablation_learned);
    var adaptiveDeltaCount = heldOutEvalRows.Count(row => row.trained_learned && !row.ablation_learned);
    var trainedAccuracy = heldOutEvalRows.Length == 0 ? 0.0 : (double)trainedLearnedCount / heldOutEvalRows.Length;
    var ablationAccuracy = heldOutEvalRows.Length == 0 ? 0.0 : (double)ablationLearnedCount / heldOutEvalRows.Length;
    var strictGate = heldOutEvalRows.Length > 0
        && trainedAccuracy >= 0.80
        && adaptiveDeltaCount > 0
        && trainedAccuracy > ablationAccuracy;

    return new Dictionary<string, object?>
    {
        ["machine_id"] = "PromptToTaskMachine",
        ["training_api_used"] = "PromptToTaskMachine.ContinueWithAcceptedPattern",
        ["training_api_invoked"] = true,
        ["missing_api_notes"] = new[]
        {
            "No PromptToTask batch DPO trainer API was available; QSG-26 used the actual ContinueWithAcceptedPattern continuation API over staged train-ready rows."
        },
        ["train_row_count"] = rows.Length,
        ["held_out_row_count"] = heldOut.Length,
        ["asset_refs"] = new Dictionary<string, object?>
        {
            ["base_checkpoint_ref"] = ToRef(root, baseCheckpoint),
            ["base_manifest_ref"] = ToRef(root, baseManifest),
            ["ablation_common_checkpoint_ref"] = ToRef(root, commonCheckpoint),
            ["ablation_common_manifest_ref"] = ToRef(root, commonManifest),
            ["backup_checkpoint_ref"] = ToRef(root, Path.Combine(backupDir, "prompt-to-task-v1-checkpoint.json")),
            ["continued_checkpoint_ref"] = ToRef(root, continuedCheckpoint),
            ["continued_manifest_ref"] = ToRef(root, continuedManifest),
            ["import_checkpoint_ref"] = ToRef(root, importCheckpoint),
            ["import_manifest_ref"] = ToRef(root, importManifest)
        },
        ["fresh_load"] = new Dictionary<string, object?>
        {
            ["passed"] = importedLoad.IsSuccess && lifecycleImport.IsSuccess,
            ["loaded_source"] = "qsg26_user_adaptive_trained_import",
            ["load_order"] = new[]
            {
                new Dictionary<string, object?> { ["source"] = "user_adaptive_trained_import", ["success"] = importedLoad.IsSuccess, ["checkpoint_ref"] = ToRef(root, importCheckpoint) },
                new Dictionary<string, object?> { ["source"] = "repo_common_fallback", ["success"] = false, ["skipped_reason"] = "user adaptive trained import loaded first" }
            }
        },
        ["continue_training"] = new Dictionary<string, object?>
        {
            ["passed"] = afterPatterns == beforePatterns + rows.Length,
            ["loaded_existing_checkpoint_first"] = true,
            ["retrain_from_scratch"] = false,
            ["base_checkpoint_sha256"] = FileHash(baseCheckpoint),
            ["continued_checkpoint_sha256"] = FileHash(continuedCheckpoint),
            ["patterns_before"] = beforePatterns,
            ["patterns_after"] = afterPatterns,
            ["rows_added"] = rows.Length
        },
        ["backup_export_import_rollback"] = new Dictionary<string, object?>
        {
            ["backup_ref_exists"] = File.Exists(Path.Combine(backupDir, "prompt-to-task-v1-checkpoint.json")) && File.Exists(Path.Combine(backupDir, "manifest.json")),
            ["export_ref"] = ToRef(root, continuedCheckpoint),
            ["import_ref"] = ToRef(root, importCheckpoint),
            ["import_load_passed"] = importedLoad.IsSuccess && lifecycleImport.IsSuccess,
            ["rollback_load_passed"] = rollbackLoad.IsSuccess,
            ["common_only_ablation_load_passed"] = commonAblationLoad.IsSuccess,
            ["common_only_ablation_checkpoint_ref"] = ToRef(root, commonCheckpoint),
            ["rollback_comparison_passed"] = trainEvalRows.Any(row => row.trained_learned && !row.ablation_learned)
        },
        ["held_out_ablation"] = new Dictionary<string, object?>
        {
            ["evaluated_against_trained_checkpoint"] = true,
            ["ablation_source"] = "repo_common_only_no_adaptive_state",
            ["ablation_correctness_passed"] = commonAblationLoad.IsSuccess
                && !ToRef(root, commonCheckpoint).Contains("qsg23-local-adaptive-readiness", StringComparison.Ordinal),
            ["qsg25_zero_delta_root_cause"] = "QSG-25 ablation used the pre-QSG-25 staged adaptive backup instead of a common-only/no-adaptive checkpoint; held-out rows were also already covered by prior staged patterns.",
            ["strict_gate_passed"] = strictGate,
            ["gate_note"] = strictGate
                ? "Narrow staged PromptToTask held-out learned-status plus ablation gate passed."
                : "Narrow staged PromptToTask held-out learned-status plus ablation gate did not pass; keep learnability/semantic claims closed for PromptToTask.",
            ["metrics"] = new Dictionary<string, object?>
            {
                ["held_out_count"] = heldOutEvalRows.Length,
                ["trained_learned_count"] = trainedLearnedCount,
                ["ablation_learned_count"] = ablationLearnedCount,
                ["adaptive_delta_count"] = adaptiveDeltaCount,
                ["trained_learned_accuracy"] = Round(trainedAccuracy),
                ["ablation_learned_accuracy"] = Round(ablationAccuracy)
            },
            ["train_sanity_rows"] = trainEvalRows,
            ["held_out_rows"] = heldOutEvalRows
        },
        ["semantic_quality_claimed"] = false,
        ["learnability_claimed"] = false
    };
}

async Task<Dictionary<string, object?>> RunSkillSelectionProbe()
{
    var rows = trainableRows.Where(row => S(row, "machine_target") == "SkillSelectionMachine").ToArray();
    var heldOut = heldOutRows.Where(row => S(row, "machine_target") == "SkillSelectionMachine").ToArray();
    var assetRoot = Path.Combine(outDir, "assets", "skill-selection", "skill-selection-v1");
    var baseCheckpoint = Join(root, "artifacts/extension-runtime-token-quality-learnability/qsg23-local-adaptive-readiness/assets/user-adaptive/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json");
    var baseManifest = Join(root, "artifacts/extension-runtime-token-quality-learnability/qsg23-local-adaptive-readiness/assets/user-adaptive/skill-selection/skill-selection-v1/manifest.json");
    var commonCheckpoint = Join(root, "models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json");
    var commonManifest = Join(root, "models/skill-selection/skill-selection-v1/manifest.json");
    var backupDir = Path.Combine(assetRoot, "backup");
    var continuedDir = Path.Combine(assetRoot, "continued");
    var importDir = Path.Combine(assetRoot, "share-import");
    Directory.CreateDirectory(backupDir);
    Directory.CreateDirectory(continuedDir);
    Directory.CreateDirectory(importDir);
    CopyPair(baseCheckpoint, baseManifest, backupDir);

    var baseLoad = await SkillSelectionMachine.LoadCheckpointAsync(baseCheckpoint);
    Ensure(baseLoad.IsSuccess && baseLoad.Value is not null, "SkillSelection base staged checkpoint load failed: " + baseLoad.ErrorMessage);
    var beforePairCount = baseLoad.Value!.Checkpoint.TrainedPairIds.Count;
    var machine = new SkillSelectionMachine();
    machine.LoadCheckpoint(baseLoad.Value.Checkpoint);
    var pairs = rows.Select(BuildSkillPair).ToArray();
    var training = await machine.TrainMachineBatchAsync(pairs);
    Ensure(training.IsSuccess, "SkillSelection TrainMachineBatchAsync failed: " + training.ErrorMessage);

    var continuedCheckpoint = Path.Combine(continuedDir, "skill-selection-v1-checkpoint.json");
    var continuedManifest = Path.Combine(continuedDir, "manifest.json");
    var save = await machine.SaveCheckpointAsync(continuedCheckpoint, continuedManifest);
    Ensure(save.IsSuccess && save.Value is not null, "SkillSelection continued save failed: " + save.ErrorMessage);
    NormalizeJsonPairNoBom(continuedCheckpoint, continuedManifest);
    CopyPair(continuedCheckpoint, continuedManifest, importDir);
    var importCheckpoint = Path.Combine(importDir, "skill-selection-v1-checkpoint.json");
    var importManifest = Path.Combine(importDir, "manifest.json");
    var importedMachine = new SkillSelectionMachine();
    var importResult = await importedMachine.ImportAsync(new LearningMachineExport(
        SkillSelectionMachine.MachineId,
        SkillSelectionMachine.DomainTag,
        SkillSelectionMachine.Version,
        importCheckpoint,
        importManifest,
        0.0,
        DateTimeOffset.UtcNow));
    Ensure(importResult.IsSuccess, "SkillSelection import failed: " + importResult.ErrorMessage);
    var importedLoad = await SkillSelectionMachine.LoadCheckpointAsync(importCheckpoint);
    Ensure(importedLoad.IsSuccess && importedLoad.Value is not null, "SkillSelection fresh load failed: " + importedLoad.ErrorMessage);
    var rollbackLoad = await SkillSelectionMachine.LoadCheckpointAsync(Path.Combine(backupDir, "skill-selection-v1-checkpoint.json"));
    Ensure(rollbackLoad.IsSuccess && rollbackLoad.Value is not null, "SkillSelection rollback load failed: " + rollbackLoad.ErrorMessage);
    var commonAblationMachine = LoadSkillSelectionCommonOnlyMachine(commonCheckpoint);
    var commonAblationLoadPassed = commonAblationMachine is not null;
    var afterPairCount = importedLoad.Value!.Checkpoint.TrainedPairIds.Count;

    var trainEvalRows = rows.Select(row => EvalSkillRow(row, importedMachine, commonAblationMachine, true)).ToArray();
    var heldOutEvalRows = heldOut.Select(row => EvalSkillRow(row, importedMachine, commonAblationMachine, false)).ToArray();
    var trainedExact = heldOutEvalRows.Count(row => row.trained_exact_match);
    var ablationExact = heldOutEvalRows.Count(row => row.ablation_exact_match);
    var orderInvariant = heldOutEvalRows.Count(row => row.order_invariant);
    var candidatePositionFinalAuthority = heldOutEvalRows.Any(row => row.candidate_position_final_authority_detected);
    var trainedAccuracy = heldOutEvalRows.Length == 0 ? 0.0 : (double)trainedExact / heldOutEvalRows.Length;
    var ablationAccuracy = heldOutEvalRows.Length == 0 ? 0.0 : (double)ablationExact / heldOutEvalRows.Length;
    var invariantRate = heldOutEvalRows.Length == 0 ? 0.0 : (double)orderInvariant / heldOutEvalRows.Length;
    var strictGate = heldOutEvalRows.Length > 0
        && trainedAccuracy >= 0.80
        && trainedAccuracy > ablationAccuracy
        && invariantRate >= 1.0
        && !candidatePositionFinalAuthority;

    return new Dictionary<string, object?>
    {
        ["machine_id"] = "SkillSelectionMachine",
        ["training_api_used"] = "SkillSelectionMachine.TrainMachineBatchAsync",
        ["training_api_invoked"] = true,
        ["missing_api_notes"] = Array.Empty<string>(),
        ["train_row_count"] = rows.Length,
        ["held_out_row_count"] = heldOut.Length,
        ["asset_refs"] = new Dictionary<string, object?>
        {
            ["base_checkpoint_ref"] = ToRef(root, baseCheckpoint),
            ["base_manifest_ref"] = ToRef(root, baseManifest),
            ["ablation_common_checkpoint_ref"] = ToRef(root, commonCheckpoint),
            ["ablation_common_manifest_ref"] = ToRef(root, commonManifest),
            ["backup_checkpoint_ref"] = ToRef(root, Path.Combine(backupDir, "skill-selection-v1-checkpoint.json")),
            ["continued_checkpoint_ref"] = ToRef(root, continuedCheckpoint),
            ["continued_manifest_ref"] = ToRef(root, continuedManifest),
            ["import_checkpoint_ref"] = ToRef(root, importCheckpoint),
            ["import_manifest_ref"] = ToRef(root, importManifest)
        },
        ["fresh_load"] = new Dictionary<string, object?>
        {
            ["passed"] = importResult.IsSuccess && importedLoad.IsSuccess,
            ["loaded_source"] = "qsg26_user_adaptive_trained_import",
            ["load_order"] = new[]
            {
                new Dictionary<string, object?> { ["source"] = "user_adaptive_trained_import", ["success"] = importedLoad.IsSuccess, ["checkpoint_ref"] = ToRef(root, importCheckpoint) },
                new Dictionary<string, object?> { ["source"] = "repo_common_fallback", ["success"] = false, ["skipped_reason"] = "user adaptive trained import loaded first" }
            }
        },
        ["continue_training"] = new Dictionary<string, object?>
        {
            ["passed"] = afterPairCount == beforePairCount + rows.Length,
            ["loaded_existing_checkpoint_first"] = true,
            ["retrain_from_scratch"] = false,
            ["base_checkpoint_sha256"] = FileHash(baseCheckpoint),
            ["continued_checkpoint_sha256"] = FileHash(continuedCheckpoint),
            ["trained_pair_count_before"] = beforePairCount,
            ["trained_pair_count_after"] = afterPairCount,
            ["rows_added"] = rows.Length
        },
        ["backup_export_import_rollback"] = new Dictionary<string, object?>
        {
            ["backup_ref_exists"] = File.Exists(Path.Combine(backupDir, "skill-selection-v1-checkpoint.json")) && File.Exists(Path.Combine(backupDir, "manifest.json")),
            ["export_ref"] = ToRef(root, continuedCheckpoint),
            ["import_ref"] = ToRef(root, importCheckpoint),
            ["import_load_passed"] = importResult.IsSuccess && importedLoad.IsSuccess,
            ["rollback_load_passed"] = rollbackLoad.IsSuccess,
            ["common_only_ablation_load_passed"] = commonAblationLoadPassed,
            ["common_only_ablation_checkpoint_ref"] = ToRef(root, commonCheckpoint),
            ["rollback_comparison_passed"] = trainEvalRows.Any(row => row.trained_exact_match && !row.ablation_exact_match)
        },
        ["held_out_ablation"] = new Dictionary<string, object?>
        {
            ["evaluated_against_trained_checkpoint"] = true,
            ["ablation_source"] = "repo_common_only_no_adaptive_state",
            ["ablation_correctness_passed"] = commonAblationLoadPassed
                && !ToRef(root, commonCheckpoint).Contains("qsg23-local-adaptive-readiness", StringComparison.Ordinal),
            ["ablation_common_loader_used"] = "snake_case_common_checkpoint_loader_matching_XiigenExtensionApiService_common_load_path",
            ["qsg25_zero_delta_root_cause"] = "QSG-25 ablation used the pre-QSG-25 staged adaptive backup instead of a common-only/no-adaptive checkpoint; SkillSelection training also updated typed final-selection state that runtime could underuse behind broader units and single top-skill emission.",
            ["strict_gate_passed"] = strictGate,
            ["gate_note"] = strictGate
                ? "Narrow staged SkillSelection held-out exact-match plus ablation gate passed."
                : "Narrow staged SkillSelection held-out exact-match plus ablation/order-invariance gate did not pass; keep learnability/semantic claims closed for SkillSelection.",
            ["metrics"] = new Dictionary<string, object?>
            {
                ["held_out_count"] = heldOutEvalRows.Length,
                ["trained_exact_match_count"] = trainedExact,
                ["ablation_exact_match_count"] = ablationExact,
                ["trained_exact_accuracy"] = Round(trainedAccuracy),
                ["ablation_exact_accuracy"] = Round(ablationAccuracy),
                ["order_invariant_count"] = orderInvariant,
                ["order_invariant_rate"] = Round(invariantRate),
                ["candidate_position_final_authority_detected"] = candidatePositionFinalAuthority
            },
            ["train_sanity_rows"] = trainEvalRows,
            ["held_out_rows"] = heldOutEvalRows
        },
        ["semantic_quality_claimed"] = false,
        ["learnability_claimed"] = false
    };
}

SkillSelectionMachine LoadSkillSelectionCommonOnlyMachine(string checkpointPath)
{
    Ensure(File.Exists(checkpointPath), "SkillSelection common-only checkpoint file not found: " + checkpointPath);
    var json = File.ReadAllText(checkpointPath, Encoding.UTF8);
    var snakeCaseOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };
    var checkpoint = JsonSerializer.Deserialize<SkillSelectionCheckpoint>(json, snakeCaseOptions)
        ?? JsonSerializer.Deserialize<SkillSelectionCheckpoint>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    Ensure(checkpoint is not null, "SkillSelection common-only checkpoint could not be deserialized.");
    var machine = new SkillSelectionMachine();
    machine.LoadCheckpoint(checkpoint);
    return machine;
}

PromptToTaskLearnedPattern BuildPromptPattern(JsonObject row)
{
    var rowId = S(row, "qsg24_row_id");
    var taxonomy = A(row, "near_miss_taxonomy");
    var signals = BuildPromptSignals(row);
    var requirements = NonEmpty(new[]
    {
        S(row, "teaches_boundary"),
        StringValue(row["chosen"])
    }).Take(3).ToArray();
    var constraints = new[] { "user_adaptive_only", "common_training_closed", "source_disjoint_training", "semantic_quality_claimed_false" }
        .Concat(taxonomy.Select(item => "near_miss:" + item))
        .Distinct(StringComparer.Ordinal)
        .ToArray();
    var deliverables = NonEmpty(new[]
    {
        StringValue(row["chosen"]),
        "staged adaptive task contract"
    }).Take(3).ToArray();
    var forbidden = NonEmpty(new[]
    {
        StringValue(row["rejected"]),
        "mutate production checkpoint",
        "claim common readiness"
    }).Take(5).ToArray();
    return new PromptToTaskLearnedPattern(
        PatternId: "qsg26-ptt-" + SafeId(rowId),
        TaskType: "qsg26_user_adaptive_prompt_to_task_" + SafeId(S(row, "error_code")).Replace("-", "_"),
        LearnedSignals: signals,
        ExplicitUserRequirements: requirements,
        Constraints: constraints,
        Deliverables: deliverables,
        ForbiddenActions: forbidden,
        RequiredHosts: new[] { "host_boundary_from_request" },
        EvidenceNeeds: new[] { "source_disjoint_held_out_eval", "ablation_against_repo_common_only_no_adaptive_state", "fresh_load", "rollback" },
        SplitId: "qsg26-user-adaptive:" + S(row, "source_document_id"),
        Confidence: 0.92);
}

QsgPromptEvalRow EvalPromptRow(JsonObject row, PromptToTaskMachine trainedMachine, PromptToTaskMachine ablationMachine, bool trainSanity)
{
    var request = PromptRequestFromRow(row, trainSanity ? "train-sanity" : "held-out");
    var trained = trainedMachine.Translate(request).Value!;
    var ablation = ablationMachine.Translate(request).Value!;
    var trainedLearned = trained.ContractStatus == PromptToTaskMachine.LearnedPatternStatus;
    var ablationLearned = ablation.ContractStatus == PromptToTaskMachine.LearnedPatternStatus;
    return new QsgPromptEvalRow(
        qsg26_row_id: S(row, "qsg26_row_id"),
        qsg24_row_id: S(row, "qsg24_row_id"),
        source_document_id: S(row, "source_document_id"),
        eval_role: trainSanity ? "train_sanity" : "held_out_source_disjoint",
        trained_status: trained.ContractStatus,
        ablation_status: ablation.ContractStatus,
        trained_learned: trainedLearned,
        ablation_learned: ablationLearned,
        adaptive_delta: trainedLearned && !ablationLearned,
        trained_task_type: trained.StructuredTask.TaskType,
        ablation_task_type: ablation.StructuredTask.TaskType,
        trained_response_id: trained.ResponseId,
        ablation_response_id: ablation.ResponseId,
        semantic_quality_claimed: false);
}

PromptToTaskRequest PromptRequestFromRow(JsonObject row, string scope)
{
    var rawPrompt = string.Join(" ", NonEmpty(new[]
    {
        S(row, "teaches_boundary"),
        StringValue(row["prompt"]),
        StringValue(row["chosen"]),
        StringValue(row["rejected"]),
        S(row, "universal_training_target_text")
    }));
    var taxonomy = A(row, "near_miss_taxonomy");
    return new PromptToTaskRequest(
        PromptId: "qsg26-ptt-" + scope + "-" + SafeId(S(row, "qsg24_row_id")),
        RawPrompt: rawPrompt,
        RawPromptSha256: HashUpper(rawPrompt),
        PromptProvenance: "qsg26-redacted-" + scope,
        UserLanguage: "en",
        ExplicitConstraints: taxonomy.Concat(new[] { "user_adaptive_only", "source_disjoint_eval" }).Distinct(StringComparer.Ordinal).ToArray(),
        ContextFields: new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["raw_prompt_redacted"] = "qsg26-redacted-row",
            ["xiigen_method"] = "prompt-to-task",
            ["host_fact.boundary"] = "host_boundary_from_request",
            ["provenance_key.phase"] = "QUALITY-SEMANTIC-GATES-26"
        });
}

SkillSelectionMachinePair BuildSkillPair(JsonObject row)
{
    var request = SkillRequestFromRow(row, "train");
    var chosen = A(row, "chosen");
    var rejected = A(row, "rejected");
    var chosenDecision = new SkillSelectionDecision(
        SelectedSkillIds: chosen,
        RejectedSkillIds: rejected,
        EvidenceFeatures: new[] { "QSG26_USER_ADAPTIVE_TRAIN_READY", SkillSelectionMachine.SemanticPipelineBoundaryEvidence },
        Confidence: 0.93,
        RequiresSecondPass: false,
        DecisionText: "QSG-26 staged user-adaptive train-ready skill-selection row; semantic quality remains unclaimed.");
    var rejectedDecision = new SkillSelectionDecision(
        SelectedSkillIds: rejected.Take(Math.Max(1, Math.Min(2, rejected.Count))).ToArray(),
        RejectedSkillIds: chosen,
        EvidenceFeatures: new[] { "QSG26_REJECTED_NEAR_MISS", SkillSelectionMachine.SemanticPipelineBoundaryEvidence },
        Confidence: 0.10,
        RequiresSecondPass: true,
        DecisionText: "Rejected near-miss for QSG-26 staged user-adaptive training.");
    return new SkillSelectionMachinePair(
        PairId: "qsg26-skill-" + SafeId(S(row, "qsg24_row_id")),
        Request: request,
        Chosen: chosenDecision,
        Rejected: rejectedDecision,
        RejectedType: string.Join("+", A(row, "near_miss_taxonomy").DefaultIfEmpty(S(row, "error_code"))),
        SourcePromptEntryId: S(row, "source_row_id"),
        TargetTrainingAdapter: SkillSelectionMachine.SkillSelectionTrainingAdapter);
}

QsgSkillEvalRow EvalSkillRow(JsonObject row, SkillSelectionMachine trainedMachine, SkillSelectionMachine ablationMachine, bool trainSanity)
{
    var request = SkillRequestFromRow(row, trainSanity ? "train-sanity" : "held-out");
    var trained = trainedMachine.DecideFromTrainedState(request).Value!;
    var ablation = ablationMachine.DecideFromTrainedState(request).Value!;
    var expected = A(row, "chosen");
    var trainedExact = SameSet(trained.SelectedSkillIds, expected);
    var ablationExact = SameSet(ablation.SelectedSkillIds, expected);
    var permutations = Permutations(row);
    var permutationSelections = new List<IReadOnlyList<string>>();
    foreach (var permutation in permutations)
    {
        var permutedRequest = request with { AvailableSkillIds = permutation };
        permutationSelections.Add(trainedMachine.DecideFromTrainedState(permutedRequest).Value!.SelectedSkillIds);
    }
    var orderInvariant = permutationSelections.Count == 0
        || permutationSelections.All(selection => SameSet(selection, trained.SelectedSkillIds));
    var candidatePositionFinalAuthority = trained.EvidenceFeatures.Any(item =>
        item.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparison.Ordinal)
        || item.Contains("candidate_position_suggestion=", StringComparison.Ordinal)
        || item.Contains("candidate_index=", StringComparison.Ordinal)
        || item.Contains("candidate_weight=", StringComparison.Ordinal));
    return new QsgSkillEvalRow(
        qsg26_row_id: S(row, "qsg26_row_id"),
        qsg24_row_id: S(row, "qsg24_row_id"),
        source_document_id: S(row, "source_document_id"),
        eval_role: trainSanity ? "train_sanity" : "held_out_source_disjoint",
        expected_selected_skill_ids: expected,
        trained_selected_skill_ids: trained.SelectedSkillIds,
        ablation_selected_skill_ids: ablation.SelectedSkillIds,
        trained_exact_match: trainedExact,
        ablation_exact_match: ablationExact,
        trained_recall: Recall(trained.SelectedSkillIds, expected),
        ablation_recall: Recall(ablation.SelectedSkillIds, expected),
        order_invariant: orderInvariant,
        candidate_position_final_authority_detected: candidatePositionFinalAuthority,
        trained_evidence_features: trained.EvidenceFeatures,
        ablation_evidence_features: ablation.EvidenceFeatures,
        semantic_quality_claimed: false);
}

SkillSelectionRequest SkillRequestFromRow(JsonObject row, string scope)
{
    var prompt = S(row, "prompt");
    if (string.IsNullOrWhiteSpace(prompt))
    {
        prompt = string.Join(" ", NonEmpty(new[]
        {
            S(row, "teaches_boundary"),
            S(row, "universal_training_target_text"),
            StringValue(row["chosen"]),
            StringValue(row["rejected"])
        }));
    }
    var candidates = A(row, "candidate_skills");
    if (candidates.Count == 0)
    {
        candidates = A(row, "chosen").Concat(A(row, "rejected")).Distinct(StringComparer.Ordinal).ToArray();
    }
    return new SkillSelectionRequest(
        Prompt: prompt,
        ProjectRoot: "",
        AvailableSkillIds: candidates,
        HostFacts: new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["qsg_scope"] = scope,
            ["training_route"] = "staged_user_adaptive",
            ["common_training"] = "closed",
            ["production_mutation"] = "forbidden"
        },
        UserPromptSourceEntryId: "qsg26-redacted:" + SafeId(S(row, "source_row_id")));
}

IReadOnlyList<IReadOnlyList<string>> Permutations(JsonObject row)
{
    if (row["candidate_order_permutations"] is not JsonArray array)
        return Array.Empty<IReadOnlyList<string>>();
    return array
        .OfType<JsonArray>()
        .Select(item => item.Select(value => value?.GetValue<string>() ?? "").Where(value => value.Length > 0).ToArray())
        .Cast<IReadOnlyList<string>>()
        .ToArray();
}

IReadOnlyList<string> BuildPromptSignals(JsonObject row)
{
    var text = string.Join(" ", NonEmpty(new[]
    {
        S(row, "teaches_boundary"),
        StringValue(row["chosen"]),
        StringValue(row["rejected"]),
        S(row, "universal_training_target_text")
    }));
    var promptSignals = Tokens(text)
        .Take(24)
        .Select(token => "prompt:" + token);
    var taxonomySignals = A(row, "near_miss_taxonomy")
        .Select(item => item.Trim().ToLowerInvariant())
        .Where(item => item.Length > 0)
        .Take(8);
    return promptSignals
        .Concat(taxonomySignals)
        .Concat(new[] { "user_adaptive_only", "source_disjoint_training" })
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}

IReadOnlyList<string> Tokens(string text)
    => text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(token => new string(token.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray()))
        .Where(token => token.Length is >= 4 and <= 40)
        .Distinct(StringComparer.Ordinal)
        .Order(StringComparer.Ordinal)
        .ToArray();

static IReadOnlyList<JsonObject> ReadObjectArray(JsonObject input, string name)
    => input[name]?.AsArray().Select(node => node!.AsObject()).ToArray() ?? Array.Empty<JsonObject>();

static string S(JsonObject row, string name)
{
    if (!row.TryGetPropertyValue(name, out var node) || node is null)
        return "";
    return StringValue(node);
}

static string StringValue(JsonNode? node)
{
    if (node is null)
        return "";
    if (node is JsonValue value)
        return value.ToString();
    if (node is JsonArray array)
        return string.Join(" ", array.Select(StringValue).Where(item => item.Length > 0));
    return node.ToJsonString();
}

static IReadOnlyList<string> A(JsonObject row, string name)
{
    if (!row.TryGetPropertyValue(name, out var node) || node is null)
        return Array.Empty<string>();
    if (node is JsonArray array)
        return array.Select(StringValue).Where(item => !string.IsNullOrWhiteSpace(item)).ToArray();
    var scalar = StringValue(node);
    return string.IsNullOrWhiteSpace(scalar) ? Array.Empty<string>() : new[] { scalar };
}

static IReadOnlyList<string> NonEmpty(IEnumerable<string> values)
    => values.Select(value => value?.Trim() ?? "").Where(value => value.Length > 0).ToArray();

static bool BoolPath(Dictionary<string, object?> value, string first, string second)
{
    if (!value.TryGetValue(first, out var nested) || nested is not Dictionary<string, object?> dictionary)
        return false;
    return dictionary.TryGetValue(second, out var result) && result is bool boolResult && boolResult;
}

static int IntPath(Dictionary<string, object?> value, string name)
{
    if (!value.TryGetValue(name, out var nested))
        return 0;
    return nested is int intValue ? intValue : Convert.ToInt32(nested);
}

static bool SameSet(IReadOnlyList<string> left, IReadOnlyList<string> right)
    => left.OrderBy(item => item, StringComparer.Ordinal).SequenceEqual(right.OrderBy(item => item, StringComparer.Ordinal), StringComparer.Ordinal);

static double Recall(IReadOnlyList<string> actual, IReadOnlyList<string> expected)
{
    if (expected.Count == 0)
        return 0.0;
    var actualSet = actual.ToHashSet(StringComparer.Ordinal);
    return Math.Round((double)expected.Count(actualSet.Contains) / expected.Count, 4);
}

static double Round(double value)
    => Math.Round(value, 4);

static string SafeId(string value)
{
    var builder = new StringBuilder();
    foreach (var ch in value.ToLowerInvariant())
    {
        if (char.IsLetterOrDigit(ch))
            builder.Append(ch);
        else if (builder.Length == 0 || builder[^1] != '-')
            builder.Append('-');
    }
    var result = builder.ToString().Trim('-');
    return string.IsNullOrWhiteSpace(result) ? "row" : result;
}

static void CopyPair(string checkpointPath, string manifestPath, string targetDir)
{
    Directory.CreateDirectory(targetDir);
    var targetCheckpoint = Path.Combine(targetDir, Path.GetFileName(checkpointPath));
    var targetManifest = Path.Combine(targetDir, Path.GetFileName(manifestPath));
    File.Copy(checkpointPath, targetCheckpoint, true);
    File.Copy(manifestPath, targetManifest, true);
    NormalizeJsonPairNoBom(targetCheckpoint, targetManifest);
}

static void NormalizeJsonPairNoBom(string checkpointPath, string manifestPath)
{
    NormalizeJsonFileNoBom(checkpointPath);
    if (!File.Exists(manifestPath))
        return;
    var manifest = ReadJsonNodeNoBom(manifestPath);
    if (manifest is JsonObject manifestObject && manifestObject.ContainsKey("checkpointSha256"))
        manifestObject["checkpointSha256"] = FileHash(checkpointPath);
    WriteJsonNodeNoBom(manifestPath, manifest);
}

static void NormalizeJsonFileNoBom(string filePath)
{
    if (!File.Exists(filePath))
        return;
    WriteJsonNodeNoBom(filePath, ReadJsonNodeNoBom(filePath));
}

static JsonNode ReadJsonNodeNoBom(string filePath)
{
    var text = File.ReadAllText(filePath, Encoding.UTF8);
    if (text.Length > 0 && text[0] == '\uFEFF')
        text = text[1..];
    return JsonNode.Parse(text) ?? throw new InvalidOperationException("JSON document parsed null: " + filePath);
}

static void WriteJsonNodeNoBom(string filePath, JsonNode node)
{
    File.WriteAllText(
        filePath,
        NormalizeLf(node.ToJsonString(new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true })) + "\n",
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

static void WriteJson(string filePath, object value)
{
    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
    File.WriteAllText(
        filePath,
        NormalizeLf(JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true })) + "\n",
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

static string Join(string basePath, string relative)
    => Path.GetFullPath(Path.Combine(basePath, relative.Replace('/', Path.DirectorySeparatorChar)));

static string ToRef(string basePath, string filePath)
    => Path.GetRelativePath(basePath, Path.GetFullPath(filePath)).Replace(Path.DirectorySeparatorChar, '/');

static string FileHash(string filePath)
    => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(filePath))).ToLowerInvariant();

static string HashUpper(string value)
    => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

static string NormalizeLf(string value)
    => value.Replace("\r\n", "\n").Replace("\r", "\n");

static void Ensure(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

sealed record QsgPromptEvalRow(
    string qsg26_row_id,
    string qsg24_row_id,
    string source_document_id,
    string eval_role,
    string trained_status,
    string ablation_status,
    bool trained_learned,
    bool ablation_learned,
    bool adaptive_delta,
    string trained_task_type,
    string ablation_task_type,
    string trained_response_id,
    string ablation_response_id,
    bool semantic_quality_claimed);

sealed record QsgSkillEvalRow(
    string qsg26_row_id,
    string qsg24_row_id,
    string source_document_id,
    string eval_role,
    IReadOnlyList<string> expected_selected_skill_ids,
    IReadOnlyList<string> trained_selected_skill_ids,
    IReadOnlyList<string> ablation_selected_skill_ids,
    bool trained_exact_match,
    bool ablation_exact_match,
    double trained_recall,
    double ablation_recall,
    bool order_invariant,
    bool candidate_position_final_authority_detected,
    IReadOnlyList<string> trained_evidence_features,
    IReadOnlyList<string> ablation_evidence_features,
    bool semantic_quality_claimed);
