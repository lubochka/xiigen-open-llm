// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ExperimentsV2.LogicalFlow;
using OrchestratorModel.Logical;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

public sealed class DynamicGraphFactorySelectedTargetExecutionCompositionTests
{
    private const string EdgeId = "factory-selected-target";
    private const string RunId = "factory-selected-run";
    private const string ScenarioId = "factory-selected-scenario";
    private const string Input = "Execute the admitted selected target.";
    private const string AuthorityUnitId = "factory-selected-authority";
    private const string EvaluationId = "factory-selected-evaluation";
    private const string A = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string B = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string C = "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";

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

    private const string RequestInvalidCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_REQUEST_INVALID";
    private const string RequestInvalidMessage =
        "Dynamic graph L3 selected-target flow request is invalid.";

    [Fact]
    public void PublicSurface_IsExactSealedOneFactoryOneRunnerAndPrivateProbeConstructor()
    {
        var type = typeof(DynamicGraphFactorySelectedTargetExecutionComposition);

        Assert.True(type.IsSealed);
        Assert.Empty(type.GetConstructors(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        var constructor = Assert.Single(type.GetConstructors(
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.True(constructor.IsPrivate);
        Assert.Equal(
            [typeof(DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        var field = Assert.Single(type.GetFields(
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Equal("_probe", field.Name, StringComparer.Ordinal);
        Assert.Equal(
            typeof(DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe),
            field.FieldType);
        Assert.True(field.IsPrivate);
        Assert.False(field.IsStatic);
        Assert.True(field.IsInitOnly);

        var methods = type.GetMethods(
                BindingFlags.Public
                | BindingFlags.Static
                | BindingFlags.Instance
                | BindingFlags.DeclaredOnly)
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            ["Create", "RunAdmittedAsync", "RunAsync"],
            methods.Select(method => method.Name));

        var create = methods[0];
        Assert.True(create.IsStatic);
        Assert.Equal(
            typeof(OperationResult<DynamicGraphFactorySelectedTargetExecutionComposition>),
            create.ReturnType);
        var createParameters = create.GetParameters();
        Assert.Equal(
            [
                typeof(InMemoryLogicalFlowRegistry),
                typeof(InMemoryLogicalUnitRegistry),
                typeof(ILogicalTopologyRegistry),
                typeof(LogicalOrchestratorRequest),
                typeof(LogicalUnitTrustState)
            ],
            createParameters.Select(parameter => parameter.ParameterType));
        Assert.All(createParameters.Take(4), parameter => Assert.False(parameter.HasDefaultValue));
        Assert.True(createParameters[4].HasDefaultValue);
        Assert.Equal(LogicalUnitTrustState.Trusted, createParameters[4].DefaultValue);

        var admittedRun = methods[1];
        Assert.False(admittedRun.IsStatic);
        Assert.Equal(
            typeof(Task<OperationResult<
                DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>),
            admittedRun.ReturnType);
        var admittedRunParameters = admittedRun.GetParameters();
        Assert.Equal(
            [
                typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement),
                typeof(CancellationToken)
            ],
            admittedRunParameters.Select(parameter => parameter.ParameterType));
        Assert.False(admittedRunParameters[0].HasDefaultValue);
        Assert.True(admittedRunParameters[1].HasDefaultValue);
        Assert.Null(admittedRunParameters[1].DefaultValue);

        var run = methods[2];
        Assert.False(run.IsStatic);
        Assert.Equal(
            typeof(Task<OperationResult<
                DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>),
            run.ReturnType);
        var runParameters = run.GetParameters();
        Assert.Equal(
            [
                typeof(DynamicGraphMeasurementTarget),
                typeof(EdgeAuthorityExecutionResult),
                typeof(DynamicGraphEdgeAuthorityNoveltyEvaluationRequest),
                typeof(CancellationToken)
            ],
            runParameters.Select(parameter => parameter.ParameterType));
        Assert.All(runParameters.Take(3), parameter => Assert.False(parameter.HasDefaultValue));
        Assert.True(runParameters[3].HasDefaultValue);
        Assert.Null(runParameters[3].DefaultValue);

        Assert.Empty(type.GetProperties(
            BindingFlags.Public
            | BindingFlags.Static
            | BindingFlags.Instance
            | BindingFlags.DeclaredOnly));
        Assert.Empty(type.GetFields(
            BindingFlags.Public
            | BindingFlags.Static
            | BindingFlags.Instance
            | BindingFlags.DeclaredOnly));
        Assert.Empty(type.GetEvents(
            BindingFlags.Public
            | BindingFlags.Static
            | BindingFlags.Instance
            | BindingFlags.DeclaredOnly));
    }

    [Theory]
    [InlineData(
        0,
        "DYNAMIC_GRAPH_FACTORY_SELECTED_TARGET_FLOW_REGISTRY_REQUIRED",
        "Dynamic graph factory selected-target flow registry is required.")]
    [InlineData(
        1,
        "DYNAMIC_GRAPH_FACTORY_SELECTED_TARGET_UNIT_REGISTRY_REQUIRED",
        "Dynamic graph factory selected-target unit registry is required.")]
    [InlineData(
        2,
        "DYNAMIC_GRAPH_FACTORY_SELECTED_TARGET_TOPOLOGY_REGISTRY_REQUIRED",
        "Dynamic graph factory selected-target topology registry is required.")]
    [InlineData(
        3,
        "DYNAMIC_GRAPH_FACTORY_SELECTED_TARGET_EXECUTION_CONTEXT_REQUIRED",
        "Dynamic graph factory selected-target execution context is required.")]
    public void Create_ValidatesDependenciesInExactOrder(
        int firstMissingDependency,
        string expectedCode,
        string expectedMessage)
    {
        var flowRegistry = firstMissingDependency == 0
            ? null
            : new InMemoryLogicalFlowRegistry();
        var unitRegistry = firstMissingDependency <= 1
            ? null
            : new InMemoryLogicalUnitRegistry();
        var topologyRegistry = firstMissingDependency <= 2
            ? null
            : new InMemoryLogicalTopologyRegistry();
        var executionContext = firstMissingDependency <= 3
            ? null
            : ValidContext();

        var result = DynamicGraphFactorySelectedTargetExecutionComposition.Create(
            flowRegistry,
            unitRegistry,
            topologyRegistry,
            executionContext);

        AssertFailure(result, expectedCode, expectedMessage);
    }

    [Theory]
    [MemberData(nameof(MalformedContexts))]
    public void Create_PropagatesExactFactoryAwareRequestValidation(
        LogicalOrchestratorRequest context,
        LogicalUnitTrustState minimumTrustState)
    {
        var result = DynamicGraphFactorySelectedTargetExecutionComposition.Create(
            new InMemoryLogicalFlowRegistry(),
            new InMemoryLogicalUnitRegistry(),
            new InMemoryLogicalTopologyRegistry(),
            context,
            minimumTrustState);

        AssertFailure(result, RequestInvalidCode, RequestInvalidMessage);
    }

    [Fact]
    public async Task RunAsync_PreservesFactoryAwareOwnershipAndExecutesExactTargetOnce()
    {
        var callerTags = new[] { "pre-mutation-tag" };
        var callerNestedValues = new[] { "pre-mutation-nested" };
        var callerNested = new Dictionary<string, object>
        {
            ["values"] = callerNestedValues
        };
        var callerMetadata = new Dictionary<string, object>
        {
            ["mode"] = "factory-selected",
            ["tags"] = callerTags,
            ["nested"] = callerNested
        };
        var callerFlags = new LogicalExecutionFlags
        {
            UseLearnedUnitWhenAvailable = false,
            UseTraceEvidence = true,
            WriteEvidenceArtifacts = false
        };
        var context = new LogicalOrchestratorRequest(
            RunId,
            ScenarioId,
            Input,
            callerFlags,
            callerMetadata);
        var flow = RecordingFlow.Returning(
            ValidFlowResult(EdgeId, ScenarioId, ["spy-evidence-a", "spy-evidence-b"]));
        var factory = RecordingFactory.Returning(flow);
        var flowRegistry = RegistryWithFactory(
            EdgeId,
            ScenarioId,
            factory);
        var unitRegistry = new InMemoryLogicalUnitRegistry();
        var topologyRegistry = new InMemoryLogicalTopologyRegistry();

        var created = DynamicGraphFactorySelectedTargetExecutionComposition.Create(
            flowRegistry,
            unitRegistry,
            topologyRegistry,
            context,
            LogicalUnitTrustState.Probationary);
        Assert.True(created.IsSuccess, $"{created.ErrorCode}: {created.ErrorMessage}");

        callerTags[0] = "post-mutation-tag";
        callerNestedValues[0] = "post-mutation-nested";

        var result = await created.Value!.RunAsync(
            ValidTarget(EdgeId),
            ValidAuthorityResult(EdgeId),
            ValidNoveltyRequest(EdgeId));

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, factory.CallCount);
        Assert.Equal(1, flow.CallCount);
        Assert.Equal(1, result.Value!.DirectTargetInvocationCount);
        Assert.Equal(EdgeId, result.Value.ExecutionResult.TargetId, StringComparer.Ordinal);
        Assert.Equal(RunId, result.Value.ExecutionResult.ExecutionId, StringComparer.Ordinal);
        Assert.Equal(["spy-evidence-a", "spy-evidence-b"], result.Value.ExecutionResult.EvidenceIds);

        var factoryRequest = Assert.IsType<LogicalFlowFactoryRequest>(factory.SeenRequest);
        var flowRequest = Assert.IsType<LogicalFlowRequest<string>>(flow.SeenRequest);
        Assert.Same(unitRegistry, factoryRequest.Units);
        Assert.Same(topologyRegistry, factoryRequest.Topology);
        Assert.Equal(LogicalUnitTrustState.Probationary, factoryRequest.MinimumTrustState);
        Assert.Equal(RunId, factoryRequest.RunId, StringComparer.Ordinal);
        Assert.Equal(RunId, flowRequest.RunId, StringComparer.Ordinal);
        Assert.Equal(ScenarioId, flowRequest.ScenarioId, StringComparer.Ordinal);
        Assert.Equal(Input, flowRequest.Input, StringComparer.Ordinal);
        Assert.Equal(EdgeId, flowRequest.FlowId, StringComparer.Ordinal);

        Assert.Equal(callerFlags, factoryRequest.Flags);
        Assert.Equal(callerFlags, flowRequest.Flags);
        Assert.NotSame(callerFlags, factoryRequest.Flags);
        Assert.NotSame(callerFlags, flowRequest.Flags);
        Assert.NotSame(factoryRequest.Flags, flowRequest.Flags);
        Assert.NotSame(callerMetadata, factoryRequest.Metadata);
        Assert.NotSame(callerMetadata, flowRequest.Metadata);
        Assert.NotSame(factoryRequest.Metadata, flowRequest.Metadata);
        Assert.Equal("factory-selected", factoryRequest.Metadata["mode"]);
        Assert.Equal("factory-selected", flowRequest.Metadata["mode"]);
        Assert.Equal(
            ["pre-mutation-tag"],
            Assert.IsType<string[]>(factoryRequest.Metadata["tags"]));
        Assert.Equal(
            ["pre-mutation-tag"],
            Assert.IsType<string[]>(flowRequest.Metadata["tags"]));
        var factoryNested = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(
            factoryRequest.Metadata["nested"]);
        var flowNested = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(
            flowRequest.Metadata["nested"]);
        Assert.NotSame(callerNested, factoryNested);
        Assert.NotSame(callerNested, flowNested);
        Assert.NotSame(factoryNested, flowNested);
        Assert.Equal(
            ["pre-mutation-nested"],
            Assert.IsType<string[]>(factoryNested["values"]));
        Assert.Equal(
            ["pre-mutation-nested"],
            Assert.IsType<string[]>(flowNested["values"]));
        Assert.NotSame(factoryNested["values"], flowNested["values"]);
    }

    [Fact]
    public async Task RunAsync_RealSecurityFactoryExecutesS1EndToEnd()
    {
        const string securityRunId = "factory-selected-security-run";
        var flowRegistry = new InMemoryLogicalFlowRegistry();
        var registered = flowRegistry.RegisterFactory<string>(
            SecurityVerification34FlowAdapter.FlowId,
            SecurityVerification34FlowAdapter.ScenarioId,
            "1.0.0",
            "3",
            "3.4",
            "security-verification",
            ["security-and-authorization-model"],
            new SecurityVerification34FlowFactory());
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        var unitRegistry = new InMemoryLogicalUnitRegistry();
        var topologyRegistry = new InMemoryLogicalTopologyRegistry();
        var context = new LogicalOrchestratorRequest(
            securityRunId,
            SecurityVerification34FlowAdapter.ScenarioId,
            SecurityScenarioS1(),
            new LogicalExecutionFlags(),
            new Dictionary<string, object>
            {
                ["mode"] = "factory-selected-security",
                ["source"] = "stable-s1"
            });

        var created = DynamicGraphFactorySelectedTargetExecutionComposition.Create(
            flowRegistry,
            unitRegistry,
            topologyRegistry,
            context,
            LogicalUnitTrustState.Trusted);
        Assert.True(created.IsSuccess, $"{created.ErrorCode}: {created.ErrorMessage}");

        var result = await created.Value!.RunAsync(
            ValidTarget(SecurityVerification34FlowAdapter.FlowId),
            ValidAuthorityResult(SecurityVerification34FlowAdapter.FlowId),
            ValidNoveltyRequest(SecurityVerification34FlowAdapter.FlowId));

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, result.Value!.DirectTargetInvocationCount);
        Assert.Equal(
            SecurityVerification34FlowAdapter.FlowId,
            result.Value.ExecutionResult.TargetId,
            StringComparer.Ordinal);
        Assert.Equal(
            securityRunId,
            result.Value.ExecutionResult.ExecutionId,
            StringComparer.Ordinal);
        Assert.Equal(
            ["CB4-STAGE-5-S1", "CB4-3.4-001"],
            result.Value.ExecutionResult.EvidenceIds);
    }

    [Fact]
    public async Task RunAsync_NewestExactDirectRegistrationShadowsOlderFactoryWithoutFallback()
    {
        var olderFactoryFlow = RecordingFlow.Returning(
            ValidFlowResult(EdgeId, ScenarioId, ["older-factory-evidence"]));
        var olderFactory = RecordingFactory.Returning(olderFactoryFlow);
        var directFlow = RecordingFlow.Returning(
            ValidFlowResult(EdgeId, ScenarioId, ["direct-flow-evidence"]));
        var flowRegistry = RegistryWithFactory(EdgeId, ScenarioId, olderFactory);
        var directRegistration = flowRegistry.Register<string>(
            EdgeId,
            ScenarioId,
            "2.0.0",
            directFlow);
        Assert.True(directRegistration.IsSuccess, directRegistration.ErrorMessage);
        var created = DynamicGraphFactorySelectedTargetExecutionComposition.Create(
            flowRegistry,
            new InMemoryLogicalUnitRegistry(),
            new InMemoryLogicalTopologyRegistry(),
            ValidContext());
        Assert.True(created.IsSuccess, $"{created.ErrorCode}: {created.ErrorMessage}");

        var result = await created.Value!.RunAsync(
            ValidTarget(EdgeId),
            ValidAuthorityResult(EdgeId),
            ValidNoveltyRequest(EdgeId));

        AssertFailure(
            result,
            "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_RESOLUTION_FAILED",
            "Dynamic graph L3 selected-target flow resolution failed.");
        Assert.Equal(0, olderFactory.CallCount);
        Assert.Equal(0, olderFactoryFlow.CallCount);
        Assert.Equal(0, directFlow.CallCount);
    }

    [Fact]
    public async Task RunAsync_PreCancellationConsumesCompositionOneShotBeforeFactory()
    {
        var flow = RecordingFlow.Returning(
            ValidFlowResult(EdgeId, ScenarioId, ["cancel-evidence"]));
        var factory = RecordingFactory.Returning(flow);
        var created = DynamicGraphFactorySelectedTargetExecutionComposition.Create(
            RegistryWithFactory(EdgeId, ScenarioId, factory),
            new InMemoryLogicalUnitRegistry(),
            new InMemoryLogicalTopologyRegistry(),
            ValidContext());
        Assert.True(created.IsSuccess, $"{created.ErrorCode}: {created.ErrorMessage}");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var first = await created.Value!.RunAsync(
            ValidTarget(EdgeId),
            ValidAuthorityResult(EdgeId),
            ValidNoveltyRequest(EdgeId),
            cancellation.Token);
        var second = await created.Value.RunAsync(
            ValidTarget(EdgeId),
            ValidAuthorityResult(EdgeId),
            ValidNoveltyRequest(EdgeId));

        AssertFailure(first, "CANCELLED", "Cancelled.");
        AssertFailure(
            second,
            "DYNAMIC_GRAPH_L3_SELECTED_TARGET_EXECUTION_ALREADY_STARTED",
            "Dynamic graph L3 selected-target execution has already started.");
        Assert.Equal(0, factory.CallCount);
        Assert.Equal(0, flow.CallCount);
    }

    [Fact]
    public async Task RunAsync_ConcurrentSecondCallCannotRecreateFactory()
    {
        using var release = new ManualResetEventSlim(false);
        var entered = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var flow = RecordingFlow.Returning(
            ValidFlowResult(EdgeId, ScenarioId, ["concurrent-evidence"]));
        var factory = new RecordingFactory(_ =>
        {
            entered.TrySetResult();
            Assert.True(release.Wait(TimeSpan.FromSeconds(5)));
            return OperationResult<ILogicalFlow<string>>.Ok(flow);
        });
        var created = DynamicGraphFactorySelectedTargetExecutionComposition.Create(
            RegistryWithFactory(EdgeId, ScenarioId, factory),
            new InMemoryLogicalUnitRegistry(),
            new InMemoryLogicalTopologyRegistry(),
            ValidContext());
        Assert.True(created.IsSuccess, $"{created.ErrorCode}: {created.ErrorMessage}");

        var firstTask = Task.Run(() => created.Value!.RunAsync(
            ValidTarget(EdgeId),
            ValidAuthorityResult(EdgeId),
            ValidNoveltyRequest(EdgeId)));
        OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt> second;
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            second = await created.Value!.RunAsync(
                ValidTarget(EdgeId),
                ValidAuthorityResult(EdgeId),
                ValidNoveltyRequest(EdgeId));
        }
        finally
        {
            release.Set();
        }

        var first = await firstTask.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(first.IsSuccess, $"{first.ErrorCode}: {first.ErrorMessage}");
        AssertFailure(
            second,
            "DYNAMIC_GRAPH_L3_SELECTED_TARGET_EXECUTION_ALREADY_STARTED",
            "Dynamic graph L3 selected-target execution has already started.");
        Assert.Equal(1, factory.CallCount);
        Assert.Equal(1, flow.CallCount);
    }

    [Fact]
    public async Task RunAsync_DoesNotMutateHostRegistriesOrTopology()
    {
        var flow = RecordingFlow.Returning(
            ValidFlowResult(EdgeId, ScenarioId, ["nonmutation-evidence"]));
        var factory = RecordingFactory.Returning(flow);
        var flowRegistry = RegistryWithFactory(EdgeId, ScenarioId, factory);
        var unitRegistry = new InMemoryLogicalUnitRegistry();
        var unitRegistration = unitRegistry.Register<string, string>(
            "host-sentinel-unit",
            "host-sentinel-role",
            "host-sentinel-domain",
            "1.0.0",
            LogicalUnitTrustState.Trusted,
            false,
            new SentinelUnit());
        Assert.True(unitRegistration.IsSuccess, unitRegistration.ErrorMessage);
        var topologyRegistry = PopulatedTopologyRegistry();
        var beforeFlows = SnapshotFlowDescriptors(flowRegistry);
        var beforeUnits = SnapshotUnitDescriptors(unitRegistry);
        var beforeTopology = SnapshotTopology(topologyRegistry);

        var created = DynamicGraphFactorySelectedTargetExecutionComposition.Create(
            flowRegistry,
            unitRegistry,
            topologyRegistry,
            ValidContext());
        Assert.True(created.IsSuccess, $"{created.ErrorCode}: {created.ErrorMessage}");
        var result = await created.Value!.RunAsync(
            ValidTarget(EdgeId),
            ValidAuthorityResult(EdgeId),
            ValidNoveltyRequest(EdgeId));

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, factory.CallCount);
        Assert.Equal(1, flow.CallCount);
        Assert.Equal(beforeFlows, SnapshotFlowDescriptors(flowRegistry));
        Assert.Equal(beforeUnits, SnapshotUnitDescriptors(unitRegistry));
        Assert.Equal(beforeTopology, SnapshotTopology(topologyRegistry));
    }

    [Fact]
    public void LogicalOrchestratorAssistant_FactorySelectedTargetSurfaceIsExact()
    {
        var type = typeof(LogicalOrchestratorAssistant);
        var publicConstructor = Assert.Single(type.GetConstructors(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        var publicParameters = publicConstructor.GetParameters();
        Assert.Equal(
            [
                typeof(ISessionStorage),
                typeof(ILogicalOrchestrator),
                typeof(string),
                typeof(ILogicalFlow<LogicalTopologyCommand>)
            ],
            publicParameters.Select(parameter => parameter.ParameterType));
        Assert.All(publicParameters.Take(2), parameter => Assert.False(parameter.HasDefaultValue));
        Assert.Equal(T34LogicalFlowAdapter.ScenarioId, publicParameters[2].DefaultValue);
        Assert.Null(publicParameters[3].DefaultValue);

        var privateConstructor = Assert.Single(type.GetConstructors(
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.True(privateConstructor.IsPrivate);
        Assert.Equal(
            [
                typeof(ISessionStorage),
                typeof(ILogicalOrchestrator),
                typeof(string),
                typeof(ILogicalFlow<LogicalTopologyCommand>),
                typeof(InMemoryLogicalFlowRegistry),
                typeof(InMemoryLogicalUnitRegistry),
                typeof(ILogicalTopologyRegistry)
            ],
            privateConstructor.GetParameters().Select(parameter => parameter.ParameterType));

        var method = Assert.Single(
            type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            candidate => candidate.Name == "RunFactorySelectedTargetExecutionAsync");
        Assert.Equal(
            typeof(Task<OperationResult<
                DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>),
            method.ReturnType);
        var methodParameters = method.GetParameters();
        Assert.Equal(
            [
                typeof(LogicalOrchestratorRequest),
                typeof(DynamicGraphMeasurementTarget),
                typeof(EdgeAuthorityExecutionResult),
                typeof(DynamicGraphEdgeAuthorityNoveltyEvaluationRequest),
                typeof(LogicalUnitTrustState),
                typeof(CancellationToken)
            ],
            methodParameters.Select(parameter => parameter.ParameterType));
        Assert.All(methodParameters.Take(4), parameter => Assert.False(parameter.HasDefaultValue));
        Assert.Equal(LogicalUnitTrustState.Trusted, methodParameters[4].DefaultValue);
        Assert.Null(methodParameters[5].DefaultValue);

        var admittedMethod = Assert.Single(
            type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            candidate => candidate.Name == "RunFactoryAdmittedSelectedTargetExecutionAsync");
        Assert.Equal(
            typeof(Task<OperationResult<
                DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>),
            admittedMethod.ReturnType);
        var admittedParameters = admittedMethod.GetParameters();
        Assert.Equal(
            [
                typeof(LogicalOrchestratorRequest),
                typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement),
                typeof(LogicalUnitTrustState),
                typeof(CancellationToken)
            ],
            admittedParameters.Select(parameter => parameter.ParameterType));
        Assert.All(admittedParameters.Take(2), parameter => Assert.False(
            parameter.HasDefaultValue));
        Assert.Equal(LogicalUnitTrustState.Trusted, admittedParameters[2].DefaultValue);
        Assert.Null(admittedParameters[3].DefaultValue);

        var registryTypes = new HashSet<Type>
        {
            typeof(InMemoryLogicalFlowRegistry),
            typeof(InMemoryLogicalUnitRegistry),
            typeof(ILogicalTopologyRegistry)
        };
        var retainedFields = type.GetFields(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(field => registryTypes.Contains(field.FieldType))
            .OrderBy(field => field.MetadataToken)
            .ToArray();
        Assert.Equal(3, retainedFields.Length);
        Assert.Equal(
            [
                "_factorySelectedTargetFlowRegistry",
                "_factorySelectedTargetUnitRegistry",
                "_factorySelectedTargetTopologyRegistry"
            ],
            retainedFields.Select(field => field.Name));
        Assert.Equal(
            [
                typeof(InMemoryLogicalFlowRegistry),
                typeof(InMemoryLogicalUnitRegistry),
                typeof(ILogicalTopologyRegistry)
            ],
            retainedFields.Select(field => field.FieldType));
        Assert.All(retainedFields, field =>
        {
            Assert.True(field.IsPrivate);
            Assert.True(field.IsInitOnly);
            Assert.False(field.IsStatic);
        });

        Assert.DoesNotContain(
            type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .SelectMany(constructor => constructor.GetParameters()),
            parameter => registryTypes.Contains(parameter.ParameterType));
        Assert.DoesNotContain(
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static),
            property => registryTypes.Contains(property.PropertyType));
        Assert.DoesNotContain(
            type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static),
            field => registryTypes.Contains(field.FieldType));
        Assert.DoesNotContain(
            type.GetMethods(
                BindingFlags.Public
                | BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.DeclaredOnly),
            publicMethod => ContainsAnyRegistryType(publicMethod.ReturnType, registryTypes)
                || publicMethod.GetParameters().Any(
                    parameter => ContainsAnyRegistryType(parameter.ParameterType, registryTypes)));
    }

    [Fact]
    public async Task RunFactorySelectedTargetExecutionAsync_PublicConstructionFailsAtMissingRegistryBeforeCancellation()
    {
        var legacy = new RecordingOrchestrator();
        var assistant = new LogicalOrchestratorAssistant(
            new RecordingSessionStorage(),
            legacy);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await assistant.RunFactorySelectedTargetExecutionAsync(
            null,
            null,
            null,
            null,
            cancellationToken: cancellation.Token);

        AssertFailure(
            result,
            "DYNAMIC_GRAPH_FACTORY_SELECTED_TARGET_FLOW_REGISTRY_REQUIRED",
            "Dynamic graph factory selected-target flow registry is required.");
        Assert.Equal(0, legacy.CallCount);
    }

    [Fact]
    public async Task RunFactorySelectedTargetExecutionAsync_DefaultRetainsAdaptiveRegistryIdentityAndExecutesOnce()
    {
        const string targetId = "assistant-retained-target";
        const string scenarioId = "assistant-retained-scenario";
        const string runId = "assistant-retained-run";
        var assistant = DefaultAssistant();
        var flowRegistry = ReadPrivateField<InMemoryLogicalFlowRegistry>(
            assistant,
            "_factorySelectedTargetFlowRegistry");
        var unitRegistry = ReadPrivateField<InMemoryLogicalUnitRegistry>(
            assistant,
            "_factorySelectedTargetUnitRegistry");
        var topologyRegistry = ReadPrivateField<ILogicalTopologyRegistry>(
            assistant,
            "_factorySelectedTargetTopologyRegistry");
        var adaptive = Assert.IsType<AdaptiveLogicalOrchestrator>(
            ReadPrivateField<ILogicalOrchestrator>(assistant, "_orchestrator"));
        Assert.Same(flowRegistry, ReadPrivateField<InMemoryLogicalFlowRegistry>(adaptive, "_flows"));
        Assert.Same(unitRegistry, ReadPrivateField<InMemoryLogicalUnitRegistry>(adaptive, "_units"));
        Assert.Same(topologyRegistry, ReadPrivateField<ILogicalTopologyRegistry>(adaptive, "_topology"));

        var flow = RecordingFlow.Returning(
            ValidFlowResult(targetId, scenarioId, ["assistant-retained-evidence"]));
        var factory = RecordingFactory.Returning(flow);
        RegisterFactory(flowRegistry, targetId, scenarioId, factory, "assistant-retained");
        var beforeFlows = SnapshotFlowDescriptors(flowRegistry);
        var beforeUnits = SnapshotUnitDescriptors(unitRegistry);
        var beforeTopology = SnapshotTopology(topologyRegistry);
        var context = AssistantContext(runId, scenarioId, "Execute the retained exact target.");

        var result = await assistant.RunFactorySelectedTargetExecutionAsync(
            context,
            ValidTarget(targetId),
            ValidAuthorityResult(targetId),
            ValidNoveltyRequest(targetId));

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, result.Value!.DirectTargetInvocationCount);
        Assert.Equal(targetId, result.Value.ExecutionResult.TargetId, StringComparer.Ordinal);
        Assert.Equal(runId, result.Value.ExecutionResult.ExecutionId, StringComparer.Ordinal);
        Assert.Equal(1, factory.CallCount);
        Assert.Equal(1, flow.CallCount);
        var request = Assert.IsType<LogicalFlowFactoryRequest>(factory.SeenRequest);
        Assert.Same(unitRegistry, request.Units);
        Assert.Same(topologyRegistry, request.Topology);
        Assert.Equal(beforeFlows, SnapshotFlowDescriptors(flowRegistry));
        Assert.Equal(beforeUnits, SnapshotUnitDescriptors(unitRegistry));
        Assert.Equal(beforeTopology, SnapshotTopology(topologyRegistry));
    }

    [Fact]
    public async Task RunFactorySelectedTargetExecutionAsync_DefaultSecurityS1UsesExistingRegistration()
    {
        const string runId = "assistant-factory-selected-security-run";
        var assistant = DefaultAssistant();
        var flowRegistry = ReadPrivateField<InMemoryLogicalFlowRegistry>(
            assistant,
            "_factorySelectedTargetFlowRegistry");
        var unitRegistry = ReadPrivateField<InMemoryLogicalUnitRegistry>(
            assistant,
            "_factorySelectedTargetUnitRegistry");
        var topologyRegistry = ReadPrivateField<ILogicalTopologyRegistry>(
            assistant,
            "_factorySelectedTargetTopologyRegistry");
        var beforeFlows = SnapshotFlowDescriptors(flowRegistry);
        var beforeUnits = SnapshotUnitDescriptors(unitRegistry);
        var beforeTopology = SnapshotTopology(topologyRegistry);
        var context = AssistantContext(
            runId,
            SecurityVerification34FlowAdapter.ScenarioId,
            SecurityScenarioS1());

        var result = await assistant.RunFactorySelectedTargetExecutionAsync(
            context,
            ValidTarget(SecurityVerification34FlowAdapter.FlowId),
            ValidAuthorityResult(SecurityVerification34FlowAdapter.FlowId),
            ValidNoveltyRequest(SecurityVerification34FlowAdapter.FlowId));

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, result.Value!.DirectTargetInvocationCount);
        Assert.Equal(
            SecurityVerification34FlowAdapter.FlowId,
            result.Value.ExecutionResult.TargetId,
            StringComparer.Ordinal);
        Assert.Equal(runId, result.Value.ExecutionResult.ExecutionId, StringComparer.Ordinal);
        Assert.Equal(
            ["CB4-STAGE-5-S1", "CB4-3.4-001"],
            result.Value.ExecutionResult.EvidenceIds);
        Assert.Equal(beforeFlows, SnapshotFlowDescriptors(flowRegistry));
        Assert.Equal(beforeUnits, SnapshotUnitDescriptors(unitRegistry));
        Assert.Equal(beforeTopology, SnapshotTopology(topologyRegistry));
    }

    [Fact]
    public async Task RunFactorySelectedTargetExecutionAsync_PreservesCompositionPrecedenceBeforeCancellation()
    {
        const string targetId = "assistant-precedence-target";
        const string scenarioId = "assistant-precedence-scenario";
        var legacy = new RecordingOrchestrator();
        var manual = new LogicalOrchestratorAssistant(new RecordingSessionStorage(), legacy);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var missingRegistry = await manual.RunFactorySelectedTargetExecutionAsync(
            null,
            null,
            null,
            null,
            cancellationToken: cancellation.Token);
        AssertFailure(
            missingRegistry,
            "DYNAMIC_GRAPH_FACTORY_SELECTED_TARGET_FLOW_REGISTRY_REQUIRED",
            "Dynamic graph factory selected-target flow registry is required.");
        Assert.Equal(0, legacy.CallCount);

        var assistant = DefaultAssistant();
        var missingContext = await assistant.RunFactorySelectedTargetExecutionAsync(
            null,
            null,
            null,
            null,
            cancellationToken: cancellation.Token);
        AssertFailure(
            missingContext,
            "DYNAMIC_GRAPH_FACTORY_SELECTED_TARGET_EXECUTION_CONTEXT_REQUIRED",
            "Dynamic graph factory selected-target execution context is required.");

        var invalidTrust = await assistant.RunFactorySelectedTargetExecutionAsync(
            AssistantContext("assistant-invalid-trust-run", scenarioId, "Invalid trust request."),
            null,
            null,
            null,
            (LogicalUnitTrustState)int.MaxValue,
            cancellation.Token);
        AssertFailure(invalidTrust, RequestInvalidCode, RequestInvalidMessage);

        var flowRegistry = ReadPrivateField<InMemoryLogicalFlowRegistry>(
            assistant,
            "_factorySelectedTargetFlowRegistry");
        var flow = RecordingFlow.Returning(
            ValidFlowResult(targetId, scenarioId, ["precedence-evidence"]));
        var factory = RecordingFactory.Returning(flow);
        RegisterFactory(flowRegistry, targetId, scenarioId, factory, "assistant-precedence");
        var context = AssistantContext(
            "assistant-cancelled-run",
            scenarioId,
            "Cancelled selected-target request.");
        var cancelled = await assistant.RunFactorySelectedTargetExecutionAsync(
            context,
            null,
            null,
            null,
            cancellationToken: cancellation.Token);
        AssertFailure(cancelled, "CANCELLED", "Cancelled.");
        Assert.Equal(0, factory.CallCount);
        Assert.Equal(0, flow.CallCount);

        var invalidTarget = await assistant.RunFactorySelectedTargetExecutionAsync(
            context with { RunId = "assistant-invalid-target-run" },
            null,
            ValidAuthorityResult(targetId),
            ValidNoveltyRequest(targetId));
        AssertFailure(
            invalidTarget,
            "INVALID_DYNAMIC_GRAPH_MEASUREMENT_TARGET",
            "Dynamic graph measurement target is invalid.");
        Assert.Equal(0, factory.CallCount);
        Assert.Equal(0, flow.CallCount);
    }

    [Fact]
    public async Task RunFactorySelectedTargetExecutionAsync_NewestDirectBlocksFactoryWithoutLegacyFallback()
    {
        const string targetId = "assistant-shadow-target";
        const string scenarioId = "assistant-shadow-scenario";
        var legacy = new RecordingOrchestrator();
        var flowRegistry = new InMemoryLogicalFlowRegistry();
        var unitRegistry = new InMemoryLogicalUnitRegistry();
        var topologyRegistry = new InMemoryLogicalTopologyRegistry();
        var olderFactoryFlow = RecordingFlow.Returning(
            ValidFlowResult(targetId, scenarioId, ["shadow-factory-evidence"]));
        var olderFactory = RecordingFactory.Returning(olderFactoryFlow);
        RegisterFactory(flowRegistry, targetId, scenarioId, olderFactory, "assistant-shadow-v1");
        var directFlow = RecordingFlow.Returning(
            ValidFlowResult(targetId, scenarioId, ["shadow-direct-evidence"]));
        var direct = flowRegistry.Register<string>(targetId, scenarioId, "2.0.0", directFlow);
        Assert.True(direct.IsSuccess, direct.ErrorMessage);
        var assistant = CreateRetainedAssistant(
            legacy,
            flowRegistry,
            unitRegistry,
            topologyRegistry);

        var result = await assistant.RunFactorySelectedTargetExecutionAsync(
            AssistantContext("assistant-shadow-run", scenarioId, "Execute shadow target."),
            ValidTarget(targetId),
            ValidAuthorityResult(targetId),
            ValidNoveltyRequest(targetId));

        AssertFailure(
            result,
            "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_RESOLUTION_FAILED",
            "Dynamic graph L3 selected-target flow resolution failed.");
        Assert.Equal(0, olderFactory.CallCount);
        Assert.Equal(0, olderFactoryFlow.CallCount);
        Assert.Equal(0, directFlow.CallCount);
        Assert.Equal(0, legacy.CallCount);
    }

    [Fact]
    public async Task RunFactorySelectedTargetExecutionAsync_ConcurrentCallsUseIndependentCompositions()
    {
        const string targetId = "assistant-concurrent-target";
        const string scenarioId = "assistant-concurrent-scenario";
        using var release = new ManualResetEventSlim(false);
        var bothEntered = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var barrierEntryCount = 0;
        var assistant = DefaultAssistant();
        var flowRegistry = ReadPrivateField<InMemoryLogicalFlowRegistry>(
            assistant,
            "_factorySelectedTargetFlowRegistry");
        var unitRegistry = ReadPrivateField<InMemoryLogicalUnitRegistry>(
            assistant,
            "_factorySelectedTargetUnitRegistry");
        var topologyRegistry = ReadPrivateField<ILogicalTopologyRegistry>(
            assistant,
            "_factorySelectedTargetTopologyRegistry");
        var flow = RecordingFlow.Returning(
            ValidFlowResult(targetId, scenarioId, ["assistant-concurrent-evidence"]));
        var factory = new RecordingFactory(_ =>
        {
            if (Interlocked.Increment(ref barrierEntryCount) == 2)
                bothEntered.TrySetResult();
            Assert.True(release.Wait(TimeSpan.FromSeconds(5)));
            return OperationResult<ILogicalFlow<string>>.Ok(flow);
        });
        RegisterFactory(flowRegistry, targetId, scenarioId, factory, "assistant-concurrent");
        var beforeFlows = SnapshotFlowDescriptors(flowRegistry);
        var beforeUnits = SnapshotUnitDescriptors(unitRegistry);
        var beforeTopology = SnapshotTopology(topologyRegistry);
        var firstTask = Task.Run(() => assistant.RunFactorySelectedTargetExecutionAsync(
            AssistantContext("assistant-concurrent-run-a", scenarioId, "Concurrent request A."),
            ValidTarget(targetId),
            ValidAuthorityResult(targetId),
            ValidNoveltyRequest(targetId)));
        var secondTask = Task.Run(() => assistant.RunFactorySelectedTargetExecutionAsync(
            AssistantContext("assistant-concurrent-run-b", scenarioId, "Concurrent request B."),
            ValidTarget(targetId),
            ValidAuthorityResult(targetId),
            ValidNoveltyRequest(targetId)));
        Exception? barrierFailure = null;
        try
        {
            await bothEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(firstTask.IsCompleted);
            Assert.False(secondTask.IsCompleted);
        }
        catch (Exception exception)
        {
            barrierFailure = exception;
        }
        finally
        {
            release.Set();
        }

        var results = await Task.WhenAll(firstTask, secondTask).WaitAsync(TimeSpan.FromSeconds(5));
        if (barrierFailure is not null)
            throw barrierFailure;

        Assert.All(results, result =>
            Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}"));
        Assert.Equal(
            ["assistant-concurrent-run-a", "assistant-concurrent-run-b"],
            results
                .Select(result => result.Value!.ExecutionResult.ExecutionId)
                .OrderBy(value => value, StringComparer.Ordinal));
        Assert.Equal(2, factory.CallCount);
        Assert.Equal(2, flow.CallCount);
        Assert.Equal(beforeFlows, SnapshotFlowDescriptors(flowRegistry));
        Assert.Equal(beforeUnits, SnapshotUnitDescriptors(unitRegistry));
        Assert.Equal(beforeTopology, SnapshotTopology(topologyRegistry));
    }

    [Fact]
    public async Task RunAdmittedAsync_ExecutesOwnedCarrierDirectlyWithoutRegistryOrTopologyMutation()
    {
        var flow = RecordingFlow.Returning(
            ValidFlowResult(EdgeId, ScenarioId, ["admitted-direct-evidence"]));
        var factory = RecordingFactory.Returning(flow);
        var flowRegistry = RegistryWithFactory(EdgeId, ScenarioId, factory);
        var unitRegistry = new InMemoryLogicalUnitRegistry();
        var topologyRegistry = PopulatedTopologyRegistry();
        var beforeFlows = SnapshotFlowDescriptors(flowRegistry);
        var beforeUnits = SnapshotUnitDescriptors(unitRegistry);
        var beforeTopology = SnapshotTopology(topologyRegistry);
        var association = ValidAssociation(EdgeId);
        var created = DynamicGraphFactorySelectedTargetExecutionComposition.Create(
            flowRegistry,
            unitRegistry,
            topologyRegistry,
            ValidContext());
        Assert.True(created.IsSuccess, created.ErrorMessage);

        var result = await created.Value!.RunAdmittedAsync(association);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var receipt = result.Value!;
        Assert.Equal(1, receipt.DirectTargetInvocationCount);
        Assert.Equal(EdgeId, receipt.ExecutionResult.TargetId, StringComparer.Ordinal);
        Assert.Equal(RunId, receipt.ExecutionResult.ExecutionId, StringComparer.Ordinal);
        Assert.Equal(["admitted-direct-evidence"], receipt.ExecutionResult.EvidenceIds);
        Assert.NotSame(association, receipt.Association);
        Assert.NotSame(receipt.Association, receipt.Association);
        Assert.NotSame(receipt.ExecutionResult, receipt.ExecutionResult);
        Assert.Equal(1, factory.CallCount);
        Assert.Equal(1, flow.CallCount);
        Assert.Equal(beforeFlows, SnapshotFlowDescriptors(flowRegistry));
        Assert.Equal(beforeUnits, SnapshotUnitDescriptors(unitRegistry));
        Assert.Equal(beforeTopology, SnapshotTopology(topologyRegistry));
    }

    [Fact]
    public async Task RawAndAdmittedCallsShareOneShotProbeInBothDirections()
    {
        var flow = RecordingFlow.Returning(
            ValidFlowResult(EdgeId, ScenarioId, ["mixed-one-shot-evidence"]));
        var factory = RecordingFactory.Returning(flow);
        var registry = RegistryWithFactory(EdgeId, ScenarioId, factory);
        var units = new InMemoryLogicalUnitRegistry();
        var topology = new InMemoryLogicalTopologyRegistry();
        var rawFirst = DynamicGraphFactorySelectedTargetExecutionComposition.Create(
            registry,
            units,
            topology,
            ValidContext());
        Assert.True(rawFirst.IsSuccess, rawFirst.ErrorMessage);

        var raw = await rawFirst.Value!.RunAsync(
            ValidTarget(EdgeId),
            ValidAuthorityResult(EdgeId),
            ValidNoveltyRequest(EdgeId));
        var admittedSecond = await rawFirst.Value.RunAdmittedAsync(ValidAssociation(EdgeId));

        Assert.True(raw.IsSuccess, $"{raw.ErrorCode}: {raw.ErrorMessage}");
        AssertFailure(
            admittedSecond,
            "DYNAMIC_GRAPH_L3_SELECTED_TARGET_EXECUTION_ALREADY_STARTED",
            "Dynamic graph L3 selected-target execution has already started.");

        var admittedFirst = DynamicGraphFactorySelectedTargetExecutionComposition.Create(
            registry,
            units,
            topology,
            ValidContext() with { RunId = "admitted-first-run" });
        Assert.True(admittedFirst.IsSuccess, admittedFirst.ErrorMessage);
        var admitted = await admittedFirst.Value!.RunAdmittedAsync(ValidAssociation(EdgeId));
        var rawSecond = await admittedFirst.Value.RunAsync(
            ValidTarget(EdgeId),
            ValidAuthorityResult(EdgeId),
            ValidNoveltyRequest(EdgeId));

        Assert.True(admitted.IsSuccess, $"{admitted.ErrorCode}: {admitted.ErrorMessage}");
        AssertFailure(
            rawSecond,
            "DYNAMIC_GRAPH_L3_SELECTED_TARGET_EXECUTION_ALREADY_STARTED",
            "Dynamic graph L3 selected-target execution has already started.");
        Assert.Equal(2, factory.CallCount);
        Assert.Equal(2, flow.CallCount);
    }

    [Fact]
    public async Task RunAdmittedAsync_PreCancellationConsumesCompositionBeforeFactory()
    {
        var flow = RecordingFlow.Returning(
            ValidFlowResult(EdgeId, ScenarioId, ["cancelled-admitted-evidence"]));
        var factory = RecordingFactory.Returning(flow);
        var created = DynamicGraphFactorySelectedTargetExecutionComposition.Create(
            RegistryWithFactory(EdgeId, ScenarioId, factory),
            new InMemoryLogicalUnitRegistry(),
            new InMemoryLogicalTopologyRegistry(),
            ValidContext());
        Assert.True(created.IsSuccess, created.ErrorMessage);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var first = await created.Value!.RunAdmittedAsync(
            ValidAssociation(EdgeId),
            cancellation.Token);
        var second = await created.Value.RunAdmittedAsync(ValidAssociation(EdgeId));

        AssertFailure(first, "CANCELLED", "Cancelled.");
        AssertFailure(
            second,
            "DYNAMIC_GRAPH_L3_SELECTED_TARGET_EXECUTION_ALREADY_STARTED",
            "Dynamic graph L3 selected-target execution has already started.");
        Assert.Equal(0, factory.CallCount);
        Assert.Equal(0, flow.CallCount);
    }

    [Fact]
    public async Task RunFactoryAdmittedSelectedTargetExecutionAsync_DefaultSecurityS1RunsEndToEnd()
    {
        const string runId = "assistant-admitted-security-run";
        var assistant = DefaultAssistant();
        var flowRegistry = ReadPrivateField<InMemoryLogicalFlowRegistry>(
            assistant,
            "_factorySelectedTargetFlowRegistry");
        var unitRegistry = ReadPrivateField<InMemoryLogicalUnitRegistry>(
            assistant,
            "_factorySelectedTargetUnitRegistry");
        var topologyRegistry = ReadPrivateField<ILogicalTopologyRegistry>(
            assistant,
            "_factorySelectedTargetTopologyRegistry");
        var beforeFlows = SnapshotFlowDescriptors(flowRegistry);
        var beforeUnits = SnapshotUnitDescriptors(unitRegistry);
        var beforeTopology = SnapshotTopology(topologyRegistry);
        var context = AssistantContext(
            runId,
            SecurityVerification34FlowAdapter.ScenarioId,
            SecurityScenarioS1());

        var result = await assistant.RunFactoryAdmittedSelectedTargetExecutionAsync(
            context,
            ValidAssociation(SecurityVerification34FlowAdapter.FlowId));

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, result.Value!.DirectTargetInvocationCount);
        Assert.Equal(
            SecurityVerification34FlowAdapter.FlowId,
            result.Value.ExecutionResult.TargetId,
            StringComparer.Ordinal);
        Assert.Equal(runId, result.Value.ExecutionResult.ExecutionId, StringComparer.Ordinal);
        Assert.Equal(
            ["CB4-STAGE-5-S1", "CB4-3.4-001"],
            result.Value.ExecutionResult.EvidenceIds);
        Assert.Equal(beforeFlows, SnapshotFlowDescriptors(flowRegistry));
        Assert.Equal(beforeUnits, SnapshotUnitDescriptors(unitRegistry));
        Assert.Equal(beforeTopology, SnapshotTopology(topologyRegistry));
    }

    [Fact]
    public async Task RunFactoryAdmittedSelectedTargetExecutionAsync_PreservesFactoryAndContextPrecedence()
    {
        var legacy = new RecordingOrchestrator();
        var manual = new LogicalOrchestratorAssistant(new RecordingSessionStorage(), legacy);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var missingRegistry = await manual.RunFactoryAdmittedSelectedTargetExecutionAsync(
            null,
            null,
            cancellationToken: cancellation.Token);
        AssertFailure(
            missingRegistry,
            "DYNAMIC_GRAPH_FACTORY_SELECTED_TARGET_FLOW_REGISTRY_REQUIRED",
            "Dynamic graph factory selected-target flow registry is required.");
        Assert.Equal(0, legacy.CallCount);

        var assistant = DefaultAssistant();
        var missingContext = await assistant.RunFactoryAdmittedSelectedTargetExecutionAsync(
            null,
            null,
            cancellationToken: cancellation.Token);
        AssertFailure(
            missingContext,
            "DYNAMIC_GRAPH_FACTORY_SELECTED_TARGET_EXECUTION_CONTEXT_REQUIRED",
            "Dynamic graph factory selected-target execution context is required.");

        var malformed = await assistant.RunFactoryAdmittedSelectedTargetExecutionAsync(
            ValidContext() with { RunId = " malformed-admitted-run" },
            null,
            cancellationToken: cancellation.Token);
        AssertFailure(malformed, RequestInvalidCode, RequestInvalidMessage);

        var cancelled = await assistant.RunFactoryAdmittedSelectedTargetExecutionAsync(
            ValidContext(),
            null,
            cancellationToken: cancellation.Token);
        AssertFailure(cancelled, "CANCELLED", "Cancelled.");
    }

    public static IEnumerable<object[]> MalformedContexts()
    {
        yield return [ValidContext() with { RunId = " malformed-run" }, LogicalUnitTrustState.Trusted];
        yield return [ValidContext() with { ScenarioId = "malformed-scenario " }, LogicalUnitTrustState.Trusted];
        yield return [ValidContext() with { UserRequest = " " }, LogicalUnitTrustState.Trusted];
        yield return [ValidContext() with { Flags = null }, LogicalUnitTrustState.Trusted];
        yield return [ValidContext() with { Metadata = null! }, LogicalUnitTrustState.Trusted];
        yield return
        [
            ValidContext() with
            {
                Metadata = new Dictionary<string, object> { ["unsupported"] = new object() }
            },
            LogicalUnitTrustState.Trusted
        ];

        var cyclic = new Dictionary<string, object>();
        cyclic["self"] = cyclic;
        yield return [ValidContext() with { Metadata = cyclic }, LogicalUnitTrustState.Trusted];
        yield return [ValidContext(), (LogicalUnitTrustState)int.MaxValue];
    }

    private static LogicalOrchestratorRequest ValidContext()
        => new(
            RunId,
            ScenarioId,
            Input,
            new LogicalExecutionFlags(),
            new Dictionary<string, object> { ["mode"] = "factory-selected" });

    private static bool ContainsAnyRegistryType(
        Type candidate,
        IReadOnlySet<Type> registryTypes)
    {
        return ContainsAnyRegistryType(candidate, registryTypes, []);
    }

    private static bool ContainsAnyRegistryType(
        Type candidate,
        IReadOnlySet<Type> registryTypes,
        HashSet<Type> visited)
    {
        if (registryTypes.Contains(candidate))
        {
            return true;
        }

        if (!visited.Add(candidate))
        {
            return false;
        }

        if (candidate.HasElementType
            && candidate.GetElementType() is { } elementType
            && ContainsAnyRegistryType(elementType, registryTypes, visited))
        {
            return true;
        }

        if (candidate.IsFunctionPointer
            && (ContainsAnyRegistryType(
                    candidate.GetFunctionPointerReturnType(),
                    registryTypes,
                    visited)
                || candidate.GetFunctionPointerParameterTypes().Any(
                    parameterType => ContainsAnyRegistryType(
                        parameterType,
                        registryTypes,
                        visited))))
        {
            return true;
        }

        if (candidate.IsGenericType
            && candidate.GetGenericArguments().Any(
                genericArgument => ContainsAnyRegistryType(
                    genericArgument,
                    registryTypes,
                    visited)))
        {
            return true;
        }

        return candidate.IsGenericParameter
            && candidate.GetGenericParameterConstraints().Any(
                constraint => ContainsAnyRegistryType(constraint, registryTypes, visited));
    }

    private static LogicalOrchestratorRequest AssistantContext(
        string runId,
        string scenarioId,
        string input)
        => new(
            runId,
            scenarioId,
            input,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>
            {
                ["mode"] = "assistant-factory-selected",
                ["source"] = "slice-20am"
            });

    private static LogicalOrchestratorAssistant DefaultAssistant()
    {
        var result = LogicalOrchestratorAssistant.CreateDefault(new RecordingSessionStorage());
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return result.Value!;
    }

    private static LogicalOrchestratorAssistant CreateRetainedAssistant(
        ILogicalOrchestrator orchestrator,
        InMemoryLogicalFlowRegistry flowRegistry,
        InMemoryLogicalUnitRegistry unitRegistry,
        ILogicalTopologyRegistry topologyRegistry)
    {
        var constructor = Assert.Single(
            typeof(LogicalOrchestratorAssistant).GetConstructors(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        return Assert.IsType<LogicalOrchestratorAssistant>(constructor.Invoke(
        [
            new RecordingSessionStorage(),
            orchestrator,
            T34LogicalFlowAdapter.ScenarioId,
            null,
            flowRegistry,
            unitRegistry,
            topologyRegistry
        ]));
    }

    private static T ReadPrivateField<T>(object source, string fieldName)
        where T : class
    {
        var field = Assert.IsAssignableFrom<FieldInfo>(source.GetType().GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        return Assert.IsAssignableFrom<T>(field.GetValue(source));
    }

    private static void RegisterFactory(
        InMemoryLogicalFlowRegistry registry,
        string flowId,
        string scenarioId,
        ILogicalFlowFactory<string> factory,
        string version)
    {
        var registered = registry.RegisterFactory<string>(
            flowId,
            scenarioId,
            version,
            "assistant-phase",
            "assistant-subphase",
            "assistant-factory-selected",
            ["assistant-factory-selected"],
            factory);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
    }

    private static InMemoryLogicalFlowRegistry RegistryWithFactory(
        string flowId,
        string scenarioId,
        ILogicalFlowFactory<string> factory)
    {
        var registry = new InMemoryLogicalFlowRegistry();
        var registered = registry.RegisterFactory<string>(
            flowId,
            scenarioId,
            "1.0.0",
            "phase",
            "subphase",
            "task",
            [],
            factory);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        return registry;
    }

    private static LogicalFlowResult ValidFlowResult(
        string flowId,
        string scenarioId,
        string[] evidenceIds)
        => new(
            flowId,
            scenarioId,
            "REJECT",
            "Direct selected-target flow completed.",
            new LogicalFlowSummary(
                "selected-target-summary",
                1,
                0.25d,
                0.2d,
                "direct_quality",
                false,
                false,
                false,
                "observe",
                -0.05d,
                "No aggregate inference.",
                "Direct flow only.",
                1d,
                2d),
            new Dictionary<string, string>
            {
                ["direct"] = "true",
                ["target"] = flowId
            },
            evidenceIds,
            []);

    private static string SecurityScenarioS1()
        => """
            Our backend authentication service has verified controls in production: password minimum length 15 characters, OTP expiry 10 minutes, TOTP window 30 seconds, session idle timeout 20 minutes with risk analysis RA-AUTH-003, re-authentication before username email password or MFA factor changes, JWT validation with RS256 only, none algorithm rejected, exp nbf and aud validated, and RBAC plus per-resource ownership checks on all API endpoints. Does this implementation satisfy OWASP ASVS 5.0.0 for sub-phase 3.4?
            """;

    private static DynamicGraphMeasurementTarget ValidTarget(string edgeId)
        => new(
            edgeId,
            DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
            DynamicGraphMeasurementMechanismKind.LearningSubject,
            false);

    private static DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement
        ValidAssociation(string edgeId)
    {
        var result = DynamicGraphMeasurementCoordinator
            .MeasureEdgeAuthoritySelectionQualityEvidence(
                ValidTarget(edgeId),
                ValidAuthorityResult(edgeId),
                ValidNoveltyRequest(edgeId));
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return Assert.IsType<
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>(result.Value);
    }

    private static DynamicGraphEdgeAuthorityNoveltyEvaluationRequest ValidNoveltyRequest(
        string edgeId)
    {
        var paired = Paired(edgeId);
        return new DynamicGraphEdgeAuthorityNoveltyEvaluationRequest(
            paired,
            [
                new DynamicGraphEdgeAuthorityNoveltyTrainingBinding(
                    "train-a",
                    "train-hash",
                    [A],
                    [A])
            ],
            paired.Rows.Select(row => row.RowId).ToArray(),
            Enumerable.Range(0, paired.Rows.Count)
                .Select(index => new DynamicGraphEdgeAuthorityNoveltyRowInput(
                    $"row-{index:D3}",
                    index % 2 == 0 ? B : C,
                    [index % 2 == 0 ? B : C]))
                .ToArray(),
            "fingerprint-normalization-v1",
            "tokenization-v1");
    }

    private static DynamicGraphEdgeAuthorityAblationEvaluationResult Paired(string edgeId)
    {
        const int rowCount = 2;
        const int learnedCorrect = 2;
        const int baselineCorrect = 1;
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
            LeakageSignals);
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
            LeakageSignals);
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
            LeakageSignals);

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

    private static InMemoryLogicalTopologyRegistry PopulatedTopologyRegistry()
    {
        var registry = new InMemoryLogicalTopologyRegistry();
        var domain = registry.AddOrUpdateDomain(new LogicalDomain(
            "host-domain",
            "Host domain",
            "Sentinel domain for registry non-mutation proof.",
            ["HOST_DOMAIN_PREDICATE"],
            ["host-domain-source"]));
        Assert.True(domain.IsSuccess, domain.ErrorMessage);
        var phase = registry.AddOrUpdatePhase(new LogicalPhase(
            "host-phase",
            "Host phase",
            "Sentinel phase for registry non-mutation proof.",
            ["host-domain"],
            ["HOST_PHASE_PREDICATE"],
            ["host-phase-source"]));
        Assert.True(phase.IsSuccess, phase.ErrorMessage);
        var subPhase = registry.AddOrUpdateSubPhase(new LogicalSubPhase(
            "host-subphase",
            "host-phase",
            "Host subphase",
            "Sentinel subphase for registry non-mutation proof.",
            ["host-domain"],
            ["HOST_SUBPHASE_PREDICATE"],
            ["host-subphase-source"]));
        Assert.True(subPhase.IsSuccess, subPhase.ErrorMessage);
        var transition = registry.AddOrUpdateTransition(new LogicalTransition(
            "host-transition",
            "phase",
            "host-phase",
            "subphase",
            "host-subphase",
            "Sentinel transition for registry non-mutation proof.",
            ["HOST_TRANSITION_PREDICATE"],
            ["HOST_TRANSITION_CONSTRAINT"],
            ["host-transition-source"]));
        Assert.True(transition.IsSuccess, transition.ErrorMessage);
        return registry;
    }

    private static string[] SnapshotFlowDescriptors(InMemoryLogicalFlowRegistry registry)
        => registry.ListDescriptors()
            .Select(descriptor => string.Join(
                "|",
                descriptor.FlowId,
                descriptor.ScenarioId,
                descriptor.Version,
                descriptor.InputType.AssemblyQualifiedName,
                descriptor.PhaseId,
                descriptor.SubPhaseId,
                descriptor.TaskType,
                string.Join(",", descriptor.DomainTags),
                descriptor.RegistrationOrder))
            .ToArray();

    private static string[] SnapshotUnitDescriptors(InMemoryLogicalUnitRegistry registry)
        => registry.ListDescriptors()
            .Select(descriptor => string.Join(
                "|",
                descriptor.UnitId,
                descriptor.UnitRole,
                descriptor.DomainTag,
                descriptor.Version,
                descriptor.TrustState,
                descriptor.IsLearned,
                descriptor.InputType.AssemblyQualifiedName,
                descriptor.OutputType.AssemblyQualifiedName,
                descriptor.RegistrationOrder))
            .ToArray();

    private static string[] SnapshotTopology(ILogicalTopologyRegistry registry)
    {
        var domains = registry.ListDomains();
        var phases = registry.ListPhases();
        var subPhases = registry.ListSubPhases();
        var transitions = registry.ListTransitions();
        Assert.True(domains.IsSuccess, domains.ErrorMessage);
        Assert.True(phases.IsSuccess, phases.ErrorMessage);
        Assert.True(subPhases.IsSuccess, subPhases.ErrorMessage);
        Assert.True(transitions.IsSuccess, transitions.ErrorMessage);

        return domains.Value!
            .Select(entry => string.Join(
                "|",
                "domain",
                entry.Domain.DomainId,
                entry.Domain.Name,
                entry.Domain.Purpose,
                string.Join(",", entry.Domain.PredicateIds),
                string.Join(",", entry.Domain.SourceIds),
                entry.Revision,
                entry.UpdatedAtUtc.ToString("O")))
            .Concat(phases.Value!.Select(entry => string.Join(
                "|",
                "phase",
                entry.Phase.PhaseId,
                entry.Phase.Name,
                entry.Phase.Purpose,
                string.Join(",", entry.Phase.DomainIds),
                string.Join(",", entry.Phase.PredicateIds),
                string.Join(",", entry.Phase.SourceIds),
                entry.Revision,
                entry.UpdatedAtUtc.ToString("O"))))
            .Concat(subPhases.Value!.Select(entry => string.Join(
                "|",
                "subphase",
                entry.SubPhase.SubPhaseId,
                entry.SubPhase.PhaseId,
                entry.SubPhase.Name,
                entry.SubPhase.Purpose,
                string.Join(",", entry.SubPhase.DomainIds),
                string.Join(",", entry.SubPhase.PredicateIds),
                string.Join(",", entry.SubPhase.SourceIds),
                entry.Revision,
                entry.UpdatedAtUtc.ToString("O"))))
            .Concat(transitions.Value!.Select(entry => string.Join(
                "|",
                "transition",
                entry.Transition.TransitionId,
                entry.Transition.FromTopologyKind,
                entry.Transition.FromTopologyId,
                entry.Transition.ToTopologyKind,
                entry.Transition.ToTopologyId,
                entry.Transition.Purpose,
                string.Join(",", entry.Transition.PredicateIds),
                string.Join(",", entry.Transition.ConstraintIds),
                string.Join(",", entry.Transition.SourceIds),
                entry.Revision,
                entry.UpdatedAtUtc.ToString("O"))))
            .ToArray();
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

    private sealed class RecordingFlow : ILogicalFlow<string>
    {
        private readonly Func<
            LogicalFlowRequest<string>,
            CancellationToken,
            Task<OperationResult<LogicalFlowResult>>> _execute;
        private int _callCount;

        private RecordingFlow(
            Func<
                LogicalFlowRequest<string>,
                CancellationToken,
                Task<OperationResult<LogicalFlowResult>>> execute)
        {
            _execute = execute;
        }

        internal int CallCount => Volatile.Read(ref _callCount);

        internal LogicalFlowRequest<string>? SeenRequest { get; private set; }

        public Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
            LogicalFlowRequest<string> request,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _callCount);
            SeenRequest = request;
            return _execute(request, cancellationToken);
        }

        internal static RecordingFlow Returning(LogicalFlowResult result)
            => new((_, _) => Task.FromResult(OperationResult<LogicalFlowResult>.Ok(result)));
    }

    private sealed class RecordingFactory : ILogicalFlowFactory<string>
    {
        private readonly Func<
            LogicalFlowFactoryRequest,
            OperationResult<ILogicalFlow<string>>> _create;
        private int _callCount;

        internal RecordingFactory(
            Func<LogicalFlowFactoryRequest, OperationResult<ILogicalFlow<string>>> create)
        {
            _create = create;
        }

        internal int CallCount => Volatile.Read(ref _callCount);

        internal LogicalFlowFactoryRequest? SeenRequest { get; private set; }

        public OperationResult<ILogicalFlow<string>> Create(LogicalFlowFactoryRequest request)
        {
            Interlocked.Increment(ref _callCount);
            SeenRequest = request;
            return _create(request);
        }

        internal static RecordingFactory Returning(ILogicalFlow<string> flow)
            => new(_ => OperationResult<ILogicalFlow<string>>.Ok(flow));
    }

    private sealed class RecordingOrchestrator : ILogicalOrchestrator
    {
        private int _callCount;

        internal int CallCount => Volatile.Read(ref _callCount);

        public Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
            LogicalOrchestratorRequest request,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _callCount);
            return Task.FromResult(OperationResult<LogicalOrchestratorResult>.Fail(
                "Legacy orchestrator execution is not expected.",
                "LEGACY_ORCHESTRATOR_NOT_EXPECTED"));
        }
    }

    private sealed class RecordingSessionStorage : ISessionStorage
    {
        public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
            => Task.FromResult(OperationResult<bool>.Ok(true));

        public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
            => Task.FromResult(OperationResult<AgentState>.Fail(
                "No session was recorded.",
                "SESSION_NOT_RECORDED"));

        public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
            => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok([]));
    }

    private sealed class SentinelUnit : ILogicalUnit<string, string>
    {
        public Task<OperationResult<LogicalUnitResult<string>>> ExecuteAsync(
            LogicalUnitRequest<string> request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<LogicalUnitResult<string>>.Ok(
                new LogicalUnitResult<string>(
                    request.UnitId,
                    request.UnitRole,
                    request.DomainTag,
                    "unused",
                    1d,
                    ["HOST_SENTINEL_PREDICATE"],
                    ["host-sentinel-domain"],
                    ["host-sentinel-evidence"],
                    [])));
    }
}
