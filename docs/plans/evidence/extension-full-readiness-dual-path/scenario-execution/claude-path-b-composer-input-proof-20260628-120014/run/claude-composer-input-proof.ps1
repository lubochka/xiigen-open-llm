param(
    [string]$RepoRoot = "C:\Projects\llm_mvp_core",
    [string]$ScenarioPath = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-prep\claude-full-extension-runtime-20260627-223159\scenario.json",
    [string]$PreflightPath = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\claude-path-b-route-preflight-20260628-112625\result.json"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class ClaudeComposerWin32 {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
    [DllImport("user32.dll", SetLastError=true)] public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    public const int SW_RESTORE = 9;
    public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    public const uint MOUSEEVENTF_LEFTUP = 0x0004;
    public const uint INPUT_KEYBOARD = 1;
    public const uint KEYEVENTF_KEYUP = 0x0002;
    public const uint KEYEVENTF_UNICODE = 0x0004;
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
    [StructLayout(LayoutKind.Sequential)] public struct INPUT { public uint type; public InputUnion U; }
    [StructLayout(LayoutKind.Explicit)] public struct InputUnion { [FieldOffset(0)] public KEYBDINPUT ki; }
    [StructLayout(LayoutKind.Sequential)] public struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public UIntPtr dwExtraInfo; }
}
"@

$EvidenceDir = Split-Path -Parent $PSScriptRoot
$LogsDir = Join-Path $EvidenceDir "logs"
New-Item -ItemType Directory -Force -Path $LogsDir | Out-Null

function Get-Sha256Hex {
    param([AllowNull()] [string]$Text)
    if ($null -eq $Text) { $Text = "" }
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
        return ([System.BitConverter]::ToString($sha.ComputeHash($bytes))).Replace("-", "").ToLowerInvariant()
    } finally { $sha.Dispose() }
}

function Write-JsonFile {
    param([Parameter(Mandatory=$true)] $Value, [Parameter(Mandatory=$true)] [string]$Path)
    $json = $Value | ConvertTo-Json -Depth 20
    [System.IO.File]::WriteAllText($Path, $json + [Environment]::NewLine, [System.Text.Encoding]::UTF8)
}

function Get-BoundsObject {
    param($Current)
    $rect = $Current.BoundingRectangle
    if ($rect.IsEmpty) { return $null }
    return [ordered]@{
        x = [int][Math]::Round($rect.X)
        y = [int][Math]::Round($rect.Y)
        width = [int][Math]::Round($rect.Width)
        height = [int][Math]::Round($rect.Height)
        left = [int][Math]::Round($rect.Left)
        top = [int][Math]::Round($rect.Top)
        right = [int][Math]::Round($rect.Right)
        bottom = [int][Math]::Round($rect.Bottom)
    }
}

function Get-PatternText {
    param($Element)
    $items = New-Object "System.Collections.Generic.List[object]"
    try {
        $vp = $Element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        if ($null -ne $vp) { $items.Add([ordered]@{ source = "ValuePattern.Value"; text = [string]$vp.Current.Value }) | Out-Null }
    } catch {}
    try {
        $tp = $Element.GetCurrentPattern([System.Windows.Automation.TextPattern]::Pattern)
        if ($null -ne $tp) { $items.Add([ordered]@{ source = "TextPattern.DocumentRange"; text = [string]$tp.DocumentRange.GetText(-1) }) | Out-Null }
    } catch {}
    try {
        $name = [string]$Element.Current.Name
        if (-not [string]::IsNullOrWhiteSpace($name)) { $items.Add([ordered]@{ source = "Current.Name"; text = $name }) | Out-Null }
    } catch {}
    return @($items)
}

