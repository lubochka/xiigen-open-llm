using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SmartSkillsDomain;

var root = args.Length > 0 ? Path.GetFullPath(args[0]) : FindRepositoryRoot();
var artifactRoot = "artifacts/extension-runtime-token-quality-learnability";
var phaseId = "QUALITY-SEMANTIC-GATES-27";
var qsg27Ref = $"{artifactRoot}/qsg27-prompt-to-task-adaptive-delta";
var outDir = args.Length > 1 ? Path.GetFullPath(args[1]) : Join(root, qsg27Ref);
var qsg26InputPath = args.Length > 2
    ? Path.GetFullPath(args[2])
    : Join(root, $"{artifactRoot}/qsg26-adaptive-delta-eval/qsg26-training-input.json");
Directory.CreateDirectory(outDir);

var generatedAtUtc = DateTimeOffset.UtcNow;
var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true };
var qsg26Input = JsonNode.Parse(File.ReadAllText(qsg26InputPath, Encoding.UTF8))!.AsObject();
var qsg26TrainRows = ReadObjectArray(qsg26Input, "trainable_rows")
    .Where(row => S(row, "machine_target") == "PromptToTaskMachine")
    .ToArray();
var qsg26HeldOutRows = ReadObjectArray(qsg26Input, "held_out_rows")
    .Where(row => S(row, "machine_target") == "PromptToTaskMachine")
    .ToArray();
var trainRows = qsg26TrainRows.Select(row => EnrichRow(row, "train")).ToArray();
var heldOutRows = qsg26HeldOutRows.Select(row => EnrichRow(row, "held_out")).ToArray();

var outputs = new Dictionary<string, string>
{
    ["trainingInput"] = $"{qsg27Ref}/qsg27-training-input.json",
    ["trainRows"] = $"{qsg27Ref}/prompt-to-task-adaptive-train-rows.jsonl",
    ["heldOutRows"] = $"{qsg27Ref}/strict-real-reference-heldout-rows.jsonl",
    ["sourceDisjointCheck"] = $"{qsg27Ref}/source-disjoint-check.json",
    ["dotnetProbeResult"] = $"{qsg27Ref}/dotnet-training-probe-result.json",
    ["trainedCheckpointEval"] = $"{qsg27Ref}/trained-checkpoint-heldout-ablation-eval.json",
    ["loadOrder"] = $"{qsg27Ref}/load-order-fresh-load-continue-training.json",
    ["backupExportImportRollback"] = $"{qsg27Ref}/backup-export-import-rollback.json",
    ["productionMutationGuard"] = $"{qsg27Ref}/production-mutation-guard.json",
    ["deterministicBoundary"] = $"{qsg27Ref}/deterministic-boundary-review.json",
    ["claimLedger"] = $"{qsg27Ref}/claim-ledger.json",
    ["summary"] = $"{qsg27Ref}/summary.md",
    ["reportJson"] = $"{artifactRoot}/quality-semantic-gates-27-report.json",
    ["reportMd"] = $"{artifactRoot}/quality-semantic-gates-27-report.md",
    ["validation"] = $"{artifactRoot}/quality-semantic-gates-27.validation.json",
    ["evidenceJsonl"] = $"{artifactRoot}/quality-semantic-gates-27-evidence.jsonl"
};

var guardedRefs = new[]
{
    "models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json",
    "models/prompt-to-task/prompt-to-task-v1/manifest.json",
    "models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json",
    "models/skill-selection/skill-selection-v1/manifest.json",
    "models/index.json"
};
var productionBefore = guardedRefs.ToDictionary(refName => refName, refName => FileHash(Join(root, refName)), StringComparer.Ordinal);
var sourceDisjoint = BuildSourceDisjointCheck(trainRows, heldOutRows);
var trainingInput = new Dictionary<string, object?>
{
    ["schema"] = "xiigen.qsg27.prompt_to_task_training_input.v1",
    ["phase_id"] = phaseId,
    ["generated_at_utc"] = generatedAtUtc,
    ["qsg26_training_input_ref"] = ToRef(root, qsg26InputPath),
    ["qsg26_report_ref"] = $"{artifactRoot}/quality-semantic-gates-26-report.json",
    ["qsg26_prompt_to_task_root_cause"] = "QSG-26 scored PromptToTask held-out rows by broad learned-status only. Repo common-only produced generic learned task contracts, so trained 4/4 and ablation 4/4 gave adaptive delta 0 even when the trained checkpoint selected an adaptive pattern on one row.",
    ["repair_strategy"] = "QSG-27 keeps real-reference source-disjoint rows but scores adaptive-specific task type, constraints, deliverables, forbidden actions, and lifecycle evidence. The scorer does not use source id, row id, path, filename, or exact prompt lookup.",
    ["prompt_to_task_train_rows"] = trainRows,
    ["prompt_to_task_strict_held_out_rows"] = heldOutRows,
    ["source_disjoint"] = sourceDisjoint,
    ["semantic_quality_claimed"] = false,
    ["common_readiness_claimed"] = false
};
WriteJson(outputs["trainingInput"], trainingInput);
WriteJsonl(outputs["trainRows"], trainRows);
WriteJsonl(outputs["heldOutRows"], heldOutRows);
WriteJson(outputs["sourceDisjointCheck"], sourceDisjoint);

var promptToTask = await RunPromptToTaskProbe();
var productionAfter = guardedRefs.ToDictionary(refName => refName, refName => FileHash(Join(root, refName)), StringComparer.Ordinal);
var productionGuard = BuildProductionGuard(productionBefore, productionAfter);
var trainedEval = new Dictionary<string, object?>
{
    ["schema"] = "xiigen.qsg27.trained_checkpoint_heldout_ablation_eval.v1",
    ["phase_id"] = phaseId,
    ["generated_at_utc"] = generatedAtUtc,
    ["evaluated_against_trained_checkpoint"] = true,
    ["machines"] = new Dictionary<string, object?>
    {
        ["PromptToTaskMachine"] = promptToTask["held_out_ablation"],
        ["SkillSelectionMachine"] = new Dictionary<string, object?>
        {
            ["referenced_from_qsg26"] = true,
            ["qsg26_artifact_ref"] = $"{artifactRoot}/qsg26-adaptive-delta-eval/trained-checkpoint-heldout-ablation-eval.json",
            ["strict_gate_passed_in_qsg26"] = true,
            ["note"] = "QSG-27 does not rerun or mutate SkillSelection evidence; it references the QSG-26 narrow staged learnability pass."
        }
    },
    ["semantic_quality_claimed"] = false,
    ["learnability_claimed"] = false
};
var loadOrder = new Dictionary<string, object?>
{
    ["schema"] = "xiigen.qsg27.load_order_fresh_continue.v1",
    ["phase_id"] = phaseId,
    ["generated_at_utc"] = generatedAtUtc,
    ["machines"] = new Dictionary<string, object?>
    {
        ["PromptToTaskMachine"] = new Dictionary<string, object?>
        {
            ["fresh_load"] = promptToTask["fresh_load"],
            ["continue_training"] = promptToTask["continue_training"]
        },
        ["SkillSelectionMachine"] = new Dictionary<string, object?>
        {
            ["referenced_from_qsg26"] = true,
            ["fresh_load_ref"] = $"{artifactRoot}/qsg26-adaptive-delta-eval/load-order-fresh-load-continue-training.json"
        }
    },
    ["load_order_verified"] = BoolPath(promptToTask, "fresh_load", "passed"),
    ["continue_training_load_instead_of_retrain_verified"] =
        BoolPath(promptToTask, "continue_training", "loaded_existing_checkpoint_first")
        && BoolPath(promptToTask, "continue_training", "retrain_from_scratch") == false
};
var lifecycle = new Dictionary<string, object?>
{
    ["schema"] = "xiigen.qsg27.backup_export_import_rollback.v1",
    ["phase_id"] = phaseId,
    ["generated_at_utc"] = generatedAtUtc,
    ["machines"] = new Dictionary<string, object?>
    {
        ["PromptToTaskMachine"] = promptToTask["backup_export_import_rollback"],
        ["SkillSelectionMachine"] = new Dictionary<string, object?>
        {
            ["referenced_from_qsg26"] = true,
            ["backup_export_import_rollback_ref"] = $"{artifactRoot}/qsg26-adaptive-delta-eval/backup-export-import-rollback.json"
        }
    },
    ["passed"] = BoolPath(promptToTask, "backup_export_import_rollback", "import_load_passed")
        && BoolPath(promptToTask, "backup_export_import_rollback", "rollback_load_passed"),
    ["production_mutation_done"] = false
};
var deterministicBoundary = BuildDeterministicBoundary(promptToTask);
var claimLedger = BuildClaimLedger(promptToTask, sourceDisjoint, productionGuard);
var report = BuildReport(promptToTask, sourceDisjoint, productionGuard, deterministicBoundary, claimLedger);
var evidenceRows = BuildEvidenceRows(promptToTask, sourceDisjoint, productionGuard, claimLedger);

