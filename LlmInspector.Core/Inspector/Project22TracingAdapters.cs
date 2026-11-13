// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Inspector;

using AgentAlgorithms;

// RouteClassifierInspectionInput makes one route-classification prompt visible to the inspector.
public sealed record RouteClassifierInspectionInput(string SemanticTokens);

// SkillFilterInspectionInput makes one skill-filter call visible without serialising hidden search state.
public sealed record SkillFilterInspectionInput(
    IReadOnlyList<TextChunk> SkillCandidates,
    string TaskDescription,
    int TopK,
    double ConfidenceThreshold);

// ResultExaminationInspectionInput makes one internal result-examination call visible.
public sealed record ResultExaminationInspectionInput(
    string OriginalRequest,
    string InternalResponse,
    string[] RequiredPredicates,
    string[] ArbiterConstraints,
    double ConfidenceThreshold);

// ExternalAiRequestPackageInspection wraps the package and the exact approval prompt text.
public sealed record ExternalAiRequestPackageInspection(
    ExternalAiRequestPackage Package,
    string ApprovalPrompt);

// ExternalAiAdaptInspectionInput makes one external-answer adaptation call visible.
public sealed record ExternalAiAdaptInspectionInput(
    ExternalAiRequestPackage Package,
    string ExternalAnswer,
    string Domain);

// TracingRouteClassifier emits station traces around the project 22 RouteClassifier.
public sealed class TracingRouteClassifier
{
    // The wrapped classifier remains the source of routing behavior.
    private readonly IRouteClassifier _inner;

    // This callback sends start and completion traces to the inspector session.
    private readonly Action<StationTrace> _emitTrace;

    // Create a tracing adapter around the real project 22 route classifier.
    public TracingRouteClassifier(IRouteClassifier inner, Action<StationTrace> emitTrace)
    {
        // Store the wrapped classifier.
        _inner = inner;

        // Store the trace callback.
        _emitTrace = emitTrace;
    }

    // Classify emits traces around one RouteClassifier.Classify call.
    public OperationResult<DomainClassification> Classify(string semanticTokens)
    {
        // Build the inspectable input object.
        var input = new RouteClassifierInspectionInput(semanticTokens);

        // Create the start trace from the current flow context.
        var start = Project22TraceFactory.CreateStartTrace("RouteClassifier.Classify", "project22_route_classifier", StationKind.Unit, input);

        // Emit the start trace before calling the wrapped classifier.
        _emitTrace(start);

        // Call the real project 22 route classifier.
        var result = _inner.Classify(semanticTokens);

        // Emit the completion trace with the real classifier result.
        _emitTrace(Project22TraceFactory.CompleteTrace(start, result));

        // Return the wrapped result unchanged.
        return result;
    }
}

// TracingKnnSkillFilter emits station traces around the project 22 KNN skill filter.
public sealed class TracingKnnSkillFilter
{
    // The wrapped filter remains the source of skill-selection behavior.
    private readonly ISkillFilter _inner;

    // This callback sends start and completion traces to the inspector session.
    private readonly Action<StationTrace> _emitTrace;

    // Create a tracing adapter around the real project 22 KNN skill filter.
    public TracingKnnSkillFilter(ISkillFilter inner, Action<StationTrace> emitTrace)
    {
        // Store the wrapped skill filter.
        _inner = inner;

        // Store the trace callback.
        _emitTrace = emitTrace;
    }

    // FilterAsync emits traces around one KnnSkillFilter.FilterAsync call.
    public async Task<OperationResult<IReadOnlyList<TextChunk>>> FilterAsync(
        IReadOnlyList<TextChunk> skillCandidates,
        string taskDescription,
        int topK = 5,
        double confidenceThreshold = 0.40)
    {
        // Build the inspectable input object.
        var input = new SkillFilterInspectionInput(skillCandidates, taskDescription, topK, confidenceThreshold);

        // Create the start trace from the current flow context.
        var start = Project22TraceFactory.CreateStartTrace("KnnSkillFilter.FilterAsync", "project22_knn_skill_filter", StationKind.Unit, input);

        // Emit the start trace before calling the wrapped filter.
        _emitTrace(start);

        // Call the real project 22 KNN skill filter.
        var result = await _inner.FilterAsync(skillCandidates, taskDescription, topK, confidenceThreshold);

        // Emit the completion trace with the real filter result.
        _emitTrace(Project22TraceFactory.CompleteTrace(start, result));

        // Return the wrapped result unchanged.
        return result;
    }
}

