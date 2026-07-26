// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel.Logical;

using System.Collections.ObjectModel;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;
using ExperimentsV2.LogicalFlow;
using ProjectWikiDomain;
using SemanticCompilerAdapter;
using TranslationDomain; // TranslationRoutingFlow — Direction 1 of parallel training (A-LM-04)

// This record is the runtime-facing answer returned by the logical orchestrator assistant.
public sealed record LogicalAssistantRunResult(
    string ScenarioId,
    string SelectedFlowId,
    string OverallVerdict,
    string FinalHumanResult,
    LogicalExecutionFlags AppliedFlags,
    IReadOnlyDictionary<string, string> Metrics,
    string[] EvidenceIds);

// This service is the first production-adjacent wrapper around the logical orchestrator.
public sealed class LogicalOrchestratorAssistant : BaseService
{
    private const string EdgeAuthorityPreloadRequiredCode =
        "DYNAMIC_GRAPH_EDGE_AUTHORITY_PRELOAD_REQUIRED";
    private const string EdgeAuthorityPreloadSnapshotFailedCode =
        "DYNAMIC_GRAPH_EDGE_AUTHORITY_PRELOAD_SNAPSHOT_FAILED";
    private const string EdgeAuthorityPreloadInvalidCode =
        "DYNAMIC_GRAPH_EDGE_AUTHORITY_PRELOAD_INVALID";
    private const string EdgeAuthorityPreloadLoadFailedCode =
        "DYNAMIC_GRAPH_EDGE_AUTHORITY_PRELOAD_LOAD_FAILED";
    private const string DynamicGraphTopologySourceFactoryRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_SOURCE_FACTORY_REQUIRED";
    private const string TopologyFlowExecutionFailedCode = "TOPOLOGY_FLOW_EXECUTION_FAILED";
    private const string TopologyFlowExecutionFailedMessage = "Topology mutation flow execution failed.";
    private const string TopologyFlowResultRequiredCode = "TOPOLOGY_FLOW_RESULT_REQUIRED";
    private const string TopologyFlowResultRequiredMessage = "Topology mutation flow result is required.";
    private const string EdgeAuthorityRole = "edge-authority";

    // This version labels the first experiment-backed flow registration.
    public const string DefaultFlowVersion = "1.0.0";

    // This source value marks assistant-created logical runs in metadata.
    public const string RunSource = "logical_orchestrator_assistant";

    // The orchestrator is nullable so invalid construction can fail through RunAsync instead of throwing.
    private readonly ILogicalOrchestrator? _orchestrator;

    // The topology mutation flow is nullable so command routing can fail without hardcoded fallbacks.
    private readonly ILogicalFlow<LogicalTopologyCommand>? _topologyMutationFlow;

    // The default scenario keeps the current assistant bound to the proven T34 transition flow.
    public string DefaultScenarioId { get; }

    // Create an assistant from an already-built logical orchestrator.
    public LogicalOrchestratorAssistant(
        ISessionStorage storage,
        ILogicalOrchestrator? orchestrator,
        string defaultScenarioId = T34LogicalFlowAdapter.ScenarioId,
        ILogicalFlow<LogicalTopologyCommand>? topologyMutationFlow = null)
        : base(storage)
    {
        // Store the orchestrator so RunAsync can execute the selected logical flow.
        _orchestrator = orchestrator;

        // Store the topology mutation flow for typed topology commands.
        _topologyMutationFlow = topologyMutationFlow;

        // Fall back to the proven T34 scenario when the caller gives no useful default.
        DefaultScenarioId = string.IsNullOrWhiteSpace(defaultScenarioId)
            ? T34LogicalFlowAdapter.ScenarioId
            : defaultScenarioId;
    }

    // Return the already-configured orchestrator identity without exposing mutation or lifecycle authority.
    internal OperationResult<ILogicalOrchestrator> ReadConfiguredOrchestrator()
        => _orchestrator is null
            ? OperationResult<ILogicalOrchestrator>.Fail(
                "Logical orchestrator is not configured.",
                "ORCHESTRATOR_NOT_CONFIGURED")
            : OperationResult<ILogicalOrchestrator>.Ok(_orchestrator);

    // Build the default assistant with the T34 and CB4 logical flows registered in memory.
    public static OperationResult<LogicalOrchestratorAssistant> CreateDefault(
        ISessionStorage storage,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter = null,
        IDynamicGraphTopologySource? dynamicGraphTopologySource = null)
    {
        Func<InMemoryLogicalTopologyRegistry, OperationResult<IDynamicGraphTopologySource>>? sourceFactory =
            dynamicGraphTopologySource is null
                ? null
                : _ => OperationResult<IDynamicGraphTopologySource>.Ok(dynamicGraphTopologySource);

        return CreateDefaultCore(storage, projectWikiTrainingFlowAdapter, sourceFactory);
    }

    // Build the default assistant while allowing a topology source to be created from the fully seeded registry.
    internal static OperationResult<LogicalOrchestratorAssistant> CreateDefaultCore(
        ISessionStorage storage,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter = null,
        Func<InMemoryLogicalTopologyRegistry, OperationResult<IDynamicGraphTopologySource>>? dynamicGraphTopologySourceFactory = null)
        => CreateDefaultCoreImplementation(
            storage,
            projectWikiTrainingFlowAdapter,
            dynamicGraphTopologySourceFactory,
            null,
            null);

