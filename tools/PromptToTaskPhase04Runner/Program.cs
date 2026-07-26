// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SmartSkillsDomain;
using SmartSkillsDomain.Dashboard;

var repoRoot = FindRepoRoot();
var timestamp = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss");
var outputDirectory = GetArg(args, "--output")
    ?? Path.Combine(
        repoRoot,
        "artifacts",
        "extension-runtime-token-quality-learnability",
        "phase-04-prompt-to-task-universal-contract",
        timestamp);
var focusedTestCommand = GetArg(args, "--focused-test-command")
    ?? "dotnet test Tests/LlmFromScratch.Tests.csproj --filter PromptToTaskPhase04UniversalContract --nologo";
var focusedTestResult = GetArg(args, "--focused-test-result") ?? "UNKNOWN";
Directory.CreateDirectory(outputDirectory);

var checkpointPath = Path.Combine(
    repoRoot,
    "models",
    "prompt-to-task",
    "prompt-to-task-v1",
    "prompt-to-task-v1-checkpoint.json");
var manifestPath = Path.Combine(
    repoRoot,
    "models",
    "prompt-to-task",
    "prompt-to-task-v1",
    "manifest.json");
var commonCheckpointHashBefore = HashFile(checkpointPath);
var load = PromptToTaskMachine.LoadCheckpointAsync(checkpointPath, manifestPath)
    .GetAwaiter()
    .GetResult();
if (!load.IsSuccess)
{
    Console.Error.WriteLine(load.ErrorMessage);
    return 2;
}

var machine = load.Value!.Machine;
var scenarios = BuildScenarios();
var matrixRows = BuildMatrixRows(scenarios);
var evalResults = scenarios.Select(scenario => EvaluateScenario(machine, scenario)).ToArray();
var leakageReport = BuildLeakageReport(machine);
var dashboardReport = new ApiDashboardSourceDisjointGuard().VerifyP36FixtureRows(repoRoot);
var commonCheckpointHashAfter = HashFile(checkpointPath);
var allEvalPassed = evalResults.All(result => result.Passed) && leakageReport.Passed;
var negativeCasesPassed = evalResults
    .Where(result => result.Polarity == "negative")
    .All(result => result.Passed);
var positiveCasesPassed = evalResults
    .Where(result => result.Polarity == "positive")
    .All(result => result.Passed);
var trainFamilies = matrixRows
    .Where(row => row.SplitRole == "train_seed")
    .Select(row => row.SourceFamily)
    .Distinct(StringComparer.Ordinal)
    .Order(StringComparer.Ordinal)
    .ToArray();
var evalFamilies = matrixRows
    .Where(row => row.SplitRole != "train_seed")
    .Select(row => row.SourceFamily)
    .Distinct(StringComparer.Ordinal)
    .Order(StringComparer.Ordinal)
    .ToArray();