function Get-RedactedUiaSummary {
    param([Parameter(Mandatory=$true)] $RootElement, [Parameter(Mandatory=$true)] [string]$Prompt)
    $promptPrefix = if ($Prompt.Length -gt 40) { $Prompt.Substring(0, 40) } else { $Prompt }
    $categories = New-Object "System.Collections.Generic.HashSet[string]"
    $safeHits = New-Object "System.Collections.Generic.List[object]"
    $inputCandidates = New-Object "System.Collections.Generic.List[object]"
    $routeCandidates = New-Object "System.Collections.Generic.List[object]"
    $promptMatches = New-Object "System.Collections.Generic.List[object]"
    $sendCandidates = New-Object "System.Collections.Generic.List[object]"
    $errorMessage = $null
    $elementErrors = New-Object "System.Collections.Generic.List[object]"
    $descendantCount = 0

    try {
        $all = $RootElement.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
        $descendantCount = $all.Count
        $limit = [Math]::Min($all.Count, 6000)
        for ($i = 0; $i -lt $limit; $i++) {
            try {
            $element = $all.Item($i)
            $current = $element.Current
            $name = [string]$current.Name
            $controlType = $current.ControlType.ProgrammaticName
            $bounds = Get-BoundsObject -Current $current
            $focusable = [bool]$current.IsKeyboardFocusable
            $enabled = [bool]$current.IsEnabled
            $offscreen = [bool]$current.IsOffscreen

            $category = $null
            if ($name -match "(?i)claude") { $category = "claude_label_visible" }
            if ($name -match "(?i)chat|message|ask") { $category = "chat_surface_visible" }
            if ($name -match "(?i)xiigen|smart\s*skills") { $category = "xiigen_route_visible" }
            if ($name -match "(?i)log\s*in|sign\s*in|password|passcode|pin|credential") { $category = "credential_prompt_visible" }
            if ($null -ne $category) {
                [void]$categories.Add($category)
                $safeLabel = "[redacted UI label; category-only match]"
                if ($category -eq "xiigen_route_visible" -and $name.Length -le 140 -and $name -notmatch "[@\\/:]") { $safeLabel = $name }
                $safeHits.Add([ordered]@{
                    category = $category
                    control_type = $controlType
                    safe_label = $safeLabel
                    focusable = $focusable
                    enabled = $enabled
                    offscreen = $offscreen
                    bounds = $bounds
                }) | Out-Null
            }

            foreach ($textItem in (Get-PatternText -Element $element)) {
                $text = [string]$textItem.text
                if ([string]::IsNullOrEmpty($text)) { continue }
                $containsExact = $text.Contains($Prompt)
                $containsPrefix = (-not [string]::IsNullOrEmpty($promptPrefix)) -and $text.Contains($promptPrefix)
                if ($containsExact -or $containsPrefix) {
                    $promptMatches.Add([ordered]@{
                        element_index = $i
                        source = $textItem.source
                        control_type = $controlType
                        focusable = $focusable
                        enabled = $enabled
                        offscreen = $offscreen
                        bounds = $bounds
                        observed_text_length_chars = $text.Length
                        observed_text_sha256 = Get-Sha256Hex -Text $text
                        contains_exact_prompt = $containsExact
                        contains_prompt_prefix = $containsPrefix
                        raw_text_written = $false
                    }) | Out-Null
                }
            }

            $looksLikeComposer = (
                $controlType -eq "ControlType.Edit" -or
                ($focusable -and -not $offscreen -and $enabled -and $bounds -ne $null -and $bounds.width -gt 160 -and $bounds.height -gt 20 -and $bounds.top -gt 300)
            )
            if ($looksLikeComposer) {
                $hasValuePattern = $false; $valueReadonly = $null; $hasTextPattern = $false
                try { $vp = $element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern); $hasValuePattern = ($null -ne $vp); if ($hasValuePattern) { $valueReadonly = [bool]$vp.Current.IsReadOnly } } catch {}
                try { $tp = $element.GetCurrentPattern([System.Windows.Automation.TextPattern]::Pattern); $hasTextPattern = ($null -ne $tp) } catch {}
                $score = 0
                if ($controlType -eq "ControlType.Edit") { $score += 8 }
                if ($hasValuePattern -and $valueReadonly -eq $false) { $score += 8 }
                if ($hasTextPattern) { $score += 5 }
                if ($focusable) { $score += 4 }
                if ($bounds.top -gt 600) { $score += 5 }
                if ($bounds.width -gt 300) { $score += 2 }
                $inputCandidates.Add([ordered]@{
                    score = $score
                    index = $i
                    control_type = $controlType
                    focusable = $focusable
                    enabled = $enabled
                    offscreen = $offscreen
                    has_value_pattern = $hasValuePattern
                    value_is_readonly = $valueReadonly
                    has_text_pattern = $hasTextPattern
                    safe_label_category = if ([string]::IsNullOrWhiteSpace($name)) { "empty_label" } else { "redacted_nonempty_label" }
                    bounds = $bounds
                }) | Out-Null
            }

            if ($controlType -eq "ControlType.Button" -and $enabled -and -not $offscreen -and $name -match "(?i)^send$|send message|submit") {
                $sendCandidates.Add([ordered]@{ index = $i; control_type = $controlType; safe_label_category = "generic_send_button_label"; bounds = $bounds }) | Out-Null
            }
            if ($controlType -eq "ControlType.Button" -and $enabled -and -not $offscreen -and $name -match "(?i)xiigen|smart\s*skills") {
                $routeCandidates.Add([ordered]@{ index = $i; control_type = $controlType; safe_label = if ($name.Length -le 140 -and $name -notmatch "[@\\/:]") { $name } else { "[redacted XIIGen route label]" }; bounds = $bounds }) | Out-Null
            }
            } catch {
                if ($elementErrors.Count -lt 20) {
                    $elementErrors.Add([ordered]@{
                        element_index = $i
                        error = $_.Exception.Message
                    }) | Out-Null
                }
                continue
            }
        }
    } catch { $errorMessage = $_.Exception.Message }

    $promptMatchRows = @()
    $fullPromptMatchCount = 0
    $prefixPromptMatchCount = 0
    for ($mi = 0; $mi -lt $promptMatches.Count; $mi++) {
        $match = $promptMatches[$mi]
        if ($promptMatchRows.Count -lt 10) {
            $promptMatchRows += $match
        }
        if ([bool]$match.contains_exact_prompt) { $fullPromptMatchCount++ }
        if ([bool]$match.contains_prompt_prefix) { $prefixPromptMatchCount++ }
    }

    return [ordered]@{
        descendant_count = $descendantCount
        categories = @($categories.ToArray())
        safe_hits = @($safeHits.ToArray() | Select-Object -First 80)
        input_candidates = @($inputCandidates.ToArray() | Sort-Object -Property score -Descending | Select-Object -First 20)
        route_candidates = @($routeCandidates.ToArray() | Select-Object -First 20)
        send_candidates = @($sendCandidates.ToArray() | Select-Object -First 20)
        prompt_confirmation_matches = @($promptMatchRows)
        prompt_confirmed_by_redacted_uia = ($fullPromptMatchCount -gt 0)
        prompt_prefix_seen_without_full_match = ($prefixPromptMatchCount -gt 0)
        raw_private_text_written = $false
        skipped_element_errors = @($elementErrors.ToArray())
        error = $errorMessage
    }
}

