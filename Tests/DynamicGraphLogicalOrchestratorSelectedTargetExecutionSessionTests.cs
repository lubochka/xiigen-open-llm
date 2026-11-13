// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ExperimentsV2.LogicalFlow;
using OrchestratorModel.Logical;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

public sealed class DynamicGraphLogicalOrchestratorSelectedTargetExecutionSessionTests
{
    private const string EdgeId = SecurityVerification34FlowAdapter.FlowId;
    private const string RunId = "adapter-selected-target-run";
    private const string ScenarioId = SecurityVerification34FlowAdapter.ScenarioId;
    private const string UserRequest = "Execute the already-admitted selected target.";
    private const string AuthorityUnitId = "adapter-selected-target-authority";
    private const string EvaluationId = "adapter-selected-target-evaluation";
    private const string AblationDomainTag = "adapter-selected-target-domain";
    private const string A = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string B = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string C = "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";
    private const string RequestInvalidCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_REQUEST_INVALID";
    private const string RequestInvalidMessage =
        "Dynamic graph L3 selected-target flow request is invalid.";
    private const string RequestRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_SELECTED_TARGET_EXECUTION_REQUEST_REQUIRED";
    private const string RequestRequiredMessage =
        "A dynamic graph logical orchestrator selected-target execution request is required.";
    private const string ResultRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_SELECTED_TARGET_EXECUTION_RESULT_REQUIRED";
    private const string ResultRequiredMessage =
        "A dynamic graph logical orchestrator selected-target execution result is required.";

