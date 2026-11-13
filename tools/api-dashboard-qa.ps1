param(
    [switch]$Json
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).Path
$script:checks = @()

function Join-RepoPath {
    param([string]$RelativePath)

    return Join-Path $repoRoot $RelativePath
}

function Read-RepoText {
    param([string]$RelativePath)

    return [System.IO.File]::ReadAllText((Join-RepoPath $RelativePath), [System.Text.Encoding]::UTF8)
}

function Add-Check {
    param(
        [string]$Name,
        [bool]$Passed,
        [string]$Summary,
        [string]$Evidence
    )

    $script:checks += [pscustomobject]@{
        name = $Name
        passed = $Passed
        summary = $Summary
        evidence = $Evidence
    }
}

$dashboardProject = "43.SmartSkillsDomain.Dashboard/43.SmartSkillsDomain.Dashboard.csproj"
$catalogFile = "43.SmartSkillsDomain.Dashboard/ApiDashboardCatalog.cs"
$fixtureRunnerFile = "43.SmartSkillsDomain.Dashboard/ApiDashboardFixtureRunner.cs"
$sourceDisjointGuardFile = "43.SmartSkillsDomain.Dashboard/ApiDashboardSourceDisjointGuard.cs"
$runnerFile = "43.SmartSkillsDomain.Dashboard/ApiDashboardRunner.cs"
$viewModelFile = "43.SmartSkillsDomain.Dashboard/ApiDashboardViewModel.cs"
$testProjectFile = "Tests/LlmFromScratch.Tests.csproj"
$viewModelTestFile = "Tests/ApiDashboardViewModelTests.cs"
$interactiveWpfTestProject = "Tests.ApiDashboardWpf/Tests.ApiDashboardWpf.csproj"
$interactiveWpfTestFile = "Tests.ApiDashboardWpf/ApiDashboardWindowInteractiveTests.cs"
$contractFile = "43.SmartSkillsDomain/XiigenExtensionApiContracts.cs"
$dashboardDocFile = "docs/projects/43-smart-skills-domain-dashboard.md"
$q8ScopeReviewFile = "artifacts/smart-skills-quality-uplift/q8/dashboard-qa-scope-review.json"
$documentationIndexFile = "docs/projects/PROJECT-DOCUMENTATION-INDEX.md"
$uiTestingSkillFile = "docs/skills/ui-testing-SKILL.md"
$outputReadabilitySkillFile = "docs/skills/output-readability-gate-SKILL.md"
$p36FixturesFile = "artifacts/smart-skills-semantic-quality-completion/p36-eval-fixture-rows/p36-eval-fixtures.jsonl"
$p36SourceDisjointManifestFile = "artifacts/smart-skills-semantic-quality-completion/p36-eval-fixture-rows/p36-source-disjoint-manifest.json"
$p36DashboardScenariosFile = "artifacts/smart-skills-semantic-quality-completion/p36-eval-fixture-rows/p36-dashboard-api-scenarios.jsonl"
$p37HumanLabelsFile = "artifacts/smart-skills-semantic-quality-completion/p37-runner-dashboard-wiring-design/p37-dashboard-human-labels.jsonl"
$p37ReadinessGapLedgerFile = "artifacts/smart-skills-semantic-quality-completion/p37-runner-dashboard-wiring-design/p37-readiness-gap-ledger.jsonl"
$p42ArtifactDir = "artifacts/smart-skills-semantic-quality-completion/p42-dashboard-overclaim-firewall"
$overclaimTestFile = "Tests/ApiDashboardOverclaimFirewallTests.cs"

function Test-P42OverclaimAllowedContext {
    param([string]$Text)

    $lower = $Text.ToLowerInvariant()
    $allowedMarkers = @(
        "not ",
        "no ",
        "never",
        "cannot",
        "does not",
        "do not",
        "doesn't",
        "false",
        "without",
        "outside",
        "separate",
        "future",
        "missing",
        "blocked",
        "gap",
        "remains",
        "forbidden",
        "reject",
        "rejected",
        "must fail",
        "should fail",
        "violation",
        "negative",
        "non-claim",
        "not claimed",
        "no claim"
    )

    return @($allowedMarkers | Where-Object { $lower.Contains($_) }).Count -gt 0
}

function Get-P42OverclaimViolations {
    param(
        [string]$Source,
        [string]$Text
    )

    $patterns = @(
        @{ name = "semantic_quality_forward"; regex = "\bsemantic[- ]quality\b.{0,80}\b(proven|proved|ready|complete|completed|closed|passes|passed)\b" },
        @{ name = "semantic_quality_reverse"; regex = "\b(proves?|proved|proven|readies|completes?)\b.{0,80}\bsemantic[- ]quality\b" },
        @{ name = "trained_model_readiness"; regex = "\btrained[- ]model(?:\s+readiness)?\b.{0,80}\b(proven|proved|ready|complete|completed|passes|passed)\b" },
        @{ name = "host_runtime_forward"; regex = "\b(claude|cursor|codex)\b.{0,40}\bhost[- ]runtime\b.{0,80}\b(proven|proved|ready|complete|completed|passes|passed)\b" },
        @{ name = "host_runtime_reverse"; regex = "\b(proves?|proved|proven|readies|completes?)\b.{0,80}\b(claude|cursor|codex)\b.{0,40}\bhost[- ]runtime\b" },
        @{ name = "checkpoint_lifecycle"; regex = "\b(checkpoint rewrite|fresh-load|fresh load|continue-training|continue training)\b.{0,80}\b(complete|completed|proven|proved|ready|passes|passed)\b" }
    )

    $violations = New-Object System.Collections.Generic.List[string]
    $lines = $Text -split "\r?\n"
    for ($index = 0; $index -lt $lines.Count; $index++) {
        $line = $lines[$index]
        if (Test-P42OverclaimAllowedContext $line) {
            continue
        }

        foreach ($pattern in $patterns) {
            if ($line -match $pattern.regex) {
                $violations.Add("$($Source):$($index + 1):$($pattern.name):$($line.Trim())")
            }
        }
    }

    return @($violations)
}

$projectExists = Test-Path -LiteralPath (Join-RepoPath $dashboardProject)
Add-Check "dashboard_project_exists" $projectExists "Dashboard project file exists." $dashboardProject

