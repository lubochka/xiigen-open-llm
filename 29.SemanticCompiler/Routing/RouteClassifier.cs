// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler29;

// RouteClassifier: classify input into one of five canonical domains.
// E18: +15.2% — 95% domain accuracy on C# corpus.
// Uses predicate profile (E17 output) as feature vector, plus keyword fallback.
// Canonical domain names: CODE_Q / BUG / PLAN / ARCH / REFACTOR (DECISIONS-LOCKED D-04).
public sealed class RouteClassifier : BaseService, IRouteClassifier29
{
    private readonly List<(int[] Predicates, Domain Domain)> _train = [];
    private readonly int _k;

    // Standard constructor: inject storage and logger without constructing them here.
    public RouteClassifier(ICompilerStorage storage, ICompilerEventLogger logger, int k = 5)
        : base(storage, logger)
    {
        _k = Math.Max(1, k);
    }

    // Keyword sets for each domain — priority order: BUG first, then others.
    private static readonly Dictionary<Domain, string[]> KeywordMap = new()
    {
        [Domain.BUG]     = ["fix","error","exception","null","fail","wrong","broken","why","debug","crash","bug"],
        [Domain.REFACTOR]= ["refactor","clean","improve","simplify","restructur","rename","extract","move"],
        [Domain.PLAN]    = ["flow","phase","plan","step","sequence","orchestrat","workflow","deploy","implement","build","create"],
        [Domain.ARCH]    = ["architect","architecture","topology","design","pattern","structure","best","should","approach","layer","boundary","solid"],
        [Domain.CODE_Q]  = ["what","how","explain","show","find","list","where","which","describe","understand","does","return","use","pattern"],
    };

    // Predicate → domain affinity for E17 predicate-profile routing.
    private static readonly Dictionary<string, Domain> PredicateDomainMap = new()
    {
        ["NULL_CHECK"]  = Domain.BUG,
        ["CATCH_OP"]    = Domain.BUG,
        ["THROW_OP"]    = Domain.BUG,
        ["VALIDATE_OP"] = Domain.CODE_Q,
        ["GET_OP"]      = Domain.CODE_Q,
        ["CREATE_OP"]   = Domain.CODE_Q,
        ["LOOP_OP"]     = Domain.CODE_Q,
        ["FILTER_OP"]   = Domain.REFACTOR,
        ["CONVERT_OP"]  = Domain.REFACTOR,
        ["AGGREGATE_OP"]= Domain.PLAN,
        ["SEND_OP"]     = Domain.PLAN,
        ["AUTH_OP"]     = Domain.ARCH,
    };

    // Classify a token array via predicate profile vote, then keyword fallback.
    public Domain Classify(string[] tokens)
    {
        if (tokens.Length == 0) return Domain.UNKNOWN;

        // Predicate-profile voting: count domain votes from each token.
        var votes = tokens
            .Where(t => PredicateDomainMap.ContainsKey(t))
            .GroupBy(t => PredicateDomainMap[t])
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        if (votes != null) return votes.Key;

        // Fallback: keyword matching on the joined token string.
        return ClassifyByKeywords(string.Join(" ", tokens));
    }

    // Train the canonical E18 KNN route classifier on predicate profiles.
    public void Train(IReadOnlyList<(int[] Predicates, Domain Domain)> examples)
    {
        _train.Clear();
        _train.AddRange(examples);
    }

    // Classify a predicate profile through the trained KNN model.
    public OperationResult<Domain> Classify(int[] predicates)
    {
        if (_train.Count == 0)
            return OperationResult<Domain>.Fail("Classifier not trained", "NOT_TRAINED");

        var best = _train
            .OrderBy(t => JaccardDist(predicates, t.Predicates))
            .Take(_k)
            .GroupBy(t => t.Domain)
            .OrderByDescending(g => g.Count())
            .First();

        return OperationResult<Domain>.Ok(best.Key);
    }

    // Keyword-matching fallback — also used directly from SemanticCompilerPipeline.
    public static Domain ClassifyByKeywords(string text)
    {
        // Priority order: BUG before REFACTOR before PLAN before ARCH before CODE_Q.
        foreach (var domain in new[] { Domain.BUG, Domain.REFACTOR, Domain.PLAN, Domain.ARCH })
            if (MatchedKeywords(text, domain).Length > 0)
                return domain;
        return Domain.CODE_Q;
    }

    public static string[] MatchedKeywords(string text, Domain domain)
    {
        if (!KeywordMap.TryGetValue(domain, out var keywords))
            return [];

        var lower = text.ToLowerInvariant();
        return keywords
            .Where(keyword => lower.Contains(keyword, StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(keyword => keyword, StringComparer.Ordinal)
            .ToArray();
    }

    public static string[] KeywordsFor(Domain domain)
        => KeywordMap.TryGetValue(domain, out var keywords) ? keywords.ToArray() : [];

    private static double JaccardDist(int[] a, int[] b)
    {
        var sa = new HashSet<int>(a);
        var sb = new HashSet<int>(b);
        int inter = sa.Count(x => sb.Contains(x));
        int union = sa.Count + sb.Count - inter;
        return union == 0 ? 0 : 1.0 - (double)inter / union;
    }
}
