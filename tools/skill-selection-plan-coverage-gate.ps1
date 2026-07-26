param(
    [Parameter(Mandatory = $true)]
    [string]$UnifiedPlan,

    [string]$SkillPlan,

    [string]$Inventory,

    [switch]$RequireUserPromptLedger,

    [switch]$RequireSubAgentReviewLoop,

    [int]$MaxWorkOrderMinutes = 0
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function New-Check {
    param(
        [Parameter(Mandatory = $true)][string]$Id,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][bool]$Passed,
        [string[]]$Evidence = @(),
        [string[]]$Gaps = @()
    )

    [pscustomobject]@{
        id       = $Id
        name     = $Name
        passed   = $Passed
        evidence = @($Evidence)
        gaps     = @($Gaps)
    }
}

function Test-AllTokens {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string[]]$Tokens
    )

    foreach ($token in $Tokens) {
        if (-not $Text.ToLowerInvariant().Contains($token.ToLowerInvariant())) {
            return $false
        }
    }

    return $true
}

function Missing-Tokens {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string[]]$Tokens
    )

    $missing = @()
    foreach ($token in $Tokens) {
        if (-not $Text.ToLowerInvariant().Contains($token.ToLowerInvariant())) {
            $missing += $token
        }
    }

    return $missing
}

$inputPaths = [ordered]@{
    unified_plan = $UnifiedPlan
}
if (-not [string]::IsNullOrWhiteSpace($SkillPlan)) {
    $inputPaths.skill_plan = $SkillPlan
}
if (-not [string]::IsNullOrWhiteSpace($Inventory)) {
    $inputPaths.inventory = $Inventory
}

$missingInputs = @()
foreach ($entry in $inputPaths.GetEnumerator()) {
    if (-not (Test-Path -LiteralPath $entry.Value -PathType Leaf)) {
        $missingInputs += "$($entry.Key): $($entry.Value)"
    }
}
if (-not $RequireUserPromptLedger -and -not $RequireSubAgentReviewLoop) {
    if ([string]::IsNullOrWhiteSpace($SkillPlan)) {
        $missingInputs += "skill_plan: missing -SkillPlan argument"
    }
    if ([string]::IsNullOrWhiteSpace($Inventory)) {
        $missingInputs += "inventory: missing -Inventory argument"
    }
}

if ($missingInputs.Count -gt 0) {
    $result = [pscustomobject]@{
        passed       = $false
        input_files  = $inputPaths
        checks       = @()
        gaps         = @("Missing input file(s): $($missingInputs -join '; ')")
        generated_at = (Get-Date).ToUniversalTime().ToString("o")
    }
    $result | ConvertTo-Json -Depth 8
    exit 2
}

$unified = Get-Content -Raw -LiteralPath $UnifiedPlan
$skill = if (-not [string]::IsNullOrWhiteSpace($SkillPlan) -and (Test-Path -LiteralPath $SkillPlan -PathType Leaf)) { Get-Content -Raw -LiteralPath $SkillPlan } else { "" }
$inventoryText = if (-not [string]::IsNullOrWhiteSpace($Inventory) -and (Test-Path -LiteralPath $Inventory -PathType Leaf)) { Get-Content -Raw -LiteralPath $Inventory } else { "" }

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
$agentsPath = Join-Path $repoRoot "AGENTS.md"
$planningSkillPath = Join-Path $repoRoot "docs/skills/planning-session-startup-SKILL.md"
$planReviewSkillPath = Join-Path $repoRoot "docs/skills/plan-review-SKILL.md"

$agents = if (Test-Path -LiteralPath $agentsPath -PathType Leaf) { Get-Content -Raw -LiteralPath $agentsPath } else { "" }
$planningSkill = if (Test-Path -LiteralPath $planningSkillPath -PathType Leaf) { Get-Content -Raw -LiteralPath $planningSkillPath } else { "" }
$planReviewSkill = if (Test-Path -LiteralPath $planReviewSkillPath -PathType Leaf) { Get-Content -Raw -LiteralPath $planReviewSkillPath } else { "" }

$checks = @()

