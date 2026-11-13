param(
    [string]$RepoRoot = "C:\Projects\llm_mvp_core",
    [string]$ScenarioPath = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-prep\claude-full-extension-runtime-20260627-223159\scenario.json",
    [string]$PreflightPath = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\claude-path-b-route-preflight-20260628-112625\result.json"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class ClaudeComposerRetryWin32 {
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
    public const ushort VK_CONTROL = 0x11;
    public const ushort VK_V = 0x56;
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
    [StructLayout(LayoutKind.Sequential)] public struct INPUT { public uint type; public InputUnion U; }
    [StructLayout(LayoutKind.Explicit)] public struct InputUnion { [FieldOffset(0)] public KEYBDINPUT ki; }
    [StructLayout(LayoutKind.Sequential)] public struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public UIntPtr dwExtraInfo; }
}
"@

$EvidenceDir = Split-Path -Parent $PSScriptRoot
$LogsDir = Join-Path $EvidenceDir "logs"
New-Item -ItemType Directory -Force -Path $LogsDir | Out-Null
$Utf8NoBom = [System.Text.UTF8Encoding]::new($false)
$actionLog = New-Object "System.Collections.Generic.List[string]"

function Add-Action {
    param([Parameter(Mandatory=$true)] [string]$Message)
    $stamp = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssK")
    $actionLog.Add("- $stamp - $Message") | Out-Null
}

function Write-TextFile {
    param([Parameter(Mandatory=$true)] [string]$Path, [AllowNull()] [string]$Text)
    if ($null -eq $Text) { $Text = "" }
    $normalized = $Text -replace "`r`n", "`n" -replace "`r", "`n"
    $lines = $normalized -split "`n", -1
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $lines[$i] = $lines[$i].TrimEnd()
    }
    [System.IO.File]::WriteAllText($Path, ($lines -join "`n"), $Utf8NoBom)
}

function Write-JsonFile {
    param([Parameter(Mandatory=$true)] $Value, [Parameter(Mandatory=$true)] [string]$Path)
    $json = $Value | ConvertTo-Json -Depth 30
    Write-TextFile -Path $Path -Text ($json + "`n")
}

function Get-Sha256Hex {
    param([AllowNull()] [string]$Text)
    if ($null -eq $Text) { $Text = "" }
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
        return ([System.BitConverter]::ToString($sha.ComputeHash($bytes))).Replace("-", "").ToLowerInvariant()
    } finally {
        $sha.Dispose()
    }
}

function Get-BoundsObject {
    param($Element)
    try {
        $rect = $Element.Current.BoundingRectangle
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
    } catch {
        return $null
    }
}

function Test-BoundsInside {
    param($Outer, $Inner)
    if ($null -eq $Outer -or $null -eq $Inner) { return $false }
    $cx = [double]$Inner.left + ([double]$Inner.width / 2.0)
    $cy = [double]$Inner.top + ([double]$Inner.height / 2.0)
    return ($cx -ge $Outer.left -and $cx -le $Outer.right -and $cy -ge $Outer.top -and $cy -le $Outer.bottom)
}

function Get-RuntimeIdString {
    param($Element)
    try {
        $ids = $Element.GetRuntimeId()
        if ($null -eq $ids) { return $null }
        return [string]::Join(".", [int[]]$ids)
    } catch {
        return $null
    }
}

function Get-ElementPatternFacts {
    param($Element)
    $valuePattern = $null
    $textPattern = $null
    $invokePattern = $null
    $hasValue = $false
    $valueReadonly = $null
    $hasText = $false
    $hasInvoke = $false
    try {
        $valuePattern = $Element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        $hasValue = ($null -ne $valuePattern)
        if ($hasValue) { $valueReadonly = [bool]$valuePattern.Current.IsReadOnly }
    } catch {}
    try {
        $textPattern = $Element.GetCurrentPattern([System.Windows.Automation.TextPattern]::Pattern)
        $hasText = ($null -ne $textPattern)
    } catch {}
    try {
        $invokePattern = $Element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
        $hasInvoke = ($null -ne $invokePattern)
    } catch {}
    return [pscustomobject]@{
        value_pattern = $valuePattern
        text_pattern = $textPattern
        invoke_pattern = $invokePattern
        has_value_pattern = $hasValue
        value_is_readonly = $valueReadonly
        has_text_pattern = $hasText
        has_invoke_pattern = $hasInvoke
    }
}

function Get-ElementTextsForProbe {
    param($Element)
    $items = New-Object "System.Collections.Generic.List[object]"
    try {
        $vp = $Element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        if ($null -ne $vp) {
            $items.Add([ordered]@{ source = "ValuePattern.Value"; text = [string]$vp.Current.Value }) | Out-Null
        }
    } catch {}
    try {
        $tp = $Element.GetCurrentPattern([System.Windows.Automation.TextPattern]::Pattern)
        if ($null -ne $tp) {
            $items.Add([ordered]@{ source = "TextPattern.DocumentRange"; text = [string]$tp.DocumentRange.GetText(-1) }) | Out-Null
        }
    } catch {}
    try {
        $name = [string]$Element.Current.Name
        if (-not [string]::IsNullOrWhiteSpace($name)) {
            $items.Add([ordered]@{ source = "Current.Name"; text = $name }) | Out-Null
        }
    } catch {}
    return @($items)
}

