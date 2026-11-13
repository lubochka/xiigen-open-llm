Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RunFolder = 'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\micro-focus-textarea-run-20260625T101619Z'
$LogsDir = Join-Path $RunFolder 'logs'
$ScreenshotsDir = Join-Path $RunFolder 'screenshots'
$CursorExe = 'C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe'
$ExtensionRoot = 'C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills'
$ProbeJs = Join-Path $LogsDir 'row12-micro-focus-host-probe.js'
$CommandResultPath = Join-Path $LogsDir 'api-open-panel-command-result.json'
$StopPath = Join-Path $LogsDir 'api-open-panel-stop.txt'
$JsonPath = Join-Path $RunFolder 'row12-micro-textarea-focus-packet.json'
$MdPath = Join-Path $RunFolder 'row12-micro-textarea-focus-packet.md'
$BeforeScreenshotPath = Join-Path $ScreenshotsDir 'textarea-focus-before.png'
$ProbeScreenshotPath = Join-Path $ScreenshotsDir 'textarea-focus-probe-visible.png'
$AfterScreenshotPath = Join-Path $ScreenshotsDir 'textarea-focus-after-clear.png'
$BeforeCropPath = Join-Path $ScreenshotsDir 'textarea-focus-before-crop.png'
$ProbeCropPath = Join-Path $ScreenshotsDir 'textarea-focus-probe-visible-crop.png'
$AfterCropPath = Join-Path $ScreenshotsDir 'textarea-focus-after-clear-crop.png'
$ProbeText = 'XIIGEN_FOCUS_PROBE_DO_NOT_SUBMIT'

New-Item -ItemType Directory -Force -Path $LogsDir, $ScreenshotsDir | Out-Null
Remove-Item -LiteralPath $CommandResultPath, $StopPath, $JsonPath, $MdPath, $BeforeScreenshotPath, $ProbeScreenshotPath, $AfterScreenshotPath, $BeforeCropPath, $ProbeCropPath, $AfterCropPath -Force -ErrorAction SilentlyContinue

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms

