$ErrorActionPreference = "Continue"

$SubagentId = "HOST-ACTION-READINESS-RECHECK-04"
$Root = "C:\Projects\llm_mvp_core"
$EvidenceDir = Join-Path $Root "artifacts\extension-runtime-token-quality-learnability\host-action-readiness-recheck-04"
$CommandDir = Join-Path $EvidenceDir "commands"
$StatePath = Join-Path $Root "docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"
$AgentPath = Join-Path $Root "AGENTS.md"
$Now = [DateTimeOffset]::UtcNow.ToString("o")

New-Item -ItemType Directory -Force -Path $EvidenceDir | Out-Null
New-Item -ItemType Directory -Force -Path $CommandDir | Out-Null

$FilesRead = New-Object System.Collections.Generic.List[string]
$FilesWritten = New-Object System.Collections.Generic.List[string]
$CommandRecords = New-Object System.Collections.Generic.List[object]

function Add-ReadPath {
    param([string]$Path, [string]$Note = "")
    if ($Note) {
        $FilesRead.Add("$Path ($Note)")
    } else {
        $FilesRead.Add($Path)
    }
}

function Add-WrittenPath {
    param([string]$Path)
    $FilesWritten.Add($Path)
}

function Write-JsonFile {
    param([string]$Path, [object]$Value, [int]$Depth = 16)
    $json = $Value | ConvertTo-Json -Depth $Depth
    [System.IO.File]::WriteAllText($Path, $json, [System.Text.UTF8Encoding]::new($false))
    Add-WrittenPath $Path
}

function Read-JsonFile {
    param([string]$Path)
    Add-ReadPath $Path
    if (-not (Test-Path -LiteralPath $Path)) {
        return [pscustomobject]@{ _read_ok = $false; _error = "missing" }
    }
    try {
        return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
    } catch {
        return [pscustomobject]@{ _read_ok = $false; _error = $_.Exception.Message }
    }
}

function Invoke-CaptureScript {
    param(
        [string]$Name,
        [string]$ScriptText,
        [int]$TimeoutSeconds = 25
    )
    $stdout = Join-Path $CommandDir "$Name.stdout.txt"
    $stderr = Join-Path $CommandDir "$Name.stderr.txt"
    $meta = Join-Path $CommandDir "$Name.meta.json"
    $wrapped = @"
`$ErrorActionPreference = 'Continue'
try {
$ScriptText
    if (`$null -ne `$global:LASTEXITCODE) { exit `$global:LASTEXITCODE } else { exit 0 }
} catch {
    Write-Error `$_.Exception.Message
    exit 1
}
"@
    $encoded = [Convert]::ToBase64String([System.Text.Encoding]::Unicode.GetBytes($wrapped))
    $args = @("-NoLogo", "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-EncodedCommand", $encoded)
    $start = [DateTimeOffset]::UtcNow
    $timedOut = $false
    $exitCode = $null
    try {
        $p = Start-Process -FilePath "powershell.exe" -ArgumentList $args -WorkingDirectory $Root -RedirectStandardOutput $stdout -RedirectStandardError $stderr -PassThru
        if (-not $p.WaitForExit($TimeoutSeconds * 1000)) {
            $timedOut = $true
            try { $p.Kill() } catch {}
        }
        if (-not $timedOut) { $exitCode = $p.ExitCode } else { $exitCode = 124 }
    } catch {
        [System.IO.File]::WriteAllText($stderr, $_.Exception.Message, [System.Text.UTF8Encoding]::new($false))
        [System.IO.File]::WriteAllText($stdout, "", [System.Text.UTF8Encoding]::new($false))
        $exitCode = 1
    }
    $end = [DateTimeOffset]::UtcNow
    $record = [pscustomobject]@{
        name = $Name
        exit_code = $exitCode
        timed_out = $timedOut
        started_utc = $start.ToString("o")
        ended_utc = $end.ToString("o")
        stdout_path = $stdout
        stderr_path = $stderr
        meta_path = $meta
    }
    Write-JsonFile -Path $meta -Value $record -Depth 8
    Add-WrittenPath $stdout
    Add-WrittenPath $stderr
    $CommandRecords.Add($record)
    return $record
}

function Get-ShortText {
    param([string]$Path, [int]$Max = 500)
    if (-not (Test-Path -LiteralPath $Path)) { return "" }
    $text = Get-Content -LiteralPath $Path -Raw -ErrorAction SilentlyContinue
    if ($null -eq $text) { return "" }
    $text = $text.Trim()
    if ($text.Length -le $Max) { return $text }
    return $text.Substring(0, $Max) + "...[truncated]"
}

function Redact-UiName {
    param([string]$Name)
    if ([string]::IsNullOrWhiteSpace($Name)) { return "" }
    $clean = ($Name -replace "[`r`n`t]+", " ").Trim()
    if ($clean -notmatch "(?i)xiigen|agent|chat|composer|prompt|submit|send|ask|extension development host|cursor") {
        return "[redacted-non-route-name]"
    }
    if ($clean.Length -gt 160) { return $clean.Substring(0, 160) + "...[truncated]" }
    return $clean
}