    // Build the same default assistant with one explicit invocation-scoped node-output source.
    internal static OperationResult<LogicalOrchestratorAssistant> CreateDefaultCore(
        ISessionStorage storage,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter,
        Func<InMemoryLogicalTopologyRegistry, OperationResult<IDynamicGraphTopologySource>>? dynamicGraphTopologySourceFactory,
        IDynamicGraphNodeOutputSource dynamicGraphNodeOutputSource)
    {
        if (dynamicGraphNodeOutputSource is null)
        {
            return OperationResult<LogicalOrchestratorAssistant>.Fail(
                "Dynamic graph node-output source is required.",
                "DYNAMIC_GRAPH_NODE_OUTPUT_INVOCATION_SOURCE_REQUIRED");
        }

        return CreateDefaultCoreImplementation(
            storage,
            projectWikiTrainingFlowAdapter,
            dynamicGraphTopologySourceFactory,
            dynamicGraphNodeOutputSource,
            null);
    }

    // Build the same assistant with trusted edge authorities while retaining invocation-scoped node outputs.
    internal static OperationResult<LogicalOrchestratorAssistant> CreateDefaultCore(
        ISessionStorage storage,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter,
        Func<InMemoryLogicalTopologyRegistry, OperationResult<IDynamicGraphTopologySource>> dynamicGraphTopologySourceFactory,
        IReadOnlyList<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>> edgeAuthorityPreload)
    {
        if (dynamicGraphTopologySourceFactory is null)
        {
            return OperationResult<LogicalOrchestratorAssistant>.Fail(
                "Dynamic graph topology source factory is required.",
                DynamicGraphTopologySourceFactoryRequiredCode);
        }

        var preloadSnapshot = SnapshotEdgeAuthorityPreload(edgeAuthorityPreload);
        if (!preloadSnapshot.IsSuccess)
            return preloadSnapshot.MapError<LogicalOrchestratorAssistant>();

        return CreateDefaultCoreImplementation(
            storage,
            projectWikiTrainingFlowAdapter,
            dynamicGraphTopologySourceFactory,
            null,
            preloadSnapshot.Value!);
    }

    // Build the same assistant with a trusted host's already-loaded learned edge-authority units.
    internal static OperationResult<LogicalOrchestratorAssistant> CreateDefaultCore(
        ISessionStorage storage,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter,
        Func<InMemoryLogicalTopologyRegistry, OperationResult<IDynamicGraphTopologySource>> dynamicGraphTopologySourceFactory,
        IDynamicGraphNodeOutputSource dynamicGraphNodeOutputSource,
        IReadOnlyList<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>> edgeAuthorityPreload)
    {
        if (dynamicGraphTopologySourceFactory is null)
        {
            return OperationResult<LogicalOrchestratorAssistant>.Fail(
                "Dynamic graph topology source factory is required.",
                DynamicGraphTopologySourceFactoryRequiredCode);
        }

        if (dynamicGraphNodeOutputSource is null)
        {
            return OperationResult<LogicalOrchestratorAssistant>.Fail(
                "Dynamic graph node-output source is required.",
                "DYNAMIC_GRAPH_NODE_OUTPUT_INVOCATION_SOURCE_REQUIRED");
        }

        var preloadSnapshot = SnapshotEdgeAuthorityPreload(edgeAuthorityPreload);
        if (!preloadSnapshot.IsSuccess)
            return preloadSnapshot.MapError<LogicalOrchestratorAssistant>();

        return CreateDefaultCoreImplementation(
            storage,
            projectWikiTrainingFlowAdapter,
            dynamicGraphTopologySourceFactory,
            dynamicGraphNodeOutputSource,
            preloadSnapshot.Value!);
    }

