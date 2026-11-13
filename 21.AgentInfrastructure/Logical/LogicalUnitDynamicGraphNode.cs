// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// This inference-only adapter exposes an existing L2 logical unit through the canonical graph-node boundary.
public sealed class LogicalUnitDynamicGraphNode<TInput, TOutput> : IDynamicGraphNode<TInput, TOutput>
{
    private const string CancelledCode = "CANCELLED";
    private const string DescriptorRequiredCode = "DYNAMIC_GRAPH_NODE_DESCRIPTOR_REQUIRED";
    private const string UnitRequiredCode = "DYNAMIC_GRAPH_NODE_UNIT_REQUIRED";
    private const string ProjectionRequiredCode = "DYNAMIC_GRAPH_NODE_PROJECTION_REQUIRED";
    private const string InputTypeMismatchCode = "DYNAMIC_GRAPH_NODE_INPUT_TYPE_MISMATCH";
    private const string OutputTypeMismatchCode = "DYNAMIC_GRAPH_NODE_OUTPUT_TYPE_MISMATCH";
    private const string RequestRequiredCode = "DYNAMIC_GRAPH_NODE_REQUEST_REQUIRED";
    private const string RunIdRequiredCode = "DYNAMIC_GRAPH_NODE_RUN_ID_REQUIRED";
    private const string UnitIdRequiredCode = "DYNAMIC_GRAPH_NODE_UNIT_ID_REQUIRED";
    private const string UnitRoleRequiredCode = "DYNAMIC_GRAPH_NODE_UNIT_ROLE_REQUIRED";
    private const string DomainRequiredCode = "DYNAMIC_GRAPH_NODE_REQUEST_DOMAIN_REQUIRED";
    private const string InputRequiredCode = "DYNAMIC_GRAPH_NODE_INPUT_REQUIRED";
    private const string FlagsRequiredCode = "DYNAMIC_GRAPH_NODE_FLAGS_REQUIRED";
    private const string MetadataRequiredCode = "DYNAMIC_GRAPH_NODE_METADATA_REQUIRED";
    private const string RequestUnitMismatchCode = "DYNAMIC_GRAPH_NODE_REQUEST_UNIT_MISMATCH";
    private const string UnitTaskRequiredCode = "DYNAMIC_GRAPH_NODE_UNIT_TASK_REQUIRED";
    private const string UnitExceptionCode = "DYNAMIC_GRAPH_NODE_UNIT_EXCEPTION";
    private const string UnitResultRequiredCode = "DYNAMIC_GRAPH_NODE_UNIT_RESULT_REQUIRED";
    private const string ResultUnitMismatchCode = "DYNAMIC_GRAPH_NODE_RESULT_UNIT_MISMATCH";
    private const string ResultRoleMismatchCode = "DYNAMIC_GRAPH_NODE_RESULT_ROLE_MISMATCH";
    private const string ResultDomainMismatchCode = "DYNAMIC_GRAPH_NODE_RESULT_DOMAIN_MISMATCH";
    private const string ResultConfidenceInvalidCode = "DYNAMIC_GRAPH_NODE_RESULT_CONFIDENCE_INVALID";
    private const string ResultMetadataInvalidCode = "DYNAMIC_GRAPH_NODE_RESULT_METADATA_INVALID";
    private const string ProjectionExceptionCode = "DYNAMIC_GRAPH_NODE_PROJECTION_EXCEPTION";
    private const string ProjectionResultRequiredCode = "DYNAMIC_GRAPH_NODE_PROJECTION_RESULT_REQUIRED";
    private const string CandidateListRequiredCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_LIST_REQUIRED";
    private const string CandidateRequiredCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_REQUIRED";
    private const string CandidateIdRequiredCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_ID_REQUIRED";
    private const string DuplicateCandidateIdCode = "DYNAMIC_GRAPH_NODE_DUPLICATE_CANDIDATE_ID";
    private const string CandidateUnitMismatchCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_UNIT_MISMATCH";
    private const string CandidateUnitIdsInvalidCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_UNIT_IDS_INVALID";
    private const string CandidateLogicalUnitRefRequiredCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_LOGICAL_UNIT_REF_REQUIRED";
    private const string CandidateOutputRefsInvalidCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_OUTPUT_REFS_INVALID";
    private const string CandidateOutputTypeMismatchCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_OUTPUT_TYPE_MISMATCH";
    private const string CandidateConfidenceInvalidCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_CONFIDENCE_INVALID";
    private const string CandidateScoreInvalidCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_SCORE_INVALID";
    private const string CandidateProvenanceInvalidCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_PROVENANCE_INVALID";
    private const string CandidatePathOrderRequiredCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_PATH_ORDER_REQUIRED";
    private const string CandidateLegalStatusRequiredCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_LEGAL_STATUS_REQUIRED";
    private const string CandidateStatusRequiredCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_STATUS_REQUIRED";
    private const string CandidateErrorCodesInvalidCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_ERROR_CODES_INVALID";
    private const string CandidateSourceIdsInvalidCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_SOURCE_IDS_INVALID";
    private const string CandidateTraceRefRequiredCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_TRACE_REF_REQUIRED";
    private const string CandidateTraceRefsInvalidCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_TRACE_REFS_INVALID";
    private const string CandidateEvidenceInvalidCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_EVIDENCE_INVALID";
    private const string CandidateHandoffRefRequiredCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_HANDOFF_REF_REQUIRED";
    private const string CandidateSemanticClaimForbiddenCode = "DYNAMIC_GRAPH_NODE_CANDIDATE_SEMANTIC_CLAIM_FORBIDDEN";
    private const string DomainMismatchWarning = "DYNAMIC_GRAPH_DOMAIN_MISMATCH";