$sc1Tokens = @(
    "Phase 37",
    "Phase 38",
    "Phase 39",
    "skills-inventory-universal-vs-project-specific.md",
    "PLAN-XIIGEN-SKILL-SELECTION-MACHINE-EXTENSION-v1.md",
    "SkillSelectionMachine",
    "S.0",
    "S.15",
    "Integrated source inputs",
    "one_unified_plan=true",
    "no_one_way_reference_substitute=true"
)
$sc1Missing = @(Missing-Tokens -Text $unified -Tokens $sc1Tokens)
$sourceInputsPresent = if ($RequireUserPromptLedger -or $RequireSubAgentReviewLoop) { $true } else { $skill.Length -gt 1000 -and $inventoryText.Length -gt 1000 }
$checks += New-Check `
    -Id "SC-1" `
    -Name "Unified plan covers v20 and skill-inventory sources, not reference-only" `
    -Passed ($sc1Missing.Count -eq 0 -and $sourceInputsPresent) `
    -Evidence @("Unified plan contains Phase37/38/39, SkillSelectionMachine, S.0-S.15, integrated source-input and no-one-way-reference markers.", "Skill plan and inventory inputs are present and non-empty.") `
    -Gaps ($sc1Missing | ForEach-Object { "Unified plan missing required integration marker: $_" })

$sc2Tokens = @(
    "Phase 39 User Prompt Source Ledger",
    "UPS-S-001",
    "UPS-S-015",
    "Conversation Delta Ledger",
    "Completion Matrix",
    "section, gate, artifact",
    "S-R1",
    "S-R15",
    "CM-20",
    "CM-29"
)
$sc2Missing = @(Missing-Tokens -Text $unified -Tokens $sc2Tokens)
$checks += New-Check `
    -Id "SC-2" `
    -Name "User requirements from both conversations are mapped to sections, gates, and artifacts" `
    -Passed ($sc2Missing.Count -eq 0) `
    -Evidence @("Unified plan has UPS-S source-ledger rows, S-R requirements, and CM completion rows.") `
    -Gaps ($sc2Missing | ForEach-Object { "Unified plan missing user-requirement mapping marker: $_" })

$sc3Tokens = @(
    "Seven-Check Mental Experiment Matrix",
    "SC-1",
    "SC-2",
    "SC-3",
    "SC-4",
    "SC-5",
    "SC-6",
    "SC-7",
    "mental_experiment_scenarios_pass=true",
    "B.0-B.7 + Phase 38 gates + S.0-S.15"
)
$sc3Missing = @(Missing-Tokens -Text $unified -Tokens $sc3Tokens)
$checks += New-Check `
    -Id "SC-3" `
    -Name "Mental experiment coverage exists for all user-defined tasks" `
    -Passed ($sc3Missing.Count -eq 0) `
    -Evidence @("Unified plan contains Seven-Check Mental Experiment Matrix and cross-phase thought experiments.") `
    -Gaps ($sc3Missing | ForEach-Object { "Unified plan missing mental-experiment marker: $_" })

$sc4Tokens = @(
    "No deterministic algorithm is hidden",
    "Deterministic semantic and grounding paths are immediate failures",
    "SkillSelectionMachine",
    "trainable adapters",
    "IMachineTraining",
    "ILearningMachine",
    "S.6",
    "S.14"
)
$sc4Missing = @(Missing-Tokens -Text $unified -Tokens $sc4Tokens)
$checks += New-Check `
    -Id "SC-4" `
    -Name "No hidden deterministic semantic algorithm replaces a trainable unit" `
    -Passed ($sc4Missing.Count -eq 0) `
    -Evidence @("Unified plan bans deterministic semantic authority and requires trainable SkillSelection units/adapters.") `
    -Gaps ($sc4Missing | ForEach-Object { "Unified plan missing anti-deterministic/trainable marker: $_" })

