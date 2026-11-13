// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E08: non-code syntax experiment — three sub-experiments.
//
// Hypothesis: structural role tagging produces a more predictable token sequence
// than BPE sub-words, measured by top-1 accuracy (fairer than perplexity because
// role vocab sizes differ wildly from BPE vocab size).
//
// E08a — English prose  (Moby Dick; EnglishRoleTagger, 12 roles)
// E08b — SQL queries    (500 generated; SqlRoleTagger,     5 roles)
// E08c — JSON documents (200 generated; JsonRoleTagger,    9 roles)
//
// Primary metric: average top1_accuracy across the three sub-experiments (higher = better).
// Enhanced wins when role-tagged bigram beats BPE bigram on average.
public sealed class E08NonCodeSyntax
{
    private readonly string _cacheDir;

    public E08NonCodeSyntax(string cacheDir) => _cacheDir = cacheDir;

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var t0 = Stopwatch();

        // ── E08a: English (Moby Dick) ─────────────────────────────────────────
        var textOp = NonCodeDatasetLoader.LoadEnglishText(_cacheDir);
        if (!textOp.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("E08a: " + textOp.ErrorMessage);

        string english = textOp.Value!;
        int textLimit = tier switch { 1 => 50_000, 2 => 200_000, _ => english.Length };
        if (english.Length > textLimit) english = english[..textLimit];

        var (baseA, enhA) = RunSub(english,
            t => EnglishRoleTagger.TagText(t),
            EnglishRoleTagger.RoleCount);
        Console.Write($"  E08a(en) base={baseA:F3} enh={enhA:F3}  ");

        // ── E08b: SQL ─────────────────────────────────────────────────────────
        var sqlQueries = NonCodeDatasetLoader.GenerateSqlQueries(tier >= 2 ? 1000 : 500);
        string sqlText = string.Join("\n", sqlQueries);

        var (baseB, enhB) = RunSub(sqlText,
            t => SqlRoleTagger.TagSql(t),
            SqlRoleTagger.RoleCount);
        Console.Write($"E08b(sql) base={baseB:F3} enh={enhB:F3}  ");

        // ── E08c: JSON ────────────────────────────────────────────────────────
        var jsonDocs    = NonCodeDatasetLoader.GenerateJsonDocuments(tier >= 2 ? 400 : 200);
        // Wrap in a JSON array so Utf8JsonReader processes all documents, not just the first.
        string jsonText = "[" + string.Join(",", jsonDocs) + "]";

        var (baseC, enhC) = RunSub(jsonText,
            t => JsonRoleTagger.TagJson(t),
            JsonRoleTagger.RoleCount);
        Console.WriteLine($"E08c(json) base={baseC:F3} enh={enhC:F3}");

        double totalMs     = ElapsedMs(t0);
        double baselineAvg = (baseA + baseB + baseC) / 3.0;
        double enhancedAvg = (enhA  + enhB  + enhC)  / 3.0;

        // Scaling signal: variance of relative deltas across sub-experiments
        double[] deltas =
        [
            SafeDelta(baseA, enhA),
            SafeDelta(baseB, enhB),
            SafeDelta(baseC, enhC),
        ];
        double mean     = deltas.Average();
        double variance = deltas.Select(d => (d - mean) * (d - mean)).Average();

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E08-NON-CODE-SYNTAX",
            Tier              = tier,
            BaselinePrimary   = baselineAvg,
            EnhancedPrimary   = enhancedAvg,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = variance,
            ScalingSignalNote = "delta_variance (variance of relative Δ across 3 sub-exps — lower = more consistent)",
            BaselineTimeMs    = totalMs,
            EnhancedTimeMs    = totalMs,
            ShouldScaleUp     = baselineAvg < 0.15,
            Notes             = $"E08a(en): {baseA:F3}→{enhA:F3} | " +
                                $"E08b(sql): {baseB:F3}→{enhB:F3} | " +
                                $"E08c(json): {baseC:F3}→{enhC:F3} | " +
                                $"tier={tier}, charLimit={textLimit:N0}"
        });
    }

    // Run one sub-experiment: BPE bigram top1_accuracy vs role-tagger bigram top1_accuracy.
    private static (double bpeTop1, double roleTop1) RunSub(
        string                  text,
        Func<string, List<int>> tagger,
        int                     roleCount)
    {
        // Baseline: BPE bigram
        var bpe = new Tokenizer.BpeTokenizer();
        bpe.Train(text, vocabSize: 512);
        var bpeTokens = bpe.Encode(text);
        if (bpeTokens.Count < 4) return (0, 0);

        int bpeSplit = (int)(bpeTokens.Count * 0.8);
        var baseBigram = new BigramModelV2();
        baseBigram.Train(bpeTokens.Take(bpeSplit).ToList());
        var baseEval = baseBigram.Evaluate(bpeTokens.Skip(bpeSplit).ToList());
        if (!baseEval.IsSuccess) return (0, 0);
        (_, double bpeTop1, _, _) = baseEval.Value!;

        // Enhanced: role-tagger bigram
        var roleTokens = tagger(text);
        if (roleTokens.Count < 4) return (bpeTop1, 0);

        int roleSplit = (int)(roleTokens.Count * 0.8);
        var roleBigram = new BigramModelV2();
        roleBigram.Train(roleTokens.Take(roleSplit).ToList());
        var roleEval = roleBigram.Evaluate(roleTokens.Skip(roleSplit).ToList());
        if (!roleEval.IsSuccess) return (bpeTop1, 0);
        (_, double roleTop1, _, _) = roleEval.Value!;

        return (bpeTop1, roleTop1);
    }

    // Relative improvement (higher-is-better): (enhanced - baseline) / baseline
    private static double SafeDelta(double b, double e)
        => b == 0 ? 0 : (e - b) / b;

    private static long   Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
