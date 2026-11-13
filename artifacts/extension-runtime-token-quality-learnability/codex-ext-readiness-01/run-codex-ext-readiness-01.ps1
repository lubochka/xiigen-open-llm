[CmdletBinding()]
param()

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$EvidenceRoot = $PSScriptRoot
$RepoRoot = "C:\Projects\codex-xiigen-smart-skills"
$CoreRoot = "C:\Projects\llm_mvp_core"
$Row23InputPath = Join-Path $CoreRoot "artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json"
$CommandRoot = Join-Path $EvidenceRoot "commands"
New-Item -ItemType Directory -Force -Path $CommandRoot | Out-Null

$Script:CommandIndex = 0
$Script:Results = @()

function ConvertTo-CommandLine {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Exe,

        [Parameter(Mandatory = $true)]
        [string[]]$Args
    )

    $parts = @($Exe) + $Args
    return ($parts | ForEach-Object {
        if ($_ -match '\s') {
            '"' + ($_ -replace '"', '\"') + '"'
        } else {
            $_
        }
    }) -join ' '
}

function Invoke-CapturedCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [string]$Exe,

        [Parameter(Mandatory = $true)]
        [string[]]$Args,

        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory
    )

    $Script:CommandIndex += 1
    $safeName = $Name -replace '[^A-Za-z0-9_.-]', '-'
    $prefix = "{0:00}-{1}" -f $Script:CommandIndex, $safeName
    $stdoutPath = Join-Path $CommandRoot "$prefix.stdout.txt"
    $stderrPath = Join-Path $CommandRoot "$prefix.stderr.txt"
    $metaPath = Join-Path $CommandRoot "$prefix.command.json"
    $startedAt = Get-Date -Format o
    $commandLine = ConvertTo-CommandLine -Exe $Exe -Args $Args

    Push-Location $WorkingDirectory
    try {
        & $Exe @Args > $stdoutPath 2> $stderrPath
        $exitCode = if ($null -ne $LASTEXITCODE) { [int]$LASTEXITCODE } else { 0 }
    } finally {
        Pop-Location
    }
    $finishedAt = Get-Date -Format o

    $record = [ordered]@{
        name = $Name
        command_line = $commandLine
        working_directory = $WorkingDirectory
        stdout_path = $stdoutPath
        stderr_path = $stderrPath
        exit_code = $exitCode
        started_at = $startedAt
        finished_at = $finishedAt
    }
    $record | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $metaPath -Encoding utf8
    $Script:Results += [pscustomobject]$record
    return $exitCode
}

function Read-JsonIfPresent {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        return $null
    }

    return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
}

function Get-JsonValue {
    param(
        [AllowNull()]
        [object]$Object,

        [Parameter(Mandatory = $true)]
        [string[]]$Names,

        [AllowNull()]
        [object]$Fallback = $null
    )

    if ($null -eq $Object) {
        return $Fallback
    }

    foreach ($name in $Names) {
        $property = $Object.PSObject.Properties[$name]
        if ($null -ne $property) {
            return $property.Value
        }
    }

    return $Fallback
}

$row23 = Get-Content -LiteralPath $Row23InputPath -Raw | ConvertFrom-Json
$promptText = [string]$row23.scenario_input.prompt_text
$promptId = [string]$row23.scenario_input.prompt_id
$runId = "codex-ext-readiness-01-" + (Get-Date -Format "yyyyMMdd-HHmmss")
$row23SmokeRoot = Join-Path $EvidenceRoot "row23-smoke"
$row23RequestPath = Join-Path $row23SmokeRoot "request.json"
$row23ResponsePath = Join-Path $row23SmokeRoot "response.json"
$row23TracePath = Join-Path $row23SmokeRoot "trace.json"
$installSmokeReportPath = Join-Path $EvidenceRoot "install-smoke\install-smoke-report.json"
$hostQaRoot = Join-Path $EvidenceRoot "host-08f"
$hostQaReportPath = Join-Path $hostQaRoot "HOST-08F-codex-plugin-app-qa.json"
$parserOutputRoot = Join-Path $EvidenceRoot "available-skills-parser"

$hostFactsJson = @{
    surface = "codex-extension-readiness-local-smoke"
    scope = "CODEX-EXT-READINESS-01"
    row23_prompt_id = $promptId
    evidence_level = "adapter_core_boundary_only"
} | ConvertTo-Json -Compress
$checkSetInputsJson = @{
    row23_prompt_id = $promptId
    work_order = "CODEX-EXT-READINESS-01"
    full_readiness_claim_locked = "true"
} | ConvertTo-Json -Compress
$provenanceJson = @{
    source = "CODEX-EXT-READINESS-01"
    row23_input_path = $Row23InputPath
    external_ai_used = $false
    network_used = $false
} | ConvertTo-Json -Compress

Invoke-CapturedCommand -Name "node-check-codex-entrypoint" -Exe "node.exe" -Args @(
    "--check",
    "src\codex-entrypoint.js"
) -WorkingDirectory $RepoRoot | Out-Null

Invoke-CapturedCommand -Name "install-smoke-source-wiring" -Exe "powershell.exe" -Args @(
    "-NoProfile",
    "-ExecutionPolicy",
    "Bypass",
    "-File",
    "scripts\install-smoke.ps1",
    "-Output",
    $installSmokeReportPath
) -WorkingDirectory $RepoRoot | Out-Null

Invoke-CapturedCommand -Name "host-08f-component-qa" -Exe "node.exe" -Args @(
    "test\host-qa\HOST-08F-codexComponentQA.js",
    "--output-root",
    $hostQaRoot,
    "--host-report",
    $hostQaReportPath,
    "--core-root",
    $CoreRoot
) -WorkingDirectory $RepoRoot | Out-Null