if ($projectExists) {
    $projectText = Read-RepoText $dashboardProject
    Add-Check "dashboard_project_is_real_reference" ($projectText -match "<ProjectReference Include=`"..\\43\.SmartSkillsDomain\\43\.SmartSkillsDomain\.csproj`"" -and $projectText -match "<TargetFrameworks>net9\.0;net9\.0-windows</TargetFrameworks>") "Dashboard project references SmartSkillsDomain and targets normal test plus WPF builds." $dashboardProject
    Add-Check "dashboard_project_uses_wpf" ($projectText -match "<UseWPF>true</UseWPF>") "Dashboard project has the WPF target enabled." $dashboardProject
}

$wpfFiles = @(
    "43.SmartSkillsDomain.Dashboard/App.xaml",
    "43.SmartSkillsDomain.Dashboard/App.xaml.cs",
    "43.SmartSkillsDomain.Dashboard/ApiDashboardWindow.xaml",
    "43.SmartSkillsDomain.Dashboard/ApiDashboardWindow.xaml.cs"
)
$missingWpfFiles = @($wpfFiles | Where-Object { -not (Test-Path -LiteralPath (Join-RepoPath $_)) })
Add-Check "wpf_files_exist" ($missingWpfFiles.Count -eq 0) "WPF app and window files exist." (($wpfFiles -join "; ") + $(if ($missingWpfFiles.Count -gt 0) { "; missing: $($missingWpfFiles -join ', ')" } else { "" }))

$windowText = if (Test-Path -LiteralPath (Join-RepoPath "43.SmartSkillsDomain.Dashboard/ApiDashboardWindow.xaml")) { Read-RepoText "43.SmartSkillsDomain.Dashboard/ApiDashboardWindow.xaml" } else { "" }
$requiredWpfControls = @(
    "ApiListBox",
    "FixtureExampleListBox",
    "FixtureExpectedResultTextBox",
    "UseExampleButton",
    "InputTextBox",
    "RunButton",
    "HumanResultTextBox",
    "BoundaryItemsControl",
    "RawJsonTextBox",
    "DebugTraceTextBox",
    "HistoryListBox",
    "ErrorTextBlock"
)
$missingWpfControls = @($requiredWpfControls | Where-Object { $windowText -notmatch "x:Name=`"$_`"" })
$wpfWindowControlProjectEvidence = $projectExists -and $missingWpfFiles.Count -eq 0 -and $missingWpfControls.Count -eq 0
Add-Check "wpf_window_control_project_evidence_exists" $wpfWindowControlProjectEvidence "Dashboard WPF project and named controls needed for window/control automation are present." ("missing controls: $($missingWpfControls -join ', ')")

$requiredReadableLabels = @(
    "Available API methods",
    "Fixture examples",
    "Expected fixture result",
    "Example input",
    "Input to send",
    "Run API",
    "Show raw JSON",
    "Debug trace",
    "Prepare result copy text",
    "Prepare safe history text",
    "Safe history",
    "Evidence boundary",
    "Raw response / debug trace"
)
$missingReadableLabels = @($requiredReadableLabels | Where-Object { $windowText -notmatch [regex]::Escape($_) })
$requiredAutomationNames = @(
    "Available API methods",
    "Semantic-quality fixture examples",
    "Expected fixture result",
    "Example input for the selected API",
    "Request text input",
    "Run selected API",
    "Raw response JSON",
    "Redacted debug trace",
    "Visible error message"
)
$missingAutomationNames = @($requiredAutomationNames | Where-Object { $windowText -notmatch "AutomationProperties.Name=`"$([regex]::Escape($_))`"" })
$uxMetadataOk = $missingReadableLabels.Count -eq 0 `
    -and $missingAutomationNames.Count -eq 0 `
    -and $windowText -match "AutomationProperties.HelpText=" `
    -and $windowText -match "ToolTip=" `
    -and $windowText -match "FixtureExampleListBox" `
    -and $windowText -match "FixtureExpectedResultTextBox" `
    -and $windowText -match "TabIndex=" `
    -and $windowText -match "SelectedApiInputDescriptionTextBlock" `
    -and $windowText -match "SelectedApiOutputPreviewTextBlock" `
    -and $windowText -match "SelectedSafetyBoundaryTextBlock"
Add-Check "wpf_ux_metadata_exists" $uxMetadataOk "Dashboard XAML exposes readable labels, accessibility names/help text, tooltips, tab metadata, and example/output/safety explanations." ("missing labels: $($missingReadableLabels -join ', '); missing automation names: $($missingAutomationNames -join ', ')")

$interactiveTestProjectExists = Test-Path -LiteralPath (Join-RepoPath $interactiveWpfTestProject)
$interactiveTestProjectText = if ($interactiveTestProjectExists) { Read-RepoText $interactiveWpfTestProject } else { "" }
$interactiveTestProjectIsWpfOnly = $interactiveTestProjectText -match "<TargetFramework>net9\.0-windows</TargetFramework>" -and $interactiveTestProjectText -match "<UseWPF>true</UseWPF>"
$interactiveTestProjectReferencesDashboard = $interactiveTestProjectText -match "<ProjectReference Include=`"..\\43\.SmartSkillsDomain\.Dashboard\\43\.SmartSkillsDomain\.Dashboard\.csproj`""
$interactiveTestProjectOk = $interactiveTestProjectExists -and $interactiveTestProjectIsWpfOnly -and $interactiveTestProjectReferencesDashboard
Add-Check "wpf_interactive_test_project_exists" $interactiveTestProjectOk "WO-6 WPF-only test project exists, targets net9.0-windows with WPF enabled, and references the real dashboard project." $interactiveWpfTestProject

$interactiveTestExists = Test-Path -LiteralPath (Join-RepoPath $interactiveWpfTestFile)
$interactiveTestText = if ($interactiveTestExists) { Read-RepoText $interactiveWpfTestFile } else { "" }
$interactiveTestExecutableText = if ($interactiveTestExists) {
    $withoutBlockComments = [regex]::Replace($interactiveTestText, "(?s)/\*.*?\*/", "")
    (($withoutBlockComments -split "\r?\n") |
        Where-Object { -not $_.TrimStart().StartsWith("//") }) -join [Environment]::NewLine
} else {
    ""
}
$interactiveTestHasDashboardScope = $interactiveTestText -match "ApiDashboardWindow" `
    -and $interactiveTestText -match "ApiListBox" `
    -and $interactiveTestText -match "UseExampleButton" `
    -and $interactiveTestText -match "InputTextBox" `
    -and $interactiveTestText -match "RunButton" `
    -and $interactiveTestText -match "HumanResultTextBox" `
    -and $interactiveTestText -match "BoundaryItemsControl" `
    -and $interactiveTestText -match "RawJsonTextBox" `
    -and $interactiveTestText -match "DebugTraceTextBox" `
    -and $interactiveTestText -match "HistoryListBox" `
    -and $interactiveTestText -match "ErrorTextBlock" `
    -and $interactiveTestText -match "AutomationPeer" `
    -and $interactiveTestText -match "IInvokeProvider" `
    -and $interactiveTestText -match "IValueProvider" `
    -and $interactiveTestText -match "ISelectionItemProvider"
$fullInteractiveWpfAutomationClaimed = $wpfWindowControlProjectEvidence -and $interactiveTestProjectOk -and $interactiveTestExists -and $interactiveTestHasDashboardScope
Add-Check "full_interactive_wpf_automation_evidence_is_dashboard_scoped" $fullInteractiveWpfAutomationClaimed "Full interactive WPF automation is claimed only when the real WPF-only test project/file drives the dashboard window/control path." ("test project exists: $interactiveTestProjectExists; test file exists: $interactiveTestExists")

$interactiveUxEvidence = $interactiveTestExists `
    -and $interactiveTestText -match "ApiDashboardWindow_UxAutomationExposesHumanReadableSurfaceAndStableLayout" `
    -and $interactiveTestText -match "ApiDashboardWindow_UxAutomationTraversesKeyboardFocusStops" `
    -and $interactiveTestText -match "AssertUx" `
    -and $interactiveTestText -match "AssertNoPrimaryPathJargon" `
    -and $interactiveTestText -match "AssertNonOverlappingColumns" `
    -and $interactiveTestText -match "AssertKeyboardFocusOrder" `
    -and $interactiveTestText -match "MoveFocus" `
    -and $interactiveTestText -match "Keyboard\.FocusedElement" `
    -and $interactiveTestText -match "AutomationProperties.GetName" `
    -and $interactiveTestText -match "AutomationProperties.GetHelpText" `
    -and $interactiveTestText -match "Dashboard API request text is required"
Add-Check "qa_ui_ux_automation_evidence_exists" ($uxMetadataOk -and $interactiveUxEvidence) "WPF automation includes a separate UX layer for human-readable surface, accessibility metadata, keyboard focus traversal, no-jargon primary path, bounds, and error clarity." ("interactive UX evidence: $interactiveUxEvidence")

$p39WpfFixtureEvidence = $interactiveTestExecutableText -match "ApiDashboardWindow_FixtureCatalogSelectionRunsDashboardPreviewRequestWithoutModelClaims" `
    -and $interactiveTestExecutableText -match "FixtureExampleListBox" `
    -and $interactiveTestExecutableText -match "FixtureExpectedResultTextBox" `
    -and $interactiveTestExecutableText -match "P36-SKILLSELECTION-01" `
    -and $interactiveTestExecutableText -match "\[`"fixture_preview_only`"\]" `
    -and $interactiveTestExecutableText -match "\[`"fixture_id`"\]" `
    -and $interactiveTestExecutableText -match "SemanticQualityOrTrainedModelProofClaimed"
Add-Check "p39_wpf_fixture_catalog_scenario_exists" $p39WpfFixtureEvidence "WPF automation selects a P36 fixture row, fills the input through the UI, runs the dashboard command, and keeps proof flags false." $interactiveWpfTestFile

$q8PromptToTaskScenarioRequiredText = @(
    "ApiDashboardWindow_PromptToTaskSemanticQualityScenario_DrivesControlsAndShowsHonestDashboardEvidence",
    "dashboard-semantic-quality-prompt-to-task-1",
    "Write a plan for improving skill selection; do not implement.",
    "Missing PromptToTask checkpoint",
    "XiigenExtensionApiContract.PromptToTaskMethod",
    "Turn a user prompt into a structured task",
    "Task type: plan_review_or_plan_repair",
    "Constraints: plan only",
    "Forbidden actions: do not implement",
    "Required evidence: dashboard QA scenario",
    "implementation_authorized=false",
    "AssertPromptToTaskCheckSetValue(rawJsonText, `"implementation_authorized`", `"false`");",
    "AssertPromptToTaskCheckSetValue(rawJsonText, `"constraints`", `"plan_only;review_gates;quoted_transcripts_evidence_only`");",
    "AssertPromptToTaskCheckSetValue(rawJsonText, `"forbidden_actions`", `"do_not_implement;do_not_train;do_not_commit`");",
    "AssertPromptToTaskCheckSetValue(rawJsonText, `"required_evidence`", `"dashboard_qa_scenario;structured_task_ref;review_evidence`");",
    "AssertPromptToTaskCheckSetValue(rawJsonText, `"raw_private_prompt_copied_as_training_evidence`", `"false`");",
    "AssertPromptToTaskCheckSetValue(rawJsonText, `"semantic_quality_claimed`", `"false`");",
    "AssertPromptToTaskCheckSetValue(rawJsonText, `"trained_model_proof_claimed`", `"false`");",
    "AssertPromptToTaskCheckSetValue(rawJsonText, `"host_runtime_claimed`", `"false`");",
    "redacted_input_ref:prompt-to-task-q8-01",
    "output_fields:task_type,goal,constraints,forbidden_actions,required_evidence,implementation_authorized=false",
    "raw_private_prompt_copied_as_training_evidence",
    "dashboard_api_only_not_model_quality_not_host_runtime",
    "degraded_or_not_ready_visible",
    "PROMPT_TO_TASK_NOT_READY_FOR_SEMANTIC_QUALITY",
    "semantic_quality_claimed=false",
    "trained_model_proof_claimed=false",
    "host/runtime=false",
    "AssertPromptToTaskCheckSetValue("
)
$q8PromptToTaskMissingText = @($q8PromptToTaskScenarioRequiredText | Where-Object { $interactiveTestExecutableText -notmatch [regex]::Escape($_) })
$q8PromptToTaskControlPathRequiredSnippets = @(
    "SelectListBoxItem(apiList, promptToTaskApi);",
    "SetText(inputBox, exampleInput);",
    "Toggle(debugToggle);",
    "Toggle(rawJsonToggle);",
    "InvokeButton(runButton);",
    "Assert.Equal(exampleInput, gateway.LastRequest.UserPromptOrTaskText);",
    "SetText(inputBox, negativeInput);",
    "Assert.Equal(negativeInput, gateway.LastRequest.UserPromptOrTaskText);",
    "AssertPromptToTaskScenarioNotReadyOutput(",
    "Assert.False(negativeHistory.Success);",
    "Assert.Equal(`"prompt_to_task_not_ready_degraded_dashboard_scenario`", negativeHistory.Status);",
    "Assert.Equal(ComputeSha256(negativeInput), negativeHistory.PromptSha256);",
    "Assert.Equal(ApiDashboardRunner.DashboardEvidenceScope, negativeHistory.EvidenceScope);",
    "Assert.False(negativeHistory.HostRuntimeProofClaimed);",
    "Assert.False(negativeHistory.SemanticQualityOrTrainedModelProofClaimed);",
    "Assert.Equal(ComputeSha256(exampleInput), positiveHistory.PromptSha256);",
    "Assert.Equal(ApiDashboardRunner.DashboardEvidenceScope, positiveHistory.EvidenceScope);",
    "Assert.False(positiveHistory.HostRuntimeProofClaimed);",
    "Assert.False(positiveHistory.SemanticQualityOrTrainedModelProofClaimed);",
    "Assert.Contains(ComputeSha256(exampleInput), viewModel.LastExportText);",
    "Assert.Contains(ComputeSha256(negativeInput), viewModel.LastExportText);",
    "Assert.DoesNotContain(exampleInput, viewModel.LastExportText);",
    "Assert.DoesNotContain(negativeInput, viewModel.LastExportText);",
    "Assert.DoesNotContain(rawPrompt, resultText, StringComparison.Ordinal);",
    "Assert.DoesNotContain(rawPrompt, rawJsonText, StringComparison.Ordinal);",
    "Assert.DoesNotContain(rawPrompt, debugTraceText, StringComparison.Ordinal);",
    "private static string ComputeSha256(string value)"
)
$q8PromptToTaskMissingControlSnippets = @($q8PromptToTaskControlPathRequiredSnippets | Where-Object { -not $interactiveTestExecutableText.Contains($_) })
$q8PromptToTaskControlPathEvidence = $q8PromptToTaskMissingControlSnippets.Count -eq 0
$q8PromptToTaskPositiveNegativeEvidence = $q8PromptToTaskMissingText.Count -eq 0 -and $q8PromptToTaskControlPathEvidence
Add-Check "q8_01_prompt_to_task_semantic_quality_wpf_scenario_exists" $q8PromptToTaskPositiveNegativeEvidence "Q8-01 WPF dashboard automation scenario covers PromptToTask human API name, example input, run action, visible structured task output, raw/debug boundary, negative/untrained honesty, and no quality/runtime overclaim." ("missing text: $($q8PromptToTaskMissingText -join ', '); missing control snippets: $($q8PromptToTaskMissingControlSnippets -join ' | ')")

$q8OrchestrationScenarioRequiredText = @(
    "ApiDashboardWindow_OrchestrationReasoningSemanticQualityScenario_DrivesControlsAndShowsHonestDashboardEvidence",
    "dashboard-semantic-quality-orchestration-1",
    "Structured task: plan_review_or_plan_repair; gates: plan review, dashboard QA, documentation sync; implementation_authorized=false.",
    "Malformed OrchestrationReasoning task input",
    "XiigenExtensionApiContract.OrchestrationPlanMethod",
    "Find an orchestration plan for a structured task",
    "Selected orchestration family: plan_review_or_plan_repair",
    "Scenario refs: scenario:dashboard-semantic-quality-orchestration-1",
    "Required gates: authority ledger",
    "Implementation is not authorized",
    "warn_when_implementation_not_authorized",
    "rejected_scenario_reasons",
    "redacted_task_ref:orchestration-q8-02",
    "output_fields:orchestration_family,scenario_refs,required_gates,warnings,implementation_authorized=false",
    "malformed_input_visible",
    "ORCHESTRATION_REASONING_NOT_READY_FOR_SEMANTIC_QUALITY",
    "raw_private_task_copied_as_training_evidence",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"orchestration_family`", `"plan_review_or_plan_repair`");",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"selected_scenario_refs`", `"scenario:dashboard-semantic-quality-orchestration-1;scenario:review-gate-boundary`");",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"required_gates`", `"authority_ledger;four_review_cycles;documentation_sync;dashboard_qa`");",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"implementation_authorized`", `"false`");",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"implementation_warning`", `"warn_when_implementation_not_authorized`");",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"plan_invented`", `"false`");",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"raw_private_task_copied_as_training_evidence`", `"false`");",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"semantic_quality_claimed`", `"false`");",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"trained_model_proof_claimed`", `"false`");",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"host_runtime_claimed`", `"false`");"
)
$q8OrchestrationMissingText = @($q8OrchestrationScenarioRequiredText | Where-Object { $interactiveTestExecutableText -notmatch [regex]::Escape($_) })
$q8OrchestrationControlPathRequiredSnippets = @(
    "SelectListBoxItem(apiList, orchestrationApi);",
    "SetText(inputBox, exampleInput);",
    "Toggle(debugToggle);",
    "Toggle(rawJsonToggle);",
    "InvokeButton(runButton);",
    "Assert.Equal(XiigenExtensionApiContract.OrchestrationPlanMethod, gateway.LastRequest!.Method);",
    "Assert.Equal(exampleInput, gateway.LastRequest.UserPromptOrTaskText);",
    "AssertOrchestrationReasoningScenarioPositiveOutput(",
    "Assert.Contains(`"scenario:review-gate-boundary`", resultText, StringComparison.Ordinal);",
    "Assert.Contains(`"four review cycles`", resultText, StringComparison.Ordinal);",
    "Assert.Contains(`"documentation sync`", resultText, StringComparison.Ordinal);",
    "Assert.Contains(`"dashboard QA`", resultText, StringComparison.Ordinal);",
    "SetText(inputBox, negativeInput);",
    "Assert.Equal(negativeInput, gateway.LastRequest.UserPromptOrTaskText);",
    "AssertOrchestrationReasoningScenarioNotReadyOutput(",
    "AssertQ8ScenarioHistoryAndExport(",
    "Assert.False(negativeHistory.Success);",
    "Assert.Equal(ComputeSha256(negativeInput), negativeHistory.PromptSha256);",
    "Assert.Equal(ApiDashboardRunner.DashboardEvidenceScope, negativeHistory.EvidenceScope);",
    "Assert.False(negativeHistory.HostRuntimeProofClaimed);",
    "Assert.False(negativeHistory.SemanticQualityOrTrainedModelProofClaimed);",
    "Assert.Contains(ComputeSha256(exampleInput), viewModel.LastExportText);",
    "Assert.DoesNotContain(exampleInput, viewModel.LastExportText);",
    "Assert.DoesNotContain(negativeInput, viewModel.LastExportText);",
    "Assert.DoesNotContain(rawTask, resultText, StringComparison.Ordinal);",
    "Assert.DoesNotContain(rawTask, rawJsonText, StringComparison.Ordinal);",
    "Assert.DoesNotContain(rawTask, debugTraceText, StringComparison.Ordinal);"
)
$q8OrchestrationMissingControlSnippets = @($q8OrchestrationControlPathRequiredSnippets | Where-Object { -not $interactiveTestExecutableText.Contains($_) })
$q8OrchestrationEvidence = $q8OrchestrationMissingText.Count -eq 0 -and $q8OrchestrationMissingControlSnippets.Count -eq 0
Add-Check "q8_02_orchestration_reasoning_semantic_quality_wpf_scenario_exists" $q8OrchestrationEvidence "Q8-02 WPF dashboard automation scenario covers OrchestrationReasoning human API name, structured-task input, selected family/scenario refs/gates, implementation-not-authorized warnings, raw/debug redaction, malformed/untrained honesty, and dashboard-only evidence." ("missing text: $($q8OrchestrationMissingText -join ', '); missing control snippets: $($q8OrchestrationMissingControlSnippets -join ' | ')")

$q8SkillSelectionScenarioRequiredText = @(
    "ApiDashboardWindow_SkillSelectionSemanticQualityScenario_DrivesControlsAndShowsHonestDashboardEvidence",
    "dashboard-semantic-quality-skill-selection-1",
    "Task: plan-review dashboard QA; orchestration plan: review gates and docs sync; available skills: ui-testing, test-integrity, implementation-integrity, output-readability-gate, documentation-sync, git-automation.",
    "Missing SkillSelection inventory",
    "XiigenExtensionApiContract.SkillSelectionMethod",
    "Select relevant skills for a task",
    "Chosen skills: ui-testing, test-integrity, implementation-integrity, output-readability-gate, documentation-sync",
    "Rejected tempting skills: git-automation because commit is forbidden",
    "external-ai-tooling because external AI is forbidden",
    "Candidate-position scaffold warning",
    "candidate position is not final semantic authority",
    "Candidate-position scaffold is not success proof",
    "candidate_position_scaffold_not_final_authority",
    "candidate_position_final_authority",
    "candidate_position_success_proof_claimed",
    "redacted_task_plan_ref:skill-selection-q8-02",
    "output_fields:chosen_skills,rejected_tempting_skills,candidate_position_scaffold_warning,candidate_position_final_authority=false,candidate_position_success_proof_claimed=false",
    "missing_skill_inventory_visible",
    "SKILL_SELECTION_INVENTORY_NOT_READY_FOR_SEMANTIC_QUALITY",
    "raw_private_task_plan_copied_as_training_evidence",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"chosen_skills`", `"ui-testing;test-integrity;implementation-integrity;output-readability-gate;documentation-sync`");",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"rejected_tempting_skills`", `"git-automation:commit_forbidden;external-ai-tooling:external_ai_forbidden;broad-test-runner:focused_verification_only`");",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"candidate_position_scaffold_warning`", `"candidate_position_scaffold_not_final_authority`");",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"candidate_position_final_authority`", `"false`");",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"candidate_position_success_proof_claimed`", `"false`");",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"raw_private_task_plan_copied_as_training_evidence`", `"false`");",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"semantic_quality_claimed`", `"false`");",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"trained_model_proof_claimed`", `"false`");",
    "AssertQ8SemanticScenarioCheckSetValue(rawJsonText, `"host_runtime_claimed`", `"false`");"
)
$q8SkillSelectionMissingText = @($q8SkillSelectionScenarioRequiredText | Where-Object { $interactiveTestExecutableText -notmatch [regex]::Escape($_) })
$q8SkillSelectionControlPathRequiredSnippets = @(
    "SelectListBoxItem(apiList, skillSelectionApi);",
    "SetText(inputBox, exampleInput);",
    "Toggle(debugToggle);",
    "Toggle(rawJsonToggle);",
    "InvokeButton(runButton);",
    "Assert.Equal(XiigenExtensionApiContract.SkillSelectionMethod, gateway.LastRequest!.Method);",
    "Assert.Equal(exampleInput, gateway.LastRequest.UserPromptOrTaskText);",
    "AssertSkillSelectionScenarioPositiveOutput(",
    "Assert.Contains(`"Candidate-position scaffold is not success proof`", resultText, StringComparison.Ordinal);",
    "SetText(inputBox, negativeInput);",
    "Assert.Equal(negativeInput, gateway.LastRequest.UserPromptOrTaskText);",
    "AssertSkillSelectionScenarioNotReadyOutput(",
    "warning => warning.Contains(`"candidate_position_success_proof_claimed=false`", StringComparison.Ordinal)",
    "warning => warning.Contains(`"trained_model_proof_claimed=false`", StringComparison.Ordinal)",
    "warning => warning.Contains(`"host/runtime=false`", StringComparison.Ordinal)",
    "AssertQ8ScenarioHistoryAndExport(",
    "Assert.False(negativeHistory.Success);",
    "Assert.Equal(ComputeSha256(negativeInput), negativeHistory.PromptSha256);",
    "Assert.Equal(ApiDashboardRunner.DashboardEvidenceScope, negativeHistory.EvidenceScope);",
    "Assert.False(negativeHistory.HostRuntimeProofClaimed);",
    "Assert.False(negativeHistory.SemanticQualityOrTrainedModelProofClaimed);",
    "Assert.Contains(ComputeSha256(exampleInput), viewModel.LastExportText);",
    "Assert.DoesNotContain(exampleInput, viewModel.LastExportText);",
    "Assert.DoesNotContain(negativeInput, viewModel.LastExportText);",
    "Assert.DoesNotContain(rawTaskPlan, resultText, StringComparison.Ordinal);",
    "Assert.DoesNotContain(rawTaskPlan, rawJsonText, StringComparison.Ordinal);",
    "Assert.DoesNotContain(rawTaskPlan, debugTraceText, StringComparison.Ordinal);"
)
$q8SkillSelectionMissingControlSnippets = @($q8SkillSelectionControlPathRequiredSnippets | Where-Object { -not $interactiveTestExecutableText.Contains($_) })
$q8SkillSelectionEvidence = $q8SkillSelectionMissingText.Count -eq 0 -and $q8SkillSelectionMissingControlSnippets.Count -eq 0
Add-Check "q8_02_skill_selection_semantic_quality_wpf_scenario_exists" $q8SkillSelectionEvidence "Q8-02 WPF dashboard automation scenario covers SkillSelection human API name, task/plan input, chosen skills, rejected tempting skills with reasons, candidate-position scaffold warning, raw/debug redaction, missing-inventory/untrained honesty, and dashboard-only evidence." ("missing text: $($q8SkillSelectionMissingText -join ', '); missing control snippets: $($q8SkillSelectionMissingControlSnippets -join ' | ')")

