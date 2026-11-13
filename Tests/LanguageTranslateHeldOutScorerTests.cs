// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ContinuousLearning;
using TranslationDomain;

namespace LlmFromScratch.Tests;

public sealed class LanguageTranslateHeldOutScorerTests
{
    private const string EvidenceRef =
        "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    [Fact]
    public void ScoreOne_LocksExactAndCharacterAccuracyAtEqualWeight()
    {
        var exact = LanguageTranslateHeldOutScorer.ScoreOne("abcd", "abcd");
        var oneRuneWrong = LanguageTranslateHeldOutScorer.ScoreOne("abcd", "abce");

        Assert.True(exact.IsSuccess, exact.ErrorMessage);
        Assert.Equal(1D, exact.Value!.CharacterAccuracy);
        Assert.Equal(1D, exact.Value.CandidateScore);
        Assert.True(oneRuneWrong.IsSuccess, oneRuneWrong.ErrorMessage);
        Assert.False(oneRuneWrong.Value!.ExactMatch);
        Assert.Equal(0.75D, oneRuneWrong.Value.CharacterAccuracy, 12);
        Assert.Equal(0.375D, oneRuneWrong.Value.CandidateScore, 12);
    }

    [Fact]
    public void ScoreOne_AppliesNfcLineEndingsOuterTrimAndAsciiWhitespaceRuns()
    {
        var decomposed = "  Cafe\u0301\r\n\tline\v\f  ";

        var score = LanguageTranslateHeldOutScorer.ScoreOne(
            "Caf\u00e9 line",
            decomposed);
        Assert.True(score.IsSuccess, score.ErrorMessage);
        Assert.True(score.Value!.ExactMatch);
    }

    [Fact]
    public void ScoreOne_LocksUnicodeOuterTrimInternalPreservationAndLoneCr()
    {
        var unicodeOuterTrim = LanguageTranslateHeldOutScorer.ScoreOne(
            "value",
            "\u00a0\u2003value\u2003\u00a0");
        var internalUnicodeWhitespace = LanguageTranslateHeldOutScorer.ScoreOne(
            "left\u2003right",
            "left right");
        var loneCr = LanguageTranslateHeldOutScorer.ScoreOne(
            "left\nright",
            "left\rright");

        Assert.True(unicodeOuterTrim.IsSuccess, unicodeOuterTrim.ErrorMessage);
        Assert.True(unicodeOuterTrim.Value!.ExactMatch);
        Assert.True(
            internalUnicodeWhitespace.IsSuccess,
            internalUnicodeWhitespace.ErrorMessage);
        Assert.False(internalUnicodeWhitespace.Value!.ExactMatch);
        Assert.True(loneCr.IsSuccess, loneCr.ErrorMessage);
        Assert.True(loneCr.Value!.ExactMatch);
    }

