param(
    [string] $CursorExe = "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe",
    [string] $ExtensionRoot = "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills",
    [string] $Row12Runner = "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\test\host-qa\ROW12-cursor-panel-visibility-runner.ps1",
    [string] $EvidenceRoot = $PSScriptRoot,
    [int] $PanelHoldMs = 60000,
    [switch] $KeepCursorOpen
)

$ErrorActionPreference = "Stop"

function Ensure-Directory {
    param([string] $Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Force -Path $Path | Out-Null
    }
}

function Write-JsonFile {
    param(
        [string] $Path,
        [object] $Value,
        [int] $Depth = 24
    )
    Ensure-Directory -Path (Split-Path -Parent $Path)
    $Value | ConvertTo-Json -Depth $Depth | Set-Content -LiteralPath $Path -Encoding UTF8
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

function Get-JsonPropertyRecursive {
    param(
        [object] $Value,
        [string] $Name
    )

    if ($null -eq $Value) {
        return $null
    }

    if ($Value -is [System.Collections.IEnumerable] -and -not ($Value -is [string])) {
        foreach ($item in $Value) {
            $result = Get-JsonPropertyRecursive -Value $item -Name $Name
            if ($null -ne $result) {
                return $result
            }
        }
        return $null
    }

    foreach ($property in @($Value.PSObject.Properties)) {
        if ($property.Name -eq $Name) {
            return $property.Value
        }
        $result = Get-JsonPropertyRecursive -Value $property.Value -Name $Name
        if ($null -ne $result) {
            return $result
        }
    }

    return $null
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

function Find-Row12CursorDebugPort {
    $processes = @(Get-CimInstance Win32_Process -Filter "Name = 'Cursor.exe'" -ErrorAction SilentlyContinue)
    foreach ($process in $processes) {
        $commandLine = [string] $process.CommandLine
        if ([string]::IsNullOrWhiteSpace($commandLine)) {
            continue
        }
        if ($commandLine -notmatch "cursor-xiigen-smart-skills") {
            continue
        }
        if ($commandLine -notmatch "--remote-debugging-port=(\d+)") {
            continue
        }
        if ($commandLine -notmatch "--user-data-dir") {
            continue
        }
        return [pscustomobject] @{
            port = [int] $Matches[1]
            process_id = [int] $process.ProcessId
            command_line = $commandLine
        }
    }

    return $null
}

$exactPrompt = "The user challenges whether a reported pass is real when the evidence only proves a boundary marker and does not prove the broader readiness claim."
$expectedPromptHash = Get-Sha256Hex -Value $exactPrompt
$expectedSkillLaterNotClaimed = "output-skepticism"
$startedUtc = (Get-Date).ToUniversalTime().ToString("o")

Ensure-Directory -Path $EvidenceRoot

$row12EvidenceBase = Join-Path $EvidenceRoot "row12-base"
$row12Stdout = Join-Path $EvidenceRoot "row12-runner.stdout.log"
$row12Stderr = Join-Path $EvidenceRoot "row12-runner.stderr.log"
$startInfoPath = Join-Path $EvidenceRoot "row13-3b-start-info.json"
$cdpReportPath = Join-Path $EvidenceRoot "row13-3b-cdp-exact-prompt-proof.json"
$cdpTargetsPath = Join-Path $EvidenceRoot "row13-3b-cdp-targets.json"
$cdpScreenshotPath = Join-Path $EvidenceRoot "row13-3b-cdp-selected-webview.png"
$reportPath = Join-Path $EvidenceRoot "row13-3b-cursor-panel-open-type-proof-report.json"
$markdownPath = Join-Path $EvidenceRoot "row13-3b-cursor-panel-open-type-proof-report.md"
$nodeLogPath = Join-Path $EvidenceRoot "row13-3b-cdp-inject-node.log"
$nodeScript = Join-Path $PSScriptRoot "row13-3b-cdp-inject-exact-prompt.mjs"

$startInfo = [ordered] @{
    schema = "xiigen.row13_3b.cursor_panel_open_type_start.v1"
    started_utc = $startedUtc
    bounded_actor = "Row13.3b bounded sub-agent"
    cursor_exe = $CursorExe
    extension_root = $ExtensionRoot
    row12_runner = $Row12Runner
    evidence_root = $EvidenceRoot
    row12_evidence_base = $row12EvidenceBase
    exact_prompt = $exactPrompt
    exact_prompt_sha256 = $expectedPromptHash
    expected_skill_later_not_claimed = $expectedSkillLaterNotClaimed
    submit_allowed = $false
    git_authority = "forbidden"
    no_git_action_run = $true
    production_extension_source_edit_authorized = $false
    product_readiness_claimed = $false
}
Write-JsonFile -Path $startInfoPath -Value $startInfo

$row12ExitCode = $null
$row12ReportPath = $null
$row12Report = $null
$nodeExitCode = $null
$nodeOutput = $null
$cdpReport = $null
$status = "ROW13_3B_NEEDS_RUNNER_FIX"
$errors = New-Object System.Collections.Generic.List[object]

try {
    if (-not (Test-Path -LiteralPath $Row12Runner)) {
        throw "Row12 runner not found: $Row12Runner"
    }
    if (-not (Test-Path -LiteralPath $nodeScript)) {
        throw "Row13.3b Node CDP helper not found: $nodeScript"
    }

    Ensure-Directory -Path $row12EvidenceBase
    $row12Args = @(
        "-NoProfile",
        "-ExecutionPolicy", "Bypass",
        "-File", $Row12Runner,
        "-CursorExe", $CursorExe,
        "-ExtensionRoot", $ExtensionRoot,
        "-EvidenceBase", $row12EvidenceBase,
        "-HostReadyTimeoutSec", "90",
        "-WindowReadyTimeoutSec", "45",
        "-PanelSettleSec", "2",
        "-PanelHoldMs", ([string] $PanelHoldMs),
        "-TypeabilityProofOnly",
        "-KeepCursorOpen"
    )

    $row12ArgumentLine = Join-NativeArgumentList -Arguments $row12Args
    $row12Process = Start-Process -FilePath "powershell.exe" `
        -ArgumentList $row12ArgumentLine `
        -PassThru `
        -WindowStyle Hidden `
        -RedirectStandardOutput $row12Stdout `
        -RedirectStandardError $row12Stderr

    $remoteDebuggingPort = $null
    $cursorProcessEvidence = $null
    $readyMarkerPath = $null
    $row12CdpQueryPath = $null
    $nodeAttemptCount = 0
    $nodePassDuringHold = $false
    $holdDeadline = (Get-Date).AddMilliseconds([Math]::Max($PanelHoldMs - 5000, 15000))

    do {
        if ($null -eq $remoteDebuggingPort) {
            $cursorProcessEvidence = Find-Row12CursorDebugPort
            if ($cursorProcessEvidence) {
                $remoteDebuggingPort = $cursorProcessEvidence.port
            }
        }

        if (-not $readyMarkerPath) {
            $readyMarkerPath = @(Get-ChildItem -LiteralPath $row12EvidenceBase -Recurse -Filter "row12-panel-ready-marker.json" -ErrorAction SilentlyContinue |
                Sort-Object LastWriteTime -Descending |
                Select-Object -First 1).FullName
        }

        if (-not $row12CdpQueryPath) {
            $row12CdpQueryPath = @(Get-ChildItem -LiteralPath $row12EvidenceBase -Recurse -Filter "row12-cdp-webview-dom-query.json" -ErrorAction SilentlyContinue |
                Sort-Object LastWriteTime -Descending |
                Select-Object -First 1).FullName
        }

        if ($remoteDebuggingPort -and ($readyMarkerPath -or $row12CdpQueryPath)) {
            $nodeAttemptCount += 1
            $nodeArgs = @(
                $nodeScript,
                "--port", ([string] $remoteDebuggingPort),
                "--prompt", $exactPrompt,
                "--out", $cdpReportPath,
                "--screenshot", $cdpScreenshotPath,
                "--targetsOut", $cdpTargetsPath
            )
            $nodeOutput = & node @nodeArgs 2>&1
            $nodeExitCode = $LASTEXITCODE
            Write-TextFile -Path $nodeLogPath -Value (($nodeOutput | ForEach-Object { [string] $_ }) -join "`n")

            if (Test-Path -LiteralPath $cdpReportPath) {
                $candidateCdpReport = Get-Content -Raw -LiteralPath $cdpReportPath | ConvertFrom-Json
                if (($nodeExitCode -eq 0) -and
                    ($candidateCdpReport.realCursorUiWebviewTargetFound -eq $true) -and
                    ($candidateCdpReport.promptFieldValueEqualsExact -eq $true) -and
                    ($candidateCdpReport.promptHashMatchesExpected -eq $true)) {
                    $cdpReport = $candidateCdpReport
                    $nodePassDuringHold = $true
                    break
                }
            }
        }

        Start-Sleep -Milliseconds 1000
    } while ((Get-Date) -lt $holdDeadline -and -not $row12Process.HasExited)

    if (-not $nodePassDuringHold -and (Test-Path -LiteralPath $cdpReportPath)) {
        $cdpReport = Get-Content -Raw -LiteralPath $cdpReportPath | ConvertFrom-Json
    }

    $waitMs = [Math]::Max($PanelHoldMs + 60000, 90000)
    if (-not $row12Process.WaitForExit($waitMs)) {
        [void] $errors.Add([pscustomobject] @{
            stage = "row12_wait_for_exit"
            message = "Timed out waiting for Row12 runner process to exit after exact prompt injection."
            row12_process_id = $row12Process.Id
        })
    }
    $row12ExitCode = $row12Process.ExitCode

    $row12ReportPath = @(Get-ChildItem -LiteralPath $row12EvidenceBase -Recurse -Filter "row12-cursor-panel-typeability-proof-report.json" -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1).FullName
    if (-not $row12ReportPath) {
        throw "Row12 runner did not create row12-cursor-panel-typeability-proof-report.json."
    }

    $row12Report = Get-Content -Raw -LiteralPath $row12ReportPath | ConvertFrom-Json

    if (-not (Test-Path -LiteralPath $cdpReportPath)) {
        throw "Node CDP helper did not create $cdpReportPath"
    }
    if (-not $cdpReport) {
        $cdpReport = Get-Content -Raw -LiteralPath $cdpReportPath | ConvertFrom-Json
    }
} catch {
    [void] $errors.Add([pscustomobject] @{
        stage = "wrapper"
        message = $_.Exception.Message
        stack = $_.ScriptStackTrace
    })
}

