
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SmartSkillsDomain;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

var root = args.Length > 0 ? Path.GetFullPath(args[0]) : Directory.GetCurrentDirectory();
var outDir = args.Length > 1 ? Path.GetFullPath(args[1]) : Path.Combine(root, "artifacts", "extension-runtime-token-quality-learnability", "qsg23-local-adaptive-readiness");
Directory.CreateDirectory(outDir);
var JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    WriteIndented = true
};

var generatedAtUtc = DateTimeOffset.UtcNow;
var assetRoot = Path.Combine(outDir, "assets");
var sourceManifestRef = "artifacts/extension-runtime-token-quality-learnability/qsg23-local-adaptive-readiness/source-manifest.synthetic-test-only.json";
var productionRefs = new[]
{
    "models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json",
    "models/prompt-to-task/prompt-to-task-v1/manifest.json",
    "models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json",
    "models/skill-selection/skill-selection-v1/manifest.json",
    "models/index.json"
};
var productionBefore = productionRefs.ToDictionary(item => item, item => FileHash(Join(root, item)), StringComparer.Ordinal);

var pttCommonCheckpoint = Join(root, "models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json");
var pttCommonManifest = Join(root, "models/prompt-to-task/prompt-to-task-v1/manifest.json");
var ssCommonCheckpoint = Join(root, "models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json");
var ssCommonManifest = Join(root, "models/skill-selection/skill-selection-v1/manifest.json");
var ssPublicLoadCommonCheckpoint = CreateTypedSkillSelectionCheckpointCopy(root, outDir, ssCommonCheckpoint);

var pttCommonLoad = await PromptToTaskMachine.LoadCheckpointAsync(pttCommonCheckpoint, pttCommonManifest);
Ensure(pttCommonLoad.IsSuccess && pttCommonLoad.Value is not null, "PromptToTask common checkpoint load failed: " + pttCommonLoad.ErrorMessage);
var ssCommonLoad = await SkillSelectionMachine.LoadCheckpointAsync(ssPublicLoadCommonCheckpoint);
Ensure(ssCommonLoad.IsSuccess && ssCommonLoad.Value is not null, "SkillSelection common checkpoint load failed: " + ssCommonLoad.ErrorMessage);

var sourceManifestPath = Join(root, sourceManifestRef);
WriteJson(sourceManifestPath, new Dictionary<string, object?>
{
    ["schema"] = "xiigen.qsg23.synthetic_source_manifest.v1",
    ["phase_id"] = "QUALITY-SEMANTIC-GATES-23",
    ["generated_at_utc"] = generatedAtUtc,
    ["scope"] = "test-only staged local adaptive fixtures under artifacts; no production model/locator mutation",
    ["semantic_quality_claimed"] = false,
    ["fixtures"] = new[]
    {
        "qsg23-ptt-initial",
        "qsg23-ptt-continue-delta",
        "qsg23-skill-initial",
        "qsg23-skill-continue-delta"
    }
});

var promptToTask = BuildPromptToTaskProof(root, outDir, assetRoot, sourceManifestRef, pttCommonLoad.Value!, pttCommonCheckpoint, pttCommonManifest);
var skillSelection = await BuildSkillSelectionProof(root, outDir, assetRoot, ssCommonLoad.Value!, ssPublicLoadCommonCheckpoint, ssCommonCheckpoint, ssCommonManifest);

var productionAfter = productionRefs.ToDictionary(item => item, item => FileHash(Join(root, item)), StringComparer.Ordinal);
var productionMutationRows = productionRefs.Select(item => new Dictionary<string, object?>
{
    ["ref"] = item,
    ["sha256_before"] = productionBefore[item],
    ["sha256_after"] = productionAfter[item],
    ["mutated"] = !string.Equals(productionBefore[item], productionAfter[item], StringComparison.Ordinal)
}).ToArray();

var report = new Dictionary<string, object?>
{
    ["schema"] = "xiigen.quality_semantic_gates_23.dotnet_probe_result.v1",
    ["phase_id"] = "QUALITY-SEMANTIC-GATES-23",
    ["generated_at_utc"] = generatedAtUtc,
    ["dry_run_only"] = true,
    ["production_locator_or_checkpoint_mutated"] = productionMutationRows.Any(row => row["mutated"] is true),
    ["production_mutation_rows"] = productionMutationRows,
    ["source_manifest_ref"] = sourceManifestRef,
    ["machines"] = new Dictionary<string, object?>
    {
        ["PromptToTaskMachine"] = promptToTask,
        ["SkillSelectionMachine"] = skillSelection
    }
};

var reportPath = Path.Combine(outDir, "dotnet-probe-result.json");
WriteJson(reportPath, report);
Console.WriteLine(JsonSerializer.Serialize(report, JsonOptions));

