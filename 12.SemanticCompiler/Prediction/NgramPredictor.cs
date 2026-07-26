// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler.Prediction;

// Shared N-gram utility functions used by production prediction services.
public static class NgramUtils
{
    // Small probability assigned to unseen events so perplexity never takes log(0).
    public const double Epsilon = 1e-10;

    // Compute the standard 2^(-average log2 probability) perplexity score.
    public static double Perplexity(IReadOnlyList<double> probabilities)
    {
        if (probabilities.Count == 0)
            return double.MaxValue;

        var sumLog = probabilities.Sum(probability => Math.Log2(Math.Max(probability, Epsilon)));
        return Math.Pow(2.0, -sumLog / probabilities.Count);
    }
}

// Unigram model: P(token) = count(token) / total observations.
public sealed class UnigramModel
{
    private Dictionary<int, double>? _probabilities;
    private int _vocabSize;

    // Build token probabilities from a non-empty training sequence.
    public OperationResult<bool> Train(IReadOnlyList<int> tokens)
    {
        if (tokens.Count == 0)
            return OperationResult<bool>.Fail("Empty token list", "EMPTY_TOKENS");

        _vocabSize = tokens.Max() + 1;
        var counts = new Dictionary<int, int>();
        foreach (var token in tokens)
            counts[token] = counts.GetValueOrDefault(token, 0) + 1;

        var total = tokens.Count;
        _probabilities = counts.ToDictionary(pair => pair.Key, pair => pair.Value / (double)total);
        return OperationResult<bool>.Ok(true);
    }

    // Predict the globally most frequent token.
    public OperationResult<int> Predict()
    {
        if (_probabilities is null)
            return OperationResult<int>.Fail("Model not trained", "MODEL_NOT_TRAINED");

        return OperationResult<int>.Ok(_probabilities.MaxBy(pair => pair.Value).Key);
    }

    // Return the probability assigned to one token, with epsilon for unseen tokens.
    public double Probability(int token)
    {
        return _probabilities is not null && _probabilities.TryGetValue(token, out var probability)
            ? probability
            : NgramUtils.Epsilon;
    }

    // Evaluate perplexity and top-1 accuracy on a validation sequence.
    public OperationResult<(double perplexity, double top1, int uniqueKeys, double avgObs)> Evaluate(IReadOnlyList<int> tokens)
    {
        if (_probabilities is null)
            return OperationResult<(double, double, int, double)>.Fail("Not trained", "MODEL_NOT_TRAINED");
        if (tokens.Count == 0)
            return OperationResult<(double, double, int, double)>.Fail("Empty validation tokens", "EMPTY_TOKENS");

        var best = Predict().Value;
        var correct = 0;
        var probabilities = new List<double>(tokens.Count);
        foreach (var token in tokens)
        {
            probabilities.Add(Probability(token));
            if (token == best)
                correct++;
        }

        var uniqueKeys = _probabilities.Count;
        var averageObservations = tokens.Count / (double)Math.Max(1, uniqueKeys);
        return OperationResult<(double, double, int, double)>.Ok((
            NgramUtils.Perplexity(probabilities),
            correct / (double)tokens.Count,
            uniqueKeys,
            averageObservations));
    }

    // Expose the vocabulary size so callers can validate predicted ids.
    public int VocabSize => _vocabSize;
}

// Bigram model: P(next | prev) with Laplace smoothing.
public sealed class BigramModelV2
{
    private int[][]? _counts;
    private int _vocabSize;
    private int _fallback;

    // Build the bigram count table from a training sequence.
    public OperationResult<bool> Train(IReadOnlyList<int> tokens, int vocabSizeHint = 0)
    {
        if (tokens.Count < 2)
            return OperationResult<bool>.Fail("Need at least 2 tokens for bigram", "TOO_FEW_TOKENS");

        _vocabSize = Math.Max(tokens.Max() + 1, vocabSizeHint);
        _counts = new int[_vocabSize][];
        for (var i = 0; i < _vocabSize; i++)
            _counts[i] = new int[_vocabSize];

        var marginals = new int[_vocabSize];
        for (var i = 0; i < tokens.Count - 1; i++)
        {
            _counts[tokens[i]][tokens[i + 1]]++;
            marginals[tokens[i]]++;
        }

        _fallback = marginals.Select((count, index) => (count, index)).MaxBy(pair => pair.count).index;
        return OperationResult<bool>.Ok(true);
    }

