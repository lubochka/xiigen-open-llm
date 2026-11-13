// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

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
    // Use the same project-23 compiled prompt representation that production teaching uses.
    private static int[] TextToPredicates(string text)
    {
        var compiled = new SemanticCompilerAdapter.SemanticTeachingPromptCompiler().Compile(text);
        return compiled.IsSuccess
            ? compiled.Value!.Compilation.DevelopmentPredicateIds
                .Concat(compiled.Value.Compilation.PipelinePredicates)
                .SelectMany(CompiledPredicateToExperimentPredicate)
                .Concat(CompiledDomainToExperimentPredicate(compiled.Value.Compilation.Domain))
                .Distinct()
                .ToArray()
            : [];
    }

    private static IEnumerable<int> CompiledPredicateToExperimentPredicate(string predicate)
    {
        switch (predicate)
        {
            case "FAIL":
            case "QA":
                yield return LogicExtractor.FAIL;
                break;
            case "SECURITY":
            case "AUTHORIZATION":
            case "ARCHITECTURE":
            case "API_CONTRACT":
                yield return LogicExtractor.ASSIGN;
                break;
            case "IMPLEMENT":
            case "CREATE_OP":
                yield return LogicExtractor.CREATE_OP;
                break;
            case "TEST":
            case "VALIDATE_OP":
                yield return LogicExtractor.VALIDATE_OP;
                break;
            case "LEARNING":
                yield return LogicExtractor.ITERATE;
                break;
            case "GET_OP":
                yield return LogicExtractor.GET_CALL;
                break;
            case "SEND_OP":
                yield return LogicExtractor.CALL;
                break;
        }
    }

    private static IEnumerable<int> CompiledDomainToExperimentPredicate(SemanticCompiler29.Domain domain)
    {
        switch (domain)
        {
            case SemanticCompiler29.Domain.BUG:
                yield return LogicExtractor.FAIL;
                yield return LogicExtractor.NULL_GUARD;
                break;
            case SemanticCompiler29.Domain.PLAN:
                yield return LogicExtractor.ITERATE;
                break;
            case SemanticCompiler29.Domain.ARCH:
                yield return LogicExtractor.ASSIGN;
                yield return LogicExtractor.DECLARE;
                break;
            case SemanticCompiler29.Domain.REFACTOR:
                yield return LogicExtractor.CALL;
                break;
            case SemanticCompiler29.Domain.CODE_Q:
                yield return LogicExtractor.GET_CALL;
                yield return LogicExtractor.RETURN;
                break;
        }
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