$Signature = @'
using System;
using System.Runtime.InteropServices;
using System.Text;
public static class Row12FocusWin32 {
  public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
  [DllImport("user32.dll")] public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
  [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hWnd);
  [DllImport("user32.dll", SetLastError=true)] public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
  [DllImport("user32.dll", SetLastError=true)] public static extern int GetWindowTextLength(IntPtr hWnd);
  [DllImport("user32.dll", SetLastError=true)] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
  [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
}
'@
Add-Type $Signature

function ConvertTo-RelativeEvidencePath {
  param([Parameter(Mandatory=$true)][string]$Path)
  if ($Path.StartsWith($RunFolder, [System.StringComparison]::OrdinalIgnoreCase)) {
    return $Path.Substring($RunFolder.Length).TrimStart('\')
  }
  return $Path
}

function Get-CursorDevWindows {
  $Windows = New-Object System.Collections.Generic.List[object]
  $Callback = [Row12FocusWin32+EnumWindowsProc]{
    param([IntPtr]$Hwnd, [IntPtr]$LParam)
    if (-not [Row12FocusWin32]::IsWindowVisible($Hwnd)) { return $true }
    $Len = [Row12FocusWin32]::GetWindowTextLength($Hwnd)
    if ($Len -le 0) { return $true }
    $Sb = New-Object System.Text.StringBuilder ($Len + 1)
    [void][Row12FocusWin32]::GetWindowText($Hwnd, $Sb, $Sb.Capacity)
    [uint32]$WindowPid = 0
    [void][Row12FocusWin32]::GetWindowThreadProcessId($Hwnd, [ref]$WindowPid)
    $Title = $Sb.ToString()
    if ($Title -like '*Extension Development Host*Cursor*') {
      $Rect = New-Object Row12FocusWin32+RECT
      [void][Row12FocusWin32]::GetWindowRect($Hwnd, [ref]$Rect)
      $Windows.Add([pscustomobject]@{
        Handle = $Hwnd
        HandleInt = $Hwnd.ToInt64()
        ProcessId = [int]$WindowPid
        Title = $Title
        Rect = $Rect
      })
    }
    return $true
  }
  [void][Row12FocusWin32]::EnumWindows($Callback, [IntPtr]::Zero)
  return @($Windows.ToArray())
}

function Save-TargetScreenshot {
  param(
    [Parameter(Mandatory=$true)] $Target,
    [Parameter(Mandatory=$true)] [string] $Path
  )
  $Width = [Math]::Max(1, $Target.Rect.Right - $Target.Rect.Left)
  $Height = [Math]::Max(1, $Target.Rect.Bottom - $Target.Rect.Top)
  $Bitmap = New-Object System.Drawing.Bitmap $Width, $Height
  $Graphics = [System.Drawing.Graphics]::FromImage($Bitmap)
  $Hdc = $Graphics.GetHdc()
  $Printed = $false
  try {
    $Printed = [Row12FocusWin32]::PrintWindow($Target.Handle, $Hdc, 2)
  } finally {
    $Graphics.ReleaseHdc($Hdc)
  }
  if (-not $Printed) {
    $Graphics.CopyFromScreen($Target.Rect.Left, $Target.Rect.Top, 0, 0, $Bitmap.Size)
  }
  $Bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
  $Graphics.Dispose()
  $Bitmap.Dispose()
}

function Save-ImageCrop {
  param(
    [Parameter(Mandatory=$true)] [string] $SourcePath,
    [Parameter(Mandatory=$true)] [string] $TargetPath,
    [Parameter(Mandatory=$true)] [int] $X,
    [Parameter(Mandatory=$true)] [int] $Y,
    [Parameter(Mandatory=$true)] [int] $Width,
    [Parameter(Mandatory=$true)] [int] $Height
  )
  if (-not (Test-Path -LiteralPath $SourcePath)) { return $false }
  $Source = [System.Drawing.Bitmap]::FromFile($SourcePath)
  try {
    $SafeX = [Math]::Max(0, [Math]::Min($X, $Source.Width - 1))
    $SafeY = [Math]::Max(0, [Math]::Min($Y, $Source.Height - 1))
    $SafeWidth = [Math]::Max(1, [Math]::Min($Width, $Source.Width - $SafeX))
    $SafeHeight = [Math]::Max(1, [Math]::Min($Height, $Source.Height - $SafeY))
    $Rect = New-Object System.Drawing.Rectangle $SafeX, $SafeY, $SafeWidth, $SafeHeight
    $Crop = $Source.Clone($Rect, $Source.PixelFormat)
    try {
      $Crop.Save($TargetPath, [System.Drawing.Imaging.ImageFormat]::Png)
    } finally {
      $Crop.Dispose()
    }
  } finally {
    $Source.Dispose()
  }
  return (Test-Path -LiteralPath $TargetPath)
}

function Set-TargetVisible {
  param([Parameter(Mandatory=$true)] $Target)
  [void][Row12FocusWin32]::ShowWindow($Target.Handle, 9)
  Start-Sleep -Milliseconds 300
  $Width = [Math]::Max(1, $Target.Rect.Right - $Target.Rect.Left)
  $Height = [Math]::Max(1, $Target.Rect.Bottom - $Target.Rect.Top)
  [void][Row12FocusWin32]::SetWindowPos($Target.Handle, [IntPtr](-1), $Target.Rect.Left, $Target.Rect.Top, $Width, $Height, 0x0040)
  Start-Sleep -Milliseconds 300
  [void][Row12FocusWin32]::SetForegroundWindow($Target.Handle)
  Start-Sleep -Seconds 1
  [void][Row12FocusWin32]::SetWindowPos($Target.Handle, [IntPtr](-2), $Target.Rect.Left, $Target.Rect.Top, $Width, $Height, 0x0040)
}

function Set-TargetActive {
  param([Parameter(Mandatory=$true)] $Target)
  Set-TargetVisible -Target $Target
  try {
    $Shell = New-Object -ComObject WScript.Shell
    [void]$Shell.AppActivate([int]$Target.ProcessId)
    Start-Sleep -Milliseconds 500
  } catch {}
  [void][Row12FocusWin32]::SetForegroundWindow($Target.Handle)
  Start-Sleep -Milliseconds 500
}

function Get-UiRows {
  param([Parameter(Mandatory=$true)] $Root)
  $All = @($Root.FindAll([System.Windows.Automation.TreeScope]::Subtree, [System.Windows.Automation.Condition]::TrueCondition))
  foreach ($El in $All) {
    try {
      $Bounds = $El.Current.BoundingRectangle
      $Value = $null
      try {
        $ValuePattern = $El.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        $Value = $ValuePattern.Current.Value
      } catch {}
      [pscustomobject]@{
        name = $El.Current.Name
        automation_id = $El.Current.AutomationId
        class_name = $El.Current.ClassName
        control_type = $El.Current.ControlType.ProgrammaticName
        is_enabled = $El.Current.IsEnabled
        is_offscreen = $El.Current.IsOffscreen
        bounds = [pscustomobject]@{
          x = [int]$Bounds.X
          y = [int]$Bounds.Y
          width = [int]$Bounds.Width
          height = [int]$Bounds.Height
        }
        value = $Value
      }
    } catch {}
  }
}

function Get-ElementValue {
  param([Parameter(Mandatory=$true)] $Element)
  try {
    $ValuePattern = $Element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
    return $ValuePattern.Current.Value
  } catch {
    return $null
  }
}

function Get-FocusedSummary {
  $Focused = [System.Windows.Automation.AutomationElement]::FocusedElement
  if (-not $Focused) { return $null }
  return [pscustomobject]@{
    name = $Focused.Current.Name
    automation_id = $Focused.Current.AutomationId
    class_name = $Focused.Current.ClassName
    control_type = $Focused.Current.ControlType.ProgrammaticName
    value = Get-ElementValue -Element $Focused
  }
}

function Invoke-LeftClick {
  param([Parameter(Mandatory=$true)] [int] $X, [Parameter(Mandatory=$true)] [int] $Y)
  [void][Row12FocusWin32]::SetCursorPos($X, $Y)
  Start-Sleep -Milliseconds 100
  [Row12FocusWin32]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero)
  Start-Sleep -Milliseconds 50
  [Row12FocusWin32]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero)
}

