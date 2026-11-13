// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

public sealed class DynamicGraphEdgeAuthoritySelectionTargetExecutionProbeTests
{
    private const string EdgeId = "edge-selection-target-execution";
    private const string AuthorityUnitId = "edge-selection-authority-unit";
    private const string EvaluationId = "edge-selection-target-evaluation";
    private const string ExecutionId = "edge-selection-target-execution-001";
    private const string A = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string B = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string C = "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";

    private const string ExecutorRequiredCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_EXECUTOR_REQUIRED";
    private const string ExecutorRequiredMessage =
        "Dynamic graph L3 selected-target executor is required.";
    private const string TimeProviderRequiredCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_TIME_PROVIDER_REQUIRED";
    private const string TimeProviderRequiredMessage =
        "Dynamic graph L3 selected-target time provider is required.";
    private const string AlreadyStartedCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_EXECUTION_ALREADY_STARTED";
    private const string AlreadyStartedMessage =
        "Dynamic graph L3 selected-target execution has already started.";
    private const string AssociationRequiredCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_ASSOCIATION_REQUIRED";
    private const string AssociationRequiredMessage =
        "Dynamic graph L3 selected-target association is required.";
    private const string AssociationInvalidCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_ASSOCIATION_INVALID";
    private const string AssociationInvalidMessage =
        "Dynamic graph L3 selected-target association is invalid.";
    private const string ExecutionFailedCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_EXECUTION_FAILED";
    private const string ExecutionFailedMessage =
        "Dynamic graph L3 selected-target execution failed.";
    private const string ResultRequiredCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_EXECUTION_RESULT_REQUIRED";
    private const string ResultRequiredMessage =
        "Dynamic graph L3 selected-target execution result is required.";
    private const string ResultInvalidCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_EXECUTION_RESULT_INVALID";
    private const string ResultInvalidMessage =
        "Dynamic graph L3 selected-target execution result is invalid.";
    private const string TargetMismatchCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_EXECUTION_TARGET_MISMATCH";
    private const string TargetMismatchMessage =
        "Dynamic graph L3 selected-target execution result does not match the admitted target.";
    private const string RunSpeedTimingInvalidCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_RUN_SPEED_TIMING_INVALID";
    private const string RunSpeedTimingInvalidMessage =
        "Dynamic graph L3 selected-target run-speed timing is invalid.";

