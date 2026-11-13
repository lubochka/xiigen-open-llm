// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text;
using System.Text.Json;
using SmartSkillsDomain;

public sealed class OrchestrationReasoningR17CanonicalStateTests
{
    [Fact]
    public void ReorderingPatternsAndSetValues_ProducesIdenticalCanonicalBytesAndHashes()
    {
        var first = Pattern("b", ["z", "a", "a"], "Cafe\u0301\r\nline");
        var second = Pattern("a", ["two", "one"], "other");

        var left = OrchestrationReasoningCanonicalState.SerializePatterns([first, second]);
        var right = OrchestrationReasoningCanonicalState.SerializePatterns(
        [
            second with { RequiredConstraintSignals = ["one", "two"] },
            first with { RequiredConstraintSignals = ["a", "z"] }
        ]);

        Assert.Equal(left, right);
        Assert.Equal(
            OrchestrationReasoningCanonicalState.HashBytes(left),
            OrchestrationReasoningCanonicalState.HashBytes(right));
        Assert.DoesNotContain('\r', Encoding.UTF8.GetString(left));
        using var document = JsonDocument.Parse(left);
        Assert.Equal(
            "Café\nline",
            document.RootElement[1]
                .GetProperty("required_deliverable_signals")[0]
                .GetString());
        Assert.NotEqual((byte)0xef, left[0]);
        Assert.NotEqual((byte)'\n', left[^1]);
    }

    [Fact]
    public void SemanticMutation_ChangesPatternAndStateHashes()
    {
        var pattern = Pattern("a", ["constraint"], "deliverable");
        var mutated = pattern with { ScenarioRef = "scenario:mutated" };
        var hash = OrchestrationReasoningSourceAdmissionTestFixture.Hash("manifest");
        var originalState = OrchestrationReasoningCanonicalState.CreateDescriptor(
            "state",
            "guide",
            "manifest",
            hash,
            "source",
            [pattern]);
        var mutatedState = OrchestrationReasoningCanonicalState.CreateDescriptor(
            "state",
            "guide",
            "manifest",
            hash,
            "source",
            [mutated]);

        Assert.NotEqual(originalState.LearnedPatternsSha256, mutatedState.LearnedPatternsSha256);
        Assert.NotEqual(originalState.StateSha256, mutatedState.StateSha256);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void NonFiniteOrOutOfRangeConfidence_IsRejected(double confidence)
    {
        var pattern = Pattern("a", ["constraint"], "deliverable") with
        {
            Confidence = confidence
        };

        Assert.Throws<InvalidDataException>(() =>
            OrchestrationReasoningCanonicalState.SerializePatterns([pattern]));
    }

    private static OrchestrationReasoningLearnedPattern Pattern(
        string id,
        IReadOnlyList<string> constraints,
        string deliverable)
        => new(
            id,
            $"scenario:{id}",
            ["universal_reasoning"],
            constraints,
            ["requirement"],
            [deliverable],
            ["evidence"],
            ["phase"],
            ["gate"],
            ["asset"],
            ["host"],
            ["risk"],
            ["next"],
            0.8);
}
