// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace PossibleDiffs.Candidate;
using ProjectWikiDomain;

using System.Text;

public sealed class ProjectWikiRequestRoutingTrainableModel
{
    public const double MinimumHeldOutRouteAccuracy = 0.95;
    public const double MinimumWrongAnnotationRate = 1.0;
    public const double MinimumAverageConfidence = 0.85;
    public const double MinimumTopTwoMargin = 0.02;
    public const double MinimumSelectionConfidence = 0.85;

    private const int EpochCount = 18;
    private const double LearningRate = 0.14;
    private const double LowConfidenceCeiling = 0.49;
    private const double NumericZero = 0.000000001;

    private static readonly ProjectWikiRequestRouteKind[] Labels =
    [
        ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining,
        ProjectWikiRequestRouteKind.NegativeDpoTraining,
        ProjectWikiRequestRouteKind.ProjectValidation,
        ProjectWikiRequestRouteKind.AnswerComposerTraining,
        ProjectWikiRequestRouteKind.DocumentationGeneratorTraining,
        ProjectWikiRequestRouteKind.CodeTraceTraining,
        ProjectWikiRequestRouteKind.EntityImpactTraining,
        ProjectWikiRequestRouteKind.DocumentationBuild,
        ProjectWikiRequestRouteKind.StackVersionTraining,
        ProjectWikiRequestRouteKind.AnswerVerifierTraining,
        ProjectWikiRequestRouteKind.TraceAggregatorTraining,
        ProjectWikiRequestRouteKind.QueryPlanningTraining,
        ProjectWikiRequestRouteKind.ContextRoutingTraining,
        ProjectWikiRequestRouteKind.WikiQuestionAnswer,
        ProjectWikiRequestRouteKind.NotProjectWiki
    ];

    private readonly Dictionary<string, double[]> _tokenWeights;
    private readonly double[] _biases;
    private readonly ProjectWikiRequestRoutingGateEvidence? _gateEvidence;

    private ProjectWikiRequestRoutingTrainableModel(
        Dictionary<string, double[]> tokenWeights,
        double[] biases,
        ProjectWikiRequestRoutingGateEvidence? gateEvidence = null)
    {
        _tokenWeights = tokenWeights;
        _biases = biases;
        _gateEvidence = gateEvidence;
    }

    public int AuditPresentationCount { get; private set; }
    public int AuditModelUpdateCount { get; private set; }

    public static ProjectWikiRequestRoutingTrainableModel Train(IReadOnlyList<ProjectWikiRequestRoutingDpoPair> rows)
    {
        var model = new ProjectWikiRequestRoutingTrainableModel([], new double[Labels.Length]);
        for (var epoch = 0; epoch < EpochCount; epoch++)
        {
            foreach (var row in rows)
            {
                model.AuditPresentationCount++;
                var features = Featurize(row.Prompt);
                var expected = LabelIndex(row.ExpectedRouteKind);
                var scores = model.Score(features);
                var predicted = 0;
                var bestCompetingScore = double.NegativeInfinity;
                for (var index = 0; index < scores.Length; index++)
                {
                    if (index != expected && scores[index] > bestCompetingScore)
                    {
                        predicted = index;
                        bestCompetingScore = scores[index];
                    }
                }
                const double trainingMargin = 0.10;
                if (scores[expected] - bestCompetingScore >= trainingMargin)
                    continue;

                model.AuditModelUpdateCount++;
                foreach (var (token, value) in features)
                {
                    var weights = model.GetWeights(token);
                    weights[expected] += LearningRate * value;
                    weights[predicted] -= LearningRate * value;
                }

                model._biases[expected] += LearningRate;
                model._biases[predicted] -= LearningRate;
            }
        }

        return model;
    }

    public static ProjectWikiRequestRoutingTrainableModel FromSnapshot(ProjectWikiRequestRoutingModelSnapshot snapshot)
        => new(
            snapshot.TokenWeights.ToDictionary(
                item => item.Key,
                item => NormalizeVector(item.Value),
                StringComparer.Ordinal),
            NormalizeVector(snapshot.Biases),
            snapshot.ToGateEvidence());

    public ProjectWikiRequestRoutingDecision Route(string prompt)
        => DecideOrDefer(prompt, requireEvaluationGate: true);

    public ProjectWikiRequestRoutingDecision RouteForEvaluation(string prompt)
        => DecideOrDefer(prompt, requireEvaluationGate: false);

    public ProjectWikiRequestRoutingTrainableModel WithGateEvidence(ProjectWikiRequestRoutingGateEvidence gateEvidence)
        => new(CopyTokenWeights(_tokenWeights), _biases.ToArray(), gateEvidence);

