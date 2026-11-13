$ErrorActionPreference = "Stop"

$RunRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$CursorExe = "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe"
$ExtensionRoot = "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills"
$UserDataDir = Join-Path $RunRoot "user-data"
$ExtensionsDir = Join-Path $RunRoot "extensions"
$JsonPath = Join-Path $RunRoot "host-launch-recovery.json"
$MdPath = Join-Path $RunRoot "host-launch-recovery.md"

Add-Type @"
using System;
using System.Text;
using System.Runtime.InteropServices;
public class Row12HostRecoveryWindowTools {
  public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
  [DllImport("user32.dll")] public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
  [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hWnd);
  [DllImport("user32.dll", CharSet=CharSet.Auto)] public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
  [DllImport("user32.dll", CharSet=CharSet.Auto)] public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  public static uint GetProcessId(IntPtr hWnd) {
    uint processId;
    GetWindowThreadProcessId(hWnd, out processId);
    return processId;
  }
}
"@

function Get-VisibleWindowSnapshots {
  return @(Get-Process -Name "Cursor" -ErrorAction SilentlyContinue |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_.MainWindowTitle) } |
    Select-Object @{Name = "hwnd"; Expression = { $_.MainWindowHandle.ToInt64() }},
      @{Name = "process_id"; Expression = { $_.Id }},
      @{Name = "class_name"; Expression = { "process-main-window" }},
      @{Name = "title"; Expression = { $_.MainWindowTitle }})
}

function Get-CursorProcessSnapshots {
  return @(Get-Process -Name "Cursor" -ErrorAction SilentlyContinue |
    Select-Object @{Name = "ProcessId"; Expression = { $_.Id }},
      @{Name = "Name"; Expression = { "Cursor.exe" }},
      @{Name = "MainWindowTitle"; Expression = { $_.MainWindowTitle }},
      @{Name = "MainWindowHandle"; Expression = { $_.MainWindowHandle.ToInt64() }},
      @{Name = "CommandLine"; Expression = { $null }})
}

function Find-HostWindows {
  param(
    [object[]]$Windows,
    [object[]]$Processes
  )
  $processIds = @($Processes | ForEach-Object { [int]$_.ProcessId })
  return @($Windows | Where-Object {
    ($_.title -match "Extension Development Host|cursor-xiigen-smart-skills|XIIGen Smart Skills") -or
    (($processIds -contains [int]$_.process_id) -and ($_.title -match "Cursor|Extension Development Host"))
  })
}

function Write-RecoveryPacket {
  param(
    [string]$Status,
    [string]$HumanExplanation,
    [string]$NextSmallestAction,
    [object[]]$BeforeWindows,
    [object[]]$BeforeProcesses,
    [object[]]$AfterWindows,
    [object[]]$AfterProcesses,
    [object[]]$HostWindows,
    [object]$LaunchProcess,
    [string]$LaunchError
  )

  $windowTitle = (($HostWindows | Where-Object { -not [string]::IsNullOrWhiteSpace($_.title) } | Select-Object -First 1).title)
  $packet = [pscustomobject]@{
    schema_version = "row12-cursor-host-recovery/v1"
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    status = $Status
    run_folder = $RunRoot
    cursor_executable = $CursorExe
    extension_root = $ExtensionRoot
    user_data_dir = $UserDataDir
    extensions_dir = $ExtensionsDir
    window_title = $windowTitle
    matching_host_windows = $HostWindows
    launch_process = if ($LaunchProcess) {
      [pscustomobject]@{
        id = $LaunchProcess.Id
        has_exited = $LaunchProcess.HasExited
      }
    } else {
      $null
    }
    launch_error = $LaunchError
    process_summary = [pscustomobject]@{
      before_cursor_process_count = $BeforeProcesses.Count
      after_cursor_process_count = $AfterProcesses.Count
      before_cursor_processes = $BeforeProcesses
      after_cursor_processes = $AfterProcesses
      before_matching_window_count = (Find-HostWindows -Windows $BeforeWindows -Processes $BeforeProcesses).Count
      after_matching_window_count = $HostWindows.Count
    }
    before_matching_windows = Find-HostWindows -Windows $BeforeWindows -Processes $BeforeProcesses
    after_matching_windows = $HostWindows
    launch_contract = [pscustomobject]@{
      used_real_local_program = $true
      did_not_run_row12_scenario = $true
      did_not_login = $true
      did_not_inspect_account_surfaces = $true
      did_not_close_user_windows = $true
      external_ai_send = $false
      args = @(
        "--new-window",
        "--user-data-dir", $UserDataDir,
        "--extensions-dir", $ExtensionsDir,
        "--extensionDevelopmentPath", $ExtensionRoot,
        "--disable-workspace-trust",
        "--skip-welcome",
        "--skip-release-notes",
        "--skip-onboarding",
        "--disable-telemetry",
        "--disable-updates",
        "--disable-gpu",
        $ExtensionRoot
      )
    }
    human_explanation = $HumanExplanation
    next_smallest_action = $NextSmallestAction
  }

  $packet | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $JsonPath -Encoding UTF8

  $md = @"
# ROW12_CURSOR_HOST_RECOVERY_PACKET

status: $Status
run_folder: $RunRoot
files_created:
- $JsonPath
- $MdPath
- $($MyInvocation.MyCommand.Path)
window_title: $windowTitle
process_summary: before_cursor_process_count=$($BeforeProcesses.Count); after_cursor_process_count=$($AfterProcesses.Count); after_matching_window_count=$($HostWindows.Count)
human_explanation: $HumanExplanation
next_smallest_action: $NextSmallestAction
"@
  $md | Set-Content -LiteralPath $MdPath -Encoding UTF8
  Write-Output $Status
  Write-Output $JsonPath
}

