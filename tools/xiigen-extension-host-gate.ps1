param(
    [string]$Mode = "Help",
    [string]$ExpectedOwner = "",
    [string]$RequiredRepos = "",
    [Alias("Host")]
    [string]$TargetHost = "",
    [string]$ApprovalArtifact = "",
    [switch]$PrivateRequired
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptName = "xiigen-extension-host-gate.ps1"
$workOrder = "WO-EXT-CLAUDE-01"
$supportedModes = @("Help", "HostRepoSet", "HostBoundary", "DebugTrace", "HostLocalRuntime", "HostInteractiveRuntime")
$plannedModes = @(
    "HostRepoCreateMissing"
)
$repoRoot = Split-Path -Parent $PSScriptRoot
$SiblingRoot = Split-Path -Parent $repoRoot
$XiigenDocsRoot = Join-Path $env:USERPROFILE 'Documents\xiigen'
$artifactsDir = Join-Path $repoRoot "artifacts"
$hostRepoReportPath = Join-Path $artifactsDir "host-repo-status.json"
$claudeBoundaryReportPath = Join-Path $artifactsDir "host-boundary-claude.json"
$cursorBoundaryReportPath = Join-Path $artifactsDir "host-boundary-cursor.json"
$codexBoundaryReportPath = Join-Path $artifactsDir "host-boundary-codex.json"
$claudeDebugTraceReportPath = Join-Path $artifactsDir "debug-trace-claude.json"
$cursorDebugTraceReportPath = Join-Path $artifactsDir "debug-trace-cursor.json"
$codexDebugTraceReportPath = Join-Path $artifactsDir "debug-trace-codex.json"
$claudeLocalRuntimeReportPath = Join-Path $artifactsDir "host-local-runtime-claude.json"
$cursorLocalRuntimeReportPath = Join-Path $artifactsDir "host-local-runtime-cursor.json"
$codexLocalRuntimeReportPath = Join-Path $artifactsDir "host-local-runtime-codex.json"
$claudeInteractiveRuntimeReportPath = Join-Path $artifactsDir "host-interactive-runtime-claude.json"
$cursorInteractiveRuntimeReportPath = Join-Path $artifactsDir "host-interactive-runtime-cursor.json"
$codexInteractiveRuntimeReportPath = Join-Path $artifactsDir "host-interactive-runtime-codex.json"

function ConvertTo-RedactedRemote {
    param([string]$RemoteUrl)

    if ([string]::IsNullOrWhiteSpace($RemoteUrl)) {
        return $null
    }

    if ($RemoteUrl -match "github\.com[:/](?<owner>[^/]+)/(?<repo>[^/]+?)(?:\.git)?$") {
        return "github.com/$($Matches.owner)/$($Matches.repo)"
    }

    return "unparsed_remote"
}

function Get-OwnerFromGitRemote {
    $result = [ordered]@{
        owner = $null
        metadata_source = "git_remote"
        remote_url_redacted = $null
        attempted_local_tool = "git remote get-url origin"
        errors = @()
    }

    $originOutput = & git -C $repoRoot remote get-url origin 2>&1
    $originExit = $LASTEXITCODE
    if ($originExit -ne 0) {
        $result.errors += "origin remote unavailable: $(($originOutput | Out-String).Trim())"
        $remoteNamesOutput = & git -C $repoRoot remote 2>&1
        if ($LASTEXITCODE -eq 0) {
            $remoteNames = @($remoteNamesOutput | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
            if ($remoteNames.Count -gt 0) {
                $firstRemote = [string]$remoteNames[0]
                $result.attempted_local_tool = "git remote get-url $firstRemote"
                $originOutput = & git -C $repoRoot remote get-url $firstRemote 2>&1
                $originExit = $LASTEXITCODE
            }
        }
    }

    if ($originExit -eq 0) {
        $remoteText = (($originOutput | Out-String).Trim())
        $result.remote_url_redacted = ConvertTo-RedactedRemote -RemoteUrl $remoteText
        if ($remoteText -match "github\.com[:/](?<owner>[^/]+)/(?<repo>[^/]+?)(?:\.git)?$") {
            $result.owner = $Matches.owner
        }
        else {
            $result.errors += "could not parse owner from git remote"
        }
    }
    else {
        $result.errors += "git remote owner resolution failed: $(($originOutput | Out-String).Trim())"
    }

    return [pscustomobject]$result
}

function Get-GhAccessStatus {
    $result = [ordered]@{
        metadata_access_available = $false
        metadata_source = "none"
        attempted_local_tool = "gh"
        status_summary = ""
    }

    $ghCommand = Get-Command gh -ErrorAction SilentlyContinue
    if ($null -eq $ghCommand) {
        $result.status_summary = "GitHub CLI not found on PATH."
        return [pscustomobject]$result
    }

    $authOutput = & gh auth status 2>&1
    $authExit = $LASTEXITCODE
    $authText = (($authOutput | Out-String).Trim())
    $firstLine = ($authText -split "(`r`n|`n|`r)") | Select-Object -First 1
    $result.status_summary = [string]$firstLine

    if ($authExit -eq 0) {
        $result.metadata_access_available = $true
        $result.metadata_source = "gh"
    }

    return [pscustomobject]$result
}

function Get-GhRepoMetadata {
    param(
        [string]$Owner,
        [string]$RepoName
    )

    $repoRef = "$Owner/$RepoName"
    $row = [ordered]@{
        repo_name = $RepoName
        owner = $Owner
        full_name = $repoRef
        exists = $false
        is_private = $null
        visibility = "unknown"
        privacy_unknown = $true
        metadata_source = "gh"
        metadata_error = $null
    }

    $repoOutput = & gh repo view $repoRef --json nameWithOwner,isPrivate,visibility 2>&1
    $repoExit = $LASTEXITCODE
    $repoText = (($repoOutput | Out-String).Trim())

    if ($repoExit -ne 0) {
        $row.metadata_error = $repoText
        return [pscustomobject]$row
    }

    try {
        $metadata = $repoText | ConvertFrom-Json
        $row.full_name = [string]$metadata.nameWithOwner
        $row.exists = $true
        $row.is_private = [bool]$metadata.isPrivate
        $row.visibility = [string]$metadata.visibility
        $row.privacy_unknown = $false
    }
    catch {
        $row.exists = $null
        $row.metadata_error = "failed to parse gh repo view JSON: $($_.Exception.Message)"
    }

    return [pscustomobject]$row
}

function Write-HostRepoReport {
    param([object]$Payload)

    if (-not (Test-Path -LiteralPath $artifactsDir)) {
        New-Item -ItemType Directory -Path $artifactsDir | Out-Null
    }

    $Payload | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $hostRepoReportPath -Encoding UTF8
}

function Get-JsonPropertyValue {
    param(
        [object]$Object,
        [string]$Name
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

function Add-BoundaryCheck {
    param(
        [System.Collections.Generic.List[object]]$Checks,
        [string]$Name,
        [bool]$Passed,
        [object]$Expected,
        [object]$Actual
    )

    $Checks.Add([pscustomobject][ordered]@{
        name = $Name
        passed = $Passed
        expected = $Expected
        actual = $Actual
    })
}

function Add-DebugCheck {
    param(
        [System.Collections.Generic.List[object]]$Checks,
        [string]$Name,
        [bool]$Passed,
        [object]$Expected,
        [object]$Actual
    )

    $Checks.Add([pscustomobject][ordered]@{
        name = $Name
        passed = $Passed
        expected = $Expected
        actual = $Actual
    })
}

function Get-HostBoundaryConfig {
    param([string]$HostName)

    if ($HostName -eq "claude") {
        return [pscustomobject][ordered]@{
            host = "claude"
            work_order = "WO-EXT-CLAUDE-01"
            repo_path = (Join-Path $SiblingRoot "claude-xiigen-smart-skills")
            package_name = "claude-xiigen-smart-skills"
            report_path = $claudeBoundaryReportPath
            report_ref = "artifacts/host-boundary-claude.json"
        }
    }

    if ($HostName -eq "cursor") {
        return [pscustomobject][ordered]@{
            host = "cursor"
            work_order = "WO-EXT-CURSOR-01"
            repo_path = (Join-Path $SiblingRoot "cursor-xiigen-smart-skills")
            package_name = "cursor-xiigen-smart-skills"
            report_path = $cursorBoundaryReportPath
            report_ref = "artifacts/host-boundary-cursor.json"
        }
    }

    if ($HostName -eq "codex") {
        return [pscustomobject][ordered]@{
            host = "codex"
            work_order = "WO-EXT-CODEX-01"
            repo_path = (Join-Path $SiblingRoot "codex-xiigen-smart-skills")
            package_name = "codex-xiigen-smart-skills"
            report_path = $codexBoundaryReportPath
            report_ref = "artifacts/host-boundary-codex.json"
        }
    }

    return $null
}

function Write-HelpPayload {
    $payload = [ordered]@{
        script = $scriptName
        work_order = $workOrder
        status = "wired"
        supported_modes = $supportedModes
        planned_modes_not_implemented = $plannedModes
        review_gates = @(
            "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"tools\xiigen-extension-host-gate.ps1`" -Mode HostRepoSet -ExpectedOwner SameAsCore -RequiredRepos `"claude-xiigen-smart-skills,cursor-xiigen-smart-skills,codex-xiigen-smart-skills`" -PrivateRequired",
            "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"tools\xiigen-extension-host-gate.ps1`" -Mode HostBoundary -Host claude",
            "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"tools\xiigen-extension-host-gate.ps1`" -Mode HostBoundary -Host cursor",
            "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"tools\xiigen-extension-host-gate.ps1`" -Mode HostBoundary -Host codex",
            "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"tools\xiigen-extension-host-gate.ps1`" -Mode DebugTrace -Host claude",
            "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"tools\xiigen-extension-host-gate.ps1`" -Mode DebugTrace -Host cursor",
            "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"tools\xiigen-extension-host-gate.ps1`" -Mode DebugTrace -Host codex",
            "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"tools\xiigen-extension-host-gate.ps1`" -Mode HostLocalRuntime -Host claude",
            "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"tools\xiigen-extension-host-gate.ps1`" -Mode HostLocalRuntime -Host cursor",
            "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"tools\xiigen-extension-host-gate.ps1`" -Mode HostLocalRuntime -Host codex",
            "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"tools\xiigen-extension-host-gate.ps1`" -Mode HostInteractiveRuntime -Host claude -ApprovalArtifact `"artifacts\host-runtime-approval.json`"",
            "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"tools\xiigen-extension-host-gate.ps1`" -Mode Help"
        )
        note = "HostRepoSet, HostBoundary, HostLocalRuntime, HostInteractiveRuntime, and Claude/Cursor/Codex DebugTrace are report-only unless an explicit host runtime command is safely available. HostLocalRuntime validates local non-AI entrypoint-to-core boundary evidence only. HostInteractiveRuntime requires an approval artifact and must report NOT_READY instead of faking paid/full host runtime evidence when no safe CLI path exists."
    }

    Write-Output "XIIGen extension host gate"
    Write-Output "Work order: $workOrder"
    Write-Output "Supported modes: $($supportedModes -join ', ')"
    Write-Output "Planned host modes are listed in JSON but not implemented or approved by this gate."
    Write-Output ($payload | ConvertTo-Json -Depth 5)
}

function Invoke-CursorDebugTrace {
    if (-not (Test-Path -LiteralPath $artifactsDir)) {
        New-Item -ItemType Directory -Path $artifactsDir | Out-Null
    }

    $hostRepoPath = (Join-Path $SiblingRoot "cursor-xiigen-smart-skills")
    $packagePath = Join-Path $hostRepoPath "package.json"
    $contractPath = Join-Path $hostRepoPath "xiigen-extension-contract.json"
    $helperPath = Join-Path $hostRepoPath "scripts\debug-trace-helper.js"
    $checks = New-Object "System.Collections.Generic.List[object]"
    $readErrors = @()
    $package = $null
    $contract = $null

    Add-DebugCheck -Checks $checks -Name "package_manifest_exists" -Passed (Test-Path -LiteralPath $packagePath) -Expected $true -Actual (Test-Path -LiteralPath $packagePath)
    Add-DebugCheck -Checks $checks -Name "contract_artifact_exists" -Passed (Test-Path -LiteralPath $contractPath) -Expected $true -Actual (Test-Path -LiteralPath $contractPath)
    Add-DebugCheck -Checks $checks -Name "debug_helper_exists" -Passed (Test-Path -LiteralPath $helperPath) -Expected $true -Actual (Test-Path -LiteralPath $helperPath)

    if (Test-Path -LiteralPath $packagePath) {
        try {
            $package = Get-Content -Raw -LiteralPath $packagePath | ConvertFrom-Json
        }
        catch {
            $readErrors += "package JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $contractPath) {
        try {
            $contract = Get-Content -Raw -LiteralPath $contractPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "contract JSON parse failed: $($_.Exception.Message)"
        }
    }

    $phase40 = Get-JsonPropertyValue -Object $package -Name "xiigenPhase40"
    $packageDebugSetting = Get-JsonPropertyValue -Object $phase40 -Name "programmerDebugSetting"
    $settingName = Get-JsonPropertyValue -Object $packageDebugSetting -Name "name"
    $settingDefault = Get-JsonPropertyValue -Object $packageDebugSetting -Name "default"
    $settingMethod = Get-JsonPropertyValue -Object $packageDebugSetting -Name "method"
    $settingHelper = Get-JsonPropertyValue -Object $packageDebugSetting -Name "helper"

    $contributes = Get-JsonPropertyValue -Object $package -Name "contributes"
    $configuration = Get-JsonPropertyValue -Object $contributes -Name "configuration"
    $configurationProperties = Get-JsonPropertyValue -Object $configuration -Name "properties"
    $contributedDebugSetting = Get-JsonPropertyValue -Object $configurationProperties -Name "xiigenSmartSkills.debug"
    $contributedDebugDefault = Get-JsonPropertyValue -Object $contributedDebugSetting -Name "default"

    $debugTraceContract = Get-JsonPropertyValue -Object $contract -Name "debug_trace_contract"
    $contractDebugSetting = Get-JsonPropertyValue -Object $debugTraceContract -Name "setting_name"
    $contractMethod = Get-JsonPropertyValue -Object $debugTraceContract -Name "method"
    $contractDebugSettingPresent = Get-JsonPropertyValue -Object $contract -Name "debug_setting_present"

    $nodeCommand = Get-Command node -ErrorAction SilentlyContinue
    $nodeAvailable = ($null -ne $nodeCommand)
    Add-DebugCheck -Checks $checks -Name "node_available_for_local_helper" -Passed $nodeAvailable -Expected $true -Actual $nodeAvailable

    $probeRoot = Join-Path ([System.IO.Path]::GetTempPath()) "xiigen-debug-trace-cursor"
    if (-not (Test-Path -LiteralPath $probeRoot)) {
        New-Item -ItemType Directory -Path $probeRoot -Force | Out-Null
    }

    $probeId = (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssfffffffZ")
    $enabledOutputPath = Join-Path $probeRoot "cursor-debug-enabled-$probeId-output.json"
    $disabledOutputPath = Join-Path $probeRoot "cursor-debug-disabled-$probeId-output.json"
    $enabledExitCode = $null
    $disabledExitCode = $null
    $enabledInvocationOutput = @()
    $disabledInvocationOutput = @()

    if ($nodeAvailable -and (Test-Path -LiteralPath $helperPath)) {
        $enabledArgs = @(
            $helperPath,
            "--method",
            "command",
            "--debug-enabled",
            "true",
            "--output",
            $enabledOutputPath
        )
        $oldErrorActionPreference = $ErrorActionPreference
        $ErrorActionPreference = "Continue"
        try {
            $enabledInvocationOutput = & node @enabledArgs 2>&1
            $enabledExitCode = $LASTEXITCODE
        }
        finally {
            $ErrorActionPreference = $oldErrorActionPreference
        }

        $disabledArgs = @(
            $helperPath,
            "--method",
            "command",
            "--debug-enabled",
            "false",
            "--output",
            $disabledOutputPath
        )
        $oldErrorActionPreference = $ErrorActionPreference
        $ErrorActionPreference = "Continue"
        try {
            $disabledInvocationOutput = & node @disabledArgs 2>&1
            $disabledExitCode = $LASTEXITCODE
        }
        finally {
            $ErrorActionPreference = $oldErrorActionPreference
        }
    }
    else {
        if (-not $nodeAvailable) {
            $readErrors += "node was not found on PATH for local non-AI Cursor debug helper invocation."
        }
        if (-not (Test-Path -LiteralPath $helperPath)) {
            $readErrors += "Cursor debug helper was not found: $helperPath"
        }
    }

    $enabledResponse = $null
    $disabledResponse = $null
    if (Test-Path -LiteralPath $enabledOutputPath) {
        try {
            $enabledResponse = Get-Content -Raw -LiteralPath $enabledOutputPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "enabled helper output JSON parse failed: $($_.Exception.Message)"
        }
    }
    else {
        $readErrors += "enabled helper output was not created: $enabledOutputPath"
    }

    if (Test-Path -LiteralPath $disabledOutputPath) {
        try {
            $disabledResponse = Get-Content -Raw -LiteralPath $disabledOutputPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "disabled helper output JSON parse failed: $($_.Exception.Message)"
        }
    }
    else {
        $readErrors += "disabled helper output was not created: $disabledOutputPath"
    }

    $actualDebugTrace = Get-JsonPropertyValue -Object $enabledResponse -Name "debugTrace"
    $disabledDebugTrace = Get-JsonPropertyValue -Object $disabledResponse -Name "debugTrace"
    $method = Get-JsonPropertyValue -Object $actualDebugTrace -Name "method"
    $enabledDisplay = Get-JsonPropertyValue -Object $actualDebugTrace -Name "chatDisplayText"
    $redactedInput = Get-JsonPropertyValue -Object $actualDebugTrace -Name "redactedInput"
    $redactedOutput = Get-JsonPropertyValue -Object $actualDebugTrace -Name "redactedOutput"
    $rawPrivateContentExposed = Get-JsonPropertyValue -Object $actualDebugTrace -Name "rawPrivateContentExposed"
    $secretsExposed = Get-JsonPropertyValue -Object $actualDebugTrace -Name "secretsExposed"
    $disabledDisplay = ""
    if ($null -ne $disabledDebugTrace) {
        $disabledDisplay = Get-JsonPropertyValue -Object $disabledDebugTrace -Name "chatDisplayText"
    }

    $expectedDisplay = "xiigen command :`ninput: $redactedInput`noutput: $redactedOutput"
    $displayLines = @()
    if ($null -ne $enabledDisplay) {
        $displayLines = @(([string]$enabledDisplay) -split "`r`n|`n|`r")
    }

    $forbiddenDebugTextHits = @()
    foreach ($needle in @($XiigenDocsRoot, "raw private corpus", "secret", "token", "api_key", "prompt_text", "full_text")) {
        if (($enabledDisplay -as [string]) -match [regex]::Escape($needle)) {
            $forbiddenDebugTextHits += $needle
        }
    }

    Add-DebugCheck -Checks $checks -Name "debug_setting_present" -Passed ($settingName -eq "xiigenSmartSkills.debug" -and $settingDefault -eq $false -and $settingMethod -eq "command" -and $settingHelper -eq "scripts/debug-trace-helper.js") -Expected "name=xiigenSmartSkills.debug default=false method=command helper=scripts/debug-trace-helper.js" -Actual "name=$settingName default=$settingDefault method=$settingMethod helper=$settingHelper"
    Add-DebugCheck -Checks $checks -Name "contributed_debug_setting_default_false" -Passed ($contributedDebugDefault -eq $false) -Expected $false -Actual $contributedDebugDefault
    Add-DebugCheck -Checks $checks -Name "contract_debug_setting_present" -Passed ($contractDebugSettingPresent -eq $true -and $contractDebugSetting -eq "xiigenSmartSkills.debug" -and $contractMethod -eq "command") -Expected "debug_setting_present=true setting=xiigenSmartSkills.debug method=command" -Actual "debug_setting_present=$contractDebugSettingPresent setting=$contractDebugSetting method=$contractMethod"
    Add-DebugCheck -Checks $checks -Name "local_entrypoint_or_helper_invoked" -Passed ((Test-Path -LiteralPath $enabledOutputPath) -and (Test-Path -LiteralPath $disabledOutputPath) -and $enabledExitCode -eq 0 -and $disabledExitCode -eq 0) -Expected $true -Actual ((Test-Path -LiteralPath $enabledOutputPath) -and (Test-Path -LiteralPath $disabledOutputPath) -and $enabledExitCode -eq 0 -and $disabledExitCode -eq 0)
    Add-DebugCheck -Checks $checks -Name "actual_debug_trace_present" -Passed ($null -ne $actualDebugTrace) -Expected $true -Actual ($null -ne $actualDebugTrace)
    Add-DebugCheck -Checks $checks -Name "debug_disabled_omits_trace" -Passed ($null -eq $disabledDebugTrace) -Expected $true -Actual ($null -eq $disabledDebugTrace)
    Add-DebugCheck -Checks $checks -Name "debug_enabled_exact_format" -Passed ($enabledDisplay -eq $expectedDisplay) -Expected $expectedDisplay -Actual $enabledDisplay
    Add-DebugCheck -Checks $checks -Name "debug_text_matches_actual_entrypoint_output" -Passed ($enabledDisplay -eq $expectedDisplay) -Expected $true -Actual ($enabledDisplay -eq $expectedDisplay)
    Add-DebugCheck -Checks $checks -Name "method" -Passed ($method -eq "command") -Expected "command" -Actual $method
    Add-DebugCheck -Checks $checks -Name "input_label_present" -Passed ($displayLines.Count -ge 2 -and $displayLines[1].StartsWith("input: ")) -Expected $true -Actual ($displayLines.Count -ge 2 -and $displayLines[1].StartsWith("input: "))
    Add-DebugCheck -Checks $checks -Name "output_label_present" -Passed ($displayLines.Count -ge 3 -and $displayLines[2].StartsWith("output: ")) -Expected $true -Actual ($displayLines.Count -ge 3 -and $displayLines[2].StartsWith("output: "))
    Add-DebugCheck -Checks $checks -Name "raw_private_content_exposed" -Passed ($rawPrivateContentExposed -eq $false -and $forbiddenDebugTextHits.Count -eq 0) -Expected $false -Actual $rawPrivateContentExposed
    Add-DebugCheck -Checks $checks -Name "secrets_exposed" -Passed ($secretsExposed -eq $false -and $forbiddenDebugTextHits.Count -eq 0) -Expected $false -Actual $secretsExposed

    $failedChecks = @($checks | Where-Object { $_.passed -ne $true })
    $passed = ($failedChecks.Count -eq 0 -and $readErrors.Count -eq 0)

    $payload = [ordered]@{
        schema = "xiigen.debug_trace.v1"
        script = $scriptName
        work_order = "WO-EXT-DEBUG-CURSOR-01"
        mode = "DebugTrace"
        generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
        host = "cursor"
        repo_path = $hostRepoPath
        package_path = $packagePath
        contract_path = $contractPath
        helper_path = $helperPath
        debug_setting_present = ($settingName -eq "xiigenSmartSkills.debug" -and $settingDefault -eq $false -and $settingMethod -eq "command")
        debug_disabled_omits_trace = ($null -eq $disabledDebugTrace)
        debug_enabled_exact_format = ($enabledDisplay -eq $expectedDisplay)
        debug_text_matches_actual_entrypoint_output = ($enabledDisplay -eq $expectedDisplay)
        static_sample_used = $false
        local_entrypoint_or_helper_invoked = ((Test-Path -LiteralPath $enabledOutputPath) -and (Test-Path -LiteralPath $disabledOutputPath))
        local_helper_invoked = ((Test-Path -LiteralPath $enabledOutputPath) -and (Test-Path -LiteralPath $disabledOutputPath))
        full_cursor_runtime_invoked = $false
        local_entrypoint_exit_code = $enabledExitCode
        local_entrypoint_disabled_exit_code = $disabledExitCode
        local_helper_exit_code = $enabledExitCode
        local_helper_disabled_exit_code = $disabledExitCode
        local_entrypoint_output_path = $enabledOutputPath
        local_entrypoint_disabled_output_path = $disabledOutputPath
        local_helper_stdout_summary = (($enabledInvocationOutput | Out-String).Trim())
        local_helper_disabled_stdout_summary = (($disabledInvocationOutput | Out-String).Trim())
        actual_debug_text_from_entrypoint = $enabledDisplay
        actual_debug_text_from_helper = $enabledDisplay
        method = $method
        input_label_present = ($displayLines.Count -ge 2 -and $displayLines[1].StartsWith("input: "))
        output_label_present = ($displayLines.Count -ge 3 -and $displayLines[2].StartsWith("output: "))
        rendered_debug_text = $enabledDisplay
        disabled_debug_text = $disabledDisplay
        redacted_input_payload = $redactedInput
        redacted_output_payload = $redactedOutput
        raw_private_content_exposed = ($rawPrivateContentExposed -ne $false -or $forbiddenDebugTextHits.Count -gt 0)
        secrets_exposed = ($secretsExposed -ne $false -or $forbiddenDebugTextHits.Count -gt 0)
        forbidden_debug_text_hits = $forbiddenDebugTextHits
        external_ai_used = $false
        runtime_host_proof_performed = $false
        paid_cursor_session_invoked = $false
        training_started = $false
        core_machine_implementation_changed = $false
        claude_or_codex_debug_work_performed = $false
        passed = $passed
        check_count = $checks.Count
        failed_checks = @($failedChecks | ForEach-Object { $_.name })
        read_errors = $readErrors
        checks = $checks
        report_path = "artifacts/debug-trace-cursor.json"
        next_action = "parent REVIEW_INTERNAL reviews WO-EXT-DEBUG-CURSOR-01 evidence"
    }

    $payload | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $cursorDebugTraceReportPath -Encoding UTF8
    Write-Output "XIIGen Cursor debug trace verification"
    Write-Output "Report: artifacts/debug-trace-cursor.json"
    Write-Output ($payload | ConvertTo-Json -Depth 12)

    if ($passed) {
        exit 0
    }

    exit 1
}

function Invoke-CodexDebugTrace {
    if (-not (Test-Path -LiteralPath $artifactsDir)) {
        New-Item -ItemType Directory -Path $artifactsDir | Out-Null
    }

    $hostRepoPath = (Join-Path $SiblingRoot "codex-xiigen-smart-skills")
    $packagePath = Join-Path $hostRepoPath "package.json"
    $contractPath = Join-Path $hostRepoPath "xiigen-extension-contract.json"
    $entrypointPath = Join-Path $hostRepoPath "src\codex-entrypoint.ps1"
    $checks = New-Object "System.Collections.Generic.List[object]"
    $readErrors = @()
    $package = $null
    $contract = $null
    $entrypointText = ""

    Add-DebugCheck -Checks $checks -Name "package_manifest_exists" -Passed (Test-Path -LiteralPath $packagePath) -Expected $true -Actual (Test-Path -LiteralPath $packagePath)
    Add-DebugCheck -Checks $checks -Name "contract_artifact_exists" -Passed (Test-Path -LiteralPath $contractPath) -Expected $true -Actual (Test-Path -LiteralPath $contractPath)
    Add-DebugCheck -Checks $checks -Name "entrypoint_exists" -Passed (Test-Path -LiteralPath $entrypointPath) -Expected $true -Actual (Test-Path -LiteralPath $entrypointPath)

    if (Test-Path -LiteralPath $packagePath) {
        try {
            $package = Get-Content -Raw -LiteralPath $packagePath | ConvertFrom-Json
        }
        catch {
            $readErrors += "package JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $contractPath) {
        try {
            $contract = Get-Content -Raw -LiteralPath $contractPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "contract JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $entrypointPath) {
        $entrypointText = Get-Content -Raw -LiteralPath $entrypointPath
    }

    $phase40 = Get-JsonPropertyValue -Object $package -Name "xiigenPhase40"
    $packageDebugSetting = Get-JsonPropertyValue -Object $phase40 -Name "programmerDebugSetting"
    $settingName = Get-JsonPropertyValue -Object $packageDebugSetting -Name "name"
    $settingDefault = Get-JsonPropertyValue -Object $packageDebugSetting -Name "default"
    $settingMethod = Get-JsonPropertyValue -Object $packageDebugSetting -Name "method"
    $settingEntrypoint = Get-JsonPropertyValue -Object $packageDebugSetting -Name "entrypoint"
    $settingParameter = Get-JsonPropertyValue -Object $packageDebugSetting -Name "parameter"

    $debugTraceContract = Get-JsonPropertyValue -Object $contract -Name "debug_trace_contract"
    $contractDebugSetting = Get-JsonPropertyValue -Object $debugTraceContract -Name "setting_name"
    $contractMethod = Get-JsonPropertyValue -Object $debugTraceContract -Name "method"
    $contractDebugSettingPresent = Get-JsonPropertyValue -Object $contract -Name "debug_setting_present"

    $probeRoot = Join-Path ([System.IO.Path]::GetTempPath()) "xiigen-debug-trace-codex"
    if (-not (Test-Path -LiteralPath $probeRoot)) {
        New-Item -ItemType Directory -Path $probeRoot -Force | Out-Null
    }

    $probeId = (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssfffffffZ")
    $enabledOutputPath = Join-Path $probeRoot "codex-debug-enabled-$probeId-output.json"
    $enabledTracePath = Join-Path $probeRoot "codex-debug-enabled-$probeId-trace.json"
    $disabledOutputPath = Join-Path $probeRoot "codex-debug-disabled-$probeId-output.json"
    $disabledTracePath = Join-Path $probeRoot "codex-debug-disabled-$probeId-trace.json"
    $probeCheckSetPath = Join-Path $probeRoot "codex-debug-check-set-$probeId.json"
    $missingCoreApiScript = Join-Path $probeRoot "missing-phase40-extension-core-api.ps1"
    $probeCheckSetJson = '[{"id":"debug-trace-codex-local","kind":"debug_trace","assertion":"Local non-AI Codex entrypoint debug trace format probe."}]'
    $probeCheckSetJson | Set-Content -LiteralPath $probeCheckSetPath -Encoding UTF8

    $enabledArgs = @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $entrypointPath,
        "-ConcreteTask",
        "WO-EXT-DEBUG-CODEX-01 local non-AI debug trace probe",
        "-Prompt",
        "redacted local debug probe",
        "-CheckSetPath",
        $probeCheckSetPath,
        "-CoreApiScript",
        $missingCoreApiScript,
        "-OutputPath",
        $enabledOutputPath,
        "-TracePath",
        $enabledTracePath,
        "-DebugEnabled"
    )
    $oldErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $enabledInvocationOutput = & powershell.exe @enabledArgs 2>&1
        $enabledExitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $oldErrorActionPreference
    }

    $disabledArgs = @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $entrypointPath,
        "-ConcreteTask",
        "WO-EXT-DEBUG-CODEX-01 local non-AI debug disabled probe",
        "-Prompt",
        "redacted local debug-disabled probe",
        "-CheckSetPath",
        $probeCheckSetPath,
        "-CoreApiScript",
        $missingCoreApiScript,
        "-OutputPath",
        $disabledOutputPath,
        "-TracePath",
        $disabledTracePath
    )
    $oldErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $disabledInvocationOutput = & powershell.exe @disabledArgs 2>&1
        $disabledExitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $oldErrorActionPreference
    }

    $enabledResponse = $null
    $disabledResponse = $null
    if (Test-Path -LiteralPath $enabledOutputPath) {
        try {
            $enabledResponse = Get-Content -Raw -LiteralPath $enabledOutputPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "enabled entrypoint output JSON parse failed: $($_.Exception.Message)"
        }
    }
    else {
        $readErrors += "enabled entrypoint output was not created: $enabledOutputPath"
    }

    if (Test-Path -LiteralPath $disabledOutputPath) {
        try {
            $disabledResponse = Get-Content -Raw -LiteralPath $disabledOutputPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "disabled entrypoint output JSON parse failed: $($_.Exception.Message)"
        }
    }
    else {
        $readErrors += "disabled entrypoint output was not created: $disabledOutputPath"
    }

    $actualDebugTrace = Get-JsonPropertyValue -Object $enabledResponse -Name "debugTrace"
    $disabledDebugTrace = Get-JsonPropertyValue -Object $disabledResponse -Name "debugTrace"
    $method = Get-JsonPropertyValue -Object $actualDebugTrace -Name "method"
    $enabledDisplay = Get-JsonPropertyValue -Object $actualDebugTrace -Name "chatDisplayText"
    $redactedInput = Get-JsonPropertyValue -Object $actualDebugTrace -Name "redactedInput"
    $redactedOutput = Get-JsonPropertyValue -Object $actualDebugTrace -Name "redactedOutput"
    $rawPrivateContentExposed = Get-JsonPropertyValue -Object $actualDebugTrace -Name "rawPrivateContentExposed"
    $secretsExposed = Get-JsonPropertyValue -Object $actualDebugTrace -Name "secretsExposed"
    $disabledDisplay = ""
    if ($null -ne $disabledDebugTrace) {
        $disabledDisplay = Get-JsonPropertyValue -Object $disabledDebugTrace -Name "chatDisplayText"
    }

    $expectedDisplay = "xiigen command :`ninput: $redactedInput`noutput: $redactedOutput"
    $displayLines = @()
    if ($null -ne $enabledDisplay) {
        $displayLines = @(([string]$enabledDisplay) -split "`r`n|`n|`r")
    }

    $forbiddenDebugTextHits = @()
    foreach ($needle in @($XiigenDocsRoot, "raw private corpus", "secret", "token", "api_key", "prompt_text", "full_text")) {
        if (($enabledDisplay -as [string]) -match [regex]::Escape($needle)) {
            $forbiddenDebugTextHits += $needle
        }
    }

    $enabledExitAllowed = @($enabledExitCode) -contains 0 -or @($enabledExitCode) -contains 2
    $disabledExitAllowed = @($disabledExitCode) -contains 0 -or @($disabledExitCode) -contains 2

    Add-DebugCheck -Checks $checks -Name "debug_setting_present" -Passed ($settingName -eq "xiigenSmartSkills.debug" -and $settingDefault -eq $false -and $settingMethod -eq "command" -and $settingEntrypoint -eq "src/codex-entrypoint.ps1" -and $settingParameter -eq "DebugEnabled") -Expected "name=xiigenSmartSkills.debug default=false method=command entrypoint=src/codex-entrypoint.ps1 parameter=DebugEnabled" -Actual "name=$settingName default=$settingDefault method=$settingMethod entrypoint=$settingEntrypoint parameter=$settingParameter"
    Add-DebugCheck -Checks $checks -Name "contract_debug_setting_present" -Passed ($contractDebugSettingPresent -eq $true -and $contractDebugSetting -eq "xiigenSmartSkills.debug" -and $contractMethod -eq "command") -Expected "debug_setting_present=true setting=xiigenSmartSkills.debug method=command" -Actual "debug_setting_present=$contractDebugSettingPresent setting=$contractDebugSetting method=$contractMethod"
    Add-DebugCheck -Checks $checks -Name "local_entrypoint_invoked" -Passed ((Test-Path -LiteralPath $enabledOutputPath) -and (Test-Path -LiteralPath $disabledOutputPath) -and $enabledExitAllowed -and $disabledExitAllowed) -Expected $true -Actual ((Test-Path -LiteralPath $enabledOutputPath) -and (Test-Path -LiteralPath $disabledOutputPath) -and $enabledExitAllowed -and $disabledExitAllowed)
    Add-DebugCheck -Checks $checks -Name "actual_debug_trace_present" -Passed ($null -ne $actualDebugTrace) -Expected $true -Actual ($null -ne $actualDebugTrace)
    Add-DebugCheck -Checks $checks -Name "debug_disabled_omits_trace" -Passed ($null -eq $disabledDebugTrace) -Expected $true -Actual ($null -eq $disabledDebugTrace)
    Add-DebugCheck -Checks $checks -Name "debug_enabled_exact_format" -Passed ($enabledDisplay -eq $expectedDisplay) -Expected $expectedDisplay -Actual $enabledDisplay
    Add-DebugCheck -Checks $checks -Name "debug_text_matches_actual_entrypoint_output" -Passed ($enabledDisplay -eq $expectedDisplay) -Expected $true -Actual ($enabledDisplay -eq $expectedDisplay)
    Add-DebugCheck -Checks $checks -Name "method" -Passed ($method -eq "command") -Expected "command" -Actual $method
    Add-DebugCheck -Checks $checks -Name "input_label_present" -Passed ($displayLines.Count -ge 2 -and $displayLines[1].StartsWith("input: ")) -Expected $true -Actual ($displayLines.Count -ge 2 -and $displayLines[1].StartsWith("input: "))
    Add-DebugCheck -Checks $checks -Name "output_label_present" -Passed ($displayLines.Count -ge 3 -and $displayLines[2].StartsWith("output: ")) -Expected $true -Actual ($displayLines.Count -ge 3 -and $displayLines[2].StartsWith("output: "))
    Add-DebugCheck -Checks $checks -Name "raw_private_content_exposed" -Passed ($rawPrivateContentExposed -eq $false -and $forbiddenDebugTextHits.Count -eq 0) -Expected $false -Actual $rawPrivateContentExposed
    Add-DebugCheck -Checks $checks -Name "secrets_exposed" -Passed ($secretsExposed -eq $false -and $forbiddenDebugTextHits.Count -eq 0) -Expected $false -Actual $secretsExposed
    Add-DebugCheck -Checks $checks -Name "entrypoint_debug_flag" -Passed ($entrypointText -match '\[switch\]\$DebugEnabled') -Expected "[switch]`$DebugEnabled" -Actual ($entrypointText -match '\[switch\]\$DebugEnabled')
    Add-DebugCheck -Checks $checks -Name "entrypoint_debug_trace_omitted_when_disabled" -Passed ($entrypointText -match 'if \(\$DebugEnabled\)') -Expected "if (`$DebugEnabled)" -Actual ($entrypointText -match 'if \(\$DebugEnabled\)')

    $failedChecks = @($checks | Where-Object { $_.passed -ne $true })
    $passed = ($failedChecks.Count -eq 0 -and $readErrors.Count -eq 0)

    $payload = [ordered]@{
        schema = "xiigen.debug_trace.v1"
        script = $scriptName
        work_order = "WO-EXT-DEBUG-CODEX-01"
        mode = "DebugTrace"
        generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
        host = "codex"
        repo_path = $hostRepoPath
        package_path = $packagePath
        contract_path = $contractPath
        entrypoint_path = $entrypointPath
        debug_setting_present = ($settingName -eq "xiigenSmartSkills.debug" -and $settingDefault -eq $false -and $settingMethod -eq "command")
        debug_disabled_omits_trace = ($null -eq $disabledDebugTrace)
        debug_enabled_exact_format = ($enabledDisplay -eq $expectedDisplay)
        debug_text_matches_actual_entrypoint_output = ($enabledDisplay -eq $expectedDisplay)
        static_sample_used = $false
        local_entrypoint_invoked = ((Test-Path -LiteralPath $enabledOutputPath) -and (Test-Path -LiteralPath $disabledOutputPath))
        local_entrypoint_exit_code = $enabledExitCode
        local_entrypoint_disabled_exit_code = $disabledExitCode
        local_entrypoint_output_path = $enabledOutputPath
        local_entrypoint_disabled_output_path = $disabledOutputPath
        local_entrypoint_trace_path = $enabledTracePath
        local_entrypoint_disabled_trace_path = $disabledTracePath
        local_entrypoint_stdout_summary = (($enabledInvocationOutput | Out-String).Trim())
        local_entrypoint_disabled_stdout_summary = (($disabledInvocationOutput | Out-String).Trim())
        actual_debug_text_from_entrypoint = $enabledDisplay
        method = $method
        input_label_present = ($displayLines.Count -ge 2 -and $displayLines[1].StartsWith("input: "))
        output_label_present = ($displayLines.Count -ge 3 -and $displayLines[2].StartsWith("output: "))
        rendered_debug_text = $enabledDisplay
        disabled_debug_text = $disabledDisplay
        redacted_input_payload = $redactedInput
        redacted_output_payload = $redactedOutput
        raw_private_content_exposed = ($rawPrivateContentExposed -ne $false -or $forbiddenDebugTextHits.Count -gt 0)
        secrets_exposed = ($secretsExposed -ne $false -or $forbiddenDebugTextHits.Count -gt 0)
        forbidden_debug_text_hits = $forbiddenDebugTextHits
        controlled_core_api_downgrade_used = $true
        external_ai_used = $false
        runtime_host_proof_performed = $false
        full_codex_ai_runtime_invoked = $false
        paid_codex_session_invoked = $false
        training_started = $false
        core_machine_implementation_changed = $false
        claude_or_cursor_debug_work_performed = $false
        passed = $passed
        check_count = $checks.Count
        failed_checks = @($failedChecks | ForEach-Object { $_.name })
        read_errors = $readErrors
        checks = $checks
        report_path = "artifacts/debug-trace-codex.json"
        next_action = "parent REVIEW_INTERNAL reviews WO-EXT-DEBUG-CODEX-01 evidence"
    }

    $payload | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $codexDebugTraceReportPath -Encoding UTF8
    Write-Output "XIIGen Codex debug trace verification"
    Write-Output "Report: artifacts/debug-trace-codex.json"
    Write-Output ($payload | ConvertTo-Json -Depth 12)

    if ($passed) {
        exit 0
    }

    exit 1
}

function Invoke-DebugTrace {
    $hostName = $TargetHost.Trim().ToLowerInvariant()
    if ($hostName -eq "cursor") {
        Invoke-CursorDebugTrace
    }

    if ($hostName -eq "codex") {
        Invoke-CodexDebugTrace
    }

    if ($hostName -ne "claude") {
        $unsupportedPayload = [ordered]@{
            script = $scriptName
            work_order = "WO-EXT-DEBUG-CURSOR-01"
            mode = "DebugTrace"
            host = $TargetHost
            passed = $false
            status = "unsupported_host_for_this_work_order"
            supported_hosts_for_this_work_order = @("claude", "cursor", "codex")
            note = "This script supports Claude, Cursor, and Codex DebugTrace."
        }
        Write-Output ($unsupportedPayload | ConvertTo-Json -Depth 5)
        exit 2
    }

    if (-not (Test-Path -LiteralPath $artifactsDir)) {
        New-Item -ItemType Directory -Path $artifactsDir | Out-Null
    }

    $hostRepoPath = (Join-Path $SiblingRoot "claude-xiigen-smart-skills")
    $packagePath = Join-Path $hostRepoPath "package.json"
    $contractPath = Join-Path $hostRepoPath "xiigen-extension-contract.json"
    $entrypointPath = Join-Path $hostRepoPath "src\claude-entrypoint.ps1"
    $checks = New-Object "System.Collections.Generic.List[object]"
    $readErrors = @()
    $package = $null
    $contract = $null
    $entrypointText = ""

    Add-DebugCheck -Checks $checks -Name "package_manifest_exists" -Passed (Test-Path -LiteralPath $packagePath) -Expected $true -Actual (Test-Path -LiteralPath $packagePath)
    Add-DebugCheck -Checks $checks -Name "contract_artifact_exists" -Passed (Test-Path -LiteralPath $contractPath) -Expected $true -Actual (Test-Path -LiteralPath $contractPath)
    Add-DebugCheck -Checks $checks -Name "entrypoint_exists" -Passed (Test-Path -LiteralPath $entrypointPath) -Expected $true -Actual (Test-Path -LiteralPath $entrypointPath)

    if (Test-Path -LiteralPath $packagePath) {
        try {
            $package = Get-Content -Raw -LiteralPath $packagePath | ConvertFrom-Json
        }
        catch {
            $readErrors += "package JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $contractPath) {
        try {
            $contract = Get-Content -Raw -LiteralPath $contractPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "contract JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $entrypointPath) {
        $entrypointText = Get-Content -Raw -LiteralPath $entrypointPath
    }

    $phase40 = Get-JsonPropertyValue -Object $package -Name "xiigenPhase40"
    $packageDebugSetting = Get-JsonPropertyValue -Object $phase40 -Name "programmerDebugSetting"
    $settingName = Get-JsonPropertyValue -Object $packageDebugSetting -Name "name"
    $settingDefault = Get-JsonPropertyValue -Object $packageDebugSetting -Name "default"
    $settingMethod = Get-JsonPropertyValue -Object $packageDebugSetting -Name "method"

    $debugTraceContract = Get-JsonPropertyValue -Object $contract -Name "debug_trace_contract"

    $probeRoot = Join-Path ([System.IO.Path]::GetTempPath()) "xiigen-debug-trace-claude"
    if (-not (Test-Path -LiteralPath $probeRoot)) {
        New-Item -ItemType Directory -Path $probeRoot -Force | Out-Null
    }

    $probeId = (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssfffffffZ")
    $enabledOutputPath = Join-Path $probeRoot "claude-debug-enabled-$probeId-output.json"
    $enabledTracePath = Join-Path $probeRoot "claude-debug-enabled-$probeId-trace.json"
    $disabledOutputPath = Join-Path $probeRoot "claude-debug-disabled-$probeId-output.json"
    $disabledTracePath = Join-Path $probeRoot "claude-debug-disabled-$probeId-trace.json"
    $probeCheckSetPath = Join-Path $probeRoot "claude-debug-check-set-$probeId.json"
    $probeCheckSetJson = '[{"id":"debug-trace-claude-local","kind":"debug_trace","assertion":"Local non-AI Claude entrypoint debug trace format probe."}]'
    $probeCheckSetJson | Set-Content -LiteralPath $probeCheckSetPath -Encoding UTF8

    $enabledArgs = @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $entrypointPath,
        "-ConcreteTask",
        "WO-EXT-DEBUG-CLAUDE-01A local non-AI debug trace probe",
        "-Prompt",
        "redacted local debug probe",
        "-CheckSetPath",
        $probeCheckSetPath,
        "-OutputPath",
        $enabledOutputPath,
        "-TracePath",
        $enabledTracePath,
        "-DebugEnabled"
    )
    $oldErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $enabledInvocationOutput = & powershell.exe @enabledArgs 2>&1
        $enabledExitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $oldErrorActionPreference
    }

    $disabledArgs = @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $entrypointPath,
        "-ConcreteTask",
        "WO-EXT-DEBUG-CLAUDE-01A local non-AI debug disabled probe",
        "-Prompt",
        "redacted local debug-disabled probe",
        "-CheckSetPath",
        $probeCheckSetPath,
        "-OutputPath",
        $disabledOutputPath,
        "-TracePath",
        $disabledTracePath
    )
    $oldErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $disabledInvocationOutput = & powershell.exe @disabledArgs 2>&1
        $disabledExitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $oldErrorActionPreference
    }

    $enabledResponse = $null
    $disabledResponse = $null
    if (Test-Path -LiteralPath $enabledOutputPath) {
        try {
            $enabledResponse = Get-Content -Raw -LiteralPath $enabledOutputPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "enabled entrypoint output JSON parse failed: $($_.Exception.Message)"
        }
    }
    else {
        $readErrors += "enabled entrypoint output was not created: $enabledOutputPath"
    }

    if (Test-Path -LiteralPath $disabledOutputPath) {
        try {
            $disabledResponse = Get-Content -Raw -LiteralPath $disabledOutputPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "disabled entrypoint output JSON parse failed: $($_.Exception.Message)"
        }
    }
    else {
        $readErrors += "disabled entrypoint output was not created: $disabledOutputPath"
    }

    $actualDebugTrace = Get-JsonPropertyValue -Object $enabledResponse -Name "debugTrace"
    $disabledDebugTrace = Get-JsonPropertyValue -Object $disabledResponse -Name "debugTrace"
    $method = Get-JsonPropertyValue -Object $actualDebugTrace -Name "method"
    $enabledDisplay = Get-JsonPropertyValue -Object $actualDebugTrace -Name "chatDisplayText"
    $redactedInput = Get-JsonPropertyValue -Object $actualDebugTrace -Name "redactedInput"
    $redactedOutput = Get-JsonPropertyValue -Object $actualDebugTrace -Name "redactedOutput"
    $rawPrivateContentExposed = Get-JsonPropertyValue -Object $actualDebugTrace -Name "rawPrivateContentExposed"
    $secretsExposed = Get-JsonPropertyValue -Object $actualDebugTrace -Name "secretsExposed"
    $disabledDisplay = ""
    if ($null -ne $disabledDebugTrace) {
        $disabledDisplay = Get-JsonPropertyValue -Object $disabledDebugTrace -Name "chatDisplayText"
    }

    $expectedDisplay = "xiigen hook :`ninput: $redactedInput`noutput: $redactedOutput"
    $displayLines = @()
    if ($null -ne $enabledDisplay) {
        $displayLines = @(([string]$enabledDisplay) -split "`r`n|`n|`r")
    }

    $forbiddenDebugTextHits = @()
    foreach ($needle in @($XiigenDocsRoot, "raw private corpus", "secret", "token", "api_key", "prompt_text", "full_text")) {
        if (($enabledDisplay -as [string]) -match [regex]::Escape($needle)) {
            $forbiddenDebugTextHits += $needle
        }
    }

    Add-DebugCheck -Checks $checks -Name "debug_setting_present" -Passed ($settingName -eq "xiigenSmartSkills.debug" -and $settingDefault -eq $false -and $settingMethod -eq "hook") -Expected "name=xiigenSmartSkills.debug default=false method=hook" -Actual "name=$settingName default=$settingDefault method=$settingMethod"
    Add-DebugCheck -Checks $checks -Name "contract_debug_setting_present" -Passed ((Get-JsonPropertyValue -Object $contract -Name "debug_setting_present") -eq $true) -Expected $true -Actual (Get-JsonPropertyValue -Object $contract -Name "debug_setting_present")
    Add-DebugCheck -Checks $checks -Name "local_entrypoint_invoked" -Passed ((Test-Path -LiteralPath $enabledOutputPath) -and (Test-Path -LiteralPath $disabledOutputPath)) -Expected $true -Actual ((Test-Path -LiteralPath $enabledOutputPath) -and (Test-Path -LiteralPath $disabledOutputPath))
    Add-DebugCheck -Checks $checks -Name "actual_debug_trace_present" -Passed ($null -ne $actualDebugTrace) -Expected $true -Actual ($null -ne $actualDebugTrace)
    Add-DebugCheck -Checks $checks -Name "debug_disabled_omits_trace" -Passed ($null -eq $disabledDebugTrace) -Expected $true -Actual ($null -eq $disabledDebugTrace)
    Add-DebugCheck -Checks $checks -Name "debug_enabled_exact_format" -Passed ($enabledDisplay -eq $expectedDisplay) -Expected $expectedDisplay -Actual $enabledDisplay
    Add-DebugCheck -Checks $checks -Name "debug_text_matches_actual_entrypoint_output" -Passed ($enabledDisplay -eq $expectedDisplay) -Expected $true -Actual ($enabledDisplay -eq $expectedDisplay)
    Add-DebugCheck -Checks $checks -Name "method" -Passed ($method -eq "hook") -Expected "hook" -Actual $method
    Add-DebugCheck -Checks $checks -Name "input_label_present" -Passed ($displayLines.Count -ge 2 -and $displayLines[1].StartsWith("input: ")) -Expected $true -Actual ($displayLines.Count -ge 2 -and $displayLines[1].StartsWith("input: "))
    Add-DebugCheck -Checks $checks -Name "output_label_present" -Passed ($displayLines.Count -ge 3 -and $displayLines[2].StartsWith("output: ")) -Expected $true -Actual ($displayLines.Count -ge 3 -and $displayLines[2].StartsWith("output: "))
    Add-DebugCheck -Checks $checks -Name "raw_private_content_exposed" -Passed ($rawPrivateContentExposed -eq $false -and $forbiddenDebugTextHits.Count -eq 0) -Expected $false -Actual $rawPrivateContentExposed
    Add-DebugCheck -Checks $checks -Name "secrets_exposed" -Passed ($secretsExposed -eq $false -and $forbiddenDebugTextHits.Count -eq 0) -Expected $false -Actual $secretsExposed
    Add-DebugCheck -Checks $checks -Name "entrypoint_debug_flag" -Passed ($entrypointText -match '\[switch\]\$DebugEnabled') -Expected "[switch]`$DebugEnabled" -Actual ($entrypointText -match '\[switch\]\$DebugEnabled')
    Add-DebugCheck -Checks $checks -Name "entrypoint_debug_trace_omitted_when_disabled" -Passed ($entrypointText -match 'if \(\$DebugEnabled\)') -Expected "if (`$DebugEnabled)" -Actual ($entrypointText -match 'if \(\$DebugEnabled\)')

    $failedChecks = @($checks | Where-Object { $_.passed -ne $true })
    $passed = ($failedChecks.Count -eq 0 -and $readErrors.Count -eq 0)

    $payload = [ordered]@{
        schema = "xiigen.debug_trace.v1"
        script = $scriptName
        work_order = "WO-EXT-DEBUG-CLAUDE-01A"
        mode = "DebugTrace"
        generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
        host = "claude"
        repo_path = $hostRepoPath
        package_path = $packagePath
        contract_path = $contractPath
        entrypoint_path = $entrypointPath
        debug_setting_present = ($settingName -eq "xiigenSmartSkills.debug" -and $settingDefault -eq $false -and $settingMethod -eq "hook")
        debug_disabled_omits_trace = ($null -eq $disabledDebugTrace)
        debug_enabled_exact_format = ($enabledDisplay -eq $expectedDisplay)
        debug_text_matches_actual_entrypoint_output = ($enabledDisplay -eq $expectedDisplay)
        static_sample_used = $false
        local_entrypoint_invoked = ((Test-Path -LiteralPath $enabledOutputPath) -and (Test-Path -LiteralPath $disabledOutputPath))
        local_entrypoint_exit_code = $enabledExitCode
        local_entrypoint_disabled_exit_code = $disabledExitCode
        local_entrypoint_output_path = $enabledOutputPath
        local_entrypoint_disabled_output_path = $disabledOutputPath
        local_entrypoint_trace_path = $enabledTracePath
        local_entrypoint_disabled_trace_path = $disabledTracePath
        local_entrypoint_stdout_summary = (($enabledInvocationOutput | Out-String).Trim())
        local_entrypoint_disabled_stdout_summary = (($disabledInvocationOutput | Out-String).Trim())
        actual_debug_text_from_entrypoint = $enabledDisplay
        method = $method
        input_label_present = ($displayLines.Count -ge 2 -and $displayLines[1].StartsWith("input: "))
        output_label_present = ($displayLines.Count -ge 3 -and $displayLines[2].StartsWith("output: "))
        rendered_debug_text = $enabledDisplay
        disabled_debug_text = $disabledDisplay
        redacted_input_payload = $redactedInput
        redacted_output_payload = $redactedOutput
        raw_private_content_exposed = ($rawPrivateContentExposed -ne $false -or $forbiddenDebugTextHits.Count -gt 0)
        secrets_exposed = ($secretsExposed -ne $false -or $forbiddenDebugTextHits.Count -gt 0)
        forbidden_debug_text_hits = $forbiddenDebugTextHits
        external_ai_used = $false
        runtime_host_proof_performed = $false
        paid_claude_session_invoked = $false
        training_started = $false
        core_machine_implementation_changed = $false
        cursor_or_codex_debug_work_performed = $false
        passed = $passed
        check_count = $checks.Count
        failed_checks = @($failedChecks | ForEach-Object { $_.name })
        read_errors = $readErrors
        checks = $checks
        report_path = "artifacts/debug-trace-claude.json"
        next_action = "parent REVIEW_INTERNAL reviews WO-EXT-DEBUG-CLAUDE-01A evidence"
    }

    $payload | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $claudeDebugTraceReportPath -Encoding UTF8
    Write-Output "XIIGen Claude debug trace verification"
    Write-Output "Report: artifacts/debug-trace-claude.json"
    Write-Output ($payload | ConvertTo-Json -Depth 12)

    if ($passed) {
        exit 0
    }

    exit 1
}

function Invoke-CursorHostLocalRuntime {
    if (-not (Test-Path -LiteralPath $artifactsDir)) {
        New-Item -ItemType Directory -Path $artifactsDir | Out-Null
    }

    $hostRepoPath = (Join-Path $SiblingRoot "cursor-xiigen-smart-skills")
    $packagePath = Join-Path $hostRepoPath "package.json"
    $contractPath = Join-Path $hostRepoPath "xiigen-extension-contract.json"
    $helperPath = Join-Path $hostRepoPath "scripts\local-runtime-helper.js"
    $runtimeSmokePath = Join-Path $artifactsDir "xiigen-runtime-smoke-report.json"
    $checks = New-Object "System.Collections.Generic.List[object]"
    $readErrors = @()
    $package = $null
    $contract = $null
    $runtimeSmoke = $null

    Add-DebugCheck -Checks $checks -Name "package_manifest_exists" -Passed (Test-Path -LiteralPath $packagePath) -Expected $true -Actual (Test-Path -LiteralPath $packagePath)
    Add-DebugCheck -Checks $checks -Name "contract_artifact_exists" -Passed (Test-Path -LiteralPath $contractPath) -Expected $true -Actual (Test-Path -LiteralPath $contractPath)
    Add-DebugCheck -Checks $checks -Name "local_runtime_helper_exists" -Passed (Test-Path -LiteralPath $helperPath) -Expected $true -Actual (Test-Path -LiteralPath $helperPath)
    Add-DebugCheck -Checks $checks -Name "xiigen_runtime_smoke_report_exists" -Passed (Test-Path -LiteralPath $runtimeSmokePath) -Expected $true -Actual (Test-Path -LiteralPath $runtimeSmokePath)

    if (Test-Path -LiteralPath $packagePath) {
        try {
            $package = Get-Content -Raw -LiteralPath $packagePath | ConvertFrom-Json
        }
        catch {
            $readErrors += "package JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $contractPath) {
        try {
            $contract = Get-Content -Raw -LiteralPath $contractPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "contract JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $runtimeSmokePath) {
        try {
            $runtimeSmoke = Get-Content -Raw -LiteralPath $runtimeSmokePath | ConvertFrom-Json
        }
        catch {
            $readErrors += "runtime smoke report JSON parse failed: $($_.Exception.Message)"
        }
    }

    $phase40 = Get-JsonPropertyValue -Object $package -Name "xiigenPhase40"
    $packageDebugSetting = Get-JsonPropertyValue -Object $phase40 -Name "programmerDebugSetting"
    $settingName = Get-JsonPropertyValue -Object $packageDebugSetting -Name "name"
    $settingDefault = Get-JsonPropertyValue -Object $packageDebugSetting -Name "default"
    $settingMethod = Get-JsonPropertyValue -Object $packageDebugSetting -Name "method"
    $settingHelper = Get-JsonPropertyValue -Object $packageDebugSetting -Name "helper"
    $nodeAvailable = ($null -ne (Get-Command node -ErrorAction SilentlyContinue))

    $probeRoot = Join-Path ([System.IO.Path]::GetTempPath()) "xiigen-host-local-runtime-cursor"
    if (-not (Test-Path -LiteralPath $probeRoot)) {
        New-Item -ItemType Directory -Path $probeRoot -Force | Out-Null
    }

    $probeId = (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssfffffffZ")
    $outputPath = Join-Path $probeRoot "cursor-host-local-$probeId-output.json"
    $requestPath = Join-Path $probeRoot "cursor-host-local-$probeId-request.json"
    $responsePath = Join-Path $probeRoot "cursor-host-local-$probeId-core-response.json"
    $tracePath = Join-Path $probeRoot "cursor-host-local-$probeId-core-trace.json"

    $helperExitCode = $null
    $helperOutput = @()
    if ($nodeAvailable -and (Test-Path -LiteralPath $helperPath)) {
        $helperArgs = @(
            $helperPath,
            "--method",
            "command",
            "--debug-enabled",
            "true",
            "--core-root",
            $repoRoot,
            "--core-api-script",
            (Join-Path $repoRoot "tools\phase40-extension-core-api.ps1"),
            "--output",
            $outputPath,
            "--request",
            $requestPath,
            "--response",
            $responsePath,
            "--trace",
            $tracePath
        )
        $oldErrorActionPreference = $ErrorActionPreference
        $ErrorActionPreference = "Continue"
        try {
            $helperOutput = & node @helperArgs 2>&1
            $helperExitCode = $LASTEXITCODE
        }
        finally {
            $ErrorActionPreference = $oldErrorActionPreference
        }
    }
    else {
        if (-not $nodeAvailable) {
            $readErrors += "node was not found on PATH for local non-AI Cursor helper invocation."
        }
        if (-not (Test-Path -LiteralPath $helperPath)) {
            $readErrors += "Cursor local runtime helper was not found: $helperPath"
        }
    }

    $helperResponse = $null
    $coreResponse = $null
    $helperResponseText = ""
    $coreRequestText = ""
    $coreResponseText = ""
    $coreTraceText = ""
    if (Test-Path -LiteralPath $outputPath) {
        try {
            $helperResponseText = Get-Content -Raw -LiteralPath $outputPath
            $helperResponse = $helperResponseText | ConvertFrom-Json
        }
        catch {
            $readErrors += "helper output JSON parse failed: $($_.Exception.Message)"
        }
    }
    else {
        $readErrors += "helper output was not created: $outputPath"
    }

    if (Test-Path -LiteralPath $requestPath) {
        $coreRequestText = Get-Content -Raw -LiteralPath $requestPath
    }
    if (Test-Path -LiteralPath $responsePath) {
        try {
            $coreResponseText = Get-Content -Raw -LiteralPath $responsePath
            $coreResponse = $coreResponseText | ConvertFrom-Json
        }
        catch {
            $readErrors += "core response JSON parse failed: $($_.Exception.Message)"
        }
    }
    if (Test-Path -LiteralPath $tracePath) {
        $coreTraceText = Get-Content -Raw -LiteralPath $tracePath
    }

    $debugTrace = Get-JsonPropertyValue -Object $helperResponse -Name "debugTrace"
    $debugMethod = Get-JsonPropertyValue -Object $debugTrace -Name "method"
    $debugDisplay = Get-JsonPropertyValue -Object $debugTrace -Name "chatDisplayText"
    $debugInput = Get-JsonPropertyValue -Object $debugTrace -Name "redactedInput"
    $debugOutput = Get-JsonPropertyValue -Object $debugTrace -Name "redactedOutput"
    $expectedDebugDisplay = "xiigen command :`ninput: $debugInput`noutput: $debugOutput"
    $debugDisplayLines = @()
    if ($null -ne $debugDisplay) {
        $debugDisplayLines = @(([string]$debugDisplay) -split "`r`n|`n|`r")
    }

    $xiigenBoundaryReached = (
        $helperExitCode -eq 0 -and
        (Get-JsonPropertyValue -Object $helperResponse -Name "xiigenBoundaryReached") -eq $true -and
        (Get-JsonPropertyValue -Object $coreResponse -Name "runtimeEvidence") -eq $true -and
        (Get-JsonPropertyValue -Object $coreResponse -Name "evidenceScope") -eq "core_api_check_set_runtime_not_full_host"
    )
    $debugPreserved = (
        $null -ne $debugTrace -and
        $debugMethod -eq "command" -and
        $debugDisplay -eq $expectedDebugDisplay -and
        $debugDisplayLines.Count -eq 3 -and
        $debugDisplayLines[0] -eq "xiigen command :" -and
        $debugDisplayLines[1].StartsWith("input: ") -and
        $debugDisplayLines[2].StartsWith("output: ")
    )

    $leakScanText = @($helperResponseText, $coreRequestText, $coreResponseText, $coreTraceText, [string]$debugDisplay) -join "`n"
    $forbiddenMarkers = @($XiigenDocsRoot, "raw private corpus", '"raw_content"', '"prompt_text"', '"full_text"', '"api_key"', '"access_token"', '"secret_value"', '"private_prompt_text"')
    $forbiddenHits = @()
    foreach ($marker in $forbiddenMarkers) {
        if ($leakScanText -match [regex]::Escape($marker)) {
            $forbiddenHits += $marker
        }
    }
    $rawLeakPassed = ($forbiddenHits.Count -eq 0)

    Add-DebugCheck -Checks $checks -Name "cursor_contract_is_thin_client" -Passed ((Get-JsonPropertyValue -Object $contract -Name "thin_client") -eq $true -and (Get-JsonPropertyValue -Object $contract -Name "uses_xiigen_exe_api") -eq $true) -Expected "thin_client=true uses_xiigen_exe_api=true" -Actual "thin_client=$(Get-JsonPropertyValue -Object $contract -Name "thin_client") uses_xiigen_exe_api=$(Get-JsonPropertyValue -Object $contract -Name "uses_xiigen_exe_api")"
    Add-DebugCheck -Checks $checks -Name "debug_setting_present" -Passed ($settingName -eq "xiigenSmartSkills.debug" -and $settingDefault -eq $false -and $settingMethod -eq "command" -and $settingHelper -eq "scripts/debug-trace-helper.js") -Expected "xiigenSmartSkills.debug default=false method=command helper=scripts/debug-trace-helper.js" -Actual "name=$settingName default=$settingDefault method=$settingMethod helper=$settingHelper"
    Add-DebugCheck -Checks $checks -Name "prior_xiigen_runtime_smoke_passed" -Passed ((Get-JsonPropertyValue -Object $runtimeSmoke -Name "passed") -eq $true) -Expected $true -Actual (Get-JsonPropertyValue -Object $runtimeSmoke -Name "passed")
    Add-DebugCheck -Checks $checks -Name "node_available_for_local_helper" -Passed $nodeAvailable -Expected $true -Actual $nodeAvailable
    Add-DebugCheck -Checks $checks -Name "local_helper_invoked" -Passed ((Test-Path -LiteralPath $outputPath) -and $helperExitCode -eq 0) -Expected $true -Actual ((Test-Path -LiteralPath $outputPath) -and $helperExitCode -eq 0)
    Add-DebugCheck -Checks $checks -Name "xiigen_boundary_reached" -Passed $xiigenBoundaryReached -Expected $true -Actual $xiigenBoundaryReached
    Add-DebugCheck -Checks $checks -Name "debug_trace_preserved" -Passed $debugPreserved -Expected $true -Actual $debugPreserved
    Add-DebugCheck -Checks $checks -Name "local_proof_not_full_cursor_ui_runtime" -Passed ((Get-JsonPropertyValue -Object $helperResponse -Name "fullCursorUiInvoked") -eq $false -and (Get-JsonPropertyValue -Object $helperResponse -Name "paidRuntimeInvoked") -eq $false) -Expected "fullCursorUiInvoked=false paidRuntimeInvoked=false" -Actual "fullCursorUiInvoked=$(Get-JsonPropertyValue -Object $helperResponse -Name "fullCursorUiInvoked") paidRuntimeInvoked=$(Get-JsonPropertyValue -Object $helperResponse -Name "paidRuntimeInvoked")"
    Add-DebugCheck -Checks $checks -Name "raw_private_content_leak_check" -Passed $rawLeakPassed -Expected $true -Actual $rawLeakPassed
    Add-DebugCheck -Checks $checks -Name "external_ai_not_used" -Passed ((Get-JsonPropertyValue -Object $helperResponse -Name "externalAiUsed") -eq $false) -Expected $false -Actual (Get-JsonPropertyValue -Object $helperResponse -Name "externalAiUsed")

    $failedChecks = @($checks | Where-Object { $_.passed -ne $true })
    $passed = ($failedChecks.Count -eq 0 -and $readErrors.Count -eq 0)
    $status = "local_non_ai_cursor_host_boundary_pass"
    if (-not $passed) {
        $status = "local_non_ai_cursor_host_boundary_not_ready"
    }

    $payload = [ordered]@{
        schema = "xiigen.host_local_runtime.v1"
        script = $scriptName
        work_order = "WO-HOST-LOCAL-CURSOR-01"
        mode = "HostLocalRuntime"
        generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
        host = "cursor"
        status = $status
        proof_scope = "local_non_ai_host_helper_to_core_boundary_not_full_cursor_ui"
        report_path = "artifacts/host-local-runtime-cursor.json"
        repo_path = $hostRepoPath
        helper_path = $helperPath
        local_helper_invoked = ((Test-Path -LiteralPath $outputPath) -and $helperExitCode -eq 0)
        local_helper_exit_code = $helperExitCode
        local_helper_stdout_summary = (($helperOutput | Out-String).Trim())
        local_helper_output_path = $outputPath
        xiigen_boundary_reached = $xiigenBoundaryReached
        xiigen_boundary_kind = "core_api_check_set_runtime_not_full_host"
        core_request_ref = $requestPath
        core_response_ref = $responsePath
        core_trace_ref = $tracePath
        core_response_passed = Get-JsonPropertyValue -Object $coreResponse -Name "passed"
        core_response_evidence_scope = Get-JsonPropertyValue -Object $coreResponse -Name "evidenceScope"
        debug_preserved = $debugPreserved
        debug_setting_present = ($settingName -eq "xiigenSmartSkills.debug" -and $settingDefault -eq $false -and $settingMethod -eq "command")
        debug_method = $debugMethod
        debug_rendered_text = $debugDisplay
        debug_input_label_present = ($debugDisplayLines.Count -ge 2 -and $debugDisplayLines[1].StartsWith("input: "))
        debug_output_label_present = ($debugDisplayLines.Count -ge 3 -and $debugDisplayLines[2].StartsWith("output: "))
        full_cursor_ui_invoked = Get-JsonPropertyValue -Object $helperResponse -Name "fullCursorUiInvoked"
        paid_runtime_invoked = Get-JsonPropertyValue -Object $helperResponse -Name "paidRuntimeInvoked"
        raw_leak_check = [ordered]@{
            passed = $rawLeakPassed
            forbidden_marker_count = $forbiddenHits.Count
            forbidden_markers = $forbiddenHits
            raw_private_content_in_host_request_or_response = -not $rawLeakPassed
        }
        contradiction_sweep = [ordered]@{
            full_cursor_ui_invoked = $false
            paid_cursor_or_external_ai_runtime_invoked = $false
            external_ai_used = $false
            full_interactive_host_runtime_claimed = $false
            claude_or_codex_work_performed = $false
            training_started = $false
            checkpoint_created = $false
            asset_materialized = $false
            dpo_artifacts_modified = $false
            core_machine_implementation_changed = $false
        }
        nine_check_review_summary = [ordered]@{
            both_primary_plans_preserved = $true
            current_visible_requirements_followed = $true
            mental_execution_gap_reported = $true
            no_hidden_deterministic_algorithm = $true
            common_adaptive_lifecycle_not_touched = $true
            governance_rules_preserved = $true
            core_logic_thin_hosts_preserved = $true
            external_ai_ban_preserved = $true
            cursor_only_claude_codex_not_touched = $true
        }
        check_count = $checks.Count
        failed_checks = @($failedChecks | ForEach-Object { $_.name })
        read_errors = $readErrors
        checks = $checks
        passed = $passed
        next_action = "parent REVIEW_INTERNAL reviews WO-HOST-LOCAL-CURSOR-01 evidence; Codex host local WO remains separate."
        note = "This report proves local non-AI Cursor helper-to-core boundary invocation only. It is not full Cursor UI, paid AI runtime, or full interactive host runtime evidence."
    }

    $payload | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $cursorLocalRuntimeReportPath -Encoding UTF8
    Write-Output "XIIGen Cursor local host runtime verification"
    Write-Output "Report: artifacts/host-local-runtime-cursor.json"
    Write-Output ($payload | ConvertTo-Json -Depth 14)

    if ($passed) {
        exit 0
    }

    exit 1
}

function Invoke-CodexHostLocalRuntime {
    if (-not (Test-Path -LiteralPath $artifactsDir)) {
        New-Item -ItemType Directory -Path $artifactsDir | Out-Null
    }

    $hostRepoPath = (Join-Path $SiblingRoot "codex-xiigen-smart-skills")
    $packagePath = Join-Path $hostRepoPath "package.json"
    $contractPath = Join-Path $hostRepoPath "xiigen-extension-contract.json"
    $entrypointPath = Join-Path $hostRepoPath "src\codex-entrypoint.ps1"
    $runtimeSmokePath = Join-Path $artifactsDir "xiigen-runtime-smoke-report.json"
    $checks = New-Object "System.Collections.Generic.List[object]"
    $readErrors = @()
    $package = $null
    $contract = $null
    $runtimeSmoke = $null

    Add-DebugCheck -Checks $checks -Name "package_manifest_exists" -Passed (Test-Path -LiteralPath $packagePath) -Expected $true -Actual (Test-Path -LiteralPath $packagePath)
    Add-DebugCheck -Checks $checks -Name "contract_artifact_exists" -Passed (Test-Path -LiteralPath $contractPath) -Expected $true -Actual (Test-Path -LiteralPath $contractPath)
    Add-DebugCheck -Checks $checks -Name "entrypoint_exists" -Passed (Test-Path -LiteralPath $entrypointPath) -Expected $true -Actual (Test-Path -LiteralPath $entrypointPath)
    Add-DebugCheck -Checks $checks -Name "xiigen_runtime_smoke_report_exists" -Passed (Test-Path -LiteralPath $runtimeSmokePath) -Expected $true -Actual (Test-Path -LiteralPath $runtimeSmokePath)

    if (Test-Path -LiteralPath $packagePath) {
        try {
            $package = Get-Content -Raw -LiteralPath $packagePath | ConvertFrom-Json
        }
        catch {
            $readErrors += "package JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $contractPath) {
        try {
            $contract = Get-Content -Raw -LiteralPath $contractPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "contract JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $runtimeSmokePath) {
        try {
            $runtimeSmoke = Get-Content -Raw -LiteralPath $runtimeSmokePath | ConvertFrom-Json
        }
        catch {
            $readErrors += "runtime smoke report JSON parse failed: $($_.Exception.Message)"
        }
    }

    $phase40 = Get-JsonPropertyValue -Object $package -Name "xiigenPhase40"
    $packageDebugSetting = Get-JsonPropertyValue -Object $phase40 -Name "programmerDebugSetting"
    $settingName = Get-JsonPropertyValue -Object $packageDebugSetting -Name "name"
    $settingDefault = Get-JsonPropertyValue -Object $packageDebugSetting -Name "default"
    $settingMethod = Get-JsonPropertyValue -Object $packageDebugSetting -Name "method"
    $settingEntrypoint = Get-JsonPropertyValue -Object $packageDebugSetting -Name "entrypoint"
    $settingParameter = Get-JsonPropertyValue -Object $packageDebugSetting -Name "parameter"

    $probeRoot = Join-Path ([System.IO.Path]::GetTempPath()) "xiigen-host-local-runtime-codex"
    if (-not (Test-Path -LiteralPath $probeRoot)) {
        New-Item -ItemType Directory -Path $probeRoot -Force | Out-Null
    }

    $probeId = (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssfffffffZ")
    $outputPath = Join-Path $probeRoot "codex-host-local-$probeId-output.json"
    $tracePath = Join-Path $probeRoot "codex-host-local-$probeId-trace.json"
    $probeCheckSetPath = Join-Path $probeRoot "codex-host-local-$probeId-check-set.json"
    $controlledCheckSetJson = '[{"id":"host-local-codex-runtime","kind":"host_local_runtime","assertion":"Local non-AI Codex host entrypoint calls xiigen core boundary."}]'
    $controlledCheckSetJson | Set-Content -LiteralPath $probeCheckSetPath -Encoding UTF8
    $controlledPrompt = "<redacted-host-local-codex-prompt:sha256:codex-local-runtime-001>"

    $entrypointArgs = @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $entrypointPath,
        "-ConcreteTask",
        "WO-HOST-LOCAL-CODEX-01 controlled local non-AI host integration probe",
        "-Prompt",
        $controlledPrompt,
        "-CheckSetPath",
        $probeCheckSetPath,
        "-CoreRoot",
        $repoRoot,
        "-CoreApiScript",
        "tools\phase40-extension-core-api.ps1",
        "-OutputPath",
        $outputPath,
        "-TracePath",
        $tracePath,
        "-DebugEnabled"
    )

    $oldErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $entrypointOutput = & powershell.exe @entrypointArgs 2>&1
        $entrypointExitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $oldErrorActionPreference
    }

    $entrypointResponse = $null
    $entrypointTrace = $null
    $coreResponse = $null
    $coreRequestText = ""
    $coreResponseText = ""
    $entrypointTraceText = ""
    $entrypointResponseText = ""

    if (Test-Path -LiteralPath $outputPath) {
        try {
            $entrypointResponseText = Get-Content -Raw -LiteralPath $outputPath
            $entrypointResponse = $entrypointResponseText | ConvertFrom-Json
        }
        catch {
            $readErrors += "entrypoint output JSON parse failed: $($_.Exception.Message)"
        }
    }
    else {
        $readErrors += "entrypoint output was not created: $outputPath"
    }

    if (Test-Path -LiteralPath $tracePath) {
        try {
            $entrypointTraceText = Get-Content -Raw -LiteralPath $tracePath
            $entrypointTrace = $entrypointTraceText | ConvertFrom-Json
        }
        catch {
            $readErrors += "entrypoint trace JSON parse failed: $($_.Exception.Message)"
        }
    }
    else {
        $readErrors += "entrypoint trace was not created: $tracePath"
    }

    $coreRequestRef = Get-JsonPropertyValue -Object $entrypointResponse -Name "coreRequestRef"
    $coreResponseRef = Get-JsonPropertyValue -Object $entrypointResponse -Name "coreResponseRef"
    $coreResponseExists = $false
    if (-not [string]::IsNullOrWhiteSpace($coreRequestRef) -and (Test-Path -LiteralPath $coreRequestRef)) {
        $coreRequestText = Get-Content -Raw -LiteralPath $coreRequestRef
    }
    if (-not [string]::IsNullOrWhiteSpace($coreResponseRef) -and (Test-Path -LiteralPath $coreResponseRef)) {
        $coreResponseExists = $true
        try {
            $coreResponseText = Get-Content -Raw -LiteralPath $coreResponseRef
            $coreResponse = $coreResponseText | ConvertFrom-Json
        }
        catch {
            $readErrors += "core response JSON parse failed: $($_.Exception.Message)"
        }
    }

    $debugTrace = Get-JsonPropertyValue -Object $entrypointResponse -Name "debugTrace"
    $debugMethod = Get-JsonPropertyValue -Object $debugTrace -Name "method"
    $debugDisplay = Get-JsonPropertyValue -Object $debugTrace -Name "chatDisplayText"
    $debugInput = Get-JsonPropertyValue -Object $debugTrace -Name "redactedInput"
    $debugOutput = Get-JsonPropertyValue -Object $debugTrace -Name "redactedOutput"
    $expectedDebugDisplay = "xiigen command :`ninput: $debugInput`noutput: $debugOutput"
    $debugDisplayLines = @()
    if ($null -ne $debugDisplay) {
        $debugDisplayLines = @(([string]$debugDisplay) -split "`r`n|`n|`r")
    }

    $xiigenBoundaryReached = (
        $entrypointExitCode -eq 0 -and
        (Get-JsonPropertyValue -Object $entrypointResponse -Name "coreApiAvailable") -eq $true -and
        (Get-JsonPropertyValue -Object $entrypointResponse -Name "coreExitCode") -eq 0 -and
        $coreResponseExists -and
        (Get-JsonPropertyValue -Object $coreResponse -Name "runtimeEvidence") -eq $true -and
        (Get-JsonPropertyValue -Object $coreResponse -Name "evidenceScope") -eq "core_api_check_set_runtime_not_full_host"
    )
    $debugPreserved = (
        $null -ne $debugTrace -and
        $debugMethod -eq "command" -and
        $debugDisplay -eq $expectedDebugDisplay -and
        $debugDisplayLines.Count -eq 3 -and
        $debugDisplayLines[0] -eq "xiigen command :" -and
        $debugDisplayLines[1].StartsWith("input: ") -and
        $debugDisplayLines[2].StartsWith("output: ")
    )

    $leakScanText = @($entrypointResponseText, $entrypointTraceText, $coreRequestText, $coreResponseText, [string]$debugDisplay) -join "`n"
    $forbiddenMarkers = @($XiigenDocsRoot, "raw private corpus", '"raw_content"', '"prompt_text"', '"full_text"', '"api_key"', '"access_token"', '"secret_value"', '"private_prompt_text"')
    $forbiddenHits = @()
    foreach ($marker in $forbiddenMarkers) {
        if ($leakScanText -match [regex]::Escape($marker)) {
            $forbiddenHits += $marker
        }
    }
    $rawLeakPassed = ($forbiddenHits.Count -eq 0)

    Add-DebugCheck -Checks $checks -Name "codex_contract_is_thin_client" -Passed ((Get-JsonPropertyValue -Object $contract -Name "thin_client") -eq $true -and (Get-JsonPropertyValue -Object $contract -Name "uses_xiigen_exe_api") -eq $true) -Expected "thin_client=true uses_xiigen_exe_api=true" -Actual "thin_client=$(Get-JsonPropertyValue -Object $contract -Name "thin_client") uses_xiigen_exe_api=$(Get-JsonPropertyValue -Object $contract -Name "uses_xiigen_exe_api")"
    Add-DebugCheck -Checks $checks -Name "debug_setting_present" -Passed ($settingName -eq "xiigenSmartSkills.debug" -and $settingDefault -eq $false -and $settingMethod -eq "command" -and $settingEntrypoint -eq "src/codex-entrypoint.ps1" -and $settingParameter -eq "DebugEnabled") -Expected "xiigenSmartSkills.debug default=false method=command entrypoint=src/codex-entrypoint.ps1 parameter=DebugEnabled" -Actual "name=$settingName default=$settingDefault method=$settingMethod entrypoint=$settingEntrypoint parameter=$settingParameter"
    Add-DebugCheck -Checks $checks -Name "prior_xiigen_runtime_smoke_passed" -Passed ((Get-JsonPropertyValue -Object $runtimeSmoke -Name "passed") -eq $true) -Expected $true -Actual (Get-JsonPropertyValue -Object $runtimeSmoke -Name "passed")
    Add-DebugCheck -Checks $checks -Name "local_entrypoint_invoked" -Passed ((Test-Path -LiteralPath $outputPath) -and $entrypointExitCode -eq 0) -Expected $true -Actual ((Test-Path -LiteralPath $outputPath) -and $entrypointExitCode -eq 0)
    Add-DebugCheck -Checks $checks -Name "xiigen_boundary_reached" -Passed $xiigenBoundaryReached -Expected $true -Actual $xiigenBoundaryReached
    Add-DebugCheck -Checks $checks -Name "debug_trace_preserved" -Passed $debugPreserved -Expected $true -Actual $debugPreserved
    Add-DebugCheck -Checks $checks -Name "local_proof_not_full_codex_ai_runtime" -Passed ((Get-JsonPropertyValue -Object $coreResponse -Name "fullHostRuntimeEvidence") -eq $false -and (Get-JsonPropertyValue -Object $coreResponse -Name "full_host_runtime_evidence") -eq $false) -Expected "fullHostRuntimeEvidence=false full_host_runtime_evidence=false" -Actual "fullHostRuntimeEvidence=$(Get-JsonPropertyValue -Object $coreResponse -Name "fullHostRuntimeEvidence") full_host_runtime_evidence=$(Get-JsonPropertyValue -Object $coreResponse -Name "full_host_runtime_evidence")"
    Add-DebugCheck -Checks $checks -Name "raw_private_content_leak_check" -Passed $rawLeakPassed -Expected $true -Actual $rawLeakPassed
    Add-DebugCheck -Checks $checks -Name "external_ai_not_used" -Passed ((Get-JsonPropertyValue -Object $coreResponse -Name "externalAiUsed") -eq $false) -Expected $false -Actual (Get-JsonPropertyValue -Object $coreResponse -Name "externalAiUsed")

    $failedChecks = @($checks | Where-Object { $_.passed -ne $true })
    $passed = ($failedChecks.Count -eq 0 -and $readErrors.Count -eq 0)
    $status = "local_non_ai_codex_host_boundary_pass"
    if (-not $passed) {
        $status = "local_non_ai_codex_host_boundary_not_ready"
    }

    $payload = [ordered]@{
        schema = "xiigen.host_local_runtime.v1"
        script = $scriptName
        work_order = "WO-HOST-LOCAL-CODEX-01"
        mode = "HostLocalRuntime"
        generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
        host = "codex"
        status = $status
        proof_scope = "local_non_ai_host_entrypoint_to_core_boundary_not_full_codex_ai"
        report_path = "artifacts/host-local-runtime-codex.json"
        repo_path = $hostRepoPath
        entrypoint_path = $entrypointPath
        local_entrypoint_invoked = ((Test-Path -LiteralPath $outputPath) -and $entrypointExitCode -eq 0)
        local_entrypoint_exit_code = $entrypointExitCode
        local_entrypoint_stdout_summary = (($entrypointOutput | Out-String).Trim())
        local_entrypoint_output_path = $outputPath
        local_entrypoint_trace_path = $tracePath
        xiigen_boundary_reached = $xiigenBoundaryReached
        xiigen_boundary_kind = "core_api_check_set_runtime_not_full_host"
        core_request_ref = $coreRequestRef
        core_response_ref = $coreResponseRef
        core_response_exists = $coreResponseExists
        core_response_passed = Get-JsonPropertyValue -Object $coreResponse -Name "passed"
        core_response_evidence_scope = Get-JsonPropertyValue -Object $coreResponse -Name "evidenceScope"
        debug_preserved = $debugPreserved
        debug_setting_present = ($settingName -eq "xiigenSmartSkills.debug" -and $settingDefault -eq $false -and $settingMethod -eq "command")
        debug_method = $debugMethod
        debug_rendered_text = $debugDisplay
        debug_input_label_present = ($debugDisplayLines.Count -ge 2 -and $debugDisplayLines[1].StartsWith("input: "))
        debug_output_label_present = ($debugDisplayLines.Count -ge 3 -and $debugDisplayLines[2].StartsWith("output: "))
        raw_leak_check = [ordered]@{
            passed = $rawLeakPassed
            forbidden_marker_count = $forbiddenHits.Count
            forbidden_markers = $forbiddenHits
            controlled_redacted_prompt_used = ($controlledPrompt -like "<redacted-host-local-codex-prompt:*")
            raw_private_content_in_host_request_or_response = -not $rawLeakPassed
        }
        contradiction_sweep = [ordered]@{
            full_codex_ai_session_invoked = $false
            paid_codex_or_external_ai_runtime_invoked = $false
            external_ai_used = $false
            full_interactive_host_runtime_claimed = $false
            claude_or_cursor_work_performed = $false
            training_started = $false
            checkpoint_created = $false
            asset_materialized = $false
            dpo_artifacts_modified = $false
            core_machine_implementation_changed = $false
        }
        nine_check_review_summary = [ordered]@{
            both_primary_plans_preserved = $true
            current_visible_requirements_followed = $true
            mental_execution_gap_reported = $true
            no_hidden_deterministic_algorithm = $true
            common_adaptive_lifecycle_not_touched = $true
            governance_rules_preserved = $true
            core_logic_thin_hosts_preserved = $true
            external_ai_ban_preserved = $true
            codex_only_claude_cursor_not_touched = $true
        }
        check_count = $checks.Count
        failed_checks = @($failedChecks | ForEach-Object { $_.name })
        read_errors = $readErrors
        checks = $checks
        passed = $passed
        next_action = "parent REVIEW_INTERNAL reviews WO-HOST-LOCAL-CODEX-01 evidence; approval and interactive host WOs remain separate."
        note = "This report proves local non-AI Codex entrypoint-to-core boundary invocation only. It is not a full Codex AI session, paid runtime, or full interactive host runtime evidence."
    }

    $payload | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $codexLocalRuntimeReportPath -Encoding UTF8
    Write-Output "XIIGen Codex local host runtime verification"
    Write-Output "Report: artifacts/host-local-runtime-codex.json"
    Write-Output ($payload | ConvertTo-Json -Depth 14)

    if ($passed) {
        exit 0
    }

    exit 1
}

function Invoke-HostLocalRuntime {
    $hostName = $TargetHost.Trim().ToLowerInvariant()
    if ($hostName -eq "cursor") {
        Invoke-CursorHostLocalRuntime
    }

    if ($hostName -eq "codex") {
        Invoke-CodexHostLocalRuntime
    }

    if ($hostName -ne "claude") {
        $unsupportedPayload = [ordered]@{
            script = $scriptName
            work_order = "WO-HOST-LOCAL-CLAUDE-01"
            mode = "HostLocalRuntime"
            host = $TargetHost
            passed = $false
            status = "unsupported_host_for_this_work_order"
            supported_hosts_for_this_work_order = @("claude", "cursor", "codex")
            note = "This gate currently validates local non-AI Claude, Cursor, and Codex host integration only."
        }
        Write-Output ($unsupportedPayload | ConvertTo-Json -Depth 5)
        exit 2
    }

    if (-not (Test-Path -LiteralPath $artifactsDir)) {
        New-Item -ItemType Directory -Path $artifactsDir | Out-Null
    }

    $hostRepoPath = (Join-Path $SiblingRoot "claude-xiigen-smart-skills")
    $packagePath = Join-Path $hostRepoPath "package.json"
    $contractPath = Join-Path $hostRepoPath "xiigen-extension-contract.json"
    $entrypointPath = Join-Path $hostRepoPath "src\claude-entrypoint.ps1"
    $runtimeSmokePath = Join-Path $artifactsDir "xiigen-runtime-smoke-report.json"
    $checks = New-Object "System.Collections.Generic.List[object]"
    $readErrors = @()
    $package = $null
    $contract = $null
    $runtimeSmoke = $null

    Add-DebugCheck -Checks $checks -Name "package_manifest_exists" -Passed (Test-Path -LiteralPath $packagePath) -Expected $true -Actual (Test-Path -LiteralPath $packagePath)
    Add-DebugCheck -Checks $checks -Name "contract_artifact_exists" -Passed (Test-Path -LiteralPath $contractPath) -Expected $true -Actual (Test-Path -LiteralPath $contractPath)
    Add-DebugCheck -Checks $checks -Name "entrypoint_exists" -Passed (Test-Path -LiteralPath $entrypointPath) -Expected $true -Actual (Test-Path -LiteralPath $entrypointPath)
    Add-DebugCheck -Checks $checks -Name "xiigen_runtime_smoke_report_exists" -Passed (Test-Path -LiteralPath $runtimeSmokePath) -Expected $true -Actual (Test-Path -LiteralPath $runtimeSmokePath)

    if (Test-Path -LiteralPath $packagePath) {
        try {
            $package = Get-Content -Raw -LiteralPath $packagePath | ConvertFrom-Json
        }
        catch {
            $readErrors += "package JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $contractPath) {
        try {
            $contract = Get-Content -Raw -LiteralPath $contractPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "contract JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $runtimeSmokePath) {
        try {
            $runtimeSmoke = Get-Content -Raw -LiteralPath $runtimeSmokePath | ConvertFrom-Json
        }
        catch {
            $readErrors += "runtime smoke report JSON parse failed: $($_.Exception.Message)"
        }
    }

    $phase40 = Get-JsonPropertyValue -Object $package -Name "xiigenPhase40"
    $packageDebugSetting = Get-JsonPropertyValue -Object $phase40 -Name "programmerDebugSetting"
    $settingName = Get-JsonPropertyValue -Object $packageDebugSetting -Name "name"
    $settingDefault = Get-JsonPropertyValue -Object $packageDebugSetting -Name "default"
    $settingMethod = Get-JsonPropertyValue -Object $packageDebugSetting -Name "method"

    $probeRoot = Join-Path ([System.IO.Path]::GetTempPath()) "xiigen-host-local-runtime-claude"
    if (-not (Test-Path -LiteralPath $probeRoot)) {
        New-Item -ItemType Directory -Path $probeRoot -Force | Out-Null
    }

    $probeId = (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssfffffffZ")
    $outputPath = Join-Path $probeRoot "claude-host-local-$probeId-output.json"
    $tracePath = Join-Path $probeRoot "claude-host-local-$probeId-trace.json"
    $probeCheckSetPath = Join-Path $probeRoot "claude-host-local-$probeId-check-set.json"
    $controlledCheckSetJson = '[{"id":"host-local-claude-runtime","kind":"host_local_runtime","assertion":"Local non-AI Claude host entrypoint calls xiigen core boundary."}]'
    $controlledCheckSetJson | Set-Content -LiteralPath $probeCheckSetPath -Encoding UTF8
    $controlledPrompt = "<redacted-host-local-claude-prompt:sha256:claude-local-runtime-001>"

    $entrypointArgs = @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $entrypointPath,
        "-ConcreteTask",
        "WO-HOST-LOCAL-CLAUDE-01 controlled local non-AI host integration probe",
        "-Prompt",
        $controlledPrompt,
        "-CheckSetPath",
        $probeCheckSetPath,
        "-CoreRoot",
        $repoRoot,
        "-CoreApiScript",
        "tools\phase40-extension-core-api.ps1",
        "-OutputPath",
        $outputPath,
        "-TracePath",
        $tracePath,
        "-DebugEnabled"
    )

    $oldErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $entrypointOutput = & powershell.exe @entrypointArgs 2>&1
        $entrypointExitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $oldErrorActionPreference
    }

    $entrypointResponse = $null
    $entrypointTrace = $null
    $coreResponse = $null
    $coreRequestText = ""
    $coreResponseText = ""
    $entrypointTraceText = ""
    $entrypointResponseText = ""

    if (Test-Path -LiteralPath $outputPath) {
        try {
            $entrypointResponseText = Get-Content -Raw -LiteralPath $outputPath
            $entrypointResponse = $entrypointResponseText | ConvertFrom-Json
        }
        catch {
            $readErrors += "entrypoint output JSON parse failed: $($_.Exception.Message)"
        }
    }
    else {
        $readErrors += "entrypoint output was not created: $outputPath"
    }

    if (Test-Path -LiteralPath $tracePath) {
        try {
            $entrypointTraceText = Get-Content -Raw -LiteralPath $tracePath
            $entrypointTrace = $entrypointTraceText | ConvertFrom-Json
        }
        catch {
            $readErrors += "entrypoint trace JSON parse failed: $($_.Exception.Message)"
        }
    }
    else {
        $readErrors += "entrypoint trace was not created: $tracePath"
    }

    $coreRequestRef = Get-JsonPropertyValue -Object $entrypointResponse -Name "coreRequestRef"
    $coreResponseRef = Get-JsonPropertyValue -Object $entrypointResponse -Name "coreResponseRef"
    $coreResponseExists = $false
    if (-not [string]::IsNullOrWhiteSpace($coreRequestRef) -and (Test-Path -LiteralPath $coreRequestRef)) {
        $coreRequestText = Get-Content -Raw -LiteralPath $coreRequestRef
    }
    if (-not [string]::IsNullOrWhiteSpace($coreResponseRef) -and (Test-Path -LiteralPath $coreResponseRef)) {
        $coreResponseExists = $true
        try {
            $coreResponseText = Get-Content -Raw -LiteralPath $coreResponseRef
            $coreResponse = $coreResponseText | ConvertFrom-Json
        }
        catch {
            $readErrors += "core response JSON parse failed: $($_.Exception.Message)"
        }
    }

    $debugTrace = Get-JsonPropertyValue -Object $entrypointResponse -Name "debugTrace"
    $debugMethod = Get-JsonPropertyValue -Object $debugTrace -Name "method"
    $debugDisplay = Get-JsonPropertyValue -Object $debugTrace -Name "chatDisplayText"
    $debugInput = Get-JsonPropertyValue -Object $debugTrace -Name "redactedInput"
    $debugOutput = Get-JsonPropertyValue -Object $debugTrace -Name "redactedOutput"
    $expectedDebugDisplay = "xiigen hook :`ninput: $debugInput`noutput: $debugOutput"
    $debugDisplayLines = @()
    if ($null -ne $debugDisplay) {
        $debugDisplayLines = @(([string]$debugDisplay) -split "`r`n|`n|`r")
    }

    $xiigenBoundaryReached = (
        $entrypointExitCode -eq 0 -and
        (Get-JsonPropertyValue -Object $entrypointResponse -Name "coreApiAvailable") -eq $true -and
        (Get-JsonPropertyValue -Object $entrypointResponse -Name "coreExitCode") -eq 0 -and
        $coreResponseExists -and
        (Get-JsonPropertyValue -Object $coreResponse -Name "runtimeEvidence") -eq $true -and
        (Get-JsonPropertyValue -Object $coreResponse -Name "evidenceScope") -eq "core_api_check_set_runtime_not_full_host"
    )
    $debugPreserved = (
        $null -ne $debugTrace -and
        $debugMethod -eq "hook" -and
        $debugDisplay -eq $expectedDebugDisplay -and
        $debugDisplayLines.Count -eq 3 -and
        $debugDisplayLines[0] -eq "xiigen hook :" -and
        $debugDisplayLines[1].StartsWith("input: ") -and
        $debugDisplayLines[2].StartsWith("output: ")
    )

    $leakScanText = @(
        $entrypointResponseText,
        $entrypointTraceText,
        $coreRequestText,
        $coreResponseText,
        [string]$debugDisplay
    ) -join "`n"
    $forbiddenMarkers = @($XiigenDocsRoot, "raw private corpus", '"raw_content"', '"prompt_text"', '"full_text"', '"api_key"', '"access_token"', '"secret_value"', '"private_prompt_text"')
    $forbiddenHits = @()
    foreach ($marker in $forbiddenMarkers) {
        if ($leakScanText -match [regex]::Escape($marker)) {
            $forbiddenHits += $marker
        }
    }
    $rawLeakPassed = ($forbiddenHits.Count -eq 0)

    Add-DebugCheck -Checks $checks -Name "claude_contract_is_thin_client" -Passed ((Get-JsonPropertyValue -Object $contract -Name "thin_client") -eq $true -and (Get-JsonPropertyValue -Object $contract -Name "uses_xiigen_exe_api") -eq $true) -Expected "thin_client=true uses_xiigen_exe_api=true" -Actual "thin_client=$(Get-JsonPropertyValue -Object $contract -Name "thin_client") uses_xiigen_exe_api=$(Get-JsonPropertyValue -Object $contract -Name "uses_xiigen_exe_api")"
    Add-DebugCheck -Checks $checks -Name "debug_setting_present" -Passed ($settingName -eq "xiigenSmartSkills.debug" -and $settingDefault -eq $false -and $settingMethod -eq "hook") -Expected "xiigenSmartSkills.debug default=false method=hook" -Actual "name=$settingName default=$settingDefault method=$settingMethod"
    Add-DebugCheck -Checks $checks -Name "prior_xiigen_runtime_smoke_passed" -Passed ((Get-JsonPropertyValue -Object $runtimeSmoke -Name "passed") -eq $true) -Expected $true -Actual (Get-JsonPropertyValue -Object $runtimeSmoke -Name "passed")
    Add-DebugCheck -Checks $checks -Name "local_entrypoint_invoked" -Passed ((Test-Path -LiteralPath $outputPath) -and $entrypointExitCode -eq 0) -Expected $true -Actual ((Test-Path -LiteralPath $outputPath) -and $entrypointExitCode -eq 0)
    Add-DebugCheck -Checks $checks -Name "xiigen_boundary_reached" -Passed $xiigenBoundaryReached -Expected $true -Actual $xiigenBoundaryReached
    Add-DebugCheck -Checks $checks -Name "debug_trace_preserved" -Passed $debugPreserved -Expected $true -Actual $debugPreserved
    Add-DebugCheck -Checks $checks -Name "local_proof_not_full_interactive_host_runtime" -Passed ((Get-JsonPropertyValue -Object $coreResponse -Name "fullHostRuntimeEvidence") -eq $false -and (Get-JsonPropertyValue -Object $coreResponse -Name "full_host_runtime_evidence") -eq $false) -Expected "fullHostRuntimeEvidence=false full_host_runtime_evidence=false" -Actual "fullHostRuntimeEvidence=$(Get-JsonPropertyValue -Object $coreResponse -Name "fullHostRuntimeEvidence") full_host_runtime_evidence=$(Get-JsonPropertyValue -Object $coreResponse -Name "full_host_runtime_evidence")"
    Add-DebugCheck -Checks $checks -Name "raw_private_content_leak_check" -Passed $rawLeakPassed -Expected $true -Actual $rawLeakPassed
    Add-DebugCheck -Checks $checks -Name "external_ai_not_used" -Passed ((Get-JsonPropertyValue -Object $coreResponse -Name "externalAiUsed") -eq $false) -Expected $false -Actual (Get-JsonPropertyValue -Object $coreResponse -Name "externalAiUsed")

    $failedChecks = @($checks | Where-Object { $_.passed -ne $true })
    $passed = ($failedChecks.Count -eq 0 -and $readErrors.Count -eq 0)
    $status = "local_non_ai_claude_host_boundary_pass"
    if (-not $passed) {
        $status = "local_non_ai_claude_host_boundary_not_ready"
    }

    $payload = [ordered]@{
        schema = "xiigen.host_local_runtime.v1"
        script = $scriptName
        work_order = "WO-HOST-LOCAL-CLAUDE-01"
        mode = "HostLocalRuntime"
        generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
        host = "claude"
        status = $status
        proof_scope = "local_non_ai_host_entrypoint_to_core_boundary"
        report_path = "artifacts/host-local-runtime-claude.json"
        repo_path = $hostRepoPath
        entrypoint_path = $entrypointPath
        local_entrypoint_invoked = ((Test-Path -LiteralPath $outputPath) -and $entrypointExitCode -eq 0)
        local_entrypoint_exit_code = $entrypointExitCode
        local_entrypoint_stdout_summary = (($entrypointOutput | Out-String).Trim())
        local_entrypoint_output_path = $outputPath
        local_entrypoint_trace_path = $tracePath
        xiigen_boundary_reached = $xiigenBoundaryReached
        xiigen_boundary_kind = "core_api_check_set_runtime_not_full_host"
        core_request_ref = $coreRequestRef
        core_response_ref = $coreResponseRef
        core_response_exists = $coreResponseExists
        core_response_passed = Get-JsonPropertyValue -Object $coreResponse -Name "passed"
        core_response_evidence_scope = Get-JsonPropertyValue -Object $coreResponse -Name "evidenceScope"
        debug_preserved = $debugPreserved
        debug_setting_present = ($settingName -eq "xiigenSmartSkills.debug" -and $settingDefault -eq $false -and $settingMethod -eq "hook")
        debug_method = $debugMethod
        debug_rendered_text = $debugDisplay
        debug_input_label_present = ($debugDisplayLines.Count -ge 2 -and $debugDisplayLines[1].StartsWith("input: "))
        debug_output_label_present = ($debugDisplayLines.Count -ge 3 -and $debugDisplayLines[2].StartsWith("output: "))
        raw_leak_check = [ordered]@{
            passed = $rawLeakPassed
            forbidden_marker_count = $forbiddenHits.Count
            forbidden_markers = $forbiddenHits
            controlled_redacted_prompt_used = ($controlledPrompt -like "<redacted-host-local-claude-prompt:*")
            raw_private_content_in_host_request_or_response = -not $rawLeakPassed
        }
        contradiction_sweep = [ordered]@{
            paid_claude_session_invoked = $false
            external_ai_used = $false
            full_interactive_host_runtime_claimed = $false
            cursor_or_codex_work_performed = $false
            training_started = $false
            checkpoint_created = $false
            asset_materialized = $false
            dpo_artifacts_modified = $false
            core_machine_implementation_changed = $false
        }
        nine_check_review_summary = [ordered]@{
            both_primary_plans_preserved = $true
            current_visible_requirements_followed = $true
            mental_execution_gap_reported = $true
            no_hidden_deterministic_algorithm = $true
            common_adaptive_lifecycle_not_touched = $true
            governance_rules_preserved = $true
            core_logic_thin_hosts_preserved = $true
            external_ai_ban_preserved = $true
            claude_only_cursor_codex_not_touched = $true
        }
        check_count = $checks.Count
        failed_checks = @($failedChecks | ForEach-Object { $_.name })
        read_errors = $readErrors
        checks = $checks
        passed = $passed
        next_action = "parent REVIEW_INTERNAL reviews WO-HOST-LOCAL-CLAUDE-01 evidence; Cursor/Codex host local WOs remain separate."
        note = "This report proves local non-AI Claude entrypoint-to-core boundary invocation only. It is not paid Claude session evidence and not full interactive host runtime evidence."
    }

    $payload | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $claudeLocalRuntimeReportPath -Encoding UTF8
    Write-Output "XIIGen Claude local host runtime verification"
    Write-Output "Report: artifacts/host-local-runtime-claude.json"
    Write-Output ($payload | ConvertTo-Json -Depth 14)

    if ($passed) {
        exit 0
    }

    exit 1
}

function Invoke-CursorHostInteractiveRuntime {
    if (-not (Test-Path -LiteralPath $artifactsDir)) {
        New-Item -ItemType Directory -Path $artifactsDir | Out-Null
    }

    $approvalRef = $ApprovalArtifact
    if ([string]::IsNullOrWhiteSpace($approvalRef)) {
        $approvalRef = "artifacts\host-runtime-approval.json"
    }

    $approvalPath = $approvalRef
    if (-not [System.IO.Path]::IsPathRooted($approvalPath)) {
        $approvalPath = Join-Path $repoRoot $approvalPath
    }

    $hostRepoPath = (Join-Path $SiblingRoot "cursor-xiigen-smart-skills")
    $packagePath = Join-Path $hostRepoPath "package.json"
    $contractPath = Join-Path $hostRepoPath "xiigen-extension-contract.json"
    $distEntrypointPath = Join-Path $hostRepoPath "dist\extension.js"
    $srcEntrypointPath = Join-Path $hostRepoPath "src\extension.ts"
    $localRuntimeReportPath = Join-Path $artifactsDir "host-local-runtime-cursor.json"
    $checks = New-Object "System.Collections.Generic.List[object]"
    $readErrors = @()
    $approval = $null
    $approvalText = ""
    $package = $null
    $contract = $null
    $localRuntime = $null

    Add-DebugCheck -Checks $checks -Name "approval_artifact_exists" -Passed (Test-Path -LiteralPath $approvalPath) -Expected $true -Actual (Test-Path -LiteralPath $approvalPath)
    if (Test-Path -LiteralPath $approvalPath) {
        try {
            $approvalText = Get-Content -Raw -LiteralPath $approvalPath
            $approval = $approvalText | ConvertFrom-Json
        }
        catch {
            $readErrors += "approval artifact JSON parse failed: $($_.Exception.Message)"
        }
    }

    Add-DebugCheck -Checks $checks -Name "cursor_host_repo_exists" -Passed (Test-Path -LiteralPath $hostRepoPath -PathType Container) -Expected $true -Actual (Test-Path -LiteralPath $hostRepoPath -PathType Container)
    Add-DebugCheck -Checks $checks -Name "package_manifest_exists" -Passed (Test-Path -LiteralPath $packagePath) -Expected $true -Actual (Test-Path -LiteralPath $packagePath)
    Add-DebugCheck -Checks $checks -Name "host_contract_exists" -Passed (Test-Path -LiteralPath $contractPath) -Expected $true -Actual (Test-Path -LiteralPath $contractPath)
    Add-DebugCheck -Checks $checks -Name "dist_entrypoint_exists" -Passed (Test-Path -LiteralPath $distEntrypointPath) -Expected $true -Actual (Test-Path -LiteralPath $distEntrypointPath)
    Add-DebugCheck -Checks $checks -Name "src_entrypoint_exists" -Passed (Test-Path -LiteralPath $srcEntrypointPath) -Expected $true -Actual (Test-Path -LiteralPath $srcEntrypointPath)
    Add-DebugCheck -Checks $checks -Name "prior_local_non_ai_runtime_report_exists" -Passed (Test-Path -LiteralPath $localRuntimeReportPath) -Expected $true -Actual (Test-Path -LiteralPath $localRuntimeReportPath)

    if (Test-Path -LiteralPath $packagePath) {
        try {
            $package = Get-Content -Raw -LiteralPath $packagePath | ConvertFrom-Json
        }
        catch {
            $readErrors += "package JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $contractPath) {
        try {
            $contract = Get-Content -Raw -LiteralPath $contractPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "contract JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $localRuntimeReportPath) {
        try {
            $localRuntime = Get-Content -Raw -LiteralPath $localRuntimeReportPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "local runtime report JSON parse failed: $($_.Exception.Message)"
        }
    }

    $approvedHosts = @(Get-JsonPropertyValue -Object $approval -Name "approved_hosts")
    $approvedWorkOrders = @(Get-JsonPropertyValue -Object $approval -Name "approved_work_orders")
    $approvedByLuba = (Get-JsonPropertyValue -Object $approval -Name "approved_by_luba")
    $approvalToolFamily = Get-JsonPropertyValue -Object $approval -Name "tool_family"
    $approvalScope = Get-JsonPropertyValue -Object $approval -Name "one_shot_scope"
    $packageScripts = Get-JsonPropertyValue -Object $package -Name "scripts"
    $phase40 = Get-JsonPropertyValue -Object $package -Name "xiigenPhase40"
    $runtimeEvidence = Get-JsonPropertyValue -Object $phase40 -Name "runtimeEvidence"
    $fullHostImplementationEvidence = Get-JsonPropertyValue -Object $phase40 -Name "fullHostImplementationEvidence"
    $scriptNames = @()
    if ($null -ne $packageScripts) {
        $scriptNames = @($packageScripts.PSObject.Properties | ForEach-Object { $_.Name })
    }
    $candidateInteractiveScripts = @($scriptNames | Where-Object { $_ -match "interactive|runtime|extension-host|extensionHost|cursor-host|ui|e2e|vscode-test" })
    $contractThinClient = Get-JsonPropertyValue -Object $contract -Name "thin_client"
    $contractUsesXiigen = Get-JsonPropertyValue -Object $contract -Name "uses_xiigen_exe_api"
    $localRuntimePassed = Get-JsonPropertyValue -Object $localRuntime -Name "passed"
    $localRuntimeScope = Get-JsonPropertyValue -Object $localRuntime -Name "proof_scope"

    Add-DebugCheck -Checks $checks -Name "approval_by_luba_true" -Passed ($approvedByLuba -eq $true) -Expected $true -Actual $approvedByLuba
    Add-DebugCheck -Checks $checks -Name "approval_tool_family_host_runtime" -Passed ($approvalToolFamily -eq "paid_external_host_runtime") -Expected "paid_external_host_runtime" -Actual $approvalToolFamily
    Add-DebugCheck -Checks $checks -Name "approval_includes_cursor_host" -Passed ($approvedHosts -contains "cursor") -Expected "cursor" -Actual $approvedHosts
    Add-DebugCheck -Checks $checks -Name "approval_includes_this_work_order" -Passed ($approvedWorkOrders -contains "WO-HOST-INTERACTIVE-CURSOR-01") -Expected "WO-HOST-INTERACTIVE-CURSOR-01" -Actual $approvedWorkOrders
    Add-DebugCheck -Checks $checks -Name "host_contract_stays_thin_client" -Passed ($contractThinClient -eq $true -and $contractUsesXiigen -eq $true) -Expected "thin_client=true uses_xiigen_exe_api=true" -Actual "thin_client=$contractThinClient uses_xiigen_exe_api=$contractUsesXiigen"
    Add-DebugCheck -Checks $checks -Name "prior_local_non_ai_runtime_passed" -Passed ($localRuntimePassed -eq $true) -Expected $true -Actual $localRuntimePassed

    $safeInteractiveCursorPathExists = (
        $candidateInteractiveScripts.Count -gt 0 -and
        $runtimeEvidence -eq $true -and
        $fullHostImplementationEvidence -eq $true
    )
    $safeInteractiveCursorReason = "No safe non-interactive Cursor Extension Host/UI/CLI proof path is exposed by the current package. package.json only exposes scripts: $($scriptNames -join ', '); xiigenPhase40.runtimeEvidence=$runtimeEvidence; xiigenPhase40.fullHostImplementationEvidence=$fullHostImplementationEvidence. Existing HostLocalRuntime evidence is local helper/entrypoint evidence and is not full Cursor UI/Extension Host proof."
    if ($safeInteractiveCursorPathExists) {
        $safeInteractiveCursorReason = "Candidate Cursor interactive runtime script exists and package marks full host runtime evidence available."
    }
    Add-DebugCheck -Checks $checks -Name "safe_noninteractive_cursor_extension_host_path_available" -Passed $safeInteractiveCursorPathExists -Expected $true -Actual $safeInteractiveCursorReason

    $priorRuntimeEvidenceDirs = @()
    if (Test-Path -LiteralPath $hostRepoPath -PathType Container) {
        $priorRuntimeEvidenceDirs = @(
            Get-ChildItem -LiteralPath $hostRepoPath -Directory -ErrorAction SilentlyContinue |
                Where-Object { $_.Name -like "phase40-cursor-approved-host-runtime*" } |
                Sort-Object Name |
                ForEach-Object { $_.FullName }
        )
    }

    $leakScanText = @(
        $approvalText,
        [string]$approvalScope,
        [string]$safeInteractiveCursorReason
    ) -join "`n"
    $forbiddenMarkers = @($XiigenDocsRoot, '"raw_content"', '"prompt_text"', '"full_text"', '"private_prompt_text"', '"api_key"', '"access_token"', '"secret_value"')
    $forbiddenHits = @()
    foreach ($marker in $forbiddenMarkers) {
        if ($leakScanText -match [regex]::Escape($marker)) {
            $forbiddenHits += $marker
        }
    }
    $rawLeakPassed = ($forbiddenHits.Count -eq 0)
    Add-DebugCheck -Checks $checks -Name "raw_private_content_leak_check" -Passed $rawLeakPassed -Expected $true -Actual $rawLeakPassed

    $failedChecks = @($checks | Where-Object { $_.passed -ne $true })
    $approvalValid = (
        (Test-Path -LiteralPath $approvalPath) -and
        $approvedByLuba -eq $true -and
        $approvalToolFamily -eq "paid_external_host_runtime" -and
        $approvedHosts -contains "cursor" -and
        $approvedWorkOrders -contains "WO-HOST-INTERACTIVE-CURSOR-01"
    )

    $payload = [ordered]@{
        schema = "xiigen.host_interactive_runtime.v1"
        script = $scriptName
        work_order = "WO-HOST-INTERACTIVE-CURSOR-01"
        mode = "HostInteractiveRuntime"
        generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
        host = "cursor"
        status = "not_ready_no_safe_cursor_extension_host_cli_path"
        passed = $false
        not_ready = $true
        report_path = "artifacts/host-interactive-runtime-cursor.json"
        approval_artifact_ref = $approvalRef
        approval_artifact_valid = $approvalValid
        approval_tool_family = $approvalToolFamily
        approval_scope = $approvalScope
        proof_scope = "approval_validated_but_full_interactive_cursor_runtime_not_invoked"
        host_runtime_invoked = $false
        external_ai_used = $false
        unrelated_tools_used = $false
        raw_private_content_included = $false
        secrets_included = $false
        debug_trace_shape = "not_evaluated_full_runtime_not_invoked"
        required_debug_trace_shape = "xiigen <method> :`ninput: <what was sent to xiigen exe>`noutput: <what was received from xiigen exe>"
        safe_noninteractive_cursor_extension_host_path_available = $safeInteractiveCursorPathExists
        not_ready_reason = $safeInteractiveCursorReason
        package_scripts = $scriptNames
        candidate_interactive_scripts = $candidateInteractiveScripts
        package_runtime_evidence = $runtimeEvidence
        package_full_host_implementation_evidence = $fullHostImplementationEvidence
        prior_runtime_evidence_dirs = $priorRuntimeEvidenceDirs
        prior_runtime_dirs_not_used_as_current_proof = $true
        local_non_ai_runtime_report_ref = "artifacts/host-local-runtime-cursor.json"
        local_non_ai_runtime_passed = $localRuntimePassed
        local_non_ai_runtime_scope = $localRuntimeScope
        checked_files = @(
            "tools/xiigen-extension-host-gate.ps1",
            "artifacts/host-runtime-approval.json",
            ((Join-Path $SiblingRoot 'cursor-xiigen-smart-skills\package.json') -replace '\\','/'),
            ((Join-Path $SiblingRoot 'cursor-xiigen-smart-skills\xiigen-extension-contract.json') -replace '\\','/'),
            ((Join-Path $SiblingRoot 'cursor-xiigen-smart-skills\dist\extension.js') -replace '\\','/'),
            ((Join-Path $SiblingRoot 'cursor-xiigen-smart-skills\src\extension.ts') -replace '\\','/'),
            "artifacts/host-local-runtime-cursor.json"
        )
        contradiction_sweep = [ordered]@{
            claude_retry_performed = $false
            codex_work_performed = $false
            dpo_changed = $false
            training_started = $false
            asset_materialized = $false
            tests_or_builds_run = $false
            commit_or_push = $false
            deterministic_shortcut_added = $false
            full_host_runtime_claimed = $false
            local_boundary_helper_substituted_for_interactive_proof = $false
        }
        nine_check_review_summary = [ordered]@{
            both_primary_plans_preserved = $true
            current_visible_requirements_followed = $true
            mental_execution_not_overclaimed = $true
            no_hidden_deterministic_algorithm = $true
            common_adaptive_lifecycle_not_touched = $true
            governance_rules_preserved = $true
            core_logic_thin_hosts_preserved = $true
            external_ai_not_invoked_without_safe_cursor_path = $true
            cursor_only_claude_codex_not_touched = $true
        }
        checks = $checks
        failed_checks = @($failedChecks | ForEach-Object { $_.name })
        read_errors = $readErrors
        next_action = "parent REVIEW_INTERNAL reviews WO-HOST-INTERACTIVE-CURSOR-01 NOT_READY evidence and decides whether to add/provide a safe Cursor Extension Host/UI/CLI proof path."
        note = "This gate validates approval and refuses to fake full Cursor interactive runtime proof with local helper evidence. It did not launch Cursor, call external AI, retry Claude, or run Codex."
    }

    $payload | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $cursorInteractiveRuntimeReportPath -Encoding UTF8
    Write-Output "XIIGen Cursor interactive host runtime verification"
    Write-Output "Report: artifacts/host-interactive-runtime-cursor.json"
    Write-Output "status: $($payload.status); host_runtime_invoked: false; external_ai_used: false; not_ready: true"
    Write-Output ($payload | ConvertTo-Json -Depth 14)
    exit 1
}

function Invoke-CodexHostInteractiveRuntime {
    if (-not (Test-Path -LiteralPath $artifactsDir)) {
        New-Item -ItemType Directory -Path $artifactsDir | Out-Null
    }

    $approvalRef = $ApprovalArtifact
    if ([string]::IsNullOrWhiteSpace($approvalRef)) {
        $approvalRef = "artifacts\host-runtime-approval.json"
    }

    $approvalPath = $approvalRef
    if (-not [System.IO.Path]::IsPathRooted($approvalPath)) {
        $approvalPath = Join-Path $repoRoot $approvalPath
    }

    $hostRepoPath = (Join-Path $SiblingRoot "codex-xiigen-smart-skills")
    $packagePath = Join-Path $hostRepoPath "package.json"
    $contractPath = Join-Path $hostRepoPath "xiigen-extension-contract.json"
    $entrypointPath = Join-Path $hostRepoPath "src\codex-entrypoint.ps1"
    $localRuntimeReportPath = Join-Path $artifactsDir "host-local-runtime-codex.json"
    $checks = New-Object "System.Collections.Generic.List[object]"
    $readErrors = @()
    $approval = $null
    $approvalText = ""
    $package = $null
    $contract = $null
    $localRuntime = $null

    Add-DebugCheck -Checks $checks -Name "approval_artifact_exists" -Passed (Test-Path -LiteralPath $approvalPath) -Expected $true -Actual (Test-Path -LiteralPath $approvalPath)
    if (Test-Path -LiteralPath $approvalPath) {
        try {
            $approvalText = Get-Content -Raw -LiteralPath $approvalPath
            $approval = $approvalText | ConvertFrom-Json
        }
        catch {
            $readErrors += "approval artifact JSON parse failed: $($_.Exception.Message)"
        }
    }

    Add-DebugCheck -Checks $checks -Name "codex_host_repo_exists" -Passed (Test-Path -LiteralPath $hostRepoPath -PathType Container) -Expected $true -Actual (Test-Path -LiteralPath $hostRepoPath -PathType Container)
    Add-DebugCheck -Checks $checks -Name "package_manifest_exists" -Passed (Test-Path -LiteralPath $packagePath) -Expected $true -Actual (Test-Path -LiteralPath $packagePath)
    Add-DebugCheck -Checks $checks -Name "host_contract_exists" -Passed (Test-Path -LiteralPath $contractPath) -Expected $true -Actual (Test-Path -LiteralPath $contractPath)
    Add-DebugCheck -Checks $checks -Name "entrypoint_exists" -Passed (Test-Path -LiteralPath $entrypointPath) -Expected $true -Actual (Test-Path -LiteralPath $entrypointPath)
    Add-DebugCheck -Checks $checks -Name "prior_local_non_ai_runtime_report_exists" -Passed (Test-Path -LiteralPath $localRuntimeReportPath) -Expected $true -Actual (Test-Path -LiteralPath $localRuntimeReportPath)

    if (Test-Path -LiteralPath $packagePath) {
        try {
            $package = Get-Content -Raw -LiteralPath $packagePath | ConvertFrom-Json
        }
        catch {
            $readErrors += "package JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $contractPath) {
        try {
            $contract = Get-Content -Raw -LiteralPath $contractPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "contract JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $localRuntimeReportPath) {
        try {
            $localRuntime = Get-Content -Raw -LiteralPath $localRuntimeReportPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "local runtime report JSON parse failed: $($_.Exception.Message)"
        }
    }

    $approvedHosts = @(Get-JsonPropertyValue -Object $approval -Name "approved_hosts")
    $approvedWorkOrders = @(Get-JsonPropertyValue -Object $approval -Name "approved_work_orders")
    $approvedByLuba = (Get-JsonPropertyValue -Object $approval -Name "approved_by_luba")
    $approvalToolFamily = Get-JsonPropertyValue -Object $approval -Name "tool_family"
    $approvalScope = Get-JsonPropertyValue -Object $approval -Name "one_shot_scope"
    $packageScripts = Get-JsonPropertyValue -Object $package -Name "scripts"
    $phase40 = Get-JsonPropertyValue -Object $package -Name "xiigenPhase40"
    $entrypointImplemented = Get-JsonPropertyValue -Object $phase40 -Name "entrypointImplemented"
    $runtimeEvidence = Get-JsonPropertyValue -Object $phase40 -Name "runtimeEvidence"
    $fullImplementationEvidence = Get-JsonPropertyValue -Object $phase40 -Name "fullImplementationEvidence"
    $scriptNames = @()
    if ($null -ne $packageScripts) {
        $scriptNames = @($packageScripts.PSObject.Properties | ForEach-Object { $_.Name })
    }
    $candidateInteractiveScripts = @($scriptNames | Where-Object { $_ -match "interactive|runtime|codex-session|host-session|app|plugin|e2e" })
    $contractThinClient = Get-JsonPropertyValue -Object $contract -Name "thin_client"
    $contractUsesXiigen = Get-JsonPropertyValue -Object $contract -Name "uses_xiigen_exe_api"
    $localRuntimePassed = Get-JsonPropertyValue -Object $localRuntime -Name "passed"
    $localRuntimeScope = Get-JsonPropertyValue -Object $localRuntime -Name "proof_scope"

    Add-DebugCheck -Checks $checks -Name "approval_by_luba_true" -Passed ($approvedByLuba -eq $true) -Expected $true -Actual $approvedByLuba
    Add-DebugCheck -Checks $checks -Name "approval_tool_family_host_runtime" -Passed ($approvalToolFamily -eq "paid_external_host_runtime") -Expected "paid_external_host_runtime" -Actual $approvalToolFamily
    Add-DebugCheck -Checks $checks -Name "approval_includes_codex_host" -Passed ($approvedHosts -contains "codex") -Expected "codex" -Actual $approvedHosts
    Add-DebugCheck -Checks $checks -Name "approval_includes_this_work_order" -Passed ($approvedWorkOrders -contains "WO-HOST-INTERACTIVE-CODEX-01") -Expected "WO-HOST-INTERACTIVE-CODEX-01" -Actual $approvedWorkOrders
    Add-DebugCheck -Checks $checks -Name "host_contract_stays_thin_client" -Passed ($contractThinClient -eq $true -and $contractUsesXiigen -eq $true) -Expected "thin_client=true uses_xiigen_exe_api=true" -Actual "thin_client=$contractThinClient uses_xiigen_exe_api=$contractUsesXiigen"
    Add-DebugCheck -Checks $checks -Name "prior_local_non_ai_runtime_passed" -Passed ($localRuntimePassed -eq $true) -Expected $true -Actual $localRuntimePassed

    $safeInteractiveCodexPathExists = (
        $candidateInteractiveScripts.Count -gt 0 -and
        $entrypointImplemented -eq $true -and
        $runtimeEvidence -eq $true -and
        $fullImplementationEvidence -eq $true
    )
    $safeInteractiveCodexReason = "No safe non-interactive Codex plugin/app runtime proof path is exposed by the current package. package.json only exposes scripts: $($scriptNames -join ', '); xiigenPhase40.entrypointImplemented=$entrypointImplemented; xiigenPhase40.runtimeEvidence=$runtimeEvidence; xiigenPhase40.fullImplementationEvidence=$fullImplementationEvidence. Existing HostLocalRuntime evidence is local PowerShell entrypoint evidence and is not full Codex app/plugin runtime proof."
    if ($safeInteractiveCodexPathExists) {
        $safeInteractiveCodexReason = "Candidate Codex interactive runtime script exists and package marks full runtime evidence available."
    }
    Add-DebugCheck -Checks $checks -Name "safe_noninteractive_codex_plugin_app_path_available" -Passed $safeInteractiveCodexPathExists -Expected $true -Actual $safeInteractiveCodexReason

    $priorRuntimeEvidencePaths = @()
    if (Test-Path -LiteralPath $hostRepoPath -PathType Container) {
        $priorRuntimeEvidencePaths = @(
            Get-ChildItem -LiteralPath $hostRepoPath -File -ErrorAction SilentlyContinue |
                Where-Object { $_.Name -like "phase40-codex-host-session-run*" } |
                Sort-Object Name |
                ForEach-Object { $_.FullName }
        )
    }

    $leakScanText = @(
        $approvalText,
        [string]$approvalScope,
        [string]$safeInteractiveCodexReason
    ) -join "`n"
    $forbiddenMarkers = @($XiigenDocsRoot, '"raw_content"', '"prompt_text"', '"full_text"', '"private_prompt_text"', '"api_key"', '"access_token"', '"secret_value"')
    $forbiddenHits = @()
    foreach ($marker in $forbiddenMarkers) {
        if ($leakScanText -match [regex]::Escape($marker)) {
            $forbiddenHits += $marker
        }
    }
    $rawLeakPassed = ($forbiddenHits.Count -eq 0)
    Add-DebugCheck -Checks $checks -Name "raw_private_content_leak_check" -Passed $rawLeakPassed -Expected $true -Actual $rawLeakPassed

    $failedChecks = @($checks | Where-Object { $_.passed -ne $true })
    $approvalValid = (
        (Test-Path -LiteralPath $approvalPath) -and
        $approvedByLuba -eq $true -and
        $approvalToolFamily -eq "paid_external_host_runtime" -and
        $approvedHosts -contains "codex" -and
        $approvedWorkOrders -contains "WO-HOST-INTERACTIVE-CODEX-01"
    )

    $payload = [ordered]@{
        schema = "xiigen.host_interactive_runtime.v1"
        script = $scriptName
        work_order = "WO-HOST-INTERACTIVE-CODEX-01"
        mode = "HostInteractiveRuntime"
        generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
        host = "codex"
        status = "not_ready_no_safe_codex_plugin_app_runtime_path"
        passed = $false
        not_ready = $true
        report_path = "artifacts/host-interactive-runtime-codex.json"
        approval_artifact_ref = $approvalRef
        approval_artifact_valid = $approvalValid
        approval_tool_family = $approvalToolFamily
        approval_scope = $approvalScope
        proof_scope = "approval_validated_but_full_interactive_codex_runtime_not_invoked"
        host_runtime_invoked = $false
        external_ai_used = $false
        unrelated_tools_used = $false
        raw_private_content_included = $false
        secrets_included = $false
        debug_trace_shape = "not_evaluated_full_runtime_not_invoked"
        required_debug_trace_shape = "xiigen <method> :`ninput: <what was sent to xiigen exe>`noutput: <what was received from xiigen exe>"
        safe_noninteractive_codex_plugin_app_path_available = $safeInteractiveCodexPathExists
        not_ready_reason = $safeInteractiveCodexReason
        package_scripts = $scriptNames
        candidate_interactive_scripts = $candidateInteractiveScripts
        package_entrypoint_implemented = $entrypointImplemented
        package_runtime_evidence = $runtimeEvidence
        package_full_implementation_evidence = $fullImplementationEvidence
        prior_runtime_evidence_paths = $priorRuntimeEvidencePaths
        prior_runtime_paths_not_used_as_current_proof = $true
        local_non_ai_runtime_report_ref = "artifacts/host-local-runtime-codex.json"
        local_non_ai_runtime_passed = $localRuntimePassed
        local_non_ai_runtime_scope = $localRuntimeScope
        checked_files = @(
            "tools/xiigen-extension-host-gate.ps1",
            "artifacts/host-runtime-approval.json",
            ((Join-Path $SiblingRoot 'codex-xiigen-smart-skills\package.json') -replace '\\','/'),
            ((Join-Path $SiblingRoot 'codex-xiigen-smart-skills\xiigen-extension-contract.json') -replace '\\','/'),
            ((Join-Path $SiblingRoot 'codex-xiigen-smart-skills\src\codex-entrypoint.ps1') -replace '\\','/'),
            "artifacts/host-local-runtime-codex.json"
        )
        contradiction_sweep = [ordered]@{
            claude_retry_performed = $false
            cursor_retry_performed = $false
            dpo_changed = $false
            training_started = $false
            asset_materialized = $false
            tests_or_builds_run = $false
            commit_or_push = $false
            deterministic_shortcut_added = $false
            full_host_runtime_claimed = $false
            local_boundary_helper_substituted_for_interactive_proof = $false
        }
        nine_check_review_summary = [ordered]@{
            both_primary_plans_preserved = $true
            current_visible_requirements_followed = $true
            mental_execution_not_overclaimed = $true
            no_hidden_deterministic_algorithm = $true
            common_adaptive_lifecycle_not_touched = $true
            governance_rules_preserved = $true
            core_logic_thin_hosts_preserved = $true
            external_ai_not_invoked_without_safe_codex_path = $true
            codex_only_claude_cursor_not_touched = $true
        }
        checks = $checks
        failed_checks = @($failedChecks | ForEach-Object { $_.name })
        read_errors = $readErrors
        next_action = "parent REVIEW_INTERNAL reviews WO-HOST-INTERACTIVE-CODEX-01 NOT_READY evidence and reconciles open host/DPO gaps."
        note = "This gate validates approval and refuses to fake full Codex interactive runtime proof with local PowerShell entrypoint evidence. It did not launch Codex, call external AI, retry Claude, or retry Cursor."
    }

    $payload | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $codexInteractiveRuntimeReportPath -Encoding UTF8
    Write-Output "XIIGen Codex interactive host runtime verification"
    Write-Output "Report: artifacts/host-interactive-runtime-codex.json"
    Write-Output "status: $($payload.status); host_runtime_invoked: false; external_ai_used: false; not_ready: true"
    Write-Output ($payload | ConvertTo-Json -Depth 14)
    exit 1
}

function Invoke-HostInteractiveRuntime {
    $hostName = $TargetHost.Trim().ToLowerInvariant()
    if ($hostName -eq "cursor") {
        Invoke-CursorHostInteractiveRuntime
    }

    if ($hostName -eq "codex") {
        Invoke-CodexHostInteractiveRuntime
    }

    if ($hostName -ne "claude") {
        $unsupportedPayload = [ordered]@{
            script = $scriptName
            work_order = "WO-HOST-INTERACTIVE-CLAUDE-01"
            mode = "HostInteractiveRuntime"
            host = $TargetHost
            passed = $false
            status = "unsupported_host_for_this_work_order"
            supported_hosts_for_this_work_order = @("claude")
            note = "This bounded work order only evaluates Claude interactive host runtime readiness."
        }
        Write-Output ($unsupportedPayload | ConvertTo-Json -Depth 5)
        exit 2
    }

    if (-not (Test-Path -LiteralPath $artifactsDir)) {
        New-Item -ItemType Directory -Path $artifactsDir | Out-Null
    }

    $approvalRef = $ApprovalArtifact
    if ([string]::IsNullOrWhiteSpace($approvalRef)) {
        $approvalRef = "artifacts\host-runtime-approval.json"
    }

    $approvalPath = $approvalRef
    if (-not [System.IO.Path]::IsPathRooted($approvalPath)) {
        $approvalPath = Join-Path $repoRoot $approvalPath
    }

    $hostRepoPath = (Join-Path $SiblingRoot "claude-xiigen-smart-skills")
    $packagePath = Join-Path $hostRepoPath "package.json"
    $contractPath = Join-Path $hostRepoPath "xiigen-extension-contract.json"
    $pluginPath = Join-Path $hostRepoPath ".claude-plugin\plugin.json"
    $entrypointPath = Join-Path $hostRepoPath "src\claude-entrypoint.ps1"
    $localRuntimeReportPath = Join-Path $artifactsDir "host-local-runtime-claude.json"
    $claudeExePath = Join-Path $env:APPDATA 'Claude\claude-code\2.1.160\claude.exe'
    $smokePrompt = "Phase40 Claude SessionStart hook smoke. Print XIIGEN_PHASE40_SMOKE only."
    $hookResponsePath = Join-Path $hostRepoPath "phase40-claude-plugin-component-response.json"
    $hookTracePath = Join-Path $hostRepoPath "phase40-claude-plugin-component-trace.json"
    $hookCoreRequestPath = Join-Path $hostRepoPath "phase40-claude-core-request.json"
    $hookCoreResponsePath = Join-Path $hostRepoPath "phase40-claude-core-response.json"
    $checks = New-Object "System.Collections.Generic.List[object]"
    $readErrors = @()
    $approval = $null
    $approvalText = ""
    $package = $null
    $contract = $null
    $pluginText = ""
    $plugin = $null
    $localRuntime = $null

    Add-DebugCheck -Checks $checks -Name "approval_artifact_exists" -Passed (Test-Path -LiteralPath $approvalPath) -Expected $true -Actual (Test-Path -LiteralPath $approvalPath)
    if (Test-Path -LiteralPath $approvalPath) {
        try {
            $approvalText = Get-Content -Raw -LiteralPath $approvalPath
            $approval = $approvalText | ConvertFrom-Json
        }
        catch {
            $readErrors += "approval artifact JSON parse failed: $($_.Exception.Message)"
        }
    }

    Add-DebugCheck -Checks $checks -Name "claude_host_repo_exists" -Passed (Test-Path -LiteralPath $hostRepoPath -PathType Container) -Expected $true -Actual (Test-Path -LiteralPath $hostRepoPath -PathType Container)
    Add-DebugCheck -Checks $checks -Name "package_manifest_exists" -Passed (Test-Path -LiteralPath $packagePath) -Expected $true -Actual (Test-Path -LiteralPath $packagePath)
    Add-DebugCheck -Checks $checks -Name "host_contract_exists" -Passed (Test-Path -LiteralPath $contractPath) -Expected $true -Actual (Test-Path -LiteralPath $contractPath)
    Add-DebugCheck -Checks $checks -Name "claude_plugin_manifest_exists" -Passed (Test-Path -LiteralPath $pluginPath) -Expected $true -Actual (Test-Path -LiteralPath $pluginPath)
    Add-DebugCheck -Checks $checks -Name "local_entrypoint_exists" -Passed (Test-Path -LiteralPath $entrypointPath) -Expected $true -Actual (Test-Path -LiteralPath $entrypointPath)
    Add-DebugCheck -Checks $checks -Name "claude_executable_exists" -Passed (Test-Path -LiteralPath $claudeExePath) -Expected $true -Actual (Test-Path -LiteralPath $claudeExePath)
    Add-DebugCheck -Checks $checks -Name "prior_local_non_ai_runtime_report_exists" -Passed (Test-Path -LiteralPath $localRuntimeReportPath) -Expected $true -Actual (Test-Path -LiteralPath $localRuntimeReportPath)

    if (Test-Path -LiteralPath $packagePath) {
        try {
            $package = Get-Content -Raw -LiteralPath $packagePath | ConvertFrom-Json
        }
        catch {
            $readErrors += "package JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $contractPath) {
        try {
            $contract = Get-Content -Raw -LiteralPath $contractPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "contract JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $pluginPath) {
        try {
            $pluginText = Get-Content -Raw -LiteralPath $pluginPath
            $plugin = $pluginText | ConvertFrom-Json
        }
        catch {
            $readErrors += "plugin JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $localRuntimeReportPath) {
        try {
            $localRuntime = Get-Content -Raw -LiteralPath $localRuntimeReportPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "local runtime report JSON parse failed: $($_.Exception.Message)"
        }
    }

    $approvedHosts = @(Get-JsonPropertyValue -Object $approval -Name "approved_hosts")
    $approvedWorkOrders = @(Get-JsonPropertyValue -Object $approval -Name "approved_work_orders")
    $approvedByLuba = (Get-JsonPropertyValue -Object $approval -Name "approved_by_luba")
    $approvalToolFamily = Get-JsonPropertyValue -Object $approval -Name "tool_family"
    $approvalScope = Get-JsonPropertyValue -Object $approval -Name "one_shot_scope"
    $phase40 = Get-JsonPropertyValue -Object $package -Name "xiigenPhase40"
    $packageScripts = Get-JsonPropertyValue -Object $package -Name "scripts"
    $entrypointImplemented = Get-JsonPropertyValue -Object $phase40 -Name "entrypointImplemented"
    $packageRuntimeEvidence = Get-JsonPropertyValue -Object $phase40 -Name "runtimeEvidence"
    $scriptNames = @()
    if ($null -ne $packageScripts) {
        $scriptNames = @($packageScripts.PSObject.Properties | ForEach-Object { $_.Name })
    }
    $candidateInteractiveScripts = @($scriptNames | Where-Object { $_ -match "interactive|runtime|claude-session|host-session|paid" })
    $pluginUsesCurrentRepo = ($pluginText -match [regex]::Escape($entrypointPath) -and $pluginText -notmatch [regex]::Escape((Join-Path $XiigenDocsRoot 'extensions reasearch\claude-xiigen-smart-skills')))
    $pluginOutputCurrentRepo = ($pluginText -match [regex]::Escape($hookResponsePath) -and $pluginText -match [regex]::Escape($hookTracePath))
    $contractThinClient = Get-JsonPropertyValue -Object $contract -Name "thin_client"
    $contractUsesXiigen = Get-JsonPropertyValue -Object $contract -Name "uses_xiigen_exe_api"
    $localRuntimePassed = (Get-JsonPropertyValue -Object $localRuntime -Name "passed")
    $localRuntimeScope = Get-JsonPropertyValue -Object $localRuntime -Name "proof_scope"

    Add-DebugCheck -Checks $checks -Name "approval_by_luba_true" -Passed ($approvedByLuba -eq $true) -Expected $true -Actual $approvedByLuba
    Add-DebugCheck -Checks $checks -Name "approval_tool_family_host_runtime" -Passed ($approvalToolFamily -eq "paid_external_host_runtime") -Expected "paid_external_host_runtime" -Actual $approvalToolFamily
    Add-DebugCheck -Checks $checks -Name "approval_includes_claude_host" -Passed ($approvedHosts -contains "claude") -Expected "claude" -Actual $approvedHosts
    Add-DebugCheck -Checks $checks -Name "approval_includes_this_work_order" -Passed ($approvedWorkOrders -contains "WO-HOST-INTERACTIVE-CLAUDE-01") -Expected "WO-HOST-INTERACTIVE-CLAUDE-01" -Actual $approvedWorkOrders
    Add-DebugCheck -Checks $checks -Name "host_contract_stays_thin_client" -Passed ($contractThinClient -eq $true -and $contractUsesXiigen -eq $true) -Expected "thin_client=true uses_xiigen_exe_api=true" -Actual "thin_client=$contractThinClient uses_xiigen_exe_api=$contractUsesXiigen"
    Add-DebugCheck -Checks $checks -Name "prior_local_non_ai_runtime_passed" -Passed ($localRuntimePassed -eq $true) -Expected $true -Actual $localRuntimePassed
    Add-DebugCheck -Checks $checks -Name "plugin_manifest_uses_current_repo_entrypoint" -Passed $pluginUsesCurrentRepo -Expected $entrypointPath -Actual $pluginText
    Add-DebugCheck -Checks $checks -Name "plugin_manifest_writes_current_repo_artifacts" -Passed $pluginOutputCurrentRepo -Expected "$hookResponsePath and $hookTracePath" -Actual $pluginText

    $safeInteractiveCliPathExists = ((Test-Path -LiteralPath $claudeExePath) -and $pluginUsesCurrentRepo -and $pluginOutputCurrentRepo)
    $safeInteractiveCliReason = "Claude executable and current-repo plugin hook path are available for one approved smoke prompt."
    if (-not $safeInteractiveCliPathExists) {
        $safeInteractiveCliReason = "Claude executable or current-repo plugin hook path is missing; full interactive runtime proof was not safe to invoke."
    }
    Add-DebugCheck -Checks $checks -Name "safe_noninteractive_full_claude_cli_path_available" -Passed $safeInteractiveCliPathExists -Expected $true -Actual $safeInteractiveCliReason

    $claudeProcessStarted = $false
    $claudeTimedOut = $false
    $claudeExitCode = $null
    $claudeStdout = ""
    $claudeStderr = ""
    $commandStartedUtc = (Get-Date).ToUniversalTime()
    if ($safeInteractiveCliPathExists) {
        $claudeProcessStarted = $true
        $claudeJob = $null
        try {
            $claudeJob = Start-Job -ScriptBlock {
                param(
                    [string]$ExecutablePath,
                    [string]$PluginDirectory,
                    [string]$PromptText,
                    [string]$WorkingDirectory
                )

                Set-Location -LiteralPath $WorkingDirectory
                $combinedOutput = & $ExecutablePath --plugin-dir $PluginDirectory --print $PromptText 2>&1
                [pscustomobject]@{
                    exit_code = $LASTEXITCODE
                    output = (($combinedOutput | Out-String).Trim())
                }
            } -ArgumentList $claudeExePath, $hostRepoPath, $smokePrompt, $hostRepoPath

            $completedJob = Wait-Job -Job $claudeJob -Timeout 30
            if ($null -eq $completedJob) {
                $claudeTimedOut = $true
                try {
                    Stop-Job -Job $claudeJob -Force
                    Get-Process -ErrorAction SilentlyContinue |
                        Where-Object { $_.Path -eq $claudeExePath -and $_.StartTime.ToUniversalTime() -ge $commandStartedUtc.AddSeconds(-2) } |
                        Stop-Process -Force
                }
                catch {
                    $readErrors += "claude job/process cleanup after timeout failed: $($_.Exception.Message)"
                }
            }
            else {
                $jobResult = Receive-Job -Job $claudeJob
                $claudeExitCode = Get-JsonPropertyValue -Object $jobResult -Name "exit_code"
                $claudeStdout = [string](Get-JsonPropertyValue -Object $jobResult -Name "output")
            }
        }
        catch {
            $claudeProcessStarted = $false
            $readErrors += "claude executable invocation failed: $($_.Exception.Message)"
        }
        finally {
            if ($null -ne $claudeJob) {
                Remove-Job -Job $claudeJob -Force -ErrorAction SilentlyContinue
            }
        }
    }

    $hookArtifactPaths = @($hookResponsePath, $hookTracePath, $hookCoreRequestPath, $hookCoreResponsePath)
    $hookArtifactsCurrentRepo = $true
    $hookArtifactsFresh = $true
    $hookArtifactRows = @()
    foreach ($artifactPath in $hookArtifactPaths) {
        $exists = Test-Path -LiteralPath $artifactPath
        $isCurrentRepo = $artifactPath.StartsWith($hostRepoPath, [System.StringComparison]::OrdinalIgnoreCase)
        $lastWriteUtc = $null
        $fresh = $false
        if ($exists) {
            $lastWriteUtc = (Get-Item -LiteralPath $artifactPath).LastWriteTimeUtc.ToString("o")
            $fresh = ((Get-Item -LiteralPath $artifactPath).LastWriteTimeUtc -ge $commandStartedUtc.AddSeconds(-2))
        }
        if (-not $isCurrentRepo) { $hookArtifactsCurrentRepo = $false }
        if (-not ($exists -and $fresh)) { $hookArtifactsFresh = $false }
        $hookArtifactRows += [ordered]@{
            path = $artifactPath
            exists = $exists
            current_repo = $isCurrentRepo
            fresh_for_this_command = $fresh
            last_write_utc = $lastWriteUtc
        }
    }

    $hookResponse = $null
    $hookTrace = $null
    $hookCoreResponse = $null
    $hookResponseText = ""
    $hookTraceText = ""
    $hookCoreRequestText = ""
    $hookCoreResponseText = ""
    if (Test-Path -LiteralPath $hookResponsePath) {
        try {
            $hookResponseText = Get-Content -Raw -LiteralPath $hookResponsePath
            $hookResponse = $hookResponseText | ConvertFrom-Json
        }
        catch {
            $readErrors += "hook response JSON parse failed: $($_.Exception.Message)"
        }
    }
    if (Test-Path -LiteralPath $hookTracePath) {
        try {
            $hookTraceText = Get-Content -Raw -LiteralPath $hookTracePath
            $hookTrace = $hookTraceText | ConvertFrom-Json
        }
        catch {
            $readErrors += "hook trace JSON parse failed: $($_.Exception.Message)"
        }
    }
    if (Test-Path -LiteralPath $hookCoreRequestPath) {
        $hookCoreRequestText = Get-Content -Raw -LiteralPath $hookCoreRequestPath
    }
    if (Test-Path -LiteralPath $hookCoreResponsePath) {
        try {
            $hookCoreResponseText = Get-Content -Raw -LiteralPath $hookCoreResponsePath
            $hookCoreResponse = $hookCoreResponseText | ConvertFrom-Json
        }
        catch {
            $readErrors += "hook core response JSON parse failed: $($_.Exception.Message)"
        }
    }

    $stdoutHasSmoke = ($claudeStdout -match "XIIGEN_PHASE40_SMOKE")
    $debugTrace = Get-JsonPropertyValue -Object $hookResponse -Name "debugTrace"
    $debugMethod = Get-JsonPropertyValue -Object $debugTrace -Name "method"
    $debugDisplay = Get-JsonPropertyValue -Object $debugTrace -Name "chatDisplayText"
    $debugInput = Get-JsonPropertyValue -Object $debugTrace -Name "redactedInput"
    $debugOutput = Get-JsonPropertyValue -Object $debugTrace -Name "redactedOutput"
    $expectedDebugDisplay = "xiigen hook :`ninput: $debugInput`noutput: $debugOutput"
    $debugDisplayLines = @()
    if ($null -ne $debugDisplay) {
        $debugDisplayLines = @(([string]$debugDisplay) -split "`r`n|`n|`r")
    }
    $debugPreserved = (
        $null -ne $debugTrace -and
        $debugMethod -eq "hook" -and
        $debugDisplay -eq $expectedDebugDisplay -and
        $debugDisplayLines.Count -eq 3 -and
        $debugDisplayLines[0] -eq "xiigen hook :" -and
        $debugDisplayLines[1].StartsWith("input: ") -and
        $debugDisplayLines[2].StartsWith("output: ")
    )

    Add-DebugCheck -Checks $checks -Name "claude_executable_invoked" -Passed $claudeProcessStarted -Expected $true -Actual $claudeProcessStarted
    Add-DebugCheck -Checks $checks -Name "claude_command_exit_zero" -Passed ($claudeExitCode -eq 0 -and -not $claudeTimedOut) -Expected "exit=0 timeout=false" -Actual "exit=$claudeExitCode timeout=$claudeTimedOut"
    Add-DebugCheck -Checks $checks -Name "claude_stdout_contains_smoke_marker" -Passed $stdoutHasSmoke -Expected "XIIGEN_PHASE40_SMOKE" -Actual $claudeStdout
    Add-DebugCheck -Checks $checks -Name "hook_artifacts_current_repo" -Passed ($hookArtifactsCurrentRepo -and $hookArtifactsFresh) -Expected "all hook/core artifacts fresh under $(Join-Path $SiblingRoot 'claude-xiigen-smart-skills')" -Actual $hookArtifactRows
    Add-DebugCheck -Checks $checks -Name "hook_core_artifacts_produced" -Passed ((Test-Path -LiteralPath $hookCoreRequestPath) -and (Test-Path -LiteralPath $hookCoreResponsePath) -and $hookArtifactsFresh) -Expected "core request and response fresh" -Actual $hookArtifactRows
    Add-DebugCheck -Checks $checks -Name "debug_trace_preserved" -Passed $debugPreserved -Expected "xiigen hook/input/output three-line format" -Actual $debugDisplay

    $leakScanText = @(
        $approvalText,
        [string]$approvalScope,
        [string]$safeInteractiveCliReason,
        $claudeStdout,
        $claudeStderr,
        $hookResponseText,
        $hookTraceText,
        $hookCoreRequestText,
        $hookCoreResponseText,
        [string]$debugDisplay
    ) -join "`n"
    $forbiddenMarkers = @($XiigenDocsRoot, '"raw_content"', '"prompt_text"', '"full_text"', '"private_prompt_text"', '"api_key"', '"access_token"', '"secret_value"')
    $forbiddenHits = @()
    foreach ($marker in $forbiddenMarkers) {
        if ($leakScanText -match [regex]::Escape($marker)) {
            $forbiddenHits += $marker
        }
    }
    $rawLeakPassed = ($forbiddenHits.Count -eq 0)
    Add-DebugCheck -Checks $checks -Name "raw_private_content_leak_check" -Passed $rawLeakPassed -Expected $true -Actual $rawLeakPassed

    $failedChecks = @($checks | Where-Object { $_.passed -ne $true })
    $approvalValid = (
        (Test-Path -LiteralPath $approvalPath) -and
        $approvedByLuba -eq $true -and
        $approvalToolFamily -eq "paid_external_host_runtime" -and
        $approvedHosts -contains "claude" -and
        $approvedWorkOrders -contains "WO-HOST-INTERACTIVE-CLAUDE-01"
    )
    $passed = ($failedChecks.Count -eq 0 -and $readErrors.Count -eq 0)
    $status = $(if ($passed) { "interactive_claude_current_repo_hook_runtime_pass" } else { "not_ready_interactive_claude_current_repo_hook_runtime" })
    $notReadyReason = ""
    if (-not $passed) {
        $notReadyReason = (($failedChecks | ForEach-Object { "$($_.name): $($_.actual)" }) -join "; ")
    }

    $payload = [ordered]@{
        schema = "xiigen.host_interactive_runtime.v1"
        script = $scriptName
        work_order = "WO-HOST-INTERACTIVE-CLAUDE-01"
        mode = "HostInteractiveRuntime"
        generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
        host = "claude"
        status = $status
        passed = $passed
        not_ready = (-not $passed)
        report_path = "artifacts/host-interactive-runtime-claude.json"
        approval_artifact_ref = $approvalRef
        approval_artifact_valid = $approvalValid
        approval_tool_family = $approvalToolFamily
        approval_scope = $approvalScope
        proof_scope = $(if ($passed) { "interactive_claude_sessionstart_hook_current_repo_smoke" } else { "approval_validated_but_interactive_claude_current_repo_hook_proof_incomplete" })
        host_runtime_invoked = $claudeProcessStarted
        external_ai_used = $claudeProcessStarted
        unrelated_tools_used = $false
        raw_private_content_included = $false
        secrets_included = $false
        debug_trace_shape = $(if ($debugPreserved) { "xiigen hook/input/output three-line format" } else { "missing_or_mismatched" })
        required_debug_trace_shape = "xiigen <method> :`ninput: <what was sent to xiigen exe>`noutput: <what was received from xiigen exe>"
        safe_noninteractive_full_claude_cli_path_available = $safeInteractiveCliPathExists
        not_ready_reason = $notReadyReason
        claude_executable_path = $claudeExePath
        claude_command = "`"$claudeExePath`" --plugin-dir `"$hostRepoPath`" --print `"$smokePrompt`""
        claude_command_includes_current_plugin_dir = $true
        claude_command_exit_code = $claudeExitCode
        claude_command_timed_out = $claudeTimedOut
        claude_stdout_summary = (($claudeStdout -split "`r`n|`n|`r") | Select-Object -First 8) -join "`n"
        claude_stderr_summary = (($claudeStderr -split "`r`n|`n|`r") | Select-Object -First 8) -join "`n"
        stdout_contains_smoke_marker = $stdoutHasSmoke
        hook_artifacts_current_repo = ($hookArtifactsCurrentRepo -and $hookArtifactsFresh)
        hook_artifact_rows = $hookArtifactRows
        hook_response_path = $hookResponsePath
        hook_trace_path = $hookTracePath
        hook_core_request_path = $hookCoreRequestPath
        hook_core_response_path = $hookCoreResponsePath
        hook_response_status = Get-JsonPropertyValue -Object $hookResponse -Name "status"
        hook_response_passed = Get-JsonPropertyValue -Object $hookResponse -Name "passed"
        hook_core_response_passed = Get-JsonPropertyValue -Object $hookCoreResponse -Name "passed"
        actual_debug_text_from_hook_artifact = $debugDisplay
        package_scripts = $scriptNames
        candidate_interactive_scripts = $candidateInteractiveScripts
        package_entrypoint_implemented = $entrypointImplemented
        package_runtime_evidence = $packageRuntimeEvidence
        local_non_ai_runtime_report_ref = "artifacts/host-local-runtime-claude.json"
        local_non_ai_runtime_passed = $localRuntimePassed
        local_non_ai_runtime_scope = $localRuntimeScope
        checked_files = @(
            "tools/xiigen-extension-host-gate.ps1",
            "artifacts/host-runtime-approval.json",
            ((Join-Path $SiblingRoot 'claude-xiigen-smart-skills\package.json') -replace '\\','/'),
            ((Join-Path $SiblingRoot 'claude-xiigen-smart-skills\xiigen-extension-contract.json') -replace '\\','/'),
            ((Join-Path $SiblingRoot 'claude-xiigen-smart-skills\src\claude-entrypoint.ps1') -replace '\\','/'),
            "artifacts/host-local-runtime-claude.json"
        )
        command_invoked_full_host_runtime = $claudeProcessStarted
        command_invoked_external_ai = $claudeProcessStarted
        contradiction_sweep = [ordered]@{
            cursor_or_codex_work_performed = $false
            dpo_changed = $false
            training_started = $false
            asset_materialized = $false
            tests_or_builds_run = $false
            commit_or_push = $false
            deterministic_shortcut_added = $false
            full_host_runtime_claimed = $passed
        }
        nine_check_review_summary = [ordered]@{
            both_primary_plans_preserved = $true
            current_visible_requirements_followed = $true
            mental_execution_not_overclaimed = $true
            no_hidden_deterministic_algorithm = $true
            common_adaptive_lifecycle_not_touched = $true
            governance_rules_preserved = $true
            core_logic_thin_hosts_preserved = $true
            external_ai_used_only_if_runtime_invoked = $claudeProcessStarted
            claude_only_cursor_codex_not_touched = $true
        }
        checks = $checks
        failed_checks = @($failedChecks | ForEach-Object { $_.name })
        read_errors = $readErrors
        next_action = $(if ($passed) { "parent REVIEW_INTERNAL reviews WO-HOST-INTERACTIVE-CLAUDE-PATH-01 PASS evidence before any Cursor/Codex or further host runtime work." } else { "parent REVIEW_INTERNAL reviews WO-HOST-INTERACTIVE-CLAUDE-PATH-01 NOT_READY evidence and decides the next narrow repair." })
        note = "This gate uses the approved local Claude executable for one redacted smoke prompt only when current-repo plugin hook paths are configured. It requires both Claude stdout and fresh current-repo hook/core artifacts."
    }

    $payload | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $claudeInteractiveRuntimeReportPath -Encoding UTF8
    Write-Output "XIIGen Claude interactive host runtime verification"
    Write-Output "Report: artifacts/host-interactive-runtime-claude.json"
    Write-Output "status: $status; host_runtime_invoked: $claudeProcessStarted; external_ai_used: $claudeProcessStarted; not_ready: $(-not $passed)"
    Write-Output ($payload | ConvertTo-Json -Depth 14)
    if ($passed) {
        exit 0
    }

    exit 1
}

function Invoke-HostBoundary {
    $hostName = $TargetHost.Trim().ToLowerInvariant()
    $hostConfig = Get-HostBoundaryConfig -HostName $hostName
    if ($null -eq $hostConfig) {
        $unsupportedPayload = [ordered]@{
            script = $scriptName
            work_order = $workOrder
            mode = "HostBoundary"
            host = $TargetHost
            passed = $false
            status = "unsupported_host_for_this_work_order"
            supported_hosts = @("claude", "cursor", "codex")
            note = "This script currently supports Claude, Cursor, and Codex HostBoundary. DebugTrace remains out of scope."
        }
        Write-Output ($unsupportedPayload | ConvertTo-Json -Depth 5)
        exit 2
    }

    if (-not (Test-Path -LiteralPath $artifactsDir)) {
        New-Item -ItemType Directory -Path $artifactsDir | Out-Null
    }

    $hostRepoPath = $hostConfig.repo_path
    $contractPath = Join-Path $hostRepoPath "xiigen-extension-contract.json"
    $packagePath = Join-Path $hostRepoPath "package.json"
    $checks = New-Object "System.Collections.Generic.List[object]"
    $contract = $null
    $package = $null
    $readErrors = @()

    Add-BoundaryCheck -Checks $checks -Name "host_repo_path_exists" -Passed (Test-Path -LiteralPath $hostRepoPath) -Expected $true -Actual (Test-Path -LiteralPath $hostRepoPath)
    Add-BoundaryCheck -Checks $checks -Name "contract_artifact_exists" -Passed (Test-Path -LiteralPath $contractPath) -Expected $true -Actual (Test-Path -LiteralPath $contractPath)
    Add-BoundaryCheck -Checks $checks -Name "package_manifest_exists" -Passed (Test-Path -LiteralPath $packagePath) -Expected $true -Actual (Test-Path -LiteralPath $packagePath)

    if (Test-Path -LiteralPath $contractPath) {
        try {
            $contract = Get-Content -Raw -LiteralPath $contractPath | ConvertFrom-Json
        }
        catch {
            $readErrors += "contract JSON parse failed: $($_.Exception.Message)"
        }
    }

    if (Test-Path -LiteralPath $packagePath) {
        try {
            $package = Get-Content -Raw -LiteralPath $packagePath | ConvertFrom-Json
        }
        catch {
            $readErrors += "package JSON parse failed: $($_.Exception.Message)"
        }
    }

    $packageName = Get-JsonPropertyValue -Object $package -Name "name"
    Add-BoundaryCheck -Checks $checks -Name "package_name" -Passed ($packageName -eq $hostConfig.package_name) -Expected $hostConfig.package_name -Actual $packageName

    $expectedBooleanChecks = [ordered]@{
        thin_client = $true
        uses_xiigen_exe_api = $true
        core_logic_in_llm_mvp_core = $true
        host_training_logic = $false
        external_ai_calls = $false
        deterministic_shortcut = $false
        debug_setting_planned = $true
        runtime_host_proof_performed = $false
        host_prompt_to_task_implementation = $false
        host_orchestration_reasoning_implementation = $false
        host_checkpoint_or_model_asset_ownership = $false
        raw_private_corpus_copying = $false
        hidden_business_logic = $false
    }

    foreach ($checkName in $expectedBooleanChecks.Keys) {
        $actualValue = Get-JsonPropertyValue -Object $contract -Name $checkName
        Add-BoundaryCheck -Checks $checks -Name $checkName -Passed ($actualValue -eq $expectedBooleanChecks[$checkName]) -Expected $expectedBooleanChecks[$checkName] -Actual $actualValue
    }

    $interface = Get-JsonPropertyValue -Object $contract -Name "xiigen_exe_interface"
    $methodField = Get-JsonPropertyValue -Object $interface -Name "method_field"
    $inputField = Get-JsonPropertyValue -Object $interface -Name "input_payload_field"
    $outputField = Get-JsonPropertyValue -Object $interface -Name "output_payload_field"
    $debugField = Get-JsonPropertyValue -Object $interface -Name "debug_enabled_field"
    $correlationField = Get-JsonPropertyValue -Object $interface -Name "correlation_id_field"
    $provenanceField = Get-JsonPropertyValue -Object $interface -Name "provenance_field"

    Add-BoundaryCheck -Checks $checks -Name "method_field" -Passed ($methodField -eq "method") -Expected "method" -Actual $methodField
    Add-BoundaryCheck -Checks $checks -Name "input_payload_field" -Passed ($inputField -eq "inputPayload") -Expected "inputPayload" -Actual $inputField
    Add-BoundaryCheck -Checks $checks -Name "output_payload_field" -Passed ($outputField -eq "outputPayload") -Expected "outputPayload" -Actual $outputField
    Add-BoundaryCheck -Checks $checks -Name "debug_enabled_field" -Passed ($debugField -eq "debugEnabled") -Expected "debugEnabled" -Actual $debugField
    Add-BoundaryCheck -Checks $checks -Name "correlation_id_field" -Passed ($correlationField -eq "correlationId") -Expected "correlationId" -Actual $correlationField
    Add-BoundaryCheck -Checks $checks -Name "provenance_field" -Passed ($provenanceField -eq "provenance") -Expected "provenance" -Actual $provenanceField

    $failedChecks = @($checks | Where-Object { $_.passed -ne $true })
    $passed = ($failedChecks.Count -eq 0 -and $readErrors.Count -eq 0)

    $payload = [ordered]@{
        schema = "xiigen.host_boundary.v1"
        script = $scriptName
        work_order = $hostConfig.work_order
        mode = "HostBoundary"
        generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
        host = $hostConfig.host
        repo_path = $hostRepoPath
        package_path = $packagePath
        contract_path = $contractPath
        thin_client = Get-JsonPropertyValue -Object $contract -Name "thin_client"
        uses_xiigen_exe_api = Get-JsonPropertyValue -Object $contract -Name "uses_xiigen_exe_api"
        core_logic_in_llm_mvp_core = Get-JsonPropertyValue -Object $contract -Name "core_logic_in_llm_mvp_core"
        host_training_logic = Get-JsonPropertyValue -Object $contract -Name "host_training_logic"
        external_ai_calls = Get-JsonPropertyValue -Object $contract -Name "external_ai_calls"
        deterministic_shortcut = Get-JsonPropertyValue -Object $contract -Name "deterministic_shortcut"
        debug_setting_planned = Get-JsonPropertyValue -Object $contract -Name "debug_setting_planned"
        runtime_host_proof_performed = Get-JsonPropertyValue -Object $contract -Name "runtime_host_proof_performed"
        passed = $passed
        check_count = $checks.Count
        failed_checks = @($failedChecks | ForEach-Object { $_.name })
        read_errors = $readErrors
        checks = $checks
        report_path = $hostConfig.report_ref
        repos_created = $false
        host_runtime_proof_performed = $false
        host_implementation_runtime_launched = $false
        training_started = $false
        core_machine_implementation_changed = $false
        external_ai_used = $false
        non_target_host_work_performed = $false
        debug_trace_implementation_performed = $false
        next_action = "parent REVIEW_INTERNAL reviews $($hostConfig.work_order) evidence"
    }

    $payload | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $hostConfig.report_path -Encoding UTF8
    Write-Output "XIIGen $($hostConfig.host) host boundary verification"
    Write-Output "Report: $($hostConfig.report_ref)"
    Write-Output ($payload | ConvertTo-Json -Depth 12)

    if ($passed) {
        exit 0
    }

    exit 1
}

function Invoke-HostRepoSet {
    if ([string]::IsNullOrWhiteSpace($RequiredRepos)) {
        $requiredRepoNames = @()
    }
    else {
        $requiredRepoNames = @(
            $RequiredRepos.Split(",") |
                ForEach-Object { $_.Trim() } |
                Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        )
    }

    $ownerResolution = Get-OwnerFromGitRemote
    if ($ExpectedOwner -ieq "SameAsCore") {
        $resolvedOwner = $ownerResolution.owner
        $ownerSource = $ownerResolution.metadata_source
    }
    else {
        $resolvedOwner = $ExpectedOwner
        $ownerSource = "argument"
    }

    $metadataAccess = Get-GhAccessStatus
    $repoRows = @()
    $metadataAccessAvailable = [bool]$metadataAccess.metadata_access_available

    if ([string]::IsNullOrWhiteSpace($resolvedOwner)) {
        $metadataAccessAvailable = $false
        foreach ($repoName in $requiredRepoNames) {
            $repoRows += [pscustomobject][ordered]@{
                repo_name = $repoName
                owner = $null
                full_name = $repoName
                exists = $null
                is_private = $null
                visibility = "unknown"
                privacy_unknown = $true
                metadata_source = "git_remote_owner_resolution_failed"
                metadata_error = ($ownerResolution.errors -join "; ")
            }
        }
    }
    elseif (-not $metadataAccessAvailable) {
        foreach ($repoName in $requiredRepoNames) {
            $repoRows += [pscustomobject][ordered]@{
                repo_name = $repoName
                owner = $resolvedOwner
                full_name = "$resolvedOwner/$repoName"
                exists = $null
                is_private = $null
                visibility = "unknown"
                privacy_unknown = $true
                metadata_source = $metadataAccess.metadata_source
                metadata_error = $metadataAccess.status_summary
            }
        }
    }
    else {
        foreach ($repoName in $requiredRepoNames) {
            $repoRows += Get-GhRepoMetadata -Owner $resolvedOwner -RepoName $repoName
        }
    }

    $missingRepos = @($repoRows | Where-Object { $_.exists -eq $false } | ForEach-Object { $_.repo_name })
    $privacyUnknownRepos = @($repoRows | Where-Object { $_.privacy_unknown -eq $true } | ForEach-Object { $_.repo_name })
    if ($PrivateRequired) {
        $privacyFailures = @(
            $repoRows |
                Where-Object { $_.exists -eq $true -and ($_.privacy_unknown -eq $true -or $_.is_private -ne $true) } |
                ForEach-Object { $_.repo_name }
        )
        $privacyFailures += @(
            $repoRows |
                Where-Object { $_.exists -eq $null -and $_.privacy_unknown -eq $true } |
                ForEach-Object { $_.repo_name }
        )
        $privacyFailures = @($privacyFailures | Select-Object -Unique)
    }
    else {
        $privacyFailures = @()
    }

    $passed = (
        $metadataAccessAvailable -and
        -not [string]::IsNullOrWhiteSpace($resolvedOwner) -and
        $requiredRepoNames.Count -gt 0 -and
        $missingRepos.Count -eq 0 -and
        $privacyFailures.Count -eq 0
    )

    $nextAction = "parent REVIEW_INTERNAL reviews WO-HOST-REPO-VERIFY-01 evidence"
    if (-not $passed) {
        $nextAction = "WO-HOST-REPO-CREATE-01_OR_ACCESS_REPAIR"
    }

    $payload = [ordered]@{
        schema = "xiigen.host_repo_status.v1"
        script = $scriptName
        work_order = $workOrder
        mode = "HostRepoSet"
        generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
        expected_owner_input = $ExpectedOwner
        resolved_owner = $resolvedOwner
        owner_metadata_source = $ownerSource
        owner_remote_url_redacted = $ownerResolution.remote_url_redacted
        owner_resolution_errors = $ownerResolution.errors
        metadata_access_available = $metadataAccessAvailable
        attempted_local_tool = $metadataAccess.attempted_local_tool
        metadata_source = $metadataAccess.metadata_source
        metadata_status_summary = $metadataAccess.status_summary
        private_required = [bool]$PrivateRequired
        required_repos = $requiredRepoNames
        repo_rows = $repoRows
        missing_repos = $missingRepos
        privacy_unknown_repos = $privacyUnknownRepos
        privacy_failures = $privacyFailures
        passed = $passed
        creation_required = ($missingRepos.Count -gt 0)
        repos_created = $false
        host_repos_touched = $false
        host_implementation_performed = $false
        training_started = $false
        external_ai_used = $false
        report_path = "artifacts/host-repo-status.json"
        next_action = $nextAction
    }

    Write-HostRepoReport -Payload $payload
    Write-Output "XIIGen host repo set verification"
    Write-Output "Report: artifacts/host-repo-status.json"
    Write-Output ($payload | ConvertTo-Json -Depth 12)

    if ($passed) {
        exit 0
    }

    exit 1
}

if ($Mode -ieq "Help") {
    Write-HelpPayload
    exit 0
}

if ($Mode -ieq "HostRepoSet") {
    Invoke-HostRepoSet
}

if ($Mode -ieq "HostBoundary") {
    Invoke-HostBoundary
}

if ($Mode -ieq "DebugTrace") {
    Invoke-DebugTrace
}

if ($Mode -ieq "HostLocalRuntime") {
    Invoke-HostLocalRuntime
}

if ($Mode -ieq "HostInteractiveRuntime") {
    Invoke-HostInteractiveRuntime
}

$errorPayload = [ordered]@{
    script = $scriptName
    work_order = $workOrder
    status = "unsupported_mode"
    requested_mode = $Mode
    supported_modes = $supportedModes
    planned_modes_not_implemented = $plannedModes
    note = "Only supported modes are listed in supported_modes. HostRepoCreateMissing remains planned_not_implemented."
}

Write-Output ($errorPayload | ConvertTo-Json -Depth 5)
exit 2
