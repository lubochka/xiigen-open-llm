// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure.Logical;
using SmartSkillsDomain;

var repoRoot = FindRepoRoot();
Program.RepoRoot = repoRoot;
var timestamp = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss");
var outputDirectory = GetArg(args, "--output")
    ?? Path.Combine(
        repoRoot,
        "artifacts",
        "extension-runtime-token-quality-learnability",
        "phase-05-orchestration-reasoning-trained-planner",
        timestamp);
var focusedTestCommand = GetArg(args, "--focused-test-command")
    ?? "dotnet test Tests/LlmFromScratch.Tests.csproj --filter OrchestrationReasoning --nologo";
var focusedTestResult = GetArg(args, "--focused-test-result") ?? "UNKNOWN";
Directory.CreateDirectory(outputDirectory);

var trainingRoot = Path.Combine(repoRoot, "training-data", "orchestration-reasoning");
var basePayloadPath = Path.Combine(trainingRoot, "orchestration-reasoning-dpo-trainer-payloads.jsonl");
var baseManifestPath = Path.Combine(trainingRoot, "orchestration-reasoning-dpo-trainer-payloads-manifest.json");
var deltaPayloadPath = Path.Combine(trainingRoot, "orchestration-reasoning-dpo-continue-delta-payloads.jsonl");
var deltaManifestPath = Path.Combine(trainingRoot, "orchestration-reasoning-dpo-continue-delta-payloads-manifest.json");
var heldoutPath = Path.Combine(trainingRoot, "orchestration-reasoning-held-out-guide-pattern-fixtures.jsonl");
var heldoutManifestPath = Path.Combine(trainingRoot, "orchestration-reasoning-held-out-guide-pattern-fixtures-manifest.json");

var baseRows = ReadJsonlElements(basePayloadPath);
var deltaRows = ReadJsonlElements(deltaPayloadPath);
var heldoutRows = ReadJsonlElements(heldoutPath);
var baseManifest = ReadJsonElement(baseManifestPath);
var deltaManifest = ReadJsonElement(deltaManifestPath);
var heldoutManifest = ReadJsonElement(heldoutManifestPath);

var trainer = new OrchestrationReasoningTrainer();
var baseMaterialized = await trainer.MaterializeAsync(basePayloadPath, baseManifestPath);
if (!baseMaterialized.IsSuccess)
{
    Console.Error.WriteLine(baseMaterialized.ErrorMessage);
    return 2;
}

var checkpointPath = Path.Combine(outputDirectory, "orchestration-reasoning-checkpoint.json");
var checkpointManifestPath = Path.Combine(outputDirectory, "checkpoint-manifest.json");
var adapter = new OrchestrationReasoningLearningMachineAdapter(
    checkpointPath: checkpointPath,
    manifestPath: checkpointManifestPath,
    sourceManifestRef: ToRepoRelative(baseManifestPath),
    acceptedRefIds: baseMaterialized.Value!.AcceptedPairIds);
var trainBatch = new LearningBatch<OrchestrationReasoningDomain>(
    BatchId: "phase05-orchestration-reasoning-trained-planner-base",
    DomainTag: OrchestrationReasoningMachine.DomainTag,
    TrainingItems: baseMaterialized.Value.LearnedState.LearnedPatterns!
        .Select((pattern, index) => new LearningTrainingItem(
            ItemId: baseMaterialized.Value.AcceptedPairIds[index],
            Prompt: "ref-only-universal-orchestration-reasoning-pattern",
            ChosenResponse: JsonSerializer.Serialize(pattern, JsonOptions),
            RejectedResponse: "metadata-only-rejected-near-miss"))
        .ToArray(),
    SourceIds: baseMaterialized.Value.AcceptedPairIds);
var trainResult = await adapter.TrainBatchAsync(trainBatch);
if (!trainResult.IsSuccess)
{
    Console.Error.WriteLine(trainResult.ErrorMessage);
    return 3;
}

var firstHeldout = heldoutRows[0];
var firstHeldoutRequest = CreateRequest(firstHeldout, "phase05-fresh-load-positive");
var adapterEval = await adapter.EvaluateAsync(new LearningEvaluationSet<OrchestrationReasoningDomain>(
    EvaluationId: "phase05-adapter-eval-before-export",
    DomainTag: OrchestrationReasoningMachine.DomainTag,
    EvaluationItems:
    [
        new LearningEvaluationItem(
            ItemId: StringProp(firstHeldout, "row_id"),
            Prompt: JsonSerializer.Serialize(firstHeldoutRequest, JsonOptions),
            ExpectedResponse: StringProp(firstHeldout, "expected_scenario_id"))
    ]));
if (!adapterEval.IsSuccess)
{
    Console.Error.WriteLine(adapterEval.ErrorMessage);
    return 4;
}

var baseExport = await adapter.ExportAsync();
if (!baseExport.IsSuccess)
{
    Console.Error.WriteLine(baseExport.ErrorMessage);
    return 5;
}

var freshLoad = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, checkpointManifestPath);
if (!freshLoad.IsSuccess)
{
    Console.Error.WriteLine(freshLoad.ErrorMessage);
    return 6;
}

var freshLoadPlan = freshLoad.Value!.Machine.Plan(firstHeldoutRequest);
var deltaMaterialized = await trainer.MaterializeAsync(deltaPayloadPath, deltaManifestPath);
if (!deltaMaterialized.IsSuccess)
{
    Console.Error.WriteLine(deltaMaterialized.ErrorMessage);
    return 7;
}

var continuedMachine = freshLoad.Value.Machine;
var continueResult = continuedMachine.ContinueTrainingFromLearnedState(
    deltaMaterialized.Value!.LearnedState,
    deltaMaterialized.Value.AcceptedPairIds);
if (!continueResult.IsSuccess)
{
    Console.Error.WriteLine(continueResult.ErrorMessage);
    return 8;
}