    private static OperationResult<LogicalOrchestratorAssistant> CreateDefaultCoreImplementation(
        ISessionStorage storage,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter,
        Func<InMemoryLogicalTopologyRegistry, OperationResult<IDynamicGraphTopologySource>>? dynamicGraphTopologySourceFactory,
        IDynamicGraphNodeOutputSource? dynamicGraphNodeOutputSource,
        ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>[]? edgeAuthorityPreload)
    {
        // Reject missing storage through the project result envelope.
        if (storage is null)
            return OperationResult<LogicalOrchestratorAssistant>.Fail("Session storage is required.", "STORAGE_REQUIRED");

        // Create the flow registry used by this first runtime-ready wrapper.
        var registry = new InMemoryLogicalFlowRegistry();

        // Register the T34 adapter as the default executable logical flow.
        var t34Registration = registry.Register(
            T34LogicalFlowAdapter.FlowId,
            T34LogicalFlowAdapter.ScenarioId,
            DefaultFlowVersion,
            new T34LogicalFlowAdapter());

        // Return registry failures through the project result envelope.
        if (!t34Registration.IsSuccess)
            return OperationResult<LogicalOrchestratorAssistant>.Fail(t34Registration.ErrorMessage, "FLOW_REGISTRATION_FAILED");

        // Register the CB4 security-verification flow as the first external-source Stage 5 route.
        var securityRegistration = registry.RegisterFactory(
            SecurityVerification34FlowAdapter.FlowId,
            SecurityVerification34FlowAdapter.ScenarioId,
            DefaultFlowVersion,
            "3",
            "3.4",
            "security-verification",
            ["security-and-authorization-model"],
            new SecurityVerification34FlowFactory());

        // Return registry failures through the project result envelope.
        if (!securityRegistration.IsSuccess)
            return OperationResult<LogicalOrchestratorAssistant>.Fail(securityRegistration.ErrorMessage, "FLOW_REGISTRATION_FAILED");

        // Create the runtime topology registry for assistant-driven topology mutations and adaptive routing.
        var topologyRegistry = new InMemoryLogicalTopologyRegistry();

        // Seed all seven development phases and all 31 sub-phases with executable nine-node flows.
        var lifecycleSeed = DevelopmentLifecycleTopologySeeder.Seed(topologyRegistry, registry, DefaultFlowVersion);

        // Return topology registration failures through the project result envelope.
        if (!lifecycleSeed.IsSuccess)
            return OperationResult<LogicalOrchestratorAssistant>.Fail(lifecycleSeed.ErrorMessage, "TOPOLOGY_REGISTRATION_FAILED");

        // Create the optional read-only dynamic-graph source only after the complete lifecycle registry is seeded.
        IDynamicGraphTopologySource? dynamicGraphTopologySource = null;
        if (dynamicGraphTopologySourceFactory is not null)
        {
            OperationResult<IDynamicGraphTopologySource>? sourceResult;
            try
            {
                // Invoke the caller-owned factory exactly once against the seeded registry snapshot.
                sourceResult = dynamicGraphTopologySourceFactory(topologyRegistry);
            }
            catch (Exception exception) when (IsNonFatalPreloadException(exception))
            {
                return OperationResult<LogicalOrchestratorAssistant>.Fail(
                    "Dynamic graph topology source creation failed.",
                    "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_SOURCE_FACTORY_EXCEPTION");
            }

            if (sourceResult is null || (sourceResult.IsSuccess && sourceResult.Value is null))
            {
                return OperationResult<LogicalOrchestratorAssistant>.Fail(
                    "Dynamic graph topology source result is required.",
                    "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_SOURCE_RESULT_REQUIRED");
            }

            if (!sourceResult.IsSuccess)
                return sourceResult.MapError<LogicalOrchestratorAssistant>();

            dynamicGraphTopologySource = sourceResult.Value;
        }

        var projectDocumentationRegistration = registry.Register(
            ProjectDocumentationPipelineFlowAdapter.FlowId,
            ProjectDocumentationPipelineFlowAdapter.ScenarioId,
            DefaultFlowVersion,
            "2",
            "2.1",
            "project-documentation-wiki",
            ["project-local-wiki", "documentation", "code-trace"],
            new ProjectDocumentationPipelineFlowAdapter());
        if (!projectDocumentationRegistration.IsSuccess)
            return OperationResult<LogicalOrchestratorAssistant>.Fail(projectDocumentationRegistration.ErrorMessage, "FLOW_REGISTRATION_FAILED");

        var projectWikiTrainingRegistration = registry.Register(
            ProjectWikiTrainingFlowAdapter.FlowId,
            ProjectWikiTrainingFlowAdapter.ScenarioId,
            DefaultFlowVersion,
            "7",
            "7.5",
            "project-wiki-training",
            ["project-local-wiki", "training", "learned-routing", "documentation"],
            projectWikiTrainingFlowAdapter ?? new ProjectWikiTrainingFlowAdapter());
        if (!projectWikiTrainingRegistration.IsSuccess)
            return OperationResult<LogicalOrchestratorAssistant>.Fail(projectWikiTrainingRegistration.ErrorMessage, "FLOW_REGISTRATION_FAILED");

        // Register the project-wiki-orchestration-flow that wraps the six Bootstrap units from TD1.
        // This flow receives SubmitUserTaskRequest data through Metadata and returns the full human route.
        var projectWikiOrchestrationRegistration = registry.Register(
            ProjectWikiOrchestrationFlow.FlowId,
            ProjectWikiOrchestrationFlow.ScenarioId,
            DefaultFlowVersion,
            new ProjectWikiOrchestrationFlow());
        if (!projectWikiOrchestrationRegistration.IsSuccess)
            return OperationResult<LogicalOrchestratorAssistant>.Fail(projectWikiOrchestrationRegistration.ErrorMessage, "FLOW_REGISTRATION_FAILED");

        // Register the translation-routing-flow for Direction 1 (27.TranslationDomain).
        // This flow routes TranslationRoutingRequest to the correct direction adapter
        // (RU_EN, EN_RU, HE_EN, or EN_HE) — governed by decision A-LM-04.
        var translationRoutingRegistration = registry.Register(
            TranslationRoutingFlow.FlowId,
            TranslationRoutingFlow.ScenarioId,
            DefaultFlowVersion,
            new TranslationRoutingFlow());
        if (!translationRoutingRegistration.IsSuccess)
            return OperationResult<LogicalOrchestratorAssistant>.Fail(translationRoutingRegistration.ErrorMessage, "FLOW_REGISTRATION_FAILED");

        // Register the design-conversion runtime flow through a string-input adapter.
        // The adapter reads only typed metadata and never treats prompt text as the domain payload.
        var designConversionRegistration = registry.Register(
            DesignConversionRoutingFlow.FlowId,
            DesignConversionRoutingFlow.ScenarioId,
            DefaultFlowVersion,
            new DesignConversionOrchestrationFlowAdapter());
        if (!designConversionRegistration.IsSuccess)
            return OperationResult<LogicalOrchestratorAssistant>.Fail(designConversionRegistration.ErrorMessage, "FLOW_REGISTRATION_FAILED");

        // Create the prompt compiler and router used by the adaptive orchestrator.
        var promptCompiler = new PromptRoutingInputCompilerUnit();
        var router = new SubPhaseRouterUnit(topologyRegistry);
        var multiRouter = new MultiSubPhaseRouterUnit(topologyRegistry, router);
        var unitRegistry = new InMemoryLogicalUnitRegistry();

        // Register deterministic prompt compiler and domain classifier as trusted baseline units.
        var compilerUnitRegistration = unitRegistry.Register(
            PromptRoutingInputCompilerUnit.Id,
            PromptRoutingInputCompilerUnit.Role,
            PromptRoutingInputCompilerUnit.DomainTag,
            DefaultFlowVersion,
            LogicalUnitTrustState.Trusted,
            isLearned: false,
            promptCompiler);
        if (!compilerUnitRegistration.IsSuccess)
            return OperationResult<LogicalOrchestratorAssistant>.Fail(compilerUnitRegistration.ErrorMessage, "UNIT_REGISTRATION_FAILED");

        var domainClassifierRegistration = unitRegistry.Register(
            DomainClassifierUnit.Id,
            DomainClassifierUnit.Role,
            DomainClassifierUnit.DomainTag,
            DefaultFlowVersion,
            LogicalUnitTrustState.Trusted,
            isLearned: false,
            new DomainClassifierUnit());
        if (!domainClassifierRegistration.IsSuccess)
            return OperationResult<LogicalOrchestratorAssistant>.Fail(domainClassifierRegistration.ErrorMessage, "UNIT_REGISTRATION_FAILED");

        // Load only trusted-host, already-loaded inference units after deterministic reserved ids are registered.
        if (edgeAuthorityPreload is not null)
        {
            var loadedEntries = edgeAuthorityPreload
                .Select(entry => new LogicalUnitRegistryLoadedEntry(entry.Descriptor, entry.Unit))
                .ToArray();
            var baselineDescriptorCount = unitRegistry.ListDescriptors().Count;

            OperationResult<int>? preloadLoad;
            try
            {
                preloadLoad = unitRegistry.LoadEntries(loadedEntries, replaceExisting: false);
            }
            catch (Exception exception) when (IsNonFatalPreloadException(exception))
            {
                return FailedEdgeAuthorityPreloadLoad();
            }

            if (preloadLoad is null
                || !preloadLoad.IsSuccess
                || preloadLoad.Value != loadedEntries.Length
                || unitRegistry.ListDescriptors().Count != baselineDescriptorCount + loadedEntries.Length)
            {
                return FailedEdgeAuthorityPreloadLoad();
            }
        }

        // Create the adaptive orchestrator from runtime registries, route units, and the default capability trainer.
        var orchestrator = dynamicGraphNodeOutputSource is null
            ? new AdaptiveLogicalOrchestrator(
                topologyRegistry,
                promptCompiler,
                router,
                registry,
                unitRegistry,
                multiRouter,
                capabilityTrainer: new DefaultLogicalCapabilityTrainer(),
                dynamicGraphTopologySource: dynamicGraphTopologySource)
            : new AdaptiveLogicalOrchestrator(
                topologyRegistry,
                promptCompiler,
                router,
                registry,
                unitRegistry,
                multiRouter,
                learningEventScheduler: null,
                capabilityTrainer: new DefaultLogicalCapabilityTrainer(),
                dynamicGraphTopologySource: dynamicGraphTopologySource,
                dynamicGraphNodeOutputSource: dynamicGraphNodeOutputSource);

        // Create the add-domain mutation unit backed by the topology registry.
        var addDomainUnit = new AddDomainTopologyMutationUnit(topologyRegistry);

        // Create the add-phase mutation unit backed by the same topology registry.
        var addPhaseUnit = new AddPhaseTopologyMutationUnit(topologyRegistry);

        // Create the add-sub-phase mutation unit backed by the same topology registry.
        var addSubPhaseUnit = new AddSubPhaseTopologyMutationUnit(topologyRegistry);

        // Create the add-transition mutation unit backed by the same topology registry.
        var addTransitionUnit = new AddTransitionTopologyMutationUnit(topologyRegistry);

        // Create the topology mutation flow that accepts typed topology commands.
        var topologyMutationFlow = new TopologyMutationFlow(addDomainUnit, addPhaseUnit, addSubPhaseUnit, addTransitionUnit);

        // Return the assistant through the project result envelope.
        return OperationResult<LogicalOrchestratorAssistant>.Ok(new LogicalOrchestratorAssistant(storage, orchestrator, topologyMutationFlow: topologyMutationFlow));
    }

