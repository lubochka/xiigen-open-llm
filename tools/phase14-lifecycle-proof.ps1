[CmdletBinding()]
param(
    [string]$RunId = ""
)

$ErrorActionPreference = "Stop"

function Normalize-Text {
    param([string]$Value)
    return ($Value -replace "`r`n", "`n" -replace "`r", "`n").TrimEnd() + "`n"
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$repoRoot = [System.IO.Path]::GetFullPath($repoRoot)
$artifactRoot = Join-Path $repoRoot "artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\lifecycle-proof"
if ([string]::IsNullOrWhiteSpace($RunId)) {
    $RunId = "p14-fix-03-" + (Get-Date -Format "yyyyMMdd-HHmmss")
}

$runDir = Join-Path $artifactRoot $RunId
$probeDir = Join-Path $runDir "probe"
New-Item -ItemType Directory -Force -Path $probeDir | Out-Null

$csprojPath = Join-Path $probeDir "Phase14LifecycleProof.csproj"
$programPath = Join-Path $probeDir "Program.cs"
$commandLogPath = Join-Path $runDir "phase14-lifecycle-command-log.txt"

$csproj = @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="__REPO_ROOT__\43.SmartSkillsDomain\43.SmartSkillsDomain.csproj" />
  </ItemGroup>
</Project>
'@

$program = @'
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using SmartSkillsDomain;

var repoRoot = Path.GetFullPath(args.Length > 0 ? args[0] : Directory.GetCurrentDirectory());
var runDir = Path.GetFullPath(args.Length > 1
    ? args[1]
    : Path.Combine(repoRoot, "artifacts", "extension-runtime-token-quality-learnability", "phase-14-fixes", "lifecycle-proof", "manual-run"));

Directory.CreateDirectory(runDir);

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};
var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
var generatedAtUtc = DateTimeOffset.UtcNow;
var commonHashesBefore = CaptureCommonHashes(repoRoot);
var machines = new List<MachineProof>();
var command = $"dotnet run --project {Path.Combine(runDir, "probe", "Phase14LifecycleProof.csproj")} -- {repoRoot} {runDir}";

try
{
    machines.Add(await ProvePromptToTaskAsync(repoRoot, runDir));
    machines.Add(await ProveSkillSelectionAsync(repoRoot, runDir));
}
catch (Exception ex)
{
    var needsFix = new
    {
        schema = "xiigen.phase14.lifecycle_proof.exception.v1",
        generated_at_utc = generatedAtUtc,
        verdict = "NEEDS_FIX",
        exception = ex.ToString(),
        no_common_checkpoint_locator_mutation_claimed = false,
        note = "Runner stopped before a complete proof could be produced."
    };
    WriteJson(Path.Combine(runDir, "phase14-lifecycle-exception.json"), needsFix);
    throw;
}

var commonHashesAfter = CaptureCommonHashes(repoRoot);
var commonMutationDetected = commonHashesBefore
    .Any(item => !commonHashesAfter.TryGetValue(item.Key, out var after) || !string.Equals(item.Value, after, StringComparison.Ordinal));

var changedFiles = Directory.EnumerateFiles(runDir, "*", SearchOption.AllDirectories)
    .Select(path => Relative(repoRoot, path))
    .Order(StringComparer.Ordinal)
    .ToArray();