    private readonly DynamicGraphNodeDescriptor? _descriptor;
    private readonly ILogicalUnit<TInput, TOutput>? _logicalUnit;
    private readonly Func<LogicalUnitRequest<TInput>, LogicalUnitResult<TOutput>,
        OperationResult<IReadOnlyList<GraphNodeCandidate>>>? _candidateProjection;
    private readonly OperationResult<bool> _constructorValidation;

    // The constructor snapshots the descriptor and validates all immutable adapter dependencies once.
    public LogicalUnitDynamicGraphNode(
        DynamicGraphNodeDescriptor descriptor,
        ILogicalUnit<TInput, TOutput> logicalUnit,
        Func<LogicalUnitResult<TOutput>, OperationResult<IReadOnlyList<GraphNodeCandidate>>> candidateProjection)
    {
        _descriptor = descriptor is null ? null : descriptor with { };
        _logicalUnit = logicalUnit;
        _candidateProjection = candidateProjection is null
            ? null
            : (_, validatedResult) => candidateProjection(validatedResult);
        _constructorValidation = ValidateConstructorState(_descriptor, _logicalUnit, _candidateProjection);
    }

    // Create a contextual adapter without adding a second externally callable constructor overload.
    internal static LogicalUnitDynamicGraphNode<TInput, TOutput> CreateContextual(
        DynamicGraphNodeDescriptor descriptor,
        ILogicalUnit<TInput, TOutput> logicalUnit,
        Func<LogicalUnitRequest<TInput>, LogicalUnitResult<TOutput>,
            OperationResult<IReadOnlyList<GraphNodeCandidate>>> candidateProjection)
        => new(descriptor, logicalUnit, candidateProjection);

    private LogicalUnitDynamicGraphNode(
        DynamicGraphNodeDescriptor descriptor,
        ILogicalUnit<TInput, TOutput> logicalUnit,
        Func<LogicalUnitRequest<TInput>, LogicalUnitResult<TOutput>,
            OperationResult<IReadOnlyList<GraphNodeCandidate>>> contextualCandidateProjection)
    {
        _descriptor = descriptor is null ? null : descriptor with { };
        _logicalUnit = logicalUnit;
        _candidateProjection = contextualCandidateProjection;
        _constructorValidation = ValidateConstructorState(_descriptor, _logicalUnit, _candidateProjection);
    }

    // NodeId is the immutable descriptor identity and never comes from mutable runtime request state.
    public string NodeId => _descriptor?.NodeId ?? string.Empty;

    // Descriptor returns the immutable constructor snapshot used by execution validation.
    public DynamicGraphNodeDescriptor Descriptor => _descriptor!;

