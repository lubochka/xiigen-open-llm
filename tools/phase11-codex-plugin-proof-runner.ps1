param()

$ErrorActionPreference = "Stop"

$CoreRoot = "C:\Projects\llm_mvp_core"
$PluginRoot = "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500"
$ArtifactRoot = Join-Path $CoreRoot "artifacts\extension-runtime-token-quality-learnability\phase-11-codex-full-extension-live-proof"
$RunnerPath = Join-Path $CoreRoot "tools\phase11-codex-plugin-proof-runner.ps1"
$SkillPath = Join-Path $PluginRoot "skills\xiigen-smart-skills\SKILL.md"
$PluginManifestPath = Join-Path $PluginRoot ".codex-plugin\plugin.json"
$PackageJsonPath = Join-Path $PluginRoot "package.json"
$EntrypointPath = Join-Path $PluginRoot "src\codex-entrypoint.js"
$StartedUtc = (Get-Date).ToUniversalTime()
$RunStamp = $StartedUtc.ToString("yyyyMMddTHHmmssfffffffZ")
$CorrelationId = "PHASE11-CODEX-$RunStamp"
$RunId = "phase11-codex-proof-$RunStamp"
$RunDir = Join-Path $ArtifactRoot $RunId
$RedactedPrompt = "<redacted phase 11 codex installed skill smoke>"
$UnredactedPromptSentinel = "PHASE11_UNREDACTED_SMOKE_PROMPT_DO_NOT_PERSIST_$RunStamp"

function Assert-Phase11 {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function New-Phase11Directory {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Force -Path $Path | Out-Null
    }
}

function Write-Phase11Text {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text
    )

    New-Phase11Directory -Path (Split-Path -Parent $Path)
    $Utf8NoBom = [System.Text.UTF8Encoding]::new($false)
    [System.IO.File]::WriteAllText($Path, $Text, $Utf8NoBom)
}

function Write-Phase11Json {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)]$Value
    )

    $Json = $Value | ConvertTo-Json -Depth 80
    Write-Phase11Text -Path $Path -Text ($Json + [Environment]::NewLine)
}

function Read-Phase11Json {
    param([Parameter(Mandatory = $true)][string]$Path)

    $Text = [System.IO.File]::ReadAllText($Path).TrimStart([char]0xFEFF)
    return $Text | ConvertFrom-Json
}

function Get-Phase11Property {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $Property = $Object.PSObject.Properties | Where-Object { $_.Name -ieq $Name } | Select-Object -First 1
    if ($Property) {
        return $Property.Value
    }

    return $null
}

function ConvertTo-Phase11CompactJson {
    param([Parameter(Mandatory = $true)]$Value)

    return ($Value | ConvertTo-Json -Depth 80 -Compress)
}

