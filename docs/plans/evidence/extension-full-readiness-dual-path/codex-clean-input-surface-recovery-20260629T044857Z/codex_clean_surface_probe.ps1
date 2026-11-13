param(
    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

function Get-HashPrefix {
    param([string]$Text)
    if ([string]::IsNullOrEmpty($Text)) {
        return ""
    }
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    $hash = $sha.ComputeHash($bytes)
    return -join ($hash[0..3] | ForEach-Object { $_.ToString("x2") })
}

function Get-SafeText {
    param([string]$Text)
    if ([string]::IsNullOrWhiteSpace($Text)) {
        return ""
    }

    $safeExact = @(
        "Codex",
        "New chat",
        "New Chat",
        "New thread",
        "New Thread",
        "Send",
        "Settings",
        "History",
        "Search",
        "Message",
        "Ask",
        "Prompt"
    )

    if ($safeExact -contains $Text) {
        return $Text
    }

    if ($Text -match '^(Codex)(\s*[-|].*)?$') {
        return "Codex"
    }

    return "[redacted len=$($Text.Length) sha256_8=$(Get-HashPrefix $Text)]"
}

function Get-RectInfo {
    param($Rect)
    if ($null -eq $Rect -or $Rect.IsEmpty) {
        return $null
    }
    return [ordered]@{
        x = [math]::Round($Rect.X, 0)
        y = [math]::Round($Rect.Y, 0)
        width = [math]::Round($Rect.Width, 0)
        height = [math]::Round($Rect.Height, 0)
    }
}

function Test-PatternAvailable {
    param(
        [System.Windows.Automation.AutomationElement]$Element,
        [System.Windows.Automation.AutomationPattern]$Pattern
    )
    try {
        $obj = $null
        return $Element.TryGetCurrentPattern($Pattern, [ref]$obj)
    } catch {
        return $false
    }
}

$root = [System.Windows.Automation.AutomationElement]::RootElement
$windows = $root.FindAll(
    [System.Windows.Automation.TreeScope]::Children,
    [System.Windows.Automation.Condition]::TrueCondition
)

$candidateWindows = @()
foreach ($window in $windows) {
    $winPid = $window.Current.ProcessId
    $proc = Get-Process -Id $winPid -ErrorAction SilentlyContinue
    $name = [string]$window.Current.Name
    $className = [string]$window.Current.ClassName
    $processName = if ($proc) { [string]$proc.ProcessName } else { "" }
    $isCandidate = ($processName -match '(?i)codex') -or ($name -match '(?i)\bcodex\b') -or (($className -eq "Chrome_WidgetWin_1") -and ($name -match '(?i)\bcodex\b'))
    if ($isCandidate) {
        $candidateWindows += [pscustomobject]@{
            element = $window
            summary = [ordered]@{
                process = $processName
                pid = $winPid
                class = $className
                control_type = $window.Current.ControlType.ProgrammaticName
                name_safe = Get-SafeText $name
                rect = Get-RectInfo $window.Current.BoundingRectangle
                is_enabled = $window.Current.IsEnabled
                is_offscreen = $window.Current.IsOffscreen
            }
        }
    }
}

$windowReports = @()
foreach ($candidate in $candidateWindows) {
    $window = $candidate.element
    $descendants = $window.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.Condition]::TrueCondition
    )

    $controlsOfInterest = @()
    $inputCandidates = @()
    $safeButtons = @()
    $maxItems = [Math]::Min($descendants.Count, 2500)

    for ($i = 0; $i -lt $maxItems; $i++) {
        $el = $descendants.Item($i)
        $ct = $el.Current.ControlType.ProgrammaticName
        $name = [string]$el.Current.Name
        $automationId = [string]$el.Current.AutomationId
        $className = [string]$el.Current.ClassName
        $rect = $el.Current.BoundingRectangle
        $isFocusable = $el.Current.IsKeyboardFocusable
        $hasValue = Test-PatternAvailable $el ([System.Windows.Automation.ValuePattern]::Pattern)
        $hasText = Test-PatternAvailable $el ([System.Windows.Automation.TextPattern]::Pattern)

        $safeName = Get-SafeText $name
        $safeAutomationId = Get-SafeText $automationId
        $safeClassName = Get-SafeText $className

        $isInputLike = ($ct -eq "ControlType.Edit") -or $hasValue -or $hasText -or ($automationId -match '(?i)(input|prompt|message|textarea|composer|editor)')
        $hasUsableRect = ($null -ne $rect -and -not $rect.IsEmpty -and $rect.Width -gt 20 -and $rect.Height -gt 10)

        if ($isInputLike -and $hasUsableRect) {
            $inputCandidates += [ordered]@{
                index = $i
                control_type = $ct
                name_safe = $safeName
                automation_id_safe = $safeAutomationId
                class_safe = $safeClassName
                rect = Get-RectInfo $rect
                is_enabled = $el.Current.IsEnabled
                is_offscreen = $el.Current.IsOffscreen
                is_keyboard_focusable = $isFocusable
                value_pattern = $hasValue
                text_pattern = $hasText
            }
        }

        if (($ct -eq "ControlType.Button") -and ($name -match '(?i)^(New chat|New thread|Send|Settings|History|Search)$')) {
            $safeButtons += [ordered]@{
                index = $i
                name_safe = Get-SafeText $name
                rect = Get-RectInfo $rect
                is_enabled = $el.Current.IsEnabled
                is_offscreen = $el.Current.IsOffscreen
            }
        }

        if (($ct -eq "ControlType.Edit") -or ($ct -eq "ControlType.Document") -or ($ct -eq "ControlType.Button")) {
            $controlsOfInterest += [ordered]@{
                index = $i
                control_type = $ct
                name_safe = $safeName
                automation_id_safe = $safeAutomationId
                class_safe = $safeClassName
                rect = Get-RectInfo $rect
                is_enabled = $el.Current.IsEnabled
                is_offscreen = $el.Current.IsOffscreen
                is_keyboard_focusable = $isFocusable
                value_pattern = $hasValue
                text_pattern = $hasText
            }
        }
    }

    $windowReports += [ordered]@{
        window = $candidate.summary
        descendant_count_scanned = $maxItems
        safe_buttons = $safeButtons
        input_candidates = $inputCandidates
        controls_of_interest_sample = $controlsOfInterest | Select-Object -First 120
    }
}

$result = [ordered]@{
    generated_utc = [DateTime]::UtcNow.ToString("o")
    private_text_policy = "Names are exact only for small allowlisted UI labels; other UI text is redacted with length and short hash."
    candidate_window_count = $candidateWindows.Count
    clean_surface_verified = $false
    input_control_found = ($windowReports | ForEach-Object { $_.input_candidates } | Measure-Object).Count -gt 0
    prompt_submitted = $false
    screenshot_captured = $false
    windows = $windowReports
}

$json = $result | ConvertTo-Json -Depth 8
Set-Content -LiteralPath $OutputPath -Value $json -Encoding UTF8
$result | ConvertTo-Json -Depth 5
