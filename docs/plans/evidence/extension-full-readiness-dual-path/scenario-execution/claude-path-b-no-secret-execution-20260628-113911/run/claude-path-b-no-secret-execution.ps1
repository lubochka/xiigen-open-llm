$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$EvidenceDir = Split-Path -Parent $ScriptDir
$RepoRoot = (Resolve-Path (Join-Path $EvidenceDir "..\..\..\..\..\..")).Path
$LogsDir = Join-Path $EvidenceDir "logs"
New-Item -ItemType Directory -Force -Path $LogsDir | Out-Null

$ScenarioPath = Join-Path $RepoRoot "docs\plans\evidence\extension-full-readiness-dual-path\scenario-prep\claude-full-extension-runtime-20260627-223159\scenario.json"
$PreflightPath = Join-Path $RepoRoot "docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\claude-path-b-route-preflight-20260628-112625\result.json"

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public struct RECT {
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}

public static class ClaudePathBWin32 {
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}

public static class ClaudePathBInput {
    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT {
        public uint type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion {
        [FieldOffset(0)]
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    const uint INPUT_KEYBOARD = 1;
    const uint KEYEVENTF_KEYUP = 0x0002;
    const uint KEYEVENTF_UNICODE = 0x0004;

    public static uint SendUnicodeText(string text) {
        uint sent = 0;
        foreach (char ch in text) {
            INPUT down = new INPUT();
            down.type = INPUT_KEYBOARD;
            down.U.ki.wVk = 0;
            down.U.ki.wScan = ch;
            down.U.ki.dwFlags = KEYEVENTF_UNICODE;

            INPUT up = new INPUT();
            up.type = INPUT_KEYBOARD;
            up.U.ki.wVk = 0;
            up.U.ki.wScan = ch;
            up.U.ki.dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP;

            INPUT[] inputs = new INPUT[] { down, up };
            sent += SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
        return sent;
    }

    public static uint SendEnter() {
        INPUT down = new INPUT();
        down.type = INPUT_KEYBOARD;
        down.U.ki.wVk = 0x0D;

        INPUT up = new INPUT();
        up.type = INPUT_KEYBOARD;
        up.U.ki.wVk = 0x0D;
        up.U.ki.dwFlags = KEYEVENTF_KEYUP;

        INPUT[] inputs = new INPUT[] { down, up };
        return SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
    }
}
"@

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

function Write-JsonFile {
    param(
        [Parameter(Mandatory = $true)] [object] $Value,
        [Parameter(Mandatory = $true)] [string] $Path
    )
    $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Get-Sha256Hex {
    param([Parameter(Mandatory = $true)] [string] $Text)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    ($sha.ComputeHash($bytes) | ForEach-Object { $_.ToString("x2") }) -join ""
}

function Get-ElementBounds {
    param($Current)
    try {
        $r = $Current.BoundingRectangle
        return [ordered]@{
            x = [int]$r.X
            y = [int]$r.Y
            width = [int]$r.Width
            height = [int]$r.Height
            left = [int]$r.Left
            top = [int]$r.Top
            right = [int]$r.Right
            bottom = [int]$r.Bottom
        }
    } catch {
        return $null
    }
}

function Get-RedactedUiaSummary {
    param(
        [Parameter(Mandatory = $true)] $RootElement,
        [Parameter(Mandatory = $true)] [string] $Prompt,
        [int] $Limit = 4000
    )

    $categories = New-Object "System.Collections.Generic.HashSet[string]"
    $safeHits = New-Object "System.Collections.Generic.List[object]"
    $inputCandidates = New-Object "System.Collections.Generic.List[object]"
    $sendCandidates = New-Object "System.Collections.Generic.List[object]"
    $promptVisibleByValueOrName = $false
    $promptPrefix = if ($Prompt.Length -gt 24) { $Prompt.Substring(0, 24) } else { $Prompt }
    $descendantCount = 0
    $errorMessage = $null

    try {
        $all = $RootElement.FindAll(
            [System.Windows.Automation.TreeScope]::Descendants,
            [System.Windows.Automation.Condition]::TrueCondition
        )
        $descendantCount = $all.Count
        $actualLimit = [Math]::Min($all.Count, $Limit)

        for ($i = 0; $i -lt $actualLimit; $i++) {
            $element = $all.Item($i)
            $current = $element.Current
            $name = $current.Name
            $controlType = $current.ControlType.ProgrammaticName
            $bounds = Get-ElementBounds -Current $current
            $focusable = $current.IsKeyboardFocusable
            $enabled = $current.IsEnabled
            $offscreen = $current.IsOffscreen

            if (-not [string]::IsNullOrEmpty($name)) {
                if ($name.Contains($Prompt) -or ($promptPrefix.Length -gt 0 -and $name.Contains($promptPrefix))) {
                    $promptVisibleByValueOrName = $true
                }

                $matchedCategory = $null
                if ($name -match "(?i)xiigen|smart\s*skills") {
                    [void]$categories.Add("xiigen_route_visible")
                    $matchedCategory = "xiigen_route_visible"
                } elseif ($name -match "(?i)\bextension\b|\bplugin\b|\bmcp\b|\bskill\b|\bproject\b") {
                    [void]$categories.Add("route_related_label_visible")
                    $matchedCategory = "route_related_label_visible"
                } elseif ($name -match "(?i)\b(sign\s*in|log\s*in|continue\s+with|password|pin)\b") {
                    [void]$categories.Add("auth_prompt_or_login_visible")
                    if (-not $offscreen -and $enabled -and $name -match "(?i)\b(sign\s*in|log\s*in|continue\s+with|password|pin)\b") {
                        [void]$categories.Add("credential_prompt_visible")
                    }
                    $matchedCategory = "auth_prompt_or_login_visible"
                } elseif ($name -match "(?i)message\s+claude|ask\s+claude|new\s+chat|start\s+chat|chat") {
                    [void]$categories.Add("chat_surface_visible")
                    $matchedCategory = "chat_surface_visible"
                } elseif ($name -match "(?i)\bClaude\b") {
                    [void]$categories.Add("claude_label_visible")
                    $matchedCategory = "claude_label_visible"
                } elseif ($name -match "(?i)\bstop\b|generating|retry|continue|copy") {
                    [void]$categories.Add("response_or_generation_status_visible")
                    $matchedCategory = "response_or_generation_status_visible"
                }

                if ($matchedCategory -ne $null) {
                    $safeLabel = "[redacted UI label; category-only match]"
                    if (
                        ($matchedCategory -eq "xiigen_route_visible" -or $matchedCategory -eq "response_or_generation_status_visible") -and
                        $name.Length -le 120 -and
                        $name -notmatch "[@\\/:]"
                    ) {
                        $safeLabel = $name
                    }

                    $safeHits.Add([ordered]@{
                        category = $matchedCategory
                        control_type = $controlType
                        safe_label = $safeLabel
                        focusable = $focusable
                        enabled = $enabled
                        offscreen = $offscreen
                        bounds = $bounds
                    }) | Out-Null
                }
            }

            $valuePattern = $null
            $hasValuePattern = $false
            try {
                $valuePattern = $element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
                $hasValuePattern = ($null -ne $valuePattern)
            } catch {
                $hasValuePattern = $false
            }

            if ($hasValuePattern) {
                try {
                    $currentValue = $valuePattern.Current.Value
                    if ($currentValue -and ($currentValue.Contains($Prompt) -or ($promptPrefix.Length -gt 0 -and $currentValue.Contains($promptPrefix)))) {
                        $promptVisibleByValueOrName = $true
                    }
                } catch {
                }
            }

            $looksLikeComposer = (
                $controlType -eq "ControlType.Edit" -or
                ($focusable -and -not $offscreen -and -not [string]::IsNullOrWhiteSpace($name) -and $name -match "(?i)message\s+claude|ask\s+claude|prompt|composer")
            )

            if ($looksLikeComposer -and $enabled -and -not $offscreen -and $bounds -ne $null -and $bounds.width -gt 80 -and $bounds.height -gt 10) {
                $inputCandidates.Add([ordered]@{
                    index = $i
                    control_type = $controlType
                    focusable = $focusable
                    enabled = $enabled
                    offscreen = $offscreen
                    has_value_pattern = $hasValuePattern
                    value_is_readonly = if ($hasValuePattern) { $valuePattern.Current.IsReadOnly } else { $null }
                    safe_label_category = if ($name -match "(?i)message\s+claude|ask\s+claude") { "generic_claude_composer_label" } elseif ([string]::IsNullOrWhiteSpace($name)) { "empty_label" } else { "redacted_nonempty_label" }
                    bounds = $bounds
                }) | Out-Null
            }

            if ($controlType -eq "ControlType.Button" -and $enabled -and -not $offscreen -and -not [string]::IsNullOrWhiteSpace($name) -and $name -match "(?i)^send$|send message|submit|arrow") {
                $sendCandidates.Add([ordered]@{
                    index = $i
                    control_type = $controlType
                    safe_label_category = "generic_send_button_label"
                    bounds = $bounds
                }) | Out-Null
            }
        }
    } catch {
        $errorMessage = $_.Exception.Message
    }

    return [ordered]@{
        descendant_count = $descendantCount
        categories = @($categories)
        safe_hits = @($safeHits | Select-Object -First 80)
        input_candidates = @($inputCandidates | Sort-Object { $_.bounds.top } -Descending | Select-Object -First 20)
        send_candidates = @($sendCandidates | Select-Object -First 20)
        prompt_visible_by_value_or_name = $promptVisibleByValueOrName
        error = $errorMessage
    }
}

function Find-XiigenRouteButton {
    param([Parameter(Mandatory = $true)] $RootElement)

    $all = $RootElement.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.Condition]::TrueCondition
    )
    $candidates = New-Object "System.Collections.Generic.List[object]"
    $actualLimit = [Math]::Min($all.Count, 4000)

    for ($i = 0; $i -lt $actualLimit; $i++) {
        $element = $all.Item($i)
        $current = $element.Current
        $name = $current.Name
        if ([string]::IsNullOrWhiteSpace($name)) {
            continue
        }
        $controlType = $current.ControlType.ProgrammaticName
        $bounds = Get-ElementBounds -Current $current
        if ($controlType -ne "ControlType.Button" -or -not $current.IsEnabled -or $current.IsOffscreen -or $null -eq $bounds) {
            continue
        }
        if ($name -notmatch "(?i)xiigen|smart\s*skills") {
            continue
        }

        $invokePattern = $null
        $hasInvokePattern = $false
        try {
            $invokePattern = $element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
            $hasInvokePattern = ($null -ne $invokePattern)
        } catch {
            $hasInvokePattern = $false
        }

        $score = 0
        if ($name -match "(?i)XIIGen Smart Skills UI proof run") { $score += 20 }
        if ($name -match "(?i)^Idle\s+") { $score += 5 }
        if ($hasInvokePattern) { $score += 3 }

        $candidates.Add([ordered]@{
            score = $score
            element = $element
            invoke_pattern = $invokePattern
            has_invoke_pattern = $hasInvokePattern
            safe_label = if ($name.Length -le 120 -and $name -notmatch "[@\\/:]") { $name } else { "[redacted XIIGen route label]" }
            bounds = $bounds
        }) | Out-Null
    }

    return @($candidates | Sort-Object -Property score -Descending | Select-Object -First 1)[0]
}

function Find-ComposerElement {
    param([Parameter(Mandatory = $true)] $RootElement)

    $all = $RootElement.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.Condition]::TrueCondition
    )
    $candidates = New-Object "System.Collections.Generic.List[object]"
    $actualLimit = [Math]::Min($all.Count, 4000)