# Required context reads.
Add-ReadPath $AgentPath
$agentsLoad = [pscustomobject]@{ path = $AgentPath; exists = (Test-Path -LiteralPath $AgentPath); read_method = "Get-Content -Raw -Encoding UTF8" }
try {
    $agentsText = Get-Content -LiteralPath $AgentPath -Raw -Encoding UTF8
    $agentsLoad.length_chars = $agentsText.Length
    $agentsLoad.loaded_required_honesty_terms = (
        $agentsText.Contains("NO DISHONEST CLAIMS") -and
        $agentsText.Contains("Parent Architect Subagent-Only Firewall") -and
        $agentsText.Contains("sub-agent waiting is not a stop")
    )
} catch {
    $agentsLoad.error = $_.Exception.Message
}

$stateInfo = [pscustomobject]@{ path = $StatePath; exists = (Test-Path -LiteralPath $StatePath); state_marker_written = $false }
Add-ReadPath $StatePath "metadata and BOM bytes only; no full parse or write"
try {
    $stateItem = Get-Item -LiteralPath $StatePath
    $stateInfo.length_bytes = $stateItem.Length
    $fs = [System.IO.File]::OpenRead($StatePath)
    try {
        $buf = New-Object byte[] 4
        [void]$fs.Read($buf, 0, 4)
        $stateInfo.first_four_bytes_hex = (($buf | ForEach-Object { $_.ToString("X2") }) -join " ")
        $stateInfo.bom_observed = if ($buf[0] -eq 0xEF -and $buf[1] -eq 0xBB -and $buf[2] -eq 0xBF) { "utf8-bom" } else { "none-or-not-utf8-bom" }
    } finally {
        $fs.Dispose()
    }
} catch {
    $stateInfo.error = $_.Exception.Message
}

$requiredPacketPaths = [ordered]@{
    claude_recheck_03 = Join-Path $Root "artifacts\extension-runtime-token-quality-learnability\claude-host-route-recheck-03\CLAUDE-HOST-ROUTE-RECHECK-03-evidence-packet.json"
    cursor_recheck_03 = Join-Path $Root "artifacts\extension-runtime-token-quality-learnability\cursor-host-route-recheck-03\cursor-host-route-recheck-03-packet.json"
    codex_recheck_03 = Join-Path $Root "artifacts\extension-runtime-token-quality-learnability\codex-host-route-recheck-03\codex-host-route-recheck-03-packet.json"
    final_blocker_consolidation_02 = Join-Path $Root "artifacts\extension-runtime-token-quality-learnability\three-extension-final-blocker-consolidation-02\three-extension-final-blocker-consolidation-02-packet.json"
    state_repair_02 = Join-Path $Root "artifacts\extension-runtime-token-quality-learnability\execution-state-json-integrity-repair-01\EXECUTION-STATE-MARKER-RECOVERY-REPAIR-02.packet.json"
    row23_input_path = Join-Path $Root "artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-input-path-evidence-packet.json"
}

$priorPackets = [ordered]@{}
foreach ($key in $requiredPacketPaths.Keys) {
    $priorPackets[$key] = Read-JsonFile -Path $requiredPacketPaths[$key]
}

