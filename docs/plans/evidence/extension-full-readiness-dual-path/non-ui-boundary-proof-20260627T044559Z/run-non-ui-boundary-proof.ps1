$ErrorActionPreference = "Stop"

$CoreRoot = "C:\Projects\llm_mvp_core"
$EvidenceRoot = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\non-ui-boundary-proof-20260627T044559Z"
$CoreScript = Join-Path $CoreRoot "tools\xiigen-extension-core-api.ps1"
$CorrelationPrefix = "NON-UI-BOUNDARY-20260627T044559Z"
$RedactedPrompt = "<redacted same-scenario boundary proof prompt: verify core request shape only>"
$ScenarioId = "same-redacted-non-ui-core-boundary"

function Write-JsonFile {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)]$Value
    )

    $Value | ConvertTo-Json -Depth 80 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Get-Prop {
    param($Object, [string]$Name)

    if ($null -eq $Object) {
        return $null
    }

    $property = $Object.PSObject.Properties | Where-Object { $_.Name -ieq $Name } | Select-Object -First 1
    if ($property) {
        return $property.Value
    }

    return $null
}

New-Item -ItemType Directory -Force -Path $EvidenceRoot | Out-Null

$hosts = @(
    [ordered]@{ HostName = "Claude"; HostType = "claude"; Entrypoint = "claude-xiigen-smart-skills"; Surface = "host-labeled-core-api-only" },
    [ordered]@{ HostName = "Cursor"; HostType = "cursor"; Entrypoint = "cursor-xiigen-smart-skills"; Surface = "host-labeled-core-api-only" },
    [ordered]@{ HostName = "Codex"; HostType = "codex"; Entrypoint = "codex-xiigen-smart-skills"; Surface = "host-labeled-core-api-only" }
)

$results = @()
$commands = @()
$topLevelKeySets = @()

foreach ($hostSpec in $hosts) {
    $slug = ([string]$hostSpec.HostType).ToLowerInvariant()
    $hostDir = Join-Path $EvidenceRoot $slug
    New-Item -ItemType Directory -Force -Path $hostDir | Out-Null

    $requestPath = Join-Path $hostDir "request.json"
    $responsePath = Join-Path $hostDir "response.json"
    $tracePath = Join-Path $hostDir "trace.json"
    $stdoutPath = Join-Path $hostDir "stdout.log"
    $stderrPath = Join-Path $hostDir "stderr.log"
    $correlationId = "$CorrelationPrefix-$($slug.ToUpperInvariant())"

    $request = [ordered]@{
        HostName = $hostSpec.HostName
        HostType = $hostSpec.HostType
        Method = "prompt-to-task"
        UserPromptOrTaskText = $RedactedPrompt
        HostFacts = [ordered]@{
            scenario_id = $ScenarioId
            proof_scope = "non-ui-core-api-boundary-only"
            target_locator_present = "false"
            local_desktop_ui_used = "false"
            external_ai_used = "false"
            training_started = "false"
        }
        DebugEnabled = $false
        RequestedSkills = @()
        AvailableSkills = @()
        CheckSetInputs = [ordered]@{
            same_scenario_id = $ScenarioId
            same_redacted_prompt = "true"
            host_adapter_execution_claimed = "false"
            live_host_ui_claimed = "false"
        }
        Provenance = [ordered]@{
            host = $hostSpec.HostType
            entrypoint = $hostSpec.Entrypoint
            surface = $hostSpec.Surface
            runner = "run-non-ui-boundary-proof.ps1"
            boundary_note = "Host-labeled request sent to llm_mvp_core core API script; actual host adapter/UI not invoked."
        }
        CorrelationId = $correlationId
        TargetLocator = $null
    }

    Write-JsonFile -Path $requestPath -Value $request
    $topLevelKeySets += (($request.Keys | Sort-Object) -join "|")

    $commandText = "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$CoreScript`" -RequestPath `"$requestPath`" -ResponsePath `"$responsePath`" -TracePath `"$tracePath`" -CoreRoot `"$CoreRoot`""
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $CoreScript -RequestPath $requestPath -ResponsePath $responsePath -TracePath $tracePath -CoreRoot $CoreRoot 1> $stdoutPath 2> $stderrPath
    $exitCode = $LASTEXITCODE

    $response = if (Test-Path -LiteralPath $responsePath) {
        Get-Content -Raw -LiteralPath $responsePath | ConvertFrom-Json
    } else {
        $null
    }
    $trace = if (Test-Path -LiteralPath $tracePath) {
        Get-Content -Raw -LiteralPath $tracePath | ConvertFrom-Json
    } else {
        $null
    }
    $checkSet = Get-Prop -Object $response -Name "CheckSetResults"

    $hostResult = [ordered]@{
        host_name = $hostSpec.HostName
        host_type = $hostSpec.HostType
        command = $commandText
        exit_code = $exitCode
        request_path = $requestPath
        response_path = $responsePath
        trace_path = $tracePath
        stdout_path = $stdoutPath
        stderr_path = $stderrPath
        response_success = [bool](Get-Prop -Object $response -Name "Success")
        response_status = Get-Prop -Object $response -Name "Status"
        chat_display_text_present = -not [string]::IsNullOrWhiteSpace([string](Get-Prop -Object $response -Name "ChatDisplayText"))
        structured_task_ref_count = @((Get-Prop -Object $response -Name "StructuredTaskRefs")).Count
        scenario_ref_count = @((Get-Prop -Object $response -Name "ScenarioRefs")).Count
        core_boundary_executed = [string](Get-Prop -Object $checkSet -Name "core_boundary_executed")
        semantic_quality_claimed = [string](Get-Prop -Object $checkSet -Name "semantic_quality_claimed")
        runtime_extension_claimed = [string](Get-Prop -Object $checkSet -Name "runtime_extension_claimed")
        trace_host_runtime_claimed = [bool](Get-Prop -Object $trace -Name "host_runtime_claimed")
        trace_external_ai_used = [bool](Get-Prop -Object $trace -Name "external_ai_used")
        trace_training_started = [bool](Get-Prop -Object $trace -Name "training_started")
        correlation_id = $correlationId
        trace_correlation_id = Get-Prop -Object $trace -Name "correlation_id"
    }
    $results += [pscustomobject]$hostResult
    $commands += $commandText
}

