// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Text.Json;
using AgentInfrastructure.Logical;
using ProjectWikiDomain;

public sealed class OrchestrationPlanTests
{
    [Fact]
    public async Task OrchestrationPlan_FreshMachine_WorksWithoutCorpusDir()
    {
        var result = await new OrchestrationPlanMachine().GeneratePlanAsync("continue implementation");
        Assert.True(result.IsSuccess);
        Assert.Equal(GenerationRouteMode.Mode2Generalization, result.Value!.RouteMode);
    }

    [Fact]
    public void OrchestrationPlan_DetectsExplicitDashedFlowRef()
        => Assert.Contains("FLOW-33", FlowReferenceDetector.Detect("FLOW-33").AllRefs);

    [Fact]
    public void OrchestrationPlan_DetectsSpacedFlowRef()
        => Assert.Contains("FLOW-33", FlowReferenceDetector.Detect("flow 33").AllRefs);

    [Fact]
    public void OrchestrationPlan_DetectsStarFlowRef()
        => Assert.Contains("FLOW-33", FlowReferenceDetector.Detect("33* files").AllRefs);

    [Fact]
    public void OrchestrationPlan_DetectsDashStarFlowRef()
        => Assert.Contains("FLOW-31", FlowReferenceDetector.Detect("31-* documents").AllRefs);

    [Fact]
    public void OrchestrationPlan_DetectsNamedSuffixFlowRef()
        => Assert.Contains("FLOW-31", FlowReferenceDetector.Detect("31-functionality").AllRefs);

    [Fact]
    public void OrchestrationPlan_DetectsWordOrdinalFlowRef()
        => Assert.Contains("FLOW-01", FlowReferenceDetector.Detect("first flow").AllRefs);

    [Fact]
    public void OrchestrationPlan_DetectsHyphenatedDigitOrdinalFlowRef()
        => Assert.Contains("FLOW-01", FlowReferenceDetector.Detect("the 1-st flow").AllRefs);

    [Fact]
    public void OrchestrationPlan_DetectsForbiddenBareNumber()
        => Assert.Contains("FLOW-34", FlowReferenceDetector.Detect("33* files, 34 not now").ForbiddenCandidates);

    [Fact]
    public async Task Mode1_LearnedGrounding_ProducesFlowSpecificPlan()
    {
        var machine = await TrainedMachineAsync();
        var result = await machine.GeneratePlanAsync("flow 33");
        Assert.True(result.IsSuccess);
        Assert.Equal(GenerationRouteMode.LearnedFlowGrounding, result.Value!.RouteMode);
        Assert.True(result.Value.MlGenerated);
        Assert.Equal("FLOW-33", result.Value.FlowGrounding!.FlowRef);
        Assert.Contains("FLOW-33", result.Value.PlanText);
    }

    [Fact]
    public async Task OrchestrationPlan_DocumentationMigrationWithFlow_RoutesMode2()
    {
        var result = await (await TrainedMachineAsync()).GeneratePlanAsync("translate flow 33 from python/react native to node/react");
        Assert.Equal(GenerationRouteMode.Mode2Generalization, result.Value!.RouteMode);
    }

    [Fact]
    public async Task OrchestrationPlan_PhaseContinuationWithFlow_RoutesMode2()
    {
        var result = await (await TrainedMachineAsync()).GeneratePlanAsync("phase 1-2.4 are done, proceed 2.5 phase only for FLOW-33");
        Assert.Equal(GenerationRouteMode.Mode2Generalization, result.Value!.RouteMode);
        Assert.Contains("phase_scope", result.Value.PlanText);
    }

    [Fact]
    public async Task OrchestrationPlan_ArchitectureReviewWithFlow_RoutesMode2()
    {
        var result = await (await TrainedMachineAsync()).GeneratePlanAsync("review architecture for FLOW-33");
        Assert.Equal(GenerationRouteMode.Mode2Generalization, result.Value!.RouteMode);
    }

    [Fact]
    public async Task OrchestrationPlan_TestingPlanWithFlow_RoutesMode2()
    {
        var result = await (await TrainedMachineAsync()).GeneratePlanAsync("create testing plan for FLOW-33");
        Assert.Equal(GenerationRouteMode.Mode2Generalization, result.Value!.RouteMode);
    }

    [Fact]
    public async Task OrchestrationPlan_ExplanationKeyword_RoutesMode3()
    {
        var result = await (await TrainedMachineAsync()).GeneratePlanAsync("explain FLOW-33 orchestration");
        Assert.Equal(GenerationRouteMode.Mode3HumanExplanation, result.Value!.RouteMode);
    }

    [Fact]
    public async Task OrchestrationPlan_Mode2EvalPurity_HoldsInLoopReport()
    {
        var report = await RunTempLoopAsync(requireResume: false);
        Assert.All(report.Mode2EvalPurity, row => Assert.True(row.Passed));
    }