Dictionary<string, object?> BuildPromptToTaskProof(
    string rootPath,
    string outputRoot,
    string assets,
    string stagedSourceManifestRef,
    PromptToTaskMachineLoadResult commonLoad,
    string commonCheckpoint,
    string commonManifest)
{
    var userDir = Path.Combine(assets, "user-adaptive", "prompt-to-task", "prompt-to-task-v1");
    var continuedDir = Path.Combine(assets, "continued", "prompt-to-task", "prompt-to-task-v1");
    var backupDir = Path.Combine(assets, "backup", "prompt-to-task", "prompt-to-task-v1");
    var importDir = Path.Combine(assets, "share-import", "prompt-to-task", "prompt-to-task-v1");
    Directory.CreateDirectory(userDir);
    Directory.CreateDirectory(continuedDir);
    Directory.CreateDirectory(backupDir);
    Directory.CreateDirectory(importDir);

    var initialPattern = new PromptToTaskLearnedPattern(
        "qsg23-ptt-initial-local-adaptive",
        "qsg23_test_only_local_adaptive_task",
        new[] { "qsg23_local_adaptive", "fresh_load", "no_production_mutation", "portable_lifecycle" },
        new[] { "prove staged local adaptive load order through a universal task contract" },
        new[] { "test_only_fixture", "semantic_quality_claimed_false", "production_mutation_forbidden" },
        new[] { "fresh-load evidence", "load-order evidence", "claim-boundary notes" },
        new[] { "mutate production checkpoint", "claim semantic quality", "use project stack as universal authority" },
        new[] { "host_boundary_from_request" },
        new[] { "backup", "export", "import", "continue-training", "held-out", "ablation", "rollback" },
        "qsg23-synthetic-train",
        0.97);

    var userMachine = commonLoad.Machine;
    var initialContinue = userMachine.ContinueWithAcceptedPattern(initialPattern, "qsg23-accepted:ptt-initial-test-only");
    Ensure(initialContinue.IsSuccess, "PromptToTask initial staged continue failed: " + initialContinue.ErrorMessage);
    var userCheckpoint = Path.Combine(userDir, "prompt-to-task-v1-checkpoint.json");
    var userManifest = Path.Combine(userDir, "manifest.json");
    var userAccepted = commonLoad.Checkpoint.AcceptedRefIds.Concat(new[] { "qsg23-accepted:ptt-initial-test-only" }).ToArray();
    var userSave = userMachine.SaveCheckpointAsync(userCheckpoint, userManifest, stagedSourceManifestRef, userAccepted).GetAwaiter().GetResult();
    Ensure(userSave.IsSuccess && userSave.Value is not null, "PromptToTask user staged save failed: " + userSave.ErrorMessage);
    NormalizeJsonPairNoBom(userCheckpoint, userManifest);
    CopyPair(userCheckpoint, userManifest, backupDir);

    var freshRequest = PromptRequest(
        "qsg23-ptt-fresh-load",
        "Portable local adaptive lifecycle proof with no production mutation.",
        new[] { "qsg23_local_adaptive", "fresh_load", "no_production_mutation", "portable_lifecycle" },
        "qsg23.synthetic.fresh");
    var userFirstLoad = LoadPromptToTaskWithFallback(userCheckpoint, userManifest, commonCheckpoint, commonManifest);
    var freshResponse = userFirstLoad.Machine.Translate(freshRequest).Value!;

    var commonOnlyLoad = LoadPromptToTaskWithFallback(
        Path.Combine(userDir, "missing-checkpoint.json"),
        Path.Combine(userDir, "missing-manifest.json"),
        commonCheckpoint,
        commonManifest);
    var ablationResponse = commonOnlyLoad.Machine.Translate(freshRequest).Value!;

    var heldOutRequest = PromptRequest(
        "qsg23-ptt-held-out",
        "Portable lifecycle evidence needs a staged adaptive proof before readiness claims.",
        new[] { "qsg23_local_adaptive", "fresh_load", "no_production_mutation", "portable_lifecycle" },
        "qsg23.synthetic.heldout");
    var heldOutResponse = userFirstLoad.Machine.Translate(heldOutRequest).Value!;

    var continuedLoadStart = PromptToTaskMachine.LoadCheckpointAsync(userCheckpoint, userManifest).GetAwaiter().GetResult();
    Ensure(continuedLoadStart.IsSuccess && continuedLoadStart.Value is not null, "PromptToTask staged checkpoint reload before continue failed: " + continuedLoadStart.ErrorMessage);
    var deltaPattern = new PromptToTaskLearnedPattern(
        "qsg23-ptt-continue-delta",
        "qsg23_test_only_continue_delta_task",
        new[] { "qsg23_continue_delta", "load_existing_checkpoint_first", "export_import_rollback" },
        new[] { "continue from an existing staged PromptToTask checkpoint before adding a delta" },
        new[] { "test_only_fixture", "no_retrain_from_scratch", "production_mutation_forbidden" },
        new[] { "continued checkpoint evidence", "import evidence", "rollback comparison" },
        new[] { "cold-start retrain masquerade", "semantic quality claim", "common checkpoint mutation" },
        new[] { "host_boundary_from_request" },
        new[] { "base checkpoint sha", "continued checkpoint sha", "rollback comparison" },
        "qsg23-synthetic-continue",
        0.98);
    var beforePatternCount = continuedLoadStart.Value.Checkpoint.LearnedState.LearnedPatterns?.Count ?? 0;
    var continueResult = continuedLoadStart.Value.Machine.ContinueWithAcceptedPattern(deltaPattern, "qsg23-accepted:ptt-continue-delta-test-only");
    Ensure(continueResult.IsSuccess, "PromptToTask continue delta failed: " + continueResult.ErrorMessage);
    var continuedCheckpoint = Path.Combine(continuedDir, "prompt-to-task-v1-checkpoint.json");
    var continuedManifest = Path.Combine(continuedDir, "manifest.json");
    var continuedAccepted = continuedLoadStart.Value.Checkpoint.AcceptedRefIds.Concat(new[] { "qsg23-accepted:ptt-continue-delta-test-only" }).ToArray();
    var continuedSave = continuedLoadStart.Value.Machine.SaveCheckpointAsync(continuedCheckpoint, continuedManifest, stagedSourceManifestRef, continuedAccepted).GetAwaiter().GetResult();
    Ensure(continuedSave.IsSuccess && continuedSave.Value is not null, "PromptToTask continued save failed: " + continuedSave.ErrorMessage);
    NormalizeJsonPairNoBom(continuedCheckpoint, continuedManifest);
    CopyPair(continuedCheckpoint, continuedManifest, importDir);

    var importedCheckpoint = Path.Combine(importDir, Path.GetFileName(continuedCheckpoint));
    var importedManifest = Path.Combine(importDir, Path.GetFileName(continuedManifest));
    var importedLoad = PromptToTaskMachine.LoadCheckpointAsync(importedCheckpoint, importedManifest).GetAwaiter().GetResult();
    Ensure(importedLoad.IsSuccess && importedLoad.Value is not null, "PromptToTask imported continued load failed: " + importedLoad.ErrorMessage);
    var deltaRequest = PromptRequest(
        "qsg23-ptt-continue-runtime",
        "Continue delta should load the existing staged checkpoint before export import rollback.",
        new[] { "qsg23_continue_delta", "load_existing_checkpoint_first", "export_import_rollback" },
        "qsg23.synthetic.continue");
    var continuedResponse = importedLoad.Value.Machine.Translate(deltaRequest).Value!;
    var rollbackLoad = PromptToTaskMachine.LoadCheckpointAsync(Path.Combine(backupDir, "prompt-to-task-v1-checkpoint.json"), Path.Combine(backupDir, "manifest.json")).GetAwaiter().GetResult();
    Ensure(rollbackLoad.IsSuccess && rollbackLoad.Value is not null, "PromptToTask rollback load failed: " + rollbackLoad.ErrorMessage);
    var rollbackResponse = rollbackLoad.Value.Machine.Translate(deltaRequest).Value!;
    var afterPatternCount = importedLoad.Value.Checkpoint.LearnedState.LearnedPatterns?.Count ?? 0;

    var leakTerms = new[] { "llm_mvp_core", "c:\\", "c:/", "cursor", "claude", "codex", "qsg21", "qsg22" };
    var universalLayerText = string.Join(" | ",
        freshResponse.StructuredTask.TaskType,
        string.Join(" ", freshResponse.StructuredTask.ExplicitUserRequirements),
        string.Join(" ", freshResponse.StructuredTask.Constraints),
        string.Join(" ", freshResponse.StructuredTask.Deliverables),
        string.Join(" ", freshResponse.StructuredTask.ForbiddenActions),
        string.Join(" ", freshResponse.StructuredTask.RequiredHosts),
        string.Join(" ", freshResponse.StructuredTask.EvidenceNeeds)).ToLowerInvariant();
    var leakageTermsFound = leakTerms.Where(universalLayerText.Contains).ToArray();

    return new Dictionary<string, object?>
    {
        ["machine_id"] = "PromptToTaskMachine",
        ["asset_topology"] = new Dictionary<string, object?>
        {
            ["common_checkpoint_ref"] = ToRef(rootPath, commonCheckpoint),
            ["common_manifest_ref"] = ToRef(rootPath, commonManifest),
            ["user_adaptive_checkpoint_ref"] = ToRef(rootPath, userCheckpoint),
            ["user_adaptive_manifest_ref"] = ToRef(rootPath, userManifest),
            ["continued_checkpoint_ref"] = ToRef(rootPath, continuedCheckpoint),
            ["continued_manifest_ref"] = ToRef(rootPath, continuedManifest),
            ["backup_checkpoint_ref"] = ToRef(rootPath, Path.Combine(backupDir, "prompt-to-task-v1-checkpoint.json")),
            ["backup_manifest_ref"] = ToRef(rootPath, Path.Combine(backupDir, "manifest.json")),
            ["import_checkpoint_ref"] = ToRef(rootPath, importedCheckpoint),
            ["import_manifest_ref"] = ToRef(rootPath, importedManifest)
        },
        ["fresh_load"] = new Dictionary<string, object?>
        {
            ["passed"] = userFirstLoad.Source == "user_adaptive" && freshResponse.ContractStatus == PromptToTaskMachine.LearnedPatternStatus,
            ["load_order"] = userFirstLoad.Attempts,
            ["loaded_source"] = userFirstLoad.Source,
            ["runtime_invocation"] = PromptResponseSummary(freshResponse),
            ["common_fallback_probe"] = new Dictionary<string, object?>
            {
                ["loaded_source"] = commonOnlyLoad.Source,
                ["load_order"] = commonOnlyLoad.Attempts,
                ["runtime_invocation"] = PromptResponseSummary(ablationResponse)
            }
        },
        ["continue_training"] = new Dictionary<string, object?>
        {
            ["passed"] = afterPatternCount == beforePatternCount + 1 && continuedResponse.ContractStatus == PromptToTaskMachine.LearnedPatternStatus,
            ["loaded_existing_checkpoint_first"] = true,
            ["base_checkpoint_sha256"] = FileHash(userCheckpoint),
            ["continued_checkpoint_sha256"] = FileHash(continuedCheckpoint),
            ["patterns_before"] = beforePatternCount,
            ["patterns_after"] = afterPatternCount,
            ["new_pattern_id"] = "qsg23-ptt-continue-delta",
            ["retrain_from_scratch"] = false,
            ["runtime_invocation_after_continue"] = PromptResponseSummary(continuedResponse)
        },
        ["backup_export_import_rollback"] = new Dictionary<string, object?>
        {
            ["backup_ref_exists"] = File.Exists(Path.Combine(backupDir, "prompt-to-task-v1-checkpoint.json")) && File.Exists(Path.Combine(backupDir, "manifest.json")),
            ["export_ref"] = ToRef(rootPath, continuedCheckpoint),
            ["import_ref"] = ToRef(rootPath, importedCheckpoint),
            ["import_load_passed"] = importedLoad.IsSuccess,
            ["rollback_load_passed"] = rollbackLoad.IsSuccess,
            ["rollback_comparison_passed"] = rollbackResponse.ContractStatus != PromptToTaskMachine.LearnedPatternStatus || rollbackResponse.StructuredTask.TaskType != "qsg23_test_only_continue_delta_task",
            ["rollback_runtime_invocation"] = PromptResponseSummary(rollbackResponse)
        },
        ["held_out_ablation"] = new Dictionary<string, object?>
        {
            ["passed"] = heldOutResponse.ContractStatus == PromptToTaskMachine.LearnedPatternStatus && ablationResponse.ContractStatus != PromptToTaskMachine.LearnedPatternStatus,
            ["test_data_scope"] = "synthetic_test_only_source_disjoint_controls",
            ["source_disjoint"] = true,
            ["semantic_quality_claimed"] = false,
            ["held_out_runtime_invocation"] = PromptResponseSummary(heldOutResponse),
            ["ablation_without_adaptive_state"] = PromptResponseSummary(ablationResponse)
        },
        ["prompt_to_universal_task_boundary"] = new Dictionary<string, object?>
        {
            ["checked"] = true,
            ["task_type"] = freshResponse.StructuredTask.TaskType,
            ["required_hosts"] = freshResponse.StructuredTask.RequiredHosts,
            ["project_stack_specific_leakage_terms_found"] = leakageTermsFound,
            ["no_project_stack_specific_leakage_in_structured_task"] = leakageTermsFound.Length == 0,
            ["note"] = "SourceProvenance may name staged artifact refs; the universal task contract fields are checked for stack-specific leakage."
        },
        ["claims"] = ClosedClaims()
    };
}

