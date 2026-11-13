$ErrorActionPreference = "Stop"

$RunRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$LogPath = Join-Path $RunRoot "row10-s5d-preflight.log"
$PacketJsonPath = Join-Path $RunRoot "row10-s5d-preflight-packet.json"
$ReportMdPath = Join-Path $RunRoot "row10-s5d-preflight-report.md"
$SourceJsonPath = Join-Path $RunRoot "s5d-00-source-readback.json"
$PathStorageJsonPath = Join-Path $RunRoot "s5d-01-path-storage-preflight.json"
$ProcessJsonPath = Join-Path $RunRoot "s5d-02-stale-process-discovery.json"
$DesktopJsonPath = Join-Path $RunRoot "s5d-03-desktop-foreground-lock.json"
$DecisionJsonPath = Join-Path $RunRoot "s5d-04-launch-decision.json"

$Utf8NoBom = [System.Text.UTF8Encoding]::new($false)
function Write-Utf8File {
    param([string]$Path, [string]$Text)
    [System.IO.File]::WriteAllText($Path, $Text + [Environment]::NewLine, $Utf8NoBom)
}
function Save-Json {
    param([string]$Path, $Value)
    Write-Utf8File -Path $Path -Text ($Value | ConvertTo-Json -Depth 12)
}
function Add-Log {
    param([string]$Message)
    $line = "{0} {1}" -f (Get-Date -Format "o"), $Message
    [System.IO.File]::AppendAllText($LogPath, $line + [Environment]::NewLine, $Utf8NoBom)
}
function Test-ProbeFile {
    param([string]$Directory)
    $probePath = Join-Path $Directory "s5d-write-probe.tmp"
    $result = [ordered]@{
        directory = $Directory
        probe_path = $probePath
        write_ok = $false
        read_ok = $false
        delete_ok = $false
        error = $null
    }
    try {
        Write-Utf8File -Path $probePath -Text "row10-s5d-probe"
        $result.write_ok = Test-Path -LiteralPath $probePath -PathType Leaf
        $result.read_ok = ((Get-Content -LiteralPath $probePath -Raw) -eq ("row10-s5d-probe" + [Environment]::NewLine))
        Remove-Item -LiteralPath $probePath -Force
        $result.delete_ok = -not (Test-Path -LiteralPath $probePath)
    } catch {
        $result.error = $_.Exception.Message
    }
    return $result
}

New-Item -ItemType Directory -Force -Path $RunRoot | Out-Null
if (Test-Path -LiteralPath $LogPath) { Remove-Item -LiteralPath $LogPath -Force }
Add-Log "Row10-S5d preflight-only runner started."

$CursorExe = "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe"
$ExtensionRoot = "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills"
$PrepFolder = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s5c-clean-launch-no-send-work-order-prep-20260627-053922"
$ReviewFolder = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s5c-clean-launch-no-send-work-order-prep-review-20260627-054717"
$SourcePaths = [ordered]@{
    prep_markdown = Join-Path $PrepFolder "row10-s5c-clean-launch-no-send-work-order-prep.md"
    prep_json = Join-Path $PrepFolder "row10-s5c-clean-launch-no-send-work-order-prep.json"
    review_markdown = Join-Path $ReviewFolder "row10-s5c-clean-launch-no-send-work-order-prep-review.md"
    review_json = Join-Path $ReviewFolder "row10-s5c-clean-launch-no-send-work-order-prep-review.json"
}

