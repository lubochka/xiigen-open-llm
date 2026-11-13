param(
  [Parameter(Mandatory = $true)]
  [string]$EvidenceDir,

  [Parameter(Mandatory = $true)]
  [string]$ScenarioJsonPath,

  [switch]$ReloadFirst,

  [switch]$CloseEditorsBeforePanel,

  [string]$FilePrefix = ""
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$script:actions = New-Object System.Collections.Generic.List[object]
$script:screenshots = New-Object System.Collections.Generic.List[string]
$script:filePrefix = $FilePrefix

function Add-Action {
  param(
    [string]$Step,
    [string]$Detail,
    [object]$Data = $null
  )
  $script:actions.Add([pscustomobject]@{
    at = (Get-Date).ToString("o")
    step = $Step
    detail = $Detail
    data = $Data
  })
}

$user32 = @"
using System;
using System.Runtime.InteropServices;
using System.Text;

public static class User32Bridge {
  public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

  [DllImport("user32.dll")]
  public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

  [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
  public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

  [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
  public static extern int GetWindowTextLength(IntPtr hWnd);

  [DllImport("user32.dll")]
  public static extern bool IsWindowVisible(IntPtr hWnd);

  [DllImport("user32.dll")]
  public static extern bool SetForegroundWindow(IntPtr hWnd);

  [DllImport("user32.dll")]
  public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

  [DllImport("user32.dll")]
  public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

  [DllImport("user32.dll")]
  public static extern bool SetCursorPos(int x, int y);

  [DllImport("user32.dll")]
  public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

  [DllImport("user32.dll")]
  public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
}

[StructLayout(LayoutKind.Sequential)]
public struct RECT {
  public int Left;
  public int Top;
  public int Right;
  public int Bottom;
}
"@

Add-Type $user32

function Get-VisibleWindows {
  $windows = New-Object System.Collections.Generic.List[object]
  $callback = [User32Bridge+EnumWindowsProc]{
    param([IntPtr]$hWnd, [IntPtr]$lParam)
    if ([User32Bridge]::IsWindowVisible($hWnd)) {
      $len = [User32Bridge]::GetWindowTextLength($hWnd)
      if ($len -gt 0) {
        $sb = New-Object System.Text.StringBuilder ($len + 1)
        [void][User32Bridge]::GetWindowText($hWnd, $sb, $sb.Capacity)
        $title = $sb.ToString()
        $rect = New-Object RECT
        [void][User32Bridge]::GetWindowRect($hWnd, [ref]$rect)
        $windows.Add([pscustomobject]@{
          handle = ("0x{0:X}" -f $hWnd.ToInt64())
          title = $title
          left = $rect.Left
          top = $rect.Top
          right = $rect.Right
          bottom = $rect.Bottom
          width = $rect.Right - $rect.Left
          height = $rect.Bottom - $rect.Top
        })
      }
    }
    return $true
  }
  [void][User32Bridge]::EnumWindows($callback, [IntPtr]::Zero)
  return $windows
}

function Capture-Screen {
  param([string]$Name)
  $bounds = [System.Windows.Forms.SystemInformation]::VirtualScreen
  $bitmap = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
  $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
  try {
    $graphics.CopyFromScreen($bounds.Left, $bounds.Top, 0, 0, $bounds.Size)
    $path = Join-Path $EvidenceDir ($script:filePrefix + $Name)
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $script:screenshots.Add($path)
    Add-Action "screenshot" "Captured $Name" @{ path = $path }
    return $path
  }
  finally {
    $graphics.Dispose()
    $bitmap.Dispose()
  }
}

function Send-KeysAndWait {
  param(
    [string]$Keys,
    [int]$DelayMs = 350
  )

  function KeyDown([byte]$vk) {
    [User32Bridge]::keybd_event($vk, 0, 0, [UIntPtr]::Zero)
  }

  function KeyUp([byte]$vk) {
    [User32Bridge]::keybd_event($vk, 0, 2, [UIntPtr]::Zero)
  }

  switch ($Keys) {
    "^+P" {
      KeyDown 0x11
      KeyDown 0x10
      KeyDown 0x50
      Start-Sleep -Milliseconds 80
      KeyUp 0x50
      KeyUp 0x10
      KeyUp 0x11
    }
    "^A" {
      KeyDown 0x11
      KeyDown 0x41
      Start-Sleep -Milliseconds 60
      KeyUp 0x41
      KeyUp 0x11
    }
    "^V" {
      KeyDown 0x11
      KeyDown 0x56
      Start-Sleep -Milliseconds 60
      KeyUp 0x56
      KeyUp 0x11
    }
    "{ENTER}" {
      KeyDown 0x0D
      Start-Sleep -Milliseconds 60
      KeyUp 0x0D
    }
    "{TAB}" {
      KeyDown 0x09
      Start-Sleep -Milliseconds 60
      KeyUp 0x09
    }
    "{ESC}" {
      KeyDown 0x1B
      Start-Sleep -Milliseconds 60
      KeyUp 0x1B
    }
    default {
      [System.Windows.Forms.SendKeys]::SendWait($Keys)
    }
  }

  Start-Sleep -Milliseconds $DelayMs
}

function Set-ClipboardText {
  param([string]$Text)
  [System.Windows.Forms.Clipboard]::SetText($Text)
  Start-Sleep -Milliseconds 150
}

function Click-At {
  param(
    [int]$X,
    [int]$Y,
    [int]$DelayMs = 350
  )
  [void][User32Bridge]::SetCursorPos($X, $Y)
  Start-Sleep -Milliseconds 80
  [User32Bridge]::mouse_event(0x0002, [uint32]$X, [uint32]$Y, 0, [UIntPtr]::Zero)
  [User32Bridge]::mouse_event(0x0004, [uint32]$X, [uint32]$Y, 0, [UIntPtr]::Zero)
  Start-Sleep -Milliseconds $DelayMs
}

function Focus-TargetWindow {
  param([object]$Window)
  $handle = [IntPtr]::new([Convert]::ToInt64(($Window.handle -replace "^0x", ""), 16))
  [void][User32Bridge]::ShowWindow($handle, 3)
  Start-Sleep -Milliseconds 500
  [void][User32Bridge]::SetForegroundWindow($handle)
  Start-Sleep -Milliseconds 800
}

New-Item -ItemType Directory -Force -Path $EvidenceDir | Out-Null

$scenarioRaw = [System.IO.File]::ReadAllText($ScenarioJsonPath, [System.Text.UTF8Encoding]::new($false))
$scenario = $scenarioRaw | ConvertFrom-Json
$prompt = $null
$promptSource = "exact_prompt"
if ($scenario.PSObject.Properties.Name -contains "exact_prompt" -and -not [string]::IsNullOrWhiteSpace([string]$scenario.exact_prompt)) {
  $prompt = [string]$scenario.exact_prompt
}
else {
  $promptSource = "fallback_missing_exact_prompt"
  $prompt = [string]($scenario.dual_path_same_scenario_proof.path_b.goal)
}

[System.IO.File]::WriteAllText(
  (Join-Path $EvidenceDir ($script:filePrefix + "prompt-source.json")),
  (@{
    scenario_id = [string]$scenario.scenario_id
    prompt_source = $promptSource
    exact_prompt_present = ($promptSource -eq "exact_prompt")
    prompt = $prompt
  } | ConvertTo-Json -Depth 6),
  [System.Text.UTF8Encoding]::new($false)
)

Add-Action "source" "Loaded scenario prompt" @{
  scenario_id = [string]$scenario.scenario_id
  prompt_source = $promptSource
  prompt_length = $prompt.Length
}

$windowsBefore = @(Get-VisibleWindows)
[System.IO.File]::WriteAllText(
  (Join-Path $EvidenceDir ($script:filePrefix + "window-inventory-before.json")),
  ($windowsBefore | ConvertTo-Json -Depth 6),
  [System.Text.UTF8Encoding]::new($false)
)

$target = $windowsBefore |
  Where-Object { $_.title -like "*Extension Development Host*" -and $_.title -like "*cursor-xiigen-smart-skills*" -and $_.title -like "*Cursor*" } |
  Select-Object -First 1

if (-not $target) {
  Add-Action "target-window" "Target Cursor Extension Development Host window was not found" $null
  Capture-Screen "00-desktop-target-window-not-found.png" | Out-Null
  throw "TARGET_WINDOW_NOT_FOUND"
}

Add-Action "target-window" "Found target Cursor Extension Development Host window" $target
Focus-TargetWindow $target
Send-KeysAndWait "{ESC}" 300

if ($ReloadFirst) {
  Capture-Screen "00-before-reload.png" | Out-Null
  Set-ClipboardText "Developer: Reload Window"
  Send-KeysAndWait "^+P" 700
  Send-KeysAndWait "^A" 150
  Send-KeysAndWait "^V" 700
  Capture-Screen "01-command-palette-reload-window.png" | Out-Null
  Send-KeysAndWait "{ENTER}" 8500
  Add-Action "reload-window" "Requested Developer: Reload Window before opening panel" $null

  $deadline = (Get-Date).AddSeconds(30)
  do {
    $windowsAfterReload = @(Get-VisibleWindows)
    $targetAfterReload = $windowsAfterReload |
      Where-Object { $_.title -like "*Extension Development Host*" -and $_.title -like "*cursor-xiigen-smart-skills*" -and $_.title -like "*Cursor*" } |
      Select-Object -First 1
    if ($targetAfterReload) {
      $target = $targetAfterReload
      break
    }
    Start-Sleep -Milliseconds 750
  } while ((Get-Date) -lt $deadline)

  if (-not $target) {
    Add-Action "reload-window" "Target window did not reappear after reload request" $null
    Capture-Screen "02-after-reload-target-missing.png" | Out-Null
    throw "TARGET_WINDOW_NOT_FOUND_AFTER_RELOAD"
  }

  Focus-TargetWindow $target
  Send-KeysAndWait "{ESC}" 300
  Capture-Screen "02-after-reload-focused.png" | Out-Null
}

if ($CloseEditorsBeforePanel) {
  Set-ClipboardText "View: Close All Editors"
  Send-KeysAndWait "^+P" 700
  Send-KeysAndWait "^A" 150
  Send-KeysAndWait "^V" 700
  Capture-Screen "03-command-palette-close-editors.png" | Out-Null
  Send-KeysAndWait "{ENTER}" 1800
  Add-Action "close-editors" "Requested View: Close All Editors before opening fresh XIIGen panel" $null
  Capture-Screen "04-after-close-editors.png" | Out-Null
  Send-KeysAndWait "{ESC}" 300
}

Capture-Screen "00-desktop-before-panel-open.png" | Out-Null

Set-ClipboardText "XIIGen: Open Smart Skills Panel"
Send-KeysAndWait "^+P" 700
Send-KeysAndWait "^A" 150
Send-KeysAndWait "^V" 700
Capture-Screen "01-command-palette-xiigen-panel-command.png" | Out-Null
Send-KeysAndWait "{ENTER}" 2500
Add-Action "panel-open" "Requested XIIGen Smart Skills panel through command palette" @{
  command = "XIIGen: Open Smart Skills Panel"
}
Capture-Screen "02-panel-after-open.png" | Out-Null

$windowsAfterPanel = @(Get-VisibleWindows)
[System.IO.File]::WriteAllText(
  (Join-Path $EvidenceDir ($script:filePrefix + "window-inventory-after-panel.json")),
  ($windowsAfterPanel | ConvertTo-Json -Depth 6),
  [System.Text.UTF8Encoding]::new($false)
)
$targetAfter = $windowsAfterPanel |
  Where-Object { $_.title -like "*Extension Development Host*" -and $_.title -like "*cursor-xiigen-smart-skills*" -and $_.title -like "*Cursor*" } |
  Select-Object -First 1
if ($targetAfter) {
  $target = $targetAfter
}

$clickX = [int]($target.left + ($target.width * 0.50))
$textareaY = [int]($target.top + [Math]::Min([Math]::Max(220, $target.height * 0.24), 320))
Click-At $clickX $textareaY 500
Set-ClipboardText $prompt
Send-KeysAndWait "^A" 100
Send-KeysAndWait "^V" 900
Add-Action "prompt-input" "Pasted scenario prompt into expected textarea region" @{
  prompt_source = $promptSource
  prompt_length = $prompt.Length
  click_x = $clickX
  click_y = $textareaY
}
Capture-Screen "03-before-submit-prompt-visible.png" | Out-Null

Send-KeysAndWait "{TAB}" 250
Send-KeysAndWait "{ENTER}" 1500
Add-Action "submit" "Sent Tab then Enter after textarea paste to activate submit button" $null

$buttonX = [int]($target.left + ($target.width * 0.30))
$buttonY = [int]($textareaY + 155)
Click-At $buttonX $buttonY 1500
Add-Action "submit-fallback-click" "Clicked approximate submit button position after keyboard submit attempt" @{
  click_x = $buttonX
  click_y = $buttonY
}

Start-Sleep -Seconds 8
Capture-Screen "04-after-submit-response-or-status.png" | Out-Null

$windowsAfterSubmit = @(Get-VisibleWindows)
[System.IO.File]::WriteAllText(
  (Join-Path $EvidenceDir ($script:filePrefix + "window-inventory-after-submit.json")),
  ($windowsAfterSubmit | ConvertTo-Json -Depth 6),
  [System.Text.UTF8Encoding]::new($false)
)

[System.IO.File]::WriteAllText(
  (Join-Path $EvidenceDir ($script:filePrefix + "runner-action-log.json")),
  ($script:actions | ConvertTo-Json -Depth 8),
  [System.Text.UTF8Encoding]::new($false)
)

[System.IO.File]::WriteAllText(
  (Join-Path $EvidenceDir ($script:filePrefix + "runner-summary.json")),
  (@{
    scenario_id = [string]$scenario.scenario_id
    prompt_source = $promptSource
    target_window_found = $true
    prompt_typed_attempted = $true
    submit_attempted = $true
    screenshots = @($script:screenshots)
  } | ConvertTo-Json -Depth 8),
  [System.Text.UTF8Encoding]::new($false)
)

Write-Output (@{
  evidence_dir = $EvidenceDir
  scenario_id = [string]$scenario.scenario_id
  screenshots = @($script:screenshots)
  action_count = $script:actions.Count
} | ConvertTo-Json -Depth 8)
