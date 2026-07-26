// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using OrchestratorModel.Logical;
using ProjectWikiDomain;

public sealed class DynamicGraphMachineCompositionFactoryTests
{
    private static readonly string[] ExactLeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];

    [Fact]
    public async Task CreateHonestUntrained_RealCompositionReturnsOneStableUnderlyingOrchestrator()
    {
        var storage = new CountingSessionStorage();
        var result = DynamicGraphMachineCompositionFactory.CreateHonestUntrained(
            storage,
            CreateProfile(),
            new ProjectWikiTrainingFlowAdapter());

        var machine = AssertSuccess(result);
        var first = await machine.GetOrchestratorAsync();
        var second = await machine.GetOrchestratorAsync();

        Assert.True(first.IsSuccess, $"{first.ErrorCode}: {first.ErrorMessage}");
        Assert.Same(first, second);
        Assert.Same(first.Value, second.Value);
        Assert.IsType<AdaptiveLogicalOrchestrator>(first.Value);
        Assert.Equal("honest_untrained", DynamicGraphMachine.RuntimeMode);
        Assert.False(DynamicGraphMachine.SemanticQualityClaimed);
        Assert.Equal((0, 0, 0), storage.CallCounts);
    }

    [Fact]
    public async Task CreateHonestUntrainedCore_PreOwnsDeepSnapshot_ComposesOnce_AndReturnsConcurrentStableIdentity()
    {
        var storage = new CountingSessionStorage();
        var callerProfile = CreateProfile();
        DynamicGraphLogicalOrchestratorBootstrapProfile? receivedProfile = null;
        var compositionCalls = 0;

        var result = DynamicGraphMachineCompositionFactory.CreateHonestUntrainedCore(
            storage,
            callerProfile,
            new ProjectWikiTrainingFlowAdapter(),
            (receivedStorage, snapshot, adapter) =>
            {
                compositionCalls++;
                receivedProfile = snapshot;

                Assert.Same(storage, receivedStorage);
                Assert.NotSame(callerProfile, snapshot);
                Assert.NotSame(callerProfile.NodeDefinitions, snapshot.NodeDefinitions);
                Assert.NotSame(callerProfile.NodeDefinitions[0], snapshot.NodeDefinitions[0]);
                Assert.NotSame(callerProfile.NodeDefinitions[0].Descriptor, snapshot.NodeDefinitions[0].Descriptor);
                Assert.NotSame(callerProfile.EdgeBindings, snapshot.EdgeBindings);
                Assert.NotSame(callerProfile.EdgeBindings[0], snapshot.EdgeBindings[0]);
                Assert.NotSame(
                    callerProfile.EdgeBindings[0].LogicalTransitionIds,
                    snapshot.EdgeBindings[0].LogicalTransitionIds);
                Assert.NotSame(
                    callerProfile.EdgeBindings[0].ExcludedLeakageSignals,
                    snapshot.EdgeBindings[0].ExcludedLeakageSignals);
                Assert.NotSame(callerProfile.Budget, snapshot.Budget);

                // Mutation during the composition call cannot reach the already-owned snapshot.
                callerProfile.NodeDefinitions[0] = CreateNodeDefinition("mutated-during-call");
                callerProfile.EdgeBindings[0].LogicalTransitionIds[0] = "mutated-transition-during-call";
                callerProfile.EdgeBindings[0].ExcludedLeakageSignals[0] = "mutated-leakage-during-call";
                callerProfile.EdgeBindings[0] = callerProfile.EdgeBindings[0] with
                {
                    DeterministicFinalAuthority = true
                };

                Assert.Equal("3.4", snapshot.NodeDefinitions[0].Descriptor.NodeId);
                Assert.Equal(
                    "transition-sub-phase-3.4-to-3.5",
                    snapshot.EdgeBindings[0].LogicalTransitionIds[0]);
                Assert.Equal("candidate_index", snapshot.EdgeBindings[0].ExcludedLeakageSignals[0]);
                Assert.False(snapshot.EdgeBindings[0].DeterministicFinalAuthority);

                return DynamicGraphLogicalOrchestratorComposition.Create(
                    receivedStorage,
                    snapshot,
                    adapter);
            });

        var machine = AssertSuccess(result);
        var calls = await Task.WhenAll(
            machine.GetOrchestratorAsync(),
            machine.GetOrchestratorAsync(),
            machine.GetOrchestratorAsync());

        Assert.Equal(1, compositionCalls);
        Assert.NotNull(receivedProfile);
        Assert.All(calls, call => Assert.True(call.IsSuccess, $"{call.ErrorCode}: {call.ErrorMessage}"));
        Assert.Same(calls[0], calls[1]);
        Assert.Same(calls[0], calls[2]);
        Assert.Same(calls[0].Value, calls[1].Value);
        Assert.Same(calls[0].Value, calls[2].Value);

        // Mutation after creation also cannot change what the composition function received.
        callerProfile.EdgeBindings[1] = null!;
        Assert.Equal("3.4", receivedProfile!.NodeDefinitions[0].Descriptor.NodeId);
        Assert.NotNull(receivedProfile.EdgeBindings[1]);
        Assert.Equal((0, 0, 0), storage.CallCounts);
    }

    [Fact]
    public async Task CreateHonestUntrained_OwnsProfileAndRuntimeSnapshotAgainstLaterMutation()
    {
        var profile = CreateProfile();
        var machine = AssertSuccess(DynamicGraphMachineCompositionFactory.CreateHonestUntrained(
            new CountingSessionStorage(),
            profile));
        var firstOrchestrator = AssertSuccess(await machine.GetOrchestratorAsync());
        var source = GetTopologySource(firstOrchestrator);
        var before = Snapshot(AssertSuccess(source.ReadCurrent()));

        profile.NodeDefinitions[0] = new DynamicGraphLogicalOrchestratorNodeDefinition(
            "mutated",
            profile.NodeDefinitions[0].Descriptor with
            {
                NodeId = "mutated",
                IsTrained = true,
                RuntimeSemanticQualityClaimed = true,
                AuthorityRole = "trained_node",
                CheckpointRef = "checkpoint:mutated",
                ManifestRef = "manifest:mutated"
            });
        profile.EdgeBindings[0] = profile.EdgeBindings[0] with
        {
            DeterministicFinalAuthority = true,
            LogicalTransitionIds = ["mutated-transition"]
        };

        var secondOrchestrator = AssertSuccess(await machine.GetOrchestratorAsync());
        var after = Snapshot(AssertSuccess(source.ReadCurrent()));

        Assert.Same(firstOrchestrator, secondOrchestrator);
        Assert.Equal(before, after);
        Assert.DoesNotContain("mutated", after, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("trained-package")]
    [InlineData("semantic-quality")]
    [InlineData("checkpoint-only")]
    [InlineData("manifest-only")]
    [InlineData("diagnostics-role")]
    public void CreateHonestUntrained_RejectsEveryComposedNonBaselineNodeSignal(string signal)
    {
        var profile = CreateProfile();
        var descriptor = profile.NodeDefinitions[0].Descriptor;
        profile.NodeDefinitions[0] = profile.NodeDefinitions[0] with
        {
            Descriptor = signal switch
            {
                "trained-package" => descriptor with
                {
                    IsTrained = true,
                    AuthorityRole = "trained_node",
                    CheckpointRef = "checkpoint:trained",
                    ManifestRef = "manifest:trained"
                },
                "semantic-quality" => descriptor with
                {
                    IsTrained = true,
                    RuntimeSemanticQualityClaimed = true,
                    AuthorityRole = "trained_node",
                    CheckpointRef = "checkpoint:semantic",
                    ManifestRef = "manifest:semantic"
                },
                "checkpoint-only" => descriptor with { CheckpointRef = "checkpoint:unexpected" },
                "manifest-only" => descriptor with { ManifestRef = "manifest:unexpected" },
                "diagnostics-role" => descriptor with { AuthorityRole = "diagnostics_only" },
                _ => throw new InvalidOperationException("Unknown signal mutation.")
            }
        };

        var result = DynamicGraphMachineCompositionFactory.CreateHonestUntrained(
            new CountingSessionStorage(),
            profile);

        AssertFailure(
            result,
            DynamicGraphMachineCompositionFactory.TrainedStateForbiddenCode,
            "Honest-untrained dynamic graph machine composition forbids trained or cutover state.");
    }

    [Fact]
    public void CreateHonestUntrained_NodeDeterministicFinalAuthorityIsRejectedByUpstreamInvariant()
    {
        var profile = CreateProfile();
        profile.NodeDefinitions[0] = profile.NodeDefinitions[0] with
        {
            Descriptor = profile.NodeDefinitions[0].Descriptor with
            {
                DeterministicFinalAuthority = true
            }
        };

        var result = DynamicGraphMachineCompositionFactory.CreateHonestUntrained(
            new CountingSessionStorage(),
            profile);

        AssertFailure(
            result,
            "DYNAMIC_GRAPH_NODE_DETERMINISTIC_AUTHORITY_FORBIDDEN",
            "Dynamic graph node '3.4' cannot use deterministic final authority.");
    }

    [Fact]
    public void CreateHonestUntrained_EdgeDeterministicFinalAuthorityIsRejectedByUpstreamInvariant()
    {
        var profile = CreateProfile();
        profile.EdgeBindings[0] = profile.EdgeBindings[0] with
        {
            DeterministicFinalAuthority = true
        };

        var result = DynamicGraphMachineCompositionFactory.CreateHonestUntrained(
            new CountingSessionStorage(),
            profile);

        AssertFailure(
            result,
            "DYNAMIC_GRAPH_EDGE_DETERMINISTIC_AUTHORITY_FORBIDDEN",
            "Dynamic graph edge 'edge-34-to-35' cannot use deterministic final authority.");
    }

    [Fact]
    public void CreateHonestUntrained_PreservesExactUpstreamFailureCodeAndMessage()
    {
        var result = DynamicGraphMachineCompositionFactory.CreateHonestUntrained(
            null!,
            CreateProfile());

        AssertFailure(result, "STORAGE_REQUIRED", "Session storage is required.");
    }

    [Fact]
    public void CreateHonestUntrained_NullProfilePreservesExactUpstreamFailure()
    {
        var result = DynamicGraphMachineCompositionFactory.CreateHonestUntrained(
            new CountingSessionStorage(),
            null!);

        AssertFailure(
            result,
            "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_REQUIRED",
            "An explicit dynamic graph logical orchestrator bootstrap profile is required.");
    }

    [Theory]
    [InlineData(
        "node-definitions",
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_NODE_DEFINITIONS_REQUIRED",
        "At least one dynamic graph logical orchestrator node definition is required.")]
    [InlineData(
        "node-entry",
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_NODE_DEFINITION_REQUIRED",
        "A dynamic graph logical orchestrator node definition is required.")]
    [InlineData(
        "descriptor",
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_NODE_DESCRIPTOR_REQUIRED",
        "A dynamic graph logical orchestrator node descriptor is required.")]
    [InlineData(
        "edge-bindings",
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_EDGE_BINDINGS_REQUIRED",
        "Dynamic graph logical orchestrator edge bindings are required.")]
    [InlineData(
        "edge-entry",
        "DYNAMIC_GRAPH_REGISTRY_PROJECTION_EDGE_BINDINGS_INVALID",
        "Dynamic graph registry edge binding is invalid.")]
    [InlineData(
        "edge-transitions",
        "DYNAMIC_GRAPH_REGISTRY_PROJECTION_TRANSITIONS_REQUIRED",
        "At least one logical transition id is required for each graph edge binding.")]
    [InlineData(
        "edge-leakage",
        "DYNAMIC_GRAPH_EDGE_LEAKAGE_EXCLUSIONS_REQUIRED",
        "Dynamic graph edge 'edge-34-to-35' leakage exclusion list is required.")]
    [InlineData(
        "budget",
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_BUDGET_REQUIRED",
        "A dynamic graph topology budget is required.")]
    public void CreateHonestUntrained_MalformedShapePreservesExactUpstreamFailure(
        string malformedField,
        string expectedCode,
        string expectedMessage)
    {
        var profile = CreateProfile();
        switch (malformedField)
        {
            case "node-definitions":
                profile = profile with { NodeDefinitions = null! };
                break;
            case "node-entry":
                profile.NodeDefinitions[0] = null!;
                break;
            case "descriptor":
                profile.NodeDefinitions[0] = profile.NodeDefinitions[0] with { Descriptor = null! };
                break;
            case "edge-bindings":
                profile = profile with { EdgeBindings = null! };
                break;
            case "edge-entry":
                profile.EdgeBindings[0] = null!;
                break;
            case "edge-transitions":
                profile.EdgeBindings[0] = profile.EdgeBindings[0] with { LogicalTransitionIds = null! };
                break;
            case "edge-leakage":
                profile.EdgeBindings[0] = profile.EdgeBindings[0] with { ExcludedLeakageSignals = null! };
                break;
            case "budget":
                profile = profile with { Budget = null! };
                break;
            default:
                throw new InvalidOperationException("Unknown malformed field.");
        }

        var result = DynamicGraphMachineCompositionFactory.CreateHonestUntrained(
            new CountingSessionStorage(),
            profile);

        AssertFailure(result, expectedCode, expectedMessage);
    }

    [Fact]
    public void AssistantAccessor_IsInternalReadOnlyAndFailsClosedForMissingOrchestrator()
    {
        var method = typeof(LogicalOrchestratorAssistant).GetMethod(
            "ReadConfiguredOrchestrator",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        Assert.NotNull(method);
        Assert.True(method!.IsAssembly);
        Assert.False(method.IsPublic);
        Assert.Empty(method.GetParameters());
        Assert.Equal(typeof(OperationResult<ILogicalOrchestrator>), method.ReturnType);
        Assert.DoesNotContain(
            typeof(LogicalOrchestratorAssistant)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            candidate => candidate.Name == method.Name);

        var assistant = new LogicalOrchestratorAssistant(
            new CountingSessionStorage(),
            orchestrator: null);
        var result = assistant.ReadConfiguredOrchestrator();
        AssertFailure(result, "ORCHESTRATOR_NOT_CONFIGURED", "Logical orchestrator is not configured.");
    }

    [Fact]
    public async Task FactoryAddsNoLifecycleOrStorageSideEffects()
    {
        var storage = new CountingSessionStorage();
        var machine = AssertSuccess(DynamicGraphMachineCompositionFactory.CreateHonestUntrained(
            storage,
            CreateProfile()));

        var train = await machine.TrainBatchAsync(null!);
        var evaluate = await machine.EvaluateAsync(null!);
        var export = await machine.ExportAsync();
        var import = await machine.ImportAsync(null!);
        var orchestrator = await machine.GetOrchestratorAsync();

        Assert.Equal(DynamicGraphMachine.TrainingNotActivatedCode, train.ErrorCode);
        Assert.Equal(DynamicGraphMachine.EvaluationNotActivatedCode, evaluate.ErrorCode);
        Assert.Equal(DynamicGraphMachine.ExportNotActivatedCode, export.ErrorCode);
        Assert.Equal(DynamicGraphMachine.ImportNotActivatedCode, import.ErrorCode);
        Assert.True(orchestrator.IsSuccess);
        Assert.Equal((0, 0, 0), storage.CallCounts);
    }

    [Fact]
    public void PublicCompositionSurfaceRemainsExactAndFactoryHasOnlyOneCreationEntryPoint()
    {
        var compositionMethods = typeof(DynamicGraphLogicalOrchestratorComposition)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        Assert.Equal(3, compositionMethods.Length);
        Assert.Equal(2, compositionMethods.Count(method => method.Name == "Create"));
        Assert.Single(compositionMethods, method => method.Name == "CreateAxisSweepInvocation");

        var factoryMethod = Assert.Single(
            typeof(DynamicGraphMachineCompositionFactory)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Equal("CreateHonestUntrained", factoryMethod.Name);
        Assert.Equal(typeof(OperationResult<DynamicGraphMachine>), factoryMethod.ReturnType);
        Assert.Equal(
            [
                typeof(ISessionStorage),
                typeof(DynamicGraphLogicalOrchestratorBootstrapProfile),
                typeof(ProjectWikiTrainingFlowAdapter)
            ],
            factoryMethod.GetParameters().Select(parameter => parameter.ParameterType));

        var coreMethod = typeof(DynamicGraphMachineCompositionFactory).GetMethod(
            "CreateHonestUntrainedCore",
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
        Assert.NotNull(coreMethod);
        Assert.True(coreMethod!.IsAssembly);
        Assert.False(coreMethod.IsPublic);
        Assert.True(typeof(DynamicGraphLogicalOrchestratorCompositionFunction).IsNotPublic);
    }

    private static DynamicGraphLogicalOrchestratorBootstrapProfile CreateProfile()
        => new(
            ProfileId: "machine-profile-34-to-41",
            TenantId: "machine-tenant",
            DomainTag: DevelopmentLifecycleTopologySeeder.DomainId,
            ContextId: "machine-context",
            SignalId: "machine-signal",
            SourceRef: "logical-registry:machine-profile",
            TraceRef: "trace:machine-profile",
            NodeDefinitions:
            [
                CreateNodeDefinition("3.4"),
                CreateNodeDefinition("3.5"),
                CreateNodeDefinition("4.1")
            ],
            EdgeBindings:
            [
                CreateEdgeBinding(
                    "edge-34-to-35",
                    "transition-sub-phase-3.4-to-3.5",
                    "authority-machine-34",
                    "34-35"),
                CreateEdgeBinding(
                    "edge-35-to-41",
                    "transition-sub-phase-3.5-to-4.1",
                    "authority-machine-35",
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
                AuthorityRole: RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole,
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

    private static IDynamicGraphTopologySource GetTopologySource(ILogicalOrchestrator orchestrator)
    {
        var adaptive = Assert.IsType<AdaptiveLogicalOrchestrator>(orchestrator);
        var sourceField = typeof(AdaptiveLogicalOrchestrator).GetField(
            "_dynamicGraphTopologySource",
            BindingFlags.Instance | BindingFlags.NonPublic);
        return Assert.IsAssignableFrom<IDynamicGraphTopologySource>(sourceField!.GetValue(adaptive));
    }

    private static string Snapshot(DynamicGraphTopologyRuntimeSnapshot snapshot)
        => string.Join(
            "|",
            snapshot.Revision,
            snapshot.SignalId,
            snapshot.SourceRef,
            snapshot.TraceRef,
            string.Join(",", snapshot.Topology.Nodes.Select(node =>
                $"{node.NodeId}:{node.Descriptor.IsTrained}:{node.Descriptor.AuthorityRole}:{node.Descriptor.CheckpointRef}:{node.Descriptor.ManifestRef}")),
            string.Join(",", snapshot.Topology.Edges.Select(edge =>
                $"{edge.EdgeId}:{edge.DeterministicFinalAuthority}:{edge.AuthorityUnitId}:{edge.SourceProof}:{edge.CutoverProof}:{edge.AblationProofRef}")));

    private static T AssertSuccess<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return Assert.IsAssignableFrom<T>(result.Value);
    }

    private static void AssertFailure<T>(OperationResult<T> result, string code, string message)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(code, result.ErrorCode);
        Assert.Equal(message, result.ErrorMessage);
    }

    private sealed class CountingSessionStorage : ISessionStorage
    {
        private int _saveCalls;
        private int _loadCalls;
        private int _listCalls;

        public (int Save, int Load, int List) CallCounts
            => (Volatile.Read(ref _saveCalls), Volatile.Read(ref _loadCalls), Volatile.Read(ref _listCalls));

        public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        {
            Interlocked.Increment(ref _saveCalls);
            return Task.FromResult(OperationResult<bool>.Ok(true));
        }

        public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        {
            Interlocked.Increment(ref _loadCalls);
            return Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NOT_PERSISTED"));
        }

        public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        {
            Interlocked.Increment(ref _listCalls);
            return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>()));
        }
    }
}