$sourceReadback = [ordered]@{
    paths = $SourcePaths
    files_exist = [ordered]@{}
    json_parse = [ordered]@{}
    s5c_prep_verdict = $null
    s5c_review_verdict = $null
    s5c_launch_after_pass_authorized = $false
    stale_policy_read_only_stop_forbidden = $false
    prompt_typing_allowed = $null
    prompt_send_allowed = $null
    source_check_pass = $false
    errors = @()
}
foreach ($key in $SourcePaths.Keys) {
    $sourceReadback.files_exist[$key] = Test-Path -LiteralPath $SourcePaths[$key]
}
try {
    $prepJson = Get-Content -LiteralPath $SourcePaths.prep_json -Raw | ConvertFrom-Json
    $reviewJson = Get-Content -LiteralPath $SourcePaths.review_json -Raw | ConvertFrom-Json
    $sourceReadback.json_parse.prep_json = "PASS"
    $sourceReadback.json_parse.review_json = "PASS"
    $sourceReadback.s5c_prep_verdict = $prepJson.verdict
    $sourceReadback.s5c_review_verdict = $reviewJson.verdict
    $sourceReadback.s5c_launch_after_pass_authorized = [bool]$prepJson.future_work_order_decision.future_work_order_authorizes_launch
    $sourceReadback.stale_policy_read_only_stop_forbidden = (
        [bool]$prepJson.preflight_checks.stale_process_policy.read_only_discovery_authorized -and
        -not [bool]$prepJson.preflight_checks.stale_process_policy.stop_kill_close_authorized
    )
    $sourceReadback.prompt_typing_allowed = [bool]$prepJson.future_work_order_decision.prompt_typing_allowed
    $sourceReadback.prompt_send_allowed = [bool]$prepJson.future_work_order_decision.prompt_send_allowed
} catch {
    $sourceReadback.errors += $_.Exception.Message
}
$sourceReadback.source_check_pass = (
    -not ($sourceReadback.files_exist.Values -contains $false) -and
    $sourceReadback.json_parse.prep_json -eq "PASS" -and
    $sourceReadback.json_parse.review_json -eq "PASS" -and
    $sourceReadback.s5c_prep_verdict -eq "PASS_WORK_ORDER_PREP" -and
    $sourceReadback.s5c_review_verdict -eq "PASS_WORK_ORDER_PREP" -and
    $sourceReadback.s5c_launch_after_pass_authorized -and
    $sourceReadback.stale_policy_read_only_stop_forbidden -and
    -not $sourceReadback.prompt_typing_allowed -and
    -not $sourceReadback.prompt_send_allowed
)
Save-Json -Path $SourceJsonPath -Value $sourceReadback
Add-Log "Source readback saved."

$childDirs = [ordered]@{
    user_data_clean = Join-Path $RunRoot "user-data-clean"
    extensions_clean = Join-Path $RunRoot "extensions-clean"
    temp = Join-Path $RunRoot "temp"
    logs = Join-Path $RunRoot "logs"
}
$pathStorage = [ordered]@{
    cursor_exe = [ordered]@{ path = $CursorExe; exists = $false; length = $null; sha256 = $null; error = $null }
    extension_development_path = [ordered]@{ path = $ExtensionRoot; exists = $false; item_type = $null; error = $null }
    run_root = $RunRoot
    child_dirs = [ordered]@{}
    prior_s5_reuse_detected = $false
    storage_check_pass = $false
}
try {
    $pathStorage.cursor_exe.exists = Test-Path -LiteralPath $CursorExe -PathType Leaf
    if ($pathStorage.cursor_exe.exists) {
        $cursorItem = Get-Item -LiteralPath $CursorExe
        $pathStorage.cursor_exe.length = $cursorItem.Length
        $pathStorage.cursor_exe.sha256 = (Get-FileHash -LiteralPath $CursorExe -Algorithm SHA256).Hash
    }
} catch {
    $pathStorage.cursor_exe.error = $_.Exception.Message
}
try {
    $pathStorage.extension_development_path.exists = Test-Path -LiteralPath $ExtensionRoot -PathType Container
    if ($pathStorage.extension_development_path.exists) {
        $pathStorage.extension_development_path.item_type = (Get-Item -LiteralPath $ExtensionRoot).PSIsContainer
    }
} catch {
    $pathStorage.extension_development_path.error = $_.Exception.Message
}
foreach ($name in $childDirs.Keys) {
    $dir = $childDirs[$name]
    $entry = [ordered]@{ path = $dir; exists = $false; resolved = $null; path_length = $null; probe = $null; error = $null }
    try {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
        $entry.exists = Test-Path -LiteralPath $dir -PathType Container
        $entry.resolved = (Resolve-Path -LiteralPath $dir).Path
        $entry.path_length = $entry.resolved.Length
        if ($name -ne "logs") { $entry.probe = Test-ProbeFile -Directory $dir }
    } catch {
        $entry.error = $_.Exception.Message
    }
    $pathStorage.child_dirs[$name] = $entry
}
$priorMarkers = @(
    "row10-s5-cursor-launch-recovery-no-send-20260627-045817\user-data",
    "row10-s5-cursor-launch-recovery-no-send-20260627-045817\user-data-quoted-retry",
    "row10-s5-cursor-launch-recovery-no-send-20260627-045817\extensions",
    "row10-s5-cursor-launch-recovery-no-send-20260627-045817\extensions-quoted-retry"
)
$pathStorage.prior_s5_reuse_detected = $priorMarkers | Where-Object { $RunRoot -like "*$_*" } | ForEach-Object { $true } | Select-Object -First 1
if ($null -eq $pathStorage.prior_s5_reuse_detected) { $pathStorage.prior_s5_reuse_detected = $false }
$probesPass = $true
foreach ($probeName in @("user_data_clean", "extensions_clean", "temp")) {
    $probe = $pathStorage.child_dirs[$probeName].probe
    if (-not ($probe.write_ok -and $probe.read_ok -and $probe.delete_ok)) { $probesPass = $false }
}
$pathStorage.storage_check_pass = (
    $pathStorage.cursor_exe.exists -and
    $pathStorage.extension_development_path.exists -and
    -not $pathStorage.prior_s5_reuse_detected -and
    $probesPass -and
    $pathStorage.child_dirs.logs.exists
)
Save-Json -Path $PathStorageJsonPath -Value $pathStorage
Add-Log "Path and storage preflight saved."

