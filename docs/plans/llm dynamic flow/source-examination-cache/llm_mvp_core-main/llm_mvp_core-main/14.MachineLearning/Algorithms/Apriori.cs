namespace MachineLearning;

// Apriori algorithm for association rule mining.
// Generates frequent itemsets level-by-level (BFS) then derives rules.
public sealed class AprioriAlgorithm
{
    public sealed record FrequentItemset(IReadOnlyList<string> Items, double Support);

    public sealed record AssociationRule(
        IReadOnlyList<string> Antecedent,
        IReadOnlyList<string> Consequent,
        double Support,
        double Confidence);

    public OperationResult<(List<FrequentItemset>, List<AssociationRule>)> Mine(
        List<List<string>> transactions,
        double minimumSupport    = 0.5,
        double minimumConfidence = 0.7)
    {
        if (transactions.Count == 0)
            return OperationResult<(List<FrequentItemset>, List<AssociationRule>)>.Fail("No transactions");

        int total = transactions.Count;

        // ── Generate frequent 1-itemsets ──────────────────────────────────
        var frequentSets = new Dictionary<string, double>(); // key=canonical, value=support

        var itemCounts = new Dictionary<string, int>();
        foreach (var t in transactions)
            foreach (var item in t.Distinct())
                itemCounts[item] = itemCounts.GetValueOrDefault(item) + 1;

        var freq1 = itemCounts
            .Where(kv => (double)kv.Value / total >= minimumSupport)
            .Select(kv => new[] { kv.Key }.ToList())
            .ToList();

        var allFrequent = new List<FrequentItemset>();
        var currentLevel = new List<List<string>>();

        foreach (var item in freq1)
        {
            string key     = Key(item);
            double support = (double)itemCounts[item[0]] / total;
            frequentSets[key] = support;
            allFrequent.Add(new FrequentItemset(item, support));
            currentLevel.Add(item);
        }

        // ── Generate larger itemsets via Apriori join ─────────────────────
        while (currentLevel.Count > 0)
        {
            var candidates = new List<List<string>>();
            for (int i = 0; i < currentLevel.Count; i++)
                for (int j = i + 1; j < currentLevel.Count; j++)
                {
                    var a = currentLevel[i];
                    var b = currentLevel[j];
                    // Join if first k-1 elements equal
                    if (a.Count == 1 || a.Take(a.Count - 1).SequenceEqual(b.Take(b.Count - 1)))
                    {
                        var candidate = a.Concat(new[] { b[^1] }).Distinct().OrderBy(x => x).ToList();
                        if (candidate.Count == a.Count + 1)
                            candidates.Add(candidate);
                    }
                }

            var nextLevel = new List<List<string>>();
            foreach (var candidate in candidates)
            {
                int count = transactions.Count(t =>
                    candidate.All(item => t.Contains(item)));
                double support = (double)count / total;
                if (support >= minimumSupport)
                {
                    string key = Key(candidate);
                    if (!frequentSets.ContainsKey(key))
                    {
                        frequentSets[key] = support;
                        allFrequent.Add(new FrequentItemset(candidate, support));
                    }
                    nextLevel.Add(candidate);
                }
            }
            currentLevel = nextLevel;
        }

        // ── Generate association rules ────────────────────────────────────
        var rules = new List<AssociationRule>();
        foreach (var fs in allFrequent.Where(f => f.Items.Count >= 2))
        {
            foreach (var consequent in NonEmptySubsets(fs.Items.ToList()))
            {
                var antecedent = fs.Items.Except(consequent).OrderBy(x => x).ToList();
                if (antecedent.Count == 0) continue;

                if (!frequentSets.TryGetValue(Key(antecedent), out double antSupport))
                    continue;

                double confidence = fs.Support / antSupport;
                if (confidence >= minimumConfidence)
                    rules.Add(new AssociationRule(antecedent, consequent, fs.Support, confidence));
            }
        }

        return OperationResult<(List<FrequentItemset>, List<AssociationRule>)>.Ok(
            (allFrequent, rules));
    }

    private static string Key(IEnumerable<string> items)
        => string.Join("|", items.OrderBy(x => x));

    private static IEnumerable<List<string>> NonEmptySubsets(List<string> items)
    {
        int n = items.Count;
        for (int mask = 1; mask < (1 << n); mask++)
        {
            var subset = new List<string>();
            for (int i = 0; i < n; i++)
                if ((mask & (1 << i)) != 0)
                    subset.Add(items[i]);
            yield return subset;
        }
    }
}
