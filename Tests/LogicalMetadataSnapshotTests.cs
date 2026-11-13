// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using SemanticCompilerAdapter;
using Domain = SemanticCompiler29.Domain;

// These tests call the production snapshot boundary directly and pin ownership and deterministic order.
public sealed class LogicalMetadataSnapshotTests
{
    [Fact]
    public void TryCreateCopies_NestedMetadata_OrdersKeysAndOwnsEveryMutableLevelIndependently()
    {
        var tags = new[] { "source-tag", "stable-tag" };
        var numbers = new[] { 3, 5, 8 };
        var labels = new Dictionary<string, string>
        {
            ["z-label"] = "last",
            ["a-label"] = "first"
        };
        var nested = new Dictionary<string, object>
        {
            ["z-numbers"] = numbers,
            ["a-labels"] = labels
        };
        var source = new Dictionary<string, object>
        {
            ["z-tags"] = tags,
            ["a-nested"] = nested,
            ["m-enabled"] = true
        };

        var created = LogicalMetadataSnapshot.TryCreateCopies(source, 2, out var snapshots);

        Assert.True(created);
        Assert.Equal(2, snapshots.Length);
        var first = snapshots[0];
        var second = snapshots[1];
        Assert.NotSame(source, first);
        Assert.NotSame(first, second);
        Assert.Equal(["a-nested", "m-enabled", "z-tags"], first.Keys.ToArray());

        var firstNested = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(first["a-nested"]);
        var secondNested = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(second["a-nested"]);
        Assert.NotSame(nested, firstNested);
        Assert.NotSame(firstNested, secondNested);
        Assert.Equal(["a-labels", "z-numbers"], firstNested.Keys.ToArray());

        var firstLabels = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(firstNested["a-labels"]);
        var secondLabels = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(secondNested["a-labels"]);
        var firstNumbers = Assert.IsType<int[]>(firstNested["z-numbers"]);
        var secondNumbers = Assert.IsType<int[]>(secondNested["z-numbers"]);
        var firstTags = Assert.IsType<string[]>(first["z-tags"]);
        var secondTags = Assert.IsType<string[]>(second["z-tags"]);
        Assert.Equal(["a-label", "z-label"], firstLabels.Keys.ToArray());
        Assert.NotSame(labels, firstLabels);
        Assert.NotSame(firstLabels, secondLabels);
        Assert.NotSame(numbers, firstNumbers);
        Assert.NotSame(firstNumbers, secondNumbers);
        Assert.NotSame(tags, firstTags);
        Assert.NotSame(firstTags, secondTags);

        source["new-source-key"] = "late";
        nested["new-nested-key"] = "late";
        labels["a-label"] = "mutated";
        numbers[0] = 99;
        tags[0] = "mutated";

        Assert.False(first.ContainsKey("new-source-key"));
        Assert.False(firstNested.ContainsKey("new-nested-key"));
        Assert.Equal("first", firstLabels["a-label"]);
        Assert.Equal(3, firstNumbers[0]);
        Assert.Equal("source-tag", firstTags[0]);

        firstNumbers[0] = 77;
        firstTags[0] = "first-copy-only";
        Assert.Equal(3, secondNumbers[0]);
        Assert.Equal("source-tag", secondTags[0]);

        var writableTopView = Assert.IsAssignableFrom<IDictionary<string, object>>(first);
        var writableNestedView = Assert.IsAssignableFrom<IDictionary<string, object>>(firstNested);
        var writableLabelsView = Assert.IsAssignableFrom<IDictionary<string, string>>(firstLabels);
        Assert.Throws<NotSupportedException>(() => writableTopView.Add("forbidden", "value"));
        Assert.Throws<NotSupportedException>(() => writableNestedView.Add("forbidden", "value"));
        Assert.Throws<NotSupportedException>(() => writableLabelsView.Add("forbidden", "value"));
    }

