// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using System.Reflection;

// These tests pin the foundational DynamicGraph contract shape and deterministic topology validation behavior.
public sealed class DynamicGraphContractsTests
{
    // These fixture carriers define one consistent tenant/domain/context scope for valid topologies.
    private const string TenantId = "tenant-alpha";
    private const string DomainTag = "dynamic-graph";
    private const string ContextId = "context-build-001";

    // Every valid edge fixture explicitly carries the complete owner-defined leakage exclusion vocabulary.
    private static readonly string[] RequiredLeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];

    [Fact]
    public void NodeContracts_ExposeExactOwnerShapeAndQueryableCandidateBoundary()
    {
        // The non-generic base exposes only stable identity and the canonical descriptor.
        var baseProperties = typeof(IDynamicGraphNode).GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType))
            .ToArray();
        Assert.Equal(
            new (string Name, Type Type)[]
            {
                ("Descriptor", typeof(DynamicGraphNodeDescriptor)),
                ("NodeId", typeof(string))
            },
            baseProperties);

        // The closed generic contract retains exactly one inference method and the IQueryable candidate boundary.
        var typedNode = typeof(IDynamicGraphNode<string, int>);
        Assert.Contains(typeof(IDynamicGraphNode), typedNode.GetInterfaces());
        var execute = Assert.Single(typedNode.GetMethods(), method => !method.IsSpecialName);
        Assert.Equal("ExecuteAsync", execute.Name);
        Assert.Equal(
            typeof(Task<OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>>),
            execute.ReturnType);
        Assert.Equal(
            [typeof(LogicalUnitRequest<string>), typeof(CancellationToken)],
            execute.GetParameters().Select(parameter => parameter.ParameterType));

        // Record constructor order, names, and types are pinned because positional records are serialized contracts.
        AssertCanonicalRecordShape<DynamicGraphNodeDescriptor>(
            [
                ("NodeId", typeof(string)),
                ("DomainTag", typeof(string)),
                ("TInputTypeName", typeof(string)),
                ("TOutputTypeName", typeof(string)),
                ("IsTrained", typeof(bool)),
                ("DeterministicFinalAuthority", typeof(bool)),
                ("RuntimeSemanticQualityClaimed", typeof(bool)),
                ("AuthorityRole", typeof(string)),
                ("CheckpointRef", typeof(string)),
                ("ManifestRef", typeof(string))
            ]);

        AssertCanonicalRecordShape<GraphNodeCandidate>(
            [
                ("CandidateId", typeof(string)),
                ("UnitId", typeof(string)),
                ("UnitIds", typeof(string[])),
                ("LogicalUnitRef", typeof(string)),
                ("OutputRefs", typeof(string[])),
                ("OutputType", typeof(string)),
                ("Confidence", typeof(double)),
                ("Score", typeof(double?)),
                ("ProvenanceRefs", typeof(string[])),
                ("PathOrderRef", typeof(string)),
                ("LegalStatus", typeof(string)),
                ("Status", typeof(string)),
                ("ErrorCodes", typeof(string[])),
                ("SourceIds", typeof(string[])),
                ("TraceRef", typeof(string)),
                ("TraceRefs", typeof(string[])),
                ("EvidenceIds", typeof(string[])),
                ("B1TeacherJudgeRef", typeof(string)),
                ("B7GraphOrderRef", typeof(string)),
                ("B11AsyncBatchRef", typeof(string)),
                ("B12WinnerSelectionRef", typeof(string)),
                ("B6UnitDpoBuilderRef", typeof(string)),
                ("SemanticQualityClaimed", typeof(bool))
            ]);

        // Node contracts remain inference-only and do not expose a graph-machine runtime type.
        var forbiddenNodeMethods = new[] { typeof(IDynamicGraphNode), typedNode }
            .SelectMany(type => type.GetMethods())
            .Where(method => IsTrainingOrMutationName(method.Name))
            .ToArray();
        Assert.Empty(forbiddenNodeMethods);
        Assert.Null(typeof(IDynamicGraphNode).Assembly.GetType("AgentInfrastructure.Logical.DynamicGraphMachine"));
    }

    [Fact]
    public void AsyncCandidateRecords_ExposeExactOwnerPositionalShapes()
    {
        AssertCanonicalRecordShape<GraphCandidateRunRequest>(
            [
                ("ScenarioId", typeof(string)),
                ("Query", typeof(string)),
                ("InputGraphRefs", typeof(string[])),
                ("InputUnitRefs", typeof(string[])),
                ("Budget", typeof(AsyncCombinationBudget)),
                ("CombinationCount", typeof(int)),
                ("Cancellation", typeof(CancellationToken)),
                ("LegalScope", typeof(string)),
                ("ProvenanceRefs", typeof(string[])),
                ("TraceRef", typeof(string))
            ]);

        AssertCanonicalRecordShape<AsyncCombinationBudget>(
            [
                ("MaxCombinations", typeof(int)),
                ("Timeout", typeof(TimeSpan?)),
                ("MaxParallelBranches", typeof(int)),
                ("MaxParallelCombinationRuns", typeof(int))
            ]);

        AssertCanonicalRecordShape<GraphCandidateOutput>(
            [
                ("CandidateId", typeof(string)),
                ("OutputRefs", typeof(IReadOnlyList<GraphNodeCandidate>)),
                ("UnitId", typeof(string)),
                ("UnitIds", typeof(string[])),
                ("Confidence", typeof(double)),
                ("Score", typeof(double?)),
                ("CombinationId", typeof(string)),
                ("ProvenanceRefs", typeof(string[])),
                ("PathTraceRef", typeof(string)),
                ("TraceRef", typeof(string)),
                ("ErrorCodes", typeof(string[])),
                ("ElapsedMs", typeof(long))
            ]);

        AssertCanonicalRecordShape<AsyncCandidateBatch>(
            [
                ("BatchId", typeof(string)),
                ("Request", typeof(GraphCandidateRunRequest)),
                ("Candidates", typeof(IReadOnlyList<GraphCandidateOutput>)),
                ("CandidateStream", typeof(IAsyncEnumerable<GraphCandidateOutput>)),
                ("RequestedCombinationCount", typeof(int)),
                ("ActualCombinationCount", typeof(int)),
                ("Status", typeof(string)),
                ("ErrorCodes", typeof(string[])),
                ("ElapsedMs", typeof(long))
            ]);

        AssertCanonicalRecordShape<CombinationPlan>(
            [
                ("Index", typeof(int)),
                ("CombinationId", typeof(string)),
                ("NodeSetIds", typeof(string[])),
                ("EdgeOrderKey", typeof(string)),
                ("Flags", typeof(LogicalExecutionFlags))
            ]);
    }

    [Fact]
    public void AsyncCombinationBudget_PreservesOwnerDefaultAndExactTimeoutType()
    {
        var parameters = Assert.Single(typeof(AsyncCombinationBudget).GetConstructors()).GetParameters();

        Assert.Equal(4, parameters.Length);
        Assert.Equal(typeof(TimeSpan?), parameters[1].ParameterType);
        Assert.False(parameters[2].IsOptional);
        Assert.True(parameters[3].IsOptional);
        Assert.True(parameters[3].HasDefaultValue);
        Assert.Equal(1, parameters[3].DefaultValue);
    }

    [Fact]
    public void AsyncCandidatePayloads_UseOnlyTypedOwnerCollectionsAndNullableSurfaces()
    {
        var outputRefs = typeof(GraphCandidateOutput).GetProperty(nameof(GraphCandidateOutput.OutputRefs));
        Assert.NotNull(outputRefs);
        Assert.Equal(typeof(IReadOnlyList<GraphNodeCandidate>), outputRefs.PropertyType);

        var candidates = typeof(AsyncCandidateBatch).GetProperty(nameof(AsyncCandidateBatch.Candidates));
        Assert.NotNull(candidates);
        Assert.Equal(typeof(IReadOnlyList<GraphCandidateOutput>), candidates.PropertyType);

        var candidateStream = typeof(AsyncCandidateBatch).GetProperty(nameof(AsyncCandidateBatch.CandidateStream));
        Assert.NotNull(candidateStream);
        Assert.Equal(typeof(IAsyncEnumerable<GraphCandidateOutput>), candidateStream.PropertyType);
        var nullability = new NullabilityInfoContext();
        Assert.Equal(NullabilityState.Nullable, nullability.Create(candidateStream).ReadState);

        var payloadProperties = new[] { typeof(GraphCandidateOutput), typeof(AsyncCandidateBatch) }
            .SelectMany(type => type.GetProperties())
            .ToArray();
        Assert.DoesNotContain(payloadProperties, property => property.PropertyType == typeof(object));
        Assert.DoesNotContain(payloadProperties, property => property.PropertyType == typeof(object[]));
        Assert.DoesNotContain(
            payloadProperties,
            property => typeof(IQueryable).IsAssignableFrom(property.PropertyType));
        Assert.DoesNotContain(
            payloadProperties,
            property => property.PropertyType == typeof(System.Collections.IEnumerable)
                || property.PropertyType == typeof(IEnumerable<object>)
                || property.PropertyType == typeof(IAsyncEnumerable<object>));

        var recordTypes = new[]
        {
            typeof(GraphCandidateRunRequest),
            typeof(AsyncCombinationBudget),
            typeof(GraphCandidateOutput),
            typeof(AsyncCandidateBatch),
            typeof(CombinationPlan)
        };
        var nullableProperties = recordTypes
            .SelectMany(type => type.GetProperties().Select(property => (Type: type, Property: property)))
            .Where(item => Nullable.GetUnderlyingType(item.Property.PropertyType) is not null
                || (!item.Property.PropertyType.IsValueType
                    && nullability.Create(item.Property).ReadState == NullabilityState.Nullable))
            .Select(item => $"{item.Type.Name}.{item.Property.Name}")
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            [
                "AsyncCandidateBatch.CandidateStream",
                "AsyncCombinationBudget.Timeout",
                "GraphCandidateOutput.Score"
            ],
            nullableProperties);

        var nullableParameters = recordTypes
            .SelectMany(type => Assert.Single(type.GetConstructors()).GetParameters()
                .Select(parameter => (Type: type, Parameter: parameter)))
            .Where(item => Nullable.GetUnderlyingType(item.Parameter.ParameterType) is not null
                || (!item.Parameter.ParameterType.IsValueType
                    && nullability.Create(item.Parameter).ReadState == NullabilityState.Nullable))
            .Select(item => $"{item.Type.Name}.{item.Parameter.Name}")
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            [
                "AsyncCandidateBatch.CandidateStream",
                "AsyncCombinationBudget.Timeout",
                "GraphCandidateOutput.Score"
            ],
            nullableParameters);
    }

    [Fact]
    public void GraphCandidateRunner_ExposesExactlyOneAsyncOnlyOwnerMethod()
    {
        Assert.True(typeof(IGraphCandidateRunner).IsInterface);
        Assert.Empty(typeof(IGraphCandidateRunner).GetProperties());
        var method = Assert.Single(
            typeof(IGraphCandidateRunner).GetMethods(),
            candidate => !candidate.IsSpecialName);

        Assert.Equal("RunCandidatesAsync", method.Name);
        Assert.Equal(typeof(Task<OperationResult<AsyncCandidateBatch>>), method.ReturnType);
        var parameters = method.GetParameters();
        Assert.Equal(
            new (string Name, Type Type)[]
            {
                ("request", typeof(GraphCandidateRunRequest)),
                ("cancellationToken", typeof(CancellationToken))
            },
            parameters.Select(parameter => (parameter.Name!, parameter.ParameterType)));
        Assert.False(parameters[0].IsOptional);
        Assert.True(parameters[1].IsOptional);
        Assert.True(parameters[1].HasDefaultValue);
        Assert.DoesNotContain(
            typeof(IGraphCandidateRunner).GetMethods(),
            candidate => StringComparer.Ordinal.Equals(candidate.Name, "RunCandidates"));
    }

    [Fact]
    public void AsyncCandidateBoundary_RejectsTrainingSelectionAndUnownedImplementations()
    {
        var boundedTypes = new[]
        {
            typeof(GraphCandidateRunRequest),
            typeof(AsyncCombinationBudget),
            typeof(GraphCandidateOutput),
            typeof(AsyncCandidateBatch),
            typeof(CombinationPlan),
            typeof(IGraphCandidateRunner)
        };
        var forbiddenFragments = new[]
        {
            "Train",
            "Learn",
            "Fit",
            "UpdateWeights",
            "BuildDpo",
            "Chosen",
            "Rejected",
            "SelectWinner",
            "GraphOrderDecision",
            "Teacher",
            "Judge"
        };
        var boundedSurface = boundedTypes
            .SelectMany(type => type.GetMembers()
                .Select(member => $"{type.Name}.{member.Name}"))
            .Concat(boundedTypes.Select(type => type.Name))
            .ToArray();

        foreach (var fragment in forbiddenFragments)
        {
            Assert.DoesNotContain(
                boundedSurface,
                name => name.Contains(fragment, StringComparison.OrdinalIgnoreCase));
        }

        var outputPropertyNames = typeof(GraphCandidateOutput).GetProperties()
            .Select(property => property.Name)
            .ToArray();
        Assert.DoesNotContain("LegalStatus", outputPropertyNames);
        Assert.DoesNotContain("Status", outputPropertyNames);

        // This check is intentionally bounded to this six-type slice and does not ban future owner-approved types.
        Assert.DoesNotContain(
            boundedTypes,
            type => type.IsClass
                && !type.IsAbstract
                && typeof(IGraphCandidateRunner).IsAssignableFrom(type));
        Assert.DoesNotContain(
            boundedTypes,
            type => type.Name.Contains("Strategy", StringComparison.Ordinal));
    }

    [Fact]
    public void GraphEdgeKind_HasExactlyFiveCanonicalValues()
    {
        // Enum declaration order is part of the public serialized contract for this foundational slice.
        Assert.Equal(
            [
                GraphEdgeKind.FanOut,
                GraphEdgeKind.FanIn,
                GraphEdgeKind.ManyToMany,
                GraphEdgeKind.MultiOutput,
                GraphEdgeKind.ReturnLoop
            ],
            Enum.GetValues<GraphEdgeKind>());
        Assert.Equal(
            ["FanOut", "FanIn", "ManyToMany", "MultiOutput", "ReturnLoop"],
            Enum.GetNames<GraphEdgeKind>());
    }

    [Fact]
    public void EdgeContract_ExposesExactlyFourteenTypedDeclarativeProperties()
    {
        // Compare stable name/type pairs, not reflection declaration order, so the test pins the serialized shape.
        var actual = typeof(IDynamicGraphEdge).GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType))
            .ToArray();
        var expected = new (string Name, Type Type)[]
        {
            ("AblationProofRef", typeof(string)),
            ("AuthorityUnitId", typeof(string)),
            ("AuthorityUnitLevel", typeof(string)),
            ("BaselineRole", typeof(string)),
            ("BaselineUnitId", typeof(string)),
            ("CutoverProof", typeof(string)),
            ("DeterministicFinalAuthority", typeof(bool)),
            ("EdgeId", typeof(string)),
            ("ExcludedLeakageSignals", typeof(IReadOnlyList<string>)),
            ("FromNodeId", typeof(string)),
            ("Kind", typeof(GraphEdgeKind)),
            ("ParallelEligible", typeof(bool)),
            ("SourceProof", typeof(string)),
            ("ToNodeIds", typeof(IReadOnlyList<string>))
        };
        Assert.Equal(expected, actual);

        // Authority remains a registry id, and the foundation exposes no traversal or mutable unit object.
        Assert.Equal(typeof(string), actual.Single(property => property.Name == "AuthorityUnitId").PropertyType);
        Assert.DoesNotContain(typeof(IDynamicGraphEdge).GetMethods(), method => !method.IsSpecialName);
        Assert.DoesNotContain(
            actual,
            property => property.PropertyType.IsGenericType
                && property.PropertyType.GetGenericTypeDefinition() == typeof(ILogicalUnit<,>));
    }

    [Fact]
    public void DynamicGraphTopology_RequiresMaterializedReadOnlyListsAndRejectsLazyEnumerableShape()
    {
        // Constructor parameter order and types reject an arbitrary lazy IEnumerable boundary.
        var constructor = Assert.Single(typeof(DynamicGraphTopology).GetConstructors());
        var actual = constructor.GetParameters()
            .Select(parameter => (parameter.Name!, parameter.ParameterType))
            .ToArray();
        Assert.Equal(
            new (string Name, Type Type)[]
            {
                ("tenantId", typeof(string)),
                ("domainTag", typeof(string)),
                ("contextId", typeof(string)),
                ("nodes", typeof(IReadOnlyList<IDynamicGraphNode>)),
                ("edges", typeof(IReadOnlyList<IDynamicGraphEdge>)),
                ("order", typeof(IReadOnlyList<DynamicGraphNodeOrder>))
            },
            actual);

        // Reflection cannot smuggle a lazy-only sequence into the materialized node parameter.
        var enumerationCount = 0;
        IEnumerable<IDynamicGraphNode> LazyNodes()
        {
            enumerationCount++;
            yield return CreateNode("node-a");
        }

        Assert.ThrowsAny<ArgumentException>(() => constructor.Invoke(
            [
                TenantId,
                DomainTag,
                ContextId,
                LazyNodes(),
                Array.Empty<IDynamicGraphEdge>(),
                Array.Empty<DynamicGraphNodeOrder>()
            ]));
        Assert.Equal(0, enumerationCount);
    }

    [Fact]
    public void DynamicGraphTopology_ListEdgesFromHasExactSurfaceOrderIdentityAndFreshArraySemantics()
    {
        var declaredMethods = typeof(DynamicGraphTopology).GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();
        var method = Assert.Single(declaredMethods);
        Assert.Equal("ListEdgesFrom", method.Name);
        Assert.Equal(typeof(IDynamicGraphEdge[]), method.ReturnType);
        var parameter = Assert.Single(method.GetParameters());
        Assert.Equal("nodeId", parameter.Name);
        Assert.Equal(typeof(string), parameter.ParameterType);
        Assert.False(parameter.IsOptional);

        var topology = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c")],
            [
                CreateEdge("edge-z", "node-a", ["node-c"], GraphEdgeKind.FanOut),
                CreateEdge("edge-b", "node-b", ["node-c"], GraphEdgeKind.FanOut),
                CreateEdge("edge-a", "node-a", ["node-b"], GraphEdgeKind.FanOut)
            ],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1), CreateOrder("node-c", 2)]);

        var first = topology.ListEdgesFrom("node-a");
        var second = topology.ListEdgesFrom("node-a");

        Assert.Equal(["edge-z", "edge-a"], first.Select(edge => edge.EdgeId));
        Assert.Empty(topology.ListEdgesFrom("NODE-A"));
        Assert.NotSame(first, second);
        Assert.Same(topology.Edges[0], first[0]);
        Assert.Same(topology.Edges[2], first[1]);
        Assert.Same(first[0], second[0]);
        Assert.Same(first[1], second[1]);

        first[0] = first[1];
        var afterMutation = topology.ListEdgesFrom("node-a");
        Assert.Equal(["edge-z", "edge-b", "edge-a"], topology.Edges.Select(edge => edge.EdgeId));
        Assert.Equal(["edge-z", "edge-a"], afterMutation.Select(edge => edge.EdgeId));
        Assert.Same(topology.Edges[0], afterMutation[0]);
        Assert.Same(topology.Edges[2], afterMutation[1]);

        var topologyWithNullCarrier = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b")],
            new IDynamicGraphEdge[]
            {
                null!,
                CreateEdge("edge-valid", "node-a", ["node-b"], GraphEdgeKind.FanOut)
            },
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1)]);
        var validation = DynamicGraphValidation.Validate(topologyWithNullCarrier);
        Assert.False(validation.IsSuccess);
        Assert.Equal("DYNAMIC_GRAPH_EDGE_REQUIRED", validation.ErrorCode);
        var nullSafeResult = topologyWithNullCarrier.ListEdgesFrom("node-a");
        var retainedEdge = Assert.Single(nullSafeResult);
        Assert.Equal("edge-valid", retainedEdge.EdgeId);
        Assert.Same(topologyWithNullCarrier.Edges[1], retainedEdge);
    }

    [Fact]
    public void DynamicGraphTopology_DeepSnapshotsObjectsScalarsAndNestedLists()
    {
        // Mutable implementations and nested lists prove the topology severs every caller-owned object path.
        var originalDescriptor = CreateDescriptor("node-a");
        var mutableNode = new MutableNode("node-a", originalDescriptor);
        var nodes = new List<IDynamicGraphNode>
        {
            mutableNode,
            CreateNode("node-b"),
            CreateNode("node-c")
        };
        var targetIds = new List<string> { "node-b", "node-c" };
        var leakageSignals = new List<string>(RequiredLeakageSignals);
        var mutableEdge = CreateMutableEdge("edge-a", "node-a", targetIds, GraphEdgeKind.FanOut);
        mutableEdge.ExcludedLeakageSignals = leakageSignals;
        var edges = new List<IDynamicGraphEdge> { mutableEdge };
        var originalOrder = CreateOrder("node-a", 0);
        var order = new List<DynamicGraphNodeOrder>
        {
            originalOrder,
            CreateOrder("node-b", 1),
            CreateOrder("node-c", 2)
        };

        var topology = CreateTopology(nodes, edges, order);

        // Mutate object scalars, nested lists, record slots, and all outer caller collections after construction.
        mutableNode.NodeId = "node-mutated";
        mutableNode.Descriptor = CreateDescriptor("node-mutated", authorityRole: "arbitrary_role");
        mutableEdge.FromNodeId = "node-mutated";
        mutableEdge.Kind = GraphEdgeKind.ReturnLoop;
        mutableEdge.AuthorityUnitId = "unit-mutated";
        mutableEdge.AuthorityUnitLevel = "L6";
        mutableEdge.BaselineRole = "final_authority";
        targetIds.Clear();
        targetIds.Add("node-mutated");
        leakageSignals.Clear();
        order[0] = originalOrder with { Position = 99 };
        nodes.Clear();
        edges.Clear();
        order.Clear();

        // The topology retains its own immutable carriers and the original valid semantic values.
        Assert.Equal(3, topology.Nodes.Count);
        var snapshotNode = topology.Nodes[0];
        Assert.Equal("node-a", snapshotNode.NodeId);
        Assert.Equal("node-a", snapshotNode.Descriptor.NodeId);
        Assert.Equal("baseline_candidate_generator", snapshotNode.Descriptor.AuthorityRole);
        Assert.NotSame(mutableNode, snapshotNode);
        Assert.NotSame(originalDescriptor, snapshotNode.Descriptor);

        var snapshotEdge = Assert.Single(topology.Edges);
        Assert.Equal("node-a", snapshotEdge.FromNodeId);
        Assert.Equal(GraphEdgeKind.FanOut, snapshotEdge.Kind);
        Assert.Equal("unit-edge-authority-v1", snapshotEdge.AuthorityUnitId);
        Assert.Equal("L2", snapshotEdge.AuthorityUnitLevel);
        Assert.Equal("candidate_generator", snapshotEdge.BaselineRole);
        Assert.Equal(["node-b", "node-c"], snapshotEdge.ToNodeIds);
        Assert.Equal(RequiredLeakageSignals, snapshotEdge.ExcludedLeakageSignals);
        Assert.NotSame(mutableEdge, snapshotEdge);
        Assert.NotSame(targetIds, snapshotEdge.ToNodeIds);
        Assert.NotSame(leakageSignals, snapshotEdge.ExcludedLeakageSignals);

        Assert.Equal([0, 1, 2], topology.Order.Select(entry => entry.Position));
        Assert.NotSame(originalOrder, topology.Order[0]);
        Assert.True(DynamicGraphValidation.Validate(topology).IsSuccess);

        // Both outer and nested exposed collections reject mutation attempts.
        var nodeCollection = Assert.IsAssignableFrom<ICollection<IDynamicGraphNode>>(topology.Nodes);
        var edgeCollection = Assert.IsAssignableFrom<ICollection<IDynamicGraphEdge>>(topology.Edges);
        var orderCollection = Assert.IsAssignableFrom<ICollection<DynamicGraphNodeOrder>>(topology.Order);
        var targetCollection = Assert.IsAssignableFrom<ICollection<string>>(snapshotEdge.ToNodeIds);
        var leakageCollection = Assert.IsAssignableFrom<ICollection<string>>(snapshotEdge.ExcludedLeakageSignals);
        Assert.True(nodeCollection.IsReadOnly);
        Assert.True(edgeCollection.IsReadOnly);
        Assert.True(orderCollection.IsReadOnly);
        Assert.True(targetCollection.IsReadOnly);
        Assert.True(leakageCollection.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => targetCollection.Add("node-x"));
        Assert.Throws<NotSupportedException>(() => leakageCollection.Add("hidden_signal"));
    }

    [Fact]
    public void DynamicGraphTopology_PreservesNullCarriersForValidatorFailures()
    {
        // Deep snapshot construction must not replace validator diagnostics with constructor null exceptions.
        var nullNode = CreateTopology(
            new IDynamicGraphNode[] { null! },
            [],
            []);
        AssertFailure(nullNode, "DYNAMIC_GRAPH_NODE_REQUIRED");

        var nullDescriptor = CreateTopology(
            [new MutableNode("node-a", null!)],
            [],
            [CreateOrder("node-a", 0)]);
        AssertFailure(nullDescriptor, "DYNAMIC_GRAPH_NODE_DESCRIPTOR_REQUIRED");

        var nullEdge = CreateTopology(
            [CreateNode("node-a")],
            new IDynamicGraphEdge[] { null! },
            [CreateOrder("node-a", 0)]);
        AssertFailure(nullEdge, "DYNAMIC_GRAPH_EDGE_REQUIRED");

        var nullOrder = CreateTopology(
            [CreateNode("node-a")],
            [],
            new DynamicGraphNodeOrder[] { null! });
        AssertFailure(nullOrder, "DYNAMIC_GRAPH_ORDER_ENTRY_REQUIRED");

        var edgeWithNullTargets = CreateMutableEdge("edge-null-targets", "node-a", null!, GraphEdgeKind.FanOut);
        var nullTargets = CreateTopology(
            [CreateNode("node-a")],
            [edgeWithNullTargets],
            [CreateOrder("node-a", 0)]);
        AssertFailure(nullTargets, "DYNAMIC_GRAPH_EDGE_TARGETS_REQUIRED");
    }

    [Fact]
    public void Validate_AcceptsGenuineFanOutFanInManyToManyAndMultiOutputDag()
    {
        // Nine nodes carry real one-to-many, grouped many-to-one, grouped N-to-M, and multi-output relations.
        var nodes = new IDynamicGraphNode[]
        {
            CreateNode("node-a"),
            CreateNode("node-b"),
            CreateNode("node-c"),
            CreateNode("node-d"),
            CreateNode("node-e"),
            CreateNode("node-f"),
            CreateNode("node-g"),
            CreateNode("node-h"),
            CreateNode("node-i")
        };
        var order = nodes.Select((node, position) => CreateOrder(node.NodeId, position)).ToArray();
        var edges = new IDynamicGraphEdge[]
        {
            CreateEdge("edge-fan-out", "node-a", ["node-b", "node-c", "node-g"], GraphEdgeKind.FanOut),
            CreateEdge("edge-fan-in-b", "node-b", ["node-d"], GraphEdgeKind.FanIn),
            CreateEdge("edge-fan-in-c", "node-c", ["node-d"], GraphEdgeKind.FanIn),
            CreateEdge("edge-many-d", "node-d", ["node-e", "node-f"], GraphEdgeKind.ManyToMany),
            CreateEdge("edge-many-g", "node-g", ["node-f", "node-e"], GraphEdgeKind.ManyToMany),
            CreateEdge("edge-multi-output", "node-e", ["node-h", "node-i"], GraphEdgeKind.MultiOutput)
        };

        var topology = CreateTopology(nodes, edges, order);
        var result = DynamicGraphValidation.Validate(topology);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Same(topology, result.Value);
    }

    [Fact]
    public void Validate_AcceptsOneTargetFanOutAndOneTargetMultiOutput()
    {
        // FanOut is one-source to one-or-more targets, so a single explicit target is structurally valid.
        var fanOut = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b")],
            [CreateEdge("edge-fan-out-one", "node-a", ["node-b"], GraphEdgeKind.FanOut)],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1)]);
        var fanOutResult = DynamicGraphValidation.Validate(fanOut);
        Assert.True(fanOutResult.IsSuccess, fanOutResult.ErrorMessage);

        // MultiOutput payload multiplicity belongs to the payload contract, not the topology target count.
        var multiOutput = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b")],
            [CreateEdge("edge-multi-output-one", "node-a", ["node-b"], GraphEdgeKind.MultiOutput)],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1)]);
        var multiOutputResult = DynamicGraphValidation.Validate(multiOutput);
        Assert.True(multiOutputResult.IsSuccess, multiOutputResult.ErrorMessage);
    }

    [Fact]
    public void Validate_AcceptsOrdinaryForwardArcWithDeclarativeCorrectionBackEdge()
    {
        // A->{B,C} is the ordinary DAG; B->A is a nonparallel correction arc initiated by L3 for later L4 execution.
        var topology = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c")],
            [
                CreateEdge("edge-a-forward", "node-a", ["node-b", "node-c"], GraphEdgeKind.FanOut),
                CreateEdge("edge-b-correction", "node-b", ["node-a"], GraphEdgeKind.ReturnLoop)
            ],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1), CreateOrder("node-c", 2)]);

        var result = DynamicGraphValidation.Validate(topology);
        Assert.True(result.IsSuccess, result.ErrorMessage);
    }

    [Fact]
    public void Validate_RejectsOrdinaryCycleEvenWhenEveryFanOutHasRealCardinality()
    {
        // Both ordinary FanOut edges have two targets while A<->B still forms a real structural cycle.
        var topology = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c")],
            [
                CreateEdge("edge-a", "node-a", ["node-b", "node-c"], GraphEdgeKind.FanOut),
                CreateEdge("edge-b", "node-b", ["node-a", "node-c"], GraphEdgeKind.FanOut)
            ],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1), CreateOrder("node-c", 2)]);

        AssertFailure(topology, "DYNAMIC_GRAPH_CYCLE_DETECTED");
    }

    [Fact]
    public void Validate_RejectsSelfReturnLoop()
    {
        var topology = CreateTopology(
            [CreateNode("node-a")],
            [CreateEdge("edge-self", "node-a", ["node-a"], GraphEdgeKind.ReturnLoop)],
            [CreateOrder("node-a", 0)]);

        AssertFailure(topology, "DYNAMIC_GRAPH_SELF_EDGE");
    }

    [Fact]
    public void Validate_RejectsForwardReturnLoop()
    {
        var topology = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b")],
            [CreateEdge("edge-forward-loop", "node-a", ["node-b"], GraphEdgeKind.ReturnLoop)],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1)]);

        AssertFailure(topology, "DYNAMIC_GRAPH_RETURN_LOOP_DIRECTION_INVALID");
    }

    [Fact]
    public void Validate_RejectsParallelEligibleReturnLoop()
    {
        var topology = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b")],
            [CreateEdge("edge-parallel-loop", "node-b", ["node-a"], GraphEdgeKind.ReturnLoop, parallelEligible: true)],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1)]);

        AssertFailure(topology, "DYNAMIC_GRAPH_RETURN_LOOP_PARALLEL_FORBIDDEN");
    }

    [Fact]
    public void Validate_RejectsMultiTargetReturnLoop()
    {
        var topology = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c")],
            [CreateEdge("edge-multi-loop", "node-c", ["node-a", "node-b"], GraphEdgeKind.ReturnLoop)],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1), CreateOrder("node-c", 2)]);

        AssertFailure(topology, "DYNAMIC_GRAPH_RETURN_LOOP_CARDINALITY_INVALID");
    }

    [Theory]
    [InlineData(GraphEdgeKind.FanIn, "DYNAMIC_GRAPH_FAN_IN_CARDINALITY_INVALID")]
    [InlineData(GraphEdgeKind.ManyToMany, "DYNAMIC_GRAPH_MANY_TO_MANY_CARDINALITY_INVALID")]
    public void Validate_RejectsGroupedRelationsThatDoNotReachRealCardinality(GraphEdgeKind kind, string errorCode)
    {
        // FanIn uses one source into its sole target; ManyToMany uses one source into two targets.
        IReadOnlyList<string> targets = kind == GraphEdgeKind.ManyToMany
            ? ["node-b", "node-c"]
            : ["node-b"];
        var topology = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c")],
            [CreateEdge("edge-cardinality", "node-a", targets, kind)],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1), CreateOrder("node-c", 2)]);

        AssertFailure(topology, errorCode);
    }

    [Fact]
    public void Validate_RejectsFanInMultiTargetAndManyToManySingleTargetShapes()
    {
        var nodes = new IDynamicGraphNode[] { CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c") };
        var order = new[] { CreateOrder("node-a", 0), CreateOrder("node-b", 1), CreateOrder("node-c", 2) };

        var fanIn = CreateTopology(
            nodes,
            [CreateEdge("edge-fan-in", "node-a", ["node-b", "node-c"], GraphEdgeKind.FanIn)],
            order);
        AssertFailure(fanIn, "DYNAMIC_GRAPH_FAN_IN_CARDINALITY_INVALID");

        var manyToMany = CreateTopology(
            nodes,
            [CreateEdge("edge-many", "node-a", ["node-b"], GraphEdgeKind.ManyToMany)],
            order);
        AssertFailure(manyToMany, "DYNAMIC_GRAPH_MANY_TO_MANY_CARDINALITY_INVALID");
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(0, 2)]
    public void Validate_RejectsMissingZeroOrGappedOrderPositions(int firstPosition, int secondPosition)
    {
        var topology = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b")],
            [],
            [CreateOrder("node-a", firstPosition), CreateOrder("node-b", secondPosition)]);

        AssertFailure(topology, "DYNAMIC_GRAPH_ORDER_NOT_CONTIGUOUS");
    }

    [Fact]
    public void Validate_RejectsDuplicateOrderPositions()
    {
        var topology = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b")],
            [],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 0)]);

        AssertFailure(topology, "DYNAMIC_GRAPH_DUPLICATE_ORDER_POSITION");
    }

    [Fact]
    public void Validate_RejectsArbitraryNodeAuthorityRole()
    {
        var topology = CreateTopology(
            [CreateNode("node-a", authorityRole: "model_owner")],
            [],
            [CreateOrder("node-a", 0)]);

        AssertFailure(topology, "DYNAMIC_GRAPH_NODE_AUTHORITY_ROLE_INVALID");
    }

    [Fact]
    public void Validate_RejectsUntrainedNodeWithTrainedAuthorityRole()
    {
        var topology = CreateTopology(
            [CreateNode("node-a", authorityRole: "trained_node")],
            [],
            [CreateOrder("node-a", 0)]);

        AssertFailure(topology, "DYNAMIC_GRAPH_NODE_AUTHORITY_STATE_MISMATCH");
    }

    [Fact]
    public void Validate_RejectsTrainedNodeWithBaselineAuthorityRole()
    {
        // Checkpoint and manifest are populated so authority-role coherence is the observed defect.
        var topology = CreateTopology(
            [CreateNode(
                "node-a",
                authorityRole: "baseline_candidate_generator",
                isTrained: true,
                checkpointRef: "checkpoint-a",
                manifestRef: "manifest-a")],
            [],
            [CreateOrder("node-a", 0)]);

        AssertFailure(topology, "DYNAMIC_GRAPH_NODE_AUTHORITY_STATE_MISMATCH");
    }

    [Fact]
    public void Validate_RejectsSemanticQualityClaimWithoutTrainedAuthorityRole()
    {
        // Trained evidence is present, isolating the additional semantic-claim role requirement.
        var topology = CreateTopology(
            [CreateNode(
                "node-a",
                authorityRole: "diagnostics_only",
                isTrained: true,
                runtimeSemanticQualityClaimed: true,
                checkpointRef: "checkpoint-a",
                manifestRef: "manifest-a")],
            [],
            [CreateOrder("node-a", 0)]);

        AssertFailure(topology, "DYNAMIC_GRAPH_NODE_SEMANTIC_CLAIM_ROLE_REQUIRED");
    }

    [Fact]
    public void Validate_AcceptsCoherentTrainedBaselineAndDiagnosticsNodeRoles()
    {
        var topology = CreateTopology(
            [
                CreateNode(
                    "node-trained",
                    authorityRole: "trained_node",
                    isTrained: true,
                    runtimeSemanticQualityClaimed: true,
                    checkpointRef: "checkpoint-trained",
                    manifestRef: "manifest-trained"),
                CreateNode("node-baseline", authorityRole: "baseline_candidate_generator"),
                CreateNode("node-diagnostics", authorityRole: "diagnostics_only")
            ],
            [],
            [
                CreateOrder("node-trained", 0),
                CreateOrder("node-baseline", 1),
                CreateOrder("node-diagnostics", 2)
            ]);

        var result = DynamicGraphValidation.Validate(topology);
        Assert.True(result.IsSuccess, result.ErrorMessage);
    }

    [Fact]
    public void Validate_RejectsL6EdgeAuthorityBypass()
    {
        var topology = CreateValidFanOutTopology(
            CreateEdge(
                "edge-l6",
                "node-a",
                ["node-b", "node-c"],
                GraphEdgeKind.FanOut,
                authorityUnitLevel: "L6"));

        AssertFailure(topology, "DYNAMIC_GRAPH_EDGE_AUTHORITY_LEVEL_INVALID");
    }

    [Fact]
    public void Validate_RejectsFinalAuthorityBaselineRole()
    {
        var topology = CreateValidFanOutTopology(
            CreateEdge(
                "edge-baseline-role",
                "node-a",
                ["node-b", "node-c"],
                GraphEdgeKind.FanOut,
                baselineRole: "final_authority"));

        AssertFailure(topology, "DYNAMIC_GRAPH_EDGE_BASELINE_ROLE_INVALID");
    }

    [Fact]
    public void Validate_RejectsMissingBaselineIdentity()
    {
        var topology = CreateValidFanOutTopology(
            CreateEdge(
                "edge-baseline-id",
                "node-a",
                ["node-b", "node-c"],
                GraphEdgeKind.FanOut,
                baselineUnitId: null));

        AssertFailure(topology, "DYNAMIC_GRAPH_EDGE_BASELINE_UNIT_REQUIRED");
    }

    [Fact]
    public void Validate_AcceptsExactOrderedLeakageExclusionVocabulary()
    {
        var topology = CreateValidFanOutTopology(
            CreateEdge(
                "edge-leakage-canonical",
                "node-a",
                ["node-b", "node-c"],
                GraphEdgeKind.FanOut,
                excludedLeakageSignals: RequiredLeakageSignals.ToArray()));

        var result = DynamicGraphValidation.Validate(topology);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Same(topology, result.Value);
    }

    [Fact]
    public void AsyncCandidateTimingCarriers_AreRequiredNonOptionalLongs()
    {
        foreach (var type in new[] { typeof(GraphCandidateOutput), typeof(AsyncCandidateBatch) })
        {
            var parameter = Assert.Single(type.GetConstructors()).GetParameters()
                .Single(candidate => candidate.Name == "ElapsedMs");
            Assert.Equal(typeof(long), parameter.ParameterType);
            Assert.False(parameter.IsOptional);
            Assert.False(parameter.HasDefaultValue);
        }
    }

    [Fact]
    public void LeakageSignalVocabulary_ReturnsDefensiveCanonicalSnapshots()
    {
        string[] expected =
            ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];
        var first = DynamicGraphEdgeAuthorityLeakageSignalVocabulary.CreateSnapshot();
        var second = DynamicGraphEdgeAuthorityLeakageSignalVocabulary.CreateSnapshot();

        Assert.NotSame(first, second);
        Assert.Equal(expected, first);
        Assert.Equal(expected, second);

        first[0] = "mutated_candidate_index";
        var fresh = DynamicGraphEdgeAuthorityLeakageSignalVocabulary.CreateSnapshot();

        Assert.Equal(expected, second);
        Assert.Equal(expected, fresh);
        Assert.True(DynamicGraphEdgeAuthorityLeakageSignalVocabulary.IsExact(expected));
        Assert.True(DynamicGraphEdgeAuthorityLeakageSignalVocabulary.IsExact(fresh));
        Assert.False(DynamicGraphEdgeAuthorityLeakageSignalVocabulary.IsExact(first));
    }

    [Fact]
    public void Validate_RejectsNullLeakageExclusionCarrierWithStableCode()
    {
        var canonicalEdge = Assert.IsType<TestEdge>(CreateEdge(
            "edge-leakage-null",
            "node-a",
            ["node-b", "node-c"],
            GraphEdgeKind.FanOut));
        var topology = CreateValidFanOutTopology(
            canonicalEdge with { ExcludedLeakageSignals = null! });

        AssertFailure(topology, "DYNAMIC_GRAPH_EDGE_LEAKAGE_EXCLUSIONS_REQUIRED");
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("reordered")]
    [InlineData("duplicate")]
    [InlineData("superset")]
    public void Validate_RejectsNonExactLeakageExclusionVocabulary(string mutation)
    {
        var leakageSignals = mutation switch
        {
            "missing" => RequiredLeakageSignals.Where((_, index) => index != 2).ToArray(),
            "reordered" => RequiredLeakageSignals.Reverse().ToArray(),
            "duplicate" => RequiredLeakageSignals.Concat([RequiredLeakageSignals[0]]).ToArray(),
            "superset" => RequiredLeakageSignals.Concat(["extra_signal"]).ToArray(),
            _ => throw new InvalidOperationException("Unknown leakage vocabulary mutation.")
        };
        var topology = CreateValidFanOutTopology(
            CreateEdge(
                "edge-leakage",
                "node-a",
                ["node-b", "node-c"],
                GraphEdgeKind.FanOut,
                excludedLeakageSignals: leakageSignals));

        AssertFailure(topology, "DYNAMIC_GRAPH_EDGE_LEAKAGE_EXCLUSION_REQUIRED");
    }

    [Fact]
    public void Validate_RejectsMissingEdgeAuthority()
    {
        var topology = CreateValidFanOutTopology(
            CreateEdge(
                "edge-no-authority",
                "node-a",
                ["node-b", "node-c"],
                GraphEdgeKind.FanOut,
                authorityUnitId: " "));

        AssertFailure(topology, "DYNAMIC_GRAPH_EDGE_AUTHORITY_REQUIRED");
    }

    [Fact]
    public void Validate_RejectsDeterministicFinalAuthorityBypass()
    {
        var topology = CreateValidFanOutTopology(
            CreateEdge(
                "edge-shortcut",
                "node-a",
                ["node-b", "node-c"],
                GraphEdgeKind.FanOut,
                deterministicFinalAuthority: true));

        AssertFailure(topology, "DYNAMIC_GRAPH_EDGE_DETERMINISTIC_AUTHORITY_FORBIDDEN");
    }

    [Fact]
    public void Validate_RejectsDanglingEdgeEndpoints()
    {
        var topology = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b")],
            [CreateEdge("edge-dangling", "node-a", ["node-b", "node-missing"], GraphEdgeKind.FanOut)],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1)]);

        AssertFailure(topology, "DYNAMIC_GRAPH_DANGLING_TARGET_NODE");
    }

    [Fact]
    public void Validate_RejectsDuplicateNodeIds()
    {
        var topology = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-a")],
            [],
            [CreateOrder("node-a", 0), CreateOrder("node-a", 1, orderId: "order-node-a-copy")]);

        AssertFailure(topology, "DYNAMIC_GRAPH_DUPLICATE_NODE_ID");
    }

    [Fact]
    public void Validate_RejectsCrossTenantNodeOrder()
    {
        var topology = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b")],
            [],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1, tenantId: "tenant-foreign")]);

        AssertFailure(topology, "DYNAMIC_GRAPH_TENANT_MISMATCH");
    }

    [Fact]
    public void Validate_RejectsCrossDomainNodes()
    {
        var topology = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b", domainTag: "foreign-domain")],
            [],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1)]);

        AssertFailure(topology, "DYNAMIC_GRAPH_DOMAIN_MISMATCH");
    }

    [Fact]
    public void Validate_RejectsNullAndBlankRequiredTopologyCarriers()
    {
        var nullResult = DynamicGraphValidation.Validate(null);
        Assert.False(nullResult.IsSuccess);
        Assert.Equal("DYNAMIC_GRAPH_TOPOLOGY_REQUIRED", nullResult.ErrorCode);

        var blankTenant = CreateTopology(
            [CreateNode("node-a")],
            [],
            [CreateOrder("node-a", 0)],
            tenantId: "   ");
        AssertFailure(blankTenant, "DYNAMIC_GRAPH_TENANT_REQUIRED");
    }

    // AssertCanonicalRecordShape pins both positional constructor identity and public property identity.
    private static void AssertCanonicalRecordShape<T>((string Name, Type Type)[] expected)
    {
        var constructor = Assert.Single(typeof(T).GetConstructors());
        var parameters = constructor.GetParameters()
            .Select(parameter => (parameter.Name!, parameter.ParameterType))
            .ToArray();
        Assert.Equal(expected, parameters);

        var properties = typeof(T).GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType))
            .ToArray();
        Assert.Equal(expected.OrderBy(item => item.Name, StringComparer.Ordinal), properties);
    }

    // CreateDescriptor builds one honest untrained node passport suitable for foundational topology fixtures.
    private static DynamicGraphNodeDescriptor CreateDescriptor(
        string nodeId,
        string domainTag = DomainTag,
        string authorityRole = "baseline_candidate_generator",
        bool isTrained = false,
        bool runtimeSemanticQualityClaimed = false,
        string? checkpointRef = null,
        string? manifestRef = null)
        => new(
            NodeId: nodeId,
            DomainTag: domainTag,
            TInputTypeName: typeof(string).Name,
            TOutputTypeName: typeof(GraphNodeCandidate).Name,
            IsTrained: isTrained,
            DeterministicFinalAuthority: false,
            RuntimeSemanticQualityClaimed: runtimeSemanticQualityClaimed,
            AuthorityRole: authorityRole,
            CheckpointRef: checkpointRef,
            ManifestRef: manifestRef);

    // CreateNode wraps the canonical descriptor in the non-generic topology identity boundary.
    private static IDynamicGraphNode CreateNode(
        string nodeId,
        string domainTag = DomainTag,
        string authorityRole = "baseline_candidate_generator",
        bool isTrained = false,
        bool runtimeSemanticQualityClaimed = false,
        string? checkpointRef = null,
        string? manifestRef = null)
        => new TestNode(
            nodeId,
            CreateDescriptor(
                nodeId,
                domainTag,
                authorityRole,
                isTrained,
                runtimeSemanticQualityClaimed,
                checkpointRef,
                manifestRef));

    // CreateOrder binds one node to the standard fixture scope and a stable deterministic position.
    private static DynamicGraphNodeOrder CreateOrder(
        string nodeId,
        int position,
        string? orderId = null,
        string tenantId = TenantId,
        string domainTag = DomainTag,
        string contextId = ContextId)
        => new(
            OrderId: orderId ?? $"order-{nodeId}",
            NodeId: nodeId,
            TenantId: tenantId,
            DomainTag: domainTag,
            ContextId: contextId,
            Position: position);

    // CreateEdge builds one canonical declarative relation with explicit L2 and non-authoritative baseline labels.
    private static IDynamicGraphEdge CreateEdge(
        string edgeId,
        string fromNodeId,
        IReadOnlyList<string> toNodeIds,
        GraphEdgeKind kind,
        bool? parallelEligible = null,
        string authorityUnitId = "unit-edge-authority-v1",
        string authorityUnitLevel = "L2",
        bool deterministicFinalAuthority = false,
        string? baselineUnitId = "unit-edge-baseline-v1",
        string baselineRole = "candidate_generator",
        IReadOnlyList<string>? excludedLeakageSignals = null)
        => new TestEdge(
            EdgeId: edgeId,
            FromNodeId: fromNodeId,
            ToNodeIds: toNodeIds,
            Kind: kind,
            ParallelEligible: parallelEligible ?? (kind != GraphEdgeKind.ReturnLoop),
            AuthorityUnitId: authorityUnitId,
            AuthorityUnitLevel: authorityUnitLevel,
            DeterministicFinalAuthority: deterministicFinalAuthority,
            BaselineUnitId: baselineUnitId,
            BaselineRole: baselineRole,
            ExcludedLeakageSignals: excludedLeakageSignals ?? RequiredLeakageSignals,
            SourceProof: "fixture-source-proof",
            CutoverProof: "fixture-cutover-not-applicable",
            AblationProofRef: "fixture-ablation-pending");

    // CreateMutableEdge supplies a caller-owned implementation for deep-snapshot and null-carrier tests.
    private static MutableEdge CreateMutableEdge(
        string edgeId,
        string fromNodeId,
        IReadOnlyList<string> toNodeIds,
        GraphEdgeKind kind)
        => new()
        {
            EdgeId = edgeId,
            FromNodeId = fromNodeId,
            ToNodeIds = toNodeIds,
            Kind = kind,
            ParallelEligible = kind != GraphEdgeKind.ReturnLoop,
            AuthorityUnitId = "unit-edge-authority-v1",
            AuthorityUnitLevel = "L2",
            DeterministicFinalAuthority = false,
            BaselineUnitId = "unit-edge-baseline-v1",
            BaselineRole = "candidate_generator",
            ExcludedLeakageSignals = new List<string>(RequiredLeakageSignals),
            SourceProof = "fixture-source-proof",
            CutoverProof = "fixture-cutover-not-applicable",
            AblationProofRef = "fixture-ablation-pending"
        };

    // CreateValidFanOutTopology isolates one authority mutation behind an otherwise valid real FanOut shape.
    private static DynamicGraphTopology CreateValidFanOutTopology(IDynamicGraphEdge edge)
        => CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c")],
            [edge],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1), CreateOrder("node-c", 2)]);

    // CreateTopology centralizes fixture scope while allowing each test to vary one rejection carrier.
    private static DynamicGraphTopology CreateTopology(
        IReadOnlyList<IDynamicGraphNode> nodes,
        IReadOnlyList<IDynamicGraphEdge> edges,
        IReadOnlyList<DynamicGraphNodeOrder> order,
        string tenantId = TenantId,
        string domainTag = DomainTag,
        string contextId = ContextId)
        => new(tenantId, domainTag, contextId, nodes, edges, order);

    // AssertFailure pins the deterministic error code produced by one malformed topology.
    private static void AssertFailure(DynamicGraphTopology topology, string errorCode)
    {
        var result = DynamicGraphValidation.Validate(topology);
        Assert.False(result.IsSuccess);
        Assert.Equal(errorCode, result.ErrorCode);
    }

    // IsTrainingOrMutationName identifies verbs forbidden from node inference contracts.
    private static bool IsTrainingOrMutationName(string methodName)
        => methodName.Contains("Train", StringComparison.OrdinalIgnoreCase)
            || methodName.Contains("Evaluate", StringComparison.OrdinalIgnoreCase)
            || methodName.Contains("Export", StringComparison.OrdinalIgnoreCase)
            || methodName.Contains("Import", StringComparison.OrdinalIgnoreCase)
            || methodName.Contains("Mutat", StringComparison.OrdinalIgnoreCase);

    // TestNode is the minimal immutable heterogeneous node implementation used by ordinary fixtures.
    private sealed record TestNode(
        string NodeId,
        DynamicGraphNodeDescriptor Descriptor) : IDynamicGraphNode;

    // TestEdge is the minimal immutable canonical relation implementation used by ordinary fixtures.
    private sealed record TestEdge(
        string EdgeId,
        string FromNodeId,
        IReadOnlyList<string> ToNodeIds,
        GraphEdgeKind Kind,
        bool ParallelEligible,
        string AuthorityUnitId,
        string AuthorityUnitLevel,
        bool DeterministicFinalAuthority,
        string? BaselineUnitId,
        string BaselineRole,
        IReadOnlyList<string> ExcludedLeakageSignals,
        string SourceProof,
        string CutoverProof,
        string AblationProofRef) : IDynamicGraphEdge;

    // MutableNode proves topology snapshots do not retain caller node implementations or descriptors.
    private sealed class MutableNode : IDynamicGraphNode
    {
        public MutableNode(string nodeId, DynamicGraphNodeDescriptor descriptor)
        {
            NodeId = nodeId;
            Descriptor = descriptor;
        }

        public string NodeId { get; set; }

        public DynamicGraphNodeDescriptor Descriptor { get; set; }
    }

    // MutableEdge proves topology snapshots do not retain caller edge implementations, scalars, or nested lists.
    private sealed class MutableEdge : IDynamicGraphEdge
    {
        public string EdgeId { get; set; } = null!;

        public string FromNodeId { get; set; } = null!;

        public IReadOnlyList<string> ToNodeIds { get; set; } = null!;

        public GraphEdgeKind Kind { get; set; }

        public bool ParallelEligible { get; set; }

        public string AuthorityUnitId { get; set; } = null!;

        public string AuthorityUnitLevel { get; set; } = null!;

        public bool DeterministicFinalAuthority { get; set; }

        public string? BaselineUnitId { get; set; }

        public string BaselineRole { get; set; } = null!;

        public IReadOnlyList<string> ExcludedLeakageSignals { get; set; } = null!;

        public string SourceProof { get; set; } = null!;

        public string CutoverProof { get; set; } = null!;

        public string AblationProofRef { get; set; } = null!;
    }
}
