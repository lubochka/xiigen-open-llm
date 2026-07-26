param(
    [string]$RunId,
    [string]$CoreRoot = (Split-Path -Parent $PSScriptRoot),
    [string]$PluginRoot = (Join-Path $env:USERPROFILE '.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500'),
    [string]$OutputRoot = (Join-Path (Split-Path -Parent $PSScriptRoot) 'artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\codex-ui-proof')
)

$ErrorActionPreference = "Stop"
$Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding($false)

function New-Directory {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Force -Path $Path | Out-Null
    }
}

function ConvertTo-RepositoryText {
    param([AllowNull()][string]$Text)

    if ($null -eq $Text) {
        $Text = ""
    }

    $normalized = $Text -replace "`r`n", "`n" -replace "`r", "`n"
    $normalized = $normalized -replace "[ `t]+(?=`n)", ""
    $normalized = $normalized -replace "[ `t]+\z", ""

    if (-not $normalized.EndsWith("`n")) {
        $normalized += "`n"
    }

    return $normalized
}

function Write-RepositoryText {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [AllowNull()][string]$Text
    )

    New-Directory -Path (Split-Path -Parent $Path)
    [System.IO.File]::WriteAllText(
        $Path,
        (ConvertTo-RepositoryText -Text $Text),
        $Utf8NoBomEncoding)
}

function Read-JsonIfPresent {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return $null
    }

    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Write-Json {
    param(
        [Parameter(Mandatory = $true)]$Value,
        [Parameter(Mandatory = $true)][string]$Path
    )

    New-Directory -Path (Split-Path -Parent $Path)
    Write-RepositoryText -Path $Path -Text ($Value | ConvertTo-Json -Depth 64)
}

function ConvertTo-CompactJson {
    param([Parameter(Mandatory = $true)]$Value)

    return ($Value | ConvertTo-Json -Depth 64 -Compress)
}

function ConvertFrom-Utf8Base64 {
    param([Parameter(Mandatory = $true)][string]$Value)

    return [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($Value))
}

function Format-CommandLine {
    param([string[]]$Arguments)

    $quoted = foreach ($argument in $Arguments) {
        if ($argument -match '[\s"`{}[\],:]') {
            '"' + ($argument -replace '"', '\"') + '"'
        }
        else {
            $argument
        }
    }

    return "node " + ($quoted -join " ")
}

function Invoke-CodexEntrypoint {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Method,
        [Parameter(Mandatory = $true)][string]$PromptText,
        [Parameter(Mandatory = $true)]$HostFacts,
        [Parameter(Mandatory = $true)]$CheckSetInputs,
        [Parameter(Mandatory = $true)]$Provenance,
        [Parameter(Mandatory = $true)][string]$CorrelationId,
        [Parameter(Mandatory = $true)][string]$RequestPath,
        [Parameter(Mandatory = $true)][string]$ResponsePath,
        [Parameter(Mandatory = $true)][string]$TracePath,
        [Parameter(Mandatory = $true)][string]$CommandEvidenceDir
    )

    New-Directory -Path $CommandEvidenceDir

    $driverPath = Join-Path $CommandEvidenceDir "invoke-entrypoint-driver.mjs"
    $driverOptionsPath = Join-Path $CommandEvidenceDir "driver-options.json"
    $driverResultPath = Join-Path $CommandEvidenceDir "driver-result.json"
    $driverSource = @'
import fs from 'node:fs';
import { spawnSync } from 'node:child_process';
import { pathToFileURL } from 'node:url';

function readJsonIfPresent(filePath) {
  if (!fs.existsSync(filePath)) {
    return null;
  }

  return JSON.parse(fs.readFileSync(filePath, 'utf8').replace(/^\uFEFF/, ''));
}

