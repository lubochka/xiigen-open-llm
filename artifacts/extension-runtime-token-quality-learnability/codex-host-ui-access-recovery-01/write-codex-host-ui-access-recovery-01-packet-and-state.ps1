$ErrorActionPreference = "Stop"

$EvidenceRoot = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-host-ui-access-recovery-01"
$StatePath = "C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"
$PacketJsonPath = Join-Path $EvidenceRoot "codex-host-ui-access-recovery-01-packet.json"
$PacketMdPath = Join-Path $EvidenceRoot "codex-host-ui-access-recovery-01-packet.md"
$timestamp = Get-Date
$stamp = $timestamp.ToString("yyyyMMdd_HHmmss")
$markerKey = "codex_host_ui_access_recovery_01_$stamp"

$changedFiles = @(
  (Join-Path $EvidenceRoot "run-codex-host-ui-access-recovery-01.ps1"),
  (Join-Path $EvidenceRoot "inspect-codex-global-state-route-keys.ps1"),
  (Join-Path $EvidenceRoot "write-codex-host-ui-access-recovery-01-packet-and-state.ps1"),
  (Join-Path $EvidenceRoot "browser-route-evidence.json"),
  (Join-Path $EvidenceRoot "route-discovery-summary.json"),
  (Join-Path $EvidenceRoot "codex-global-state-route-key-inspection.json"),
  (Join-Path $EvidenceRoot "commands\01-get-command-codex.stdout.txt"),
  (Join-Path $EvidenceRoot "commands\01-get-command-codex.stderr.txt"),
  (Join-Path $EvidenceRoot "commands\01-get-command-codex.meta.json"),
  (Join-Path $EvidenceRoot "commands\02-codex-version.stdout.txt"),
  (Join-Path $EvidenceRoot "commands\02-codex-version.stderr.txt"),
  (Join-Path $EvidenceRoot "commands\02-codex-version.meta.json"),
  (Join-Path $EvidenceRoot "commands\03-codex-help.stdout.txt"),
  (Join-Path $EvidenceRoot "commands\03-codex-help.stderr.txt"),
  (Join-Path $EvidenceRoot "commands\03-codex-help.meta.json"),
  (Join-Path $EvidenceRoot "commands\04-codex-plugin-list-json.stdout.txt"),
  (Join-Path $EvidenceRoot "commands\04-codex-plugin-list-json.stderr.txt"),
  (Join-Path $EvidenceRoot "commands\04-codex-plugin-list-json.meta.json"),
  (Join-Path $EvidenceRoot "commands\05-codex-plugin-help.stdout.txt"),
  (Join-Path $EvidenceRoot "commands\05-codex-plugin-help.stderr.txt"),
  (Join-Path $EvidenceRoot "commands\05-codex-plugin-help.meta.json"),
  $PacketJsonPath,
  $PacketMdPath,
  $StatePath
)

$exactFilesRead = @(
  "C:\Projects\llm_mvp_core\AGENTS.md (Get-Content command exited 0 but transcript output was truncated; no full-file-read claim is made)",
  "C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md (first 220 lines, only as needed)",
  "C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json (metadata/head/tail only; raw read timed out)",
  "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json",
  "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-host-ui-route-proof-or-blocker-01\codex-host-ui-route-proof-or-blocker-01-packet.json",
  "C:\Users\lubao\.codex\plugins\cache\openai-bundled\browser\26.616.51431\skills\control-in-app-browser\SKILL.md",
  "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\skills\xiigen-smart-skills\SKILL.md",
  "C:\Users\lubao\.codex\.codex-global-state.json (shape-only key/type/count inspection; scalar values not emitted)",
  (Join-Path $EvidenceRoot "browser-route-evidence.json"),
  (Join-Path $EvidenceRoot "route-discovery-summary.json"),
  (Join-Path $EvidenceRoot "codex-global-state-route-key-inspection.json"),
  (Join-Path $EvidenceRoot "commands\01-get-command-codex.stdout.txt"),
  (Join-Path $EvidenceRoot "commands\02-codex-version.meta.json"),
  (Join-Path $EvidenceRoot "commands\04-codex-plugin-list-json.meta.json")
)