$q8ScenarioHistoryExportRequiredSnippets = @(
    "Assert.Equal(expectedNegativeStatus, negativeHistory.Status);",
    "Assert.Equal(ComputeSha256(negativeInput), negativeHistory.PromptSha256);",
    "Assert.Equal(ApiDashboardRunner.DashboardEvidenceScope, negativeHistory.EvidenceScope);",
    "Assert.False(negativeHistory.HostRuntimeProofClaimed);",
    "Assert.False(negativeHistory.SemanticQualityOrTrainedModelProofClaimed);",
    "Assert.DoesNotContain(negativeInput, negativeHistory.ToString(), StringComparison.Ordinal);",
    "Assert.DoesNotContain(positiveInput, negativeHistory.ToString(), StringComparison.Ordinal);",
    "Assert.Equal(ComputeSha256(positiveInput), positiveHistory.PromptSha256);",
    "Assert.Equal(ApiDashboardRunner.DashboardEvidenceScope, positiveHistory.EvidenceScope);",
    "Assert.False(positiveHistory.HostRuntimeProofClaimed);",
    "Assert.False(positiveHistory.SemanticQualityOrTrainedModelProofClaimed);",
    "Assert.DoesNotContain(positiveInput, positiveHistory.ToString(), StringComparison.Ordinal);",
    "Assert.DoesNotContain(negativeInput, positiveHistory.ToString(), StringComparison.Ordinal);",
    "Assert.Contains(expectedMethod, viewModel.LastExportText);",
    "Assert.Contains(ApiDashboardRunner.DashboardEvidenceScope, viewModel.LastExportText);",
    "Assert.Contains(ComputeSha256(positiveInput), viewModel.LastExportText);",
    "Assert.Contains(ComputeSha256(negativeInput), viewModel.LastExportText);",
    "Assert.DoesNotContain(positiveInput, viewModel.LastExportText);",
    "Assert.DoesNotContain(negativeInput, viewModel.LastExportText);"
)
$q8ScenarioHistoryExportMissingSnippets = @($q8ScenarioHistoryExportRequiredSnippets | Where-Object { -not $interactiveTestExecutableText.Contains($_) })
$q8OrchestrationHistoryExportCall = $interactiveTestExecutableText -match 'AssertQ8ScenarioHistoryAndExport\(\s*viewModel,\s*exportButton,\s*historyList,\s*XiigenExtensionApiContract\.OrchestrationPlanMethod,\s*exampleInput,\s*negativeInput,\s*"orchestration_reasoning_not_ready_degraded_dashboard_scenario"\)'
$q8SkillSelectionHistoryExportCall = $interactiveTestExecutableText -match 'AssertQ8ScenarioHistoryAndExport\(\s*viewModel,\s*exportButton,\s*historyList,\s*XiigenExtensionApiContract\.SkillSelectionMethod,\s*exampleInput,\s*negativeInput,\s*"skill_selection_inventory_missing_degraded_dashboard_scenario"\)'
$q8ScenarioHistoryExportEvidence = $q8ScenarioHistoryExportMissingSnippets.Count -eq 0 -and $q8OrchestrationHistoryExportCall -and $q8SkillSelectionHistoryExportCall
Add-Check "q8_02_history_export_privacy_assertions_exist" $q8ScenarioHistoryExportEvidence "Q8-02 WPF automation keeps both OrchestrationReasoning and SkillSelection wired to the shared history/export assertions for prompt hashes, evidence scope, false proof flags, and raw input omission across positive and negative branches." ("missing snippets: $($q8ScenarioHistoryExportMissingSnippets -join ' | '); orchestration call: $q8OrchestrationHistoryExportCall; skill selection call: $q8SkillSelectionHistoryExportCall")