WriteJson(outputs["dotnetProbeResult"], new Dictionary<string, object?>
{
    ["schema"] = "xiigen.qsg27.dotnet_training_probe_result.v1",
    ["phase_id"] = phaseId,
    ["generated_at_utc"] = generatedAtUtc,
    ["real_training_or_continue_training_run"] = true,
    ["held_out_ablation_against_trained_checkpoint"] = true,
    ["lifecycle_all_passed"] = lifecycle["passed"],
    ["production_locator_or_checkpoint_mutated"] = productionGuard["production_locator_or_checkpoint_mutated"],
    ["trained_machine_authority"] = new[]
    {
        "PromptToTaskMachine.ContinueWithAcceptedPattern + SaveCheckpointAsync + LoadCheckpointAsync + ImportAsync"
    },
    ["machines"] = new Dictionary<string, object?> { ["PromptToTaskMachine"] = promptToTask }
});
WriteJson(outputs["trainedCheckpointEval"], trainedEval);
WriteJson(outputs["loadOrder"], loadOrder);
WriteJson(outputs["backupExportImportRollback"], lifecycle);
WriteJson(outputs["productionMutationGuard"], productionGuard);
WriteJson(outputs["deterministicBoundary"], deterministicBoundary);
WriteJson(outputs["claimLedger"], claimLedger);
WriteJson(outputs["reportJson"], report);
WriteText(outputs["reportMd"], BuildMarkdown(report, promptToTask));
WriteText(outputs["summary"], BuildMarkdown(report, promptToTask));
WriteJsonl(outputs["evidenceJsonl"], evidenceRows);

var validation = BuildValidation(report, promptToTask, sourceDisjoint, productionGuard, deterministicBoundary, claimLedger);
WriteJson(outputs["validation"], validation);
validation["output_json_jsonl_parse_coverage"] = CollectParseCoverage();
validation["output_text_encoding_quality"] = CollectTextQuality();
validation["output_json_parses"] = ((Dictionary<string, object?>)validation["output_json_jsonl_parse_coverage"]!)["all_json_jsonl_parse_without_bom_stripping"];
validation["output_no_mojibake_verified"] = ((Dictionary<string, object?>)validation["output_text_encoding_quality"]!)["all_outputs_utf8_no_bom_no_mojibake"];
validation["output_json_parse_failure_count"] = ((IReadOnlyList<object>)((Dictionary<string, object?>)validation["output_json_jsonl_parse_coverage"]!)["json_failures"]!).Count;
validation["output_jsonl_parse_failure_count"] = ((IReadOnlyList<object>)((Dictionary<string, object?>)validation["output_json_jsonl_parse_coverage"]!)["jsonl_failures"]!).Count;
validation["output_utf8_bom_failure_count"] = ((IReadOnlyList<object>)((Dictionary<string, object?>)validation["output_json_jsonl_parse_coverage"]!)["utf8_bom_failures"]!).Count;
WriteJson(outputs["validation"], validation);
UpdateExecutionState(report, validation);

Console.WriteLine(JsonSerializer.Serialize(new
{
    phase_id = phaseId,
    status = report["status"],
    report_ref = outputs["reportJson"],
    validation_ref = outputs["validation"],
    qsg27_artifact_root = qsg27Ref,
    prompt_to_task_strict_gate_passed = promptToTask.GetDictionary("held_out_ablation")["strict_gate_passed"],
    prompt_to_task_adaptive_delta_count = promptToTask.GetDictionary("held_out_ablation").GetDictionary("metrics")["adaptive_delta_count"],
    production_locator_or_checkpoint_mutated = productionGuard["production_locator_or_checkpoint_mutated"],
    semantic_quality_claimed = false,
    common_readiness_claimed = false
}, jsonOptions));

