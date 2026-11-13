namespace Experiments;

// ─── Bigram (order-1) Markov chain over integer tokens ────────────────────────

// Builds a transition-count table from a training sequence.
// Predicts the next token as argmax of the row for the current token.
// The vocabulary size is inferred from the maximum token value seen during training.
public sealed class BigramMarkov
{
    // transitionCounts[prev][next] = number of times 'next' followed 'prev'.
    private int[][]? _counts;

    // Size of the token vocabulary (max token value + 1).
    private int _vocabSize;

    // ── Training ──────────────────────────────────────────────────────────────

    // Build the bigram table from 'tokens'.  The list must contain at least 2 items.
    public OperationResult<bool> Train(IReadOnlyList<int> tokens)
    {
        if (tokens.Count < 2)
            return OperationResult<bool>.Fail("Need at least 2 tokens to build bigram");

        // Determine vocabulary size from the maximum token id seen.
        _vocabSize = tokens.Max() + 1;

        // Allocate a jagged array: each row is a count vector over the whole vocab.
        _counts = new int[_vocabSize][];
        for (int i = 0; i < _vocabSize; i++)
            _counts[i] = new int[_vocabSize];

        // Tally each consecutive pair (prev, next).
        for (int i = 0; i < tokens.Count - 1; i++)
            _counts[tokens[i]][tokens[i + 1]]++;

        return OperationResult<bool>.Ok(true);
    }

    // ── Prediction ────────────────────────────────────────────────────────────

    // Predict the most likely next token given 'prev'.
    // Returns -1 (as success) when 'prev' was never seen in training.
    public OperationResult<int> Predict(int prev)
    {
        if (_counts is null)
            return OperationResult<int>.Fail("Model not trained");
        if (prev < 0 || prev >= _vocabSize)
            return OperationResult<int>.Ok(-1);  // unseen token → no prediction

        // Find the column with the highest count in the row for 'prev'.
        int best = 0, bestCount = -1;
        int[] row = _counts[prev];
        for (int j = 0; j < row.Length; j++)
        {
            if (row[j] > bestCount) { bestCount = row[j]; best = j; }
        }
        return OperationResult<int>.Ok(best);
    }

    // ── Accuracy evaluation ───────────────────────────────────────────────────

    // Run prediction on every consecutive pair in 'tokens' and return the fraction
    // of correct predictions.  Pairs where Predict returns -1 count as wrong.
    public OperationResult<double> Evaluate(IReadOnlyList<int> tokens)
    {
        if (tokens.Count < 2)
            return OperationResult<double>.Fail("Need at least 2 tokens to evaluate");

        int correct = 0, total = tokens.Count - 1;
        for (int i = 0; i < total; i++)
        {
            var pred = Predict(tokens[i]);
            if (pred.IsSuccess && pred.Value == tokens[i + 1])
                correct++;
        }
        return OperationResult<double>.Ok(correct / (double)total);
    }

    // Number of token types in the vocabulary.
    public int VocabSize => _vocabSize;
}

// ─── Trigram (order-2) Markov chain over integer tokens ───────────────────────

// Builds a sparse map (prev2, prev1) → next-token counts.
// Falls back to a pre-built BigramMarkov when the trigram key is not found,
// so every prediction is always deterministic.
public sealed class TrigramMarkov
{
    // Sparse table: key = (prev2, prev1), value = count array indexed by next token.
    private Dictionary<(int, int), int[]>? _trigramCounts;

    // Fallback bigram used when the trigram context is unseen.
    private BigramMarkov? _fallback;

    // Size of the vocabulary, inferred from training data.
    private int _vocabSize;

    // ── Training ──────────────────────────────────────────────────────────────

    // Build both the trigram table and a fallback bigram from 'tokens'.
    public OperationResult<bool> Train(IReadOnlyList<int> tokens)
    {
        if (tokens.Count < 3)
            return OperationResult<bool>.Fail("Need at least 3 tokens for trigram");

        _vocabSize     = tokens.Max() + 1;
        _trigramCounts = new Dictionary<(int, int), int[]>();

        for (int i = 2; i < tokens.Count; i++)
        {
            var key = (tokens[i - 2], tokens[i - 1]);
            if (!_trigramCounts.TryGetValue(key, out var row))
            {
                row = new int[_vocabSize];
                _trigramCounts[key] = row;
            }
            row[tokens[i]]++;
        }

        // Train the bigram fallback on the same data.
        _fallback = new BigramMarkov();
        _fallback.Train(tokens);

        return OperationResult<bool>.Ok(true);
    }

    // ── Prediction ────────────────────────────────────────────────────────────

    // Predict next token given the two preceding tokens.
    // Falls back to bigram prediction if the pair (prev2, prev1) was not seen.
    public OperationResult<int> Predict(int prev2, int prev1)
    {
        if (_trigramCounts is null || _fallback is null)
            return OperationResult<int>.Fail("Model not trained");

        if (_trigramCounts.TryGetValue((prev2, prev1), out var row))
        {
            // Trigram hit: argmax over the row.
            int best = 0, bestCount = -1;
            for (int j = 0; j < row.Length; j++)
                if (row[j] > bestCount) { bestCount = row[j]; best = j; }
            return OperationResult<int>.Ok(best);
        }

        // Trigram miss: fall back to bigram on prev1.
        return _fallback.Predict(prev1);
    }

    // ── Accuracy evaluation ───────────────────────────────────────────────────

    // Evaluate prediction accuracy over every trigram in 'tokens'.
    public OperationResult<double> Evaluate(IReadOnlyList<int> tokens)
    {
        if (tokens.Count < 3)
            return OperationResult<double>.Fail("Need at least 3 tokens to evaluate trigram");

        int correct = 0, total = tokens.Count - 2;
        for (int i = 2; i < tokens.Count; i++)
        {
            var pred = Predict(tokens[i - 2], tokens[i - 1]);
            if (pred.IsSuccess && pred.Value == tokens[i])
                correct++;
        }
        return OperationResult<double>.Ok(correct / (double)total);
    }
}
