using ExperimentsV2;
using ExperimentsV2.LogicalFlow;

namespace LlmFromScratch.Tests;

public sealed class LogicalOrchestratorTests
{
    [Fact]
    public async Task InMemoryLogicalOrchestrator_T34Scenario_SelectsT34Flow()
    {
        // Create an orchestrator with the T34 flow bound to the T34 scenario.
        var orchestrator = BuildOrchestrator();

        // Run the T34 scenario through the assistant-level surface.
        var result = await orchestrator.RunAsync(BuildRequest(T34LogicalFlowAdapter.ScenarioId));

        // The orchestrator must succeed through the shared operation envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The selected flow must be the T34 flow.
        Assert.Equal(T34LogicalFlowAdapter.FlowId, result.Value!.SelectedFlowId);

        // The scenario id must be preserved in the assistant-level result.
        Assert.Equal(T34LogicalFlowAdapter.ScenarioId, result.Value.ScenarioId);
    }

    [Fact]
    public async Task InMemoryLogicalOrchestrator_T34Scenario_ReturnsPassWithGaps()
    {
        // Create an orchestrator with the T34 flow.
        var orchestrator = BuildOrchestrator();

        // Run the accepted T34 scenario.
        var result = await orchestrator.RunAsync(BuildRequest(T34LogicalFlowAdapter.ScenarioId));

        // The run must succeed before verdict checks.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The assistant-level verdict must match the T34 flow verdict.
        Assert.Equal("PASS_WITH_GAPS", result.Value!.OverallVerdict);

        // The final result must include the known T34 concern language.
        Assert.Contains("BaseService", result.Value.FinalHumanResult);

        // The final result must include the known cross-phase concern language.
        Assert.Contains("cross-phase context", result.Value.FinalHumanResult, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InMemoryLogicalOrchestrator_DefaultFlags_AreApplied()
    {
        // Create an orchestrator with the T34 flow.
        var orchestrator = BuildOrchestrator();

        // Build a request without explicit flags so defaults must be applied.
        var result = await orchestrator.RunAsync(BuildRequest(T34LogicalFlowAdapter.ScenarioId, flags: null));

        // The run must succeed before applied flags are checked.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The compiled predicate representation flag must default on.
        Assert.True(result.Value!.AppliedFlags.UseCompiledPredicateRepresentation);

        // Arbiter authority must default on.
        Assert.True(result.Value.AppliedFlags.UseArbiterAuthority);

        // Oracle comparison must default on.
        Assert.True(result.Value.AppliedFlags.CompareAgainstOracle);
    }

    [Fact]
    public async Task InMemoryLogicalOrchestrator_UnknownScenario_FailsCleanly()
    {
        // Create an orchestrator with only the T34 scenario.
        var orchestrator = BuildOrchestrator();

        // Run an unknown scenario.
        var result = await orchestrator.RunAsync(BuildRequest("unknown-scenario"));

        // Unknown scenarios must fail instead of falling through to T34.
        Assert.False(result.IsSuccess);

        // The failure must name the missing flow registration.
        Assert.Contains("No logical flow registered", result.ErrorMessage);
    }

    [Fact]
    public async Task InMemoryLogicalOrchestrator_EmptyRequest_FailsCleanly()
    {
        // Create an orchestrator with the T34 flow.
        var orchestrator = BuildOrchestrator();

        // Build an empty request for the known scenario.
        var request = BuildRequest(T34LogicalFlowAdapter.ScenarioId, userRequest: " ");

        // Run the empty request.
        var result = await orchestrator.RunAsync(request);

        // Empty requests must fail through the shared envelope.
        Assert.False(result.IsSuccess);

        // The failure must name the empty user request.
        Assert.Contains("cannot be empty", result.ErrorMessage);
    }

    [Fact]
    public async Task InMemoryLogicalOrchestrator_CanBeCancelled()
    {
        // Create an orchestrator with the T34 flow.
        var orchestrator = BuildOrchestrator();

        // Create a cancellation source that is already cancelled.
        using var cts = new CancellationTokenSource();

        // Cancel before execution.
        cts.Cancel();

        // Execute the known scenario with cancellation.
        var result = await orchestrator.RunAsync(BuildRequest(T34LogicalFlowAdapter.ScenarioId), cts.Token);

        // Cancellation must fail through the shared envelope.
        Assert.False(result.IsSuccess);

        // The failure must name cancellation.
        Assert.Contains("Cancelled", result.ErrorMessage);
    }

    private static InMemoryLogicalOrchestrator BuildOrchestrator()
    {
        // Bind the T34 scenario to the T34 logical-flow adapter.
        var bindings = new Dictionary<string, LogicalFlowBinding>(StringComparer.Ordinal)
        {
            [T34LogicalFlowAdapter.ScenarioId] = new(T34LogicalFlowAdapter.FlowId, new T34LogicalFlowAdapter())
        };

        // Return the in-memory orchestrator for this phase.
        return new InMemoryLogicalOrchestrator(bindings);
    }

    private static LogicalOrchestratorRequest BuildRequest(string scenarioId, string userRequest = "run T34 transition", LogicalExecutionFlags? flags = null)
    {
        // Build a standard assistant-level request.
        return new LogicalOrchestratorRequest(
            "run-logical-orchestrator-test",
            scenarioId,
            userRequest,
            flags,
            new Dictionary<string, object> { ["source"] = "phase28" });
    }
}