    [Fact]
    public void OrchestrationPlan_Mode3Split_HasNoFlowRefOverlap()
    {
        var pairs = OrchestrationCorpusBuilder.BuildPairs(SyntheticSources());
        var train = pairs.Where(p => p.Mode == PairMode.Mode3HumanExplanation && !p.IsHeldOut).Select(p => p.FlowRef).ToHashSet();
        var eval = pairs.Where(p => p.Mode == PairMode.Mode3HumanExplanation && p.IsHeldOut).Select(p => p.FlowRef).ToHashSet();
        Assert.False(train.Overlaps(eval));
    }

    [Fact]
    public void OrchestrationPlan_Mode3Split_HasRequiredCounts()
    {
        var pairs = OrchestrationCorpusBuilder.BuildPairs(SyntheticSources());
        Assert.True(pairs.Count(p => p.Mode == PairMode.Mode3HumanExplanation && !p.IsHeldOut) >= 30);
        Assert.True(pairs.Count(p => p.Mode == PairMode.Mode3HumanExplanation && p.IsHeldOut) >= 20);
    }

    [Fact]
    public async Task OrchestrationPlan_Loop_UsesOneCombinedBatchPerCycle()
    {
        var report = await RunTempLoopAsync(requireResume: false, maxCycles: 1);
        Assert.Equal(1, Assert.Single(report.Cycles).CombinedTrainBatchCalls);
    }

    [Fact]
    public async Task OrchestrationPlan_Loop_WritesCheckpoint()
    {
        var report = await RunTempLoopAsync(requireResume: false);
        Assert.True(report.CheckpointWritten);
    }

    [Fact]
    public async Task OrchestrationPlan_Resume_LoadsTrainedState()
    {
        var (_, resume) = await RunTempLoopWithResumeAsync();
        Assert.True(resume.TrainedStateLoaded);
    }

    [Fact]
    public async Task OrchestrationPlan_Resume_DoesNotTrainRestoredPairIds()
    {
        var (_, resume) = await RunTempLoopWithResumeAsync();
        Assert.Equal(0, resume.ResumeTrainBatchCalls);
    }

    [Fact]
    public async Task OrchestrationPlan_Resume_UsesTrainedPairIdsForExclusionOnly()
    {
        var (_, resume) = await RunTempLoopWithResumeAsync();
        Assert.True(resume.TrainedPairIdsUsedForExclusionOnly);
    }

    [Fact]
    public async Task OrchestrationPlan_Resume_RestoresPairIds()
    {
        var (_, resume) = await RunTempLoopWithResumeAsync();
        Assert.True(resume.TrainedPairIdsRestoredCount > 0);
        Assert.True(resume.Mode1FlowGroundingKnnSize > 0);
        Assert.True(resume.Mode1FlowGroundingWeightsSize > 0);
    }

    [Fact]
    public async Task OrchestrationPlan_Resume_ReportIncludesMode3PatternMapEvidence()
    {
        var (_, resume) = await RunTempLoopWithResumeAsync();
        Assert.True(resume.Mode3TrainRows > 0);
        Assert.True(resume.Mode3HumanExplanationPatternMapSize > 0);
    }

    [Fact]
    public async Task OrchestrationPlan_Report_OverallAccuracyEqualsMeanOfMode1AndMode2()
    {
        var report = await RunTempLoopAsync(requireResume: false);
        Assert.Equal((report.LearnedFlowGroundingAccuracy + report.UnseenStyleAccuracy) / 2.0, report.OverallTrainingAccuracy, precision: 6);
    }

    [Fact]
    public async Task OrchestrationPlan_Fixture_Flow33UsesLearnedGrounding()
    {
        var report = await RunTempLoopAsync(requireResume: false);
        Assert.Contains(report.FixtureResults, row => row.Fixture == "flow 33" && row.Passed && row.RouteMode == GenerationRouteMode.LearnedFlowGrounding);
    }

    [Fact]
    public async Task OrchestrationPlan_Report_IncludesUntrainedFlow33FallbackFixture()
    {
        var report = await RunTempLoopAsync(requireResume: false);
        Assert.Contains(report.FixtureResults, row =>
            row.Fixture == "fixture_untrained[\"flow 33\"]" &&
            row.Passed &&
            row.RouteMode == GenerationRouteMode.Mode2Generalization &&
            row.MlGenerated &&
            row.PredictedFlowRef is null);
    }

    [Fact]
    public async Task OrchestrationPlan_Fixture_OneStFlowPredictsFlow01()
    {
        var report = await RunTempLoopAsync(requireResume: false);
        Assert.Contains(report.FixtureResults, row => row.Fixture.Contains("1-st flow", StringComparison.Ordinal) && row.Passed && row.PredictedFlowRef == "FLOW-01");
    }

