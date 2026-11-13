param(
    [string] $CursorExe = "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe",
    [string] $ExtensionRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
    [string] $EvidenceBase = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z",
    [int] $HostReadyTimeoutSec = 90,
    [int] $WindowReadyTimeoutSec = 45,
    [int] $PanelSettleSec = 4,
    [int] $PanelHoldMs = 120000,
    [int] $SubmitResultWaitMs = 20000,
    [int] $SubmitResultCdpTimeoutSec = 30,
    [switch] $TypeabilityProofOnly,
    [switch] $SubmitResultProof,
    [switch] $KeepCursorOpen
)

$ErrorActionPreference = "Stop"

if ($TypeabilityProofOnly.IsPresent -and $SubmitResultProof.IsPresent) {
    throw "-TypeabilityProofOnly and -SubmitResultProof are separate Row12 proof modes; pass only one."
}

function New-Stamp {
    return (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssZ")
}

function Ensure-Directory {
    param([string] $Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Force -Path $Path | Out-Null
    }
}

function Write-JsonFile {
    param(
        [string] $Path,
        [object] $Value
    )
    Ensure-Directory -Path (Split-Path -Parent $Path)
    $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Write-TextFile {
    param(
        [string] $Path,
        [string] $Value
    )
    Ensure-Directory -Path (Split-Path -Parent $Path)
    Set-Content -LiteralPath $Path -Value $Value -Encoding UTF8
}

function Get-Sha256Hex {
    param([AllowNull()] [string] $Value)

    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes([string] $Value)
        return ([System.BitConverter]::ToString($sha.ComputeHash($bytes))).Replace("-", "").ToLowerInvariant()
    } finally {
        $sha.Dispose()
    }
}

function Copy-DirectoryContentsIfExists {
    param(
        [string] $Source,
        [string] $Destination
    )

    if (-not (Test-Path -LiteralPath $Source)) {
        return @()
    }

    Ensure-Directory -Path $Destination
    $copied = @()
    foreach ($item in @(Get-ChildItem -LiteralPath $Source -Force -ErrorAction SilentlyContinue)) {
        try {
            Copy-Item -LiteralPath $item.FullName -Destination $Destination -Recurse -Force -ErrorAction Stop
            $copied += [pscustomobject] @{
                source = $item.FullName
                destination = (Join-Path $Destination $item.Name)
            }
        } catch {
            $copied += [pscustomobject] @{
                source = $item.FullName
                destination = (Join-Path $Destination $item.Name)
                error = $_.Exception.Message
            }
        }
    }

    return @($copied)
}

function ConvertTo-NativeArgument {
    param([AllowNull()] [string] $Value)

    if ($null -eq $Value) {
        return '""'
    }

    if ($Value -notmatch '[\s"]') {
        return $Value
    }

    $result = '"'
    $backslashCount = 0
    foreach ($char in $Value.ToCharArray()) {
        if ($char -eq '\') {
            $backslashCount += 1
            continue
        }

        if ($char -eq '"') {
            $result += ('\' * (($backslashCount * 2) + 1))
            $result += '"'
            $backslashCount = 0
            continue
        }

        if ($backslashCount -gt 0) {
            $result += ('\' * $backslashCount)
            $backslashCount = 0
        }
        $result += $char
    }

    if ($backslashCount -gt 0) {
        $result += ('\' * ($backslashCount * 2))
    }
    $result += '"'
    return $result
}

function Join-NativeArgumentList {
    param([string[]] $Arguments)
    return (($Arguments | ForEach-Object { ConvertTo-NativeArgument -Value $_ }) -join " ")
}

function Resolve-CursorExecutable {
    param([string] $Preferred)
    if ($Preferred -and (Test-Path -LiteralPath $Preferred)) {
        return (Resolve-Path -LiteralPath $Preferred).Path
    }

    $cmd = Get-Command cursor.cmd -ErrorAction SilentlyContinue
    if ($cmd -and $cmd.Source) {
        $candidate = Join-Path (Split-Path -Parent (Split-Path -Parent $cmd.Source)) "Cursor.exe"
        if (Test-Path -LiteralPath $candidate) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    throw "Cursor executable not found. Pass -CursorExe explicitly."
}

Add-Type @"
using System;
using System.Text;
using System.Runtime.InteropServices;

public static class Row12Native {
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
"@

function Get-CursorWindows {
    $windows = @()
    foreach ($proc in @(Get-Process Cursor -ErrorAction SilentlyContinue)) {
        try {
            if ($proc.MainWindowHandle -eq [IntPtr]::Zero) {
                continue
            }
            if ([string]::IsNullOrWhiteSpace($proc.MainWindowTitle)) {
                continue
            }

            $windows += [pscustomobject] @{
                Handle = $proc.MainWindowHandle
                HandleHex = ("0x{0:X}" -f $proc.MainWindowHandle.ToInt64())
                ProcessId = [int] $proc.Id
                ProcessName = $proc.ProcessName
                Title = $proc.MainWindowTitle
                StartTime = $proc.StartTime
            }
        } catch {
        }
    }

    return @($windows)
}

function Select-CursorDevHostWindow {
    param(
        [string[]] $BeforeHandles,
        [datetime] $LaunchStart,
        [int] $TimeoutSec
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    do {
        $windows = @(Get-CursorWindows)
        $preferred = @($windows | Where-Object {
            $_.Title -match "Extension Development Host" -or
            $_.Title -match "XIIGen Smart Skills" -or
            $_.Title -match "cursor-xiigen-smart-skills"
        })

        $newPreferred = @($preferred | Where-Object {
            ($BeforeHandles -notcontains $_.HandleHex) -or $_.StartTime -ge $LaunchStart.AddSeconds(-5)
        })
        if ($newPreferred.Count -gt 0) {
            return ($newPreferred | Sort-Object StartTime -Descending | Select-Object -First 1)
        }

        $newAny = @($windows | Where-Object {
            ($BeforeHandles -notcontains $_.HandleHex) -or $_.StartTime -ge $LaunchStart.AddSeconds(-5)
        })
        if ($newAny.Count -gt 0) {
            return ($newAny | Sort-Object StartTime -Descending | Select-Object -First 1)
        }

        if ($preferred.Count -gt 0) {
            return ($preferred | Sort-Object StartTime -Descending | Select-Object -First 1)
        }

        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)

    return $null
}

function Focus-Window {
    param([IntPtr] $Handle)
    [void] [Row12Native]::ShowWindowAsync($Handle, 9)
    Start-Sleep -Milliseconds 300
    [void] [Row12Native]::SetForegroundWindow($Handle)
    Start-Sleep -Milliseconds 600
}

function Capture-WindowPng {
    param(
        [IntPtr] $Handle,
        [string] $Path
    )

    Add-Type -AssemblyName System.Drawing
    $rect = New-Object Row12Native+RECT
    if (-not [Row12Native]::GetWindowRect($Handle, [ref] $rect)) {
        throw "GetWindowRect failed for $Handle"
    }

    $width = [Math]::Max(1, $rect.Right - $rect.Left)
    $height = [Math]::Max(1, $rect.Bottom - $rect.Top)
    $bitmap = New-Object System.Drawing.Bitmap $width, $height
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, (New-Object System.Drawing.Size $width, $height))
        Ensure-Directory -Path (Split-Path -Parent $Path)
        $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    } finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }

    return [pscustomobject] @{
        Path = $Path
        Left = $rect.Left
        Top = $rect.Top
        Width = $width
        Height = $height
    }
}

function Read-UiaSnapshot {
    param([IntPtr] $Handle)

    $snapshot = [ordered] @{
        attempted = $true
        available = $false
        error = $null
        names = @()
        edit_controls = @()
    }

    try {
        Add-Type -AssemblyName UIAutomationClient
        Add-Type -AssemblyName UIAutomationTypes
        $root = [System.Windows.Automation.AutomationElement]::FromHandle($Handle)
        if (-not $root) {
            throw "AutomationElement.FromHandle returned null."
        }

        $all = $root.FindAll(
            [System.Windows.Automation.TreeScope]::Descendants,
            [System.Windows.Automation.Condition]::TrueCondition
        )
        $names = New-Object System.Collections.Generic.List[string]
        $edits = New-Object System.Collections.Generic.List[object]
        $editType = [System.Windows.Automation.ControlType]::Edit

        foreach ($element in $all) {
            try {
                $name = $element.Current.Name
                if (-not [string]::IsNullOrWhiteSpace($name)) {
                    $names.Add($name)
                }

                if ($element.Current.ControlType -eq $editType) {
                    $edits.Add([pscustomobject] @{
                        Name = $name
                        AutomationId = $element.Current.AutomationId
                        ClassName = $element.Current.ClassName
                    })
                }
            } catch {
            }
        }

        $snapshot.available = $true
        $snapshot.names = @($names | Select-Object -Unique)
        $snapshot.edit_controls = @($edits)
    } catch {
        $snapshot.error = $_.Exception.Message
    }

    return [pscustomobject] $snapshot
}

function New-ExtensionHostTestFile {
    param(
        [string] $Path,
        [int] $HoldMs
    )

    $content = @'
const fs = require("fs");
const path = require("path");
const vscode = require("vscode");

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function writeJson(filePath, value) {
  ensureDir(path.dirname(filePath));
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function summarizeTabInput(input) {
  if (!input) {
    return null;
  }

  const summary = {
    constructorName: input.constructor && input.constructor.name ? input.constructor.name : null
  };

  if (input.uri && typeof input.uri.toString === "function") {
    summary.uri = input.uri.toString();
  }
  if (input.modified && input.modified.uri && typeof input.modified.uri.toString === "function") {
    summary.modifiedUri = input.modified.uri.toString();
  }
  if (input.original && input.original.uri && typeof input.original.uri.toString === "function") {
    summary.originalUri = input.original.uri.toString();
  }
  if (Object.prototype.hasOwnProperty.call(input, "viewType")) {
    summary.viewType = input.viewType;
  }
  if (Object.prototype.hasOwnProperty.call(input, "notebookType")) {
    summary.notebookType = input.notebookType;
  }

  return summary;
}

function summarizeTab(tab, index) {
  const input = summarizeTabInput(tab.input);
  return {
    index,
    label: tab.label,
    isActive: tab.isActive,
    isDirty: tab.isDirty,
    isPinned: tab.isPinned,
    input,
    viewType: input && Object.prototype.hasOwnProperty.call(input, "viewType") ? input.viewType : null
  };
}

function collectWindowDiagnostics() {
  const activeEditor = vscode.window.activeTextEditor;
  const diagnostics = {
    active_editor: activeEditor ? {
      document_file_name: activeEditor.document && activeEditor.document.fileName ? activeEditor.document.fileName : null,
      document_uri: activeEditor.document && activeEditor.document.uri ? activeEditor.document.uri.toString() : null,
      document_language_id: activeEditor.document ? activeEditor.document.languageId : null,
      document_is_dirty: activeEditor.document ? activeEditor.document.isDirty : null,
      view_column: activeEditor.viewColumn
    } : null,
    active_window_state: vscode.window.state ? {
      focused: vscode.window.state.focused,
      active: vscode.window.state.active
    } : null,
    tab_groups_available: !!(vscode.window.tabGroups && Array.isArray(vscode.window.tabGroups.all)),
    tab_groups: []
  };

  if (diagnostics.tab_groups_available) {
    diagnostics.tab_groups = vscode.window.tabGroups.all.map((group, groupIndex) => {
      const tabs = group.tabs.map((tab, tabIndex) => summarizeTab(tab, tabIndex));
      const activeTab = tabs.find((tab) => tab.isActive) || null;
      return {
        group_index: groupIndex,
        is_active: group.isActive,
        view_column: group.viewColumn,
        active_tab_label: activeTab ? activeTab.label : null,
        active_tab_input: activeTab ? activeTab.input : null,
        active_tab_view_type: activeTab ? activeTab.viewType : null,
        active_tab_is_active: activeTab ? activeTab.isActive : null,
        tabs
      };
    });
  }

  return diagnostics;
}

async function run() {
  const started = new Date();
  const reportPath = process.env.ROW12_PANEL_HOST_REPORT;
  const markerPath = process.env.ROW12_PANEL_READY_MARKER;
  const generatedRunnerHoldMs = Number("__PANEL_HOLD_MS__");
  const holdMsEnvRaw = process.env.ROW12_PANEL_HOLD_MS;
  const holdMs = holdMsEnvRaw ? Number(holdMsEnvRaw) : generatedRunnerHoldMs;
  if (!Number.isFinite(holdMs) || holdMs <= 5000) {
    throw new Error(`ROW12 panel hold must be explicitly propagated from the runner and greater than 5000ms; received ${holdMsEnvRaw || generatedRunnerHoldMs}`);
  }
  const holdMsSource = holdMsEnvRaw ? "ROW12_PANEL_HOLD_MS" : "generated_runner_hold_ms";
  const commandName = "xiigenSmartSkills.panel";
  const row13SelectedSkillTraceInput = {
    prompt: "",
    scenario: "positive_request",
    method: "prompt-to-task",
    requestedSkills: ["output-skepticism"],
    availableSkills: ["output-skepticism", "phase-completion-packager", "verification-before-completion", "plan-review"],
    redactedInput: {
      marker: "row13-3c-selected-skill-trace-fix",
      requested_skill_surface: "output-skepticism",
      available_skill_surface: "output-skepticism|phase-completion-packager|verification-before-completion|plan-review",
      privateDataIncluded: false
    }
  };
  const report = {
    schema: "xiigen.row12.cursor_panel.extension_host_open.v1",
    started_utc: started.toISOString(),
    host_kind: "Cursor",
    extension_development_host: true,
    command_invoked: commandName,
    targets_real_panel_command: true,
    test_only_handler_used: false,
    generic_chat_used: false,
    no_submit_performed: true,
    panel_title_expected: "XIIGen Smart Skills",
    prompt_textarea_selector_expected: "textarea#xiigen-smart-skills-prompt",
    submit_button_not_pressed: true,
    panel_hold_ms_generated_from_runner: generatedRunnerHoldMs,
    panel_hold_ms_env_raw: holdMsEnvRaw || null,
    panel_hold_ms_effective: holdMs,
    panel_hold_ms_source: holdMsSource,
    panel_hold_fallback_5000_used: false,
    product_readiness_claimed: false,
    row13_selected_skill_trace_input: {
      method: row13SelectedSkillTraceInput.method,
      requestedSkills: row13SelectedSkillTraceInput.requestedSkills,
      availableSkills: row13SelectedSkillTraceInput.availableSkills
    },
    errors: []
  };

  try {
    const extension =
      vscode.extensions.getExtension("xiigen.cursor-xiigen-smart-skills") ||
      vscode.extensions.getExtension("lubochka.cursor-xiigen-smart-skills");
    report.extension_visible_to_host = !!extension;
    if (!extension) {
      throw new Error("Cursor XIIGen development extension was not visible to the Extension Host.");
    }

    report.extension_id = extension.id;
    report.extension_path = extension.extensionPath;
    report.extension_was_active_before = extension.isActive;
    await extension.activate();
    report.extension_active_after_activate = extension.isActive;

    const result = await vscode.commands.executeCommand(commandName, row13SelectedSkillTraceInput);
    report.command_return_value = typeof result === "undefined" ? { type: "undefined" } : result;
    report.panelAction = result && Object.prototype.hasOwnProperty.call(result, "panelAction") ? result.panelAction : null;
    report.window_diagnostics_after_command = collectWindowDiagnostics();
    report.window_title_after_command = null;
    report.window_title_after_command_note = "The extension host API does not expose the native window title; the PowerShell runner records Cursor MainWindowTitle after the command.";
    report.command_resolved = true;
    report.command_result = result;
    report.open_only_result = result && result.panelOpenOnly === true;
    report.core_path_invoked = result && result.corePathInvoked === true;
    report.panel_submission_path_invoked = result && result.panelSubmissionPathInvoked === true;
    report.prompt_focus_requested = result && result.promptFocusRequested === true;
    report.no_submit_performed = report.core_path_invoked !== true && report.panel_submission_path_invoked !== true;
    report.ended_utc = new Date().toISOString();
    report.elapsed_ms = new Date(report.ended_utc).getTime() - started.getTime();
    writeJson(reportPath, report);
    writeJson(markerPath, {
      ready: true,
      ready_utc: new Date().toISOString(),
      command_invoked: commandName,
      no_submit_performed: report.no_submit_performed,
      panel_hold_ms_effective: holdMs,
      panel_hold_ms_source: holdMsSource,
      panel_hold_fallback_5000_used: false,
      report_path: reportPath
    });
    await new Promise((resolve) => setTimeout(resolve, holdMs));
  } catch (error) {
    report.command_resolved = false;
    report.errors.push({
      message: error && error.message ? error.message : String(error),
      stack: error && error.stack ? String(error.stack) : null
    });
    report.ended_utc = new Date().toISOString();
    report.elapsed_ms = new Date(report.ended_utc).getTime() - started.getTime();
    writeJson(reportPath, report);
    writeJson(markerPath, {
      ready: false,
      ready_utc: new Date().toISOString(),
      command_invoked: commandName,
      panel_hold_ms_effective: holdMs,
      panel_hold_ms_source: holdMsSource,
      panel_hold_fallback_5000_used: false,
      error: report.errors[0],
      report_path: reportPath
    });
    throw error;
  }
}

module.exports = { run };
'@

    $content = $content.Replace("__PANEL_HOLD_MS__", [string] $HoldMs)

    Write-TextFile -Path $Path -Value $content
}

function Invoke-WithTemporaryEnvironment {
    param(
        [hashtable] $Values,
        [scriptblock] $Action
    )

    $previous = @{}
    foreach ($key in $Values.Keys) {
        $previous[$key] = [Environment]::GetEnvironmentVariable($key, "Process")
        [Environment]::SetEnvironmentVariable($key, [string] $Values[$key], "Process")
    }

    try {
        & $Action
    } finally {
        foreach ($key in $Values.Keys) {
            [Environment]::SetEnvironmentVariable($key, $previous[$key], "Process")
        }
    }
}

function Stop-LaunchedCursorProcesses {
    param([datetime] $LaunchStart)
    $candidates = @(Get-Process Cursor -ErrorAction SilentlyContinue | Where-Object {
        try {
            $_.StartTime -ge $LaunchStart.AddSeconds(-2)
        } catch {
            $false
        }
    })

    foreach ($proc in $candidates) {
        try {
            Stop-Process -Id $proc.Id -Force -ErrorAction Stop
        } catch {
        }
    }
}

function Get-FreeLocalTcpPort {
    $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, 0)
    try {
        $listener.Start()
        return [int] $listener.LocalEndpoint.Port
    } finally {
        $listener.Stop()
    }
}

function Get-ObjectPropertyValue {
    param(
        [AllowNull()] [object] $Object,
        [string] $Name
    )

    if ($null -eq $Object) {
        return $null
    }

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

function ConvertTo-NonNullArray {
    param([AllowNull()] [object] $Value)

    if ($null -eq $Value) {
        return @()
    }

    return @($Value | Where-Object { $null -ne $_ })
}

function Get-Row12RuntimeEvidenceKind {
    param([string] $Name)

    switch -Regex ($Name) {
        "handler-only" { return "handler_only" }
        "entrypoint-response" { return "entrypoint_response" }
        "core-response" { return "response" }
        "core-trace" { return "core_trace" }
        "host-trace" { return "host_trace" }
        "panel-ui" { return "panel_ui" }
        "redacted-trace" { return "redacted_trace" }
        "request" { return "request" }
        "^xiigen-cursor-host-runtime-latest\.json$" { return "latest" }
        default { return $null }
    }
}

function Copy-Row12GlobalStorageRuntimeEvidence {
    param(
        [string] $UserDataDir,
        [string] $Destination
    )

    $globalStorageRoot = Join-Path $UserDataDir "User\globalStorage"
    $report = [ordered] @{
        source_root = $globalStorageRoot
        destination_root = $Destination
        attempted = $true
        source_exists = $false
        copied = @()
        errors = @()
    }

    if (-not (Test-Path -LiteralPath $globalStorageRoot)) {
        return [pscustomobject] $report
    }

    $report.source_exists = $true
    Ensure-Directory -Path $Destination
    $files = @(Get-ChildItem -LiteralPath $globalStorageRoot -Recurse -File -ErrorAction SilentlyContinue | Where-Object {
        $_.Name -like "xiigen-cursor-xiigenSmartSkills.panel-*.json" -or
        $_.Name -eq "xiigen-cursor-host-runtime-latest.json"
    })

    foreach ($file in $files) {
        try {
            $relative = $file.FullName.Substring($globalStorageRoot.Length).TrimStart([char[]] @("\", "/"))
            $destinationPath = Join-Path $Destination $relative
            Ensure-Directory -Path (Split-Path -Parent $destinationPath)
            Copy-Item -LiteralPath $file.FullName -Destination $destinationPath -Force -ErrorAction Stop
            $report.copied += [pscustomobject] @{
                kind = Get-Row12RuntimeEvidenceKind -Name $file.Name
                source = $file.FullName
                destination = $destinationPath
            }
        } catch {
            $report.errors += [pscustomobject] @{
                source = $file.FullName
                message = $_.Exception.Message
            }
        }
    }

    return [pscustomobject] $report
}

function Get-Row12RuntimeEvidenceSummary {
    param([string[]] $Roots)

    $kindOrder = @("request", "response", "core_trace", "host_trace", "panel_ui", "entrypoint_response", "redacted_trace", "latest", "handler_only")
    $byKind = @{}
    foreach ($kind in $kindOrder) {
        $byKind[$kind] = New-Object System.Collections.Generic.List[object]
    }

    $rootsChecked = @()
    foreach ($root in @($Roots | Where-Object { -not [string]::IsNullOrWhiteSpace([string] $_) } | Select-Object -Unique)) {
        $rootExists = Test-Path -LiteralPath $root
        $rootsChecked += [pscustomobject] @{
            path = $root
            exists = $rootExists
        }
        if (-not $rootExists) {
            continue
        }

        foreach ($file in @(Get-ChildItem -LiteralPath $root -Recurse -File -Filter "*.json" -ErrorAction SilentlyContinue)) {
            $kind = Get-Row12RuntimeEvidenceKind -Name $file.Name
            if ([string]::IsNullOrWhiteSpace([string] $kind)) {
                continue
            }
            $byKind[$kind].Add($file) | Out-Null
        }
    }

    $selected = [ordered] @{}
    $selectedFiles = @()
    foreach ($kind in $kindOrder) {
        $match = @($byKind[$kind].ToArray() | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1)
        if ($match.Count -gt 0) {
            $selected[$kind] = $match[0]
            $selectedFiles += [pscustomobject] @{
                kind = $kind
                path = $match[0].FullName
                last_write_utc = $match[0].LastWriteTimeUtc.ToString("o")
            }
        } else {
            $selected[$kind] = $null
        }
    }

    $requestRef = if ($selected.request) { [string] $selected.request.FullName } else { $null }
    $responseRef = if ($selected.response) { [string] $selected.response.FullName } else { $null }
    $coreTraceRef = if ($selected.core_trace) { [string] $selected.core_trace.FullName } else { $null }
    $hostTraceRef = if ($selected.host_trace) { [string] $selected.host_trace.FullName } else { $null }
    $panelUiRef = if ($selected.panel_ui) { [string] $selected.panel_ui.FullName } else { $null }
    $entrypointResponseRef = if ($selected.entrypoint_response) { [string] $selected.entrypoint_response.FullName } else { $null }
    $redactedTraceRef = if ($selected.redacted_trace) { [string] $selected.redacted_trace.FullName } else { $null }
    $latestRef = if ($selected.latest) { [string] $selected.latest.FullName } else { $null }
    $handlerOnlyRef = if ($selected.handler_only) { [string] $selected.handler_only.FullName } else { $null }

    $combinedEvidenceText = ""
    foreach ($file in @($selectedFiles | Where-Object { $_.path })) {
        try {
            $combinedEvidenceText += "`n"
            $combinedEvidenceText += Get-Content -Raw -LiteralPath $file.path -ErrorAction Stop
        } catch {
        }
    }

    $testOnlyTruthy = (-not [string]::IsNullOrWhiteSpace([string] $handlerOnlyRef)) -or
        ($combinedEvidenceText -match '"(testOnlyHandlerUsed|submit_test_only_handler_used|test_only_handler_used)"\s*:\s*true')
    $readinessTruthy = $combinedEvidenceText -match '"(readinessClaimed|readiness_claimed|productReadinessClaimed|product_readiness_claimed)"\s*:\s*true'
    $tokenTruthy = $combinedEvidenceText -match '"(tokenSavingsClaimed|token_savings_claimed)"\s*:\s*true'
    $semanticTruthy = $combinedEvidenceText -match '"(semanticQualityClaimed|semantic_quality_claimed)"\s*:\s*true'
    $trainingTruthy = $combinedEvidenceText -match '"(trainingClaimed|training_claimed)"\s*:\s*true'

    $coreHostArtifactsPresent =
        (-not [string]::IsNullOrWhiteSpace([string] $requestRef)) -and
        (-not [string]::IsNullOrWhiteSpace([string] $responseRef)) -and
        (-not [string]::IsNullOrWhiteSpace([string] $coreTraceRef)) -and
        (-not [string]::IsNullOrWhiteSpace([string] $hostTraceRef))
    $forbiddenClaimsAbsent =
        (-not $testOnlyTruthy) -and
        (-not $readinessTruthy) -and
        (-not $tokenTruthy) -and
        (-not $semanticTruthy) -and
        (-not $trainingTruthy)

    return [pscustomobject] [ordered] @{
        schema = "xiigen.row12.submit_result.runtime_artifact_evidence.v1"
        roots_checked = @($rootsChecked)
        selected_files = @($selectedFiles)
        request_ref = $requestRef
        response_ref = $responseRef
        core_trace_ref = $coreTraceRef
        host_trace_ref = $hostTraceRef
        panel_ui_ref = $panelUiRef
        entrypoint_response_ref = $entrypointResponseRef
        redacted_trace_ref = $redactedTraceRef
        latest_ref = $latestRef
        handler_only_ref = $handlerOnlyRef
        evidence_refs = [ordered] @{
            request_ref = $requestRef
            response_ref = $responseRef
            core_trace_ref = $coreTraceRef
            host_trace_ref = $hostTraceRef
            panel_ui_ref = $panelUiRef
            entrypoint_response_ref = $entrypointResponseRef
            redacted_trace_ref = $redactedTraceRef
            latest_ref = $latestRef
            handler_only_ref = $handlerOnlyRef
        }
        actual_submit_artifacts_present = $coreHostArtifactsPresent
        panel_ui_artifact_available = (-not [string]::IsNullOrWhiteSpace([string] $panelUiRef))
        classification_can_use = ($coreHostArtifactsPresent -and $forbiddenClaimsAbsent)
        classification_basis = "request + core-response + core-trace + host-trace artifacts under the Row12 runtime evidence roots; panel-ui is recorded and used when available"
        forbidden_claims_absent = $forbiddenClaimsAbsent
        test_only_handler_used = $testOnlyTruthy
        readiness_claimed = $readinessTruthy
        token_savings_claimed = $tokenTruthy
        semantic_quality_claimed = $semanticTruthy
        training_claimed = $trainingTruthy
    }
}

function ConvertTo-CdpTargetSummary {
    param(
        [object] $Target,
        [int] $Index,
        [string] $Source = "json_list"
    )

    $id = Get-ObjectPropertyValue -Object $Target -Name "id"
    $targetId = Get-ObjectPropertyValue -Object $Target -Name "targetId"
    if ([string]::IsNullOrWhiteSpace([string] $id)) {
        $id = $targetId
    }
    $type = Get-ObjectPropertyValue -Object $Target -Name "type"
    $title = Get-ObjectPropertyValue -Object $Target -Name "title"
    $url = Get-ObjectPropertyValue -Object $Target -Name "url"
    $description = Get-ObjectPropertyValue -Object $Target -Name "description"
    $webSocketDebuggerUrl = Get-ObjectPropertyValue -Object $Target -Name "webSocketDebuggerUrl"
    $attached = Get-ObjectPropertyValue -Object $Target -Name "attached"
    $openerId = Get-ObjectPropertyValue -Object $Target -Name "openerId"
    $canAccessOpener = Get-ObjectPropertyValue -Object $Target -Name "canAccessOpener"
    $browserContextId = Get-ObjectPropertyValue -Object $Target -Name "browserContextId"
    $haystack = @($type, $title, $url, $description) -join "`n"
    $candidateReasons = @()

    if ($haystack -match "(?i)vscode-webview") {
        $candidateReasons += "vscode-webview"
    }
    if ($haystack -match "(?i)xiigen") {
        $candidateReasons += "xiigen"
    }
    if ($haystack -match "(?i)smart skills") {
        $candidateReasons += "smart-skills"
    }

    return [pscustomobject] [ordered] @{
        index = $Index
        source = $Source
        id = $id
        targetId = $targetId
        type = $type
        title = $title
        url = $url
        description = $description
        attached = $attached
        openerId = $openerId
        canAccessOpener = $canAccessOpener
        browserContextId = $browserContextId
        webSocketDebuggerUrl = $webSocketDebuggerUrl
        has_websocket_debugger_url = -not [string]::IsNullOrWhiteSpace([string] $webSocketDebuggerUrl)
        is_candidate = $candidateReasons.Count -gt 0
        candidate_reasons = @($candidateReasons)
    }
}

function Receive-CdpWebSocketMessage {
    param(
        [System.Net.WebSockets.ClientWebSocket] $Socket,
        [int] $TimeoutSec = 5
    )

    $buffer = New-Object byte[] 65536
    $stream = New-Object System.IO.MemoryStream
    try {
        do {
            $cancel = New-Object System.Threading.CancellationTokenSource
            try {
                $cancel.CancelAfter([TimeSpan]::FromSeconds($TimeoutSec))
                $segment = [System.ArraySegment[byte]]::new($buffer)
                $result = $Socket.ReceiveAsync($segment, $cancel.Token).GetAwaiter().GetResult()
            } finally {
                $cancel.Dispose()
            }

            if ($result.MessageType -eq [System.Net.WebSockets.WebSocketMessageType]::Close) {
                throw "CDP websocket closed before the matching response was received."
            }

            if ($result.Count -gt 0) {
                [void] $stream.Write($buffer, 0, $result.Count)
            }
        } while (-not $result.EndOfMessage)

        $text = [System.Text.Encoding]::UTF8.GetString($stream.ToArray())
        if ([string]::IsNullOrWhiteSpace($text)) {
            return $null
        }

        return ($text | ConvertFrom-Json)
    } finally {
        $stream.Dispose()
    }
}

function Invoke-CdpWebSocketCommand {
    param(
        [System.Net.WebSockets.ClientWebSocket] $Socket,
        [int] $Id,
        [string] $Method,
        [AllowNull()] [object] $Params,
        [AllowNull()] [string] $SessionId = $null,
        [int] $TimeoutSec = 5,
        [AllowNull()] [System.Collections.IList] $EventSink = $null
    )

    if ($null -eq $Params) {
        $Params = @{}
    } elseif ($Params -is [System.Collections.IDictionary] -and $Params -isnot [hashtable]) {
        $normalizedParams = @{}
        foreach ($key in $Params.Keys) {
            $normalizedParams[[string] $key] = $Params[$key]
        }
        $Params = $normalizedParams
    }

    $message = [ordered] @{
        id = $Id
        method = $Method
        params = $Params
    }
    if (-not [string]::IsNullOrWhiteSpace($SessionId)) {
        $message.sessionId = $SessionId
    }
    $json = $message | ConvertTo-Json -Depth 20 -Compress
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    $sendSegment = [System.ArraySegment[byte]]::new($bytes)
    [void] $Socket.SendAsync($sendSegment, [System.Net.WebSockets.WebSocketMessageType]::Text, $true, [System.Threading.CancellationToken]::None).GetAwaiter().GetResult()

    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    do {
        $response = Receive-CdpWebSocketMessage -Socket $Socket -TimeoutSec $TimeoutSec
        if ($response -and (Get-ObjectPropertyValue -Object $response -Name "id") -eq $Id) {
            return $response
        }
        if ($response -and $null -ne $EventSink) {
            [void] $EventSink.Add($response)
        }
    } while ((Get-Date) -lt $deadline)

    throw "Timed out waiting for CDP response id $Id for method $Method."
}

function Read-CdpWebSocketEvents {
    param(
        [System.Net.WebSockets.ClientWebSocket] $Socket,
        [System.Collections.IList] $EventSink,
        [int] $MaxMessages = 20,
        [int] $TimeoutSec = 1
    )

    for ($i = 0; $i -lt $MaxMessages; $i += 1) {
        try {
            $message = Receive-CdpWebSocketMessage -Socket $Socket -TimeoutSec $TimeoutSec
            if ($message) {
                [void] $EventSink.Add($message)
            }
        } catch {
            break
        }
    }
}

function Invoke-CdpRuntimeEvaluate {
    param(
        [string] $WebSocketDebuggerUrl,
        [string] $Expression,
        [int] $TimeoutSec = 8
    )

    $socket = [System.Net.WebSockets.ClientWebSocket]::new()
    try {
        $cancel = New-Object System.Threading.CancellationTokenSource
        try {
            $cancel.CancelAfter([TimeSpan]::FromSeconds($TimeoutSec))
            [void] $socket.ConnectAsync([Uri] $WebSocketDebuggerUrl, $cancel.Token).GetAwaiter().GetResult()
        } finally {
            $cancel.Dispose()
        }

        return Invoke-CdpWebSocketCommand -Socket $socket -Id 1 -Method "Runtime.evaluate" -Params @{
            expression = $Expression
            returnByValue = $true
            awaitPromise = $true
            userGesture = $false
        } -TimeoutSec $TimeoutSec
    } finally {
        try {
            if ($socket.State -eq [System.Net.WebSockets.WebSocketState]::Open) {
                [void] $socket.CloseAsync(
                    [System.Net.WebSockets.WebSocketCloseStatus]::NormalClosure,
                    "row12-read-only-complete",
                    [System.Threading.CancellationToken]::None
                ).GetAwaiter().GetResult()
            }
        } catch {
        }
        $socket.Dispose()
    }
}

function Invoke-CdpBrowserTargetRuntimeEvaluate {
    param(
        [string] $BrowserWebSocketDebuggerUrl,
        [string] $TargetId,
        [string] $Expression,
        [int] $TimeoutSec = 8
    )

    $socket = [System.Net.WebSockets.ClientWebSocket]::new()
    try {
        $cancel = New-Object System.Threading.CancellationTokenSource
        try {
            $cancel.CancelAfter([TimeSpan]::FromSeconds($TimeoutSec))
            [void] $socket.ConnectAsync([Uri] $BrowserWebSocketDebuggerUrl, $cancel.Token).GetAwaiter().GetResult()
        } finally {
            $cancel.Dispose()
        }

        $attachResponse = Invoke-CdpWebSocketCommand -Socket $socket -Id 1 -Method "Target.attachToTarget" -Params @{
            targetId = $TargetId
            flatten = $true
        } -TimeoutSec $TimeoutSec
        $attachResult = Get-ObjectPropertyValue -Object $attachResponse -Name "result"
        $sessionId = Get-ObjectPropertyValue -Object $attachResult -Name "sessionId"
        if ([string]::IsNullOrWhiteSpace([string] $sessionId)) {
            throw "Target.attachToTarget did not return a sessionId for target $TargetId."
        }

        return Invoke-CdpWebSocketCommand -Socket $socket -Id 2 -Method "Runtime.evaluate" -Params @{
            expression = $Expression
            returnByValue = $true
            awaitPromise = $true
            userGesture = $false
        } -SessionId ([string] $sessionId) -TimeoutSec $TimeoutSec
    } finally {
        try {
            if ($socket.State -eq [System.Net.WebSockets.WebSocketState]::Open) {
                [void] $socket.CloseAsync(
                    [System.Net.WebSockets.WebSocketCloseStatus]::NormalClosure,
                    "row12-browser-target-read-only-complete",
                    [System.Threading.CancellationToken]::None
                ).GetAwaiter().GetResult()
            }
        } catch {
        }
        $socket.Dispose()
    }
}

function New-Row12TypeabilityProbeExpression {
    param([string] $ProbeText)

    $probeTextJson = $ProbeText | ConvertTo-Json -Compress
    return @"
(() => {
  const probeText = $probeTextJson;
  const selector = "#xiigen-smart-skills-prompt";
  const submitSelector = "#xiigen-smart-skills-submit";
  const result = {
    schema: "xiigen.row12.cursor_panel.typeability_probe.v2",
    attempted: true,
    selector,
    submitSelector,
    found: false,
    submitButtonFound: false,
    submitButtonDisabled: null,
    documentResolution: "document_plus_accessible_iframe_contentDocument",
    documentCandidateCount: 0,
    accessibleIframeDocumentCount: 0,
    documentCandidates: [],
    selectedDocumentLabel: null,
    selectedDocumentSource: null,
    selectedDocumentFrameMeta: null,
    selectedDocumentContainsPrompt: false,
    readbackValue: null,
    readbackMatches: false,
    inputEventDispatched: false,
    changeEventDispatched: false,
    inputEvent: { dispatched: false },
    changeEvent: { dispatched: false },
    afterValue: null,
    readbackMatchesProbe: false,
    submitClicked: false,
    submitButtonClickInvoked: false,
    submitButtonClickedByProbe: false,
    noSubmitPerformedByProbe: true,
    corePathInvokedByProbe: false,
    panelSubmissionPathInvokedByProbe: false,
    error: null
  };

  function makeEvent(type) {
    if (type === "input" && typeof InputEvent === "function") {
      return new InputEvent("input", {
        bubbles: true,
        cancelable: true,
        composed: true,
        inputType: "insertText",
        data: probeText
      });
    }
    return new Event(type, {
      bubbles: true,
      cancelable: true,
      composed: true
    });
  }

  try {
    if (typeof document === "undefined") {
      result.error = "Runtime context has no document object.";
      return result;
    }

    const candidates = [];
    candidates.push({
      label: "document",
      source: "document",
      doc: document,
      frameMeta: null,
      accessible: true,
      error: null
    });

    const iframes = Array.from(document.querySelectorAll("iframe")).slice(0, 25);
    for (let index = 0; index < iframes.length; index += 1) {
      const frame = iframes[index];
      const rect = frame.getBoundingClientRect();
      const frameMeta = {
        index,
        id: frame.id || null,
        name: frame.getAttribute("name"),
        title: frame.getAttribute("title"),
        src: frame.getAttribute("src"),
        className: frame.className || null,
        rect: {
          x: rect.x,
          y: rect.y,
          width: rect.width,
          height: rect.height
        },
        visibleByLayout: !!(frame.offsetWidth || frame.offsetHeight || frame.getClientRects().length)
      };
      try {
        const childDocument = frame.contentDocument;
        if (childDocument) {
          candidates.push({
            label: "iframe[" + index + "].contentDocument",
            source: "iframe.contentDocument",
            doc: childDocument,
            frameMeta,
            accessible: true,
            error: null
          });
        }
      } catch (error) {
        result.documentCandidates.push({
          label: "iframe[" + index + "].contentDocument",
          source: "iframe.contentDocument",
          accessible: false,
          frameMeta,
          error: error && error.message ? error.message : String(error),
          containsPrompt: false,
          containsSubmit: false
        });
      }
    }

    let selected = null;
    for (const candidate of candidates) {
      let containsPrompt = false;
      let containsSubmit = false;
      let candidateError = candidate.error;
      try {
        containsPrompt = !!candidate.doc.querySelector(selector);
        containsSubmit = !!candidate.doc.querySelector(submitSelector);
      } catch (error) {
        candidateError = error && error.message ? error.message : String(error);
      }

      result.documentCandidates.push({
        label: candidate.label,
        source: candidate.source,
        accessible: candidate.accessible,
        frameMeta: candidate.frameMeta,
        error: candidateError,
        containsPrompt,
        containsSubmit
      });

      if (!selected && containsPrompt) {
        selected = candidate;
      }
    }

    result.documentCandidateCount = result.documentCandidates.length;
    result.accessibleIframeDocumentCount = result.documentCandidates.filter((entry) => entry.source === "iframe.contentDocument" && entry.accessible === true).length;

    if (!selected) {
      result.error = "Textarea selector was not found in document or accessible iframe.contentDocument contexts.";
      return result;
    }

    result.selectedDocumentLabel = selected.label;
    result.selectedDocumentSource = selected.source;
    result.selectedDocumentFrameMeta = selected.frameMeta;
    result.selectedDocumentContainsPrompt = true;

    const textarea = selected.doc.querySelector(selector);
    const submitButton = selected.doc.querySelector(submitSelector);
    result.found = !!textarea;
    result.submitButtonFound = !!submitButton;
    result.submitButtonDisabled = submitButton ? !!submitButton.disabled : null;

    if (!textarea) {
      result.error = "Textarea selector was not found in selected document context.";
      return result;
    }

    textarea.value = probeText;
    result.inputEventDispatched = textarea.dispatchEvent(makeEvent("input")) === true;
    result.changeEventDispatched = textarea.dispatchEvent(makeEvent("change")) === true;
    result.inputEvent.dispatched = result.inputEventDispatched;
    result.changeEvent.dispatched = result.changeEventDispatched;
    result.readbackValue = typeof textarea.value === "string" ? textarea.value : null;
    result.afterValue = result.readbackValue;
    result.readbackMatches = result.readbackValue === probeText;
    result.readbackMatchesProbe = result.readbackMatches;
    return result;
  } catch (error) {
    result.error = error && error.message ? error.message : String(error);
    return result;
  }
})()
"@
}

function New-Row12SubmitResultProbeExpression {
    param(
        [string] $ProbeText,
        [int] $SubmitWaitMs = 20000
    )

    $probeTextJson = $ProbeText | ConvertTo-Json -Compress
    $submitWaitMsJson = $SubmitWaitMs | ConvertTo-Json -Compress
    return @"
(async () => {
  const probeText = $probeTextJson;
  const selector = "#xiigen-smart-skills-prompt";
  const submitSelector = "#xiigen-smart-skills-submit";
  const statusSelector = "#xiigen-smart-skills-status";
  const resultSelector = "#xiigen-smart-skills-result";
  const responseRefSelector = "#xiigen-smart-skills-response-ref";
  const traceRefSelector = "#xiigen-smart-skills-trace-ref";
  const result = {
    schema: "xiigen.row12.cursor_panel.submit_result_probe.v1",
    attempted: true,
    submitProbeAttempted: true,
    selector,
    submitSelector,
    statusSelector,
    resultSelector,
    responseRefSelector,
    traceRefSelector,
    found: false,
    submitButtonFound: false,
    submitButtonDisabledBeforeClick: null,
    submitButtonDisabledAfterClick: null,
    textareaVisibleByLayout: false,
    submitButtonVisibleByLayout: false,
    documentResolution: "document_plus_accessible_iframe_contentDocument",
    documentCandidateCount: 0,
    accessibleIframeDocumentCount: 0,
    documentCandidates: [],
    selectedDocumentLabel: null,
    selectedDocumentSource: null,
    selectedDocumentFrameMeta: null,
    selectedDocumentContainsPrompt: false,
    inputEventDispatched: false,
    changeEventDispatched: false,
    inputEvent: { dispatched: false },
    changeEvent: { dispatched: false },
    readbackValue: null,
    readbackMatchesProbe: false,
    submitButtonClickInvoked: false,
    submitButtonClickedByProbe: false,
    submitClickMethod: null,
    panelSubmissionPathInvokedByProbe: false,
    corePathInvokedByProbe: false,
    testOnlyHandlerUsedByProbe: false,
    notTestOnlyPath: false,
    resultRenderedInWebview: false,
    statusTextBeforeSubmit: null,
    statusTextAfterSubmit: null,
    resultTextBeforeSubmit: null,
    resultTextAfterSubmit: null,
    resultText: null,
    responseRef: null,
    traceRef: null,
    hostPanelUiRef: null,
    correlationId: null,
    evidenceRefs: {
      response_ref: null,
      trace_ref: null,
      host_panel_ui_ref: null
    },
    waitMs: $submitWaitMsJson,
    waitPolls: 0,
    waitTimedOut: false,
    noReadinessClaim: true,
    readinessClaimed: false,
    tokenSavingsClaimed: false,
    semanticQualityClaimed: false,
    trainingClaimed: false,
    error: null
  };

  function safeText(node) {
    return node && typeof node.textContent === "string" ? node.textContent.trim() : "";
  }

  function visibleByLayout(node) {
    return !!(node && (node.offsetWidth || node.offsetHeight || node.getClientRects().length));
  }

  function makeEvent(doc, type) {
    const view = doc.defaultView || window;
    if (type === "input" && typeof view.InputEvent === "function") {
      return new view.InputEvent("input", {
        bubbles: true,
        cancelable: true,
        composed: true,
        inputType: "insertText",
        data: probeText
      });
    }
    return new view.Event(type, {
      bubbles: true,
      cancelable: true,
      composed: true
    });
  }

  function inferHostPanelUiRef(traceRef) {
    if (!traceRef) {
      return "";
    }
    if (!/host-trace-[^\\/]+\.json$/i.test(traceRef)) {
      return "";
    }
    return traceRef.replace(/host-trace-([^\\/]+\.json)$/i, "panel-ui-$1");
  }

  function getInitialCorrelationId() {
    try {
      if (typeof initialState !== "undefined" && initialState && typeof initialState.correlationId === "string") {
        return initialState.correlationId;
      }
    } catch {
    }
    return null;
  }

  function collectDocuments() {
    const candidates = [];
    candidates.push({
      label: "document",
      source: "document",
      doc: document,
      frameMeta: null,
      accessible: true,
      error: null
    });

    const iframes = Array.from(document.querySelectorAll("iframe")).slice(0, 25);
    for (let index = 0; index < iframes.length; index += 1) {
      const frame = iframes[index];
      const rect = frame.getBoundingClientRect();
      const frameMeta = {
        index,
        id: frame.id || null,
        name: frame.getAttribute("name"),
        title: frame.getAttribute("title"),
        src: frame.getAttribute("src"),
        className: frame.className || null,
        rect: {
          x: rect.x,
          y: rect.y,
          width: rect.width,
          height: rect.height
        },
        visibleByLayout: visibleByLayout(frame)
      };
      try {
        const childDocument = frame.contentDocument;
        if (childDocument) {
          candidates.push({
            label: "iframe[" + index + "].contentDocument",
            source: "iframe.contentDocument",
            doc: childDocument,
            frameMeta,
            accessible: true,
            error: null
          });
        }
      } catch (error) {
        result.documentCandidates.push({
          label: "iframe[" + index + "].contentDocument",
          source: "iframe.contentDocument",
          accessible: false,
          frameMeta,
          error: error && error.message ? error.message : String(error),
          containsPrompt: false,
          containsSubmit: false
        });
      }
    }
    return candidates;
  }

  try {
    if (typeof document === "undefined") {
      result.error = "Runtime context has no document object.";
      return result;
    }

    const candidates = collectDocuments();
    let selected = null;
    for (const candidate of candidates) {
      let containsPrompt = false;
      let containsSubmit = false;
      let candidateError = candidate.error;
      try {
        containsPrompt = !!candidate.doc.querySelector(selector);
        containsSubmit = !!candidate.doc.querySelector(submitSelector);
      } catch (error) {
        candidateError = error && error.message ? error.message : String(error);
      }

      result.documentCandidates.push({
        label: candidate.label,
        source: candidate.source,
        accessible: candidate.accessible,
        frameMeta: candidate.frameMeta,
        error: candidateError,
        containsPrompt,
        containsSubmit
      });

      if (!selected && containsPrompt && containsSubmit) {
        selected = candidate;
      }
    }

    result.documentCandidateCount = result.documentCandidates.length;
    result.accessibleIframeDocumentCount = result.documentCandidates.filter((entry) => entry.source === "iframe.contentDocument" && entry.accessible === true).length;

    if (!selected) {
      result.error = "Prompt and submit selectors were not found in the same document or accessible iframe.contentDocument context.";
      return result;
    }

    result.selectedDocumentLabel = selected.label;
    result.selectedDocumentSource = selected.source;
    result.selectedDocumentFrameMeta = selected.frameMeta;
    result.selectedDocumentContainsPrompt = true;
    result.correlationId = getInitialCorrelationId();

    const doc = selected.doc;
    const textarea = doc.querySelector(selector);
    const submitButton = doc.querySelector(submitSelector);
    const statusNode = doc.querySelector(statusSelector);
    const resultNode = doc.querySelector(resultSelector);
    const responseRefNode = doc.querySelector(responseRefSelector);
    const traceRefNode = doc.querySelector(traceRefSelector);
    result.found = !!textarea;
    result.submitButtonFound = !!submitButton;
    result.submitButtonDisabledBeforeClick = submitButton ? !!submitButton.disabled : null;
    result.textareaVisibleByLayout = visibleByLayout(textarea);
    result.submitButtonVisibleByLayout = visibleByLayout(submitButton);

    if (!textarea) {
      result.error = "Textarea selector was not found in selected document context.";
      return result;
    }
    if (!submitButton) {
      result.error = "Submit selector was not found in selected document context.";
      return result;
    }

    result.statusTextBeforeSubmit = safeText(statusNode);
    result.resultTextBeforeSubmit = safeText(resultNode);

    textarea.value = probeText;
    result.inputEventDispatched = textarea.dispatchEvent(makeEvent(doc, "input")) === true;
    result.changeEventDispatched = textarea.dispatchEvent(makeEvent(doc, "change")) === true;
    result.inputEvent.dispatched = result.inputEventDispatched;
    result.changeEvent.dispatched = result.changeEventDispatched;
    result.readbackValue = typeof textarea.value === "string" ? textarea.value : null;
    result.readbackMatchesProbe = result.readbackValue === probeText;

    try {
      if (typeof textarea.focus === "function") {
        textarea.focus();
      }
      if (typeof submitButton.focus === "function") {
        submitButton.focus();
      }
    } catch {
    }

    result.submitButtonClickInvoked = true;
    if (typeof submitButton.click === "function") {
      submitButton.click();
      result.submitClickMethod = "HTMLElement.click";
    } else {
      const view = doc.defaultView || window;
      submitButton.dispatchEvent(new view.MouseEvent("click", {
        bubbles: true,
        cancelable: true,
        composed: true,
        view
      }));
      result.submitClickMethod = "dispatchEvent_MouseEvent_click";
    }
    result.submitButtonClickedByProbe = true;

    const deadline = Date.now() + result.waitMs;
    let rendered = false;
    while (Date.now() < deadline) {
      result.waitPolls += 1;
      await new Promise((resolve) => setTimeout(resolve, 250));
      const statusText = safeText(statusNode);
      const resultText = safeText(resultNode);
      const responseRef = safeText(responseRefNode);
      const traceRef = safeText(traceRefNode);
      const statusReachedResult = /Core response rendered in the panel|Panel request returned an error/i.test(statusText);
      const refsPresent = responseRef.length > 0 || traceRef.length > 0;
      const resultChanged = resultText.length > 0 && resultText !== result.resultTextBeforeSubmit;
      if (statusReachedResult || refsPresent || resultChanged) {
        rendered = true;
        break;
      }
    }

    result.statusTextAfterSubmit = safeText(statusNode);
    result.resultTextAfterSubmit = safeText(resultNode);
    result.resultText = result.resultTextAfterSubmit;
    result.responseRef = safeText(responseRefNode);
    result.traceRef = safeText(traceRefNode);
    result.hostPanelUiRef = inferHostPanelUiRef(result.traceRef);
    result.evidenceRefs.response_ref = result.responseRef;
    result.evidenceRefs.trace_ref = result.traceRef;
    result.evidenceRefs.host_panel_ui_ref = result.hostPanelUiRef;
    result.submitButtonDisabledAfterClick = submitButton ? !!submitButton.disabled : null;
    result.resultRenderedInWebview = rendered || (
      result.resultTextAfterSubmit.length > 0 &&
      result.resultTextAfterSubmit !== result.resultTextBeforeSubmit
    );
    result.waitTimedOut = !result.resultRenderedInWebview;
    const combinedEvidence = [
      result.statusTextAfterSubmit,
      result.resultTextAfterSubmit,
      result.responseRef,
      result.traceRef,
      result.hostPanelUiRef
    ].join("\n");
    result.testOnlyHandlerUsedByProbe = /TEST_ONLY|handler-only/i.test(combinedEvidence);
    result.notTestOnlyPath = result.testOnlyHandlerUsedByProbe !== true;
    result.panelSubmissionPathInvokedByProbe =
      result.submitButtonClickInvoked === true &&
      result.submitButtonClickedByProbe === true &&
      result.resultRenderedInWebview === true &&
      result.notTestOnlyPath === true;
    result.corePathInvokedByProbe =
      result.resultRenderedInWebview === true &&
      result.responseRef.length > 0 &&
      result.traceRef.length > 0 &&
      result.notTestOnlyPath === true;
    return result;
  } catch (error) {
    result.error = error && error.message ? error.message : String(error);
    return result;
  }
})()
"@
}

function New-Row12SubmitResultProofRecord {
    param(
        [string] $Schema,
        [string] $TargetId,
        [AllowNull()] [string] $TargetSource,
        [AllowNull()] [string] $SessionId,
        [AllowNull()] [object] $FrameId,
        [AllowNull()] [object] $ExecutionContextId,
        [string] $ContextSource,
        [bool] $SelectorContextMatchesVisibilityContext,
        [AllowNull()] [string] $CommandPath,
        [AllowNull()] [string] $EvaluationLabel,
        [AllowNull()] [object] $FrameIndex,
        [string] $ProbeText,
        [string] $ExpressionHash,
        [int] $SubmitWaitMs = 20000,
        [int] $RuntimeEvaluateTimeoutSec = 30
    )

    return [ordered] @{
        schema = $Schema
        attempted = $true
        status = "SUBMIT_RESULT_NEEDS_FIX"
        method = "Runtime.evaluate_set_prompt_dispatch_input_change_click_visible_submit_wait_result"
        target_id = $TargetId
        submit_context_source = $ContextSource
        submit_target_id = $TargetId
        submit_target_source = $TargetSource
        submit_session_id = $SessionId
        submit_frame_id = $FrameId
        submit_execution_context_id = $ExecutionContextId
        submit_selector_context_matches_visibility_context = $SelectorContextMatchesVisibilityContext
        command_path = $CommandPath
        evaluation_label = $EvaluationLabel
        frame_index = $FrameIndex
        selector = "#xiigen-smart-skills-prompt"
        submit_selector = "#xiigen-smart-skills-submit"
        status_selector = "#xiigen-smart-skills-status"
        result_selector = "#xiigen-smart-skills-result"
        response_ref_selector = "#xiigen-smart-skills-response-ref"
        trace_ref_selector = "#xiigen-smart-skills-trace-ref"
        probe_text = $ProbeText
        probe_text_sha256 = Get-Sha256Hex -Value $ProbeText
        submit_probe_attempted = $true
        submit_probe_text = $ProbeText
        submit_probe_text_sha256 = Get-Sha256Hex -Value $ProbeText
        submit_probe_text_hash = Get-Sha256Hex -Value $ProbeText
        runtime_evaluate_expression_sha256 = $ExpressionHash
        submit_js_wait_ms = $SubmitWaitMs
        runtime_evaluate_timeout_sec = $RuntimeEvaluateTimeoutSec
        submit_proof_scope = "first_attached_runtime_visible_context_path"
        submit_retry_policy = "Prefer the first attached-runtime visible context path; frame/evaluated submit proof is allowed only if no attached-runtime submit path was attempted."
        runtime_evaluate_response = $null
        runtime_exception = $null
        error = $null
        raw_result = $null
        readback_value = $null
        readback_matches_probe = $false
        input_event_dispatched = $false
        change_event_dispatched = $false
        submit_button_found = $false
        submit_button_disabled_before_click = $null
        submit_button_disabled_after_click = $null
        textarea_visible_by_layout = $false
        submit_button_visible_by_layout = $false
        submit_button_click_invoked = $false
        submit_button_clicked_by_probe = $false
        submit_click_method = $null
        panel_submission_path_invoked = $false
        core_path_invoked = $false
        submit_test_only_handler_used = $false
        submit_not_test_only_path = $false
        result_rendered_in_webview = $false
        result_text = $null
        result_text_sha256 = $null
        result_status_text = $null
        result_response_ref = $null
        result_trace_ref = $null
        result_host_panel_ui_ref = $null
        result_correlation_id = $null
        evidence_refs = $null
        no_readiness_claim = $true
        readiness_claimed = $false
        token_savings_claimed = $false
        semantic_quality_claimed = $false
        training_claimed = $false
    }
}

function Update-Row12SubmitResultProofRecord {
    param(
        [System.Collections.IDictionary] $Record,
        [AllowNull()] [object] $Value
    )

    $Record.raw_result = $Value
    if ($null -eq $Value) {
        return
    }

    $Record.readback_value = Get-ObjectPropertyValue -Object $Value -Name "readbackValue"
    $Record.readback_matches_probe = (Get-ObjectPropertyValue -Object $Value -Name "readbackMatchesProbe") -eq $true
    $inputEvent = Get-ObjectPropertyValue -Object $Value -Name "inputEvent"
    $changeEvent = Get-ObjectPropertyValue -Object $Value -Name "changeEvent"
    $Record.input_event_dispatched =
        ((Get-ObjectPropertyValue -Object $Value -Name "inputEventDispatched") -eq $true) -or
        ($inputEvent -and (Get-ObjectPropertyValue -Object $inputEvent -Name "dispatched") -eq $true)
    $Record.change_event_dispatched =
        ((Get-ObjectPropertyValue -Object $Value -Name "changeEventDispatched") -eq $true) -or
        ($changeEvent -and (Get-ObjectPropertyValue -Object $changeEvent -Name "dispatched") -eq $true)
    $Record.submit_button_found = (Get-ObjectPropertyValue -Object $Value -Name "submitButtonFound") -eq $true
    $Record.submit_button_disabled_before_click = Get-ObjectPropertyValue -Object $Value -Name "submitButtonDisabledBeforeClick"
    $Record.submit_button_disabled_after_click = Get-ObjectPropertyValue -Object $Value -Name "submitButtonDisabledAfterClick"
    $Record.textarea_visible_by_layout = (Get-ObjectPropertyValue -Object $Value -Name "textareaVisibleByLayout") -eq $true
    $Record.submit_button_visible_by_layout = (Get-ObjectPropertyValue -Object $Value -Name "submitButtonVisibleByLayout") -eq $true
    $Record.submit_button_click_invoked = (Get-ObjectPropertyValue -Object $Value -Name "submitButtonClickInvoked") -eq $true
    $Record.submit_button_clicked_by_probe = (Get-ObjectPropertyValue -Object $Value -Name "submitButtonClickedByProbe") -eq $true
    $Record.submit_click_method = Get-ObjectPropertyValue -Object $Value -Name "submitClickMethod"
    $Record.panel_submission_path_invoked = (Get-ObjectPropertyValue -Object $Value -Name "panelSubmissionPathInvokedByProbe") -eq $true
    $Record.core_path_invoked = (Get-ObjectPropertyValue -Object $Value -Name "corePathInvokedByProbe") -eq $true
    $Record.submit_test_only_handler_used = (Get-ObjectPropertyValue -Object $Value -Name "testOnlyHandlerUsedByProbe") -eq $true
    $Record.submit_not_test_only_path = (Get-ObjectPropertyValue -Object $Value -Name "notTestOnlyPath") -eq $true
    $Record.result_rendered_in_webview = (Get-ObjectPropertyValue -Object $Value -Name "resultRenderedInWebview") -eq $true
    $Record.result_text = Get-ObjectPropertyValue -Object $Value -Name "resultText"
    if ($null -ne $Record.result_text) {
        $Record.result_text_sha256 = Get-Sha256Hex -Value ([string] $Record.result_text)
    }
    $Record.result_status_text = Get-ObjectPropertyValue -Object $Value -Name "statusTextAfterSubmit"
    $Record.result_response_ref = Get-ObjectPropertyValue -Object $Value -Name "responseRef"
    $Record.result_trace_ref = Get-ObjectPropertyValue -Object $Value -Name "traceRef"
    $Record.result_host_panel_ui_ref = Get-ObjectPropertyValue -Object $Value -Name "hostPanelUiRef"
    $Record.result_correlation_id = Get-ObjectPropertyValue -Object $Value -Name "correlationId"
    $Record.evidence_refs = Get-ObjectPropertyValue -Object $Value -Name "evidenceRefs"

    if ($Record.submit_button_click_invoked -eq $true -and
        $Record.submit_button_clicked_by_probe -eq $true -and
        $Record.input_event_dispatched -eq $true -and
        $Record.change_event_dispatched -eq $true -and
        $Record.readback_matches_probe -eq $true -and
        $Record.panel_submission_path_invoked -eq $true -and
        $Record.core_path_invoked -eq $true -and
        $Record.result_rendered_in_webview -eq $true -and
        $Record.submit_test_only_handler_used -ne $true) {
        $Record.status = "SUBMIT_RESULT_PASS"
    }
}

function Copy-Row12SubmitResultProofRecordToProbe {
    param(
        [System.Collections.IDictionary] $Probe,
        [System.Collections.IDictionary] $Record
    )

    $Probe.submit_result_proof = [pscustomobject] $Record
    $Probe.submit_result_proof_status = [string] $Record.status
    $Probe.submit_probe_text = $Record.submit_probe_text
    $Probe.submit_probe_text_sha256 = $Record.submit_probe_text_sha256
    $Probe.submit_probe_text_hash = $Record.submit_probe_text_hash
    $Probe.submit_probe_expression_sha256 = $Record.runtime_evaluate_expression_sha256
    $Probe.submit_js_wait_ms = $Record.submit_js_wait_ms
    $Probe.submit_cdp_timeout_sec = $Record.runtime_evaluate_timeout_sec
    $Probe.submit_proof_scope = $Record.submit_proof_scope
    $Probe.submit_retry_policy = $Record.submit_retry_policy
    $Probe.submit_context_source = $Record.submit_context_source
    $Probe.submit_target_id = $Record.submit_target_id
    $Probe.submit_target_source = $Record.submit_target_source
    $Probe.submit_session_id = $Record.submit_session_id
    $Probe.submit_frame_id = $Record.submit_frame_id
    $Probe.submit_execution_context_id = $Record.submit_execution_context_id
    $Probe.submit_selector_context_matches_visibility_context = $Record.submit_selector_context_matches_visibility_context
    $Probe.submit_button_click_invoked = $Record.submit_button_click_invoked
    $Probe.submit_button_clicked_by_probe = $Record.submit_button_clicked_by_probe
    $Probe.panel_submission_path_invoked = $Record.panel_submission_path_invoked
    $Probe.core_path_invoked = $Record.core_path_invoked
    $Probe.submit_test_only_handler_used = $Record.submit_test_only_handler_used
    $Probe.submit_not_test_only_path = $Record.submit_not_test_only_path
    $Probe.result_rendered_in_webview = $Record.result_rendered_in_webview
    $Probe.result_text = $Record.result_text
    $Probe.result_text_sha256 = $Record.result_text_sha256
    $Probe.result_status_text = $Record.result_status_text
    $Probe.result_response_ref = $Record.result_response_ref
    $Probe.result_trace_ref = $Record.result_trace_ref
    $Probe.result_host_panel_ui_ref = $Record.result_host_panel_ui_ref
    $Probe.result_correlation_id = $Record.result_correlation_id
    $Probe.submit_evidence_refs = $Record.evidence_refs
    $Probe.submit_no_readiness_claim = $Record.no_readiness_claim
    $Probe.submit_readiness_claimed = $Record.readiness_claimed
}

function Invoke-CdpTargetReadOnlyProbe {
    param(
        [object] $Target,
        [AllowNull()] [string] $BrowserWebSocketDebuggerUrl,
        [string] $Expression,
        [switch] $EnableTypeabilityProof,
        [AllowNull()] [string] $TypeabilityProbeText,
        [switch] $EnableSubmitResultProof,
        [AllowNull()] [string] $SubmitResultProbeText,
        [int] $SubmitResultWaitMs = 20000,
        [int] $SubmitResultCdpTimeoutSec = 30,
        [int] $TimeoutSec = 8
    )

    $targetId = Get-ObjectPropertyValue -Object $Target -Name "targetId"
    if ([string]::IsNullOrWhiteSpace([string] $targetId)) {
        $targetId = Get-ObjectPropertyValue -Object $Target -Name "id"
    }
    $targetSource = Get-ObjectPropertyValue -Object $Target -Name "source"
    $targetWebSocketDebuggerUrl = Get-ObjectPropertyValue -Object $Target -Name "webSocketDebuggerUrl"
    $events = New-Object System.Collections.Generic.List[object]
    $evaluations = New-Object System.Collections.Generic.List[object]
    $errors = New-Object System.Collections.Generic.List[object]
    $frameContextProbeErrors = New-Object System.Collections.Generic.List[object]
    $typeabilityProofErrors = New-Object System.Collections.Generic.List[object]
    $submitResultProofErrors = New-Object System.Collections.Generic.List[object]
    $frameContextProbeLimit = 12
    $submitResultJsWaitMs = [Math]::Max(20000, [int] $SubmitResultWaitMs)
    $submitResultCdpTimeoutSec = [Math]::Max([int] $SubmitResultCdpTimeoutSec, ([int] [Math]::Ceiling($submitResultJsWaitMs / 1000.0)) + 10)
    $probe = [ordered] @{
        attempted = $true
        read_only = -not $EnableSubmitResultProof.IsPresent
        typeability_proof_requested = $EnableTypeabilityProof.IsPresent
        typeability_proof_attempted = $false
        typeability_proof_status = if ($EnableTypeabilityProof.IsPresent) { "NOT_REACHED" } else { "NOT_REQUESTED" }
        typeability_proof = $null
        typeability_proof_errors = @()
        typeability_probe_attempted = $false
        typeability_probe_text_hash = if ($EnableTypeabilityProof.IsPresent) { Get-Sha256Hex -Value $TypeabilityProbeText } else { $null }
        typeability_probe_expression_sha256 = $null
        typeability_context_source = $null
        typeability_target_id = $targetId
        typeability_target_source = $targetSource
        typeability_session_id = $null
        typeability_frame_id = $null
        typeability_execution_context_id = $null
        typeability_selector_context_matches_visibility_context = $false
        typeability_readback_matches = $false
        typeability_no_submit_guard = $false
        typeability_core_invoked = $false
        typeability_readback_value = $null
        typeability_readback_sha256 = $null
        typeability_readback_matches_probe = $false
        typeability_no_submit_performed_by_probe = $false
        typeability_submit_button_click_invoked = $false
        typeability_submit_button_clicked_by_probe = $false
        typeability_core_path_invoked_by_probe = $false
        typeability_panel_submission_path_invoked_by_probe = $false
        submit_result_proof_requested = $EnableSubmitResultProof.IsPresent
        submit_result_proof_attempted = $false
        submit_result_proof_status = if ($EnableSubmitResultProof.IsPresent) { "NOT_REACHED" } else { "NOT_REQUESTED" }
        submit_result_proof = $null
        submit_result_proof_errors = @()
        submit_probe_attempted = $false
        submit_probe_text = if ($EnableSubmitResultProof.IsPresent) { $SubmitResultProbeText } else { $null }
        submit_probe_text_sha256 = if ($EnableSubmitResultProof.IsPresent) { Get-Sha256Hex -Value $SubmitResultProbeText } else { $null }
        submit_probe_text_hash = if ($EnableSubmitResultProof.IsPresent) { Get-Sha256Hex -Value $SubmitResultProbeText } else { $null }
        submit_probe_expression_sha256 = $null
        submit_js_wait_ms = if ($EnableSubmitResultProof.IsPresent) { $submitResultJsWaitMs } else { $null }
        submit_cdp_timeout_sec = if ($EnableSubmitResultProof.IsPresent) { $submitResultCdpTimeoutSec } else { $null }
        submit_proof_scope = if ($EnableSubmitResultProof.IsPresent) { "first_attached_runtime_visible_context_path" } else { $null }
        submit_retry_policy = if ($EnableSubmitResultProof.IsPresent) { "Prefer the first attached-runtime visible context path; frame/evaluated submit proof is allowed only if no attached-runtime submit path was attempted." } else { $null }
        submit_first_attached_runtime_visible_context_attempted = $false
        submit_frame_or_evaluated_retry_skipped = $false
        submit_context_source = $null
        submit_target_id = $targetId
        submit_target_source = $targetSource
        submit_session_id = $null
        submit_frame_id = $null
        submit_execution_context_id = $null
        submit_selector_context_matches_visibility_context = $false
        submit_button_click_invoked = $false
        submit_button_clicked_by_probe = $false
        panel_submission_path_invoked = $false
        core_path_invoked = $false
        submit_test_only_handler_used = $false
        submit_not_test_only_path = $false
        result_rendered_in_webview = $false
        result_text = $null
        result_text_sha256 = $null
        result_status_text = $null
        result_response_ref = $null
        result_trace_ref = $null
        result_host_panel_ui_ref = $null
        result_correlation_id = $null
        submit_evidence_refs = $null
        submit_no_readiness_claim = if ($EnableSubmitResultProof.IsPresent) { $true } else { $null }
        submit_readiness_claimed = $false
        target_id = $targetId
        attach_method = $null
        session_id = $null
        attached_session_id_present = $false
        attached_session_command_path = $null
        attached_runtime_evaluate_attempted = $false
        attached_runtime_evaluate_available = $false
        attached_runtime_evaluate_error = $null
        attached_runtime_evaluate_response = $null
        attached_runtime_evaluate_value = $null
        attached_runtime_selectors_found = [ordered] @{
            "#xiigen-smart-skills-prompt" = $false
            "#xiigen-smart-skills-submit" = $false
        }
        attached_runtime_texts_found = [ordered] @{
            "XIIGen Smart Skills" = $false
            "XIIGen task prompt" = $false
        }
        attached_runtime_all_required_selectors_found = $false
        attached_runtime_all_required_text_found = $false
        attached_runtime_all_required_dom_visible = $false
        attached_dom_snapshot_attempted = $false
        attached_dom_snapshot_available = $false
        attached_dom_snapshot_error = $null
        attached_dom_snapshot_response = $null
        page_enable_response = $null
        runtime_enable_response = $null
        frame_tree_response = $null
        frame_tree = $null
        dom_snapshot_response = $null
        dom_snapshot_available = $false
        frame_context_probe_attempted = $false
        frame_context_probe_method = "Page.createIsolatedWorld_per_frame_Runtime.evaluate_read_only"
        frame_context_probe_limit = $frameContextProbeLimit
        frame_context_probe_truncated = $false
        frame_context_probe_frame_count = 0
        frame_context_required_dom_visible = $false
        frame_context_probes = @()
        frame_context_probe_errors = @()
        execution_context_events = @()
        execution_contexts = @()
        evaluations = @()
        errors = @()
    }

    $socket = [System.Net.WebSockets.ClientWebSocket]::new()
    $sessionId = $null
    $nextCommandId = 1
    $attachedRuntimeExecutionContextId = $null
    try {
        $connectUrl = $targetWebSocketDebuggerUrl
        if ([string]::IsNullOrWhiteSpace([string] $connectUrl)) {
            if ([string]::IsNullOrWhiteSpace([string] $BrowserWebSocketDebuggerUrl)) {
                throw "Candidate target has no webSocketDebuggerUrl and no browser websocket fallback URL is available."
            }
            if ([string]::IsNullOrWhiteSpace([string] $targetId)) {
                throw "Candidate browser-level target has no targetId."
            }
            $connectUrl = $BrowserWebSocketDebuggerUrl
            $probe.attach_method = "browser_webSocketDebuggerUrl_Target.attachToTarget_flatten_true_then_read_only_probe"
        } else {
            $probe.attach_method = "target_webSocketDebuggerUrl_read_only_probe"
        }

        $cancel = New-Object System.Threading.CancellationTokenSource
        try {
            $cancel.CancelAfter([TimeSpan]::FromSeconds($TimeoutSec))
            [void] $socket.ConnectAsync([Uri] $connectUrl, $cancel.Token).GetAwaiter().GetResult()
        } finally {
            $cancel.Dispose()
        }

        if ($probe.attach_method -like "browser_webSocketDebuggerUrl_*") {
            $attachResponse = Invoke-CdpWebSocketCommand -Socket $socket -Id $nextCommandId -Method "Target.attachToTarget" -Params @{
                targetId = $targetId
                flatten = $true
            } -TimeoutSec $TimeoutSec -EventSink $events
            $nextCommandId += 1
            $attachError = Get-ObjectPropertyValue -Object $attachResponse -Name "error"
            if ($attachError) {
                throw "Target.attachToTarget returned error: $(($attachError | ConvertTo-Json -Depth 8 -Compress))"
            }
            $attachResult = Get-ObjectPropertyValue -Object $attachResponse -Name "result"
            $sessionId = Get-ObjectPropertyValue -Object $attachResult -Name "sessionId"
            if ([string]::IsNullOrWhiteSpace([string] $sessionId)) {
                throw "Target.attachToTarget did not return a sessionId for target $targetId."
            }
            $probe.session_id = $sessionId
            $probe.attached_session_id_present = $true
            $probe.attached_session_command_path = "browser_websocket_Target.attachToTarget_flatten_true_sessionId_envelope"

            $probe.attached_runtime_evaluate_attempted = $true
            try {
                $immediateEvalResponse = Invoke-CdpWebSocketCommand -Socket $socket -Id $nextCommandId -Method "Runtime.evaluate" -Params @{
                    expression = $Expression
                    returnByValue = $true
                    awaitPromise = $true
                    userGesture = $false
                } -SessionId ([string] $sessionId) -TimeoutSec $TimeoutSec -EventSink $events
                $nextCommandId += 1
                $probe.attached_runtime_evaluate_response = $immediateEvalResponse
                $immediateEvalError = Get-ObjectPropertyValue -Object $immediateEvalResponse -Name "error"
                if ($immediateEvalError) {
                    throw "Runtime.evaluate returned error: $(($immediateEvalError | ConvertTo-Json -Depth 8 -Compress))"
                }

                $immediateEvalResult = Get-ObjectPropertyValue -Object $immediateEvalResponse -Name "result"
                $immediateExceptionDetails = Get-ObjectPropertyValue -Object $immediateEvalResult -Name "exceptionDetails"
                if ($immediateExceptionDetails) {
                    throw "Runtime.evaluate returned exceptionDetails: $(($immediateExceptionDetails | ConvertTo-Json -Depth 8 -Compress))"
                }

                $immediateRuntimeResult = Get-ObjectPropertyValue -Object $immediateEvalResult -Name "result"
                $immediateValue = Get-ObjectPropertyValue -Object $immediateRuntimeResult -Name "value"
                $probe.attached_runtime_evaluate_value = $immediateValue
                if ($null -ne $immediateValue) {
                    $probe.attached_runtime_evaluate_available = $true
                    $immediateDocuments = @(ConvertTo-NonNullArray -Value (Get-ObjectPropertyValue -Object $immediateValue -Name "documents"))
                    if ($immediateDocuments.Count -eq 0) {
                        $immediateDocuments = @($immediateValue)
                    }

                    foreach ($documentSample in $immediateDocuments) {
                        $documentSelectorResults = Get-ObjectPropertyValue -Object $documentSample -Name "selectorResults"
                        foreach ($selector in @("#xiigen-smart-skills-prompt", "#xiigen-smart-skills-submit")) {
                            $selectorResult = Get-ObjectPropertyValue -Object $documentSelectorResults -Name $selector
                            if ($selectorResult -and (Get-ObjectPropertyValue -Object $selectorResult -Name "found") -eq $true) {
                                $probe.attached_runtime_selectors_found[$selector] = $true
                            }
                        }

                        $documentTextResults = Get-ObjectPropertyValue -Object $documentSample -Name "textResults"
                        foreach ($textName in @("XIIGen Smart Skills", "XIIGen task prompt")) {
                            if ($documentTextResults -and (Get-ObjectPropertyValue -Object $documentTextResults -Name $textName) -eq $true) {
                                $probe.attached_runtime_texts_found[$textName] = $true
                            }
                        }

                        if ((Get-ObjectPropertyValue -Object $documentSample -Name "allRequiredDomVisible") -eq $true) {
                            $probe.attached_runtime_all_required_dom_visible = $true
                        }
                    }

                    $probe.attached_runtime_all_required_selectors_found =
                        $probe.attached_runtime_selectors_found["#xiigen-smart-skills-prompt"] -eq $true -and
                        $probe.attached_runtime_selectors_found["#xiigen-smart-skills-submit"] -eq $true
                    $probe.attached_runtime_all_required_text_found =
                        $probe.attached_runtime_texts_found["XIIGen Smart Skills"] -eq $true -and
                        $probe.attached_runtime_texts_found["XIIGen task prompt"] -eq $true
                    $probe.attached_runtime_all_required_dom_visible =
                        $probe.attached_runtime_all_required_dom_visible -eq $true -or
                        ($probe.attached_runtime_all_required_selectors_found -eq $true -and $probe.attached_runtime_all_required_text_found -eq $true)

                    if ($EnableTypeabilityProof.IsPresent -and
                        $probe.attached_runtime_all_required_dom_visible -eq $true -and
                        $probe.typeability_proof_status -ne "TYPEABILITY_PASS") {
                        $probe.typeability_proof_attempted = $true
                        $probe.typeability_probe_attempted = $true
                        $typeabilityExpression = New-Row12TypeabilityProbeExpression -ProbeText $TypeabilityProbeText
                        $typeabilityExpressionHash = Get-Sha256Hex -Value $typeabilityExpression
                        $probe.typeability_probe_expression_sha256 = $typeabilityExpressionHash
                        $typeabilityParams = @{
                            expression = $typeabilityExpression
                            returnByValue = $true
                            awaitPromise = $true
                            userGesture = $false
                        }
                        $typeabilityRecord = [ordered] @{
                            schema = "xiigen.row12.attached_session_typeability_result.v1"
                            attempted = $true
                            status = "TYPEABILITY_NEEDS_FIX"
                            method = "attached_session_Runtime.evaluate_set_textarea_dispatch_input_change_readback"
                            target_id = $targetId
                            typeability_context_source = "attached_runtime_visibility_context"
                            typeability_target_id = $targetId
                            typeability_target_source = $targetSource
                            typeability_session_id = $sessionId
                            typeability_frame_id = $null
                            typeability_execution_context_id = $attachedRuntimeExecutionContextId
                            typeability_selector_context_matches_visibility_context = $true
                            command_path = $probe.attached_session_command_path
                            attached_runtime_all_required_dom_visible_before_probe = $probe.attached_runtime_all_required_dom_visible
                            selector = "#xiigen-smart-skills-prompt"
                            probe_text = $TypeabilityProbeText
                            probe_text_sha256 = Get-Sha256Hex -Value $TypeabilityProbeText
                            typeability_probe_attempted = $true
                            typeability_probe_text_hash = Get-Sha256Hex -Value $TypeabilityProbeText
                            runtime_evaluate_expression_sha256 = $typeabilityExpressionHash
                            runtime_evaluate_response = $null
                            runtime_exception = $null
                            error = $null
                            raw_result = $null
                            readback_value = $null
                            readback_sha256 = $null
                            readback_matches_probe = $false
                            typeability_readback_matches = $false
                            input_event_dispatched = $false
                            change_event_dispatched = $false
                            submit_button_found = $false
                            submit_button_disabled = $null
                            submit_button_click_invoked = $false
                            submit_button_clicked_by_probe = $false
                            no_submit_performed_by_probe = $false
                            core_path_invoked_by_probe = $false
                            panel_submission_path_invoked_by_probe = $false
                            typeability_no_submit_guard = $false
                            typeability_core_invoked = $false
                        }

                        try {
                            $typeabilityResponse = Invoke-CdpWebSocketCommand -Socket $socket -Id $nextCommandId -Method "Runtime.evaluate" -Params $typeabilityParams -SessionId ([string] $sessionId) -TimeoutSec $TimeoutSec -EventSink $events
                            $nextCommandId += 1
                            $typeabilityRecord.runtime_evaluate_response = $typeabilityResponse
                            $typeabilityResponseError = Get-ObjectPropertyValue -Object $typeabilityResponse -Name "error"
                            if ($typeabilityResponseError) {
                                throw "Runtime.evaluate typeability probe returned error: $(($typeabilityResponseError | ConvertTo-Json -Depth 8 -Compress))"
                            }

                            $typeabilityEvalResult = Get-ObjectPropertyValue -Object $typeabilityResponse -Name "result"
                            $typeabilityExceptionDetails = Get-ObjectPropertyValue -Object $typeabilityEvalResult -Name "exceptionDetails"
                            if ($typeabilityExceptionDetails) {
                                $typeabilityRecord.runtime_exception = $typeabilityExceptionDetails
                                throw "Runtime.evaluate typeability probe returned exceptionDetails."
                            }

                            $typeabilityRuntimeResult = Get-ObjectPropertyValue -Object $typeabilityEvalResult -Name "result"
                            $typeabilityValue = Get-ObjectPropertyValue -Object $typeabilityRuntimeResult -Name "value"
                            $typeabilityRecord.raw_result = $typeabilityValue
                            $selectedDocumentFrameMeta = Get-ObjectPropertyValue -Object $typeabilityValue -Name "selectedDocumentFrameMeta"
                            if ($selectedDocumentFrameMeta) {
                                $typeabilityRecord.typeability_frame_id = Get-ObjectPropertyValue -Object $selectedDocumentFrameMeta -Name "id"
                            }
                            $readbackValue = Get-ObjectPropertyValue -Object $typeabilityValue -Name "afterValue"
                            $typeabilityRecord.readback_value = $readbackValue
                            if ($null -ne $readbackValue) {
                                $typeabilityRecord.readback_sha256 = Get-Sha256Hex -Value ([string] $readbackValue)
                            }
                            $typeabilityRecord.readback_matches_probe = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "readbackMatchesProbe") -eq $true
                            $typeabilityRecord.typeability_readback_matches = $typeabilityRecord.readback_matches_probe
                            $typeabilityInputEvent = Get-ObjectPropertyValue -Object $typeabilityValue -Name "inputEvent"
                            $typeabilityChangeEvent = Get-ObjectPropertyValue -Object $typeabilityValue -Name "changeEvent"
                            $typeabilityRecord.input_event_dispatched = (Get-ObjectPropertyValue -Object $typeabilityInputEvent -Name "dispatched") -eq $true
                            $typeabilityRecord.change_event_dispatched = (Get-ObjectPropertyValue -Object $typeabilityChangeEvent -Name "dispatched") -eq $true
                            $typeabilityRecord.submit_button_found = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "submitButtonFound") -eq $true
                            $typeabilityRecord.submit_button_disabled = Get-ObjectPropertyValue -Object $typeabilityValue -Name "submitButtonDisabled"
                            $typeabilityRecord.submit_button_click_invoked = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "submitButtonClickInvoked") -eq $true
                            $typeabilityRecord.submit_button_clicked_by_probe = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "submitButtonClickedByProbe") -eq $true
                            $typeabilityRecord.no_submit_performed_by_probe = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "noSubmitPerformedByProbe") -eq $true
                            $typeabilityRecord.core_path_invoked_by_probe = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "corePathInvokedByProbe") -eq $true
                            $typeabilityRecord.panel_submission_path_invoked_by_probe = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "panelSubmissionPathInvokedByProbe") -eq $true
                            $typeabilityRecord.typeability_core_invoked = $typeabilityRecord.core_path_invoked_by_probe
                            $typeabilityRecord.typeability_no_submit_guard =
                                $typeabilityRecord.submit_button_click_invoked -ne $true -and
                                $typeabilityRecord.submit_button_clicked_by_probe -ne $true -and
                                $typeabilityRecord.core_path_invoked_by_probe -ne $true -and
                                $typeabilityRecord.panel_submission_path_invoked_by_probe -ne $true

                            if ($typeabilityRecord.readback_matches_probe -eq $true -and
                                $typeabilityRecord.input_event_dispatched -eq $true -and
                                $typeabilityRecord.change_event_dispatched -eq $true -and
                                $typeabilityRecord.typeability_no_submit_guard -eq $true) {
                                $typeabilityRecord.status = "TYPEABILITY_PASS"
                            }
                        } catch {
                            $typeabilityRecord.error = $_.Exception.Message
                            $errorRecord = [pscustomobject] [ordered] @{
                                stage = "attached_session_typeability_probe"
                                method = "Runtime.evaluate_set_textarea_dispatch_input_change_readback"
                                command_path = $probe.attached_session_command_path
                                message = $_.Exception.Message
                            }
                            $typeabilityProofErrors.Add($errorRecord) | Out-Null
                            $errors.Add($errorRecord) | Out-Null
                        }

                        $probe.typeability_proof = [pscustomobject] $typeabilityRecord
                        $probe.typeability_proof_status = [string] $typeabilityRecord.status
                        $probe.typeability_context_source = $typeabilityRecord.typeability_context_source
                        $probe.typeability_target_id = $typeabilityRecord.typeability_target_id
                        $probe.typeability_target_source = $typeabilityRecord.typeability_target_source
                        $probe.typeability_session_id = $typeabilityRecord.typeability_session_id
                        $probe.typeability_frame_id = $typeabilityRecord.typeability_frame_id
                        $probe.typeability_execution_context_id = $typeabilityRecord.typeability_execution_context_id
                        $probe.typeability_selector_context_matches_visibility_context = $typeabilityRecord.typeability_selector_context_matches_visibility_context
                        $probe.typeability_readback_value = $typeabilityRecord.readback_value
                        $probe.typeability_readback_sha256 = $typeabilityRecord.readback_sha256
                        $probe.typeability_readback_matches_probe = $typeabilityRecord.readback_matches_probe
                        $probe.typeability_readback_matches = $typeabilityRecord.typeability_readback_matches
                        $probe.typeability_no_submit_performed_by_probe = $typeabilityRecord.no_submit_performed_by_probe
                        $probe.typeability_submit_button_click_invoked = $typeabilityRecord.submit_button_click_invoked
                        $probe.typeability_submit_button_clicked_by_probe = $typeabilityRecord.submit_button_clicked_by_probe
                        $probe.typeability_core_path_invoked_by_probe = $typeabilityRecord.core_path_invoked_by_probe
                        $probe.typeability_panel_submission_path_invoked_by_probe = $typeabilityRecord.panel_submission_path_invoked_by_probe
                        $probe.typeability_no_submit_guard = $typeabilityRecord.typeability_no_submit_guard
                        $probe.typeability_core_invoked = $typeabilityRecord.typeability_core_invoked
                    }

                    if ($EnableSubmitResultProof.IsPresent -and
                        $probe.attached_runtime_all_required_dom_visible -eq $true -and
                        $probe.submit_result_proof_status -ne "SUBMIT_RESULT_PASS") {
                        $probe.submit_result_proof_attempted = $true
                        $probe.submit_probe_attempted = $true
                        $probe.submit_first_attached_runtime_visible_context_attempted = $true
                        $submitResultExpression = New-Row12SubmitResultProbeExpression -ProbeText $SubmitResultProbeText -SubmitWaitMs $submitResultJsWaitMs
                        $submitResultExpressionHash = Get-Sha256Hex -Value $submitResultExpression
                        $probe.submit_probe_expression_sha256 = $submitResultExpressionHash
                        $submitResultParams = @{
                            expression = $submitResultExpression
                            returnByValue = $true
                            awaitPromise = $true
                            userGesture = $true
                        }
                        $submitResultRecord = New-Row12SubmitResultProofRecord `
                            -Schema "xiigen.row12.attached_session_submit_result.v1" `
                            -TargetId $targetId `
                            -TargetSource $targetSource `
                            -SessionId $sessionId `
                            -FrameId $null `
                            -ExecutionContextId $attachedRuntimeExecutionContextId `
                            -ContextSource "attached_runtime_visibility_context" `
                            -SelectorContextMatchesVisibilityContext $true `
                            -CommandPath $probe.attached_session_command_path `
                            -EvaluationLabel $null `
                            -FrameIndex $null `
                            -ProbeText $SubmitResultProbeText `
                            -ExpressionHash $submitResultExpressionHash `
                            -SubmitWaitMs $submitResultJsWaitMs `
                            -RuntimeEvaluateTimeoutSec $submitResultCdpTimeoutSec

                        try {
                            $submitResultResponse = Invoke-CdpWebSocketCommand -Socket $socket -Id $nextCommandId -Method "Runtime.evaluate" -Params $submitResultParams -SessionId ([string] $sessionId) -TimeoutSec $submitResultCdpTimeoutSec -EventSink $events
                            $nextCommandId += 1
                            $submitResultRecord.runtime_evaluate_response = $submitResultResponse
                            $submitResultResponseError = Get-ObjectPropertyValue -Object $submitResultResponse -Name "error"
                            if ($submitResultResponseError) {
                                throw "Runtime.evaluate submit/result proof returned error: $(($submitResultResponseError | ConvertTo-Json -Depth 8 -Compress))"
                            }

                            $submitResultEvalResult = Get-ObjectPropertyValue -Object $submitResultResponse -Name "result"
                            $submitResultExceptionDetails = Get-ObjectPropertyValue -Object $submitResultEvalResult -Name "exceptionDetails"
                            if ($submitResultExceptionDetails) {
                                $submitResultRecord.runtime_exception = $submitResultExceptionDetails
                                throw "Runtime.evaluate submit/result proof returned exceptionDetails."
                            }

                            $submitResultRuntimeResult = Get-ObjectPropertyValue -Object $submitResultEvalResult -Name "result"
                            $submitResultValue = Get-ObjectPropertyValue -Object $submitResultRuntimeResult -Name "value"
                            Update-Row12SubmitResultProofRecord -Record $submitResultRecord -Value $submitResultValue
                        } catch {
                            $submitResultRecord.error = $_.Exception.Message
                            $errorRecord = [pscustomobject] [ordered] @{
                                stage = "attached_session_submit_result_probe"
                                method = "Runtime.evaluate_set_prompt_dispatch_input_change_click_visible_submit_wait_result"
                                command_path = $probe.attached_session_command_path
                                message = $_.Exception.Message
                            }
                            $submitResultProofErrors.Add($errorRecord) | Out-Null
                            $errors.Add($errorRecord) | Out-Null
                        }

                        Copy-Row12SubmitResultProofRecordToProbe -Probe $probe -Record $submitResultRecord
                        $probe.submit_frame_or_evaluated_retry_skipped = $true
                    }
                }
            } catch {
                $probe.attached_runtime_evaluate_error = $_.Exception.Message
                $errors.Add([pscustomobject] [ordered] @{
                    stage = "attached_session_Runtime.evaluate"
                    command_path = $probe.attached_session_command_path
                    message = $_.Exception.Message
                }) | Out-Null
            }

            $probe.attached_dom_snapshot_attempted = $true
            try {
                $attachedDomSnapshotResponse = Invoke-CdpWebSocketCommand -Socket $socket -Id $nextCommandId -Method "DOMSnapshot.captureSnapshot" -Params @{
                    computedStyles = @()
                    includeDOMRects = $true
                    includePaintOrder = $false
                } -SessionId ([string] $sessionId) -TimeoutSec $TimeoutSec -EventSink $events
                $nextCommandId += 1
                $probe.attached_dom_snapshot_response = $attachedDomSnapshotResponse
                $attachedDomSnapshotError = Get-ObjectPropertyValue -Object $attachedDomSnapshotResponse -Name "error"
                if ($attachedDomSnapshotError) {
                    throw "DOMSnapshot.captureSnapshot returned error: $(($attachedDomSnapshotError | ConvertTo-Json -Depth 8 -Compress))"
                }
                $probe.attached_dom_snapshot_available = $null -ne (Get-ObjectPropertyValue -Object $attachedDomSnapshotResponse -Name "result")
            } catch {
                $probe.attached_dom_snapshot_error = $_.Exception.Message
                $errors.Add([pscustomobject] [ordered] @{
                    stage = "attached_session_DOMSnapshot.captureSnapshot"
                    command_path = $probe.attached_session_command_path
                    message = $_.Exception.Message
                }) | Out-Null
            }
        } elseif ($probe.attach_method -eq "target_webSocketDebuggerUrl_read_only_probe") {
            $probe.attached_session_command_path = "target_webSocketDebuggerUrl_no_flatten_session"
        }

        foreach ($domainCommand in @(
            @{ method = "Page.enable"; key = "page_enable_response"; params = @{} },
            @{ method = "Runtime.enable"; key = "runtime_enable_response"; params = @{} }
        )) {
            try {
                $domainResponse = Invoke-CdpWebSocketCommand -Socket $socket -Id $nextCommandId -Method $domainCommand.method -Params $domainCommand.params -SessionId $sessionId -TimeoutSec $TimeoutSec -EventSink $events
                $nextCommandId += 1
                $probe[$domainCommand.key] = $domainResponse
            } catch {
                $errors.Add([pscustomobject] @{
                    stage = $domainCommand.method
                    message = $_.Exception.Message
                }) | Out-Null
            }
        }

        Read-CdpWebSocketEvents -Socket $socket -EventSink $events -MaxMessages 30 -TimeoutSec 1

        try {
            $frameTreeResponse = Invoke-CdpWebSocketCommand -Socket $socket -Id $nextCommandId -Method "Page.getFrameTree" -Params @{} -SessionId $sessionId -TimeoutSec $TimeoutSec -EventSink $events
            $nextCommandId += 1
            $probe.frame_tree_response = $frameTreeResponse
            $frameTreeResult = Get-ObjectPropertyValue -Object $frameTreeResponse -Name "result"
            $probe.frame_tree = Get-ObjectPropertyValue -Object $frameTreeResult -Name "frameTree"
        } catch {
            $errors.Add([pscustomobject] @{
                stage = "Page.getFrameTree"
                message = $_.Exception.Message
            }) | Out-Null
        }

        $probe.frame_context_probe_attempted = $true
        $flatFrames = @()
        if ($probe.frame_tree) {
            $frameQueue = [System.Collections.Generic.Queue[object]]::new()
            $frameQueue.Enqueue($probe.frame_tree)
            while ($frameQueue.Count -gt 0) {
                $frameTreeNode = $frameQueue.Dequeue()
                $frame = Get-ObjectPropertyValue -Object $frameTreeNode -Name "frame"
                $frameId = Get-ObjectPropertyValue -Object $frame -Name "id"
                if (-not [string]::IsNullOrWhiteSpace([string] $frameId)) {
                    $flatFrames += [pscustomobject] [ordered] @{
                        index = $flatFrames.Count
                        frame_id = $frameId
                        parent_frame_id = Get-ObjectPropertyValue -Object $frame -Name "parentId"
                        name = Get-ObjectPropertyValue -Object $frame -Name "name"
                        url = Get-ObjectPropertyValue -Object $frame -Name "url"
                        mime_type = Get-ObjectPropertyValue -Object $frame -Name "mimeType"
                        security_origin = Get-ObjectPropertyValue -Object $frame -Name "securityOrigin"
                    }
                }

                $childFrames = @(ConvertTo-NonNullArray -Value (Get-ObjectPropertyValue -Object $frameTreeNode -Name "childFrames"))
                foreach ($childFrame in $childFrames) {
                    $frameQueue.Enqueue($childFrame)
                }
            }
        }

        $probe.frame_context_probe_frame_count = $flatFrames.Count
        $probe.frame_context_probe_truncated = $flatFrames.Count -gt $frameContextProbeLimit
        if ($flatFrames.Count -eq 0) {
            $errorRecord = [pscustomobject] [ordered] @{
                stage = "frame_context_probe"
                method = $probe.frame_context_probe_method
                message = "No frames were available from Page.getFrameTree for per-frame isolated-world probing."
            }
            $frameContextProbeErrors.Add($errorRecord) | Out-Null
            $errors.Add($errorRecord) | Out-Null
        }

        $frameContextProbes = New-Object System.Collections.Generic.List[object]
        foreach ($frameInfo in @($flatFrames | Select-Object -First $frameContextProbeLimit)) {
            $frameId = [string] (Get-ObjectPropertyValue -Object $frameInfo -Name "frame_id")
            $frameIndex = Get-ObjectPropertyValue -Object $frameInfo -Name "index"
            $frameProbe = [ordered] @{
                frame_index = $frameIndex
                frame_id = $frameId
                parent_frame_id = Get-ObjectPropertyValue -Object $frameInfo -Name "parent_frame_id"
                name = Get-ObjectPropertyValue -Object $frameInfo -Name "name"
                url = Get-ObjectPropertyValue -Object $frameInfo -Name "url"
                mime_type = Get-ObjectPropertyValue -Object $frameInfo -Name "mime_type"
                security_origin = Get-ObjectPropertyValue -Object $frameInfo -Name "security_origin"
                attempted = $true
                read_only = $true
                method = $probe.frame_context_probe_method
                create_isolated_world_response = $null
                runtime_evaluate_response = $null
                execution_context_id = $null
                evaluated = $false
                status = "NEEDS_FIX"
                runtime_exception = $null
                error = $null
                selector_results = $null
                text_results = $null
                marker_results = $null
                selectors_found = [ordered] @{
                    "#xiigen-smart-skills-prompt" = $false
                    "#xiigen-smart-skills-submit" = $false
                }
                texts_found = [ordered] @{
                    "XIIGen Smart Skills" = $false
                    "XIIGen task prompt" = $false
                }
                controls_summary = @()
                documents_summary = @()
                all_required_selectors_found = $false
                all_required_text_found = $false
                all_required_dom_visible = $false
                typeability_proof_attempted = $false
                typeability_proof = $null
            }

            try {
                $createWorldResponse = Invoke-CdpWebSocketCommand -Socket $socket -Id $nextCommandId -Method "Page.createIsolatedWorld" -Params @{
                    frameId = $frameId
                    worldName = "row12-read-only-frame-context-probe-$frameIndex"
                } -SessionId $sessionId -TimeoutSec $TimeoutSec -EventSink $events
                $nextCommandId += 1
                $frameProbe.create_isolated_world_response = $createWorldResponse
                $createWorldError = Get-ObjectPropertyValue -Object $createWorldResponse -Name "error"
                if ($createWorldError) {
                    throw "Page.createIsolatedWorld returned error: $(($createWorldError | ConvertTo-Json -Depth 8 -Compress))"
                }

                $createWorldResult = Get-ObjectPropertyValue -Object $createWorldResponse -Name "result"
                $executionContextId = Get-ObjectPropertyValue -Object $createWorldResult -Name "executionContextId"
                if ($null -eq $executionContextId -or [string]::IsNullOrWhiteSpace([string] $executionContextId)) {
                    throw "Page.createIsolatedWorld did not return executionContextId for frame $frameId."
                }
                $frameProbe.execution_context_id = $executionContextId

                $frameEvalParams = @{
                    expression = $Expression
                    returnByValue = $true
                    awaitPromise = $true
                    userGesture = $false
                }
                $frameEvalParams.contextId = [int] $executionContextId

                $evalResponse = Invoke-CdpWebSocketCommand -Socket $socket -Id $nextCommandId -Method "Runtime.evaluate" -Params $frameEvalParams -SessionId $sessionId -TimeoutSec $TimeoutSec -EventSink $events
                $nextCommandId += 1
                $frameProbe.runtime_evaluate_response = $evalResponse
                $evalError = Get-ObjectPropertyValue -Object $evalResponse -Name "error"
                if ($evalError) {
                    throw "Runtime.evaluate returned error: $(($evalError | ConvertTo-Json -Depth 8 -Compress))"
                }

                $evalResult = Get-ObjectPropertyValue -Object $evalResponse -Name "result"
                $exceptionDetails = Get-ObjectPropertyValue -Object $evalResult -Name "exceptionDetails"
                if ($exceptionDetails) {
                    $frameProbe.runtime_exception = $exceptionDetails
                    throw "Runtime.evaluate returned exceptionDetails."
                }

                $runtimeResult = Get-ObjectPropertyValue -Object $evalResult -Name "result"
                $value = Get-ObjectPropertyValue -Object $runtimeResult -Name "value"
                $frameProbe.evaluated = $true
                $frameProbe.status = "EVALUATED"
                $frameProbe.selector_results = Get-ObjectPropertyValue -Object $value -Name "selectorResults"
                $frameProbe.text_results = Get-ObjectPropertyValue -Object $value -Name "textResults"
                $frameProbe.marker_results = Get-ObjectPropertyValue -Object $value -Name "markerResults"

                $documents = @(ConvertTo-NonNullArray -Value (Get-ObjectPropertyValue -Object $value -Name "documents"))
                if ($documents.Count -eq 0 -and $null -ne $value) {
                    $documents = @($value)
                }

                foreach ($documentSample in $documents) {
                    $documentLabel = Get-ObjectPropertyValue -Object $documentSample -Name "label"
                    $documentSelectorResults = Get-ObjectPropertyValue -Object $documentSample -Name "selectorResults"
                    $documentTextResults = Get-ObjectPropertyValue -Object $documentSample -Name "textResults"
                    $documentSelectorsFound = [ordered] @{
                        "#xiigen-smart-skills-prompt" = $false
                        "#xiigen-smart-skills-submit" = $false
                    }
                    $documentTextsFound = [ordered] @{
                        "XIIGen Smart Skills" = $false
                        "XIIGen task prompt" = $false
                    }

                    foreach ($selector in @("#xiigen-smart-skills-prompt", "#xiigen-smart-skills-submit")) {
                        $selectorResult = Get-ObjectPropertyValue -Object $documentSelectorResults -Name $selector
                        $selectorFound = $false
                        if ($selectorResult) {
                            $selectorFound = (Get-ObjectPropertyValue -Object $selectorResult -Name "found") -eq $true
                        }
                        if ($selectorFound) {
                            $documentSelectorsFound[$selector] = $true
                            $frameProbe.selectors_found[$selector] = $true
                        }
                    }

                    foreach ($textName in @("XIIGen Smart Skills", "XIIGen task prompt")) {
                        $textFound = $false
                        if ($documentTextResults) {
                            $textFound = (Get-ObjectPropertyValue -Object $documentTextResults -Name $textName) -eq $true
                        }
                        if ($textFound) {
                            $documentTextsFound[$textName] = $true
                            $frameProbe.texts_found[$textName] = $true
                        }
                    }

                    $documentAllRequiredSelectorsFound =
                        $documentSelectorsFound["#xiigen-smart-skills-prompt"] -eq $true -and
                        $documentSelectorsFound["#xiigen-smart-skills-submit"] -eq $true
                    $documentAllRequiredTextFound =
                        $documentTextsFound["XIIGen Smart Skills"] -eq $true -and
                        $documentTextsFound["XIIGen task prompt"] -eq $true
                    $documentAllRequiredDomVisible = (Get-ObjectPropertyValue -Object $documentSample -Name "allRequiredDomVisible") -eq $true
                    if ($documentAllRequiredDomVisible) {
                        $frameProbe.all_required_dom_visible = $true
                    }

                    $remainingControlCount = 50 - @($frameProbe.controls_summary).Count
                    if ($remainingControlCount -gt 0) {
                        $controls = @(ConvertTo-NonNullArray -Value (Get-ObjectPropertyValue -Object $documentSample -Name "controlsSummary"))
                        foreach ($control in @($controls | Select-Object -First $remainingControlCount)) {
                            $frameProbe.controls_summary += [pscustomobject] [ordered] @{
                                document_label = $documentLabel
                                tagName = Get-ObjectPropertyValue -Object $control -Name "tagName"
                                id = Get-ObjectPropertyValue -Object $control -Name "id"
                                type = Get-ObjectPropertyValue -Object $control -Name "type"
                                role = Get-ObjectPropertyValue -Object $control -Name "role"
                                ariaLabel = Get-ObjectPropertyValue -Object $control -Name "ariaLabel"
                                placeholder = Get-ObjectPropertyValue -Object $control -Name "placeholder"
                                text = Get-ObjectPropertyValue -Object $control -Name "text"
                                disabled = Get-ObjectPropertyValue -Object $control -Name "disabled"
                                visibleByLayout = Get-ObjectPropertyValue -Object $control -Name "visibleByLayout"
                            }
                        }
                    }

                    $frameProbe.documents_summary += [pscustomobject] [ordered] @{
                        label = $documentLabel
                        location_href = Get-ObjectPropertyValue -Object $documentSample -Name "locationHref"
                        document_title = Get-ObjectPropertyValue -Object $documentSample -Name "documentTitle"
                        ready_state = Get-ObjectPropertyValue -Object $documentSample -Name "readyState"
                        accessible = Get-ObjectPropertyValue -Object $documentSample -Name "accessible"
                        selectors_found = $documentSelectorsFound
                        texts_found = $documentTextsFound
                        all_required_selectors_found = $documentAllRequiredSelectorsFound
                        all_required_text_found = $documentAllRequiredTextFound
                        all_required_dom_visible = $documentAllRequiredDomVisible
                    }
                }

                $frameProbe.all_required_selectors_found =
                    $frameProbe.selectors_found["#xiigen-smart-skills-prompt"] -eq $true -and
                    $frameProbe.selectors_found["#xiigen-smart-skills-submit"] -eq $true
                $frameProbe.all_required_text_found =
                    $frameProbe.texts_found["XIIGen Smart Skills"] -eq $true -and
                    $frameProbe.texts_found["XIIGen task prompt"] -eq $true
                if ($frameProbe.all_required_dom_visible -eq $true) {
                    $frameProbe.status = "FRAME_CONTEXT_DOM_VISIBLE"
                }

                if ($EnableTypeabilityProof.IsPresent -and
                    $frameProbe.all_required_dom_visible -eq $true -and
                    $probe.typeability_proof_status -ne "TYPEABILITY_PASS") {
                    $typeabilityContext = [ordered] @{
                        source = "frame_context_probe"
                        target_id = $targetId
                        target_source = $targetSource
                        session_id = $sessionId
                        frame_id = $frameId
                        execution_context_id = $executionContextId
                        selector_context_matches_visibility_context = $true
                    }
                    $frameProbe.typeability_proof_attempted = $true
                    $probe.typeability_proof_attempted = $true
                    $typeabilityExpression = New-Row12TypeabilityProbeExpression -ProbeText $TypeabilityProbeText
                    $typeabilityExpressionHash = Get-Sha256Hex -Value $typeabilityExpression
                    $probe.typeability_probe_expression_sha256 = $typeabilityExpressionHash
                    $typeabilityParams = @{
                        expression = $typeabilityExpression
                        returnByValue = $true
                        awaitPromise = $true
                        userGesture = $false
                        contextId = [int] $typeabilityContext.execution_context_id
                    }

                    $typeabilityRecord = [ordered] @{
                        schema = "xiigen.row12.frame_context_typeability_result.v1"
                        attempted = $true
                        status = "TYPEABILITY_NEEDS_FIX"
                        target_id = $targetId
                        typeability_context_source = $typeabilityContext.source
                        typeability_target_id = $typeabilityContext.target_id
                        typeability_target_source = $typeabilityContext.target_source
                        typeability_session_id = $typeabilityContext.session_id
                        typeability_frame_id = $typeabilityContext.frame_id
                        typeability_execution_context_id = $typeabilityContext.execution_context_id
                        typeability_selector_context_matches_visibility_context = $typeabilityContext.selector_context_matches_visibility_context
                        frame_index = $frameIndex
                        frame_id = $frameId
                        execution_context_id = $executionContextId
                        selector = "#xiigen-smart-skills-prompt"
                        probe_text = $TypeabilityProbeText
                        probe_text_sha256 = Get-Sha256Hex -Value $TypeabilityProbeText
                        runtime_evaluate_expression_sha256 = $typeabilityExpressionHash
                        runtime_evaluate_response = $null
                        runtime_exception = $null
                        error = $null
                        raw_result = $null
                        readback_value = $null
                        readback_sha256 = $null
                        readback_matches_probe = $false
                        input_event_dispatched = $false
                        change_event_dispatched = $false
                        submit_button_found = $false
                        submit_button_disabled = $null
                        submit_button_click_invoked = $false
                        submit_button_clicked_by_probe = $false
                        no_submit_performed_by_probe = $false
                        core_path_invoked_by_probe = $false
                        panel_submission_path_invoked_by_probe = $false
                        typeability_no_submit_guard = $false
                    }

                    try {
                        $typeabilityResponse = Invoke-CdpWebSocketCommand -Socket $socket -Id $nextCommandId -Method "Runtime.evaluate" -Params $typeabilityParams -SessionId $sessionId -TimeoutSec $TimeoutSec -EventSink $events
                        $nextCommandId += 1
                        $typeabilityRecord.runtime_evaluate_response = $typeabilityResponse
                        $typeabilityResponseError = Get-ObjectPropertyValue -Object $typeabilityResponse -Name "error"
                        if ($typeabilityResponseError) {
                            throw "Runtime.evaluate typeability proof returned error: $(($typeabilityResponseError | ConvertTo-Json -Depth 8 -Compress))"
                        }

                        $typeabilityEvalResult = Get-ObjectPropertyValue -Object $typeabilityResponse -Name "result"
                        $typeabilityExceptionDetails = Get-ObjectPropertyValue -Object $typeabilityEvalResult -Name "exceptionDetails"
                        if ($typeabilityExceptionDetails) {
                            $typeabilityRecord.runtime_exception = $typeabilityExceptionDetails
                            throw "Runtime.evaluate typeability proof returned exceptionDetails."
                        }

                        $typeabilityRuntimeResult = Get-ObjectPropertyValue -Object $typeabilityEvalResult -Name "result"
                        $typeabilityValue = Get-ObjectPropertyValue -Object $typeabilityRuntimeResult -Name "value"
                        $typeabilityRecord.raw_result = $typeabilityValue
                        $readbackValue = Get-ObjectPropertyValue -Object $typeabilityValue -Name "afterValue"
                        $typeabilityRecord.readback_value = $readbackValue
                        if ($null -ne $readbackValue) {
                            $typeabilityRecord.readback_sha256 = Get-Sha256Hex -Value ([string] $readbackValue)
                        }
                        $typeabilityRecord.readback_matches_probe = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "readbackMatchesProbe") -eq $true
                        $typeabilityInputEvent = Get-ObjectPropertyValue -Object $typeabilityValue -Name "inputEvent"
                        $typeabilityChangeEvent = Get-ObjectPropertyValue -Object $typeabilityValue -Name "changeEvent"
                        $typeabilityRecord.input_event_dispatched = (Get-ObjectPropertyValue -Object $typeabilityInputEvent -Name "dispatched") -eq $true
                        $typeabilityRecord.change_event_dispatched = (Get-ObjectPropertyValue -Object $typeabilityChangeEvent -Name "dispatched") -eq $true
                        $typeabilityRecord.submit_button_found = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "submitButtonFound") -eq $true
                        $typeabilityRecord.submit_button_disabled = Get-ObjectPropertyValue -Object $typeabilityValue -Name "submitButtonDisabled"
                        $typeabilityRecord.submit_button_click_invoked = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "submitButtonClickInvoked") -eq $true
                        $typeabilityRecord.submit_button_clicked_by_probe = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "submitButtonClickedByProbe") -eq $true
                        $typeabilityRecord.no_submit_performed_by_probe = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "noSubmitPerformedByProbe") -eq $true
                        $typeabilityRecord.core_path_invoked_by_probe = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "corePathInvokedByProbe") -eq $true
                        $typeabilityRecord.panel_submission_path_invoked_by_probe = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "panelSubmissionPathInvokedByProbe") -eq $true
                        $typeabilityRecord.typeability_no_submit_guard =
                            $typeabilityRecord.submit_button_click_invoked -ne $true -and
                            $typeabilityRecord.submit_button_clicked_by_probe -ne $true -and
                            $typeabilityRecord.core_path_invoked_by_probe -ne $true -and
                            $typeabilityRecord.panel_submission_path_invoked_by_probe -ne $true

                        if ($typeabilityRecord.readback_matches_probe -eq $true -and
                            $typeabilityRecord.input_event_dispatched -eq $true -and
                            $typeabilityRecord.change_event_dispatched -eq $true -and
                            $typeabilityRecord.typeability_no_submit_guard -eq $true) {
                            $typeabilityRecord.status = "TYPEABILITY_PASS"
                        }
                    } catch {
                        $typeabilityRecord.error = $_.Exception.Message
                        $errorRecord = [pscustomobject] [ordered] @{
                            stage = "frame_context_typeability_probe"
                            method = "Runtime.evaluate_set_textarea_dispatch_input_change_readback"
                            frame_index = $frameIndex
                            frame_id = $frameId
                            message = $_.Exception.Message
                        }
                        $typeabilityProofErrors.Add($errorRecord) | Out-Null
                        $errors.Add($errorRecord) | Out-Null
                    }

                    $frameProbe.typeability_proof = [pscustomobject] $typeabilityRecord
                    if ($typeabilityRecord.status -eq "TYPEABILITY_PASS" -or $null -eq $probe.typeability_proof) {
                        $probe.typeability_proof = [pscustomobject] $typeabilityRecord
                        $probe.typeability_proof_status = [string] $typeabilityRecord.status
                        $probe.typeability_context_source = $typeabilityRecord.typeability_context_source
                        $probe.typeability_target_id = $typeabilityRecord.typeability_target_id
                        $probe.typeability_target_source = $typeabilityRecord.typeability_target_source
                        $probe.typeability_session_id = $typeabilityRecord.typeability_session_id
                        $probe.typeability_frame_id = $typeabilityRecord.typeability_frame_id
                        $probe.typeability_execution_context_id = $typeabilityRecord.typeability_execution_context_id
                        $probe.typeability_selector_context_matches_visibility_context = $typeabilityRecord.typeability_selector_context_matches_visibility_context
                    }
                }

                if ($EnableSubmitResultProof.IsPresent -and
                    $frameProbe.all_required_dom_visible -eq $true -and
                    $probe.submit_first_attached_runtime_visible_context_attempted -ne $true -and
                    $probe.submit_result_proof_attempted -ne $true -and
                    $probe.submit_result_proof_status -ne "SUBMIT_RESULT_PASS") {
                    $submitContext = [ordered] @{
                        source = "frame_context_probe"
                        target_id = $targetId
                        target_source = $targetSource
                        session_id = $sessionId
                        frame_id = $frameId
                        execution_context_id = $executionContextId
                        selector_context_matches_visibility_context = $true
                    }
                    $frameProbe.submit_result_proof_attempted = $true
                    $probe.submit_result_proof_attempted = $true
                    $probe.submit_probe_attempted = $true
                    $submitResultExpression = New-Row12SubmitResultProbeExpression -ProbeText $SubmitResultProbeText -SubmitWaitMs $submitResultJsWaitMs
                    $submitResultExpressionHash = Get-Sha256Hex -Value $submitResultExpression
                    $probe.submit_probe_expression_sha256 = $submitResultExpressionHash
                    $submitResultParams = @{
                        expression = $submitResultExpression
                        returnByValue = $true
                        awaitPromise = $true
                        userGesture = $true
                        contextId = [int] $submitContext.execution_context_id
                    }
                    $submitResultRecord = New-Row12SubmitResultProofRecord `
                        -Schema "xiigen.row12.frame_context_submit_result.v1" `
                        -TargetId $targetId `
                        -TargetSource $targetSource `
                        -SessionId $sessionId `
                        -FrameId $frameId `
                        -ExecutionContextId $executionContextId `
                        -ContextSource $submitContext.source `
                        -SelectorContextMatchesVisibilityContext $true `
                        -CommandPath $probe.frame_context_probe_method `
                        -EvaluationLabel $null `
                        -FrameIndex $frameIndex `
                        -ProbeText $SubmitResultProbeText `
                        -ExpressionHash $submitResultExpressionHash `
                        -SubmitWaitMs $submitResultJsWaitMs `
                        -RuntimeEvaluateTimeoutSec $submitResultCdpTimeoutSec

                    try {
                        $submitResultResponse = Invoke-CdpWebSocketCommand -Socket $socket -Id $nextCommandId -Method "Runtime.evaluate" -Params $submitResultParams -SessionId $sessionId -TimeoutSec $submitResultCdpTimeoutSec -EventSink $events
                        $nextCommandId += 1
                        $submitResultRecord.runtime_evaluate_response = $submitResultResponse
                        $submitResultResponseError = Get-ObjectPropertyValue -Object $submitResultResponse -Name "error"
                        if ($submitResultResponseError) {
                            throw "Runtime.evaluate submit/result proof returned error: $(($submitResultResponseError | ConvertTo-Json -Depth 8 -Compress))"
                        }

                        $submitResultEvalResult = Get-ObjectPropertyValue -Object $submitResultResponse -Name "result"
                        $submitResultExceptionDetails = Get-ObjectPropertyValue -Object $submitResultEvalResult -Name "exceptionDetails"
                        if ($submitResultExceptionDetails) {
                            $submitResultRecord.runtime_exception = $submitResultExceptionDetails
                            throw "Runtime.evaluate submit/result proof returned exceptionDetails."
                        }

                        $submitResultRuntimeResult = Get-ObjectPropertyValue -Object $submitResultEvalResult -Name "result"
                        $submitResultValue = Get-ObjectPropertyValue -Object $submitResultRuntimeResult -Name "value"
                        Update-Row12SubmitResultProofRecord -Record $submitResultRecord -Value $submitResultValue
                    } catch {
                        $submitResultRecord.error = $_.Exception.Message
                        $errorRecord = [pscustomobject] [ordered] @{
                            stage = "frame_context_submit_result_probe"
                            method = "Runtime.evaluate_set_prompt_dispatch_input_change_click_visible_submit_wait_result"
                            frame_index = $frameIndex
                            frame_id = $frameId
                            message = $_.Exception.Message
                        }
                        $submitResultProofErrors.Add($errorRecord) | Out-Null
                        $errors.Add($errorRecord) | Out-Null
                    }

                    $frameProbe.submit_result_proof = [pscustomobject] $submitResultRecord
                    if ($submitResultRecord.status -eq "SUBMIT_RESULT_PASS" -or $null -eq $probe.submit_result_proof) {
                        Copy-Row12SubmitResultProofRecordToProbe -Probe $probe -Record $submitResultRecord
                        $probe.submit_frame_or_evaluated_retry_skipped = $true
                    }
                }
            } catch {
                $frameProbe.error = $_.Exception.Message
                $errorRecord = [pscustomobject] [ordered] @{
                    stage = "frame_context_probe"
                    method = $probe.frame_context_probe_method
                    frame_index = $frameIndex
                    frame_id = $frameId
                    message = $_.Exception.Message
                }
                $frameContextProbeErrors.Add($errorRecord) | Out-Null
                $errors.Add($errorRecord) | Out-Null
            }

            $frameContextProbes.Add([pscustomobject] $frameProbe) | Out-Null
        }

        $probe.frame_context_probes = @($frameContextProbes.ToArray())
        $probe.frame_context_probe_errors = @($frameContextProbeErrors.ToArray())
        $probe.frame_context_required_dom_visible = @($frameContextProbes | Where-Object {
            (Get-ObjectPropertyValue -Object $_ -Name "all_required_dom_visible") -eq $true
        }).Count -gt 0

        try {
            $domSnapshotResponse = Invoke-CdpWebSocketCommand -Socket $socket -Id $nextCommandId -Method "DOMSnapshot.captureSnapshot" -Params @{
                computedStyles = @()
                includeDOMRects = $true
                includePaintOrder = $false
            } -SessionId $sessionId -TimeoutSec $TimeoutSec -EventSink $events
            $nextCommandId += 1
            $probe.dom_snapshot_response = $domSnapshotResponse
            $probe.dom_snapshot_available = $null -ne (Get-ObjectPropertyValue -Object $domSnapshotResponse -Name "result")
        } catch {
            $errors.Add([pscustomobject] @{
                stage = "DOMSnapshot.captureSnapshot"
                message = $_.Exception.Message
            }) | Out-Null
        }

        $contextEvents = @($events | Where-Object {
            (Get-ObjectPropertyValue -Object $_ -Name "method") -eq "Runtime.executionContextCreated"
        })
        $contexts = @()
        foreach ($event in $contextEvents) {
            $eventParams = Get-ObjectPropertyValue -Object $event -Name "params"
            $context = Get-ObjectPropertyValue -Object $eventParams -Name "context"
            if ($context) {
                $contexts += [pscustomobject] [ordered] @{
                    id = Get-ObjectPropertyValue -Object $context -Name "id"
                    name = Get-ObjectPropertyValue -Object $context -Name "name"
                    origin = Get-ObjectPropertyValue -Object $context -Name "origin"
                    uniqueId = Get-ObjectPropertyValue -Object $context -Name "uniqueId"
                    auxData = Get-ObjectPropertyValue -Object $context -Name "auxData"
                }
            }
        }

        $probe.execution_context_events = @($contextEvents)
        $probe.execution_contexts = @($contexts)

        $evaluationRequests = New-Object System.Collections.Generic.List[object]
        $evaluationRequests.Add([pscustomobject] @{
            label = "default"
            contextId = $null
            context = $null
        }) | Out-Null
        $seenContextIds = @{}
        foreach ($context in @($contexts | Select-Object -First 30)) {
            $contextId = Get-ObjectPropertyValue -Object $context -Name "id"
            if ($null -eq $contextId -or [string]::IsNullOrWhiteSpace([string] $contextId)) {
                continue
            }
            $contextKey = [string] $contextId
            if ($seenContextIds.ContainsKey($contextKey)) {
                continue
            }
            $seenContextIds[$contextKey] = $true
            $evaluationRequests.Add([pscustomobject] @{
                label = "context-$contextKey"
                contextId = $contextId
                context = $context
            }) | Out-Null
        }

        foreach ($request in $evaluationRequests) {
            $evalParams = @{
                expression = $Expression
                returnByValue = $true
                awaitPromise = $true
                userGesture = $false
            }
            if ($null -ne $request.contextId -and -not [string]::IsNullOrWhiteSpace([string] $request.contextId)) {
                $evalParams.contextId = [int] $request.contextId
            }

            $evaluation = [ordered] @{
                label = $request.label
                contextId = $request.contextId
                context = $request.context
                status = "NEEDS_FIX"
                runtime_exception = $null
                error = $null
                value = $null
                typeability_proof_attempted = $false
                typeability_proof = $null
            }
            try {
                $response = Invoke-CdpWebSocketCommand -Socket $socket -Id $nextCommandId -Method "Runtime.evaluate" -Params $evalParams -SessionId $sessionId -TimeoutSec $TimeoutSec -EventSink $events
                $nextCommandId += 1
                $exceptionDetails = Get-ObjectPropertyValue -Object $response -Name "exceptionDetails"
                if ($exceptionDetails) {
                    $evaluation.runtime_exception = $exceptionDetails
                    throw "Runtime.evaluate returned exceptionDetails."
                }
                $responseError = Get-ObjectPropertyValue -Object $response -Name "error"
                if ($responseError) {
                    throw "Runtime.evaluate returned error: $(($responseError | ConvertTo-Json -Depth 8 -Compress))"
                }
                $responseResult = Get-ObjectPropertyValue -Object $response -Name "result"
                $runtimeResult = Get-ObjectPropertyValue -Object $responseResult -Name "result"
                $evaluation.value = Get-ObjectPropertyValue -Object $runtimeResult -Name "value"
                $evaluation.status = "EVALUATED"
                $evaluationDomVisible = (Get-ObjectPropertyValue -Object $evaluation.value -Name "allRequiredDomVisible") -eq $true
                $evaluationHasExplicitContext =
                    $null -ne $request.contextId -and
                    -not [string]::IsNullOrWhiteSpace([string] $request.contextId)
                if ($EnableTypeabilityProof.IsPresent -and
                    $evaluationDomVisible -eq $true -and
                    $evaluationHasExplicitContext -eq $true -and
                    $probe.typeability_proof_status -ne "TYPEABILITY_PASS") {
                    $typeabilityContext = [ordered] @{
                        source = "evaluated_document_context_probe"
                        target_id = $targetId
                        target_source = $targetSource
                        session_id = $sessionId
                        frame_id = $null
                        execution_context_id = $request.contextId
                        selector_context_matches_visibility_context = $true
                    }
                    $evaluation.typeability_proof_attempted = $true
                    $probe.typeability_proof_attempted = $true
                    $typeabilityExpression = New-Row12TypeabilityProbeExpression -ProbeText $TypeabilityProbeText
                    $typeabilityExpressionHash = Get-Sha256Hex -Value $typeabilityExpression
                    $probe.typeability_probe_expression_sha256 = $typeabilityExpressionHash
                    $typeabilityParams = @{
                        expression = $typeabilityExpression
                        returnByValue = $true
                        awaitPromise = $true
                        userGesture = $false
                    }
                    if ($null -ne $typeabilityContext.execution_context_id -and -not [string]::IsNullOrWhiteSpace([string] $typeabilityContext.execution_context_id)) {
                        $typeabilityParams.contextId = [int] $typeabilityContext.execution_context_id
                    }

                    $typeabilityRecord = [ordered] @{
                        schema = "xiigen.row12.evaluated_document_typeability_result.v1"
                        attempted = $true
                        status = "TYPEABILITY_NEEDS_FIX"
                        target_id = $targetId
                        typeability_context_source = $typeabilityContext.source
                        typeability_target_id = $typeabilityContext.target_id
                        typeability_target_source = $typeabilityContext.target_source
                        typeability_session_id = $typeabilityContext.session_id
                        typeability_frame_id = $typeabilityContext.frame_id
                        typeability_execution_context_id = $typeabilityContext.execution_context_id
                        typeability_selector_context_matches_visibility_context = $typeabilityContext.selector_context_matches_visibility_context
                        evaluation_label = $request.label
                        execution_context_id = $request.contextId
                        selector = "#xiigen-smart-skills-prompt"
                        probe_text = $TypeabilityProbeText
                        probe_text_sha256 = Get-Sha256Hex -Value $TypeabilityProbeText
                        runtime_evaluate_expression_sha256 = $typeabilityExpressionHash
                        runtime_evaluate_response = $null
                        runtime_exception = $null
                        error = $null
                        raw_result = $null
                        readback_value = $null
                        readback_sha256 = $null
                        readback_matches_probe = $false
                        input_event_dispatched = $false
                        change_event_dispatched = $false
                        submit_button_found = $false
                        submit_button_disabled = $null
                        submit_button_click_invoked = $false
                        submit_button_clicked_by_probe = $false
                        no_submit_performed_by_probe = $false
                        core_path_invoked_by_probe = $false
                        panel_submission_path_invoked_by_probe = $false
                        typeability_no_submit_guard = $false
                    }

                    try {
                        $typeabilityResponse = Invoke-CdpWebSocketCommand -Socket $socket -Id $nextCommandId -Method "Runtime.evaluate" -Params $typeabilityParams -SessionId $sessionId -TimeoutSec $TimeoutSec -EventSink $events
                        $nextCommandId += 1
                        $typeabilityRecord.runtime_evaluate_response = $typeabilityResponse
                        $typeabilityResponseError = Get-ObjectPropertyValue -Object $typeabilityResponse -Name "error"
                        if ($typeabilityResponseError) {
                            throw "Runtime.evaluate typeability proof returned error: $(($typeabilityResponseError | ConvertTo-Json -Depth 8 -Compress))"
                        }

                        $typeabilityEvalResult = Get-ObjectPropertyValue -Object $typeabilityResponse -Name "result"
                        $typeabilityExceptionDetails = Get-ObjectPropertyValue -Object $typeabilityEvalResult -Name "exceptionDetails"
                        if ($typeabilityExceptionDetails) {
                            $typeabilityRecord.runtime_exception = $typeabilityExceptionDetails
                            throw "Runtime.evaluate typeability proof returned exceptionDetails."
                        }

                        $typeabilityRuntimeResult = Get-ObjectPropertyValue -Object $typeabilityEvalResult -Name "result"
                        $typeabilityValue = Get-ObjectPropertyValue -Object $typeabilityRuntimeResult -Name "value"
                        $typeabilityRecord.raw_result = $typeabilityValue
                        $readbackValue = Get-ObjectPropertyValue -Object $typeabilityValue -Name "afterValue"
                        $typeabilityRecord.readback_value = $readbackValue
                        if ($null -ne $readbackValue) {
                            $typeabilityRecord.readback_sha256 = Get-Sha256Hex -Value ([string] $readbackValue)
                        }
                        $typeabilityRecord.readback_matches_probe = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "readbackMatchesProbe") -eq $true
                        $typeabilityInputEvent = Get-ObjectPropertyValue -Object $typeabilityValue -Name "inputEvent"
                        $typeabilityChangeEvent = Get-ObjectPropertyValue -Object $typeabilityValue -Name "changeEvent"
                        $typeabilityRecord.input_event_dispatched = (Get-ObjectPropertyValue -Object $typeabilityInputEvent -Name "dispatched") -eq $true
                        $typeabilityRecord.change_event_dispatched = (Get-ObjectPropertyValue -Object $typeabilityChangeEvent -Name "dispatched") -eq $true
                        $typeabilityRecord.submit_button_found = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "submitButtonFound") -eq $true
                        $typeabilityRecord.submit_button_disabled = Get-ObjectPropertyValue -Object $typeabilityValue -Name "submitButtonDisabled"
                        $typeabilityRecord.submit_button_click_invoked = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "submitButtonClickInvoked") -eq $true
                        $typeabilityRecord.submit_button_clicked_by_probe = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "submitButtonClickedByProbe") -eq $true
                        $typeabilityRecord.no_submit_performed_by_probe = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "noSubmitPerformedByProbe") -eq $true
                        $typeabilityRecord.core_path_invoked_by_probe = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "corePathInvokedByProbe") -eq $true
                        $typeabilityRecord.panel_submission_path_invoked_by_probe = (Get-ObjectPropertyValue -Object $typeabilityValue -Name "panelSubmissionPathInvokedByProbe") -eq $true
                        $typeabilityRecord.typeability_no_submit_guard =
                            $typeabilityRecord.submit_button_click_invoked -ne $true -and
                            $typeabilityRecord.submit_button_clicked_by_probe -ne $true -and
                            $typeabilityRecord.core_path_invoked_by_probe -ne $true -and
                            $typeabilityRecord.panel_submission_path_invoked_by_probe -ne $true

                        if ($typeabilityRecord.readback_matches_probe -eq $true -and
                            $typeabilityRecord.input_event_dispatched -eq $true -and
                            $typeabilityRecord.change_event_dispatched -eq $true -and
                            $typeabilityRecord.typeability_no_submit_guard -eq $true) {
                            $typeabilityRecord.status = "TYPEABILITY_PASS"
                        }
                    } catch {
                        $typeabilityRecord.error = $_.Exception.Message
                        $errorRecord = [pscustomobject] [ordered] @{
                            stage = "evaluated_document_typeability_probe"
                            method = "Runtime.evaluate_set_textarea_dispatch_input_change_readback"
                            evaluation_label = $request.label
                            context_id = $request.contextId
                            message = $_.Exception.Message
                        }
                        $typeabilityProofErrors.Add($errorRecord) | Out-Null
                        $errors.Add($errorRecord) | Out-Null
                    }

                    $evaluation.typeability_proof = [pscustomobject] $typeabilityRecord
                    if ($typeabilityRecord.status -eq "TYPEABILITY_PASS" -or $null -eq $probe.typeability_proof) {
                        $probe.typeability_proof = [pscustomobject] $typeabilityRecord
                        $probe.typeability_proof_status = [string] $typeabilityRecord.status
                        $probe.typeability_context_source = $typeabilityRecord.typeability_context_source
                        $probe.typeability_target_id = $typeabilityRecord.typeability_target_id
                        $probe.typeability_target_source = $typeabilityRecord.typeability_target_source
                        $probe.typeability_session_id = $typeabilityRecord.typeability_session_id
                        $probe.typeability_frame_id = $typeabilityRecord.typeability_frame_id
                        $probe.typeability_execution_context_id = $typeabilityRecord.typeability_execution_context_id
                        $probe.typeability_selector_context_matches_visibility_context = $typeabilityRecord.typeability_selector_context_matches_visibility_context
                    }
                }

                if ($EnableSubmitResultProof.IsPresent -and
                    $evaluationDomVisible -eq $true -and
                    $evaluationHasExplicitContext -eq $true -and
                    $probe.submit_first_attached_runtime_visible_context_attempted -ne $true -and
                    $probe.submit_result_proof_attempted -ne $true -and
                    $probe.submit_result_proof_status -ne "SUBMIT_RESULT_PASS") {
                    $submitContext = [ordered] @{
                        source = "evaluated_document_context_probe"
                        target_id = $targetId
                        target_source = $targetSource
                        session_id = $sessionId
                        frame_id = $null
                        execution_context_id = $request.contextId
                        selector_context_matches_visibility_context = $true
                    }
                    $evaluation.submit_result_proof_attempted = $true
                    $probe.submit_result_proof_attempted = $true
                    $probe.submit_probe_attempted = $true
                    $submitResultExpression = New-Row12SubmitResultProbeExpression -ProbeText $SubmitResultProbeText -SubmitWaitMs $submitResultJsWaitMs
                    $submitResultExpressionHash = Get-Sha256Hex -Value $submitResultExpression
                    $probe.submit_probe_expression_sha256 = $submitResultExpressionHash
                    $submitResultParams = @{
                        expression = $submitResultExpression
                        returnByValue = $true
                        awaitPromise = $true
                        userGesture = $true
                    }
                    if ($null -ne $submitContext.execution_context_id -and -not [string]::IsNullOrWhiteSpace([string] $submitContext.execution_context_id)) {
                        $submitResultParams.contextId = [int] $submitContext.execution_context_id
                    }
                    $submitResultRecord = New-Row12SubmitResultProofRecord `
                        -Schema "xiigen.row12.evaluated_document_submit_result.v1" `
                        -TargetId $targetId `
                        -TargetSource $targetSource `
                        -SessionId $sessionId `
                        -FrameId $null `
                        -ExecutionContextId $request.contextId `
                        -ContextSource $submitContext.source `
                        -SelectorContextMatchesVisibilityContext $true `
                        -CommandPath "evaluated_document_context_Runtime.evaluate" `
                        -EvaluationLabel $request.label `
                        -FrameIndex $null `
                        -ProbeText $SubmitResultProbeText `
                        -ExpressionHash $submitResultExpressionHash `
                        -SubmitWaitMs $submitResultJsWaitMs `
                        -RuntimeEvaluateTimeoutSec $submitResultCdpTimeoutSec

                    try {
                        $submitResultResponse = Invoke-CdpWebSocketCommand -Socket $socket -Id $nextCommandId -Method "Runtime.evaluate" -Params $submitResultParams -SessionId $sessionId -TimeoutSec $submitResultCdpTimeoutSec -EventSink $events
                        $nextCommandId += 1
                        $submitResultRecord.runtime_evaluate_response = $submitResultResponse
                        $submitResultResponseError = Get-ObjectPropertyValue -Object $submitResultResponse -Name "error"
                        if ($submitResultResponseError) {
                            throw "Runtime.evaluate submit/result proof returned error: $(($submitResultResponseError | ConvertTo-Json -Depth 8 -Compress))"
                        }

                        $submitResultEvalResult = Get-ObjectPropertyValue -Object $submitResultResponse -Name "result"
                        $submitResultExceptionDetails = Get-ObjectPropertyValue -Object $submitResultEvalResult -Name "exceptionDetails"
                        if ($submitResultExceptionDetails) {
                            $submitResultRecord.runtime_exception = $submitResultExceptionDetails
                            throw "Runtime.evaluate submit/result proof returned exceptionDetails."
                        }

                        $submitResultRuntimeResult = Get-ObjectPropertyValue -Object $submitResultEvalResult -Name "result"
                        $submitResultValue = Get-ObjectPropertyValue -Object $submitResultRuntimeResult -Name "value"
                        Update-Row12SubmitResultProofRecord -Record $submitResultRecord -Value $submitResultValue
                    } catch {
                        $submitResultRecord.error = $_.Exception.Message
                        $errorRecord = [pscustomobject] [ordered] @{
                            stage = "evaluated_document_submit_result_probe"
                            method = "Runtime.evaluate_set_prompt_dispatch_input_change_click_visible_submit_wait_result"
                            evaluation_label = $request.label
                            context_id = $request.contextId
                            message = $_.Exception.Message
                        }
                        $submitResultProofErrors.Add($errorRecord) | Out-Null
                        $errors.Add($errorRecord) | Out-Null
                    }

                    $evaluation.submit_result_proof = [pscustomobject] $submitResultRecord
                    if ($submitResultRecord.status -eq "SUBMIT_RESULT_PASS" -or $null -eq $probe.submit_result_proof) {
                        Copy-Row12SubmitResultProofRecordToProbe -Probe $probe -Record $submitResultRecord
                    }
                }
            } catch {
                $evaluation.error = $_.Exception.Message
            }
            $evaluations.Add([pscustomobject] $evaluation) | Out-Null
        }
    } catch {
        $errors.Add([pscustomobject] @{
            stage = "target_probe"
            message = $_.Exception.Message
        }) | Out-Null
    } finally {
        try {
            if ($sessionId -and $socket.State -eq [System.Net.WebSockets.WebSocketState]::Open) {
                [void] (Invoke-CdpWebSocketCommand -Socket $socket -Id 9999 -Method "Target.detachFromTarget" -Params @{
                    sessionId = $sessionId
                } -TimeoutSec 2 -EventSink $events)
            }
        } catch {
        }
        try {
            if ($socket.State -eq [System.Net.WebSockets.WebSocketState]::Open) {
                [void] $socket.CloseAsync(
                    [System.Net.WebSockets.WebSocketCloseStatus]::NormalClosure,
                    "row12-read-only-inner-frame-probe-complete",
                    [System.Threading.CancellationToken]::None
                ).GetAwaiter().GetResult()
            }
        } catch {
        }
        $socket.Dispose()
    }

    $probe.evaluations = @($evaluations.ToArray())
    $probe.typeability_proof_errors = @($typeabilityProofErrors.ToArray())
    $probe.submit_result_proof_errors = @($submitResultProofErrors.ToArray())
    $probe.errors = @($errors.ToArray())
    return [pscustomobject] $probe
}

function Invoke-Row12CdpWebViewDomInspection {
    param(
        [int] $Port,
        [string] $RunRoot,
        [string] $InspectionTiming = "before_process_close",
        [switch] $EnableTypeabilityProof,
        [AllowNull()] [string] $TypeabilityProbeText,
        [switch] $EnableSubmitResultProof,
        [AllowNull()] [string] $SubmitResultProbeText,
        [int] $SubmitResultWaitMs = 20000,
        [int] $SubmitResultCdpTimeoutSec = 30,
        [int] $EndpointTimeoutSec = 12
    )

    $targetsPath = Join-Path $RunRoot "row12-cdp-targets.json"
    $queryPath = Join-Path $RunRoot "row12-cdp-webview-dom-query.json"
    $innerFrameSummaryPath = Join-Path $RunRoot "row12-cdp-webview-inner-frame-summary.json"
    $endpoint = "http://127.0.0.1:$Port/json"
    $versionEndpoint = "http://127.0.0.1:$Port/json/version"
    $versionPath = Join-Path $RunRoot "row12-cdp-browser-version.json"
    $browserTargetsPath = Join-Path $RunRoot "row12-browser-cdp-target-gettargets.json"
    $expression = @'
(() => {
  const selectorNames = ["#xiigen-smart-skills-prompt", "#xiigen-smart-skills-submit"];
  const textNames = ["XIIGen Smart Skills", "XIIGen task prompt"];
  const markerNames = ["xiigen-smart-skills-prompt", "xiigen-smart-skills-submit", "XIIGen Smart Skills", "XIIGen task prompt"];

  function safeString(value) {
    return value == null ? "" : String(value);
  }

  function inspectDocument(doc, label, frameMeta) {
    const result = {
      label,
      frameMeta: frameMeta || null,
      accessible: false,
      error: null,
      locationHref: null,
      documentTitle: null,
      readyState: null,
      selectorResults: {},
      textResults: {},
      markerResults: {},
      outerHTMLLength: 0,
      textContentLength: 0,
      bodyOuterHTMLLength: 0,
      outerHTMLSample: "",
      bodyOuterHTMLSample: "",
      textContentSample: "",
      bodyTextSample: "",
      controlsSummary: [],
      allRequiredSelectorsFound: false,
      allRequiredTextFound: false,
      allRequiredDomVisible: false,
      readOnly: true
    };

    try {
      if (!doc || !doc.documentElement) {
        result.error = "No documentElement available.";
        return result;
      }

      result.accessible = true;
      result.locationHref = doc.location ? safeString(doc.location.href) : null;
      result.documentTitle = safeString(doc.title || "");
      result.readyState = safeString(doc.readyState || "");

      const outerHTML = safeString(doc.documentElement.outerHTML);
      const bodyOuterHTML = safeString(doc.body ? doc.body.outerHTML : "");
      const textContent = safeString(doc.documentElement.innerText || doc.documentElement.textContent || "");
      result.outerHTMLLength = outerHTML.length;
      result.bodyOuterHTMLLength = bodyOuterHTML.length;
      result.textContentLength = textContent.length;
      result.outerHTMLSample = outerHTML.slice(0, 12000);
      result.bodyOuterHTMLSample = bodyOuterHTML.slice(0, 12000);
      result.textContentSample = textContent.slice(0, 4000);
      result.bodyTextSample = result.textContentSample;

      for (const selector of selectorNames) {
        const node = doc.querySelector(selector);
        if (!node) {
          result.selectorResults[selector] = { found: false };
          continue;
        }
        const rects = Array.from(node.getClientRects()).map((rect) => ({
          x: rect.x,
          y: rect.y,
          width: rect.width,
          height: rect.height
        }));
        result.selectorResults[selector] = {
          found: true,
          tagName: node.tagName,
          id: node.id || null,
          type: node.getAttribute("type"),
          ariaLabel: node.getAttribute("aria-label"),
          placeholder: node.getAttribute("placeholder"),
          text: safeString(node.innerText || node.textContent || node.value || "").slice(0, 500),
          valueLength: typeof node.value === "string" ? node.value.length : null,
          disabled: !!node.disabled,
          visibleByLayout: !!(node.offsetWidth || node.offsetHeight || rects.length),
          rects
        };
      }

      for (const text of textNames) {
        result.textResults[text] = textContent.includes(text);
      }

      for (const marker of markerNames) {
        result.markerResults[marker] = result.outerHTMLSample.includes(marker) ||
          result.bodyOuterHTMLSample.includes(marker) ||
          result.textContentSample.includes(marker);
      }

      result.controlsSummary = Array.from(doc.querySelectorAll("textarea, button, input, select, [role='button'], [contenteditable='true']")).slice(0, 50).map((node) => ({
        tagName: node.tagName,
        id: node.id || null,
        type: node.getAttribute("type"),
        role: node.getAttribute("role"),
        ariaLabel: node.getAttribute("aria-label"),
        placeholder: node.getAttribute("placeholder"),
        text: safeString(node.innerText || node.textContent || node.value || "").slice(0, 300),
        disabled: !!node.disabled,
        visibleByLayout: !!(node.offsetWidth || node.offsetHeight || node.getClientRects().length)
      }));

      result.allRequiredSelectorsFound = selectorNames.every((selector) => result.selectorResults[selector] && result.selectorResults[selector].found === true);
      result.allRequiredTextFound = textNames.every((text) => result.textResults[text] === true);
      result.allRequiredDomVisible = result.allRequiredSelectorsFound && result.allRequiredTextFound;
      return result;
    } catch (error) {
      result.error = error && error.message ? error.message : String(error);
      return result;
    }
  }

  if (typeof document === "undefined") {
    return {
      hasDocument: false,
      documentUnavailableReason: "Runtime context has no document object.",
      documents: [],
      iframeProbes: [],
      selectorResults: {},
      textResults: {},
      markerResults: {},
      outerHTMLLength: 0,
      textContentLength: 0,
      bodyOuterHTMLLength: 0,
      outerHTMLSample: "",
      bodyOuterHTMLSample: "",
      textContentSample: "",
      bodyTextSample: "",
      controlsSummary: [],
      allRequiredSelectorsFound: false,
      allRequiredTextFound: false,
      allRequiredDomVisible: false,
      readOnly: true
    };
  }

  const documents = [];
  const rootDocument = inspectDocument(document, "current-document", null);
  documents.push(rootDocument);

  const iframeProbes = Array.from(document.querySelectorAll("iframe")).slice(0, 25).map((frame, index) => {
    const rect = frame.getBoundingClientRect();
    const frameMeta = {
      index,
      id: frame.id || null,
      name: frame.getAttribute("name"),
      title: frame.getAttribute("title"),
      src: frame.getAttribute("src"),
      className: frame.className || null,
      rect: {
        x: rect.x,
        y: rect.y,
        width: rect.width,
        height: rect.height
      },
      visibleByLayout: !!(frame.offsetWidth || frame.offsetHeight || frame.getClientRects().length)
    };
    const probe = {
      frameMeta,
      contentDocumentAccessible: false,
      error: null,
      document: null
    };
    try {
      const childDocument = frame.contentDocument;
      if (childDocument) {
        probe.contentDocumentAccessible = true;
        probe.document = inspectDocument(childDocument, "iframe[" + index + "].contentDocument", frameMeta);
        documents.push(probe.document);
      }
    } catch (error) {
      probe.error = error && error.message ? error.message : String(error);
    }
    return probe;
  });

  const anyDocumentWithRequiredDom = documents.some((entry) => entry.allRequiredDomVisible === true);
  const anyRequiredSelectorsFound = selectorNames.every((selector) => documents.some((entry) => entry.selectorResults && entry.selectorResults[selector] && entry.selectorResults[selector].found === true));
  const anyRequiredTextFound = textNames.every((text) => documents.some((entry) => entry.textResults && entry.textResults[text] === true));

  return {
    hasDocument: true,
    contextLocationHref: safeString(location.href),
    contextDocumentTitle: safeString(document.title || ""),
    contextReadyState: safeString(document.readyState || ""),
    locationHref: rootDocument.locationHref,
    documentTitle: rootDocument.documentTitle,
    readyState: rootDocument.readyState,
    selectorResults: rootDocument.selectorResults,
    textResults: rootDocument.textResults,
    markerResults: rootDocument.markerResults,
    outerHTMLLength: rootDocument.outerHTMLLength,
    textContentLength: rootDocument.textContentLength,
    bodyOuterHTMLLength: rootDocument.bodyOuterHTMLLength,
    outerHTMLSample: rootDocument.outerHTMLSample,
    bodyOuterHTMLSample: rootDocument.bodyOuterHTMLSample,
    textContentSample: rootDocument.textContentSample,
    bodyTextSample: rootDocument.bodyTextSample,
    controlsSummary: rootDocument.controlsSummary,
    documents,
    iframeProbes,
    iframeCount: iframeProbes.length,
    accessibleIframeDocumentCount: iframeProbes.filter((probe) => probe.contentDocumentAccessible).length,
    anyRequiredSelectorsFound,
    anyRequiredTextFound,
    allRequiredSelectorsFound: anyRequiredSelectorsFound,
    allRequiredTextFound: anyRequiredTextFound,
    allRequiredDomVisible: anyDocumentWithRequiredDom,
    readOnly: true
  };
})()
'@

    $inspection = [ordered] @{
        schema = "xiigen.row12.cursor_panel.cdp_webview_dom.v1"
        attempted = $true
        generated_utc = (Get-Date).ToUniversalTime().ToString("o")
        inspection_timing = $InspectionTiming
        port = $Port
        endpoint = $endpoint
        version_endpoint = $versionEndpoint
        status = "BROWSER_CDP_UNAVAILABLE"
        targets_path = $targetsPath
        query_path = $queryPath
        browser_version_path = $versionPath
        browser_target_gettargets_path = $browserTargetsPath
        browser_version = $null
        browser_version_available = $false
        browser_websocket_debugger_url = $null
        browser_websocket_debugger_url_present = $false
        json_endpoint_available = $false
        browser_target_gettargets_attempted = $false
        browser_target_gettargets_available = $false
        browser_target_gettargets_response = $null
        browser_targets = @()
        browser_target_count = 0
        browser_candidate_targets = @()
        browser_candidate_target_count = 0
        webview_target_count = 0
        non_webview_candidate_count = 0
        target_selection = $null
        typeability_proof_requested = $EnableTypeabilityProof.IsPresent
        typeability_proof_attempted = $false
        typeability_proof_status = if ($EnableTypeabilityProof.IsPresent) { "NOT_REACHED" } else { "NOT_REQUESTED" }
        typeability_proof = $null
        typeability_probe_text = if ($EnableTypeabilityProof.IsPresent) { $TypeabilityProbeText } else { $null }
        typeability_probe_text_sha256 = if ($EnableTypeabilityProof.IsPresent) { Get-Sha256Hex -Value $TypeabilityProbeText } else { $null }
        typeability_probe_attempted = $false
        typeability_probe_text_hash = if ($EnableTypeabilityProof.IsPresent) { Get-Sha256Hex -Value $TypeabilityProbeText } else { $null }
        typeability_probe_expression_sha256 = $null
        typeability_context_source = $null
        typeability_target_id = $null
        typeability_target_source = $null
        typeability_session_id = $null
        typeability_frame_id = $null
        typeability_execution_context_id = $null
        typeability_selector_context_matches_visibility_context = $false
        typeability_readback_value = $null
        typeability_readback_sha256 = $null
        typeability_readback_matches_probe = $false
        typeability_readback_matches = $false
        typeability_no_submit_performed_by_probe = $false
        typeability_submit_button_click_invoked = $false
        typeability_submit_button_clicked_by_probe = $false
        typeability_core_path_invoked_by_probe = $false
        typeability_panel_submission_path_invoked_by_probe = $false
        typeability_no_submit_guard = $false
        typeability_core_invoked = $false
        typeability_proof_errors = @()
        submit_result_proof_requested = $EnableSubmitResultProof.IsPresent
        submit_result_proof_attempted = $false
        submit_result_proof_status = if ($EnableSubmitResultProof.IsPresent) { "NOT_REACHED" } else { "NOT_REQUESTED" }
        submit_result_proof = $null
        submit_result_proof_errors = @()
        submit_probe_attempted = $false
        submit_probe_text = if ($EnableSubmitResultProof.IsPresent) { $SubmitResultProbeText } else { $null }
        submit_probe_text_sha256 = if ($EnableSubmitResultProof.IsPresent) { Get-Sha256Hex -Value $SubmitResultProbeText } else { $null }
        submit_probe_text_hash = if ($EnableSubmitResultProof.IsPresent) { Get-Sha256Hex -Value $SubmitResultProbeText } else { $null }
        submit_probe_expression_sha256 = $null
        submit_js_wait_ms = if ($EnableSubmitResultProof.IsPresent) { [Math]::Max(20000, [int] $SubmitResultWaitMs) } else { $null }
        submit_cdp_timeout_sec = if ($EnableSubmitResultProof.IsPresent) { [Math]::Max([int] $SubmitResultCdpTimeoutSec, ([int] [Math]::Ceiling(([Math]::Max(20000, [int] $SubmitResultWaitMs)) / 1000.0)) + 10) } else { $null }
        submit_proof_scope = if ($EnableSubmitResultProof.IsPresent) { "first_attached_runtime_visible_context_path" } else { $null }
        submit_retry_policy = if ($EnableSubmitResultProof.IsPresent) { "Prefer the first attached-runtime visible context path; frame/evaluated submit proof is allowed only if no attached-runtime submit path was attempted." } else { $null }
        submit_first_attached_runtime_visible_context_attempted = $false
        submit_frame_or_evaluated_retry_skipped = $false
        submit_context_source = $null
        submit_target_id = $null
        submit_target_source = $null
        submit_session_id = $null
        submit_frame_id = $null
        submit_execution_context_id = $null
        submit_selector_context_matches_visibility_context = $false
        submit_button_click_invoked = $false
        submit_button_clicked_by_probe = $false
        panel_submission_path_invoked = $false
        core_path_invoked = $false
        submit_test_only_handler_used = $false
        submit_not_test_only_path = $false
        result_rendered_in_webview = $false
        result_text = $null
        result_text_sha256 = $null
        result_status_text = $null
        result_response_ref = $null
        result_trace_ref = $null
        result_host_panel_ui_ref = $null
        result_correlation_id = $null
        submit_evidence_refs = $null
        submit_no_readiness_claim = if ($EnableSubmitResultProof.IsPresent) { $true } else { $null }
        submit_readiness_claimed = $false
        attached_session_id_present = $false
        attached_session_command_path = $null
        attached_runtime_evaluate_attempted = $false
        attached_runtime_evaluate_available = $false
        attached_runtime_evaluate_error = $null
        attached_runtime_selectors_found = [ordered] @{
            "#xiigen-smart-skills-prompt" = $false
            "#xiigen-smart-skills-submit" = $false
        }
        attached_runtime_texts_found = [ordered] @{
            "XIIGen Smart Skills" = $false
            "XIIGen task prompt" = $false
        }
        attached_runtime_all_required_selectors_found = $false
        attached_runtime_all_required_text_found = $false
        attached_runtime_all_required_dom_visible = $false
        dom_snapshot_files = @()
        cdp_frame_tree_files = @()
        cdp_execution_context_files = @()
        cdp_protocol_dom_snapshot_files = @()
        inner_document_sample_files = @()
        inner_frame_summary_path = $innerFrameSummaryPath
        expected_snapshot_markers_present = [ordered] @{
            "xiigen-smart-skills-prompt" = $false
            "xiigen-smart-skills-submit" = $false
            "XIIGen Smart Skills" = $false
            "XIIGen task prompt" = $false
        }
        browser_target_gettargets_error = $null
        targets = @()
        target_count = 0
        merged_target_count = 0
        json_targets = @()
        json_target_count = 0
        candidate_targets = @()
        candidate_target_count = 0
        query_results = @()
        selector_text_requirements = [ordered] @{
            selectors = @("#xiigen-smart-skills-prompt", "#xiigen-smart-skills-submit")
            text = @("XIIGen Smart Skills", "XIIGen task prompt")
        }
        all_required_selectors_found = $false
        all_required_text_found = $false
        all_required_dom_visible = $false
        error = $null
        last_endpoint_error = $null
        last_version_error = $null
    }

    $targets = $null
    $deadline = (Get-Date).AddSeconds($EndpointTimeoutSec)
    do {
        try {
            $targetsResponse = Invoke-RestMethod -Uri $endpoint -TimeoutSec 3 -ErrorAction Stop
            $targets = @(ConvertTo-NonNullArray -Value $targetsResponse)
            $inspection.json_endpoint_available = $true
            break
        } catch {
            $inspection.last_endpoint_error = $_.Exception.Message
            Start-Sleep -Milliseconds 500
        }
    } while ((Get-Date) -lt $deadline)

    if ($null -eq $targets) {
        $inspection.error = "CDP endpoint did not return /json targets: $($inspection.last_endpoint_error)"
        $targets = @()
    }

    $summaries = @()
    for ($i = 0; $i -lt $targets.Count; $i += 1) {
        $summaries += ConvertTo-CdpTargetSummary -Target $targets[$i] -Index $i
    }

    $inspection.json_targets = @($summaries)
    $inspection.json_target_count = $summaries.Count
    $inspection.targets = @($summaries)
    $inspection.target_count = $summaries.Count
    Write-JsonFile -Path $targetsPath -Value ([ordered] @{
        schema = "xiigen.row12.cursor_panel.cdp_targets.v1"
        generated_utc = (Get-Date).ToUniversalTime().ToString("o")
        port = $Port
        endpoint = $endpoint
        version_endpoint = $versionEndpoint
        target_count = $summaries.Count
        targets = @($summaries)
    })

    $browserVersion = $null
    $browserWebSocketDebuggerUrl = $null
    $versionDeadline = (Get-Date).AddSeconds($EndpointTimeoutSec)
    do {
        try {
            $browserVersion = Invoke-RestMethod -Uri $versionEndpoint -TimeoutSec 3 -ErrorAction Stop
            break
        } catch {
            $inspection.last_version_error = $_.Exception.Message
            Start-Sleep -Milliseconds 500
        }
    } while ((Get-Date) -lt $versionDeadline)

    if ($browserVersion) {
        $browserWebSocketDebuggerUrl = Get-ObjectPropertyValue -Object $browserVersion -Name "webSocketDebuggerUrl"
        $inspection.browser_version = $browserVersion
        $inspection.browser_websocket_debugger_url = $browserWebSocketDebuggerUrl
    }
    $inspection.browser_version_available = $null -ne $browserVersion
    $inspection.browser_websocket_debugger_url_present = -not [string]::IsNullOrWhiteSpace([string] $browserWebSocketDebuggerUrl)
    Write-JsonFile -Path $versionPath -Value ([ordered] @{
        schema = "xiigen.row12.cursor_panel.cdp_browser_version.v1"
        generated_utc = (Get-Date).ToUniversalTime().ToString("o")
        port = $Port
        endpoint = $versionEndpoint
        available = $inspection.browser_version_available
        browser_websocket_debugger_url = $browserWebSocketDebuggerUrl
        browser_websocket_debugger_url_present = $inspection.browser_websocket_debugger_url_present
        version = $browserVersion
        error = if ($browserVersion) { $null } else { $inspection.last_version_error }
    })

    $browserTargetSummaries = @()
    if (-not [string]::IsNullOrWhiteSpace([string] $browserWebSocketDebuggerUrl)) {
        $inspection.browser_target_gettargets_attempted = $true
        $socket = [System.Net.WebSockets.ClientWebSocket]::new()
        try {
            $cancel = New-Object System.Threading.CancellationTokenSource
            try {
                $cancel.CancelAfter([TimeSpan]::FromSeconds($EndpointTimeoutSec))
                [void] $socket.ConnectAsync([Uri] $browserWebSocketDebuggerUrl, $cancel.Token).GetAwaiter().GetResult()
            } finally {
                $cancel.Dispose()
            }

            $targetResponse = Invoke-CdpWebSocketCommand -Socket $socket -Id 1 -Method "Target.getTargets" -Params @{} -TimeoutSec $EndpointTimeoutSec
            $inspection.browser_target_gettargets_response = $targetResponse
            $targetResponseError = Get-ObjectPropertyValue -Object $targetResponse -Name "error"
            if ($targetResponseError) {
                throw "Target.getTargets returned error: $(($targetResponseError | ConvertTo-Json -Depth 8 -Compress))"
            }
            $targetResult = Get-ObjectPropertyValue -Object $targetResponse -Name "result"
            $targetInfosRaw = Get-ObjectPropertyValue -Object $targetResult -Name "targetInfos"
            if ($null -eq $targetInfosRaw) {
                throw "Target.getTargets response did not include targetInfos."
            }
            $targetInfos = @(ConvertTo-NonNullArray -Value $targetInfosRaw)
            for ($i = 0; $i -lt $targetInfos.Count; $i += 1) {
                $browserTargetSummaries += ConvertTo-CdpTargetSummary -Target $targetInfos[$i] -Index $i -Source "browser_target_gettargets"
            }

            $inspection.browser_target_gettargets_available = $true
        } catch {
            $inspection.browser_target_gettargets_error = $_.Exception.Message
        } finally {
            try {
                if ($socket.State -eq [System.Net.WebSockets.WebSocketState]::Open) {
                    [void] $socket.CloseAsync(
                        [System.Net.WebSockets.WebSocketCloseStatus]::NormalClosure,
                        "row12-browser-target-discovery-complete",
                        [System.Threading.CancellationToken]::None
                    ).GetAwaiter().GetResult()
                }
            } catch {
            }
            $socket.Dispose()
        }
    } else {
        $inspection.browser_target_gettargets_error = "No browser websocket URL was available from /json/version."
    }

    $inspection.browser_targets = @($browserTargetSummaries)
    $inspection.browser_target_count = $browserTargetSummaries.Count
    Write-JsonFile -Path $browserTargetsPath -Value ([ordered] @{
        schema = "xiigen.row12.cursor_panel.browser_target_gettargets.v1"
        generated_utc = (Get-Date).ToUniversalTime().ToString("o")
        port = $Port
        version_endpoint = $versionEndpoint
        version_available = $inspection.browser_version_available
        browser_websocket_debugger_url = $browserWebSocketDebuggerUrl
        browser_websocket_debugger_url_present = $inspection.browser_websocket_debugger_url_present
        attempted = $inspection.browser_target_gettargets_attempted
        available = $inspection.browser_target_gettargets_available
        response = $inspection.browser_target_gettargets_response
        json_target_count = $summaries.Count
        target_count = $browserTargetSummaries.Count
        targets = @($browserTargetSummaries)
        error = $inspection.browser_target_gettargets_error
    })

    $allTargetSummaries = @($summaries) + @($browserTargetSummaries)
    $inspection.targets = @($allTargetSummaries)
    $inspection.target_count = $allTargetSummaries.Count
    $inspection.merged_target_count = $allTargetSummaries.Count
    Write-JsonFile -Path $targetsPath -Value ([ordered] @{
        schema = "xiigen.row12.cursor_panel.cdp_targets.v1"
        generated_utc = (Get-Date).ToUniversalTime().ToString("o")
        port = $Port
        endpoint = $endpoint
        version_endpoint = $versionEndpoint
        browser_websocket_debugger_url = $browserWebSocketDebuggerUrl
        json_target_count = $summaries.Count
        browser_target_count = $browserTargetSummaries.Count
        merged_target_count = $allTargetSummaries.Count
        target_count = $allTargetSummaries.Count
        targets = @($allTargetSummaries)
    })

    $webviewTargets = @($allTargetSummaries | Where-Object { @($_.candidate_reasons) -contains "vscode-webview" })
    $nonWebviewCandidates = @($allTargetSummaries | Where-Object {
        $_.is_candidate -eq $true -and @($_.candidate_reasons) -notcontains "vscode-webview"
    })
    $candidates = if ($webviewTargets.Count -gt 0) { @($webviewTargets) } else { @($nonWebviewCandidates) }
    $browserCandidates = @($browserTargetSummaries | Where-Object { $_.is_candidate -eq $true })
    $inspection.candidate_targets = @($candidates)
    $inspection.candidate_target_count = $candidates.Count
    $inspection.browser_candidate_targets = @($browserCandidates)
    $inspection.browser_candidate_target_count = $browserCandidates.Count
    $inspection.webview_target_count = $webviewTargets.Count
    $inspection.non_webview_candidate_count = $nonWebviewCandidates.Count
    $inspection.target_selection = if ($webviewTargets.Count -gt 0) {
        "vscode-webview targets only"
    } elseif ($nonWebviewCandidates.Count -gt 0) {
        "fallback non-webview candidate targets only because no vscode-webview target was discovered"
    } else {
        "none"
    }

    if ($candidates.Count -eq 0) {
        if ($inspection.browser_target_gettargets_available -eq $true) {
            $inspection.status = "INNER_FRAME_NO_APP_DOM"
        } else {
            $inspection.status = "NEEDS_FIX"
        }
        Write-JsonFile -Path $innerFrameSummaryPath -Value ([ordered] @{
            schema = "xiigen.row12.inner_frame_summary.v1"
            generated_utc = (Get-Date).ToUniversalTime().ToString("o")
            status = $inspection.status
            target_selection = $inspection.target_selection
            candidate_target_count = 0
            all_required_selectors_found = $false
            all_required_text_found = $false
            all_required_dom_visible = $false
            frame_tree_files = @()
            execution_context_files = @()
            protocol_dom_snapshot_files = @()
            inner_document_sample_files = @()
            selector_text_requirements = $inspection.selector_text_requirements
            query_results = @()
        })
        Write-JsonFile -Path $queryPath -Value $inspection
        return [pscustomobject] $inspection
    }

    $queryResults = @()
    $submitResultProofAlreadyAttempted = $false
    foreach ($target in $candidates) {
        $targetResult = [ordered] @{
            target = $target
            attempted = $true
            status = "NEEDS_FIX"
            error = $null
            error_type = $null
            error_line = $null
            error_command = $null
            error_position = $null
            error_stack = $null
            runtime_exception = $null
            result = $null
            attach_method = $null
            attached_session_id_present = $false
            attached_session_command_path = $null
            attached_runtime_evaluate_attempted = $false
            attached_runtime_evaluate_available = $false
            attached_runtime_evaluate_error = $null
            attached_runtime_selectors_found = [ordered] @{
                "#xiigen-smart-skills-prompt" = $false
                "#xiigen-smart-skills-submit" = $false
            }
            attached_runtime_texts_found = [ordered] @{
                "XIIGen Smart Skills" = $false
                "XIIGen task prompt" = $false
            }
            attached_runtime_all_required_selectors_found = $false
            attached_runtime_all_required_text_found = $false
            attached_runtime_all_required_dom_visible = $false
            typeability_proof_requested = $EnableTypeabilityProof.IsPresent
            typeability_proof_attempted = $false
            typeability_proof_status = if ($EnableTypeabilityProof.IsPresent) { "NOT_REACHED" } else { "NOT_REQUESTED" }
            typeability_proof = $null
            typeability_proof_errors = @()
            typeability_probe_attempted = $false
            typeability_probe_text_hash = if ($EnableTypeabilityProof.IsPresent) { Get-Sha256Hex -Value $TypeabilityProbeText } else { $null }
            typeability_probe_expression_sha256 = $null
            typeability_context_source = $null
            typeability_target_id = $null
            typeability_target_source = $null
            typeability_session_id = $null
            typeability_frame_id = $null
            typeability_execution_context_id = $null
            typeability_selector_context_matches_visibility_context = $false
            typeability_readback_matches = $false
            typeability_no_submit_guard = $false
            typeability_core_invoked = $false
            submit_result_proof_requested = $EnableSubmitResultProof.IsPresent
            submit_result_proof_attempted = $false
            submit_result_proof_status = if ($EnableSubmitResultProof.IsPresent) { "NOT_REACHED" } else { "NOT_REQUESTED" }
            submit_result_proof = $null
            submit_result_proof_errors = @()
            submit_probe_attempted = $false
            submit_probe_text = if ($EnableSubmitResultProof.IsPresent) { $SubmitResultProbeText } else { $null }
            submit_probe_text_sha256 = if ($EnableSubmitResultProof.IsPresent) { Get-Sha256Hex -Value $SubmitResultProbeText } else { $null }
            submit_probe_text_hash = if ($EnableSubmitResultProof.IsPresent) { Get-Sha256Hex -Value $SubmitResultProbeText } else { $null }
            submit_probe_expression_sha256 = $null
            submit_js_wait_ms = if ($EnableSubmitResultProof.IsPresent) { [Math]::Max(20000, [int] $SubmitResultWaitMs) } else { $null }
            submit_cdp_timeout_sec = if ($EnableSubmitResultProof.IsPresent) { [Math]::Max([int] $SubmitResultCdpTimeoutSec, ([int] [Math]::Ceiling(([Math]::Max(20000, [int] $SubmitResultWaitMs)) / 1000.0)) + 10) } else { $null }
            submit_proof_scope = if ($EnableSubmitResultProof.IsPresent) { "first_attached_runtime_visible_context_path" } else { $null }
            submit_retry_policy = if ($EnableSubmitResultProof.IsPresent) { "Prefer the first attached-runtime visible context path; frame/evaluated submit proof is allowed only if no attached-runtime submit path was attempted." } else { $null }
            submit_first_attached_runtime_visible_context_attempted = $false
            submit_frame_or_evaluated_retry_skipped = $false
            submit_context_source = $null
            submit_target_id = $null
            submit_target_source = $null
            submit_session_id = $null
            submit_frame_id = $null
            submit_execution_context_id = $null
            submit_selector_context_matches_visibility_context = $false
            submit_button_click_invoked = $false
            submit_button_clicked_by_probe = $false
            panel_submission_path_invoked = $false
            core_path_invoked = $false
            submit_test_only_handler_used = $false
            submit_not_test_only_path = $false
            result_rendered_in_webview = $false
            result_text = $null
            result_text_sha256 = $null
            result_status_text = $null
            result_response_ref = $null
            result_trace_ref = $null
            result_host_panel_ui_ref = $null
            result_correlation_id = $null
            submit_evidence_refs = $null
            submit_no_readiness_claim = if ($EnableSubmitResultProof.IsPresent) { $true } else { $null }
            submit_readiness_claimed = $false
            attached_dom_snapshot_attempted = $false
            attached_dom_snapshot_available = $false
            attached_dom_snapshot_error = $null
            probe_errors = @()
            frame_tree_path = $null
            execution_contexts_path = $null
            protocol_dom_snapshot_path = $null
            execution_context_count = 0
            frame_tree_frame_count = 0
            evaluation_count = 0
            frame_context_probe_attempted = $false
            frame_context_probe_method = $null
            frame_context_probe_limit = 0
            frame_context_probe_truncated = $false
            frame_context_probe_frame_count = 0
            frame_context_probe_count = 0
            frame_context_probe_required_dom_visible = $false
            frame_context_probe_errors = @()
            frame_context_probes = @()
            document_sample_files = @()
            documents_summary = @()
            iframe_probes_summary = @()
            dom_snapshot = [ordered] @{
                captured = $false
                read_only_runtime_evaluate = $true
                outer_html_sample_path = $null
                body_outer_html_sample_path = $null
                text_content_sample_path = $null
                outer_html_sample_chars = 0
                body_outer_html_sample_chars = 0
                text_content_sample_chars = 0
                outer_html_length = $null
                body_outer_html_length = $null
                text_content_length = $null
                controls_summary = @()
                markers_present = [ordered] @{
                    "xiigen-smart-skills-prompt" = $false
                    "xiigen-smart-skills-submit" = $false
                    "XIIGen Smart Skills" = $false
                    "XIIGen task prompt" = $false
                }
            }
            selectors_found = [ordered] @{
                "#xiigen-smart-skills-prompt" = $false
                "#xiigen-smart-skills-submit" = $false
            }
            texts_found = [ordered] @{
                "XIIGen Smart Skills" = $false
                "XIIGen task prompt" = $false
            }
            all_required_selectors_found = $false
            all_required_text_found = $false
            same_context_required_dom_visible = $false
            all_required_dom_visible = $false
        }

        try {
            $safeSource = ([string] (Get-ObjectPropertyValue -Object $target -Name "source")) -replace "[^A-Za-z0-9_.-]", "_"
            if ([string]::IsNullOrWhiteSpace($safeSource)) {
                $safeSource = "unknown_source"
            }
            $safeIndex = [string] (Get-ObjectPropertyValue -Object $target -Name "index")
            if ([string]::IsNullOrWhiteSpace($safeIndex)) {
                $safeIndex = [string] $queryResults.Count
            }

            $snapshotPrefix = "row12-webview-inner-frame-target-$safeSource-$safeIndex"
            $enableSubmitForThisTarget = $EnableSubmitResultProof.IsPresent -and -not $submitResultProofAlreadyAttempted
            $probe = Invoke-CdpTargetReadOnlyProbe -Target $target -BrowserWebSocketDebuggerUrl ([string] $browserWebSocketDebuggerUrl) -Expression $expression -EnableTypeabilityProof:([bool] $EnableTypeabilityProof.IsPresent) -TypeabilityProbeText $TypeabilityProbeText -EnableSubmitResultProof:([bool] $enableSubmitForThisTarget) -SubmitResultProbeText $SubmitResultProbeText -SubmitResultWaitMs $SubmitResultWaitMs -SubmitResultCdpTimeoutSec $SubmitResultCdpTimeoutSec -TimeoutSec $EndpointTimeoutSec
            $targetResult.attach_method = $probe.attach_method
            $targetResult.attached_session_id_present = $probe.attached_session_id_present -eq $true
            $targetResult.attached_session_command_path = $probe.attached_session_command_path
            $targetResult.attached_runtime_evaluate_attempted = $probe.attached_runtime_evaluate_attempted -eq $true
            $targetResult.attached_runtime_evaluate_available = $probe.attached_runtime_evaluate_available -eq $true
            $targetResult.attached_runtime_evaluate_error = $probe.attached_runtime_evaluate_error
            $targetResult.attached_runtime_selectors_found = $probe.attached_runtime_selectors_found
            $targetResult.attached_runtime_texts_found = $probe.attached_runtime_texts_found
            $targetResult.attached_runtime_all_required_selectors_found = $probe.attached_runtime_all_required_selectors_found -eq $true
            $targetResult.attached_runtime_all_required_text_found = $probe.attached_runtime_all_required_text_found -eq $true
            $targetResult.attached_runtime_all_required_dom_visible = $probe.attached_runtime_all_required_dom_visible -eq $true
            $targetResult.attached_dom_snapshot_attempted = $probe.attached_dom_snapshot_attempted -eq $true
            $targetResult.attached_dom_snapshot_available = $probe.attached_dom_snapshot_available -eq $true
            $targetResult.attached_dom_snapshot_error = $probe.attached_dom_snapshot_error
            $targetResult.probe_errors = @($probe.errors)
            $targetResult.execution_context_count = @($probe.execution_contexts).Count
            $targetResult.evaluation_count = @($probe.evaluations).Count
            $targetResult.frame_context_probe_attempted = $probe.frame_context_probe_attempted
            $targetResult.frame_context_probe_method = $probe.frame_context_probe_method
            $targetResult.frame_context_probe_limit = $probe.frame_context_probe_limit
            $targetResult.frame_context_probe_truncated = $probe.frame_context_probe_truncated
            $targetResult.frame_context_probe_frame_count = $probe.frame_context_probe_frame_count
            $targetResult.frame_context_probe_count = @($probe.frame_context_probes).Count
            $targetResult.frame_context_probe_required_dom_visible = $probe.frame_context_required_dom_visible
            $targetResult.frame_context_probe_errors = @($probe.frame_context_probe_errors)
            $targetResult.frame_context_probes = @($probe.frame_context_probes)
            $targetResult.typeability_proof_requested = $probe.typeability_proof_requested -eq $true
            $targetResult.typeability_proof_attempted = $probe.typeability_proof_attempted -eq $true
            $targetResult.typeability_proof_status = $probe.typeability_proof_status
            $targetResult.typeability_proof = $probe.typeability_proof
            $targetResult.typeability_proof_errors = @($probe.typeability_proof_errors)
            $targetResult.typeability_probe_attempted = $probe.typeability_probe_attempted -eq $true
            $targetResult.typeability_probe_text_hash = $probe.typeability_probe_text_hash
            $targetResult.typeability_probe_expression_sha256 = $probe.typeability_probe_expression_sha256
            $targetResult.typeability_context_source = $probe.typeability_context_source
            $targetResult.typeability_target_id = $probe.typeability_target_id
            $targetResult.typeability_target_source = $probe.typeability_target_source
            $targetResult.typeability_session_id = $probe.typeability_session_id
            $targetResult.typeability_frame_id = $probe.typeability_frame_id
            $targetResult.typeability_execution_context_id = $probe.typeability_execution_context_id
            $targetResult.typeability_selector_context_matches_visibility_context = $probe.typeability_selector_context_matches_visibility_context -eq $true
            $targetResult.typeability_readback_matches = $probe.typeability_readback_matches -eq $true
            $targetResult.typeability_no_submit_guard = $probe.typeability_no_submit_guard -eq $true
            $targetResult.typeability_core_invoked = $probe.typeability_core_invoked -eq $true
            $targetResult.submit_result_proof_requested = $probe.submit_result_proof_requested -eq $true
            $targetResult.submit_result_proof_attempted = $probe.submit_result_proof_attempted -eq $true
            $targetResult.submit_result_proof_status = $probe.submit_result_proof_status
            $targetResult.submit_result_proof = $probe.submit_result_proof
            $targetResult.submit_result_proof_errors = @($probe.submit_result_proof_errors)
            $targetResult.submit_probe_attempted = $probe.submit_probe_attempted -eq $true
            $targetResult.submit_probe_text = $probe.submit_probe_text
            $targetResult.submit_probe_text_sha256 = $probe.submit_probe_text_sha256
            $targetResult.submit_probe_text_hash = $probe.submit_probe_text_hash
            $targetResult.submit_probe_expression_sha256 = $probe.submit_probe_expression_sha256
            $targetResult.submit_js_wait_ms = $probe.submit_js_wait_ms
            $targetResult.submit_cdp_timeout_sec = $probe.submit_cdp_timeout_sec
            $targetResult.submit_proof_scope = $probe.submit_proof_scope
            $targetResult.submit_retry_policy = $probe.submit_retry_policy
            $targetResult.submit_first_attached_runtime_visible_context_attempted = $probe.submit_first_attached_runtime_visible_context_attempted -eq $true
            $targetResult.submit_frame_or_evaluated_retry_skipped = $probe.submit_frame_or_evaluated_retry_skipped -eq $true
            $targetResult.submit_context_source = $probe.submit_context_source
            $targetResult.submit_target_id = $probe.submit_target_id
            $targetResult.submit_target_source = $probe.submit_target_source
            $targetResult.submit_session_id = $probe.submit_session_id
            $targetResult.submit_frame_id = $probe.submit_frame_id
            $targetResult.submit_execution_context_id = $probe.submit_execution_context_id
            $targetResult.submit_selector_context_matches_visibility_context = $probe.submit_selector_context_matches_visibility_context -eq $true
            $targetResult.submit_button_click_invoked = $probe.submit_button_click_invoked -eq $true
            $targetResult.submit_button_clicked_by_probe = $probe.submit_button_clicked_by_probe -eq $true
            $targetResult.panel_submission_path_invoked = $probe.panel_submission_path_invoked -eq $true
            $targetResult.core_path_invoked = $probe.core_path_invoked -eq $true
            $targetResult.submit_test_only_handler_used = $probe.submit_test_only_handler_used -eq $true
            $targetResult.submit_not_test_only_path = $probe.submit_not_test_only_path -eq $true
            $targetResult.result_rendered_in_webview = $probe.result_rendered_in_webview -eq $true
            $targetResult.result_text = $probe.result_text
            $targetResult.result_text_sha256 = $probe.result_text_sha256
            $targetResult.result_status_text = $probe.result_status_text
            $targetResult.result_response_ref = $probe.result_response_ref
            $targetResult.result_trace_ref = $probe.result_trace_ref
            $targetResult.result_host_panel_ui_ref = $probe.result_host_panel_ui_ref
            $targetResult.result_correlation_id = $probe.result_correlation_id
            $targetResult.submit_evidence_refs = $probe.submit_evidence_refs
            $targetResult.submit_no_readiness_claim = $probe.submit_no_readiness_claim
            $targetResult.submit_readiness_claimed = $probe.submit_readiness_claimed
            if ($probe.submit_result_proof_attempted -eq $true) {
                $submitResultProofAlreadyAttempted = $true
            }
            if ($targetResult.frame_context_probe_required_dom_visible -eq $true) {
                $targetResult.same_context_required_dom_visible = $true
            }
            foreach ($selector in @("#xiigen-smart-skills-prompt", "#xiigen-smart-skills-submit")) {
                if ($probe.attached_runtime_selectors_found -and $probe.attached_runtime_selectors_found[$selector] -eq $true) {
                    $targetResult.selectors_found[$selector] = $true
                }
            }
            foreach ($textName in @("XIIGen Smart Skills", "XIIGen task prompt")) {
                if ($probe.attached_runtime_texts_found -and $probe.attached_runtime_texts_found[$textName] -eq $true) {
                    $targetResult.texts_found[$textName] = $true
                }
            }
            if ($targetResult.attached_runtime_all_required_dom_visible -eq $true) {
                $targetResult.same_context_required_dom_visible = $true
            }
            foreach ($frameContextProbe in @($probe.frame_context_probes)) {
                $frameSelectorsFound = Get-ObjectPropertyValue -Object $frameContextProbe -Name "selectors_found"
                foreach ($selector in @("#xiigen-smart-skills-prompt", "#xiigen-smart-skills-submit")) {
                    if ($frameSelectorsFound -and $frameSelectorsFound[$selector] -eq $true) {
                        $targetResult.selectors_found[$selector] = $true
                    }
                }

                $frameTextsFound = Get-ObjectPropertyValue -Object $frameContextProbe -Name "texts_found"
                foreach ($textName in @("XIIGen Smart Skills", "XIIGen task prompt")) {
                    if ($frameTextsFound -and $frameTextsFound[$textName] -eq $true) {
                        $targetResult.texts_found[$textName] = $true
                    }
                }
            }

            $frameTreePath = Join-Path $RunRoot "$snapshotPrefix.frame-tree.json"
            $executionContextsPath = Join-Path $RunRoot "$snapshotPrefix.execution-contexts.json"
            $protocolDomSnapshotPath = Join-Path $RunRoot "$snapshotPrefix.dom-snapshot-protocol.json"
            Write-JsonFile -Path $frameTreePath -Value ([ordered] @{
                schema = "xiigen.row12.cdp_frame_tree.v1"
                target = $target
                attach_method = $probe.attach_method
                attached_session_id_present = $probe.attached_session_id_present
                attached_session_command_path = $probe.attached_session_command_path
                frame_tree = $probe.frame_tree
                frame_tree_response = $probe.frame_tree_response
                errors = @($probe.errors | Where-Object { $_.stage -eq "Page.getFrameTree" })
            })
            Write-JsonFile -Path $executionContextsPath -Value ([ordered] @{
                schema = "xiigen.row12.cdp_execution_contexts.v1"
                target = $target
                attach_method = $probe.attach_method
                attached_session_id_present = $probe.attached_session_id_present
                attached_session_command_path = $probe.attached_session_command_path
                execution_context_count = @($probe.execution_contexts).Count
                execution_contexts = @($probe.execution_contexts)
                execution_context_events = @($probe.execution_context_events)
            })
            Write-JsonFile -Path $protocolDomSnapshotPath -Value ([ordered] @{
                schema = "xiigen.row12.cdp_dom_snapshot_protocol.v1"
                target = $target
                attach_method = $probe.attach_method
                attached_session_id_present = $probe.attached_session_id_present
                attached_session_command_path = $probe.attached_session_command_path
                attached_runtime_evaluate_attempted = $probe.attached_runtime_evaluate_attempted
                attached_runtime_evaluate_available = $probe.attached_runtime_evaluate_available
                attached_runtime_evaluate_error = $probe.attached_runtime_evaluate_error
                attached_runtime_selectors_found = $probe.attached_runtime_selectors_found
                attached_runtime_texts_found = $probe.attached_runtime_texts_found
                attached_runtime_all_required_selectors_found = $probe.attached_runtime_all_required_selectors_found
                attached_runtime_all_required_text_found = $probe.attached_runtime_all_required_text_found
                attached_runtime_all_required_dom_visible = $probe.attached_runtime_all_required_dom_visible
                attached_runtime_evaluate_response = $probe.attached_runtime_evaluate_response
                typeability_proof_requested = $probe.typeability_proof_requested
                typeability_proof_attempted = $probe.typeability_proof_attempted
                typeability_proof_status = $probe.typeability_proof_status
                typeability_context_source = $probe.typeability_context_source
                typeability_target_id = $probe.typeability_target_id
                typeability_target_source = $probe.typeability_target_source
                typeability_session_id = $probe.typeability_session_id
                typeability_frame_id = $probe.typeability_frame_id
                typeability_execution_context_id = $probe.typeability_execution_context_id
                typeability_selector_context_matches_visibility_context = $probe.typeability_selector_context_matches_visibility_context
                typeability_proof = $probe.typeability_proof
                typeability_proof_errors = @($probe.typeability_proof_errors)
                submit_result_proof_requested = $probe.submit_result_proof_requested
                submit_result_proof_attempted = $probe.submit_result_proof_attempted
                submit_result_proof_status = $probe.submit_result_proof_status
                submit_probe_text = if ($EnableSubmitResultProof.IsPresent) { $SubmitResultProbeText } else { $null }
                submit_probe_text_sha256 = $probe.submit_probe_text_sha256
                submit_probe_text_hash = $probe.submit_probe_text_hash
                submit_probe_expression_sha256 = $probe.submit_probe_expression_sha256
                submit_js_wait_ms = $probe.submit_js_wait_ms
                submit_cdp_timeout_sec = $probe.submit_cdp_timeout_sec
                submit_proof_scope = $probe.submit_proof_scope
                submit_retry_policy = $probe.submit_retry_policy
                submit_first_attached_runtime_visible_context_attempted = $probe.submit_first_attached_runtime_visible_context_attempted
                submit_frame_or_evaluated_retry_skipped = $probe.submit_frame_or_evaluated_retry_skipped
                submit_context_source = $probe.submit_context_source
                submit_target_id = $probe.submit_target_id
                submit_target_source = $probe.submit_target_source
                submit_session_id = $probe.submit_session_id
                submit_frame_id = $probe.submit_frame_id
                submit_execution_context_id = $probe.submit_execution_context_id
                submit_selector_context_matches_visibility_context = $probe.submit_selector_context_matches_visibility_context
                submit_button_click_invoked = $probe.submit_button_click_invoked
                submit_button_clicked_by_probe = $probe.submit_button_clicked_by_probe
                panel_submission_path_invoked = $probe.panel_submission_path_invoked
                core_path_invoked = $probe.core_path_invoked
                submit_test_only_handler_used = $probe.submit_test_only_handler_used
                submit_not_test_only_path = $probe.submit_not_test_only_path
                result_rendered_in_webview = $probe.result_rendered_in_webview
                result_text = $probe.result_text
                result_text_sha256 = $probe.result_text_sha256
                result_status_text = $probe.result_status_text
                result_response_ref = $probe.result_response_ref
                result_trace_ref = $probe.result_trace_ref
                result_host_panel_ui_ref = $probe.result_host_panel_ui_ref
                result_correlation_id = $probe.result_correlation_id
                submit_evidence_refs = $probe.submit_evidence_refs
                submit_no_readiness_claim = $probe.submit_no_readiness_claim
                submit_readiness_claimed = $probe.submit_readiness_claimed
                submit_result_proof = $probe.submit_result_proof
                submit_result_proof_errors = @($probe.submit_result_proof_errors)
                attached_dom_snapshot_attempted = $probe.attached_dom_snapshot_attempted
                attached_dom_snapshot_available = $probe.attached_dom_snapshot_available
                attached_dom_snapshot_error = $probe.attached_dom_snapshot_error
                attached_dom_snapshot_response = $probe.attached_dom_snapshot_response
                available = $probe.dom_snapshot_available
                response = $probe.dom_snapshot_response
                errors = @($probe.errors | Where-Object { $_.stage -eq "DOMSnapshot.captureSnapshot" })
            })
            $targetResult.frame_tree_path = $frameTreePath
            $targetResult.execution_contexts_path = $executionContextsPath
            $targetResult.protocol_dom_snapshot_path = $protocolDomSnapshotPath

            $frameTreeJson = if ($probe.frame_tree) { $probe.frame_tree | ConvertTo-Json -Depth 20 -Compress } else { "" }
            $targetResult.frame_tree_frame_count = ([regex]::Matches($frameTreeJson, '"id"')).Count

            foreach ($evaluation in @($probe.evaluations)) {
                $value = Get-ObjectPropertyValue -Object $evaluation -Name "value"
                $label = ([string] (Get-ObjectPropertyValue -Object $evaluation -Name "label")) -replace "[^A-Za-z0-9_.-]", "_"
                if ([string]::IsNullOrWhiteSpace($label)) {
                    $label = "evaluation"
                }
                if ($null -eq $value) {
                    continue
                }

                $targetResult.result = $value
                $documents = @(ConvertTo-NonNullArray -Value (Get-ObjectPropertyValue -Object $value -Name "documents"))
                if ($documents.Count -eq 0) {
                    $documents = @($value)
                }

                $documentIndex = 0
                foreach ($documentSample in $documents) {
                    $documentLabel = ([string] (Get-ObjectPropertyValue -Object $documentSample -Name "label")) -replace "[^A-Za-z0-9_.-]", "_"
                    if ([string]::IsNullOrWhiteSpace($documentLabel)) {
                        $documentLabel = "document-$documentIndex"
                    }
                    $samplePrefix = "$snapshotPrefix.$label.$documentLabel"
                    $outerHtmlSample = [string] (Get-ObjectPropertyValue -Object $documentSample -Name "outerHTMLSample")
                    $bodyOuterHtmlSample = [string] (Get-ObjectPropertyValue -Object $documentSample -Name "bodyOuterHTMLSample")
                    $textContentSample = [string] (Get-ObjectPropertyValue -Object $documentSample -Name "textContentSample")
                    $outerHtmlSamplePath = Join-Path $RunRoot "$samplePrefix.outerHTML.txt"
                    $bodyOuterHtmlSamplePath = Join-Path $RunRoot "$samplePrefix.bodyOuterHTML.txt"
                    $textContentSamplePath = Join-Path $RunRoot "$samplePrefix.textContent.txt"
                    Write-TextFile -Path $outerHtmlSamplePath -Value $outerHtmlSample
                    Write-TextFile -Path $bodyOuterHtmlSamplePath -Value $bodyOuterHtmlSample
                    Write-TextFile -Path $textContentSamplePath -Value $textContentSample

                    $targetResult.document_sample_files += @($outerHtmlSamplePath, $bodyOuterHtmlSamplePath, $textContentSamplePath)
                    $captured = -not [string]::IsNullOrWhiteSpace($outerHtmlSample) -or -not [string]::IsNullOrWhiteSpace($textContentSample)
                    if ($captured -and $targetResult.dom_snapshot.captured -ne $true) {
                        $targetResult.dom_snapshot.captured = $true
                        $targetResult.dom_snapshot.outer_html_sample_path = $outerHtmlSamplePath
                        $targetResult.dom_snapshot.body_outer_html_sample_path = $bodyOuterHtmlSamplePath
                        $targetResult.dom_snapshot.text_content_sample_path = $textContentSamplePath
                        $targetResult.dom_snapshot.outer_html_sample_chars = $outerHtmlSample.Length
                        $targetResult.dom_snapshot.body_outer_html_sample_chars = $bodyOuterHtmlSample.Length
                        $targetResult.dom_snapshot.text_content_sample_chars = $textContentSample.Length
                        $targetResult.dom_snapshot.outer_html_length = Get-ObjectPropertyValue -Object $documentSample -Name "outerHTMLLength"
                        $targetResult.dom_snapshot.body_outer_html_length = Get-ObjectPropertyValue -Object $documentSample -Name "bodyOuterHTMLLength"
                        $targetResult.dom_snapshot.text_content_length = Get-ObjectPropertyValue -Object $documentSample -Name "textContentLength"
                        $targetResult.dom_snapshot.controls_summary = @(ConvertTo-NonNullArray -Value (Get-ObjectPropertyValue -Object $documentSample -Name "controlsSummary"))
                    }

                    $markerResults = Get-ObjectPropertyValue -Object $documentSample -Name "markerResults"
                    foreach ($marker in @("xiigen-smart-skills-prompt", "xiigen-smart-skills-submit", "XIIGen Smart Skills", "XIIGen task prompt")) {
                        $markerFound = $false
                        if ($markerResults) {
                            $markerFound = (Get-ObjectPropertyValue -Object $markerResults -Name $marker) -eq $true
                        }
                        if ($markerFound) {
                            $targetResult.dom_snapshot.markers_present[$marker] = $true
                        }
                    }

                    $selectorResults = Get-ObjectPropertyValue -Object $documentSample -Name "selectorResults"
                    foreach ($selector in @("#xiigen-smart-skills-prompt", "#xiigen-smart-skills-submit")) {
                        $selectorResult = Get-ObjectPropertyValue -Object $selectorResults -Name $selector
                        $found = $false
                        if ($selectorResult) {
                            $found = (Get-ObjectPropertyValue -Object $selectorResult -Name "found") -eq $true
                        }
                        if ($found) {
                            $targetResult.selectors_found[$selector] = $true
                        }
                    }

                    $textResults = Get-ObjectPropertyValue -Object $documentSample -Name "textResults"
                    foreach ($textName in @("XIIGen Smart Skills", "XIIGen task prompt")) {
                        $found = $false
                        if ($textResults) {
                            $found = (Get-ObjectPropertyValue -Object $textResults -Name $textName) -eq $true
                        }
                        if ($found) {
                            $targetResult.texts_found[$textName] = $true
                        }
                    }

                    $documentAllRequiredDomVisible = (Get-ObjectPropertyValue -Object $documentSample -Name "allRequiredDomVisible") -eq $true
                    if ($documentAllRequiredDomVisible) {
                        $targetResult.same_context_required_dom_visible = $true
                    }

                    $targetResult.documents_summary += [pscustomobject] @{
                        evaluation_label = $label
                        evaluation_context_id = Get-ObjectPropertyValue -Object $evaluation -Name "contextId"
                        document_label = Get-ObjectPropertyValue -Object $documentSample -Name "label"
                        location_href = Get-ObjectPropertyValue -Object $documentSample -Name "locationHref"
                        document_title = Get-ObjectPropertyValue -Object $documentSample -Name "documentTitle"
                        ready_state = Get-ObjectPropertyValue -Object $documentSample -Name "readyState"
                        accessible = Get-ObjectPropertyValue -Object $documentSample -Name "accessible"
                        all_required_selectors_found = Get-ObjectPropertyValue -Object $documentSample -Name "allRequiredSelectorsFound"
                        all_required_text_found = Get-ObjectPropertyValue -Object $documentSample -Name "allRequiredTextFound"
                        all_required_dom_visible = $documentAllRequiredDomVisible
                        outer_html_sample_path = $outerHtmlSamplePath
                        body_outer_html_sample_path = $bodyOuterHtmlSamplePath
                        text_content_sample_path = $textContentSamplePath
                    }
                    $documentIndex += 1
                }

                $iframeProbes = @(ConvertTo-NonNullArray -Value (Get-ObjectPropertyValue -Object $value -Name "iframeProbes"))
                foreach ($iframeProbe in $iframeProbes) {
                    $targetResult.iframe_probes_summary += [pscustomobject] @{
                        evaluation_label = $label
                        evaluation_context_id = Get-ObjectPropertyValue -Object $evaluation -Name "contextId"
                        frame_meta = Get-ObjectPropertyValue -Object $iframeProbe -Name "frameMeta"
                        content_document_accessible = Get-ObjectPropertyValue -Object $iframeProbe -Name "contentDocumentAccessible"
                        error = Get-ObjectPropertyValue -Object $iframeProbe -Name "error"
                    }
                }
            }

            $targetResult.all_required_selectors_found =
                $targetResult.selectors_found["#xiigen-smart-skills-prompt"] -eq $true -and
                $targetResult.selectors_found["#xiigen-smart-skills-submit"] -eq $true
            $targetResult.all_required_text_found =
                $targetResult.texts_found["XIIGen Smart Skills"] -eq $true -and
                $targetResult.texts_found["XIIGen task prompt"] -eq $true
            $targetResult.all_required_dom_visible =
                $targetResult.same_context_required_dom_visible -eq $true
            if ($targetResult.all_required_dom_visible) {
                $targetResult.status = "INNER_FRAME_DOM_VISIBLE"
            } elseif ($targetResult.dom_snapshot.captured -eq $true) {
                $targetResult.status = "INNER_FRAME_NO_APP_DOM"
            } else {
                $targetResult.status = "INNER_FRAME_NO_APP_DOM"
            }
        } catch {
            $targetResult.error = $_.Exception.Message
            $targetResult.error_type = $_.Exception.GetType().FullName
            $targetResult.error_line = $_.InvocationInfo.ScriptLineNumber
            $targetResult.error_command = $_.InvocationInfo.Line
            $targetResult.error_position = $_.InvocationInfo.PositionMessage
            $targetResult.error_stack = $_.ScriptStackTrace
        }

        $queryResults += [pscustomobject] $targetResult
    }

    $inspection.query_results = @($queryResults)
    $inspection.attached_session_id_present = @($queryResults | Where-Object { $_.attached_session_id_present -eq $true }).Count -gt 0
    $firstAttachedCommandPath = @($queryResults | Where-Object {
        -not [string]::IsNullOrWhiteSpace([string] $_.attached_session_command_path)
    } | Select-Object -First 1)
    $inspection.attached_session_command_path = if ($firstAttachedCommandPath.Count -gt 0) {
        [string] $firstAttachedCommandPath[0].attached_session_command_path
    } else {
        $null
    }
    $inspection.attached_runtime_evaluate_attempted = @($queryResults | Where-Object { $_.attached_runtime_evaluate_attempted -eq $true }).Count -gt 0
    $inspection.attached_runtime_evaluate_available = @($queryResults | Where-Object { $_.attached_runtime_evaluate_available -eq $true }).Count -gt 0
    $firstAttachedRuntimeError = @($queryResults | Where-Object {
        -not [string]::IsNullOrWhiteSpace([string] $_.attached_runtime_evaluate_error)
    } | Select-Object -First 1)
    $inspection.attached_runtime_evaluate_error = if ($firstAttachedRuntimeError.Count -gt 0) {
        [string] $firstAttachedRuntimeError[0].attached_runtime_evaluate_error
    } else {
        $null
    }
    foreach ($selector in @("#xiigen-smart-skills-prompt", "#xiigen-smart-skills-submit")) {
        $inspection.attached_runtime_selectors_found[$selector] = @($queryResults | Where-Object {
            $_.attached_runtime_selectors_found -and $_.attached_runtime_selectors_found[$selector] -eq $true
        }).Count -gt 0
    }
    foreach ($textName in @("XIIGen Smart Skills", "XIIGen task prompt")) {
        $inspection.attached_runtime_texts_found[$textName] = @($queryResults | Where-Object {
            $_.attached_runtime_texts_found -and $_.attached_runtime_texts_found[$textName] -eq $true
        }).Count -gt 0
    }
    $inspection.attached_runtime_all_required_selectors_found =
        $inspection.attached_runtime_selectors_found["#xiigen-smart-skills-prompt"] -eq $true -and
        $inspection.attached_runtime_selectors_found["#xiigen-smart-skills-submit"] -eq $true
    $inspection.attached_runtime_all_required_text_found =
        $inspection.attached_runtime_texts_found["XIIGen Smart Skills"] -eq $true -and
        $inspection.attached_runtime_texts_found["XIIGen task prompt"] -eq $true
    $inspection.attached_runtime_all_required_dom_visible =
        @($queryResults | Where-Object { $_.attached_runtime_all_required_dom_visible -eq $true }).Count -gt 0
    $inspection.all_required_selectors_found = @($queryResults | Where-Object { $_.all_required_selectors_found -eq $true }).Count -gt 0
    $inspection.all_required_text_found = @($queryResults | Where-Object { $_.all_required_text_found -eq $true }).Count -gt 0
    $inspection.all_required_dom_visible = @($queryResults | Where-Object { $_.all_required_dom_visible -eq $true }).Count -gt 0
    $typeabilityAttempts = @($queryResults | Where-Object { $_.typeability_proof_attempted -eq $true })
    $inspection.typeability_proof_attempted = $typeabilityAttempts.Count -gt 0
    $typeabilityPasses = @($queryResults | Where-Object {
        $_.typeability_proof -and $_.typeability_proof.status -eq "TYPEABILITY_PASS"
    })
    $selectedTypeabilityProof = $null
    if ($typeabilityPasses.Count -gt 0) {
        $selectedTypeabilityProof = $typeabilityPasses[0].typeability_proof
    } elseif ($typeabilityAttempts.Count -gt 0) {
        $firstTypeabilityAttempt = @($typeabilityAttempts | Where-Object { $_.typeability_proof } | Select-Object -First 1)
        if ($firstTypeabilityAttempt.Count -gt 0) {
            $selectedTypeabilityProof = $firstTypeabilityAttempt[0].typeability_proof
        }
    }
    if ($selectedTypeabilityProof) {
        $inspection.typeability_proof = $selectedTypeabilityProof
        $inspection.typeability_proof_status = [string] $selectedTypeabilityProof.status
        $inspection.typeability_readback_value = Get-ObjectPropertyValue -Object $selectedTypeabilityProof -Name "readback_value"
        $inspection.typeability_readback_sha256 = Get-ObjectPropertyValue -Object $selectedTypeabilityProof -Name "readback_sha256"
        $inspection.typeability_readback_matches_probe = (Get-ObjectPropertyValue -Object $selectedTypeabilityProof -Name "readback_matches_probe") -eq $true
        $inspection.typeability_probe_expression_sha256 = Get-ObjectPropertyValue -Object $selectedTypeabilityProof -Name "runtime_evaluate_expression_sha256"
        $inspection.typeability_context_source = Get-ObjectPropertyValue -Object $selectedTypeabilityProof -Name "typeability_context_source"
        $inspection.typeability_target_id = Get-ObjectPropertyValue -Object $selectedTypeabilityProof -Name "typeability_target_id"
        $inspection.typeability_target_source = Get-ObjectPropertyValue -Object $selectedTypeabilityProof -Name "typeability_target_source"
        $inspection.typeability_session_id = Get-ObjectPropertyValue -Object $selectedTypeabilityProof -Name "typeability_session_id"
        $inspection.typeability_frame_id = Get-ObjectPropertyValue -Object $selectedTypeabilityProof -Name "typeability_frame_id"
        $inspection.typeability_execution_context_id = Get-ObjectPropertyValue -Object $selectedTypeabilityProof -Name "typeability_execution_context_id"
        $inspection.typeability_selector_context_matches_visibility_context =
            (Get-ObjectPropertyValue -Object $selectedTypeabilityProof -Name "typeability_selector_context_matches_visibility_context") -eq $true
        $inspection.typeability_no_submit_performed_by_probe = (Get-ObjectPropertyValue -Object $selectedTypeabilityProof -Name "no_submit_performed_by_probe") -eq $true
        $inspection.typeability_submit_button_click_invoked = (Get-ObjectPropertyValue -Object $selectedTypeabilityProof -Name "submit_button_click_invoked") -eq $true
        $inspection.typeability_submit_button_clicked_by_probe = (Get-ObjectPropertyValue -Object $selectedTypeabilityProof -Name "submit_button_clicked_by_probe") -eq $true
        $inspection.typeability_core_path_invoked_by_probe = (Get-ObjectPropertyValue -Object $selectedTypeabilityProof -Name "core_path_invoked_by_probe") -eq $true
        $inspection.typeability_panel_submission_path_invoked_by_probe = (Get-ObjectPropertyValue -Object $selectedTypeabilityProof -Name "panel_submission_path_invoked_by_probe") -eq $true
    } elseif ($EnableTypeabilityProof.IsPresent -and $inspection.typeability_proof_attempted -eq $true) {
        $inspection.typeability_proof_status = "TYPEABILITY_NEEDS_FIX"
    }
    $inspection.typeability_probe_attempted = $inspection.typeability_proof_attempted -eq $true
    $inspection.typeability_probe_text_hash = $inspection.typeability_probe_text_sha256
    $inspection.typeability_readback_matches = $inspection.typeability_readback_matches_probe -eq $true
    $inspection.typeability_core_invoked = $inspection.typeability_core_path_invoked_by_probe -eq $true
    $inspection.typeability_no_submit_guard =
        $inspection.typeability_submit_button_click_invoked -ne $true -and
        $inspection.typeability_submit_button_clicked_by_probe -ne $true -and
        $inspection.typeability_core_path_invoked_by_probe -ne $true -and
        $inspection.typeability_panel_submission_path_invoked_by_probe -ne $true
    $inspection.typeability_proof_errors = @($queryResults | ForEach-Object { @($_.typeability_proof_errors) })
    $submitResultAttempts = @($queryResults | Where-Object { $_.submit_result_proof_attempted -eq $true })
    $inspection.submit_result_proof_attempted = $submitResultAttempts.Count -gt 0
    $submitResultPasses = @($queryResults | Where-Object {
        $_.submit_result_proof -and $_.submit_result_proof.status -eq "SUBMIT_RESULT_PASS"
    })
    $selectedSubmitResultProof = $null
    if ($submitResultPasses.Count -gt 0) {
        $selectedSubmitResultProof = $submitResultPasses[0].submit_result_proof
    } elseif ($submitResultAttempts.Count -gt 0) {
        $firstSubmitResultAttempt = @($submitResultAttempts | Where-Object { $_.submit_result_proof } | Select-Object -First 1)
        if ($firstSubmitResultAttempt.Count -gt 0) {
            $selectedSubmitResultProof = $firstSubmitResultAttempt[0].submit_result_proof
        }
    }
    if ($selectedSubmitResultProof) {
        $inspection.submit_result_proof = $selectedSubmitResultProof
        $inspection.submit_result_proof_status = [string] $selectedSubmitResultProof.status
        $inspection.submit_probe_text = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "submit_probe_text"
        $inspection.submit_probe_text_sha256 = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "submit_probe_text_sha256"
        $inspection.submit_probe_text_hash = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "submit_probe_text_hash"
        $inspection.submit_probe_expression_sha256 = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "runtime_evaluate_expression_sha256"
        $inspection.submit_js_wait_ms = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "submit_js_wait_ms"
        $inspection.submit_cdp_timeout_sec = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "runtime_evaluate_timeout_sec"
        $inspection.submit_proof_scope = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "submit_proof_scope"
        $inspection.submit_retry_policy = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "submit_retry_policy"
        $inspection.submit_context_source = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "submit_context_source"
        $inspection.submit_target_id = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "submit_target_id"
        $inspection.submit_target_source = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "submit_target_source"
        $inspection.submit_session_id = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "submit_session_id"
        $inspection.submit_frame_id = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "submit_frame_id"
        $inspection.submit_execution_context_id = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "submit_execution_context_id"
        $inspection.submit_selector_context_matches_visibility_context =
            (Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "submit_selector_context_matches_visibility_context") -eq $true
        $inspection.submit_button_click_invoked = (Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "submit_button_click_invoked") -eq $true
        $inspection.submit_button_clicked_by_probe = (Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "submit_button_clicked_by_probe") -eq $true
        $inspection.panel_submission_path_invoked = (Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "panel_submission_path_invoked") -eq $true
        $inspection.core_path_invoked = (Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "core_path_invoked") -eq $true
        $inspection.submit_test_only_handler_used = (Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "submit_test_only_handler_used") -eq $true
        $inspection.submit_not_test_only_path = (Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "submit_not_test_only_path") -eq $true
        $inspection.result_rendered_in_webview = (Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "result_rendered_in_webview") -eq $true
        $inspection.result_text = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "result_text"
        $inspection.result_text_sha256 = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "result_text_sha256"
        $inspection.result_status_text = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "result_status_text"
        $inspection.result_response_ref = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "result_response_ref"
        $inspection.result_trace_ref = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "result_trace_ref"
        $inspection.result_host_panel_ui_ref = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "result_host_panel_ui_ref"
        $inspection.result_correlation_id = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "result_correlation_id"
        $inspection.submit_evidence_refs = Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "evidence_refs"
        $inspection.submit_no_readiness_claim = (Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "no_readiness_claim") -eq $true
        $inspection.submit_readiness_claimed = (Get-ObjectPropertyValue -Object $selectedSubmitResultProof -Name "readiness_claimed") -eq $true
    } elseif ($EnableSubmitResultProof.IsPresent -and $inspection.submit_result_proof_attempted -eq $true) {
        $inspection.submit_result_proof_status = "SUBMIT_RESULT_NEEDS_FIX"
    }
    $inspection.submit_probe_attempted = $inspection.submit_result_proof_attempted -eq $true
    if ($EnableSubmitResultProof.IsPresent) {
        if ([string]::IsNullOrWhiteSpace([string] $inspection.submit_probe_text)) {
            $inspection.submit_probe_text = $SubmitResultProbeText
        }
        if ([string]::IsNullOrWhiteSpace([string] $inspection.submit_probe_text_sha256)) {
            $inspection.submit_probe_text_sha256 = Get-Sha256Hex -Value $SubmitResultProbeText
        }
        if ([string]::IsNullOrWhiteSpace([string] $inspection.submit_probe_text_hash)) {
            $inspection.submit_probe_text_hash = $inspection.submit_probe_text_sha256
        }
    } else {
        $inspection.submit_probe_text_hash = $null
    }
    $inspection.submit_first_attached_runtime_visible_context_attempted =
        @($queryResults | Where-Object { $_.submit_first_attached_runtime_visible_context_attempted -eq $true }).Count -gt 0
    $inspection.submit_frame_or_evaluated_retry_skipped =
        @($queryResults | Where-Object { $_.submit_frame_or_evaluated_retry_skipped -eq $true }).Count -gt 0
    $inspection.submit_result_proof_errors = @($queryResults | ForEach-Object { @($_.submit_result_proof_errors) })
    $inspection.cdp_frame_tree_files = @($queryResults | ForEach-Object { $_.frame_tree_path } | Where-Object { -not [string]::IsNullOrWhiteSpace([string] $_) })
    $inspection.cdp_execution_context_files = @($queryResults | ForEach-Object { $_.execution_contexts_path } | Where-Object { -not [string]::IsNullOrWhiteSpace([string] $_) })
    $inspection.cdp_protocol_dom_snapshot_files = @($queryResults | ForEach-Object { $_.protocol_dom_snapshot_path } | Where-Object { -not [string]::IsNullOrWhiteSpace([string] $_) })
    $inspection.inner_document_sample_files = @($queryResults | ForEach-Object { @($_.document_sample_files) } | Where-Object { -not [string]::IsNullOrWhiteSpace([string] $_) })
    $inspection.dom_snapshot_files = @($queryResults | ForEach-Object {
        if ($_.dom_snapshot) {
            $_.dom_snapshot.outer_html_sample_path
            $_.dom_snapshot.body_outer_html_sample_path
            $_.dom_snapshot.text_content_sample_path
        }
    } | Where-Object { -not [string]::IsNullOrWhiteSpace([string] $_) })
    foreach ($marker in @("xiigen-smart-skills-prompt", "xiigen-smart-skills-submit", "XIIGen Smart Skills", "XIIGen task prompt")) {
        $inspection.expected_snapshot_markers_present[$marker] = @($queryResults | Where-Object {
            $_.dom_snapshot -and $_.dom_snapshot.markers_present[$marker] -eq $true
        }).Count -gt 0
    }
    $snapshotCaptured = @($queryResults | Where-Object { $_.dom_snapshot -and $_.dom_snapshot.captured -eq $true }).Count -gt 0
    if ($inspection.all_required_dom_visible) {
        $inspection.status = "INNER_FRAME_DOM_VISIBLE"
    } elseif ($snapshotCaptured) {
        $inspection.status = "INNER_FRAME_NO_APP_DOM"
    } else {
        $inspection.status = "NEEDS_FIX"
    }

    Write-JsonFile -Path $innerFrameSummaryPath -Value ([ordered] @{
        schema = "xiigen.row12.inner_frame_summary.v1"
        generated_utc = (Get-Date).ToUniversalTime().ToString("o")
        status = $inspection.status
        target_selection = $inspection.target_selection
        candidate_target_count = $inspection.candidate_target_count
        attached_session_id_present = $inspection.attached_session_id_present
        attached_session_command_path = $inspection.attached_session_command_path
        attached_runtime_evaluate_attempted = $inspection.attached_runtime_evaluate_attempted
        attached_runtime_evaluate_available = $inspection.attached_runtime_evaluate_available
        attached_runtime_evaluate_error = $inspection.attached_runtime_evaluate_error
        attached_runtime_selectors_found = $inspection.attached_runtime_selectors_found
        attached_runtime_texts_found = $inspection.attached_runtime_texts_found
        attached_runtime_all_required_selectors_found = $inspection.attached_runtime_all_required_selectors_found
        attached_runtime_all_required_text_found = $inspection.attached_runtime_all_required_text_found
        attached_runtime_all_required_dom_visible = $inspection.attached_runtime_all_required_dom_visible
        typeability_proof_requested = $inspection.typeability_proof_requested
        typeability_proof_attempted = $inspection.typeability_proof_attempted
        typeability_proof_status = $inspection.typeability_proof_status
        typeability_probe_text = $inspection.typeability_probe_text
        typeability_probe_text_sha256 = $inspection.typeability_probe_text_sha256
        typeability_probe_attempted = $inspection.typeability_probe_attempted
        typeability_probe_text_hash = $inspection.typeability_probe_text_hash
        typeability_probe_expression_sha256 = $inspection.typeability_probe_expression_sha256
        typeability_context_source = $inspection.typeability_context_source
        typeability_target_id = $inspection.typeability_target_id
        typeability_target_source = $inspection.typeability_target_source
        typeability_session_id = $inspection.typeability_session_id
        typeability_frame_id = $inspection.typeability_frame_id
        typeability_execution_context_id = $inspection.typeability_execution_context_id
        typeability_selector_context_matches_visibility_context = $inspection.typeability_selector_context_matches_visibility_context
        typeability_readback_value = $inspection.typeability_readback_value
        typeability_readback_sha256 = $inspection.typeability_readback_sha256
        typeability_readback_matches_probe = $inspection.typeability_readback_matches_probe
        typeability_readback_matches = $inspection.typeability_readback_matches
        typeability_no_submit_performed_by_probe = $inspection.typeability_no_submit_performed_by_probe
        typeability_submit_button_click_invoked = $inspection.typeability_submit_button_click_invoked
        typeability_submit_button_clicked_by_probe = $inspection.typeability_submit_button_clicked_by_probe
        typeability_core_path_invoked_by_probe = $inspection.typeability_core_path_invoked_by_probe
        typeability_panel_submission_path_invoked_by_probe = $inspection.typeability_panel_submission_path_invoked_by_probe
        typeability_no_submit_guard = $inspection.typeability_no_submit_guard
        typeability_core_invoked = $inspection.typeability_core_invoked
        typeability_proof = $inspection.typeability_proof
        typeability_proof_errors = @($inspection.typeability_proof_errors)
        submit_result_proof_requested = $inspection.submit_result_proof_requested
        submit_result_proof_attempted = $inspection.submit_result_proof_attempted
        submit_result_proof_status = $inspection.submit_result_proof_status
        submit_probe_text = $inspection.submit_probe_text
        submit_probe_text_sha256 = $inspection.submit_probe_text_sha256
        submit_probe_text_hash = $inspection.submit_probe_text_hash
        submit_probe_expression_sha256 = $inspection.submit_probe_expression_sha256
        submit_js_wait_ms = $inspection.submit_js_wait_ms
        submit_cdp_timeout_sec = $inspection.submit_cdp_timeout_sec
        submit_proof_scope = $inspection.submit_proof_scope
        submit_retry_policy = $inspection.submit_retry_policy
        submit_first_attached_runtime_visible_context_attempted = $inspection.submit_first_attached_runtime_visible_context_attempted
        submit_frame_or_evaluated_retry_skipped = $inspection.submit_frame_or_evaluated_retry_skipped
        submit_context_source = $inspection.submit_context_source
        submit_target_id = $inspection.submit_target_id
        submit_target_source = $inspection.submit_target_source
        submit_session_id = $inspection.submit_session_id
        submit_frame_id = $inspection.submit_frame_id
        submit_execution_context_id = $inspection.submit_execution_context_id
        submit_selector_context_matches_visibility_context = $inspection.submit_selector_context_matches_visibility_context
        submit_button_click_invoked = $inspection.submit_button_click_invoked
        submit_button_clicked_by_probe = $inspection.submit_button_clicked_by_probe
        panel_submission_path_invoked = $inspection.panel_submission_path_invoked
        core_path_invoked = $inspection.core_path_invoked
        submit_test_only_handler_used = $inspection.submit_test_only_handler_used
        submit_not_test_only_path = $inspection.submit_not_test_only_path
        result_rendered_in_webview = $inspection.result_rendered_in_webview
        result_text = $inspection.result_text
        result_text_sha256 = $inspection.result_text_sha256
        result_status_text = $inspection.result_status_text
        result_response_ref = $inspection.result_response_ref
        result_trace_ref = $inspection.result_trace_ref
        result_host_panel_ui_ref = $inspection.result_host_panel_ui_ref
        result_correlation_id = $inspection.result_correlation_id
        submit_evidence_refs = $inspection.submit_evidence_refs
        submit_no_readiness_claim = $inspection.submit_no_readiness_claim
        submit_readiness_claimed = $inspection.submit_readiness_claimed
        submit_result_proof = $inspection.submit_result_proof
        submit_result_proof_errors = @($inspection.submit_result_proof_errors)
        all_required_selectors_found = $inspection.all_required_selectors_found
        all_required_text_found = $inspection.all_required_text_found
        all_required_dom_visible = $inspection.all_required_dom_visible
        frame_tree_files = @($inspection.cdp_frame_tree_files)
        execution_context_files = @($inspection.cdp_execution_context_files)
        protocol_dom_snapshot_files = @($inspection.cdp_protocol_dom_snapshot_files)
        inner_document_sample_files = @($inspection.inner_document_sample_files)
        selector_text_requirements = $inspection.selector_text_requirements
        query_results = @($queryResults)
    })
    Write-JsonFile -Path $queryPath -Value $inspection
    return [pscustomobject] $inspection
}

$stamp = New-Stamp
$runMode = if ($SubmitResultProof.IsPresent) { "submit_result_proof" } elseif ($TypeabilityProofOnly.IsPresent) { "typeability_proof_only" } else { "visibility" }
$typeabilityProbeText = if ($TypeabilityProofOnly.IsPresent) { "row12-ui-typeability-probe-$stamp" } else { $null }
$typeabilityProbeTextSha256 = if ($TypeabilityProofOnly.IsPresent) { Get-Sha256Hex -Value $typeabilityProbeText } else { $null }
$submitResultProbeText = if ($SubmitResultProof.IsPresent) { "The user challenges whether a reported pass is real when the evidence only proves a boundary marker and does not prove the broader readiness claim." } else { $null }
$submitResultProbeTextSha256 = if ($SubmitResultProof.IsPresent) { Get-Sha256Hex -Value $submitResultProbeText } else { $null }
$submitResultJsWaitMs = if ($SubmitResultProof.IsPresent) { [Math]::Max(20000, [int] $SubmitResultWaitMs) } else { [int] $SubmitResultWaitMs }
$submitResultCdpTimeoutSec = if ($SubmitResultProof.IsPresent) { [Math]::Max([int] $SubmitResultCdpTimeoutSec, ([int] [Math]::Ceiling($submitResultJsWaitMs / 1000.0)) + 10) } else { [int] $SubmitResultCdpTimeoutSec }
$runRootPrefix = if ($SubmitResultProof.IsPresent) { "runner-submit-result-proof" } elseif ($TypeabilityProofOnly.IsPresent) { "runner-typeability-proof" } else { "runner-webview-inner-frame" }
$reportStem = if ($SubmitResultProof.IsPresent) { "row12-cursor-panel-submit-result-proof-report" } elseif ($TypeabilityProofOnly.IsPresent) { "row12-cursor-panel-typeability-proof-report" } else { "row12-cursor-panel-visibility-report" }
$runRoot = Join-Path $EvidenceBase "$runRootPrefix-$stamp"
$runtimeEvidenceRoot = Join-Path $runRoot "runtime-evidence"
$hostReportPath = Join-Path $runRoot "row12-extension-host-command-report.json"
$readyMarkerPath = Join-Path $runRoot "row12-panel-ready-marker.json"
$extensionTestPath = Join-Path $runRoot "row12-open-panel.extension-test.js"
$stdoutLog = Join-Path $runRoot "cursor-panel-runner.stdout.log"
$stderrLog = Join-Path $runRoot "cursor-panel-runner.stderr.log"
$screenshotPath = Join-Path $runRoot "row12-cursor-panel.png"
$reportPath = Join-Path $runRoot "$reportStem.json"
$markdownPath = Join-Path $runRoot "$reportStem.md"
$shortId = ([guid]::NewGuid().ToString("N")).Substring(0, 8)
$shortRuntimeRoot = Join-Path $env:TEMP "xiigen-row12-$shortId"
$userDataDir = Join-Path $shortRuntimeRoot "ud"
$extensionsDir = Join-Path $shortRuntimeRoot "ext"
$runtimeCopyRoot = Join-Path $runRoot "short-temp-runtime-copy"
$userDataLogsCopy = Join-Path $runtimeCopyRoot "cursor-user-data-logs"
$extensionsDirCopy = Join-Path $runtimeCopyRoot "cursor-extensions-dir"
$globalStorageRuntimeEvidenceCopy = Join-Path $runtimeCopyRoot "cursor-global-storage-runtime-evidence"
$runtimePathsPath = Join-Path $runRoot "row12-short-temp-runtime-paths.json"
$remoteDebuggingPort = Get-FreeLocalTcpPort
$effectivePanelHoldMs = [int] $PanelHoldMs
if ($effectivePanelHoldMs -le 5000) {
    throw "PanelHoldMs must be greater than 5000ms so Row12 CDP inspection can run while Cursor is held open; received $effectivePanelHoldMs."
}

Ensure-Directory -Path $runRoot
Ensure-Directory -Path $runtimeEvidenceRoot
Ensure-Directory -Path $shortRuntimeRoot
Ensure-Directory -Path $userDataDir
Ensure-Directory -Path $extensionsDir

$cursor = Resolve-CursorExecutable -Preferred $CursorExe
New-ExtensionHostTestFile -Path $extensionTestPath -HoldMs $effectivePanelHoldMs

$beforeHandles = @(Get-CursorWindows | ForEach-Object { $_.HandleHex })
$launchStart = Get-Date
$arguments = @(
    "--extensionDevelopmentPath", $ExtensionRoot,
    "--extensionTestsPath", $extensionTestPath,
    "--user-data-dir", $userDataDir,
    "--extensions-dir", $extensionsDir,
    "--disable-workspace-trust",
    "--skip-welcome",
    "--skip-release-notes",
    "--skip-onboarding",
    "--disable-telemetry",
    "--disable-updates",
    "--force-renderer-accessibility",
    "--remote-debugging-address=127.0.0.1",
    "--remote-debugging-port=$remoteDebuggingPort",
    "--log", "trace",
    "--new-window",
    $ExtensionRoot
)
$argumentLine = Join-NativeArgumentList -Arguments $arguments

$startInfo = [ordered] @{
    cursor_exe = $cursor
    extension_root = $ExtensionRoot
    evidence_root = $runRoot
    runtime_evidence_root = $runtimeEvidenceRoot
    short_runtime_root = $shortRuntimeRoot
    user_data_dir = $userDataDir
    extensions_dir = $extensionsDir
    copied_runtime_diagnostics_path = $runtimePathsPath
    extension_test_path = $extensionTestPath
    run_mode = $runMode
    typeability_proof_only = $TypeabilityProofOnly.IsPresent
    typeability_probe_text = $typeabilityProbeText
    typeability_probe_text_sha256 = $typeabilityProbeTextSha256
    submit_result_proof = $SubmitResultProof.IsPresent
    submit_probe_text = $submitResultProbeText
    submit_probe_text_sha256 = $submitResultProbeTextSha256
    submit_js_wait_ms = if ($SubmitResultProof.IsPresent) { $submitResultJsWaitMs } else { $null }
    submit_cdp_timeout_sec = if ($SubmitResultProof.IsPresent) { $submitResultCdpTimeoutSec } else { $null }
    submit_runtime_evidence_env = if ($SubmitResultProof.IsPresent) { "PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR=$runtimeEvidenceRoot" } else { $null }
    submit_proof_scope = if ($SubmitResultProof.IsPresent) { "first_attached_runtime_visible_context_path" } else { $null }
    submit_retry_policy = if ($SubmitResultProof.IsPresent) { "Prefer the first attached-runtime visible context path; frame/evaluated submit proof is allowed only if no attached-runtime submit path was attempted." } else { $null }
    remote_debugging_address = "127.0.0.1"
    remote_debugging_port = $remoteDebuggingPort
    cdp_targets_endpoint = "http://127.0.0.1:$remoteDebuggingPort/json"
    cdp_browser_version_endpoint = "http://127.0.0.1:$remoteDebuggingPort/json/version"
    panel_hold_ms_requested = $PanelHoldMs
    panel_hold_ms_effective = $effectivePanelHoldMs
    panel_hold_ms_source = "-PanelHoldMs runner parameter propagated through ROW12_PANEL_HOLD_MS and generated extension test"
    panel_hold_fallback_5000_used = $false
    command_line = ('"{0}" {1}' -f $cursor, $argumentLine)
    no_git_action_run = $true
}

$process = $null
$hostMarker = $null
$hostReport = $null
$window = $null
$screenshot = $null
$uia = $null
$cdpInspection = $null
$cdpInspectionTiming = "not_attempted"
$status = "NEEDS_FIX"
$errors = New-Object System.Collections.Generic.List[object]

try {
    Invoke-WithTemporaryEnvironment -Values @{
        ROW12_PANEL_HOST_REPORT = $hostReportPath
        ROW12_PANEL_READY_MARKER = $readyMarkerPath
        ROW12_PANEL_HOLD_MS = $effectivePanelHoldMs
        PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR = $runtimeEvidenceRoot
    } -Action {
        $script:process = Start-Process -FilePath $cursor `
            -ArgumentList $argumentLine `
            -PassThru `
            -RedirectStandardOutput $stdoutLog `
            -RedirectStandardError $stderrLog `
            -WindowStyle Normal

        $deadline = (Get-Date).AddSeconds($HostReadyTimeoutSec)
        while ((Get-Date) -lt $deadline -and -not (Test-Path -LiteralPath $readyMarkerPath)) {
            Start-Sleep -Milliseconds 500
        }
    }

    if (Test-Path -LiteralPath $readyMarkerPath) {
        $hostMarker = Get-Content -Raw -LiteralPath $readyMarkerPath | ConvertFrom-Json
    } else {
        $errors.Add([pscustomobject] @{
            stage = "wait_for_extension_host_marker"
            message = "Timed out before the Extension Host test wrote row12-panel-ready-marker.json."
        })
    }

    if (Test-Path -LiteralPath $hostReportPath) {
        $hostReport = Get-Content -Raw -LiteralPath $hostReportPath | ConvertFrom-Json
    }

    $cdpInspectionTiming = "before_process_close"
    $cdpInspection = Invoke-Row12CdpWebViewDomInspection -Port $remoteDebuggingPort -RunRoot $runRoot -InspectionTiming $cdpInspectionTiming -EnableTypeabilityProof:([bool] $TypeabilityProofOnly.IsPresent) -TypeabilityProbeText $typeabilityProbeText -EnableSubmitResultProof:([bool] $SubmitResultProof.IsPresent) -SubmitResultProbeText $submitResultProbeText -SubmitResultWaitMs $submitResultJsWaitMs -SubmitResultCdpTimeoutSec $submitResultCdpTimeoutSec

    $window = Select-CursorDevHostWindow -BeforeHandles $beforeHandles -LaunchStart $launchStart -TimeoutSec $WindowReadyTimeoutSec
    if ($window) {
        Focus-Window -Handle $window.Handle
        Start-Sleep -Seconds $PanelSettleSec
        $screenshot = Capture-WindowPng -Handle $window.Handle -Path $screenshotPath
        $uia = Read-UiaSnapshot -Handle $window.Handle
    } else {
        $errors.Add([pscustomobject] @{
            stage = "find_cursor_window"
            message = "Could not find a visible Cursor Extension Development Host window."
        })
    }

} catch {
    $errors.Add([pscustomobject] @{
        stage = "runner"
        message = $_.Exception.Message
        stack = $_.ScriptStackTrace
    })
} finally {
    if (-not $KeepCursorOpen) {
        Stop-LaunchedCursorProcesses -LaunchStart $launchStart
    }
}