    public ProjectWikiRequestRoutingModelSnapshot ToSnapshot(ProjectWikiRequestRoutingGateEvidence? gateEvidence = null)
    {
        var selectedGate = gateEvidence ?? _gateEvidence ?? MissingEvaluationGate();
        return new ProjectWikiRequestRoutingModelSnapshot(
            "averaged-token-perceptron-v1",
            Labels.Select(label => label.ToString()).ToArray(),
            CopyTokenWeights(_tokenWeights),
            _biases.ToArray(),
            EpochCount,
            _tokenWeights.Count,
            selectedGate.TrainingRowCount,
            selectedGate.HeldOutRowCount,
            selectedGate.EvaluationId,
            selectedGate.RouteAccuracy,
            selectedGate.WrongAnnotationRate,
            selectedGate.AverageConfidence,
            selectedGate.SemanticQualityClaimed,
            selectedGate.HeldOutGatePassed,
            selectedGate.MinimumRouteAccuracy,
            selectedGate.MinimumWrongAnnotationRate,
            selectedGate.MinimumAverageConfidence,
            selectedGate.MinimumTopTwoMargin,
            selectedGate.MinimumSelectionConfidence,
            selectedGate.PublishAllowed,
            selectedGate.Warnings);
    }

    public static ProjectWikiRequestRoutingDecision BuildLowConfidenceDecision(
        string reason,
        ProjectWikiRequestRoutingDecisionStatus status = ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass,
        ProjectWikiRequestRoutingGateEvidence? gateEvidence = null,
        double topTwoMargin = 0,
        string[]? warnings = null)
    {
        var selectedWarnings = warnings is { Length: > 0 }
            ? warnings
            : ["request_routing_deferred_needs_second_pass"];
        return new ProjectWikiRequestRoutingDecision(
            ProjectWikiRequestRouteKind.NotProjectWiki,
            "main-orchestrator",
            reason,
            LowConfidenceFromMargin(topTwoMargin),
            [],
            selectedWarnings)
        {
            DecisionStatus = status,
            GateEvidence = gateEvidence,
            TopTwoMargin = topTwoMargin
        };
    }