$sc5Tokens = @(
    "Common And Adaptive Asset Boundary",
    "common trained/shared",
    "adaptive user/project-local",
    "saveable, importable, exportable, publishable, and upgradable",
    ".xiigen",
    "S.9",
    "S.11",
    "S.13"
)
$sc5Missing = @(Missing-Tokens -Text $unified -Tokens $sc5Tokens)
$checks += New-Check `
    -Id "SC-5" `
    -Name "Models and data sets are repo assets split into common/shared and adaptive/project-local parts" `
    -Passed ($sc5Missing.Count -eq 0) `
    -Evidence @("Unified plan contains common/adaptive asset boundary and upgrade/continue gates.") `
    -Gaps ($sc5Missing | ForEach-Object { "Unified plan missing asset-boundary marker: $_" })

$sc6PlanTokens = @(
    "User Workstyle Rule Coverage",
    "architect anti-simplification",
    "no-corner-cutting",
    "AGENTS.md",
    "planning-session-startup-SKILL.md",
    "plan-review-SKILL.md",
    "OP-45"
)
$sc6PlanMissing = @(Missing-Tokens -Text $unified -Tokens $sc6PlanTokens)
$sc6AgentTokens = @("OP-45", "anti-masquerade", "Architect", "simplify")
$sc6PlanningTokens = @("architect", "simplification")
$sc6ReviewTokens = @("architect", "corner")
$sc6Missing = @()
$sc6Missing += ($sc6PlanMissing | ForEach-Object { "Unified plan missing workstyle marker: $_" })
$sc6Missing += (@(Missing-Tokens -Text $agents -Tokens $sc6AgentTokens) | ForEach-Object { "AGENTS.md missing workstyle marker: $_" })
$sc6Missing += (@(Missing-Tokens -Text $planningSkill -Tokens $sc6PlanningTokens) | ForEach-Object { "planning-session-startup skill missing marker: $_" })
$sc6Missing += (@(Missing-Tokens -Text $planReviewSkill -Tokens $sc6ReviewTokens) | ForEach-Object { "plan-review skill missing marker: $_" })
$checks += New-Check `
    -Id "SC-6" `
    -Name "User workstyle is persisted in AGENTS and planning/plan-review skills, including OP-45" `
    -Passed ($sc6Missing.Count -eq 0) `
    -Evidence @("Unified plan and governance/skill files contain OP-45 and architect anti-simplification/corner-cutting markers.") `
    -Gaps $sc6Missing