if ($null -eq $cdpInspection) {
    $cdpTargetsPath = Join-Path $runRoot "row12-cdp-targets.json"
    $cdpQueryPath = Join-Path $runRoot "row12-cdp-webview-dom-query.json"
    $cdpBrowserVersionPath = Join-Path $runRoot "row12-cdp-browser-version.json"
    $cdpBrowserTargetsPath = Join-Path $runRoot "row12-browser-cdp-target-gettargets.json"
    $cdpInnerFrameSummaryPath = Join-Path $runRoot "row12-cdp-webview-inner-frame-summary.json"
    $cdpInspection = [pscustomobject] [ordered] @{
        schema = "xiigen.row12.cursor_panel.cdp_webview_dom.v1"
        attempted = $false
        generated_utc = (Get-Date).ToUniversalTime().ToString("o")
        inspection_timing = $cdpInspectionTiming
        port = $remoteDebuggingPort
        endpoint = "http://127.0.0.1:$remoteDebuggingPort/json"
        version_endpoint = "http://127.0.0.1:$remoteDebuggingPort/json/version"
        status = "BROWSER_CDP_UNAVAILABLE"
        targets_path = $cdpTargetsPath
        query_path = $cdpQueryPath
        browser_version_path = $cdpBrowserVersionPath
        browser_target_gettargets_path = $cdpBrowserTargetsPath
        inner_frame_summary_path = $cdpInnerFrameSummaryPath
        browser_version = $null
        browser_version_available = $false
        browser_websocket_debugger_url = $null
        browser_websocket_debugger_url_present = $false
        browser_target_gettargets_attempted = $false
        browser_target_gettargets_available = $false
        browser_target_gettargets_error = $null
        browser_targets = @()
        browser_target_count = 0
        browser_candidate_targets = @()
        browser_candidate_target_count = 0
        targets = @()
        target_count = 0
        merged_target_count = 0
        json_targets = @()
        json_target_count = 0
        candidate_targets = @()
        candidate_target_count = 0
        query_results = @()
        typeability_proof_requested = $TypeabilityProofOnly.IsPresent
        typeability_proof_attempted = $false
        typeability_proof_status = if ($TypeabilityProofOnly.IsPresent) { "NOT_REACHED" } else { "NOT_REQUESTED" }
        typeability_proof = $null
        typeability_probe_text = $typeabilityProbeText
        typeability_probe_text_sha256 = $typeabilityProbeTextSha256
        typeability_probe_attempted = $false
        typeability_probe_text_hash = $typeabilityProbeTextSha256
        typeability_probe_expression_sha256 = $null
        typeability_readback_value = $null
        typeability_readback_sha256 = $null
        typeability_readback_matches_probe = $false
        typeability_readback_matches = $false
        typeability_no_submit_performed_by_probe = $false
        typeability_submit_button_click_invoked = $false
        typeability_submit_button_clicked_by_probe = $false
        typeability_core_path_invoked_by_probe = $false
        typeability_panel_submission_path_invoked_by_probe = $false
        typeability_no_submit_guard = $false
        typeability_core_invoked = $false
        typeability_proof_errors = @()
        submit_result_proof_requested = $SubmitResultProof.IsPresent
        submit_result_proof_attempted = $false
        submit_result_proof_status = if ($SubmitResultProof.IsPresent) { "NOT_REACHED" } else { "NOT_REQUESTED" }
        submit_result_proof = $null
        submit_result_proof_errors = @()
        submit_probe_attempted = $false
        submit_probe_text = $submitResultProbeText
        submit_probe_text_sha256 = $submitResultProbeTextSha256
        submit_probe_text_hash = $submitResultProbeTextSha256
        submit_probe_expression_sha256 = $null
        submit_js_wait_ms = if ($SubmitResultProof.IsPresent) { $submitResultJsWaitMs } else { $null }
        submit_cdp_timeout_sec = if ($SubmitResultProof.IsPresent) { $submitResultCdpTimeoutSec } else { $null }
        submit_proof_scope = if ($SubmitResultProof.IsPresent) { "first_attached_runtime_visible_context_path" } else { $null }
        submit_retry_policy = if ($SubmitResultProof.IsPresent) { "Prefer the first attached-runtime visible context path; frame/evaluated submit proof is allowed only if no attached-runtime submit path was attempted." } else { $null }
        submit_first_attached_runtime_visible_context_attempted = $false
        submit_frame_or_evaluated_retry_skipped = $false
        submit_context_source = $null
        submit_target_id = $null
        submit_target_source = $null
        submit_session_id = $null
        submit_frame_id = $null
        submit_execution_context_id = $null
        submit_selector_context_matches_visibility_context = $false
        submit_button_click_invoked = $false
        submit_button_clicked_by_probe = $false
        panel_submission_path_invoked = $false
        core_path_invoked = $false
        submit_test_only_handler_used = $false
        submit_not_test_only_path = $false
        result_rendered_in_webview = $false
        result_text = $null
        result_text_sha256 = $null
        result_status_text = $null
        result_response_ref = $null
        result_trace_ref = $null
        result_host_panel_ui_ref = $null
        result_correlation_id = $null
        submit_evidence_refs = $null
        submit_no_readiness_claim = if ($SubmitResultProof.IsPresent) { $true } else { $null }
        submit_readiness_claimed = $false
        attached_session_id_present = $false
        attached_session_command_path = $null
        attached_runtime_evaluate_attempted = $false
        attached_runtime_evaluate_available = $false
        attached_runtime_evaluate_error = $null
        attached_runtime_selectors_found = [ordered] @{
            "#xiigen-smart-skills-prompt" = $false
            "#xiigen-smart-skills-submit" = $false
        }
        attached_runtime_texts_found = [ordered] @{
            "XIIGen Smart Skills" = $false
            "XIIGen task prompt" = $false
        }
        attached_runtime_all_required_selectors_found = $false
        attached_runtime_all_required_text_found = $false
        attached_runtime_all_required_dom_visible = $false
        cdp_frame_tree_files = @()
        cdp_execution_context_files = @()
        cdp_protocol_dom_snapshot_files = @()
        inner_document_sample_files = @()
        all_required_selectors_found = $false
        all_required_text_found = $false
        all_required_dom_visible = $false
        error = "CDP inspection was not reached before the runner left the launch/capture block."
    }
    Write-JsonFile -Path $cdpTargetsPath -Value $cdpInspection
    Write-JsonFile -Path $cdpQueryPath -Value $cdpInspection
    Write-JsonFile -Path $cdpBrowserVersionPath -Value $cdpInspection
    Write-JsonFile -Path $cdpBrowserTargetsPath -Value $cdpInspection
    Write-JsonFile -Path $cdpInnerFrameSummaryPath -Value $cdpInspection
}

