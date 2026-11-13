// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E25: PatternClassifier — determine word-order pattern from POS tag sequence.
//
// Hypothesis: POS bigrams predict the sentence pattern (SOV, SVO, IMPERATIVE, ...)
// more accurately than a simple "Russian = always SOV" rule.
//
// Baseline:  rules on first-verb position
// Enhanced:  K-NN on POS bigrams (Jaccard)
// Dataset:   synthetic POS sequences, 8 patterns × N examples
// Primary metric: top1_accuracy (8 classes, random = 12.5%)
// Scaling signal: hard_pattern_accuracy (SOV / VSO / TOPIC_OV)
public sealed class E25PatternClassifier
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var examples = SentencePatternClassifier.GenerateExamples(seed: 42, perPattern: tier * 20);
        int split    = (int)(examples.Count * 0.8);
        var train    = examples.Take(split).ToList();
        var val      = examples.Skip(split).ToList();

        var clf = new SentencePatternClassifier(k: 3);
        clf.Train(train);

        // ── Baseline: rules ──────────────────────────────────────────────────
        var t0 = Stopwatch();
        int baseCorrect = val.Count(e =>
            SentencePatternClassifier.PredictByRules(e.PosSeq) == e.Pattern);
        double baselineAcc = baseCorrect / (double)val.Count;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: K-NN on POS bigrams ────────────────────────────────────
        var t1 = Stopwatch();
        int enhCorrect = val.Count(e => clf.Predict(e.PosSeq) == e.Pattern);
        double enhancedAcc = enhCorrect / (double)val.Count;
        double enhancedMs  = ElapsedMs(t1);

        // Hard patterns: everything except the two simplest
        var hardCases = val.Where(e =>
            e.Pattern != SentencePatternClassifier.SVO &&
            e.Pattern != SentencePatternClassifier.IMPERATIVE_V).ToList();
        int enhHardCorrect = hardCases.Count(e => clf.Predict(e.PosSeq) == e.Pattern);
        double hardAcc = hardCases.Count > 0 ? enhHardCorrect / (double)hardCases.Count : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E25-PATTERN-CLASSIFIER",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = hardAcc,
            ScalingSignalNote = "hard_pattern_accuracy (SOV/VSO/TOPIC — hardest patterns)",
            ShouldScaleUp     = val.Count < 30,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"examples={examples.Count}, val={val.Count}, " +
                    $"baseAcc={baselineAcc:P1}, enhAcc={enhancedAcc:P1}, " +
                    $"hardCases={hardCases.Count}, hardAcc={hardAcc:P1}"
        });
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
