// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

using AgentInfrastructure.Logical;

// E13B v2 held-out validation for exact expression memory.
// Known training partials must match; unseen held-out partials must abstain.
public sealed class E13BHeldOutValidation
{
    public async Task<OperationResult<E13BHeldOutValidationResult>> RunAsync(
        CancellationToken cancellationToken = default)
    {
        var trainingRows = TrainingRows();
        var heldOutRows = HeldOutRows();
        var normalisationRows = NormalisationVariantRows();
        var trained = ExpressionCompletionUnit.Train(trainingRows.Select(row => row.Expression).ToArray());

        if (!trained.IsSuccess)
            return OperationResult<E13BHeldOutValidationResult>.Fail(trained.ErrorMessage);

        var trainingResults = await EvaluateAsync(trained.Value!, trainingRows, expectMatch: true, cancellationToken);
        var heldOutResults = await EvaluateAsync(trained.Value!, heldOutRows, expectMatch: false, cancellationToken);
        var normalisationResults = await EvaluateQueriesAsync(trained.Value!, normalisationRows, cancellationToken);
        var trainingMatchedRate = MatchedRate(trainingResults);
        var trainingCorrectRate = CorrectRate(trainingResults);
        var heldOutMatchedRate = MatchedRate(heldOutResults);
        var heldOutAbstainRate = 1.0 - heldOutMatchedRate;
        var falsePositiveRate = heldOutMatchedRate;
        var normalisationVariantAccuracy = CorrectRate(normalisationResults);
        var success = trainingMatchedRate == 1.0
            && trainingCorrectRate == 1.0
            && heldOutMatchedRate == 0.0
            && heldOutAbstainRate == 1.0
            && falsePositiveRate == 0.0
            && normalisationVariantAccuracy == 1.0;

        return OperationResult<E13BHeldOutValidationResult>.Ok(new E13BHeldOutValidationResult(
            TrainingCount: trainingRows.Count,
            HeldOutCount: heldOutRows.Count,
            TrainingMatchedRate: trainingMatchedRate,
            TrainingCompletionAccuracy: trainingCorrectRate,
            HeldOutMatchedRate: heldOutMatchedRate,
            HeldOutAbstainRate: heldOutAbstainRate,
            FalsePositiveRate: falsePositiveRate,
            NormalisationVariantAccuracy: normalisationVariantAccuracy,
            Success: success,
            TrainingResults: trainingResults,
            HeldOutResults: heldOutResults,
            NormalisationVariantResults: normalisationResults));
    }

