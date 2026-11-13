namespace OrchestratorModel.Logical;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ExperimentsV2.LogicalFlow;

// This record is the runtime-facing answer returned by the logical orchestrator assistant.
public sealed record LogicalAssistantRunResult(
    string ScenarioId,
    string SelectedFlowId,
    string OverallVerdict,
    string FinalHumanResult,
    LogicalExecutionFlags AppliedFlags,
    IReadOnlyDictionary<string, string> Metrics,
    string[] EvidenceIds);

// This service is the first production-adjacent wrapper around the logical orchestrator.
public sealed class LogicalOrchestratorAssistant : BaseService
{
    // This version labels the first experiment-backed flow registration.
    public const string DefaultFlowVersion = "1.0.0";

    // This source value marks assistant-created logical runs in metadata.
    public const string RunSource = "logical_orchestrator_assistant";

    // The orchestrator is nullable so invalid construction can fail through RunAsync instead of throwing.
    private readonly ILogicalOrchestrator? _orchestrator;

    // The topology mutation flow is nullable so command routing can fail without hardcoded fallbacks.
    private readonly ILogicalFlow<LogicalTopologyCommand>? _topologyMutationFlow;

    // The default scenario keeps the current assistant bound to the proven T34 transition flow.
    public string DefaultScenarioId { get; }

    // Create an assistant from an already-built logical orchestrator.
    public LogicalOrchestratorAssistant(
        ISessionStorage storage,
        ILogicalOrchestrator? orchestrator,
        string defaultScenarioId = T34LogicalFlowAdapter.ScenarioId,
        ILogicalFlow<LogicalTopologyCommand>? topologyMutationFlow = null)
        : base(storage)
    {
        // Store the orchestrator so RunAsync can execute the selected logical flow.
        _orchestrator = orchestrator;

        // Store the topology mutation flow for typed topology commands.
        _topologyMutationFlow = topologyMutationFlow;

        // Fall back to the proven T34 scenario when the caller gives no useful default.
        DefaultScenarioId = string.IsNullOrWhiteSpace(defaultScenarioId)
            ? T34LogicalFlowAdapter.ScenarioId
            : defaultScenarioId;
    }

