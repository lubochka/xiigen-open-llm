Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$EvidenceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ScenarioPrepPath = Join-Path $EvidenceDir "scenario-prep.ru.md"
$Marker = "FRDP-P01-ROW12-CLAUDE-ANCHORED-DUAL-PATH-V1"
$ScenarioId = "frdp-p01-claude-anchored-dual-path"
$PairId = "frdp-p01-row12-claude-anchored-v1"

Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class XiigenWin32 {
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
}
public struct RECT {
  public int Left;
  public int Top;
  public int Right;
  public int Bottom;
}
"@

function Write-JsonFile {
    param(
        [Parameter(Mandatory = $true)]$Value,
        [Parameter(Mandatory = $true)][string]$Path
    )
    $Value | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Write-TextFile {
    param(
        [Parameter(Mandatory = $true)][string]$Value,
        [Parameter(Mandatory = $true)][string]$Path
    )
    Set-Content -LiteralPath $Path -Value $Value -Encoding UTF8
}

function Get-PromptFromPrep {
    $text = Get-Content -Raw -Encoding UTF8 -LiteralPath $ScenarioPrepPath
    $match = [regex]::Match($text, '```text\r?\n([\s\S]*?)\r?\n```')
    if (-not $match.Success) {
        throw "Could not extract prompt from scenario-prep.ru.md"
    }
    return ($match.Groups[1].Value -replace "`r`n?", "`n")
}

function Get-Sha256 {
    param([string]$Text)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
        return ([BitConverter]::ToString($sha.ComputeHash($bytes)) -replace "-", "").ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }
}

function Get-ClaudeWindowProcess {
    return Get-Process |
        Where-Object { $_.ProcessName -match '(?i)^claude$' -and $_.MainWindowTitle -eq 'Claude' -and $_.MainWindowHandle -ne 0 } |
        Sort-Object Id |
        Select-Object -First 1
}

function Get-RootElement {
    param([System.IntPtr]$Handle)
    return [System.Windows.Automation.AutomationElement]::FromHandle($Handle)
}

function Get-WindowRectangle {
    param([System.IntPtr]$Handle)
    $rect = New-Object RECT
    [void][XiigenWin32]::GetWindowRect($Handle, [ref]$rect)
    return $rect
}

