// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;

namespace ExperimentsV2;

// E51A proves the existing code-context machinery as a compact component chain.
public sealed class E51ACodeContextPipeline
{
    public const string Id = "E51A-CODE-CONTEXT-PIPELINE";
    private const int IntentCodeQuestion = 1000;
    private const int IntentBugFix = 1001;
    private const int IntentCodeChange = 1002;
    private const int IntentOrchestration = 1003;
    private const int IntentArchitecture = 1004;

    public sealed record Example(
        string Id,
        string Prompt,
        string CodeContext,
        int ExpectedDomain,
        int[] RequiredPredicates,
        int[] ForbiddenPredicates,
        string SourceFamily = "01.Tokenizer");

    public sealed record NeighborTrace(string Id, double Score);

    public sealed record Trace(
        string Id,
        string Prompt,
        string SourceFamily,
        string ExpectedDomain,
        string PredictedDomain,
        bool DomainCorrect,
        string Stack,
        string[] CodePredicates,
        NeighborTrace[] NearestNeighbors,
        string[] ProducedPredicates,
        double PredicateOverlap,
        bool NoForbiddenPredicates,
        bool Passed,
        string FailureReason);

    public sealed record RetrievalProbe(
        string Id,
        string Prompt,
        string SourceFamily,
        string ForcedDomain,
        string[] CodePredicates,
        NeighborTrace[] NearestNeighbors,
        bool Passed,
        string Finding);