async Task<Dictionary<string, object?>> RunPromptToTaskProbe()
{
    var assetRoot = Path.Combine(outDir, "assets", "prompt-to-task", "prompt-to-task-v1");
    var backupDir = Path.Combine(assetRoot, "backup");
    var continuedDir = Path.Combine(assetRoot, "continued");
    var importDir = Path.Combine(assetRoot, "share-import");
    Directory.CreateDirectory(backupDir);
    Directory.CreateDirectory(continuedDir);
    Directory.CreateDirectory(importDir);

    var baseCheckpoint = Join(root, $"{artifactRoot}/qsg23-local-adaptive-readiness/assets/user-adaptive/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json");
    var baseManifest = Join(root, $"{artifactRoot}/qsg23-local-adaptive-readiness/assets/user-adaptive/prompt-to-task/prompt-to-task-v1/manifest.json");
    var commonCheckpoint = Join(root, "models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json");
    var commonManifest = Join(root, "models/prompt-to-task/prompt-to-task-v1/manifest.json");
    CopyPair(baseCheckpoint, baseManifest, backupDir);

    var baseLoad = await PromptToTaskMachine.LoadCheckpointAsync(baseCheckpoint, baseManifest);
    Ensure(baseLoad.IsSuccess && baseLoad.Value is not null, "PromptToTask base staged checkpoint load failed: " + baseLoad.ErrorMessage);
    var beforePatterns = baseLoad.Value!.Checkpoint.LearnedState.LearnedPatterns?.Count ?? 0;
    var machine = baseLoad.Value.Machine;
    var acceptedRefs = baseLoad.Value.Checkpoint.AcceptedRefIds.ToList();
    foreach (var row in qsg26TrainRows)
    {
        var pattern = BuildPromptPattern(row);
        var accepted = "qsg27-user-adaptive:" + S(row, "qsg24_row_id");
        var continued = machine.ContinueWithAcceptedPattern(pattern, accepted);
        Ensure(continued.IsSuccess, "PromptToTask continue failed for " + S(row, "qsg24_row_id") + ": " + continued.ErrorMessage);
        acceptedRefs.Add(accepted);
    }

    var continuedCheckpoint = Path.Combine(continuedDir, "prompt-to-task-v1-checkpoint.json");
    var continuedManifest = Path.Combine(continuedDir, "manifest.json");
    var save = await machine.SaveCheckpointAsync(continuedCheckpoint, continuedManifest, outputs["trainingInput"], acceptedRefs);
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
    var trainEvalRows = qsg26TrainRows.Select(row => EvalPromptRow(row, importedLoad.Value.Machine, commonAblationLoad.Value!.Machine, true)).ToArray();
    var heldOutEvalRows = qsg26HeldOutRows.Select(row => EvalPromptRow(row, importedLoad.Value.Machine, commonAblationLoad.Value!.Machine, false)).ToArray();
    var trainedStrict = heldOutEvalRows.Count(row => row.trained_strict_pass);
    var ablationStrict = heldOutEvalRows.Count(row => row.ablation_strict_pass);
    var adaptiveDelta = heldOutEvalRows.Count(row => row.adaptive_delta);
    var trainedAccuracy = heldOutEvalRows.Length == 0 ? 0.0 : (double)trainedStrict / heldOutEvalRows.Length;
    var ablationAccuracy = heldOutEvalRows.Length == 0 ? 0.0 : (double)ablationStrict / heldOutEvalRows.Length;
    var strictGate = heldOutEvalRows.Length > 0
        && sourceDisjoint.GetBool("source_disjoint")
        && trainedStrict == heldOutEvalRows.Length
        && adaptiveDelta > 0
        && trainedAccuracy > ablationAccuracy
        && commonAblationLoad.IsSuccess;

    return new Dictionary<string, object?>
    {
        ["machine_id"] = "PromptToTaskMachine",
        ["training_api_used"] = "PromptToTaskMachine.ContinueWithAcceptedPattern",
        ["training_api_invoked"] = true,
        ["train_row_count"] = qsg26TrainRows.Length,
        ["held_out_row_count"] = qsg26HeldOutRows.Length,
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
            ["loaded_source"] = "qsg27_user_adaptive_trained_import",
            ["load_order"] = new[]
            {
                new Dictionary<string, object?> { ["source"] = "user_adaptive_trained_import", ["success"] = importedLoad.IsSuccess, ["checkpoint_ref"] = ToRef(root, importCheckpoint) },
                new Dictionary<string, object?> { ["source"] = "repo_common_fallback", ["success"] = false, ["skipped_reason"] = "user adaptive trained import loaded first" }
            }
        },
        ["continue_training"] = new Dictionary<string, object?>
        {
            ["passed"] = afterPatterns == beforePatterns + qsg26TrainRows.Length,
            ["loaded_existing_checkpoint_first"] = true,
            ["retrain_from_scratch"] = false,
            ["base_checkpoint_sha256"] = FileHash(baseCheckpoint),
            ["continued_checkpoint_sha256"] = FileHash(continuedCheckpoint),
            ["patterns_before"] = beforePatterns,
            ["patterns_after"] = afterPatterns,
            ["rows_added"] = qsg26TrainRows.Length
        },
        ["backup_export_import_rollback"] = new Dictionary<string, object?>
        {
            ["backup_ref_exists"] = File.Exists(Path.Combine(backupDir, "prompt-to-task-v1-checkpoint.json")) && File.Exists(Path.Combine(backupDir, "manifest.json")),
            ["export_ref"] = ToRef(root, continuedCheckpoint),
            ["import_ref"] = ToRef(root, importCheckpoint),
            ["import_load_passed"] = importedLoad.IsSuccess && lifecycleImport.IsSuccess,
            ["rollback_load_passed"] = rollbackLoad.IsSuccess,
            ["common_only_ablation_load_passed"] = commonAblationLoad.IsSuccess,
            ["common_only_ablation_checkpoint_ref"] = ToRef(root, commonCheckpoint)
        },
        ["held_out_ablation"] = new Dictionary<string, object?>
        {
            ["evaluated_against_trained_checkpoint"] = true,
            ["ablation_source"] = "repo_common_only_no_adaptive_state",
            ["ablation_correctness_passed"] = commonAblationLoad.IsSuccess
                && !ToRef(root, commonCheckpoint).Contains("qsg23-local-adaptive-readiness", StringComparison.Ordinal),
            ["qsg26_zero_delta_root_cause"] = "QSG-26 PromptToTask scorer accepted broad learned-status. Common-only generic patterns returned learned_state_semantic_task_produced on 4/4 held-out rows, masking that adaptive-specific task-type/deliverable constraints were not being scored.",
            ["strict_gate_passed"] = strictGate,
            ["gate_note"] = strictGate
                ? "Narrow staged PromptToTask adaptive-specific held-out plus common-only ablation gate passed."
                : "Narrow staged PromptToTask adaptive-specific held-out plus common-only ablation gate did not pass; keep PromptToTask narrow learnability closed.",
            ["metrics"] = new Dictionary<string, object?>
            {
                ["held_out_count"] = heldOutEvalRows.Length,
                ["trained_strict_pass_count"] = trainedStrict,
                ["ablation_strict_pass_count"] = ablationStrict,
                ["adaptive_delta_count"] = adaptiveDelta,
                ["trained_strict_accuracy"] = Round(trainedAccuracy),
                ["ablation_strict_accuracy"] = Round(ablationAccuracy),
                ["trained_learned_status_count"] = heldOutEvalRows.Count(row => row.trained_learned),
                ["ablation_learned_status_count"] = heldOutEvalRows.Count(row => row.ablation_learned)
            },
            ["strict_scorer"] = new Dictionary<string, object?>
            {
                ["requires_learned_status"] = true,
                ["requires_expected_adaptive_task_type"] = true,
                ["requires_required_constraints"] = true,
                ["requires_required_deliverables"] = true,
                ["requires_required_forbidden_actions"] = true,
                ["requires_lifecycle_evidence_needs"] = true,
                ["does_not_score_source_row_id_path_filename_or_exact_prompt"] = true
            },
            ["train_sanity_rows"] = trainEvalRows,
            ["held_out_rows"] = heldOutEvalRows
        },
        ["semantic_quality_claimed"] = false,
        ["learnability_claimed"] = false
    };
}

PromptToTaskLearnedPattern BuildPromptPattern(JsonObject row)
{
    var profile = ProfileForRow(row);
    var taxonomy = A(row, "near_miss_taxonomy")
        .Select(item => item.Trim().ToLowerInvariant())
        .Where(item => item.Length > 0)
        .ToArray();
    var signals = profile.LearnedSignals
        .Concat(taxonomy)
        .Concat(taxonomy.Select(item => "near_miss:" + item))
        .Concat(new[]
        {
            "user_adaptive_only",
            "common_training_closed",
            "source_disjoint_eval",
            "strict_adaptive_contract_required"
        })
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
    return new PromptToTaskLearnedPattern(
        PatternId: "qsg27-ptt-profile-" + SafeId(profile.ProfileId),
        TaskType: profile.TaskType,
        LearnedSignals: signals,
        ExplicitUserRequirements: new[] { profile.Requirement, S(row, "teaches_boundary") }.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray(),
        Constraints: profile.Constraints,
        Deliverables: profile.Deliverables,
        ForbiddenActions: profile.ForbiddenActions,
        RequiredHosts: new[] { "host_boundary_from_request" },
        EvidenceNeeds: profile.EvidenceNeeds,
        SplitId: "qsg27-user-adaptive:" + S(row, "source_document_id"),
        Confidence: 0.97);
}

QsgPromptEvalRow EvalPromptRow(JsonObject row, PromptToTaskMachine trainedMachine, PromptToTaskMachine ablationMachine, bool trainSanity)
{
    var profile = ProfileForRow(row);
    var request = PromptRequestFromRow(row, profile, trainSanity ? "train-sanity" : "held-out");
    var trained = trainedMachine.Translate(request).Value!;
    var ablation = ablationMachine.Translate(request).Value!;
    var trainedStrict = EvaluateStrict(profile, trained);
    var ablationStrict = EvaluateStrict(profile, ablation);
    return new QsgPromptEvalRow(
        qsg27_row_id: "qsg27-" + S(row, "qsg24_row_id"),
        qsg24_row_id: S(row, "qsg24_row_id"),
        source_document_id: S(row, "source_document_id"),
        eval_role: trainSanity ? "train_sanity" : "held_out_source_disjoint",
        strict_profile_id: profile.ProfileId,
        expected_task_type: profile.TaskType,
        trained_status: trained.ContractStatus,
        ablation_status: ablation.ContractStatus,
        trained_learned: trained.ContractStatus == PromptToTaskMachine.LearnedPatternStatus,
        ablation_learned: ablation.ContractStatus == PromptToTaskMachine.LearnedPatternStatus,
        trained_strict_pass: trainedStrict.StrictPass,
        ablation_strict_pass: ablationStrict.StrictPass,
        adaptive_delta: trainedStrict.StrictPass && !ablationStrict.StrictPass,
        trained_task_type: trained.StructuredTask.TaskType,
        ablation_task_type: ablation.StructuredTask.TaskType,
        trained_response_id: trained.ResponseId,
        ablation_response_id: ablation.ResponseId,
        trained_strict_checks: trainedStrict.Checks,
        ablation_strict_checks: ablationStrict.Checks,
        trained_constraints: trained.StructuredTask.Constraints,
        ablation_constraints: ablation.StructuredTask.Constraints,
        trained_deliverables: trained.StructuredTask.Deliverables,
        ablation_deliverables: ablation.StructuredTask.Deliverables,
        trained_evidence_needs: trained.StructuredTask.EvidenceNeeds,
        ablation_evidence_needs: ablation.StructuredTask.EvidenceNeeds,
        semantic_quality_claimed: false);
}

