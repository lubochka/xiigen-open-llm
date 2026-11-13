$ErrorActionPreference = 'Stop'

$evidenceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = 'C:\Projects\llm_mvp_core'
$statePath = Join-Path $repoRoot 'docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json'
$agentsPath = Join-Path $repoRoot 'AGENTS.md'
$packetPath = Join-Path $evidenceDir 'three-extension-final-blocker-consolidation-02-packet.json'
$packetMdPath = Join-Path $evidenceDir 'three-extension-final-blocker-consolidation-02-packet.md'
$stateAppendPath = Join-Path $evidenceDir 'state-append-block.json'
$stateVerifyPath = Join-Path $evidenceDir 'state-update-verification.json'
$summaryPath = Join-Path $evidenceDir 'runner-summary.json'
$timestampUtc = (Get-Date).ToUniversalTime().ToString('o')
$timestampLocal = Get-Date -Format o
$stateMarkerKey = 'three_extension_final_blocker_consolidation_02_' + (Get-Date).ToUniversalTime().ToString('yyyyMMdd_HHmmss')

function Read-JsonPacket {
  param(
    [Parameter(Mandatory = $true)][string]$Path,
    [Parameter(Mandatory = $true)][string]$Label,
    [Parameter(Mandatory = $true)][string]$Extension
  )
  $json = [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
  return [ordered]@{
    label = $Label
    extension = $Extension
    path = $Path
    json = $json
  }
}

function To-JsonText {
  param($Value)
  if ($null -eq $Value) {
    return $null
  }
  return ($Value | ConvertTo-Json -Depth 20 -Compress)
}

function Get-StatusFields {
  param($Json)
  $out = [ordered]@{}
  foreach ($prop in $Json.PSObject.Properties) {
    if ($prop.Name -match 'status' -and $prop.Name -notmatch 'state_transition_made_or_not|status_source') {
      $out[$prop.Name] = $prop.Value
    }
  }
  return $out
}

function Get-GateFields {
  param($Json)
  $out = [ordered]@{}
  foreach ($prop in $Json.PSObject.Properties) {
    if ($prop.Name -match 'gates|not_proven|non_claims|no_claims|claim_locks') {
      $out[$prop.Name] = $prop.Value
    }
  }
  return $out
}

function Get-Blockers {
  param($Json)
  if ($null -eq $Json.blockers) {
    return @()
  }
  return @($Json.blockers)
}

function Get-NextScope {
  param($Json)
  if ($null -ne $Json.next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION) {
    return $Json.next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION
  }
  if ($null -ne $Json.next_bounded_worker_scope) {
    return $Json.next_bounded_worker_scope
  }
  return $null
}

function Get-HostAction {
  param($Json)
  if ($null -ne $Json.exact_user_visible_host_action_needed) {
    return $Json.exact_user_visible_host_action_needed
  }
  if ($null -ne $Json.live_host_blocker -and $null -ne $Json.live_host_blocker.exact_user_visible_host_action_or_approval_needed) {
    return $Json.live_host_blocker.exact_user_visible_host_action_or_approval_needed
  }
  return $null
}

# Explicitly read AGENTS.md for the requested honesty/no-overclaim guard.
$agentsText = [System.IO.File]::ReadAllText($agentsPath, [System.Text.Encoding]::UTF8)
$agentsHonestyLoaded = ($agentsText.Contains('NO DISHONEST CLAIMS') -or $agentsText.Contains('OVERCLAIM'))

$packetSpecs = @(
  @('execution-state-repair-02', 'State', 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\execution-state-json-integrity-repair-01\EXECUTION-STATE-MARKER-RECOVERY-REPAIR-02.packet.json'),
  @('claude-metadata-proof-02b', 'Claude', 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\claude-host-metadata-proof-02b\CLAUDE-HOST-METADATA-PROOF-02B-evidence-packet.json'),
  @('claude-live-host-blocker-02', 'Claude', 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\claude-live-host-blocker-packet-02\CLAUDE-LIVE-HOST-BLOCKER-PACKET-02-evidence-packet.json'),
  @('claude-live-host-readback-02b', 'Claude', 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\claude-live-host-blocker-packet-02\readback-02b\CLAUDE-LIVE-HOST-BLOCKER-STATE-READBACK-02B-readback-packet.json'),
  @('cursor-path-b-runtime-01', 'Cursor', 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-ext-path-b-runtime-01\cursor-ext-path-b-runtime-01-packet.json'),
  @('cursor-ui-screenshot-01', 'Cursor', 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-ext-path-b-ui-screenshot-01\cursor-ext-path-b-ui-screenshot-01-packet.json'),
  @('cursor-same-row23-visible-ui-blocker-02', 'Cursor', 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-same-row23-visible-ui-blocker-packet-02\cursor-same-row23-visible-ui-blocker-packet-02-packet.json'),
  @('cursor-lifecycle-rollback-nonhost-01', 'Cursor', 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-lifecycle-rollback-nonhost-validation-01\cursor-lifecycle-rollback-nonhost-validation-01-packet.json'),
  @('cursor-docs-token-nonhost-01', 'Cursor', 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-docs-final-coverage-or-token-quality-nonhost-01\cursor-docs-final-coverage-or-token-quality-nonhost-01-packet.json'),
  @('cursor-token-quality-same-pair-nonhost-01', 'Cursor', 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-token-quality-same-pair-nonhost-01\cursor-token-quality-same-pair-nonhost-01-packet.json'),
  @('codex-readiness-01', 'Codex', 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-ext-readiness-01\codex-ext-readiness-01-packet.json'),
  @('codex-path-b-runtime-01', 'Codex', 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-ext-path-b-runtime-01\codex-ext-path-b-runtime-01-packet.json'),
  @('codex-host-ui-blocker-01', 'Codex', 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-host-ui-route-proof-or-blocker-01\codex-host-ui-route-proof-or-blocker-01-packet.json'),
  @('codex-access-recovery-01', 'Codex', 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-host-ui-access-recovery-01\codex-host-ui-access-recovery-01-packet.json'),
  @('previous-consolidation-01', 'All', 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\three-extension-readiness-consolidation-01\three-extension-readiness-consolidation-01-packet.json'),
  @('row23-input-path', 'All', 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-input-path-evidence-packet.json')
)

$packets = @()
foreach ($spec in $packetSpecs) {
  $packets += Read-JsonPacket -Label $spec[0] -Extension $spec[1] -Path $spec[2]
}

$exactFilesRead = @($agentsPath, $statePath + ' (targeted tail/sentinel read and marker insertion only; no full parse claim)')
foreach ($entry in $packets) {
  $exactFilesRead += $entry.path
}

$packetExtracts = @()
foreach ($entry in $packets) {
  $j = $entry.json
  $packetExtracts += [ordered]@{
    label = $entry.label
    extension = $entry.extension
    path = $entry.path
    subagent_id_or_nickname = $j.subagent_id_or_nickname
    status_fields = Get-StatusFields -Json $j
    gate_fields = Get-GateFields -Json $j
    host_action = Get-HostAction -Json $j
    next_scope = Get-NextScope -Json $j
    evidence_supports_only = $j.evidence_supports_only
    blockers = Get-Blockers -Json $j
  }
}

$claudeHostAction = (Get-HostAction -Json (($packets | Where-Object { $_.label -eq 'claude-live-host-blocker-02' }).json))
$cursorHostAction = (Get-HostAction -Json (($packets | Where-Object { $_.label -eq 'cursor-same-row23-visible-ui-blocker-02' }).json))
$codexHostAction = (Get-NextScope -Json (($packets | Where-Object { $_.label -eq 'codex-access-recovery-01' }).json))

$statusesByExtension = [ordered]@{
  Claude = [ordered]@{
    status = 'BLOCKED_USER_HOST_ACTION_REQUIRED'
    readiness_claimed = $false
    evidence_basis = @('claude-metadata-proof-02b', 'claude-live-host-blocker-02', 'claude-live-host-readback-02b')
    gates_proven = @(
      'Claude metadata/plugin validation/details are retained as safe metadata evidence only.',
      'Prior local adapter/core Row23 evidence is retained as partial boundary evidence only.',
      'Claude live-host route is explicitly blocked rather than guessed or externally submitted.'
    )
    gates_not_proven = @(
      'live Claude chat/session/UI route for Row23',
      'Claude-visible answer/error/debug path from a real host surface',
      'token savings',
      'semantic quality improvement',
      'learnability/training and lifecycle',
      'rollback/target promotion',
      'full Claude readiness'
    )
    blockers = Get-Blockers -Json (($packets | Where-Object { $_.label -eq 'claude-live-host-blocker-02' }).json)
    exact_user_host_actions_required = @($claudeHostAction)
    safe_next_worker_after_user_action = Get-NextScope -Json (($packets | Where-Object { $_.label -eq 'claude-live-host-blocker-02' }).json)
  }
  Cursor = [ordered]@{
    status = 'PARTIAL_WITH_SAME_ROW23_VISIBLE_UI_BLOCKER_AND_NONHOST_GAPS'
    readiness_claimed = $false
    evidence_basis = @('cursor-path-b-runtime-01', 'cursor-ui-screenshot-01', 'cursor-same-row23-visible-ui-blocker-02', 'cursor-lifecycle-rollback-nonhost-01', 'cursor-docs-token-nonhost-01', 'cursor-token-quality-same-pair-nonhost-01')
    gates_proven = @(
      'Cursor Extension Host command-path Path B evidence is retained for Row23.',
      'A safe nonblank Cursor XIIGen Smart Skills panel screenshot is retained.',
      'Cursor token/quality/lifecycle/docs packets exist but remain nonhost or partial by their own non-claims.'
    )
    gates_not_proven = @(
      'same-Row23 visible UI submission through a proven local-only panel route',
      'token savings',
      'semantic quality improvement',
      'learnability/training',
      'checkpoint promotion',
      'full Cursor readiness'
    )
    blockers = Get-Blockers -Json (($packets | Where-Object { $_.label -eq 'cursor-same-row23-visible-ui-blocker-02' }).json)
    exact_user_host_actions_required = @($cursorHostAction)
    safe_next_worker_after_user_action = Get-NextScope -Json (($packets | Where-Object { $_.label -eq 'cursor-same-row23-visible-ui-blocker-02' }).json)
  }
  Codex = [ordered]@{
    status = 'BLOCKED_USER_HOST_ACTION_REQUIRED'
    readiness_claimed = $false
    evidence_basis = @('codex-readiness-01', 'codex-path-b-runtime-01', 'codex-host-ui-blocker-01', 'codex-access-recovery-01')
    gates_proven = @(
      'Codex local adapter/source/plugin package and installed-cache evidence is retained as partial evidence only.',
      'Row23 installed-cache smoke reached llm_mvp_core locally.',
      'Codex host UI/access blockers are explicit and not converted into readiness.'
    )
    gates_not_proven = @(
      'user-visible Codex host/plugin/chat/UI/app route',
      'visible Codex host answer/error/debug path',
      'same-scenario live Path B through Codex host',
      'token savings',
      'semantic quality improvement',
      'learnability/training and lifecycle',
      'rollback/target promotion',
      'full Codex readiness'
    )
    blockers = Get-Blockers -Json (($packets | Where-Object { $_.label -eq 'codex-access-recovery-01' }).json)
    exact_user_host_actions_required = @($codexHostAction)
    safe_next_worker_after_user_action = $codexHostAction
  }
}

$overall = [ordered]@{
  status = 'BLOCKED_NOT_READY_NO_FULL_READINESS_CLAIM'
  full_extensions_claimed = 0
  readiness_claimed = $false
  blocked_or_partial_extensions = 3
  human_summary = 'All three extensions still have unclosed blockers or partial-only evidence. Claude and Codex require user-host action to expose safe live/user-visible routes. Cursor remains partial because same-Row23 visible UI and nonhost readiness gates are not closed.'
}

$exactUserHostActions = [ordered]@{
  Claude = @($claudeHostAction)
  Cursor = @($cursorHostAction)
  Codex = @($codexHostAction)
}

$safeNextWorker = [ordered]@{
  Claude = $statusesByExtension.Claude.safe_next_worker_after_user_action
  Cursor = $statusesByExtension.Cursor.safe_next_worker_after_user_action
  Codex = $statusesByExtension.Codex.safe_next_worker_after_user_action
}

$evidenceSupportsOnly = 'This consolidation supports only current packet-level blocker/status rollup: Claude BLOCKED by no approved no-secret live host route; Cursor PARTIAL with same-Row23 visible UI blocker and nonhost readiness gaps; Codex BLOCKED by no accessible user-visible host route or working command route. It does not prove readiness, token savings, semantic quality improvement, learnability/training, lifecycle completion, rollback/target promotion, final dual-path coverage, source changes, product probes, UI/browser checks by this worker, external AI calls, network calls, git actions, or all-three extension readiness.'

$blockers = @(
  'Claude: user must approve/provide a no-secret Claude live-host route before a bounded worker can prove SessionStart/local hook execution or explicitly approved Row23 Claude --print/chat with redacted I/O.',
  'Cursor: user must provide a local-only visible Cursor XIIGen Smart Skills panel route with Cursor Agent/chat closed or unfocusable before same-Row23 visible submission can proceed.',
  'Codex: user must expose an in-app Browser Codex host/plugin/thread tab, callable Codex host automation route, or a fixed Codex CLI/plugin command route that does not return Access denied.'
)

$changedFiles = @(
  $MyInvocation.MyCommand.Path,
  $packetPath,
  $packetMdPath,
  $stateAppendPath,
  $stateVerifyPath,
  $summaryPath,
  (Join-Path $evidenceDir 'runner.stdout.txt'),
  (Join-Path $evidenceDir 'runner.stderr.txt'),
  (Join-Path $evidenceDir 'runner.exit-code.txt'),
  $statePath
)

$packet = [ordered]@{
  schema = 'xiigen.three_extension_final_blocker_consolidation_02.packet.v1'
  subagent_id_or_nickname = 'THREE-EXTENSION-FINAL-BLOCKER-CONSOLIDATION-02'
  generated_at_local = $timestampLocal
  generated_at_utc = $timestampUtc
  requested_artifact_type = 'final current-status blocker consolidation packet + narrow state marker; does not claim readiness'
  active_luba_quote_creating_authority = 'Ne ponimayu pochemu snova stop??? Naydi i etu lazeyku pozhaluysta. Zanesi eto i deystviya vnizu v plan raboty- sdelay plan raboty polnym po nashim standartam. Sdelay 20 prohodov i ubedis chto plan dovedet nakonets neobhodimye extension do polnoy gotovnosti. I pristupayk deystviyu do polnoy gotovnosti vseh 3 extensions. NO STOPS approved!!! All through agents!!!'
  claim_scope = 'Packet/readback/state-marker consolidation only. This worker did not run product commands, UI/browser, external AI, network, tests, git, or source edits.'
  evidence_scope = 'Read/parsed selected final packet JSON files, AGENTS.md honesty text, and targeted execution-state marker location only.'
  count_claimed = [ordered]@{
    packet_json_files_parsed = $packets.Count
    extensions_consolidated = 3
    full_readiness_claims = 0
    product_commands_run_by_this_worker = 0
    ui_or_browser_checks_run_by_this_worker = 0
    external_ai_calls_by_this_worker = 0
    network_calls_by_this_worker = 0
    git_actions_by_this_worker = 0
    source_edits_by_this_worker = 0
  }
  independent_evidence_count = $packets.Count
  subagent_id_or_nickname_confirmed = 'THREE-EXTENSION-FINAL-BLOCKER-CONSOLIDATION-02'
  downgrade_needed = $true
  changed_files = $changedFiles
  exact_files_read = $exactFilesRead
  exact_files_written = $changedFiles
  packet_extracts = $packetExtracts
  statuses_by_extension = $statusesByExtension
  overall_three_extension_readiness_status = $overall
  exact_user_host_actions_required = $exactUserHostActions
  safe_next_worker_after_user_action = $safeNextWorker
  state_transition_made_or_not = 'PENDING_IN_SCRIPT_BEFORE_STATE_MARKER_WRITE'
  evidence_supports_only = $evidenceSupportsOnly
  blockers = $blockers
  non_claims = @(
    'No readiness claim.',
    'No full extension claim.',
    'No token savings claim.',
    'No semantic quality improvement claim.',
    'No learnability/training claim.',
    'No lifecycle/export/import/backup/continue-training completion claim.',
    'No rollback/target promotion claim.',
    'No product command, UI/browser, external AI, network, git, or source edit by this worker.'
  )
  AGENTS_honesty_guard_loaded = $agentsHonestyLoaded
  parent_will_perform_commit_push_after_review = $true
  must_stop_after_report = $true
}

$stateMarker = [ordered]@{
  schema = 'xiigen.three_extension_final_blocker_consolidation_02.state_marker.v1'
  subagent_id_or_nickname = 'THREE-EXTENSION-FINAL-BLOCKER-CONSOLIDATION-02'
  timestamp_utc = $timestampUtc
  evidence_packet = $packetPath
  statuses_by_extension = [ordered]@{
    Claude = 'BLOCKED_USER_HOST_ACTION_REQUIRED'
    Cursor = 'PARTIAL_WITH_SAME_ROW23_VISIBLE_UI_BLOCKER_AND_NONHOST_GAPS'
    Codex = 'BLOCKED_USER_HOST_ACTION_REQUIRED'
  }
  overall_three_extension_readiness_status = $overall
  exact_user_host_actions_required = $exactUserHostActions
  safe_next_worker_after_user_action = $safeNextWorker
  evidence_supports_only = $evidenceSupportsOnly
  blockers = $blockers
  readiness_claimed = $false
  source_edits_performed = $false
  git_actions_performed = $false
}

$stateAppend = [ordered]@{
  state_marker_key = $stateMarkerKey
  state_marker = $stateMarker
}
$stateAppend | ConvertTo-Json -Depth 30 | Set-Content -Path $stateAppendPath -Encoding UTF8

$stateTransition = 'NOT_MADE'
$tailContains = $false
$stateUpdateMethod = 'not attempted'
$stateFailure = $null

try {
  $stateText = [System.IO.File]::ReadAllText($statePath, [System.Text.Encoding]::UTF8)
  $sentinel = "`r`n  },`r`n  `"malformed_original_preserved_in_backup`""
  if (-not $stateText.Contains($sentinel)) {
    $sentinel = "`n  },`n  `"malformed_original_preserved_in_backup`""
  }
  if ($stateText.Contains($stateMarkerKey)) {
    $stateTransition = 'NOT_MADE: marker key already existed'
    $stateUpdateMethod = 'targeted marker readback only'
  } elseif ($stateText.Contains($sentinel)) {
    $markerJson = $stateMarker | ConvertTo-Json -Depth 30
    $insert = ",`r`n    `"$stateMarkerKey`": $markerJson"
    $replacement = $insert + $sentinel
    $stateText = $stateText.Replace($sentinel, $replacement)
    [System.IO.File]::WriteAllText($statePath, $stateText, [System.Text.Encoding]::UTF8)
    $stateTransition = "MADE: inserted $stateMarkerKey under recovered_top_level_state_markers before malformed_original_preserved_in_backup"
    $stateUpdateMethod = 'targeted sentinel insertion; no full-state parse or reserialization claim'
  } else {
    $stateTransition = 'NOT_MADE: safe recovered_top_level_state_markers sentinel not found'
    $stateUpdateMethod = 'skipped because safe insertion point was not found'
  }

  $tailText = (Get-Content -Path $statePath -Tail 160) -join "`n"
  $tailContains = $tailText.Contains($stateMarkerKey)
} catch {
  $stateTransition = 'NOT_MADE: exception during safe marker update'
  $stateFailure = $_.Exception.Message
  $stateUpdateMethod = 'failed safe marker update'
}

$packet.state_transition_made_or_not = $stateTransition

$packet | ConvertTo-Json -Depth 40 | Set-Content -Path $packetPath -Encoding UTF8

$md = @"
# THREE-EXTENSION-FINAL-BLOCKER-CONSOLIDATION-02

subagent_id_or_nickname: THREE-EXTENSION-FINAL-BLOCKER-CONSOLIDATION-02

This is a consolidation/status blocker packet only. It does not claim readiness.

## Statuses By Extension

- Claude: BLOCKED_USER_HOST_ACTION_REQUIRED. Evidence supports only metadata/plugin/local adapter facts plus an explicit no-secret live-host blocker.
- Cursor: PARTIAL_WITH_SAME_ROW23_VISIBLE_UI_BLOCKER_AND_NONHOST_GAPS. Evidence supports retained command-path/screenshot/nonhost packets, while same-Row23 visible UI and nonhost readiness gates remain unclosed.
- Codex: BLOCKED_USER_HOST_ACTION_REQUIRED. Evidence supports only local adapter/cache facts plus explicit host-route/access blockers.

## Exact User Host Actions Required

- Claude: $(To-JsonText $exactUserHostActions.Claude)
- Cursor: $(To-JsonText $exactUserHostActions.Cursor)
- Codex: $(To-JsonText $exactUserHostActions.Codex)

## Safe Next Worker After User Action

- Claude: $(To-JsonText $safeNextWorker.Claude)
- Cursor: $(To-JsonText $safeNextWorker.Cursor)
- Codex: $(To-JsonText $safeNextWorker.Codex)

## Evidence Supports Only

$evidenceSupportsOnly

## State Transition

$stateTransition
"@
$md | Set-Content -Path $packetMdPath -Encoding UTF8

$verification = [ordered]@{
  schema = 'xiigen.three_extension_final_blocker_consolidation_02.state_update_verification.v1'
  generated_at_utc = (Get-Date).ToUniversalTime().ToString('o')
  state_path = $statePath
  state_marker_key = $stateMarkerKey
  tail_contains_marker_key = $tailContains
  full_state_parse_attempted = $false
  full_state_parse_claimed = $false
  state_update_method = $stateUpdateMethod
  state_transition_made_or_not = $stateTransition
  exception = $stateFailure
}
$verification | ConvertTo-Json -Depth 20 | Set-Content -Path $stateVerifyPath -Encoding UTF8

$summary = [ordered]@{
  generated_at_utc = (Get-Date).ToUniversalTime().ToString('o')
  packet_path = $packetPath
  packet_md_path = $packetMdPath
  state_append_path = $stateAppendPath
  state_verify_path = $stateVerifyPath
  state_marker_key = $stateMarkerKey
  tail_contains_marker_key = $tailContains
  state_transition_made_or_not = $stateTransition
}
$summary | ConvertTo-Json -Depth 10 | Set-Content -Path $summaryPath -Encoding UTF8

Write-Output "packet=$packetPath"
Write-Output "packet_md=$packetMdPath"
Write-Output "state_append=$stateAppendPath"
Write-Output "state_verification=$stateVerifyPath"
Write-Output "summary=$summaryPath"
Write-Output "state_marker_key=$stateMarkerKey"
Write-Output "state_transition_made_or_not=$stateTransition"
Write-Output "tail_contains_marker_key=$tailContains"