function formatCommand(args) {
  return `node ${args.map((arg) => /[\s"`{}\[\],:]/u.test(arg) ? `"${arg.replaceAll('"', '\\"')}"` : arg).join(' ')}`;
}

const optionsPath = process.argv[2];
const options = JSON.parse(fs.readFileSync(optionsPath, 'utf8').replace(/^\uFEFF/, ''));
const args = [
  options.entrypointPath,
  '--method', options.method,
  '--user-prompt-or-task-text', options.promptText,
  '--host-facts-json', JSON.stringify(options.hostFacts),
  '--requested-skills-json', JSON.stringify(options.requestedSkills),
  '--available-skills-json', JSON.stringify(options.availableSkills),
  '--check-set-inputs-json', JSON.stringify(options.checkSetInputs),
  '--provenance-json', JSON.stringify(options.provenance),
  '--correlation-id', options.correlationId,
  '--debug-enabled',
  '--core-root', options.coreRoot,
  '--extension-root', '.',
  '--request-path', options.requestPath,
  '--response-path', options.responsePath,
  '--trace-path', options.tracePath
];

const child = spawnSync('node', args, {
  cwd: options.pluginRoot,
  encoding: 'utf8',
  windowsHide: true
});
const request = readJsonIfPresent(options.requestPath);
const response = readJsonIfPresent(options.responsePath);
const trace = readJsonIfPresent(options.tracePath);
let adapterVisibleAnswerDisplay = null;
let adapterDisplayError = null;

try {
  const entrypoint = await import(pathToFileURL(options.entrypointPath).href);
  adapterVisibleAnswerDisplay = entrypoint.createVisibleAnswerDisplay({
    method: options.method,
    response,
    exitCode: child.status,
    stderr: child.stderr,
    debugEnabled: true
  });
} catch (error) {
  adapterDisplayError = error instanceof Error ? error.message : String(error);
}

const driverResult = {
  name: options.name,
  exit_code: child.status,
  signal: child.signal,
  plugin_root: options.pluginRoot,
  entrypoint_path: options.entrypointPath,
  child_command: formatCommand(args),
  stdout: child.stdout ?? '',
  stderr: child.stderr ?? '',
  request_path: options.requestPath,
  response_path: options.responsePath,
  trace_path: options.tracePath,
  request,
  response,
  trace,
  adapter_visible_answer_display: adapterVisibleAnswerDisplay,
  adapter_display_error: adapterDisplayError
};

fs.writeFileSync(options.resultPath, `${JSON.stringify(driverResult, null, 2)}\n`, 'utf8');

if (child.stdout) {
  process.stdout.write(child.stdout);
}

if (child.stderr) {
  process.stderr.write(child.stderr);
}

process.exitCode = typeof child.status === 'number' ? child.status : 1;
'@
    Write-RepositoryText -Path $driverPath -Text $driverSource
    Write-Json -Path $driverOptionsPath -Value ([ordered]@{
        name = $Name
        pluginRoot = $PluginRoot
        entrypointPath = $entrypointPath
        method = $Method
        promptText = $PromptText
        hostFacts = $HostFacts
        requestedSkills = @("xiigen-smart-skills")
        availableSkills = @("xiigen-smart-skills")
        checkSetInputs = $CheckSetInputs
        provenance = $Provenance
        correlationId = $CorrelationId
        coreRoot = $CoreRoot
        requestPath = $RequestPath
        responsePath = $ResponsePath
        tracePath = $TracePath
        resultPath = $driverResultPath
    })

    $arguments = @($driverPath, $driverOptionsPath)
    $commandLine = Format-CommandLine -Arguments $arguments
    Write-RepositoryText -Path (Join-Path $CommandEvidenceDir "command.txt") -Text $commandLine

    $previousLocation = (Get-Location).Path
    $previousErrorActionPreference = $ErrorActionPreference
    try {
        Set-Location -LiteralPath $PluginRoot
        $ErrorActionPreference = "Continue"
        $outputLines = & node @arguments 2>&1
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
        Set-Location -LiteralPath $previousLocation
    }

    $combinedOutput = @($outputLines | ForEach-Object { [string]$_ })
    Write-RepositoryText -Path (Join-Path $CommandEvidenceDir "combined-output.txt") -Text ($combinedOutput -join "`n")
    $driverResult = Read-JsonIfPresent $driverResultPath
    $childCommand = if ($null -eq $driverResult) { $commandLine } else { [string]$driverResult.child_command }
    Write-Json -Path (Join-Path $CommandEvidenceDir "process-result.json") -Value ([ordered]@{
        name = $Name
        exit_code = $exitCode
        command_cwd = $PluginRoot
        command = $commandLine
        child_command = $childCommand
        combined_output_path = Join-Path $CommandEvidenceDir "combined-output.txt"
        driver_path = $driverPath
        driver_options_path = $driverOptionsPath
        driver_result_path = $driverResultPath
        request_path = $RequestPath
        response_path = $ResponsePath
        trace_path = $TracePath
    })

    return [ordered]@{
        name = $Name
        exit_code = $exitCode
        command = $childCommand
        runner_command = $commandLine
        combined_output = $combinedOutput
        command_evidence_dir = $CommandEvidenceDir
        driver_path = $driverPath
        driver_options_path = $driverOptionsPath
        driver_result_path = $driverResultPath
        driver_result = $driverResult
        request = Read-JsonIfPresent $RequestPath
        response = Read-JsonIfPresent $ResponsePath
        trace = Read-JsonIfPresent $TracePath
        request_path = $RequestPath
        response_path = $ResponsePath
        trace_path = $TracePath
    }
}

