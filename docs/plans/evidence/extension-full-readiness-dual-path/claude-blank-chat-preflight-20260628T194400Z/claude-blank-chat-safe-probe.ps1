param(
    [string]$OutDir = (Split-Path -Parent $MyInvocation.MyCommand.Path)
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class NativeWindowSafeProbe {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")] public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, UIntPtr dwExtraInfo);
}
"@

$Marker = "FRDP-P01-ROW12-CLAUDE-ANCHORED-DUAL-PATH-V1"
$ScenarioId = "frdp-p01-claude-anchored-dual-path"
$PairId = "frdp-p01-row12-claude-anchored-v1"
$ExpectedPromptSha256 = "f1c7299220fbf3c8027dc700a0980e0732761d9af028e9e3e6c8c28dc34feafa"
$Prompt = @"
FRDP-P01-ROW12-CLAUDE-ANCHORED-DUAL-PATH-V1
scenario_id=frdp-p01-claude-anchored-dual-path
pair_id=frdp-p01-row12-claude-anchored-v1

Safe redacted XIIGen smart-skills orchestration probe:
For a generic local extension orchestration task, select relevant XIIGen smart skills and return a concise JSON-like plan with:
- selected_skills
- rationale
- next_safe_action

Do not use secrets.
Do not claim tests, builds, token savings, training, checkpoint promotion, or product readiness.
Echo the marker, scenario_id, and pair_id in the response.
"@
$Prompt = ($Prompt -replace "`r`n", "`n").TrimEnd("`n")

function Get-Sha256Hex {
    param([Parameter(Mandatory)][string]$Text)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    $hash = $sha.ComputeHash($bytes)
    -join ($hash | ForEach-Object { $_.ToString("x2") })
}

function Write-JsonFile {
    param(
        [Parameter(Mandatory)]$Value,
        [Parameter(Mandatory)][string]$Path
    )
    $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Write-BlockerMarkdown {
    param(
        [Parameter(Mandatory)][string]$Reason,
        [string]$NextAction = "Открыть пустой авторизованный Claude chat вручную без приватного содержимого на экране, затем повторить этот bounded probe."
    )
    $text = @"
# Blocker: Claude blank chat preflight

subagent_id: CLAUDE-BLANK-CHAT-PREFLIGHT-16

## Итог

Пустой авторизованный Claude chat не подтвержден безопасно.

## Blocker

$Reason

## Safety

- no_secrets_captured: true
- private_content_captured: false
- credentials_typed: false
- screenshots_of_existing_private_chat: false
- prompt_submitted: false

## Следующее минимальное действие

$NextAction
"@
    Set-Content -LiteralPath (Join-Path $OutDir "blocker.ru.md") -Value $text -Encoding UTF8
}

function New-SafeReport {
    [ordered]@{
        schema = "frdp.claude.blank_chat_preflight.report.v1"
        subagent_id = "CLAUDE-BLANK-CHAT-PREFLIGHT-16"
        marker = $Marker
        scenario_id = $ScenarioId
        pair_id = $PairId
        prompt_sha256 = $ExpectedPromptSha256
        prompt_hash_verified_before_submit = $false
        blank_chat_verified = $false
        blank_chat_verification_method = $null
        prompt_submitted = $false
        prompt_submission_method = $null
        no_secrets_captured = $true
        private_content_captured = $false
        credentials_typed = $false
        existing_private_chat_screenshot_captured = $false
        screenshots = @()
        safe_actions_attempted = @()
        claude_window_found = $false
        new_chat_control_found = $false
        new_chat_control_invoked = $false
        keyboard_shortcut_attempted = $false
        composer_found = $false
        submit_control_found = $false
        blocker = $null
        next_smallest_action = $null
        non_claims = @(
            "product readiness",
            "token savings",
            "semantic quality",
            "training",
            "checkpoint promotion"
        )
    }
}

function Find-ExactName {
    param(
        [Parameter(Mandatory)]$Root,
        [Parameter(Mandatory)][string]$Name,
        $ControlType = $null
    )
    $nameCond = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::NameProperty,
        $Name
    )
    if ($null -ne $ControlType) {
        $typeCond = [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            $ControlType
        )
        $cond = [System.Windows.Automation.AndCondition]::new($nameCond, $typeCond)
        return $Root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $cond)
    }
    return $Root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $nameCond)
}