// TracingInternalResultExaminer emits station traces around the project 22 result examiner.
public sealed class TracingInternalResultExaminer
{
    // The wrapped examiner remains the source of confidence and gap behavior.
    private readonly InternalResultExaminer _inner;

    // This callback sends start and completion traces to the inspector session.
    private readonly Action<StationTrace> _emitTrace;

    // Create a tracing adapter around the real internal result examiner.
    public TracingInternalResultExaminer(InternalResultExaminer inner, Action<StationTrace> emitTrace)
    {
        // Store the wrapped examiner.
        _inner = inner;

        // Store the trace callback.
        _emitTrace = emitTrace;
    }

    // Examine emits traces around one InternalResultExaminer.Examine call.
    public OperationResult<ResultExamination> Examine(
        string originalRequest,
        string internalResponse,
        string[] requiredPredicates,
        string[] arbiterConstraints,
        double confidenceThreshold = 0.50)
    {
        // Build the inspectable input object.
        var input = new ResultExaminationInspectionInput(originalRequest, internalResponse, requiredPredicates, arbiterConstraints, confidenceThreshold);

        // Create the start trace from the current flow context.
        var start = Project22TraceFactory.CreateStartTrace("InternalResultExaminer.Examine", "project22_internal_result_examiner", StationKind.Unit, input);

        // Emit the start trace before calling the wrapped examiner.
        _emitTrace(start);

        // Call the real project 22 internal result examiner.
        var result = _inner.Examine(originalRequest, internalResponse, requiredPredicates, arbiterConstraints, confidenceThreshold);

        // Emit the completion trace with the real examination result.
        _emitTrace(Project22TraceFactory.CompleteTrace(start, result));

        // Return the wrapped result unchanged.
        return result;
    }
}

// TracingExternalAiRequestPackageFactory emits traces around approval-package construction.
public sealed class TracingExternalAiRequestPackageFactory
{
    // This callback sends start and completion traces to the inspector session.
    private readonly Action<StationTrace> _emitTrace;

    // Create a tracing factory for project 22 external-AI request packages.
    public TracingExternalAiRequestPackageFactory(Action<StationTrace> emitTrace)
    {
        // Store the trace callback.
        _emitTrace = emitTrace;
    }

    // Create emits traces around one approval package construction.
    public OperationResult<ExternalAiRequestPackageInspection> Create(ExternalAiRequestPackage package)
    {
        // Create the start trace from the current flow context.
        var start = Project22TraceFactory.CreateStartTrace("ExternalAiRequestPackage.Create", "project22_external_ai_request_package", StationKind.Unit, package);

        // Emit the start trace before producing the approval text.
        _emitTrace(start);

        // Build the exact approval prompt text from the package.
        var inspection = new ExternalAiRequestPackageInspection(package, package.ToApprovalPrompt());

        // Wrap the approval package and prompt in the project 22 result envelope.
        var result = OperationResult<ExternalAiRequestPackageInspection>.Ok(inspection);

        // Emit the completion trace with package and prompt visible.
        _emitTrace(Project22TraceFactory.CompleteTrace(start, result));

        // Return the inspection payload unchanged.
        return result;
    }
}

// TracingExternalAiResultAdapter emits station traces around the project 22 external result adapter.
public sealed class TracingExternalAiResultAdapter
{
    // The wrapped adapter remains the source of arbiter-aware adaptation behavior.
    private readonly ExternalAiResultAdapter _inner;

    // This callback sends start and completion traces to the inspector session.
    private readonly Action<StationTrace> _emitTrace;