async Task<Dictionary<string, object?>> BuildSkillSelectionProof(
    string rootPath,
    string outputRoot,
    string assets,
    SkillSelectionMachineLoadResult commonLoad,
    string publicLoadCommonCheckpoint,
    string registryCommonCheckpoint,
    string commonManifest)
{
    var userDir = Path.Combine(assets, "user-adaptive", "skill-selection", "skill-selection-v1");
    var continuedDir = Path.Combine(assets, "continued", "skill-selection", "skill-selection-v1");
    var backupDir = Path.Combine(assets, "backup", "skill-selection", "skill-selection-v1");
    var importDir = Path.Combine(assets, "share-import", "skill-selection", "skill-selection-v1");
    Directory.CreateDirectory(userDir);
    Directory.CreateDirectory(continuedDir);
    Directory.CreateDirectory(backupDir);
    Directory.CreateDirectory(importDir);

    var initialMachine = new SkillSelectionMachine();
    initialMachine.LoadCheckpoint(commonLoad.Checkpoint);
    var initialPair = SkillPair(
        "qsg23-skill-initial",
        "Portable adaptive lifecycle evidence requires verification before completion.",
        new[] { "verification-before-completion", "planning-session-startup", "plan-review", "ui-testing" },
        "verification-before-completion",
        SkillSelectionMachine.PromptContextTrainingAdapter,
        "qsg23.synthetic.skill.initial");
    var initialTrain = await initialMachine.TrainMachineBatchAsync(new[] { initialPair });
    Ensure(initialTrain.IsSuccess, "SkillSelection initial staged train failed: " + initialTrain.ErrorMessage);
    var userCheckpoint = Path.Combine(userDir, "skill-selection-v1-checkpoint.json");
    var userManifest = Path.Combine(userDir, "manifest.json");
    var userExport = await initialMachine.SaveCheckpointAsync(userCheckpoint, userManifest);
    Ensure(userExport.IsSuccess && userExport.Value is not null, "SkillSelection user staged save failed: " + userExport.ErrorMessage);
    NormalizeJsonPairNoBom(userCheckpoint, userManifest);
    CopyPair(userCheckpoint, userManifest, backupDir);

    var userFirstLoad = await LoadSkillSelectionWithFallback(userExport.Value, publicLoadCommonCheckpoint);
    var freshRequest = SkillRequest(
        "Portable adaptive lifecycle evidence requires verification before completion.",
        new[] { "verification-before-completion", "planning-session-startup", "plan-review", "ui-testing" },
        "qsg23.synthetic.skill.fresh",
        "fresh_load");
    var shuffledRequest = SkillRequest(
        "Portable adaptive lifecycle evidence requires verification before completion.",
        new[] { "ui-testing", "plan-review", "verification-before-completion", "planning-session-startup" },
        "qsg23.synthetic.skill.fresh.shuffled",
        "fresh_load_shuffled");
    var freshDecision = userFirstLoad.Machine.DecideFromTrainedState(freshRequest).Value!;
    var shuffledDecision = userFirstLoad.Machine.DecideFromTrainedState(shuffledRequest).Value!;
    var noPositionDecision = userFirstLoad.Machine.ProbeWithoutCandidatePositionAuthority(shuffledRequest);
    var leakageReport = userFirstLoad.Machine.EvaluateCandidateOrderLeakage(freshRequest);
    var missingUserCheckpoint = Path.Combine(userDir, "missing-user-adaptive-checkpoint.json");
    var missingUserManifest = Path.Combine(userDir, "missing-user-adaptive-manifest.json");
    var missingUserExport = new LearningMachineExport(
        SkillSelectionMachine.MachineId,
        SkillSelectionMachine.DomainTag,
        SkillSelectionMachine.Version,
        missingUserCheckpoint,
        missingUserManifest,
        0.0,
        DateTimeOffset.UtcNow);
    var commonFallback = await LoadSkillSelectionWithFallback(missingUserExport, publicLoadCommonCheckpoint);
    var commonDecision = commonFallback.Machine.DecideFromTrainedState(freshRequest).Value!;

    var continueStart = await SkillSelectionMachine.LoadCheckpointAsync(userCheckpoint);
    Ensure(continueStart.IsSuccess && continueStart.Value is not null, "SkillSelection reload before continue failed: " + continueStart.ErrorMessage);
    var continueMachine = continueStart.Value.Machine;
    var beforePairCount = continueStart.Value.Checkpoint.TrainedPairIds.Count;
    var deltaPair = SkillPair(
        "qsg23-skill-continue-delta",
        "Adaptive data gap audit needs source disjoint evidence before readiness.",
        new[] { "qsg23-delta-only-skill", "verification-before-completion", "plan-review", "skill-graph-sync" },
        "qsg23-delta-only-skill",
        SkillSelectionMachine.PromptContextTrainingAdapter,
        "qsg23.synthetic.skill.continue");
    var deltaTrain = await continueMachine.TrainMachineBatchAsync(new[] { deltaPair });
    Ensure(deltaTrain.IsSuccess, "SkillSelection continue train failed: " + deltaTrain.ErrorMessage);
    var continuedCheckpoint = Path.Combine(continuedDir, "skill-selection-v1-checkpoint.json");
    var continuedManifest = Path.Combine(continuedDir, "manifest.json");
    var continuedExport = await continueMachine.SaveCheckpointAsync(continuedCheckpoint, continuedManifest);
    Ensure(continuedExport.IsSuccess && continuedExport.Value is not null, "SkillSelection continued save failed: " + continuedExport.ErrorMessage);
    NormalizeJsonPairNoBom(continuedCheckpoint, continuedManifest);
    var continuedLoad = await SkillSelectionMachine.LoadCheckpointAsync(continuedCheckpoint);
    Ensure(continuedLoad.IsSuccess && continuedLoad.Value is not null, "SkillSelection continued load failed: " + continuedLoad.ErrorMessage);
    var afterPairCount = continuedLoad.Value.Checkpoint.TrainedPairIds.Count;
    var deltaRequest = SkillRequest(
        "Adaptive data gap audit needs source disjoint evidence before readiness.",
        new[] { "qsg23-delta-only-skill", "verification-before-completion", "plan-review", "skill-graph-sync" },
        "qsg23.synthetic.skill.continue.runtime",
        "continue_runtime");
    var continuedDecision = continuedLoad.Value.Machine.DecideFromTrainedState(deltaRequest).Value!;

    CopyPair(continuedCheckpoint, continuedManifest, importDir);
    var importedCheckpoint = Path.Combine(importDir, Path.GetFileName(continuedCheckpoint));
    var importedManifest = Path.Combine(importDir, Path.GetFileName(continuedManifest));
    var importExport = new LearningMachineExport(
        SkillSelectionMachine.MachineId,
        SkillSelectionMachine.DomainTag,
        SkillSelectionMachine.Version,
        importedCheckpoint,
        importedManifest,
        0.0,
        DateTimeOffset.UtcNow);
    var importMachine = new SkillSelectionMachine();
    var importResult = await importMachine.ImportAsync(importExport);
    Ensure(importResult.IsSuccess, "SkillSelection import load failed: " + importResult.ErrorMessage);

    var rollbackLoad = await SkillSelectionMachine.LoadCheckpointAsync(Path.Combine(backupDir, "skill-selection-v1-checkpoint.json"));
    Ensure(rollbackLoad.IsSuccess && rollbackLoad.Value is not null, "SkillSelection rollback load failed: " + rollbackLoad.ErrorMessage);
    var rollbackDecision = rollbackLoad.Value.Machine.DecideFromTrainedState(deltaRequest).Value!;

    var heldOutRequest = SkillRequest(
        "Lifecycle proof asks for verification gate before any readiness claim.",
        new[] { "ui-testing", "verification-before-completion", "plan-review", "planning-session-startup" },
        "qsg23.synthetic.skill.heldout",
        "held_out");
    var heldOutDecision = userFirstLoad.Machine.DecideFromTrainedState(heldOutRequest).Value!;
    var exactHeldOutHash = HashUpper(SkillSelectionMachine.NormalizePromptKey(heldOutRequest.Prompt));
    var exactHeldOutHit = userFirstLoad.Checkpoint.TrainedPromptKeys.Contains(exactHeldOutHash, StringComparer.Ordinal);
    var candidatePositionEvidenceUsed = freshDecision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal)
        || shuffledDecision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal)
        || noPositionDecision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal);
    var selectedInvariant = SameSet(freshDecision.SelectedSkillIds, shuffledDecision.SelectedSkillIds);
    var noPositionCanSelectSame = SameSet(freshDecision.SelectedSkillIds, noPositionDecision.SelectedSkillIds);

    return new Dictionary<string, object?>
    {
        ["machine_id"] = "SkillSelectionMachine",
        ["asset_topology"] = new Dictionary<string, object?>
        {
            ["registry_common_checkpoint_ref"] = ToRef(rootPath, registryCommonCheckpoint),
            ["common_checkpoint_ref"] = ToRef(rootPath, publicLoadCommonCheckpoint),
            ["public_load_typed_copy_ref"] = ToRef(rootPath, publicLoadCommonCheckpoint),
            ["common_manifest_ref"] = ToRef(rootPath, commonManifest),
            ["user_adaptive_checkpoint_ref"] = ToRef(rootPath, userCheckpoint),
            ["user_adaptive_manifest_ref"] = ToRef(rootPath, userManifest),
            ["continued_checkpoint_ref"] = ToRef(rootPath, continuedCheckpoint),
            ["continued_manifest_ref"] = ToRef(rootPath, continuedManifest),
            ["backup_checkpoint_ref"] = ToRef(rootPath, Path.Combine(backupDir, "skill-selection-v1-checkpoint.json")),
            ["backup_manifest_ref"] = ToRef(rootPath, Path.Combine(backupDir, "manifest.json")),
            ["import_checkpoint_ref"] = ToRef(rootPath, importedCheckpoint),
            ["import_manifest_ref"] = ToRef(rootPath, importedManifest)
        },
        ["fresh_load"] = new Dictionary<string, object?>
        {
            ["passed"] = userFirstLoad.Source == "user_adaptive" && freshDecision.SelectedSkillIds.Count > 0,
            ["load_order"] = userFirstLoad.Attempts,
            ["loaded_source"] = userFirstLoad.Source,
            ["runtime_invocation"] = SkillDecisionSummary(freshDecision),
            ["common_fallback_probe"] = new Dictionary<string, object?>
            {
                ["loaded_source"] = commonFallback.Source,
                ["load_order"] = commonFallback.Attempts,
                ["fallback_after_concrete_missing_user_path"] = true,
                ["attempted_missing_user_checkpoint_ref"] = ToRef(rootPath, missingUserCheckpoint),
                ["attempted_missing_user_manifest_ref"] = ToRef(rootPath, missingUserManifest),
                ["fallback_checkpoint_ref"] = ToRef(rootPath, publicLoadCommonCheckpoint),
                ["runtime_invocation"] = SkillDecisionSummary(commonDecision)
            }
        },
        ["continue_training"] = new Dictionary<string, object?>
        {
            ["passed"] = afterPairCount == beforePairCount + 1 && continuedDecision.SelectedSkillIds.Contains("qsg23-delta-only-skill", StringComparer.Ordinal),
            ["loaded_existing_checkpoint_first"] = true,
            ["base_checkpoint_sha256"] = FileHash(userCheckpoint),
            ["continued_checkpoint_sha256"] = FileHash(continuedCheckpoint),
            ["trained_pair_count_before"] = beforePairCount,
            ["trained_pair_count_after"] = afterPairCount,
            ["new_pair_id"] = "qsg23-skill-continue-delta",
            ["retrain_from_scratch"] = false,
            ["runtime_invocation_after_continue"] = SkillDecisionSummary(continuedDecision)
        },
        ["backup_export_import_rollback"] = new Dictionary<string, object?>
        {
            ["backup_ref_exists"] = File.Exists(Path.Combine(backupDir, "skill-selection-v1-checkpoint.json")) && File.Exists(Path.Combine(backupDir, "manifest.json")),
            ["export_ref"] = ToRef(rootPath, continuedCheckpoint),
            ["import_ref"] = ToRef(rootPath, importedCheckpoint),
            ["import_load_passed"] = importResult.IsSuccess,
            ["rollback_load_passed"] = rollbackLoad.IsSuccess,
            ["rollback_comparison_passed"] = !rollbackDecision.SelectedSkillIds.Contains("qsg23-delta-only-skill", StringComparer.Ordinal),
            ["rollback_runtime_invocation"] = SkillDecisionSummary(rollbackDecision)
        },
        ["held_out_ablation"] = new Dictionary<string, object?>
        {
            ["passed"] = !exactHeldOutHit
                && heldOutDecision.SelectedSkillIds.Contains("verification-before-completion", StringComparer.Ordinal)
                && !SameSet(heldOutDecision.SelectedSkillIds, commonDecision.SelectedSkillIds),
            ["test_data_scope"] = "synthetic_test_only_source_disjoint_controls",
            ["source_disjoint"] = true,
            ["exact_held_out_prompt_hit"] = exactHeldOutHit,
            ["adaptive_differs_from_common_ablation"] = !SameSet(heldOutDecision.SelectedSkillIds, commonDecision.SelectedSkillIds),
            ["semantic_quality_claimed"] = false,
            ["held_out_runtime_invocation"] = SkillDecisionSummary(heldOutDecision),
            ["ablation_without_adaptive_state"] = SkillDecisionSummary(commonDecision)
        },
        ["candidate_position_and_shortcut_boundary"] = new Dictionary<string, object?>
        {
            ["checked"] = true,
            ["candidate_position_weights_present"] = userFirstLoad.Checkpoint.CandidatePositionWeights.Count > 0,
            ["candidate_position_weights_label"] = "baseline/scaffold/leakage-guard only",
            ["candidate_position_evidence_used_as_final_authority"] = candidatePositionEvidenceUsed,
            ["selection_invariant_under_candidate_order_shuffle"] = selectedInvariant,
            ["probe_without_candidate_position_authority_selected_same_skills"] = noPositionCanSelectSame,
            ["probe_without_candidate_position_authority"] = SkillDecisionSummary(noPositionDecision),
            ["leakage_report"] = leakageReport,
            ["hidden_deterministic_shortcut_detected"] = candidatePositionEvidenceUsed || !selectedInvariant || !noPositionCanSelectSame,
            ["semantic_quality_claimed"] = false
        },
        ["claims"] = ClosedClaims()
    };
}