var allRows = machines.SelectMany(machine => machine.Rows).ToArray();
var allPassed = machines.All(machine => machine.Rows.All(row => row.Status == "PASS")) && !commonMutationDetected;
var report = new
{
    schema = "xiigen.phase14.full_machine_lifecycle_proof.v1",
    phase = "Phase 14",
    fix_id = "P14-FIX-03",
    fr = "FR-07",
    generated_at_utc = generatedAtUtc,
    verdict = allPassed ? "PASS_FULL_MACHINE_LIFECYCLE_PROOF" : "NEEDS_FIX",
    summary_ru = allPassed
        ? B64("0J/RgNC+0LLQtdGA0LrQsCBQMTQtRklYLTAzINGB0L7Qt9C00LDQu9CwIHN0YWdlZCBsaWZlY3ljbGUt0LTQvtC60LDQt9Cw0YLQtdC70YzRgdGC0LLQviDQtNC70Y8gUHJvbXB0VG9UYXNrINC4IFNraWxsU2VsZWN0aW9uLiDQntCx0LUg0LzQsNGI0LjQvdGLINC/0YDQvtGI0LvQuCDQt9Cw0LPRgNGD0LfQutGDIGNoZWNrcG9pbnQrbWFuaWZlc3Qg0YfQtdGA0LXQtyDQv9GD0LHQu9C40YfQvdGL0LUgbWFjaGluZS9jb3JlINCz0YDQsNC90LjRhtGLLCBiYWNrdXAvZXhwb3J0L2ltcG9ydC9zaGFyZSDQvdCwINGH0LjRgdGC0L7QvCDQv9GD0YLQuCwgY29udGludWUtdHJhaW5pbmcg0L7RgiDQuNC80L/QvtGA0YLQuNGA0L7QstCw0L3QvdC+0LPQviBjaGVja3BvaW50LCBmcmVzaC1sb2FkINC/0L7RgdC70LUgY29udGludWUsIGhlbGQtb3V0L2FibGF0aW9uL25vLWxlYWthZ2Ug0L/RgNC+0LLQtdGA0LrQuCDQv9C+0YHQu9C1IGxpZmVjeWNsZSwgcm9sbGJhY2sg0Log0L/RgNC10LTRi9C00YPRidC10LzRgyDRgdC+0YHRgtC+0Y/QvdC40Y4g0Lgg0LrQvtC90YLRgNC+0LvRjCDQvtGC0YHRg9GC0YHRgtCy0LjRjyBjb21tb24gY2hlY2twb2ludC9sb2NhdG9yIG11dGF0aW9uLg==")
        : B64("0J/RgNC+0LLQtdGA0LrQsCBQMTQtRklYLTAzINC90LUg0LfQsNC60YDRi9C70LAg0LLRgdC1INC+0LHRj9C30LDRgtC10LvRjNC90YvQtSDRgdGC0YDQvtC60LguINCh0LzQvtGC0YDQuNGC0LUgcGVyLW1hY2hpbmUgcm93cyDRgdC+INGB0YLQsNGC0YPRgdC+0LwgTkVFRFNfRklYLg=="),
    what_is_proven_ru = new[]
    {
        B64("UHJvbXB0VG9UYXNrINC4IFNraWxsU2VsZWN0aW9uINC/0YDQvtCy0LXRgNC10L3RiyDQutCw0Logc3RhZ2VkIGZ1bGwtbWFjaGluZSBsaWZlY3ljbGUgcHJvb2Yg0LLQvdGD0YLRgNC4IGFydGlmYWN0cy9leHRlbnNpb24tcnVudGltZS10b2tlbi1xdWFsaXR5LWxlYXJuYWJpbGl0eS9waGFzZS0xNC1maXhlcy9saWZlY3ljbGUtcHJvb2Yu"),
        B64("0J7Qv9C10YDQsNGG0LjQuCDQstGL0L/QvtC70L3Rj9C70LjRgdGMINC90LAg0YHQstC10LbQuNGFIHN0YWdlZCBjaGVja3BvaW50L21hbmlmZXN0L2V4cG9ydC9zaGFyZS9yb2xsYmFjayDQv9GD0YLRj9GFLCDQsCDQvdC1INC90LAgY29tbW9uIG1vZGVsIGxvY2F0b3Iu"),
        B64("Q29udGludWUtdHJhaW5pbmcg0L/RgNC+0LLQtdGA0Y/Qu9GB0Y8g0L/QvtGB0LvQtSBpbXBvcnQgY2xlYW4gc2hhcmUgYXJ0aWZhY3Q7IGZyZXNoLWxvYWQg0L/QvtGB0LvQtSBjb250aW51ZSDQvtGC0LTQtdC70YzQvdC+INC30LDQs9GA0YPQttCw0Lsg0YHQvtGF0YDQsNC90LXQvdC90YvQuSBjaGVja3BvaW50L3N0YXRlLg=="),
        B64("SGVsZC1vdXQsIGFibGF0aW9uL25vLWxlYWthZ2Ug0L/RgNC+0LLQtdGA0LrQuCDQstGL0L/QvtC70L3Rj9C70LjRgdGMINC/0L7RgdC70LUgbGlmZWN5Y2xlIG9wZXJhdGlvbnMu")
    },
    what_is_not_proven_ru = new[]
    {
        B64("0K3RgtC+INC90LUgQ2xhdWRlL0NvZGV4IFVJIHByb29mLg=="),
        B64("0K3RgtC+INC90LUgZmluYWwgcmVhZGluZXNzIGNsYWltINC4INC90LUgaG9zdC9kYXNoYm9hcmQvQVBJIHJlYWRpbmVzcy4="),
        B64("0K3RgtC+INC90LUgc2VtYW50aWMgcXVhbGl0eSBjbGFpbTsgc3RhZ2VkIHByb29mINC40YHQv9C+0LvRjNC30YPQtdGCINC80LjQvdC40LzQsNC70YzQvdGL0LUgZGV0ZXJtaW5pc3RpYyByb3dzINC00LvRjyBsaWZlY3ljbGUgaW50ZWdyaXR5Lg=="),
        B64("0K3RgtC+INC90LUgcHJvbW90aW9uINC4INC90LUgbG9jYXRvciB1cGRhdGUgY29tbW9uL2ZpbmFsIGNoZWNrcG9pbnQgcGF0aHMu")
    },
    commands = new[]
    {
        "powershell -NoProfile -ExecutionPolicy Bypass -File tools\\phase14-lifecycle-proof.ps1",
        command
    },
    machines,
    required_rows = allRows,
    common_checkpoint_locator_hashes_before = commonHashesBefore,
    common_checkpoint_locator_hashes_after = commonHashesAfter,
    common_checkpoint_locator_mutation_detected = commonMutationDetected,
    no_common_model_checkpoint_locator_mutation_outside_staged_artifacts = !commonMutationDetected,
    external_ai_used = false,
    web_used = false,
    git_commands_run = Array.Empty<string>(),
    no_overclaim_statement = "Evidence supports only staged full-machine lifecycle proof for PromptToTask and SkillSelection rows 14.08/14.09 / FR-07. It does not prove final readiness, UI integration, host runtime readiness, dashboard readiness, semantic quality, promotion, or common locator update.",
    changed_files = changedFiles
};

WriteJson(Path.Combine(runDir, "phase14-lifecycle-proof.json"), report);
WriteMarkdown(Path.Combine(runDir, "phase14-lifecycle-proof.md"), BuildMarkdown(report, machines, commonMutationDetected), utf8NoBom);
WriteJson(Path.Combine(runDir, "phase14-lifecycle-summary.json"), new
{
    verdict = report.verdict,
    per_machine = machines.ToDictionary(machine => machine.MachineName, machine => machine.Rows.ToDictionary(row => row.ProofRequirement, row => row.Status)),
    evidence_artifacts = machines.SelectMany(machine => machine.EvidenceArtifacts).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
    changed_files = changedFiles,
    no_git_commands_run = true,
    no_common_mutation = !commonMutationDetected
});

Console.WriteLine($"P14-FIX-03 lifecycle proof verdict: {report.verdict}");
Console.WriteLine(Path.Combine(runDir, "phase14-lifecycle-proof.json"));
return allPassed ? 0 : 2;

