// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E26: ContextualDictionary — choose the correct word translation from context.
//
// Hypothesis: K-NN on neighboring POS tags selects the right meaning more
// accurately than returning the highest-priority dictionary entry.
//
// Baseline:  highest-priority entry (no context)
// Enhanced:  K-NN on context (neighboring POS tags)
// Dataset:   15 fixed technical Russian words × multiple contexts
// Primary metric: disambiguation_accuracy
// Scaling signal: hard_word_accuracy (words with 3+ meanings: поток, запрос)
public sealed class E26ContextualDictionary
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var dict      = ContextualDictionary.BuildDefault();
        var testCases = BuildTestCases();

        // ── Baseline: highest-priority entry ─────────────────────────────────
        var t0 = Stopwatch();
        int baseCorrect = testCases.Count(tc =>
            dict.TranslateBaseline(tc.SourceWord) == tc.ExpectedTranslation);
        double baselineAcc = baseCorrect / (double)testCases.Count;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: context-aware translation ──────────────────────────────
        var t1 = Stopwatch();
        int enhCorrect = testCases.Count(tc =>
            dict.TranslateWithContext(
                tc.SourceWord, tc.ContextBefore, tc.ContextAfter)
            == tc.ExpectedTranslation);
        double enhancedAcc = enhCorrect / (double)testCases.Count;
        double enhancedMs  = ElapsedMs(t1);

        // Hard cases: words with 3+ distinct meanings
        var hardCases = testCases.Where(tc => tc.IsHard).ToList();
        int enhHardOk = hardCases.Count(tc =>
            dict.TranslateWithContext(
                tc.SourceWord, tc.ContextBefore, tc.ContextAfter)
            == tc.ExpectedTranslation);
        double hardAcc = hardCases.Count > 0 ? enhHardOk / (double)hardCases.Count : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E26-CONTEXTUAL-DICTIONARY",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "disambiguation_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = hardAcc,
            ScalingSignalNote = "hard_word_accuracy (words with 3+ meanings)",
            ShouldScaleUp     = testCases.Count < 20,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"testCases={testCases.Count}, hard={hardCases.Count}, " +
                    $"baseAcc={baselineAcc:P1}, enhAcc={enhancedAcc:P1}, " +
                    $"hardAcc={hardAcc:P1}"
        });
    }

    private static readonly int[] Nil = Array.Empty<int>();

    private static List<TranslationTestCase> BuildTestCases() => new()
    {
        // "записать" — log vs write vs record
        new("записать", new[]{8}, new[]{7},  "log",        false),
        new("записать", new[]{8}, new[]{11}, "write",      false),
        new("запись",   new[]{7}, new[]{7},  "record",     false),

        // "поток" — three meanings (hardest)
        new("поток",    new[]{8}, Nil,        "thread",    true),
        new("потоки",   Nil,      new[]{7},   "flow",      true),
        new("потоком",  new[]{9}, Nil,        "stream",    true),

        // "запрос" — three meanings
        new("запрос",   Nil,      new[]{11},  "request",   true),
        new("запросу",  Nil,      new[]{7},   "query",     true),
        new("запрос",   new[]{7}, Nil,        "prompt",    true),

        // "проверку" — two meanings
        new("проверку", new[]{8}, Nil,        "check",     false),
        new("проверка", Nil,      new[]{9},   "validation",false),

        // "вернуть" — two meanings
        new("вернуть",  Nil,      new[]{3},   "return",    false),
        new("вернуть",  Nil,      new[]{9},   "restore",   false),

        // "обработку" — two meanings
        new("обработку", new[]{8,7}, Nil,      "processing",false),
        new("обработку", Nil,        new[]{7}, "handling",  false),
    };

    private sealed record TranslationTestCase(
        string SourceWord,
        int[]  ContextBefore,
        int[]  ContextAfter,
        string ExpectedTranslation,
        bool   IsHard);

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
