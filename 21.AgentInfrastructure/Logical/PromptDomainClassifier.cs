// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

// This classifier maps raw developer prompts to the five D-04 business domains before sub-phase routing.
public static class PromptDomainClassifier
{
    public const string DomainLabelMetadataKey = "domain.label";
    public const string DomainConfidenceMetadataKey = "domain.confidence";
    public const string DomainEvidenceTermsMetadataKey = "domain.evidence_terms";
    public const string DomainUsedContextMetadataKey = "domain.used_context";
    public const string CompiledPromptTextMetadataKey = "prompt.compiled.text";
    public const string CompiledPredicateIdsMetadataKey = "prompt.compiled.predicate_ids";
    public const string CompiledPredicateSlotsMetadataKey = "prompt.compiled.predicate_slots";

    private static readonly SemanticCompilerAdapter.DomainClassifierUnit Classifier = BuildSeededClassifier();

    // Classify one raw prompt into the D-04 domain label used for domain-scoped adapters.
    public static PromptDomainClassification Classify(
        string prompt,
        IReadOnlyDictionary<string, object>? metadata = null,
        SemanticCompilerAdapter.DomainClassifierUnit? classifier = null)
    {
        var compiledPrompt = ReadString(metadata, CompiledPromptTextMetadataKey) ?? prompt ?? string.Empty;
        var predicateSlots = ReadIntArray(metadata, CompiledPredicateSlotsMetadataKey)
            ?? SemanticCompilerAdapter.DomainClassifierUnit.EncodePredicateProfile(ReadStringArray(metadata, CompiledPredicateIdsMetadataKey));
        var classification = (classifier ?? Classifier).Classify(new DomainClassifierRequest(
            compiledPrompt,
            predicateSlots.Length == 0 ? null : predicateSlots,
            Metadata: metadata));
        return new PromptDomainClassification(
            classification.Label,
            classification.Confidence,
            classification.Evidence,
            classification.UsedContext);
    }

    // Add domain classification to metadata without mutating caller-owned dictionaries.
    public static IReadOnlyDictionary<string, object> AddToMetadata(
        IReadOnlyDictionary<string, object> metadata,
        PromptDomainClassification classification)
    {
        var enriched = new Dictionary<string, object>(metadata ?? new Dictionary<string, object>(), StringComparer.Ordinal)
        {
            [DomainLabelMetadataKey] = classification.Domain.ToString(),
            [DomainConfidenceMetadataKey] = classification.Confidence,
            [DomainEvidenceTermsMetadataKey] = classification.EvidenceTerms,
            [DomainUsedContextMetadataKey] = classification.UsedContext
        };

        return enriched;
    }

    public static IReadOnlyDictionary<string, object> AddCompiledPromptToMetadata(
        IReadOnlyDictionary<string, object> metadata,
        SubPhaseRouterSourceDocument compiledPrompt)
    {
        var predicateIds = new string[compiledPrompt.CompiledPredicateIds.Length];
        Array.Copy(compiledPrompt.CompiledPredicateIds, predicateIds, predicateIds.Length);
        var enriched = new Dictionary<string, object>(metadata ?? new Dictionary<string, object>(), StringComparer.Ordinal)
        {
            [CompiledPromptTextMetadataKey] = compiledPrompt.CompiledText,
            [CompiledPredicateIdsMetadataKey] = predicateIds,
            [CompiledPredicateSlotsMetadataKey] = SemanticCompilerAdapter.DomainClassifierUnit.EncodePredicateProfile(predicateIds)
        };

        return enriched;
    }

    private static SemanticCompilerAdapter.DomainClassifierUnit BuildSeededClassifier()
    {
        var classifier = new SemanticCompilerAdapter.DomainClassifierUnit();
        classifier.Train([
            ([0, 1, 2], Domain.BUG),
            ([1], Domain.BUG),
            ([5, 8, 9, 10, 19], Domain.PLAN),
            ([0, 2], Domain.ARCH),
            ([0, 2, 3, 4, 21], Domain.ARCH),
            ([12, 13, 14], Domain.CODE_Q),
            ([5, 7], Domain.REFACTOR)
        ]);
        return classifier;
    }

    private static string? ReadString(IReadOnlyDictionary<string, object>? metadata, string key)
    {
        if (metadata is null || !metadata.TryGetValue(key, out var value))
            return null;

        return value as string;
    }

    private static string[] ReadStringArray(IReadOnlyDictionary<string, object>? metadata, string key)
    {
        if (metadata is null || !metadata.TryGetValue(key, out var value))
            return [];

        return value switch
        {
            string[] typed => typed,
            IEnumerable<string> enumerable => enumerable.ToArray(),
            object[] objects => objects.OfType<string>().ToArray(),
            _ => []
        };
    }

    private static int[]? ReadIntArray(IReadOnlyDictionary<string, object>? metadata, string key)
    {
        if (metadata is null || !metadata.TryGetValue(key, out var value))
            return null;

        return value switch
        {
            int[] typed => typed,
            IEnumerable<int> enumerable => enumerable.ToArray(),
            object[] objects => objects.OfType<int>().ToArray(),
            _ => null
        };
    }

}

public sealed record PromptDomainClassification(
    Domain Domain,
    double Confidence,
    string[] EvidenceTerms,
    bool UsedContext = false);

// This unit is the registry-resolvable domain classifier used before domain-scoped adapter selection.
public sealed class PromptDomainClassifierUnit : ILogicalUnit<string, PromptDomainClassification>
{
    // This id identifies the deterministic semantic-compiler-backed classifier.
    public const string Id = "unit-prompt-domain-classifier-v1";

    // This role is the swappable slot learned domain classifiers replace.
    public const string Role = SemanticCompilerAdapter.DomainClassifierUnit.Role;

    // This domain tag scopes the classifier to whole-development-area orchestration.
    public const string DomainTag = "whole_development_area";

    private readonly SemanticCompilerAdapter.DomainClassifierUnit _classifier;

    public PromptDomainClassifierUnit(SemanticCompilerAdapter.DomainClassifierUnit? classifier = null)
    {
        _classifier = classifier ?? new SemanticCompilerAdapter.DomainClassifierUnit();
    }

    // Execute one prompt classification through the shared logical-unit contract.
    public Task<global::AgentInfrastructure.OperationResult<LogicalUnitResult<PromptDomainClassification>>> ExecuteAsync(
        LogicalUnitRequest<string> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(global::AgentInfrastructure.OperationResult<LogicalUnitResult<PromptDomainClassification>>.Fail("Cancelled.", "CANCELLED"));

        // Reject empty prompts before classification.
        if (string.IsNullOrWhiteSpace(request.Input))
            return Task.FromResult(global::AgentInfrastructure.OperationResult<LogicalUnitResult<PromptDomainClassification>>.Fail("User prompt cannot be empty.", "EMPTY_REQUEST"));

        // Delegate to the canonical semantic adapter classifier.
        var classification = PromptDomainClassifier.Classify(request.Input, request.Metadata, _classifier);

        // Wrap the classification in a traceable logical-unit result.
        var result = new LogicalUnitResult<PromptDomainClassification>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            classification,
            classification.Confidence,
            classification.EvidenceTerms,
            [DomainTag, classification.Domain.ToString()],
            classification.EvidenceTerms.Length == 0 ? [] : classification.EvidenceTerms,
            []);

        return Task.FromResult(global::AgentInfrastructure.OperationResult<LogicalUnitResult<PromptDomainClassification>>.Ok(result));
    }
}