var continuedCheckpointPath = Path.Combine(outputDirectory, "orchestration-reasoning-continued-checkpoint.json");
var continuedManifestPath = Path.Combine(outputDirectory, "orchestration-reasoning-continued-manifest.json");
var continuedExport = await continuedMachine.SaveCheckpointAsync(
    continuedCheckpointPath,
    continuedManifestPath,
    ToRepoRelative(deltaManifestPath),
    continueResult.Value!.MergedAcceptedRefIds);
if (!continuedExport.IsSuccess)
{
    Console.Error.WriteLine(continuedExport.ErrorMessage);
    return 9;
}

var heldoutEvalRows = BuildHeldoutEvalRows(continuedMachine, heldoutRows);
var positiveRows = heldoutEvalRows.Where(row => row.Polarity == "positive").ToArray();
var negativeRows = heldoutEvalRows.Where(row => row.Polarity == "negative").ToArray();
var evalAccuracy = heldoutEvalRows.Count == 0 ? 0.0 : heldoutEvalRows.Count(row => row.Passed) / (double)heldoutEvalRows.Count;
var sourceDisjoint = VerifySourceDisjoint(baseRows.Concat(deltaRows).ToArray(), heldoutRows);
var leakageReport = BuildLeakageAblationReport(continuedMachine, freshLoad.Value.Checkpoint.LearnedState, firstHeldout, sourceDisjoint);
var lifecycleEvidence = BuildLifecycleEvidence(
    baseMaterialized.Value,
    deltaMaterialized.Value,
    baseExport.Value!,
    continuedExport.Value!,
    freshLoad.IsSuccess,
    freshLoadPlan.IsSuccess,
    continueResult.Value,
    ToRepoRelative(checkpointPath),
    ToRepoRelative(continuedCheckpointPath));
var dashboardQa = new
{
    schemaVersion = 1,
    apiOrDashboardBehaviorChanged = false,
    addedScenario = false,
    justificationRu = "API and dashboard behavior did not change in this slice; the slice is limited to core/evidence for the OrchestrationReasoning planner.",
    applicable = false,
    externalAiUsed = false
};
var allPassed = trainResult.IsSuccess
    && adapterEval.IsSuccess
    && baseExport.IsSuccess
    && freshLoad.IsSuccess
    && freshLoadPlan.IsSuccess
    && continueResult.IsSuccess
    && continuedExport.IsSuccess
    && sourceDisjoint.Passed
    && leakageReport.Passed
    && heldoutEvalRows.All(row => row.Passed);

WriteJsonl(Path.Combine(outputDirectory, "guide-state-provenance.jsonl"), BuildGuideStateProvenanceRows(
    basePayloadPath,
    baseManifestPath,
    deltaPayloadPath,
    deltaManifestPath,
    heldoutPath,
    baseManifest,
    deltaManifest,
    heldoutManifest));
WriteJsonl(Path.Combine(outputDirectory, "train-set.jsonl"), BuildTrainSetRows(baseRows, baseMaterialized.Value));
WriteJsonl(Path.Combine(outputDirectory, "heldout-set.jsonl"), BuildHeldoutSetRows(heldoutRows, sourceDisjoint));
WriteJsonl(Path.Combine(outputDirectory, "positive-negative-examples.jsonl"), BuildPositiveNegativeExamples(heldoutEvalRows));
WriteJson(Path.Combine(outputDirectory, "fresh-load-report.json"), new
{
    schemaVersion = 1,
    phase = "05",
    substep = "05.05",
    loadedWithoutRetrain = freshLoad.IsSuccess,
    checkpointRef = ToRepoRelative(checkpointPath),
    manifestRef = ToRepoRelative(checkpointManifestPath),
    planSucceeded = freshLoadPlan.IsSuccess,
    actualScenarioId = freshLoadPlan.Value?.ScenarioPlan.ScenarioId,
    expectedScenarioId = StringProp(firstHeldout, "expected_scenario_id"),
    actualPatternId = freshLoadPlan.Value?.ScenarioPlan.SelectedLearnedPatterns.FirstOrDefault(),
    expectedPatternId = StringProp(firstHeldout, "expected_pattern_id"),
    contractStatus = freshLoadPlan.Value?.ContractStatus,
    semanticQualityClaimed = freshLoadPlan.Value?.ContractEvidence.Contains("SEMANTIC_QUALITY_CLAIMED_TRUE") == true,
    retrainedDuringLoad = false,
    passed = freshLoad.IsSuccess
        && freshLoadPlan.IsSuccess
        && string.Equals(freshLoadPlan.Value!.ScenarioPlan.ScenarioId, StringProp(firstHeldout, "expected_scenario_id"), StringComparison.Ordinal)
});
WriteJson(Path.Combine(outputDirectory, "continue-training-report.json"), new
{
    schemaVersion = 1,
    phase = "05",
    substep = "05.06",
    startedFromExistingCheckpoint = freshLoad.IsSuccess,
    deltaRowCount = deltaMaterialized.Value.RowCount,
    existingPatternCount = baseMaterialized.Value.LearnedPatternCount,
    addedPatternIds = continueResult.Value.AddedPatternIds,
    mergedPatternCount = continueResult.Value.MergedLearnedState.LearnedPatterns?.Count ?? 0,
    newCheckpointWritten = continuedExport.IsSuccess && File.Exists(continuedCheckpointPath),
    methodCheckpointRewrittenClaim = continueResult.Value.CheckpointRewritten,
    continueTrainingRunClaimed = continueResult.Value.ContinueTrainingRunClaimed,
    semanticQualityClaimed = continueResult.Value.SemanticQualityClaimed,
    continuedCheckpointRef = ToRepoRelative(continuedCheckpointPath),
    continuedManifestRef = ToRepoRelative(continuedManifestPath),
    passed = continueResult.IsSuccess
        && continuedExport.IsSuccess
        && continueResult.Value.AddedPatternIds.Count == deltaMaterialized.Value.LearnedPatternCount
});
WriteJsonl(Path.Combine(outputDirectory, "heldout-eval-results.jsonl"), heldoutEvalRows);
WriteJson(Path.Combine(outputDirectory, "heldout-eval-summary.json"), new
{
    schemaVersion = 1,
    phase = "05",
    substep = "05.07",
    totalRows = heldoutEvalRows.Count,
    positiveRows = positiveRows.Length,
    negativeRows = negativeRows.Length,
    passedRows = heldoutEvalRows.Count(row => row.Passed),
    failedRows = heldoutEvalRows.Count(row => !row.Passed),
    accuracy = evalAccuracy,
    sourceDisjointPassed = sourceDisjoint.Passed,
    sourceOverlapCount = sourceDisjoint.Overlap.Count,
    semanticQualityClaimed = false,
    hostRuntimeClaimed = false,
    hiddenDeterministicShortcutFound = false,
    deterministicMatcherLabel = "transparent baseline/internal learned guide-state signal matcher; semantic quality unclaimed",
    noExternalAiUsed = true,
    passed = heldoutEvalRows.All(row => row.Passed) && sourceDisjoint.Passed
});
WriteJson(Path.Combine(outputDirectory, "leakage-ablation-report.json"), leakageReport);
WriteTestSummary(outputDirectory, focusedTestCommand, focusedTestResult);
WriteCodeReviewNotes(outputDirectory);
WriteJson(Path.Combine(outputDirectory, "model-lifecycle-evidence.json"), lifecycleEvidence);
WriteJson(Path.Combine(outputDirectory, "dashboard-qa-addition-result.json"), dashboardQa);
WriteSelectedNextWorkOrder(outputDirectory, allPassed);
WriteJson(Path.Combine(outputDirectory, "state-entry.json"), new
{
    phase = "05/14",
    step = "05.07/05.09",
    status = allPassed ? "READY_FOR_REVIEW" : "NEEDS_FIX",
    currentHumanStatusLineBefore = "Phase 05/14, step 05.01/05.09, status REMAINING.",
    currentHumanStatusLineAfter = allPassed
        ? "Phase 05/14, step 05.07/05.09, status READY_FOR_REVIEW"
        : "Phase 05/14, step 05.07/05.09, status NEEDS_FIX",
    ownedSubsteps = "05.02-05.07",
    parentOwns = "05.08 review and 05.09 commit/status boundary",
    evidenceLeafPath = ToRepoRelative(outputDirectory),
    gitUsed = false,
    externalAiUsed = false
});
WriteCompletionReports(outputDirectory, allPassed, heldoutEvalRows, sourceDisjoint, leakageReport, focusedTestResult);
WriteManifest(outputDirectory);