$runtimeCopyReport = [ordered] @{
    temp_root = $shortRuntimeRoot
    user_data_dir = $userDataDir
    extensions_dir = $extensionsDir
    evidence_copy_root = $runtimeCopyRoot
    runtime_evidence_root = $runtimeEvidenceRoot
    global_storage_runtime_evidence_copy = $globalStorageRuntimeEvidenceCopy
    user_data_logs_source = (Join-Path $userDataDir "logs")
    user_data_logs_copy = $userDataLogsCopy
    extensions_dir_copy = $extensionsDirCopy
    user_data_logs_copied = @()
    extensions_dir_copied = @()
    global_storage_runtime_evidence_copied = $null
}
$runtimeCopyReport.user_data_logs_copied = @(Copy-DirectoryContentsIfExists -Source $runtimeCopyReport.user_data_logs_source -Destination $userDataLogsCopy)
$runtimeCopyReport.extensions_dir_copied = @(Copy-DirectoryContentsIfExists -Source $extensionsDir -Destination $extensionsDirCopy)
$runtimeCopyReport.global_storage_runtime_evidence_copied = Copy-Row12GlobalStorageRuntimeEvidence -UserDataDir $userDataDir -Destination $globalStorageRuntimeEvidenceCopy
$runtimeSubmitEvidence = if ($SubmitResultProof.IsPresent) {
    Get-Row12RuntimeEvidenceSummary -Roots @($runtimeEvidenceRoot, $globalStorageRuntimeEvidenceCopy)
} else {
    $null
}
$runtimeCopyReport.submit_runtime_artifact_evidence = $runtimeSubmitEvidence
Write-JsonFile -Path $runtimePathsPath -Value $runtimeCopyReport

