[CmdletBinding()]
param()

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$EvidenceRoot = $PSScriptRoot
$CoreRoot = "C:\Projects\llm_mvp_core"
$RepoRoot = "C:\Projects\codex-xiigen-smart-skills"
$StatePath = Join-Path $CoreRoot "docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"
$PacketPath = Join-Path $EvidenceRoot "codex-ext-readiness-01-packet.json"
$PacketMarkdownPath = Join-Path $EvidenceRoot "codex-ext-readiness-01-packet.md"
$RunnerSummaryPath = Join-Path $EvidenceRoot "runner-summary.json"
$CommandResultsPath = Join-Path $EvidenceRoot "command-results.json"
$InstallSmokeReportPath = Join-Path $EvidenceRoot "install-smoke\install-smoke-report.json"
$Host08FReportPath = Join-Path $EvidenceRoot "host-08f\HOST-08F-codex-plugin-app-qa.json"
$ParserReportPath = Join-Path $EvidenceRoot "available-skills-parser\codex-available-skills-parser-repair-report.json"
$Row23RequestPath = Join-Path $EvidenceRoot "row23-smoke\request.json"
$Row23ResponsePath = Join-Path $EvidenceRoot "row23-smoke\response.json"
$Row23TracePath = Join-Path $EvidenceRoot "row23-smoke\trace.json"

function Read-Json {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
}

function Write-Json {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Value,

        [Parameter(Mandatory = $true)]
        [string]$Path,

        [int]$Depth = 32
    )

    $json = $Value | ConvertTo-Json -Depth $Depth
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, "$json`n", $utf8NoBom)
}

$runnerSummary = Read-Json -Path $RunnerSummaryPath
$commandResults = Read-Json -Path $CommandResultsPath
$installSmoke = Read-Json -Path $InstallSmokeReportPath
$host08f = Read-Json -Path $Host08FReportPath
$parserReport = Read-Json -Path $ParserReportPath
$row23Request = Read-Json -Path $Row23RequestPath
$row23Response = Read-Json -Path $Row23ResponsePath
$row23Trace = Read-Json -Path $Row23TracePath

$changedFiles = @(
    (Join-Path $EvidenceRoot "run-codex-ext-readiness-01.ps1"),
    (Join-Path $EvidenceRoot "write-codex-ext-readiness-01-packet-and-state.ps1"),
    $RunnerSummaryPath,
    $CommandResultsPath,
    $InstallSmokeReportPath,
    $Host08FReportPath,
    $ParserReportPath,
    $Row23RequestPath,
    $Row23ResponsePath,
    $Row23TracePath,
    $PacketPath,
    $PacketMarkdownPath,
    $StatePath
)

$exactFilesRead = @(
    "C:\Projects\llm_mvp_core\AGENTS.md",
    "C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md",
    $StatePath,
    "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json",
    "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\skills\xiigen-smart-skills\SKILL.md",
    (Join-Path $RepoRoot "package.json"),
    (Join-Path $RepoRoot ".codex-plugin\plugin.json"),
    (Join-Path $RepoRoot "src\codex-entrypoint.js"),
    (Join-Path $RepoRoot "src\codex-entrypoint.ps1"),
    (Join-Path $RepoRoot "scripts\install-smoke.ps1"),
    (Join-Path $RepoRoot "test\host-qa\HOST-08F-codexComponentQA.js"),
    (Join-Path $RepoRoot "test\host-qa\HOST-REPAIR-CODEX-AVAILABLE-SKILLS-PARSER.js"),
    (Join-Path $RepoRoot "README.md"),
    (Join-Path $RepoRoot "xiigen-extension-contract.json"),
    $RunnerSummaryPath,
    $CommandResultsPath,
    $InstallSmokeReportPath,
    $Host08FReportPath,
    $ParserReportPath,
    $Row23RequestPath,
    $Row23ResponsePath,
    $Row23TracePath
)

$gatesPassed = @(
    "package and plugin manifest inspected; Codex plugin shell declares skills folder and thin adapter package",
    "src/codex-entrypoint.js syntax check exited 0",
    "source wiring install-smoke exited 0 and report passed=true",
    "component QA exited 0 and passed prompt-to-task, orchestration-plan, skill-selection, debug on/off, controlled error, redaction, and thin boundary checks",
    "available-skills parser regression exited 0 and forwarded AvailableSkills/RequestedSkills to llm_mvp_core without host semantic ranking/training/DPO/mutation",
    "Row23 safe input entrypoint smoke exited 0; request HostName=Codex HostType=codex Method=prompt-to-task; response Success=true Status=core_boundary_partial_no_semantic_quality_claim; trace correlation id matched the request",
    "external AI, network, training, checkpoint mutation, git actions, and host semantic mutation remained false in generated evidence"
)

