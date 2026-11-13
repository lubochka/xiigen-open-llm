// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class InMemoryDynamicGraphTopologyRuntimeTests
{
    private const string TenantId = "tenant-runtime";
    private const string DomainTag = "dynamic-graph-runtime";
    private const string ContextId = "context-runtime";
    private const string DependencyRequiredCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_DEPENDENCY_REQUIRED";
    private const string RequestRequiredCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_REQUEST_REQUIRED";
    private const string ExpectedRevisionInvalidCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_EXPECTED_REVISION_INVALID";
    private const string SignalPreviousForbiddenCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_SIGNAL_PREVIOUS_FORBIDDEN";
    private const string RevisionConflictCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_REVISION_CONFLICT";
    private const string CurrentNotFoundCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_CURRENT_NOT_FOUND";
    private const string BuilderResultRequiredCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_BUILDER_RESULT_REQUIRED";
    private const string BuilderFailedCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_BUILDER_FAILED";
    private const string BuilderResultInvalidCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_BUILDER_RESULT_INVALID";
    private const string ScopeMismatchCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_SCOPE_MISMATCH";
    private const string TopologyRequiredCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_TOPOLOGY_REQUIRED";
    private const string ExceptionCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_EXCEPTION";
    private const string BudgetExceededCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_EXCEEDED";
    private const string RestoreStateRequiredCode =
        "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_RESTORE_STATE_REQUIRED";
    private const string RestoreStateInvalidCode =
        "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_RESTORE_STATE_INVALID";
    private const string RestoreAlreadyConsumedCode =
        "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_RESTORE_ALREADY_CONSUMED";

    private static readonly string[] RequiredLeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];

    [Fact]
    public void Contracts_ExposeExactReadOnlySourceAndOwnerSurface()
    {
        AssertRecordShape<DynamicGraphTopologyApplyRequest>(
            [
                ("ExpectedRevision", typeof(long)),
                ("Signal", typeof(DynamicGraphBuildSignal)),
                ("Budget", typeof(TopologyBuildBudget))
            ]);
        AssertRecordShape<DynamicGraphTopologyRuntimeSnapshot>(
            [
                ("Revision", typeof(long)),
                ("Topology", typeof(DynamicGraphTopology)),
                ("SignalId", typeof(string)),
                ("SourceKind", typeof(DynamicGraphBuildSourceKind)),
                ("SourceRef", typeof(string)),
                ("TraceRef", typeof(string)),
                ("BudgetProof", typeof(TopologyBudgetProof)),
                ("MutationEvidence", typeof(TopologyMutationEvidence))
            ]);
        AssertRecordShape<DynamicGraphTopologyApplyResult>(
            [
                ("PreviousRevision", typeof(long)),
                ("CurrentRevision", typeof(long)),
                ("RevisionAdvanced", typeof(bool)),
                ("BuildResult", typeof(BuildDynamicTopologyResult)),
                ("Current", typeof(DynamicGraphTopologyRuntimeSnapshot))
            ]);

        var sourceMethod = Assert.Single(typeof(IDynamicGraphTopologySource).GetMethods());
        Assert.Equal(nameof(IDynamicGraphTopologySource.ReadCurrent), sourceMethod.Name);
        Assert.Equal(typeof(OperationResult<DynamicGraphTopologyRuntimeSnapshot>), sourceMethod.ReturnType);
        Assert.Empty(sourceMethod.GetParameters());

        Assert.Equal([typeof(IDynamicGraphTopologySource)], typeof(IDynamicGraphTopologyRuntimeOwner).GetInterfaces());
        var ownerMethods = typeof(IDynamicGraphTopologyRuntimeOwner).GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Equal(
            [nameof(IDynamicGraphTopologyRuntimeOwner.ApplyBuildSignal), nameof(IDynamicGraphTopologyRuntimeOwner.ListRevisions)],
            ownerMethods.Select(method => method.Name).OrderBy(name => name, StringComparer.Ordinal));
        Assert.Equal(
            [typeof(IDynamicGraphTopologyRuntimeOwner), typeof(IDynamicGraphTopologySource)],
            typeof(InMemoryDynamicGraphTopologyRuntime).GetInterfaces()
                .OrderBy(type => type.Name, StringComparer.Ordinal));
        Assert.Empty(typeof(InMemoryDynamicGraphTopologyRuntime).GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    [Fact]
    public void PublicSurface_HasNoLearningTeacherJudgeWinnerOrPersistenceAuthority()
    {
        var boundedTypes = new[]
        {
            typeof(DynamicGraphTopologyApplyRequest),
            typeof(DynamicGraphTopologyRuntimeSnapshot),
            typeof(DynamicGraphTopologyApplyResult),
            typeof(IDynamicGraphTopologySource),
            typeof(IDynamicGraphTopologyRuntimeOwner),
            typeof(InMemoryDynamicGraphTopologyRuntime)
        };
        var forbidden = new[] { "Train", "Dpo", "Teacher", "Judge", "Winner", "Persist", "Checkpoint" };

        Assert.DoesNotContain(
            boundedTypes.SelectMany(type => type.GetMembers(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)),
            member => forbidden.Any(token => member.Name.Contains(token, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void NullDependency_ConstructorDoesNotThrowAndEveryOperationFailsClosed()
    {
        var runtime = new InMemoryDynamicGraphTopologyRuntime(null!);

        AssertFailure(runtime.ReadCurrent(), DependencyRequiredCode);
        AssertFailure(runtime.ListRevisions(), DependencyRequiredCode);
        AssertFailure(runtime.ApplyBuildSignal(CreateRequest(0, CreateSingleTopology())), DependencyRequiredCode);
    }

    [Fact]
    public void EmptyRuntime_ReadFailsAndHistoryIsSuccessfulEmptySnapshot()
    {
        var runtime = CreateRuntime();

        AssertFailure(runtime.ReadCurrent(), CurrentNotFoundCode);
        var history = AssertSuccess(runtime.ListRevisions());
        Assert.Empty(history);
    }

    [Fact]
    public void Apply_RejectsNullRequestNegativeRevisionAndCallerPreviousBeforeBuilder()
    {
        var builder = new TrackingBuilder(new DeterministicDynamicGraphTopologyBuilder());
        var runtime = new InMemoryDynamicGraphTopologyRuntime(builder);
        var topology = CreateSingleTopology();

        AssertFailure(runtime.ApplyBuildSignal(null!), RequestRequiredCode);
        AssertFailure(runtime.ApplyBuildSignal(CreateRequest(-1, topology)), ExpectedRevisionInvalidCode);
        var request = CreateRequest(0, topology) with
        {
            Signal = CreateSignal(topology, "previous-forbidden") with
            {
                PreviousTopology = CreateSingleTopology()
            }
        };
        AssertFailure(runtime.ApplyBuildSignal(request), SignalPreviousForbiddenCode);
        Assert.Equal(0, builder.CallCount);
    }

    [Fact]
    public void InitialAcceptedBuild_AdvancesZeroToOneAndPreservesMetadataAndProof()
    {
        var runtime = CreateRuntime();
        var topology = CreateSingleTopology();
        var request = CreateRequest(
            0,
            topology,
            signalId: "signal-initial",
            sourceKind: DynamicGraphBuildSourceKind.LogicalRegistryProjection,
            sourceRef: "registry:alpha",
            traceRef: "trace:alpha");

        var applied = AssertSuccess(runtime.ApplyBuildSignal(request));

        Assert.Equal(0, applied.PreviousRevision);
        Assert.Equal(1, applied.CurrentRevision);
        Assert.True(applied.RevisionAdvanced);
        Assert.Null(applied.BuildResult.FailureReason);
        Assert.True(applied.BuildResult.MutationEvidence.MutationApplied);
        var current = Assert.IsType<DynamicGraphTopologyRuntimeSnapshot>(applied.Current);
        Assert.Equal(1, current.Revision);
        Assert.Equal("signal-initial", current.SignalId);
        Assert.Equal(DynamicGraphBuildSourceKind.LogicalRegistryProjection, current.SourceKind);
        Assert.Equal("registry:alpha", current.SourceRef);
        Assert.Equal("trace:alpha", current.TraceRef);
        Assert.Equal(ValidBudget().MaxNodes, current.BudgetProof.MaxNodes);
        Assert.Equal(ValidBudget().MaxEdges, current.BudgetProof.MaxEdges);
        Assert.Equal(ValidBudget().MaxDepth, current.BudgetProof.MaxDepth);
        Assert.Equal(ValidBudget().MaxParallelBranches, current.BudgetProof.MaxParallelBranches);
        Assert.True(current.BudgetProof.WithinBudget);
        Assert.Equal(1, current.BudgetProof.ActualNodes);
        Assert.Equal(["node-a"], current.MutationEvidence.ProposedAddedNodeIds);
        Assert.Single(AssertSuccess(runtime.ListRevisions()));
    }

    [Fact]
    public void AcceptedMutation_AdvancesExactlyOneRevisionAndBuilderReceivesCurrentSnapshot()
    {
        DynamicGraphTopology? previousSeen = null;
        var builder = new DelegateBuilder((signal, budget) =>
        {
            previousSeen = signal.PreviousTopology;
            return new DeterministicDynamicGraphTopologyBuilder().BuildDynamicTopology(signal, budget);
        });
        var runtime = new InMemoryDynamicGraphTopologyRuntime(builder);
        AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(0, CreateSingleTopology(), signalId: "signal-one")));

        var nextTopology = CreateChainTopology("node-a", "node-b");
        var applied = AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(
            1,
            nextTopology,
            signalId: "signal-two",
            sourceKind: DynamicGraphBuildSourceKind.MachineTopologyProposal)));

        Assert.NotNull(previousSeen);
        Assert.Equal(["node-a"], previousSeen!.Nodes.Select(node => node.NodeId));
        Assert.Equal(1, applied.PreviousRevision);
        Assert.Equal(2, applied.CurrentRevision);
        Assert.True(applied.RevisionAdvanced);
        Assert.Equal(["node-a", "node-b"], applied.Current!.Topology.Nodes.Select(node => node.NodeId));
        Assert.Equal([1L, 2L], AssertSuccess(runtime.ListRevisions()).Select(snapshot => snapshot.Revision));
    }

    [Fact]
    public void StaleRevision_IsRejectedBeforeBuilderInvocation()
    {
        var builder = new TrackingBuilder(new DeterministicDynamicGraphTopologyBuilder());
        var runtime = new InMemoryDynamicGraphTopologyRuntime(builder);
        AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(0, CreateSingleTopology())));
        var callsBeforeConflict = builder.CallCount;

        AssertFailure(
            runtime.ApplyBuildSignal(CreateRequest(0, CreateChainTopology("node-a", "node-b"))),
            RevisionConflictCode);

        Assert.Equal(callsBeforeConflict, builder.CallCount);
        Assert.Equal(1, AssertSuccess(runtime.ReadCurrent()).Revision);
    }

    [Fact]
    public async Task ConcurrentWriters_WithSameExpectedRevisionYieldOneAdvanceAndOneConflict()
    {
        var builder = new TrackingBuilder(new DeterministicDynamicGraphTopologyBuilder());
        var runtime = new InMemoryDynamicGraphTopologyRuntime(builder);
        AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(0, CreateSingleTopology())));
        var topology = CreateChainTopology("node-a", "node-b");
        using var ready = new CountdownEvent(2);
        using var start = new ManualResetEventSlim(false);

        Task<OperationResult<DynamicGraphTopologyApplyResult>> Run(string signalId) => Task.Run(() =>
        {
            ready.Signal();
            start.Wait();
            return runtime.ApplyBuildSignal(CreateRequest(
                1,
                topology,
                signalId: signalId,
                sourceKind: DynamicGraphBuildSourceKind.MachineTopologyProposal));
        });

        var first = Run("concurrent-a");
        var second = Run("concurrent-b");
        Assert.True(ready.Wait(TimeSpan.FromSeconds(5)));
        start.Set();
        var results = await Task.WhenAll(first, second);

        Assert.Single(results, result => result.IsSuccess);
        Assert.Single(results, result => !result.IsSuccess && result.ErrorCode == RevisionConflictCode);
        Assert.Equal(2, AssertSuccess(runtime.ReadCurrent()).Revision);
        Assert.Equal([1L, 2L], AssertSuccess(runtime.ListRevisions()).Select(snapshot => snapshot.Revision));
        Assert.Equal(2, builder.CallCount);
    }

    [Fact]
    public void NoOpBuild_RetainsRevisionAndDoesNotAppendHistory()
    {
        var runtime = CreateRuntime();
        var topology = CreateSingleTopology();
        AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(0, topology)));

        var applied = AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(
            1,
            topology,
            signalId: "signal-noop",
            sourceKind: DynamicGraphBuildSourceKind.MachineTopologyProposal)));

        Assert.Equal(1, applied.PreviousRevision);
        Assert.Equal(1, applied.CurrentRevision);
        Assert.False(applied.RevisionAdvanced);
        Assert.False(applied.BuildResult.MutationEvidence.MutationApplied);
        Assert.Equal(1, applied.Current!.Revision);
        Assert.Single(AssertSuccess(runtime.ListRevisions()));
    }

    [Fact]
    public void InitialBudgetRejection_KeepsRuntimeEmptyAndDoesNotAppendHistory()
    {
        var runtime = CreateRuntime();
        var rejected = AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(
            0,
            CreateChainTopology("node-a", "node-b"),
            budget: new TopologyBuildBudget(1, 4, 4, 4))));

        Assert.Equal(BudgetExceededCode, rejected.BuildResult.FailureReason);
        Assert.False(rejected.RevisionAdvanced);
        Assert.Equal(0, rejected.PreviousRevision);
        Assert.Equal(0, rejected.CurrentRevision);
        Assert.Null(rejected.Current);
        AssertFailure(runtime.ReadCurrent(), CurrentNotFoundCode);
        Assert.Empty(AssertSuccess(runtime.ListRevisions()));
    }

    [Fact]
    public void BudgetRejection_WithCurrentRetainsStateRevisionAndHistory()
    {
        var runtime = CreateRuntime();
        AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(0, CreateSingleTopology())));

        var rejected = AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(
            1,
            CreateChainTopology("node-a", "node-b"),
            signalId: "signal-rejected",
            sourceKind: DynamicGraphBuildSourceKind.MachineTopologyProposal,
            budget: new TopologyBuildBudget(1, 4, 4, 4))));

        Assert.Equal(BudgetExceededCode, rejected.BuildResult.FailureReason);
        Assert.False(rejected.RevisionAdvanced);
        Assert.Equal(1, rejected.CurrentRevision);
        Assert.Equal(["node-a"], rejected.Current!.Topology.Nodes.Select(node => node.NodeId));
        Assert.Equal(1, AssertSuccess(runtime.ReadCurrent()).Revision);
        Assert.Single(AssertSuccess(runtime.ListRevisions()));
    }

    [Fact]
    public void BuilderFailure_IsSanitizedWithoutStateChange()
    {
        var runtime = new InMemoryDynamicGraphTopologyRuntime(new DelegateBuilder(
            (_, _) => OperationResult<BuildDynamicTopologyResult>.Fail("sentinel failure", "SENTINEL_BUILDER_CODE")));

        var result = runtime.ApplyBuildSignal(CreateRequest(0, CreateSingleTopology()));

        AssertFailure(result, BuilderFailedCode);
        Assert.DoesNotContain("sentinel", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(AssertSuccess(runtime.ListRevisions()));
    }

    [Fact]
    public void NullOperationAndSuccessfulNullValue_AreSanitizedAsRequiredResult()
    {
        var nullOperation = new InMemoryDynamicGraphTopologyRuntime(new DelegateBuilder((_, _) => null!));
        var nullValue = new InMemoryDynamicGraphTopologyRuntime(new DelegateBuilder(
            (_, _) => OperationResult<BuildDynamicTopologyResult>.Ok(null!)));

        AssertFailure(nullOperation.ApplyBuildSignal(CreateRequest(0, CreateSingleTopology())), BuilderResultRequiredCode);
        AssertFailure(nullValue.ApplyBuildSignal(CreateRequest(0, CreateSingleTopology())), BuilderResultRequiredCode);
    }

    [Fact]
    public void ThrowingBuilder_IsSanitizedWithoutExceptionDetails()
    {
        var runtime = new InMemoryDynamicGraphTopologyRuntime(new DelegateBuilder(
            (_, _) => throw new InvalidOperationException("secret exception detail")));

        var result = runtime.ApplyBuildSignal(CreateRequest(0, CreateSingleTopology()));

        AssertFailure(result, ExceptionCode);
        Assert.DoesNotContain("secret", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("InvalidOperationException", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void BuilderCancellationWithoutRuntimeCancellationAuthority_PropagatesAndDoesNotPublish()
    {
        var cancellation = new OperationCanceledException("builder-owned cancellation");
        var runtime = new InMemoryDynamicGraphTopologyRuntime(new DelegateBuilder(
            (_, _) => throw cancellation));

        var thrown = Assert.Throws<OperationCanceledException>(() =>
            runtime.ApplyBuildSignal(CreateRequest(0, CreateSingleTopology())));

        Assert.Same(cancellation, thrown);
        AssertFailure(runtime.ReadCurrent(), CurrentNotFoundCode);
        Assert.Empty(AssertSuccess(runtime.ListRevisions()));
    }

    [Fact]
    public void WrappedBuilderCancellation_PropagatesExactOuterExceptionAndDoesNotPublish()
    {
        var wrappedCancellation = new TypeInitializationException(
            "builder-type",
            new OperationCanceledException("builder-owned cancellation"));
        var runtime = new InMemoryDynamicGraphTopologyRuntime(new DelegateBuilder(
            (_, _) => throw wrappedCancellation));

        var thrown = Assert.Throws<TypeInitializationException>(() =>
            runtime.ApplyBuildSignal(CreateRequest(0, CreateSingleTopology())));

        Assert.Same(wrappedCancellation, thrown);
        AssertFailure(runtime.ReadCurrent(), CurrentNotFoundCode);
        Assert.Empty(AssertSuccess(runtime.ListRevisions()));
    }

    [Fact]
    public void AggregateBuilderFailure_WithFatalNonFirstChild_PropagatesExactOuterAndDoesNotPublish()
    {
        var aggregate = new AggregateException(
            "aggregate builder failure",
            new InvalidOperationException("ordinary first child"),
            new BadImageFormatException("fatal second child"));
        var runtime = new InMemoryDynamicGraphTopologyRuntime(new DelegateBuilder(
            (_, _) => throw aggregate));

        var thrown = Assert.Throws<AggregateException>(() =>
            runtime.ApplyBuildSignal(CreateRequest(0, CreateSingleTopology())));

        Assert.Same(aggregate, thrown);
        AssertFailure(runtime.ReadCurrent(), CurrentNotFoundCode);
        Assert.Empty(AssertSuccess(runtime.ListRevisions()));
    }

    [Fact]
    public void WrappedNonFatalBuilderFailure_IsSanitizedWithoutDetailsOrStateChange()
    {
        var runtime = new InMemoryDynamicGraphTopologyRuntime(new DelegateBuilder(
            (_, _) => throw new ApplicationException(
                "outer secret",
                new InvalidOperationException("inner secret"))));

        var result = runtime.ApplyBuildSignal(CreateRequest(0, CreateSingleTopology()));

        AssertFailure(result, ExceptionCode);
        Assert.DoesNotContain("secret", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ApplicationException", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidOperationException", result.ErrorMessage, StringComparison.Ordinal);
        AssertFailure(runtime.ReadCurrent(), CurrentNotFoundCode);
        Assert.Empty(AssertSuccess(runtime.ListRevisions()));
    }

    [Theory]
    [InlineData("unknown-failure")]
    [InlineData("accepted-outside-budget")]
    [InlineData("initial-no-mutation")]
    [InlineData("signal-id-mismatch")]
    [InlineData("source-kind-mismatch")]
    [InlineData("source-ref-mismatch")]
    [InlineData("trace-ref-mismatch")]
    public void MalformedAcceptedBuilderOutputs_AreRejected(string caseName)
    {
        var runtime = new InMemoryDynamicGraphTopologyRuntime(new DelegateBuilder((signal, budget) =>
        {
            var good = AssertSuccess(
                new DeterministicDynamicGraphTopologyBuilder().BuildDynamicTopology(signal, budget));
            var malformed = caseName switch
            {
                "unknown-failure" => good with { FailureReason = "UNKNOWN_REASON" },
                "accepted-outside-budget" => good with
                {
                    BudgetProof = good.BudgetProof with { WithinBudget = false }
                },
                "initial-no-mutation" => good with
                {
                    MutationEvidence = EmptyEvidence(signal, mutationApplied: false)
                },
                "signal-id-mismatch" => good with
                {
                    MutationEvidence = good.MutationEvidence with { SignalId = "different" }
                },
                "source-kind-mismatch" => good with
                {
                    MutationEvidence = good.MutationEvidence with
                    {
                        SourceKind = DynamicGraphBuildSourceKind.MachineTopologyProposal
                    }
                },
                "source-ref-mismatch" => good with
                {
                    MutationEvidence = good.MutationEvidence with { SourceRef = "different" }
                },
                "trace-ref-mismatch" => good with
                {
                    MutationEvidence = good.MutationEvidence with { TraceRef = "different" }
                },
                _ => throw new ArgumentOutOfRangeException(nameof(caseName))
            };
            return OperationResult<BuildDynamicTopologyResult>.Ok(malformed);
        }));

        AssertFailure(runtime.ApplyBuildSignal(CreateRequest(0, CreateSingleTopology())), BuilderResultInvalidCode);
        Assert.Empty(AssertSuccess(runtime.ListRevisions()));
    }

    [Fact]
    public void AcceptedAppliedResultWithoutTopology_UsesTopologyRequiredCode()
    {
        var runtime = new InMemoryDynamicGraphTopologyRuntime(new DelegateBuilder((signal, budget) =>
        {
            var good = AssertSuccess(
                new DeterministicDynamicGraphTopologyBuilder().BuildDynamicTopology(signal, budget));
            return OperationResult<BuildDynamicTopologyResult>.Ok(good with { Topology = null });
        }));

        AssertFailure(runtime.ApplyBuildSignal(CreateRequest(0, CreateSingleTopology())), TopologyRequiredCode);
    }

    [Theory]
    [InlineData("within-budget")]
    [InlineData("mutation-applied")]
    [InlineData("initial-topology-retained")]
    public void MalformedBudgetRejectedOutputs_AreRejected(string caseName)
    {
        var runtime = new InMemoryDynamicGraphTopologyRuntime(new DelegateBuilder((signal, budget) =>
        {
            var good = AssertSuccess(
                new DeterministicDynamicGraphTopologyBuilder().BuildDynamicTopology(signal, budget));
            Assert.Equal(BudgetExceededCode, good.FailureReason);
            var malformed = caseName switch
            {
                "within-budget" => good with
                {
                    BudgetProof = good.BudgetProof with { WithinBudget = true }
                },
                "mutation-applied" => good with
                {
                    MutationEvidence = good.MutationEvidence with { MutationApplied = true }
                },
                "initial-topology-retained" => good with { Topology = signal.ProposedTopology },
                _ => throw new ArgumentOutOfRangeException(nameof(caseName))
            };
            return OperationResult<BuildDynamicTopologyResult>.Ok(malformed);
        }));

        AssertFailure(
            runtime.ApplyBuildSignal(CreateRequest(
                0,
                CreateChainTopology("node-a", "node-b"),
                budget: new TopologyBuildBudget(1, 4, 4, 4))),
            BuilderResultInvalidCode);
        Assert.Empty(AssertSuccess(runtime.ListRevisions()));
    }

    [Fact]
    public void InvalidOutputTopology_PreservesValidatorFailureCode()
    {
        var invalid = new DynamicGraphTopology(
            TenantId,
            DomainTag,
            ContextId,
            [CreateNode("node-a", domainTag: "wrong-domain")],
            [],
            [CreateOrder("node-a", 0)]);
        var validatorFailure = DynamicGraphValidation.Validate(invalid);
        Assert.False(validatorFailure.IsSuccess);
        var runtime = new InMemoryDynamicGraphTopologyRuntime(new DelegateBuilder((signal, budget) =>
        {
            var good = AssertSuccess(
                new DeterministicDynamicGraphTopologyBuilder().BuildDynamicTopology(signal, budget));
            return OperationResult<BuildDynamicTopologyResult>.Ok(good with { Topology = invalid });
        }));

        AssertFailure(
            runtime.ApplyBuildSignal(CreateRequest(0, CreateSingleTopology())),
            validatorFailure.ErrorCode);
    }

    [Theory]
    [InlineData("tenant")]
    [InlineData("domain")]
    [InlineData("context")]
    public void CallerScopeMismatch_IsRejectedBeforeBuilderWithoutChangingAcceptedState(string mismatch)
    {
        var builderCalls = 0;
        var runtime = new InMemoryDynamicGraphTopologyRuntime(new DelegateBuilder((signal, budget) =>
        {
            builderCalls++;
            return new DeterministicDynamicGraphTopologyBuilder().BuildDynamicTopology(signal, budget);
        }));
        AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(
            0,
            CreateSingleTopology(),
            signalId: "accepted-scope")));
        var before = AssertSuccess(runtime.ReadCurrent());
        var beforeFingerprint = SnapshotFingerprint(before);
        var proposed = mismatch switch
        {
            "tenant" => CreateScopedSingleTopology("tenant-other", DomainTag, ContextId),
            "domain" => CreateScopedSingleTopology(TenantId, "domain-other", ContextId),
            "context" => CreateScopedSingleTopology(TenantId, DomainTag, "context-other"),
            _ => throw new ArgumentOutOfRangeException(nameof(mismatch))
        };

        var result = runtime.ApplyBuildSignal(CreateRequest(
            1,
            proposed,
            signalId: $"malicious-{mismatch}",
            sourceKind: DynamicGraphBuildSourceKind.MachineTopologyProposal));

        AssertFailure(result, ScopeMismatchCode);
        Assert.Equal(1, builderCalls);
        var after = AssertSuccess(runtime.ReadCurrent());
        Assert.Equal(1, after.Revision);
        Assert.Equal(beforeFingerprint, SnapshotFingerprint(after));
        var history = AssertSuccess(runtime.ListRevisions());
        var saved = Assert.Single(history);
        Assert.Equal(beforeFingerprint, SnapshotFingerprint(saved));
        Assert.Equal("accepted-scope", saved.SignalId);
        Assert.Equal(TenantId, saved.Topology.TenantId);
        Assert.Equal(DomainTag, saved.Topology.DomainTag);
        Assert.Equal(ContextId, saved.Topology.ContextId);
    }

    [Fact]
    public void PublishedBuilderScopeFailure_IsPropagatedWithoutRuntimeWrapping()
    {
        const string publishedScopeMismatchCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_SCOPE_MISMATCH";
        var runtime = CreateRuntime();
        AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(0, CreateSingleTopology())));

        var result = runtime.ApplyBuildSignal(CreateRequest(
            1,
            CreateScopedSingleTopology(TenantId, "domain-other", ContextId),
            sourceKind: DynamicGraphBuildSourceKind.MachineTopologyProposal));

        AssertFailure(result, publishedScopeMismatchCode);
        Assert.Equal(1, AssertSuccess(runtime.ReadCurrent()).Revision);
        Assert.Single(AssertSuccess(runtime.ListRevisions()));
    }

    [Fact]
    public void CurrentCoherence_RejectsChangedNoOpAndUnchangedAppliedMutation()
    {
        var mode = "seed";
        var runtime = new InMemoryDynamicGraphTopologyRuntime(new DelegateBuilder((signal, budget) =>
        {
            var good = AssertSuccess(
                new DeterministicDynamicGraphTopologyBuilder().BuildDynamicTopology(signal, budget));
            if (mode == "changed-noop")
            {
                return OperationResult<BuildDynamicTopologyResult>.Ok(good with
                {
                    MutationEvidence = EmptyEvidence(signal, mutationApplied: false)
                });
            }

            if (mode == "unchanged-applied")
            {
                return OperationResult<BuildDynamicTopologyResult>.Ok(good with
                {
                    MutationEvidence = good.MutationEvidence with
                    {
                        ProposedAddedNodeIds = ["node-a"],
                        MutationApplied = true
                    }
                });
            }

            return OperationResult<BuildDynamicTopologyResult>.Ok(good);
        }));
        var currentTopology = CreateSingleTopology();
        AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(0, currentTopology)));

        mode = "changed-noop";
        AssertFailure(
            runtime.ApplyBuildSignal(CreateRequest(
                1,
                CreateChainTopology("node-a", "node-b"),
                sourceKind: DynamicGraphBuildSourceKind.MachineTopologyProposal)),
            BuilderResultInvalidCode);

        mode = "unchanged-applied";
        AssertFailure(
            runtime.ApplyBuildSignal(CreateRequest(
                1,
                currentTopology,
                sourceKind: DynamicGraphBuildSourceKind.MachineTopologyProposal)),
            BuilderResultInvalidCode);
        Assert.Single(AssertSuccess(runtime.ListRevisions()));
    }

    [Fact]
    public void History_IsAscendingAndExcludesNoOpAndBudgetRejectedAttempts()
    {
        var runtime = CreateRuntime();
        var first = CreateSingleTopology();
        AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(0, first, signalId: "accepted-one")));
        AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(
            1,
            first,
            signalId: "no-op",
            sourceKind: DynamicGraphBuildSourceKind.MachineTopologyProposal)));
        AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(
            1,
            CreateChainTopology("node-a", "node-b"),
            signalId: "budget-rejected",
            sourceKind: DynamicGraphBuildSourceKind.MachineTopologyProposal,
            budget: new TopologyBuildBudget(1, 4, 4, 4))));
        AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(
            1,
            CreateChainTopology("node-a", "node-b"),
            signalId: "accepted-two",
            sourceKind: DynamicGraphBuildSourceKind.MachineTopologyProposal)));

        var history = AssertSuccess(runtime.ListRevisions());
        Assert.Equal([1L, 2L], history.Select(snapshot => snapshot.Revision));
        Assert.Equal(["accepted-one", "accepted-two"], history.Select(snapshot => snapshot.SignalId));
    }

    [Fact]
    public void CurrentHistoryAndApplyResults_AreDeepDefensiveSnapshots()
    {
        var exceeded = new List<string>();
        var added = new List<string> { "node-a" };
        var topology = CreateSingleTopology();
        var runtime = new InMemoryDynamicGraphTopologyRuntime(new DelegateBuilder((signal, budget) =>
            OperationResult<BuildDynamicTopologyResult>.Ok(
                new BuildDynamicTopologyResult(
                    Topology: topology,
                    BudgetProof: new TopologyBudgetProof(
                        budget.MaxNodes,
                        1,
                        budget.MaxEdges,
                        0,
                        budget.MaxDepth,
                        0,
                        budget.MaxParallelBranches,
                        1,
                        true,
                        exceeded),
                    FailureReason: null,
                    MutationEvidence: new TopologyMutationEvidence(
                        signal.SignalId,
                        signal.SourceKind,
                        signal.SourceRef,
                        signal.TraceRef,
                        added,
                        [],
                        [],
                        [],
                        [],
                        [],
                        [],
                        true)))));

        var applied = AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(0, topology)));
        added[0] = "corrupted-node";
        exceeded.Add("nodes");
        var firstRead = AssertSuccess(runtime.ReadCurrent());
        var secondRead = AssertSuccess(runtime.ReadCurrent());
        var firstHistory = Assert.Single(AssertSuccess(runtime.ListRevisions()));
        var secondHistory = Assert.Single(AssertSuccess(runtime.ListRevisions()));

        Assert.Equal(["node-a"], firstRead.MutationEvidence.ProposedAddedNodeIds);
        Assert.Empty(firstRead.BudgetProof.ExceededDimensions);
        Assert.NotSame(applied.BuildResult.Topology, applied.Current!.Topology);
        Assert.NotSame(applied.Current.Topology, firstRead.Topology);
        Assert.NotSame(firstRead.Topology, secondRead.Topology);
        Assert.NotSame(firstRead.Topology.Nodes, secondRead.Topology.Nodes);
        Assert.NotSame(firstRead.Topology.Nodes[0], secondRead.Topology.Nodes[0]);
        Assert.NotSame(firstRead.BudgetProof.ExceededDimensions, secondRead.BudgetProof.ExceededDimensions);
        Assert.NotSame(firstRead.MutationEvidence.ProposedAddedNodeIds, secondRead.MutationEvidence.ProposedAddedNodeIds);
        Assert.NotSame(firstHistory.Topology, secondHistory.Topology);
        Assert.NotSame(firstHistory.Topology.Nodes, secondHistory.Topology.Nodes);
    }

    [Fact]
    public void EveryReturnedSurface_IsDeepDetachedReadOnlyAndCannotAliasStoredRevision()
    {
        var runtime = CreateRuntime();
        AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(
            0,
            CreateMutationBaseTopology(),
            signalId: "deep-base")));
        var applied = AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(
            1,
            CreateMutationProposedTopology(),
            signalId: "deep-mutation",
            sourceKind: DynamicGraphBuildSourceKind.MachineTopologyProposal)));
        var applyCurrent = Assert.IsType<DynamicGraphTopologyRuntimeSnapshot>(applied.Current);
        var buildTopology = Assert.IsType<DynamicGraphTopology>(applied.BuildResult.Topology);
        foreach (var evidenceList in EvidenceLists(applied.BuildResult.MutationEvidence))
            Assert.NotEmpty(evidenceList);

        var readOne = AssertSuccess(runtime.ReadCurrent());
        var readTwo = AssertSuccess(runtime.ReadCurrent());
        var historyOne = AssertSuccess(runtime.ListRevisions());
        var historyTwo = AssertSuccess(runtime.ListRevisions());
        Assert.Equal(2, historyOne.Count);
        Assert.Equal(2, historyTwo.Count);
        Assert.NotSame(historyOne, historyTwo);
        AssertReadOnlyList(historyOne);
        AssertReadOnlyList(historyTwo);
        var historyOneCurrent = historyOne[1];
        var historyTwoCurrent = historyTwo[1];
        var fingerprintBeforeAliasAttempts = SnapshotFingerprint(readOne);

        var surfaces = new[]
        {
            new ReturnedSurface(
                "apply-build-result",
                buildTopology,
                applied.BuildResult.BudgetProof,
                applied.BuildResult.MutationEvidence),
            new ReturnedSurface(
                "apply-current",
                applyCurrent.Topology,
                applyCurrent.BudgetProof,
                applyCurrent.MutationEvidence),
            new ReturnedSurface("read-one", readOne.Topology, readOne.BudgetProof, readOne.MutationEvidence),
            new ReturnedSurface("read-two", readTwo.Topology, readTwo.BudgetProof, readTwo.MutationEvidence),
            new ReturnedSurface(
                "history-one-current",
                historyOneCurrent.Topology,
                historyOneCurrent.BudgetProof,
                historyOneCurrent.MutationEvidence),
            new ReturnedSurface(
                "history-two-current",
                historyTwoCurrent.Topology,
                historyTwoCurrent.BudgetProof,
                historyTwoCurrent.MutationEvidence)
        };

        foreach (var surface in surfaces)
            AssertSurfaceReadOnly(surface);
        for (var left = 0; left < surfaces.Length; left++)
        {
            for (var right = left + 1; right < surfaces.Length; right++)
                AssertDetachedSurfaces(surfaces[left], surfaces[right]);
        }

        var afterAliasAttempts = AssertSuccess(runtime.ReadCurrent());
        Assert.Equal(fingerprintBeforeAliasAttempts, SnapshotFingerprint(afterAliasAttempts));
        Assert.Equal(2, afterAliasAttempts.Revision);
        Assert.Equal(2, AssertSuccess(runtime.ListRevisions()).Count);
    }

    [Fact]
    public void AcceptedCurrentTopology_AlwaysPassesCoreValidator()
    {
        var runtime = CreateRuntime();
        AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(
            0,
            CreateChainTopology("node-a", "node-b", "node-c"))));

        var validation = DynamicGraphValidation.Validate(AssertSuccess(runtime.ReadCurrent()).Topology);

        Assert.True(validation.IsSuccess, $"{validation.ErrorCode}: {validation.ErrorMessage}");
    }

    [Fact]
    public void RestoreCheckpoint_IsOneShotFailureAtomicAndReplacesOnlyBootstrapHistory()
    {
        var runtime = CreateRuntime();
        var topology = CreateSingleTopology();

        AssertFailure(
            runtime.RestoreCheckpoint(null!),
            RestoreStateRequiredCode);
        AssertFailure(
            runtime.RestoreCheckpoint(CreateRestoreState(topology)),
            RestoreStateInvalidCode);

        var bootstrap = AssertSuccess(runtime.ApplyBuildSignal(CreateRequest(
            0,
            topology,
            signalId: "restore-bootstrap")));
        Assert.Equal(1, bootstrap.CurrentRevision);
        var valid = CreateRestoreState(topology, revision: 4);
        var invalidStates = new[]
        {
            valid with { InitialRevisionAdvanced = false },
            valid with
            {
                Current = valid.Current with { Revision = long.MaxValue }
            },
            valid with
            {
                Current = valid.Current with
                {
                    SourceKind = DynamicGraphBuildSourceKind.MachineTopologyProposal
                }
            },
            valid with
            {
                Current = valid.Current with
                {
                    Topology = CreateScopedSingleTopology(
                        tenantId: "tenant-other",
                        DomainTag,
                        ContextId)
                }
            },
            valid with
            {
                Current = valid.Current with
                {
                    Topology = CreateChainTopology("node-a", "node-b")
                }
            },
            valid with
            {
                Current = valid.Current with
                {
                    BudgetProof = valid.Current.BudgetProof with { ActualNodes = 2 }
                }
            },
            valid with
            {
                Current = valid.Current with
                {
                    MutationEvidence = valid.Current.MutationEvidence with
                    {
                        ProposedUpdatedNodeIds = ["node-z", "node-a"]
                    }
                }
            },
            valid with
            {
                Current = valid.Current with
                {
                    MutationEvidence = EmptyEvidence(
                        CreateSignal(topology, "restore-current"),
                        mutationApplied: false)
                }
            }
        };

        foreach (var invalid in invalidStates)
        {
            AssertFailure(runtime.RestoreCheckpoint(invalid), RestoreStateInvalidCode);
            Assert.Equal(1, AssertSuccess(runtime.ReadCurrent()).Revision);
            Assert.Single(AssertSuccess(runtime.ListRevisions()));
        }

        var restored = AssertSuccess(runtime.RestoreCheckpoint(valid));
        Assert.Equal(4, restored.Revision);
        Assert.Equal("restore-current", restored.SignalId);
        Assert.Equal(["node-a"], restored.MutationEvidence.ProposedUpdatedNodeIds);
        var readback = AssertSuccess(runtime.ReadCurrent());
        var history = AssertSuccess(runtime.ListRevisions());
        Assert.Equal(4, readback.Revision);
        Assert.Equal("restore-current", readback.SignalId);
        Assert.Single(history);
        Assert.Equal(4, history[0].Revision);
        Assert.NotSame(restored.Topology, readback.Topology);
        Assert.NotSame(readback.Topology, history[0].Topology);
        AssertFailure(
            runtime.RestoreCheckpoint(valid),
            RestoreAlreadyConsumedCode);
        Assert.Equal(4, AssertSuccess(runtime.ReadCurrent()).Revision);
        Assert.Single(AssertSuccess(runtime.ListRevisions()));
    }

    private static InMemoryDynamicGraphTopologyRuntime CreateRuntime()
        => new(new DeterministicDynamicGraphTopologyBuilder());

    private static InMemoryDynamicGraphTopologyRestoreState CreateRestoreState(
        DynamicGraphTopology topology,
        long revision = 3)
    {
        var signal = CreateSignal(
            topology,
            signalId: "restore-current",
            sourceKind: DynamicGraphBuildSourceKind.LogicalRegistryProjection,
            sourceRef: "restore-source",
            traceRef: "restore-trace");
        return new InMemoryDynamicGraphTopologyRestoreState(
            new DynamicGraphTopologyRuntimeSnapshot(
                revision,
                topology,
                signal.SignalId,
                signal.SourceKind,
                signal.SourceRef,
                signal.TraceRef,
                new TopologyBudgetProof(
                    ValidBudget().MaxNodes,
                    topology.Nodes.Count,
                    ValidBudget().MaxEdges,
                    topology.Edges.Count,
                    ValidBudget().MaxDepth,
                    ActualDepth: 0,
                    ValidBudget().MaxParallelBranches,
                    ActualParallelBranches: 1,
                    WithinBudget: true,
                    ExceededDimensions: []),
                new TopologyMutationEvidence(
                    signal.SignalId,
                    signal.SourceKind,
                    signal.SourceRef,
                    signal.TraceRef,
                    ProposedAddedNodeIds: [],
                    ProposedRemovedNodeIds: [],
                    ProposedUpdatedNodeIds: ["node-a"],
                    ProposedAddedEdgeIds: [],
                    ProposedRemovedEdgeIds: [],
                    ProposedUpdatedEdgeIds: [],
                    ProposedReorderedNodeIds: [],
                    MutationApplied: true)),
            InitialRevisionAdvanced: true);
    }

    private static DynamicGraphTopologyApplyRequest CreateRequest(
        long expectedRevision,
        DynamicGraphTopology topology,
        string signalId = "signal-runtime",
        DynamicGraphBuildSourceKind sourceKind = DynamicGraphBuildSourceKind.LogicalRegistryProjection,
        string sourceRef = "source-runtime",
        string traceRef = "trace-runtime",
        TopologyBuildBudget? budget = null)
        => new(
            ExpectedRevision: expectedRevision,
            Signal: CreateSignal(topology, signalId, sourceKind, sourceRef, traceRef),
            Budget: budget ?? ValidBudget());

    private static DynamicGraphBuildSignal CreateSignal(
        DynamicGraphTopology topology,
        string signalId,
        DynamicGraphBuildSourceKind sourceKind = DynamicGraphBuildSourceKind.LogicalRegistryProjection,
        string sourceRef = "source-runtime",
        string traceRef = "trace-runtime")
        => new(
            SignalId: signalId,
            SourceKind: sourceKind,
            ProposedTopology: topology,
            PreviousTopology: null,
            SourceRef: sourceRef,
            TraceRef: traceRef);

    private static TopologyBuildBudget ValidBudget()
        => new(MaxNodes: 16, MaxEdges: 32, MaxDepth: 16, MaxParallelBranches: 8);

    private static DynamicGraphTopology CreateSingleTopology()
        => CreateScopedSingleTopology(TenantId, DomainTag, ContextId);

    private static DynamicGraphTopology CreateScopedSingleTopology(
        string tenantId,
        string domainTag,
        string contextId)
        => CreateTopology(
            [CreateNode("node-a", domainTag)],
            [],
            [CreateOrder("node-a", 0, tenantId, domainTag, contextId)],
            tenantId,
            domainTag,
            contextId);

    private static DynamicGraphTopology CreateChainTopology(params string[] nodeIds)
        => CreateTopology(
            nodeIds.Select(nodeId => CreateNode(nodeId)).Cast<IDynamicGraphNode>().ToArray(),
            nodeIds.Zip(nodeIds.Skip(1), (from, to) =>
                    CreateEdge($"edge-{from}-{to}", from, [to]))
                .Cast<IDynamicGraphEdge>()
                .ToArray(),
            nodeIds.Select((nodeId, index) => CreateOrder(nodeId, index)).ToArray());

    private static DynamicGraphTopology CreateTopology(
        IReadOnlyList<IDynamicGraphNode> nodes,
        IReadOnlyList<IDynamicGraphEdge> edges,
        IReadOnlyList<DynamicGraphNodeOrder> order,
        string tenantId = TenantId,
        string domainTag = DomainTag,
        string contextId = ContextId)
        => new(tenantId, domainTag, contextId, nodes, edges, order);

    private static TestNode CreateNode(
        string nodeId,
        string domainTag = DomainTag,
        string outputType = "runtime-output")
        => new(
            nodeId,
            new DynamicGraphNodeDescriptor(
                NodeId: nodeId,
                DomainTag: domainTag,
                TInputTypeName: "runtime-input",
                TOutputTypeName: outputType,
                IsTrained: false,
                DeterministicFinalAuthority: false,
                RuntimeSemanticQualityClaimed: false,
                AuthorityRole: "baseline_candidate_generator",
                CheckpointRef: null,
                ManifestRef: null));

    private static TestEdge CreateEdge(
        string edgeId,
        string fromNodeId,
        IReadOnlyList<string> toNodeIds)
        => new(
            EdgeId: edgeId,
            FromNodeId: fromNodeId,
            ToNodeIds: toNodeIds,
            Kind: GraphEdgeKind.FanOut,
            ParallelEligible: false,
            AuthorityUnitId: "authority-unit-runtime",
            AuthorityUnitLevel: "L2",
            DeterministicFinalAuthority: false,
            BaselineUnitId: "baseline-unit-runtime",
            BaselineRole: "candidate_generator",
            ExcludedLeakageSignals: RequiredLeakageSignals,
            SourceProof: "source-proof-runtime",
            CutoverProof: "cutover-proof-runtime",
            AblationProofRef: "ablation-proof-runtime");

    private static DynamicGraphNodeOrder CreateOrder(
        string nodeId,
        int position,
        string tenantId = TenantId,
        string domainTag = DomainTag,
        string contextId = ContextId)
        => new(
            OrderId: $"order-{nodeId}",
            NodeId: nodeId,
            TenantId: tenantId,
            DomainTag: domainTag,
            ContextId: contextId,
            Position: position);

    private static DynamicGraphTopology CreateMutationBaseTopology()
        => CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c")],
            [
                CreateEdge("edge-update", "node-a", ["node-b"]),
                CreateEdge("edge-remove", "node-b", ["node-c"])
            ],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1), CreateOrder("node-c", 2)]);

    private static DynamicGraphTopology CreateMutationProposedTopology()
        => CreateTopology(
            [
                CreateNode("node-a", outputType: "runtime-output-v2"),
                CreateNode("node-c"),
                CreateNode("node-d")
            ],
            [
                CreateEdge("edge-update", "node-a", ["node-c"]),
                CreateEdge("edge-add", "node-c", ["node-d"])
            ],
            [CreateOrder("node-a", 0), CreateOrder("node-c", 1), CreateOrder("node-d", 2)]);

    private static TopologyMutationEvidence EmptyEvidence(
        DynamicGraphBuildSignal signal,
        bool mutationApplied)
        => new(
            signal.SignalId,
            signal.SourceKind,
            signal.SourceRef,
            signal.TraceRef,
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            mutationApplied);

    private static IReadOnlyList<string>[] EvidenceLists(TopologyMutationEvidence evidence)
        =>
        [
            evidence.ProposedAddedNodeIds,
            evidence.ProposedRemovedNodeIds,
            evidence.ProposedUpdatedNodeIds,
            evidence.ProposedAddedEdgeIds,
            evidence.ProposedRemovedEdgeIds,
            evidence.ProposedUpdatedEdgeIds,
            evidence.ProposedReorderedNodeIds
        ];

    private static void AssertSurfaceReadOnly(ReturnedSurface surface)
    {
        AssertReadOnlyList(surface.Topology.Nodes);
        AssertReadOnlyList(surface.Topology.Edges);
        AssertReadOnlyList(surface.Topology.Order);
        foreach (var edge in surface.Topology.Edges)
        {
            AssertReadOnlyList(edge.ToNodeIds);
            AssertReadOnlyList(edge.ExcludedLeakageSignals);
        }

        AssertReadOnlyList(surface.BudgetProof.ExceededDimensions);
        foreach (var evidenceList in EvidenceLists(surface.MutationEvidence))
        {
            Assert.NotEmpty(evidenceList);
            AssertReadOnlyList(evidenceList);
        }
    }

    private static void AssertDetachedSurfaces(ReturnedSurface left, ReturnedSurface right)
    {
        Assert.NotSame(left.Topology, right.Topology);
        Assert.NotSame(left.Topology.Nodes, right.Topology.Nodes);
        Assert.NotSame(left.Topology.Edges, right.Topology.Edges);
        Assert.NotSame(left.Topology.Order, right.Topology.Order);
        Assert.Equal(left.Topology.Nodes.Count, right.Topology.Nodes.Count);
        Assert.Equal(left.Topology.Edges.Count, right.Topology.Edges.Count);
        Assert.Equal(left.Topology.Order.Count, right.Topology.Order.Count);
        for (var index = 0; index < left.Topology.Nodes.Count; index++)
        {
            Assert.NotSame(left.Topology.Nodes[index], right.Topology.Nodes[index]);
            Assert.NotSame(
                left.Topology.Nodes[index].Descriptor,
                right.Topology.Nodes[index].Descriptor);
        }

        for (var index = 0; index < left.Topology.Edges.Count; index++)
        {
            Assert.NotSame(left.Topology.Edges[index], right.Topology.Edges[index]);
            Assert.NotSame(
                left.Topology.Edges[index].ToNodeIds,
                right.Topology.Edges[index].ToNodeIds);
            Assert.NotSame(
                left.Topology.Edges[index].ExcludedLeakageSignals,
                right.Topology.Edges[index].ExcludedLeakageSignals);
        }

        for (var index = 0; index < left.Topology.Order.Count; index++)
            Assert.NotSame(left.Topology.Order[index], right.Topology.Order[index]);

        Assert.NotSame(left.BudgetProof, right.BudgetProof);
        Assert.NotSame(
            left.BudgetProof.ExceededDimensions,
            right.BudgetProof.ExceededDimensions);
        Assert.NotSame(left.MutationEvidence, right.MutationEvidence);
        var leftEvidence = EvidenceLists(left.MutationEvidence);
        var rightEvidence = EvidenceLists(right.MutationEvidence);
        for (var index = 0; index < leftEvidence.Length; index++)
            Assert.NotSame(leftEvidence[index], rightEvidence[index]);
    }

    private static void AssertReadOnlyList<T>(IReadOnlyList<T> values)
    {
        var list = Assert.IsAssignableFrom<IList<T>>(values);
        Assert.True(list.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => list.Add(default!));
        if (values.Count > 0)
            Assert.Throws<NotSupportedException>(() => list[0] = values[0]);
    }

    private static string SnapshotFingerprint(DynamicGraphTopologyRuntimeSnapshot snapshot)
        => string.Join(
            "\n",
            snapshot.Revision,
            snapshot.SignalId,
            snapshot.SourceKind,
            snapshot.SourceRef,
            snapshot.TraceRef,
            TopologyFingerprint(snapshot.Topology),
            ProofFingerprint(snapshot.BudgetProof),
            EvidenceFingerprint(snapshot.MutationEvidence));

    private static string TopologyFingerprint(DynamicGraphTopology topology)
        => string.Join(
            "|",
            topology.TenantId,
            topology.DomainTag,
            topology.ContextId,
            string.Join(",", topology.Nodes.Select(node =>
                $"{node.NodeId}:{node.Descriptor.DomainTag}:{node.Descriptor.TInputTypeName}:{node.Descriptor.TOutputTypeName}")),
            string.Join(",", topology.Edges.Select(edge =>
                $"{edge.EdgeId}:{edge.FromNodeId}:{string.Join("+", edge.ToNodeIds)}:{string.Join("+", edge.ExcludedLeakageSignals)}")),
            string.Join(",", topology.Order.Select(entry =>
                $"{entry.OrderId}:{entry.NodeId}:{entry.TenantId}:{entry.DomainTag}:{entry.ContextId}:{entry.Position}")));

    private static string ProofFingerprint(TopologyBudgetProof proof)
        => string.Join(
            "|",
            proof.MaxNodes,
            proof.ActualNodes,
            proof.MaxEdges,
            proof.ActualEdges,
            proof.MaxDepth,
            proof.ActualDepth,
            proof.MaxParallelBranches,
            proof.ActualParallelBranches,
            proof.WithinBudget,
            string.Join(",", proof.ExceededDimensions));

    private static string EvidenceFingerprint(TopologyMutationEvidence evidence)
        => string.Join(
            "|",
            evidence.SignalId,
            evidence.SourceKind,
            evidence.SourceRef,
            evidence.TraceRef,
            string.Join(",", evidence.ProposedAddedNodeIds),
            string.Join(",", evidence.ProposedRemovedNodeIds),
            string.Join(",", evidence.ProposedUpdatedNodeIds),
            string.Join(",", evidence.ProposedAddedEdgeIds),
            string.Join(",", evidence.ProposedRemovedEdgeIds),
            string.Join(",", evidence.ProposedUpdatedEdgeIds),
            string.Join(",", evidence.ProposedReorderedNodeIds),
            evidence.MutationApplied);

    private static T AssertSuccess<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return Assert.IsAssignableFrom<T>(result.Value);
    }

    private static void AssertFailure<T>(OperationResult<T> result, string code)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(code, result.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    private static void AssertRecordShape<T>((string Name, Type Type)[] expected)
    {
        var constructor = Assert.Single(typeof(T).GetConstructors());
        Assert.Equal(
            expected,
            constructor.GetParameters().Select(parameter => (parameter.Name!, parameter.ParameterType)));
        Assert.Equal(
            expected.OrderBy(item => item.Name, StringComparer.Ordinal),
            typeof(T).GetProperties()
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .Select(property => (property.Name, property.PropertyType)));
    }

    private sealed record ReturnedSurface(
        string Name,
        DynamicGraphTopology Topology,
        TopologyBudgetProof BudgetProof,
        TopologyMutationEvidence MutationEvidence);

    private sealed class DelegateBuilder(
        Func<DynamicGraphBuildSignal, TopologyBuildBudget, OperationResult<BuildDynamicTopologyResult>> handler)
        : IDynamicGraphTopologyBuilder
    {
        public OperationResult<BuildDynamicTopologyResult> BuildDynamicTopology(
            DynamicGraphBuildSignal signal,
            TopologyBuildBudget budget)
            => handler(signal, budget);
    }

    private sealed class TrackingBuilder(IDynamicGraphTopologyBuilder inner) : IDynamicGraphTopologyBuilder
    {
        private int _callCount;

        public int CallCount => Volatile.Read(ref _callCount);

        public OperationResult<BuildDynamicTopologyResult> BuildDynamicTopology(
            DynamicGraphBuildSignal signal,
            TopologyBuildBudget budget)
        {
            Interlocked.Increment(ref _callCount);
            return inner.BuildDynamicTopology(signal, budget);
        }
    }

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
}
