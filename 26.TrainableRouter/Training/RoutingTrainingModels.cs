// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

public sealed record RoutingTrainingExample(
    string ExampleId,
    string Prompt,
    string TaskSlice,
    string? RequiredDirection,
    string? DomainHint,
    string[] CandidateUnitIds,
    string ChosenUnitId,
    string RejectedUnitId,
    string CorrectReason,
    string WrongRouteConsequence);

public sealed record RoutingCaseResult(
    string ExampleId,
    string ExpectedUnitId,
    string ActualUnitId,
    bool Correct,
    double Confidence,
    bool KnowledgeEdge = false,
    double ScoreGap = 0,
    string DecisionReason = "");

public sealed record RoutingTrainingRunResult(
    int TrainingExampleCount,
    int EvaluationExampleCount,
    int Correct,
    double Accuracy,
    UnitScoringProfile[] Profiles,
    RoutingDpoPair[] DpoPairs,
    RoutingCaseResult[] Cases)
{
    public int KnowledgeEdgeCount { get; init; }

    public RoutingDecisionRecord[] DecisionRecords { get; init; } = [];
}

public sealed record OrchestrationScenarioExpectation(
    string ScenarioId,
    string Prompt,
    string[] ExpectedSteps);

public sealed record OrchestrationPromptAcceptanceCase(
    string ScenarioId,
    string Prompt,
    string[] ExpectedSteps,
    string[] ExpectedPhaseTags);

public sealed record OrchestrationAcceptanceCaseResult(
    string ScenarioId,
    string ActualUnitId,
    bool RoutedToExpectedUnit,
    double Confidence,
    string[] PredictedPhaseTags,
    string[] MissingPhaseTags,
    bool PhaseGatePassed);

public sealed record OrchestrationAcceptanceGateResult(
    int ScenarioCount,
    int RoutedCorrect,
    int PhaseCorrect,
    double RoutingAccuracy,
    double PhaseCoverage,
    OrchestrationAcceptanceCaseResult[] Cases);