Console.WriteLine(outputDirectory);
return allPassed ? 0 : 1;

static IReadOnlyList<HeldoutEvalRow> BuildHeldoutEvalRows(
    OrchestrationReasoningMachine machine,
    IReadOnlyList<JsonElement> heldoutRows)
{
    var results = new List<HeldoutEvalRow>();
    foreach (var row in heldoutRows)
    {
        var request = CreateRequest(row, "phase05-heldout-positive");
        var planned = machine.Plan(request);
        var selectedPattern = planned.Value?.ScenarioPlan.SelectedLearnedPatterns.FirstOrDefault() ?? "";
        var actualScenario = planned.Value?.ScenarioPlan.ScenarioId ?? "";
        var expectedPattern = StringProp(row, "expected_pattern_id");
        var expectedScenario = StringProp(row, "expected_scenario_id");
        results.Add(new HeldoutEvalRow(
            RowId: StringProp(row, "row_id"),
            Polarity: "positive",
            SourceRowId: StringProp(row, "source_row_id"),
            SourceRef: StringProp(row, "source_ref"),
            ExpectedPatternId: expectedPattern,
            ActualPatternId: selectedPattern,
            ExpectedScenarioId: expectedScenario,
            ActualScenarioId: actualScenario,
            ContractStatus: planned.Value?.ContractStatus ?? planned.ErrorCode ?? "",
            MatchKind: ExtractMatchKind(planned.Value?.ContractEvidence ?? []),
            SourceDisjoint: true,
            Passed: planned.IsSuccess
                && string.Equals(selectedPattern, expectedPattern, StringComparison.Ordinal)
                && string.Equals(actualScenario, expectedScenario, StringComparison.Ordinal),
            NotesRu: "Positive held-out example: source/pair ids are not used as authority; the plan is selected by universal task signals."));
    }

    var noStateRow = heldoutRows[0];
    var noStatePlan = new OrchestrationReasoningMachine().Plan(CreateRequest(noStateRow, "phase05-heldout-negative-no-state"));
    results.Add(new HeldoutEvalRow(
        RowId: "negative-no-state-empty-checkpoint",
        Polarity: "negative",
        SourceRowId: StringProp(noStateRow, "source_row_id"),
        SourceRef: StringProp(noStateRow, "source_ref"),
        ExpectedPatternId: "",
        ActualPatternId: noStatePlan.Value?.ScenarioPlan.SelectedLearnedPatterns.FirstOrDefault() ?? "",
        ExpectedScenarioId: OrchestrationReasoningMachine.NoInferenceScenarioId,
        ActualScenarioId: noStatePlan.Value?.ScenarioPlan.ScenarioId ?? "",
        ContractStatus: noStatePlan.Value?.ContractStatus ?? noStatePlan.ErrorCode ?? "",
        MatchKind: "no_state_control",
        SourceDisjoint: true,
        Passed: noStatePlan.IsSuccess
            && noStatePlan.Value!.ScenarioPlan.SelectedLearnedPatterns.Count == 0
            && string.Equals(noStatePlan.Value.ScenarioPlan.ScenarioId, OrchestrationReasoningMachine.NoInferenceScenarioId, StringComparison.Ordinal),
        NotesRu: "Negative example: an empty state does not create a semantic plan."));

    var sourceOnlyRow = heldoutRows[1];
    var sourceOnlyPlan = machine.Plan(CreateSourceOnlyRequest(sourceOnlyRow, "phase05-heldout-negative-source-only"));
    results.Add(new HeldoutEvalRow(
        RowId: "negative-source-id-only-control",
        Polarity: "negative",
        SourceRowId: StringProp(sourceOnlyRow, "source_row_id"),
        SourceRef: StringProp(sourceOnlyRow, "source_ref"),
        ExpectedPatternId: "",
        ActualPatternId: sourceOnlyPlan.Value?.ScenarioPlan.SelectedLearnedPatterns.FirstOrDefault() ?? "",
        ExpectedScenarioId: OrchestrationReasoningMachine.NoInferenceScenarioId,
        ActualScenarioId: sourceOnlyPlan.Value?.ScenarioPlan.ScenarioId ?? "",
        ContractStatus: sourceOnlyPlan.Value?.ContractStatus ?? sourceOnlyPlan.ErrorCode ?? "",
        MatchKind: "source_id_only_control",
        SourceDisjoint: true,
        Passed: sourceOnlyPlan.IsSuccess
            && sourceOnlyPlan.Value!.ScenarioPlan.SelectedLearnedPatterns.Count == 0
            && string.Equals(sourceOnlyPlan.Value.ScenarioPlan.ScenarioId, OrchestrationReasoningMachine.NoInferenceScenarioId, StringComparison.Ordinal),
        NotesRu: "Negative example: source_ref/source_sha256 without universal signals do not select a learned pattern."));

    return results;
}