$exactFilesWritten = $changedFiles

$commandsOrToolsUsed = @(
  [ordered]@{ name = "Get-Content -Raw AGENTS.md"; result = "exit_code=0, but tool transcript truncated output; full-file-read claim downgraded" },
  [ordered]@{ name = "Get-Content -Raw execution state"; result = "exit_code=124 timeout on raw 469MB state; switched to targeted metadata/head/tail reads" },
  [ordered]@{ name = "Targeted Row23/prior packet/plan/browser skill/xiigen skill reads"; result = "exit_code=0 for targeted reads" },
  [ordered]@{ name = "browser.documentation via Node REPL"; result = "succeeded; browser API docs emitted/read" },
  [ordered]@{ name = "browser.user.openTabs/browser.tabs.list/browser.tabs.selected"; result = "browser available; open_user_tabs=[]; session_tabs=[]; selected tab error='No active tab found'; wrote browser-route-evidence.json" },
  [ordered]@{ name = "list_mcp_resources/list_mcp_resource_templates"; result = "resources exposed GitHub app/skills only; no Codex thread/host-route MCP resource or template listed" },
  [ordered]@{ name = "run-codex-host-ui-access-recovery-01.ps1"; result = "exit_code=0; wrote route-discovery-summary.json and command stdout/stderr/meta files" },
  [ordered]@{ name = "Get-Command codex -All"; result = "exit_code=0; codex command is discoverable" },
  [ordered]@{ name = "codex --version"; result = "exit_code=127; Access is denied" },
  [ordered]@{ name = "codex --help"; result = "exit_code=127; Access is denied" },
  [ordered]@{ name = "codex plugin list --json"; result = "exit_code=127; Access is denied" },
  [ordered]@{ name = "codex plugin --help"; result = "exit_code=127; Access is denied" },
  [ordered]@{ name = "known Codex path inventory"; result = "top-level names/sizes only; no file contents; no usable route handle found" },
  [ordered]@{ name = "process inventory without window titles"; result = "Codex processes exist, but process presence is not a controllable route" },
  [ordered]@{ name = "inspect-codex-global-state-route-keys.ps1"; result = "exit_code=0; emitted key/type/count shapes only; no scalar values or usable route handle emitted" }
)

$codexGates = [ordered]@{
  row23_safe_input_selected = $true
  no_secret_policy_preserved = $true
  in_app_browser_available = $true
  in_app_browser_user_tab_route_available = $false
  in_app_browser_session_tab_route_available = $false
  in_app_browser_selected_tab_available = $false
  codex_cli_discoverable = $true
  codex_cli_executable_by_worker = $false
  codex_plugin_cli_route_available = $false
  codex_thread_or_host_mcp_route_available = $false
  local_global_state_contains_metadata_shapes = $true
  local_global_state_usable_no_secret_route_handle_found = $false
  installed_plugin_cache_present = $true
  installed_plugin_cache_is_live_host_route = $false
  visible_codex_host_answer_proven = $false
  same_scenario_path_b_proven = $false
  token_savings_proven = $false
  semantic_quality_improvement_proven = $false
  learnability_or_training_proven = $false
  backup_export_import_continue_training_proven = $false
  rollback_or_target_safety_proven = $false
  final_three_extension_readiness_proven = $false
}

$blockers = @(
  "In-app browser is available but exposes no user tabs, no session tabs, and no selected tab; this worker cannot claim or inspect a user-visible Codex host route.",
  "Codex command is discoverable, but codex --version, codex --help, codex plugin list --json, and codex plugin --help all returned Access is denied.",
  "The active MCP/resource surfaces exposed GitHub resources only; no Codex thread, app, host automation, or route tool was listed for this worker.",
  "Known Codex metadata/config paths and global-state key shapes show local app metadata, pinned/thread/session-like shapes, and running Codex processes, but no scalar route values were emitted and no callable no-secret route handle was exposed.",
  "Installed XIIGen Codex plugin cache exists, but cache/package presence is not a live user-visible Codex chat/UI/app invocation.",
  "Submitting Row23 to an external AI/chat surface was forbidden and was not done."
)