function Save-ContentPaneScreenshot {
    param(
        [System.IntPtr]$Handle,
        [string]$Path
    )
    $rect = Get-WindowRectangle -Handle $Handle
    $windowWidth = [Math]::Max(1, $rect.Right - $rect.Left)
    $windowHeight = [Math]::Max(1, $rect.Bottom - $rect.Top)
    $leftInset = [Math]::Min(340, [int]($windowWidth * 0.30))
    $topInset = 50
    $rightInset = 20
    $bottomInset = 20
    $x = $rect.Left + $leftInset
    $y = $rect.Top + $topInset
    $width = [Math]::Max(1, $windowWidth - $leftInset - $rightInset)
    $height = [Math]::Max(1, $windowHeight - $topInset - $bottomInset)
    $bitmap = New-Object System.Drawing.Bitmap($width, $height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.CopyFromScreen($x, $y, 0, 0, (New-Object System.Drawing.Size($width, $height)))
        $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

function Find-ButtonByNameRegex {
    param(
        [System.Windows.Automation.AutomationElement]$Root,
        [string]$Pattern
    )
    $all = $Root.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($element in $all) {
        try {
            if ($element.Current.ControlType.ProgrammaticName -ne "ControlType.Button") {
                continue
            }
            $name = [string]$element.Current.Name
            if ($name -match $Pattern) {
                return $element
            }
        }
        catch {
        }
    }
    return $null
}

function Invoke-Element {
    param([System.Windows.Automation.AutomationElement]$Element)
    $pattern = $null
    if ($Element.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$pattern)) {
        $pattern.Invoke()
        return $true
    }
    return $false
}

function Find-Composer {
    param([System.Windows.Automation.AutomationElement]$Root)
    $all = $Root.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    $candidates = @()
    foreach ($element in $all) {
        try {
            $className = [string]$element.Current.ClassName
            $controlType = [string]$element.Current.ControlType.ProgrammaticName
            if ($className -match "ProseMirror" -or $controlType -eq "ControlType.Edit") {
                $rect = $element.Current.BoundingRectangle
                if ($rect.Width -gt 100 -and $rect.Height -gt 10) {
                    $candidates += [pscustomobject]@{ Element = $element; Bottom = $rect.Bottom; Top = $rect.Top; Left = $rect.Left }
                }
            }
        }
        catch {
        }
    }
    return ($candidates | Sort-Object Bottom -Descending | Select-Object -First 1).Element
}

function Get-VisibleContentPaneText {
    param(
        [System.Windows.Automation.AutomationElement]$Root,
        [RECT]$WindowRect
    )
    $leftBoundary = $WindowRect.Left + [Math]::Min(340, [int](($WindowRect.Right - $WindowRect.Left) * 0.30))
    $all = $Root.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    $lines = New-Object System.Collections.Generic.List[string]
    foreach ($element in $all) {
        try {
            $rect = $element.Current.BoundingRectangle
            if ($rect.X -lt $leftBoundary -or $rect.Width -le 0 -or $rect.Height -le 0) {
                continue
            }
            $name = [string]$element.Current.Name
            if ([string]::IsNullOrWhiteSpace($name)) {
                continue
            }
            $trimmed = $name.Trim()
            if ($trimmed.Length -gt 0 -and $trimmed.Length -lt 2000) {
                $lines.Add($trimmed)
            }
        }
        catch {
        }
    }
    return (($lines | Select-Object -Unique) -join "`n")
}

function Write-Blocker {
    param(
        [string]$Reason,
        [hashtable]$Extra = @{}
    )
    $blocker = [ordered]@{
        schema = "frdp.claude.path_b.blocker.v1"
        path = "B"
        marker = $Marker
        scenario_id = $ScenarioId
        pair_id = $PairId
        blocker = $Reason
        no_secrets_typed = $true
        same_prompt_submitted = $false
        screenshots_captured = 0
        smallest_next_action = "Manually open a blank authenticated Claude chat with no private content visible, then rerun this bounded Path B UI probe."
    }
    foreach ($key in $Extra.Keys) {
        $blocker[$key] = $Extra[$key]
    }
    Write-JsonFile -Value $blocker -Path (Join-Path $EvidenceDir "path-b-blocker.json")
}

$prompt = Get-PromptFromPrep
$promptHash = Get-Sha256 -Text $prompt
$startedUtc = (Get-Date).ToUniversalTime().ToString("o")
$process = Get-ClaudeWindowProcess
if (-not $process) {
    Write-Blocker -Reason "No visible local Claude UI/app window was found at submit time."
    exit 2
}

$handle = [System.IntPtr]$process.MainWindowHandle
[void][XiigenWin32]::ShowWindow($handle, 5)
[void][XiigenWin32]::SetForegroundWindow($handle)
Start-Sleep -Milliseconds 800

$root = Get-RootElement -Handle $handle
$newChatButton = Find-ButtonByNameRegex -Root $root -Pattern "(?i)^\s*(new chat|start new chat|new conversation)\s*$"
$newChatInvoked = $false
if ($newChatButton) {
    $newChatInvoked = Invoke-Element -Element $newChatButton
    Start-Sleep -Milliseconds 1200
}
if (-not $newChatInvoked) {
    Write-Blocker -Reason "Could not invoke a visible New Chat control before submitting; avoiding capture or mutation of an existing conversation." -Extra @{ new_chat_control_found = [bool]$newChatButton }
    exit 3
}

$root = Get-RootElement -Handle $handle
$composer = Find-Composer -Root $root
if (-not $composer) {
    Write-Blocker -Reason "New Chat opened, but no focusable Claude composer/edit control was found." -Extra @{ new_chat_invoked = $newChatInvoked }
    exit 4
}

$beforePath = Join-Path $EvidenceDir "path-b-before.png"
Save-ContentPaneScreenshot -Handle $handle -Path $beforePath

$composer.SetFocus()
Start-Sleep -Milliseconds 300
Set-Clipboard -Value $prompt
[System.Windows.Forms.SendKeys]::SendWait("^v")
Start-Sleep -Milliseconds 1000

$root = Get-RootElement -Handle $handle
$sendButton = Find-ButtonByNameRegex -Root $root -Pattern "(?i)^\s*(send|send message)\s*$"
$sendInvoked = $false
if ($sendButton) {
    $sendInvoked = Invoke-Element -Element $sendButton
}
if (-not $sendInvoked) {
    [System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
    $sendInvoked = $true
}

$afterSubmitPath = Join-Path $EvidenceDir "path-b-after-submit.png"
Start-Sleep -Milliseconds 1500
Save-ContentPaneScreenshot -Handle $handle -Path $afterSubmitPath

$stableRounds = 0
$lastText = ""
for ($i = 0; $i -lt 36; $i++) {
    Start-Sleep -Seconds 2
    $root = Get-RootElement -Handle $handle
    $rect = Get-WindowRectangle -Handle $handle
    $text = Get-VisibleContentPaneText -Root $root -WindowRect $rect
    if ($text -eq $lastText -and $text.Length -gt 0) {
        $stableRounds += 1
    }
    else {
        $stableRounds = 0
        $lastText = $text
    }
    if ($stableRounds -ge 3 -and $text -match [regex]::Escape($Marker)) {
        break
    }
}

$finalPath = Join-Path $EvidenceDir "path-b-final.png"
Save-ContentPaneScreenshot -Handle $handle -Path $finalPath
$visibleTextPath = Join-Path $EvidenceDir "path-b-visible-text.txt"
Write-TextFile -Value $lastText -Path $visibleTextPath

$completedUtc = (Get-Date).ToUniversalTime().ToString("o")
$summary = [ordered]@{
    schema = "frdp.claude.path_b.summary.v1"
    path = "B"
    marker = $Marker
    scenario_id = $ScenarioId
    pair_id = $PairId
    prompt_sha256 = $promptHash
    process_id = $process.Id
    process_path = $process.Path
    window_title = "Claude"
    started_utc = $startedUtc
    completed_utc = $completedUtc
    new_chat_invoked = $newChatInvoked
    composer_found = $true
    clipboard_overwritten_with_safe_prompt = $true
    clipboard_previous_value_read = $false
    send_invoked = $sendInvoked
    screenshots = @($beforePath, $afterSubmitPath, $finalPath)
    visible_text_path = $visibleTextPath
    visible_text_contains_marker = $lastText.Contains($Marker)
    visible_text_contains_scenario_id = $lastText.Contains($ScenarioId)
    visible_text_contains_pair_id = $lastText.Contains($PairId)
    no_secrets_typed = $true
    no_product_readiness_claim = $true
    downgrade = if ($lastText.Contains($Marker) -and $lastText.Contains($ScenarioId) -and $lastText.Contains($PairId)) { $null } else { "Path B submitted prompt, but accessible visible text did not expose all structured anchors; screenshots are the primary UI evidence." }
}

Write-JsonFile -Value $summary -Path (Join-Path $EvidenceDir "path-b-summary.json")

if (-not $summary.visible_text_contains_marker) {
    Write-Blocker -Reason "Path B UI submit ran, but the final accessible text did not expose the marker, so anchored UI proof is downgraded." -Extra @{
        new_chat_invoked = $newChatInvoked
        composer_found = $true
        send_invoked = $sendInvoked
        screenshots_captured = 3
        same_prompt_submitted = $true
        visible_text_path = $visibleTextPath
        prompt_sha256 = $promptHash
    }
    exit 5
}

Write-Output "PATH_B_DONE prompt_sha256=$promptHash"
exit 0