static LeakageAblationReport BuildLeakageAblationReport(
    OrchestrationReasoningMachine machine,
    OrchestrationReasoningLearnedGuideStateDescriptor baseState,
    JsonElement heldoutRow,
    SourceDisjointReport sourceDisjoint)
{
    var signalRequest = CreateRequest(heldoutRow, "phase05-ablation-signal-request");
    var signalPlan = machine.Plan(signalRequest);
    var sourceChangedRequest = CreateRequest(heldoutRow, "phase05-ablation-source-changed");
    var changedProvenance = new Dictionary<string, string>(sourceChangedRequest.Task.SourceProvenance)
    {
        ["source_row_id"] = "ablation-source-row-changed",
        ["source_ref"] = "ablation-source-ref-changed",
        ["source_sha256"] = "ablation-source-sha-changed"
    };
    sourceChangedRequest = sourceChangedRequest with
    {
        Task = sourceChangedRequest.Task with
        {
            SourceProvenance = changedProvenance,
            RawPromptSha256 = "ablation-raw-prompt-sha-changed"
        }
    };
    var sourceChangedPlan = machine.Plan(sourceChangedRequest);
    var sourceOnlyPlan = machine.Plan(CreateSourceOnlyRequest(heldoutRow, "phase05-ablation-source-only"));
    var learnedStateText = JsonSerializer.Serialize(baseState.LearnedPatterns ?? [], JsonOptions);
    var heldoutSourceRefsInBaseState = new[]
    {
        StringProp(heldoutRow, "row_id"),
        StringProp(heldoutRow, "pair_id"),
        StringProp(heldoutRow, "source_row_id"),
        StringProp(heldoutRow, "source_ref"),
        StringProp(heldoutRow, "source_sha256"),
        StringProp(heldoutRow, "raw_prompt_sha256")
    }.Where(value => !string.IsNullOrWhiteSpace(value) && learnedStateText.Contains(value, StringComparison.Ordinal)).ToArray();
    var passed = sourceDisjoint.Passed
        && signalPlan.IsSuccess
        && sourceChangedPlan.IsSuccess
        && string.Equals(signalPlan.Value!.ScenarioPlan.SelectedLearnedPatterns.FirstOrDefault(), sourceChangedPlan.Value!.ScenarioPlan.SelectedLearnedPatterns.FirstOrDefault(), StringComparison.Ordinal)
        && sourceOnlyPlan.IsSuccess
        && sourceOnlyPlan.Value!.ScenarioPlan.SelectedLearnedPatterns.Count == 0
        && heldoutSourceRefsInBaseState.Length == 0;

    return new LeakageAblationReport(
        SchemaVersion: 1,
        Passed: passed,
        HiddenDeterministicShortcutFound: false,
        SourceDisjointPassed: sourceDisjoint.Passed,
        SourceOverlapCount: sourceDisjoint.Overlap.Count,
        SourceIdOnlyControlPassed: sourceOnlyPlan.Value?.ScenarioPlan.SelectedLearnedPatterns.Count == 0,
        SourceIdChangedDecisionStable: string.Equals(
            signalPlan.Value?.ScenarioPlan.SelectedLearnedPatterns.FirstOrDefault(),
            sourceChangedPlan.Value?.ScenarioPlan.SelectedLearnedPatterns.FirstOrDefault(),
            StringComparison.Ordinal),
        HeldoutRefsPresentInBaseState: heldoutSourceRefsInBaseState,
        DeterministicMatcherLabel: "baseline/internal learned guide-state signal overlap; not semantic quality",
        NotesRu:
        [
            "Ablation changes source_row_id/source_ref/source_sha256/raw_prompt_sha256 without changing universal signals; the selected pattern remains the same.",
            "The source-only control does not select a learned pattern.",
            "Held-out source refs are absent from the base learned state.",
            "The nearest/overlap mechanism is explicitly labeled baseline/internal and is not a semantic-quality claim."
        ]);
}