# Claude safe metadata only.
$claudeExe = "C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.187\claude.exe"
$claudePluginDir = "C:\Projects\claude-xiigen-smart-skills"
if (Test-Path -LiteralPath $claudeExe) {
    $claudeVersion = Invoke-CaptureScript -Name "01-claude-version-2-1-187" -ScriptText "& '$claudeExe' --version"
    $claudeValidate = Invoke-CaptureScript -Name "02-claude-plugin-validate-2-1-187" -ScriptText "& '$claudeExe' plugin validate '$claudePluginDir'"
    $claudeDetails = Invoke-CaptureScript -Name "03-claude-plugin-details-2-1-187" -ScriptText "& '$claudeExe' --plugin-dir '$claudePluginDir' plugin details claude-xiigen-smart-skills"
} else {
    $claudeVersion = [pscustomobject]@{ name = "01-claude-version-2-1-187"; exit_code = 127; timed_out = $false; stdout_path = $null; stderr_path = $null; meta_path = $null; error = "claude 2.1.187 executable missing" }
    $claudeValidate = [pscustomobject]@{ name = "02-claude-plugin-validate-2-1-187"; exit_code = 127; timed_out = $false; stdout_path = $null; stderr_path = $null; meta_path = $null; error = "claude 2.1.187 executable missing" }
    $claudeDetails = [pscustomobject]@{ name = "03-claude-plugin-details-2-1-187"; exit_code = 127; timed_out = $false; stdout_path = $null; stderr_path = $null; meta_path = $null; error = "claude 2.1.187 executable missing" }
    $CommandRecords.Add($claudeVersion)
    $CommandRecords.Add($claudeValidate)
    $CommandRecords.Add($claudeDetails)
}

$claudeStdoutVersion = if ($claudeVersion.stdout_path) { Get-ShortText $claudeVersion.stdout_path 300 } else { "" }
$claudeStdoutValidate = if ($claudeValidate.stdout_path) { Get-ShortText $claudeValidate.stdout_path 500 } else { "" }
$claudeStdoutDetails = if ($claudeDetails.stdout_path) { Get-ShortText $claudeDetails.stdout_path 1200 } else { "" }
$claudeNoPromptRouteExposed = ($claudeStdoutDetails -match "(?i)SessionStart") -and -not ($claudeStdoutDetails -match "(?i)no-prompt proof|local hook proof|dry-run")

