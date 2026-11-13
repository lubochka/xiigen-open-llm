// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Collections;
using System.Reflection;
using AgentInfrastructure.Logical;
using ExperimentsV2;
using ExperimentsV2.LogicalFlow;

namespace LlmFromScratch.Tests;

public sealed class LogicalFlowRegistryTests
{
    private const string FactoryFlowId = "registry-factory-flow";
    private const string FactoryScenarioId = "registry-factory-scenario";

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
    public void LogicalFlowRegistry_RegistersRouteMetadataAndResolvesBySubPhase()
    {
        // Create the registry for this test.
        var registry = new InMemoryLogicalFlowRegistry();

        // Register the security flow under its development lifecycle route.
        var registration = RegisterSecurity34Flow(registry, "1.0.0");

        // The registration must succeed through the shared envelope.
        Assert.True(registration.IsSuccess, registration.ErrorMessage);

        // The descriptor must expose route metadata for topology-aware flow selection.
        Assert.Equal("3", registration.Value!.PhaseId);
        Assert.Equal("3.4", registration.Value.SubPhaseId);
        Assert.Equal("security-verification", registration.Value.TaskType);
        Assert.Contains("security-and-authorization-model", registration.Value.DomainTags);

        // Resolve the flow by sub-phase id.
        var resolved = registry.ResolveBySubPhase<string>("3.4");

        // The registry must return the executable route flow.
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        Assert.IsType<SecurityVerification34FlowAdapter>(resolved.Value);
    }