async Task<MachineProof> ProvePromptToTaskAsync(string root, string baseRunDir)
{
    var machineDir = Path.Combine(baseRunDir, "prompt-to-task");
    Directory.CreateDirectory(machineDir);
    var baseDir = Path.Combine(machineDir, "01-initial-export");
    var backupDir = Path.Combine(machineDir, "02-backup-before-continue");
    var shareDir = Path.Combine(machineDir, "03-share-package-source");
    var shareZip = Path.Combine(machineDir, "03-share-package.zip");
    var cleanShareDir = Path.Combine(machineDir, "04-clean-share-import");
    var continueDir = Path.Combine(machineDir, "05-continued-export");
    var rollbackDir = Path.Combine(machineDir, "06-rollback-load");

    Directory.CreateDirectory(baseDir);
    Directory.CreateDirectory(backupDir);
    Directory.CreateDirectory(shareDir);
    Directory.CreateDirectory(cleanShareDir);
    Directory.CreateDirectory(continueDir);
    Directory.CreateDirectory(rollbackDir);

    var basePattern = new PromptToTaskLearnedPattern(
        PatternId: "p14-ptt-base-lifecycle",
        TaskType: "phase14_lifecycle_proof",
        LearnedSignals: ["phase14_lifecycle", "work_order:P14-FIX-03"],
        ExplicitUserRequirements: ["produce full-machine PromptToTask lifecycle proof"],
        Constraints: ["staged_artifacts_only", "no_common_checkpoint_mutation"],
        Deliverables: ["lifecycle proof packet"],
        ForbiddenActions: ["final readiness claim", "git"],
        RequiredHosts: ["llm_mvp_core"],
        EvidenceNeeds: ["checkpoint manifest load", "fresh load", "rollback"],
        SplitId: "p14-ptt-base-heldout",
        Confidence: 0.95);
    var deltaPattern = new PromptToTaskLearnedPattern(
        PatternId: "p14-ptt-delta-continue",
        TaskType: "phase14_continue_training_delta",
        LearnedSignals: ["phase14_continue_delta", "work_order:P14-FIX-03-DELTA"],
        ExplicitUserRequirements: ["continue from imported PromptToTask checkpoint"],
        Constraints: ["preserve imported base pattern"],
        Deliverables: ["continued checkpoint"],
        ForbiddenActions: ["retrain from scratch"],
        RequiredHosts: ["llm_mvp_core"],
        EvidenceNeeds: ["delta present after continue", "delta absent after rollback"],
        SplitId: "p14-ptt-delta-heldout",
        Confidence: 0.91);
    var source = new PromptToTaskMachine(new StaticPromptToTaskLoader(CreatePromptDescriptor([basePattern], "base")));

    var baseCheckpoint = Path.Combine(baseDir, "prompt-to-task-checkpoint.json");
    var baseManifest = Path.Combine(baseDir, "prompt-to-task-manifest.json");
    var baseExport = await source.SaveCheckpointAsync(baseCheckpoint, baseManifest, "phase14/prompt-to-task/source-manifest.json", ["p14-ptt-base-ref"]);
    Require(baseExport.IsSuccess, $"PromptToTask initial SaveCheckpointAsync failed: {baseExport.ErrorCode} {baseExport.ErrorMessage}");

    var baseLoad = await PromptToTaskMachine.LoadCheckpointAsync(baseCheckpoint, baseManifest);
    Require(baseLoad.IsSuccess, $"PromptToTask initial LoadCheckpointAsync failed: {baseLoad.ErrorCode} {baseLoad.ErrorMessage}");
    Require(baseLoad.Value!.Manifest.LearnedPatternCount == 1, "PromptToTask initial manifest did not record one learned pattern.");

    File.Copy(baseCheckpoint, Path.Combine(backupDir, "prompt-to-task-checkpoint.json"), overwrite: true);
    File.Copy(baseManifest, Path.Combine(backupDir, "prompt-to-task-manifest.json"), overwrite: true);
    File.Copy(baseCheckpoint, Path.Combine(shareDir, "prompt-to-task-checkpoint.json"), overwrite: true);
    File.Copy(baseManifest, Path.Combine(shareDir, "prompt-to-task-manifest.json"), overwrite: true);
    if (File.Exists(shareZip))
        File.Delete(shareZip);
    ZipFile.CreateFromDirectory(shareDir, shareZip);
    ZipFile.ExtractToDirectory(shareZip, cleanShareDir, overwriteFiles: true);

    var cleanCheckpoint = Path.Combine(cleanShareDir, "prompt-to-task-checkpoint.json");
    var cleanManifest = Path.Combine(cleanShareDir, "prompt-to-task-manifest.json");
    var cleanExport = new PromptToTaskLifecycleExport(
        PromptToTaskMachine.MachineId,
        PromptToTaskMachine.DomainTag,
        PromptToTaskMachine.Version,
        cleanCheckpoint,
        cleanManifest,
        Sha256File(cleanCheckpoint),
        DateTimeOffset.UtcNow);
    var imported = new PromptToTaskMachine();
    var importResult = await imported.ImportAsync(cleanExport);
    Require(importResult.IsSuccess, $"PromptToTask ImportAsync from clean share failed: {importResult.ErrorCode} {importResult.ErrorMessage}");

    var baseRequest = CreatePromptRequest(
        "p14-ptt-heldout-base-001",
        "Private raw PromptToTask held-out wording A is intentionally not stored.",
        "phase14_lifecycle",
        "P14-FIX-03");
    var baseDecisionBeforeContinue = imported.Translate(baseRequest);
    Require(baseDecisionBeforeContinue.IsSuccess, baseDecisionBeforeContinue.ErrorMessage ?? "PromptToTask base translate failed before continue.");
    Require(baseDecisionBeforeContinue.Value!.ContractStatus == PromptToTaskMachine.LearnedPatternStatus, "PromptToTask base held-out did not match after clean import.");

    var continueResult = imported.ContinueWithAcceptedPattern(deltaPattern, "p14-ptt-delta-ref");
    Require(continueResult.IsSuccess, $"PromptToTask ContinueWithAcceptedPattern failed: {continueResult.ErrorCode} {continueResult.ErrorMessage}");
    Require(continueResult.Value!.LearnedState.LearnedPatterns!.Count == 2, "PromptToTask continue did not preserve base pattern plus add delta.");

    var continueCheckpoint = Path.Combine(continueDir, "prompt-to-task-continued-checkpoint.json");
    var continueManifest = Path.Combine(continueDir, "prompt-to-task-continued-manifest.json");
    var continuedExport = await imported.SaveCheckpointAsync(continueCheckpoint, continueManifest, "phase14/prompt-to-task/continued-source-manifest.json", ["p14-ptt-base-ref", "p14-ptt-delta-ref"]);
    Require(continuedExport.IsSuccess, $"PromptToTask continued SaveCheckpointAsync failed: {continuedExport.ErrorCode} {continuedExport.ErrorMessage}");
    var continuedFreshLoad = await PromptToTaskMachine.LoadCheckpointAsync(continueCheckpoint, continueManifest);
    Require(continuedFreshLoad.IsSuccess, $"PromptToTask continued LoadCheckpointAsync failed: {continuedFreshLoad.ErrorCode} {continuedFreshLoad.ErrorMessage}");

    var deltaRequest = CreatePromptRequest(
        "p14-ptt-heldout-delta-001",
        "Private raw PromptToTask held-out wording B is intentionally not stored.",
        "phase14_continue_delta",
        "P14-FIX-03-DELTA");
    var continuedBaseDecision = continuedFreshLoad.Value!.Machine.Translate(baseRequest);
    var continuedDeltaDecision = continuedFreshLoad.Value.Machine.Translate(deltaRequest);
    Require(continuedBaseDecision.IsSuccess && continuedBaseDecision.Value!.StructuredTask.TaskType == "phase14_lifecycle_proof", "PromptToTask base held-out failed after continue fresh-load.");
    Require(continuedDeltaDecision.IsSuccess && continuedDeltaDecision.Value!.StructuredTask.TaskType == "phase14_continue_training_delta", "PromptToTask delta held-out failed after continue fresh-load.");

    var ablatedRequest = new PromptToTaskRequest(
        PromptId: "p14-ptt-ablation-missing-work-order",
        RawPrompt: "Private raw PromptToTask ablation wording is intentionally not stored.",
        RawPromptSha256: "sha256:p14-ptt-ablation-missing-work-order",
        PromptProvenance: "phase14:ablation",
        UserLanguage: "ru",
        ExplicitConstraints: ["unrelated_constraint_after_signal_ablation"],
        ContextFields: new Dictionary<string, string>());
    var ablatedDecision = continuedFreshLoad.Value.Machine.Translate(ablatedRequest);
    Require(ablatedDecision.IsSuccess, ablatedDecision.ErrorMessage ?? "PromptToTask ablation translate failed.");
    Require(ablatedDecision.Value!.ContractStatus != PromptToTaskMachine.LearnedPatternStatus, "PromptToTask ablation unexpectedly matched a learned pattern without required lifecycle/work_order signals.");

    var continuedFiles = new[] { continueCheckpoint, continueManifest };
    var forbiddenRaw = new[] { baseRequest.RawPrompt, deltaRequest.RawPrompt, ablatedRequest.RawPrompt };
    var leakageHits = FindTextHits(continuedFiles, forbiddenRaw.Concat(["p14-ptt-heldout-base-forbidden-training-row"]).ToArray());
    Require(leakageHits.Count == 0, $"PromptToTask no-leakage scan found hits: {string.Join(", ", leakageHits)}");

    var rollbackCheckpoint = Path.Combine(backupDir, "prompt-to-task-checkpoint.json");
    var rollbackManifest = Path.Combine(backupDir, "prompt-to-task-manifest.json");
    var rollbackLoad = await PromptToTaskMachine.LoadCheckpointAsync(rollbackCheckpoint, rollbackManifest);
    Require(rollbackLoad.IsSuccess, $"PromptToTask rollback LoadCheckpointAsync failed: {rollbackLoad.ErrorCode} {rollbackLoad.ErrorMessage}");
    var rollbackBaseDecision = rollbackLoad.Value!.Machine.Translate(baseRequest);
    var rollbackDeltaDecision = rollbackLoad.Value.Machine.Translate(deltaRequest);
    Require(rollbackBaseDecision.IsSuccess && rollbackBaseDecision.Value!.StructuredTask.TaskType == "phase14_lifecycle_proof", "PromptToTask rollback did not preserve previous base state.");
    Require(rollbackDeltaDecision.IsSuccess && rollbackDeltaDecision.Value!.StructuredTask.TaskType != "phase14_continue_training_delta", "PromptToTask rollback still contained delta pattern.");

    var rollbackMarker = Path.Combine(rollbackDir, "rollback-observation.json");
    WriteJson(rollbackMarker, new
    {
        rollback_checkpoint = Relative(repoRoot, rollbackCheckpoint),
        rollback_manifest = Relative(repoRoot, rollbackManifest),
        base_task_type_after_rollback = rollbackBaseDecision.Value.StructuredTask.TaskType,
        delta_task_type_after_rollback = rollbackDeltaDecision.Value.StructuredTask.TaskType,
        delta_absent_after_rollback = rollbackDeltaDecision.Value.StructuredTask.TaskType != "phase14_continue_training_delta"
    });

    var evidence = new[]
    {
        baseCheckpoint,
        baseManifest,
        Path.Combine(backupDir, "prompt-to-task-checkpoint.json"),
        Path.Combine(backupDir, "prompt-to-task-manifest.json"),
        shareZip,
        cleanCheckpoint,
        cleanManifest,
        continueCheckpoint,
        continueManifest,
        rollbackMarker
    }.Select(path => Relative(repoRoot, path)).ToArray();

    var rows = new List<ProofRow>
    {
        Pass("checkpoint + manifest load through public boundary", "PromptToTaskMachine.LoadCheckpointAsync loaded staged checkpoint and manifest; manifest learnedPatternCount=1."),
        Pass("backup/export/import/share artifact clean reload", "SaveCheckpointAsync export was copied into a zip share package, extracted to a clean staged path, and imported through PromptToTaskMachine.ImportAsync."),
        Pass("continue-training starts from existing/imported checkpoint", "ContinueWithAcceptedPattern ran on the clean-imported machine and preserved base pattern while adding exactly one delta pattern."),
        Pass("fresh-load after continue-training works", "PromptToTaskMachine.LoadCheckpointAsync loaded the continued checkpoint+manifest and matched both base and delta held-out requests."),
        Pass("held-out/ablation/no-leakage checks remain passing", "Held-out base and delta requests passed after lifecycle; ablation without work_order did not match learned pattern; raw prompt/no-leakage scan had zero hits."),
        Pass("rollback restores previous checkpoint/state", "Rollback load from backup checkpoint+manifest preserved base state and removed the continue delta."),
        Pass("no common model/checkpoint/locator mutation outside staged artifacts", "All PromptToTask writes were under the Phase 14 lifecycle-proof artifact directory; common hashes are checked at report level."),
        Pass("operator-language report states proven and not proven", "phase14-lifecycle-proof.md is written in the operator's language (plain language; Russian for the original author) and separates proof from non-claims.")
    };

    WriteJson(Path.Combine(machineDir, "prompt-to-task-proof-details.json"), new
    {
        machine = "PromptToTask",
        rows,
        evidence,
        decisions = new
        {
            base_after_import = baseDecisionBeforeContinue.Value.ContractStatus,
            base_after_continue = continuedBaseDecision.Value.ContractStatus,
            delta_after_continue = continuedDeltaDecision.Value.ContractStatus,
            ablation_status = ablatedDecision.Value.ContractStatus,
            rollback_delta_task_type = rollbackDeltaDecision.Value.StructuredTask.TaskType
        },
        leakage_hits = leakageHits
    });

    return new MachineProof("PromptToTask", "PASS", rows, evidence);
}