    // Predict the most likely continuation for the previous token.
    public OperationResult<int> Predict(int prev)
    {
        if (_counts is null)
            return OperationResult<int>.Fail("Not trained", "MODEL_NOT_TRAINED");
        if (prev < 0 || prev >= _vocabSize)
            return OperationResult<int>.Ok(_fallback);

        var row = _counts[prev];
        var best = 0;
        var bestCount = -1;
        for (var next = 0; next < row.Length; next++)
        {
            if (row[next] <= bestCount)
                continue;

            bestCount = row[next];
            best = next;
        }

        return OperationResult<int>.Ok(best);
    }

    // Return smoothed P(next | prev), including safe handling for unseen contexts.
    public double Probability(int prev, int next)
    {
        if (_counts is null)
            return NgramUtils.Epsilon;

        var count = 0;
        var rowTotal = 0;
        if (prev >= 0 && prev < _vocabSize)
        {
            var row = _counts[prev];
            rowTotal = row.Sum();
            if (next >= 0 && next < _vocabSize)
                count = row[next];
        }

        return (count + 1.0) / (rowTotal + _vocabSize);
    }

    // Evaluate perplexity and top-1 accuracy over every predictable validation position.
    public OperationResult<(double perplexity, double top1, int uniqueKeys, double avgObs)> Evaluate(IReadOnlyList<int> tokens)
    {
        if (_counts is null)
            return OperationResult<(double, double, int, double)>.Fail("Not trained", "MODEL_NOT_TRAINED");
        if (tokens.Count < 2)
            return OperationResult<(double, double, int, double)>.Fail("Too short", "TOO_FEW_TOKENS");

        var correct = 0;
        var total = tokens.Count - 1;
        var probabilities = new List<double>(total);
        for (var i = 0; i < total; i++)
        {
            probabilities.Add(Probability(tokens[i], tokens[i + 1]));
            var predicted = Predict(tokens[i]);
            if (predicted.IsSuccess && predicted.Value == tokens[i + 1])
                correct++;
        }

        var uniqueKeys = _counts.Sum(row => row.Count(count => count > 0));
        var averageObservations = total / (double)Math.Max(1, uniqueKeys);
        return OperationResult<(double, double, int, double)>.Ok((
            NgramUtils.Perplexity(probabilities),
            correct / (double)total,
            uniqueKeys,
            averageObservations));
    }

    // Count observed bigram rows so the facade can decide whether bigram data is dense enough.
    public int UniqueBigramCount => _counts?.Sum(row => row.Count(count => count > 0)) ?? 0;

    // Expose the vocabulary size so grammar filters can stay inside the same token id range.
    public int VocabSize => _vocabSize;
}

// Trigram model with bigram fallback for unseen two-token contexts.
public sealed class TrigramModelV2
{
    private Dictionary<(int Prev2, int Prev1), int[]>? _counts;
    private BigramModelV2? _fallback;
    private int _vocabSize;

    // Build trigram rows and the fallback bigram model from one sequence.
    public OperationResult<bool> Train(IReadOnlyList<int> tokens)
    {
        if (tokens.Count < 3)
            return OperationResult<bool>.Fail("Need at least 3 tokens for trigram", "TOO_FEW_TOKENS");

        _vocabSize = tokens.Max() + 1;
        _counts = new Dictionary<(int Prev2, int Prev1), int[]>();
        for (var i = 2; i < tokens.Count; i++)
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
        return _fallback.Train(tokens);
    }

    // Predict the highest-count next token for a two-token context.
    public OperationResult<int> Predict(int prev2, int prev1)
    {
        if (_counts is null || _fallback is null)
            return OperationResult<int>.Fail("Not trained", "MODEL_NOT_TRAINED");

        if (!_counts.TryGetValue((prev2, prev1), out var row))
            return _fallback.Predict(prev1);

        var best = 0;
        var bestCount = -1;
        for (var next = 0; next < row.Length; next++)
        {
            if (row[next] <= bestCount)
                continue;

            bestCount = row[next];
            best = next;
        }

        return OperationResult<int>.Ok(best);
    }