function Find-XiigenRouteButton {
    param([Parameter(Mandatory=$true)] $RootElement)
    $all = $RootElement.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    $candidates = New-Object "System.Collections.Generic.List[object]"
    $limit = [Math]::Min($all.Count, 6000)
    for ($i = 0; $i -lt $limit; $i++) {
        $element = $all.Item($i); $current = $element.Current; $name = [string]$current.Name
        if ([string]::IsNullOrWhiteSpace($name) -or $name -notmatch "(?i)xiigen|smart\s*skills") { continue }
        if ($current.ControlType.ProgrammaticName -ne "ControlType.Button" -or -not $current.IsEnabled -or $current.IsOffscreen) { continue }
        $bounds = Get-BoundsObject -Current $current; if ($null -eq $bounds) { continue }
        $invoke = $null; $hasInvoke = $false
        try { $invoke = $element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern); $hasInvoke = ($null -ne $invoke) } catch {}
        $score = 0
        if ($name -match "(?i)XIIGen Smart Skills UI proof run") { $score += 20 }
        if ($name -match "(?i)^Idle\s+") { $score += 5 }
        if ($hasInvoke) { $score += 3 }
        $candidates.Add([ordered]@{ score = $score; element = $element; invoke_pattern = $invoke; has_invoke_pattern = $hasInvoke; safe_label = if ($name.Length -le 140 -and $name -notmatch "[@\\/:]") { $name } else { "[redacted XIIGen route label]" }; bounds = $bounds }) | Out-Null
    }
    return @($candidates | Sort-Object -Property score -Descending | Select-Object -First 1)[0]
}