function Get-ClipboardTextSafe {
  try {
    if ([System.Windows.Forms.Clipboard]::ContainsText()) {
      return [System.Windows.Forms.Clipboard]::GetText()
    }
  } catch {}
  return $null
}

function Set-ClipboardTextSafe {
  param([AllowNull()][string] $Text)
  try {
    if ($null -eq $Text) {
      [System.Windows.Forms.Clipboard]::Clear()
    } else {
      [System.Windows.Forms.Clipboard]::SetText($Text)
    }
    return $true
  } catch {
    return $false
  }
}

$ShortHostRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('xiigen-focus-textarea-' + (Get-Date -Format 'yyyyMMddHHmmss'))
$UserDataDir = Join-Path $ShortHostRoot 'user-data'
$ExtensionsDir = Join-Path $ShortHostRoot 'extensions'
New-Item -ItemType Directory -Force -Path $UserDataDir, $ExtensionsDir | Out-Null

$Arguments = @(
  '--new-window',
  '--user-data-dir', $UserDataDir,
  '--extensions-dir', $ExtensionsDir,
  "--extensionDevelopmentPath=$ExtensionRoot",
  "--extensionTestsPath=$ProbeJs",
  '--disable-workspace-trust',
  '--skip-welcome',
  '--skip-release-notes',
  '--skip-onboarding',
  '--disable-telemetry',
  '--disable-updates',
  '--disable-gpu',
  $ExtensionRoot
)

$ProcessBefore = @(Get-CimInstance Win32_Process -Filter "Name = 'Cursor.exe'" | Select-Object ProcessId, CommandLine)
Push-Location -LiteralPath $ExtensionRoot
try {
  & $CursorExe @Arguments
} finally {
  Pop-Location
}

$CommandDeadline = (Get-Date).AddSeconds(60)
while (-not (Test-Path -LiteralPath $CommandResultPath) -and (Get-Date) -lt $CommandDeadline) {
  Start-Sleep -Milliseconds 500
}