async Task<MachineProof> ProveSkillSelectionAsync(string root, string baseRunDir)
{
    var machineDir = Path.Combine(baseRunDir, "skill-selection");
    Directory.CreateDirectory(machineDir);
    var baseDir = Path.Combine(machineDir, "01-initial-export");
    var backupDir = Path.Combine(machineDir, "02-backup-before-continue");
    var shareDir = Path.Combine(machineDir, "03-share-package-source");
    var shareZip = Path.Combine(machineDir, "03-share-package.zip");
    var cleanShareDir = Path.Combine(machineDir, "04-clean-share-import");
    var continueDir = Path.Combine(machineDir, "05-continued-export");
    var rollbackDir = Path.Combine(machineDir, "06-rollback-load");

    Directory.CreateDirectory(baseDir);
    Directory.CreateDirectory(backupDir);
    Directory.CreateDirectory(shareDir);
    Directory.CreateDirectory(cleanShareDir);
    Directory.CreateDirectory(continueDir);
    Directory.CreateDirectory(rollbackDir);

    var baseTrainingRequest = new SkillSelectionRequest(
        Prompt: "Use redacted host facts and provenance context to choose the context audit skill for a safe session.",
        ProjectRoot: "artifact-staged-project-root",
        AvailableSkillIds: ["context-audit", "image-generation"],
        HostFacts: new Dictionary<string, string>
        {
            ["host_type"] = "code_agent",
            ["privacy_scope"] = "redacted"
        },
        UserPromptSourceEntryId: "p14-ss-train-base-001");
    var baseHeldoutRequest = new SkillSelectionRequest(
        Prompt: "Choose the context audit skill using redacted host facts and provenance details for this session.",
        ProjectRoot: "artifact-staged-project-root",
        AvailableSkillIds: ["image-generation", "context-audit"],
        HostFacts: new Dictionary<string, string>
        {
            ["host_type"] = "code_agent",
            ["privacy_scope"] = "redacted"
        },
        UserPromptSourceEntryId: "p14-ss-heldout-base-001");
    var source = new SkillSelectionMachine();
    var training = await source.TrainMachineBatchAsync([
        SkillPair(
            "p14-ss-base-pair-001",
            baseTrainingRequest,
            ["context-audit"],
            ["image-generation"],
            "context_audit_over_visual_near_miss",
            SkillSelectionMachine.PromptContextTrainingAdapter)
    ]);
    Require(training.IsSuccess, $"SkillSelection initial TrainMachineBatchAsync failed: {training.ErrorCode} {training.ErrorMessage}");

    var baseExport = await source.SaveCheckpointAsync(
        Path.Combine(baseDir, "skill-selection-checkpoint.json"),
        Path.Combine(baseDir, "skill-selection-manifest.json"));
    Require(baseExport.IsSuccess, $"SkillSelection initial SaveCheckpointAsync failed: {baseExport.ErrorCode} {baseExport.ErrorMessage}");
    var baseLoad = await SkillSelectionMachine.LoadCheckpointAsync(baseExport.Value!.CheckpointRef);
    Require(baseLoad.IsSuccess, $"SkillSelection initial LoadCheckpointAsync failed: {baseLoad.ErrorCode} {baseLoad.ErrorMessage}");

    File.Copy(baseExport.Value.CheckpointRef, Path.Combine(backupDir, "skill-selection-checkpoint.json"), overwrite: true);
    File.Copy(baseExport.Value.ManifestRef, Path.Combine(backupDir, "skill-selection-manifest.json"), overwrite: true);
    File.Copy(baseExport.Value.CheckpointRef, Path.Combine(shareDir, "skill-selection-checkpoint.json"), overwrite: true);
    File.Copy(baseExport.Value.ManifestRef, Path.Combine(shareDir, "skill-selection-manifest.json"), overwrite: true);
    if (File.Exists(shareZip))
        File.Delete(shareZip);
    ZipFile.CreateFromDirectory(shareDir, shareZip);
    ZipFile.ExtractToDirectory(shareZip, cleanShareDir, overwriteFiles: true);

    var cleanExport = new LearningMachineExport(
        SkillSelectionMachine.MachineId,
        SkillSelectionMachine.DomainTag,
        SkillSelectionMachine.Version,
        Path.Combine(cleanShareDir, "skill-selection-checkpoint.json"),
        Path.Combine(cleanShareDir, "skill-selection-manifest.json"),
        baseExport.Value.AccuracyAtExport,
        DateTimeOffset.UtcNow);
    var imported = new SkillSelectionMachine();
    var importResult = await imported.ImportAsync(cleanExport);
    Require(importResult.IsSuccess, $"SkillSelection ImportAsync from clean share failed: {importResult.ErrorCode} {importResult.ErrorMessage}");
    var importedBaseDecision = imported.DecideFromTrainedState(baseHeldoutRequest);
    Require(IsSkillDecision(importedBaseDecision, "context-audit"), "SkillSelection base held-out did not pass after clean import.");
    Require(!importedBaseDecision.Value!.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence), "SkillSelection base held-out used candidate-position scaffold evidence.");

    var deltaTrainingRequest = new SkillSelectionRequest(
        Prompt: "Select final lifecycle proof skill after checking continue training checkpoint rollback evidence.",
        ProjectRoot: "artifact-staged-project-root",
        AvailableSkillIds: ["phase-lifecycle-proof", "context-audit"],
        HostFacts: new Dictionary<string, string>(),
        UserPromptSourceEntryId: "p14-ss-train-delta-001");
    var deltaHeldoutRequest = new SkillSelectionRequest(
        Prompt: "Choose lifecycle proof skill for continue training and rollback evidence review.",
        ProjectRoot: "artifact-staged-project-root",
        AvailableSkillIds: ["context-audit", "phase-lifecycle-proof"],
        HostFacts: new Dictionary<string, string>(),
        UserPromptSourceEntryId: "p14-ss-heldout-delta-001");
    var beforeContinue = imported.ExportCheckpoint();
    var continueTraining = await imported.TrainMachineBatchAsync([
        SkillPair(
            "p14-ss-delta-pair-001",
            deltaTrainingRequest,
            ["phase-lifecycle-proof"],
            ["context-audit"],
            "final_lifecycle_proof_over_context_near_miss",
            SkillSelectionMachine.SkillSelectionTrainingAdapter)
    ]);
    Require(continueTraining.IsSuccess, $"SkillSelection continue TrainMachineBatchAsync failed: {continueTraining.ErrorCode} {continueTraining.ErrorMessage}");
    var afterContinue = imported.ExportCheckpoint();
    Require(beforeContinue.TrainedPairIds.Count == 1, "SkillSelection imported checkpoint did not contain exactly one base training pair before continue.");
    Require(afterContinue.TrainedPairIds.Contains("p14-ss-base-pair-001", StringComparer.Ordinal), "SkillSelection continue lost base pair id.");
    Require(afterContinue.TrainedPairIds.Contains("p14-ss-delta-pair-001", StringComparer.Ordinal), "SkillSelection continue did not add delta pair id.");

    var continuedExport = await imported.SaveCheckpointAsync(
        Path.Combine(continueDir, "skill-selection-continued-checkpoint.json"),
        Path.Combine(continueDir, "skill-selection-continued-manifest.json"));
    Require(continuedExport.IsSuccess, $"SkillSelection continued SaveCheckpointAsync failed: {continuedExport.ErrorCode} {continuedExport.ErrorMessage}");
    var continuedFreshLoad = await SkillSelectionMachine.LoadCheckpointAsync(continuedExport.Value!.CheckpointRef);
    Require(continuedFreshLoad.IsSuccess, $"SkillSelection continued LoadCheckpointAsync failed: {continuedFreshLoad.ErrorCode} {continuedFreshLoad.ErrorMessage}");

    var continuedBaseDecision = continuedFreshLoad.Value!.Machine.DecideFromTrainedState(baseHeldoutRequest);
    var continuedDeltaDecision = continuedFreshLoad.Value.Machine.DecideFromTrainedState(deltaHeldoutRequest);
    Require(IsSkillDecision(continuedBaseDecision, "context-audit"), "SkillSelection base held-out failed after continue fresh-load.");
    Require(IsSkillDecision(continuedDeltaDecision, "phase-lifecycle-proof"), "SkillSelection delta held-out failed after continue fresh-load.");

    var evalSet = new LearningEvaluationSet<SkillSelectionDomain>(
        EvaluationId: "p14-ss-heldout-after-lifecycle",
        DomainTag: SkillSelectionMachine.DomainTag,
        EvaluationItems:
        [
            SkillEval("p14-ss-heldout-base-001", baseHeldoutRequest, ["context-audit"], ["image-generation"]),
            SkillEval("p14-ss-heldout-delta-001", deltaHeldoutRequest, ["phase-lifecycle-proof"], ["context-audit"])
        ]);
    var eval = await continuedFreshLoad.Value.Machine.EvaluateAsync(evalSet);
    Require(eval.IsSuccess, $"SkillSelection held-out EvaluateAsync failed: {eval.ErrorCode} {eval.ErrorMessage}");
    Require(eval.Value!.PassesGate && Math.Abs(eval.Value.Accuracy - 1.0) < 0.0001, "SkillSelection held-out evaluation did not pass after lifecycle.");

    var strippedBaseDecision = continuedFreshLoad.Value.Machine.DecideFromTrainedState(baseHeldoutRequest with
    {
        ProjectRoot = "",
        HostFacts = new Dictionary<string, string>(),
        UserPromptSourceEntryId = ""
    });
    Require(strippedBaseDecision.IsSuccess, strippedBaseDecision.ErrorMessage ?? "SkillSelection ablation decision failed.");
    Require(continuedBaseDecision.Value!.Confidence > strippedBaseDecision.Value!.Confidence, "SkillSelection ablation did not lower confidence when context/provenance were removed.");
    var leakageReport = continuedFreshLoad.Value.Machine.EvaluateCandidateOrderLeakage(baseHeldoutRequest);
    Require(leakageReport.PermutationSelectionInvariant, "SkillSelection candidate order leakage guard was not permutation invariant.");

    var continuedFiles = new[] { continuedExport.Value.CheckpointRef, continuedExport.Value.ManifestRef };
    var leakageHits = FindTextHits(continuedFiles, [
        baseTrainingRequest.Prompt,
        baseHeldoutRequest.Prompt,
        deltaTrainingRequest.Prompt,
        deltaHeldoutRequest.Prompt,
        "p14-ss-heldout-base-001",
        "p14-ss-heldout-delta-001"
    ]);
    Require(leakageHits.Count == 0, $"SkillSelection no-leakage scan found hits: {string.Join(", ", leakageHits)}");

    var rollbackExport = new LearningMachineExport(
        SkillSelectionMachine.MachineId,
        SkillSelectionMachine.DomainTag,
        SkillSelectionMachine.Version,
        Path.Combine(backupDir, "skill-selection-checkpoint.json"),
        Path.Combine(backupDir, "skill-selection-manifest.json"),
        0.0,
        DateTimeOffset.UtcNow);
    var rollbackMachine = new SkillSelectionMachine();
    var rollbackImport = await rollbackMachine.ImportAsync(rollbackExport);
    Require(rollbackImport.IsSuccess, $"SkillSelection rollback ImportAsync failed: {rollbackImport.ErrorCode} {rollbackImport.ErrorMessage}");
    var rollbackCheckpoint = rollbackMachine.ExportCheckpoint();
    var rollbackBaseDecision = rollbackMachine.DecideFromTrainedState(baseHeldoutRequest);
    var rollbackDeltaDecision = rollbackMachine.DecideFromTrainedState(deltaHeldoutRequest);
    Require(IsSkillDecision(rollbackBaseDecision, "context-audit"), "SkillSelection rollback did not preserve base state.");
    Require(!rollbackCheckpoint.TrainedPairIds.Contains("p14-ss-delta-pair-001", StringComparer.Ordinal), "SkillSelection rollback checkpoint still contained delta pair id.");
    Require(!IsSkillDecision(rollbackDeltaDecision, "phase-lifecycle-proof"), "SkillSelection rollback still selected delta skill.");

    var rollbackMarker = Path.Combine(rollbackDir, "rollback-observation.json");
    WriteJson(rollbackMarker, new
    {
        rollback_checkpoint = Relative(repoRoot, rollbackExport.CheckpointRef),
        rollback_manifest = Relative(repoRoot, rollbackExport.ManifestRef),
        rollback_trained_pair_ids = rollbackCheckpoint.TrainedPairIds,
        delta_absent_after_rollback = !rollbackCheckpoint.TrainedPairIds.Contains("p14-ss-delta-pair-001", StringComparer.Ordinal),
        rollback_base_selected_skills = rollbackBaseDecision.Value!.SelectedSkillIds,
        rollback_delta_selected_skills = rollbackDeltaDecision.Value!.SelectedSkillIds
    });

    var evidence = new[]
    {
        baseExport.Value.CheckpointRef,
        baseExport.Value.ManifestRef,
        Path.Combine(backupDir, "skill-selection-checkpoint.json"),
        Path.Combine(backupDir, "skill-selection-manifest.json"),
        shareZip,
        cleanExport.CheckpointRef,
        cleanExport.ManifestRef,
        continuedExport.Value.CheckpointRef,
        continuedExport.Value.ManifestRef,
        rollbackMarker
    }.Select(path => Relative(repoRoot, path)).ToArray();

    var rows = new List<ProofRow>
    {
        Pass("checkpoint + manifest load through public boundary", "SkillSelection ImportAsync validated staged manifest refs and loaded checkpoint; LoadCheckpointAsync also fresh-loaded the checkpoint through the public machine boundary."),
        Pass("backup/export/import/share artifact clean reload", "SaveCheckpointAsync output was copied into a zip share package, extracted to a clean staged path, and imported through SkillSelectionMachine.ImportAsync."),
        Pass("continue-training starts from existing/imported checkpoint", "TrainMachineBatchAsync ran on the clean-imported machine; before pair count=1 and after checkpoint contains base+delta pair ids."),
        Pass("fresh-load after continue-training works", "SkillSelectionMachine.LoadCheckpointAsync loaded the continued checkpoint and both base/delta held-out decisions passed."),
        Pass("held-out/ablation/no-leakage checks remain passing", "EvaluateAsync accuracy=1.0 after lifecycle; context/provenance ablation lowered confidence; candidate-order leakage guard was invariant; raw prompt/held-out id scan had zero hits."),
        Pass("rollback restores previous checkpoint/state", "Rollback ImportAsync from backup manifest/checkpoint preserved base state and removed delta pair/decision."),
        Pass("no common model/checkpoint/locator mutation outside staged artifacts", "All SkillSelection writes were under the Phase 14 lifecycle-proof artifact directory; common hashes are checked at report level."),
        Pass("operator-language report states proven and not proven", "phase14-lifecycle-proof.md is written in the operator's language (plain language; Russian for the original author) and separates proof from non-claims.")
    };

    WriteJson(Path.Combine(machineDir, "skill-selection-proof-details.json"), new
    {
        machine = "SkillSelection",
        rows,
        evidence,
        decisions = new
        {
            base_after_import = importedBaseDecision.Value.SelectedSkillIds,
            base_after_continue = continuedBaseDecision.Value.SelectedSkillIds,
            delta_after_continue = continuedDeltaDecision.Value.SelectedSkillIds,
            ablation_confidence = strippedBaseDecision.Value!.Confidence,
            base_confidence = continuedBaseDecision.Value.Confidence,
            rollback_delta_selected = rollbackDeltaDecision.Value.SelectedSkillIds,
            heldout_accuracy = eval.Value.Accuracy
        },
        leakage_hits = leakageHits,
        candidate_order_leakage = new
        {
            leakageReport.PermutationSelectionInvariant,
            leakageReport.OrderSensitivityDetected,
            leakageReport.PermutationCount
        }
    });

    return new MachineProof("SkillSelection", "PASS", rows, evidence);
}