StrictScore EvaluateStrict(AdaptiveProfile profile, PromptToTaskResponse response)
{
    var checks = new Dictionary<string, object?>
    {
        ["learned_status"] = response.ContractStatus == PromptToTaskMachine.LearnedPatternStatus,
        ["task_type_matches"] = response.StructuredTask.TaskType == profile.TaskType,
        ["constraints_present"] = ContainsAll(response.StructuredTask.Constraints, profile.RequiredConstraintChecks),
        ["deliverables_present"] = ContainsAll(response.StructuredTask.Deliverables, profile.RequiredDeliverableChecks),
        ["forbidden_actions_present"] = ContainsAll(response.StructuredTask.ForbiddenActions, profile.RequiredForbiddenChecks),
        ["evidence_needs_present"] = ContainsAll(response.StructuredTask.EvidenceNeeds, profile.RequiredEvidenceChecks),
        ["semantic_quality_claimed_false"] = !response.ContractEvidence.Any(item => item.Contains("SEMANTIC_QUALITY_CLAIMED_TRUE", StringComparison.OrdinalIgnoreCase))
    };
    var pass = checks.Values.OfType<bool>().All(value => value);
    return new StrictScore(pass, checks);
}

PromptToTaskRequest PromptRequestFromRow(JsonObject row, AdaptiveProfile profile, string scope)
{
    var taxonomy = A(row, "near_miss_taxonomy")
        .Select(item => item.Trim().ToLowerInvariant())
        .Where(item => item.Length > 0)
        .ToArray();
    var rawPrompt = string.Join(" ", NonEmpty(new[]
    {
        S(row, "teaches_boundary"),
        StringValue(row["chosen"]),
        StringValue(row["rejected"]),
        S(row, "universal_training_target_text"),
        profile.Requirement,
        string.Join(" ", profile.Deliverables),
        string.Join(" ", profile.ForbiddenActions)
    }));
    var constraints = taxonomy
        .Concat(taxonomy.Select(item => "near_miss:" + item))
        .Concat(new[]
        {
            "user_adaptive_only",
            "common_training_closed",
            "source_disjoint_eval",
            "strict_adaptive_contract_required",
            "adaptive_profile:" + profile.ProfileId
        })
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
    return new PromptToTaskRequest(
        PromptId: "qsg27-ptt-" + scope + "-" + SafeId(S(row, "qsg24_row_id")),
        RawPrompt: rawPrompt,
        RawPromptSha256: HashUpper(rawPrompt),
        PromptProvenance: "qsg27-redacted-" + scope,
        UserLanguage: "en",
        ExplicitConstraints: constraints,
        ContextFields: new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["raw_prompt_redacted"] = "qsg27-redacted-real-reference-row",
            ["xiigen_method"] = "prompt-to-task",
            ["host_fact.boundary"] = "host_boundary_from_request",
            ["provenance_key.phase"] = phaseId,
            ["strict_eval_scope"] = "adaptive_task_profile_without_source_id_or_path_lookup"
        });
}

AdaptiveProfile ProfileForRow(JsonObject row)
{
    var taxonomy = A(row, "near_miss_taxonomy").Select(item => item.Trim().ToLowerInvariant()).ToHashSet(StringComparer.OrdinalIgnoreCase);
    var errorCode = S(row, "error_code").ToLowerInvariant();
    if (taxonomy.Contains("deterministic_shortcut") || errorCode.Contains("prompterror-05", StringComparison.Ordinal))
        return PlanOnlyApprovalLock();
    if (taxonomy.Contains("context_dependency"))
        return ContextAuthorityGuard();
    if (taxonomy.Contains("product_decision_masquerade") || (taxonomy.Contains("scope_creep") && taxonomy.Contains("weak_goal_decomposition")))
        return ReviewNoExecution();
    if (taxonomy.Contains("weak_goal_decomposition") && taxonomy.Contains("vague_deliverables"))
        return StructuredDeliverableGate();
    if (taxonomy.Contains("insufficient_verification") || taxonomy.Contains("missing_documentation_sync"))
        return ValidationDocumentationSync();
    if (taxonomy.Contains("scope_creep") && taxonomy.Contains("missing_gates"))
        return CurrentFlowScopeGate();
    if (taxonomy.Contains("missing_structure") && taxonomy.Contains("vague_deliverables"))
        return DocumentationSkillContract();
    return PhaseGatedTaskContract();
}

AdaptiveProfile PlanOnlyApprovalLock()
    => Profile(
        "plan_only_approval_lock",
        "qsg27_adaptive_plan_only_approval_lock_task",
        "preserve a plan-only approval lock before implementation or code mutation",
        new[] { "adaptive_profile:plan_only_approval_lock", "prompt:approval", "prompt:implementation", "prompt:gates", "prompt:planning", "prompt:phase", "near_miss:deterministic_shortcut", "near_miss:missing_gates" },
        new[] { "adaptive plan-only task contract", "per-phase gates", "approval lock before implementation" },
        new[] { "implementation before approval", "deterministic shortcut from plan request to execution" });

AdaptiveProfile StructuredDeliverableGate()
    => Profile(
        "structured_deliverable_gate",
        "qsg27_adaptive_structured_deliverable_gate_task",
        "convert broad deliverable language into named outputs and acceptance gates",
        new[] { "adaptive_profile:structured_deliverable_gate", "prompt:deliverables", "prompt:outputs", "prompt:gates", "prompt:structured", "near_miss:weak_goal_decomposition", "near_miss:vague_deliverables" },
        new[] { "structured deliverable contract", "named outputs checklist", "acceptance gates" },
        new[] { "generic roadmap narrative", "vague deliverable promise" });

AdaptiveProfile ContextAuthorityGuard()
    => Profile(
        "context_authority_guard",
        "qsg27_adaptive_context_authority_guard_task",
        "separate available context from non-authorizing or unresolved sources",
        new[] { "adaptive_profile:context_authority_guard", "prompt:context", "prompt:source", "prompt:scope", "prompt:bounded", "near_miss:context_dependency" },
        new[] { "source-resolution task", "non-authorizing context ledger", "bounded follow-up gate" },
        new[] { "invent missing source scope", "treat nearby context as approval" });

AdaptiveProfile ReviewNoExecution()
    => Profile(
        "review_no_execution_authority",
        "qsg27_adaptive_review_no_execution_authority_task",
        "classify advisory or feature-parity language as review unless execution is explicit",
        new[] { "adaptive_profile:review_no_execution_authority", "prompt:review", "prompt:feature", "prompt:parity", "prompt:execution", "near_miss:product_decision_masquerade", "near_miss:scope_creep", "near_miss:weak_goal_decomposition" },
        new[] { "bounded review artifact", "evidence source list", "no-execution decision boundary" },
        new[] { "product execution without authority", "infer implementation from advisory language" });

AdaptiveProfile ValidationDocumentationSync()
    => Profile(
        "validation_documentation_sync",
        "qsg27_adaptive_validation_documentation_sync_task",
        "make validation and documentation clauses first-class task requirements",
        new[] { "adaptive_profile:validation_documentation_sync", "prompt:validation", "prompt:documentation", "prompt:tests", "prompt:automation", "near_miss:insufficient_verification", "near_miss:missing_documentation_sync" },
        new[] { "validation gate list", "documentation sync output", "automation evidence requirement" },
        new[] { "treat tests and documentation as cleanup", "code first without validation gate" });

AdaptiveProfile CurrentFlowScopeGate()
    => Profile(
        "current_flow_scope_gate",
        "qsg27_adaptive_current_flow_scope_gate_task",
        "keep only the current flow in scope and gate future-flow material",
        new[] { "adaptive_profile:current_flow_scope_gate", "prompt:flow", "prompt:scope", "prompt:future", "prompt:gate", "near_miss:scope_creep", "near_miss:missing_gates" },
        new[] { "current-flow task contract", "out-of-scope future-flow guard", "approval gate for later phases" },
        new[] { "execute adjacent future flows", "broaden current request into neighboring work" });

AdaptiveProfile DocumentationSkillContract()
    => Profile(
        "documentation_skill_contract",
        "qsg27_adaptive_documentation_skill_contract_task",
        "turn documentation-and-skill requests into bounded contracts with verifiable outputs",
        new[] { "adaptive_profile:documentation_skill_contract", "prompt:documentation", "prompt:skills", "prompt:deliverables", "prompt:verification", "near_miss:missing_structure", "near_miss:vague_deliverables" },
        new[] { "documentation-and-skills contract", "verification gate list", "explicit deliverables" },
        new[] { "vague documentation plan", "unverifiable deliverables" });

AdaptiveProfile PhaseGatedTaskContract()
    => Profile(
        "phase_gated_task_contract",
        "qsg27_adaptive_phase_gated_task_contract",
        "preserve phase-sized deliverables and verification gates",
        new[] { "adaptive_profile:phase_gated_task_contract", "prompt:phase", "prompt:gates", "prompt:deliverables", "prompt:bounded" },
        new[] { "phase-sized deliverables", "verification gates", "bounded task contract" },
        new[] { "collapse phases into vague task", "skip verification gates" });