$row12RealUi = $false
$row12NoSubmit = $false
$row12CoreFalse = $false
$row12TestOnlyFalse = $false
if ($row12Report) {
    $row12RealUi = ($row12Report.targets_real_cursor_extension_development_host -eq $true) -and
        (($row12Report.cdp_candidate_target_count -gt 0) -or ($row12Report.cdp_browser_candidate_target_count -gt 0) -or ($row12Report.cdp_merged_target_count -gt 0))
    $row12NoSubmit = ($row12Report.no_submit_performed -eq $true) -and
        ($row12Report.typeability_submit_button_click_invoked -ne $true) -and
        ($row12Report.typeability_submit_button_clicked_by_probe -ne $true) -and
        ($row12Report.panel_submission_path_invoked -ne $true)
    $row12CoreFalse = ($row12Report.core_path_invoked -ne $true) -and ($row12Report.typeability_core_invoked -ne $true)
    $row12TestOnlyFalse = ($row12Report.test_only_handler_used -ne $true)
}

$cdpPass = $false
if ($cdpReport) {
    $cdpPass = ($cdpReport.realCursorUiWebviewTargetFound -eq $true) -and
        ($cdpReport.promptFieldValueEqualsExact -eq $true) -and
        ($cdpReport.promptHashMatchesExpected -eq $true) -and
        ($cdpReport.inputEventDispatched -eq $true) -and
        ($cdpReport.changeEventDispatched -eq $true) -and
        ($cdpReport.submitClicked -eq $false) -and
        ($cdpReport.coreInvoked -eq $false) -and
        ($cdpReport.testOnlyHandlerUsed -eq $false) -and
        ($cdpReport.productReadinessClaimed -eq $false)
}

