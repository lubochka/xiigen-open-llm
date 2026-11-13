namespace SemanticCompiler;

// RouteClassifier classifies the domain of a request by predicate profile.
// E18: 95% domain accuracy with K-NN (K-Nearest Neighbours) on predicate sets.
// Domains follow D-04 canonical names: CODE_Q, BUG, PLAN, ARCH, REFACTOR.
public sealed class RouteClassifier
{
    // Keyword sets for baseline classification — also used to auto-label training data
    private static readonly Dictionary<DomainLabel, string[]> Keywords = new()
    {
        [DomainLabel.BUG]     = ["fix","error","exception","null","fail","wrong","broken","why","debug"],
        [DomainLabel.REFACTOR]= ["refactor","clean","improve","simplify","restructur","rename"],
        [DomainLabel.CODE_Q]  = ["what","how","explain","show","find","list","where","which"],
        [DomainLabel.PLAN]    = ["flow","phase","plan","step","sequence","orchestrat","workflow","deploy"],
        [DomainLabel.ARCH]    = ["design","pattern","structure","architect","best","should","approach"],
    };

    private readonly List<(int[] Predicates, DomainLabel Domain)> _train = [];
    private readonly int _k;

    public RouteClassifier(int k = 5) => _k = k;

    public void Train(IReadOnlyList<(int[] Predicates, DomainLabel Domain)> examples)
    {
        _train.Clear();
        _train.AddRange(examples);
    }

    // Enhanced: K-NN on predicate sets (Jaccard distance)
    public OperationResult<DomainLabel> Classify(int[] predicates)
    {
        if (_train.Count == 0)
            return OperationResult<DomainLabel>.Fail("Classifier not trained", "NOT_TRAINED");

        var best = _train
            .OrderBy(t => JaccardDist(predicates, t.Predicates))
            .Take(_k)
            .GroupBy(t => t.Domain)
            .OrderByDescending(g => g.Count())
            .First();

        return OperationResult<DomainLabel>.Ok(best.Key);
    }

    // Baseline: keyword matching (fast, no training required)
    public static DomainLabel ClassifyByKeywords(string prompt)
    {
        var lower = prompt.ToLowerInvariant();
        // Priority order: BUG before REFACTOR before PLAN before ARCH before CODE_Q
        foreach (var domain in new[] { DomainLabel.BUG, DomainLabel.REFACTOR,
                                        DomainLabel.PLAN, DomainLabel.ARCH })
            if (Keywords[domain].Any(kw => lower.Contains(kw)))
                return domain;
        return DomainLabel.CODE_Q;
    }

    // Auto-label a prompt for training data generation
    public static DomainLabel AutoLabel(string prompt) => ClassifyByKeywords(prompt);

    private static double JaccardDist(int[] a, int[] b)
    {
        var sa = new HashSet<int>(a); var sb = new HashSet<int>(b);
        int inter = sa.Count(x => sb.Contains(x));
        int union  = sa.Count + sb.Count - inter;
        return union == 0 ? 0 : 1.0 - (double)inter / union;
    }
}