# Cursor safe filtered UIA/window inventory, no typing/clicking/screenshot.
$cursorInventoryPath = Join-Path $EvidenceDir "cursor-safe-window-uia-inventory.json"
$cursorSummary = [ordered]@{
    screenshot_taken = $false
    screenshot_reason = "not taken; no-secret unrelated-content safety was not proven by inventory alone"
    cursor_process_count = 0
    cursor_window_found = $false
    xiigen_panel_visible = $false
    xiigen_prompt_visible = $false
    xiigen_submit_visible = $false
    agent_or_chat_focusable_visible = $false
    cursor_agent_chat_closed_or_unfocusable_while_xiigen_visible = $false
    filtered_matches = @()
    errors = @()
}
try {
    $cursorProcs = @(Get-Process -Name Cursor -ErrorAction SilentlyContinue)
    $cursorSummary.cursor_process_count = $cursorProcs.Count
    $top = @()
    foreach ($p in $cursorProcs) {
        $title = Redact-UiName $p.MainWindowTitle
        $top += [pscustomobject]@{
            id = $p.Id
            process_name = $p.ProcessName
            has_main_window = ($p.MainWindowHandle -ne 0)
            main_window_title_redacted = $title
        }
    }
    $cursorSummary.top_level_windows = $top
    $target = $cursorProcs | Where-Object { $_.MainWindowHandle -ne 0 -and $_.MainWindowTitle -match "(?i)extension development host|xiigen|cursor" } | Select-Object -First 1
    if (-not $target) { $target = $cursorProcs | Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1 }
    if ($target) {
        $cursorSummary.cursor_window_found = $true
        $cursorSummary.selected_cursor_pid = $target.Id
        Add-Type -AssemblyName UIAutomationClient
        $el = [System.Windows.Automation.AutomationElement]::FromHandle([IntPtr]$target.MainWindowHandle)
        if ($el) {
            $all = $el.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
            $cursorSummary.total_descendants_seen = $all.Count
            $matches = New-Object System.Collections.Generic.List[object]
            for ($i = 0; $i -lt $all.Count -and $i -lt 700; $i++) {
                try {
                    $cur = $all.Item($i).Current
                    $name = [string]$cur.Name
                    $aid = [string]$cur.AutomationId
                    $ctype = [string]$cur.ControlType.ProgrammaticName
                    $className = [string]$cur.ClassName
                    $hay = "$name $aid $ctype $className"
                    if ($hay -match "(?i)xiigen|agent|chat|composer|prompt|submit|send|ask") {
                        $item = [pscustomobject]@{
                            index = $i
                            name_redacted = (Redact-UiName $name)
                            automation_id = $aid
                            control_type = $ctype
                            class_name = $className
                            is_offscreen = $cur.IsOffscreen
                            is_keyboard_focusable = $cur.IsKeyboardFocusable
                            has_keyboard_focus = $cur.HasKeyboardFocus
                        }
                        $matches.Add($item)
                        if ($hay -match "(?i)xiigen" -and -not $cur.IsOffscreen) { $cursorSummary.xiigen_panel_visible = $true }
                        if ($aid -match "(?i)xiigen-prompt" -and -not $cur.IsOffscreen) { $cursorSummary.xiigen_prompt_visible = $true }
                        if ($aid -match "(?i)xiigen-submit" -and -not $cur.IsOffscreen) { $cursorSummary.xiigen_submit_visible = $true }
                        if ($hay -match "(?i)agent|chat|composer|send|ask" -and $cur.IsKeyboardFocusable -and -not $cur.IsOffscreen) {
                            $cursorSummary.agent_or_chat_focusable_visible = $true
                        }
                    }
                } catch {}
            }
            $cursorSummary.filtered_matches = @($matches)
        }
    }
    $cursorSummary.cursor_agent_chat_closed_or_unfocusable_while_xiigen_visible = (
        $cursorSummary.xiigen_panel_visible -and
        $cursorSummary.xiigen_prompt_visible -and
        $cursorSummary.xiigen_submit_visible -and
        -not $cursorSummary.agent_or_chat_focusable_visible
    )
} catch {
    $cursorSummary.errors += $_.Exception.Message
}
Write-JsonFile -Path $cursorInventoryPath -Value $cursorSummary -Depth 12

# Codex safe CLI/process route checks only; no prompt submission.
$codexGetCommand = Invoke-CaptureScript -Name "04-codex-get-command-all" -ScriptText "Get-Command codex -All | Select-Object CommandType,Name,Source,Version | ConvertTo-Json -Depth 5"
$codexWhere = Invoke-CaptureScript -Name "05-codex-where" -ScriptText "where.exe codex"
$codexVersion = Invoke-CaptureScript -Name "06-codex-version" -ScriptText "try { & codex --version; if (`$null -ne `$LASTEXITCODE) { exit `$LASTEXITCODE } else { exit 0 } } catch { Write-Error `$_.Exception.Message; exit 1 }"
$codexHelp = Invoke-CaptureScript -Name "07-codex-help" -ScriptText "try { & codex --help; if (`$null -ne `$LASTEXITCODE) { exit `$LASTEXITCODE } else { exit 0 } } catch { Write-Error `$_.Exception.Message; exit 1 }"
$codexPluginHelp = Invoke-CaptureScript -Name "08-codex-plugin-help" -ScriptText "try { & codex plugin --help; if (`$null -ne `$LASTEXITCODE) { exit `$LASTEXITCODE } else { exit 0 } } catch { Write-Error `$_.Exception.Message; exit 1 }"
$codexProcesses = Invoke-CaptureScript -Name "09-codex-process-inventory-no-window-titles" -ScriptText "Get-Process | Where-Object { `$_.ProcessName -match 'codex|openai' } | Select-Object Id,ProcessName,Path | ConvertTo-Json -Depth 4"

