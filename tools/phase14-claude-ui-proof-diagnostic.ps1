param(
    [string]$OutputRoot = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\claude-ui-proof"
)

$ErrorActionPreference = "Stop"

function New-DirectoryIfMissing {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        New-Item -ItemType Directory -Path $Path | Out-Null
    }
}

function Write-Utf8NoBomLfFile {
    param(
        [string]$Path,
        [string]$Content
    )

    $normalized = $Content -replace "`r`n", "`n"
    $normalized = $normalized -replace "`r", "`n"
    $normalized = [regex]::Replace($normalized, "[ `t]+(?=`n)", "")
    if (-not $normalized.EndsWith("`n")) {
        $normalized += "`n"
    }

    $encoding = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $normalized, $encoding)
}

function ConvertTo-PlainObject {
    param($Value)
    if ($null -eq $Value) { return $null }
    return $Value
}

$runId = "p14-fix-01-claude-ui-proof-" + (Get-Date -Format "yyyyMMdd-HHmmss")
$runDir = Join-Path $OutputRoot $runId
New-DirectoryIfMissing -Path $OutputRoot
New-DirectoryIfMissing -Path $runDir

$checks = New-Object System.Collections.Generic.List[object]

$claudeCommand = Get-Command claude -ErrorAction SilentlyContinue
$checks.Add([pscustomobject]@{
    check = "claude_command_in_path"
    command = "Get-Command claude -ErrorAction SilentlyContinue"
    result = if ($claudeCommand) { "FOUND" } else { "NOT_FOUND" }
    evidence = if ($claudeCommand) {
        [pscustomobject]@{
            name = $claudeCommand.Name
            source = $claudeCommand.Source
            command_type = $claudeCommand.CommandType.ToString()
        }
    } else {
        "No shell-resolvable claude command was found in PATH for a safe non-AI CLI check."
    }
})

$processes = @(Get-CimInstance Win32_Process | Where-Object {
    $_.Name -match "^(?i:claude|anthropic)" -or
    $_.ExecutablePath -match "\\WindowsApps\\Claude_|\\AppData\\Roaming\\Claude\\claude-code\\"
})
$processSummaries = foreach ($p in $processes) {
    $cmd = [string]$p.CommandLine
    [pscustomobject]@{
        process_id = $p.ProcessId
        name = $p.Name
        executable_path = $p.ExecutablePath
        is_desktop_windows_app = ($p.ExecutablePath -match "\\WindowsApps\\Claude_")
        is_claude_code_agent = ($p.ExecutablePath -match "\\claude-code\\" -or $cmd -match "--output-format")
        has_renderer_or_utility_type = ($cmd -match "--type=")
        has_plugin_dir_flag = ($cmd -match "--plugin-dir")
        has_remote_debugging_flag = ($cmd -match "--remote-debugging")
        command_line_redacted = "redacted: flags above only"
    }
}
$checks.Add([pscustomobject]@{
    check = "claude_process_discovery"
    command = "Get-CimInstance Win32_Process | Where-Object { Name/ExecutablePath match Claude Desktop or Claude Code }"
    result = if ($processSummaries.Count -gt 0) { "FOUND" } else { "NOT_FOUND" }
    evidence = $processSummaries
})

$startApps = @()
if (Get-Command Get-StartApps -ErrorAction SilentlyContinue) {
    $startApps = @(Get-StartApps | Where-Object {
        $_.Name -match "Claude|Anthropic" -or $_.AppID -match "Claude|Anthropic"
    } | Select-Object Name, AppID)
}
$checks.Add([pscustomobject]@{
    check = "windows_start_app_discovery"
    command = "Get-StartApps | Where-Object { Name/AppID match Claude|Anthropic }"
    result = if ($startApps.Count -gt 0) { "FOUND" } else { "NOT_FOUND" }
    evidence = $startApps
})

$candidatePaths = @(
    "$env:APPDATA\Claude",
    "$env:LOCALAPPDATA\Claude",
    "$env:LOCALAPPDATA\AnthropicClaude",
    "$env:LOCALAPPDATA\Programs\Claude",
    "$env:ProgramFiles\Claude",
    "${env:ProgramFiles(x86)}\Claude"
) | Where-Object { $_ -and $_.Trim().Length -gt 0 }
$pathEvidence = foreach ($path in $candidatePaths) {
    [pscustomobject]@{
        path = $path
        exists = (Test-Path -LiteralPath $path)
        type = if (Test-Path -LiteralPath $path -PathType Container) {
            "Directory"
        } elseif (Test-Path -LiteralPath $path -PathType Leaf) {
            "File"
        } else {
            "Missing"
        }
    }
}
$checks.Add([pscustomobject]@{
    check = "claude_known_local_paths"
    command = "Test-Path for common Claude app/config locations"
    result = if (($pathEvidence | Where-Object { $_.exists }).Count -gt 0) { "FOUND" } else { "NOT_FOUND" }
    evidence = $pathEvidence
})