$viewModelTestsExist = Test-Path -LiteralPath (Join-RepoPath $viewModelTestFile)
if ($viewModelTestsExist) {
    $viewModelTestText = Read-RepoText $viewModelTestFile
    $hasCommandPathTests = $viewModelTestText -match "SelectsApiUsesExampleRunsAndShowsResult" -and $viewModelTestText -match "RawJsonToggleUsesSameRunCommandPath" -and $viewModelTestText -match "CopyAndExportAreShellCommandsOverSafeViewState"
    Add-Check "view_model_tests_exist" $hasCommandPathTests "View-model command-path automation tests exist." $viewModelTestFile
} else {
    Add-Check "view_model_tests_exist" $false "View-model command-path automation test file is missing." $viewModelTestFile
}

$testProjectText = Read-RepoText $testProjectFile
$hasDashboardProjectReference = $testProjectText -match "<ProjectReference Include=`"..\\43\.SmartSkillsDomain\.Dashboard\\43\.SmartSkillsDomain\.Dashboard\.csproj`""
$hasDashboardCompileLink = $testProjectText -match "<Compile\s+Include=`"..\\43\.SmartSkillsDomain\.Dashboard\\" -or $testProjectText -match "Link=`".*43\.SmartSkillsDomain\.Dashboard"
Add-Check "no_test_only_compile_include_substitute" ($hasDashboardProjectReference -and -not $hasDashboardCompileLink) "Tests use the real dashboard project reference, not a Compile Include/source-link substitute." $testProjectFile

