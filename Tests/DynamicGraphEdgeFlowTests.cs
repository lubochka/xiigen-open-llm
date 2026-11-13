// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using SemanticCompilerAdapter;
using Domain = SemanticCompiler29.Domain;

// These tests pin deterministic FanIn/ReturnLoop dispatch and the learned L3 target-handoff boundary.
public sealed class DynamicGraphEdgeFlowTests
{
    private const string RunId = "run-edge-001";
    private const string FlowId = "flow-dynamic-edge";
    private const string ScenarioId = "scenario-dynamic-edge";
    private const string MergeKey = "merge:dynamic-edge";
    private const string PathOrderRef = "path-order:dynamic-edge";
    private const string TenantId = "tenant-edge";
    private const string DomainTag = "software-development";
    private const string ContextId = "context-edge";

    private static readonly string[] RequiredLeakageSignals =
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
    public void PublicSurface_ExposesExactOwnerRecordsConstructorsFlowMethodAndAuthorityMaterializer()
    {
        AssertCanonicalRecordShape<EdgeTraversalInput>(
            [
                ("RunId", typeof(string)),
                ("EdgeId", typeof(string)),
                ("Source", typeof(SubPhaseRouterSourceDocument)),
                ("Context", typeof(ContextSignals)),
                ("UpstreamNodeOutput", typeof(NodeOutputToEdgeInputAdapter))
            ]);
        AssertCanonicalRecordShape<NodeOutputToEdgeInputAdapter>(
            [
                ("SourceNodeId", typeof(string)),
                ("UpstreamCandidateRefs", typeof(IReadOnlyList<string>)),
                ("UpstreamOutputHash", typeof(string)),
                ("ProjectionRule", typeof(string))
            ]);

        Assert.Contains(
            typeof(ILogicalFlow<EdgeTraversalInput>),
            typeof(DynamicGraphEdgeFlow).GetInterfaces());
        var constructors = typeof(DynamicGraphEdgeFlow).GetConstructors()
            .OrderBy(candidate => candidate.GetParameters().Length)
            .ToArray();
        Assert.Collection(
            constructors,
            legacyConstructor => Assert.Equal(
                new (string Name, Type Type)[]
                {
                    ("topology", typeof(DynamicGraphTopology)),
                    ("merger", typeof(IEdgeFanInMerger)),
                    (
                        "fanInInputMaterializer",
                        typeof(Func<
                            DynamicGraphTopology,
                            IDynamicGraphEdge,
                            EdgeTraversalInput,
                            OperationResult<GraphFanInMergeInput>>))
                },
                legacyConstructor.GetParameters()
                    .Select(parameter => (parameter.Name!, parameter.ParameterType))
                    .ToArray()),
            learnedAuthorityConstructor => Assert.Equal(
                new (string Name, Type Type)[]
                {
                    ("topology", typeof(DynamicGraphTopology)),
                    ("merger", typeof(IEdgeFanInMerger)),
                    (
                        "fanInInputMaterializer",
                        typeof(Func<
                            DynamicGraphTopology,
                            IDynamicGraphEdge,
                            EdgeTraversalInput,
                            OperationResult<GraphFanInMergeInput>>)),
                    ("edgeAuthorityPipeline", typeof(IDynamicGraphEdgeAuthorityExecutionPipeline))
                },
                learnedAuthorityConstructor.GetParameters()
                    .Select(parameter => (parameter.Name!, parameter.ParameterType))
                    .ToArray()));

        var method = Assert.Single(typeof(DynamicGraphEdgeFlow).GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(candidate => !candidate.IsSpecialName));
        Assert.Equal("ExecuteAsync", method.Name);
        Assert.Equal(typeof(Task<OperationResult<LogicalFlowResult>>), method.ReturnType);
        Assert.Equal(
            new (string Name, Type Type)[]
            {
                ("request", typeof(LogicalFlowRequest<EdgeTraversalInput>)),
                ("cancellationToken", typeof(CancellationToken))
            },
            method.GetParameters()
                .Select(parameter => (parameter.Name!, parameter.ParameterType))
                .ToArray());
        Assert.Empty(typeof(DynamicGraphEdgeFlow).GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(typeof(DynamicGraphEdgeFlow).GetFields(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));

        var forbiddenNames = new[]
        {
            "Train", "Learn", "Dpo", "Authority", "Traverse", "FollowTransition", "RunCandidates"
        };
        Assert.DoesNotContain(
            typeof(DynamicGraphEdgeFlow).GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly),
            candidate => forbiddenNames.Any(fragment =>
                candidate.Name.Contains(fragment, StringComparison.OrdinalIgnoreCase)));
        var materializerType = typeof(DeterministicDynamicGraphEdgeAuthorityInputMaterializer);
        Assert.True(materializerType.IsPublic);
        Assert.True(materializerType.IsSealed);
        Assert.Equal(
            [typeof(IDynamicGraphEdgeAuthorityInputMaterializer)],
            materializerType.GetInterfaces());
        var materializerConstructor = Assert.Single(materializerType.GetConstructors());
        Assert.Empty(materializerConstructor.GetParameters());
        var materializeMethod = Assert.Single(materializerType.GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(candidate => !candidate.IsSpecialName));
        Assert.Equal("Materialize", materializeMethod.Name);
        Assert.Equal(typeof(OperationResult<EdgeAuthorityInput>), materializeMethod.ReturnType);
        Assert.Equal(
            new (string Name, Type Type)[]
            {
                ("edge", typeof(IDynamicGraphEdge)),
                ("input", typeof(EdgeTraversalInput)),
                ("candidates", typeof(IReadOnlyList<EdgeCandidate>))
            },
            materializeMethod.GetParameters()
                .Select(parameter => (parameter.Name!, parameter.ParameterType))
                .ToArray());
        Assert.Empty(materializerType.GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(materializerType.GetFields(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));
    }

    [Fact]
    public void AuthorityMaterializer_RuntimeLeakageBoundaryAcceptsCanonicalOrderedVocabulary()
    {
        var edge = CreateEdge(
            "edge-a",
            "node-a",
            ["node-b"],
            GraphEdgeKind.FanOut,
            RequiredLeakageSignals.ToArray());
        var input = CreateTraversalInput(edgeId: edge.EdgeId, sourceNodeId: edge.FromNodeId);
        EdgeCandidate[] candidates =
        [
            new(
                TargetSubPhaseId: "node-b",
                EvidencePredicates: ["PREDICATE-A"],
                BaselineScore: 0.42d,
                AuthorityRole: "baseline_candidate_generator")
        ];

        var result = new DeterministicDynamicGraphEdgeAuthorityInputMaterializer()
            .Materialize(edge, input, candidates);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(RequiredLeakageSignals, result.Value!.ExcludedLeakageSignals);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("reordered")]
    [InlineData("duplicate")]
    [InlineData("superset")]
    public void AuthorityMaterializer_RuntimeLeakageBoundaryRejectsNonExactVocabularyWithStableCode(
        string mutation)
    {
        var leakageSignals = mutation switch
        {
            "missing" => RequiredLeakageSignals.Where((_, index) => index != 2).ToArray(),
            "reordered" => RequiredLeakageSignals.Reverse().ToArray(),
            "duplicate" => RequiredLeakageSignals.Concat([RequiredLeakageSignals[0]]).ToArray(),
            "superset" => RequiredLeakageSignals.Concat(["extra_signal"]).ToArray(),
            _ => throw new InvalidOperationException("Unknown leakage vocabulary mutation.")
        };
        var edge = CreateEdge(
            "edge-a",
            "node-a",
            ["node-b"],
            GraphEdgeKind.FanOut,
            leakageSignals);
        var input = CreateTraversalInput(edgeId: edge.EdgeId, sourceNodeId: edge.FromNodeId);
        EdgeCandidate[] candidates =
        [
            new(
                TargetSubPhaseId: "node-b",
                EvidencePredicates: ["PREDICATE-A"],
                BaselineScore: 0.42d,
                AuthorityRole: "baseline_candidate_generator")
        ];

        var result = new DeterministicDynamicGraphEdgeAuthorityInputMaterializer()
            .Materialize(edge, input, candidates);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("EDGE_AUTHORITY_LEAKAGE_BOUNDARY_INVALID", result.ErrorCode);
        Assert.Equal("Edge authority leakage boundary is invalid.", result.ErrorMessage);
    }

    [Fact]
    public void AuthorityMaterializer_RuntimeLeakageBoundaryRejectsNullCarrierWithStableCode()
    {
        var canonicalEdge = Assert.IsType<TestEdge>(CreateEdge(
            "edge-a",
            "node-a",
            ["node-b"],
            GraphEdgeKind.FanOut));
        var edge = canonicalEdge with { ExcludedLeakageSignals = null! };
        var input = CreateTraversalInput(edgeId: edge.EdgeId, sourceNodeId: edge.FromNodeId);
        EdgeCandidate[] candidates =
        [
            new(
                TargetSubPhaseId: "node-b",
                EvidencePredicates: ["PREDICATE-A"],
                BaselineScore: 0.42d,
                AuthorityRole: "baseline_candidate_generator")
        ];

        var result = new DeterministicDynamicGraphEdgeAuthorityInputMaterializer()
            .Materialize(edge, input, candidates);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("EDGE_AUTHORITY_LEAKAGE_BOUNDARY_INVALID", result.ErrorCode);
        Assert.Equal("Edge authority leakage boundary is invalid.", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_ValidTwoSourceFanInPassesExactArgumentsToRealMergerOnceAndReturnsWorstMergedResult()
    {
        var topology = CreateFanInTopology();
        var input = CreateTraversalInput();
        var request = CreateRequest(input);
        var mergeInput = CreateMergeInput(input.RunId);
        var expected = new DeterministicGraphFanInMerger().Merge(mergeInput);
        Assert.True(expected.IsSuccess, expected.ErrorMessage);
        var materializer = new MaterializerSpy((_, _, _) =>
            OperationResult<GraphFanInMergeInput>.Ok(mergeInput));
        var merger = new MergerSpy(value => new DeterministicGraphFanInMerger().Merge(value));
        var flow = new DynamicGraphEdgeFlow(topology, merger, materializer.Invoke);

        var result = await flow.ExecuteAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("REJECT", result.Value!.OverallVerdict);
        Assert.Equal(StructuralJson(expected.Value!.Merged), StructuralJson(result.Value));
        Assert.Equal(1, materializer.CallCount);
        Assert.Equal(1, merger.CallCount);
        Assert.Same(topology, materializer.Topology);
        var sourceScopedEdge = Assert.Single(topology
            .ListEdgesFrom(input.UpstreamNodeOutput.SourceNodeId)
            .Where(edge => StringComparer.Ordinal.Equals(edge.EdgeId, input.EdgeId)));
        Assert.Same(sourceScopedEdge, materializer.Edge);
        Assert.NotNull(materializer.Input);
        Assert.NotSame(input, materializer.Input);
        Assert.Equal(StructuralJson(input), StructuralJson(materializer.Input));
        Assert.Same(mergeInput, merger.LastInput);
    }

    [Fact]
    public async Task ExecuteAsync_TopologyEdgeEnumerationPermutationsProduceIdenticalResults()
    {
        var mergeInput = CreateMergeInput(RunId);
        var forwardTopology = CreateFanInTopology(reverseEdges: false);
        var reversedTopology = CreateFanInTopology(reverseEdges: true);
        var firstMaterializer = new MaterializerSpy((_, _, _) =>
            OperationResult<GraphFanInMergeInput>.Ok(mergeInput));
        var secondMaterializer = new MaterializerSpy((_, _, _) =>
            OperationResult<GraphFanInMergeInput>.Ok(mergeInput));
        var firstMerger = new MergerSpy(value => new DeterministicGraphFanInMerger().Merge(value));
        var secondMerger = new MergerSpy(value => new DeterministicGraphFanInMerger().Merge(value));

        var first = await new DynamicGraphEdgeFlow(
            forwardTopology,
            firstMerger,
            firstMaterializer.Invoke).ExecuteAsync(CreateRequest(CreateTraversalInput()));
        var second = await new DynamicGraphEdgeFlow(
            reversedTopology,
            secondMerger,
            secondMaterializer.Invoke).ExecuteAsync(CreateRequest(CreateTraversalInput()));

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(StructuralJson(first.Value), StructuralJson(second.Value));
        Assert.Equal("edge-a", firstMaterializer.Edge!.EdgeId);
        Assert.Equal("edge-a", secondMaterializer.Edge!.EdgeId);
        Assert.Equal(1, firstMaterializer.CallCount);
        Assert.Equal(1, secondMaterializer.CallCount);
        Assert.Equal(1, firstMerger.CallCount);
        Assert.Equal(1, secondMerger.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotMutateInputTopologyOrCallerOwnedReferenceCollections()
    {
        // "B" precedes "a" ordinally, pinning ordinal rather than culture-sensitive canonical order.
        var refs = new List<string> { "B", "a" };
        var source = CreateSource();
        var context = new ContextSignals(
            [Domain.CODE_Q],
            ["sub-phase-a"],
            ["knowledge-edge-a"],
            new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.75 });
        var input = CreateTraversalInput(refs: refs, source: source, context: context);
        var request = CreateRequest(input);
        var topology = CreateFanInTopology();
        var inputBefore = StructuralJson(input);
        var topologyBefore = StructuralJson(topology);
        var refsBefore = refs.ToArray();
        var mergeInput = CreateMergeInput(RunId);
        var materializer = new MaterializerSpy((_, _, _) =>
            OperationResult<GraphFanInMergeInput>.Ok(mergeInput));
        var merger = new MergerSpy(value => new DeterministicGraphFanInMerger().Merge(value));

        var result = await new DynamicGraphEdgeFlow(
            topology,
            merger,
            materializer.Invoke).ExecuteAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(inputBefore, StructuralJson(input));
        Assert.Equal(topologyBefore, StructuralJson(topology));
        Assert.Equal(refsBefore, refs);
        Assert.Same(refs, input.UpstreamNodeOutput.UpstreamCandidateRefs);
    }

    [Fact]
    public async Task ExecuteAsync_CancelledTokenWinsBeforeRequestAndDependencies()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var materializer = CreateSuccessfulMaterializer();
        var merger = CreateSuccessfulFakeMerger();
        var flow = new DynamicGraphEdgeFlow(CreateFanInTopology(), merger, materializer.Invoke);

        var result = await flow.ExecuteAsync(null!, cancellation.Token);

        AssertFailure(result, "CANCELLED", "Cancelled.");
        Assert.Equal(0, materializer.CallCount);
        Assert.Equal(0, merger.CallCount);
    }

    [Theory]
    [InlineData("null-request", "DYNAMIC_GRAPH_EDGE_REQUEST_REQUIRED")]
    [InlineData("null-input", "DYNAMIC_GRAPH_EDGE_INPUT_REQUIRED")]
    [InlineData("request-run", "DYNAMIC_GRAPH_EDGE_RUN_ID_REQUIRED")]
    [InlineData("flow", "DYNAMIC_GRAPH_EDGE_FLOW_ID_REQUIRED")]
    [InlineData("scenario", "DYNAMIC_GRAPH_EDGE_SCENARIO_ID_REQUIRED")]
    [InlineData("input-run", "DYNAMIC_GRAPH_EDGE_RUN_ID_REQUIRED")]
    [InlineData("run-mismatch", "DYNAMIC_GRAPH_EDGE_RUN_ID_MISMATCH")]
    [InlineData("edge-id", "DYNAMIC_GRAPH_EDGE_ID_REQUIRED")]
    [InlineData("source", "DYNAMIC_GRAPH_EDGE_SOURCE_DOCUMENT_REQUIRED")]
    [InlineData("context", "DYNAMIC_GRAPH_EDGE_CONTEXT_REQUIRED")]
    [InlineData("upstream", "DYNAMIC_GRAPH_UPSTREAM_OUTPUT_REQUIRED")]
    [InlineData("upstream-source", "DYNAMIC_GRAPH_UPSTREAM_SOURCE_REQUIRED")]
    [InlineData("refs-null", "DYNAMIC_GRAPH_UPSTREAM_REFS_INVALID")]
    [InlineData("refs-empty", "DYNAMIC_GRAPH_UPSTREAM_REFS_INVALID")]
    [InlineData("refs-null-entry", "DYNAMIC_GRAPH_UPSTREAM_REFS_INVALID")]
    [InlineData("refs-blank", "DYNAMIC_GRAPH_UPSTREAM_REFS_INVALID")]
    [InlineData("refs-unsorted", "DYNAMIC_GRAPH_UPSTREAM_REFS_NOT_CANONICAL")]
    [InlineData("refs-duplicate", "DYNAMIC_GRAPH_UPSTREAM_REFS_NOT_CANONICAL")]
    [InlineData("hash", "DYNAMIC_GRAPH_UPSTREAM_HASH_REQUIRED")]
    [InlineData("projection", "DYNAMIC_GRAPH_UPSTREAM_PROJECTION_RULE_REQUIRED")]
    public async Task ExecuteAsync_RequestNestedAndReferenceGuardsFailBeforeDependencies(
        string caseName,
        string expectedCode)
    {
        var materializer = CreateSuccessfulMaterializer();
        var merger = CreateSuccessfulFakeMerger();
        var flow = new DynamicGraphEdgeFlow(CreateFanInTopology(), merger, materializer.Invoke);
        var request = CreateGuardCase(caseName);

        var result = await flow.ExecuteAsync(request!);

        AssertFailure(result, expectedCode);
        Assert.Equal(0, materializer.CallCount);
        Assert.Equal(0, merger.CallCount);
    }

    [Theory]
    [InlineData("null", "DYNAMIC_GRAPH_TOPOLOGY_REQUIRED")]
    [InlineData("invalid", "DYNAMIC_GRAPH_TENANT_REQUIRED")]
    public async Task ExecuteAsync_PropagatesExactTopologyValidationFailure(
        string caseName,
        string expectedCode)
    {
        DynamicGraphTopology? topology = caseName == "null"
            ? null
            : CreateFanInTopology(tenantId: " ");
        var expected = DynamicGraphValidation.Validate(topology);
        Assert.False(expected.IsSuccess);
        var materializer = CreateSuccessfulMaterializer();
        var merger = CreateSuccessfulFakeMerger();
        var flow = new DynamicGraphEdgeFlow(topology, merger, materializer.Invoke);

        var result = await flow.ExecuteAsync(CreateRequest(CreateTraversalInput()));

        AssertFailure(result, expectedCode, expected.ErrorMessage);
        Assert.Equal(0, materializer.CallCount);
        Assert.Equal(0, merger.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesGenuineSingleSourceFanInTopologyCardinalityFailureBeforeDependencies()
    {
        var topology = CreateSingleSourceFanInTopology();
        var expected = DynamicGraphValidation.Validate(topology);
        Assert.False(expected.IsSuccess);
        Assert.Equal("DYNAMIC_GRAPH_FAN_IN_CARDINALITY_INVALID", expected.ErrorCode);
        Assert.Equal(
            "Dynamic graph FanIn target 'node-target' must have at least two distinct sources.",
            expected.ErrorMessage);
        var materializer = CreateSuccessfulMaterializer();
        var merger = CreateSuccessfulFakeMerger();

        var result = await new DynamicGraphEdgeFlow(
            topology,
            merger,
            materializer.Invoke).ExecuteAsync(CreateRequest(CreateTraversalInput()));

        AssertFailure(result, expected.ErrorCode, expected.ErrorMessage);
        Assert.Equal(0, materializer.CallCount);
        Assert.Equal(0, merger.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_MissingEdgeFailsBeforeDependencies()
    {
        var materializer = CreateSuccessfulMaterializer();
        var merger = CreateSuccessfulFakeMerger();
        var input = CreateTraversalInput(edgeId: "EDGE-A");

        var result = await new DynamicGraphEdgeFlow(
            CreateFanInTopology(),
            merger,
            materializer.Invoke).ExecuteAsync(CreateRequest(input));

        AssertFailure(
            result,
            "DYNAMIC_GRAPH_EDGE_NOT_FOUND",
            "Dynamic graph edge 'EDGE-A' was not found.");
        Assert.Equal(0, materializer.CallCount);
        Assert.Equal(0, merger.CallCount);
    }

    [Theory]
    [InlineData(GraphEdgeKind.FanOut, "node-a")]
    [InlineData(GraphEdgeKind.ManyToMany, "node-a")]
    [InlineData(GraphEdgeKind.MultiOutput, "node-a")]
    public async Task ExecuteAsync_LearnedKindsWithoutAuthorityPipelineFailBeforeFanInDependencyCalls(
        GraphEdgeKind kind,
        string sourceNodeId)
    {
        var topology = CreateTopologyForKind(kind);
        var validation = DynamicGraphValidation.Validate(topology);
        Assert.True(validation.IsSuccess, validation.ErrorMessage);
        var materializer = CreateSuccessfulMaterializer();
        var merger = CreateSuccessfulFakeMerger();
        var input = CreateTraversalInput(edgeId: "edge-primary", sourceNodeId: sourceNodeId);

        var result = await new DynamicGraphEdgeFlow(
            topology,
            merger,
            materializer.Invoke).ExecuteAsync(CreateRequest(input));

        AssertFailure(
            result,
            "EDGE_AUTHORITY_PIPELINE_REQUIRED",
            "Learned edge authority pipeline is required.");
        Assert.Equal(0, materializer.CallCount);
        Assert.Equal(0, merger.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnLoopEmitsExactDeterministicHandoffWithoutDependencyCalls()
    {
        var topology = CreateTopologyForKind(GraphEdgeKind.ReturnLoop);
        var validation = DynamicGraphValidation.Validate(topology);
        Assert.True(validation.IsSuccess, validation.ErrorMessage);
        var materializer = CreateSuccessfulMaterializer();
        var merger = CreateSuccessfulFakeMerger();
        var pipeline = new AuthorityPipelineSpy((_, _, _, _, _) =>
            throw new InvalidOperationException("ReturnLoop must not invoke learned authority."));
        var input = CreateTraversalInput(edgeId: "edge-primary", sourceNodeId: "node-c");

        var result = await new DynamicGraphEdgeFlow(
            topology,
            merger,
            materializer.Invoke,
            pipeline).ExecuteAsync(CreateRequest(input));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var value = Assert.IsType<LogicalFlowResult>(result.Value);
        Assert.Equal(FlowId, value.FlowId);
        Assert.Equal(ScenarioId, value.ScenarioId);
        Assert.Equal("RETURN_LOOP_HANDOFF_READY", value.OverallVerdict);
        Assert.Equal(
            "Topology-defined ReturnLoop emitted its single correction target; target execution was not performed.",
            value.FinalHumanResult);
        Assert.Equal("flow-dynamic-edge:return-loop-handoff", value.Summary.SummaryId);
        Assert.Equal(3, value.Summary.Tier);
        Assert.Equal(0d, value.Summary.BaselinePrimary);
        Assert.Equal(1d, value.Summary.EnhancedPrimary);
        Assert.Equal("correction_target_count", value.Summary.PrimaryMetricName);
        Assert.False(value.Summary.LowerIsBetter);
        Assert.True(value.Summary.Passed);
        Assert.False(value.Summary.ShouldScaleUp);
        Assert.Equal("HANDOFF_RETURN_LOOP_TO_L4", value.Summary.NextAction);
        Assert.Equal(0d, value.Summary.ScalingSignal);
        Assert.Equal(
            "Deterministic correction handoff only; scaling was not evaluated.",
            value.Summary.ScalingSignalNote);
        Assert.Equal(
            "Topology-defined ReturnLoop produced a single typed correction handoff; no target was executed and topology was not mutated.",
            value.Summary.Notes);
        Assert.Equal(0d, value.Summary.BaselineTimeMs);
        Assert.Equal(0d, value.Summary.EnhancedTimeMs);
        Assert.Empty(value.EvidenceIds);
        Assert.Empty(value.Warnings);
        Assert.Empty(value.TransitionSignals);
        Assert.Equal(9, value.Metrics.Count);
        Assert.Equal("topology_return_loop", value.Metrics["authority_source"]);
        Assert.Equal("false", value.Metrics["authority_gap"]);
        Assert.Equal("0", value.Metrics["authority_warning_count"]);
        Assert.Equal("edge-primary", value.Metrics["edge_id"]);
        Assert.Equal("ReturnLoop", value.Metrics["edge_kind"]);
        Assert.Equal("false", value.Metrics["parallel_eligible"]);
        Assert.Equal("1", value.Metrics["target_count"]);
        Assert.Equal("false", value.Metrics["targets_executed"]);
        Assert.Equal("false", value.Metrics["topology_mutated"]);

        var handoff = Assert.IsType<DynamicGraphEdgeTargetHandoff>(value.EdgeTargetHandoff);
        Assert.Equal(RunId, handoff.RunId);
        Assert.Equal("edge-primary", handoff.EdgeId);
        Assert.Equal("node-c", handoff.FromNodeId);
        Assert.Equal(DomainTag, handoff.DomainTag);
        Assert.Equal(GraphEdgeKind.ReturnLoop, handoff.Kind);
        Assert.False(handoff.ParallelEligible);
        var target = Assert.Single(handoff.OrderedTargets);
        Assert.Equal("node-a", target.TargetNodeId);
        Assert.Equal(1d, target.Confidence);
        Assert.Equal(0, target.Ordinal);
        Assert.False(handoff.KnowledgeEdge);
        Assert.Equal("topology_return_loop", handoff.AuthoritySource);
        Assert.Equal(input.UpstreamNodeOutput.UpstreamCandidateRefs, handoff.UpstreamCandidateRefs);
        Assert.Equal(input.UpstreamNodeOutput.UpstreamOutputHash, handoff.UpstreamOutputHash);
        Assert.Equal(input.UpstreamNodeOutput.ProjectionRule, handoff.ProjectionRule);
        Assert.False(handoff.TargetsExecuted);
        Assert.False(handoff.TopologyMutated);
        Assert.Equal(0, materializer.CallCount);
        Assert.Equal(0, merger.CallCount);
        Assert.Equal(0, pipeline.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ValidLearnedFanOutEmitsExactTypedHandoffAndSanitizedEvidence()
    {
        var topology = CreateTopologyForKind(GraphEdgeKind.FanOut);
        var validation = DynamicGraphValidation.Validate(topology);
        Assert.True(validation.IsSuccess, validation.ErrorMessage);
        var context = CreateLearnedContext();
        var input = CreateTraversalInput(
            edgeId: "edge-primary",
            sourceNodeId: "node-a",
            context: context);
        var request = CreateRequest(input);
        var materializer = CreateSuccessfulMaterializer();
        var merger = CreateSuccessfulFakeMerger();
        var pipeline = new AuthorityPipelineSpy((_, _, _, _, _) =>
            OperationResult<EdgeAuthorityExecutionResult>.Ok(CreateLearnedExecution(context)));

        var result = await new DynamicGraphEdgeFlow(
            topology,
            merger,
            materializer.Invoke,
            pipeline).ExecuteAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, pipeline.CallCount);
        Assert.NotNull(pipeline.Edge);
        Assert.Equal("edge-primary", pipeline.Edge!.EdgeId);
        Assert.Equal(GraphEdgeKind.FanOut, pipeline.Edge.Kind);
        Assert.NotNull(pipeline.Input);
        Assert.Equal(StructuralJson(input), StructuralJson(pipeline.Input));
        Assert.Equal(DomainTag, pipeline.DomainTag);
        Assert.Same(request.Flags, pipeline.Flags);
        Assert.Equal(default, pipeline.CancellationToken);
        Assert.Equal(0, materializer.CallCount);
        Assert.Equal(0, merger.CallCount);

        var value = Assert.IsType<LogicalFlowResult>(result.Value);
        Assert.Equal(FlowId, value.FlowId);
        Assert.Equal(ScenarioId, value.ScenarioId);
        Assert.Equal("TARGET_HANDOFF_READY", value.OverallVerdict);
        Assert.Equal(
            "Learned edge authority selected target nodes; target execution was not performed.",
            value.FinalHumanResult);
        Assert.Equal("flow-dynamic-edge:edge-target-handoff", value.Summary.SummaryId);
        Assert.Equal(3, value.Summary.Tier);
        Assert.Equal(0d, value.Summary.BaselinePrimary);
        Assert.Equal(1d, value.Summary.EnhancedPrimary);
        Assert.Equal("selected_target_count", value.Summary.PrimaryMetricName);
        Assert.False(value.Summary.LowerIsBetter);
        Assert.True(value.Summary.Passed);
        Assert.False(value.Summary.ShouldScaleUp);
        Assert.Equal("HANDOFF_TO_L4", value.Summary.NextAction);
        Assert.Equal(0d, value.Summary.ScalingSignal);
        Assert.Equal(
            "Inference handoff only; scaling was not evaluated.",
            value.Summary.ScalingSignalNote);
        Assert.Equal(
            "Learned edge authority produced a typed handoff; no target was executed and topology was not mutated.",
            value.Summary.Notes);
        Assert.Equal(0d, value.Summary.BaselineTimeMs);
        Assert.Equal(0d, value.Summary.EnhancedTimeMs);
        Assert.Equal(["evidence-learned"], value.EvidenceIds);
        Assert.Equal(
            ["edge_authority_reserved_warning_redacted", "safe-warning"],
            value.Warnings);
        Assert.Empty(value.TransitionSignals);
        Assert.Equal(9, value.Metrics.Count);
        Assert.Equal("trained_edge_unit", value.Metrics["authority_source"]);
        Assert.Equal("false", value.Metrics["authority_gap"]);
        Assert.Equal("2", value.Metrics["authority_warning_count"]);
        Assert.Equal("edge-primary", value.Metrics["edge_id"]);
        Assert.Equal("FanOut", value.Metrics["edge_kind"]);
        Assert.Equal("true", value.Metrics["parallel_eligible"]);
        Assert.Equal("1", value.Metrics["target_count"]);
        Assert.Equal("false", value.Metrics["targets_executed"]);
        Assert.Equal("false", value.Metrics["topology_mutated"]);

        var handoff = Assert.IsType<DynamicGraphEdgeTargetHandoff>(value.EdgeTargetHandoff);
        Assert.Equal(RunId, handoff.RunId);
        Assert.Equal("edge-primary", handoff.EdgeId);
        Assert.Equal("node-a", handoff.FromNodeId);
        Assert.Equal(DomainTag, handoff.DomainTag);
        Assert.Equal(GraphEdgeKind.FanOut, handoff.Kind);
        Assert.True(handoff.ParallelEligible);
        var target = Assert.Single(handoff.OrderedTargets);
        Assert.Equal("node-c", target.TargetNodeId);
        Assert.Equal(0.91d, target.Confidence);
        Assert.Equal(0, target.Ordinal);
        Assert.False(handoff.KnowledgeEdge);
        Assert.Equal("trained_edge_unit", handoff.AuthoritySource);
        Assert.Equal(input.UpstreamNodeOutput.UpstreamCandidateRefs, handoff.UpstreamCandidateRefs);
        Assert.Equal(input.UpstreamNodeOutput.UpstreamOutputHash, handoff.UpstreamOutputHash);
        Assert.Equal(input.UpstreamNodeOutput.ProjectionRule, handoff.ProjectionRule);
        Assert.False(handoff.TargetsExecuted);
        Assert.False(handoff.TopologyMutated);
    }

    [Theory]
    [InlineData("membership-mismatch")]
    [InlineData("duplicate-membership")]
    [InlineData("noncanonical-membership")]
    [InlineData("undefined-domain-membership")]
    [InlineData("prior-confidence-map-mismatch")]
    public async Task ExecuteAsync_HostileLearnedNormalizedContextFailsClosedWithoutHandoff(
        string mutation)
    {
        var topology = CreateTopologyForKind(GraphEdgeKind.FanOut);
        var context = CreateLearnedContext();
        var hostileContext = CreateHostileLearnedContext(context, mutation);
        var materializer = CreateSuccessfulMaterializer();
        var merger = CreateSuccessfulFakeMerger();
        var pipeline = new AuthorityPipelineSpy((_, _, _, _, _) =>
            OperationResult<EdgeAuthorityExecutionResult>.Ok(CreateLearnedExecution(hostileContext)));
        var input = CreateTraversalInput(
            edgeId: "edge-primary",
            sourceNodeId: "node-a",
            context: context);

        var result = await new DynamicGraphEdgeFlow(
            topology,
            merger,
            materializer.Invoke,
            pipeline).ExecuteAsync(CreateRequest(input));

        AssertFailure(
            result,
            "EDGE_AUTHORITY_RESULT_INVALID",
            "Learned edge authority result is invalid.");
        Assert.Null(result.Value?.EdgeTargetHandoff);
        Assert.Equal(1, pipeline.CallCount);
        Assert.Equal(0, materializer.CallCount);
        Assert.Equal(0, merger.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_UpstreamSourceMustMatchResolvedEdgeSource()
    {
        var materializer = CreateSuccessfulMaterializer();
        var merger = CreateSuccessfulFakeMerger();
        var input = CreateTraversalInput(sourceNodeId: "NODE-A");

        var result = await new DynamicGraphEdgeFlow(
            CreateFanInTopology(),
            merger,
            materializer.Invoke).ExecuteAsync(CreateRequest(input));

        AssertFailure(result, "DYNAMIC_GRAPH_EDGE_SOURCE_MISMATCH");
        Assert.Equal(0, materializer.CallCount);
        Assert.Equal(0, merger.CallCount);
    }

    [Theory]
    [InlineData("merger", "EDGE_FAN_IN_MERGER_REQUIRED")]
    [InlineData("materializer", "EDGE_FAN_IN_INPUT_MATERIALIZER_REQUIRED")]
    [InlineData("both", "EDGE_FAN_IN_MERGER_REQUIRED")]
    public async Task ExecuteAsync_NullDependenciesFailInDeterministicOrder(
        string caseName,
        string expectedCode)
    {
        var materializer = CreateSuccessfulMaterializer();
        var merger = CreateSuccessfulFakeMerger();
        IEdgeFanInMerger? configuredMerger = caseName is "merger" or "both" ? null : merger;
        Func<DynamicGraphTopology, IDynamicGraphEdge, EdgeTraversalInput, OperationResult<GraphFanInMergeInput>>?
            configuredMaterializer = caseName is "materializer" or "both" ? null : materializer.Invoke;
        var flow = new DynamicGraphEdgeFlow(
            CreateFanInTopology(),
            configuredMerger,
            configuredMaterializer);

        var result = await flow.ExecuteAsync(CreateRequest(CreateTraversalInput()));

        AssertFailure(result, expectedCode);
        Assert.Equal(0, materializer.CallCount);
        Assert.Equal(0, merger.CallCount);
    }

    [Theory]
    [InlineData("failed", "MATERIALIZER_SENTINEL")]
    [InlineData("failed-local-code", "EDGE_FAN_IN_INPUT_MATERIALIZER_FAILED")]
    [InlineData("thrown", "EDGE_FAN_IN_INPUT_MATERIALIZER_FAILED")]
    [InlineData("null-value", "EDGE_FAN_IN_MERGE_INPUT_REQUIRED")]
    [InlineData("blank-run", "EDGE_FAN_IN_RUN_ID_REQUIRED")]
    [InlineData("run-mismatch", "EDGE_FAN_IN_RUN_ID_MISMATCH")]
    [InlineData("cancelled-while-invoking", "CANCELLED")]
    [InlineData("uncancelled-oce", "EDGE_FAN_IN_INPUT_MATERIALIZER_FAILED")]
    [InlineData("null-envelope", "EDGE_FAN_IN_INPUT_MATERIALIZER_FAILED")]
    public async Task ExecuteAsync_MaterializerFailuresAreStableAndNeverCallMerger(
        string behavior,
        string expectedCode)
    {
        using var cancellation = new CancellationTokenSource();
        var validMergeInput = CreateMergeInput(RunId);
        var materializer = new MaterializerSpy(behavior switch
        {
            "failed" => (_, _, _) => OperationResult<GraphFanInMergeInput>.Fail(
                "Materializer sentinel.",
                "MATERIALIZER_SENTINEL"),
            "failed-local-code" => (_, _, _) => OperationResult<GraphFanInMergeInput>.Fail(
                "Dependency-owned materializer failure.",
                "EDGE_FAN_IN_INPUT_MATERIALIZER_FAILED"),
            "thrown" => (_, _, _) => throw new InvalidOperationException("materializer-secret"),
            "null-value" => (_, _, _) => OperationResult<GraphFanInMergeInput>.Ok(null!),
            "blank-run" => (_, _, _) => OperationResult<GraphFanInMergeInput>.Ok(
                validMergeInput with { RunId = " " }),
            "run-mismatch" => (_, _, _) => OperationResult<GraphFanInMergeInput>.Ok(
                validMergeInput with { RunId = "RUN-EDGE-001" }),
            "cancelled-while-invoking" => (_, _, _) =>
            {
                cancellation.Cancel();
                throw new OperationCanceledException(cancellation.Token);
            },
            "uncancelled-oce" => (_, _, _) => throw new OperationCanceledException(),
            "null-envelope" => (_, _, _) => null!,
            _ => throw new InvalidOperationException("Unknown materializer behavior.")
        });
        var merger = CreateSuccessfulFakeMerger();
        var flow = new DynamicGraphEdgeFlow(CreateFanInTopology(), merger, materializer.Invoke);

        var result = await flow.ExecuteAsync(CreateRequest(CreateTraversalInput()), cancellation.Token);

        var propagatedMessage = behavior switch
        {
            "failed" => "Materializer sentinel.",
            "failed-local-code" => "Dependency-owned materializer failure.",
            _ => null
        };
        AssertFailure(result, expectedCode, propagatedMessage);
        Assert.Equal(1, materializer.CallCount);
        Assert.Equal(0, merger.CallCount);
        if (behavior == "failed")
            Assert.Equal("Materializer sentinel.", result.ErrorMessage);
        if (behavior == "thrown")
            Assert.False(result.ErrorMessage.Contains("materializer-secret", StringComparison.Ordinal));
        if (behavior == "cancelled-while-invoking")
            Assert.Equal("Cancelled.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("failed", "MERGER_SENTINEL")]
    [InlineData("failed-local-code", "EDGE_FAN_IN_MERGER_FAILED")]
    [InlineData("thrown", "EDGE_FAN_IN_MERGER_FAILED")]
    [InlineData("null-value", "EDGE_FAN_IN_MERGE_RESULT_REQUIRED")]
    [InlineData("null-envelope", "EDGE_FAN_IN_MERGER_FAILED")]
    public async Task ExecuteAsync_MergerFailuresAreStableAndMaterializerRunsOnce(
        string behavior,
        string expectedCode)
    {
        var mergeInput = CreateMergeInput(RunId);
        var materializer = new MaterializerSpy((_, _, _) =>
            OperationResult<GraphFanInMergeInput>.Ok(mergeInput));
        var merger = new MergerSpy(behavior switch
        {
            "failed" => _ => OperationResult<GraphFanInMergeResult>.Fail(
                "Merger sentinel.",
                "MERGER_SENTINEL"),
            "failed-local-code" => _ => OperationResult<GraphFanInMergeResult>.Fail(
                "Dependency-owned merger failure.",
                "EDGE_FAN_IN_MERGER_FAILED"),
            "thrown" => _ => throw new InvalidOperationException("merger-secret"),
            "null-value" => _ => OperationResult<GraphFanInMergeResult>.Ok(null!),
            "null-envelope" => _ => null!,
            _ => throw new InvalidOperationException("Unknown merger behavior.")
        });
        var flow = new DynamicGraphEdgeFlow(CreateFanInTopology(), merger, materializer.Invoke);

        var result = await flow.ExecuteAsync(CreateRequest(CreateTraversalInput()));

        var propagatedMessage = behavior switch
        {
            "failed" => "Merger sentinel.",
            "failed-local-code" => "Dependency-owned merger failure.",
            _ => null
        };
        AssertFailure(result, expectedCode, propagatedMessage);
        Assert.Equal(1, materializer.CallCount);
        Assert.Equal(1, merger.CallCount);
        if (behavior == "failed")
            Assert.Equal("Merger sentinel.", result.ErrorMessage);
        if (behavior == "thrown")
            Assert.False(result.ErrorMessage.Contains("merger-secret", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("no-branches", "NO_BRANCHES_TO_MERGE")]
    [InlineData("merge-key", "MERGE_KEY_MISMATCH")]
    [InlineData("path-order", "PATH_ORDER_REF_MISSING")]
    [InlineData("provenance", "PROVENANCE_REF_MISSING")]
    [InlineData("legal", "LEGAL_SCOPE_BLOCKED")]
    [InlineData("conflict", "CONFLICT_UNRESOLVED")]
    public async Task ExecuteAsync_PropagatesExactRealMergerFailures(
        string caseName,
        string expectedCode)
    {
        var mergeInput = CreateRealMergerFailureInput(caseName);
        var expected = new DeterministicGraphFanInMerger().Merge(mergeInput);
        Assert.False(expected.IsSuccess);
        Assert.Equal(expectedCode, expected.ErrorCode);
        var materializer = new MaterializerSpy((_, _, _) =>
            OperationResult<GraphFanInMergeInput>.Ok(mergeInput));
        var merger = new MergerSpy(value => new DeterministicGraphFanInMerger().Merge(value));
        var flow = new DynamicGraphEdgeFlow(CreateFanInTopology(), merger, materializer.Invoke);

        var result = await flow.ExecuteAsync(CreateRequest(CreateTraversalInput()));

        AssertFailure(result, expectedCode, expected.ErrorMessage);
        Assert.Equal(1, materializer.CallCount);
        Assert.Equal(1, merger.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_OneBranchRealMergeIsRejectedAtFlowCardinalityBoundary()
    {
        var mergeInput = CreateMergeInput(RunId, branchCount: 1);
        var realResult = new DeterministicGraphFanInMerger().Merge(mergeInput);
        Assert.True(realResult.IsSuccess, realResult.ErrorMessage);
        Assert.Equal(1, realResult.Value!.MergedBranchCount);
        var materializer = new MaterializerSpy((_, _, _) =>
            OperationResult<GraphFanInMergeInput>.Ok(mergeInput));
        var merger = new MergerSpy(value => new DeterministicGraphFanInMerger().Merge(value));

        var result = await new DynamicGraphEdgeFlow(
            CreateFanInTopology(),
            merger,
            materializer.Invoke).ExecuteAsync(CreateRequest(CreateTraversalInput()));

        AssertFailure(result, "DYNAMIC_GRAPH_FAN_IN_BRANCH_CARDINALITY_INVALID");
        Assert.Equal(1, materializer.CallCount);
        Assert.Equal(1, merger.CallCount);
    }

    [Theory]
    [InlineData("invalid-role", "BEST_OF_MERGE_FORBIDDEN")]
    [InlineData("order-authority", "MERGE_ORDER_USED_AS_AUTHORITY")]
    public async Task ExecuteAsync_InvalidMergeWitnessesAreRejected(
        string caseName,
        string expectedCode)
    {
        var fakeResult = caseName == "invalid-role"
            ? CreateFakeMergeResult(role: (FanInMergeRole)999)
            : CreateFakeMergeResult(mergeOrderIsAuthority: true);
        var materializer = CreateSuccessfulMaterializer();
        var merger = new MergerSpy(_ => OperationResult<GraphFanInMergeResult>.Ok(fakeResult));

        var result = await new DynamicGraphEdgeFlow(
            CreateFanInTopology(),
            merger,
            materializer.Invoke).ExecuteAsync(CreateRequest(CreateTraversalInput()));

        AssertFailure(result, expectedCode);
        Assert.Equal(1, materializer.CallCount);
        Assert.Equal(1, merger.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ConflictPreservingTwoBranchMergeReturnsStructurallyExactMergedOnly()
    {
        var mergeInput = CreateMergeInput(RunId, divergentMetrics: true);
        var expected = new DeterministicGraphFanInMerger().Merge(mergeInput);
        Assert.True(expected.IsSuccess, expected.ErrorMessage);
        Assert.Contains("CONFLICT_UNRESOLVED", expected.Value!.FailureReasonCodes);
        var materializer = new MaterializerSpy((_, _, _) =>
            OperationResult<GraphFanInMergeInput>.Ok(mergeInput));
        var merger = new MergerSpy(value => new DeterministicGraphFanInMerger().Merge(value));

        var result = await new DynamicGraphEdgeFlow(
            CreateFanInTopology(),
            merger,
            materializer.Invoke).ExecuteAsync(CreateRequest(CreateTraversalInput()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.IsType<LogicalFlowResult>(result.Value);
        Assert.Equal(StructuralJson(expected.Value.Merged), StructuralJson(result.Value));
        Assert.False(result.Value!.Summary.Passed);
        Assert.Contains("branch-a", result.Value.Metrics["decision"], StringComparison.Ordinal);
        Assert.Contains("branch-b", result.Value.Metrics["decision"], StringComparison.Ordinal);
        Assert.Equal(1, materializer.CallCount);
        Assert.Equal(1, merger.CallCount);
    }

    private static LogicalFlowRequest<EdgeTraversalInput>? CreateGuardCase(string caseName)
    {
        var input = CreateTraversalInput();
        var request = CreateRequest(input);
        return caseName switch
        {
            "null-request" => null,
            "null-input" => request with { Input = null! },
            "request-run" => request with { RunId = " " },
            "flow" => request with { FlowId = " " },
            "scenario" => request with { ScenarioId = " " },
            "input-run" => request with { Input = input with { RunId = " " } },
            "run-mismatch" => request with { Input = input with { RunId = "RUN-EDGE-001" } },
            "edge-id" => request with { Input = input with { EdgeId = " " } },
            "source" => request with { Input = input with { Source = null! } },
            "context" => request with { Input = input with { Context = null! } },
            "upstream" => request with { Input = input with { UpstreamNodeOutput = null! } },
            "upstream-source" => WithUpstream(request, adapter => adapter with { SourceNodeId = " " }),
            "refs-null" => WithUpstream(request, adapter => adapter with { UpstreamCandidateRefs = null! }),
            "refs-empty" => WithUpstream(request, adapter => adapter with { UpstreamCandidateRefs = [] }),
            "refs-null-entry" => WithUpstream(
                request,
                adapter => adapter with { UpstreamCandidateRefs = ["candidate-a", null!] }),
            "refs-blank" => WithUpstream(
                request,
                adapter => adapter with { UpstreamCandidateRefs = ["candidate-a", " "] }),
            "refs-unsorted" => WithUpstream(
                request,
                adapter => adapter with { UpstreamCandidateRefs = ["candidate-b", "candidate-a"] }),
            "refs-duplicate" => WithUpstream(
                request,
                adapter => adapter with { UpstreamCandidateRefs = ["candidate-a", "candidate-a"] }),
            "hash" => WithUpstream(request, adapter => adapter with { UpstreamOutputHash = " " }),
            "projection" => WithUpstream(request, adapter => adapter with { ProjectionRule = " " }),
            _ => throw new InvalidOperationException("Unknown guard case.")
        };
    }

    private static LogicalFlowRequest<EdgeTraversalInput> WithUpstream(
        LogicalFlowRequest<EdgeTraversalInput> request,
        Func<NodeOutputToEdgeInputAdapter, NodeOutputToEdgeInputAdapter> mutate)
        => request with
        {
            Input = request.Input with
            {
                UpstreamNodeOutput = mutate(request.Input.UpstreamNodeOutput)
            }
        };

    private static DynamicGraphTopology CreateFanInTopology(
        bool reverseEdges = false,
        string tenantId = TenantId)
    {
        var edges = new IDynamicGraphEdge[]
        {
            CreateEdge("edge-a", "node-a", ["node-target"], GraphEdgeKind.FanIn),
            CreateEdge("edge-b", "node-b", ["node-target"], GraphEdgeKind.FanIn)
        };
        if (reverseEdges)
            Array.Reverse(edges);

        return CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-target")],
            edges,
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1), CreateOrder("node-target", 2)],
            tenantId);
    }

    private static DynamicGraphTopology CreateSingleSourceFanInTopology()
        => CreateTopology(
            [CreateNode("node-a"), CreateNode("node-target")],
            [CreateEdge("edge-a", "node-a", ["node-target"], GraphEdgeKind.FanIn)],
            [CreateOrder("node-a", 0), CreateOrder("node-target", 1)]);

    private static DynamicGraphTopology CreateTopologyForKind(GraphEdgeKind kind)
    {
        if (kind == GraphEdgeKind.ManyToMany)
        {
            return CreateTopology(
                [CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c"), CreateNode("node-d")],
                [
                    CreateEdge("edge-primary", "node-a", ["node-c", "node-d"], kind),
                    CreateEdge("edge-secondary", "node-b", ["node-c", "node-d"], kind)
                ],
                [
                    CreateOrder("node-a", 0),
                    CreateOrder("node-b", 1),
                    CreateOrder("node-c", 2),
                    CreateOrder("node-d", 3)
                ]);
        }

        if (kind == GraphEdgeKind.ReturnLoop)
        {
            return CreateTopology(
                [CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c")],
                [CreateEdge("edge-primary", "node-c", ["node-a"], kind)],
                [CreateOrder("node-a", 0), CreateOrder("node-b", 1), CreateOrder("node-c", 2)]);
        }

        return CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c")],
            [CreateEdge("edge-primary", "node-a", ["node-c"], kind)],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1), CreateOrder("node-c", 2)]);
    }

    private static ContextSignals CreateLearnedContext()
        => new(
            [Domain.CODE_Q],
            ["sub-phase-a"],
            ["knowledge-edge-a"],
            new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.75d });

    private static ContextSignals CreateHostileLearnedContext(
        ContextSignals expected,
        string mutation)
        => mutation switch
        {
            "membership-mismatch" => expected with
            {
                ActiveKnowledgeEdges = ["knowledge-edge-b"]
            },
            "duplicate-membership" => expected with
            {
                CompletedSubPhases = ["sub-phase-a", "sub-phase-a"]
            },
            "noncanonical-membership" => expected with
            {
                CompletedSubPhases = [" sub-phase-a"]
            },
            "undefined-domain-membership" => expected with
            {
                PriorDomains = [(Domain)999]
            },
            "prior-confidence-map-mismatch" => expected with
            {
                PriorConfidenceMap = new Dictionary<Domain, double>
                {
                    [Domain.CODE_Q] = 0.74d
                }
            },
            _ => throw new InvalidOperationException("Unknown learned context mutation.")
        };

    private static EdgeAuthorityExecutionResult CreateLearnedExecution(ContextSignals normalizedContext)
    {
        var candidate = new EdgeCandidate(
            TargetSubPhaseId: "node-c",
            EvidencePredicates: ["PREDICATE-A"],
            BaselineScore: 0.42d,
            AuthorityRole: "baseline_candidate_generator");
        var normalizedInput = new EdgeAuthorityInput(
            EdgeId: "edge-primary",
            SemanticTokens: ["compiled", "edge"],
            PredicateIds: ["PREDICATE-A"],
            Context: normalizedContext,
            Candidates: Array.AsReadOnly(new[] { candidate }),
            ExcludedLeakageSignals: RequiredLeakageSignals.ToArray());
        var decision = new EdgeAuthorityDecision(
            OrderedTargets: Array.AsReadOnly(new[] { ("node-c", 0.91d) }),
            FanOutCount: 1,
            KnowledgeEdge: false,
            AuthoritySource: "trained_edge_unit",
            AblationBaselineRank: Array.AsReadOnly(new[] { "node-c" }));
        var candidateOrderLeakageReport = new DynamicGraphEdgeCandidateOrderLeakageReport(
            EdgeId: "edge-primary",
            ResolvedAuthorityUnitId: "unit-edge-authority-v1",
            IsOrderInvariant: true,
            Probes: Array.AsReadOnly(Enumerable.Range(1, 5)
                .Select(index => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                    ProbeId: $"candidate-order-permutation-{index:D2}",
                    CandidateOrder: Array.AsReadOnly(new[] { candidate }),
                    Decision: decision))
                .ToArray()),
            ExcludedLeakageSignals: RequiredLeakageSignals);
        var identityLeakageReport = new DynamicGraphEdgeIdentityLeakageReport(
            EdgeId: "edge-primary",
            ResolvedAuthorityUnitId: "unit-edge-authority-v1",
            IsIdentityInvariant: true,
            Probes: Array.AsReadOnly(Enumerable.Range(0, 3)
                .Select(index => new DynamicGraphEdgeIdentityControlProbe(
                    ProbeId: IdentityProbeIds[index],
                    ControlledSignal: IdentityControlledSignals[index],
                    ControlValue: IdentityControlValues[index],
                    Decision: decision))
                .ToArray()),
            ExcludedLeakageSignals: RequiredLeakageSignals);

        return new EdgeAuthorityExecutionResult(
            NormalizedInput: normalizedInput,
            ProposedCandidates: Array.AsReadOnly(new[] { candidate }),
            Decision: decision,
            AuthorityEvidence: new DynamicGraphEdgeAuthorityEvidence(
                EdgeId: "edge-primary",
                AuthorityUnitId: "unit-edge-authority-v1",
                AuthorityUnitLevel: "L2",
                DeterministicFinalAuthority: false,
                BaselineUnitId: "unit-edge-baseline-v1",
                BaselineRole: "candidate_generator",
                ExcludedLeakageSignals: RequiredLeakageSignals,
                SourceProof: "fixture-source-proof",
                CutoverProof: "fixture-cutover-proof",
                AblationProofRef: "fixture-ablation-proof"),
            CandidateOrderLeakageReport: candidateOrderLeakageReport,
            IdentityLeakageReport: identityLeakageReport,
            UnitConfidence: 0.88d,
            EvidenceIds: Array.AsReadOnly(new[] { "evidence-learned" }),
            Warnings: Array.AsReadOnly(
                new[]
                {
                    "knowledge_edge internal detail",
                    "KNOWLEDGE_EDGE duplicate detail",
                    "safe-warning"
                }));
    }

    private static DynamicGraphTopology CreateTopology(
        IReadOnlyList<IDynamicGraphNode> nodes,
        IReadOnlyList<IDynamicGraphEdge> edges,
        IReadOnlyList<DynamicGraphNodeOrder> order,
        string tenantId = TenantId)
        => new(tenantId, DomainTag, ContextId, nodes, edges, order);

    private static IDynamicGraphNode CreateNode(string nodeId)
        => new TestNode(
            nodeId,
            new DynamicGraphNodeDescriptor(
                NodeId: nodeId,
                DomainTag: DomainTag,
                TInputTypeName: "System.String",
                TOutputTypeName: "System.String",
                IsTrained: false,
                DeterministicFinalAuthority: false,
                RuntimeSemanticQualityClaimed: false,
                AuthorityRole: "baseline_candidate_generator",
                CheckpointRef: null,
                ManifestRef: null));

    private static IDynamicGraphEdge CreateEdge(
        string edgeId,
        string fromNodeId,
        IReadOnlyList<string> toNodeIds,
        GraphEdgeKind kind,
        IReadOnlyList<string>? excludedLeakageSignals = null)
        => new TestEdge(
            EdgeId: edgeId,
            FromNodeId: fromNodeId,
            ToNodeIds: toNodeIds,
            Kind: kind,
            ParallelEligible: kind != GraphEdgeKind.ReturnLoop,
            AuthorityUnitId: "unit-edge-authority-v1",
            AuthorityUnitLevel: "L2",
            DeterministicFinalAuthority: false,
            BaselineUnitId: "unit-edge-baseline-v1",
            BaselineRole: "candidate_generator",
            ExcludedLeakageSignals: excludedLeakageSignals ?? RequiredLeakageSignals,
            SourceProof: "fixture-source-proof",
            CutoverProof: "fixture-cutover-proof",
            AblationProofRef: "fixture-ablation-proof");

    private static DynamicGraphNodeOrder CreateOrder(string nodeId, int position)
        => new(
            OrderId: $"order-{nodeId}",
            NodeId: nodeId,
            TenantId: TenantId,
            DomainTag: DomainTag,
            ContextId: ContextId,
            Position: position);

    private static EdgeTraversalInput CreateTraversalInput(
        string edgeId = "edge-a",
        string sourceNodeId = "node-a",
        IReadOnlyList<string>? refs = null,
        SubPhaseRouterSourceDocument? source = null,
        ContextSignals? context = null)
        => new(
            RunId: RunId,
            EdgeId: edgeId,
            Source: source ?? CreateSource(),
            Context: context ?? ContextSignals.Empty,
            UpstreamNodeOutput: new NodeOutputToEdgeInputAdapter(
                SourceNodeId: sourceNodeId,
                UpstreamCandidateRefs: refs ?? ["candidate-a", "candidate-b"],
                UpstreamOutputHash: "sha256:upstream-output",
                ProjectionRule: "GraphNodeCandidate.Id -> UpstreamCandidateRefs (order-free set)"));

    private static SubPhaseRouterSourceDocument CreateSource()
        => new(
            DocumentId: "document-edge",
            SourcePath: "source://edge",
            SourceTier: "internal",
            LicenseOrAuthority: "owner-approved",
            DocumentType: "plan",
            DocumentTitle: "Dynamic edge source",
            CompiledText: "compiled edge source",
            CompiledPredicateIds: ["predicate-a", "predicate-b"],
            SourceStatus: "accepted",
            KnownFailureMode: string.Empty,
            DomainLabel: DomainTag,
            SegmentDocuments: []);

    private static LogicalFlowRequest<EdgeTraversalInput> CreateRequest(EdgeTraversalInput input)
        => new(
            RunId: RunId,
            FlowId: FlowId,
            ScenarioId: ScenarioId,
            Input: input,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>(StringComparer.Ordinal));

    private static GraphFanInMergeInput CreateMergeInput(
        string runId,
        int branchCount = 2,
        bool divergentMetrics = false)
    {
        var branches = new List<GraphAsyncBranchResult>
        {
            CreateBranch("branch-a", "path:01", "PASS", 0.90, "left", divergentMetrics)
        };
        if (branchCount > 1)
        {
            branches.Add(CreateBranch(
                "branch-b",
                "path:02",
                "REJECT",
                0.20,
                "right",
                divergentMetrics));
        }

        var branchArray = branches.ToArray();
        return new GraphFanInMergeInput(
            RunId: runId,
            Branches: branchArray,
            MergeKey: MergeKey,
            PathOrderRef: PathOrderRef,
            LegalScope: "LEGAL_SCOPE_ALLOWED",
            SourceRefs: ["source-envelope"],
            ProvenanceRefs: ["provenance-envelope"],
            TraceRefs: ["trace-envelope"],
            RejectedAlternatives: [],
            Conflicts: CreateDerivedConflicts(branchArray),
            FailureReasonCodes: [],
            UseContextCarryForward: true);
    }

    private static GraphAsyncBranchResult CreateBranch(
        string branchId,
        string pathOrderRef,
        string verdict,
        double confidence,
        string decision,
        bool divergentMetrics)
    {
        var passed = verdict == "PASS";
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["decision"] = divergentMetrics ? decision : "shared"
        };
        var summary = new LogicalFlowSummary(
            SummaryId: $"summary-{branchId}",
            Tier: 1,
            BaselinePrimary: confidence,
            EnhancedPrimary: confidence,
            PrimaryMetricName: "quality",
            LowerIsBetter: false,
            Passed: passed,
            ShouldScaleUp: passed,
            NextAction: passed ? "continue" : "repair",
            ScalingSignal: confidence,
            ScalingSignalNote: "diagnostic",
            Notes: branchId,
            BaselineTimeMs: 10,
            EnhancedTimeMs: 12);
        var inner = new LogicalFlowResult(
            FlowId: $"flow-{branchId}",
            ScenarioId: $"scenario-{branchId}",
            OverallVerdict: verdict,
            FinalHumanResult: $"result-{branchId}",
            Summary: summary,
            Metrics: metrics,
            EvidenceIds: [$"evidence-{branchId}"],
            Warnings: [])
        {
            TransitionSignals = []
        };

        return new GraphAsyncBranchResult(
            CombinationId: $"combination-{branchId}",
            BranchId: branchId,
            OverallVerdict: verdict,
            Confidence: confidence,
            SourceHashes: [$"source-hash-{branchId}"],
            Inner: inner,
            MergeKey: MergeKey,
            PathOrderRef: pathOrderRef,
            ProvenanceRefs: [$"provenance-{branchId}"],
            TraceRefs: [$"trace-{branchId}"],
            ErrorCodes: []);
    }

    private static GraphFanInConflict[] CreateDerivedConflicts(
        IReadOnlyList<GraphAsyncBranchResult> branches)
    {
        var ordered = branches
            .OrderBy(branch => branch.MergeKey, StringComparer.Ordinal)
            .ThenBy(branch => branch.PathOrderRef, StringComparer.Ordinal)
            .ToArray();
        var conflicts = new List<GraphFanInConflict>();
        foreach (var metricKey in ordered
                     .SelectMany(branch => branch.Inner.Metrics.Keys)
                     .Distinct(StringComparer.Ordinal)
                     .OrderBy(key => key, StringComparer.Ordinal))
        {
            var states = ordered
                .Select(branch => branch.Inner.Metrics.TryGetValue(metricKey, out var value)
                    ? (IsPresent: true, Value: value)
                    : (IsPresent: false, Value: string.Empty))
                .ToArray();
            if (states.Distinct().Count() == 1)
                continue;

            var values = new SortedDictionary<string, string>(StringComparer.Ordinal);
            for (var index = 0; index < ordered.Length; index++)
            {
                var state = states[index];
                values.Add(
                    ordered[index].BranchId,
                    state.IsPresent ? $"P:{state.Value.Length}:{state.Value}" : "M:0:");
            }

            conflicts.Add(new GraphFanInConflict($"Metrics.{metricKey}", values));
        }

        return conflicts.ToArray();
    }

    private static GraphFanInMergeInput CreateRealMergerFailureInput(string caseName)
    {
        var input = CreateMergeInput(RunId);
        var branches = input.Branches.ToArray();
        return caseName switch
        {
            "no-branches" => input with { Branches = [], Conflicts = [] },
            "merge-key" => input with
            {
                Branches = ReplaceBranch(branches, 0, branches[0] with { MergeKey = "merge:other" })
            },
            "path-order" => input with
            {
                Branches = ReplaceBranch(branches, 0, branches[0] with { PathOrderRef = " " })
            },
            "provenance" => input with
            {
                Branches = ReplaceBranch(branches, 0, branches[0] with { ProvenanceRefs = [] })
            },
            "legal" => input with { LegalScope = "BLOCKED" },
            "conflict" => input with
            {
                Branches = ReplaceBranch(
                    branches,
                    1,
                    branches[1] with
                    {
                        Inner = branches[1].Inner with
                        {
                            Metrics = new Dictionary<string, string>(StringComparer.Ordinal)
                            {
                                ["decision"] = "different"
                            }
                        }
                    }),
                Conflicts = []
            },
            _ => throw new InvalidOperationException("Unknown real merger failure case.")
        };
    }

    private static GraphAsyncBranchResult[] ReplaceBranch(
        GraphAsyncBranchResult[] branches,
        int index,
        GraphAsyncBranchResult replacement)
    {
        var snapshot = branches.ToArray();
        snapshot[index] = replacement;
        return snapshot;
    }

    private static GraphFanInMergeResult CreateFakeMergeResult(
        FanInMergeRole role = FanInMergeRole.DeterministicFanInBaseline,
        bool mergeOrderIsAuthority = false,
        int mergedBranchCount = 2)
    {
        var merged = CreateStandaloneFlowResult();
        return new GraphFanInMergeResult(
            Merged: merged,
            Context: new LogicalContextPacket(
                RunId,
                [merged.FlowId],
                [merged.ScenarioId],
                [],
                DateTimeOffset.UnixEpoch),
            MergeRole: role,
            MergeOrderIsAuthority: mergeOrderIsAuthority,
            MergedBranchCount: mergedBranchCount,
            RejectedAlternatives: Array.Empty<GraphAsyncBranchResult>(),
            Conflicts: Array.Empty<GraphFanInConflict>(),
            ProvenanceRefs: Array.Empty<string>(),
            TraceRefs: Array.Empty<string>(),
            FailureReasonCodes: Array.Empty<string>());
    }

    private static LogicalFlowResult CreateStandaloneFlowResult()
        => new(
            FlowId: "flow-fake-merged",
            ScenarioId: "scenario-fake-merged",
            OverallVerdict: "PASS",
            FinalHumanResult: "fake merged result",
            Summary: new LogicalFlowSummary(
                SummaryId: "summary-fake-merged",
                Tier: 1,
                BaselinePrimary: 1,
                EnhancedPrimary: 1,
                PrimaryMetricName: "quality",
                LowerIsBetter: false,
                Passed: true,
                ShouldScaleUp: false,
                NextAction: "continue",
                ScalingSignal: 1,
                ScalingSignalNote: "diagnostic",
                Notes: "fake",
                BaselineTimeMs: 0,
                EnhancedTimeMs: 0),
            Metrics: new Dictionary<string, string>(StringComparer.Ordinal) { ["decision"] = "shared" },
            EvidenceIds: ["evidence-fake"],
            Warnings: []);

    private static MaterializerSpy CreateSuccessfulMaterializer()
        => new((_, _, input) => OperationResult<GraphFanInMergeInput>.Ok(
            CreateMergeInput(input.RunId)));

    private static MergerSpy CreateSuccessfulFakeMerger()
        => new(_ => OperationResult<GraphFanInMergeResult>.Ok(CreateFakeMergeResult()));

    private static void AssertCanonicalRecordShape<T>((string Name, Type Type)[] expected)
    {
        var constructor = Assert.Single(typeof(T).GetConstructors());
        Assert.Equal(
            expected,
            constructor.GetParameters()
                .Select(parameter => (parameter.Name!, parameter.ParameterType))
                .ToArray());
        Assert.Equal(
            expected.OrderBy(item => item.Name, StringComparer.Ordinal),
            typeof(T).GetProperties()
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .Select(property => (property.Name, property.PropertyType)));
    }

    private static void AssertFailure(
        OperationResult<LogicalFlowResult> result,
        string expectedCode,
        string? expectedMessage = null)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        var canonicalMessage = expectedMessage ?? GetCanonicalLocalMessage(expectedCode);
        Assert.Equal(canonicalMessage, result.ErrorMessage);
    }

    private static string GetCanonicalLocalMessage(string errorCode)
        => errorCode switch
        {
            "CANCELLED" => "Cancelled.",
            "DYNAMIC_GRAPH_EDGE_REQUEST_REQUIRED" => "Dynamic graph edge flow request is required.",
            "DYNAMIC_GRAPH_EDGE_INPUT_REQUIRED" => "Dynamic graph edge traversal input is required.",
            "DYNAMIC_GRAPH_EDGE_RUN_ID_REQUIRED" => "Dynamic graph edge RunId is required.",
            "DYNAMIC_GRAPH_EDGE_RUN_ID_MISMATCH" => "Flow request RunId does not match edge traversal RunId.",
            "DYNAMIC_GRAPH_EDGE_FLOW_ID_REQUIRED" => "Dynamic graph edge FlowId is required.",
            "DYNAMIC_GRAPH_EDGE_SCENARIO_ID_REQUIRED" => "Dynamic graph edge ScenarioId is required.",
            "DYNAMIC_GRAPH_EDGE_ID_REQUIRED" => "Dynamic graph edge traversal EdgeId is required.",
            "DYNAMIC_GRAPH_EDGE_SOURCE_DOCUMENT_REQUIRED" => "Dynamic graph edge source document is required.",
            "DYNAMIC_GRAPH_EDGE_CONTEXT_REQUIRED" => "Dynamic graph edge context is required.",
            "DYNAMIC_GRAPH_UPSTREAM_OUTPUT_REQUIRED" => "Upstream node output is required.",
            "DYNAMIC_GRAPH_UPSTREAM_SOURCE_REQUIRED" => "Upstream source node id is required.",
            "DYNAMIC_GRAPH_UPSTREAM_REFS_INVALID" => "Upstream candidate references must be materialized and non-empty.",
            "DYNAMIC_GRAPH_UPSTREAM_REFS_NOT_CANONICAL" => "Upstream candidate references must be distinct and ordinal-sorted.",
            "DYNAMIC_GRAPH_UPSTREAM_HASH_REQUIRED" => "Upstream output hash is required.",
            "DYNAMIC_GRAPH_UPSTREAM_PROJECTION_RULE_REQUIRED" => "Upstream projection rule is required.",
            "EDGE_FAN_IN_MERGER_REQUIRED" => "Edge fan-in merger is required.",
            "EDGE_FAN_IN_INPUT_MATERIALIZER_REQUIRED" => "Edge fan-in input materializer is required.",
            "DYNAMIC_GRAPH_EDGE_SOURCE_MISMATCH" => "Upstream source node does not match the selected edge source.",
            "EDGE_FAN_IN_INPUT_MATERIALIZER_FAILED" => "Edge fan-in input materialization failed.",
            "EDGE_FAN_IN_MERGE_INPUT_REQUIRED" => "Materialized edge fan-in merge input is required.",
            "EDGE_FAN_IN_RUN_ID_REQUIRED" => "Materialized edge fan-in RunId is required.",
            "EDGE_FAN_IN_RUN_ID_MISMATCH" => "Materialized edge fan-in RunId does not match traversal RunId.",
            "EDGE_FAN_IN_MERGER_FAILED" => "Edge fan-in merge failed.",
            "EDGE_FAN_IN_MERGE_RESULT_REQUIRED" => "Edge fan-in merge result is required.",
            "BEST_OF_MERGE_FORBIDDEN" => "Fan-in merge must use the deterministic baseline role.",
            "MERGE_ORDER_USED_AS_AUTHORITY" => "Fan-in merge order cannot be semantic authority.",
            "DYNAMIC_GRAPH_FAN_IN_BRANCH_CARDINALITY_INVALID" => "Dynamic graph FanIn traversal requires at least two merged branches.",
            _ => throw new InvalidOperationException(
                $"No canonical local error message is registered for code '{errorCode}'.")
        };

    private static string StructuralJson<T>(T value)
        => JsonSerializer.Serialize(value);

    private sealed record TestNode(
        string NodeId,
        DynamicGraphNodeDescriptor Descriptor) : IDynamicGraphNode;

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

    private sealed class AuthorityPipelineSpy : IDynamicGraphEdgeAuthorityExecutionPipeline
    {
        private readonly Func<
            IDynamicGraphEdge,
            EdgeTraversalInput,
            string,
            LogicalExecutionFlags,
            CancellationToken,
            OperationResult<EdgeAuthorityExecutionResult>> _handler;

        public AuthorityPipelineSpy(Func<
            IDynamicGraphEdge,
            EdgeTraversalInput,
            string,
            LogicalExecutionFlags,
            CancellationToken,
            OperationResult<EdgeAuthorityExecutionResult>> handler)
            => _handler = handler;

        public int CallCount { get; private set; }

        public IDynamicGraphEdge? Edge { get; private set; }

        public EdgeTraversalInput? Input { get; private set; }

        public string? DomainTag { get; private set; }

        public LogicalExecutionFlags? Flags { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<OperationResult<EdgeAuthorityExecutionResult>> ExecuteAsync(
            IDynamicGraphEdge edge,
            EdgeTraversalInput input,
            string domainTag,
            LogicalExecutionFlags flags,
            CancellationToken cancellationToken = default)
        {
            if (CallCount != 0)
                throw new InvalidOperationException("Authority pipeline may be invoked only once.");

            CallCount++;
            Edge = edge;
            Input = input;
            DomainTag = domainTag;
            Flags = flags;
            CancellationToken = cancellationToken;
            return Task.FromResult(_handler(edge, input, domainTag, flags, cancellationToken));
        }
    }

    private sealed class MaterializerSpy
    {
        private readonly Func<
            DynamicGraphTopology,
            IDynamicGraphEdge,
            EdgeTraversalInput,
            OperationResult<GraphFanInMergeInput>> _handler;

        public MaterializerSpy(Func<
            DynamicGraphTopology,
            IDynamicGraphEdge,
            EdgeTraversalInput,
            OperationResult<GraphFanInMergeInput>> handler)
            => _handler = handler;

        public int CallCount { get; private set; }

        public DynamicGraphTopology? Topology { get; private set; }

        public IDynamicGraphEdge? Edge { get; private set; }

        public EdgeTraversalInput? Input { get; private set; }

        public OperationResult<GraphFanInMergeInput> Invoke(
            DynamicGraphTopology topology,
            IDynamicGraphEdge edge,
            EdgeTraversalInput input)
        {
            CallCount++;
            Topology = topology;
            Edge = edge;
            Input = input;
            return _handler(topology, edge, input);
        }
    }

    private sealed class MergerSpy : IEdgeFanInMerger
    {
        private readonly Func<GraphFanInMergeInput, OperationResult<GraphFanInMergeResult>> _handler;

        public MergerSpy(Func<GraphFanInMergeInput, OperationResult<GraphFanInMergeResult>> handler)
            => _handler = handler;

        public int CallCount { get; private set; }

        public GraphFanInMergeInput? LastInput { get; private set; }

        public OperationResult<GraphFanInMergeResult> Merge(GraphFanInMergeInput input)
        {
            CallCount++;
            LastInput = input;
            return _handler(input);
        }
    }
}
