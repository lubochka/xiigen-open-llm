param(
    [string]$RepoRoot = "C:\Projects\llm_mvp_core",
    [string]$ScenarioPath = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-prep\claude-full-extension-runtime-20260627-223159\scenario.json",
    [string]$PreflightPath = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\claude-path-b-route-preflight-20260628-112625\result.json",
    [int]$MaxTopWindows = 90,
    [int]$MaxChildWindows = 180,
    [int]$MaxUiaDepth = 18,
    [int]$MaxUiaNodes = 1800
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms

if (-not ("ClaudeComposerProof.NativeMethods" -as [type])) {
    Add-Type -ReferencedAssemblies @("UIAutomationClient", "UIAutomationTypes", "WindowsBase") -TypeDefinition @"
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;

namespace ClaudeComposerProof
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
        public int ValueLength;
        public bool ValueContainsPrompt;
        public bool ValueContainsPrefix;
        public bool HasTextPattern;
        public bool TextContainsPrompt;
        public bool TextContainsPrefix;
        public bool HasInvokePattern;
        public bool NameContainsPrompt;
        public bool NameContainsPrefix;
        public AutomationElement Element;
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

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError=true)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        [DllImport("user32.dll", SetLastError=true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private const int SW_RESTORE = 9;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const ushort VK_CONTROL = 0x11;
        private const ushort VK_V = 0x56;

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

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

        public static void ClickPoint(int x, int y)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
        }

        public static bool SendCtrlV()
        {
            INPUT[] inputs = new INPUT[4];
            inputs[0] = KeyInput(VK_CONTROL, 0);
            inputs[1] = KeyInput(VK_V, 0);
            inputs[2] = KeyInput(VK_V, KEYEVENTF_KEYUP);
            inputs[3] = KeyInput(VK_CONTROL, KEYEVENTF_KEYUP);
            return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT))) == inputs.Length;
        }

        private static INPUT KeyInput(ushort vk, uint flags)
        {
            var input = new INPUT();
            input.type = INPUT_KEYBOARD;
            input.U.ki.wVk = vk;
            input.U.ki.wScan = 0;
            input.U.ki.dwFlags = flags;
            input.U.ki.time = 0;
            input.U.ki.dwExtraInfo = UIntPtr.Zero;
            return input;
        }

        private static RawWindowRecord CaptureWindow(IntPtr hWnd)
        {
            int pid;
            GetWindowThreadProcessId(hWnd, out pid);

            return new RawWindowRecord
            {
                Hwnd = hWnd.ToInt64(),
                Pid = pid,
                ClassName = ReadClassName(hWnd),
                Caption = ReadCaption(hWnd),
                Visible = IsWindowVisible(hWnd)
            };
        }

        private static string ReadCaption(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            if (length <= 0)
            {
                return "";
            }

            var builder = new StringBuilder(length + 1);
            GetWindowText(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }

        private static string ReadClassName(IntPtr hWnd)
        {
            var builder = new StringBuilder(256);
            GetClassName(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }
    }

    public static class UiaWalker
    {
        public static List<NodeRecord> Walk(AutomationElement root, int maxDepth, int maxNodes, string prompt, string promptPrefix)
        {
            var records = new List<NodeRecord>();
            var stack = new Stack<Tuple<AutomationElement, int>>();
            stack.Push(new Tuple<AutomationElement, int>(root, 0));
            var walker = TreeWalker.RawViewWalker;

            while (stack.Count > 0 && records.Count < maxNodes)
            {
                var item = stack.Pop();
                var element = item.Item1;
                var depth = item.Item2;
                records.Add(Capture(element, depth, records.Count, prompt, promptPrefix));

                if (depth >= maxDepth)
                {
                    continue;
                }

                var children = new List<AutomationElement>();
                try
                {
                    AutomationElement child = walker.GetFirstChild(element);
                    while (child != null && children.Count < maxNodes)
                    {
                        children.Add(child);
                        child = walker.GetNextSibling(child);
                    }
                }
                catch
                {
                    continue;
                }

                for (int i = children.Count - 1; i >= 0; i--)
                {
                    stack.Push(new Tuple<AutomationElement, int>(children[i], depth + 1));
                }
            }

            return records;
        }

        public static NodeRecord Probe(AutomationElement element, string prompt, string promptPrefix)
        {
            return Capture(element, 0, 0, prompt, promptPrefix);
        }

        public static bool TryInvoke(AutomationElement element, out string error)
        {
            error = "";
            try
            {
                object pattern;
                if (!element.TryGetCurrentPattern(InvokePattern.Pattern, out pattern))
                {
                    error = "InvokePattern not available.";
                    return false;
                }

                ((InvokePattern)pattern).Invoke();
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static bool TrySetFocus(AutomationElement element, out string error)
        {
            error = "";
            try
            {
                element.SetFocus();
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static bool TrySetValue(AutomationElement element, string value, out string error)
        {
            error = "";
            try
            {
                object pattern;
                if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out pattern))
                {
                    error = "ValuePattern not available.";
                    return false;
                }

                var valuePattern = (ValuePattern)pattern;
                if (valuePattern.Current.IsReadOnly)
                {
                    error = "ValuePattern is read-only.";
                    return false;
                }

                valuePattern.SetValue(value);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static NodeRecord Capture(AutomationElement element, int depth, int index, string prompt, string promptPrefix)
        {
            var record = new NodeRecord();
            record.Index = index;
            record.Depth = depth;
            record.Element = element;
            record.ValueLength = -1;

            try
            {
                record.Name = element.Current.Name ?? "";
            }
            catch
            {
                record.Name = "";
            }

            try
            {
                record.AutomationId = element.Current.AutomationId ?? "";
            }
            catch
            {
                record.AutomationId = "";
            }

            try
            {
                record.ClassName = element.Current.ClassName ?? "";
            }
            catch
            {
                record.ClassName = "";
            }

            try
            {
                record.ControlType = element.Current.ControlType.ProgrammaticName ?? "";
            }
            catch
            {
                record.ControlType = "";
            }

            try
            {
                record.Enabled = element.Current.IsEnabled;
                record.Offscreen = element.Current.IsOffscreen;
                record.Focusable = element.Current.IsKeyboardFocusable;
            }
            catch
            {
                record.Enabled = false;
                record.Offscreen = true;
                record.Focusable = false;
            }

            try
            {
                var rect = element.Current.BoundingRectangle;
                record.X = CleanDouble(rect.X);
                record.Y = CleanDouble(rect.Y);
                record.Width = CleanDouble(rect.Width);
                record.Height = CleanDouble(rect.Height);
            }
            catch
            {
                record.X = 0;
                record.Y = 0;
                record.Width = 0;
                record.Height = 0;
            }

            try
            {
                var runtimeId = element.GetRuntimeId();
                record.RuntimeId = runtimeId == null ? "" : string.Join(".", runtimeId);
            }
            catch
            {
                record.RuntimeId = "";
            }

            record.NameContainsPrompt = Contains(record.Name, prompt);
            record.NameContainsPrefix = Contains(record.Name, promptPrefix);

            try
            {
                object pattern;
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out pattern))
                {
                    var valuePattern = (ValuePattern)pattern;
                    record.HasValuePattern = true;
                    record.ValueReadOnly = valuePattern.Current.IsReadOnly;
                    var value = valuePattern.Current.Value ?? "";
                    record.ValueLength = value.Length;
                    record.ValueContainsPrompt = Contains(value, prompt);
                    record.ValueContainsPrefix = Contains(value, promptPrefix);
                }
            }
            catch
            {
                record.HasValuePattern = false;
            }

            try
            {
                object pattern;
                if (element.TryGetCurrentPattern(TextPattern.Pattern, out pattern))
                {
                    var textPattern = (TextPattern)pattern;
                    record.HasTextPattern = true;
                    if (!string.IsNullOrEmpty(prompt))
                    {
                        record.TextContainsPrompt = textPattern.DocumentRange.FindText(prompt, false, false) != null;
                    }

                    if (!string.IsNullOrEmpty(promptPrefix))
                    {
                        record.TextContainsPrefix = textPattern.DocumentRange.FindText(promptPrefix, false, false) != null;
                    }
                }
            }
            catch
            {
                record.HasTextPattern = false;
            }

            try
            {
                object pattern;
                record.HasInvokePattern = element.TryGetCurrentPattern(InvokePattern.Pattern, out pattern);
            }
            catch
            {
                record.HasInvokePattern = false;
            }

            return record;
        }

        private static bool Contains(string text, string needle)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(needle))
            {
                return false;
            }

            return text.IndexOf(needle, StringComparison.Ordinal) >= 0;
        }

        private static double CleanDouble(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return 0;
            }

            if (value > 1000000 || value < -1000000)
            {
                return 0;
            }

            return value;
        }
    }
}
"@
}