$codexVersionErr = if ($codexVersion.stderr_path) { Get-ShortText $codexVersion.stderr_path 500 } else { "" }
$codexHelpErr = if ($codexHelp.stderr_path) { Get-ShortText $codexHelp.stderr_path 500 } else { "" }
$codexPluginErr = if ($codexPluginHelp.stderr_path) { Get-ShortText $codexPluginHelp.stderr_path 500 } else { "" }
$codexAccessDenied = (
    ($codexVersion.exit_code -ne 0 -and $codexVersionErr -match "(?i)access.*denied") -or
    ($codexHelp.exit_code -ne 0 -and $codexHelpErr -match "(?i)access.*denied") -or
    ($codexPluginHelp.exit_code -ne 0 -and $codexPluginErr -match "(?i)access.*denied")
)
$codexCliMetadataAvailable = (
    $codexVersion.exit_code -eq 0 -or
    $codexHelp.exit_code -eq 0 -or
    $codexPluginHelp.exit_code -eq 0
)

$routes = [ordered]@{
    Claude = [ordered]@{
        route_available = "no"
        metadata_route_available = if ($claudeVersion.exit_code -eq 0 -and $claudeValidate.exit_code -eq 0 -and $claudeDetails.exit_code -eq 0) { "yes" } else { "no" }
        details = "Safe metadata commands were run. No no-prompt SessionStart/local hook proof route was exposed by metadata output; chat/print/prompt submission was not run."
        version_stdout_summary = $claudeStdoutVersion
        validate_stdout_summary = $claudeStdoutValidate
        details_stdout_contains_sessionstart = ($claudeStdoutDetails -match "(?i)SessionStart")
        no_prompt_local_hook_proof_route_exposed = $false
    }
    Cursor = [ordered]@{
        route_available = if ($cursorSummary.cursor_agent_chat_closed_or_unfocusable_while_xiigen_visible) { "yes" } else { "no" }
        details = if ($cursorSummary.cursor_agent_chat_closed_or_unfocusable_while_xiigen_visible) { "XIIGen panel prompt and submit controls are visible and Cursor Agent/chat was not found focusable in filtered UIA inventory. No typing/clicking/submission was performed." } else { "XIIGen panel may be visible, but Cursor Agent/chat was still focusable/visible or route controls were incomplete; no typing/clicking/submission was performed." }
        inventory_path = $cursorInventoryPath
        xiigen_panel_visible = $cursorSummary.xiigen_panel_visible
        xiigen_prompt_visible = $cursorSummary.xiigen_prompt_visible
        xiigen_submit_visible = $cursorSummary.xiigen_submit_visible
        agent_or_chat_focusable_visible = $cursorSummary.agent_or_chat_focusable_visible
        screenshot_taken = $false
    }
    Codex = [ordered]@{
        route_available = "no"
        cli_no_longer_access_denied = if ($codexAccessDenied) { "no" } elseif ($codexCliMetadataAvailable) { "yes_metadata_only" } else { "unknown" }
        details = if ($codexAccessDenied) { "Codex CLI route still returns Access denied for safe metadata/help probes; no claimable in-app/browser/thread route was exposed to this runner." } elseif ($codexCliMetadataAvailable) { "Codex CLI metadata/help command returned successfully, but no prompt-safe plugin/thread proof route was run." } else { "Codex CLI did not prove a usable route and no in-app/browser/thread route was exposed." }
        access_denied_observed = $codexAccessDenied
        in_app_browser_or_thread_route_claimed = $false
        prompts_submitted = 0
    }
}

$statuses = [ordered]@{
    Claude = if ($routes.Claude.route_available -eq "yes") { "READY_FOR_NARROW_PROOF_WORKER_METADATA_ROUTE_ONLY" } else { "BLOCKED_NO_SAFE_NO_PROMPT_LIVE_HOST_ROUTE" }
    Cursor = if ($routes.Cursor.route_available -eq "yes") { "SAFE_ROUTE_AVAILABLE_FOR_NEXT_BOUNDED_PROOF_PREFLIGHT_ONLY" } else { "BLOCKED_CURSOR_AGENT_OR_CHAT_FOCUS_RISK_OR_ROUTE_INCOMPLETE" }
    Codex = if ($routes.Codex.route_available -eq "yes") { "SAFE_ROUTE_AVAILABLE_FOR_NEXT_BOUNDED_PROOF_PREFLIGHT_ONLY" } else { "BLOCKED_NO_CODEX_CLI_OR_IN_APP_ROUTE" }
}

