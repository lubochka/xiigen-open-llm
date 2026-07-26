// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed record DesignConversionInternalEvaluateRequest(
    string JobId,
    string RequestId,
    DesignConversionSourceKind SourceKind,
    string Html,
    string Css,
    DesignTreePayload DesignTreePayload,
    DesignTreePayload OriginalTreePayload,
    IReadOnlyList<DesignRenderArtifact> Screenshots,
    IReadOnlyList<DesignRenderArtifact> DesignRenders,
    DesignConversionBudgetMetadata Budget,
    IReadOnlyList<RenderQaResolution> RenderResolutions,
    IReadOnlyList<string> AiTeacherEvidenceRefs,
    IReadOnlyDictionary<string, string> DeterministicEvidenceRefs,
    IReadOnlyDictionary<string, string> Provenance);

public sealed record DesignConversionInternalEvaluationResult(
    string JobId,
    SubmittedDesignConversionRequest SubmittedRequest,
    IReadOnlyList<DesignConversionRenderResult> RenderResults,
    IReadOnlyList<DesignConversionQaComparisonArtifact> ComparisonArtifacts,
    IReadOnlyList<DesignConversionVisualJudgeResult> VisualJudgeResults,
    IReadOnlyList<RenderQaResultRecord> QaResults,
    DesignConversionRuntimeFlowResult RuntimeFlowResult,
    VisualVerdict Verdict,
    string ImprovedHtml,
    string ImprovedCss,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> ScaffoldMarkers,
    bool DeterministicLogicClaimedAsLearning,
    DesignConversionVisualQaBoundaryTrace VisualQaBoundaryTrace)
{
    public IReadOnlyList<DesignConversionViewportFailure> ViewportFailures { get; init; } = [];
    public DesignConversionVisualCostAccounting VisualCostAccounting { get; init; } =
        DesignConversionVisualCostAccounting.Unavailable;
}

public sealed record DesignConversionVisualCostAccounting(
    decimal TotalCostUsd,
    bool MeasurementComplete,
    int MissingMeasurementCount,
    int EvaluationAttemptCount,
    string CostSource)
{
    public const string UnknownCostSource = "unknown";
    public const string MixedCostSource = "mixed";

    public static DesignConversionVisualCostAccounting Unavailable { get; } = new(
        TotalCostUsd: 0m,
        MeasurementComplete: false,
        MissingMeasurementCount: 1,
        EvaluationAttemptCount: 0,
        CostSource: UnknownCostSource);

    public static DesignConversionVisualCostAccounting FromTierPolicy(
        DesignConversionTierPolicyDecision policy,
        int evaluationAttemptCount)
        => new(
            TotalCostUsd: policy.EstimatedCostUsd,
            MeasurementComplete: policy.ExternalCostMeasurementComplete,
            MissingMeasurementCount: policy.MissingExternalCostMeasurementCount,
            EvaluationAttemptCount: evaluationAttemptCount,
            CostSource: string.IsNullOrWhiteSpace(policy.CostSource)
                ? UnknownCostSource
                : policy.CostSource);
}

public sealed record DesignConversionViewportFailure(
    string ViewportId,
    int Width,
    int Height,
    double DeviceScaleFactor,
    int OneBasedIndex,
    string Stage,
    string ErrorCode,
    IReadOnlyList<string> EvidenceRefs);

public interface IDesignConversionRenderProvider
{
    Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
        DesignConversionRenderRequest request,
        CancellationToken cancellationToken = default);
}

public interface IDesignConversionBoundedConcurrentRenderProvider
{
    int MaxConcurrentRenderOperations { get; }
}

public interface IDesignConversionComparisonProvider
{
    Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
        DesignConversionQaComparisonRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class DesignConversionInternalService
{
    public const string ServiceFirstMarker = "DESIGN_CONVERSION_INTERNAL_SERVICE_FIRST_NO_ASP_NET_HOST";
    public const string InjectedRendererMarker = "DESIGN_CONVERSION_RENDER_PROVIDER_INJECTED_FAKEABLE";
    public const string InjectedComparisonMarker = "DESIGN_CONVERSION_COMPARISON_PROVIDER_INJECTED_FAKEABLE";
    public const string InjectedVisualJudgeMarker = "DESIGN_CONVERSION_VISUAL_JUDGE_OPTIONAL_INJECTED_FAKEABLE";
    public const string NoLearningClaimMarker = "DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE";
    public const string VisualComparisonProofUnavailableStatus = "VISUAL_COMPARISON_PROOF_UNAVAILABLE";
    public const string ExpectedRenderResolutionMissingCode = "EXPECTED_RENDER_RESOLUTION_MISSING";
    public const string ImprovementCandidateBudgetRequiredCode = "IMPROVEMENT_CANDIDATE_BUDGET_REQUIRED";
    public const string NegativeBudgetUsdErrorCode = "IMPROVEMENT_LOOP_BUDGET_USD_NEGATIVE";
    public const string PayloadTooLargeCode = "DESIGN_CONVERSION_PAYLOAD_TOO_LARGE";
    public const string RawPayloadRequiredCode = "DESIGN_CONVERSION_RAW_PAYLOAD_REQUIRED";
    public const string ViewportOperationFailureMarker = "DESIGN_CONVERSION_VIEWPORT_OPERATION_FAILURE";
    public const string ViewportRenderFailedStatus = "VIEWPORT_RENDER_FAILED";
    public const string ViewportExpectedReferenceMissingStatus = "VIEWPORT_EXPECTED_REFERENCE_MISSING";
    public const string ViewportComparisonFailedStatus = "VIEWPORT_COMPARISON_FAILED";
    public const string ViewportVisualJudgeFailedStatus = "VIEWPORT_VISUAL_JUDGE_FAILED";
    private const int MaxBoundedConcurrentRenderWorkerCount = 4;
    public const int MaxRawPayloadChars = 262_144;

    private const string ViewportFailureStageRender = "render";
    private const string ViewportFailureStageExpectedReference = "expected_reference";
    private const string ViewportFailureStageComparison = "comparison";
    private const string ViewportFailureStageVisualJudge = "visual_judge";

    private readonly IDesignConversionRenderProvider _renderProvider;
    private readonly IDesignConversionComparisonProvider _comparisonProvider;
    private readonly IDesignConversionVisualJudge _visualJudge;
    private readonly DesignConversionRoutingFlow? _routingFlow;
    private readonly Task<OperationResult<ILogicalOrchestrator>>? _machineOrchestratorTask;

    public DesignConversionInternalService(
        IDesignConversionRenderProvider renderProvider,
        IDesignConversionComparisonProvider comparisonProvider,
        DesignConversionRoutingFlow? routingFlow)
        : this(renderProvider, comparisonProvider, visualJudge: null, routingFlow)
    {
    }

    public DesignConversionInternalService(
        IDesignConversionRenderProvider renderProvider,
        IDesignConversionComparisonProvider comparisonProvider,
        IDesignConversionVisualJudge? visualJudge = null,
        DesignConversionRoutingFlow? routingFlow = null)
    {
        _renderProvider = renderProvider ?? throw new ArgumentNullException(nameof(renderProvider));
        _comparisonProvider = comparisonProvider ?? throw new ArgumentNullException(nameof(comparisonProvider));
        _visualJudge = visualJudge ?? new DefaultLocalDesignConversionVisualJudge();
        _routingFlow = routingFlow ?? new DesignConversionRoutingFlow();
        _machineOrchestratorTask = null;
    }

    public DesignConversionInternalService(
        IDesignConversionRenderProvider renderProvider,
        IDesignConversionComparisonProvider comparisonProvider,
        IDesignConversionVisualJudge? visualJudge,
        DesignConversionMachine machine)
    {
        _renderProvider = renderProvider ?? throw new ArgumentNullException(nameof(renderProvider));
        _comparisonProvider = comparisonProvider ?? throw new ArgumentNullException(nameof(comparisonProvider));
        _visualJudge = visualJudge ?? new DefaultLocalDesignConversionVisualJudge();
        _routingFlow = null;
        _machineOrchestratorTask = (machine ?? throw new ArgumentNullException(nameof(machine)))
            .GetOrchestratorAsync(CancellationToken.None);
    }

    public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
        DesignConversionInternalEvaluateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                "DesignConversion internal evaluation cancelled.",
                "CANCELLED");

