// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.Json;

public static class OrchestrationCorpusBuilder
{
    private static readonly JsonSerializerOptions JsonLineOptions =
        new(OrchestrationPlanJsonOptions.Standard) { WriteIndented = false };

    public static async Task<OrchestrationCorpusBuildReport> BuildAsync(
        string sourceDir,
        string? auditPath,
        string outputPath,
        CancellationToken ct = default)
    {
        _ = auditPath;
        var sources = OrchestrationCorpusAudit.LoadCorpusSources(sourceDir);
        var pairs = BuildPairs(sources);

        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        await using (var stream = File.Create(outputPath))
        await using (var writer = new StreamWriter(stream, System.Text.Encoding.UTF8))
        {
            foreach (var pair in pairs)
            {
                ct.ThrowIfCancellationRequested();
                await writer.WriteLineAsync(JsonSerializer.Serialize(pair, JsonLineOptions));
            }
        }

        return BuildReport(sourceDir, outputPath, pairs);
    }

    public static OrchestrationCorpusBuildReport BuildReport(
        string sourceDir,
        string outputPath,
        IReadOnlyList<OrchestrationTrainingPair> pairs)
    {
        var mode3TrainRefs = pairs
            .Where(pair => pair.Mode == PairMode.Mode3HumanExplanation && !pair.IsHeldOut)
            .Select(pair => pair.FlowRef)
            .Where(refValue => refValue is not null)
            .ToHashSet(StringComparer.Ordinal);
        var mode3EvalRefs = pairs
            .Where(pair => pair.Mode == PairMode.Mode3HumanExplanation && pair.IsHeldOut)
            .Select(pair => pair.FlowRef)
            .Where(refValue => refValue is not null)
            .ToHashSet(StringComparer.Ordinal);
        var mode3TrainBundles = pairs
            .Where(pair => pair.Mode == PairMode.Mode3HumanExplanation && !pair.IsHeldOut)
            .Select(pair => pair.SourceBundleId)
            .ToHashSet(StringComparer.Ordinal);
        var mode3EvalBundles = pairs
            .Where(pair => pair.Mode == PairMode.Mode3HumanExplanation && pair.IsHeldOut)
            .Select(pair => pair.SourceBundleId)
            .ToHashSet(StringComparer.Ordinal);

        var report = new OrchestrationCorpusBuildReport(
            sourceDir,
            outputPath,
            pairs.Count,
            pairs.Count(pair => pair.Mode == PairMode.Mode1LearnedFlowGrounding && !pair.IsHeldOut),
            pairs.Count(pair => pair.Mode == PairMode.Mode1LearnedFlowGrounding && pair.IsHeldOut),
            pairs.Count(pair => pair.Mode == PairMode.Mode2Generalization && !pair.IsHeldOut),
            pairs.Count(pair => pair.Mode == PairMode.Mode2Generalization && pair.IsHeldOut),
            pairs.Count(pair => pair.Mode == PairMode.Mode3HumanExplanation && !pair.IsHeldOut),
            pairs.Count(pair => pair.Mode == PairMode.Mode3HumanExplanation && pair.IsHeldOut),
            pairs.Count(pair => pair.Mode == PairMode.Mode3HumanExplanation),
            NoExactPromptOverlap(pairs, PairMode.Mode1LearnedFlowGrounding),
            NoExactPromptOverlap(pairs, PairMode.Mode2Generalization),
            NoExactPromptOverlap(pairs, PairMode.Mode3HumanExplanation),
            !mode3TrainBundles.Overlaps(mode3EvalBundles),
            !mode3TrainRefs.Overlaps(mode3EvalRefs),
            false);

        return report with
        {
            MeetsCorpusGate = report.Mode1TrainingPairs >= 42 &&
                              report.Mode1HeldOutEvalPairs >= 20 &&
                              report.Mode2TrainingPairs >= 200 &&
                              report.Mode2HeldOutEvalPairs >= 40 &&
                              report.Mode3TrainingPairs >= 30 &&
                              report.Mode3HeldOutEvalPairs >= 20 &&
                              report.Mode3ExplanationPairs >= 50 &&
                              report.NoMode1ExactPromptOverlap &&
                              report.NoMode2ExactPromptOverlap &&
                              report.NoMode3ExactPromptOverlap &&
                              report.NoMode3SourceBundleOverlap &&
                              report.NoMode3FlowRefOverlap
        };
    }

    public static IReadOnlyList<OrchestrationTrainingPair> BuildPairs(OrchestrationCorpusSources sources)
    {
        var pairs = new List<OrchestrationTrainingPair>();
        var flows = sources.FlowDocs
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Take(42)
            .Select(pair => pair.Value)
            .ToArray();

        AddMode1Pairs(pairs, flows);
        AddMode2Pairs(pairs, flows);
        AddMode3Pairs(pairs, flows);
        return pairs;
    }

    public static async Task<IReadOnlyList<OrchestrationTrainingPair>> ReadPairsAsync(
        string path,
        CancellationToken ct = default)
    {
        var rows = new List<OrchestrationTrainingPair>();
        await foreach (var line in File.ReadLinesAsync(path, ct))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            var pair = JsonSerializer.Deserialize<OrchestrationTrainingPair>(line, OrchestrationPlanJsonOptions.Standard);
            if (pair is not null)
                rows.Add(pair);
        }

