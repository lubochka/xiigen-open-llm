param(
    [string]$CursorExePath = "C:\Users\lubao\AppData\Local\Programs\Cursor\Cursor.exe",
    [string]$ExtensionDevelopmentPath = "C:\Projects\cursor-xiigen-smart-skills",
    [string]$UserDataDir = "",
    [string]$ExtensionsDir = "",
    [string]$RunId = "",
    [switch]$EnableFirewallProbe,
    [switch]$LaunchCursor,
    [switch]$Cleanup
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-IsoNow {
    return (Get-Date).ToString("yyyy-MM-ddTHH:mm:sszzz")
}

function Test-Admin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Quote-Arg {
    param([string]$Value)
    if ($Value -match '[\s"]') {
        return '"' + ($Value -replace '"', '\"') + '"'
    }
    return $Value
}

function Get-ProcessTreeIds {
    param([int]$RootPid)
    $all = @(Get-CimInstance Win32_Process)
    $ids = @($RootPid)
    do {
        $before = $ids.Count
        $children = @($all | Where-Object { $ids -contains [int]$_.ParentProcessId } | ForEach-Object { [int]$_.ProcessId })
        $ids = @($ids + $children | Sort-Object -Unique)
    } while ($ids.Count -gt $before)
    return $ids
}

if ([string]::IsNullOrWhiteSpace($RunId)) {
    $RunId = "cursor-safe-launch-isolation-preflight-" + (Get-Date).ToString("yyyyMMdd-HHmmss")
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$defaultRunRoot = Join-Path $repoRoot ("artifacts/extension-runtime-token-quality-learnability/cursor-safe-launch-network-process-isolation-repair/" + $RunId)
if ([string]::IsNullOrWhiteSpace($UserDataDir)) {
    $UserDataDir = Join-Path $defaultRunRoot "isolated-user-data"
}
if ([string]::IsNullOrWhiteSpace($ExtensionsDir)) {
    $ExtensionsDir = Join-Path $defaultRunRoot "isolated-extensions"
}

$cursorExists = Test-Path -LiteralPath $CursorExePath -PathType Leaf
$extensionExists = Test-Path -LiteralPath $ExtensionDevelopmentPath -PathType Container
$packageJsonPath = Join-Path $ExtensionDevelopmentPath "package.json"
$packageJsonExists = Test-Path -LiteralPath $packageJsonPath -PathType Leaf
$isAdmin = Test-Admin

$createdDirs = @()
foreach ($dir in @($UserDataDir, $ExtensionsDir)) {
    if (-not (Test-Path -LiteralPath $dir -PathType Container)) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
        $createdDirs += $dir
    }
}

$cursorArgs = @(
    "--extensionDevelopmentPath", $ExtensionDevelopmentPath,
    "--user-data-dir", $UserDataDir,
    "--extensions-dir", $ExtensionsDir,
    "--disable-workspace-trust",
    "--skip-welcome",
    "--skip-release-notes",
    "--skip-onboarding",
    "--disable-telemetry",
    "--disable-updates",
    "--log", "trace",
    "--new-window", $ExtensionDevelopmentPath
)
$commandLine = (Quote-Arg $CursorExePath) + " " + (($cursorArgs | ForEach-Object { Quote-Arg $_ }) -join " ")

$firewallRuleName = "XIIGen-Cursor-Isolation-$RunId"
$firewallAvailable = [bool](Get-Command New-NetFirewallRule -ErrorAction SilentlyContinue)
$networkMonitorAvailable = [bool](Get-Command Get-NetTCPConnection -ErrorAction SilentlyContinue)
$processMonitorAvailable = [bool](Get-Command Get-CimInstance -ErrorAction SilentlyContinue)

$firewallProbe = [ordered]@{
    enabled = [bool]$EnableFirewallProbe
    status = "NOT_REQUESTED_DRY_RUN"
    admin_required = $false
    rule_name = $firewallRuleName
    program = $CursorExePath
    created = $false
    verified_present = $false
    removed = $false
    verified_removed = $false
    error = $null
}

if ($EnableFirewallProbe) {
    if (-not $isAdmin) {
        $firewallProbe.status = "DEFER_ADMIN_REQUIRED_NO_FIREWALL_CHANGE"
        $firewallProbe.admin_required = $true
    } elseif (-not $firewallAvailable) {
        $firewallProbe.status = "FAIL_FIREWALL_CMDLET_MISSING"
    } else {
        try {
            New-NetFirewallRule -DisplayName $firewallRuleName -Direction Outbound -Action Block -Program $CursorExePath -Profile Any -Enabled True | Out-Null
            $firewallProbe.created = $true
            $present = Get-NetFirewallRule -DisplayName $firewallRuleName -ErrorAction Stop
            $firewallProbe.verified_present = [bool]$present
            Remove-NetFirewallRule -DisplayName $firewallRuleName -ErrorAction Stop
            $firewallProbe.removed = $true
            $remaining = Get-NetFirewallRule -DisplayName $firewallRuleName -ErrorAction SilentlyContinue
            $firewallProbe.verified_removed = -not [bool]$remaining
            if ($firewallProbe.verified_present -and $firewallProbe.verified_removed) {
                $firewallProbe.status = "PASS_TEMP_RULE_CREATED_VERIFIED_REMOVED"
            } else {
                $firewallProbe.status = "FAIL_TEMP_RULE_VERIFICATION_INCOMPLETE"
            }
        } catch {
            $firewallProbe.status = "FAIL_TEMP_RULE_PROBE_ERROR"
            $firewallProbe.error = $_.Exception.Message
            if ($firewallProbe.created) {
                Remove-NetFirewallRule -DisplayName $firewallRuleName -ErrorAction SilentlyContinue
            }
        }
    }
}

$launchResult = [ordered]@{
    requested = [bool]$LaunchCursor
    status = "NOT_REQUESTED_DRY_RUN"
    launched = $false
    root_pid = $null
    process_tree_pids = @()
    established_connections_snapshot = @()
    live_firewall_rule_name = "$firewallRuleName-live"
    cleanup_performed = $false
    error = $null
}

if ($LaunchCursor) {
    if (-not $cursorExists -or -not $extensionExists -or -not $packageJsonExists) {
        $launchResult.status = "REFUSED_PATH_VALIDATION_FAILED"
    } elseif (-not $isAdmin) {
        $launchResult.status = "REFUSED_ADMIN_REQUIRED"
    } elseif (-not $EnableFirewallProbe -or $firewallProbe.status -ne "PASS_TEMP_RULE_CREATED_VERIFIED_REMOVED") {
        $launchResult.status = "REFUSED_FIREWALL_PROBE_REQUIRED"
    } elseif (-not $networkMonitorAvailable -or -not $processMonitorAvailable) {
        $launchResult.status = "REFUSED_MONITOR_COMMANDS_MISSING"
    } elseif (-not $Cleanup) {
        $launchResult.status = "REFUSED_CLEANUP_REQUIRED_FOR_NONPROMPT_LAUNCH"
    } else {
        try {
            New-NetFirewallRule -DisplayName $launchResult.live_firewall_rule_name -Direction Outbound -Action Block -Program $CursorExePath -Profile Any -Enabled True | Out-Null
            $process = Start-Process -FilePath $CursorExePath -ArgumentList $cursorArgs -PassThru
            $launchResult.launched = $true
            $launchResult.root_pid = $process.Id
            Start-Sleep -Seconds 5
            $pids = @(Get-ProcessTreeIds -RootPid $process.Id)
            $launchResult.process_tree_pids = $pids
            $connections = @(Get-NetTCPConnection -ErrorAction SilentlyContinue | Where-Object { $pids -contains [int]$_.OwningProcess } | Select-Object State,LocalAddress,LocalPort,RemoteAddress,RemotePort,OwningProcess)
            $launchResult.established_connections_snapshot = $connections
            foreach ($pid in ($pids | Sort-Object -Descending)) {
                Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
            }
            $launchResult.cleanup_performed = $true
            $launchResult.status = "PASS_LAUNCHED_WITH_TEMP_DENY_CAPTURED_AND_CLEANED_NO_PROMPT_SENT"
        } catch {
            $launchResult.status = "FAIL_LAUNCH_BRANCH_ERROR"
            $launchResult.error = $_.Exception.Message
        } finally {
            Remove-NetFirewallRule -DisplayName $launchResult.live_firewall_rule_name -ErrorAction SilentlyContinue
        }
    }
}

if ($Cleanup -and -not $LaunchCursor) {
    foreach ($dir in @($UserDataDir, $ExtensionsDir)) {
        if ((Test-Path -LiteralPath $dir -PathType Container) -and $dir.StartsWith($defaultRunRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
            Remove-Item -LiteralPath $dir -Recurse -Force
        }
    }
}

$pathValidationPass = $cursorExists -and $extensionExists -and $packageJsonExists
$monitorCommandsPass = $networkMonitorAvailable -and $processMonitorAvailable
$contractReady = $pathValidationPass -and $monitorCommandsPass -and $firewallAvailable
$dryRunPass = $contractReady -and -not $LaunchCursor

$result = [ordered]@{
    schema = "xiigen.cursor_safe_launch_isolation_preflight.helper_result.v1"
    generated_at_local = Get-IsoNow
    run_id = $RunId
    dry_run_default = (-not $LaunchCursor)
    cursor_exe_path = $CursorExePath
    cursor_exists = $cursorExists
    cursor_product_version = if ($cursorExists) { (Get-Item -LiteralPath $CursorExePath).VersionInfo.ProductVersion } else { $null }
    extension_development_path = $ExtensionDevelopmentPath
    extension_path_exists = $extensionExists
    extension_package_json_exists = $packageJsonExists
    user_data_dir = $UserDataDir
    extensions_dir = $ExtensionsDir
    isolated_dirs_created_or_validated = @($UserDataDir, $ExtensionsDir)
    isolated_dirs_created_this_run = $createdDirs
    command_line = $commandLine
    command_args = $cursorArgs
    firewall_probe = $firewallProbe
    monitor_prerequisites = [ordered]@{
        get_net_tcp_connection_available = $networkMonitorAvailable
        get_cim_instance_available = $processMonitorAvailable
        process_tree_capture_command = '$rootPid = <CURSOR_ROOT_PID>; $all = @(Get-CimInstance Win32_Process); $ids = @($rootPid); do { $before = $ids.Count; $children = @($all | Where-Object { $ids -contains [int]$_.ParentProcessId } | ForEach-Object { [int]$_.ProcessId }); $ids = @($ids + $children | Sort-Object -Unique) } while ($ids.Count -gt $before); $all | Where-Object { $ids -contains [int]$_.ProcessId } | Select-Object ProcessId,ParentProcessId,Name,ExecutablePath,CommandLine | ConvertTo-Json -Depth 5'
        network_snapshot_command = '$processIds = @(<PROCESS_TREE_PIDS>); Get-NetTCPConnection -ErrorAction SilentlyContinue | Where-Object { $processIds -contains [int]$_.OwningProcess } | Select-Object CreationTime,State,LocalAddress,LocalPort,RemoteAddress,RemotePort,OwningProcess | ConvertTo-Json -Depth 5'
    }
    launch = $launchResult
    contract_ready_for_next_live_work_order = $contractReady
    launch_allowed_now = $false
    passed = $dryRunPass
    no_external_ai_or_model_calls = $true
    no_cursor_prompt_or_chat_sent = $true
    git_add_commit_push_used = $false
}

$result | ConvertTo-Json -Depth 10
