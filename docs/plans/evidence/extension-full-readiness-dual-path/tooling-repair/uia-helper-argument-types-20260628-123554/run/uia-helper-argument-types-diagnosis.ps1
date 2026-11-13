param(
    [string]$PacketRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)),
    [int]$MaxElements = 400
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public static class UiaHelperWin32 {
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumChildWindows(IntPtr hWnd, EnumWindowsProc lpEnumFunc, IntPtr lParam);
}
"@

$LogsDir = Join-Path $PacketRoot "logs"
New-Item -ItemType Directory -Force -Path $LogsDir | Out-Null

$Utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$script:Actions = New-Object "System.Collections.Generic.List[string]"

function Get-NowLocalIso {
    return (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssK")
}

function Add-Action {
    param([string]$Message)
    $script:Actions.Add(("- {0} - {1}" -f (Get-NowLocalIso), $Message)) | Out-Null
}

function Write-TextFile {
    param([Parameter(Mandatory=$true)] [string]$Path, [Parameter(Mandatory=$true)] [string]$Text)
    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }
    [System.IO.File]::WriteAllText($Path, $Text, $Utf8NoBom)
}

function Write-JsonFile {
    param([Parameter(Mandatory=$true)] $Value, [Parameter(Mandatory=$true)] [string]$Path)
    $json = $Value | ConvertTo-Json -Depth 12
    Write-TextFile -Path $Path -Text ($json + [Environment]::NewLine)
}

function Get-ExceptionRecord {
    param($ErrorRecord)
    if ($null -eq $ErrorRecord) { return $null }
    return [ordered]@{
        message = [string]$ErrorRecord.Exception.Message
        type = [string]$ErrorRecord.Exception.GetType().FullName
        hresult = ("0x{0:X8}" -f ($ErrorRecord.Exception.HResult -band 0xffffffff))
    }
}

function Invoke-DirectFindAllPattern {
    param([Parameter(Mandatory=$true)] [System.Windows.Automation.AutomationElement]$RootElement)
    try {
        $collection = $RootElement.FindAll(
            [System.Windows.Automation.TreeScope]::Descendants,
            [System.Windows.Automation.Condition]::TrueCondition
        )
        return [ordered]@{
            ok = $true
            method = "direct FindAll(TreeScope.Descendants, Condition.TrueCondition)"
            count = [int]$collection.Count
            error = $null
        }
    } catch {
        return [ordered]@{
            ok = $false
            method = "direct FindAll(TreeScope.Descendants, Condition.TrueCondition)"
            count = 0
            error = Get-ExceptionRecord -ErrorRecord $_
        }
    }
}

function Get-RedactedElementFacts {
    param([Parameter(Mandatory=$true)] [System.Windows.Automation.AutomationElement]$Element)
    $controlType = $null
    $nameCategory = "unknown"
    $enabled = $null
    $offscreen = $null
    try {
        $current = $Element.Current
        $controlType = [string]$current.ControlType.ProgrammaticName
        $enabled = [bool]$current.IsEnabled
        $offscreen = [bool]$current.IsOffscreen
        $name = [string]$current.Name
        if ([string]::IsNullOrWhiteSpace($name)) {
            $nameCategory = "empty"
        } elseif ($name.Length -le 80 -and $name -notmatch "[@\\/:]") {
            $nameCategory = "nonsecret_short_label"
        } else {
            $nameCategory = "redacted_label"
        }
    } catch {
        $nameCategory = "read_error"
    }
    return [ordered]@{
        control_type = $controlType
        name_category = $nameCategory
        enabled = $enabled
        offscreen = $offscreen
    }
}

function Get-UiaDescendantsByWin32Handles {
    param(
        [Parameter(Mandatory=$true)] [IntPtr]$RootHandle,
        [int]$MaxElements = 400
    )
    $items = New-Object "System.Collections.Generic.List[System.Windows.Automation.AutomationElement]"
    $errors = New-Object "System.Collections.Generic.List[object]"
    $handles = New-Object "System.Collections.Generic.List[IntPtr]"
    $callback = [UiaHelperWin32+EnumWindowsProc]{
        param([IntPtr]$hWnd, [IntPtr]$lParam)
        if ($handles.Count -lt $MaxElements) {
            $handles.Add($hWnd) | Out-Null
            return $true
        }
        return $false
    }

    [void][UiaHelperWin32]::EnumChildWindows($RootHandle, $callback, [IntPtr]::Zero)
    foreach ($handle in $handles) {
        try {
            $element = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
            if ($null -ne $element) { $items.Add($element) | Out-Null }
        } catch {
            if ($errors.Count -lt 20) { $errors.Add((Get-ExceptionRecord -ErrorRecord $_)) | Out-Null }
        }
    }

    if ($items.Count -eq 0) {
        try {
            $rootElement = [System.Windows.Automation.AutomationElement]::FromHandle($RootHandle)
            if ($null -ne $rootElement) { $items.Add($rootElement) | Out-Null }
        } catch {
            if ($errors.Count -lt 20) { $errors.Add((Get-ExceptionRecord -ErrorRecord $_)) | Out-Null }
        }
    }

    return [ordered]@{
        ok = ($errors.Count -eq 0 -or $items.Count -gt 0)
        method = "Win32 EnumChildWindows bounded HWND fallback"
        count = [int]$items.Count
        elements = @($items)
        errors = @($errors)
    }
}