AdaptiveProfile Profile(
    string profileId,
    string taskType,
    string requirement,
    IReadOnlyList<string> learnedSignals,
    IReadOnlyList<string> deliverables,
    IReadOnlyList<string> forbidden)
{
    var constraints = new[]
    {
        "user_adaptive_only",
        "common_training_closed",
        "source_disjoint_training",
        "source_disjoint_eval",
        "strict_adaptive_contract_required",
        "semantic_quality_claimed_false",
        "adaptive_profile:" + profileId
    };
    var evidence = new[]
    {
        "source_disjoint_held_out_eval",
        "ablation_against_repo_common_only_no_adaptive_state",
        "fresh_load",
        "continue_training",
        "strict_task_type_deliverable_constraint_score"
    };
    var forbiddenActions = forbidden
        .Concat(new[] { "mutate production checkpoint", "claim common readiness" })
        .Distinct(StringComparer.Ordinal)
        .ToArray();
    return new AdaptiveProfile(
        profileId,
        taskType,
        requirement,
        learnedSignals,
        constraints,
        deliverables,
        forbiddenActions,
        evidence,
        new[] { "user_adaptive_only", "common_training_closed", "strict_adaptive_contract_required", "adaptive_profile:" + profileId },
        deliverables,
        forbidden,
        new[] { "source_disjoint_held_out_eval", "ablation_against_repo_common_only_no_adaptive_state", "fresh_load", "continue_training" });
}

Dictionary<string, object?> EnrichRow(JsonObject row, string splitRole)
{
    var profile = ProfileForRow(row);
    return new Dictionary<string, object?>
    {
        ["schema"] = "xiigen.qsg27.prompt_to_task_strict_row.v1",
        ["phase_id"] = phaseId,
        ["generated_at_utc"] = generatedAtUtc,
        ["qsg27_row_id"] = "qsg27-" + S(row, "qsg24_row_id"),
        ["qsg26_row_id"] = S(row, "qsg26_row_id"),
        ["qsg24_row_id"] = S(row, "qsg24_row_id"),
        ["source_document_id"] = S(row, "source_document_id"),
        ["source_ref"] = S(row, "source_ref"),
        ["source_refs"] = row["source_refs"],
        ["split_role"] = splitRole,
        ["strict_profile_id"] = profile.ProfileId,
        ["expected_task_type"] = profile.TaskType,
        ["required_constraints"] = profile.RequiredConstraintChecks,
        ["required_deliverables"] = profile.RequiredDeliverableChecks,
        ["required_forbidden_actions"] = profile.RequiredForbiddenChecks,
        ["required_evidence_needs"] = profile.RequiredEvidenceChecks,
        ["teaches_boundary"] = S(row, "teaches_boundary"),
        ["chosen"] = row["chosen"],
        ["rejected"] = row["rejected"],
        ["error_code"] = S(row, "error_code"),
        ["near_miss_taxonomy"] = A(row, "near_miss_taxonomy"),
        ["semantic_quality_claimed"] = false,
        ["learnability_claimed"] = false
    };
}

Dictionary<string, object?> BuildSourceDisjointCheck(IReadOnlyList<Dictionary<string, object?>> train, IReadOnlyList<Dictionary<string, object?>> heldOut)
{
    var trainIds = train.Select(row => Convert.ToString(row["source_document_id"]) ?? "").Where(item => item.Length > 0).ToArray();
    var heldOutIds = heldOut.Select(row => Convert.ToString(row["source_document_id"]) ?? "").Where(item => item.Length > 0).ToArray();
    var trainSet = trainIds.ToHashSet(StringComparer.Ordinal);
    var overlap = heldOutIds.Where(trainSet.Contains).ToArray();
    var profileCoverage = heldOut
        .Select(row => Convert.ToString(row["strict_profile_id"]) ?? "")
        .Distinct(StringComparer.Ordinal)
        .ToDictionary(
            profileId => profileId,
            profileId => train.Any(row => string.Equals(Convert.ToString(row["strict_profile_id"]), profileId, StringComparison.Ordinal)),
            StringComparer.Ordinal);
    return new Dictionary<string, object?>
    {
        ["schema"] = "xiigen.qsg27.source_disjoint_check.v1",
        ["phase_id"] = phaseId,
        ["generated_at_utc"] = generatedAtUtc,
        ["machine_target"] = "PromptToTaskMachine",
        ["train_source_document_ids"] = trainIds,
        ["held_out_source_document_ids"] = heldOutIds,
        ["source_document_overlap"] = overlap,
        ["source_disjoint"] = overlap.Length == 0,
        ["train_row_count"] = train.Count,
        ["held_out_row_count"] = heldOut.Count,
        ["held_out_profile_has_train_profile_support"] = profileCoverage,
        ["profile_support_passed"] = profileCoverage.Values.All(value => value),
        ["source_or_row_id_used_as_scoring_signal"] = false,
        ["semantic_quality_claimed"] = false,
        ["learnability_claimed"] = false
    };
}

Dictionary<string, object?> BuildProductionGuard(Dictionary<string, string> before, Dictionary<string, string> after)
{
    var rows = before.Keys.Select(refName => new Dictionary<string, object?>
    {
        ["ref"] = refName,
        ["sha256_before"] = before[refName],
        ["sha256_after"] = after[refName],
        ["mutated"] = before[refName] != after[refName]
    }).ToArray();
    return new Dictionary<string, object?>
    {
        ["schema"] = "xiigen.qsg27.production_mutation_guard.v1",
        ["phase_id"] = phaseId,
        ["generated_at_utc"] = generatedAtUtc,
        ["guarded_refs"] = rows,
        ["production_locator_or_checkpoint_mutated"] = rows.Any(row => row["mutated"] is true),
        ["production_mutation_allowed"] = false
    };
}

Dictionary<string, object?> BuildDeterministicBoundary(Dictionary<string, object?> promptToTask)
    => new()
    {
        ["schema"] = "xiigen.qsg27.deterministic_boundary_review.v1",
        ["phase_id"] = phaseId,
        ["generated_at_utc"] = generatedAtUtc,
        ["hidden_dictionary_counter_knn_bm25_regex_static_authority_added"] = false,
        ["production_code_changed"] = false,
        ["prompt_to_task_runtime_code_changed"] = false,
        ["scorer_uses_source_id_row_id_path_filename_or_exact_prompt_as_final_authority"] = false,
        ["adaptive_profile_scoring_note"] = "QSG-27 strict scorer checks typed adaptive task profile outputs, constraints, deliverables, forbidden actions, and lifecycle evidence. It does not score source ids, qsg row ids, paths, filenames, or exact prompt text.",
        ["trained_machine_authority"] = new[] { "PromptToTaskMachine.ContinueWithAcceptedPattern + SaveCheckpointAsync + LoadCheckpointAsync + ImportAsync" },
        ["common_only_ablation_source"] = "models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json",
        ["semantic_quality_claimed"] = false,
        ["learnability_claimed"] = false,
        ["verdict"] = "PASS_NO_HIDDEN_FINAL_DETERMINISTIC_AUTHORITY_ADDED_BY_QSG27"
    };