    private static readonly string[] LeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];
    private static readonly string[] IdentityProbeIds =
        ["source-id-control-01", "source-path-control-02", "correlation-id-control-03"];
    private static readonly string[] IdentityControlledSignals =
        ["source_id", "source_path", "correlation_id"];
    private static readonly string[] IdentityControlValues =
    [
        "edge-authority-source-id-control",
        "identity-control://edge-authority-source-path",
        "edge-authority-correlation-id-control"
    ];

    [Fact]
    public void Contracts_ExposeExactOwnedNonOverclaimSurface()
    {
        var executorMethod = Assert.Single(typeof(IDynamicGraphL3SelectedTargetExecutor)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Equal("ExecuteAsync", executorMethod.Name);
        Assert.Equal(
            typeof(Task<OperationResult<DynamicGraphL3SelectedTargetExecutionResult>>),
            executorMethod.ReturnType);
        var executorParameters = executorMethod.GetParameters();
        Assert.Equal(2, executorParameters.Length);
        Assert.Equal("association", executorParameters[0].Name, StringComparer.Ordinal);
        Assert.Equal(
            typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement),
            executorParameters[0].ParameterType);
        Assert.False(executorParameters[0].HasDefaultValue);
        Assert.Equal("cancellationToken", executorParameters[1].Name, StringComparer.Ordinal);
        Assert.Equal(typeof(CancellationToken), executorParameters[1].ParameterType);
        Assert.True(executorParameters[1].HasDefaultValue);
        Assert.Null(executorParameters[1].DefaultValue);

        AssertOwnedShape<DynamicGraphL3SelectedTargetExecutionResult>(
            ("TargetId", typeof(string)),
            ("ExecutionId", typeof(string)),
            ("EvidenceIds", typeof(IReadOnlyList<string>)));
        AssertOwnedShape<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>(
            ("Association",
                typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement)),
            ("ExecutionResult", typeof(DynamicGraphL3SelectedTargetExecutionResult)),
            ("RunSpeed", typeof(DynamicGraphTargetRunSpeedMeasurement)),
            ("DirectTargetInvocationCount", typeof(int)));
        var receiptConstructor = Assert.Single(
            typeof(DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt)
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Equal(
            [
                typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement),
                typeof(DynamicGraphL3SelectedTargetExecutionResult),
                typeof(DynamicGraphTargetRunSpeedMeasurement)
            ],
            receiptConstructor.GetParameters().Select(parameter => parameter.ParameterType));

        var receiptSurface = string.Join(
            '|',
            typeof(DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name));
        foreach (var forbidden in new[]
                 {
                     "Verdict", "Overall", "Training", "Dpo", "Checkpoint", "Promotion",
                     "Lifecycle", "GraphMutation"
                 })
        {
            Assert.DoesNotContain(forbidden, receiptSurface, StringComparison.OrdinalIgnoreCase);
        }

        var probeType = typeof(DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe);
        Assert.True(probeType.IsSealed);
        Assert.Single(probeType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Empty(probeType.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        var fields = probeType.GetFields(
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Equal(3, fields.Length);
        Assert.True(Assert.Single(fields, field => field.Name == "_executor").IsInitOnly);
        Assert.True(Assert.Single(fields, field => field.Name == "_timeProvider").IsInitOnly);
        Assert.False(Assert.Single(fields, field => field.Name == "_started").IsInitOnly);

        var methods = probeType.GetMethods(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance |
                BindingFlags.DeclaredOnly)
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(4, methods.Length);
        var createMethods = methods.Where(method => method.Name == "Create").ToArray();
        Assert.Equal(2, createMethods.Length);
        Assert.All(createMethods, method =>
        {
            Assert.True(method.IsStatic);
            Assert.Equal(
                typeof(OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe>),
                method.ReturnType);
        });
        Assert.Equal(
            [typeof(IDynamicGraphL3SelectedTargetExecutor)],
            createMethods.Single(method => method.GetParameters().Length == 1)
                .GetParameters()
                .Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [typeof(IDynamicGraphL3SelectedTargetExecutor), typeof(TimeProvider)],
            createMethods.Single(method => method.GetParameters().Length == 2)
                .GetParameters()
                .Select(parameter => parameter.ParameterType));
        var runMethod = Assert.Single(methods, method => method.Name == "RunAsync");
        Assert.False(runMethod.IsStatic);
        Assert.Equal(
            typeof(Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>),
            runMethod.ReturnType);
        Assert.Equal(
            new[]
            {
                typeof(DynamicGraphMeasurementTarget),
                typeof(EdgeAuthorityExecutionResult),
                typeof(DynamicGraphEdgeAuthorityNoveltyEvaluationRequest),
                typeof(CancellationToken)
            },
            runMethod.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.True(runMethod.GetParameters()[3].HasDefaultValue);
        Assert.Single(methods, method => method.Name == "RunAdmittedAsync");
    }

    [Fact]
    public void Contracts_ExposeExactPublicAdmittedCarrierSurface()
    {
        var probeType = typeof(DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe);
        var method = Assert.Single(
            probeType.GetMethods(BindingFlags.Public | BindingFlags.Instance |
                                 BindingFlags.DeclaredOnly),
            candidate => candidate.Name == "RunAdmittedAsync");

        Assert.Equal("RunAdmittedAsync", method.Name, StringComparer.Ordinal);
        Assert.Equal(
            typeof(Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>),
            method.ReturnType);
        Assert.Equal(
            "AgentInfrastructure.Logical.DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement",
            method.GetParameters()[0].ParameterType.FullName,
            StringComparer.Ordinal);
        Assert.Equal("association", method.GetParameters()[0].Name, StringComparer.Ordinal);
        Assert.False(method.GetParameters()[0].HasDefaultValue);
        Assert.Equal(
            "System.Threading.CancellationToken",
            method.GetParameters()[1].ParameterType.FullName,
            StringComparer.Ordinal);
        Assert.Equal("cancellationToken", method.GetParameters()[1].Name, StringComparer.Ordinal);
        Assert.True(method.GetParameters()[1].HasDefaultValue);
        Assert.Null(method.GetParameters()[1].DefaultValue);
        Assert.Equal(
            ["Create", "Create", "RunAdmittedAsync", "RunAsync"],
            probeType.GetMethods(BindingFlags.Public | BindingFlags.Static |
                                 BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(candidate => candidate.Name)
                .OrderBy(name => name, StringComparer.Ordinal));
    }

    [Fact]
    public async Task RunAdmittedAsync_UsesOwnedCarrierWithoutRemeasurementAndPreservesTokenAndReceiptOwnership()
    {
        var callerAssociation = ValidAssociation();
        var callerRows = RequestedRowIdsBackingArray(callerAssociation);
        var expectedFirstRow = callerRows[0];
        var resultCarrier = new DynamicGraphL3SelectedTargetExecutionResult(
            EdgeId,
            ExecutionId,
            ["evidence-a", "evidence-b"]);
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement?
            executorAssociation = null;
        var executor = new RecordingExecutor((association, _) =>
        {
            executorAssociation = association;
            RequestedRowIdsBackingArray(association)[0] = "executor-mutated-row";
            MutateAssociationTarget(association, "executor-mutated-target");
            return Task.FromResult(
                OperationResult<DynamicGraphL3SelectedTargetExecutionResult>.Ok(resultCarrier));
        });
        var probe = CreateProbe(executor);
        using var cancellation = new CancellationTokenSource();

        var result = await probe.RunAdmittedAsync(callerAssociation, cancellation.Token);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, executor.CallCount);
        Assert.Equal(cancellation.Token, executor.SeenToken);
        Assert.NotNull(executorAssociation);
        Assert.NotSame(callerAssociation, executorAssociation);
        var receipt = result.Value!;
        Assert.Equal(EdgeId, receipt.Association.Target.TargetId, StringComparer.Ordinal);
        Assert.Equal(
            expectedFirstRow,
            receipt.Association.EdgeAuthoritySelectionQualityEvidence
                .NoveltyEvaluation.RequestedRowIds[0],
            StringComparer.Ordinal);

        callerRows[0] = "caller-mutated-row";
        MutateAssociationTarget(callerAssociation, "caller-mutated-target");
        Assert.IsType<string[]>(GetEvidenceIdsField().GetValue(resultCarrier))[0] =
            "executor-result-mutated";
        var returnedAssociation = receipt.Association;
        var returnedExecution = receipt.ExecutionResult;
        RequestedRowIdsBackingArray(returnedAssociation)[0] = "receipt-getter-mutated-row";
        MutateAssociationTarget(returnedAssociation, "receipt-getter-mutated-target");
        Assert.IsType<string[]>(GetEvidenceIdsField().GetValue(returnedExecution))[0] =
            "receipt-result-getter-mutated";

        Assert.Equal(EdgeId, receipt.Association.Target.TargetId, StringComparer.Ordinal);
        Assert.Equal(
            expectedFirstRow,
            receipt.Association.EdgeAuthoritySelectionQualityEvidence
                .NoveltyEvaluation.RequestedRowIds[0],
            StringComparer.Ordinal);
        Assert.Equal("evidence-a", receipt.ExecutionResult.EvidenceIds[0], StringComparer.Ordinal);
        Assert.NotSame(receipt.Association, receipt.Association);
        Assert.NotSame(receipt.ExecutionResult, receipt.ExecutionResult);
    }

    [Theory]
    [InlineData("admitted_then_raw")]
    [InlineData("raw_then_admitted")]
    [InlineData("concurrent_cross_entry")]
    public async Task RunAdmittedAsync_AndRunAsync_ShareOneShotGate(string scenario)
    {
        var association = ValidAssociation();
        if (scenario == "concurrent_cross_entry")
        {
            var entered = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var executor = new RecordingExecutor(async (_, _) =>
            {
                entered.TrySetResult();
                await release.Task.ConfigureAwait(false);
                return SuccessfulExecution();
            });
            var probe = CreateProbe(executor);
            var admittedTask = probe.RunAdmittedAsync(association);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));

            var rawLoser = await probe.RunAsync(
                ValidTarget(),
                ValidAuthorityResult(),
                ValidNoveltyRequest());
            release.TrySetResult();
            var admittedWinner = await admittedTask;

            Assert.True(admittedWinner.IsSuccess, admittedWinner.ErrorMessage);
            AssertReceiptFailure(rawLoser, AlreadyStartedCode, AlreadyStartedMessage);
            Assert.Equal(1, executor.CallCount);
            return;
        }

        var sequentialExecutor = SuccessfulExecutor();
        var sequentialProbe = CreateProbe(sequentialExecutor);
        OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt> first;
        OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt> second;
        if (scenario == "admitted_then_raw")
        {
            first = await sequentialProbe.RunAdmittedAsync(association);
            second = await sequentialProbe.RunAsync(null, null, null);
        }
        else if (scenario == "raw_then_admitted")
        {
            first = await RunValid(sequentialProbe);
            second = await sequentialProbe.RunAdmittedAsync(null);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        Assert.True(first.IsSuccess, first.ErrorMessage);
        AssertReceiptFailure(second, AlreadyStartedCode, AlreadyStartedMessage);
        Assert.Equal(1, sequentialExecutor.CallCount);
    }

    [Theory]
    [InlineData("pre_cancelled_null")]
    [InlineData("null_required")]
    [InlineData("constructible_binding_mismatch")]
    [InlineData("nonfatal_copy_failure")]
    public async Task RunAdmittedAsync_AppliesAssociationPrecedenceAndValidation(string scenario)
    {
        var executor = SuccessfulExecutor();
        var probe = CreateProbe(executor);
        using var cancellation = new CancellationTokenSource();
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement? association;
        if (scenario == "pre_cancelled_null")
        {
            cancellation.Cancel();
            association = null;
        }
        else if (scenario == "null_required")
        {
            association = null;
        }
        else if (scenario == "constructible_binding_mismatch")
        {
            var valid = ValidAssociation();
            association = new DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement(
                ValidTarget() with { TargetId = "different-constructible-edge" },
                valid.EdgeAuthoritySelectionQualityEvidence);
        }
        else if (scenario == "nonfatal_copy_failure")
        {
            association = ValidAssociation();
            GetAssociationEvidenceField().SetValue(association, null);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        var first = await probe.RunAdmittedAsync(association, cancellation.Token);
        var second = await probe.RunAdmittedAsync(ValidAssociation());

        if (scenario == "pre_cancelled_null")
            AssertCancelled(first);
        else if (scenario == "null_required")
            AssertReceiptFailure(first, AssociationRequiredCode, AssociationRequiredMessage);
        else
            AssertReceiptFailure(first, AssociationInvalidCode, AssociationInvalidMessage);
        AssertReceiptFailure(second, AlreadyStartedCode, AlreadyStartedMessage);
        Assert.Equal(0, executor.CallCount);
    }

    [Theory]
    [InlineData("null_task")]
    [InlineData("null_result_shapes")]
    public async Task RunAdmittedAsync_RequiresExecutorTaskAndResult(string scenario)
    {
        if (scenario == "null_task")
        {
            var executor = new RecordingExecutor((_, _) => null!);
            var result = await CreateProbe(executor).RunAdmittedAsync(ValidAssociation());

            AssertReceiptFailure(result, ResultRequiredCode, ResultRequiredMessage);
            Assert.Equal(1, executor.CallCount);
            return;
        }

        if (scenario != "null_result_shapes")
            throw new ArgumentOutOfRangeException(nameof(scenario));

        var nullEnvelopeExecutor = new RecordingExecutor((_, _) =>
            Task.FromResult<OperationResult<DynamicGraphL3SelectedTargetExecutionResult>>(null!));
        var nullEnvelope = await CreateProbe(nullEnvelopeExecutor)
            .RunAdmittedAsync(ValidAssociation());
        var nullValueExecutor = RecordingExecutor.Returning(
            OperationResult<DynamicGraphL3SelectedTargetExecutionResult>.Ok(null!));
        var nullValue = await CreateProbe(nullValueExecutor)
            .RunAdmittedAsync(ValidAssociation());

        AssertReceiptFailure(nullEnvelope, ResultRequiredCode, ResultRequiredMessage);
        AssertReceiptFailure(nullValue, ResultRequiredCode, ResultRequiredMessage);
        Assert.Equal(1, nullEnvelopeExecutor.CallCount);
        Assert.Equal(1, nullValueExecutor.CallCount);
    }

    [Fact]
    public async Task RunAdmittedAsync_PreservesSharedExecutorOutcomeMappings()
    {
        var declaredExecutor = RecordingExecutor.Returning(
            OperationResult<DynamicGraphL3SelectedTargetExecutionResult>.Fail(
                "Exact admitted executor failure.",
                "EXACT_ADMITTED_EXECUTOR_FAILURE"));
        var declared = await CreateProbe(declaredExecutor)
            .RunAdmittedAsync(ValidAssociation());
        AssertReceiptFailure(
            declared,
            "EXACT_ADMITTED_EXECUTOR_FAILURE",
            "Exact admitted executor failure.");
        Assert.Equal(1, declaredExecutor.CallCount);

        using var cancellation = new CancellationTokenSource();
        var requestedException = new OperationCanceledException(
            "requested admitted cancellation",
            cancellation.Token);
        var cancellationExecutor = new RecordingExecutor((_, token) =>
        {
            Assert.Equal(cancellation.Token, token);
            cancellation.Cancel();
            return Task.FromException<
                OperationResult<DynamicGraphL3SelectedTargetExecutionResult>>(
                requestedException);
        });
        var cancelled = await CreateProbe(cancellationExecutor)
            .RunAdmittedAsync(ValidAssociation(), cancellation.Token);
        AssertCancelled(cancelled);
        Assert.Equal(cancellation.Token, cancellationExecutor.SeenToken);
        Assert.Equal(1, cancellationExecutor.CallCount);

        var malformedExecutor = RecordingExecutor.Returning(
            OperationResult<DynamicGraphL3SelectedTargetExecutionResult>.Ok(
                new DynamicGraphL3SelectedTargetExecutionResult(
                    EdgeId,
                    ExecutionId,
                    ["duplicate", "duplicate"])));
        var malformed = await CreateProbe(malformedExecutor)
            .RunAdmittedAsync(ValidAssociation());
        AssertReceiptFailure(malformed, ResultInvalidCode, ResultInvalidMessage);
        Assert.Equal(1, malformedExecutor.CallCount);

        var wrongTargetExecutor = RecordingExecutor.Returning(
            OperationResult<DynamicGraphL3SelectedTargetExecutionResult>.Ok(
                new DynamicGraphL3SelectedTargetExecutionResult(
                    "different-returned-target",
                    ExecutionId,
                    ["evidence"])));
        var wrongTarget = await CreateProbe(wrongTargetExecutor)
            .RunAdmittedAsync(ValidAssociation());
        AssertReceiptFailure(wrongTarget, TargetMismatchCode, TargetMismatchMessage);
        Assert.Equal(1, wrongTargetExecutor.CallCount);

        var nonfatalException = new InvalidOperationException(
            "nonfatal admitted executor failure");
        var nonfatalExecutor = new RecordingExecutor((_, _) =>
            Task.FromException<OperationResult<DynamicGraphL3SelectedTargetExecutionResult>>(
                nonfatalException));
        var nonfatal = await CreateProbe(nonfatalExecutor)
            .RunAdmittedAsync(ValidAssociation());
        AssertReceiptFailure(nonfatal, ExecutionFailedCode, ExecutionFailedMessage);
        Assert.Equal(1, nonfatalExecutor.CallCount);
    }

    [Fact]
    public async Task RunAdmittedAsync_PreservesExceptionalEscapeSemantics()
    {
        var unrequestedCancellation = new OperationCanceledException(
            "unrequested admitted cancellation");
        var cancellationExecutor = new RecordingExecutor((_, _) =>
            Task.FromException<OperationResult<DynamicGraphL3SelectedTargetExecutionResult>>(
                unrequestedCancellation));
        var observedCancellation = await Assert.ThrowsAsync<OperationCanceledException>(() =>
            CreateProbe(cancellationExecutor).RunAdmittedAsync(ValidAssociation()));
        Assert.Same(unrequestedCancellation, observedCancellation);
        Assert.Equal(1, cancellationExecutor.CallCount);

        foreach (var scenario in new[] { "direct", "wrapped", "aggregate-not-first" })
        {
            var fatal = FatalGraph(scenario);
            var executor = new RecordingExecutor((_, _) =>
                Task.FromException<OperationResult<DynamicGraphL3SelectedTargetExecutionResult>>(
                    fatal));
            Exception? observed = null;
            try
            {
                await CreateProbe(executor).RunAdmittedAsync(ValidAssociation());
            }
            catch (Exception exception)
            {
                observed = exception;
            }

            Assert.Same(fatal, observed);
            Assert.Equal(1, executor.CallCount);
        }

        var innerField = GetExceptionInnerField();
        var cycle = new InvalidOperationException("admitted cyclic nonfatal node");
        innerField.SetValue(cycle, cycle);
        var cyclicFatal = new AggregateException(
            "admitted cyclic graph with fatal node",
            new BadImageFormatException("admitted cyclic fatal"),
            cycle);
        var cyclicExecutor = new RecordingExecutor((_, _) =>
            Task.FromException<OperationResult<DynamicGraphL3SelectedTargetExecutionResult>>(
                cyclicFatal));
        Exception? observedCyclic = null;
        try
        {
            try
            {
                await CreateProbe(cyclicExecutor)
                    .RunAdmittedAsync(ValidAssociation())
                    .WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (Exception exception)
            {
                observedCyclic = exception;
            }
        }
        finally
        {
            innerField.SetValue(cycle, null);
        }

        Assert.Same(cyclicFatal, observedCyclic);
        Assert.Equal(1, cyclicExecutor.CallCount);
    }

    [Fact]
    public void Create_RequiresExecutorWithExactStableFailure()
    {
        var result = DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe.Create(null);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(ExecutorRequiredCode, result.ErrorCode, StringComparer.Ordinal);
        Assert.Equal(ExecutorRequiredMessage, result.ErrorMessage, StringComparer.Ordinal);
    }

    [Fact]
    public void Create_ValidatesExecutorBeforeExplicitTimeProviderAndKeepsLegacyFactoryValid()
    {
        var bothNull = DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe.Create(
            null,
            null);
        Assert.False(bothNull.IsSuccess);
        Assert.Equal(ExecutorRequiredCode, bothNull.ErrorCode, StringComparer.Ordinal);
        Assert.Equal(ExecutorRequiredMessage, bothNull.ErrorMessage, StringComparer.Ordinal);

        var providerNull = DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe.Create(
            SuccessfulExecutor(),
            null);
        Assert.False(providerNull.IsSuccess);
        Assert.Null(providerNull.Value);
        Assert.Equal(TimeProviderRequiredCode, providerNull.ErrorCode, StringComparer.Ordinal);
        Assert.Equal(TimeProviderRequiredMessage, providerNull.ErrorMessage, StringComparer.Ordinal);

        var legacy = DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe.Create(
            SuccessfulExecutor());
        Assert.True(legacy.IsSuccess, $"{legacy.ErrorCode}: {legacy.ErrorMessage}");
        Assert.NotNull(legacy.Value);
    }

    [Theory]
    [InlineData(100L, 10_101L, 2L)]
    [InlineData(500L, 500L, 1L)]
    public async Task RunAdmittedAsync_MeasuresOneDirectInvocationWithPositiveCeilingMilliseconds(
        long startedTimestamp,
        long completedTimestamp,
        long expectedElapsedMs)
    {
        var timeProvider = new TracingTimeProvider(
            startedTimestamp,
            completedTimestamp);
        var executor = new RecordingExecutor((_, _) =>
        {
            timeProvider.Record("executor");
            return Task.FromResult(SuccessfulExecution());
        });

        var result = await CreateProbe(executor, timeProvider)
            .RunAdmittedAsync(ValidAssociation());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, executor.CallCount);
        Assert.Equal(
            ["timestamp-start", "executor", "timestamp-end"],
            timeProvider.Trace);
        Assert.Equal(1, timeProvider.ElapsedConversionCount);
        AssertExactRunSpeed(result.Value!, expectedElapsedMs);
    }

    [Fact]
    public async Task RunAdmittedAsync_ReadsCompletionTimestampOnlyAfterExecutorTaskCompletes()
    {
        var timeProvider = new TracingTimeProvider(1_000L, 11_000L);
        var entered = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var executor = new RecordingExecutor(async (_, _) =>
        {
            timeProvider.Record("executor-start");
            entered.TrySetResult();
            await release.Task.ConfigureAwait(false);
            timeProvider.Record("executor-complete");
            return SuccessfulExecution();
        });
        var running = CreateProbe(executor, timeProvider)
            .RunAdmittedAsync(ValidAssociation());

        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(
            ["timestamp-start", "executor-start"],
            timeProvider.Trace);
        release.TrySetResult();
        var result = await running;

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(
            ["timestamp-start", "executor-start", "executor-complete", "timestamp-end"],
            timeProvider.Trace);
        AssertExactRunSpeed(result.Value!, expectedElapsedMs: 1);
    }

    [Fact]
    public async Task RunAdmittedAsync_NegativeElapsedTimeFailsWithExactTimingCodeAndNoReceipt()
    {
        var timeProvider = new TracingTimeProvider(2_000L, 1_999L);
        var executor = SuccessfulExecutor();

        var result = await CreateProbe(executor, timeProvider)
            .RunAdmittedAsync(ValidAssociation());

        AssertReceiptFailure(
            result,
            RunSpeedTimingInvalidCode,
            RunSpeedTimingInvalidMessage);
        Assert.Equal(1, executor.CallCount);
        Assert.Equal(1, timeProvider.ElapsedConversionCount);
    }

    [Theory]
    [InlineData("association-invalid", AssociationInvalidCode, AssociationInvalidMessage, 0)]
    [InlineData("declared-failure", "EXACT_TIMING_PRECEDENCE", "Exact timing precedence.", 2)]
    [InlineData("malformed-result", ResultInvalidCode, ResultInvalidMessage, 2)]
    [InlineData("target-mismatch", TargetMismatchCode, TargetMismatchMessage, 2)]
    [InlineData("cancel-after-await", "CANCELLED", "Cancelled.", 2)]
    public async Task RunAdmittedAsync_FailurePrecedenceNeverCreatesRunSpeedReceipt(
        string scenario,
        string expectedCode,
        string expectedMessage,
        int expectedTimestampReads)
    {
        var timeProvider = new TracingTimeProvider(1_000L, 11_000L);
        using var cancellation = new CancellationTokenSource();
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement association =
            ValidAssociation();
        RecordingExecutor executor;
        if (scenario == "association-invalid")
        {
            association = new DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement(
                ValidTarget() with { TargetId = "different-target" },
                association.EdgeAuthoritySelectionQualityEvidence);
            executor = SuccessfulExecutor();
        }
        else if (scenario == "declared-failure")
        {
            executor = RecordingExecutor.Returning(
                OperationResult<DynamicGraphL3SelectedTargetExecutionResult>.Fail(
                    expectedMessage,
                    expectedCode));
        }
        else if (scenario == "malformed-result")
        {
            executor = RecordingExecutor.Returning(
                OperationResult<DynamicGraphL3SelectedTargetExecutionResult>.Ok(
                    new DynamicGraphL3SelectedTargetExecutionResult(
                        EdgeId,
                        ExecutionId,
                        ["duplicate", "duplicate"])));
        }
        else if (scenario == "target-mismatch")
        {
            executor = RecordingExecutor.Returning(
                OperationResult<DynamicGraphL3SelectedTargetExecutionResult>.Ok(
                    new DynamicGraphL3SelectedTargetExecutionResult(
                        "different-target",
                        ExecutionId,
                        ["evidence"])));
        }
        else if (scenario == "cancel-after-await")
        {
            executor = new RecordingExecutor((_, _) =>
            {
                cancellation.Cancel();
                return Task.FromResult(SuccessfulExecution());
            });
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        var result = await CreateProbe(executor, timeProvider)
            .RunAdmittedAsync(association, cancellation.Token);

        AssertReceiptFailure(result, expectedCode, expectedMessage);
        Assert.Equal(expectedTimestampReads, timeProvider.TimestampReadCount);
        Assert.Equal(0, timeProvider.ElapsedConversionCount);
        Assert.Equal(scenario == "association-invalid" ? 0 : 1, executor.CallCount);
    }

    [Theory]
    [InlineData("start", false)]
    [InlineData("start", true)]
    [InlineData("end", false)]
    [InlineData("end", true)]
    public async Task RunAdmittedAsync_TimeProviderProgrammingAndFatalExceptionsEscape(
        string throwAt,
        bool fatal)
    {
        Exception exception = fatal
            ? new OutOfMemoryException("Synthetic time-provider fatal.")
            : new InvalidOperationException("Synthetic time-provider programming failure.");
        var timeProvider = new TracingTimeProvider(
            1_000L,
            11_000L,
            throwAt == "start" ? exception : null,
            throwAt == "end" ? exception : null);
        var executor = SuccessfulExecutor();

        Exception? observed = null;
        try
        {
            await CreateProbe(executor, timeProvider).RunAdmittedAsync(ValidAssociation());
        }
        catch (Exception caught)
        {
            observed = caught;
        }

        Assert.Same(exception, observed);
        Assert.Equal(throwAt == "start" ? 0 : 1, executor.CallCount);
        Assert.Equal(0, timeProvider.ElapsedConversionCount);
    }

    [Fact]
    public void Receipt_RequiresAndDeepOwnsAssociationExecutionAndNestedRunSpeed()
    {
        var association = ValidAssociation();
        var execution = new DynamicGraphL3SelectedTargetExecutionResult(
            EdgeId,
            ExecutionId,
            ["owned-evidence"]);
        var runSpeed = DeterministicRunSpeed(ValidTarget(), inferenceElapsedMs: 7);
        var receipt = new DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt(
            association,
            execution,
            runSpeed);

        MutateAssociationTarget(association, "mutated-caller-association");
        MutateRunSpeed(runSpeed, "mutated-caller-speed-target", 99);
        var returnedRunSpeed = receipt.RunSpeed;
        MutateRunSpeed(returnedRunSpeed, "mutated-returned-speed-target", 101);

        Assert.Equal(EdgeId, receipt.Association.Target.TargetId, StringComparer.Ordinal);
        Assert.Equal(EdgeId, receipt.ExecutionResult.TargetId, StringComparer.Ordinal);
        AssertExactRunSpeed(receipt, expectedElapsedMs: 7);
        Assert.NotSame(runSpeed, receipt.RunSpeed);
        Assert.NotSame(receipt.RunSpeed, receipt.RunSpeed);
        Assert.NotSame(receipt.RunSpeed.Target, receipt.RunSpeed.Target);
        Assert.NotSame(receipt.RunSpeed.RunSpeed, receipt.RunSpeed.RunSpeed);
        Assert.NotSame(
            receipt.RunSpeed.RunSpeed.RunSpeed,
            receipt.RunSpeed.RunSpeed.RunSpeed);
    }

    [Fact]
    public async Task RunAsync_SuccessMatches20AhAndCallsExecutorExactlyOnceWithSameToken()
    {
        var evidenceIds = new List<string> { "evidence-b", "Evidence-B", "evidence-a" };
        var resultCarrier = new DynamicGraphL3SelectedTargetExecutionResult(
            EdgeId,
            ExecutionId,
            evidenceIds);
        var executor = RecordingExecutor.Returning(
            OperationResult<DynamicGraphL3SelectedTargetExecutionResult>.Ok(resultCarrier));
        var probe = CreateProbe(executor);
        using var cancellation = new CancellationTokenSource();
        var target = ValidTarget();
        var authority = ValidAuthorityResult();
        var request = ValidNoveltyRequest();
        var direct = DynamicGraphMeasurementCoordinator
            .MeasureEdgeAuthoritySelectionQualityEvidence(target, authority, request);

        var result = await probe.RunAsync(target, authority, request, cancellation.Token);

        Assert.True(direct.IsSuccess, $"{direct.ErrorCode}: {direct.ErrorMessage}");
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, executor.CallCount);
        Assert.Equal(cancellation.Token, executor.SeenToken);
        AssertAssociationEquivalent(direct.Value!, executor.SeenAssociation!);
        var receipt = result.Value!;
        Assert.Equal(1, receipt.DirectTargetInvocationCount);
        AssertAssociationEquivalent(direct.Value!, receipt.Association);
        Assert.NotSame(executor.SeenAssociation, receipt.Association);
        Assert.Equal(EdgeId, receipt.ExecutionResult.TargetId, StringComparer.Ordinal);
        Assert.Equal(ExecutionId, receipt.ExecutionResult.ExecutionId, StringComparer.Ordinal);
        Assert.Equal(evidenceIds, receipt.ExecutionResult.EvidenceIds);
    }

    [Fact]
    public async Task RunAsync_SequentialSecondCallFailsBeforeInputsAndDoesNotReinvoke()
    {
        var executor = SuccessfulExecutor();
        var probe = CreateProbe(executor);
        var first = await RunValid(probe);

        var second = await probe.RunAsync(null, null, null);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        AssertReceiptFailure(second, AlreadyStartedCode, AlreadyStartedMessage);
        Assert.Equal(1, executor.CallCount);
    }

    [Fact]
    public async Task RunAsync_ConcurrentSecondCallFailsWhileFirstRemainsSingleInvocation()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var executor = new RecordingExecutor(async (_, _) =>
        {
            entered.TrySetResult();
            await release.Task.ConfigureAwait(false);
            return SuccessfulExecution();
        });
        var probe = CreateProbe(executor);

        var firstTask = RunValid(probe);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var second = await probe.RunAsync(null, null, null);
        release.TrySetResult();
        var first = await firstTask;

        Assert.True(first.IsSuccess, first.ErrorMessage);
        AssertReceiptFailure(second, AlreadyStartedCode, AlreadyStartedMessage);
        Assert.Equal(1, executor.CallCount);
    }

    [Fact]
    public async Task RunAsync_FailedAdmissionConsumesOneShotAndNeverCallsExecutor()
    {
        var executor = SuccessfulExecutor();
        var probe = CreateProbe(executor);

        var first = await probe.RunAsync(null, ValidAuthorityResult(), ValidNoveltyRequest());
        var second = await RunValid(probe);

        AssertReceiptFailure(
            first,
            "INVALID_DYNAMIC_GRAPH_MEASUREMENT_TARGET",
            "Dynamic graph measurement target is invalid.");
        AssertReceiptFailure(second, AlreadyStartedCode, AlreadyStartedMessage);
        Assert.Equal(0, executor.CallCount);
    }

    [Fact]
    public async Task RunAsync_PreCancellationWinsBeforeInputsAndConsumesOneShot()
    {
        var executor = SuccessfulExecutor();
        var probe = CreateProbe(executor);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var first = await probe.RunAsync(null, null, null, cancellation.Token);
        var second = await RunValid(probe);

        AssertCancelled(first);
        AssertReceiptFailure(second, AlreadyStartedCode, AlreadyStartedMessage);
        Assert.Equal(0, executor.CallCount);
    }

    [Theory]
    [InlineData("non-l3", "DYNAMIC_GRAPH_EDGE_QUALITY_EVIDENCE_TARGET_LEVEL_INVALID",
        "Dynamic graph edge quality evidence can only be associated with an L3 logical-flow target.")]
    [InlineData("null-authority", "INVALID_EDGE_AUTHORITY_CONTROL_MEASUREMENT",
        "Dynamic graph edge authority control measurement is invalid.")]
    [InlineData("null-novelty", "EDGE_AUTHORITY_NOVELTY_REQUEST_REQUIRED",
        "Dynamic graph edge authority novelty evaluation request is required.")]
    [InlineData("correlation", "EDGE_AUTHORITY_SELECTION_QUALITY_EVIDENCE_MISMATCH",
        "Dynamic graph edge authority selection quality evidence is not correlated.")]
    [InlineData("target-mismatch", "DYNAMIC_GRAPH_EDGE_QUALITY_EVIDENCE_TARGET_MISMATCH",
        "Dynamic graph edge quality evidence target does not match the measured edge.")]
    public async Task RunAsync_PropagatesExact20AhFailuresWithoutExecutorBypass(
        string scenario,
        string expectedCode,
        string expectedMessage)
    {
        var executor = SuccessfulExecutor();
        var probe = CreateProbe(executor);
        var target = scenario switch
        {
            "non-l3" => ValidTarget() with { Level = DynamicGraphMeasurementTargetLevel.L2LogicalUnit },
            "target-mismatch" => ValidTarget() with { TargetId = "different-edge" },
            _ => ValidTarget()
        };
        var authority = scenario == "null-authority" ? null : ValidAuthorityResult();
        var novelty = scenario switch
        {
            "null-novelty" => null,
            "correlation" => ValidNoveltyRequest(Paired(edgeId: "different-edge")),
            _ => ValidNoveltyRequest()
        };

        var result = await probe.RunAsync(target, authority, novelty);

        AssertReceiptFailure(result, expectedCode, expectedMessage);
        Assert.Equal(0, executor.CallCount);
    }

    [Fact]
    public async Task RunAsync_FalseNoveltyAndAblationGatesStillExecute()
    {
        var paired = Paired(learnedCorrect: 1, baselineCorrect: 1);
        var request = ValidNoveltyRequest(
            paired,
            requestedRowIds: paired.Rows.Select(row => row.RowId)
                .Append("requested-but-missing")
                .ToArray());
        var executor = SuccessfulExecutor();
        var probe = CreateProbe(executor);

        var result = await probe.RunAsync(ValidTarget(), ValidAuthorityResult(), request);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, executor.CallCount);
        var evidence = result.Value!.Association.EdgeAuthoritySelectionQualityEvidence;
        Assert.False(evidence.NoveltyEvaluation.Measurement.MeetsNoveltyGate);
        Assert.False(evidence.SelectionAblation.MeetsQ11NumericalGate);
    }

    [Fact]
    public async Task RunAsync_DeclaredExecutorFailurePropagatesUnchanged()
    {
        var executor = RecordingExecutor.Returning(
            OperationResult<DynamicGraphL3SelectedTargetExecutionResult>.Fail(
                "Declared selected target failure.",
                "DECLARED_SELECTED_TARGET_FAILURE"));

        var result = await RunValid(CreateProbe(executor));

        AssertReceiptFailure(
            result,
            "DECLARED_SELECTED_TARGET_FAILURE",
            "Declared selected target failure.");
        Assert.Equal(1, executor.CallCount);
    }

    [Theory]
    [InlineData("null-task")]
    [InlineData("null-envelope")]
    [InlineData("null-value")]
    public async Task RunAsync_MissingExecutionEnvelopeOrValueReturnsResultRequired(string scenario)
    {
        var executor = scenario switch
        {
            "null-task" => new RecordingExecutor((_, _) => null!),
            "null-envelope" => new RecordingExecutor((_, _) =>
                Task.FromResult<OperationResult<DynamicGraphL3SelectedTargetExecutionResult>>(null!)),
            "null-value" => RecordingExecutor.Returning(
                OperationResult<DynamicGraphL3SelectedTargetExecutionResult>.Ok(null!)),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };

        var result = await RunValid(CreateProbe(executor));

        AssertReceiptFailure(result, ResultRequiredCode, ResultRequiredMessage);
        Assert.Equal(1, executor.CallCount);
    }

    [Theory]
    [InlineData("sync")]
    [InlineData("async")]
    public async Task RunAsync_NonfatalExecutorExceptionReturnsExecutionFailed(string scenario)
    {
        var exception = new InvalidOperationException("nonfatal executor failure");
        var executor = scenario == "sync"
            ? new RecordingExecutor((_, _) => throw exception)
            : new RecordingExecutor((_, _) =>
                Task.FromException<OperationResult<DynamicGraphL3SelectedTargetExecutionResult>>(
                    exception));

        var result = await RunValid(CreateProbe(executor));

        AssertReceiptFailure(result, ExecutionFailedCode, ExecutionFailedMessage);
        Assert.Equal(1, executor.CallCount);
    }

    [Fact]
    public async Task RunAsync_RejectsEveryMalformedExecutionResultAndNonfatalTraversal()
    {
        var malformed = new List<DynamicGraphL3SelectedTargetExecutionResult>
        {
            new(null!, ExecutionId, ["evidence"]),
            new(" target", ExecutionId, ["evidence"]),
            new(EdgeId, null!, ["evidence"]),
            new(EdgeId, "execution\ninvalid", ["evidence"]),
            new(EdgeId, ExecutionId, []),
            new(EdgeId, ExecutionId, ["evidence", "evidence"]),
            new(EdgeId, ExecutionId, ["valid", " invalid"]),
            new(EdgeId, ExecutionId, ["\ud800"])
        };
        var corrupted = new DynamicGraphL3SelectedTargetExecutionResult(
            EdgeId,
            ExecutionId,
            ["evidence"]);
        GetEvidenceIdsField().SetValue(corrupted, null);
        malformed.Add(corrupted);

        foreach (var value in malformed)
        {
            var executor = RecordingExecutor.Returning(
                OperationResult<DynamicGraphL3SelectedTargetExecutionResult>.Ok(value));

            var result = await RunValid(CreateProbe(executor));

            AssertReceiptFailure(result, ResultInvalidCode, ResultInvalidMessage);
            Assert.Equal(1, executor.CallCount);
        }
    }

    [Theory]
    [InlineData("different-edge")]
    [InlineData("EDGE-SELECTION-TARGET-EXECUTION")]
    [InlineData(AuthorityUnitId)]
    [InlineData(EvaluationId)]
    public async Task RunAsync_RequiresOrdinalResultTargetMatch(string returnedTargetId)
    {
        var executor = RecordingExecutor.Returning(
            OperationResult<DynamicGraphL3SelectedTargetExecutionResult>.Ok(
                new DynamicGraphL3SelectedTargetExecutionResult(
                    returnedTargetId,
                    ExecutionId,
                    ["evidence"])));

        var result = await RunValid(CreateProbe(executor));

        AssertReceiptFailure(result, TargetMismatchCode, TargetMismatchMessage);
    }

    [Fact]
    public async Task RunAsync_CancellationRaisedDuring20AhStopsBeforeExecutor()
    {
        using var cancellation = new CancellationTokenSource();
        var paired = Paired();
        var requested = paired.Rows.Select(row => row.RowId).ToArray();
        var request = ValidNoveltyRequest(
            paired,
            requestedRowIds: new CancellingReadOnlyList<string>(requested, cancellation));
        var executor = SuccessfulExecutor();

        var result = await CreateProbe(executor).RunAsync(
            ValidTarget(),
            ValidAuthorityResult(),
            request,
            cancellation.Token);

        AssertCancelled(result);
        Assert.Equal(0, executor.CallCount);
    }

    [Fact]
    public async Task RunAsync_RequestedExecutorCancellationReturnsCancelled()
    {
        using var cancellation = new CancellationTokenSource();
        var exception = new OperationCanceledException("requested cancellation");
        var executor = new RecordingExecutor((_, _) =>
        {
            cancellation.Cancel();
            return Task.FromException<
                OperationResult<DynamicGraphL3SelectedTargetExecutionResult>>(exception);
        });

        var result = await CreateProbe(executor).RunAsync(
            ValidTarget(),
            ValidAuthorityResult(),
            ValidNoveltyRequest(),
            cancellation.Token);

        AssertCancelled(result);
        Assert.Equal(1, executor.CallCount);
    }

    [Fact]
    public async Task RunAsync_CancellationAfterAwaitWinsBeforePublication()
    {
        using var cancellation = new CancellationTokenSource();
        var executor = new RecordingExecutor(async (_, _) =>
        {
            await Task.Yield();
            cancellation.Cancel();
            return SuccessfulExecution();
        });

        var result = await CreateProbe(executor).RunAsync(
            ValidTarget(),
            ValidAuthorityResult(),
            ValidNoveltyRequest(),
            cancellation.Token);

        AssertCancelled(result);
        Assert.Equal(1, executor.CallCount);
    }

    [Fact]
    public async Task RunAsync_UnrequestedOperationCancellationEscapesSameInstance()
    {
        var exception = new OperationCanceledException("not requested");
        var executor = new RecordingExecutor((_, _) =>
            Task.FromException<OperationResult<DynamicGraphL3SelectedTargetExecutionResult>>(
                exception));

        var thrown = await Assert.ThrowsAsync<OperationCanceledException>(
            () => RunValid(CreateProbe(executor)));

        Assert.Same(exception, thrown);
        Assert.Equal(1, executor.CallCount);
    }

    [Theory]
    [InlineData("direct")]
    [InlineData("wrapped")]
    [InlineData("aggregate-not-first")]
    public async Task RunAsync_DirectWrappedAndAggregateFatalGraphsEscapeSameOuter(
        string scenario)
    {
        var exception = FatalGraph(scenario);
        var executor = new RecordingExecutor((_, _) =>
            Task.FromException<OperationResult<DynamicGraphL3SelectedTargetExecutionResult>>(
                exception));

        Exception? thrown = null;
        try
        {
            await RunValid(CreateProbe(executor));
        }
        catch (Exception caught)
        {
            thrown = caught;
        }

        Assert.Same(exception, thrown);
        Assert.Equal(1, executor.CallCount);
    }

    [Fact]
    public async Task RunAsync_CyclicFatalGraphEscapesSameOuterWithinBoundedTraversal()
    {
        var innerField = GetExceptionInnerField();
        var cycle = new InvalidOperationException("cyclic nonfatal node");
        innerField.SetValue(cycle, cycle);
        Assert.Same(cycle, cycle.InnerException);
        var exception = new AggregateException(
            "cyclic graph with non-first fatal node",
            new BadImageFormatException("fatal visited after cycle"),
            cycle);
        var executor = new RecordingExecutor((_, _) =>
            Task.FromException<OperationResult<DynamicGraphL3SelectedTargetExecutionResult>>(
                exception));
        Exception? thrown = null;

        try
        {
            try
            {
                await Task.Run(() => RunValid(CreateProbe(executor)))
                    .WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (Exception caught)
            {
                thrown = caught;
            }
        }
        finally
        {
            innerField.SetValue(cycle, null);
        }

        Assert.Same(exception, thrown);
        Assert.Equal(1, executor.CallCount);
    }

    [Fact]
    public async Task RunAsync_DeepOwnsCallerResultExecutorAssociationAndReturnedSnapshots()
    {
        var callerEvidence = new List<string> { "evidence-a", "evidence-b" };
        var executorResult = new DynamicGraphL3SelectedTargetExecutionResult(
            EdgeId,
            ExecutionId,
            callerEvidence);
        var executor = RecordingExecutor.Returning(
            OperationResult<DynamicGraphL3SelectedTargetExecutionResult>.Ok(executorResult));

        var result = await RunValid(CreateProbe(executor));
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var receipt = result.Value!;

        callerEvidence[0] = "caller-mutated";
        Assert.IsType<string[]>(GetEvidenceIdsField().GetValue(executorResult))[0] =
            "executor-result-mutated";
        MutateAssociationTarget(executor.SeenAssociation!, "executor-association-mutated");

        Assert.Equal(EdgeId, receipt.Association.Target.TargetId, StringComparer.Ordinal);
        Assert.Equal("evidence-a", receipt.ExecutionResult.EvidenceIds[0], StringComparer.Ordinal);

        var returnedAssociation = receipt.Association;
        var returnedExecution = receipt.ExecutionResult;
        MutateAssociationTarget(returnedAssociation, "returned-association-mutated");
        Assert.IsType<string[]>(GetEvidenceIdsField().GetValue(returnedExecution))[0] =
            "returned-execution-mutated";

        Assert.Equal(EdgeId, receipt.Association.Target.TargetId, StringComparer.Ordinal);
        Assert.Equal("evidence-a", receipt.ExecutionResult.EvidenceIds[0], StringComparer.Ordinal);
        Assert.NotSame(receipt.Association, receipt.Association);
        Assert.NotSame(receipt.ExecutionResult, receipt.ExecutionResult);
    }

    [Fact]
    public void RunAsync_HasAsyncStateMachineAndOnlyPlannedExceptionHandlers()
    {
        var rawMethod = Assert.IsAssignableFrom<MethodInfo>(
            typeof(DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe).GetMethod(
                nameof(DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe.RunAsync),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        var admittedMethod = Assert.IsAssignableFrom<MethodInfo>(
            typeof(DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe).GetMethod(
                nameof(DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe.RunAdmittedAsync),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        var commonTailMethod = Assert.IsAssignableFrom<MethodInfo>(
            typeof(DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe).GetMethod(
                "ExecuteAdmittedCoreAsync",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

        Assert.False(rawMethod.IsGenericMethod);
        Assert.False(admittedMethod.IsGenericMethod);
        Assert.False(commonTailMethod.IsGenericMethod);
        AssertHandlerShape(AsyncBody(rawMethod), expectedFilters: 0);
        AssertHandlerShape(AsyncBody(admittedMethod), expectedFilters: 2);
        AssertHandlerShape(
            AsyncBody(commonTailMethod),
            expectedFilters: 3,
            typeof(ArgumentOutOfRangeException),
            typeof(OverflowException));

        static MethodBody AsyncBody(MethodInfo method)
        {
            var stateMachine = Assert.IsType<AsyncStateMachineAttribute>(
                method.GetCustomAttribute<AsyncStateMachineAttribute>());
            var moveNext = Assert.IsAssignableFrom<MethodInfo>(
                stateMachine.StateMachineType.GetMethod(
                    "MoveNext",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
            return Assert.IsAssignableFrom<MethodBody>(moveNext.GetMethodBody());
        }

        static void AssertHandlerShape(
            MethodBody body,
            int expectedFilters,
            params Type[] expectedTypedCatches)
        {
            Assert.Equal(
                expectedFilters + expectedTypedCatches.Length + 1,
                body.ExceptionHandlingClauses.Count);
            Assert.Equal(
                expectedFilters,
                body.ExceptionHandlingClauses.Count(clause => clause.Flags ==
                    ExceptionHandlingClauseOptions.Filter));
            Assert.Equal(
                expectedTypedCatches.OrderBy(type => type.FullName, StringComparer.Ordinal),
                body.ExceptionHandlingClauses
                    .Where(clause =>
                        clause.Flags == ExceptionHandlingClauseOptions.Clause
                        && clause.CatchType != typeof(Exception))
                    .Select(clause => clause.CatchType!)
                    .OrderBy(type => type.FullName, StringComparer.Ordinal));
            var asyncInfrastructureCatch = Assert.Single(
                body.ExceptionHandlingClauses,
                clause => clause.Flags == ExceptionHandlingClauseOptions.Clause
                          && clause.CatchType == typeof(Exception));
            Assert.Equal(typeof(Exception), asyncInfrastructureCatch.CatchType);
        }
    }

    private static RecordingExecutor SuccessfulExecutor()
        => RecordingExecutor.Returning(SuccessfulExecution());

    private static OperationResult<DynamicGraphL3SelectedTargetExecutionResult>
        SuccessfulExecution()
        => OperationResult<DynamicGraphL3SelectedTargetExecutionResult>.Ok(
            new DynamicGraphL3SelectedTargetExecutionResult(
                EdgeId,
                ExecutionId,
                ["evidence-a", "evidence-b"]));

    private static DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe CreateProbe(
        IDynamicGraphL3SelectedTargetExecutor executor)
    {
        var result = DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe.Create(executor);
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return result.Value!;
    }

    private static DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe CreateProbe(
        IDynamicGraphL3SelectedTargetExecutor executor,
        TimeProvider timeProvider)
    {
        var result = DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe.Create(
            executor,
            timeProvider);
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return result.Value!;
    }

    private static DynamicGraphTargetRunSpeedMeasurement DeterministicRunSpeed(
        DynamicGraphMeasurementTarget target,
        long inferenceElapsedMs)
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureRunSpeed(
            target,
            new DynamicGraphRunSpeedObservation(
                EvaluatedRowCount: 1,
                InferenceElapsedMs: inferenceElapsedMs,
                RoutingElapsedMs: 0,
                CandidateElapsedMsSamples: Array.Empty<long>(),
                BatchElapsedMsSamples: Array.Empty<long>()));
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return result.Value!;
    }

    private static void AssertExactRunSpeed(
        DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt receipt,
        long expectedElapsedMs)
    {
        var associationTarget = receipt.Association.Target;
        var runSpeed = receipt.RunSpeed;
        Assert.Equal(associationTarget, runSpeed.Target);
        Assert.Equal(receipt.ExecutionResult.TargetId, runSpeed.Target.TargetId);
        Assert.Equal(1, runSpeed.RunSpeed.EvaluatedRowCount);
        Assert.Equal(0, runSpeed.RunSpeed.CandidateSampleCount);
        Assert.Equal(0, runSpeed.RunSpeed.BatchSampleCount);
        Assert.Equal(expectedElapsedMs, runSpeed.RunSpeed.RunSpeed.InferenceElapsedMs);
        Assert.Equal(
            1000d / expectedElapsedMs,
            runSpeed.RunSpeed.RunSpeed.ThroughputRowsPerSec);
        Assert.Equal(0L, runSpeed.RunSpeed.RunSpeed.RoutingElapsedMs);
        Assert.Null(runSpeed.RunSpeed.RunSpeed.PerCandidateElapsedMs);
        Assert.Null(runSpeed.RunSpeed.RunSpeed.PerBatchElapsedMs);
    }

    private static Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>
        RunValid(DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe probe)
        => probe.RunAsync(ValidTarget(), ValidAuthorityResult(), ValidNoveltyRequest());

    private static DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement
        ValidAssociation()
    {
        var result = DynamicGraphMeasurementCoordinator
            .MeasureEdgeAuthoritySelectionQualityEvidence(
                ValidTarget(),
                ValidAuthorityResult(),
                ValidNoveltyRequest());
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return result.Value!;
    }

    private static DynamicGraphMeasurementTarget ValidTarget()
        => new(
            EdgeId,
            DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
            DynamicGraphMeasurementMechanismKind.LearningSubject,
            false);

    private static DynamicGraphEdgeAuthorityNoveltyEvaluationRequest ValidNoveltyRequest(
        DynamicGraphEdgeAuthorityAblationEvaluationResult? paired = null,
        IReadOnlyList<string>? requestedRowIds = null)
    {
        paired ??= Paired();
        return new DynamicGraphEdgeAuthorityNoveltyEvaluationRequest(
            paired,
            [
                new DynamicGraphEdgeAuthorityNoveltyTrainingBinding(
                    "train-a",
                    "train-hash",
                    [A],
                    [A])
            ],
            requestedRowIds ?? paired.Rows.Select(row => row.RowId).ToArray(),
            Enumerable.Range(0, paired.Rows.Count)
                .Select(index => new DynamicGraphEdgeAuthorityNoveltyRowInput(
                    $"row-{index:D3}",
                    index % 2 == 0 ? B : C,
                    [index % 2 == 0 ? B : C]))
                .ToArray(),
            "fingerprint-normalization-v1",
            "tokenization-v1");
    }

    private static DynamicGraphEdgeAuthorityAblationEvaluationResult Paired(
        int learnedCorrect = 2,
        int baselineCorrect = 1,
        string edgeId = EdgeId)
    {
        const int rowCount = 2;
        var split = new DynamicGraphMeasurementSplitProof(
            ["train-a"],
            ["eval-a"],
            ["train-hash", "eval-hash"],
            0,
            "simhash",
            0.85d);
        var rows = Enumerable.Range(0, rowCount)
            .Select(index =>
            {
                var learnedMatches = index < learnedCorrect;
                var baselineMatches = index < baselineCorrect;
                return new DynamicGraphEdgeAuthorityAblationRowEvidence(
                    $"row-{index:D3}",
                    "eval-a",
                    "eval-hash",
                    "document-000",
                    "repo://eval/a",
                    $"label-{index:D3}",
                    "target-a",
                    learnedMatches ? "target-a" : "target-b",
                    baselineMatches ? "target-a" : "target-b",
                    learnedMatches,
                    baselineMatches);
            })
            .ToArray();
        var learnedAccuracy = (double)learnedCorrect / rowCount;
        var baselineAccuracy = (double)baselineCorrect / rowCount;
        var rawDrop = learnedAccuracy - baselineAccuracy;
        var normalizedDrop = Math.Round(rawDrop, 12, MidpointRounding.AwayFromZero);
        var constructor = Assert.Single(
            typeof(DynamicGraphEdgeAuthorityAblationEvaluationResult)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance),
            candidate => candidate.GetParameters().Length == 12);
        return Assert.IsType<DynamicGraphEdgeAuthorityAblationEvaluationResult>(
            constructor.Invoke(
            [
                EvaluationId,
                edgeId,
                AuthorityUnitId,
                split,
                rows,
                learnedCorrect,
                learnedAccuracy,
                baselineCorrect,
                baselineAccuracy,
                rawDrop,
                normalizedDrop,
                normalizedDrop >= DynamicGraphAblationMeter.MinimumAccuracyDrop
            ]));
    }

    private static EdgeAuthorityExecutionResult ValidAuthorityResult()
    {
        var candidates = Enumerable.Range(0, 3)
            .Select(index => new EdgeCandidate(
                $"target-{(char)('a' + index)}",
                [$"PREDICATE_{(char)('A' + index)}"],
                0.2d + (index * 0.2d),
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole))
            .ToArray();
        var decision = new EdgeAuthorityDecision(
            [("target-c", 0.93d)],
            1,
            false,
            RoutingAuthorityVocabulary.TrainedEdgeUnitRole,
            candidates
                .OrderByDescending(candidate => candidate.BaselineScore)
                .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
                .Select(candidate => candidate.TargetSubPhaseId)
                .ToArray());
        var normalizedInput = new EdgeAuthorityInput(
            EdgeId,
            ["edge", "selection", "target"],
            candidates.SelectMany(candidate => candidate.EvidencePredicates).ToArray(),
            new ContextSignals(
                [Domain.CODE_Q],
                ["completed-sub-phase"],
                ["active-edge"],
                new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.8d }),
            candidates,
            LeakageSignals);
        var orderReport = new DynamicGraphEdgeCandidateOrderLeakageReport(
            EdgeId,
            AuthorityUnitId,
            true,
            Enumerable.Range(0, 5)
                .Select(index => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                    $"candidate-order-permutation-{index + 1:D2}",
                    CreatePermutation(candidates, index),
                    CopyDecision(decision)))
                .ToArray(),
            LeakageSignals);
        var identityReport = new DynamicGraphEdgeIdentityLeakageReport(
            EdgeId,
            AuthorityUnitId,
            true,
            Enumerable.Range(0, 3)
                .Select(index => new DynamicGraphEdgeIdentityControlProbe(
                    IdentityProbeIds[index],
                    IdentityControlledSignals[index],
                    IdentityControlValues[index],
                    CopyDecision(decision)))
                .ToArray(),
            LeakageSignals);

        return new EdgeAuthorityExecutionResult(
            normalizedInput,
            candidates.Select(CopyCandidate).ToArray(),
            CopyDecision(decision),
            new DynamicGraphEdgeAuthorityEvidence(
                EdgeId,
                AuthorityUnitId,
                "L2",
                false,
                "selection-target-baseline-unit",
                "candidate_generator",
                LeakageSignals,
                "selection-target-source-proof",
                "selection-target-cutover-proof",
                "selection-target-ablation-proof"),
            orderReport,
            identityReport,
            0.93d,
            ["evidence"],
            []);
    }

    private static EdgeCandidate[] CreatePermutation(
        IReadOnlyList<EdgeCandidate> candidates,
        int index)
    {
        var source = candidates.Select(CopyCandidate).ToArray();
        var offset = index switch
        {
            0 => 0,
            1 => 1,
            2 => 2,
            3 => -1,
            4 => -2,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
        if (offset < 0)
            return Rotate(source.Reverse().ToArray(), -offset - 1);

        return Rotate(source, offset);
    }

    private static EdgeCandidate[] Rotate(IReadOnlyList<EdgeCandidate> source, int offset)
        => Enumerable.Range(0, source.Count)
            .Select(index => CopyCandidate(source[(index + offset) % source.Count]))
            .ToArray();

    private static EdgeCandidate CopyCandidate(EdgeCandidate source)
        => source with { EvidencePredicates = source.EvidencePredicates.ToArray() };

    private static EdgeAuthorityDecision CopyDecision(EdgeAuthorityDecision source)
        => new(
            source.OrderedTargets.ToArray(),
            source.FanOutCount,
            source.KnowledgeEdge,
            source.AuthoritySource,
            source.AblationBaselineRank.ToArray());

    private static void AssertAssociationEquivalent(
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement expected,
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement actual)
    {
        Assert.Equal(expected.Target, actual.Target);
        var expectedEvidence = expected.EdgeAuthoritySelectionQualityEvidence;
        var actualEvidence = actual.EdgeAuthoritySelectionQualityEvidence;
        Assert.Equal(
            expectedEvidence.EdgeAuthorityControl.AuthorityEvidence.EdgeId,
            actualEvidence.EdgeAuthorityControl.AuthorityEvidence.EdgeId,
            StringComparer.Ordinal);
        Assert.Equal(
            expectedEvidence.EdgeAuthorityControl.AuthorityEvidence.AuthorityUnitId,
            actualEvidence.EdgeAuthorityControl.AuthorityEvidence.AuthorityUnitId,
            StringComparer.Ordinal);
        Assert.Equal(
            expectedEvidence.NoveltyEvaluation.PairedSelection.EvaluationId,
            actualEvidence.NoveltyEvaluation.PairedSelection.EvaluationId,
            StringComparer.Ordinal);
        Assert.Equal(
            expectedEvidence.NoveltyEvaluation.RequestedRowIds,
            actualEvidence.NoveltyEvaluation.RequestedRowIds);
        Assert.Equal(
            expectedEvidence.NoveltyEvaluation.Measurement.MeetsNoveltyGate,
            actualEvidence.NoveltyEvaluation.Measurement.MeetsNoveltyGate);
        Assert.Equal(
            expectedEvidence.SelectionAblation.MeetsQ11NumericalGate,
            actualEvidence.SelectionAblation.MeetsQ11NumericalGate);
    }

    private static void AssertOwnedShape<T>(params (string Name, Type Type)[] expected)
    {
        var properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .OrderBy(property => property.MetadataToken)
            .ToArray();
        Assert.Equal(expected.Select(item => item.Name), properties.Select(property => property.Name));
        Assert.Equal(expected.Select(item => item.Type), properties.Select(property => property.PropertyType));
        Assert.All(properties, property => Assert.Null(property.SetMethod));
        Assert.Empty(typeof(T).GetFields(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.All(
            typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            field => Assert.True(field.IsInitOnly));
    }

    private static void AssertCancelled(
        OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt> result)
        => AssertReceiptFailure(result, "CANCELLED", "Cancelled.");

    private static void AssertReceiptFailure(
        OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode, StringComparer.Ordinal);
        Assert.Equal(expectedMessage, result.ErrorMessage, StringComparer.Ordinal);
    }

    private static FieldInfo GetEvidenceIdsField()
        => Assert.IsAssignableFrom<FieldInfo>(
            typeof(DynamicGraphL3SelectedTargetExecutionResult).GetField(
                "_evidenceIds",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    private static FieldInfo GetAssociationEvidenceField()
        => Assert.IsAssignableFrom<FieldInfo>(
            typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement).GetField(
                "_edgeAuthoritySelectionQualityEvidence",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    private static string[] RequestedRowIdsBackingArray(
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement association)
    {
        var selectionField = Assert.IsAssignableFrom<FieldInfo>(
            typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement).GetField(
                "_edgeAuthoritySelectionQualityEvidence",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        var selection = Assert.IsType<
            DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement>(
            selectionField.GetValue(association));
        var noveltyField = Assert.IsAssignableFrom<FieldInfo>(
            typeof(DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement).GetField(
                "_noveltyEvaluation",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        var novelty = Assert.IsType<DynamicGraphEdgeAuthorityNoveltyEvaluationResult>(
            noveltyField.GetValue(selection));
        var requestedRowsField = Assert.IsAssignableFrom<FieldInfo>(
            typeof(DynamicGraphEdgeAuthorityNoveltyEvaluationResult).GetField(
                "_requestedRowIds",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        return Assert.IsType<string[]>(requestedRowsField.GetValue(novelty));
    }

    private static void MutateAssociationTarget(
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement association,
        string targetId)
    {
        var field = Assert.IsAssignableFrom<FieldInfo>(
            typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement).GetField(
                "_target",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        var target = Assert.IsType<DynamicGraphMeasurementTarget>(field.GetValue(association));
        var targetIdField = Assert.IsAssignableFrom<FieldInfo>(
            typeof(DynamicGraphMeasurementTarget).GetField(
                "<TargetId>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        targetIdField.SetValue(target, targetId);
    }

    private static void MutateRunSpeed(
        DynamicGraphTargetRunSpeedMeasurement runSpeed,
        string targetId,
        long inferenceElapsedMs)
    {
        var targetIdField = Assert.IsAssignableFrom<FieldInfo>(
            typeof(DynamicGraphMeasurementTarget).GetField(
                "<TargetId>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        targetIdField.SetValue(runSpeed.Target, targetId);
        var inferenceField = Assert.IsAssignableFrom<FieldInfo>(
            typeof(DynamicGraphRunSpeedAxis).GetField(
                "<InferenceElapsedMs>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        inferenceField.SetValue(runSpeed.RunSpeed.RunSpeed, inferenceElapsedMs);
    }

    private static Exception FatalGraph(string scenario)
    {
        if (scenario == "direct")
            return new BadImageFormatException("direct fatal");
        if (scenario == "wrapped")
        {
            return new InvalidOperationException(
                "wrapped fatal",
                new BadImageFormatException("nested fatal"));
        }

        if (scenario == "aggregate-not-first")
        {
            return new AggregateException(
                "aggregate fatal",
                new InvalidOperationException("ordinary first"),
                new BadImageFormatException("non-first fatal"));
        }

        throw new ArgumentOutOfRangeException(nameof(scenario));
    }

    private static FieldInfo GetExceptionInnerField()
        => Assert.Single(
            typeof(Exception).GetFields(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            field => field.FieldType == typeof(Exception));

    private sealed class RecordingExecutor : IDynamicGraphL3SelectedTargetExecutor
    {
        private readonly Func<
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement,
            CancellationToken,
            Task<OperationResult<DynamicGraphL3SelectedTargetExecutionResult>>> _execute;
        private int _callCount;

        internal RecordingExecutor(
            Func<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement,
                CancellationToken,
                Task<OperationResult<DynamicGraphL3SelectedTargetExecutionResult>>> execute)
        {
            _execute = execute;
        }

        internal int CallCount => Volatile.Read(ref _callCount);

        internal DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement?
            SeenAssociation
        { get; private set; }

        internal CancellationToken SeenToken { get; private set; }

        public Task<OperationResult<DynamicGraphL3SelectedTargetExecutionResult>> ExecuteAsync(
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement association,
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _callCount);
            SeenAssociation = association;
            SeenToken = cancellationToken;
            return _execute(association, cancellationToken);
        }

        internal static RecordingExecutor Returning(
            OperationResult<DynamicGraphL3SelectedTargetExecutionResult> result)
            => new((_, _) => Task.FromResult(result));
    }

    private sealed class TracingTimeProvider : TimeProvider
    {
        private readonly object _gate = new();
        private readonly List<string> _trace = [];
        private readonly long _startedTimestamp;
        private readonly long _completedTimestamp;
        private readonly Exception? _startException;
        private readonly Exception? _endException;
        private int _timestampReadCount;
        private int _elapsedConversionCount;

        internal TracingTimeProvider(
            long startedTimestamp,
            long completedTimestamp,
            Exception? startException = null,
            Exception? endException = null)
        {
            _startedTimestamp = startedTimestamp;
            _completedTimestamp = completedTimestamp;
            _startException = startException;
            _endException = endException;
        }

        public override long TimestampFrequency
        {
            get
            {
                Interlocked.Increment(ref _elapsedConversionCount);
                return TimeSpan.TicksPerSecond;
            }
        }

        public override long GetTimestamp()
        {
            var ordinal = Interlocked.Increment(ref _timestampReadCount);
            var isStart = ordinal == 1;
            Record(isStart ? "timestamp-start" : "timestamp-end");
            var exception = isStart ? _startException : _endException;
            if (exception is not null)
                throw exception;
            return isStart ? _startedTimestamp : _completedTimestamp;
        }

        internal int TimestampReadCount => Volatile.Read(ref _timestampReadCount);

        internal int ElapsedConversionCount => Volatile.Read(ref _elapsedConversionCount);

        internal IReadOnlyList<string> Trace
        {
            get
            {
                lock (_gate)
                    return _trace.ToArray();
            }
        }

        internal void Record(string value)
        {
            lock (_gate)
                _trace.Add(value);
        }
    }

    private sealed class CancellingReadOnlyList<T> : IReadOnlyList<T>
    {
        private readonly IReadOnlyList<T> _source;
        private readonly CancellationTokenSource _cancellation;

        internal CancellingReadOnlyList(
            IReadOnlyList<T> source,
            CancellationTokenSource cancellation)
        {
            _source = source;
            _cancellation = cancellation;
        }

        public int Count
        {
            get
            {
                _cancellation.Cancel();
                return _source.Count;
            }
        }

        public T this[int index]
        {
            get
            {
                _cancellation.Cancel();
                return _source[index];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            _cancellation.Cancel();
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