static PromptToTaskLearnedStateDescriptor CreatePromptDescriptor(
    IReadOnlyList<PromptToTaskLearnedPattern> patterns,
    string suffix)
    => new(
        StateId: $"phase14-prompt-to-task-state:{suffix}",
        ManifestId: $"phase14-prompt-to-task-manifest:{suffix}",
        ManifestSha256: $"sha256:{Sha256Text($"manifest:{suffix}")}",
        MachineType: nameof(PromptToTaskMachine),
        SchemaVersion: "prompt-to-task-state-v1",
        SourceBaseCommit: "staged-phase14-no-git",
        StateSha256: $"sha256:{Sha256Text(string.Join("|", patterns.Select(pattern => pattern.PatternId)))}",
        LearnedPatterns: patterns);

static PromptToTaskRequest CreatePromptRequest(string promptId, string rawPrompt, string constraint, string workOrder)
    => new(
        PromptId: promptId,
        RawPrompt: rawPrompt,
        RawPromptSha256: $"sha256:{Sha256Text(rawPrompt)}",
        PromptProvenance: "phase14:p14-fix-03:staged-heldout",
        UserLanguage: "ru",
        ExplicitConstraints: [constraint],
        ContextFields: new Dictionary<string, string>
        {
            ["work_order"] = workOrder,
            ["raw_prompt_redacted"] = "true"
        });