$sc7Tokens = @(
    "Core/Host API Boundary",
    "Separate Host Repositories",
    "llm_mvp_core",
    "host repos use API/check-set request-response",
    "cursor-xiigen-smart-skills",
    "claude-xiigen-smart-skills",
    "codex-xiigen-smart-skills",
    "S.7",
    "S.8",
    "S.10"
)
$sc7Missing = @(Missing-Tokens -Text $unified -Tokens $sc7Tokens)
$checks += New-Check `
    -Id "SC-7" `
    -Name "Core logic remains in llm_mvp_core; host repos stay simple API/check-set shells" `
    -Passed ($sc7Missing.Count -eq 0) `
    -Evidence @("Unified plan contains core/host API boundary and separate cursor/claude/codex host repo names.") `
    -Gaps ($sc7Missing | ForEach-Object { "Unified plan missing core/host boundary marker: $_" })

if ($RequireUserPromptLedger) {
    $ledgerTokens = @(
        "Phase 39 User Prompt Source Ledger",
        "UPS-S-001",
        "UPS-S-002",
        "UPS-S-003",
        "UPS-S-004",
        "UPS-S-005",
        "UPS-S-011",
        "UPS-S-015",
        "UPS-S-016",
        "skills-inventory-universal-vs-project-specific.md",
        "часть extensions",
        "не больше 15 минут",
        "sub-agent",
        "Architect Sub-Agent Merge Review Loop",
        "architect anti-simplification",
        "no-corner-cutting",
        "PRE_SPLIT_SEVEN_CHECK",
        "POST_SPLIT_SEVEN_CHECK",
        "OP-45",
        "anti-masquerade",
        "external/image-generation",
        "project-owned trainable assets",
        "checkpoints/manifests",
        "host repos use API/check-set request-response"
    )
    $ledgerMissing = @(Missing-Tokens -Text $unified -Tokens $ledgerTokens)
    $checks += New-Check `
        -Id "S12-LEDGER" `
        -Name "User prompt source ledger covers smart-skills, v20 integration, sub-agent loops, separate repos, OP-45, and external-model ban" `
        -Passed ($ledgerMissing.Count -eq 0) `
        -Evidence @("Unified plan contains user prompt source ledger rows and markers for every S.12 topic.") `
        -Gaps ($ledgerMissing | ForEach-Object { "Unified plan missing S.12 ledger marker: $_" })

    $matrixTokens = @(
        "Completion Matrix",
        "CM-20",
        "CM-21",
        "CM-23",
        "CM-24",
        "CM-25",
        "CM-29",
        "CM-30",
        "User Prompt Source Ledger",
        "Common/Adaptive Asset Boundary",
        "Core/Host API Boundary",
        "No external/image-generation model"
    )
    $matrixMissing = @(Missing-Tokens -Text $unified -Tokens $matrixTokens)
    $checks += New-Check `
        -Id "S12-COMPLETION-MATRIX" `
        -Name "Completion matrix has corresponding rows for S.12 ledger topics and external-model ban" `
        -Passed ($matrixMissing.Count -eq 0) `
        -Evidence @("Completion Matrix contains rows for user prompt ledger, split checks, common/adaptive assets, core/host boundary, and external-model ban.") `
        -Gaps ($matrixMissing | ForEach-Object { "Unified plan missing S.12 completion-matrix marker: $_" })
}

if ($RequireSubAgentReviewLoop) {
    $s15ReviewTokens = @(
        "S.15 plan approval loop and bounded work-order ledger",
        "Architect Sub-Agent Merge Review Loop",
        "sub-agent review",
        "SS-WO-14a",
        "SS-WO-14b",
        "open_findings: []",
        "Repeat until no blocking or minor findings remain"
    )
    $s15ReviewMissing = @(Missing-Tokens -Text $unified -Tokens $s15ReviewTokens)
    $checks += New-Check `
        -Id "S15-SUBAGENT-REVIEW-LOOP" `
        -Name "Plan has independent sub-agent review/fix rounds before approval" `
        -Passed ($s15ReviewMissing.Count -eq 0) `
        -Evidence @("Unified plan contains Architect Sub-Agent Merge Review Loop and SS-WO-14a/14b review/fix cycle markers.") `
        -Gaps ($s15ReviewMissing | ForEach-Object { "Unified plan missing S.15 sub-agent review marker: $_" })

    $s15FindingTokens = @(
        "recorded as a real blocker",
        "Architect patches the plan or records a real blocker",
        "sub_agent_review_findings_open=0",
        "review finds any `SS-WO` row too broad",
        "architect splits that row before execution"
    )
    $s15FindingMissing = @(Missing-Tokens -Text $unified -Tokens $s15FindingTokens)
    $checks += New-Check `
        -Id "S15-FINDINGS-CLOSED-OR-BLOCKED" `
        -Name "Every review finding is fixed or recorded as a real blocker" `
        -Passed ($s15FindingMissing.Count -eq 0) `
        -Evidence @("Unified plan requires findings to be patched or recorded as real blockers and has zero-open-findings evidence marker.") `
        -Gaps ($s15FindingMissing | ForEach-Object { "Unified plan missing finding-closure marker: $_" })

    $maxMinuteGaps = @()
    if ($MaxWorkOrderMinutes -le 0) {
        $maxMinuteGaps += "MaxWorkOrderMinutes must be a positive integer."
    }
    $workOrderRows = @([regex]::Matches($unified, '(?m)^\|\s*(P37-WO|P38-WO|SS-WO)-[^|]*\|.*$') | ForEach-Object { $_.Value })
    if ($workOrderRows.Count -eq 0) {
        $maxMinuteGaps += "No P37/P38/SS work-order rows found."
    }
    $durationMarker = "<=$MaxWorkOrderMinutes min"
    $rowsWithoutDuration = @()
    foreach ($row in $workOrderRows) {
        if ($MaxWorkOrderMinutes -gt 0 -and -not $row.ToLowerInvariant().Contains($durationMarker.ToLowerInvariant())) {
            $rowsWithoutDuration += $row
        }
    }
    foreach ($row in $rowsWithoutDuration) {
        $maxMinuteGaps += "Work-order row missing $durationMarker marker: $row"
    }
    $checks += New-Check `
        -Id "S15-MAX-WORK-ORDER-MINUTES" `
        -Name "Every executor work order is bounded to one narrow <= MaxWorkOrderMinutes slice" `
        -Passed ($maxMinuteGaps.Count -eq 0) `
        -Evidence @("Found $($workOrderRows.Count) P37/P38/SS work-order ledger rows and checked each for $durationMarker.") `
        -Gaps $maxMinuteGaps

    $s15ReviewGateTokens = @(
        "seven-check review",
        "PRE_SPLIT_SEVEN_CHECK",
        "POST_SPLIT_SEVEN_CHECK",
        "OP-45",
        "OP-46",
        "external/image-generation",
        "No external/image-generation model"
    )
    $s15ReviewGateMissing = @(Missing-Tokens -Text $unified -Tokens $s15ReviewGateTokens)
    $s15ReviewGateMissing += (@(Missing-Tokens -Text $agents -Tokens @("OP-45", "OP-46", "anti-masquerade", "external AI")) | ForEach-Object { "AGENTS.md missing review/gate marker: $_" })
    $checks += New-Check `
        -Id "S15-SEVEN-CHECK-OP45-OP46" `
        -Name "Seven-check review, OP-45, and OP-46 are represented in review/gate requirements" `
        -Passed ($s15ReviewGateMissing.Count -eq 0) `
        -Evidence @("Unified plan and AGENTS.md contain seven-check, OP-45 anti-masquerade, and OP-46 external-tool-ban markers.") `
        -Gaps ($s15ReviewGateMissing | ForEach-Object { "S.15 review/gate coverage missing marker: $_" })

    $s15ArchitectTokens = @(
        "architect anti-simplification",
        "no-corner-cutting",
        "architect fix-list requirements are not stop gates",
        "fix-list requirements",
        "not stop gates",
        "parent architect session does not execute production/test code or dotnet commands"
    )
    $s15ArchitectMissing = @(Missing-Tokens -Text $unified -Tokens $s15ArchitectTokens)
    $checks += New-Check `
        -Id "S15-ARCHITECT-NO-STOP-MISUSE" `
        -Name "Architect fix-list requirements are not stop gates and parent architect does not execute code/test/build" `
        -Passed ($s15ArchitectMissing.Count -eq 0) `
        -Evidence @("Unified plan contains architect anti-simplification/no-corner-cutting markers and parent-architect no-code-execution boundary.") `
        -Gaps ($s15ArchitectMissing | ForEach-Object { "Unified plan missing architect no-stop/no-execution marker: $_" })

    $s15CompletionTokens = @(
        "Completion Matrix",
        "CM-18",
        "CM-19",
        "CM-20",
        "CM-21",
        "CM-22",
        "CM-28",
        "CM-30"
    )
    $s15CompletionMissing = @(Missing-Tokens -Text $unified -Tokens $s15CompletionTokens)
    $checks += New-Check `
        -Id "S15-COMPLETION-MATRIX-CONTROLS" `
        -Name "Completion matrix has rows for sub-agent loop, work-order bounds, split seven-checks, and OP-46" `
        -Passed ($s15CompletionMissing.Count -eq 0) `
        -Evidence @("Completion Matrix contains CM-18/19/20/21/22/28/30 control rows.") `
        -Gaps ($s15CompletionMissing | ForEach-Object { "Unified plan missing S.15 completion matrix marker: $_" })
}

$allGaps = @()
foreach ($check in $checks) {
    if (-not $check.passed) {
        $allGaps += $check.gaps
    }
}

$result = [pscustomobject]@{
    passed       = ($allGaps.Count -eq 0)
    generated_at = (Get-Date).ToUniversalTime().ToString("o")
    input_files  = $inputPaths
    checked_files = [ordered]@{
        agents_md = $agentsPath
        planning_session_startup_skill = $planningSkillPath
        plan_review_skill = $planReviewSkillPath
    }
    checks       = $checks
    gaps         = $allGaps
}

$result | ConvertTo-Json -Depth 12
if ($result.passed) {
    exit 0
}

exit 1
