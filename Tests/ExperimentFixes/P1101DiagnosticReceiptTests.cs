// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Проверки приёмной квитанции опыта P11-PRIMARY-01, подслучай P11.1.
//
// Закрываемый дефект: прежний приёмщик складывал вердикт из части сторожей.
// Панель контроля конечной разностью и неизменность ответов плеча A считались и
// печатались, но в итог не входили. Поэтому провал контроля «в лоб» мог остаться
// внутри квитанции, а общий итог всё равно говорил «гипотеза подтверждена».
//
// Числа опорного случая взяты из уже сохранённой попытки
// experiments-v2/RP01-MICRO/P1101/results/attempt-0001 (status.json, arms.json,
// metrics.json). Ни одного обращения к модели, ни одного обновления весов здесь
// нет. Проверки вызывают ДЕЙСТВУЮЩИЙ построитель, который компилируется и в
// хост опыта, и сюда.

namespace LlmFromScratch.Tests;

using P1101Experiment;

public sealed class P1101DiagnosticReceiptTests
{
    /// <summary>Вид учебной строки панели, которой урок назначен напрямую.</summary>
    private const string LessonKind = "обучение и удержание";

    /// <summary>Вид экзаменационной строки панели.</summary>
    private const string ExamKind = "экзамен";

    /// <summary>
    /// Опорный случай: сохранённый прогон проходит все семь обязательных контролей,
    /// но допуск переноса не выдаётся, потому что 2 из 4 ответов объясняются
    /// постоянным ответчиком знаком «плюс».
    /// </summary>
    [Fact]
    public void SavedRunPassesEveryMandatoryControlYetStaysAtTheConstantPlusBaseline()
    {
        var receipt = P1101DiagnosticReceipt.Build(SavedRun()); // Действующий построитель на сохранённых числах.

        Assert.Equal(7, receipt.MandatoryControls.Count); // Семь обязательных контролей, названных ремонтом.
        Assert.Empty(receipt.FailedMandatoryControls); // На сохранённом прогоне не провалился ни один.
        Assert.True(receipt.AllMandatoryControlsAccepted); // Общий итог приёмки принят.
        Assert.Equal("SUPPORTED_FOR_NAMED_METRIC", receipt.HypothesisStatus); // Гипотеза подтверждена для названной меры.
        Assert.Equal(2, receipt.ConstantPlusBaselineCorrect); // Постоянный ответ «плюс» угадал бы две строки из четырёх.
        Assert.Equal(4, receipt.PanelRows); // В панели четыре строки.
        Assert.Equal(2, receipt.ArmB.CorrectAfter); // Плечо B тоже дало две верных строки.
        Assert.False(receipt.BeatsConstantPlusBaseline); // Тривиальный предел не превзойдён.
        Assert.Equal("NOT_ADMITTED_AT_OR_BELOW_CONSTANT_PLUS_BASELINE", receipt.TransferAdmission); // Перенос не допущен.
    }

    /// <summary>
    /// Переходы строк выводятся по отдельности: удержано, не выучено, исправлено и
    /// испорчено. Испорченная строка — валидная измеренная потеря, а не отсутствие проверки.
    /// </summary>
    [Fact]
    public void RowTransitionsSeparateHeldNotLearnedFixedAndBrokenForTheProductArm()
    {
        var receipt = P1101DiagnosticReceipt.Build(SavedRun()); // Квитанция сохранённого прогона.
        var armB = receipt.RowTransitions.Where(row => row.Arm == "B")
            .ToDictionary(row => row.RowId, row => row.Transition, StringComparer.Ordinal); // Переходы плеча B.

        Assert.Equal("удержано", armB["T1"]); // Верный ответ учебной строки сохранён.
        Assert.Equal("не выучено", armB["T2"]); // Вторая учебная строка так и осталась неверной.
        Assert.Equal("исправлено", armB["E1"]); // Экзаменационная строка стала верной.
        Assert.Equal("испорчено", armB["E2"]); // Вторая экзаменационная строка стала неверной.
        Assert.All(receipt.RowTransitions.Where(row => row.Arm == "A"),
            row => Assert.True(row.Transition is "удержано" or "не выучено")); // Плечо A ответы не меняло.
    }

