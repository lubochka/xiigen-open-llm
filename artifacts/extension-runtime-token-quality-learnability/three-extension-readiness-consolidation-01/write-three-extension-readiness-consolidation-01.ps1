$ErrorActionPreference = 'Stop'

$evidenceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = 'C:\Projects\llm_mvp_core'
$statePath = Join-Path $repoRoot 'docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json'
$packetPath = Join-Path $evidenceDir 'three-extension-readiness-consolidation-01-packet.json'
$packetMdPath = Join-Path $evidenceDir 'three-extension-readiness-consolidation-01-packet.md'
$stateAppendPath = Join-Path $evidenceDir 'state-append-block.json'
$stateVerifyPath = Join-Path $evidenceDir 'state-update-verification.json'
$timestampLocal = Get-Date -Format o
$timestampUtc = (Get-Date).ToUniversalTime().ToString('o')
$stateMarkerKey = 'three_extension_readiness_consolidation_01_' + (Get-Date).ToUniversalTime().ToString('yyyyMMdd_HHmmss')

$row23InputPath = 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json'
$activeQuote = ([System.IO.File]::ReadAllText($row23InputPath, [System.Text.Encoding]::UTF8) | ConvertFrom-Json).approval_quote_used

$exactFilesRead = @(
  'C:\Projects\llm_mvp_core\AGENTS.md',
  'C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md (first 220 lines and targeted gates only)',
  'C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json (metadata/head/tail/targeted markers only; no full parse claim)',
  'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-input-path-evidence-packet.json',
  'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json',
  'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\claude-ext-readiness-01\CLAUDE-EXT-READINESS-01-evidence-packet.json',
  'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\claude-host-metadata-proof-02b\CLAUDE-HOST-METADATA-PROOF-02B-evidence-packet.json',
  'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\claude-host-runtime-proof-02\claude-host-runtime-proof-02-runner-summary.json',
  'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\claude-live-host-route-proof-or-blocker-01\CLAUDE-LIVE-HOST-ROUTE-PROOF-OR-BLOCKER-01-evidence-packet.json',
  'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-ext-readiness-01\cursor-ext-readiness-01-packet.json',
  'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-ext-path-b-runtime-01\cursor-ext-path-b-runtime-01-packet.json',
  'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-ext-path-b-ui-screenshot-01\cursor-ext-path-b-ui-screenshot-01-packet.json',
  'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-ext-path-b-same-row23-visible-ui-route-01\same-row23-visible-ui-route-decision.json',
  'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-ext-path-b-same-row23-visible-ui-route-01\same-row23-visible-ui-route-verification.json',
  'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-ext-readiness-01\codex-ext-readiness-01-packet.json',
  'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-ext-path-b-runtime-01\codex-ext-path-b-runtime-01-packet.json',
  'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-host-ui-route-proof-or-blocker-01\codex-host-ui-route-proof-or-blocker-01-packet.json',
  'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-host-ui-route-proof-or-blocker-01\browser-route-evidence.json',
  'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-host-ui-access-recovery-01\codex-host-ui-access-recovery-01-packet.json',
  'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-host-ui-access-recovery-01\browser-route-evidence.json'
)