$uiaNames = @()
if ($uia -and $uia.names) {
    foreach ($name in $uia.names) {
        if ($null -ne $name -and -not [string]::IsNullOrWhiteSpace([string] $name)) {
            $uiaNames += [string] $name
        }
    }
}
$sampledUiaNames = @($uiaNames | Select-Object -First 80)

$uiaEditControls = @()
if ($uia -and $uia.edit_controls) {
    foreach ($edit in $uia.edit_controls) {
        $uiaEditControls += [pscustomobject] @{
            Name = if ($null -ne $edit.Name) { [string] $edit.Name } else { "" }
            AutomationId = if ($null -ne $edit.AutomationId) { [string] $edit.AutomationId } else { "" }
            ClassName = if ($null -ne $edit.ClassName) { [string] $edit.ClassName } else { "" }
        }
    }
}
$screenshotCaptured = Test-Path -LiteralPath $screenshotPath
$windowReport = if ($window) {
    [pscustomobject] @{
        HandleHex = [string] $window.HandleHex
        ProcessId = [int] $window.ProcessId
        ProcessName = [string] $window.ProcessName
        Title = [string] $window.Title
        StartTime = if ($window.StartTime) { ([datetime] $window.StartTime).ToString("o") } else { $null }
    }
} else {
    $null
}
$screenshotReport = if ($screenshot) {
    [pscustomobject] @{
        Path = [string] $screenshot.Path
        Left = [int] $screenshot.Left
        Top = [int] $screenshot.Top
        Width = [int] $screenshot.Width
        Height = [int] $screenshot.Height
    }
} else {
    $null
}
$screenshotRef = $null
if ($screenshotCaptured) {
    $screenshotRef = [string] $screenshotPath
}
$uiaAttempted = $false
$uiaAvailable = $false
$uiaError = "UIA snapshot not attempted because no window was found."
if ($null -ne $uia) {
    $attemptedProp = $uia.PSObject.Properties["attempted"]
    $availableProp = $uia.PSObject.Properties["available"]
    $errorProp = $uia.PSObject.Properties["error"]
    $uiaAttempted = $null -ne $attemptedProp -and $attemptedProp.Value -eq $true
    $uiaAvailable = $null -ne $availableProp -and $availableProp.Value -eq $true
    $uiaError = if ($null -ne $errorProp -and $null -ne $errorProp.Value) { [string] $errorProp.Value } else { $null }
}
$uiaTextLines = @()
foreach ($name in $uiaNames) {
    if (-not [string]::IsNullOrWhiteSpace($name)) {
        $uiaTextLines += $name
    }
}
foreach ($edit in $uiaEditControls) {
    $editText = @($edit.Name, $edit.AutomationId, $edit.ClassName) -join " "
    if (-not [string]::IsNullOrWhiteSpace($editText)) {
        $uiaTextLines += $editText
    }
}
$uiaExtractedText = (@($uiaTextLines | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique) -join "`n")
$visualTextExtractionMethod = "unscoped_whole_window_uia_accessibility_snapshot_no_ocr"
$visualTextExtractionAvailable = $uiaAvailable -and -not [string]::IsNullOrWhiteSpace($uiaExtractedText)
$windowTitleContainsXiigen = $false
if ($windowReport -and $windowReport.Title) {
    $windowTitleContainsXiigen = [string] $windowReport.Title -match "XIIGen Smart Skills"
}