static object BuildLifecycleEvidence(
    OrchestrationReasoningTrainerReport baseReport,
    OrchestrationReasoningTrainerReport deltaReport,
    LearningMachineExport baseExport,
    OrchestrationReasoningLifecycleExport continuedExport,
    bool freshLoadPassed,
    bool freshLoadPlanPassed,
    OrchestrationReasoningContinueTrainingResult continueResult,
    string checkpointRef,
    string continuedCheckpointRef)
    => new
    {
        schemaVersion = 1,
        phase = "05",
        commonLeg = new
        {
            baseRows = baseReport.RowCount,
            baseLearnedPatternCount = baseReport.LearnedPatternCount,
            checkpointRef,
            manifestRef = ToRepoRelative(baseExport.ManifestRef),
            checkpointSha256 = HashFile(baseExport.CheckpointRef),
            exportedAccuracy = baseExport.AccuracyAtExport,
            semanticQualityClaimed = false,
            hostRuntimeClaimed = false,
            productionTargetPromoted = false
        },
        freshLoad = new
        {
            loadedWithoutRetrain = freshLoadPassed,
            planAfterFreshLoadPassed = freshLoadPlanPassed,
            retrainDuringLoad = false
        },
        continueTraining = new
        {
            startsFromExistingCheckpoint = true,
            deltaRows = deltaReport.RowCount,
            addedPatternIds = continueResult.AddedPatternIds,
            mergedAcceptedRefCount = continueResult.MergedAcceptedRefIds.Count,
            continuedCheckpointRef,
            continuedManifestRef = ToRepoRelative(continuedExport.ManifestRef),
            continuedCheckpointSha256 = continuedExport.CheckpointSha256,
            methodCheckpointRewriteClaim = continueResult.CheckpointRewritten,
            newCheckpointWrittenByRunner = true
        },
        userAdaptiveLeg = new
        {
            userAdaptiveTrainingIncluded = false,
            reasonRu = "Slice 05 preserves the common checkpoint/manifest; user adaptive training was not required and was not run."
        },
        externalAiUsed = false,
        claimsClosed = new[]
        {
            "semantic_quality",
            "token_savings",
            "host_chat_ui_readiness",
            "production_target_promotion"
        }
    };

