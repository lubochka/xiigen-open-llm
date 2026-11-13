namespace ExperimentsV2;

// ─── Shared N-gram utilities used by E01, E03, E05 ────────────────────────────

// Perplexity formula (from RESEARCH-04):
//   perplexity = 2^(−(1/N) × Σ log2(P(correct_token_i)))
// When P = 0 (unseen context), we use a small epsilon to avoid log(0).
// Lower perplexity = model is more confident = better predictions.

internal static class NgramUtils
{
    // Small probability assigned to completely unseen events to avoid log2(0).
    internal const double Epsilon = 1e-10;

    // Compute 2^(-avg log2(p)) from a list of per-position probabilities.
    public static double Perplexity(IReadOnlyList<double> probs)
    {
        if (probs.Count == 0) return double.MaxValue;
        double sumLog = probs.Sum(p => Math.Log2(Math.Max(p, Epsilon)));
        return Math.Pow(2.0, -sumLog / probs.Count);
    }
}

// ─── Unigram model: P(token) = count(token) / total ──────────────────────────

// The unigram model ignores context entirely and predicts by raw frequency.
// It is the baseline for E03 and represents "chance-level" contextual prediction.
public sealed class UnigramModel
{
    private Dictionary<int, double>? _probs;
    private int                      _vocabSize;

    // Build the frequency table from a training sequence.
    public OperationResult<bool> Train(IReadOnlyList<int> tokens)
    {
        if (tokens.Count == 0)
            return OperationResult<bool>.Fail("Empty token list");

        _vocabSize = tokens.Max() + 1;
        var counts = new Dictionary<int, int>();
        foreach (int t in tokens)
            counts[t] = counts.GetValueOrDefault(t, 0) + 1;

        double n = tokens.Count;
        _probs = counts.ToDictionary(kv => kv.Key, kv => kv.Value / n);
        return OperationResult<bool>.Ok(true);
    }

    // Predict the most frequent token.
    public OperationResult<int> Predict()
    {
        if (_probs is null) return OperationResult<int>.Fail("Model not trained");
        return OperationResult<int>.Ok(_probs.MaxBy(kv => kv.Value).Key);
    }

    // Probability assigned to 'token'.
    public double Probability(int token)
        => _probs is not null && _probs.TryGetValue(token, out double p) ? p : NgramUtils.Epsilon;

    // Evaluate perplexity and top1-accuracy over the validation tokens.
    public OperationResult<(double perplexity, double top1, int uniqueKeys, double avgObs)>
        Evaluate(IReadOnlyList<int> tokens)
    {
        if (_probs is null) return OperationResult<(double, double, int, double)>.Fail("Not trained");
        if (tokens.Count == 0) return OperationResult<(double, double, int, double)>.Fail("Empty val");

        int best    = Predict().Value;
        int correct = 0;
        var probs   = new List<double>(tokens.Count);

        foreach (int t in tokens)
        {
            probs.Add(Probability(t));
            if (t == best) correct++;
        }

        double perp    = NgramUtils.Perplexity(probs);
        double top1    = correct / (double)tokens.Count;
        int    uniqs   = _probs!.Count;
        double avgObs  = tokens.Count / (double)(uniqs == 0 ? 1 : uniqs);
        return OperationResult<(double, double, int, double)>.Ok((perp, top1, uniqs, avgObs));
    }

    public int VocabSize => _vocabSize;
}

// ─── Bigram model: P(next|prev) = count(prev,next) / count(prev) ──────────────

// The bigram model uses one token of context.  For positions where the previous
// token was never seen in training, it falls back to the most frequent token.
public sealed class BigramModelV2
{
    private int[][]? _counts;  // counts[prev][next]
    private int      _vocabSize;
    private int      _fallback;  // most frequent token (for unseen contexts)

    // Build the bigram table from a training sequence.
    // vocabSizeHint: pass the full vocab size (train+val) so OOV val tokens
    // are within [0, vocabSizeHint) and Laplace smoothing covers them.
    public OperationResult<bool> Train(IReadOnlyList<int> tokens, int vocabSizeHint = 0)
    {
        if (tokens.Count < 2)
            return OperationResult<bool>.Fail("Need at least 2 tokens for bigram");

        _vocabSize = Math.Max(tokens.Max() + 1, vocabSizeHint);
        _counts    = new int[_vocabSize][];
        for (int i = 0; i < _vocabSize; i++) _counts[i] = new int[_vocabSize];

        var marginals = new int[_vocabSize];
        for (int i = 0; i < tokens.Count - 1; i++)
        {
            _counts[tokens[i]][tokens[i + 1]]++;
            marginals[tokens[i]]++;
        }
        // Fallback = most frequent token overall.
        _fallback = marginals.Select((c, i) => (c, i)).MaxBy(t => t.c).i;
        return OperationResult<bool>.Ok(true);
    }

    // Predict the most likely next token given 'prev', or return the fallback.
    public OperationResult<int> Predict(int prev)
    {
        if (_counts is null) return OperationResult<int>.Fail("Not trained");
        if (prev < 0 || prev >= _vocabSize) return OperationResult<int>.Ok(_fallback);
        int[] row = _counts[prev];
        int best = 0, bestCount = -1;
        for (int j = 0; j < row.Length; j++)
            if (row[j] > bestCount) { bestCount = row[j]; best = j; }
        return OperationResult<int>.Ok(best);
    }

