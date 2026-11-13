param(
    [string] $CursorExe = "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe",
    [string] $ExtensionRoot = "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills",
    [string] $EvidenceRoot = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-semantic-quality-heldout\row13-3c-staged-candidate-ui-rerun-20260626T092751Z",
    [string] $StagedCheckpointPath = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-semantic-quality-heldout\row13-1b-evidence-only-adapter-train-export-20260626T073726Z\staged-candidate\skill-selection-checkpoint.json",
    [string] $StagedManifestPath = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-semantic-quality-heldout\row13-1b-evidence-only-adapter-train-export-20260626T073726Z\staged-candidate\skill-selection-manifest.json",
    [string] $SourceRunnerPath = "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\test\host-qa\ROW12-cursor-panel-visibility-runner.ps1",
    [int] $PanelHoldMs = 70000,
    [int] $SubmitResultWaitMs = 45000,
    [int] $SubmitResultCdpTimeoutSec = 60
)

$ErrorActionPreference = "Stop"

$ExactPrompt = "The user challenges whether a reported pass is real when the evidence only proves a boundary marker and does not prove the broader readiness claim."
$RequestedSkills = @("output-skepticism")
$AvailableSkills = @("output-skepticism", "phase-completion-packager", "verification-before-completion", "plan-review")
$Scenario = "row13-3c-staged-candidate-output-skepticism-ui"
$CorrelationId = "row13-3c-staged-candidate-ui-20260626T092751Z"

function Ensure-Directory {
    param([string] $Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Force -Path $Path | Out-Null
    }
}

