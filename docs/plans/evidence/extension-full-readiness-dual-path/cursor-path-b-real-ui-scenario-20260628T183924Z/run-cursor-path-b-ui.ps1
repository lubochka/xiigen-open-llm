param(
  [string]$EvidenceDir = "C:\Projects\llm_mvp_core_codex_sync_20260628\docs\plans\evidence\extension-full-readiness-dual-path\cursor-path-b-real-ui-scenario-20260628T183924Z"
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$nativeSource = @"
using System;
using System.Text;
using System.Runtime.InteropServices;

public static class NativeWindowTools {
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

Add-Type -TypeDefinition $nativeSource

function Get-VisibleWindows {
  $items = New-Object System.Collections.ArrayList
  $callback = [NativeWindowTools+EnumWindowsProc]{
    param([IntPtr]$hWnd, [IntPtr]$lParam)

    if (-not [NativeWindowTools]::IsWindowVisible($hWnd)) {
      return $true
    }

    $length = [NativeWindowTools]::GetWindowTextLength($hWnd)
    if ($length -le 0) {
      return $true
    }

    $builder = New-Object System.Text.StringBuilder ($length + 1)
    [void][NativeWindowTools]::GetWindowText($hWnd, $builder, $builder.Capacity)
    $title = $builder.ToString()
    if ([string]::IsNullOrWhiteSpace($title)) {
      return $true
    }

    $procId = [uint32]0
    [void][NativeWindowTools]::GetWindowThreadProcessId($hWnd, [ref]$procId)
    $processPath = $null
    $processName = $null
    try {
      $proc = Get-Process -Id ([int]$procId) -ErrorAction Stop
      $processName = $proc.ProcessName
      $processPath = $proc.Path
    } catch {
      $processName = $null
      $processPath = $null
    }

    $rect = New-Object NativeWindowTools+RECT
    [void][NativeWindowTools]::GetWindowRect($hWnd, [ref]$rect)
    [void]$items.Add([pscustomobject]@{
      handle = $hWnd.ToInt64()
      process_id = [int]$procId
      process_name = $processName
      process_path = $processPath
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

  [void][NativeWindowTools]::EnumWindows($callback, [IntPtr]::Zero)
  return @($items)
}

function Save-TopCrop {
  param(
    [Parameter(Mandatory=$true)][IntPtr]$Handle,
    [Parameter(Mandatory=$true)][string]$FileName,
    [int]$Height = 320
  )

  $rect = New-Object NativeWindowTools+RECT
  if (-not [NativeWindowTools]::GetWindowRect($Handle, [ref]$rect)) {
    throw "GetWindowRect failed for handle $($Handle.ToInt64())."
  }

  $width = [Math]::Max(1, $rect.Right - $rect.Left)
  $windowHeight = [Math]::Max(1, $rect.Bottom - $rect.Top)
  $cropHeight = [Math]::Min($Height, $windowHeight)

  $bitmap = New-Object System.Drawing.Bitmap $width, $cropHeight
  $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
  try {
    $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, (New-Object System.Drawing.Size($width, $cropHeight)))
    $path = Join-Path $EvidenceDir $FileName
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    return [pscustomobject]@{
      file = $path
      width = $width
      height = $cropHeight
      source_rect = [pscustomobject]@{
        left = $rect.Left
        top = $rect.Top
        right = $rect.Right
        bottom = $rect.Bottom
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

New-Item -ItemType Directory -Force -Path $EvidenceDir | Out-Null

$targetTitle = "[Extension Development Host] XIIGen Smart Skills - cursor-xiigen-smart-skills - Cursor"
$markerText = "FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1 | frdp-p01-cursor-token-savings-baseline | frdp-p01-row12-cursor-token-baseline-v1"
$startUtc = (Get-Date).ToUniversalTime().ToString("o")
$screens = @()
$actions = New-Object System.Collections.ArrayList

$windows = Get-VisibleWindows
$windows | ConvertTo-Json -Depth 8 | Set-Content -Path (Join-Path $EvidenceDir "window-inventory.json") -Encoding UTF8

$target = $windows |
  Where-Object { $_.title -eq $targetTitle -and $_.process_name -eq "Cursor" } |
  Select-Object -First 1

if (-not $target) {
  $report = [pscustomobject]@{
    schema_version = "frdp.row12.cursor.path_b.real_ui_scenario.v1"
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    status = "NEEDS_PARENT_TOOLING"
    pass = $false
    blocker = "Target Cursor Extension Development Host window was not found by exact title/process."
    target_title = $targetTitle
    screenshots = @()
    actions_performed = @()
    window_inventory = "window-inventory.json"
    no_secrets_captured = $true
    submitted_to_core = $false
  }
  $report | ConvertTo-Json -Depth 8 | Set-Content -Path (Join-Path $EvidenceDir "report.json") -Encoding UTF8
  exit 2
}

$handle = [IntPtr]::new([int64]$target.handle)
[void][NativeWindowTools]::ShowWindow($handle, 9)
Start-Sleep -Milliseconds 250
[void][NativeWindowTools]::SetForegroundWindow($handle)
Start-Sleep -Milliseconds 700
[void]$actions.Add("Activated target Cursor Extension Development Host window by exact title/process.")

$screens += Save-TopCrop -Handle $handle -FileName "01-target-extension-development-host-top.png" -Height 170

[System.Windows.Forms.SendKeys]::SendWait("{ESC}")
Start-Sleep -Milliseconds 300
[void]$actions.Add("Pressed Esc once to clear transient UI state.")

[System.Windows.Forms.SendKeys]::SendWait("{F1}")
Start-Sleep -Milliseconds 900
[void]$actions.Add("Pressed F1 to open Cursor command palette.")

Send-Text -Text "xiigen select smart skill"
Start-Sleep -Milliseconds 900
[void]$actions.Add("Typed command palette filter: xiigen select smart skill.")
$screens += Save-TopCrop -Handle $handle -FileName "02-command-palette-xiigen-select-smart-skill-top.png" -Height 180

[System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
Start-Sleep -Milliseconds 900
[void]$actions.Add("Pressed Enter to open the XIIGen Select Smart Skill input.")

Send-Text -Text $markerText
Start-Sleep -Milliseconds 900
[void]$actions.Add("Typed safe Row12 marker/scenario/pair text into the XIIGen input.")
[System.Windows.Forms.SendKeys]::SendWait("{HOME}")
Start-Sleep -Milliseconds 200
[void]$actions.Add("Pressed Home so the screenshot shows the beginning of the marker text.")
$screens += Save-TopCrop -Handle $handle -FileName "03-row12-marker-in-xiigen-input-top.png" -Height 180

[System.Windows.Forms.SendKeys]::SendWait("{ESC}")
Start-Sleep -Milliseconds 500
[void]$actions.Add("Pressed Esc to cancel without submitting the prompt.")
$screens += Save-TopCrop -Handle $handle -FileName "04-after-cancel-no-submit-top.png" -Height 170

$report = [pscustomobject]@{
  schema_version = "frdp.row12.cursor.path_b.real_ui_scenario.v1"
  generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
  start_utc = $startUtc
  status = "PASS_PATH_B_REAL_CURSOR_UI_ROUTE_EVIDENCE_CREATED"
  pass = $true
  scenario_id = "frdp-p01-cursor-token-savings-baseline"
  pair_id = "frdp-p01-row12-cursor-token-baseline-v1"
  marker = "FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1"
  prompt_hash_path_a_expected = "sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374"
  evidence_scope = "Path B real local Cursor UI route evidence only: screenshots are top-window crops of Extension Development Host, command palette route, XIIGen input with Row12 marker/scenario/pair, then cancellation."
  ui_target = $target
  screenshots = $screens
  actions_performed = @($actions)
  expected_visible_state = "Real Cursor Extension Development Host exposes XIIGen command route and accepts Row12 marker/scenario/pair text in XIIGen input."
  actual_visible_state = "Fresh top-window screenshots were captured during one local UI attempt. Human visual review is still required before joining with Path A."
  login_or_session_issue_seen = $false
  no_secrets_captured = $true
  screenshot_redaction = "Top-window crops only; no clipboard read; no password/token typing; typed text is synthetic marker/scenario/pair only."
  submitted_to_core = $false
  external_ai_used = $false
  non_claims = @(
    "product_readiness_claimed=false",
    "token_savings_claimed=false",
    "semantic_quality_claimed=false",
    "training_claimed=false",
    "checkpoint_promotion_claimed=false",
    "locator_update_claimed=false",
    "full_live_chat_path_claimed=false",
    "joined_path_a_b_readiness_claimed=false"
  )
}

$report | ConvertTo-Json -Depth 12 | Set-Content -Path (Join-Path $EvidenceDir "report.json") -Encoding UTF8
exit 0