    private ProjectWikiRequestRoutingDecision DecideOrDefer(string prompt, bool requireEvaluationGate)
    {
        var features = Featurize(prompt ?? "");
        var scores = Score(features);
        var predicted = MaxIndex(scores);
        var topTwoMargin = ComputeTopTwoMargin(scores);

        if (!HasTrainedSignal())
            return BuildLowConfidenceDecision(
                "Request routing deferred for second pass because this trainable model has no trained route weights.",
                ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass,
                _gateEvidence,
                topTwoMargin,
                ["request_routing_model_untrained"]);

        if (scores.All(IsZero))
            return BuildLowConfidenceDecision(
                "Request routing deferred for second pass because every route score is zero.",
                ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass,
                _gateEvidence,
                topTwoMargin,
                ["request_routing_scores_all_zero"]);

        if (topTwoMargin < MinimumTopTwoMargin)
            return BuildLowConfidenceDecision(
                "Request routing deferred for second pass because the top-two route margin is below the selection threshold.",
                ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass,
                _gateEvidence,
                topTwoMargin,
                ["request_routing_margin_below_gate"]);

        if (requireEvaluationGate && !HasPassedEvaluationGate(_gateEvidence))
            return BuildLowConfidenceDecision(
                "Request routing deferred for second pass because no passed held-out evaluation gate is attached to this snapshot.",
                ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass,
                _gateEvidence,
                topTwoMargin,
                ["request_routing_missing_or_failed_eval_gate"]);

        var confidence = CalibratedConfidence(scores, predicted, topTwoMargin);
        if (confidence < MinimumSelectionConfidence)
            return BuildLowConfidenceDecision(
                "Request routing deferred for second pass because calibrated confidence is below the selection threshold.",
                ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass,
                _gateEvidence,
                topTwoMargin,
                ["request_routing_confidence_below_gate"]);

        var routeKind = Labels[predicted];
        return routeKind switch
        {
            ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining => Decision(routeKind, "project-wiki-request-router-unit", "The held-out-gated request routing baseline selected request-router training.", confidence, ["developer_prompt", "route_kind_taxonomy"], topTwoMargin),
            ProjectWikiRequestRouteKind.NegativeDpoTraining => Decision(routeKind, "negative-dpo-generator-unit", "The held-out-gated request routing baseline selected negative DPO training.", confidence, ["accepted_documentation_units", "negative_type_taxonomy"], topTwoMargin),
            ProjectWikiRequestRouteKind.ProjectValidation => Decision(routeKind, "ProjectWikiRealProjectValidationRunner", "The held-out-gated request routing baseline selected real project wiki validation.", confidence, ["project_roots", "wiki_domain_root", "validation_question_set", "exact_evidence_gate"], topTwoMargin),
            ProjectWikiRequestRouteKind.AnswerComposerTraining => Decision(routeKind, "project-wiki-answer-composer-unit", "The held-out-gated request routing baseline selected answer composition training.", confidence, ["retrieved_evidence", "answer_contract"], topTwoMargin),
            ProjectWikiRequestRouteKind.DocumentationGeneratorTraining => Decision(routeKind, "documentation-generator-unit", "The held-out-gated request routing baseline selected documentation generator training.", confidence, ["documentation_unit", "required_fields"], topTwoMargin),
            ProjectWikiRequestRouteKind.CodeTraceTraining => Decision(routeKind, "code-trace-builder-unit", "The held-out-gated request routing baseline selected code trace training.", confidence, ["source_text", "expected_call_edges", "expected_value_flows"], topTwoMargin),
            ProjectWikiRequestRouteKind.EntityImpactTraining => Decision(routeKind, "entity-impact-mapper-unit", "The held-out-gated request routing baseline selected entity impact training.", confidence, ["entity_name", "evidence_records", "touchpoint_taxonomy"], topTwoMargin),
            ProjectWikiRequestRouteKind.DocumentationBuild => Decision(routeKind, "project-documentation-pipeline-orchestrator", "The held-out-gated request routing baseline selected documentation build.", confidence, ["project_root", "documentation_version"], topTwoMargin),
            ProjectWikiRequestRouteKind.StackVersionTraining => Decision(routeKind, "stack-and-version-classifier-unit", "The held-out-gated request routing baseline selected stack/version training.", confidence, ["csproj_text", "stack_version_taxonomy"], topTwoMargin),
            ProjectWikiRequestRouteKind.AnswerVerifierTraining => Decision(routeKind, "project-answer-verifier-unit", "The held-out-gated request routing baseline selected answer verifier training.", confidence, ["candidate_answer", "evidence_index", "expected_project_id"], topTwoMargin),
            ProjectWikiRequestRouteKind.TraceAggregatorTraining => Decision(routeKind, "wiki-trace-aggregator-unit", "The held-out-gated request routing baseline selected trace aggregation training.", confidence, ["wiki_query_traces", "wiki_unit_traces"], topTwoMargin),
            ProjectWikiRequestRouteKind.QueryPlanningTraining => Decision(routeKind, "project-wiki-query-planning-flow", "The held-out-gated request routing baseline selected query planning training.", confidence, ["developer_prompt", "query_plan_taxonomy"], topTwoMargin),
            ProjectWikiRequestRouteKind.ContextRoutingTraining => Decision(routeKind, "wiki-context-routing-unit", "The held-out-gated request routing baseline selected context routing training.", confidence, ["query_plan", "retriever_availability"], topTwoMargin),
            ProjectWikiRequestRouteKind.WikiQuestionAnswer => Decision(routeKind, "project-local-wiki-orchestrator", "The held-out-gated request routing baseline selected project-local wiki answering.", confidence, ["project_id", "wiki_domain_root", "question"], topTwoMargin),
            _ => Decision(ProjectWikiRequestRouteKind.NotProjectWiki, "main-orchestrator", "The held-out-gated request routing baseline selected the main orchestrator.", confidence, [], topTwoMargin)
        };
    }

    private ProjectWikiRequestRoutingDecision Decision(
        ProjectWikiRequestRouteKind routeKind,
        string targetUnitId,
        string reason,
        double confidence,
        string[] requiredInputs,
        double topTwoMargin)
        => new(routeKind, targetUnitId, reason, confidence, requiredInputs, [])
        {
            DecisionStatus = ProjectWikiRequestRoutingDecisionStatus.Selected,
            GateEvidence = _gateEvidence,
            TopTwoMargin = topTwoMargin
        };

    private int PredictIndex(Dictionary<string, double> features)
        => MaxIndex(Score(features));

    private double[] Score(Dictionary<string, double> features)
    {
        var scores = _biases.ToArray();
        foreach (var (token, value) in features)
        {
            if (!_tokenWeights.TryGetValue(token, out var weights))
                continue;

            for (var index = 0; index < scores.Length; index++)
                scores[index] += weights[index] * value;
        }

        return scores;
    }

    private double[] GetWeights(string token)
    {
        if (_tokenWeights.TryGetValue(token, out var weights))
            return weights;

        weights = new double[Labels.Length];
        _tokenWeights[token] = weights;
        return weights;
    }

