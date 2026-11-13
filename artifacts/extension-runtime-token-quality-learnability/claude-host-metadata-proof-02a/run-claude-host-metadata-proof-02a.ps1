$ErrorActionPreference = 'Stop'

$EvidenceRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$StatePath = 'C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json'
$PlanPath = 'C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md'
$AgentsPath = 'C:\Projects\llm_mvp_core\AGENTS.md'
$Row23InputPath = 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json'
$RunnerPath = $MyInvocation.MyCommand.Path
$SubagentId = 'CLAUDE-HOST-METADATA-PROOF-02A'
$StateKey = 'claude_host_metadata_proof_02a_20260628'
$ClaudeExe = 'C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.187\claude.exe'
$PluginDir = 'C:\Projects\claude-xiigen-smart-skills'
$Now = (Get-Date).ToString('o')

function Invoke-CapturedCommand {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string[]]$Arguments
    )

    $stdoutPath = Join-Path $EvidenceRoot "$Name.stdout.txt"
    $stderrPath = Join-Path $EvidenceRoot "$Name.stderr.txt"
    $metaPath = Join-Path $EvidenceRoot "$Name.command.json"
    $started = Get-Date
    $record = [ordered]@{
        name = $Name
        executable = $FilePath
        arguments = $Arguments
        command_line = ($FilePath + ' ' + ($Arguments -join ' '))
        stdout = $stdoutPath
        stderr = $stderrPath
        command_meta = $metaPath
        started_at = $started.ToString('o')
        exit_code = $null
        duration_ms = $null
        launch_error = $null
    }

    try {
        if (-not (Test-Path -LiteralPath $FilePath)) {
            throw "Executable not found: $FilePath"
        }
        $process = Start-Process -FilePath $FilePath -ArgumentList $Arguments -WorkingDirectory $EvidenceRoot -RedirectStandardOutput $stdoutPath -RedirectStandardError $stderrPath -NoNewWindow -Wait -PassThru
        $record.exit_code = [int]$process.ExitCode
    }
    catch {
        $record.exit_code = $null
        $record.launch_error = $_.Exception.Message
        $_.Exception.Message | Set-Content -LiteralPath $stderrPath -Encoding UTF8
        '' | Set-Content -LiteralPath $stdoutPath -Encoding UTF8
    }

    $finished = Get-Date
    $record.finished_at = $finished.ToString('o')
    $record.duration_ms = [int]($finished - $started).TotalMilliseconds
    if (Test-Path -LiteralPath $stdoutPath) {
        $stdoutText = [System.IO.File]::ReadAllText($stdoutPath)
        $record.stdout_preview = ($stdoutText -replace "`r", '').Trim()
    }
    if (Test-Path -LiteralPath $stderrPath) {
        $stderrText = [System.IO.File]::ReadAllText($stderrPath)
        $record.stderr_preview = ($stderrText -replace "`r", '').Trim()
    }
    ($record | ConvertTo-Json -Depth 12) | Set-Content -LiteralPath $metaPath -Encoding UTF8
    return [pscustomobject]$record
}

$commands = @(
    [ordered]@{ name = '01-claude-version'; executable = $ClaudeExe; arguments = @('--version') },
    [ordered]@{ name = '02-plugin-validate'; executable = $ClaudeExe; arguments = @('plugin', 'validate', $PluginDir) },
    [ordered]@{ name = '03-plugin-details'; executable = $ClaudeExe; arguments = @('--plugin-dir', $PluginDir, 'plugin', 'details', 'claude-xiigen-smart-skills') }
)

$commandRecords = @()
foreach ($command in $commands) {
    $commandRecords += Invoke-CapturedCommand -Name $command.name -FilePath $command.executable -Arguments $command.arguments
}

$exitCodes = @($commandRecords | ForEach-Object { $_.exit_code })
$successfulCommands = @($commandRecords | Where-Object { $_.exit_code -eq 0 })
if ($successfulCommands.Count -eq $commandRecords.Count) {
    $hostMetadataStatus = 'PASS'
} elseif ($successfulCommands.Count -gt 0) {
    $hostMetadataStatus = 'PARTIAL'
} else {
    $hostMetadataStatus = 'BLOCKED'
}

if ($hostMetadataStatus -eq 'BLOCKED') {
    $claudeReadinessStatus = 'BLOCKED'
} else {
    $claudeReadinessStatus = 'PARTIAL'
}