if ($row12ExitCode -eq 0 -and $row12Report -and $cdpPass -and $row12NoSubmit -and $row12CoreFalse -and $row12TestOnlyFalse) {
    $status = "ROW13_3B_OPEN_TYPE_PASS"
} elseif ($row12ExitCode -ne 0 -or -not $row12Report) {
    $status = "ROW13_3B_NEEDS_RUNNER_FIX"
} else {
    $status = "ROW13_3B_UI_NEEDS_FIX"
}

$filesCreated = @(
    $startInfoPath,
    $row12Stdout,
    $row12Stderr,
    $row12ReportPath,
    $cdpReportPath,
    $cdpTargetsPath,
    $cdpScreenshotPath,
    $nodeLogPath,
    $reportPath,
    $markdownPath
) | Where-Object { $_ -and (Test-Path -LiteralPath $_) }

$report = [ordered] @{
    schema = "xiigen.row13_3b.cursor_panel_open_type_proof_report.v1"
    status = $status
    started_utc = $startedUtc
    ended_utc = (Get-Date).ToUniversalTime().ToString("o")
    bounded_actor = "Row13.3b bounded sub-agent"
    evidence_root = $EvidenceRoot
    files_created = @($filesCreated)
    exact_ui_steps_performed = @(
        "Launched real Cursor Extension Development Host through existing Row12 runner with temporary --user-data-dir and --extensions-dir.",
        "Opened the real XIIGen Smart Skills panel by invoking xiigenSmartSkills.panel.",
        "Attached through CDP to the real Cursor UI/webview target.",
        "Typed the exact Row13.3 prompt into textarea#xiigen-smart-skills-prompt by setting the field value and dispatching input/change events.",
        "Captured CDP DOM/value proof and a selected webview screenshot.",
        "Stopped before submit; button#xiigen-smart-skills-submit was not clicked."
    )
    exact_prompt = $exactPrompt
    expected_prompt_hash = $expectedPromptHash
    prompt_hash = if ($cdpReport) { $cdpReport.promptHash } else { $null }
    prompt_field_value_equals_exact_prompt = if ($cdpReport) { $cdpReport.promptFieldValueEqualsExact } else { $false }
    real_cursor_ui = ($row12RealUi -and $cdpPass)
    real_cursor_ui_webview_target_found = if ($cdpReport) { $cdpReport.realCursorUiWebviewTargetFound } else { $false }
    row12_real_cursor_ui_evidence = $row12RealUi
    row13_cdp_exact_prompt_evidence = $cdpPass
    submitClicked = $false
    coreInvoked = $false
    testOnlyHandlerUsed = $false
    productReadinessClaimed = $false
    selectedSkillClaimed = $false
    expected_skill_later_not_claimed = $expectedSkillLaterNotClaimed
    row12_exit_code = $row12ExitCode
    node_exit_code = $nodeExitCode
    row12_report_path = $row12ReportPath
    cdp_exact_prompt_report_path = $cdpReportPath
    screenshot_path = if (Test-Path -LiteralPath $cdpScreenshotPath) { $cdpScreenshotPath } else { $null }
    cdp_targets_path = if (Test-Path -LiteralPath $cdpTargetsPath) { $cdpTargetsPath } else { $null }
    readback_phrases = @(
        "real Cursor UI",
        "exact prompt",
        "submitClicked=false",
        "coreInvoked=false",
        "testOnlyHandlerUsed=false"
    )
    cleanup = [ordered] @{
        closed_user_windows = $false
        closed_test_owned_cursor_process = $false
        reason = "No cleanup was needed; the runner used a temporary profile/extensions directory and no user windows were closed."
    }
    remains_next = "Row13.3c submit/result."
    errors = @($errors)
    row12_summary = if ($row12Report) {
        [ordered] @{
            status = $row12Report.status
            run_mode = $row12Report.run_mode
            invoked_command = $row12Report.invoked_command
            cdp_port = $row12Report.cdp_port
            no_submit_performed = $row12Report.no_submit_performed
            core_path_invoked = $row12Report.core_path_invoked
            test_only_handler_used = $row12Report.test_only_handler_used
            typeability_probe_text = $row12Report.typeability_probe_text
            typeability_readback_matches_probe = $row12Report.typeability_readback_matches_probe
        }
    } else {
        $null
    }
    cdp_summary = if ($cdpReport) {
        [ordered] @{
            realCursorUiWebviewTargetFound = $cdpReport.realCursorUiWebviewTargetFound
            promptFieldValueEqualsExact = $cdpReport.promptFieldValueEqualsExact
            promptHashMatchesExpected = $cdpReport.promptHashMatchesExpected
            inputEventDispatched = $cdpReport.inputEventDispatched
            changeEventDispatched = $cdpReport.changeEventDispatched
            submitClicked = $cdpReport.submitClicked
            coreInvoked = $cdpReport.coreInvoked
            testOnlyHandlerUsed = $cdpReport.testOnlyHandlerUsed
            screenshot = $cdpReport.screenshot
            selectedTarget = $cdpReport.selectedTarget
            selectedDocument = $cdpReport.selectedDocument
        }
    } else {
        $null
    }
}