$beforeProcesses = Get-CursorProcessSnapshots
$beforeWindows = Get-VisibleWindowSnapshots
$existingHostWindows = Find-HostWindows -Windows $beforeWindows -Processes $beforeProcesses

if ($existingHostWindows.Count -gt 0) {
  Write-RecoveryPacket `
    -Status "HOST_ALREADY_READY" `
    -HumanExplanation "A visible Cursor Extension Development Host-compatible window already exists; no account surfaces were opened." `
    -NextSmallestAction "Proceed to the separately authorized account-discovery slice without running row12." `
    -BeforeWindows $beforeWindows `
    -BeforeProcesses $beforeProcesses `
    -AfterWindows $beforeWindows `
    -AfterProcesses $beforeProcesses `
    -HostWindows $existingHostWindows `
    -LaunchProcess $null `
    -LaunchError $null
  exit 0
}

if (-not (Test-Path -LiteralPath $CursorExe)) {
  Write-RecoveryPacket `
    -Status "HOST_LAUNCH_BLOCKED_WITH_EVIDENCE" `
    -HumanExplanation "Cursor.exe was not found at the row12 launcher contract path." `
    -NextSmallestAction "Repair the launcher executable path or install Cursor, then retry host recovery." `
    -BeforeWindows $beforeWindows `
    -BeforeProcesses $beforeProcesses `
    -AfterWindows $beforeWindows `
    -AfterProcesses $beforeProcesses `
    -HostWindows @() `
    -LaunchProcess $null `
    -LaunchError "Cursor executable not found: $CursorExe"
  exit 2
}

if (-not (Test-Path -LiteralPath $ExtensionRoot)) {
  Write-RecoveryPacket `
    -Status "HOST_LAUNCH_BLOCKED_WITH_EVIDENCE" `
    -HumanExplanation "The cursor-xiigen-smart-skills extension root was not found at the row12 launcher contract path." `
    -NextSmallestAction "Repair the extensionDevelopmentPath, then retry host recovery." `
    -BeforeWindows $beforeWindows `
    -BeforeProcesses $beforeProcesses `
    -AfterWindows $beforeWindows `
    -AfterProcesses $beforeProcesses `
    -HostWindows @() `
    -LaunchProcess $null `
    -LaunchError "Extension root not found: $ExtensionRoot"
  exit 2
}

New-Item -ItemType Directory -Force -Path $UserDataDir | Out-Null
New-Item -ItemType Directory -Force -Path $ExtensionsDir | Out-Null

$cursorArgs = @(
  "--new-window",
  "--user-data-dir", $UserDataDir,
  "--extensions-dir", $ExtensionsDir,
  "--extensionDevelopmentPath", $ExtensionRoot,
  "--disable-workspace-trust",
  "--skip-welcome",
  "--skip-release-notes",
  "--skip-onboarding",
  "--disable-telemetry",
  "--disable-updates",
  "--disable-gpu",
  $ExtensionRoot
)

$launchProcess = $null
$launchError = $null
try {
  $launchProcess = Start-Process -FilePath $CursorExe -ArgumentList $cursorArgs -WorkingDirectory $ExtensionRoot -WindowStyle Normal -PassThru
} catch {
  $launchError = $_.Exception.Message
}

$afterProcesses = @()
$afterWindows = @()
$hostWindows = @()
$deadline = (Get-Date).AddSeconds(75)
do {
  Start-Sleep -Seconds 2
  $afterProcesses = Get-CursorProcessSnapshots
  $afterWindows = Get-VisibleWindowSnapshots
  $hostWindows = Find-HostWindows -Windows $afterWindows -Processes $afterProcesses
  if ($hostWindows.Count -gt 0) {
    foreach ($hostWindow in $hostWindows) {
      if ($hostWindow.hwnd) {
        [void][Row12HostRecoveryWindowTools]::ShowWindow([IntPtr]$hostWindow.hwnd, 9)
        [void][Row12HostRecoveryWindowTools]::SetForegroundWindow([IntPtr]$hostWindow.hwnd)
      }
    }
    break
  }
} while ((Get-Date) -lt $deadline)

if ($hostWindows.Count -gt 0) {
  Write-RecoveryPacket `
    -Status "HOST_WINDOW_READY_FOR_ACCOUNT_DISCOVERY" `
    -HumanExplanation "Cursor was launched as a real local Extension Development Host for cursor-xiigen-smart-skills; no account surfaces were inspected and row12 was not run." `
    -NextSmallestAction "Start the separately authorized account-surface discovery slice from this host window." `
    -BeforeWindows $beforeWindows `
    -BeforeProcesses $beforeProcesses `
    -AfterWindows $afterWindows `
    -AfterProcesses $afterProcesses `
    -HostWindows $hostWindows `
    -LaunchProcess $launchProcess `
    -LaunchError $launchError
  exit 0
}

Write-RecoveryPacket `
  -Status "HOST_LAUNCH_BLOCKED_WITH_EVIDENCE" `
  -HumanExplanation "Cursor launch was attempted, but no visible Extension Development Host-compatible window was found before timeout." `
  -NextSmallestAction "Inspect host-launch-recovery.json and Cursor process state, then repair the launcher contract or window/session state." `
  -BeforeWindows $beforeWindows `
  -BeforeProcesses $beforeProcesses `
  -AfterWindows $afterWindows `
  -AfterProcesses $afterProcesses `
  -HostWindows @() `
  -LaunchProcess $launchProcess `
  -LaunchError $launchError
exit 2