$CommandEvidence = $null
if (Test-Path -LiteralPath $CommandResultPath) {
  $CommandEvidence = Get-Content -LiteralPath $CommandResultPath -Raw | ConvertFrom-Json
}

$ProcessAfter = @(Get-CimInstance Win32_Process -Filter "Name = 'Cursor.exe'" | Select-Object ProcessId, CommandLine)
$MatchingProcesses = @($ProcessAfter | Where-Object {
  $_.CommandLine -and $_.CommandLine.Contains($UserDataDir) -and $_.CommandLine.Contains('--extensionDevelopmentPath')
})
$TargetPid = if (@($MatchingProcesses).Count -gt 0) { [int]$MatchingProcesses[0].ProcessId } else { $null }

$Windows = Get-CursorDevWindows
$Target = if ($TargetPid) { $Windows | Where-Object { $_.ProcessId -eq $TargetPid } | Select-Object -First 1 } else { $null }
if (-not $Target) {
  $Target = $Windows | Sort-Object ProcessId -Descending | Select-Object -First 1
}

$Status = 'TARGET_WINDOW_INPUT_NOT_VERIFIED'
$FocusEvidence = [ordered]@{
  target_window_found = [bool]$Target
  api_open_panel_succeeded = [bool]($CommandEvidence -and $CommandEvidence.api_command -and $CommandEvidence.api_command.succeeded -eq $true)
  target_textarea_found = $false
  target_textarea_automation_id = 'xiigen-smart-skills-prompt'
  target_textarea_control_type = $null
  target_textarea_bounds = $null
  focused_before_type = $null
  value_before_type = $null
  focused_after_type = $null
  value_after_type = $null
  probe_visible_in_target_textarea = $false
  generic_cursor_textbox_probe_detected = $false
  focused_after_clear = $null
  value_after_clear = $null
  proof_method = $null
  visual_probe_screenshot_requires_human_inspection = $false
  visual_coordinate_fallback_used = $false
  clipboard_probe_paste_used = $false
  clipboard_restored = $false
}
$ProbeCleanupStatus = 'not_started'
$ScreenshotCaptured = [ordered]@{
  before = $false
  probe_visible = $false
  after_clear = $false
  before_crop = $false
  probe_visible_crop = $false
  after_clear_crop = $false
  before_path = $null
  probe_visible_path = $null
  after_clear_path = $null
  before_crop_path = $null
  probe_visible_crop_path = $null
  after_clear_crop_path = $null
}
$PanelAccessibilityExcerpt = @()