function Get-UiaDescendants {
    param([Parameter(Mandatory=$true)] $RootElement)
    try {
        $collection = $RootElement.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
        $items = New-Object "System.Collections.Generic.List[object]"
        for ($i = 0; $i -lt $collection.Count; $i++) {
            $items.Add($collection.Item($i)) | Out-Null
        }
        return [pscustomobject]@{ elements = @($items); error = $null; method = "FindAll(TreeScope.Descendants, TrueCondition)" }
    } catch {
        return [pscustomobject]@{ elements = @(); error = $_.Exception.Message; method = "FindAll(TreeScope.Descendants, TrueCondition)" }
    }
}

function New-SafeLabel {
    param([AllowNull()] [string]$Name, [string]$Category)
    if ([string]::IsNullOrWhiteSpace($Name)) { return "" }
    if ($Category -eq "xiigen_route_visible" -and $Name.Length -le 160 -and $Name -notmatch "[@\\/:]") { return $Name }
    return "[redacted UI label; category-only match]"
}

function Copy-FirstItems {
    param($List, [int]$Limit)
    $items = @()
    if ($null -eq $List -or $Limit -le 0) { return $items }
    $count = 0
    foreach ($item in $List) {
        if ($count -ge $Limit) { break }
        $items += $item
        $count++
    }
    return $items
}