$nextScope = "Expose one safe, user-visible Codex route to a bounded worker, then rerun CODEX-HOST-UI-ROUTE-PROOF for Row23. Acceptable routes: an already-open in-app Browser tab to the Codex host/plugin/thread page that the worker can claim; a callable Codex app/thread/host automation tool exposed to the worker; or a fixed Codex CLI/plugin command route that executes without Access denied. Continue to forbid secrets, external AI prompt submission outside Row23 authority, network installs, and git."
$authorityQuote = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String("0J3QtSDQv9C+0L3QuNC80LDRjiDQv9C+0YfQtdC80YMg0YHQvdC+0LLQsCDRgdGC0L7Qvz8/PyDQndCw0LnQtNC4INC4INGN0YLRgyDQu9Cw0LfQtdC50LrRgyDQv9C+0LbQsNC70YPQudGB0YLQsC4g0JfQsNC90LXRgdC4INGN0YLQviDQuCDQtNC10LnRgdGC0LLQuNGPINCy0L3QuNC30YMg0LIg0L/Qu9Cw0L0g0YDQsNCx0L7RgtGLLSDRgdC00LXQu9Cw0Lkg0L/Qu9Cw0L0g0YDQsNCx0L7RgtGLINC/0L7Qu9C90YvQvCDQv9C+INC90LDRiNC40Lwg0YHRgtCw0L3QtNCw0YDRgtCw0LwuINCh0LTQtdC70LDQuSAyMCDQv9GA0L7RhdC+0LTQvtCyINC4INGD0LHQtdC00LjRgdGMINGH0YLQviDQv9C70LDQvSDQtNC+0LLQtdC00ZHRgiDQvdCw0LrQvtC90LXRhiDQvdC10L7QsdGF0L7QtNC40LzRi9C1IGV4dGVuc2lvbiDQtNC+INC/0L7Qu9C90L7QuSDQs9C+0YLQvtCy0L3QvtGB0YLQuC4g0Jgg0L/RgNC40YHRgtGD0L/QsNC50Log0LTQtdC50YHRgtCy0LjRjiDQtNC+INC/0L7Qu9C90L7QuSDQs9C+0YLQvtCy0L3QvtGB0YLQuCDQstGB0LXRhSAzIGV4dGVuc2lvbnMuIE5PIFNUT1BTIGFwcHJvdmVkISEhIEFsbCB0aHJvdWdoIGFnZW50cyEhIQ=="))

$packet = [ordered]@{
  schema = "xiigen.codex_host_ui_access_recovery_01.packet.v1"
  subagent_id_or_nickname = "CODEX-HOST-UI-ACCESS-RECOVERY-01"
  generated_at_local = $timestamp.ToString("o")
  active_luba_quote_creating_authority = $authorityQuote
  requested_artifact_type = "no-secret Codex host route recovery evidence, or exact requires-user-visible-host-action blocker + state update"
  changed_files = $changedFiles
  exact_files_read = $exactFilesRead
  exact_files_written = $exactFilesWritten
  commands_or_tools_used = $commandsOrToolsUsed
  route_recovery_status = "BLOCKED"
  codex_readiness_status = "BLOCKED"
  codex_readiness_exact_gates = $codexGates
  state_transition_made_or_not = "MADE: appended $markerKey with route_recovery_status=BLOCKED, codex_readiness_status=BLOCKED, exact blockers, and next bounded scope."
  state_marker_key = $markerKey
  next_bounded_worker_scope = $nextScope
  evidence_supports_only = "This worker proves only that no safe sub-agent-accessible user-visible Codex host route is exposed right now. It also proves partial local facts: Row23 input is safe, browser tooling exists, Codex processes and installed plugin cache exist, and Codex CLI is discoverable but blocked by Access denied. It does not prove a live Codex host answer, same-scenario Path B, token savings, semantic quality improvement, learnability/training, lifecycle, rollback, documentation, or full three-extension readiness."
  blockers = $blockers
  redaction_and_safety = [ordered]@{
    no_secrets_captured = $true
    scalar_values_from_codex_global_state_emitted = $false
    browser_window_titles_captured = $false
    process_window_titles_captured = $false
    external_ai_calls_made = $false
    external_ai_prompt_submitted = $false
    network_installs_or_downloads_made = $false
    git_actions_performed = $false
    source_edits_performed = $false
  }
  no_overclaim_booleans = [ordered]@{
    codex_full_readiness_claimed = $false
    codex_host_ui_route_pass_claimed = $false
    full_codex_chat_ui_proven = $false
    real_codex_path_b_ui_proven = $false
    token_savings_proven = $false
    quality_improvement_proven = $false
    learnability_or_training_proven = $false
    final_dual_path_scenario_coverage_proven = $false
  }
  parent_will_perform_commit_push_after_review = $true
  must_stop_after_report = $true
}