static SkillSelectionMachinePair SkillPair(
    string pairId,
    SkillSelectionRequest request,
    IReadOnlyList<string> chosen,
    IReadOnlyList<string> rejected,
    string rejectedType,
    string adapter)
    => new(
        PairId: pairId,
        Request: request,
        Chosen: new SkillSelectionDecision(chosen, rejected, ["P14_TRAINING_CHOSEN"], 1.0, false, "Phase 14 staged chosen skill."),
        Rejected: new SkillSelectionDecision(rejected, chosen, ["P14_TRAINING_REJECTED"], 0.1, true, "Phase 14 staged rejected near miss."),
        RejectedType: rejectedType,
        SourcePromptEntryId: request.UserPromptSourceEntryId,
        TargetTrainingAdapter: adapter);

static LearningEvaluationItem SkillEval(
    string itemId,
    SkillSelectionRequest request,
    IReadOnlyList<string> expected,
    IReadOnlyList<string> rejected)
{
    var expectedPayload = JsonSerializer.Serialize(new
    {
        selectedSkillIds = expected,
        availableSkillIds = expected.Concat(rejected).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
        hostFacts = request.HostFacts,
        sourcePromptEntryId = itemId
    }, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    return new LearningEvaluationItem(itemId, request.Prompt, expectedPayload);
}

static bool IsSkillDecision(OperationResult<SkillSelectionDecision> decision, string expected)
    => decision.IsSuccess
        && decision.Value is not null
        && decision.Value.SelectedSkillIds.Contains(expected, StringComparer.Ordinal)
        && !decision.Value.RequiresSecondPass;

static ProofRow Pass(string requirement, string evidence)
    => new(requirement, "PASS", evidence);

static IReadOnlyDictionary<string, string?> CaptureCommonHashes(string root)
{
    var paths = new[]
    {
        Path.Combine(root, "models", "prompt-to-task", "prompt-to-task-v1", "manifest.json"),
        Path.Combine(root, "models", "prompt-to-task", "prompt-to-task-v1", "prompt-to-task-v1-checkpoint.json"),
        Path.Combine(root, "models", "skill-selection", "skill-selection-v1", "manifest.json"),
        Path.Combine(root, "models", "skill-selection", "skill-selection-v1", "runner-report.json"),
        Path.Combine(root, "models", "skill-selection", "skill-selection-v1", "skill-selection-v1-checkpoint.json")
    };
    return paths.ToDictionary(path => Relative(root, path), path => File.Exists(path) ? Sha256File(path) : null, StringComparer.Ordinal);
}

static IReadOnlyList<string> FindTextHits(IEnumerable<string> files, IReadOnlyList<string> forbidden)
{
    var hits = new List<string>();
    foreach (var file in files)
    {
        var text = File.Exists(file) ? File.ReadAllText(file, Encoding.UTF8) : "";
        foreach (var value in forbidden.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            if (text.Contains(value, StringComparison.Ordinal))
                hits.Add($"{Path.GetFileName(file)}:{value}");
        }
    }

    return hits;
}

static void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

static string Sha256File(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
}

static string Sha256Text(string value)
    => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

static string Relative(string root, string path)
    => Path.GetRelativePath(root, path).Replace('\\', '/');

void WriteJson(string path, object value)
{
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    var text = JsonSerializer.Serialize(value, jsonOptions)
        .Replace("\r\n", "\n", StringComparison.Ordinal)
        .Replace("\r", "\n", StringComparison.Ordinal)
        .TrimEnd() + "\n";
    File.WriteAllText(path, text, utf8NoBom);
}

static void WriteMarkdown(string path, string text, Encoding encoding)
{
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    var normalized = text
        .Replace("\r\n", "\n", StringComparison.Ordinal)
        .Replace("\r", "\n", StringComparison.Ordinal)
        .TrimEnd() + "\n";
    File.WriteAllText(path, normalized, encoding);
}

static string BuildMarkdown(object report, IReadOnlyList<MachineProof> machines, bool commonMutationDetected)
{
    var rows = string.Join("\n", machines.SelectMany(machine =>
        machine.Rows.Select(row => $"| {machine.MachineName} | {row.ProofRequirement} | {row.Status} | {row.Evidence} |")));
    var artifacts = string.Join("\n", machines.SelectMany(machine => machine.EvidenceArtifacts).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).Select(path => $"- `{path}`"));
    var conclusionHeading = B64("0JLRi9Cy0L7QtA==");
    var verdictLabel = B64("0JLQtdGA0LTQuNC60YI=");
    var mainStatement = B64("0K3RgtCwINC/0YDQvtCy0LXRgNC60LAg0LTQvtC60LDQt9GL0LLQsNC10YIg0YLQvtC70YzQutC+IHN0YWdlZCBsaWZlY3ljbGUg0LTQu9GPIFBoYXNlIDE0IHJvd3MgMTQuMDgvMTQuMDkgLyBGUi0wNzogY2hlY2twb2ludCttYW5pZmVzdCBsb2FkLCBiYWNrdXAvZXhwb3J0L2ltcG9ydC9zaGFyZSwgY29udGludWUtdHJhaW5pbmcg0L7RgiDQuNC80L/QvtGA0YLQuNGA0L7QstCw0L3QvdC+0LPQviBjaGVja3BvaW50LCBmcmVzaC1sb2FkINC/0L7RgdC70LUgY29udGludWUsIGhlbGQtb3V0L2FibGF0aW9uL25vLWxlYWthZ2UsIHJvbGxiYWNrINC4INC+0YLRgdGD0YLRgdGC0LLQuNC1IG11dGF0aW9uIGNvbW1vbiBjaGVja3BvaW50L2xvY2F0b3Ig0L/QviBoYXNoLdC60L7QvdGC0YDQvtC70Y4u");
    var notProvenHeading = B64("0KfRgtC+INC90LUg0LTQvtC60LDQt9Cw0L3Qvg==");
    var notFinalReadiness = B64("0J3QtSDQtNC+0LrQsNC30LDQvdCwIGZpbmFsIHJlYWRpbmVzcy4=");
    var notUi = B64("0J3QtSDQtNC+0LrQsNC30LDQvdCwIENsYXVkZS9Db2RleCBVSSDQuNC90YLQtdCz0YDQsNGG0LjRjy4=");
    var notDashboard = B64("0J3QtSDQtNC+0LrQsNC30LDQvdCwIGRhc2hib2FyZC9ob3N0L0FQSSByZWFkaW5lc3Mu");
    var notSemantic = B64("0J3QtSDQt9Cw0Y/QstC70LXQvdCwIHNlbWFudGljIHF1YWxpdHkg0LjQu9C4IHByb21vdGlvbiBjb21tb24vZmluYWwgbW9kZWwgbG9jYXRvci4=");
    var matrixHeading = B64("0JzQsNGC0YDQuNGG0LA=");
    return $"""
# P14-FIX-03: full-machine lifecycle proof

## {conclusionHeading}
{verdictLabel}: `{(commonMutationDetected ? "NEEDS_FIX" : "PASS_FULL_MACHINE_LIFECYCLE_PROOF")}`.

{mainStatement}

## {notProvenHeading}
- {notFinalReadiness}
- {notUi}
- {notDashboard}
- {notSemantic}

## {matrixHeading}
| Machine | Requirement | Status | Evidence |
|---|---|---|---|
{rows}

## Common mutation control
Common checkpoint/locator mutation detected: `{commonMutationDetected}`.

## Evidence artifacts
{artifacts}

## No-overclaim
Evidence supports only the staged full-machine lifecycle scope named above. It must not be reused as final readiness, UI proof, production runtime proof, or semantic quality proof.
""";
}