function Get-CheckSetValue {
    param(
        $Response,
        [Parameter(Mandatory = $true)][string]$Name
    )

    if ($null -eq $Response -or $null -eq $Response.CheckSetResults) {
        return $null
    }

    $property = $Response.CheckSetResults.PSObject.Properties | Where-Object { $_.Name -ieq $Name } | Select-Object -First 1
    if ($property) {
        return [string]$property.Value
    }

    return $null
}

function Test-ChatDisplayHasRedactedDebug {
    param($Response)

    if ($null -eq $Response -or [string]::IsNullOrWhiteSpace([string]$Response.ChatDisplayText)) {
        return $false
    }

    $text = [string]$Response.ChatDisplayText
    return $text.Contains("xiigen prompt-to-task :") -and
        $text.Contains("input:") -and
        $text.Contains("output:") -and
        $text.Contains("[redacted]")
}

function Test-ChatDisplayHasCorrelationId {
    param(
        $Response,
        [Parameter(Mandatory = $true)][string]$CorrelationId
    )

    if ($null -eq $Response -or [string]::IsNullOrWhiteSpace([string]$Response.ChatDisplayText)) {
        return $false
    }

    return ([string]$Response.ChatDisplayText).Contains($CorrelationId)
}

if ([string]::IsNullOrWhiteSpace($RunId)) {
    $RunId = "P14-FIX-02-" + (Get-Date).ToUniversalTime().ToString("yyyyMMdd-HHmmss")
}

$startedUtc = (Get-Date).ToUniversalTime().ToString("o")
$skillPath = Join-Path $PluginRoot "skills\xiigen-smart-skills\SKILL.md"
$entrypointPath = Join-Path $PluginRoot "src\codex-entrypoint.js"
$packagePath = Join-Path $PluginRoot "package.json"
$pluginEvidenceRoot = Join-Path $PluginRoot (Join-Path "reports\P14-FIX-02-codex-ui-proof" $RunId)
$runOutputRoot = Join-Path $OutputRoot $RunId
$positiveRoot = Join-Path $pluginEvidenceRoot "positive"
$negativeRoot = Join-Path $pluginEvidenceRoot "negative"
$positiveCommandDir = Join-Path $runOutputRoot "positive-command"
$negativeCommandDir = Join-Path $runOutputRoot "negative-command"

New-Directory -Path $positiveRoot
New-Directory -Path $negativeRoot
New-Directory -Path $runOutputRoot

$positiveCorrelationId = "$RunId-POSITIVE"
$negativeCorrelationId = "$RunId-NEGATIVE"
$safePrompt = "<redacted synthetic P14-FIX-02 smoke task>"