$contractText = Read-RepoText $contractFile
$methodMatches = [regex]::Matches($contractText, 'public const string (\w+Method) = "([^"]+)";')
$methodConstants = @($methodMatches | ForEach-Object { $_.Groups[1].Value })
$methodIds = @($methodMatches | ForEach-Object { $_.Groups[2].Value })
$catalogText = Read-RepoText $catalogFile
$viewModelText = Read-RepoText $viewModelFile
$missingCatalogRefs = @($methodConstants | Where-Object { $catalogText -notmatch "\[XiigenExtensionApiContract\.$_\]" })
$humanNameCount = ([regex]::Matches($catalogText, "HumanName:\s*`"[^`"]+`"")).Count
$exampleCount = ([regex]::Matches($catalogText, "ExampleInputText:\s*`"[^`"]+`"")).Count
$hasCatalogHumanData = $missingCatalogRefs.Count -eq 0 -and $humanNameCount -ge $methodConstants.Count -and $exampleCount -ge $methodConstants.Count
Add-Check "catalog_rows_have_human_names_and_examples" $hasCatalogHumanData "Every contract method is represented in dashboard metadata with human names and examples." ("methods: $($methodIds -join ', '); missing refs: $($missingCatalogRefs -join ', ')")

$fixtureFilesExist = (Test-Path -LiteralPath (Join-RepoPath $p36FixturesFile)) -and (Test-Path -LiteralPath (Join-RepoPath $p36DashboardScenariosFile)) -and (Test-Path -LiteralPath (Join-RepoPath $p37HumanLabelsFile))
if ($fixtureFilesExist) {
    $fixtureRows = @(Get-Content -LiteralPath (Join-RepoPath $p36FixturesFile) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object { $_ | ConvertFrom-Json -ErrorAction Stop })
    $scenarioRows = @(Get-Content -LiteralPath (Join-RepoPath $p36DashboardScenariosFile) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object { $_ | ConvertFrom-Json -ErrorAction Stop })
    $labelRows = @(Get-Content -LiteralPath (Join-RepoPath $p37HumanLabelsFile) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object { $_ | ConvertFrom-Json -ErrorAction Stop })
    $fixtureMachines = @($fixtureRows | Group-Object machine | ForEach-Object { "$($_.Name):$($_.Count)" })
    $p39FixtureRowsOk = $fixtureRows.Count -eq 36 `
        -and $scenarioRows.Count -eq 36 `
        -and $labelRows.Count -eq 36 `
        -and @($fixtureRows | Where-Object { $_.training_allowed -ne $false -or $_.eval_execution_allowed -ne $false -or $_.not_executed_yet -ne $true -or $_.raw_private_text_copied -ne $false }).Count -eq 0 `
        -and @($scenarioRows | Where-Object { $_.evidence_scope -ne "fixture-design-only-not-runtime-quality" }).Count -eq 0
    Add-Check "p39_fixture_source_rows_are_available_and_safe" $p39FixtureRowsOk "P36/P37 fixture source rows are present, parseable, counted, and marked as non-training/non-eval fixture evidence." ("fixtures: $($fixtureRows.Count); scenarios: $($scenarioRows.Count); labels: $($labelRows.Count); machines: $($fixtureMachines -join ', ')")
} else {
    Add-Check "p39_fixture_source_rows_are_available_and_safe" $false "P36/P37 fixture source files are missing." "$p36FixturesFile; $p36DashboardScenariosFile; $p37HumanLabelsFile"
}

$catalogHasFixtureProvider = $catalogText -match "SemanticQualityFixtureCatalogProvider" `
    -and $catalogText -match "ApiDashboardFixtureExampleRow" `
    -and $catalogText -match "BuildFixtureExamplesByMethod" `
    -and $catalogText -match "Fixture preview only" `
    -and $catalogText -match "ClaimsSemanticQuality: false" `
    -and $catalogText -match "ClaimsRuntimeExtensionProof: false"
Add-Check "p39_catalog_attaches_fixture_examples" $catalogHasFixtureProvider "Dashboard catalog has a P39 fixture provider that attaches P36/P37 examples without semantic-quality or runtime claims." $catalogFile

$requestBuilderText = Read-RepoText "43.SmartSkillsDomain.Dashboard/ApiDashboardRequestBuilder.cs"
$requestBuilderHasFixtureBoundary = $requestBuilderText -match "ApiDashboardFixtureExampleRow selectedFixture" `
    -and $requestBuilderText -match "API_DASHBOARD_FIXTURE_METHOD_MISMATCH" `
    -and $requestBuilderText -match "\[`"fixture_preview_only`"\].*`"true`"" `
    -and $requestBuilderText -match "\[`"training_allowed`"\]" `
    -and $requestBuilderText -match "\[`"eval_execution_allowed`"\]" `
    -and $requestBuilderText -match "\[`"raw_private_text_copied`"\]"
Add-Check "p39_request_builder_carries_fixture_boundary" $requestBuilderHasFixtureBoundary "Request builder can create fixture-preview requests with fixture ids and false training/eval/proof flags." "43.SmartSkillsDomain.Dashboard/ApiDashboardRequestBuilder.cs"

$viewModelHasFixtureSelection = $viewModelText -match "SelectedFixtureExamples" `
    -and $viewModelText -match "SelectedFixtureExpectedResult" `
    -and $viewModelText -match "ShouldRunSelectedFixture" `
    -and $viewModelText -match "api-dashboard-fixture-ui"
Add-Check "p39_view_model_can_select_fixture_examples" $viewModelHasFixtureSelection "View-model exposes fixture examples, expected result text, and fixture-aware run path." $viewModelFile

$p39UnitTestsExist = $catalogText -match "SemanticQualityFixtureCatalogProvider" `
    -and (Read-RepoText "Tests/ApiDashboardCatalogTests.cs") -match "SemanticQualityFixtureCatalogProvider_BuildFixtureExamples_LoadsP36FixtureRowsForThreeApiMethods" `
    -and (Read-RepoText "Tests/ApiDashboardRequestBuilderTests.cs") -match "ApiDashboardRequestBuilder_BuildRequest_FromFixtureExampleAddsSafeFixtureBoundaryFields" `
    -and (Read-RepoText "Tests/ApiDashboardViewModelTests.cs") -match "ApiDashboardViewModel_SelectedFixtureExamplePopulatesInputAndRunsFixtureRequest" `
    -and (Read-RepoText "Tests/ApiDashboardRunnerTests.cs") -match "ApiDashboardRunner_Run_FixtureExampleCallsGatewayWithFixturePreviewBoundary"
Add-Check "p39_unit_and_handler_tests_exist" $p39UnitTestsExist "P39 unit/handler tests cover fixture loading, request metadata, view-model run path, and runner boundary flags." "Tests/ApiDashboard*Tests.cs"

$fixtureRunnerExists = Test-Path -LiteralPath (Join-RepoPath $fixtureRunnerFile)
$fixtureRunnerText = if ($fixtureRunnerExists) { Read-RepoText $fixtureRunnerFile } else { "" }
$fixtureRunnerHasGuardSurface = $fixtureRunnerText -match "ApiDashboardFixtureRunner" `
    -and $fixtureRunnerText -match "GuardFixture" `
    -and $fixtureRunnerText -match "GuardFixtureWithAuthorizationManifestPath" `
    -and $fixtureRunnerText -match "SemanticQualityFixtureCatalogProvider.BuildFixtureExamples" `
    -and $fixtureRunnerText -match "p37-readiness-gap-ledger.jsonl" `
    -and $fixtureRunnerText -match "No eval authorization manifest" `
    -and $fixtureRunnerText -match "authorization_metadata_accepted"
Add-Check "p40_fixture_runner_guard_source_exists" $fixtureRunnerHasGuardSurface "P40 dashboard fixture runner guard source exists, loads P36/P37 evidence, accepts object/path authorization, and has an explicit refusal path." $fixtureRunnerFile

$fixtureRunnerKeepsClaimsFalse = $fixtureRunnerText -match "TrainingExecuted: false" `
    -and $fixtureRunnerText -match "EvalExecuted: false" `
    -and $fixtureRunnerText -match "CheckpointMutated: false" `
    -and $fixtureRunnerText -match "SemanticQualityClaimed: false" `
    -and $fixtureRunnerText -match "HostRuntimeProofClaimed: false" `
    -and $fixtureRunnerText -match "ExternalAiUsed: false" `
    -and $fixtureRunnerText -match "P40 still stops before eval/model execution"
Add-Check "p40_fixture_runner_guard_keeps_execution_and_proof_flags_false" $fixtureRunnerKeepsClaimsFalse "P40 guard keeps training, eval, checkpoint, semantic-quality, host-runtime, and external-AI execution/proof flags false even when authorization metadata is accepted." $fixtureRunnerFile

$fixtureRunnerTestsText = if (Test-Path -LiteralPath (Join-RepoPath "Tests/ApiDashboardFixtureRunnerTests.cs")) { Read-RepoText "Tests/ApiDashboardFixtureRunnerTests.cs" } else { "" }
$fixtureRunnerTestsExist = $fixtureRunnerTestsText -match "LoadsKnownP36FixtureAndRefusesWithoutAuthorization" `
    -and $fixtureRunnerTestsText -match "MissingFixtureReturnsReadableFailure" `
    -and $fixtureRunnerTestsText -match "MalformedAuthorizationRefuses" `
    -and $fixtureRunnerTestsText -match "NonAllowingAuthorizationRefuses" `
    -and $fixtureRunnerTestsText -match "ExplicitAllowChangesOnlyAuthorizationMetadata" `
    -and $fixtureRunnerTestsText -match "ExplicitAllowPathStillDoesNotRunEval" `
    -and $fixtureRunnerTestsText -match "AssertNoExecutionOrProofClaims"