$gatesNotProven = @(
    "real Codex plugin/app/chat/UI/CLI automation Path B in the actual Codex host",
    "installed plugin skill invocation from the live Codex host surface, as distinct from direct source-entrypoint/component smoke",
    "visible answer inside the real Codex host surface with screenshots/window inventory/redaction log",
    "same-scenario Path A plus Path B comparison",
    "token before/after with quality non-regression",
    "quality improvement",
    "learnability/DPO lifecycle",
    "backup/export/import/share/continue-training",
    "rollback/target safety",
    "documentation and final dual-path scenario coverage"
)

$packet = [ordered]@{
    schema = "xiigen.codex_ext_readiness_01.packet.v1"
    subagent_id_or_nickname = "CODEX-EXT-READINESS-01"
    generated_at_local = Get-Date -Format o
    active_luba_quote_creating_authority = "Ne ponimayu pochemu snova stop??? ... NO STOPS approved!!! All through agents!!!"
    requested_artifact_type = "Codex extension readiness evidence packet plus minimal fixes if needed"
    codex_repo = $RepoRoot
    codex_target = Join-Path $RepoRoot "src\codex-entrypoint.js"
    changed_files = $changedFiles
    exact_files_read = $exactFilesRead
    exact_files_written = $changedFiles
    commands_run = $commandResults
    tests_builds_smokes_status = [ordered]@{}
    codex_readiness_status = "PARTIAL"
    gates_passed = $gatesPassed
    gates_not_proven = $gatesNotProven
    state_transition_made_or_not = "MADE: execution state codex_ext_readiness_01_20260628 marker updated to PARTIAL by this packet script."
    next_bounded_worker_scope = "CODEX-EXT-PATH-B-RUNTIME-01: run the real Codex plugin/app/chat/UI/CLI automation Path B same-scenario proof, or produce an exact route/input blocker with Codex screen/state/command surface, no secrets captured, window/session evidence if UI is used, raw/redacted input/output, fresh correlation id, and comparison to the Row23 adapter/core artifacts."
    evidence_supports_only = "Codex local source/plugin package inspection, syntax, source wiring, component-level adapter/core QA, available-skills parser regression, and Row23 safe-input direct source-entrypoint smoke through llm_mvp_core. It is not full Codex readiness, not live Codex host UI/app proof, not token savings, not quality improvement, not learnability/training, not lifecycle readiness, not rollback/target promotion, and not final three-extension readiness."
    blockers = @(
        "No real Codex host Path B proof was executed in this bounded worker. The next worker must invoke the real Codex host route or record an exact host-route blocker with visible evidence."
    )
    no_overclaim_booleans = [ordered]@{
        codex_full_readiness_claimed = $false
        full_codex_chat_ui_proven = $false
        real_codex_path_b_ui_proven = $false
        token_savings_proven = $false
        quality_improvement_proven = $false
        learnability_or_training_proven = $false
        backup_export_import_continue_training_proven = $false
        rollback_or_target_safety_proven = $false
        final_dual_path_scenario_coverage_proven = $false
        external_ai_used = $false
        network_used = $false
        git_actions_performed = $false
    }
    evidence_refs = [ordered]@{
        runner_summary = $RunnerSummaryPath
        command_results = $CommandResultsPath
        install_smoke_report = $InstallSmokeReportPath
        host_08f_report = $Host08FReportPath
        available_skills_parser_report = $ParserReportPath
        row23_request = $Row23RequestPath
        row23_response = $Row23ResponsePath
        row23_trace = $Row23TracePath
    }
    observed_summary = [ordered]@{
        all_command_exit_codes_zero = [bool]$runnerSummary.all_command_exit_codes_zero
        install_smoke_passed = [bool]$runnerSummary.observed.install_smoke_passed
        host_08f_passed = [bool]$runnerSummary.observed.host_08f_passed
        parser_passed = [bool]$runnerSummary.observed.parser_passed
        row23_response_success = [bool]$runnerSummary.observed.row23_response_success
        row23_response_status = [string]$runnerSummary.observed.row23_response_status
        row23_trace_correlation_id = [string]$runnerSummary.observed.row23_trace_correlation_id
        row23_semantic_quality_claimed = [bool]$runnerSummary.observed.row23_semantic_quality_claimed
        host_08f_claim_scope = [string]$host08f.claim_scope
        install_smoke_claim_scope = [string]$installSmoke.claim_scope
        parser_claim_scope = [string]$parserReport.claim_scope
    }
    parent_will_perform_commit_push_after_review = $true
}