$metadataBlockers = @()
foreach ($record in $commandRecords) {
    if ($record.exit_code -ne 0) {
        $metadataBlockers += "$($record.name) did not exit 0; exit_code=$($record.exit_code); launch_error=$($record.launch_error)"
    }
}

$remainingGates = @(
    'Real Claude live host chat/UI or approved real host invocation not run in this command-only packet.',
    'No live prompt submission, visible answer, visible error path, screenshot/transcript, or Path B same-scenario UI proof was collected.',
    'No external AI/chat prompt was sent and no network/UI/browser action was allowed or run.',
    'Token before/after, quality improvement, learnability/training, backup/export/import/share, continue-training, rollback, target promotion, Cursor, and Codex remain unproven by this packet.'
)

$blockers = @()
$blockers += $metadataBlockers
$blockers += $remainingGates

$stateTransition = [ordered]@{
    from = 'CLAUDE-EXT-READINESS-01 PARTIAL: prior state recorded local adapter/core/debug support plus missing Claude CLI path.'
    to = "CLAUDE-HOST-METADATA-PROOF-02A $hostMetadataStatus for metadata/plugin validation only; Claude readiness remains $claudeReadinessStatus because live host gates were not in scope."
    metadata_validation_claimed = ($hostMetadataStatus -eq 'PASS')
    full_claude_readiness_claimed = $false
    state_record_key = $StateKey
}

$nextScope = if ($hostMetadataStatus -eq 'PASS') {
    'CLAUDE-LIVE-HOST-RUNTIME-PROOF-02B: separately authorized bounded worker should prove real Claude host runtime/chat or exact host blocker with visible answer/error/debug/redaction/correlation and same-scenario Path B evidence; no secrets captured.'
} else {
    'CLAUDE-HOST-METADATA-PATH-REPAIR-02B: separately authorized bounded worker should repair the Claude CLI/plugin metadata blocker, then rerun version/validate/details before any live host proof.'
}

$exactFilesRead = @(
    $AgentsPath,
    $PlanPath,
    $StatePath,
    $Row23InputPath
)

$exactFilesWritten = @(
    $RunnerPath,
    (Join-Path $EvidenceRoot '01-claude-version.stdout.txt'),
    (Join-Path $EvidenceRoot '01-claude-version.stderr.txt'),
    (Join-Path $EvidenceRoot '01-claude-version.command.json'),
    (Join-Path $EvidenceRoot '02-plugin-validate.stdout.txt'),
    (Join-Path $EvidenceRoot '02-plugin-validate.stderr.txt'),
    (Join-Path $EvidenceRoot '02-plugin-validate.command.json'),
    (Join-Path $EvidenceRoot '03-plugin-details.stdout.txt'),
    (Join-Path $EvidenceRoot '03-plugin-details.stderr.txt'),
    (Join-Path $EvidenceRoot '03-plugin-details.command.json'),
    (Join-Path $EvidenceRoot 'CLAUDE-HOST-METADATA-PROOF-02A-packet.json'),
    (Join-Path $EvidenceRoot 'CLAUDE-HOST-METADATA-PROOF-02A-packet.md'),
    $StatePath
)

$packet = [ordered]@{
    schema = 'xiigen.claude_host_metadata_proof_02a.packet.v1'
    created_at_local = $Now
    subagent_id_or_nickname = $SubagentId
    changed_files = $exactFilesWritten
    exact_files_read = $exactFilesRead
    exact_files_written = $exactFilesWritten
    commands_run = $commandRecords
    host_metadata_status = $hostMetadataStatus
    claude_readiness_status = $claudeReadinessStatus
    claude_readiness_exact_gates = $remainingGates
    state_transition_made_or_not = $stateTransition
    next_bounded_worker_scope = $nextScope
    evidence_supports_only = 'The evidence supports only local Claude executable metadata availability plus plugin validate/details command results for C:\Projects\claude-xiigen-smart-skills. It does not prove live Claude chat/UI/runtime behavior, external AI response quality, token savings, semantic quality improvement, learnability/training, backup/export/import/share, continue-training, rollback, target promotion, Cursor, Codex, or final dual-path scenario coverage.'
    blockers = $blockers
    forbidden_actions_observed = [ordered]@{
        source_edits = $false
        live_chat_or_external_ai = $false
        ui_or_browser = $false
        network = $false
        installs = $false
        cursor_or_codex_execution = $false
        git = $false
    }
    parent_will_perform_commit_push_after_review = $true
    git_actions_performed_by_this_subagent = $false
    count_claimed = [ordered]@{
        metadata_commands_requested = 3
        metadata_commands_run = $commandRecords.Count
        metadata_commands_exit_zero = $successfulCommands.Count
        source_files_edited = 0
        live_chat_or_external_ai_calls = 0
        ui_or_browser_checks = 0
        network_calls = 0
        installs = 0
        git_actions = 0
    }
    independent_evidence_count = $commandRecords.Count
    downgrade_needed = ($claudeReadinessStatus -ne 'FULL')
}

