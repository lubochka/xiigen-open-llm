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
    // Код отказа, которым обучаемая память формул сообщает «такой формулы я не
    // учила». Это законное воздержание, а не сбой обращения.
    private const string NotInMemoryErrorCode = "EXPRESSION_NOT_IN_MEMORY";

    // Причина в записи результата: формула в уроках не встречалась. Это признак
    // воздержания, и ставится он ТОЛЬКО в неуспешной ветке — там, где память
    // отказалась отвечать с кодом «формулы нет в памяти». В успешном конверте
    // память ответила, поэтому этого признака там быть не может.
    private const string NotSeenReason = "expression_not_seen";

    // Причина в записи результата: ответ пришёл из выученной памяти.
    private const string ExactMatchReason = "exact_match";

    // Причина в записи результата: память ответила, но её ответ не зачтён
    // совпадением. Это НЕ воздержание: воздержание — это отказ отвечать, а здесь
    // ответ есть, просто он не тот. Человеческое значение так и читается —
    // «ответил, но не совпал», — и в долю воздержания такая строка не входит.
    private const string AnsweredNotMatchedReason = "answered_not_matched";

    // Причина в записи результата: обращение к памяти сорвалось. Это НЕ
    // воздержание и остаётся неправильным при любом ожидании.
    private const string ExecutionFailedReason = "execution_failed";

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
        // Доля воздержания считается по признаку воздержания, а не как дополнение
        // доли совпадений: сорванное обращение к памяти — тоже не совпадение.
        var heldOutAbstainRate = AbstainRate(heldOutResults);
        // Доля правильных по плечу закрытых формул входит в общий исход: без неё
        // прогон с сорванной памятью отчитался бы успехом.
        var heldOutCorrectRate = CorrectRate(heldOutResults);
        var falsePositiveRate = heldOutMatchedRate;
        var normalisationVariantAccuracy = CorrectRate(normalisationResults);
        var success = trainingMatchedRate == 1.0
            && trainingCorrectRate == 1.0
            && heldOutMatchedRate == 0.0
            && heldOutAbstainRate == 1.0
            && heldOutCorrectRate == 1.0
            && falsePositiveRate == 0.0
            && normalisationVariantAccuracy == 1.0;

        return OperationResult<E13BHeldOutValidationResult>.Ok(new E13BHeldOutValidationResult(
            TrainingCount: trainingRows.Count,
            HeldOutCount: heldOutRows.Count,
            TrainingMatchedRate: trainingMatchedRate,
            TrainingCompletionAccuracy: trainingCorrectRate,
            HeldOutMatchedRate: heldOutMatchedRate,
            HeldOutAbstainRate: heldOutAbstainRate,
            HeldOutCorrectRate: heldOutCorrectRate,
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

            // Отказ больше не означает один исход: память формул отвечает
            // типизированным отказом и на законное незнание формулы, и на
            // настоящий сбой обращения. Эти два случая считаются по-разному.
            if (!result.IsSuccess)
            {
                // Воздержание опознаётся по коду отказа самой памяти, а не по
                // пустоте поля ответа: пустого успешного конверта больше нет.
                var abstained = IsNotInMemoryRefusal(result);

                results.Add(new E13BExpressionEvaluation(
                    row.Expression,
                    row.Partial,
                    row.Completion,
                    // У отказа значения ответа нет вовсе.
                    "",
                    // Воздержание не является совпадением ни при каком ожидании.
                    false,
                    // Для плеча «ожидаем, что формулу НЕ знают» воздержание и есть
                    // правильный ответ; любой другой отказ неправилен всегда.
                    abstained && !expectMatch,
                    // Уверенности у отказа нет.
                    0,
                    // Причина различает «формулу не учили» и «обращение сорвалось».
                    abstained ? NotSeenReason : ExecutionFailedReason));
                continue;
            }

            var value = result.Value!;

            // Успешный конверт после переноса всегда несёт выученное значение,
            // поэтому признак совпадения читается прямо из него.
            var matched = value.Confidence > 0.0 && value.Output is not null;

            results.Add(new E13BExpressionEvaluation(
                row.Expression,
                row.Partial,
                row.Completion,
                value.Output ?? "",
                matched,
                // В плече «ожидаем знание» правильность — это точное совпадение с
                // выученной правой частью. В плече «ожидаем незнание» любой ответ
                // памяти — ложное срабатывание, а не правильный ответ.
                expectMatch
                    ? matched && string.Equals(value.Output, row.Completion, StringComparison.Ordinal)
                    : !matched,
                value.Confidence,
                SuccessEnvelopeReason(matched)));
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

            // Тот же договор, что и в плечах обучающих и закрытых формул: отказ
            // «формулы нет в памяти» — законное воздержание, любой другой отказ —
            // настоящий сбой обращения.
            if (!result.IsSuccess)
            {
                // Признак воздержания берётся у кода отказа, а не у пустоты поля.
                var abstained = IsNotInMemoryRefusal(result);

                results.Add(new E13BExpressionEvaluation(
                    row.Query,
                    row.Query,
                    row.Expected,
                    // У отказа значения ответа нет вовсе.
                    "",
                    // Воздержание не является совпадением.
                    false,
                    // Запрос, от которого ожидали незнания, воздержанием отвечен
                    // правильно; при ожидании знания воздержание неправильно.
                    abstained && !row.ExpectMatch,
                    // Уверенности у отказа нет.
                    0,
                    // Причина различает «формулу не учили» и «обращение сорвалось».
                    abstained ? NotSeenReason : ExecutionFailedReason));
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
                SuccessEnvelopeReason(matched)));
        }

        return results;
    }

    // Отличает честное воздержание обучаемой памяти от настоящего сбоя
    // обращения: законным воздержанием считается только отказ с кодом «формулы
    // нет в памяти», любой другой код и отсутствие кода — сбой.
    //
    // Тип конверта здесь выписан полностью не для красоты. В этом проекте опытов
    // есть свой одноимённый конверт результата (ExperimentsV2.OperationResult),
    // и короткое имя в этом файле означает именно его, а у него поля с кодом
    // причины нет вообще. Код причины несёт только конверт слоя единиц
    // (AgentInfrastructure.OperationResult), который и возвращает память формул.
    private static bool IsNotInMemoryRefusal(
        AgentInfrastructure.OperationResult<LogicalUnitResult<string>> result)
        => string.Equals(result.ErrorCode, NotInMemoryErrorCode, StringComparison.Ordinal);

    // Разрешитель признака исхода для УСПЕШНОГО конверта ответа памяти формул.
    // Успешный конверт означает ровно одно: обращение состоялось и память
    // ответила. Дальше возможны два исхода, и ни один из них не является
    // воздержанием: ответ зачтён совпадением с выученной правой частью, либо
    // ответ есть, но совпадением он не зачтён.
    //
    // Прежде второй исход получал признак воздержания «формула не встречалась».
    // Это сливало два разных события в одно — «память честно промолчала» и
    // «память ответила неверно», — то есть прятало неверные ответы в графу
    // честных отказов. Доля воздержания считается именно по этому признаку,
    // поэтому она раздувалась бы неответами, и условие успеха прогона закрылось
    // бы само собой.
    //
    // Воздержание живёт только в неуспешной ветке и опознаётся там по коду
    // отказа самой памяти, а не здесь.
    //
    // Отдельным методом это выражение вынесено ещё и потому, что через рабочую
    // поверхность второй исход сегодня недостижим: память отвечает успехом
    // только когда нашла значение, и тогда уверенность всегда единица, а
    // значение ответа непусто. Отдельный метод делает недостижимую ветку
    // предъявляемой проверке без подделки продуктовой логики.
    private static string SuccessEnvelopeReason(bool matched)
        => matched ? ExactMatchReason : AnsweredNotMatchedReason;

    private static double MatchedRate(IReadOnlyList<E13BExpressionEvaluation> results)
        => results.Count == 0 ? 0 : results.Count(result => result.Matched) / (double)results.Count;

    private static double CorrectRate(IReadOnlyList<E13BExpressionEvaluation> results)
        => results.Count == 0 ? 0 : results.Count(result => result.Correct) / (double)results.Count;

    // Доля воздержания: сколько строк честно промолчали. Строка промолчала тогда
    // и только тогда, когда причина в записи — «формула не встречалась».
    //
    // Считать эту долю как «единица минус доля совпадений» нельзя. Сорванное
    // обращение к памяти — тоже не совпадение, поэтому такой подсчёт выдал бы
    // сбой за честное молчание: прогон, в котором не промолчал никто, показывал
    // бы сто процентов воздержания.
    private static double AbstainRate(IReadOnlyList<E13BExpressionEvaluation> results)
        => results.Count == 0
            ? 0
            : results.Count(result =>
                  string.Equals(result.Reason, NotSeenReason, StringComparison.Ordinal))
              / (double)results.Count;
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
    double HeldOutCorrectRate,
    double FalsePositiveRate,
    double NormalisationVariantAccuracy,
    bool Success,
    IReadOnlyList<E13BExpressionEvaluation> TrainingResults,
    IReadOnlyList<E13BExpressionEvaluation> HeldOutResults,
    IReadOnlyList<E13BExpressionEvaluation> NormalisationVariantResults);
