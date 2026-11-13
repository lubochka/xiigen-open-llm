param(
  [string]$EvidenceDir = "C:\Projects\llm_mvp_core_codex_sync_20260628\docs\plans\evidence\extension-full-readiness-dual-path\cursor-path-b-ui-submit-to-core-20260628T185708Z"
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$nativeSource = @"
using System;
using System.Text;
using System.Runtime.InteropServices;

public static class NativeWindowToolsSubmit11 {
  public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

  [DllImport("user32.dll")]
  public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

  [DllImport("user32.dll")]
  public static extern bool IsWindowVisible(IntPtr hWnd);

  [DllImport("user32.dll", CharSet = CharSet.Unicode)]
  public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

  [DllImport("user32.dll", CharSet = CharSet.Unicode)]
  public static extern int GetWindowTextLength(IntPtr hWnd);

  [DllImport("user32.dll")]
  public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

  [DllImport("user32.dll")]
  public static extern bool SetForegroundWindow(IntPtr hWnd);

  [DllImport("user32.dll")]
  public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

  [DllImport("user32.dll")]
  public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

  [StructLayout(LayoutKind.Sequential)]
  public struct RECT {
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
  }
}
"@

if (-not ("NativeWindowToolsSubmit11" -as [type])) {
  Add-Type -TypeDefinition $nativeSource
}

function Write-JsonFile {
  param(
    [Parameter(Mandatory=$true)]$Value,
    [Parameter(Mandatory=$true)][string]$Path
  )
  $Value | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Get-VisibleWindows {
  $items = New-Object System.Collections.ArrayList
  $callback = [NativeWindowToolsSubmit11+EnumWindowsProc]{
    param([IntPtr]$hWnd, [IntPtr]$lParam)

    if (-not [NativeWindowToolsSubmit11]::IsWindowVisible($hWnd)) {
      return $true
    }

    $length = [NativeWindowToolsSubmit11]::GetWindowTextLength($hWnd)
    if ($length -le 0) {
      return $true
    }

    $builder = New-Object System.Text.StringBuilder ($length + 1)
    [void][NativeWindowToolsSubmit11]::GetWindowText($hWnd, $builder, $builder.Capacity)
    $title = $builder.ToString()
    if ([string]::IsNullOrWhiteSpace($title)) {
      return $true
    }

    $procId = [uint32]0
    [void][NativeWindowToolsSubmit11]::GetWindowThreadProcessId($hWnd, [ref]$procId)
    $processPath = $null
    $processName = $null
    $commandLine = $null
    try {
      $proc = Get-Process -Id ([int]$procId) -ErrorAction Stop
      $processName = $proc.ProcessName
      $processPath = $proc.Path
      $cim = Get-CimInstance Win32_Process -Filter "ProcessId=$([int]$procId)" -ErrorAction SilentlyContinue
      if ($cim) {
        $commandLine = $cim.CommandLine
      }
    } catch {
      $processName = $null
      $processPath = $null
      $commandLine = $null
    }

    $rect = New-Object NativeWindowToolsSubmit11+RECT
    [void][NativeWindowToolsSubmit11]::GetWindowRect($hWnd, [ref]$rect)
    [void]$items.Add([pscustomobject]@{
      handle = $hWnd.ToInt64()
      process_id = [int]$procId
      process_name = $processName
      process_path = $processPath
      command_line = $commandLine
      title = $title
      rect = [pscustomobject]@{
        left = $rect.Left
        top = $rect.Top
        right = $rect.Right
        bottom = $rect.Bottom
        width = $rect.Right - $rect.Left
        height = $rect.Bottom - $rect.Top
      }
    })
    return $true
  }

  [void][NativeWindowToolsSubmit11]::EnumWindows($callback, [IntPtr]::Zero)
  return @($items)
}

function Get-WindowRectObject {
  param([Parameter(Mandatory=$true)][IntPtr]$Handle)
  $rect = New-Object NativeWindowToolsSubmit11+RECT
  if (-not [NativeWindowToolsSubmit11]::GetWindowRect($Handle, [ref]$rect)) {
    throw "GetWindowRect failed for handle $($Handle.ToInt64())."
  }
  return [pscustomobject]@{
    left = $rect.Left
    top = $rect.Top
    right = $rect.Right
    bottom = $rect.Bottom
    width = $rect.Right - $rect.Left
    height = $rect.Bottom - $rect.Top
  }
}

function Save-WindowCrop {
  param(
    [Parameter(Mandatory=$true)][IntPtr]$Handle,
    [Parameter(Mandatory=$true)][string]$FileName,
    [int]$X = 0,
    [int]$Y = 0,
    [int]$Width = 0,
    [int]$Height = 220
  )

  $rect = Get-WindowRectObject -Handle $Handle
  $windowWidth = [Math]::Max(1, $rect.width)
  $windowHeight = [Math]::Max(1, $rect.height)
  $captureWidth = if ($Width -gt 0) { [Math]::Min($Width, $windowWidth - $X) } else { $windowWidth }
  $captureHeight = [Math]::Min($Height, $windowHeight - $Y)
  $captureWidth = [Math]::Max(1, $captureWidth)
  $captureHeight = [Math]::Max(1, $captureHeight)

  $bitmap = New-Object System.Drawing.Bitmap $captureWidth, $captureHeight
  $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
  try {
    $graphics.CopyFromScreen($rect.left + $X, $rect.top + $Y, 0, 0, (New-Object System.Drawing.Size($captureWidth, $captureHeight)))
    $path = Join-Path (Join-Path $EvidenceDir "screenshots") $FileName
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    return [pscustomobject]@{
      file = $path
      width = $captureWidth
      height = $captureHeight
      source_rect = [pscustomobject]@{
        left = $rect.left + $X
        top = $rect.top + $Y
        right = $rect.left + $X + $captureWidth
        bottom = $rect.top + $Y + $captureHeight
      }
    }
  } finally {
    $graphics.Dispose()
    $bitmap.Dispose()
  }
}

function Send-Text {
  param([Parameter(Mandatory=$true)][string]$Text)
  [System.Windows.Forms.SendKeys]::SendWait($Text)
}

function Get-UserDataDirFromCommandLine {
  param([string]$CommandLine)
  if ([string]::IsNullOrWhiteSpace($CommandLine)) {
    return $null
  }

  $patterns = @(
    '--user-data-dir="([^"]+)"',
    "--user-data-dir='([^']+)'",
    '--user-data-dir\s+([^\s]+)'
  )

  foreach ($pattern in $patterns) {
    $match = [regex]::Match($CommandLine, $pattern)
    if ($match.Success) {
      return $match.Groups[1].Value
    }
  }
  return $null
}

function Get-CandidateEvidenceRoots {
  param($Target)

  $roots = New-Object System.Collections.Generic.List[string]
  $bases = New-Object System.Collections.Generic.List[string]
  $appData = [Environment]::GetFolderPath("ApplicationData")
  if ($appData) {
    $bases.Add((Join-Path $appData "Cursor\User\globalStorage"))
  }

  $userDataDir = Get-UserDataDirFromCommandLine -CommandLine $Target.command_line
  if ($userDataDir) {
    $bases.Add((Join-Path $userDataDir "User\globalStorage"))
  }

  $explicit = @(
    "C:\Users\lubao\AppData\Roaming\Cursor\User\globalStorage\xiigen.cursor-xiigen-smart-skills\xiigen-cursor-host-runtime",
    "C:\Users\lubao\AppData\Roaming\Cursor\User\globalStorage\xiigen.cursor-xiigen-smart-skills",
    "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path",
    "C:\Projects\llm_mvp_core_codex_sync_20260628\docs\plans\evidence\extension-full-readiness-dual-path"
  )

  foreach ($path in $explicit) {
    if (Test-Path -LiteralPath $path) {
      if ((Split-Path -Leaf $path) -eq "xiigen-cursor-host-runtime") {
        $roots.Add($path)
      } else {
        $bases.Add($path)
      }
    }
  }

  foreach ($base in ($bases | Select-Object -Unique)) {
    if (-not (Test-Path -LiteralPath $base)) {
      continue
    }
    if ((Split-Path -Leaf $base) -eq "xiigen-cursor-host-runtime") {
      $roots.Add($base)
      continue
    }
    try {
      Get-ChildItem -LiteralPath $base -Directory -Recurse -Filter "xiigen-cursor-host-runtime" -ErrorAction SilentlyContinue |
        ForEach-Object { $roots.Add($_.FullName) }
    } catch {
      # Non-fatal. Some global storage folders may be locked or disappear.
    }
  }

  return @($roots | Select-Object -Unique)
}

function Get-NewRuntimeFiles {
  param(
    [string[]]$Roots,
    [DateTime]$SinceUtc
  )

  $files = New-Object System.Collections.ArrayList
  foreach ($root in $Roots) {
    if (-not (Test-Path -LiteralPath $root)) {
      continue
    }
    try {
      Get-ChildItem -LiteralPath $root -File -Filter "xiigen-cursor-xiigen.smartSkills.select-*" -ErrorAction SilentlyContinue |
        Where-Object { $_.LastWriteTimeUtc -ge $SinceUtc.AddSeconds(-5) } |
        ForEach-Object {
          [void]$files.Add([pscustomobject]@{
            full_name = $_.FullName
            name = $_.Name
            length = $_.Length
            last_write_utc = $_.LastWriteTimeUtc.ToString("o")
            evidence_root = $root
          })
        }
      Get-ChildItem -LiteralPath $root -File -Filter "xiigen-cursor-host-runtime-latest.json" -ErrorAction SilentlyContinue |
        Where-Object { $_.LastWriteTimeUtc -ge $SinceUtc.AddSeconds(-5) } |
        ForEach-Object {
          [void]$files.Add([pscustomobject]@{
            full_name = $_.FullName
            name = $_.Name
            length = $_.Length
            last_write_utc = $_.LastWriteTimeUtc.ToString("o")
            evidence_root = $root
          })
        }
    } catch {
      # Continue scanning other roots.
    }
  }

  return @($files | Sort-Object full_name -Unique)
}

function Get-TraceCorrelation {
  param(
    [object[]]$RuntimeFiles,
    [string]$Marker,
    [string]$ScenarioId,
    [string]$PairId,
    [string]$RuntimeCopyDir
  )

  New-Item -ItemType Directory -Force -Path $RuntimeCopyDir | Out-Null
  $records = New-Object System.Collections.ArrayList
  foreach ($file in $RuntimeFiles) {
    $raw = ""
    try {
      $raw = Get-Content -Raw -LiteralPath $file.full_name
    } catch {
      $raw = ""
    }
    $copyPath = Join-Path $RuntimeCopyDir $file.name
    try {
      Copy-Item -LiteralPath $file.full_name -Destination $copyPath -Force
    } catch {
      $copyPath = $null
    }
    [void]$records.Add([pscustomobject]@{
      source_path = $file.full_name
      copied_path = $copyPath
      name = $file.name
      length = $file.length
      last_write_utc = $file.last_write_utc
      contains_marker = $raw.Contains($Marker)
      contains_scenario_id = $raw.Contains($ScenarioId)
      contains_pair_id = $raw.Contains($PairId)
      looks_like_request = $file.name -like "*request-*.json"
      looks_like_host_trace = $file.name -like "*host-trace-*.json"
      looks_like_core_trace = $file.name -like "*core-trace-*.json"
      looks_like_core_response = $file.name -like "*core-response-*.json"
      looks_like_entrypoint_response = $file.name -like "*entrypoint-response-*.json"
    })
  }

  $requestRecords = @($records | Where-Object { $_.looks_like_request -and $_.contains_marker -and $_.contains_scenario_id -and $_.contains_pair_id })
  $sameMarker = $requestRecords.Count -gt 0
  $coreTraceRecords = @($records | Where-Object { $_.looks_like_core_trace })
  $hostTraceRecords = @($records | Where-Object { $_.looks_like_host_trace })
  $responseRecords = @($records | Where-Object { $_.looks_like_core_response })
  $entrypointRecords = @($records | Where-Object { $_.looks_like_entrypoint_response })
  $submitted = $sameMarker -and ($coreTraceRecords.Count -gt 0) -and ($hostTraceRecords.Count -gt 0 -or $responseRecords.Count -gt 0 -or $entrypointRecords.Count -gt 0)

  return [pscustomobject]@{
    same_marker = $sameMarker
    submitted_to_core = $submitted
    copied_runtime_file_count = @($records | Where-Object { $_.copied_path }).Count
    runtime_file_count = @($records).Count
    same_marker_request_count = $requestRecords.Count
    core_trace_count = $coreTraceRecords.Count
    host_trace_count = $hostTraceRecords.Count
    core_response_count = $responseRecords.Count
    entrypoint_response_count = $entrypointRecords.Count
    files = @($records)
  }
}

New-Item -ItemType Directory -Force -Path $EvidenceDir | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $EvidenceDir "screenshots") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $EvidenceDir "logs") | Out-Null

