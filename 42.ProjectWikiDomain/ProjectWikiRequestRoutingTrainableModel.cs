// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

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

    public static ProjectWikiRequestRoutingTrainableModel Train(IReadOnlyList<ProjectWikiRequestRoutingDpoPair> rows)
    {
        var model = new ProjectWikiRequestRoutingTrainableModel([], new double[Labels.Length]);
        for (var epoch = 0; epoch < EpochCount; epoch++)
        {
            foreach (var row in rows)
            {
                var features = Featurize(row.Prompt);
                var expected = LabelIndex(row.ExpectedRouteKind);
                var predicted = model.PredictIndex(features);
                if (predicted == expected)
                    continue;

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

    internal ProjectWikiRequestRoutingDecision RouteForEvaluation(string prompt)
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
            ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining => Decision(routeKind, ProjectWikiRequestRouterUnit.UnitId, "The held-out-gated request routing baseline selected request-router training.", confidence, ["developer_prompt", "route_kind_taxonomy"], topTwoMargin),
            ProjectWikiRequestRouteKind.NegativeDpoTraining => Decision(routeKind, NegativeDpoGeneratorTrainingRunner.UnitId, "The held-out-gated request routing baseline selected negative DPO training.", confidence, ["accepted_documentation_units", "negative_type_taxonomy"], topTwoMargin),
            ProjectWikiRequestRouteKind.ProjectValidation => Decision(routeKind, nameof(ProjectWikiRealProjectValidationRunner), "The held-out-gated request routing baseline selected real project wiki validation.", confidence, ["project_roots", "wiki_domain_root", "validation_question_set", "exact_evidence_gate"], topTwoMargin),
            ProjectWikiRequestRouteKind.AnswerComposerTraining => Decision(routeKind, ProjectWikiAnswerComposerTrainingRunner.UnitId, "The held-out-gated request routing baseline selected answer composition training.", confidence, ["retrieved_evidence", "answer_contract"], topTwoMargin),
            ProjectWikiRequestRouteKind.DocumentationGeneratorTraining => Decision(routeKind, DocumentationGeneratorUnit.UnitId, "The held-out-gated request routing baseline selected documentation generator training.", confidence, ["documentation_unit", "required_fields"], topTwoMargin),
            ProjectWikiRequestRouteKind.CodeTraceTraining => Decision(routeKind, CodeTraceBuilderUnit.UnitId, "The held-out-gated request routing baseline selected code trace training.", confidence, ["source_text", "expected_call_edges", "expected_value_flows"], topTwoMargin),
            ProjectWikiRequestRouteKind.EntityImpactTraining => Decision(routeKind, EntityImpactMapperUnit.UnitId, "The held-out-gated request routing baseline selected entity impact training.", confidence, ["entity_name", "evidence_records", "touchpoint_taxonomy"], topTwoMargin),
            ProjectWikiRequestRouteKind.DocumentationBuild => Decision(routeKind, ProjectDocumentationPipelineOrchestrator.FlowId, "The held-out-gated request routing baseline selected documentation build.", confidence, ["project_root", "documentation_version"], topTwoMargin),
            ProjectWikiRequestRouteKind.StackVersionTraining => Decision(routeKind, StackAndVersionClassifierUnit.UnitId, "The held-out-gated request routing baseline selected stack/version training.", confidence, ["csproj_text", "stack_version_taxonomy"], topTwoMargin),
            ProjectWikiRequestRouteKind.AnswerVerifierTraining => Decision(routeKind, ProjectAnswerVerifier.UnitId, "The held-out-gated request routing baseline selected answer verifier training.", confidence, ["candidate_answer", "evidence_index", "expected_project_id"], topTwoMargin),
            ProjectWikiRequestRouteKind.TraceAggregatorTraining => Decision(routeKind, WikiTraceAggregatorUnit.UnitId, "The held-out-gated request routing baseline selected trace aggregation training.", confidence, ["wiki_query_traces", "wiki_unit_traces"], topTwoMargin),
            ProjectWikiRequestRouteKind.QueryPlanningTraining => Decision(routeKind, ProjectWikiQueryPlanningFlow.FlowId, "The held-out-gated request routing baseline selected query planning training.", confidence, ["developer_prompt", "query_plan_taxonomy"], topTwoMargin),
            ProjectWikiRequestRouteKind.ContextRoutingTraining => Decision(routeKind, WikiContextRoutingUnit.UnitId, "The held-out-gated request routing baseline selected context routing training.", confidence, ["query_plan", "retriever_availability"], topTwoMargin),
            ProjectWikiRequestRouteKind.WikiQuestionAnswer => Decision(routeKind, ProjectLocalWikiOrchestrator.FlowId, "The held-out-gated request routing baseline selected project-local wiki answering.", confidence, ["project_id", "wiki_domain_root", "question"], topTwoMargin),
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

public sealed class ProjectWikiRequestRoutingUnit : ILogicalUnit<ProjectWikiRequestRoutingInput, ProjectWikiRequestRoutingDecision>
{
    public const string UnitId = "project-wiki-request-routing-unit";
    public const string UnitRole = "project_wiki_request_routing_confidence_decision";
    public const string DomainTag = "project-wiki";

    private readonly ProjectWikiRequestRoutingTrainableModel? _model;

    public ProjectWikiRequestRoutingUnit(ProjectWikiRequestRoutingTrainableModel? model = null)
    {
        _model = model;
    }

    public Task<OperationResult<LogicalUnitResult<ProjectWikiRequestRoutingDecision>>> ExecuteAsync(
        LogicalUnitRequest<ProjectWikiRequestRoutingInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<ProjectWikiRequestRoutingDecision>>.Fail("Request routing cancelled.", "REQUEST_ROUTING_CANCELLED"));

        if (request is null)
            return Task.FromResult(OperationResult<LogicalUnitResult<ProjectWikiRequestRoutingDecision>>.Fail("Logical unit request is required.", "LOGICAL_UNIT_REQUEST_REQUIRED"));

        if (string.IsNullOrWhiteSpace(request.Input.Prompt))
            return Task.FromResult(OperationResult<LogicalUnitResult<ProjectWikiRequestRoutingDecision>>.Fail("Prompt is required.", "PROMPT_REQUIRED"));

        var model = request.Input.ModelSnapshot is null
            ? _model
            : ProjectWikiRequestRoutingTrainableModel.FromSnapshot(request.Input.ModelSnapshot);
        var decision = model is null
            ? ProjectWikiRequestRoutingTrainableModel.BuildLowConfidenceDecision(
                "Request routing deferred for second pass because no trained request-routing snapshot is loaded.",
                ProjectWikiRequestRoutingDecisionStatus.NoRoute,
                warnings: ["request_routing_snapshot_missing"])
            : model.Route(request.Input.Prompt);
        var evidenceIds = string.IsNullOrWhiteSpace(decision.GateEvidence?.EvaluationId)
            ? []
            : new[] { decision.GateEvidence.EvaluationId };
        var result = new LogicalUnitResult<ProjectWikiRequestRoutingDecision>(
            string.IsNullOrWhiteSpace(request.UnitId) ? UnitId : request.UnitId,
            UnitRole,
            DomainTag,
            decision,
            decision.Confidence,
            ["project_wiki_request_routing_gate"],
            [DomainTag, "request-routing"],
            evidenceIds,
            decision.Warnings);

        return Task.FromResult(OperationResult<LogicalUnitResult<ProjectWikiRequestRoutingDecision>>.Ok(result));
    }
}