function Find-ComposerElement {
    param([Parameter(Mandatory=$true)] $RootElement)
    $all = $RootElement.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    $candidates = New-Object "System.Collections.Generic.List[object]"
    $limit = [Math]::Min($all.Count, 6000)
    for ($i = 0; $i -lt $limit; $i++) {
        $element = $all.Item($i); $current = $element.Current; $bounds = Get-BoundsObject -Current $current
        if (-not $current.IsEnabled -or $current.IsOffscreen -or $null -eq $bounds -or $bounds.width -lt 160 -or $bounds.height -lt 20 -or $bounds.top -lt 300) { continue }
        $focusable = [bool]$current.IsKeyboardFocusable; $controlType = $current.ControlType.ProgrammaticName
        $name = [string]$current.Name
        $hasValue = $false; $valuePattern = $null; $readonly = $null; $hasText = $false
        try { $valuePattern = $element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern); $hasValue = ($null -ne $valuePattern); if ($hasValue) { $readonly = [bool]$valuePattern.Current.IsReadOnly } } catch {}
        try { $textPattern = $element.GetCurrentPattern([System.Windows.Automation.TextPattern]::Pattern); $hasText = ($null -ne $textPattern) } catch {}
        $looks = $controlType -eq "ControlType.Edit" -or $focusable -or $hasValue -or $hasText -or ($name -match "(?i)message|ask|composer|prompt")
        if (-not $looks) { continue }
        $score = 0
        if ($controlType -eq "ControlType.Edit") { $score += 8 }
        if ($hasValue -and $readonly -eq $false) { $score += 8 }
        if ($hasText) { $score += 5 }
        if ($focusable) { $score += 4 }
        if ($bounds.top -gt 600) { $score += 8 }
        if ($bounds.width -gt 300) { $score += 2 }
        $candidates.Add([ordered]@{ score = $score; element = $element; control_type = $controlType; bounds = $bounds; has_value_pattern = $hasValue; value_pattern = $valuePattern; value_is_readonly = $readonly; has_text_pattern = $hasText; focusable = $focusable }) | Out-Null
    }
    return @($candidates | Sort-Object -Property score -Descending | Select-Object -First 1)[0]
}

