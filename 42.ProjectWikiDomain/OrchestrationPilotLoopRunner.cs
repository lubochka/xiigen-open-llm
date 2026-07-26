// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.Json;

public static class OrchestrationPilotLoopRunner
{
    private const string ApprovalGateFixture = "proceed but do not continue until I approve";

    public static async Task<OrchestrationLoopReport> RunAsync(
        OrchestrationLoopRunOptions options,
        CancellationToken ct = default)
    {
        if (!File.Exists(options.PrivatePairsPath))
            throw new FileNotFoundException("Private orchestration pair file not found.", options.PrivatePairsPath);
        if (options.TrainSetSize <= 0 || options.TestSetSize <= 0 || options.Mode3TrainSetSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Train and test sizes must be positive.");
        if (options.MaxCycles < 0)
            throw new ArgumentOutOfRangeException(nameof(options), "MaxCycles must not be negative.");

        var pairs = await OrchestrationCorpusBuilder.ReadPairsAsync(options.PrivatePairsPath, ct);
        var checkpointPath = string.IsNullOrWhiteSpace(options.CheckpointPath)
            ? null
            : Path.GetFullPath(options.CheckpointPath);

        OrchestrationPlanMachine machine;
        OrchestrationLoopCheckpoint? loop = null;
        var checkpointLoaded = false;
        var checkpointWritten = false;
        var trainedStateLoaded = false;
        var resumeMode = "fresh";
        var replayedCycles = 0;
        var initialTrainingSkippedDueCheckpoint = false;
        var resumeTrainBatchCalls = 0;
        var trainedPairIdsUsedForExclusionOnly = false;
        var trainedPairIdsRestoredCount = 0;
        var mode1Restored = 0;
        var mode2Restored = 0;
        var mode3Restored = 0;
        var cursor = 0;
        var lastCompletedCycle = 0;

        if (checkpointPath is not null && File.Exists(checkpointPath))
        {
            var load = await OrchestrationPlanMachine.LoadCheckpointAsync(checkpointPath, ct);
            if (!load.IsSuccess)
                throw new InvalidOperationException($"Failed to load checkpoint: {load.ErrorMessage} ({load.ErrorCode})");

            machine = load.Value.Machine;
            loop = load.Value.Loop;
            checkpointLoaded = true;
            trainedStateLoaded = true;
            resumeMode = "checkpoint";
            initialTrainingSkippedDueCheckpoint = options.RequireCheckpointResume;
            trainedPairIdsUsedForExclusionOnly = options.RequireCheckpointResume;
            trainedPairIdsRestoredCount = machine.TrainedPairIds.Count;
            mode1Restored = machine.Mode1PromotedPairIds.Count;
            mode2Restored = machine.Mode2PromotedPairIds.Count;
            mode3Restored = machine.Mode3PromotedPairIds.Count;
            cursor = loop?.Cursor ?? trainedPairIdsRestoredCount;
            lastCompletedCycle = loop?.LastCompletedCycle ?? 0;
        }
        else if (options.RequireCheckpointResume)
        {
            throw new FileNotFoundException($"Checkpoint required but not found: {checkpointPath}", checkpointPath);
        }
        else
        {
            machine = new OrchestrationPlanMachine();
        }

        var cycles = new List<OrchestrationLoopCycleReport>();
        if (!options.RequireCheckpointResume)
        {
            var minimumCycles = Math.Min(options.MaxCycles, 3);
            for (var i = 0; i < options.MaxCycles; i++)
            {
                var cycleNumber = lastCompletedCycle + 1;
                var selectedMode1 = SelectTrainPairs(
                    pairs,
                    PairMode.Mode1LearnedFlowGrounding,
                    machine.TrainedPairIds,
                    options.TrainSetSize);
                var selectedMode2 = SelectTrainPairs(
                    pairs,
                    PairMode.Mode2Generalization,
                    machine.TrainedPairIds,
                    options.TrainSetSize);
                var selectedMode3 = SelectTrainPairs(
                    pairs,
                    PairMode.Mode3HumanExplanation,
                    machine.TrainedPairIds,
                    options.Mode3TrainSetSize);
                var combined = selectedMode1.Concat(selectedMode2).Concat(selectedMode3).ToArray();

                if (combined.Length > 0)
                {
                    var train = await machine.TrainOrchestrationBatchAsync(
                        new OrchestrationTrainingBatch($"orchestration-cycle-{cycleNumber:D3}", combined),
                        ct);
                    if (!train.IsSuccess)
                        throw new InvalidOperationException(train.ErrorMessage);
                    cursor += train.Value!.RowsTrained;
                }

                lastCompletedCycle = cycleNumber;
                if (checkpointPath is not null)
                {
                    loop = new OrchestrationLoopCheckpoint(
                        "private-pairs-sha256:" + HashFile(options.PrivatePairsPath),
                        lastCompletedCycle,
                        cursor,
                        machine.TrainedPairIds.Order(StringComparer.Ordinal).ToArray(),
                        machine.Mode1PromotedPairIds.Order(StringComparer.Ordinal).ToArray(),
                        machine.Mode2PromotedPairIds.Order(StringComparer.Ordinal).ToArray(),
                        machine.Mode3PromotedPairIds.Order(StringComparer.Ordinal).ToArray());
                    var save = await machine.SaveCheckpointAsync(checkpointPath, loop, ct);
                    if (!save.IsSuccess)
                        throw new InvalidOperationException($"Failed to save checkpoint: {save.ErrorMessage} ({save.ErrorCode})");
                    checkpointWritten = true;
                }

                var metrics = await EvaluateAsync(machine, pairs, ct);
                var cycle = new OrchestrationLoopCycleReport(
                    cycleNumber,
                    selectedMode1.Count,
                    selectedMode2.Count,
                    selectedMode3.Count,
                    combined.Length == 0 ? 0 : 1,
                    checkpointWritten,
                    metrics.LearnedFlowGroundingAccuracy,
                    metrics.OverallTrainingAccuracy,
                    metrics.UnseenStyleAccuracy,
                    metrics.HumanExplanationAccuracy,
                    metrics.ExactPromptLeakageRate,
                    metrics.ForbiddenScopeViolationRate,
                    metrics.PhaseOnlyCompliance,
                    metrics.ApprovalGateCompliance,
                    MetricsPass(metrics, options));
                cycles.Add(cycle);

                if (cycle.PassesTarget && cycles.Count >= minimumCycles)
                    break;
                if (combined.Length == 0 && cycles.Count >= minimumCycles)
                    break;
            }
        }
        else
        {
            resumeTrainBatchCalls = 0;
        }

        var finalMetrics = await EvaluateAsync(machine, pairs, ct);
        var finalLoop = new OrchestrationLoopCheckpoint(
            "private-pairs-sha256:" + HashFile(options.PrivatePairsPath),
            lastCompletedCycle,
            cursor,
            machine.TrainedPairIds.Order(StringComparer.Ordinal).ToArray(),
            machine.Mode1PromotedPairIds.Order(StringComparer.Ordinal).ToArray(),
            machine.Mode2PromotedPairIds.Order(StringComparer.Ordinal).ToArray(),
            machine.Mode3PromotedPairIds.Order(StringComparer.Ordinal).ToArray());

        if (checkpointPath is not null)
        {
            var save = await machine.SaveCheckpointAsync(checkpointPath, finalLoop, ct);
            if (!save.IsSuccess)
                throw new InvalidOperationException($"Failed to save checkpoint: {save.ErrorMessage} ({save.ErrorCode})");
            checkpointWritten = true;
        }

        var passesGate = MetricsPass(finalMetrics, options);
        var report = new OrchestrationLoopReport(
            Path.GetFullPath(options.PrivatePairsPath),
            pairs.Count,
            options.TrainSetSize,
            options.TestSetSize,
            options.Mode3TrainSetSize,
            options.MaxCycles,
            options.FlowGroundingTarget,
            options.StyleTarget,
            options.ExplanationTarget,
            checkpointLoaded,
            checkpointWritten,
            trainedStateLoaded,
            resumeMode,
            replayedCycles,
            initialTrainingSkippedDueCheckpoint,
            resumeTrainBatchCalls,
            trainedPairIdsUsedForExclusionOnly,
            trainedPairIdsRestoredCount,
            mode1Restored,
            mode2Restored,
            mode3Restored,
            finalLoop.Cursor,
            finalLoop.LastCompletedCycle,
            machine.Mode1FlowGroundingKnnSize,
            machine.Mode1FlowGroundingWeightsSize,
            machine.Mode2KnnStoreSize,
            machine.Mode2PatternMapSize,
            machine.Mode3KnnStoreSize,
            machine.Mode3HumanExplanationPatternMapSize,
            machine.Mode3PromotedPairIds.Count,
            finalMetrics.LearnedFlowGroundingAccuracy,
            finalMetrics.OverallTrainingAccuracy,
            finalMetrics.UnseenStyleAccuracy,
            finalMetrics.HumanExplanationAccuracy,
            finalMetrics.ExactPromptLeakageRate,
            finalMetrics.ExactPromptLeakagePairs,
            finalMetrics.ForbiddenScopeViolationRate,
            finalMetrics.PhaseOnlyCompliance,
            finalMetrics.ApprovalGateCompliance,
            passesGate,
            passesGate,
            cycles,
            finalMetrics.FixtureResults,
            finalMetrics.Mode1EvalPurity,
            finalMetrics.Mode2EvalPurity,
            finalMetrics.Mode3EvalPurity);

        if (!string.IsNullOrWhiteSpace(options.ReportPath))
            await WriteReportAsync(report, options.ReportPath, ct);

        return report;
    }

    public static async Task WriteReportAsync(
        OrchestrationLoopReport report,
        string path,
        CancellationToken ct = default)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(
            path,
            JsonSerializer.Serialize(report, OrchestrationPlanJsonOptions.Standard) + Environment.NewLine,
            System.Text.Encoding.UTF8,
            ct);
    }

