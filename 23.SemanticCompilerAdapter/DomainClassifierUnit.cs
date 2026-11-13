// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompilerAdapter;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using SemanticCompiler29;

public sealed record DomainClassifierRequest(
    string Prompt,
    int[]? Predicates = null,
    string[]? DevelopmentPredicateIds = null,
    string[]? PipelinePredicates = null,
    string? NormalizedText = null,
    Domain? CompilerDomain = null,
    object? PriorContext = null,
    IReadOnlyDictionary<string, object>? Metadata = null);

public sealed record DomainClassification(
    Domain Label,
    double Confidence,
    string[] Evidence,
    bool UsedContext);

public sealed class DomainClassifierUnit : ILogicalUnit<DomainClassifierRequest, DomainClassification>
{
    public const string Id = "unit-domain-classifier-v1";
    public const string Role = "domain_classifier";
    public const string DomainTag = "whole_development_area";

    private readonly SemanticCompiler.RouteClassifier _classifier;
    private bool _trained;

    public DomainClassifierUnit(SemanticCompiler.RouteClassifier? classifier = null)
    {
        _classifier = classifier ?? new SemanticCompiler.RouteClassifier();
    }

    public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<DomainClassification>>> ExecuteAsync(
        LogicalUnitRequest<DomainClassifierRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<DomainClassification>>.Fail("Cancelled.", "CANCELLED"));