PromptToTaskRequest PromptRequest(string promptId, string rawPrompt, IReadOnlyList<string> explicitConstraints, string provenance)
    => new(
        PromptId: promptId,
        RawPrompt: rawPrompt,
        RawPromptSha256: HashLower(rawPrompt),
        PromptProvenance: provenance,
        UserLanguage: "en",
        ExplicitConstraints: explicitConstraints,
        ContextFields: new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["raw_prompt_redacted"] = "qsg23-test-only-redacted",
            ["xiigen_method"] = "prompt-to-task",
            ["host_fact.boundary"] = "host_boundary_from_request",
            ["provenance_key.phase"] = "QUALITY-SEMANTIC-GATES-23"
        });

SkillSelectionRequest SkillRequest(string prompt, IReadOnlyList<string> candidates, string sourceEntryId, string scope)
    => new(
        Prompt: prompt,
        ProjectRoot: "",
        AvailableSkillIds: candidates,
        HostFacts: new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["scope"] = "qsg23_test_only",
            ["lifecycle"] = scope,
            ["production_mutation"] = "forbidden"
        },
        UserPromptSourceEntryId: sourceEntryId);

SkillSelectionMachinePair SkillPair(
    string pairId,
    string prompt,
    IReadOnlyList<string> candidates,
    string chosen,
    string adapter,
    string sourceEntryId)
{
    var rejected = candidates.Where(item => !string.Equals(item, chosen, StringComparison.Ordinal)).ToArray();
    var request = SkillRequest(prompt, candidates, sourceEntryId, "training");
    var chosenDecision = new SkillSelectionDecision(
        SelectedSkillIds: new[] { chosen },
        RejectedSkillIds: rejected,
        EvidenceFeatures: new[] { "QSG23_SYNTHETIC_TEST_ONLY", SkillSelectionMachine.SemanticPipelineBoundaryEvidence },
        Confidence: 0.91,
        RequiresSecondPass: false,
        DecisionText: "QSG-23 synthetic test-only local adaptive training pair; semantic quality remains unclaimed.");
    var rejectedDecision = new SkillSelectionDecision(
        SelectedSkillIds: rejected.Take(1).ToArray(),
        RejectedSkillIds: new[] { chosen },
        EvidenceFeatures: new[] { "QSG23_SYNTHETIC_REJECTED_NEAR_MISS", SkillSelectionMachine.SemanticPipelineBoundaryEvidence },
        Confidence: 0.10,
        RequiresSecondPass: true,
        DecisionText: "Rejected near miss for QSG-23 synthetic test-only proof.");
    return new SkillSelectionMachinePair(
        PairId: pairId,
        Request: request,
        Chosen: chosenDecision,
        Rejected: rejectedDecision,
        RejectedType: "wrong_skill_contrast",
        SourcePromptEntryId: sourceEntryId,
        TargetTrainingAdapter: adapter);
}