$processDiscovery = [ordered]@{
    policy_from_s5c = "read_only_discovery_allowed_stop_kill_close_forbidden"
    process_stop_kill_close_performed = $false
    processes = @()
    cursor_process_count = 0
    stale_policy_check_pass = $false
    blockers = @()
}
try {
    $cursorProcesses = Get-CimInstance Win32_Process | Where-Object {
        $_.Name -ieq "Cursor.exe" -or
        ($_.ExecutablePath -and ([System.IO.Path]::GetFileName($_.ExecutablePath) -ieq "Cursor.exe"))
    }
    foreach ($proc in $cursorProcesses) {
        $cmd = $proc.CommandLine
        $classification = "default_or_no_user_data_flag"
        if ([string]::IsNullOrWhiteSpace($cmd)) {
            $classification = "unknown_command_line"
        } elseif ($cmd.IndexOf($RunRoot, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            $classification = "future_clean_dirs"
        } elseif ($priorMarkers | Where-Object { $cmd.IndexOf($_, [System.StringComparison]::OrdinalIgnoreCase) -ge 0 }) {
            $classification = "prior_s5_dirs"
        } elseif ($cmd -match "--user-data-dir") {
            $classification = "custom_user_data_dir_not_future"
        }
        $processDiscovery.processes += [ordered]@{
            process_id = $proc.ProcessId
            name = $proc.Name
            executable_path = $proc.ExecutablePath
            command_line = $cmd
            storage_dir_classification = $classification
            command_line_contains_future_run_root = ($cmd -and $cmd.IndexOf($RunRoot, [System.StringComparison]::OrdinalIgnoreCase) -ge 0)
        }
        if ($classification -eq "future_clean_dirs") {
            $processDiscovery.blockers += "existing_cursor_process_uses_future_run_root"
        }
    }
    $processDiscovery.cursor_process_count = @($processDiscovery.processes).Count
} catch {
    $processDiscovery.blockers += $_.Exception.Message
}
$processDiscovery.stale_policy_check_pass = (
    $sourceReadback.stale_policy_read_only_stop_forbidden -and
    @($processDiscovery.blockers).Count -eq 0
)
Save-Json -Path $ProcessJsonPath -Value $processDiscovery
Add-Log "Stale process discovery saved."

$desktopCheck = [ordered]@{
    api = "OpenInputDesktop/SwitchDesktop plus foreground process check; no screenshot captured"
    screenshot_captured = $false
    no_secrets_captured = $true
    desktop_unlocked = $false
    foreground_window_handle_present = $false
    foreground_process_id = $null
    foreground_process_name = $null
    lock_or_secret_surface_detected = $null
    desktop_check_pass = $false
    errors = @()
}
try {
    Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class S5dNative {
    [DllImport("user32.dll", SetLastError=true)]
    public static extern IntPtr OpenInputDesktop(uint dwFlags, bool fInherit, uint dwDesiredAccess);
    [DllImport("user32.dll", SetLastError=true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SwitchDesktop(IntPtr hDesktop);
    [DllImport("user32.dll", SetLastError=true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseDesktop(IntPtr hDesktop);
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
}
"@
    $DESKTOP_SWITCHDESKTOP = 0x0100
    $desktopHandle = [S5dNative]::OpenInputDesktop(0, $false, $DESKTOP_SWITCHDESKTOP)
    if ($desktopHandle -ne [IntPtr]::Zero) {
        $desktopCheck.desktop_unlocked = [S5dNative]::SwitchDesktop($desktopHandle)
        [void][S5dNative]::CloseDesktop($desktopHandle)
    } else {
        $desktopCheck.errors += "OpenInputDesktop returned zero."
    }
    $foregroundHandle = [S5dNative]::GetForegroundWindow()
    $desktopCheck.foreground_window_handle_present = ($foregroundHandle -ne [IntPtr]::Zero)
    if ($desktopCheck.foreground_window_handle_present) {
        [uint32]$foregroundPid = 0
        [void][S5dNative]::GetWindowThreadProcessId($foregroundHandle, [ref]$foregroundPid)
        $desktopCheck.foreground_process_id = [int]$foregroundPid
        if ($foregroundPid -gt 0) {
            $foregroundProcess = Get-Process -Id $foregroundPid -ErrorAction SilentlyContinue
            if ($foregroundProcess) { $desktopCheck.foreground_process_name = $foregroundProcess.ProcessName }
        }
    }
    $lockNames = @("LockApp", "LogonUI", "CredentialUIBroker")
    $desktopCheck.lock_or_secret_surface_detected = (
        -not $desktopCheck.desktop_unlocked -or
        ($desktopCheck.foreground_process_name -in $lockNames)
    )
} catch {
    $desktopCheck.errors += $_.Exception.Message
}
$desktopCheck.desktop_check_pass = (
    $desktopCheck.desktop_unlocked -and
    $desktopCheck.foreground_window_handle_present -and
    -not $desktopCheck.lock_or_secret_surface_detected -and
    @($desktopCheck.errors).Count -eq 0
)
Save-Json -Path $DesktopJsonPath -Value $desktopCheck
Add-Log "Desktop foreground and lock preflight saved."

$verdict = "PASS_PREFLIGHT_READY_FOR_LAUNCH"
$reasons = @()
if (-not $sourceReadback.source_check_pass) {
    $verdict = "NEEDS_FIX"
    $reasons += "S5c source readback or policy agreement failed."
} elseif (-not $pathStorage.storage_check_pass) {
    $verdict = "BLOCKED_PATH_OR_STORAGE"
    $reasons += "Cursor path, extensionDevelopmentPath, or isolated storage/temp preflight failed."
} elseif (-not $processDiscovery.stale_policy_check_pass) {
    $verdict = "BLOCKED_STALE_PROCESS_POLICY_MISSING"
    $reasons += "Stale Cursor process policy/read-only discovery preflight did not pass."
} elseif (-not $desktopCheck.desktop_check_pass) {
    $verdict = "BLOCKED_LOCK_OR_FOREGROUND"
    $reasons += "Desktop unlocked/foreground preflight did not pass."
} else {
    $reasons += "All S5d preflight-only checks passed; launch was not performed in S5d."
}

$decision = [ordered]@{
    verdict = $verdict
    reasons = $reasons
    cursor_launch_performed = $false
    ui_automation_performed = $false
    prompt_typing_performed = $false
    prompt_send_performed = $false
    process_stop_kill_close_performed = $false
    launch_authorized_by_s5c_after_preflight_pass = $sourceReadback.s5c_launch_after_pass_authorized
    s5d_scope_decision = "preflight_only_stop_before_launch"
    exact_next_task = "Reviewer: inspect this S5d packet, parse JSON, verify whitespace, confirm no Cursor launch/process stop/UI automation happened, and if accepted dispatch the separate S5c launch executor using the accepted script-file runner rule and S5c launch gate."
}
Save-Json -Path $DecisionJsonPath -Value $decision

$packet = [ordered]@{
    packet_type = "row10_s5d_preflight_script_runner_packet"
    verdict = $verdict
    created_by = "Row10-S5d bounded sub-agent current session"
    created_local = (Get-Date -Format "o")
    run_root = $RunRoot
    runner_script = $MyInvocation.MyCommand.Path
    honesty = [ordered]@{
        claim_scope = "Row10-S5d preflight-only script-file runner execution. Created one scoped evidence folder, wrote and executed the runner file, validated source policy, paths, isolated storage/temp probes, stale Cursor process discovery, and desktop foreground/lock state. Did not launch Cursor."
        evidence_scope = "Evidence is limited to this runner's filesystem checks, JSON parsing, read-only WMI process discovery, and user32 desktop/foreground probes. No screenshot, UI automation, prompt typing/send, model-backed response, product readiness, token, quality, training, checkpoint, locator, commit, push, or code edit evidence was produced."
        count_claimed = "1 runner script file created, 1 runner execution, 5 JSON evidence files written, 1 log written, 1 Russian Markdown report written."
        independent_evidence_count = "Runner outputs plus post-run JSON/whitespace verification to be performed by caller."
        subagent_id_or_NONE = "Row10-S5d bounded sub-agent / current session"
        downgrade_needed = "Yes for anything beyond preflight readiness. This does not prove Cursor launch, UI route, product readiness, storage recovery during runtime, or no-send behavior."
    }
    sources = $sourceReadback
    path_and_storage = $pathStorage
    stale_process_discovery = $processDiscovery
    desktop_foreground_lock = $desktopCheck
    decision = $decision
    files_written = @(
        $SourceJsonPath,
        $PathStorageJsonPath,
        $ProcessJsonPath,
        $DesktopJsonPath,
        $DecisionJsonPath,
        $LogPath,
        $ReportMdPath,
        $PacketJsonPath
    )
}
Save-Json -Path $PacketJsonPath -Value $packet

$reasonLines = ($reasons | ForEach-Object { "- $_" }) -join [Environment]::NewLine
$mdLines = @(
    "# Row10-S5d preflight-only script-file runner packet",
    "",
    "## Honesty block",
    "",
    "claim_scope: Row10-S5d preflight-only script-file runner execution. Created one scoped evidence folder, wrote and executed the runner file, checked source policy, paths, isolated storage/temp probes, stale Cursor process discovery, and desktop foreground/lock state. Cursor was not launched.",
    "evidence_scope: Only filesystem checks, JSON parsing, read-only WMI process discovery, and user32 desktop/foreground probes. No screenshot, UI automation, prompt typing/send, model-backed response, product readiness, token, quality, training, checkpoint, locator, commit/push, or code edits were performed.",
    "count_claimed: 1 runner script file created, 1 runner execution, 5 JSON evidence files, 1 log, 1 Markdown report, 1 JSON packet.",
    "independent_evidence_count: runner outputs plus post-run JSON/whitespace verification by caller.",
    "subagent_id_or_NONE: Row10-S5d bounded sub-agent / current session",
    "downgrade_needed: yes; this is preflight readiness, not Cursor launch/UI/product readiness.",
    "",
    "## Verdict",
    "",
    $verdict,
    "",
    "Reasons:",
    $reasonLines,
    "",
    "## Results",
    "",
    "- S5c source/readback PASS: $($sourceReadback.source_check_pass)",
    "- S5c launch after preflight PASS authorized: $($sourceReadback.s5c_launch_after_pass_authorized)",
    "- Stale policy read-only / stop forbidden: $($sourceReadback.stale_policy_read_only_stop_forbidden)",
    "- Cursor executable exists: $($pathStorage.cursor_exe.exists)",
    "- extensionDevelopmentPath exists: $($pathStorage.extension_development_path.exists)",
    "- Isolated storage/temp PASS: $($pathStorage.storage_check_pass)",
    "- Cursor processes discovered: $($processDiscovery.cursor_process_count)",
    "- Stale process policy PASS: $($processDiscovery.stale_policy_check_pass)",
    "- Desktop unlocked: $($desktopCheck.desktop_unlocked)",
    "- Foreground window handle present: $($desktopCheck.foreground_window_handle_present)",
    "- Lock/secret surface detected by non-screenshot probes: $($desktopCheck.lock_or_secret_surface_detected)",
    "- Desktop preflight PASS: $($desktopCheck.desktop_check_pass)",
    "- Cursor launch performed: false",
    "- Prompt typing/send performed: false",
    "- Process stop/kill/close performed: false",
    "",
    "## Files",
    "",
    "- $SourceJsonPath",
    "- $PathStorageJsonPath",
    "- $ProcessJsonPath",
    "- $DesktopJsonPath",
    "- $DecisionJsonPath",
    "- $PacketJsonPath",
    "- $LogPath",
    "",
    "## Exact next review task",
    "",
    "Reviewer: inspect this S5d packet, parse JSON, verify whitespace, confirm no Cursor launch/process stop/UI automation happened, and if accepted dispatch the separate S5c launch executor using the accepted script-file runner rule and S5c launch gate."
)
Write-Utf8File -Path $ReportMdPath -Text ($mdLines -join [Environment]::NewLine)
Add-Log "Markdown report and JSON packet saved."
Add-Log "Row10-S5d preflight-only runner finished with verdict $verdict."

Write-Output $verdict