    private static IReadOnlyList<OrchestrationTrainingPair> SelectTrainPairs(
        IReadOnlyList<OrchestrationTrainingPair> pairs,
        PairMode mode,
        IReadOnlyCollection<string> trainedPairIds,
        int count)
        => pairs
            .Where(pair => pair.Mode == mode && !pair.IsHeldOut && !trainedPairIds.Contains(pair.PairId))
            .Take(count)
            .ToArray();

    private static async Task<EvaluationMetrics> EvaluateAsync(
        OrchestrationPlanMachine machine,
        IReadOnlyList<OrchestrationTrainingPair> pairs,
        CancellationToken ct)
    {
        var mode1Eval = pairs.Where(pair => pair.Mode == PairMode.Mode1LearnedFlowGrounding && pair.IsHeldOut).ToArray();
        var mode2Eval = pairs.Where(pair => pair.Mode == PairMode.Mode2Generalization && pair.IsHeldOut).ToArray();
        var mode3Eval = pairs.Where(pair => pair.Mode == PairMode.Mode3HumanExplanation && pair.IsHeldOut).ToArray();
        var leakagePairs = ExactPromptLeakagePairs(pairs);

        var mode1Accuracy = await AccuracyAsync(machine, mode1Eval, result =>
            result.RouteMode == GenerationRouteMode.LearnedFlowGrounding &&
            result.MlGenerated &&
            result.FlowGrounding is not null, ct);
        var styleAccuracy = await AccuracyAsync(machine, mode2Eval, result =>
            result.RouteMode == GenerationRouteMode.Mode2Generalization &&
            result.BoundarySource == BoundarySource.Classifier &&
            result.MlGenerated, ct);
        var explanationAccuracy = mode3Eval.Length == 0
            ? 1.0
            : await AccuracyAsync(machine, mode3Eval, result =>
                result.RouteMode == GenerationRouteMode.Mode3HumanExplanation &&
                result.MlGenerated, ct);

        var mode1Purity = await PurityAsync(machine, mode1Eval, ct);
        var mode2Purity = await PurityAsync(machine, mode2Eval, ct);
        var mode3Purity = await PurityAsync(machine, mode3Eval, ct);
        var fixtures = await RunFixturesAsync(machine, ct);

        return new EvaluationMetrics(
            mode1Accuracy,
            (mode1Accuracy + styleAccuracy) / 2.0,
            styleAccuracy,
            explanationAccuracy,
            leakagePairs.Count == 0 ? 0.0 : leakagePairs.Count / (double)Math.Max(1, mode1Eval.Length + mode2Eval.Length + mode3Eval.Length),
            leakagePairs,
            ForbiddenScopeViolationRate: fixtures.Any(f => f.Fixture.Contains("34 not now", StringComparison.Ordinal) && !f.Passed) ? 1.0 : 0.0,
            PhaseOnlyCompliance: fixtures.Any(f => f.Fixture.Contains("phase 1-2.4", StringComparison.Ordinal) && !f.Passed) ? 0.0 : 1.0,
            ApprovalGateCompliance: fixtures.Any(f => f.Fixture == ApprovalGateFixture && f.Passed) ? 1.0 : 0.0,
            fixtures,
            mode1Purity,
            mode2Purity,
            mode3Purity);
    }