$expectedVisibleLabels = @(
    "XIIGen Smart Skills",
    "XIIGen task prompt",
    "Enter the task to route through llm_mvp_core",
    "Submit to llm_mvp_core",
    "textarea#xiigen-smart-skills-prompt"
)
$uiaUnscopedPanelTitleTextPresent = $visualTextExtractionAvailable -and $uiaExtractedText -match "(?i)\bXIIGen Smart Skills\b"
$uiaUnscopedPromptLabelTextPresent = $visualTextExtractionAvailable -and (
    $uiaExtractedText -match "(?i)XIIGen task prompt" -or
    $uiaExtractedText -match "(?i)Enter the task to route through llm_mvp_core"
)
$uiaUnscopedSubmitToCoreTextPresent = $visualTextExtractionAvailable -and $uiaExtractedText -match "(?i)Submit to llm_mvp_core"
$uiaUnscopedXiigenTextareaTextPresent = @($uiaEditControls | Where-Object {
    (($_.Name, $_.AutomationId, $_.ClassName) -join " ") -match "(?i)(xiigen|llm_mvp_core|task prompt)"
}).Count -gt 0
$anyEditControlVisible = $uiaEditControls.Count -gt 0
$uiaUnscopedXiigenTextPresent = $uiaUnscopedPanelTitleTextPresent -or
    $uiaUnscopedPromptLabelTextPresent -or
    $uiaUnscopedSubmitToCoreTextPresent -or
    $uiaUnscopedXiigenTextareaTextPresent
