$ErrorActionPreference = 'Stop'

$root = 'C:\Projects\llm_mvp_core'
$evidenceDir = Join-Path $root 'artifacts\extension-runtime-token-quality-learnability\claude-host-metadata-proof-02b'
$statePath = Join-Path $root 'docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json'
$planPath = Join-Path $root 'docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md'
$agentsPath = Join-Path $root 'AGENTS.md'
$packetPath = Join-Path $evidenceDir 'claude-host-metadata-proof-02b.packet.json'
$mdPath = Join-Path $evidenceDir 'claude-host-metadata-proof-02b.packet.md'
$scriptPath = $PSCommandPath
$timestamp = (Get-Date).ToString('o')

function Read-TrimmedFile([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) { return $null }
    return (Get-Content -LiteralPath $Path -Raw).Trim()
}

$validateStdout = Join-Path $evidenceDir 'plugin_validate.stdout.txt'
$validateStderr = Join-Path $evidenceDir 'plugin_validate.stderr.txt'
$validateExitPath = Join-Path $evidenceDir 'plugin_validate.exit-code.txt'
$detailsStdout = Join-Path $evidenceDir 'plugin_details.stdout.txt'
$detailsStderr = Join-Path $evidenceDir 'plugin_details.stderr.txt'
$detailsExitPath = Join-Path $evidenceDir 'plugin_details.exit-code.txt'

$validateExit = [int](Read-TrimmedFile $validateExitPath)
$detailsExit = [int](Read-TrimmedFile $detailsExitPath)
$bothPassed = ($validateExit -eq 0 -and $detailsExit -eq 0)
$hostMetadataStatus = if ($bothPassed) { 'PASS' } else { 'BLOCKED' }
$claudeReadinessStatus = if ($bothPassed) { 'PARTIAL' } else { 'BLOCKED' }

$commandsRun = @(
    [ordered]@{
        id = 'plugin_validate'
        command = 'C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.187\claude.exe plugin validate C:\Projects\claude-xiigen-smart-skills'
        exit_code = $validateExit
        stdout_path = $validateStdout
        stderr_path = $validateStderr
        exit_code_path = $validateExitPath
        stdout_excerpt = Read-TrimmedFile $validateStdout
        stderr_excerpt = Read-TrimmedFile $validateStderr
    },
    [ordered]@{
        id = 'plugin_details'
        command = 'C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.187\claude.exe --plugin-dir C:\Projects\claude-xiigen-smart-skills plugin details claude-xiigen-smart-skills'
        exit_code = $detailsExit
        stdout_path = $detailsStdout
        stderr_path = $detailsStderr
        exit_code_path = $detailsExitPath
        stdout_excerpt = Read-TrimmedFile $detailsStdout
        stderr_excerpt = Read-TrimmedFile $detailsStderr
    }
)

$nonClaims = @(
    'No live Claude chat/UI prompt was run.',
    'No external AI/model task-solving was invoked.',
    'No Claude Desktop/Appx visible conversation readiness was proven.',
    'No token savings were measured or claimed.',
    'No quality improvement or semantic quality was measured or claimed.',
    'No learnability, training, DPO, eval, checkpoint mutation, or target promotion was proven.',
    'No full Claude/Cursor/Codex extension readiness was proven.',
    'No source files, Claude/Cursor/Codex repos, git, network, installs, UI, browser, or Codex/Cursor execution were touched by this packet.'
)

$exactFilesRead = @(
    $agentsPath,
    $planPath,
    $statePath,
    $validateStdout,
    $validateStderr,
    $validateExitPath,
    $detailsStdout,
    $detailsStderr,
    $detailsExitPath
)

$exactFilesWritten = @(
    $scriptPath,
    $packetPath,
    $mdPath,
    $statePath,
    $validateStdout,
    $validateStderr,
    $validateExitPath,
    $detailsStdout,
    $detailsStderr,
    $detailsExitPath
)

$evidenceSupportsOnly = 'Claude Code 2.1.187 plugin validate accepted the claude-xiigen-smart-skills manifest, and plugin details loaded inline metadata showing the plugin name, version, inventory, SessionStart hook, and projected token cost. This proves narrow Claude host plugin metadata only; it does not prove live Claude chat/UI/runtime task flow, external-AI behavior, token savings, quality improvement, learnability, training, checkpoint mutation, or full readiness.'

$stateTransition = if ($bothPassed) {
    'MADE: added top-level claude_host_metadata_proof_02b packet with host_metadata_status PASS and claude_readiness_status PARTIAL.'
} else {
    'MADE: added top-level claude_host_metadata_proof_02b packet with host_metadata_status BLOCKED and claude_readiness_status BLOCKED.'
}

$nextBoundedWorkerScope = 'Parent review of this narrow metadata packet; then, if still required by the active plan, a separate bounded Claude host runtime/chat/UI proof or blocker packet that preserves no-external-AI and no-overclaim gates.'