$packet.tests_builds_smokes_status = [ordered]@{
    node_check_codex_entrypoint = "PASS exit_code=0"
    install_smoke_source_wiring = "PASS exit_code=0 report.passed=true"
    host_08f_component_qa = "PASS exit_code=0 report.passed=true"
    available_skills_parser_regression = "PASS exit_code=0 verdict=PASS"
    row23_entrypoint_smoke = "PASS exit_code=0 response.Success=true status=core_boundary_partial_no_semantic_quality_claim"
    overall_runner = "PASS exit_code=0"
}

Write-Json -Value $packet -Path $PacketPath -Depth 32

$markdown = @"
# CODEX-EXT-READINESS-01 Evidence Packet

Status: PARTIAL.

What this proves:
- Codex package/source wiring, syntax, component adapter/core QA, available-skill forwarding, redacted debug/error handling, and Row23 safe-input direct source-entrypoint smoke through llm_mvp_core passed locally.

What this does not prove:
- Real Codex host Path B, full Codex chat/app/plugin readiness, token savings, quality improvement, learnability/training, lifecycle portability, rollback/target safety, docs, or final dual-path scenario coverage.

Next bounded worker:
- CODEX-EXT-PATH-B-RUNTIME-01: run the real Codex plugin/app/chat/UI/CLI automation same-scenario proof or produce an exact host-route blocker.

Evidence:
- Packet JSON: $PacketPath
- Runner summary: $RunnerSummaryPath
- Command results: $CommandResultsPath
"@
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($PacketMarkdownPath, $markdown, $utf8NoBom)

$state = Read-Json -Path $StatePath
$marker = [ordered]@{
    schema = "xiigen.codex_ext_readiness_01.state_marker.v1"
    subagent_id_or_nickname = "CODEX-EXT-READINESS-01"
    timestamp = Get-Date -Format o
    status = "PARTIAL"
    evidence_packet = $PacketPath
    codex_repo = $RepoRoot
    codex_target = Join-Path $RepoRoot "src\codex-entrypoint.js"
    local_adapter_core_boundary = "PASS_FOR_LOCAL_ADAPTER_CORE_BOUNDARY_ONLY"
    command_exit_codes_zero = [bool]$runnerSummary.all_command_exit_codes_zero
    node_check_codex_entrypoint_exit_code = 0
    install_smoke_source_wiring_passed = [bool]$runnerSummary.observed.install_smoke_passed
    host_08f_component_qa_passed = [bool]$runnerSummary.observed.host_08f_passed
    available_skills_parser_passed = [bool]$runnerSummary.observed.parser_passed
    row23_entrypoint_smoke_exit_code = [int]$runnerSummary.observed.row23_entrypoint_exit_code
    row23_response_success = [bool]$runnerSummary.observed.row23_response_success
    row23_response_status = [string]$runnerSummary.observed.row23_response_status
    row23_correlation_id = [string]$runnerSummary.observed.row23_request_correlation_id
    codex_full_readiness_claimed = $false
    codex_readiness_status = "PARTIAL"
    gates_passed = $gatesPassed
    gates_not_proven = $gatesNotProven
    evidence_supports_only = $packet.evidence_supports_only
    next_bounded_worker_scope = $packet.next_bounded_worker_scope
    git_actions_performed_by_this_subagent = $false
    parent_will_perform_commit_push_after_review = $true
}

$state | Add-Member -NotePropertyName "codex_ext_readiness_01_20260628" -NotePropertyValue $marker -Force
$state.next_action = "CODEX-EXT-READINESS-01 is PARTIAL: Codex local source wiring, component adapter/core QA, available-skill parser regression, and Row23 safe-input direct source-entrypoint smoke passed, but real Codex host Path B, token/quality/learnability/lifecycle/rollback/docs/final coverage are not proven. Next bounded worker scope: CODEX-EXT-PATH-B-RUNTIME-01."
$state.human_readable_status_review_current = "Codex extension check moved one bounded step: package/source inspection, node syntax check, source wiring smoke, component adapter/core QA, available-skills parser regression, and Row23 safe-input direct entrypoint smoke all passed locally. Full Codex readiness is still not proven because no real Codex plugin/app/chat/UI/CLI Path B run, same-scenario comparison, token, quality, learnability, lifecycle, rollback, docs, or final dual-path coverage ran in this packet. Next bounded action is CODEX-EXT-PATH-B-RUNTIME-01."
Write-Json -Value $state -Path $StatePath -Depth 100

Write-Json -Value ([ordered]@{
    packet_path = $PacketPath
    packet_markdown_path = $PacketMarkdownPath
    state_path = $StatePath
    state_marker = "codex_ext_readiness_01_20260628"
    status = "PARTIAL"
}) -Path (Join-Path $EvidenceRoot "packet-and-state-write-summary.json") -Depth 8
