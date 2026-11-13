[CmdletBinding()]
param(
    [string]$EvidenceRoot = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\claude-ext-readiness-01",
    [string]$StatePath = "C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
$packetPath = Join-Path $EvidenceRoot "CLAUDE-EXT-READINESS-01-evidence-packet.json"
$packetMarkdownPath = Join-Path $EvidenceRoot "CLAUDE-EXT-READINESS-01-evidence-packet.md"
$runnerSummaryPath = Join-Path $EvidenceRoot "runner-summary.json"
$installReportPath = Join-Path $EvidenceRoot "install-smoke\install-smoke-report.json"
$row23ResponsePath = Join-Path $EvidenceRoot "row23-adapter-smoke\row23-claude-response.json"
$row23HostTracePath = Join-Path $EvidenceRoot "row23-adapter-smoke\xiigen-claude-host-trace.json"
$row23WithSkillsCommandPath = Join-Path $EvidenceRoot "row23-adapter-smoke-with-skills\row23-claude-adapter-smoke-with-skills.command.json"
$row23WithSkillsResponsePath = Join-Path $EvidenceRoot "row23-adapter-smoke-with-skills\row23-claude-response.json"
$row23WithSkillsHostTracePath = Join-Path $EvidenceRoot "row23-adapter-smoke-with-skills\xiigen-claude-host-trace.json"

function Read-JsonFile {
    param([Parameter(Mandatory = $true)][string]$Path)
    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Write-JsonFile {
    param(
        [Parameter(Mandatory = $true)][object]$Value,
        [Parameter(Mandatory = $true)][string]$Path
    )
    [System.IO.File]::WriteAllText($Path, (($Value | ConvertTo-Json -Depth 100) + [Environment]::NewLine), $utf8NoBom)
}

function Set-NoteProperty {
    param(
        [Parameter(Mandatory = $true)][object]$Object,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][object]$Value
    )
    if ($Object.PSObject.Properties.Name -contains $Name) {
        $Object.$Name = $Value
    }
    else {
        $Object | Add-Member -NotePropertyName $Name -NotePropertyValue $Value
    }
}

$runnerSummary = Read-JsonFile -Path $runnerSummaryPath
$installReport = Read-JsonFile -Path $installReportPath
$row23Response = Read-JsonFile -Path $row23ResponsePath
$row23HostTrace = Read-JsonFile -Path $row23HostTracePath
$row23WithSkillsCommand = Read-JsonFile -Path $row23WithSkillsCommandPath
$row23WithSkillsResponse = Read-JsonFile -Path $row23WithSkillsResponsePath
$row23WithSkillsHostTrace = Read-JsonFile -Path $row23WithSkillsHostTracePath
$state = Read-JsonFile -Path $StatePath

$recordedClaudeExe = "C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.177\claude.exe"
$recordedClaudeExeExists = Test-Path -LiteralPath $recordedClaudeExe
$allEvidenceFiles = @(Get-ChildItem -LiteralPath $EvidenceRoot -Recurse -File | ForEach-Object { $_.FullName } | Sort-Object)

$commandsRun = @()
foreach ($command in @($runnerSummary.commands)) {
    $commandsRun += [ordered]@{
        name = $command.name
        executable = $command.executable
        exit_code = [int]$command.exit_code
        stdout = $command.stdout
        stderr = $command.stderr
        command_meta = $command.command_meta
    }
}
$commandsRun += [ordered]@{
    name = $row23WithSkillsCommand.name
    executable = $row23WithSkillsCommand.executable
    exit_code = [int]$row23WithSkillsCommand.exit_code
    stdout = $row23WithSkillsCommand.stdout
    stderr = $row23WithSkillsCommand.stderr
    command_meta = $row23WithSkillsCommand.command_meta
}
$commandsRun += [ordered]@{
    name = "recorded-claude-exe-test-path"
    executable = "Test-Path"
    exit_code = 0
    result = $recordedClaudeExeExists
    target = $recordedClaudeExe
}
$commandsRun += [ordered]@{
    name = "claude-command-path-check"
    executable = "Get-Command claude / claude.exe"
    exit_code = 1
    result = "no command found on PATH"
}

$gateStatus = [ordered]@{
    real_claude_host_entrypoint_path = "PARTIAL: source entrypoint and plugin manifest exist; recorded Claude executable path missing; no real Claude host invocation proved."
    thin_adapter_call_to_llm_mvp_core = "PASS: install-smoke and Row23 adapter smokes invoked C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1 and returned Success=true."
    visible_answer_or_exact_host_blocker = "PARTIAL: local stdout/chat display exists; real Claude chat/UI visible answer not run; exact host blocker is missing Claude CLI path."
    debug_error_path = "PASS_NARROW: debug trace displayed with redacted input/output and ContainsSecrets=false; install-smoke report records safe local adapter behavior."
    raw_redacted_input_output = "PASS_NARROW: raw prompt kept out of host trace, user prompt hash/redacted input written; output stored in local response JSON."
    fresh_correlation_id = "PASS_NARROW: Row23 with skills correlation id $($row23WithSkillsHostTrace.correlation_id)."
    no_hidden_deterministic_shortcut = "PASS_NARROW: host trace reports host_semantic_ranking=false, host_model_logic=false, host_training=false, external_ai_used=false."
    same_scenario_path_a = "PARTIAL: Row23 safe input ran through Claude adapter/core boundary; selected skill list remained empty, but core boundary and non-claim locks passed."
    path_b_real_host_ui_or_blocker = "BLOCKED: no Claude executable found at recorded path or PATH; no UI/browser check was allowed or run."
}

$packet = [ordered]@{
    schema = "xiigen.claude_ext_readiness_01.evidence_packet.v1"
    subagent_id_or_nickname = "CLAUDE-EXT-READINESS-01"
    created_at = Get-Date -Format o
    changed_files = @(
        "No Claude source files changed.",
        "Evidence files under $EvidenceRoot",
        $StatePath
    )
    exact_files_read = @(
        "C:\Projects\llm_mvp_core\AGENTS.md",
        "C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md",
        $StatePath,
        "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json",
        "C:\Projects\claude-xiigen-smart-skills\package.json",
        "C:\Projects\claude-xiigen-smart-skills\README.md",
        "C:\Projects\claude-xiigen-smart-skills\docs\PROJECT-DOC.md",
        "C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.js",
        "C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.ps1",
        "C:\Projects\claude-xiigen-smart-skills\scripts\install-smoke.ps1",
        "C:\Projects\claude-xiigen-smart-skills\prompts\claude-smoke-prompts.jsonl",
        "C:\Projects\claude-xiigen-smart-skills\.claude-plugin\plugin.json",
        "C:\Projects\claude-xiigen-smart-skills\hooks\hooks.json",
        $runnerSummaryPath,
        $installReportPath,
        $row23ResponsePath,
        $row23HostTracePath,
        $row23WithSkillsCommandPath,
        $row23WithSkillsResponsePath,
        $row23WithSkillsHostTracePath
    )
    exact_files_written = @($allEvidenceFiles + $packetPath + $packetMarkdownPath + $StatePath | Sort-Object -Unique)
    commands_run = $commandsRun
    tests_builds_smokes_status = [ordered]@{
        npm_install_smoke = "PASS exit 0; adapterFunctionalityPassed=true; runtimeEvidence=false; evidence_scope=adapter_functionality_only_no_runtime_claim."
        row23_adapter_smoke = "PASS exit 0; core boundary returned Success=true and host trace kept host semantic/training/external AI flags false."
        row23_adapter_smoke_with_available_skills = "PASS exit 0; core boundary returned Success=true; selected skill list remained empty; semantic quality/readiness remained unclaimed."
        build_tests = "No separate build/test script exists in package.json beyond install-smoke; no npm install, network, UI, browser, git, Cursor, or Codex commands were run."
    }
    claude_readiness_status = "PARTIAL"
    claude_readiness_exact_gates = $gateStatus
    state_transition_made_or_not = "MADE: added claude_ext_readiness_01_20260628 and updated next_action/human_readable_status_review_current for Claude status only."
    next_bounded_worker_scope = "CLAUDE-HOST-RUNTIME-PATH-REPAIR-01: find or restore a safe local Claude Code CLI/plugin path, then run plugin validate/details or an approved real Claude host runtime proof without UI/browser/network/secret capture. Do not move to Cursor until parent accepts Claude PARTIAL or this repair passes."
    evidence_supports_only = "Claude extension local source/adapter/core/debug boundary support and exact host-runtime blocker. It does not prove real Claude chat/UI runtime, full Claude readiness, token savings, semantic quality improvement, learnability/training, backup/export/import/share, continue-training, rollback, target promotion, final dual-path scenario coverage, Cursor, or Codex."
    blockers = @(
        "Recorded Claude executable path does not exist: $recordedClaudeExe",
        "No claude or claude.exe command was found on PATH.",
        "No real Claude host/UI/chat Path B proof was run; UI/browser checks were forbidden.",
        "Row23 task-to-skills smoke returned Success=true but no non-empty selected skill list."
    )
    non_claims = [ordered]@{
        full_claude_readiness = $false
        cursor_or_codex_readiness = $false
        token_savings = $false
        semantic_quality_improvement = $false
        learnability_or_training = $false
        backup_export_import_share_continue_training = $false
        rollback_or_target_promotion = $false
        final_dual_path_scenario_coverage = $false
    }
}

Write-JsonFile -Value $packet -Path $packetPath

$markdown = @"
# CLAUDE-EXT-READINESS-01 Evidence Packet

Status: PARTIAL.

What passed:
- Local package smoke passed with exit code 0.
- Row23 safe input passed through the Claude adapter into llm_mvp_core with exit code 0.
- Redacted debug trace was produced; secrets/raw private content flags were false.
- Host trace says host semantic ranking/model logic/training/external AI were false.

What did not pass:
- Real Claude host runtime was not proven.
- Recorded Claude executable path is missing and no claude command was found on PATH.
- Row23 task-to-skills smoke stayed narrow; selected skills were empty.

Next bounded worker scope:
- CLAUDE-HOST-RUNTIME-PATH-REPAIR-01.

Evidence supports only:
- Claude local adapter/core/debug boundary support plus exact host runtime blocker.
"@
[System.IO.File]::WriteAllText($packetMarkdownPath, $markdown, $utf8NoBom)

$stateRecord = [ordered]@{
    schema = "xiigen.claude_ext_readiness_01.state_transition.v1"
    subagent_id_or_nickname = "CLAUDE-EXT-READINESS-01"
    timestamp = Get-Date -Format o
    status = "PARTIAL"
    evidence_packet = $packetPath
    evidence_root = $EvidenceRoot
    gates = $gateStatus
    commands_run = $commandsRun
    changed_files = $packet.changed_files
    evidence_supports_only = $packet.evidence_supports_only
    next_bounded_worker_scope = $packet.next_bounded_worker_scope
    blockers = $packet.blockers
    parent_will_perform_commit_push_after_review = $true
    git_actions_performed_by_this_subagent = $false
}

Set-NoteProperty -Object $state -Name "claude_ext_readiness_01_20260628" -Value $stateRecord
Set-NoteProperty -Object $state -Name "next_action" -Value "CLAUDE-EXT-READINESS-01 is PARTIAL: local adapter/core/debug evidence passed, but real Claude host runtime is blocked by missing Claude CLI path and Row23 selected skills remained empty. Next bounded worker scope: CLAUDE-HOST-RUNTIME-PATH-REPAIR-01 unless parent accepts this partial packet and deliberately routes Cursor next."
Set-NoteProperty -Object $state -Name "human_readable_status_review_current" -Value "Claude extension check moved one bounded step: local Claude adapter/core/debug smokes passed, no Claude source change was needed, but full Claude readiness is not proven. The exact blocker is host runtime path: recorded claude.exe is missing and no claude command was found on PATH. Scenario coverage remains incomplete; token savings, quality improvement, learnability, rollback, and final readiness remain unclaimed. Next bounded action is Claude host runtime path repair or parent acceptance of PARTIAL before Cursor."
Set-NoteProperty -Object $state -Name "done" -Value $false
Set-NoteProperty -Object $state -Name "operator_input_required" -Value $false
Write-JsonFile -Value $state -Path $StatePath

Write-Output $packetPath
