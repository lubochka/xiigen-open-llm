// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using SmartSkillsDomain;

public sealed class ExtensionQualityScoringTests
{
    [Fact]
    public void Score_ReturnsPassOnlyWhenAllFourLayersPassAndNoForbiddenFailures()
    {
        var result = ExtensionQualityScoring.Score(CreateInput(
            Layer(5, "prompt_to_task"),
            Layer(4, "orchestration_reasoning"),
            Layer(4, "skill_selection"),
            Layer(5, "final_output_usefulness")));

        Assert.Equal(ExtensionQualityVerdict.Pass, result.Verdict);
        Assert.Empty(result.Reasons);
        Assert.False(result.QualityResultClaimed);
        Assert.False(result.SemanticQualityClaimed);
        Assert.False(result.LearnabilityClaimed);
        Assert.False(result.TokenSavingsClaimed);
        Assert.Contains("does not prove semantic quality", result.HumanReviewerNote, StringComparison.Ordinal);
    }

    [Fact]
    public void Score_ReturnsNeedsRepairWhenAllLayersArePresentButOneLayerIsThree()
    {
        var result = ExtensionQualityScoring.Score(CreateInput(
            Layer(5, "prompt_to_task"),
            Layer(3, "orchestration_reasoning", failures: ["human_unreadable"]),
            Layer(4, "skill_selection"),
            Layer(5, "final_output_usefulness")));

        Assert.Equal(ExtensionQualityVerdict.NeedsRepair, result.Verdict);
        Assert.Contains("Needs repair before pass", result.HumanReviewerNote, StringComparison.Ordinal);
    }

    [Fact]
    public void Score_FailsWhenAnyLayerScoreIsTooLow()
    {
        var result = ExtensionQualityScoring.Score(CreateInput(
            Layer(5, "prompt_to_task"),
            Layer(2, "orchestration_reasoning", failures: ["approval_boundary"]),
            Layer(4, "skill_selection"),
            Layer(5, "final_output_usefulness")));

        Assert.Equal(ExtensionQualityVerdict.Fail, result.Verdict);
        Assert.Contains("Failed quality gate", result.HumanReviewerNote, StringComparison.Ordinal);
    }

    [Fact]
    public void Score_FailsWhenForbiddenFailureExistsEvenWithHighScores()
    {
        var input = CreateInput(
            Layer(5, "prompt_to_task"),
            Layer(5, "orchestration_reasoning"),
            Layer(5, "skill_selection"),
            Layer(5, "final_output_usefulness")) with
        {
            ForbiddenFailures = ["hidden_deterministic_shortcut"]
        };

        var result = ExtensionQualityScoring.Score(input);

        Assert.Equal(ExtensionQualityVerdict.Fail, result.Verdict);
        Assert.Contains("forbidden_failure:hidden_deterministic_shortcut", result.Reasons);
    }

    [Fact]
    public void Score_FailsWhenAnyRequiredLayerIsMissing()
    {
        var result = ExtensionQualityScoring.Score(CreateInput(
            Layer(5, "prompt_to_task"),
            Layer(5, "orchestration_reasoning"),
            Layer(5, "skill_selection")));

        Assert.Equal(ExtensionQualityVerdict.Fail, result.Verdict);
        Assert.Contains("missing_required_layer:final_output_usefulness", result.Reasons);
    }

    [Fact]
    public void Score_TokenReductionAloneCannotPassQuality()
    {
        var input = new ExtensionQualityScoreInput(
            ScenarioId: "TQ-009-token-before-after",
            PairId: "pair-token-only",
            LayerScores: [],
            ForbiddenFailures: [],
            PrivacyAndRedactionPassed: true,
            TokenComparison: new ExtensionQualityTokenComparison(
                BaselineTokenTotal: 200,
                AdaptiveTokenTotal: 120,
                QualityRegressed: false));

        var result = ExtensionQualityScoring.Score(input);

        Assert.Equal(ExtensionQualityVerdict.Fail, result.Verdict);
        Assert.Contains("token_reduction_without_quality_scores_cannot_pass", result.Reasons);
        Assert.Contains("Token reduction", result.HumanReviewerNote, StringComparison.Ordinal);
    }

    [Fact]
    public void Score_TokenReductionCannotPassWhenQualityRegresses()
    {
        var input = CreateInput(
            Layer(5, "prompt_to_task"),
            Layer(5, "orchestration_reasoning"),
            Layer(5, "skill_selection"),
            Layer(5, "final_output_usefulness")) with
        {
            TokenComparison = new ExtensionQualityTokenComparison(
                BaselineTokenTotal: 200,
                AdaptiveTokenTotal: 100,
                QualityRegressed: true)
        };

        var result = ExtensionQualityScoring.Score(input);

        Assert.Equal(ExtensionQualityVerdict.Fail, result.Verdict);
        Assert.Contains("token_reduction_cannot_pass_when_quality_regresses", result.Reasons);
    }

    [Fact]
    public void Score_RequiresHumanReviewerNoteForEachLayer()
    {
        var result = ExtensionQualityScoring.Score(CreateInput(
            Layer(5, "prompt_to_task") with { HumanReviewerNote = "" },
            Layer(5, "orchestration_reasoning"),
            Layer(5, "skill_selection"),
            Layer(5, "final_output_usefulness")));

        Assert.Equal(ExtensionQualityVerdict.Fail, result.Verdict);
        Assert.Contains("missing_human_reviewer_note:prompt_to_task", result.Reasons);
    }

    [Fact]
    public void Score_StructuralReasonPreventsNeedsRepairVerdict()
    {
        var result = ExtensionQualityScoring.Score(CreateInput(
            Layer(3, "prompt_to_task", failures: ["human_unreadable"]) with { HumanReviewerNote = "" },
            Layer(4, "orchestration_reasoning"),
            Layer(4, "skill_selection"),
            Layer(4, "final_output_usefulness")));

        Assert.Equal(ExtensionQualityVerdict.Fail, result.Verdict);
        Assert.Contains("missing_human_reviewer_note:prompt_to_task", result.Reasons);
    }

    private static ExtensionQualityScoreInput CreateInput(params ExtensionQualityLayerScore[] layers)
        => new(
            ScenarioId: "TQ-001-prompt-to-task-docs",
            PairId: "pair-quality-001",
            LayerScores: layers,
            ForbiddenFailures: [],
            PrivacyAndRedactionPassed: true,
            TokenComparison: null);

    private static ExtensionQualityLayerScore Layer(
        int score,
        string layerId,
        IReadOnlyList<string>? failures = null)
        => new(
            LayerId: layerId,
            HumanName: layerId.Replace('_', ' '),
            Score: score,
            EvidenceRefs: ["artifact://fixture"],
            FailureCategories: failures ?? [],
            HumanReviewerNote: $"Human reviewer note for {layerId}.");
}
