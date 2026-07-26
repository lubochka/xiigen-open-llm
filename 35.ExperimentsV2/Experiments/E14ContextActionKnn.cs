// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E14: context→action K-NN — AST file summary predicts required change type.
//
// Hypothesis: a file's AST feature vector predicts which action type it needs
// (ADD_NULL_CHECK / ADD_LOOP / REFACTOR_EXTRACT / ADD_METHOD) better than
// a majority-class baseline. Validates Variant B (LoRA instruction tuning):
// if K-NN cannot detect the signal, LoRA will have nothing to learn from.
//
// Labels are assigned deterministically by ActionLabelGenerator — no git,
// no manual annotation. This makes E14 a direct source for DPO pairs in project 11.
//
// Baseline:  majority-class classifier
// Enhanced:  K-NN (k=3, cosine distance on normalised SyntaxKind vectors)
// Scaling signal: label_balance (< 0.3 = imbalanced, scale up for diversity)
public sealed class E14ContextActionKnn
{
    private readonly int _k;
    public E14ContextActionKnn(int k = 3) => _k = k;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Too few files: {files.Count}. Need ≥ 4.");

        var labeled = files
            .Select(f => (
                Features: ActionLabelGenerator.FeatureVector(f.Text),
                Label:    ActionLabelGenerator.Label(f.Text),
                Path:     f.Path))
            .ToList();

        int split    = (int)(labeled.Count * 0.8);
        var trainSet = labeled.Take(split).ToList();
        var valSet   = labeled.Skip(split).ToList();

        if (valSet.Count == 0)
            return OperationResult<ExperimentV2Result>.Fail("Empty validation set");

        // ── Baseline: majority-class classifier ───────────────────────────────
        var t0 = Stopwatch();
        var labelFreq = trainSet
            .GroupBy(x => x.Label)
            .ToDictionary(g => g.Key, g => g.Count());
        int majorityLabel = labelFreq.Count > 0
            ? labelFreq.MaxBy(kv => kv.Value).Key : 0;
        int baseCorrect   = valSet.Count(x => x.Label == majorityLabel);
        double baselineAcc = baseCorrect / (double)valSet.Count;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: K-NN (cosine) ───────────────────────────────────────────
        var t1 = Stopwatch();
        var knn = new FileActionKnnPredictor(_k);
        knn.Train(trainSet.Select(x => (x.Features, x.Label)).ToList());

        int knnCorrect = 0;
        var misclassified = new List<string>();
        foreach (var x in valSet)
        {
            int predicted = knn.Predict(x.Features);
            if (predicted == x.Label) knnCorrect++;
            else
                misclassified.Add(
                    $"{Path.GetFileName(x.Path)}: pred={ActionLabelGenerator.Labels[predicted]}" +
                    $" actual={ActionLabelGenerator.Labels[x.Label]}");
        }
        double knnAcc     = knnCorrect / (double)valSet.Count;
        double enhancedMs = ElapsedMs(t1);

        string labelDist = string.Join(", ",
            labeled.GroupBy(x => ActionLabelGenerator.Labels[x.Label])
                   .OrderByDescending(g => g.Count())
                   .Select(g => $"{g.Key}={g.Count()}"));

        double balance = labelFreq.Count > 1
            ? labelFreq.Values.Min() / (double)labelFreq.Values.Max()
            : 1.0;

        string confNote = misclassified.Count > 0
            ? " | misclassified: " + string.Join("; ", misclassified.Take(3))
            : "";

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E14-CONTEXT-ACTION-KNN",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = knnAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = balance,
            ScalingSignalNote = "label_balance (1.0 = uniform, < 0.3 = imbalanced → scale up)",
            ShouldScaleUp     = balance < 0.3 || valSet.Count < 5,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"files={files.Count}, trainFiles={trainSet.Count}, " +
                    $"valFiles={valSet.Count}, k={_k}, " +
                    $"baseAcc={baselineAcc:P1}, knnAcc={knnAcc:P1}, " +
                    $"balance={balance:F2}, labels=[{labelDist}]{confNote}"
        });
    }

    private static long   Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