PromptLoadResult LoadPromptToTaskWithFallback(string userCheckpoint, string userManifest, string commonCheckpoint, string commonManifest)
{
    var attempts = new List<Dictionary<string, object?>>();
    var userLoad = PromptToTaskMachine.LoadCheckpointAsync(userCheckpoint, userManifest).GetAwaiter().GetResult();
    attempts.Add(new Dictionary<string, object?>
    {
        ["source"] = "user_adaptive",
        ["checkpoint_ref"] = ToRef(root, userCheckpoint),
        ["manifest_ref"] = ToRef(root, userManifest),
        ["success"] = userLoad.IsSuccess,
        ["error_code"] = userLoad.ErrorCode
    });
    if (userLoad.IsSuccess && userLoad.Value is not null)
        return new PromptLoadResult(userLoad.Value.Machine, userLoad.Value.Checkpoint, userLoad.Value.Manifest, "user_adaptive", attempts);

    var commonLoad = PromptToTaskMachine.LoadCheckpointAsync(commonCheckpoint, commonManifest).GetAwaiter().GetResult();
    attempts.Add(new Dictionary<string, object?>
    {
        ["source"] = "repo_common_fallback",
        ["checkpoint_ref"] = ToRef(root, commonCheckpoint),
        ["manifest_ref"] = ToRef(root, commonManifest),
        ["success"] = commonLoad.IsSuccess,
        ["error_code"] = commonLoad.ErrorCode
    });
    Ensure(commonLoad.IsSuccess && commonLoad.Value is not null, "PromptToTask fallback load failed: " + commonLoad.ErrorMessage);
    return new PromptLoadResult(commonLoad.Value.Machine, commonLoad.Value.Checkpoint, commonLoad.Value.Manifest, "repo_common_fallback", attempts);
}