$packet | ConvertTo-Json -Depth 16 | Out-File -LiteralPath $PacketJsonPath -Encoding utf8

$md = @"
# CODEX-HOST-UI-ACCESS-RECOVERY-01 Packet

route_recovery_status: BLOCKED
codex_readiness_status: BLOCKED
state_marker_key: $markerKey

Evidence supports only: no safe sub-agent-accessible user-visible Codex host route is exposed right now. Browser tooling exists, Codex processes and installed plugin cache exist, and Codex CLI is discoverable, but the worker has no claimable browser tab, no Codex host MCP route, and Codex CLI/plugin commands fail with Access denied.

Required user-visible/tool action: expose an already-open in-app Browser tab to a Codex host/plugin/thread page, provide a callable Codex app/thread/host automation tool, or fix Codex CLI/plugin execution so the next bounded worker can rerun Row23 without secrets and without external AI submission beyond the approved Row23 path.

Blockers:
- $($blockers -join "`n- ")

No overclaim: no live Codex host answer, same-scenario Path B, token savings, quality improvement, learnability/training, lifecycle, rollback, documentation, or full three-extension readiness was proven.
"@
$md | Out-File -LiteralPath $PacketMdPath -Encoding utf8

$marker = [ordered]@{
  schema = "xiigen.codex_host_ui_access_recovery_01.state_marker.v1"
  subagent_id_or_nickname = "CODEX-HOST-UI-ACCESS-RECOVERY-01"
  timestamp = $timestamp.ToString("o")
  evidence_packet = $PacketJsonPath
  route_recovery_status = "BLOCKED"
  codex_readiness_status = "BLOCKED"
  row23_prompt_id = "row23-codex-safe-local-semantic-quality-input-001"
  routes_inspected = @(
    "in-app browser tabs/selected tab",
    "Codex CLI/plugin commands",
    "MCP/resource surfaces",
    "known Codex local metadata/config path inventory",
    "Codex global-state key/type/count shape inspection",
    "installed XIIGen Codex plugin cache metadata",
    "prior route-proof blocker packet"
  )
  next_bounded_worker_scope = $nextScope
  evidence_supports_only = $packet.evidence_supports_only
  blockers = $blockers
  parent_will_perform_commit_push_after_review = $true
  git_actions_performed_by_this_subagent = $false
}

$markerJson = $marker | ConvertTo-Json -Depth 16
$appendText = ",`r`n    `"$markerKey`": $markerJson`r`n}`r`n"
$appendBytes = [System.Text.Encoding]::UTF8.GetBytes($appendText)
$fs = [System.IO.File]::Open($StatePath, [System.IO.FileMode]::Open, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::Read)
try {
  $pos = $fs.Length - 1
  while ($pos -ge 0) {
    $fs.Seek($pos, [System.IO.SeekOrigin]::Begin) | Out-Null
    $byte = $fs.ReadByte()
    if ($byte -notin @(9, 10, 13, 32)) {
      break
    }
    $pos--
  }
  if ($pos -lt 0 -or $byte -ne 125) {
    throw "State file final non-whitespace byte is not a closing brace."
  }
  $fs.SetLength($pos)
  $fs.Seek($pos, [System.IO.SeekOrigin]::Begin) | Out-Null
  $fs.Write($appendBytes, 0, $appendBytes.Length)
} finally {
  $fs.Dispose()
}

[ordered]@{
  packet = $PacketJsonPath
  packet_md = $PacketMdPath
  state = $StatePath
  marker_key = $markerKey
} | ConvertTo-Json -Depth 4
