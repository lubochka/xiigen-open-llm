namespace ExperimentsV2.LogicalFlow;

// This record describes one registered logical flow.
public sealed record LogicalFlowDescriptor(
    string FlowId,
    string ScenarioId,
    string Version,
    Type InputType,
    long RegistrationOrder);

// This registry is the first local storage surface for logical flows.
public sealed class InMemoryLogicalFlowRegistry
{
    // The flow table stores descriptors and executable flow objects together.
    private readonly List<(LogicalFlowDescriptor Descriptor, object Flow)> _entries = [];

    // This counter preserves registration order for latest-flow resolution.
    private long _nextRegistrationOrder;

    // Register one logical flow for one scenario.
    public OperationResult<LogicalFlowDescriptor> Register<TInput>(string flowId, string scenarioId, string version, ILogicalFlow<TInput> flow)
    {
        // Reject empty flow ids because callers cannot select an unnamed topology.
        if (string.IsNullOrWhiteSpace(flowId))
            return OperationResult<LogicalFlowDescriptor>.Fail("Flow id cannot be empty.");

        // Reject empty scenario ids because scenario selection is the current flow lookup key.
        if (string.IsNullOrWhiteSpace(scenarioId))
            return OperationResult<LogicalFlowDescriptor>.Fail("Scenario id cannot be empty.");

        // Reject empty versions because future rollback and topology updates need version identity.
        if (string.IsNullOrWhiteSpace(version))
            return OperationResult<LogicalFlowDescriptor>.Fail("Version cannot be empty.");

        // Build the descriptor for audit and later registry-driven selection.
        var descriptor = new LogicalFlowDescriptor(
            flowId,
            scenarioId,
            version,
            typeof(TInput),
            ++_nextRegistrationOrder);

        // Store descriptor and executable flow together.
        _entries.Add((descriptor, flow));

        // Return the descriptor for tests and evidence.
        return OperationResult<LogicalFlowDescriptor>.Ok(descriptor);
    }

    // Resolve a flow by exact flow id and input type.
    public OperationResult<ILogicalFlow<TInput>> Resolve<TInput>(string flowId)
    {
        // Find the newest matching flow id and input type.
        var entry = _entries
            .Where(candidate => candidate.Descriptor.FlowId == flowId && candidate.Descriptor.InputType == typeof(TInput))
            .OrderByDescending(candidate => candidate.Descriptor.RegistrationOrder)
            .FirstOrDefault();

        // Return a clean failure when no flow matches.
        if (entry.Flow is null)
            return OperationResult<ILogicalFlow<TInput>>.Fail($"No logical flow registered for id '{flowId}'.");

        // Return the executable flow under its typed interface.
        return OperationResult<ILogicalFlow<TInput>>.Ok((ILogicalFlow<TInput>)entry.Flow);
    }

    // Resolve a flow by scenario id and input type.
    public OperationResult<ILogicalFlow<TInput>> ResolveByScenario<TInput>(string scenarioId)
    {
        // Find the newest flow for the requested scenario id and input type.
        var entry = _entries
            .Where(candidate => candidate.Descriptor.ScenarioId == scenarioId && candidate.Descriptor.InputType == typeof(TInput))
            .OrderByDescending(candidate => candidate.Descriptor.RegistrationOrder)
            .FirstOrDefault();

        // Return a clean failure when no flow matches the scenario.
        if (entry.Flow is null)
            return OperationResult<ILogicalFlow<TInput>>.Fail($"No logical flow registered for scenario '{scenarioId}'.");

        // Return the executable flow under its typed interface.
        return OperationResult<ILogicalFlow<TInput>>.Ok((ILogicalFlow<TInput>)entry.Flow);
    }

    // Resolve a scenario binding for the Phase 28 in-memory orchestrator.
    public OperationResult<LogicalFlowBinding> ResolveBindingByScenario(string scenarioId)
    {
        // Find the newest string-input flow for the requested scenario.
        var entry = _entries
            .Where(candidate => candidate.Descriptor.ScenarioId == scenarioId && candidate.Descriptor.InputType == typeof(string))
            .OrderByDescending(candidate => candidate.Descriptor.RegistrationOrder)
            .FirstOrDefault();

        // Return a clean failure when no compatible binding exists.
        if (entry.Flow is null)
            return OperationResult<LogicalFlowBinding>.Fail($"No string-input logical flow registered for scenario '{scenarioId}'.");

        // Return the binding shape the in-memory orchestrator already understands.
        return OperationResult<LogicalFlowBinding>.Ok(new LogicalFlowBinding(entry.Descriptor.FlowId, (ILogicalFlow<string>)entry.Flow));
    }

    // Return descriptors for audit and tests.
    public IReadOnlyList<LogicalFlowDescriptor> ListDescriptors()
    {
        // Project descriptors into a read-only array.
        return _entries.Select(entry => entry.Descriptor).ToArray();
    }
}
