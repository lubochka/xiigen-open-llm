using System.Text.RegularExpressions;

namespace Experiments;

// ─── E08: Bigram + POS filter vs plain bigram on natural language ──────────────
//
// Hypothesis: simple regex-based POS rules (no ML) narrow the candidate set
// and improve word prediction accuracy relative to a raw bigram.
//
// Baseline: bigram on word tokens (= E07 enhanced).
// Enhanced: same bigram, but constrained by a set of pattern rules that whitelist
//           only grammatically plausible next words.
public sealed class E08NaturalLanguageGrammar
{
    // Articles and prepositions — used by POS filter rules
    private static readonly HashSet<string> Articles     = ["the", "a", "an"];
    private static readonly HashSet<string> Prepositions = ["in", "on", "at", "of", "to", "from", "with", "by", "for", "into", "upon"];
    private static readonly HashSet<string> SaidWords    = ["said", "asked", "cried", "replied", "whispered", "shouted", "thought"];
    private static readonly Regex           AdjectiveSuffix = new(@"(?:ful|less|ous|al|ive|able|ible|ing|ed)$", RegexOptions.Compiled);

    public OperationResult<ExperimentResult> Run(AliceDataset data, int scale = 0)
    {
        if (data.Tokens.Count < 20)
            return OperationResult<ExperimentResult>.Fail("Too few word tokens for E08");

        int split = (int)(data.Tokens.Count * 0.8);
        var train = data.Tokens.Take(split).ToList();
        var val   = data.Tokens.Skip(split).ToList();

        // Reverse vocab map for string lookup
        var idToWord = data.Vocab.ToDictionary(kv => kv.Value, kv => kv.Key);

        // ── Baseline: plain bigram ────────────────────────────────────────────────
        long basMemStart = GC.GetAllocatedBytesForCurrentThread();
        var  t0          = System.Diagnostics.Stopwatch.GetTimestamp();

        var bigram = new BigramMarkov();
        bigram.Train(train);

        int baseCorrect = 0, baseTotal = val.Count - 1;
        for (int i = 0; i < baseTotal; i++)
        {
            var pred = bigram.Predict(val[i]);
            if (pred.IsSuccess && pred.Value == val[i + 1]) baseCorrect++;
        }
        double baselineAcc   = baseTotal > 0 ? baseCorrect / (double)baseTotal : 0;
        double baselineMs    = ElapsedMs(t0);
        double baselineMemMb = (GC.GetAllocatedBytesForCurrentThread() - basMemStart) / 1_048_576.0;

        // ── Enhanced: bigram + POS filter ────────────────────────────────────────
        long enhMemStart = GC.GetAllocatedBytesForCurrentThread();
        var  t1          = System.Diagnostics.Stopwatch.GetTimestamp();

        // Build vocabulary list for candidate filtering
        var vocabTokens = data.Vocab.Values.ToHashSet();

        int enhCorrect   = 0;
        int filteredPred = 0;  // how many times filter changed prediction
        for (int i = 0; i < val.Count - 1; i++)
        {
            int curId = val[i];
            string curWord = idToWord.GetValueOrDefault(curId, "");

            // Get allowed candidates from POS filter
            var allowed = GetAllowedFollowers(curWord, idToWord, data.Vocab);

            int prediction;
            if (allowed != null && allowed.Count > 0)
            {
                // Pick the highest-bigram-frequency word from the allowed set
                prediction = BestAllowed(bigram, curId, allowed);
                if (prediction != bigram.Predict(curId).Value) filteredPred++;
            }
            else
            {
                var pred = bigram.Predict(curId);
                prediction = pred.IsSuccess ? pred.Value : -1;
            }

            if (prediction == val[i + 1]) enhCorrect++;
        }
        double enhancedAcc   = (val.Count - 1) > 0 ? enhCorrect / (double)(val.Count - 1) : 0;
        double enhancedMs    = ElapsedMs(t1);
        double enhancedMemMb = (GC.GetAllocatedBytesForCurrentThread() - enhMemStart) / 1_048_576.0;

        return OperationResult<ExperimentResult>.Ok(new ExperimentResult
        {
            ExperimentId = "E08",
            Scale        = 0,
            Baseline = new RunMetrics
            {
                AccuracyFraction  = baselineAcc,
                TimeMs            = baselineMs,
                MemoryMb          = baselineMemMb,
                DatasetTokenCount = data.Tokens.Count
            },
            Enhanced = new RunMetrics
            {
                AccuracyFraction  = enhancedAcc,
                TimeMs            = enhancedMs,
                MemoryMb          = enhancedMemMb,
                DatasetTokenCount = data.Tokens.Count
            },
            Notes = $"words={data.Tokens.Count}, vocab={data.Vocab.Count}, " +
                    $"filteredPredictions={filteredPred}/{val.Count - 1} " +
                    $"({filteredPred * 100.0 / Math.Max(1, val.Count - 1):F1}%), " +
                    $"baseAcc={baselineAcc * 100:F1}%, enhAcc={enhancedAcc * 100:F1}%"
        });
    }

    // Returns a set of allowed next word IDs based on grammar rules, or null if no rule applies.
    private HashSet<int>? GetAllowedFollowers(
        string curWord,
        Dictionary<int, string> idToWord,
        IReadOnlyDictionary<string, int> vocab)
    {
        if (string.IsNullOrEmpty(curWord)) return null;

        // Rule 1: after article → only words that are adjectives or nouns (not articles/prepositions)
        if (Articles.Contains(curWord))
        {
            return vocab
                .Where(kv => !Articles.Contains(kv.Key) && !Prepositions.Contains(kv.Key))
                .Select(kv => kv.Value)
                .ToHashSet();
        }

        // Rule 2: after preposition → not another preposition or article
        if (Prepositions.Contains(curWord))
        {
            return vocab
                .Where(kv => !Prepositions.Contains(kv.Key))
                .Select(kv => kv.Value)
                .ToHashSet();
        }

        // Rule 3: after speech verbs ("said", "asked", …) → likely "the", "a", "an", "i", "it", "she", "he"
        if (SaidWords.Contains(curWord))
        {
            var speechFollowers = new HashSet<string> { "the", "a", "an", "i", "it", "she", "he", "alice", "that", "you" };
            var result = new HashSet<int>();
            foreach (var w in speechFollowers)
                if (vocab.TryGetValue(w, out int id)) result.Add(id);
            return result.Count > 0 ? result : null;
        }

        return null;
    }

    // Among the allowed token IDs, return the one with highest bigram frequency from curId.
    private static int BestAllowed(BigramMarkov bigram, int curId, HashSet<int> allowed)
    {
        // Use bigram to find the best-allowed prediction by trying each allowed token
        // We pick the allowed token that the bigram assigns the highest count to.
        // Since BigramMarkov doesn't expose counts, we fall back: predict normally and check if allowed.
        var defaultPred = bigram.Predict(curId);
        if (defaultPred.IsSuccess && allowed.Contains(defaultPred.Value))
            return defaultPred.Value;

        // If the top bigram prediction is not allowed, return the first allowed token we know of.
        return allowed.First();
    }

    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
