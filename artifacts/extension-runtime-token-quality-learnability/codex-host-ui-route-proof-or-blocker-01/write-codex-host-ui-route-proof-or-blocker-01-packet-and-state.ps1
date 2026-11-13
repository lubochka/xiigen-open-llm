param(
    [string]$OutRoot = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-host-ui-route-proof-or-blocker-01"
)

$ErrorActionPreference = "Stop"
$statePath = "C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"
$row23Path = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json"
$priorReadinessPath = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-ext-readiness-01\codex-ext-readiness-01-packet.json"
$priorPathBPath = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-ext-path-b-runtime-01\codex-ext-path-b-runtime-01-packet.json"
$routeDiscoveryPath = Join-Path $OutRoot "route-discovery-summary.json"
$browserEvidencePath = Join-Path $OutRoot "browser-route-evidence.json"
$packetPath = Join-Path $OutRoot "codex-host-ui-route-proof-or-blocker-01-packet.json"
$packetMdPath = Join-Path $OutRoot "codex-host-ui-route-proof-or-blocker-01-packet.md"

$row23 = Get-Content -Raw -Path $row23Path | ConvertFrom-Json
$priorReadiness = Get-Content -Raw -Path $priorReadinessPath | ConvertFrom-Json
$priorPathB = Get-Content -Raw -Path $priorPathBPath | ConvertFrom-Json
$routeDiscovery = Get-Content -Raw -Path $routeDiscoveryPath | ConvertFrom-Json
$browserEvidence = Get-Content -Raw -Path $browserEvidencePath | ConvertFrom-Json

$timestamp = Get-Date
$markerKey = "codex_host_ui_route_proof_or_blocker_01_" + $timestamp.ToString("yyyyMMdd_HHmmss")
$commandsUsed = @(
    [ordered]@{
        name = "Get-Content -Raw AGENTS.md"
        result = "exit_code=0; file content command executed; output was truncated by tool transcript, so packet relies on AGENTS metadata/hash and active work-order rules for narrow claims"
    },
    [ordered]@{
        name = "AGENTS metadata/hash"
        result = "exit_code=0; line_count=5183; sha256=6563AD96157AB9D07E1BD88B4CD6E0CEE8E8AD38AE8028C4029C637399588367"
    },
    [ordered]@{
        name = "plan/state/Row23/prior packet reads"
        result = "exit_code=0 for targeted plan excerpts, Row23 safe input, codex-ext-readiness-01 packet, codex-ext-path-b-runtime-01 packet; raw state read timed out once because file is 469MB, then targeted reads succeeded"
    },
    [ordered]@{
        name = "browser skill setup and browser.documentation"
        result = "Node REPL tool succeeded; in-app browser API docs emitted/read"
    },
    [ordered]@{
        name = "browser route evidence"
        result = "saved browser-route-evidence.json; in-app browser available; openUserTabs=[]; sessionTabs=[]; selectedTab_error='No active tab found'"
    },
    [ordered]@{
        name = "run-codex-host-ui-route-discovery-01.ps1"
        result = "exit_code=0; wrote route-discovery-summary.json and command metadata"
    }
)

foreach ($cmd in $routeDiscovery.route_discovery_commands) {
    $commandsUsed += [ordered]@{
        name = $cmd.name
        command_line = $cmd.command_line
        exit_code = $cmd.exit_code
        result = $(if ($cmd.error) { $cmd.error } else { "ok" })
        stdout_path = $cmd.stdout_path
        stderr_path = $cmd.stderr_path
    }
}

$exactFilesRead = @(
    "C:\Projects\llm_mvp_core\AGENTS.md",
    "C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md",
    $statePath,
    $row23Path,
    $priorReadinessPath,
    $priorPathBPath,
    "C:\Users\lubao\.codex\plugins\cache\openai-bundled\browser\26.616.51431\skills\control-in-app-browser\SKILL.md",
    "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\.codex-plugin\plugin.json",
    "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\package.json",
    $routeDiscoveryPath,
    $browserEvidencePath,
    (Join-Path $OutRoot "commands\02-codex-version.meta.json"),
    (Join-Path $OutRoot "commands\05-codex-plugin-list-json.meta.json")
)