$positive = Invoke-CodexEntrypoint `
    -Name "positive-installed-skill-smoke" `
    -Method "prompt-to-task" `
    -PromptText $safePrompt `
    -HostFacts ([ordered]@{
        surface = "installed-codex-skill"
        scope = "P14-FIX-02"
        proof_attempt = "installed-plugin-adapter-core-boundary"
        full_codex_chat_ui_claimed = "false"
    }) `
    -CheckSetInputs ([ordered]@{
        runtime04e = "installed-skill-smoke"
        phase14_fix = "P14-FIX-02"
    }) `
    -Provenance ([ordered]@{
        source = "installed-plugin-skill"
        plugin = "codex-xiigen-smart-skills"
        phase = "14"
        fix = "P14-FIX-02"
    }) `
    -CorrelationId $positiveCorrelationId `
    -RequestPath (Join-Path $positiveRoot "request.json") `
    -ResponsePath (Join-Path $positiveRoot "response.json") `
    -TracePath (Join-Path $positiveRoot "trace.json") `
    -CommandEvidenceDir $positiveCommandDir

$negative = Invoke-CodexEntrypoint `
    -Name "negative-missing-required-target-locator" `
    -Method "prompt-to-task" `
    -PromptText $safePrompt `
    -HostFacts ([ordered]@{
        surface = "installed-codex-skill"
        scope = "P14-FIX-02"
        proof_attempt = "controlled-negative-validation"
        full_codex_chat_ui_claimed = "false"
    }) `
    -CheckSetInputs ([ordered]@{
        runtime04e = "installed-skill-smoke"
        phase14_fix = "P14-FIX-02"
        target_locator_required = "true"
    }) `
    -Provenance ([ordered]@{
        source = "installed-plugin-skill"
        plugin = "codex-xiigen-smart-skills"
        phase = "14"
        fix = "P14-FIX-02"
        negative_path = "target_locator_required_without_target_locator"
    }) `
    -CorrelationId $negativeCorrelationId `
    -RequestPath (Join-Path $negativeRoot "request.json") `
    -ResponsePath (Join-Path $negativeRoot "response.json") `
    -TracePath (Join-Path $negativeRoot "trace.json") `
    -CommandEvidenceDir $negativeCommandDir

$positiveCoreBoundary = (Get-CheckSetValue -Response $positive.response -Name "core_boundary_executed")
$positiveRuntimeClaim = (Get-CheckSetValue -Response $positive.response -Name "runtime_extension_claimed")
$positiveSemanticClaim = (Get-CheckSetValue -Response $positive.response -Name "semantic_quality_claimed")
$negativeCoreBoundary = (Get-CheckSetValue -Response $negative.response -Name "core_boundary_executed")
$negativeRuntimeClaim = (Get-CheckSetValue -Response $negative.response -Name "runtime_extension_claimed")
$negativeSemanticClaim = (Get-CheckSetValue -Response $negative.response -Name "semantic_quality_claimed")

$installedSkillSurfaceLoaded = (Test-Path -LiteralPath $skillPath) -and (Test-Path -LiteralPath $packagePath)
$adapterCoreBoundaryExecuted = $positive.exit_code -eq 0 -and
    $null -ne $positive.response -and
    [bool]$positive.response.Success -and
    $null -ne $positive.trace -and
    [string]$positive.trace.call_target -eq "SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest" -and
    [string]$positive.trace.external_ai_used -eq "False"
$installedPluginRuntimeInvocationProven = $adapterCoreBoundaryExecuted -and
    (Test-Path -LiteralPath $entrypointPath) -and
    [string]$positive.request.HostType -eq "codex" -and
    [string]$positive.request.Provenance.plugin -eq "codex-xiigen-smart-skills"
$positiveDebugRedacted = Test-ChatDisplayHasRedactedDebug -Response $positive.response
$negativeDebugRedacted = Test-ChatDisplayHasRedactedDebug -Response $negative.response
$positiveCorrelationVisible = Test-ChatDisplayHasCorrelationId -Response $positive.response -CorrelationId $positiveCorrelationId
$negativeCorrelationVisible = Test-ChatDisplayHasCorrelationId -Response $negative.response -CorrelationId $negativeCorrelationId
$negativeControlled = $negative.exit_code -eq 0 -and
    $null -ne $negative.response -and
    -not [bool]$negative.response.Success -and
    [string]$negative.response.Status -eq "invalid_request" -and
    (@($negative.response.Errors) -contains "target_locator_required")

$fullCodexChatUiProven = $false
$tokenSavingsProven = $false
$qualityImprovementProven = $false
$learnabilityProven = $false
$trainingStarted = $false
$evalStarted = $false
$checkpointMutationStarted = $false
$verdict = "NEEDS_FIX"
$nextAction = "Add a Codex host/chat/plugin UI harness or callable plugin command bridge that originates the request from the actual Codex chat/plugin surface, captures the user-visible answer/error/debug with correlation id, and reuses the same redaction and controlled-negative checks. Current evidence stops at installed plugin adapter -> llm_mvp_core core boundary."