    // ExecuteAsync delegates inference once and projects its typed result once into the canonical queryable shape.
    public async Task<OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>> ExecuteAsync(
        LogicalUnitRequest<TInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        if (!_constructorValidation.IsSuccess)
            return Failure(_constructorValidation.ErrorMessage, _constructorValidation.ErrorCode);

        var requestValidation = ValidateRequest(request, NodeId);
        if (!requestValidation.IsSuccess)
            return Failure(requestValidation.ErrorMessage, requestValidation.ErrorCode);

        var descriptor = _descriptor!;
        if (!StringComparer.Ordinal.Equals(descriptor.DomainTag, request.DomainTag)
            && !StringComparer.Ordinal.Equals(descriptor.DomainTag, DynamicGraphValidation.DomainAgnosticTag))
        {
            return OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>.Ok(
                CreateDomainMismatchResult(request));
        }

        OperationResult<LogicalUnitResult<TOutput>>? unitExecution;
        try
        {
            var unitTask = _logicalUnit!.ExecuteAsync(request, cancellationToken);
            if (unitTask is null)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Failure("Cancelled.", CancelledCode);

                return Failure("Logical-unit execution task is required.", UnitTaskRequiredCode);
            }

            unitExecution = await unitTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failure("Cancelled.", CancelledCode);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return cancellationToken.IsCancellationRequested
                ? Failure("Cancelled.", CancelledCode)
                : Failure("Logical-unit execution failed unexpectedly.", UnitExceptionCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        if (unitExecution is null)
            return Failure("Logical-unit execution result is required.", UnitResultRequiredCode);

        if (!unitExecution.IsSuccess)
            return Failure(unitExecution.ErrorMessage, unitExecution.ErrorCode);

        var unitResult = unitExecution.Value;
        if (unitResult is null)
            return Failure("Successful logical-unit result is required.", UnitResultRequiredCode);

        var unitResultValidation = ValidateUnitResult(request, unitResult);
        if (!unitResultValidation.IsSuccess)
            return Failure(unitResultValidation.ErrorMessage, unitResultValidation.ErrorCode);

        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        OperationResult<IReadOnlyList<GraphNodeCandidate>>? projection;
        try
        {
            projection = _candidateProjection!(request, unitResult);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return cancellationToken.IsCancellationRequested
                ? Failure("Cancelled.", CancelledCode)
                : Failure("Graph-node candidate projection failed unexpectedly.", ProjectionExceptionCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        if (projection is null)
            return Failure("Graph-node candidate projection result is required.", ProjectionResultRequiredCode);

        if (!projection.IsSuccess)
            return Failure(projection.ErrorMessage, projection.ErrorCode);

        var projectedCandidates = projection.Value;
        if (projectedCandidates is null)
            return Failure("Graph-node candidate list is required.", CandidateListRequiredCode);

        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        GraphNodeCandidate[] candidates;
        try
        {
            // This is the single materialization boundary; order is retained exactly as projected.
            candidates = projectedCandidates.Select(SnapshotCandidate).ToArray();
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return cancellationToken.IsCancellationRequested
                ? Failure("Cancelled.", CancelledCode)
                : Failure("Graph-node candidate projection failed unexpectedly.", ProjectionExceptionCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        var candidatesValidation = ValidateCandidates(candidates, request.UnitId, descriptor);
        if (!candidatesValidation.IsSuccess)
            return Failure(candidatesValidation.ErrorMessage, candidatesValidation.ErrorCode);

        var result = new LogicalUnitResult<IQueryable<GraphNodeCandidate>>(
            unitResult.UnitId,
            unitResult.UnitRole,
            unitResult.DomainTag,
            candidates.AsQueryable(),
            unitResult.Confidence,
            unitResult.PredicateIds.ToArray(),
            unitResult.DomainTags.ToArray(),
            unitResult.EvidenceIds.ToArray(),
            unitResult.Warnings.ToArray())
        {
            TransitionSignal = CopyTransitionSignal(unitResult.TransitionSignal)
        };

        return OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>.Ok(result);
    }

    private static OperationResult<bool> ValidateConstructorState(
        DynamicGraphNodeDescriptor? descriptor,
        ILogicalUnit<TInput, TOutput>? logicalUnit,
        Func<LogicalUnitRequest<TInput>, LogicalUnitResult<TOutput>,
            OperationResult<IReadOnlyList<GraphNodeCandidate>>>? candidateProjection)
    {
        if (descriptor is null)
            return OperationResult<bool>.Fail("Dynamic graph node descriptor is required.", DescriptorRequiredCode);

        if (logicalUnit is null)
            return OperationResult<bool>.Fail("Logical unit is required.", UnitRequiredCode);

        if (candidateProjection is null)
            return OperationResult<bool>.Fail("Candidate projection is required.", ProjectionRequiredCode);

        var validationDomain = string.IsNullOrWhiteSpace(descriptor.DomainTag)
            || StringComparer.Ordinal.Equals(descriptor.DomainTag, DynamicGraphValidation.DomainAgnosticTag)
                ? "__dynamic_graph_descriptor_validation__"
                : descriptor.DomainTag;

        var descriptorProbe = new DescriptorValidationNode(descriptor.NodeId, descriptor);
        var topology = new DynamicGraphTopology(
            "__dynamic_graph_descriptor_validation__",
            validationDomain,
            "__dynamic_graph_descriptor_validation__",
            [descriptorProbe],
            [],
            [new DynamicGraphNodeOrder(
                "__dynamic_graph_descriptor_validation__",
                descriptor.NodeId,
                "__dynamic_graph_descriptor_validation__",
                validationDomain,
                "__dynamic_graph_descriptor_validation__",
                0)]);

        var descriptorValidation = DynamicGraphValidation.Validate(topology);
        if (!descriptorValidation.IsSuccess)
            return OperationResult<bool>.Fail(descriptorValidation.ErrorMessage, descriptorValidation.ErrorCode);

        if (!StringComparer.Ordinal.Equals(descriptor.TInputTypeName, typeof(TInput).Name))
        {
            return OperationResult<bool>.Fail(
                $"Dynamic graph node '{descriptor.NodeId}' input type does not match '{typeof(TInput).Name}'.",
                InputTypeMismatchCode);
        }

        if (!StringComparer.Ordinal.Equals(descriptor.TOutputTypeName, typeof(TOutput).Name))
        {
            return OperationResult<bool>.Fail(
                $"Dynamic graph node '{descriptor.NodeId}' output type does not match '{typeof(TOutput).Name}'.",
                OutputTypeMismatchCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateRequest(LogicalUnitRequest<TInput>? request, string nodeId)
    {
        if (request is null)
            return OperationResult<bool>.Fail("Dynamic graph node request is required.", RequestRequiredCode);
        if (string.IsNullOrWhiteSpace(request.RunId))
            return OperationResult<bool>.Fail("Dynamic graph node RunId is required.", RunIdRequiredCode);
        if (string.IsNullOrWhiteSpace(request.UnitId))
            return OperationResult<bool>.Fail("Dynamic graph node UnitId is required.", UnitIdRequiredCode);
        if (string.IsNullOrWhiteSpace(request.UnitRole))
            return OperationResult<bool>.Fail("Dynamic graph node UnitRole is required.", UnitRoleRequiredCode);
        if (string.IsNullOrWhiteSpace(request.DomainTag))
            return OperationResult<bool>.Fail("Dynamic graph node request domain is required.", DomainRequiredCode);
        if (request.Input is null)
            return OperationResult<bool>.Fail("Dynamic graph node input is required.", InputRequiredCode);
        if (request.Flags is null)
            return OperationResult<bool>.Fail("Dynamic graph node execution flags are required.", FlagsRequiredCode);
        if (request.Metadata is null)
            return OperationResult<bool>.Fail("Dynamic graph node metadata is required.", MetadataRequiredCode);
        if (!StringComparer.Ordinal.Equals(request.UnitId, nodeId))
            return OperationResult<bool>.Fail("Dynamic graph node request UnitId does not match NodeId.", RequestUnitMismatchCode);

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateUnitResult(
        LogicalUnitRequest<TInput> request,
        LogicalUnitResult<TOutput> result)
    {
        if (!StringComparer.Ordinal.Equals(result.UnitId, request.UnitId))
            return OperationResult<bool>.Fail("Logical-unit result UnitId drifted from the request.", ResultUnitMismatchCode);
        if (!StringComparer.Ordinal.Equals(result.UnitRole, request.UnitRole))
            return OperationResult<bool>.Fail("Logical-unit result UnitRole drifted from the request.", ResultRoleMismatchCode);
        if (!StringComparer.Ordinal.Equals(result.DomainTag, request.DomainTag))
            return OperationResult<bool>.Fail("Logical-unit result DomainTag drifted from the request.", ResultDomainMismatchCode);
        if (!IsUnitInterval(result.Confidence))
            return OperationResult<bool>.Fail("Logical-unit result confidence must be finite and within [0,1].", ResultConfidenceInvalidCode);
        if (result.PredicateIds is null
            || result.DomainTags is null
            || result.EvidenceIds is null
            || result.Warnings is null
            || (result.TransitionSignal is not null && result.TransitionSignal.EvidenceIds is null))
        {
            return OperationResult<bool>.Fail("Logical-unit result metadata carriers are required.", ResultMetadataInvalidCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateCandidates(
        IReadOnlyList<GraphNodeCandidate> candidates,
        string expectedUnitId,
        DynamicGraphNodeDescriptor descriptor)
    {
        var candidateIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var candidate in candidates)
        {
            if (candidate is null)
                return OperationResult<bool>.Fail("Projected graph-node candidate is required.", CandidateRequiredCode);
            if (string.IsNullOrWhiteSpace(candidate.CandidateId))
                return OperationResult<bool>.Fail("Graph-node candidate id is required.", CandidateIdRequiredCode);
            if (!candidateIds.Add(candidate.CandidateId))
                return OperationResult<bool>.Fail("Graph-node candidate ids must be unique.", DuplicateCandidateIdCode);
            if (!StringComparer.Ordinal.Equals(candidate.UnitId, expectedUnitId))
                return OperationResult<bool>.Fail("Graph-node candidate UnitId does not match the request.", CandidateUnitMismatchCode);
            if (!AreRequiredDistinctRefsValid(candidate.UnitIds)
                || !candidate.UnitIds.Contains(candidate.UnitId, StringComparer.Ordinal))
            {
                return OperationResult<bool>.Fail("Graph-node candidate UnitIds are invalid.", CandidateUnitIdsInvalidCode);
            }
            if (string.IsNullOrWhiteSpace(candidate.LogicalUnitRef))
                return OperationResult<bool>.Fail("Graph-node candidate LogicalUnitRef is required.", CandidateLogicalUnitRefRequiredCode);
            if (!AreRequiredDistinctRefsValid(candidate.OutputRefs))
                return OperationResult<bool>.Fail("Graph-node candidate OutputRefs are invalid.", CandidateOutputRefsInvalidCode);
            if (!StringComparer.Ordinal.Equals(candidate.OutputType, typeof(TOutput).Name))
                return OperationResult<bool>.Fail("Graph-node candidate output type does not match the requested type.", CandidateOutputTypeMismatchCode);
            if (!IsUnitInterval(candidate.Confidence))
                return OperationResult<bool>.Fail("Graph-node candidate confidence must be finite and within [0,1].", CandidateConfidenceInvalidCode);
            if (candidate.Score is { } score && !double.IsFinite(score))
                return OperationResult<bool>.Fail("Graph-node candidate score must be finite when present.", CandidateScoreInvalidCode);
            if (!AreRequiredDistinctRefsValid(candidate.ProvenanceRefs))
                return OperationResult<bool>.Fail("Graph-node candidate ProvenanceRefs are invalid.", CandidateProvenanceInvalidCode);
            if (string.IsNullOrWhiteSpace(candidate.PathOrderRef))
                return OperationResult<bool>.Fail("Graph-node candidate PathOrderRef is required.", CandidatePathOrderRequiredCode);
            if (string.IsNullOrWhiteSpace(candidate.LegalStatus))
                return OperationResult<bool>.Fail("Graph-node candidate LegalStatus is required.", CandidateLegalStatusRequiredCode);
            if (string.IsNullOrWhiteSpace(candidate.Status))
                return OperationResult<bool>.Fail("Graph-node candidate Status is required.", CandidateStatusRequiredCode);
            if (!AreOptionalDistinctRefsValid(candidate.ErrorCodes))
                return OperationResult<bool>.Fail("Graph-node candidate ErrorCodes are invalid.", CandidateErrorCodesInvalidCode);
            if (!AreRequiredDistinctRefsValid(candidate.SourceIds))
                return OperationResult<bool>.Fail("Graph-node candidate SourceIds are invalid.", CandidateSourceIdsInvalidCode);
            if (string.IsNullOrWhiteSpace(candidate.TraceRef))
                return OperationResult<bool>.Fail("Graph-node candidate TraceRef is required.", CandidateTraceRefRequiredCode);
            if (!AreRequiredDistinctRefsValid(candidate.TraceRefs)
                || !candidate.TraceRefs.Contains(candidate.TraceRef, StringComparer.Ordinal))
            {
                return OperationResult<bool>.Fail("Graph-node candidate TraceRefs are invalid.", CandidateTraceRefsInvalidCode);
            }
            if (!AreRequiredDistinctRefsValid(candidate.EvidenceIds))
                return OperationResult<bool>.Fail("Graph-node candidate EvidenceIds are invalid.", CandidateEvidenceInvalidCode);
            if (string.IsNullOrWhiteSpace(candidate.B1TeacherJudgeRef)
                || string.IsNullOrWhiteSpace(candidate.B7GraphOrderRef)
                || string.IsNullOrWhiteSpace(candidate.B11AsyncBatchRef)
                || string.IsNullOrWhiteSpace(candidate.B12WinnerSelectionRef)
                || string.IsNullOrWhiteSpace(candidate.B6UnitDpoBuilderRef))
            {
                return OperationResult<bool>.Fail("Graph-node candidate handoff references are required.", CandidateHandoffRefRequiredCode);
            }
            if (candidate.SemanticQualityClaimed && !descriptor.RuntimeSemanticQualityClaimed)
            {
                return OperationResult<bool>.Fail(
                    "Graph-node candidate cannot claim semantic quality when its descriptor does not.",
                    CandidateSemanticClaimForbiddenCode);
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    // Own every mutable candidate carrier while preserving malformed nulls for the existing exact validators.
    private static GraphNodeCandidate SnapshotCandidate(GraphNodeCandidate candidate)
        => candidate is null
            ? null!
            : candidate with
            {
                UnitIds = candidate.UnitIds is null ? null! : candidate.UnitIds.ToArray(),
                OutputRefs = candidate.OutputRefs is null ? null! : candidate.OutputRefs.ToArray(),
                ProvenanceRefs = candidate.ProvenanceRefs is null ? null! : candidate.ProvenanceRefs.ToArray(),
                ErrorCodes = candidate.ErrorCodes is null ? null! : candidate.ErrorCodes.ToArray(),
                SourceIds = candidate.SourceIds is null ? null! : candidate.SourceIds.ToArray(),
                TraceRefs = candidate.TraceRefs is null ? null! : candidate.TraceRefs.ToArray(),
                EvidenceIds = candidate.EvidenceIds is null ? null! : candidate.EvidenceIds.ToArray()
            };

    private static LogicalUnitResult<IQueryable<GraphNodeCandidate>> CreateDomainMismatchResult(
        LogicalUnitRequest<TInput> request)
        => new(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            Array.Empty<GraphNodeCandidate>().AsQueryable(),
            0.0,
            [],
            [request.DomainTag],
            [],
            [DomainMismatchWarning]);

    private static PhaseTransitionSignal? CopyTransitionSignal(PhaseTransitionSignal? signal)
        => signal is null
            ? null
            : signal with { EvidenceIds = signal.EvidenceIds.ToArray() };

    private static bool AreRequiredDistinctRefsValid(string[]? refs)
        => refs is { Length: > 0 }
           && refs.All(value => !string.IsNullOrWhiteSpace(value))
           && refs.Distinct(StringComparer.Ordinal).Count() == refs.Length;

    private static bool AreOptionalDistinctRefsValid(string[]? refs)
        => refs is not null
           && refs.All(value => !string.IsNullOrWhiteSpace(value))
           && refs.Distinct(StringComparer.Ordinal).Count() == refs.Length;

    private static bool IsUnitInterval(double value)
        => double.IsFinite(value) && value is >= 0.0 and <= 1.0;

    private static bool IsNonFatalException(Exception exception)
        => exception is not OutOfMemoryException
           && exception is not StackOverflowException
           && exception is not AccessViolationException
           && exception is not AppDomainUnloadedException
           && exception is not BadImageFormatException
           && !StringComparer.Ordinal.Equals(
               exception.GetType().FullName,
               "System.Threading.ThreadAbortException");

    private static OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>> Failure(
        string message,
        string code)
        => OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>.Fail(message, code);

    private sealed record DescriptorValidationNode(
        string NodeId,
        DynamicGraphNodeDescriptor Descriptor) : IDynamicGraphNode;
}

// Named public construction keeps contextual projection explicit and the legacy null constructor call unambiguous.
public static class LogicalUnitDynamicGraphNodeFactory
{
    public static LogicalUnitDynamicGraphNode<TInput, TOutput> CreateContextual<TInput, TOutput>(
        DynamicGraphNodeDescriptor descriptor,
        ILogicalUnit<TInput, TOutput> logicalUnit,
        Func<LogicalUnitRequest<TInput>, LogicalUnitResult<TOutput>,
            OperationResult<IReadOnlyList<GraphNodeCandidate>>> candidateProjection)
        => LogicalUnitDynamicGraphNode<TInput, TOutput>.CreateContextual(
            descriptor,
            logicalUnit,
            candidateProjection);
}
