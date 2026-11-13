// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using LlmFromScratch.DynamicGraphCompositionLiveLocalServer;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SmartSkillsDomain;

public sealed class DynamicGraphCompositionPromptToTaskUntrainedHonestyExecutionServiceTests
{
    private const string EvaluationId = "evaluation:l2-untrained:001";
    private const string RunId = "run:l2-untrained:001";
    private const string PromptId = "prompt:l2-untrained:001";
    private const string RawPrompt = "Create a deterministic local task.";
    private const string UserLanguage = "en";

    [Fact]
    public async Task DefaultService_ExecutesAndBindsExactUntrainedL2Measurement()
    {
        using var service = new PromptToTaskUntrainedHonestyExecutionService();

        var operation = await ExecuteAsync(service);

        Assert.True(operation.IsSuccess);
        var executed = Assert.IsType<PromptToTaskUntrainedHonestyExecutionResult>(
            operation.Value);
        Assert.Equal(PromptToTaskUntrainedHonestyExecutionStatus.Measured, executed.Status);
        Assert.Equal(1, executed.DirectUnitInvocationCount);
        var bound = Assert.IsType<DynamicGraphTargetUntrainedHonestyMeasurement>(
            executed.Measurement);
        Assert.Equal(PromptToTaskIntentUnit.UnitId, bound.Target.TargetId);
        Assert.Equal(DynamicGraphMeasurementTargetLevel.L2LogicalUnit, bound.Target.Level);
        Assert.Equal(
            DynamicGraphMeasurementMechanismKind.LearningSubject,
            bound.Target.MechanismKind);
        Assert.False(bound.Target.DeterministicFinalAuthority);

        var producer = bound.UntrainedHonesty.Producer;
        Assert.Equal(PromptToTaskIntentUnit.UnitId, producer.UnitId);
        Assert.Equal(PromptToTaskIntentUnit.Role, producer.UnitRole);
        Assert.Equal(PromptToTaskMachine.DomainTag, producer.DomainTag);
        Assert.Equal(PromptToTaskMachine.Version, producer.Version);
        Assert.Equal(1, producer.RegistrationOrder);
        Assert.Equal(typeof(PromptToTaskIntentUnit).FullName, producer.ProducerTypeName);

        var measurement = bound.UntrainedHonesty.Honesty;
        Assert.Equal(EvaluationId, measurement.EvaluationId);
        Assert.True(measurement.RuntimeReportedNotReady);
        Assert.True(measurement.HasNoTrainedStateEvidence);
        Assert.True(measurement.HasNoSemanticClassificationEvidence);
        Assert.Empty(measurement.ForbiddenShortcutMatches);
        Assert.True(measurement.MeetsUntrainedHonestyGate);
        Assert.Contains(typeof(PromptToTaskMachine).FullName!, measurement.InspectedTypeNames);
    }

