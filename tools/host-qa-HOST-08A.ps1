[CmdletBinding()]
param(
    [string]$CoreRoot = "C:\Projects\llm_mvp_core",
    [string]$ClaudeRepo = "C:\Projects\claude-xiigen-smart-skills",
    [string]$CursorRepo = "C:\Projects\cursor-xiigen-smart-skills",
    [string]$CodexRepo = "C:\Projects\codex-xiigen-smart-skills"
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
    $Value | ConvertTo-Json -Depth 64 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Read-JsonFile {
    param([Parameter(Mandatory = $true)][string]$Path)
    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Assert-True {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )
    if (-not $Condition) {
        throw $Message
    }
}

function Get-Prop {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$Name
    )
    $property = $Object.PSObject.Properties | Where-Object { $_.Name -ieq $Name } | Select-Object -First 1
    if ($property) {
        return $property.Value
    }
    return $null
}

function ConvertTo-FlatText {
    param($Value)
    if ($null -eq $Value) {
        return ""
    }
    if ($Value -is [string]) {
        return $Value
    }
    return ($Value | ConvertTo-Json -Depth 64 -Compress)
}

function Invoke-NodeEntrypoint {
    param(
        [Parameter(Mandatory = $true)][string]$HostName,
        [Parameter(Mandatory = $true)][string]$WorkingDirectory,
        [Parameter(Mandatory = $true)][string]$ScriptPath,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$StdoutPath,
        [Parameter(Mandatory = $true)][string]$StderrPath
    )

    $start = Get-Date
    Push-Location -LiteralPath $WorkingDirectory
    try {
        $previousErrorActionPreference = $ErrorActionPreference
        $ErrorActionPreference = "Continue"
        & node.exe $ScriptPath @Arguments 1> $StdoutPath 2> $StderrPath
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
        Pop-Location
    }
    $elapsedMs = [int]((Get-Date) - $start).TotalMilliseconds
    return [ordered]@{
        host = $HostName
        command = "node.exe $ScriptPath <HOST-08A args>"
        exit_code = $exitCode
        elapsed_ms = $elapsedMs
        stdout_ref = $StdoutPath
        stderr_ref = $StderrPath
    }
}

function Test-CoreResponse {
    param(
        [Parameter(Mandatory = $true)][string]$HostName,
        [Parameter(Mandatory = $true)][string]$Method,
        [Parameter(Mandatory = $true)][string]$RequestPath,
        [Parameter(Mandatory = $true)][string]$ResponsePath,
        [Parameter(Mandatory = $true)][string]$TracePath,
        [Parameter(Mandatory = $true)][string]$Sentinel
    )
    Assert-True -Condition (Test-Path -LiteralPath $RequestPath) -Message "$HostName $Method request file missing."
    Assert-True -Condition (Test-Path -LiteralPath $ResponsePath) -Message "$HostName $Method response file missing."
    Assert-True -Condition (Test-Path -LiteralPath $TracePath) -Message "$HostName $Method trace file missing."

    $request = Read-JsonFile -Path $RequestPath
    $response = Read-JsonFile -Path $ResponsePath
    $trace = Read-JsonFile -Path $TracePath
    $responseText = ConvertTo-FlatText -Value $response
    $traceText = ConvertTo-FlatText -Value $trace

    Assert-True -Condition ((Get-Prop -Object $request -Name "Method") -eq $Method) -Message "$HostName $Method request method mismatch."
    Assert-True -Condition ($responseText -notmatch [regex]::Escape($Sentinel)) -Message "$HostName $Method response leaked sentinel."
    Assert-True -Condition ($traceText -notmatch [regex]::Escape($Sentinel)) -Message "$HostName $Method trace leaked sentinel."

    return [ordered]@{
        host = $HostName
        method = $Method
        request_ref = $RequestPath
        response_ref = $ResponsePath
        trace_ref = $TracePath
        response_success = [bool](Get-Prop -Object $response -Name "Success")
        response_status = Get-Prop -Object $response -Name "Status"
        runtime_extension_claimed = Get-Prop -Object $response -Name "RuntimeExtensionClaimed"
        semantic_quality_claimed = Get-Prop -Object $response -Name "SemanticQualityClaimed"
        sentinel_absent_from_response = $true
        sentinel_absent_from_trace = $true
    }
}

$runId = (Get-Date).ToUniversalTime().ToString("yyyyMMdd-HHmmss")
$runRoot = Join-Path $CoreRoot "artifacts\host-qa\HOST-08A\$runId"
Ensure-Directory -Path $runRoot