    // Run a user request through the logical orchestrator surface.
    public async Task<OperationResult<LogicalAssistantRunResult>> RunAsync(
        string userRequest,
        string? scenarioId = null,
        LogicalExecutionFlags? flags = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the project result envelope.
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalAssistantRunResult>.Fail("Cancelled.", "CANCELLED");

        var prepared = PrepareRunRequest(userRequest, scenarioId, flags, metadata);
        if (!prepared.IsSuccess)
            return prepared.MapError<LogicalAssistantRunResult>();

        if (prepared.Value is null)
            return FailedOrchestratorResultRequired();

        return await RunPreparedAsync(prepared.Value, cancellationToken);
    }

    // Prepare one assistant request without starting orchestrator execution.
    internal OperationResult<LogicalOrchestratorRequest> PrepareRunRequest(
        string userRequest,
        string? scenarioId = null,
        LogicalExecutionFlags? flags = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        // Reject empty user requests before creating a logical run.
        if (string.IsNullOrWhiteSpace(userRequest))
            return OperationResult<LogicalOrchestratorRequest>.Fail("User request cannot be empty.", "EMPTY_REQUEST");

        // Reject invalid construction before trying to execute a flow.
        if (_orchestrator is null)
            return OperationResult<LogicalOrchestratorRequest>.Fail("Logical orchestrator is not configured.", "ORCHESTRATOR_NOT_CONFIGURED");

        // Use adaptive routing unless the caller supplies an explicit scenario id.
        var hasExplicitScenario = !string.IsNullOrWhiteSpace(scenarioId);
        var selectedScenarioId = hasExplicitScenario ? scenarioId! : AdaptiveLogicalOrchestrator.ScenarioId;
        ProjectWikiRequestRoutingDecision? projectWikiPreRoute = null;
        if (!hasExplicitScenario)
        {
            var projectWikiRoute = new ProjectWikiRequestRouterUnit().Route(userRequest);
            if (projectWikiRoute.IsSuccess)
                projectWikiPreRoute = projectWikiRoute.Value;

            if (projectWikiPreRoute is not null && IsProjectWikiTrainingRoute(projectWikiPreRoute.RouteKind))
                selectedScenarioId = ProjectWikiTrainingFlowAdapter.ScenarioId;
            else if (projectWikiPreRoute is not null && projectWikiPreRoute.RouteKind == ProjectWikiRequestRouteKind.DocumentationBuild)
                selectedScenarioId = ProjectDocumentationPipelineFlowAdapter.ScenarioId;
        }

        // Create a traceable run id for this assistant invocation.
        var runId = CreateRunId("assistant");

        // Attach source metadata so evidence can distinguish runtime wrapper calls from direct tests.
        var requestMetadata = new Dictionary<string, object>(metadata ?? new Dictionary<string, object>(), StringComparer.Ordinal)
        {
            // Mark the component that created this logical orchestrator request.
            ["source"] = RunSource,

            // Preserve the assistant's default scenario for audit.
            ["default_scenario_id"] = DefaultScenarioId,

            // Tell the adaptive orchestrator whether scenario selection was explicit.
            ["scenario_selection_mode"] = hasExplicitScenario ? "explicit" : "adaptive"
        };
        if (projectWikiPreRoute is not null &&
            (selectedScenarioId == ProjectWikiTrainingFlowAdapter.ScenarioId ||
             selectedScenarioId == ProjectDocumentationPipelineFlowAdapter.ScenarioId))
        {
            requestMetadata["project_wiki_pre_route_kind"] = projectWikiPreRoute.RouteKind.ToString();
            requestMetadata["project_wiki_pre_route_target_unit_id"] = projectWikiPreRoute.TargetUnitId;
            requestMetadata["project_wiki_pre_route_confidence"] = projectWikiPreRoute.Confidence;
            requestMetadata["routed_phase_id"] = projectWikiPreRoute.RouteKind == ProjectWikiRequestRouteKind.DocumentationBuild ? "2" : "7";
            requestMetadata["routed_sub_phase_id"] = projectWikiPreRoute.RouteKind == ProjectWikiRequestRouteKind.DocumentationBuild ? "2.1" : "7.5";
        }

        // Build the orchestrator request with caller flags or default-on flags.
        return OperationResult<LogicalOrchestratorRequest>.Ok(new LogicalOrchestratorRequest(
            runId,
            selectedScenarioId,
            userRequest,
            flags,
            requestMetadata));
    }