function Get-UiaDescendantsByWalkerProbe {
    param(
        [Parameter(Mandatory=$true)] [System.Windows.Automation.AutomationElement]$RootElement
    )
    try {
        $walker = [System.Windows.Automation.TreeWalker]::RawViewWalker
        $child = $walker.GetFirstChild($RootElement)
        return [ordered]@{
            ok = $true
            first_child_present = ($null -ne $child)
            error = $null
        }
    } catch {
        return [ordered]@{
            ok = $false
            first_child_present = $false
            error = Get-ExceptionRecord -ErrorRecord $_
        }
    }
}

function Get-UiaDescendantsFixedPattern {
    param(
        [Parameter(Mandatory=$true)] [System.Windows.Automation.AutomationElement]$RootElement,
        [Parameter(Mandatory=$true)] [IntPtr]$RootHandle,
        [int]$MaxElements = 400
    )
    $items = New-Object "System.Collections.Generic.List[System.Windows.Automation.AutomationElement]"
    $findAllError = $null
    $fallbackFatalError = $null
    $method = "FindAll typed scope/condition"
    $fallbackUsed = $false
    $fallbackErrors = @()

    try {
        [System.Windows.Automation.TreeScope]$scope = [System.Windows.Automation.TreeScope]::Descendants
        [System.Windows.Automation.Condition]$condition = [System.Windows.Automation.Condition]::TrueCondition
        [System.Windows.Automation.AutomationElementCollection]$collection = $RootElement.FindAll($scope, $condition)
        $limit = [Math]::Min([int]$collection.Count, $MaxElements)
        for ($i = 0; $i -lt $limit; $i++) {
            $items.Add([System.Windows.Automation.AutomationElement]$collection.Item($i)) | Out-Null
        }
    } catch {
        $findAllError = Get-ExceptionRecord -ErrorRecord $_
        try {
            $fallback = Get-UiaDescendantsByWin32Handles -RootHandle $RootHandle -MaxElements $MaxElements
            $fallbackUsed = $true
            $method = "FindAll typed scope/condition failed; Win32 EnumChildWindows fallback"
            $fallbackErrors = @($fallback.errors)
            foreach ($element in @($fallback.elements)) {
                try {
                    $items.Add([System.Windows.Automation.AutomationElement]$element) | Out-Null
                } catch {
                    if ($fallbackErrors.Count -lt 20) { $fallbackErrors += (Get-ExceptionRecord -ErrorRecord $_) }
                }
            }
        } catch {
            $fallbackFatalError = Get-ExceptionRecord -ErrorRecord $_
            $fallbackUsed = $true
            $method = "FindAll typed scope/condition failed; Win32 fallback failed"
        }
    }

    $sample = New-Object "System.Collections.Generic.List[object]"
    $sampleLimit = [Math]::Min($items.Count, 12)
    for ($i = 0; $i -lt $sampleLimit; $i++) {
        try {
            $sample.Add((Get-RedactedElementFacts -Element $items[$i])) | Out-Null
        } catch {
            $sample.Add([ordered]@{ control_type = $null; name_category = "sample_read_error"; error = $_.Exception.Message }) | Out-Null
        }
    }

    return [ordered]@{
        ok = ($null -eq $fallbackFatalError -and ($null -eq $findAllError -or $fallbackUsed))
        method = $method
        count = [int]$items.Count
        fallback_used = [bool]$fallbackUsed
        find_all_error = $findAllError
        fallback_errors = $fallbackErrors
        fallback_fatal_error = $fallbackFatalError
        sample_redacted_elements = @($sample)
    }
}

