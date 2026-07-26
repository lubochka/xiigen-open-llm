// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

public static class ExtensionQualityScoring
{
    public const string SchemaVersion = "xiigen.extension_quality_score.v1";
    public const int PassingLayerScoreMin = 4;
    public const int NeedsRepairLayerScoreMin = 3;

    public static readonly IReadOnlySet<string> RequiredLayerIds = new HashSet<string>(StringComparer.Ordinal)
    {
        "prompt_to_task",
        "orchestration_reasoning",
        "skill_selection",
        "final_output_usefulness"
    };

    public static ExtensionQualityScoreResult Score(ExtensionQualityScoreInput input)
    {
        var reasons = new List<string>();
        var layerScores = input.LayerScores.ToArray();
        var layersById = layerScores
            .GroupBy(layer => layer.LayerId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        foreach (var requiredLayerId in RequiredLayerIds)
        {
            if (!layersById.ContainsKey(requiredLayerId))
            {
                reasons.Add($"missing_required_layer:{requiredLayerId}");
            }
        }

        foreach (var duplicate in layersById.Where(pair => pair.Value.Length > 1).Select(pair => pair.Key))
        {
            reasons.Add($"duplicate_layer:{duplicate}");
        }

        foreach (var layer in layerScores)
        {
            if (layer.Score is < 0 or > 5)
            {
                reasons.Add($"layer_score_out_of_range:{layer.LayerId}");
            }

            if (string.IsNullOrWhiteSpace(layer.HumanReviewerNote))
            {
                reasons.Add($"missing_human_reviewer_note:{layer.LayerId}");
            }

            if (layer.Score <= 2 && layer.FailureCategories.Count == 0)
            {
                reasons.Add($"low_score_requires_failure_category:{layer.LayerId}");
            }
        }

        if (input.ForbiddenFailures.Count > 0)
        {
            reasons.AddRange(input.ForbiddenFailures.Select(failure => $"forbidden_failure:{failure}"));
        }

        if (!input.PrivacyAndRedactionPassed)
        {
            reasons.Add("privacy_or_redaction_failed");
        }

        if (input.TokenComparison is not null)
        {
            var tokenReduction = input.TokenComparison.BaselineTokenTotal - input.TokenComparison.AdaptiveTokenTotal;
            if (tokenReduction > 0 && layerScores.Length == 0)
            {
                reasons.Add("token_reduction_without_quality_scores_cannot_pass");
            }

            if (input.TokenComparison.QualityRegressed)
            {
                reasons.Add("token_reduction_cannot_pass_when_quality_regresses");
            }
        }

        var validLayerScores = layerScores
            .Where(layer => layer.Score is >= 0 and <= 5)
            .ToArray();
        var averageScore = validLayerScores.Length == 0
            ? 0
            : validLayerScores.Average(layer => layer.Score);

        var hasCompleteLayerSet =
            RequiredLayerIds.All(id => layersById.TryGetValue(id, out var layers) && layers.Length == 1);
        var hasFailingLayer = validLayerScores.Any(layer => layer.Score <= 2);
        var hasNeedsRepairLayer = validLayerScores.Any(layer => layer.Score == NeedsRepairLayerScoreMin);
        var hasPassingLayerSet =
            hasCompleteLayerSet && validLayerScores.Length == RequiredLayerIds.Count &&
            validLayerScores.All(layer => layer.Score >= PassingLayerScoreMin);

        var verdict = ExtensionQualityVerdict.Fail;
        if (reasons.Count == 0 && hasPassingLayerSet)
        {
            verdict = ExtensionQualityVerdict.Pass;
        }
        else if (reasons.Count == 0 &&
                 !hasFailingLayer &&
                 input.ForbiddenFailures.Count == 0 &&
                 input.PrivacyAndRedactionPassed &&
                 hasCompleteLayerSet &&
                 validLayerScores.Length == RequiredLayerIds.Count &&
                 validLayerScores.All(layer => layer.Score >= NeedsRepairLayerScoreMin) &&
                 hasNeedsRepairLayer)
        {
            verdict = ExtensionQualityVerdict.NeedsRepair;
        }

        return new ExtensionQualityScoreResult(
            SchemaVersion: SchemaVersion,
            ScenarioId: input.ScenarioId,
            PairId: input.PairId,
            Verdict: verdict,
            LayerScores: layerScores,
            AverageScore: Math.Round(averageScore, 2),
            Reasons: reasons,
            ForbiddenFailures: input.ForbiddenFailures,
            HumanReviewerNote: BuildHumanReviewerNote(verdict, layerScores, reasons),
            QualityResultClaimed: false,
            SemanticQualityClaimed: false,
            LearnabilityClaimed: false,
            TokenSavingsClaimed: false);
    }

    private static string BuildHumanReviewerNote(
        ExtensionQualityVerdict verdict,
        IReadOnlyList<ExtensionQualityLayerScore> layerScores,
        IReadOnlyList<string> reasons)
    {
        if (verdict == ExtensionQualityVerdict.Pass)
        {
            return "Rubric gate passed for this scored evidence packet only; it does not prove semantic quality, learnability, training, or checkpoint readiness.";
        }

        if (verdict == ExtensionQualityVerdict.NeedsRepair)
        {
            var repairLayers = string.Join(", ", layerScores.Where(layer => layer.Score == NeedsRepairLayerScoreMin).Select(layer => layer.LayerId));
            return $"Needs repair before pass. Repair layers: {repairLayers}. This is not a semantic-quality claim.";
        }

        var reasonSummary = reasons.Count == 0 ? "layer score failed" : string.Join("; ", reasons.Take(5));
        return $"Failed quality gate: {reasonSummary}. Token reduction or runtime evidence alone cannot override this result.";
    }
}

public sealed record ExtensionQualityScoreInput(
    string ScenarioId,
    string PairId,
    IReadOnlyList<ExtensionQualityLayerScore> LayerScores,
    IReadOnlyList<string> ForbiddenFailures,
    bool PrivacyAndRedactionPassed,
    ExtensionQualityTokenComparison? TokenComparison);

public sealed record ExtensionQualityLayerScore(
    string LayerId,
    string HumanName,
    int Score,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> FailureCategories,
    string HumanReviewerNote);

public sealed record ExtensionQualityTokenComparison(
    int BaselineTokenTotal,
    int AdaptiveTokenTotal,
    bool QualityRegressed);

public sealed record ExtensionQualityScoreResult(
    string SchemaVersion,
    string ScenarioId,
    string PairId,
    ExtensionQualityVerdict Verdict,
    IReadOnlyList<ExtensionQualityLayerScore> LayerScores,
    double AverageScore,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<string> ForbiddenFailures,
    string HumanReviewerNote,
    bool QualityResultClaimed,
    bool SemanticQualityClaimed,
    bool LearnabilityClaimed,
    bool TokenSavingsClaimed);

public enum ExtensionQualityVerdict
{
    Pass,
    NeedsRepair,
    Fail
}
