// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;
using DevelopmentPromptCompilation = SemanticCompiler29.DevelopmentPromptCompilation;
using SemanticCompilerAdapter;

// This unit compiles a raw assistant request into the source-document shape consumed by the sub-phase router.
public sealed class PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
{
    // This id identifies the first prompt-to-routing-input compiler.
    public const string Id = "unit-prompt-routing-input-compiler-v1";

    // This role names the normalization step before sub-phase routing.
    public const string Role = "prompt-routing-input-compiler";

    // This domain tag scopes the compiler to whole-development-area orchestration.
    public const string DomainTag = "whole_development_area";

    private readonly SemanticTeachingPromptCompiler _compiler;
    private readonly InMemoryLogicalUnitRegistry? _logicalUnits;

    public PromptRoutingInputCompilerUnit(
        SemanticTeachingPromptCompiler? compiler = null,
        InMemoryLogicalUnitRegistry? logicalUnits = null)
    {
        _compiler = compiler ?? new SemanticTeachingPromptCompiler();
        _logicalUnits = logicalUnits;
    }

    public PromptRoutingInputCompilerUnit WithLogicalUnits(InMemoryLogicalUnitRegistry? logicalUnits)
    {
        return new PromptRoutingInputCompilerUnit(_compiler, logicalUnits);
    }

    // Execute one prompt compilation through the shared logical-unit contract.
    public async Task<OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>> ExecuteAsync(
        LogicalUnitRequest<LogicalOrchestratorRequest> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail("Cancelled.", "CANCELLED");

        // Reject missing requests because there is no prompt to compile.
        if (request.Input is null)
            return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail("Orchestrator request is required.", "ORCHESTRATOR_REQUEST_REQUIRED");

        // Reject empty user prompts before building router input.
        if (string.IsNullOrWhiteSpace(request.Input.UserRequest))
            return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail("User request cannot be empty.", "EMPTY_REQUEST");

        var compiledPrompt = _compiler.Compile(request.Input.UserRequest);
        if (!compiledPrompt.IsSuccess)
            return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail(compiledPrompt.ErrorMessage, compiledPrompt.ErrorCode);

        var compiled = compiledPrompt.Value!.Compilation;
        var predicateIds = compiled.DevelopmentPredicateIds;

        // Build a stable document id from the run id so routing evidence remains traceable.
        var documentId = string.IsNullOrWhiteSpace(request.Input.RunId)
            ? "logical-orchestrator-prompt"
            : $"{request.Input.RunId.Trim()}-prompt";

        var segmentDocuments = await BuildSegmentDocumentsAsync(documentId, compiled, request.Input.Metadata, request.Flags, cancellationToken);
        if (!segmentDocuments.IsSuccess)
            return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail(segmentDocuments.ErrorMessage, segmentDocuments.ErrorCode);

        // Build the compiled source document expected by SubPhaseRouterUnit.
        var sourceDocument = new SubPhaseRouterSourceDocument(
            documentId,
            "logical-orchestrator://user-prompt",
            "runtime_prompt",
            "user_request",
            "user_prompt",
            "Runtime user prompt",
            BuildCompiledTextPublic(compiled),
            predicateIds,
            "ready",
            predicateIds.Length == 0 ? "knowledge_edge: no high-signal development predicate found" : "runtime_prompt_compiled_by_semantic_compiler",
            compiled.Domain.ToString(),
            segmentDocuments.Value);

        // Build warnings only when the compiler found no high-signal predicate.
        var warnings = predicateIds.Length == 0
            ? new[] { "no_high_signal_predicate" }
            : [];

        // Wrap the source document in the standard logical-unit result.
        var result = new LogicalUnitResult<SubPhaseRouterSourceDocument>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            sourceDocument,
            predicateIds.Length == 0 ? 0.70 : 0.92,
            predicateIds,
            [DomainTag],
            [documentId],
            warnings);

        // Return the compiled routing input through the shared operation envelope.
        return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Ok(result);
    }

    public static string BuildCompiledTextPublic(DevelopmentPromptCompilation compiled)
    {
        return SemanticTeachingPromptCompiler.BuildCanonicalPromptText(compiled);
    }

    private async Task<OperationResult<SubPhaseRouterSourceDocument[]?>> BuildSegmentDocumentsAsync(
        string documentId,
        DevelopmentPromptCompilation compiled,
        IReadOnlyDictionary<string, object> metadata,
        LogicalExecutionFlags flags,
        CancellationToken cancellationToken)
    {
        if (compiled.Segments.Length <= 1)
            return OperationResult<SubPhaseRouterSourceDocument[]?>.Ok(null);

        var router = ResolveAttributedSegmentRouter(flags);
        var attributedResult = await router.ExecuteAsync(new LogicalUnitRequest<DevelopmentPromptCompilation>(
            documentId,
            AttributedSegmentRouterUnit.Id,
            AttributedSegmentRouterUnit.Role,
            AttributedSegmentRouterUnit.DomainTag,
            compiled,
            flags,
            metadata), cancellationToken);
        if (!attributedResult.IsSuccess)
            return OperationResult<SubPhaseRouterSourceDocument[]?>.Fail(attributedResult.ErrorMessage, attributedResult.ErrorCode);

        var documents = compiled.Segments
            .Zip(attributedResult.Value!.Output, (segment, attributedSegment) => (segment, attributedSegment))
            .Select((pair, index) => new SubPhaseRouterSourceDocument(
                $"{documentId}-segment-{index + 1}",
                "logical-orchestrator://user-prompt/segment",
                "runtime_prompt",
                "user_request",
                "user_prompt_segment",
                $"Runtime user prompt segment {index + 1}",
                string.Join(" ",
                    pair.segment.Text,
                    string.Join(" ", pair.segment.PipelinePredicates),
                    $"semantic_domain:{pair.attributedSegment.Domain}",
                    $"semantic_domain_confidence:{pair.attributedSegment.DomainConfidence:0.00}",
                    string.Join(" ", pair.attributedSegment.Evidence)),
                pair.segment.DevelopmentPredicateIds,
                "ready",
                pair.segment.DevelopmentPredicateIds.Length == 0 ? "knowledge_edge: no high-signal development predicate found" : "runtime_prompt_segment_compiled_by_semantic_compiler",
                pair.attributedSegment.Domain.ToString()))
            .ToArray();

        return OperationResult<SubPhaseRouterSourceDocument[]?>.Ok(documents);
    }

    private ILogicalUnit<DevelopmentPromptCompilation, AttributedSegmentDocument[]> ResolveAttributedSegmentRouter(LogicalExecutionFlags flags)
    {
        if (!flags.UseLearnedUnitWhenAvailable || _logicalUnits is null)
            return new AttributedSegmentRouterUnit();

        var resolved = _logicalUnits.ResolveLatest<DevelopmentPromptCompilation, AttributedSegmentDocument[]>(
            AttributedSegmentRouterUnit.Role,
            AttributedSegmentRouterUnit.DomainTag,
            LogicalUnitTrustState.Probationary);
        return resolved.IsSuccess ? resolved.Value! : new AttributedSegmentRouterUnit();
    }
}