    // Execute an already-prepared request through the configured legacy orchestrator entry.
    internal async Task<OperationResult<LogicalAssistantRunResult>> RunPreparedAsync(
        LogicalOrchestratorRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        if (request is null || _orchestrator is null)
            return FailedOrchestratorResultRequired();

        OperationResult<LogicalOrchestratorResult>? orchestratorResult;
        try
        {
            orchestratorResult = await _orchestrator.RunAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (IsNonFatalAssistantException(exception))
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Logical orchestrator execution failed.",
                "ORCHESTRATOR_FAILED");
        }

        return CompleteOrchestratorRun(orchestratorResult, cancellationToken);
    }

    // Execute an already-prepared adaptive request against one exact topology/output invocation pair.
    internal async Task<OperationResult<LogicalAssistantRunResult>> RunPreparedDynamicGraphAsync(
        LogicalOrchestratorRequest request,
        DynamicGraphTopologyRuntimeSnapshot topologySnapshot,
        IDynamicGraphNodeOutputSource nodeOutputSource,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        if (request is null
            || topologySnapshot is null
            || nodeOutputSource is null
            || _orchestrator is not AdaptiveLogicalOrchestrator adaptiveOrchestrator
            || !StringComparer.Ordinal.Equals(request.ScenarioId, AdaptiveLogicalOrchestrator.ScenarioId))
        {
            return FailedOrchestratorResultRequired();
        }

        OperationResult<LogicalOrchestratorResult>? orchestratorResult;
        try
        {
            orchestratorResult = await adaptiveOrchestrator.RunWithInvocationNodeOutputsAsync(
                request,
                topologySnapshot,
                nodeOutputSource,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (IsNonFatalAssistantException(exception))
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Logical orchestrator execution failed.",
                "ORCHESTRATOR_FAILED");
        }

        return CompleteOrchestratorRun(orchestratorResult, cancellationToken);
    }

    private static OperationResult<LogicalAssistantRunResult> CompleteOrchestratorRun(
        OperationResult<LogicalOrchestratorResult>? orchestratorResult,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        if (orchestratorResult is null
            || (orchestratorResult.IsSuccess && orchestratorResult.Value is null))
        {
            return FailedOrchestratorResultRequired();
        }

        // Return orchestrator failures through the project result envelope.
        if (!orchestratorResult.IsSuccess)
            return OperationResult<LogicalAssistantRunResult>.Fail(orchestratorResult.ErrorMessage, "ORCHESTRATOR_FAILED");

        try
        {
            if (cancellationToken.IsCancellationRequested)
                return Cancelled();

            if (!TryMapOrchestratorResult(orchestratorResult.Value!, out var assistantResult))
            {
                return cancellationToken.IsCancellationRequested
                    ? Cancelled()
                    : FailedOrchestratorResultRequired();
            }

            if (cancellationToken.IsCancellationRequested)
                return Cancelled();

            // Return the runtime-facing owned assistant result through the project envelope.
            return OperationResult<LogicalAssistantRunResult>.Ok(assistantResult);
        }
        catch (Exception exception) when (IsNonFatalAssistantException(exception))
        {
            return cancellationToken.IsCancellationRequested
                ? Cancelled()
                : FailedOrchestratorResultRequired();
        }
    }

    // Run a typed topology command through the topology mutation flow.
    public async Task<OperationResult<LogicalAssistantRunResult>> RunTopologyCommandAsync(
        LogicalTopologyCommand command,
        LogicalExecutionFlags? flags = null,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the project result envelope.
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalAssistantRunResult>.Fail("Cancelled.", "CANCELLED");

        // Reject missing topology commands before building the flow request.
        if (command is null)
            return OperationResult<LogicalAssistantRunResult>.Fail("Topology command is required.", "TOPOLOGY_COMMAND_REQUIRED");

        // Reject invalid construction before trying to mutate topology.
        if (_topologyMutationFlow is null)
            return OperationResult<LogicalAssistantRunResult>.Fail("Topology mutation flow is not configured.", "TOPOLOGY_FLOW_NOT_CONFIGURED");

        // Apply default flags unless the caller supplied explicit flags.
        var appliedFlags = flags ?? new LogicalExecutionFlags();

        // Create a traceable run id for this topology mutation invocation.
        var runId = CreateRunId("assistant-topology");

        // Attach source metadata so topology mutations are auditable.
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            // Mark the component that created this topology mutation request.
            ["source"] = RunSource,

            // Record the typed command name without interpreting natural language.
            ["command_type"] = command.GetType().Name
        };

        // Build the typed flow request for topology mutation.
        var request = new LogicalFlowRequest<LogicalTopologyCommand>(
            runId,
            TopologyMutationFlow.FlowId,
            TopologyMutationFlow.ScenarioId,
            command,
            appliedFlags,
            metadata);

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        OperationResult<LogicalFlowResult>? flowResult;
        try
        {
            // Capture the task separately so every started topology mutation remains observed.
            Task<OperationResult<LogicalFlowResult>>? flowTask =
                _topologyMutationFlow.ExecuteAsync(request, cancellationToken);
            if (flowTask is null)
            {
                return cancellationToken.IsCancellationRequested
                    ? Cancelled()
                    : FailedTopologyFlowResultRequired();
            }

            flowResult = await flowTask;
        }
        catch (OperationCanceledException)
        {
            return cancellationToken.IsCancellationRequested
                ? Cancelled()
                : FailedTopologyFlowExecution();
        }
        catch (Exception exception) when (IsNonFatalAssistantException(exception))
        {
            return cancellationToken.IsCancellationRequested
                ? Cancelled()
                : FailedTopologyFlowExecution();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        if (flowResult is null
            || (flowResult.IsSuccess && flowResult.Value is null))
        {
            return FailedTopologyFlowResultRequired();
        }

        // Preserve flow failures at the assistant boundary.
        if (!flowResult.IsSuccess)
            return OperationResult<LogicalAssistantRunResult>.Fail(flowResult.ErrorMessage, flowResult.ErrorCode);

        try
        {
            var successfulFlowResult = flowResult.Value!;
            if (!StringComparer.Ordinal.Equals(successfulFlowResult.FlowId, request.FlowId)
                || !StringComparer.Ordinal.Equals(successfulFlowResult.ScenarioId, request.ScenarioId))
            {
                return cancellationToken.IsCancellationRequested
                    ? Cancelled()
                    : FailedTopologyFlowResultRequired();
            }

            if (!TryMapAssistantResult(
                    successfulFlowResult.ScenarioId,
                    successfulFlowResult.FlowId,
                    successfulFlowResult.OverallVerdict,
                    successfulFlowResult.FinalHumanResult,
                    appliedFlags,
                    successfulFlowResult.Metrics,
                    successfulFlowResult.EvidenceIds,
                    out var assistantResult))
            {
                return cancellationToken.IsCancellationRequested
                    ? Cancelled()
                    : FailedTopologyFlowResultRequired();
            }

            if (cancellationToken.IsCancellationRequested)
                return Cancelled();

            // Return the runtime-facing owned assistant result through the project envelope.
            return OperationResult<LogicalAssistantRunResult>.Ok(assistantResult);
        }
        catch (Exception exception) when (IsNonFatalAssistantException(exception))
        {
            return cancellationToken.IsCancellationRequested
                ? Cancelled()
                : FailedTopologyFlowResultRequired();
        }
    }

    // Run a prompt through the adaptive path that can surface capability-generation gaps.
    public async Task<OperationResult<LogicalAssistantRunResult>> RunWithCapabilityGenerationAsync(
        string userRequest,
        LogicalExecutionFlags? flags = null,
        CancellationToken cancellationToken = default)
    {
        // Fall back to normal adaptive routing when the configured orchestrator does not expose capability generation.
        if (_orchestrator is not AdaptiveLogicalOrchestrator adaptiveOrchestrator)
            return await RunAsync(userRequest, scenarioId: null, flags, cancellationToken: cancellationToken);

        // Return cancellation through the project result envelope.
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalAssistantRunResult>.Fail("Cancelled.", "CANCELLED");

        // Reject empty user requests before creating a logical run.
        if (string.IsNullOrWhiteSpace(userRequest))
            return OperationResult<LogicalAssistantRunResult>.Fail("User request cannot be empty.", "EMPTY_REQUEST");

        // Create a traceable run id for this capability-generation invocation.
        var runId = CreateRunId("assistant-capability");

        // Attach source metadata so adaptive retries are auditable.
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["source"] = RunSource,
            ["default_scenario_id"] = DefaultScenarioId,
            ["scenario_selection_mode"] = "adaptive-capability-generation"
        };

        // Build the adaptive orchestrator request.
        var request = new LogicalOrchestratorRequest(
            runId,
            AdaptiveLogicalOrchestrator.ScenarioId,
            userRequest,
            flags,
            metadata);

        // Run the full knowledge-edge training-and-retry surface.
        var orchestratorResult = await adaptiveOrchestrator.RunWithCapabilityGenerationAsync(request, cancellationToken);

        return CompleteOrchestratorRun(orchestratorResult, cancellationToken);
    }

    // Validate and own every reference exposed by the assistant result without adding content policy.
    private static bool TryMapOrchestratorResult(
        LogicalOrchestratorResult orchestratorResult,
        out LogicalAssistantRunResult assistantResult)
    {
        assistantResult = null!;
        if (orchestratorResult is null
            || orchestratorResult.FlowResult is null)
        {
            return false;
        }

        if (!StringComparer.Ordinal.Equals(
                orchestratorResult.ScenarioId,
                orchestratorResult.FlowResult.ScenarioId)
            || !StringComparer.Ordinal.Equals(
                orchestratorResult.SelectedFlowId,
                orchestratorResult.FlowResult.FlowId))
        {
            return false;
        }

        return TryMapAssistantResult(
            orchestratorResult.ScenarioId,
            orchestratorResult.SelectedFlowId,
            orchestratorResult.OverallVerdict,
            orchestratorResult.FinalHumanResult,
            orchestratorResult.AppliedFlags,
            orchestratorResult.FlowResult.Metrics,
            orchestratorResult.FlowResult.EvidenceIds,
            out assistantResult);
    }

    // Apply one ownership policy to both orchestrator and topology-flow success projections.
    private static bool TryMapAssistantResult(
        string? scenarioId,
        string? selectedFlowId,
        string? overallVerdict,
        string? finalHumanResult,
        LogicalExecutionFlags? appliedFlags,
        IReadOnlyDictionary<string, string>? sourceMetrics,
        string[]? sourceEvidenceIds,
        out LogicalAssistantRunResult assistantResult)
    {
        assistantResult = null!;
        if (scenarioId is null
            || selectedFlowId is null
            || overallVerdict is null
            || finalHumanResult is null
            || appliedFlags is null
            || sourceMetrics is null
            || sourceEvidenceIds is null)
        {
            return false;
        }

        var metrics = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var metric in sourceMetrics)
        {
            if (metric.Key is null
                || metric.Value is null
                || !metrics.TryAdd(metric.Key, metric.Value))
            {
                return false;
            }
        }

        var evidenceIds = sourceEvidenceIds.ToArray();
        if (evidenceIds.Any(value => value is null))
            return false;

        assistantResult = new LogicalAssistantRunResult(
            scenarioId,
            selectedFlowId,
            overallVerdict,
            finalHumanResult,
            appliedFlags with { },
            new ReadOnlyDictionary<string, string>(metrics),
            evidenceIds);
        return true;
    }

    private static string CreateRunId(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}";

    private static OperationResult<LogicalAssistantRunResult> Cancelled()
        => OperationResult<LogicalAssistantRunResult>.Fail("Cancelled.", "CANCELLED");

    private static OperationResult<LogicalAssistantRunResult> FailedTopologyFlowExecution()
        => OperationResult<LogicalAssistantRunResult>.Fail(
            TopologyFlowExecutionFailedMessage,
            TopologyFlowExecutionFailedCode);

    private static OperationResult<LogicalAssistantRunResult> FailedTopologyFlowResultRequired()
        => OperationResult<LogicalAssistantRunResult>.Fail(
            TopologyFlowResultRequiredMessage,
            TopologyFlowResultRequiredCode);

    private static OperationResult<LogicalAssistantRunResult> FailedOrchestratorResultRequired()
        => OperationResult<LogicalAssistantRunResult>.Fail(
            "Logical orchestrator result is required.",
            "ORCHESTRATOR_FAILED");

    private static bool IsNonFatalAssistantException(Exception exception)
        => exception is not OutOfMemoryException
           && exception is not StackOverflowException
           && exception is not AccessViolationException
           && exception is not AppDomainUnloadedException
           && exception is not BadImageFormatException
           && !StringComparer.Ordinal.Equals(
               exception.GetType().FullName,
               "System.Threading.ThreadAbortException");

    private static OperationResult<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>[]>
        SnapshotEdgeAuthorityPreload(
            IReadOnlyList<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>> edgeAuthorityPreload)
    {
        if (edgeAuthorityPreload is null)
        {
            return OperationResult<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>[]>.Fail(
                "An explicit edge-authority preload is required.",
                EdgeAuthorityPreloadRequiredCode);
        }

        ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>[] snapshot;
        try
        {
            var ownedEntries = new List<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>>();
            foreach (var entry in edgeAuthorityPreload)
            {
                if (entry is null)
                {
                    ownedEntries.Add(null!);
                    continue;
                }

                var descriptor = entry.Descriptor;
                var unit = entry.Unit;
                ownedEntries.Add(new ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>(
                    descriptor is null ? null! : descriptor with { },
                    unit));
            }

            snapshot = ownedEntries.ToArray();
        }
        catch (Exception exception) when (IsNonFatalPreloadException(exception))
        {
            return OperationResult<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>[]>.Fail(
                "Edge-authority preload snapshot failed.",
                EdgeAuthorityPreloadSnapshotFailedCode);
        }

        var validation = ValidateEdgeAuthorityPreloadSnapshot(snapshot);
        return validation.IsSuccess
            ? OperationResult<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>[]>.Ok(snapshot)
            : validation.MapError<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>[]>();
    }

    private static OperationResult<bool> ValidateEdgeAuthorityPreloadSnapshot(
        IReadOnlyList<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>> snapshot)
    {
        if (snapshot.Count == 0)
            return InvalidEdgeAuthorityPreload();

        var unitIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in snapshot)
        {
            if (entry is null || entry.Descriptor is null || entry.Unit is null)
                return InvalidEdgeAuthorityPreload();

            var descriptor = entry.Descriptor;
            if (!IsCanonicalIdentity(descriptor.UnitId)
                || !StringComparer.Ordinal.Equals(descriptor.UnitRole, EdgeAuthorityRole)
                || !IsCanonicalIdentity(descriptor.DomainTag)
                || !IsCanonicalIdentity(descriptor.Version)
                || descriptor.InputType != typeof(EdgeAuthorityInput)
                || descriptor.OutputType != typeof(EdgeAuthorityDecision)
                || entry.Unit is not ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>
                || !descriptor.IsLearned
                || !Enum.IsDefined(descriptor.TrustState)
                || descriptor.TrustState < LogicalUnitTrustState.Probationary
                || descriptor.TrustState == LogicalUnitTrustState.Rejected
                || descriptor.RegistrationOrder <= 0
                || StringComparer.Ordinal.Equals(descriptor.UnitId, PromptRoutingInputCompilerUnit.Id)
                || StringComparer.Ordinal.Equals(descriptor.UnitId, DomainClassifierUnit.Id)
                || !unitIds.Add(descriptor.UnitId))
            {
                return InvalidEdgeAuthorityPreload();
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> InvalidEdgeAuthorityPreload()
        => OperationResult<bool>.Fail(
            "Edge-authority preload is invalid.",
            EdgeAuthorityPreloadInvalidCode);

    private static OperationResult<LogicalOrchestratorAssistant> FailedEdgeAuthorityPreloadLoad()
        => OperationResult<LogicalOrchestratorAssistant>.Fail(
            "Edge-authority preload could not be loaded.",
            EdgeAuthorityPreloadLoadFailedCode);

    private static bool IsCanonicalIdentity(string value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim());

    private static bool IsNonFatalPreloadException(Exception exception)
        => exception is not OperationCanceledException
           && exception is not OutOfMemoryException
           && exception is not StackOverflowException
           && exception is not AccessViolationException
           && exception is not AppDomainUnloadedException
           && exception is not BadImageFormatException
           && !StringComparer.Ordinal.Equals(
               exception.GetType().FullName,
               "System.Threading.ThreadAbortException");

    private static bool IsProjectWikiTrainingRoute(ProjectWikiRequestRouteKind routeKind)
        => routeKind is ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining
            or ProjectWikiRequestRouteKind.StackVersionTraining
            or ProjectWikiRequestRouteKind.CodeTraceTraining
            or ProjectWikiRequestRouteKind.EntityImpactTraining
            or ProjectWikiRequestRouteKind.DocumentationGeneratorTraining
            or ProjectWikiRequestRouteKind.QueryPlanningTraining
            or ProjectWikiRequestRouteKind.AnswerComposerTraining
            or ProjectWikiRequestRouteKind.ContextRoutingTraining
            or ProjectWikiRequestRouteKind.NegativeDpoTraining
            or ProjectWikiRequestRouteKind.AnswerVerifierTraining
            or ProjectWikiRequestRouteKind.TraceAggregatorTraining;
}