    for ($i = 0; $i -lt $actualLimit; $i++) {
        $element = $all.Item($i)
        $current = $element.Current
        $name = $current.Name
        $controlType = $current.ControlType.ProgrammaticName
        $bounds = Get-ElementBounds -Current $current
        $focusable = $current.IsKeyboardFocusable
        $enabled = $current.IsEnabled
        $offscreen = $current.IsOffscreen
        if (-not $enabled -or $offscreen -or $null -eq $bounds -or $bounds.width -lt 80 -or $bounds.height -lt 10) {
            continue
        }

        $valuePattern = $null
        $hasValuePattern = $false
        try {
            $valuePattern = $element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
            $hasValuePattern = ($null -ne $valuePattern)
        } catch {
            $hasValuePattern = $false
        }

        $looksLikeComposer = (
            $controlType -eq "ControlType.Edit" -or
            ($focusable -and -not [string]::IsNullOrWhiteSpace($name) -and $name -match "(?i)message\s+claude|ask\s+claude|prompt|composer")
        )
        if (-not $looksLikeComposer) {
            continue
        }

        $score = 0
        if ($controlType -eq "ControlType.Edit") { $score += 5 }
        if ($hasValuePattern -and -not $valuePattern.Current.IsReadOnly) { $score += 5 }
        if ($focusable) { $score += 3 }
        if (-not [string]::IsNullOrWhiteSpace($name) -and $name -match "(?i)message\s+claude|ask\s+claude") { $score += 4 }
        if ($bounds.top -gt 0) { $score += [Math]::Min(5, [int]($bounds.top / 300)) }

        $candidates.Add([ordered]@{
            score = $score
            element = $element
            control_type = $controlType
            has_value_pattern = $hasValuePattern
            value_pattern = $valuePattern
            bounds = $bounds
            focusable = $focusable
        }) | Out-Null
    }