    /// <summary>
    /// Главная проверка ремонта: провал одной координаты контроля «в лоб» опускает
    /// общий итог приёмки, хотя местный вывод оракула сложения остаётся верным.
    /// Прежний приёмщик этого не делал и объявлял гипотезу подтверждённой.
    /// </summary>
    [Fact]
    public void OneFailedFiniteDifferenceCoordinateStopsAcceptanceWhileTheScatterFindingHolds()
    {
        var observation = SavedRun(); // Исходные сохранённые числа.
        var broken = observation with
        {
            Controls = observation.Controls with { FiniteDifferenceAgreed = 8 } // Девять было, сошлось восемь.
        };

        var receipt = P1101DiagnosticReceipt.Build(broken); // Тот же действующий построитель.

        Assert.False(receipt.AllMandatoryControlsAccepted); // Приёмка опущена одним провалом.
        Assert.Contains("finite_difference_panel", receipt.FailedMandatoryControls); // Назван именно этот контроль.
        Assert.Equal("NOT_SUPPORTED_IN_VALID_COMPARISON", receipt.HypothesisStatus); // Гипотеза больше не подтверждена.
        Assert.Equal("NOT_ADMITTED_CONTROLS_FAILED", receipt.TransferAdmission); // Допуск переноса закрыт.
        Assert.True(receipt.LocalScatterFindingHolds); // Местный вывод оракула при этом остаётся верным.
        Assert.Equal("8/9", receipt.MandatoryControls.Single(x => x.Id == "finite_difference_panel").Observed);
    }

    /// <summary>
    /// Нарушенная панель плеча A: таблица не изменилась ни на одно число, но ответ
    /// строки стал другим. Такое наблюдение обязано опустить приёмку.
    /// </summary>
    [Fact]
    public void ChangedArmAAnswerAtZeroTableShiftFailsTheReadoutControl()
    {
        var observation = SavedRun(); // Исходные сохранённые числа.
        var after = observation.ArmA.After.ToArray(); // Копия ответов плеча A после обучения.
        after[0] = after[0] with { Produced = "-" }; // Ответ первой строки подменён при нулевом сдвиге таблицы.
        var broken = observation with { ArmA = observation.ArmA with { After = after } };

        var receipt = P1101DiagnosticReceipt.Build(broken); // Тот же действующий построитель.

        Assert.False(receipt.AllMandatoryControlsAccepted); // Приёмка опущена.
        Assert.Contains("arm_a_readout_unchanged", receipt.FailedMandatoryControls); // Назван контроль ответов плеча A.
        Assert.Equal("NOT_SUPPORTED_IN_VALID_COMPARISON", receipt.HypothesisStatus); // Гипотеза не подтверждена.
    }

    /// <summary>
    /// Сдвиг весов без наблюдаемого изменения ответов не считается учебным переходом:
    /// квалификация называет это прямо, а обязательный контроль перехода не проходит.
    /// </summary>
    [Fact]
    public void MovedWeightsWithoutAnyAnswerChangeDoNotQualifyAsAnObservedLearningTransition()
    {
        var observation = SavedRun(); // Исходные сохранённые числа.
        var frozenAnswers = observation.ArmB.Before.ToArray(); // Ответы до обучения.
        var broken = observation with
        {
            ArmB = observation.ArmB with { After = frozenAnswers } // После обучения ответы те же самые.
        };

        var receipt = P1101DiagnosticReceipt.Build(broken); // Тот же действующий построитель.

        Assert.Equal(1088, receipt.ArmB.ChangedCoordinates); // Веса при этом сдвинулись на 1088 чисел.
        Assert.False(receipt.ArmB.ObservedAnswerTransition); // Наблюдаемого изменения ответов нет.
        Assert.Equal("VALID_WEIGHTS_MOVED_NO_OBSERVED_ANSWER_TRANSITION", receipt.ArmB.LearningQualification);
        Assert.Contains("actual_learning_transition", receipt.FailedMandatoryControls); // Контроль перехода не прошёл.
        Assert.False(receipt.AllMandatoryControlsAccepted); // Приёмка опущена.
    }

