// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E08: comparison of POS tags (structural word types) vs BPE subwords
// on ordinary English text.
// Baseline: bigram on the text's BPE tokens (vocab 512)
// Enhanced: bigram on POS tags (vocab ~17 Universal Dependencies tags)
public sealed class E08TextSyntax
{
    private readonly string _scriptPath;

    public E08TextSyntax(string scriptPath)
    {
        _scriptPath = scriptPath;
    }

    public OperationResult<ExperimentV2Result> Run(
        TextDatasetContents dataset,
        int tier = 1)
    {
        if (dataset.SentenceCount < 10)
            return OperationResult<ExperimentV2Result>.Fail("Too few sentences");

        // Baseline: BPE on the whole text
        var t0     = Stopwatch();
        var bpeTok = new Tokenizer.BpeTokenizer();
        bpeTok.Train(dataset.CombinedText, vocabSize: 512);
        var bpeTokens = bpeTok.Encode(dataset.CombinedText);

        int bpeSplit = (int)(bpeTokens.Count * 0.8);
        var bpeTrain = bpeTokens.Take(bpeSplit).ToList();
        var bpeVal   = bpeTokens.Skip(bpeSplit).ToList();

        var baseBigram = new BigramModelV2();
        baseBigram.Train(bpeTrain);
        var baseEval = baseBigram.Evaluate(bpeVal);
        if (!baseEval.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("Baseline: " + baseEval.ErrorMessage);

        (double bpePerp, double bpeTop1, _, _) = baseEval.Value!;
        double baselineMs = ElapsedMs(t0);

        // Enhanced: POS tags via SpaCy
        var t1        = Stopwatch();
        var posTokens = new List<int>();
        var posVocab  = new Dictionary<string, int>();

        using var tagger = new SpacyPosTagger(_scriptPath);

        foreach (var sentence in dataset.Sentences)
        {
            var tagsResult = tagger.GetPosTags(sentence);
            if (!tagsResult.IsSuccess) continue;

            foreach (var tag in tagsResult.Value!)
            {
                if (!posVocab.TryGetValue(tag, out int tagId))
                    posVocab[tag] = tagId = posVocab.Count;
                posTokens.Add(tagId);
            }
        }

        if (posTokens.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail(
                "SpaCy returned too few tags. Check that Python and SpaCy are installed.");

        int posSplit  = (int)(posTokens.Count * 0.8);
        var posTrain  = posTokens.Take(posSplit).ToList();
        var posVal    = posTokens.Skip(posSplit).ToList();

        var enhBigram = new BigramModelV2();
        enhBigram.Train(posTrain);
        var enhEval = enhBigram.Evaluate(posVal);
        if (!enhEval.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("Enhanced: " + enhEval.ErrorMessage);

        (double posPerp, double posTop1, _, _) = enhEval.Value!;
        double enhancedMs = ElapsedMs(t1);

        double vocabRatio = (double)posVocab.Count / bpeTok.VocabSize;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E08-TEXT-SYNTAX",
            Tier              = tier,
            BaselinePrimary   = bpePerp,
            EnhancedPrimary   = posPerp,
            PrimaryMetricName = "perplexity",
            LowerIsBetter     = true,
            ScalingSignal     = vocabRatio,
            ScalingSignalNote = "pos_vocab_ratio (POS types / BPE types — lower = more compact)",
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes             = $"sentences={dataset.SentenceCount}, " +
                                $"bpeTokens={bpeTokens.Count}, bpeVocab={bpeTok.VocabSize}, " +
                                $"posTokens={posTokens.Count}, posVocab={posVocab.Count} " +
                                $"({string.Join(",", posVocab.Keys.Take(10))}), " +
                                $"bpeTop1={bpeTop1:P1}, posTop1={posTop1:P1}"
        });
    }

    private static long   Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