        var validation = Validate(request);
        if (!validation.IsSuccess)
            return validation.MapError<DesignConversionInternalEvaluationResult>();

        var submittedRequest = ToSubmittedRequest(request);
        var renderResults = new List<DesignConversionRenderResult>();
        var comparisonArtifacts = new List<DesignConversionQaComparisonArtifact>();
        var visualJudgeResults = new List<DesignConversionVisualJudgeResult>();
        var qaResults = new List<RenderQaResultRecord>();
        var viewportFailures = new List<DesignConversionViewportFailure>();

        var maxRuns = Math.Min(request.RenderResolutions.Count, request.Budget.MaxRenderQaRuns);
        var concurrentRenderOperations = await TryRunBoundedConcurrentRenderOperationsAsync(
            request,
            submittedRequest,
            maxRuns,
            cancellationToken);

        if (concurrentRenderOperations is not null)
        {
            foreach (var renderOperation in concurrentRenderOperations)
            {
                if (cancellationToken.IsCancellationRequested)
                    return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                        "DesignConversion internal evaluation cancelled.",
                        "CANCELLED");

                var processed = await ProcessViewportRenderResultAsync(
                    request,
                    submittedRequest,
                    renderOperation.Resolution,
                    renderOperation.Index,
                    renderOperation.Request,
                    renderOperation.Result,
                    renderResults,
                    comparisonArtifacts,
                    visualJudgeResults,
                    qaResults,
                    viewportFailures,
                    cancellationToken);
                if (!processed.IsSuccess)
                    return processed.MapError<DesignConversionInternalEvaluationResult>();
            }
        }

        for (var index = concurrentRenderOperations is null ? 0 : maxRuns; index < maxRuns; index++)
        {
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                    "DesignConversion internal evaluation cancelled.",
                    "CANCELLED");

            var resolution = request.RenderResolutions[index];
            var renderRequest = BuildRenderRequest(request, submittedRequest, resolution, index);
            var renderResult = await ExecuteViewportOperationAsync(
                () => _renderProvider.RenderAsync(renderRequest, cancellationToken),
                cancellationToken);

            var processed = await ProcessViewportRenderResultAsync(
                request,
                submittedRequest,
                resolution,
                index,
                renderRequest,
                renderResult,
                renderResults,
                comparisonArtifacts,
                visualJudgeResults,
                qaResults,
                viewportFailures,
                cancellationToken);
            if (!processed.IsSuccess)
                return processed.MapError<DesignConversionInternalEvaluationResult>();
        }

        var visualQaBoundaryTrace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            request.Budget,
            request.RenderResolutions,
            renderResults,
            visualJudgeResults,
            viewportFailures,
            ResolveImprovementAttemptCount(request));

        var deterministicEvidenceRefs = BuildDeterministicEvidenceRefs(request);
        var runtimeInput = new DesignConversionRuntimeFlowInput(
            Request: submittedRequest,
            QaResults: qaResults.ToArray(),
            ComparisonArtifacts: comparisonArtifacts.ToArray(),
            AiTeacherEvidenceRefs: request.AiTeacherEvidenceRefs.ToArray(),
            DeterministicEvidenceRefs: deterministicEvidenceRefs);

        var executionMetadata = BuildRuntimeExecutionMetadata();
        var flowEvaluation = await RunRuntimeFlowAsync(
            runtimeInput,
            request.JobId,
            new LogicalExecutionFlags(),
            executionMetadata,
            cancellationToken);
        if (!flowEvaluation.IsSuccess)
            return flowEvaluation.MapError<DesignConversionInternalEvaluationResult>();

        var value = flowEvaluation.Value!.FlowResult;
        if (viewportFailures.Count > 0)
            value = ApplyViewportFailureBoundary(value, request.JobId, viewportFailures);

        var evidenceRefs = renderResults
            .Select(result => $"render-result:{result.RenderResultId}")
            .Concat(comparisonArtifacts.Select(artifact => $"comparison-artifact:{artifact.ArtifactId}"))
            .Concat(visualJudgeResults.Select(result => $"visual-judge-result:{result.JudgeResultId}"))
            .Concat(visualJudgeResults.SelectMany(result => result.EvidenceRefs))
            .Concat(qaResults.Select(result => $"render-qa-result:{result.ResultId}"))
            .Concat(viewportFailures.SelectMany(failure => failure.EvidenceRefs))
            .Concat(visualQaBoundaryTrace.EvidenceRefs)
            .Concat(value.EvidenceRefs)
            .Append(ServiceFirstMarker)
            .Append(InjectedRendererMarker)
            .Append(InjectedComparisonMarker)
            .Append(InjectedVisualJudgeMarker)
            .Append(NoLearningClaimMarker)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var warnings = renderResults
            .SelectMany(result => result.Warnings)
            .Concat(visualJudgeResults
                .Where(result => result.IsBlocked)
                .Select(result => result.Status))
            .Concat(viewportFailures.Select(failure => ViewportFailureStatus(failure.Stage)))
            .Concat(visualQaBoundaryTrace.Warnings)
            .Concat(flowEvaluation.Value.Warnings)
            .Append("DESIGN_CONVERSION_INTERNAL_SERVICE_USES_INJECTED_RENDER_AND_COMPARISON_EVIDENCE")
            .Append(DesignConversionRuntimeScaffoldCriticUnit.InputEvidenceOnlyWarning)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return OperationResult<DesignConversionInternalEvaluationResult>.Ok(new DesignConversionInternalEvaluationResult(
            JobId: request.JobId,
            SubmittedRequest: submittedRequest,
            RenderResults: renderResults.ToArray(),
            ComparisonArtifacts: comparisonArtifacts.ToArray(),
            VisualJudgeResults: visualJudgeResults.ToArray(),
            QaResults: qaResults.ToArray(),
            RuntimeFlowResult: value,
            Verdict: value.Output.Verdict,
            ImprovedHtml: value.Output.Html,
            ImprovedCss: value.Output.Css,
            EvidenceRefs: evidenceRefs,
            Warnings: warnings,
            ScaffoldMarkers: BuildScaffoldMarkers(),
            DeterministicLogicClaimedAsLearning: value.DeterministicLogicClaimedAsLearning,
            VisualQaBoundaryTrace: visualQaBoundaryTrace)
        {
            ViewportFailures = viewportFailures.ToArray(),
            VisualCostAccounting = DesignConversionVisualCostAccounting.FromTierPolicy(
                visualQaBoundaryTrace.TierPolicy,
                evaluationAttemptCount: 1)
        });
    }

    private async Task<OperationResult<bool>> ProcessViewportRenderResultAsync(
        DesignConversionInternalEvaluateRequest request,
        SubmittedDesignConversionRequest submittedRequest,
        RenderQaResolution resolution,
        int index,
        DesignConversionRenderRequest renderRequest,
        OperationResult<DesignConversionRenderResult> renderResult,
        List<DesignConversionRenderResult> renderResults,
        List<DesignConversionQaComparisonArtifact> comparisonArtifacts,
        List<DesignConversionVisualJudgeResult> visualJudgeResults,
        List<RenderQaResultRecord> qaResults,
        List<DesignConversionViewportFailure> viewportFailures,
        CancellationToken cancellationToken)
    {
        if (!renderResult.IsSuccess)
        {
            if (IsCancellationFailure(renderResult))
                return renderResult.MapError<bool>();

            viewportFailures.Add(BuildViewportFailure(
                resolution,
                index,
                ViewportFailureStageRender,
                renderResult.ErrorCode));
            return OperationResult<bool>.Ok(false);
        }

        var trustedRenderResult = DesignConversionRenderEvidenceTrustBoundary.Canonicalize(
            _renderProvider,
            renderRequest,
            renderResult.Value!);
        if (!trustedRenderResult.IsSuccess)
        {
            if (IsCancellationFailure(trustedRenderResult))
                return trustedRenderResult.MapError<bool>();

            viewportFailures.Add(BuildViewportFailure(
                resolution,
                index,
                ViewportFailureStageRender,
                trustedRenderResult.ErrorCode));
            return OperationResult<bool>.Ok(false);
        }

        var rendered = trustedRenderResult.Value!;
        renderResults.Add(rendered);

        var expectedImage = BuildExpectedImage(submittedRequest, resolution, index);
        if (!expectedImage.IsSuccess)
        {
            viewportFailures.Add(BuildExpectedReferenceFailure(
                resolution,
                index,
                expectedImage.ErrorCode));
            return OperationResult<bool>.Ok(false);
        }

        var comparisonRequest = BuildComparisonRequest(
            request,
            submittedRequest,
            expectedImage.Value!,
            rendered.Image,
            rendered.RenderResultId,
            index);
        var comparisonResult = await ExecuteViewportOperationAsync(
            () => _comparisonProvider.CompareAsync(comparisonRequest, cancellationToken),
            cancellationToken);

        if (!comparisonResult.IsSuccess)
        {
            if (IsCancellationFailure(comparisonResult))
                return comparisonResult.MapError<bool>();

            viewportFailures.Add(BuildViewportFailure(
                resolution,
                index,
                ViewportFailureStageComparison,
                comparisonResult.ErrorCode));
            return OperationResult<bool>.Ok(false);
        }

        var trustedComparisonResult = DesignConversionComparisonEvidenceTrustBoundary.Canonicalize(
            _comparisonProvider,
            comparisonRequest,
            comparisonResult.Value!);
        if (!trustedComparisonResult.IsSuccess)
        {
            if (IsCancellationFailure(trustedComparisonResult))
                return trustedComparisonResult.MapError<bool>();

            viewportFailures.Add(BuildViewportFailure(
                resolution,
                index,
                ViewportFailureStageComparison,
                trustedComparisonResult.ErrorCode));
            return OperationResult<bool>.Ok(false);
        }

        var comparison = trustedComparisonResult.Value!;
        comparisonArtifacts.Add(comparison);

        var visualJudgeRequest = BuildVisualJudgeRequest(
            request,
            submittedRequest,
            comparison,
            rendered.RenderResultId,
            index);
        var visualJudgeResult = await ExecuteViewportOperationAsync(
            () => _visualJudge.JudgeAsync(visualJudgeRequest, cancellationToken),
            cancellationToken);

        if (!visualJudgeResult.IsSuccess)
        {
            if (IsCancellationFailure(visualJudgeResult))
                return visualJudgeResult.MapError<bool>();

            viewportFailures.Add(BuildViewportFailure(
                resolution,
                index,
                ViewportFailureStageVisualJudge,
                visualJudgeResult.ErrorCode));
            return OperationResult<bool>.Ok(false);
        }

        var visualJudge = visualJudgeResult.Value!;
        visualJudgeResults.Add(visualJudge);
        qaResults.Add(new RenderQaResultRecord(
            ResultId: $"{request.JobId}:qa-result:{index + 1}",
            RequestId: submittedRequest.RequestId,
            Expected: comparison.Expected,
            Actual: comparison.Actual,
            Verdict: visualJudge.Verdict,
            Metrics: BuildQaMetrics(comparison, visualJudge),
            Warnings: rendered.Warnings
                .Concat(visualJudge.IsBlocked ? [visualJudge.Status] : [])
                .ToArray()));

        return OperationResult<bool>.Ok(true);
    }

    private async Task<IReadOnlyList<BoundedConcurrentRenderOperation>?> TryRunBoundedConcurrentRenderOperationsAsync(
        DesignConversionInternalEvaluateRequest request,
        SubmittedDesignConversionRequest submittedRequest,
        int maxRuns,
        CancellationToken cancellationToken)
    {
        var maxConcurrency = ResolveBoundedConcurrentRenderOperationCount(maxRuns);
        if (maxConcurrency <= 1)
            return null;

        var operations = new BoundedConcurrentRenderOperation?[maxRuns];
        var nextIndex = -1;
        var workerTasks = Enumerable.Range(0, maxConcurrency)
            .Select(_ => RunBoundedConcurrentRenderWorkerAsync(
                request,
                submittedRequest,
                maxRuns,
                operations,
                () => Interlocked.Increment(ref nextIndex),
                cancellationToken))
            .ToArray();

        try
        {
            await Task.WhenAll(workerTasks);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await ObserveStartedRenderTasksAsync(workerTasks);
            throw;
        }
        catch
        {
            await ObserveStartedRenderTasksAsync(workerTasks);
            throw;
        }

        var completed = new BoundedConcurrentRenderOperation[maxRuns];
        for (var index = 0; index < maxRuns; index++)
            completed[index] = operations[index] ?? throw new InvalidOperationException(
                $"Bounded render worker did not produce operation for viewport index {index}.");

        return completed;
    }

    private int ResolveBoundedConcurrentRenderOperationCount(int maxRuns)
    {
        if (maxRuns <= 1 ||
            _renderProvider is not IDesignConversionBoundedConcurrentRenderProvider concurrentRenderProvider)
        {
            return 1;
        }

        var advertisedConcurrency = concurrentRenderProvider.MaxConcurrentRenderOperations;
        if (advertisedConcurrency <= 1)
            return 1;

        return Math.Min(
            Math.Min(maxRuns, advertisedConcurrency),
            MaxBoundedConcurrentRenderWorkerCount);
    }

    private async Task RunBoundedConcurrentRenderWorkerAsync(
        DesignConversionInternalEvaluateRequest request,
        SubmittedDesignConversionRequest submittedRequest,
        int maxRuns,
        BoundedConcurrentRenderOperation?[] operations,
        Func<int> claimNextIndex,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var index = claimNextIndex();
            if (index >= maxRuns)
                return;

            var resolution = request.RenderResolutions[index];
            var renderRequest = BuildRenderRequest(request, submittedRequest, resolution, index);
            var renderResult = await ExecuteViewportOperationAsync(
                () => _renderProvider.RenderAsync(renderRequest, cancellationToken),
                cancellationToken);

            operations[index] = new BoundedConcurrentRenderOperation(
                Index: index,
                Resolution: resolution,
                Request: renderRequest,
                Result: renderResult);
        }
    }

    private static async Task ObserveStartedRenderTasksAsync(
        IReadOnlyList<Task> tasks)
    {
        foreach (var task in tasks)
        {
            try
            {
                await task;
            }
            catch
            {
                // Observation only. The original exception path is preserved by the caller.
            }
        }
    }

    private sealed record BoundedConcurrentRenderOperation(
        int Index,
        RenderQaResolution Resolution,
        DesignConversionRenderRequest Request,
        OperationResult<DesignConversionRenderResult> Result);

    private async Task<OperationResult<RuntimeFlowEvaluation>> RunRuntimeFlowAsync(
        DesignConversionRuntimeFlowInput runtimeInput,
        string jobId,
        LogicalExecutionFlags flags,
        IReadOnlyDictionary<string, object> executionMetadata,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<RuntimeFlowEvaluation>.Fail(
                "DesignConversion internal evaluation cancelled.",
                "CANCELLED");

        if (_machineOrchestratorTask is null)
        {
            var directFlow = await _routingFlow!.RunAsync(
                runtimeInput,
                jobId,
                flags,
                executionMetadata,
                cancellationToken);
            return directFlow.IsSuccess
                ? OperationResult<RuntimeFlowEvaluation>.Ok(new RuntimeFlowEvaluation(directFlow.Value!, []))
                : directFlow.MapError<RuntimeFlowEvaluation>();
        }

        OperationResult<ILogicalOrchestrator> orchestrator;
        try
        {
            orchestrator = await _machineOrchestratorTask.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return OperationResult<RuntimeFlowEvaluation>.Fail(
                "DesignConversion internal evaluation cancelled.",
                "CANCELLED");
        }

        if (!orchestrator.IsSuccess)
            return orchestrator.MapError<RuntimeFlowEvaluation>();
        if (orchestrator.Value is null)
        {
            return OperationResult<RuntimeFlowEvaluation>.Fail(
                "DesignConversion machine orchestrator is required.",
                "ORCHESTRATOR_REQUIRED");
        }

        var capture = new DesignConversionRuntimeFlowResultCapture();
        var machineExecutionMetadata = executionMetadata
            .Concat(new Dictionary<string, object>
            {
                ["runtime_flow_input"] = runtimeInput,
                [DesignConversionRuntimeFlowResultCapture.MetadataKey] = capture
            })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        OperationResult<LogicalOrchestratorResult> orchestrated;
        try
        {
            orchestrated = await orchestrator.Value.RunAsync(new LogicalOrchestratorRequest(
                RunId: jobId,
                ScenarioId: DesignConversionRoutingFlow.ScenarioId,
                UserRequest: "convert this design",
                Flags: flags,
                Metadata: machineExecutionMetadata), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return OperationResult<RuntimeFlowEvaluation>.Fail(
                "DesignConversion internal evaluation cancelled.",
                "CANCELLED");
        }

        if (!orchestrated.IsSuccess)
            return orchestrated.MapError<RuntimeFlowEvaluation>();
        if (orchestrated.Value is null)
        {
            return OperationResult<RuntimeFlowEvaluation>.Fail(
                "DesignConversion machine orchestrator result is required.",
                "ORCHESTRATOR_RESULT_REQUIRED");
        }

        if (!capture.TryTake(out var capturedFlowResult))
        {
            return OperationResult<RuntimeFlowEvaluation>.Fail(
                "DesignConversion machine orchestrator did not capture the runtime flow result from this execution.",
                "RUNTIME_FLOW_RESULT_CAPTURE_REQUIRED");
        }

        return OperationResult<RuntimeFlowEvaluation>.Ok(
            BuildRuntimeFlowEvaluation(capturedFlowResult, orchestrated.Value));
    }

    private static IReadOnlyDictionary<string, object> BuildRuntimeExecutionMetadata()
        => new Dictionary<string, object>
        {
            ["internal_service"] = nameof(DesignConversionInternalService),
            ["service_first_marker"] = ServiceFirstMarker
        };

    private static RuntimeFlowEvaluation BuildRuntimeFlowEvaluation(
        DesignConversionRuntimeFlowResult captured,
        LogicalOrchestratorResult orchestrated)
    {
        var flowResult = orchestrated.FlowResult;
        var metrics = captured.Candidate.Metadata
            .Concat(flowResult.Metrics ?? new Dictionary<string, string>())
            .Concat(new Dictionary<string, string>
            {
                ["logical_flow_id"] = flowResult.FlowId,
                ["logical_flow_scenario_id"] = flowResult.ScenarioId,
                ["logical_flow_primary_metric_name"] = flowResult.Summary.PrimaryMetricName,
                ["logical_flow_enhanced_primary"] = flowResult.Summary.EnhancedPrimary.ToString("0.######", CultureInfo.InvariantCulture),
                ["logical_flow_overall_verdict"] = flowResult.OverallVerdict,
                [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] = DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue,
                [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] = DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue
            })
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);
        var evidenceRefs = captured.EvidenceRefs
            .Concat(flowResult.EvidenceIds ?? [])
            .Append($"logical-flow:{flowResult.FlowId}")
            .Append($"logical-orchestrator-selected-flow:{orchestrated.SelectedFlowId}")
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var candidate = captured.Candidate with
        {
            EvidenceRefs = captured.Candidate.EvidenceRefs
                .Concat(evidenceRefs)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Metadata = metrics
        };
        var output = captured.Output with
        {
            EvidenceRefs = captured.Output.EvidenceRefs
                .Concat(evidenceRefs)
                .Distinct(StringComparer.Ordinal)
                .ToArray()
        };
        var improverOutput = captured.ImproverOutput with
        {
            Candidate = candidate,
            Output = output
        };
        var warnings = (flowResult.Warnings ?? [])
            .Concat(orchestrated.Warnings ?? [])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new RuntimeFlowEvaluation(captured with
        {
            ImproverOutput = improverOutput,
            Candidate = candidate,
            Output = output,
            EvidenceRefs = evidenceRefs
        }, warnings);
    }

    private sealed record RuntimeFlowEvaluation(
        DesignConversionRuntimeFlowResult FlowResult,
        IReadOnlyList<string> Warnings);

    private static IReadOnlyDictionary<string, double> BuildQaMetrics(
        DesignConversionQaComparisonArtifact comparison,
        DesignConversionVisualJudgeResult visualJudge)
        => comparison.Metrics
            .Concat(new Dictionary<string, double>
            {
                ["visual_judge_score"] = visualJudge.Verdict.Score,
                ["visual_judge_blocked"] = visualJudge.IsBlocked ? 1.0 : 0.0,
                ["external_ai_visual_judge_executed"] = visualJudge.Provider.ExternalCallExecuted ? 1.0 : 0.0,
                ["estimated_visual_judge_cost_usd"] = (double)(visualJudge.EstimatedCostUsd ?? 0m)
            })
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);

    private static bool IsCancellationFailure<T>(OperationResult<T> result)
        => string.Equals(result.ErrorCode, "CANCELLED", StringComparison.OrdinalIgnoreCase);

    private static async Task<OperationResult<T>> ExecuteViewportOperationAsync<T>(
        Func<Task<OperationResult<T>>> operation,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await operation();
            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return OperationResult<T>.Fail("Viewport operation failed.", "EXCEPTION_THROWN");
        }
    }

    private static DesignConversionViewportFailure BuildViewportFailure(
        RenderQaResolution resolution,
        int index,
        string stage,
        string errorCode)
        => BuildViewportFailure(
            resolution,
            index,
            stage,
            errorCode,
            includeViewportIdEvidence: true);

    private static DesignConversionViewportFailure BuildExpectedReferenceFailure(
        RenderQaResolution resolution,
        int index,
        string errorCode)
        => BuildViewportFailure(
            resolution,
            index,
            ViewportFailureStageExpectedReference,
            errorCode,
            includeViewportIdEvidence: false);

    private static DesignConversionViewportFailure BuildViewportFailure(
        RenderQaResolution resolution,
        int index,
        string stage,
        string errorCode,
        bool includeViewportIdEvidence)
    {
        var oneBasedIndex = index + 1;
        var safeStage = SanitizeFailureToken(stage, fallback: "operation");
        var safeErrorCode = SanitizeFailureToken(errorCode, fallback: "ERROR");
        var fallbackViewportId = $"viewport-{oneBasedIndex.ToString(CultureInfo.InvariantCulture)}";
        var safeViewportId = includeViewportIdEvidence
            ? SanitizeFailureToken(resolution.ResolutionId, fallback: fallbackViewportId)
            : fallbackViewportId;
        var evidenceRefs = new List<string>
        {
            ViewportOperationFailureMarker,
            $"viewport_operation_failure:stage={safeStage}",
            $"viewport_operation_failure:index={oneBasedIndex.ToString(CultureInfo.InvariantCulture)}",
            $"viewport_operation_failure:error_code={safeErrorCode}"
        };
        if (includeViewportIdEvidence)
            evidenceRefs.Add($"viewport_operation_failure:viewport={safeViewportId}");

        return new DesignConversionViewportFailure(
            ViewportId: safeViewportId,
            Width: resolution.Width,
            Height: resolution.Height,
            DeviceScaleFactor: resolution.DeviceScaleFactor,
            OneBasedIndex: oneBasedIndex,
            Stage: safeStage,
            ErrorCode: safeErrorCode,
            EvidenceRefs: evidenceRefs.ToArray());
    }

    private static DesignConversionRuntimeFlowResult ApplyViewportFailureBoundary(
        DesignConversionRuntimeFlowResult value,
        string jobId,
        IReadOnlyList<DesignConversionViewportFailure> failures)
    {
        var failureEvidenceRefs = failures
            .SelectMany(failure => failure.EvidenceRefs)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var output = value.Output with
        {
            Verdict = BuildViewportFailureVerdict(jobId, failures),
            EvidenceRefs = value.Output.EvidenceRefs
                .Concat(failureEvidenceRefs)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Accepted = false
        };

        return value with
        {
            ImproverOutput = value.ImproverOutput with { Output = output },
            Output = output,
            EvidenceRefs = value.EvidenceRefs
                .Concat(failureEvidenceRefs)
                .Distinct(StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static VisualVerdict BuildViewportFailureVerdict(
        string jobId,
        IReadOnlyList<DesignConversionViewportFailure> failures)
        => new(
            VerdictId: $"{jobId}:viewport-operation-failure-verdict",
            Score: 0.0,
            Defects: [],
            EvidenceRefs: failures
                .SelectMany(failure => failure.EvidenceRefs)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Passed: false);

    private static string ViewportFailureStatus(string stage)
        => stage switch
        {
            ViewportFailureStageRender => ViewportRenderFailedStatus,
            ViewportFailureStageExpectedReference => ViewportExpectedReferenceMissingStatus,
            ViewportFailureStageComparison => ViewportComparisonFailedStatus,
            ViewportFailureStageVisualJudge => ViewportVisualJudgeFailedStatus,
            _ => "VIEWPORT_OPERATION_FAILED"
        };

    private static string SanitizeFailureToken(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        var chars = value
            .Trim()
            .Take(64)
            .Select(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-' or '.'
                ? ch
                : '_')
            .ToArray();
        var token = new string(chars).Trim('_');
        return string.IsNullOrWhiteSpace(token) ? fallback : token;
    }

    private static OperationResult<bool> Validate(DesignConversionInternalEvaluateRequest request)
    {
        if (request is null)
            return OperationResult<bool>.Fail("DesignConversion internal request is required.", "REQUEST_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.JobId))
            return OperationResult<bool>.Fail("DesignConversion job id is required.", "JOB_ID_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.RequestId))
            return OperationResult<bool>.Fail("DesignConversion request id is required.", "REQUEST_ID_REQUIRED");
        if (request.DesignTreePayload is null || request.OriginalTreePayload is null)
            return OperationResult<bool>.Fail("DesignConversion tree payloads are required.", "TREE_PAYLOADS_REQUIRED");
        var rawPayloadSize = ValidateRawPayloadSizes(request);
        if (!rawPayloadSize.IsSuccess)
            return rawPayloadSize;

        if (request.Budget is null)
            return OperationResult<bool>.Fail("DesignConversion budget metadata is required.", "BUDGET_REQUIRED");
        if (request.Budget.BudgetUsd < 0m)
            return OperationResult<bool>.Fail(
                "DesignConversion improvement-loop budgetUsd must be greater than or equal to zero.",
                NegativeBudgetUsdErrorCode);
        if (request.Budget.MaxImprovementCandidates < 0 ||
            (request.Budget.MaxImprovementCandidates == 0 &&
             !DesignConversionVisualQaBoundaryPolicy.IsServerOwnedEntitlementTier(request.Budget.Tier)))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion improvement candidate budget must be greater than zero.",
                ImprovementCandidateBudgetRequiredCode);
        }
        if (request.Budget.MaxRenderQaRuns <= 0)
            return OperationResult<bool>.Fail("DesignConversion render QA budget must be greater than zero.", "RENDER_QA_BUDGET_REQUIRED");
        if (request.RenderResolutions is null || request.RenderResolutions.Count == 0)
            return OperationResult<bool>.Fail("DesignConversion render resolutions are required.", "RENDER_RESOLUTIONS_REQUIRED");
        if ((request.DesignRenders is null || request.DesignRenders.Count == 0) &&
            (request.Screenshots is null || request.Screenshots.Count == 0))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion expected design render or screenshot is required.",
                "EXPECTED_RENDER_REQUIRED");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateRawPayloadSizes(DesignConversionInternalEvaluateRequest request)
    {
        var html = ValidateRawPayloadSize("Html", request.Html);
        if (!html.IsSuccess)
            return html;

        var css = ValidateRawPayloadSize("Css", request.Css);
        if (!css.IsSuccess)
            return css;

        var designTree = ValidateRawPayloadSize("DesignTreePayload.Content", request.DesignTreePayload.Content);
        if (!designTree.IsSuccess)
            return designTree;

        return ValidateRawPayloadSize("OriginalTreePayload.Content", request.OriginalTreePayload.Content);
    }

    private static OperationResult<bool> ValidateRawPayloadSize(string field, string? value)
    {
        if (value is null)
        {
            return OperationResult<bool>.Fail(
                $"DesignConversion raw field '{field}' is required.",
                RawPayloadRequiredCode);
        }

        return value.Length <= MaxRawPayloadChars
            ? OperationResult<bool>.Ok(true)
            : OperationResult<bool>.Fail(
                $"DesignConversion raw field '{field}' exceeds the maximum of {MaxRawPayloadChars.ToString(CultureInfo.InvariantCulture)} characters.",
                PayloadTooLargeCode);
    }

    private static int ResolveImprovementAttemptCount(DesignConversionInternalEvaluateRequest request)
    {
        const string loopAttemptMarker = ":loop-attempt:";
        var markerIndex = request.JobId.LastIndexOf(loopAttemptMarker, StringComparison.Ordinal);
        if (markerIndex >= 0)
        {
            var rawAttempt = request.JobId[(markerIndex + loopAttemptMarker.Length)..];
            if (int.TryParse(rawAttempt, NumberStyles.None, CultureInfo.InvariantCulture, out var attemptFromJob) &&
                attemptFromJob > 0)
            {
                return attemptFromJob;
            }
        }

        if (request.Provenance is not null &&
            request.Provenance.TryGetValue("improvement_loop_attempt", out var provenanceAttempt) &&
            int.TryParse(provenanceAttempt, NumberStyles.None, CultureInfo.InvariantCulture, out var attemptFromProvenance) &&
            attemptFromProvenance > 0)
        {
            return attemptFromProvenance;
        }

        return 1;
    }

    private static SubmittedDesignConversionRequest ToSubmittedRequest(DesignConversionInternalEvaluateRequest request)
    {
        var renderResolutions = request.RenderResolutions ?? [];
        return new SubmittedDesignConversionRequest(
            RequestId: request.RequestId,
            SourceKind: request.SourceKind,
            Html: request.Html,
            Css: request.Css,
            DesignTreePayload: request.DesignTreePayload,
            OriginalTreePayload: request.OriginalTreePayload,
            Screenshots: CloneSubmittedArtifacts(request.Screenshots, renderResolutions),
            DesignRenders: CloneSubmittedArtifacts(request.DesignRenders, renderResolutions),
            Budget: request.Budget,
            Provenance: (request.Provenance ?? new Dictionary<string, string>())
                .Concat(new Dictionary<string, string>
                {
                    ["internal_service_job_id"] = request.JobId,
                    ["submission_mapper"] = nameof(DesignConversionInternalService)
                })
                .GroupBy(pair => pair.Key, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal));
    }

    private static DesignConversionRenderRequest BuildRenderRequest(
        DesignConversionInternalEvaluateRequest request,
        SubmittedDesignConversionRequest submittedRequest,
        RenderQaResolution resolution,
        int index)
        => new(
            RenderRequestId: $"{request.JobId}:render-request:{index + 1}",
            RequestId: submittedRequest.RequestId,
            Html: submittedRequest.Html,
            Css: submittedRequest.Css,
            Resolution: resolution,
            Metadata: new Dictionary<string, string>
            {
                ["job_id"] = request.JobId,
                ["render_index"] = (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["qa_data_role"] = "input_evidence_only",
                ["renderer"] = "injected_provider"
            });

    private static DesignConversionQaComparisonRequest BuildComparisonRequest(
        DesignConversionInternalEvaluateRequest request,
        SubmittedDesignConversionRequest submittedRequest,
        RenderQaImageRecord expected,
        RenderQaImageRecord actual,
        string renderResultId,
        int index)
        => new(
            ComparisonRequestId: $"{request.JobId}:comparison-request:{index + 1}",
            RequestId: submittedRequest.RequestId,
            Expected: expected,
            Actual: actual,
            Metadata: new Dictionary<string, string>
            {
                ["job_id"] = request.JobId,
                ["render_result_id"] = renderResultId,
                ["comparison_index"] = (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["comparison"] = "injected_provider",
                ["qa_data_role"] = "input_evidence_only"
            });

    private static DesignConversionVisualJudgeRequest BuildVisualJudgeRequest(
        DesignConversionInternalEvaluateRequest request,
        SubmittedDesignConversionRequest submittedRequest,
        DesignConversionQaComparisonArtifact comparisonArtifact,
        string renderResultId,
        int index)
    {
        var comparisonProofUnavailable = IsComparisonProofUnavailable(comparisonArtifact);
        var comparisonScore = comparisonProofUnavailable ? 0.0 : comparisonArtifact.Verdict.Score;
        var comparisonPassed = comparisonArtifact.Verdict.Passed && !comparisonProofUnavailable;
        var comparisonDefectCategories = comparisonArtifact.Verdict.Defects
            .Select(defect => defect.Category)
            .Where(category => !string.IsNullOrWhiteSpace(category));
        if (comparisonProofUnavailable)
            comparisonDefectCategories = comparisonDefectCategories
                .Append(DeterministicDesignConversionComparisonProvider.VisualComparisonNotProvenMarker);
        var comparisonDefectCategoryList = comparisonDefectCategories
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var qualityThreshold = DesignConversionVisualQaBoundaryPolicy.ResolveQualityThreshold(request.Budget);

        var metadata = new Dictionary<string, string>
            {
                ["job_id"] = request.JobId,
                ["render_result_id"] = renderResultId,
                ["comparison_request_id"] = comparisonArtifact.ComparisonRequestId,
                ["comparison_artifact_id"] = comparisonArtifact.ArtifactId,
                ["visual_judge_index"] = (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["visual_judge"] = "optional_injected_provider",
                ["qa_data_role"] = "input_evidence_only",
                ["comparison_verdict_score"] = comparisonScore.ToString("R", CultureInfo.InvariantCulture),
                ["comparison_verdict_passed"] = comparisonPassed.ToString(),
                ["quality_threshold"] = qualityThreshold.ToString("R", CultureInfo.InvariantCulture),
                ["comparison_defect_count"] = comparisonDefectCategoryList.Length.ToString(CultureInfo.InvariantCulture),
                ["comparison_defect_categories"] = string.Join("|", comparisonDefectCategoryList)
            };
        if (comparisonProofUnavailable)
        {
            metadata["comparison_proof_status"] = VisualComparisonProofUnavailableStatus;
            metadata[DeterministicDesignConversionComparisonProvider.VisualComparisonNotProvenMarker] = bool.TrueString;
            metadata["metadata_only_comparison_rejected"] = bool.TrueString;
            metadata["product_ready"] = bool.FalseString;
        }

        foreach (var metric in comparisonArtifact.Metrics)
        {
            metadata[$"comparison_metric_{metric.Key}"] = metric.Value.ToString("R", CultureInfo.InvariantCulture);
            if (string.Equals(metric.Key, "visual_distance", StringComparison.Ordinal))
                metadata["visual_distance"] = metric.Value.ToString("R", CultureInfo.InvariantCulture);
        }

        return new DesignConversionVisualJudgeRequest(
            JudgeRequestId: $"{request.JobId}:visual-judge-request:{index + 1}",
            RequestId: submittedRequest.RequestId,
            ExpectedScreenshot: comparisonArtifact.Expected,
            RenderedImage: comparisonArtifact.Actual,
            DiffArtifactUri: comparisonArtifact.DiffImageUri,
            DiffArtifactSha256: comparisonArtifact.DiffImageSha256,
            Metadata: metadata);
    }

    private static bool IsComparisonProofUnavailable(DesignConversionQaComparisonArtifact comparisonArtifact)
        => !MetadataIsTruthy(comparisonArtifact.Metadata, DesignConversionComparisonEvidenceTrustBoundary.ServerCanonicalizedKey)
           || !MetadataIsTruthy(comparisonArtifact.Metadata, DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey)
           || !MetadataIsTruthy(comparisonArtifact.Metadata, DesignConversionRenderEvidenceTrustBoundary.ApprovalEvidenceAllowedKey)
           || !MetadataIsTruthy(comparisonArtifact.Metadata, "visual_comparison_proven")
           || MetadataIsTruthy(comparisonArtifact.Metadata, DeterministicDesignConversionComparisonProvider.VisualComparisonNotProvenMarker)
           || MetadataIsTruthy(comparisonArtifact.Metadata, "metadata_only_comparison_rejected")
           || MetadataEquals(comparisonArtifact.Metadata, "hidden_determinism_boundary", "metadata_only_comparison")
           || EvidenceContains(comparisonArtifact.EvidenceRefs, DeterministicDesignConversionComparisonProvider.BoundaryMarker)
           || EvidenceContains(comparisonArtifact.EvidenceRefs, DeterministicDesignConversionComparisonProvider.VisualComparisonNotProvenMarker)
           || EvidenceContains(comparisonArtifact.EvidenceRefs, DeterministicDesignConversionComparisonProvider.MetadataOnlyComparisonRejectedMarker)
           || EvidenceContains(comparisonArtifact.Verdict.EvidenceRefs, DeterministicDesignConversionComparisonProvider.BoundaryMarker)
           || EvidenceContains(comparisonArtifact.Verdict.EvidenceRefs, DeterministicDesignConversionComparisonProvider.VisualComparisonNotProvenMarker)
           || EvidenceContains(comparisonArtifact.Verdict.EvidenceRefs, DeterministicDesignConversionComparisonProvider.MetadataOnlyComparisonRejectedMarker)
           || MetricGreaterThanZero(comparisonArtifact.Metrics, DeterministicDesignConversionComparisonProvider.VisualComparisonNotProvenMarker);

    private static bool MetadataIsTruthy(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "1", StringComparison.Ordinal) ||
            string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));

    private static bool MetadataEquals(IReadOnlyDictionary<string, string>? metadata, string key, string expected)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           string.Equals(value, expected, StringComparison.Ordinal);

    private static bool EvidenceContains(IReadOnlyList<string>? evidenceRefs, string expected)
        => evidenceRefs is not null && evidenceRefs.Contains(expected, StringComparer.Ordinal);

    private static bool MetricGreaterThanZero(IReadOnlyDictionary<string, double>? metrics, string key)
        => metrics is not null && metrics.TryGetValue(key, out var value) && value > 0.0;

    private static OperationResult<RenderQaImageRecord> BuildExpectedImage(
        SubmittedDesignConversionRequest request,
        RenderQaResolution resolution,
        int index)
    {
        var artifact = DesignConversionRenderResolutionFactory.ResolutionHasCallerIdentity(resolution)
            ? ResolveIdentifiedExpectedArtifact(request, resolution)
            : OperationResult<DesignRenderArtifact?>.Ok(
                request.DesignRenders.FirstOrDefault(render =>
                    render.Width == resolution.Width && render.Height == resolution.Height)
                ?? request.Screenshots.FirstOrDefault(render =>
                    render.Width == resolution.Width && render.Height == resolution.Height));
        if (!artifact.IsSuccess)
            return OperationResult<RenderQaImageRecord>.Fail(artifact.ErrorMessage, artifact.ErrorCode);

        var expectedArtifact = artifact.Value;
        if (expectedArtifact is null)
        {
            return OperationResult<RenderQaImageRecord>.Fail(
                $"Expected design render or screenshot is missing for render QA viewport at {resolution.Width}x{resolution.Height}.",
                ExpectedRenderResolutionMissingCode);
        }

        var expectedResolution = new RenderQaResolution(
            ResolutionId: resolution.ResolutionId,
            Width: expectedArtifact.Width,
            Height: expectedArtifact.Height,
            DeviceScaleFactor: resolution.DeviceScaleFactor);

        return OperationResult<RenderQaImageRecord>.Ok(new RenderQaImageRecord(
            ImageId: $"{expectedArtifact.ArtifactId}:expected:{index + 1}",
            Label: "expected-design-render",
            Resolution: expectedResolution,
            Uri: expectedArtifact.Uri,
            Sha256: expectedArtifact.Sha256,
            Metadata: (expectedArtifact.Metadata ?? new Dictionary<string, string>())
                .Concat(new Dictionary<string, string>
                {
                    ["source_artifact_id"] = expectedArtifact.ArtifactId,
                    ["source_artifact_kind"] = expectedArtifact.Kind
                })
                .GroupBy(pair => pair.Key, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal)));
    }

    private static IReadOnlyList<DesignRenderArtifact> CloneSubmittedArtifacts(
        IReadOnlyList<DesignRenderArtifact>? artifacts,
        IReadOnlyList<RenderQaResolution> renderResolutions)
        => (artifacts ?? [])
            .Select(artifact => CloneSubmittedArtifact(artifact, renderResolutions))
            .ToArray();

    private static DesignRenderArtifact CloneSubmittedArtifact(
        DesignRenderArtifact artifact,
        IReadOnlyList<RenderQaResolution> renderResolutions)
    {
        var metadata = CloneMetadataWithoutViewportAliases(artifact.Metadata);
        var canonicalViewportId = ResolveSingleCanonicalViewportId(artifact, renderResolutions);
        if (!string.IsNullOrWhiteSpace(canonicalViewportId))
            metadata[DesignConversionRenderResolutionFactory.CanonicalViewportIdMetadataKey] = canonicalViewportId;

        return artifact with { Metadata = metadata };
    }

    private static Dictionary<string, string> CloneMetadataWithoutViewportAliases(
        IReadOnlyDictionary<string, string>? metadata)
    {
        var clone = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in metadata ?? new Dictionary<string, string>())
        {
            if (DesignConversionRenderResolutionFactory.IsViewportIdentityMetadataKey(pair.Key))
                continue;

            clone[pair.Key] = pair.Value;
        }

        return clone;
    }

    private static string ResolveSingleCanonicalViewportId(
        DesignRenderArtifact artifact,
        IReadOnlyList<RenderQaResolution> renderResolutions)
    {
        var matches = renderResolutions
            .Where(resolution => resolution.Width == artifact.Width && resolution.Height == artifact.Height)
            .Select(resolution => new
            {
                Resolution = resolution,
                Canonical = DesignConversionRenderResolutionFactory.CanonicalResolutionIdForArtifact(
                    artifact,
                    resolution)
            })
            .Where(match => match.Canonical.IsSuccess &&
                            string.Equals(match.Canonical.Value, match.Resolution.ResolutionId, StringComparison.Ordinal))
            .Select(match => match.Resolution.ResolutionId)
            .Distinct(StringComparer.Ordinal)
            .Take(2)
            .ToArray();

        return matches.Length == 1 ? matches[0] : string.Empty;
    }

    private static OperationResult<DesignRenderArtifact?> ResolveIdentifiedExpectedArtifact(
        SubmittedDesignConversionRequest request,
        RenderQaResolution resolution)
    {
        var designRender = ResolveSingleIdentifiedExpectedArtifact(request.DesignRenders, resolution);
        if (!designRender.IsSuccess || designRender.Value is not null)
            return designRender;

        return ResolveSingleIdentifiedExpectedArtifact(request.Screenshots, resolution);
    }

    private static OperationResult<DesignRenderArtifact?> ResolveSingleIdentifiedExpectedArtifact(
        IReadOnlyList<DesignRenderArtifact> artifacts,
        RenderQaResolution resolution)
    {
        var matches = artifacts
            .Where(artifact => artifact.Width == resolution.Width && artifact.Height == resolution.Height)
            .Where(artifact => MetadataCanonicalViewportIdMatchesResolution(artifact, resolution))
            .Take(2)
            .ToArray();

        return matches.Length switch
        {
            0 => OperationResult<DesignRenderArtifact?>.Ok(null),
            1 => OperationResult<DesignRenderArtifact?>.Ok(matches[0]),
            _ => ExpectedIdentifiedReferenceMissingOrAmbiguous(resolution)
        };
    }

    private static bool MetadataCanonicalViewportIdMatchesResolution(
        DesignRenderArtifact artifact,
        RenderQaResolution resolution)
        => artifact.Metadata is not null &&
           artifact.Metadata.TryGetValue(
               DesignConversionRenderResolutionFactory.CanonicalViewportIdMetadataKey,
               out var canonicalViewportId) &&
           string.Equals(canonicalViewportId, resolution.ResolutionId, StringComparison.Ordinal);

    private static OperationResult<DesignRenderArtifact?> ExpectedIdentifiedReferenceMissingOrAmbiguous(
        RenderQaResolution resolution)
        => OperationResult<DesignRenderArtifact?>.Fail(
            $"Expected design render or screenshot is missing or ambiguous for identified render QA viewport at {resolution.Width}x{resolution.Height}.",
            ExpectedRenderResolutionMissingCode);

    private static IReadOnlyDictionary<string, string> BuildDeterministicEvidenceRefs(
        DesignConversionInternalEvaluateRequest request)
        => (request.DeterministicEvidenceRefs ?? new Dictionary<string, string>())
            .Concat(new Dictionary<string, string>
            {
                ["internal_service"] = "service_first_no_host",
                ["render_provider"] = "injected_fakeable",
                ["comparison_provider"] = "injected_fakeable",
                ["visual_judge_provider"] = "optional_injected_fakeable",
                ["deterministic_logic_claimed_as_learning"] = bool.FalseString
            })
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);

    private IReadOnlyDictionary<string, string> BuildScaffoldMarkers()
    {
        var explicitLocalBrowserInjectionProven = _renderProvider is BrowserBackedDesignConversionRenderProvider;

        return new Dictionary<string, string>
        {
            ["service_first_no_asp_net_host"] = bool.TrueString,
            ["render_provider_injected"] = bool.TrueString,
            ["comparison_provider_injected"] = bool.TrueString,
            ["visual_judge_provider_injected"] = bool.TrueString,
            ["browser_renderer_provider_available"] = bool.TrueString,
            ["browser_renderer_explicit_local_injection_proven"] = explicitLocalBrowserInjectionProven.ToString(),
            ["browser_renderer_default_production_wired"] = bool.FalseString,
            ["browser_renderer_default_scope"] = explicitLocalBrowserInjectionProven
                ? "explicit_local_injection_only_not_production_default"
                : "not_browser_backed_for_current_injection",
            ["browser_renderer_injected_provider_kind"] = ResolveRenderProviderKind(_renderProvider),
            ["external_ai_visual_judge_provider_available"] = bool.TrueString,
            ["external_ai_visual_judge_configured"] = bool.FalseString,
            ["local_visual_judge_default_wired"] = bool.TrueString,
            ["persistent_storage_implemented"] = bool.FalseString,
            ["deterministic_logic_claimed_as_learning"] = bool.FalseString
        };
    }

    private static string ResolveRenderProviderKind(IDesignConversionRenderProvider renderProvider)
        => renderProvider switch
        {
            BrowserBackedDesignConversionRenderProvider => BrowserBackedDesignConversionRenderProvider.ProviderKind,
            DeterministicLocalDesignConversionPngRenderProvider => DeterministicLocalDesignConversionPngRenderProvider.ProviderKind,
            DeterministicDesignConversionRenderProvider => DeterministicDesignConversionRenderProvider.ProviderKind,
            _ => renderProvider.GetType().Name
        };
}