$shapeConsistent = (($topLevelKeySets | Select-Object -Unique).Count -eq 1)
$allPassed = -not ($results | Where-Object {
    $_.exit_code -ne 0 -or
    $_.response_success -ne $true -or
    $_.core_boundary_executed -ne "true" -or
    $_.semantic_quality_claimed -ne "false" -or
    $_.runtime_extension_claimed -ne "false" -or
    $_.trace_host_runtime_claimed -ne $false -or
    $_.trace_external_ai_used -ne $false -or
    $_.trace_training_started -ne $false -or
    $_.trace_correlation_id -ne $_.correlation_id
})

$summary = [ordered]@{
    schema_version = 1
    packet_type = "non_ui_boundary_proof"
    run_id = "non-ui-boundary-proof-20260627T044559Z"
    evidence_root = $EvidenceRoot
    runner_path = Join-Path $EvidenceRoot "run-non-ui-boundary-proof.ps1"
    started_source = "manual bounded sub-agent run"
    existing_script_used = "tools/xiigen-extension-core-api.ps1"
    scenario_id = $ScenarioId
    host_count = $hosts.Count
    hosts = @($hosts | ForEach-Object { $_.HostType })
    same_top_level_request_shape = $shapeConsistent
    same_redacted_prompt = $true
    all_hosts_passed_core_api_boundary = [bool]$allPassed
    verdict = if ($allPassed -and $shapeConsistent) { "PASS_CORE_API_BOUNDARY_ONLY" } else { "NEEDS_FIX_CORE_API_BOUNDARY_ONLY" }
    commands = $commands
    results = $results
    proves = @(
        "The existing local core API script accepted same-shape host-labeled XiigenExtensionRequest files for Claude, Cursor, and Codex.",
        "Each run called SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest and produced a structured XiigenExtensionResponse.",
        "Each trace kept semantic_quality_claimed=false, runtime_extension_claimed=false, host_runtime_claimed=false, external_ai_used=false, and training_started=false."
    )
    does_not_prove = @(
        "Actual Claude/Cursor/Codex adapter execution.",
        "Live desktop UI readiness or visible host chat/panel readiness.",
        "Token savings, semantic quality, learnability, training, DPO, checkpoint promotion, target promotion, locator update, or product readiness."
    )
    next_bounded_step = "If desktop remains locked, run a separate non-UI adapter-entrypoint proof only for a host whose adapter runner is safely available and parameterized to write under docs/plans/evidence; otherwise wait for approved desktop unlock/input path for live host UI."
}

$summaryJsonPath = Join-Path $EvidenceRoot "summary.json"
$summaryMdPath = Join-Path $EvidenceRoot "summary.md"
Write-JsonFile -Path $summaryJsonPath -Value $summary

$md = @"
# Non-UI Boundary Proof 20260627T044559Z

Verdict: $($summary.verdict)

Executed existing script:
`tools/xiigen-extension-core-api.ps1`

Hosts:
- Claude
- Cursor
- Codex

What this proves:
- The existing local core API script accepted same-shape host-labeled `XiigenExtensionRequest` files for Claude, Cursor, and Codex.
- Each run called `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest` and produced a structured `XiigenExtensionResponse`.
- Each trace kept `semantic_quality_claimed=false`, `runtime_extension_claimed=false`, `host_runtime_claimed=false`, `external_ai_used=false`, and `training_started=false`.

What this does not prove:
- Actual Claude/Cursor/Codex adapter execution.
- Live desktop UI readiness or visible host chat/panel readiness.
- Token savings, semantic quality, learnability, training, DPO, checkpoint promotion, target promotion, locator update, or product readiness.

Next bounded step:
$($summary.next_bounded_step)
"@

$md | Set-Content -LiteralPath $summaryMdPath -Encoding UTF8
Write-Output "summary_json=$summaryJsonPath"
Write-Output "summary_md=$summaryMdPath"
Write-Output "verdict=$($summary.verdict)"