    [Fact]
    public async Task OrchestrationPlan_LoadPlanningSkills_Passthrough()
    {
        var result = await (await TrainedMachineAsync()).GeneratePlanAsync("load planning skills");
        Assert.Equal(GenerationRouteMode.Passthrough, result.Value!.RouteMode);
    }

    [Fact]
    public async Task ApprovalGateCompliance_IsMeasuredFromApprovalFixture()
    {
        var contract = HumanIntentBoundaryClassifier.Classify("proceed but do not continue until I approve");
        Assert.True(contract.RequiresApprovalGate);

        var report = await RunTempLoopAsync(requireResume: false, maxCycles: 1);
        Assert.Equal(1.0, report.ApprovalGateCompliance);
        Assert.Contains(report.FixtureResults, row =>
            row.Fixture == "proceed but do not continue until I approve" &&
            row.Passed &&
            row.Warnings.Contains("PLAN_ONLY_PENDING_APPROVAL"));
    }

    [Fact]
    public async Task OrchestrationPlan_CorpusBuild_HasV20Counts()
    {
        var root = TempRoot();
        var report = await OrchestrationCorpusBuilder.BuildAsync(root, null, Path.Combine(root, "pairs.jsonl"));
        Assert.True(report.Mode1TrainingPairs >= 42);
        Assert.True(report.Mode1HeldOutEvalPairs >= 20);
        Assert.True(report.Mode2TrainingPairs >= 200);
        Assert.True(report.Mode2HeldOutEvalPairs >= 40);
        Assert.True(report.MeetsCorpusGate);
    }

    [Fact]
    public async Task OrchestrationPlan_CorpusAudit_HasV20Schema()
    {
        var report = await OrchestrationCorpusAudit.AuditAsync(TempRoot());
        Assert.True(report.TrainingReadyCount >= 20);
        Assert.True(report.ExplanationReadyCount >= 10);
        Assert.False(report.PrivateRawContentWrittenToCommittedArtifacts);
    }

    [Fact]
    public async Task Mode1Grounding_ExportImport_Survives()
    {
        var machine = await TrainedMachineAsync();
        var checkpoint = machine.ExportCheckpoint();
        var fresh = new OrchestrationPlanMachine();
        Assert.True(fresh.ImportCheckpoint(checkpoint).IsSuccess);
        var result = await fresh.GeneratePlanAsync("flow 33");
        Assert.Equal(GenerationRouteMode.LearnedFlowGrounding, result.Value!.RouteMode);
        Assert.Equal("FLOW-33", result.Value.FlowGrounding!.FlowRef);
    }