$statusesByExtension = [ordered]@{
  Claude = [ordered]@{
    status = 'BLOCKED'
    full_readiness_can_be_claimed = $false
    proven_gates = @(
      'Claude Code 2.1.187 executable exists and metadata commands pass.',
      'Plugin validate/list/details show session-local claude-xiigen-smart-skills metadata and SessionStart hook inventory.',
      'Local adapter/core Row23 probes reach llm_mvp_core with no external AI send.',
      'Selected-skills probe produced non-empty selected skills in CLAUDE-HOST-RUNTIME-PROOF-02.'
    )
    unproven_gates = @(
      'Live Claude chat/session/UI route for approved Row23 prompt.',
      'Claude-visible answer/error/debug path from the real host.',
      'Same-scenario full Path B through live Claude host.',
      'Token savings, semantic quality improvement, learnability/DPO lifecycle, backup/export/import/share/continue-training, rollback/target safety, documentation, final coverage.'
    )
    blockers = @(
      'Safe metadata/plugin commands do not submit Row23 through Claude host.',
      'Claude --print/chat would submit to the Claude model/external service and was not authorized by the evidence packets.',
      'No no-secret host harness was exposed that proves SessionStart hook execution for Row23 without external prompt submission.'
    )
    next_action = 'NEEDS_USER_HOST_ACTION: provide or approve one no-secret Claude live-host route: either a safe Claude Code session/plugin hook harness that proves SessionStart/core artifacts without external prompt submission, or explicit approval to submit the approved Row23 safe prompt through Claude --print/chat with redacted I/O and no secrets.'
    key_evidence = @(
      'claude-host-runtime-proof-02-runner-summary.json',
      'CLAUDE-LIVE-HOST-ROUTE-PROOF-OR-BLOCKER-01-evidence-packet.json'
    )
  }
  Cursor = [ordered]@{
    status = 'PARTIAL'
    full_readiness_can_be_claimed = $false
    proven_gates = @(
      'Local adapter/core boundary smoke passed for Row23.',
      'Cursor Extension Host command-path Path B passed for same Row23 prompt.',
      'Visible Cursor XIIGen Smart Skills panel screenshot captured.',
      'Same-Row23 visible local panel route verification PASS: panel submitted prompt, panel response visible, core success true, external AI flag false.'
    )
    unproven_gates = @(
      'Token before/after with quality non-regression.',
      'Semantic quality improvement.',
      'Learnability/DPO lifecycle.',
      'Backup/export/import/share/continue-training.',
      'Rollback/target safety.',
      'Documentation and final dual-path scenario coverage.',
      'Full Cursor readiness and all-three readiness.'
    )
    blockers = @(
      'No current host-route blocker after same-Row23 local panel proof, but non-host full-readiness gates remain unproven.',
      'Cursor evidence is local extension panel/command route evidence, not a signed-in Cursor Agent/external AI chat proof.'
    )
    next_action = 'CURSOR-NON-HOST-READINESS-GAPS-01: advance token/quality/learnability/lifecycle/rollback/docs coverage using existing local Cursor panel/core evidence as host-path baseline; no external AI submission, no secrets, no git.'
    key_evidence = @(
      'cursor-ext-path-b-runtime-01-packet.json',
      'cursor-ext-path-b-ui-screenshot-01-packet.json',
      'same-row23-visible-ui-route-verification.json'
    )
  }
  Codex = [ordered]@{
    status = 'BLOCKED'
    full_readiness_can_be_claimed = $false
    proven_gates = @(
      'Local source/plugin package inspection, syntax, source wiring, component QA, parser regression, and Row23 direct source-entrypoint smoke passed.',
      'Installed Codex plugin cache metadata exists and installed-cache entrypoint smoke reaches llm_mvp_core.',
      'Row23 input is safe and no secrets/external AI submission were used in the packets.'
    )
    unproven_gates = @(
      'User-visible live Codex host/plugin/chat/UI/app route.',
      'Visible Codex host answer/error/debug path.',
      'Same-scenario live Path B through Codex host.',
      'Token savings, semantic quality improvement, learnability/DPO lifecycle, backup/export/import/share/continue-training, rollback/target safety, documentation, final coverage.'
    )
    blockers = @(
      'In-app Browser exposes no user tabs, no session tabs, and no selected Codex host tab.',
      'Codex CLI is discoverable but codex --version/help/plugin commands return Access is denied.',
      'MCP/resources expose no Codex thread/app/host automation route to this worker.',
      'Installed plugin cache/package presence is not a live user-visible Codex host invocation.'
    )
    next_action = 'NEEDS_USER_HOST_ACTION: expose one safe user-visible Codex route to a bounded worker: an already-open in-app Browser tab to the Codex host/plugin/thread page, a callable Codex app/thread/host automation tool, or a fixed Codex CLI/plugin command route that executes without Access denied. Then rerun Codex host UI route proof for the same Row23 prompt with no secrets and no external AI submission outside authority.'
    key_evidence = @(
      'codex-ext-readiness-01-packet.json',
      'codex-ext-path-b-runtime-01-packet.json',
      'codex-host-ui-route-proof-or-blocker-01-packet.json',
      'codex-host-ui-access-recovery-01-packet.json'
    )
  }
}