function Get-RedactedUiaSummary {
    param([Parameter(Mandatory=$true)] $RootElement, [Parameter(Mandatory=$true)] [string]$Prompt)
    $promptPrefix = if ($Prompt.Length -gt 40) { $Prompt.Substring(0, 40) } else { $Prompt }
    $desc = Get-UiaDescendants -RootElement $RootElement
    $categories = New-Object "System.Collections.Generic.List[string]"
    $safeHits = New-Object "System.Collections.Generic.List[object]"
    $inputCandidates = New-Object "System.Collections.Generic.List[object]"
    $routeCandidates = New-Object "System.Collections.Generic.List[object]"
    $sendCandidates = New-Object "System.Collections.Generic.List[object]"
    $promptMatches = New-Object "System.Collections.Generic.List[object]"
    $elementErrors = New-Object "System.Collections.Generic.List[object]"
    $fullPromptMatchCount = 0
    $prefixPromptMatchCount = 0
    $elements = @($desc.elements)
    $limit = [Math]::Min($elements.Count, 7000)

    for ($i = 0; $i -lt $limit; $i++) {
        try {
            $element = $elements[$i]
            $current = $element.Current
            $name = [string]$current.Name
            $controlType = [string]$current.ControlType.ProgrammaticName
            $bounds = Get-BoundsObject -Element $element
            $focusable = [bool]$current.IsKeyboardFocusable
            $enabled = [bool]$current.IsEnabled
            $offscreen = [bool]$current.IsOffscreen
            $facts = Get-ElementPatternFacts -Element $element

            $category = $null
            if ($name -match "(?i)log\s*in|sign\s*in|password|passcode|pin|credential|one-time|2fa") { $category = "credential_prompt_visible" }
            elseif ($name -match "(?i)xiigen|smart\s*skills") { $category = "xiigen_route_visible" }
            elseif ($name -match "(?i)claude") { $category = "claude_label_visible" }
            elseif ($name -match "(?i)chat|message|ask") { $category = "chat_surface_visible" }

            if ($null -ne $category) {
                if (-not $categories.Contains($category)) { $categories.Add($category) | Out-Null }
                if ($safeHits.Count -lt 120) {
                    $safeHits.Add([ordered]@{
                        category = $category
                        control_type = $controlType
                        safe_label = New-SafeLabel -Name $name -Category $category
                        focusable = $focusable
                        enabled = $enabled
                        offscreen = $offscreen
                        bounds = $bounds
                    }) | Out-Null
                }
            }

            foreach ($textItem in @(Get-ElementTextsForProbe -Element $element)) {
                $text = [string]$textItem.text
                if ([string]::IsNullOrEmpty($text)) { continue }
                $containsExact = $text.Contains($Prompt)
                $containsPrefix = (-not [string]::IsNullOrEmpty($promptPrefix)) -and $text.Contains($promptPrefix)
                if ($containsExact -or $containsPrefix) {
                    if ($containsExact) { $fullPromptMatchCount++ }
                    if ($containsPrefix) { $prefixPromptMatchCount++ }
                    if ($promptMatches.Count -lt 12) {
                        $promptMatches.Add([ordered]@{
                            element_index = $i
                            source = [string]$textItem.source
                            control_type = $controlType
                            focusable = $focusable
                            enabled = $enabled
                            offscreen = $offscreen
                            bounds = $bounds
                            observed_text_length_chars = $text.Length
                            observed_text_sha256 = Get-Sha256Hex -Text $text
                            contains_exact_prompt = [bool]$containsExact
                            contains_prompt_prefix = [bool]$containsPrefix
                            raw_text_written = $false
                        }) | Out-Null
                    }
                }
            }

            $looksLikeComposer = (
                $controlType -eq "ControlType.Edit" -or
                $controlType -eq "ControlType.Document" -or
                (($focusable -or $facts.has_value_pattern -or $facts.has_text_pattern) -and
                    $enabled -and -not $offscreen -and $null -ne $bounds -and
                    $bounds.width -gt 220 -and $bounds.height -gt 18 -and $bounds.top -gt 280)
            )
            if ($looksLikeComposer -and $null -ne $bounds) {
                $score = 0
                if ($controlType -eq "ControlType.Edit") { $score += 12 }
                if ($controlType -eq "ControlType.Document") { $score += 5 }
                if ($facts.has_value_pattern -and $facts.value_is_readonly -eq $false) { $score += 14 }
                if ($facts.has_text_pattern) { $score += 8 }
                if ($focusable) { $score += 6 }
                if ($bounds.top -gt 650) { $score += 8 }
                if ($bounds.width -gt 350) { $score += 4 }
                if ($name -match "(?i)message|ask|composer|prompt|reply") { $score += 8 }
                if ($controlType -eq "ControlType.Button") { $score -= 20 }
                if ($enabled -and -not $offscreen -and $score -gt 0) {
                    $inputCandidates.Add([ordered]@{
                        score = $score
                        index = $i
                        runtime_id = Get-RuntimeIdString -Element $element
                        control_type = $controlType
                        focusable = $focusable
                        enabled = $enabled
                        offscreen = $offscreen
                        has_value_pattern = [bool]$facts.has_value_pattern
                        value_is_readonly = $facts.value_is_readonly
                        has_text_pattern = [bool]$facts.has_text_pattern
                        safe_label_category = if ([string]::IsNullOrWhiteSpace($name)) { "empty_label" } else { "redacted_nonempty_label" }
                        bounds = $bounds
                    }) | Out-Null
                }
            }

            if ($controlType -eq "ControlType.Button" -and $enabled -and -not $offscreen -and $name -match "(?i)^send$|send message|submit") {
                $sendCandidates.Add([ordered]@{ index = $i; control_type = $controlType; safe_label_category = "generic_send_button_label"; bounds = $bounds }) | Out-Null
            }
            if ($enabled -and -not $offscreen -and $name -match "(?i)xiigen|smart\s*skills") {
                $routeCandidates.Add([ordered]@{
                    index = $i
                    control_type = $controlType
                    safe_label = New-SafeLabel -Name $name -Category "xiigen_route_visible"
                    bounds = $bounds
                }) | Out-Null
            }
        } catch {
            if ($elementErrors.Count -lt 30) {
                $elementErrors.Add([ordered]@{ element_index = $i; error = $_.Exception.Message }) | Out-Null
            }
        }
    }

    $categoryRows = @()
    foreach ($categoryItem in $categories) { $categoryRows += [string]$categoryItem }

    return [ordered]@{
        descendant_count = $elements.Count
        enumeration_method = $desc.method
        helper_error = $desc.error
        helper_failed_before_detection = ($null -ne $desc.error -and $elements.Count -eq 0)
        categories = $categoryRows
        safe_hit_count = $safeHits.Count
        input_candidate_count = $inputCandidates.Count
        route_candidate_count = $routeCandidates.Count
        send_candidate_count = $sendCandidates.Count
        prompt_confirmation_match_count = $promptMatches.Count
        prompt_confirmation_matches = @()
        prompt_confirmed_by_redacted_uia = ($fullPromptMatchCount -gt 0)
        prompt_prefix_seen_without_full_match = ($prefixPromptMatchCount -gt 0 -and $fullPromptMatchCount -eq 0)
        prompt_exact_match_count = $fullPromptMatchCount
        prompt_prefix_match_count = $prefixPromptMatchCount
        raw_private_text_written = $false
        skipped_element_error_count = $elementErrors.Count
    }
}

function Find-XiigenRouteElement {
    param([Parameter(Mandatory=$true)] $RootElement)
    $desc = Get-UiaDescendants -RootElement $RootElement
    $best = $null
    $elements = @($desc.elements)
    for ($i = 0; $i -lt [Math]::Min($elements.Count, 7000); $i++) {
        try {
            $element = $elements[$i]
            $current = $element.Current
            $name = [string]$current.Name
            if ([string]::IsNullOrWhiteSpace($name) -or $name -notmatch "(?i)xiigen|smart\s*skills") { continue }
            if (-not $current.IsEnabled -or $current.IsOffscreen) { continue }
            $bounds = Get-BoundsObject -Element $element
            if ($null -eq $bounds) { continue }
            $facts = Get-ElementPatternFacts -Element $element
            $score = 0
            if ([string]$current.ControlType.ProgrammaticName -eq "ControlType.Button") { $score += 8 }
            if ($facts.has_invoke_pattern) { $score += 6 }
            if ($name -match "(?i)XIIGen Smart Skills UI proof run") { $score += 18 }
            if ($name -match "(?i)^Idle\s+") { $score += 5 }
            $candidate = [pscustomobject]@{
                score = $score
                element = $element
                facts = $facts
                safe_label = New-SafeLabel -Name $name -Category "xiigen_route_visible"
                bounds = $bounds
            }
            if ($null -eq $best -or $candidate.score -gt $best.score) { $best = $candidate }
        } catch {}
    }
    return $best
}