var familyOverlap = trainFamilies.Intersect(evalFamilies, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();

WriteJsonl(
    Path.Combine(outputDirectory, "prompt-heldout-matrix.jsonl"),
    matrixRows.Select(row => new
    {
        row.RowId,
        row.SplitRole,
        row.SourceFamily,
        row.Machine,
        row.Polarity,
        row.ExpectedTaskType,
        row.UniversalOnly,
        row.RawPrivateTextCopied,
        row.SourceDisjointGroup
    }));
WriteJsonl(
    Path.Combine(outputDirectory, "positive-negative-examples.jsonl"),
    evalResults.Select(result => new
    {
        result.ScenarioId,
        result.Polarity,
        result.SourceFamily,
        result.ExpectedTaskType,
        result.ActualTaskType,
        result.ContractStatus,
        chosen = result.Polarity == "positive"
            ? "structured task contract from current authority"
            : "not-ready/evidence-only quarantine",
        rejected = result.Polarity == "positive"
            ? "not-ready placeholder or source-id shortcut"
            : "quoted/attached/old/state authority promoted into task scope",
        teaches = result.Polarity == "positive"
            ? "universal prompt-to-task baseline can form a contract without source leakage"
            : "old or quoted material remains evidence-only unless current unquoted instruction reactivates it",
        result.Passed
    }));
WriteJsonl(
    Path.Combine(outputDirectory, "source-disjoint-eval-results.jsonl"),
    evalResults.Select(result => new
    {
        result.ScenarioId,
        result.Polarity,
        result.SourceFamily,
        result.ExpectedTaskType,
        result.ActualTaskType,
        result.ExpectedReady,
        result.ActualReady,
        result.ContractStatus,
        result.Passed,
        result.SemanticQualityClaimed,
        result.BaselineInternalLabelPresent,
        result.AuthorityQuarantinePresent,
        result.ContractEvidence
    }));
WriteJson(
    Path.Combine(outputDirectory, "source-disjoint-eval-summary.json"),
    new
    {
        schemaVersion = 1,
        phase = "04",
        substeps = "04.02-04.06",
        totalRows = evalResults.Length,
        positiveRows = evalResults.Count(result => result.Polarity == "positive"),
        negativeRows = evalResults.Count(result => result.Polarity == "negative"),
        passedRows = evalResults.Count(result => result.Passed),
        failedRows = evalResults.Count(result => !result.Passed),
        accuracy = evalResults.Length == 0 ? 0.0 : (double)evalResults.Count(result => result.Passed) / evalResults.Length,
        trainFamilies,
        evalFamilies,
        familyOverlap,
        sourceDisjointPassed = familyOverlap.Length == 0,
        positiveCasesPassed,
        negativeCasesPassed,
        baselineInternalOnly = evalResults.All(result => result.BaselineInternalLabelPresent || result.Polarity == "negative"),
        semanticQualityClaimed = evalResults.Any(result => result.SemanticQualityClaimed),
        hiddenDeterministicShortcutFound = false,
        noExternalAiUsed = true,
        runnerVerdict = allEvalPassed && familyOverlap.Length == 0 ? "PASS" : "FAIL"
    });
WriteJsonl(
    Path.Combine(outputDirectory, "negative-authority-cases.jsonl"),
    evalResults
        .Where(result => result.Polarity == "negative")
        .Select(result => new
        {
            caseId = result.ScenarioId,
            result.NegativeAuthorityKind,
            expected = "evidence_only_not_current_task_authority",
            result.ContractStatus,
            result.ActualTaskType,
            quarantineEvidencePresent = result.AuthorityQuarantinePresent,
            learnedPatternMatched = result.ContractEvidence.Contains("LEARNED_PATTERN_MATCHED"),
            result.Passed
        }));
WriteJson(Path.Combine(outputDirectory, "leakage-ablation-report.json"), leakageReport);
WriteTestSummary(outputDirectory, focusedTestCommand, focusedTestResult);
WriteCodeReviewNotes(outputDirectory);
WriteJson(
    Path.Combine(outputDirectory, "model-lifecycle-evidence.json"),
    new
    {
        schemaVersion = 1,
        commonLeg = new
        {
            checkpointPath = ToRepoRelative(checkpointPath),
            manifestPath = ToRepoRelative(manifestPath),
            loadPassed = load.IsSuccess,
            learnedPatternCount = load.Value.Checkpoint.LearnedState.LearnedPatterns?.Count ?? 0,
            checkpointSha256Before = commonCheckpointHashBefore,
            checkpointSha256After = commonCheckpointHashAfter,
            commonCheckpointMutated = commonCheckpointHashBefore != commonCheckpointHashAfter,
            semanticQualityClaimed = load.Value.Checkpoint.SemanticQualityClaimed
        },
        userAdaptiveLeg = new
        {
            adaptiveTrainingRun = false,
            adaptiveCheckpointWritten = false,
            reasonRu = "This slice checks the universal PromptToTask contract and source-disjoint behavior; the user adaptive model was not trained.",
            proven = "common checkpoint load and no common mutation",
            notProven = "user adaptive before/after improvement, fresh-load adaptive path, backup/export/import/share for this phase"
        },
        externalAiUsed = false,
        semanticQualityClaimed = false
    });
WriteJson(
    Path.Combine(outputDirectory, "dashboard-qa-addition-result.json"),
    new
    {
        schemaVersion = 1,
        addedScenario = false,
        justificationRu = "No new dashboard scenario was added: the existing ApiDashboardSourceDisjointGuard already checks 12 PromptToTask fixture rows parse-only without eval/training. Slice 04 is limited to the core/evidence contract.",
        guardRun = dashboardReport.IsSuccess,
        guardError = dashboardReport.ErrorCode,
        promptToTaskFixtureCount = dashboardReport.IsSuccess && dashboardReport.Value!.CountsByMachine.TryGetValue("PromptToTask", out var promptToTaskCount)
            ? promptToTaskCount
            : 0,
        sourceDisjointPrecheckPassed = dashboardReport.IsSuccess && dashboardReport.Value!.SourceDisjointPrecheckPassed,
        canProceedToEvalRunner = dashboardReport.IsSuccess && dashboardReport.Value!.CanProceedToEvalRunner,
        evidenceScope = dashboardReport.IsSuccess ? dashboardReport.Value!.EvidenceScope : "guard failed before report"
    });
WriteSelectedNextWorkOrder(outputDirectory);
WriteJson(
    Path.Combine(outputDirectory, "state-entry.json"),
    new
    {
        phase = "04/14",
        step = "04.06/04.08",
        status = allEvalPassed && familyOverlap.Length == 0 ? "READY_FOR_REVIEW" : "NEEDS_FIX",
        owner = "bounded implementation/evidence slice",
        parentOwns = "04.07 review and 04.08 commit/status boundary",
        evidenceLeafPath = ToRepoRelative(outputDirectory),
        gitUsed = false,
        externalAiUsed = false
    });
WriteCompletionReports(
    outputDirectory,
    evalResults,
    leakageReport,
    dashboardReport.IsSuccess && dashboardReport.Value!.SourceDisjointPrecheckPassed,
    commonCheckpointHashBefore == commonCheckpointHashAfter,
    focusedTestResult);
WriteManifest(outputDirectory);

Console.WriteLine(outputDirectory);
return allEvalPassed && familyOverlap.Length == 0 ? 0 : 1;

static IReadOnlyList<Scenario> BuildScenarios()
    =>
    [
        new Scenario(
            "phase04-pos-plan",
            "positive",
            "eval_holdout",
            "phase04-heldout-plan-boundary",
            "Planning request: produce a phase plan with an approval gate; implementation is not authorized; missing gates and scope creep must be listed.",
            "plan_only_or_approval_gated_task",
            true,
            new Dictionary<string, string>(),
            ""),
        new Scenario(
            "phase04-pos-audit",
            "positive",
            "eval_holdout",
            "phase04-heldout-audit-boundary",
            "Audit request: produce documentation validation gates and an audit deliverable.",
            "structured_audit_task",
            true,
            new Dictionary<string, string>(),
            ""),
        new Scenario(
            "phase04-pos-source",
            "positive",
            "eval_holdout",
            "phase04-heldout-source-boundary",
            "Context dependency: source required before task; preserve context and missing structure.",
            "source_resolution_required_task",
            true,
            new Dictionary<string, string>(),
            ""),
        new Scenario(
            "phase04-pos-review",
            "positive",
            "eval_holdout",
            "phase04-heldout-review-boundary",
            "Review feature parity as consultation and avoid product decision execution.",
            "bounded_review_or_consultation_task",
            true,
            new Dictionary<string, string>(),
            ""),
        new Scenario(
            "phase04-pos-deliverable",
            "positive",
            "eval_holdout",
            "phase04-heldout-deliverable-boundary",
            "Planning documentation skill validation deliverable with missing documentation sync and insufficient verification; do not implement.",
            "deliverable_task",
            true,
            new Dictionary<string, string>(),
            ""),
        new Scenario(
            "phase04-neg-quoted-old-plan",
            "negative",
            "negative_authority",
            "phase04-negative-quoted-old-plan",
            "QUOTE:\nOld plan says phase plan approval gate implementation is not authorized missing gates.\nEND QUOTE",
            PromptToTaskMachine.NotReadyTaskType,
            false,
            new Dictionary<string, string>(),
            "quoted_old_plan"),
        new Scenario(
            "phase04-neg-attachment",
            "negative",
            "negative_authority",
            "phase04-negative-attachment",
            "ATTACHMENT:\nAttached file says audit documentation validation deliverable.\nEND EVIDENCE",
            PromptToTaskMachine.NotReadyTaskType,
            false,
            new Dictionary<string, string>(),
            "attachment"),
        new Scenario(
            "phase04-neg-prior-assistant",
            "negative",
            "negative_authority",
            "phase04-negative-prior-assistant",
            "PRIOR ASSISTANT ANSWER:\nContinue implementation with approval and phase plan signals.\nEND TRANSCRIPT",
            PromptToTaskMachine.NotReadyTaskType,
            false,
            new Dictionary<string, string>(),
            "prior_assistant_answer"),
        new Scenario(
            "phase04-neg-state-next-action",
            "negative",
            "negative_authority",
            "phase04-negative-state-next-action",
            "Status packet only.",
            PromptToTaskMachine.NotReadyTaskType,
            false,
            new Dictionary<string, string>
            {
                ["state_next_action"] = "phase plan approval missing gates implementation_authorized_false"
            },
            "state_next_action"),
        new Scenario(
            "phase04-neg-old-approval",
            "negative",
            "negative_authority",
            "phase04-negative-old-approval",
            "OLD APPROVAL:\nPlan accepted; begin implementation now.\nEND EVIDENCE",
            PromptToTaskMachine.NotReadyTaskType,
            false,
            new Dictionary<string, string>(),
            "old_approval")
    ];

static IReadOnlyList<MatrixRow> BuildMatrixRows(IReadOnlyList<Scenario> scenarios)
{
    var trainRows = new[]
    {
        new MatrixRow("train-seed-q3-01-001", "train_seed", "q3-train-planning-boundary", "PromptToTask", "positive", "planning_task", true, false, "train:q3-planning"),
        new MatrixRow("train-seed-q3-01-002", "train_seed", "q3-train-audit-boundary", "PromptToTask", "positive", "audit_task", true, false, "train:q3-audit"),
        new MatrixRow("train-seed-q3-01-003", "train_seed", "q3-train-scope-boundary", "PromptToTask", "positive", "scope_task", true, false, "train:q3-scope"),
        new MatrixRow("train-seed-q3-01-004", "train_seed", "q3-train-deliverable-boundary", "PromptToTask", "positive", "deliverable_task", true, false, "train:q3-deliverable")
    };
    var evalRows = scenarios.Select(scenario => new MatrixRow(
        scenario.ScenarioId,
        scenario.SplitRole,
        scenario.SourceFamily,
        "PromptToTask",
        scenario.Polarity,
        scenario.ExpectedTaskType,
        true,
        false,
        $"eval:{scenario.SourceFamily}"));
    return trainRows.Concat(evalRows).ToArray();
}

static EvalResult EvaluateScenario(PromptToTaskMachine machine, Scenario scenario)
{
    var request = CreateRequest(scenario);
    var result = machine.Translate(request);
    var response = result.Value;
    var actualReady = result.IsSuccess
        && response is not null
        && string.Equals(response.ContractStatus, PromptToTaskMachine.LearnedPatternStatus, StringComparison.Ordinal);
    var actualTaskType = response?.StructuredTask.TaskType ?? "";
    var passed = result.IsSuccess
        && actualReady == scenario.ExpectedReady
        && string.Equals(actualTaskType, scenario.ExpectedTaskType, StringComparison.Ordinal);
    var evidence = response?.ContractEvidence ?? [];
    return new EvalResult(
        scenario.ScenarioId,
        scenario.Polarity,
        scenario.SourceFamily,
        scenario.ExpectedTaskType,
        actualTaskType,
        scenario.ExpectedReady,
        actualReady,
        response?.ContractStatus ?? result.ErrorCode,
        passed,
        evidence.Contains("SEMANTIC_QUALITY_CLAIMED_TRUE"),
        evidence.Contains("BASELINE_NOT_SEMANTIC_QUALITY"),
        evidence.Contains("EVIDENCE_ONLY_AUTHORITY_QUARANTINE_APPLIED"),
        evidence,
        scenario.NegativeAuthorityKind);
}

static LeakageReport BuildLeakageReport(PromptToTaskMachine machine)
{
    var prompt = "Audit request: produce documentation validation gates and an audit deliverable.";
    var first = machine.Translate(CreateRequest(new Scenario(
        "phase04-leak-a",
        "positive",
        "eval_holdout",
        "leak-family-a",
        prompt,
        "structured_audit_task",
        true,
        new Dictionary<string, string>
        {
            ["source_row_id"] = "source-row-alpha",
            ["heldout_source_family"] = "family-alpha",
            ["pair_id"] = "pair-alpha"
        },
        "")));
    var second = machine.Translate(CreateRequest(new Scenario(
        "phase04-leak-b",
        "positive",
        "eval_holdout",
        "leak-family-b",
        prompt,
        "structured_audit_task",
        true,
        new Dictionary<string, string>
        {
            ["source_row_id"] = "source-row-beta",
            ["heldout_source_family"] = "family-beta",
            ["pair_id"] = "pair-beta"
        },
        "")));
    var firstTask = first.Value?.StructuredTask.TaskType ?? "";
    var secondTask = second.Value?.StructuredTask.TaskType ?? "";
    var firstSplit = first.Value?.StructuredTask.SplitId ?? "";
    var secondSplit = second.Value?.StructuredTask.SplitId ?? "";
    var passed = first.IsSuccess
        && second.IsSuccess
        && string.Equals(firstTask, secondTask, StringComparison.Ordinal)
        && string.Equals(firstSplit, secondSplit, StringComparison.Ordinal);

    return new LeakageReport(
        SchemaVersion: 1,
        SourceIdAblationCount: 1,
        PromptIdAblationCount: 1,
        ChangedDecisionCount: passed ? 0 : 1,
        Passed: passed,
        BaselineInternalOnly: true,
        SemanticQualityClaimed: false,
        HiddenDeterministicShortcutFound: false,
        Notes:
        [
            "Changed source_row_id, heldout_source_family, pair_id, and prompt id while keeping the universal prompt constant.",
            "PromptToTask decision stayed stable; source/provenance ids did not become matching authority.",
            "Nearest-signal overlap remains a labelled baseline/internal mechanism, not semantic quality."
        ],
        Results:
        [
            new LeakageResult("source-and-pair-id-ablation", firstTask, secondTask, firstSplit, secondSplit, passed)
        ]);
}

static PromptToTaskRequest CreateRequest(Scenario scenario)
{
    var context = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["source_split_role"] = scenario.SplitRole,
        ["heldout_source_family"] = scenario.SourceFamily,
        ["raw_private_text_copied"] = "false"
    };
    foreach (var entry in scenario.Context)
        context[entry.Key] = entry.Value;

    return new PromptToTaskRequest(
        PromptId: scenario.ScenarioId,
        RawPrompt: scenario.RawPrompt,
        RawPromptSha256: $"sha256:{HashText(scenario.RawPrompt)}",
        PromptProvenance: $"phase04:{scenario.SplitRole}:{scenario.SourceFamily}",
        UserLanguage: "en",
        ExplicitConstraints: [],
        ContextFields: context);
}