    private static readonly string[] ExactLeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];
    private static readonly string[] AblationTargets = ["target-a", "target-b", "target-c"];
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
    public void PublicSurface_IsExactAndExposesNoRuntimePolicyOrTrainingControls()
    {
        var requestType = typeof(DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest);
        Assert.True(requestType.IsPublic);
        Assert.True(requestType.IsSealed);
        var constructor = Assert.Single(requestType.GetConstructors(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Equal(
            [
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(DynamicGraphMeasurementTarget),
                typeof(EdgeAuthorityExecutionResult),
                typeof(DynamicGraphEdgeAuthorityNoveltyEvaluationRequest)
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["RunId", "ScenarioId", "UserRequest", "Target", "AuthorityResult", "NoveltyRequest"],
            constructor.GetParameters().Select(parameter => parameter.Name));
        Assert.All(constructor.GetParameters(), parameter => Assert.False(parameter.HasDefaultValue));
        Assert.Equal(
            ["RunId", "ScenarioId", "UserRequest", "Target", "AuthorityResult", "NoveltyRequest"],
            requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .OrderBy(property => property.MetadataToken)
                .Select(property => property.Name));

        var contract = typeof(IDynamicGraphLogicalOrchestratorSelectedTargetExecutionSession);
        Assert.True(contract.IsPublic);
        Assert.True(contract.IsInterface);
        var method = Assert.Single(contract.GetMethods(BindingFlags.Public | BindingFlags.Instance));
        Assert.Equal("RunAsync", method.Name, StringComparer.Ordinal);
        Assert.Equal(
            typeof(Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>),
            method.ReturnType);
        var parameters = method.GetParameters();
        Assert.Equal(
            [
                typeof(DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest),
                typeof(CancellationToken)
            ],
            parameters.Select(parameter => parameter.ParameterType));
        Assert.False(parameters[0].HasDefaultValue);
        Assert.True(parameters[1].HasDefaultValue);
        Assert.Null(parameters[1].DefaultValue);

        var admittedRequestType = typeof(
            DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest);
        Assert.True(admittedRequestType.IsPublic);
        Assert.True(admittedRequestType.IsSealed);
        var admittedConstructor = Assert.Single(admittedRequestType.GetConstructors(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Equal(
            [
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement)
            ],
            admittedConstructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["RunId", "ScenarioId", "UserRequest", "Association"],
            admittedConstructor.GetParameters().Select(parameter => parameter.Name));
        Assert.All(admittedConstructor.GetParameters(), parameter => Assert.False(
            parameter.HasDefaultValue));
        Assert.Equal(
            ["RunId", "ScenarioId", "UserRequest", "Association"],
            admittedRequestType.GetProperties(
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .OrderBy(property => property.MetadataToken)
                .Select(property => property.Name));

        var admittedContract = typeof(
            IDynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionSession);
        Assert.True(admittedContract.IsPublic);
        Assert.True(admittedContract.IsInterface);
        var admittedMethod = Assert.Single(admittedContract.GetMethods(
            BindingFlags.Public | BindingFlags.Instance));
        Assert.Equal("RunAdmittedAsync", admittedMethod.Name, StringComparer.Ordinal);
        Assert.Equal(
            typeof(Task<OperationResult<
                DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>),
            admittedMethod.ReturnType);
        var admittedParameters = admittedMethod.GetParameters();
        Assert.Equal(
            [
                typeof(DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest),
                typeof(CancellationToken)
            ],
            admittedParameters.Select(parameter => parameter.ParameterType));
        Assert.False(admittedParameters[0].HasDefaultValue);
        Assert.True(admittedParameters[1].HasDefaultValue);
        Assert.Null(admittedParameters[1].DefaultValue);

        var publicTypes = PublicSignatureTypes(requestType)
            .Concat(PublicSignatureTypes(contract))
            .Concat(PublicSignatureTypes(admittedRequestType))
            .Concat(PublicSignatureTypes(admittedContract))
            .ToArray();
        Assert.DoesNotContain(publicTypes, type => type == typeof(LogicalOrchestratorRequest));
        Assert.DoesNotContain(publicTypes, type => type == typeof(LogicalExecutionFlags));
        Assert.DoesNotContain(publicTypes, type => type == typeof(LogicalUnitTrustState));
        Assert.DoesNotContain(publicTypes, type => type == typeof(LogicalOrchestratorAssistant));
        Assert.DoesNotContain(publicTypes, type => type == typeof(ISessionStorage));
        Assert.DoesNotContain(
            publicTypes,
            type => type.FullName?.Contains("Training", StringComparison.OrdinalIgnoreCase) == true
                    || type.FullName?.Contains("DPO", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public async Task ComposedSession_ProvidesNewCastWhileLegacyRunAndTopologyRemainOperational()
    {
        var invoker = new RecordingInvoker();
        var handles = Compose(invoker);
        var before = AssertSuccess(handles.Composition.ReadCurrent());

        Assert.Same(handles.Composition, handles.SelectedTarget);
        Assert.Same(handles.Composition, handles.AdmittedSelectedTarget);
        var legacy = Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorRunSession>(
            handles.Composition);
        Assert.Same(handles.Composition, legacy);
        var legacyResult = await legacy.RunAsync(
            new DynamicGraphLogicalOrchestratorRunRequest(
                "Check the technical-design to implementation transition."));

        Assert.True(
            legacyResult.IsSuccess,
            $"{legacyResult.ErrorCode}: {legacyResult.ErrorMessage}");
        Assert.Equal(T34LogicalFlowAdapter.FlowId, legacyResult.Value!.SelectedFlowId);
        Assert.Equal(before, AssertSuccess(handles.Composition.ReadCurrent()));
        Assert.Equal(0, invoker.CallCount);
        Assert.Equal(0, invoker.AdmittedCallCount);
    }

    [Fact]
    public async Task RunAsync_NullWrapperWinsOverPreCancellationWithoutInvokingAssistantBoundary()
    {
        var invoker = new RecordingInvoker();
        var session = Compose(invoker).SelectedTarget;
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await session.RunAsync(null!, cancellation.Token);

        AssertFailure(result, RequestRequiredCode, RequestRequiredMessage);
        Assert.Equal(0, invoker.CallCount);
    }

    [Fact]
    public async Task RunAsync_InvokesOnceWithExactContextAndSameEvidenceCarrierIdentities()
    {
        var invoker = new RecordingInvoker();
        var session = Compose(invoker).SelectedTarget;
        var target = ValidTarget(EdgeId);
        var authority = ValidAuthorityResult(EdgeId);
        var novelty = (await ValidNoveltyCarrierAsync(EdgeId)).Request;
        using var cancellation = new CancellationTokenSource();
        var request = new DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest(
            RunId,
            ScenarioId,
            UserRequest,
            target,
            authority,
            novelty);

        var result = await session.RunAsync(request, cancellation.Token);

        AssertFailure(result, RecordingInvoker.StopCode, RecordingInvoker.StopMessage);
        var call = Assert.Single(invoker.Calls);
        Assert.Equal(RunId, call.ExecutionContext.RunId, StringComparer.Ordinal);
        Assert.Equal(ScenarioId, call.ExecutionContext.ScenarioId, StringComparer.Ordinal);
        Assert.Equal(UserRequest, call.ExecutionContext.UserRequest, StringComparer.Ordinal);
        Assert.Same(target, call.Target);
        Assert.Same(authority, call.AuthorityResult);
        Assert.Same(novelty, call.NoveltyRequest);
        Assert.Equal(cancellation.Token, call.CancellationToken);
    }

    [Fact]
    public async Task RunAsync_ForwardsTheExactAssistantRetainedByTheComposedRuntime()
    {
        var invoker = new RecordingInvoker();
        var handles = Compose(invoker);
        var runtime = Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorRuntimeSessionAccessor>(
            handles.Composition).Runtime;

        var result = await handles.SelectedTarget.RunAsync(await ValidRequestAsync());

        AssertFailure(result, RecordingInvoker.StopCode, RecordingInvoker.StopMessage);
        Assert.Same(runtime.Assistant, Assert.Single(invoker.Calls).Assistant);
    }

    [Fact]
    public async Task RunAsync_CreatesFreshFlagsWithSixBoundaryOverridesAndAllOtherDefaults()
    {
        var invoker = new RecordingInvoker();
        var session = Compose(invoker).SelectedTarget;

        await session.RunAsync(await ValidRequestAsync(runId: "flags-run-one"));
        await session.RunAsync(await ValidRequestAsync(runId: "flags-run-two"));

        var calls = invoker.Calls;
        Assert.Equal(2, calls.Count);
        var first = Assert.IsType<LogicalExecutionFlags>(calls[0].ExecutionContext.Flags);
        var second = Assert.IsType<LogicalExecutionFlags>(calls[1].ExecutionContext.Flags);
        Assert.NotSame(first, second);
        AssertNonLearningFlags(first);
        AssertNonLearningFlags(second);
    }

    [Fact]
    public async Task RunAsync_CreatesFreshEmptyOrdinalMetadataAndDoesNotRetainInvokerMutation()
    {
        var invoker = new RecordingInvoker
        {
            Handler = call =>
            {
                var metadata = Assert.IsType<Dictionary<string, object>>(
                    call.ExecutionContext.Metadata);
                if (call.Ordinal == 1)
                    metadata["poison"] = new object();
                return RecordingInvoker.Stop();
            }
        };
        var session = Compose(invoker).SelectedTarget;

        await session.RunAsync(await ValidRequestAsync(runId: "metadata-run-one"));
        await session.RunAsync(await ValidRequestAsync(runId: "metadata-run-two"));

        var calls = invoker.Calls;
        var first = Assert.IsType<Dictionary<string, object>>(calls[0].ExecutionContext.Metadata);
        var second = Assert.IsType<Dictionary<string, object>>(calls[1].ExecutionContext.Metadata);
        Assert.NotSame(first, second);
        Assert.Equal(StringComparer.Ordinal, first.Comparer);
        Assert.Equal(StringComparer.Ordinal, second.Comparer);
        Assert.Equal(["poison"], first.Keys);
        Assert.Empty(second);
    }

    [Fact]
    public async Task RunAsync_UsesFixedTrustedPolicyThatIsAbsentFromThePublicBoundary()
    {
        var invoker = new RecordingInvoker();
        var session = Compose(invoker).SelectedTarget;

        var result = await session.RunAsync(await ValidRequestAsync());

        AssertFailure(result, RecordingInvoker.StopCode, RecordingInvoker.StopMessage);
        Assert.Equal(LogicalUnitTrustState.Trusted, Assert.Single(invoker.Calls).MinimumTrustState);
        Assert.DoesNotContain(
            typeof(DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest).GetProperties(),
            property => property.Name.Contains("Trust", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            typeof(IDynamicGraphLogicalOrchestratorSelectedTargetExecutionSession)
                .GetMethods()
                .SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType == typeof(LogicalUnitTrustState)
                         || parameter.Name?.Contains("trust", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public async Task RunAsync_MalformedContextWinsOverPreCancellationBeforeEvidenceTraversal()
    {
        var session = ComposeProduction();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var request = new DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest(
            " malformed-run",
            ScenarioId,
            UserRequest,
            null,
            null,
            ThrowingNoveltyRequest());

        var result = await session.RunAsync(request, cancellation.Token);

        AssertFailure(result, RequestInvalidCode, RequestInvalidMessage);
    }

    [Fact]
    public async Task RunAsync_ValidContextHonorsPreCancellationBeforeEvidenceTraversal()
    {
        var session = ComposeProduction();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var request = new DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest(
            RunId,
            ScenarioId,
            UserRequest,
            null,
            null,
            ThrowingNoveltyRequest());

        var result = await session.RunAsync(request, cancellation.Token);

        AssertFailure(result, "CANCELLED", "Cancelled.");
    }

    [Fact]
    public async Task RunAsync_MapsDownstreamFailureAndFailsClosedForEveryMissingResultShapeWithoutRetry()
    {
        var downstream = new RecordingInvoker
        {
            Handler = _ => Task.FromResult(
                OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Fail(
                    "Downstream exact failure.",
                    "DOWNSTREAM_EXACT"))
        };
        var downstreamResult = await Compose(downstream).SelectedTarget.RunAsync(
            await ValidRequestAsync());
        AssertFailure(downstreamResult, "DOWNSTREAM_EXACT", "Downstream exact failure.");
        Assert.Equal(1, downstream.CallCount);

        var nullTask = new RecordingInvoker { Handler = _ => null! };
        var nullTaskResult = await Compose(nullTask).SelectedTarget.RunAsync(
            await ValidRequestAsync());
        AssertFailure(nullTaskResult, ResultRequiredCode, ResultRequiredMessage);
        Assert.Equal(1, nullTask.CallCount);

        var nullEnvelope = new RecordingInvoker
        {
            Handler = _ => Task.FromResult<
                OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>(null!)
        };
        var nullEnvelopeResult = await Compose(nullEnvelope).SelectedTarget.RunAsync(
            await ValidRequestAsync());
        AssertFailure(nullEnvelopeResult, ResultRequiredCode, ResultRequiredMessage);
        Assert.Equal(1, nullEnvelope.CallCount);

        var successNull = new RecordingInvoker
        {
            Handler = _ => Task.FromResult(
                OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Ok(null!))
        };
        var successNullResult = await Compose(successNull).SelectedTarget.RunAsync(
            await ValidRequestAsync());
        AssertFailure(successNullResult, ResultRequiredCode, ResultRequiredMessage);
        Assert.Equal(1, successNull.CallCount);
    }

    [Fact]
    public async Task RunAsync_PreservesExactOwnedRunSpeedWithoutRecomputation()
    {
        var association = await ValidAssociationAsync(EdgeId);
        var runSpeed = DeterministicRunSpeed(association.Target, inferenceElapsedMs: 17);
        var downstreamReceipt = new DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt(
            association,
            new DynamicGraphL3SelectedTargetExecutionResult(
                EdgeId,
                "raw-run-speed",
                ["raw-run-speed-evidence"]),
            runSpeed);
        var invoker = new RecordingInvoker
        {
            Handler = _ => Task.FromResult(
                OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Ok(
                    downstreamReceipt))
        };

        var result = await Compose(invoker).SelectedTarget.RunAsync(
            await ValidRequestAsync(runId: "raw-run-speed"));

        var receipt = AssertSuccess(result);
        Assert.NotSame(downstreamReceipt, receipt);
        AssertRunSpeed(receipt.RunSpeed, EdgeId, inferenceElapsedMs: 17);
        Assert.NotSame(downstreamReceipt.RunSpeed, receipt.RunSpeed);
        Assert.NotSame(receipt.RunSpeed, receipt.RunSpeed);
        Assert.NotSame(receipt.RunSpeed.Target, receipt.RunSpeed.Target);
        Assert.NotSame(receipt.RunSpeed.RunSpeed, receipt.RunSpeed.RunSpeed);
        Assert.NotSame(
            receipt.RunSpeed.RunSpeed.RunSpeed,
            receipt.RunSpeed.RunSpeed.RunSpeed);
    }

    [Fact]
    public async Task RunAsync_RealDefaultSecurityS1OwnsResultsPreservesTopologyAndSupportsConcurrentCalls()
    {
        var composition = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter().Compose(CreateProfile()));
        var session = Assert.IsAssignableFrom<
            IDynamicGraphLogicalOrchestratorSelectedTargetExecutionSession>(composition);
        var before = AssertSuccess(composition.ReadCurrent());
        var mutableNovelty = await ValidNoveltyCarrierAsync(EdgeId);

        var firstResult = await session.RunAsync(
            await ValidRequestAsync(
                runId: "real-s1-owned-run",
                userRequest: SecurityScenarioS1(),
                noveltyRequest: mutableNovelty.Request));

        Assert.True(firstResult.IsSuccess, $"{firstResult.ErrorCode}: {firstResult.ErrorMessage}");
        var firstReceipt = firstResult.Value!;
        Assert.Equal(1, firstReceipt.DirectTargetInvocationCount);
        Assert.Equal(EdgeId, firstReceipt.ExecutionResult.TargetId, StringComparer.Ordinal);
        Assert.Equal("real-s1-owned-run", firstReceipt.ExecutionResult.ExecutionId, StringComparer.Ordinal);
        Assert.Equal(["CB4-STAGE-5-S1", "CB4-3.4-001"], firstReceipt.ExecutionResult.EvidenceIds);
        Assert.NotSame(firstReceipt.Association, firstReceipt.Association);
        Assert.NotSame(firstReceipt.ExecutionResult, firstReceipt.ExecutionResult);
        var retainedRowIds = firstReceipt.Association.EdgeAuthoritySelectionQualityEvidence
            .NoveltyEvaluation.RequestedRowIds.ToArray();
        var retainedTrainingRoots = firstReceipt.Association.EdgeAuthoritySelectionQualityEvidence
            .NoveltyEvaluation.TrainingEvidence
            .Select(evidence => evidence.TrainSourceRoot)
            .ToArray();
        mutableNovelty.TrainingBindings.Clear();
        mutableNovelty.RequestedRowIds.Clear();
        mutableNovelty.NoveltyRows.Clear();
        Assert.Equal(
            retainedRowIds,
            firstReceipt.Association.EdgeAuthoritySelectionQualityEvidence
                .NoveltyEvaluation.RequestedRowIds);
        Assert.Equal(
            retainedTrainingRoots,
            firstReceipt.Association.EdgeAuthoritySelectionQualityEvidence
                .NoveltyEvaluation.TrainingEvidence.Select(evidence => evidence.TrainSourceRoot));

        var concurrentRequestA = await ValidRequestAsync(
            runId: "real-s1-concurrent-a",
            userRequest: SecurityScenarioS1());
        var concurrentRequestB = await ValidRequestAsync(
            runId: "real-s1-concurrent-b",
            userRequest: SecurityScenarioS1());
        var concurrent = await Task.WhenAll(
            session.RunAsync(concurrentRequestA),
            session.RunAsync(concurrentRequestB));

        Assert.All(
            concurrent,
            result => Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}"));
        Assert.All(concurrent, result => Assert.Equal(1, result.Value!.DirectTargetInvocationCount));
        Assert.Equal(
            ["real-s1-concurrent-a", "real-s1-concurrent-b"],
            concurrent.Select(result => result.Value!.ExecutionResult.ExecutionId));
        var firstConcurrentReceipt = concurrent[0].Value!;
        var secondConcurrentReceipt = concurrent[1].Value!;
        Assert.NotSame(firstConcurrentReceipt, secondConcurrentReceipt);
        Assert.NotSame(firstConcurrentReceipt.Association, secondConcurrentReceipt.Association);
        Assert.NotSame(
            firstConcurrentReceipt.ExecutionResult,
            secondConcurrentReceipt.ExecutionResult);
        Assert.Equal(before, AssertSuccess(composition.ReadCurrent()));
    }

    [Fact]
    public async Task RunAdmittedAsync_NullWrapperWinsOverPreCancellationWithoutInvokingAssistantBoundary()
    {
        var invoker = new RecordingInvoker();
        var session = Compose(invoker).AdmittedSelectedTarget;
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await session.RunAdmittedAsync(null!, cancellation.Token);

        AssertFailure(result, RequestRequiredCode, RequestRequiredMessage);
        Assert.Equal(0, invoker.AdmittedCallCount);
    }

    [Fact]
    public async Task RunAdmittedAsync_InvokesOnceWithExactCarrierContextTrustTokenAndFreshBoundaryState()
    {
        var invoker = new RecordingInvoker();
        var handles = Compose(invoker);
        var runtime = Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorRuntimeSessionAccessor>(
            handles.Composition).Runtime;
        var firstAssociation = await ValidAssociationAsync(EdgeId);
        var secondAssociation = await ValidAssociationAsync(EdgeId);
        using var cancellation = new CancellationTokenSource();

        var first = await handles.AdmittedSelectedTarget.RunAdmittedAsync(
            new DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest(
                "admitted-context-one",
                ScenarioId,
                UserRequest,
                firstAssociation),
            cancellation.Token);
        var second = await handles.AdmittedSelectedTarget.RunAdmittedAsync(
            new DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest(
                "admitted-context-two",
                ScenarioId,
                "Second admitted request.",
                secondAssociation),
            cancellation.Token);

        AssertFailure(first, RecordingInvoker.StopCode, RecordingInvoker.StopMessage);
        AssertFailure(second, RecordingInvoker.StopCode, RecordingInvoker.StopMessage);
        var calls = invoker.AdmittedCalls;
        Assert.Equal(2, calls.Count);
        Assert.Same(runtime.Assistant, calls[0].Assistant);
        Assert.Equal("admitted-context-one", calls[0].ExecutionContext.RunId);
        Assert.Equal(ScenarioId, calls[0].ExecutionContext.ScenarioId);
        Assert.Equal(UserRequest, calls[0].ExecutionContext.UserRequest);
        Assert.Equal("admitted-context-two", calls[1].ExecutionContext.RunId);
        Assert.Equal("Second admitted request.", calls[1].ExecutionContext.UserRequest);
        Assert.Same(firstAssociation, calls[0].Association);
        Assert.Same(secondAssociation, calls[1].Association);
        Assert.Equal(LogicalUnitTrustState.Trusted, calls[0].MinimumTrustState);
        Assert.Equal(LogicalUnitTrustState.Trusted, calls[1].MinimumTrustState);
        Assert.Equal(cancellation.Token, calls[0].CancellationToken);
        Assert.Equal(cancellation.Token, calls[1].CancellationToken);
        var firstFlags = Assert.IsType<LogicalExecutionFlags>(calls[0].ExecutionContext.Flags);
        var secondFlags = Assert.IsType<LogicalExecutionFlags>(calls[1].ExecutionContext.Flags);
        Assert.NotSame(firstFlags, secondFlags);
        AssertNonLearningFlags(firstFlags);
        AssertNonLearningFlags(secondFlags);
        var firstMetadata = Assert.IsType<Dictionary<string, object>>(
            calls[0].ExecutionContext.Metadata);
        var secondMetadata = Assert.IsType<Dictionary<string, object>>(
            calls[1].ExecutionContext.Metadata);
        Assert.NotSame(firstMetadata, secondMetadata);
        Assert.Equal(StringComparer.Ordinal, firstMetadata.Comparer);
        Assert.Equal(StringComparer.Ordinal, secondMetadata.Comparer);
        Assert.Empty(firstMetadata);
        Assert.Empty(secondMetadata);
    }

    [Fact]
    public async Task RunAdmittedAsync_MapsDeclaredFailureAndFailsClosedForEveryMissingResultShape()
    {
        var association = await ValidAssociationAsync(EdgeId);
        var request = new DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest(
            RunId,
            ScenarioId,
            UserRequest,
            association);
        var downstream = new RecordingInvoker
        {
            AdmittedHandler = _ => Task.FromResult(
                OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Fail(
                    "Admitted downstream exact failure.",
                    "ADMITTED_DOWNSTREAM_EXACT"))
        };
        AssertFailure(
            await Compose(downstream).AdmittedSelectedTarget.RunAdmittedAsync(request),
            "ADMITTED_DOWNSTREAM_EXACT",
            "Admitted downstream exact failure.");
        Assert.Equal(1, downstream.AdmittedCallCount);

        var nullTask = new RecordingInvoker { AdmittedHandler = _ => null! };
        AssertFailure(
            await Compose(nullTask).AdmittedSelectedTarget.RunAdmittedAsync(request),
            ResultRequiredCode,
            ResultRequiredMessage);
        Assert.Equal(1, nullTask.AdmittedCallCount);

        var nullEnvelope = new RecordingInvoker
        {
            AdmittedHandler = _ => Task.FromResult<OperationResult<
                DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>(null!)
        };
        AssertFailure(
            await Compose(nullEnvelope).AdmittedSelectedTarget.RunAdmittedAsync(request),
            ResultRequiredCode,
            ResultRequiredMessage);
        Assert.Equal(1, nullEnvelope.AdmittedCallCount);

        var successNull = new RecordingInvoker
        {
            AdmittedHandler = _ => Task.FromResult(
                OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Ok(null!))
        };
        AssertFailure(
            await Compose(successNull).AdmittedSelectedTarget.RunAdmittedAsync(request),
            ResultRequiredCode,
            ResultRequiredMessage);
        Assert.Equal(1, successNull.AdmittedCallCount);

        var mutableNovelty = await ValidNoveltyCarrierAsync(EdgeId);
        var downstreamAssociation = AssertSuccess(
            DynamicGraphMeasurementCoordinator.MeasureEdgeAuthoritySelectionQualityEvidence(
                ValidTarget(EdgeId),
                ValidAuthorityResult(EdgeId),
                mutableNovelty.Request));
        var evidenceIds = new List<string> { "admitted-success-evidence" };
        var downstreamExecution = new DynamicGraphL3SelectedTargetExecutionResult(
            EdgeId,
            "admitted-success-run",
            evidenceIds);
        var downstreamReceipt = new DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt(
            downstreamAssociation,
            downstreamExecution,
            DeterministicRunSpeed(
                downstreamAssociation.Target,
                inferenceElapsedMs: 37));
        var successful = new RecordingInvoker
        {
            AdmittedHandler = _ => Task.FromResult(
                OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Ok(
                    downstreamReceipt))
        };

        var successfulResult = await Compose(successful).AdmittedSelectedTarget.RunAdmittedAsync(
            new DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest(
                "admitted-success-run",
                ScenarioId,
                UserRequest,
                downstreamAssociation));

        Assert.True(
            successfulResult.IsSuccess,
            $"{successfulResult.ErrorCode}: {successfulResult.ErrorMessage}");
        var returnedReceipt = successfulResult.Value!;
        Assert.NotSame(downstreamReceipt, returnedReceipt);
        Assert.Equal(1, returnedReceipt.DirectTargetInvocationCount);
        Assert.Equal(EdgeId, returnedReceipt.ExecutionResult.TargetId, StringComparer.Ordinal);
        Assert.Equal(
            "admitted-success-run",
            returnedReceipt.ExecutionResult.ExecutionId,
            StringComparer.Ordinal);
        Assert.Equal(["admitted-success-evidence"], returnedReceipt.ExecutionResult.EvidenceIds);
        AssertRunSpeed(returnedReceipt.RunSpeed, EdgeId, inferenceElapsedMs: 37);
        var retainedRows = returnedReceipt.Association.EdgeAuthoritySelectionQualityEvidence
            .NoveltyEvaluation.RequestedRowIds.ToArray();
        evidenceIds.Clear();
        mutableNovelty.TrainingBindings.Clear();
        mutableNovelty.RequestedRowIds.Clear();
        mutableNovelty.NoveltyRows.Clear();
        Assert.Equal(
            retainedRows,
            returnedReceipt.Association.EdgeAuthoritySelectionQualityEvidence
                .NoveltyEvaluation.RequestedRowIds);
        Assert.Equal(["admitted-success-evidence"], returnedReceipt.ExecutionResult.EvidenceIds);
        Assert.NotSame(returnedReceipt.Association, returnedReceipt.Association);
        Assert.NotSame(returnedReceipt.ExecutionResult, returnedReceipt.ExecutionResult);
        Assert.NotSame(returnedReceipt.RunSpeed, returnedReceipt.RunSpeed);
        Assert.NotSame(returnedReceipt.RunSpeed.Target, returnedReceipt.RunSpeed.Target);
        Assert.NotSame(returnedReceipt.RunSpeed.RunSpeed, returnedReceipt.RunSpeed.RunSpeed);
        Assert.NotSame(
            returnedReceipt.RunSpeed.RunSpeed.RunSpeed,
            returnedReceipt.RunSpeed.RunSpeed.RunSpeed);
        Assert.NotSame(downstreamReceipt.Association, returnedReceipt.Association);
        Assert.NotSame(downstreamReceipt.ExecutionResult, returnedReceipt.ExecutionResult);
        Assert.NotSame(downstreamReceipt.RunSpeed, returnedReceipt.RunSpeed);
        Assert.Equal(1, successful.AdmittedCallCount);
    }

    [Fact]
    public async Task RunAdmittedAsync_UnrequestedCancellationAndFatalExceptionEscapeUnchanged()
    {
        var request = new DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest(
            RunId,
            ScenarioId,
            UserRequest,
            await ValidAssociationAsync(EdgeId));
        var cancellation = new RecordingInvoker
        {
            AdmittedHandler = _ => Task.FromException<OperationResult<
                DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>(
                    new OperationCanceledException("Unrequested cancellation."))
        };
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            Compose(cancellation).AdmittedSelectedTarget.RunAdmittedAsync(request));
        Assert.Equal(1, cancellation.AdmittedCallCount);

        var fatal = new RecordingInvoker
        {
            AdmittedHandler = _ => Task.FromException<OperationResult<
                DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>(
                    new OutOfMemoryException("Synthetic fatal boundary."))
        };
        await Assert.ThrowsAsync<OutOfMemoryException>(() =>
            Compose(fatal).AdmittedSelectedTarget.RunAdmittedAsync(request));
        Assert.Equal(1, fatal.AdmittedCallCount);
    }

    [Fact]
    public async Task RunAdmittedAsync_RealDefaultSecurityS1ReturnsOwnedReceiptWithoutTopologyMutation()
    {
        var composition = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter().Compose(CreateProfile()));
        var session = Assert.IsAssignableFrom<
            IDynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionSession>(composition);
        var before = AssertSuccess(composition.ReadCurrent());
        var association = await ValidAssociationAsync(EdgeId);

        var result = await session.RunAdmittedAsync(
            new DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest(
                "admitted-real-s1-run",
                ScenarioId,
                SecurityScenarioS1(),
                association));

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var receipt = result.Value!;
        Assert.Equal(1, receipt.DirectTargetInvocationCount);
        Assert.Equal(EdgeId, receipt.ExecutionResult.TargetId, StringComparer.Ordinal);
        Assert.Equal("admitted-real-s1-run", receipt.ExecutionResult.ExecutionId);
        Assert.Equal(["CB4-STAGE-5-S1", "CB4-3.4-001"], receipt.ExecutionResult.EvidenceIds);
        Assert.NotSame(association, receipt.Association);
        Assert.NotSame(receipt.Association, receipt.Association);
        Assert.NotSame(receipt.ExecutionResult, receipt.ExecutionResult);
        Assert.Equal(before, AssertSuccess(composition.ReadCurrent()));
    }

    [Fact]
    public async Task RunAdmittedAsync_MalformedContextAndPreCancellationKeepFactoryPrecedence()
    {
        var session = ComposeProductionAdmitted();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var malformed = new DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest(
            " malformed-admitted-run",
            ScenarioId,
            UserRequest,
            null);

        var malformedResult = await session.RunAdmittedAsync(malformed, cancellation.Token);
        AssertFailure(malformedResult, RequestInvalidCode, RequestInvalidMessage);

        var validContext = new DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest(
            RunId,
            ScenarioId,
            UserRequest,
            null);
        var cancelled = await session.RunAdmittedAsync(validContext, cancellation.Token);
        AssertFailure(cancelled, "CANCELLED", "Cancelled.");
    }

    private static SessionHandles Compose(RecordingInvoker invoker)
    {
        var composition = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter(
                    new CountingSessionStorage(),
                    projectWikiTrainingFlowAdapter: null,
                    selectedTargetExecutionInvoker: invoker)
                .Compose(CreateProfile()));
        return new SessionHandles(
            composition,
            Assert.IsAssignableFrom<
                IDynamicGraphLogicalOrchestratorSelectedTargetExecutionSession>(composition),
            Assert.IsAssignableFrom<
                IDynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionSession>(
                    composition));
    }

    private static IDynamicGraphLogicalOrchestratorSelectedTargetExecutionSession
        ComposeProduction()
        => Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorSelectedTargetExecutionSession>(
            AssertSuccess(
                new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter()
                    .Compose(CreateProfile())));

    private static IDynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionSession
        ComposeProductionAdmitted()
        => Assert.IsAssignableFrom<
            IDynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionSession>(
            AssertSuccess(
                new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter()
                    .Compose(CreateProfile())));

    private static async Task<DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest>
        ValidRequestAsync(
        string runId = RunId,
        string userRequest = UserRequest,
        DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? noveltyRequest = null)
    {
        var resolvedNovelty = noveltyRequest
            ?? (await ValidNoveltyCarrierAsync(EdgeId)).Request;
        return new DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest(
            runId,
            ScenarioId,
            userRequest,
            ValidTarget(EdgeId),
            ValidAuthorityResult(EdgeId),
            resolvedNovelty);
    }

    private static async Task<
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>
        ValidAssociationAsync(string edgeId)
        => AssertSuccess(
            DynamicGraphMeasurementCoordinator.MeasureEdgeAuthoritySelectionQualityEvidence(
                ValidTarget(edgeId),
                ValidAuthorityResult(edgeId),
                (await ValidNoveltyCarrierAsync(edgeId)).Request));

    private static DynamicGraphTargetRunSpeedMeasurement DeterministicRunSpeed(
        DynamicGraphMeasurementTarget target,
        long inferenceElapsedMs)
        => AssertSuccess(
            DynamicGraphMeasurementCoordinator.MeasureRunSpeed(
                target,
                new DynamicGraphRunSpeedObservation(
                    EvaluatedRowCount: 1,
                    InferenceElapsedMs: inferenceElapsedMs,
                    RoutingElapsedMs: 0,
                    CandidateElapsedMsSamples: Array.Empty<long>(),
                    BatchElapsedMsSamples: Array.Empty<long>())));

    private static void AssertRunSpeed(
        DynamicGraphTargetRunSpeedMeasurement runSpeed,
        string expectedTargetId,
        long inferenceElapsedMs)
    {
        Assert.Equal(expectedTargetId, runSpeed.Target.TargetId, StringComparer.Ordinal);
        Assert.Equal(1, runSpeed.RunSpeed.EvaluatedRowCount);
        Assert.Equal(0, runSpeed.RunSpeed.CandidateSampleCount);
        Assert.Equal(0, runSpeed.RunSpeed.BatchSampleCount);
        Assert.Equal(inferenceElapsedMs, runSpeed.RunSpeed.RunSpeed.InferenceElapsedMs);
        Assert.Equal(
            1000d / inferenceElapsedMs,
            runSpeed.RunSpeed.RunSpeed.ThroughputRowsPerSec);
        Assert.Equal(0L, runSpeed.RunSpeed.RunSpeed.RoutingElapsedMs);
        Assert.Null(runSpeed.RunSpeed.RunSpeed.PerCandidateElapsedMs);
        Assert.Null(runSpeed.RunSpeed.RunSpeed.PerBatchElapsedMs);
    }

    private static void AssertNonLearningFlags(LogicalExecutionFlags flags)
    {
        Assert.True(flags.UseCompiledPredicateRepresentation);
        Assert.True(flags.UseDomainScopedRetrieval);
        Assert.True(flags.UseLowConfidenceLessonSignal);
        Assert.False(flags.UseLearnedUnitWhenAvailable);
        Assert.True(flags.UseTraceEvidence);
        Assert.True(flags.UseContextCarryForward);
        Assert.False(flags.UseIdleLearningScheduling);
        Assert.False(flags.UseE35DependencyDetection);
        Assert.False(flags.UseSkillRetriever);
        Assert.False(flags.UseReplayFusionRanking);
        Assert.True(flags.UseArbiterAuthority);
        Assert.False(flags.UseKnowledgeEdgeDetection);
        Assert.True(flags.UseInternalFirstExecution);
        Assert.True(flags.RequireExternalAiApproval);
        Assert.True(flags.RejectArbiterContradictions);
        Assert.False(flags.UseDeterministicFixtureBaseline);
        Assert.False(flags.WriteEvidenceArtifacts);
        Assert.True(flags.CompareAgainstOracle);
        Assert.True(flags.FailOnMetricDrift);
    }

    private static DynamicGraphEdgeAuthorityNoveltyEvaluationRequest ThrowingNoveltyRequest()
        => new(
            null,
            new ThrowingReadOnlyList<DynamicGraphEdgeAuthorityNoveltyTrainingBinding>(),
            new ThrowingReadOnlyList<string>(),
            new ThrowingReadOnlyList<DynamicGraphEdgeAuthorityNoveltyRowInput>(),
            null,
            null);

    private static DynamicGraphMeasurementTarget ValidTarget(string edgeId)
        => new(
            edgeId,
            DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
            DynamicGraphMeasurementMechanismKind.LearningSubject,
            false);

    private static async Task<MutableNoveltyCarrier> ValidNoveltyCarrierAsync(string edgeId)
    {
        var paired = await PairedAsync(edgeId);
        var trainingBindings = new List<DynamicGraphEdgeAuthorityNoveltyTrainingBinding>
        {
            new("train-a", "train-hash", [A], [A])
        };
        var requestedRowIds = paired.Rows.Select(row => row.RowId).ToList();
        var noveltyRows = Enumerable.Range(0, paired.Rows.Count)
            .Select(index => new DynamicGraphEdgeAuthorityNoveltyRowInput(
                $"row-{index:D3}",
                index % 2 == 0 ? B : C,
                [index % 2 == 0 ? B : C]))
            .ToList();
        return new MutableNoveltyCarrier(
            new DynamicGraphEdgeAuthorityNoveltyEvaluationRequest(
                paired,
                trainingBindings,
                requestedRowIds,
                noveltyRows,
                "fingerprint-normalization-v1",
                "tokenization-v1"),
            trainingBindings,
            requestedRowIds,
            noveltyRows);
    }

    private static async Task<DynamicGraphEdgeAuthorityAblationEvaluationResult> PairedAsync(
        string edgeId)
    {
        var probe = new DeterministicAblationProbe(edgeId);
        var produced = await new DynamicGraphEdgeAuthorityAblationEvaluationRunner(probe)
            .RunAsync(CreateAblationEvaluationRequest(edgeId));

        Assert.True(produced.IsSuccess, $"{produced.ErrorCode}: {produced.ErrorMessage}");
        Assert.Equal(2, probe.CallCount);
        var paired = Assert.IsType<DynamicGraphEdgeAuthorityAblationEvaluationResult>(
            produced.Value);
        Assert.Equal(EvaluationId, paired.EvaluationId, StringComparer.Ordinal);
        Assert.Equal(edgeId, paired.EdgeId, StringComparer.Ordinal);
        Assert.Equal(2, paired.LearnedCorrectCount);
        Assert.Equal(1, paired.BaselineCorrectCount);
        Assert.Equal(0.5d, paired.NormalizedAccuracyDrop);
        Assert.True(paired.MeetsQ11NumericalGate);
        return paired;
    }

    private static DynamicGraphEdgeAuthorityAblationEvaluationRequest
        CreateAblationEvaluationRequest(string edgeId)
    {
        var traversals = Enumerable.Range(0, 2)
            .Select(index => CreateAblationTraversal(index, edgeId))
            .ToArray();
        var rows = Enumerable.Range(0, traversals.Length)
            .Select(index => new DynamicGraphEdgeAuthorityAblationEvaluationRow(
                $"row-{index:D3}",
                "eval-a",
                "eval-hash",
                traversals[index],
                "target-a",
                $"label-{index:D3}"))
            .ToArray();
        return new DynamicGraphEdgeAuthorityAblationEvaluationRequest(
            EvaluationId,
            new AblationEdge(edgeId),
            AblationDomainTag,
            new LogicalExecutionFlags(),
            new DynamicGraphMeasurementSplitProof(
                ["train-a"],
                ["eval-a"],
                ["train-hash", "eval-hash"],
                0,
                "simhash",
                0.85d),
            [
                new DynamicGraphEdgeAuthorityAblationSourceBinding(
                    "eval-a",
                    "eval-hash",
                    "document-000",
                    "repo://eval/a")
            ],
            rows);
    }

    private static EdgeTraversalInput CreateAblationTraversal(int index, string edgeId)
        => new(
            $"ablation-run-{index:D3}",
            edgeId,
            new SubPhaseRouterSourceDocument(
                "document-000",
                "repo://eval/a",
                "internal",
                "AGPL-3.0-only",
                "design",
                $"Evaluation {index:D3}",
                $"evaluation source text {index:D3}",
                ["PREDICATE_A", "PREDICATE_B", "PREDICATE_C"],
                "compiled",
                "none",
                AblationDomainTag),
            new ContextSignals(
                [Domain.CODE_Q],
                [$"completed-{index:D3}"],
                [$"active-{index:D3}"],
                new ReadOnlyDictionary<Domain, double>(
                    new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.75d })),
            new NodeOutputToEdgeInputAdapter(
                "source-node",
                [$"candidate-{index:D3}-a", $"candidate-{index:D3}-b"],
                $"upstream-hash-{index:D3}",
                "GraphNodeCandidate.Id -> UpstreamCandidateRefs (order-free set)"));

    private static DynamicGraphEdgeAuthorityAblationProbeResult CreateAblationProbeResult(
        EdgeTraversalInput traversal,
        string learnedTop,
        string baselineTop,
        string edgeId)
    {
        var candidates = AblationTargets
            .Select(target => new EdgeCandidate(
                target,
                [$"PREDICATE_{target[^1].ToString().ToUpperInvariant()}"],
                StringComparer.Ordinal.Equals(target, baselineTop) ? 0.90d : 0.20d,
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole))
            .ToArray();
        var subjectInput = CreateAblationInput(traversal, candidates, edgeId);
        var baselineInput = CreateAblationInput(traversal, candidates, edgeId);
        var baselineOrder = candidates
            .OrderByDescending(candidate => candidate.BaselineScore)
            .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
            .Select(candidate => candidate.TargetSubPhaseId)
            .ToArray();
        var decision = new EdgeAuthorityDecision(
            [(learnedTop, 0.90d)],
            1,
            false,
            RoutingAuthorityVocabulary.TrainedEdgeUnitRole,
            baselineOrder);
        var orderReport = new DynamicGraphEdgeCandidateOrderLeakageReport(
            edgeId,
            AuthorityUnitId,
            true,
            Enumerable.Range(0, 5)
                .Select(index => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                    $"candidate-order-permutation-{index + 1:D2}",
                    CreatePermutation(candidates, index),
                    CopyDecision(decision)))
                .ToArray(),
            ExactLeakageSignals);
        var identityReport = new DynamicGraphEdgeIdentityLeakageReport(
            edgeId,
            AuthorityUnitId,
            true,
            Enumerable.Range(0, 3)
                .Select(index => new DynamicGraphEdgeIdentityControlProbe(
                    IdentityProbeIds[index],
                    IdentityControlledSignals[index],
                    IdentityControlValues[index],
                    CopyDecision(decision)))
                .ToArray(),
            ExactLeakageSignals);
        var subject = new EdgeAuthorityExecutionResult(
            subjectInput,
            candidates,
            decision,
            new DynamicGraphEdgeAuthorityEvidence(
                edgeId,
                AuthorityUnitId,
                "L2",
                false,
                "baseline-unit",
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole,
                ExactLeakageSignals,
                "source-proof",
                "cutover-proof",
                "ablation-proof"),
            orderReport,
            identityReport,
            0.90d,
            ["subject-evidence"],
            []);
        return new DynamicGraphEdgeAuthorityAblationProbeResult(
            subject,
            baselineInput,
            baselineOrder,
            AuthorityUnitId);
    }

    private static EdgeAuthorityInput CreateAblationInput(
        EdgeTraversalInput traversal,
        IReadOnlyList<EdgeCandidate> candidates,
        string edgeId)
        => new(
            edgeId,
            ["semantic-a", "semantic-b"],
            traversal.Source.CompiledPredicateIds.ToArray(),
            new ContextSignals(
                traversal.Context.PriorDomains.ToArray(),
                traversal.Context.CompletedSubPhases.ToArray(),
                traversal.Context.ActiveKnowledgeEdges.ToArray(),
                new ReadOnlyDictionary<Domain, double>(
                    traversal.Context.PriorConfidenceMap.ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value))),
            Array.AsReadOnly(candidates.Select(CopyCandidate).ToArray()),
            ExactLeakageSignals.ToArray());

    private static EdgeAuthorityExecutionResult ValidAuthorityResult(string edgeId)
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
            edgeId,
            ["edge", "selection", "target"],
            candidates.SelectMany(candidate => candidate.EvidencePredicates).ToArray(),
            new ContextSignals(
                [Domain.CODE_Q],
                ["completed-sub-phase"],
                ["active-edge"],
                new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.8d }),
            candidates,
            ExactLeakageSignals);
        var orderReport = new DynamicGraphEdgeCandidateOrderLeakageReport(
            edgeId,
            AuthorityUnitId,
            true,
            Enumerable.Range(0, 5)
                .Select(index => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                    $"candidate-order-permutation-{index + 1:D2}",
                    CreatePermutation(candidates, index),
                    CopyDecision(decision)))
                .ToArray(),
            ExactLeakageSignals);
        var identityReport = new DynamicGraphEdgeIdentityLeakageReport(
            edgeId,
            AuthorityUnitId,
            true,
            Enumerable.Range(0, 3)
                .Select(index => new DynamicGraphEdgeIdentityControlProbe(
                    IdentityProbeIds[index],
                    IdentityControlledSignals[index],
                    IdentityControlValues[index],
                    CopyDecision(decision)))
                .ToArray(),
            ExactLeakageSignals);

        return new EdgeAuthorityExecutionResult(
            normalizedInput,
            candidates.Select(CopyCandidate).ToArray(),
            CopyDecision(decision),
            new DynamicGraphEdgeAuthorityEvidence(
                edgeId,
                AuthorityUnitId,
                "L2",
                false,
                "selection-target-baseline-unit",
                "candidate_generator",
                ExactLeakageSignals,
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

    private static DynamicGraphLogicalOrchestratorBootstrapProfile CreateProfile()
        => new(
            ProfileId: "executor-profile-34-to-41",
            TenantId: "executor-tenant",
            DomainTag: DevelopmentLifecycleTopologySeeder.DomainId,
            ContextId: "executor-context",
            SignalId: "executor-signal",
            SourceRef: "logical-registry:executor-profile",
            TraceRef: "trace:executor-profile",
            NodeDefinitions:
            [
                CreateNodeDefinition("3.4"),
                CreateNodeDefinition("3.5"),
                CreateNodeDefinition("4.1")
            ],
            EdgeBindings:
            [
                CreateEdgeBinding(
                    "executor-edge-34-to-35",
                    "transition-sub-phase-3.4-to-3.5",
                    "executor-authority-34",
                    "34-35"),
                CreateEdgeBinding(
                    "executor-edge-35-to-41",
                    "transition-sub-phase-3.5-to-4.1",
                    "executor-authority-35",
                    "35-41")
            ],
            Budget: new TopologyBuildBudget(8, 8, 8, 8));

    private static DynamicGraphLogicalOrchestratorNodeDefinition CreateNodeDefinition(string nodeId)
        => new(
            nodeId,
            new DynamicGraphNodeDescriptor(
                NodeId: nodeId,
                DomainTag: DevelopmentLifecycleTopologySeeder.DomainId,
                TInputTypeName: "System.String",
                TOutputTypeName: "System.String",
                IsTrained: false,
                DeterministicFinalAuthority: false,
                RuntimeSemanticQualityClaimed: false,
                AuthorityRole: "baseline_candidate_generator",
                CheckpointRef: null,
                ManifestRef: null));

    private static DynamicGraphRegistryEdgeBinding CreateEdgeBinding(
        string edgeId,
        string transitionId,
        string authorityUnitId,
        string proofSuffix)
        => new(
            EdgeId: edgeId,
            LogicalTransitionIds: [transitionId],
            Kind: GraphEdgeKind.FanOut,
            ParallelEligible: false,
            AuthorityUnitId: authorityUnitId,
            AuthorityUnitLevel: "L2",
            DeterministicFinalAuthority: false,
            BaselineUnitId: $"baseline-{edgeId}",
            BaselineRole: "candidate_generator",
            ExcludedLeakageSignals: ExactLeakageSignals.ToArray(),
            SourceProof: $"source-proof:{proofSuffix}",
            CutoverProof: $"cutover-proof:{proofSuffix}",
            AblationProofRef: $"ablation-proof:{proofSuffix}");

    private static string SecurityScenarioS1()
        => """
            Our backend authentication service has verified controls in production: password minimum length 15 characters, OTP expiry 10 minutes, TOTP window 30 seconds, session idle timeout 20 minutes with risk analysis RA-AUTH-003, re-authentication before username email password or MFA factor changes, JWT validation with RS256 only, none algorithm rejected, exp nbf and aud validated, and RBAC plus per-resource ownership checks on all API endpoints. Does this implementation satisfy OWASP ASVS 5.0.0 for sub-phase 3.4?
            """;

    private static IEnumerable<Type> PublicSignatureTypes(Type type)
        => type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .SelectMany(constructor => constructor.GetParameters().Select(parameter => parameter.ParameterType))
            .Concat(type.GetMethods(
                    BindingFlags.Public
                    | BindingFlags.Instance
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly)
                .SelectMany(method => method.GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .Append(method.ReturnType)))
            .Concat(type.GetProperties(
                    BindingFlags.Public
                    | BindingFlags.Instance
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly)
                .Select(property => property.PropertyType))
            .SelectMany(FlattenType);

    private static IEnumerable<Type> FlattenType(Type type)
    {
        yield return type;
        if (!type.IsGenericType)
            yield break;

        foreach (var argument in type.GetGenericArguments().SelectMany(FlattenType))
            yield return argument;
    }

    private static T AssertSuccess<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return Assert.IsAssignableFrom<T>(result.Value);
    }

    private static void AssertFailure<T>(
        OperationResult<T> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode, StringComparer.Ordinal);
        Assert.Equal(expectedMessage, result.ErrorMessage, StringComparer.Ordinal);
    }

    private sealed record SessionHandles(
        IDynamicGraphLogicalOrchestratorCompositionSession Composition,
        IDynamicGraphLogicalOrchestratorSelectedTargetExecutionSession SelectedTarget,
        IDynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionSession
            AdmittedSelectedTarget);

    private sealed record MutableNoveltyCarrier(
        DynamicGraphEdgeAuthorityNoveltyEvaluationRequest Request,
        List<DynamicGraphEdgeAuthorityNoveltyTrainingBinding> TrainingBindings,
        List<string> RequestedRowIds,
        List<DynamicGraphEdgeAuthorityNoveltyRowInput> NoveltyRows);

    private sealed record Invocation(
        int Ordinal,
        LogicalOrchestratorAssistant Assistant,
        LogicalOrchestratorRequest ExecutionContext,
        DynamicGraphMeasurementTarget? Target,
        EdgeAuthorityExecutionResult? AuthorityResult,
        DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? NoveltyRequest,
        LogicalUnitTrustState MinimumTrustState,
        CancellationToken CancellationToken);

    private sealed record AdmittedInvocation(
        int Ordinal,
        LogicalOrchestratorAssistant Assistant,
        LogicalOrchestratorRequest ExecutionContext,
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement? Association,
        LogicalUnitTrustState MinimumTrustState,
        CancellationToken CancellationToken);

    private sealed class DeterministicAblationProbe
        : IDynamicGraphEdgeAuthorityAblationProbe
    {
        private readonly string _edgeId;
        private int _callCount;

        internal DeterministicAblationProbe(string edgeId)
        {
            _edgeId = edgeId;
        }

        internal int CallCount => Volatile.Read(ref _callCount);

        public Task<OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>> RunAsync(
            IDynamicGraphEdge? edge,
            EdgeTraversalInput? input,
            string? domainTag,
            LogicalExecutionFlags? flags,
            CancellationToken cancellationToken = default)
        {
            var index = Interlocked.Increment(ref _callCount) - 1;
            if (edge is null
                || input is null
                || !StringComparer.Ordinal.Equals(_edgeId, edge.EdgeId)
                || !StringComparer.Ordinal.Equals(AblationDomainTag, domainTag)
                || flags is null
                || index is < 0 or > 1)
            {
                return Task.FromResult(
                    OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>.Fail(
                        "Deterministic ablation fixture invocation is invalid.",
                        "DETERMINISTIC_ABLATION_FIXTURE_INVALID"));
            }

            var baselineTop = index == 0 ? "target-a" : "target-b";
            return Task.FromResult(
                OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>.Ok(
                    CreateAblationProbeResult(input, "target-a", baselineTop, _edgeId)));
        }
    }

    private sealed class AblationEdge : IDynamicGraphEdge
    {
        internal AblationEdge(string edgeId)
        {
            EdgeId = edgeId;
        }

        public string EdgeId { get; }

        public string FromNodeId => "source-node";

        public IReadOnlyList<string> ToNodeIds => Array.AsReadOnly(AblationTargets.ToArray());

        public GraphEdgeKind Kind => GraphEdgeKind.ManyToMany;

        public bool ParallelEligible => true;

        public string AuthorityUnitId =>
            DynamicGraphLogicalOrchestratorSelectedTargetExecutionSessionTests.AuthorityUnitId;

        public string AuthorityUnitLevel => "L2";

        public bool DeterministicFinalAuthority => false;

        public string? BaselineUnitId => "baseline-unit";

        public string BaselineRole => RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole;

        public IReadOnlyList<string> ExcludedLeakageSignals
            => Array.AsReadOnly(ExactLeakageSignals.ToArray());

        public string SourceProof => "source-proof";

        public string CutoverProof => "cutover-proof";

        public string AblationProofRef => "ablation-proof";
    }

    private sealed class RecordingInvoker
        : IDynamicGraphLogicalOrchestratorSelectedTargetExecutionInvoker
    {
        internal const string StopCode = "SELECTED_TARGET_TEST_STOP";
        internal const string StopMessage = "Selected-target test stopped after capture.";

        private readonly object _gate = new();
        private readonly List<Invocation> _calls = [];
        private readonly List<AdmittedInvocation> _admittedCalls = [];
        private int _callCount;
        private int _admittedCallCount;

        internal Func<Invocation, Task<OperationResult<
            DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>>? Handler { get; init; }

        internal Func<AdmittedInvocation, Task<OperationResult<
            DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>>? AdmittedHandler
        {
            get;
            init;
        }

        internal int CallCount => Volatile.Read(ref _callCount);

        internal int AdmittedCallCount => Volatile.Read(ref _admittedCallCount);

        internal IReadOnlyList<Invocation> Calls
        {
            get
            {
                lock (_gate)
                    return _calls.ToArray();
            }
        }

        internal IReadOnlyList<AdmittedInvocation> AdmittedCalls
        {
            get
            {
                lock (_gate)
                    return _admittedCalls.ToArray();
            }
        }

        public Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>
            InvokeAsync(
                LogicalOrchestratorAssistant assistant,
                LogicalOrchestratorRequest executionContext,
                DynamicGraphMeasurementTarget? target,
                EdgeAuthorityExecutionResult? authorityResult,
                DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? noveltyRequest,
                LogicalUnitTrustState minimumTrustState,
                CancellationToken cancellationToken)
        {
            var ordinal = Interlocked.Increment(ref _callCount);
            var call = new Invocation(
                ordinal,
                assistant,
                executionContext,
                target,
                authorityResult,
                noveltyRequest,
                minimumTrustState,
                cancellationToken);
            lock (_gate)
                _calls.Add(call);
            return Handler is null ? Stop() : Handler(call);
        }

        public Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>
            InvokeAdmittedAsync(
                LogicalOrchestratorAssistant assistant,
                LogicalOrchestratorRequest executionContext,
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement? association,
                LogicalUnitTrustState minimumTrustState,
                CancellationToken cancellationToken)
        {
            var ordinal = Interlocked.Increment(ref _admittedCallCount);
            var call = new AdmittedInvocation(
                ordinal,
                assistant,
                executionContext,
                association,
                minimumTrustState,
                cancellationToken);
            lock (_gate)
                _admittedCalls.Add(call);
            return AdmittedHandler is null ? Stop() : AdmittedHandler(call);
        }

        internal static Task<OperationResult<
            DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>> Stop()
            => Task.FromResult(
                OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Fail(
                    StopMessage,
                    StopCode));
    }

    private sealed class ThrowingReadOnlyList<T> : IReadOnlyList<T>
    {
        public int Count => throw new InvalidOperationException("Evidence list was traversed.");

        public T this[int index]
            => throw new InvalidOperationException("Evidence list was traversed.");

        public IEnumerator<T> GetEnumerator()
            => throw new InvalidOperationException("Evidence list was traversed.");

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class CountingSessionStorage : ISessionStorage
    {
        public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
            => Task.FromResult(OperationResult<bool>.Ok(true));

        public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
            => Task.FromResult(
                OperationResult<AgentState>.Fail("Not persisted.", "COUNTING_STORAGE_EMPTY"));

        public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
            => Task.FromResult(
                OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>()));
    }
}
