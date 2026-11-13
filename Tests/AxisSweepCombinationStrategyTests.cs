// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class AxisSweepCombinationStrategyTests
{
    private const string RequestRequiredCode = "DYNAMIC_GRAPH_COMBINATION_STRATEGY_REQUEST_REQUIRED";
    private const string RequestInvalidCode = "DYNAMIC_GRAPH_COMBINATION_STRATEGY_REQUEST_INVALID";
    private const string CountOutOfRangeCode = "DYNAMIC_GRAPH_COMBINATION_STRATEGY_COUNT_OUT_OF_RANGE";
    private const string BudgetInvalidCode = "DYNAMIC_GRAPH_COMBINATION_STRATEGY_BUDGET_INVALID";
    private const string AxisInvalidCode = "DYNAMIC_GRAPH_COMBINATION_STRATEGY_AXIS_INVALID";

    [Fact]
    public void Interface_HasExactSynchronousEnumerationBoundary()
    {
        var method = Assert.Single(typeof(IGraphCombinationStrategy).GetMethods());

        Assert.Equal(nameof(IGraphCombinationStrategy.Enumerate), method.Name);
        Assert.Equal(typeof(OperationResult<IReadOnlyList<CombinationPlan>>), method.ReturnType);
        Assert.Equal([typeof(GraphCandidateRunRequest)], method.GetParameters().Select(value => value.ParameterType));
        Assert.Equal([typeof(IGraphCombinationStrategy)], typeof(AxisSweepCombinationStrategy).GetInterfaces());
        Assert.Empty(typeof(AxisSweepCombinationStrategy).GetFields(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    [Fact]
    public void Enumerate_FourProfilesHaveStableOrderFlagsAndKnownHashIdentity()
    {
        var result = new AxisSweepCombinationStrategy().Enumerate(CreateRequest(4));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var plans = Assert.IsAssignableFrom<IReadOnlyList<CombinationPlan>>(result.Value);
        Assert.Equal(4, plans.Count);
        Assert.Equal([0, 1, 2, 3], plans.Select(plan => plan.Index));
        Assert.Equal(4, plans.Select(plan => plan.CombinationId).Distinct(StringComparer.Ordinal).Count());
        Assert.All(plans, plan =>
        {
            Assert.Equal(["node-a"], plan.NodeSetIds);
            Assert.Equal("edge-a", plan.EdgeOrderKey);
            Assert.True(plan.Flags.UseDeterministicFixtureBaseline);
        });

        Assert.Equal(
            [
                "combination-06dd1a1841e0da256f15e7f0471c8d3d81269ff7573562baa78c7ea289588d0a",
                "combination-7892649006adbf4d10ee29994d8cf4c1713977af6c1f5c155e64934abb8cc0d1",
                "combination-3785420d5bb4a8987d20245d3e52b39483c8165d835f5dad9f8033be76738755",
                "combination-2731e181a1bddb54d885eff36dc89069f2a7af55a261853964e515c90c5b6726"
            ],
            plans.Select(plan => plan.CombinationId));
        AssertProfile(plans[0].Flags, carry: false, e35: false, lessonSignal: false);
        AssertProfile(plans[1].Flags, carry: true, e35: false, lessonSignal: false);
        AssertProfile(plans[2].Flags, carry: true, e35: true, lessonSignal: false);
        AssertProfile(plans[3].Flags, carry: true, e35: true, lessonSignal: true);
    }

    [Fact]
    public void Enumerate_UnicodeAxesUseUtf8ByteLengthsInKnownHashIdentity()
    {
        var result = new AxisSweepCombinationStrategy().Enumerate(CreateRequest(4) with
        {
            InputUnitRefs = ["node-😀"],
            InputGraphRefs = ["edge-雪"]
        });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            "combination-05543d4af9d850d627119d7b7ed7772b987e3cbc016bdd1dd1525cb06fb7c6eb",
            result.Value![0].CombinationId);
        Assert.Equal("node-😀", result.Value[0].NodeSetIds[0]);
        Assert.Equal("edge-雪", result.Value[0].EdgeOrderKey);
    }

    [Fact]
    public void Enumerate_ShuffledDuplicateAxesProduceIdenticalPlans()
    {
        var strategy = new AxisSweepCombinationStrategy();
        var first = strategy.Enumerate(CreateRequest(8) with
        {
            InputUnitRefs = ["node-b", "node-a", "node-b"],
            InputGraphRefs = ["edge-b", "edge-a", "edge-b"]
        });
        var second = strategy.Enumerate(CreateRequest(8) with
        {
            InputUnitRefs = ["node-a", "node-b"],
            InputGraphRefs = ["edge-a", "edge-b"]
        });

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(
            second.Value!.Select(Snapshot),
            first.Value!.Select(Snapshot));
    }

    [Fact]
    public void Enumerate_UsesNodeThenEdgeThenFlagProfileAxisOrder()
    {
        var result = new AxisSweepCombinationStrategy().Enumerate(CreateRequest(8) with
        {
            InputUnitRefs = ["node-b", "node-a"],
            InputGraphRefs = ["edge-y", "edge-x"]
        });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            [
                "node-a|edge-x|000",
                "node-a|edge-x|100",
                "node-a|edge-x|110",
                "node-a|edge-x|111",
                "node-a|edge-y|000",
                "node-a|edge-y|100",
                "node-a|edge-y|110",
                "node-a|edge-y|111"
            ],
            result.Value!.Select(plan =>
                $"{plan.NodeSetIds[0]}|{plan.EdgeOrderKey}|"
                + $"{Bit(plan.Flags.UseContextCarryForward)}"
                + $"{Bit(plan.Flags.UseE35DependencyDetection)}"
                + $"{Bit(plan.Flags.UseLowConfidenceLessonSignal)}"));
    }

    [Fact]
    public void Enumerate_WhenRequestedSpaceExceedsUniqueTuplesReturnsOnlyUniquePlansWithoutPadding()
    {
        var result = new AxisSweepCombinationStrategy().Enumerate(CreateRequest(8));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(4, result.Value!.Count);
        Assert.Equal([0, 1, 2, 3], result.Value.Select(plan => plan.Index));
        Assert.Equal(4, result.Value.Select(plan => Snapshot(plan)).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Enumerate_ReturnsOwnedSnapshots()
    {
        var request = CreateRequest(4);
        var result = new AxisSweepCombinationStrategy().Enumerate(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        request.InputUnitRefs[0] = "mutated-node";
        request.InputGraphRefs[0] = "mutated-edge";
        var firstNodes = result.Value![0].NodeSetIds;
        firstNodes[0] = "mutated-plan-node";

        Assert.Equal("node-a", result.Value[1].NodeSetIds[0]);
        Assert.Equal("edge-a", result.Value[1].EdgeOrderKey);
        Assert.NotSame(result.Value[0].NodeSetIds, result.Value[1].NodeSetIds);
        Assert.NotSame(result.Value[0].Flags, result.Value[1].Flags);
    }

    [Fact]
    public void Enumerate_RejectsMissingOrMalformedRequestWithStableCodes()
    {
        var strategy = new AxisSweepCombinationStrategy();
        var valid = CreateRequest(4);
        var cases = new (GraphCandidateRunRequest Request, string Code)[]
        {
            (null!, RequestRequiredCode),
            (valid with { ScenarioId = " " }, RequestInvalidCode),
            (valid with { Query = " " }, RequestInvalidCode),
            (valid with { LegalScope = " " }, RequestInvalidCode),
            (valid with { TraceRef = " " }, RequestInvalidCode),
            (valid with { ProvenanceRefs = [] }, RequestInvalidCode),
            (valid with { ProvenanceRefs = [" "] }, RequestInvalidCode)
        };

        foreach (var testCase in cases)
            AssertFailure(strategy.Enumerate(testCase.Request), testCase.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(9)]
    public void Enumerate_RejectsCombinationCountOutsideTwoThroughEight(int count)
    {
        var request = CreateRequest(4) with { CombinationCount = count };

        AssertFailure(
            new AxisSweepCombinationStrategy().Enumerate(request),
            CountOutOfRangeCode);
    }

    [Fact]
    public void Enumerate_RejectsInvalidBudgetWithStableCode()
    {
        var strategy = new AxisSweepCombinationStrategy();
        var valid = CreateRequest(4);
        var cases = new[]
        {
            valid with { Budget = null! },
            valid with { Budget = valid.Budget with { MaxCombinations = 3 } },
            valid with { Budget = valid.Budget with { MaxCombinations = 9 } },
            valid with { Budget = valid.Budget with { MaxParallelBranches = 0 } },
            valid with { Budget = valid.Budget with { MaxParallelCombinationRuns = 0 } },
            valid with { Budget = valid.Budget with { MaxParallelCombinationRuns = 5 } },
            valid with { Budget = valid.Budget with { Timeout = TimeSpan.Zero } }
        };

        foreach (var request in cases)
            AssertFailure(strategy.Enumerate(request), BudgetInvalidCode);
    }

    [Fact]
    public void Enumerate_RejectsEmptyNullOrBlankAxesWithStableCode()
    {
        var strategy = new AxisSweepCombinationStrategy();
        var valid = CreateRequest(4);
        var cases = new[]
        {
            valid with { InputUnitRefs = null! },
            valid with { InputUnitRefs = [] },
            valid with { InputUnitRefs = [" "] },
            valid with { InputGraphRefs = null! },
            valid with { InputGraphRefs = [] },
            valid with { InputGraphRefs = [" "] }
        };

        foreach (var request in cases)
            AssertFailure(strategy.Enumerate(request), AxisInvalidCode);
    }

    private static GraphCandidateRunRequest CreateRequest(int count)
        => new(
            ScenarioId: "scenario-a",
            Query: "query-a",
            InputGraphRefs: ["edge-a"],
            InputUnitRefs: ["node-a"],
            Budget: new AsyncCombinationBudget(
                MaxCombinations: count,
                Timeout: null,
                MaxParallelBranches: 2,
                MaxParallelCombinationRuns: Math.Min(2, count)),
            CombinationCount: count,
            Cancellation: CancellationToken.None,
            LegalScope: "legal-scope-a",
            ProvenanceRefs: ["provenance-a"],
            TraceRef: "trace-a");

    private static string Snapshot(CombinationPlan plan)
        => $"{plan.Index}|{plan.CombinationId}|{plan.NodeSetIds[0]}|{plan.EdgeOrderKey}|"
           + $"{Bit(plan.Flags.UseContextCarryForward)}"
           + $"{Bit(plan.Flags.UseE35DependencyDetection)}"
           + $"{Bit(plan.Flags.UseLowConfidenceLessonSignal)}";

    private static char Bit(bool value) => value ? '1' : '0';

    private static void AssertProfile(
        LogicalExecutionFlags flags,
        bool carry,
        bool e35,
        bool lessonSignal)
    {
        Assert.Equal(carry, flags.UseContextCarryForward);
        Assert.Equal(e35, flags.UseE35DependencyDetection);
        Assert.Equal(lessonSignal, flags.UseLowConfidenceLessonSignal);
        Assert.True(flags.UseDeterministicFixtureBaseline);
    }

    private static void AssertFailure<T>(OperationResult<T> result, string expectedCode)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }
}