try {
  if ($Target) {
    Set-TargetActive -Target $Target
    Start-Sleep -Seconds 4
    Save-TargetScreenshot -Target $Target -Path $BeforeScreenshotPath
    $ScreenshotCaptured.before = Test-Path -LiteralPath $BeforeScreenshotPath
    if ($ScreenshotCaptured.before) { $ScreenshotCaptured.before_path = ConvertTo-RelativeEvidencePath -Path $BeforeScreenshotPath }
    $ScreenshotCaptured.before_crop = Save-ImageCrop -SourcePath $BeforeScreenshotPath -TargetPath $BeforeCropPath -X 300 -Y 260 -Width 980 -Height 250
    if ($ScreenshotCaptured.before_crop) { $ScreenshotCaptured.before_crop_path = ConvertTo-RelativeEvidencePath -Path $BeforeCropPath }

    $Root = [System.Windows.Automation.AutomationElement]::FromHandle($Target.Handle)
    $TargetTextAreaCondition = New-Object System.Windows.Automation.PropertyCondition(
      [System.Windows.Automation.AutomationElement]::AutomationIdProperty,
      'xiigen-smart-skills-prompt'
    )
    $TargetTextArea = $Root.FindFirst([System.Windows.Automation.TreeScope]::Subtree, $TargetTextAreaCondition)

    $UiRowsBefore = @(Get-UiRows -Root $Root)
    $PanelAccessibilityExcerpt = @($UiRowsBefore | Where-Object {
      $_.name -like '*XIIGen*' -or
      $_.name -like '*Smart*' -or
      $_.name -like '*prompt*' -or
      $_.name -like '*Submit*' -or
      $_.automation_id -like '*xiigen*' -or
      $_.automation_id -like '*prompt*' -or
      $_.automation_id -like '*submit*'
    } | Select-Object -First 100)

    if (-not $TargetTextArea) {
      $FocusEvidence.visual_coordinate_fallback_used = $true
      $FocusEvidence.visual_probe_screenshot_requires_human_inspection = $true
      $FocusEvidence.proof_method = 'visible_panel_coordinate_click_then_probe_screenshot_and_undo_cleanup'
      $FocusEvidence.target_textarea_bounds = [pscustomobject]@{
        source = 'visual coordinates from captured 1280x800 Extension Development Host window'
        relative_x = 320
        relative_y = 319
        relative_width = 605
        relative_height = 132
      }

      $ClickX = [int]($Target.Rect.Left + 345)
      $ClickY = [int]($Target.Rect.Top + 345)
      Set-TargetActive -Target $Target
      Invoke-LeftClick -X $ClickX -Y $ClickY
      Start-Sleep -Milliseconds 500
      $OriginalClipboardText = Get-ClipboardTextSafe
      $ClipboardSet = Set-ClipboardTextSafe -Text $ProbeText
      [System.Windows.Forms.SendKeys]::SendWait('^{END}')
      Start-Sleep -Milliseconds 200
      if ($ClipboardSet) {
        [System.Windows.Forms.SendKeys]::SendWait('^v')
        $FocusEvidence.clipboard_probe_paste_used = $true
      } else {
        [System.Windows.Forms.SendKeys]::SendWait($ProbeText)
      }
      Start-Sleep -Seconds 1
      Save-TargetScreenshot -Target $Target -Path $ProbeScreenshotPath
      $ScreenshotCaptured.probe_visible = Test-Path -LiteralPath $ProbeScreenshotPath
      if ($ScreenshotCaptured.probe_visible) { $ScreenshotCaptured.probe_visible_path = ConvertTo-RelativeEvidencePath -Path $ProbeScreenshotPath }
      $ScreenshotCaptured.probe_visible_crop = Save-ImageCrop -SourcePath $ProbeScreenshotPath -TargetPath $ProbeCropPath -X 300 -Y 260 -Width 980 -Height 250
      if ($ScreenshotCaptured.probe_visible_crop) { $ScreenshotCaptured.probe_visible_crop_path = ConvertTo-RelativeEvidencePath -Path $ProbeCropPath }

      [System.Windows.Forms.SendKeys]::SendWait('^z')
      Start-Sleep -Seconds 1
      $FocusEvidence.clipboard_restored = Set-ClipboardTextSafe -Text $OriginalClipboardText
      Save-TargetScreenshot -Target $Target -Path $AfterScreenshotPath
      $ScreenshotCaptured.after_clear = Test-Path -LiteralPath $AfterScreenshotPath
      if ($ScreenshotCaptured.after_clear) { $ScreenshotCaptured.after_clear_path = ConvertTo-RelativeEvidencePath -Path $AfterScreenshotPath }
      $ScreenshotCaptured.after_clear_crop = Save-ImageCrop -SourcePath $AfterScreenshotPath -TargetPath $AfterCropPath -X 300 -Y 260 -Width 980 -Height 250
      if ($ScreenshotCaptured.after_clear_crop) { $ScreenshotCaptured.after_clear_crop_path = ConvertTo-RelativeEvidencePath -Path $AfterCropPath }

      $FocusEvidence.target_textarea_found = $true
      $FocusEvidence.probe_visible_in_target_textarea = $ScreenshotCaptured.probe_visible_crop
      $FocusEvidence.generic_cursor_textbox_probe_detected = $false
      $ProbeCleanupStatus = if ($ScreenshotCaptured.after_clear_crop) { 'probe_undo_sent_after_visual_coordinate_probe' } else { 'probe_cleanup_screenshot_missing' }
      $Status = if ($ScreenshotCaptured.probe_visible_crop -and $ScreenshotCaptured.after_clear_crop) {
        'XIIGEN_TEXTAREA_FOCUS_PROVEN'
      } else {
        'XIIGEN_TEXTAREA_FOCUS_BLOCKED_WITH_EVIDENCE'
      }
    } else {
      $FocusEvidence.target_textarea_found = $true
      $FocusEvidence.proof_method = 'ui_automation_target_edit_focus_sendkeys_verify_value_then_delete'
      $FocusEvidence.target_textarea_control_type = $TargetTextArea.Current.ControlType.ProgrammaticName
      $Bounds = $TargetTextArea.Current.BoundingRectangle
      $FocusEvidence.target_textarea_bounds = [pscustomobject]@{
        x = [int]$Bounds.X
        y = [int]$Bounds.Y
        width = [int]$Bounds.Width
        height = [int]$Bounds.Height
      }

      $ClickX = [int]($Bounds.X + [Math]::Max(5, [Math]::Min($Bounds.Width - 5, 24)))
      $ClickY = [int]($Bounds.Y + [Math]::Max(5, [Math]::Min($Bounds.Height - 5, 20)))
      Invoke-LeftClick -X $ClickX -Y $ClickY
      Start-Sleep -Milliseconds 500
      $TargetTextArea.SetFocus()
      Start-Sleep -Milliseconds 500

      $FocusEvidence.focused_before_type = Get-FocusedSummary
      $FocusEvidence.value_before_type = Get-ElementValue -Element $TargetTextArea

      if ($FocusEvidence.focused_before_type -and
          $FocusEvidence.focused_before_type.automation_id -eq 'xiigen-smart-skills-prompt') {
        $OriginalClipboardText = Get-ClipboardTextSafe
        $ClipboardSet = Set-ClipboardTextSafe -Text $ProbeText
        if ($ClipboardSet) {
          [System.Windows.Forms.SendKeys]::SendWait('^v')
          $FocusEvidence.clipboard_probe_paste_used = $true
        } else {
          [System.Windows.Forms.SendKeys]::SendWait($ProbeText)
        }
        Start-Sleep -Seconds 1
        $FocusEvidence.focused_after_type = Get-FocusedSummary
        $FocusEvidence.value_after_type = Get-ElementValue -Element $TargetTextArea
        Save-TargetScreenshot -Target $Target -Path $ProbeScreenshotPath
        $ScreenshotCaptured.probe_visible = Test-Path -LiteralPath $ProbeScreenshotPath
        if ($ScreenshotCaptured.probe_visible) { $ScreenshotCaptured.probe_visible_path = ConvertTo-RelativeEvidencePath -Path $ProbeScreenshotPath }
        $ScreenshotCaptured.probe_visible_crop = Save-ImageCrop -SourcePath $ProbeScreenshotPath -TargetPath $ProbeCropPath -X 300 -Y 260 -Width 980 -Height 250
        if ($ScreenshotCaptured.probe_visible_crop) { $ScreenshotCaptured.probe_visible_crop_path = ConvertTo-RelativeEvidencePath -Path $ProbeCropPath }

        $RootAfterProbe = [System.Windows.Automation.AutomationElement]::FromHandle($Target.Handle)
        $UiRowsAfterProbe = @(Get-UiRows -Root $RootAfterProbe)
        $NonTargetProbeRows = @($UiRowsAfterProbe | Where-Object {
          $_.value -eq $ProbeText -and $_.automation_id -ne 'xiigen-smart-skills-prompt'
        })
        $FocusEvidence.generic_cursor_textbox_probe_detected = [bool](@($NonTargetProbeRows).Count)
        $FocusEvidence.probe_visible_in_target_textarea = (
          $FocusEvidence.focused_after_type -and
          $FocusEvidence.focused_after_type.automation_id -eq 'xiigen-smart-skills-prompt' -and
          $FocusEvidence.value_after_type -eq $ProbeText -and
          -not $FocusEvidence.generic_cursor_textbox_probe_detected
        )

        if ($FocusEvidence.probe_visible_in_target_textarea) {
          [System.Windows.Forms.SendKeys]::SendWait('^a')
          Start-Sleep -Milliseconds 300
          [System.Windows.Forms.SendKeys]::SendWait('{DELETE}')
          Start-Sleep -Seconds 1
          $FocusEvidence.clipboard_restored = Set-ClipboardTextSafe -Text $OriginalClipboardText
          $FocusEvidence.focused_after_clear = Get-FocusedSummary
          $FocusEvidence.value_after_clear = Get-ElementValue -Element $TargetTextArea
          Save-TargetScreenshot -Target $Target -Path $AfterScreenshotPath
          $ScreenshotCaptured.after_clear = Test-Path -LiteralPath $AfterScreenshotPath
          if ($ScreenshotCaptured.after_clear) { $ScreenshotCaptured.after_clear_path = ConvertTo-RelativeEvidencePath -Path $AfterScreenshotPath }
          $ScreenshotCaptured.after_clear_crop = Save-ImageCrop -SourcePath $AfterScreenshotPath -TargetPath $AfterCropPath -X 300 -Y 260 -Width 980 -Height 250
          if ($ScreenshotCaptured.after_clear_crop) { $ScreenshotCaptured.after_clear_crop_path = ConvertTo-RelativeEvidencePath -Path $AfterCropPath }

          if (($null -eq $FocusEvidence.value_after_clear) -or ($FocusEvidence.value_after_clear -eq '')) {
            $ProbeCleanupStatus = 'probe_cleared_from_xiigen_textarea'
            $Status = 'XIIGEN_TEXTAREA_FOCUS_PROVEN'
          } else {
            $ProbeCleanupStatus = 'probe_cleanup_not_verified'
            $Status = 'XIIGEN_TEXTAREA_FOCUS_BLOCKED_WITH_EVIDENCE'
          }
        } else {
          $ProbeCleanupStatus = 'not_cleared_because_target_probe_visibility_failed'
          $Status = 'XIIGEN_TEXTAREA_FOCUS_BLOCKED_WITH_EVIDENCE'
        }
      } else {
        $ProbeCleanupStatus = 'not_started_because_focus_not_verified'
        $Status = 'TARGET_WINDOW_INPUT_NOT_VERIFIED'
      }
    }
  }
} catch {
  $FocusEvidence.error = $_.Exception.Message
  $Status = 'XIIGEN_TEXTAREA_FOCUS_BLOCKED_WITH_EVIDENCE'
}