    // Build the default assistant with the T34 and CB4 logical flows registered in memory.
    public static OperationResult<LogicalOrchestratorAssistant> CreateDefault(ISessionStorage storage)
    {
        // Reject missing storage through the project result envelope.
        if (storage is null)
            return OperationResult<LogicalOrchestratorAssistant>.Fail("Session storage is required.", "STORAGE_REQUIRED");

        // Create the flow registry used by this first runtime-ready wrapper.
        var registry = new InMemoryLogicalFlowRegistry();

        // Register the T34 adapter as the default executable logical flow.
        var t34Registration = registry.Register(
            T34LogicalFlowAdapter.FlowId,
            T34LogicalFlowAdapter.ScenarioId,
            DefaultFlowVersion,
            new T34LogicalFlowAdapter());

        // Return registry failures through the project result envelope.
        if (!t34Registration.IsSuccess)
            return OperationResult<LogicalOrchestratorAssistant>.Fail(t34Registration.ErrorMessage, "FLOW_REGISTRATION_FAILED");

        // Register the CB4 security-verification flow as the first external-source Stage 5 route.
        var securityRegistration = registry.Register(
            SecurityVerification34FlowAdapter.FlowId,
            SecurityVerification34FlowAdapter.ScenarioId,
            DefaultFlowVersion,
            new SecurityVerification34FlowAdapter());

        // Return registry failures through the project result envelope.
        if (!securityRegistration.IsSuccess)
            return OperationResult<LogicalOrchestratorAssistant>.Fail(securityRegistration.ErrorMessage, "FLOW_REGISTRATION_FAILED");

        // Resolve the scenario binding expected by the in-memory logical orchestrator.
        var t34Binding = registry.ResolveBindingByScenario(T34LogicalFlowAdapter.ScenarioId);

        // Return binding failures through the project result envelope.
        if (!t34Binding.IsSuccess)
            return OperationResult<LogicalOrchestratorAssistant>.Fail(t34Binding.ErrorMessage, "FLOW_BINDING_FAILED");

        // Resolve the security scenario binding expected by the in-memory logical orchestrator.
        var securityBinding = registry.ResolveBindingByScenario(SecurityVerification34FlowAdapter.ScenarioId);

        // Return binding failures through the project result envelope.
        if (!securityBinding.IsSuccess)
            return OperationResult<LogicalOrchestratorAssistant>.Fail(securityBinding.ErrorMessage, "FLOW_BINDING_FAILED");

        // Build the scenario map for the first logical orchestrator instance.
        var scenarioBindings = new Dictionary<string, LogicalFlowBinding>(StringComparer.Ordinal)
        {
            // Bind the accepted T34 scenario id to the registered T34 logical flow.
            [T34LogicalFlowAdapter.ScenarioId] = t34Binding.Value!,

            // Bind the CB4 security scenario id to the registered security-verification flow.
            [SecurityVerification34FlowAdapter.ScenarioId] = securityBinding.Value!
        };

        // Create the in-memory orchestrator from the scenario map.
        var orchestrator = new InMemoryLogicalOrchestrator(scenarioBindings);

        // Create the runtime topology registry for assistant-driven topology mutations.
        var topologyRegistry = new InMemoryLogicalTopologyRegistry();

        // Create the add-domain mutation unit backed by the topology registry.
        var addDomainUnit = new AddDomainTopologyMutationUnit(topologyRegistry);

        // Create the add-phase mutation unit backed by the same topology registry.
        var addPhaseUnit = new AddPhaseTopologyMutationUnit(topologyRegistry);

        // Create the add-sub-phase mutation unit backed by the same topology registry.
        var addSubPhaseUnit = new AddSubPhaseTopologyMutationUnit(topologyRegistry);

        // Create the add-transition mutation unit backed by the same topology registry.
        var addTransitionUnit = new AddTransitionTopologyMutationUnit(topologyRegistry);

        // Create the topology mutation flow that accepts typed topology commands.
        var topologyMutationFlow = new TopologyMutationFlow(addDomainUnit, addPhaseUnit, addSubPhaseUnit, addTransitionUnit);

        // Return the assistant through the project result envelope.
        return OperationResult<LogicalOrchestratorAssistant>.Ok(new LogicalOrchestratorAssistant(storage, orchestrator, topologyMutationFlow: topologyMutationFlow));
    }

    // Run a user request through the logical orchestrator surface.
    public async Task<OperationResult<LogicalAssistantRunResult>> RunAsync(
        string userRequest,
        string? scenarioId = null,
        LogicalExecutionFlags? flags = null,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the project result envelope.
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalAssistantRunResult>.Fail("Cancelled.", "CANCELLED");

        // Reject empty user requests before creating a logical run.
        if (string.IsNullOrWhiteSpace(userRequest))
            return OperationResult<LogicalAssistantRunResult>.Fail("User request cannot be empty.", "EMPTY_REQUEST");

        // Reject invalid construction before trying to execute a flow.
        if (_orchestrator is null)
            return OperationResult<LogicalAssistantRunResult>.Fail("Logical orchestrator is not configured.", "ORCHESTRATOR_NOT_CONFIGURED");

        // Use the assistant default scenario unless the caller supplies a non-empty scenario id.
        var selectedScenarioId = string.IsNullOrWhiteSpace(scenarioId) ? DefaultScenarioId : scenarioId;

        // Create a traceable run id for this assistant invocation.
        var runId = $"assistant-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}";

        // Attach source metadata so evidence can distinguish runtime wrapper calls from direct tests.
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            // Mark the component that created this logical orchestrator request.
            ["source"] = RunSource,

            // Preserve the assistant's default scenario for audit.
            ["default_scenario_id"] = DefaultScenarioId
        };

