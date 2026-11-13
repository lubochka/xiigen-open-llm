Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$runFolder = 'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z'
$cursorExe = 'C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe'
$extensionDevelopmentPath = 'C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills'
$logsDir = Join-Path $runFolder 'logs'
$shortHostRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('xiigen-row12-cursor-devhost-' + (Get-Date -Format 'yyyyMMddHHmmss'))
$userDataDir = Join-Path $shortHostRoot 'user-data'
$extensionsDir = Join-Path $shortHostRoot 'extensions'

New-Item -ItemType Directory -Force -Path $userDataDir, $extensionsDir, $logsDir | Out-Null

function Get-ExtensionHostSignals {
  param(
    [Parameter(Mandatory = $true)]
    [string]$UserDataDir
  )

  $logRoot = Join-Path $UserDataDir 'logs'
  if (-not (Test-Path -LiteralPath $logRoot)) {
    return @()
  }

  $patterns = @(
    'Extension host with pid',
    'Extension host \(LocalProcess \[role: user\] pid: .* is responsive',
    'Extension host did not start in 10 seconds',
    'Extension host \(LocalProcess \[role: user\] pid: .* is unresponsive'
  )

  $files = Get-ChildItem -Recurse -File -LiteralPath $logRoot -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -in @('renderer.log', 'exthost.log') }
  $signals = foreach ($file in $files) {
    Select-String -LiteralPath $file.FullName -Pattern $patterns -ErrorAction SilentlyContinue | ForEach-Object {
      [pscustomobject]@{
        path = $_.Path
        line = $_.LineNumber
        text = $_.Line
      }
    }
  }

  return @($signals)
}

function Wait-ForExtensionHostResponsive {
  param(
    [Parameter(Mandatory = $true)]
    [string]$UserDataDir,
    [int]$TimeoutSeconds = 45
  )

  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  $signals = @()
  do {
    $signals = Get-ExtensionHostSignals -UserDataDir $UserDataDir
    $responsive = @($signals | Where-Object { $_.text -match 'is responsive' }).Count -gt 0
    if ($responsive) {
      return [pscustomobject]@{
        timeout_seconds = $TimeoutSeconds
        responsive_seen = $true
        waited_until_utc = (Get-Date).ToUniversalTime().ToString('o')
        signals = $signals
      }
    }
    Start-Sleep -Seconds 1
  } while ((Get-Date) -lt $deadline)

  return [pscustomobject]@{
    timeout_seconds = $TimeoutSeconds
    responsive_seen = $false
    waited_until_utc = (Get-Date).ToUniversalTime().ToString('o')
    signals = $signals
  }
}

if (-not (Test-Path -LiteralPath $cursorExe)) {
  throw "Cursor executable not found: $cursorExe"
}

if (-not (Test-Path -LiteralPath $extensionDevelopmentPath)) {
  throw "Extension development path not found: $extensionDevelopmentPath"
}

$arguments = @(
  '--new-window',
  '--user-data-dir', $userDataDir,
  '--extensions-dir', $extensionsDir,
  "--extensionDevelopmentPath=$extensionDevelopmentPath",
  '--disable-workspace-trust',
  '--skip-welcome',
  '--skip-release-notes',
  '--skip-onboarding',
  '--disable-telemetry',
  '--disable-updates',
  '--disable-gpu',
  $extensionDevelopmentPath
)

$before = Get-CimInstance Win32_Process -Filter "Name = 'Cursor.exe'" |
  Select-Object ProcessId, CommandLine

$launch_method = 'powershell_native_invocation_argument_array'
Push-Location -LiteralPath $extensionDevelopmentPath
try {
  & $cursorExe @arguments
} finally {
  Pop-Location
}
$extensionHostWait = Wait-ForExtensionHostResponsive -UserDataDir $userDataDir -TimeoutSeconds 45

$after = Get-CimInstance Win32_Process -Filter "Name = 'Cursor.exe'" |
  Select-Object ProcessId, CommandLine

$expectedSequence = @("--extensionDevelopmentPath=$extensionDevelopmentPath")
$matchingProcesses = @(
  $after | Where-Object {
    $_.CommandLine -and
    $_.CommandLine.Contains('--extensionDevelopmentPath') -and
    $_.CommandLine.Contains($extensionDevelopmentPath) -and
    $_.CommandLine.Contains($userDataDir)
  }
)

$record = [pscustomobject]@{
  packet_type = 'ROW12_CORRECTED_CURSOR_LAUNCH_RECORD'
  launched_at_utc = (Get-Date).ToUniversalTime().ToString('o')
  cursor_exe = $cursorExe
  run_folder = $runFolder
  short_host_root = $shortHostRoot
  user_data_dir = $userDataDir
  extensions_dir = $extensionsDir
  extension_development_path = $extensionDevelopmentPath
  argument_array = $arguments
  expected_separate_argument_sequence = $expectedSequence
  launch_method = $launch_method
  launch_diagnostic_fix = 'Use a short fresh temp user-data/extensions root to avoid Cursor SQLITE_CANTOPEN and extension-host log path failures from the deep evidence path; pass extensionDevelopmentPath with equals-form so the value is unambiguous.'
  process_start_pid = $null
  cursor_processes_before_count = @($before).Count
  cursor_processes_after_count = @($after).Count
  matching_processes_with_full_path = $matchingProcesses
  command_line_full_path_found = @($matchingProcesses).Count -gt 0
  extension_host_wait = $extensionHostWait
  note = 'Arguments were passed through PowerShell native invocation with an argument array; --extensionDevelopmentPath uses equals-form with the full path in one array entry. Cursor volatile state is intentionally outside the deep evidence folder.'
}

$recordPath = Join-Path $logsDir 'corrected-launch-record.json'
$record | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $recordPath -Encoding UTF8
$record | ConvertTo-Json -Depth 8