    /// <summary>
    /// Один лишний контрольный прямой проход модели ломает полноту бюджета: контрольные
    /// фазы входят в общий счёт, а не стоят рядом с ним.
    /// </summary>
    [Fact]
    public void OneExtraControlForwardBreaksBudgetCompletenessBecauseControlPhasesAreCounted()
    {
        var observation = SavedRun(); // Исходные сохранённые числа.
        var broken = observation with
        {
            Budget = observation.Budget with { ControlModelForwards = observation.Budget.ControlModelForwards + 1 }
        };

        var receipt = P1101DiagnosticReceipt.Build(broken); // Тот же действующий построитель.

        Assert.Equal(60, receipt.Budget.TotalModelForwards); // Было 59, стало 60.
        Assert.False(receipt.Budget.MatchesExpectation); // Полный бюджет больше не совпадает.
        Assert.Contains("budget_completeness", receipt.FailedMandatoryControls); // Назван контроль полноты бюджета.
        Assert.False(receipt.AllMandatoryControlsAccepted); // Приёмка опущена.
    }

    /// <summary>
    /// Усечённое исполнение не закрывается правильными ответами: приёмка опускается,
    /// даже когда все семь контролей по отдельности прошли.
    /// </summary>
    [Fact]
    public void PartialExecutionIsNotRescuedByCorrectAnswers()
    {
        var observation = SavedRun(); // Исходные сохранённые числа.
        var allCorrect = observation.ArmB.After.Select(row => row with { Produced = row.Gold }).ToArray(); // Все ответы верные.
        var broken = observation with
        {
            ExecutionStatus = "PARTIAL", // Прогон исполнен не целиком.
            ArmB = observation.ArmB with { After = allCorrect }
        };

        var receipt = P1101DiagnosticReceipt.Build(broken); // Тот же действующий построитель.

        Assert.Equal(4, receipt.ArmB.CorrectAfter); // Ответы при этом все верные.
        Assert.True(receipt.BeatsConstantPlusBaseline); // И тривиальный предел превзойдён.
        Assert.False(receipt.ExecutionComplete); // Но исполнение не полное.
        Assert.False(receipt.AllMandatoryControlsAccepted); // Поэтому приёмка опущена.
        Assert.Equal("NOT_ADMITTED_CONTROLS_FAILED", receipt.TransferAdmission); // И допуск переноса закрыт.
    }

    /// <summary>
    /// Потеря умения с одного верного ответа до нуля — валидная измеренная регрессия:
    /// квалификация остаётся годной, а не превращается в «обучения не было».
    /// </summary>
    [Fact]
    public void LosingTheOnlyCorrectAnswerStaysAValidMeasuredRegression()
    {
        var observation = SavedRun(); // Исходные сохранённые числа.
        var before = new[]
        {
            Row("T1", LessonKind, "+", "+"), // До обучения верна только эта строка.
            Row("T2", LessonKind, "-", "+"), // Остальные три строки неверны.
            Row("E1", ExamKind, "+", "-"),
            Row("E2", ExamKind, "-", "+")
        };
        var after = new[]
        {
            Row("T1", LessonKind, "+", "-"), // После обучения верных ответов не осталось.
            Row("T2", LessonKind, "-", "+"),
            Row("E1", ExamKind, "+", "-"),
            Row("E2", ExamKind, "-", "+")
        };
        var regression = observation with
        {
            ArmA = observation.ArmA with { Before = before, After = before }, // Обе половины панели до обучения совпадают.
            ArmB = observation.ArmB with { Before = before, After = after }
        };

        var receipt = P1101DiagnosticReceipt.Build(regression); // Тот же действующий построитель.

        Assert.Equal(1, receipt.ArmB.CorrectBefore); // Был один верный ответ.
        Assert.Equal(0, receipt.ArmB.CorrectAfter); // Стало ноль верных ответов.
        Assert.Equal("VALID_FOR_NAMED_UNIT", receipt.ArmB.LearningQualification); // Это годная измеренная регрессия.
        Assert.Equal("испорчено", receipt.RowTransitions.Single(row => row.Arm == "B" && row.RowId == "T1").Transition);
        Assert.True(receipt.AllMandatoryControlsAccepted); // Контроли прошли: потеря умения их не опускает.
        Assert.False(receipt.BeatsConstantPlusBaseline); // Но тривиальный предел не превзойдён.
    }

