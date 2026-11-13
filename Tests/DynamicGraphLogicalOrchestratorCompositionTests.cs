// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using OrchestratorModel.Logical;

public sealed class DynamicGraphLogicalOrchestratorCompositionTests
{
    private const string ProfileRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_REQUIRED";
    private const string ProfileIdRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_ID_REQUIRED";
    private const string NodeDefinitionsRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_NODE_DEFINITIONS_REQUIRED";
    private const string NodeDefinitionRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_NODE_DEFINITION_REQUIRED";
    private const string NodeDescriptorRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_NODE_DESCRIPTOR_REQUIRED";
    private const string EdgeBindingsRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_EDGE_BINDINGS_REQUIRED";

    private static readonly string[] ExactLeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];

    [Fact]
    public void Create_ValidExplicitProfilePublishesExactProfileAndSingleRevisionOneRuntime()
    {
        var result = DynamicGraphLogicalOrchestratorComposition.Create(
            new NullSessionStorage(),
            CreateProfile());

        var composition = AssertSuccess(result);
        Assert.Equal("profile-explicit-34-to-41", composition.ProfileId);
        Assert.Equal(0, composition.Runtime.InitialApply.PreviousRevision);
        Assert.Equal(1, composition.Runtime.InitialApply.CurrentRevision);
        Assert.True(composition.Runtime.InitialApply.RevisionAdvanced);

        var current = AssertSuccess(composition.Runtime.TopologySource.ReadCurrent());
        Assert.Equal(1, current.Revision);
        Assert.Equal(["3.4", "3.5", "4.1"], current.Topology.Order.Select(entry => entry.NodeId));
        Assert.Same(composition.Runtime.TopologySource, GetAdaptiveSource(composition.Runtime.Assistant));
    }

    [Fact]
    public void Create_NullProfileFailsWithCarrierCode()
        => AssertFailure(
            DynamicGraphLogicalOrchestratorComposition.Create(
                new NullSessionStorage(),
                null!),
            ProfileRequiredCode);

    [Fact]
    public void Create_BlankProfileIdFailsWithCarrierCode()
        => AssertFailure(
            DynamicGraphLogicalOrchestratorComposition.Create(
                new NullSessionStorage(),
                CreateProfile() with { ProfileId = "  " }),
            ProfileIdRequiredCode);

    [Fact]
    public void Create_NullOrEmptyNodeDefinitionsFailWithCarrierCode()
    {
        AssertFailure(
            DynamicGraphLogicalOrchestratorComposition.Create(
                new NullSessionStorage(),
                CreateProfile() with { NodeDefinitions = null! }),
            NodeDefinitionsRequiredCode);
        AssertFailure(
            DynamicGraphLogicalOrchestratorComposition.Create(
                new NullSessionStorage(),
                CreateProfile() with { NodeDefinitions = [] }),
            NodeDefinitionsRequiredCode);
    }

    [Fact]
    public void Create_NullNodeDefinitionFailsWithCarrierCode()
    {
        var profile = CreateProfile();
        profile.NodeDefinitions[1] = null!;

        AssertFailure(
            DynamicGraphLogicalOrchestratorComposition.Create(
                new NullSessionStorage(),
                profile),
            NodeDefinitionRequiredCode);
    }

    [Fact]
    public void Create_NullNodeDescriptorFailsWithCarrierCode()
    {
        var profile = CreateProfile();
        profile.NodeDefinitions[1] = new DynamicGraphLogicalOrchestratorNodeDefinition(
            "3.5",
            null!);

        AssertFailure(
            DynamicGraphLogicalOrchestratorComposition.Create(
                new NullSessionStorage(),
                profile),
            NodeDescriptorRequiredCode);
    }

    [Fact]
    public void Create_NullEdgeBindingsFailWithCarrierCode()
        => AssertFailure(
            DynamicGraphLogicalOrchestratorComposition.Create(
                new NullSessionStorage(),
                CreateProfile() with { EdgeBindings = null! }),
            EdgeBindingsRequiredCode);

    [Fact]
    public void Create_NullStoragePreservesRuntimeStorageFailure()
    {
        var result = DynamicGraphLogicalOrchestratorComposition.Create(
            null!,
            CreateProfile());

        AssertFailure(result, "STORAGE_REQUIRED");
        Assert.Equal("Session storage is required.", result.ErrorMessage);
    }

    [Fact]
    public void Create_InvalidBudgetPreservesDownstreamCodeAndDetails()
    {
        var result = DynamicGraphLogicalOrchestratorComposition.Create(
            new NullSessionStorage(),
            CreateProfile() with
            {
                Budget = new TopologyBuildBudget(
                    MaxNodes: 0,
                    MaxEdges: 8,
                    MaxDepth: 8,
                    MaxParallelBranches: 8)
            });

        AssertFailure(result, "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_INVALID");
        Assert.Equal("Dynamic graph topology build budget is invalid.", result.ErrorMessage);
    }

    [Fact]
    public void Create_NullBudgetPreservesRuntimeBudgetRequiredFailure()
    {
        var result = DynamicGraphLogicalOrchestratorComposition.Create(
            new NullSessionStorage(),
            CreateProfile() with { Budget = null! });

        AssertFailure(result, "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_BUDGET_REQUIRED");
        Assert.Equal("A dynamic graph topology budget is required.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("authority", "DYNAMIC_GRAPH_EDGE_AUTHORITY_REQUIRED", "authority unit id is required")]
    [InlineData("source-proof", "DYNAMIC_GRAPH_EDGE_SOURCE_PROOF_REQUIRED", "source proof is required")]
    [InlineData("cutover-proof", "DYNAMIC_GRAPH_EDGE_CUTOVER_PROOF_REQUIRED", "cutover proof is required")]
    [InlineData("ablation-proof", "DYNAMIC_GRAPH_EDGE_ABLATION_PROOF_REQUIRED", "ablation proof reference is required")]
    public void Create_MissingAuthorityOrProofPreservesDownstreamFailure(
        string missingField,
        string expectedCode,
        string expectedMessageFragment)
    {
        var profile = CreateProfile();
        profile.EdgeBindings[0] = missingField switch
        {
            "authority" => profile.EdgeBindings[0] with { AuthorityUnitId = " " },
            "source-proof" => profile.EdgeBindings[0] with { SourceProof = " " },
            "cutover-proof" => profile.EdgeBindings[0] with { CutoverProof = " " },
            "ablation-proof" => profile.EdgeBindings[0] with { AblationProofRef = " " },
            _ => throw new InvalidOperationException("Unknown test field.")
        };

        var result = DynamicGraphLogicalOrchestratorComposition.Create(
            new NullSessionStorage(),
            profile);

        AssertFailure(result, expectedCode);
        Assert.Contains(expectedMessageFragment, result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void Create_BlankScopeAndSignalRefsAreNotSynthesized()
    {
        var blankScope = DynamicGraphLogicalOrchestratorComposition.Create(
            new NullSessionStorage(),
            CreateProfile() with { TenantId = " " });
        var blankSignal = DynamicGraphLogicalOrchestratorComposition.Create(
            new NullSessionStorage(),
            CreateProfile() with { SourceRef = " " });

        AssertFailure(blankScope, "DYNAMIC_GRAPH_REGISTRY_PROJECTION_SCOPE_INVALID");
        Assert.Equal("Dynamic graph registry projection scope is invalid.", blankScope.ErrorMessage);
        AssertFailure(blankSignal, "DYNAMIC_GRAPH_REGISTRY_PROJECTION_SIGNAL_REFERENCES_INVALID");
        Assert.Equal("Dynamic graph registry projection signal references are invalid.", blankSignal.ErrorMessage);
    }

    [Fact]
    public void Create_CopiesExactDescriptorAuthorityAndProofFieldsWithoutSynthesis()
    {
        var composition = AssertSuccess(DynamicGraphLogicalOrchestratorComposition.Create(
            new NullSessionStorage(),
            CreateProfile()));
        var current = AssertSuccess(composition.Runtime.TopologySource.ReadCurrent());

        Assert.Equal("logical-registry:explicit-profile", current.SourceRef);
        Assert.Equal("trace:explicit-profile", current.TraceRef);
        Assert.All(current.Topology.Nodes, node =>
        {
            Assert.Equal("baseline_candidate_generator", node.Descriptor.AuthorityRole);
            Assert.False(node.Descriptor.IsTrained);
            Assert.False(node.Descriptor.DeterministicFinalAuthority);
            Assert.False(node.Descriptor.RuntimeSemanticQualityClaimed);
            Assert.Null(node.Descriptor.CheckpointRef);
            Assert.Null(node.Descriptor.ManifestRef);
        });

        Assert.Collection(
            current.Topology.Edges.OrderBy(edge => edge.EdgeId, StringComparer.Ordinal),
            edge => AssertExactEdge(
                edge,
                "authority-explicit-34",
                "source-proof:34-35",
                "cutover-proof:34-35",
                "ablation-proof:34-35"),
            edge => AssertExactEdge(
                edge,
                "authority-explicit-35",
                "source-proof:35-41",
                "cutover-proof:35-41",
                "ablation-proof:35-41"));
    }

    [Fact]
    public void Create_SourceMutationsAfterReturnCannotChangeRuntimeSnapshot()
    {
        var profile = CreateProfile();
        var composition = AssertSuccess(DynamicGraphLogicalOrchestratorComposition.Create(
            new NullSessionStorage(),
            profile));
        var before = Snapshot(AssertSuccess(composition.Runtime.TopologySource.ReadCurrent()));

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

        var after = Snapshot(AssertSuccess(composition.Runtime.TopologySource.ReadCurrent()));
        Assert.Equal(before, after);
    }

    [Fact]
    public void Create_NullEdgeEntryIsCopiedAndRejectedByExistingProjectorCode()
    {
        var profile = CreateProfile();
        profile.EdgeBindings[0] = null!;

        var result = DynamicGraphLogicalOrchestratorComposition.Create(
            new NullSessionStorage(),
            profile);

        AssertFailure(result, "DYNAMIC_GRAPH_REGISTRY_PROJECTION_EDGE_BINDINGS_INVALID");
    }

    [Fact]
    public void PublicSurface_ExposesOnlyExplicitCompositionCarriersAndCreateBoundary()
    {
        var methods = typeof(DynamicGraphLogicalOrchestratorComposition)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .ThenBy(method => method.GetParameters().Length)
            .ToArray();

        Assert.Equal(3, methods.Length);

        var create = methods[0];
        Assert.Equal("Create", create.Name);
        Assert.Equal(
            [typeof(ISessionStorage), typeof(DynamicGraphLogicalOrchestratorBootstrapProfile), typeof(ProjectWikiDomain.ProjectWikiTrainingFlowAdapter)],
            create.GetParameters().Select(parameter => parameter.ParameterType));

        var createWithLearnedAuthority = methods[1];
        Assert.Equal("Create", createWithLearnedAuthority.Name);
        Assert.Equal(
            [
                typeof(ISessionStorage),
                typeof(DynamicGraphLogicalOrchestratorBootstrapProfile),
                typeof(IDynamicGraphNodeOutputSource),
                typeof(IReadOnlyList<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>>),
                typeof(ProjectWikiDomain.ProjectWikiTrainingFlowAdapter)
            ],
            createWithLearnedAuthority.GetParameters().Select(parameter => parameter.ParameterType));

        var createAxisSweepInvocation = methods[2];
        Assert.Equal("CreateAxisSweepInvocation", createAxisSweepInvocation.Name);
        Assert.True(createAxisSweepInvocation.IsGenericMethodDefinition);
        Assert.Equal(2, createAxisSweepInvocation.GetGenericArguments().Length);
        var invocationParameters = createAxisSweepInvocation.GetParameters();
        Assert.Equal(5, invocationParameters.Length);
        Assert.Equal(typeof(ISessionStorage), invocationParameters[0].ParameterType);
        Assert.Equal(typeof(DynamicGraphLogicalOrchestratorBootstrapProfile), invocationParameters[1].ParameterType);
        Assert.Equal(
            typeof(IReadOnlyList<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>>),
            invocationParameters[2].ParameterType);
        Assert.Equal(typeof(LogicalUnitDynamicGraphNode<,>), invocationParameters[3].ParameterType.GetGenericTypeDefinition());
        Assert.Equal(typeof(Func<,,>), invocationParameters[4].ParameterType.GetGenericTypeDefinition());

        Assert.Equal(
            ["Descriptor", "LogicalSubPhaseId"],
            typeof(DynamicGraphLogicalOrchestratorNodeDefinition)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal));
        Assert.Equal(
            ["ProfileId", "Runtime"],
            typeof(DynamicGraphLogicalOrchestratorCompositionResult)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal));
        Assert.Equal(
            ["Descriptor", "Invocation"],
            typeof(DynamicGraphCandidateInvocationCompositionResult)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal));
    }

    [Fact]
    public void Create_PublishesReadOnlySourceThatCannotBeDowncastToRuntimeOwner()
    {
        var runtime = AssertSuccess(DynamicGraphLogicalOrchestratorComposition.Create(
            new NullSessionStorage(),
            CreateProfile())).Runtime;

        Assert.False(runtime.TopologySource is IDynamicGraphTopologyRuntimeOwner);
        Assert.DoesNotContain(
            typeof(DynamicGraphLogicalOrchestratorCompositionResult)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(property => property.PropertyType),
            type => type == typeof(IDynamicGraphTopologyRuntimeOwner)
                    || type == typeof(ILogicalRegistryDynamicGraphProjector));
    }

    private static DynamicGraphLogicalOrchestratorBootstrapProfile CreateProfile()
        => new(
            ProfileId: "profile-explicit-34-to-41",
            TenantId: "tenant-explicit-profile",
            DomainTag: DevelopmentLifecycleTopologySeeder.DomainId,
            ContextId: "context-explicit-profile",
            SignalId: "signal-explicit-profile",
            SourceRef: "logical-registry:explicit-profile",
            TraceRef: "trace:explicit-profile",
            NodeDefinitions:
            [
                CreateNodeDefinition("3.4"),
                CreateNodeDefinition("3.5"),
                CreateNodeDefinition("4.1")
            ],
            EdgeBindings:
            [
                CreateEdgeBinding(
                    "edge-explicit-34-to-35",
                    "transition-sub-phase-3.4-to-3.5",
                    "authority-explicit-34",
                    "34-35"),
                CreateEdgeBinding(
                    "edge-explicit-35-to-41",
                    "transition-sub-phase-3.5-to-4.1",
                    "authority-explicit-35",
                    "35-41")
            ],
            Budget: new TopologyBuildBudget(
                MaxNodes: 8,
                MaxEdges: 8,
                MaxDepth: 8,
                MaxParallelBranches: 8));

    private static DynamicGraphLogicalOrchestratorNodeDefinition CreateNodeDefinition(string nodeId)
        => new(
            LogicalSubPhaseId: nodeId,
            Descriptor: new DynamicGraphNodeDescriptor(
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

    private static void AssertExactEdge(
        IDynamicGraphEdge edge,
        string authorityUnitId,
        string sourceProof,
        string cutoverProof,
        string ablationProofRef)
    {
        Assert.Equal(authorityUnitId, edge.AuthorityUnitId);
        Assert.Equal("L2", edge.AuthorityUnitLevel);
        Assert.False(edge.DeterministicFinalAuthority);
        Assert.Equal("candidate_generator", edge.BaselineRole);
        Assert.Equal(ExactLeakageSignals, edge.ExcludedLeakageSignals);
        Assert.Equal(sourceProof, edge.SourceProof);
        Assert.Equal(cutoverProof, edge.CutoverProof);
        Assert.Equal(ablationProofRef, edge.AblationProofRef);
    }

    private static IDynamicGraphTopologySource? GetAdaptiveSource(LogicalOrchestratorAssistant assistant)
    {
        var orchestratorField = typeof(LogicalOrchestratorAssistant).GetField(
            "_orchestrator",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var orchestrator = Assert.IsType<AdaptiveLogicalOrchestrator>(orchestratorField!.GetValue(assistant));
        var sourceField = typeof(AdaptiveLogicalOrchestrator).GetField(
            "_dynamicGraphTopologySource",
            BindingFlags.Instance | BindingFlags.NonPublic);
        return Assert.IsAssignableFrom<IDynamicGraphTopologySource>(sourceField!.GetValue(orchestrator));
    }

    private static string Snapshot(DynamicGraphTopologyRuntimeSnapshot snapshot)
        => string.Join(
            "|",
            snapshot.Revision,
            snapshot.SignalId,
            snapshot.SourceRef,
            snapshot.TraceRef,
            string.Join(",", snapshot.Topology.Nodes.Select(node =>
                $"{node.NodeId}:{node.Descriptor.DomainTag}:{node.Descriptor.AuthorityRole}:{node.Descriptor.CheckpointRef}:{node.Descriptor.ManifestRef}")),
            string.Join(",", snapshot.Topology.Edges.Select(edge =>
                $"{edge.EdgeId}:{edge.AuthorityUnitId}:{string.Join('+', edge.ExcludedLeakageSignals)}:{edge.SourceProof}:{edge.CutoverProof}:{edge.AblationProofRef}")));

    private static T AssertSuccess<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return Assert.IsAssignableFrom<T>(result.Value);
    }

    private static void AssertFailure<T>(OperationResult<T> result, string expectedCode)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    private sealed class NullSessionStorage : ISessionStorage
    {
        public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
            => Task.FromResult(OperationResult<bool>.Ok(true));

        public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
            => Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));

        public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
            => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>()));
    }
}