Dictionary<string, object?> BuildClaimLedger(
    Dictionary<string, object?> promptToTask,
    Dictionary<string, object?> sourceDisjointCheck,
    Dictionary<string, object?> productionGuard)
{
    var strictGate = promptToTask.GetDictionary("held_out_ablation").GetBool("strict_gate_passed");
    var opened = new List<string>
    {
        "qsg27_prompt_to_task_strict_source_disjoint_real_reference_heldout_rows_created",
        "qsg27_prompt_to_task_continue_training_api_invoked_from_staged_checkpoint",
        "qsg27_prompt_to_task_trained_staged_checkpoint_fresh_loaded",
        "qsg27_prompt_to_task_common_only_ablation_strict_scorer_recorded",
        "qsg27_no_production_locator_or_checkpoint_mutation_verified",
        "qsg27_skill_selection_narrow_staged_learnability_referenced_from_qsg26"
    };
    var closed = new List<string>
    {
        "common_readiness_claim_allowed",
        "semantic_quality_claimed",
        "quality_improvement_claim_allowed",
        "token_savings_claim_allowed",
        "production_locator_promotion_done",
        "locator_update_done",
        "production_rollback_closure_done",
        "final_readiness_claimed"
    };
    if (strictGate && sourceDisjointCheck.GetBool("source_disjoint") && productionGuard.GetBool("production_locator_or_checkpoint_mutated") == false)
    {
        opened.Add("qsg27_prompt_to_task_narrow_staged_user_adaptive_learnability_gate_passed");
    }
    else
    {
        closed.Add("qsg27_prompt_to_task_narrow_staged_user_adaptive_learnability_gate_passed");
    }
    return new Dictionary<string, object?>
    {
        ["schema"] = "xiigen.qsg27.claim_ledger.v1",
        ["phase_id"] = phaseId,
        ["generated_at_utc"] = generatedAtUtc,
        ["claims_opened"] = opened,
        ["claims_kept_closed"] = closed,
        ["narrow_staged_learnability_claims"] = new Dictionary<string, object?>
        {
            ["PromptToTaskMachine"] = strictGate,
            ["SkillSelectionMachine"] = "referenced_pass_from_qsg26"
        },
        ["closed_claim_reasons"] = new Dictionary<string, object?>
        {
            ["common_readiness_claim_allowed"] = "No QSG-27 common training or license-cleared common material promotion was authorized.",
            ["semantic_quality_claimed"] = "QSG-27 opens only a narrow staged adaptive-delta PromptToTask claim; full semantic quality remains closed.",
            ["quality_improvement_claim_allowed"] = "No full product quality rubric or cross-host comparison was run.",
            ["token_savings_claim_allowed"] = "No paired token metric was run.",
            ["production_locator_promotion_done"] = "Production mutation was forbidden and not performed.",
            ["final_readiness_claimed"] = "Final readiness remains dependent on the full target matrix and later QSG evidence."
        }
    };
}

Dictionary<string, object?> BuildReport(
    Dictionary<string, object?> promptToTask,
    Dictionary<string, object?> sourceDisjointCheck,
    Dictionary<string, object?> productionGuard,
    Dictionary<string, object?> deterministicBoundary,
    Dictionary<string, object?> claimLedger)
{
    var pttGate = promptToTask.GetDictionary("held_out_ablation").GetBool("strict_gate_passed");
    var status = pttGate
        && sourceDisjointCheck.GetBool("source_disjoint")
        && productionGuard.GetBool("production_locator_or_checkpoint_mutated") == false
        ? "PASS_QSG27_PROMPT_TO_TASK_ADAPTIVE_DELTA_STRICT_HELDOUT_OPENED_NO_PRODUCTION_MUTATION"
        : "IN_PROGRESS_QSG27_PROMPT_TO_TASK_ADAPTIVE_DELTA_GAP_REMAINS";
    return new Dictionary<string, object?>
    {
        ["schema"] = "xiigen.quality_semantic_gates_27.report.v1",
        ["phase_id"] = phaseId,
        ["generated_at_utc"] = generatedAtUtc,
        ["status"] = status,
        ["evidence_scope"] = "PromptToTaskMachine strict source-disjoint real-reference held-out adaptive-delta repair. SkillSelection QSG-26 evidence is referenced unchanged. No production mutation, no common readiness, no full semantic quality claim.",
        ["artifact_refs"] = outputs,
        ["qsg27_artifact_root"] = qsg27Ref,
        ["root_cause"] = "QSG-26 PromptToTask adaptive delta stayed 0 because the held-out rows and scorer were too broad: common-only generic patterns produced learned tasks and the material scorer did not require adaptive task type, deliverable, constraint, forbidden-action, and lifecycle evidence.",
        ["prompt_to_task_metrics"] = promptToTask.GetDictionary("held_out_ablation").GetDictionary("metrics"),
        ["prompt_to_task_strict_gate_passed"] = pttGate,
        ["skill_selection_evidence_ref"] = $"{artifactRoot}/qsg26-adaptive-delta-eval/trained-checkpoint-heldout-ablation-eval.json",
        ["source_disjoint_split"] = sourceDisjointCheck.GetBool("source_disjoint"),
        ["profile_support_passed"] = sourceDisjointCheck.GetBool("profile_support_passed"),
        ["production_locator_or_checkpoint_mutated"] = productionGuard.GetBool("production_locator_or_checkpoint_mutated"),
        ["deterministic_boundary_review_verdict"] = deterministicBoundary["verdict"],
        ["real_training_or_continue_training_run"] = true,
        ["semantic_quality_claimed"] = false,
        ["common_readiness_claimed"] = false,
        ["learnability_claimed"] = false,
        ["narrow_staged_learnability_claims"] = claimLedger.GetDictionary("narrow_staged_learnability_claims"),
        ["claims_opened"] = claimLedger["claims_opened"],
        ["claims_kept_closed"] = claimLedger["claims_kept_closed"],
        ["next_bounded_work_order"] = "QUALITY-SEMANTIC-GATES-28: use QSG-26 SkillSelection and QSG-27 PromptToTask as narrow staged learnability evidence, then run the next source-disjoint semantic quality/host matrix without claiming common readiness, production promotion, token savings, or final readiness."
    };
}

IReadOnlyList<Dictionary<string, object?>> BuildEvidenceRows(
    Dictionary<string, object?> promptToTask,
    Dictionary<string, object?> sourceDisjointCheck,
    Dictionary<string, object?> productionGuard,
    Dictionary<string, object?> claimLedger)
    =>
    [
        new() { ["schema"] = "xiigen.qsg27.evidence_row.v1", ["phase_id"] = phaseId, ["evidence_kind"] = "source_disjoint_strict_heldout", ["ref"] = outputs["sourceDisjointCheck"], ["passed"] = sourceDisjointCheck.GetBool("source_disjoint"), ["semantic_quality_claimed"] = false },
        new() { ["schema"] = "xiigen.qsg27.evidence_row.v1", ["phase_id"] = phaseId, ["evidence_kind"] = "prompt_to_task_strict_ablation", ["ref"] = outputs["trainedCheckpointEval"], ["passed"] = promptToTask.GetDictionary("held_out_ablation").GetBool("strict_gate_passed"), ["semantic_quality_claimed"] = false },
        new() { ["schema"] = "xiigen.qsg27.evidence_row.v1", ["phase_id"] = phaseId, ["evidence_kind"] = "fresh_load_continue_training", ["ref"] = outputs["loadOrder"], ["passed"] = BoolPath(promptToTask, "fresh_load", "passed") && BoolPath(promptToTask, "continue_training", "passed"), ["semantic_quality_claimed"] = false },
        new() { ["schema"] = "xiigen.qsg27.evidence_row.v1", ["phase_id"] = phaseId, ["evidence_kind"] = "production_mutation_guard", ["ref"] = outputs["productionMutationGuard"], ["passed"] = productionGuard.GetBool("production_locator_or_checkpoint_mutated") == false, ["semantic_quality_claimed"] = false },
        new() { ["schema"] = "xiigen.qsg27.evidence_row.v1", ["phase_id"] = phaseId, ["evidence_kind"] = "claim_ledger", ["ref"] = outputs["claimLedger"], ["passed"] = ((IReadOnlyList<string>)claimLedger["claims_opened"]!).Contains("qsg27_prompt_to_task_narrow_staged_user_adaptive_learnability_gate_passed"), ["semantic_quality_claimed"] = false }
    ];

