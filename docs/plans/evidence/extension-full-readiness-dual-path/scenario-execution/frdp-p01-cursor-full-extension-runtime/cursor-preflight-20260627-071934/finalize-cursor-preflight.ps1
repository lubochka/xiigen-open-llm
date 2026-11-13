$ErrorActionPreference = 'Stop'

$RunDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$InitialProcessPath = Join-Path $RunDir 'initial-processes.json'
$TopWindowsPath = Join-Path $RunDir 'top-level-windows.json'
$InitialShot = Join-Path $RunDir '01-cursor-window-initial.png'
$DialogShot = Join-Path $RunDir '02-storage-error-dialog.png'
$RedactedShot = Join-Path $RunDir '03-redacted-session-lock.png'
$SessionFactsPath = Join-Path $RunDir 'session-preflight-facts.json'
$ReportJsonPath = Join-Path $RunDir 'preflight-packet.json'
$ReportMdPath = Join-Path $RunDir 'preflight-packet.md'

Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Text;
using System.Runtime.InteropServices;
public class Win32ForegroundProbe {
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")] public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
  [DllImport("user32.dll")] public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
}
'@

function New-RedactedImage {
  param(
    [Parameter(Mandatory = $true)][string]$Source,
    [Parameter(Mandatory = $true)][string]$Destination
  )

  $bitmap = [System.Drawing.Bitmap]::FromFile($Source)
  try {
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
      $brush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 20, 20, 20))
      $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(245, 245, 245), 2)
      $centerLeft = [Math]::Max(0, [int]($bitmap.Width * 0.10))
      $centerTop = [Math]::Max(0, [int]($bitmap.Height * 0.20))
      $centerWidth = [Math]::Min([int]($bitmap.Width * 0.80), $bitmap.Width - $centerLeft)
      $centerHeight = [Math]::Min([int]($bitmap.Height * 0.78), $bitmap.Height - $centerTop)
      $topWidth = [Math]::Min(260, $bitmap.Width)
      $topHeight = [Math]::Min(42, $bitmap.Height)
      $rects = @(
        [System.Drawing.Rectangle]::new($centerLeft, $centerTop, $centerWidth, $centerHeight),
        [System.Drawing.Rectangle]::new(0, 0, $topWidth, $topHeight)
      )
      foreach ($rect in $rects) {
        $graphics.FillRectangle($brush, $rect)
        $graphics.DrawRectangle($pen, $rect)
      }
    }
    finally {
      if ($brush) { $brush.Dispose() }
      if ($pen) { $pen.Dispose() }
      $graphics.Dispose()
    }
    $savePath = $Destination
    if ([System.IO.Path]::GetFullPath($Source) -eq [System.IO.Path]::GetFullPath($Destination)) {
      $savePath = "$Destination.tmp.png"
    }
    $bitmap.Save($savePath, [System.Drawing.Imaging.ImageFormat]::Png)
  }
  finally {
    $bitmap.Dispose()
  }
  if ($savePath -ne $Destination) {
    Move-Item -LiteralPath $savePath -Destination $Destination -Force
  }
}

function Get-ForegroundFact {
  $hwnd = [Win32ForegroundProbe]::GetForegroundWindow()
  $titleBuilder = New-Object System.Text.StringBuilder 2048
  [void][Win32ForegroundProbe]::GetWindowText($hwnd, $titleBuilder, $titleBuilder.Capacity)
  $classBuilder = New-Object System.Text.StringBuilder 256
  [void][Win32ForegroundProbe]::GetClassName($hwnd, $classBuilder, $classBuilder.Capacity)
  $processId = 0
  [void][Win32ForegroundProbe]::GetWindowThreadProcessId($hwnd, [ref]$processId)
  $process = $null
  try { $process = Get-Process -Id $processId -ErrorAction Stop } catch {}

  [pscustomobject]@{
    handle = ('0x{0:X}' -f $hwnd.ToInt64())
    handleDecimal = $hwnd.ToInt64()
    title = $titleBuilder.ToString()
    class = $classBuilder.ToString()
    processId = $processId
    processName = if ($process) { $process.ProcessName } else { $null }
  }
}

New-RedactedImage -Source $InitialShot -Destination $RedactedShot
New-RedactedImage -Source $InitialShot -Destination $InitialShot
New-RedactedImage -Source $DialogShot -Destination $DialogShot

$topWindows = Get-Content -LiteralPath $TopWindowsPath -Raw | ConvertFrom-Json
$initialProcesses = Get-Content -LiteralPath $InitialProcessPath -Raw | ConvertFrom-Json
$foreground = Get-ForegroundFact
$logonProcesses = Get-Process -Name LogonUI,LockApp -ErrorAction SilentlyContinue |
  Select-Object Id, ProcessName, MainWindowTitle, MainWindowHandle
$queryUserText = try { (& query user) -join "`n" } catch { "query user failed: $($_.Exception.Message)" }

$cursorWindow = $topWindows.windows |
  Where-Object { $_.title -eq '[Extension Development Host] cursor-xiigen-smart-skills - Cursor' } |
  Select-Object -First 1
$storageDialog = $topWindows.windows |
  Where-Object { $_.title -eq 'Storage Error' -and $_.processName -eq 'Cursor' } |
  Select-Object -First 1

$sessionFacts = [pscustomobject]@{
  timestamp = (Get-Date).ToString('o')
  visible_screen_observation = 'Cursor crop and dialog crop show Windows PIN entry / lock screen, not Cursor UI content.'
  no_secret_policy = 'No credentials typed. Screenshots redacted for user-identifying lock-screen text.'
  foreground = $foreground
  logon_or_lock_processes = @($logonProcesses)
  query_user = $queryUserText
}
$sessionFacts | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $SessionFactsPath -Encoding UTF8