    /// <summary>
    /// Нулевое число действующих изменений не называется обучением ни при каком
    /// числе вызовов оптимизатора.
    /// </summary>
    [Fact]
    public void FourOptimizerCallsWithoutAnyEffectiveChangeAreNotCalledLearning()
    {
        var receipt = P1101DiagnosticReceipt.Build(SavedRun()); // Квитанция сохранённого прогона.

        Assert.Equal(4, receipt.ArmA.OptimizerCalls); // Оптимизатор плеча A вызван четыре раза.
        Assert.Equal(0, receipt.ArmA.EffectiveUpdates); // Действующих изменений при этом ноль.
        Assert.Equal("INVALID_NO_EFFECTIVE_UPDATE", receipt.ArmA.LearningQualification); // Обучением это не называется.
        Assert.Equal(4, receipt.ArmB.OptimizerCalls); // В плече B столько же вызовов.
        Assert.Equal(4, receipt.ArmB.EffectiveUpdates); // И четыре действующих изменения.
        Assert.Equal("VALID_FOR_NAMED_UNIT", receipt.ArmB.LearningQualification); // Здесь урок дошёл до получателя.
    }

    /// <summary>Расходы разложены на основную и контрольную часть и сложены в общий итог.</summary>
    [Fact]
    public void BudgetBreakdownSplitsMainAndControlPhasesAndSumsThem()
    {
        var receipt = P1101DiagnosticReceipt.Build(SavedRun()); // Квитанция сохранённого прогона.

        Assert.Equal(40, receipt.Budget.MainModelForwards); // Сорок основных прямых проходов модели.
        Assert.Equal(19, receipt.Budget.ControlModelForwards); // Девятнадцать контрольных.
        Assert.Equal(59, receipt.Budget.TotalModelForwards); // Вместе пятьдесят девять.
        Assert.Equal(9, receipt.Budget.TotalModelBackwards); // Девять обратных проходов модели.
        Assert.Equal(3, receipt.Budget.LayerForwards); // Три прямых прохода одного слоя вложений.
        Assert.Equal(1, receipt.Budget.LayerBackwards); // Один обратный проход одного слоя вложений.
        Assert.True(receipt.Budget.MatchesExpectation); // Полный бюджет совпал с ожиданием.
    }

