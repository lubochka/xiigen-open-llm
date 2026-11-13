// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using DPO;
using DPO.Models;
using TensorCore.Core;

public sealed class DesignConversionDpoOptimizerSideContractTests
{
    private const string Chosen =
        "{\"html\":\"<main>chosen</main>\",\"css\":\"main { color: #123456; }\"}";
    private const string Rejected =
        "{\"html\":\"<main>rejected</main>\",\"css\":\"main { color: #654321; }\"}";

    [Fact]
    public void ExactCanonicalPair_PassesThroughProductionAuthority()
    {
        var result = ProjectFinal(Chosen, Rejected);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(result.Value!.Content.Rows);
    }

    [Fact]
    public void PropertyOrderIsSemanticButPropertyCaseIsExact()
    {
        var reversedOrder =
            "{\"css\":\"main { color: red; }\",\"html\":\"<main>chosen</main>\"}";
        Assert.True(ProjectFinal(reversedOrder, Rejected).IsSuccess);

        AssertSideContractFailure(ProjectFinal(
            "{\"Html\":\"<main>chosen</main>\",\"css\":\"main{}\"}",
            Rejected));
    }

    [Theory]
    [InlineData("accepted")]
    [InlineData("Score")]
    [InlineData("Passed")]
    [InlineData("verdict")]
    [InlineData("candidate_id")]
    [InlineData("output_id")]
    public void ExtraOutcomeOrIdentityProperty_FailsClosed(string property)
    {
        var chosen = string.Concat(
            "{\"html\":\"<main>chosen</main>\",\"css\":\"main{}\",\"",
            property,
            "\":true}");

        AssertSideContractFailure(ProjectFinal(chosen, Rejected));
    }

    [Theory]
    [InlineData("{\"html\":\"<main>x</main>\",\"html\":\"<main>y</main>\",\"css\":\"main{}\"}")]
    [InlineData("{\"html\":\"<main>x</main>\"}")]
    [InlineData("{\"html\":1,\"css\":\"main{}\"}")]
    [InlineData("{\"html\":\"<main>x</main>\",\"css\":{\"accepted\":true}}")]
    public void DuplicateMissingWrongTypeAndNestedLabelProperties_FailClosed(
        string chosen)
        => AssertSideContractFailure(ProjectFinal(chosen, Rejected));

    [Fact]
    public void SideWithoutAnyNonWhitespaceContent_FailsClosed()
        => AssertSideContractFailure(ProjectFinal(
            "{\"html\":\"  \",\"css\":\"\\r\\n\"}",
            Rejected));

    [Theory]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("sha256:bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb")]
    [InlineData("candidate-0123456789abcdef")]
    public void HashOnlySide_FailsClosed(string surrogate)
    {
        var chosen = string.Concat(
            "{\"html\":\"",
            surrogate,
            "\",\"css\":\"\"}");

        AssertSideContractFailure(ProjectFinal(chosen, Rejected));
    }

    [Fact]
    public void LegitimateLabelTextAndHexColors_Pass()
    {
        const string chosen =
            "{\"html\":\"<main data-state=\\\"accepted\\\">accepted</main>\",\"css\":\"main { color: #abcdef; background: #123456; }\"}";

        Assert.True(ProjectFinal(chosen, Rejected).IsSuccess);
    }

    [Fact]
    public void NewlineNormalizedIdentityPair_FailsClosed()
    {
        const string chosen =
            "{\"html\":\"<main>same</main>\\r\\n\",\"css\":\"main{}\\r\"}";
        const string rejected =
            "{\"html\":\"<main>same</main>\\n\",\"css\":\"main{}\\n\"}";

        AssertSideContractFailure(ProjectFinal(chosen, rejected));
    }

    [Theory]
    [InlineData("pipeline")]
    [InlineData("persisted")]
    [InlineData("bridge")]
    public void CompositeMaliciousStage_FailsBeforeFraming(string stage)
    {
        var fixture = CompositeFixture.Create();
        const string malicious =
            "{\"html\":\"<main>x</main>\",\"css\":\"main{}\",\"accepted\":true}";
        var item = stage == "pipeline"
            ? fixture.Item with { ChosenResponse = malicious }
            : fixture.Item;
        var pair = stage == "persisted"
            ? fixture.Pair with { Chosen = malicious }
            : fixture.Pair;
        if (stage == "persisted")
        {
            var rebound = DpoPairTrainingContentAuthority.Bind(pair);
            Assert.True(rebound.IsSuccess, rebound.ErrorMessage);
            pair = rebound.Value!;
        }
        var bridge = stage == "bridge"
            ? fixture.Bridge with { Chosen = malicious }
            : fixture.Bridge;

        var result = DesignConversionDpoTrainingContentAuthority
            .ProjectCompositeOptimizerInputs(
                [item],
                [fixture.Provenance],
                [pair],
                [bridge]);

        AssertSideContractFailure(result);
    }

    private static OperationResult<
        DesignConversionFinalOptimizerBatch<LearningTrainingItem>> ProjectFinal(
            string chosen,
            string rejected)
    {
        var item = new LearningTrainingItem(
            "item-1",
            "prompt",
            chosen,
            rejected);
        return DesignConversionDpoTrainingContentAuthority
            .ProjectFinalOptimizerInputs(
                [item],
                [TrainingTeacherProvenance.Deterministic("request-1")],
                static input => input.Prompt,
                static input => input.ChosenResponse,
                static input => input.RejectedResponse,
                static _ => Array.Empty<float>(),
                static _ => null,
                static _ => "side-contract-test");
    }

    private static void AssertSideContractFailure<T>(OperationResult<T> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionDpoOptimizerSideContract.InvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(
            DesignConversionDpoOptimizerSideContract.InvalidMessage,
            result.ErrorMessage);
        Assert.DoesNotContain("<main", result.ErrorMessage, StringComparison.Ordinal);
    }

    private sealed record CompositeFixture(
        TrainingTeacherProvenance Provenance,
        LearningTrainingItem Item,
        ContinuousLearning.DpoPair Pair,
        DesignConversionDpoEvaluatedPackedBatch Bridge)
    {
        internal static CompositeFixture Create()
        {
            var provenance = TrainingTeacherProvenance.Deterministic(
                "composite-side-contract-request");
            var item = new LearningTrainingItem(
                "item-1",
                "prompt",
                Chosen,
                Rejected);
            var unboundPair = new ContinuousLearning.DpoPair(
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
                    ["sha256:" + new string('c', 64)])
            };
            var bound = DpoPairTrainingContentAuthority.Bind(unboundPair);
            Assert.True(bound.IsSuccess, bound.ErrorMessage);
            var vectorBatch = new DpoVectorBatch(
                new DpoBatch
                {
                    ChosenIds = new Tensor(
                        [0f, 0f],
                        new TensorShape(1, 2)),
                    RejectedIds = new Tensor(
                        [0f, 0f],
                        new TensorShape(1, 2)),
                    ChosenMask = new Tensor(
                        [1f, 1f],
                        new TensorShape(1, 2)),
                    RejectedMask = new Tensor(
                        [1f, 1f],
                        new TensorShape(1, 2))
                },
                [1.25f],
                0.75f);
            var bridge = new DesignConversionDpoEvaluatedPackedBatch(
                vectorBatch,
                new string('a', 64),
                new string('b', 64),
                "bridge-prompt",
                Chosen,
                Rejected,
                provenance);
            return new CompositeFixture(
                provenance,
                item,
                bound.Value!,
                bridge);
        }
    }
}
