// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LlmFromScratch.Tests")]

namespace OrchestratorModel.Logical;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ProjectWikiDomain;
using SemanticCompilerAdapter;

// Owns registry projection and topology mutation while publishing only a read-only topology source.
public sealed class DynamicGraphLogicalOrchestratorRuntime
{
    private const string BootstrapRequestRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_BOOTSTRAP_REQUEST_REQUIRED";
    private const string BudgetRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_BUDGET_REQUIRED";
    private const string SignalInvalidCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_SIGNAL_INVALID";
    private const string ApplyResultRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_APPLY_RESULT_REQUIRED";
    private const string NotAppliedCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_NOT_APPLIED";
    private const string SourceResultRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_SOURCE_RESULT_REQUIRED";
    private const string NodeOutputSourceRequiredCode =
        "DYNAMIC_GRAPH_NODE_OUTPUT_INVOCATION_SOURCE_REQUIRED";
    private const string EdgeAuthorityPreloadRequiredCode =
        "DYNAMIC_GRAPH_EDGE_AUTHORITY_PRELOAD_REQUIRED";
    private const string EdgeAuthorityPreloadSnapshotFailedCode =
        "DYNAMIC_GRAPH_EDGE_AUTHORITY_PRELOAD_SNAPSHOT_FAILED";
    private const string EdgeAuthorityPreloadInvalidCode =
        "DYNAMIC_GRAPH_EDGE_AUTHORITY_PRELOAD_INVALID";
    private const string EdgeAuthorityPreloadTopologyMismatchCode =
        "DYNAMIC_GRAPH_EDGE_AUTHORITY_PRELOAD_TOPOLOGY_MISMATCH";
    private const string EdgeAuthorityPreloadValidationFailedCode =
        "DYNAMIC_GRAPH_EDGE_AUTHORITY_PRELOAD_VALIDATION_FAILED";
    private const string EdgeAuthorityRole = "edge-authority";

    private readonly ILogicalRegistryDynamicGraphProjector _projector;
    private readonly IDynamicGraphTopologyRuntimeOwner _runtimeOwner;
    private readonly ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>[]? _edgeAuthorityPreload;

    private DynamicGraphLogicalOrchestratorRuntime(
        LogicalOrchestratorAssistant assistant,
        IDynamicGraphTopologySource topologySource,
        DynamicGraphTopologyApplyResult initialApply,
        ILogicalRegistryDynamicGraphProjector projector,
        IDynamicGraphTopologyRuntimeOwner runtimeOwner,
        ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>[]? edgeAuthorityPreload)
    {
        Assistant = assistant;
        TopologySource = topologySource;
        InitialApply = initialApply;
        _projector = projector;
        _runtimeOwner = runtimeOwner;
        _edgeAuthorityPreload = edgeAuthorityPreload;
    }

    // Publishes the assistant that consumes the same read-only topology source as this facade.
    public LogicalOrchestratorAssistant Assistant { get; }

    // Publishes current topology reads without exposing compare-and-swap owner authority.
    public IDynamicGraphTopologySource TopologySource { get; }

    // Publishes proof that the explicit registry projection advanced revision zero to one.
    public DynamicGraphTopologyApplyResult InitialApply { get; }

    // Build the production-adjacent assistant and its first explicit registry-projected topology revision.
    public static OperationResult<DynamicGraphLogicalOrchestratorRuntime> CreateDefault(
        ISessionStorage storage,
        DynamicGraphRegistryProjectionRequest projectionRequest,
        TopologyBuildBudget budget,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter = null)
        => CreateDefaultImplementation(
            storage,
            projectionRequest,
            budget,
            projectWikiTrainingFlowAdapter,
            null,
            null);

    // Build the same runtime with one explicit invocation-scoped source of actual node outputs.
    public static OperationResult<DynamicGraphLogicalOrchestratorRuntime> CreateDefault(
        ISessionStorage storage,
        DynamicGraphRegistryProjectionRequest projectionRequest,
        TopologyBuildBudget budget,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter,
        IDynamicGraphNodeOutputSource dynamicGraphNodeOutputSource)
    {
        if (dynamicGraphNodeOutputSource is null)
        {
            return OperationResult<DynamicGraphLogicalOrchestratorRuntime>.Fail(
                "Dynamic graph node-output source is required.",
                NodeOutputSourceRequiredCode);
        }

        return CreateDefaultImplementation(
            storage,
            projectionRequest,
            budget,
            projectWikiTrainingFlowAdapter,
            dynamicGraphNodeOutputSource,
            null);
    }