$sentinel = "HOST08A_SYNTHETIC_SENTINEL_DO_NOT_LEAK"
$fixturePrompt = "HOST-08A synthetic non-private prompt: turn a small release note into a concrete task. $sentinel"
$methods = @("prompt-to-task", "orchestration-plan", "skill-selection")
$results = New-Object System.Collections.ArrayList
$commands = New-Object System.Collections.ArrayList
$passed = $true
$gaps = New-Object System.Collections.ArrayList

foreach ($method in $methods) {
    $hostRunRoot = Join-Path $runRoot "claude-$method"
    Ensure-Directory -Path $hostRunRoot
    $responsePath = Join-Path $hostRunRoot "response.json"
    $tracePath = Join-Path $hostRunRoot "trace.json"
    $stdoutPath = Join-Path $hostRunRoot "stdout.log"
    $stderrPath = Join-Path $hostRunRoot "stderr.log"
    $command = Invoke-NodeEntrypoint -HostName "claude" -WorkingDirectory $ClaudeRepo -ScriptPath "src\claude-entrypoint.js" -StdoutPath $stdoutPath -StderrPath $stderrPath -Arguments @(
        "-ConcreteTask", $fixturePrompt,
        "-Method", $method,
        "-CoreRoot", $CoreRoot,
        "-OutputPath", $responsePath,
        "-TracePath", $tracePath,
        "-CheckSetJson", "[]"
    )
    [void]$commands.Add($command)
    if ($command.exit_code -ne 0) {
        $passed = $false
        [void]$gaps.Add("Claude $method source entrypoint exited $($command.exit_code).")
    }
    $requestPath = Join-Path $hostRunRoot "xiigen-claude-core-request.json"
    try {
        $caseResult = Test-CoreResponse -HostName "claude" -Method $method -RequestPath $requestPath -ResponsePath $responsePath -TracePath $tracePath -Sentinel $sentinel
        if (-not $caseResult.response_success) {
            $passed = $false
            [void]$gaps.Add("Claude $method core response was not successful: $($caseResult.response_status).")
        }
        [void]$results.Add($caseResult)
    } catch {
        $passed = $false
        [void]$gaps.Add($_.Exception.Message)
    }
}

foreach ($method in $methods) {
    $hostRunRoot = Join-Path $runRoot "codex-$method"
    Ensure-Directory -Path $hostRunRoot
    $requestPath = Join-Path $hostRunRoot "request.json"
    $responsePath = Join-Path $hostRunRoot "response.json"
    $tracePath = Join-Path $hostRunRoot "trace.json"
    $stdoutPath = Join-Path $hostRunRoot "stdout.log"
    $stderrPath = Join-Path $hostRunRoot "stderr.log"
    $command = Invoke-NodeEntrypoint -HostName "codex" -WorkingDirectory $CodexRepo -ScriptPath "src\codex-entrypoint.js" -StdoutPath $stdoutPath -StderrPath $stderrPath -Arguments @(
        "--method", $method,
        "--user-prompt-or-task-text", $fixturePrompt,
        "--core-root", $CoreRoot,
        "--request-path", $requestPath,
        "--response-path", $responsePath,
        "--trace-path", $tracePath
    )
    [void]$commands.Add($command)
    if ($command.exit_code -ne 0) {
        $passed = $false
        [void]$gaps.Add("Codex $method source entrypoint exited $($command.exit_code).")
    }
    try {
        $caseResult = Test-CoreResponse -HostName "codex" -Method $method -RequestPath $requestPath -ResponsePath $responsePath -TracePath $tracePath -Sentinel $sentinel
        if (-not $caseResult.response_success) {
            $passed = $false
            [void]$gaps.Add("Codex $method core response was not successful: $($caseResult.response_status).")
        }
        [void]$results.Add($caseResult)
    } catch {
        $passed = $false
        [void]$gaps.Add($_.Exception.Message)
    }
}