function Find-ComposerElement {
    param([Parameter(Mandatory=$true)] $RootElement)
    $desc = Get-UiaDescendants -RootElement $RootElement
    $best = $null
    $elements = @($desc.elements)
    for ($i = 0; $i -lt [Math]::Min($elements.Count, 7000); $i++) {
        try {
            $element = $elements[$i]
            $current = $element.Current
            if (-not $current.IsEnabled -or $current.IsOffscreen) { continue }
            $bounds = Get-BoundsObject -Element $element
            if ($null -eq $bounds -or $bounds.width -lt 220 -or $bounds.height -lt 18 -or $bounds.top -lt 280) { continue }
            $name = [string]$current.Name
            $controlType = [string]$current.ControlType.ProgrammaticName
            if ($controlType -eq "ControlType.Button") { continue }
            $facts = Get-ElementPatternFacts -Element $element
            $focusable = [bool]$current.IsKeyboardFocusable
            $looks = (
                $controlType -eq "ControlType.Edit" -or
                $controlType -eq "ControlType.Document" -or
                $facts.has_value_pattern -or
                $facts.has_text_pattern -or
                $focusable -or
                $name -match "(?i)message|ask|composer|prompt|reply"
            )
            if (-not $looks) { continue }
            $score = 0
            if ($controlType -eq "ControlType.Edit") { $score += 14 }
            if ($controlType -eq "ControlType.Document") { $score += 6 }
            if ($facts.has_value_pattern -and $facts.value_is_readonly -eq $false) { $score += 18 }
            if ($facts.has_text_pattern) { $score += 10 }
            if ($focusable) { $score += 6 }
            if ($bounds.top -gt 650) { $score += 10 }
            if ($bounds.width -gt 350) { $score += 4 }
            if ($name -match "(?i)message|ask|composer|prompt|reply") { $score += 8 }
            if ($score -lt 8) { continue }
            $candidate = [pscustomobject]@{
                score = $score
                element = $element
                facts = $facts
                runtime_id = Get-RuntimeIdString -Element $element
                control_type = $controlType
                bounds = $bounds
                focusable = $focusable
                safe_label_category = if ([string]::IsNullOrWhiteSpace($name)) { "empty_label" } else { "redacted_nonempty_label" }
            }
            if ($null -eq $best -or $candidate.score -gt $best.score) { $best = $candidate }
        } catch {}
    }
    return $best
}