    // Return P(next | prev2, prev1), falling back to bigram when the context is unknown.
    public double Probability(int prev2, int prev1, int next)
    {
        if (_counts is null || _fallback is null)
            return NgramUtils.Epsilon;

        if (_counts.TryGetValue((prev2, prev1), out var row))
        {
            var total = row.Sum();
            if (total > 0 && next >= 0 && next < row.Length)
                return row[next] > 0 ? row[next] / (double)total : NgramUtils.Epsilon;
        }

        return _fallback.Probability(prev1, next);
    }

    // Evaluate trigram predictions over every validation position with two previous tokens.
    public OperationResult<(double perplexity, double top1, int uniqueKeys, double avgObs)> Evaluate(IReadOnlyList<int> tokens)
    {
        if (_counts is null)
            return OperationResult<(double, double, int, double)>.Fail("Not trained", "MODEL_NOT_TRAINED");
        if (tokens.Count < 3)
            return OperationResult<(double, double, int, double)>.Fail("Too short", "TOO_FEW_TOKENS");

        var correct = 0;
        var total = tokens.Count - 2;
        var probabilities = new List<double>(total);
        for (var i = 2; i < tokens.Count; i++)
        {
            probabilities.Add(Probability(tokens[i - 2], tokens[i - 1], tokens[i]));
            var predicted = Predict(tokens[i - 2], tokens[i - 1]);
            if (predicted.IsSuccess && predicted.Value == tokens[i])
                correct++;
        }

        var uniqueKeys = _counts.Count;
        var averageObservations = total / (double)Math.Max(1, uniqueKeys);
        return OperationResult<(double, double, int, double)>.Ok((
            NgramUtils.Perplexity(probabilities),
            correct / (double)total,
            uniqueKeys,
            averageObservations));
    }

    // Expose the observed continuation set for grammar-mask filtering.
    public IReadOnlySet<int> ValidContinuations(int prev2, int prev1)
    {
        if (_counts is null || !_counts.TryGetValue((prev2, prev1), out var row))
            return new HashSet<int>();

        return row.Select((count, index) => (count, index))
            .Where(pair => pair.count > 0)
            .Select(pair => pair.index)
            .ToHashSet();
    }

    // Expose the vocabulary size so callers can validate predicted ids.
    public int VocabSize => _vocabSize;
}

// Production facade over E03 N-gram behavior.
public sealed class NgramPredictor
{
    private UnigramModel? _unigram;
    private BigramModelV2? _bigram;
    private IReadOnlySet<int> _vocabulary = new HashSet<int>();

    // Train unigram and, when dense enough, bigram prediction for the same token stream.
    public OperationResult<bool> Train(IReadOnlyList<int> tokens)
    {
        if (tokens.Count == 0)
            return OperationResult<bool>.Fail("Empty token list", "EMPTY_TOKENS");

        _vocabulary = tokens.ToHashSet();
        _unigram = new UnigramModel();
        var unigramTrain = _unigram.Train(tokens);
        if (!unigramTrain.IsSuccess)
            return unigramTrain;

        OptimalN = 1;
        _bigram = null;
        if (tokens.Count < 4)
            return OperationResult<bool>.Ok(true);

        var bigram = new BigramModelV2();
        var bigramTrain = bigram.Train(tokens);
        if (!bigramTrain.IsSuccess)
            return OperationResult<bool>.Ok(true);

        var averageObservations = (tokens.Count - 1) / (double)Math.Max(1, bigram.UniqueBigramCount);
        _bigram = bigram;
        if (averageObservations >= 3.0)
            OptimalN = 2;

        return OperationResult<bool>.Ok(true);
    }

    // Predict the next token, preferring bigram when it has enough observations.
    public int PredictNext(int prev)
    {
        if (OptimalN == 2 && _bigram is not null)
        {
            var predicted = _bigram.Predict(prev);
            if (predicted.IsSuccess)
                return predicted.Value;
        }

        return _unigram?.Predict().Value ?? 0;
    }

    // Return the probability assigned by the active model to one continuation.
    public double Probability(int prev, int token)
    {
        return OptimalN == 2 && _bigram is not null
            ? _bigram.Probability(prev, token)
            : _unigram?.Probability(token) ?? NgramUtils.Epsilon;
    }

    // Check whether a token id belongs to the training vocabulary.
    public bool IsKnownToken(int token)
    {
        return _vocabulary.Contains(token);
    }

    // Record which model order is currently active for observability and tests.
    public int OptimalN { get; private set; } = 1;

    // Expose the active vocabulary size as the maximum trained id plus one.
    public int VocabSize => _unigram?.VocabSize ?? 0;
}