$exactFilesWritten = @(
    (Join-Path $OutRoot "run-codex-host-ui-route-discovery-01.ps1"),
    (Join-Path $OutRoot "route-discovery-summary.json"),
    (Join-Path $OutRoot "browser-route-evidence.json"),
    (Join-Path $OutRoot "commands\01-get-command-codex.stdout.txt"),
    (Join-Path $OutRoot "commands\01-get-command-codex.stderr.txt"),
    (Join-Path $OutRoot "commands\01-get-command-codex.meta.json"),
    (Join-Path $OutRoot "commands\02-codex-version.stdout.txt"),
    (Join-Path $OutRoot "commands\02-codex-version.stderr.txt"),
    (Join-Path $OutRoot "commands\02-codex-version.meta.json"),
    (Join-Path $OutRoot "commands\03-codex-help.stdout.txt"),
    (Join-Path $OutRoot "commands\03-codex-help.stderr.txt"),
    (Join-Path $OutRoot "commands\03-codex-help.meta.json"),
    (Join-Path $OutRoot "commands\04-codex-plugin-help.stdout.txt"),
    (Join-Path $OutRoot "commands\04-codex-plugin-help.stderr.txt"),
    (Join-Path $OutRoot "commands\04-codex-plugin-help.meta.json"),
    (Join-Path $OutRoot "commands\05-codex-plugin-list-json.stdout.txt"),
    (Join-Path $OutRoot "commands\05-codex-plugin-list-json.stderr.txt"),
    (Join-Path $OutRoot "commands\05-codex-plugin-list-json.meta.json"),
    (Join-Path $OutRoot "commands\06-codex-process-inventory.stdout.json"),
    (Join-Path $OutRoot "commands\06-codex-process-inventory.stderr.txt"),
    $packetPath,
    $packetMdPath,
    $statePath
)