    // P(next | prev) with Laplace add-1 smoothing — no epsilon, handles OOV.
    public double Probability(int prev, int next)
    {
        if (_counts is null) return NgramUtils.Epsilon;
        int c        = 0;
        int rowTotal = 0;
        if (prev >= 0 && prev < _vocabSize)
        {
            int[] row = _counts[prev];
            rowTotal  = row.Sum();
            if (next >= 0 && next < _vocabSize) c = row[next];
        }
        // Laplace: P = (c+1) / (rowTotal + V)  — never returns zero.
        return (c + 1.0) / (rowTotal + _vocabSize);
    }

    // Evaluate perplexity and top1-accuracy over val[1..]  (each predicted from val[i-1]).
    public OperationResult<(double perplexity, double top1, int uniqueKeys, double avgObs)>
        Evaluate(IReadOnlyList<int> tokens)
    {
        if (_counts is null) return OperationResult<(double, double, int, double)>.Fail("Not trained");
        if (tokens.Count < 2) return OperationResult<(double, double, int, double)>.Fail("Too short");

        int    correct = 0;
        int    total   = tokens.Count - 1;
        var    probs   = new List<double>(total);

        for (int i = 0; i < total; i++)
        {
            probs.Add(Probability(tokens[i], tokens[i + 1]));
            var pred = Predict(tokens[i]);
            if (pred.IsSuccess && pred.Value == tokens[i + 1]) correct++;
        }

        // Count unique non-zero bigram keys.
        int uniqs = _counts!.Sum(row => row.Count(c => c > 0));
        double avgObs = total / (double)(uniqs == 0 ? 1 : uniqs);
        return OperationResult<(double, double, int, double)>.Ok(
            (NgramUtils.Perplexity(probs), correct / (double)total, uniqs, avgObs));
    }

    public int VocabSize => _vocabSize;
}

// ─── Trigram model with bigram fallback ────────────────────────────────────────

// The trigram model uses two tokens of context.  When the pair (prev2, prev1) was
// not seen in training, it falls back to the bigram prediction for prev1.
public sealed class TrigramModelV2
{
    private Dictionary<(int, int), int[]>? _counts;
    private BigramModelV2?                  _fallback;
    private int                             _vocabSize;

    // Build the trigram table and the fallback bigram from a training sequence.
    public OperationResult<bool> Train(IReadOnlyList<int> tokens)
    {
        if (tokens.Count < 3)
            return OperationResult<bool>.Fail("Need at least 3 tokens for trigram");

        _vocabSize = tokens.Max() + 1;
        _counts    = new Dictionary<(int, int), int[]>();

        for (int i = 2; i < tokens.Count; i++)
        {
            var key = (tokens[i - 2], tokens[i - 1]);
            if (!_counts.TryGetValue(key, out var row))
            {
                row = new int[_vocabSize];
                _counts[key] = row;
            }
            row[tokens[i]]++;
        }

        _fallback = new BigramModelV2();
        _fallback.Train(tokens);
        return OperationResult<bool>.Ok(true);
    }

    // Predict the most likely next token.
    public OperationResult<int> Predict(int prev2, int prev1)
    {
        if (_counts is null || _fallback is null)
            return OperationResult<int>.Fail("Not trained");

        if (_counts.TryGetValue((prev2, prev1), out var row))
        {
            int best = 0, bestCount = -1;
            for (int j = 0; j < row.Length; j++)
                if (row[j] > bestCount) { bestCount = row[j]; best = j; }
            return OperationResult<int>.Ok(best);
        }
        return _fallback.Predict(prev1);
    }

    // P(next | prev2, prev1) with bigram fallback for unseen trigram contexts.
    public double Probability(int prev2, int prev1, int next)
    {
        if (_counts is null || _fallback is null) return NgramUtils.Epsilon;

        if (_counts.TryGetValue((prev2, prev1), out var row))
        {
            int total = row.Sum();
            if (total > 0 && next >= 0 && next < row.Length)
                return row[next] > 0 ? row[next] / (double)total : NgramUtils.Epsilon;
        }
        return _fallback.Probability(prev1, next);
    }

    // Evaluate perplexity and top1-accuracy over val[2..].
    public OperationResult<(double perplexity, double top1, int uniqueKeys, double avgObs)>
        Evaluate(IReadOnlyList<int> tokens)
    {
        if (_counts is null) return OperationResult<(double, double, int, double)>.Fail("Not trained");
        if (tokens.Count < 3) return OperationResult<(double, double, int, double)>.Fail("Too short");

        int  correct = 0;
        int  total   = tokens.Count - 2;
        var  probs   = new List<double>(total);

        for (int i = 2; i < tokens.Count; i++)
        {
            probs.Add(Probability(tokens[i - 2], tokens[i - 1], tokens[i]));
            var pred = Predict(tokens[i - 2], tokens[i - 1]);
            if (pred.IsSuccess && pred.Value == tokens[i]) correct++;
        }

        int uniqs  = _counts!.Count;
        double avg = total / (double)(uniqs == 0 ? 1 : uniqs);
        return OperationResult<(double, double, int, double)>.Ok(
            (NgramUtils.Perplexity(probs), correct / (double)total, uniqs, avg));
    }

    // Expose the set of SyntaxKind IDs observed after a given bigram context —
    // used by E05 to build the "valid continuation" mask.
    public IReadOnlySet<int> ValidContinuations(int prev2, int prev1)
    {
        if (_counts is null) return new HashSet<int>();
        if (!_counts.TryGetValue((prev2, prev1), out var row)) return new HashSet<int>();
        return row.Select((c, i) => (c, i))
                  .Where(t => t.c > 0)
                  .Select(t => t.i)
                  .ToHashSet();
    }

    public int VocabSize => _vocabSize;
}