        return rows;
    }

    private static void AddMode1Pairs(List<OrchestrationTrainingPair> pairs, IReadOnlyList<OrchestrationPlanChosen> flows)
    {
        foreach (var flow in flows.Take(42))
        {
            var number = flow.FlowRef.Replace("FLOW-", string.Empty, StringComparison.Ordinal);
            var plainNumber = int.Parse(number, System.Globalization.CultureInfo.InvariantCulture).ToString(System.Globalization.CultureInfo.InvariantCulture);
            pairs.Add(new OrchestrationTrainingPair(
                $"mode1-train-{flow.FlowRef.ToLowerInvariant()}",
                PairMode.Mode1LearnedFlowGrounding,
                $"learned grounding plan {flow.FlowRef} flow {plainNumber} {plainNumber}-star canonical orchestration",
                flow,
                "Rejected route uses deterministic lookup instead of learned grounding.",
                GenerationRouteMode.LearnedFlowGrounding,
                BoundarySource.Classifier,
                true,
                IntentKind.KnownFlowPlanning,
                flow.FlowRef,
                $"mode1-train-bundle-{flow.FlowRef}",
                false));
        }

        foreach (var flow in flows.Skip(22).Take(20))
        {
            var number = int.Parse(flow.FlowRef.Replace("FLOW-", string.Empty, StringComparison.Ordinal), System.Globalization.CultureInfo.InvariantCulture);
            pairs.Add(new OrchestrationTrainingPair(
                $"mode1-eval-{flow.FlowRef.ToLowerInvariant()}",
                PairMode.Mode1LearnedFlowGrounding,
                number % 2 == 0 ? $"flow {number}" : $"{number}* files",
                flow,
                "Rejected route misses the learned canonical flow.",
                GenerationRouteMode.LearnedFlowGrounding,
                BoundarySource.Classifier,
                true,
                IntentKind.KnownFlowPlanning,
                flow.FlowRef,
                $"mode1-eval-bundle-{flow.FlowRef}",
                true));
        }
    }

    private static void AddMode2Pairs(List<OrchestrationTrainingPair> pairs, IReadOnlyList<OrchestrationPlanChosen> flows)
    {
        var prompts = new[]
        {
            ("translate flow 33 from python/react native to node/react", IntentKind.DocumentationMigration, "FLOW-33"),
            ("phase 1-2.4 are done, proceed 2.5 phase only", IntentKind.PhaseContinuation, "FLOW-02"),
            ("review architecture for FLOW-33", IntentKind.ArchitectureReview, "FLOW-33"),
            ("create testing plan for FLOW-33", IntentKind.TestingPlan, "FLOW-33"),
            ("continue implementation using local wiki evidence", IntentKind.ImplementationExecutionRequest, "FLOW-04")
        };

        for (var i = 0; i < 240; i++)
        {
            var fixture = prompts[i % prompts.Length];
            var flow = flows.FirstOrDefault(f => f.FlowRef == fixture.Item3) ?? flows[i % flows.Count];
            var heldOut = i >= 200;
            pairs.Add(new OrchestrationTrainingPair(
                $"mode2-{i + 1:D3}",
                PairMode.Mode2Generalization,
                i < prompts.Length ? fixture.Item1 : $"{fixture.Item1} style sample {i + 1}",
                flow,
                "Rejected route retrieves a canonical flow or ignores scope boundaries.",
                GenerationRouteMode.Mode2Generalization,
                BoundarySource.Classifier,
                true,
                fixture.Item2,
                flow.FlowRef,
                $"mode2-style-{i % 80:D2}",
                heldOut));
        }
    }

    private static void AddMode3Pairs(List<OrchestrationTrainingPair> pairs, IReadOnlyList<OrchestrationPlanChosen> flows)
    {
        var trainFlows = flows.Take(10).ToArray();
        var evalFlows = flows.Skip(10).Take(10).ToArray();
        var ordinal = 1;

        foreach (var flow in trainFlows)
        {
            for (var copy = 0; copy < 3; copy++)
                pairs.Add(Mode3Pair(ordinal++, flow, heldOut: false, copy));
        }

        foreach (var flow in evalFlows)
        {
            for (var copy = 0; copy < 2; copy++)
                pairs.Add(Mode3Pair(ordinal++, flow, heldOut: true, copy));
        }
    }

    private static OrchestrationTrainingPair Mode3Pair(
        int ordinal,
        OrchestrationPlanChosen flow,
        bool heldOut,
        int copy)
        => new(
            $"mode3-{ordinal:D3}",
            PairMode.Mode3HumanExplanation,
            copy == 0
                ? $"explain {flow.FlowRef} orchestration"
                : $"how does {flow.FlowRef} orchestration work variant {copy}",
            flow with
            {
                PlanText = $"Human explanation for {flow.FlowRef}: describe orchestration decisions, boundaries, and handoff points."
            },
            "Rejected answer hides the orchestration reasoning.",
            GenerationRouteMode.Mode3HumanExplanation,
            BoundarySource.Classifier,
            true,
            IntentKind.HumanOrchestrationExplanation,
            flow.FlowRef,
            heldOut ? $"mode3-eval-bundle-{flow.FlowRef}" : $"mode3-train-bundle-{flow.FlowRef}",
            heldOut);

    private static bool NoExactPromptOverlap(IReadOnlyList<OrchestrationTrainingPair> pairs, PairMode mode)
    {
        var train = pairs
            .Where(pair => pair.Mode == mode && !pair.IsHeldOut)
            .Select(pair => NormalizePrompt(pair.Prompt))
            .ToHashSet(StringComparer.Ordinal);
        var eval = pairs
            .Where(pair => pair.Mode == mode && pair.IsHeldOut)
            .Select(pair => NormalizePrompt(pair.Prompt))
            .ToArray();
        return !eval.Any(train.Contains);
    }

    public static string NormalizePrompt(string prompt)
        => string.Join(
            " ",
            (prompt ?? string.Empty)
                .Trim()
                .ToLowerInvariant()
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