    [Fact]
    public async Task InjectableInvoker_ReceivesExactOwnedRequestFlagsHashAndTokenOnce()
    {
        var invoker = new CapturingInvoker();
        using var service = new PromptToTaskUntrainedHonestyExecutionService(invoker);
        using var cancellation = new CancellationTokenSource();

        var operation = await service.ExecuteAsync(
            EvaluationId,
            RunId,
            PromptId,
            RawPrompt,
            UserLanguage,
            cancellation.Token);

        Assert.True(operation.IsSuccess);
        Assert.Equal(1, invoker.InvocationCount);
        Assert.Equal(cancellation.Token, invoker.CancellationToken);
        var descriptor = Assert.IsType<LogicalUnitDescriptor>(invoker.ResolvedProducer?.Descriptor);
        Assert.Equal(PromptToTaskIntentUnit.UnitId, descriptor.UnitId);
        Assert.Equal(PromptToTaskIntentUnit.Role, descriptor.UnitRole);
        Assert.Equal(PromptToTaskMachine.DomainTag, descriptor.DomainTag);
        Assert.Equal(PromptToTaskMachine.Version, descriptor.Version);
        Assert.Equal(LogicalUnitTrustState.Candidate, descriptor.TrustState);
        Assert.False(descriptor.IsLearned);

        var request = Assert.IsType<LogicalUnitRequest<PromptToTaskRequest>>(invoker.Request);
        Assert.Equal(RunId, request.RunId);
        Assert.Equal(descriptor.UnitId, request.UnitId);
        Assert.Equal(descriptor.UnitRole, request.UnitRole);
        Assert.Equal(descriptor.DomainTag, request.DomainTag);
        Assert.Empty(request.Metadata);
        Assert.Equal(PromptId, request.Input.PromptId);
        Assert.Equal(RawPrompt, request.Input.RawPrompt);
        Assert.Equal(
            $"sha256:{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(RawPrompt)))}",
            request.Input.RawPromptSha256);
        Assert.Equal(
            PromptToTaskUntrainedHonestyExecutionService.PromptProvenance,
            request.Input.PromptProvenance);
        Assert.Equal(UserLanguage, request.Input.UserLanguage);
        Assert.Empty(request.Input.ExplicitConstraints);
        Assert.Empty(request.Input.ContextFields);

        var booleanFlags = typeof(LogicalExecutionFlags)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.PropertyType == typeof(bool))
            .ToArray();
        Assert.NotEmpty(booleanFlags);
        Assert.All(booleanFlags, property => Assert.False((bool)property.GetValue(request.Flags)!));
    }

    [Fact]
    public async Task PreCancelledRequest_InvokesNothingAndReportsCancellation()
    {
        var invoker = new CapturingInvoker();
        using var service = new PromptToTaskUntrainedHonestyExecutionService(invoker);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var operation = await service.ExecuteAsync(
            EvaluationId,
            RunId,
            PromptId,
            RawPrompt,
            UserLanguage,
            cancellation.Token);

        Assert.True(operation.IsSuccess);
        Assert.Equal(0, invoker.InvocationCount);
        Assert.Equal(
            PromptToTaskUntrainedHonestyExecutionStatus.Cancelled,
            operation.Value!.Status);
        Assert.Equal(0, operation.Value.DirectUnitInvocationCount);
        Assert.Null(operation.Value.Measurement);
    }

    [Fact]
    public async Task AcquiredInvocation_CancelledFailureRequiresExactRequestedToken()
    {
        using var cancellation = new CancellationTokenSource();
        var invoker = new CapturingInvoker(_ =>
        {
            cancellation.Cancel();
            return OperationResult<LogicalUnitResult<PromptToTaskResponse>>.Fail(
                "cancelled",
                "CANCELLED");
        });
        using var service = new PromptToTaskUntrainedHonestyExecutionService(invoker);

        var operation = await service.ExecuteAsync(
            EvaluationId,
            RunId,
            PromptId,
            RawPrompt,
            UserLanguage,
            cancellation.Token);

        Assert.True(operation.IsSuccess);
        Assert.Equal(
            PromptToTaskUntrainedHonestyExecutionStatus.Cancelled,
            operation.Value!.Status);
        Assert.Equal(1, operation.Value.DirectUnitInvocationCount);
        Assert.Null(operation.Value.Measurement);
        Assert.Equal(1, invoker.InvocationCount);
        Assert.Equal(cancellation.Token, invoker.CancellationToken);
    }

    [Fact]
    public async Task AcquiredInvocation_CaseVariantCancelledCodeRemainsFailureEvenWhenTokenRequested()
    {
        using var cancellation = new CancellationTokenSource();
        var invoker = new CapturingInvoker(_ =>
        {
            cancellation.Cancel();
            return OperationResult<LogicalUnitResult<PromptToTaskResponse>>.Fail(
                "cancelled",
                "cancelled");
        });
        using var service = new PromptToTaskUntrainedHonestyExecutionService(invoker);

        var operation = await service.ExecuteAsync(
            "evaluation:test:001",
            "run:test:001",
            "prompt:test:001",
            "Create a bounded deterministic task.",
            "en",
            cancellation.Token);

        Assert.False(operation.IsSuccess);
        Assert.Equal("cancelled", operation.ErrorCode);
        Assert.Equal(1, invoker.InvocationCount);
        Assert.Equal(cancellation.Token, invoker.CancellationToken);
    }

    [Fact]
    public async Task AcquiredInvocation_UnconfirmedCancelledCodeRemainsFailure()
    {
        var invoker = new CapturingInvoker(_ =>
            OperationResult<LogicalUnitResult<PromptToTaskResponse>>.Fail(
                "cancelled",
                "CANCELLED"));
        using var service = new PromptToTaskUntrainedHonestyExecutionService(invoker);

        var operation = await ExecuteAsync(service);

        Assert.False(operation.IsSuccess);
        Assert.Equal("CANCELLED", operation.ErrorCode);
        Assert.Equal(1, invoker.InvocationCount);
    }

    [Fact]
    public async Task OccupiedOneSlotGate_RejectsSecondRequestWithoutInvokingIt()
    {
        var invoker = new BlockingInvoker();
        using var service = new PromptToTaskUntrainedHonestyExecutionService(invoker);

        var firstTask = ExecuteAsync(service);
        await invoker.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));

        var second = await service.ExecuteAsync(
            "evaluation:l2-untrained:002",
            "run:l2-untrained:002",
            "prompt:l2-untrained:002",
            RawPrompt,
            UserLanguage,
            CancellationToken.None);

        Assert.True(second.IsSuccess);
        Assert.Equal(
            PromptToTaskUntrainedHonestyExecutionStatus.ConcurrencyLimited,
            second.Value!.Status);
        Assert.Equal(0, second.Value.DirectUnitInvocationCount);
        Assert.Null(second.Value.Measurement);
        Assert.Equal(1, invoker.InvocationCount);

        invoker.Release.TrySetResult();
        var first = await firstTask;
        Assert.True(first.IsSuccess);
        Assert.Equal(1, first.Value!.DirectUnitInvocationCount);
        Assert.Equal(1, invoker.InvocationCount);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(false, false, false)]
    public async Task RuntimeNotReadyPredicate_IsExactOrdinalAnd(
        bool exactStatus,
        bool exactTaskType,
        bool expectedGate)
    {
        var invoker = new CapturingInvoker(result =>
        {
            var value = result.Value!;
            var output = value.Output with
            {
                ContractStatus = exactStatus
                    ? PromptToTaskMachine.NotTrainedStatus
                    : PromptToTaskMachine.NotTrainedStatus.ToUpperInvariant(),
                StructuredTask = value.Output.StructuredTask with
                {
                    TaskType = exactTaskType
                        ? PromptToTaskMachine.NotReadyTaskType
                        : PromptToTaskMachine.NotReadyTaskType.ToUpperInvariant(),
                },
            };
            return OperationResult<LogicalUnitResult<PromptToTaskResponse>>.Ok(
                value with { Output = output });
        });
        using var service = new PromptToTaskUntrainedHonestyExecutionService(invoker);

        var operation = await ExecuteAsync(service);

        Assert.True(operation.IsSuccess);
        var honesty = operation.Value!.Measurement!.UntrainedHonesty.Honesty;
        Assert.Equal(expectedGate, honesty.RuntimeReportedNotReady);
        Assert.Equal(expectedGate, honesty.MeetsUntrainedHonestyGate);
        Assert.Equal(1, operation.Value.DirectUnitInvocationCount);
        Assert.Equal(1, invoker.InvocationCount);
    }

    [Theory]
    [InlineData(nameof(LogicalUnitResult<PromptToTaskResponse>.UnitId))]
    [InlineData(nameof(LogicalUnitResult<PromptToTaskResponse>.UnitRole))]
    [InlineData(nameof(LogicalUnitResult<PromptToTaskResponse>.DomainTag))]
    public async Task WrongReturnedProducerIdentity_FailsWithoutRetry(string field)
    {
        var invoker = new CapturingInvoker(result =>
        {
            var value = result.Value!;
            var mutated = field switch
            {
                nameof(LogicalUnitResult<PromptToTaskResponse>.UnitId) =>
                    value with { UnitId = "wrong-unit" },
                nameof(LogicalUnitResult<PromptToTaskResponse>.UnitRole) =>
                    value with { UnitRole = "wrong-role" },
                nameof(LogicalUnitResult<PromptToTaskResponse>.DomainTag) =>
                    value with { DomainTag = "wrong-domain" },
                _ => throw new InvalidOperationException("Unknown identity field."),
            };
            return OperationResult<LogicalUnitResult<PromptToTaskResponse>>.Ok(mutated);
        });
        using var service = new PromptToTaskUntrainedHonestyExecutionService(invoker);

        var operation = await ExecuteAsync(service);

        Assert.False(operation.IsSuccess);
        Assert.Equal("INVALID_UNTRAINED_HONESTY_L2_PROBE", operation.ErrorCode);
        Assert.Equal(1, invoker.InvocationCount);
    }

    [Fact]
    public async Task NullAndSuccessNullInvocationResults_ReturnStableRequiredFailure()
    {
        using var nullTaskService =
            new PromptToTaskUntrainedHonestyExecutionService(new NullTaskInvoker());
        using var successNullService =
            new PromptToTaskUntrainedHonestyExecutionService(new SuccessNullInvoker());

        var nullTask = await ExecuteAsync(nullTaskService);
        var successNull = await ExecuteAsync(successNullService);

        Assert.False(nullTask.IsSuccess);
        Assert.Equal(
            PromptToTaskUntrainedHonestyExecutionService.ResultRequiredCode,
            nullTask.ErrorCode);
        Assert.False(successNull.IsSuccess);
        Assert.Equal(
            PromptToTaskUntrainedHonestyExecutionService.ResultRequiredCode,
            successNull.ErrorCode);
    }

    [Fact]
    public async Task NonFatalInvokerException_IsSanitizedAndFatalExceptionEscapes()
    {
        using var nonFatalService =
            new PromptToTaskUntrainedHonestyExecutionService(
                new ThrowingInvoker(new InvalidOperationException("sensitive")));
        var fatal = new OutOfMemoryException("fatal");
        using var fatalService =
            new PromptToTaskUntrainedHonestyExecutionService(new ThrowingInvoker(fatal));

        var nonFatal = await ExecuteAsync(nonFatalService);
        var thrown = await Assert.ThrowsAsync<OutOfMemoryException>(
            () => ExecuteAsync(fatalService));

        Assert.False(nonFatal.IsSuccess);
        Assert.Equal(
            PromptToTaskUntrainedHonestyExecutionService.ExecutionFailedCode,
            nonFatal.ErrorCode);
        Assert.DoesNotContain("sensitive", nonFatal.ErrorMessage, StringComparison.Ordinal);
        Assert.Same(fatal, thrown);
    }

    [Fact]
    public async Task Dispose_IsIdempotentAndRejectsFurtherExecution()
    {
        var service = new PromptToTaskUntrainedHonestyExecutionService();

        service.Dispose();
        service.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => ExecuteAsync(service));
    }

    [Fact]
    public void ServiceSource_UsesOneResolvedExecutionThenAlreadyExecutedMeasurementAndBind()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepoRoot(),
            "tools",
            "DynamicGraphCompositionLiveLocalServer",
            "PromptToTaskUntrainedHonestyExecutionService.cs"));
        var invocations = CSharpSyntaxTree.ParseText(source)
            .GetRoot()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .ToArray();

        _ = Assert.Single(
            invocations,
            invocation => invocation.Expression.ToString()
                == "_invoker.InvokeAsync");
        _ = Assert.Single(
            invocations,
            invocation => invocation.Expression.ToString()
                == "resolvedProducer.Unit.ExecuteAsync");
        _ = Assert.Single(
            invocations,
            invocation => invocation.Expression.ToString()
                == "DynamicGraphUntrainedHonestyL2Probe.MeasureAlreadyExecuted");
        _ = Assert.Single(
            invocations,
            invocation => invocation.Expression.ToString()
                == "DynamicGraphMeasurementCoordinator.BindL2UntrainedHonesty");
        Assert.DoesNotContain(
            "DynamicGraphUntrainedHonestyL2Probe.MeasureAsync(",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain("TrainingLoop", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DPO", source, StringComparison.Ordinal);
    }

    private static Task<OperationResult<PromptToTaskUntrainedHonestyExecutionResult>>
        ExecuteAsync(PromptToTaskUntrainedHonestyExecutionService service)
        => service.ExecuteAsync(
            EvaluationId,
            RunId,
            PromptId,
            RawPrompt,
            UserLanguage,
            CancellationToken.None);

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return current.FullName;
            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private sealed class CapturingInvoker(
        Func<
            OperationResult<LogicalUnitResult<PromptToTaskResponse>>,
            OperationResult<LogicalUnitResult<PromptToTaskResponse>>>? transform = null)
        : IPromptToTaskUntrainedHonestyExecutionInvoker
    {
        internal int InvocationCount { get; private set; }
        internal ResolvedLogicalUnitRegistryEntry<PromptToTaskRequest, PromptToTaskResponse>?
            ResolvedProducer
        { get; private set; }
        internal LogicalUnitRequest<PromptToTaskRequest>? Request { get; private set; }
        internal CancellationToken CancellationToken { get; private set; }

        public async Task<OperationResult<LogicalUnitResult<PromptToTaskResponse>>> InvokeAsync(
            ResolvedLogicalUnitRegistryEntry<PromptToTaskRequest, PromptToTaskResponse>
                resolvedProducer,
            LogicalUnitRequest<PromptToTaskRequest> request,
            CancellationToken cancellationToken)
        {
            InvocationCount++;
            ResolvedProducer = resolvedProducer;
            Request = request;
            CancellationToken = cancellationToken;
            var result = await resolvedProducer.Unit.ExecuteAsync(request, cancellationToken);
            return transform?.Invoke(result) ?? result;
        }
    }

    private sealed class BlockingInvoker : IPromptToTaskUntrainedHonestyExecutionInvoker
    {
        internal TaskCompletionSource Entered { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource Release { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal int InvocationCount { get; private set; }

        public async Task<OperationResult<LogicalUnitResult<PromptToTaskResponse>>> InvokeAsync(
            ResolvedLogicalUnitRegistryEntry<PromptToTaskRequest, PromptToTaskResponse>
                resolvedProducer,
            LogicalUnitRequest<PromptToTaskRequest> request,
            CancellationToken cancellationToken)
        {
            InvocationCount++;
            Entered.TrySetResult();
            await Release.Task.WaitAsync(cancellationToken);
            return await resolvedProducer.Unit.ExecuteAsync(request, cancellationToken);
        }
    }

    private sealed class NullTaskInvoker : IPromptToTaskUntrainedHonestyExecutionInvoker
    {
        public Task<OperationResult<LogicalUnitResult<PromptToTaskResponse>>> InvokeAsync(
            ResolvedLogicalUnitRegistryEntry<PromptToTaskRequest, PromptToTaskResponse>
                resolvedProducer,
            LogicalUnitRequest<PromptToTaskRequest> request,
            CancellationToken cancellationToken)
            => null!;
    }

    private sealed class SuccessNullInvoker
        : IPromptToTaskUntrainedHonestyExecutionInvoker
    {
        public Task<OperationResult<LogicalUnitResult<PromptToTaskResponse>>> InvokeAsync(
            ResolvedLogicalUnitRegistryEntry<PromptToTaskRequest, PromptToTaskResponse>
                resolvedProducer,
            LogicalUnitRequest<PromptToTaskRequest> request,
            CancellationToken cancellationToken)
            => Task.FromResult(
                OperationResult<LogicalUnitResult<PromptToTaskResponse>>.Ok(null!));
    }

    private sealed class ThrowingInvoker(Exception exception)
        : IPromptToTaskUntrainedHonestyExecutionInvoker
    {
        public Task<OperationResult<LogicalUnitResult<PromptToTaskResponse>>> InvokeAsync(
            ResolvedLogicalUnitRegistryEntry<PromptToTaskRequest, PromptToTaskResponse>
                resolvedProducer,
            LogicalUnitRequest<PromptToTaskRequest> request,
            CancellationToken cancellationToken)
            => throw exception;
    }
}