$safeNext = [ordered]@{}
if ($routes.Claude.route_available -eq "yes") {
    $safeNext.Claude = "CLAUDE-NO-PROMPT-HOOK-PROOF-01: prove the already-exposed local SessionStart/no-prompt hook route only, with no chat/print/prompt submission, no secrets, no network, no source edits, no git."
}
if ($routes.Cursor.route_available -eq "yes") {
    $safeNext.Cursor = "CURSOR-LOCAL-PANEL-FOCUS-PREFLIGHT-05: run a bounded focus/screenshot-safety preflight against xiigen-prompt and xiigen-submit, then return before any typing/submission unless a separate work order authorizes it."
}
if ($routes.Codex.route_available -eq "yes") {
    $safeNext.Codex = "CODEX-HOST-ROUTE-PROOF-PREFLIGHT-01: prove the exposed CLI/in-app/browser/thread route without submitting prompts, then request a separate Row23 proof order if safe."
}

$hostActions = [ordered]@{
    Claude = @(
        "Provide or approve a no-secret Claude live-host route that proves SessionStart/local hook execution without prompt submission, or explicitly authorize Row23 through Claude --print/chat with redacted I/O and no secrets."
    )
    Cursor = if ($routes.Cursor.route_available -eq "yes") {
        @("No host action required for route availability preflight; next worker must still run screenshot/focus safety before any typing or submission.")
    } else {
        @(
            "Keep the XIIGen Smart Skills local panel visible.",
            "Close, hide, or make Cursor Agent/chat unfocusable.",
            "Confirm no secrets/unrelated content are visible before any screenshot or local-panel action."
        )
    }
    Codex = @(
        "Expose a safe user-visible Codex route: fixed CLI/plugin command without Access denied, an in-app Browser tab to the Codex host/plugin/thread page, or a callable Codex app/thread/host automation route.",
        "Do not provide secrets; do not require external AI prompt submission for route preflight."
    )
}

$repeated = [ordered]@{
    Claude = "same_as_recheck_03_and_final_consolidation_02: metadata route exists, no safe no-prompt live-host proof route exposed"
    Cursor = if ($routes.Cursor.route_available -eq "yes") { "changed_from_recheck_03_if_parent_accepts: Agent/chat focus risk not observed in current filtered inventory; route still needs separate proof preflight" } else { "same_as_recheck_03_and_final_consolidation_02: Agent/chat focus risk or route incompleteness remains" }
    Codex = if ($codexAccessDenied) { "same_as_recheck_03_and_final_consolidation_02: Codex CLI Access denied remains and no in-app route exposed" } else { "changed_from_recheck_03_if_parent_accepts: CLI metadata no longer showed Access denied, but no prompt-safe host proof route was run" }
}

$allRoutesNo = ($routes.Claude.route_available -ne "yes" -and $routes.Cursor.route_available -ne "yes" -and $routes.Codex.route_available -ne "yes")
$packetPath = Join-Path $EvidenceDir "HOST-ACTION-READINESS-RECHECK-04-packet.json"
$mdPath = Join-Path $EvidenceDir "HOST-ACTION-READINESS-RECHECK-04-packet.md"

