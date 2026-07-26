// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler29;

// Canonical compiler for developer prompts entering the orchestration machine.
// Project 21 must consume this output instead of maintaining its own prompt lexicon.
public sealed class DevelopmentPromptCompiler
{
    private static readonly (string PredicateId, string[] Terms)[] DevelopmentPredicateLexicon =
    [
        ("SECURITY", ["security", "authorization", "authentication", "auth", "jwt", "oauth", "oidc", "pkce", "session", "asvs", "mfa", "rbac"]),
        ("AUTHORIZATION", ["authorization", "authorisation", "permission", "rbac", "scope", "tenant", "ownership"]),
        ("API_CONTRACT", ["api", "endpoint", "contract", "schema", "request", "response", "openapi"]),
        ("ARCHITECTURE", ["architecture", "architect", "component topology", "system boundary", "dependency direction", "project", "codebase", "code base", "existing code", "old code", "current code"]),
        ("IMPLEMENT", ["implement the", "implementation plan", "connector", "provider", "integration layer", "code change", "code", "fix", "bug", "issue", "async", "null", "generated frame", "error", "failure", "failed", "no data", "returned no data", "log"]),
        ("TEST", ["test", "testing", "integration test", "unit test", "coverage", "fixture", "verify", "validation", "automation test", "fix", "bug", "issue", "regression", "error", "failure", "failed", "no data", "returned no data", "log", "try again"]),
        ("QA", ["qa", "quality", "release", "acceptance", "verification"]),
        ("UI", ["ui", "ux", "screen", "layout", "figma", "frame", "interaction", "accessibility"]),
        ("DOCUMENT", ["documentation", "docs", "readme", "write document", "create document", "generate document", "document this", "spec", "specification", "requirements"]),
        ("AUTOMATION", ["automation", "automate", "automated", "ci", "pipeline", "quality gate", "quality gates", "build gate", "build gates", "monitoring feedback", "coverage reporting", "reporting dashboard", "repeatable validation"]),
        ("LEARNING", ["skill", "skills", "claude code", "learning", "teach", "training", "reusable guidance", "knowledge", "model feedback"])
    ];

    private readonly ISemanticCompiler29 _pipeline;
    private readonly Decomposer _decomposer;
    private readonly SemanticNormalizer _normalizer;
    private readonly FamilyCollapser _collapser;

    public DevelopmentPromptCompiler(
        ISemanticCompiler29? pipeline = null,
        Decomposer? decomposer = null,
        SemanticNormalizer? normalizer = null,
        FamilyCollapser? collapser = null)
    {
        var storage = new InMemoryCompilerStorage();
        var logger = new NoopCompilerEventLogger();
        _pipeline = pipeline ?? new SemanticCompilerPipeline(storage, logger);
        _decomposer = decomposer ?? new Decomposer(storage, logger);
        _normalizer = normalizer ?? new SemanticNormalizer(storage, logger);
        _collapser = collapser ?? new FamilyCollapser(storage, logger);
    }

    public OperationResult<DevelopmentPromptCompilation> Compile(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return OperationResult<DevelopmentPromptCompilation>.Fail("Prompt cannot be empty.", "EMPTY_PROMPT");

        var root = CompileOne(prompt.Trim());
        if (!root.IsSuccess)
            return root.MapError<DevelopmentPromptCompilation>();

        var split = _decomposer.Decompose(prompt.Trim());
        if (!split.IsSuccess)
            return split.MapError<DevelopmentPromptCompilation>();

        var segmentTexts = split.Value!.Length <= 1
            ? SplitBoundaryFallback(prompt.Trim())
            : split.Value!;

        var segments = segmentTexts
            .Select(segment => CompileOne(segment.Trim()))
            .Where(compiled => compiled.IsSuccess)
            .Select(compiled => new DevelopmentPromptSegment(
                compiled.Value!.Text,
                compiled.Value.Domain,
                compiled.Value.PipelinePredicates,
                compiled.Value.DevelopmentPredicateIds,
                compiled.Value.CompressionRatio))
            .ToArray();

        return OperationResult<DevelopmentPromptCompilation>.Ok(root.Value! with { Segments = segments });
    }

    private static string[] SplitBoundaryFallback(string prompt)
    {
        var lower = prompt.ToLowerInvariant();
        foreach (var marker in new[] { " and then ", " after that ", " next ", " additionally " })
        {
            var index = lower.IndexOf(marker, StringComparison.Ordinal);
            if (index < 0)
                continue;

            var left = prompt[..index].Trim();
            var right = prompt[(index + marker.Length)..].Trim();
            if (left.Length > 0 && right.Length > 0)
                return [left, right];
        }

        return [prompt];
    }

    private OperationResult<DevelopmentPromptCompilation> CompileOne(string text)
    {
        var pipeline = _pipeline.Compile(new PipelineInput(text, FilePath: null));
        if (!pipeline.IsSuccess)
            return pipeline.MapError<DevelopmentPromptCompilation>();

        var normalized = _normalizer.Normalize(text);
        var collapsed = _collapser.Collapse(normalized);
        var naturalEvidenceText = string.Join(" ", [text, .. normalized, .. collapsed]);
        var developmentPredicates = ExtractDevelopmentPredicates(text, naturalEvidenceText, pipeline.Value!.Domain, pipeline.Value.Predicates);

        return OperationResult<DevelopmentPromptCompilation>.Ok(new DevelopmentPromptCompilation(
            text,
            pipeline.Value.Domain,
            pipeline.Value.StackLanguage,
            pipeline.Value.Predicates,
            developmentPredicates,
            string.Join(" ", collapsed),
            pipeline.Value.CompressionRatio,
            []));
    }