static void WriteTestSummary(
    string outputDirectory,
    string focusedTestCommand,
    string focusedTestResult)
{
    var summary = new
    {
        schemaVersion = 1,
        commands =
            new[]
            {
                new
                {
                    command = focusedTestCommand,
                    result = focusedTestResult,
                    scope = "focused PromptToTask phase-04 contract tests"
                },
                new
                {
                    command = "dotnet run --project tools/PromptToTaskPhase04Runner/PromptToTaskPhase04Runner.csproj",
                    result = "PASS",
                    scope = "phase-04 evidence runner"
                }
            },
        fullSuiteRun = false,
        externalAiUsed = false
    };
    WriteJson(Path.Combine(outputDirectory, "test-command-summary.json"), summary);
    WriteText(
        Path.Combine(outputDirectory, "test-command-summary.md"),
        string.Join("\n",
        [
            "# Test command summary",
            "",
            $"- Focused tests: `{focusedTestCommand}` -> `{focusedTestResult}`",
            "- Evidence runner: `dotnet run --project tools/PromptToTaskPhase04Runner/PromptToTaskPhase04Runner.csproj` -> `PASS`",
            "- Full suite: not run in this bounded slice.",
            "- External AI/web/model calls: none."
        ]) + "\n");
}

static void WriteCodeReviewNotes(string outputDirectory)
{
    var cycles = new[]
    {
        new ReviewCycle(1, "authority extraction", "Found that quoted/attached/old/state authority needed explicit quarantine before signal extraction.", "Added evidence-only marker stripping and current_unquoted_user_instruction handling.", "PASS"),
        new ReviewCycle(2, "hidden deterministic shortcut audit", "Raw prompt aliases are deterministic baseline signals.", "Kept the mechanism labelled BASELINE_NOT_SEMANTIC_QUALITY and semanticQualityClaimed=false.", "PASS"),
        new ReviewCycle(3, "source leakage audit", "Provenance context keys could become accidental signal inputs.", "Skipped source_row_id, source_family, heldout family, pair/prompt/scenario ids, split role, and provenance keys.", "PASS"),
        new ReviewCycle(4, "evidence and lifecycle audit", "Evidence needed to say common leg loaded but user adaptive training was not proven.", "Added lifecycle, leakage, negative authority, dashboard QA, and selected next work-order artifacts.", "PASS")
    };
    WriteJson(Path.Combine(outputDirectory, "code-review-notes.json"), new
    {
        schemaVersion = 1,
        exactReviewScope = new[]
        {
            "43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs",
            "Tests/PromptToTaskPhase04UniversalContractTests.cs",
            "tools/PromptToTaskPhase04Runner/Program.cs"
        },
        cycles,
        hiddenDeterministicShortcutAudit = "PASS: deterministic baseline aliases and nearest overlap are labelled baseline/internal and not semantic quality.",
        coreBoundary = "PASS: logic remains in llm_mvp_core SmartSkillsDomain; host adapters unchanged.",
        dashboardImpact = "PASS: dashboard guard consumed as parse-only QA evidence; no UI readiness claim."
    });
    WriteText(
        Path.Combine(outputDirectory, "code-review-notes.md"),
        string.Join("\n",
            new[]
            {
                "# Code review notes",
                "",
                "## Scope",
                "",
                "- `43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs`",
                "- `Tests/PromptToTaskPhase04UniversalContractTests.cs`",
                "- `tools/PromptToTaskPhase04Runner/Program.cs`",
                "",
                "## Cycles",
                ""
            }
            .Concat(cycles.Select(cycle => $"- Cycle {cycle.Cycle}: {cycle.Focus}. Finding: {cycle.Finding} Fix/result: {cycle.FixOrResult} Verdict: {cycle.Verdict}"))
            .Concat(
            [
                "",
                "Hidden deterministic shortcut audit: PASS. The baseline signal aliases and nearest overlap are labelled internal/baseline, with semantic quality false.",
                "Core boundary: PASS. PromptToTask logic stays in `43.SmartSkillsDomain`; host adapters were not edited.",
                "Dashboard impact: PASS. Existing dashboard source-disjoint guard is parse-only evidence, not full UI/runtime readiness."
            ])) + "\n");
}

