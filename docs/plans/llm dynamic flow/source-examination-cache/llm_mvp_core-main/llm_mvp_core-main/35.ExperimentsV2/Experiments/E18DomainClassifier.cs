namespace ExperimentsV2;

// E18: DomainClassifier — request domain from predicate profile.
//
// Hypothesis: the predicate profile of a prompt (E17) contains enough signal
// to classify domain without relying on explicit keywords.
//
// Baseline:  keyword matching (BUG_FIX if "fix"/"error", CODE_CHANGE if "add"/"create", …)
// Enhanced:  K-NN (Jaccard on predicate sets derived from FamilyCollapser tags)
// Dataset:   300 synthetic prompts, auto-labelled by keyword rules
// Primary metric: top1_accuracy (5 classes, random = 20%)
// Scaling signal: ambiguity_ratio (prompts without obvious keywords)
public sealed class E18DomainClassifier
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var examples = GenerateExamples(seed: 42, count: tier * 100);
        int split    = (int)(examples.Count * 0.8);
        var train    = examples.Take(split).ToList();
        var val      = examples.Skip(split).ToList();

        // Train K-NN on predicate vectors derived from prompts
        var classifier = new DomainClassifier(k: 5);
        classifier.Train(train.Select(e => (TextToPredicates(e.Prompt), e.Domain)).ToList());

        // ── Baseline: keyword matching ────────────────────────────────────────
        var t0 = Stopwatch();
        int baseCorrect   = val.Count(e => DomainClassifier.PredictByKeywords(e.Prompt) == e.Domain);
        double baselineAcc = baseCorrect / (double)val.Count;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: K-NN on predicates ──────────────────────────────────────
        var t1 = Stopwatch();
        int enhCorrect   = val.Count(e => classifier.Predict(TextToPredicates(e.Prompt)) == e.Domain);
        double enhancedAcc = enhCorrect / (double)val.Count;
        double enhancedMs  = ElapsedMs(t1);

        string perDomain = string.Join(", ",
            Enumerable.Range(0, DomainClassifier.DomainNames.Length).Select(d =>
            {
                var dVal = val.Where(e => e.Domain == d).ToList();
                if (dVal.Count == 0) return $"{DomainClassifier.DomainNames[d]}=N/A";
                int c = dVal.Count(e => classifier.Predict(TextToPredicates(e.Prompt)) == d);
                return $"{DomainClassifier.DomainNames[d]}={c}/{dVal.Count}";
            }));

        double ambiguityRatio = examples.Count > 0
            ? (double)examples.Count(e => IsAmbiguous(e.Prompt)) / examples.Count
            : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E18-DOMAIN-CLASSIFIER",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = ambiguityRatio,
            ScalingSignalNote = "ambiguity_ratio (prompts without obvious keywords)",
            ShouldScaleUp     = val.Count < 30,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"examples={examples.Count}, val={val.Count}, " +
                    $"baseAcc={baselineAcc:P1}, enhAcc={enhancedAcc:P1}, " +
                    $"per_domain=[{perDomain}]"
        });
    }

    // Map English prompt words directly to predicate IDs.
    // SemanticNormalizer+FamilyCollapser is designed for C# code tokens, not English
    // sentences, so short prompts produce mostly empty results. Direct keyword detection
    // gives K-NN a meaningful predicate set to distinguish domains.
    private static int[] TextToPredicates(string text)
    {
        var lower = text.ToLowerInvariant();
        var preds = new HashSet<int>();

        if (new[] { "null", "none", "missing" }.Any(lower.Contains))
            preds.Add(LogicExtractor.NULL_GUARD);
        if (new[] { "fail", "error", "exception", "fix", "wrong", "broken", "why", "debug" }.Any(lower.Contains))
            preds.Add(LogicExtractor.FAIL);
        if (new[] { "add", "create", "implement", "write", "remove", "refactor", "update", "change" }.Any(lower.Contains))
            preds.Add(LogicExtractor.CREATE_OP);
        if (new[] { "get", "find", "show", "list", "what", "explain", "how", "where", "which" }.Any(lower.Contains))
            preds.Add(LogicExtractor.GET_CALL);
        if (new[] { "valid", "check", "verify", "test" }.Any(lower.Contains))
            preds.Add(LogicExtractor.VALIDATE_OP);
        if (new[] { "flow", "step", "phase", "plan", "sequence", "workflow", "orchestr" }.Any(lower.Contains))
            preds.Add(LogicExtractor.ITERATE);
        if (new[] { "design", "architect", "pattern", "structure", "approach", "should" }.Any(lower.Contains))
            preds.Add(LogicExtractor.ASSIGN);

        return preds.ToArray();
    }

    // A prompt is "ambiguous" if it lacks any of the obvious domain keywords
    private static bool IsAmbiguous(string p) =>
        !new[] { "fix", "add", "what", "flow", "design" }
            .Any(kw => p.ToLowerInvariant().Contains(kw));

    private static List<(string Prompt, int Domain)> GenerateExamples(int seed, int count)
    {
        var rng = new Random(seed);
        var templates = new (string Template, int Domain)[]
        {
            // BUG_FIX
            ("Fix NullReferenceException in {svc}", DomainClassifier.BUG_FIX),
            ("Why does {method} return null when {obj} exists", DomainClassifier.BUG_FIX),
            ("Debug the {method} logic — it fails for {obj}", DomainClassifier.BUG_FIX),
            ("Error: {obj} is null after calling {method}", DomainClassifier.BUG_FIX),
            ("The {svc} throws exception when {obj} is missing", DomainClassifier.BUG_FIX),
            // CODE_CHANGE
            ("Add null check to {method} before accessing {prop}", DomainClassifier.CODE_CHANGE),
            ("Refactor {svc} to use the new {method}", DomainClassifier.CODE_CHANGE),
            ("Update {method} to return OperationResult", DomainClassifier.CODE_CHANGE),
            ("Create {method} in {svc} that validates {obj}", DomainClassifier.CODE_CHANGE),
            ("Remove the duplicate {method} from {svc}", DomainClassifier.CODE_CHANGE),
            // CODE_QUESTION
            ("What does {method} return when {obj} is null", DomainClassifier.CODE_QUESTION),
            ("How does {svc} handle {obj} validation", DomainClassifier.CODE_QUESTION),
            ("Explain the {method} logic in {svc}", DomainClassifier.CODE_QUESTION),
            ("Find all methods that call {method}", DomainClassifier.CODE_QUESTION),
            ("Show the flow of {obj} through {svc}", DomainClassifier.CODE_QUESTION),
            // ORCHESTRATION
            ("Plan phase A of flow-01 for {svc}", DomainClassifier.ORCHESTRATION),
            ("Create the step sequence for {method} workflow", DomainClassifier.ORCHESTRATION),
            ("Design the flow for {obj} processing pipeline", DomainClassifier.ORCHESTRATION),
            ("Outline the phases for implementing {svc}", DomainClassifier.ORCHESTRATION),
            ("What steps are needed to deploy {svc}", DomainClassifier.ORCHESTRATION),
            // ARCHITECTURE
            ("Design the architecture for {svc}", DomainClassifier.ARCHITECTURE),
            ("What is the best pattern for {obj} validation", DomainClassifier.ARCHITECTURE),
            ("How should {svc} be structured for multi-tenant", DomainClassifier.ARCHITECTURE),
            ("Architect the {method} module for scalability", DomainClassifier.ARCHITECTURE),
            ("What approach should {svc} use for {obj}", DomainClassifier.ARCHITECTURE),
        };

        var svcs    = new[] { "UserService", "AuthService", "TokenService", "PaymentService", "OrderService" };
        var methods = new[] { "GetUser", "ValidateToken", "ProcessPayment", "BuildSearchFilter", "CreateSession" };
        var objs    = new[] { "user", "token", "payment", "order", "session", "tenant", "config" };
        var props   = new[] { "Id", "Name", "Email", "Status", "CreatedAt", "TenantId" };

        var result = new List<(string, int)>();
        while (result.Count < count)
        {
            var (tmpl, domain) = templates[rng.Next(templates.Length)];
            string prompt = tmpl
                .Replace("{svc}",    svcs[rng.Next(svcs.Length)])
                .Replace("{method}", methods[rng.Next(methods.Length)])
                .Replace("{obj}",    objs[rng.Next(objs.Length)])
                .Replace("{prop}",   props[rng.Next(props.Length)]);
            result.Add((prompt, domain));
        }
        return result;
    }

    private static long   Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
