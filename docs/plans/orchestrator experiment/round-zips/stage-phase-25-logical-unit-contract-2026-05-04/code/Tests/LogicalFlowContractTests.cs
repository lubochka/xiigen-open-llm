using ExperimentsV2;
using ExperimentsV2.LogicalFlow;
using System.Reflection;

namespace LlmFromScratch.Tests;

public sealed class LogicalFlowContractTests
{
    [Fact]
    public void LogicalExecutionFlags_Defaults_EnableOptimizationFlags()
    {
        // Create default flags exactly as a product path or normal experiment will receive them.
        var flags = new LogicalExecutionFlags();

        // The semantic compiler path is the first default-on optimization.
        Assert.True(flags.UseCompiledPredicateRepresentation);

        // Domain-scoped retrieval is default-on because E51A proved it prevents wrong-pool confidence.
        Assert.True(flags.UseDomainScopedRetrieval);

        // The confidence floor is default-on because low-confidence retrieval must not look authoritative.
        Assert.True(flags.UseConfidenceFloor);

        // Learned units may be used once they have passed their conformance gate.
        Assert.True(flags.UseLearnedUnitWhenAvailable);

        // Trace evidence is default-on so every run remains auditable.
        Assert.True(flags.UseTraceEvidence);

        // Phase outputs should carry forward unless a transition-failure experiment disables them.
        Assert.True(flags.UseContextCarryForward);

        // Idle learning scheduling is default-on so accepted feedback can become local learning material.
        Assert.True(flags.UseIdleLearningScheduling);
    }

    [Fact]
    public void LogicalExecutionFlags_Defaults_EnableSafetyFlags()
    {
        // Create default flags exactly as product paths should receive them.
        var flags = new LogicalExecutionFlags();

        // Arbiter authority must be active in normal paths.
        Assert.True(flags.UseArbiterAuthority);

        // Knowledge-edge detection must be active so missing competence is named.
        Assert.True(flags.UseKnowledgeEdgeDetection);

        // Internal-first execution must be active before any external AI request.
        Assert.True(flags.UseInternalFirstExecution);

        // External AI approval must be required by default.
        Assert.True(flags.RequireExternalAiApproval);

        // Arbiter contradictions must be rejected for internal and external outputs.
        Assert.True(flags.RejectArbiterContradictions);
    }

    [Fact]
    public void LogicalExecutionFlags_Defaults_SetExperimentControlFlags()
    {
        // Create default flags exactly as a replacement experiment will receive them.
        var flags = new LogicalExecutionFlags();

        // Fixture baseline is off unless a round explicitly asks for an oracle fixture comparison.
        Assert.False(flags.UseDeterministicFixtureBaseline);

        // Evidence writing is on so every accepted round can produce artifacts.
        Assert.True(flags.WriteEvidenceArtifacts);

        // Oracle comparison is on so replacement units cannot drift silently.
        Assert.True(flags.CompareAgainstOracle);

        // Metric drift fails the experiment by default.
        Assert.True(flags.FailOnMetricDrift);
    }

    [Fact]
    public void LogicalExecutionFlags_CategoryLists_AreCompleteAndDisjoint()
    {
        // Optimization flags should have the seven proven performance and routing behaviors.
        Assert.Equal(7, LogicalExecutionFlags.OptimizationFlagNames.Count);

        // Safety flags should have the five mandatory authority and approval behaviors.
        Assert.Equal(5, LogicalExecutionFlags.SafetyFlagNames.Count);

        // Experiment-control flags should have the four proof-mode behaviors.
        Assert.Equal(4, LogicalExecutionFlags.ExperimentControlFlagNames.Count);

        // Combine all names so overlaps can be detected by comparing total and distinct counts.
        var allNames = LogicalExecutionFlags.OptimizationFlagNames
            .Concat(LogicalExecutionFlags.SafetyFlagNames)
            .Concat(LogicalExecutionFlags.ExperimentControlFlagNames)
            .ToArray();

        // No flag may belong to two categories.
        Assert.Equal(allNames.Length, allNames.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public async Task LogicalUnitContract_DeterministicUnit_CanReturnTaskFromResult()
    {
        // Create a deterministic unit that does no live model work.
        var unit = new EchoLogicalUnit();

        // Build a request with default flags and simple metadata.
        var request = new LogicalUnitRequest<string>(
            "run-phase-25",
            "unit-echo",
            "contract_probe",
            "logical-flow-contract",
            "hello",
            new LogicalExecutionFlags(),
            new Dictionary<string, object> { ["source"] = "phase25" });

        // Execute through the async interface even though the implementation is deterministic.
        var result = await unit.ExecuteAsync(request);

        // The operation envelope must report success.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The unit output should preserve the request payload.
        Assert.Equal("hello", result.Value!.Output);

        // The deterministic unit should still provide a confidence score.
        Assert.Equal(1.0, result.Value.Confidence);

        // The unit should carry predicate evidence in the shared output envelope.
        Assert.Contains("RETURN", result.Value.PredicateIds);
    }

    [Fact]
    public void LogicalUnitContract_ExecuteAsync_HasCancellationToken()
    {
        // Inspect the interface method so future changes cannot remove the async production path.
        var method = typeof(ILogicalUnit<string, string>).GetMethod(nameof(ILogicalUnit<string, string>.ExecuteAsync));

        // The method must exist under the async contract name.
        Assert.NotNull(method);

        // The return type must be Task<OperationResult<LogicalUnitResult<TOutput>>>.
        Assert.Equal(typeof(Task<OperationResult<LogicalUnitResult<string>>>), method!.ReturnType);

        // The last parameter must be a cancellation token for future LLM-backed units.
        Assert.Equal(typeof(CancellationToken), method.GetParameters().Last().ParameterType);
    }

    private sealed class EchoLogicalUnit : ILogicalUnit<string, string>
    {
        // Deterministic units can satisfy the async contract without doing live asynchronous work.
        public Task<OperationResult<LogicalUnitResult<string>>> ExecuteAsync(LogicalUnitRequest<string> request, CancellationToken cancellationToken = default)
        {
            // Return cancellation through the shared operation envelope if the caller cancels before execution.
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(OperationResult<LogicalUnitResult<string>>.Fail("Cancelled"));

            // Build the logical unit output with predicate evidence and no warnings.
            var output = new LogicalUnitResult<string>(
                request.UnitId,
                request.UnitRole,
                request.DomainTag,
                request.Input,
                1.0,
                ["RETURN"],
                [request.DomainTag],
                [request.RunId],
                []);

            // Return the deterministic result through Task.FromResult to preserve one interface shape.
            return Task.FromResult(OperationResult<LogicalUnitResult<string>>.Ok(output));
        }
    }
}