    [Fact]
    public void LogicalFlowRegistry_SubPhaseBindingCanDriveInMemoryOrchestrator()
    {
        // Create the registry for this test.
        var registry = new InMemoryLogicalFlowRegistry();

        // Register the security flow under its development lifecycle route.
        RegisterSecurity34Flow(registry, "1.0.0");

        // Resolve a binding by sub-phase id.
        var binding = registry.ResolveBindingBySubPhase("3.4");

        // The binding must resolve through route metadata.
        Assert.True(binding.IsSuccess, binding.ErrorMessage);
        Assert.Equal(SecurityVerification34FlowAdapter.FlowId, binding.Value!.FlowId);
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

    [Fact]
    public void ResolveFactoryBindingByFlowId_ExposesExactPublicSurface()
    {
        var method = Assert.IsAssignableFrom<MethodInfo>(
            typeof(InMemoryLogicalFlowRegistry).GetMethod(
                nameof(InMemoryLogicalFlowRegistry.ResolveFactoryBindingByFlowId),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

        Assert.Equal(
            [typeof(string), typeof(string), typeof(LogicalFlowFactoryRequest)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            typeof(AgentInfrastructure.OperationResult<LogicalFlowBinding>),
            method.ReturnType);
        Assert.False(method.IsGenericMethod);
    }

    [Fact]
    public void ResolveFactoryBindingByFlowId_RequiresRequestBeforeAllOtherValidation()
    {
        var registry = new InMemoryLogicalFlowRegistry();

        var result = registry.ResolveFactoryBindingByFlowId(null, null, null);

        AssertFailure(
            result,
            "FLOW_FACTORY_REQUEST_REQUIRED",
            "Flow factory request is required.");
    }

    [Theory]
    [InlineData("flow-null")]
    [InlineData("flow-leading")]
    [InlineData("flow-surrogate")]
    [InlineData("scenario-null")]
    [InlineData("scenario-control")]
    [InlineData("run-leading")]
    [InlineData("run-surrogate")]
    public void ResolveFactoryBindingByFlowId_RejectsNoncanonicalCallerIdentityBeforeLookup(
        string scenario)
    {
        string? flowId = FactoryFlowId;
        string? expectedScenarioId = FactoryScenarioId;
        var runId = "registry-factory-run";
        switch (scenario)
        {
            case "flow-null": flowId = null; break;
            case "flow-leading": flowId = $" {FactoryFlowId}"; break;
            case "flow-surrogate": flowId = "flow\ud800"; break;
            case "scenario-null": expectedScenarioId = null; break;
            case "scenario-control": expectedScenarioId = "scenario\u0001"; break;
            case "run-leading": runId = " run"; break;
            case "run-surrogate": runId = "run\ud800"; break;
            default: throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        var factory = RecordingFactory.Returning(new RegistryTestFlow());
        var registry = RegistryWithFactory(factory);
        var result = registry.ResolveFactoryBindingByFlowId(
            flowId,
            expectedScenarioId,
            FactoryRequest(runId));

        AssertFailure(
            result,
            "FLOW_FACTORY_BINDING_IDENTITY_INVALID",
            "Flow factory binding identity is invalid.");
        Assert.Equal(0, factory.CallCount);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("case")]
    [InlineData("wrong-type")]
    public void ResolveFactoryBindingByFlowId_RequiresExactStringInputRegistration(
        string scenario)
    {
        var registry = new InMemoryLogicalFlowRegistry();
        switch (scenario)
        {
            case "missing": break;
            case "case":
                Assert.True(registry.RegisterFactory<string>(
                    FactoryFlowId.ToUpperInvariant(),
                    FactoryScenarioId,
                    "v1",
                    "phase",
                    "subphase",
                    "task",
                    [],
                    RecordingFactory.Returning(new RegistryTestFlow())).IsSuccess);
                break;
            case "wrong-type":
                Assert.True(registry.RegisterFactory<int>(
                    FactoryFlowId,
                    FactoryScenarioId,
                    "v1",
                    "phase",
                    "subphase",
                    "task",
                    [],
                    new IntFactory()).IsSuccess);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        var result = registry.ResolveFactoryBindingByFlowId(
            FactoryFlowId,
            FactoryScenarioId,
            FactoryRequest());

        AssertFailure(
            result,
            "FLOW_FACTORY_REGISTRATION_NOT_FOUND",
            "No exact string-input logical flow factory registration was found.");
    }

    [Theory]
    [InlineData("scenario-only")]
    [InlineData("subphase-only")]
    public void ResolveFactoryBindingByFlowId_DoesNotRouteThroughScenarioOrSubPhase(
        string scenario)
    {
        var factory = RecordingFactory.Returning(new RegistryTestFlow());
        var registry = new InMemoryLogicalFlowRegistry();
        var registered = registry.RegisterFactory<string>(
            "different-flow",
            scenario == "scenario-only" ? FactoryScenarioId : "different-scenario",
            "v1",
            "phase",
            scenario == "subphase-only" ? FactoryFlowId : "different-subphase",
            "task",
            [],
            factory);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        var result = registry.ResolveFactoryBindingByFlowId(
            FactoryFlowId,
            FactoryScenarioId,
            FactoryRequest());

        AssertFailure(
            result,
            "FLOW_FACTORY_REGISTRATION_NOT_FOUND",
            "No exact string-input logical flow factory registration was found.");
        Assert.Equal(0, factory.CallCount);
    }

    [Theory]
    [InlineData("scenario-leading")]
    [InlineData("scenario-surrogate")]
    public void ResolveFactoryBindingByFlowId_RejectsMalformedStoredDescriptorBeforeFactory(
        string scenario)
    {
        var factory = RecordingFactory.Returning(new RegistryTestFlow());
        var registry = RegistryWithFactory(factory);
        var descriptor = GetStoredDescriptor(registry);
        SetAutoProperty(
            descriptor,
            nameof(LogicalFlowDescriptor.ScenarioId),
            scenario == "scenario-leading" ? " scenario" : "scenario\ud800");

        var result = registry.ResolveFactoryBindingByFlowId(
            FactoryFlowId,
            FactoryScenarioId,
            FactoryRequest());

        AssertFailure(
            result,
            "INCOMPATIBLE_FLOW_REGISTRATION",
            "Registered flow descriptor has incompatible binding identity.");
        Assert.Equal(0, factory.CallCount);
    }

    [Theory]
    [InlineData("different")]
    [InlineData("case")]
    public void ResolveFactoryBindingByFlowId_RequiresOrdinalScenarioIdentity(string scenario)
    {
        var factory = RecordingFactory.Returning(new RegistryTestFlow());
        var registry = RegistryWithFactory(factory);
        var expected = scenario == "case"
            ? FactoryScenarioId.ToUpperInvariant()
            : "different-scenario";

        var result = registry.ResolveFactoryBindingByFlowId(
            FactoryFlowId,
            expected,
            FactoryRequest());

        AssertFailure(
            result,
            "FLOW_FACTORY_BINDING_SCENARIO_MISMATCH",
            "Registered flow factory scenario does not match the requested scenario.");
        Assert.Equal(0, factory.CallCount);
    }

    [Fact]
    public void ResolveFactoryBindingByFlowId_NewestDirectBlocksOlderFactoryWithoutFallback()
    {
        var older = RecordingFactory.Returning(new RegistryTestFlow());
        var registry = RegistryWithFactory(older, version: "v1");
        Assert.True(registry.Register<string>(
            FactoryFlowId,
            FactoryScenarioId,
            "v2",
            new RegistryTestFlow()).IsSuccess);

        var result = registry.ResolveFactoryBindingByFlowId(
            FactoryFlowId,
            FactoryScenarioId,
            FactoryRequest());

        AssertFailure(
            result,
            "FLOW_FACTORY_REGISTRATION_REQUIRED",
            "Newest exact string-input registration is not a logical flow factory.");
        Assert.Equal(0, older.CallCount);
    }

    [Fact]
    public void ResolveFactoryBindingByFlowId_NewestExactFactoryWinsAndReceivesExactRequestOnce()
    {
        var older = RecordingFactory.Returning(new RegistryTestFlow());
        var newerFlow = new RegistryTestFlow();
        var newer = RecordingFactory.Returning(newerFlow);
        var registry = RegistryWithFactory(older, version: "v1");
        Assert.True(registry.RegisterFactory<string>(
            FactoryFlowId,
            FactoryScenarioId,
            "v2",
            "phase",
            "subphase",
            "task",
            [],
            newer).IsSuccess);
        var request = FactoryRequest();

        var result = registry.ResolveFactoryBindingByFlowId(
            FactoryFlowId,
            FactoryScenarioId,
            request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, older.CallCount);
        Assert.Equal(1, newer.CallCount);
        Assert.Same(request, newer.SeenRequest);
        Assert.Same(newerFlow, result.Value!.Flow);
        Assert.Equal(FactoryFlowId, result.Value.FlowId, StringComparer.Ordinal);
        Assert.Equal(FactoryScenarioId, result.Value.ScenarioId, StringComparer.Ordinal);
    }

    [Fact]
    public void ResolveFactoryBindingByFlowId_WrongInputTypeDoesNotShadowStringFactory()
    {
        var selected = RecordingFactory.Returning(new RegistryTestFlow());
        var registry = RegistryWithFactory(selected, version: "v1");
        Assert.True(registry.RegisterFactory<int>(
            FactoryFlowId,
            FactoryScenarioId,
            "v2",
            "phase",
            "subphase",
            "task",
            [],
            new IntFactory()).IsSuccess);

        var result = registry.ResolveFactoryBindingByFlowId(
            FactoryFlowId,
            FactoryScenarioId,
            FactoryRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, selected.CallCount);
    }

    [Theory]
    [InlineData("null-envelope")]
    [InlineData("null-value")]
    public void ResolveFactoryBindingByFlowId_RequiresFactoryResult(string scenario)
    {
        var factory = new RecordingFactory(_ => scenario == "null-envelope"
            ? null!
            : AgentInfrastructure.OperationResult<ILogicalFlow<string>>.Ok(null!));
        var registry = RegistryWithFactory(factory);

        var result = registry.ResolveFactoryBindingByFlowId(
            FactoryFlowId,
            FactoryScenarioId,
            FactoryRequest());

        AssertFailure(
            result,
            "FLOW_FACTORY_RESULT_REQUIRED",
            "Flow factory result is required.");
        Assert.Equal(1, factory.CallCount);
    }

    [Fact]
    public void ResolveFactoryBindingByFlowId_PropagatesDeclaredFailureWithoutFallback()
    {
        var older = RecordingFactory.Returning(new RegistryTestFlow());
        var newer = new RecordingFactory(_ =>
            AgentInfrastructure.OperationResult<ILogicalFlow<string>>.Fail(
                "Declared factory failure.",
                "DECLARED_FACTORY_FAILURE"));
        var registry = RegistryWithFactory(older, version: "v1");
        Assert.True(registry.RegisterFactory<string>(
            FactoryFlowId,
            FactoryScenarioId,
            "v2",
            "phase",
            "subphase",
            "task",
            [],
            newer).IsSuccess);

        var result = registry.ResolveFactoryBindingByFlowId(
            FactoryFlowId,
            FactoryScenarioId,
            FactoryRequest());

        AssertFailure(result, "DECLARED_FACTORY_FAILURE", "Declared factory failure.");
        Assert.Equal(0, older.CallCount);
        Assert.Equal(1, newer.CallCount);
    }

    [Theory]
    [InlineData("ordinary")]
    [InlineData("cancellation")]
    [InlineData("fatal")]
    public void ResolveFactoryBindingByFlowId_DoesNotCatchFactoryExceptions(string scenario)
    {
        Exception exception = scenario switch
        {
            "ordinary" => new InvalidOperationException("ordinary factory failure"),
            "cancellation" => new OperationCanceledException("factory cancellation"),
            "fatal" => new BadImageFormatException("fatal factory failure"),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
        var factory = new RecordingFactory(_ => throw exception);
        var registry = RegistryWithFactory(factory);

        var thrown = Assert.Throws(exception.GetType(), () =>
            registry.ResolveFactoryBindingByFlowId(
                FactoryFlowId,
                FactoryScenarioId,
                FactoryRequest()));

        Assert.Same(exception, thrown);
        Assert.Equal(1, factory.CallCount);
    }

    private static InMemoryLogicalFlowRegistry RegistryWithFactory(
        ILogicalFlowFactory<string> factory,
        string version = "v1")
    {
        var registry = new InMemoryLogicalFlowRegistry();
        var registered = registry.RegisterFactory<string>(
            FactoryFlowId,
            FactoryScenarioId,
            version,
            "phase",
            "subphase",
            "task",
            [],
            factory);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        return registry;
    }

    [Fact]
    public void LogicalFlowRegistry_FatalBeforeCompensatableMutationLeavesNoEntry()
    {
        var registry = new InMemoryLogicalFlowRegistry();
        var prior = InMemoryLogicalFlowRegistry
            .BeforeCompensatableMutationForTesting;
        try
        {
            InMemoryLogicalFlowRegistry.BeforeCompensatableMutationForTesting =
                static () => throw new OutOfMemoryException(
                    "injected pre-mutation fatal failure");

            Assert.Throws<OutOfMemoryException>(() =>
                registry.RegisterFactoryCompensatable(
                    "fatal-flow",
                    "fatal-scenario",
                    "1.0.0",
                    "phase",
                    "subphase",
                    "task",
                    [],
                    RecordingFactory.Returning(new RegistryTestFlow())));

            Assert.Empty(registry.ListDescriptors());
        }
        finally
        {
            InMemoryLogicalFlowRegistry.BeforeCompensatableMutationForTesting =
                prior;
        }
    }

    [Fact]
    public void LogicalFlowRegistry_PublicationLeaseCanMoveThreadsAndPreservesTimeouts()
    {
        var registry = new InMemoryLogicalFlowRegistry();
        var lease = registry.AcquirePublicationLease();

        var nested = registry.TryAcquirePublicationLease(TimeSpan.Zero);
        Assert.True(nested.IsSuccess, nested.ErrorMessage);
        nested.Value!.Dispose();

        AgentInfrastructure.OperationResult<IDisposable>? timedOut = null;
        RunOnDedicatedThread(() =>
            timedOut = registry.TryAcquirePublicationLease(
                TimeSpan.FromMilliseconds(25)));
        Assert.NotNull(timedOut);
        Assert.False(timedOut.IsSuccess);
        Assert.Equal(
            "LOGICAL_FLOW_REGISTRY_PUBLICATION_LEASE_TIMEOUT",
            timedOut.ErrorCode);

        RunOnDedicatedThread(lease.Dispose);

        using var reacquired = registry.AcquirePublicationLease();
        var registered = registry.Register(
            "cross-thread-flow",
            "cross-thread-scenario",
            "1.0.0",
            new RegistryTestFlow());
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        Assert.Single(registry.ListDescriptors());
    }

    [Fact]
    public void RegistryPublicationLease_UsesInternalUnitThenFlowProtocol()
    {
        var units = new InMemoryLogicalUnitRegistry();
        var flows = new InMemoryLogicalFlowRegistry();

        Assert.Null(typeof(InMemoryLogicalUnitRegistry).GetMethod(
            "AcquirePublicationLease",
            BindingFlags.Instance | BindingFlags.Public));
        Assert.Null(typeof(InMemoryLogicalUnitRegistry).GetMethod(
            "TryAcquirePublicationLease",
            BindingFlags.Instance | BindingFlags.Public));
        Assert.Null(typeof(InMemoryLogicalFlowRegistry).GetMethod(
            "AcquirePublicationLease",
            BindingFlags.Instance | BindingFlags.Public));
        Assert.Null(typeof(InMemoryLogicalFlowRegistry).GetMethod(
            "TryAcquirePublicationLease",
            BindingFlags.Instance | BindingFlags.Public));

        var lease = LogicalRegistryPublicationLease.TryAcquireUnitThenFlow(
            units,
            flows,
            TimeSpan.FromSeconds(1));
        Assert.True(lease.IsSuccess, lease.ErrorMessage);
        using var publicationLease = lease.Value!;

        var nestedLease = LogicalRegistryPublicationLease.TryAcquireUnitThenFlow(
            units,
            flows,
            TimeSpan.Zero);
        Assert.True(nestedLease.IsSuccess, nestedLease.ErrorMessage);
        using var nestedPublicationLease = nestedLease.Value!;

        var unit = units.Register(
            T34TransitionDomainSplitterUnit.Id,
            T34TransitionDomainSplitterUnit.Role,
            T34TransitionDomainSplitterUnit.DomainTag,
            "1.0.0",
            LogicalUnitTrustState.Trusted,
            isLearned: false,
            new T34TransitionDomainSplitterUnit());
        var flow = flows.Register(
            "nested-gate-flow",
            "nested-gate-scenario",
            "1.0.0",
            new RegistryTestFlow());

        Assert.True(unit.IsSuccess, unit.ErrorMessage);
        Assert.True(flow.IsSuccess, flow.ErrorMessage);
        Assert.Single(units.ListDescriptors());
        Assert.Single(flows.ListDescriptors());
    }

    [Fact]
    public void RegistryPublicationLease_PublicSurfaceExposesOnlyCompositeLeaseAndOrderPreview()
    {
        var leaseType = typeof(LogicalRegistryPublicationLease);

        Assert.True(leaseType.IsPublic);
        Assert.True(leaseType.IsSealed);
        Assert.Empty(leaseType.GetConstructors(
            BindingFlags.Public | BindingFlags.Instance));
        Assert.Null(typeof(InMemoryLogicalUnitRegistry).GetMethod(
            "TryAcquirePublicationLease",
            BindingFlags.Instance | BindingFlags.Public));
        Assert.Null(typeof(InMemoryLogicalUnitRegistry).GetMethod(
            "PeekNextRegistrationOrderForPublication",
            BindingFlags.Instance | BindingFlags.Public));
        Assert.Null(typeof(InMemoryLogicalFlowRegistry).GetMethod(
            "TryAcquirePublicationLease",
            BindingFlags.Instance | BindingFlags.Public));
        Assert.Null(typeof(InMemoryLogicalFlowRegistry).GetMethod(
            "PeekNextRegistrationOrderForPublication",
            BindingFlags.Instance | BindingFlags.Public));

        var staticMethods = leaseType.GetMethods(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly)
            .Where(static method => !method.IsSpecialName)
            .ToArray();
        var acquire = Assert.Single(staticMethods);
        Assert.Equal("TryAcquireUnitThenFlow", acquire.Name);
        Assert.Equal(
            typeof(AgentInfrastructure.OperationResult<
                LogicalRegistryPublicationLease>),
            acquire.ReturnType);
        Assert.Equal(
            [
                typeof(InMemoryLogicalUnitRegistry),
                typeof(InMemoryLogicalFlowRegistry),
                typeof(TimeSpan)
            ],
            acquire.GetParameters()
                .Select(static parameter => parameter.ParameterType)
                .ToArray());

        var instanceMethodNames = leaseType.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly)
            .Where(static method => !method.IsSpecialName)
            .Select(static method => method.Name)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(["Dispose", "PeekNextRegistrationOrders"], instanceMethodNames);

        var units = new InMemoryLogicalUnitRegistry();
        var flows = new InMemoryLogicalFlowRegistry();
        var lease = LogicalRegistryPublicationLease.TryAcquireUnitThenFlow(
            units,
            flows,
            TimeSpan.FromSeconds(1));
        Assert.True(lease.IsSuccess, lease.ErrorMessage);

        var preview = lease.Value!.PeekNextRegistrationOrders();
        Assert.True(preview.IsSuccess, preview.ErrorMessage);
        Assert.Equal(1, preview.Value.NextUnitRegistrationOrder);
        Assert.Equal(1, preview.Value.NextFlowRegistrationOrder);

        var unit = units.Register(
            "public-composite-lease-unit",
            "public-composite-lease-role",
            "public-composite-lease-domain",
            "1.0.0",
            LogicalUnitTrustState.Trusted,
            isLearned: false,
            new T34TransitionDomainSplitterUnit());
        var flow = flows.Register(
            "public-composite-lease-flow",
            "public-composite-lease-scenario",
            "1.0.0",
            new RegistryTestFlow());
        Assert.True(unit.IsSuccess, unit.ErrorMessage);
        Assert.True(flow.IsSuccess, flow.ErrorMessage);

        var postRegistrationPreview = lease.Value.PeekNextRegistrationOrders();
        Assert.True(
            postRegistrationPreview.IsSuccess,
            postRegistrationPreview.ErrorMessage);
        Assert.Equal(2, postRegistrationPreview.Value.NextUnitRegistrationOrder);
        Assert.Equal(2, postRegistrationPreview.Value.NextFlowRegistrationOrder);

        lease.Value.Dispose();
        var disposedPreview = lease.Value.PeekNextRegistrationOrders();
        Assert.False(disposedPreview.IsSuccess);
        Assert.Equal(
            "LOGICAL_REGISTRY_PUBLICATION_LEASE_INACTIVE",
            disposedPreview.ErrorCode);
    }

    private static void RunOnDedicatedThread(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(5)));
        if (failure is not null)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo
                .Capture(failure)
                .Throw();
    }

    private static LogicalFlowFactoryRequest FactoryRequest(
        string runId = "registry-factory-run")
        => new(
            runId,
            new LogicalExecutionFlags(),
            new InMemoryLogicalUnitRegistry(),
            new Dictionary<string, object>());

    private static LogicalFlowDescriptor GetStoredDescriptor(
        InMemoryLogicalFlowRegistry registry)
    {
        var field = Assert.IsAssignableFrom<FieldInfo>(
            typeof(InMemoryLogicalFlowRegistry).GetField(
                "_entries",
                BindingFlags.NonPublic | BindingFlags.Instance));
        var entries = Assert.IsAssignableFrom<IList>(field.GetValue(registry));
        var tuple = Assert.IsAssignableFrom<object>(Assert.Single(entries.Cast<object>()));
        var descriptorField = Assert.IsAssignableFrom<FieldInfo>(
            tuple.GetType().GetField("Item1", BindingFlags.Public | BindingFlags.Instance));
        return Assert.IsType<LogicalFlowDescriptor>(descriptorField.GetValue(tuple));
    }

    private static void SetAutoProperty(object source, string propertyName, object? value)
        => Assert.IsAssignableFrom<FieldInfo>(source.GetType().GetField(
                $"<{propertyName}>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .SetValue(source, value);

    private static void AssertFailure<T>(
        AgentInfrastructure.OperationResult<T> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode, StringComparer.Ordinal);
        Assert.Equal(expectedMessage, result.ErrorMessage, StringComparer.Ordinal);
    }

    private static AgentInfrastructure.OperationResult<LogicalFlowDescriptor> RegisterT34Flow(InMemoryLogicalFlowRegistry registry, string version)
    {
        // Register the T34 flow under its scenario id.
        return registry.Register(
            T34LogicalFlowAdapter.FlowId,
            T34LogicalFlowAdapter.ScenarioId,
            version,
            new T34LogicalFlowAdapter());
    }

    private static AgentInfrastructure.OperationResult<LogicalFlowDescriptor> RegisterSecurity34Flow(InMemoryLogicalFlowRegistry registry, string version)
    {
        // Register the security verification flow under the route it teaches.
        return registry.Register(
            SecurityVerification34FlowAdapter.FlowId,
            SecurityVerification34FlowAdapter.ScenarioId,
            version,
            "3",
            "3.4",
            "security-verification",
            ["security-and-authorization-model"],
            new SecurityVerification34FlowAdapter());
    }

    private sealed class RegistryTestFlow : ILogicalFlow<string>
    {
        public Task<AgentInfrastructure.OperationResult<LogicalFlowResult>> ExecuteAsync(
            LogicalFlowRequest<string> request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Registry test flow execution is not expected.",
                "REGISTRY_TEST_FLOW_NOT_EXECUTED"));
    }

    private sealed class RecordingFactory : ILogicalFlowFactory<string>
    {
        private readonly Func<
            LogicalFlowFactoryRequest,
            AgentInfrastructure.OperationResult<ILogicalFlow<string>>> _create;
        private int _callCount;

        internal RecordingFactory(
            Func<
                LogicalFlowFactoryRequest,
                AgentInfrastructure.OperationResult<ILogicalFlow<string>>> create)
        {
            _create = create;
        }

        internal int CallCount => Volatile.Read(ref _callCount);

        internal LogicalFlowFactoryRequest? SeenRequest { get; private set; }

        public AgentInfrastructure.OperationResult<ILogicalFlow<string>> Create(
            LogicalFlowFactoryRequest request)
        {
            Interlocked.Increment(ref _callCount);
            SeenRequest = request;
            return _create(request);
        }

        internal static RecordingFactory Returning(ILogicalFlow<string> flow)
            => new(_ => AgentInfrastructure.OperationResult<ILogicalFlow<string>>.Ok(flow));
    }

    private sealed class IntFactory : ILogicalFlowFactory<int>
    {
        public AgentInfrastructure.OperationResult<ILogicalFlow<int>> Create(
            LogicalFlowFactoryRequest request)
            => AgentInfrastructure.OperationResult<ILogicalFlow<int>>.Ok(new IntFlow());
    }

    private sealed class IntFlow : ILogicalFlow<int>
    {
        public Task<AgentInfrastructure.OperationResult<LogicalFlowResult>> ExecuteAsync(
            LogicalFlowRequest<int> request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Integer registry test flow execution is not expected.",
                "REGISTRY_INT_TEST_FLOW_NOT_EXECUTED"));
    }
}
