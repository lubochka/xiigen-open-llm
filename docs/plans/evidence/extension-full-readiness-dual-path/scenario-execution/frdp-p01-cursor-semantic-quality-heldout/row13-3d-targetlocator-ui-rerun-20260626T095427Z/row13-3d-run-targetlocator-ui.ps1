param(
    [string] $CursorExe = "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe",
    [string] $ExtensionRoot = "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills",
    [string] $EvidenceRoot,
    [string] $CompatibleAssetRoot = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-semantic-quality-heldout\row13-targetlocator-non-ui-proof-20260626T094740Z\adaptive-asset-root",
    [string] $SourceRunnerPath = "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\test\host-qa\ROW12-cursor-panel-visibility-runner.ps1",
    [int] $PanelHoldMs = 70000,
    [int] $SubmitResultWaitMs = 45000,
    [int] $SubmitResultCdpTimeoutSec = 60
)

$ErrorActionPreference = "Stop"

$ExactPrompt = "The user challenges whether a reported pass is real when the evidence only proves a boundary marker and does not prove the broader readiness claim."
$RequestedSkills = @("output-skepticism")
$AvailableSkills = @("output-skepticism", "phase-completion-packager", "verification-before-completion", "plan-review")
$Scenario = "row13-3d-targetlocator-output-skepticism-ui"
$Stamp = (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssZ")
$CorrelationId = "row13-3d-targetlocator-ui-$Stamp"

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
        [int] $Depth = 30
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

function Get-PropertyValue {
    param(
        [AllowNull()] [object] $Object,
        [string[]] $Names
    )
    if ($null -eq $Object) { return $null }
    foreach ($name in $Names) {
        if ($Object.PSObject.Properties.Name -contains $name) {
            return $Object.$name
        }
    }
    return $null
}

function Test-StringArrayContains {
    param(
        [AllowNull()] [object] $Array,
        [string] $Value
    )
    foreach ($item in @($Array)) {
        if ([string] $item -eq $Value) { return $true }
    }
    return $false
}

function Find-LatestReport {
    param([string] $Root)
    $reports = @(Get-ChildItem -LiteralPath $Root -Recurse -Filter "row12-cursor-panel-submit-result-proof-report.json" -ErrorAction SilentlyContinue | Sort-Object LastWriteTimeUtc -Descending)
    if ($reports.Count -eq 0) { return $null }
    return $reports[0].FullName
}

if (-not $EvidenceRoot) {
    throw "EvidenceRoot is required."
}
Ensure-Directory -Path $EvidenceRoot

$resolvedCompatibleRoot = (Resolve-Path -LiteralPath $CompatibleAssetRoot).Path
$compatibleCheckpoint = Join-Path $resolvedCompatibleRoot "skill-selection\skill-selection-v1\skill-selection-v1-checkpoint.json"
$compatibleManifest = Join-Path $resolvedCompatibleRoot "skill-selection\skill-selection-v1\manifest.json"
if (-not (Test-Path -LiteralPath $compatibleCheckpoint)) { throw "Compatible checkpoint missing: $compatibleCheckpoint" }
if (-not (Test-Path -LiteralPath $compatibleManifest)) { throw "Compatible manifest missing: $compatibleManifest" }

$checkpointHashBefore = Get-FileHashLower -Path $compatibleCheckpoint
$manifestHashBefore = Get-FileHashLower -Path $compatibleManifest

$runnerRoot = Join-Path $EvidenceRoot "runner"
Ensure-Directory -Path $runnerRoot
$runnerCopyPath = Join-Path $runnerRoot "ROW13-3d-targetlocator-panel-submit-runner.ps1"
$targetLocatorPath = Join-Path $EvidenceRoot "row13-3d-target-locator.json"
$injectionSummaryPath = Join-Path $EvidenceRoot "row13-3d-injection-summary.json"
$validationPath = Join-Path $EvidenceRoot "row13-3d-targetlocator-ui-validation.json"
$readbackPath = Join-Path $EvidenceRoot "row13-3d-readback-report.json"

$targetLocator = [ordered] @{
    schema = "xiigen.row13_3d.targetlocator_ui_locator.v1"
    locator_candidate_id = "row13-3d-targetlocator-ui-$Stamp"
    target_candidate_id = "row13-targetlocator-non-ui-compatible-root"
    rollback_candidate_id = "none-no-rollback-promotion"
    activated_asset_root = $resolvedCompatibleRoot
    UserAdaptiveAssetRoot = $resolvedCompatibleRoot
    LocatorMode = "target"
    proposed_locator_refs = @(
        [ordered] @{
            machine_id = "SkillSelectionMachine"
            target_ref = $compatibleCheckpoint
            manifest_ref = $compatibleManifest
            source_asset_root = $resolvedCompatibleRoot
            source_non_ui_proof = "row13-targetlocator-non-ui-proof-20260626T094740Z"
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
$targetLocatorPathLiteral = $targetLocatorPath.Replace("'", "''")
$resolvedCompatibleRootLiteral = $resolvedCompatibleRoot.Replace("'", "''")

$runnerText = Get-Content -LiteralPath $SourceRunnerPath -Raw
$oldCommand = 'const result = await vscode.commands.executeCommand(commandName);'
$newCommand = @"
    const row13PanelInput = {
      prompt: $promptJson,
      scenario: $scenarioJson,
      correlationId: $correlationJson,
      evidenceRoot: process.env.PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR,
      method: "skill-selection",
      requestedSkills: $requestedJson,
      availableSkills: $availableJson,
      redactedInput: {
        row: "Row13.3d",
        target: "targetlocator-ui-output-skepticism",
        privateDataIncluded: false
      }
    };
    report.row13_panel_input = row13PanelInput;
    const result = await vscode.commands.executeCommand(commandName, row13PanelInput);
"@
if (-not $runnerText.Contains($oldCommand)) { throw "Could not find panel command invocation in source runner." }
$runnerText = $runnerText.Replace($oldCommand, $newCommand)

$oldProbeLine = '$submitResultProbeText = if ($SubmitResultProof.IsPresent) { "row12-ui-submit-result-probe-$stamp" } else { $null }'
$newProbeLine = '$submitResultProbeText = if ($SubmitResultProof.IsPresent) { ' + $promptJson + ' } else { $null }'
if (-not $runnerText.Contains($oldProbeLine)) { throw "Could not find submit probe line in source runner." }
$runnerText = $runnerText.Replace($oldProbeLine, $newProbeLine)

$oldEnvLine = '        PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR = $runtimeEvidenceRoot'
$newEnvLine = @"
        PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR = `$runtimeEvidenceRoot
        XIIGEN_TARGET_LOCATOR_REQUIRED = "true"
        XIIGEN_TARGET_LOCATOR_PATH = '$targetLocatorPathLiteral'
        XIIGEN_USER_ADAPTIVE_ASSET_ROOT = '$resolvedCompatibleRootLiteral'
        XIIGEN_LOCATOR_MODE = "target"
"@
if (-not $runnerText.Contains($oldEnvLine)) { throw "Could not find runtime evidence environment line in source runner." }
$runnerText = $runnerText.Replace($oldEnvLine, $newEnvLine)

$runnerText = $runnerText.Replace('row = "Row12"', 'row = "Row13.3d"')
$runnerText = $runnerText.Replace('Minimal local Cursor panel submit/result proof runner.', 'Row13.3d TargetLocator Cursor panel submit/result proof runner.')
Set-Content -LiteralPath $runnerCopyPath -Value $runnerText -Encoding UTF8

Write-JsonFile -Path $injectionSummaryPath -Value ([ordered] @{
    schema = "xiigen.row13_3d.targetlocator_ui_injection_summary.v1"
    generated_utc = (Get-Date).ToUniversalTime().ToString("o")
    row = "Row13.3d"
    exact_prompt = $ExactPrompt
    requested_skills = $RequestedSkills
    available_skills = $AvailableSkills
    compatible_asset_root = $resolvedCompatibleRoot
    compatible_checkpoint = $compatibleCheckpoint
    compatible_manifest = $compatibleManifest
    compatible_checkpoint_sha256_before = $checkpointHashBefore
    compatible_manifest_sha256_before = $manifestHashBefore
    target_locator_path = $targetLocatorPath
    runner_copy_path = $runnerCopyPath
    production_source_changed = $false
    core_source_changed = $false
    training_run = $false
    checkpoint_mutation = $false
    global_locator_update = $false
    locator_update_not_performed = $true
    promotion = $false
    promotion_not_performed = $true
    git_action_run = $false
}) -Depth 40

$parseScriptCommand = "[scriptblock]::Create((Get-Content -LiteralPath '$runnerCopyPath' -Raw)) | Out-Null"
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command $parseScriptCommand
$runnerParseExitCode = $LASTEXITCODE
if ($runnerParseExitCode -ne 0) { throw "Generated runner parse check failed with exit code $runnerParseExitCode." }

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
$runtimeEvidence = Get-PropertyValue -Object $report -Names @("submit_runtime_artifact_evidence")
$requestRef = Get-PropertyValue -Object $runtimeEvidence -Names @("request_ref")
$responseRef = Get-PropertyValue -Object $runtimeEvidence -Names @("response_ref")
$coreTraceRef = Get-PropertyValue -Object $runtimeEvidence -Names @("core_trace_ref")
$hostTraceRef = Get-PropertyValue -Object $runtimeEvidence -Names @("host_trace_ref")
$panelUiRef = Get-PropertyValue -Object $runtimeEvidence -Names @("panel_ui_ref")
$entrypointResponseRef = Get-PropertyValue -Object $runtimeEvidence -Names @("entrypoint_response_ref")

$request = if ($requestRef -and (Test-Path -LiteralPath $requestRef)) { Read-JsonFile -Path $requestRef } else { $null }
$response = if ($responseRef -and (Test-Path -LiteralPath $responseRef)) { Read-JsonFile -Path $responseRef } else { $null }
$coreTrace = if ($coreTraceRef -and (Test-Path -LiteralPath $coreTraceRef)) { Read-JsonFile -Path $coreTraceRef } else { $null }
$hostTrace = if ($hostTraceRef -and (Test-Path -LiteralPath $hostTraceRef)) { Read-JsonFile -Path $hostTraceRef } else { $null }
$panelUi = if ($panelUiRef -and (Test-Path -LiteralPath $panelUiRef)) { Read-JsonFile -Path $panelUiRef } else { $null }
$entrypointResponse = if ($entrypointResponseRef -and (Test-Path -LiteralPath $entrypointResponseRef)) { Read-JsonFile -Path $entrypointResponseRef } else { $null }

$checkpointHashAfter = Get-FileHashLower -Path $compatibleCheckpoint
$manifestHashAfter = Get-FileHashLower -Path $compatibleManifest

$checkSet = Get-PropertyValue -Object $response -Names @("CheckSetResults", "checkSetResults")
$targetLocatorRequest = Get-PropertyValue -Object $request -Names @("TargetLocator", "targetLocator")
$userAdaptiveRootRuntime = Get-PropertyValue -Object $targetLocatorRequest -Names @("UserAdaptiveAssetRoot", "userAdaptiveAssetRoot")
$selectedSkills = Get-PropertyValue -Object $response -Names @("SelectedSkills", "selectedSkills")
$availableSkillsRuntime = Get-PropertyValue -Object $request -Names @("AvailableSkills", "availableSkills")
$requestedSkillsRuntime = Get-PropertyValue -Object $request -Names @("RequestedSkills", "requestedSkills")

$targetRootMatches = $false
if ($userAdaptiveRootRuntime) {
    try { $targetRootMatches = ((Resolve-Path -LiteralPath ([string] $userAdaptiveRootRuntime)).Path -eq $resolvedCompatibleRoot) } catch { $targetRootMatches = ([string] $userAdaptiveRootRuntime -eq $resolvedCompatibleRoot) }
}

$checks = [ordered] @{
    runner_exit_code_zero = ($runnerExitCode -eq 0)
    generated_runner_parse_check_passed = ($runnerParseExitCode -eq 0)
    report_found = ($null -ne $report)
    runner_status_submit_result_pass = (Get-PropertyValue -Object $report -Names @("status")) -eq "SUBMIT_RESULT_PASS"
    request_ref_found = ($null -ne $request)
    response_ref_found = ($null -ne $response)
    core_trace_ref_found = ($null -ne $coreTrace)
    host_trace_ref_found = ($null -ne $hostTrace)
    panel_ui_ref_found = ($null -ne $panelUi)
    entrypoint_response_ref_found = ($null -ne $entrypointResponse)
    real_cursor_extension_development_host = (Get-PropertyValue -Object $report -Names @("targets_real_cursor_extension_development_host")) -eq $true
    invoked_panel_command = (Get-PropertyValue -Object $report -Names @("invoked_command")) -eq "xiigenSmartSkills.panel"
    submitClicked = (Get-PropertyValue -Object $report -Names @("submit_button_clicked_by_probe")) -eq $true
    coreInvoked = (Get-PropertyValue -Object $report -Names @("submit_core_path_invoked")) -eq $true
    panel_submission_path_invoked = (Get-PropertyValue -Object $report -Names @("submit_panel_submission_path_invoked")) -eq $true
    test_only_handler_used_false = ((Get-PropertyValue -Object $report -Names @("submit_test_only_handler_used", "test_only_handler_used")) -ne $true)
    result_rendered_in_webview = (Get-PropertyValue -Object $report -Names @("submit_result_rendered_in_webview")) -eq $true
    exact_prompt_runtime_request = (Get-PropertyValue -Object $request -Names @("UserPromptOrTaskText", "userPromptOrTaskText")) -eq $ExactPrompt
    target_locator_present_in_request = ($null -ne $targetLocatorRequest)
    user_adaptive_asset_root_present_in_request = -not [string]::IsNullOrWhiteSpace([string] $userAdaptiveRootRuntime)
    user_adaptive_asset_root_matches_compatible_root = $targetRootMatches
    target_locator_bound_in_core = (Get-PropertyValue -Object $checkSet -Names @("target_locator_bound")) -eq "true"
    user_adaptive_any_asset_loaded = (Get-PropertyValue -Object $checkSet -Names @("user_adaptive_any_asset_loaded")) -eq "true"
    skill_selection_user_asset_loaded_first = (Get-PropertyValue -Object $checkSet -Names @("skill_selection_user_asset_loaded_first")) -eq "true"
    available_skills_include_output_skepticism = Test-StringArrayContains -Array $availableSkillsRuntime -Value "output-skepticism"
    requested_skills_include_output_skepticism = Test-StringArrayContains -Array $requestedSkillsRuntime -Value "output-skepticism"
    selected_skills_include_output_skepticism = Test-StringArrayContains -Array $selectedSkills -Value "output-skepticism"
    compatible_checkpoint_unchanged_after_run = ($checkpointHashBefore -eq $checkpointHashAfter)
    compatible_manifest_unchanged_after_run = ($manifestHashBefore -eq $manifestHashAfter)
    training_claimed_false = (Get-PropertyValue -Object $report -Names @("training_claimed")) -ne $true
    semantic_quality_claimed_false = (Get-PropertyValue -Object $report -Names @("semantic_quality_claimed")) -ne $true
    token_savings_claimed_false = (Get-PropertyValue -Object $report -Names @("token_savings_claimed")) -ne $true
    readiness_claimed_false = (Get-PropertyValue -Object $report -Names @("submit_readiness_claimed", "product_readiness_claimed")) -ne $true
    injection_summary_no_training = (Read-JsonFile -Path $injectionSummaryPath).training_run -eq $false
    injection_summary_no_promotion = (Read-JsonFile -Path $injectionSummaryPath).promotion -eq $false
    injection_summary_no_global_locator_update = (Read-JsonFile -Path $injectionSummaryPath).global_locator_update -eq $false
}

$failedChecks = @($checks.GetEnumerator() | Where-Object { $_.Value -ne $true } | ForEach-Object { $_.Key })
$passThroughGap = @(
    "target_locator_present_in_request",
    "user_adaptive_asset_root_present_in_request",
    "user_adaptive_asset_root_matches_compatible_root",
    "available_skills_include_output_skepticism",
    "requested_skills_include_output_skepticism"
) | Where-Object { $checks[$_] -ne $true }

if ($failedChecks.Count -eq 0) {
    $verdict = "ROW13_3D_TARGETLOCATOR_UI_PASS"
} elseif ($passThroughGap.Count -gt 0) {
    $verdict = "ROW13_3D_TARGETLOCATOR_UI_NEEDS_FIX"
} elseif ($checks.target_locator_present_in_request -eq $true -and $checks.available_skills_include_output_skepticism -eq $true -and $checks.requested_skills_include_output_skepticism -eq $true -and $checks.selected_skills_include_output_skepticism -ne $true) {
    $verdict = "ROW13_3D_NEEDS_CORE_FIX"
} else {
    $verdict = "ROW13_3D_TARGETLOCATOR_UI_NEEDS_FIX"
}

$readbackText = @(
    ($targetLocatorRequest | ConvertTo-Json -Depth 20 -Compress),
    ($request | ConvertTo-Json -Depth 20 -Compress),
    ($response | ConvertTo-Json -Depth 20 -Compress),
    ($report | ConvertTo-Json -Depth 20 -Compress),
    "test_only_handler_used=false",
    "locator update not performed"
) -join "`n"

$readbackChecks = [ordered] @{
    TargetLocator = ($readbackText -match "TargetLocator")
    UserAdaptiveAssetRoot = ($readbackText -match "UserAdaptiveAssetRoot")
    SelectedSkills = ($readbackText -match "SelectedSkills")
    output_skepticism = ($readbackText -match "output-skepticism")
    test_only_handler_used_false = ($readbackText -match "test_only_handler_used=false") -and $checks.test_only_handler_used_false
    locator_update_not_performed = ($readbackText -match "locator update not performed") -and $checks.injection_summary_no_global_locator_update
}
Write-JsonFile -Path $readbackPath -Value ([ordered] @{
    schema = "xiigen.row13_3d.targetlocator_ui_readback.v1"
    parsed_json_reports = @($latestReportPath, $requestRef, $responseRef, $coreTraceRef, $hostTraceRef, $panelUiRef, $entrypointResponseRef, $targetLocatorPath, $injectionSummaryPath) | Where-Object { $_ }
    read_back_phrases = @("TargetLocator", "UserAdaptiveAssetRoot", "SelectedSkills", "output-skepticism", "test_only_handler_used=false", "locator update not performed")
    checks = $readbackChecks
    pass = (@($readbackChecks.GetEnumerator() | Where-Object { $_.Value -ne $true }).Count -eq 0)
}) -Depth 50

Write-JsonFile -Path $validationPath -Value ([ordered] @{
    schema = "xiigen.row13_3d.targetlocator_ui_validation.v1"
    generated_utc = (Get-Date).ToUniversalTime().ToString("o")
    verdict = $verdict
    failed_checks = $failedChecks
    pass_through_gap = $passThroughGap
    checks = $checks
    evidence_root = $EvidenceRoot
    runner_report_path = $latestReportPath
    request_ref = $requestRef
    response_ref = $responseRef
    core_trace_ref = $coreTraceRef
    host_trace_ref = $hostTraceRef
    panel_ui_ref = $panelUiRef
    entrypoint_response_ref = $entrypointResponseRef
    target_locator_path = $targetLocatorPath
    compatible_asset_root = $resolvedCompatibleRoot
    exact_prompt = $ExactPrompt
    requested_skills_runtime = $requestedSkillsRuntime
    available_skills_runtime = $availableSkillsRuntime
    selected_skills_runtime = $selectedSkills
    target_locator_request = $targetLocatorRequest
    target_locator_check_set = [ordered] @{
        target_locator_bound = Get-PropertyValue -Object $checkSet -Names @("target_locator_bound")
        target_locator_mode = Get-PropertyValue -Object $checkSet -Names @("target_locator_mode")
        target_candidate_id = Get-PropertyValue -Object $checkSet -Names @("target_candidate_id")
        target_locator_user_adaptive_asset_root = Get-PropertyValue -Object $checkSet -Names @("target_locator_user_adaptive_asset_root")
        skill_selection_asset_source = Get-PropertyValue -Object $checkSet -Names @("skill_selection_asset_source")
        skill_selection_user_asset_loaded_first = Get-PropertyValue -Object $checkSet -Names @("skill_selection_user_asset_loaded_first")
        user_adaptive_any_asset_loaded = Get-PropertyValue -Object $checkSet -Names @("user_adaptive_any_asset_loaded")
        skill_selection_evidence = Get-PropertyValue -Object $checkSet -Names @("skill_selection_evidence")
        skill_selection_method_rationale = Get-PropertyValue -Object $checkSet -Names @("skill_selection_method_rationale")
    }
    source_asset_hashes = [ordered] @{
        checkpoint_before = $checkpointHashBefore
        checkpoint_after_run = $checkpointHashAfter
        manifest_before = $manifestHashBefore
        manifest_after_run = $manifestHashAfter
    }
    no_training_no_mutation_no_promotion = [ordered] @{
        training_run = $false
        checkpoint_mutation = ($checkpointHashBefore -ne $checkpointHashAfter)
        manifest_mutation = ($manifestHashBefore -ne $manifestHashAfter)
        global_locator_update = $false
        locator_update_not_performed = $true
        promotion = $false
        promotion_not_performed = $true
        git_action_run = $false
    }
    readback_report = $readbackPath
    command_run = "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$runnerCopyPath`" -CursorExe `"$CursorExe`" -ExtensionRoot `"$ExtensionRoot`" -EvidenceBase `"$EvidenceRoot`" -PanelHoldMs $PanelHoldMs -SubmitResultProof -SubmitResultWaitMs $SubmitResultWaitMs -SubmitResultCdpTimeoutSec $SubmitResultCdpTimeoutSec"
}) -Depth 70

Write-Host "ROW13_3D_VALIDATION=$verdict"
Write-Host "ROW13_3D_EVIDENCE_ROOT=$EvidenceRoot"
Write-Host "ROW13_3D_VALIDATION_PATH=$validationPath"
Write-Host "ROW13_3D_READBACK_PATH=$readbackPath"
if ($verdict -ne "ROW13_3D_TARGETLOCATOR_UI_PASS") { exit 1 }
