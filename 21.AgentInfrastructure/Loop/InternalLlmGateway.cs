// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure;

using AgentInfrastructure.Logical;
using SemanticCompilerAdapter;

public enum InternalModelDomain { CodeQ, Bug, Plan, Arch, Refactor, Generic }

public interface IDomainModelRegistry
{
    OperationResult<string> ResolveAdapter(InternalModelDomain domain);
}

public sealed record InternalLlmGatewayResponse(
    string Response,
    InternalModelDomain Domain,
    string AdapterPath,
    bool UsedFallbackAdapter);

public sealed record InternalLlmRoutingInspection(
    InternalModelDomain Domain,
    string AdapterPath,
    bool UsedFallbackAdapter,
    double Confidence);

public sealed class InternalLlmGateway
{
    private readonly ILlmClient _llm;
    private readonly IDomainModelRegistry _registry;
    private readonly DomainClassifierUnit _domainClassifier;
    private readonly InMemoryLogicalUnitRegistry? _logicalUnits;

    public int ExternalCallCount => 0;

    public InternalLlmGateway(
        ILlmClient llm,
        IDomainModelRegistry registry,
        DomainClassifierUnit? domainClassifier = null,
        InMemoryLogicalUnitRegistry? logicalUnits = null)
    {
        _llm = llm;
        _registry = registry;
        _domainClassifier = domainClassifier ?? new DomainClassifierUnit();
        _logicalUnits = logicalUnits;
    }

    public async Task<OperationResult<InternalLlmGatewayResponse>> SendAsync(
        string prompt,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return OperationResult<InternalLlmGatewayResponse>.Fail("Prompt is required.", "EMPTY_PROMPT");

        var inspection = InspectRouting(prompt);
        if (!inspection.IsSuccess)
            return OperationResult<InternalLlmGatewayResponse>.Fail(inspection.ErrorMessage, inspection.ErrorCode);

        var route = inspection.Value!;
        var history = new List<ConversationMessage>
        {
            new("system", $"Use internal adapter: {route.AdapterPath}"),
            new("user", prompt)
        };

        var result = await _llm.SendAsync(history, null, ct);
        if (!result.IsSuccess)
            return OperationResult<InternalLlmGatewayResponse>.Fail(result.ErrorMessage, result.ErrorCode);

        return OperationResult<InternalLlmGatewayResponse>.Ok(new InternalLlmGatewayResponse(
            result.Value!.ResponseText,
            route.Domain,
            route.AdapterPath,
            route.UsedFallbackAdapter));
    }

    public OperationResult<InternalLlmRoutingInspection> InspectRouting(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return OperationResult<InternalLlmRoutingInspection>.Fail("Prompt is required.", "EMPTY_PROMPT");

        var (domain, confidence) = ClassifyWithCanonicalEvidence(prompt);
        if (confidence < 0.30)
            domain = InternalModelDomain.Generic;

        var adapter = ResolveAdapterWithFallback(domain);
        return OperationResult<InternalLlmRoutingInspection>.Ok(new InternalLlmRoutingInspection(
            domain,
            adapter.AdapterPath,
            adapter.UsedFallback,
            confidence));
    }

    private (string AdapterPath, bool UsedFallback) ResolveAdapterWithFallback(InternalModelDomain domain)
    {
        var resolved = _registry.ResolveAdapter(domain);
        if (resolved.IsSuccess)
            return (resolved.Value!, false);

        var fallback = _registry.ResolveAdapter(InternalModelDomain.Generic);
        return fallback.IsSuccess
            ? (fallback.Value!, true)
            : ("generic.lora", true);
    }

    private static InternalModelDomain MapDomain(SemanticCompiler29.Domain domain)
        => domain switch
        {
            SemanticCompiler29.Domain.BUG => InternalModelDomain.Bug,
            SemanticCompiler29.Domain.PLAN => InternalModelDomain.Plan,
            SemanticCompiler29.Domain.ARCH => InternalModelDomain.Arch,
            SemanticCompiler29.Domain.REFACTOR => InternalModelDomain.Refactor,
            SemanticCompiler29.Domain.CODE_Q => InternalModelDomain.CodeQ,
            _ => InternalModelDomain.Generic
        };

    private (InternalModelDomain Domain, double Confidence) ClassifyWithCanonicalEvidence(string prompt)
    {
        var compiled = new SemanticTeachingPromptCompiler().Compile(prompt);
        var classifierRequest = compiled.IsSuccess
            ? DomainClassifierUnit.BuildRequestFromCompilation(
                compiled.Value!.Compilation,
                metadata: new Dictionary<string, object>
                {
                    ["compiled_prompt.normalized_text"] = compiled.Value.Compilation.NormalizedText,
                    ["compiled_prompt.domain"] = compiled.Value.Compilation.Domain.ToString()
                })
            : new DomainClassifierRequest(prompt);

        var classified = ClassifyWithRegistryFallback(classifierRequest);
        return classified.Confidence < 0.30
            ? (InternalModelDomain.Generic, classified.Confidence)
            : (MapDomain(classified.Label), classified.Confidence);
    }

    private DomainClassification ClassifyWithRegistryFallback(DomainClassifierRequest classifierRequest)
    {
        var resolved = _logicalUnits?.ResolveLatest<DomainClassifierRequest, DomainClassification>(
            CompiledDomainClassifierUnit.Role,
            CompiledDomainClassifierUnit.DomainTag,
            LogicalUnitTrustState.Probationary);
        if (resolved is { IsSuccess: true, Value: CompiledDomainClassifierUnit compiledUnit })
            return compiledUnit.Classify(classifierRequest);
        if (resolved is { IsSuccess: true, Value: DomainClassifierUnit domainUnit })
            return domainUnit.Classify(classifierRequest);

        return _domainClassifier.Classify(classifierRequest);
    }
}
