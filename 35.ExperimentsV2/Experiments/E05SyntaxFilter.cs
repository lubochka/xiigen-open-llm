// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E05: Roslyn grammar mask on N-gram predictions.
public sealed class E05SyntaxFilter
{
    // Run the syntax-filter experiment using production GrammarMaskFilter.
    public OperationResult<ExperimentV2Result> Run(
        string csharpSource,
        IReadOnlyList<string>? allSources = null,
        int tier = 1)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return OperationResult<ExperimentV2Result>.Fail("Empty source for E05");

        var astResult = GrammarMaskFilter.BuildVocabulary([csharpSource]);
        if (!astResult.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("AST parse: " + astResult.ErrorMessage);

        var astTokens = astResult.Value!.Tokens;
        var kindToIdx = astResult.Value.KindToIdx;
        if (astTokens.Count < 6)
            return OperationResult<ExperimentV2Result>.Fail("Too few AST nodes for E05");

        var splitAt = (int)(astTokens.Count * 0.8);
        var trainTokens = astTokens.Take(splitAt).ToList();
        var validationTokens = astTokens.Skip(splitAt).ToList();
        if (validationTokens.Count < 3)
            validationTokens = astTokens.TakeLast(3).ToList();

        var started = Stopwatch();
        var trigram = new TrigramModelV2();
        var trigramTrain = trigram.Train(trainTokens);
        if (!trigramTrain.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("Train: " + trigramTrain.ErrorMessage);

        var grammarSources = allSources ?? [csharpSource];
        var grammarFilter = new GrammarMaskFilter();
        var grammarTrain = grammarFilter.Train(trainTokens, grammarSources, kindToIdx);
        if (!grammarTrain.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("Grammar filter train: " + grammarTrain.ErrorMessage);

        var baselineCorrect = 0;
        var enhancedCorrect = 0;
        var total = validationTokens.Count - 2;
        var syntaxConstrained = 0;
        var maskSizes = new List<int>(Math.Max(0, total));
        for (var index = 2; index < validationTokens.Count; index++)
        {
            var prev2 = validationTokens[index - 2];
            var prev1 = validationTokens[index - 1];
            var actual = validationTokens[index];

            var baseline = trigram.Predict(prev2, prev1);
            if (baseline.IsSuccess && baseline.Value == actual)
                baselineCorrect++;

            var mask = grammarFilter.ValidContinuations(prev2, prev1);
            maskSizes.Add(mask.Count);
            if (mask.Count > 0)
            {
                if (grammarFilter.PredictNext(prev2, prev1) == actual)
                    enhancedCorrect++;

                syntaxConstrained++;
            }
            else if (baseline.IsSuccess && baseline.Value == actual)
            {
                enhancedCorrect++;
            }
        }

        var elapsedMs = ElapsedMs(started);
        var baselineAccuracy = total > 0 ? baselineCorrect / (double)total : 0;
        var enhancedAccuracy = total > 0 ? enhancedCorrect / (double)total : 0;
        var syntaxPct = total > 0 ? syntaxConstrained / (double)total : 0;
        var avgMask = maskSizes.Count > 0 ? maskSizes.Average() : 0;
        var p10Mask = Percentile(maskSizes, 10);
        var p90Mask = Percentile(maskSizes, 90);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId = "E05-SYNTAX-FILTER",
            Tier = tier,
            BaselinePrimary = baselineAccuracy,
            EnhancedPrimary = enhancedAccuracy,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter = false,
            ScalingSignal = avgMask,
            ScalingSignalNote = "avg_mask_size (smaller = more constrained = filter helps more)",
            BaselineTimeMs = elapsedMs,
            EnhancedTimeMs = elapsedMs,
            Notes = $"syntaxCorrectPct={syntaxPct:P1}, avgMask={avgMask:F1}, maskP10={p10Mask:F0}, maskP90={p90Mask:F0}, astNodes={astTokens.Count}"
        });
    }

    // Compute the requested percentile of mask sizes.
    private static double Percentile(List<int> values, int percentile)
    {
        if (values.Count == 0)
            return 0;

        var sorted = values.Order().ToArray();
        var index = (int)Math.Ceiling(percentile / 100.0 * sorted.Length) - 1;
        return sorted[Math.Clamp(index, 0, sorted.Length - 1)];
    }

    private static long Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();

    private static double ElapsedMs(long started)
    {
        return (System.Diagnostics.Stopwatch.GetTimestamp() - started)
            / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
    }
}
