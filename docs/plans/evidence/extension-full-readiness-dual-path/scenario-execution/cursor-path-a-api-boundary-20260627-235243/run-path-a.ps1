param(
    [string]$CoreRoot = "C:\Projects\llm_mvp_core"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2.0

function Write-Utf8NoBomText {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text
    )

    $directory = Split-Path -Parent $Path
    if ($directory -and -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $normalized = $Text.Replace("`r`n", "`n").Replace("`r", "`n")
    if (-not $normalized.EndsWith("`n")) {
        $normalized += "`n"
    }

    [System.IO.File]::WriteAllText($Path, $normalized, [System.Text.UTF8Encoding]::new($false))
}

function Write-JsonFile {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)]$Value
    )

    Write-Utf8NoBomText -Path $Path -Text ($Value | ConvertTo-Json -Depth 100)
}

function Read-JsonFile {
    param([Parameter(Mandatory = $true)][string]$Path)

    return Get-Content -Raw -Encoding UTF8 -LiteralPath $Path | ConvertFrom-Json
}

function Get-Sha256Text {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text)

    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
        return ([BitConverter]::ToString($sha.ComputeHash($bytes)) -replace "-", "").ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }
}

function Invoke-CapturedProcess {
    param(
        [Parameter(Mandatory = $true)][string]$FileName,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$WorkingDirectory,
        [Parameter(Mandatory = $true)][string]$StdoutPath,
        [Parameter(Mandatory = $true)][string]$StderrPath
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    $startInfo.Arguments = (@($Arguments | ForEach-Object {
        $argument = [string]$_
        if ($argument -match '[\s"]') {
            '"' + ($argument -replace '"', '\"') + '"'
        } else {
            $argument
        }
    }) -join " ")
    $startInfo.WorkingDirectory = $WorkingDirectory
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true

    $startedUtc = (Get-Date).ToUniversalTime().ToString("o")
    $process = [System.Diagnostics.Process]::Start($startInfo)
    $stdout = $process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()
    $completedUtc = (Get-Date).ToUniversalTime().ToString("o")

    Write-Utf8NoBomText -Path $StdoutPath -Text $stdout
    Write-Utf8NoBomText -Path $StderrPath -Text $stderr

    return [ordered]@{
        file_name = $FileName
        arguments = $Arguments
        working_directory = $WorkingDirectory
        started_utc = $startedUtc
        completed_utc = $completedUtc
        exit_code = $process.ExitCode
        stdout_path = $StdoutPath
        stderr_path = $StderrPath
    }
}

$coreRootPath = [System.IO.Path]::GetFullPath($CoreRoot)
$evidenceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$scenarioId = "cursor-full-extension-runtime-after-unlock-20260627-090159"
$timestamp = "20260627-235243"
$prepDir = Join-Path $coreRootPath "docs\plans\evidence\extension-full-readiness-dual-path\scenario-prep\$scenarioId"
$blockerDir = Join-Path $coreRootPath "docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\cursor-current-state-preflight-20260627-234230"
$scenarioPath = Join-Path $prepDir "scenario.json"
$blockerResultPath = Join-Path $blockerDir "result.json"
$coreApiScript = Join-Path $coreRootPath "tools\xiigen-extension-core-api.ps1"

$requestPath = Join-Path $evidenceDir "request.json"
$responsePath = Join-Path $evidenceDir "response.json"
$tracePath = Join-Path $evidenceDir "trace.json"
$adapterTracePath = Join-Path $evidenceDir "adapter-trace.json"
$resultPath = Join-Path $evidenceDir "result.json"
$verificationPath = Join-Path $evidenceDir "verification.json"
$commandPath = Join-Path $evidenceDir "command.json"
$stdoutPath = Join-Path $evidenceDir "core-api.stdout.log"
$stderrPath = Join-Path $evidenceDir "core-api.stderr.log"
$readmePath = Join-Path $evidenceDir "README.md"

$scenario = Read-JsonFile -Path $scenarioPath
$blocker = Read-JsonFile -Path $blockerResultPath
if ($scenario.scenario_id -ne $scenarioId) {
    throw "Scenario id mismatch in prep scenario.json."
}
if ($blocker.scenario.id -ne $scenarioId) {
    throw "Scenario id mismatch in blocker result.json."
}

$prompt = [string]$scenario.exact_prompt
$promptSha256 = Get-Sha256Text -Text $prompt
$requestedSkills = @(
    "prompt-to-task",
    "orchestration-reasoning",
    "skill-selection",
    "api-dashboard-qa"
)
$availableSkills = @(
    "prompt-to-task",
    "orchestration-reasoning",
    "skill-selection",
    "api-dashboard-qa",
    "xiigen-smart-skills"
)
$correlationId = "$scenarioId|path-a-api-boundary|$timestamp"

$request = [ordered]@{
    HostName = "Cursor"
    HostType = "cursor-api-boundary"
    Method = "prompt-to-task"
    UserPromptOrTaskText = $prompt
    HostFacts = [ordered]@{
        host = "cursor"
        surface = "direct-api-boundary-no-ui"
        scenario_id = $scenarioId
        evidence_folder = "cursor-path-a-api-boundary-$timestamp"
        exact_prompt_source = "scenario-prep/$scenarioId/scenario.json"
        path_b_current_status = [string]$blocker.coverage_status.path_b.status
        path_b_current_blocker = "Storage Error modal"
        ui_used = "false"
        requested_skills_count = [string]$requestedSkills.Count
        available_skills_count = [string]$availableSkills.Count
        closest_boundary_used = "tools/xiigen-extension-core-api.ps1"
        exact_cursor_adapter_cli_used = "false"
    }
    DebugEnabled = $true
    RequestedSkills = $requestedSkills
    AvailableSkills = $availableSkills
    CheckSetInputs = [ordered]@{
        scenario_id = $scenarioId
        path_a_kind = "direct_api_core_boundary_fallback"
        same_scenario_prompt_sha256 = $promptSha256
        external_ai_used = "false"
        ui_used = "false"
        hostSelectorOrScorerAdded = "false"
        semantic_quality_claimed = "false"
        runtime_extension_claimed = "false"
        product_readiness_claimed = "false"
        requested_available_skills_supported = "true"
    }
    Provenance = [ordered]@{
        source = "bounded-sub-agent"
        active_task = "create Cursor Path A same-scenario adapter/API/core evidence"
        prep_folder = "docs/plans/evidence/extension-full-readiness-dual-path/scenario-prep/$scenarioId"
        current_blocker_folder = "docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/cursor-current-state-preflight-20260627-234230"
        route_used = "direct XiigenExtensionRequest API/core boundary"
        exact_cursor_adapter_cli = "not used; unavailable inside allowed write scope"
        core_endpoint = "tools/xiigen-extension-core-api.ps1"
    }
    CorrelationId = $correlationId
    TargetLocator = $null
}

Write-JsonFile -Path $requestPath -Value $request

$arguments = @(
    "-NoProfile",
    "-ExecutionPolicy", "Bypass",
    "-File", $coreApiScript,
    "-RequestPath", $requestPath,
    "-ResponsePath", $responsePath,
    "-TracePath", $tracePath,
    "-CoreRoot", $coreRootPath
)
$commandResult = Invoke-CapturedProcess `
    -FileName "powershell.exe" `
    -Arguments $arguments `
    -WorkingDirectory $coreRootPath `
    -StdoutPath $stdoutPath `
    -StderrPath $stderrPath
Write-JsonFile -Path $commandPath -Value $commandResult

$response = if (Test-Path -LiteralPath $responsePath) { Read-JsonFile -Path $responsePath } else { $null }
$trace = if (Test-Path -LiteralPath $tracePath) { Read-JsonFile -Path $tracePath } else { $null }
if ($null -ne $response) {
    Write-JsonFile -Path $responsePath -Value $response
}
if ($null -ne $trace) {
    Write-JsonFile -Path $tracePath -Value $trace
}
$checkSet = if ($null -ne $response -and $null -ne $response.CheckSetResults) { $response.CheckSetResults } else { $null }
$selectedSkills = if ($null -ne $response -and $null -ne $response.SelectedSkills) { @($response.SelectedSkills) } else { @() }
$structuredTaskRefs = if ($null -ne $response -and $null -ne $response.StructuredTaskRefs) { @($response.StructuredTaskRefs) } else { @() }
$scenarioRefs = if ($null -ne $response -and $null -ne $response.ScenarioRefs) { @($response.ScenarioRefs) } else { @() }
$structuredResponseReturned = (
    $selectedSkills.Count -gt 0 -or
    $structuredTaskRefs.Count -gt 0 -or
    $scenarioRefs.Count -gt 0 -or
    ($null -ne $response -and -not [string]::IsNullOrWhiteSpace([string]$response.ChatDisplayText))
)
$coreBoundaryExecuted = (
    $null -ne $checkSet -and
    ($checkSet.core_boundary_executed -eq "true" -or $checkSet.core_boundary_executed -eq $true)
)
$tracePromptMatches = (
    $null -ne $trace -and
    [string]$trace.user_prompt_sha256 -eq $promptSha256
)
$pathAAccepted = (
    $commandResult.exit_code -eq 0 -and
    $null -ne $response -and
    $response.Success -eq $true -and
    $structuredResponseReturned -eq $true -and
    $coreBoundaryExecuted -eq $true -and
    $tracePromptMatches -eq $true
)
$pathAStatus = if ($pathAAccepted) { "accepted" } else { "blocked" }

$adapterTrace = [ordered]@{
    schema = "xiigen.cursor.path_a.adapter_trace.v1"
    scenario_id = $scenarioId
    host = "cursor"
    host_name_in_request = $request.HostName
    host_type_in_request = $request.HostType
    route_used = "direct XiigenExtensionRequest API/core boundary fallback"
    exact_cursor_adapter_cli_used = $false
    exact_cursor_adapter_cli_unavailable_reason = "No exact same-scenario Cursor adapter CLI was found that both used this prompt and stayed inside the allowed write folder."
    core_endpoint = "tools/xiigen-extension-core-api.ps1"
    ui_used = $false
    hostSelectorOrScorerAdded = $false
    requested_skills_supported = $true
    available_skills_supported = $true
    requested_skills_sent = $requestedSkills
    available_skills_sent = $availableSkills
    correlation_id = $correlationId
    prompt_sha256 = $promptSha256
}
Write-JsonFile -Path $adapterTracePath -Value $adapterTrace

$result = [ordered]@{
    schema = "xiigen.cursor.path_a.api_boundary_result.v1"
    packet = "cursor-path-a-api-boundary-$timestamp"
    created_at = (Get-Date).ToString("yyyy-MM-ddTHH:mm:sszzz")
    subagent_id = "bounded-sub-agent-current-session"
    scope = "Cursor scenario coverage Path A only; no product readiness claim"
    cursor_scenarios_count = 1
    scenario = [ordered]@{
        id = $scenarioId
        prep_folder = "docs/plans/evidence/extension-full-readiness-dual-path/scenario-prep/$scenarioId"
        prompt_sha256 = $promptSha256
        exact_prompt_source = "scenario.json exact_prompt"
    }
    path_a = [ordered]@{
        status = $pathAStatus
        accepted = $pathAAccepted
        route_used = "direct XiigenExtensionRequest API/core boundary fallback"
        exact_cursor_adapter_cli_used = $false
        host_name = $request.HostName
        host_type = $request.HostType
        ui_used = $false
        core_exit_code = $commandResult.exit_code
        response_success = if ($null -ne $response) { [bool]$response.Success } else { $false }
        response_status = if ($null -ne $response) { [string]$response.Status } else { "missing_response" }
        core_boundary_executed = $coreBoundaryExecuted
        structured_response_returned = $structuredResponseReturned
        trace_prompt_matches_exact_prompt = $tracePromptMatches
        selected_skills = $selectedSkills
        structured_task_refs = $structuredTaskRefs
        scenario_refs = $scenarioRefs
    }
    path_b_current = [ordered]@{
        status = "blocked_current_storage_modal"
        blocker_folder = "docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/cursor-current-state-preflight-20260627-234230"
        blocker = "Storage Error modal"
        prompt_entered = $false
        prompt_submitted = $false
        response_visible = $false
    }
    files = [ordered]@{
        request = "request.json"
        response = "response.json"
        trace = "trace.json"
        adapter_trace = "adapter-trace.json"
        command = "command.json"
        stdout = "core-api.stdout.log"
        stderr = "core-api.stderr.log"
        verification = "verification.json"
        readme = "README.md"
        runner = "run-path-a.ps1"
    }
    not_proven = @(
        "real Cursor UI Path B",
        "Cursor login/session readiness",
        "visible Cursor response",
        "token savings",
        "semantic quality improvement",
        "training",
        "DPO",
        "checkpoint mutation or promotion",
        "product readiness"
    )
    parent_commit_push_recommendation = "commit_push_new_evidence_folder_after_review"
}
Write-JsonFile -Path $resultPath -Value $result

$jsonFiles = @(
    $requestPath,
    $responsePath,
    $tracePath,
    $adapterTracePath,
    $commandPath,
    $resultPath
)
$jsonParseFailures = @()
foreach ($jsonFile in $jsonFiles) {
    try {
        [void](Read-JsonFile -Path $jsonFile)
    }
    catch {
        $jsonParseFailures += [ordered]@{
            file = [System.IO.Path]::GetFileName($jsonFile)
            error = $_.Exception.Message
        }
    }
}

$pathAAcceptedText = $pathAAccepted.ToString().ToLowerInvariant()
$coreExitCodeText = [string]$commandResult.exit_code
$responseStatusText = if ($null -ne $response) { [string]$response.Status } else { "missing_response" }
$structuredResponseText = $structuredResponseReturned.ToString().ToLowerInvariant()
$tracePromptMatchesText = $tracePromptMatches.ToString().ToLowerInvariant()

$readme = @"
# Cursor Path A API/core boundary

Дата пакета: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz").

Сценарий: $scenarioId.

## Счетчик

Всего Cursor-сценариев в этом пакете: 1.
Path A: $pathAStatus.
Path B сейчас: blocked_current_storage_modal из уже сохраненного blocker evidence.
Осталось: вручную или отдельным bounded worker решить Cursor `Storage Error`, затем повторить Path B same-scenario preflight и UI prompt-run.

## Что было запущено

Запущен не UI и не полный Cursor Extension Host. Запущен прямой fallback boundary: tools/xiigen-extension-core-api.ps1 с XiigenExtensionRequest.

В request указано:

- HostName = Cursor
- HostType = cursor-api-boundary
- HostFacts.host = cursor
- `RequestedSkills` переданы
- `AvailableSkills` переданы
- hostSelectorOrScorerAdded = false

Exact prompt взят из prep `scenario.json`; prompt не переписывался вручную.

## Результат

Path A accepted: $pathAAcceptedText.
Core exit code: $coreExitCodeText.
Core response status: $responseStatusText.
Structured response returned: $structuredResponseText.
Trace prompt hash matches prep prompt: $tracePromptMatchesText.

Этот пакет доказывает только no-UI API/core boundary для exact Cursor scenario prompt. Он не доказывает живой Cursor UI, token savings, semantic quality improvement, training, DPO, checkpoint promotion или product readiness.

## Path B blocker

Path B остается blocked_current_storage_modal. В blocker-пакете cursor-current-state-preflight-20260627-234230 уже зафиксировано, что Cursor показывает Storage Error, prompt не вводился, submit не выполнялся, visible response и UI core trace не получены.

## Parent commit/push recommendation

Рекомендация: parent может commit/push только эту новую evidence-папку после review. Это полезное same-scenario Path A evidence, но не product readiness claim.
"@
Write-Utf8NoBomText -Path $readmePath -Text $readme

$readmeText = Get-Content -Raw -Encoding UTF8 -LiteralPath $readmePath
$placeholderPatterns = @("TODO", "TBD", "PLACEHOLDER", "<placeholder>", "[object Object]", "System.Object", "@{")
$placeholderHits = @()
foreach ($pattern in $placeholderPatterns) {
    if ($readmeText.Contains($pattern)) {
        $placeholderHits += $pattern
    }
}

$verification = [ordered]@{
    schema = "xiigen.cursor.path_a.api_boundary_verification.v1"
    created_at = (Get-Date).ToString("yyyy-MM-ddTHH:mm:sszzz")
    scenario_id = $scenarioId
    json_parse = [ordered]@{
        files_checked = @($jsonFiles | ForEach-Object { [System.IO.Path]::GetFileName($_) })
        failure_count = $jsonParseFailures.Count
        failures = $jsonParseFailures
    }
    readme_scan = [ordered]@{
        checked = $true
        no_placeholder_or_raw_object_dump = ($placeholderHits.Count -eq 0)
        hits = $placeholderHits
    }
    text_format_scan = [ordered]@{
        utf8_no_bom_writer_used = $true
        lf_writer_used = $true
        files_scanned = @("README.md", "core-api.stdout.log", "core-api.stderr.log")
    }
    evidence_acceptance = [ordered]@{
        path_a_accepted = $pathAAccepted
        core_exit_code = $commandResult.exit_code
        core_boundary_executed = $coreBoundaryExecuted
        structured_response_returned = $structuredResponseReturned
        trace_prompt_matches_exact_prompt = $tracePromptMatches
    }
    git_diff_check = [ordered]@{
        command = "git diff --check -- docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/cursor-path-a-api-boundary-$timestamp"
        exit_code_recorded_in_this_file = $null
        note = "Run after evidence generation; final agent report records the observed exit code."
    }
}
Write-JsonFile -Path $verificationPath -Value $verification

Write-Output "Path A status: $pathAStatus"
Write-Output "Evidence folder: $evidenceDir"
Write-Output "Core exit code: $($commandResult.exit_code)"