function Invoke-SafeControl {
    param([Parameter(Mandatory)]$Element)
    $pattern = $null
    if ($Element.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$pattern)) {
        $pattern.Invoke()
        return "invoke_pattern"
    }
    $rect = $Element.Current.BoundingRectangle
    if ($rect.Width -gt 0 -and $rect.Height -gt 0) {
        $x = [int]($rect.X + ($rect.Width / 2))
        $y = [int]($rect.Y + ($rect.Height / 2))
        [NativeWindowSafeProbe]::SetCursorPos($x, $y) | Out-Null
        Start-Sleep -Milliseconds 100
        [NativeWindowSafeProbe]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero)
        [NativeWindowSafeProbe]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero)
        return "center_click"
    }
    return $null
}

function Capture-ElementCrop {
    param(
        [Parameter(Mandatory)]$Element,
        [Parameter(Mandatory)][string]$FileName
    )
    $rect = $Element.Current.BoundingRectangle
    if ($rect.Width -le 0 -or $rect.Height -le 0) {
        return $null
    }
    $pad = 16
    $x = [Math]::Max(0, [int]($rect.X - $pad))
    $y = [Math]::Max(0, [int]($rect.Y - $pad))
    $w = [int]($rect.Width + ($pad * 2))
    $h = [int]($rect.Height + ($pad * 2))
    if ($w -lt 40 -or $h -lt 30 -or $w -gt 1400 -or $h -gt 500) {
        return $null
    }
    $bmp = [System.Drawing.Bitmap]::new($w, $h)
    $gfx = [System.Drawing.Graphics]::FromImage($bmp)
    try {
        $gfx.CopyFromScreen($x, $y, 0, 0, [System.Drawing.Size]::new($w, $h))
        $path = Join-Path $OutDir $FileName
        $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
        return $path
    }
    finally {
        $gfx.Dispose()
        $bmp.Dispose()
    }
}

$report = New-SafeReport
$report.prompt_hash_verified_before_submit = ((Get-Sha256Hex -Text $Prompt) -eq $ExpectedPromptSha256)

