// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class OrchestrationTaskDecompositionFlow : ILogicalFlow<OrchestrationTaskDecompositionRequest>
{
    public const string FlowId = "orchestration-task-decomposition-flow";
    public const string ScenarioId = "orchestration_prompt_file_task_decomposition";

    private static readonly string[] CandidateUnits =
    [
        OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
        OrchestrationRoutingCurriculum.TranslationRuEnUnitId,
        OrchestrationRoutingCurriculum.DocumentationDotNetUnitId
    ];

    private static readonly string[] PhaseOrder =
    [
        "intake-source",
        "rag-context-planning",
        "screen-and-system-analysis",
        "architecture-flow-design",
        "model-round-execution",
        "technical-design-and-implementation-plan",
        "test-debug-deploy",
        "feedback-memory-loop",
        "multi-technology-alternatives",
        "skill-library-packaging"
    ];

    private readonly DistributedRoutingRuntime _runtime;
    private readonly OrchestrationScenarioPhasePlanner _phasePlanner;

    public OrchestrationTaskDecompositionFlow(
        DistributedRoutingRuntime runtime,
        OrchestrationScenarioPhasePlanner? phasePlanner = null)
    {
        _runtime = runtime;
        _phasePlanner = phasePlanner ?? new OrchestrationScenarioPhasePlanner();
    }

    public async Task<OperationResult<OrchestrationTaskDecompositionResult>> DecomposeAsync(
        OrchestrationTaskDecompositionRequest request,
        CancellationToken ct = default)
    {
        if (request is null)
            return OperationResult<OrchestrationTaskDecompositionResult>.Fail("Decomposition request is required.", "DECOMPOSITION_REQUEST_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.Prompt))
            return OperationResult<OrchestrationTaskDecompositionResult>.Fail("Prompt is required for orchestration decomposition.", "PROMPT_REQUIRED");

        var routingRequest = new RoutingRequest(
            request.RequestId,
            request.Prompt,
            string.Join(" ", request.StepHints),
            null,
            "orchestration",
            CandidateUnits,
            new Dictionary<string, string>
            {
                ["scenario_id"] = request.ScenarioId,
                ["flow_id"] = FlowId
            });

        var routed = await DistributedRoutingBootstrap.RouteAndLocateAsync(_runtime, routingRequest, ct);
        if (!routed.IsSuccess)
            return routed.MapError<OrchestrationTaskDecompositionResult>();

        var planned = _phasePlanner.PlanPhases(request.Prompt, request.StepHints);
        if (!planned.IsSuccess)
            return planned.MapError<OrchestrationTaskDecompositionResult>();

        var phaseTags = planned.Value!
            .OrderBy(PhaseIndex)
            .ThenBy(tag => tag, StringComparer.Ordinal)
            .ToArray();
        var phases = BuildPhases(phaseTags, request.StepHints);
        var result = new OrchestrationTaskDecompositionResult(
            request.RequestId,
            request.ScenarioId,
            routed.Value!.Decision.ChosenUnitId,
            routed.Value.Manifest.TrainingRunId,
            routed.Value.Manifest.SourceDatasetSha256,
            phaseTags,
            phases,
            routed.Value.Decision.Confidence);

        return OperationResult<OrchestrationTaskDecompositionResult>.Ok(result);
    }

    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<OrchestrationTaskDecompositionRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return OperationResult<LogicalFlowResult>.Fail("Logical flow request is required.", "FLOW_REQUEST_REQUIRED");

        var decomposed = await DecomposeAsync(request.Input, cancellationToken);
        if (!decomposed.IsSuccess)
            return decomposed.MapError<LogicalFlowResult>();

        var value = decomposed.Value!;
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["chosen_unit_id"] = value.ChosenUnitId,
            ["training_run_id"] = value.TrainingRunId,
            ["source_dataset_sha256"] = value.SourceDatasetSha256,
            ["phase_count"] = value.Phases.Length.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["step_count"] = value.Phases.Sum(phase => phase.Steps.Length).ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["phase_tags"] = string.Join(";", value.PhaseTags),
            ["routing_confidence"] = value.RoutingConfidence.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture)
        };

        var summary = new LogicalFlowSummary(
            request.RunId,
            value.Phases.Length,
            0,
            value.RoutingConfidence,
            "routing_confidence",
            false,
            true,
            false,
            "ORCHESTRATION_TASK_DECOMPOSED",
            value.RoutingConfidence,
            value.ChosenUnitId,
            $"Decomposed orchestration scenario '{value.ScenarioId}' into {value.Phases.Length} ordered phases.",
            0,
            0);

        var finalHumanResult = string.Join(
            " | ",
            value.Phases.Select(phase => $"{phase.Order:00}:{phase.PhaseTag}({phase.Steps.Length})"));

        var flowResult = new LogicalFlowResult(
            request.FlowId,
            request.ScenarioId,
            "PASS",
            finalHumanResult,
            summary,
            metrics,
            [$"{value.RequestId}:decomposition:{value.ChosenUnitId}"],
            []);

        return OperationResult<LogicalFlowResult>.Ok(flowResult);
    }

    private static OrchestrationTaskPhase[] BuildPhases(
        string[] phaseTags,
        string[] stepHints)
    {
        var phases = phaseTags
            .Select((tag, index) => new OrchestrationTaskPhase(
                tag,
                index + 1,
                stepHints.Where(step => StepBelongsToPhase(step, tag)).ToArray()))
            .ToArray();

        var assigned = phases.SelectMany(phase => phase.Steps).ToHashSet(StringComparer.Ordinal);
        var unassigned = stepHints.Where(step => !assigned.Contains(step)).ToArray();
        if (unassigned.Length == 0)
            return phases;

        var fallbackIndex = Array.FindIndex(phases, phase => phase.PhaseTag == "technical-design-and-implementation-plan");
        if (fallbackIndex < 0)
            fallbackIndex = 0;

        phases[fallbackIndex] = phases[fallbackIndex] with
        {
            Steps = phases[fallbackIndex].Steps.Concat(unassigned).Distinct(StringComparer.Ordinal).ToArray()
        };
        return phases;
    }

    private static bool StepBelongsToPhase(string step, string phaseTag)
    {
        var lower = step.ToLowerInvariant();
        return phaseTag switch
        {
            "intake-source" => ContainsAny(lower, ["input", "source", "read", "load", "parse", "figma", "form", "audio", "text", "xml", "drawio", "examples"]),
            "rag-context-planning" => ContainsAny(lower, ["rag", "context", "previous", "retrieval", "memory"]),
            "screen-and-system-analysis" => ContainsAny(lower, ["screen", "component", "system", "theme", "style", "documentation"]),
            "architecture-flow-design" => ContainsAny(lower, ["architecture", "flow", "flows", "interface", "microservice", "dna", "pattern", "lifecycle", "entity", "generic"]),
            "model-round-execution" => ContainsAny(lower, ["ai", "model", "models", "round", "generation", "generator", "avatar", "music"]),
            "technical-design-and-implementation-plan" => ContainsAny(lower, ["technical", "tech", "implementation", "implement", "code", "plan", "swagger", "mcp", "schedule"]),
            "test-debug-deploy" => ContainsAny(lower, ["test", "debug", "deploy", "validate", "safe", "optimization", "ci cd"]),
            "feedback-memory-loop" => ContainsAny(lower, ["feedback", "store", "save", "daily", "whatsapp", "chat"]),
            "multi-technology-alternatives" => ContainsAny(lower, ["alternative", "language", "backend", "queue", "database", "hosting", "swappable"]),
            "skill-library-packaging" => ContainsAny(lower, ["skill", "library", "copilot", "claude", "cursor", "subfolder"]),
            _ => false
        };
    }

    private static bool ContainsAny(string text, string[] needles)
        => needles.Any(needle => text.Contains(needle, StringComparison.OrdinalIgnoreCase));

    private static int PhaseIndex(string phaseTag)
    {
        var index = Array.IndexOf(PhaseOrder, phaseTag);
        return index < 0 ? int.MaxValue : index;
    }
}

public sealed record OrchestrationTaskDecompositionRequest(
    string RequestId,
    string ScenarioId,
    string Prompt,
    string[] StepHints);

public sealed record OrchestrationTaskDecompositionResult(
    string RequestId,
    string ScenarioId,
    string ChosenUnitId,
    string TrainingRunId,
    string SourceDatasetSha256,
    string[] PhaseTags,
    OrchestrationTaskPhase[] Phases,
    double RoutingConfidence);

public sealed record OrchestrationTaskPhase(
    string PhaseTag,
    int Order,
    string[] Steps);