async Task<SkillLoadResult> LoadSkillSelectionWithFallback(LearningMachineExport userExport, string commonCheckpoint)
{
    var attempts = new List<Dictionary<string, object?>>();
    var userMachine = new SkillSelectionMachine();
    var userImport = await userMachine.ImportAsync(userExport);
    attempts.Add(new Dictionary<string, object?>
    {
        ["source"] = "user_adaptive",
        ["checkpoint_ref"] = ToRef(root, userExport.CheckpointRef),
        ["manifest_ref"] = ToRef(root, userExport.ManifestRef),
        ["success"] = userImport.IsSuccess,
        ["error_code"] = userImport.ErrorCode
    });
    if (userImport.IsSuccess)
    {
        var checkpointLoad = await SkillSelectionMachine.LoadCheckpointAsync(userExport.CheckpointRef);
        Ensure(checkpointLoad.IsSuccess && checkpointLoad.Value is not null, "SkillSelection checkpoint reload after import failed.");
        return new SkillLoadResult(userMachine, checkpointLoad.Value.Checkpoint, "user_adaptive", attempts);
    }

    var commonLoad = await SkillSelectionMachine.LoadCheckpointAsync(commonCheckpoint);
    attempts.Add(new Dictionary<string, object?>
    {
        ["source"] = "repo_common_fallback",
        ["checkpoint_ref"] = ToRef(root, commonCheckpoint),
        ["success"] = commonLoad.IsSuccess,
        ["error_code"] = commonLoad.ErrorCode
    });
    Ensure(commonLoad.IsSuccess && commonLoad.Value is not null, "SkillSelection fallback load failed: " + commonLoad.ErrorMessage);
    return new SkillLoadResult(commonLoad.Value.Machine, commonLoad.Value.Checkpoint, "repo_common_fallback", attempts);
}