$overallStatus = [ordered]@{
  status = 'BLOCKED'
  full_extensions = 0
  partial_extensions = 1
  blocked_extensions = 2
  full_readiness_can_be_claimed = $false
  human_summary = 'No full three-extension readiness claim is supported. Cursor is PARTIAL with same-Row23 visible local panel proof; Claude and Codex are BLOCKED on safe user-visible/live-host route exposure or approval.'
}

$changedFiles = @(
  $MyInvocation.MyCommand.Path,
  $packetPath,
  $packetMdPath,
  $stateAppendPath,
  $stateVerifyPath,
  (Join-Path $evidenceDir 'runner.stdout.txt'),
  (Join-Path $evidenceDir 'runner.stderr.txt'),
  (Join-Path $evidenceDir 'runner.exit-code.txt'),
  $statePath
)

$packet = [ordered]@{
  schema = 'xiigen.three_extension_readiness_consolidation_01.packet.v1'
  subagent_id_or_nickname = 'THREE-EXTENSION-READINESS-CONSOLIDATION-01'
  generated_at_local = $timestampLocal
  generated_at_utc = $timestampUtc
  requested_artifact_type = 'consolidation evidence packet + execution state update with exact current statuses, blockers, and next bounded action'
  active_luba_quote_creating_authority = $activeQuote
  claim_scope = 'Consolidation of already-written evidence packets only; no product probes, UI, external AI, network, tests, git, or source edits.'
  evidence_scope = 'Read/parsed named packet/state/plan evidence only. Execution state was not fully parsed because the raw 467 MB read timed out; state update is an appended marker.'
  count_claimed = [ordered]@{
    extensions_consolidated = 3
    full_extensions_claimed = 0
    product_commands_run = 0
    ui_or_browser_checks_run = 0
    external_ai_calls = 0
    network_calls = 0
    git_actions = 0
    source_edits = 0
  }
  independent_evidence_count = 20
  downgrade_needed = $true
  changed_files = $changedFiles
  exact_files_read = $exactFilesRead
  exact_files_written = $changedFiles
  statuses_by_extension = $statusesByExtension
  overall_three_extension_readiness_status = $overallStatus
  state_transition_made_or_not = "MADE: appended $stateMarkerKey marker to execution state without full-state reserialization."
  next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION = 'NEEDS_USER_HOST_ACTION for Claude and Codex host-route blockers. Cursor can continue with CURSOR-NON-HOST-READINESS-GAPS-01, but full three-extension readiness cannot advance to FULL until Claude and Codex expose approved no-secret live/user-visible host routes.'
  evidence_supports_only = 'The evidence supports only: Claude BLOCKED with partial metadata/adapter/core evidence; Cursor PARTIAL with same-Row23 visible local panel/core proof but non-host gates unproven; Codex BLOCKED with partial adapter/cache evidence and no accessible user-visible host route. It does not support token savings, semantic quality improvement, learnability/training, lifecycle, rollback, documentation completion, final dual-path coverage, or full readiness for any extension.'
  blockers = @(
    'Claude needs an approved no-secret live host route/harness or explicit approval for Row23 through Claude --print/chat; current packets intentionally avoid unsafe external prompt submission.',
    'Codex needs an exposed user-visible host route or fixed CLI/plugin command route; current in-app browser has no usable tab and codex CLI/plugin commands return Access is denied.',
    'Cursor still lacks token, quality, learnability, lifecycle, rollback, docs, and final coverage proof even though same-Row23 local panel Path B passed.'
  )
  forbidden_actions_absent_by_this_worker = [ordered]@{
    source_edits = $true
    product_probes = $true
    ui_browser = $true
    network = $true
    external_ai = $true
    git = $true
  }
  parent_will_perform_commit_push_after_review = $true
  must_stop_after_report = $true
}