    private static async Task<double> AccuracyAsync(
        OrchestrationPlanMachine machine,
        IReadOnlyList<OrchestrationTrainingPair> pairs,
        Func<OrchestrationGenerationResult, bool> predicate,
        CancellationToken ct)
    {
        if (pairs.Count == 0)
            return 1.0;

        var correct = 0;
        foreach (var pair in pairs)
        {
            var result = await machine.GeneratePlanAsync(pair.Prompt, ct);
            if (result.IsSuccess && predicate(result.Value!) && PairFlowMatches(pair, result.Value!))
                correct++;
        }

        return correct / (double)pairs.Count;
    }

    private static bool PairFlowMatches(OrchestrationTrainingPair pair, OrchestrationGenerationResult result)
        => pair.FlowRef is null ||
           result.FlowGrounding?.FlowRef == pair.FlowRef ||
           result.PlanText.Contains(pair.FlowRef, StringComparison.Ordinal);

    private static async Task<IReadOnlyList<OrchestrationEvalPurityResult>> PurityAsync(
        OrchestrationPlanMachine machine,
        IReadOnlyList<OrchestrationTrainingPair> pairs,
        CancellationToken ct)
    {
        var results = new List<OrchestrationEvalPurityResult>();
        foreach (var pair in pairs)
        {
            var generated = await machine.GeneratePlanAsync(pair.Prompt, ct);
            if (!generated.IsSuccess)
                continue;

            var value = generated.Value!;
            results.Add(new OrchestrationEvalPurityResult(
                pair.PairId,
                value.BoundarySource,
                value.MlGenerated,
                value.RouteMode,
                value.FlowGrounding?.FlowRef,
                pair.Mode switch
                {
                    PairMode.Mode1LearnedFlowGrounding => value.RouteMode == GenerationRouteMode.LearnedFlowGrounding &&
                                                          value.MlGenerated &&
                                                          value.FlowGrounding?.FlowRef == pair.FlowRef,
                    PairMode.Mode2Generalization => value.RouteMode == GenerationRouteMode.Mode2Generalization &&
                                                     value.BoundarySource == BoundarySource.Classifier &&
                                                     value.MlGenerated,
                    _ => value.MlGenerated &&
                         value.RouteMode == GenerationRouteMode.Mode3HumanExplanation
                }));
        }

        return results;
    }

