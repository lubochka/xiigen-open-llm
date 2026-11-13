param(
    [string]$OutDir = $PSScriptRoot,
    [string]$PreferredProcessName = "notepad",
    [int]$MaxTopWindows = 80,
    [int]$MaxChildWindows = 120
)

$ErrorActionPreference = "Stop"

function Write-Utf8NoBom {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Text
    )

    $encoding = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($Path, $Text, $encoding)
}

function Redact-Text {
    param([AllowNull()][string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return ""
    }

    return "<redacted:{0} chars>" -f $Text.Trim().Length
}

function Add-RunLog {
    param([string]$Message)

    $line = "{0} {1}" -f (Get-Date -Format "o"), $Message
    $script:RunLog.Add($line) | Out-Null
    Write-Host $line
}

function Get-WindowCaption {
    param([IntPtr]$Handle)

    $length = [SafeFallback.NativeWindowMethods]::GetWindowTextLength($Handle)
    if ($length -le 0) {
        return ""
    }

    $builder = New-Object System.Text.StringBuilder ($length + 1)
    [SafeFallback.NativeWindowMethods]::GetWindowText($Handle, $builder, $builder.Capacity) | Out-Null
    return $builder.ToString()
}

function Get-WindowClassName {
    param([IntPtr]$Handle)

    $builder = New-Object System.Text.StringBuilder 256
    [SafeFallback.NativeWindowMethods]::GetClassName($Handle, $builder, $builder.Capacity) | Out-Null
    return $builder.ToString()
}

function Get-WindowProcessId {
    param([IntPtr]$Handle)

    $processId = 0
    [SafeFallback.NativeWindowMethods]::GetWindowThreadProcessId($Handle, [ref]$processId) | Out-Null
    return $processId
}

function Format-Hwnd {
    param([IntPtr]$Handle)
    return "0x{0:X}" -f $Handle.ToInt64()
}

function Get-ProcessNameSafe {
    param([int]$ProcessId)

    try {
        return (Get-Process -Id $ProcessId -ErrorAction Stop).ProcessName
    } catch {
        return ""
    }
}

function Get-TopWindowRecords {
    param([int]$Limit)

    $records = New-Object System.Collections.Generic.List[object]
    $rawRecords = [SafeFallback.NativeWindowMethods]::EnumerateTopWindows($Limit)

    foreach ($raw in $rawRecords) {
        $caption = [string]$raw.Caption
        $processId = [int]$raw.Pid
        $processName = Get-ProcessNameSafe -ProcessId $processId

        $records.Add([pscustomobject]@{
            hwnd = "0x{0:X}" -f ([Int64]$raw.Hwnd)
            hwnd_int64 = [Int64]$raw.Hwnd
            pid = $processId
            process = $processName
            class = [string]$raw.ClassName
            caption_redacted = Redact-Text -Text $caption
            caption_length = $caption.Trim().Length
            category = if ($caption.Trim().Length -gt 0) { "visible_top_window_with_caption" } else { "visible_top_window_no_caption" }
        }) | Out-Null
    }

    return $records.ToArray()
}

function Get-ChildWindowRecords {
    param(
        [IntPtr]$RootHandle,
        [int]$Limit
    )

    $records = New-Object System.Collections.Generic.List[object]
    $rawRecords = [SafeFallback.NativeWindowMethods]::EnumerateChildWindows($RootHandle, $Limit)

    foreach ($raw in $rawRecords) {
        $caption = [string]$raw.Caption
        $visible = [bool]$raw.Visible

        $records.Add([pscustomobject]@{
            hwnd = "0x{0:X}" -f ([Int64]$raw.Hwnd)
            class = [string]$raw.ClassName
            visible = $visible
            caption_redacted = Redact-Text -Text $caption
            caption_length = $caption.Trim().Length
            category = if ($visible) { "child_visible" } else { "child_hidden_or_offscreen" }
        }) | Out-Null
    }

    return $records.ToArray()
}

function Wait-ForMainWindow {
    param(
        [System.Diagnostics.Process]$Process,
        [int]$TimeoutMs = 4000
    )

    $deadline = (Get-Date).AddMilliseconds($TimeoutMs)
    do {
        try {
            $Process.Refresh()
            if ($Process.MainWindowHandle -ne [IntPtr]::Zero) {
                return $Process.MainWindowHandle
            }
        } catch {
            return [IntPtr]::Zero
        }

        Start-Sleep -Milliseconds 100
    } while ((Get-Date) -lt $deadline)

    return [IntPtr]::Zero
}

$script:RunLog = New-Object System.Collections.Generic.List[string]
$createdTargetProcess = $false
$targetProcess = $null
$targetHandle = [IntPtr]::Zero
$targetSource = "none"
$status = "FAILED_NOT_RUN"
$fixedPatternVerified = $false
$safeToRetryClaudeRunner = $false
$nextStep = "Inspect run.log."
$summary = $null

try {
    $resolvedOutDir = [System.IO.Path]::GetFullPath($OutDir)
    New-Item -ItemType Directory -Force -Path $resolvedOutDir | Out-Null

    Add-RunLog "Starting bounded HWND fallback helper run."

    if (-not ("SafeFallback.NativeWindowMethods" -as [type])) {
        Add-Type -TypeDefinition @"
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SafeFallback
{
    public sealed class RawWindowRecord
    {
        public long Hwnd { get; set; }
        public int Pid { get; set; }
        public string ClassName { get; set; }
        public string Caption { get; set; }
        public bool Visible { get; set; }
    }

    public static class NativeWindowMethods
    {
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

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

        private static RawWindowRecord CaptureWindow(IntPtr hWnd)
        {
            int processId;
            GetWindowThreadProcessId(hWnd, out processId);

            return new RawWindowRecord
            {
                Hwnd = hWnd.ToInt64(),
                Pid = processId,
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
}
"@
    }

    $existingTarget = Get-Process -Name $PreferredProcessName -ErrorAction SilentlyContinue |
        Where-Object { $_.ProcessName -notmatch "(?i)claude" } |
        Select-Object -First 1

    if ($existingTarget) {
        $targetProcess = $existingTarget
        $targetHandle = Wait-ForMainWindow -Process $targetProcess
        $targetSource = "existing_process"
        Add-RunLog ("Using existing harmless target process: {0}/{1}." -f $targetProcess.ProcessName, $targetProcess.Id)
    } else {
        Add-RunLog ("No existing {0} process found; starting temporary harmless target." -f $PreferredProcessName)
        $targetProcess = Start-Process -FilePath ($PreferredProcessName + ".exe") -PassThru
        $createdTargetProcess = $true
        $targetHandle = Wait-ForMainWindow -Process $targetProcess
        $targetSource = "temporary_process_started_by_script"
        Add-RunLog ("Started target process: {0}/{1}." -f $targetProcess.ProcessName, $targetProcess.Id)
    }

    $topWindows = Get-TopWindowRecords -Limit $MaxTopWindows
    Add-RunLog ("Enumerated {0} visible top-level windows with limit {1}." -f $topWindows.Count, $MaxTopWindows)

    $selectedTopWindow = $null
    if ($targetHandle -ne [IntPtr]::Zero) {
        $targetHandleValue = $targetHandle.ToInt64()
        $selectedTopWindow = $topWindows | Where-Object { $_.hwnd_int64 -eq $targetHandleValue } | Select-Object -First 1
    }

    if (-not $selectedTopWindow -and $targetProcess) {
        $selectedTopWindow = $topWindows | Where-Object { $_.pid -eq $targetProcess.Id } | Select-Object -First 1
        if ($selectedTopWindow) {
            $targetHandle = [IntPtr]::new([Int64]$selectedTopWindow.hwnd_int64)
        }
    }

    if (-not $selectedTopWindow) {
        $selectedTopWindow = $topWindows |
            Where-Object { $_.process -notmatch "(?i)claude" -and $_.caption_length -ge 0 } |
            Select-Object -First 1
        if ($selectedTopWindow) {
            $targetHandle = [IntPtr]::new([Int64]$selectedTopWindow.hwnd_int64)
            $targetSource = "first_visible_non_claude_window"
        }
    }

    if (-not $selectedTopWindow -or $targetHandle -eq [IntPtr]::Zero) {
        throw "No harmless top-level target window was available."
    }

    $childWindows = Get-ChildWindowRecords -RootHandle $targetHandle -Limit $MaxChildWindows
    Add-RunLog ("Enumerated {0} child HWND records with limit {1}." -f $childWindows.Count, $MaxChildWindows)

    $processCategories = @($topWindows |
        Group-Object -Property process |
        Sort-Object -Property Count -Descending |
        Select-Object -First 20 |
        ForEach-Object {
            [pscustomobject]@{
                process = if ([string]::IsNullOrWhiteSpace($_.Name)) { "<unknown>" } else { $_.Name }
                visible_top_window_count = $_.Count
            }
        })

    $childCategories = @($childWindows |
        Group-Object -Property category |
        ForEach-Object {
            [pscustomobject]@{
                category = $_.Name
                count = $_.Count
            }
        })

    $summary = [pscustomobject]@{
        run_started = (Get-Date).ToString("o")
        target = [pscustomobject]@{
            source = $targetSource
            process = $selectedTopWindow.process
            pid = $selectedTopWindow.pid
            hwnd = $selectedTopWindow.hwnd
            class = $selectedTopWindow.class
            caption_redacted = $selectedTopWindow.caption_redacted
            caption_length = $selectedTopWindow.caption_length
        }
        limits = [pscustomobject]@{
            max_top_windows = $MaxTopWindows
            max_child_windows = $MaxChildWindows
        }
        counts = [pscustomobject]@{
            top_windows_seen = $topWindows.Count
            child_windows_seen = $childWindows.Count
        }
        process_categories = $processCategories
        child_categories = $childCategories
        sample_children = @($childWindows | Select-Object -First 20)
    }

    Write-Utf8NoBom -Path (Join-Path $resolvedOutDir "window-summary.json") -Text (($summary | ConvertTo-Json -Depth 8) + [Environment]::NewLine)

    $status = "UIA_HELPER_FALLBACK_VERIFIED"
    $fixedPatternVerified = $true
    $safeToRetryClaudeRunner = $true
    $nextStep = "Apply the selected-window HWND fallback to the next Claude runner: preselect the Claude top-level HWND, keep strict node limits, redact labels, and only then add a bounded handle-rooted UIA walk if HWND summaries are insufficient."
    Add-RunLog "Completed bounded HWND fallback helper run."
} catch {
    $status = "FAILED_RUNTIME_EXCEPTION"
    $fixedPatternVerified = $false
    $safeToRetryClaudeRunner = $false
    $nextStep = "Repair the fallback script before any Claude runner retry."
    Add-RunLog ("ERROR: {0}" -f $_.Exception.Message)
    Add-RunLog ("ERROR_STACK: {0}" -f $_.ScriptStackTrace)
} finally {
    if ($createdTargetProcess -and $targetProcess) {
        try {
            $targetProcess.Refresh()
            if (-not $targetProcess.HasExited) {
                $targetProcess.CloseMainWindow() | Out-Null
                Start-Sleep -Milliseconds 300
                $targetProcess.Refresh()
                if (-not $targetProcess.HasExited) {
                    Stop-Process -Id $targetProcess.Id -Force -ErrorAction SilentlyContinue
                }
            }
            Add-RunLog "Temporary target process closed."
        } catch {
            Add-RunLog ("Temporary target cleanup warning: {0}" -f $_.Exception.Message)
        }
    }

    $targetUsed = $null
    if ($summary -and $summary.target) {
        $targetUsed = $summary.target
    } elseif ($targetProcess) {
        $targetUsed = [pscustomobject]@{
            source = $targetSource
            process = $targetProcess.ProcessName
            pid = $targetProcess.Id
            hwnd = if ($targetHandle -ne [IntPtr]::Zero) { Format-Hwnd -Handle $targetHandle } else { "" }
        }
    } else {
        $targetUsed = [pscustomobject]@{
            source = "none"
            process = ""
            pid = 0
            hwnd = ""
        }
    }

    $result = [pscustomobject]@{
        status = $status
        fixed_pattern_verified = $fixedPatternVerified
        safe_to_retry_claude_runner = $safeToRetryClaudeRunner
        target_used = $targetUsed
        next_step = $nextStep
    }

    $resolvedOutDir = [System.IO.Path]::GetFullPath($OutDir)
    Write-Utf8NoBom -Path (Join-Path $resolvedOutDir "run.log") -Text (($script:RunLog -join [Environment]::NewLine) + [Environment]::NewLine)
    Write-Utf8NoBom -Path (Join-Path $resolvedOutDir "result.json") -Text (($result | ConvertTo-Json -Depth 8) + [Environment]::NewLine)

    if ($status -ne "UIA_HELPER_FALLBACK_VERIFIED") {
        exit 1
    }
}