    [Fact]
    public void TryCreateCopies_ContextSignals_UsesCanonicalDomainOrderAndOwnsEveryCopy()
    {
        var priorDomains = new[] { Domain.REFACTOR, Domain.CODE_Q, Domain.ARCH };
        var completed = new[] { "4.4", "3.4" };
        var activeEdges = new[] { "knowledge:4.4", "knowledge:3.4" };
        var confidence = new Dictionary<Domain, double>
        {
            [Domain.REFACTOR] = 0.41d,
            [Domain.ARCH] = 0.83d,
            [Domain.CODE_Q] = 0.67d
        };
        var context = new ContextSignals(priorDomains, completed, activeEdges, confidence);
        var source = new Dictionary<string, object> { ["context"] = context };

        var created = LogicalMetadataSnapshot.TryCreateCopies(source, 2, out var snapshots);

        Assert.True(created);
        var first = Assert.IsType<ContextSignals>(snapshots[0]["context"]);
        var second = Assert.IsType<ContextSignals>(snapshots[1]["context"]);
        Assert.NotSame(context, first);
        Assert.NotSame(first, second);
        Assert.Equal(
            [Domain.CODE_Q, Domain.ARCH, Domain.REFACTOR],
            first.PriorConfidenceMap.Keys.ToArray());
        Assert.Equal([Domain.REFACTOR, Domain.CODE_Q, Domain.ARCH], first.PriorDomains);
        Assert.Equal(["4.4", "3.4"], first.CompletedSubPhases);
        Assert.Equal(["knowledge:4.4", "knowledge:3.4"], first.ActiveKnowledgeEdges);
        Assert.NotSame(priorDomains, first.PriorDomains);
        Assert.NotSame(first.PriorDomains, second.PriorDomains);
        Assert.NotSame(completed, first.CompletedSubPhases);
        Assert.NotSame(first.CompletedSubPhases, second.CompletedSubPhases);
        Assert.NotSame(activeEdges, first.ActiveKnowledgeEdges);
        Assert.NotSame(first.ActiveKnowledgeEdges, second.ActiveKnowledgeEdges);
        Assert.NotSame(confidence, first.PriorConfidenceMap);
        Assert.NotSame(first.PriorConfidenceMap, second.PriorConfidenceMap);

        priorDomains[0] = Domain.BUG;
        completed[0] = "mutated";
        activeEdges[0] = "mutated";
        confidence[Domain.CODE_Q] = 0.01d;
        Assert.Equal(Domain.REFACTOR, first.PriorDomains[0]);
        Assert.Equal("4.4", first.CompletedSubPhases[0]);
        Assert.Equal("knowledge:4.4", first.ActiveKnowledgeEdges[0]);
        Assert.Equal(0.67d, first.PriorConfidenceMap[Domain.CODE_Q]);

        first.PriorDomains[0] = Domain.PLAN;
        first.CompletedSubPhases[0] = "first-copy-only";
        first.ActiveKnowledgeEdges[0] = "first-copy-only";
        Assert.Equal(Domain.REFACTOR, second.PriorDomains[0]);
        Assert.Equal("4.4", second.CompletedSubPhases[0]);
        Assert.Equal("knowledge:4.4", second.ActiveKnowledgeEdges[0]);

        var writableConfidenceView = Assert.IsAssignableFrom<IDictionary<Domain, double>>(
            first.PriorConfidenceMap);
        Assert.Throws<NotSupportedException>(() => writableConfidenceView.Add(Domain.UNKNOWN, 0.1d));
    }

