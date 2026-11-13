namespace MachineLearning;

// Multinomial Naive Bayes classifier for text classification.
// P(class|doc) ∝ P(class) × Π P(word|class)
// Laplace smoothing prevents zero probabilities for unseen words.
public sealed class NaiveBayesAlgorithm
{
    private Dictionary<string, double>             _classPriors       = [];
    private Dictionary<string, Dictionary<string, double>> _wordProbs = [];
    private bool _fitted;

    public OperationResult<bool> Fit(List<string[]> documents, List<string> labels)
    {
        if (documents.Count == 0)
            return OperationResult<bool>.Fail("No training documents");
        if (documents.Count != labels.Count)
            return OperationResult<bool>.Fail("Document/label count mismatch");

        var classes   = labels.Distinct().ToList();
        int totalDocs = documents.Count;
        _classPriors  = [];
        _wordProbs    = [];

        foreach (string cls in classes)
        {
            var classDocs = documents
                .Select((d, i) => (d, i))
                .Where(t => labels[t.i] == cls)
                .Select(t => t.d)
                .ToList();

            _classPriors[cls] = Math.Log((double)classDocs.Count / totalDocs);

            var wordCounts   = new Dictionary<string, int>();
            int totalWords   = 0;

            foreach (var doc in classDocs)
                foreach (var word in doc)
                {
                    wordCounts[word] = wordCounts.GetValueOrDefault(word) + 1;
                    totalWords++;
                }

            // Collect vocabulary from all docs for this class
            var vocab = documents.SelectMany(d => d).Distinct().ToList();
            _wordProbs[cls] = [];

            foreach (var word in vocab)
            {
                int count = wordCounts.GetValueOrDefault(word);
                // Laplace smoothing: (count + 1) / (total + vocabSize)
                _wordProbs[cls][word] = Math.Log((count + 1.0) / (totalWords + vocab.Count));
            }
        }

        _fitted = true;
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<string> Classify(string[] document)
    {
        if (!_fitted) return OperationResult<string>.Fail("Model not fitted");
        if (document.Length == 0) return OperationResult<string>.Fail("Empty document");

        string? best  = null;
        double  bestScore = double.NegativeInfinity;

        foreach (var (cls, prior) in _classPriors)
        {
            double score = prior;
            if (_wordProbs.TryGetValue(cls, out var probs))
                foreach (var word in document)
                    if (probs.TryGetValue(word, out double p))
                        score += p;
            if (score > bestScore) { bestScore = score; best = cls; }
        }

        return best is null
            ? OperationResult<string>.Fail("No classes available")
            : OperationResult<string>.Ok(best);
    }
}

public sealed class NaiveBayesService : BaseService
{
    private readonly NaiveBayesAlgorithm _algo = new();

    public NaiveBayesService(IModelStateStorage storage, IPredictionLogger logger)
        : base(storage, logger) { }

    public OperationResult<bool> Train(
        string operationId, List<string[]> docs, List<string> labels)
    {
        if (Idempotency.TryGetResult(operationId, out _))
            return OperationResult<bool>.Ok(true);

        var result = _algo.Fit(docs, labels);
        if (result.IsSuccess)
        {
            Idempotency.Record(operationId, new() { ["trained"] = true });
            Storage.Save("naive_bayes", new() { ["fitted"] = true });
        }
        return result;
    }

    public OperationResult<string> Classify(string[] document)
    {
        var result = _algo.Classify(document);
        if (result.IsSuccess)
            Logger.Log("NaiveBayes",
                new() { ["tokens"] = document },
                new() { ["class"]  = result.Value! });
        return result;
    }
}