    // Build the same runtime from a trusted host's already-loaded learned edge-authority units.
    public static OperationResult<DynamicGraphLogicalOrchestratorRuntime> CreateDefault(
        ISessionStorage storage,
        DynamicGraphRegistryProjectionRequest projectionRequest,
        TopologyBuildBudget budget,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter,
        IDynamicGraphNodeOutputSource dynamicGraphNodeOutputSource,
        IReadOnlyList<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>> edgeAuthorityPreload)
    {
        if (dynamicGraphNodeOutputSource is null)
        {
            return OperationResult<DynamicGraphLogicalOrchestratorRuntime>.Fail(
                "Dynamic graph node-output source is required.",
                NodeOutputSourceRequiredCode);
        }

        var preloadSnapshot = SnapshotEdgeAuthorityPreload(edgeAuthorityPreload);
        if (!preloadSnapshot.IsSuccess)
            return preloadSnapshot.MapError<DynamicGraphLogicalOrchestratorRuntime>();

        return CreateDefaultImplementation(
            storage,
            projectionRequest,
            budget,
            projectWikiTrainingFlowAdapter,
            dynamicGraphNodeOutputSource,
            preloadSnapshot.Value!);
    }

    // Build the same runtime with trusted edge authorities and invocation-scoped node outputs only.
    public static OperationResult<DynamicGraphLogicalOrchestratorRuntime> CreateDefaultWithEdgeAuthorityPreload(
        ISessionStorage storage,
        DynamicGraphRegistryProjectionRequest projectionRequest,
        TopologyBuildBudget budget,
        IReadOnlyList<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>> edgeAuthorityPreload,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter = null)
    {
        var preloadSnapshot = SnapshotEdgeAuthorityPreload(edgeAuthorityPreload);
        if (!preloadSnapshot.IsSuccess)
            return preloadSnapshot.MapError<DynamicGraphLogicalOrchestratorRuntime>();

        return CreateDefaultImplementation(
            storage,
            projectionRequest,
            budget,
            projectWikiTrainingFlowAdapter,
            null,
            preloadSnapshot.Value!);
    }

    private static OperationResult<DynamicGraphLogicalOrchestratorRuntime> CreateDefaultImplementation(
        ISessionStorage storage,
        DynamicGraphRegistryProjectionRequest projectionRequest,
        TopologyBuildBudget budget,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter,
        IDynamicGraphNodeOutputSource? dynamicGraphNodeOutputSource,
        ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>[]? edgeAuthorityPreload)
    {
        if (projectionRequest is null)
        {
            return OperationResult<DynamicGraphLogicalOrchestratorRuntime>.Fail(
                "An explicit dynamic graph bootstrap projection request is required.",
                BootstrapRequestRequiredCode);
        }

        if (budget is null)
        {
            return OperationResult<DynamicGraphLogicalOrchestratorRuntime>.Fail(
                "A dynamic graph topology budget is required.",
                BudgetRequiredCode);
        }

        ILogicalRegistryDynamicGraphProjector? capturedProjector = null;
        IDynamicGraphTopologyRuntimeOwner? capturedOwner = null;
        IDynamicGraphTopologySource? capturedSource = null;
        DynamicGraphTopologyApplyResult? capturedInitialApply = null;

        Func<InMemoryLogicalTopologyRegistry, OperationResult<IDynamicGraphTopologySource>> topologySourceFactory =
            topologyRegistry =>
            {
                var projector = new LogicalRegistryDynamicGraphProjector(topologyRegistry);
                var owner = new InMemoryDynamicGraphTopologyRuntime(
                    new DeterministicDynamicGraphTopologyBuilder());

                var projected = ProjectValidatedSignal(projector, projectionRequest);
                if (!projected.IsSuccess)
                    return projected.MapError<IDynamicGraphTopologySource>();

                if (edgeAuthorityPreload is not null)
                {
                    var preloadValidation = ValidateProjectedEdgeAuthorityPreload(
                        projected.Value!.ProposedTopology,
                        edgeAuthorityPreload);
                    if (!preloadValidation.IsSuccess)
                        return preloadValidation.MapError<IDynamicGraphTopologySource>();
                }

                var applied = owner.ApplyBuildSignal(new DynamicGraphTopologyApplyRequest(
                    ExpectedRevision: 0,
                    Signal: projected.Value!,
                    Budget: budget));
                if (applied is null || (applied.IsSuccess && applied.Value is null))
                {
                    return OperationResult<IDynamicGraphTopologySource>.Fail(
                        "Dynamic graph topology apply result is required.",
                        ApplyResultRequiredCode);
                }

                if (!applied.IsSuccess)
                    return applied.MapError<IDynamicGraphTopologySource>();

                if (!IsExactInitialApply(applied.Value!, projected.Value!))
                {
                    return OperationResult<IDynamicGraphTopologySource>.Fail(
                        "The initial registry projection was not applied as revision one.",
                        NotAppliedCode);
                }

                var source = new ReadOnlyDynamicGraphTopologySource(owner);
                capturedProjector = projector;
                capturedOwner = owner;
                capturedSource = source;
                capturedInitialApply = applied.Value;
                return OperationResult<IDynamicGraphTopologySource>.Ok(source);
            };

        OperationResult<LogicalOrchestratorAssistant> assistantResult;
        if (dynamicGraphNodeOutputSource is null && edgeAuthorityPreload is null)
        {
            assistantResult = LogicalOrchestratorAssistant.CreateDefaultCore(
                storage,
                projectWikiTrainingFlowAdapter,
                topologySourceFactory);
        }
        else if (dynamicGraphNodeOutputSource is null)
        {
            assistantResult = LogicalOrchestratorAssistant.CreateDefaultCore(
                storage,
                projectWikiTrainingFlowAdapter,
                topologySourceFactory,
                edgeAuthorityPreload!);
        }
        else if (edgeAuthorityPreload is null)
        {
            assistantResult = LogicalOrchestratorAssistant.CreateDefaultCore(
                storage,
                projectWikiTrainingFlowAdapter,
                topologySourceFactory,
                dynamicGraphNodeOutputSource);
        }
        else
        {
            assistantResult = LogicalOrchestratorAssistant.CreateDefaultCore(
                storage,
                projectWikiTrainingFlowAdapter,
                topologySourceFactory,
                dynamicGraphNodeOutputSource,
                edgeAuthorityPreload);
        }

        if (!assistantResult.IsSuccess)
            return assistantResult.MapError<DynamicGraphLogicalOrchestratorRuntime>();

        if (assistantResult.Value is null
            || capturedProjector is null
            || capturedOwner is null
            || capturedSource is null
            || capturedInitialApply is null)
        {
            return OperationResult<DynamicGraphLogicalOrchestratorRuntime>.Fail(
                "Dynamic graph topology source result is required.",
                SourceResultRequiredCode);
        }

        return OperationResult<DynamicGraphLogicalOrchestratorRuntime>.Ok(
            new DynamicGraphLogicalOrchestratorRuntime(
                assistantResult.Value,
                capturedSource,
                capturedInitialApply,
                capturedProjector,
                capturedOwner,
                edgeAuthorityPreload));
    }

