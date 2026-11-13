// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using AgentInfrastructure;

public sealed record HeldOutScorerIdentity(
    string DomainTag,
    string ScorerId,
    string ScorerVersion,
    string NormalizationProfileId,
    string MetricProfileId);

public sealed record ExpectedLabelSource(
    string Origin,
    string AuthorityRef,
    bool FromNodeUnderTest,
    string SplitId);

public sealed record HeldOutScoringRow(
    string CaseId,
    string ExpectedLabel,
    string PredictedOutput);

public sealed record HeldOutScoringRequest(
    string EvaluationSetId,
    string DomainTag,
    ExpectedLabelSource ExpectedLabelSource,
    IReadOnlyList<HeldOutScoringRow> Rows);

public sealed record ProductAccuracyScore(
    double RuntimeProductAccuracy,
    string PerDomainMetricUsed,
    int ScoredRows,
    string ExpectedLabelOrigin);

public interface IHeldOutScorer
{
    HeldOutScorerIdentity Identity { get; }

    OperationResult<ProductAccuracyScore> Score(HeldOutScoringRequest request);
}

public sealed record TeacherStageGapReport(
    string Stage,
    string ScorerId,
    string ScorerVersion,
    string ThresholdSetId,
    double HeldOutAccuracyYgoal,
    double HeldOutAccuracyYbest,
    double TeacherGapToGold,
    double TeacherBeatRate,
    double? StageGap9bVs235b,
    bool EvalOnly,
    string WindowRef)
{
    public double? MeasurementNoiseFloor { get; init; }
    public string MeasurementNoiseStatus { get; init; } =
        MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus;
}