$windowEvidence = @(Get-Process -Name claude -ErrorAction SilentlyContinue | ForEach-Object {
    [pscustomobject]@{
        process_id = $_.Id
        process_name = $_.ProcessName
        has_main_window_handle = ($_.MainWindowHandle -ne 0)
        title_status = if ([string]::IsNullOrWhiteSpace($_.MainWindowTitle)) { "empty_or_not_exposed" } else { "nonempty_redacted" }
        title_redacted = if ([string]::IsNullOrWhiteSpace($_.MainWindowTitle)) { "" } else { "[redacted]" }
    }
})
$checks.Add([pscustomobject]@{
    check = "claude_visible_window_discovery"
    command = "Get-Process -Name claude | inspect MainWindowHandle/MainWindowTitle with title redaction"
    result = if ((($windowEvidence | Where-Object { $_.has_main_window_handle -eq $true }) | Measure-Object).Count -gt 0) { "WINDOW_FOUND" } else { "NO_MAIN_WINDOW_HANDLE_FOUND" }
    evidence = $windowEvidence
})

$remoteDebugEvidence = [pscustomobject]@{
    remote_debugging_flag_found = (($processSummaries | Where-Object { $_.has_remote_debugging_flag }).Count -gt 0)
    process_count_checked = $processSummaries.Count
}
$checks.Add([pscustomobject]@{
    check = "claude_remote_debugging_discovery"
    command = "Inspect redacted Claude process command lines for --remote-debugging"
    result = if ($remoteDebugEvidence.remote_debugging_flag_found) { "FOUND" } else { "NOT_FOUND" }
    evidence = $remoteDebugEvidence
})

$protocolEvidence = @()
foreach ($key in @("HKCU:\Software\Classes\claude", "HKCR:\claude")) {
    $exists = Test-Path -LiteralPath $key
    $protocolEvidence += [pscustomobject]@{
        registry_key = $key
        exists = $exists
        note = if ($exists) { "protocol key exists; not proof of prompt submission or core invocation" } else { "missing" }
    }
}
$checks.Add([pscustomobject]@{
    check = "claude_uri_protocol_discovery"
    command = "Test-Path HKCU/HKCR claude protocol keys"
    result = if ((($protocolEvidence | Where-Object { $_.exists -eq $true }) | Measure-Object).Count -gt 0) { "FOUND" } else { "NOT_FOUND" }
    evidence = $protocolEvidence
})

$packet = [pscustomobject]@{
    packet_schema = "phase14.claude_ui_proof_diagnostic.v1"
    task_id = "P14-FIX-01"
    fr_id = "FR-01"
    run_id = $runId
    timestamp_utc = (Get-Date).ToUniversalTime().ToString("o")
    cwd = (Get-Location).Path
    constraints = [pscustomobject]@{
        no_external_ai_calls_attempted = $true
        no_web_browsing_attempted = $true
        no_git_commands_attempted = $true
        product_extension_files_edited = $false
        generated_under_allowed_scope = $true
    }
    distinction = [pscustomobject]@{
        handler_or_package_proof_status = "evidence_only"
        real_claude_desktop_or_app_ui_proof_status = "not_proven_by_this_run"
        required_for_pass = @(
            "prompt accepted by visible Claude Desktop/app surface",
            "same-run thin adapter invocation into llm_mvp_core",
            "user-visible answer/error/debug with correlation id",
            "redacted input/output and freshness",
            "controlled negative path"
        )
    }
    checks = $checks
    proof_gap = [pscustomobject]@{
        verdict = "NEEDS_FIX"
        blocker = "Claude Desktop/app availability was found, but this Codex environment did not expose a safe controllable Claude UI/app surface for submitting a prompt, capturing a visible answer/error, and tying it to a same-run llm_mvp_core adapter/core correlation id. No prompt was sent."
        smallest_next_bounded_work_order = "Run an operator-approved Claude Desktop/app UI capture in the active Claude app session using a test-only XIIGen prompt and negative prompt, with screen/output redaction, same-run correlation id from the adapter/core trace, and no git authority."
    }
}

$jsonPath = Join-Path $runDir "claude-ui-proof-gap-diagnostic.json"
Write-Utf8NoBomLfFile -Path $jsonPath -Content ($packet | ConvertTo-Json -Depth 20)

[pscustomobject]@{
    run_id = $runId
    output_dir = $runDir
    json_path = $jsonPath
    verdict = "NEEDS_FIX"
} | ConvertTo-Json -Depth 5