function Write-JsonFile {
    param(
        [string] $Path,
        [object] $Value,
        [int] $Depth = 20
    )
    Ensure-Directory -Path (Split-Path -Parent $Path)
    $Value | ConvertTo-Json -Depth $Depth | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Read-JsonFile {
    param([string] $Path)
    return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
}

function Get-FileHashLower {
    param([string] $Path)
    return (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash.ToLowerInvariant()
}

function Set-JsonProperty {
    param(
        [object] $Object,
        [string] $Name,
        [object] $Value
    )
    if ($Object.PSObject.Properties.Name -contains $Name) {
        $Object.$Name = $Value
    } else {
        $Object | Add-Member -NotePropertyName $Name -NotePropertyValue $Value
    }
}

function Find-LatestReport {
    param([string] $Root)
    $reports = @(Get-ChildItem -LiteralPath $Root -Recurse -Filter "row12-cursor-panel-submit-result-proof-report.json" -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTimeUtc -Descending)
    if ($reports.Count -eq 0) {
        return $null
    }
    return $reports[0].FullName
}

function Test-StringArrayContains {
    param(
        [AllowNull()] [object] $Array,
        [string] $Value
    )
    foreach ($item in @($Array)) {
        if ([string] $item -eq $Value) {
            return $true
        }
    }
    return $false
}

function Get-PropertyValue {
    param(
        [AllowNull()] [object] $Object,
        [string] $Name
    )
    if ($null -eq $Object) {
        return $null
    }
    if ($Object.PSObject.Properties.Name -contains $Name) {
        return $Object.$Name
    }
    return $null
}

Ensure-Directory -Path $EvidenceRoot

$inputCheckpointHashBefore = Get-FileHashLower -Path $StagedCheckpointPath
$inputManifestHashBefore = Get-FileHashLower -Path $StagedManifestPath

$adaptiveRoot = Join-Path $EvidenceRoot "adaptive-asset-root"
$skillRoot = Join-Path $adaptiveRoot "skill-selection\skill-selection-v1"
$copiedInputsRoot = Join-Path $EvidenceRoot "input-file-copies"
$runnerRoot = Join-Path $EvidenceRoot "runner"
$targetLocatorPath = Join-Path $EvidenceRoot "row13-3c-target-locator.json"
$runnerCopyPath = Join-Path $runnerRoot "ROW13-3c-staged-candidate-panel-submit-runner.ps1"
$injectionSummaryPath = Join-Path $EvidenceRoot "row13-3c-injection-summary.json"
$validationPath = Join-Path $EvidenceRoot "row13-3c-staged-candidate-ui-validation.json"

Ensure-Directory -Path $skillRoot
Ensure-Directory -Path $copiedInputsRoot
Ensure-Directory -Path $runnerRoot

Copy-Item -LiteralPath $StagedCheckpointPath -Destination (Join-Path $copiedInputsRoot "input-skill-selection-checkpoint.json") -Force
Copy-Item -LiteralPath $StagedManifestPath -Destination (Join-Path $copiedInputsRoot "input-skill-selection-manifest.json") -Force
Copy-Item -LiteralPath $StagedCheckpointPath -Destination (Join-Path $skillRoot "skill-selection-v1-checkpoint.json") -Force

$manifestCopy = Read-JsonFile -Path $StagedManifestPath
Set-JsonProperty -Object $manifestCopy -Name "checkpointRef" -Value "skill-selection-v1-checkpoint.json"
Set-JsonProperty -Object $manifestCopy -Name "manifestRef" -Value "manifest.json"
Write-JsonFile -Path (Join-Path $skillRoot "manifest.json") -Value $manifestCopy -Depth 40

$targetLocator = [ordered] @{
    schema = "xiigen.row13_3c.staged_candidate_target_locator.v1"
    locator_candidate_id = "row13-3c-staged-candidate-locator"
    target_candidate_id = "row13-1b-staged-skill-selection-candidate"
    rollback_candidate_id = "none-no-rollback-promotion"
    activated_asset_root = $adaptiveRoot
    LocatorMode = "target"
    proposed_locator_refs = @(
        [ordered] @{
            machine_id = "SkillSelectionMachine"
            target_ref = (Join-Path $skillRoot "skill-selection-v1-checkpoint.json")
            manifest_ref = (Join-Path $skillRoot "manifest.json")
            source_checkpoint_ref = $StagedCheckpointPath
            source_manifest_ref = $StagedManifestPath
            no_checkpoint_mutation = $true
            no_global_locator_update = $true
            no_promotion = $true
        }
    )
}
Write-JsonFile -Path $targetLocatorPath -Value $targetLocator -Depth 40

$promptJson = $ExactPrompt | ConvertTo-Json -Compress
$requestedJson = ConvertTo-Json -InputObject ([string[]] $RequestedSkills) -Compress
$availableJson = ConvertTo-Json -InputObject ([string[]] $AvailableSkills) -Compress
$scenarioJson = $Scenario | ConvertTo-Json -Compress
$correlationJson = $CorrelationId | ConvertTo-Json -Compress

$runnerText = Get-Content -LiteralPath $SourceRunnerPath -Raw
$oldCommand = 'const result = await vscode.commands.executeCommand(commandName);'
$newCommand = @"
    const row13PanelInput = {
      prompt: $promptJson,
      scenario: $scenarioJson,
      correlationId: $correlationJson,
      evidenceRoot: process.env.PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR,
      method: "prompt-to-task",
      requestedSkills: $requestedJson,
      availableSkills: $availableJson,
      redactedInput: {
        row: "Row13.3c",
        target: "staged-candidate-ui-output-skepticism",
        privateDataIncluded: false
      }
    };
    report.row13_panel_input = row13PanelInput;
    const result = await vscode.commands.executeCommand(commandName, row13PanelInput);
"@
if (-not $runnerText.Contains($oldCommand)) {
    throw "Could not find panel command invocation in source runner."
}
$runnerText = $runnerText.Replace($oldCommand, $newCommand)

$oldProbeLine = '$submitResultProbeText = if ($SubmitResultProof.IsPresent) { "row12-ui-submit-result-probe-$stamp" } else { $null }'
$newProbeLine = '$submitResultProbeText = if ($SubmitResultProof.IsPresent) { ' + $promptJson + ' } else { $null }'
if (-not $runnerText.Contains($oldProbeLine)) {
    throw "Could not find submit probe line in source runner."
}
$runnerText = $runnerText.Replace($oldProbeLine, $newProbeLine)

$oldEnvLine = '        PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR = $runtimeEvidenceRoot'
$newEnvLine = @"
        PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR = `$runtimeEvidenceRoot
        XIIGEN_TARGET_LOCATOR_REQUIRED = "true"
        XIIGEN_TARGET_LOCATOR_PATH = '$targetLocatorPath'
        XIIGEN_USER_ADAPTIVE_ASSET_ROOT = '$adaptiveRoot'
        XIIGEN_LOCATOR_MODE = "target"
"@
if (-not $runnerText.Contains($oldEnvLine)) {
    throw "Could not find runtime evidence environment line in source runner."
}
$runnerText = $runnerText.Replace($oldEnvLine, $newEnvLine)

$runnerText = $runnerText.Replace('"row12-ui-submit-result-probe-"', '"row13-ui-submit-result-probe-"')
$runnerText = $runnerText.Replace('row = "Row12"', 'row = "Row13.3c"')
$runnerText = $runnerText.Replace('Minimal local Cursor panel submit/result proof runner.', 'Row13.3c staged-candidate Cursor panel submit/result proof runner.')
Set-Content -LiteralPath $runnerCopyPath -Value $runnerText -Encoding UTF8

$inputCheckpointHashAfterPrepare = Get-FileHashLower -Path $StagedCheckpointPath
$inputManifestHashAfterPrepare = Get-FileHashLower -Path $StagedManifestPath
$adaptiveCheckpointHash = Get-FileHashLower -Path (Join-Path $skillRoot "skill-selection-v1-checkpoint.json")

Write-JsonFile -Path $injectionSummaryPath -Value ([ordered] @{
    schema = "xiigen.row13_3c.staged_candidate_ui_injection_summary.v1"
    generated_utc = (Get-Date).ToUniversalTime().ToString("o")
    row = "Row13.3c"
    exact_prompt = $ExactPrompt
    requested_skills = $RequestedSkills
    available_skills = $AvailableSkills
    staged_checkpoint_path = $StagedCheckpointPath
    staged_manifest_path = $StagedManifestPath
    input_checkpoint_sha256_before = $inputCheckpointHashBefore
    input_checkpoint_sha256_after_prepare = $inputCheckpointHashAfterPrepare
    input_manifest_sha256_before = $inputManifestHashBefore
    input_manifest_sha256_after_prepare = $inputManifestHashAfterPrepare
    input_files_unchanged_after_prepare = ($inputCheckpointHashBefore -eq $inputCheckpointHashAfterPrepare -and $inputManifestHashBefore -eq $inputManifestHashAfterPrepare)
    adaptive_asset_root = $adaptiveRoot
    adaptive_skill_selection_checkpoint = (Join-Path $skillRoot "skill-selection-v1-checkpoint.json")
    adaptive_skill_selection_manifest = (Join-Path $skillRoot "manifest.json")
    adaptive_checkpoint_sha256 = $adaptiveCheckpointHash
    adaptive_checkpoint_matches_input = ($adaptiveCheckpointHash -eq $inputCheckpointHashBefore)
    copied_manifest_refs_adjusted_for_loader_contract = $true
    target_locator_path = $targetLocatorPath
    target_locator_required_env = "true"
    runner_copy_path = $runnerCopyPath
    production_source_changed = $false
    core_source_changed = $false
    training_run = $false
    checkpoint_mutation = $false
    global_locator_update = $false
    promotion = $false
    git_action_run = $false
}) -Depth 40

$parseScriptCommand = "[scriptblock]::Create((Get-Content -LiteralPath '$runnerCopyPath' -Raw)) | Out-Null"
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command $parseScriptCommand
$runnerParseExitCode = $LASTEXITCODE
if ($runnerParseExitCode -ne 0) {
    throw "Generated runner parse check failed with exit code $runnerParseExitCode."
}

& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $runnerCopyPath `
    -CursorExe $CursorExe `
    -ExtensionRoot $ExtensionRoot `
    -EvidenceBase $EvidenceRoot `
    -PanelHoldMs $PanelHoldMs `
    -SubmitResultProof `
    -SubmitResultWaitMs $SubmitResultWaitMs `
    -SubmitResultCdpTimeoutSec $SubmitResultCdpTimeoutSec
$runnerExitCode = $LASTEXITCODE

$latestReportPath = Find-LatestReport -Root $EvidenceRoot
$report = if ($latestReportPath) { Read-JsonFile -Path $latestReportPath } else { $null }
$runtimeEvidence = Get-PropertyValue -Object $report -Name "submit_runtime_artifact_evidence"
$requestRef = Get-PropertyValue -Object $runtimeEvidence -Name "request_ref"
$responseRef = Get-PropertyValue -Object $runtimeEvidence -Name "response_ref"
$coreTraceRef = Get-PropertyValue -Object $runtimeEvidence -Name "core_trace_ref"
$hostTraceRef = Get-PropertyValue -Object $runtimeEvidence -Name "host_trace_ref"
$panelUiRef = Get-PropertyValue -Object $runtimeEvidence -Name "panel_ui_ref"

$request = if ($requestRef -and (Test-Path -LiteralPath $requestRef)) { Read-JsonFile -Path $requestRef } else { $null }
$response = if ($responseRef -and (Test-Path -LiteralPath $responseRef)) { Read-JsonFile -Path $responseRef } else { $null }
$coreTrace = if ($coreTraceRef -and (Test-Path -LiteralPath $coreTraceRef)) { Read-JsonFile -Path $coreTraceRef } else { $null }
$hostTrace = if ($hostTraceRef -and (Test-Path -LiteralPath $hostTraceRef)) { Read-JsonFile -Path $hostTraceRef } else { $null }
$panelUi = if ($panelUiRef -and (Test-Path -LiteralPath $panelUiRef)) { Read-JsonFile -Path $panelUiRef } else { $null }

$inputCheckpointHashAfterRun = Get-FileHashLower -Path $StagedCheckpointPath
$inputManifestHashAfterRun = Get-FileHashLower -Path $StagedManifestPath

$checkSet = Get-PropertyValue -Object $response -Name "CheckSetResults"
$targetLocatorRequest = Get-PropertyValue -Object $request -Name "TargetLocator"
$selectedSkills = Get-PropertyValue -Object $response -Name "SelectedSkills"
$availableSkillsRuntime = Get-PropertyValue -Object $request -Name "AvailableSkills"
$requestedSkillsRuntime = Get-PropertyValue -Object $request -Name "RequestedSkills"

$checks = [ordered] @{
    runner_exit_code_zero = ($runnerExitCode -eq 0)
    generated_runner_parse_check_passed = ($runnerParseExitCode -eq 0)
    report_found = ($null -ne $report)
    request_ref_found = ($null -ne $request)
    response_ref_found = ($null -ne $response)
    core_trace_ref_found = ($null -ne $coreTrace)
    host_trace_ref_found = ($null -ne $hostTrace)
    panel_ui_ref_found = ($null -ne $panelUi)
    real_cursor_extension_development_host = (Get-PropertyValue -Object $report -Name "targets_real_cursor_extension_development_host") -eq $true
    invoked_panel_command = (Get-PropertyValue -Object $report -Name "invoked_command") -eq "xiigenSmartSkills.panel"
    submit_clicked = (Get-PropertyValue -Object $report -Name "submit_button_clicked_by_probe") -eq $true
    core_invoked = (Get-PropertyValue -Object $report -Name "submit_core_path_invoked") -eq $true
    panel_submission_path_invoked = (Get-PropertyValue -Object $report -Name "submit_panel_submission_path_invoked") -eq $true
    test_only_handler_used_false = ((Get-PropertyValue -Object $report -Name "submit_test_only_handler_used") -ne $true -and (Get-PropertyValue -Object $report -Name "test_only_handler_used") -ne $true)
    result_rendered_in_webview = (Get-PropertyValue -Object $report -Name "submit_result_rendered_in_webview") -eq $true
    exact_prompt_runtime_request = (Get-PropertyValue -Object $request -Name "UserPromptOrTaskText") -eq $ExactPrompt
    target_locator_present_in_request = ($null -ne $targetLocatorRequest)
    target_locator_bound_in_core = (Get-PropertyValue -Object $checkSet -Name "target_locator_bound") -eq "true"
    user_adaptive_asset_loaded = (Get-PropertyValue -Object $checkSet -Name "user_adaptive_any_asset_loaded") -eq "true"
    skill_selection_user_asset_loaded_first = (Get-PropertyValue -Object $checkSet -Name "skill_selection_user_asset_loaded_first") -eq "true"
    skill_selection_asset_source_user_adaptive = (Get-PropertyValue -Object $checkSet -Name "skill_selection_asset_source") -eq "user_adaptive_cache"
    available_skills_include_output_skepticism = Test-StringArrayContains -Array $availableSkillsRuntime -Value "output-skepticism"
    requested_skills_include_output_skepticism = Test-StringArrayContains -Array $requestedSkillsRuntime -Value "output-skepticism"
    selected_skills_include_output_skepticism = Test-StringArrayContains -Array $selectedSkills -Value "output-skepticism"
    input_checkpoint_unchanged_after_run = ($inputCheckpointHashBefore -eq $inputCheckpointHashAfterRun)
    input_manifest_unchanged_after_run = ($inputManifestHashBefore -eq $inputManifestHashAfterRun)
    adaptive_checkpoint_matches_input_after_run = ((Get-FileHashLower -Path (Join-Path $skillRoot "skill-selection-v1-checkpoint.json")) -eq $inputCheckpointHashBefore)
    training_claimed_false = (Get-PropertyValue -Object $report -Name "training_claimed") -ne $true
    semantic_quality_claimed_false = (Get-PropertyValue -Object $report -Name "semantic_quality_claimed") -ne $true
    token_savings_claimed_false = (Get-PropertyValue -Object $report -Name "token_savings_claimed") -ne $true
    readiness_claimed_false = (Get-PropertyValue -Object $report -Name "submit_readiness_claimed") -ne $true
    injection_summary_no_training = (Read-JsonFile -Path $injectionSummaryPath).training_run -eq $false
    injection_summary_no_promotion = (Read-JsonFile -Path $injectionSummaryPath).promotion -eq $false
    injection_summary_no_global_locator_update = (Read-JsonFile -Path $injectionSummaryPath).global_locator_update -eq $false
}

$failedChecks = @($checks.GetEnumerator() | Where-Object { $_.Value -ne $true } | ForEach-Object { $_.Key })
$verdict = if ($failedChecks.Count -eq 0) { "ROW13_3C_STAGED_CANDIDATE_UI_PASS" } else { "NEEDS_FIX" }
if ($checks.report_found -eq $true -and
    $checks.target_locator_present_in_request -eq $true -and
    $checks.target_locator_bound_in_core -ne $true) {
    $verdict = "ROW13_3C_STAGED_CANDIDATE_NEEDS_CORE_FIX"
}

Write-JsonFile -Path $validationPath -Value ([ordered] @{
    schema = "xiigen.row13_3c.staged_candidate_ui_validation.v1"
    generated_utc = (Get-Date).ToUniversalTime().ToString("o")
    verdict = $verdict
    failed_checks = $failedChecks
    checks = $checks
    evidence_root = $EvidenceRoot
    runner_report_path = $latestReportPath
    request_ref = $requestRef
    response_ref = $responseRef
    core_trace_ref = $coreTraceRef
    host_trace_ref = $hostTraceRef
    panel_ui_ref = $panelUiRef
    target_locator_path = $targetLocatorPath
    adaptive_asset_root = $adaptiveRoot
    exact_prompt = $ExactPrompt
    requested_skills_runtime = $requestedSkillsRuntime
    available_skills_runtime = $availableSkillsRuntime
    selected_skills_runtime = $selectedSkills
    check_set_selected_skill_count = Get-PropertyValue -Object $checkSet -Name "selected_skills_count"
    skill_selection_evidence = Get-PropertyValue -Object $checkSet -Name "skill_selection_evidence"
    skill_selection_method_rationale = Get-PropertyValue -Object $checkSet -Name "skill_selection_method_rationale"
    target_locator_request = $targetLocatorRequest
    target_locator_check_set = [ordered] @{
        target_locator_bound = Get-PropertyValue -Object $checkSet -Name "target_locator_bound"
        target_locator_mode = Get-PropertyValue -Object $checkSet -Name "target_locator_mode"
        target_candidate_id = Get-PropertyValue -Object $checkSet -Name "target_candidate_id"
        target_locator_user_adaptive_asset_root = Get-PropertyValue -Object $checkSet -Name "target_locator_user_adaptive_asset_root"
        skill_selection_asset_source = Get-PropertyValue -Object $checkSet -Name "skill_selection_asset_source"
        skill_selection_user_asset_loaded_first = Get-PropertyValue -Object $checkSet -Name "skill_selection_user_asset_loaded_first"
        user_adaptive_any_asset_loaded = Get-PropertyValue -Object $checkSet -Name "user_adaptive_any_asset_loaded"
    }
    source_input_hashes = [ordered] @{
        checkpoint_before = $inputCheckpointHashBefore
        checkpoint_after_run = $inputCheckpointHashAfterRun
        manifest_before = $inputManifestHashBefore
        manifest_after_run = $inputManifestHashAfterRun
    }
    no_training_no_mutation_no_promotion = [ordered] @{
        training_run = $false
        checkpoint_mutation = ($inputCheckpointHashBefore -ne $inputCheckpointHashAfterRun)
        manifest_mutation = ($inputManifestHashBefore -ne $inputManifestHashAfterRun)
        global_locator_update = $false
        promotion = $false
        git_action_run = $false
    }
}) -Depth 60

Write-Host "ROW13_3C_VALIDATION=$verdict"
Write-Host "ROW13_3C_EVIDENCE_ROOT=$EvidenceRoot"
Write-Host "ROW13_3C_VALIDATION_PATH=$validationPath"

if ($verdict -ne "ROW13_3C_STAGED_CANDIDATE_UI_PASS") {
    exit 1
}