$uiaUnscopedSecondaryPanelTextPresent = $uiaUnscopedPromptLabelTextPresent -or
    $uiaUnscopedXiigenTextareaTextPresent -or
    $uiaUnscopedSubmitToCoreTextPresent
$cdpStatus = "BROWSER_CDP_UNAVAILABLE"
if ($cdpInspection -and $cdpInspection.status) {
    $cdpStatus = [string] $cdpInspection.status
}
$submitRuntimeArtifactEvidenceUsable = $false
if ($SubmitResultProof.IsPresent -and $runtimeSubmitEvidence) {
    $submitRuntimeArtifactEvidenceUsable = $runtimeSubmitEvidence.classification_can_use -eq $true
    $cdpInspection | Add-Member -NotePropertyName submit_runtime_artifact_evidence -NotePropertyValue $runtimeSubmitEvidence -Force
    $cdpInspection | Add-Member -NotePropertyName submit_result_artifact_evidence_used_for_classification -NotePropertyValue $submitRuntimeArtifactEvidenceUsable -Force

    if ($submitRuntimeArtifactEvidenceUsable) {
        $cdpInspection.submit_result_proof_attempted = $true
        $cdpInspection.submit_probe_attempted = $true
        $cdpInspection.submit_button_click_invoked = $true
        $cdpInspection.submit_button_clicked_by_probe = $true
        $cdpInspection.panel_submission_path_invoked = $true
        $cdpInspection.core_path_invoked = $true
        $cdpInspection.submit_test_only_handler_used = $false
        $cdpInspection.submit_not_test_only_path = $true
        $cdpInspection.submit_no_readiness_claim = $true
        $cdpInspection.submit_readiness_claimed = $false
        $cdpInspection.result_response_ref = $runtimeSubmitEvidence.response_ref
        $cdpInspection.result_trace_ref = $runtimeSubmitEvidence.host_trace_ref
        $cdpInspection.result_host_panel_ui_ref = $runtimeSubmitEvidence.panel_ui_ref
        $cdpInspection.submit_evidence_refs = $runtimeSubmitEvidence.evidence_refs
        if ($cdpInspection.submit_result_proof_status -ne "SUBMIT_RESULT_PASS") {
            $cdpInspection.submit_result_proof_status = "SUBMIT_RESULT_PASS"
        }
    }
}
$panelContentLabelsConfirmed = $cdpStatus -eq "INNER_FRAME_DOM_VISIBLE"
$panelContentConfirmationSource = if ($panelContentLabelsConfirmed) {
    "read-only CDP inner frame/document inspection confirmed required selectors and text in a candidate target context"
} else {
    "none: CDP inner frame/document inspection did not confirm the required selectors and text. Screenshot and whole-window UIA remain diagnostics only."
}
$panelContentConfirmationMethod = if ($panelContentLabelsConfirmed) { "cdp_inner_frame_document" } else { "none" }
$panelContentProofRequired = "read-only CDP frame/context/document inspection confirming #xiigen-smart-skills-prompt, #xiigen-smart-skills-submit, XIIGen Smart Skills, and XIIGen task prompt"