    private static string[] ExtractDevelopmentPredicates(string rawText, string evidenceText, Domain domain, string[] pipelinePredicates)
    {
        var raw = rawText.ToLowerInvariant();
        var normalized = evidenceText.ToLowerInvariant();
        var predicates = DevelopmentPredicateLexicon
            .Where(entry => entry.Terms.Any(term => normalized.Contains(term, StringComparison.Ordinal)))
            .Select(entry => entry.PredicateId)
            .ToHashSet(StringComparer.Ordinal);

        var hasImplementationSignal =
            raw.Contains("implement ", StringComparison.Ordinal) ||
            raw.Contains("implement the", StringComparison.Ordinal) ||
            raw.Contains("build", StringComparison.Ordinal) ||
            raw.Contains("create", StringComparison.Ordinal) ||
            raw.Contains("provider", StringComparison.Ordinal) ||
            raw.Contains("connector", StringComparison.Ordinal) ||
            raw.Contains("integration layer", StringComparison.Ordinal);

        foreach (var predicate in pipelinePredicates)
        {
            if (predicate.Contains("AUTH", StringComparison.OrdinalIgnoreCase))
            {
                predicates.Add("SECURITY");
                predicates.Add("AUTHORIZATION");
            }
            if (predicate.Contains("VALIDATE", StringComparison.OrdinalIgnoreCase) ||
                predicate.Contains("NULL", StringComparison.OrdinalIgnoreCase) ||
                predicate.Contains("THROW", StringComparison.OrdinalIgnoreCase) ||
                predicate.Contains("CATCH", StringComparison.OrdinalIgnoreCase))
            {
                predicates.Add("IMPLEMENT");
                predicates.Add("TEST");
            }
            if (predicate.Contains("CREATE", StringComparison.OrdinalIgnoreCase) ||
                predicate.Contains("UPDATE", StringComparison.OrdinalIgnoreCase) ||
                (predicate.Contains("CALL", StringComparison.OrdinalIgnoreCase) && hasImplementationSignal))
                predicates.Add("IMPLEMENT");
        }

        if (domain == Domain.ARCH)
            predicates.Add("ARCHITECTURE");
        if (domain == Domain.PLAN)
            predicates.Add("LEARNING");
        if (domain == Domain.REFACTOR)
            predicates.Add("IMPLEMENT");
        if (domain == Domain.BUG)
        {
            predicates.Add("IMPLEMENT");
            predicates.Add("TEST");
        }

        var hasSecuritySignal = predicates.Contains("SECURITY") || predicates.Contains("AUTHORIZATION");
        var hasRuntimeFailureSignal =
            raw.Contains("fix", StringComparison.Ordinal) ||
            raw.Contains("bug", StringComparison.Ordinal) ||
            raw.Contains("error", StringComparison.Ordinal) ||
            raw.Contains("failed", StringComparison.Ordinal) ||
            raw.Contains("failure", StringComparison.Ordinal) ||
            raw.Contains("null", StringComparison.Ordinal) ||
            raw.Contains("no data", StringComparison.Ordinal);
        var hasExplicitTestSignal =
            raw.Contains("test", StringComparison.Ordinal) ||
            raw.Contains("tests", StringComparison.Ordinal) ||
            raw.Contains("regression", StringComparison.Ordinal) ||
            raw.Contains("unit test", StringComparison.Ordinal);
        if (hasSecuritySignal && !hasRuntimeFailureSignal && !hasExplicitTestSignal && !hasImplementationSignal)
        {
            predicates.Remove("IMPLEMENT");
            predicates.Remove("TEST");
            predicates.Remove("QA");
            predicates.Remove("LEARNING");
        }

        return predicates.OrderBy(predicate => predicate, StringComparer.Ordinal).ToArray();
    }

    private sealed class InMemoryCompilerStorage : ICompilerStorage
    {
        private readonly Dictionary<string, PipelineOutput> _items = new(StringComparer.Ordinal);
        public void Save(string key, PipelineOutput output) => _items[key] = output;
        public PipelineOutput? Load(string key) => _items.TryGetValue(key, out var output) ? output : null;
    }

    private sealed class NoopCompilerEventLogger : ICompilerEventLogger
    {
        public void Log(StandardEvent evt) { }
    }
}

public sealed record DevelopmentPromptCompilation(
    string Text,
    Domain Domain,
    string StackLanguage,
    string[] PipelinePredicates,
    string[] DevelopmentPredicateIds,
    string NormalizedText,
    double CompressionRatio,
    DevelopmentPromptSegment[] Segments);

public sealed record DevelopmentPromptSegment(
    string Text,
    Domain Domain,
    string[] PipelinePredicates,
    string[] DevelopmentPredicateIds,
    double CompressionRatio);