    /// <summary>Расходившийся состав строк панели — отказ построителя, а не тихая приёмка.</summary>
    [Fact]
    public void LostPanelRowIdAtEqualCountIsRefusedByTheBuilder()
    {
        var observation = SavedRun(); // Исходные сохранённые числа.
        var after = observation.ArmB.After.ToArray(); // Копия ответов после обучения.
        after[1] = after[1] with { RowId = "НЕОЖИДАННАЯ-СТРОКА" }; // Имя строки подменено при том же их числе.
        var broken = observation with { ArmB = observation.ArmB with { After = after } };

        var error = Assert.Throws<InvalidOperationException>(() => P1101DiagnosticReceipt.Build(broken));
        Assert.StartsWith("P1101_RECEIPT_ROW_SET_DIFFERS", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Одна строка панели в виде, понятном построителю квитанции.</summary>
    private static P1101PanelAnswer Row(string id, string kind, string gold, string produced)
        => new() { RowId = id, Kind = kind, Gold = gold, Produced = produced };

    /// <summary>
    /// Наблюдения сохранённой попытки attempt-0001. Каждое число названо своим
    /// источником; выдуманных значений здесь нет.
    /// </summary>
    private static P1101DiagnosticObservation SavedRun()
    {
        var sharedBefore = new[]
        {
            Row("T1", LessonKind, "+", "+"), // metrics.json panel.before: верно.
            Row("T2", LessonKind, "-", "+"), // metrics.json panel.before: неверно.
            Row("E1", ExamKind, "+", "-"), // metrics.json panel.before: неверно.
            Row("E2", ExamKind, "-", "-") // metrics.json panel.before: верно.
        }; // Панели обоих плеч до обучения совпали знак в знак.
        return new P1101DiagnosticObservation
        {
            ExperimentId = "P11-PRIMARY-01", // status.json: experiment.
            ExecutionStatus = "COMPLETED", // status.json: execution_status.
            ArmA = new P1101ArmObservation
            {
                Arm = "A", // Назначенная абляция.
                Before = sharedBefore, // Та же панель до обучения.
                After = sharedBefore, // metrics.json panel.after для «A:после»: ответы не изменились.
                OptimizerCalls = 4, // arms.json steps: четыре урока плеча A.
                EffectiveUpdates = 0, // arms.json steps: ни один не изменил таблицу.
                ChangedCoordinates = 0, // status.json evidence: arm_a_changed_coordinates.
                FrozenBaseIntact = true // arms.json guards: frozen_base_intact.
            },
            ArmB = new P1101ArmObservation
            {
                Arm = "B", // Нынешний продуктовый обратный проход.
                Before = sharedBefore, // Та же панель до обучения.
                After =
                [
                    Row("T1", LessonKind, "+", "+"), // metrics.json panel.after: удержано.
                    Row("T2", LessonKind, "-", "+"), // metrics.json panel.after: не выучено.
                    Row("E1", ExamKind, "+", "+"), // metrics.json panel.after: исправлено.
                    Row("E2", ExamKind, "-", "+") // metrics.json panel.after: испорчено.
                ],
                OptimizerCalls = 4, // arms.json steps: четыре урока плеча B.
                EffectiveUpdates = 4, // arms.json steps: все четыре изменили таблицу.
                ChangedCoordinates = 1088, // status.json evidence: arm_b_changed_coordinates.
                FrozenBaseIntact = true // arms.json guards: frozen_base_intact.
            },
            Controls = new P1101ControlObservation
            {
                ScatterOracleAgrees = true, // arms.json oracle.result.Agrees.
                AbsentRowsExactlyZero = true, // arms.json oracle.result.AbsentRowsAreExactlyZero.
                RepeatedIdRows = 2, // arms.json oracle.result.RepeatedIdRows.
                FiniteDifferenceAgreed = 9, // arms.json guards.finite_difference_agreed.
                FiniteDifferenceTotal = 9, // arms.json guards.finite_difference_total.
                ArmBChangedRowsConfined = true, // arms.json guards.arm_b_changed_rows_confined_to_reachable.
                ReferenceIntact = true, // arms.json guards.reference_intact.
                BeforePanelsAgreeAcrossArms = true // metrics.json panel.before: обе половины совпали.
            },
            Budget = new P1101BudgetObservation
            {
                TrainForwards = 8, // status.json counters.train_forwards.
                TrainBackwards = 8, // status.json counters.train_backwards.
                Generations = 16, // status.json counters.generations.
                ScoringForwards = 16, // status.json counters.scoring_forwards.
                ControlModelForwards = 19, // status.json counters.control_model_forwards.
                ControlModelBackwards = 1, // status.json counters.control_model_backwards.
                ControlLayerForwards = 3, // status.json counters.control_layer_forwards.
                ControlLayerBackwards = 1, // status.json counters.control_layer_backwards.
                DeadlineHeld = true // arms.json guards.deadline_held.
            },
            ExpectedBudget = new P1101BudgetExpectation
            {
                TotalModelForwards = 59, // Разбор 87: сорок основных плюс девятнадцать контрольных.
                TotalModelBackwards = 9, // Разбор 87: восемь учебных плюс один контрольный.
                TotalLayerForwards = 3, // Разбор 87: прямые проходы одного слоя вложений.
                TotalLayerBackwards = 1 // Разбор 87: обратный проход одного слоя вложений.
            }
        };
    }
}