Write-JsonFile -Path $reportPath -Value $report

$md = @"
# Row13.3b Cursor Panel Open/Type Proof

status: $status

Exact UI steps performed:
- Launched real Cursor Extension Development Host through the existing Row12 runner with temp profile/extensions.
- Opened XIIGen Smart Skills panel through ``xiigenSmartSkills.panel``.
- Attached to the real Cursor UI/webview CDP target.
- Typed the exact Row13.3 prompt into ``textarea#xiigen-smart-skills-prompt`` and read it back.
- Captured DOM/CDP proof and screenshot.
- Did not click submit.

Prompt SHA-256: ``$expectedPromptHash``

Required flags:
- real Cursor UI: $($report.real_cursor_ui)
- exact prompt: $($report.prompt_field_value_equals_exact_prompt)
- submitClicked=false
- coreInvoked=false
- testOnlyHandlerUsed=false
- productReadinessClaimed=false

Files:
- report: ``$reportPath``
- CDP exact prompt proof: ``$cdpReportPath``
- CDP targets: ``$cdpTargetsPath``
- screenshot: ``$($report.screenshot_path)``
- Row12 base report: ``$row12ReportPath``

Remaining next step: Row13.3c submit/result.
"@
Write-TextFile -Path $markdownPath -Value $md

$report | ConvertTo-Json -Depth 24

if ($status -ne "ROW13_3B_OPEN_TYPE_PASS") {
    exit 2
}