$packetPath = Join-Path $runOutputRoot "p14-fix-02-codex-ui-proof-gap-packet.json"
$reportPath = Join-Path $runOutputRoot "p14-fix-02-codex-ui-proof-gap-report.ru.md"
$summaryPath = Join-Path $runOutputRoot "run-summary.json"

$packet = [ordered]@{
    schema_version = 1
    task = "P14-FIX-02 Codex chat/plugin UI proof or proof-gap packet"
    active_task_sentence_source = "latest unquoted Luba instruction was `"Continue please`", continuing the already-authorized no-stop execution chain"
    run_id = $RunId
    started_utc = $startedUtc
    completed_utc = (Get-Date).ToUniversalTime().ToString("o")
    verdict = $verdict
    fr_03_verdict = $verdict
    russian_first_report = $reportPath
    changed_files = @(
        $PSCommandPath,
        $packetPath,
        $reportPath,
        $summaryPath
    )
    generated_evidence_paths = @(
        $positive.request_path,
        $positive.response_path,
        $positive.trace_path,
        (Join-Path $positiveCommandDir "command.txt"),
        (Join-Path $positiveCommandDir "combined-output.txt"),
        (Join-Path $positiveCommandDir "process-result.json"),
        (Join-Path $positiveCommandDir "invoke-entrypoint-driver.mjs"),
        (Join-Path $positiveCommandDir "driver-options.json"),
        (Join-Path $positiveCommandDir "driver-result.json"),
        $negative.request_path,
        $negative.response_path,
        $negative.trace_path,
        (Join-Path $negativeCommandDir "command.txt"),
        (Join-Path $negativeCommandDir "combined-output.txt"),
        (Join-Path $negativeCommandDir "process-result.json"),
        (Join-Path $negativeCommandDir "invoke-entrypoint-driver.mjs"),
        (Join-Path $negativeCommandDir "driver-options.json"),
        (Join-Path $negativeCommandDir "driver-result.json"),
        $packetPath,
        $reportPath,
        $summaryPath
    )
    commands_checks_run = @(
        [ordered]@{
            name = "read installed skill instructions"
            command = "Get-Content -Raw $skillPath"
            result = "completed before script invocation by current Codex turn"
        },
        [ordered]@{
            name = "positive installed plugin adapter smoke"
            command = $positive.command
            cwd = $PluginRoot
            exit_code = $positive.exit_code
            response_success = if ($null -eq $positive.response) { $null } else { [bool]$positive.response.Success }
            response_status = if ($null -eq $positive.response) { $null } else { [string]$positive.response.Status }
            correlation_id = $positiveCorrelationId
        },
        [ordered]@{
            name = "controlled negative adapter/core validation path"
            command = $negative.command
            cwd = $PluginRoot
            exit_code = $negative.exit_code
            response_success = if ($null -eq $negative.response) { $null } else { [bool]$negative.response.Success }
            response_status = if ($null -eq $negative.response) { $null } else { [string]$negative.response.Status }
            expected_error = "target_locator_required"
            correlation_id = $negativeCorrelationId
        },
        [ordered]@{
            name = "packet synthesis"
            command = "$PSCommandPath"
            result = "report and JSON packet written"
        }
    )
    evidence_artifact_paths = @(
        $packetPath,
        $reportPath,
        $summaryPath,
        $positive.request_path,
        $positive.response_path,
        $positive.trace_path,
        $negative.request_path,
        $negative.response_path,
        $negative.trace_path
    )
    boundaries = [ordered]@{
        installed_skill_surface_loaded = $installedSkillSurfaceLoaded
        adapter_core_boundary_executed = $adapterCoreBoundaryExecuted
        installed_plugin_runtime_invocation_proven = $installedPluginRuntimeInvocationProven
        full_codex_chat_ui_proven = $fullCodexChatUiProven
        exact_missing_boundary = "No evidence path available in this turn that originates from the actual Codex chat/plugin UI surface and captures its rendered user-visible response. The fresh run invokes the installed plugin adapter directly with node from the plugin cache."
    }
    booleans = [ordered]@{
        installed_skill_surface_loaded = $installedSkillSurfaceLoaded
        adapter_core_boundary_executed = $adapterCoreBoundaryExecuted
        installed_plugin_runtime_invocation_proven = $installedPluginRuntimeInvocationProven
        full_codex_chat_ui_proven = $fullCodexChatUiProven
        token_savings_proven = $tokenSavingsProven
        quality_improvement_proven = $qualityImprovementProven
        learnability_proven = $learnabilityProven
        training_started = $trainingStarted
        eval_started = $evalStarted
        checkpoint_mutation_started = $checkpointMutationStarted
    }
    redaction_and_debug = [ordered]@{
        positive_chat_display_has_redacted_debug = $positiveDebugRedacted
        positive_chat_display_has_correlation_id = $positiveCorrelationVisible
        negative_chat_display_has_redacted_debug = $negativeDebugRedacted
        negative_chat_display_has_correlation_id = $negativeCorrelationVisible
        positive_adapter_visible_answer_display = if ($null -eq $positive.driver_result) { $null } else { [string]$positive.driver_result.adapter_visible_answer_display }
        negative_adapter_visible_answer_display = if ($null -eq $negative.driver_result) { $null } else { [string]$negative.driver_result.adapter_visible_answer_display }
        positive_user_prompt_sha256_present = $null -ne $positive.trace -and -not [string]::IsNullOrWhiteSpace([string]$positive.trace.user_prompt_sha256)
        positive_user_prompt_redacted_in_trace = $null -ne $positive.trace -and [string]$positive.trace.user_prompt_redacted -eq "True"
        positive_raw_private_prompt_persisted = $null -ne $positive.trace -and [string]$positive.trace.raw_private_prompt_persisted -eq "True"
        negative_expected_error_present = $negativeControlled
    }
    response_claims = [ordered]@{
        positive_core_boundary_executed = $positiveCoreBoundary
        positive_runtime_extension_claimed = $positiveRuntimeClaim
        positive_semantic_quality_claimed = $positiveSemanticClaim
        negative_core_boundary_executed = $negativeCoreBoundary
        negative_runtime_extension_claimed = $negativeRuntimeClaim
        negative_semantic_quality_claimed = $negativeSemanticClaim
    }
    safety = [ordered]@{
        no_external_ai_calls_beyond_local_adapter_path = $true
        git_authority = "forbidden"
        git_commands_run = @()
        no_git_commands_were_run = $true
        governance_skills_agents_claude_edited = $false
        checkpoints_mutated = $false
        final_readiness_claimed = $false
    }
    exact_next_action_if_needs_fix = $nextAction
}

$summary = [ordered]@{
    run_id = $RunId
    verdict = $verdict
    installed_skill_surface_loaded = $installedSkillSurfaceLoaded
    adapter_core_boundary_executed = $adapterCoreBoundaryExecuted
    installed_plugin_runtime_invocation_proven = $installedPluginRuntimeInvocationProven
    full_codex_chat_ui_proven = $fullCodexChatUiProven
    positive_status = if ($null -eq $positive.response) { $null } else { [string]$positive.response.Status }
    negative_status = if ($null -eq $negative.response) { $null } else { [string]$negative.response.Status }
    packet_path = $packetPath
    report_path = $reportPath
}

Write-Json -Value $packet -Path $packetPath
Write-Json -Value $summary -Path $summaryPath

$packetPathText = $packetPath
$summaryPathText = $summaryPath
$positiveRequestPathText = [string]$positive["request_path"]
$positiveResponsePathText = [string]$positive["response_path"]
$positiveTracePathText = [string]$positive["trace_path"]
$negativeRequestPathText = [string]$negative["request_path"]
$negativeResponsePathText = [string]$negative["response_path"]
$negativeTracePathText = [string]$negative["trace_path"]
$positiveCommandText = [string]$positive["command"]
$negativeCommandText = [string]$negative["command"]
$ruVerdictHeading = ConvertFrom-Utf8Base64 "0JLQtdGA0LTQuNC60YI="
$ruFreshParagraph = ConvertFrom-Utf8Base64 "0KHQstC10LbQuNC5INC30LDQv9GD0YHQuiDQtNC+0LrQsNC30LDQuyDRgtC+0LvRjNC60L4g0YPRgdGC0LDQvdC+0LLQu9C10L3QvdGL0LkgcGx1Z2luL3NraWxsIHNtb2tlOiBpbnN0YWxsZWQgcGx1Z2luIGFkYXB0ZXIgc3JjL2NvZGV4LWVudHJ5cG9pbnQuanMgY2FsbGVkIGxsbV9tdnBfY29yZSB0aHJvdWdoIHRvb2xzL3hpaWdlbi1leHRlbnNpb24tY29yZS1hcGkucHMxLCBhbmQgY29yZSByZXR1cm5lZCBwb3NpdGl2ZSBwbHVzIGNvbnRyb2xsZWQgbmVnYXRpdmUgcmVzcG9uc2VzIHdpdGggY29ycmVsYXRpb24gaWRzLiDQrdGC0L4g0L3QtSDQtNC+0LrQsNC30YvQstCw0LXRgiDQv9C+0LvQvdC+0YbQtdC90L3Ri9C5IENvZGV4IGNoYXQvYXBwIFVJIHJ1bnRpbWUsINC/0L7RgtC+0LzRgyDRh9GC0L4g0LfQsNC/0YDQvtGBINCx0YvQuyDQt9Cw0L/Rg9GJ0LXQvSDQvdCw0L/RgNGP0LzRg9GOINGH0LXRgNC10Lcgbm9kZSDQuNC3IGNhY2hlIHBsdWdpbiByb290LCDQsCDQvdC1INC40Lcg0YTQsNC60YLQuNGH0LXRgdC60L7QuSBDb2RleCBjaGF0L3BsdWdpbiBVSSDQv9C+0LLQtdGA0YXQvdC+0YHRgtC4Lg=="
$ruProvenHeading = ConvertFrom-Utf8Base64 "0KfRgtC+INC00L7QutCw0LfQsNC90L4="
$ruNotProvenHeading = ConvertFrom-Utf8Base64 "0KfRgtC+INC90LUg0LTQvtC60LDQt9Cw0L3Qvg=="

$report = @"
# P14-FIX-02 Codex UI proof-gap packet

## $ruVerdictHeading

NEEDS_FIX for Phase 14 FR-03.

$ruFreshParagraph

## $ruProvenHeading

- installed_skill_surface_loaded: $installedSkillSurfaceLoaded
- adapter_core_boundary_executed: $adapterCoreBoundaryExecuted
- installed_plugin_runtime_invocation_proven: $installedPluginRuntimeInvocationProven
- positive correlation id: $positiveCorrelationId
- negative correlation id: $negativeCorrelationId
- positive response status: $($summary.positive_status)
- negative response status: $($summary.negative_status)
- controlled negative expected error present: $negativeControlled
- redacted/debug display present in positive response: $positiveDebugRedacted
- redacted/debug display present in negative response: $negativeDebugRedacted

## $ruNotProvenHeading

- full_codex_chat_ui_proven: false
- token_savings_proven: false
- quality_improvement_proven: false
- learnability_proven: false
- training_started: false
- eval_started: false
- checkpoint_mutation_started: false

Exact missing boundary: no evidence path in this turn originated from the actual Codex chat/plugin UI surface and captured the rendered user-visible response/error/debug there. Current evidence stops at installed plugin adapter -> llm_mvp_core core boundary.

## Evidence paths

- Packet JSON: $packetPathText
- Summary JSON: $summaryPathText
- Positive request: $positiveRequestPathText
- Positive response: $positiveResponsePathText
- Positive trace: $positiveTracePathText
- Negative request: $negativeRequestPathText
- Negative response: $negativeResponsePathText
- Negative trace: $negativeTracePathText

## Commands/checks run

1. Read installed skill instructions: Get-Content -Raw $skillPath.
2. Positive installed plugin adapter smoke from ${PluginRoot}: $positiveCommandText.
3. Controlled negative adapter/core validation path from ${PluginRoot}: $negativeCommandText.
4. Packet synthesis: $PSCommandPath.

## Safety confirmations

- No git commands were run.
- No governance, skills, AGENTS, or CLAUDE files were edited.
- No external AI call was used by this diagnostic path; evidence is local adapter/core invocation only.
- No training/eval/checkpoint mutation was started.
- No final readiness claim is made.

## Exact next action

$nextAction
"@

Write-RepositoryText -Path $reportPath -Text $report

Write-Json -Value $packet -Path $packetPath

Write-Output "P14-FIX-02 packet written: $packetPath"
Write-Output "P14-FIX-02 report written: $reportPath"
Write-Output "P14-FIX-02 verdict: $verdict"
