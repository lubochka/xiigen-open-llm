[CmdletBinding()]
param(
    [string]$CoreRoot = "C:\Projects\llm_mvp_core",
    [string]$ClaudeRepo = "C:\Projects\claude-xiigen-smart-skills"
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

function Ensure-Directory {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Force -Path $Path | Out-Null
    }
}

function Write-JsonFile {
    param(
        [Parameter(Mandatory = $true)]$Value,
        [Parameter(Mandatory = $true)][string]$Path
    )
    Ensure-Directory -Path (Split-Path -Parent $Path)
    $json = $Value | ConvertTo-Json -Depth 80
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, ($json -replace "`r`n", "`n") + "`n", $utf8NoBom)
}

function Read-JsonFile {
    param([Parameter(Mandatory = $true)][string]$Path)
    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Get-Prop {
    param($Object, [string]$Name)
    $property = $Object.PSObject.Properties | Where-Object { $_.Name -ieq $Name } | Select-Object -First 1
    if ($property) { return $property.Value }
    return $null
}

function ConvertTo-FlatText {
    param($Value)
    if ($null -eq $Value) { return "" }
    if ($Value -is [string]) { return $Value }
    return ($Value | ConvertTo-Json -Depth 80 -Compress)
}

function Invoke-ClaudeSource {
    param(
        [Parameter(Mandatory = $true)][string]$Method,
        [Parameter(Mandatory = $true)][bool]$DebugEnabled,
        [Parameter(Mandatory = $true)][string]$CaseRoot,
        [Parameter(Mandatory = $true)][string]$Prompt
    )

    Ensure-Directory -Path $CaseRoot
    $responsePath = Join-Path $CaseRoot "response.json"
    $tracePath = Join-Path $CaseRoot "trace.json"
    $stdoutPath = Join-Path $CaseRoot "stdout.log"
    $stderrPath = Join-Path $CaseRoot "stderr.log"
    $args = @(
        "src\claude-entrypoint.js",
        "-ConcreteTask", $Prompt,
        "-Prompt", $Prompt,
        "-Method", $Method,
        "-CoreRoot", $CoreRoot,
        "-OutputPath", $responsePath,
        "-TracePath", $tracePath,
        "-CheckSetJson", "[]"
    )
    if ($DebugEnabled) {
        $args += "-DebugEnabled"
    }

    $started = Get-Date
    Push-Location -LiteralPath $ClaudeRepo
    try {
        & node.exe @args 1> $stdoutPath 2> $stderrPath
        $exitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }
    $elapsedMs = [int]((Get-Date) - $started).TotalMilliseconds

    return [ordered]@{
        command = "node.exe src\claude-entrypoint.js <HOST-08E $Method debug=$DebugEnabled args>"
        exit_code = $exitCode
        elapsed_ms = $elapsedMs
        stdout_ref = $stdoutPath
        stderr_ref = $stderrPath
        response_ref = $responsePath
        trace_ref = $tracePath
        request_ref = Join-Path $CaseRoot "xiigen-claude-core-request.json"
        host_trace_ref = Join-Path $CaseRoot "xiigen-claude-host-trace.json"
    }
}

$runId = (Get-Date).ToUniversalTime().ToString("yyyyMMdd-HHmmss")
$runRoot = Join-Path $CoreRoot "artifacts\host-qa\HOST-08E\$runId"
$claudeProbeRoot = Join-Path $ClaudeRepo "reports\HOST-08E-probe\hook-command\$runId"
Ensure-Directory -Path $runRoot
Ensure-Directory -Path $claudeProbeRoot

$plugin = Read-JsonFile -Path (Join-Path $ClaudeRepo ".claude-plugin\plugin.json")
$package = Read-JsonFile -Path (Join-Path $ClaudeRepo "package.json")
$hookCommand = [string]$plugin.hooks.SessionStart[0].hooks[0].command
$manifestHookPointsToJs = ($hookCommand -match "node\.exe") -and ($hookCommand -match "src\\claude-entrypoint\.js")
$ps1AcceptedAsImplementation = [bool]$package.xiigenPhase40.ps1AcceptedAsImplementation

$sentinel = "HOST08E_CLAUDE_QA_SENTINEL_DO_NOT_DISPLAY"
$prompt = "HOST-08E synthetic non-private Claude hook QA prompt. $sentinel"
$caseSpecs = @(
    @{ label = "prompt-to-task-debug-off"; method = "prompt-to-task"; debug = $false },
    @{ label = "prompt-to-task-debug-on"; method = "prompt-to-task"; debug = $true },
    @{ label = "orchestration-plan-debug-off"; method = "orchestration-plan"; debug = $false },
    @{ label = "skill-selection-debug-off"; method = "skill-selection"; debug = $false }
)

$commands = New-Object System.Collections.ArrayList
$cases = New-Object System.Collections.ArrayList
$gaps = New-Object System.Collections.ArrayList
$passed = $manifestHookPointsToJs -and (-not $ps1AcceptedAsImplementation)

foreach ($spec in $caseSpecs) {
    $caseRoot = Join-Path $runRoot $spec.label
    $command = Invoke-ClaudeSource -Method $spec.method -DebugEnabled ([bool]$spec.debug) -CaseRoot $caseRoot -Prompt $prompt
    [void]$commands.Add($command)
    if ($command.exit_code -ne 0) {
        $passed = $false
        [void]$gaps.Add("$($spec.label) exited $($command.exit_code)")
    }

    $request = if (Test-Path -LiteralPath $command.request_ref) { Read-JsonFile -Path $command.request_ref } else { $null }
    $response = if (Test-Path -LiteralPath $command.response_ref) { Read-JsonFile -Path $command.response_ref } else { $null }
    $trace = if (Test-Path -LiteralPath $command.trace_ref) { Read-JsonFile -Path $command.trace_ref } else { $null }
    $hostTrace = if (Test-Path -LiteralPath $command.host_trace_ref) { Read-JsonFile -Path $command.host_trace_ref } else { $null }
    $stdoutText = if (Test-Path -LiteralPath $command.stdout_ref) { Get-Content -Raw -LiteralPath $command.stdout_ref } else { "" }
    $responseText = ConvertTo-FlatText -Value $response
    $traceText = ConvertTo-FlatText -Value $trace
    $hostTraceText = ConvertTo-FlatText -Value $hostTrace
    $sentinelAbsent = ($stdoutText -notmatch [regex]::Escape($sentinel)) -and ($responseText -notmatch [regex]::Escape($sentinel)) -and ($traceText -notmatch [regex]::Escape($sentinel)) -and ($hostTraceText -notmatch [regex]::Escape($sentinel))
    $debugShapePresent = $stdoutText -match "xiigen $($spec.method) :\s*`r?`ninput:\s*\{"
    $debugShapeAbsent = -not ($stdoutText -match "xiigen .* :\s*`r?`ninput:")
    $responseSuccess = [bool](Get-Prop -Object $response -Name "Success")
    $semanticQualityClaimed = [string](Get-Prop -Object $response -Name "semantic_quality_claimed")
    $runtimeExtensionClaimed = [string](Get-Prop -Object $response -Name "runtime_extension_claimed")

    if (-not $responseSuccess) {
        $passed = $false
        [void]$gaps.Add("$($spec.label) response was not successful")
    }
    if (-not $sentinelAbsent) {
        $passed = $false
        [void]$gaps.Add("$($spec.label) leaked sentinel outside request payload")
    }
    if ([bool]$spec.debug -and (-not $debugShapePresent)) {
        $passed = $false
        [void]$gaps.Add("$($spec.label) did not show expected debug shape")
    }
    if ((-not [bool]$spec.debug) -and (-not $debugShapeAbsent)) {
        $passed = $false
        [void]$gaps.Add("$($spec.label) displayed debug shape while disabled")
    }

    [void]$cases.Add([ordered]@{
        label = $spec.label
        method = $spec.method
        debug_enabled = [bool]$spec.debug
        exit_code = $command.exit_code
        response_success = $responseSuccess
        response_status = Get-Prop -Object $response -Name "Status"
        semantic_quality_claimed = $semanticQualityClaimed
        runtime_extension_claimed = $runtimeExtensionClaimed
        debug_shape_present = [bool]$debugShapePresent
        debug_shape_absent = [bool]$debugShapeAbsent
        sentinel_absent_from_display_response_trace = [bool]$sentinelAbsent
        request_contains_prompt_by_contract = ($null -ne $request)
        refs = [ordered]@{
            request_ref = $command.request_ref
            response_ref = $command.response_ref
            trace_ref = $command.trace_ref
            host_trace_ref = $command.host_trace_ref
            stdout_ref = $command.stdout_ref
            stderr_ref = $command.stderr_ref
        }
    })
}

$report = [ordered]@{
    schema_version = 1
    work_order = "HOST-08E-CLAUDE-HOOK-QA-AUTOMATION-AFTER-SOURCE-REPAIR"
    passed = [bool]$passed
    verdict = if ($passed) { "PASS_PENDING_PARENT_REVIEW" } else { "FAIL_NEEDS_REPAIR" }
    claim_scope = "Claude hook-command/source QA only; this does not claim full Claude chat UI evidence, final/cross-host completion, or semantic quality."
    run_id = $runId
    run_root = $runRoot
    claude_probe_root = $claudeProbeRoot
    hook_command_qa = $true
    claude_cli_plugin_qa = $false
    full_chat_ui_evidence = $false
    manifest_hook_points_to_js = [bool]$manifestHookPointsToJs
    ps1_accepted_as_implementation = [bool]$ps1AcceptedAsImplementation
    hook_command = $hookCommand
    source_entrypoint = "src/claude-entrypoint.js"
    core_endpoint = "C:/Projects/llm_mvp_core/tools/xiigen-extension-core-api.ps1"
    commands = $commands
    cases = $cases
    redaction = [ordered]@{
        sentinel = $sentinel
        raw_prompt_allowed_in_request_payload_only = $true
        sentinel_absent_from_display_response_trace = [bool](@($cases | Where-Object { -not $_.sentinel_absent_from_display_response_trace }).Count -eq 0)
    }
    thin_host_sweep = [ordered]@{
        semantic_ranking_in_host = $false
        model_logic_in_host = $false
        training_in_host = $false
        corpus_parsing_in_host = $false
        dpo_generation_in_host = $false
        model_mutation_in_host = $false
        json_ps1_report_substitute_accepted = $false
    }
    semantic_quality_claimed = $false
    runtime_extension_claimed = $false
    remaining_distinction = "HOST-08E covers Claude hook-command/source QA after JS repair. It is not full Claude chat UI evidence and cannot satisfy final/cross-host completion alone."
    gaps = $gaps
    forbidden_scope = [ordered]@{
        cursor_repo_edited = $false
        codex_repo_edited = $false
        training_model_corpus_dpo_started = $false
        model_backed_external_ai_used = $false
        final_cross_host_gate_started = $false
    }
}

$coreReportPath = Join-Path $runRoot "HOST-08E-claude-hook-qa.json"
$coreSummaryPath = Join-Path $runRoot "claude-hook-qa-command-summary.json"
$claudeReportPath = Join-Path $ClaudeRepo "reports\HOST-08E-claude-hook-qa.json"
Write-JsonFile -Value $report -Path $coreReportPath
Write-JsonFile -Value ([ordered]@{ run_id = $runId; commands = $commands; passed = [bool]$passed; gaps = $gaps }) -Path $coreSummaryPath
Write-JsonFile -Value $report -Path $claudeReportPath

if (-not $passed) {
    exit 1
}