Add-Check "p40_fixture_runner_guard_tests_exist" $fixtureRunnerTestsExist "P40 tests cover known fixture refusal, missing fixture failure, malformed/non-allowing authorization, explicit allow metadata, and no execution/proof claims." "Tests/ApiDashboardFixtureRunnerTests.cs"

$sourceDisjointGuardExists = Test-Path -LiteralPath (Join-RepoPath $sourceDisjointGuardFile)
$sourceDisjointGuardText = if ($sourceDisjointGuardExists) { Read-RepoText $sourceDisjointGuardFile } else { "" }
$sourceDisjointGuardHasSurface = $sourceDisjointGuardText -match "ApiDashboardSourceDisjointGuard" `
    -and $sourceDisjointGuardText -match "VerifyP36FixtureRows" `
    -and $sourceDisjointGuardText -match "p36-source-disjoint-manifest.json" `
    -and $sourceDisjointGuardText -match "p36-eval-fixtures.jsonl" `
    -and $sourceDisjointGuardText -match "DuplicateFixtureIds" `
    -and $sourceDisjointGuardText -match "MissingManifestFamilies" `
    -and $sourceDisjointGuardText -match "UnexpectedManifestFamilies" `
    -and $sourceDisjointGuardText -match "FamiliesBothTrainAndEval" `
    -and $sourceDisjointGuardText -match "CanProceedToEvalRunner: false" `
    -and $sourceDisjointGuardText -match "parse-only source-disjoint pre-eval guard"
Add-Check "p41_source_disjoint_guard_source_exists" $sourceDisjointGuardHasSurface "P41 source-disjoint guard source reads the P36 manifest and fixtures, reports duplicate ids, family mismatches, train/eval overlap, and never authorizes eval." $sourceDisjointGuardFile

$sourceDisjointGuardKeepsClaimsFalse = $sourceDisjointGuardText -match "TrainingRun" `
    -and $sourceDisjointGuardText -match "EvalRun" `
    -and $sourceDisjointGuardText -match "CheckpointMutated" `
    -and $sourceDisjointGuardText -match "SemanticQualityClaimed" `
    -and $sourceDisjointGuardText -match "CanProceedToEvalRunner: false" `
    -and $sourceDisjointGuardText -match "no model eval, no training, no checkpoint mutation, no semantic-quality proof"
Add-Check "p41_source_disjoint_guard_keeps_execution_and_proof_flags_false" $sourceDisjointGuardKeepsClaimsFalse "P41 guard reports proof flags and keeps eval authorization closed in this parse-only slice." $sourceDisjointGuardFile

