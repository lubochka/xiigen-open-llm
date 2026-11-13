param(
    [string]$PacketDir = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\claude-path-b-route-locator-repair-20260628-134229",
    [int]$MaxTopWindows = 90,
    [int]$MaxChildWindows = 180,
    [int]$MaxUiaDepth = 18,
    [int]$MaxUiaNodes = 2200,
    [int]$MaxRecoveryActions = 7
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

if (-not ("ClaudeRouteLocatorRepair20260628134229.NativeMethods" -as [type])) {
    Add-Type -ReferencedAssemblies @("UIAutomationClient", "UIAutomationTypes", "WindowsBase") -TypeDefinition @"
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Automation;

namespace ClaudeRouteLocatorRepair20260628134229
{
    public sealed class RawWindowRecord
    {
        public long Hwnd;
        public int Pid;
        public string ClassName;
        public string Caption;
        public bool Visible;
    }

    public sealed class NodeRecord
    {
        public int Index;
        public int Depth;
        public string RuntimeId;
        public string Name;
        public string AutomationId;
        public string ClassName;
        public string ControlType;
        public bool Enabled;
        public bool Offscreen;
        public bool Focusable;
        public double X;
        public double Y;
        public double Width;
        public double Height;
        public bool HasValuePattern;
        public bool ValueReadOnly;
        public bool HasTextPattern;
        public bool HasInvokePattern;
        public bool HasSelectionItemPattern;
        public bool HasScrollItemPattern;
        public bool HasExpandCollapsePattern;
        public AutomationElement Element;
    }

    public sealed class ActionResult
    {
        public bool Ok;
        public string Method;
        public string Error;
    }

    public static class NativeMethods
    {
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError=true)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        private const int SW_RESTORE = 9;

        public static List<RawWindowRecord> EnumerateTopWindows(int limit)
        {
            var records = new List<RawWindowRecord>();
            EnumWindows((hWnd, lParam) =>
            {
                if (records.Count >= limit)
                {
                    return false;
                }
                if (!IsWindowVisible(hWnd))
                {
                    return true;
                }
                records.Add(CaptureWindow(hWnd));
                return true;
            }, IntPtr.Zero);
            return records;
        }

        public static List<RawWindowRecord> EnumerateChildWindows(IntPtr parent, int limit)
        {
            var records = new List<RawWindowRecord>();
            EnumChildWindows(parent, (hWnd, lParam) =>
            {
                if (records.Count >= limit)
                {
                    return false;
                }
                records.Add(CaptureWindow(hWnd));
                return true;
            }, IntPtr.Zero);
            return records;
        }

        public static bool BringToFront(IntPtr hWnd)
        {
            if (IsIconic(hWnd))
            {
                ShowWindow(hWnd, SW_RESTORE);
            }
            return SetForegroundWindow(hWnd);
        }

        public static AutomationElement ElementFromHandle(long hwnd)
        {
            return AutomationElement.FromHandle(new IntPtr(hwnd));
        }

        public static List<NodeRecord> WalkBounded(AutomationElement root, int maxDepth, int maxNodes)
        {
            var records = new List<NodeRecord>();
            var walker = TreeWalker.RawViewWalker;
            WalkNode(root, walker, 0, maxDepth, maxNodes, records);
            return records;
        }

        public static List<NodeRecord> ParentChain(AutomationElement element, int maxDepth)
        {
            var records = new List<NodeRecord>();
            var walker = TreeWalker.RawViewWalker;
            AutomationElement current = element;
            for (int i = 0; i < maxDepth && current != null; i++)
            {
                records.Add(CaptureNode(current, i, i));
                try
                {
                    current = walker.GetParent(current);
                }
                catch
                {
                    current = null;
                }
            }
            return records;
        }

        public static ActionResult TryBestAction(AutomationElement element)
        {
            var result = new ActionResult();
            if (element == null)
            {
                result.Ok = false;
                result.Method = "NONE";
                result.Error = "element is null";
                return result;
            }

            object pattern;
            try
            {
                if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out pattern))
                {
                    var expanded = (ExpandCollapsePattern)pattern;
                    if (expanded.Current.ExpandCollapseState == ExpandCollapseState.Collapsed)
                    {
                        expanded.Expand();
                        result.Ok = true;
                        result.Method = "ExpandCollapsePattern.Expand";
                        result.Error = null;
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Ok = false;
                result.Method = "ExpandCollapsePattern.Expand";
                result.Error = ex.Message;
            }

            try
            {
                if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out pattern))
                {
                    ((SelectionItemPattern)pattern).Select();
                    result.Ok = true;
                    result.Method = "SelectionItemPattern.Select";
                    result.Error = null;
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Ok = false;
                result.Method = "SelectionItemPattern.Select";
                result.Error = ex.Message;
            }

            try
            {
                if (element.TryGetCurrentPattern(InvokePattern.Pattern, out pattern))
                {
                    ((InvokePattern)pattern).Invoke();
                    result.Ok = true;
                    result.Method = "InvokePattern.Invoke";
                    result.Error = null;
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Ok = false;
                result.Method = "InvokePattern.Invoke";
                result.Error = ex.Message;
            }

            try
            {
                element.SetFocus();
                result.Ok = true;
                result.Method = "SetFocus";
                result.Error = null;
                return result;
            }
            catch (Exception ex)
            {
                result.Ok = false;
                result.Method = "SetFocus";
                result.Error = ex.Message;
                return result;
            }
        }

        private static void WalkNode(AutomationElement element, TreeWalker walker, int depth, int maxDepth, int maxNodes, List<NodeRecord> records)
        {
            if (element == null || records.Count >= maxNodes || depth > maxDepth)
            {
                return;
            }

            records.Add(CaptureNode(element, records.Count, depth));

            if (records.Count >= maxNodes || depth == maxDepth)
            {
                return;
            }

            AutomationElement child = null;
            try
            {
                child = walker.GetFirstChild(element);
            }
            catch
            {
                child = null;
            }

            while (child != null && records.Count < maxNodes)
            {
                WalkNode(child, walker, depth + 1, maxDepth, maxNodes, records);
                try
                {
                    child = walker.GetNextSibling(child);
                }
                catch
                {
                    child = null;
                }
            }
        }

        private static RawWindowRecord CaptureWindow(IntPtr hWnd)
        {
            int pid;
            GetWindowThreadProcessId(hWnd, out pid);

            string caption = "";
            int length = GetWindowTextLength(hWnd);
            if (length > 0)
            {
                var text = new StringBuilder(length + 1);
                GetWindowText(hWnd, text, text.Capacity);
                caption = text.ToString();
            }

            var className = new StringBuilder(512);
            GetClassName(hWnd, className, className.Capacity);

            return new RawWindowRecord
            {
                Hwnd = hWnd.ToInt64(),
                Pid = pid,
                ClassName = className.ToString(),
                Caption = caption,
                Visible = IsWindowVisible(hWnd)
            };
        }

        private static NodeRecord CaptureNode(AutomationElement element, int index, int depth)
        {
            var record = new NodeRecord();
            record.Index = index;
            record.Depth = depth;
            record.Element = element;

            try { record.RuntimeId = string.Join(".", element.GetRuntimeId()); } catch { record.RuntimeId = ""; }
            try { record.Name = element.Current.Name ?? ""; } catch { record.Name = ""; }
            try { record.AutomationId = element.Current.AutomationId ?? ""; } catch { record.AutomationId = ""; }
            try { record.ClassName = element.Current.ClassName ?? ""; } catch { record.ClassName = ""; }
            try { record.ControlType = element.Current.ControlType.ProgrammaticName ?? ""; } catch { record.ControlType = ""; }
            try { record.Enabled = element.Current.IsEnabled; } catch { record.Enabled = false; }
            try { record.Offscreen = element.Current.IsOffscreen; } catch { record.Offscreen = true; }
            try { record.Focusable = element.Current.IsKeyboardFocusable; } catch { record.Focusable = false; }

            try
            {
                Rect rect = element.Current.BoundingRectangle;
                record.X = rect.X;
                record.Y = rect.Y;
                record.Width = rect.Width;
                record.Height = rect.Height;
            }
            catch
            {
                record.X = 0;
                record.Y = 0;
                record.Width = 0;
                record.Height = 0;
            }

            object pattern;
            try
            {
                record.HasValuePattern = element.TryGetCurrentPattern(ValuePattern.Pattern, out pattern);
                if (record.HasValuePattern)
                {
                    try { record.ValueReadOnly = ((ValuePattern)pattern).Current.IsReadOnly; } catch { record.ValueReadOnly = true; }
                }
            }
            catch
            {
                record.HasValuePattern = false;
                record.ValueReadOnly = true;
            }

            try { record.HasTextPattern = element.TryGetCurrentPattern(TextPattern.Pattern, out pattern); } catch { record.HasTextPattern = false; }
            try { record.HasInvokePattern = element.TryGetCurrentPattern(InvokePattern.Pattern, out pattern); } catch { record.HasInvokePattern = false; }
            try { record.HasSelectionItemPattern = element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out pattern); } catch { record.HasSelectionItemPattern = false; }
            try { record.HasScrollItemPattern = element.TryGetCurrentPattern(ScrollItemPattern.Pattern, out pattern); } catch { record.HasScrollItemPattern = false; }
            try { record.HasExpandCollapsePattern = element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out pattern); } catch { record.HasExpandCollapsePattern = false; }

            return record;
        }
    }
}
"@
}

