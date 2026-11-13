// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using OrchestratorModel.Logical;

public sealed class DynamicGraphMachineHonestUntrainedTests
{
    [Fact]
    public void Contract_LivesOutsideCoreAndExposesHonestUntrainedState()
    {
        Assert.Equal("OrchestratorModel.Logical", typeof(DynamicGraphDomain).Namespace);
        Assert.Equal("OrchestratorModel.Logical", typeof(DynamicGraphMachine).Namespace);
        Assert.NotEqual(typeof(ILearningMachine<>).Assembly, typeof(DynamicGraphMachine).Assembly);
        Assert.Null(typeof(ILearningMachine<>).Assembly.GetType(
            "AgentInfrastructure.Logical.DynamicGraphMachine"));
        Assert.Equal("DynamicGraphDomain", DynamicGraphDomain.DomainTag);
        Assert.Equal(DynamicGraphDomain.DomainTag, DynamicGraphMachine.DomainTag);
        Assert.Equal("honest_untrained", DynamicGraphMachine.RuntimeMode);
        Assert.False(DynamicGraphMachine.SemanticQualityClaimed);

        var machine = CreateMachine((_) => throw new InvalidOperationException("must not run"));
        Assert.IsAssignableFrom<ILearningMachine<DynamicGraphDomain>>(machine);
    }

    [Fact]
    public async Task LifecycleMethods_FailWithMethodSpecificNotActivatedCodes_WithoutFactoryCalls()
    {
        var factoryCalls = 0;
        var machine = CreateMachine(_ =>
        {
            factoryCalls++;
            return Task.FromResult(OperationResult<ILogicalOrchestrator>.Ok(new StubOrchestrator()));
        });

        var train = await machine.TrainBatchAsync(new LearningBatch<DynamicGraphDomain>(
            "batch", DynamicGraphDomain.DomainTag, [], []));
        var evaluate = await machine.EvaluateAsync(new LearningEvaluationSet<DynamicGraphDomain>(
            "evaluation", DynamicGraphDomain.DomainTag, []));
        var export = await machine.ExportAsync();
        var import = await machine.ImportAsync(new LearningMachineExport(
            "machine", DynamicGraphDomain.DomainTag, "0.0.0", "checkpoint", "manifest", 0, DateTimeOffset.UnixEpoch));

        AssertFailure(train, "DYNAMIC_GRAPH_TRAINING_NOT_ACTIVATED", "Dynamic graph training is not activated.");
        AssertFailure(evaluate, "DYNAMIC_GRAPH_EVALUATION_NOT_ACTIVATED", "Dynamic graph evaluation is not activated.");
        AssertFailure(export, "DYNAMIC_GRAPH_EXPORT_NOT_ACTIVATED", "Dynamic graph export is not activated.");
        AssertFailure(import, "DYNAMIC_GRAPH_IMPORT_NOT_ACTIVATED", "Dynamic graph import is not activated.");
        Assert.Equal(0, factoryCalls);
    }

    [Fact]
    public async Task LifecycleMethods_UncancelledNullInputsStillReportNotActivated_WithoutFactoryCalls()
    {
        var factoryCalls = 0;
        var machine = CreateMachine(_ =>
        {
            factoryCalls++;
            return Task.FromResult(OperationResult<ILogicalOrchestrator>.Ok(new StubOrchestrator()));
        });

        var train = await machine.TrainBatchAsync(null!);
        var evaluate = await machine.EvaluateAsync(null!);
        var import = await machine.ImportAsync(null!);

        AssertFailure(train, "DYNAMIC_GRAPH_TRAINING_NOT_ACTIVATED", "Dynamic graph training is not activated.");
        AssertFailure(evaluate, "DYNAMIC_GRAPH_EVALUATION_NOT_ACTIVATED", "Dynamic graph evaluation is not activated.");
        AssertFailure(import, "DYNAMIC_GRAPH_IMPORT_NOT_ACTIVATED", "Dynamic graph import is not activated.");
        Assert.Equal(0, factoryCalls);
    }

    [Fact]
    public async Task LifecycleMethods_GiveCancellationPriority_WithoutFactoryCalls()
    {
        var factoryCalls = 0;
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var machine = CreateMachine(_ =>
        {
            factoryCalls++;
            return Task.FromResult(OperationResult<ILogicalOrchestrator>.Ok(new StubOrchestrator()));
        });

        var train = await machine.TrainBatchAsync(null!, cancellation.Token);
        var evaluate = await machine.EvaluateAsync(null!, cancellation.Token);
        var export = await machine.ExportAsync(cancellation.Token);
        var import = await machine.ImportAsync(null!, cancellation.Token);

        AssertCancelled(train);
        AssertCancelled(evaluate);
        AssertCancelled(export);
        AssertCancelled(import);
        Assert.Equal(0, factoryCalls);
    }