function Send-UnicodeText {
    param([Parameter(Mandatory=$true)] [string]$Text)
    $sent = 0
    foreach ($ch in $Text.ToCharArray()) {
        $down = New-Object ClaudeComposerWin32+INPUT
        $down.type = [ClaudeComposerWin32]::INPUT_KEYBOARD
        $down.U.ki.wVk = 0
        $down.U.ki.wScan = [uint16][char]$ch
        $down.U.ki.dwFlags = [ClaudeComposerWin32]::KEYEVENTF_UNICODE
        $down.U.ki.time = 0
        $down.U.ki.dwExtraInfo = [UIntPtr]::Zero
        $up = New-Object ClaudeComposerWin32+INPUT
        $up.type = [ClaudeComposerWin32]::INPUT_KEYBOARD
        $up.U.ki.wVk = 0
        $up.U.ki.wScan = [uint16][char]$ch
        $up.U.ki.dwFlags = [ClaudeComposerWin32]::KEYEVENTF_UNICODE -bor [ClaudeComposerWin32]::KEYEVENTF_KEYUP
        $up.U.ki.time = 0
        $up.U.ki.dwExtraInfo = [UIntPtr]::Zero
        $arr = [ClaudeComposerWin32+INPUT[]]@($down, $up)
        [void][ClaudeComposerWin32]::SendInput(2, $arr, [System.Runtime.InteropServices.Marshal]::SizeOf([ClaudeComposerWin32+INPUT]))
        $sent++
        Start-Sleep -Milliseconds 2
    }
    return $sent
}