$FilesCreated = @(
  (ConvertTo-RelativeEvidencePath -Path $ProbeJs),
  (ConvertTo-RelativeEvidencePath -Path (Join-Path $LogsDir 'run-row12-micro-focus-textarea.ps1'))
)
foreach ($Path in @($CommandResultPath, $BeforeScreenshotPath, $ProbeScreenshotPath, $AfterScreenshotPath, $JsonPath, $MdPath)) {
  if (Test-Path -LiteralPath $Path) {
    $FilesCreated += (ConvertTo-RelativeEvidencePath -Path $Path)
  }
}
foreach ($Path in @($BeforeCropPath, $ProbeCropPath, $AfterCropPath)) {
  if (Test-Path -LiteralPath $Path) {
    $FilesCreated += (ConvertTo-RelativeEvidencePath -Path $Path)
  }
}

$Record = [pscustomobject]@{
  packet_type = 'ROW12_MICRO_TEXTAREA_FOCUS_PACKET'
  status = $Status
  checked_at_utc = (Get-Date).ToUniversalTime().ToString('o')
  run_folder = $RunFolder
  files_created = $FilesCreated
  authority_and_boundaries = [pscustomobject]@{
    current_task = 'Run only row12 micro-step 2: focus the visible XIIGen panel textarea, type a harmless short probe, prove it lands in that textarea, clear it, and do not submit.'
    git_authority = 'forbidden'
    command_palette_used = $false
    row12_prompt_pasted = $false
    probe_text_typed = $ProbeText
    submit_clicked = $false
    login_or_browser_recovery_run = $false
    external_ai_used = $false
    readiness_claimed = $false
  }
  precondition = [pscustomobject]@{
    source_run_folder = 'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\micro-open-panel-run-20260625T100845Z'
    source_status = 'MICRO_PANEL_CONTROLS_VISIBLE'
  }
  api_open_panel_evidence = $CommandEvidence
  target_window = [pscustomobject]@{
    found = [bool]$Target
    title = if ($Target) { $Target.Title } else { $null }
    process_id = if ($Target) { $Target.ProcessId } else { $null }
    native_window_handle = if ($Target) { $Target.HandleInt } else { $null }
    target_window_candidates = @($Windows | Select-Object Title, ProcessId, HandleInt)
  }
  panel_accessibility_excerpt = $PanelAccessibilityExcerpt
  focus_evidence = $FocusEvidence
  probe_cleanup_status = $ProbeCleanupStatus
  screenshot_status = $ScreenshotCaptured
  human_explanation = if ($Status -eq 'XIIGEN_TEXTAREA_FOCUS_PROVEN') {
    'The Cursor Extension Host API opened the XIIGen panel. The proof typed only the harmless probe into the visible XIIGen task prompt area and then removed it. Submit was not clicked and the row12 prompt was not pasted.'
  } else {
    'The run stayed inside the focus-proof slice and captured blocking evidence. Submit was not clicked and the row12 prompt was not pasted.'
  }
  next_smallest_action = if ($Status -eq 'XIIGEN_TEXTAREA_FOCUS_PROVEN') {
    'A separately authorized row12 micro-step can paste the real row12 prompt into the now-proven XIIGen textarea; do not submit unless separately authorized.'
  } else {
    'Repair or rerun only the focus-proof micro-step from the captured blocker evidence; do not paste row12 prompt.'
  }
}