    // Create a tracing adapter around the real external result adapter.
    public TracingExternalAiResultAdapter(ExternalAiResultAdapter inner, Action<StationTrace> emitTrace)
    {
        // Store the wrapped adapter.
        _inner = inner;

        // Store the trace callback.
        _emitTrace = emitTrace;
    }

    // Adapt emits traces around one ExternalAiResultAdapter.Adapt call.
    public OperationResult<PreprocessedLearningData> Adapt(
        ExternalAiRequestPackage package,
        string externalAnswer,
        string domain)
    {
        // Build the inspectable input object.
        var input = new ExternalAiAdaptInspectionInput(package, externalAnswer, domain);

        // Create the start trace from the current flow context.
        var start = Project22TraceFactory.CreateStartTrace("ExternalAiResultAdapter.Adapt", "project22_external_result_adapter_arbiter", StationKind.Unit, input);

        // Emit the start trace before calling the wrapped adapter.
        _emitTrace(start);

        // Call the real project 22 external result adapter.
        var result = _inner.Adapt(package, externalAnswer, domain);

        // Emit the completion trace with the real adaptation result.
        _emitTrace(Project22TraceFactory.CompleteTrace(start, result));

        // Return the wrapped result unchanged.
        return result;
    }
}

// DeterministicProject22SearchProvider supplies cheap local scores for the inspector sample.
internal sealed class DeterministicProject22SearchProvider : ISemanticSearchProvider
{
    // SearchAsync returns stable scores so the inspector sample never calls an external model.
    public Task<OperationResult<List<(string ChunkId, double Score)>>> SearchAsync(string query, int topK)
    {
        // Build the fixed score list in descending order for the KNN filter.
        var scores = new List<(string ChunkId, double Score)>
        {
            // The bug-fix skill should clearly win for the sample prompt.
            ("bug-fix-skill", 0.91),

            // The crash-diagnostics skill should also remain inside the top-K set.
            ("crash-diagnostics-skill", 0.63),

            // The code-question skill remains lower so the filter demonstrates selection.
            ("code-question-skill", 0.12)
        };

        // Return the requested number of scores through the project 22 result envelope.
        return Task.FromResult(OperationResult<List<(string ChunkId, double Score)>>.Ok(scores.Take(topK).ToList()));
    }
}

// Project22TraceFactory centralizes start and completion trace construction for these adapters.
internal static class Project22TraceFactory
{
    // CreateStartTrace creates one project 22 station-style trace.
    public static StationTrace CreateStartTrace(string stationName, string stationRole, StationKind kind, object inputValue)
    {
        // Read the current parent context.
        var context = TraceContextScope.Current;

        // Use the inherited run id or create a direct-call run id.
        var runId = context?.RunId ?? Guid.NewGuid().ToString("N");

        // Use the inherited parent trace id so the station attaches below its flow.
        var parentTraceId = context?.ParentTraceId;

        // Use the inherited depth or root depth for direct calls.
        var depth = context?.Depth ?? 0;

        // Use the inherited wave index for sibling layout.
        var waveIndex = context?.WaveIndex ?? 0;

        // Create a fresh trace id for this station.
        var traceId = Guid.NewGuid().ToString("N");

        // Serialise the station input for the inspector panel.
        var input = PropertyNodeSerialiser.Serialise(inputValue);

        // Return the in-flight station trace.
        return new StationTrace(
            traceId,
            parentTraceId,
            runId,
            depth,
            waveIndex,
            kind,
            stationName,
            stationRole,
            input,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            null,
            StationResourceSampler.Capture());
    }

    // CompleteTrace attaches a project 22 operation result to the matching trace.
    public static StationTrace CompleteTrace<T>(StationTrace start, OperationResult<T> result)
    {
        // Serialise the operation result envelope.
        var output = PropertyNodeSerialiser.Serialise(result);

        // Return the completion event with the same trace identity.
        return start with
        {
            Output = output,
            ErrorCode = result.IsSuccess ? null : result.ErrorCode,
            ErrorMessage = result.IsSuccess ? null : result.ErrorMessage,
            CompletedAt = DateTimeOffset.UtcNow,
            Resources = StationResourceSampler.Capture()
        };
    }
}