$packet = [ordered]@{
    schema = "xiigen.codex_host_ui_route_proof_or_blocker_01.packet.v1"
    subagent_id_or_nickname = "CODEX-HOST-UI-ROUTE-PROOF-OR-BLOCKER-01"
    generated_at_local = $timestamp.ToString("o")
    active_luba_quote_creating_authority = $row23.approval_quote_used
    requested_artifact_type = "user-visible Codex host route proof for same Row23 prompt, OR exact blocker packet with evidence + state update"
    changed_files = $exactFilesWritten
    exact_files_read = $exactFilesRead
    exact_files_written = $exactFilesWritten
    commands_or_tools_used = $commandsUsed
    row23_same_prompt = [ordered]@{
        prompt_id = $row23.scenario_input.prompt_id
        prompt_text = $row23.scenario_input.prompt_text
        scenario_id = $row23.scenario_id
        host = $row23.host
        no_secrets_in_input = $true
    }
    routes_inspected = [ordered]@{
        in_app_browser = [ordered]@{
            status = "BLOCKED"
            evidence = $browserEvidencePath
            browser_available = $browserEvidence.route_findings.in_app_browser_available
            open_user_tabs_count = $browserEvidence.route_findings.open_user_tabs_count
            session_tabs_count = $browserEvidence.route_findings.session_tabs_count
            selected_tab_available = $browserEvidence.route_findings.selected_tab_available
            reason = $browserEvidence.route_findings.blocker_reason
        }
        codex_cli = [ordered]@{
            status = "BLOCKED"
            evidence = $routeDiscoveryPath
            codex_cli_found_on_path = $routeDiscovery.route_findings.codex_cli_found_on_path
            version_exit_code = (($routeDiscovery.route_discovery_commands | Where-Object { $_.name -eq "02-codex-version" }).exit_code)
            plugin_list_exit_code = (($routeDiscovery.route_discovery_commands | Where-Object { $_.name -eq "05-codex-plugin-list-json" }).exit_code)
            reason = "codex executable is on PATH, but safe invocations returned Access is denied, so this worker cannot prove a live host command route."
        }
        installed_plugin_cache = [ordered]@{
            status = "PARTIAL"
            evidence = $routeDiscoveryPath
            root_exists = $routeDiscovery.plugin_cache.root_exists
            manifest_exists = $routeDiscovery.plugin_cache.manifest_exists
            package_json_exists = $routeDiscovery.plugin_cache.package_json_exists
            reason = "Installed cache metadata proves package presence only; it is not a live user-visible Codex host UI/chat/app invocation."
        }
        prior_adapter_core_path_a = [ordered]@{
            status = "PARTIAL"
            evidence = $priorReadinessPath
            prior_subagent = $priorReadiness.subagent_id_or_nickname
            prior_status = $priorReadiness.codex_readiness_status
            evidence_scope = $priorReadiness.evidence_supports_only
        }
        prior_installed_cache_path_b_attempt = [ordered]@{
            status = $priorPathB.codex_path_b_status
            evidence = $priorPathBPath
            prior_subagent = $priorPathB.subagent_id_or_nickname
            comparison_status = $priorPathB.path_a_path_b_comparison_status
            evidence_scope = $priorPathB.evidence_supports_only
        }
    }
    codex_host_ui_route_status = "BLOCKED"
    codex_readiness_status = "BLOCKED"
    codex_readiness_exact_gates = [ordered]@{
        row23_safe_input_selected = $true
        prior_adapter_core_boundary_success = $true
        installed_plugin_cache_present = $routeDiscovery.route_findings.installed_plugin_cache_present
        in_app_browser_available = $browserEvidence.route_findings.in_app_browser_available
        in_app_browser_live_route_available = $false
        codex_cli_live_route_available = $false
        visible_codex_host_answer_proven = $false
        same_scenario_live_path_b_proven = $false
        token_savings_proven = $false
        semantic_quality_improvement_proven = $false
        learnability_or_training_proven = $false
        backup_export_import_continue_training_proven = $false
        rollback_or_target_safety_proven = $false
        final_three_extension_readiness_proven = $false
    }
    state_transition_made_or_not = "PENDING in packet draft; writer appends state marker after packet serialization."
    state_marker_key = $markerKey
    next_bounded_worker_scope = "CODEX-HOST-UI-ACCESS-RECOVERY-01: obtain or expose an accessible no-secret Codex host route for the same Row23 prompt, such as an already-open Codex app/plugin page, an authorized host automation surface, or a safe command route that can invoke the installed plugin without Access denied; then rerun this proof. No external AI prompt submission, no secrets, no network installs, no git."
    evidence_supports_only = "This turn proves a blocker for a user-visible Codex host UI/chat/app route from this sub-agent: the in-app browser has no open/session/selected tab, Codex CLI/plugin commands are blocked by Access denied, and installed plugin cache metadata is present but not a live host invocation. Prior evidence remains only adapter/core and installed-cache partial evidence for the Row23 safe input."
    blockers = @(
        "No user-visible Codex host page/thread/plugin route is exposed through the in-app browser: openUserTabs=[], sessionTabs=[], selectedTab_error='No active tab found'.",
        "Codex executable is discoverable on PATH, but codex --version, codex --help, codex plugin --help, and codex plugin list --json all returned exit_code=127 with Access is denied.",
        "Installed plugin cache metadata exists, but metadata/cache presence cannot prove a live Codex host UI/chat/app answer.",
        "Submitting the Row23 prompt to an external AI/chat surface was forbidden by the work order and was not done."
    )
    redaction_and_safety = [ordered]@{
        no_secrets_captured = $true
        browser_window_titles_captured = $false
        process_window_titles_redacted = $true
        external_ai_calls_made = $false
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

$packet.state_transition_made_or_not = "MADE: appended state marker $markerKey with codex_host_ui_route_status=BLOCKED and next bounded scope."
$packet | ConvertTo-Json -Depth 40 | Set-Content -Path $packetPath -Encoding UTF8

$md = @"
# CODEX-HOST-UI-ROUTE-PROOF-OR-BLOCKER-01

Status: BLOCKED for user-visible Codex host UI route.

Evidence supports only: $($packet.evidence_supports_only)

Routes inspected:
- In-app browser: BLOCKED. Browser exists, but openUserTabs=[], sessionTabs=[], and selectedTab is unavailable.
- Codex CLI/plugin command route: BLOCKED. codex is on PATH, but safe commands returned Access is denied.
- Installed plugin cache: PARTIAL. Cache metadata exists, but cache presence is not a live host UI/chat/app answer.
- Prior Row23 adapter/core evidence: PARTIAL only.

Next bounded worker scope:
$($packet.next_bounded_worker_scope)

No secrets captured. No external AI calls. No network installs/downloads. No git actions. No source edits.
"@
$md | Set-Content -Path $packetMdPath -Encoding UTF8

$stateEntry = [ordered]@{
    schema = "xiigen.codex_host_ui_route_proof_or_blocker_01.state_marker.v1"
    subagent_id_or_nickname = "CODEX-HOST-UI-ROUTE-PROOF-OR-BLOCKER-01"
    timestamp = $timestamp.ToString("o")
    evidence_packet = $packetPath
    codex_host_ui_route_status = "BLOCKED"
    codex_readiness_status = "BLOCKED"
    row23_prompt_id = $row23.scenario_input.prompt_id
    routes_inspected = @("in-app browser", "Codex CLI/plugin commands", "installed plugin cache metadata", "prior adapter/core packets")
    state_transition_made_or_not = "MADE: appended narrow CODEX-HOST-UI-ROUTE-PROOF-OR-BLOCKER-01 marker with BLOCKED host UI route status and explicit non-claims."
    next_bounded_worker_scope = $packet.next_bounded_worker_scope
    evidence_supports_only = $packet.evidence_supports_only
    blockers = $packet.blockers
    parent_will_perform_commit_push_after_review = $true
    git_actions_performed_by_this_subagent = $false
}

$stateText = [System.IO.File]::ReadAllText($statePath)
$idx = $stateText.Length - 1
while ($idx -ge 0 -and [char]::IsWhiteSpace($stateText[$idx])) {
    $idx--
}
if ($idx -lt 0 -or $stateText[$idx] -ne '}') {
    throw "State file does not end with a JSON object terminator."
}
$prefix = $stateText.Substring(0, $idx)
$suffix = $stateText.Substring($idx)
$entryJson = $stateEntry | ConvertTo-Json -Depth 30
$separator = $(if ($prefix.TrimEnd().EndsWith("{")) { "`r`n" } else { ",`r`n" })
$newStateText = $prefix + $separator + "    `"$markerKey`": " + $entryJson + "`r`n" + $suffix
[System.IO.File]::WriteAllText($statePath, $newStateText, [System.Text.UTF8Encoding]::new($false))

Write-Output $packetPath