$packetJsonPath = Join-Path $EvidenceRoot 'CLAUDE-HOST-METADATA-PROOF-02A-packet.json'
$packetMdPath = Join-Path $EvidenceRoot 'CLAUDE-HOST-METADATA-PROOF-02A-packet.md'
($packet | ConvertTo-Json -Depth 20) | Set-Content -LiteralPath $packetJsonPath -Encoding UTF8

$commandLines = ($commandRecords | ForEach-Object { "- $($_.name): exit_code=$($_.exit_code); stdout=$($_.stdout); stderr=$($_.stderr)" }) -join "`r`n"
$blockerLines = ($blockers | ForEach-Object { "- $_" }) -join "`r`n"
$md = @"
# CLAUDE-HOST-METADATA-PROOF-02A Packet

subagent_id_or_nickname: $SubagentId

host_metadata_status: $hostMetadataStatus

claude_readiness_status: $claudeReadinessStatus

commands_run:
$commandLines

state_transition_made_or_not: $($stateTransition.to)

next_bounded_worker_scope: $nextScope

evidence_supports_only: $($packet.evidence_supports_only)

blockers:
$blockerLines
"@
$md | Set-Content -LiteralPath $packetMdPath -Encoding UTF8

$stateRecord = [ordered]@{
    schema = 'xiigen.claude_host_metadata_proof_02a.state_transition.v1'
    subagent_id_or_nickname = $SubagentId
    timestamp = $Now
    status = $hostMetadataStatus
    evidence_packet = $packetJsonPath
    evidence_markdown = $packetMdPath
    evidence_root = $EvidenceRoot
    commands_run = $commandRecords
    host_metadata_status = $hostMetadataStatus
    claude_readiness_status = $claudeReadinessStatus
    gates = [ordered]@{
        claude_executable_version = if ($commandRecords[0].exit_code -eq 0) { 'PASS_METADATA_ONLY' } else { 'BLOCKED' }
        plugin_validate = if ($commandRecords[1].exit_code -eq 0) { 'PASS_METADATA_ONLY' } else { 'BLOCKED' }
        plugin_details = if ($commandRecords[2].exit_code -eq 0) { 'PASS_METADATA_ONLY' } else { 'BLOCKED' }
        live_claude_host_runtime = 'NOT_RUN_COMMAND_ONLY_SCOPE'
        live_chat_or_external_ai = 'NOT_RUN_FORBIDDEN'
        token_quality_learnability = 'NOT_PROVEN_BY_METADATA'
    }
    changed_files = $exactFilesWritten
    evidence_supports_only = $packet.evidence_supports_only
    next_bounded_worker_scope = $nextScope
    blockers = $blockers
    parent_will_perform_commit_push_after_review = $true
    git_actions_performed_by_this_subagent = $false
}

$stateText = [System.IO.File]::ReadAllText($StatePath)
if ($stateText -match ('"' + [regex]::Escape($StateKey) + '"\s*:')) {
    throw "State already contains $StateKey; refusing to create duplicate top-level record."
}
$stateJson = ($stateRecord | ConvertTo-Json -Depth 20)
$insert = ",`r`n    `"$StateKey`":  $stateJson`r`n"
$lastBrace = $stateText.LastIndexOf('}')
if ($lastBrace -lt 0) {
    throw 'Execution state file has no closing brace.'
}
$updatedState = $stateText.Substring(0, $lastBrace) + $insert + $stateText.Substring($lastBrace)
[System.IO.File]::WriteAllText($StatePath, $updatedState, [System.Text.Encoding]::UTF8)

($packet | ConvertTo-Json -Depth 20)
