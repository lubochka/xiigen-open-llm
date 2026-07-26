// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E28v2: ProjectAdaptor — ProbeNeuralNetwork specialized on project-A files.
// Baseline:  neural net trained on all files (generic)
// Enhanced:  neural net trained only on project-A files (adapted)
// Hypothesis: neural weights learn project-specific predicate transitions that
//             bigram couldn't show because reordering counts has no effect.
public sealed class E28ProjectAdaptorV2
{
    private readonly int _epochs;
    public E28ProjectAdaptorV2(int epochs = 3) => _epochs = epochs;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> allFiles,
        int tier = 1)
    {
        if (allFiles.Count < 6)
            return OperationResult<ExperimentV2Result>.Fail("Need >= 6 files");

        const int vocabSize = 22; // LogicExtractor produces predicates 0-21
        int adaptSplit   = (int)(allFiles.Count * 0.6);
        var projectA     = allFiles.Take(adaptSplit).ToList();
        var testFiles    = allFiles.Skip(adaptSplit).ToList();

        var allPreds     = ExtractPreds(allFiles);
        var projectPreds = ExtractPreds(projectA);
        var testPreds    = ExtractPreds(testFiles);

        if (testPreds.Count < 5)
            return OperationResult<ExperimentV2Result>.Fail("Too few test predicates");

        // ── Baseline: generic neural net (all files) ──────────────────────────
        var t0      = Stopwatch();
        var generic = new ProbeNeuralNetwork(vocabSize, hiddenSize: 32, lr: 0.02f, seed: 42);
        for (int ep = 0; ep < _epochs; ep++)
            for (int i = 0; i < allPreds.Count - 1; i++)
                generic.TrainStep(allPreds[i], allPreds[i + 1]);
        double baseAcc    = generic.Evaluate(testPreds);
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: adapted neural net (project-A only) ─────────────────────
        var t1      = Stopwatch();
        var adapted = new ProbeNeuralNetwork(vocabSize, hiddenSize: 32, lr: 0.02f, seed: 42);
        for (int ep = 0; ep < _epochs; ep++)
            for (int i = 0; i < projectPreds.Count - 1; i++)
                adapted.TrainStep(projectPreds[i], projectPreds[i + 1]);
        double enhAcc     = adapted.Evaluate(testPreds);
        double enhancedMs = ElapsedMs(t1);

        double overlap = ComputeOverlap(projectPreds, testPreds);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E28V2-PROJECT-ADAPTOR",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = overlap,
            ScalingSignalNote = "predicate_overlap (higher → adaptation more valuable)",
            ShouldScaleUp     = testPreds.Count < 20,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"allFiles={allFiles.Count}, adaptFiles={projectA.Count}, " +
                    $"allPreds={allPreds.Count}, projectPreds={projectPreds.Count}, " +
                    $"testPreds={testPreds.Count}, overlap={overlap:F2}, " +
                    $"baseAcc={baseAcc:P1}, enhAcc={enhAcc:P1}"
        });
    }

    private static List<int> ExtractPreds(IEnumerable<(string, string)> files)
    {
        var r = new List<int>();
        foreach (var (_, t) in files)
        {
            var res = LogicExtractor.Extract(t);
            if (res.IsSuccess) r.AddRange(res.Value!);
        }
        return r;
    }

    private static double ComputeOverlap(List<int> a, List<int> b)
    {
        var sa = a.ToHashSet(); var sb = b.ToHashSet();
        int inter = sa.Count(x => sb.Contains(x));
        int union  = sa.Count + sb.Count - inter;
        return union == 0 ? 0 : (double)inter / union;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