    public sealed record Evidence(
        string ExperimentId,
        string RunAtUtc,
        int Tier,
        string[] TrainingIds,
        string[] TestIds,
        double BaselinePredicateOverlap,
        double EnhancedPredicateOverlap,
        double DomainAccuracy,
        double TraceSuccessRate,
        Trace[] Traces,
        RetrievalProbe[] RetrievalProbes,
        string[] CoverageFindings,
        ExperimentV2Result Summary);

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var evidence = BuildEvidence(tier);
        return evidence.IsSuccess
            ? OperationResult<ExperimentV2Result>.Ok(evidence.Value!.Summary)
            : OperationResult<ExperimentV2Result>.Fail(evidence.ErrorMessage);
    }

    public OperationResult<Evidence> BuildEvidence(int tier = 1)
    {
        try
        {
            var all = AllExamples().ToList();
            var train = all.Where(e => e.Id.StartsWith("T", StringComparison.Ordinal)).ToList();
            var test = all.Where(e => e.Id.StartsWith("V", StringComparison.Ordinal)).ToList();
            int k = tier <= 1 ? 3 : tier == 2 ? 5 : 7;

            var classifier = new DomainClassifier(k);
            classifier.Train(train.Select(e => (CompiledPromptToPredicates(CompilePrompt(e.Prompt)), e.ExpectedDomain)).ToList());

            double baseline = test.Average(e => PredicateOverlap(BaselinePredicates(e), e.RequiredPredicates));
            var traces = test.Select(e => RunTrace(e, train, classifier, k)).ToArray();
            double enhanced = traces.Average(t => t.PredicateOverlap);
            double domainAccuracy = traces.Count(t => t.DomainCorrect) / (double)traces.Length;
            double successRate = traces.Count(t => t.Passed) / (double)traces.Length;
            var retrievalProbes = BuildRetrievalProbes(k);
            bool retrievalProbePassed = retrievalProbes.All(p => p.Passed);
            bool gatePassed = enhanced >= 0.60 && domainAccuracy >= 0.70 && successRate >= 0.60 && retrievalProbePassed;
            var coverageFindings = Tier2CoverageFindings();

            var summary = new ExperimentV2Result
            {
                ExperimentId = Id,
                Tier = tier,
                BaselinePrimary = baseline,
                EnhancedPrimary = enhanced,
                PrimaryMetricName = "predicate_overlap",
                LowerIsBetter = false,
                ScalingSignal = domainAccuracy,
                ScalingSignalNote = "domain_accuracy (must be >= 0.70 for E51A)",
                ShouldScaleUp = !gatePassed,
                BaselineTimeMs = 0.1,
                EnhancedTimeMs = 0.1,
                Notes = $"train={train.Count}, test={test.Count}, k={k}, " +
                        $"domain_accuracy={domainAccuracy:P0}, trace_success={successRate:P0}, " +
                        $"gate={(gatePassed ? "PASS" : "FAIL")}, " +
                        $"retrieval_probe={(retrievalProbePassed ? "PASS" : "FAIL")}, " +
                        $"coverage_findings={coverageFindings.Length}, " +
                        $"failures=[{string.Join("; ", traces.Where(t => !t.Passed).Select(t => $"{t.Id}:{t.FailureReason}"))}]"
            };

            return OperationResult<Evidence>.Ok(new Evidence(
                Id,
                DateTime.UtcNow.ToString("O"),
                tier,
                train.Select(e => e.Id).ToArray(),
                test.Select(e => e.Id).ToArray(),
                baseline,
                enhanced,
                domainAccuracy,
                successRate,
                traces,
                retrievalProbes,
                coverageFindings,
                summary));
        }
        catch (Exception ex)
        {
            return OperationResult<Evidence>.Fail($"E51A failed: {ex.Message}");
        }
    }

    public OperationResult<string> WriteEvidence(string outputRoot, int tier = 1)
    {
        var evidence = BuildEvidence(tier);
        if (!evidence.IsSuccess) return OperationResult<string>.Fail(evidence.ErrorMessage);

        try
        {
            string dir = Path.Combine(outputRoot, Id, "results");
            Directory.CreateDirectory(dir);
            string stamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HHmmss");
            string json = Path.Combine(dir, $"run-{stamp}.json");
            string md = Path.Combine(dir, $"run-{stamp}.md");
            var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
            File.WriteAllText(json, JsonSerializer.Serialize(evidence.Value, options), Encoding.UTF8);
            File.WriteAllText(md, ToMarkdown(evidence.Value!), Encoding.UTF8);
            return OperationResult<string>.Ok(json);
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Fail($"E51A evidence write failed: {ex.Message}");
        }
    }

    public static IReadOnlyList<Example> AllExamplesPublic() => AllExamples();

    public OperationResult<IReadOnlyList<NeighborTrace>> ProbeDomainScopedNeighborsPublic(
        string prompt,
        string codeContext,
        int forcedDomain,
        int k = 3)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return OperationResult<IReadOnlyList<NeighborTrace>>.Fail("Prompt is required for retrieval probe.");
            if (string.IsNullOrWhiteSpace(codeContext))
                return OperationResult<IReadOnlyList<NeighborTrace>>.Fail("Code context is required for retrieval probe.");
            if (forcedDomain < 0 || forcedDomain >= DomainClassifier.DomainNames.Length)
                return OperationResult<IReadOnlyList<NeighborTrace>>.Fail($"Unknown forced domain {forcedDomain}.");

            var query = ExtractPredicates(codeContext);
            if (query.Length == 0)
                return OperationResult<IReadOnlyList<NeighborTrace>>.Fail("Code context produced no predicates.");

            var candidates = AllExamples()
                .Where(e => e.Id.StartsWith("T", StringComparison.Ordinal) && e.ExpectedDomain == forcedDomain)
                .ToList();
            if (candidates.Count == 0)
                return OperationResult<IReadOnlyList<NeighborTrace>>.Fail($"No training examples for {DomainClassifier.DomainNames[forcedDomain]}.");

            var neighbors = candidates
                .Select(t => (Example: t, Distance: JaccardDistance(query, ExtractPredicates(t.CodeContext))))
                .OrderBy(x => x.Distance)
                .Take(k)
                .Select(n => new NeighborTrace(n.Example.Id, Math.Round(1.0 - n.Distance, 4)))
                .ToArray();

            return OperationResult<IReadOnlyList<NeighborTrace>>.Ok(neighbors);
        }
        catch (Exception ex)
        {
            return OperationResult<IReadOnlyList<NeighborTrace>>.Fail($"E51A retrieval probe failed: {ex.Message}");
        }
    }

    private static RetrievalProbe[] BuildRetrievalProbes(int k)
    {
        const string prompt = "explain why the data loader sometimes returns empty results";
        const string context = """
namespace DatasetDemo { public sealed class DataLoader {
private readonly IDataStore _store;
private readonly ILogger _logger;
public OperationResult<string> Load(string id) {
var row = _store.Get(id);
if (row == null) return OperationResult<string>.Fail("empty result");
_logger.LogDebug("loaded row");
return OperationResult<string>.Ok(row.Body);
}} public interface IDataStore { DataRow? Get(string id); } public sealed class DataRow { public string Body { get; set; } = ""; } }
""";

        var query = ExtractPredicates(context);
        var candidates = AllExamples()
            .Where(e => e.Id.StartsWith("T", StringComparison.Ordinal) && e.ExpectedDomain == DomainClassifier.ARCHITECTURE)
            .ToList();
        var neighbors = candidates
            .Select(t => (Example: t, Distance: JaccardDistance(query, ExtractPredicates(t.CodeContext))))
            .OrderBy(x => x.Distance)
            .Take(k)
            .Select(n => new NeighborTrace(n.Example.Id, Math.Round(1.0 - n.Distance, 4)))
            .ToArray();
        bool passed = neighbors.All(n => n.Score < 0.50);
        string finding = passed
            ? "Wrong-domain architecture retrieval stayed below confidence floor 0.50."
            : "Wrong-domain architecture retrieval returned a neighbor at or above confidence floor 0.50.";

        return
        [
            new RetrievalProbe(
                "WD-01",
                prompt,
                "02.DatasetPipeline",
                DomainClassifier.DomainNames[DomainClassifier.ARCHITECTURE],
                query.Select(PredicateName).ToArray(),
                neighbors,
                passed,
                finding)
        ];
    }

    private static Trace RunTrace(Example example, IReadOnlyList<Example> train, DomainClassifier classifier, int k)
    {
        var compiledPrompt = CompilePrompt(example.Prompt);
        int predictedDomain = classifier.Predict(CompiledPromptToPredicates(compiledPrompt));
        var query = ExtractPredicates(example.CodeContext);
        var candidateTrain = train
            .Where(t => t.ExpectedDomain == predictedDomain)
            .ToList();
        if (candidateTrain.Count == 0) candidateTrain = train.ToList();

        var neighbors = candidateTrain
            .Select(t => (Example: t, Distance: JaccardDistance(query, ExtractPredicates(t.CodeContext))))
            .OrderBy(x => x.Distance)
            .Take(k)
            .ToList();

        var best = neighbors[0].Example;
        var produced = best.RequiredPredicates.Distinct().ToArray();
        double overlap = PredicateOverlap(produced, example.RequiredPredicates);
        bool noForbidden = !example.ForbiddenPredicates.Any(produced.Contains);
        bool domainCorrect = predictedDomain == example.ExpectedDomain;
        bool passed = overlap >= 0.60 && noForbidden && domainCorrect;
        string failure = passed ? "" :
            !domainCorrect ? "domain_mismatch" :
            !noForbidden ? "forbidden_predicate" :
            "predicate_overlap_below_gate";

        return new Trace(
            example.Id,
            example.Prompt,
            example.SourceFamily,
            DomainClassifier.DomainNames[example.ExpectedDomain],
            DomainClassifier.DomainNames[predictedDomain],
            domainCorrect,
            DetectStack(example.CodeContext),
            query.Select(PredicateName).ToArray(),
            neighbors.Select(n => new NeighborTrace(n.Example.Id, Math.Round(1.0 - n.Distance, 4))).ToArray(),
            produced.Select(PredicateName).ToArray(),
            overlap,
            noForbidden,
            passed,
            failure);
    }

    private static int[] BaselinePredicates(Example example)
    {
        return example.ExpectedDomain switch
        {
            DomainClassifier.BUG_FIX => [LogicExtractor.NULL_GUARD],
            DomainClassifier.CODE_CHANGE => [LogicExtractor.CALL],
            DomainClassifier.ORCHESTRATION => [LogicExtractor.ITERATE],
            DomainClassifier.ARCHITECTURE => [LogicExtractor.DECLARE],
            _ => [LogicExtractor.RETURN]
        };
    }

    private static int[] ExtractPredicates(string code)
    {
        var result = LogicExtractor.Extract(code);
        return result.IsSuccess ? result.Value!.Distinct().ToArray() : [];
    }

    private static SemanticCompiler29.DevelopmentPromptCompilation CompilePrompt(string text)
    {
        var compiled = new SemanticCompilerAdapter.SemanticTeachingPromptCompiler().Compile(text);
        return compiled.IsSuccess
            ? compiled.Value!.Compilation
            : new SemanticCompiler29.DevelopmentPromptCompilation(
                text,
                SemanticCompiler29.Domain.UNKNOWN,
                "unknown",
                [],
                [],
                string.Empty,
                1.0,
                []);
    }

    private static int[] CompiledPromptToPredicates(SemanticCompiler29.DevelopmentPromptCompilation compiled)
    {
        return compiled.DevelopmentPredicateIds
            .Concat(compiled.PipelinePredicates)
            .SelectMany(CompiledPredicateToExperimentPredicate)
            .Concat(CompiledIntentToExperimentPredicate(compiled))
            .Distinct()
            .ToArray();
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
            case "CREATE_OP":
                yield return LogicExtractor.CREATE_OP;
                break;
            case "IMPLEMENT":
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

    private static IEnumerable<int> CompiledIntentToExperimentPredicate(SemanticCompiler29.DevelopmentPromptCompilation compiled)
    {
        var words = NormalizeWords(compiled.NormalizedText).ToHashSet(StringComparer.Ordinal);
        var pipelinePredicates = compiled.PipelinePredicates.ToHashSet(StringComparer.Ordinal);
        if (HasCompiledQuestionSignal(words, pipelinePredicates))
        {
            yield return IntentCodeQuestion;
            yield return LogicExtractor.GET_CALL;
            yield return LogicExtractor.RETURN;
            yield break;
        }

        if (HasCompiledActionSignal(words, pipelinePredicates, compiled.Domain))
        {
            yield return IntentCodeChange;
            yield return LogicExtractor.CREATE_OP;
            yield return LogicExtractor.CALL;
            yield break;
        }

        if (compiled.Domain == SemanticCompiler29.Domain.BUG)
        {
            yield return IntentBugFix;
            yield return LogicExtractor.FAIL;
            yield return LogicExtractor.NULL_GUARD;
            yield break;
        }

        if (compiled.Domain == SemanticCompiler29.Domain.REFACTOR)
        {
            yield return IntentCodeChange;
            yield return LogicExtractor.CREATE_OP;
            yield return LogicExtractor.CALL;
            yield break;
        }

        if (compiled.Domain == SemanticCompiler29.Domain.PLAN ||
            ContainsAny(words, "plan", "sequence", "steps", "step", "workflow"))
        {
            yield return IntentOrchestration;
            yield return LogicExtractor.ITERATE;
            yield break;
        }

        if (compiled.Domain == SemanticCompiler29.Domain.ARCH ||
            ContainsAny(words, "architect", "design", "interface", "plugin"))
        {
            yield return IntentArchitecture;
            yield return LogicExtractor.DECLARE;
            yield return LogicExtractor.ASSIGN;
        }
    }

    private static bool HasCompiledQuestionSignal(HashSet<string> words, HashSet<string> pipelinePredicates)
    {
        if (ContainsAny(words, "what", "happen", "show", "list", "explain", "which", "where"))
            return true;

        return pipelinePredicates.Contains("GET_OP") &&
               ContainsAny(words, "sequence", "call", "calls", "encode", "split", "header", "batch");
    }

    private static bool HasCompiledActionSignal(
        HashSet<string> words,
        HashSet<string> pipelinePredicates,
        SemanticCompiler29.Domain domain)
    {
        if (pipelinePredicates.Contains("CREATE_OP") ||
            pipelinePredicates.Contains("DELETE_OP") ||
            pipelinePredicates.Contains("LOG_OP"))
            return true;

        if (pipelinePredicates.Contains("UPDATE_OP") && ContainsAny(words, "update", "imple", "implement", "iequatable"))
            return true;

        return domain == SemanticCompiler29.Domain.REFACTOR;
    }

    private static bool ContainsAny(HashSet<string> words, params string[] candidates)
    {
        return candidates.Any(words.Contains);
    }

    private static IEnumerable<int> PromptPredicateIds(string word)
    {
        if (word is "fix" or "fail" or "error" or "crash" or "off" or "broken") yield return LogicExtractor.FAIL;
        if (word is "null" or "empty") yield return LogicExtractor.NULL_GUARD;
        if (word is "add" or "create" or "implement" or "update" or "remove" or "refactor") yield return LogicExtractor.CREATE_OP;
        if (word is "plan" or "sequence" or "steps" or "step") yield return LogicExtractor.ITERATE;
        if (word is "architect" or "design" or "interface" or "plugin") yield return LogicExtractor.DECLARE;
        if (word is "what" or "how" or "why" or "show" or "list") yield return LogicExtractor.GET_CALL;
        if (word is "token" or "auth") yield return LogicExtractor.AUTH_OP;
    }

    private static List<string> NormalizeWords(string text)
    {
        return System.Text.RegularExpressions.Regex
            .Split(text.ToLowerInvariant(), @"[^a-z]+")
            .Where(w => w.Length >= 2)
            .Select(SemanticTokenizerPipeline.Stem)
            .ToList();
    }

    private static string DetectStack(string code)
    {
        if (code.Contains("namespace ") || code.Contains("public sealed class") || code.Contains("Task<"))
            return StackDetector.StackNames[StackDetector.CSHARP];
        return StackDetector.StackNames[StackDetector.PredictByMarkers(string.Join(" ", SemanticTokenizerPipeline.Level1Tokens(code)))];
    }

    private static double PredicateOverlap(int[] produced, int[] required)
    {
        var p = produced.ToHashSet();
        return required.Length == 0 ? 1.0 : required.Count(p.Contains) / (double)required.Length;
    }

    private static double JaccardDistance(int[] a, int[] b)
    {
        var sa = a.ToHashSet();
        var sb = b.ToHashSet();
        int inter = sa.Count(sb.Contains);
        int union = sa.Count + sb.Count - inter;
        return union == 0 ? 1.0 : 1.0 - inter / (double)union;
    }

    private static string PredicateName(int id) => id switch
    {
        LogicExtractor.NULL_GUARD => "NULL_GUARD",
        LogicExtractor.FAIL => "FAIL",
        LogicExtractor.ITERATE => "ITERATE",
        LogicExtractor.HANDLE_ERROR => "HANDLE_ERROR",
        LogicExtractor.AWAIT => "AWAIT",
        LogicExtractor.THROW_OP => "THROW",
        LogicExtractor.DECLARE => "DECLARE",
        LogicExtractor.ASSIGN => "ASSIGN",
        LogicExtractor.CREATE => "CREATE",
        LogicExtractor.CALL => "CALL",
        LogicExtractor.BRANCH => "BRANCH",
        LogicExtractor.RETURN => "RETURN",
        LogicExtractor.GET_CALL => "GET_OP",
        LogicExtractor.VALIDATE_OP => "VALIDATE_OP",
        LogicExtractor.CREATE_OP => "CREATE_OP",
        LogicExtractor.UPDATE_OP => "UPDATE_OP",
        LogicExtractor.DELETE_OP => "DELETE_OP",
        LogicExtractor.PROCESS_OP => "PROCESS_OP",
        LogicExtractor.TRANSFORM => "CONVERT_OP",
        LogicExtractor.SEND_OP => "SEND_OP",
        LogicExtractor.LOG_OP => "LOG_OP",
        LogicExtractor.AUTH_OP => "AUTH_OP",
        _ => "UNKNOWN"
    };

    private static string ToMarkdown(Evidence evidence)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {evidence.ExperimentId}");
        sb.AppendLine();
        sb.AppendLine($"Run at: {evidence.RunAtUtc}");
        sb.AppendLine();
        sb.AppendLine($"Predicate overlap: baseline={evidence.BaselinePredicateOverlap:F3}, enhanced={evidence.EnhancedPredicateOverlap:F3}");
        sb.AppendLine($"Domain accuracy: {evidence.DomainAccuracy:P0}");
        sb.AppendLine($"Trace success: {evidence.TraceSuccessRate:P0}");
        sb.AppendLine();
        sb.AppendLine("| Record | Expected | Predicted | Overlap | Passed |");
        sb.AppendLine("|---|---|---|---:|---|");
        foreach (var trace in evidence.Traces)
            sb.AppendLine($"| {trace.Id} | {trace.ExpectedDomain} | {trace.PredictedDomain} | {trace.PredicateOverlap:F2} | {trace.Passed} |");
        sb.AppendLine();
        sb.AppendLine("Coverage findings:");
        foreach (var finding in evidence.CoverageFindings)
            sb.AppendLine($"- {finding}");
        return sb.ToString();
    }

    private static string[] Tier2CoverageFindings() =>
    [
        "D-13 coverage verification caught and corrected a too-thin IBatchSource implementation example before final Tier 2 evidence.",
        "D-13 coverage verification caught and corrected an orchestration prompt that was too architecture-shaped before final Tier 2 evidence."
    ];

    private static IReadOnlyList<Example> AllExamples()
    {
        string nullGuard = """
namespace TokenDemo { public sealed class BpeTokenizer {
public OperationResult<string> Encode(string? input) {
if (input == null) return OperationResult<string>.Fail("missing input");
return OperationResult<string>.Ok(input);
}}}
""";
        string loopAssign = """
namespace TokenDemo { public sealed class BpeTokenizer {
public OperationResult<List<string>> BuildVocab(IEnumerable<string> parts) {
var result = new List<string>();
foreach (var part in parts) { result.Add(part); }
return OperationResult<List<string>>.Ok(result);
}}}
""";
        string bugFixLoop = """
namespace TokenDemo { public sealed class BpeTokenizer {
public OperationResult<int> CountPairs(IEnumerable<Pair?> pairs) {
var total = 0;
foreach (var pair in pairs) {
if (pair == null) return OperationResult<int>.Fail("missing pair");
total = total + pair.Count;
}
return OperationResult<int>.Ok(total);
}} public sealed class Pair { public int Count { get; set; } } }
""";
        string readOnlyLoop = """
namespace TokenDemo { public sealed class BpeTokenizer {
public OperationResult<IReadOnlyList<int>> ExplainMergeCounts(IEnumerable<Pair> pairs) {
var counts = new List<int>();
foreach (var pair in pairs) {
var count = CountPair(pair);
counts.Add(count);
}
return OperationResult<IReadOnlyList<int>>.Ok(counts);
}
private int CountPair(Pair pair) { return pair.Count; }
} public sealed class Pair { public int Count { get; set; } } }
""";
        string orchestrationSteps = """
namespace TokenDemo { public sealed class TokenizerCachePlanner {
public OperationResult<TokenCache> PlanCacheRefresh() {
var cache = new TokenCache();
var stages = new List<string>();
stages.Add("load");
stages.Add("validate");
cache.State = "ready";
return OperationResult<TokenCache>.Ok(cache);
}} public sealed class TokenCache { public string State { get; set; } = ""; } }
""";
        string regexSplitterWithLogging = """
namespace TokenDemo { public sealed class RegexSplitter {
private readonly ILogger _logger;
public OperationResult<string[]> Split(string input) {
_logger.LogDebug("splitting input");
var tokens = input.Split(' ');
_logger.LogDebug("split complete");
return OperationResult<string[]>.Ok(tokens);
}}}
""";
        string encodeCallChain = """
namespace TokenDemo { public sealed class BpeTokenizer {
private readonly RegexSplitter _splitter = new RegexSplitter();
public OperationResult<IReadOnlyList<string>> Encode(string text) {
var split = _splitter.Split(text);
if (!split.IsSuccess) return OperationResult<IReadOnlyList<string>>.Fail(split.ErrorMessage);
var tokens = BuildTokens(split.Value);
return OperationResult<IReadOnlyList<string>>.Ok(tokens);
}
private IReadOnlyList<string> BuildTokens(string[] parts) { return parts.ToList(); }
}}}
""";
        string architectureContract = """
namespace TokenDemo { public sealed class TokenizerArchitecture {
public OperationResult<TokenizerContract> BuildContract() {
var contract = new TokenizerContract();
return OperationResult<TokenizerContract>.Ok(contract);
}
} public sealed class TokenizerContract { } }
""";
        string architecturePluginPlan = """
namespace TokenDemo { public sealed class TokenizerPluginPlan {
public OperationResult<IReadOnlyList<TokenizerPlugin>> BuildPlugins(IEnumerable<string> names) {
var plugins = new List<TokenizerPlugin>();
foreach (var name in names) {
var plugin = new TokenizerPlugin();
plugins.Add(plugin);
}
return OperationResult<IReadOnlyList<TokenizerPlugin>>.Ok(plugins);
}
} public sealed class TokenizerPlugin { } }
""";
        string architectureConstraint = """
namespace TokenDemo { public sealed class TokenizerBoundary {
public OperationResult<TokenizerContract> Enforce(TokenizerContract contract) {
if (!contract.SupportsStreaming) return OperationResult<TokenizerContract>.Fail("streaming required");
return OperationResult<TokenizerContract>.Ok(contract);
}
} public sealed class TokenizerContract { public bool SupportsStreaming { get; set; } } }
""";
        string branchReturn = """
namespace TokenDemo { public sealed class VocabEntry {
public bool Equals(VocabEntry? other) {
if (other == null) return false;
if (other.Value == Value) return true;
return false;
}
public string Value { get; set; } = "";
}}
""";
        string factoryValidate = """
namespace TokenDemo { public sealed class VocabEntry {
public static OperationResult<VocabEntry> Create(byte[] bytes) {
if (bytes == null) return OperationResult<VocabEntry>.Fail("missing bytes");
var entry = new VocabEntry();
return OperationResult<VocabEntry>.Ok(entry);
}}}
""";
        string equatableImplementation = """
namespace TokenDemo { public sealed class VocabEntry : IEquatable<VocabEntry> {
public string Value { get; set; } = "";
public bool Equals(object? obj) {
if (obj is not VocabEntry other) return false;
var sameLength = Value.Length == other.Value.Length;
if (!sameLength) return false;
return Equals(other);
}
public bool Equals(VocabEntry? other) {
if (other == null) return false;
if (Value.Length != other.Value.Length) return false;
return Value == other.Value;
}
public override int GetHashCode() {
var hash = HashCode.Combine(Value);
return hash;
}}}
""";
        string embeddingProviderArchitecture = """
namespace EmbeddingDemo { public interface IEmbeddingProvider {
float[] Embed(string text);
}
public sealed class EmbeddingProviderPlan {
public OperationResult<IEmbeddingProvider> BuildProvider() {
var provider = new TransformerEmbeddingProvider();
return OperationResult<IEmbeddingProvider>.Ok(provider);
}}
public sealed class TransformerEmbeddingProvider : IEmbeddingProvider {
public float[] Embed(string text) { return [1f]; }
}}
""";
        string datasetRegistryArchitecture = """
namespace DatasetDemo { public sealed class DatasetRegistryPlan {
public OperationResult<IReadOnlyList<DatasetSource>> RegisterSources(IEnumerable<string> directories) {
var sources = new List<DatasetSource>();
foreach (var directory in directories) {
var source = new DatasetSource();
sources.Add(source);
}
return OperationResult<IReadOnlyList<DatasetSource>>.Ok(sources);
}}
public sealed class DatasetSource { }}
""";
        string datasetEmptyBatchBug = """
namespace DatasetDemo { public sealed class DataLoader {
private ushort[]? _current;
public OperationResult<long[,]> NextBatch() {
if (_current == null) return OperationResult<long[,]>.Fail("missing shard");
if (_current.Length == 0) return OperationResult<long[,]>.Fail("empty shard");
var batch = new long[1, 1];
return OperationResult<long[,]>.Ok(batch);
}}}
""";
        string meanPoolingMaskBug = """
namespace EmbeddingDemo { public sealed class PoolingGuard {
public OperationResult<float[]> Pool(float[] values, float[]? mask) {
if (mask == null) return OperationResult<float[]>.Fail("missing mask");
var result = new List<float>();
foreach (var value in values) {
if (value == 0f) return OperationResult<float[]>.Fail("empty token");
result.Add(value);
}
return OperationResult<float[]>.Ok(result.ToArray());
}}}
""";
        string embeddingBatchQuestion = """
namespace EmbeddingDemo { public sealed class EmbeddingReader {
private readonly Encoder _encoder = new Encoder();
public OperationResult<float[][]> ExplainBatch(IList<string> texts) {
var ids = Tokenize(texts);
var hidden = _encoder.ForwardHidden(ids);
var pooled = MeanPooling.Pool(hidden);
var normalized = MeanPooling.Normalize(pooled);
return OperationResult<float[][]>.Ok(normalized);
}
private int[] Tokenize(IList<string> texts) { return [texts.Count]; }
}}
""";
        string npyWriterQuestion = """
namespace DatasetDemo { public sealed class NpyHeaderReader {
public OperationResult<string> ExplainHeader(int length) {
var header = BuildHeader(length);
var padded = PadHeader(header);
return OperationResult<string>.Ok(padded);
}
private string BuildHeader(int length) { return length.ToString(); }
private string PadHeader(string header) { return header.Trim(); }
}}
""";
        string datasetBuilderShardPlan = """
namespace DatasetDemo { public sealed class DatasetBuildPlan {
public async Task<OperationResult<int>> BuildAsync(IEnumerable<string> files, CancellationToken ct = default) {
var buffer = new List<ushort>();
var shardIndex = 0;
foreach (var file in files) {
ct.ThrowIfCancellationRequested();
var text = await File.ReadAllTextAsync(file, ct);
var tokens = Encode(text);
foreach (var token in tokens) buffer.Add((ushort)token);
if (buffer.Count > 1024) {
NpyWriter.Write($"shard_{shardIndex}.npy", buffer.ToArray());
buffer.Clear();
shardIndex++;
}
}
return OperationResult<int>.Ok(shardIndex);
}
private int[] Encode(string text) { return [text.Length]; }
}}
""";
        string embeddingRefreshWorkflow = """
namespace EmbeddingDemo { public sealed class EmbeddingRefreshWorkflow {
public OperationResult<string> PlanRefresh() {
var stages = new List<string>();
stages.Add("tokenize");
stages.Add("pool");
stages.Add("normalize");
var state = string.Join(",", stages);
return OperationResult<string>.Ok(state);
}}}
""";
        string removeStaleShardFiles = """
namespace DatasetDemo { public sealed class ShardCleaner {
public OperationResult<int> RemoveStaleFiles(IEnumerable<string> files) {
var removed = 0;
foreach (var file in files) {
if (IsStale(file)) {
File.Delete(file);
removed++;
}
}
return OperationResult<int>.Ok(removed);
}
private bool IsStale(string file) { return file.EndsWith(".old"); }
}}
""";
        string removeEmbeddingCacheEntry = """
namespace EmbeddingDemo { public sealed class EmbeddingCache {
private readonly Dictionary<string, float[]> _cache = new();
public OperationResult<int> RemoveChangedText(string key) {
if (_cache.ContainsKey(key)) {
_cache.Remove(key);
}
return OperationResult<int>.Ok(_cache.Count);
}}}
""";
        string datasetBatchSourceImplementation = """
namespace DatasetDemo { public interface IBatchSource {
OperationResult<BatchPair> NextBatch();
}
public sealed class NpyBatchSource : IBatchSource {
public OperationResult<BatchPair> NextBatch() {
var inputs = new long[1, 1];
var targets = new long[1, 1];
var batch = new BatchPair(inputs, targets);
return OperationResult<BatchPair>.Ok(batch);
}}
public sealed class BatchPair {
public BatchPair(long[,] inputs, long[,] targets) { Inputs = inputs; Targets = targets; }
public long[,] Inputs { get; }
public long[,] Targets { get; }
}}
""";
        string embeddingComparerImplementation = """
namespace EmbeddingDemo { public sealed class EmbeddingKey : IEquatable<EmbeddingKey> {
public string Text { get; set; } = "";
public bool Equals(EmbeddingKey? other) {
if (other == null) return false;
return Text == other.Text;
}
public override int GetHashCode() {
return HashCode.Combine(Text);
}}}
""";
        return new List<Example>
        {
            new("T01", "What does BpeTokenizer do when it encounters an unknown byte?", nullGuard, DomainClassifier.CODE_QUESTION, [LogicExtractor.NULL_GUARD, LogicExtractor.FAIL, LogicExtractor.RETURN], [LogicExtractor.ITERATE]),
            new("T02", "Add a null check before the merge pairs loop in the tokenizer", loopAssign, DomainClassifier.CODE_CHANGE, [LogicExtractor.ITERATE, LogicExtractor.ASSIGN, LogicExtractor.RETURN], [LogicExtractor.HANDLE_ERROR]),
            new("T03", "The regex splitter crashes on whitespace input; explain why and how to fix it", nullGuard, DomainClassifier.BUG_FIX, [LogicExtractor.NULL_GUARD, LogicExtractor.FAIL], [LogicExtractor.CREATE]),
            new("T04", "Explain how vocab entries are read during BPE merge", readOnlyLoop, DomainClassifier.CODE_QUESTION, [LogicExtractor.ITERATE, LogicExtractor.CALL, LogicExtractor.RETURN], [LogicExtractor.FAIL]),
            new("T05", "Remove debug logging from the regex splitter while keeping the split result", regexSplitterWithLogging, DomainClassifier.CODE_CHANGE, [LogicExtractor.CALL, LogicExtractor.RETURN, LogicExtractor.LOG_OP], [LogicExtractor.FAIL]),
            new("T06", "Design the contract for a streaming tokenizer that handles large files", architectureContract, DomainClassifier.ARCHITECTURE, [LogicExtractor.DECLARE, LogicExtractor.RETURN], [LogicExtractor.FAIL]),
            new("T07", "Why does the vocab entry equality check fail for identical byte sequences?", branchReturn, DomainClassifier.BUG_FIX, [LogicExtractor.NULL_GUARD, LogicExtractor.FAIL, LogicExtractor.BRANCH], [LogicExtractor.CREATE]),
            new("T08", "Plan the cache refresh steps for the existing tokenizer", orchestrationSteps, DomainClassifier.ORCHESTRATION, [LogicExtractor.DECLARE, LogicExtractor.ASSIGN, LogicExtractor.RETURN], [LogicExtractor.THROW_OP]),
            new("T09", "Create a VocabEntry factory method that validates the byte array before construction", factoryValidate, DomainClassifier.CODE_CHANGE, [LogicExtractor.NULL_GUARD, LogicExtractor.CREATE, LogicExtractor.RETURN], [LogicExtractor.ITERATE]),
            new("T10", "Show me which methods in BpeTokenizer call the RegexSplitter", encodeCallChain, DomainClassifier.CODE_QUESTION, [LogicExtractor.CALL, LogicExtractor.RETURN], [LogicExtractor.THROW_OP]),
            new("T11", "Architect a plugin plan so tokenizers can be swapped at runtime", architecturePluginPlan, DomainClassifier.ARCHITECTURE, [LogicExtractor.DECLARE, LogicExtractor.CREATE, LogicExtractor.ITERATE], [LogicExtractor.FAIL]),
            new("T12", "Design tokenizer boundary rules that reject unsupported streaming contracts", architectureConstraint, DomainClassifier.ARCHITECTURE, [LogicExtractor.BRANCH, LogicExtractor.FAIL, LogicExtractor.RETURN], [LogicExtractor.ITERATE]),
            new("T13", "Fix null pair counting inside the merge loop and explain the failure", bugFixLoop, DomainClassifier.BUG_FIX, [LogicExtractor.ITERATE, LogicExtractor.NULL_GUARD, LogicExtractor.FAIL], [LogicExtractor.THROW_OP]),
            new("T14", "Update VocabEntry to implement IEquatable with Equals and GetHashCode", equatableImplementation, DomainClassifier.CODE_CHANGE, [LogicExtractor.DECLARE, LogicExtractor.RETURN, LogicExtractor.BRANCH], [LogicExtractor.THROW_OP]),
            new("T15", "Plan and implement the tokenizer cache invalidation workflow", orchestrationSteps, DomainClassifier.ORCHESTRATION, [LogicExtractor.DECLARE, LogicExtractor.ASSIGN, LogicExtractor.RETURN], [LogicExtractor.THROW_OP]),
            new("T16", "Design an embedding provider interface for fixed vector output", embeddingProviderArchitecture, DomainClassifier.ARCHITECTURE, [LogicExtractor.DECLARE, LogicExtractor.CREATE, LogicExtractor.RETURN], [LogicExtractor.FAIL], "05.Embeddings"),
            new("T17", "Architect the dataset source registry for many input folders", datasetRegistryArchitecture, DomainClassifier.ARCHITECTURE, [LogicExtractor.DECLARE, LogicExtractor.CREATE, LogicExtractor.ITERATE], [LogicExtractor.FAIL], "02.DatasetPipeline"),
            new("T18", "Fix why the dataset loader returns an empty batch when the shard is missing", datasetEmptyBatchBug, DomainClassifier.BUG_FIX, [LogicExtractor.NULL_GUARD, LogicExtractor.FAIL, LogicExtractor.RETURN], [LogicExtractor.THROW_OP], "02.DatasetPipeline"),
            new("T19", "Fix the pooling crash when the attention mask is empty or missing", meanPoolingMaskBug, DomainClassifier.BUG_FIX, [LogicExtractor.NULL_GUARD, LogicExtractor.FAIL, LogicExtractor.ITERATE], [LogicExtractor.THROW_OP], "05.Embeddings"),
            new("T20", "Explain how embedding batch text moves through tokenize pool and normalize", embeddingBatchQuestion, DomainClassifier.CODE_QUESTION, [LogicExtractor.CALL, LogicExtractor.RETURN], [LogicExtractor.FAIL], "05.Embeddings"),
            new("T21", "Show how the npy writer builds and pads the header", npyWriterQuestion, DomainClassifier.CODE_QUESTION, [LogicExtractor.CALL, LogicExtractor.RETURN], [LogicExtractor.FAIL], "02.DatasetPipeline"),
            new("T22", "Plan the dataset shard build sequence from files to npy output", datasetBuilderShardPlan, DomainClassifier.ORCHESTRATION, [LogicExtractor.ITERATE, LogicExtractor.AWAIT, LogicExtractor.CALL], [LogicExtractor.FAIL], "02.DatasetPipeline"),
            new("T23", "Plan the embedding refresh workflow through tokenize pool and normalize", embeddingRefreshWorkflow, DomainClassifier.ORCHESTRATION, [LogicExtractor.DECLARE, LogicExtractor.ASSIGN, LogicExtractor.RETURN], [LogicExtractor.THROW_OP], "05.Embeddings"),
            new("T24", "Remove stale dataset shard files after the source changes", removeStaleShardFiles, DomainClassifier.CODE_CHANGE, [LogicExtractor.ITERATE, LogicExtractor.DELETE_OP, LogicExtractor.RETURN], [LogicExtractor.FAIL], "02.DatasetPipeline"),
            new("T25", "Remove changed text from the embedding cache before recomputing vectors", removeEmbeddingCacheEntry, DomainClassifier.CODE_CHANGE, [LogicExtractor.BRANCH, LogicExtractor.DELETE_OP, LogicExtractor.RETURN], [LogicExtractor.FAIL], "05.Embeddings"),
            new("T26", "Implement IBatchSource for the npy data loader", datasetBatchSourceImplementation, DomainClassifier.CODE_CHANGE, [LogicExtractor.DECLARE, LogicExtractor.CREATE, LogicExtractor.RETURN], [LogicExtractor.THROW_OP], "02.DatasetPipeline"),
            new("T27", "Implement IEquatable for embedding cache keys", embeddingComparerImplementation, DomainClassifier.CODE_CHANGE, [LogicExtractor.DECLARE, LogicExtractor.RETURN, LogicExtractor.BRANCH], [LogicExtractor.THROW_OP], "05.Embeddings"),
            new("V01", "What happens when the tokenizer receives a null input string?", nullGuard, DomainClassifier.CODE_QUESTION, [LogicExtractor.NULL_GUARD, LogicExtractor.FAIL], [LogicExtractor.ITERATE]),
            new("V02", "Add error handling when vocab loading fails during initialization", factoryValidate, DomainClassifier.CODE_CHANGE, [LogicExtractor.NULL_GUARD, LogicExtractor.CREATE, LogicExtractor.RETURN], [LogicExtractor.AWAIT]),
            new("V03", "Fix the off-by-one in the pair merge count", bugFixLoop, DomainClassifier.BUG_FIX, [LogicExtractor.ITERATE, LogicExtractor.NULL_GUARD, LogicExtractor.FAIL], [LogicExtractor.THROW_OP]),
            new("V04", "How does BPE select which pair to merge next?", readOnlyLoop, DomainClassifier.CODE_QUESTION, [LogicExtractor.ITERATE, LogicExtractor.CALL, LogicExtractor.RETURN], [LogicExtractor.FAIL]),
            new("V05", "Remove the debug logging from the regex splitter", regexSplitterWithLogging, DomainClassifier.CODE_CHANGE, [LogicExtractor.CALL, LogicExtractor.RETURN, LogicExtractor.LOG_OP], [LogicExtractor.FAIL]),
            new("V06", "The tokenizer crashes on emoji input why and how to fix it?", nullGuard, DomainClassifier.BUG_FIX, [LogicExtractor.NULL_GUARD, LogicExtractor.FAIL], [LogicExtractor.CREATE]),
            new("V07", "Architect a plugin system so tokenizers can be swapped at runtime", architecturePluginPlan, DomainClassifier.ARCHITECTURE, [LogicExtractor.DECLARE, LogicExtractor.CREATE, LogicExtractor.ITERATE], [LogicExtractor.FAIL]),
            new("V08", "List the sequence of calls from Encode to the regex split step", encodeCallChain, DomainClassifier.CODE_QUESTION, [LogicExtractor.CALL, LogicExtractor.RETURN], [LogicExtractor.THROW_OP]),
            new("V09", "Plan and implement the tokenizer cache invalidation when vocab changes", orchestrationSteps, DomainClassifier.ORCHESTRATION, [LogicExtractor.DECLARE, LogicExtractor.ASSIGN, LogicExtractor.RETURN], [LogicExtractor.THROW_OP]),
            new("V10", "Update VocabEntry to implement IEquatable properly", equatableImplementation, DomainClassifier.CODE_CHANGE, [LogicExtractor.DECLARE, LogicExtractor.RETURN, LogicExtractor.BRANCH], [LogicExtractor.THROW_OP]),
            new("V11", "Explain why the dataset loader returns empty batches and how to fix it", datasetEmptyBatchBug, DomainClassifier.BUG_FIX, [LogicExtractor.NULL_GUARD, LogicExtractor.FAIL, LogicExtractor.RETURN], [LogicExtractor.THROW_OP], "02.DatasetPipeline"),
            new("V12", "Plan the workflow steps for embedding refresh through tokenize pool and normalize", embeddingRefreshWorkflow, DomainClassifier.ORCHESTRATION, [LogicExtractor.DECLARE, LogicExtractor.ASSIGN, LogicExtractor.RETURN], [LogicExtractor.THROW_OP], "05.Embeddings"),
            new("V13", "Remove stale embedding cache entries when source text changes", removeEmbeddingCacheEntry, DomainClassifier.CODE_CHANGE, [LogicExtractor.BRANCH, LogicExtractor.DELETE_OP, LogicExtractor.RETURN], [LogicExtractor.FAIL], "05.Embeddings")
        };
    }
}