$targetTitle = "[Extension Development Host] XIIGen Smart Skills - cursor-xiigen-smart-skills - Cursor"
$marker = "FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1"
$scenarioId = "frdp-p01-cursor-token-savings-baseline"
$pairId = "frdp-p01-row12-cursor-token-baseline-v1"
$markerText = "$marker | $scenarioId | $pairId"
$scriptStartUtc = [DateTime]::UtcNow
$screens = @()
$actions = New-Object System.Collections.ArrayList
$blocker = $null
$exceptionText = $null
$submitStartUtc = $null

try {
  $windows = Get-VisibleWindows
  Write-JsonFile -Value $windows -Path (Join-Path $EvidenceDir "window-inventory.json")

  $target = $windows |
    Where-Object { $_.title -eq $targetTitle -and $_.process_name -eq "Cursor" } |
    Select-Object -First 1

  if (-not $target) {
    $blocker = "Target Cursor Extension Development Host window was not found by exact title/process."
    throw $blocker
  }

  $evidenceRootsBefore = Get-CandidateEvidenceRoots -Target $target
  Write-JsonFile -Value $evidenceRootsBefore -Path (Join-Path $EvidenceDir "logs\evidence-roots-before.json")

  $handle = [IntPtr]::new([int64]$target.handle)
  [void][NativeWindowToolsSubmit11]::ShowWindow($handle, 9)
  Start-Sleep -Milliseconds 250
  [void][NativeWindowToolsSubmit11]::SetForegroundWindow($handle)
  Start-Sleep -Milliseconds 800
  [void]$actions.Add("Activated target Cursor Extension Development Host window by exact title/process.")
  $screens += Save-WindowCrop -Handle $handle -FileName "01-target-extension-development-host-top.png" -Height 210

  [System.Windows.Forms.SendKeys]::SendWait("{ESC}")
  Start-Sleep -Milliseconds 300
  [void]$actions.Add("Pressed Esc once to clear transient UI state.")

  [System.Windows.Forms.SendKeys]::SendWait("{F1}")
  Start-Sleep -Milliseconds 900
  [void]$actions.Add("Pressed F1 to open Cursor command palette.")

  Send-Text -Text "xiigen select smart skill"
  Start-Sleep -Milliseconds 900
  [void]$actions.Add("Typed command palette filter: xiigen select smart skill.")
  $screens += Save-WindowCrop -Handle $handle -FileName "02-command-palette-xiigen-select-smart-skill-top.png" -Height 230

  [System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
  Start-Sleep -Milliseconds 900
  [void]$actions.Add("Pressed Enter to open the XIIGen Select Smart Skill input.")

  Send-Text -Text $markerText
  Start-Sleep -Milliseconds 900
  [void]$actions.Add("Typed safe Row12 marker/scenario/pair text into the XIIGen input.")
  [System.Windows.Forms.SendKeys]::SendWait("{HOME}")
  Start-Sleep -Milliseconds 200
  [void]$actions.Add("Pressed Home so the screenshot shows the beginning of the marker text.")
  $screens += Save-WindowCrop -Handle $handle -FileName "03-row12-marker-in-xiigen-input-top.png" -Height 230

  $submitStartUtc = [DateTime]::UtcNow
  [System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
  [void]$actions.Add("Pressed Enter in the XIIGen input to submit the marker/scenario/pair prompt.")

  Start-Sleep -Seconds 4
  [void][NativeWindowToolsSubmit11]::SetForegroundWindow($handle)
  Start-Sleep -Milliseconds 300
  $screens += Save-WindowCrop -Handle $handle -FileName "04-after-submit-result-top.png" -Height 260
  $rectAfter = Get-WindowRectObject -Handle $handle
  $bottomCropY = [Math]::Max(0, $rectAfter.height - 300)
  $bottomCropX = [Math]::Max(0, $rectAfter.width - 620)
  $screens += Save-WindowCrop -Handle $handle -FileName "05-after-submit-bottom-right.png" -X $bottomCropX -Y $bottomCropY -Width 620 -Height 300

  $runtimeFiles = @()
  $evidenceRootsAfter = $evidenceRootsBefore
  for ($i = 0; $i -lt 8; $i++) {
    $evidenceRootsAfter = Get-CandidateEvidenceRoots -Target $target
    $runtimeFiles = @(Get-NewRuntimeFiles -Roots $evidenceRootsAfter -SinceUtc $submitStartUtc)
    if (@($runtimeFiles | Where-Object { $_.name -like "*core-trace-*.json" }).Count -gt 0 -and
        @($runtimeFiles | Where-Object { $_.name -like "*request-*.json" }).Count -gt 0) {
      break
    }
    Start-Sleep -Seconds 2
  }
  Write-JsonFile -Value $evidenceRootsAfter -Path (Join-Path $EvidenceDir "logs\evidence-roots-after.json")
  Write-JsonFile -Value $runtimeFiles -Path (Join-Path $EvidenceDir "logs\runtime-files-after-submit.json")

  $correlation = Get-TraceCorrelation -RuntimeFiles $runtimeFiles -Marker $marker -ScenarioId $scenarioId -PairId $pairId -RuntimeCopyDir (Join-Path $EvidenceDir "runtime-evidence")
  Write-JsonFile -Value $correlation -Path (Join-Path $EvidenceDir "trace-correlation.json")

  $status = if ($correlation.submitted_to_core) { "PASS_UI_SUBMIT_REACHED_CORE_SAME_MARKER" } else { "NEEDS_FIX_UI_SUBMIT_CORE_TRACE_NOT_PROVEN" }
  $pass = [bool]$correlation.submitted_to_core
  $blockerText = if ($pass) { $null } else { "UI submit action was attempted, but same-marker request plus core trace/host trace evidence was not found in discovered runtime evidence roots." }

  $report = [pscustomobject]@{
    schema_version = "frdp.row12.cursor.path_b.ui_submit_to_core.v1"
    generated_at_utc = [DateTime]::UtcNow.ToString("o")
    script_start_utc = $scriptStartUtc.ToString("o")
    submit_start_utc = if ($submitStartUtc) { $submitStartUtc.ToString("o") } else { $null }
    status = $status
    pass = $pass
    marker = $marker
    scenario_id = $scenarioId
    pair_id = $pairId
    ui_target = $target
    actions_performed = @($actions)
    screenshots = $screens
    screenshot_count = @($screens).Count
    evidence_roots = $evidenceRootsAfter
    runtime_files_after_submit_count = @($runtimeFiles).Count
    trace_correlation = $correlation
    submitted_to_core = [bool]$correlation.submitted_to_core
    same_marker = [bool]$correlation.same_marker
    no_secrets_captured = $true
    external_ai_used = $false
    readiness_claimed = $false
    token_savings_claimed = $false
    semantic_quality_claimed = $false
    product_code_edits = 0
    blocker = $blockerText
  }
  Write-JsonFile -Value $report -Path (Join-Path $EvidenceDir "report.json")
  if (-not $pass) {
    exit 3
  }
  exit 0
} catch {
  if (-not $blocker) {
    $blocker = "UI submit helper failed before it could prove submit-to-core."
  }
  $exceptionText = $_.Exception.Message
  $report = [pscustomobject]@{
    schema_version = "frdp.row12.cursor.path_b.ui_submit_to_core.v1"
    generated_at_utc = [DateTime]::UtcNow.ToString("o")
    script_start_utc = $scriptStartUtc.ToString("o")
    submit_start_utc = if ($submitStartUtc) { $submitStartUtc.ToString("o") } else { $null }
    status = "NEEDS_FIX_UI_HELPER_BLOCKED"
    pass = $false
    marker = $marker
    scenario_id = $scenarioId
    pair_id = $pairId
    actions_performed = @($actions)
    screenshots = $screens
    screenshot_count = @($screens).Count
    submitted_to_core = $false
    same_marker = $false
    no_secrets_captured = $true
    external_ai_used = $false
    readiness_claimed = $false
    token_savings_claimed = $false
    semantic_quality_claimed = $false
    product_code_edits = 0
    blocker = $blocker
    exception = $exceptionText
  }
  Write-JsonFile -Value $report -Path (Join-Path $EvidenceDir "report.json")
  exit 2
}