    [Fact]
    public void TryCreateCopies_ResultContextAndTrainingCarriers_AreDeepOwnedWithoutReorderingSequences()
    {
        var flowMetrics = new Dictionary<string, string>
        {
            ["z-metric"] = "last",
            ["a-metric"] = "first"
        };
        var flowEvidence = new[] { "flow-evidence-1", "flow-evidence-2" };
        var flowWarnings = new[] { "flow-warning" };
        var transitionEvidence = new[] { "transition-evidence" };
        var flow = new LogicalFlowResult(
            "flow-1",
            "scenario-1",
            "PASS",
            "human-result",
            new LogicalFlowSummary(
                "summary-1",
                3,
                0.4d,
                0.8d,
                "quality",
                LowerIsBetter: false,
                Passed: true,
                ShouldScaleUp: false,
                "continue",
                0.7d,
                "stable",
                "notes",
                10d,
                8d),
            flowMetrics,
            flowEvidence,
            flowWarnings)
        {
            TransitionSignals =
            [
                new PhaseTransitionSignal(
                    Domain.BUG,
                    "transition-prompt",
                    "node-1",
                    "4.4",
                    transitionEvidence)
            ]
        };
        var flowResults = new[] { flow };

        var fragmentMetrics = new Dictionary<string, string>
        {
            ["z-fragment"] = "last",
            ["a-fragment"] = "first"
        };
        var fragmentEvidence = new[] { "fragment-evidence" };
        var fragmentWarnings = new[] { "fragment-warning" };
        var fragment = new LogicalContextFragment(
            "upstream-flow",
            "upstream-scenario",
            "PASS",
            "upstream-human",
            "4",
            "4.4",
            fragmentMetrics,
            fragmentEvidence,
            fragmentWarnings);
        var sourceFlowIds = new[] { "upstream-flow" };
        var sourceScenarioIds = new[] { "upstream-scenario" };
        var fragments = new[] { fragment };
        var packet = new LogicalContextPacket(
            "run-1",
            sourceFlowIds,
            sourceScenarioIds,
            fragments,
            new DateTimeOffset(2026, 7, 22, 0, 0, 0, TimeSpan.Zero));

        var registeredFlowIds = new[] { "registered-flow" };
        var registeredSubPhaseIds = new[] { "4.4" };
        var registeredUnitIds = new[] { "registered-unit" };
        var trainingEvidence = new[] { "training-evidence" };
        var timing = CreateTiming("4.4", "registered-unit");
        var branchTimings = new[] { timing };
        var training = new LogicalCapabilityTrainingResult(
            CapabilityRegistered: true,
            registeredFlowIds,
            registeredSubPhaseIds,
            registeredUnitIds,
            TrainingTimeMs: 25d,
            trainingEvidence,
            "training-metadata-only")
        {
            Domain = "software-development",
            BatchedBranchCount = 1,
            BatchedDpoStepCount = 0,
            BranchTimings = branchTimings
        };

        var source = new Dictionary<string, object>
        {
            ["flow-results"] = flowResults,
            ["context-packet"] = packet,
            ["training-result"] = training
        };

        var created = LogicalMetadataSnapshot.TryCreateCopies(source, 1, out var snapshots);

        Assert.True(created);
        var snapshot = Assert.Single(snapshots);
        var stableFlow = Assert.Single(Assert.IsType<LogicalFlowResult[]>(snapshot["flow-results"]));
        var stablePacket = Assert.IsType<LogicalContextPacket>(snapshot["context-packet"]);
        var stableTraining = Assert.IsType<LogicalCapabilityTrainingResult>(snapshot["training-result"]);
        var stableFragment = Assert.Single(stablePacket.Fragments);
        var stableSignal = Assert.Single(stableFlow.TransitionSignals);

        Assert.NotSame(flow, stableFlow);
        Assert.NotSame(flow.Summary, stableFlow.Summary);
        Assert.NotSame(flowMetrics, stableFlow.Metrics);
        Assert.Equal(["a-metric", "z-metric"], stableFlow.Metrics.Keys.ToArray());
        Assert.NotSame(flowEvidence, stableFlow.EvidenceIds);
        Assert.NotSame(flowWarnings, stableFlow.Warnings);
        Assert.NotSame(transitionEvidence, stableSignal.EvidenceIds);
        Assert.NotSame(packet, stablePacket);
        Assert.NotSame(sourceFlowIds, stablePacket.SourceFlowIds);
        Assert.NotSame(sourceScenarioIds, stablePacket.SourceScenarioIds);
        Assert.NotSame(fragments, stablePacket.Fragments);
        Assert.NotSame(fragment, stableFragment);
        Assert.Equal(["a-fragment", "z-fragment"], stableFragment.Metrics.Keys.ToArray());
        Assert.NotSame(fragmentEvidence, stableFragment.EvidenceIds);
        Assert.NotSame(fragmentWarnings, stableFragment.Warnings);
        Assert.NotSame(training, stableTraining);
        Assert.NotSame(registeredFlowIds, stableTraining.RegisteredFlowIds);
        Assert.NotSame(registeredSubPhaseIds, stableTraining.RegisteredSubPhaseIds);
        Assert.NotSame(registeredUnitIds, stableTraining.RegisteredUnitIds);
        Assert.NotSame(trainingEvidence, stableTraining.EvidenceIds);
        Assert.NotSame(branchTimings, stableTraining.BranchTimings);
        Assert.NotSame(timing, Assert.Single(stableTraining.BranchTimings));

        flowMetrics["a-metric"] = "mutated";
        flowEvidence[0] = "mutated";
        flowWarnings[0] = "mutated";
        transitionEvidence[0] = "mutated";
        sourceFlowIds[0] = "mutated";
        sourceScenarioIds[0] = "mutated";
        fragments[0] = fragment with { FlowId = "mutated" };
        fragmentMetrics["a-fragment"] = "mutated";
        fragmentEvidence[0] = "mutated";
        fragmentWarnings[0] = "mutated";
        registeredFlowIds[0] = "mutated";
        registeredSubPhaseIds[0] = "mutated";
        registeredUnitIds[0] = "mutated";
        trainingEvidence[0] = "mutated";
        branchTimings[0] = CreateTiming("mutated", "mutated");

        Assert.Equal("first", stableFlow.Metrics["a-metric"]);
        Assert.Equal("flow-evidence-1", stableFlow.EvidenceIds[0]);
        Assert.Equal("flow-warning", stableFlow.Warnings[0]);
        Assert.Equal("transition-evidence", stableSignal.EvidenceIds[0]);
        Assert.Equal("upstream-flow", stablePacket.SourceFlowIds[0]);
        Assert.Equal("upstream-scenario", stablePacket.SourceScenarioIds[0]);
        Assert.Equal("upstream-flow", stableFragment.FlowId);
        Assert.Equal("first", stableFragment.Metrics["a-fragment"]);
        Assert.Equal("fragment-evidence", stableFragment.EvidenceIds[0]);
        Assert.Equal("fragment-warning", stableFragment.Warnings[0]);
        Assert.Equal("registered-flow", stableTraining.RegisteredFlowIds[0]);
        Assert.Equal("4.4", stableTraining.RegisteredSubPhaseIds[0]);
        Assert.Equal("registered-unit", stableTraining.RegisteredUnitIds[0]);
        Assert.Equal("training-evidence", stableTraining.EvidenceIds[0]);
        Assert.Equal("4.4", stableTraining.BranchTimings[0].SubPhaseId);
        Assert.Equal("registered-unit", stableTraining.BranchTimings[0].UnitId);
    }