function Click-BoundsCenter {
    param([Parameter(Mandatory=$true)] $Bounds)
    $x = [int]($Bounds.left + [Math]::Max(12, [Math]::Min($Bounds.width - 12, [int]($Bounds.width / 2))))
    $y = [int]($Bounds.top + [Math]::Max(8, [Math]::Min($Bounds.height - 8, [int]($Bounds.height / 2))))
    [void][ClaudeComposerRetryWin32]::SetCursorPos($x, $y)
    Start-Sleep -Milliseconds 100
    [ClaudeComposerRetryWin32]::mouse_event([ClaudeComposerRetryWin32]::MOUSEEVENTF_LEFTDOWN, [uint32]$x, [uint32]$y, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 80
    [ClaudeComposerRetryWin32]::mouse_event([ClaudeComposerRetryWin32]::MOUSEEVENTF_LEFTUP, [uint32]$x, [uint32]$y, 0, [UIntPtr]::Zero)
    return [ordered]@{ x = $x; y = $y }
}

function Send-KeyStroke {
    param([Parameter(Mandatory=$true)] [ushort]$VirtualKey, [bool]$KeyUp)
    $input = New-Object ClaudeComposerRetryWin32+INPUT
    $input.type = [ClaudeComposerRetryWin32]::INPUT_KEYBOARD
    $input.U.ki.wVk = $VirtualKey
    $input.U.ki.wScan = 0
    $input.U.ki.dwFlags = if ($KeyUp) { [ClaudeComposerRetryWin32]::KEYEVENTF_KEYUP } else { 0 }
    $input.U.ki.time = 0
    $input.U.ki.dwExtraInfo = [UIntPtr]::Zero
    return $input
}

function Send-CtrlV {
    $inputs = [ClaudeComposerRetryWin32+INPUT[]]@(
        (Send-KeyStroke -VirtualKey ([ClaudeComposerRetryWin32]::VK_CONTROL) -KeyUp $false),
        (Send-KeyStroke -VirtualKey ([ClaudeComposerRetryWin32]::VK_V) -KeyUp $false),
        (Send-KeyStroke -VirtualKey ([ClaudeComposerRetryWin32]::VK_V) -KeyUp $true),
        (Send-KeyStroke -VirtualKey ([ClaudeComposerRetryWin32]::VK_CONTROL) -KeyUp $true)
    )
    return [ClaudeComposerRetryWin32]::SendInput([uint32]$inputs.Length, $inputs, [System.Runtime.InteropServices.Marshal]::SizeOf([ClaudeComposerRetryWin32+INPUT]))
}

function Test-FocusOnComposer {
    param([Parameter(Mandatory=$true)] $Composer)
    try {
        $focused = [System.Windows.Automation.AutomationElement]::FocusedElement
        if ($null -eq $focused) { return [pscustomobject]@{ ok = $false; reason = "no_focused_element"; focused = $null } }
        $focusedId = Get-RuntimeIdString -Element $focused
        $focusedBounds = Get-BoundsObject -Element $focused
        $candidateId = $Composer.runtime_id
        $same = (-not [string]::IsNullOrWhiteSpace($candidateId) -and $candidateId -eq $focusedId)
        $inside = Test-BoundsInside -Outer $Composer.bounds -Inner $focusedBounds
        $controlType = ""; try { $controlType = [string]$focused.Current.ControlType.ProgrammaticName } catch {}
        $ok = ($same -or $inside -or $controlType -eq "ControlType.Edit" -or $controlType -eq "ControlType.Document")
        return [pscustomobject]@{
            ok = [bool]$ok
            reason = if ($ok) { "focused element matched composer or stayed inside candidate bounds" } else { "focused element did not match composer candidate" }
            focused = [ordered]@{ runtime_id = $focusedId; control_type = $controlType; bounds = $focusedBounds }
        }
    } catch {
        return [pscustomobject]@{ ok = $false; reason = $_.Exception.Message; focused = $null }
    }
}

function Set-ClipboardTextPreservingPrevious {
    param([Parameter(Mandatory=$true)] [string]$Text)
    $state = [ordered]@{ had_text = $false; previous_text = $null; set_ok = $false; restore_attempted = $false; restore_ok = $false }
    try {
        if ([System.Windows.Forms.Clipboard]::ContainsText()) {
            $state.had_text = $true
            $state.previous_text = [System.Windows.Forms.Clipboard]::GetText()
        }
    } catch {}
    [System.Windows.Forms.Clipboard]::SetText($Text)
    $state.set_ok = $true
    return $state
}

function Restore-ClipboardText {
    param($State)
    if ($null -eq $State) { return }
    $State.restore_attempted = $true
    try {
        if ($State.had_text) {
            [System.Windows.Forms.Clipboard]::SetText([string]$State.previous_text)
        } else {
            [System.Windows.Forms.Clipboard]::Clear()
        }
        $State.restore_ok = $true
    } catch {
        $State.restore_ok = $false
    }
    $State.previous_text = $null
}

$startedAt = Get-Date
$status = "FAILED_UNINITIALIZED"
$blocker = $null
$prompt = $null
$promptHash = $null
$preflight = $null
$routeSelected = $false
$routeAction = $null
$routeLabel = $null
$composerFound = $false
$composerSummary = $null
$composerClickPoint = $null
$focusProbe = $null
$promptTypedOrPasted = $false
$promptInputMethod = $null
$promptConfirmed = $false
$promptConfirmationMethod = $null
$promptSent = $false
$enterSent = $false
$sendInputUnits = 0
$windowInventory = @()
$beforeSummary = $null
$afterRouteSummary = $null
$afterInputSummary = $null
$clipboardState = $null

try {
    Add-Action "Runner started. Scope is composer input proof only; Send and Enter are forbidden."
    $scenario = [System.IO.File]::ReadAllText($ScenarioPath, $Utf8NoBom) | ConvertFrom-Json
    $preflight = [System.IO.File]::ReadAllText($PreflightPath, $Utf8NoBom) | ConvertFrom-Json
    $prompt = [string]$scenario.exact_prompt
    $promptHash = Get-Sha256Hex -Text $prompt
    Add-Action "Loaded scenario prompt in memory only; raw prompt will not be written to evidence."

    $claudeProcesses = Get-Process | ForEach-Object {
        $path = ""
        try { $path = $_.Path } catch { $path = "" }
        if ($_.ProcessName -match "^(?i:claude)$" -or $path -match "(?i)\\Claude\\|Claude_|\\claude-code\\") {
            [pscustomobject]@{
                Id = $_.Id
                ProcessName = $_.ProcessName
                MainWindowTitle = $_.MainWindowTitle
                Path = $path
                MainWindowHandle = $_.MainWindowHandle
            }
        }
    }

    foreach ($process in @($claudeProcesses)) {
        $rectObject = $null
        $visible = $false
        $minimized = $false
        if ($process.MainWindowHandle -ne 0) {
            $hwnd = [IntPtr]$process.MainWindowHandle
            $rect = New-Object ClaudeComposerRetryWin32+RECT
            [void][ClaudeComposerRetryWin32]::GetWindowRect($hwnd, [ref]$rect)
            $rectObject = [ordered]@{
                left = $rect.Left
                top = $rect.Top
                right = $rect.Right
                bottom = $rect.Bottom
                width = $rect.Right - $rect.Left
                height = $rect.Bottom - $rect.Top
            }
            $visible = [ClaudeComposerRetryWin32]::IsWindowVisible($hwnd)
            $minimized = [ClaudeComposerRetryWin32]::IsIconic($hwnd)
        }
        $windowInventory += [ordered]@{
            pid = $process.Id
            process_name = $process.ProcessName
            title = $process.MainWindowTitle
            path = $process.Path
            hwnd_decimal = [int64]$process.MainWindowHandle
            has_main_window_handle = ($process.MainWindowHandle -ne 0)
            visible = $visible
            minimized = $minimized
            rect = $rectObject
        }
    }
    Write-JsonFile -Value $windowInventory -Path (Join-Path $LogsDir "window-inventory.json")
    Add-Action "Wrote redacted window inventory."

    $targetWindow = @($windowInventory | Where-Object {
        $_.has_main_window_handle -and $_.visible -and -not $_.minimized -and $_.title -match "(?i)Claude"
    } | Sort-Object { $_.rect.width * $_.rect.height } -Descending | Select-Object -First 1)[0]

    if ($null -eq $targetWindow) {
        $status = "FAILED_TARGET_WINDOW_NOT_FOUND"
        $blocker = "No visible non-minimized Claude window with a Claude title was found."
        Add-Action $blocker
    } else {
        $hwnd = [IntPtr]$targetWindow.hwnd_decimal
        [void][ClaudeComposerRetryWin32]::ShowWindow($hwnd, [ClaudeComposerRetryWin32]::SW_RESTORE)
        [void][ClaudeComposerRetryWin32]::SetForegroundWindow($hwnd)
        Start-Sleep -Milliseconds 800
        Add-Action "Focused visible Claude window by HWND."

        $root = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
        $beforeSummary = Get-RedactedUiaSummary -RootElement $root -Prompt $prompt
        Write-JsonFile -Value $beforeSummary -Path (Join-Path $LogsDir "uia-before-action-redacted-summary.json")
        Add-Action "Wrote redacted UIA summary before route/composer action."

        if ($beforeSummary.helper_failed_before_detection -and $beforeSummary.helper_error -match "Argument types do not match") {
            $status = "BLOCKED_UIA_HELPER_ARGUMENT_TYPES_RETRY_FAILED"
            $blocker = "UIA helper retry still failed before route/composer detection with Argument types do not match."
            Add-Action $blocker
        } elseif (@($beforeSummary.categories) -contains "credential_prompt_visible") {
            $status = "FAILED_CREDENTIAL_PROMPT_VISIBLE"
            $blocker = "Credential/login/password/PIN category was visible; stopped before interacting with secrets."
            Add-Action $blocker
        } elseif (-not (@($beforeSummary.categories) -contains "xiigen_route_visible")) {
            $status = "BLOCKED_COMPOSER_INPUT_NOT_CONFIRMED"
            $blocker = "Claude window was found, but XIIGen/Smart Skills route was not visible in redacted UIA summary."
            Add-Action $blocker
        } else {
            $route = Find-XiigenRouteElement -RootElement $root
            if ($null -ne $route) {
                $routeLabel = $route.safe_label
                try {
                    if ($route.facts.has_invoke_pattern) {
                        $route.facts.invoke_pattern.Invoke()
                        $routeAction = "UIAutomation InvokePattern on XIIGen route element"
                    } else {
                        $route.element.SetFocus()
                        $routeAction = "UIAutomation SetFocus on XIIGen route element"
                    }
                    $routeSelected = $true
                    Add-Action "Selected or focused XIIGen route through UIA."
                    Start-Sleep -Seconds 2
                } catch {
                    $routeSelected = $false
                    $routeAction = "Route element found but UIA focus/invoke failed: $($_.Exception.Message)"
                    Add-Action $routeAction
                }
            } else {
                $routeSelected = $true
                $routeAction = "XIIGen route was visible in redacted UIA summary; no invokable route element found"
                Add-Action $routeAction
            }

            $root = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
            $afterRouteSummary = Get-RedactedUiaSummary -RootElement $root -Prompt $prompt
            Write-JsonFile -Value $afterRouteSummary -Path (Join-Path $LogsDir "uia-after-route-select-redacted-summary.json")
            Add-Action "Wrote redacted UIA summary after route focus/selection."

            $composer = Find-ComposerElement -RootElement $root
            if ($null -eq $composer) {
                $status = "BLOCKED_COMPOSER_INPUT_NOT_CONFIRMED"
                $blocker = "No enabled on-screen composer/input candidate was found through UI Automation."
                Add-Action $blocker
            } else {
                $composerFound = $true
                $composerSummary = [ordered]@{
                    score = $composer.score
                    runtime_id = $composer.runtime_id
                    control_type = $composer.control_type
                    focusable = [bool]$composer.focusable
                    has_value_pattern = [bool]$composer.facts.has_value_pattern
                    value_is_readonly = $composer.facts.value_is_readonly
                    has_text_pattern = [bool]$composer.facts.has_text_pattern
                    safe_label_category = $composer.safe_label_category
                    bounds = $composer.bounds
                }
                Add-Action "Composer candidate found with redacted metadata."

                $hasSafeInputPath = (
                    ($composer.facts.has_value_pattern -and $composer.facts.value_is_readonly -eq $false) -or
                    $composer.facts.has_text_pattern
                )
                if (-not $hasSafeInputPath) {
                    $status = "BLOCKED_COMPOSER_INPUT_NOT_CONFIRMED"
                    $blocker = "Composer candidate was found, but it had no writable ValuePattern or readable TextPattern, so prompt input was not attempted."
                    Add-Action $blocker
                } else {
                    try { $composer.element.SetFocus() } catch {}
                    Start-Sleep -Milliseconds 250
                    $composerClickPoint = Click-BoundsCenter -Bounds $composer.bounds
                    Start-Sleep -Milliseconds 450
                    $focusProbe = Test-FocusOnComposer -Composer $composer
                    if (-not $focusProbe.ok) {
                        $status = "BLOCKED_COMPOSER_INPUT_NOT_CONFIRMED"
                        $blocker = "Composer candidate could not be safely focused before input: $($focusProbe.reason)"
                        Add-Action $blocker
                    } else {
                        $setValueSucceeded = $false
                        if ($composer.facts.has_value_pattern -and $composer.facts.value_is_readonly -eq $false) {
                            try {
                                $composer.facts.value_pattern.SetValue($prompt)
                                $setValueSucceeded = $true
                                $promptInputMethod = "UIAutomation ValuePattern.SetValue"
                                Add-Action "Prompt placed with UIAutomation ValuePattern.SetValue."
                            } catch {
                                Add-Action "ValuePattern.SetValue failed; will use clipboard paste only because TextPattern/readback path exists."
                            }
                        }
                        if (-not $setValueSucceeded) {
                            if (-not $composer.facts.has_text_pattern) {
                                $status = "BLOCKED_COMPOSER_INPUT_NOT_CONFIRMED"
                                $blocker = "ValuePattern.SetValue failed and no TextPattern readback path existed, so prompt input was not attempted."
                                Add-Action $blocker
                            } else {
                                $clipboardState = Set-ClipboardTextPreservingPrevious -Text $prompt
                                $sendInputUnits = [int](Send-CtrlV)
                                $promptInputMethod = "Clipboard paste via Ctrl+V after UIA focus/click; clipboard restored in memory after run"
                                Add-Action "Prompt pasted with Ctrl+V; no Enter/Send key was used."
                            }
                        }

                        if ($null -eq $blocker) {
                            $promptTypedOrPasted = $true
                            Start-Sleep -Milliseconds 1600
                            $root = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
                            $afterInputSummary = Get-RedactedUiaSummary -RootElement $root -Prompt $prompt
                            Write-JsonFile -Value $afterInputSummary -Path (Join-Path $LogsDir "uia-after-input-redacted-summary.json")
                            Add-Action "Wrote redacted UIA summary after input."
                            $promptConfirmed = [bool]$afterInputSummary.prompt_confirmed_by_redacted_uia
                            if ($promptConfirmed) {
                                $status = "CLAUDE_COMPOSER_INPUT_CONFIRMED"
                                $promptConfirmationMethod = "Redacted UIA exact prompt match; evidence stores hash/length/source/bounds only"
                                $blocker = $null
                                Add-Action "Prompt presence in composer/UIA tree confirmed without raw prompt evidence; prompt_sent=false."
                            } else {
                                $status = "BLOCKED_COMPOSER_INPUT_NOT_CONFIRMED"
                                $blocker = "Prompt input was attempted, but redacted UI Automation did not find the full exact prompt after input."
                                Add-Action $blocker
                            }
                        }
                    }
                }
            }
        }
    }
} catch {
    if ($_.Exception.Message -match "Argument types do not match" -and -not $composerFound -and -not $promptTypedOrPasted) {
        $status = "BLOCKED_UIA_HELPER_ARGUMENT_TYPES_RETRY_FAILED"
    } else {
        $status = "FAILED_EXCEPTION"
    }
    $blocker = $_.Exception.Message
    Write-JsonFile -Value ([ordered]@{
        message = $_.Exception.Message
        type = $_.Exception.GetType().FullName
        invocation = [string]$_.InvocationInfo.PositionMessage
        script_stack_trace = [string]$_.ScriptStackTrace
    }) -Path (Join-Path $LogsDir "exception-details.json")
    Add-Action "Runner exception captured in logs/exception-details.json."
} finally {
    Restore-ClipboardText -State $clipboardState
    if ($null -ne $clipboardState -and $clipboardState.restore_attempted) {
        Add-Action "Clipboard restore attempted; raw clipboard contents not written."
    }

    $endedAt = Get-Date
    $logs = New-Object "System.Collections.Generic.List[string]"
    foreach ($name in @(
        "logs/window-inventory.json",
        "logs/uia-before-action-redacted-summary.json",
        "logs/uia-after-route-select-redacted-summary.json",
        "logs/uia-after-input-redacted-summary.json",
        "logs/exception-details.json",
        "logs/runner-stdout.log",
        "logs/runner-stderr.log"
    )) {
        if (Test-Path (Join-Path $EvidenceDir $name)) { $logs.Add($name) | Out-Null }
    }

    $result = [ordered]@{
        status = $status
        scenario_id = "frdp-p01-claude-full-extension-entrypoint"
        scope = "Claude Path B composer-input proof only; no prompt submission."
        latest_result_parsed = $true
        local_runner_repaired_helper_argument_path = $true
        prompt_source = [ordered]@{
            source_file = "docs/plans/evidence/extension-full-readiness-dual-path/scenario-prep/claude-full-extension-runtime-20260627-223159/scenario.json"
            source_field = "exact_prompt"
            prompt_sha256 = $promptHash
            prompt_length_chars = if ($null -ne $prompt) { $prompt.Length } else { $null }
            raw_prompt_written_to_evidence = $false
        }
        preflight_result = "docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/claude-path-b-route-preflight-20260628-112625/result.json"
        preflight_status = if ($null -ne $preflight) { $preflight.status } else { $null }
        visible_claude_window_found = [bool]($windowInventory | Where-Object { $_.has_main_window_handle -and $_.visible -and -not $_.minimized -and $_.title -match "(?i)Claude" } | Select-Object -First 1)
        route_selected = [bool]$routeSelected
        route_selection_action = $routeAction
        route_selection_safe_label = $routeLabel
        composer_found = [bool]$composerFound
        composer_candidate = $composerSummary
        composer_focus_probe = $focusProbe
        composer_click_point = $composerClickPoint
        prompt_typed_or_pasted = [bool]$promptTypedOrPasted
        prompt_input_method = $promptInputMethod
        send_input_units = $sendInputUnits
        prompt_confirmed_in_composer = [bool]$promptConfirmed
        prompt_confirmation_method = $promptConfirmationMethod
        prompt_sent = $false
        enter_sent = $false
        response_or_status_seen = $false
        core_or_adapter_correlation_seen = $false
        screenshots_or_redacted_logs = @($logs)
        blocker_if_any = $blocker
        next_step = if ($promptConfirmed) { "Stop here; a separate explicit work order is required before any Send/Enter submission." } else { "Run a synthetic/minimal UIA helper argument-type test without Claude UI before retrying composer detection." }
        deprecated_product_readiness_used = $false
        claims_not_made = @(
            "prompt submission",
            "Claude response",
            "core or adapter correlation after submission",
            "token savings",
            "semantic quality",
            "learnability or training",
            "checkpoint promotion",
            "cross-host completion"
        )
        redaction = [ordered]@{
            screenshots_captured = $false
            raw_private_text_written = $false
            raw_prompt_written = $false
            route_labels_limited_to_xiigen_safe_labels = $true
            note_file = "redaction-note.md"
        }
        clipboard_handling = [ordered]@{
            clipboard_used_for_input = [bool]($null -ne $clipboardState -and $clipboardState.set_ok)
            previous_clipboard_text_written_to_evidence = $false
            restore_attempted = [bool]($null -ne $clipboardState -and $clipboardState.restore_attempted)
            restore_ok = if ($null -ne $clipboardState) { [bool]$clipboardState.restore_ok } else { $null }
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
# Claude Path B: retry проверки composer input

Финальный статус: ``$status``.

Этот пакет фиксирует повторную попытку починить UIA helper для проверки Claude Path B. Повтор был узким: подтвердить, что runner может дойти до выбора маршрута XIIGen/Smart Skills, найти composer и доказать наличие безопасного prompt в composer. Prompt не отправлялся: ``prompt_sent=false`` и ``enter_sent=false``.

Что реально подтверждено:

- Видимое окно Claude найдено.
- Redacted inventory окна записан.
- Redacted UIA summary записан до действия с route/composer.
- Если helper падает с ``Argument types do not match``, route/composer proof не достигнут.

Что не подтверждено при таком блокере:

- ``route_selected=false``
- ``composer_found=false``
- ``prompt_typed_or_pasted=false``
- ``prompt_confirmed_in_composer=false``
- ``prompt_sent=false``

Итог: этот retry не является proof маршрута, proof composer или proof ввода prompt, если последняя evidence показывает ошибку UIA helper до detection.

Следующий минимальный ремонт: сделать synthetic/minimal UIA helper test без Claude UI, чтобы изолированно проверить типы аргументов helper-вызова, и только потом снова запускать Claude composer detection.
"@
    Write-TextFile -Path (Join-Path $EvidenceDir "README.md") -Text ($readme + "`n")

    $redaction = @"
# Redaction note

Raw prompt and raw private UI text were not written to evidence. UIA evidence stores only safe XIIGen labels, category-only redactions, prompt hash/length, text hashes, control types, and bounds.

No screenshot was captured because the UIA logs were enough for this retry and a full screenshot could expose private chat or account text.
"@
    Write-TextFile -Path (Join-Path $EvidenceDir "redaction-note.md") -Text ($redaction + "`n")

    $actionLog.Insert(0, "# Action log") | Out-Null
    Write-TextFile -Path (Join-Path $EvidenceDir "action-log.md") -Text (($actionLog -join "`n") + "`n")
}
