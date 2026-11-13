param(
    [string]$RepoRoot = "C:\Projects\llm_mvp_core",
    [string]$PacketDir = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\claude-path-b-composer-locator-map-20260628-132937",
    [int]$MaxTopWindows = 90,
    [int]$MaxChildWindows = 180,
    [int]$MaxUiaDepth = 18,
    [int]$MaxUiaNodes = 1800
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

if (-not ("ClaudeComposerLocatorMap.NativeMethods" -as [type])) {
    Add-Type -ReferencedAssemblies @("UIAutomationClient", "UIAutomationTypes", "WindowsBase") -TypeDefinition @"
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Automation;

namespace ClaudeComposerLocatorMap
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

        public static ActionResult TryInvoke(AutomationElement element)
        {
            var result = new ActionResult();
            if (element == null)
            {
                result.Ok = false;
                result.Error = "element is null";
                return result;
            }

            try
            {
                object pattern;
                if (!element.TryGetCurrentPattern(InvokePattern.Pattern, out pattern))
                {
                    result.Ok = false;
                    result.Error = "InvokePattern not available";
                    return result;
                }

                ((InvokePattern)pattern).Invoke();
                result.Ok = true;
                result.Error = null;
                return result;
            }
            catch (Exception ex)
            {
                result.Ok = false;
                result.Error = ex.Message;
                return result;
            }
        }

        public static ActionResult TryFocus(AutomationElement element)
        {
            var result = new ActionResult();
            if (element == null)
            {
                result.Ok = false;
                result.Error = "element is null";
                return result;
            }

            try
            {
                element.SetFocus();
                result.Ok = true;
                result.Error = null;
                return result;
            }
            catch (Exception ex)
            {
                result.Ok = false;
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
    $json = $Value | ConvertTo-Json -Depth 20
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
    try {
        return (Get-Process -Id $ProcessId -ErrorAction Stop).ProcessName
    } catch {
        return "<unknown>"
    }
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
    if ([double]$Node.Width -lt 4 -or [double]$Node.Height -lt 4) { return $false }
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
    $horizontal = if ($cx -lt 0.33) { "left" } elseif ($cx -lt 0.66) { "center" } else { "right" }
    $vertical = if ($cy -lt 0.33) { "top" } elseif ($cy -lt 0.66) { "middle" } else { "bottom" }
    [pscustomobject]@{
        center_x_ratio = [Math]::Round($cx, 3)
        center_y_ratio = [Math]::Round($cy, 3)
        horizontal_zone = $horizontal
        vertical_zone = $vertical
    }
}

function ConvertTo-SafeWindowRecord {
    param($Raw)
    $processName = Get-ProcessNameSafe -ProcessId ([int]$Raw.Pid)
    $title = [string]$Raw.Caption
    $safeTitle = if ($processName -eq "claude" -or $title -eq "Claude") { "Claude" } elseif ([string]::IsNullOrWhiteSpace($title)) { "" } else { "[redacted window title; {0} chars]" -f $title.Length }
    [pscustomobject]@{
        hwnd = Format-Hwnd -Hwnd ([Int64]$Raw.Hwnd)
        pid = [int]$Raw.Pid
        process = $processName
        class_hash = Get-Sha256Hex -Text ([string]$Raw.ClassName)
        title_safe = $safeTitle
        visible = [bool]$Raw.Visible
    }
}

function ConvertTo-SafeChildRecord {
    param($Raw)
    [pscustomobject]@{
        hwnd = Format-Hwnd -Hwnd ([Int64]$Raw.Hwnd)
        pid = [int]$Raw.Pid
        class_hash = Get-Sha256Hex -Text ([string]$Raw.ClassName)
        caption_safe = if ([string]::IsNullOrWhiteSpace([string]$Raw.Caption)) { "" } else { "[redacted child caption; {0} chars]" -f ([string]$Raw.Caption).Length }
        visible = [bool]$Raw.Visible
    }
}

function Get-LabelBucket {
    param($Node)
    $name = [string]$Node.Name
    if ([string]::IsNullOrWhiteSpace($name)) { return "empty" }
    if ($name -match "(?i)XIIGen|Smart Skills|llm_mvp_core|xiigen[-_ ]|xiigen\b") { return "safe_route_label" }
    if ($name -match "(?i)message claude|ask claude|prompt|composer|сообщение|спрос") { return "generic_composer_label" }
    return ("redacted_label_present_{0}_chars" -f $name.Trim().Length)
}

function ConvertTo-SafeNodeRecord {
    param($Node, $Window, [string[]]$Category, [int]$Score, [string[]]$Reasons)
    [pscustomobject]@{
        index = [int]$Node.Index
        depth = [int]$Node.Depth
        runtime_id_hash = Get-Sha256Hex -Text ([string]$Node.RuntimeId)
        control_type = [string]$Node.ControlType
        label_bucket = Get-LabelBucket -Node $Node
        automation_id_hash = Get-Sha256Hex -Text ([string]$Node.AutomationId)
        class_hash = Get-Sha256Hex -Text ([string]$Node.ClassName)
        focusable = [bool]$Node.Focusable
        enabled = [bool]$Node.Enabled
        offscreen = [bool]$Node.Offscreen
        bounds = [pscustomobject]@{
            x = [Math]::Round([double]$Node.X, 1)
            y = [Math]::Round([double]$Node.Y, 1)
            width = [Math]::Round([double]$Node.Width, 1)
            height = [Math]::Round([double]$Node.Height, 1)
            left = [Math]::Round([double]$Node.X, 1)
            top = [Math]::Round([double]$Node.Y, 1)
            right = [Math]::Round(([double]$Node.X + [double]$Node.Width), 1)
            bottom = [Math]::Round(([double]$Node.Y + [double]$Node.Height), 1)
        }
        relative_position = Get-RelativePosition -Node $Node -Window $Window
        patterns = [pscustomobject]@{
            value = [bool]$Node.HasValuePattern
            value_readonly = [bool]$Node.ValueReadOnly
            text = [bool]$Node.HasTextPattern
            invoke = [bool]$Node.HasInvokePattern
            selection_item = [bool]$Node.HasSelectionItemPattern
            scroll_item = [bool]$Node.HasScrollItemPattern
            expand_collapse = [bool]$Node.HasExpandCollapsePattern
        }
        category = @($Category)
        score = [int]$Score
        evidence_reasons = @($Reasons)
    }
}

function Get-RouteCandidates {
    param($Nodes, $Window)
    @($Nodes | Where-Object {
        (Test-VisibleInsideWindow -Node $_ -Window $Window) -and
        ([string]$_.Name -match "(?i)XIIGen|Smart Skills|llm_mvp_core|xiigen[-_ ]|xiigen\b")
    } | Sort-Object `
        @{ Expression = { if ([bool]$_.HasInvokePattern) { 0 } else { 1 } } }, `
        @{ Expression = { if ([bool]$_.Focusable) { 0 } else { 1 } } }, `
        @{ Expression = { [int]$_.Depth } })
}

function Get-CandidateClassification {
    param($Node, $Window)

    $categories = New-Object System.Collections.Generic.List[string]
    $reasons = New-Object System.Collections.Generic.List[string]
    $score = 0

    if (-not (Test-VisibleInsideWindow -Node $Node -Window $Window)) {
        return [pscustomobject]@{ IsCandidate = $false; Score = 0; Categories = @(); Reasons = @() }
    }

    if ([string]$Node.Name -match "(?i)XIIGen|Smart Skills|llm_mvp_core|xiigen[-_ ]|xiigen\b") {
        return [pscustomobject]@{ IsCandidate = $false; Score = 0; Categories = @(); Reasons = @("route control, not composer") }
    }

    $control = [string]$Node.ControlType
    $excludedControl = $control -match "ControlType\.(Button|MenuItem|TabItem|TitleBar|Window|ScrollBar|Separator|Hyperlink|Image)"
    $relative = Get-RelativePosition -Node $Node -Window $Window
    $inLowerBand = ([double]$relative.center_y_ratio -ge 0.55)
    $wideEnough = ([double]$Node.Width -ge [Math]::Max(180.0, [double]$Window.width * 0.20))
    $reasonableHeight = ([double]$Node.Height -ge 18 -and [double]$Node.Height -le 260)
    $inputishControl = $control -match "ControlType\.(Edit|Document|Pane|Custom|Group|Text)"
    $explicitComposer = [string]$Node.Name -match "(?i)message claude|ask claude|prompt|composer|сообщение|спрос"
    $editableValue = [bool]$Node.HasValuePattern -and -not [bool]$Node.ValueReadOnly
    $textTarget = [bool]$Node.HasTextPattern -and [bool]$Node.Focusable

    if ($explicitComposer) {
        $score += 100
        $categories.Add("explicit_composer_label") | Out-Null
        $reasons.Add("generic composer/input label matched in memory and was redacted in logs") | Out-Null
    }
    if ($editableValue) {
        $score += 90
        $categories.Add("editable_value_pattern") | Out-Null
        $reasons.Add("ValuePattern exists and is not read-only") | Out-Null
    }
    if ($textTarget) {
        $score += 70
        $categories.Add("focusable_text_pattern") | Out-Null
        $reasons.Add("TextPattern exists on a focusable element") | Out-Null
    } elseif ([bool]$Node.HasTextPattern) {
        $score += 35
        $categories.Add("text_pattern_container") | Out-Null
        $reasons.Add("TextPattern exists") | Out-Null
    }
    if ([bool]$Node.Focusable) {
        $score += 45
        $categories.Add("focusable") | Out-Null
        $reasons.Add("keyboard focusable") | Out-Null
    }
    if ($inputishControl) {
        $score += 30
        $categories.Add("inputish_control_type") | Out-Null
        $reasons.Add(("control type is {0}" -f $control)) | Out-Null
    }
    if ($inLowerBand) {
        $score += 30
        $categories.Add("lower_composer_band") | Out-Null
        $reasons.Add("center is in the lower part of the Claude window") | Out-Null
    }
    if ($wideEnough) {
        $score += 20
        $categories.Add("wide_input_region") | Out-Null
        $reasons.Add("bounding rectangle is wide enough for a composer region") | Out-Null
    }
    if ($reasonableHeight) {
        $score += 10
        $categories.Add("reasonable_input_height") | Out-Null
        $reasons.Add("height is plausible for a composer target") | Out-Null
    }
    if ($excludedControl) {
        $score -= 120
        $categories.Add("excluded_click_control_type") | Out-Null
        $reasons.Add(("excluded non-input control type {0}" -f $control)) | Out-Null
    }
    if (-not $inLowerBand -and -not $explicitComposer) {
        $score -= 35
        $reasons.Add("not in lower composer band and has no generic composer label") | Out-Null
    }
    if (-not $wideEnough -and -not $explicitComposer -and -not $editableValue) {
        $score -= 20
        $reasons.Add("narrow target without editable evidence") | Out-Null
    }

    $isCandidate = ($score -ge 65) -and (-not $excludedControl) -and (
        $explicitComposer -or
        $editableValue -or
        $textTarget -or
        ($inputishControl -and $inLowerBand -and $wideEnough -and ([bool]$Node.Focusable -or [bool]$Node.HasTextPattern))
    )

    [pscustomobject]@{
        IsCandidate = [bool]$isCandidate
        Score = [int]$score
        Categories = @($categories)
        Reasons = @($reasons)
    }
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

function Write-PacketDocs {
    param(
        [string]$Status,
        [int]$CandidateCount,
        $SelectedTarget,
        [bool]$RouteSelected,
        [string]$NextStep,
        [string]$Blocker
    )

    $summaryLine = if ($SelectedTarget) {
        "Выбран лучший кандидат: index=$($SelectedTarget.index), control_type=$($SelectedTarget.control_type), category=$(@($SelectedTarget.category) -join ', '), score=$($SelectedTarget.score)."
    } else {
        "Безопасный composer target не выбран."
    }

    $readme = @"
# Claude Path B: mapping composer input target

Финальный статус: `$Status`.

Этот пакет закрывает только узкий discovery-шаг: найти безопасный target для будущего ввода в composer локального Claude UI после выбора XIIGen/Smart Skills route. Prompt не вводился, не вставлялся и не отправлялся.

Что выполнено:

- выбран реальный локальный Claude window через top-level `EnumWindows`;
- использован bounded helper pattern: selected Claude HWND, `EnumChildWindows` только от выбранного окна, handle-rooted `TreeWalker.RawViewWalker`;
- whole-tree `RootElement.FindAll` не использовался;
- XIIGen/Smart Skills route был найден в памяти по безопасному route-term и выбран/сфокусирован без prompt input;
- после route selection собраны только redacted candidate attributes: control type, bounds, focusable/enabled flags, available patterns, relative position, category and score;
- private UI text, raw prompt, screenshots, clipboard text and message content не сохранялись.

Итог mapping:

- route_selected: `$RouteSelected`;
- candidate_count: `$CandidateCount`;
- $summaryLine

Блокер: $Blocker

Следующий шаг: $NextStep

Что этим пакетом не заявляется:

- prompt был typed/pasted/sent;
- Claude response;
- core/adapter correlation after submission;
- token savings;
- semantic quality;
- learnability/training;
- checkpoint promotion;
- all-host readiness or product readiness.
"@

    $actionLog = @"
# Журнал действий

Этот журнал относится только к composer locator mapping. Prompt не вводился, не вставлялся и не отправлялся.

$($Actions -join "`n")

Итог: $Status. candidate_count=$CandidateCount; route_selected=$RouteSelected; prompt_typed_or_pasted=false; prompt_sent=false.
"@

    $redaction = @"
# Redaction note

Evidence packet stores redacted UI Automation metadata only. Raw private Claude text, chat content, account text, prompt text, clipboard text and screenshots were not saved.

Names were used in memory only for two bounded classifications:

- safe route detection: XIIGen / Smart Skills / llm_mvp_core terms;
- generic composer detection: composer/prompt/message style labels.

Candidate logs contain safe buckets instead of raw labels, plus hashes for runtime id, class and automation id.
"@

    Write-Utf8File -Path (Join-Path $PacketDir "README.md") -Text ($readme + "`n")
    Write-Utf8File -Path (Join-Path $PacketDir "action-log.md") -Text ($actionLog + "`n")
    Write-Utf8File -Path (Join-Path $PacketDir "redaction-note.md") -Text ($redaction + "`n")
}

function Ensure-PlaceholderLog {
    param([string]$RelativePath, [string]$Phase, [string]$Reason)
    $path = Join-Path $PacketDir $RelativePath
    if (Test-Path -LiteralPath $path) { return }
    Write-JsonFile -Value ([pscustomobject]@{
        phase = $Phase
        produced = $false
        reason = $Reason
        prompt_typed_or_pasted = $false
        prompt_sent = $false
        raw_private_text_written = $false
    }) -Path $path
}

$status = "BLOCKED_RUNTIME_EXCEPTION"
$routeSelected = $false
$routeAction = [pscustomobject]@{ ok = $false; method = "NONE"; error = "not attempted" }
$candidateCount = 0
$selectedTargetSummary = $null
$blocker = $null
$nextStep = "Inspect this packet and rerun only composer locator mapping after fixing the blocker."
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

try {
    Add-Action "Runner started. Scope: composer target mapping only; prompt input and send are forbidden."

    $topWindows = [ClaudeComposerLocatorMap.NativeMethods]::EnumerateTopWindows($MaxTopWindows)
    $helperPattern.enum_windows_top_level = $true
    $safeWindows = @($topWindows | ForEach-Object { ConvertTo-SafeWindowRecord -Raw $_ })
    Write-JsonFile -Value ([pscustomobject]@{
        redaction = "window titles redacted except generic Claude title"
        count = $safeWindows.Count
        windows = $safeWindows
    }) -Path (Join-Path $LogsDir "window-inventory.json")

    $selectedWindow = Select-ClaudeWindow -Windows $topWindows
    if ($null -eq $selectedWindow) {
        $status = "BLOCKED_CLAUDE_WINDOW_NOT_FOUND"
        $blocker = "No visible top-level Claude window was found by bounded EnumWindows."
        $nextStep = "Open/unlock Claude locally, then rerun only composer locator mapping."
        throw $blocker
    }

    $helperPattern.selected_claude_hwnd = $true
    $hwnd = [Int64]$selectedWindow.Hwnd
    [ClaudeComposerLocatorMap.NativeMethods]::BringToFront([IntPtr]$hwnd) | Out-Null
    Add-Action ("Selected Claude HWND {0}; process={1}; pid={2}." -f (Format-Hwnd -Hwnd $hwnd), (Get-ProcessNameSafe -ProcessId ([int]$selectedWindow.Pid)), [int]$selectedWindow.Pid)
    Start-Sleep -Milliseconds 600

    $childWindows = [ClaudeComposerLocatorMap.NativeMethods]::EnumerateChildWindows([IntPtr]$hwnd, $MaxChildWindows)
    $helperPattern.enum_child_windows_selected_hwnd = $true
    Write-JsonFile -Value ([pscustomobject]@{
        redaction = "child captions redacted"
        count = $childWindows.Count
        children = @($childWindows | ForEach-Object { ConvertTo-SafeChildRecord -Raw $_ })
    }) -Path (Join-Path $LogsDir "child-window-inventory.json")

    $rootElement = [ClaudeComposerLocatorMap.NativeMethods]::ElementFromHandle($hwnd)
    $nodesBefore = [ClaudeComposerLocatorMap.NativeMethods]::WalkBounded($rootElement, $MaxUiaDepth, $MaxUiaNodes)
    $helperPattern.handle_rooted_bounded_treewalker = $true
    $window = Get-WindowBounds -RootNode $nodesBefore[0]
    Add-Action ("Bounded UIA before route action: nodes={0}; window={1}x{2}." -f $nodesBefore.Count, [int]$window.width, [int]$window.height)

    $routeCandidates = Get-RouteCandidates -Nodes $nodesBefore -Window $window
    $safeRouteCandidates = @($routeCandidates | Select-Object -First 20 | ForEach-Object {
        ConvertTo-SafeNodeRecord -Node $_ -Window $window -Category @("route_candidate") -Score 0 -Reasons @("safe XIIGen/Smart Skills route term matched in memory")
    })
    Write-JsonFile -Value ([pscustomobject]@{
        phase = "before_route_select"
        helper_pattern = "selected Claude HWND -> bounded TreeWalker"
        route_candidate_count = $routeCandidates.Count
        route_candidates_sample = $safeRouteCandidates
        raw_private_text_written = $false
    }) -Path (Join-Path $LogsDir "route-candidates-redacted.json")

    if ($routeCandidates.Count -eq 0) {
        $status = "BLOCKED_ROUTE_TARGET_NOT_FOUND"
        $blocker = "No visible XIIGen/Smart Skills route candidate was found in the bounded selected-HWND UIA tree."
        $nextStep = "Bring Claude to the XIIGen/Smart Skills route manually or repair route locator before composer mapping."
        throw $blocker
    }

    $routeNode = $routeCandidates[0]
    if ([bool]$routeNode.HasInvokePattern) {
        $invoke = [ClaudeComposerLocatorMap.NativeMethods]::TryInvoke($routeNode.Element)
        $routeAction = [pscustomobject]@{ ok = [bool]$invoke.Ok; method = "InvokePattern"; error = $invoke.Error }
    } elseif ([bool]$routeNode.Focusable) {
        $focus = [ClaudeComposerLocatorMap.NativeMethods]::TryFocus($routeNode.Element)
        $routeAction = [pscustomobject]@{ ok = [bool]$focus.Ok; method = "SetFocus"; error = $focus.Error }
    } else {
        $routeAction = [pscustomobject]@{ ok = $false; method = "NONE"; error = "route candidate had no InvokePattern and was not focusable" }
    }

    $routeSelected = [bool]$routeAction.ok
    Add-Action ("Route selection/focus action: method={0}; ok={1}." -f $routeAction.method, $routeAction.ok)
    if (-not $routeSelected) {
        $status = "BLOCKED_ROUTE_TARGET_NOT_FOCUSED"
        $blocker = "XIIGen/Smart Skills route candidate was found but could not be selected/focused: $($routeAction.error)"
        $nextStep = "Repair only route focus/selection before composer target mapping."
        throw $blocker
    }

    Start-Sleep -Milliseconds 900
    $nodesAfter = [ClaudeComposerLocatorMap.NativeMethods]::WalkBounded($rootElement, $MaxUiaDepth, $MaxUiaNodes)
    $windowAfter = Get-WindowBounds -RootNode $nodesAfter[0]
    Add-Action ("Bounded UIA after route selection: nodes={0}." -f $nodesAfter.Count)

    $candidateRows = New-Object System.Collections.Generic.List[object]
    foreach ($node in $nodesAfter) {
        $classification = Get-CandidateClassification -Node $node -Window $windowAfter
        if ([bool]$classification.IsCandidate) {
            $candidateRows.Add((ConvertTo-SafeNodeRecord -Node $node -Window $windowAfter -Category $classification.Categories -Score $classification.Score -Reasons $classification.Reasons)) | Out-Null
        }
    }

    $safeCandidates = @($candidateRows | Sort-Object @{ Expression = { -[int]$_.score } }, @{ Expression = { [double]$_.relative_position.center_y_ratio } })
    $candidateCount = $safeCandidates.Count
    Write-JsonFile -Value ([pscustomobject]@{
        phase = "after_route_select_candidate_map"
        helper_pattern = "selected Claude HWND -> bounded TreeWalker"
        limits = [pscustomobject]@{ max_depth = $MaxUiaDepth; max_nodes = $MaxUiaNodes }
        counts = [pscustomobject]@{
            nodes_seen = $nodesAfter.Count
            candidate_count = $candidateCount
        }
        selection_rule = "highest score among redacted bottom-band input-like targets; buttons/menus/images excluded"
        candidates = $safeCandidates
        raw_private_text_written = $false
        prompt_typed_or_pasted = $false
        prompt_sent = $false
    }) -Path (Join-Path $LogsDir "composer-candidates-redacted.json")

    if ($candidateCount -eq 0) {
        $status = "BLOCKED_COMPOSER_TARGET_NOT_FOUND"
        $blocker = "Route was selected/focused, but no safe composer input-like target passed the bounded redacted candidate classifier."
        $nextStep = "Repair composer locator heuristics or manually expose the Claude composer, then rerun no-input mapping."
    } else {
        $selectedTargetSummary = $safeCandidates[0]
        $selectedRaw = @($nodesAfter | Where-Object {
            (Get-Sha256Hex -Text ([string]$_.RuntimeId)) -eq $selectedTargetSummary.runtime_id_hash
        })[0]

        $focusProbe = [ClaudeComposerLocatorMap.NativeMethods]::TryFocus($selectedRaw.Element)
        $selectedTargetSummary | Add-Member -NotePropertyName focus_probe -NotePropertyValue ([pscustomobject]@{
            attempted = $true
            ok = [bool]$focusProbe.Ok
            error = $focusProbe.Error
            prompt_typed_or_pasted = $false
            prompt_sent = $false
        })

        Write-JsonFile -Value ([pscustomobject]@{
            phase = "selected_target_focus_probe_no_input"
            selected_target = $selectedTargetSummary
            prompt_typed_or_pasted = $false
            prompt_sent = $false
            raw_private_text_written = $false
        }) -Path (Join-Path $LogsDir "selected-target-redacted.json")

        $strongMapping = (
            [int]$selectedTargetSummary.score -ge 125 -and
            (
                [bool]$selectedTargetSummary.patterns.value -or
                [bool]$selectedTargetSummary.patterns.text -or
                [bool]$selectedTargetSummary.focus_probe.ok -or
                @($selectedTargetSummary.category) -contains "explicit_composer_label"
            )
        )

        if ($strongMapping) {
            $status = "CLAUDE_COMPOSER_TARGET_MAPPED"
            $blocker = "нет"
            $nextStep = "Use this selected target in a separate explicitly authorized no-submit input proof; still do not Send without a separate work order."
        } else {
            $status = "BLOCKED_COMPOSER_TARGET_NOT_FOUND"
            $blocker = "A possible target was scored, but evidence was not strong enough to call it the correct composer target."
            $nextStep = "Repair composer target classifier or collect an additional bounded UIA focus/click mapping packet without typing."
        }
    }
} catch {
    if ($null -eq $blocker) {
        $blocker = $_.Exception.Message
    }
    if ($status -eq "BLOCKED_RUNTIME_EXCEPTION") {
        $nextStep = "Inspect action-log and logs, then rerun only the no-input locator mapping."
    }
    Add-Action ("Runner stopped with status {0}: {1}" -f $status, $blocker)
} finally {
    Ensure-PlaceholderLog -RelativePath "logs/composer-candidates-redacted.json" -Phase "not_run" -Reason $blocker
    Ensure-PlaceholderLog -RelativePath "logs/selected-target-redacted.json" -Phase "not_run" -Reason $blocker

    $result = [pscustomobject]@{
        status = $status
        host = "Claude"
        scope = "Claude Path B composer locator mapping only; no prompt input and no prompt submission."
        helper_pattern_used = $helperPattern
        route_selected = [bool]$routeSelected
        route_selection_action = $routeAction
        candidate_count = [int]$candidateCount
        selected_target_summary = $selectedTargetSummary
        prompt_typed_or_pasted = $false
        prompt_sent = $false
        enter_sent = $false
        send_button_clicked = $false
        next_step = $nextStep
        blocker_if_any = $blocker
        deprecated_product_readiness_used = $false
        claims_not_made = @(
            "prompt typed or pasted",
            "prompt submission",
            "Claude response",
            "core or adapter correlation after submission",
            "token savings",
            "semantic quality",
            "learnability or training",
            "checkpoint promotion",
            "all-host readiness",
            "product readiness"
        )
        redaction = [pscustomobject]@{
            screenshots_captured = $false
            raw_private_text_written = $false
            raw_prompt_written = $false
            previous_clipboard_text_read = $false
            note_file = "redaction-note.md"
        }
        logs = @(
            "logs/window-inventory.json",
            "logs/child-window-inventory.json",
            "logs/route-candidates-redacted.json",
            "logs/composer-candidates-redacted.json",
            "logs/selected-target-redacted.json"
        )
        timing = [pscustomobject]@{
            ended_local = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssK")
            ended_utc = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
        }
    }

    Write-JsonFile -Value $result -Path (Join-Path $PacketDir "result.json")
    Write-PacketDocs -Status $status -CandidateCount $candidateCount -SelectedTarget $selectedTargetSummary -RouteSelected $routeSelected -NextStep $nextStep -Blocker $blocker

    Write-Output ("status={0}; route_selected={1}; candidate_count={2}; prompt_typed_or_pasted=false; prompt_sent=false" -f $status, $routeSelected, $candidateCount)

    if ($status -notin @("CLAUDE_COMPOSER_TARGET_MAPPED", "BLOCKED_COMPOSER_TARGET_NOT_FOUND") -and -not $status.StartsWith("BLOCKED_")) {
        exit 1
    }
}