    public async Task<OperationResult<ExperimentV2Result>> RunAsExperimentResultAsync(
        int tier = 1,
        CancellationToken cancellationToken = default)
    {
        var result = await RunAsync(cancellationToken);
        if (!result.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail(result.ErrorMessage);

        var summary = result.Value!;
        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId = "E13B-EXPRESSION-PREFIX-COMPLETION-HELDOUT",
            Tier = tier,
            BaselinePrimary = 0,
            EnhancedPrimary = summary.HeldOutAbstainRate,
            PrimaryMetricName = "held_out_abstain_rate",
            LowerIsBetter = false,
            ScalingSignal = summary.FalsePositiveRate,
            ScalingSignalNote = "held_out_false_positive_rate",
            ShouldScaleUp = !summary.Success,
            BaselineTimeMs = 0,
            EnhancedTimeMs = 0,
            Notes = $"train={summary.TrainingCount}, heldOut={summary.HeldOutCount}, " +
                    $"trainingMatched={summary.TrainingMatchedRate:P1}, " +
                    $"trainingCorrect={summary.TrainingCompletionAccuracy:P1}, " +
                    $"heldOutMatched={summary.HeldOutMatchedRate:P1}, " +
                    $"heldOutAbstained={summary.HeldOutAbstainRate:P1}, " +
                    $"falsePositive={summary.FalsePositiveRate:P1}, " +
                    $"normalisationVariants={summary.NormalisationVariantAccuracy:P1}, " +
                    $"success={summary.Success}"
        });
    }

    private static List<E13BExpressionRow> TrainingRows()
    {
        return
        [
            new("Y + Z = X"),
            new("Width + Padding = OuterWidth"),
            new("Height + Margin = OuterHeight"),
            new("Subtotal + Tax = Total"),
            new("Price * Quantity = Total"),
            new("Revenue - Cost = Profit"),
            new("Start + Duration = End"),
            new("End - Start = Duration"),
            new("Current + Delta = Next"),
            new("Previous + Change = Current"),
            new("Base * Rate = Fee"),
            new("Principal * Interest = Charge"),
            new("Rows * Columns = Cells"),
            new("PageSize * PageIndex = Offset"),
            new("Offset + Limit = WindowEnd"),
            new("Input + Transform = Output"),
            new("Prompt + Policy = Decision"),
            new("Memory + Query = Recall"),
            new("Branch + Evidence = Verdict"),
            new("Payload + Metadata = Envelope")
        ];
    }

    private static List<E13BExpressionRow> HeldOutRows()
    {
        return
        [
            new("Signal + Threshold = Alert"),
            new("Vector + Predicate = Score"),
            new("Score + Margin = Rank"),
            new("Plan + Constraint = Task"),
            new("Spec + Design = Blueprint"),
            new("Frame + Bytes = Payload"),
            new("Assets - Liabilities = Equity"),
            new("Errors + Warnings = Findings"),
            new("Request + Context = Prompt"),
            new("Success + Warning = Status")
        ];
    }

    private static List<E13BExpressionQuery> NormalisationVariantRows()
    {
        return
        [
            new("Revenue - Cost = ?", "Profit", true),
            new("Revenue-Cost=?", "Profit", true),
            new("REVENUE - COST = ?", "Profit", true),
            new("A + B = ?", "", false)
        ];
    }

    private static async Task<List<E13BExpressionEvaluation>> EvaluateAsync(
        ExpressionCompletionUnit unit,
        IReadOnlyList<E13BExpressionRow> rows,
        bool expectMatch,
        CancellationToken cancellationToken)
    {
        var results = new List<E13BExpressionEvaluation>();
        foreach (var row in rows)
        {
            var result = await unit.ExecuteAsync(new LogicalUnitRequest<string>(
                "e13b-heldout-validation",
                ExpressionCompletionUnit.Id,
                ExpressionCompletionUnit.Role,
                ExpressionCompletionUnit.DomainTag,
                row.Partial,
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()),
                cancellationToken);

            if (!result.IsSuccess)
            {
                results.Add(new E13BExpressionEvaluation(
                    row.Expression,
                    row.Partial,
                    row.Completion,
                    "",
                    false,
                    false,
                    0,
                    "execution_failed"));
                continue;
            }

            var value = result.Value!;
            results.Add(new E13BExpressionEvaluation(
                row.Expression,
                row.Partial,
                row.Completion,
                value.Output ?? "",
                value.Confidence > 0.0 && value.Output is not null,
                expectMatch
                    ? value.Confidence > 0.0 && string.Equals(value.Output, row.Completion, StringComparison.Ordinal)
                    : value.Confidence == 0.0 && value.Output is null,
                value.Confidence,
                value.Confidence > 0.0 ? "exact_match" : "expression_not_seen"));
        }

        return results;
    }

    private static async Task<List<E13BExpressionEvaluation>> EvaluateQueriesAsync(
        ExpressionCompletionUnit unit,
        IReadOnlyList<E13BExpressionQuery> rows,
        CancellationToken cancellationToken)
    {
        var results = new List<E13BExpressionEvaluation>();
        foreach (var row in rows)
        {
            var result = await unit.ExecuteAsync(new LogicalUnitRequest<string>(
                "e13b-heldout-validation",
                ExpressionCompletionUnit.Id,
                ExpressionCompletionUnit.Role,
                ExpressionCompletionUnit.DomainTag,
                row.Query,
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()),
                cancellationToken);

            if (!result.IsSuccess)
            {
                results.Add(new E13BExpressionEvaluation(
                    row.Query,
                    row.Query,
                    row.Expected,
                    "",
                    false,
                    false,
                    0,
                    "execution_failed"));
                continue;
            }

            var value = result.Value!;
            var matched = value.Confidence > 0.0 && value.Output is not null;
            var correct = row.ExpectMatch
                ? matched && string.Equals(value.Output, row.Expected, StringComparison.Ordinal)
                : !matched;

            results.Add(new E13BExpressionEvaluation(
                row.Query,
                row.Query,
                row.Expected,
                value.Output ?? "",
                matched,
                correct,
                value.Confidence,
                matched ? "exact_match" : "expression_not_seen"));
        }

        return results;
    }

    private static double MatchedRate(IReadOnlyList<E13BExpressionEvaluation> results)
        => results.Count == 0 ? 0 : results.Count(result => result.Matched) / (double)results.Count;

    private static double CorrectRate(IReadOnlyList<E13BExpressionEvaluation> results)
        => results.Count == 0 ? 0 : results.Count(result => result.Correct) / (double)results.Count;
}

public sealed record E13BExpressionRow(string Expression)
{
    public string Partial { get; } = Expression[..(Expression.LastIndexOf('=') + 1)].Trim();
    public string Completion { get; } = Expression[(Expression.LastIndexOf('=') + 1)..].Trim();
}

public sealed record E13BExpressionQuery(string Query, string Expected, bool ExpectMatch);

public sealed record E13BExpressionEvaluation(
    string Expression,
    string Partial,
    string Expected,
    string Predicted,
    bool Matched,
    bool Correct,
    double Confidence,
    string Reason);

public sealed record E13BHeldOutValidationResult(
    int TrainingCount,
    int HeldOutCount,
    double TrainingMatchedRate,
    double TrainingCompletionAccuracy,
    double HeldOutMatchedRate,
    double HeldOutAbstainRate,
    double FalsePositiveRate,
    double NormalisationVariantAccuracy,
    bool Success,
    IReadOnlyList<E13BExpressionEvaluation> TrainingResults,
    IReadOnlyList<E13BExpressionEvaluation> HeldOutResults,
    IReadOnlyList<E13BExpressionEvaluation> NormalisationVariantResults);