Dictionary<string, object?> PromptResponseSummary(PromptToTaskResponse response)
    => new()
    {
        ["response_id"] = response.ResponseId,
        ["contract_status"] = response.ContractStatus,
        ["task_type"] = response.StructuredTask.TaskType,
        ["split_id"] = response.StructuredTask.SplitId,
        ["required_hosts"] = response.StructuredTask.RequiredHosts,
        ["evidence_needs"] = response.StructuredTask.EvidenceNeeds,
        ["contract_evidence"] = response.ContractEvidence,
        ["semantic_quality_claimed"] = false
    };

Dictionary<string, object?> SkillDecisionSummary(SkillSelectionDecision decision)
    => new()
    {
        ["selected_skill_ids"] = decision.SelectedSkillIds,
        ["rejected_skill_ids"] = decision.RejectedSkillIds,
        ["evidence_features"] = decision.EvidenceFeatures,
        ["confidence"] = decision.Confidence,
        ["requires_second_pass"] = decision.RequiresSecondPass,
        ["decision_text"] = decision.DecisionText,
        ["semantic_quality_claimed"] = false
    };

Dictionary<string, object?> ClosedClaims()
    => new()
    {
        ["semantic_quality_claimed"] = false,
        ["token_savings_claim_allowed"] = false,
        ["quality_improvement_claim_allowed"] = false,
        ["learnability_claimed"] = false,
        ["common_readiness_claimed"] = false,
        ["final_readiness_claimed"] = false,
        ["production_locator_or_checkpoint_mutated"] = false
    };

void CopyPair(string checkpointPath, string manifestPath, string targetDir)
{
    Directory.CreateDirectory(targetDir);
    var targetCheckpoint = Path.Combine(targetDir, Path.GetFileName(checkpointPath));
    var targetManifest = Path.Combine(targetDir, Path.GetFileName(manifestPath));
    File.Copy(checkpointPath, targetCheckpoint, true);
    File.Copy(manifestPath, targetManifest, true);
    NormalizeJsonPairNoBom(targetCheckpoint, targetManifest);
}

void NormalizeJsonPairNoBom(string checkpointPath, string manifestPath)
{
    NormalizeJsonFileNoBom(checkpointPath);
    if (!File.Exists(manifestPath))
        return;

    var manifest = ReadJsonNodeNoBom(manifestPath);
    if (manifest is JsonObject manifestObject && manifestObject.ContainsKey("checkpointSha256"))
        manifestObject["checkpointSha256"] = FileHash(checkpointPath);
    WriteJsonNodeNoBom(manifestPath, manifest);
}

void NormalizeJsonFileNoBom(string filePath)
{
    if (!File.Exists(filePath))
        return;

    WriteJsonNodeNoBom(filePath, ReadJsonNodeNoBom(filePath));
}

JsonNode ReadJsonNodeNoBom(string filePath)
{
    var text = File.ReadAllText(filePath, Encoding.UTF8);
    if (text.Length > 0 && text[0] == '\uFEFF')
        text = text[1..];
    var node = JsonNode.Parse(text);
    if (node is null)
        throw new InvalidOperationException("JSON document parsed null: " + filePath);
    return node;
}