$statePacket = [ordered]@{
    schema = 'xiigen.claude_host_metadata_proof_02b.state_packet.v1'
    subagent_id_or_nickname = 'CLAUDE-HOST-METADATA-PROOF-02B'
    timestamp = $timestamp
    evidence_packet_path = $packetPath
    markdown_packet_path = $mdPath
    commands_run = $commandsRun
    host_metadata_status = $hostMetadataStatus
    claude_readiness_status = $claudeReadinessStatus
    claude_readiness_exact_gates = @(
        'PASS only for plugin validate/details metadata when both commands exit 0.',
        'PARTIAL remains because active plan evidence says full readiness is not claimed and no current packet proves real Claude host runtime plus final gates.',
        'FULL would require separate proof for live Claude host/chat/UI/runtime path and remaining final gates.'
    )
    state_transition_made_or_not = $stateTransition
    non_claims = $nonClaims
    evidence_supports_only = $evidenceSupportsOnly
    next_bounded_worker_scope = $nextBoundedWorkerScope
    parent_will_perform_commit_push_after_review = $true
}

if (Select-String -LiteralPath $statePath -Pattern '"claude_host_metadata_proof_02b"' -Quiet) {
    throw 'State already contains claude_host_metadata_proof_02b; refusing duplicate append.'
}

$stateJson = $statePacket | ConvertTo-Json -Depth 12
$encoding = [System.Text.UTF8Encoding]::new($false)
$fs = [System.IO.File]::Open($statePath, [System.IO.FileMode]::Open, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None)
try {
    $pos = $fs.Length - 1
    $last = -1
    while ($pos -ge 0) {
        $fs.Position = $pos
        $last = $fs.ReadByte()
        if ($last -notin @(9, 10, 13, 32)) { break }
        $pos--
    }
    if ($last -ne 125) { throw 'Execution state does not end with a top-level JSON object close brace.' }
    $fs.SetLength($pos)
    $fs.Position = $pos
    $suffix = ",`r`n    `"claude_host_metadata_proof_02b`": $stateJson`r`n}`r`n"
    $bytes = $encoding.GetBytes($suffix)
    $fs.Write($bytes, 0, $bytes.Length)
}
finally {
    $fs.Dispose()
}

$stream = [System.IO.File]::OpenRead($statePath)
try {
    $doc = [System.Text.Json.JsonDocument]::Parse($stream)
    $doc.Dispose()
    $stateJsonValidation = 'PASS'
}
finally {
    $stream.Dispose()
}

$packet = [ordered]@{
    schema = 'xiigen.claude_host_metadata_proof_02b.packet.v1'
    subagent_id_or_nickname = 'CLAUDE-HOST-METADATA-PROOF-02B'
    timestamp = $timestamp
    active_luba_quote_creating_authority = 'Не понимаю почему снова стоп??? Найди и эту лазейку пожалуйста. Занеси это и действия внизу в план работы- сделай план работы полным по нашим стандартам. Сделай 20 проходов и убедись что план доведёт наконец необходимые extension до полной готовности. И приступайк действию до полной готовности всех 3 extensions. NO STOPS approved!!! All through agents!!!'
    changed_files = @(
        $statePath,
        $packetPath,
        $mdPath,
        $scriptPath,
        $validateStdout,
        $validateStderr,
        $validateExitPath,
        $detailsStdout,
        $detailsStderr,
        $detailsExitPath
    )
    exact_files_read = $exactFilesRead
    exact_files_written = $exactFilesWritten
    commands_run = $commandsRun
    host_metadata_status = $hostMetadataStatus
    claude_readiness_status = $claudeReadinessStatus
    claude_readiness_exact_gates = $statePacket.claude_readiness_exact_gates
    state_transition_made_or_not = $stateTransition
    state_json_validation = $stateJsonValidation
    next_bounded_worker_scope = $nextBoundedWorkerScope
    evidence_supports_only = $evidenceSupportsOnly
    blockers = @()
    non_claims = $nonClaims
}

$packet | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $packetPath -Encoding utf8

$md = @"
# CLAUDE-HOST-METADATA-PROOF-02B Packet

- subagent_id_or_nickname: CLAUDE-HOST-METADATA-PROOF-02B
- host_metadata_status: $hostMetadataStatus
- claude_readiness_status: $claudeReadinessStatus
- state_transition_made_or_not: $stateTransition
- state_json_validation: $stateJsonValidation

## Commands

1. `C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.187\claude.exe plugin validate C:\Projects\claude-xiigen-smart-skills`
   - exit_code: $validateExit
   - stdout_path: $validateStdout
   - stderr_path: $validateStderr
2. `C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.187\claude.exe --plugin-dir C:\Projects\claude-xiigen-smart-skills plugin details claude-xiigen-smart-skills`
   - exit_code: $detailsExit
   - stdout_path: $detailsStdout
   - stderr_path: $detailsStderr

## Evidence Supports Only

$evidenceSupportsOnly

## Non-Claims

$($nonClaims | ForEach-Object { "- $_" } | Out-String)

## Next Bounded Worker Scope

$nextBoundedWorkerScope
"@

Set-Content -LiteralPath $mdPath -Value $md -Encoding utf8

[ordered]@{
    packet_path = $packetPath
    markdown_path = $mdPath
    state_path = $statePath
    host_metadata_status = $hostMetadataStatus
    claude_readiness_status = $claudeReadinessStatus
    state_json_validation = $stateJsonValidation
} | ConvertTo-Json -Depth 6