        // Build the orchestrator request with caller flags or default-on flags.
        var request = new LogicalOrchestratorRequest(
            runId,
            selectedScenarioId,
            userRequest,
            flags,
            metadata);

        // Run the selected logical flow through the shared logical orchestrator.
        var orchestratorResult = await _orchestrator.RunAsync(request, cancellationToken);

        // Return orchestrator failures through the project result envelope.
        if (!orchestratorResult.IsSuccess)
            return OperationResult<LogicalAssistantRunResult>.Fail(orchestratorResult.ErrorMessage, "ORCHESTRATOR_FAILED");

        // Build the runtime-facing result from the logical orchestrator result.
        var assistantResult = new LogicalAssistantRunResult(
            orchestratorResult.Value!.ScenarioId,
            orchestratorResult.Value.SelectedFlowId,
            orchestratorResult.Value.OverallVerdict,
            orchestratorResult.Value.FinalHumanResult,
            orchestratorResult.Value.AppliedFlags,
            orchestratorResult.Value.FlowResult.Metrics,
            orchestratorResult.Value.FlowResult.EvidenceIds);

        // Return the runtime-facing assistant result through the project envelope.
        return OperationResult<LogicalAssistantRunResult>.Ok(assistantResult);
    }

    // Run a typed topology command through the topology mutation flow.
    public async Task<OperationResult<LogicalAssistantRunResult>> RunTopologyCommandAsync(
        LogicalTopologyCommand command,
        LogicalExecutionFlags? flags = null,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the project result envelope.
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalAssistantRunResult>.Fail("Cancelled.", "CANCELLED");

        // Reject missing topology commands before building the flow request.
        if (command is null)
            return OperationResult<LogicalAssistantRunResult>.Fail("Topology command is required.", "TOPOLOGY_COMMAND_REQUIRED");

        // Reject invalid construction before trying to mutate topology.
        if (_topologyMutationFlow is null)
            return OperationResult<LogicalAssistantRunResult>.Fail("Topology mutation flow is not configured.", "TOPOLOGY_FLOW_NOT_CONFIGURED");

        // Apply default flags unless the caller supplied explicit flags.
        var appliedFlags = flags ?? new LogicalExecutionFlags();

        // Create a traceable run id for this topology mutation invocation.
        var runId = $"assistant-topology-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}";

        // Attach source metadata so topology mutations are auditable.
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            // Mark the component that created this topology mutation request.
            ["source"] = RunSource,

            // Record the typed command name without interpreting natural language.
            ["command_type"] = command.GetType().Name
        };

        // Build the typed flow request for topology mutation.
        var request = new LogicalFlowRequest<LogicalTopologyCommand>(
            runId,
            TopologyMutationFlow.FlowId,
            TopologyMutationFlow.ScenarioId,
            command,
            appliedFlags,
            metadata);

        // Execute the topology mutation flow.
        var flowResult = await _topologyMutationFlow.ExecuteAsync(request, cancellationToken);

        // Preserve flow failures at the assistant boundary.
        if (!flowResult.IsSuccess)
            return OperationResult<LogicalAssistantRunResult>.Fail(flowResult.ErrorMessage, flowResult.ErrorCode);

        // Build the runtime-facing result from the topology flow result.
        var assistantResult = new LogicalAssistantRunResult(
            flowResult.Value!.ScenarioId,
            flowResult.Value.FlowId,
            flowResult.Value.OverallVerdict,
            flowResult.Value.FinalHumanResult,
            appliedFlags,
            flowResult.Value.Metrics,
            flowResult.Value.EvidenceIds);

        // Return the runtime-facing assistant result through the project envelope.
        return OperationResult<LogicalAssistantRunResult>.Ok(assistantResult);
    }
}
