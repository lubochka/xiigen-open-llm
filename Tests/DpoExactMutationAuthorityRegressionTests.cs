// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Collections;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using DPO;
using DPO.Models;
using ProjectWikiDomain;
using TensorCore.Core;

public sealed class DpoExactMutationAuthorityRegressionTests
{
    [Fact]
    public async Task ProjectWikiMetricSnapshot_UsesOnlyFiftyBoundedIndexReads()
    {
        var row = new ProjectWikiDpoMetricPair(
            string.Empty,
            "prompt",
            "chosen",
            "[WRONG: invalid] rejected");
        var callerList = new ThrowingEnumeratorFiftyRowList(row);
        var runner = new ProjectWikiDpoMetricRunner(
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
            epochCount: 1,
            vocabSize: 256,
            contextLen: 16,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);

        var result = await runner.TrainAndEvaluateAsync(
            "bounded-snapshot-unit",
            "bounded-snapshot-batch",
            callerList,
            callerList,
            Path.GetTempPath());

        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_DPO_METRIC_ROW", result.ErrorCode);
        Assert.Equal(100, callerList.IndexReadCount);
        Assert.Equal(0, callerList.EnumeratorCallCount);
    }

    [Fact]
    public void CompositeOptimizerAuthority_HoldsExactStageObjectsAndRejectsRequestMismatch()
    {
        var provenance = TrainingTeacherProvenance.Deterministic(
            "composite-request-1");
        const string chosen =
            "{\"html\":\"<main>chosen</main>\",\"css\":\"main { color: #123456; }\"}";
        const string rejected =
            "{\"html\":\"<main>rejected</main>\",\"css\":\"main { color: #654321; }\"}";
        var item = new LearningTrainingItem(
            "item-1",
            "prompt",
            chosen,
            rejected);
        var pair = new ContinuousLearning.DpoPair(
            item.Prompt,
            item.ChosenResponse,
            item.RejectedResponse,
            DesignConversionMachine.DomainTag,
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = provenance,
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                ["sha256:" + new string('c', 64),
                 "sha256:" + new string('d', 64)])
        };
        var boundPair = DpoPairTrainingContentAuthority.Bind(pair);
        Assert.True(boundPair.IsSuccess, boundPair.ErrorMessage);

        var vectorBatch = new DpoVectorBatch(
            new DpoBatch
            {
                ChosenIds = new Tensor([0f, 0f], new TensorShape(1, 2)),
                RejectedIds = new Tensor([0f, 0f], new TensorShape(1, 2)),
                ChosenMask = new Tensor([1f, 1f], new TensorShape(1, 2)),
                RejectedMask = new Tensor([1f, 1f], new TensorShape(1, 2))
            },
            [1.25f, -0.0f],
            0.75f);
        var packed = new DesignConversionDpoEvaluatedPackedBatch(
            vectorBatch,
            new string('a', 64),
            new string('b', 64),
            "bridge-prompt",
            chosen,
            rejected,
            provenance);

        var projected = DesignConversionDpoTrainingContentAuthority
            .ProjectCompositeOptimizerInputs(
                [item],
                [provenance],
                [boundPair.Value!],
                [packed]);

        Assert.True(projected.IsSuccess, projected.ErrorMessage);
        var held = Assert.Single(projected.Value!.Inputs);
        Assert.Same(item, held.PipelineInput);
        Assert.Same(boundPair.Value, held.PersistedPair);
        Assert.Same(packed, held.BridgeInput);
        Assert.Same(vectorBatch, held.BridgeInput!.VectorBatch);

        var reversedGovernancePair = pair with
        {
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                ["sha256:" + new string('d', 64),
                 "sha256:" + new string('c', 64)])
        };
        var reboundGovernancePair = DpoPairTrainingContentAuthority.Bind(
            reversedGovernancePair);
        Assert.True(
            reboundGovernancePair.IsSuccess,
            reboundGovernancePair.ErrorMessage);
        var governanceChanged = DesignConversionDpoTrainingContentAuthority
            .ProjectCompositeOptimizerInputs(
                [item],
                [provenance],
                [reboundGovernancePair.Value!],
                [packed]);
        Assert.True(governanceChanged.IsSuccess, governanceChanged.ErrorMessage);
        Assert.NotEqual(
            projected.Value.Content.Rows[0].Sha256,
            governanceChanged.Value!.Content.Rows[0].Sha256);

        var mismatched = DesignConversionDpoTrainingContentAuthority
            .ProjectCompositeOptimizerInputs(
                [item],
                [provenance with { RequestId = "different-request" }],
                [boundPair.Value!],
                [packed]);
        Assert.False(mismatched.IsSuccess);
    }

    private sealed class ThrowingEnumeratorFiftyRowList(
        ProjectWikiDpoMetricPair row)
        : IReadOnlyList<ProjectWikiDpoMetricPair>
    {
        internal int IndexReadCount { get; private set; }
        internal int EnumeratorCallCount { get; private set; }
        public int Count => 50;

        public ProjectWikiDpoMetricPair this[int index]
        {
            get
            {
                if (index is < 0 or >= 50)
                    throw new ArgumentOutOfRangeException(nameof(index));
                IndexReadCount++;
                return row;
            }
        }

        public IEnumerator<ProjectWikiDpoMetricPair> GetEnumerator()
        {
            EnumeratorCallCount++;
            throw new InvalidOperationException(
                "Caller enumeration must never be used for the bounded snapshot.");
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