$EvidenceDir = Split-Path -Parent $PSScriptRoot
$LogsDir = Join-Path $EvidenceDir "logs"
New-Item -ItemType Directory -Force -Path $LogsDir | Out-Null
$Utf8NoBom = [System.Text.UTF8Encoding]::new($false)
$ActionLines = New-Object "System.Collections.Generic.List[string]"

function Add-Action {
    param([Parameter(Mandatory = $true)][string]$Message)

    $stamp = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssK")
    $ActionLines.Add("- $stamp - $Message") | Out-Null
}

function Write-TextFile {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [AllowNull()][string]$Text
    )

    if ($null -eq $Text) {
        $Text = ""
    }

    $normalized = $Text -replace "`r`n", "`n" -replace "`r", "`n"
    $lines = $normalized -split "`n", -1
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $lines[$i] = $lines[$i].TrimEnd()
    }

    [System.IO.File]::WriteAllText($Path, ($lines -join "`n"), $Utf8NoBom)
}

function Write-JsonFile {
    param(
        [Parameter(Mandatory = $true)]$Value,
        [Parameter(Mandatory = $true)][string]$Path
    )

    Write-TextFile -Path $Path -Text (($Value | ConvertTo-Json -Depth 40) + "`n")
}

function Get-Sha256Hex {
    param([AllowNull()][string]$Text)

    if ($null -eq $Text) {
        $Text = ""
    }

    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
        return ([System.BitConverter]::ToString($sha.ComputeHash($bytes))).Replace("-", "").ToLowerInvariant()
    } finally {
        $sha.Dispose()
    }
}