        var classification = Classify(request.Input);
        var result = new LogicalUnitResult<DomainClassification>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            classification,
            classification.Confidence,
            classification.Evidence,
            [DomainTag, classification.Label.ToString()],
            classification.Evidence,
            []);

        return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<DomainClassification>>.Ok(result));
    }

    public void Train(IReadOnlyList<(int[] Predicates, Domain Domain)> examples)
    {
        _classifier.Train(examples);
        _trained = examples.Count > 0;
    }

    public static int[] EncodePredicateProfile(IEnumerable<string> predicateIds)
    {
        return predicateIds
            .Select(TrainingVectorBuilder.PredicateSlotFor)
            .Where(slot => slot >= 0)
            .Distinct()
            .OrderBy(slot => slot)
            .ToArray();
    }

    public static DomainClassifierRequest BuildRequestFromCompilation(
        DevelopmentPromptCompilation compiled,
        object? priorContext = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        var predicateIds = compiled.DevelopmentPredicateIds
            .Concat(compiled.PipelinePredicates)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return new DomainClassifierRequest(
            compiled.NormalizedText,
            EncodePredicateProfile(predicateIds),
            compiled.DevelopmentPredicateIds,
            compiled.PipelinePredicates,
            compiled.NormalizedText,
            compiled.Domain,
            priorContext,
            metadata);
    }

    public DomainClassification Classify(DomainClassifierRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt) && (request.Predicates is null || request.Predicates.Length == 0))
            return new DomainClassification(Domain.UNKNOWN, 0.20, ["empty_prompt"], UsedContext: false);

        var baseClassification = ClassifyBaseDomain(request);
        var baseDomain = baseClassification.Domain;
        var evidenceText = request.NormalizedText ?? request.Prompt;
        var keywordEvidence = SemanticCompiler29.RouteClassifier.MatchedKeywords(evidenceText, baseDomain).ToList();
        var evidence = keywordEvidence.ToList();
        if (request.Predicates is { Length: > 0 })
            evidence.Add("compiled_predicate_profile");
        if (baseClassification.UsedKnn)
            evidence.Add("trained_predicate_knn");
        if (baseClassification.UsedPredicateAffinity)
            evidence.Add("predicate_affinity");

        var confidence = CalculateBaseConfidence(evidenceText, baseDomain, keywordEvidence.Count);
        if (baseClassification.UsedKnn)
            confidence = Math.Max(confidence, 0.90);
        else if (baseClassification.UsedPredicateAffinity && keywordEvidence.Count == 0)
            confidence = Math.Min(confidence, 0.25);
        var context = request.PriorContext is not null
            ? ContextPacketReader.Read(request.PriorContext)
            : ContextPacketReader.Read(request.Metadata);
        var usedContext = false;

        if (!context.IsEmpty)
        {
            var contextDomain = context.PriorDomains.FirstOrDefault();
            if (context.PriorDomains.Contains(baseDomain))
            {
                confidence += 0.10;
                evidence.Add("context_domain_match");
                usedContext = true;
            }
            else if (contextDomain != Domain.UNKNOWN && baseDomain != Domain.UNKNOWN)
            {
                confidence -= 0.05;
                evidence.Add("context_domain_shift");
                usedContext = true;
            }

            if (context.PriorConfidenceMap.TryGetValue(baseDomain, out var priorConfidence))
            {
                confidence = Math.Max(confidence, Math.Min(0.99, priorConfidence + 0.02));
                evidence.Add("context_prior_confidence");
                usedContext = true;
            }
        }

        return new DomainClassification(
            baseDomain,
            Math.Round(Math.Clamp(confidence, 0.20, 0.99), 2),
            evidence.Count == 0 ? ["semantic_domain_keyword_fallback"] : evidence.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            usedContext);
    }

    private BaseDomainClassification ClassifyBaseDomain(DomainClassifierRequest request)
    {
        if (_trained && request.Predicates is { Length: > 0 } predicates)
        {
            var classified = _classifier.Classify(predicates);
            if (classified.IsSuccess)
                return new BaseDomainClassification(classified.Value, UsedKnn: true, UsedPredicateAffinity: false);
        }

        var evidenceText = request.NormalizedText ?? request.Prompt;
        if (HasDirectQuestionSignal(evidenceText))
            return new BaseDomainClassification(Domain.CODE_Q, UsedKnn: false, UsedPredicateAffinity: false);

        if (HasRuntimeFailureSignal(evidenceText))
            return new BaseDomainClassification(Domain.BUG, UsedKnn: false, UsedPredicateAffinity: false);

        if (request.Predicates is { Length: > 0 } untrainedPredicates)
        {
            var affinity = ClassifyByPredicateAffinity(untrainedPredicates);
            if (affinity != Domain.UNKNOWN)
                return new BaseDomainClassification(affinity, UsedKnn: false, UsedPredicateAffinity: true);
        }

        return new BaseDomainClassification(SemanticCompiler.RouteClassifier.ClassifyByKeywords(evidenceText), UsedKnn: false, UsedPredicateAffinity: false);
    }

    private static Domain ClassifyByPredicateAffinity(int[] predicates)
    {
        var votes = predicates
            .Select(DomainForPredicateSlot)
            .Where(domain => domain != Domain.UNKNOWN)
            .GroupBy(domain => domain)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => DomainTieBreakRank(group.Key))
            .FirstOrDefault();

        return votes?.Key ?? Domain.UNKNOWN;
    }

    private static Domain DomainForPredicateSlot(int slot) => slot switch
    {
        // SECURITY / AUTHORIZATION / API_CONTRACT / ARCHITECTURE / AUTH_OP.
        0 or 2 or 3 or 4 or 21 => Domain.ARCH,

        // IMPLEMENT / UI / DOCUMENT / LEARNING / SEND_OP.
        5 or 8 or 9 or 10 or 19 => Domain.PLAN,

        // TEST / QA / FAIL.
        1 or 6 or 7 => Domain.BUG,

        // GET_OP / VALIDATE_OP / CREATE_OP preserve code-question lookup semantics.
        12 or 13 or 14 => Domain.CODE_Q,

        _ => Domain.UNKNOWN
    };

    private static int DomainTieBreakRank(Domain domain) => domain switch
    {
        Domain.BUG => 0,
        Domain.REFACTOR => 1,
        Domain.PLAN => 2,
        Domain.ARCH => 3,
        Domain.CODE_Q => 4,
        _ => 5
    };

    private static bool HasRuntimeFailureSignal(string prompt)
    {
        var lower = (prompt ?? string.Empty).ToLowerInvariant();
        return ContainsAny(lower, "fix", "error", "exception", "fail", "failed", "failure", "wrong", "broken", "debug", "crash", "bug", "runtime error", "null reference");
    }

    private static bool HasDirectQuestionSignal(string prompt)
    {
        var lower = (prompt ?? string.Empty).ToLowerInvariant();
        return ContainsAny(lower,
            "what does",
            "what happens",
            "what pattern",
            "how does",
            "show ",
            "list ",
            "find ",
            "where ",
            "which ",
            "does ",
            "return ");
    }

    private static bool ContainsAny(string text, params string[] terms)
    {
        return terms.Any(term => text.Contains(term, StringComparison.Ordinal));
    }

    private static double CalculateBaseConfidence(string prompt, Domain domain, int evidenceCount)
    {
        if (domain == Domain.UNKNOWN)
            return 0.20;

        if (string.IsNullOrWhiteSpace(prompt))
            return 0.50;

        return evidenceCount switch
        {
            >= 3 => 0.95,
            2 => 0.90,
            1 => 0.82,
            _ => 0.20
        };
    }

    private sealed record BaseDomainClassification(Domain Domain, bool UsedKnn, bool UsedPredicateAffinity);
}