static string B64(string value)
    => Encoding.UTF8.GetString(Convert.FromBase64String(value));

sealed record ProofRow(
    string ProofRequirement,
    string Status,
    string Evidence);

sealed record MachineProof(
    string MachineName,
    string Verdict,
    IReadOnlyList<ProofRow> Rows,
    IReadOnlyList<string> EvidenceArtifacts);

sealed class StaticPromptToTaskLoader(PromptToTaskLearnedStateDescriptor descriptor)
    : IPromptToTaskLearnedStateLoader
{
    public OperationResult<PromptToTaskLearnedStateDescriptor> Load()
        => OperationResult<PromptToTaskLearnedStateDescriptor>.Ok(descriptor);
}
'@

$utf8NoBomEncoding = [System.Text.UTF8Encoding]::new($false)
$csproj = $csproj.Replace("__REPO_ROOT__", $repoRoot)
[System.IO.File]::WriteAllText($csprojPath, (Normalize-Text $csproj), $utf8NoBomEncoding)
[System.IO.File]::WriteAllText($programPath, (Normalize-Text $program), $utf8NoBomEncoding)

$commandLine = "dotnet run --project `"$csprojPath`" -- `"$repoRoot`" `"$runDir`""
[System.IO.File]::WriteAllText($commandLogPath, (Normalize-Text "Running: $commandLine"), $utf8NoBomEncoding)

$commandOutput = & dotnet run --project $csprojPath -- $repoRoot $runDir 2>&1
$exitCode = $LASTEXITCODE
$commandOutput | ForEach-Object { Write-Host $_ }
$logText = "Running: $commandLine`n" + ($commandOutput -join "`n")
[System.IO.File]::WriteAllText(
    $commandLogPath,
    (Normalize-Text $logText),
    $utf8NoBomEncoding)
if ($exitCode -ne 0) {
    throw "Phase 14 lifecycle proof runner failed with exit code $exitCode. See $commandLogPath"
}

Write-Host "Phase 14 lifecycle proof artifacts: $runDir"
