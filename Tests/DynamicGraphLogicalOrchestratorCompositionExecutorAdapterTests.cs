// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using System.Runtime.CompilerServices;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using OrchestratorModel.Logical;

public sealed class DynamicGraphLogicalOrchestratorCompositionExecutorAdapterTests
{
    private static readonly string[] ExactLeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];

    [Fact]
    public void ParameterlessAdapter_ComposesRealRevisionOneSessionWithExactSafeEvidence()
    {
        IDynamicGraphLogicalOrchestratorCompositionExecutor executor =
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter();

        var session = AssertSuccess(executor.Compose(CreateProfile()));
        var current = AssertSuccess(session.ReadCurrent());

        Assert.Equal(
            new DynamicGraphLogicalOrchestratorSessionDescriptor(
                "executor-profile-34-to-41",
                "executor-tenant",
                DevelopmentLifecycleTopologySeeder.DomainId,
                "executor-context"),
            session.Descriptor);
        Assert.Equal(session.Descriptor, current.Descriptor);
        Assert.Equal(1, current.Revision);
        Assert.Equal("executor-signal", current.SignalId);
        Assert.Equal(DynamicGraphBuildSourceKind.LogicalRegistryProjection, current.SourceKind);
        Assert.Equal(3, current.NodeCount);
        Assert.Equal(2, current.EdgeCount);
        Assert.True(current.InitialRevisionAdvanced);
    }

    [Fact]
    public void LegacyCreateAndExecutor_PublishEquivalentInitialTopologyEvidence()
    {
        var profile = CreateProfile();
        var executorSession = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter().Compose(profile));
        var executorCurrent = AssertSuccess(executorSession.ReadCurrent());
        var legacy = AssertSuccess(DynamicGraphLogicalOrchestratorComposition.Create(
            new CountingSessionStorage(),
            profile));
        var legacyCurrent = AssertSuccess(legacy.Runtime.TopologySource.ReadCurrent());

        Assert.Equal(legacy.ProfileId, executorCurrent.Descriptor.ProfileId);
        Assert.Equal(legacyCurrent.Topology.TenantId, executorCurrent.Descriptor.TenantId);
        Assert.Equal(legacyCurrent.Topology.DomainTag, executorCurrent.Descriptor.DomainTag);
        Assert.Equal(legacyCurrent.Topology.ContextId, executorCurrent.Descriptor.ContextId);
        Assert.Equal(legacyCurrent.Revision, executorCurrent.Revision);
        Assert.Equal(legacyCurrent.SignalId, executorCurrent.SignalId);
        Assert.Equal(legacyCurrent.SourceKind, executorCurrent.SourceKind);
        Assert.Equal(legacyCurrent.Topology.Nodes.Count, executorCurrent.NodeCount);
        Assert.Equal(legacyCurrent.Topology.Edges.Count, executorCurrent.EdgeCount);
        Assert.Equal(
            legacy.Runtime.InitialApply.RevisionAdvanced
            && legacy.Runtime.InitialApply.CurrentRevision > legacy.Runtime.InitialApply.PreviousRevision,
            executorCurrent.InitialRevisionAdvanced);
    }

    [Fact]
    public void Session_RetainsRuntimeAfterComposeScopeAndGcPressure()
    {
        var session = CreateRetainedSession();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var current = AssertSuccess(session.ReadCurrent());
        Assert.Equal(1, current.Revision);
        Assert.Equal(3, current.NodeCount);
        Assert.Equal(2, current.EdgeCount);
    }

    [Fact]
    public void Compose_ProfilesAndNestedArrayMutationsCannotChangeRetainedSessionEvidence()
    {
        var profile = CreateProfile();
        var session = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter().Compose(profile));
        var beforeDescriptor = session.Descriptor;
        var before = AssertSuccess(session.ReadCurrent());

        profile.NodeDefinitions[0] = new DynamicGraphLogicalOrchestratorNodeDefinition(
            "mutated-sub-phase",
            profile.NodeDefinitions[0].Descriptor with
            {
                NodeId = "mutated-node",
                DomainTag = "mutated-domain",
                AuthorityRole = "trained",
                CheckpointRef = "mutated-checkpoint",
                ManifestRef = "mutated-manifest"
            });
        profile.EdgeBindings[0].LogicalTransitionIds[0] = "mutated-transition";
        profile.EdgeBindings[0].ExcludedLeakageSignals[0] = "mutated-leakage";
        profile.EdgeBindings[0] = profile.EdgeBindings[0] with
        {
            AuthorityUnitId = "mutated-authority",
            SourceProof = "mutated-source-proof",
            CutoverProof = "mutated-cutover-proof",
            AblationProofRef = "mutated-ablation-proof"
        };
        profile.EdgeBindings[1] = null!;

        Assert.Equal(beforeDescriptor, session.Descriptor);
        Assert.Equal(before, AssertSuccess(session.ReadCurrent()));
    }

    [Fact]
    public void InternalStorageSeam_PerformsNoStorageOperationsDuringComposeOrRead()
    {
        var storage = new CountingSessionStorage();
        var executor = new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter(storage);

        var session = AssertSuccess(executor.Compose(CreateProfile()));
        AssertSuccess(session.ReadCurrent());

        Assert.Equal(0, storage.SaveCount);
        Assert.Equal(0, storage.LoadCount);
        Assert.Equal(0, storage.ListCount);
    }

    [Fact]
    public void PublicAdapterAndSessionSurface_ExposeNoRuntimeOwnerOrOperationalMethods()
    {
        var adapterType = typeof(DynamicGraphLogicalOrchestratorCompositionExecutorAdapter);
        var publicConstructor = Assert.Single(adapterType.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Empty(publicConstructor.GetParameters());

        var adapterMethods = adapterType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();
        var composeMethod = Assert.Single(adapterMethods);
        Assert.Equal("Compose", composeMethod.Name);
        Assert.Equal(typeof(DynamicGraphLogicalOrchestratorBootstrapProfile), Assert.Single(composeMethod.GetParameters()).ParameterType);
        Assert.Equal(
            typeof(OperationResult<IDynamicGraphLogicalOrchestratorCompositionSession>),
            composeMethod.ReturnType);

        var session = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter().Compose(CreateProfile()));
        Assert.False(session.GetType().IsPublic);
        Assert.False(session.GetType().IsNestedPublic);
        Assert.False(typeof(DynamicGraphLogicalOrchestratorRuntime).IsAssignableFrom(session.GetType()));
        Assert.False(typeof(IDynamicGraphTopologyRuntimeOwner).IsAssignableFrom(session.GetType()));

        var sessionInterface = typeof(IDynamicGraphLogicalOrchestratorCompositionSession);
        Assert.Equal(["Descriptor"], sessionInterface.GetProperties().Select(property => property.Name));
        Assert.Equal(
            ["ReadCurrent"],
            sessionInterface.GetMethods().Where(method => !method.IsSpecialName).Select(method => method.Name));
        Assert.DoesNotContain(
            adapterMethods.Concat(sessionInterface.GetMethods()),
            method => ContainsForbiddenOperationName(method.Name));
        Assert.DoesNotContain(
            PublicSignatureTypes(adapterType)
                .Concat(PublicSignatureTypes(sessionInterface)),
            IsForbiddenPublicType);
    }

    [Fact]
    public void SessionResult_IsBoundedDtoWithoutReceiptProvenanceOrMutableCollections()
    {
        Assert.Equal(
            [
                "Descriptor",
                "Revision",
                "SignalId",
                "SourceKind",
                "NodeCount",
                "EdgeCount",
                "InitialRevisionAdvanced"
            ],
            typeof(DynamicGraphLogicalOrchestratorSessionResult)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .OrderBy(property => property.MetadataToken)
                .Select(property => property.Name));
        Assert.DoesNotContain(
            typeof(DynamicGraphLogicalOrchestratorSessionResult)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance),
            property => property.PropertyType.IsArray
                        || typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType)
                           && property.PropertyType != typeof(string));
    }

    [Fact]
    public void Compose_NullAndBlankProfilesPreserveExactCarrierFailures()
    {
        var executor = new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter();

        var missing = executor.Compose(null!);
        var blank = executor.Compose(CreateProfile() with { ProfileId = "  " });

        AssertFailure(
            missing,
            "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_REQUIRED",
            "An explicit dynamic graph logical orchestrator bootstrap profile is required.");
        AssertFailure(
            blank,
            "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_ID_REQUIRED",
            "A dynamic graph logical orchestrator profile id is required.");
    }

    [Fact]
    public void Compose_InvalidBudgetPreservesExactDownstreamFailure()
    {
        var profile = CreateProfile() with
        {
            Budget = new TopologyBuildBudget(0, 8, 8, 8)
        };

        var result = new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter().Compose(profile);

        AssertFailure(
            result,
            "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_INVALID",
            "Dynamic graph topology build budget is invalid.");
    }

    [Fact]
    public void Compose_InvalidAuthorityPreservesExactDownstreamFailure()
    {
        var profile = CreateProfile();
        profile.EdgeBindings[0] = profile.EdgeBindings[0] with { AuthorityUnitId = " " };

        var result = new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter().Compose(profile);

        AssertFailure(
            result,
            "DYNAMIC_GRAPH_EDGE_AUTHORITY_REQUIRED",
            "Dynamic graph edge 'executor-edge-34-to-35' authority unit id is required.");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IDynamicGraphLogicalOrchestratorCompositionSession CreateRetainedSession()
        => AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter().Compose(CreateProfile()));

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

    private static IEnumerable<Type> PublicSignatureTypes(Type type)
        => type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .SelectMany(constructor => constructor.GetParameters().Select(parameter => parameter.ParameterType))
            .Concat(type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType)))
            .Concat(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(property => property.PropertyType))
            .SelectMany(FlattenType);

    private static IEnumerable<Type> FlattenType(Type type)
    {
        yield return type;
        if (type.IsGenericType)
        {
            foreach (var argument in type.GetGenericArguments().SelectMany(FlattenType))
                yield return argument;
        }
    }

    private static bool IsForbiddenPublicType(Type type)
        => type == typeof(DynamicGraphLogicalOrchestratorRuntime)
           || type == typeof(IDynamicGraphTopologyRuntimeOwner)
           || type == typeof(ISessionStorage)
           || type.FullName?.Contains("LogicalOrchestratorAssistant", StringComparison.Ordinal) == true
           || type.FullName?.Contains("ProjectWikiTrainingFlowAdapter", StringComparison.Ordinal) == true;

    private static bool ContainsForbiddenOperationName(string name)
        => new[] { "Run", "Train", "Dpo", "Apply", "Save", "Persist" }
            .Any(token => name.Contains(token, StringComparison.OrdinalIgnoreCase));

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
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    private sealed class CountingSessionStorage : ISessionStorage
    {
        public int SaveCount { get; private set; }

        public int LoadCount { get; private set; }

        public int ListCount { get; private set; }

        public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        {
            SaveCount++;
            return Task.FromResult(OperationResult<bool>.Ok(true));
        }

        public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        {
            LoadCount++;
            return Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "COUNTING_STORAGE_EMPTY"));
        }

        public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        {
            ListCount++;
            return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>()));
        }
    }
}