    [Fact]
    public async Task Mode1Grounding_AfterRestore_DeliversTheBodyOfTheGroundedRoute()
    {
        // Repair R4 of the 27.10 transfer audit, IM row. The case above restores
        // a machine and checks which route it grounds on. The audit's warning is
        // about the step after that: the route can be right while the body handed
        // back belongs to another route, or to an older pair. So the body is what
        // is checked here, and against the training material rather than against
        // the model's own opinion.
        var machine = await TrainedMachineAsync();
        var fresh = new OrchestrationPlanMachine();
        Assert.True(fresh.ImportCheckpoint(machine.ExportCheckpoint()).IsSuccess);

        var result = await fresh.GeneratePlanAsync("flow 33");
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(GenerationRouteMode.LearnedFlowGrounding, result.Value!.RouteMode);
        Assert.Equal("FLOW-33", result.Value.FlowGrounding!.FlowRef);

        // The body is composed rather than copied, so it is read for what it
        // names. It must name the route that was grounded, and it must name no
        // other: a body taken from another route's pair would carry that
        // route's own identifier.
        Assert.Contains("flow_ref=FLOW-33", result.Value.PlanText, StringComparison.Ordinal);

        var otherRoutes = OrchestrationCorpusBuilder.BuildPairs(SyntheticSources())
            .Where(pair => !pair.IsHeldOut && pair.Mode == PairMode.Mode1LearnedFlowGrounding)
            .Select(pair => pair.FlowRef)
            .Where(flowRef => !string.Equals(flowRef, "FLOW-33", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(otherRoutes);
        foreach (var other in otherRoutes)
            Assert.DoesNotContain(other, result.Value.PlanText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Mode1Grounding_Ablation_EmptyWeightsDegrade()
    {
        var machine = await TrainedMachineAsync();
        var checkpoint = machine.ExportCheckpoint() with
        {
            FlowGroundingWeights = new Dictionary<string, OrchestrationFlowGroundingWeights>(StringComparer.Ordinal)
        };
        var ablated = new OrchestrationPlanMachine();
        Assert.True(ablated.ImportCheckpoint(checkpoint).IsSuccess);
        var result = await ablated.GeneratePlanAsync("flow 33");
        Assert.NotEqual(GenerationRouteMode.LearnedFlowGrounding, result.Value!.RouteMode);
    }

    [Fact]
    public async Task Mode2_ExportImport_GeneralizationSurvives()
    {
        var machine = await TrainedMachineAsync();
        var fresh = new OrchestrationPlanMachine();
        Assert.True(fresh.ImportCheckpoint(machine.ExportCheckpoint()).IsSuccess);
        var result = await fresh.GeneratePlanAsync("translate flow 33 from python/react native to node/react");
        Assert.Equal(GenerationRouteMode.Mode2Generalization, result.Value!.RouteMode);
    }

    [Fact]
    public async Task ExactPromptOverlap_FailsGate()
    {
        var root = TempRoot();
        var pairs = OrchestrationCorpusBuilder.BuildPairs(SyntheticSources()).ToList();
        var duplicate = pairs.First(pair => pair.Mode == PairMode.Mode1LearnedFlowGrounding && !pair.IsHeldOut);
        pairs.Add(duplicate with { PairId = "mode1-eval-overlap", IsHeldOut = true });
        var pairsPath = Path.Combine(root, "pairs.jsonl");
        Directory.CreateDirectory(root);
        await WritePairsAsync(pairsPath, pairs);
        var report = await OrchestrationPilotLoopRunner.RunAsync(new OrchestrationLoopRunOptions(
            pairsPath,
            50,
            50,
            20,
            1,
            0.75,
            0.75,
            0.70,
            Path.Combine(root, "checkpoint.json"),
            false,
            Path.Combine(root, "report.json")));
        Assert.True(report.ExactPromptLeakageRate > 0.0);
        Assert.NotEmpty(report.ExactPromptLeakagePairs);
        Assert.False(report.PassesGate);
    }

    [Fact]
    public async Task ContinueTraining_AfterImport_AddsNewPairs()
    {
        var machine = await TrainedMachineAsync(mode2Limit: 10);
        var before = machine.Mode2KnnStoreSize;
        var pairs = OrchestrationCorpusBuilder.BuildPairs(SyntheticSources())
            .Where(pair => pair.Mode == PairMode.Mode2Generalization && !pair.IsHeldOut)
            .Skip(20)
            .Take(10)
            .ToArray();
        var result = await machine.TrainOrchestrationBatchAsync(new OrchestrationTrainingBatch("continue", pairs));
        Assert.True(result.IsSuccess);
        Assert.True(machine.Mode2KnnStoreSize > before);
    }

    [Fact]
    public async Task OrchestrationPlanMachine_TrainBatchDoesNotInventAccuracyAtExport()
    {
        var machine = new OrchestrationPlanMachine();
        var trained = await machine.TrainBatchAsync(SingleTrainingBatch("opm-01-train-only"));
        Assert.True(trained.IsSuccess);
        Assert.Equal(0.0, trained.Value!.AccuracyBefore);
        Assert.Equal(0.0, trained.Value.AccuracyAfter);
        Assert.True(machine.TrainingProgressScore > 0.0);

        var export = await machine.ExportAsync();
        Assert.True(export.IsSuccess);
        Assert.Equal(0.0, export.Value!.AccuracyAtExport);
        Assert.Contains("baseline_unverified", export.Value.ManifestRef);

        var checkpoint = machine.ExportCheckpoint();
        Assert.True(checkpoint.TrainingProgressScore > 0.0);
        Assert.Null(checkpoint.LastEvaluationGate);

        var fresh = new OrchestrationPlanMachine();
        Assert.True(fresh.ImportCheckpoint(checkpoint).IsSuccess);
        Assert.True(fresh.TrainingProgressScore > 0.0);
        Assert.Null(fresh.LastEvaluationGate);
        var freshExport = await fresh.ExportAsync();
        Assert.Equal(0.0, freshExport.Value!.AccuracyAtExport);
    }

    [Fact]
    public async Task OrchestrationPlanMachine_ExportLabelsUnverifiedWhenNoEvalGatePassed()
    {
        var machine = new OrchestrationPlanMachine();

        var export = await machine.ExportAsync();
        Assert.True(export.IsSuccess);
        Assert.Equal(0.0, export.Value!.AccuracyAtExport);
        Assert.Contains("baseline_unverified", export.Value.CheckpointRef);
        Assert.Contains("baseline_unverified", export.Value.ManifestRef);

        var path = Path.Combine(TempRoot(), "opm-01-unverified-checkpoint.json");
        var saved = await machine.SaveCheckpointAsync(path);
        Assert.True(saved.IsSuccess);
        Assert.Equal(0.0, saved.Value!.AccuracyAtExport);
        Assert.Equal(Path.GetFullPath(path), saved.Value.CheckpointRef);
        Assert.Contains("baseline_unverified", saved.Value.ManifestRef);

        var loaded = await OrchestrationPlanMachine.LoadCheckpointAsync(path);
        Assert.True(loaded.IsSuccess);
        Assert.Null(loaded.Value!.Machine.LastEvaluationGate);
    }

    [Fact]
    public async Task OrchestrationPlanMachine_ExportCarriesLastPassedEvaluationProvenance()
    {
        var machine = new OrchestrationPlanMachine();
        var evaluation = await machine.EvaluateAsync(PassingEvaluationSet("eval-pass-opm-01"));
        Assert.True(evaluation.IsSuccess);
        Assert.True(evaluation.Value!.PassesGate);
        Assert.Equal(1.0, evaluation.Value.Accuracy);

        var export = await machine.ExportAsync();
        Assert.True(export.IsSuccess);
        Assert.Equal(1.0, export.Value!.AccuracyAtExport);
        Assert.Contains("eval-pass-opm-01", export.Value.CheckpointRef);
        Assert.Contains("eval-pass-opm-01", export.Value.ManifestRef);
        Assert.DoesNotContain("baseline_unverified", export.Value.ManifestRef);

        var checkpoint = machine.ExportCheckpoint();
        Assert.NotNull(checkpoint.LastEvaluationGate);
        Assert.Equal("eval-pass-opm-01", checkpoint.LastEvaluationGate!.EvaluationId);
        Assert.Equal(1.0, checkpoint.LastEvaluationGate.Accuracy);
        Assert.True(checkpoint.LastEvaluationGate.PassesGate);
        Assert.False(checkpoint.LastEvaluationGate.SemanticQualityClaimed);

        var fresh = new OrchestrationPlanMachine();
        Assert.True(fresh.ImportCheckpoint(checkpoint).IsSuccess);
        var freshExport = await fresh.ExportAsync();
        Assert.Equal(1.0, freshExport.Value!.AccuracyAtExport);
        Assert.Contains("eval-pass-opm-01", freshExport.Value.ManifestRef);
    }

    [Fact]
    public async Task OrchestrationPlanMachine_TrainingAfterPassedEvaluationInvalidatesExportAccuracy()
    {
        var machine = new OrchestrationPlanMachine();
        var evaluation = await machine.EvaluateAsync(PassingEvaluationSet("eval-stale-opm-01"));
        Assert.True(evaluation.IsSuccess);
        Assert.True(evaluation.Value!.PassesGate);

        var passedExport = await machine.ExportAsync();
        Assert.True(passedExport.IsSuccess);
        Assert.True(passedExport.Value!.AccuracyAtExport > 0.0);
        Assert.Contains("eval-stale-opm-01", passedExport.Value.ManifestRef);

        var training = await machine.TrainBatchAsync(SingleTrainingBatch("opm-01-stale-gate-train"));
        Assert.True(training.IsSuccess);

        var staleExport = await machine.ExportAsync();
        Assert.True(staleExport.IsSuccess);
        Assert.Equal(0.0, staleExport.Value!.AccuracyAtExport);
        Assert.Contains("baseline_unverified", staleExport.Value.ManifestRef);
        Assert.True(machine.LastEvaluationGate is null or { PassesGate: false });
    }

    [Fact]
    public async Task OrchestrationPlanMachine_EvaluateAccuracyIsTheOnlyQualityGate()
    {
        var machine = new OrchestrationPlanMachine();
        var trained = await machine.TrainBatchAsync(SingleTrainingBatch("opm-01-quality-gate"));
        Assert.True(trained.IsSuccess);
        Assert.True(machine.TrainingProgressScore > 0.0);

        var trainOnlyExport = await machine.ExportAsync();
        Assert.Equal(0.0, trainOnlyExport.Value!.AccuracyAtExport);

        var failedEvaluation = await machine.EvaluateAsync(FailingEvaluationSet("eval-fail-opm-01"));
        Assert.True(failedEvaluation.IsSuccess);
        Assert.False(failedEvaluation.Value!.PassesGate);
        Assert.NotNull(machine.LastEvaluationGate);
        Assert.False(machine.LastEvaluationGate!.PassesGate);

        var failedExport = await machine.ExportAsync();
        Assert.Equal(0.0, failedExport.Value!.AccuracyAtExport);
        Assert.Contains("baseline_unverified", failedExport.Value.ManifestRef);

        var passedEvaluation = await machine.EvaluateAsync(PassingEvaluationSet("eval-pass-quality-opm-01"));
        Assert.True(passedEvaluation.IsSuccess);
        Assert.True(passedEvaluation.Value!.PassesGate);

        var passedExport = await machine.ExportAsync();
        Assert.Equal(passedEvaluation.Value.Accuracy, passedExport.Value!.AccuracyAtExport);
        Assert.Contains("eval-pass-quality-opm-01", passedExport.Value.ManifestRef);
        Assert.False(machine.LastEvaluationGate!.SemanticQualityClaimed);
    }

    [Fact]
    public void NoRuntimeFlowReferenceDetectorUsage()
    {
        Assert.DoesNotContain(nameof(FlowReferenceDetector), ReadProjectFile("42.ProjectWikiDomain", "OrchestrationPlanMachine.cs"));
        Assert.DoesNotContain(nameof(FlowReferenceDetector), ReadProjectFile("42.ProjectWikiDomain", "TrainedRouteComposerUnit.cs"));
        Assert.DoesNotContain(nameof(FlowReferenceDetector), ReadProjectFile("42.ProjectWikiDomain", "HumanIntentBoundaryClassifier.cs"));
    }

    [Fact]
    public async Task PromptFlowGroundingModel_PredictsFlowRefWithoutHint()
    {
        var model = new PromptFlowGroundingModel();
        var pair = OrchestrationCorpusBuilder.BuildPairs(SyntheticSources())
            .First(row => row.Mode == PairMode.Mode1LearnedFlowGrounding && row.FlowRef == "FLOW-33" && !row.IsHeldOut);
        model.Train(pair);
        var result = model.TryGround("flow 33", HumanIntentContract.Empty);
        Assert.NotNull(result);
        Assert.Equal("FLOW-33", result!.FlowRef);
    }

    [Fact]
    public void PromptFlowGroundingModel_DoesNotGroundFromChosenTitleOnly()
    {
        var model = new PromptFlowGroundingModel();
        model.Train(TargetLeakageTrainingPair(
            "pfg-01-title-only",
            "FLOW-88",
            "ordinary source prompt",
            "Unique Descriptor Alpha"));

        var result = model.TryGround("Unique Descriptor Alpha", HumanIntentContract.Empty);

        Assert.Null(result);
        var weights = Assert.Single(model.ExportWeights()).Value;
        Assert.DoesNotContain("unique", weights.InputFeatureWeights.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("unique", weights.LabelDescriptorFeatures.Keys, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void PromptFlowGroundingModel_DoesNotUsePredictedFlowRefAsKnnQueryFeature()
    {
        var model = new PromptFlowGroundingModel();
        model.Train(TargetLeakageTrainingPair(
            "pfg-01-knn-query",
            "FLOW-88",
            "FLOW-88",
            "Neutral Descriptor"));
        var grounding = new FlowGroundingResult("FLOW-88", 0.95, [], [], [], []);

        var result = model.FindLearnedPair(string.Empty, HumanIntentContract.Empty, grounding);

        Assert.Null(result);
    }

    [Fact]
    public void PromptFlowGroundingModel_ConfidenceUsesInputOnlyFeatures()
    {
        var leakyTitleModel = new PromptFlowGroundingModel();
        leakyTitleModel.Train(TargetLeakageTrainingPair(
            "pfg-01-confidence-leaky-title",
            "FLOW-88",
            "anchor evidence",
            "Descriptor Alpha"));
        var neutralTitleModel = new PromptFlowGroundingModel();
        neutralTitleModel.Train(TargetLeakageTrainingPair(
            "pfg-01-confidence-neutral-title",
            "FLOW-88",
            "anchor evidence",
            "Different Target Title"));

        var leakyTitleResult = leakyTitleModel.TryGround("anchor descriptor alpha", HumanIntentContract.Empty);
        var neutralTitleResult = neutralTitleModel.TryGround("anchor descriptor alpha", HumanIntentContract.Empty);

        Assert.NotNull(leakyTitleResult);
        Assert.NotNull(neutralTitleResult);
        Assert.Equal(neutralTitleResult!.Confidence, leakyTitleResult!.Confidence, precision: 6);
        var weights = Assert.Single(leakyTitleModel.ExportWeights()).Value;
        Assert.DoesNotContain("descriptor", weights.InputFeatureWeights.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("descriptor", weights.LabelDescriptorFeatures.Keys, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OrchestrationPlanMachine_Mode1GroundingRejectsTargetLabelLeakageRows()
    {
        var machine = new OrchestrationPlanMachine();
        var trained = await machine.TrainOrchestrationBatchAsync(new OrchestrationTrainingBatch(
            "pfg-01-target-label-leakage",
            [
                TargetLeakageTrainingPair(
                    "pfg-01-machine-row",
                    "FLOW-88",
                    "ordinary source prompt",
                    "Unique Descriptor Alpha")
            ]));
        Assert.True(trained.IsSuccess);

        var result = await machine.GeneratePlanAsync("Unique Descriptor Alpha");

        Assert.True(result.IsSuccess);
        Assert.NotEqual(GenerationRouteMode.LearnedFlowGrounding, result.Value!.RouteMode);
        Assert.Null(result.Value.FlowGrounding);
        Assert.DoesNotContain("LEARNED_FLOW_GROUNDING_PLAN", result.Value.PlanText);
    }

    [Fact]
    public void NoRemovedFlowContractsRemain()
    {
        var contracts = ReadProjectFile("42.ProjectWikiDomain", "OrchestrationPlanContracts.cs");
        var removedTerms = new[]
        {
            string.Concat("Prompt", "Flow", "Index"),
            string.Concat("_", "flow", "Index"),
            string.Concat("Flow", "Index", "Ref"),
            string.Concat("Retrieval", "Source"),
            string.Concat("Known", "Flow", "Retrieval")
        };
        Assert.All(removedTerms, term => Assert.DoesNotContain(term, contracts));
    }

    [Fact]
    public async Task Checkpoint_ContainsPromptGroundingState()
    {
        var machine = await TrainedMachineAsync();
        var checkpoint = machine.ExportCheckpoint();
        Assert.NotEmpty(checkpoint.FlowGroundingKnn);
        Assert.NotEmpty(checkpoint.FlowGroundingWeights);
        Assert.NotEmpty(checkpoint.KnnStore);
        Assert.NotEmpty(checkpoint.HumanExplanationKnn);
    }

    [Fact]
    public void HumanIntentBoundaryClassifier_IsPolicyGuard()
    {
        var contract = HumanIntentBoundaryClassifier.Classify("33* files, 34 not now");
        Assert.Contains("FLOW-33", contract.AllowedFlowScope);
        Assert.Contains("FLOW-34", contract.ForbiddenFlowScope);
        Assert.NotEmpty(contract.RequestedFlowTextSpans);
    }

    [Fact]
    public async Task ForbiddenScope_DoesNotPromoteForbiddenFlow()
    {
        var result = await (await TrainedMachineAsync()).GeneratePlanAsync("33* files, 34 not now");
        Assert.Equal(GenerationRouteMode.LearnedFlowGrounding, result.Value!.RouteMode);
        Assert.Equal("FLOW-33", result.Value.FlowGrounding!.FlowRef);
        var planLines = result.Value.PlanText.Split('\n').Select(line => line.TrimEnd('\r')).ToArray();
        Assert.DoesNotContain("target_flow=FLOW-34", planLines);
        Assert.DoesNotContain("flow_scope=FLOW-34", planLines);
        Assert.DoesNotContain("FLOW-34", result.Value.PlanText);
    }

    [Fact]
    public async Task Mode1EvalPurity_AllRowsUseLearnedGrounding()
    {
        var report = await RunTempLoopAsync(requireResume: false);
        Assert.All(report.Mode1EvalPurity, row => Assert.True(row.Passed));
    }

    [Fact]
    public async Task FreshUntrainedMachine_DoesNotGroundFlow33()
    {
        var result = await new OrchestrationPlanMachine().GeneratePlanAsync("flow 33");
        Assert.NotEqual(GenerationRouteMode.LearnedFlowGrounding, result.Value!.RouteMode);
        Assert.Null(result.Value.FlowGrounding);
        Assert.DoesNotContain("LEARNED_FLOW_GROUNDING_PLAN", result.Value.PlanText);
    }

    [Fact]
    public async Task TrainedMachine_FixtureFlow33ProducesLearnedStructure()
    {
        var result = await (await TrainedMachineAsync()).GeneratePlanAsync("flow 33");
        Assert.Contains("LEARNED_FLOW_GROUNDING_PLAN", result.Value!.PlanText);
        Assert.Equal("FLOW-33", result.Value.FlowGrounding!.FlowRef);
    }

    private static async Task<OrchestrationPlanMachine> TrainedMachineAsync(int mode2Limit = 200)
    {
        var machine = new OrchestrationPlanMachine();
        var all = OrchestrationCorpusBuilder.BuildPairs(SyntheticSources());
        var allowedMode2 = all
            .Where(pair => pair.Mode == PairMode.Mode2Generalization && !pair.IsHeldOut)
            .Take(mode2Limit)
            .Select(pair => pair.PairId)
            .ToHashSet(StringComparer.Ordinal);
        var pairs = all
            .Where(pair => !pair.IsHeldOut &&
                           (pair.Mode != PairMode.Mode2Generalization ||
                            allowedMode2.Contains(pair.PairId)))
            .ToArray();
        var result = await machine.TrainOrchestrationBatchAsync(new OrchestrationTrainingBatch("train", pairs));
        Assert.True(result.IsSuccess);
        return machine;
    }

    private static LearningBatch<OrchestrationPlanDomain> SingleTrainingBatch(string batchId)
    {
        var pair = SingleMode2TrainingPair($"{batchId}-pair");
        var chosen = JsonSerializer.Serialize(pair, OrchestrationPlanJsonOptions.Standard);
        return new LearningBatch<OrchestrationPlanDomain>(
            batchId,
            OrchestrationPlanMachine.DomainTag,
            [
                new LearningTrainingItem(
                    $"{batchId}-item",
                    pair.Prompt,
                    chosen,
                    pair.RejectedText)
            ],
            ["opm-01-test-source"]);
    }

    private static OrchestrationTrainingPair SingleMode2TrainingPair(string pairId)
        => new(
            pairId,
            PairMode.Mode2Generalization,
            "continue implementation",
            new OrchestrationPlanChosen(
                "FLOW-33",
                "Flow 33 implementation plan",
                "docs/flows/FLOW-33.md",
                "PLAN BODY"),
            "rejected plan",
            GenerationRouteMode.Mode2Generalization,
            BoundarySource.Classifier,
            true,
            IntentKind.ImplementationExecutionRequest,
            "FLOW-33",
            "opm-01-bundle",
            false);

    private static OrchestrationTrainingPair TargetLeakageTrainingPair(
        string pairId,
        string flowRef,
        string prompt,
        string title)
        => new(
            pairId,
            PairMode.Mode1LearnedFlowGrounding,
            prompt,
            new OrchestrationPlanChosen(
                flowRef,
                title,
                $"docs/flows/{flowRef}.md",
                $"PLAN BODY {flowRef}"),
            "Rejected route uses target-label leakage.",
            GenerationRouteMode.LearnedFlowGrounding,
            BoundarySource.Classifier,
            true,
            IntentKind.KnownFlowPlanning,
            flowRef,
            $"pfg-01-bundle-{flowRef}",
            false);

    private static LearningEvaluationSet<OrchestrationPlanDomain> PassingEvaluationSet(string evaluationId)
        => new(
            evaluationId,
            OrchestrationPlanMachine.DomainTag,
            [
                new LearningEvaluationItem(
                    $"{evaluationId}-item",
                    "continue implementation",
                    "ORCHESTRATION_PLAN")
            ]);

    private static LearningEvaluationSet<OrchestrationPlanDomain> FailingEvaluationSet(string evaluationId)
        => new(
            evaluationId,
            OrchestrationPlanMachine.DomainTag,
            [
                new LearningEvaluationItem(
                    $"{evaluationId}-item",
                    "continue implementation",
                    "THIS_TEXT_SHOULD_NOT_APPEAR_IN_THE_PLAN")
            ]);

    private static OrchestrationCorpusSources SyntheticSources()
        => OrchestrationCorpusAudit.LoadCorpusSources(Path.Combine(Path.GetTempPath(), "missing-orchestration-source"));

    private static async Task<OrchestrationLoopReport> RunTempLoopAsync(
        bool requireResume,
        int maxCycles = 3,
        string? root = null,
        string? checkpoint = null)
    {
        root ??= TempRoot();
        Directory.CreateDirectory(root);
        var pairsPath = Path.Combine(root, "pairs.jsonl");
        var build = await OrchestrationCorpusBuilder.BuildAsync(root, null, pairsPath);
        Assert.True(build.MeetsCorpusGate);
        checkpoint ??= Path.Combine(root, "checkpoint.json");
        return await OrchestrationPilotLoopRunner.RunAsync(new OrchestrationLoopRunOptions(
            pairsPath,
            50,
            50,
            20,
            maxCycles,
            0.75,
            0.75,
            0.70,
            checkpoint,
            requireResume,
            Path.Combine(root, requireResume ? "resume-report.json" : "report.json")));
    }

    private static async Task<(OrchestrationLoopReport First, OrchestrationLoopReport Resume)> RunTempLoopWithResumeAsync()
    {
        var root = TempRoot();
        var checkpoint = Path.Combine(root, "checkpoint.json");
        var first = await RunTempLoopAsync(requireResume: false, root: root, checkpoint: checkpoint);
        var resume = await RunTempLoopAsync(requireResume: true, root: root, checkpoint: checkpoint);
        return (first, resume);
    }

    private static async Task WritePairsAsync(string path, IReadOnlyList<OrchestrationTrainingPair> pairs)
    {
        var options = new JsonSerializerOptions(OrchestrationPlanJsonOptions.Standard)
        {
            WriteIndented = false
        };
        await using var stream = File.Create(path);
        await using var writer = new StreamWriter(stream, System.Text.Encoding.UTF8);
        foreach (var pair in pairs)
            await writer.WriteLineAsync(JsonSerializer.Serialize(pair, options));
    }

    private static string ReadProjectFile(params string[] parts)
        => File.ReadAllText(Path.Combine(ProjectRoot(), Path.Combine(parts)));

    private static string ProjectRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "AGENTS.md")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Project root not found.");
    }

    private static string TempRoot()
        => Path.Combine(Path.GetTempPath(), $"orchestration-plan-tests-{Guid.NewGuid():N}");
}
