namespace ExperimentsV2;

// DomainClassifier classifies the domain of a request by predicate profile.
// Five domains: CODE_QUESTION, BUG_FIX, CODE_CHANGE, ORCHESTRATION, ARCHITECTURE.
public sealed class DomainClassifier
{
    public const int CODE_QUESTION = 0;
    public const int BUG_FIX       = 1;
    public const int CODE_CHANGE   = 2;
    public const int ORCHESTRATION = 3;
    public const int ARCHITECTURE  = 4;

    public static readonly string[] DomainNames =
        ["CODE_QUESTION", "BUG_FIX", "CODE_CHANGE", "ORCHESTRATION", "ARCHITECTURE"];

    private static readonly Dictionary<int, string[]> Keywords = new()
    {
        [BUG_FIX]       = ["fix","error","exception","null","fail","wrong","broken","why","debug"],
        [CODE_CHANGE]   = ["add","create","update","refactor","remove","implement","write","change"],
        [CODE_QUESTION] = ["what","how","explain","show","find","list","where","which"],
        [ORCHESTRATION] = ["flow","phase","plan","step","sequence","orchestrat","workflow"],
        [ARCHITECTURE]  = ["design","pattern","structure","architect","best","should","approach"],
    };

    private readonly List<(int[] Predicates, int Domain)> _train = new();
    private readonly int _k;

    public DomainClassifier(int k = 5) => _k = k;

    public void Train(IReadOnlyList<(int[] Predicates, int Domain)> examples)
    {
        _train.Clear();
        _train.AddRange(examples);
    }

    // Enhanced: K-NN on predicate sets
    public int Predict(int[] predicates)
    {
        if (_train.Count == 0) return CODE_QUESTION;
        return _train
            .OrderBy(t => JaccardDist(predicates, t.Predicates))
            .Take(_k)
            .GroupBy(t => t.Domain)
            .OrderByDescending(g => g.Count())
            .First().Key;
    }

    // Baseline: keyword matching on normalized text
    public static int PredictByKeywords(string text)
    {
        var lower = text.ToLowerInvariant();
        foreach (var domain in new[] { BUG_FIX, CODE_CHANGE, ORCHESTRATION, ARCHITECTURE })
            if (Keywords[domain].Any(kw => lower.Contains(kw)))
                return domain;
        return CODE_QUESTION;
    }

    // Auto-label a prompt for dataset generation (same rule as baseline)
    public static int AutoLabel(string prompt) => PredictByKeywords(prompt);

    private static double JaccardDist(int[] a, int[] b)
    {
        var sa = new HashSet<int>(a); var sb = new HashSet<int>(b);
        int inter = sa.Count(x => sb.Contains(x));
        int union  = sa.Count + sb.Count - inter;
        return union == 0 ? 0 : 1.0 - (double)inter / union;
    }
}