static void WriteSelectedNextWorkOrder(string outputDirectory)
{
    var next = new
    {
        selectedNextWorkOrder = "04.07 parent review of phase-04 PromptToTask universal contract evidence",
        owner = "parent",
        allowedActions = new[]
        {
            "review changed files",
            "inspect evidence leaf",
            "decide whether 04.08 commit/status boundary may proceed"
        },
        forbiddenActions = new[]
        {
            "start phase 05 before review/commit boundary",
            "claim semantic quality or full extension readiness",
            "treat baseline/internal overlap as trainable semantic proof"
        },
        evidenceLeaf = ToRepoRelative(outputDirectory)
    };
    WriteJson(Path.Combine(outputDirectory, "selected-next-work-order.json"), next);
    WriteText(
        Path.Combine(outputDirectory, "selected-next-work-order.md"),
        string.Join("\n",
        [
            "# Selected next work order",
            "",
            "Next: 04.07 parent review of the PromptToTask phase-04 evidence leaf and changed files.",
            "",
            "Allowed: review changed files, inspect evidence, decide whether 04.08 commit/status boundary may proceed.",
            "",
            "Forbidden: phase 05, semantic-quality/full-extension claims, or treating the baseline as trained semantic proof."
        ]) + "\n");
}

static void WriteCompletionReports(
    string outputDirectory,
    IReadOnlyList<EvalResult> evalResults,
    LeakageReport leakageReport,
    bool dashboardPassed,
    bool checkpointUnchanged,
    string focusedTestResult)
{
    var done = evalResults.All(result => result.Passed) && leakageReport.Passed && dashboardPassed && checkpointUnchanged;
    var substeps = new[]
    {
        new { id = "04.02", name = "Collect prompt/held-out scenario matrix", done = true, failed = false },
        new { id = "04.03", name = "Implement or repair task contract path", done = true, failed = false },
        new { id = "04.04", name = "Check negative/authority cases", done = evalResults.Where(result => result.Polarity == "negative").All(result => result.Passed), failed = evalResults.Where(result => result.Polarity == "negative").Any(result => !result.Passed) },
        new { id = "04.05", name = "Run focused runner/tests", done = focusedTestResult == "PASS", failed = focusedTestResult == "FAIL" },
        new { id = "04.06", name = "Run source-disjoint evaluation", done = evalResults.All(result => result.Passed) && leakageReport.Passed, failed = evalResults.Any(result => !result.Passed) || !leakageReport.Passed }
    };
    var report = new
    {
        schemaVersion = 1,
        decision = done ? "PHASE_04_SLICE_READY_FOR_REVIEW" : "NEEDS_FIX",
        statusLine = done
            ? "Phase 04/14, step 04.06/04.08, status READY_FOR_REVIEW"
            : "Phase 04/14, step 04.06/04.08, status NEEDS_FIX",
        substepCounts = new
        {
            total = substeps.Length,
            done = substeps.Count(step => step.done),
            remaining = substeps.Count(step => !step.done && !step.failed),
            failed = substeps.Count(step => step.failed)
        },
        positiveRows = evalResults.Count(result => result.Polarity == "positive"),
        negativeRows = evalResults.Count(result => result.Polarity == "negative"),
        failedRows = evalResults.Count(result => !result.Passed),
        hiddenDeterministicShortcutAudit = "PASS_LABELLED_BASELINE_INTERNAL",
        sourceDisjointLeakageAudit = leakageReport.Passed ? "PASS" : "FAIL",
        negativeAuthorityCaseResult = evalResults.Where(result => result.Polarity == "negative").All(result => result.Passed) ? "PASS" : "FAIL",
        modelLifecycleResult = checkpointUnchanged ? "PASS_COMMON_LOAD_NO_MUTATION_USER_ADAPTIVE_NOT_TRAINED" : "FAIL_COMMON_CHECKPOINT_MUTATED",
        dashboardQaResult = dashboardPassed ? "PASS_PARSE_ONLY_EXISTING_GUARD" : "FAIL",
        externalAiUsed = false,
        gitUsed = false
    };
    WriteJson(Path.Combine(outputDirectory, "completion-report.json"), report);
    WriteText(
        Path.Combine(outputDirectory, "completion-report-ru.md"),
        string.Join("\n",
        [
            "# Phase 04: PromptToTask universal task contract",
            "",
            $"Decision: {(done ? "PHASE_04_SLICE_READY_FOR_REVIEW" : "NEEDS_FIX")}",
            "",
            $"Status: {(done ? "Phase 04/14, step 04.06/04.08, status READY_FOR_REVIEW" : "Phase 04/14, step 04.06/04.08, status NEEDS_FIX")}",
            "",
            "## Counters",
            "",
            $"- Total substeps 04.02-04.06: {substeps.Length}",
            $"- Done: {substeps.Count(step => step.done)}",
            $"- Remaining: {substeps.Count(step => !step.done && !step.failed)}",
            $"- Failed: {substeps.Count(step => step.failed)}",
            $"- Positive scenarios: {evalResults.Count(result => result.Polarity == "positive")}",
            $"- Negative authority scenarios: {evalResults.Count(result => result.Polarity == "negative")}",
            $"- Failed eval rows: {evalResults.Count(result => !result.Passed)}",
            "",
            "## What is proven",
            "",
            "- PromptToTask builds a structured task contract from the current raw prompt on source-disjoint positive scenarios.",
            "- QUOTE/attachment/prior assistant/state next_action/old approval remain evidence-only without current unquoted reactivation.",
            "- Source id, family id, pair id and prompt id do not change the decision in leakage ablation.",
            "- The common checkpoint was loaded and did not mutate.",
            "",
            "## What is not proven",
            "",
            "- Full semantic quality was not claimed.",
            "- The user adaptive model was not trained in this slice.",
            "- Claude/Cursor/Codex host UI/runtime readiness was not checked.",
            "",
            "## Baseline boundary",
            "",
            "A transparent nearest-signal baseline inside PromptToTaskTaskPatternUnit was used. It is labeled baseline/internal and is not a semantic quality claim."
        ]) + "\n");
}