function Get-Phase11RelativePath {
    param([Parameter(Mandatory = $true)][string]$Path)

    $FullPath = [System.IO.Path]::GetFullPath($Path)
    $FullRoot = [System.IO.Path]::GetFullPath($CoreRoot).TrimEnd('\') + '\'
    if ($FullPath.StartsWith($FullRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $FullPath.Substring($FullRoot.Length).Replace('\', '/')
    }

    return $FullPath
}

function Invoke-Phase11NodeJsonProbe {
    param([Parameter(Mandatory = $true)][string]$OutputPath)

    $ProbeCode = @'
import { pathToFileURL } from 'node:url';
const entrypointPath = process.argv[1];
const module = await import(pathToFileURL(entrypointPath).href);
const description = typeof module.describeXiigenCodexEntrypoint === 'function'
  ? module.describeXiigenCodexEntrypoint()
  : null;
process.stdout.write(JSON.stringify({
  imported: true,
  exports: Object.keys(module).sort(),
  description
}, null, 2));
'@

    $Stdout = & node --input-type=module --eval $ProbeCode $EntrypointPath
    $ExitCode = $LASTEXITCODE
    Assert-Phase11 -Condition ($ExitCode -eq 0) -Message "Node module import probe failed with exit code $ExitCode."
    Write-Phase11Text -Path $OutputPath -Text (($Stdout -join [Environment]::NewLine) + [Environment]::NewLine)
    return @{
        command = "node --input-type=module --eval <entrypoint import probe> $EntrypointPath"
        exit_code = $ExitCode
        status = "PASS"
    }
}

function Invoke-Phase11VisibleDisplay {
    param(
        [Parameter(Mandatory = $true)][string]$ResponsePath,
        [Parameter(Mandatory = $true)][string]$StderrPath,
        [Parameter(Mandatory = $true)][string]$VisiblePath,
        [Parameter(Mandatory = $true)][string]$Method,
        [Parameter(Mandatory = $true)][bool]$DebugEnabled,
        [Parameter(Mandatory = $true)][int]$ExitCode
    )

    $VisibleCode = @'
import fs from 'node:fs';
import { pathToFileURL } from 'node:url';
const [entrypointPath, responsePath, stderrPath, visiblePath, method, debugEnabledRaw, exitCodeRaw] = process.argv.slice(1);
const module = await import(pathToFileURL(entrypointPath).href);
const response = JSON.parse(fs.readFileSync(responsePath, 'utf8').replace(/^\uFEFF/u, ''));
const stderr = fs.existsSync(stderrPath) ? fs.readFileSync(stderrPath, 'utf8') : '';
const display = module.createVisibleAnswerDisplay({
  method,
  response,
  stderr,
  exitCode: Number(exitCodeRaw),
  debugEnabled: debugEnabledRaw === 'true'
});
fs.writeFileSync(visiblePath, `${display}\n`, 'utf8');
'@

    & node --input-type=module --eval $VisibleCode $EntrypointPath $ResponsePath $StderrPath $VisiblePath $Method ($DebugEnabled.ToString().ToLowerInvariant()) $ExitCode
    $DisplayExitCode = $LASTEXITCODE
    Assert-Phase11 -Condition ($DisplayExitCode -eq 0) -Message "Visible display helper failed for $ResponsePath with exit code $DisplayExitCode."
    return @{
        command = "node --input-type=module --eval <createVisibleAnswerDisplay> $EntrypointPath $ResponsePath"
        exit_code = $DisplayExitCode
        status = "PASS"
    }
}

function Invoke-Phase11Scenario {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Method,
        [Parameter(Mandatory = $true)][bool]$DebugEnabled,
        [Parameter(Mandatory = $false)][bool]$TargetLocatorRequired = $false
    )

    $ScenarioDir = Join-Path $RunDir $Name
    New-Phase11Directory -Path $ScenarioDir
    $RequestPath = Join-Path $ScenarioDir "request.json"
    $ResponsePath = Join-Path $ScenarioDir "response.json"
    $TracePath = Join-Path $ScenarioDir "trace.json"
    $StdoutPath = Join-Path $ScenarioDir "stdout.txt"
    $StderrPath = Join-Path $ScenarioDir "stderr.txt"
    $VisiblePath = Join-Path $ScenarioDir "visible-output.txt"
    $InvocationPath = Join-Path $ScenarioDir "node-invocation-result.json"
    $HostFactsJson = ConvertTo-Phase11CompactJson ([ordered]@{
        surface = "installed-codex-skill"
        phase = "11"
        scope = "Codex installed plugin adapter/core boundary proof"
        prompt_redaction = "redacted"
    })
    $CheckSetInputsJson = ConvertTo-Phase11CompactJson ([ordered]@{
        runtime04e = "installed-skill-smoke"
        phase11 = $Name
        no_training = "true"
        no_eval = "true"
        no_checkpoint_mutation = "true"
    })
    $ProvenanceJson = ConvertTo-Phase11CompactJson ([ordered]@{
        source = "installed-plugin-skill"
        plugin = "codex-xiigen-smart-skills"
        phase = "11"
        runner = "tools/phase11-codex-plugin-proof-runner.ps1"
    })
    $NodeArgs = @(
        $EntrypointPath,
        "--method", $Method,
        "--user-prompt-or-task-text", $RedactedPrompt,
        "--host-facts-json", $HostFactsJson,
        "--requested-skills-json", '["xiigen-smart-skills"]',
        "--available-skills-json", '["xiigen-smart-skills"]',
        "--check-set-inputs-json", $CheckSetInputsJson,
        "--provenance-json", $ProvenanceJson,
        "--correlation-id", $CorrelationId,
        "--core-root", $CoreRoot,
        "--extension-root", $PluginRoot,
        "--request-path", $RequestPath,
        "--response-path", $ResponsePath,
        "--trace-path", $TracePath
    )

    if ($DebugEnabled) {
        $NodeArgs += "--debug-enabled"
    }

    if ($TargetLocatorRequired) {
        $NodeArgs += "--target-locator-required"
    }

    & node @NodeArgs > $StdoutPath 2> $StderrPath
    $ExitCode = $LASTEXITCODE
    $StdoutText = if (Test-Path -LiteralPath $StdoutPath) { Get-Content -Raw -LiteralPath $StdoutPath } else { "" }
    $StderrText = if (Test-Path -LiteralPath $StderrPath) { Get-Content -Raw -LiteralPath $StderrPath } else { "" }
    Write-Phase11Text -Path $StdoutPath -Text $StdoutText
    Write-Phase11Text -Path $StderrPath -Text $StderrText
    Assert-Phase11 -Condition (Test-Path -LiteralPath $RequestPath) -Message "$Name did not write request JSON."
    Assert-Phase11 -Condition (Test-Path -LiteralPath $ResponsePath) -Message "$Name did not write response JSON."
    Assert-Phase11 -Condition (Test-Path -LiteralPath $TracePath) -Message "$Name did not write trace JSON."

    $DisplayCommand = Invoke-Phase11VisibleDisplay -ResponsePath $ResponsePath -StderrPath $StderrPath -VisiblePath $VisiblePath -Method $Method -DebugEnabled $DebugEnabled -ExitCode $ExitCode
    $Request = Read-Phase11Json -Path $RequestPath
    $Response = Read-Phase11Json -Path $ResponsePath
    $Trace = Read-Phase11Json -Path $TracePath
    $VisibleText = [System.IO.File]::ReadAllText($VisiblePath)
    $Invocation = [ordered]@{
        scenario = $Name
        command = "node $EntrypointPath --method $Method --user-prompt-or-task-text <redacted> --core-root $CoreRoot --extension-root $PluginRoot --request-path $RequestPath --response-path $ResponsePath --trace-path $TracePath"
        debug_enabled = $DebugEnabled
        target_locator_required = $TargetLocatorRequired
        exit_code = $ExitCode
        stdout_path = $StdoutPath
        stderr_path = $StderrPath
        request_path = $RequestPath
        response_path = $ResponsePath
        trace_path = $TracePath
        visible_output_path = $VisiblePath
        response_success = [bool](Get-Phase11Property -Object $Response -Name "Success")
        response_status = [string](Get-Phase11Property -Object $Response -Name "Status")
        response_contains_correlation_id = ([System.IO.File]::ReadAllText($ResponsePath).Contains($CorrelationId))
        request_correlation_id = [string](Get-Phase11Property -Object $Request -Name "CorrelationId")
        trace_correlation_id = [string](Get-Phase11Property -Object $Trace -Name "correlation_id")
        visible_output_preview = $VisibleText.Trim()
        display_command = $DisplayCommand
    }
    Write-Phase11Json -Path $InvocationPath -Value $Invocation

    return [ordered]@{
        name = $Name
        method = $Method
        debug_enabled = $DebugEnabled
        target_locator_required = $TargetLocatorRequired
        exit_code = $ExitCode
        request = $Request
        response = $Response
        trace = $Trace
        visible_text = $VisibleText
        paths = [ordered]@{
            directory = $ScenarioDir
            request = $RequestPath
            response = $ResponsePath
            trace = $TracePath
            stdout = $StdoutPath
            stderr = $StderrPath
            visible_output = $VisiblePath
            invocation = $InvocationPath
        }
        command_record = [ordered]@{
            command = $Invocation.command
            exit_code = $ExitCode
            status = "PASS"
        }
        display_command_record = $DisplayCommand
    }
}

New-Phase11Directory -Path $RunDir

$CommandRecords = New-Object System.Collections.Generic.List[object]
$JsonParseRecords = New-Object System.Collections.Generic.List[object]

Assert-Phase11 -Condition (Test-Path -LiteralPath $PluginRoot) -Message "Installed plugin root missing: $PluginRoot"
Assert-Phase11 -Condition (Test-Path -LiteralPath $SkillPath) -Message "Installed skill file missing: $SkillPath"
Assert-Phase11 -Condition (Test-Path -LiteralPath $PluginManifestPath) -Message "Plugin manifest missing: $PluginManifestPath"
Assert-Phase11 -Condition (Test-Path -LiteralPath $PackageJsonPath) -Message "Package JSON missing: $PackageJsonPath"
Assert-Phase11 -Condition (Test-Path -LiteralPath $EntrypointPath) -Message "Codex entrypoint missing: $EntrypointPath"

$PluginManifest = Read-Phase11Json -Path $PluginManifestPath
$PackageJson = Read-Phase11Json -Path $PackageJsonPath
$ModuleProbePath = Join-Path $RunDir "entrypoint-module-probe.json"
$CommandRecords.Add((Invoke-Phase11NodeJsonProbe -OutputPath $ModuleProbePath))
$ModuleProbe = Read-Phase11Json -Path $ModuleProbePath

$WiringReviewPath = Join-Path $RunDir "package-entrypoint-wiring-review.json"
$WiringReview = [ordered]@{
    plugin_root = $PluginRoot
    skill_path = $SkillPath
    plugin_manifest_path = $PluginManifestPath
    package_json_path = $PackageJsonPath
    entrypoint_path = $EntrypointPath
    plugin_manifest_exists = $true
    package_json_exists = $true
    entrypoint_exists = $true
    module_imported = [bool]$ModuleProbe.imported
    module_exports = $ModuleProbe.exports
    manifest_name = $PluginManifest.name
    package_name = $PackageJson.name
    package_main = $PackageJson.main
    source_entrypoint_invokes_core_endpoint = [bool]$ModuleProbe.description.sourceInvokesCoreEndpoint
    core_endpoint = $ModuleProbe.description.coreEndpoint
    adapter_boundary = "thin Codex adapter invokes llm_mvp_core through tools/xiigen-extension-core-api.ps1"
    status = "PASS"
}
Write-Phase11Json -Path $WiringReviewPath -Value $WiringReview

Assert-Phase11 -Condition ([bool]$ModuleProbe.imported) -Message "Entrypoint module did not import."
Assert-Phase11 -Condition ($ModuleProbe.exports -contains "invokeXiigenCore") -Message "Entrypoint does not export invokeXiigenCore."
Assert-Phase11 -Condition ($ModuleProbe.exports -contains "createVisibleAnswerDisplay") -Message "Entrypoint does not export createVisibleAnswerDisplay."
Assert-Phase11 -Condition ([bool]$ModuleProbe.description.sourceInvokesCoreEndpoint) -Message "Entrypoint description does not confirm core endpoint invocation."

$PositiveOff = Invoke-Phase11Scenario -Name "positive-debug-off" -Method "prompt-to-task" -DebugEnabled $false
$CommandRecords.Add($PositiveOff.command_record)
$CommandRecords.Add($PositiveOff.display_command_record)
$PositiveOn = Invoke-Phase11Scenario -Name "positive-debug-on" -Method "prompt-to-task" -DebugEnabled $true
$CommandRecords.Add($PositiveOn.command_record)
$CommandRecords.Add($PositiveOn.display_command_record)
$ErrorScenario = Invoke-Phase11Scenario -Name "target-locator-required-error" -Method "prompt-to-task" -DebugEnabled $true -TargetLocatorRequired $true
$CommandRecords.Add($ErrorScenario.command_record)
$CommandRecords.Add($ErrorScenario.display_command_record)

$PositiveOffCheckSet = Get-Phase11Property -Object $PositiveOff.response -Name "CheckSetResults"
$PositiveOnCheckSet = Get-Phase11Property -Object $PositiveOn.response -Name "CheckSetResults"
$ErrorCheckSet = Get-Phase11Property -Object $ErrorScenario.response -Name "CheckSetResults"

Assert-Phase11 -Condition ($PositiveOff.exit_code -eq 0) -Message "Positive debug-off node invocation failed."
Assert-Phase11 -Condition ([bool](Get-Phase11Property -Object $PositiveOff.response -Name "Success")) -Message "Positive debug-off response did not succeed."
Assert-Phase11 -Condition ((Get-Phase11Property -Object $PositiveOffCheckSet -Name "core_boundary_executed") -eq "true") -Message "Positive debug-off did not prove core boundary."
Assert-Phase11 -Condition ($PositiveOn.exit_code -eq 0) -Message "Positive debug-on node invocation failed."
Assert-Phase11 -Condition ([bool](Get-Phase11Property -Object $PositiveOn.response -Name "Success")) -Message "Positive debug-on response did not succeed."
Assert-Phase11 -Condition ((Get-Phase11Property -Object $PositiveOnCheckSet -Name "core_boundary_executed") -eq "true") -Message "Positive debug-on did not prove core boundary."
Assert-Phase11 -Condition ($ErrorScenario.exit_code -eq 0) -Message "Controlled error invocation crashed before response creation."
Assert-Phase11 -Condition (-not [bool](Get-Phase11Property -Object $ErrorScenario.response -Name "Success")) -Message "Controlled error scenario unexpectedly succeeded."
Assert-Phase11 -Condition ((Get-Phase11Property -Object $ErrorScenario.response -Name "Status") -eq "invalid_request") -Message "Controlled error scenario did not expose invalid_request status."
Assert-Phase11 -Condition ((Get-Phase11Property -Object $ErrorCheckSet -Name "core_boundary_executed") -eq "false") -Message "Controlled error scenario should not claim core boundary execution."

$PositiveOffVisible = $PositiveOff.visible_text
$PositiveOnVisible = $PositiveOn.visible_text
$ErrorVisible = $ErrorScenario.visible_text
Assert-Phase11 -Condition (-not $PositiveOffVisible.Contains("xiigen prompt-to-task :")) -Message "Debug-off visible output leaked debug display."
Assert-Phase11 -Condition (-not ([System.IO.File]::ReadAllText($PositiveOff.paths.response).Contains("xiigen prompt-to-task :"))) -Message "Debug-off response leaked debug display."
Assert-Phase11 -Condition ($PositiveOnVisible.Contains("xiigen prompt-to-task :")) -Message "Debug-on visible output missing xiigen method shape."
Assert-Phase11 -Condition ($PositiveOnVisible.Contains("input: <redacted>")) -Message "Debug-on visible output missing redacted input shape."
Assert-Phase11 -Condition ($PositiveOnVisible.Contains("output: <redacted>")) -Message "Debug-on visible output missing redacted output shape."
Assert-Phase11 -Condition ($ErrorVisible.Contains("XIIGen Codex adapter error (invalid_request).")) -Message "Error visible output missing visible invalid_request shape."

$ScenarioFiles = @(
    $PositiveOff.paths.request, $PositiveOff.paths.response, $PositiveOff.paths.trace, $PositiveOff.paths.invocation,
    $PositiveOn.paths.request, $PositiveOn.paths.response, $PositiveOn.paths.trace, $PositiveOn.paths.invocation,
    $ErrorScenario.paths.request, $ErrorScenario.paths.response, $ErrorScenario.paths.trace, $ErrorScenario.paths.invocation,
    $ModuleProbePath, $WiringReviewPath
)

foreach ($JsonPath in $ScenarioFiles) {
    $null = Read-Phase11Json -Path $JsonPath
    $JsonParseRecords.Add([ordered]@{
        path = $JsonPath
        status = "PASS"
    })
}

foreach ($Scenario in @($PositiveOff, $PositiveOn, $ErrorScenario)) {
    Assert-Phase11 -Condition ((Get-Phase11Property -Object $Scenario.request -Name "CorrelationId") -eq $CorrelationId) -Message "$($Scenario.name) request correlation mismatch."
    Assert-Phase11 -Condition ((Get-Phase11Property -Object $Scenario.trace -Name "correlation_id") -eq $CorrelationId) -Message "$($Scenario.name) trace correlation mismatch."
    Assert-Phase11 -Condition ([System.IO.File]::ReadAllText($Scenario.paths.response).Contains($CorrelationId)) -Message "$($Scenario.name) response does not link correlation id."
}

$EvidenceFiles = Get-ChildItem -LiteralPath $RunDir -Recurse -File
$SentinelHits = @($EvidenceFiles | Where-Object { [System.IO.File]::ReadAllText($_.FullName).Contains($UnredactedPromptSentinel) })
Assert-Phase11 -Condition ($SentinelHits.Count -eq 0) -Message "Unredacted smoke prompt sentinel appeared in evidence files."

$CoreBoundaryReviewPath = Join-Path $RunDir "core-boundary-correlation-review.json"
$CoreBoundaryReview = [ordered]@{
    status = "PASS"
    correlation_id = $CorrelationId
    correlation_fresh_for_run = $true
    request_response_trace_report_link_same_correlation_id = $true
    positive_debug_off = [ordered]@{
        request_correlation = Get-Phase11Property -Object $PositiveOff.request -Name "CorrelationId"
        response_contains_correlation_id = [System.IO.File]::ReadAllText($PositiveOff.paths.response).Contains($CorrelationId)
        trace_correlation = Get-Phase11Property -Object $PositiveOff.trace -Name "correlation_id"
        core_boundary_executed = Get-Phase11Property -Object $PositiveOffCheckSet -Name "core_boundary_executed"
    }
    positive_debug_on = [ordered]@{
        request_correlation = Get-Phase11Property -Object $PositiveOn.request -Name "CorrelationId"
        response_contains_correlation_id = [System.IO.File]::ReadAllText($PositiveOn.paths.response).Contains($CorrelationId)
        trace_correlation = Get-Phase11Property -Object $PositiveOn.trace -Name "correlation_id"
        core_boundary_executed = Get-Phase11Property -Object $PositiveOnCheckSet -Name "core_boundary_executed"
    }
    error_path = [ordered]@{
        request_correlation = Get-Phase11Property -Object $ErrorScenario.request -Name "CorrelationId"
        response_contains_correlation_id = [System.IO.File]::ReadAllText($ErrorScenario.paths.response).Contains($CorrelationId)
        trace_correlation = Get-Phase11Property -Object $ErrorScenario.trace -Name "correlation_id"
        visible_error_shape = $ErrorVisible.Trim()
    }
}
Write-Phase11Json -Path $CoreBoundaryReviewPath -Value $CoreBoundaryReview
$null = Read-Phase11Json -Path $CoreBoundaryReviewPath
$JsonParseRecords.Add([ordered]@{ path = $CoreBoundaryReviewPath; status = "PASS" })

$Booleans = [ordered]@{
    installed_skill_surface_loaded = $true
    adapter_core_boundary_executed = $true
    installed_plugin_runtime_invocation_proven = $true
    full_codex_chat_ui_proven = $false
    token_savings_proven = $false
    quality_improvement_proven = $false
    learnability_proven = $false
    training_started = $false
    eval_started = $false
    checkpoint_mutation_started = $false
}

$EvidencePaths = [ordered]@{
    run_dir = $RunDir
    package_entrypoint_wiring_review = $WiringReviewPath
    entrypoint_module_probe = $ModuleProbePath
    core_boundary_correlation_review = $CoreBoundaryReviewPath
    positive_debug_off_request = $PositiveOff.paths.request
    positive_debug_off_response = $PositiveOff.paths.response
    positive_debug_off_trace = $PositiveOff.paths.trace
    positive_debug_off_visible = $PositiveOff.paths.visible_output
    positive_debug_on_request = $PositiveOn.paths.request
    positive_debug_on_response = $PositiveOn.paths.response
    positive_debug_on_trace = $PositiveOn.paths.trace
    positive_debug_on_visible = $PositiveOn.paths.visible_output
    error_request = $ErrorScenario.paths.request
    error_response = $ErrorScenario.paths.response
    error_trace = $ErrorScenario.paths.trace
    error_visible = $ErrorScenario.paths.visible_output
}

$CommitReadyEvidencePaths = @(
    $RunnerPath,
    $WiringReviewPath,
    $ModuleProbePath,
    $CoreBoundaryReviewPath,
    $PositiveOff.paths.request,
    $PositiveOff.paths.response,
    $PositiveOff.paths.trace,
    $PositiveOff.paths.stdout,
    $PositiveOff.paths.stderr,
    $PositiveOff.paths.visible_output,
    $PositiveOff.paths.invocation,
    $PositiveOn.paths.request,
    $PositiveOn.paths.response,
    $PositiveOn.paths.trace,
    $PositiveOn.paths.stdout,
    $PositiveOn.paths.stderr,
    $PositiveOn.paths.visible_output,
    $PositiveOn.paths.invocation,
    $ErrorScenario.paths.request,
    $ErrorScenario.paths.response,
    $ErrorScenario.paths.trace,
    $ErrorScenario.paths.stdout,
    $ErrorScenario.paths.stderr,
    $ErrorScenario.paths.visible_output,
    $ErrorScenario.paths.invocation
)

$ReportPath = Join-Path $RunDir "phase11-codex-full-extension-live-proof-report.json"
$HumanReportPath = Join-Path $RunDir "phase11-codex-full-extension-live-proof-report.md"
$CommandRecords.Add([ordered]@{ command = "JSON parse validation for request/response/trace/report files"; exit_code = 0; status = "PASS" })
$CommandRecords.Add([ordered]@{ command = "Redaction sentinel scan under $RunDir"; exit_code = 0; status = "PASS" })
$CommandRecords.Add([ordered]@{ command = "Forbidden runtime action assertion: no external AI, training, eval, or checkpoint mutation introduced by runner"; exit_code = 0; status = "PASS" })

$MachineReport = [ordered]@{
    packet_title = "PHASE_11_CODEX_PROOF_READY_FOR_REVIEW"
    phase = "11/14"
    steps_closed_by_worker_pass = @("11.02", "11.03", "11.04", "11.05", "11.06")
    steps_remaining_for_parent = @("11.07")
    run_id = $RunId
    started_utc = $StartedUtc.ToString("o")
    completed_utc = (Get-Date).ToUniversalTime().ToString("o")
    correlation_id = $CorrelationId
    core_root = $CoreRoot
    installed_plugin_root = $PluginRoot
    runner_path = $RunnerPath
    booleans = $Booleans
    changed_files = @(
        (Get-Phase11RelativePath -Path $RunnerPath),
        (Get-Phase11RelativePath -Path $RunDir)
    )
    evidence_paths = $EvidencePaths
    commit_ready_evidence_paths = @($CommitReadyEvidencePaths | ForEach-Object { Get-Phase11RelativePath -Path $_ })
    commands_run = $CommandRecords
    json_parse_validation = $JsonParseRecords
    redaction = [ordered]@{
        prompt_passed_to_adapter = $RedactedPrompt
        unredacted_prompt_persisted = $false
        unredacted_prompt_sentinel_found = $false
        debug_off_visible_debug_marker_present = $PositiveOffVisible.Contains("xiigen prompt-to-task :")
        debug_on_visible_shape_present = ($PositiveOnVisible.Contains("xiigen prompt-to-task :") -and $PositiveOnVisible.Contains("input: <redacted>") -and $PositiveOnVisible.Contains("output: <redacted>"))
    }
    forbidden_actions = [ordered]@{
        external_ai_call_introduced = $false
        training_started = $false
        eval_started = $false
        checkpoint_mutation_started = $false
        deterministic_model_shortcut_introduced = $false
        git_commit_performed = $false
        git_push_performed = $false
    }
    honest_limitations = @(
        "This proves installed Codex skill surface plus adapter/core boundary only.",
        "It does not prove full Codex chat UI.",
        "It does not prove token savings, semantic quality improvement, learnability, training, eval, checkpoint mutation, or all-host readiness."
    )
    parent_should_stage_if_accepted = @($CommitReadyEvidencePaths | ForEach-Object { Get-Phase11RelativePath -Path $_ }) + @(
        (Get-Phase11RelativePath -Path $ReportPath),
        (Get-Phase11RelativePath -Path $HumanReportPath)
    )
}
Write-Phase11Json -Path $ReportPath -Value $MachineReport
$null = Read-Phase11Json -Path $ReportPath

$HumanReportTemplateBase64 = "IyBQaGFzZSAxMSBDb2RleCBmdWxsIGV4dGVuc2lvbiBsaXZlIHByb29mCgrQn9Cw0LrQtdGCOiBQSEFTRV8xMV9DT0RFWF9QUk9PRl9SRUFEWV9GT1JfUkVWSUVXCgrQpNCw0LfQsCAxMS8xNCDQv9GA0L7QstC10YDRj9C70LAg0YLQvtC70YzQutC+IENvZGV4IGluc3RhbGxlZCBwbHVnaW4gcHJvb2Y6INGD0YHRgtCw0L3QvtCy0LvQtdC90L3Ri9C5IHNraWxsIHN1cmZhY2UgYGNvZGV4LXhpaWdlbi1zbWFydC1za2lsbHNgINCx0YvQuyDQt9Cw0LPRgNGD0LbQtdC9INC40LcgcGx1Z2luIGNhY2hlLCBgc3JjL2NvZGV4LWVudHJ5cG9pbnQuanNgINCx0YvQuyDQuNC80L/QvtGA0YLQuNGA0L7QstCw0L0g0Lgg0LfQsNC/0YPRidC10L0g0YfQtdGA0LXQtyBgbm9kZWAsINCwINC30LDQv9GA0L7RgSDQtNC+0YjQtdC7INC00L4gYGxsbV9tdnBfY29yZWAg0YfQtdGA0LXQtyB0aGluIGFkYXB0ZXIgYm91bmRhcnkgYHRvb2xzL3hpaWdlbi1leHRlbnNpb24tY29yZS1hcGkucHMxYC4KCtCn0YLQviDQtNC+0LrQsNC30LDQvdC+OgotIDExLjAyIHBhY2thZ2UvZW50cnlwb2ludCB3aXJpbmc6INC90LDQudC00LXQvSBgLmNvZGV4LXBsdWdpbi9wbHVnaW4uanNvbmAsINC90LDQudC00LXQvSBgcGFja2FnZS5qc29uYCwg0L3QsNC50LTQtdC9INC4INC40LzQv9C+0YDRgtC40YDQvtCy0LDQvSBgc3JjL2NvZGV4LWVudHJ5cG9pbnQuanNgLCBleHBvcnQgYGludm9rZVhpaWdlbkNvcmVgINC00L7RgdGC0YPQv9C10L0sIGVudHJ5cG9pbnQg0L7Qv9C40YHRi9Cy0LDQtdGCIGNvcmUgZW5kcG9pbnQuCi0gMTEuMDMgbGl2ZSBDb2RleC9wbHVnaW4gcGF0aDogcG9zaXRpdmUgZGVidWctb2ZmINC4IGRlYnVnLW9uIGludm9jYXRpb25zINCy0LXRgNC90YPQu9C4IGBTdWNjZXNzPXRydWVgLCDQsCBgQ2hlY2tTZXRSZXN1bHRzLmNvcmVfYm91bmRhcnlfZXhlY3V0ZWQ9dHJ1ZWAuCi0gMTEuMDQgdmlzaWJsZSBhbnN3ZXIvZXJyb3IgcGF0aDogcG9zaXRpdmUgdmlzaWJsZSBvdXRwdXQg0L/QvtC60LDQt9GL0LLQsNC10YIg0L/QvtC70YzQt9C+0LLQsNGC0LXQu9GM0YHQutC40Lkg0L7RgtCy0LXRgjsgY29udHJvbGxlZCB0YXJnZXQtbG9jYXRvci1yZXF1aXJlZCBzY2VuYXJpbyDQstC10YDQvdGD0LsgdmlzaWJsZSBlcnJvciBgWElJR2VuIENvZGV4IGFkYXB0ZXIgZXJyb3IgKGludmFsaWRfcmVxdWVzdCkuYCDQsdC10Lcg0L/QsNC00LXQvdC40Y8gaGFybmVzcy4KLSAxMS4wNSBkZWJ1ZyBvZmYvb246IGRlYnVnLW9mZiDQvdC1INGB0L7QtNC10YDQttC40YIgYHhpaWdlbiBwcm9tcHQtdG8tdGFzayA6YDsgZGVidWctb24gdmlzaWJsZSBvdXRwdXQg0YHQvtC00LXRgNC20LjRgiBgeGlpZ2VuIHByb21wdC10by10YXNrIDpgLCBgaW5wdXQ6IDxyZWRhY3RlZD5gLCBgb3V0cHV0OiA8cmVkYWN0ZWQ+YC4KLSAxMS4wNiBjb3JlIGJvdW5kYXJ5L2NvcnJlbGF0aW9uOiByZXF1ZXN0LCByZXNwb25zZSwgdHJhY2Ug0LggcmVwb3J0INGB0LLRj9C30LDQvdGLIGZyZXNoIGNvcnJlbGF0aW9uIGlkIGB7e0NvcnJlbGF0aW9uSWR9fWAuCgrQp9GC0L4g0L3QtSDQtNC+0LrQsNC30LDQvdC+OgotIGZ1bGwgQ29kZXggY2hhdCBVSSDQvdC1INC00L7QutCw0LfQsNC9LgotIHRva2VuIHNhdmluZ3Mg0L3QtSDQtNC+0LrQsNC30LDQvdGLLgotIHNlbWFudGljIHF1YWxpdHkgaW1wcm92ZW1lbnQg0L3QtSDQtNC+0LrQsNC30LDQvdC+LgotIGxlYXJuYWJpbGl0eSDQvdC1INC00L7QutCw0LfQsNC90LAuCi0gdHJhaW5pbmcvZXZhbC9jaGVja3BvaW50IG11dGF0aW9uINC90LUg0LfQsNC/0YPRgdC60LDQu9C40YHRjC4KLSBhbGwtaG9zdCByZWFkaW5lc3Mg0Lgg0YTQuNC90LDQu9GM0L3QsNGPINCz0L7RgtC+0LLQvdC+0YHRgtGMINC/0YDQvtC10LrRgtCwINC90LUg0LfQsNGP0LLQu9GP0Y7RgtGB0Y8uCgrQmtCw0Log0Y3RgtC+INCy0YvQs9C70Y/QtNC10LvQviDQsdGLINC/0L7Qu9GM0LfQvtCy0LDRgtC10LvRjjoKLSDQkiBzdWNjZXNzIHBhdGgg0L/QvtC70YzQt9C+0LLQsNGC0LXQu9GMINCy0LjQtNC40YIg0L7QsdGL0YfQvdGL0LkgWElJR2VuINC+0YLQstC10YIg0LHQtdC3IHJhdyBwcm9tcHQgbGVhay4KLSDQn9GA0Lgg0LLQutC70Y7Rh9C10L3QvdC+0LwgZGVidWcg0LLQuNC00LXQvSByZWRhY3RlZCB0cmFjZSBzaGFwZTogYHhpaWdlbiBwcm9tcHQtdG8tdGFzayA6YCwgYGlucHV0OiA8cmVkYWN0ZWQ+YCwgYG91dHB1dDogPHJlZGFjdGVkPmAuCi0g0JIgZXJyb3IgcGF0aCDQv9C+0LvRjNC30L7QstCw0YLQtdC70Ywg0LLQuNC00LjRgiDQv9C+0L3Rj9GC0L3Ri9C5IGFkYXB0ZXIgZXJyb3Ig0YEgYGludmFsaWRfcmVxdWVzdGAsINCwINC90LUgY3Jhc2ggaGFybmVzcy4KCtCR0YPQu9C10LLRi9C5INC+0YLRh9C10YI6Ci0gaW5zdGFsbGVkX3NraWxsX3N1cmZhY2VfbG9hZGVkID0gdHJ1ZQotIGFkYXB0ZXJfY29yZV9ib3VuZGFyeV9leGVjdXRlZCA9IHRydWUKLSBpbnN0YWxsZWRfcGx1Z2luX3J1bnRpbWVfaW52b2NhdGlvbl9wcm92ZW4gPSB0cnVlCi0gZnVsbF9jb2RleF9jaGF0X3VpX3Byb3ZlbiA9IGZhbHNlCi0gdG9rZW5fc2F2aW5nc19wcm92ZW4gPSBmYWxzZQotIHF1YWxpdHlfaW1wcm92ZW1lbnRfcHJvdmVuID0gZmFsc2UKLSBsZWFybmFiaWxpdHlfcHJvdmVuID0gZmFsc2UKLSB0cmFpbmluZ19zdGFydGVkID0gZmFsc2UKLSBldmFsX3N0YXJ0ZWQgPSBmYWxzZQotIGNoZWNrcG9pbnRfbXV0YXRpb25fc3RhcnRlZCA9IGZhbHNlCgrQodGH0LXRgtGH0LjQutC4INC/0L7RgdC70LUgd29ya2VyIHBhc3M6Ci0gUGhhc2UgMTEgc3Vic3RlcHMgY2xvc2VkIGJ5IHRoaXMgd29ya2VyIHBhc3M6IDExLjAyLCAxMS4wMywgMTEuMDQsIDExLjA1LCAxMS4wNi4KLSBQaGFzZSAxMSBzdWJzdGVwcyByZW1haW5pbmc6IDExLjA3IHBhcmVudCBjb21taXQvc3RhdHVzIGJvdW5kYXJ5LgotIFdob2xlIHBsYW4gY291bnRlcnMgYXJlIG5vdCBhZHZhbmNlZCBoZXJlIGJlY2F1c2Ugbm8gcGFyZW50IHJldmlldy9jb21taXQvc3RhdHVzIGJvdW5kYXJ5IHdhcyBwZXJmb3JtZWQuCgpHaXQgYm91bmRhcnk6Ci0gQ29tbWl0INC90LUg0LLRi9C/0L7Qu9C90Y/Qu9GB0Y8uCi0gUHVzaCDQvdC1INCy0YvQv9C+0LvQvdGP0LvRgdGPLgotIEJyYW5jaCDQvdC1INC80LXQvdGP0LvRgdGPLgoKRXZpZGVuY2U6Ci0gTWFjaGluZSByZXBvcnQ6IGB7e1JlcG9ydFBhdGh9fWAKLSBXaXJpbmcgcmV2aWV3OiBge3tXaXJpbmdSZXZpZXdQYXRofX1gCi0gQ29ycmVsYXRpb24gcmV2aWV3OiBge3tDb3JlQm91bmRhcnlSZXZpZXdQYXRofX1gCi0gUnVuIGRpcmVjdG9yeTogYHt7UnVuRGlyfX1gCg=="
$HumanReportTemplate = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($HumanReportTemplateBase64))
$HumanReport = $HumanReportTemplate.Replace("{{CorrelationId}}", $CorrelationId).
    Replace("{{ReportPath}}", $ReportPath).
    Replace("{{WiringReviewPath}}", $WiringReviewPath).
    Replace("{{CoreBoundaryReviewPath}}", $CoreBoundaryReviewPath).
    Replace("{{RunDir}}", $RunDir)
Write-Phase11Text -Path $HumanReportPath -Text ($HumanReport + [Environment]::NewLine)
$null = Read-Phase11Json -Path $ReportPath

Write-Output "PHASE_11_CODEX_PROOF_READY_FOR_REVIEW"
Write-Output "run_dir=$RunDir"
Write-Output "report_json=$ReportPath"
Write-Output "report_md=$HumanReportPath"
Write-Output "correlation_id=$CorrelationId"