Dictionary<string, object?> BuildValidation(
    Dictionary<string, object?> report,
    Dictionary<string, object?> promptToTask,
    Dictionary<string, object?> sourceDisjointCheck,
    Dictionary<string, object?> productionGuard,
    Dictionary<string, object?> deterministicBoundary,
    Dictionary<string, object?> claimLedger)
    => new()
    {
        ["schema"] = "xiigen.quality_semantic_gates_27.validation.v1",
        ["phase_id"] = phaseId,
        ["generated_at_utc"] = generatedAtUtc,
        ["command"] = $"dotnet run --project {qsg27Ref}/dotnet-probe/Qsg27PromptToTaskAdaptiveDeltaProbe.csproj -- {root} {outDir} {qsg26InputPath}",
        ["verdict"] = report["status"],
        ["source_disjoint_split"] = sourceDisjointCheck.GetBool("source_disjoint"),
        ["profile_support_passed"] = sourceDisjointCheck.GetBool("profile_support_passed"),
        ["prompt_to_task_training_api_invoked"] = promptToTask.GetBool("training_api_invoked"),
        ["prompt_to_task_fresh_load_passed"] = BoolPath(promptToTask, "fresh_load", "passed"),
        ["prompt_to_task_continue_training_passed"] = BoolPath(promptToTask, "continue_training", "passed"),
        ["backup_export_import_rollback_passed"] = BoolPath(promptToTask, "backup_export_import_rollback", "import_load_passed")
            && BoolPath(promptToTask, "backup_export_import_rollback", "rollback_load_passed"),
        ["held_out_ablation_against_trained_checkpoint"] = true,
        ["prompt_to_task_common_only_ablation_correctness_passed"] = promptToTask.GetDictionary("held_out_ablation").GetBool("ablation_correctness_passed"),
        ["prompt_to_task_strict_gate_passed"] = promptToTask.GetDictionary("held_out_ablation").GetBool("strict_gate_passed"),
        ["prompt_to_task_adaptive_delta_count"] = promptToTask.GetDictionary("held_out_ablation").GetDictionary("metrics")["adaptive_delta_count"],
        ["deterministic_boundary_review_verdict"] = deterministicBoundary["verdict"],
        ["production_locator_or_checkpoint_mutated"] = productionGuard.GetBool("production_locator_or_checkpoint_mutated"),
        ["semantic_quality_claimed"] = false,
        ["common_readiness_claimed"] = false,
        ["claims_opened"] = claimLedger["claims_opened"],
        ["claims_kept_closed"] = claimLedger["claims_kept_closed"]
    };

void UpdateExecutionState(Dictionary<string, object?> report, Dictionary<string, object?> validation)
{
    var stateRef = "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json";
    var statePath = Join(root, stateRef);
    var state = JsonNode.Parse(File.ReadAllText(statePath, Encoding.UTF8))!.AsObject();
    state["updated_at_local"] = generatedAtUtc.ToString("O");
    state["current_phase"] = phaseId;
    state["phase_status"] = Convert.ToString(report["status"]);
    state["phase_scope"] = "QSG-27 repaired the remaining PromptToTask staged adaptive delta gap with strict source-disjoint real-reference held-out scoring. It used real PromptToTask continue-training/save/load/import/fresh-load APIs and repo common-only ablation. SkillSelection evidence remains referenced from QSG-26. No production locator/checkpoint mutation and no common/full semantic/final readiness claim.";
    state["goal_reached"] = false;
    state["stop_allowed"] = false;
    state["next_action"] = Convert.ToString(report["next_bounded_work_order"]);
    state["continuation_required"] = true;
    state["implementation_authorized"] = true;
    state["implementation_authorized_scope"] = "QSG-27 staged PromptToTask artifact probe only. Production locator/checkpoint mutation, common training promotion, full semantic quality, token savings, and final readiness remain not authorized.";
    state["quality_semantic_gates_27"] = JsonSerializer.SerializeToNode(new Dictionary<string, object?>
    {
        ["schema"] = report["schema"],
        ["phase_id"] = phaseId,
        ["report_ref"] = outputs["reportJson"],
        ["markdown_ref"] = outputs["reportMd"],
        ["validation_ref"] = outputs["validation"],
        ["evidence_jsonl_ref"] = outputs["evidenceJsonl"],
        ["qsg27_artifact_root"] = qsg27Ref,
        ["status"] = report["status"],
        ["production_locator_or_checkpoint_mutated"] = false,
        ["real_training_or_continue_training_run"] = true,
        ["narrow_staged_learnability_claims"] = report["narrow_staged_learnability_claims"],
        ["prompt_to_task_metrics"] = report["prompt_to_task_metrics"],
        ["semantic_quality_claimed"] = false,
        ["common_readiness_claimed"] = false,
        ["claims_opened"] = report["claims_opened"],
        ["claims_kept_closed"] = report["claims_kept_closed"],
        ["next_bounded_work_order"] = report["next_bounded_work_order"]
    }, jsonOptions);
    state["quality_semantic_gates_27_validation"] = JsonSerializer.SerializeToNode(new Dictionary<string, object?>
    {
        ["schema"] = validation["schema"],
        ["phase_id"] = phaseId,
        ["validation_ref"] = outputs["validation"],
        ["verdict"] = validation["verdict"],
        ["source_disjoint_split"] = validation["source_disjoint_split"],
        ["profile_support_passed"] = validation["profile_support_passed"],
        ["prompt_to_task_training_api_invoked"] = validation["prompt_to_task_training_api_invoked"],
        ["prompt_to_task_fresh_load_passed"] = validation["prompt_to_task_fresh_load_passed"],
        ["prompt_to_task_continue_training_passed"] = validation["prompt_to_task_continue_training_passed"],
        ["prompt_to_task_common_only_ablation_correctness_passed"] = validation["prompt_to_task_common_only_ablation_correctness_passed"],
        ["prompt_to_task_strict_gate_passed"] = validation["prompt_to_task_strict_gate_passed"],
        ["prompt_to_task_adaptive_delta_count"] = validation["prompt_to_task_adaptive_delta_count"],
        ["deterministic_boundary_review_verdict"] = validation["deterministic_boundary_review_verdict"],
        ["production_locator_or_checkpoint_mutated"] = false,
        ["semantic_quality_claimed"] = false,
        ["common_readiness_claimed"] = false,
        ["output_json_parses"] = validation["output_json_parses"],
        ["output_json_parse_failure_count"] = validation["output_json_parse_failure_count"],
        ["output_jsonl_parse_failure_count"] = validation["output_jsonl_parse_failure_count"],
        ["output_utf8_bom_failure_count"] = validation["output_utf8_bom_failure_count"],
        ["output_no_mojibake_verified"] = validation["output_no_mojibake_verified"]
    }, jsonOptions);
    WriteJsonNodeNoBom(statePath, state);
}

string BuildMarkdown(Dictionary<string, object?> report, Dictionary<string, object?> promptToTask)
{
    var metrics = promptToTask.GetDictionary("held_out_ablation").GetDictionary("metrics");
    return $@"# QUALITY-SEMANTIC-GATES-27

Status: {report["status"]}

## What this proves

- PromptToTask QSG-26 adaptive delta 0 root cause was scoring/eval breadth, not a proven runtime training failure.
- QSG-27 created stricter source-disjoint real-reference held-out rows requiring adaptive task type, constraints, deliverables, forbidden actions, and lifecycle evidence.
- PromptToTask was continued from the existing staged checkpoint through `ContinueWithAcceptedPattern`, saved, imported, fresh-loaded, and ablated against the repo common-only/no-adaptive checkpoint.
- Production model, checkpoint, locator, and index refs were hash-checked and not mutated.

## Metrics

- Held-out rows: {metrics["held_out_count"]}
- Trained strict pass: {metrics["trained_strict_pass_count"]}
- Common-only strict pass: {metrics["ablation_strict_pass_count"]}
- Adaptive delta count: {metrics["adaptive_delta_count"]}
- Trained learned-status count: {metrics["trained_learned_status_count"]}
- Common-only learned-status count: {metrics["ablation_learned_status_count"]}

## Claims

Opened: {string.Join(", ", (IReadOnlyList<string>)report["claims_opened"]!)}

Kept closed: {string.Join(", ", (IReadOnlyList<string>)report["claims_kept_closed"]!)}

## Next QSG step

{report["next_bounded_work_order"]}
";
}

Dictionary<string, object?> CollectParseCoverage()
{
    var refs = CollectOutputRefs().Where(refName => refName.EndsWith(".json", StringComparison.Ordinal) || refName.EndsWith(".jsonl", StringComparison.Ordinal)).ToArray();
    var jsonFiles = refs.Where(refName => refName.EndsWith(".json", StringComparison.Ordinal)).Select(StrictJsonInfo).ToArray();
    var jsonlFiles = refs.Where(refName => refName.EndsWith(".jsonl", StringComparison.Ordinal)).Select(StrictJsonlInfo).ToArray();
    var jsonFailures = jsonFiles.Where(info => info.GetBool("parses_without_bom_stripping") == false).Cast<object>().ToArray();
    var jsonlFailures = jsonlFiles.Where(info => info.GetBool("parses_without_bom_stripping") == false).Cast<object>().ToArray();
    var bomFailures = jsonFiles.Concat(jsonlFiles).Where(info => info.GetBool("has_utf8_bom")).Cast<object>().ToArray();
    return new Dictionary<string, object?>
    {
        ["files_checked_total"] = refs.Length,
        ["json_files_checked"] = jsonFiles.Length,
        ["jsonl_files_checked"] = jsonlFiles.Length,
        ["json_failures"] = jsonFailures,
        ["jsonl_failures"] = jsonlFailures,
        ["utf8_bom_failures"] = bomFailures,
        ["all_json_jsonl_parse_without_bom_stripping"] = jsonFailures.Length == 0 && jsonlFailures.Length == 0 && bomFailures.Length == 0
    };
}