    private static async Task<IReadOnlyList<OrchestrationFixtureResult>> RunFixturesAsync(
        OrchestrationPlanMachine machine,
        CancellationToken ct)
    {
        var untrained = new OrchestrationPlanMachine();
        var untrainedFlow33 = await untrained.GeneratePlanAsync("flow 33", ct);
        var fixtures = new[]
        {
            "flow 33",
            "31-* documents attached",
            "start with the 1-st flow",
            "translate flow 33 from python/react native to node/react",
            "phase 1-2.4 are done, proceed 2.5 phase only",
            "33-system initiation.md explain",
            "33* files, 34 not now",
            "load planning skills",
            ApprovalGateFixture
        };

        var results = new List<OrchestrationFixtureResult>();
        if (untrainedFlow33.IsSuccess)
        {
            var value = untrainedFlow33.Value!;
            results.Add(new OrchestrationFixtureResult(
                "fixture_untrained[\"flow 33\"]",
                value.RouteMode,
                value.BoundarySource,
                value.MlGenerated,
                value.IntentKind,
                value.FlowGrounding?.FlowRef,
                value.Warnings,
                value.RouteMode == GenerationRouteMode.Mode2Generalization &&
                value.MlGenerated &&
                value.FlowGrounding is null &&
                !value.PlanText.Contains("LEARNED_FLOW_GROUNDING_PLAN", StringComparison.Ordinal)));
        }

        foreach (var fixture in fixtures)
        {
            var generated = await machine.GeneratePlanAsync(fixture, ct);
            if (!generated.IsSuccess)
                continue;

            var value = generated.Value!;
            var passed = fixture switch
            {
                "flow 33" => value.RouteMode == GenerationRouteMode.LearnedFlowGrounding && value.FlowGrounding?.FlowRef == "FLOW-33" && value.MlGenerated,
                "31-* documents attached" => value.RouteMode == GenerationRouteMode.LearnedFlowGrounding && value.FlowGrounding?.FlowRef == "FLOW-31" && value.MlGenerated,
                "start with the 1-st flow" => value.RouteMode == GenerationRouteMode.LearnedFlowGrounding && value.FlowGrounding?.FlowRef == "FLOW-01" && value.MlGenerated,
                "translate flow 33 from python/react native to node/react" => value.RouteMode == GenerationRouteMode.Mode2Generalization,
                "phase 1-2.4 are done, proceed 2.5 phase only" => value.RouteMode == GenerationRouteMode.Mode2Generalization && value.PlanText.Contains("phase_scope", StringComparison.Ordinal),
                "33-system initiation.md explain" => value.RouteMode == GenerationRouteMode.Mode3HumanExplanation,
                "33* files, 34 not now" => value.RouteMode == GenerationRouteMode.LearnedFlowGrounding && value.FlowGrounding?.FlowRef == "FLOW-33" && !value.PlanText.Contains("FLOW-34", StringComparison.Ordinal),
                "load planning skills" => value.RouteMode == GenerationRouteMode.Passthrough,
                ApprovalGateFixture => value.Warnings.Contains("PLAN_ONLY_PENDING_APPROVAL", StringComparer.Ordinal) &&
                                       value.PlanText.Contains("approval_gate=required", StringComparison.Ordinal),
                _ => false
            };

            results.Add(new OrchestrationFixtureResult(
                fixture,
                value.RouteMode,
                value.BoundarySource,
                value.MlGenerated,
                value.IntentKind,
                value.FlowGrounding?.FlowRef,
                value.Warnings,
                passed));
        }

        return results;
    }