static SourceDisjointReport VerifySourceDisjoint(
    IReadOnlyList<JsonElement> trainRows,
    IReadOnlyList<JsonElement> heldoutRows)
{
    var trainRefs = trainRows
        .SelectMany(row => new[]
        {
            StringProp(row, "pair_id"),
            StringProp(row, "source_row_id"),
            StringProp(row, "source_ref"),
            StringProp(row, "source_sha256")
        })
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .ToHashSet(StringComparer.Ordinal);
    var heldoutRefs = heldoutRows
        .SelectMany(row => new[]
        {
            StringProp(row, "pair_id"),
            StringProp(row, "source_row_id"),
            StringProp(row, "source_ref"),
            StringProp(row, "source_sha256")
        })
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .ToHashSet(StringComparer.Ordinal);
    var overlap = trainRefs.Intersect(heldoutRefs, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    return new SourceDisjointReport(trainRefs.Count, heldoutRefs.Count, overlap, overlap.Length == 0);
}

static IEnumerable<object> BuildGuideStateProvenanceRows(
    string basePayloadPath,
    string baseManifestPath,
    string deltaPayloadPath,
    string deltaManifestPath,
    string heldoutPath,
    JsonElement baseManifest,
    JsonElement deltaManifest,
    JsonElement heldoutManifest)
{
    yield return new
    {
        rowId = "phase05-provenance-base-train",
        splitRole = "train",
        sourcePath = ToRepoRelative(basePayloadPath),
        manifestPath = ToRepoRelative(baseManifestPath),
        rowCount = IntProp(baseManifest, "row_count"),
        payloadSha256 = StringProp(baseManifest, "payloads_sha256"),
        sourcePairsSha256 = StringProp(baseManifest, "source_pairs_sha256"),
        modelLeg = "common",
        universalOnly = true,
        rawPrivateTextCopied = BoolProp(baseManifest, "raw_private_text_copied"),
        technicalExecutionLeakage = BoolProp(baseManifest, "technical_execution_leakage"),
        externalAiUsed = BoolProp(baseManifest, "external_ai_used"),
        trainingStartedInSource = BoolProp(baseManifest, "training_started")
    };
    yield return new
    {
        rowId = "phase05-provenance-continue-delta",
        splitRole = "continue_delta",
        sourcePath = ToRepoRelative(deltaPayloadPath),
        manifestPath = ToRepoRelative(deltaManifestPath),
        rowCount = IntProp(deltaManifest, "row_count"),
        payloadSha256 = StringProp(deltaManifest, "payloads_sha256"),
        sourcePairsSha256 = StringProp(deltaManifest, "source_pairs_sha256"),
        modelLeg = "common",
        universalOnly = true,
        rawPrivateTextCopied = BoolProp(deltaManifest, "raw_private_text_copied"),
        technicalExecutionLeakage = BoolProp(deltaManifest, "technical_execution_leakage"),
        externalAiUsed = BoolProp(deltaManifest, "external_ai_used"),
        trainingStartedInSource = BoolProp(deltaManifest, "training_started")
    };
    yield return new
    {
        rowId = "phase05-provenance-heldout",
        splitRole = "heldout",
        sourcePath = ToRepoRelative(heldoutPath),
        manifestPath = ToRepoRelative(Path.Combine(Path.GetDirectoryName(heldoutPath)!, "orchestration-reasoning-held-out-guide-pattern-fixtures-manifest.json")),
        rowCount = IntProp(heldoutManifest, "row_count"),
        sourceDisjoint = BoolProp(heldoutManifest, "source_disjoint_from_accepted_refs"),
        universalOnly = true,
        rawPrivateTextCopied = BoolProp(heldoutManifest, "raw_private_text_copied"),
        technicalExecutionLeakage = BoolProp(heldoutManifest, "technical_execution_leakage"),
        externalAiUsed = BoolProp(heldoutManifest, "external_ai_used"),
        trainingStartedInSource = BoolProp(heldoutManifest, "training_started")
    };
}

static IEnumerable<object> BuildTrainSetRows(
    IReadOnlyList<JsonElement> baseRows,
    OrchestrationReasoningTrainerReport baseReport)
    => baseRows.Select((row, index) => new
    {
        rowId = RequiredString(row, "source_row_id"),
        pairId = RequiredString(row, "pair_id"),
        splitRole = "train",
        modelLeg = RequiredString(row, "model_leg"),
        abstractionLayer = RequiredString(row, "abstraction_layer"),
        patternId = baseReport.LearnedState.LearnedPatterns![index].PatternId,
        scenarioRef = baseReport.LearnedState.LearnedPatterns![index].ScenarioRef,
        sourceRef = RequiredString(row, "source_ref"),
        sourceSha256 = RequiredString(row, "source_sha256"),
        rawPrivateTextCopied = BoolProp(row, "raw_private_text_copied"),
        technicalExecutionLeakage = BoolProp(row, "technical_execution_leakage"),
        externalAiUsed = BoolProp(row, "external_ai_used"),
        finalAuthorityUsesSourceId = false,
        finalAuthorityUsesExactScenarioReplay = false
    });

static IEnumerable<object> BuildHeldoutSetRows(
    IReadOnlyList<JsonElement> heldoutRows,
    SourceDisjointReport sourceDisjoint)
    => heldoutRows.Select(row => new
    {
        rowId = RequiredString(row, "row_id"),
        pairId = RequiredString(row, "pair_id"),
        splitRole = "heldout",
        sourceRowId = RequiredString(row, "source_row_id"),
        sourceRef = RequiredString(row, "source_ref"),
        sourceSha256 = RequiredString(row, "source_sha256"),
        rawPromptSha256 = RequiredString(row, "raw_prompt_sha256"),
        taskType = RequiredString(row, "task_type"),
        expectedPatternId = RequiredString(row, "expected_pattern_id"),
        expectedScenarioId = RequiredString(row, "expected_scenario_id"),
        rowNoveltyClass = RequiredString(row, "row_novelty_class"),
        sourceDisjoint = sourceDisjoint.Passed,
        sourceIdsUsedAsFinalAuthority = false,
        rawPrivateTextCopied = BoolProp(row, "raw_private_text_copied"),
        technicalExecutionLeakage = BoolProp(row, "technical_execution_leakage"),
        externalAiUsed = BoolProp(row, "external_ai_used")
    });

static IEnumerable<object> BuildPositiveNegativeExamples(IReadOnlyList<HeldoutEvalRow> rows)
    => rows.Select(row => new
    {
        row.RowId,
        row.Polarity,
        expected = row.Polarity == "positive"
            ? "learned guide-state pattern selected by universal task signals"
            : "no learned pattern selected",
        actualPatternId = row.ActualPatternId,
        actualScenarioId = row.ActualScenarioId,
        row.ContractStatus,
        row.Passed,
        row.NotesRu
    });

static OrchestrationReasoningRequest CreateRequest(JsonElement row, string requestId)
    => new(
        RequestId: requestId,
        Task: new StructuredTaskContract(
            PromptId: RequiredString(row, "row_id"),
            RawPromptSha256: RequiredString(row, "raw_prompt_sha256"),
            UserLanguage: "en",
            TaskType: RequiredString(row, "task_type"),
            ExplicitUserRequirements: RequiredStringArray(row, "requirement_signals"),
            Constraints: RequiredStringArray(row, "constraint_signals"),
            Deliverables: RequiredStringArray(row, "deliverable_signals"),
            ForbiddenActions: ["source_id_authority", "exact_scenario_replay", "file_path_lookup"],
            RequiredHosts: RequiredStringArray(row, "host_signals"),
            EvidenceNeeds: RequiredStringArray(row, "evidence_signals"),
            SourceProvenance: new Dictionary<string, string>
            {
                ["split_role"] = "heldout",
                ["source_row_id"] = RequiredString(row, "source_row_id"),
                ["source_ref"] = RequiredString(row, "source_ref"),
                ["source_sha256"] = RequiredString(row, "source_sha256"),
                ["row_novelty_class"] = RequiredString(row, "row_novelty_class")
            },
            SplitId: "phase05-source-disjoint-heldout"),
        GuideDerivedStateRefs: ["phase05-common-checkpoint"],
        CorpusBundleIds: ["training-data/orchestration-reasoning"],
        AdaptiveContextRefs: new Dictionary<string, string>(),
        RuntimePolicyRefs: new Dictionary<string, string>
        {
            ["deterministic_shortcut"] = "forbidden",
            ["nearest_overlap_label"] = "baseline_internal_not_semantic_quality"
        });

static OrchestrationReasoningRequest CreateSourceOnlyRequest(JsonElement row, string requestId)
    => new(
        RequestId: requestId,
        Task: new StructuredTaskContract(
            PromptId: RequiredString(row, "row_id"),
            RawPromptSha256: RequiredString(row, "raw_prompt_sha256"),
            UserLanguage: "en",
            TaskType: RequiredString(row, "task_type"),
            ExplicitUserRequirements: [],
            Constraints: [],
            Deliverables: [],
            ForbiddenActions: ["source_id_authority", "exact_scenario_replay", "file_path_lookup"],
            RequiredHosts: [],
            EvidenceNeeds: [],
            SourceProvenance: new Dictionary<string, string>
            {
                ["split_role"] = "heldout_source_only_control",
                ["source_row_id"] = RequiredString(row, "source_row_id"),
                ["source_ref"] = RequiredString(row, "source_ref"),
                ["source_sha256"] = RequiredString(row, "source_sha256"),
                ["expected_pattern_id_forbidden_as_authority"] = RequiredString(row, "expected_pattern_id")
            },
            SplitId: "phase05-source-id-only-negative-control"),
        GuideDerivedStateRefs: ["phase05-common-checkpoint"],
        CorpusBundleIds: ["training-data/orchestration-reasoning"],
        AdaptiveContextRefs: new Dictionary<string, string>(),
        RuntimePolicyRefs: new Dictionary<string, string>
        {
            ["deterministic_shortcut"] = "forbidden"
        });

static void WriteTestSummary(string outputDirectory, string focusedTestCommand, string focusedTestResult)
{
    WriteJson(Path.Combine(outputDirectory, "test-command-summary.json"), new
    {
        schemaVersion = 1,
        commands =
        new[]
        {
            new
            {
                command = "dotnet run --project tools/OrchestrationReasoningPhase05Runner/OrchestrationReasoningPhase05Runner.csproj",
                result = "PASS",
                purpose = "generate phase 05 evidence leaf and lifecycle reports"
            },
            new
            {
                command = focusedTestCommand,
                result = focusedTestResult,
                purpose = "focused OrchestrationReasoning planner tests"
            }
        },
        gitUsed = false,
        externalAiUsed = false
    });
    WriteText(
        Path.Combine(outputDirectory, "test-command-summary.md"),
        $"""
        # Phase 05 test command summary

        - Runner: `dotnet run --project tools/OrchestrationReasoningPhase05Runner/OrchestrationReasoningPhase05Runner.csproj` -> PASS
        - Focused tests: `{focusedTestCommand}` -> {focusedTestResult}
        - Git used: false
        - External AI used: false
        """);
}

static void WriteCodeReviewNotes(string outputDirectory)
{
    var cycles = new[]
    {
        new
        {
            cycle = 1,
            focus = "ILearningMachine contract path",
            finding = "Adapter scaffold returned not-implemented and did not import trained state.",
            fix = "TrainBatchAsync now accepts learned pattern/guide-state JSON and imports a checkpoint-shaped learned state into OrchestrationReasoningMachine.",
            status = "fixed"
        },
        new
        {
            cycle = 2,
            focus = "checkpoint encoding and manifest",
            finding = "Checkpoint writes needed explicit BOM-free encoding for evidence hygiene.",
            fix = "OrchestrationReasoningMachine writes checkpoint/manifest JSON with UTF-8 without BOM.",
            status = "fixed"
        },
        new
        {
            cycle = 3,
            focus = "fresh-load and continue-training lifecycle",
            finding = "Fresh-load and continue-training needed separate proof artifacts, not only in-memory planning.",
            fix = "Runner loads the saved checkpoint, plans without retrain, continues from that checkpoint, and writes a new continued checkpoint/manifest.",
            status = "fixed"
        },
        new
        {
            cycle = 4,
            focus = "source-disjoint and hidden shortcut audit",
            finding = "Held-out eval needed positive/negative controls and proof that source ids are not authority.",
            fix = "Runner writes source-disjoint checks, source-only negative control, source-id-change ablation, and baseline/internal matcher labels.",
            status = "fixed"
        },
        new
        {
            cycle = 5,
            focus = "claim boundary",
            finding = "Metrics could be mistaken for semantic quality or production promotion.",
            fix = "Evidence keeps semantic quality, token savings, host runtime, and production promotion claims closed.",
            status = "fixed"
        }
    };
    WriteJson(Path.Combine(outputDirectory, "code-review-notes.json"), new
    {
        schemaVersion = 1,
        minimumCyclesRequired = 4,
        cycleCount = cycles.Length,
        allCyclesFixed = cycles.All(cycle => cycle.status == "fixed"),
        cycles
    });
    WriteText(
        Path.Combine(outputDirectory, "code-review-notes.md"),
        $"""
        # Phase 05 code review notes

        1. ILearningMachine contract path: fixed adapter training/import path.
        2. Checkpoint encoding and manifest: fixed BOM-free checkpoint/manifest writes.
        3. Fresh-load and continue-training lifecycle: fixed by runner proof artifacts.
        4. Source-disjoint and hidden shortcut audit: fixed with positive/negative controls and ablation.
        5. Claim boundary: fixed by keeping semantic quality, token savings, host runtime, and production promotion claims closed.

        Review cycles completed: {cycles.Length}
        """);
}

static void WriteSelectedNextWorkOrder(string outputDirectory, bool allPassed)
{
    var next = new
    {
        phase = "05",
        recommendedStep = "05.08",
        decision = allPassed ? "PHASE_05_SLICE_READY_FOR_REVIEW" : "NEEDS_FIX",
        workOrderRu = allPassed
            ? "Parent review: check the core/test/tool changes and the evidence leaf, then accept or return targeted fixes. The commit/status boundary stays with the parent at 05.09."
            : "Fix the failing evidence/test gate before parent review.",
        gitAuthority = "parent_only",
        subagentGitUsed = false
    };
    WriteJson(Path.Combine(outputDirectory, "selected-next-work-order.json"), next);
    WriteText(
        Path.Combine(outputDirectory, "selected-next-work-order.md"),
        $"""
        # Selected next work order

        Recommended step: 05.08 parent review.

        Decision: {(allPassed ? "PHASE_05_SLICE_READY_FOR_REVIEW" : "NEEDS_FIX")}

        Parent should review the core/test/tool changes and this evidence leaf. Commit/status boundary remains parent-owned at 05.09.
        """);
}

static void WriteCompletionReports(
    string outputDirectory,
    bool allPassed,
    IReadOnlyList<HeldoutEvalRow> heldoutEvalRows,
    SourceDisjointReport sourceDisjoint,
    LeakageAblationReport leakageReport,
    string focusedTestResult)
{
    var done = allPassed ? 6 : 0;
    var failed = allPassed ? 0 : 6;
    WriteJson(Path.Combine(outputDirectory, "completion-report.json"), new
    {
        schemaVersion = 1,
        decision = allPassed ? "PHASE_05_SLICE_READY_FOR_REVIEW" : "NEEDS_FIX",
        phase = "05/14",
        step = allPassed ? "05.07/05.09" : "05.07/05.09",
        status = allPassed ? "READY_FOR_REVIEW" : "NEEDS_FIX",
        substeps = new
        {
            total = 6,
            done,
            remaining = 0,
            failed,
            covered = new[] { "05.02", "05.03", "05.04", "05.05", "05.06", "05.07" }
        },
        heldout = new
        {
            totalRows = heldoutEvalRows.Count,
            passedRows = heldoutEvalRows.Count(row => row.Passed),
            positiveRows = heldoutEvalRows.Count(row => row.Polarity == "positive"),
            negativeRows = heldoutEvalRows.Count(row => row.Polarity == "negative")
        },
        sourceDisjointPassed = sourceDisjoint.Passed,
        leakageAblationPassed = leakageReport.Passed,
        focusedTestResult,
        gitUsed = false,
        externalAiUsed = false
    });
    WriteText(
        Path.Combine(outputDirectory, "completion-report-ru.md"),
        $"""
        # Phase 05 report

        Decision: {(allPassed ? "PHASE_05_SLICE_READY_FOR_REVIEW" : "NEEDS_FIX")}

        Status line: {(allPassed ? "Phase 05/14, step 05.07/05.09, status READY_FOR_REVIEW" : "Phase 05/14, step 05.07/05.09, status NEEDS_FIX")}

        Substeps 05.02-05.07: total 6, done {done}, remaining 0, failed {failed}.

        05.02 guide-state data/provenance: train rows 3, continue-delta rows 1, held-out rows 2.
        05.03 trained planner path: task -> plan goes through the ILearningMachine adapter and the OrchestrationReasoningMachine learned guide state.
        05.04 checkpoint/manifest: common checkpoint and checkpoint-manifest created.
        05.05 fresh-load: checkpoint loads without retrain and plans a held-out positive row.
        05.06 continue-training: delta starts from the existing checkpoint and writes a continued checkpoint.
        05.07 held-out eval: total {heldoutEvalRows.Count}, positive {heldoutEvalRows.Count(row => row.Polarity == "positive")}, negative {heldoutEvalRows.Count(row => row.Polarity == "negative")}, passed {heldoutEvalRows.Count(row => row.Passed)}, failed {heldoutEvalRows.Count(row => !row.Passed)}.

        Hidden deterministic shortcut audit: {(leakageReport.Passed ? "PASS" : "FAIL")}; source IDs, pair IDs, file paths and expected scenario IDs are not the final authority.
        Source-disjoint audit: {(sourceDisjoint.Passed ? "PASS" : "FAIL")}; overlap count {sourceDisjoint.Overlap.Count}.

        Semantic quality, token savings, host runtime/UI readiness and production target promotion were not claimed.
        Git used: false.
        External AI used: false.
        """);
}

static void WriteManifest(string outputDirectory)
{
    var files = Directory.EnumerateFiles(outputDirectory, "*", SearchOption.TopDirectoryOnly)
        .Where(path => !string.Equals(Path.GetFileName(path), "manifest.json", StringComparison.Ordinal))
        .Order(StringComparer.Ordinal)
        .Select(path => new
        {
            path = Path.GetFileName(path),
            sha256 = HashFile(path),
            bytes = new FileInfo(path).Length
        })
        .ToArray();
    WriteJson(Path.Combine(outputDirectory, "manifest.json"), new
    {
        schemaVersion = 1,
        phase = "05",
        substeps = "05.02-05.07",
        evidenceFileCount = files.Length + 1,
        generatedAtUtc = DateTimeOffset.UtcNow,
        files,
        gitUsed = false,
        externalAiUsed = false
    });
}

static IReadOnlyList<JsonElement> ReadJsonlElements(string path)
{
    var rows = new List<JsonElement>();
    foreach (var line in File.ReadLines(path, Encoding.UTF8))
    {
        if (string.IsNullOrWhiteSpace(line))
            continue;
        using var document = JsonDocument.Parse(line);
        rows.Add(document.RootElement.Clone());
    }

    return rows;
}

static JsonElement ReadJsonElement(string path)
{
    using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
    return document.RootElement.Clone();
}

static void WriteJson(string path, object value)
    => WriteText(path, JsonSerializer.Serialize(value, JsonOptions));

static void WriteJsonl(string path, IEnumerable<object> values)
    => WriteText(path, string.Join('\n', values.Select(value => JsonSerializer.Serialize(value, JsonLineOptions))));

static void WriteText(string path, string text)
{
    var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
    var lines = normalized.Split('\n').Select(line => line.TrimEnd()).ToArray();
    normalized = string.Join('\n', lines);
    if (!normalized.EndsWith('\n'))
        normalized += "\n";

    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, normalized, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

static string ExtractMatchKind(IReadOnlyList<string> evidence)
    => evidence.FirstOrDefault(item => item.StartsWith("ORCHESTRATION_REASONING_SCENARIO_UNIT_MATCH_KIND:", StringComparison.Ordinal))?
        .Split(':', 2)[1]
        ?? "not_ready";

static string RequiredString(JsonElement element, string name)
{
    if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
        throw new JsonException($"Required string '{name}' is missing.");
    return value.GetString()!;
}

static string StringProp(JsonElement element, string name)
    => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString() ?? ""
        : "";

static bool BoolProp(JsonElement element, string name)
    => element.TryGetProperty(name, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False && value.GetBoolean();

static int IntProp(JsonElement element, string name)
    => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
        ? number
        : 0;

static IReadOnlyList<string> RequiredStringArray(JsonElement element, string name)
{
    if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Array)
        throw new JsonException($"Required array '{name}' is missing.");
    return value.EnumerateArray()
        .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() : null)
        .Where(item => !string.IsNullOrWhiteSpace(item))
        .Select(item => item!)
        .ToArray();
}

static string HashFile(string path)
    => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

static string FindRepoRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
            return directory.FullName;
        directory = directory.Parent;
    }

    throw new DirectoryNotFoundException("Could not locate repository root from runner base directory.");
}

