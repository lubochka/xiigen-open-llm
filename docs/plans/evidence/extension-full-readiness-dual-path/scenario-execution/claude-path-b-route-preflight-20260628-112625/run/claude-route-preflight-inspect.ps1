$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$EvidenceDir = Split-Path -Parent $ScriptDir
$LogsDir = Join-Path $EvidenceDir "logs"
New-Item -ItemType Directory -Force -Path $LogsDir | Out-Null

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public struct RECT {
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}

public static class RoutePreflightWin32 {
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);
}
"@

try {
    Add-Type -AssemblyName UIAutomationClient
    Add-Type -AssemblyName UIAutomationTypes
    $uiaAvailable = $true
} catch {
    $uiaAvailable = $false
    $uiaLoadError = $_.Exception.Message
}

$startedLocal = Get-Date -Format "yyyy-MM-ddTHH:mm:ssK"
$startedUtc = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")

$claudeProcesses = Get-Process |
    ForEach-Object {
        $path = ""
        try {
            $path = $_.Path
        } catch {
            $path = ""
        }

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

$commandDiscovery = Get-Command claude, claude-code -ErrorAction SilentlyContinue |
    Select-Object Name, Source, Version, CommandType

$appxDiscovery = Get-AppxPackage *Claude* |
    Select-Object Name, PackageFullName, InstallLocation, Status

$windowInventory = @()
foreach ($process in $claudeProcesses) {
    $hwnd = [IntPtr]$process.MainWindowHandle
    $rectObject = $null
    $visible = $false
    $minimized = $false
    if ($process.MainWindowHandle -ne 0) {
        $rect = New-Object RECT
        [void][RoutePreflightWin32]::GetWindowRect($hwnd, [ref]$rect)
        $rectObject = [ordered]@{
            left = $rect.Left
            top = $rect.Top
            right = $rect.Right
            bottom = $rect.Bottom
            width = $rect.Right - $rect.Left
            height = $rect.Bottom - $rect.Top
        }
        $visible = [RoutePreflightWin32]::IsWindowVisible($hwnd)
        $minimized = [RoutePreflightWin32]::IsIconic($hwnd)
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

$uiaSummaries = @()
if ($uiaAvailable) {
    foreach ($window in $windowInventory | Where-Object { $_.has_main_window_handle }) {
        $categories = New-Object "System.Collections.Generic.HashSet[string]"
        $safeHits = New-Object "System.Collections.Generic.List[object]"
        $errorMessage = $null
        $descendantCount = 0

        try {
            $element = [System.Windows.Automation.AutomationElement]::FromHandle([IntPtr]$window.hwnd_decimal)
            $all = $element.FindAll(
                [System.Windows.Automation.TreeScope]::Descendants,
                [System.Windows.Automation.Condition]::TrueCondition
            )
            $descendantCount = $all.Count
            $limit = [Math]::Min($all.Count, 2500)

            for ($i = 0; $i -lt $limit; $i++) {
                $current = $all.Item($i).Current
                $name = $current.Name
                if ([string]::IsNullOrWhiteSpace($name)) {
                    continue
                }

                $matchedCategory = $null
                if ($name -match "(?i)xiigen|smart\s*skills") {
                    [void]$categories.Add("xiigen_route_visible")
                    $matchedCategory = "xiigen_route_visible"
                } elseif ($name -match "(?i)\bextension\b|\bplugin\b|\bmcp\b|\bskill\b|\bproject\b") {
                    [void]$categories.Add("route_related_label_visible")
                    $matchedCategory = "route_related_label_visible"
                } elseif ($name -match "(?i)sign\s*in|log\s*in|continue\s+with|password|pin") {
                    [void]$categories.Add("auth_prompt_or_login_visible")
                    $matchedCategory = "auth_prompt_or_login_visible"
                } elseif ($name -match "(?i)message\s+claude|ask\s+claude|new\s+chat|start\s+chat|chat") {
                    [void]$categories.Add("chat_surface_visible")
                    $matchedCategory = "chat_surface_visible"
                } elseif ($name -match "(?i)\bClaude\b") {
                    [void]$categories.Add("claude_label_visible")
                    $matchedCategory = "claude_label_visible"
                }

                if ($matchedCategory -ne $null) {
                    $safeLabel = "[redacted UI label; category-only match]"
                    if ($matchedCategory -eq "xiigen_route_visible" -and $name.Length -le 120 -and $name -notmatch "[@\\/:]") {
                        $safeLabel = $name
                    }

                    $safeHits.Add([ordered]@{
                        category = $matchedCategory
                        control_type = $current.ControlType.ProgrammaticName
                        safe_label = $safeLabel
                    }) | Out-Null
                }
            }
        } catch {
            $errorMessage = $_.Exception.Message
        }

        $uiaSummaries += [ordered]@{
            pid = $window.pid
            title = $window.title
            descendant_count = $descendantCount
            categories = @($categories)
            safe_hits = @($safeHits | Select-Object -First 40)
            error = $errorMessage
        }
    }
} else {
    $uiaSummaries += [ordered]@{
        error = "UIAutomation assemblies unavailable: $uiaLoadError"
    }
}

$allCategories = @($uiaSummaries | ForEach-Object { $_.categories } | ForEach-Object { $_ }) | Sort-Object -Unique
$appFound = (($appxDiscovery | Measure-Object).Count -gt 0) -or (($claudeProcesses | Measure-Object).Count -gt 0)
$windowFound = (($windowInventory | Where-Object { $_.has_main_window_handle -and $_.title -match "Claude" } | Measure-Object).Count -gt 0)
$sessionAuthStateVisible = $allCategories -contains "auth_prompt_or_login_visible" -or $allCategories -contains "chat_surface_visible"
$xiigenRouteVisibleOrProven = $allCategories -contains "xiigen_route_visible"

$status = "BLOCKED_XIIGEN_ROUTE_NOT_VISIBLE"
$blocker = "Claude app/window was found, but XIIGen route/entrypoint was not visible or proven by this no-send inspection."
if (-not $appFound) {
    $status = "BLOCKED_APP_NOT_FOUND"
    $blocker = "No Claude app/package/process was found by local discovery."
} elseif (-not $windowFound) {
    $status = "BLOCKED_WINDOW_NOT_FOUND"
    $blocker = "Claude process/package exists, but no titled local Claude window was found."
} elseif ($xiigenRouteVisibleOrProven) {
    $status = "CLAUDE_ROUTE_READY"
    $blocker = $null
}

$summary = [ordered]@{
    started_local = $startedLocal
    started_utc = $startedUtc
    scope = "Claude Path B route-preflight only; no prompt submission."
    app_found = $appFound
    window_found = $windowFound
    session_auth_state_visible = $sessionAuthStateVisible
    xiigen_route_visible_or_proven = $xiigenRouteVisibleOrProven
    prompt_sent = $false
    screenshots_captured = @()
    status = $status
    blocker_if_any = $blocker
    next_step = if ($status -eq "CLAUDE_ROUTE_READY") { "Proceed only to a separately authorized no-secret Claude Path B scenario execution." } else { "Identify a safe Claude XIIGen route/entrypoint surface or install/select the XIIGen route before any prompt submission." }
    deprecated_product_readiness_used = $false
    command_discovery_count = ($commandDiscovery | Measure-Object).Count
    appx_discovery_count = ($appxDiscovery | Measure-Object).Count
    process_count = ($claudeProcesses | Measure-Object).Count
    window_count = ($windowInventory | Where-Object { $_.has_main_window_handle } | Measure-Object).Count
    ui_categories_seen = $allCategories
    redaction_note = "No raw accessibility names were stored except exact XIIGen labels if found; other UI matches are category-only redacted. No screenshot was captured because private chat/account content could be exposed; route visibility is proven only by redacted UI Automation category/label evidence."
}

$windowInventory | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $LogsDir "window-inventory.json") -Encoding UTF8
$commandDiscovery | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $LogsDir "command-discovery.json") -Encoding UTF8
$appxDiscovery | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $LogsDir "appx-discovery.json") -Encoding UTF8
$uiaSummaries | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $LogsDir "uia-redacted-summary.json") -Encoding UTF8
$summary | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $LogsDir "inspection-summary.json") -Encoding UTF8

$summary | ConvertTo-Json -Depth 10