    [Fact]
    public void TryCreateCopies_InvalidCountUnsupportedNullAndCyclicGraphs_FailWithoutPartialSnapshots()
    {
        AssertSnapshotFailure(null, count: 1);
        AssertSnapshotFailure(new Dictionary<string, object> { ["value"] = "valid" }, count: 0);
        AssertSnapshotFailure(new Dictionary<string, object> { ["unknown"] = new object() }, count: 1);
        AssertSnapshotFailure(new Dictionary<string, object> { ["null"] = null! }, count: 1);
        AssertSnapshotFailure(new Dictionary<string, object> { ["null-array"] = new string[] { null! } }, count: 1);

        var cycle = new Dictionary<string, object>();
        cycle["self"] = cycle;
        AssertSnapshotFailure(cycle, count: 1);
    }

    [Fact]
    public void TryCreateCopies_DepthBoundary_AcceptsSixteenAndRejectsSeventeenAtomically()
    {
        var accepted = LogicalMetadataSnapshot.TryCreateCopies(
            CreateNestedMetadata(dictionaryCount: 16),
            1,
            out var acceptedSnapshots);

        Assert.True(accepted);
        Assert.Single(acceptedSnapshots);

        var rejected = LogicalMetadataSnapshot.TryCreateCopies(
            CreateNestedMetadata(dictionaryCount: 17),
            1,
            out var rejectedSnapshots);

        Assert.False(rejected);
        Assert.Null(rejectedSnapshots);
    }

    private static LogicalCapabilityTrainingBranchTiming CreateTiming(
        string subPhaseId,
        string unitId)
        => new(
            subPhaseId,
            unitId,
            ParentUnitId: null,
            TrainingStepCount: 1,
            UsedWarmCache: false,
            BuildStackMs: 1d,
            ReferenceModelMs: 2d,
            DpoMs: 0d,
            SaveArtifactsMs: 4d,
            WrapUnitMs: 5d,
            SaveManifestMs: 6d,
            RegisterUnitMs: 7d,
            RegisterFlowMs: 8d,
            TotalMs: 33d);

    private static IReadOnlyDictionary<string, object> CreateNestedMetadata(int dictionaryCount)
    {
        object value = "leaf";
        for (var index = 0; index < dictionaryCount; index++)
            value = new Dictionary<string, object> { [$"level-{index:D2}"] = value };

        return Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(value);
    }

    private static void AssertSnapshotFailure(
        IReadOnlyDictionary<string, object>? source,
        int count)
    {
        var created = LogicalMetadataSnapshot.TryCreateCopies(source, count, out var snapshots);

        Assert.False(created);
        Assert.Null(snapshots);
    }
}