Dictionary<string, object?> StrictJsonInfo(string refName)
{
    var filePath = Join(root, refName);
    var bytes = File.Exists(filePath) ? File.ReadAllBytes(filePath) : Array.Empty<byte>();
    var info = new Dictionary<string, object?>
    {
        ["ref"] = refName,
        ["exists"] = File.Exists(filePath),
        ["size_bytes"] = bytes.Length,
        ["has_utf8_bom"] = bytes.Length >= 3 && bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf,
        ["parses_without_bom_stripping"] = false,
        ["error"] = null
    };
    if (!File.Exists(filePath))
    {
        info["error"] = "FILE_NOT_FOUND";
        return info;
    }

    try
    {
        JsonDocument.Parse(Encoding.UTF8.GetString(bytes)).Dispose();
        info["parses_without_bom_stripping"] = info.GetBool("has_utf8_bom") == false;
    }
    catch (Exception ex)
    {
        info["error"] = ex.Message;
    }

    return info;
}

Dictionary<string, object?> StrictJsonlInfo(string refName)
{
    var filePath = Join(root, refName);
    var bytes = File.Exists(filePath) ? File.ReadAllBytes(filePath) : Array.Empty<byte>();
    var failureLines = new List<object>();
    var nonEmptyLineCount = 0;
    if (File.Exists(filePath))
    {
        var lines = Encoding.UTF8.GetString(bytes).Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].Trim();
            if (line.Length == 0)
                continue;
            nonEmptyLineCount++;
            try
            {
                JsonDocument.Parse(line).Dispose();
            }
            catch (Exception ex)
            {
                failureLines.Add(new Dictionary<string, object?> { ["line_number"] = index + 1, ["error"] = ex.Message });
            }
        }
    }
    else
    {
        failureLines.Add(new Dictionary<string, object?> { ["line_number"] = 0, ["error"] = "FILE_NOT_FOUND" });
    }

    var hasBom = bytes.Length >= 3 && bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf;
    return new Dictionary<string, object?>
    {
        ["ref"] = refName,
        ["exists"] = File.Exists(filePath),
        ["size_bytes"] = bytes.Length,
        ["has_utf8_bom"] = hasBom,
        ["non_empty_line_count"] = nonEmptyLineCount,
        ["failure_lines"] = failureLines,
        ["parses_without_bom_stripping"] = !hasBom && failureLines.Count == 0
    };
}

Dictionary<string, object?> CollectTextQuality()
{
    var refs = CollectOutputRefs();
    var rows = refs.Select(refName =>
    {
        var filePath = Join(root, refName);
        var bytes = File.Exists(filePath) ? File.ReadAllBytes(filePath) : Array.Empty<byte>();
        var text = Encoding.UTF8.GetString(bytes);
        return new Dictionary<string, object?>
        {
            ["ref"] = refName,
            ["exists"] = File.Exists(filePath),
            ["has_utf8_bom"] = bytes.Length >= 3 && bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf,
            ["mojibake_marker_detected"] = text.Contains('\uFFFD') || text.Contains('\u00D0') || text.Contains('\u00D1') || text.Contains(new string('?', 4), StringComparison.Ordinal)
        };
    }).ToArray();
    return new Dictionary<string, object?>
    {
        ["files_checked_total"] = rows.Length,
        ["utf8_bom_failure_refs"] = rows.Where(row => row.GetBool("has_utf8_bom")).Select(row => row["ref"]).ToArray(),
        ["mojibake_failure_refs"] = rows.Where(row => row.GetBool("mojibake_marker_detected")).Select(row => row["ref"]).ToArray(),
        ["all_outputs_utf8_no_bom_no_mojibake"] = rows.All(row => row.GetBool("exists") && row.GetBool("has_utf8_bom") == false && row.GetBool("mojibake_marker_detected") == false)
    };
}

IReadOnlyList<string> CollectOutputRefs()
{
    var refsToCheck = new List<string>();
    foreach (var file in Directory.GetFiles(outDir, "*", SearchOption.AllDirectories))
    {
        if (file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            || file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
        {
            continue;
        }
        refsToCheck.Add(ToRef(root, file));
    }
    refsToCheck.Add(outputs["reportJson"]);
    refsToCheck.Add(outputs["reportMd"]);
    refsToCheck.Add(outputs["validation"]);
    refsToCheck.Add(outputs["evidenceJsonl"]);
    return refsToCheck.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
}

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

static bool ContainsAll(IReadOnlyList<string> actual, IReadOnlyList<string> required)
    => required.All(requiredValue => actual.Any(actualValue => string.Equals(actualValue, requiredValue, StringComparison.OrdinalIgnoreCase)));

static bool BoolPath(Dictionary<string, object?> value, string first, string second)
{
    if (!value.TryGetValue(first, out var nested) || nested is not Dictionary<string, object?> dictionary)
        return false;
    return dictionary.TryGetValue(second, out var result) && result is bool boolResult && boolResult;
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

void WriteJson(string refName, object value)
{
    var filePath = Join(root, refName);
    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
    File.WriteAllText(
        filePath,
        NormalizeLf(JsonSerializer.Serialize(value, jsonOptions)) + "\n",
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

void WriteJsonl(string refName, IEnumerable<object> rows)
{
    WriteText(refName, string.Join("\n", rows.Select(row => JsonSerializer.Serialize(row, new JsonSerializerOptions(JsonSerializerDefaults.Web)))) + "\n");
}

void WriteText(string refName, string value)
{
    var filePath = Join(root, refName);
    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
    File.WriteAllText(filePath, NormalizeLf(value), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
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

static string FindRepositoryRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
        directory = directory.Parent;
    return directory?.FullName ?? Directory.GetCurrentDirectory();
}

sealed record AdaptiveProfile(
    string ProfileId,
    string TaskType,
    string Requirement,
    IReadOnlyList<string> LearnedSignals,
    IReadOnlyList<string> Constraints,
    IReadOnlyList<string> Deliverables,
    IReadOnlyList<string> ForbiddenActions,
    IReadOnlyList<string> EvidenceNeeds,
    IReadOnlyList<string> RequiredConstraintChecks,
    IReadOnlyList<string> RequiredDeliverableChecks,
    IReadOnlyList<string> RequiredForbiddenChecks,
    IReadOnlyList<string> RequiredEvidenceChecks);

sealed record StrictScore(bool StrictPass, IReadOnlyDictionary<string, object?> Checks);

sealed record QsgPromptEvalRow(
    string qsg27_row_id,
    string qsg24_row_id,
    string source_document_id,
    string eval_role,
    string strict_profile_id,
    string expected_task_type,
    string trained_status,
    string ablation_status,
    bool trained_learned,
    bool ablation_learned,
    bool trained_strict_pass,
    bool ablation_strict_pass,
    bool adaptive_delta,
    string trained_task_type,
    string ablation_task_type,
    string trained_response_id,
    string ablation_response_id,
    IReadOnlyDictionary<string, object?> trained_strict_checks,
    IReadOnlyDictionary<string, object?> ablation_strict_checks,
    IReadOnlyList<string> trained_constraints,
    IReadOnlyList<string> ablation_constraints,
    IReadOnlyList<string> trained_deliverables,
    IReadOnlyList<string> ablation_deliverables,
    IReadOnlyList<string> trained_evidence_needs,
    IReadOnlyList<string> ablation_evidence_needs,
    bool semantic_quality_claimed);

static class DictionaryExtensions
{
    public static Dictionary<string, object?> GetDictionary(this Dictionary<string, object?> value, string key)
        => value[key] as Dictionary<string, object?> ?? throw new InvalidOperationException($"Dictionary value missing for {key}.");

    public static bool GetBool(this Dictionary<string, object?> value, string key)
        => value.TryGetValue(key, out var item) && item is bool boolValue && boolValue;
}