$stateMarker = [ordered]@{
  schema = 'xiigen.three_extension_readiness_consolidation_01.state_marker.v1'
  subagent_id_or_nickname = 'THREE-EXTENSION-READINESS-CONSOLIDATION-01'
  timestamp_utc = $timestampUtc
  evidence_packet = $packetPath
  statuses_by_extension = [ordered]@{
    Claude = 'BLOCKED'
    Cursor = 'PARTIAL'
    Codex = 'BLOCKED'
  }
  overall_three_extension_readiness_status = $overallStatus
  next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION = $packet.next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION
  evidence_supports_only = $packet.evidence_supports_only
  blockers = $packet.blockers
  full_three_extension_readiness_claimed = $false
  source_edits_performed = $false
  git_actions_performed = $false
}

$packet | ConvertTo-Json -Depth 20 | Set-Content -Path $packetPath -Encoding UTF8

$md = @"
# THREE-EXTENSION-READINESS-CONSOLIDATION-01

subagent_id_or_nickname: THREE-EXTENSION-READINESS-CONSOLIDATION-01

## Statuses By Extension

- Claude: BLOCKED. Partial metadata/plugin/local adapter evidence exists, but no approved no-secret live Claude host route proves Row23 in a real Claude session/chat/UI.
- Cursor: PARTIAL. Same-Row23 visible local panel route passes and reaches llm_mvp_core, but token, quality, learnability, lifecycle, rollback, docs, and final coverage remain unproven.
- Codex: BLOCKED. Local adapter/cache evidence exists, but no user-visible Codex host route is exposed; browser has no usable tab and Codex CLI/plugin commands are Access denied.

## Overall

No FULL readiness claim is supported for any extension or for all three extensions together.

## Next

NEEDS_USER_HOST_ACTION for Claude and Codex: expose or approve safe no-secret live/user-visible host routes before repeating host-route probes. Cursor can continue with non-host readiness gaps using the existing local panel/core proof as baseline.

## Evidence Supports Only

$($packet.evidence_supports_only)
"@
$md | Set-Content -Path $packetMdPath -Encoding UTF8

$stateAppend = [ordered]@{
  state_marker_key = $stateMarkerKey
  state_marker = $stateMarker
}
$stateAppend | ConvertTo-Json -Depth 20 | Set-Content -Path $stateAppendPath -Encoding UTF8

$markerJson = $stateMarker | ConvertTo-Json -Depth 20
$stateEntry = ",`r`n  `"$stateMarkerKey`": $markerJson"
Add-Content -Path $statePath -Value $stateEntry -Encoding UTF8

$tail = Get-Content -Path $statePath -Tail 120
$tailText = $tail -join "`n"
$verification = [ordered]@{
  schema = 'xiigen.three_extension_readiness_consolidation_01.state_update_verification.v1'
  generated_at_utc = (Get-Date).ToUniversalTime().ToString('o')
  state_path = $statePath
  state_marker_key = $stateMarkerKey
  tail_contains_marker_key = $tailText.Contains($stateMarkerKey)
  full_state_parse_attempted = $false
  full_state_parse_claimed = $false
  state_update_method = 'append-only marker; no full-state reserialization'
  exit_if_marker_missing = $false
}
$verification | ConvertTo-Json -Depth 10 | Set-Content -Path $stateVerifyPath -Encoding UTF8

Write-Output "packet=$packetPath"
Write-Output "packet_md=$packetMdPath"
Write-Output "state_append=$stateAppendPath"
Write-Output "state_verification=$stateVerifyPath"
Write-Output "state_marker_key=$stateMarkerKey"
Write-Output "tail_contains_marker_key=$($verification.tail_contains_marker_key)"