$genericIndicatorDefinitions = @(
    [pscustomobject] @{ marker = "Plan"; pattern = "(?im)(^|[\s\r\n])Plan($|[\s\r\n])" },
    [pscustomobject] @{ marker = "Build"; pattern = "(?im)(^|[\s\r\n])Build($|[\s\r\n])" },
    [pscustomobject] @{ marker = "/ for skills"; pattern = "(?i)/\s*for\s*skills" },
    [pscustomobject] @{ marker = "@ for context"; pattern = "(?i)@\s*for\s*context" },
    [pscustomobject] @{ marker = "Log in"; pattern = "(?i)\bLog in\b" }
)
$detectedGenericIndicators = @()
if ($visualTextExtractionAvailable) {
    foreach ($definition in $genericIndicatorDefinitions) {
        if ($uiaExtractedText -match $definition.pattern) {
            $detectedGenericIndicators += [pscustomobject] @{
                marker = $definition.marker
                source = "uia_text"
            }
        }
    }
}
$genericLabelsTextConfirmed = $detectedGenericIndicators.Count -gt 0
$genericDetectionInconclusive = -not $uiaUnscopedXiigenTextPresent -and -not $genericLabelsTextConfirmed
$genericDetectionReason = "Unscoped whole-window UIA text contained XIIGen markers and no configured generic Cursor labels, but this is diagnostic only and does not confirm panel content."
if ($genericLabelsTextConfirmed) {
    $genericDetectionReason = "UIA text confirmed one or more configured generic Cursor labels."
} elseif ($genericDetectionInconclusive) {
    $genericDetectionReason = "No XIIGen labels and no generic Cursor labels were text-confirmed; generic detection is inconclusive and screenshot/manual review is required."
}
$visualTextExtractionReliable = $false
$genericCursorChatOrLoginDetected = if ($genericLabelsTextConfirmed) {
    $true
} elseif ($genericDetectionInconclusive) {
    $null
} else {
    $false
}
$visualTextConfirmed = $panelContentLabelsConfirmed

$visualFailMarkers = @()
if (-not $screenshotCaptured) {
    $visualFailMarkers += "screenshot_not_captured"
}
if (-not $visualTextExtractionAvailable) {
    $visualFailMarkers += "uia_unscoped_text_present=false: OCR is unavailable and UIA did not expose any usable whole-window text"
}
if ($genericDetectionInconclusive) {
    $visualFailMarkers += "generic_detection_inconclusive=true: no XIIGen labels and no generic Cursor labels were text-confirmed"
}
if (-not $panelContentLabelsConfirmed) {
    $visualFailMarkers += "panel_content_labels_confirmed=false: no screenshot OCR, DOM, or scoped UIA source confirmed visible XIIGen panel content"
}
if ($uiaUnscopedXiigenTextPresent -and -not $panelContentLabelsConfirmed) {
    $visualFailMarkers += "uia_unscoped_xiigen_text_not_accepted: whole-window UIA text may come from title, tabs, menus, or hidden/off-panel controls"
}
if ($genericCursorChatOrLoginDetected -eq $true) {
    $visualFailMarkers += "generic_cursor_chat_or_login_detected: {0}" -f (($detectedGenericIndicators | ForEach-Object { $_.marker }) -join ", ")
}
if ($windowTitleContainsXiigen -and -not $panelContentLabelsConfirmed) {
    $visualFailMarkers += "title_only_not_accepted: window title contains XIIGen but does not prove visible panel content labels"
}
$noSubmitPerformed = $false
$corePathInvoked = $false
$panelSubmissionPathInvoked = $false
$testOnlyHandlerUsed = $false
if ($hostReport) {
    $corePathInvoked = $hostReport.core_path_invoked -eq $true
    $panelSubmissionPathInvoked = $hostReport.panel_submission_path_invoked -eq $true
    $testOnlyHandlerUsed = $hostReport.test_only_handler_used -eq $true
    $noSubmitPerformed = ($hostReport.no_submit_performed -eq $true) -and
        (-not $corePathInvoked) -and
        (-not $panelSubmissionPathInvoked)
}
$typeabilityNoSubmitCoreFlagsRemainFalse =
    (-not $corePathInvoked) -and
    (-not $panelSubmissionPathInvoked) -and
    (-not $testOnlyHandlerUsed) -and
    ($cdpInspection.typeability_submit_button_click_invoked -ne $true) -and
    ($cdpInspection.typeability_submit_button_clicked_by_probe -ne $true) -and
    ($cdpInspection.typeability_core_path_invoked_by_probe -ne $true) -and
    ($cdpInspection.typeability_panel_submission_path_invoked_by_probe -ne $true)

if ($SubmitResultProof.IsPresent) {
    $submitResultEvidenceObserved = ($cdpInspection.result_rendered_in_webview -eq $true) -or $submitRuntimeArtifactEvidenceUsable
    $submitPanelSubmissionObserved = ($cdpInspection.panel_submission_path_invoked -eq $true) -or $submitRuntimeArtifactEvidenceUsable
    $submitCorePathObserved = ($cdpInspection.core_path_invoked -eq $true) -or $submitRuntimeArtifactEvidenceUsable
    $submitClickObserved = ($cdpInspection.submit_button_click_invoked -eq $true) -or $submitRuntimeArtifactEvidenceUsable
    if ($cdpStatus -ne "INNER_FRAME_DOM_VISIBLE" -and -not $submitRuntimeArtifactEvidenceUsable) {
        $status = "SUBMIT_RESULT_NEEDS_FIX"
    } elseif (($cdpInspection.submit_result_proof_status -eq "SUBMIT_RESULT_PASS" -or $submitRuntimeArtifactEvidenceUsable) -and
        $submitClickObserved -eq $true -and
        $submitPanelSubmissionObserved -eq $true -and
        $submitCorePathObserved -eq $true -and
        $submitResultEvidenceObserved -eq $true -and
        $cdpInspection.submit_test_only_handler_used -ne $true -and
        $cdpInspection.submit_no_readiness_claim -eq $true -and
        $cdpInspection.submit_readiness_claimed -ne $true) {
        $status = "SUBMIT_RESULT_PASS"
    } else {
        $status = "SUBMIT_RESULT_NEEDS_FIX"
    }
} elseif ($TypeabilityProofOnly.IsPresent) {
    if ($cdpStatus -ne "INNER_FRAME_DOM_VISIBLE") {
        $status = "RUNNER_STILL_BROKEN"
    } elseif ($cdpInspection.typeability_proof_status -eq "TYPEABILITY_PASS" -and
        $cdpInspection.typeability_readback_matches_probe -eq $true -and
        $typeabilityNoSubmitCoreFlagsRemainFalse -eq $true) {
        $status = "TYPEABILITY_PASS"
    } else {
        $status = "TYPEABILITY_NEEDS_FIX"
    }
} else {
    if ($cdpStatus -eq "INNER_FRAME_DOM_VISIBLE") {
        $status = "INNER_FRAME_DOM_VISIBLE"
    } elseif ($cdpStatus -eq "INNER_FRAME_NO_APP_DOM") {
        $status = "INNER_FRAME_NO_APP_DOM"
    } else {
        $status = "NEEDS_FIX"
    }
}

$panelContentCheckStatus = "FAIL_REQUIRES_SCOPED_PANEL_CONTENT_PROOF"
if ($panelContentLabelsConfirmed) {
    $panelContentCheckStatus = "PASS"
}
$noSubmitCheckStatus = "FAIL"
if ($noSubmitPerformed) {
    $noSubmitCheckStatus = "PASS"
}
$screenshotCheckStatus = "FAIL"
if ($screenshotCaptured) {
    $screenshotCheckStatus = "PASS"
}
$visualTextCheckStatus = "FAIL"
if ($visualTextConfirmed) {
    $visualTextCheckStatus = "PASS"
} else {
    $visualTextCheckStatus = "FAIL_REQUIRES_SCOPED_PANEL_CONTENT_PROOF"
}
$genericCursorUiCheckStatus = "PASS"
if ($genericCursorChatOrLoginDetected -eq $true) {
    $genericCursorUiCheckStatus = "FAIL"
} elseif ($genericDetectionInconclusive) {
    $genericCursorUiCheckStatus = "INCONCLUSIVE_REQUIRES_SCREENSHOT_REVIEW"
}
$errorReport = @()
foreach ($errorItem in $errors) {
    $errorReport += $errorItem
}
$cdpTargetsPath = if ($cdpInspection -and $cdpInspection.targets_path) { [string] $cdpInspection.targets_path } else { Join-Path $runRoot "row12-cdp-targets.json" }
$cdpQueryPath = if ($cdpInspection -and $cdpInspection.query_path) { [string] $cdpInspection.query_path } else { Join-Path $runRoot "row12-cdp-webview-dom-query.json" }
$cdpBrowserVersionPath = if ($cdpInspection -and $cdpInspection.browser_version_path) { [string] $cdpInspection.browser_version_path } else { Join-Path $runRoot "row12-cdp-browser-version.json" }
$cdpBrowserTargetsPath = if ($cdpInspection -and $cdpInspection.browser_target_gettargets_path) { [string] $cdpInspection.browser_target_gettargets_path } else { Join-Path $runRoot "row12-browser-cdp-target-gettargets.json" }
$cdpInnerFrameSummaryPath = if ($cdpInspection -and $cdpInspection.inner_frame_summary_path) { [string] $cdpInspection.inner_frame_summary_path } else { Join-Path $runRoot "row12-cdp-webview-inner-frame-summary.json" }
$cdpTargetsSummary = @()
if ($cdpInspection -and $cdpInspection.targets) {
    foreach ($target in @($cdpInspection.targets)) {
        $cdpTargetsSummary += [pscustomobject] @{
            index = Get-ObjectPropertyValue -Object $target -Name "index"
            source = Get-ObjectPropertyValue -Object $target -Name "source"
            id = Get-ObjectPropertyValue -Object $target -Name "id"
            targetId = Get-ObjectPropertyValue -Object $target -Name "targetId"
            type = Get-ObjectPropertyValue -Object $target -Name "type"
            title = Get-ObjectPropertyValue -Object $target -Name "title"
            url = Get-ObjectPropertyValue -Object $target -Name "url"
            attached = Get-ObjectPropertyValue -Object $target -Name "attached"
            openerId = Get-ObjectPropertyValue -Object $target -Name "openerId"
            is_candidate = Get-ObjectPropertyValue -Object $target -Name "is_candidate"
            candidate_reasons = @(Get-ObjectPropertyValue -Object $target -Name "candidate_reasons")
        }
    }
}
$cdpSelectorTextResults = @()
if ($cdpInspection -and $cdpInspection.query_results) {
    foreach ($queryResult in @($cdpInspection.query_results)) {
        $queryTarget = Get-ObjectPropertyValue -Object $queryResult -Name "target"
        $queryValue = Get-ObjectPropertyValue -Object $queryResult -Name "result"
        $cdpSelectorTextResults += [pscustomobject] @{
            target_index = Get-ObjectPropertyValue -Object $queryTarget -Name "index"
            target_source = Get-ObjectPropertyValue -Object $queryTarget -Name "source"
            target_id = Get-ObjectPropertyValue -Object $queryTarget -Name "id"
            target_targetId = Get-ObjectPropertyValue -Object $queryTarget -Name "targetId"
            target_title = Get-ObjectPropertyValue -Object $queryTarget -Name "title"
            target_url = Get-ObjectPropertyValue -Object $queryTarget -Name "url"
            status = Get-ObjectPropertyValue -Object $queryResult -Name "status"
            attached_session_id_present = Get-ObjectPropertyValue -Object $queryResult -Name "attached_session_id_present"
            attached_session_command_path = Get-ObjectPropertyValue -Object $queryResult -Name "attached_session_command_path"
            attached_runtime_evaluate_attempted = Get-ObjectPropertyValue -Object $queryResult -Name "attached_runtime_evaluate_attempted"
            attached_runtime_evaluate_available = Get-ObjectPropertyValue -Object $queryResult -Name "attached_runtime_evaluate_available"
            attached_runtime_evaluate_error = Get-ObjectPropertyValue -Object $queryResult -Name "attached_runtime_evaluate_error"
            attached_runtime_selectors_found = Get-ObjectPropertyValue -Object $queryResult -Name "attached_runtime_selectors_found"
            attached_runtime_texts_found = Get-ObjectPropertyValue -Object $queryResult -Name "attached_runtime_texts_found"
            selectors_found = Get-ObjectPropertyValue -Object $queryResult -Name "selectors_found"
            texts_found = Get-ObjectPropertyValue -Object $queryResult -Name "texts_found"
            all_required_selectors_found = Get-ObjectPropertyValue -Object $queryResult -Name "all_required_selectors_found"
            all_required_text_found = Get-ObjectPropertyValue -Object $queryResult -Name "all_required_text_found"
            all_required_dom_visible = Get-ObjectPropertyValue -Object $queryResult -Name "all_required_dom_visible"
            body_text_sample = if ($queryValue) { Get-ObjectPropertyValue -Object $queryValue -Name "bodyTextSample" } else { $null }
            error = Get-ObjectPropertyValue -Object $queryResult -Name "error"
        }
    }
}

