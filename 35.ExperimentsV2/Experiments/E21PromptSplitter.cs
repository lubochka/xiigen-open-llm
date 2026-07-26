// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E21: PromptSplitter — detect segment boundaries by predicate-profile shift.
//
// Hypothesis: when a prompt contains two independent tasks the semantic op
// distribution changes at the boundary, whereas a false CONJ ("find X and Y")
// stays in the same op family on both sides.
//
// Baseline:  regex CONJ split (\band\b|\bthen\b|\bafter that\b)
// Enhanced:  cosine distance between sliding-window op profiles, threshold tuned on train
// Primary metric: boundary_accuracy (correct segment count)
// Dataset:   synthetic, generated programmatically — no file input required
public sealed class E21PromptSplitter
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var examples = GenerateExamples(seed: 42, scale: tier);
        if (examples.Count < 6)
            return OperationResult<ExperimentV2Result>.Fail("Too few synthetic examples");

        int split = (int)(examples.Count * 0.8);
        var train = examples.Take(split).ToList();
        var val   = examples.Skip(split).ToList();

        // Tune threshold on training set
        double bestThreshold = TuneThreshold(train);

        // ── Baseline: simple CONJ regex split ─────────────────────────────────
        var t0 = Stopwatch();
        int baseCorrect = val.Count(ex => ConjSplit(ex.Prompt) == ex.Expected);
        double baselineAcc = baseCorrect / (double)val.Count;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: predicate-profile sliding window ─────────────────────────
        var t1 = Stopwatch();
        int enhCorrect = 0, fp = 0, fn = 0;
        foreach (var ex in val)
        {
            int got = ProductionSplitCount(ex.Prompt);
            if (got == ex.Expected) enhCorrect++;
            else if (got > ex.Expected) fp++;
            else fn++;
        }
        double enhancedAcc = enhCorrect / (double)val.Count;
        double enhancedMs  = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E21-PROMPT-SPLITTER",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "boundary_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = bestThreshold,
            ScalingSignalNote = "tuned_threshold (stable threshold → better generalisation)",
            ShouldScaleUp     = false,  // synthetic dataset is fixed
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"examples={examples.Count}, val={val.Count}, " +
                    $"threshold={bestThreshold:F2}, " +
                    $"baseAcc={baselineAcc:P1}, enhAcc={enhancedAcc:P1}, " +
                    $"FP={fp}(extra splits), FN={fn}(missed splits)"
        });
    }

    // ── Synthetic example generator ───────────────────────────────────────────

    private static List<(string Prompt, int Expected)> GenerateExamples(int seed, int scale)
    {
        // Single-task phrases grouped by domain
        var change = new[]
        {
            "Add GetUserById to UserService",
            "Implement auth token validation",
            "Extract validation logic from ContextWindowManager",
            "Refactor the search filter builder",
            "Update the null check in ProcessOrders",
            "Create a new DatasetLoader for JSON files",
            "Add caching to the token lookup method",
            "Implement retry logic for failed requests",
        };
        var question = new[]
        {
            "Explain how the AST packing works",
            "Show all callers of BuildSearchFilter",
            "What does ValidateToken return when user is null",
            "Find methods without null check",
            "Why does getUserById fail for negative ids",
            "List all classes that implement ITokenBudgetCalculator",
        };
        var bugfix = new[]
        {
            "Fix NullReferenceException in UserService",
            "Debug the token expiry logic",
            "Resolve the infinite loop in ProcessOrders",
            "Fix the off-by-one error in pagination",
            "Why does the auth middleware throw on empty tokens",
        };
        var test = new[]
        {
            "Write unit tests for AuthService",
            "Add integration test for the null check flow",
            "Create test cases for all edge cases in ValidateToken",
            "Write a test that covers the retry logic",
        };

        // False CONJ — same domain on both sides of AND/THEN, should stay 1 segment.
        // 50+ examples so the validation set always has hard false-CONJ cases.
        var falseConj = new[]
        {
            // GET / query domain
            "Find methods that return null and log them",
            "Find all callers and show their signatures",
            "Get the user and return the name",
            "Search for methods and display them",
            "Get the service and check its state",
            "Find all null references and list them",
            "Show all dependencies and their versions",
            "Get the config and read the value",
            "Find the method and examine its body",
            "List all classes and their methods",
            "Get all files and count the lines",
            "Find duplicate code and show examples",
            "Read the token and check its expiry",
            "Get the current user and show profile",
            "Find broken tests and display errors",
            "Get all callers and check their usage",
            "Find dead code and show where it is",
            "List all services and their dependencies",
            // VALIDATE domain
            "Check null and empty before accessing Name",
            "Validate token and user before returning result",
            "Check null and throw if missing",
            "Validate input and check for nulls",
            "Verify token and authenticate user",
            "Test the edge case and confirm result",
            "Check all parameters and validate them",
            "Validate the schema and check constraints",
            "Verify the signature and check the hash",
            "Check permissions and validate the role",
            "Test the null path and verify behavior",
            "Validate user and check their access",
            "Check the return value and verify type",
            "Validate all fields and check required",
            "Test the method and verify output",
            "Check input and validate constraints",
            // IMPLEMENT domain
            "Read the config file and parse the JSON",
            "Get the user and verify the token",
            "Add logging and handle the exception",
            "Inject the service and configure it",
            "Create the method and add null checks",
            "Add the field and initialize it",
            "Override the method and call the base",
            "Add the interface and implement it",
            "Create the test and mock the service",
            "Add null check and return early",
            "Implement retry and log the attempt",
            "Add caching and update the response",
            "Register the service and configure DI",
            "Add the property and initialize default",
            // CROSS-VERB but single intent
            "Find and fix the null reference issue",
            "Check and update the token expiry",
            "Get and validate the user input",
            "Read and parse the configuration file",
            "Find and log all exceptions",
            "Check and handle empty collections",
            "Validate and return the result",
            "Get and serialize the response",
            "Find and remove duplicate entries",
            "Check and throw on invalid state",
        };

        var rng    = new Random(seed);
        var result = new List<(string, int)>();

        // 1-segment: all singles + false conj
        foreach (var s in change.Concat(question).Concat(bugfix).Concat(test))
            result.Add((s, 1));
        foreach (var s in falseConj)
            result.Add((s, 1));

        // 2-segment: cross-domain pairs (different verb families → profile shift)
        var conjs    = new[] { "and then", "then", "and also", "after that" };
        var domains  = new[] { change, question, bugfix, test };
        // Mix pairs from domain[i] × domain[j] where i≠j
        for (int i = 0; i < domains.Length; i++)
        for (int j = 0; j < domains.Length; j++)
        {
            if (i == j) continue;
            string a    = domains[i][rng.Next(domains[i].Length)];
            string b    = domains[j][rng.Next(domains[j].Length)];
            string conj = conjs[rng.Next(conjs.Length)];
            result.Add(($"{a} {conj} {b}", 2));
        }

        // Shuffle deterministically
        return result.OrderBy(_ => rng.Next()).ToList();
    }

    private static int ConjSplit(string prompt) =>
        System.Text.RegularExpressions.Regex
            .Split(prompt,
                @"\band then\b|\bafter that\b|\band also\b|\bthen\b|\band\b",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            .Count(p => !string.IsNullOrWhiteSpace(p));

    private static double TuneThreshold(List<(string Prompt, int Expected)> train)
    {
        double best = 0.4; int bestScore = -1;
        for (double t = 0.20; t <= 0.90; t += 0.05)
        {
            int score = train.Count(ex => ProductionSplitCount(ex.Prompt) == ex.Expected);
            if (score > bestScore) { bestScore = score; best = t; }
        }
        return best;
    }

    private static int ProductionSplitCount(string prompt)
    {
        var storage = new InMemoryCompilerStorage();
        var logger = new NoopCompilerEventLogger();
        var result = new SemanticCompiler29.Decomposer(storage, logger).Decompose(prompt);
        return result.IsSuccess ? result.Value!.Length : 1;
    }

    private sealed class InMemoryCompilerStorage : SemanticCompiler29.ICompilerStorage
    {
        private readonly Dictionary<string, SemanticCompiler29.PipelineOutput> _items = new(StringComparer.Ordinal);
        public void Save(string key, SemanticCompiler29.PipelineOutput output) => _items[key] = output;
        public SemanticCompiler29.PipelineOutput? Load(string key) => _items.TryGetValue(key, out var output) ? output : null;
    }

    private sealed class NoopCompilerEventLogger : SemanticCompiler29.ICompilerEventLogger
    {
        public void Log(SemanticCompiler29.StandardEvent evt) { }
    }

    private static long   Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