    [Theory]
    [InlineData("Paris", "paris")]
    [InlineData("hello!", "hello?")]
    [InlineData("\u05e9\u05dc\u05d5\u05dd", "\u0645\u0631\u062d\u0628\u0627")]
    [InlineData("resume", "r\u00e9sum\u00e9")]
    [InlineData("Alice", "Bob")]
    public void ScoreOne_PreservesSemanticCharactersAndCase(string expected, string actual)
    {
        var result = LanguageTranslateHeldOutScorer.ScoreOne(expected, actual);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.ExactMatch);
        Assert.True(result.Value.CandidateScore < 1D);
    }

    [Fact]
    public void ScoreOne_UsesUnicodeScalarsRatherThanUtf16CodeUnits()
    {
        var result = LanguageTranslateHeldOutScorer.ScoreOne("A\U0001f600B", "A\U0001f603B");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2D / 3D, result.Value!.CharacterAccuracy, 12);
        Assert.Equal(1D / 3D, result.Value.CandidateScore, 12);
    }

    [Fact]
    public void ScoreOne_RejectsQuadraticWorkBeyondTheHardLimit()
    {
        var result = LanguageTranslateHeldOutScorer.ScoreOne(
            new string('a', 3_000),
            new string('b', 3_000));

        Assert.False(result.IsSuccess);
        Assert.Equal("HELD_OUT_SCORING_WORK_LIMIT_EXCEEDED", result.ErrorCode);
    }

    [Fact]
    public void ScoreOne_PreCancelledTokenStopsBeforeNormalization()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            LanguageTranslateHeldOutScorer.ScoreOne(
                new string('a', 3_000),
                new string('b', 3_000),
                cancellation.Token));
    }

    [Fact]
    public void EstimateWorkCells_IsBoundedAndCancelledBeforeReservation()
    {
        var oversized = LanguageTranslateHeldOutScorer.EstimateWorkCells(
            new string('a', 16_385),
            "x",
            CancellationToken.None);
        Assert.False(oversized.IsSuccess);
        Assert.Equal("HELD_OUT_SCORING_VALUE_LIMIT_EXCEEDED", oversized.ErrorCode);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() =>
            LanguageTranslateHeldOutScorer.EstimateWorkCells(
                new string('a', 16_384),
                "x",
                cancellation.Token));
    }

    [Theory]
    [InlineData("scorer alias")]
    [InlineData("-scorer")]
    [InlineData("scorer/alias")]
    public void Constructor_RejectsFreeFormScorerAliases(string scorerId)
    {
        Assert.Throws<ArgumentException>(() =>
            new LanguageTranslateHeldOutScorer(scorerId, "v1"));
    }

    [Fact]
    public void ScoreOne_InvalidUtf16FailsAsTypedResult()
    {
        var result = LanguageTranslateHeldOutScorer.ScoreOne("\ud800", "x");

        Assert.False(result.IsSuccess);
        Assert.Equal("HELD_OUT_SCORING_VALUE_INVALID", result.ErrorCode);
    }

    [Fact]
    public void AggregateScore_RejectsWorkAboveTheAggregateBudgetBeforeDp()
    {
        var scorer = new LanguageTranslateHeldOutScorer("scorer-explicit", "v1");
        var rows = Enumerable.Range(0, 6)
            .Select(index => new HeldOutScoringRow(
                $"case-{index}",
                new string('a', 2_500),
                new string('b', 2_500)))
            .ToArray();

        var result = scorer.Score(Request(rows));

        Assert.False(result.IsSuccess);
        Assert.Equal("HELD_OUT_SCORING_WORK_LIMIT_EXCEEDED", result.ErrorCode);
    }

    [Theory]
    [InlineData("", "", 1D, 1D)]
    [InlineData("", "x", 0D, 0D)]
    [InlineData("x", "", 0D, 0D)]
    public void ScoreOne_HandlesEmptyBoundaries(
        string expected,
        string actual,
        double characterAccuracy,
        double candidateScore)
    {
        var result = LanguageTranslateHeldOutScorer.ScoreOne(expected, actual);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(characterAccuracy, result.Value!.CharacterAccuracy);
        Assert.Equal(candidateScore, result.Value.CandidateScore);
    }

    [Fact]
    public void AggregateScore_IsInvariantToRowPermutation()
    {
        var scorer = new LanguageTranslateHeldOutScorer("scorer-explicit", "v1");
        var rows = new[]
        {
            new HeldOutScoringRow("case-1", "abcd", "abcd"),
            new HeldOutScoringRow("case-2", "abcd", "abce"),
        };

        var forward = scorer.Score(Request(rows));
        var reverse = scorer.Score(Request(rows.Reverse().ToArray()));

        Assert.True(forward.IsSuccess, forward.ErrorMessage);
        Assert.True(reverse.IsSuccess, reverse.ErrorMessage);
        Assert.Equal(
            forward.Value!.RuntimeProductAccuracy,
            reverse.Value!.RuntimeProductAccuracy,
            12);
        Assert.Equal(
            LanguageTranslateHeldOutScorer.MetricProfile,
            forward.Value.PerDomainMetricUsed);
    }

    [Fact]
    public void AggregateScore_ManyRowsUseCanonicalCaseOrderForFloatingAccumulation()
    {
        var scorer = new LanguageTranslateHeldOutScorer("scorer-explicit", "v1");
        var rows = Enumerable.Range(0, 1_024)
            .Select(index =>
            {
                var length = index % 37 + 1;
                return new HeldOutScoringRow(
                    $"case-{index:D4}",
                    new string('a', length),
                    index % 19 == 0
                        ? new string('a', length)
                        : new string('a', length - 1) + 'b');
            })
            .ToArray();

        var forward = scorer.Score(Request(rows));
        var reverse = scorer.Score(Request(rows.Reverse().ToArray()));
        var interleaved = scorer.Score(Request(rows
            .OrderBy(static row => row.ExpectedLabel.Length % 7)
            .ThenByDescending(static row => row.CaseId, StringComparer.Ordinal)
            .ToArray()));

        Assert.True(forward.IsSuccess, forward.ErrorMessage);
        Assert.True(reverse.IsSuccess, reverse.ErrorMessage);
        Assert.True(interleaved.IsSuccess, interleaved.ErrorMessage);
        Assert.Equal(
            forward.Value!.RuntimeProductAccuracy,
            reverse.Value!.RuntimeProductAccuracy);
        Assert.Equal(
            forward.Value.RuntimeProductAccuracy,
            interleaved.Value!.RuntimeProductAccuracy);
    }

    [Fact]
    public void AggregateScore_RejectsWrongDomainAndDuplicateCases()
    {
        var scorer = new LanguageTranslateHeldOutScorer("scorer-explicit", "v1");
        var wrongDomain = Request(
            [new HeldOutScoringRow("case-1", "x", "x")]) with
        {
            DomainTag = "another-domain",
        };
        var duplicates = Request(
        [
            new HeldOutScoringRow("case-1", "x", "x"),
            new HeldOutScoringRow("case-1", "x", "x"),
        ]);

        Assert.False(scorer.Score(wrongDomain).IsSuccess);
        Assert.False(scorer.Score(duplicates).IsSuccess);
    }

    [Theory]
    [InlineData("external_gold", false, "held-out")]
    [InlineData("synthetic_self_authored", true, "held-out")]
    [InlineData("synthetic_self_authored", false, "training")]
    public void AggregateScore_RejectsNonSelfAuthoredNodeGoldOrWrongSplit(
        string origin,
        bool fromNodeUnderTest,
        string split)
    {
        var scorer = new LanguageTranslateHeldOutScorer("scorer-explicit", "v1");
        var request = Request([new HeldOutScoringRow("case-1", "x", "x")]) with
        {
            ExpectedLabelSource = new ExpectedLabelSource(
                origin,
                EvidenceRef,
                fromNodeUnderTest,
                split),
        };

        Assert.False(scorer.Score(request).IsSuccess);
    }

    private static HeldOutScoringRequest Request(IReadOnlyList<HeldOutScoringRow> rows)
        => new(
            "evaluation-set-1",
            LanguageTranslateHeldOutScorer.DomainTag,
            new ExpectedLabelSource(
                LanguageTranslateHeldOutScorer.SelfAuthoredOrigin,
                EvidenceRef,
                FromNodeUnderTest: false,
                LanguageTranslateHeldOutScorer.HeldOutSplit),
            rows);
}