function Get-ProcessNameSafe {
    param([int]$ProcessId)

    try {
        return (Get-Process -Id $ProcessId -ErrorAction Stop).ProcessName
    } catch {
        return ""
    }
}

function Redact-Text {
    param([AllowNull()][string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return ""
    }

    return "<redacted:{0} chars>" -f $Text.Trim().Length
}

function Format-Hwnd {
    param([Int64]$Hwnd)
    return "0x{0:X}" -f $Hwnd
}

function ConvertTo-SafeWindowRecord {
    param($Raw)

    $processName = Get-ProcessNameSafe -ProcessId ([int]$Raw.Pid)
    $caption = [string]$Raw.Caption
    [pscustomobject]@{
        hwnd = Format-Hwnd -Hwnd ([Int64]$Raw.Hwnd)
        hwnd_int64 = [Int64]$Raw.Hwnd
        pid = [int]$Raw.Pid
        process = $processName
        class = [string]$Raw.ClassName
        caption_redacted = Redact-Text -Text $caption
        caption_length = $caption.Trim().Length
        visible = [bool]$Raw.Visible
        target_hint = ($processName -match "(?i)^claude$" -or $caption -match "(?i)\bClaude\b")
    }
}

function ConvertTo-SafeChildRecord {
    param($Raw)

    $caption = [string]$Raw.Caption
    [pscustomobject]@{
        hwnd = Format-Hwnd -Hwnd ([Int64]$Raw.Hwnd)
        class = [string]$Raw.ClassName
        caption_redacted = Redact-Text -Text $caption
        caption_length = $caption.Trim().Length
        visible = [bool]$Raw.Visible
    }
}

function Get-RectObject {
    param($Node)

    [pscustomobject]@{
        x = [int][Math]::Round([double]$Node.X)
        y = [int][Math]::Round([double]$Node.Y)
        width = [int][Math]::Round([double]$Node.Width)
        height = [int][Math]::Round([double]$Node.Height)
        left = [int][Math]::Round([double]$Node.X)
        top = [int][Math]::Round([double]$Node.Y)
        right = [int][Math]::Round(([double]$Node.X + [double]$Node.Width))
        bottom = [int][Math]::Round(([double]$Node.Y + [double]$Node.Height))
    }
}

function Get-SafeNodeLabel {
    param($Node)

    $name = [string]$Node.Name
    if ([string]::IsNullOrWhiteSpace($name)) {
        return ""
    }

    if ($name -match "(?i)XIIGen|Smart Skills|llm_mvp_core|xiigen[-_ ]|xiigen\b") {
        $safe = ($name -replace "[\r\n\t]+", " ").Trim()
        if ($safe.Length -le 180) {
            return $safe
        }
    }

    if ($name -match "(?i)message claude|ask claude|prompt|composer") {
        return "[generic composer label]"
    }

    return "[redacted UI label; {0} chars]" -f $name.Trim().Length
}

function ConvertTo-SafeNodeRecord {
    param($Node)

    [pscustomobject]@{
        index = [int]$Node.Index
        depth = [int]$Node.Depth
        runtime_id_hash = Get-Sha256Hex -Text ([string]$Node.RuntimeId)
        control_type = [string]$Node.ControlType
        class_redacted = if ([string]::IsNullOrWhiteSpace([string]$Node.ClassName)) { "" } else { Redact-Text -Text ([string]$Node.ClassName) }
        automation_id_redacted = if ([string]::IsNullOrWhiteSpace([string]$Node.AutomationId)) { "" } else { Redact-Text -Text ([string]$Node.AutomationId) }
        safe_label = Get-SafeNodeLabel -Node $Node
        focusable = [bool]$Node.Focusable
        enabled = [bool]$Node.Enabled
        offscreen = [bool]$Node.Offscreen
        bounds = Get-RectObject -Node $Node
        patterns = [pscustomobject]@{
            value = [bool]$Node.HasValuePattern
            value_readonly = [bool]$Node.ValueReadOnly
            value_length = [int]$Node.ValueLength
            text = [bool]$Node.HasTextPattern
            invoke = [bool]$Node.HasInvokePattern
        }
        prompt_probe = [pscustomobject]@{
            exact = ([bool]$Node.NameContainsPrompt -or [bool]$Node.ValueContainsPrompt -or [bool]$Node.TextContainsPrompt)
            prefix = ([bool]$Node.NameContainsPrefix -or [bool]$Node.ValueContainsPrefix -or [bool]$Node.TextContainsPrefix)
            sources = @(
                if ([bool]$Node.NameContainsPrompt) { "Current.Name exact" }
                if ([bool]$Node.ValueContainsPrompt) { "ValuePattern exact" }
                if ([bool]$Node.TextContainsPrompt) { "TextPattern exact" }
                if ([bool]$Node.NameContainsPrefix) { "Current.Name prefix" }
                if ([bool]$Node.ValueContainsPrefix) { "ValuePattern prefix" }
                if ([bool]$Node.TextContainsPrefix) { "TextPattern prefix" }
            )
        }
    }
}

function Test-NodeVisibleInsideWindow {
    param($Node, $Window)

    if ([bool]$Node.Offscreen -or -not [bool]$Node.Enabled) {
        return $false
    }

    $width = [double]$Node.Width
    $height = [double]$Node.Height
    if ($width -lt 1 -or $height -lt 1) {
        return $false
    }

    $cx = [double]$Node.X + ($width / 2.0)
    $cy = [double]$Node.Y + ($height / 2.0)
    return ($cx -ge $Window.left -and $cx -le $Window.right -and $cy -ge $Window.top -and $cy -le $Window.bottom)
}

function Get-WindowRectFromNode {
    param($RootNode)

    [pscustomobject]@{
        left = [int][Math]::Round([double]$RootNode.X)
        top = [int][Math]::Round([double]$RootNode.Y)
        right = [int][Math]::Round(([double]$RootNode.X + [double]$RootNode.Width))
        bottom = [int][Math]::Round(([double]$RootNode.Y + [double]$RootNode.Height))
        width = [int][Math]::Round([double]$RootNode.Width)
        height = [int][Math]::Round([double]$RootNode.Height)
    }
}

function Get-RouteCandidates {
    param($Nodes, $Window)

    @($Nodes |
        Where-Object {
            Test-NodeVisibleInsideWindow -Node $_ -Window $Window -and
            ([string]$_.Name -match "(?i)XIIGen|Smart Skills|llm_mvp_core|xiigen[-_ ]|xiigen\b") -and
            ([string]$_.Name -notmatch "(?i)^More options")
        } |
        Sort-Object `
            @{ Expression = { if ([string]$_.Name -match "(?i)XIIGen Smart Skills UI proof run") { 0 } else { 1 } } }, `
            @{ Expression = { if ([string]$_.ControlType -match "Button") { 0 } else { 1 } } }, `
            @{ Expression = { [double]$_.Y } })
}

function Get-ComposerCandidates {
    param($Nodes, $Window)

    $lowerBound = [double]$Window.top + ([double]$Window.height * 0.50)
    @($Nodes |
        Where-Object {
            Test-NodeVisibleInsideWindow -Node $_ -Window $Window -and
            [double]$_.Y -ge $lowerBound -and
            [double]$_.Width -ge 240 -and
            [double]$_.Height -ge 18 -and
            [double]$_.Height -le ([double]$Window.height * 0.45) -and
            [string]$_.ControlType -notmatch "ControlType.Button|ControlType.MenuItem|ControlType.Text" -and
            (
                [bool]$_.HasValuePattern -or
                [bool]$_.HasTextPattern -or
                [bool]$_.Focusable -or
                [string]$_.Name -match "(?i)message claude|ask claude|prompt|composer"
            )
        } |
        Sort-Object `
            @{ Expression = { if ([string]$_.Name -match "(?i)message claude|ask claude|prompt|composer") { 0 } else { 1 } } }, `
            @{ Expression = { if ([bool]$_.HasValuePattern -and -not [bool]$_.ValueReadOnly) { 0 } else { 1 } } }, `
            @{ Expression = { if ([bool]$_.HasTextPattern) { 0 } else { 1 } } }, `
            @{ Expression = { -[double]$_.Y } })
}

function New-UiaSummary {
    param(
        [Parameter(Mandatory = $true)]$RootElement,
        [Parameter(Mandatory = $true)][string]$Prompt,
        [Parameter(Mandatory = $true)][string]$PromptPrefix,
        [Parameter(Mandatory = $true)][string]$Phase,
        [int]$MaxDepth,
        [int]$MaxNodes
    )

    $nodes = @([ClaudeComposerProof.UiaWalker]::Walk($RootElement, $MaxDepth, $MaxNodes, $Prompt, $PromptPrefix))
    $rootNode = $nodes | Select-Object -First 1
    $windowRect = if ($rootNode) { Get-WindowRectFromNode -RootNode $rootNode } else { $null }
    $routeCandidates = if ($windowRect) { Get-RouteCandidates -Nodes $nodes -Window $windowRect } else { @() }
    $composerCandidates = if ($windowRect) { Get-ComposerCandidates -Nodes $nodes -Window $windowRect } else { @() }
    $promptMatches = @($nodes | Where-Object { [bool]$_.NameContainsPrompt -or [bool]$_.ValueContainsPrompt -or [bool]$_.TextContainsPrompt })
    $prefixMatches = @($nodes | Where-Object { [bool]$_.NameContainsPrefix -or [bool]$_.ValueContainsPrefix -or [bool]$_.TextContainsPrefix })

    $categories = New-Object "System.Collections.Generic.List[string]"
    if ($routeCandidates.Count -gt 0) { $categories.Add("xiigen_route_visible") | Out-Null }
    if ($composerCandidates.Count -gt 0) { $categories.Add("composer_candidate_visible") | Out-Null }
    if ($promptMatches.Count -gt 0) { $categories.Add("prompt_exact_match_visible_or_value_present") | Out-Null }
    if ($prefixMatches.Count -gt 0) { $categories.Add("prompt_prefix_match_visible_or_value_present") | Out-Null }

    [pscustomobject]@{
        phase = $Phase
        helper_pattern = "selected Claude HWND -> bounded TreeWalker"
        limits = [pscustomobject]@{
            max_depth = $MaxDepth
            max_nodes = $MaxNodes
        }
        counts = [pscustomobject]@{
            nodes_seen = $nodes.Count
            route_candidates = $routeCandidates.Count
            composer_candidates = $composerCandidates.Count
            prompt_exact_matches = $promptMatches.Count
            prompt_prefix_matches = $prefixMatches.Count
        }
        categories = @($categories)
        window_bounds = $windowRect
        route_candidates_sample = @($routeCandidates | Select-Object -First 8 | ForEach-Object { ConvertTo-SafeNodeRecord -Node $_ })
        composer_candidates_sample = @($composerCandidates | Select-Object -First 8 | ForEach-Object { ConvertTo-SafeNodeRecord -Node $_ })
        prompt_matches_sample = @($promptMatches | Select-Object -First 5 | ForEach-Object { ConvertTo-SafeNodeRecord -Node $_ })
        prefix_matches_sample = @($prefixMatches | Select-Object -First 5 | ForEach-Object { ConvertTo-SafeNodeRecord -Node $_ })
        nodes = $nodes
        route_candidates_raw = $routeCandidates
        composer_candidates_raw = $composerCandidates
    }
}

function Write-SafeUiaSummary {
    param($Summary, [string]$Path)

    $safe = [pscustomobject]@{
        phase = $Summary.phase
        helper_pattern = $Summary.helper_pattern
        limits = $Summary.limits
        counts = $Summary.counts
        categories = $Summary.categories
        window_bounds = $Summary.window_bounds
        route_candidates_sample = $Summary.route_candidates_sample
        composer_candidates_sample = $Summary.composer_candidates_sample
        prompt_matches_sample = $Summary.prompt_matches_sample
        prefix_matches_sample = $Summary.prefix_matches_sample
    }
    Write-JsonFile -Value $safe -Path $Path
}

function Invoke-RouteCandidate {
    param($Node)

    $invokeError = ""
    $invokeOk = [ClaudeComposerProof.UiaWalker]::TryInvoke($Node.Element, [ref]$invokeError)
    if ($invokeOk) {
        return [pscustomobject]@{ ok = $true; method = "InvokePattern"; error = $null }
    }

    $x = [int][Math]::Round([double]$Node.X + ([double]$Node.Width / 2.0))
    $y = [int][Math]::Round([double]$Node.Y + ([double]$Node.Height / 2.0))
    [ClaudeComposerProof.NativeMethods]::ClickPoint($x, $y)
    return [pscustomobject]@{ ok = $true; method = "center_click"; error = $invokeError; x = $x; y = $y }
}

function Focus-ComposerTarget {
    param($Node, $Window)

    if ($Node) {
        $focusError = ""
        $focusOk = [ClaudeComposerProof.UiaWalker]::TrySetFocus($Node.Element, [ref]$focusError)
        $x = [int][Math]::Round([double]$Node.X + ([double]$Node.Width / 2.0))
        $y = [int][Math]::Round([double]$Node.Y + ([double]$Node.Height / 2.0))
        [ClaudeComposerProof.NativeMethods]::ClickPoint($x, $y)
        return [pscustomobject]@{ method = "composer_candidate"; focus_ok = $focusOk; focus_error = $focusError; x = $x; y = $y }
    }

    $fallbackX = [int][Math]::Round([double]$Window.left + ([double]$Window.width * 0.64))
    $fallbackY = [int][Math]::Round([double]$Window.top + ([double]$Window.height * 0.90))
    [ClaudeComposerProof.NativeMethods]::ClickPoint($fallbackX, $fallbackY)
    return [pscustomobject]@{ method = "window_bottom_coordinate_fallback"; focus_ok = $null; focus_error = $null; x = $fallbackX; y = $fallbackY }
}

function Try-SetPromptInput {
    param($ComposerNode, [string]$Prompt)

    if ($ComposerNode -and [bool]$ComposerNode.HasValuePattern -and -not [bool]$ComposerNode.ValueReadOnly) {
        $valueError = ""
        $valueOk = [ClaudeComposerProof.UiaWalker]::TrySetValue($ComposerNode.Element, $Prompt, [ref]$valueError)
        if ($valueOk) {
            return [pscustomobject]@{ ok = $true; method = "ValuePattern.SetValue"; error = $null; clipboard_used = $false }
        }
    }

    $clipboardSet = $false
    $clipboardError = $null
    try {
        [System.Windows.Forms.Clipboard]::SetText($Prompt)
        $clipboardSet = $true
    } catch {
        $clipboardError = $_.Exception.Message
        try {
            Set-Clipboard -Value $Prompt
            $clipboardSet = $true
        } catch {
            $clipboardError = $clipboardError + " / " + $_.Exception.Message
        }
    }

    if (-not $clipboardSet) {
        return [pscustomobject]@{ ok = $false; method = "clipboard_paste"; error = $clipboardError; clipboard_used = $false }
    }

    $pasteOk = [ClaudeComposerProof.NativeMethods]::SendCtrlV()
    return [pscustomobject]@{ ok = $pasteOk; method = "safe_clipboard_set_then_ctrl_v"; error = if ($pasteOk) { $null } else { "SendInput Ctrl+V returned false." }; clipboard_used = $true }
}

function Get-FocusedPromptProbe {
    param([string]$Prompt, [string]$PromptPrefix)

    try {
        $focused = [System.Windows.Automation.AutomationElement]::FocusedElement
        if ($null -eq $focused) {
            return $null
        }

        $probe = [ClaudeComposerProof.UiaWalker]::Probe($focused, $Prompt, $PromptPrefix)
        return ConvertTo-SafeNodeRecord -Node $probe
    } catch {
        return [pscustomobject]@{
            error = $_.Exception.Message
        }
    }
}

function Write-PacketDocs {
    param(
        [string]$Status,
        [string]$Blocker,
        [string]$NextStep,
        [bool]$PromptConfirmed,
        [bool]$PromptSent
    )

    $readme = @"
# Claude Path B: proof ввода prompt в composer после helper-fallback

Финальный статус: `$Status`.

Этот пакет проверяет только узкий шаг: безопасный prompt из сценария должен оказаться в composer реального локального Claude UI. Отправка prompt запрещена и не выполнялась: `prompt_sent=$PromptSent`.

Что сделано:

- использован проверенный helper-паттерн: top-level `EnumWindows`, выбранный Claude HWND, `EnumChildWindows` только от выбранного окна и bounded handle-rooted `TreeWalker`;
- маршрут XIIGen/Smart Skills был выбран или сфокусирован, если runner нашел подходящий видимый элемент;
- prompt был введен только до стадии composer proof;
- сырые приватные UI labels и clipboard-before не записывались в evidence.

Что подтверждено этим пакетом:

- `prompt_confirmed_in_composer=$PromptConfirmed`;
- prompt не отправлялся;
- product readiness, token savings, semantic quality, learnability/training и checkpoint promotion не заявляются.

Blocker, если есть: $Blocker

Следующий шаг: $NextStep
"@

    $actionHeader = @"
# Журнал действий

Этот журнал описывает фактически выполненный bounded helper-runner. Parent не нажимал Send/Enter вручную; UI-действия выполнялись только через runner.

"@

    Write-TextFile -Path (Join-Path $EvidenceDir "README.md") -Text ($readme + "`n")
    Write-TextFile -Path (Join-Path $EvidenceDir "action-log.md") -Text ($actionHeader + (($ActionLines -join "`n") + "`n"))
    Write-TextFile -Path (Join-Path $EvidenceDir "redaction-note.md") -Text @"
# Redaction note

Сырые private UI labels, chat text, account data, password/PIN and previous clipboard contents were not written to evidence. The scenario prompt was loaded in memory and compared by hash/boolean probes; runner logs store prompt length/hash and redacted UI summaries. The runner may set the clipboard to the safe scenario prompt for paste fallback, but it does not read or store prior clipboard contents.
"@
}

$status = "BLOCKED_NOT_RUN"
$blocker = $null
$nextStep = "Inspect result.json and logs."
$routeSelected = $false
$composerFound = $false
$promptTypedOrPasted = $false
$promptConfirmed = $false
$promptSent = $false
$helperPatternUsed = [pscustomobject]@{
    enum_windows_top_level = $true
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
$promptMetadata = $null
$routeAction = $null
$composerFocus = $null
$inputAction = $null
$focusedProbe = $null
$selectedWindow = $null
$windowSummaryPath = "logs/window-inventory.json"

try {
    Add-Action "Runner started. Scope: composer input proof only; Send and Enter are forbidden."

    if (-not (Test-Path -LiteralPath $ScenarioPath)) {
        throw "Scenario JSON not found: $ScenarioPath"
    }

    $scenario = Get-Content -LiteralPath $ScenarioPath -Encoding UTF8 -Raw | ConvertFrom-Json
    $prompt = [string]$scenario.exact_prompt
    if ([string]::IsNullOrWhiteSpace($prompt)) {
        throw "Scenario exact_prompt is empty."
    }

    $promptPrefix = if ($prompt.Length -gt 40) { $prompt.Substring(0, 40) } else { $prompt }
    $promptHash = Get-Sha256Hex -Text $prompt
    $promptMetadata = [pscustomobject]@{
        source_file = $ScenarioPath.Replace($RepoRoot + "\", "")
        source_field = "exact_prompt"
        prompt_sha256 = $promptHash
        prompt_length_chars = $prompt.Length
        raw_prompt_written_to_evidence = $false
    }
    Add-Action ("Prompt loaded in memory only. length={0}; sha256={1}." -f $prompt.Length, $promptHash)

    $preflight = $null
    if (Test-Path -LiteralPath $PreflightPath) {
        $preflight = Get-Content -LiteralPath $PreflightPath -Encoding UTF8 -Raw | ConvertFrom-Json
        Add-Action ("Preflight parsed: {0}." -f $preflight.status)
    } else {
        Add-Action "Preflight result was not found; continuing with live window discovery."
    }

    $topRaw = [ClaudeComposerProof.NativeMethods]::EnumerateTopWindows($MaxTopWindows)
    $topWindows = @($topRaw | ForEach-Object { ConvertTo-SafeWindowRecord -Raw $_ })
    $claudeWindows = @($topWindows | Where-Object { $_.target_hint -or $_.process -match "(?i)^claude$" } | Sort-Object @{ Expression = { if ($_.process -match "(?i)^claude$") { 0 } else { 1 } } }, @{ Expression = { -[int]$_.caption_length } })

    if ($claudeWindows.Count -eq 0) {
        $status = "BLOCKED_CLAUDE_WINDOW_NOT_FOUND"
        $blocker = "No visible top-level Claude window was found by bounded EnumWindows."
        $nextStep = "Open/unlock Claude locally, then retry the same bounded composer-input proof."
        Write-JsonFile -Value ([pscustomobject]@{ windows = $topWindows; selected = $null; limits = @{ max_top_windows = $MaxTopWindows } }) -Path (Join-Path $EvidenceDir $windowSummaryPath)
        Add-Action $blocker
        throw $blocker
    }

    $selectedWindow = $claudeWindows | Select-Object -First 1
    $helperPatternUsed.selected_claude_hwnd = $true
    $hwnd = [IntPtr]::new([Int64]$selectedWindow.hwnd_int64)
    [ClaudeComposerProof.NativeMethods]::BringToFront($hwnd) | Out-Null
    Start-Sleep -Milliseconds 600
    Add-Action ("Selected Claude HWND {0}; process={1}; pid={2}." -f $selectedWindow.hwnd, $selectedWindow.process, $selectedWindow.pid)

    $childRaw = [ClaudeComposerProof.NativeMethods]::EnumerateChildWindows($hwnd, $MaxChildWindows)
    $childWindows = @($childRaw | ForEach-Object { ConvertTo-SafeChildRecord -Raw $_ })
    $helperPatternUsed.enum_child_windows_selected_hwnd = $true
    Write-JsonFile -Value ([pscustomobject]@{
        selected = $selectedWindow
        top_window_count = $topWindows.Count
        top_windows_sample = @($topWindows | Select-Object -First 30)
        child_window_count = $childWindows.Count
        child_windows_sample = @($childWindows | Select-Object -First 40)
        limits = [pscustomobject]@{ max_top_windows = $MaxTopWindows; max_child_windows = $MaxChildWindows }
    }) -Path (Join-Path $EvidenceDir $windowSummaryPath)
    Add-Action ("Wrote redacted window inventory; child HWND count={0}." -f $childWindows.Count)

    $rootElement = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
    if ($null -eq $rootElement) {
        $status = "BLOCKED_CLAUDE_UIA_ROOT_NOT_AVAILABLE"
        $blocker = "Claude HWND was found, but AutomationElement.FromHandle returned null."
        $nextStep = "Retry after Claude window is responsive, or run a smaller HWND-only route/composer diagnostic."
        throw $blocker
    }

    $helperPatternUsed.handle_rooted_bounded_treewalker = $true
    $beforeSummary = New-UiaSummary -RootElement $rootElement -Prompt $prompt -PromptPrefix $promptPrefix -Phase "before_route_or_input" -MaxDepth $MaxUiaDepth -MaxNodes $MaxUiaNodes
    Write-SafeUiaSummary -Summary $beforeSummary -Path (Join-Path $LogsDir "bounded-uia-before-route-or-input.json")
    Add-Action ("Bounded handle-rooted UIA summary before action: nodes={0}; route candidates={1}; composer candidates={2}; exact prompt matches={3}." -f $beforeSummary.counts.nodes_seen, $beforeSummary.counts.route_candidates, $beforeSummary.counts.composer_candidates, $beforeSummary.counts.prompt_exact_matches)

    $windowBounds = $beforeSummary.window_bounds
    if ($null -eq $windowBounds) {
        $status = "BLOCKED_CLAUDE_WINDOW_BOUNDS_NOT_AVAILABLE"
        $blocker = "Selected Claude UIA root had no usable window bounds."
        $nextStep = "Retry with Claude visible and not minimized."
        throw $blocker
    }

    $routeCandidate = @($beforeSummary.route_candidates_raw | Select-Object -First 1)
    if ($routeCandidate.Count -gt 0) {
        $routeAction = Invoke-RouteCandidate -Node $routeCandidate[0]
        $routeSelected = [bool]$routeAction.ok
        Add-Action ("Route candidate selected/focused using {0}; ok={1}." -f $routeAction.method, $routeAction.ok)
        Start-Sleep -Milliseconds 900
    } else {
        $status = "BLOCKED_ROUTE_NOT_VISIBLE"
        $blocker = "No visible XIIGen/Smart Skills route candidate was found in the selected Claude HWND."
        $nextStep = "Open the XIIGen route in Claude, then retry composer-input proof."
        Add-Action $blocker
        throw $blocker
    }

    $afterRouteSummary = New-UiaSummary -RootElement $rootElement -Prompt $prompt -PromptPrefix $promptPrefix -Phase "after_route_select_before_input" -MaxDepth $MaxUiaDepth -MaxNodes $MaxUiaNodes
    Write-SafeUiaSummary -Summary $afterRouteSummary -Path (Join-Path $LogsDir "bounded-uia-after-route-before-input.json")
    Add-Action ("After route select: nodes={0}; composer candidates={1}; exact prompt matches before input={2}." -f $afterRouteSummary.counts.nodes_seen, $afterRouteSummary.counts.composer_candidates, $afterRouteSummary.counts.prompt_exact_matches)

    $composerCandidate = @($afterRouteSummary.composer_candidates_raw | Select-Object -First 1)
    if ($composerCandidate.Count -gt 0) {
        $composerFound = $true
        $composerFocus = Focus-ComposerTarget -Node $composerCandidate[0] -Window $afterRouteSummary.window_bounds
    } else {
        $composerFocus = Focus-ComposerTarget -Node $null -Window $afterRouteSummary.window_bounds
    }
    Add-Action ("Composer focus action: method={0}; x={1}; y={2}." -f $composerFocus.method, $composerFocus.x, $composerFocus.y)
    Start-Sleep -Milliseconds 500

    $inputAction = Try-SetPromptInput -ComposerNode $(if ($composerCandidate.Count -gt 0) { $composerCandidate[0] } else { $null }) -Prompt $prompt
    $promptTypedOrPasted = [bool]$inputAction.ok
    Add-Action ("Prompt input attempt: method={0}; ok={1}; clipboard_used={2}." -f $inputAction.method, $inputAction.ok, $inputAction.clipboard_used)
    Start-Sleep -Milliseconds 1200

    $focusedProbe = Get-FocusedPromptProbe -Prompt $prompt -PromptPrefix $promptPrefix
    $afterInputSummary = New-UiaSummary -RootElement $rootElement -Prompt $prompt -PromptPrefix $promptPrefix -Phase "after_input_no_submit" -MaxDepth $MaxUiaDepth -MaxNodes $MaxUiaNodes
    Write-SafeUiaSummary -Summary $afterInputSummary -Path (Join-Path $LogsDir "bounded-uia-after-input-no-submit.json")
    Write-JsonFile -Value $focusedProbe -Path (Join-Path $LogsDir "focused-element-prompt-probe.json")

    $focusedExact = $false
    if ($focusedProbe -and $focusedProbe.prompt_probe) {
        $focusedExact = [bool]$focusedProbe.prompt_probe.exact
    }

    $promptConfirmed = ($afterInputSummary.counts.prompt_exact_matches -gt 0 -or $focusedExact)
    if ($promptConfirmed -and -not $composerFound) {
        $composerFound = $true
    }

    Add-Action ("After input: exact prompt matches={0}; focused exact={1}; prompt_confirmed={2}; prompt_sent=false." -f $afterInputSummary.counts.prompt_exact_matches, $focusedExact, $promptConfirmed)

    if ($promptConfirmed) {
        $status = "CLAUDE_COMPOSER_INPUT_CONFIRMED"
        $blocker = $null
        $nextStep = "Review this packet. A separate explicit work order is required before any Send/Enter or response/correlation proof."
    } else {
        $status = "BLOCKED_COMPOSER_INPUT_NOT_CONFIRMED"
        $blocker = "Prompt input was attempted, but the bounded selected-HWND UIA probes did not confirm the exact prompt in composer."
        $nextStep = "Repair the composer targeting/confirmation step only; do not submit the prompt."
    }
} catch {
    if ($null -eq $blocker) {
        $status = "BLOCKED_RUNTIME_EXCEPTION"
        $blocker = $_.Exception.Message
        $nextStep = "Inspect logs and repair only this composer-input runner before retrying."
    }
    Add-Action ("Runner stopped with status {0}: {1}" -f $status, $blocker)
} finally {
    $result = [pscustomobject]@{
        status = $status
        scenario_id = "frdp-p01-claude-full-extension-entrypoint"
        scope = "Claude Path B composer-input proof only; no prompt submission."
        helper_pattern_used = $helperPatternUsed
        prompt_source = $promptMetadata
        preflight_result = $PreflightPath.Replace($RepoRoot + "\", "")
        route_selected = $routeSelected
        route_selection_action = $routeAction
        composer_found = $composerFound
        composer_focus_action = $composerFocus
        prompt_typed_or_pasted = $promptTypedOrPasted
        prompt_input_action = $inputAction
        prompt_confirmed_in_composer = $promptConfirmed
        prompt_sent = $promptSent
        enter_sent = $false
        send_button_clicked = $false
        focused_probe = $focusedProbe
        blocker_if_any = $blocker
        next_step = $nextStep
        deprecated_product_readiness_used = $false
        claims_not_made = @(
            "prompt submission",
            "Claude response",
            "core or adapter correlation after submission",
            "token savings",
            "semantic quality",
            "learnability or training",
            "checkpoint promotion",
            "all-host readiness"
        )
        redaction = [pscustomobject]@{
            screenshots_captured = $false
            raw_private_text_written = $false
            raw_prompt_written = $false
            previous_clipboard_text_read = $false
            note_file = "redaction-note.md"
        }
        logs = @(
            $windowSummaryPath,
            "logs/bounded-uia-before-route-or-input.json",
            "logs/bounded-uia-after-route-before-input.json",
            "logs/bounded-uia-after-input-no-submit.json",
            "logs/focused-element-prompt-probe.json"
        )
        timing = [pscustomobject]@{
            ended_local = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssK")
            ended_utc = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
        }
    }

    Write-JsonFile -Value $result -Path (Join-Path $EvidenceDir "result.json")
    Write-PacketDocs -Status $status -Blocker $(if ($blocker) { $blocker } else { "нет" }) -NextStep $nextStep -PromptConfirmed $promptConfirmed -PromptSent $promptSent

    Write-Output ("status={0}; prompt_confirmed_in_composer={1}; prompt_sent=false" -f $status, $promptConfirmed)

    if ($status -ne "CLAUDE_COMPOSER_INPUT_CONFIRMED") {
        exit 1
    }
}