$changedFiles = @(
  'initial-processes.json',
  'top-level-windows.json',
  '01-cursor-window-initial.png',
  '02-storage-error-dialog.png',
  '03-redacted-session-lock.png',
  'session-preflight-facts.json',
  'preflight-packet.json',
  'preflight-packet.md',
  'finalize-cursor-preflight.ps1'
)

$packet = [pscustomobject]@{
  human_status = [pscustomobject]@{
    total = 1
    done = 1
    in_progress = 0
    remaining = 0
    meaning = 'Cursor real-program UI preflight attempted. Window discovery and target bounds succeeded; route discovery was blocked before prompt submission because the local desktop screenshot showed Windows PIN entry.'
  }
  windows_found = [pscustomobject]@{
    cursor_target = $cursorWindow
    cursor_dialog = $storageDialog
    monitors = $topWindows.monitors
    cursor_processes = $initialProcesses
  }
  session_state = [pscustomobject]@{
    appears_logged_in_or_anonymous = 'BLOCKED_LOGIN_REQUIRED: local Windows desktop is at PIN entry; Cursor app account/session could not be inspected.'
    redacted_screenshot = $RedactedShot
    supporting_facts = $SessionFactsPath
  }
  route_attempt = [pscustomobject]@{
    attempted = 'Safe focus and screenshot only.'
    exact_route = 'Identified Cursor extension development host window, called ShowWindow/SetForegroundWindow on its handle, captured the target bounds. Did not press Ctrl+Shift+X, type search text, submit prompts, or dismiss dialogs because the visible surface was Windows PIN entry.'
    screenshots = @($InitialShot, $DialogShot, $RedactedShot)
  }
  verdict = 'BLOCKED_LOGIN_REQUIRED'
  next_repair = 'After Luba unlocks the Windows session, run a fresh bounded preflight that focuses the same Cursor handle/title, dismisses only non-secret blocking dialogs if visible, opens Extensions via Ctrl+Shift+X or Command Palette route discovery, searches XIIGen without prompt submission, and captures redacted screenshots.'
  changed_evidence_files = $changedFiles | ForEach-Object { Join-Path $RunDir $_ }
  evidence_boundary = 'Marker-only/window-only is not extension readiness. This run proves window discovery and a local login/session block only.'
}
$packet | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $ReportJsonPath -Encoding UTF8

$cursorBounds = $cursorWindow.bounds
$dialogBounds = $storageDialog.bounds
$changedFilesMd = ($changedFiles | ForEach-Object { "- $(Join-Path $RunDir $_)" }) -join [Environment]::NewLine
$md = @(
  '# Cursor Real-Program UI Preflight Packet'
  ''
  '## Human-readable status'
  ''
  'Total steps: 1'
  'Done: 1'
  'In progress: 0'
  'Remaining: 0'
  ''
  'The Cursor window was found and its monitor/bounds were captured. The UI route to XIIGen was not attempted beyond safe focus and screenshots because the visible local desktop was at Windows PIN entry, so typing or prompt submission would be unsafe and impossible to verify.'
  ''
  '## Exact windows found'
  ''
  "- Cursor target: title ""$($cursorWindow.title)""; process ""$($cursorWindow.processName)""; handle ""$($cursorWindow.handle)"" / $($cursorWindow.handleDecimal); bounds left $($cursorBounds.left), top $($cursorBounds.top), right $($cursorBounds.right), bottom $($cursorBounds.bottom), width $($cursorBounds.width), height $($cursorBounds.height)."
  "- Cursor dialog: title ""$($storageDialog.title)""; process ""$($storageDialog.processName)""; handle ""$($storageDialog.handle)"" / $($storageDialog.handleDecimal); bounds left $($dialogBounds.left), top $($dialogBounds.top), right $($dialogBounds.right), bottom $($dialogBounds.bottom), width $($dialogBounds.width), height $($dialogBounds.height)."
  '- Monitor \\.\DISPLAY1: non-primary; bounds left -1920, top 0, width 1280, height 720.'
  '- Monitor \\.\DISPLAY2: primary; bounds left 0, top 0, width 1920, height 1080.'
  ''
  '## Session observation'
  ''
  'Cursor app login state could not be inspected. The local desktop appeared at Windows PIN entry. No credentials were typed and no secrets were captured intentionally.'
  ''
  "Redacted screenshot: $RedactedShot"
  ''
  '## XIIGen route attempted'
  ''
  'Attempted route: identify Cursor extension-development-host window, focus it by handle, capture target-window screenshots.'
  ''
  'Not attempted: Ctrl+Shift+X, extension search typing, command-palette route, chat prompt entry, or dialog dismissal. Reason: visible surface was Windows PIN entry, so the real Cursor UI was not available.'
  ''
  'Screenshots:'
  "- $InitialShot"
  "- $DialogShot"
  "- $RedactedShot"
  ''
  '## Verdict'
  ''
  'BLOCKED_LOGIN_REQUIRED'
  ''
  '## Concrete next repair'
  ''
  'After Luba unlocks the Windows session, run a new bounded preflight on the same target: focus [Extension Development Host] cursor-xiigen-smart-skills - Cursor, confirm the visible Cursor UI, dismiss only non-secret blocking dialogs if present, open Extensions with Ctrl+Shift+X or route through Command Palette, search for XIIGen without submitting a prompt, and capture redacted route screenshots.'
  ''
  '## Changed evidence files'
  ''
  $changedFilesMd
  ''
  '## Evidence boundary'
  ''
  'Marker-only/window-only is not extension readiness. The evidence supports only window discovery plus local login/session block.'
) -join [Environment]::NewLine
$md | Set-Content -LiteralPath $ReportMdPath -Encoding UTF8

Write-Output ($packet | ConvertTo-Json -Depth 12)
