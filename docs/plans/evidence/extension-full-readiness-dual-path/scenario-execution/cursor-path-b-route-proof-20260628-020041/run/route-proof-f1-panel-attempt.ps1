param(
  [string]$EvidenceRoot = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\cursor-path-b-route-proof-20260628-020041",
  [string]$SearchText = "XIIGen: Open Smart Skills Panel"
)

$ErrorActionPreference = "Stop"
$screenshotsDir = Join-Path $EvidenceRoot "screenshots"
$logsDir = Join-Path $EvidenceRoot "logs"
New-Item -ItemType Directory -Force -Path $screenshotsDir, $logsDir | Out-Null

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

function Capture-PrimaryScreen([string]$path) {
  $bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
  $bitmap = [System.Drawing.Bitmap]::new($bounds.Width, $bounds.Height)
  $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
  $graphics.CopyFromScreen($bounds.Left, $bounds.Top, 0, 0, $bounds.Size)
  $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
  $graphics.Dispose()
  $bitmap.Dispose()
}

function Send-KeyChord([string]$keys, [int]$delayMs = 700) {
  [System.Windows.Forms.SendKeys]::SendWait($keys)
  Start-Sleep -Milliseconds $delayMs
}

$target = Get-Process Cursor | Where-Object {
  $_.MainWindowTitle -like "*[Extension Development Host]*" -and
  $_.MainWindowTitle -like "*cursor-xiigen-smart-skills*"
} | Select-Object -First 1

if (-not $target) { throw "TARGET_WINDOW_NOT_FOUND" }

$shell = New-Object -ComObject WScript.Shell
[void]$shell.AppActivate($target.Id)
Start-Sleep -Milliseconds 900
Capture-PrimaryScreen (Join-Path $screenshotsDir "08-f1-attempt-before.png")

Send-KeyChord "{ESC}" 400
Send-KeyChord "{F1}" 900
Send-KeyChord "^a" 300
[System.Windows.Forms.Clipboard]::SetText($SearchText)
Send-KeyChord "^v" 900
Capture-PrimaryScreen (Join-Path $screenshotsDir "09-f1-attempt-command-palette.png")

Send-KeyChord "{ENTER}" 7000
Capture-PrimaryScreen (Join-Path $screenshotsDir "10-f1-attempt-route-result.png")

@{
  status = "F1_ATTEMPT_COMPLETED"
  targetProcessId = $target.Id
  targetTitleAtStart = $target.MainWindowTitle
  searchText = $SearchText
  scenarioPromptTyped = $false
  scenarioPromptSubmitted = $false
  screenshots = @(
    (Join-Path $screenshotsDir "08-f1-attempt-before.png"),
    (Join-Path $screenshotsDir "09-f1-attempt-command-palette.png"),
    (Join-Path $screenshotsDir "10-f1-attempt-route-result.png")
  )
  finalCursorWindows = @(Get-Process Cursor | Where-Object { $_.MainWindowTitle } | Select-Object Id, ProcessName, MainWindowTitle)
} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $logsDir "f1-attempt-summary.json") -Encoding UTF8

Write-Output "F1_ATTEMPT_COMPLETED"