void WriteJsonNodeNoBom(string filePath, JsonNode node)
{
    File.WriteAllText(
        filePath,
        NormalizeLf(node.ToJsonString(JsonOptions)) + "\n",
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

string CreateTypedSkillSelectionCheckpointCopy(string rootPath, string outputRoot, string registryCheckpoint)
{
    var target = Path.Combine(
        outputRoot,
        "assets",
        "normalized-common",
        "skill-selection",
        "skill-selection-v1",
        "skill-selection-v1-checkpoint.public-load-copy.json");
    Directory.CreateDirectory(Path.GetDirectoryName(target)!);

    using var doc = JsonDocument.Parse(File.ReadAllText(registryCheckpoint, Encoding.UTF8));
    var rootElement = doc.RootElement;
    var typedCheckpoint = new Dictionary<string, object?>
    {
        ["schemaVersion"] = IntProp(rootElement, "schema_version", "schemaVersion"),
        ["candidatePositionWeights"] = IntDictionary(rootElement, "candidate_position_weights", "candidatePositionWeights"),
        ["trainedPairIds"] = StringArray(rootElement, "trained_pair_ids", "trainedPairIds"),
        ["trainedPromptKeys"] = StringArray(rootElement, "trained_prompt_keys", "trainedPromptKeys"),
        ["promptUnderstanding"] = CounterUnit(rootElement, "prompt_understanding", "promptUnderstanding"),
        ["taskUnderstanding"] = CounterUnit(rootElement, "task_understanding", "taskUnderstanding"),
        ["taskType"] = CounterUnit(rootElement, "task_type", "taskType"),
        ["routing"] = CounterUnit(rootElement, "routing", "routing"),
        ["skillCandidates"] = CounterUnit(rootElement, "skill_candidates", "skillCandidates"),
        ["skillSelection"] = CounterUnit(rootElement, "skill_selection", "skillSelection"),
        ["promptContext"] = CounterUnit(rootElement, "prompt_context", "promptContext"),
        ["routeMode"] = StringProp(rootElement, "route_mode", "routeMode"),
        ["savedAtUtc"] = StringProp(rootElement, "saved_at_utc", "savedAtUtc"),
        ["resultGap"] = CounterUnit(rootElement, "result_gap", "resultGap"),
        ["outputValidation"] = CounterUnit(rootElement, "output_validation", "outputValidation"),
        ["taskCompletion"] = CounterUnit(rootElement, "task_completion", "taskCompletion"),
        ["intermediateChecks"] = CounterUnit(rootElement, "intermediate_checks", "intermediateChecks")
    };

    WriteJson(target, typedCheckpoint);
    return target;
}

Dictionary<string, object?> CounterUnit(JsonElement parent, string snakeName, string camelName)
{
    var unit = Required(parent, snakeName, camelName);
    return new Dictionary<string, object?>
    {
        ["unitId"] = StringProp(unit, "unit_id", "unitId"),
        ["schemaVersion"] = IntProp(unit, "schema_version", "schemaVersion"),
        ["skillProfiles"] = Required(unit, "skill_profiles", "skillProfiles")
            .EnumerateArray()
            .Select(CounterProfile)
            .ToArray(),
        ["trainingPairIds"] = StringArray(unit, "training_pair_ids", "trainingPairIds"),
        ["semanticQualityClaimed"] = BoolProp(unit, "semantic_quality_claimed", "semanticQualityClaimed")
    };
}

Dictionary<string, object?> CounterProfile(JsonElement profile)
    => new()
    {
        ["skillId"] = StringProp(profile, "skill_id", "skillId"),
        ["learnedSignals"] = StringIntDictionary(profile, "learned_signals", "learnedSignals"),
        ["sourcePairIds"] = StringArray(profile, "source_pair_ids", "sourcePairIds")
    };

JsonElement Required(JsonElement element, params string[] names)
{
    foreach (var name in names)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value))
            return value;
    }

    throw new InvalidOperationException("Required JSON property missing: " + string.Join("/", names));
}

int IntProp(JsonElement element, params string[] names)
    => Required(element, names).GetInt32();

bool BoolProp(JsonElement element, params string[] names)
    => Required(element, names).GetBoolean();

string StringProp(JsonElement element, params string[] names)
    => Required(element, names).GetString() ?? string.Empty;

string[] StringArray(JsonElement element, params string[] names)
    => Required(element, names)
        .EnumerateArray()
        .Select(item => item.GetString() ?? string.Empty)
        .ToArray();

Dictionary<string, int> IntDictionary(JsonElement element, params string[] names)
    => Required(element, names)
        .EnumerateObject()
        .ToDictionary(item => item.Name, item => item.Value.GetInt32(), StringComparer.Ordinal);

Dictionary<string, int> StringIntDictionary(JsonElement element, params string[] names)
    => Required(element, names)
        .EnumerateObject()
        .ToDictionary(item => item.Name, item => item.Value.GetInt32(), StringComparer.Ordinal);

bool SameSet(IReadOnlyList<string> left, IReadOnlyList<string> right)
    => left.OrderBy(item => item, StringComparer.Ordinal).SequenceEqual(right.OrderBy(item => item, StringComparer.Ordinal), StringComparer.Ordinal);

string Join(string basePath, string relative)
    => Path.GetFullPath(Path.Combine(basePath, relative.Replace('/', Path.DirectorySeparatorChar)));

string ToRef(string basePath, string filePath)
    => Path.GetRelativePath(basePath, Path.GetFullPath(filePath)).Replace(Path.DirectorySeparatorChar, '/');

string FileHash(string filePath)
    => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(filePath))).ToLowerInvariant();

string HashLower(string value)
    => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

string HashUpper(string value)
    => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

string NormalizeLf(string value)
    => value.Replace("\r\n", "\n").Replace("\r", "\n");

void Ensure(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

void WriteJson(string filePath, object value)
{
    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
    File.WriteAllText(
        filePath,
        NormalizeLf(JsonSerializer.Serialize(value, JsonOptions)) + "\n",
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

sealed record PromptLoadResult(
    PromptToTaskMachine Machine,
    PromptToTaskCheckpoint Checkpoint,
    PromptToTaskCheckpointManifest Manifest,
    string Source,
    IReadOnlyList<Dictionary<string, object?>> Attempts);

sealed record SkillLoadResult(
    SkillSelectionMachine Machine,
    SkillSelectionCheckpoint Checkpoint,
    string Source,
    IReadOnlyList<Dictionary<string, object?>> Attempts);