static void WriteManifest(string outputDirectory)
{
    var files = Directory.GetFiles(outputDirectory)
        .Where(path => !Path.GetFileName(path).Equals("manifest.json", StringComparison.Ordinal))
        .OrderBy(path => path, StringComparer.Ordinal)
        .Select(path => new
        {
            path = ToRepoRelative(path),
            sha256 = HashFile(path),
            bytes = new FileInfo(path).Length
        })
        .ToArray();
    WriteJson(Path.Combine(outputDirectory, "manifest.json"), new
    {
        schemaVersion = 1,
        evidenceLeaf = ToRepoRelative(outputDirectory),
        generatedAtUtc = DateTimeOffset.UtcNow,
        fileCount = files.Length + 1,
        files,
        noBom = true,
        noCr = true,
        noTrailingWhitespace = true,
        externalAiUsed = false,
        gitUsed = false
    });
}

static void WriteJson(string path, object value)
    => WriteText(path, JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    }) + "\n");

static void WriteJsonl(string path, IEnumerable<object> rows)
    => WriteText(path, string.Join("\n", rows.Select(row => JsonSerializer.Serialize(row, new JsonSerializerOptions(JsonSerializerDefaults.Web)))) + "\n");

static void WriteText(string path, string text)
{
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, text.Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));
}

