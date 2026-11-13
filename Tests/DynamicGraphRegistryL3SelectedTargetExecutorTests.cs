// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

public sealed class DynamicGraphRegistryL3SelectedTargetExecutorTests
{
    private const string EdgeId = "registry-selected-target";
    private const string RunId = "registry-selected-run";
    private const string ScenarioId = "registry-selected-scenario";
    private const string Input = "Execute the admitted selected target.";
    private const string AuthorityUnitId = "registry-selected-authority";
    private const string EvaluationId = "registry-selected-evaluation";
    private const string A = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string B = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string C = "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";

    private const string RegistryRequiredCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_REGISTRY_REQUIRED";
    private const string RegistryRequiredMessage =
        "Dynamic graph L3 selected-target flow registry is required.";
    private const string RequestInvalidCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_REQUEST_INVALID";
    private const string RequestInvalidMessage =
        "Dynamic graph L3 selected-target flow request is invalid.";
    private const string AlreadyStartedCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_EXECUTION_ALREADY_STARTED";
    private const string AlreadyStartedMessage =
        "Dynamic graph L3 selected-target flow execution has already started.";
    private const string AssociationInvalidCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_ASSOCIATION_INVALID";
    private const string AssociationInvalidMessage =
        "Dynamic graph L3 selected-target association is invalid.";
    private const string ResolutionFailedCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_RESOLUTION_FAILED";
    private const string ResolutionFailedMessage =
        "Dynamic graph L3 selected-target flow resolution failed.";
    private const string ExecutionFailedCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_EXECUTION_FAILED";
    private const string ExecutionFailedMessage =
        "Dynamic graph L3 selected-target flow execution failed.";
    private const string ResultRequiredCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_RESULT_REQUIRED";
    private const string ResultRequiredMessage =
        "Dynamic graph L3 selected-target flow execution result is required.";
    private const string ResultInvalidCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_RESULT_INVALID";
    private const string ResultInvalidMessage =
        "Dynamic graph L3 selected-target flow execution result is invalid.";
    private const string IdentityMismatchCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_RESULT_IDENTITY_MISMATCH";
    private const string IdentityMismatchMessage =
        "Dynamic graph L3 selected-target flow execution result does not match the request.";

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
    public void Carrier_PublicSurfaceUnchangedAndInternalBindingReturnsFreshTarget()
    {
        var association = ValidAssociation();
        var properties = typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .OrderBy(property => property.MetadataToken)
            .ToArray();

        Assert.Equal(
            ["Target", "EdgeAuthoritySelectionQualityEvidence"],
            properties.Select(property => property.Name));
        var accessor = Assert.IsAssignableFrom<MethodInfo>(
            typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement).GetMethod(
                "SnapshotTargetAuthorityEdgeBinding",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.True(accessor.IsAssembly);
        Assert.Empty(accessor.GetParameters());
        var fields = typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Equal(
            ["_target", "_edgeAuthoritySelectionQualityEvidence", "_authorityEdgeId"],
            fields.OrderBy(field => field.MetadataToken).Select(field => field.Name));
        Assert.All(fields, field => Assert.True(field.IsInitOnly));

        var first = Assert.IsType<ValueTuple<DynamicGraphMeasurementTarget, string>>(
            accessor.Invoke(association, null));
        var second = Assert.IsType<ValueTuple<DynamicGraphMeasurementTarget, string>>(
            accessor.Invoke(association, null));

        Assert.Equal(EdgeId, first.Item1.TargetId, StringComparer.Ordinal);
        Assert.Equal(EdgeId, first.Item2, StringComparer.Ordinal);
        Assert.Equal(first, second);
        Assert.NotSame(first.Item1, second.Item1);
    }

    [Fact]
    public void Executor_ExposesPrivateCtorExactFactoryAndOptionalExecutionToken()
    {
        var type = typeof(DynamicGraphRegistryL3SelectedTargetExecutor);
        Assert.True(type.IsSealed);
        Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Single(type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Contains(typeof(IDynamicGraphL3SelectedTargetExecutor), type.GetInterfaces());
        var factories = type.GetMethods(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(["Create", "CreateFactoryAware"], factories.Select(method => method.Name));
        var create = Assert.Single(factories, method => method.Name == "Create");
        Assert.Equal("Create", create.Name);
        Assert.Equal(
            [
                typeof(InMemoryLogicalFlowRegistry), typeof(string), typeof(string), typeof(string),
                typeof(LogicalExecutionFlags), typeof(IReadOnlyDictionary<string, object>)
            ],
            create.GetParameters().Select(parameter => parameter.ParameterType));
        var factoryAware = Assert.Single(
            factories,
            method => method.Name == "CreateFactoryAware");
        Assert.Equal(
            [
                typeof(InMemoryLogicalFlowRegistry), typeof(string), typeof(string),
                typeof(LogicalFlowFactoryRequest)
            ],
            factoryAware.GetParameters().Select(parameter => parameter.ParameterType));
        var execute = Assert.Single(type.GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Equal("ExecuteAsync", execute.Name);
        var token = execute.GetParameters()[1];
        Assert.Equal("cancellationToken", token.Name, StringComparer.Ordinal);
        Assert.Equal(typeof(CancellationToken), token.ParameterType);
        Assert.True(token.HasDefaultValue);
        Assert.Null(token.DefaultValue);
    }

    [Fact]
    public void Create_RequiresRegistryWithExactStableFailure()
    {
        var result = DynamicGraphRegistryL3SelectedTargetExecutor.Create(
            null,
            RunId,
            ScenarioId,
            Input,
            new LogicalExecutionFlags(),
            Metadata());

        AssertFailure(result, RegistryRequiredCode, RegistryRequiredMessage);
    }

    [Theory]
    [InlineData("run-null")]
    [InlineData("run-leading")]
    [InlineData("run-surrogate")]
    [InlineData("scenario-empty")]
    [InlineData("scenario-control")]
    [InlineData("input-null")]
    [InlineData("input-whitespace")]
    [InlineData("flags-null")]
    [InlineData("metadata-null")]
    [InlineData("metadata-unsupported")]
    [InlineData("metadata-cycle")]
    public void Create_RejectsInvalidTemplateWithExactStableFailure(string scenario)
    {
        string? runId = RunId;
        string? scenarioId = ScenarioId;
        string? input = Input;
        LogicalExecutionFlags? flags = new();
        IReadOnlyDictionary<string, object>? metadata = Metadata();
        switch (scenario)
        {
            case "run-null": runId = null; break;
            case "run-leading": runId = " run"; break;
            case "run-surrogate": runId = "run\ud800"; break;
            case "scenario-empty": scenarioId = ""; break;
            case "scenario-control": scenarioId = "scenario\u0001"; break;
            case "input-null": input = null; break;
            case "input-whitespace": input = " "; break;
            case "flags-null": flags = null; break;
            case "metadata-null": metadata = null; break;
            case "metadata-unsupported":
                metadata = new Dictionary<string, object>
                {
                    ["unsupported"] = new object()
                }; break;
            case "metadata-cycle":
                var cycle = new Dictionary<string, object>();
                cycle["cycle"] = cycle;
                metadata = cycle;
                break;
            default: throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        var result = DynamicGraphRegistryL3SelectedTargetExecutor.Create(
            new InMemoryLogicalFlowRegistry(),
            runId,
            scenarioId,
            input,
            flags,
            metadata);

        AssertFailure(result, RequestInvalidCode, RequestInvalidMessage);
    }

    [Fact]
    public void CreateFactoryAware_RequiresRegistryWithExactStableFailure()
    {
        var result = DynamicGraphRegistryL3SelectedTargetExecutor.CreateFactoryAware(
            null,
            ScenarioId,
            Input,
            FactoryRequest());

        AssertFailure(result, RegistryRequiredCode, RegistryRequiredMessage);
    }

    [Theory]
    [InlineData("request-null")]
    [InlineData("scenario-null")]
    [InlineData("scenario-leading")]
    [InlineData("input-null")]
    [InlineData("input-whitespace")]
    [InlineData("run-leading")]
    [InlineData("run-surrogate")]
    [InlineData("flags-null")]
    [InlineData("units-null")]
    [InlineData("metadata-null")]
    [InlineData("metadata-unsupported")]
    [InlineData("metadata-cycle")]
    [InlineData("minimum-trust")]
    public void CreateFactoryAware_RejectsInvalidTemplateWithExactStableFailure(
        string scenario)
    {
        string? scenarioId = ScenarioId;
        string? input = Input;
        string? runId = RunId;
        LogicalExecutionFlags? flags = new();
        InMemoryLogicalUnitRegistry? units = new();
        IReadOnlyDictionary<string, object>? metadata = Metadata();
        var minimumTrust = LogicalUnitTrustState.Probationary;
        LogicalFlowFactoryRequest? request;
        switch (scenario)
        {
            case "request-null":
                request = null;
                break;
            case "scenario-null":
                scenarioId = null;
                request = FactoryRequest();
                break;
            case "scenario-leading":
                scenarioId = $" {ScenarioId}";
                request = FactoryRequest();
                break;
            case "input-null":
                input = null;
                request = FactoryRequest();
                break;
            case "input-whitespace":
                input = " ";
                request = FactoryRequest();
                break;
            default:
                if (scenario == "run-leading") runId = " run";
                if (scenario == "run-surrogate") runId = "run\ud800";
                if (scenario == "flags-null") flags = null;
                if (scenario == "units-null") units = null;
                if (scenario == "metadata-null") metadata = null;
                if (scenario == "metadata-unsupported")
                {
                    metadata = new Dictionary<string, object> { ["unsupported"] = new object() };
                }

                if (scenario == "metadata-cycle")
                {
                    var cycle = new Dictionary<string, object>();
                    cycle["cycle"] = cycle;
                    metadata = cycle;
                }

                if (scenario == "minimum-trust")
                    minimumTrust = (LogicalUnitTrustState)int.MaxValue;
                request = new LogicalFlowFactoryRequest(
                    runId!,
                    flags!,
                    units!,
                    metadata!)
                {
                    MinimumTrustState = minimumTrust
                };
                break;
        }

        var result = DynamicGraphRegistryL3SelectedTargetExecutor.CreateFactoryAware(
            new InMemoryLogicalFlowRegistry(),
            scenarioId,
            input,
            request);

        AssertFailure(result, RequestInvalidCode, RequestInvalidMessage);
    }

    [Fact]
    public async Task ExecuteAsync_FactoryAwareOwnsTemplateRetainsDependenciesAndUsesDistinctSnapshots()
    {
        var tags = new[] { "factory-configured-a", "factory-configured-b" };
        var metadata = new Dictionary<string, object>
        {
            ["configured"] = new Dictionary<string, object> { ["tags"] = tags },
            ["purpose"] = "factory-selected-target"
        };
        var flags = new LogicalExecutionFlags { UseSkillRetriever = true };
        var units = new InMemoryLogicalUnitRegistry();
        var topology = new InMemoryLogicalTopologyRegistry();
        var factoryRequest = new LogicalFlowFactoryRequest(
            RunId,
            flags,
            units,
            metadata,
            topology)
        {
            MinimumTrustState = LogicalUnitTrustState.Probationary
        };
        var flow = RecordingFlow.Returning(ValidFlowResult());
        var factory = RecordingFlowFactory.Returning(flow);
        var executor = CreateFactoryAwareExecutor(
            RegistryWithFactory(factory),
            factoryRequest);
        tags[0] = "caller-mutated";
        SetAutoProperty(flags, nameof(LogicalExecutionFlags.UseSkillRetriever), false);

        var result = await executor.ExecuteAsync(ValidAssociation());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, factory.CallCount);
        Assert.Equal(1, flow.CallCount);
        var seenFactory = Assert.IsType<LogicalFlowFactoryRequest>(factory.SeenRequest);
        var seenFlow = Assert.IsType<LogicalFlowRequest<string>>(flow.SeenRequest);
        Assert.NotSame(factoryRequest, seenFactory);
        Assert.Same(units, seenFactory.Units);
        Assert.Same(topology, seenFactory.Topology);
        Assert.Equal(LogicalUnitTrustState.Probationary, seenFactory.MinimumTrustState);
        Assert.Equal(RunId, seenFactory.RunId, StringComparer.Ordinal);
        Assert.Equal(RunId, seenFlow.RunId, StringComparer.Ordinal);
        Assert.Equal(ScenarioId, seenFlow.ScenarioId, StringComparer.Ordinal);
        Assert.Equal(EdgeId, seenFlow.FlowId, StringComparer.Ordinal);
        Assert.Equal(Input, seenFlow.Input, StringComparer.Ordinal);
        var expectedFlags = new LogicalExecutionFlags { UseSkillRetriever = true };
        Assert.Equal(expectedFlags, seenFactory.Flags);
        Assert.Equal(expectedFlags, seenFlow.Flags);
        Assert.Equal(seenFactory.Flags, seenFlow.Flags);
        Assert.NotSame(flags, seenFactory.Flags);
        Assert.NotSame(seenFactory.Flags, seenFlow.Flags);
        Assert.NotSame(metadata, seenFactory.Metadata);
        Assert.NotSame(seenFactory.Metadata, seenFlow.Metadata);
        var expectedMetadataKeys = new[] { "configured", "purpose" };
        Assert.Equal(
            expectedMetadataKeys,
            seenFactory.Metadata.Keys.OrderBy(key => key, StringComparer.Ordinal));
        Assert.Equal(
            expectedMetadataKeys,
            seenFlow.Metadata.Keys.OrderBy(key => key, StringComparer.Ordinal));
        var forbiddenMetadataKeyFragments = new[]
        {
            "target", "scenario", "association", "authority", "evidence", "novelty",
            "training", "ablation"
        };
        Assert.DoesNotContain(seenFactory.Metadata.Keys, key =>
            forbiddenMetadataKeyFragments.Any(fragment =>
                key.Contains(fragment, StringComparison.OrdinalIgnoreCase)));
        Assert.DoesNotContain(seenFlow.Metadata.Keys, key =>
            forbiddenMetadataKeyFragments.Any(fragment =>
                key.Contains(fragment, StringComparison.OrdinalIgnoreCase)));
        var factoryNested = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(
            seenFactory.Metadata["configured"]);
        var flowNested = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(
            seenFlow.Metadata["configured"]);
        var factoryTags = Assert.IsType<string[]>(factoryNested["tags"]);
        var flowTags = Assert.IsType<string[]>(flowNested["tags"]);
        Assert.Equal(["factory-configured-a", "factory-configured-b"], factoryTags);
        Assert.Equal(["factory-configured-a", "factory-configured-b"], flowTags);
        Assert.NotSame(factoryNested, flowNested);
        Assert.NotSame(tags, factoryTags);
        Assert.NotSame(tags, flowTags);
        Assert.NotSame(factoryTags, flowTags);
        Assert.Equal(RunId, result.Value!.ExecutionId, StringComparer.Ordinal);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("scenario-mismatch")]
    [InlineData("newest-direct")]
    [InlineData("descriptor-invalid")]
    [InlineData("binding-mismatch")]
    [InlineData("declared-failure")]
    [InlineData("null-envelope")]
    [InlineData("null-value")]
    [InlineData("ordinary-throw")]
    [InlineData("wrong-case-flow")]
    [InlineData("scenario-only")]
    [InlineData("subphase-only")]
    public async Task ExecuteAsync_FactoryAwareNormalizesResolutionFailures(string scenario)
    {
        var flow = RecordingFlow.Returning(ValidFlowResult());
        LogicalFlowDescriptor? bindingDescriptor = null;
        var factory = scenario switch
        {
            "binding-mismatch" => new RecordingFlowFactory(_ =>
            {
                SetAutoProperty(
                    bindingDescriptor!,
                    nameof(LogicalFlowDescriptor.FlowId),
                    "different-flow");
                return OperationResult<ILogicalFlow<string>>.Ok(flow);
            }),
            "declared-failure" => new RecordingFlowFactory(_ =>
                OperationResult<ILogicalFlow<string>>.Fail(
                    "Declared factory failure.",
                    "DECLARED_FACTORY_FAILURE")),
            "null-envelope" => new RecordingFlowFactory(_ => null!),
            "null-value" => new RecordingFlowFactory(_ =>
                OperationResult<ILogicalFlow<string>>.Ok(null!)),
            "ordinary-throw" => new RecordingFlowFactory(_ =>
                throw new InvalidOperationException("ordinary factory failure")),
            _ => RecordingFlowFactory.Returning(flow)
        };
        var registry = new InMemoryLogicalFlowRegistry();
        if (scenario != "missing")
        {
            var registeredFlowId = scenario switch
            {
                "wrong-case-flow" => EdgeId.ToUpperInvariant(),
                "scenario-only" or "subphase-only" => "different-flow",
                _ => EdgeId
            };
            var registeredScenarioId = scenario switch
            {
                "scenario-mismatch" or "subphase-only" => "other-scenario",
                _ => ScenarioId
            };
            var registeredSubPhaseId = scenario == "subphase-only"
                ? EdgeId
                : "subphase";
            Assert.True(registry.RegisterFactory<string>(
                registeredFlowId,
                registeredScenarioId,
                "v1",
                "phase",
                registeredSubPhaseId,
                "task",
                [],
                factory).IsSuccess);
            if (scenario == "binding-mismatch")
                bindingDescriptor = GetStoredFlowDescriptor(registry);
        }

        if (scenario == "newest-direct")
        {
            Assert.True(registry.Register<string>(
                EdgeId,
                ScenarioId,
                "v2",
                flow).IsSuccess);
        }

        if (scenario == "descriptor-invalid")
        {
            var descriptor = GetStoredFlowDescriptor(registry);
            SetAutoProperty(descriptor, nameof(LogicalFlowDescriptor.ScenarioId), " scenario");
        }

        var result = await CreateFactoryAwareExecutor(registry)
            .ExecuteAsync(ValidAssociation());

        AssertFailure(result, ResolutionFailedCode, ResolutionFailedMessage);
        Assert.Equal(0, flow.CallCount);
        Assert.Equal(
            scenario is "binding-mismatch" or "declared-failure" or "null-envelope"
                or "null-value" or "ordinary-throw"
                ? 1
                : 0,
            factory.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_FactoryAwarePreCancellationConsumesOneShotBeforeFactory()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var factory = RecordingFlowFactory.Returning(
            RecordingFlow.Returning(ValidFlowResult()));
        var executor = CreateFactoryAwareExecutor(RegistryWithFactory(factory));

        var first = await executor.ExecuteAsync(ValidAssociation(), cancellation.Token);
        var second = await executor.ExecuteAsync(ValidAssociation());

        AssertCancelled(first);
        AssertFailure(second, AlreadyStartedCode, AlreadyStartedMessage);
        Assert.Equal(0, factory.CallCount);
    }

    [Theory]
    [InlineData("throw-requested")]
    [InlineData("return-after-cancel")]
    public async Task ExecuteAsync_FactoryAwareCancellationDuringConstructionStopsBeforeFlow(
        string scenario)
    {
        using var cancellation = new CancellationTokenSource();
        var flow = RecordingFlow.Returning(ValidFlowResult());
        var factory = new RecordingFlowFactory(_ =>
        {
            cancellation.Cancel();
            if (scenario == "throw-requested")
                throw new OperationCanceledException("requested factory cancellation");

            return OperationResult<ILogicalFlow<string>>.Ok(flow);
        });

        var result = await CreateFactoryAwareExecutor(RegistryWithFactory(factory)).ExecuteAsync(
            ValidAssociation(),
            cancellation.Token);

        AssertCancelled(result);
        Assert.Equal(1, factory.CallCount);
        Assert.Equal(0, flow.CallCount);
    }

    [Theory]
    [InlineData("unrequested")]
    [InlineData("wrapped-cancellation")]
    [InlineData("fatal")]
    [InlineData("aggregate-cancellation")]
    [InlineData("wrapped-fatal")]
    [InlineData("aggregate-fatal")]
    public async Task ExecuteAsync_FactoryAwareUnrequestedWrappedAndFatalExceptionsEscape(
        string scenario)
    {
        Exception exception = scenario switch
        {
            "unrequested" => new OperationCanceledException("unrequested factory cancellation"),
            "wrapped-cancellation" => new InvalidOperationException(
                "wrapped factory cancellation",
                new OperationCanceledException("inner cancellation")),
            "fatal" => new BadImageFormatException("fatal factory failure"),
            "aggregate-cancellation" => new AggregateException(
                "aggregate factory cancellation",
                new InvalidOperationException("ordinary first"),
                new OperationCanceledException("non-first cancellation")),
            "wrapped-fatal" => new InvalidOperationException(
                "wrapped factory fatal",
                new BadImageFormatException("inner fatal")),
            "aggregate-fatal" => new AggregateException(
                "aggregate factory fatal",
                new InvalidOperationException("ordinary first"),
                new BadImageFormatException("non-first fatal")),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
        var flow = RecordingFlow.Returning(ValidFlowResult());
        var factory = new RecordingFlowFactory(_ => throw exception);

        var thrown = await CaptureExceptionAsync(() =>
            CreateFactoryAwareExecutor(RegistryWithFactory(factory))
                .ExecuteAsync(ValidAssociation()));

        Assert.Same(exception, thrown);
        Assert.Equal(1, factory.CallCount);
        Assert.Equal(0, flow.CallCount);
    }

    [Theory]
    [InlineData("cancellation")]
    [InlineData("fatal")]
    public async Task ExecuteAsync_FactoryAwareCyclicExceptionGraphEscapesSameOuterWithinBound(
        string scenario)
    {
        var (exception, cycle, innerField) = scenario == "cancellation"
            ? CyclicCancellationGraph()
            : CyclicFatalGraph();
        var flow = RecordingFlow.Returning(ValidFlowResult());
        var factory = new RecordingFlowFactory(_ => throw exception);
        Exception? thrown = null;

        try
        {
            thrown = await CaptureExceptionAsync(() => Task.Run(() =>
                    CreateFactoryAwareExecutor(RegistryWithFactory(factory))
                        .ExecuteAsync(ValidAssociation())))
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            innerField.SetValue(cycle, null);
        }

        Assert.Same(exception, thrown);
        Assert.Equal(1, factory.CallCount);
        Assert.Equal(0, flow.CallCount);
    }

    [Theory]
    [InlineData("declared-failure")]
    [InlineData("null-envelope")]
    [InlineData("null-value")]
    [InlineData("invalid-result")]
    public async Task ExecuteAsync_FactorySuccessPreservesDownstreamFlowFailureBoundary(
        string scenario)
    {
        var flow = scenario switch
        {
            "declared-failure" => RecordingFlow.Returning(
                OperationResult<LogicalFlowResult>.Fail(
                    "Declared flow failure.",
                    "DECLARED_FLOW_FAILURE")),
            "null-envelope" => new RecordingFlow((_, _) =>
                Task.FromResult<OperationResult<LogicalFlowResult>>(null!)),
            "null-value" => RecordingFlow.Returning(
                OperationResult<LogicalFlowResult>.Ok(null!)),
            "invalid-result" => RecordingFlow.Returning(
                MalformedFlowResult("evidence-empty")),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
        var factory = RecordingFlowFactory.Returning(flow);

        var result = await CreateFactoryAwareExecutor(RegistryWithFactory(factory))
            .ExecuteAsync(ValidAssociation());

        if (scenario == "declared-failure")
        {
            AssertFailure(result, "DECLARED_FLOW_FAILURE", "Declared flow failure.");
        }
        else if (scenario == "invalid-result")
        {
            AssertFailure(result, ResultInvalidCode, ResultInvalidMessage);
        }
        else
        {
            AssertFailure(result, ResultRequiredCode, ResultRequiredMessage);
        }

        Assert.Equal(1, factory.CallCount);
        Assert.Equal(1, flow.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_FactoryAwareNewestExactFactoryWinsOverOlderFactoryAndDirect()
    {
        var olderFactory = RecordingFlowFactory.Returning(
            RecordingFlow.Returning(ValidFlowResult()));
        var olderDirect = RecordingFlow.Returning(ValidFlowResult());
        var selectedFlow = RecordingFlow.Returning(ValidFlowResult());
        var selectedFactory = RecordingFlowFactory.Returning(selectedFlow);
        var registry = RegistryWithFactory(olderFactory, version: "v1");
        Assert.True(registry.Register<string>(
            EdgeId,
            ScenarioId,
            "v2",
            olderDirect).IsSuccess);
        Assert.True(registry.RegisterFactory<string>(
            EdgeId,
            ScenarioId,
            "v3",
            "phase",
            "subphase",
            "task",
            [],
            selectedFactory).IsSuccess);

        var result = await CreateFactoryAwareExecutor(registry)
            .ExecuteAsync(ValidAssociation());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(0, olderFactory.CallCount);
        Assert.Equal(0, olderDirect.CallCount);
        Assert.Equal(1, selectedFactory.CallCount);
        Assert.Equal(1, selectedFlow.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_FactoryAwareConcurrentSecondCallCannotRecreateFactory()
    {
        using var release = new ManualResetEventSlim(false);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var flow = RecordingFlow.Returning(ValidFlowResult());
        var factory = new RecordingFlowFactory(_ =>
        {
            entered.TrySetResult();
            Assert.True(release.Wait(TimeSpan.FromSeconds(5)));
            return OperationResult<ILogicalFlow<string>>.Ok(flow);
        });
        var executor = CreateFactoryAwareExecutor(RegistryWithFactory(factory));

        var firstTask = Task.Run(() => executor.ExecuteAsync(ValidAssociation()));
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var second = await executor.ExecuteAsync(null!);
        release.Set();
        var first = await firstTask;

        Assert.True(first.IsSuccess, first.ErrorMessage);
        AssertFailure(second, AlreadyStartedCode, AlreadyStartedMessage);
        Assert.Equal(1, factory.CallCount);
        Assert.Equal(1, flow.CallCount);
    }

    [Fact]
    public async Task EndToEnd_20AhTo20AiTo20AkExecutesOneFactoryCreatedExactFlow()
    {
        var flow = RecordingFlow.Returning(ValidFlowResult());
        var factory = RecordingFlowFactory.Returning(flow);
        var adapter = CreateFactoryAwareExecutor(RegistryWithFactory(factory));
        var probeCreation = DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe.Create(adapter);
        Assert.True(probeCreation.IsSuccess, probeCreation.ErrorMessage);

        var result = await probeCreation.Value!.RunAsync(
            ValidTarget(),
            ValidAuthorityResult(),
            ValidNoveltyRequest());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, factory.CallCount);
        Assert.Equal(1, flow.CallCount);
        Assert.Equal(1, result.Value!.DirectTargetInvocationCount);
        Assert.Equal(EdgeId, result.Value.ExecutionResult.TargetId, StringComparer.Ordinal);
        Assert.Equal(RunId, result.Value.ExecutionResult.ExecutionId, StringComparer.Ordinal);
        var seenRequest = Assert.IsType<LogicalFlowRequest<string>>(flow.SeenRequest);
        Assert.Equal(EdgeId, seenRequest.FlowId, StringComparer.Ordinal);
        Assert.Equal(ScenarioId, seenRequest.ScenarioId, StringComparer.Ordinal);
        Assert.Equal(RunId, seenRequest.RunId, StringComparer.Ordinal);
        Assert.Equal(Input, seenRequest.Input, StringComparer.Ordinal);
        Assert.Equal(
            ["evidence-b", "Evidence-B", "evidence-a"],
            result.Value.ExecutionResult.EvidenceIds);
    }

    [Fact]
    public async Task ExecuteAsync_UsesExactOwnedTemplateAndLeaksNoAssociationEvidence()
    {
        var tags = new[] { "configured-a", "configured-b" };
        var nested = new Dictionary<string, object> { ["tags"] = tags };
        var metadata = new Dictionary<string, object>
        {
            ["configured"] = nested,
            ["purpose"] = "selected-target"
        };
        var flags = new LogicalExecutionFlags { UseSkillRetriever = true };
        var flow = RecordingFlow.Returning(ValidFlowResult());
        var registry = RegistryWithDirect(EdgeId, flow);
        var executor = CreateExecutor(registry, flags: flags, metadata: metadata);
        tags[0] = "caller-mutated";
        SetAutoProperty(flags, nameof(LogicalExecutionFlags.UseSkillRetriever), false);
        using var cancellation = new CancellationTokenSource();

        var result = await executor.ExecuteAsync(ValidAssociation(), cancellation.Token);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, flow.CallCount);
        Assert.Equal(cancellation.Token, flow.SeenToken);
        var request = flow.SeenRequest!;
        Assert.Equal(RunId, request.RunId, StringComparer.Ordinal);
        Assert.Equal(EdgeId, request.FlowId, StringComparer.Ordinal);
        Assert.Equal(ScenarioId, request.ScenarioId, StringComparer.Ordinal);
        Assert.Equal(Input, request.Input, StringComparer.Ordinal);
        Assert.True(request.Flags.UseSkillRetriever);
        Assert.Equal(["configured", "purpose"], request.Metadata.Keys);
        Assert.DoesNotContain(request.Metadata.Keys, key =>
            key.Contains("association", StringComparison.OrdinalIgnoreCase)
            || key.Contains("authority", StringComparison.OrdinalIgnoreCase)
            || key.Contains("evidence", StringComparison.OrdinalIgnoreCase));
        var requestNested = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(
            request.Metadata["configured"]);
        Assert.Equal("configured-a", Assert.IsType<string[]>(requestNested["tags"])[0]);
        Assert.Equal(EdgeId, result.Value!.TargetId, StringComparer.Ordinal);
        Assert.Equal(RunId, result.Value.ExecutionId, StringComparer.Ordinal);
        Assert.Equal(["evidence-b", "Evidence-B", "evidence-a"], result.Value.EvidenceIds);
    }

    [Theory]
    [InlineData("novelty")]
    [InlineData("training")]
    [InlineData("rows")]
    [InlineData("ablation")]
    public async Task ExecuteAsync_UsesOnlyCachedBindingWhenFullEvidenceTraversalIsCorrupt(
        string scenario)
    {
        var association = ValidAssociation();
        CorruptAssociationEvidence(association, scenario);
        Assert.ThrowsAny<Exception>(() => _ = association.EdgeAuthoritySelectionQualityEvidence);
        var flow = RecordingFlow.Returning(ValidFlowResult());
        var executor = CreateExecutor(RegistryWithDirect(EdgeId, flow));

        var result = await executor.ExecuteAsync(association);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, flow.CallCount);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("cached-case")]
    [InlineData("cached-surrogate")]
    [InlineData("target-case")]
    [InlineData("target-surrogate")]
    [InlineData("level")]
    [InlineData("mechanism")]
    [InlineData("deterministic")]
    public async Task ExecuteAsync_RejectsCorruptAssociationBeforeRegistryRoute(string scenario)
    {
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement? association =
            ValidAssociation();
        if (scenario == "null")
            association = null;
        else
            CorruptAssociationBinding(association, scenario);
        var oldFlow = RecordingFlow.Returning(ValidFlowResult());
        var registry = RegistryWithDirect(EdgeId, oldFlow);
        Assert.True(registry.RegisterFactory<string>(
            EdgeId,
            "newest-factory-scenario",
            "v2",
            "phase-newest",
            "subphase-newest",
            "task-newest",
            [],
            new FailingIfCreatedFactory()).IsSuccess);

        var result = await CreateExecutor(registry).ExecuteAsync(association!);

        AssertFailure(result, AssociationInvalidCode, AssociationInvalidMessage);
        Assert.Equal(0, oldFlow.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ResolvesExactFlowIdIgnoringScenarioAndSubPhaseRoutes()
    {
        var selected = RecordingFlow.Returning(ValidFlowResult());
        var scenarioFallback = RecordingFlow.Returning(
            ValidFlowResult(flowId: "scenario-fallback"));
        var subPhaseFallback = RecordingFlow.Returning(
            ValidFlowResult(flowId: "subphase-fallback"));
        var registry = new InMemoryLogicalFlowRegistry();
        Assert.True(registry.Register<string>(
            "scenario-fallback",
            ScenarioId,
            "v1",
            "phase-a",
            "subphase-a",
            "task-a",
            [],
            scenarioFallback).IsSuccess);
        Assert.True(registry.Register<string>(
            "subphase-fallback",
            "other-scenario",
            "v1",
            "phase-b",
            EdgeId,
            "task-b",
            [],
            subPhaseFallback).IsSuccess);
        Assert.True(registry.Register<string>(
            EdgeId,
            "descriptor-scenario-does-not-control-execution",
            "v1",
            "phase-selected",
            "unrelated-subphase",
            "task-selected",
            [],
            selected).IsSuccess);

        var result = await CreateExecutor(registry).ExecuteAsync(ValidAssociation());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, selected.CallCount);
        Assert.Equal(0, scenarioFallback.CallCount);
        Assert.Equal(0, subPhaseFallback.CallCount);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("case")]
    [InlineData("wrong-type")]
    [InlineData("scenario-only")]
    [InlineData("subphase-only")]
    public async Task ExecuteAsync_HasNoResolutionFallback(string scenario)
    {
        var registry = new InMemoryLogicalFlowRegistry();
        var unselected = RecordingFlow.Returning(ValidFlowResult(flowId: "unselected"));
        switch (scenario)
        {
            case "missing": break;
            case "case":
                Assert.True(registry.Register<string>(
                    EdgeId.ToUpperInvariant(), "case", "v1", unselected).IsSuccess);
                break;
            case "wrong-type":
                Assert.True(registry.Register<int>(
                    EdgeId, "wrong-type", "v1", new IntFlow()).IsSuccess);
                break;
            case "scenario-only":
                Assert.True(registry.Register<string>(
                    "different-flow", ScenarioId, "v1", unselected).IsSuccess);
                break;
            case "subphase-only":
                Assert.True(registry.Register<string>(
                    "different-flow",
                    "different-scenario",
                    "v1",
                    "phase",
                    EdgeId,
                    "task",
                    [],
                    unselected).IsSuccess);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        var result = await CreateExecutor(registry).ExecuteAsync(ValidAssociation());

        AssertFailure(result, ResolutionFailedCode, ResolutionFailedMessage);
        Assert.Equal(0, unselected.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_UsesNewestExactDirectRegistrationOnce()
    {
        var older = RecordingFlow.Returning(ValidFlowResult(evidenceIds: ["older"]));
        var newest = RecordingFlow.Returning(ValidFlowResult(evidenceIds: ["newest"]));
        var registry = RegistryWithDirect(EdgeId, older, version: "v1");
        Assert.True(registry.Register<string>(EdgeId, "newest", "v2", newest).IsSuccess);

        var result = await CreateExecutor(registry).ExecuteAsync(ValidAssociation());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(["newest"], result.Value!.EvidenceIds);
        Assert.Equal(0, older.CallCount);
        Assert.Equal(1, newest.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_NewestFactoryFailureDoesNotFallBackToOlderDirectFlow()
    {
        var older = RecordingFlow.Returning(ValidFlowResult(evidenceIds: ["older"]));
        var registry = RegistryWithDirect(EdgeId, older, version: "v1");
        var factory = new FailingIfCreatedFactory();
        Assert.True(registry.RegisterFactory<string>(
            EdgeId,
            "newest",
            "v2",
            "phase",
            "subphase",
            "task",
            [],
            factory).IsSuccess);

        var result = await CreateExecutor(registry).ExecuteAsync(ValidAssociation());

        AssertFailure(result, ResolutionFailedCode, ResolutionFailedMessage);
        Assert.Equal(0, older.CallCount);
        Assert.Equal(0, factory.CreateCount);
    }

    [Fact]
    public async Task ExecuteAsync_SequentialSecondCallFailsBeforeAssociationAndDoesNotReinvoke()
    {
        var flow = RecordingFlow.Returning(ValidFlowResult());
        var executor = CreateExecutor(RegistryWithDirect(EdgeId, flow));

        var first = await executor.ExecuteAsync(ValidAssociation());
        var second = await executor.ExecuteAsync(null!);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        AssertFailure(second, AlreadyStartedCode, AlreadyStartedMessage);
        Assert.Equal(1, flow.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ConcurrentSecondCallFailsWhileFirstIsSingleInvocation()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var flow = new RecordingFlow(async (_, _) =>
        {
            entered.TrySetResult();
            await release.Task.ConfigureAwait(false);
            return OperationResult<LogicalFlowResult>.Ok(ValidFlowResult());
        });
        var executor = CreateExecutor(RegistryWithDirect(EdgeId, flow));

        var firstTask = executor.ExecuteAsync(ValidAssociation());
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var second = await executor.ExecuteAsync(null!);
        release.TrySetResult();
        var first = await firstTask;

        Assert.True(first.IsSuccess, first.ErrorMessage);
        AssertFailure(second, AlreadyStartedCode, AlreadyStartedMessage);
        Assert.Equal(1, flow.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_PreCancellationConsumesOneShotBeforeAssociation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var flow = RecordingFlow.Returning(ValidFlowResult());
        var executor = CreateExecutor(RegistryWithDirect(EdgeId, flow));

        var first = await executor.ExecuteAsync(null!, cancellation.Token);
        var second = await executor.ExecuteAsync(ValidAssociation());

        AssertCancelled(first);
        AssertFailure(second, AlreadyStartedCode, AlreadyStartedMessage);
        Assert.Equal(0, flow.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_RequestedFlowCancellationReturnsCancelled()
    {
        using var cancellation = new CancellationTokenSource();
        var exception = new OperationCanceledException("requested");
        var flow = new RecordingFlow((_, _) =>
        {
            cancellation.Cancel();
            return Task.FromException<OperationResult<LogicalFlowResult>>(exception);
        });

        var result = await CreateExecutor(RegistryWithDirect(EdgeId, flow)).ExecuteAsync(
            ValidAssociation(),
            cancellation.Token);

        AssertCancelled(result);
        Assert.Equal(1, flow.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_CancellationAfterAwaitWinsBeforePublication()
    {
        using var cancellation = new CancellationTokenSource();
        var flow = new RecordingFlow(async (_, _) =>
        {
            await Task.Yield();
            cancellation.Cancel();
            return OperationResult<LogicalFlowResult>.Ok(ValidFlowResult());
        });

        var result = await CreateExecutor(RegistryWithDirect(EdgeId, flow)).ExecuteAsync(
            ValidAssociation(),
            cancellation.Token);

        AssertCancelled(result);
        Assert.Equal(1, flow.CallCount);
    }

    [Theory]
    [InlineData("null-task")]
    [InlineData("null-envelope")]
    [InlineData("null-value")]
    public async Task ExecuteAsync_MissingTaskEnvelopeOrValueReturnsResultRequired(string scenario)
    {
        var flow = scenario switch
        {
            "null-task" => new RecordingFlow((_, _) => null!),
            "null-envelope" => new RecordingFlow((_, _) =>
                Task.FromResult<OperationResult<LogicalFlowResult>>(null!)),
            "null-value" => RecordingFlow.Returning(
                OperationResult<LogicalFlowResult>.Ok(null!)),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };

        var result = await CreateExecutor(RegistryWithDirect(EdgeId, flow))
            .ExecuteAsync(ValidAssociation());

        AssertFailure(result, ResultRequiredCode, ResultRequiredMessage);
        Assert.Equal(1, flow.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_DeclaredFlowFailurePropagatesUnchanged()
    {
        var flow = RecordingFlow.Returning(
            OperationResult<LogicalFlowResult>.Fail(
                "Declared flow failure.",
                "DECLARED_FLOW_FAILURE"));

        var result = await CreateExecutor(RegistryWithDirect(EdgeId, flow))
            .ExecuteAsync(ValidAssociation());

        AssertFailure(result, "DECLARED_FLOW_FAILURE", "Declared flow failure.");
    }

    [Theory]
    [InlineData("sync")]
    [InlineData("async")]
    public async Task ExecuteAsync_NonfatalFlowExceptionReturnsExecutionFailed(string scenario)
    {
        var exception = new InvalidOperationException("ordinary flow failure");
        var flow = scenario == "sync"
            ? new RecordingFlow((_, _) => throw exception)
            : new RecordingFlow((_, _) =>
                Task.FromException<OperationResult<LogicalFlowResult>>(exception));

        var result = await CreateExecutor(RegistryWithDirect(EdgeId, flow))
            .ExecuteAsync(ValidAssociation());

        AssertFailure(result, ExecutionFailedCode, ExecutionFailedMessage);
        Assert.Equal(1, flow.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_UnrequestedOperationCancellationEscapesSameInstance()
    {
        var exception = new OperationCanceledException("unrequested");
        var flow = new RecordingFlow((_, _) =>
            Task.FromException<OperationResult<LogicalFlowResult>>(exception));

        var thrown = await Assert.ThrowsAsync<OperationCanceledException>(() =>
            CreateExecutor(RegistryWithDirect(EdgeId, flow)).ExecuteAsync(ValidAssociation()));

        Assert.Same(exception, thrown);
    }

    [Fact]
    public async Task Create_MetadataCancellationGraphEscapesSameOuter()
    {
        var exception = new OperationCanceledException("unrequested metadata cancellation");
        var metadata = new ThrowingDictionary<string, object>(exception);

        var thrown = await CaptureExceptionAsync(() => Task.Run(() =>
            DynamicGraphRegistryL3SelectedTargetExecutor.Create(
                new InMemoryLogicalFlowRegistry(),
                RunId,
                ScenarioId,
                Input,
                new LogicalExecutionFlags(),
                metadata)));

        Assert.Same(exception, thrown);
    }

    [Theory]
    [InlineData("direct")]
    [InlineData("wrapped")]
    [InlineData("aggregate-not-first")]
    public async Task ExecuteAsync_FatalFlowGraphsEscapeSameOuter(string scenario)
    {
        var exception = FatalGraph(scenario);
        var flow = new RecordingFlow((_, _) =>
            Task.FromException<OperationResult<LogicalFlowResult>>(exception));

        var thrown = await CaptureExceptionAsync(() =>
            CreateExecutor(RegistryWithDirect(EdgeId, flow)).ExecuteAsync(ValidAssociation()));

        Assert.Same(exception, thrown);
    }

    [Fact]
    public async Task ExecuteAsync_CyclicFatalFlowGraphEscapesSameOuterWithinBoundedTraversal()
    {
        var (exception, cycle, innerField) = CyclicFatalGraph();
        var flow = new RecordingFlow((_, _) =>
            Task.FromException<OperationResult<LogicalFlowResult>>(exception));
        Exception? thrown = null;

        try
        {
            thrown = await CaptureExceptionAsync(() => Task.Run(() =>
                    CreateExecutor(RegistryWithDirect(EdgeId, flow))
                        .ExecuteAsync(ValidAssociation())))
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            innerField.SetValue(cycle, null);
        }

        Assert.Same(exception, thrown);
    }

    [Theory]
    [InlineData("flow-null")]
    [InlineData("flow-leading")]
    [InlineData("scenario-null")]
    [InlineData("verdict-surrogate")]
    [InlineData("human-empty")]
    [InlineData("summary-null")]
    [InlineData("summary-nan")]
    [InlineData("metrics-null")]
    [InlineData("evidence-empty")]
    [InlineData("evidence-duplicate")]
    [InlineData("evidence-leading")]
    [InlineData("evidence-surrogate")]
    [InlineData("warnings-null")]
    [InlineData("signals-null")]
    [InlineData("signal-invalid")]
    [InlineData("handoff-run-leading")]
    [InlineData("handoff-kind-undefined")]
    [InlineData("handoff-target-duplicate")]
    [InlineData("handoff-target-nan")]
    [InlineData("handoff-target-ordinal")]
    [InlineData("handoff-refs-duplicate")]
    [InlineData("handoff-knowledge-cardinality")]
    [InlineData("nonfatal-traversal")]
    public async Task ExecuteAsync_RejectsMalformedFlowResult(string scenario)
    {
        var value = MalformedFlowResult(scenario);
        var flow = RecordingFlow.Returning(OperationResult<LogicalFlowResult>.Ok(value));

        var result = await CreateExecutor(RegistryWithDirect(EdgeId, flow))
            .ExecuteAsync(ValidAssociation());

        AssertFailure(result, ResultInvalidCode, ResultInvalidMessage);
    }

    [Theory]
    [InlineData("flow", "different-flow", ScenarioId)]
    [InlineData("flow-case", "REGISTRY-SELECTED-TARGET", ScenarioId)]
    [InlineData("scenario", EdgeId, "different-scenario")]
    [InlineData("scenario-case", EdgeId, "REGISTRY-SELECTED-SCENARIO")]
    public async Task ExecuteAsync_RequiresOrdinalFlowAndScenarioIdentity(
        string _,
        string flowId,
        string scenarioId)
    {
        var flow = RecordingFlow.Returning(
            ValidFlowResult(flowId: flowId, scenarioId: scenarioId));

        var result = await CreateExecutor(RegistryWithDirect(EdgeId, flow))
            .ExecuteAsync(ValidAssociation());

        AssertFailure(result, IdentityMismatchCode, IdentityMismatchMessage);
    }

    [Fact]
    public async Task ExecuteAsync_FalseVerdictAndFalseSummaryPassStillSucceed()
    {
        var flow = RecordingFlow.Returning(ValidFlowResult(
            verdict: "REJECT",
            passed: false,
            shouldScale: false));

        var result = await CreateExecutor(RegistryWithDirect(EdgeId, flow))
            .ExecuteAsync(ValidAssociation());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
    }

    [Fact]
    public async Task ExecuteAsync_AcceptsStructurallyValidOptionalHandoff()
    {
        var value = ValidFlowResult() with { EdgeTargetHandoff = ValidHandoff() };
        var flow = RecordingFlow.Returning(value);

        var result = await CreateExecutor(RegistryWithDirect(EdgeId, flow))
            .ExecuteAsync(ValidAssociation());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
    }

    [Fact]
    public async Task ExecuteAsync_DeepSnapshotsFlowEvidencePreservingOrdinalOrder()
    {
        var evidence = new[] { "evidence-b", "Evidence-B", "evidence-a" };
        var flowResult = ValidFlowResult(evidenceIds: evidence);
        var flow = RecordingFlow.Returning(flowResult);

        var result = await CreateExecutor(RegistryWithDirect(EdgeId, flow))
            .ExecuteAsync(ValidAssociation());
        evidence[0] = "mutated";
        flowResult.EvidenceIds[1] = "mutated-again";

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(["evidence-b", "Evidence-B", "evidence-a"], result.Value!.EvidenceIds);
    }

    [Theory]
    [InlineData("direct")]
    [InlineData("wrapped")]
    [InlineData("aggregate-not-first")]
    public async Task Create_MetadataFatalGraphsEscapeSameOuter(string scenario)
    {
        var exception = FatalGraph(scenario);
        var metadata = new ThrowingDictionary<string, object>(exception);

        var thrown = await CaptureExceptionAsync(() => Task.Run(() =>
            DynamicGraphRegistryL3SelectedTargetExecutor.Create(
                new InMemoryLogicalFlowRegistry(),
                RunId,
                ScenarioId,
                Input,
                new LogicalExecutionFlags(),
                metadata)));

        Assert.Same(exception, thrown);
    }

    [Fact]
    public async Task Create_MetadataCyclicFatalGraphEscapesSameOuterWithinBoundedTraversal()
    {
        var (exception, cycle, innerField) = CyclicFatalGraph();
        var metadata = new ThrowingDictionary<string, object>(exception);
        Exception? thrown = null;

        try
        {
            thrown = await CaptureExceptionAsync(() => Task.Run(() =>
                    DynamicGraphRegistryL3SelectedTargetExecutor.Create(
                        new InMemoryLogicalFlowRegistry(),
                        RunId,
                        ScenarioId,
                        Input,
                        new LogicalExecutionFlags(),
                        metadata)))
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            innerField.SetValue(cycle, null);
        }

        Assert.Same(exception, thrown);
    }

    [Theory]
    [InlineData("direct")]
    [InlineData("wrapped")]
    [InlineData("aggregate-not-first")]
    public async Task ExecuteAsync_ResultSnapshotFatalGraphsEscapeSameOuter(string scenario)
    {
        var exception = FatalGraph(scenario);
        var value = ValidFlowResult() with
        {
            Metrics = new ThrowingDictionary<string, string>(exception)
        };
        var flow = RecordingFlow.Returning(OperationResult<LogicalFlowResult>.Ok(value));

        var thrown = await CaptureExceptionAsync(() =>
            CreateExecutor(RegistryWithDirect(EdgeId, flow)).ExecuteAsync(ValidAssociation()));

        Assert.Same(exception, thrown);
    }

    [Fact]
    public async Task ExecuteAsync_ResultSnapshotCyclicFatalGraphEscapesSameOuterWithinBoundedTraversal()
    {
        var (exception, cycle, innerField) = CyclicFatalGraph();
        var value = ValidFlowResult() with
        {
            Metrics = new ThrowingDictionary<string, string>(exception)
        };
        var flow = RecordingFlow.Returning(OperationResult<LogicalFlowResult>.Ok(value));
        Exception? thrown = null;

        try
        {
            thrown = await CaptureExceptionAsync(() => Task.Run(() =>
                    CreateExecutor(RegistryWithDirect(EdgeId, flow))
                        .ExecuteAsync(ValidAssociation())))
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            innerField.SetValue(cycle, null);
        }

        Assert.Same(exception, thrown);
    }

    [Fact]
    public async Task ExecuteAsync_ResultSnapshotRequestedCancellationReturnsCancelledAndConsumesOneShot()
    {
        using var cancellation = new CancellationTokenSource();
        var exception = new OperationCanceledException("requested result snapshot cancellation");
        var value = ValidFlowResult() with
        {
            Metrics = new CancellingThrowingDictionary<string, string>(cancellation, exception)
        };
        var flow = RecordingFlow.Returning(OperationResult<LogicalFlowResult>.Ok(value));
        var executor = CreateExecutor(RegistryWithDirect(EdgeId, flow));
        Assert.False(cancellation.IsCancellationRequested);

        var first = await executor.ExecuteAsync(ValidAssociation(), cancellation.Token);
        var second = await executor.ExecuteAsync(ValidAssociation());

        AssertCancelled(first);
        AssertFailure(second, AlreadyStartedCode, AlreadyStartedMessage);
        Assert.Equal(1, flow.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ResultSnapshotCancellationGraphEscapesSameOuter()
    {
        var exception = new OperationCanceledException("unrequested result cancellation");
        var value = ValidFlowResult() with
        {
            Metrics = new ThrowingDictionary<string, string>(exception)
        };
        var flow = RecordingFlow.Returning(OperationResult<LogicalFlowResult>.Ok(value));

        var thrown = await CaptureExceptionAsync(() =>
            CreateExecutor(RegistryWithDirect(EdgeId, flow)).ExecuteAsync(ValidAssociation()));

        Assert.Same(exception, thrown);
    }

    [Fact]
    public async Task EndToEnd_20AhTo20AiTo20AjExecutesOneExactRegistryFlow()
    {
        var flow = RecordingFlow.Returning(ValidFlowResult());
        var adapter = CreateExecutor(RegistryWithDirect(EdgeId, flow));
        var probeCreation = DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe.Create(adapter);
        Assert.True(probeCreation.IsSuccess, probeCreation.ErrorMessage);

        var result = await probeCreation.Value!.RunAsync(
            ValidTarget(),
            ValidAuthorityResult(),
            ValidNoveltyRequest());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, flow.CallCount);
        Assert.Equal(1, result.Value!.DirectTargetInvocationCount);
        Assert.Equal(EdgeId, result.Value.Association.Target.TargetId, StringComparer.Ordinal);
        Assert.Equal(EdgeId, result.Value.ExecutionResult.TargetId, StringComparer.Ordinal);
        Assert.Equal(RunId, result.Value.ExecutionResult.ExecutionId, StringComparer.Ordinal);
        Assert.Equal(["evidence-b", "Evidence-B", "evidence-a"],
            result.Value.ExecutionResult.EvidenceIds);
    }

    [Fact]
    public void LogicalMetadataSnapshot_UsesGraphClassifierForBothPublicBoundaries()
    {
        var filters = typeof(LogicalMetadataSnapshot)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => method.Name == "TryCreate" && method.GetParameters().Length == 2)
            .Select(method => method.GetMethodBody())
            .Where(body => body is not null)
            .SelectMany(body => body!.ExceptionHandlingClauses)
            .Where(clause => clause.Flags == ExceptionHandlingClauseOptions.Filter)
            .ToArray();
        var classifier = Assert.IsAssignableFrom<MethodInfo>(
            typeof(SnapshotExceptionGraphClassifier).GetMethod(
                nameof(SnapshotExceptionGraphClassifier.IsNonFatal),
                BindingFlags.Public | BindingFlags.Static));

        Assert.Equal(2, filters.Length);
        Assert.All(filters, clause => Assert.True(clause.FilterOffset >= 0));
        Assert.NotNull(classifier);
        Assert.Null(typeof(LogicalMetadataSnapshot).GetMethod(
            "IsFatal",
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly));
    }

    private static DynamicGraphRegistryL3SelectedTargetExecutor CreateExecutor(
        InMemoryLogicalFlowRegistry registry,
        LogicalExecutionFlags? flags = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        var result = DynamicGraphRegistryL3SelectedTargetExecutor.Create(
            registry,
            RunId,
            ScenarioId,
            Input,
            flags ?? new LogicalExecutionFlags(),
            metadata ?? Metadata());
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return result.Value!;
    }

    private static DynamicGraphRegistryL3SelectedTargetExecutor CreateFactoryAwareExecutor(
        InMemoryLogicalFlowRegistry registry,
        LogicalFlowFactoryRequest? factoryRequest = null)
    {
        var result = DynamicGraphRegistryL3SelectedTargetExecutor.CreateFactoryAware(
            registry,
            ScenarioId,
            Input,
            factoryRequest ?? FactoryRequest());
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return result.Value!;
    }

    private static LogicalFlowFactoryRequest FactoryRequest()
        => new(
            RunId,
            new LogicalExecutionFlags(),
            new InMemoryLogicalUnitRegistry(),
            Metadata())
        {
            MinimumTrustState = LogicalUnitTrustState.Probationary
        };

    private static InMemoryLogicalFlowRegistry RegistryWithFactory(
        ILogicalFlowFactory<string> factory,
        string version = "v1")
    {
        var registry = new InMemoryLogicalFlowRegistry();
        var registered = registry.RegisterFactory<string>(
            EdgeId,
            ScenarioId,
            version,
            "phase",
            "subphase",
            "task",
            [],
            factory);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        return registry;
    }

    private static LogicalFlowDescriptor GetStoredFlowDescriptor(
        InMemoryLogicalFlowRegistry registry)
    {
        var field = Assert.IsAssignableFrom<FieldInfo>(
            typeof(InMemoryLogicalFlowRegistry).GetField(
                "_entries",
                BindingFlags.NonPublic | BindingFlags.Instance));
        var entries = Assert.IsAssignableFrom<IList>(field.GetValue(registry));
        var tuple = Assert.Single(entries.Cast<object>());
        var descriptorField = Assert.IsAssignableFrom<FieldInfo>(
            tuple.GetType().GetField("Item1", BindingFlags.Public | BindingFlags.Instance));
        return Assert.IsType<LogicalFlowDescriptor>(descriptorField.GetValue(tuple));
    }

    private static InMemoryLogicalFlowRegistry RegistryWithDirect(
        string flowId,
        ILogicalFlow<string> flow,
        string version = "v1")
    {
        var registry = new InMemoryLogicalFlowRegistry();
        var registered = registry.Register<string>(flowId, "descriptor-scenario", version, flow);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        return registry;
    }

    private static IReadOnlyDictionary<string, object> Metadata()
        => new Dictionary<string, object>
        {
            ["mode"] = "selected-target",
            ["tags"] = new[] { "configured" }
        };

    private static LogicalFlowResult ValidFlowResult(
        string? flowId = EdgeId,
        string? scenarioId = ScenarioId,
        string verdict = "REJECT",
        bool passed = false,
        bool shouldScale = false,
        string[]? evidenceIds = null)
        => new(
            flowId!,
            scenarioId!,
            verdict,
            "Direct selected-target flow completed.",
            new LogicalFlowSummary(
                "selected-target-summary",
                1,
                0.25d,
                0.2d,
                "direct_quality",
                false,
                passed,
                shouldScale,
                "observe",
                -0.05d,
                "No aggregate inference.",
                "Direct flow only.",
                1d,
                2d),
            new Dictionary<string, string>
            {
                ["direct"] = "true",
                ["target"] = EdgeId
            },
            evidenceIds ?? ["evidence-b", "Evidence-B", "evidence-a"],
            []);

    private static LogicalFlowResult MalformedFlowResult(string scenario)
    {
        var value = ValidFlowResult();
        switch (scenario)
        {
            case "flow-null": return value with { FlowId = null! };
            case "flow-leading": return value with { FlowId = " target" };
            case "scenario-null": return value with { ScenarioId = null! };
            case "verdict-surrogate": return value with { OverallVerdict = "reject\ud800" };
            case "human-empty": return value with { FinalHumanResult = " " };
            case "summary-null": return value with { Summary = null! };
            case "summary-nan":
                return value with
                {
                    Summary = value.Summary with { BaselinePrimary = double.NaN }
                };
            case "metrics-null": return value with { Metrics = null! };
            case "evidence-empty": return value with { EvidenceIds = [] };
            case "evidence-duplicate":
                return value with
                {
                    EvidenceIds = ["duplicate", "duplicate"]
                };
            case "evidence-leading": return value with { EvidenceIds = [" evidence"] };
            case "evidence-surrogate": return value with { EvidenceIds = ["evidence\ud800"] };
            case "warnings-null": return value with { Warnings = null! };
            case "signals-null": return value with { TransitionSignals = null! };
            case "signal-invalid":
                return value with
                {
                    TransitionSignals =
                [
                    new PhaseTransitionSignal(
                        Domain.CODE_Q,
                        "transition",
                        " node",
                        "subphase",
                        ["signal-evidence"])
                ]
                };
            case "handoff-run-leading":
                return value with { EdgeTargetHandoff = ValidHandoff(runId: " run") };
            case "handoff-kind-undefined":
                return value with
                {
                    EdgeTargetHandoff = ValidHandoff(kind: (GraphEdgeKind)int.MaxValue)
                };
            case "handoff-target-duplicate":
                return value with
                {
                    EdgeTargetHandoff = ValidHandoff(
                        orderedTargets:
                        [
                            new DynamicGraphEdgeTargetSelection("target", 0.9d, 0),
                            new DynamicGraphEdgeTargetSelection("target", 0.8d, 1)
                        ])
                };
            case "handoff-target-nan":
                return value with
                {
                    EdgeTargetHandoff = ValidHandoff(
                        orderedTargets:
                        [new DynamicGraphEdgeTargetSelection("target", double.NaN, 0)])
                };
            case "handoff-target-ordinal":
                return value with
                {
                    EdgeTargetHandoff = ValidHandoff(
                        orderedTargets:
                        [new DynamicGraphEdgeTargetSelection("target", 0.9d, 1)])
                };
            case "handoff-refs-duplicate":
                return value with
                {
                    EdgeTargetHandoff = ValidHandoff(
                        upstreamCandidateRefs: ["candidate", "candidate"])
                };
            case "handoff-knowledge-cardinality":
                return value with
                {
                    EdgeTargetHandoff = ValidHandoff(knowledgeEdge: true)
                };
            case "nonfatal-traversal":
                return value with
                {
                    Metrics = new ThrowingDictionary<string, string>(
                    new InvalidOperationException("ordinary metrics traversal failure"))
                };
            default: throw new ArgumentOutOfRangeException(nameof(scenario));
        }
    }

    private static DynamicGraphEdgeTargetHandoff ValidHandoff(
        string runId = RunId,
        GraphEdgeKind kind = GraphEdgeKind.FanOut,
        IReadOnlyList<DynamicGraphEdgeTargetSelection>? orderedTargets = null,
        IReadOnlyList<string>? upstreamCandidateRefs = null,
        bool knowledgeEdge = false)
        => new(
            runId,
            "handoff-edge",
            "handoff-source",
            "handoff-domain",
            kind,
            ParallelEligible: false,
            orderedTargets
            ?? [new DynamicGraphEdgeTargetSelection("target", 0.9d, 0)],
            knowledgeEdge,
            "LEARNED_EDGE_AUTHORITY",
            upstreamCandidateRefs ?? ["candidate"],
            "upstream-hash",
            "projection-rule");

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

    private static DynamicGraphEdgeAuthorityNoveltyEvaluationRequest ValidNoveltyRequest()
    {
        var paired = Paired();
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

    private static DynamicGraphEdgeAuthorityAblationEvaluationResult Paired()
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
                EdgeId,
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

    private static void CorruptAssociationEvidence(
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement association,
        string scenario)
    {
        var selection = GetPrivateField(
            association,
            "_edgeAuthoritySelectionQualityEvidence");
        switch (scenario)
        {
            case "novelty":
                SetPrivateField(selection, "_noveltyEvaluation", null);
                break;
            case "training":
                SetPrivateField(
                    GetPrivateField(selection, "_noveltyEvaluation"),
                    "_trainingEvidence",
                    null);
                break;
            case "rows":
                SetPrivateField(
                    GetPrivateField(selection, "_noveltyEvaluation"),
                    "_rows",
                    null);
                break;
            case "ablation":
                SetPrivateField(selection, "_selectionAblation", null);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario));
        }
    }

    private static void CorruptAssociationBinding(
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement association,
        string scenario)
    {
        var target = GetPrivateField(association, "_target");
        switch (scenario)
        {
            case "cached-case":
                SetPrivateField(association, "_authorityEdgeId", EdgeId.ToUpperInvariant());
                break;
            case "cached-surrogate":
                SetPrivateField(association, "_authorityEdgeId", "edge\ud800");
                break;
            case "target-case":
                SetAutoProperty(target, nameof(DynamicGraphMeasurementTarget.TargetId),
                    EdgeId.ToUpperInvariant());
                break;
            case "target-surrogate":
                SetAutoProperty(target, nameof(DynamicGraphMeasurementTarget.TargetId), "edge\ud800");
                break;
            case "level":
                SetAutoProperty(target, nameof(DynamicGraphMeasurementTarget.Level),
                    DynamicGraphMeasurementTargetLevel.L2LogicalUnit);
                break;
            case "mechanism":
                SetAutoProperty(target, nameof(DynamicGraphMeasurementTarget.MechanismKind),
                    DynamicGraphMeasurementMechanismKind.Regex);
                break;
            case "deterministic":
                SetAutoProperty(target, nameof(DynamicGraphMeasurementTarget.DeterministicFinalAuthority),
                    true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario));
        }
    }

    private static object GetPrivateField(object source, string fieldName)
        => Assert.IsAssignableFrom<FieldInfo>(source.GetType().GetField(
                fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .GetValue(source)!;

    private static void SetPrivateField(object source, string fieldName, object? value)
        => Assert.IsAssignableFrom<FieldInfo>(source.GetType().GetField(
                fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .SetValue(source, value);

    private static void SetAutoProperty(object source, string propertyName, object? value)
        => Assert.IsAssignableFrom<FieldInfo>(source.GetType().GetField(
                $"<{propertyName}>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .SetValue(source, value);

    private static Exception FatalGraph(string scenario)
        => scenario switch
        {
            "direct" => new BadImageFormatException("direct fatal"),
            "wrapped" => new InvalidOperationException(
                "wrapped fatal",
                new BadImageFormatException("nested fatal")),
            "aggregate-not-first" => new AggregateException(
                "aggregate fatal",
                new InvalidOperationException("ordinary first"),
                new BadImageFormatException("non-first fatal")),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };

    private static (Exception Outer, Exception Cycle, FieldInfo InnerField) CyclicFatalGraph()
    {
        var innerField = Assert.Single(
            typeof(Exception).GetFields(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            field => field.FieldType == typeof(Exception));
        var cycle = new InvalidOperationException("cycle");
        innerField.SetValue(cycle, cycle);
        Assert.Same(cycle, cycle.InnerException);
        var outer = new AggregateException(
            "cyclic graph with fatal node",
            new BadImageFormatException("fatal visited after cycle"),
            cycle);
        return (outer, cycle, innerField);
    }

    private static (Exception Outer, Exception Cycle, FieldInfo InnerField)
        CyclicCancellationGraph()
    {
        var innerField = Assert.Single(
            typeof(Exception).GetFields(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            field => field.FieldType == typeof(Exception));
        var cycle = new InvalidOperationException("cancellation cycle");
        innerField.SetValue(cycle, cycle);
        Assert.Same(cycle, cycle.InnerException);
        var outer = new AggregateException(
            "cyclic graph with cancellation node",
            new OperationCanceledException("cancellation visited after cycle"),
            cycle);
        return (outer, cycle, innerField);
    }

    private static async Task<Exception?> CaptureExceptionAsync(Func<Task> action)
    {
        try
        {
            await action();
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    private static void AssertCancelled(
        OperationResult<DynamicGraphL3SelectedTargetExecutionResult> result)
        => AssertFailure(result, "CANCELLED", "Cancelled.");

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

        internal RecordingFlow(
            Func<
                LogicalFlowRequest<string>,
                CancellationToken,
                Task<OperationResult<LogicalFlowResult>>> execute)
        {
            _execute = execute;
        }

        internal int CallCount => Volatile.Read(ref _callCount);

        internal LogicalFlowRequest<string>? SeenRequest { get; private set; }

        internal CancellationToken SeenToken { get; private set; }

        public Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
            LogicalFlowRequest<string> request,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _callCount);
            SeenRequest = request;
            SeenToken = cancellationToken;
            return _execute(request, cancellationToken);
        }

        internal static RecordingFlow Returning(OperationResult<LogicalFlowResult> result)
            => new((_, _) => Task.FromResult(result));

        internal static RecordingFlow Returning(LogicalFlowResult result)
            => Returning(OperationResult<LogicalFlowResult>.Ok(result));
    }

    private sealed class IntFlow : ILogicalFlow<int>
    {
        public Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
            LogicalFlowRequest<int> request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<LogicalFlowResult>.Ok(ValidFlowResult()));
    }

    private sealed class RecordingFlowFactory : ILogicalFlowFactory<string>
    {
        private readonly Func<
            LogicalFlowFactoryRequest,
            OperationResult<ILogicalFlow<string>>> _create;
        private int _callCount;

        internal RecordingFlowFactory(
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

        internal static RecordingFlowFactory Returning(ILogicalFlow<string> flow)
            => new(_ => OperationResult<ILogicalFlow<string>>.Ok(flow));
    }

    private sealed class FailingIfCreatedFactory : ILogicalFlowFactory<string>
    {
        private int _createCount;

        internal int CreateCount => Volatile.Read(ref _createCount);

        public OperationResult<ILogicalFlow<string>> Create(LogicalFlowFactoryRequest request)
        {
            Interlocked.Increment(ref _createCount);
            throw new InvalidOperationException("Factory must not be invoked by exact Resolve<string>.");
        }
    }

    private sealed class ThrowingDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
    {
        private readonly Exception _exception;

        internal ThrowingDictionary(Exception exception)
        {
            _exception = exception;
        }

        public int Count => 1;

        public IEnumerable<TKey> Keys => [];

        public IEnumerable<TValue> Values => [];

        public TValue this[TKey key] => throw _exception;

        public bool ContainsKey(TKey key) => throw _exception;

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default!;
            throw _exception;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => throw _exception;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class CancellingThrowingDictionary<TKey, TValue>
        : IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
    {
        private readonly CancellationTokenSource _cancellation;
        private readonly Exception _exception;

        internal CancellingThrowingDictionary(
            CancellationTokenSource cancellation,
            Exception exception)
        {
            _cancellation = cancellation;
            _exception = exception;
        }

        public int Count => 1;

        public IEnumerable<TKey> Keys => [];

        public IEnumerable<TValue> Values => [];

        public TValue this[TKey key] => throw CancelAndReturn();

        public bool ContainsKey(TKey key) => throw CancelAndReturn();

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default!;
            throw CancelAndReturn();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            => throw CancelAndReturn();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private Exception CancelAndReturn()
        {
            _cancellation.Cancel();
            return _exception;
        }
    }
}