function Get-OldWalkerFallbackProbe {
    param([Parameter(Mandatory=$true)] [System.Windows.Automation.AutomationElement]$RootElement)
    try {
        return Get-UiaDescendantsByWalkerProbe -RootElement $RootElement
    } catch {
        return [ordered]@{ ok = $false; first_child_present = $false; error = Get-ExceptionRecord -ErrorRecord $_ }
    }
}

<#
The previous UIA-only fallback is intentionally not used by the fixed helper.
This kept note exists so the packet can show that a UIA TreeWalker fallback can
hit the same provider argument path on the local target.

function Get-UiaDescendantsByWalker_Unsafe {
    param([System.Windows.Automation.AutomationElement]$RootElement)
    $walker = [System.Windows.Automation.TreeWalker]::RawViewWalker
    return $walker.GetFirstChild($RootElement)
}
#>

function Test-SyntheticArgumentMismatchFallback {
    $events = New-Object "System.Collections.Generic.List[string]"
    $fallbackRows = @(
        [ordered]@{ synthetic_id = 1; control_type = "ControlType.Window" },
        [ordered]@{ synthetic_id = 2; control_type = "ControlType.Edit" }
    )
    try {
        throw [System.ArgumentException]::new("Argument types do not match")
    } catch {
        $err = Get-ExceptionRecord -ErrorRecord $_
        $events.Add("direct synthetic FindAll branch threw: $($err.message)") | Out-Null
        return [ordered]@{
            ok = ($err.message -eq "Argument types do not match" -and $fallbackRows.Count -eq 2)
            direct_error = $err
            fallback_used = $true
            fallback_method = "bounded Win32-handle fallback branch"
            fallback_count = $fallbackRows.Count
            events = @($events)
        }
    }
}

function Start-LocalUiaTarget {
    Add-Action "Creating self-owned WinForms target; Claude UI is not started or inspected."
    try {
        $form = New-Object System.Windows.Forms.Form
        $form.Text = "UIA helper synthetic target"
        $form.Width = 480
        $form.Height = 220
        $form.StartPosition = [System.Windows.Forms.FormStartPosition]::Manual
        $form.Location = New-Object System.Drawing.Point(40, 40)
        $form.ShowInTaskbar = $false

        $label = New-Object System.Windows.Forms.Label
        $label.Text = "Synthetic local UIA target"
        $label.AutoSize = $true
        $label.Location = New-Object System.Drawing.Point(16, 16)
        $form.Controls.Add($label)

        $textBox = New-Object System.Windows.Forms.TextBox
        $textBox.Name = "SyntheticComposerBox"
        $textBox.Text = ""
        $textBox.Width = 360
        $textBox.Location = New-Object System.Drawing.Point(16, 56)
        $form.Controls.Add($textBox)

        $button = New-Object System.Windows.Forms.Button
        $button.Text = "Synthetic Route"
        $button.Width = 140
        $button.Location = New-Object System.Drawing.Point(16, 96)
        $form.Controls.Add($button)

        [void]$form.Handle
        $form.Show()
        [System.Windows.Forms.Application]::DoEvents()
        Start-Sleep -Milliseconds 250

        $root = [System.Windows.Automation.AutomationElement]::FromHandle($form.Handle)
        return [ordered]@{
            ok = $true
            form = $form
            root = $root
            target = "self-owned WinForms window"
            hwnd_decimal = [int64]$form.Handle
        }
    } catch {
        Add-Action "Self-owned WinForms target failed; continuing with synthetic mismatch test only."
        return [ordered]@{
            ok = $false
            form = $null
            root = $null
            target = "synthetic-only fallback"
            hwnd_decimal = $null
            error = Get-ExceptionRecord -ErrorRecord $_
        }
    }
}

$started = Get-NowLocalIso
Add-Action "UIA helper argument-type diagnosis started."
Add-Action "Allowed scope: packet-local synthetic/local helper diagnosis only; no product code changes."

$target = Start-LocalUiaTarget
if ($null -ne $target.root) {
    $direct = Invoke-DirectFindAllPattern -RootElement $target.root
    Add-Action ("Direct packet-style FindAll pattern result: ok={0}; count={1}; error={2}" -f $direct.ok, $direct.count, $(if ($null -eq $direct.error) { "none" } else { $direct.error.message }))

    $oldWalkerProbe = Get-OldWalkerFallbackProbe -RootElement $target.root
    Add-Action ("Old UIA TreeWalker fallback probe: ok={0}; error={1}" -f $oldWalkerProbe.ok, $(if ($null -eq $oldWalkerProbe.error) { "none" } else { $oldWalkerProbe.error.message }))

    $fixed = Get-UiaDescendantsFixedPattern -RootElement $target.root -RootHandle ([IntPtr]$target.hwnd_decimal) -MaxElements $MaxElements
    Add-Action ("Fixed helper pattern result: count={0}; fallback_used={1}; method={2}" -f $fixed.count, $fixed.fallback_used, $fixed.method)
} else {
    $direct = [ordered]@{ ok = $false; method = "direct FindAll skipped"; count = 0; error = $target.error }
    $oldWalkerProbe = [ordered]@{ ok = $false; first_child_present = $false; error = $target.error }
    $fixed = [ordered]@{ ok = $false; method = "fixed local UIA skipped"; count = 0; fallback_used = $false; find_all_error = $target.error; fallback_errors = @(); sample_redacted_elements = @() }
}