$cursorSource = Get-Content -Raw -LiteralPath (Join-Path $CursorRepo "src\extension.ts")
$cursorPackage = Get-Content -Raw -LiteralPath (Join-Path $CursorRepo "package.json") | ConvertFrom-Json
$cursorStatic = [ordered]@{
    host = "cursor"
    source_ref = "C:/Projects/cursor-xiigen-smart-skills/src/extension.ts"
    package_ref = "C:/Projects/cursor-xiigen-smart-skills/package.json"
    source_static_checked = $true
    reason_not_directly_invoked_in_HOST_08A = "Cursor entrypoint is a VS Code Extension Host activation module; direct source invocation would bypass the real host path. Later HOST-08 harness must use Extension Host QA automation."
    command_entrypoint_declared = ($cursorPackage.activationEvents -contains "onCommand:xiigen.smartSkills.select")
    chat_entrypoint_declared = ($cursorPackage.activationEvents -contains "onChatParticipant:xiigen-smart-skills")
    panel_entrypoint_declared = ($cursorPackage.activationEvents -contains "onCommand:xiigenSmartSkills.panel")
    current_request_fields_present = @(
        "HostName",
        "HostType",
        "Method",
        "UserPromptOrTaskText",
        "HostFacts",
        "DebugEnabled",
        "RequestedSkills",
        "AvailableSkills",
        "CheckSetInputs",
        "Provenance",
        "CorrelationId"
    ) | ForEach-Object { [ordered]@{ field = $_; present = ($cursorSource -match [regex]::Escape($_)) } }
    debug_shape_source_backed = ($cursorSource -match "xiigen \$\{request\.Method\} :\\ninput:")
    thin_host_flags_present = (($cursorSource -match "host_semantic_ranking") -and ($cursorSource -match "host_training") -and ($cursorSource -match "host_corpus_parsing"))
}

foreach ($fieldResult in $cursorStatic.current_request_fields_present) {
    if (-not $fieldResult.present) { $passed = $false }
}
$passed = $passed -and $cursorStatic.command_entrypoint_declared -and $cursorStatic.chat_entrypoint_declared -and $cursorStatic.panel_entrypoint_declared -and $cursorStatic.debug_shape_source_backed -and $cursorStatic.thin_host_flags_present
if (-not $cursorStatic.command_entrypoint_declared -or -not $cursorStatic.chat_entrypoint_declared -or -not $cursorStatic.panel_entrypoint_declared -or -not $cursorStatic.debug_shape_source_backed -or -not $cursorStatic.thin_host_flags_present) {
    [void]$gaps.Add("Cursor static admission check failed.")
}

$report = [ordered]@{
    schema_version = 1
    work_order = "HOST-08A-QA-SCENARIO-AUTOMATION-ADMISSION-AND-FIRST-HARNESS"
    passed = [bool]$passed
    verdict = if ($passed) { "PASS_PENDING_PARENT_REVIEW" } else { "FAIL_NEEDS_REPAIR" }
    claim_scope = "First QA/scenario automation admission harness only. Claude/Codex direct source entrypoints were invoked through the current core endpoint; Cursor was statically admitted and requires later VS Code Extension Host QA automation. No final/cross-host completion claim."
    run_id = $runId
    run_root = $runRoot
    fixture = [ordered]@{
        prompt_kind = "synthetic non-private"
        sentinel = $sentinel
        raw_prompt_allowed_location = "request files only"
    }
    commands = $commands
    source_entrypoint_results = $results
    cursor_static_admission = $cursorStatic
    scenarios_covered = @(
        "prompt-to-task via Claude and Codex source entrypoints",
        "task-to-orchestration-plan via Claude and Codex source entrypoints",
        "task-to-skills via Claude and Codex source entrypoints",
        "debug disabled baseline through source entrypoints",
        "Cursor command/chat/panel source/package admission for later Extension Host QA",
        "no host semantic ranking/training/corpus/DPO/model mutation authority",
        "no JSON/PS1/report substitute accepted"
    )
    scenarios_remaining = @(
        "VS Code/Cursor Extension Host command/chat/panel QA automation",
        "Claude app/plugin hook QA automation beyond direct source entrypoint",
        "Codex plugin/app runtime QA automation beyond direct source entrypoint",
        "debug enabled UI/chat visible trace through real host paths",
        "trained semantic quality e2e only if model-backed quality is claimed"
    )
    gaps = $gaps
    next_bounded_fix_if_failed = "HOST-08A-REPAIR-CODEX-HARNESS-REQUEST-SHAPE | code/check | Repair Codex source request/response handling exposed by HOST-08A first harness, then rerun HOST-08A harness | Codex host source or core harness only as parent authorizes | <=15 min | training/model/corpus/DPO/final gates"
    forbidden_scope = [ordered]@{
        final_cross_host_gate_started = $false
        training_model_corpus_dpo_started = $false
        broad_dotnet_tests_run = $false
        model_backed_external_ai_used = $false
        host_source_edited = $false
    }
}

$reportPath = Join-Path $CoreRoot "artifacts\host-qa\HOST-08A\HOST-08A-first-harness-report.json"
Write-JsonFile -Value $report -Path $reportPath

if (-not $passed) {
    exit 1
}

exit 0