$sourceDisjointGuardTestsText = if (Test-Path -LiteralPath (Join-RepoPath "Tests/ApiDashboardSourceDisjointGuardTests.cs")) { Read-RepoText "Tests/ApiDashboardSourceDisjointGuardTests.cs" } else { "" }
$sourceDisjointGuardTestsExist = $sourceDisjointGuardTestsText -match "PassesCurrentP36ButDoesNotAuthorizeEval" `
    -and $sourceDisjointGuardTestsText -match "DetectsDuplicateFixtureIds" `
    -and $sourceDisjointGuardTestsText -match "DetectsMissingAndUnexpectedFamilyMismatch" `
    -and $sourceDisjointGuardTestsText -match "DetectsTrainEvalOverlap" `
    -and $sourceDisjointGuardTestsText -match "DoesNotTurnProofFlagsTrue" `
    -and $sourceDisjointGuardTestsText -match "AssertNoProofFlags"
Add-Check "p41_source_disjoint_guard_tests_exist" $sourceDisjointGuardTestsExist "P41 tests cover the current P36 pass, duplicate fixture ids, missing/extra family mismatch, train/eval overlap, and no proof flags." "Tests/ApiDashboardSourceDisjointGuardTests.cs"

$humanNames = @([regex]::Matches($catalogText, "HumanName:\s*`"([^`"]+)`"") | ForEach-Object { $_.Groups[1].Value })
$exampleTexts = @([regex]::Matches($catalogText, "ExampleInputText:\s*`"([^`"]+)`"") | ForEach-Object { $_.Groups[1].Value })

$dashboardDir = Join-RepoPath "43.SmartSkillsDomain.Dashboard"
$dashboardSourceText = (Get-ChildItem -LiteralPath $dashboardDir -Recurse -File |
    Where-Object { $_.Extension -in ".cs", ".xaml" } |
    ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw }) -join [Environment]::NewLine
$bannedClaimPatterns = @(
    'ClaimsExternalAi:\s*true',
    'ClaimsRuntimeExtensionProof:\s*true',
    'ExternalAiUsed:\s*true',
    'ScriptParityUsed:\s*true',
    'HostRuntimeProofClaimed:\s*true',
    'RuntimeExtensionProofClaimed:\s*true',
    'SemanticQualityOrTrainedModelProofClaimed:\s*true',
    '\["external_ai"\]\s*=\s*"true"',
    '\["host_runtime_claimed"\]\s*=\s*"true"',
    '\["runtime_extension_claimed"\]\s*=\s*"true"',
    '\["semantic_quality_claimed"\]\s*=\s*"true"',
    '\["trained_model_proof_claimed"\]\s*=\s*"true"'
)
$matchedBannedClaims = @($bannedClaimPatterns | Where-Object { $dashboardSourceText -match $_ })
Add-Check "no_external_ai_or_host_runtime_claims" ($matchedBannedClaims.Count -eq 0) "Dashboard source does not claim external AI, script parity, host runtime, runtime extension proof, semantic quality, or trained-model proof." ($matchedBannedClaims -join "; ")

$p42AllowedExamples = @(
    "not semantic quality proof",
    "not host runtime proof",
    "checkpoint rewrite is not complete in dashboard smoke",
    "fresh-load remains a future gate, not dashboard evidence"
)
$p42RejectedExamples = @(
    "semantic quality proven by dashboard smoke",
    "Claude host runtime proven by dashboard",
    "trained model readiness proven by preview",
    "checkpoint rewrite complete after smoke test"
)
$p42AllowedExampleViolations = @($p42AllowedExamples | ForEach-Object { Get-P42OverclaimViolations "p42-allowed-example" $_ })
$p42RejectedExampleMisses = @($p42RejectedExamples | Where-Object { @(Get-P42OverclaimViolations "p42-rejected-example" $_).Count -eq 0 })
Add-Check "p42_overclaim_firewall_examples_pass" ($p42AllowedExampleViolations.Count -eq 0 -and $p42RejectedExampleMisses.Count -eq 0) "P42 overclaim firewall allows negative wording and rejects positive dashboard/preview/smoke readiness claims." ("allowed violations: $($p42AllowedExampleViolations -join ' | '); missed rejects: $($p42RejectedExampleMisses -join ' | ')")

$p42ScanTargets = New-Object System.Collections.Generic.List[string]
$p42ScanTargets.Add($dashboardDocFile)
$p42ScanTargets.Add($p37ReadinessGapLedgerFile)
Get-ChildItem -LiteralPath (Join-RepoPath "43.SmartSkillsDomain.Dashboard") -Recurse -File |
    Where-Object { $_.Extension -in ".cs", ".xaml" } |
    ForEach-Object { $p42ScanTargets.Add($_.FullName.Substring($repoRoot.Length + 1).Replace("\", "/")) }
if (Test-Path -LiteralPath (Join-RepoPath $p42ArtifactDir)) {
    Get-ChildItem -LiteralPath (Join-RepoPath $p42ArtifactDir) -Recurse -File |
        Where-Object { $_.Extension -in ".md", ".json", ".jsonl" } |
        ForEach-Object { $p42ScanTargets.Add($_.FullName.Substring($repoRoot.Length + 1).Replace("\", "/")) }
}
$p42OverclaimViolations = @($p42ScanTargets |
    Sort-Object -Unique |
    Where-Object { Test-Path -LiteralPath (Join-RepoPath $_) } |
    ForEach-Object { Get-P42OverclaimViolations $_ (Read-RepoText $_) })
Add-Check "p42_dashboard_overclaim_firewall_scans_docs_source_and_artifacts" ($p42OverclaimViolations.Count -eq 0) "P42 overclaim firewall scans dashboard docs, dashboard source, P37 gap ledger, and P42 report-facing artifacts for positive readiness overclaims." ($p42OverclaimViolations -join " | ")

$overclaimTestsText = if (Test-Path -LiteralPath (Join-RepoPath $overclaimTestFile)) { Read-RepoText $overclaimTestFile } else { "" }
$overclaimTestsExist = $overclaimTestsText -match "AllowsNegativeWordingExamples" `
    -and $overclaimTestsText -match "RejectsPositiveOverclaimExamples" `
    -and $overclaimTestsText -match "ScansDashboardDocsSourceAndP42Artifacts" `
    -and $overclaimTestsText -match "semantic quality proven by dashboard smoke" `
    -and $overclaimTestsText -match "Claude host runtime proven by dashboard"
Add-Check "p42_overclaim_firewall_tests_exist" $overclaimTestsExist "P42 tests cover positive and negative examples plus real docs/source/artifact scan." $overclaimTestFile

$runnerText = Read-RepoText $runnerFile
$viewModelText = Read-RepoText $viewModelFile
$evidenceScopeOk = $runnerText -match "dashboard-to-core invocation only; not extension host runtime proof" -and $viewModelText -match "Dashboard-to-core evidence only; host runtime and semantic quality proof stay separate"
Add-Check "evidence_scope_dashboard_to_core_only" $evidenceScopeOk "Evidence scope is dashboard-to-core only and does not upgrade to host runtime proof." "$runnerFile; $viewModelFile"

$docExists = Test-Path -LiteralPath (Join-RepoPath $dashboardDocFile)
if ($docExists) {
    $docText = Read-RepoText $dashboardDocFile
    $docHasRequiredScope = $docText -match "dashboard-to-core" -and $docText -match "WO-5 view-model/handler evidence was an intermediate" -and $docText -match "Tests.ApiDashboardWpf/ApiDashboardWindowInteractiveTests.cs" -and $docText -match "WO-6 adds dashboard-scoped WPF window/control automation" -and $docText -match "QA UI/UX automation" -and $docText -match "Claude, Cursor, or Codex host" -and $docText -match "external AI is not used"
    Add-Check "dashboard_docs_capture_evidence_boundaries" $docHasRequiredScope "Dashboard documentation records WO-5 intermediate scope, real WO-6 WPF-only test evidence, host runtime boundary, and external AI boundary." $dashboardDocFile

    $staleFullWpfNotClaimedPhrase = "full interactive WPF automation " + "is not claimed"
    $staleFullWpfRemainsPhrase = "full interactive WPF automation " + "remains not claimed"
    $staleFullWpfFinalStatus = $docText -match [regex]::Escape($staleFullWpfNotClaimedPhrase) -or $docText -match [regex]::Escape($staleFullWpfRemainsPhrase)
    Add-Check "dashboard_docs_no_stale_full_wpf_current_status" (-not $staleFullWpfFinalStatus) "Dashboard documentation no longer keeps the stale WO-5 full WPF automation non-claim as current status." $dashboardDocFile

    $docRequiredSections = @(
        "Importable UML",
        "UML Connection Simulations",
        "No-Code Algorithms",
        "Xiigen Extension API Methods Exposed In The Dashboard",
        "Important Code References",
        "QA And Evidence Mapping"
    )
    $missingDocSections = @($docRequiredSections | Where-Object { $docText -notmatch [regex]::Escape($_) })
    Add-Check "dashboard_docs_have_required_user_sections" ($missingDocSections.Count -eq 0) "Dashboard documentation contains UML, simulations, no-code algorithms, API examples, code references, and QA mapping." ("missing sections: $($missingDocSections -join ', ')")

    $missingHumanNamesInDoc = @($humanNames | Where-Object { $docText -notmatch [regex]::Escape($_) })
    $missingExamplesInDoc = @($exampleTexts | Where-Object { $docText -notmatch [regex]::Escape($_) })
    Add-Check "dashboard_docs_cover_catalog_names_and_examples" ($missingHumanNamesInDoc.Count -eq 0 -and $missingExamplesInDoc.Count -eq 0) "Dashboard documentation covers catalog human names and example inputs." ("missing human names: $($missingHumanNamesInDoc -join ', '); missing examples: $($missingExamplesInDoc -join ' | ')")

    $mojibakeMarkers = @([char]0x00D0, [char]0x00D1, [char]0x00C2, [char]0x00E2)
    $docMojibakeMatches = @($mojibakeMarkers | Where-Object { $docText.Contains($_) } | ForEach-Object { [string]$_ })
    Add-Check "dashboard_docs_no_mojibake" ($docMojibakeMatches.Count -eq 0) "Dashboard documentation has no broken encoding markers." ("matches: $($docMojibakeMatches -join ', ')")

    $q8DocRequiredText = @(
        "PromptToTask Semantic-Quality Dashboard Scenario",
        "Understand a request as a task",
        '`RecordingGateway` fixture',
        'does not call `PromptToTaskMachine.Translate`',
        "Write a plan for improving skill selection; do not implement.",
        "implementation_authorized=false",
        "Missing or untrained checkpoint",
        "raw/debug",
        "redacted input ref",
        "dashboard_api_only_not_model_quality_not_host_runtime",
        "semantic quality",
        "host runtime"
    )
    $q8DocMissingText = @($q8DocRequiredText | Where-Object { $docText -notmatch [regex]::Escape($_) })
    Add-Check "q8_01_prompt_to_task_dashboard_doc_exists" ($q8DocMissingText.Count -eq 0) "Dashboard documentation contains the human-readable Q8-01 PromptToTask scenario, expected result, negative case, raw/debug boundary, and evidence scope." ("missing text: $($q8DocMissingText -join ', ')")

    $q8OrchestrationDocRequiredText = @(
        "OrchestrationReasoning Semantic-Quality Dashboard Scenario",
        "dashboard/API fixture evidence",
        "Plan the work steps from a task",
        '`RecordingGateway` fixture',
        'does not call `OrchestrationReasoningMachine.Plan`',
        "Structured task: plan_review_or_plan_repair; gates: plan review, dashboard QA, documentation sync; implementation_authorized=false.",
        "selected orchestration family",
        "required gates",
        "implementation is not authorized",
        "Malformed OrchestrationReasoning task input",
        "redacted task ref",
        "dashboard_api_only_not_model_quality_not_host_runtime",
        "semantic quality",
        "host runtime"
    )
    $q8OrchestrationDocMissingText = @($q8OrchestrationDocRequiredText | Where-Object { $docText -notmatch [regex]::Escape($_) })
    Add-Check "q8_02_orchestration_reasoning_dashboard_doc_exists" ($q8OrchestrationDocMissingText.Count -eq 0) "Dashboard documentation contains the human-readable Q8-02 OrchestrationReasoning scenario, expected result, negative case, raw/debug boundary, and evidence scope." ("missing text: $($q8OrchestrationDocMissingText -join ', ')")

    $q8SkillSelectionDocRequiredText = @(
        "SkillSelection Semantic-Quality Dashboard Scenario",
        "dashboard/API fixture evidence",
        "Choose the right skills for the task",
        '`RecordingGateway` fixture',
        'does not call `SkillSelectionMachine.SelectSkills`',
        "Task: plan-review dashboard QA; orchestration plan: review gates and docs sync; available skills: ui-testing, test-integrity, implementation-integrity, output-readability-gate, documentation-sync, git-automation.",
        "chosen skills",
        "rejected tempting skills with reasons",
        "candidate-position scaffold warning",
        "no claim that candidate order is final authority",
        "Missing SkillSelection inventory",
        "redacted task/plan ref",
        "dashboard_api_only_not_model_quality_not_host_runtime",
        "semantic quality",
        "host runtime"
    )
    $q8SkillSelectionDocMissingText = @($q8SkillSelectionDocRequiredText | Where-Object { $docText -notmatch [regex]::Escape($_) })
    Add-Check "q8_02_skill_selection_dashboard_doc_exists" ($q8SkillSelectionDocMissingText.Count -eq 0) "Dashboard documentation contains the human-readable Q8-02 SkillSelection scenario, expected result, negative case, raw/debug boundary, and evidence scope." ("missing text: $($q8SkillSelectionDocMissingText -join ', ')")

    $p39DocRequiredText = @(
        "P39 Fixture Catalog/API Surface",
        "fixture catalog",
        "36 fixture examples",
        "12 examples to each primary API row",
        "P36-PROMPTTOTASK-04",
        "P36-ORCHESTRATIONREASONING-01",
        "P36-SKILLSELECTION-01",
        "fixture id, scenario id, held-out source family",
        "no model eval",
        "no training",
        "no checkpoint mutation",
        "no semantic-quality proof",
        "ApiDashboardRequestBuilder.BuildRequest(ApiDashboardCatalogRow selectedRow, ApiDashboardFixtureExampleRow selectedFixture",
        "SemanticQualityFixtureCatalogProvider"
    )
    $p39DocMissingText = @($p39DocRequiredText | Where-Object { $docText -notmatch [regex]::Escape($_) })
    Add-Check "p39_fixture_catalog_dashboard_doc_exists" ($p39DocMissingText.Count -eq 0) "Dashboard documentation explains P39 fixture catalog/API surface, examples, request metadata, tests, and evidence boundaries." ("missing text: $($p39DocMissingText -join ', ')")

    $p40DocRequiredText = @(
        "P40 Fixture Runner Authorization Guard",
        "fixture runner guard",
        "ApiDashboardFixtureRunner.GuardFixture",
        "ApiDashboardFixtureRunnerAuthorizationManifest",
        "semantic-quality-fixture-eval",
        "authorization metadata",
        "does not run model evaluation",
        "does not train",
        "does not mutate checkpoints",
        "does not prove semantic quality",
        "does not prove host runtime",
        "P37-GAP-02"
    )
    $p40DocMissingText = @($p40DocRequiredText | Where-Object { $docText -notmatch [regex]::Escape($_) })
    Add-Check "p40_fixture_runner_guard_dashboard_doc_exists" ($p40DocMissingText.Count -eq 0) "Dashboard documentation explains the P40 fixture runner guard, authorization manifest, examples, and evidence boundaries." ("missing text: $($p40DocMissingText -join ', ')")

    $p41DocRequiredText = @(
        "P41 Source-Disjoint Pre-Eval Guard",
        "source-disjoint guard",
        "ApiDashboardSourceDisjointGuard.VerifyP36FixtureRows",
        "p36-source-disjoint-manifest.json",
        "p36-eval-fixtures.jsonl",
        "duplicate fixture ids",
        "missing manifest families",
        "unexpected manifest families",
        "families that appear in both train and eval roles",
        "does not authorize eval",
        "does not run model evaluation",
        "does not train",
        "does not mutate checkpoints",
        "does not prove semantic quality",
        "P37-GAP-03"
    )
    $p41DocMissingText = @($p41DocRequiredText | Where-Object { $docText -notmatch [regex]::Escape($_) })
    Add-Check "p41_source_disjoint_guard_dashboard_doc_exists" ($p41DocMissingText.Count -eq 0) "Dashboard documentation explains the P41 source-disjoint pre-eval guard, examples, and evidence boundaries." ("missing text: $($p41DocMissingText -join ', ')")

    $p42DocRequiredText = @(
        "P42 adds the dashboard overclaim firewall",
        "preview, smoke, documentation, and report-facing wording",
        "dashboard documentation, dashboard source strings, the P37 readiness gap ledger, and P42 artifact text",
        "not semantic quality proof",
        "not host runtime proof",
        "semantic quality proven by dashboard smoke",
        "Claude host runtime proven by dashboard",
        "Reject Dashboard Overclaim Wording",
        "ApiDashboardOverclaimFirewallTests.cs",
        "P42 dashboard overclaim firewall",
        "does not run model evaluation",
        "does not train",
        "does not mutate checkpoints",
        "does not prove semantic quality",
        "does not prove host runtime"
    )
    $p42DocMissingText = @($p42DocRequiredText | Where-Object { $docText -notmatch [regex]::Escape($_) })
    Add-Check "p42_dashboard_overclaim_firewall_doc_exists" ($p42DocMissingText.Count -eq 0) "Dashboard documentation explains the P42 overclaim firewall, examples, scan scope, and evidence boundaries." ("missing text: $($p42DocMissingText -join ', ')")
} else {
    Add-Check "dashboard_docs_capture_evidence_boundaries" $false "Dashboard documentation file is missing." $dashboardDocFile
    Add-Check "dashboard_docs_no_stale_full_wpf_current_status" $false "Dashboard documentation file is missing." $dashboardDocFile
    Add-Check "dashboard_docs_have_required_user_sections" $false "Dashboard documentation file is missing." $dashboardDocFile
    Add-Check "dashboard_docs_cover_catalog_names_and_examples" $false "Dashboard documentation file is missing." $dashboardDocFile
    Add-Check "dashboard_docs_no_mojibake" $false "Dashboard documentation file is missing." $dashboardDocFile
    Add-Check "q8_01_prompt_to_task_dashboard_doc_exists" $false "Dashboard documentation file is missing." $dashboardDocFile
    Add-Check "q8_02_orchestration_reasoning_dashboard_doc_exists" $false "Dashboard documentation file is missing." $dashboardDocFile
    Add-Check "q8_02_skill_selection_dashboard_doc_exists" $false "Dashboard documentation file is missing." $dashboardDocFile
    Add-Check "p39_fixture_catalog_dashboard_doc_exists" $false "Dashboard documentation file is missing." $dashboardDocFile
    Add-Check "p40_fixture_runner_guard_dashboard_doc_exists" $false "Dashboard documentation file is missing." $dashboardDocFile
    Add-Check "p41_source_disjoint_guard_dashboard_doc_exists" $false "Dashboard documentation file is missing." $dashboardDocFile
    Add-Check "p42_dashboard_overclaim_firewall_doc_exists" $false "Dashboard documentation file is missing." $dashboardDocFile
}

if (Test-Path -LiteralPath (Join-RepoPath $q8ScopeReviewFile)) {
    try {
        $q8ScopeReview = Read-RepoText $q8ScopeReviewFile | ConvertFrom-Json -ErrorAction Stop
        $q8ScopeScenarioIds = @($q8ScopeReview.required_scenario_ids)
        $q8ScopeChecks = $q8ScopeReview.checks
        $q8ScopeReviewOk = $q8ScopeReview.work_order -eq "Q8-03_DASHBOARD_QA_SCOPE_REVIEW" `
            -and $q8ScopeReview.status -eq "PASS" `
            -and $q8ScopeReview.evidence_scope -eq "dashboard_api_only_not_model_quality_not_host_runtime" `
            -and $q8ScopeScenarioIds.Count -eq 3 `
            -and $q8ScopeScenarioIds -contains "dashboard-semantic-quality-prompt-to-task-1" `
            -and $q8ScopeScenarioIds -contains "dashboard-semantic-quality-orchestration-1" `
            -and $q8ScopeScenarioIds -contains "dashboard-semantic-quality-skill-selection-1" `
            -and $q8ScopeChecks.all_required_scenario_ids_automated -eq $true `
            -and $q8ScopeChecks.real_wpf_control_automation_used -eq $true `
            -and $q8ScopeChecks.qa_script_checks_q8_01_executable_test_text -eq $true `
            -and $q8ScopeChecks.qa_script_checks_q8_02_executable_test_text -eq $true `
            -and $q8ScopeChecks.qa_script_checks_dashboard_docs -eq $true `
            -and $q8ScopeChecks.dashboard_evidence_used_as_model_quality_proof -eq $false `
            -and $q8ScopeChecks.dashboard_evidence_used_as_host_runtime_proof -eq $false `
            -and $q8ScopeChecks.semantic_quality_claimed -eq $false `
            -and $q8ScopeChecks.trained_model_proof_claimed -eq $false `
            -and $q8ScopeChecks.host_runtime_claimed -eq $false `
            -and $q8ScopeChecks.runtime_extension_claimed -eq $false `
            -and $q8ScopeChecks.external_ai -eq $false
        $q8ScopeReviewEvidence = "status: $($q8ScopeReview.status); scenario ids: $($q8ScopeScenarioIds -join ', '); scope: $($q8ScopeReview.evidence_scope)"
    }
    catch {
        $q8ScopeReviewOk = $false
        $q8ScopeReviewEvidence = "parse error: $($_.Exception.Message)"
    }

    Add-Check "q8_03_dashboard_qa_scope_review_evidence_exists" $q8ScopeReviewOk "Q8-03 scope-review evidence is parseable, names all Q8 scenario ids, checks executable-test/docs guards, and keeps dashboard evidence out of model-quality or host-runtime proof." $q8ScopeReviewEvidence
} else {
    Add-Check "q8_03_dashboard_qa_scope_review_evidence_exists" $false "Q8-03 scope-review evidence file is missing." $q8ScopeReviewFile
}

if (Test-Path -LiteralPath (Join-RepoPath $documentationIndexFile)) {
    $indexText = Read-RepoText $documentationIndexFile
    Add-Check "project_documentation_index_lists_dashboard" ($indexText -match "43-smart-skills-domain-dashboard\.md" -and $indexText -match "43\.SmartSkillsDomain\.Dashboard") "Project documentation index lists the dashboard document." $documentationIndexFile
} else {
    Add-Check "project_documentation_index_lists_dashboard" $false "Project documentation index is missing." $documentationIndexFile
}

if (Test-Path -LiteralPath (Join-RepoPath $uiTestingSkillFile)) {
    $uiSkillText = Read-RepoText $uiTestingSkillFile
    $uiSkillHasBoundary = $uiSkillText -match "view-model automation" -and $uiSkillText -match "full interactive WPF automation" -and $uiSkillText -match "Host runtime evidence" -and $uiSkillText -match "UX automation standard" -and $uiSkillText -match "accessible names/help text" -and $uiSkillText -match "Rejected overclaim"
    Add-Check "ui_testing_skill_covers_wpf_evidence_boundaries" $uiSkillHasBoundary "UI testing skill distinguishes WPF view-model automation, full interactive WPF automation, host runtime evidence, and UX automation evidence." $uiTestingSkillFile
} else {
    Add-Check "ui_testing_skill_covers_wpf_evidence_boundaries" $false "UI testing skill file is missing." $uiTestingSkillFile
}

if (Test-Path -LiteralPath (Join-RepoPath $outputReadabilitySkillFile)) {
    $readabilitySkillText = Read-RepoText $outputReadabilitySkillFile
    $readabilitySkillHasDashboardApiGate = $readabilitySkillText -match "Dashboard And API Documentation Readability" -and $readabilitySkillText -match "Human name" -and $readabilitySkillText -match "Evidence boundary" -and $readabilitySkillText -match "view-model automation" -and $readabilitySkillText -match "UX automation evidence" -and $readabilitySkillText -match "dashboard-to-core invocation"
    Add-Check "output_readability_skill_covers_dashboard_api_docs" $readabilitySkillHasDashboardApiGate "Output readability skill covers human-readable dashboard/API docs, UX automation evidence, and evidence boundaries." $outputReadabilitySkillFile
} else {
    Add-Check "output_readability_skill_covers_dashboard_api_docs" $false "Output readability skill file is missing." $outputReadabilitySkillFile
}

$passed = -not ($script:checks | Where-Object { -not $_.passed })
$report = [pscustomobject]@{
    report = "api-dashboard-qa"
    passed = $passed
    evidence_scope = "dashboard-to-core plus dashboard-scoped WPF window/control automation and QA UI/UX automation from Tests.ApiDashboardWpf only; not Claude/Cursor/Codex host runtime proof; not external AI, semantic, or trained-model proof"
    external_ai_used = $false
    host_runtime_smoke_used = $false
    full_interactive_wpf_automation_claimed = $fullInteractiveWpfAutomationClaimed
    full_interactive_wpf_automation_scope = "dashboard-scoped ApiDashboardWindow/control path only"
    semantic_quality_claimed = $false
    trained_model_proof_claimed = $false
    checked_methods = $methodIds
    checks = $script:checks
}

$report | ConvertTo-Json -Depth 6

if (-not $passed) {
    exit 1
}

exit 0