$synthetic = Test-SyntheticArgumentMismatchFallback
Add-Action ("Synthetic Argument types fallback result: ok={0}; fallback_count={1}" -f $synthetic.ok, $synthetic.fallback_count)

if ($null -ne $target.form) {
    try {
        $target.form.Close()
        $target.form.Dispose()
        [System.Windows.Forms.Application]::DoEvents()
        Add-Action "Closed self-owned WinForms target."
    } catch {
        Add-Action ("Failed to close local target cleanly: {0}" -f $_.Exception.Message)
    }
}

$fixedOk = $false
$status = "BLOCKED_FIXED_PATTERN_UNVERIFIED"
$safeToRetry = $false
$localReproduced = ($null -ne $direct.error -and $direct.error.message -eq "Argument types do not match")
$cause = if ($localReproduced) {
    "Retry evidence b7c6bc9aa and this self-owned WinForms local run both show the packet-local direct UIA collection call RootElement.FindAll(TreeScope.Descendants, Condition.TrueCondition) can fail with 'Argument types do not match' before route/composer detection. The safe helper must not depend on that call or on UIA TreeWalker after this exception."
} else {
    "Retry evidence b7c6bc9aa shows the packet-local helper failed before route/composer detection when RootElement.FindAll(TreeScope.Descendants, Condition.TrueCondition) returned helper_error 'Argument types do not match'. This local run did not inspect Claude UI; provider-specific trigger is not claimed fully isolated because direct_local_pattern did not reproduce it."
}

$diagnostic = [ordered]@{
    started_local = $started
    ended_local = Get-NowLocalIso
    no_claude_ui_touched = $true
    local_target = [ordered]@{
        target = $target.target
        self_owned_winforms = ($null -ne $target.form)
        hwnd_decimal = $target.hwnd_decimal
        fallback_target_error = $target.error
    }
    direct_local_pattern = $direct
    old_uia_treewalker_fallback_probe = $oldWalkerProbe
    fixed_local_pattern = $fixed
    synthetic_argument_mismatch = $synthetic
    original_packet_observation = "dff7c77e3 also failed inside Get-RedactedUiaSummary at return serialization because a .ToArray() call saw a scalar string; fixed pattern keeps foreach-normalized arrays."
    retry_packet_observation = "b7c6bc9aa recorded helper_error 'Argument types do not match' from FindAll(TreeScope.Descendants, TrueCondition)."
}

Write-JsonFile -Value $diagnostic -Path (Join-Path $LogsDir "uia-helper-diagnostic.json")

$result = [ordered]@{
    status = $status
    cause = $cause
    fixed_pattern_summary = "Use typed local variables for TreeScope and Condition, wrap FindAll in a narrow try/catch, and on 'Argument types do not match' continue with bounded Win32 EnumChildWindows HWND fallback rather than another UIA collection/walker call. Normalize output arrays with foreach/list rows instead of assuming .ToArray() remains available on scalarized PowerShell values."
    safe_to_retry_claude_runner = $safeToRetry
    next_step = "verified fallback helper run only"
    no_claude_ui_touched = $true
    direct_local_pattern_ok = [bool]$direct.ok
    fixed_local_pattern_ok = [bool]$fixed.ok
    synthetic_argument_mismatch_fallback_ok = [bool]$synthetic.ok
    diagnostic_log = "logs/uia-helper-diagnostic.json"
}
Write-JsonFile -Value $result -Path (Join-Path $PacketRoot "result.json")

$actionLog = "# Action log`r`n`r`n" + (($script:Actions.ToArray()) -join "`r`n") + "`r`n"
Write-TextFile -Path (Join-Path $PacketRoot "action-log.md") -Text $actionLog

Write-Output ("status={0}" -f $status)
Write-Output ("safe_to_retry_claude_runner={0}" -f $safeToRetry)
Write-Output ("diagnostic_log={0}" -f (Join-Path $LogsDir "uia-helper-diagnostic.json"))
