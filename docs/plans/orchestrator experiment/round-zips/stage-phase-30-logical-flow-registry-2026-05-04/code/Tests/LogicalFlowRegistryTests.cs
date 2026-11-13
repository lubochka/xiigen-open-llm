using ExperimentsV2;
using ExperimentsV2.LogicalFlow;

namespace LlmFromScratch.Tests;

public sealed class LogicalFlowRegistryTests
{
    [Fact]
    public void LogicalFlowRegistry_RegistersAndResolvesT34FlowById()
    {
        // Create the registry for this test.
        var registry = new InMemoryLogicalFlowRegistry();

        // Register the T34 flow.
        var descriptor = RegisterT34Flow(registry, "1.0.0");

        // The registration must succeed through the shared envelope.
        Assert.True(descriptor.IsSuccess, descriptor.ErrorMessage);

        // Resolve the flow by its exact flow id.
        var resolved = registry.Resolve<string>(T34LogicalFlowAdapter.FlowId);

        // The registry must return the executable flow.
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);

        // The resolved flow must be the T34 adapter.
        Assert.IsType<T34LogicalFlowAdapter>(resolved.Value);
    }

    [Fact]
    public void LogicalFlowRegistry_ResolvesT34FlowByScenario()
    {
        // Create the registry for this test.
        var registry = new InMemoryLogicalFlowRegistry();

        // Register the T34 flow.
        RegisterT34Flow(registry, "1.0.0");

        // Resolve the flow by scenario id.
        var resolved = registry.ResolveByScenario<string>(T34LogicalFlowAdapter.ScenarioId);

        // The registry must return the executable flow.
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);

        // The resolved flow must be the T34 adapter.
        Assert.IsType<T34LogicalFlowAdapter>(resolved.Value);
    }

    [Fact]
    public void LogicalFlowRegistry_UnknownFlow_FailsCleanly()
    {
        // Create an empty registry.
        var registry = new InMemoryLogicalFlowRegistry();

        // Resolve a flow that was never registered.
        var resolved = registry.Resolve<string>("unknown-flow");

        // Missing flow ids must fail through the shared envelope.
        Assert.False(resolved.IsSuccess);

        // The failure must name the missing flow id.
        Assert.Contains("unknown-flow", resolved.ErrorMessage);
    }

    [Fact]
    public async Task LogicalFlowRegistry_BindingCanDriveInMemoryOrchestrator()
    {
        // Create the flow registry.
        var registry = new InMemoryLogicalFlowRegistry();

        // Register the T34 flow.
        RegisterT34Flow(registry, "1.0.0");

        // Resolve a binding by scenario id.
        var binding = registry.ResolveBindingByScenario(T34LogicalFlowAdapter.ScenarioId);

        // The binding must resolve.
        Assert.True(binding.IsSuccess, binding.ErrorMessage);

        // Feed the binding into the Phase 28 orchestrator shape.
        var orchestrator = new InMemoryLogicalOrchestrator(new Dictionary<string, LogicalFlowBinding>(StringComparer.Ordinal)
        {
            [T34LogicalFlowAdapter.ScenarioId] = binding.Value!
        });

        // Run the T34 scenario through the orchestrator.
        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "run-flow-registry-test",
            T34LogicalFlowAdapter.ScenarioId,
            "run T34",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // The orchestrator must succeed with the registry-provided binding.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The selected flow id must be T34.
        Assert.Equal(T34LogicalFlowAdapter.FlowId, result.Value!.SelectedFlowId);
    }

    [Fact]
    public void LogicalFlowRegistry_LatestScenarioRegistrationWins()
    {
        // Create the registry for this test.
        var registry = new InMemoryLogicalFlowRegistry();

        // Register v1 for the scenario.
        RegisterT34Flow(registry, "1.0.0");

        // Register v2 for the same scenario.
        registry.Register(
            "T34-PHASE-3-TO-4-TRANSITION-GATE-v2",
            T34LogicalFlowAdapter.ScenarioId,
            "2.0.0",
            new T34LogicalFlowAdapter());

        // Resolve the newest binding by scenario.
        var binding = registry.ResolveBindingByScenario(T34LogicalFlowAdapter.ScenarioId);

        // The binding must resolve.
        Assert.True(binding.IsSuccess, binding.ErrorMessage);

        // The latest flow id should be the v2 flow id.
        Assert.Equal("T34-PHASE-3-TO-4-TRANSITION-GATE-v2", binding.Value!.FlowId);
    }

    [Fact]
    public void LogicalFlowRegistry_InvalidRegistration_FailsCleanly()
    {
        // Create the registry for this test.
        var registry = new InMemoryLogicalFlowRegistry();

        // Try to register a flow without a scenario id.
        var result = registry.Register(
            T34LogicalFlowAdapter.FlowId,
            "",
            "1.0.0",
            new T34LogicalFlowAdapter());

        // Invalid registration must fail through the shared envelope.
        Assert.False(result.IsSuccess);

        // The failure must name the scenario id.
        Assert.Contains("Scenario id", result.ErrorMessage);
    }

    private static OperationResult<LogicalFlowDescriptor> RegisterT34Flow(InMemoryLogicalFlowRegistry registry, string version)
    {
        // Register the T34 flow under its scenario id.
        return registry.Register(
            T34LogicalFlowAdapter.FlowId,
            T34LogicalFlowAdapter.ScenarioId,
            version,
            new T34LogicalFlowAdapter());
    }
}