$LogsDir = Join-Path $PacketDir "logs"
$RunDir = Join-Path $PacketDir "run"
New-Item -ItemType Directory -Force -Path $PacketDir, $LogsDir, $RunDir | Out-Null

$Actions = New-Object System.Collections.Generic.List[string]
$script:RecoveryAttempts = @()
$script:RouteScans = @()

function Add-Action {
    param([string]$Text)
    $Actions.Add(("- {0} - {1}" -f (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssK"), $Text)) | Out-Null
}

function Write-Utf8File {
    param([string]$Path, [string]$Text)
    $encoding = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, ($Text -replace "`r`n", "`n"), $encoding)
}

function Write-JsonFile {
    param($Value, [string]$Path)
    $json = $Value | ConvertTo-Json -Depth 30
    Write-Utf8File -Path $Path -Text ($json + "`n")
}

function Get-Sha256Hex {
    param([string]$Text)
    if ([string]::IsNullOrEmpty($Text)) { return "" }
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    (($sha.ComputeHash($bytes) | ForEach-Object { $_.ToString("x2") }) -join "")
}

function Format-Hwnd {
    param([Int64]$Hwnd)
    "0x{0:X}" -f $Hwnd
}

function Get-ProcessNameSafe {
    param([int]$ProcessId)
    try { return (Get-Process -Id $ProcessId -ErrorAction Stop).ProcessName } catch { return "<unknown>" }
}

function Get-WindowBounds {
    param($RootNode)
    [pscustomobject]@{
        left = [double]$RootNode.X
        top = [double]$RootNode.Y
        right = [double]($RootNode.X + $RootNode.Width)
        bottom = [double]($RootNode.Y + $RootNode.Height)
        width = [double]$RootNode.Width
        height = [double]$RootNode.Height
    }
}

function Test-VisibleInsideWindow {
    param($Node, $Window)
    if (-not [bool]$Node.Enabled) { return $false }
    if ([bool]$Node.Offscreen) { return $false }
    if ([double]$Node.Width -lt 3 -or [double]$Node.Height -lt 3) { return $false }
    $right = [double]$Node.X + [double]$Node.Width
    $bottom = [double]$Node.Y + [double]$Node.Height
    if ($right -lt [double]$Window.left -or [double]$Node.X -gt [double]$Window.right) { return $false }
    if ($bottom -lt [double]$Window.top -or [double]$Node.Y -gt [double]$Window.bottom) { return $false }
    return $true
}

function Get-RelativePosition {
    param($Node, $Window)
    $cx = ([double]$Node.X + ([double]$Node.Width / 2.0) - [double]$Window.left) / [Math]::Max([double]$Window.width, 1.0)
    $cy = ([double]$Node.Y + ([double]$Node.Height / 2.0) - [double]$Window.top) / [Math]::Max([double]$Window.height, 1.0)
    [pscustomobject]@{
        center_x_ratio = [Math]::Round($cx, 3)
        center_y_ratio = [Math]::Round($cy, 3)
        horizontal_zone = if ($cx -lt 0.33) { "left" } elseif ($cx -lt 0.66) { "center" } else { "right" }
        vertical_zone = if ($cy -lt 0.33) { "top" } elseif ($cy -lt 0.66) { "middle" } else { "bottom" }
    }
}

function Get-LabelBucket {
    param([string]$Name)
    if ([string]::IsNullOrWhiteSpace($Name)) { return "empty" }
    if ($Name -match "(?i)XIIGen|Smart\s*Skills|llm_mvp_core|xiigen[-_ ]|xiigen\b") { return "safe_route_label" }
    if ($Name -match "(?i)project|projects") { return "safe_navigation_projects_label" }
    if ($Name -match "(?i)home|chats|recents|history|sidebar|menu|tools|connectors|extensions|skills|back|show more|view all") { return "safe_navigation_label" }
    if ($Name -match "(?i)sign in|log in|continue with|password|pin") { return "auth_related_label_redacted" }
    return ("redacted_label_present_{0}_chars" -f $Name.Trim().Length)
}

function Get-SafeLabel {
    param([string]$Name)
    if ([string]::IsNullOrWhiteSpace($Name)) { return "" }
    if ($Name -match "(?i)XIIGen|Smart\s*Skills|llm_mvp_core|xiigen[-_ ]|xiigen\b") {
        $trimmed = $Name.Trim()
        if ($trimmed.Length -le 180 -and $trimmed -notmatch "[@]") { return $trimmed }
    }
    return "[redacted UI label]"
}

function ConvertTo-SafeWindowRecord {
    param($Raw)
    $processName = Get-ProcessNameSafe -ProcessId ([int]$Raw.Pid)
    $title = [string]$Raw.Caption
    [pscustomobject]@{
        hwnd = Format-Hwnd -Hwnd ([Int64]$Raw.Hwnd)
        pid = [int]$Raw.Pid
        process = $processName
        class_hash = Get-Sha256Hex -Text ([string]$Raw.ClassName)
        title_safe = if ($processName -eq "claude" -or $title -eq "Claude") { "Claude" } elseif ([string]::IsNullOrWhiteSpace($title)) { "" } else { "[redacted window title]" }
        visible = [bool]$Raw.Visible
    }
}

function ConvertTo-SafeChildRecord {
    param($Raw)
    [pscustomobject]@{
        hwnd = Format-Hwnd -Hwnd ([Int64]$Raw.Hwnd)
        pid = [int]$Raw.Pid
        class_hash = Get-Sha256Hex -Text ([string]$Raw.ClassName)
        caption_safe = if ([string]::IsNullOrWhiteSpace([string]$Raw.Caption)) { "" } else { "[redacted child caption]" }
        visible = [bool]$Raw.Visible
    }
}

function ConvertTo-SafeNodeRecord {
    param($Node, $Window, [string]$Category, [int]$Score, [string[]]$Reasons, [bool]$IncludeParentChain)
    $parentChain = @()
    if ($IncludeParentChain) {
        $parentChain = @([ClaudeRouteLocatorRepair20260628134229.NativeMethods]::ParentChain($Node.Element, 8) | ForEach-Object {
            [pscustomobject]@{
                depth_from_target = [int]$_.Depth
                control_type = [string]$_.ControlType
                label_bucket = Get-LabelBucket -Name ([string]$_.Name)
                automation_id_hash = Get-Sha256Hex -Text ([string]$_.AutomationId)
                class_hash = Get-Sha256Hex -Text ([string]$_.ClassName)
            }
        })
    }

    [pscustomobject]@{
        index = [int]$Node.Index
        depth = [int]$Node.Depth
        runtime_id_hash = Get-Sha256Hex -Text ([string]$Node.RuntimeId)
        safe_label = Get-SafeLabel -Name ([string]$Node.Name)
        label_bucket = Get-LabelBucket -Name ([string]$Node.Name)
        control_type = [string]$Node.ControlType
        automation_id_hash = Get-Sha256Hex -Text ([string]$Node.AutomationId)
        class_hash = Get-Sha256Hex -Text ([string]$Node.ClassName)
        focusable = [bool]$Node.Focusable
        enabled = [bool]$Node.Enabled
        offscreen = [bool]$Node.Offscreen
        bounding_rect = [pscustomobject]@{
            left = [Math]::Round([double]$Node.X, 1)
            top = [Math]::Round([double]$Node.Y, 1)
            right = [Math]::Round(([double]$Node.X + [double]$Node.Width), 1)
            bottom = [Math]::Round(([double]$Node.Y + [double]$Node.Height), 1)
            width = [Math]::Round([double]$Node.Width, 1)
            height = [Math]::Round([double]$Node.Height, 1)
        }
        relative_position = Get-RelativePosition -Node $Node -Window $Window
        patterns = [pscustomobject]@{
            invoke = [bool]$Node.HasInvokePattern
            selection_item = [bool]$Node.HasSelectionItemPattern
            expand_collapse = [bool]$Node.HasExpandCollapsePattern
            scroll_item = [bool]$Node.HasScrollItemPattern
            text = [bool]$Node.HasTextPattern
            value = [bool]$Node.HasValuePattern
            value_readonly = [bool]$Node.ValueReadOnly
        }
        category = $Category
        score = [int]$Score
        evidence_reasons = @($Reasons)
        parent_chain_summary = $parentChain
    }
}

function Test-RouteName {
    param([string]$Name)
    if ([string]::IsNullOrWhiteSpace($Name)) { return $false }
    return ($Name -match "(?i)XIIGen|Smart\s*Skills|llm_mvp_core|xiigen[-_ ]|xiigen\b")
}

function Get-RouteScore {
    param($Node)
    $score = 0
    $name = [string]$Node.Name
    if ($name -match "(?i)XIIGen\s+Smart\s+Skills|Smart\s+Skills") { $score += 120 }
    if ($name -match "(?i)XIIGen") { $score += 80 }
    if ($name -match "(?i)llm_mvp_core") { $score += 45 }
    if ($name -match "(?i)xiigen[-_ ]|xiigen\b") { $score += 40 }
    if ([bool]$Node.HasInvokePattern) { $score += 20 }
    if ([bool]$Node.HasSelectionItemPattern) { $score += 15 }
    if ([bool]$Node.Focusable) { $score += 10 }
    if ([string]$Node.ControlType -match "ControlType\.(Button|ListItem|MenuItem|TabItem|Hyperlink|Group|Text)") { $score += 5 }
    return $score
}

function Get-RouteCandidates {
    param($Nodes, $Window)
    @($Nodes | Where-Object {
        (Test-VisibleInsideWindow -Node $_ -Window $Window) -and (Test-RouteName -Name ([string]$_.Name))
    } | Sort-Object @{ Expression = { -(Get-RouteScore -Node $_) } }, @{ Expression = { [int]$_.Depth } })
}

function Test-SafeRecoveryName {
    param([string]$Name)
    if ([string]::IsNullOrWhiteSpace($Name)) { return $false }
    if ($Name -match "(?i)send|submit|attach|upload|dictate|microphone|voice|delete|remove|sign out|log out|logout|billing|upgrade|subscribe|password|pin|message claude|ask claude") { return $false }
    return ($Name -match "(?i)\bhome\b|project|projects|recents|recent chats|history|chats|sidebar|navigation|menu|back|show more|view all|tools|connectors|extensions|skills|mcp")
}

function Get-RecoveryScore {
    param($Node)
    $score = 0
    $name = [string]$Node.Name
    if ($name -match "(?i)project|projects") { $score += 80 }
    if ($name -match "(?i)skills|tools|connectors|extensions|mcp") { $score += 70 }
    if ($name -match "(?i)sidebar|navigation|menu") { $score += 55 }
    if ($name -match "(?i)recents|history|chats|home|back") { $score += 35 }
    if ($name -match "(?i)show more|view all") { $score += 30 }
    if ([bool]$Node.HasExpandCollapsePattern) { $score += 20 }
    if ([bool]$Node.HasSelectionItemPattern) { $score += 15 }
    if ([bool]$Node.HasInvokePattern) { $score += 10 }
    if ([bool]$Node.Focusable) { $score += 5 }
    return $score
}

function Get-RecoveryCandidates {
    param($Nodes, $Window, [string[]]$AttemptedHashes)
    @($Nodes | Where-Object {
        (Test-VisibleInsideWindow -Node $_ -Window $Window) -and
        (Test-SafeRecoveryName -Name ([string]$_.Name)) -and
        ((Get-Sha256Hex -Text ([string]$_.RuntimeId)) -notin $AttemptedHashes) -and
        ([bool]$_.HasInvokePattern -or [bool]$_.HasSelectionItemPattern -or [bool]$_.HasExpandCollapsePattern -or [bool]$_.Focusable)
    } | Sort-Object @{ Expression = { -(Get-RecoveryScore -Node $_) } }, @{ Expression = { [int]$_.Depth } })
}

function Select-ClaudeWindow {
    param($Windows)
    $claudeWindows = @($Windows | Where-Object {
        $processName = Get-ProcessNameSafe -ProcessId ([int]$_.Pid)
        $caption = [string]$_.Caption
        $processName -eq "claude" -or $caption -eq "Claude"
    })
    if ($claudeWindows.Count -eq 0) { return $null }
    return @($claudeWindows | Sort-Object @{ Expression = { if (([string]$_.Caption) -eq "Claude") { 0 } else { 1 } } })[0]
}

function Invoke-Scan {
    param([Int64]$Hwnd, [string]$Phase)
    $rootElement = [ClaudeRouteLocatorRepair20260628134229.NativeMethods]::ElementFromHandle($Hwnd)
    $nodes = [ClaudeRouteLocatorRepair20260628134229.NativeMethods]::WalkBounded($rootElement, $MaxUiaDepth, $MaxUiaNodes)
    $window = Get-WindowBounds -RootNode $nodes[0]
    $routeCandidates = Get-RouteCandidates -Nodes $nodes -Window $window
    $safeRouteCandidates = @($routeCandidates | Select-Object -First 30 | ForEach-Object {
        ConvertTo-SafeNodeRecord -Node $_ -Window $window -Category "route_target_candidate" -Score (Get-RouteScore -Node $_) -Reasons @("safe route term matched in memory") -IncludeParentChain $true
    })
    $script:RouteScans += [pscustomobject]@{
        phase = $Phase
        nodes_seen = $nodes.Count
        route_candidate_count = $routeCandidates.Count
        route_candidates_sample = $safeRouteCandidates
        raw_private_text_written = $false
    }
    [pscustomobject]@{
        phase = $Phase
        nodes = $nodes
        window = $window
        route_candidates = $routeCandidates
        safe_route_candidates = $safeRouteCandidates
    }
}

function Write-PacketDocs {
    param(
        [string]$Status,
        [bool]$RouteVisible,
        [int]$CandidateCount,
        $SelectedRouteTargetSummary,
        [string]$NextStep,
        [string]$Blocker
    )

    $selectedText = if ($SelectedRouteTargetSummary) {
        "Выбранный route target: label=`"$($SelectedRouteTargetSummary.safe_label)`", control_type=`"$($SelectedRouteTargetSummary.control_type)`", rect=($($SelectedRouteTargetSummary.bounding_rect.left), $($SelectedRouteTargetSummary.bounding_rect.top), $($SelectedRouteTargetSummary.bounding_rect.width)x$($SelectedRouteTargetSummary.bounding_rect.height)), invoke=$($SelectedRouteTargetSummary.patterns.invoke), select=$($SelectedRouteTargetSummary.patterns.selection_item)."
    } else {
        "Стабильный route target не найден."
    }

    $readme = @"
# Claude Path B: route locator repair

Статус: `$Status`.

Этот пакет делает только ремонт/карту локатора маршрута Claude для XIIGen/Smart Skills. Prompt не вводился, не вставлялся и не отправлялся. Composer не картировался.

Что было сделано:

- выбран реальный локальный top-level window `Claude`;
- окно поднято на передний план безопасным действием `SetForegroundWindow`/restore;
- UIA обход выполнялся только от выбранного Claude HWND через bounded `TreeWalker.RawViewWalker`;
- `RootElement.FindAll` по всему desktop tree не использовался;
- если route target сразу не был виден, runner пробовал только безопасные навигационные действия: sidebar/menu/projects/tools/recents/back/show more/view all;
- сохранены только redacted UIA metadata и безопасные route labels, без screenshot и без приватного текста.

Итог:

- route_visible: `$RouteVisible`;
- candidate_count: `$CandidateCount`;
- $selectedText

Блокер: $Blocker

Следующий шаг: $NextStep

Что этим пакетом не заявляется:

- prompt typed/pasted/sent;
- composer mapping;
- Claude response;
- core/adapter correlation after submission;
- token savings;
- semantic quality;
- learnability/training;
- checkpoint promotion;
- cross-host completion.
"@

    $actionLog = @"
# Журнал действий

Prompt не вводился, не вставлялся и не отправлялся. Composer не картировался.

$($Actions -join "`n")

Итог: $Status. route_visible=$RouteVisible; candidate_count=$CandidateCount; prompt_typed_or_pasted=false; prompt_sent=false.
"@

    $redaction = @"
# Redaction note

Packet stores redacted UI Automation metadata only. Raw private Claude text, account text, chat content, prompt text, clipboard text and screenshots were not saved.

Exact labels are stored only when they match the safe route terms XIIGen, Smart Skills, llm_mvp_core, or xiigen. All other labels are bucketed or hashed.
"@

    Write-Utf8File -Path (Join-Path $PacketDir "README.md") -Text ($readme + "`n")
    Write-Utf8File -Path (Join-Path $PacketDir "action-log.md") -Text ($actionLog + "`n")
    Write-Utf8File -Path (Join-Path $PacketDir "redaction-note.md") -Text ($redaction + "`n")
}

$helperPattern = [pscustomobject]@{
    enum_windows_top_level = $false
    selected_claude_hwnd = $false
    enum_child_windows_selected_hwnd = $false
    handle_rooted_bounded_treewalker = $false
    whole_desktop_root_scan = $false
    broad_uia_descendant_collection = $false
    max_top_windows = $MaxTopWindows
    max_child_windows = $MaxChildWindows
    max_uia_depth = $MaxUiaDepth
    max_uia_nodes = $MaxUiaNodes
}

$status = "BLOCKED_RUNTIME_EXCEPTION"
$routeVisible = $false
$candidateCount = 0
$selectedRouteTargetSummary = $null
$blocker = $null
$nextStep = "Inspect logs and rerun only this route-locator repair after fixing the runtime blocker."
$attemptedHashes = @()

try {
    Add-Action "Runner started. Scope: route locator repair only; prompt input/send and composer mapping are forbidden."

    $topWindows = [ClaudeRouteLocatorRepair20260628134229.NativeMethods]::EnumerateTopWindows($MaxTopWindows)
    $helperPattern.enum_windows_top_level = $true
    Write-JsonFile -Value ([pscustomobject]@{
        redaction = "window titles redacted except generic Claude title"
        count = $topWindows.Count
        windows = @($topWindows | ForEach-Object { ConvertTo-SafeWindowRecord -Raw $_ })
    }) -Path (Join-Path $LogsDir "window-inventory.json")

    $selectedWindow = Select-ClaudeWindow -Windows $topWindows
    if ($null -eq $selectedWindow) {
        $status = "BLOCKED_CLAUDE_WINDOW_NOT_FOUND"
        $blocker = "No visible local Claude top-level window was found."
        $nextStep = "Open/unminimize Claude locally, then rerun route-locator repair."
        throw $blocker
    }

    $hwnd = [Int64]$selectedWindow.Hwnd
    $helperPattern.selected_claude_hwnd = $true
    [ClaudeRouteLocatorRepair20260628134229.NativeMethods]::BringToFront([IntPtr]$hwnd) | Out-Null
    Add-Action ("Selected Claude HWND {0}; pid={1}; process={2}." -f (Format-Hwnd -Hwnd $hwnd), [int]$selectedWindow.Pid, (Get-ProcessNameSafe -ProcessId ([int]$selectedWindow.Pid)))
    Start-Sleep -Milliseconds 700

    $childWindows = [ClaudeRouteLocatorRepair20260628134229.NativeMethods]::EnumerateChildWindows([IntPtr]$hwnd, $MaxChildWindows)
    $helperPattern.enum_child_windows_selected_hwnd = $true
    Write-JsonFile -Value ([pscustomobject]@{
        redaction = "child captions redacted"
        count = $childWindows.Count
        children = @($childWindows | ForEach-Object { ConvertTo-SafeChildRecord -Raw $_ })
    }) -Path (Join-Path $LogsDir "child-window-inventory.json")

    $initialScan = Invoke-Scan -Hwnd $hwnd -Phase "initial"
    $helperPattern.handle_rooted_bounded_treewalker = $true
    Add-Action ("Initial bounded selected-HWND TreeWalker scan: nodes={0}; route_candidates={1}." -f $initialScan.nodes.Count, $initialScan.route_candidates.Count)
    $currentScan = $initialScan

    $actionIndex = 0
    while ($currentScan.route_candidates.Count -eq 0 -and $actionIndex -lt $MaxRecoveryActions) {
        $recoveryCandidates = Get-RecoveryCandidates -Nodes $currentScan.nodes -Window $currentScan.window -AttemptedHashes $attemptedHashes
        if ($recoveryCandidates.Count -eq 0) {
            Add-Action "No unused safe navigation recovery candidate remained."
            break
        }

        $recoveryNode = $recoveryCandidates[0]
        $hash = Get-Sha256Hex -Text ([string]$recoveryNode.RuntimeId)
        $attemptedHashes += $hash
        $safeRecovery = ConvertTo-SafeNodeRecord -Node $recoveryNode -Window $currentScan.window -Category "safe_navigation_recovery_candidate" -Score (Get-RecoveryScore -Node $recoveryNode) -Reasons @("safe navigation term matched in memory") -IncludeParentChain $false
        $actionResult = [ClaudeRouteLocatorRepair20260628134229.NativeMethods]::TryBestAction($recoveryNode.Element)
        $script:RecoveryAttempts += [pscustomobject]@{
            action_index = $actionIndex
            attempted_candidate = $safeRecovery
            action_result = [pscustomobject]@{
                ok = [bool]$actionResult.Ok
                method = [string]$actionResult.Method
                error = [string]$actionResult.Error
            }
            prompt_typed_or_pasted = $false
            prompt_sent = $false
        }
        Add-Action ("Safe recovery action {0}: category={1}; control={2}; method={3}; ok={4}." -f $actionIndex, $safeRecovery.label_bucket, $safeRecovery.control_type, $actionResult.Method, $actionResult.Ok)
        Start-Sleep -Milliseconds 900
        $currentScan = Invoke-Scan -Hwnd $hwnd -Phase ("after_safe_recovery_action_{0}" -f $actionIndex)
        Add-Action ("Post-action scan {0}: nodes={1}; route_candidates={2}." -f $actionIndex, $currentScan.nodes.Count, $currentScan.route_candidates.Count)
        $actionIndex++
    }

    if ($currentScan.route_candidates.Count -gt 0) {
        $routeVisible = $true
        $candidateCount = $currentScan.route_candidates.Count
        $selectedRouteTargetSummary = @($currentScan.safe_route_candidates | Sort-Object @{ Expression = { -[int]$_.score } }, @{ Expression = { [int]$_.depth } })[0]
        $status = "CLAUDE_ROUTE_TARGET_MAPPED"
        $blocker = "none"
        $nextStep = "Use selected_route_target_summary as the stable route locator candidate in a separately authorized next packet; do not type, paste, map composer, or send from this packet."
    } else {
        $status = "BLOCKED_ROUTE_TARGET_NOT_FOUND"
        $blocker = "No visible XIIGen/Smart Skills route target was found after bounded selected-HWND scans and safe navigation recovery actions."
        $nextStep = "Manually expose the XIIGen/Smart Skills entrypoint in Claude or add a more specific safe navigation locator, then rerun route-locator repair only."
    }
} catch {
    if ($null -eq $blocker) {
        $blocker = $_.Exception.Message
    }
    if ($status -eq "BLOCKED_RUNTIME_EXCEPTION") {
        $nextStep = "Inspect logs and rerun only route-locator repair after fixing this runtime exception."
    }
    Add-Action ("Runner finished with status {0}: {1}" -f $status, $blocker)
} finally {
    Write-JsonFile -Value ([pscustomobject]@{
        helper_pattern = "selected Claude HWND -> bounded TreeWalker.RawViewWalker"
        scans = @($RouteScans)
        raw_private_text_written = $false
        prompt_typed_or_pasted = $false
        prompt_sent = $false
    }) -Path (Join-Path $LogsDir "route-candidates-redacted.json")

    Write-JsonFile -Value ([pscustomobject]@{
        action_count = $RecoveryAttempts.Count
        attempts = @($RecoveryAttempts)
        raw_private_text_written = $false
        prompt_typed_or_pasted = $false
        prompt_sent = $false
    }) -Path (Join-Path $LogsDir "safe-navigation-actions-redacted.json")

    Write-JsonFile -Value ([pscustomobject]@{
        route_visible = [bool]$routeVisible
        candidate_count = [int]$candidateCount
        selected_route_target_summary = $selectedRouteTargetSummary
        raw_private_text_written = $false
    }) -Path (Join-Path $LogsDir "selected-route-target-redacted.json")

    $result = [pscustomobject]@{
        status = $status
        route_visible = [bool]$routeVisible
        candidate_count = [int]$candidateCount
        selected_route_target_summary = $selectedRouteTargetSummary
        helper_pattern_used = $helperPattern
        safe_navigation_action_count = $RecoveryAttempts.Count
        prompt_typed_or_pasted = $false
        prompt_sent = $false
        enter_sent = $false
        send_button_clicked = $false
        composer_mapped = $false
        next_step = $nextStep
        blocker_if_any = $blocker
        deprecated_product_readiness_used = $false
        claims_not_made = @(
            "prompt typing",
            "prompt paste",
            "prompt submission",
            "composer mapping",
            "Claude response",
            "core or adapter correlation after submission",
            "token savings",
            "semantic quality",
            "learnability or training",
            "checkpoint promotion",
            "cross-host completion"
        )
        logs = @(
            "logs/window-inventory.json",
            "logs/child-window-inventory.json",
            "logs/route-candidates-redacted.json",
            "logs/safe-navigation-actions-redacted.json",
            "logs/selected-route-target-redacted.json"
        )
        redaction = [pscustomobject]@{
            screenshots_captured = $false
            raw_private_text_written = $false
            raw_prompt_written = $false
            clipboard_read = $false
            exact_safe_route_labels_only = $true
            note_file = "redaction-note.md"
        }
        timing = [pscustomobject]@{
            ended_local = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssK")
            ended_utc = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
        }
    }

    Write-JsonFile -Value $result -Path (Join-Path $PacketDir "result.json")
    Write-PacketDocs -Status $status -RouteVisible $routeVisible -CandidateCount $candidateCount -SelectedRouteTargetSummary $selectedRouteTargetSummary -NextStep $nextStep -Blocker $blocker
    Write-Output ("status={0}; route_visible={1}; candidate_count={2}; prompt_typed_or_pasted=false; prompt_sent=false" -f $status, $routeVisible, $candidateCount)
}