    // Project the current logical registry explicitly and apply it through the private CAS owner.
    public OperationResult<DynamicGraphTopologyApplyResult> ApplyRegistryProjection(
        long expectedRevision,
        DynamicGraphRegistryProjectionRequest request,
        TopologyBuildBudget budget)
    {
        if (request is null)
        {
            return OperationResult<DynamicGraphTopologyApplyResult>.Fail(
                "An explicit dynamic graph bootstrap projection request is required.",
                BootstrapRequestRequiredCode);
        }

        if (budget is null)
        {
            return OperationResult<DynamicGraphTopologyApplyResult>.Fail(
                "A dynamic graph topology budget is required.",
                BudgetRequiredCode);
        }

        var projected = ProjectValidatedSignal(_projector, request);
        if (!projected.IsSuccess)
            return projected.MapError<DynamicGraphTopologyApplyResult>();

        if (_edgeAuthorityPreload is not null)
        {
            var preloadValidation = ValidateProjectedEdgeAuthorityPreload(
                projected.Value!.ProposedTopology,
                _edgeAuthorityPreload);
            if (!preloadValidation.IsSuccess)
                return preloadValidation.MapError<DynamicGraphTopologyApplyResult>();
        }

        var applied = _runtimeOwner.ApplyBuildSignal(new DynamicGraphTopologyApplyRequest(
            expectedRevision,
            projected.Value!,
            budget));
        if (applied is null || (applied.IsSuccess && applied.Value is null))
        {
            return OperationResult<DynamicGraphTopologyApplyResult>.Fail(
                "Dynamic graph topology apply result is required.",
                ApplyResultRequiredCode);
        }

        return applied;
    }

    private static OperationResult<DynamicGraphBuildSignal> ProjectValidatedSignal(
        ILogicalRegistryDynamicGraphProjector projector,
        DynamicGraphRegistryProjectionRequest request)
    {
        var projected = projector.Project(request);
        if (projected is null || (projected.IsSuccess && projected.Value is null))
        {
            return OperationResult<DynamicGraphBuildSignal>.Fail(
                "Registry projection must return a dynamic graph build signal.",
                SignalInvalidCode);
        }

        if (!projected.IsSuccess)
            return projected;

        var signal = projected.Value!;
        if (signal.SourceKind != DynamicGraphBuildSourceKind.LogicalRegistryProjection
            || signal.PreviousTopology is not null)
        {
            return OperationResult<DynamicGraphBuildSignal>.Fail(
                "Registry projection returned an invalid build signal.",
                SignalInvalidCode);
        }

        return projected;
    }