Invoke-CapturedCommand -Name "available-skills-parser-regression" -Exe "node.exe" -Args @(
    "test\host-qa\HOST-REPAIR-CODEX-AVAILABLE-SKILLS-PARSER.js",
    "--repo-root",
    $RepoRoot,
    "--core-root",
    $CoreRoot,
    "--output-root",
    $parserOutputRoot
) -WorkingDirectory $RepoRoot | Out-Null

Invoke-CapturedCommand -Name "row23-entrypoint-smoke" -Exe "node.exe" -Args @(
    "src\codex-entrypoint.js",
    "--method",
    "prompt-to-task",
    "--user-prompt-or-task-text",
    $promptText,
    "--host-facts-json",
    $hostFactsJson,
    "--requested-skills-json",
    '["xiigen-smart-skills"]',
    "--available-skills-json",
    '["xiigen-smart-skills"]',
    "--check-set-inputs-json",
    $checkSetInputsJson,
    "--provenance-json",
    $provenanceJson,
    "--correlation-id",
    $runId,
    "--debug-enabled",
    "--core-root",
    $CoreRoot,
    "--extension-root",
    $row23SmokeRoot,
    "--request-path",
    $row23RequestPath,
    "--response-path",
    $row23ResponsePath,
    "--trace-path",
    $row23TracePath
) -WorkingDirectory $RepoRoot | Out-Null

$installSmokeReport = Read-JsonIfPresent -Path $installSmokeReportPath
$hostQaReport = Read-JsonIfPresent -Path $hostQaReportPath
$parserReport = Read-JsonIfPresent -Path (Join-Path $parserOutputRoot "codex-available-skills-parser-repair-report.json")
$row23Request = Read-JsonIfPresent -Path $row23RequestPath
$row23Response = Read-JsonIfPresent -Path $row23ResponsePath
$row23Trace = Read-JsonIfPresent -Path $row23TracePath

$summary = [ordered]@{
    schema = "xiigen.codex_ext_readiness_01.runner_summary.v1"
    subagent_id_or_nickname = "CODEX-EXT-READINESS-01"
    generated_at_local = Get-Date -Format o
    run_id = $runId
    repo_root = $RepoRoot
    core_root = $CoreRoot
    row23_input_path = $Row23InputPath
    commands_run = $Script:Results
    all_command_exit_codes_zero = @($Script:Results | Where-Object { $_.exit_code -ne 0 }).Count -eq 0
    output_refs = [ordered]@{
        install_smoke_report = $installSmokeReportPath
        host_08f_report = $hostQaReportPath
        available_skills_parser_report = Join-Path $parserOutputRoot "codex-available-skills-parser-repair-report.json"
        row23_request = $row23RequestPath
        row23_response = $row23ResponsePath
        row23_trace = $row23TracePath
    }
    observed = [ordered]@{
        install_smoke_passed = [bool](Get-JsonValue -Object $installSmokeReport -Names @("passed") -Fallback $false)
        host_08f_passed = [bool](Get-JsonValue -Object $hostQaReport -Names @("passed") -Fallback $false)
        parser_passed = [bool]((Get-JsonValue -Object $parserReport -Names @("verdict") -Fallback "") -eq "PASS")
        row23_entrypoint_exit_code = [int](($Script:Results | Where-Object { $_.name -eq "row23-entrypoint-smoke" } | Select-Object -First 1).exit_code)
        row23_response_success = [bool](Get-JsonValue -Object $row23Response -Names @("Success", "success") -Fallback $false)
        row23_response_status = [string](Get-JsonValue -Object $row23Response -Names @("Status", "status") -Fallback "")
        row23_request_host_name = [string](Get-JsonValue -Object $row23Request -Names @("HostName", "host_name") -Fallback "")
        row23_request_host_type = [string](Get-JsonValue -Object $row23Request -Names @("HostType", "host_type") -Fallback "")
        row23_request_method = [string](Get-JsonValue -Object $row23Request -Names @("Method", "method") -Fallback "")
        row23_request_correlation_id = [string](Get-JsonValue -Object $row23Request -Names @("CorrelationId", "correlation_id") -Fallback "")
        row23_requested_skills = @(Get-JsonValue -Object $row23Request -Names @("RequestedSkills", "requested_skills") -Fallback @())
        row23_available_skills = @(Get-JsonValue -Object $row23Request -Names @("AvailableSkills", "available_skills") -Fallback @())
        row23_debug_enabled = [bool](Get-JsonValue -Object $row23Request -Names @("DebugEnabled", "debug_enabled") -Fallback $false)
        row23_trace_correlation_id = [string](Get-JsonValue -Object $row23Trace -Names @("CorrelationId", "correlation_id") -Fallback "")
        row23_semantic_quality_claimed = [bool](Get-JsonValue -Object $row23Response -Names @("SemanticQualityClaimed", "semantic_quality_claimed") -Fallback $false)
    }
    non_claims = [ordered]@{
        full_codex_chat_ui_proven = $false
        real_codex_path_b_ui_proven = $false
        token_savings_proven = $false
        quality_improvement_proven = $false
        learnability_or_training_proven = $false
        backup_export_import_continue_training_proven = $false
        rollback_or_target_safety_proven = $false
        final_dual_path_scenario_coverage_proven = $false
        external_ai_used = $false
        network_used = $false
        git_actions_performed = $false
    }
}

$summaryPath = Join-Path $EvidenceRoot "runner-summary.json"
$summary | ConvertTo-Json -Depth 24 | Set-Content -LiteralPath $summaryPath -Encoding utf8
$Script:Results | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath (Join-Path $EvidenceRoot "command-results.json") -Encoding utf8

if (-not $summary.all_command_exit_codes_zero) {
    exit 1
}

exit 0