function Click-BoundsCenter {
    param([Parameter(Mandatory=$true)] $Bounds)
    $x = [int]($Bounds.left + [Math]::Max(12, [Math]::Min($Bounds.width - 12, [int]($Bounds.width / 2))))
    $y = [int]($Bounds.top + [Math]::Max(8, [Math]::Min($Bounds.height - 8, [int]($Bounds.height / 2))))
    [void][ClaudeComposerWin32]::SetCursorPos($x, $y)
    Start-Sleep -Milliseconds 100
    [ClaudeComposerWin32]::mouse_event([ClaudeComposerWin32]::MOUSEEVENTF_LEFTDOWN, [uint32]$x, [uint32]$y, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 80
    [ClaudeComposerWin32]::mouse_event([ClaudeComposerWin32]::MOUSEEVENTF_LEFTUP, [uint32]$x, [uint32]$y, 0, [UIntPtr]::Zero)
    return [ordered]@{ x = $x; y = $y }
}

$startedAt = Get-Date
$status = "FAILED_UNINITIALIZED"
$blocker = $null
$routeSelected = $false
$routeSelectionAction = $null
$routeSelectionSafeLabel = $null
$promptTypedOrPasted = $false
$promptConfirmed = $false
$promptSent = $false
$enterSent = $false
$uiActionMethod = $null
$sendInputUnits = 0
$clickPoint = $null
$windowInventory = @()
$beforeSummary = $null
$afterRouteSummary = $null
$afterInputSummary = $null

try {
    $scenario = [System.IO.File]::ReadAllText($ScenarioPath, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
    $preflight = [System.IO.File]::ReadAllText($PreflightPath, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
    $prompt = [string]$scenario.exact_prompt
    $promptHash = Get-Sha256Hex -Text $prompt

    $claudeProcesses = Get-Process | ForEach-Object {
        $path = ""; try { $path = $_.Path } catch { $path = "" }
        if ($_.ProcessName -match "^(?i:claude)$" -or $path -match "(?i)\\Claude\\|Claude_|\\claude-code\\") {
            [pscustomobject]@{ Id = $_.Id; ProcessName = $_.ProcessName; MainWindowTitle = $_.MainWindowTitle; Path = $path; MainWindowHandle = $_.MainWindowHandle }
        }
    }

    foreach ($process in $claudeProcesses) {
        $rectObject = $null; $visible = $false; $minimized = $false
        if ($process.MainWindowHandle -ne 0) {
            $hwnd = [IntPtr]$process.MainWindowHandle
            $rect = New-Object ClaudeComposerWin32+RECT
            [void][ClaudeComposerWin32]::GetWindowRect($hwnd, [ref]$rect)
            $rectObject = [ordered]@{ left = $rect.Left; top = $rect.Top; right = $rect.Right; bottom = $rect.Bottom; width = $rect.Right - $rect.Left; height = $rect.Bottom - $rect.Top }
            $visible = [ClaudeComposerWin32]::IsWindowVisible($hwnd)
            $minimized = [ClaudeComposerWin32]::IsIconic($hwnd)
        }
        $windowInventory += [ordered]@{ pid = $process.Id; process_name = $process.ProcessName; title = $process.MainWindowTitle; path = $process.Path; hwnd_decimal = [int64]$process.MainWindowHandle; has_main_window_handle = ($process.MainWindowHandle -ne 0); visible = $visible; minimized = $minimized; rect = $rectObject }
    }
    Write-JsonFile -Value $windowInventory -Path (Join-Path $LogsDir "window-inventory.json")

    $targetWindow = @($windowInventory | Where-Object { $_.has_main_window_handle -and $_.visible -and -not $_.minimized -and $_.title -match "(?i)Claude" } | Sort-Object { $_.rect.width * $_.rect.height } -Descending | Select-Object -First 1)[0]
    if ($null -eq $targetWindow) {
        $status = "FAILED_TARGET_WINDOW_NOT_FOUND"
        $blocker = "No visible non-minimized Claude window with a Claude title was found."
    } else {
        $hwnd = [IntPtr]$targetWindow.hwnd_decimal
        [void][ClaudeComposerWin32]::ShowWindow($hwnd, [ClaudeComposerWin32]::SW_RESTORE)
        [void][ClaudeComposerWin32]::SetForegroundWindow($hwnd)
        Start-Sleep -Milliseconds 700
        $root = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
        $beforeSummary = Get-RedactedUiaSummary -RootElement $root -Prompt $prompt
        Write-JsonFile -Value $beforeSummary -Path (Join-Path $LogsDir "uia-before-action-redacted-summary.json")

        if (@($beforeSummary.categories) -contains "credential_prompt_visible") {
            $status = "FAILED_CREDENTIAL_PROMPT_VISIBLE"
            $blocker = "Credential/login/password/PIN category was visible; stopped before interacting with secrets."
        } elseif (-not (@($beforeSummary.categories) -contains "xiigen_route_visible")) {
            $status = "BLOCKED_COMPOSER_INPUT_NOT_CONFIRMED"
            $blocker = "Claude window was found, but XIIGen/Smart Skills route was not visible in redacted UIA summary."
        } else {
            $routeButton = Find-XiigenRouteButton -RootElement $root
            if ($null -ne $routeButton) {
                $routeSelectionSafeLabel = $routeButton.safe_label
                if ($routeButton.has_invoke_pattern) {
                    $routeButton.invoke_pattern.Invoke()
                    $routeSelectionAction = "UIAutomation InvokePattern on XIIGen route button"
                } else {
                    $routeButton.element.SetFocus()
                    $routeSelectionAction = "UIAutomation SetFocus on XIIGen route button"
                }
                $routeSelected = $true
                Start-Sleep -Seconds 2
                $root = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
                $afterRouteSummary = Get-RedactedUiaSummary -RootElement $root -Prompt $prompt
                Write-JsonFile -Value $afterRouteSummary -Path (Join-Path $LogsDir "uia-after-route-select-redacted-summary.json")
            } else {
                $routeSelected = $true
                $routeSelectionAction = "XIIGen route was visible in redacted UIA summary; no invokable route button found"
            }

            $composer = Find-ComposerElement -RootElement $root
            if ($null -eq $composer) {
                $status = "BLOCKED_COMPOSER_INPUT_NOT_CONFIRMED"
                $blocker = "No enabled on-screen composer/input candidate was found through UI Automation."
            } else {
                try { $composer.element.SetFocus() } catch {}
                Start-Sleep -Milliseconds 300
                $clickPoint = Click-BoundsCenter -Bounds $composer.bounds
                Start-Sleep -Milliseconds 300
                $setValueSucceeded = $false
                if ($composer.has_value_pattern -and $composer.value_is_readonly -eq $false) {
                    try {
                        $composer.value_pattern.SetValue($prompt)
                        $setValueSucceeded = $true
                        $uiActionMethod = "UIAutomation ValuePattern.SetValue"
                    } catch { $setValueSucceeded = $false }
                }
                if (-not $setValueSucceeded) {
                    $sendInputUnits = Send-UnicodeText -Text $prompt
                    $uiActionMethod = "Win32 SendInput Unicode after UIA focus and center click"
                }
                $promptTypedOrPasted = $true
                Start-Sleep -Milliseconds 1500
                $root = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
                $afterInputSummary = Get-RedactedUiaSummary -RootElement $root -Prompt $prompt
                Write-JsonFile -Value $afterInputSummary -Path (Join-Path $LogsDir "uia-after-input-redacted-summary.json")
                $promptConfirmed = [bool]$afterInputSummary.prompt_confirmed_by_redacted_uia
                if ($promptConfirmed) {
                    $status = "CLAUDE_COMPOSER_INPUT_CONFIRMED"
                    $blocker = $null
                } else {
                    $status = "BLOCKED_COMPOSER_INPUT_NOT_CONFIRMED"
                    $blocker = "Prompt input was attempted, but redacted UI Automation did not find the full exact prompt in Name/Value/TextPattern data after input."
                }
            }
        }
    }
} catch {
    $status = "FAILED_EXCEPTION"
    $blocker = $_.Exception.Message
    Write-JsonFile -Value ([ordered]@{
        message = $_.Exception.Message
        type = $_.Exception.GetType().FullName
        invocation = [string]$_.InvocationInfo.PositionMessage
        script_stack_trace = [string]$_.ScriptStackTrace
    }) -Path (Join-Path $LogsDir "exception-details.json")
} finally {
    $endedAt = Get-Date
    $logs = @("logs/window-inventory.json")
    if (Test-Path (Join-Path $LogsDir "uia-before-action-redacted-summary.json")) { $logs += "logs/uia-before-action-redacted-summary.json" }
    if (Test-Path (Join-Path $LogsDir "uia-after-route-select-redacted-summary.json")) { $logs += "logs/uia-after-route-select-redacted-summary.json" }
    if (Test-Path (Join-Path $LogsDir "uia-after-input-redacted-summary.json")) { $logs += "logs/uia-after-input-redacted-summary.json" }
    $result = [ordered]@{
        status = $status
        scenario_id = "frdp-p01-claude-full-extension-entrypoint"
        scope = "Claude Path B composer-input proof only; no prompt submission."
        prompt_source = [ordered]@{
            source_file = "docs/plans/evidence/extension-full-readiness-dual-path/scenario-prep/claude-full-extension-runtime-20260627-223159/scenario.json"
            source_field = "exact_prompt"
            prompt_sha256 = $promptHash
            prompt_length_chars = if ($null -ne $prompt) { $prompt.Length } else { $null }
            raw_prompt_written_to_evidence = $false
        }
        preflight_result = "docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/claude-path-b-route-preflight-20260628-112625/result.json"
        preflight_status = if ($null -ne $preflight) { $preflight.status } else { $null }
        route_selected = [bool]$routeSelected
        route_selection_action = $routeSelectionAction
        route_selection_safe_label = $routeSelectionSafeLabel
        prompt_typed_or_pasted = [bool]$promptTypedOrPasted
        prompt_input_method = $uiActionMethod
        send_input_units = $sendInputUnits
        composer_click_point = $clickPoint
        prompt_confirmed_in_composer = [bool]$promptConfirmed
        prompt_confirmation_method = if ($promptConfirmed) { "Redacted UI Automation in-memory exact prompt match; evidence stores hash/length/bounds only" } else { $null }
        prompt_sent = $false
        enter_sent = $false
        response_or_status_seen = $false
        core_or_adapter_correlation_seen = $false
        screenshots_or_redacted_logs = $logs
        blocker_if_any = $blocker
        next_step = if ($promptConfirmed) { "Separate later task may decide whether to submit; this packet intentionally stops before Send/Enter." } else { "Repair only composer input detection/focus; do not submit or claim response/correlation." }
        deprecated_product_readiness_used = $false
        claims_not_made = @("response/core correlation", "token savings", "semantic quality", "learnability/training", "checkpoint promotion", "all-host readiness")
        redaction = [ordered]@{
            screenshots_captured = $false
            raw_private_text_written = $false
            raw_prompt_written = $false
            note_file = "redaction-note.md"
        }
        timing = [ordered]@{
            started_local = $startedAt.ToString("yyyy-MM-ddTHH:mm:ssK")
            started_utc = $startedAt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
            ended_local = $endedAt.ToString("yyyy-MM-ddTHH:mm:ssK")
            ended_utc = $endedAt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
        }
    }
    Write-JsonFile -Value $result -Path (Join-Path $EvidenceDir "result.json")

    $readme = @"
# Claude Path B: composer input proof

Финальный статус: ``$status``.

Этот пакет проверяет только следующий минимальный шаг: после выбора маршрута XIIGen/Smart Skills безопасный prompt без секретов помещен в composer Claude или это не подтверждено. Отправка prompt намеренно не выполнялась.

Что подтверждено этим пакетом:

- Окно Claude искалось и фиксировалось в ``logs/window-inventory.json``.
- Маршрут XIIGen/Smart Skills выбирался или подтверждался через redacted UI Automation.
- Ввод prompt выполнялся только в composer-кандидат; Enter/Send не нажимались.
- Подтверждение prompt, если есть, записано без raw prompt: только sha256, длина, тип UIA-источника и геометрия элемента.

Что не утверждается:

- ответ Claude;
- core/adapter correlation;
- token savings;
- semantic quality;
- learnability/training;
- checkpoint promotion;
- all-host readiness.

Следующий шаг: если статус ``CLAUDE_COMPOSER_INPUT_CONFIRMED``, отдельным bounded task можно решать отправку. Этот пакет сам отправку не разрешает и не доказывает.
"@
    [System.IO.File]::WriteAllText((Join-Path $EvidenceDir "README.md"), $readme, [System.Text.Encoding]::UTF8)

    $actionLog = @"
# Action Log

- Started: $($startedAt.ToString("yyyy-MM-ddTHH:mm:ssK"))
- Read scenario source: ``docs/plans/evidence/extension-full-readiness-dual-path/scenario-prep/claude-full-extension-runtime-20260627-223159/scenario.json``.
- Read route preflight: ``docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/claude-path-b-route-preflight-20260628-112625/result.json``.
- Located visible Claude window candidates and wrote ``logs/window-inventory.json``.
- Wrote redacted UIA summary before input.
- Selected or confirmed XIIGen/Smart Skills route without submitting a prompt.
- Focused/clicked composer candidate and attempted prompt input using ``$uiActionMethod``.
- Wrote redacted UIA summary after input.
- Did not press Enter or Send.
- Final status: ``$status``.
- Blocker if any: $blocker
"@
    [System.IO.File]::WriteAllText((Join-Path $EvidenceDir "action-log.md"), $actionLog, [System.Text.Encoding]::UTF8)

    $redactionNote = @"
# Redaction Note

No screenshot was captured because the Claude window may contain private account or chat content. The UI Automation logs are redacted: raw private chat text and raw prompt text are not written. Prompt confirmation is recorded only through in-memory comparison against the scenario prompt, with hash/length/source/bounds metadata saved. No password, PIN, credential, account secret, clipboard content, response text, or private chat content was intentionally captured.
"@
    [System.IO.File]::WriteAllText((Join-Path $EvidenceDir "redaction-note.md"), $redactionNote, [System.Text.Encoding]::UTF8)
}