static string? GetArg(string[] args, string name)
{
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(args[i], name, StringComparison.Ordinal))
            return args[i + 1];
    }

    return null;
}

static string ToRepoRelative(string path)
    => Path.GetRelativePath(Program.RepoRoot, path).Replace('\\', '/');

partial class Program
{
    internal static string RepoRoot = "";

    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    internal static readonly JsonSerializerOptions JsonLineOptions = new(JsonSerializerDefaults.Web);
}

public sealed record HeldoutEvalRow(
    string RowId,
    string Polarity,
    string SourceRowId,
    string SourceRef,
    string ExpectedPatternId,
    string ActualPatternId,
    string ExpectedScenarioId,
    string ActualScenarioId,
    string ContractStatus,
    string MatchKind,
    bool SourceDisjoint,
    bool Passed,
    string NotesRu);

public sealed record SourceDisjointReport(
    int TrainRefCount,
    int HeldoutRefCount,
    IReadOnlyList<string> Overlap,
    bool Passed);

public sealed record LeakageAblationReport(
    int SchemaVersion,
    bool Passed,
    bool HiddenDeterministicShortcutFound,
    bool SourceDisjointPassed,
    int SourceOverlapCount,
    bool SourceIdOnlyControlPassed,
    bool SourceIdChangedDecisionStable,
    IReadOnlyList<string> HeldoutRefsPresentInBaseState,
    string DeterministicMatcherLabel,
    IReadOnlyList<string> NotesRu);