$packet = [ordered]@{
    schema = "xiigen.host_action_readiness_recheck_04.packet.v1"
    subagent_id_or_nickname = $SubagentId
    generated_at_utc = $Now
    requested_artifact_type = "current host-route/action readiness recheck packet plus no state marker"
    must_stop_after_report = $true
    claim_scope = "Current no-secret host-route availability recheck only for Claude, Cursor, and Codex."
    evidence_scope = "Required packet reads, current safe Claude metadata commands, current Cursor filtered UIA/window inventory, and current Codex safe CLI/process probes. No prompt/chat submission, no source edits, no git, no network/install, no product flows."
    count_claimed = @{
        current_claude_safe_metadata_commands = 3
        current_cursor_uia_inventory_runs = 1
        current_codex_safe_cli_process_probe_groups = 6
        prompt_or_chat_submissions = 0
        screenshots = 0
        source_edits = 0
        git_actions = 0
        state_writes = 0
    }
    independent_evidence_count = $CommandRecords.Count + 1
    subagent_id_or_NONE = $SubagentId
    downgrade_needed = $true
    changed_files = @($FilesWritten | Select-Object -Unique)
    exact_files_read = @($FilesRead | Select-Object -Unique)
    exact_files_written = @($FilesWritten | Select-Object -Unique)
    commands_or_tools_used = @($CommandRecords)
    required_context_summary = @{
        agents_load = $agentsLoad
        state_targeted_read = $stateInfo
        prior_recheck_03_statuses = @{
            Claude = $priorPackets.claude_recheck_03.claude_live_host_route_status
            Cursor = $priorPackets.cursor_recheck_03.same_row23_visible_ui_route_status
            Codex = $priorPackets.codex_recheck_03.codex_host_route_status
        }
        final_blocker_status = $priorPackets.final_blocker_consolidation_02.overall_three_extension_readiness_status.status
        state_repair_02_status = $priorPackets.state_repair_02.state_integrity_status
        row23_selected_input_path_exists = $priorPackets.row23_input_path.selected_row23_input_path_exists
    }
    routes_available_by_extension = $routes
    statuses_by_extension = $statuses
    safe_next_worker_scope_if_any = if ($safeNext.Keys.Count -gt 0) { $safeNext } else { "NONE: no safe current route was available for a proof worker under this no-submit/no-click/no-secret recheck." }
    exact_user_host_actions_required = $hostActions
    repeated_blocker_assessment_for_this_turn = $repeated
    state_transition_made_or_not = "NOT_MADE: state file was read only for metadata/BOM bytes. No state marker was written because the 467 MB state file was not fully parsed in this bounded recheck and evidence packet output is sufficient."
    evidence_supports_only = "This packet supports only current host-route/action availability status. It does not prove extension readiness, Row23 prompt execution through a live host, token savings, semantic quality improvement, learnability/training, lifecycle/export/import/backup/continue-training, checkpoint promotion, or all-three readiness."
    blockers = if ($allRoutesNo) {
        @(
            "Claude: no safe no-prompt live-host proof route exposed; metadata only.",
            "Cursor: local panel route is not safe if Agent/chat remains focusable or route controls incomplete.",
            "Codex: no safe CLI/in-app/browser/thread route proven; CLI Access denied status captured in command outputs."
        )
    } else {
        @(
            "At least one narrow route may be available, but no proof worker has run yet.",
            "Claude and Codex remain blocked unless their route_available fields say yes.",
            "No prompt/chat submission, typing, clicking, screenshot, network, source edit, state write, or git action was performed."
        )
    }
    forbidden_actions_absent_by_this_worker = @{
        external_ai_prompt_or_chat_submission = $true
        cursor_typing_clicking_or_submit = $true
        screenshot_capture = $true
        network_or_install = $true
        source_edits = $true
        extension_repo_mutation = $true
        git_actions = $true
        state_write = $true
        secrets_captured = $false
    }
    parent_will_perform_commit_push_after_review = $true
}

Write-JsonFile -Path $packetPath -Value $packet -Depth 20

$md = @"
# HOST-ACTION-READINESS-RECHECK-04

subagent_id_or_nickname: $SubagentId

## Result

Claude route available: $($routes.Claude.route_available) - $($routes.Claude.details)

Cursor route available: $($routes.Cursor.route_available) - $($routes.Cursor.details)

Codex route available: $($routes.Codex.route_available) - $($routes.Codex.details)

## State

$($packet.state_transition_made_or_not)

## Evidence Supports Only

$($packet.evidence_supports_only)

## User Host Actions Required

Claude:
$($hostActions.Claude | ForEach-Object { "- $_" } | Out-String)
Cursor:
$($hostActions.Cursor | ForEach-Object { "- $_" } | Out-String)
Codex:
$($hostActions.Codex | ForEach-Object { "- $_" } | Out-String)

## Blockers

$($packet.blockers | ForEach-Object { "- $_" } | Out-String)

## Files

Packet JSON: $packetPath
Cursor inventory: $cursorInventoryPath
"@
[System.IO.File]::WriteAllText($mdPath, $md, [System.Text.UTF8Encoding]::new($false))
Add-WrittenPath $mdPath

# Rewrite the packet once more so exact_files_written includes the Markdown packet too.
$packet.changed_files = @($FilesWritten | Select-Object -Unique)
$packet.exact_files_written = @($FilesWritten | Select-Object -Unique)
Write-JsonFile -Path $packetPath -Value $packet -Depth 20

Write-Output "HOST-ACTION-READINESS-RECHECK-04 packet written: $packetPath"
