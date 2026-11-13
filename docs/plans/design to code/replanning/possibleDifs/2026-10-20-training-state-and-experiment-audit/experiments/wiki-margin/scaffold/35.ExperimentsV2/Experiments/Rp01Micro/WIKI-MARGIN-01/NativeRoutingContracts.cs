// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

// Extracted without changing record fields from pinned core source.
namespace ProjectWikiDomain;

public enum ProjectWikiRequestRouteKind
{
    DocumentationBuild,
    WikiQuestionAnswer,
    ProjectWikiRequestRouterTraining,
    StackVersionTraining,
    SystemBoundaryMapperTraining,
    CrossStackBusinessFlowGraphTraining,
    CodeTraceTraining,
    EntityImpactTraining,
    DocumentationGeneratorTraining,
    QueryPlanningTraining,
    AnswerComposerTraining,
    ContextRoutingTraining,
    NegativeDpoTraining,
    AnswerVerifierTraining,
    TraceAggregatorTraining,
    ProjectValidation,
    NotProjectWiki
}

public enum ProjectWikiRequestRoutingDecisionStatus
{
    Selected,
    NoRoute,
    NeedsSecondPass
}

public sealed record ProjectWikiRequestRoutingGateEvidence(
    string EvaluationId,
    int TrainingRowCount,
    int HeldOutRowCount,
    double RouteAccuracy,
    double WrongAnnotationRate,
    double AverageConfidence,
    bool SemanticQualityClaimed,
    bool HeldOutGatePassed,
    bool PublishAllowed,
    double MinimumRouteAccuracy,
    double MinimumWrongAnnotationRate,
    double MinimumAverageConfidence,
    double MinimumTopTwoMargin,
    double MinimumSelectionConfidence,
    string[] Warnings);

public sealed record ProjectWikiRequestRoutingDecision(
    ProjectWikiRequestRouteKind RouteKind,
    string TargetUnitId,
    string Reason,
    double Confidence,
    string[] RequiredInputs,
    string[] Warnings)
{
    public ProjectWikiRequestRoutingDecisionStatus DecisionStatus { get; init; } = ProjectWikiRequestRoutingDecisionStatus.Selected;
    public ProjectWikiRequestRoutingGateEvidence? GateEvidence { get; init; }
    public double TopTwoMargin { get; init; }
    public bool CanStartTargetRunner => DecisionStatus == ProjectWikiRequestRoutingDecisionStatus.Selected;
}

public sealed record ProjectWikiRequestRoutingDpoPair(
    string PairId,
    string PromptType,
    string Prompt,
    ProjectWikiRequestRouteKind ExpectedRouteKind,
    string A,
    string Chosen,
    string Rejected,
    string RejectedType);

public sealed record ProjectWikiRequestRoutingModelSnapshot(
    string ModelKind,
    string[] Labels,
    Dictionary<string, double[]> TokenWeights,
    double[] Biases,
    int EpochCount,
    int VocabularySize,
    int TrainingRowCount,
    int HeldOutRowCount,
    string EvaluationId,
    double RouteAccuracy,
    double WrongAnnotationRate,
    double AverageConfidence,
    bool SemanticQualityClaimed,
    bool GatePassed,
    double MinimumRouteAccuracy,
    double MinimumWrongAnnotationRate,
    double MinimumAverageConfidence,
    double MinimumTopTwoMargin,
    double MinimumSelectionConfidence,
    bool PublishAllowed,
    string[] GateWarnings)
{
    public ProjectWikiRequestRoutingGateEvidence ToGateEvidence()
        => new(
            EvaluationId,
            TrainingRowCount,
            HeldOutRowCount,
            RouteAccuracy,
            WrongAnnotationRate,
            AverageConfidence,
            SemanticQualityClaimed,
            GatePassed,
            PublishAllowed,
            MinimumRouteAccuracy,
            MinimumWrongAnnotationRate,
            MinimumAverageConfidence,
            MinimumTopTwoMargin,
            MinimumSelectionConfidence,
            GateWarnings);
}
