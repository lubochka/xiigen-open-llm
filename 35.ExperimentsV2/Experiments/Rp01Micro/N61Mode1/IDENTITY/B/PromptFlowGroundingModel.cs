// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

public sealed class PromptFlowGroundingModel
{
    private readonly OrchestrationKnnStore _knn = new();
    private readonly Dictionary<string, OrchestrationFlowGroundingWeights> _weights = new(StringComparer.Ordinal);

    public int KnnSize => _knn.Count;

    public int WeightsSize => _weights.Count;

    public void Train(OrchestrationTrainingPair pair)
    {
        if (pair.Mode != PairMode.Mode1LearnedFlowGrounding || string.IsNullOrWhiteSpace(pair.FlowRef))
            return;

        _knn.Add(pair);
        _weights[pair.FlowRef] = BuildWeights(pair);
    }

    public FlowGroundingResult? TryGround(string prompt, HumanIntentContract contract)
    {
        if (_knn.Count == 0 || _weights.Count == 0)
            return null;

        var queryText = BuildQueryText(prompt, contract);
        var tokens = OrchestrationKnnStore.Tokenize(queryText).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var best = _weights
            .Select(pair => new
            {
                FlowRef = pair.Key,
                InputScore = ScoreTokens(tokens, pair.Value.InputFeatureWeights),
                AllowedScopeScore = ScoreTokens(tokens, pair.Value.AllowedScopeFeatures),
                Weights = pair.Value
            })
            .Select(item => new
            {
                item.FlowRef,
                Score = item.InputScore + item.AllowedScopeScore,
                item.InputScore,
                item.AllowedScopeScore,
                item.Weights
            })
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.AllowedScopeScore)
            .ThenBy(item => item.FlowRef, StringComparer.Ordinal)
            .FirstOrDefault();

        if (best is null || best.Score <= 0.0)
            return null;

        var nearest = _knn.FindNearestWithScore(queryText, PairMode.Mode1LearnedFlowGrounding);
        if (nearest is null || nearest.Score <= 0.0)
            return null;

        var confidence = ComputeInputOnlyConfidence(best.Score, nearest.Score);
        if (confidence < 0.50)
            return null;

        return new FlowGroundingResult(
            best.FlowRef,
            confidence,
            BuildRuntimeEvidenceFeatures(best.Weights),
            best.Weights.PhaseWeights.Keys.Order(StringComparer.Ordinal).ToArray(),
            best.Weights.ArtifactWeights.Keys.Order(StringComparer.Ordinal).ToArray(),
            best.Weights.GateWeights.Keys.Order(StringComparer.Ordinal).ToArray());
    }

    public OrchestrationTrainingPair? FindLearnedPair(
        string prompt,
        HumanIntentContract contract,
        FlowGroundingResult grounding)
    {
        var queryText = BuildLearnedPairQueryTextWithoutPredictedFlow(prompt, contract, grounding);
        // N61 / IDENTITY-M1-01: финальный кандидат допускается только из уже выбранного flow.
        return string.IsNullOrWhiteSpace(queryText)
            ? null
            : _knn.FindNearest(queryText, PairMode.Mode1LearnedFlowGrounding, grounding.FlowRef);
    }

    public IReadOnlyList<OrchestrationTrainingPair> ExportPairs()
        => _knn.ExportPairs();

    public IReadOnlyDictionary<string, OrchestrationFlowGroundingWeights> ExportWeights()
        => _weights
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

    public void ImportState(
        IEnumerable<OrchestrationTrainingPair> pairs,
        IReadOnlyDictionary<string, OrchestrationFlowGroundingWeights> weights)
    {
        _knn.ImportPairs(pairs);
        _weights.Clear();
        foreach (var pair in weights)
            _weights[pair.Key] = pair.Value;
    }

    public IReadOnlyDictionary<string, IReadOnlyList<string>> ExportPatternMap()
        => _knn.ExportPairs()
            .GroupBy(pair => pair.FlowRef ?? "UNKNOWN", StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group.Select(pair => pair.PairId).Order(StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);

    private static OrchestrationFlowGroundingWeights BuildWeights(OrchestrationTrainingPair pair)
    {
        return new OrchestrationFlowGroundingWeights(
            BuildTokenWeights(BuildInputOnlyTrainingTokens(pair)),
            BuildTokenWeights(BuildAllowedScopeFeatureTokens(pair)),
            BuildTokenWeights(OrchestrationKnnStore.Tokenize(pair.Chosen.Title)),
            BuildTokenWeights(BuildEvidenceOnlyTargetTokens(pair)),
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
            {
                ["classification"] = 1.0f,
                ["plan"] = 1.0f,
                ["gate"] = 1.0f
            },
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
            {
                ["learned-flow-plan"] = 1.0f,
                ["state"] = 0.5f
            },
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
            {
                ["Gate B"] = 1.0f,
                ["approval"] = 0.5f
            },
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
            {
                ["project-wiki"] = 1.0f,
                ["orchestration"] = 1.0f
            });
    }

    private static IReadOnlyList<string> BuildInputOnlyTrainingTokens(OrchestrationTrainingPair pair)
        => OrchestrationKnnStore.Tokenize(pair.Prompt);

    private static IReadOnlyList<string> BuildAllowedScopeFeatureTokens(OrchestrationTrainingPair pair)
        => OrchestrationKnnStore.Tokenize(pair.FlowRef ?? string.Empty);

    private static IReadOnlyList<string> BuildEvidenceOnlyTargetTokens(OrchestrationTrainingPair pair)
        => OrchestrationKnnStore.Tokenize($"{pair.FlowRef} {pair.Chosen.FlowRef}");

    private static IReadOnlyDictionary<string, float> BuildTokenWeights(IEnumerable<string> tokens)
        => tokens
            .GroupBy(token => token, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => (float)Math.Min(3, group.Count()), StringComparer.OrdinalIgnoreCase);

    private static IReadOnlyList<string> BuildRuntimeEvidenceFeatures(OrchestrationFlowGroundingWeights weights)
        => weights.InputFeatureWeights.Keys
            .Concat(weights.AllowedScopeFeatures.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.Ordinal)
            .Take(8)
            .ToArray();

    private static string BuildQueryText(string prompt, HumanIntentContract contract)
        => string.Join(
            ' ',
            new[]
            {
                prompt,
                string.Join(' ', contract.AllowedFlowScope),
                string.Join(' ', contract.RequestedFlowTextSpans),
                string.Join(' ', contract.HumanConstraints)
            }.Where(part => !string.IsNullOrWhiteSpace(part)));

    private static string BuildLearnedPairQueryTextWithoutPredictedFlow(
        string prompt,
        HumanIntentContract contract,
        FlowGroundingResult grounding)
        => string.Join(
            ' ',
            new[]
            {
                prompt,
                string.Join(' ', contract.AllowedFlowScope),
                string.Join(' ', contract.RequestedFlowTextSpans),
                string.Join(' ', contract.HumanConstraints),
                string.Join(' ', grounding.EvidenceFeatures)
            }.Where(part => !string.IsNullOrWhiteSpace(part)));

    private static double ComputeInputOnlyConfidence(double inputOnlyScore, double nearestScore)
        => inputOnlyScore <= 0.0 || nearestScore <= 0.0
            ? 0.0
            : Math.Min(1.0, 0.70 + inputOnlyScore * 0.10 + nearestScore * 0.20);

    private static double ScoreTokens(
        IReadOnlySet<string> tokens,
        IReadOnlyDictionary<string, float> weights)
        => weights
            .Where(pair => tokens.Contains(pair.Key))
            .Sum(pair => pair.Value);
}