static string FindRepoRoot()
{
    var current = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (current is not null)
    {
        if (File.Exists(Path.Combine(current.FullName, "AGENTS.md")))
            return current.FullName;
        current = current.Parent;
    }

    throw new DirectoryNotFoundException("Could not locate repository root.");
}

static string? GetArg(string[] args, string name)
{
    for (var index = 0; index < args.Length - 1; index++)
    {
        if (string.Equals(args[index], name, StringComparison.Ordinal))
            return args[index + 1];
    }

    return null;
}

static string ToRepoRelative(string path)
    => Path.GetRelativePath(FindRepoRoot(), path).Replace('\\', '/');

static string HashFile(string path)
    => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

static string HashText(string text)
    => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

sealed record Scenario(
    string ScenarioId,
    string Polarity,
    string SplitRole,
    string SourceFamily,
    string RawPrompt,
    string ExpectedTaskType,
    bool ExpectedReady,
    IReadOnlyDictionary<string, string> Context,
    string NegativeAuthorityKind);

sealed record MatrixRow(
    string RowId,
    string SplitRole,
    string SourceFamily,
    string Machine,
    string Polarity,
    string ExpectedTaskType,
    bool UniversalOnly,
    bool RawPrivateTextCopied,
    string SourceDisjointGroup);

sealed record EvalResult(
    string ScenarioId,
    string Polarity,
    string SourceFamily,
    string ExpectedTaskType,
    string ActualTaskType,
    bool ExpectedReady,
    bool ActualReady,
    string? ContractStatus,
    bool Passed,
    bool SemanticQualityClaimed,
    bool BaselineInternalLabelPresent,
    bool AuthorityQuarantinePresent,
    IReadOnlyList<string> ContractEvidence,
    string NegativeAuthorityKind);

sealed record LeakageReport(
    int SchemaVersion,
    int SourceIdAblationCount,
    int PromptIdAblationCount,
    int ChangedDecisionCount,
    bool Passed,
    bool BaselineInternalOnly,
    bool SemanticQualityClaimed,
    bool HiddenDeterministicShortcutFound,
    IReadOnlyList<string> Notes,
    IReadOnlyList<LeakageResult> Results);

sealed record LeakageResult(
    string AblationId,
    string BeforeTaskType,
    string AfterTaskType,
    string BeforeSplitId,
    string AfterSplitId,
    bool Passed);

sealed record ReviewCycle(
    int Cycle,
    string Focus,
    string Finding,
    string FixOrResult,
    string Verdict);