    private bool HasTrainedSignal()
        => _biases.Any(value => !IsZero(value)) ||
           _tokenWeights.Values.Any(weights => weights.Any(value => !IsZero(value)));

    private static bool HasPassedEvaluationGate(ProjectWikiRequestRoutingGateEvidence? gateEvidence)
        => gateEvidence is not null &&
           gateEvidence.HeldOutGatePassed &&
           gateEvidence.RouteAccuracy >= gateEvidence.MinimumRouteAccuracy &&
           gateEvidence.WrongAnnotationRate >= gateEvidence.MinimumWrongAnnotationRate &&
           gateEvidence.AverageConfidence >= gateEvidence.MinimumAverageConfidence;

    private static ProjectWikiRequestRoutingGateEvidence MissingEvaluationGate()
        => new(
            "missing-held-out-evaluation-gate",
            0,
            0,
            0,
            0,
            0,
            false,
            false,
            false,
            MinimumHeldOutRouteAccuracy,
            MinimumWrongAnnotationRate,
            MinimumAverageConfidence,
            MinimumTopTwoMargin,
            MinimumSelectionConfidence,
            ["request_routing_snapshot_missing_eval_gate"]);

    private static int LabelIndex(ProjectWikiRequestRouteKind routeKind)
    {
        var index = Array.IndexOf(Labels, routeKind);
        return index < 0 ? Labels.Length - 1 : index;
    }

    private static int MaxIndex(double[] values)
    {
        var best = 0;
        for (var index = 1; index < values.Length; index++)
        {
            if (values[index] > values[best])
                best = index;
        }

        return best;
    }

    private static double ComputeTopTwoMargin(double[] scores)
    {
        if (scores.Length == 0)
            return 0;

        var ordered = scores.OrderByDescending(score => score).Take(2).ToArray();
        return ordered.Length < 2 ? Math.Max(0, ordered[0]) : Math.Max(0, ordered[0] - ordered[1]);
    }

    private static double CalibratedConfidence(double[] scores, int predicted, double topTwoMargin)
    {
        if (topTwoMargin < MinimumTopTwoMargin)
            return LowConfidenceFromMargin(topTwoMargin);

        var marginConfidence = 1.0 / (1.0 + Math.Exp(-4.0 * topTwoMargin));
        var scoreConfidence = 1.0 / (1.0 + Math.Exp(-Math.Max(0, scores[predicted])));
        return Math.Clamp(0.70 + marginConfidence * 0.20 + scoreConfidence * 0.09, MinimumSelectionConfidence, 0.99);
    }

    private static double LowConfidenceFromMargin(double topTwoMargin)
    {
        var normalized = Math.Max(0, topTwoMargin) / Math.Max(MinimumTopTwoMargin, NumericZero);
        return Math.Clamp(normalized * LowConfidenceCeiling, 0, LowConfidenceCeiling);
    }

    private static bool IsZero(double value)
        => Math.Abs(value) <= NumericZero;

    private static Dictionary<string, double> Featurize(string text)
    {
        var tokens = RegexLikeWords(text).ToArray();
        var features = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var token in tokens)
            features[token] = features.TryGetValue(token, out var value) ? value + 1.0 : 1.0;
        for (var index = 0; index + 1 < tokens.Length; index++)
        {
            var bigram = $"{tokens[index]}_{tokens[index + 1]}";
            features[bigram] = features.TryGetValue(bigram, out var value) ? value + 1.0 : 1.0;
        }

        var normalizer = Math.Max(1.0, Math.Sqrt(features.Values.Sum(value => value * value)));
        foreach (var key in features.Keys.ToArray())
            features[key] /= normalizer;
        return features;
    }

    private static IEnumerable<string> RegexLikeWords(string text)
    {
        var builder = new StringBuilder();
        foreach (var character in text)
        {
            if (char.IsLetterOrDigit(character) || character == '_' || character == '-')
            {
                builder.Append(char.ToLowerInvariant(character));
                continue;
            }

            if (builder.Length == 0)
                continue;

            yield return builder.ToString();
            builder.Clear();
        }

        if (builder.Length > 0)
            yield return builder.ToString();
    }

    private static Dictionary<string, double[]> CopyTokenWeights(Dictionary<string, double[]> tokenWeights)
        => tokenWeights.ToDictionary(item => item.Key, item => item.Value.ToArray(), StringComparer.Ordinal);

    private static double[] NormalizeVector(double[] values)
    {
        var normalized = new double[Labels.Length];
        Array.Copy(values, normalized, Math.Min(values.Length, normalized.Length));
        return normalized;
    }
}