    private static bool IsExactInitialApply(
        DynamicGraphTopologyApplyResult applied,
        DynamicGraphBuildSignal signal)
        => applied.PreviousRevision == 0
           && applied.CurrentRevision == 1
           && applied.RevisionAdvanced
           && applied.BuildResult is not null
           && applied.BuildResult.Topology is not null
           && applied.BuildResult.FailureReason is null
           && applied.BuildResult.BudgetProof.WithinBudget
           && applied.BuildResult.MutationEvidence.MutationApplied
           && applied.Current is not null
           && applied.Current.Revision == 1
           && applied.Current.SourceKind == DynamicGraphBuildSourceKind.LogicalRegistryProjection
           && string.Equals(applied.Current.SignalId, signal.SignalId, StringComparison.Ordinal)
           && string.Equals(applied.Current.SourceRef, signal.SourceRef, StringComparison.Ordinal)
           && string.Equals(applied.Current.TraceRef, signal.TraceRef, StringComparison.Ordinal);

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

    private static OperationResult<bool> ValidateProjectedEdgeAuthorityPreload(
        DynamicGraphTopology topology,
        IReadOnlyList<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>> preload)
    {
        if (topology is null)
            return InvalidProjectedEdgeAuthorityPreload();

        IDynamicGraphEdge[] supportedEdges;
        try
        {
            supportedEdges = topology.Edges
                .Where(edge => edge is not null
                    && edge.Kind is (GraphEdgeKind.FanOut
                        or GraphEdgeKind.ManyToMany
                        or GraphEdgeKind.MultiOutput))
                .ToArray();
        }
        catch (Exception exception) when (IsNonFatalPreloadException(exception))
        {
            return InvalidProjectedEdgeAuthorityPreload();
        }

        if (supportedEdges.Length == 0)
            return InvalidProjectedEdgeAuthorityPreload();

        var topologyAuthorityIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var edge in supportedEdges)
        {
            if (!IsCanonicalIdentity(edge.AuthorityUnitId))
                return InvalidProjectedEdgeAuthorityPreload();

            topologyAuthorityIds.Add(edge.AuthorityUnitId);
        }

        var preloadAuthorityIds = new HashSet<string>(
            preload.Select(entry => entry.Descriptor.UnitId),
            StringComparer.Ordinal);
        if (topologyAuthorityIds.Count == 0
            || topologyAuthorityIds.Count != preloadAuthorityIds.Count
            || !topologyAuthorityIds.SetEquals(preloadAuthorityIds))
        {
            return InvalidProjectedEdgeAuthorityPreload();
        }

        var validationRegistry = new InMemoryLogicalUnitRegistry();
        var loadedEntries = preload
            .Select(entry => new LogicalUnitRegistryLoadedEntry(entry.Descriptor, entry.Unit))
            .ToArray();

        OperationResult<int>? loadResult;
        try
        {
            loadResult = validationRegistry.LoadEntries(loadedEntries, replaceExisting: false);
        }
        catch (Exception exception) when (IsNonFatalPreloadException(exception))
        {
            return FailedEdgeAuthorityPreloadValidation();
        }

        if (loadResult is null
            || !loadResult.IsSuccess
            || loadResult.Value != loadedEntries.Length
            || validationRegistry.ListDescriptors().Count != loadedEntries.Length)
        {
            return FailedEdgeAuthorityPreloadValidation();
        }

        var resolver = new RegistryDynamicGraphEdgeAuthorityResolver(validationRegistry);
        foreach (var edge in supportedEdges)
        {
            OperationResult<ResolvedDynamicGraphEdgeAuthority>? resolved;
            try
            {
                resolved = resolver.Resolve(edge, topology.DomainTag);
            }
            catch (Exception exception) when (IsNonFatalPreloadException(exception))
            {
                return FailedEdgeAuthorityPreloadValidation();
            }

            if (resolved is null || !resolved.IsSuccess || resolved.Value is null)
                return FailedEdgeAuthorityPreloadValidation();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> InvalidEdgeAuthorityPreload()
        => OperationResult<bool>.Fail(
            "Edge-authority preload is invalid.",
            EdgeAuthorityPreloadInvalidCode);

    private static OperationResult<bool> InvalidProjectedEdgeAuthorityPreload()
        => OperationResult<bool>.Fail(
            "Edge-authority preload does not exactly match the projected runtime topology.",
            EdgeAuthorityPreloadTopologyMismatchCode);

    private static OperationResult<bool> FailedEdgeAuthorityPreloadValidation()
        => OperationResult<bool>.Fail(
            "Edge-authority preload validation failed.",
            EdgeAuthorityPreloadValidationFailedCode);

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

    private sealed class ReadOnlyDynamicGraphTopologySource(
        IDynamicGraphTopologySource source) : IDynamicGraphTopologySource
    {
        public OperationResult<DynamicGraphTopologyRuntimeSnapshot> ReadCurrent()
            => source.ReadCurrent();
    }
}