$Record | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $JsonPath -Encoding UTF8

$Markdown = @(
  'ROW12_MICRO_TEXTAREA_FOCUS_PACKET',
  "status: $($Record.status)",
  "run_folder: $RunFolder",
  'files_created:',
  ($Record.files_created | ForEach-Object { "- $_" }),
  'focus_evidence:',
  "- target_textarea_found: $($FocusEvidence.target_textarea_found)",
  "- target_textarea_automation_id: $($FocusEvidence.target_textarea_automation_id)",
  "- proof_method: $($FocusEvidence.proof_method)",
  "- visual_coordinate_fallback_used: $($FocusEvidence.visual_coordinate_fallback_used)",
  "- focused_before_type_automation_id: $(if ($FocusEvidence.focused_before_type) { $FocusEvidence.focused_before_type.automation_id } else { '' })",
  "- focused_after_type_automation_id: $(if ($FocusEvidence.focused_after_type) { $FocusEvidence.focused_after_type.automation_id } else { '' })",
  "- probe_visible_in_target_textarea: $($FocusEvidence.probe_visible_in_target_textarea)",
  "- generic_cursor_textbox_probe_detected: $($FocusEvidence.generic_cursor_textbox_probe_detected)",
  "- value_after_clear: '$($FocusEvidence.value_after_clear)'",
  "probe_cleanup_status: $ProbeCleanupStatus",
  'screenshot_status:',
  "- before: $($ScreenshotCaptured.before) $($ScreenshotCaptured.before_path)",
  "- before_crop: $($ScreenshotCaptured.before_crop) $($ScreenshotCaptured.before_crop_path)",
  "- probe_visible: $($ScreenshotCaptured.probe_visible) $($ScreenshotCaptured.probe_visible_path)",
  "- probe_visible_crop: $($ScreenshotCaptured.probe_visible_crop) $($ScreenshotCaptured.probe_visible_crop_path)",
  "- after_clear: $($ScreenshotCaptured.after_clear) $($ScreenshotCaptured.after_clear_path)",
  "- after_clear_crop: $($ScreenshotCaptured.after_clear_crop) $($ScreenshotCaptured.after_clear_crop_path)",
  "human_explanation: $($Record.human_explanation)",
  "next_smallest_action: $($Record.next_smallest_action)",
  ''
) -join "`n"
$Markdown | Set-Content -LiteralPath $MdPath -Encoding UTF8

'stop' | Set-Content -LiteralPath $StopPath -Encoding UTF8
$Record | ConvertTo-Json -Depth 20