    [Fact]
    public async Task GetOrchestrator_ReturnsExactFactoryResultAndCallsFactoryOnce()
    {
        var factoryCalls = 0;
        var orchestrator = new StubOrchestrator();
        var upstream = OperationResult<ILogicalOrchestrator>.Ok(orchestrator);
        var machine = CreateMachine(ct =>
        {
            Assert.False(ct.IsCancellationRequested);
            factoryCalls++;
            return Task.FromResult(upstream);
        });

        var result = await machine.GetOrchestratorAsync();

        Assert.Same(upstream, result);
        Assert.Same(orchestrator, result.Value);
        Assert.Equal(1, factoryCalls);
    }

    [Fact]
    public async Task GetOrchestrator_ForwardsExactLiveSuppliedToken_Once()
    {
        using var cancellation = new CancellationTokenSource();
        var suppliedToken = cancellation.Token;
        var observedToken = default(CancellationToken);
        var factoryCalls = 0;
        var upstream = OperationResult<ILogicalOrchestrator>.Ok(new StubOrchestrator());
        var machine = CreateMachine(token =>
        {
            observedToken = token;
            factoryCalls++;
            return Task.FromResult(upstream);
        });

        Assert.NotEqual(default, suppliedToken);

        var result = await machine.GetOrchestratorAsync(suppliedToken);

        Assert.Same(upstream, result);
        Assert.Equal(suppliedToken, observedToken);
        Assert.Equal(1, factoryCalls);
    }

    [Fact]
    public async Task GetOrchestrator_PreCancelledTokenNeverCallsFactory()
    {
        var factoryCalls = 0;
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var machine = CreateMachine(_ =>
        {
            factoryCalls++;
            throw new InvalidOperationException("must not run");
        });

        var result = await machine.GetOrchestratorAsync(cancellation.Token);

        AssertCancelled(result);
        Assert.Equal(0, factoryCalls);
    }

    [Fact]
    public async Task GetOrchestrator_PreservesUpstreamFailure()
    {
        var upstream = OperationResult<ILogicalOrchestrator>.Fail("upstream message", "UPSTREAM_CODE");
        var machine = CreateMachine(_ => Task.FromResult(upstream));

        var result = await machine.GetOrchestratorAsync();

        Assert.Same(upstream, result);
        AssertFailure(result, "UPSTREAM_CODE", "upstream message");
    }

    [Fact]
    public async Task GetOrchestrator_NullResultFailsClosed()
    {
        var machine = CreateMachine(_ => Task.FromResult<OperationResult<ILogicalOrchestrator>>(null!));

        var result = await machine.GetOrchestratorAsync();

        AssertFailure(
            result,
            "DYNAMIC_GRAPH_ORCHESTRATOR_FACTORY_RESULT_REQUIRED",
            "Dynamic graph orchestrator factory must return a result.");
    }

    [Fact]
    public async Task GetOrchestrator_NullTaskFailsClosed()
    {
        var machine = CreateMachine(_ => null!);

        var result = await machine.GetOrchestratorAsync();

        AssertFailure(
            result,
            "DYNAMIC_GRAPH_ORCHESTRATOR_FACTORY_RESULT_REQUIRED",
            "Dynamic graph orchestrator factory must return a result.");
    }

    [Fact]
    public async Task GetOrchestrator_SuccessWithNullValueFailsClosed()
    {
        var machine = CreateMachine(_ => Task.FromResult(
            OperationResult<ILogicalOrchestrator>.Ok(null!)));

        var result = await machine.GetOrchestratorAsync();

        AssertFailure(
            result,
            "DYNAMIC_GRAPH_ORCHESTRATOR_REQUIRED",
            "Successful dynamic graph orchestrator factory result must contain an orchestrator.");
    }

    [Fact]
    public async Task GetOrchestrator_OrdinaryFactoryExceptionFailsClosed()
    {
        var machine = CreateMachine(_ => Task.FromException<OperationResult<ILogicalOrchestrator>>(
            new InvalidOperationException("unstable detail must not leak")));

        var result = await machine.GetOrchestratorAsync();

        AssertFailure(
            result,
            "DYNAMIC_GRAPH_ORCHESTRATOR_FACTORY_FAILED",
            "Dynamic graph orchestrator factory failed.");
    }

    [Fact]
    public async Task GetOrchestrator_CancelledSuppliedTokenMapsToStableCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var machine = CreateMachine(_ =>
        {
            cancellation.Cancel();
            return Task.FromException<OperationResult<ILogicalOrchestrator>>(
                new OperationCanceledException(cancellation.Token));
        });

        var result = await machine.GetOrchestratorAsync(cancellation.Token);