$commandModeSwitch = if ($SubmitResultProof.IsPresent) { " -SubmitResultProof -SubmitResultWaitMs $submitResultJsWaitMs -SubmitResultCdpTimeoutSec $submitResultCdpTimeoutSec" } elseif ($TypeabilityProofOnly.IsPresent) { " -TypeabilityProofOnly" } else { "" }
$report = [ordered] @{
    schema = if ($SubmitResultProof.IsPresent) { "xiigen.row12.cursor_panel_submit_result_runner.v1" } elseif ($TypeabilityProofOnly.IsPresent) { "xiigen.row12.cursor_panel_typeability_runner.v1" } else { "xiigen.row12.cursor_panel_visibility_runner.v1" }
    status = $status
    generated_utc = (Get-Date).ToUniversalTime().ToString("o")
    row = "Row12"
    purpose = if ($SubmitResultProof.IsPresent) { "Minimal local Cursor panel submit/result proof runner. It types a probe, clicks the panel submit button once through the first attached-runtime visible context path, waits for rendered result evidence, and makes no product/readiness claim." } elseif ($TypeabilityProofOnly.IsPresent) { "Minimal local Cursor panel typeability proof runner. It sets textarea value, dispatches input/change, reads back, and does not submit." } else { "Minimal local Cursor panel visibility runner." }
    evidence_root = $runRoot
    runtime_evidence_root = $runtimeEvidenceRoot
    files_created = @(
        $runtimeEvidenceRoot,
        $extensionTestPath,
        $hostReportPath,
        $readyMarkerPath,
        $screenshotPath,
        $stdoutLog,
        $stderrLog,
        $reportPath,
        $markdownPath,
        $runtimePathsPath,
        $cdpTargetsPath,
        $cdpBrowserVersionPath,
        $cdpBrowserTargetsPath,
        $cdpQueryPath,
        $cdpInnerFrameSummaryPath
    )
    no_git_action_run = $true
    product_readiness_claimed = $false
    command_to_run = 'powershell.exe -NoProfile -ExecutionPolicy Bypass -File "{0}"{1}' -f $PSCommandPath, $commandModeSwitch
    launch = $startInfo
    run_mode = $runMode
    typeability_proof_only = $TypeabilityProofOnly.IsPresent
    submit_result_proof_mode = $SubmitResultProof.IsPresent
    typeability_proof_requested = $cdpInspection.typeability_proof_requested
    typeability_proof_attempted = $cdpInspection.typeability_proof_attempted
    typeability_proof_status = $cdpInspection.typeability_proof_status
    typeability_probe_text = $cdpInspection.typeability_probe_text
    typeability_probe_text_sha256 = $cdpInspection.typeability_probe_text_sha256
    typeability_probe_attempted = $cdpInspection.typeability_probe_attempted
    typeability_probe_text_hash = $cdpInspection.typeability_probe_text_hash
    typeability_probe_expression_sha256 = $cdpInspection.typeability_probe_expression_sha256
    typeability_context_source = $cdpInspection.typeability_context_source
    typeability_target_id = $cdpInspection.typeability_target_id
    typeability_target_source = $cdpInspection.typeability_target_source
    typeability_session_id = $cdpInspection.typeability_session_id
    typeability_frame_id = $cdpInspection.typeability_frame_id
    typeability_execution_context_id = $cdpInspection.typeability_execution_context_id
    typeability_selector_context_matches_visibility_context = $cdpInspection.typeability_selector_context_matches_visibility_context
    typeability_readback_value = $cdpInspection.typeability_readback_value
    typeability_readback_sha256 = $cdpInspection.typeability_readback_sha256
    typeability_readback_matches_probe = $cdpInspection.typeability_readback_matches_probe
    typeability_readback_matches = $cdpInspection.typeability_readback_matches
    typeability_no_submit_performed_by_probe = $cdpInspection.typeability_no_submit_performed_by_probe
    typeability_submit_button_click_invoked = $cdpInspection.typeability_submit_button_click_invoked
    typeability_submit_button_clicked_by_probe = $cdpInspection.typeability_submit_button_clicked_by_probe
    typeability_core_path_invoked_by_probe = $cdpInspection.typeability_core_path_invoked_by_probe
    typeability_panel_submission_path_invoked_by_probe = $cdpInspection.typeability_panel_submission_path_invoked_by_probe
    typeability_no_submit_guard = $typeabilityNoSubmitCoreFlagsRemainFalse
    typeability_core_invoked = $cdpInspection.typeability_core_invoked
    typeability_no_submit_core_flags_remain_false = $typeabilityNoSubmitCoreFlagsRemainFalse
    typeability_proof = $cdpInspection.typeability_proof
    typeability_proof_errors = @($cdpInspection.typeability_proof_errors)
    submit_result_proof_requested = $cdpInspection.submit_result_proof_requested
    submit_result_proof_attempted = $cdpInspection.submit_result_proof_attempted
    submit_result_proof_status = $cdpInspection.submit_result_proof_status
    submit_probe_text = $cdpInspection.submit_probe_text
    submit_probe_text_sha256 = $cdpInspection.submit_probe_text_sha256
    submit_probe_text_hash = $cdpInspection.submit_probe_text_hash
    submit_probe_expression_sha256 = $cdpInspection.submit_probe_expression_sha256
    submit_js_wait_ms = $cdpInspection.submit_js_wait_ms
    submit_cdp_timeout_sec = $cdpInspection.submit_cdp_timeout_sec
    submit_proof_scope = $cdpInspection.submit_proof_scope
    submit_retry_policy = $cdpInspection.submit_retry_policy
    submit_first_attached_runtime_visible_context_attempted = $cdpInspection.submit_first_attached_runtime_visible_context_attempted
    submit_frame_or_evaluated_retry_skipped = $cdpInspection.submit_frame_or_evaluated_retry_skipped
    submit_context_source = $cdpInspection.submit_context_source
    submit_target_id = $cdpInspection.submit_target_id
    submit_target_source = $cdpInspection.submit_target_source
    submit_session_id = $cdpInspection.submit_session_id
    submit_frame_id = $cdpInspection.submit_frame_id
    submit_execution_context_id = $cdpInspection.submit_execution_context_id
    submit_selector_context_matches_visibility_context = $cdpInspection.submit_selector_context_matches_visibility_context
    submit_button_click_invoked = $cdpInspection.submit_button_click_invoked
    submit_button_clicked_by_probe = $cdpInspection.submit_button_clicked_by_probe
    submit_panel_submission_path_invoked = $cdpInspection.panel_submission_path_invoked
    submit_core_path_invoked = $cdpInspection.core_path_invoked
    submit_test_only_handler_used = $cdpInspection.submit_test_only_handler_used
    submit_test_only_false = $cdpInspection.submit_test_only_handler_used -ne $true
    submit_not_test_only_path = $cdpInspection.submit_not_test_only_path
    submit_result_rendered_in_webview = $cdpInspection.result_rendered_in_webview
    submit_result_text_sha256 = $cdpInspection.result_text_sha256
    submit_result_status_text = $cdpInspection.result_status_text
    submit_result_response_ref = $cdpInspection.result_response_ref
    submit_result_trace_ref = $cdpInspection.result_trace_ref
    submit_result_host_panel_ui_ref = $cdpInspection.result_host_panel_ui_ref
    submit_result_correlation_id = $cdpInspection.result_correlation_id
    submit_evidence_refs = $cdpInspection.submit_evidence_refs
    submit_runtime_artifact_evidence_used_for_classification = $submitRuntimeArtifactEvidenceUsable
    submit_runtime_artifact_evidence = $runtimeSubmitEvidence
    submit_no_readiness_claim = $cdpInspection.submit_no_readiness_claim
    submit_readiness_claimed = $cdpInspection.submit_readiness_claimed
    submit_result_proof = $cdpInspection.submit_result_proof
    submit_result_proof_errors = @($cdpInspection.submit_result_proof_errors)
    panel_hold_ms_requested = $PanelHoldMs
    panel_hold_ms_effective = $effectivePanelHoldMs
    panel_hold_ms_source = "-PanelHoldMs runner parameter propagated through ROW12_PANEL_HOLD_MS and generated extension test"
    panel_hold_fallback_5000_used = $false
    invoked_command = "xiigenSmartSkills.panel"
    targets_real_cursor_extension_development_host = $true
    test_only_handler_used = $testOnlyHandlerUsed
    generic_chat_used = $genericCursorChatOrLoginDetected
    no_submit_performed = $noSubmitPerformed
    core_path_invoked = $corePathInvoked
    panel_submission_path_invoked = $panelSubmissionPathInvoked
    cdp_port = $remoteDebuggingPort
    cdp_targets_endpoint = "http://127.0.0.1:$remoteDebuggingPort/json"
    cdp_browser_version_endpoint = "http://127.0.0.1:$remoteDebuggingPort/json/version"
    cdp_browser_version_path = $cdpBrowserVersionPath
    cdp_browser_target_gettargets_path = $cdpBrowserTargetsPath
    cdp_inner_frame_summary_path = $cdpInnerFrameSummaryPath
    cdp_frame_tree_files = if ($cdpInspection) { @($cdpInspection.cdp_frame_tree_files) } else { @() }
    cdp_execution_context_files = if ($cdpInspection) { @($cdpInspection.cdp_execution_context_files) } else { @() }
    cdp_protocol_dom_snapshot_files = if ($cdpInspection) { @($cdpInspection.cdp_protocol_dom_snapshot_files) } else { @() }
    inner_document_sample_files = if ($cdpInspection) { @($cdpInspection.inner_document_sample_files) } else { @() }
    cdp_browser_version_available = if ($cdpInspection) { $cdpInspection.browser_version_available } else { $false }
    cdp_browser_websocket_debugger_url = if ($cdpInspection) { $cdpInspection.browser_websocket_debugger_url } else { $null }
    cdp_browser_websocket_debugger_url_present = if ($cdpInspection) { $cdpInspection.browser_websocket_debugger_url_present } else { $false }
    cdp_browser_version = if ($cdpInspection) { $cdpInspection.browser_version } else { $null }
    cdp_json_target_count = if ($cdpInspection) { $cdpInspection.json_target_count } else { 0 }
    cdp_browser_target_count = if ($cdpInspection) { $cdpInspection.browser_target_count } else { 0 }
    cdp_merged_target_count = if ($cdpInspection) { $cdpInspection.merged_target_count } else { 0 }
    cdp_browser_target_gettargets_attempted = if ($cdpInspection) { $cdpInspection.browser_target_gettargets_attempted } else { $false }
    cdp_browser_target_gettargets_available = if ($cdpInspection) { $cdpInspection.browser_target_gettargets_available } else { $false }
    cdp_browser_target_gettargets_error = if ($cdpInspection) { $cdpInspection.browser_target_gettargets_error } else { $null }
    cdp_inspection_timing = $cdpInspectionTiming
    cdp_status = $cdpStatus
    cdp_attached_session_id_present = if ($cdpInspection) { $cdpInspection.attached_session_id_present } else { $false }
    cdp_attached_session_command_path = if ($cdpInspection) { $cdpInspection.attached_session_command_path } else { $null }
    cdp_attached_runtime_evaluate_attempted = if ($cdpInspection) { $cdpInspection.attached_runtime_evaluate_attempted } else { $false }
    cdp_attached_runtime_evaluate_available = if ($cdpInspection) { $cdpInspection.attached_runtime_evaluate_available } else { $false }
    cdp_attached_runtime_evaluate_error = if ($cdpInspection) { $cdpInspection.attached_runtime_evaluate_error } else { $null }
    cdp_attached_runtime_selectors_found = if ($cdpInspection) { $cdpInspection.attached_runtime_selectors_found } else { [ordered] @{
        "#xiigen-smart-skills-prompt" = $false
        "#xiigen-smart-skills-submit" = $false
    } }
    cdp_attached_runtime_texts_found = if ($cdpInspection) { $cdpInspection.attached_runtime_texts_found } else { [ordered] @{
        "XIIGen Smart Skills" = $false
        "XIIGen task prompt" = $false
    } }
    cdp_attached_runtime_all_required_selectors_found = if ($cdpInspection) { $cdpInspection.attached_runtime_all_required_selectors_found } else { $false }
    cdp_attached_runtime_all_required_text_found = if ($cdpInspection) { $cdpInspection.attached_runtime_all_required_text_found } else { $false }
    cdp_attached_runtime_all_required_dom_visible = if ($cdpInspection) { $cdpInspection.attached_runtime_all_required_dom_visible } else { $false }
    cdp_targets_summary = @($cdpTargetsSummary)
    selector_text_results = @($cdpSelectorTextResults)
    screenshot_status = $screenshotCheckStatus
    checks = [ordered] @{
        panel_content_labels_confirmed = $panelContentLabelsConfirmed
        panel_content_confirmation_source = $panelContentConfirmationSource
        panel_content_confirmation_method = $panelContentConfirmationMethod
        panel_content_proof_required = $panelContentProofRequired
        cdp_webview_dom_status = $cdpStatus
        cdp_inspection_timing = $cdpInspectionTiming
        cdp_webview_dom_attempted = if ($cdpInspection) { $cdpInspection.attempted } else { $false }
        cdp_target_count = if ($cdpInspection) { $cdpInspection.target_count } else { 0 }
        cdp_merged_target_count = if ($cdpInspection) { $cdpInspection.merged_target_count } else { 0 }
        cdp_candidate_target_count = if ($cdpInspection) { $cdpInspection.candidate_target_count } else { 0 }
        cdp_json_target_count = if ($cdpInspection) { $cdpInspection.json_target_count } else { 0 }
        browser_cdp_version_available = if ($cdpInspection) { $cdpInspection.browser_version_available } else { $false }
        browser_cdp_websocket_available = if ($cdpInspection) { $cdpInspection.browser_websocket_debugger_url_present } else { $false }
        browser_cdp_target_gettargets_attempted = if ($cdpInspection) { $cdpInspection.browser_target_gettargets_attempted } else { $false }
        browser_cdp_target_gettargets_available = if ($cdpInspection) { $cdpInspection.browser_target_gettargets_available } else { $false }
        browser_cdp_target_gettargets_error = if ($cdpInspection) { $cdpInspection.browser_target_gettargets_error } else { $null }
        browser_cdp_target_count = if ($cdpInspection) { $cdpInspection.browser_target_count } else { 0 }
        browser_cdp_candidate_target_count = if ($cdpInspection) { $cdpInspection.browser_candidate_target_count } else { 0 }
        cdp_attached_session_id_present = if ($cdpInspection) { $cdpInspection.attached_session_id_present } else { $false }
        cdp_attached_session_command_path = if ($cdpInspection) { $cdpInspection.attached_session_command_path } else { $null }
        cdp_attached_runtime_evaluate_attempted = if ($cdpInspection) { $cdpInspection.attached_runtime_evaluate_attempted } else { $false }
        cdp_attached_runtime_evaluate_available = if ($cdpInspection) { $cdpInspection.attached_runtime_evaluate_available } else { $false }
        cdp_attached_runtime_evaluate_error = if ($cdpInspection) { $cdpInspection.attached_runtime_evaluate_error } else { $null }
        cdp_attached_runtime_selectors_found = if ($cdpInspection) { $cdpInspection.attached_runtime_selectors_found } else { [ordered] @{
            "#xiigen-smart-skills-prompt" = $false
            "#xiigen-smart-skills-submit" = $false
        } }
        cdp_attached_runtime_texts_found = if ($cdpInspection) { $cdpInspection.attached_runtime_texts_found } else { [ordered] @{
            "XIIGen Smart Skills" = $false
            "XIIGen task prompt" = $false
        } }
        cdp_attached_runtime_all_required_selectors_found = if ($cdpInspection) { $cdpInspection.attached_runtime_all_required_selectors_found } else { $false }
        cdp_attached_runtime_all_required_text_found = if ($cdpInspection) { $cdpInspection.attached_runtime_all_required_text_found } else { $false }
        cdp_attached_runtime_all_required_dom_visible = if ($cdpInspection) { $cdpInspection.attached_runtime_all_required_dom_visible } else { $false }
        cdp_all_required_selectors_found = if ($cdpInspection) { $cdpInspection.all_required_selectors_found } else { $false }
        cdp_all_required_text_found = if ($cdpInspection) { $cdpInspection.all_required_text_found } else { $false }
        cdp_all_required_dom_visible = if ($cdpInspection) { $cdpInspection.all_required_dom_visible } else { $false }
        window_title_contains_xiigen = $windowTitleContainsXiigen
        uia_unscoped_xiigen_text_present = $uiaUnscopedXiigenTextPresent
        uia_unscoped_panel_title_text_present = $uiaUnscopedPanelTitleTextPresent
        uia_unscoped_prompt_label_text_present = $uiaUnscopedPromptLabelTextPresent
        uia_unscoped_submit_to_llm_mvp_core_text_present = $uiaUnscopedSubmitToCoreTextPresent
        uia_unscoped_xiigen_textarea_text_present = $uiaUnscopedXiigenTextareaTextPresent
        uia_unscoped_secondary_panel_text_present = $uiaUnscopedSecondaryPanelTextPresent
        uia_unscoped_text_is_panel_content_proof = $false
        any_uia_edit_control_visible = $anyEditControlVisible
        visual_text_extraction_method = $visualTextExtractionMethod
        visual_text_extraction_available = $visualTextExtractionAvailable
        visual_text_extraction_reliable = $visualTextExtractionReliable
        visual_text_confirmed = $visualTextConfirmed
        generic_labels_text_confirmed = $genericLabelsTextConfirmed
        generic_detection_inconclusive = $genericDetectionInconclusive
        generic_detection_reason = $genericDetectionReason
        generic_cursor_chat_or_login_detected = $genericCursorChatOrLoginDetected
        no_submit_performed = $noSubmitPerformed
        core_path_invoked = $corePathInvoked
        panel_submission_path_invoked = $panelSubmissionPathInvoked
        test_only_handler_used = $testOnlyHandlerUsed
        typeability_proof_requested = $cdpInspection.typeability_proof_requested
        typeability_proof_attempted = $cdpInspection.typeability_proof_attempted
        typeability_proof_status = $cdpInspection.typeability_proof_status
        typeability_readback_matches_probe = $cdpInspection.typeability_readback_matches_probe
        typeability_probe_attempted = $cdpInspection.typeability_probe_attempted
        typeability_probe_text_hash = $cdpInspection.typeability_probe_text_hash
        typeability_readback_matches = $cdpInspection.typeability_readback_matches
        typeability_no_submit_guard = $typeabilityNoSubmitCoreFlagsRemainFalse
        typeability_core_invoked = $cdpInspection.typeability_core_invoked
        typeability_no_submit_core_flags_remain_false = $typeabilityNoSubmitCoreFlagsRemainFalse
        submit_result_proof_requested = $cdpInspection.submit_result_proof_requested
        submit_result_proof_attempted = $cdpInspection.submit_result_proof_attempted
        submit_result_proof_status = $cdpInspection.submit_result_proof_status
        submit_probe_text_hash = $cdpInspection.submit_probe_text_hash
        submit_probe_text_sha256 = $cdpInspection.submit_probe_text_sha256
        submit_context_source = $cdpInspection.submit_context_source
        submit_button_click_invoked = $cdpInspection.submit_button_click_invoked
        submit_button_clicked_by_probe = $cdpInspection.submit_button_clicked_by_probe
        submit_panel_submission_path_invoked = $cdpInspection.panel_submission_path_invoked
        submit_core_path_invoked = $cdpInspection.core_path_invoked
        submit_result_rendered_in_webview = $cdpInspection.result_rendered_in_webview
        submit_result_text_sha256 = $cdpInspection.result_text_sha256
        submit_result_status_text = $cdpInspection.result_status_text
        submit_result_response_ref = $cdpInspection.result_response_ref
        submit_result_trace_ref = $cdpInspection.result_trace_ref
        submit_evidence_refs = $cdpInspection.submit_evidence_refs
        submit_runtime_artifact_evidence_used_for_classification = $submitRuntimeArtifactEvidenceUsable
        submit_runtime_artifact_evidence = $runtimeSubmitEvidence
        submit_test_only_false = $cdpInspection.submit_test_only_handler_used -ne $true
        submit_no_readiness_claim = $cdpInspection.submit_no_readiness_claim
        submit_readiness_claimed = $cdpInspection.submit_readiness_claimed
        submit_js_wait_ms = $cdpInspection.submit_js_wait_ms
        submit_cdp_timeout_sec = $cdpInspection.submit_cdp_timeout_sec
        submit_proof_scope = $cdpInspection.submit_proof_scope
        screenshot_captured = $screenshotCaptured
    }
    human_readable_pass_fail = @(
        ("panel content labels confirmed: {0}" -f $panelContentCheckStatus),
        ("typeability proof status: {0}" -f $cdpInspection.typeability_proof_status),
        ("typeability readback matches probe: {0}" -f $cdpInspection.typeability_readback_matches_probe),
        ("submit result proof status: {0}" -f $cdpInspection.submit_result_proof_status),
        ("submit proof click invoked: {0}" -f $cdpInspection.submit_button_click_invoked),
        ("submit proof panel/core invoked: panel={0}; core={1}" -f $cdpInspection.panel_submission_path_invoked, $cdpInspection.core_path_invoked),
        ("submit proof result rendered: {0}" -f $cdpInspection.result_rendered_in_webview),
        ("submit proof runtime artifacts used for classification: {0}" -f $submitRuntimeArtifactEvidenceUsable),
        ("submit proof test-only false: {0}" -f ($cdpInspection.submit_test_only_handler_used -ne $true)),
        ("submit proof no readiness claim: {0}" -f $cdpInspection.submit_no_readiness_claim),
        ("visual text confirmed: {0}" -f $visualTextCheckStatus),
        ("unscoped/window XIIGen text present: {0}" -f $uiaUnscopedXiigenTextPresent),
        ("generic Cursor chat/login indicators absent: {0}" -f $genericCursorUiCheckStatus),
        ("no submit performed: {0}" -f $noSubmitCheckStatus),
        ("no submit/core/test-only flags remain false: {0}" -f $typeabilityNoSubmitCoreFlagsRemainFalse),
        ("screenshot captured: {0}" -f $screenshotCheckStatus)
    )
    visual_verifier = [ordered] @{
        schema = "xiigen.row12.visual_verifier.v1"
        pass_requires = "screenshot_captured && panel_content_labels_confirmed && no_submit_performed; window title, command return, and unscoped whole-window UIA text are insufficient"
        panel_content_labels_confirmed = $panelContentLabelsConfirmed
        panel_content_confirmation_source = $panelContentConfirmationSource
        panel_content_confirmation_method = $panelContentConfirmationMethod
        panel_content_proof_required = $panelContentProofRequired
        visual_text_confirmed = $visualTextConfirmed
        visual_text_extraction_method = $visualTextExtractionMethod
        visual_text_extraction_available = $visualTextExtractionAvailable
        visual_text_extraction_reliable = $visualTextExtractionReliable
        expected_visible_labels = $expectedVisibleLabels
        window_title_contains_xiigen = $windowTitleContainsXiigen
        uia_unscoped_xiigen_text_present = $uiaUnscopedXiigenTextPresent
        uia_unscoped_text_is_panel_content_proof = $false
        uia_unscoped_has_xiigen_smart_skills_text = $uiaUnscopedPanelTitleTextPresent
        uia_unscoped_has_prompt_label_text = $uiaUnscopedPromptLabelTextPresent
        uia_unscoped_has_xiigen_textarea_accessible_text = $uiaUnscopedXiigenTextareaTextPresent
        uia_unscoped_has_submit_to_llm_mvp_core_text = $uiaUnscopedSubmitToCoreTextPresent
        uia_unscoped_has_required_secondary_panel_text = $uiaUnscopedSecondaryPanelTextPresent
        cdp_webview_dom = $cdpInspection
        generic_fail_indicators = @($genericIndicatorDefinitions | ForEach-Object { $_.marker })
        generic_labels_text_confirmed = $genericLabelsTextConfirmed
        generic_indicators_detected = $detectedGenericIndicators
        generic_detection_inconclusive = $genericDetectionInconclusive
        generic_detection_reason = $genericDetectionReason
        generic_cursor_chat_or_login_detected = $genericCursorChatOrLoginDetected
        title_only_rejected = ($windowTitleContainsXiigen -and -not $panelContentLabelsConfirmed)
        fail_markers = $visualFailMarkers
        text_sources_used = @($visualTextExtractionMethod)
        ocr = [ordered] @{
            attempted = $false
            available = $false
            reason = "No OCR adapter is configured in this runner. Screenshot capture alone is not treated as panel-content label confirmation."
        }
        uia_text = [ordered] @{
            extraction_method = $visualTextExtractionMethod
            extraction_reliable = $visualTextExtractionReliable
            scope = "whole_window_descendants_unscoped"
            can_confirm_panel_content = $false
            attempted = $uiaAttempted
            available = $uiaAvailable
            error = $uiaError
            extracted_text_available = $visualTextExtractionAvailable
            sampled_lines = @($uiaTextLines | Select-Object -First 120)
        }
    }
    cdp = $cdpInspection
    screenshot_ref = $screenshotRef
    window = $windowReport
    screenshot = $screenshotReport
    uia = [ordered] @{
        attempted = $uiaAttempted
        available = $uiaAvailable
        error = $uiaError
        sampled_names = $sampledUiaNames
        edit_controls = $uiaEditControls
    }
    extension_host_marker = $hostMarker
    extension_host_report = $hostReport
    short_temp_runtime = $runtimeCopyReport
    stdout_log = $stdoutLog
    stderr_log = $stderrLog
    errors = $errorReport
    remains_for_typeability_submit = if ($SubmitResultProof.IsPresent) { "Submit-result proof is scoped to one observed panel submit/result path and is not a product/readiness claim." } elseif ($TypeabilityProofOnly.IsPresent) { "Typeability proof sets and reads back the textarea value only. Deliberate submit requires -SubmitResultProof." } else { "Visibility mode proves open/focus visibility only. Typeability uses -TypeabilityProofOnly; deliberate submit/result proof uses -SubmitResultProof." }
}

Write-JsonFile -Path $reportPath -Value $report

$visualFailMarkerText = "- none"
if ($visualFailMarkers.Count -gt 0) {
    $visualFailMarkerText = (($visualFailMarkers | ForEach-Object { "- {0}" -f $_ }) -join "`n")
}
$genericIndicatorText = "- none"
if ($detectedGenericIndicators.Count -gt 0) {
    $genericIndicatorText = (($detectedGenericIndicators | ForEach-Object { "- {0} ({1})" -f $_.marker, $_.source }) -join "`n")
}
$genericDetectedMarkdown = if ($null -eq $genericCursorChatOrLoginDetected) {
    "INCONCLUSIVE"
} else {
    [string] $genericCursorChatOrLoginDetected
}
$manualReviewNote = if (-not $visualTextConfirmed) {
    "panel content remains unconfirmed because this run has only whole-window UIA text and a screenshot without OCR or scoped DOM/UIA provenance."
} else {
    "Text extraction confirmed XIIGen content; screenshot remains saved for audit."
}
$cdpTargetsMarkdown = "- none"
if ($cdpTargetsSummary.Count -gt 0) {
    $cdpTargetsMarkdown = (($cdpTargetsSummary | ForEach-Object {
        "- [{0}] source={1}; id={2}; targetId={3}; type={4}; attached={5}; openerId={6}; candidate={7}; title={8}; url={9}; reasons={10}" -f
            $_.index,
            $_.source,
            $_.id,
            $_.targetId,
            $_.type,
            $_.attached,
            $_.openerId,
            $_.is_candidate,
            $_.title,
            $_.url,
            ((@($_.candidate_reasons) | Where-Object { $_ }) -join ",")
    }) -join "`n")
}
$cdpSelectorMarkdown = "- none"
if ($cdpSelectorTextResults.Count -gt 0) {
    $cdpSelectorMarkdown = (($cdpSelectorTextResults | ForEach-Object {
        "- target_index={0}; source={1}; id={2}; targetId={3}; status={4}; attached_session_id_present={5}; attached_runtime_evaluate_attempted={6}; attached_runtime_evaluate_available={7}; attached_runtime_evaluate_error={8}; selectors={9}; text={10}; error={11}" -f
            $_.target_index,
            $_.target_source,
            $_.target_id,
            $_.target_targetId,
            $_.status,
            $_.attached_session_id_present,
            $_.attached_runtime_evaluate_attempted,
            $_.attached_runtime_evaluate_available,
            $_.attached_runtime_evaluate_error,
            ($_.selectors_found | ConvertTo-Json -Depth 6 -Compress),
            ($_.texts_found | ConvertTo-Json -Depth 6 -Compress),
            $_.error
    }) -join "`n")
}

$md = @"
# Row12 Cursor Panel $runMode Runner

status: $status

Command invoked: ``xiigenSmartSkills.panel``

Typeability proof:
- mode: $runMode
- requested: $($cdpInspection.typeability_proof_requested)
- attempted: $($cdpInspection.typeability_proof_attempted)
- status: $($cdpInspection.typeability_proof_status)
- selector: ``#xiigen-smart-skills-prompt``
- probe_text: $($cdpInspection.typeability_probe_text)
- probe_text_sha256: $($cdpInspection.typeability_probe_text_sha256)
- typeability_probe_attempted: $($cdpInspection.typeability_probe_attempted)
- typeability_probe_text_hash: $($cdpInspection.typeability_probe_text_hash)
- typeability_probe_expression_sha256: $($cdpInspection.typeability_probe_expression_sha256)
- typeability_context_source: $($cdpInspection.typeability_context_source)
- typeability_target_id: $($cdpInspection.typeability_target_id)
- typeability_target_source: $($cdpInspection.typeability_target_source)
- typeability_session_id: $($cdpInspection.typeability_session_id)
- typeability_frame_id: $($cdpInspection.typeability_frame_id)
- typeability_execution_context_id: $($cdpInspection.typeability_execution_context_id)
- typeability_selector_context_matches_visibility_context: $($cdpInspection.typeability_selector_context_matches_visibility_context)
- readback_value: $($cdpInspection.typeability_readback_value)
- readback_sha256: $($cdpInspection.typeability_readback_sha256)
- readback_matches_probe: $($cdpInspection.typeability_readback_matches_probe)
- typeability_readback_matches: $($cdpInspection.typeability_readback_matches)
- submit_button_click_invoked: $($cdpInspection.typeability_submit_button_click_invoked)
- submit_button_clicked_by_probe: $($cdpInspection.typeability_submit_button_clicked_by_probe)
- typeability_no_submit_performed_by_probe: $($cdpInspection.typeability_no_submit_performed_by_probe)
- typeability_no_submit_guard: $typeabilityNoSubmitCoreFlagsRemainFalse
- core_path_invoked_by_probe: $($cdpInspection.typeability_core_path_invoked_by_probe)
- typeability_core_invoked: $($cdpInspection.typeability_core_invoked)
- panel_submission_path_invoked_by_probe: $($cdpInspection.typeability_panel_submission_path_invoked_by_probe)

Submit result proof:
- mode: $runMode
- requested: $($cdpInspection.submit_result_proof_requested)
- attempted: $($cdpInspection.submit_result_proof_attempted)
- status: $($cdpInspection.submit_result_proof_status)
- probe_text: $($cdpInspection.submit_probe_text)
- probe_text_sha256: $($cdpInspection.submit_probe_text_sha256)
- submit_probe_text_hash: $($cdpInspection.submit_probe_text_hash)
- submit_probe_expression_sha256: $($cdpInspection.submit_probe_expression_sha256)
- submit_js_wait_ms: $($cdpInspection.submit_js_wait_ms)
- submit_cdp_timeout_sec: $($cdpInspection.submit_cdp_timeout_sec)
- submit_proof_scope: $($cdpInspection.submit_proof_scope)
- submit_retry_policy: $($cdpInspection.submit_retry_policy)
- submit_first_attached_runtime_visible_context_attempted: $($cdpInspection.submit_first_attached_runtime_visible_context_attempted)
- submit_frame_or_evaluated_retry_skipped: $($cdpInspection.submit_frame_or_evaluated_retry_skipped)
- submit_context_source: $($cdpInspection.submit_context_source)
- submit_target_id: $($cdpInspection.submit_target_id)
- submit_target_source: $($cdpInspection.submit_target_source)
- submit_session_id: $($cdpInspection.submit_session_id)
- submit_frame_id: $($cdpInspection.submit_frame_id)
- submit_execution_context_id: $($cdpInspection.submit_execution_context_id)
- submit_selector_context_matches_visibility_context: $($cdpInspection.submit_selector_context_matches_visibility_context)
- submit_button_click_invoked: $($cdpInspection.submit_button_click_invoked)
- submit_button_clicked_by_probe: $($cdpInspection.submit_button_clicked_by_probe)
- panel_submission_path_invoked: $($cdpInspection.panel_submission_path_invoked)
- core_path_invoked: $($cdpInspection.core_path_invoked)
- result_rendered_in_webview: $($cdpInspection.result_rendered_in_webview)
- result_text_sha256: $($cdpInspection.result_text_sha256)
- result_status_text: $($cdpInspection.result_status_text)
- result_response_ref: $($cdpInspection.result_response_ref)
- result_trace_ref: $($cdpInspection.result_trace_ref)
- result_host_panel_ui_ref: $($cdpInspection.result_host_panel_ui_ref)
- result_correlation_id: $($cdpInspection.result_correlation_id)
- submit_evidence_refs: $(($cdpInspection.submit_evidence_refs | ConvertTo-Json -Depth 6 -Compress))
- submit_runtime_artifact_evidence_used_for_classification: $submitRuntimeArtifactEvidenceUsable
- submit_runtime_artifact_evidence: $(($runtimeSubmitEvidence | ConvertTo-Json -Depth 8 -Compress))
- submit_test_only_handler_used: $($cdpInspection.submit_test_only_handler_used)
- submit_test_only_false: $($cdpInspection.submit_test_only_handler_used -ne $true)
- submit_no_readiness_claim: $($cdpInspection.submit_no_readiness_claim)
- submit_readiness_claimed: $($cdpInspection.submit_readiness_claimed)

No-submit/core/test-only flags:
- no_submit_performed: $noSubmitPerformed
- core_path_invoked: $corePathInvoked
- panel_submission_path_invoked: $panelSubmissionPathInvoked
- test_only_handler_used: $testOnlyHandlerUsed
- no_submit_core_flags_remain_false: $typeabilityNoSubmitCoreFlagsRemainFalse

CDP/WebView DOM:
- port: $remoteDebuggingPort
- endpoint: http://127.0.0.1:$remoteDebuggingPort/json
- version_endpoint: http://127.0.0.1:$remoteDebuggingPort/json/version
- browser_version_available: $($cdpInspection.browser_version_available)
- browser_websocket_debugger_url: $($cdpInspection.browser_websocket_debugger_url)
- browser_websocket_debugger_url_present: $($cdpInspection.browser_websocket_debugger_url_present)
- status: $cdpStatus
- target_count: $($cdpInspection.target_count)
- merged_target_count: $($cdpInspection.merged_target_count)
- json_target_count: $($cdpInspection.json_target_count)
- browser_target_count: $($cdpInspection.browser_target_count)
- candidate_target_count: $($cdpInspection.candidate_target_count)
- browser_candidate_target_count: $($cdpInspection.browser_candidate_target_count)
- targets_path: $cdpTargetsPath
- browser_version_path: $cdpBrowserVersionPath
- browser_target_gettargets_path: $cdpBrowserTargetsPath
- query_path: $cdpQueryPath
- inner_frame_summary_path: $cdpInnerFrameSummaryPath
- frame_tree_files: $((@($cdpInspection.cdp_frame_tree_files) | Where-Object { $_ }) -join ", ")
- execution_context_files: $((@($cdpInspection.cdp_execution_context_files) | Where-Object { $_ }) -join ", ")
- protocol_dom_snapshot_files: $((@($cdpInspection.cdp_protocol_dom_snapshot_files) | Where-Object { $_ }) -join ", ")
- inner_document_sample_file_count: $(@($cdpInspection.inner_document_sample_files).Count)
- browser_target_gettargets_attempted: $($cdpInspection.browser_target_gettargets_attempted)
- browser_target_gettargets_available: $($cdpInspection.browser_target_gettargets_available)
- browser_target_gettargets_error: $($cdpInspection.browser_target_gettargets_error)
- inspection_timing: $cdpInspectionTiming
- attached_session_id_present: $($cdpInspection.attached_session_id_present)
- attached_session_command_path: $($cdpInspection.attached_session_command_path)
- attached_runtime_evaluate_attempted: $($cdpInspection.attached_runtime_evaluate_attempted)
- attached_runtime_evaluate_available: $($cdpInspection.attached_runtime_evaluate_available)
- attached_runtime_evaluate_error: $($cdpInspection.attached_runtime_evaluate_error)
- attached_runtime_selectors_found: $(($cdpInspection.attached_runtime_selectors_found | ConvertTo-Json -Depth 6 -Compress))
- attached_runtime_texts_found: $(($cdpInspection.attached_runtime_texts_found | ConvertTo-Json -Depth 6 -Compress))
- all_required_selectors_found: $($cdpInspection.all_required_selectors_found)
- all_required_text_found: $($cdpInspection.all_required_text_found)
- all_required_dom_visible: $($cdpInspection.all_required_dom_visible)

Panel hold:
- requested_ms: $PanelHoldMs
- effective_ms: $effectivePanelHoldMs
- source: -PanelHoldMs runner parameter propagated through ROW12_PANEL_HOLD_MS and generated extension test
- fallback_5000_used: False

What it verifies:
- panel content labels confirmed: $panelContentCheckStatus
- visual text confirmed: $visualTextCheckStatus
- unscoped/window XIIGen text present: $uiaUnscopedXiigenTextPresent
- generic Cursor chat/login indicators absent: $genericCursorUiCheckStatus
- no submit performed: $noSubmitCheckStatus
- screenshot captured: $screenshotCheckStatus

Visual verifier:
- panel_content_labels_confirmed: $panelContentLabelsConfirmed
- panel_content_confirmation_source: $panelContentConfirmationSource
- panel_content_confirmation_method: $panelContentConfirmationMethod
- panel_content_proof_required: $panelContentProofRequired
- visual_text_confirmed: $visualTextConfirmed
- visual_text_extraction_method: $visualTextExtractionMethod
- visual_text_extraction_available: $visualTextExtractionAvailable
- visual_text_extraction_reliable: $visualTextExtractionReliable
- window_title_contains_xiigen: $windowTitleContainsXiigen
- uia_unscoped_xiigen_text_present: $uiaUnscopedXiigenTextPresent
- uia_unscoped_text_is_panel_content_proof: False
- uia_unscoped_has_xiigen_smart_skills_text: $uiaUnscopedPanelTitleTextPresent
- uia_unscoped_has_prompt_label_text: $uiaUnscopedPromptLabelTextPresent
- uia_unscoped_has_submit_to_llm_mvp_core_text: $uiaUnscopedSubmitToCoreTextPresent
- uia_unscoped_has_xiigen_textarea_accessible_text: $uiaUnscopedXiigenTextareaTextPresent
- generic_labels_text_confirmed: $genericLabelsTextConfirmed
- generic_detection_inconclusive: $genericDetectionInconclusive
- generic_detection_reason: $genericDetectionReason
- title_only_rejected: $($windowTitleContainsXiigen -and -not $panelContentLabelsConfirmed)
- generic_cursor_chat_or_login_detected: $genericDetectedMarkdown
- manual_review_note: $manualReviewNote

Why window/unscoped text is not enough:
The native Cursor window title can contain XIIGen because the tab or command name mentions it, and whole-window UIA text can include titles, tabs, menus, hidden controls, or off-panel accessibility names. Row12 panel-content proof requires visible panel content from OCR, DOM/webview inspection, or UIA scoped to the XIIGen panel itself.

CDP targets summary:
$cdpTargetsMarkdown

CDP selector/text results:
$cdpSelectorMarkdown

Generic indicators detected:
$genericIndicatorText

Fail markers:
$visualFailMarkerText

Screenshot: $screenshotPath

JSON report: $reportPath

Short temp runtime paths:
- user-data-dir: $userDataDir
- extensions-dir: $extensionsDir
- copied diagnostics: $runtimePathsPath

This is not a product/readiness claim. In typeability mode it sets and reads back the textarea value only; in submit_result_proof mode it performs one scoped submit/result proof and reports only the observed panel/core/result evidence.
"@
Write-TextFile -Path $markdownPath -Value $md

$report | ConvertTo-Json -Depth 12