    return @($candidates | Sort-Object -Property score -Descending | Select-Object -First 1)[0]
}

function Get-RecentCorrelationFiles {
    param([Parameter(Mandatory = $true)] [datetime] $Since)

    $roots = @(
        (Join-Path $RepoRoot "artifacts"),
        (Join-Path $RepoRoot "docs\plans\evidence"),
        "C:\Projects\claude-xiigen-smart-skills"
    )

    $items = @()
    foreach ($root in $roots) {
        if (-not (Test-Path -LiteralPath $root)) {
            continue
        }

        $items += Get-ChildItem -LiteralPath $root -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object {
                $_.FullName -notlike "$EvidenceDir*" -and
                $_.LastWriteTime -ge $Since -and
                $_.Extension -match "(?i)^\.(json|jsonl|log|txt)$" -and
                $_.Name -notmatch "(?i)runner|script|README|action-log|verification" -and
                $_.Name -match "(?i)xiigen|claude|core|adapter|trace|request|response|correlation"
            } |
            Select-Object -First 80 |
            ForEach-Object {
                [ordered]@{
                    path = $_.FullName.Replace($RepoRoot + "\", "")
                    last_write_time = $_.LastWriteTime.ToString("yyyy-MM-ddTHH:mm:ssK")
                    length = $_.Length
                }
            }
    }

    return @($items)
}

$startedAt = Get-Date
$startedLocal = $startedAt.ToString("yyyy-MM-ddTHH:mm:ssK")
$startedUtc = $startedAt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
$promptSent = $false
$promptTypedOrSet = $false
$status = "FAILED_UNINITIALIZED"
$blocker = $null
$responseOrStatusSeen = $null
$coreOrAdapterCorrelationSeen = $false
$coreOrAdapterCorrelationFiles = @()
$uiActionMethod = $null
$routeSelectionAction = $null
$routeSelectionSafeLabel = $null
$sendInputUnits = 0
$enterSent = $false
$preSubmitSummary = $null
$postSubmitSummary = $null

$scenario = [System.IO.File]::ReadAllText($ScenarioPath, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
$preflight = [System.IO.File]::ReadAllText($PreflightPath, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
$prompt = [string]$scenario.exact_prompt
$promptHash = Get-Sha256Hex -Text $prompt

try {
    $claudeProcesses = Get-Process |
        ForEach-Object {
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

    $windowInventory = @()
    foreach ($process in $claudeProcesses) {
        $rectObject = $null
        $visible = $false
        $minimized = $false
        if ($process.MainWindowHandle -ne 0) {
            $hwnd = [IntPtr]$process.MainWindowHandle
            $rect = New-Object RECT
            [void][ClaudePathBWin32]::GetWindowRect($hwnd, [ref]$rect)
            $rectObject = [ordered]@{
                left = $rect.Left
                top = $rect.Top
                right = $rect.Right
                bottom = $rect.Bottom
                width = $rect.Right - $rect.Left
                height = $rect.Bottom - $rect.Top
            }
            $visible = [ClaudePathBWin32]::IsWindowVisible($hwnd)
            $minimized = [ClaudePathBWin32]::IsIconic($hwnd)
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

    $targetWindow = @(
        $windowInventory |
            Where-Object { $_.has_main_window_handle -and $_.visible -and -not $_.minimized -and $_.title -match "(?i)Claude" } |
            Sort-Object { $_.rect.width * $_.rect.height } -Descending
    )[0]

    if ($null -eq $targetWindow) {
        $status = "BLOCKED_TARGET_WINDOW_NOT_FOUND"
        $blocker = "No visible, non-minimized Claude window with a Claude title was found."
    } else {
        $hwnd = [IntPtr]$targetWindow.hwnd_decimal
        $rootElement = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
        $preSummary = Get-RedactedUiaSummary -RootElement $rootElement -Prompt $prompt
        Write-JsonFile -Value $preSummary -Path (Join-Path $LogsDir "uia-before-action-redacted-summary.json")

        $routeVisible = @($preSummary.categories) -contains "xiigen_route_visible"
        $loginVisible = @($preSummary.categories) -contains "credential_prompt_visible"

        if (-not $routeVisible) {
            $status = "BLOCKED_XIIGEN_ROUTE_NOT_VISIBLE"
            $blocker = "Claude window was found, but redacted UI Automation did not show the XIIGen/Smart Skills route in this attempt."
        } elseif ($loginVisible) {
            $status = "BLOCKED_LOGIN_REQUIRED"
            $blocker = "Claude window showed a login/password/PIN-related UI category; stopped before credentials."
        } else {
            [void][ClaudePathBWin32]::ShowWindow($hwnd, 9)
            [void][ClaudePathBWin32]::SetForegroundWindow($hwnd)
            Start-Sleep -Milliseconds 600

            $rootElement = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
            $routeButton = Find-XiigenRouteButton -RootElement $rootElement
            if ($null -ne $routeButton) {
                $routeSelectionSafeLabel = $routeButton.safe_label
                if ($routeButton.has_invoke_pattern) {
                    $routeButton.invoke_pattern.Invoke()
                    $routeSelectionAction = "UIAutomation InvokePattern on XIIGen route button"
                } else {
                    $routeButton.element.SetFocus()
                    $routeSelectionAction = "UIAutomation SetFocus on XIIGen route button"
                }
                Start-Sleep -Seconds 2
                $rootElement = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
                $afterRouteSummary = Get-RedactedUiaSummary -RootElement $rootElement -Prompt $prompt
                Write-JsonFile -Value $afterRouteSummary -Path (Join-Path $LogsDir "uia-after-route-select-redacted-summary.json")
            } else {
                $routeSelectionAction = "No invokable XIIGen route button found; route was visible in UIA summary only"
            }

            $composer = Find-ComposerElement -RootElement $rootElement
            if ($null -eq $composer) {
                $status = "BLOCKED_INPUT_CONTROL_NOT_FOUND"
                $blocker = "XIIGen route was visible, but no safe enabled on-screen composer/input candidate was found through UI Automation."
            } else {
                $composer.element.SetFocus()
                Start-Sleep -Milliseconds 500

                $setValueSucceeded = $false
                if ($composer.has_value_pattern -and -not $composer.value_pattern.Current.IsReadOnly) {
                    try {
                        $composer.value_pattern.SetValue($prompt)
                        $setValueSucceeded = $true
                        $uiActionMethod = "UIAutomation ValuePattern.SetValue"
                    } catch {
                        $setValueSucceeded = $false
                    }
                }

                if (-not $setValueSucceeded) {
                    $sendInputUnits = [ClaudePathBInput]::SendUnicodeText($prompt)
                    $uiActionMethod = "Win32 SendInput Unicode"
                }
                Start-Sleep -Milliseconds 1200

                $rootElement = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
                $preSubmitSummary = Get-RedactedUiaSummary -RootElement $rootElement -Prompt $prompt
                Write-JsonFile -Value $preSubmitSummary -Path (Join-Path $LogsDir "uia-before-submit-redacted-summary.json")

                $promptTypedOrSet = [bool]$preSubmitSummary.prompt_visible_by_value_or_name
                if (-not $promptTypedOrSet) {
                    $status = "CLAUDE_EXECUTION_SUBMIT_STATUS"
                    $blocker = "Prompt input was attempted with $uiActionMethod, but redacted UI Automation could not confirm the exact prompt in the composer; stopped before submit."
                } else {
                    [void][ClaudePathBInput]::SendEnter()
                    $enterSent = $true
                    $promptSent = $true
                    Start-Sleep -Seconds 18

                    $rootElement = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
                    $postSubmitSummary = Get-RedactedUiaSummary -RootElement $rootElement -Prompt $prompt
                    Write-JsonFile -Value $postSubmitSummary -Path (Join-Path $LogsDir "uia-after-submit-redacted-summary.json")

                    $postCategories = @($postSubmitSummary.categories)
                    if ($postCategories -contains "response_or_generation_status_visible") {
                        $status = "CLAUDE_EXECUTION_RESPONSE_CAPTURED"
                        $responseOrStatusSeen = "Redacted UI Automation saw response/generation-status category after submit."
                    } else {
                        $status = "CLAUDE_EXECUTION_SUBMIT_STATUS"
                        $responseOrStatusSeen = "Prompt submit was sent, but redacted UI Automation did not see a response/generation-status category within the wait window."
                    }
                }
            }
        }
    }
} catch {
    $status = "FAILED_EXCEPTION"
    $blocker = $_.Exception.Message
} finally {
    $coreOrAdapterCorrelationFiles = @(Get-RecentCorrelationFiles -Since $startedAt)
    if ($null -eq $coreOrAdapterCorrelationFiles) {
        $coreOrAdapterCorrelationFiles = @()
    }
    $coreOrAdapterCorrelationSeen = (($coreOrAdapterCorrelationFiles | Measure-Object).Count -gt 0)
    Write-JsonFile -Value $coreOrAdapterCorrelationFiles -Path (Join-Path $LogsDir "recent-correlation-file-candidates.json")
}

$endedAt = Get-Date
$redactedEvidenceFiles = @("logs/window-inventory.json", "logs/uia-before-action-redacted-summary.json")
if ($preSubmitSummary) {
    $redactedEvidenceFiles += "logs/uia-before-submit-redacted-summary.json"
}
if ($postSubmitSummary) {
    $redactedEvidenceFiles += "logs/uia-after-submit-redacted-summary.json"
}
if (Test-Path -LiteralPath (Join-Path $LogsDir "uia-after-route-select-redacted-summary.json")) {
    $redactedEvidenceFiles += "logs/uia-after-route-select-redacted-summary.json"
}
$redactedEvidenceFiles += "logs/recent-correlation-file-candidates.json"

$result = [ordered]@{
    status = $status
    scenario_id = "frdp-p01-claude-full-extension-entrypoint"
    prompt_source = [ordered]@{
        source_file = "docs/plans/evidence/extension-full-readiness-dual-path/scenario-prep/claude-full-extension-runtime-20260627-223159/scenario.json"
        source_field = "exact_prompt"
        prompt_sha256 = $promptHash
        prompt_length_chars = $prompt.Length
        fallback_prompt_used = $false
    }
    route_entrypoint_used = [ordered]@{
        preflight_result = "docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/claude-path-b-route-preflight-20260628-112625/result.json"
        preflight_status = $preflight.status
        preflight_xiigen_route_visible_or_proven = $preflight.xiigen_route_visible_or_proven
        current_window_found = ($null -ne $targetWindow)
        current_xiigen_route_visible = if ($preSummary) { @($preSummary.categories) -contains "xiigen_route_visible" } else { $false }
        route_selection_action = $routeSelectionAction
        route_selection_safe_label = $routeSelectionSafeLabel
        ui_action_method = $uiActionMethod
    }
    prompt_typed_or_set = $promptTypedOrSet
    prompt_sent = $promptSent
    enter_sent = $enterSent
    send_input_units = $sendInputUnits
    response_or_status_seen = $responseOrStatusSeen
    core_or_adapter_correlation_seen = $coreOrAdapterCorrelationSeen
    core_or_adapter_correlation_files = $coreOrAdapterCorrelationFiles
    screenshots_or_redacted_logs = $redactedEvidenceFiles
    blocker_if_any = $blocker
    next_step = if ($status -eq "CLAUDE_EXECUTION_RESPONSE_CAPTURED") {
        "Review the redacted Claude UI status and any correlation candidates; if correlation is absent, run a separate narrow trace/correlation collection work order."
    } elseif ($promptSent) {
        "Run a narrow response-readback/correlation attempt only if safe; do not upgrade to product readiness."
    } else {
        "Repair the exact blocker before another prompt attempt; do not send a second prompt under this work order."
    }
    deprecated_product_readiness_used = $false
    claims_not_made = @(
        "token savings",
        "semantic quality",
        "learnability/training",
        "checkpoint promotion",
        "all-host readiness",
        "old deprecated count wording"
    )
    timing = [ordered]@{
        started_local = $startedLocal
        started_utc = $startedUtc
        ended_local = $endedAt.ToString("yyyy-MM-ddTHH:mm:ssK")
        ended_utc = $endedAt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    }
    redaction_note = "No screenshots were captured. Raw UI text was not written except exact allowed XIIGen/Smart Skills labels and generic status labels; other UI Automation matches are category-only redacted. No password, PIN, account secret, private chat text, or clipboard data was intentionally captured."
}

Write-JsonFile -Value $result -Path (Join-Path $EvidenceDir "result.json")
$result | ConvertTo-Json -Depth 12