        AssertCancelled(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetOrchestrator_PostAwaitCancellationOverridesCompletedUpstreamResult(
        bool upstreamSucceeds)
    {
        using var cancellation = new CancellationTokenSource();
        var factoryEntered = new TaskCompletionSource<CancellationToken>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var factoryCompletion = new TaskCompletionSource<OperationResult<ILogicalOrchestrator>>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var factoryCalls = 0;
        var orchestrator = new StubOrchestrator();
        var upstream = upstreamSucceeds
            ? OperationResult<ILogicalOrchestrator>.Ok(orchestrator)
            : OperationResult<ILogicalOrchestrator>.Fail("upstream failure", "UPSTREAM_FAILURE");
        var machine = CreateMachine(token =>
        {
            factoryCalls++;
            factoryEntered.SetResult(token);
            return factoryCompletion.Task;
        });

        var pending = machine.GetOrchestratorAsync(cancellation.Token);
        var observedToken = await factoryEntered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(cancellation.Token, observedToken);
        Assert.False(pending.IsCompleted);

        cancellation.Cancel();
        factoryCompletion.SetResult(upstream);
        var result = await pending.WaitAsync(TimeSpan.FromSeconds(10));

        AssertCancelled(result);
        Assert.NotSame(upstream, result);
        Assert.NotSame(orchestrator, result.Value);
        Assert.Equal(1, factoryCalls);
    }

    [Fact]
    public async Task GetOrchestrator_CancellationHasPriorityWhenFactoryReturnsNoTask()
    {
        using var cancellation = new CancellationTokenSource();
        var machine = CreateMachine(_ =>
        {
            cancellation.Cancel();
            return null!;
        });

        var result = await machine.GetOrchestratorAsync(cancellation.Token);

        AssertCancelled(result);
    }

    [Fact]
    public async Task GetOrchestrator_CancellationHasPriorityOverOrdinaryFactoryException()
    {
        using var cancellation = new CancellationTokenSource();
        var machine = CreateMachine(_ =>
        {
            cancellation.Cancel();
            throw new InvalidOperationException("ordinary after cancellation");
        });

        var result = await machine.GetOrchestratorAsync(cancellation.Token);

        AssertCancelled(result);
    }

    [Fact]
    public async Task GetOrchestrator_UncancelledOperationCanceledExceptionPropagates()
    {
        var exception = new OperationCanceledException("upstream did not observe the supplied token");
        var machine = CreateMachine(_ => Task.FromException<OperationResult<ILogicalOrchestrator>>(exception));

        var thrown = await Assert.ThrowsAsync<OperationCanceledException>(
            () => machine.GetOrchestratorAsync());

        Assert.Same(exception, thrown);
    }

    [Fact]
    public async Task GetOrchestrator_NestedUncancelledCancellationPropagates()
    {
        var cancellation = new OperationCanceledException("nested cancellation");
        var exception = new AggregateException(
            new InvalidOperationException("ordinary"),
            new ApplicationException("wrapper", cancellation));
        var machine = CreateMachine(_ => Task.FromException<OperationResult<ILogicalOrchestrator>>(exception));

        var thrown = await Assert.ThrowsAsync<AggregateException>(
            () => machine.GetOrchestratorAsync());

        Assert.Same(exception, thrown);
    }

    [Fact]
    public async Task GetOrchestrator_DirectFatalExceptionPropagates()
    {
        var exception = new OutOfMemoryException("fatal");
        var machine = CreateMachine(_ => Task.FromException<OperationResult<ILogicalOrchestrator>>(exception));

        var thrown = await Assert.ThrowsAsync<OutOfMemoryException>(
            () => machine.GetOrchestratorAsync());

        Assert.Same(exception, thrown);
    }

    [Fact]
    public async Task GetOrchestrator_NestedFatalExceptionGraphPropagates()
    {
        var fatal = new OutOfMemoryException("fatal");
        var exception = new AggregateException(
            new InvalidOperationException("ordinary"),
            new ApplicationException("wrapper", fatal));
        var machine = CreateMachine(_ => Task.FromException<OperationResult<ILogicalOrchestrator>>(exception));

        var thrown = await Assert.ThrowsAsync<AggregateException>(
            () => machine.GetOrchestratorAsync());

        Assert.Same(exception, thrown);
    }

    private static DynamicGraphMachine CreateMachine(DynamicGraphOrchestratorFactory factory)
        => new(factory);

    private static void AssertFailure<T>(OperationResult<T> result, string code, string message)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(default(T), result.Value);
        Assert.Equal(code, result.ErrorCode);
        Assert.Equal(message, result.ErrorMessage);
    }

    private static void AssertCancelled<T>(OperationResult<T> result)
        => AssertFailure(result, "CANCELLED", "Dynamic graph machine operation was cancelled.");

    private sealed class StubOrchestrator : ILogicalOrchestrator
    {
        public Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
            LogicalOrchestratorRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException("The machine must return this instance without invoking it.");
    }
}