    private static bool MetricsPass(EvaluationMetrics metrics, OrchestrationLoopRunOptions options)
        => metrics.LearnedFlowGroundingAccuracy >= options.FlowGroundingTarget &&
           metrics.UnseenStyleAccuracy >= options.StyleTarget &&
           metrics.HumanExplanationAccuracy >= options.ExplanationTarget &&
           metrics.ExactPromptLeakageRate == 0.0 &&
           metrics.ForbiddenScopeViolationRate == 0.0 &&
           metrics.PhaseOnlyCompliance == 1.0 &&
           metrics.ApprovalGateCompliance == 1.0 &&
           metrics.Mode1EvalPurity.All(row => row.Passed) &&
           metrics.Mode2EvalPurity.All(row => row.Passed) &&
           metrics.Mode3EvalPurity.All(row => row.Passed) &&
           metrics.FixtureResults.All(row => row.Passed);

    private static IReadOnlyList<string> ExactPromptLeakagePairs(IReadOnlyList<OrchestrationTrainingPair> pairs)
    {
        var train = pairs
            .Where(pair => !pair.IsHeldOut)
            .GroupBy(pair => OrchestrationCorpusBuilder.NormalizePrompt(pair.Prompt), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(pair => pair.PairId).ToArray(), StringComparer.Ordinal);
        return pairs
            .Where(pair => pair.IsHeldOut)
            .Select(pair => new
            {
                pair.PairId,
                Key = OrchestrationCorpusBuilder.NormalizePrompt(pair.Prompt)
            })
            .Where(item => train.ContainsKey(item.Key))
            .SelectMany(item => train[item.Key].Select(trainId => $"{trainId}:{item.PairId}"))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        var hash = System.Security.Cryptography.SHA256.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed record EvaluationMetrics(
        double LearnedFlowGroundingAccuracy,
        double OverallTrainingAccuracy,
        double UnseenStyleAccuracy,
        double HumanExplanationAccuracy,
        double ExactPromptLeakageRate,
        IReadOnlyList<string> ExactPromptLeakagePairs,
        double ForbiddenScopeViolationRate,
        double PhaseOnlyCompliance,
        double ApprovalGateCompliance,
        IReadOnlyList<OrchestrationFixtureResult> FixtureResults,
        IReadOnlyList<OrchestrationEvalPurityResult> Mode1EvalPurity,
        IReadOnlyList<OrchestrationEvalPurityResult> Mode2EvalPurity,
        IReadOnlyList<OrchestrationEvalPurityResult> Mode3EvalPurity);
}