try {
    if (-not $report.prompt_hash_verified_before_submit) {
        throw "Prompt hash mismatch before submit; refusing to use prompt."
    }

    $claudeProcess = Get-Process |
        Where-Object { $_.ProcessName -eq "Claude" -and $_.MainWindowHandle -ne 0 } |
        Select-Object -First 1

    if ($null -eq $claudeProcess) {
        $reason = "No visible local Claude app window with process name Claude was found."
        $report.blocker = $reason
        $report.next_smallest_action = "Open authenticated Claude to a blank chat with no private content visible, then rerun the probe."
        Write-BlockerMarkdown -Reason $reason -NextAction $report.next_smallest_action
        return
    }

    $report.claude_window_found = $true
    $report.safe_actions_attempted += "found_visible_claude_process_without_logging_title"
    $handle = [IntPtr]$claudeProcess.MainWindowHandle
    [NativeWindowSafeProbe]::ShowWindow($handle, 9) | Out-Null
    [NativeWindowSafeProbe]::SetForegroundWindow($handle) | Out-Null
    Start-Sleep -Milliseconds 700

    $root = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
    if ($null -eq $root) {
        $reason = "Claude window handle exists, but UIAutomation root could not be obtained."
        $report.blocker = $reason
        $report.next_smallest_action = "Expose a UIAutomation-accessible blank Claude chat or open it manually, then rerun."
        Write-BlockerMarkdown -Reason $reason -NextAction $report.next_smallest_action
        return
    }

    $newChatNames = @("New Chat", "New chat", "New conversation", "Start new chat")
    foreach ($name in $newChatNames) {
        $candidate = Find-ExactName -Root $root -Name $name -ControlType ([System.Windows.Automation.ControlType]::Button)
        if ($null -ne $candidate) {
            $report.new_chat_control_found = $true
            $method = Invoke-SafeControl -Element $candidate
            if ($null -ne $method) {
                $report.new_chat_control_invoked = $true
                $report.safe_actions_attempted += "invoked_exact_safe_new_chat_control"
                Start-Sleep -Milliseconds 1200
                break
            }
        }
    }

    if (-not $report.new_chat_control_invoked) {
        $report.keyboard_shortcut_attempted = $true
        $report.safe_actions_attempted += "sent_ctrl_n_to_foreground_claude_window"
        [System.Windows.Forms.SendKeys]::SendWait("^n")
        Start-Sleep -Milliseconds 1200
    }

    $root = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
    $blankSignals = @(
        "How can I help you today?",
        "How can I help you?",
        "What can I help you with?",
        "What's on your mind?"
    )
    foreach ($signal in $blankSignals) {
        $foundSignal = Find-ExactName -Root $root -Name $signal
        if ($null -ne $foundSignal) {
            $report.blank_chat_verified = $true
            $report.blank_chat_verification_method = "exact_public_blank_signal"
            $report.safe_actions_attempted += "verified_blank_chat_by_public_signal"
            break
        }
    }

    $composerNames = @(
        "Message Claude",
        "Reply to Claude",
        "Write a message",
        "Write your prompt to Claude",
        "Message"
    )
    $composer = $null
    foreach ($name in $composerNames) {
        $composer = Find-ExactName -Root $root -Name $name -ControlType ([System.Windows.Automation.ControlType]::Edit)
        if ($null -ne $composer) { break }
    }
    if ($null -ne $composer) {
        $report.composer_found = $true
    }

    if ($report.blank_chat_verified -and $null -ne $composer) {
        $blankShot = Capture-ElementCrop -Element $composer -FileName "blank-composer-crop.png"
        if ($null -ne $blankShot) {
            $report.screenshots += "blank-composer-crop.png"
            $report.safe_actions_attempted += "captured_blank_composer_crop_only"
        }
    }

    if (-not $report.blank_chat_verified) {
        $reason = "Safe blank Claude chat state was not verified by exact public blank-chat signal after New Chat attempt/shortcut; refusing screenshot and prompt submit."
        $report.blocker = $reason
        $report.next_smallest_action = "Manually open an authenticated blank Claude chat with no private content visible, then rerun the probe."
        Write-BlockerMarkdown -Reason $reason -NextAction $report.next_smallest_action
        return
    }

    if ($null -eq $composer) {
        $reason = "Blank Claude signal was found, but a safe composer/edit control was not found; prompt was not submitted."
        $report.blocker = $reason
        $report.next_smallest_action = "Open blank Claude chat and expose/focus the composer, then rerun."
        Write-BlockerMarkdown -Reason $reason -NextAction $report.next_smallest_action
        return
    }

    $sendNames = @("Send Message", "Send message", "Send", "Submit")
    $sendControl = $null
    foreach ($name in $sendNames) {
        $sendControl = Find-ExactName -Root $root -Name $name -ControlType ([System.Windows.Automation.ControlType]::Button)
        if ($null -ne $sendControl) { break }
    }
    if ($null -ne $sendControl) {
        $report.submit_control_found = $true
    }

    if (-not $report.submit_control_found) {
        $reason = "Blank chat and composer were verified, but no exact safe submit button was found; prompt was not pasted or submitted."
        $report.blocker = $reason
        $report.next_smallest_action = "Expose a visible Send button in blank Claude chat or provide a known safe submit shortcut, then rerun."
        Write-BlockerMarkdown -Reason $reason -NextAction $report.next_smallest_action
        return
    }

    $focusPattern = $null
    if ($composer.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$focusPattern)) {
        $focusPattern.SetValue($Prompt)
        $report.prompt_submission_method = "value_pattern_then_exact_send_button"
    }
    else {
        $rect = $composer.Current.BoundingRectangle
        [NativeWindowSafeProbe]::SetCursorPos([int]($rect.X + ($rect.Width / 2)), [int]($rect.Y + ($rect.Height / 2))) | Out-Null
        [NativeWindowSafeProbe]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero)
        [NativeWindowSafeProbe]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero)
        Start-Sleep -Milliseconds 100
        [System.Windows.Forms.Clipboard]::SetText($Prompt)
        [System.Windows.Forms.SendKeys]::SendWait("^v")
        $report.prompt_submission_method = "clipboard_safe_prompt_then_exact_send_button"
    }
    Start-Sleep -Milliseconds 500

    $method2 = Invoke-SafeControl -Element $sendControl
    if ($null -eq $method2) {
        $reason = "Exact safe submit button existed, but invocation failed after safe prompt entry."
        $report.blocker = $reason
        $report.next_smallest_action = "Use a manually verified blank Claude chat and visible send control, then rerun."
        Write-BlockerMarkdown -Reason $reason -NextAction $report.next_smallest_action
        return
    }

    $report.prompt_submitted = $true
    $report.safe_actions_attempted += "submitted_safe_anchored_prompt_by_exact_send_control"
    Start-Sleep -Seconds 8

    $root = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
    $markerElement = Find-ExactName -Root $root -Name $Marker
    if ($null -ne $markerElement) {
        $markerShot = Capture-ElementCrop -Element $markerElement -FileName "submitted-marker-crop.png"
        if ($null -ne $markerShot) {
            $report.screenshots += "submitted-marker-crop.png"
            $report.safe_actions_attempted += "captured_safe_marker_crop_only"
        }
    }
}
catch {
    $reason = "Probe exception before safe completion: $($_.Exception.Message)"
    $report.blocker = $reason
    $report.next_smallest_action = "Inspect the local Claude UI manually for a blank authenticated chat, then rerun with the same safe constraints."
    Write-BlockerMarkdown -Reason $reason -NextAction $report.next_smallest_action
}
finally {
    if ($null -eq $report.blocker -and -not $report.prompt_submitted) {
        $report.blocker = "Prompt was not submitted; no additional blocker detail was set."
        $report.next_smallest_action = "Rerun with a manually verified blank Claude chat."
    }
    if ($null -eq $report.next_smallest_action) {
        $report.next_smallest_action = if ($report.prompt_submitted) {
            "Review safe captured evidence and decide whether a separate bounded result-wait probe is needed."
        } else {
            "Open blank authenticated Claude chat manually, then rerun."
        }
    }
    Write-JsonFile -Value $report -Path (Join-Path $OutDir "report.json")
    if ($report.blocker) {
        Write-JsonFile -Value ([ordered]@{
            schema = "frdp.claude.blank_chat_preflight.blocker.v1"
            subagent_id = "CLAUDE-BLANK-CHAT-PREFLIGHT-16"
            marker = $Marker
            scenario_id = $ScenarioId
            pair_id = $PairId
            prompt_sha256 = $ExpectedPromptSha256
            blank_chat_verified = $report.blank_chat_verified
            prompt_submitted = $report.prompt_submitted
            no_secrets_captured = $true
            private_content_captured = $false
            blocker = $report.blocker
            next_smallest_action = $report.next_smallest_action
        }) -Path (Join-Path $OutDir "blocker.json")
    }
    Write-Output "CLAUDE_BLANK_CHAT_PREFLIGHT_DONE blank_chat_verified=$($report.blank_chat_verified) prompt_submitted=$($report.prompt_submitted)"
}
